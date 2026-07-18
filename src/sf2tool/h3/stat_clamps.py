from __future__ import annotations

from pathlib import Path
from typing import Any

from sf2tool.h3.bizhawk import run_observer, verify_runtime_contract
from sf2tool.h3.growth import (
    _calculate_gain,
    _parse_ally_codes,
    _parse_class_bases,
    _parse_equates,
    _parse_growth_curves,
    _parse_item_equip_effects,
    _parse_stats_block,
    _rng_step,
    _verify_upstream,
)
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

FIXTURE = repo_path("tests/fixtures/h3/stat-clamp-boundaries-v1.json")
SCHEMA = repo_path("schemas/h3-stat-clamp-boundaries-fixture.schema.json")
OBSERVER = repo_path("tools/bizhawk/stat_clamp_boundaries_observer.lua")


def _clamp(operation: dict[str, Any]) -> int:
    before = operation["before"]
    amount = operation["amount"]
    cap = operation["cap"]
    if operation["kind"] == "increase-byte":
        if before + amount > 0xFF:
            return cap
        return min(cap, before + amount)
    if operation["kind"] == "increase-word":
        value = (before + amount) & 0xFFFF
        if value & 0x8000:
            return cap
        return min(cap, value)
    if operation["kind"] == "decrease-byte":
        return min(cap, max(0, before - amount))
    if operation["kind"] == "increase-7bits":
        flag = before & 0x80
        value = min(cap, (before & 0x7F) + amount)
        return flag | value
    raise ValueError(f"unknown stat clamp kind: {operation['kind']}")


def _level_up_gains(
    case: dict[str, Any], disasm: Path, curves: dict[int, dict[int, tuple[int, int]]]
) -> dict[str, int]:
    block = _parse_stats_block(disasm, case["ally"], case["classCode"])
    source_fields = {
        "hp": "maxHp",
        "mp": "maxMp",
        "attack": "baseAttack",
        "defense": "baseDefense",
        "agility": "baseAgility",
    }
    seed = case["seed"]
    gains: dict[str, int] = {}
    for stat, field in source_fields.items():
        growth = block["stats"][stat]
        current = case["input"][field]
        if current != growth["projected"]:
            raise ValueError(f"stat clamp {field} source basis drift")
        if growth["curve"] == 0:
            gains[stat] = 0
            continue
        seed, first = _rng_step(seed)
        seed, second = _rng_step(seed)
        gains[stat], _ = _calculate_gain(
            current=current,
            start=growth["start"],
            projected=growth["projected"],
            curve=growth["curve"],
            level=case["input"]["level"],
            first=first,
            second=second,
            curves=curves,
        )
    return gains


def _verify_source_contract(fixture: dict[str, Any], disasm: Path) -> None:
    case = fixture["case"]
    equates = _parse_equates(disasm)
    if _parse_ally_codes(disasm)[case["ally"]] != case["allyCode"]:
        raise ValueError("stat clamp ally source identity drift")
    if case["input"]["class"] != equates[f"CLASS_{case['classCode']}"]:
        raise ValueError("stat clamp class source identity drift")
    class_bases = _parse_class_bases(disasm, equates)[case["input"]["class"]]
    if case["input"]["baseMove"] != class_bases["move"]:
        raise ValueError("stat clamp base move source drift")

    helpers = (disasm / "code/common/stats/combatantstats_3.asm").read_text(
        encoding="utf-8"
    )
    required_fragments = (
        "IncreaseAndClampByte:",
        "bcs.s   @MakeMaxValue",
        "IncreaseAndClampWord:",
        "bmi.s   @MakeMaxValue   ; check if overflow to negative",
        "IncreaseAndClamp7Bits:",
        "andi.b  #TWO_TURN_THRESHOLD,d3",
        "andi.b  #TURN_AGILITY_MASK,d2",
        "DecreaseAndClampByte:",
        "bcs.s   @MakeMinValue",
    )
    if any(fragment not in helpers for fragment in required_fragments):
        raise ValueError("stat clamp helper source contract drift")

    gains = _level_up_gains(case, disasm, _parse_growth_curves(disasm))
    item_effects: dict[str, int] = {}
    item_mask = equates["ITEMENTRY_MASK_INDEX"]
    equipped_bit = equates["ITEM_EQUIPPED"]
    for item_entry in case["input"]["items"]:
        if item_entry & equipped_bit == 0:
            raise ValueError("stat clamp item is not equipped")
        effects, _ = _parse_item_equip_effects(disasm, item_entry & item_mask, equates)
        for effect, value in effects:
            if effect != "NONE" and effect not in item_effects:
                item_effects[effect] = value

    expected_amounts = {
        "level-up:hp": gains["hp"],
        "level-up:attack": gains["attack"],
        "level-up:defense": gains["defense"],
        "level-up:agility": gains["agility"],
        **{f"item:{effect}": value for effect, value in item_effects.items()},
    }
    expected_caps = {
        12: equates["CHAR_STATCAP_HP"],
        18: equates["CHAR_STATCAP_ATT"],
        19: equates["CHAR_STATCAP_ATT"],
        20: equates["CHAR_STATCAP_DEF"],
        21: equates["CHAR_STATCAP_DEF"],
        22: equates["CHAR_STATCAP_AGI_BASE"],
        23: equates["CHAR_STATCAP_AGI_DECREASING"],
        25: equates["CHAR_STATCAP_MOV"],
    }
    seen_sources: set[str] = set()
    for operation in case["operations"]:
        source = operation["source"]
        if source in seen_sources:
            raise ValueError("stat clamp source operation is duplicated")
        seen_sources.add(source)
        if expected_amounts.get(source) != operation["amount"]:
            raise ValueError(f"stat clamp amount source drift: {source}")
        if expected_caps.get(operation["fieldOffset"]) != operation["cap"]:
            raise ValueError(f"stat clamp cap source drift: {operation['id']}")
        if _clamp(operation) != operation["after"]:
            raise ValueError(f"stat clamp golden disagrees with source model: {operation['id']}")

    operations = {operation["source"]: operation for operation in case["operations"]}
    base_attack = operations["level-up:attack"]["after"]
    base_defense = operations["level-up:defense"]["after"]
    base_agility = operations["level-up:agility"]["after"]
    current = {
        "currentAttack": base_attack,
        "currentDefense": base_defense,
        "currentAgility": base_agility,
        "currentMove": case["input"]["baseMove"],
    }
    effect_fields = {
        "INCREASE_ATT": "currentAttack",
        "DECREASE_DEF": "currentDefense",
        "DECREASE_AGI": "currentAgility",
        "INCREASE_MOV": "currentMove",
        "DECREASE_MOV": "currentMove",
    }
    applied_sources: set[str] = set()
    cursed = False
    for item_entry in case["input"]["items"]:
        effects, item_cursed = _parse_item_equip_effects(
            disasm, item_entry & item_mask, equates
        )
        cursed |= item_cursed
        for effect, amount in effects:
            field = effect_fields.get(effect)
            if field is None:
                continue
            source = f"item:{effect}"
            operation = operations.get(source)
            if operation is not None and source not in applied_sources:
                current[field] = operation["after"]
                applied_sources.add(source)
                continue
            cap = {
                "currentAttack": equates["CHAR_STATCAP_ATT"],
                "currentDefense": equates["CHAR_STATCAP_DEF"],
                "currentAgility": equates["CHAR_STATCAP_AGI_DECREASING"],
                "currentMove": equates["CHAR_STATCAP_MOV"],
            }[field]
            delta = -amount if effect.startswith("DECREASE_") else amount
            current[field] = min(cap, max(0, current[field] + delta))

    expected_after = {
        "level": case["input"]["level"] + 1,
        "maxHp": operations["level-up:hp"]["after"],
        "baseAttack": base_attack,
        "currentAttack": current["currentAttack"],
        "baseDefense": base_defense,
        "currentDefense": current["currentDefense"],
        "baseAgility": base_agility,
        "currentAgility": current["currentAgility"],
        "baseMove": case["input"]["baseMove"],
        "currentMove": current["currentMove"],
        "status": equates["STATUSEFFECT_CURSE"] if cursed else 0,
    }
    if case["after"] != expected_after:
        raise ValueError("stat clamp final state disagrees with source model")


def verify_stat_clamp_boundaries(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 60
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner=str(FIXTURE))
    verify_runtime_contract(fixture, rom_path)
    _verify_source_contract(fixture, _verify_upstream(upstream_path))
    observed = run_observer(
        rom_path=rom_path,
        observer_path=OBSERVER,
        config={
            "function": fixture["function"],
            "ram": fixture["ram"],
            "case": fixture["case"],
        },
        output_name="stat-clamp-boundaries",
        timeout_seconds=timeout_seconds,
    )
    case = fixture["case"]
    expected = {
        "id": case["id"],
        "ally": case["ally"],
        "operations": [
            {
                "id": operation["id"],
                "before": operation["before"],
                "amount": operation["amount"],
                "after": operation["after"],
            }
            for operation in case["operations"]
        ],
        "helpersObserved": case["helpersObserved"],
        "after": case["after"],
    }
    if (
        observed.get("system") != "GEN"
        or observed.get("core") != fixture["emulator"]["core"]
        or observed.get("result") != expected
    ):
        raise ValueError("stat clamp boundary runtime mismatch")
    return {
        "Fixture": fixture["id"],
        "Operations": len(case["operations"]),
        "Caps": sum(
            (
                operation["after"] & 0x7F
                if operation["kind"] == "increase-7bits"
                else operation["after"]
            )
            == operation["cap"]
            for operation in case["operations"]
            if operation["kind"].startswith("increase")
        ),
        "Wraps": sum(
            operation["kind"] == "increase-word"
            and operation["after"] < operation["before"]
            for operation in case["operations"]
        ),
        "Underflows": sum(
            operation["id"].endswith("underflow") for operation in case["operations"]
        ),
        "Status": "PASS",
    }
