from __future__ import annotations

import hashlib
import json
import re
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.h2.entity_action_scripts import _access_rows, _global_access_rows
from sf2tool.h2.sprite_dialogue import build_sprite_dialogue_contract
from sf2tool.h2.text_banks import build_text_line_domain_contract
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.rom import inspect_rom
from sf2tool.source_text import read_upstream_text

ID = "sf2-map-script-engine-static-v1"
MANIFEST = repo_path("manifests/extractions/map-script-engine-static.json")
SCHEMA = repo_path("schemas/map-script-engine-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/map-script-engine-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-map-script-engine-static-fixture.schema.json")

MACRO_PATH = Path("sf2cutscenemacros.asm")
ENGINE_PATHS = (
    Path("code/common/scripting/map/mapscriptengine_1.asm"),
    Path("code/common/scripting/map/mapscriptengine_2.asm"),
)
DISPATCH_SOURCE = ENGINE_PATHS[1]

DIALOGUE_MACROS = (
    "nextSingleText",
    "nextSingleTextVar",
    "nextText",
    "nextTextVar",
    "textCursor",
    "hideText",
)
DIALOGUE_DISPLAY_MACROS = DIALOGUE_MACROS[:4]
DIALOGUE_HANDLER_BY_MACRO = {
    "nextSingleText": "csc00_displaySingleTextbox",
    "nextSingleTextVar": "csc01_displaySingleTextboxWithVars",
    "nextText": "csc02_displayTextbox",
    "nextTextVar": "csc03_displayTextboxWithVars",
    "textCursor": "csc04_setTextIndex",
    "hideText": "csc09_hideDialogueAndPortraitWindows",
}
DIALOGUE_MODIFIER_MACROS = ("nextSingleText", "nextText")
PORTRAIT_HANDLER = "csc1D_showPortrait"
ENTITY_DIALOGUE_CONSUMER_PATH = Path(
    "code/common/scripting/entity/getentityportaitandspeechsfx.asm"
)
ENTITY_DIALOGUE_CONSUMER = "GetEntityPortaitAndSpeechSfx"
DIALOGUE_CALLER_HANDLER_NAMES = tuple(
    DIALOGUE_HANDLER_BY_MACRO[macro] for macro in DIALOGUE_MACROS
) + (PORTRAIT_HANDLER,)
DIALOGUE_CALLEE_TARGETS = (PORTRAIT_HANDLER, ENTITY_DIALOGUE_CONSUMER)
DIALOGUE_CONSTANT_NAMES = ("COMBATANT_MASK_ALL", "ENTITY_NONE")
DIALOGUE_RUNTIME_QUESTIONS = ["dialogue-presentation/runtime-matrix"]


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _literal(value: str) -> int:
    value = value.strip()
    return int(value[1:], 16) if value.startswith("$") else int(value)


def _macro_blocks(source: str) -> dict[str, str]:
    pattern = re.compile(
        r"^(?P<name>[A-Za-z_][A-Za-z0-9_]*):\s*macro[^\n]*\n"
        r"(?P<body>.*?)(?=^\s*endm\s*$)",
        re.MULTILINE | re.DOTALL,
    )
    return {match.group("name"): match.group("body") for match in pattern.finditer(source)}


def _emission_rows(body: str) -> list[dict[str, Any]]:
    widths = {"b": 1, "w": 2, "l": 4}
    rows: list[dict[str, Any]] = []
    offset = 0
    for raw_line in body.splitlines():
        code = raw_line.split(";", 1)[0].strip()
        direct = re.match(r"^dc\.([bwl])\s+(.+)$", code)
        shorthand = re.match(
            r"^defineShorthand\.([bwl])\s+([^,]+),(.+)$", code
        )
        if direct:
            width_code, expression = direct.groups()
            encoding = "direct"
        elif shorthand:
            width_code, prefix, expression = shorthand.groups()
            encoding = f"shorthand:{prefix.strip()}"
        else:
            continue
        expression = expression.strip()
        if direct and "," in expression:
            raise ValueError(f"unsupported multi-value map-script emission: {code}")
        width = widths[width_code]
        rows.append(
            {
                "streamOffset": offset,
                "widthBytes": width,
                "expression": expression,
                "parameterOrdinals": sorted(
                    {int(value) for value in re.findall(r"\\(\d+)", expression)}
                ),
                "encoding": encoding,
            }
        )
        offset += width
    return rows


def _substitute_alias_layout(
    layout: list[dict[str, Any]], arguments: list[str]
) -> list[dict[str, Any]]:
    def substitute(match: re.Match[str]) -> str:
        ordinal = int(match.group(1))
        if ordinal > len(arguments):
            raise ValueError(f"map-script alias argument {ordinal} is missing")
        return arguments[ordinal - 1]

    rows = []
    for row in layout:
        expression = re.sub(r"\\(\d+)", substitute, row["expression"])
        rows.append(
            {
                **row,
                "expression": expression,
                "parameterOrdinals": sorted(
                    {int(value) for value in re.findall(r"\\(\d+)", expression)}
                ),
            }
        )
    return rows


def _map_macro_contracts(disasm: Path) -> dict[str, dict[str, Any]]:
    source = read_upstream_text(disasm / MACRO_PATH)
    prefix = source.split("; entity data structure", 1)[0]
    blocks = _macro_blocks(prefix)
    primary: dict[str, dict[str, Any]] = {}
    for name, body in blocks.items():
        emissions = _emission_rows(body)
        first_word = re.search(
            r"^\s*dc\.w\s+(\$?[0-9A-Fa-f]+)\b", body, re.MULTILINE
        )
        if first_word is None:
            continue
        opcode = _literal(first_word.group(1))
        if opcode <= 0x56:
            if not emissions or emissions[0]["widthBytes"] != 2:
                raise ValueError(f"map-script opcode emission is malformed: {name}")
            operand_layout = emissions[1:]
            parameters = sorted(
                {int(value) for value in re.findall(r"\\(\d+)", body)}
            )
            primary[name] = {
                "kind": "command",
                "opcode": opcode,
                "encodedBytes": sum(row["widthBytes"] for row in emissions),
                "operandBytes": sum(row["widthBytes"] for row in operand_layout),
                "operandLayout": operand_layout,
                "parameterOrdinals": parameters,
                "aliasOf": None,
            }
    if len(primary) != 82 or len({row["opcode"] for row in primary.values()}) != 82:
        raise ValueError("map-script primary macro boundary drift")

    aliases: dict[str, dict[str, Any]] = {}
    for name, body in blocks.items():
        if name in primary:
            continue
        call = re.search(r"^\s*([A-Za-z_][A-Za-z0-9_]*)\b", body, re.MULTILINE)
        if call and call.group(1) in primary:
            target = call.group(1)
            call_line = next(
                line.split(";", 1)[0].strip()
                for line in body.splitlines()
                if line.split(";", 1)[0].strip()
            )
            argument_text = call_line[len(target) :].strip()
            arguments = [value.strip() for value in argument_text.split(",")]
            operand_layout = _substitute_alias_layout(
                primary[target]["operandLayout"], arguments
            )
            aliases[name] = {
                **primary[target],
                "operandLayout": operand_layout,
                "parameterOrdinals": sorted(
                    {int(value) for value in re.findall(r"\\(\d+)", body)}
                ),
                "aliasOf": target,
            }
    if len(aliases) != 8:
        raise ValueError("map-script alias macro boundary drift")

    special_kinds = {
        "csWait": "sleep",
        "cscNop": "source-nop",
        "csc_end": "terminator",
    }
    if not set(special_kinds).issubset(blocks):
        raise ValueError("map-script special macro boundary drift")
    special = {}
    for name, kind in special_kinds.items():
        emissions = _emission_rows(blocks[name])
        operand_layout = emissions[1:] if name == "csWait" else []
        special[name] = {
            "kind": kind,
            "opcode": None,
            "encodedBytes": sum(row["widthBytes"] for row in emissions),
            "operandBytes": sum(row["widthBytes"] for row in operand_layout),
            "operandLayout": operand_layout,
            "parameterOrdinals": sorted(
                {int(value) for value in re.findall(r"\\(\d+)", blocks[name])}
            ),
            "aliasOf": None,
        }
    return {**primary, **aliases, **special}


def _dispatch_targets(source: str) -> list[str]:
    table = re.search(
        r"^rjt_cutsceneScriptCommands:\s*\n(?P<body>.*?)(?=^loc_47234:)",
        source,
        re.MULTILINE | re.DOTALL,
    )
    if table is None:
        raise ValueError("map-script dispatcher table is missing")
    targets = re.findall(
        r"^\s*dc\.w\s+\(?([A-Za-z_][A-Za-z0-9_]*)-rjt_cutsceneScriptCommands\)?",
        table.group("body"),
        re.MULTILINE,
    )
    if len(targets) != 90:
        raise ValueError(f"map-script dispatcher slot drift: {len(targets)}")
    return targets


def _statements(body: str) -> list[str]:
    statements = []
    for raw_line in body.splitlines():
        code = raw_line.split(";", 1)[0].strip()
        if not code or re.match(r"^[A-Za-z_@][A-Za-z0-9_@]*:$", code):
            continue
        if re.match(r"^[a-z][A-Za-z0-9]*(?:\.[bwls])?(?:\s+|$)", code):
            statements.append(re.sub(r"\s+", " ", code))
    return statements


def _handler_family(opcode: int, target: str) -> str:
    if target == "csc_doNothing":
        return "filler"
    if opcode <= 4:
        return "text"
    if opcode == 5:
        return "audio"
    if opcode == 6:
        return "no-op"
    if opcode == 7:
        return "map-transition"
    if opcode == 8:
        return "party"
    if opcode == 9:
        return "dialogue-ui"
    if 10 <= opcode <= 15:
        return "control-flow"
    if 16 <= opcode <= 19:
        return "story-state"
    if 20 <= opcode <= 49:
        return "entity"
    if 50 <= opcode <= 55:
        return "map-camera"
    if 57 <= opcode <= 65 or 74 <= opcode <= 75:
        return "presentation"
    if 66 <= opcode <= 73:
        return "map-state"
    if 80 <= opcode <= 86:
        return "entity-party"
    raise ValueError(f"unclassified map-script handler opcode: {opcode}")


def _cursor_flow(target: str, statements: list[str]) -> str:
    has_absolute_transfer = "movea.l (a6),a6" in statements
    has_skip = "addq.w #4,a6" in statements
    if has_absolute_transfer and has_skip:
        return "conditional-absolute-jump"
    if has_absolute_transfer:
        return "absolute-jump"
    if target == "csc14_setEntityActscriptManual":
        if (
            "move.l a6,ENTITYDEF_OFFSET_ACTSCRIPTADDR(a5)" not in statements
            or "cmpi.w #$8080,(a6)+" not in statements
        ):
            raise ValueError("map-script inline action-program cursor shape drift")
        return "inline-action-program"
    return "sequential"


def _handler_rows(
    disasm: Path,
    addresses: dict[str, int],
    dispatch_targets: list[str],
    source_counts: Counter[str],
    macro_contracts: dict[str, dict[str, Any]],
) -> list[dict[str, Any]]:
    sources = {path: read_upstream_text(disasm / path) for path in ENGINE_PATHS}
    macros_by_opcode: dict[int, list[str]] = {}
    for name, contract in macro_contracts.items():
        if contract["opcode"] is not None:
            macros_by_opcode.setdefault(contract["opcode"], []).append(name)
    rows = []
    for target in sorted(set(dispatch_targets)):
        owner = None
        body_match = None
        for path, source in sources.items():
            match = re.search(
                rf"^{re.escape(target)}:\s*\n(?P<body>.*?)"
                rf"^\s*; End of function {re.escape(target)}\s*$",
                source,
                re.MULTILINE | re.DOTALL,
            )
            if match:
                owner, body_match = path, match
                break
        if owner is None or body_match is None:
            raise ValueError(f"map-script handler body is missing: {target}")
        if target not in addresses:
            raise ValueError(f"map-script handler lacks H1 address: {target}")
        source = sources[owner]
        body = body_match.group("body")
        statements = _statements(body)
        opcodes = [index for index, value in enumerate(dispatch_targets) if value == target]
        entity_accesses = _access_rows(
            statements, re.compile(r"\b(ENTITYDEF_OFFSET_[A-Z0-9_]+)\b")
        )
        direct_calls = sorted(
            set(
                re.findall(
                    r"\b(?:bsr|jsr)(?:\.[bwl])?\s+\(?([A-Za-z_][A-Za-z0-9_]*)",
                    body,
                )
            )
        )
        macro_names = sorted(
            {name for opcode in opcodes for name in macros_by_opcode.get(opcode, [])}
        )
        encoded_sizes = {macro_contracts[name]["encodedBytes"] for name in macro_names}
        operand_sizes = {macro_contracts[name]["operandBytes"] for name in macro_names}
        if len(encoded_sizes) > 1 or len(operand_sizes) > 1:
            raise ValueError(f"map-script alias physical layout drift: {target}")
        rows.append(
            {
                "name": target,
                "opcodes": opcodes,
                "families": sorted({_handler_family(opcode, target) for opcode in opcodes}),
                "address": addresses[target],
                "sourcePath": owner.as_posix(),
                "startLine": source.count("\n", 0, body_match.start("body")) + 1,
                "endLine": source.count("\n", 0, body_match.end("body")) + 1,
                "statementCount": len(statements),
                "macroNames": macro_names,
                "encodedCommandBytes": next(iter(encoded_sizes), 2),
                "operandBytes": next(iter(operand_sizes), 0),
                "cursorFlow": _cursor_flow(target, statements),
                "sourceCommandCount": sum(source_counts[name] for name in macro_names),
                "entityFieldAccesses": entity_accesses,
                "globalStateAccesses": _global_access_rows(statements),
                "directCalls": direct_calls,
                "scriptCursorStatements": [row for row in statements if "a6" in row],
            }
        )
    if len(rows) != 83:
        raise ValueError(f"map-script unique handler boundary drift: {len(rows)}")
    return rows


def _source_usage(
    disasm: Path, macro_contracts: dict[str, dict[str, Any]]
) -> tuple[Counter[str], dict[str, int], list[str]]:
    counts: Counter[str] = Counter()
    paths: set[str] = set()
    pattern = re.compile(
        r"^\s*(?:[A-Za-z_][A-Za-z0-9_]*:\s*)?([A-Za-z_][A-Za-z0-9_]*)\b"
    )
    for root_name in ("code", "data"):
        for path in sorted((disasm / root_name).rglob("*.asm")):
            relative = path.relative_to(disasm)
            source = read_upstream_text(path)
            found = False
            for raw_line in source.splitlines():
                match = pattern.match(raw_line.split(";", 1)[0])
                if match and match.group(1) in macro_contracts:
                    counts[match.group(1)] += 1
                    found = True
            if found:
                paths.add(relative.as_posix())
    opcode_counts: Counter[int] = Counter()
    for name, count in counts.items():
        opcode = macro_contracts[name]["opcode"]
        if opcode is not None:
            opcode_counts[opcode] += count
    for name in macro_contracts:
        counts.setdefault(name, 0)
    return (
        counts,
        {str(key): value for key, value in sorted(opcode_counts.items())},
        sorted(paths),
    )


def _logical_source_lines(source: str) -> list[tuple[int, str]]:
    rows: list[tuple[int, str]] = []
    pending = ""
    start_line = 0
    for line_number, raw_line in enumerate(source.splitlines(), start=1):
        code = raw_line.split(";", 1)[0].rstrip()
        if not code.strip():
            continue
        if pending:
            code = f"{pending} {code.strip()}"
        else:
            start_line = line_number
        if code.rstrip().endswith("&"):
            pending = code.rstrip()[:-1].rstrip()
            continue
        rows.append((start_line, code))
        pending = ""
    if pending:
        raise ValueError("unterminated map-script source continuation")
    return rows


def _invocation(
    statement: str, macro_contracts: dict[str, dict[str, Any]]
) -> tuple[str, list[str]] | None:
    match = re.match(r"^\s*([A-Za-z_][A-Za-z0-9_]*)\b(.*)$", statement)
    if not match or match.group(1) not in macro_contracts:
        return None
    argument_text = match.group(2).strip()
    arguments = (
        [argument.strip() for argument in argument_text.split(",")]
        if argument_text
        else []
    )
    return match.group(1), arguments


def _target_symbol(expression: str, known_symbols: set[str]) -> str:
    candidates = re.findall(r"\b[A-Za-z_][A-Za-z0-9_]*\b", expression)
    matches = [candidate for candidate in candidates if candidate in known_symbols]
    if len(matches) != 1:
        raise ValueError(f"map-script target expression is ambiguous: {expression}")
    return matches[0]


def _program_references(
    disasm: Path,
    label_owners: dict[str, str],
    programs: list[dict[str, Any]],
) -> dict[str, Any]:
    occurrences = {label: Counter() for label in label_owners}
    scanned_files = 0
    for root_name in ("code", "data"):
        for path in sorted((disasm / root_name).rglob("*.asm")):
            scanned_files += 1
            source_path = path.relative_to(disasm).as_posix()
            for raw_line in read_upstream_text(path).splitlines():
                code = raw_line.split(";", 1)[0]
                definition = re.match(r"^([A-Za-z_][A-Za-z0-9_]*):", code)
                definition_label = definition.group(1) if definition else None
                for token in re.finditer(r"\b[A-Za-z_][A-Za-z0-9_]*\b", code):
                    label = token.group(0)
                    if label not in label_owners:
                        continue
                    if label == definition_label and token.start() == 0:
                        continue
                    occurrences[label][source_path] += 1

    owner_paths = {program["id"]: program["sourcePath"] for program in programs}
    label_rows = []
    program_counts = {program["id"]: Counter() for program in programs}
    for label, owner_program in sorted(label_owners.items()):
        owner_path = owner_paths[owner_program]
        same_file_count = occurrences[label][owner_path]
        external_sources = sorted(
            path for path, count in occurrences[label].items() if path != owner_path and count
        )
        external_count = sum(occurrences[label][path] for path in external_sources)
        program_counts[owner_program]["sameFileReferenceCount"] += same_file_count
        program_counts[owner_program]["externalReferenceCount"] += external_count
        if same_file_count or external_count:
            program_counts[owner_program]["referencedLabelCount"] += 1
        label_rows.append(
            {
                "label": label,
                "ownerProgram": owner_program,
                "sameFileReferenceCount": same_file_count,
                "externalReferenceCount": external_count,
                "externalSourcePaths": external_sources,
            }
        )

    program_rows = []
    for program in programs:
        counts = program_counts[program["id"]]
        reference_class = (
            "external"
            if counts["externalReferenceCount"]
            else "same-file-only"
            if counts["sameFileReferenceCount"]
            else "unreferenced"
        )
        program_rows.append(
            {
                "id": program["id"],
                "sourcePath": program["sourcePath"],
                "labelCount": len(program["labels"]),
                "referencedLabelCount": counts["referencedLabelCount"],
                "sameFileReferenceCount": counts["sameFileReferenceCount"],
                "externalReferenceCount": counts["externalReferenceCount"],
                "referenceClass": reference_class,
            }
        )

    class_counts = Counter(row["referenceClass"] for row in program_rows)
    return {
        "summary": {
            "scannedSourceFileCount": scanned_files,
            "referencedProgramCount": len(programs) - class_counts["unreferenced"],
            "externallyReferencedProgramCount": class_counts["external"],
            "sameFileOnlyProgramCount": class_counts["same-file-only"],
            "unreferencedProgramCount": class_counts["unreferenced"],
            "referencedLabelCount": sum(
                bool(row["sameFileReferenceCount"] or row["externalReferenceCount"])
                for row in label_rows
            ),
            "unreferencedLabelCount": sum(
                not row["sameFileReferenceCount"] and not row["externalReferenceCount"]
                for row in label_rows
            ),
            "sameFileReferenceCount": sum(
                row["sameFileReferenceCount"] for row in label_rows
            ),
            "externalReferenceCount": sum(
                row["externalReferenceCount"] for row in label_rows
            ),
        },
        "unreferencedPrograms": [
            row["id"] for row in program_rows if row["referenceClass"] == "unreferenced"
        ],
        "programs": program_rows,
        "labels": label_rows,
    }


def _enum_value(source: str, name: str) -> int:
    match = re.search(
        rf"^{re.escape(name)}:\s+equ\s+(\$?[0-9A-Fa-f]+)\b",
        source,
        re.MULTILINE,
    )
    if match is None:
        raise ValueError(f"map-script enum is missing: {name}")
    return _literal(match.group(1))


def _story_state_facts(disasm: Path, programs: list[dict[str, Any]]) -> dict[str, Any]:
    enums = read_upstream_text(disasm / "sf2enums.asm")
    yes_no_flag = _enum_value(enums, "FLAG_INDEX_YES_NO_PROMPT")
    battle_flag_start = _enum_value(enums, "BATTLE_UNLOCKED_FLAGS_START")
    reads = []
    writes = []
    prompt_writes = []
    battle_unlock_writes = []
    program_states = []
    read_counts: Counter[int] = Counter()
    write_counts: Counter[int] = Counter()
    for program in programs:
        program_reads: set[int] = set()
        program_sets: set[int] = set()
        program_clears: set[int] = set()
        program_prompts: set[int] = set()
        program_unlocks: set[int] = set()
        for command in program["commands"]:
            macro = command["macro"]
            if macro in {"jumpIfFlagSet", "jumpIfFlagClear"}:
                flag = _literal(command["arguments"][0])
                condition = "set" if macro == "jumpIfFlagSet" else "clear"
                read_counts[flag] += 1
                program_reads.add(flag)
                reads.append(
                    {
                        "program": program["id"],
                        "commandIndex": command["index"],
                        "flag": flag,
                        "condition": condition,
                        "targetSymbol": command["targetSymbol"],
                    }
                )
            elif macro in {"setF", "clearF", "csc10"}:
                flag = _literal(command["arguments"][0])
                operation = (
                    "set"
                    if macro == "setF"
                    else "clear"
                    if macro == "clearF"
                    else "set"
                    if _literal(command["arguments"][1]) != 0
                    else "clear"
                )
                write_counts[flag] += 1
                (program_sets if operation == "set" else program_clears).add(flag)
                writes.append(
                    {
                        "program": program["id"],
                        "commandIndex": command["index"],
                        "flag": flag,
                        "operation": operation,
                        "macro": macro,
                    }
                )
            elif macro == "yesNo":
                write_counts[yes_no_flag] += 1
                program_prompts.add(yes_no_flag)
                prompt_writes.append(
                    {
                        "program": program["id"],
                        "commandIndex": command["index"],
                        "flag": yes_no_flag,
                        "zeroResultOperation": "set",
                        "nonzeroResultOperation": "clear",
                    }
                )
            elif macro == "setStoryFlag":
                battle = _literal(command["arguments"][0])
                flag = battle_flag_start + battle
                write_counts[flag] += 1
                program_unlocks.add(flag)
                battle_unlock_writes.append(
                    {
                        "program": program["id"],
                        "commandIndex": command["index"],
                        "battle": battle,
                        "flag": flag,
                    }
                )
        if program_reads or program_sets or program_clears or program_prompts or program_unlocks:
            program_states.append(
                {
                    "program": program["id"],
                    "readFlags": sorted(program_reads),
                    "setFlags": sorted(program_sets),
                    "clearFlags": sorted(program_clears),
                    "promptFlags": sorted(program_prompts),
                    "battleUnlockFlags": sorted(program_unlocks),
                }
            )

    read_flags = set(read_counts)
    write_flags = set(write_counts)
    return {
        "summary": {
            "conditionalReadCount": len(reads),
            "uniqueReadFlagCount": len(read_flags),
            "directWriteCount": len(writes),
            "yesNoPromptWriteCount": len(prompt_writes),
            "battleUnlockWriteCount": len(battle_unlock_writes),
            "uniqueWriteFlagCount": len(write_flags),
            "readWriteOverlapCount": len(read_flags & write_flags),
            "statefulProgramCount": len(program_states),
        },
        "constants": {
            "yesNoPromptFlag": yes_no_flag,
            "battleUnlockedFlagsStart": battle_flag_start,
        },
        "readFlagCounts": {
            str(flag): count for flag, count in sorted(read_counts.items())
        },
        "writeFlagCounts": {
            str(flag): count for flag, count in sorted(write_counts.items())
        },
        "readWriteOverlapFlags": sorted(read_flags & write_flags),
        "directSetFlags": sorted(
            {row["flag"] for row in writes if row["operation"] == "set"}
        ),
        "directClearFlags": sorted(
            {row["flag"] for row in writes if row["operation"] == "clear"}
        ),
        "battleUnlockFlags": sorted({row["flag"] for row in battle_unlock_writes}),
        "conditionalReads": reads,
        "directWrites": writes,
        "yesNoPromptWrites": prompt_writes,
        "battleUnlockWrites": battle_unlock_writes,
        "programs": program_states,
    }


def _program_corpus(
    disasm: Path,
    source_paths: list[str],
    macro_contracts: dict[str, dict[str, Any]],
    addresses: dict[str, int],
) -> dict[str, Any]:
    target_ordinals = {
        "executeSubroutine": (0, "subroutine-call"),
        "jump": (0, "absolute-jump"),
        "jumpIfFlagSet": (1, "conditional-absolute-jump"),
        "jumpIfFlagClear": (1, "conditional-absolute-jump"),
        "jumpIfDefeatedByLastAttack": (1, "conditional-absolute-jump"),
        "jumpIfDead": (1, "conditional-absolute-jump"),
    }
    source_symbols = {
        match.group(1)
        for source_path in source_paths
        for _, line in _logical_source_lines(read_upstream_text(disasm / source_path))
        if line and not line[0].isspace()
        for match in [re.match(r"^([A-Za-z_][A-Za-z0-9_]*):", line)]
        if match
    }
    known_symbols = set(addresses) | source_symbols
    programs: list[dict[str, Any]] = []
    for source_path in source_paths:
        source = read_upstream_text(disasm / source_path)
        pending_labels: list[str] = []
        active: dict[str, Any] | None = None
        for line_number, logical_line in _logical_source_lines(source):
            statement = logical_line
            label_match = (
                re.match(r"^([A-Za-z_][A-Za-z0-9_]*):\s*(.*)$", logical_line)
                if logical_line and not logical_line[0].isspace()
                else None
            )
            if label_match:
                label, statement = label_match.groups()
                if active is None:
                    pending_labels.append(label)
                else:
                    active["labels"].append(label)
                if not statement:
                    continue
            invocation = _invocation(statement, macro_contracts)
            if invocation is None:
                if active is None:
                    pending_labels = []
                continue
            macro, arguments = invocation
            contract = macro_contracts[macro]
            if active is None:
                entry_label = pending_labels[0] if pending_labels else None
                entry = (
                    entry_label if entry_label else f"{source_path}#L{line_number}"
                )
                active = {
                    "id": entry,
                    "entryLabel": entry_label,
                    "address": addresses.get(entry),
                    "sourcePath": source_path,
                    "startLine": line_number,
                    "endLine": None,
                    "termination": None,
                    "labels": list(pending_labels),
                    "commands": [],
                }
                pending_labels = []
            command: dict[str, Any] = {
                "index": len(active["commands"]),
                "sourceLine": line_number,
                "macro": macro,
                "kind": contract["kind"],
                "opcode": contract["opcode"],
                "encodedBytes": contract["encodedBytes"],
                "arguments": arguments,
            }
            if macro in target_ordinals:
                ordinal, transfer_kind = target_ordinals[macro]
                if ordinal >= len(arguments):
                    raise ValueError(
                        f"map-script target argument is missing: {source_path}:{line_number}"
                    )
                target = _target_symbol(arguments[ordinal], known_symbols)
                command["transferKind"] = transfer_kind
                command["targetSymbol"] = target
                command["targetAddress"] = addresses.get(target)
            active["commands"].append(command)
            if macro == "csc_end":
                active["endLine"] = line_number
                active["termination"] = "csc-end"
                programs.append(active)
                active = None
                pending_labels = []
        if active is not None:
            if active["commands"][-1]["macro"] != "jump":
                raise ValueError(f"unterminated map-script program: {active['id']}")
            active["endLine"] = active["commands"][-1]["sourceLine"]
            active["termination"] = "absolute-jump"
            programs.append(active)

    if len({program["id"] for program in programs}) != len(programs):
        raise ValueError("duplicate map-script program entry label")
    label_owners: dict[str, str] = {}
    for program in programs:
        for label in program["labels"]:
            if label in label_owners:
                raise ValueError(f"duplicate map-script program label: {label}")
            label_owners[label] = program["id"]
    references = _program_references(disasm, label_owners, programs)
    story_state = _story_state_facts(disasm, programs)

    transfer_counts: Counter[str] = Counter()
    transfers = []
    for program in programs:
        for command in program["commands"]:
            if "transferKind" not in command:
                continue
            target_program = label_owners.get(command["targetSymbol"])
            relation = (
                "assembly-subroutine"
                if command["transferKind"] == "subroutine-call"
                and target_program is None
                else "same-program"
                if target_program == program["id"]
                else "cross-program"
                if target_program is not None
                else "unowned-script-target"
            )
            if relation == "unowned-script-target":
                raise ValueError(
                    f"map-script branch target has no program owner: {command['targetSymbol']}"
                )
            transfer_counts[f"{command['transferKind']}:{relation}"] += 1
            transfers.append(
                {
                    "sourceProgram": program["id"],
                    "commandIndex": command["index"],
                    "kind": command["transferKind"],
                    "targetSymbol": command["targetSymbol"],
                    "targetAddress": command["targetAddress"],
                    "targetProgram": target_program,
                    "relation": relation,
                }
            )

    command_count = sum(len(program["commands"]) for program in programs)
    source_only_programs = [
        {
            "id": program["id"],
            "sourcePath": program["sourcePath"],
            "termination": program["termination"],
        }
        for program in programs
        if program["address"] is None
    ]
    return {
        "summary": {
            "sourceFileCount": len(source_paths),
            "programCount": len(programs),
            "anonymousProgramCount": sum(
                program["entryLabel"] is None for program in programs
            ),
            "h1AddressedProgramCount": sum(
                program["address"] is not None for program in programs
            ),
            "sourceOnlyProgramCount": sum(
                program["address"] is None for program in programs
            ),
            "programLabelCount": len(label_owners),
            "cscEndTerminatedProgramCount": sum(
                program["termination"] == "csc-end" for program in programs
            ),
            "absoluteJumpTerminatedProgramCount": sum(
                program["termination"] == "absolute-jump" for program in programs
            ),
            "commandCount": command_count,
            "encodedCommandByteCount": sum(
                command["encodedBytes"]
                for program in programs
                for command in program["commands"]
            ),
            "transferCount": len(transfers),
            "sameProgramTransferCount": sum(
                transfer["relation"] == "same-program" for transfer in transfers
            ),
            "crossProgramTransferCount": sum(
                transfer["relation"] == "cross-program" for transfer in transfers
            ),
            "assemblySubroutineCallCount": sum(
                transfer["relation"] == "assembly-subroutine" for transfer in transfers
            ),
            "minimumCommandsPerProgram": min(
                len(program["commands"]) for program in programs
            ),
            "maximumCommandsPerProgram": max(
                len(program["commands"]) for program in programs
            ),
        },
        "transferCounts": dict(sorted(transfer_counts.items())),
        "referenceSummary": references["summary"],
        "storyState": story_state,
        "unreferencedPrograms": references["unreferencedPrograms"],
        "programReferences": references["programs"],
        "labelReferences": references["labels"],
        "sourceOnlyPrograms": source_only_programs,
        "largestPrograms": [
            {"id": program["id"], "commandCount": len(program["commands"])}
            for program in sorted(
                programs,
                key=lambda row: (-len(row["commands"]), row["id"]),
            )[:10]
        ],
        "labelOwners": dict(sorted(label_owners.items())),
        "transfers": transfers,
        "programs": programs,
    }


def _dialogue_equates(disasm: Path) -> dict[str, int]:
    """Parse the named source constants used by the dialogue command boundary once."""
    source = read_upstream_text(disasm / "sf2enums.asm")
    values: dict[str, int] = {}
    for match in re.finditer(
        r"^(?P<name>[A-Za-z_][A-Za-z0-9_]*):\s+equ\s+(?P<value>\$?[0-9A-Fa-f]+)\b",
        source,
        re.MULTILINE,
    ):
        values[match.group("name")] = _literal(match.group("value"))
    missing = [name for name in DIALOGUE_CONSTANT_NAMES if name not in values]
    if missing:
        raise ValueError(f"dialogue source constants are missing: {missing}")
    return values


def _resolved_dialogue_operand(argument: str, constants: dict[str, int]) -> int:
    argument = argument.strip()
    if argument in constants:
        return constants[argument]
    try:
        return _literal(argument)
    except ValueError as error:
        raise ValueError(
            f"dialogue operand is not a literal or source constant: {argument}"
        ) from error


def _handler_by_name(handlers: list[dict[str, Any]], name: str) -> dict[str, Any]:
    rows = [row for row in handlers if row["name"] == name]
    if len(rows) != 1:
        raise ValueError(f"dialogue handler inventory is ambiguous: {name}")
    return rows[0]


def _next_statement(
    statements: list[str], start: int, pattern: str, *, owner: str
) -> tuple[int, re.Match[str]]:
    for index in range(start, len(statements)):
        match = re.fullmatch(pattern, statements[index])
        if match is not None:
            return index, match
    raise ValueError(f"{owner} statement is missing: {pattern}")


def _direct_call_sites(statements: list[str], target: str) -> list[int]:
    pattern = re.compile(
        rf"^(?:bsr|jsr)(?:\.[bwls])?\s+\(?{re.escape(target)}\)?(?:\.[bwls])?(?:\s|$)"
    )
    return [index for index, statement in enumerate(statements) if pattern.match(statement)]


def _stable_handler_statements(disasm: Path, handler: dict[str, Any]) -> list[str]:
    """Read one named handler section without relying on a file-wide fragment search."""
    source = read_upstream_text(disasm / handler["sourcePath"])
    match = re.search(
        rf"^{re.escape(handler['name'])}:\s*\n(?P<body>.*?)"
        rf"^\s*; End of function {re.escape(handler['name'])}\s*$",
        source,
        re.MULTILINE | re.DOTALL,
    )
    if match is None:
        raise ValueError(f"dialogue handler section is missing: {handler['name']}")
    statements = _statements(match.group("body"))
    if len(statements) != handler["statementCount"]:
        raise ValueError(f"dialogue handler statement inventory drift: {handler['name']}")
    return statements


def _signed_word(value: int) -> int:
    return value - 0x10000 if value & 0x8000 else value


def _dialogue_caller_breakdown(
    disasm: Path, handlers: list[dict[str, Any]], entity_dialogue_consumer: dict[str, Any]
) -> dict[str, Any]:
    """Inventory direct instruction targets and their resolved effective targets per handler."""
    caller_handlers = [
        _handler_by_name(handlers, name) for name in DIALOGUE_CALLER_HANDLER_NAMES
    ]
    bounded_source_paths = {handler["sourcePath"] for handler in caller_handlers}
    portrait_handler = _handler_by_name(handlers, PORTRAIT_HANDLER)
    if entity_dialogue_consumer["function"] != ENTITY_DIALOGUE_CONSUMER:
        raise ValueError("dialogue external consumer identity drift")
    target_resolutions = [
        {
            "instructionTarget": PORTRAIT_HANDLER,
            "effectiveTarget": portrait_handler["name"],
            "effectiveTargetSourcePath": portrait_handler["sourcePath"],
        },
        {
            "instructionTarget": ENTITY_DIALOGUE_CONSUMER,
            "effectiveTarget": entity_dialogue_consumer["function"],
            "effectiveTargetSourcePath": entity_dialogue_consumer["sourcePath"],
        },
    ]
    if [row["instructionTarget"] for row in target_resolutions] != list(DIALOGUE_CALLEE_TARGETS):
        raise ValueError("dialogue instruction target declaration drift")
    if len({row["effectiveTarget"] for row in target_resolutions}) != len(target_resolutions):
        raise ValueError("dialogue effective target declaration is ambiguous")
    for row in target_resolutions:
        row["effectiveTargetScope"] = (
            "internal"
            if row["effectiveTargetSourcePath"] in bounded_source_paths
            else "external"
        )

    effective_targets = [row["effectiveTarget"] for row in target_resolutions]
    caller_rows = []
    for handler in caller_handlers:
        statements = _stable_handler_statements(disasm, handler)
        instruction_counts = {
            target: len(_direct_call_sites(statements, target))
            for target in DIALOGUE_CALLEE_TARGETS
        }
        if any(count not in {0, 1} for count in instruction_counts.values()):
            raise ValueError(f"dialogue caller site count drift: {handler['name']}")
        effective_counts = {target: 0 for target in effective_targets}
        for resolution in target_resolutions:
            effective_counts[resolution["effectiveTarget"]] += instruction_counts[
                resolution["instructionTarget"]
            ]
        caller_rows.append(
            {
                "handler": handler["name"],
                "sourcePath": handler["sourcePath"],
                "instructionTargetSiteCounts": instruction_counts,
                "effectiveTargetSiteCounts": effective_counts,
            }
        )

    instruction_totals = {
        target: sum(row["instructionTargetSiteCounts"][target] for row in caller_rows)
        for target in DIALOGUE_CALLEE_TARGETS
    }
    effective_totals = {
        target: sum(row["effectiveTargetSiteCounts"][target] for row in caller_rows)
        for target in effective_targets
    }

    def scoped_totals(counts: dict[str, int], *, target_field: str, scope: str) -> dict[str, int]:
        scopes = {
            row[target_field]: row["effectiveTargetScope"] for row in target_resolutions
        }
        return {
            target: counts[target] if scopes[target] == scope else 0 for target in counts
        }

    return {
        "callerHandlers": caller_rows,
        "targetResolutions": target_resolutions,
        "instructionTargetTotals": instruction_totals,
        "effectiveTargetTotals": effective_totals,
        "internalInstructionTargetTotals": scoped_totals(
            instruction_totals, target_field="instructionTarget", scope="internal"
        ),
        "externalInstructionTargetTotals": scoped_totals(
            instruction_totals, target_field="instructionTarget", scope="external"
        ),
        "internalEffectiveTargetTotals": scoped_totals(
            effective_totals, target_field="effectiveTarget", scope="internal"
        ),
        "externalEffectiveTargetTotals": scoped_totals(
            effective_totals, target_field="effectiveTarget", scope="external"
        ),
    }


def _dialogue_handler_facts(
    disasm: Path,
    macros: dict[str, dict[str, Any]],
    dispatch_targets: list[str],
    handlers: list[dict[str, Any]],
    modifier_entity_byte_pairs: Counter[tuple[int, int]],
    entity_dialogue_consumer: dict[str, Any],
) -> tuple[list[dict[str, Any]], dict[str, Any], dict[str, Any]]:
    """Guard the named dialogue handlers from their smallest stable sections."""
    handler_facts = []
    sentinel_values = []
    for macro in DIALOGUE_MACROS:
        contract = macros[macro]
        if contract["kind"] != "command" or contract["aliasOf"] is not None:
            raise ValueError(f"dialogue macro is not a primary command: {macro}")
        opcode = contract["opcode"]
        if opcode is None or dispatch_targets[opcode] != DIALOGUE_HANDLER_BY_MACRO[macro]:
            raise ValueError(f"dialogue macro dispatcher target drift: {macro}")
        handler = _handler_by_name(handlers, DIALOGUE_HANDLER_BY_MACRO[macro])
        if handler["opcodes"] != [opcode]:
            raise ValueError(f"dialogue handler opcode inventory drift: {handler['name']}")
        if handler["encodedCommandBytes"] != contract["encodedBytes"]:
            raise ValueError(f"dialogue handler encoded-width drift: {handler['name']}")
        statements = _stable_handler_statements(disasm, handler)
        if macro in DIALOGUE_DISPLAY_MACROS:
            skip_index = next(
                (
                    index
                    for index, statement in enumerate(statements)
                    if statement == "tst.b ((SKIP_CUTSCENE_TEXT-$1000000)).w"
                ),
                None,
            )
            skip_guard = None
            expects_skip_guard = macro in {"nextSingleText", "nextText"}
            if (skip_index is not None) != expects_skip_guard:
                raise ValueError(f"dialogue skip-guard admission drift: {handler['name']}")
            if skip_index is not None:
                branch_index, branch = _next_statement(
                    statements,
                    skip_index + 1,
                    r"bne\.[bwls]\s+\S+",
                    owner=handler["name"],
                )
                if branch_index != skip_index + 1:
                    raise ValueError(f"dialogue skip-guard order drift: {handler['name']}")
                skip_guard = {
                    "predicate": statements[skip_index],
                    "branch": branch.group(0),
                }

            sentinel_index, sentinel = _next_statement(
                statements,
                0,
                r"cmpi\.w\s+#(?P<value>-?\$?[0-9A-Fa-f]+),\(a6\)",
                owner=handler["name"],
            )
            branch_index, branch = _next_statement(
                statements,
                sentinel_index + 1,
                r"beq\.[bwls]\s+\S+",
                owner=handler["name"],
            )
            if branch_index != sentinel_index + 1:
                raise ValueError(f"dialogue sentinel-branch order drift: {handler['name']}")
            sentinel_value = _literal(sentinel.group("value")) & 0xFFFF
            sentinel_values.append(sentinel_value)
            portrait_sites = _direct_call_sites(statements, PORTRAIT_HANDLER)
            consumer_sites = _direct_call_sites(statements, ENTITY_DIALOGUE_CONSUMER)
            if len(portrait_sites) != 1 or len(consumer_sites) != 1:
                raise ValueError(f"dialogue helper call count drift: {handler['name']}")
            if portrait_sites[0] >= consumer_sites[0]:
                raise ValueError(f"dialogue helper call order drift: {handler['name']}")
            increment_index, _ = _next_statement(
                statements,
                consumer_sites[0] + 1,
                r"addq\.w\s+#1,\(\(CUTSCENE_DIALOG_INDEX-\$1000000\)\)\.w",
                owner=handler["name"],
            )
            display_index, _ = _next_statement(
                statements,
                consumer_sites[0] + 1,
                r"jsr\s+\(DisplayText\)\.l",
                owner=handler["name"],
            )
            if display_index >= increment_index:
                raise ValueError(f"dialogue display/index increment order drift: {handler['name']}")
            name_index_statements = [
                statement
                for statement in statements
                if statement
                in {
                    "move.w (a6)+,((DIALOGUE_NAME_INDEX_1-$1000000)).w",
                    "move.w (a6)+,((DIALOGUE_NAME_INDEX_2-$1000000)).w",
                }
            ]
            if macro.endswith("Var"):
                if name_index_statements != [
                    "move.w (a6)+,((DIALOGUE_NAME_INDEX_1-$1000000)).w",
                    "move.w (a6)+,((DIALOGUE_NAME_INDEX_2-$1000000)).w",
                ]:
                    raise ValueError(
                        f"dialogue variable name-word consumption drift: {handler['name']}"
                    )
            elif name_index_statements:
                raise ValueError(f"dialogue fixed handler consumes name words: {handler['name']}")
            is_single = macro.startswith("nextSingle")
            close_sequence = [
                "jsr j_ClosePortraitWindow",
                "clsTxt",
                "moveq #10,d0",
                "jsr (Sleep).w",
            ]
            if is_single:
                cursor = increment_index + 1
                for statement in close_sequence:
                    cursor, _ = _next_statement(
                        statements, cursor, re.escape(statement), owner=handler["name"]
                    )
                    cursor += 1
            elif any(statement in statements for statement in close_sequence):
                raise ValueError(f"dialogue continuing close/sleep shape drift: {handler['name']}")
            handler_facts.append(
                {
                    "macro": macro,
                    "handler": handler["name"],
                    "address": handler["address"],
                    "opcode": opcode,
                    "skipGuard": skip_guard,
                    "modifierEntityWordSentinel": {
                        "unsignedValue": sentinel_value,
                        "signedValue": _signed_word(sentinel_value),
                        "branch": branch.group(0),
                    },
                    "nameWordDestinationCount": len(name_index_statements),
                    "displayThenIndexIncrement": True,
                    "singleCloseSleepSequence": is_single,
                }
            )
        elif macro == "textCursor":
            cursor_index, _ = _next_statement(
                statements,
                0,
                r"move\.w\s+\(a6\)\+,\(\(CUTSCENE_DIALOG_INDEX-\$1000000\)\)\.w",
                owner=handler["name"],
            )
            if cursor_index != 0:
                raise ValueError("dialogue text-index write is not the first handler statement")
            handler_facts.append(
                {
                    "macro": macro,
                    "handler": handler["name"],
                    "address": handler["address"],
                    "opcode": opcode,
                    "cursorWrite": statements[cursor_index],
                }
            )
        else:
            close_index, _ = _next_statement(
                statements,
                0,
                r"jsr\s+j_ClosePortraitWindow",
                owner=handler["name"],
            )
            clear_index, _ = _next_statement(
                statements, close_index + 1, r"clsTxt", owner=handler["name"]
            )
            if (close_index, clear_index) != (0, 1):
                raise ValueError("dialogue hide-window call order drift")
            handler_facts.append(
                {
                    "macro": macro,
                    "handler": handler["name"],
                    "address": handler["address"],
                    "opcode": opcode,
                    "closeThenClear": True,
                }
            )

    if len(set(sentinel_values)) != 1:
        raise ValueError("dialogue handlers disagree on modifier/entity word sentinel")
    sentinel_value = sentinel_values[0]

    portrait_handler = _handler_by_name(handlers, PORTRAIT_HANDLER)
    portrait_statements = _stable_handler_statements(disasm, portrait_handler)
    word_read_index, _ = _next_statement(
        portrait_statements,
        0,
        r"move\.w\s+\(a6\)\+,d0",
        owner=PORTRAIT_HANDLER,
    )
    bit_rows = []
    cursor = word_read_index + 1
    for destination in ("d3", "d4"):
        zero_index, _ = _next_statement(
            portrait_statements,
            cursor,
            rf"moveq\s+#0,{destination}",
            owner=PORTRAIT_HANDLER,
        )
        bit_index, bit_match = _next_statement(
            portrait_statements,
            zero_index + 1,
            r"btst\s+#(?P<bit>\$?[0-9A-Fa-f]+),d0",
            owner=PORTRAIT_HANDLER,
        )
        branch_index, _ = _next_statement(
            portrait_statements,
            bit_index + 1,
            r"beq\.[bwls]\s+\S+",
            owner=PORTRAIT_HANDLER,
        )
        set_index, _ = _next_statement(
            portrait_statements,
            branch_index + 1,
            rf"moveq\s+#-1,{destination}",
            owner=PORTRAIT_HANDLER,
        )
        if not (zero_index < bit_index < branch_index < set_index):
            raise ValueError(f"portrait modifier branch order drift: {destination}")
        bit_rows.append({"bit": _literal(bit_match.group("bit")), "destination": destination})
        cursor = set_index + 1
    handler_tested_modifier_byte_mask = 0
    for row in bit_rows:
        byte_bit = row["bit"] - 8
        if not 0 <= byte_bit <= 7:
            raise ValueError("portrait modifier bit is outside the packed modifier byte")
        handler_tested_modifier_byte_mask |= 1 << byte_bit
    if len({row["bit"] for row in bit_rows}) != len(bit_rows):
        raise ValueError("portrait modifier bit test is duplicated")
    full_word_sentinel_bytes = (sentinel_value >> 8, sentinel_value & 0xFF)
    for modifier, entity in modifier_entity_byte_pairs:
        if (modifier, entity) == full_word_sentinel_bytes:
            continue
        if modifier & ~handler_tested_modifier_byte_mask:
            raise ValueError(
                "dialogue non-sentinel modifier byte exceeds handler-tested modifier byte mask"
            )
    consumer_sites = _direct_call_sites(portrait_statements, ENTITY_DIALOGUE_CONSUMER)
    if len(consumer_sites) != 1:
        raise ValueError("portrait helper consumer call count drift")
    if consumer_sites[0] < cursor:
        raise ValueError("portrait helper consumer call order drift")
    return (
        handler_facts,
        {
            "handler": PORTRAIT_HANDLER,
            "address": portrait_handler["address"],
            "sourcePath": portrait_handler["sourcePath"],
            "modifierEntityWordRead": portrait_statements[word_read_index],
            "handlerTestedModifierByteMask": handler_tested_modifier_byte_mask,
            "modifierBitTests": bit_rows,
        },
        _dialogue_caller_breakdown(disasm, handlers, entity_dialogue_consumer),
    )


def _entity_dialogue_consumer_facts(
    disasm: Path, constants: dict[str, int], addresses: dict[str, int]
) -> dict[str, Any]:
    source = read_upstream_text(disasm / ENTITY_DIALOGUE_CONSUMER_PATH)
    function = re.search(
        rf"^{ENTITY_DIALOGUE_CONSUMER}:\s*\n(?P<body>.*?)"
        rf"^\s*; End of function {ENTITY_DIALOGUE_CONSUMER}\s*$",
        source,
        re.MULTILINE | re.DOTALL,
    )
    if function is None:
        raise ValueError("entity dialogue consumer function is missing")
    statements = _statements(function.group("body"))
    mask_index, mask = _next_statement(
        statements,
        0,
        r"andi\.w\s+#(?P<name>[A-Za-z_][A-Za-z0-9_]*),d0",
        owner=ENTITY_DIALOGUE_CONSUMER,
    )
    constant_name = mask.group("name")
    if constant_name != "COMBATANT_MASK_ALL" or constant_name not in constants:
        raise ValueError("entity dialogue consumer low-domain mask drift")
    entity_call_index, _ = _next_statement(
        statements,
        mask_index + 1,
        r"bsr\.w\s+GetEntityAddressFromCharacter",
        owner=ENTITY_DIALOGUE_CONSUMER,
    )
    map_sprite_index, _ = _next_statement(
        statements,
        entity_call_index + 1,
        r"move\.b\s+ENTITYDEF_OFFSET_MAPSPRITE\(a5\),d0",
        owner=ENTITY_DIALOGUE_CONSUMER,
    )
    if not (mask_index < entity_call_index < map_sprite_index):
        raise ValueError("entity dialogue consumer low-domain order drift")
    if ENTITY_DIALOGUE_CONSUMER not in addresses:
        raise ValueError("entity dialogue consumer H1 address is missing")
    return {
        "function": ENTITY_DIALOGUE_CONSUMER,
        "address": addresses[ENTITY_DIALOGUE_CONSUMER],
        "sourcePath": ENTITY_DIALOGUE_CONSUMER_PATH.as_posix(),
        "lowDomainMask": {"constant": constant_name, "value": constants[constant_name]},
        "mapSpriteLoad": statements[map_sprite_index],
    }


def _modifier_source_labels(
    disasm: Path, modifier_bit_tests: list[dict[str, Any]], sentinel_value: int
) -> list[dict[str, Any]]:
    """Retain the macro's original modifier labels without treating them as new semantics."""
    blocks = _macro_blocks(read_upstream_text(disasm / MACRO_PATH))
    labels_by_macro: list[dict[int, str]] = []
    for macro in DIALOGUE_MODIFIER_MACROS:
        body = blocks.get(macro)
        if body is None:
            raise ValueError(f"dialogue modifier macro is missing: {macro}")
        match = re.search(r"dc\.b\s+\\1\s*;\s*portrait modifier \(([^)]+)\)", body)
        if match is None:
            raise ValueError(f"dialogue modifier labels are missing: {macro}")
        labels = {}
        for entry in match.group(1).split(","):
            value, label = entry.strip().split("-", 1)
            labels[_literal(value)] = label
        labels_by_macro.append(labels)
    if labels_by_macro[0] != labels_by_macro[1]:
        raise ValueError("dialogue modifier labels disagree between source macros")
    labels = labels_by_macro[0]
    full_word_sentinel_high_byte = sentinel_value >> 8
    expected_by_bit = {bit - 8: bit for bit in (row["bit"] for row in modifier_bit_tests)}
    result = []
    for value, label in sorted(labels.items()):
        row = {
            "modifierByteValue": value,
            "sourceLabel": label,
            "handlerWordBit": None,
        }
        if value not in {0, full_word_sentinel_high_byte}:
            if value <= 0 or value & (value - 1) or value.bit_length() - 1 not in expected_by_bit:
                raise ValueError("dialogue modifier label no longer matches a handler bit test")
            row["handlerWordBit"] = expected_by_bit[value.bit_length() - 1]
        result.append(row)
    if full_word_sentinel_high_byte not in labels or 0 not in labels:
        raise ValueError("dialogue modifier label boundary drift")
    return result


def _dialogue_command_facts(
    disasm: Path,
    macros: dict[str, dict[str, Any]],
    dispatch_targets: list[str],
    handlers: list[dict[str, Any]],
    program_corpus: dict[str, Any],
    addresses: dict[str, int],
    rom_path: Path,
    upstream_path: Path,
) -> dict[str, Any]:
    """Build the dialogue command contract from program references and source use sites."""
    constants = _dialogue_equates(disasm)
    programs = program_corpus["programs"]
    source_references = []
    program_totals = []
    source_counts: Counter[str] = Counter()
    modifier_values: Counter[int] = Counter()
    modifier_entity_byte_pairs: Counter[tuple[int, int]] = Counter()
    entity_values: Counter[int] = Counter()
    text_cursor_values: Counter[int] = Counter()
    for program in programs:
        counts: Counter[str] = Counter()
        command_indexes = []
        for command in program["commands"]:
            macro = command["macro"]
            if macro not in DIALOGUE_MACROS:
                continue
            command_indexes.append(command["index"])
            source_counts[macro] += 1
            counts[macro] += 1
            arguments = command["arguments"]
            if macro in DIALOGUE_DISPLAY_MACROS:
                if len(arguments) < 2:
                    raise ValueError(f"dialogue display command lacks modifier/entity: {macro}")
                modifier = _resolved_dialogue_operand(arguments[0], constants)
                entity = _resolved_dialogue_operand(arguments[1], constants)
                if not 0 <= modifier <= 0xFF or not 0 <= entity <= 0xFF:
                    raise ValueError(f"dialogue modifier/entity byte domain drift: {macro}")
                modifier_values[modifier] += 1
                modifier_entity_byte_pairs[(modifier, entity)] += 1
                entity_values[entity] += 1
            elif macro == "textCursor":
                if len(arguments) != 1:
                    raise ValueError("dialogue text cursor operand count drift")
                text_cursor_values[_resolved_dialogue_operand(arguments[0], constants)] += 1
            elif arguments:
                raise ValueError("dialogue hide command unexpectedly has operands")
        program_totals.append(
            {
                "programId": program["id"],
                "commandCount": sum(counts.values()),
                "macroCounts": {name: counts[name] for name in DIALOGUE_MACROS},
            }
        )
        if command_indexes:
            source_references.append(
                {"programId": program["id"], "commandIndexes": command_indexes}
            )
    if len(program_totals) != program_corpus["summary"]["programCount"]:
        raise ValueError("dialogue zero-inclusive program total coverage drift")
    if sum(len(row["commandIndexes"]) for row in source_references) != sum(source_counts.values()):
        raise ValueError("dialogue source reference count drift")
    flattened_references = [
        (row["programId"], command_index)
        for row in source_references
        for command_index in row["commandIndexes"]
    ]
    if len(set(flattened_references)) != len(flattened_references):
        raise ValueError("dialogue source reference identity drift")
    for name in DIALOGUE_MACROS:
        if sum(row["macroCounts"][name] for row in program_totals) != source_counts[name]:
            raise ValueError(f"dialogue per-program total drift: {name}")

    text_line_domain = build_text_line_domain_contract(rom_path, upstream_path)
    domain = text_line_domain["gamescriptFacts"]
    if not text_cursor_values:
        raise ValueError("dialogue text-cursor source use is absent")
    if (
        min(text_cursor_values) < domain["firstLineId"]
        or max(text_cursor_values) > domain["lastLineId"]
    ):
        raise ValueError("dialogue text-cursor value is outside the source text-line domain")

    sprite_dialogue = build_sprite_dialogue_contract(rom_path, upstream_path)
    if (
        sprite_dialogue["upstream"]["commit"] != text_line_domain["upstream"]["commit"]
        or sprite_dialogue["romSha256"] != text_line_domain["romSha256"]
        or sprite_dialogue["summary"]["rowCount"] != 119
    ):
        raise ValueError("dialogue sprite-property contract provenance or row boundary drift")

    entity_dialogue_consumer = _entity_dialogue_consumer_facts(disasm, constants, addresses)
    handler_facts, portrait_helper, caller_breakdown = _dialogue_handler_facts(
        disasm,
        macros,
        dispatch_targets,
        handlers,
        modifier_entity_byte_pairs,
        entity_dialogue_consumer,
    )
    modifier_source_labels = _modifier_source_labels(
        disasm,
        portrait_helper["modifierBitTests"],
        handler_facts[0]["modifierEntityWordSentinel"]["unsignedValue"],
    )
    selected_macros = []
    for name in DIALOGUE_MACROS:
        contract = macros[name]
        selected_macros.append(
            {
                "name": name,
                "opcode": contract["opcode"],
                "encodedBytes": contract["encodedBytes"],
                "operandBytes": contract["operandBytes"],
                "operandLayout": contract["operandLayout"],
                "parameterOrdinals": contract["parameterOrdinals"],
                "handler": DIALOGUE_HANDLER_BY_MACRO[name],
                "handlerAddress": _handler_by_name(
                    handlers, DIALOGUE_HANDLER_BY_MACRO[name]
                )["address"],
                "sourceCommandCount": source_counts[name],
            }
        )
    return {
        "macros": selected_macros,
        "sourceSiteReferences": source_references,
        "programTotals": program_totals,
        "operandFacts": {
            "constants": {name: constants[name] for name in DIALOGUE_CONSTANT_NAMES},
            "modifierByteCounts": [
                {"value": value, "count": modifier_values[value]}
                for value in sorted(modifier_values)
            ],
            "modifierSourceLabels": modifier_source_labels,
            "entityByteCounts": [
                {"value": value, "count": entity_values[value]} for value in sorted(entity_values)
            ],
            "textCursorValueCounts": [
                {"value": value, "count": text_cursor_values[value]}
                for value in sorted(text_cursor_values)
            ],
            "textCursorValueBounds": {
                "minimum": min(text_cursor_values),
                "maximum": max(text_cursor_values),
                "domainMinimum": domain["firstLineId"],
                "domainMaximum": domain["lastLineId"],
            },
        },
        "handlers": handler_facts,
        "portraitHelper": portrait_helper,
        "callerBreakdown": caller_breakdown,
        "entityDialogueConsumer": entity_dialogue_consumer,
        "textLineDomain": {
            "contractId": text_line_domain["id"],
            "upstreamCommit": text_line_domain["upstream"]["commit"],
            "romSha256": text_line_domain["romSha256"],
            "sourcePath": domain["sourcePath"],
            "lineIdCount": domain["lineIdCount"],
            "firstLineId": domain["firstLineId"],
            "lastLineId": domain["lastLineId"],
            "idsAreContiguous": domain["idsAreContiguous"],
        },
        "spriteDialogueTable": {
            "contractId": sprite_dialogue["id"],
            "upstreamCommit": sprite_dialogue["upstream"]["commit"],
            "romSha256": sprite_dialogue["romSha256"],
            "tableAddress": sprite_dialogue["table"]["table_MapspriteDialogueProperties"],
            "consumerAddress": sprite_dialogue["table"][ENTITY_DIALOGUE_CONSUMER],
            "rowCount": sprite_dialogue["summary"]["rowCount"],
            "recordByteCount": sprite_dialogue["summary"]["recordByteCount"],
            "tableByteCount": sprite_dialogue["summary"]["tableByteCount"],
            "sourcePath": sprite_dialogue["romRange"]["sourcePath"],
        },
        "runtimeQuestions": DIALOGUE_RUNTIME_QUESTIONS,
    }


def build_map_script_engine_contract(
    rom_path: Path, upstream_path: Path
) -> dict[str, Any]:
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"map-script H1 listing is missing: {listing_path}")
    addresses = listing_symbol_addresses(listing_path.read_text(encoding="utf-8"))
    rom = rom_path.read_bytes()
    macros = _map_macro_contracts(disasm)
    source_counts, opcode_counts, source_paths = _source_usage(disasm, macros)
    program_corpus = _program_corpus(disasm, source_paths, macros, addresses)
    if program_corpus["summary"]["commandCount"] != sum(source_counts.values()):
        raise ValueError("map-script program ownership does not cover every source command")
    dispatch_source = read_upstream_text(disasm / DISPATCH_SOURCE)
    targets = _dispatch_targets(dispatch_source)
    handlers = _handler_rows(disasm, addresses, targets, source_counts, macros)
    dialogue_command_facts = _dialogue_command_facts(
        disasm,
        macros,
        targets,
        handlers,
        program_corpus,
        addresses,
        rom_path,
        upstream_path,
    )
    table_address = addresses["rjt_cutsceneScriptCommands"]
    table_bytes = rom[table_address : table_address + len(targets) * 2]
    expected_words = b"".join(
        ((addresses[target] - table_address) & 0xFFFF).to_bytes(2, "big")
        for target in targets
    )
    if table_bytes != expected_words:
        raise ValueError("map-script dispatcher source/ROM parity drift")

    primary = {
        name: row
        for name, row in macros.items()
        if row["kind"] == "command" and row["aliasOf"] is None
    }
    aliases = {name: row for name, row in macros.items() if row["aliasOf"] is not None}
    filler_indices = [index for index, target in enumerate(targets) if target == "csc_doNothing"]
    if {row["opcode"] for row in primary.values()} != set(range(90)) - set(
        filler_indices
    ):
        raise ValueError("map-script primary macros do not cover every non-filler opcode")
    family_counts = Counter(
        family for handler in handlers for family in handler["families"]
    )
    command_width_counts = Counter(row["encodedBytes"] for row in primary.values())
    handler_flow_counts = Counter(row["cursorFlow"] for row in handlers)
    summary = {
        "dispatcherSlotCount": len(targets),
        "uniqueHandlerCount": len(handlers),
        "fillerSlotCount": len(filler_indices),
        "primaryCommandMacroCount": len(primary),
        "aliasMacroCount": len(aliases),
        "specialMacroCount": sum(row["kind"] != "command" for row in macros.values()),
        "trackedMacroCount": len(macros),
        "usedMacroCount": sum(source_counts[name] > 0 for name in macros),
        "unusedMacroCount": sum(source_counts[name] == 0 for name in macros),
        "sourceCommandCount": sum(source_counts.values()),
        "sourceFileCount": len(source_paths),
        "handlerStatementCount": sum(row["statementCount"] for row in handlers),
        "handlerEntityFieldCount": len(
            {
                access["name"]
                for row in handlers
                for access in row["entityFieldAccesses"]
            }
        ),
        "handlerGlobalStateCount": len(
            {
                access["name"]
                for row in handlers
                for access in row["globalStateAccesses"]
            }
        ),
        "handlerDirectCallTargetCount": len(
            {call for row in handlers for call in row["directCalls"]}
        ),
        "primaryLogicalParameterCount": sum(
            len(row["parameterOrdinals"]) for row in primary.values()
        ),
        "primaryOperandFieldCount": sum(
            len(row["operandLayout"]) for row in primary.values()
        ),
        "primaryOperandByteCount": sum(row["operandBytes"] for row in primary.values()),
        "sequentialHandlerCount": handler_flow_counts["sequential"],
        "programCount": program_corpus["summary"]["programCount"],
        "programLabelCount": program_corpus["summary"]["programLabelCount"],
        "programTransferCount": program_corpus["summary"]["transferCount"],
        "encodedCommandByteCount": program_corpus["summary"][
            "encodedCommandByteCount"
        ],
        "referencedProgramCount": program_corpus["referenceSummary"][
            "referencedProgramCount"
        ],
        "unreferencedProgramCount": program_corpus["referenceSummary"][
            "unreferencedProgramCount"
        ],
        "statefulProgramCount": program_corpus["storyState"]["summary"][
            "statefulProgramCount"
        ],
        "storyReadFlagCount": program_corpus["storyState"]["summary"][
            "uniqueReadFlagCount"
        ],
        "storyWriteFlagCount": program_corpus["storyState"]["summary"][
            "uniqueWriteFlagCount"
        ],
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "rom": {"sha256": inspect_rom(rom_path)["sha256"]},
        "summary": summary,
        "function": {
            "ExecuteMapScript": addresses["ExecuteMapScript"],
            "rjt_cutsceneScriptCommands": table_address,
            **{row["name"]: row["address"] for row in handlers},
        },
        "dispatcher": {
            "address": table_address,
            "endExclusive": table_address + len(table_bytes),
            "sha256": hashlib.sha256(table_bytes).hexdigest().upper(),
            "targets": targets,
            "fillerTarget": "csc_doNothing",
            "fillerIndices": filler_indices,
            "sourceRomParity": True,
        },
        "familyCounts": dict(sorted(family_counts.items())),
        "abiFacts": {
            "commandWidthCounts": {
                str(width): count for width, count in sorted(command_width_counts.items())
            },
            "handlerFlowCounts": dict(sorted(handler_flow_counts.items())),
            "absoluteJumpHandlers": sorted(
                row["name"] for row in handlers if row["cursorFlow"] == "absolute-jump"
            ),
            "conditionalAbsoluteJumpHandlers": sorted(
                row["name"]
                for row in handlers
                if row["cursorFlow"] == "conditional-absolute-jump"
            ),
            "inlineActionProgramHandlers": sorted(
                row["name"]
                for row in handlers
                if row["cursorFlow"] == "inline-action-program"
            ),
            "shorthandOperands": [
                {
                    "macro": name,
                    "streamOffset": operand["streamOffset"],
                    "widthBytes": operand["widthBytes"],
                    "encoding": operand["encoding"],
                }
                for name, row in sorted(primary.items())
                for operand in row["operandLayout"]
                if operand["encoding"].startswith("shorthand:")
            ],
        },
        "macroContracts": {name: macros[name] for name in sorted(macros)},
        "macroSourceCounts": dict(sorted(source_counts.items())),
        "opcodeSourceCounts": opcode_counts,
        "unusedMacros": sorted(name for name in macros if source_counts[name] == 0),
        "handlers": handlers,
        "programCorpus": program_corpus,
        "dialogueCommandFacts": dialogue_command_facts,
        "runtimeQuestions": [
            "caller-dependent-story-branch-reachability-and-persistence",
            "entity-camera-text-wait-and-transition-frame-timing",
            "palette-fade-and-vdp-visible-presentation",
        ],
    }


def verify_map_script_engine_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner="map-script engine fixture")
    manifest = load_json(MANIFEST)
    output = build_map_script_engine_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="map-script engine static contract")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != output["rom"]["sha256"]
        or any(output["function"][name] != address for name, address in fixture["function"].items())
    ):
        raise ValueError("map-script engine provenance/address drift")
    program_fields = {
        "programSummary": output["programCorpus"]["summary"],
        "transferCounts": output["programCorpus"]["transferCounts"],
        "referenceSummary": output["programCorpus"]["referenceSummary"],
        "unreferencedPrograms": output["programCorpus"]["unreferencedPrograms"],
        "sourceOnlyPrograms": output["programCorpus"]["sourceOnlyPrograms"],
        "largestPrograms": output["programCorpus"]["largestPrograms"],
        "storyStateSummary": output["programCorpus"]["storyState"]["summary"],
        "storyStateConstants": output["programCorpus"]["storyState"]["constants"],
        "storyReadFlagCounts": output["programCorpus"]["storyState"][
            "readFlagCounts"
        ],
        "storyReadWriteOverlapFlags": output["programCorpus"]["storyState"][
            "readWriteOverlapFlags"
        ],
        "storyDirectSetFlags": output["programCorpus"]["storyState"][
            "directSetFlags"
        ],
        "storyDirectClearFlags": output["programCorpus"]["storyState"][
            "directClearFlags"
        ],
        "storyBattleUnlockFlags": output["programCorpus"]["storyState"][
            "battleUnlockFlags"
        ],
        "dialogueCommandFacts": output["dialogueCommandFacts"],
    }
    for field in (
        "summary",
        "dispatcherFacts",
        "familyCounts",
        "abiFacts",
        "programSummary",
        "transferCounts",
        "referenceSummary",
        "unreferencedPrograms",
        "sourceOnlyPrograms",
        "largestPrograms",
        "storyStateSummary",
        "storyStateConstants",
        "storyReadFlagCounts",
        "storyReadWriteOverlapFlags",
        "storyDirectSetFlags",
        "storyDirectClearFlags",
        "storyBattleUnlockFlags",
        "dialogueCommandFacts",
        "mostUsedMacros",
        "unusedMacros",
        "runtimeQuestions",
    ):
        actual = program_fields.get(field)
        if field == "dispatcherFacts":
            actual = {
                "sha256": output["dispatcher"]["sha256"],
                "fillerTarget": output["dispatcher"]["fillerTarget"],
                "fillerIndices": output["dispatcher"]["fillerIndices"],
                "sourceRomParity": output["dispatcher"]["sourceRomParity"],
            }
        elif field == "mostUsedMacros":
            actual = [
                {"macro": name, "count": count}
                for name, count in sorted(
                    output["macroSourceCounts"].items(),
                    key=lambda item: (-item[1], item[0]),
                )[:12]
            ]
        elif field not in program_fields:
            actual = output[field]
        if fixture["expected"][field] != actual:
            raise ValueError(f"map-script engine {field} drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"] or output["summary"] != manifest["summary"]:
        raise ValueError("map-script engine canonical output drift")
    destination = output_path or repo_path("local/derived/map-script-engine-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Slots": output["summary"]["dispatcherSlotCount"],
        "Handlers": output["summary"]["uniqueHandlerCount"],
        "Macros": output["summary"]["trackedMacroCount"],
        "SourceCommands": output["summary"]["sourceCommandCount"],
        "Status": "PASS",
    }
