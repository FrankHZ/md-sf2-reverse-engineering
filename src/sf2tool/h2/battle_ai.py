from __future__ import annotations

import hashlib
import json
import re
import subprocess
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.source_text import decode_upstream_text

ID = "sf2-battle-ai-static-v1"
SOURCE_ROOT = Path("code/gameflow/battle/ai")
MANIFEST = repo_path("manifests/extractions/battle-ai-static.json")
SCHEMA = repo_path("schemas/battle-ai-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/battle-ai-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-battle-ai-static-fixture.schema.json")
PRIORITY_FIXTURE = repo_path("tests/fixtures/h2/battle-ai-priority-static-v1.json")
PRIORITY_FIXTURE_SCHEMA = repo_path("schemas/h2-battle-ai-priority-static-fixture.schema.json")
HEALING_FIXTURE = repo_path("tests/fixtures/h2/battle-ai-healing-static-v1.json")
HEALING_FIXTURE_SCHEMA = repo_path("schemas/h2-battle-ai-healing-static-fixture.schema.json")
SUPPORT_FIXTURE = repo_path("tests/fixtures/h2/battle-ai-support-static-v1.json")
SUPPORT_FIXTURE_SCHEMA = repo_path("schemas/h2-battle-ai-support-static-fixture.schema.json")
ACTION_CHOICE_FIXTURE = repo_path("tests/fixtures/h2/battle-ai-action-choice-static-v1.json")
ACTION_CHOICE_FIXTURE_SCHEMA = repo_path(
    "schemas/h2-battle-ai-action-choice-static-fixture.schema.json"
)
MOVEMENT_FIXTURE = repo_path("tests/fixtures/h2/battle-ai-movement-static-v1.json")
MOVEMENT_FIXTURE_SCHEMA = repo_path("schemas/h2-battle-ai-movement-static-fixture.schema.json")
REMAINING_FIXTURE = repo_path("tests/fixtures/h2/battle-ai-remaining-static-v1.json")
REMAINING_FIXTURE_SCHEMA = repo_path("schemas/h2-battle-ai-remaining-static-fixture.schema.json")
TOOLCHAIN = repo_path("manifests/toolchain.json")
RESEARCH_INDEX = repo_path("manifests/research-index.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")

GLOBAL_LABEL_PATTERN = re.compile(r"^([A-Za-z_][A-Za-z0-9_]*):")
LOCAL_LABEL_PATTERN = re.compile(r"^\s*(@[A-Za-z0-9_]+):")
CALL_PATTERN = re.compile(r"\b(?:bsr|jsr)(?:\.[bwl])?\s+([^\s,;]+)", re.IGNORECASE)
DIRECT_TARGET_PATTERN = re.compile(r"^[A-Za-z_][A-Za-z0-9_]*$")
EQUATE_PATTERN = re.compile(r"^([A-Z][A-Z0-9_]+):\s+equ\s+(\$[0-9A-Fa-f]+|-?\d+)", re.MULTILINE)
SPELL_COMPARE_PATTERN = re.compile(r"cmpi\.b\s+#(SPELL_[A-Z0-9_]+),d5")
DC_LONG_TARGET_PATTERN = re.compile(r"^\s*dc\.l\s+([A-Za-z_][A-Za-z0-9_]*)", re.MULTILINE)
DC_BYTE_INTEGER_PATTERN = re.compile(r"^\s*dc\.b\s+(-?\d+)\b", re.MULTILINE)
DC_BYTE_VALUE_PATTERN = re.compile(r"^\s*dc\.b\s+(-?\d+|[A-Z][A-Z0-9_]+)\b", re.MULTILINE)
REGISTER_TARGETS = (
    {f"a{index}" for index in range(8)} | {f"d{index}" for index in range(8)} | {"sp", "pc"}
)


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _strip_comment(line: str) -> str:
    return line.split(";", 1)[0].rstrip()


def _direct_target(operand: str) -> str | None:
    operand = re.sub(r"\.[bwl]$", "", operand, flags=re.IGNORECASE)
    if operand.startswith("(") and operand.endswith(")"):
        operand = operand[1:-1]
    if not DIRECT_TARGET_PATTERN.fullmatch(operand) or operand.lower() in REGISTER_TARGETS:
        return None
    return operand


def _parse_source_file(path: Path, relative_path: str) -> dict[str, Any]:
    raw = path.read_bytes()
    # Hash the original bytes while accepting the pinned sound driver's one
    # legacy single-byte comment data.
    text = decode_upstream_text(raw)
    lines = text.splitlines()
    global_labels: list[str] = []
    local_label_count = 0
    statement_count = 0
    direct_calls: Counter[str] = Counter()
    indirect_call_count = 0

    for raw_line in lines:
        line = _strip_comment(raw_line)
        if not line.strip():
            continue
        global_match = GLOBAL_LABEL_PATTERN.match(line)
        local_match = LOCAL_LABEL_PATTERN.match(line)
        if global_match:
            global_labels.append(global_match.group(1))
            line = line[global_match.end() :]
        elif local_match:
            local_label_count += 1
            line = line[local_match.end() :]
        if not line.strip():
            continue
        statement_count += 1
        call_match = CALL_PATTERN.search(line)
        if not call_match:
            continue
        target = _direct_target(call_match.group(1))
        if target is None:
            indirect_call_count += 1
        else:
            direct_calls[target] += 1

    return {
        "path": relative_path,
        "sha256": hashlib.sha256(raw).hexdigest().upper(),
        "sourceLineCount": len(lines),
        "statementCount": statement_count,
        "globalLabels": global_labels,
        "localLabelCount": local_label_count,
        "directCalls": [
            {"target": target, "siteCount": count} for target, count in sorted(direct_calls.items())
        ],
        "indirectCallSiteCount": indirect_call_count,
    }


def _integer(expression: str) -> int:
    if expression.startswith("$"):
        return int(expression[1:], 16)
    return int(expression, 10)


def _equates(disasm: Path) -> dict[str, int]:
    text = (disasm / "sf2enums.asm").read_text(encoding="utf-8")
    return {name: _integer(expression) for name, expression in EQUATE_PATTERN.findall(text)}


def _function_block(source: str, name: str) -> str:
    match = re.search(
        rf"^{re.escape(name)}:\s*$" rf"(.*?)" rf"^\s*; End of function {re.escape(name)}\s*$",
        source,
        re.MULTILINE | re.DOTALL,
    )
    if not match:
        raise ValueError(f"battle AI action-filter function is missing: {name}")
    return match.group(1)


def _label_block(source: str, name: str) -> str:
    match = re.search(
        rf"^{re.escape(name)}:\s*$" rf"(.*?)(?=^[A-Za-z_][A-Za-z0-9_]*:\s*$|\Z)",
        source,
        re.MULTILINE | re.DOTALL,
    )
    if not match:
        raise ValueError(f"battle AI static data label is missing: {name}")
    return match.group(1)


def _named_values(names: list[str], equates: dict[str, int]) -> list[dict[str, int | str]]:
    missing = [name for name in names if name not in equates]
    if missing:
        raise ValueError(f"battle AI action-filter equates are missing: {missing}")
    return [{"name": name, "value": equates[name]} for name in names]


def _parse_action_filters(disasm: Path) -> dict[str, Any]:
    source = (disasm / SOURCE_ROOT / "getnextuseableaiaction.asm").read_text(encoding="utf-8")
    equates = _equates(disasm)
    attack_spell = _function_block(source, "GetNextUsableAttackSpell")
    healing_spell = _function_block(source, "GetNextHealingSpell")
    support_spell = _function_block(source, "GetNextSupportSpell")
    attack_item = _function_block(source, "GetNextUsableAttackItem")
    healing_item = _function_block(source, "GetNextUsableHealingItem")

    attack_spell_allowlist = SPELL_COMPARE_PATTERN.findall(attack_spell)
    attack_item_allowlist = SPELL_COMPARE_PATTERN.findall(attack_item)
    required_fragments = {
        "attack spell": (
            (attack_spell, "bsr.w   IsCombatantConfused"),
            (attack_spell, "move.w  #1,d7"),
            (attack_spell, "bsr.w   GetHighestUsableSpellLevel"),
            (attack_spell, "bra.w   @Next"),
        ),
        "healing spell": (
            (healing_spell, "cmpi.b  #SPELLPROPS_TYPE_HEAL,d2"),
            (healing_spell, "move.w  #SPELL_NOTHING,d1"),
        ),
        "support spell": (
            (support_spell, "cmpi.b  #SPELLPROPS_TYPE_SUPPORT,d2"),
            (support_spell, "move.w  #SPELL_NOTHING,d1"),
        ),
        "attack item": (
            (attack_item, "move.w  #1,d6"),
            (attack_item, "btst    #ITEMENTRY_BIT_EQUIPPED,d1"),
            (attack_item, "btst    #ITEMENTRY_BIT_USABLE_BY_AI,d1"),
            (attack_item, "bra.w   @Nothing"),
        ),
        "healing item": (
            (healing_item, "cmpi.b  #ITEM_HEALING_RAIN,d7"),
            (healing_item, "btst    #ITEMENTRY_BIT_USABLE_BY_AI,d1"),
            (healing_item, "beq.w   @Next"),
        ),
    }
    for owner, checks in required_fragments.items():
        if any(fragment not in block for block, fragment in checks):
            raise ValueError(f"battle AI {owner} source contract drift")

    if len(attack_spell_allowlist) != 6 or len(attack_item_allowlist) != 4:
        raise ValueError("battle AI confused-action allowlist shape drift")

    return {
        "slotCounts": {
            "spells": equates["COMBATANT_SPELLSLOTS"],
            "items": equates["COMBATANT_ITEMSLOTS"],
        },
        "sentinels": {
            "spellNothing": equates["SPELL_NOTHING"],
            "itemNothing": equates["ITEM_NOTHING"],
        },
        "spellTypes": {
            "attack": equates["SPELLPROPS_TYPE_ATTACK"],
            "heal": equates["SPELLPROPS_TYPE_HEAL"],
            "support": equates["SPELLPROPS_TYPE_SUPPORT"],
        },
        "attackSpell": {
            "function": "GetNextUsableAttackSpell",
            "alliesAlwaysUseConfusedFilter": True,
            "confusedEnemyUsesFilter": True,
            "confusedAllowlist": _named_values(attack_spell_allowlist, equates),
            "requiredSpellType": "attack",
            "rejectedCandidatePolicy": "continue-to-next-slot",
            "returnsHighestUsableLevel": True,
        },
        "healingSpell": {
            "function": "GetNextHealingSpell",
            "requiredSpellType": "heal",
            "rejectedCandidatePolicy": "continue-to-next-slot",
            "confusionFilter": False,
        },
        "supportSpell": {
            "function": "GetNextSupportSpell",
            "requiredSpellType": "support",
            "rejectedCandidatePolicy": "continue-to-next-slot",
            "confusionFilter": False,
        },
        "attackItem": {
            "function": "GetNextUsableAttackItem",
            "alliesAlwaysUseConfusedFilter": True,
            "confusedEnemyUsesFilter": True,
            "confusedAllowlist": _named_values(attack_item_allowlist, equates),
            "requiredSpellType": "attack",
            "equippedItemBypassesAiUseFlag": True,
            "unequippedItemRequiresAiUseFlag": True,
            "unusableCandidatePolicy": "continue-to-next-slot",
            "rejectedCandidatePolicy": "stop-with-item-nothing",
        },
        "healingItem": {
            "function": "GetNextUsableHealingItem",
            "requiredSpellType": "heal",
            "healingRainItem": {
                "name": "ITEM_HEALING_RAIN",
                "value": equates["ITEM_HEALING_RAIN"],
            },
            "healingRainBypassesAiUseFlag": True,
            "otherItemsRequireAiUseFlag": True,
            "rejectedCandidatePolicy": "continue-to-next-slot",
            "confusionFilter": False,
        },
    }


def _parse_attack_priority(disasm: Path) -> dict[str, Any]:
    priority_path = SOURCE_ROOT / "command/attack/prioritizetargets.asm"
    adjust_path = SOURCE_ROOT / "command/attack/adjusttargetpriority.asm"
    helper_path = SOURCE_ROOT / "command/attack/targetprioritizationhelperfunctions.asm"
    data_path = Path("data/battles/global/aipriority.asm")
    priority_source = (disasm / priority_path).read_text(encoding="utf-8")
    adjust_source = (disasm / adjust_path).read_text(encoding="utf-8")
    helper_source = (disasm / helper_path).read_text(encoding="utf-8")
    data_source = (disasm / data_path).read_text(encoding="utf-8")
    equates = _equates(disasm)

    selector_targets = DC_LONG_TARGET_PATTERN.findall(
        _label_block(priority_source, "pt_TargetPriorityScripts")
    )
    selector = []
    for target in selector_targets:
        match = re.fullmatch(r"TargetPriorityScript([1-4])", target)
        if not match:
            raise ValueError(f"unexpected target-priority script pointer: {target}")
        selector.append(int(match.group(1)))
    if len(selector) != 16:
        raise ValueError(f"expected 16 target-priority script pointers, found {len(selector)}")

    adjustment_pointer_targets = DC_LONG_TARGET_PATTERN.findall(
        _label_block(data_source, "pt_AttackPriorityAdjustmentsForMovetype")
    )
    if len(adjustment_pointer_targets) != 16:
        raise ValueError(
            "expected 16 attack-priority adjustment pointers, "
            f"found {len(adjustment_pointer_targets)}"
        )
    adjustment_table_names = (
        "table_PriorityAdjustments_Regular",
        "table_PriorityAdjustments_Mage",
        "table_PriorityAdjustments_Archer",
        "table_PriorityAdjustments_Flying",
    )
    adjustment_tables = {
        name: [
            int(value) for value in DC_BYTE_INTEGER_PATTERN.findall(_label_block(data_source, name))
        ]
        for name in adjustment_table_names
    }
    if any(len(values) != 32 for values in adjustment_tables.values()):
        raise ValueError("battle AI class-priority adjustment table shape drift")

    potential_damage = _function_block(priority_source, "CalculatePotentialDamage")
    spell_resistance = _function_block(priority_source, "AdjustSpellPowerForResistance")
    remaining_hp = _function_block(priority_source, "CalculateRemainingHpAfterPotentialDamage")
    script_blocks = {
        index: _function_block(priority_source, f"TargetPriorityScript{index}")
        for index in range(1, 5)
    }
    adjust_priority = _function_block(adjust_source, "AdjustTargetPriority")
    third_threshold = _function_block(helper_source, "IsRemainingHpAboveOneThirdOfCurrent")
    fifth_threshold = _function_block(helper_source, "IsRemainingHpAboveOneFifthOfMax")
    required_fragments = (
        (potential_damage, "moveq   #1,d2"),
        (potential_damage, "move.w  #256,d2"),
        (potential_damage, "move.w  #230,d2"),
        (potential_damage, "move.w  #205,d2"),
        (potential_damage, "lsr.w   #BYTE_SHIFT_COUNT,d6"),
        (spell_resistance, "sub.w   d3,d6"),
        (spell_resistance, "lsr.w   #1,d6"),
        (spell_resistance, "add.w   d3,d6"),
        (remaining_hp, "moveq   #0,d1"),
        (script_blocks[1], "addi.w  #15,d6"),
        (script_blocks[1], "addi.w  #2,d6"),
        (script_blocks[2], "IsRemainingHpAboveOneThirdOfCurrent"),
        (script_blocks[2], "IsRemainingHpAboveOneFifthOfMax"),
        (script_blocks[3], "j_GenerateRandomNumberUnderD6"),
        (script_blocks[3], "moveq   #18,d6"),
        (script_blocks[4], "IsRemainingHpAboveOneFifthOfMax"),
        (adjust_priority, "btst    #COMBATANT_BIT_ENEMY,d0"),
        (adjust_priority, "bsr.w   IsCombatantConfused"),
        (adjust_priority, "cmpi.b  #ALLY_SARAH,d7"),
        (third_threshold, "mulu.w  #3,d2"),
        (fifth_threshold, "bra.w   @Continue"),
        (helper_source, "mulu.w  #5,d2"),
    )
    if any(fragment not in block for block, fragment in required_fragments):
        raise ValueError("battle AI attack-priority source contract drift")

    multipliers = [
        int(value) for value in re.findall(r"move\.w\s+#(256|230|205),d2", potential_damage)
    ]
    if multipliers != [256, 230, 205]:
        raise ValueError(f"battle AI land-effect multiplier drift: {multipliers}")

    return {
        "sourcePaths": [
            priority_path.as_posix(),
            adjust_path.as_posix(),
            helper_path.as_posix(),
            data_path.as_posix(),
        ],
        "scriptSelector": {
            "difficultyRows": 4,
            "activationColumns": 4,
            "scriptsByDifficultyAndActivation": [
                selector[index : index + 4] for index in range(0, 16, 4)
            ],
            "allyActivationColumn": equates["AIBITFIELD_SECONDARY_ACTIVE"],
            "enemyRegularActivationMask": equates["BYTE_LOWER_NIBBLE_MASK"],
            "enemySpellActivationMask": equates["AIBITFIELD_TRIGGER_REGIONS_MASK"],
        },
        "regularPotentialDamage": {
            "minimumBeforeLandEffect": 1,
            "landEffectMultipliers256": multipliers,
            "rounding": "floor-after-multiply",
        },
        "spellPotentialDamage": {
            "base": "spell-power",
            "minorResistance": "power-minus-floor-quarter",
            "majorResistance": "floor-half",
            "weakness": "power-plus-floor-quarter",
            "resistanceValues": {
                "minor": equates["RESISTANCESETTING_MINOR"],
                "major": equates["RESISTANCESETTING_MAJOR"],
                "weakness": equates["RESISTANCESETTING_WEAKNESS"],
            },
            "areaScore": "sum-target-priorities",
        },
        "remainingHpMinimum": 0,
        "thresholds": {
            "script2Damage": "remaining-less-than-or-equal-current-third",
            "script2And4LowHp": "remaining-less-than-or-equal-max-fifth",
        },
        "priorityScripts": [
            {
                "id": 1,
                "base": 1,
                "lethalBonus": 15,
                "lastTargetBonus": 2,
                "usesClassAdjustment": True,
            },
            {
                "id": 2,
                "base": 1,
                "lethalBonus": 15,
                "damageThresholdBonus": 1,
                "lowHpBonus": 1,
                "lastTargetBonus": 2,
                "usesClassAdjustment": True,
            },
            {
                "id": 3,
                "rngRange": 3,
                "lethalityBranchOutcomes": 1,
                "movementBranchOutcomes": 2,
                "lethalityBase": 1,
                "lethalBonus": 15,
                "movementPriorityFormula": "max(19-2*movement,1)",
                "usesClassAdjustment": False,
            },
            {
                "id": 4,
                "base": 1,
                "lethalBonus": 15,
                "lowHpBonus": 1,
                "usesClassAdjustment": False,
            },
        ],
        "classAdjustment": {
            "alliesOnly": True,
            "skippedWhenConfused": True,
            "previousTargetSarahUsesMageTable": True,
            "sarahIndex": equates["ALLY_SARAH"],
            "movetypePointerTargets": adjustment_pointer_targets,
            "tables": adjustment_tables,
            "minimum": min(min(values) for values in adjustment_tables.values()),
            "maximum": max(max(values) for values in adjustment_tables.values()),
        },
    }


def _parse_healing(disasm: Path) -> dict[str, Any]:
    command_path = SOURCE_ROOT / "command/heal.asm"
    requires_path = SOURCE_ROOT / "command/heal/doescombatantrequirehealing.asm"
    level_path = SOURCE_ROOT / "command/heal/determinehealingspelllevel.asm"
    priority_path = SOURCE_ROOT / "command/heal/calculatehealtargetpriority.asm"
    half_hp_path = SOURCE_ROOT / "command/heal/iscombatantatlessthanhalfhp.asm"
    data_path = Path("data/battles/global/aipriority.asm")
    command_source = (disasm / command_path).read_text(encoding="utf-8")
    requires_source = (disasm / requires_path).read_text(encoding="utf-8")
    level_source = (disasm / level_path).read_text(encoding="utf-8")
    priority_source = (disasm / priority_path).read_text(encoding="utf-8")
    half_hp_source = (disasm / half_hp_path).read_text(encoding="utf-8")
    data_source = (disasm / data_path).read_text(encoding="utf-8")
    command = _function_block(command_source, "aiCommand_Heal")
    requires = _function_block(requires_source, "DoesCombatantRequireHealing")
    level = _function_block(level_source, "DetermineHealingSpellLevel")
    priority = _function_block(priority_source, "CalculateHealTargetPriority")
    half_hp = _function_block(half_hp_source, "IsCombatantAtLessThanHalfHp")
    equates = _equates(disasm)

    required_fragments = (
        (command, "bsr.w   IsCombatantConfused"),
        (command, "move.w  #COMBATANT_ENEMIES_START,d0"),
        (command, "bsr.w   IsCombatantAtLessThanHalfHp"),
        (command, "cmpi.w  #SPELL_HEAL,d2"),
        (command, "cmpi.w  #ENEMYAI_MIN_MP_HEAL1,d1"),
        (command, "cmpi.w  #SPELL_AURA,d2"),
        (command, "cmpi.w  #ENEMYAI_MIN_MP_AURA1,d1"),
        (command, "bsr.w   DoesCombatantRequireHealing"),
        (command, "bsr.w   CalculateHealTargetPriority"),
        (command, "addi.w  #4,d2"),
        (command, "move.b  d2,(a2,d4.w)"),
        (command, "cmpi.b  #ITEM_NOTHING,itemEntry(a6) ; loop to cycle"),
        (command, "bsr.w   DetermineHealingSpellLevel"),
        (command, "cmpi.b  #ENEMYAI_MIN_MP_HEAL3,d1"),
        (command, "move.b  #SPELLENTRY_LV2,d2"),
        (requires, "mulu.w  #3,d2"),
        (requires, "add.w   d1,d1"),
        (requires, "cmp.w   d2,d1"),
        (half_hp, "add.w   d2,d2"),
        (half_hp, "cmp.w   d2,d1"),
        (level, "cmpi.w  #ENEMYAI_THRESHOLD_HEAL1,d1"),
        (level, "cmpi.w  #ENEMYAI_THRESHOLD_HEAL2,d1"),
        (level, "cmpi.w  #ENEMYAI_THRESHOLD_HEAL3,d1"),
        (level, "lsl.w   #5,d1"),
        (level, "add.w   d4,d1"),
        (level, "dbf     d2,loc_CDC2"),
        (level, "cmpi.b  #1,d2"),
        (priority, "cmpi.w  #AICOMMANDSET_CRITICAL,d1"),
        (priority, "cmpi.w  #AICOMMANDSET_LEADER,d1"),
        (priority, "move.w  #MOVETYPES_NUMBER,d6"),
    )
    if any(fragment not in block for block, fragment in required_fragments):
        raise ValueError("battle AI healing source contract drift")

    movetype_names = DC_BYTE_VALUE_PATTERN.findall(
        _label_block(data_source, "table_HealPriorityMovetypes")
    )
    if len(movetype_names) != equates["MOVETYPES_NUMBER"]:
        raise ValueError(f"battle AI heal-priority table drift: {movetype_names}")
    movetype_values = [int(name) if name == "-1" else equates[name] for name in movetype_names]
    movetype_priorities = [
        {
            "name": name,
            "value": value,
            "priority": equates["MOVETYPES_NUMBER"] - index,
        }
        for index, (name, value) in enumerate(zip(movetype_names, movetype_values, strict=True))
        if index
    ]

    return {
        "sourcePaths": [
            command_path.as_posix(),
            requires_path.as_posix(),
            level_path.as_posix(),
            priority_path.as_posix(),
            half_hp_path.as_posix(),
            data_path.as_posix(),
        ],
        "command": {
            "function": "aiCommand_Heal",
            "confusedCasterExits": True,
            "targetSide": "caster-side",
            "healingRainCheckedFirst": True,
            "healingRainCondition": "first-enemy-current-hp-less-than-or-equal-half-max",
            "healingRainTarget": "caster",
            "acceptedSpellBases": [equates["SPELL_HEAL"], equates["SPELL_AURA"]],
            "minimumMpBeforeTargeting": {
                "heal": equates["ENEMYAI_MIN_MP_HEAL1"],
                "aura": equates["ENEMYAI_MIN_MP_AURA1"],
            },
            "itemFallbackAfterSpellFailure": True,
            "itemTakesPrecedenceAtActionLoad": True,
            "targetOrder": "descending-byte-priority-then-first-reachable",
        },
        "eligibility": {
            "requiresHealing": "current-hp-times-three-less-than-or-equal-max-hp-times-two",
            "requiresHealingIncludesTwoThirds": True,
            "halfHp": "current-hp-times-two-less-than-or-equal-max-hp",
            "halfHpIncludesEquality": True,
            "helperUnreachableAlternateEntries": 2,
        },
        "spellLevel": {
            "missingHpThresholds": {
                "noCastMaximum": equates["ENEMYAI_THRESHOLD_HEAL1"],
                "level1Maximum": equates["ENEMYAI_THRESHOLD_HEAL2"],
                "level3Maximum": equates["ENEMYAI_THRESHOLD_HEAL3"],
            },
            "selection": [
                {"missingHp": "0..2", "level": -1},
                {"missingHp": "3..14", "level": 0},
                {"missingHp": "15..28", "level": 2, "requiresKnownLevel": 3},
                {"missingHp": "29+", "level": 3, "requiresKnownLevel": 4},
            ],
            "fallbackLevel": 0,
            "returnsZeroBasedLevel": True,
            "neverReturnsLevel2FromHelper": True,
            "mpCheck": {
                "candidateShiftBits": 5,
                "requiredShiftBits": 6,
                "packedBaseSpellEntryIsNotMasked": True,
                "decrementsUntilAffordable": True,
            },
            "level2Override": {
                "selectedLevelBeforeOverride": 0,
                "targetMustDifferFromCaster": True,
                "minimumCasterMp": equates["ENEMYAI_MIN_MP_HEAL3"],
                "minimumKnownLevel": 3,
                "resultLevel": equates["SPELLENTRY_LV2"],
            },
        },
        "targetPriority": {
            "criticalCommandset": equates["AICOMMANDSET_CRITICAL"],
            "leaderCommandset": equates["AICOMMANDSET_LEADER"],
            "maximum": equates["MOVETYPES_NUMBER"],
            "criticalAndLeaderUseMaximum": True,
            "movetypeTable": movetype_priorities,
            "unmatchedPriority": 0,
            "aoePerTargetBase": 4,
            "aoeScore": "sum-movetype-priority-plus-four-per-target",
            "storedAsByte": True,
        },
    }


def _parse_support(disasm: Path) -> dict[str, Any]:
    command_path = SOURCE_ROOT / "command/support.asm"
    priority_path = SOURCE_ROOT / "command/support/prioritizetargetsforsupportspells.asm"
    command_source = (disasm / command_path).read_text(encoding="utf-8")
    priority_source = (disasm / priority_path).read_text(encoding="utf-8")
    command = _function_block(command_source, "aiCommand_Support")
    prioritizers = {
        name: _function_block(priority_source, name)
        for name in (
            "PrioritizeTargetsForSupportSpell_Attack",
            "PrioritizeTargetsForSupportSpell_Boost2",
            "PrioritizeTargetsForSupportSpell_Dispel",
            "PrioritizeTargetsForSupportSpell_Muddle2",
        )
    }
    calculators = {
        name: _function_block(priority_source, name)
        for name in (
            "CalculateTargetPriorityForSpell_Dispel",
            "CalculateTargetPriorityForSpell_Boost",
            "CalculateTargetPriorityForSpell_Attack",
        )
    }
    equates = _equates(disasm)

    required_fragments = (
        (command, "btst    #COMBATANT_BIT_ENEMY,d0"),
        (command, "bsr.w   IsCombatantConfused"),
        (command, "bsr.w   GetNextSupportSpell"),
        (command, "cmpi.w  #SPELL_MUDDLE|SPELL_LV2,d1"),
        (command, "cmpi.w  #SPELL_DISPEL,d1"),
        (command, "move.b  SPELLDEF_OFFSET_MP_COST(a0),d2"),
        (command, "btst    #SPELLPROPS_BIT_TARGETING,d5"),
        (command, "bsr.w   PrioritizeTargetsForSupportSpell_Attack"),
        (command, "bsr.w   PrioritizeTargetsForSupportSpell_Boost2"),
        (command, "bsr.w   PrioritizeTargetsForSupportSpell_Muddle2"),
        (command, "bsr.w   PrioritizeTargetsForSupportSpell_Dispel"),
        (command, "cmp.b   d1,d3"),
        (command, "move.b  (a0,d2.w),d0"),
        (command, "bsr.w   DetermineAttackPosition"),
        (
            prioritizers["PrioritizeTargetsForSupportSpell_Attack"],
            "move.w  #SPELL_ATTACK,d1",
        ),
        (
            prioritizers["PrioritizeTargetsForSupportSpell_Attack"],
            "move.w  #SPELL_DISPEL|SPELL_LV2,d1",
        ),
        (
            prioritizers["PrioritizeTargetsForSupportSpell_Boost2"],
            "move.w  #SPELL_DISPEL|SPELL_LV2,d1",
        ),
        (prioritizers["PrioritizeTargetsForSupportSpell_Dispel"], "cmpi.b  #2,d0"),
        (prioritizers["PrioritizeTargetsForSupportSpell_Muddle2"], "cmpi.b  #3,d0"),
        (calculators["CalculateTargetPriorityForSpell_Dispel"], "bsr.w   GetNextUsableAttackSpell"),
        (calculators["CalculateTargetPriorityForSpell_Dispel"], "bsr.w   GetNextHealingSpell"),
        (calculators["CalculateTargetPriorityForSpell_Boost"], "cmpi.w  #2,d5"),
        (calculators["CalculateTargetPriorityForSpell_Attack"], "move.w  #255,d0"),
        (calculators["CalculateTargetPriorityForSpell_Attack"], "cmpi.w  #255,d5"),
        (calculators["CalculateTargetPriorityForSpell_Attack"], "addi.w  #1,d5"),
        (calculators["CalculateTargetPriorityForSpell_Attack"], "ble.s   @Next"),
        (calculators["CalculateTargetPriorityForSpell_Attack"], "move.w  #255,d5"),
    )
    if any(fragment not in block for block, fragment in required_fragments):
        raise ValueError("battle AI support source contract drift")

    muddle2 = equates["SPELL_MUDDLE"] | equates["SPELL_LV2"]
    dispel1 = equates["SPELL_DISPEL"]
    dispel2 = dispel1 | equates["SPELL_LV2"]
    return {
        "sourcePaths": [command_path.as_posix(), priority_path.as_posix()],
        "command": {
            "function": "aiCommand_Support",
            "enemyCastersOnly": True,
            "confusedCasterStays": True,
            "firstSupportSpellOnly": True,
            "acceptedSpellEntries": [
                {"name": "SPELL_MUDDLE_LV2", "value": muddle2},
                {"name": "SPELL_DISPEL_LV1", "value": dispel1},
            ],
            "otherSupportSpellStaysWithoutScanningLaterSlots": True,
            "usesDefinitionMpCost": True,
            "targetSideUsesSpellTargetingProperty": True,
            "highestPriorityTieBreak": "later-candidate-wins",
            "failedPositionAfterSelection": "stay-without-next-target-fallback",
        },
        "reachableRoutes": {
            "muddle2": {
                "centerScore": "area-target-count",
                "minimumScore": 3,
                "filtersCentersBelowMinimum": True,
            },
            "dispel1": {
                "perTargetScore": "one-if-has-attack-or-healing-spell",
                "attackSpellTakesPrecedenceOverHealingCheck": True,
                "minimumCenterScore": 2,
                "filtersCentersBelowMinimum": True,
            },
        },
        "unreachableRoutes": {
            "reason": "command-admission-accepts-only-muddle2-or-dispel1",
            "attack": {
                "routePresent": True,
                "reachableSpellEntry": equates["SPELL_ATTACK"],
                "areaSpellEntryUsed": dispel2,
                "areaSpellMismatch": True,
                "eligibleTarget": "no-attack-spell-and-live-last-target",
                "scoreWhenAnyEligibleTarget": 255,
                "scoreSaturationCause": "addi-overwrites-cmpi-flags-before-ble",
            },
            "boost2": {
                "routePresent": True,
                "intendedSpellEntry": equates["SPELL_BOOST"] | equates["SPELL_LV2"],
                "reachableAndAreaSpellEntryUsed": dispel2,
                "spellEntryMismatch": True,
                "eligibleTarget": "no-attack-spell-and-live-last-target",
                "minimumEligibleTargets": 2,
                "score": "eligible-target-count-or-zero",
            },
        },
    }


def _parse_action_choice(disasm: Path) -> dict[str, Any]:
    choice_path = SOURCE_ROOT / "command/attack/determinebattleaction.asm"
    data_path = Path("data/battles/global/aipriority.asm")
    choice_source = (disasm / choice_path).read_text(encoding="utf-8")
    data_source = (disasm / data_path).read_text(encoding="utf-8")
    choice = _function_block(choice_source, "DetermineBattleactionForAttackAiCommand")
    equates = _equates(disasm)
    required_fragments = (
        (choice, "bset    #0,d3"),
        (choice, "bset    #1,d3"),
        (choice, "bset    #2,d3"),
        (choice, "andi.b  #%110,d4"),
        (choice, "cmpi.w  #SPELL_AQUA,d1"),
        (choice, "move.b  #6,d6"),
        (choice, "cmpi.b  #2,d7"),
        (choice, "cmpi.b  #4,d7"),
        (choice, "cmpi.b  #3,d7"),
        (choice, "cmpi.b  #5,d7"),
        (choice, "move.b  #2,d6"),
        (choice, "jsr     j_GenerateRandomNumberUnderD6"),
        (choice, "cmpi.b  #15,d4"),
        (choice, "move.b  #15,priority(a6)"),
        (choice, "lea     (pt_AttackPriorityClassesForMovetype).l,a4"),
        (choice, "lea     (table_AttackPriority_Mage).l,a4"),
        (choice, "cmpi.b  #48,d4"),
        (choice, "move.b  #-1,d2"),
        (choice, "cmp.b   d5,d2"),
        (choice, "bgt.s   @loc_51"),
        (choice, "move.b  d5,d2"),
    )
    if any(fragment not in block for block, fragment in required_fragments):
        raise ValueError("battle AI action-choice source contract drift")

    class_table_names = (
        "table_AttackPriority_Regular",
        "table_AttackPriority_Mage",
        "table_AttackPriority_Archer",
        "table_AttackPriority_Flying",
    )
    class_tables: dict[str, list[int]] = {}
    for name in class_table_names:
        tokens = DC_BYTE_VALUE_PATTERN.findall(_label_block(data_source, name))
        class_tables[name] = [int(token) if token == "-1" else equates[token] for token in tokens]
    if any(len(values) != equates["CLASSES_NUMBER"] for values in class_tables.values()):
        raise ValueError("battle AI class-order table shape drift")
    class_pointer_targets = DC_LONG_TARGET_PATTERN.findall(
        _label_block(data_source, "pt_AttackPriorityClassesForMovetype")
    )
    if len(class_pointer_targets) != 16:
        raise ValueError("battle AI class-order pointer table shape drift")

    return {
        "sourcePaths": [choice_path.as_posix(), data_path.as_posix()],
        "function": "DetermineBattleactionForAttackAiCommand",
        "viabilityBits": {"physical": 0, "spell": 1, "item": 2},
        "noViableAction": {"action": "stay", "target": -1, "priority": 0},
        "actionSelection": {
            "physicalOnly": "physical",
            "spellOnlyWithPhysical": {
                "rngRange": 6,
                "physicalRolls": [0, 1, 3, 5],
                "spellRolls": [2, 4],
                "aquaAlwaysSpell": True,
            },
            "spellOnlyWithoutPhysical": "spell",
            "itemOnlyWithPhysical": {
                "rngRange": 6,
                "physicalRolls": [0, 1, 2, 4],
                "itemRolls": [3, 5],
            },
            "itemOnlyWithoutPhysical": "item",
            "spellAndItem": {
                "rngRange": 2,
                "spellRolls": [0],
                "itemRolls": [1],
                "physicalIgnored": True,
            },
        },
        "prioritySelection": {
            "comparison": "signed-byte",
            "initialMaximum": 0,
            "criticalThreshold": 15,
            "returnedPriorityCap": 15,
            "collectsAllMaximumPriorityTargets": True,
            "collectionOrder": "reverse-input-order",
        },
        "criticalEnemyClassTieBreak": {
            "appliesTo": "enemy-with-priority-at-least-15",
            "spellActionForcesMageTable": True,
            "movetypePointerTargets": class_pointer_targets,
            "classOrderTables": class_tables,
            "retainsEarliestClassCohort": True,
        },
        "movementTieBreak": {
            "comparison": "signed-byte",
            "initialBest": -1,
            "ordinaryNonnegativeResult": "maximum-movement-value",
            "equalValuePolicy": "later-collected-target-wins",
            "maximumCandidateCount": 48,
        },
    }


def _parse_movement(disasm: Path) -> dict[str, Any]:
    move_path = SOURCE_ROOT / "command/move.asm"
    order_path = SOURCE_ROOT / "command/moveorder.asm"
    builder_path = SOURCE_ROOT / "command/moveorder/buildmovestringformoveorder.asm"
    costs_path = Path("data/battles/global/krakenmovecosts.asm")
    move = _function_block((disasm / move_path).read_text(encoding="utf-8"), "aiCommand_Move")
    order = _function_block(
        (disasm / order_path).read_text(encoding="utf-8"), "aiCommand_MoveOrder"
    )
    builder = _function_block(
        (disasm / builder_path).read_text(encoding="utf-8"),
        "BuildMoveStringForMoveOrder",
    )
    costs_source = (disasm / costs_path).read_text(encoding="utf-8")
    equates = _equates(disasm)
    checks = (
        (move, "move.w  #128,d0"),
        (move, "bsr.w   IsCombatantConfused"),
        (move, "bsr.w   GetMoveCostToEntity"),
        (move, "bcc.s   @loc_18"),
        (move, "lea     (pt_AttackPriorityClassesForMovetype).l,a1"),
        (move, "cmpi.b  #1,d0"),
        (move, "cmpi.b  #3,d0"),
        (move, "cmpi.b  #ENEMY_KRAKEN_LEG,d1"),
        (move, "cmpi.b  #ENEMY_KRAKEN_ARM,d1"),
        (move, "cmpi.b  #ENEMY_KRAKEN_HEAD,d1"),
        (move, "move.w  #4,d3"),
        (move, "clr.w   d3\n                clr.w   d4"),
        (move, "move.w  #1,d3\n                move.w  #1,d4"),
        (order, "btst    #COMBATANT_BIT_ENEMY,d0"),
        (order, "bsr.w   GetCurrentMov"),
        (order, "cmpi.b  #AIORDER_NONE,d1"),
        (order, "btst    #AIORDER_BIT_MOVE_TO,d1"),
        (order, "bsr.w   aiCommand_Attack"),
        (order, "bsr.w   BuildMoveStringForMoveOrder"),
        (order, "move.b  pathfindingMode(a6),d2"),
        (builder, "move.w  #128,d0"),
        (builder, "add.w   d3,d3"),
        (builder, "move.w  #3,d3"),
        (builder, "move.w  #3,d4"),
    )
    if any(fragment not in block for block, fragment in checks):
        raise ValueError("battle AI movement source contract drift")
    costs = [
        int(value)
        for value in DC_BYTE_INTEGER_PATTERN.findall(
            _label_block(costs_source, "table_KrakenMoveCosts")
        )
    ]
    if len(costs) != 16:
        raise ValueError("battle AI Kraken move-cost table shape drift")
    return {
        "sourcePaths": [
            move_path.as_posix(),
            order_path.as_posix(),
            builder_path.as_posix(),
            costs_path.as_posix(),
        ],
        "move": {
            "movementArrayBudget": 128,
            "confusedTarget": "first-living-side-index-without-health-or-map-check",
            "normalTargets": "opposing-living-on-map-combatants",
            "emptyNormalTargetGuard": False,
            "moveCostSort": "ascending-unsigned-byte",
            "classNeighborReorderAppliesWhenTargetListIsEnemies": True,
            "classRankDistanceMaximum": 1,
            "combatantIndexDistanceMaximum": 3,
            "selectedTarget": "first-after-reordering",
            "krakenEnemyIndexes": [
                equates[name]
                for name in ("ENEMY_KRAKEN_LEG", "ENEMY_KRAKEN_ARM", "ENEMY_KRAKEN_HEAD")
            ],
            "krakenMoveCosts": costs,
            "initialMoveStringBudget": 4,
            "postMoveAttackPositionRadii": [0, 1],
            "returnsSuccessEvenWhenActionBecomesStay": True,
        },
        "moveOrder": {
            "enemyOnly": True,
            "allyStayReadsUninitializedPathfindingMode": True,
            "stayConditions": [
                "zero-mov",
                "no-order",
                "dead-follow-target",
                "terrain-check-failure",
            ],
            "targetTypeSelectsPathIndependentlyOfOrderBits": True,
            "triesAttackBeforeMovement": True,
            "movementOnlyAction": "stay-with-move-string",
            "pathfindingModes": {"regular": 0, "blockNonMovable": 1, "blockAndCarve": 2},
        },
        "moveOrderBuilder": {
            "movementArrayBudget": 128,
            "preliminaryMoveBudget": "current-mov-times-two",
            "attackPositionRadii": [0, 1, 2, 3],
            "failureMoveString": -1,
        },
    }


def _parse_radius_rings(source: str) -> list[dict[str, Any]]:
    rings = []
    for radius in range(5):
        match = re.search(
            rf"^list_Radius{radius}:\s+dc\.b\s+([^;\r\n]+)(?P<body>.*?)(?=^list_Radius|^\s+align)",
            source,
            re.MULTILINE | re.DOTALL,
        )
        if not match:
            raise ValueError(f"missing battle AI radius-{radius} coordinate list")
        values = [int(match.group(1).strip())]
        for line in match.group("body").splitlines():
            statement = _strip_comment(line).strip()
            if not statement.startswith("dc.b"):
                continue
            values.extend(int(value.strip()) for value in statement[4:].split(","))
        count = values[0]
        coordinates = [values[index : index + 2] for index in range(1, len(values), 2)]
        if len(coordinates) != count or any(len(pair) != 2 for pair in coordinates):
            raise ValueError(f"battle AI radius-{radius} coordinate count drift")
        if any(abs(x) + abs(y) != radius for x, y in coordinates):
            raise ValueError(f"battle AI radius-{radius} is not a Manhattan ring")
        rings.append({"radius": radius, "count": count, "coordinates": coordinates})
    return rings


def _parse_movement_helpers(source: str) -> dict[str, Any]:
    quadrant = _function_block(source, "DetermineMoveOrderQuadrant")
    block_non_movable = _function_block(source, "BlockNonMovableSpacesAroundDestination")
    direct_carve = _function_block(source, "BlockAndCarveAroundDestination")
    tethered_carve = _function_block(source, "BlockAndCarveForTetheredTarget")
    clear_all = _function_block(source, "ClearAllTemporaryObstructionFlags")
    apply_quadrant = _function_block(source, "ApplyQuadrantTerrainMarking")
    mark_rectangle = _function_block(source, "MarkTerrainRectangleObstructed")
    clear_around = _function_block(source, "ClearObstructionFlagsAroundDestination")
    required = (
        (quadrant, "move.b  d2,d0"),
        (quadrant, "bset    #0,d5"),
        (quadrant, "bset    #1,d5"),
        (quadrant, "subi.w  #4,d3"),
        (quadrant, "addi.w  #4,d3"),
        (block_non_movable, "bra.w   @Done"),
        (block_non_movable, "jsr     j_BuildMovementArrays"),
        (block_non_movable, "btst    #TERRAIN_BIT_OCCUPIED,d0"),
        (direct_carve, "bsr.w   BlockAndCarveForTetheredTarget"),
        (direct_carve, "lea     list_Radius2(pc), a0"),
        (tethered_carve, "lea     list_Radius4(pc), a0"),
        (clear_all, "bclr    #TERRAIN_BIT_OCCUPIED,d0"),
        (clear_all, "bclr    #TERRAIN_BIT_IMPASSABLE,d0"),
        (apply_quadrant, "move.b  #3,d1"),
        (mark_rectangle, "mulu.w  #MAP_SIZE_MAX_TILEWIDTH,d2"),
        (clear_around, "cmpi.w  #MAP_SIZE_MAX_TILEHEIGHT,d2"),
        (clear_around, "cmpi.w  #MAP_SIZE_MAX_TILEWIDTH,d1"),
    )
    if any(fragment not in block for block, fragment in required):
        raise ValueError("battle AI movement-helper source contract drift")
    return {
        "orderSelection": {
            "quadrantHelper": "primary-else-secondary",
            "obstructionHelpers": "primary-only",
            "secondaryOnlyOrderIsIgnoredByObstructionHelpers": True,
            "deadFollowTargetResult": -1,
        },
        "quadrantBits": {"bit0": "destination-left", "bit1": "destination-below"},
        "boundExpansionTiles": 4,
        "temporaryObstructionBits": [6, 7],
        "permanentObstructionValue": 255,
        "blockNonMovable": {
            "buildsMovementGridFromDestination": True,
            "marksGridUnreachableTerrain": True,
            "skipsPermanentObstruction": True,
        },
        "blockAndCarve": {
            "lastTargetSelectsTetheredVariant": True,
            "center": "move-order-position",
            "blocksAllNonPermanentTerrainFirst": True,
            "standardClearRadii": [0, 1, 2],
            "tetheredClearRadii": [0, 1, 2, 3, 4],
        },
        "quadrantMarking": {
            "rectanglesMarkedPerDirection": 3,
            "markedSelectionCodes": {
                "right-above": [1, 2, 3],
                "left-above": [0, 2, 3],
                "left-below": [0, 1, 3],
                "right-below": [0, 1, 2],
            },
        },
        "radiusRings": _parse_radius_rings(source),
    }


def _parse_ai_commandsets(source: str, equates: dict[str, int]) -> list[dict[str, Any]]:
    pointers = DC_LONG_TARGET_PATTERN.findall(_label_block(source, "pt_AiCommandsets"))
    if len(pointers) != 16:
        raise ValueError("battle AI commandset pointer count drift")
    parsed: dict[str, list[int]] = {}
    for label in set(pointers):
        block = " ".join(
            statement
            for line in _label_block(source, label).splitlines()
            if (statement := _strip_comment(line).strip())
        )
        match = re.search(r"\baiCommandset\s+(.+)", block)
        if not match:
            raise ValueError(f"battle AI commandset body is missing: {label}")
        names = re.findall(r"[A-Z][A-Z0-9_]+", match.group(1))
        parsed[label] = [equates[f"AI_COMMAND_{name}"] for name in names]
    return [
        {"id": index, "label": label, "commands": parsed[label]}
        for index, label in enumerate(pointers)
    ]


def _parse_control(disasm: Path, source: str) -> dict[str, Any]:
    equates = _equates(disasm)
    commandsets_source = (disasm / "data/battles/global/aicommandsets.asm").read_text(
        encoding="utf-8"
    )
    swarm_source = (disasm / "data/battles/global/swarmbattles.asm").read_text(encoding="utf-8")
    start = _function_block(source, "StartAiControl")
    defeated = _function_block(source, "CountDefeatedEnemies")
    line_attacker = _function_block(source, "ProcessLineAttackerAi")
    exploder = _function_block(source, "ProcessExploderAi")
    required = (
        (start, "move.w  #AICOMMANDSET_ATTACKER1,d5"),
        (start, "cmpi.b  #AICOMMANDSET_SWARM,d1"),
        (start, "move.w  #0,(a0)"),
        (start, "cmpi.w  #ENEMY_PRISM_FLOWER,d1"),
        (start, "cmpi.w  #ENEMY_ZEON_GUARD,d1"),
        (start, "cmpi.w  #ENEMY_BURST_ROCK,d1"),
        (start, "move.w  d2,d1"),
        (start, "move.w  #AIORDER_NONE,d2"),
        (start, "jsr     j_ClearAllTemporaryObstructionFlags"),
        (defeated, "move.w  #BATTLESPRITESET_SUBSECTION_ALLIES,d1"),
        (line_attacker, "move.w  #BATTLEACTION_PRISM_LASER,(a0)"),
        (exploder, "move.w  #6,d6"),
        (exploder, "cmpi.b  #4,d7"),
        (exploder, "move.w  #BATTLEACTION_BURST_ROCK,(a0)"),
        (exploder, "move.w  #AI_COMMAND_MOVE1,d1"),
    )
    if any(fragment not in block for block, fragment in required):
        raise ValueError("battle AI control source contract drift")

    battle_match = re.search(r"^\s*battles\s+(.+)$", swarm_source, re.MULTILINE)
    if not battle_match:
        raise ValueError("battle AI swarm battle list is missing")
    battle_names = [name.strip() for name in battle_match.group(1).split(",")]
    battles = _named_values([f"BATTLE_{name}" for name in battle_names], equates)
    swarm_tables = []
    for index in range(3):
        values = [
            int(value)
            for value in DC_BYTE_INTEGER_PATTERN.findall(
                _label_block(swarm_source, f"table_SwarmAiEnemyCounts{index}")
            )
        ]
        swarm_tables.append(values)
    if [len(values) for values in swarm_tables] != [11, 12, 16]:
        raise ValueError("battle AI swarm threshold table length drift")

    pathfinding_names = DC_BYTE_VALUE_PATTERN.findall(
        _label_block(commandsets_source, "table_PathfindingModesForAiCommandset")
    )
    if len(pathfinding_names) != 18:
        raise ValueError("battle AI pathfinding-mode table length drift")
    return {
        "sourcePaths": [
            "code/gameflow/battle/ai/startaicontrol.asm",
            "data/battles/global/aicommandsets.asm",
            "data/battles/global/swarmbattles.asm",
        ],
        "allyCommandset": equates["AICOMMANDSET_ATTACKER1"],
        "swarm": {
            "commandset": equates["AICOMMANDSET_SWARM"],
            "requiresFullHp": True,
            "battles": battles,
            "activationThresholdsByEnemySlot": swarm_tables,
            "zeroThresholdBypassesWait": True,
            "activatesWhenDefeatedCountReachesThreshold": True,
            "defeatedCountIncorrectlyUsesAllySubsectionLength": True,
        },
        "activation": {
            "clearsNewlyTriggeredRegionsBeforeRead": True,
            "noTriggerRegionsStartsActivated": True,
            "inactiveCombatantRunsStandbyThenStays": True,
            "deadPrimaryFollowOrderPromotesSecondary": True,
        },
        "specialAttackers": {
            "lineEnemyIds": [
                equates["ENEMY_PRISM_FLOWER"],
                equates["ENEMY_ZEON_GUARD"],
            ],
            "lineAction": equates["BATTLEACTION_PRISM_LASER"],
            "lineTarget": "first-facing-target",
            "lineNoTargetAction": equates["BATTLEACTION_STAY"],
            "exploderEnemyId": equates["ENEMY_BURST_ROCK"],
            "exploderSpell": equates["SPELL_B_ROCK"],
            "exploderThinkingRngRange": 6,
            "exploderRoll": 4,
            "exploderAction": equates["BATTLEACTION_BURST_ROCK"],
            "exploderTarget": "self",
            "exploderFallback": "execute-move1-then-force-stay",
        },
        "pathfindingModes": [equates[name] for name in pathfinding_names],
        "commandsets": _parse_ai_commandsets(commandsets_source, equates),
        "commandLoopStopsOnFirstSuccess": True,
        "alwaysClearsTemporaryObstructionOnExit": True,
    }


def _parse_unused_helpers(disasm: Path, healing_source: str, slot_source: str) -> dict[str, Any]:
    equates = _equates(disasm)
    mp_current_vs_max = _function_block(healing_source, "sub_D3CA")
    mp_input_vs_max = _function_block(healing_source, "sub_D3E0")
    mp_input_vs_current = _function_block(healing_source, "sub_D3F0")
    spell_slot = _function_block(slot_source, "GetSlotContainingSpell")
    item_slot = _function_block(slot_source, "GetSlotContainingItem")
    required = (
        (mp_current_vs_max, "jsr     GetCurrentMp"),
        (mp_current_vs_max, "jsr     GetMaxMp"),
        (mp_input_vs_max, "jsr     GetMaxMp"),
        (mp_input_vs_current, "jsr     GetCurrentMp"),
        (mp_input_vs_current, "mulu.w  #3,d2"),
        (mp_input_vs_current, "cmp.w   d2,d1"),
        (spell_slot, "andi.b  #SPELLENTRY_MASK_INDEX,d1"),
        (spell_slot, "cmpi.w  #4,d3"),
        (spell_slot, "moveq   #SPELL_NOTHING,d1"),
        (item_slot, "andi.w  #ITEMENTRY_MASK_INDEX,d1"),
        (item_slot, "cmpi.w  #4,d3"),
        (item_slot, "move.w  #ITEM_NOTHING,d1"),
    )
    if any(fragment not in block for block, fragment in required):
        raise ValueError("battle AI unused-helper source contract drift")
    symbols = (
        "sub_D3CA",
        "sub_D3E0",
        "sub_D3F0",
        "GetSlotContainingSpell",
        "GetSlotContainingItem",
    )
    all_ai_source = "\n".join(
        path.read_text(encoding="utf-8") for path in (disasm / SOURCE_ROOT).rglob("*.asm")
    )
    called = [
        symbol
        for symbol in symbols
        if re.search(rf"\b(?:bsr|jsr)(?:\.[bwl])?\s+{symbol}\b", all_ai_source)
    ]
    if called:
        raise ValueError(f"battle AI unused helpers gained call sites: {called}")
    return {
        "directCallSitesInsideBattleAi": 0,
        "mpComparisons": [
            "max-mp-vs-three-times-current-mp",
            "max-mp-vs-three-times-input",
            "current-mp-vs-three-times-input",
        ],
        "mpResultChannel": "condition-codes-only",
        "slotLookup": {
            "slotCount": 4,
            "comparesBaseIndexOnly": True,
            "spellNoMatch": {"entry": equates["SPELL_NOTHING"], "slot": 4},
            "itemNoMatch": {"entry": equates["ITEM_NOTHING"], "slot": 4},
            "matchReturnsStoredEntryAndSlot": True,
        },
    }


def _standby_eligibility_outcome(
    primary_order: bool,
    secondary_order: bool,
    primary_trigger_configured: bool,
    secondary_trigger_configured: bool,
) -> str:
    if (primary_order and primary_trigger_configured) or (
        secondary_order and secondary_trigger_configured
    ):
        return "stay"
    if (
        primary_order
        and not primary_trigger_configured
        and not secondary_order
        and secondary_trigger_configured
    ):
        return "move-order"
    if (not primary_order and primary_trigger_configured) or (
        not secondary_order and secondary_trigger_configured
    ):
        return "regular-move"
    return "stay"


def _parse_standby(disasm: Path, movement_source: str, eligibility_source: str) -> dict[str, Any]:
    movement = _function_block(movement_source, "DetermineAiStandbyMovement")
    eligibility = _function_block(eligibility_source, "ValidateAiStandbyEligibility")
    table_source = (disasm / "data/battles/global/aistandbymovements.asm").read_text(
        encoding="utf-8"
    )
    required = (
        (movement, "move.w  #8,d6"),
        (movement, "move.b  d1,startingX(a6)"),
        (movement, "move.b  d1,startingY(a6)"),
        (movement, "andi.b  #BYTE_LOWER_NIBBLE_MASK,d1"),
        (movement, "lsr.w   #NIBBLE_SHIFT_COUNT,d1"),
        (movement, "bclr    d6,d7"),
        (movement, "cmpi.b  #MAP_SIZE_MAX_TILEWIDTH,d1"),
        (movement, "cmpi.b  #MAP_SIZE_MAX_TILEHEIGHT,d2"),
        (eligibility, "move.b  #1,d1"),
        (eligibility, "clr.w   d1"),
        (eligibility, "move.b  #1,d2"),
    )
    if any(fragment not in block for block, fragment in required):
        raise ValueError("battle AI standby source contract drift")

    pointers = DC_LONG_TARGET_PATTERN.findall(_label_block(table_source, "pt_StandbyAiMovements"))
    if pointers != ["table_StandbyAiMovements1", "table_StandbyAiMovements2"]:
        raise ValueError("battle AI standby movement pointers drift")
    movement_tables = []
    for move_count, label in zip((3, 4), pointers, strict=True):
        values: list[int] = []
        for line in _label_block(table_source, label).splitlines():
            statement = _strip_comment(line).strip()
            if not statement.startswith("dc.b"):
                continue
            values.extend(int(value.strip()) for value in statement[4:].split(","))
        coordinates = [values[index : index + 2] for index in range(0, len(values), 2)]
        if len(coordinates) != move_count or any(len(pair) != 2 for pair in coordinates):
            raise ValueError(f"battle AI standby movement table drift: {label}")
        movement_tables.append(
            {"moveCount": move_count, "label": label, "coordinates": coordinates}
        )

    decision_matrix = []
    for primary_order in (False, True):
        for secondary_order in (False, True):
            for primary_trigger_configured in (False, True):
                for secondary_trigger_configured in (False, True):
                    decision_matrix.append(
                        {
                            "primaryOrder": primary_order,
                            "secondaryOrder": secondary_order,
                            "primaryTriggerConfigured": primary_trigger_configured,
                            "secondaryTriggerConfigured": secondary_trigger_configured,
                            "callerOutcome": _standby_eligibility_outcome(
                                primary_order,
                                secondary_order,
                                primary_trigger_configured,
                                secondary_trigger_configured,
                            ),
                        }
                    )
    equates = _equates(disasm)
    return {
        "rngRange": 8,
        "immediateStayRolls": [2, 4, 6],
        "movementCandidateRolls": [0, 1, 3, 5, 7],
        "memory": {
            "lowerNibble": "move-count",
            "initializedMoveCounts": [3, 4],
            "upperNibble": "previous-relative-position-index",
            "previousCandidateExcluded": True,
            "noAlternativeClearsWholeByte": True,
        },
        "movementTables": movement_tables,
        "regularStartCoordinates": "combatant-starting-x-y",
        "moveOrderStartingYUsesXResult": True,
        "candidatePrecheck": {
            "actualMaximum": [
                equates["MAP_SIZE_MAX_TILEWIDTH"] - 1,
                equates["MAP_SIZE_MAX_TILEHEIGHT"] - 1,
            ],
            "acceptedMaximum": [
                equates["MAP_SIZE_MAX_TILEWIDTH"],
                equates["MAP_SIZE_MAX_TILEHEIGHT"],
            ],
            "acceptsOnePastMapMaximum": True,
        },
        "selection": "uniform-among-valid-except-previous",
        "successfulMoveStillUsesStayAction": True,
        "eligibility": {
            "routineCommentMeaningConflictsWithCaller": True,
            "callerTreatsD1ZeroAsMoveAttempt": True,
            "callerTreatsD2NonzeroAsMoveOrder": True,
            "decisionMatrix": decision_matrix,
        },
    }


def _parse_remaining(disasm: Path) -> dict[str, Any]:
    entries = {
        "unusedHealingMp": (SOURCE_ROOT / "command/heal/unusedfunctions_D3CA.asm", "sub_D3CA"),
        "standbyMovement": (
            SOURCE_ROOT / "determineaistandbymovement_1.asm",
            "DetermineAiStandbyMovement",
        ),
        "standbyEligibility": (
            SOURCE_ROOT / "determineaistandbymovement_2.asm",
            "ValidateAiStandbyEligibility",
        ),
        "dispatcher": (SOURCE_ROOT / "executeaicommand.asm", "ExecuteAiCommand"),
        "highestSpellLevel": (
            SOURCE_ROOT / "gethighestusablespelllevel.asm",
            "GetHighestUsableSpellLevel",
        ),
        "movementHelpers": (
            SOURCE_ROOT / "movementhelperfunctions.asm",
            "DetermineMoveOrderQuadrant",
        ),
        "controlLoop": (SOURCE_ROOT / "startaicontrol.asm", "StartAiControl"),
        "unusedSlotLookup": (SOURCE_ROOT / "unusedfunctions_CF0E.asm", "GetSlotContainingSpell"),
    }
    sources = {
        key: (disasm / path).read_text(encoding="utf-8") for key, (path, _) in entries.items()
    }
    dispatcher = _function_block(sources["dispatcher"], "ExecuteAiCommand")
    standby = _function_block(sources["standbyMovement"], "DetermineAiStandbyMovement")
    spell = _function_block(sources["highestSpellLevel"], "GetHighestUsableSpellLevel")
    checks = (
        (dispatcher, "cmpi.b  #AI_COMMAND_MOVE_ORDER5,d1"),
        (standby, "move.w  #8,d6"),
        (standby, "cmpi.b  #2,d7"),
        (standby, "cmpi.b  #4,d7"),
        (standby, "cmpi.b  #6,d7"),
        (standby, "move.b  #4,d1"),
        (standby, "move.b  #3,d1"),
        (standby, "move.b  d1,startingX(a6)"),
        (standby, "move.b  d1,startingY(a6)"),
        (spell, "andi.w  #SPELLENTRY_MASK_INDEX,d1"),
        (spell, "lsl.w   #SPELLENTRY_OFFSET_LV,d1"),
        (spell, "dbf     d2,@Loop"),
        (spell, "moveq   #SPELL_NOTHING,d1"),
    )
    if any(fragment not in block for block, fragment in checks):
        raise ValueError("battle AI remaining-source contract drift")
    return {
        "sourcePaths": [path.as_posix() for path, _ in entries.values()],
        "representativeSymbols": {key: symbol for key, (_, symbol) in entries.items()},
        "dispatcher": {
            "handledCommandValues": [0, 1, 2, 3, 4, 5, 6, 7, 10, 11, 12, 13, 14, 16, 17, 18, 19],
            "reservedNoOpValues": [8, 9, 15],
            "unknownValues": "no-op",
            "moveOrderMappings": [
                {"command": 10, "targetType": 0, "pathfindingMode": 0},
                {"command": 16, "targetType": 2, "pathfindingMode": 2},
                {"command": 17, "targetType": 1, "pathfindingMode": 1},
                {"command": 18, "targetType": 0, "pathfindingMode": 1},
                {"command": 19, "targetType": 0, "pathfindingMode": 2},
            ],
        },
        "standby": _parse_standby(
            disasm, sources["standbyMovement"], sources["standbyEligibility"]
        ),
        "highestSpellLevel": {
            "search": "known-level-down-to-zero-until-affordable",
            "levelShiftBits": 6,
            "noAffordableResult": 63,
        },
        "movementHelpers": _parse_movement_helpers(sources["movementHelpers"]),
        "control": _parse_control(disasm, sources["controlLoop"]),
        "unusedHelpers": _parse_unused_helpers(
            disasm, sources["unusedHealingMp"], sources["unusedSlotLookup"]
        ),
    }


def _resolve_upstream(upstream_path: Path) -> tuple[Path, str, dict[str, Any]]:
    upstream_path = upstream_path.resolve(strict=True)
    toolchain = load_json(TOOLCHAIN)
    commit = subprocess.run(
        ["git", "rev-parse", "HEAD"],
        cwd=upstream_path,
        check=True,
        capture_output=True,
        text=True,
        encoding="utf-8",
    ).stdout.strip()
    if commit != toolchain["sf2disasm"]["commit"]:
        raise ValueError(
            f"battle AI inventory requires SF2DISASM {toolchain['sf2disasm']['commit']}, "
            f"got {commit}"
        )
    disasm = upstream_path / "disasm"
    source_root = disasm / SOURCE_ROOT
    if not source_root.is_dir():
        raise ValueError(f"battle AI source root is missing: {source_root}")
    return disasm, commit, toolchain


def build_battle_ai_inventory(upstream_path: Path) -> dict[str, Any]:
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    source_root = disasm / SOURCE_ROOT
    source_paths = sorted(source_root.rglob("*.asm"), key=lambda path: path.as_posix())
    if not source_paths:
        raise ValueError("battle AI source inventory is empty")

    files = [_parse_source_file(path, path.relative_to(disasm).as_posix()) for path in source_paths]
    all_labels = {label for file in files for label in file["globalLabels"]}
    direct_calls: Counter[str] = Counter()
    for file in files:
        for call in file["directCalls"]:
            direct_calls[call["target"]] += call["siteCount"]
    internal_targets = sorted(target for target in direct_calls if target in all_labels)
    external_targets = sorted(target for target in direct_calls if target not in all_labels)

    research_index = load_json(RESEARCH_INDEX)
    indexed_records = sorted(
        record["id"]
        for record in research_index["records"]
        if Path(record["sourcePath"]).is_relative_to(SOURCE_ROOT)
    )
    indexed_files = sorted(
        {
            record["sourcePath"]
            for record in research_index["records"]
            if Path(record["sourcePath"]).is_relative_to(SOURCE_ROOT)
        }
    )
    summary = {
        "fileCount": len(files),
        "sourceLineCount": sum(file["sourceLineCount"] for file in files),
        "statementCount": sum(file["statementCount"] for file in files),
        "globalLabelCount": sum(len(file["globalLabels"]) for file in files),
        "localLabelCount": sum(file["localLabelCount"] for file in files),
        "directCallSiteCount": sum(direct_calls.values()),
        "indirectCallSiteCount": sum(file["indirectCallSiteCount"] for file in files),
        "uniqueDirectTargetCount": len(direct_calls),
        "internalDirectTargetCount": len(internal_targets),
        "externalDirectTargetCount": len(external_targets),
        "indexedRecordCount": len(indexed_records),
        "indexedFileCount": len(indexed_files),
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {
            "repository": toolchain["sf2disasm"]["repository"],
            "commit": commit,
        },
        "scope": SOURCE_ROOT.as_posix(),
        "summary": summary,
        "actionFilters": _parse_action_filters(disasm),
        "attackPriority": _parse_attack_priority(disasm),
        "healing": _parse_healing(disasm),
        "support": _parse_support(disasm),
        "actionChoice": _parse_action_choice(disasm),
        "movement": _parse_movement(disasm),
        "remaining": _parse_remaining(disasm),
        "indexedRecordIds": indexed_records,
        "indexedSourcePaths": indexed_files,
        "internalDirectCallTargets": internal_targets,
        "externalDirectCallTargets": external_targets,
        "files": files,
    }


def _action_filter_facts(action_filters: dict[str, Any]) -> dict[str, Any]:
    return {
        "attackSpellAllowlist": [
            entry["value"] for entry in action_filters["attackSpell"]["confusedAllowlist"]
        ],
        "attackItemAllowlist": [
            entry["value"] for entry in action_filters["attackItem"]["confusedAllowlist"]
        ],
        "attackSpellRejectedCandidatePolicy": action_filters["attackSpell"][
            "rejectedCandidatePolicy"
        ],
        "attackItemRejectedCandidatePolicy": action_filters["attackItem"][
            "rejectedCandidatePolicy"
        ],
        "attackItemUnusableCandidatePolicy": action_filters["attackItem"][
            "unusableCandidatePolicy"
        ],
        "allyAttackSpellUsesConfusedFilter": action_filters["attackSpell"][
            "alliesAlwaysUseConfusedFilter"
        ],
        "allyAttackItemUsesConfusedFilter": action_filters["attackItem"][
            "alliesAlwaysUseConfusedFilter"
        ],
        "equippedAttackItemBypassesAiUseFlag": action_filters["attackItem"][
            "equippedItemBypassesAiUseFlag"
        ],
        "healingRainItem": action_filters["healingItem"]["healingRainItem"]["value"],
        "healingRainBypassesAiUseFlag": action_filters["healingItem"][
            "healingRainBypassesAiUseFlag"
        ],
        "healingSpellType": action_filters["healingSpell"]["requiredSpellType"],
        "supportSpellType": action_filters["supportSpell"]["requiredSpellType"],
    }


def _attack_priority_facts(attack_priority: dict[str, Any]) -> dict[str, Any]:
    selector = attack_priority["scriptSelector"]
    adjustment = attack_priority["classAdjustment"]
    return {
        "scriptsByDifficultyAndActivation": selector["scriptsByDifficultyAndActivation"],
        "allyActivationColumn": selector["allyActivationColumn"],
        "enemyRegularActivationMask": selector["enemyRegularActivationMask"],
        "enemySpellActivationMask": selector["enemySpellActivationMask"],
        "landEffectMultipliers256": attack_priority["regularPotentialDamage"][
            "landEffectMultipliers256"
        ],
        "minimumDamageBeforeLandEffect": attack_priority["regularPotentialDamage"][
            "minimumBeforeLandEffect"
        ],
        "remainingHpMinimum": attack_priority["remainingHpMinimum"],
        "script2DamageThreshold": attack_priority["thresholds"]["script2Damage"],
        "script2And4LowHpThreshold": attack_priority["thresholds"]["script2And4LowHp"],
        "script3MovementFormula": attack_priority["priorityScripts"][2]["movementPriorityFormula"],
        "classAdjustmentAlliesOnly": adjustment["alliesOnly"],
        "classAdjustmentSkippedWhenConfused": adjustment["skippedWhenConfused"],
        "previousTargetSarahUsesMageTable": adjustment["previousTargetSarahUsesMageTable"],
        "sarahIndex": adjustment["sarahIndex"],
        "movetypePointerTargets": adjustment["movetypePointerTargets"],
        "adjustmentTableCount": len(adjustment["tables"]),
        "adjustmentEntriesPerTable": sorted(
            {len(values) for values in adjustment["tables"].values()}
        ),
        "minimumClassAdjustment": adjustment["minimum"],
        "maximumClassAdjustment": adjustment["maximum"],
    }


def _healing_facts(healing: dict[str, Any]) -> dict[str, Any]:
    command = healing["command"]
    eligibility = healing["eligibility"]
    spell_level = healing["spellLevel"]
    priority = healing["targetPriority"]
    return {
        "confusedCasterExits": command["confusedCasterExits"],
        "targetSide": command["targetSide"],
        "healingRainCheckedFirst": command["healingRainCheckedFirst"],
        "healingRainCondition": command["healingRainCondition"],
        "healingRainTarget": command["healingRainTarget"],
        "acceptedSpellBases": command["acceptedSpellBases"],
        "minimumMpBeforeTargeting": command["minimumMpBeforeTargeting"],
        "itemFallbackAfterSpellFailure": command["itemFallbackAfterSpellFailure"],
        "itemTakesPrecedenceAtActionLoad": command["itemTakesPrecedenceAtActionLoad"],
        "requiresHealing": eligibility["requiresHealing"],
        "requiresHealingIncludesTwoThirds": eligibility["requiresHealingIncludesTwoThirds"],
        "halfHp": eligibility["halfHp"],
        "halfHpIncludesEquality": eligibility["halfHpIncludesEquality"],
        "missingHpThresholds": spell_level["missingHpThresholds"],
        "neverReturnsLevel2FromHelper": spell_level["neverReturnsLevel2FromHelper"],
        "mpCandidateShiftBits": spell_level["mpCheck"]["candidateShiftBits"],
        "mpRequiredShiftBits": spell_level["mpCheck"]["requiredShiftBits"],
        "packedBaseSpellEntryIsNotMasked": spell_level["mpCheck"][
            "packedBaseSpellEntryIsNotMasked"
        ],
        "level2Override": spell_level["level2Override"],
        "criticalCommandset": priority["criticalCommandset"],
        "leaderCommandset": priority["leaderCommandset"],
        "maximumPriority": priority["maximum"],
        "movetypePriorities": priority["movetypeTable"],
        "unmatchedPriority": priority["unmatchedPriority"],
        "aoePerTargetBase": priority["aoePerTargetBase"],
        "aoeScore": priority["aoeScore"],
        "storedAsByte": priority["storedAsByte"],
    }


def _support_facts(support: dict[str, Any]) -> dict[str, Any]:
    command = support["command"]
    routes = support["reachableRoutes"]
    unreachable = support["unreachableRoutes"]
    return {
        "enemyCastersOnly": command["enemyCastersOnly"],
        "confusedCasterStays": command["confusedCasterStays"],
        "firstSupportSpellOnly": command["firstSupportSpellOnly"],
        "acceptedSpellEntries": command["acceptedSpellEntries"],
        "otherSupportSpellStaysWithoutScanningLaterSlots": command[
            "otherSupportSpellStaysWithoutScanningLaterSlots"
        ],
        "usesDefinitionMpCost": command["usesDefinitionMpCost"],
        "targetSideUsesSpellTargetingProperty": command["targetSideUsesSpellTargetingProperty"],
        "highestPriorityTieBreak": command["highestPriorityTieBreak"],
        "failedPositionAfterSelection": command["failedPositionAfterSelection"],
        "muddle2": routes["muddle2"],
        "dispel1": routes["dispel1"],
        "unreachableReason": unreachable["reason"],
        "unreachableAttack": unreachable["attack"],
        "unreachableBoost2": unreachable["boost2"],
    }


def _action_choice_facts(action_choice: dict[str, Any]) -> dict[str, Any]:
    critical = action_choice["criticalEnemyClassTieBreak"]
    return {
        "viabilityBits": action_choice["viabilityBits"],
        "noViableAction": action_choice["noViableAction"],
        "actionSelection": action_choice["actionSelection"],
        "prioritySelection": action_choice["prioritySelection"],
        "criticalAppliesTo": critical["appliesTo"],
        "spellActionForcesMageTable": critical["spellActionForcesMageTable"],
        "movetypePointerTargets": critical["movetypePointerTargets"],
        "classOrderTableCount": len(critical["classOrderTables"]),
        "classOrderTableLengths": sorted(
            {len(values) for values in critical["classOrderTables"].values()}
        ),
        "retainsEarliestClassCohort": critical["retainsEarliestClassCohort"],
        "movementTieBreak": action_choice["movementTieBreak"],
    }


def _movement_facts(movement: dict[str, Any]) -> dict[str, Any]:
    return {
        "move": movement["move"],
        "moveOrder": movement["moveOrder"],
        "moveOrderBuilder": movement["moveOrderBuilder"],
    }


def _remaining_facts(remaining: dict[str, Any]) -> dict[str, Any]:
    return {
        key: remaining[key]
        for key in (
            "representativeSymbols",
            "dispatcher",
            "standby",
            "highestSpellLevel",
            "movementHelpers",
            "control",
            "unusedHelpers",
        )
    }


def verify_battle_ai_inventory(
    upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    manifest = load_json(MANIFEST)
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    priority_fixture = load_json(PRIORITY_FIXTURE)
    validate_json(
        priority_fixture,
        PRIORITY_FIXTURE_SCHEMA,
        owner=str(PRIORITY_FIXTURE),
    )
    healing_fixture = load_json(HEALING_FIXTURE)
    validate_json(
        healing_fixture,
        HEALING_FIXTURE_SCHEMA,
        owner=str(HEALING_FIXTURE),
    )
    support_fixture = load_json(SUPPORT_FIXTURE)
    validate_json(
        support_fixture,
        SUPPORT_FIXTURE_SCHEMA,
        owner=str(SUPPORT_FIXTURE),
    )
    action_choice_fixture = load_json(ACTION_CHOICE_FIXTURE)
    validate_json(
        action_choice_fixture,
        ACTION_CHOICE_FIXTURE_SCHEMA,
        owner=str(ACTION_CHOICE_FIXTURE),
    )
    movement_fixture = load_json(MOVEMENT_FIXTURE)
    validate_json(movement_fixture, MOVEMENT_FIXTURE_SCHEMA, owner=str(MOVEMENT_FIXTURE))
    remaining_fixture = load_json(REMAINING_FIXTURE)
    validate_json(remaining_fixture, REMAINING_FIXTURE_SCHEMA, owner=str(REMAINING_FIXTURE))
    rom_manifest = load_json(ROM_MANIFEST)
    output = build_battle_ai_inventory(upstream_path)
    validate_json(output, SCHEMA, owner="battle AI static inventory")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != rom_manifest["hashes"]["sha256"]
    ):
        raise ValueError("battle AI static fixture provenance drift")
    if _action_filter_facts(output["actionFilters"]) != fixture["expected"]:
        raise ValueError("battle AI action-filter facts disagree with fixture")
    if (
        priority_fixture["upstreamCommit"] != output["upstream"]["commit"]
        or priority_fixture["romSha256"] != rom_manifest["hashes"]["sha256"]
    ):
        raise ValueError("battle AI priority fixture provenance drift")
    if _attack_priority_facts(output["attackPriority"]) != priority_fixture["expected"]:
        raise ValueError("battle AI attack-priority facts disagree with fixture")
    if (
        healing_fixture["upstreamCommit"] != output["upstream"]["commit"]
        or healing_fixture["romSha256"] != rom_manifest["hashes"]["sha256"]
    ):
        raise ValueError("battle AI healing fixture provenance drift")
    if _healing_facts(output["healing"]) != healing_fixture["expected"]:
        raise ValueError("battle AI healing facts disagree with fixture")
    if (
        support_fixture["upstreamCommit"] != output["upstream"]["commit"]
        or support_fixture["romSha256"] != rom_manifest["hashes"]["sha256"]
    ):
        raise ValueError("battle AI support fixture provenance drift")
    if _support_facts(output["support"]) != support_fixture["expected"]:
        raise ValueError("battle AI support facts disagree with fixture")
    if (
        action_choice_fixture["upstreamCommit"] != output["upstream"]["commit"]
        or action_choice_fixture["romSha256"] != rom_manifest["hashes"]["sha256"]
    ):
        raise ValueError("battle AI action-choice fixture provenance drift")
    if _action_choice_facts(output["actionChoice"]) != action_choice_fixture["expected"]:
        raise ValueError("battle AI action-choice facts disagree with fixture")
    if (
        movement_fixture["upstreamCommit"] != output["upstream"]["commit"]
        or movement_fixture["romSha256"] != rom_manifest["hashes"]["sha256"]
    ):
        raise ValueError("battle AI movement fixture provenance drift")
    if _movement_facts(output["movement"]) != movement_fixture["expected"]:
        raise ValueError("battle AI movement facts disagree with fixture")
    if (
        remaining_fixture["upstreamCommit"] != output["upstream"]["commit"]
        or remaining_fixture["romSha256"] != rom_manifest["hashes"]["sha256"]
    ):
        raise ValueError("battle AI remaining fixture provenance drift")
    if _remaining_facts(output["remaining"]) != remaining_fixture["expected"]:
        raise ValueError("battle AI remaining facts disagree with fixture")
    if output["summary"] != manifest["summary"]:
        raise ValueError(
            "battle AI static summary drift: "
            f"expected {manifest['summary']}, got {output['summary']}"
        )
    encoded = _canonical_bytes(output)
    digest = hashlib.sha256(encoded).hexdigest().upper()
    if manifest["outputSha256"] != "PENDING" and digest != manifest["outputSha256"]:
        raise ValueError(
            "battle AI static inventory hash mismatch: "
            f"expected {manifest['outputSha256']}, got {digest}"
        )
    destination = output_path or repo_path(manifest["outputPath"])
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(encoded)
    return {
        "Inventory": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Files": output["summary"]["fileCount"],
        "GlobalLabels": output["summary"]["globalLabelCount"],
        "DirectCallSites": output["summary"]["directCallSiteCount"],
        "IndexedRecords": output["summary"]["indexedRecordCount"],
        "Status": "PASS",
    }
