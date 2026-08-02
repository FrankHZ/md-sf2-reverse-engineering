from __future__ import annotations

import hashlib
import json
import re
import subprocess
from collections import Counter, defaultdict
from pathlib import Path
from typing import Any

from sf2tool.h2 import battle_scene_engine
from sf2tool.h2.battle_ai import _parse_source_file
from sf2tool.h2.battlefield import _require_ordered_fragments
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.source_text import decode_upstream_text

ID = "sf2-battle-actions-static-v1"
SOURCE_ROOT = Path("code/gameflow/battle/battleactions")
MANIFEST = repo_path("manifests/extractions/battle-actions-static.json")
SCHEMA = repo_path("schemas/battle-actions-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/battle-actions-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-battle-actions-static-fixture.schema.json")
TOOLCHAIN = repo_path("manifests/toolchain.json")
RESEARCH_INDEX = repo_path("manifests/research-index.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")

BATTLE_SCENE_MACROS_PATH = Path("sf2battlescenemacros.asm")
BATTLE_SCENE_ENGINE_SOURCE_ROOT = battle_scene_engine.SOURCE_ROOT
ENUMS_PATH = Path("sf2enums.asm")
TEXT_LINES_PATH = Path("data/scripting/text/gamescript.txt")
H1_LIST_PATH = Path("build/sf2build-h1.lst")
ITEM_BREAK_MESSAGES_PATH = Path("data/stats/items/itembreakmessages.asm")
MACROS_PATH = Path("sf2macros.asm")
ITEM_AUXILIARY_FIXTURE = repo_path("tests/fixtures/h2/item-auxiliary-static-v1.json")
ITEM_AUXILIARY_FIXTURE_SCHEMA = repo_path("schemas/h2-item-auxiliary-static-fixture.schema.json")
ITEM_BREAK_CONSUMER_RULE = "matched item byte adds its offset to the selected base message"

MESSAGE_MACRO_NAMES = ("displayMessage", "displayMessageWithNoWait")
MESSAGE_DISPATCH_HANDLERS = {
    "displayMessage": "bsc10_displayMessage",
    "displayMessageWithNoWait": "bsc11_displayMessageWithNoWait",
}
RUNTIME_MESSAGE_OUTPUT_SLOTS = [
    "command",
    "message",
    "combatant",
    "item-or-spell",
    "reserved-zero",
    "number",
]

GLOBAL_LABEL_PATTERN = re.compile(r"^([A-Za-z_][A-Za-z0-9_]*):")
MESSAGE_USE_PATTERN = re.compile(
    r"^\s*(displayMessageWithNoWait|displayMessage)\b\s+(.+?)\s*$"
)
H1_MESSAGE_USE_PATTERN = re.compile(
    r"^(?P<address>[0-9A-F]{8})\s+"
    r"(?P<macro>displayMessageWithNoWait|displayMessage)\b\s+"
    r"(?P<operands>.+?)\s*$"
)
H1_MACRO_EXPANSION_PATTERN = re.compile(r"\sM\s+(?P<source>.+)$")
H1_MACRO_INSTRUCTION_PATTERN = re.compile(
    r"^(?P<address>[0-9A-F]{8})\s+"
    r"(?P<bytes>(?:[0-9A-F]{4}(?:\s+|$))+?)\s+M\s+"
)
MESSAGE_EQUATE_PATTERN = re.compile(
    r"^(MESSAGE_[A-Z0-9_]+):[ \t]+equ[ \t]+"
    r"(\$[0-9A-Fa-f]+|-?\d+)(?:[ \t]*;.*)?\r?$",
    re.MULTILINE,
)
NUMERIC_EQUATE_PATTERN = re.compile(
    r"^([A-Z][A-Z0-9_]*):[ \t]+equ[ \t]+(\$[0-9A-Fa-f]+|-?\d+)"
    r"(?:[ \t]*;.*)?\r?$",
    re.MULTILINE,
)
MESSAGES_MAX_INDEX_PATTERN = re.compile(
    r"^MESSAGES_MAX_INDEX:[ \t]+equ[ \t]+"
    r"(\$[0-9A-Fa-f]+|-?\d+)(?:[ \t]*;.*)?\r?$",
    re.MULTILINE,
)
SOURCE_H1_RANGE_PATTERN = re.compile(
    r"^\s*;\s*0x([0-9A-Fa-f]+)\.\.0x([0-9A-Fa-f]+)\s*:",
    re.MULTILINE,
)
MESSAGE_COMMAND_EMISSION_PATTERN = re.compile(r"^move\.w #\$(?P<word>[0-9a-f]+),\(a6\)\+$")


REPRESENTATIVE_SYMBOLS = {
    "animateaction.asm": "battlesceneScript_AnimateAction",
    "attack.asm": "battlesceneScript_Attack",
    "battleactionsengine_1.asm": "WriteBattlesceneScript",
    "battleactionsengine_2.asm": "battlesceneScript_End",
    "breakuseditem.asm": "battlesceneScript_BreakUsedItem",
    "calculatedamage.asm": "battlesceneScript_CalculateDamage",
    "calculatespelldamage.asm": "battlesceneScript_CalculateSpellDamage",
    "castspell.asm": "battlesceneScript_CastSpell",
    "createbattlesceneanimation.asm": "battlesceneScript_PerformAnimation",
    "createbattlescenemessage.asm": "battlesceneScript_DisplayActionMessage",
    "determinecriticalhit.asm": "battlesceneScript_DetermineCriticalHit",
    "determinedodge.asm": "battlesceneScript_DetermineDodge",
    "determinedoubleandcounter.asm": "battlesceneScript_DetermineDoubleAndCounter",
    "determineineffectiveattack.asm": "battlesceneScript_DetermineIneffectiveAttack",
    "displaydeathmessage.asm": "battlesceneScript_DisplayDeathMessage",
    "dropenemyitem.asm": "battlesceneScript_DropEnemyItem",
    "earnexp.asm": "battlesceneScript_CalculateHealingExp",
    "getresistancetospell.asm": "GetResistanceToSpell",
    "getspellanimation.asm": "battlesceneScript_GetSpellanimation",
    "giveexpandgold.asm": "battlesceneScript_GiveExpAndGold",
    "inflictailment.asm": "battlesceneScript_InflictAilment",
    "inflictcursedamage.asm": "battlesceneScript_InflictCurseDamage",
    "inflictdamage.asm": "battlesceneScript_InflictDamage",
    "initbattlesceneproperties.asm": "battlesceneScript_InitializeBattlesceneProperties",
    "isabletocounterattack.asm": "battlesceneScript_ValidateCounterAttack",
    "nullsub_BBE4.asm": "nullsub_BBE4",
    "sorttargets.asm": "battlesceneScript_SortTargets",
    "unused_battleactions.asm": "OneSecondSleep",
    "useitem.asm": "battlesceneScript_UseItem",
}


def _build_action_facts(disasm: Path) -> dict[str, Any]:
    root = disasm / SOURCE_ROOT
    _require_ordered_fragments(
        root / "battleactionsengine_1.asm",
        [
            "move.w  d1,((BATTLESCENE_EXP-$1000000)).w",
            "move.w  d1,((BATTLESCENE_GOLD-$1000000)).w",
            "move.w  d1,((BATTLESCENE_ATTACK_TYPE-$1000000)).w",
            "bsr.w   battlesceneScript_DetermineTargetsByAction",
            "bsr.w   battlesceneScript_InitializeBattlesceneProperties",
            "bsr.w   battlesceneScript_DetermineIneffectiveAttack",
            "bsr.w   battlesceneScript_InitializeActors",
            "bsr.w   battlesceneScript_DisplayActionMessage",
            "bsr.w   battlesceneScript_PerformAnimation",
            "bsr.w   battlesceneScript_ApplyActionEffect",
            "bsr.w   battlesceneScript_DropEnemyItem",
            "bsr.w   battlesceneScript_BreakUsedItem",
            "bsr.w   battlesceneScript_ValidateDoubleAttack",
            "bsr.w   battlesceneScript_ValidateCounterAttack",
            "bsr.w   battlesceneScript_End",
        ],
    )
    _require_ordered_fragments(
        root / "battleactionsengine_1.asm",
        [
            "cmpi.w  #BATTLEACTION_ATTACK,(a3)",
            "cmpi.w  #BATTLEACTION_CAST_SPELL,(a3)",
            "cmpi.w  #BATTLEACTION_USE_ITEM,(a3)",
            "cmpi.w  #BATTLEACTION_BURST_ROCK,(a3)",
            "cmpi.w  #BATTLEACTION_MUDDLED,(a3)",
            "cmpi.w  #BATTLEACTION_PRISM_LASER,(a3)",
            "bsr.w   battlesceneScript_SortTargets",
        ],
    )
    _require_ordered_fragments(
        root / "attack.asm",
        [
            "bsr.w   battlesceneScript_DetermineDodge",
            "bsr.w   battlesceneScript_CalculateDamage",
            "bsr.w   battlesceneScript_DetermineCriticalHit",
            "bsr.w   battlesceneScript_InflictDamage",
            "bsr.w   battlesceneScript_InflictAilment",
            "bsr.w   battlesceneScript_InflictCurseDamage",
            "bsr.w   battlesceneScript_DetermineDoubleAndCounter",
        ],
    )
    _require_ordered_fragments(
        root / "breakuseditem.asm",
        [
            "cmpi.w  #BATTLEACTION_USE_ITEM,(a3)",
            "jsr     GetEquipmentType",
            "beq.w   @RemoveItem",
            "btst    #ITEMTYPE_BIT_BREAKABLE,ITEMDEF_OFFSET_TYPE(a0)",
            "btst    #COMBATANT_BIT_ENEMY,(a4)",
            "btst    #ITEMENTRY_BIT_BROKEN,d0",
            "moveq   #CHANCE_TO_BREAK_USED_ITEM,d0",
            "jsr     (GenerateRandomOrDebugNumber).w",
            "jsr     BreakItemBySlot",
            "jsr     RemoveItemBySlot",
        ],
    )
    _require_ordered_fragments(
        root / "useitem.asm",
        [
            "move.b  ITEMDEF_OFFSET_USE_SPELL(a0),d0",
            "andi.w  #SPELLENTRY_MASK_INDEX,d0",
            "lsr.b   #SPELLENTRY_OFFSET_LV,d0",
            "bra.w   battlesceneScript_CastSpell",
        ],
    )
    _require_ordered_fragments(
        root / "determineineffectiveattack.asm",
        [
            "cmpi.b  #BATTLE_VERSUS_TAROS,((CURRENT_BATTLE-$1000000)).w",
            "cmpi.w  #BATTLEACTION_ATTACK,(a3)",
            "cmpi.w  #ENEMY_TAROS,d1",
            "cmpi.w  #ITEM_ACHILLES_SWORD,d1",
            "move.b  #-1,ineffectiveAttackToggle(a2)",
        ],
    )
    _require_ordered_fragments(
        root / "sorttargets.asm",
        [
            "cmpi.w  #ENEMY_BURST_ROCK,d1",
            "ori.b   #COMBATANT_MASK_SORT_BIT,d0",
            "cmp.b   (a0,d1.w),d2",
            "jsr     GetCurrentHp",
            "andi.b  #COMBATANT_MASK_INDEX_AND_ENEMY_BIT,(a0,d7.w)",
        ],
    )
    null_source = (root / "nullsub_BBE4.asm").read_text(encoding="utf-8")
    unused_source = (root / "unused_battleactions.asm").read_text(encoding="utf-8")
    if "nullsub_BBE4:" not in null_source or "OneSecondSleep:" not in unused_source:
        raise ValueError("battle action unused/null helper drift")
    return {
        "engine": {
            "initialZeroedAccumulators": ["exp", "gold", "attack-type"],
            "targetActions": [
                "attack",
                "cast-spell",
                "use-item",
                "burst-rock",
                "muddled",
                "prism-laser",
            ],
            "sortsTargetsAfterConstruction": True,
            "perTargetOrder": ["switch-targets", "apply-effect", "drop-enemy-item"],
            "postTargetsOrder": [
                "actor-idle",
                "break-used-item",
                "validate-double",
                "validate-counter",
                "explode",
                "end",
            ],
            "burstRockExplosionReentersTargetAndActionSetup": True,
        },
        "physicalAttack": {
            "order": [
                "dodge",
                "damage",
                "critical",
                "inflict-damage",
                "ailment",
                "curse-damage",
                "double-counter",
            ],
            "dodgeSkipsDamageCriticalAilmentAndCurse": True,
            "directLethalSkipsAilmentCurseAndFollowups": True,
            "curseLethalSkipsFollowups": True,
        },
        "items": {
            "useItemDelegatesToPackedSpell": True,
            "nonEquipmentConsumedUnconditionally": True,
            "equipmentMustBeBreakableAndAllyUsed": True,
            "alreadyBrokenEquipmentIsDestroyed": True,
            "freshBreakableEquipmentUsesRng": True,
            "breakRngSuccessValue": 0,
        },
        "taros": {
            "battleSpecific": True,
            "allyPhysicalAttackOnly": True,
            "targetEnemy": "Taros",
            "effectiveWeapon": "Achilles Sword",
            "ineffectiveToggleOtherwise": True,
            "transientFlag": 112,
        },
        "targetSort": {
            "primaryOrder": "unsigned combatant byte ascending",
            "burstRockSortBitPlacesAfterOrdinaryTargets": True,
            "burstRockSecondaryOrder": "higher HP before lower HP",
            "sortBitClearedBeforeReturn": True,
        },
        "unused": {
            "nullsubTracked": True,
            "sleepAndNopHelpersTracked": True,
            "notClaimedReachable": True,
        },
    }


def _source_line_without_comment(line: str) -> str:
    return line.split(";", 1)[0].rstrip()


def _normalized_asm(line: str) -> str:
    return re.sub(r"\s+", " ", _source_line_without_comment(line).strip()).lower()


def _integer(expression: str) -> int:
    return int(expression[1:], 16) if expression.startswith("$") else int(expression, 10)


def _macro_block(source: str, name: str) -> tuple[int, list[str]]:
    match = re.search(
        rf"^{re.escape(name)}:\s*macro\s*$"
        rf"(?P<body>.*?)^\s*endm\s*$",
        source,
        re.MULTILINE | re.DOTALL,
    )
    if not match:
        raise ValueError(f"battle message macro definition is missing: {name}")
    line_number = source[: match.start()].count("\n") + 1
    return line_number, [
        _normalized_asm(line)
        for line in match.group("body").splitlines()
        if _normalized_asm(line)
    ]


def _parse_write_bsc_param_definition(disasm: Path) -> dict[str, Any]:
    source_path = disasm / BATTLE_SCENE_MACROS_PATH
    source = decode_upstream_text(source_path.read_bytes())
    line_number, body = _macro_block(source, "writeBscParam")
    expected = [
        "if (instr('\\1','(a')=0)",
        "move.w \\1,(a6)+",
        "else",
        "move.b #0,(a6)+",
        "move.b \\1,(a6)+",
        "endc",
    ]
    if body != expected:
        raise ValueError("writeBscParam conditional emission drift")
    return {
        "sourceMacro": "writeBscParam",
        "sourcePath": BATTLE_SCENE_MACROS_PATH.as_posix(),
        "definitionSourceLine": line_number,
        "formalParameterCount": 1,
        "addressRegisterCondition": "instr('\\1','(a')=0",
        "ordinaryEmissionTemplates": ["move.w \\1,(a6)+"],
        "addressRegisterEmissionTemplates": ["move.b #0,(a6)+", "move.b \\1,(a6)+"],
        "runtimeOutputWordCount": 1,
        "runtimeOutputByteCount": 2,
    }


def _parse_message_macro_definitions(disasm: Path) -> list[dict[str, Any]]:
    source_path = disasm / BATTLE_SCENE_MACROS_PATH
    source = decode_upstream_text(source_path.read_bytes())
    definitions: list[dict[str, Any]] = []
    for macro in MESSAGE_MACRO_NAMES:
        line_number, body = _macro_block(source, macro)
        command_match = MESSAGE_COMMAND_EMISSION_PATTERN.fullmatch(body[0]) if body else None
        if command_match is None:
            raise ValueError(f"battle message macro command emission drift: {macro}")
        command_word = int(command_match.group("word"), 16)
        if not 0 <= command_word <= 0xFFFF:
            raise ValueError(f"battle message macro command word is outside a word: {macro}")
        expected_tail = [
            "writebscparam \\1",
            "writebscparam \\2",
            "writebscparam \\3",
            "move.w #0,(a6)+",
            "writebscparam \\4",
        ]
        if body[1:] != expected_tail:
            raise ValueError(f"battle message macro emission drift: {macro}")
        definitions.append(
            {
                "sourceMacro": macro,
                "sourcePath": BATTLE_SCENE_MACROS_PATH.as_posix(),
                "definitionSourceLine": line_number,
                "commandWord": command_word,
                "formalParameterCount": 4,
                "runtimeOutputSlots": RUNTIME_MESSAGE_OUTPUT_SLOTS,
                "runtimeOutputWordCount": len(RUNTIME_MESSAGE_OUTPUT_SLOTS),
                "runtimeOutputByteCount": len(RUNTIME_MESSAGE_OUTPUT_SLOTS) * 2,
                "emissionStatementTemplates": body,
            }
        )
    return definitions


def _battle_scene_message_dispatch_contract(
    disasm: Path, *, upstream_commit: str, command_words: dict[str, int]
) -> dict[str, Any]:
    """Join parsed macro command words to the source-built scene dispatcher."""
    commands = battle_scene_engine._build_scene_facts(disasm)["scriptInterpreter"]["commands"]
    dispatches: list[dict[str, Any]] = []
    for macro, expected_handler in MESSAGE_DISPATCH_HANDLERS.items():
        command_word = command_words.get(macro)
        handler_positions = [
            index for index, handler in enumerate(commands) if handler == expected_handler
        ]
        if len(handler_positions) != 1:
            raise ValueError(
                "battle message dispatcher handler lookup drift: " f"{macro}"
            )
        dispatcher_slot = handler_positions[0]
        if command_word != dispatcher_slot:
            raise ValueError(
                "battle message macro command word does not match dispatcher slot: "
                f"{macro}"
            )
        dispatches.append(
            {
                "sourceMacro": macro,
                "commandWord": dispatcher_slot,
                "handler": expected_handler,
            }
        )
    return {
        "contractId": battle_scene_engine.ID,
        "upstreamCommit": upstream_commit,
        "sourcePath": (BATTLE_SCENE_ENGINE_SOURCE_ROOT / "battlesceneengine_0.asm").as_posix(),
        "macroDispatches": dispatches,
    }


def _split_message_operands(text: str, *, context: str) -> list[str]:
    operands = [operand.strip() for operand in text.split(",")]
    if len(operands) != 4 or any(not operand for operand in operands):
        raise ValueError(f"battle message use requires four operands: {context}")
    return operands


def _source_h1_range(text: str, *, relative_path: str) -> tuple[int, int]:
    matches = SOURCE_H1_RANGE_PATTERN.findall(text)
    if len(matches) != 1:
        raise ValueError(f"battle message source H1 range drift: {relative_path}")
    start, end = matches[0]
    start_value, end_value = int(start, 16), int(end, 16)
    if start_value >= end_value:
        raise ValueError(f"battle message source H1 range is empty: {relative_path}")
    return start_value, end_value


def _parse_source_message_uses(disasm: Path) -> list[dict[str, Any]]:
    source_root = disasm / SOURCE_ROOT
    rows: list[dict[str, Any]] = []
    for path in sorted(source_root.glob("*.asm"), key=lambda item: item.as_posix()):
        relative_path = path.relative_to(disasm).as_posix()
        text = decode_upstream_text(path.read_bytes())
        start, end = _source_h1_range(text, relative_path=relative_path)
        enclosing_label: str | None = None
        sites: list[dict[str, Any]] = []
        for line_number, raw_line in enumerate(text.splitlines(), start=1):
            line = _source_line_without_comment(raw_line)
            label = GLOBAL_LABEL_PATTERN.match(line)
            if label:
                enclosing_label = label.group(1)
                line = line[label.end() :]
            match = MESSAGE_USE_PATTERN.match(line)
            if not match:
                continue
            if enclosing_label is None:
                raise ValueError(
                    "battle message use has no enclosing global label: "
                    f"{relative_path}:{line_number}"
                )
            macro = match.group(1)
            operands = _split_message_operands(
                match.group(2), context=f"{relative_path}:{line_number}"
            )
            sites.append(
                {
                    "sourcePath": relative_path,
                    "sourceLine": line_number,
                    "enclosingGlobalLabel": enclosing_label,
                    "sourceMacro": macro,
                    "operandExpressions": operands,
                }
            )
        rows.append(
            {
                "sourcePath": relative_path,
                "sourceH1StartAddress": start,
                "sourceH1EndAddressExclusive": end,
                "sites": sites,
            }
        )
    return rows


def _h1_expansion_instruction_bytes(lines: list[str], *, context: str) -> int:
    emitted_lines = [
        line for line in lines if H1_MACRO_INSTRUCTION_PATTERN.match(line)
    ]
    if not emitted_lines:
        raise ValueError(f"battle message H1 expansion has no emitted instructions: {context}")
    byte_count = 0
    for line in emitted_lines:
        match = H1_MACRO_INSTRUCTION_PATTERN.match(line)
        assert match is not None
        byte_count += len(re.sub(r"\s+", "", match.group("bytes"))) // 2
    return byte_count


def _parse_h1_message_uses(
    upstream_path: Path, command_words: dict[str, int]
) -> list[dict[str, Any]]:
    h1_path = upstream_path / H1_LIST_PATH
    if not h1_path.is_file():
        raise ValueError(f"battle message H1 listing is missing: {h1_path}")
    lines = decode_upstream_text(h1_path.read_bytes()).splitlines()
    rows: list[dict[str, Any]] = []
    for index, line in enumerate(lines):
        match = H1_MESSAGE_USE_PATTERN.match(line)
        if not match:
            continue
        macro = match.group("macro")
        if macro not in command_words:
            raise ValueError(f"battle message H1 macro has no parsed command word: {macro}")
        operands = _split_message_operands(
            _source_line_without_comment(match.group("operands")),
            context=f"{H1_LIST_PATH.as_posix()}:{index + 1}",
        )
        expansion_lines: list[str] = []
        for following in lines[index + 1 :]:
            if not H1_MACRO_EXPANSION_PATTERN.search(following):
                break
            expansion_lines.append(following)
        nested_parameters: list[tuple[str, int]] = []
        for expansion_index, expansion_line in enumerate(expansion_lines):
            marker = H1_MACRO_EXPANSION_PATTERN.search(expansion_line)
            assert marker is not None
            source = marker.group("source").strip()
            if source.lower().startswith("writebscparam"):
                nested_parameters.append((source.split(None, 1)[1].strip(), expansion_index))
        if [parameter.lower() for parameter, _ in nested_parameters] != [
            parameter.lower() for parameter in operands
        ]:
            raise ValueError(f"battle message H1 writeBscParam order drift: {index + 1}")
        expected_instruction_count = 6 + sum("(a" in operand.lower() for operand in operands)
        emitted_instruction_count = sum(
            H1_MACRO_INSTRUCTION_PATTERN.match(expansion_line) is not None
            for expansion_line in expansion_lines
        )
        if emitted_instruction_count != expected_instruction_count:
            raise ValueError(f"battle message H1 instruction count drift: {index + 1}")
        for parameter, marker_index in nested_parameters:
            emitted_sources: list[str] = []
            for following in expansion_lines[marker_index + 1 :]:
                marker = H1_MACRO_EXPANSION_PATTERN.search(following)
                assert marker is not None
                source = _normalized_asm(marker.group("source"))
                if source.startswith("writebscparam"):
                    break
                if H1_MACRO_INSTRUCTION_PATTERN.match(following) is not None:
                    emitted_sources.append(source)
            if "(a" in parameter.lower():
                expected_emissions = [
                    "move.b #0,(a6)+",
                    f"move.b {parameter.lower()},(a6)+",
                ]
            else:
                expected_emissions = [f"move.w {parameter.lower()},(a6)+"]
            if emitted_sources[: len(expected_emissions)] != expected_emissions:
                raise ValueError(
                    f"battle message H1 writeBscParam branch emission drift: {index + 1}"
                )
        first_instruction = next(
            (
                expansion_line
                for expansion_line in expansion_lines
                if H1_MACRO_INSTRUCTION_PATTERN.match(expansion_line) is not None
            ),
            None,
        )
        if first_instruction is None:
            raise ValueError(f"battle message H1 command emission is missing: {index + 1}")
        first_match = H1_MACRO_INSTRUCTION_PATTERN.match(first_instruction)
        assert first_match is not None
        command_word = command_words[macro]
        expected_command_bytes = f"3CFC{command_word:04X}"
        if re.sub(r"\s+", "", first_match.group("bytes")) != expected_command_bytes:
            raise ValueError(f"battle message H1 command word drift: {index + 1}")
        rows.append(
            {
                "h1ExpansionAddress": int(match.group("address"), 16),
                "sourceMacro": macro,
                "operandExpressions": operands,
                "commandWord": command_word,
                "assembledInstructionByteCount": _h1_expansion_instruction_bytes(
                    expansion_lines, context=str(index + 1)
                ),
            }
        )
    return rows


def _parse_message_equates(disasm: Path) -> dict[str, int]:
    source = decode_upstream_text((disasm / ENUMS_PATH).read_bytes())
    values = {name: _integer(value) for name, value in MESSAGE_EQUATE_PATTERN.findall(source)}
    if not values:
        raise ValueError("battle message enum map is empty")
    return values


def _parse_message_line_domain(disasm: Path) -> dict[str, Any]:
    source = (disasm / TEXT_LINES_PATH).read_bytes()
    line_count = len(source.splitlines())
    if line_count < 1:
        raise ValueError("battle message line source is empty")
    enums = decode_upstream_text((disasm / ENUMS_PATH).read_bytes())
    match = MESSAGES_MAX_INDEX_PATTERN.search(enums)
    if not match:
        raise ValueError("battle message max line ID equate is missing")
    max_line_id = _integer(match.group(1))
    if max_line_id != line_count - 1:
        raise ValueError("battle message line source and max line ID equate drift")
    return {
        "sourcePath": TEXT_LINES_PATH.as_posix(),
        "sourceSha256": hashlib.sha256(source).hexdigest().upper(),
        "lineIdCount": line_count,
        "firstLineId": 0,
        "lastLineId": line_count - 1,
        "maxLineIdEquate": "MESSAGES_MAX_INDEX",
        "maxLineIdValue": max_line_id,
        "idsAreContiguous": True,
    }


def _message_operand_fact(
    expression: str, *, enums: dict[str, int], line_domain: dict[str, Any]
) -> dict[str, Any]:
    if expression.startswith("#MESSAGE_"):
        symbol = expression[1:]
        if not re.fullmatch(r"MESSAGE_[A-Z0-9_]+", symbol) or symbol not in enums:
            raise ValueError(f"battle message immediate enum is unresolved: {expression}")
        line_id = enums[symbol]
        if not line_domain["firstLineId"] <= line_id <= line_domain["lastLineId"]:
            raise ValueError(f"battle message immediate enum is outside text line domain: {symbol}")
        return {
            "kind": "immediate-message-enum",
            "messageSymbol": symbol,
            "lineId": line_id,
        }
    return {
        "kind": "dynamic-expression",
        "messageSymbol": None,
        "lineId": None,
    }


def _parse_numeric_equates(disasm: Path) -> dict[str, int]:
    """Return the directly numeric enum map used by the static message grammar."""
    source = decode_upstream_text((disasm / ENUMS_PATH).read_bytes())
    values = {name: _integer(value) for name, value in NUMERIC_EQUATE_PATTERN.findall(source)}
    if not values:
        raise ValueError("battle message numeric enum map is empty")
    return values


def _message_candidate(
    symbol: str | None, line_id: int, *, enums: dict[str, int], line_domain: dict[str, Any]
) -> dict[str, Any]:
    if not line_domain["firstLineId"] <= line_id <= line_domain["lastLineId"]:
        raise ValueError(f"battle message candidate is outside text line domain: {line_id}")
    if symbol is not None and enums.get(symbol) != line_id:
        raise ValueError(f"battle message candidate enum drift: {symbol}")
    return {"messageSymbol": symbol, "lineId": line_id}


def _symbol_candidate(
    symbol: str, *, enums: dict[str, int], line_domain: dict[str, Any]
) -> dict[str, Any]:
    if symbol not in enums:
        raise ValueError(f"battle message candidate enum is unresolved: {symbol}")
    return _message_candidate(symbol, enums[symbol], enums=enums, line_domain=line_domain)


def _control_fact(
    kind: str,
    *,
    symbol: str | None = None,
    value: int | None = None,
    branch_mnemonic: str | None = None,
    branch_target: str | None = None,
    message: dict[str, Any] | None = None,
) -> dict[str, Any]:
    return {
        "kind": kind,
        "symbol": symbol,
        "value": value,
        "branchMnemonic": branch_mnemonic,
        "branchTarget": branch_target,
        "messageSymbol": None if message is None else message["messageSymbol"],
        "messageLineId": None if message is None else message["lineId"],
    }


def _normalized_source_statements(path: Path) -> list[str]:
    return [
        _normalized_asm(_source_line_without_comment(line))
        for line in decode_upstream_text(path.read_bytes()).splitlines()
        if _normalized_asm(_source_line_without_comment(line))
    ]


def _require_exact_statement_section(
    path: Path, expected: list[str], *, context: str
) -> None:
    """Require one smallest, contiguous source grammar after removing layout/comments."""
    statements = _normalized_source_statements(path)
    normalized_expected = [_normalized_asm(statement) for statement in expected]
    matching_sections = [
        index
        for index, statement in enumerate(statements)
        if statement == normalized_expected[0]
        and statements[index : index + len(normalized_expected)] == normalized_expected
    ]
    if len(matching_sections) != 1:
        raise ValueError(
            f"battle message {context} source grammar drift: {path.as_posix()}"
        )


def _require_complete_statement_file(
    path: Path, expected: list[str], *, context: str
) -> None:
    """Require a complete, layout-free source file with no unowned statements."""
    if _normalized_source_statements(path) != [
        _normalized_asm(statement) for statement in expected
    ]:
        raise ValueError(
            f"battle message {context} source grammar drift: {path.as_posix()}"
        )


def _parse_h1_data_range(
    upstream_path: Path, *, start: int, end: int, context: str
) -> bytes:
    """Read a contiguous byte range from explicit H1 data emissions only."""
    h1_path = upstream_path / H1_LIST_PATH
    if not h1_path.is_file():
        raise ValueError(f"battle message H1 listing is missing: {h1_path}")
    emitted = bytearray()
    cursor = start
    pattern = re.compile(
        r"^(?P<address>[0-9A-F]{8})\s+"
        r"(?P<bytes>[0-9A-F]{2,4}(?:\s+[0-9A-F]{2,4})*)\s+M\s+"
    )
    for line in decode_upstream_text(h1_path.read_bytes()).splitlines():
        match = pattern.match(line)
        if match is None:
            continue
        address = int(match.group("address"), 16)
        compact = re.sub(r"\s+", "", match.group("bytes"))
        if len(compact) % 2:
            raise ValueError(f"battle message {context} H1 byte syntax drift")
        data = bytes.fromhex(compact)
        if not (start <= address < end):
            continue
        if address != cursor or address + len(data) > end:
            raise ValueError(f"battle message {context} H1 range drift")
        emitted.extend(data)
        cursor += len(data)
    if cursor != end:
        raise ValueError(f"battle message {context} H1 range is incomplete")
    return bytes(emitted)


def _static_resolution(
    resolver: str,
    register: str,
    candidate_messages: list[dict[str, Any]],
    control_facts: list[dict[str, Any]],
) -> dict[str, Any]:
    if not candidate_messages:
        raise ValueError(f"battle message resolver has no candidates: {resolver}")
    return {
        "resolver": resolver,
        "register": register,
        "candidateMessages": candidate_messages,
        "controlFacts": control_facts,
    }


def _parse_item_break_message_resolver(
    disasm: Path,
    *,
    upstream_path: Path,
    numeric_enums: dict[str, int],
    message_enums: dict[str, int],
    line_domain: dict[str, Any],
) -> dict[str, Any]:
    macro_path = disasm / MACROS_PATH
    _require_exact_statement_section(
        macro_path,
        [
            "itemBreakMessage: macro",
            "defineShorthand.b ITEM_,\\1",
            "defineShorthand.b ITEMBREAK_\\2",
            "endm",
        ],
        context="item-break macro",
    )
    table_path = disasm / ITEM_BREAK_MESSAGES_PATH
    table_source = decode_upstream_text(table_path.read_bytes())
    table_h1_start, table_h1_end = _source_h1_range(
        table_source, relative_path=ITEM_BREAK_MESSAGES_PATH.as_posix()
    )
    table_lines = [
        _source_line_without_comment(line).strip()
        for line in table_source.splitlines()
    ]
    rows: list[tuple[str, str]] = []
    for line in table_lines:
        match = re.fullmatch(r"itemBreakMessage\s+([A-Z0-9_]+)\s*,\s*([A-Z0-9_]+)", line)
        if match:
            rows.append(match.groups())
    if len(rows) != 25:
        raise ValueError("battle message item-break table row count drift")
    _require_complete_statement_file(
        table_path,
        [
            "table_ItemBreakMessages:",
            *(f"itemBreakMessage {item}, {offset}" for item, offset in rows),
            "tableEnd.w",
        ],
        context="item-break table",
    )
    required_symbols = {
        "ITEMENTRY_MASK_INDEX",
        "ITEMENTRY_SIZE",
        *(f"ITEM_{item}" for item, _ in rows),
        *(f"ITEMBREAK_{offset}" for _, offset in rows),
    }
    if not required_symbols <= numeric_enums.keys():
        missing = sorted(required_symbols - numeric_enums.keys())
        raise ValueError(f"battle message item-break enum is unresolved: {missing}")
    table_rows = [
        {
            "itemSymbol": f"ITEM_{item}",
            "itemId": numeric_enums[f"ITEM_{item}"],
            "messageOffsetSymbol": f"ITEMBREAK_{offset}",
            "messageOffset": numeric_enums[f"ITEMBREAK_{offset}"],
        }
        for item, offset in rows
    ]
    if sorted({row["messageOffset"] for row in table_rows}) != [0, 1, 2, 3, 4]:
        raise ValueError("battle message item-break table offset domain drift")
    encoded_table = bytes(
        component
        for row in table_rows
        for component in (row["itemId"], row["messageOffset"])
    ) + b"\xff\xff"
    h1_table = _parse_h1_data_range(
        upstream_path,
        start=table_h1_start,
        end=table_h1_end,
        context="item-break table",
    )
    if encoded_table != h1_table:
        raise ValueError("battle message item-break table H1 byte parity drift")
    helper_path = disasm / SOURCE_ROOT / "breakuseditem.asm"
    _require_exact_statement_section(
        helper_path,
        [
            "battlesceneScript_GetItemBreakMessage:",
            "movem.l d0/a0,-(sp)",
            "tst.b d0",
            "bne.s @Destroy",
            "tst.b dodge(a2)",
            "bne.s @BreakAndMiss",
            "move.w #MESSAGE_BATTLE_USED_ITEM_HIT_AND_BROKEN_START,d3",
            "bra.s @Goto_FindItem",
            "@BreakAndMiss:",
            "move.w #MESSAGE_BATTLE_USED_ITEM_MISS_AND_BROKEN_START,d3",
            "@Goto_FindItem:",
            "bra.s @FindItem",
            "@Destroy:",
            "tst.b dodge(a2)",
            "bne.s @DestroyAndMiss",
            "move.w #MESSAGE_BATTLE_USED_ITEM_HIT_AND_DESTROYED_START,d3",
            "bra.s @FindItem",
            "@DestroyAndMiss:",
            "move.w #MESSAGE_BATTLE_USED_ITEM_MISS_AND_DESTROYED_START,d3",
            "@FindItem:",
            "move.w ((BATTLESCENE_ITEM-$1000000)).w,d0",
            "andi.w #ITEMENTRY_MASK_INDEX,d0",
            "lea table_ItemBreakMessages(pc), a0",
            "@FindItem_Loop:",
            "cmpi.w #-1,(a0)",
            "beq.w @Done",
            "cmp.b (a0),d0",
            "beq.w @Found",
            "addq.l #ITEMENTRY_SIZE,a0",
            "bra.s @FindItem_Loop",
            "@Found:",
            "moveq #0,d0",
            "move.b 1(a0),d0",
            "add.w d0,d3",
            "@Done:",
            "movem.l (sp)+,d0/a0",
            "rts",
        ],
        context="item-break helper",
    )
    base_symbols = (
        "MESSAGE_BATTLE_USED_ITEM_HIT_AND_BROKEN_START",
        "MESSAGE_BATTLE_USED_ITEM_MISS_AND_BROKEN_START",
        "MESSAGE_BATTLE_USED_ITEM_HIT_AND_DESTROYED_START",
        "MESSAGE_BATTLE_USED_ITEM_MISS_AND_DESTROYED_START",
    )
    bases = [
        _symbol_candidate(symbol, enums=message_enums, line_domain=line_domain)
        for symbol in base_symbols
    ]
    return {
        "sourcePath": ITEM_BREAK_MESSAGES_PATH.as_posix(),
        "sourceH1StartAddress": table_h1_start,
        "sourceH1EndAddressExclusive": table_h1_end,
        "h1ListingPath": H1_LIST_PATH.as_posix(),
        "h1ByteCount": len(h1_table),
        "h1Sha256": hashlib.sha256(h1_table).hexdigest().upper(),
        "macroSourcePath": MACROS_PATH.as_posix(),
        "tableSymbol": "table_ItemBreakMessages",
        "sourceMacro": "itemBreakMessage",
        "macroFormalParameterCount": 2,
        "itemPrefix": "ITEM_",
        "messageOffsetPrefix": "ITEMBREAK_",
        "tableRows": table_rows,
        "tableSentinelValue": -1,
        "itemIndexMaskSymbol": "ITEMENTRY_MASK_INDEX",
        "itemIndexMaskValue": numeric_enums["ITEMENTRY_MASK_INDEX"],
        "entrySizeSymbol": "ITEMENTRY_SIZE",
        "entrySizeValue": numeric_enums["ITEMENTRY_SIZE"],
        "baseMessages": bases,
    }


def _build_dynamic_message_resolutions(
    disasm: Path,
    *,
    upstream_path: Path,
    enums: dict[str, int],
    line_domain: dict[str, Any],
) -> tuple[dict[str, list[dict[str, Any]]], dict[str, Any]]:
    """Resolve every dynamic display operand using only its bounded source grammar."""
    numeric_enums = _parse_numeric_equates(disasm)
    root = disasm / SOURCE_ROOT

    def symbol(name: str) -> dict[str, Any]:
        return _symbol_candidate(name, enums=enums, line_domain=line_domain)

    def value(name: str) -> int:
        if name not in numeric_enums:
            raise ValueError(f"battle message selector enum is unresolved: {name}")
        return numeric_enums[name]

    create_path = root / "createbattlescenemessage.asm"
    _require_exact_statement_section(
        create_path,
        [
            "move.w ((BATTLESCENE_ATTACK_TYPE-$1000000)).w,d2",
            "move.w #MESSAGE_BATTLE_ATTACK,d1",
            "tst.w d2",
            "beq.w @Message_Attack",
            "move.w #MESSAGE_BATTLE_SECOND_ATTACK,d1",
            "cmpi.w #BATTLEACTION_ATTACKTYPE_SECOND,d2",
            "beq.w @Message_Attack",
            "move.w #MESSAGE_BATTLE_COUNTER_ATTACK,d1",
            "@Message_Attack:",
            "displayMessage d1,d0,#0,#0",
        ],
        context="attack message selector",
    )
    attack_candidates = [
        symbol("MESSAGE_BATTLE_ATTACK"),
        symbol("MESSAGE_BATTLE_SECOND_ATTACK"),
        symbol("MESSAGE_BATTLE_COUNTER_ATTACK"),
    ]
    attack_resolution = _static_resolution(
        "attack-type-chain",
        "d1",
        attack_candidates,
        [
            _control_fact("selector-register", symbol="BATTLESCENE_ATTACK_TYPE"),
            _control_fact("default-assignment", message=attack_candidates[0]),
            _control_fact("zero-branch", branch_mnemonic="beq", branch_target="@Message_Attack"),
            _control_fact("override-assignment", message=attack_candidates[1]),
            _control_fact(
                "selector-branch",
                symbol="BATTLEACTION_ATTACKTYPE_SECOND",
                value=value("BATTLEACTION_ATTACKTYPE_SECOND"),
                branch_mnemonic="beq",
                branch_target="@Message_Attack",
            ),
            _control_fact("final-override-assignment", message=attack_candidates[2]),
        ],
    )

    _require_exact_statement_section(
        create_path,
        [
            "move.w ((BATTLESCENE_SPELL_INDEX-$1000000)).w,d2",
            "move.w #MESSAGE_SPELLCAST_PUT_ON_A_DEMON_SMILE,d1",
            "cmpi.w #SPELL_SPOIT,d2",
            "beq.w @Message_CastSpell",
            "move.w #MESSAGE_SPELLCAST_BELCHED_OUT_FLAMES,d1",
            "cmpi.w #SPELL_FLAME,d2",
            "beq.w @Message_CastSpell",
            "cmpi.w #SPELL_KIWI,d2",
            "beq.w @Message_CastSpell",
            "move.w #MESSAGE_SPELLCAST_BLEW_OUT_A_SNOWSTORM,d1",
            "cmpi.w #SPELL_SNOW,d2",
            "beq.w @Message_CastSpell",
            "move.w #MESSAGE_SPELLCAST_CAST_DEMON_BREATH,d1",
            "cmpi.w #SPELL_DEMON,d2",
            "beq.w @Message_CastSpell",
            "move.w #MESSAGE_SPELLCAST_ODD_EYE_BEAM,d1",
            "cmpi.w #SPELL_ODDEYE,d2",
            "beq.w @Message_CastSpell",
            "move.w #MESSAGE_SPELLCAST_SUMMONED,d1",
            "cmpi.w #SPELL_DAO,d2",
            "beq.w @Message_CastSpell",
            "cmpi.w #SPELL_APOLLO,d2",
            "beq.w @Message_CastSpell",
            "cmpi.w #SPELL_NEPTUN,d2",
            "beq.w @Message_CastSpell",
            "cmpi.w #SPELL_ATLAS,d2",
            "beq.w @Message_CastSpell",
            "move.w BATTLEACTION_OFFSET_ITEM_OR_SPELL(a3),d2",
            "move.w #MESSAGE_SPELLCAST_BLEW_OUT_AQUA_BREATH,d1",
            "cmpi.w #SPELL_AQUA,d2",
            "beq.w @Message_CastSpell",
            "move.w #MESSAGE_SPELLCAST_BLEW_OUT_BUBBLE_BREATH,d1",
            "cmpi.w #SPELL_AQUA|SPELL_LV2,d2",
            "beq.w @Message_CastSpell",
            "move.w #MESSAGE_SPELLCAST_DEFAULT,d1",
            "@Message_CastSpell:",
            "move.w ((BATTLESCENE_SPELL_INDEX-$1000000)).w,d2",
            "move.w ((BATTLESCENE_SPELL_LEVEL-$1000000)).w,d3",
            "addq.w #1,d3",
            "displayMessage d1,d0,d2,d3",
        ],
        context="cast message selector",
    )
    cast_symbols = (
        "MESSAGE_SPELLCAST_PUT_ON_A_DEMON_SMILE",
        "MESSAGE_SPELLCAST_BELCHED_OUT_FLAMES",
        "MESSAGE_SPELLCAST_BLEW_OUT_A_SNOWSTORM",
        "MESSAGE_SPELLCAST_CAST_DEMON_BREATH",
        "MESSAGE_SPELLCAST_ODD_EYE_BEAM",
        "MESSAGE_SPELLCAST_SUMMONED",
        "MESSAGE_SPELLCAST_BLEW_OUT_AQUA_BREATH",
        "MESSAGE_SPELLCAST_BLEW_OUT_BUBBLE_BREATH",
        "MESSAGE_SPELLCAST_DEFAULT",
    )
    cast_candidates = [symbol(name) for name in cast_symbols]
    cast_selector_names = (
        "SPELL_SPOIT",
        "SPELL_FLAME",
        "SPELL_KIWI",
        "SPELL_SNOW",
        "SPELL_DEMON",
        "SPELL_ODDEYE",
        "SPELL_DAO",
        "SPELL_APOLLO",
        "SPELL_NEPTUN",
        "SPELL_ATLAS",
    )
    cast_message_indexes = (0, 1, 1, 2, 3, 4, 5, 5, 5, 5)
    cast_facts = [_control_fact("selector-register", symbol="BATTLESCENE_SPELL_INDEX")]
    for selector, message_index in zip(
        cast_selector_names, cast_message_indexes, strict=True
    ):
        cast_facts.append(
            _control_fact(
                "selector-branch",
                symbol=selector,
                value=value(selector),
                branch_mnemonic="beq",
                branch_target="@Message_CastSpell",
                message=cast_candidates[message_index],
            )
        )
    cast_facts.extend(
        [
            _control_fact("selector-register", symbol="BATTLEACTION_OFFSET_ITEM_OR_SPELL"),
            _control_fact(
                "selector-branch",
                symbol="SPELL_AQUA",
                value=value("SPELL_AQUA"),
                branch_mnemonic="beq",
                branch_target="@Message_CastSpell",
                message=cast_candidates[6],
            ),
            _control_fact(
                "selector-branch",
                symbol="SPELL_AQUA|SPELL_LV2",
                value=value("SPELL_AQUA") | value("SPELL_LV2"),
                branch_mnemonic="beq",
                branch_target="@Message_CastSpell",
                message=cast_candidates[7],
            ),
            _control_fact("final-default-assignment", message=cast_candidates[-1]),
        ]
    )
    cast_resolution = _static_resolution("spell-selector-chain", "d1", cast_candidates, cast_facts)

    _require_exact_statement_section(
        create_path,
        [
            "move.w d0,d2",
            "move.w #MESSAGE_BATTLE_MUDDLED_ACTIONS_START,d1",
            "moveq #16,d0",
            "jsr (GenerateRandomOrDebugNumber).w",
            "cmpi.w #9,d0",
            "bls.s @Message_Muddled",
            "clr.w d0",
            "@Message_Muddled:",
            "add.w d0,d1",
            "move.w d2,d0",
            "displayMessage d1,d0,#0,#0",
        ],
        context="muddled message offset",
    )
    muddled_base = symbol("MESSAGE_BATTLE_MUDDLED_ACTIONS_START")
    muddled_candidates = [muddled_base] + [
        _message_candidate(
            None,
            muddled_base["lineId"] + offset,
            enums=enums,
            line_domain=line_domain,
        )
        for offset in range(1, 10)
    ]
    muddled_resolution = _static_resolution(
        "muddled-message-offset",
        "d1",
        muddled_candidates,
        [
            _control_fact("base-assignment", message=muddled_base),
            _control_fact("random-bound", value=16),
            _control_fact(
                "clamp-branch", value=9, branch_mnemonic="bls", branch_target="@Message_Muddled"
            ),
            _control_fact("fallback-offset", value=0),
            _control_fact("add-offset", symbol="d0"),
        ],
    )

    _require_exact_statement_section(
        create_path,
        [
            "jsr GetEnemy",
            "cmpi.w #ENEMY_ZEON_GUARD,d1",
            "bne.s @PrismFlower",
            "move.w #MESSAGE_BATTLE_DEMON_LASER,d1",
            "bra.s byte_A1E6",
            "@PrismFlower:",
            "move.w #MESSAGE_BATTLE_PRISM_LASER,d1",
            "byte_A1E6:",
            "@Message_PrismLaser:",
            "displayMessage d1,d0,#0,#0",
        ],
        context="prism message selector",
    )
    prism_candidates = [symbol("MESSAGE_BATTLE_DEMON_LASER"), symbol("MESSAGE_BATTLE_PRISM_LASER")]
    prism_resolution = _static_resolution(
        "prism-enemy-branch",
        "d1",
        prism_candidates,
        [
            _control_fact(
                "selector-branch",
                symbol="ENEMY_ZEON_GUARD",
                value=value("ENEMY_ZEON_GUARD"),
                branch_mnemonic="bne",
                branch_target="@PrismFlower",
            ),
            _control_fact("equal-assignment", message=prism_candidates[0]),
            _control_fact("non-equal-assignment", message=prism_candidates[1]),
        ],
    )

    inflict_path = root / "inflictdamage.asm"
    _require_exact_statement_section(
        inflict_path,
        [
            "@DetermineBattleMessage:",
            "btst #COMBATANT_BIT_ENEMY,(a4)",
            "bne.s @EnemyAttacker",
            "tst.b criticalHit(a2)",
            "beq.s @RegularDamageMessageIfAllyAttacker",
            "move.w #MESSAGE_BATTLE_CRITICAL_HIT,d1",
            "bra.s @Goto_CutoffMessage",
            "@RegularDamageMessageIfAllyAttacker:",
            "move.w #MESSAGE_BATTLE_DAMAGE_ALLY,d1",
            "@Goto_CutoffMessage:",
            "bra.s @CutoffMessage",
            "@EnemyAttacker:",
            "tst.b criticalHit(a2)",
            "beq.s @RegularDamageMessageIfEnemyAttacker",
            "move.w #MESSAGE_BATTLE_HEAVY_ATTACK,d1",
            "bra.s @CutoffMessage",
            "@RegularDamageMessageIfEnemyAttacker:",
            "move.w #MESSAGE_BATTLE_DAMAGE_ENEMY,d1",
            "@CutoffMessage:",
            "tst.b cutoff(a2)",
            "beq.s byte_AE1A",
            "move.w #MESSAGE_BATTLE_CUTOFF,d1",
            "byte_AE1A:",
            "@Message:",
            "displayMessage d1,(a5),#0,d6",
        ],
        context="damage message selector",
    )
    damage_candidates = [
        symbol("MESSAGE_BATTLE_CRITICAL_HIT"),
        symbol("MESSAGE_BATTLE_DAMAGE_ALLY"),
        symbol("MESSAGE_BATTLE_HEAVY_ATTACK"),
        symbol("MESSAGE_BATTLE_DAMAGE_ENEMY"),
        symbol("MESSAGE_BATTLE_CUTOFF"),
    ]
    damage_resolution = _static_resolution(
        "damage-branch-chain",
        "d1",
        damage_candidates,
        [
            _control_fact(
                "attacker-side-bit",
                symbol="COMBATANT_BIT_ENEMY",
                value=value("COMBATANT_BIT_ENEMY"),
            ),
            _control_fact("ally-critical-assignment", message=damage_candidates[0]),
            _control_fact("ally-default-assignment", message=damage_candidates[1]),
            _control_fact("enemy-critical-assignment", message=damage_candidates[2]),
            _control_fact("enemy-default-assignment", message=damage_candidates[3]),
            _control_fact("cutoff-override-assignment", message=damage_candidates[4]),
        ],
    )

    death_path = root / "displaydeathmessage.asm"
    _require_exact_statement_section(
        death_path,
        [
            "battlesceneScript_DisplayDeathMessage:",
            "move.b (a5),d0",
            "btst #COMBATANT_BIT_ENEMY,d0",
            "bne.s @Enemy",
            "move.w #MESSAGE_BATTLE_IS_EXHAUSTED,d1",
            "bra.s @WriteBattleMessageCommand",
            "@Enemy:",
            "move.w #MESSAGE_BATTLE_WAS_DEFEATED,d1",
            "@WriteBattleMessageCommand:",
            "displayMessage d1,d0,#0,#0",
            "rts",
        ],
        context="death message selector",
    )
    death_candidates = [
        symbol("MESSAGE_BATTLE_IS_EXHAUSTED"),
        symbol("MESSAGE_BATTLE_WAS_DEFEATED"),
    ]
    death_resolution = _static_resolution(
        "death-side-branch",
        "d1",
        death_candidates,
        [
            _control_fact(
                "target-side-bit",
                symbol="COMBATANT_BIT_ENEMY",
                value=value("COMBATANT_BIT_ENEMY"),
            ),
            _control_fact("ally-assignment", message=death_candidates[0]),
            _control_fact("enemy-assignment", message=death_candidates[1]),
        ],
    )

    cast_path = root / "castspell.asm"
    _require_exact_statement_section(
        cast_path,
        [
            "spellEffect_Muddle:",
            "module",
            "move.b (a5),d0",
            "tst.w ((BATTLESCENE_SPELL_LEVEL-$1000000)).w",
            "beq.w @Muddle1",
            "addq.w #CHANCE_TO_INFLICT_MUDDLE2,d2",
            "bsr.w battlesceneScript_DetermineSpellEffectiveness",
            "jsr GetStatusEffects",
            "ori.w #STATUSEFFECT_MUDDLE2,d1",
            "ori.w #STATUSEFFECT_MUDDLE,d1",
            "move.w #MESSAGE_BATTLE_IS_IN_A_DEEP_HAZE,d2",
            "bra.w @WriteScriptCommands",
            "@Muddle1:",
            "moveq #8,d2",
            "jsr GetStatusEffects",
            "andi.w #STATUSEFFECT_MUDDLE2,d1",
            "bne.s @DetermineSuccess",
            "moveq #CHANCE_TO_INFLICT_MUDDLE1,d2",
            "@DetermineSuccess:",
            "bsr.w battlesceneScript_DetermineSpellEffectiveness",
            "jsr GetStatusEffects",
            "ori.w #STATUSEFFECT_MUDDLE,d1",
            "move.w #MESSAGE_BATTLE_IS_IN_A_DEEP_HAZE,d2",
            "@WriteScriptCommands:",
            "btst #COMBATANT_BIT_ENEMY,d0",
            "bne.s byte_B4EA",
            "executeAllyReaction #0,#0,d1,#1",
            "bra.s @BattleMessage",
            "byte_B4EA:",
            "executeEnemyReaction #0,#0,d1,#1",
            "@BattleMessage:",
            "bsr.w battlesceneScript_AddStatusEffectSpellExp",
            "displayMessage d2,d0,#0,#0",
            "rts",
        ],
        context="muddle spell message",
    )
    muddle_spell_candidate = symbol("MESSAGE_BATTLE_IS_IN_A_DEEP_HAZE")
    muddle_spell_resolution = _static_resolution(
        "muddle-spell-shared-assignment",
        "d2",
        [muddle_spell_candidate],
        [
            _control_fact("muddle2-assignment", message=muddle_spell_candidate),
            _control_fact("muddle1-assignment", message=muddle_spell_candidate),
        ],
    )
    _require_exact_statement_section(
        cast_path,
        [
            "@DetermineBattleMessage:",
            "bsr.w battlesceneScript_AddExpAndGoldForKill",
            "btst #COMBATANT_BIT_ENEMY,d0",
            "bne.s @EnemyMessage",
            "move.w #MESSAGE_BATTLE_SOUL_WAS_STOLEN_ALLY,d2",
            "bra.s byte_B562",
            "@EnemyMessage:",
            "move.w #MESSAGE_BATTLE_SOUL_WAS_STOLEN_ENEMY,d2",
            "byte_B562:",
            "displayMessage d2,d0,#0,#0",
            "move.b #-1,targetDies(a2)",
            "rts",
        ],
        context="desoul message selector",
    )
    desoul_candidates = [
        symbol("MESSAGE_BATTLE_SOUL_WAS_STOLEN_ALLY"),
        symbol("MESSAGE_BATTLE_SOUL_WAS_STOLEN_ENEMY"),
    ]
    desoul_resolution = _static_resolution(
        "desoul-side-branch",
        "d2",
        desoul_candidates,
        [
            _control_fact(
                "target-side-bit",
                symbol="COMBATANT_BIT_ENEMY",
                value=value("COMBATANT_BIT_ENEMY"),
            ),
            _control_fact("ally-assignment", message=desoul_candidates[0]),
            _control_fact("enemy-assignment", message=desoul_candidates[1]),
        ],
    )
    _require_exact_statement_section(
        cast_path,
        [
            "@DetermineMessage:",
            "bsr.w battlesceneScript_AddStatusEffectSpellExp",
            "bscHideTextBox",
            "btst #COMBATANT_BIT_ENEMY,d0",
            "bne.s @EnemyMessage",
            "move.w #MESSAGE_BATTLE_ABSORBED_MAGIC_POINTS,d1",
            "bra.s byte_B66C",
            "@EnemyMessage:",
            "move.b (a5),d0",
            "move.w #MESSAGE_BATTLE_MP_WAS_DRAINED_BY,d1",
            "byte_B66C:",
            "displayMessage d1,d0,#0,d2",
            "rts",
        ],
        context="absorb message selector",
    )
    absorb_candidates = [
        symbol("MESSAGE_BATTLE_ABSORBED_MAGIC_POINTS"),
        symbol("MESSAGE_BATTLE_MP_WAS_DRAINED_BY"),
    ]
    absorb_resolution = _static_resolution(
        "absorb-side-branch",
        "d1",
        absorb_candidates,
        [
            _control_fact(
                "actor-side-bit",
                symbol="COMBATANT_BIT_ENEMY",
                value=value("COMBATANT_BIT_ENEMY"),
            ),
            _control_fact("ally-assignment", message=absorb_candidates[0]),
            _control_fact("enemy-assignment", message=absorb_candidates[1]),
        ],
    )

    item_break_resolver = _parse_item_break_message_resolver(
        disasm,
        upstream_path=upstream_path,
        numeric_enums=numeric_enums,
        message_enums=enums,
        line_domain=line_domain,
    )
    break_path = root / "breakuseditem.asm"
    _require_exact_statement_section(
        break_path,
        [
            "moveq #0,d0",
            "jsr battlesceneScript_GetItemBreakMessage(pc)",
            "nop",
            "displayMessage d3,d1,#0,#0",
        ],
        context="item-break caller break mode",
    )
    _require_exact_statement_section(
        break_path,
        [
            "@DestroyItem:",
            "moveq #1,d0",
            "jsr battlesceneScript_GetItemBreakMessage(pc)",
            "nop",
            "displayMessage d3,d1,#0,#0",
        ],
        context="item-break caller destroy mode",
    )
    bases = item_break_resolver["baseMessages"]

    def item_candidates(mode: int) -> list[dict[str, Any]]:
        selected_bases = bases[:2] if mode == 0 else bases[2:]
        offsets = sorted({row["messageOffset"] for row in item_break_resolver["tableRows"]})
        return [
            _message_candidate(
                base["messageSymbol"] if offset == 0 else None,
                base["lineId"] + offset,
                enums=enums,
                line_domain=line_domain,
            )
            for base in selected_bases
            for offset in offsets
        ]

    break_resolution = _static_resolution(
        "item-break-message",
        "d3",
        item_candidates(0),
        [
            _control_fact("caller-mode", value=0),
            _control_fact("dodge-base-selection", symbol="dodge(a2)"),
            _control_fact("table-offset-add", symbol="table_ItemBreakMessages"),
        ],
    )
    destroy_resolution = _static_resolution(
        "item-break-message",
        "d3",
        item_candidates(1),
        [
            _control_fact("caller-mode", value=1),
            _control_fact("dodge-base-selection", symbol="dodge(a2)"),
            _control_fact("table-offset-add", symbol="table_ItemBreakMessages"),
        ],
    )
    return (
        {
            "code/gameflow/battle/battleactions/createbattlescenemessage.asm": [
                attack_resolution,
                cast_resolution,
                muddled_resolution,
                prism_resolution,
            ],
            "code/gameflow/battle/battleactions/inflictdamage.asm": [damage_resolution],
            "code/gameflow/battle/battleactions/displaydeathmessage.asm": [death_resolution],
            "code/gameflow/battle/battleactions/castspell.asm": [
                muddle_spell_resolution,
                desoul_resolution,
                absorb_resolution,
            ],
            "code/gameflow/battle/battleactions/breakuseditem.asm": [
                break_resolution,
                destroy_resolution,
            ],
        },
        item_break_resolver,
    )


def _build_battle_message_contract(
    disasm: Path, upstream_path: Path, *, upstream_commit: str | None = None
) -> dict[str, Any]:
    resolved_upstream_commit = upstream_commit or load_json(TOOLCHAIN)["sf2disasm"]["commit"]
    write_bsc_param = _parse_write_bsc_param_definition(disasm)
    macro_definitions = _parse_message_macro_definitions(disasm)
    definitions_by_macro = {row["sourceMacro"]: row for row in macro_definitions}
    if set(definitions_by_macro) != set(MESSAGE_MACRO_NAMES):
        raise ValueError("battle message macro definition set drift")
    command_words = {
        row["sourceMacro"]: row["commandWord"] for row in macro_definitions
    }
    dispatcher = _battle_scene_message_dispatch_contract(
        disasm,
        upstream_commit=resolved_upstream_commit,
        command_words=command_words,
    )
    source_files = _parse_source_message_uses(disasm)
    if len(source_files) != len(REPRESENTATIVE_SYMBOLS):
        raise ValueError("battle message source file inventory drift")
    h1_uses = _parse_h1_message_uses(upstream_path, command_words)
    h1_by_file: dict[str, list[dict[str, Any]]] = defaultdict(list)
    unmatched_h1 = list(h1_uses)
    for file_row in source_files:
        start = file_row["sourceH1StartAddress"]
        end = file_row["sourceH1EndAddressExclusive"]
        matched = [row for row in h1_uses if start <= row["h1ExpansionAddress"] < end]
        h1_by_file[file_row["sourcePath"]] = matched
        unmatched_h1 = [row for row in unmatched_h1 if row not in matched]
    if unmatched_h1:
        raise ValueError("battle message H1 use falls outside the declared source inventory")

    enums = _parse_message_equates(disasm)
    line_domain = _parse_message_line_domain(disasm)
    sites: list[dict[str, Any]] = []
    file_totals: list[dict[str, Any]] = []
    for file_row in sorted(source_files, key=lambda row: row["sourceH1StartAddress"]):
        source_sites = file_row["sites"]
        h1_sites = h1_by_file[file_row["sourcePath"]]
        if len(source_sites) != len(h1_sites):
            raise ValueError(
                "battle message source/H1 site count drift: " f"{file_row['sourcePath']}"
            )
        for source_site, h1_site in zip(source_sites, h1_sites, strict=True):
            source_identity = (source_site["sourceMacro"], source_site["operandExpressions"])
            h1_identity = (h1_site["sourceMacro"], h1_site["operandExpressions"])
            if source_identity != h1_identity:
                raise ValueError(
                    "battle message source/H1 macro identity drift: "
                    f"{file_row['sourcePath']}:{source_site['sourceLine']}"
                )
            definition = definitions_by_macro[source_site["sourceMacro"]]
            if h1_site["commandWord"] != definition["commandWord"]:
                raise ValueError("battle message source/H1 command identity drift")
            sites.append(
                {
                    **source_site,
                    "messageOperand": _message_operand_fact(
                        source_site["operandExpressions"][0],
                        enums=enums,
                        line_domain=line_domain,
                    ),
                    "h1ExpansionAddress": h1_site["h1ExpansionAddress"],
                    "assembledInstructionByteCount": h1_site["assembledInstructionByteCount"],
                }
            )
        file_totals.append(
            {
                "sourcePath": file_row["sourcePath"],
                "sourceH1StartAddress": file_row["sourceH1StartAddress"],
                "sourceH1EndAddressExclusive": file_row["sourceH1EndAddressExclusive"],
                "siteCount": len(source_sites),
            }
        )

    resolutions_by_path, item_break_resolver = _build_dynamic_message_resolutions(
        disasm,
        upstream_path=upstream_path,
        enums=enums,
        line_domain=line_domain,
    )
    dynamic_by_path: dict[str, list[dict[str, Any]]] = defaultdict(list)
    for site in sites:
        if site["messageOperand"]["kind"] == "dynamic-expression":
            dynamic_by_path[site["sourcePath"]].append(site)
    if set(dynamic_by_path) != set(resolutions_by_path):
        raise ValueError("battle message dynamic resolver source inventory drift")
    for source_path, dynamic_sites in dynamic_by_path.items():
        resolutions = resolutions_by_path[source_path]
        if len(dynamic_sites) != len(resolutions):
            raise ValueError(f"battle message dynamic resolver count drift: {source_path}")
        for site, resolution in zip(dynamic_sites, resolutions, strict=True):
            if site["operandExpressions"][0].lower() != resolution["register"]:
                raise ValueError(
                    "battle message dynamic resolver register drift: "
                    f"{source_path}:{site['sourceLine']}"
                )
            site["messageOperand"]["staticResolution"] = resolution

    caller_counts: Counter[tuple[str, str]] = Counter()
    for site in sites:
        caller_counts[(site["sourcePath"], site["enclosingGlobalLabel"])] += 1
    caller_totals = [
        {
            "sourcePath": path,
            "enclosingGlobalLabel": label,
            "siteCount": count,
        }
        for (path, label), count in sorted(caller_counts.items())
    ]
    immediate_messages: list[dict[str, Any]] = []
    immediate_counts: Counter[tuple[str, int]] = Counter()
    for site in sites:
        message_operand = site["messageOperand"]
        symbol = message_operand["messageSymbol"]
        if symbol is not None:
            immediate_counts[(symbol, message_operand["lineId"])] += 1
    for (symbol, line_id), source_use_count in immediate_counts.items():
        immediate_messages.append(
            {
                "messageSymbol": symbol,
                "lineId": line_id,
                "sourceUseCount": source_use_count,
            }
        )
    mode_counts = Counter(site["sourceMacro"] for site in sites)
    kind_counts = Counter(site["messageOperand"]["kind"] for site in sites)
    dynamic_resolutions = [
        site["messageOperand"]["staticResolution"]
        for site in sites
        if "staticResolution" in site["messageOperand"]
    ]
    dynamic_candidates = [
        candidate
        for resolution in dynamic_resolutions
        for candidate in resolution["candidateMessages"]
    ]
    summary = {
        "macroDefinitionCount": len(macro_definitions),
        "writeBscParamDefinitionCount": 1,
        "completeSourceFileCount": len(file_totals),
        "positiveSourceFileCount": sum(row["siteCount"] > 0 for row in file_totals),
        "zeroSourceFileCount": sum(row["siteCount"] == 0 for row in file_totals),
        "siteCount": len(sites),
        "modeCounts": {macro: mode_counts[macro] for macro in MESSAGE_MACRO_NAMES},
        "messageOperandKindCounts": {
            "immediate-message-enum": kind_counts["immediate-message-enum"],
            "dynamic-expression": kind_counts["dynamic-expression"],
        },
        "distinctImmediateMessageSymbolCount": len(immediate_messages),
        "distinctImmediateMessageIdCount": len({row["lineId"] for row in immediate_messages}),
        "dynamicResolutionCount": len(dynamic_resolutions),
        "unresolvedDynamicSiteCount": kind_counts["dynamic-expression"] - len(dynamic_resolutions),
        "dynamicCandidateCount": len(dynamic_candidates),
        "distinctDynamicCandidateMessageIdCount": len(
            {candidate["lineId"] for candidate in dynamic_candidates}
        ),
        "callerTotalCount": len(caller_totals),
        "h1BoundSiteCount": len(sites),
    }
    if sum(row["siteCount"] for row in file_totals) != len(sites):
        raise ValueError("battle message file totals do not reconcile")
    contract = {
        "macroDefinitions": macro_definitions,
        "battleSceneDispatcher": dispatcher,
        "writeBscParamDefinition": write_bsc_param,
        "textLineDomain": line_domain,
        "itemBreakMessageResolver": item_break_resolver,
        "immediateMessages": immediate_messages,
        "fileTotals": file_totals,
        "callerTotals": caller_totals,
        "messageSites": sites,
        "summary": summary,
    }
    _reconcile_battle_message_contract(contract)
    return contract


def _reconcile_battle_message_contract(contract: dict[str, Any]) -> None:
    """Check compact aggregates directly against the complete physical site corpus."""
    sites = contract["messageSites"]
    h1_addresses = [site["h1ExpansionAddress"] for site in sites]
    if h1_addresses != sorted(h1_addresses) or len(set(h1_addresses)) != len(h1_addresses):
        raise ValueError("battle message physical site order drift")
    file_counts = Counter(site["sourcePath"] for site in sites)
    expected_file_counts = {row["sourcePath"]: row["siteCount"] for row in contract["fileTotals"]}
    if len(expected_file_counts) != len(contract["fileTotals"]) or file_counts != Counter(
        expected_file_counts
    ):
        raise ValueError("battle message file totals do not reconcile")
    caller_counts = Counter((site["sourcePath"], site["enclosingGlobalLabel"]) for site in sites)
    expected_caller_counts = {
        (row["sourcePath"], row["enclosingGlobalLabel"]): row["siteCount"]
        for row in contract["callerTotals"]
    }
    if len(expected_caller_counts) != len(contract["callerTotals"]) or caller_counts != Counter(
        expected_caller_counts
    ):
        raise ValueError("battle message caller totals do not reconcile")
    immediate_counts = Counter(
        (site["messageOperand"]["messageSymbol"], site["messageOperand"]["lineId"])
        for site in sites
        if site["messageOperand"]["messageSymbol"] is not None
    )
    expected_immediate_counts = {
        (row["messageSymbol"], row["lineId"]): row["sourceUseCount"]
        for row in contract["immediateMessages"]
    }
    if (
        len(expected_immediate_counts) != len(contract["immediateMessages"])
        or immediate_counts != Counter(expected_immediate_counts)
    ):
        raise ValueError("battle message immediate totals do not reconcile")
    dynamic_sites = [
        site for site in sites if site["messageOperand"]["kind"] == "dynamic-expression"
    ]
    dynamic_resolutions = [
        site["messageOperand"].get("staticResolution") for site in dynamic_sites
    ]
    if any(resolution is None for resolution in dynamic_resolutions):
        raise ValueError("battle message unresolved dynamic operand remains")
    if contract["summary"]["dynamicResolutionCount"] != len(dynamic_resolutions):
        raise ValueError("battle message dynamic resolution total drift")
    if contract["summary"]["unresolvedDynamicSiteCount"] != 0:
        raise ValueError("battle message unresolved dynamic operand total drift")
    candidates = [
        candidate
        for resolution in dynamic_resolutions
        for candidate in resolution["candidateMessages"]
    ]
    if contract["summary"]["dynamicCandidateCount"] != len(candidates):
        raise ValueError("battle message dynamic candidate total drift")
    if contract["summary"]["distinctDynamicCandidateMessageIdCount"] != len(
        {candidate["lineId"] for candidate in candidates}
    ):
        raise ValueError("battle message distinct dynamic candidate total drift")
    line_domain = contract["textLineDomain"]
    for candidate in candidates:
        if not line_domain["firstLineId"] <= candidate["lineId"] <= line_domain["lastLineId"]:
            raise ValueError("battle message dynamic candidate line domain drift")


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


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
    expected = toolchain["sf2disasm"]["commit"]
    if commit != expected:
        raise ValueError(f"battle-actions inventory requires SF2DISASM {expected}, got {commit}")
    disasm = upstream_path / "disasm"
    if not (disasm / SOURCE_ROOT).is_dir():
        raise ValueError(f"battle-actions source root is missing: {disasm / SOURCE_ROOT}")
    return disasm, commit, toolchain


def build_battle_actions_inventory(upstream_path: Path) -> dict[str, Any]:
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    source_paths = sorted((disasm / SOURCE_ROOT).glob("*.asm"), key=lambda path: path.as_posix())
    files = [_parse_source_file(path, path.relative_to(disasm).as_posix()) for path in source_paths]
    if {Path(row["path"]).name for row in files} != set(REPRESENTATIVE_SYMBOLS):
        raise ValueError("battle-actions source file set drift")
    all_labels = {label for row in files for label in row["globalLabels"]}
    direct_calls: Counter[str] = Counter()
    for row in files:
        for call in row["directCalls"]:
            direct_calls[call["target"]] += call["siteCount"]
    index = load_json(RESEARCH_INDEX)
    records = [r for r in index["records"] if Path(r["sourcePath"]).parent == SOURCE_ROOT]
    summary = {
        "fileCount": len(files),
        "sourceLineCount": sum(row["sourceLineCount"] for row in files),
        "statementCount": sum(row["statementCount"] for row in files),
        "globalLabelCount": sum(len(row["globalLabels"]) for row in files),
        "localLabelCount": sum(row["localLabelCount"] for row in files),
        "directCallSiteCount": sum(direct_calls.values()),
        "indirectCallSiteCount": sum(row["indirectCallSiteCount"] for row in files),
        "uniqueDirectTargetCount": len(direct_calls),
        "internalDirectTargetCount": sum(target in all_labels for target in direct_calls),
        "externalDirectTargetCount": sum(target not in all_labels for target in direct_calls),
        "indexedRecordCount": len(records),
        "indexedFileCount": len({r["sourcePath"] for r in records}),
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "scope": SOURCE_ROOT.as_posix(),
        "summary": summary,
        "indexedRecordIds": sorted(r["id"] for r in records),
        "indexedSourcePaths": sorted({r["sourcePath"] for r in records}),
        "internalDirectCallTargets": sorted(t for t in direct_calls if t in all_labels),
        "externalDirectCallTargets": sorted(t for t in direct_calls if t not in all_labels),
        "actionFacts": _build_action_facts(disasm),
        "battleMessageContract": _build_battle_message_contract(
            disasm, upstream_path, upstream_commit=commit
        ),
        "files": files,
    }


def _verify_item_break_auxiliary_owner(
    output: dict[str, Any],
    *,
    fixture_rom_sha256: str,
    canonical_rom_sha256: str,
) -> None:
    """Join source/H1-derived break facts to the accepted item-auxiliary owner."""
    item_auxiliary = load_json(ITEM_AUXILIARY_FIXTURE)
    validate_json(
        item_auxiliary,
        ITEM_AUXILIARY_FIXTURE_SCHEMA,
        owner=str(ITEM_AUXILIARY_FIXTURE),
    )
    resolver = output["battleMessageContract"]["itemBreakMessageResolver"]
    if item_auxiliary["upstreamCommit"] != output["upstream"]["commit"]:
        raise ValueError("battle message item-break owner provenance drift")
    if (
        item_auxiliary["romSha256"] != canonical_rom_sha256
        or item_auxiliary["romSha256"] != fixture_rom_sha256
    ):
        raise ValueError("battle message item-break owner ROM provenance drift")
    if item_auxiliary["table"]["table_ItemBreakMessages"] != resolver["sourceH1StartAddress"]:
        raise ValueError("battle message item-break owner table address drift")
    if item_auxiliary["summary"]["breakMessageCount"] != len(resolver["tableRows"]):
        raise ValueError("battle message item-break owner row count drift")
    if item_auxiliary["consumerRules"].get("breakMessages") != ITEM_BREAK_CONSUMER_RULE:
        raise ValueError("battle message item-break owner consumer rule drift")


def verify_battle_actions_inventory(
    upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    rom_manifest = load_json(ROM_MANIFEST)
    canonical_rom_sha256 = rom_manifest["hashes"]["sha256"]
    output = build_battle_actions_inventory(upstream_path)
    validate_json(output, SCHEMA, owner="battle-actions static inventory")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != canonical_rom_sha256
    ):
        raise ValueError("battle-actions fixture provenance drift")
    if output["summary"] != manifest["summary"]:
        raise ValueError("battle-actions static summary drift")
    by_name = {Path(row["path"]).name: row for row in output["files"]}
    for filename, symbol in fixture["expected"]["representativeSymbols"].items():
        if symbol not in by_name[filename]["globalLabels"]:
            raise ValueError(f"battle-actions representative symbol drift: {filename}::{symbol}")
    if output["actionFacts"] != fixture["expected"]["actionFacts"]:
        raise ValueError("battle-actions model drift")
    _verify_item_break_auxiliary_owner(
        output,
        fixture_rom_sha256=fixture["romSha256"],
        canonical_rom_sha256=canonical_rom_sha256,
    )
    if output["battleMessageContract"] != fixture["expected"]["battleMessageContract"]:
        raise ValueError("battle-actions message contract drift")
    if output["battleMessageContract"]["summary"] != manifest["battleMessageSummary"]:
        raise ValueError("battle-actions message summary drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"]:
        raise ValueError(
            "battle-actions static hash mismatch: expected "
            f"{manifest['outputSha256']}, got {digest}"
        )
    destination = output_path or repo_path("local/derived/battle-actions-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
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
