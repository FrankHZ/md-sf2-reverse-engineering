from __future__ import annotations

import hashlib
import json
import re
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.h2.entity_action_scripts import _access_rows, _global_access_rows
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
    for field in (
        "summary",
        "dispatcherFacts",
        "familyCounts",
        "abiFacts",
        "programSummary",
        "transferCounts",
        "sourceOnlyPrograms",
        "largestPrograms",
        "mostUsedMacros",
        "unusedMacros",
        "runtimeQuestions",
    ):
        actual = (
            output["programCorpus"]["summary"]
            if field == "programSummary"
            else output["programCorpus"][field]
            if field in {"transferCounts", "sourceOnlyPrograms", "largestPrograms"}
            else
            {
                "sha256": output["dispatcher"]["sha256"],
                "fillerTarget": output["dispatcher"]["fillerTarget"],
                "fillerIndices": output["dispatcher"]["fillerIndices"],
                "sourceRomParity": output["dispatcher"]["sourceRomParity"],
            }
            if field == "dispatcherFacts"
            else [
                {"macro": name, "count": count}
                for name, count in sorted(
                    output["macroSourceCounts"].items(),
                    key=lambda item: (-item[1], item[0]),
                )[:12]
            ]
            if field == "mostUsedMacros"
            else output[field]
        )
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
