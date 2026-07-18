from __future__ import annotations

import re
from pathlib import Path
from typing import Any

from sf2tool.h3.bizhawk import run_observer, verify_runtime_contract
from sf2tool.h3.growth import (
    _parse_equates,
    _parse_item_equip_effects,
    _verify_upstream,
)
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

FIXTURE = repo_path("tests/fixtures/h3/enemy-curse-suppression-v1.json")
SCHEMA = repo_path("schemas/h3-enemy-curse-suppression-fixture.schema.json")
OBSERVER = repo_path("tools/bizhawk/enemy_curse_suppression_observer.lua")


def _verify_source_contract(fixture: dict[str, Any], disasm: Path) -> None:
    case = fixture["case"]
    equates = _parse_equates(disasm)
    if case["combatant"] != equates["COMBATANT_ENEMIES_START"]:
        raise ValueError("enemy curse combatant boundary drift")
    expected_slot = case["combatant"] - equates["COMBATANT_ENEMIES_START_MINUS_ALLIES_SPACE_END"]
    if case["ramSlot"] != expected_slot:
        raise ValueError("enemy curse RAM slot model drift")
    if case["item"]["index"] != equates["ITEM_BLACK_RING"]:
        raise ValueError("enemy curse item identity drift")

    effects, cursed = _parse_item_equip_effects(
        disasm, case["item"]["index"], equates
    )
    if effects[0] != ("INCREASE_ATT", case["item"]["attackIncrease"]):
        raise ValueError("Black Ring attack effect drift")
    if cursed != case["item"]["cursed"]:
        raise ValueError("Black Ring curse definition drift")

    source = (disasm / "code/common/stats/updatecombatantstats.asm").read_text(
        encoding="utf-8"
    )
    required_fragments = (
        "btst    #COMBATANT_BIT_ENEMY,d0",
        "btst    #ITEMTYPE_BIT_CURSED,ITEMDEF_OFFSET_TYPE(a0)",
        "@Enemy:",
        "clr.w   d2",
        "ori.w   #STATUSEFFECT_CURSE,d3",
    )
    if any(fragment not in source for fragment in required_fragments):
        raise ValueError("enemy curse suppression source contract drift")
    mask = re.search(r"andi\.w\s+#(?P<mask>STATUSEFFECT_[^\n]+),d3", source)
    if not mask or "STATUSEFFECT_CURSE" in mask.group("mask"):
        raise ValueError("UpdateCombatantStats status mask unexpectedly preserves CURSE")
    enemy_init = (
        disasm / "code/gameflow/battle/battleloop/initializecombatants.asm"
    ).read_text(encoding="utf-8")
    if (
        "InitializeEnemyStats:" not in enemy_init
        or "jsr     j_UpdateCombatantStats" not in enemy_init
    ):
        raise ValueError("natural enemy refresh caller drift")

    before = case["input"]
    expected_after = {
        "currentAttack": min(
            equates["CHAR_STATCAP_ATT"],
            before["baseAttack"] + case["item"]["attackIncrease"],
        ),
        "currentDefense": before["baseDefense"],
        "currentAgility": before["baseAgility"],
        "currentMove": before["baseMove"],
        "currentResistance": before["baseResistance"],
        "currentProwess": before["baseProwess"],
        "status": 0,
    }
    if case["after"] != expected_after:
        raise ValueError("enemy curse suppression golden disagrees with source model")


def verify_enemy_curse_suppression(
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
        output_name="enemy-curse-suppression",
        timeout_seconds=timeout_seconds,
    )
    if observed.get("system") != "GEN" or observed.get("core") != fixture["emulator"]["core"]:
        raise ValueError("unexpected enemy curse execution system/core")
    result = observed.get("result", {})
    case = fixture["case"]
    expected = {
        "id": case["id"],
        "combatant": case["combatant"],
        "battle": fixture["battleId"],
        "after": case["after"],
        "applyItemCalls": case["applyItemCalls"],
        "enemyBranchObserved": case["enemyBranchObserved"],
    }
    if result != expected:
        raise ValueError("enemy curse suppression runtime mismatch")
    return {
        "Fixture": fixture["id"],
        "Combatant": f"0x{case['combatant']:02X}",
        "Attack": f"{case['input']['baseAttack']}->{case['after']['currentAttack']}",
        "Curse": f"{case['input']['status']}->{case['after']['status']}",
        "Status": "PASS",
    }
