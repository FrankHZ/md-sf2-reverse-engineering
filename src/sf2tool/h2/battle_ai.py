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

ID = "sf2-battle-ai-static-v1"
SOURCE_ROOT = Path("code/gameflow/battle/ai")
MANIFEST = repo_path("manifests/extractions/battle-ai-static.json")
SCHEMA = repo_path("schemas/battle-ai-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/battle-ai-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-battle-ai-static-fixture.schema.json")
PRIORITY_FIXTURE = repo_path("tests/fixtures/h2/battle-ai-priority-static-v1.json")
PRIORITY_FIXTURE_SCHEMA = repo_path("schemas/h2-battle-ai-priority-static-fixture.schema.json")
HEALING_FIXTURE = repo_path("tests/fixtures/h2/battle-ai-healing-static-v1.json")
HEALING_FIXTURE_SCHEMA = repo_path(
    "schemas/h2-battle-ai-healing-static-fixture.schema.json"
)
TOOLCHAIN = repo_path("manifests/toolchain.json")
RESEARCH_INDEX = repo_path("manifests/research-index.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")

GLOBAL_LABEL_PATTERN = re.compile(r"^([A-Za-z_][A-Za-z0-9_]*):")
LOCAL_LABEL_PATTERN = re.compile(r"^\s*(@[A-Za-z0-9_]+):")
CALL_PATTERN = re.compile(r"\b(?:bsr|jsr)(?:\.[bwl])?\s+([^\s,;]+)", re.IGNORECASE)
DIRECT_TARGET_PATTERN = re.compile(r"^[A-Za-z_][A-Za-z0-9_]*$")
EQUATE_PATTERN = re.compile(
    r"^([A-Z][A-Z0-9_]+):\s+equ\s+(\$[0-9A-Fa-f]+|-?\d+)", re.MULTILINE
)
SPELL_COMPARE_PATTERN = re.compile(r"cmpi\.b\s+#(SPELL_[A-Z0-9_]+),d5")
DC_LONG_TARGET_PATTERN = re.compile(r"^\s*dc\.l\s+([A-Za-z_][A-Za-z0-9_]*)", re.MULTILINE)
DC_BYTE_INTEGER_PATTERN = re.compile(r"^\s*dc\.b\s+(-?\d+)\b", re.MULTILINE)
DC_BYTE_VALUE_PATTERN = re.compile(
    r"^\s*dc\.b\s+(-?\d+|[A-Z][A-Z0-9_]+)\b", re.MULTILINE
)
REGISTER_TARGETS = {f"a{index}" for index in range(8)} | {
    f"d{index}" for index in range(8)
} | {"sp", "pc"}


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
    text = raw.decode("utf-8")
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
            {"target": target, "siteCount": count}
            for target, count in sorted(direct_calls.items())
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
            int(value)
            for value in DC_BYTE_INTEGER_PATTERN.findall(_label_block(data_source, name))
        ]
        for name in adjustment_table_names
    }
    if any(len(values) != 32 for values in adjustment_tables.values()):
        raise ValueError("battle AI class-priority adjustment table shape drift")

    potential_damage = _function_block(priority_source, "CalculatePotentialDamage")
    spell_resistance = _function_block(priority_source, "AdjustSpellPowerForResistance")
    remaining_hp = _function_block(
        priority_source, "CalculateRemainingHpAfterPotentialDamage"
    )
    script_blocks = {
        index: _function_block(priority_source, f"TargetPriorityScript{index}")
        for index in range(1, 5)
    }
    adjust_priority = _function_block(adjust_source, "AdjustTargetPriority")
    third_threshold = _function_block(
        helper_source, "IsRemainingHpAboveOneThirdOfCurrent"
    )
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
        int(value)
        for value in re.findall(r"move\.w\s+#(256|230|205),d2", potential_damage)
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
    movetype_values = [
        int(name) if name == "-1" else equates[name] for name in movetype_names
    ]
    movetype_priorities = [
        {
            "name": name,
            "value": value,
            "priority": equates["MOVETYPES_NUMBER"] - index,
        }
        for index, (name, value) in enumerate(
            zip(movetype_names, movetype_values, strict=True)
        )
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

    files = [
        _parse_source_file(path, path.relative_to(disasm).as_posix()) for path in source_paths
    ]
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
        "script2And4LowHpThreshold": attack_priority["thresholds"][
            "script2And4LowHp"
        ],
        "script3MovementFormula": attack_priority["priorityScripts"][2][
            "movementPriorityFormula"
        ],
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
        "itemTakesPrecedenceAtActionLoad": command[
            "itemTakesPrecedenceAtActionLoad"
        ],
        "requiresHealing": eligibility["requiresHealing"],
        "requiresHealingIncludesTwoThirds": eligibility[
            "requiresHealingIncludesTwoThirds"
        ],
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
