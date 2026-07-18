from __future__ import annotations

from pathlib import Path
from typing import Any

from sf2tool.h3.bizhawk import run_observer, verify_runtime_contract
from sf2tool.h3.growth import _verify_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

FIXTURE = repo_path("tests/fixtures/h3/spell-detox-v1.json")
SCHEMA = repo_path("schemas/h3-spell-detox-fixture.schema.json")
OBSERVER = repo_path("tools/bizhawk/spell_detox_observer.lua")


def _verify_source_contract(disasm: Path) -> None:
    source = (
        disasm / "code/gameflow/battle/battleactions/castspell.asm"
    ).read_text(encoding="utf-8")
    required = (
        "spellEffect_Detox:",
        "cmpi.w  #0,((BATTLESCENE_SPELL_LEVEL-$1000000)).w",
        "cmpi.w  #1,((BATTLESCENE_SPELL_LEVEL-$1000000)).w",
        "bclr    #STATUSEFFECT_BIT_CURSE,d1",
        "bclr    #STATUSEFFECT_BIT_STUN,d1",
        "bclr    #STATUSEFFECT_BIT_POISON,d1",
        "bsr.w   battlesceneScript_AddStatusEffectSpellExp",
        "jsr     UnequipAllItemsIfNotCursed",
        "moveq   #8,d2",
        "jsr     SetStatusEffects",
        "jsr     UpdateCombatantStats",
    )
    if any(fragment not in source for fragment in required):
        raise ValueError("DETOX source contract drift")


def _model_case(fixture: dict[str, Any], case: dict[str, Any]) -> dict[str, Any]:
    status = case["initialStatus"]
    cured = 0
    if case["internalLevel"] >= 2 and status & 4:
        status &= ~4
        cured |= 4
    if case["internalLevel"] >= 1 and status & 1:
        status &= ~1
        cured |= 2
    if status & 2:
        status &= ~2
        cured |= 1
    effective = cured != 0
    unequipped = bool(cured & 4)
    return {
        "curedFlags": cured,
        "resultStatus": status,
        "effective": effective,
        "reaction": effective,
        "exp": 5 if effective else 0,
        "curseUnequipped": unequipped,
        "finalItem": (
            fixture["setup"]["unequippedCursedItem"]
            if unequipped
            else fixture["setup"]["equippedCursedItem"]
        ),
    }


def _verify_models(fixture: dict[str, Any]) -> None:
    for case in fixture["cases"]:
        modeled = _model_case(fixture, case)
        if any(case[field] != value for field, value in modeled.items()):
            raise ValueError(f"DETOX golden disagrees with source model: {case['id']}")


def _observed_case(case: dict[str, Any]) -> dict[str, Any]:
    return {
        "id": case["id"],
        "actionSpell": case["actionSpell"],
        "initialStatus": case["initialStatus"],
        "curedFlags": case["curedFlags"],
        "resultStatus": case["resultStatus"],
        "reaction": case["reaction"],
        "exp": case["exp"],
        "curseUnequipped": case["curseUnequipped"],
        "ineffective": not case["effective"],
        "finalStatus": case["resultStatus"],
        "finalItem": case["finalItem"],
    }


def verify_spell_detox(
    rom_path: Path,
    upstream_path: Path,
    *,
    timeout_seconds: int = 90,
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner="DETOX status matrix fixture")
    verify_runtime_contract(fixture, rom_path)
    healing_shared = load_json(repo_path(fixture["sharedHarnessFixture"]))
    shared = load_json(repo_path(healing_shared["sharedHarnessFixture"]))
    disasm = _verify_upstream(upstream_path)
    _verify_source_contract(disasm)
    _verify_models(fixture)
    observed = run_observer(
        rom_path=rom_path,
        observer_path=OBSERVER,
        config={
            "function": {
                **shared["function"],
                **healing_shared["function"],
                **fixture["function"],
            },
            "ram": {**shared["ram"], **fixture["ram"]},
            "harness": shared["harness"],
            "battleId": fixture["battleId"],
            "setup": fixture["setup"],
            "cases": fixture["cases"],
        },
        output_name="spell-detox",
        timeout_seconds=timeout_seconds,
    )
    expected = {
        "battle": fixture["battleId"],
        "cases": [_observed_case(case) for case in fixture["cases"]],
    }
    if (
        observed.get("system") != "GEN"
        or observed.get("core") != fixture["emulator"]["core"]
        or observed.get("result") != expected
    ):
        raise ValueError(
            "DETOX status matrix runtime mismatch\n"
            f"expected={expected!r}\nobserved={observed!r}"
        )
    return {
        "Fixture": fixture["id"],
        "Cases": len(fixture["cases"]),
        "CuredFlags": [case["curedFlags"] for case in fixture["cases"]],
        "FinalStatus": [f"0x{case['resultStatus']:04X}" for case in fixture["cases"]],
        "Exp": [case["exp"] for case in fixture["cases"]],
        "CurseUnequipped": sum(case["curseUnequipped"] for case in fixture["cases"]),
        "Status": "PASS",
    }
