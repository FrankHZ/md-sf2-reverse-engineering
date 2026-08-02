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


def _build_battle_message_contract(
    disasm: Path, upstream_path: Path, *, upstream_commit: str | None = None
) -> dict[str, Any]:
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
        upstream_commit=upstream_commit or load_json(TOOLCHAIN)["sf2disasm"]["commit"],
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


def verify_battle_actions_inventory(
    upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_battle_actions_inventory(upstream_path)
    validate_json(output, SCHEMA, owner="battle-actions static inventory")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != load_json(ROM_MANIFEST)["hashes"]["sha256"]
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
