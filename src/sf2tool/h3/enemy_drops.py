from __future__ import annotations

from pathlib import Path
from typing import Any

from sf2tool.h3.bizhawk import run_observer, verify_runtime_contract
from sf2tool.h3.growth import _parse_equates, _rng_step, _verify_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

FIXTURE = repo_path("tests/fixtures/h3/enemy-item-drop-behavior-v1.json")
SCHEMA = repo_path("schemas/h3-enemy-item-drop-behavior-fixture.schema.json")
OBSERVER = repo_path("tools/bizhawk/enemy_item_drop_behavior_observer.lua")


def _verify_source_contract(fixture: dict[str, Any], disasm: Path) -> None:
    source = (
        disasm / "code/gameflow/battle/battleactions/dropenemyitem.asm"
    ).read_text(encoding="utf-8")
    required = (
        "cmpi.w  #ITEM_TAROS_SWORD,d3",
        "moveq   #ENEMYITEMDROP_RANDOM_CHANCE,d0",
        "bset    d0,(a0)",
        "bne.w   @Done           ; done if item dropped flag was already set",
        "jsr     RemoveItemBySlot",
        "jsr     AddItem",
    )
    if any(fragment not in source for fragment in required):
        raise ValueError("enemy item drop behavior source contract drift")
    equates = _parse_equates(disasm)
    rare_items = {
        equates["ITEM_TAROS_SWORD"],
        equates["ITEM_IRON_BALL"],
        equates["ITEM_COUNTER_SWORD"],
    }
    for case in fixture["cases"]:
        rare = case["item"] in rare_items
        roll = _rng_step(case["seed"], 32)[1] if rare else None
        drops = (not rare or roll == 0) and not case["initialFlag"]
        expected = {
            "roll": roll,
            "finalFlag": case["initialFlag"] or drops,
            "finalTargetItem": fixture["emptyItem"] if drops else case["item"],
            "finalActorItems": (
                [case["item"], fixture["emptyItem"], fixture["emptyItem"], fixture["emptyItem"]]
                if drops
                else [fixture["emptyItem"]] * 4
            ),
        }
        if any(case[field] != value for field, value in expected.items()):
            raise ValueError(f"enemy item drop golden disagrees with source model: {case['id']}")


def _verify_observation(fixture: dict[str, Any], observed: dict[str, Any]) -> None:
    expected = {
        "cases": [
            {
                "id": case["id"],
                "roll": case["roll"],
                "finalFlag": case["finalFlag"],
                "finalTargetItem": case["finalTargetItem"],
                "finalActorItems": case["finalActorItems"],
            }
            for case in fixture["cases"]
        ]
    }
    if (
        observed.get("system") != "GEN"
        or observed.get("core") != fixture["emulator"]["core"]
        or observed.get("result") != expected
    ):
        raise ValueError("enemy item drop behavior runtime matrix mismatch")


def verify_enemy_item_drop_behavior(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 75
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner=str(FIXTURE))
    verify_runtime_contract(fixture, rom_path)
    _verify_source_contract(fixture, _verify_upstream(upstream_path))
    observed = run_observer(
        rom_path=rom_path,
        observer_path=OBSERVER,
        config={
            "function": {**fixture["harness"]["function"], **fixture["function"]},
            "ram": {**fixture["harness"]["ram"], **fixture["ram"]},
            "actor": fixture["actor"],
            "emptyItem": fixture["emptyItem"],
            "cases": fixture["cases"],
        },
        output_name="enemy-item-drop-behavior",
        timeout_seconds=timeout_seconds,
    )
    _verify_observation(fixture, observed)
    return {
        "Fixture": fixture["id"],
        "Cases": len(fixture["cases"]),
        "Rolls": [case["roll"] for case in fixture["cases"]],
        "Status": "PASS",
    }
