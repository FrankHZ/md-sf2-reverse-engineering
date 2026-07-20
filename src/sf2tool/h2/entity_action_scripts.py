from __future__ import annotations

import hashlib
import json
import re
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.rom import inspect_rom
from sf2tool.source_text import read_upstream_text

ID = "sf2-entity-action-scripts-static-v1"
MANIFEST = repo_path("manifests/extractions/entity-action-scripts-static.json")
SCHEMA = repo_path("schemas/entity-action-scripts-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/entity-action-scripts-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-entity-action-scripts-static-fixture.schema.json")

MACRO_PATH = Path("sf2cutscenemacros.asm")
HANDLER_SOURCE_PATH = Path("code/common/scripting/entity/entityscriptengine_2.asm")
CORPORA = (
    {
        "id": "battle-neutral",
        "path": Path("data/scripting/entity/eas_battleneutralentities.asm"),
        "start": 0x4497A,
        "end": 0x449C6,
        "bindingSymbol": "eas_LyingLeft",
    },
    {
        "id": "main",
        "path": Path("data/scripting/entity/eas_main.asm"),
        "start": 0x44DE2,
        "end": 0x45204,
        "bindingSymbol": "word_44DEA",
    },
    {
        "id": "cutscene-actions",
        "path": Path("data/scripting/entity/eas_actions.asm"),
        "start": 0x45E44,
        "end": 0x46506,
        "bindingSymbol": "eas_Jump",
    },
)

COMMAND_PATTERN = re.compile(
    r"^\s*(?:([A-Za-z_][A-Za-z0-9_]*):\s*)?(ac_[A-Za-z0-9_]+)\b(.*)$"
)
CUSTOM_PROGRAM_PATTERN = re.compile(
    r"^\s*(?:([A-Za-z_][A-Za-z0-9_]*):\s*)?"
    r"(customActscriptWait|customActscript)\b(.*)$"
)
LABEL_PATTERN = re.compile(r"^\s*([A-Za-z_][A-Za-z0-9_]*):")
LABEL_ONLY_PATTERN = re.compile(r"^\s*([A-Za-z_][A-Za-z0-9_]*):\s*$")
BRANCH_WORD_PATTERN = re.compile(
    r"^\s*dc\.w\s+\(\s*([A-Za-z_][A-Za-z0-9_]*)\s*-\s*"
    r"([A-Za-z_][A-Za-z0-9_]*)\s*\)\s*&\s*\$FFFF\s*$"
)

HANDLER_FAMILY_BY_OPCODE = {
    **{opcode: "wait" for opcode in (0x00, 0x01, 0x0F)},
    **{opcode: "direct-control" for opcode in (0x02, 0x07, 0x08)},
    **{opcode: "movement" for opcode in (0x03, 0x04, 0x05, 0x06, 0x09, 0x0C, 0x0D, 0x0E)},
    **{opcode: "motion-state" for opcode in range(0x10, 0x15)},
    **{opcode: "entity-property" for opcode in (0x0A, 0x0B, *range(0x15, 0x23))},
    0x23: "audio",
    **{opcode: "control-flow" for opcode in range(0x30, 0x35)},
    0x40: "map-effect",
    0x41: "control-flow",
}


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _macro_contracts(disasm: Path) -> dict[str, dict[str, Any]]:
    source = read_upstream_text(disasm / MACRO_PATH)
    contracts = {}
    pattern = re.compile(
        r"^(ac_[A-Za-z0-9_]+):\s*macro\s*\n(.*?)(?=^\s*endm\s*$)",
        re.MULTILINE | re.DOTALL,
    )
    for match in pattern.finditer(source):
        body = match.group(2)
        first_word = re.search(r"^\s*dc\.w\s+(\$?[0-9A-Fa-f]+)\b", body, re.MULTILINE)
        if first_word is None:
            raise ValueError(f"entity-action macro has no literal opcode: {match.group(1)}")
        token = first_word.group(1)
        opcode = int(token[1:], 16) if token.startswith("$") else int(token)
        offset = 0
        parameters = []
        for raw_line in body.splitlines():
            directive = re.match(
                r"^\s*dc\.(?P<size>[bwl])\s+(?P<expression>[^;]+?)"
                r"(?:\s*;\s*(?P<comment>.*))?$",
                raw_line,
            )
            if directive is None:
                continue
            width = {"b": 1, "w": 2, "l": 4}[directive.group("size")]
            expression = directive.group("expression").strip()
            ordinal = re.search(r"\\(\d+)", expression)
            if ordinal:
                parameters.append(
                    {
                        "ordinal": int(ordinal.group(1)),
                        "offset": offset,
                        "widthBytes": width,
                        "expression": expression,
                        "role": (directive.group("comment") or "").strip(),
                    }
                )
            offset += width
        contracts[match.group(1)] = {
            "opcode": opcode,
            "encodedBytes": offset,
            "parameters": parameters,
        }
    if len(contracts) != 44:
        raise ValueError(f"entity-action macro boundary drift: {len(contracts)}")
    return contracts


def _parse_corpus(
    disasm: Path,
    rom: bytes,
    addresses: dict[str, int],
    macro_contracts: dict[str, dict[str, Any]],
    spec: dict[str, Any],
) -> dict[str, Any]:
    source = read_upstream_text(disasm / spec["path"])
    labels = []
    commands = []
    active_label = "$range-start"
    command_pattern = re.compile(
        r"^\s*(?:([A-Za-z_][A-Za-z0-9_]*):\s*)?(ac_[A-Za-z0-9_]+)\b(.*)$"
    )
    label_pattern = re.compile(r"^([A-Za-z_][A-Za-z0-9_]*):")
    for line_number, raw_line in enumerate(source.splitlines(), 1):
        code = raw_line.split(";", 1)[0].rstrip()
        label_match = label_pattern.match(code)
        if label_match:
            active_label = label_match.group(1)
            if active_label not in addresses:
                raise ValueError(f"entity-action label lacks H1 address: {active_label}")
            labels.append(
                {
                    "name": active_label,
                    "address": addresses[active_label],
                    "line": line_number,
                }
            )
        command_match = command_pattern.match(code)
        if not command_match:
            continue
        inline_label, macro, arguments = command_match.groups()
        if inline_label is not None:
            active_label = inline_label
        if macro not in macro_contracts:
            raise ValueError(f"undefined entity-action macro at line {line_number}: {macro}")
        commands.append(
            {
                "line": line_number,
                "ownerLabel": active_label,
                "macro": macro,
                "arguments": arguments.strip(),
                "opcode": macro_contracts[macro]["opcode"],
                "encodedBytes": macro_contracts[macro]["encodedBytes"],
            }
        )

    direct_words = re.findall(r"^\s*dc\.w\s+(.+?)(?:\s*;.*)?$", source, re.MULTILINE)
    command_bytes = sum(row["encodedBytes"] for row in commands)
    direct_word_bytes = len(direct_words) * 2
    start_address = spec["start"]
    byte_count = spec["end"] - start_address
    if command_bytes + direct_word_bytes != byte_count:
        raise ValueError(
            "entity-action source sizing drift: "
            f"commands={command_bytes}, directWords={direct_word_bytes}, range={byte_count}"
        )

    branch_targets = []
    lines = source.splitlines()
    for index, line in enumerate(lines):
        if re.search(r"\bac_branch\b", line.split(";", 1)[0]) is None:
            continue
        if index + 1 >= len(lines):
            raise ValueError("entity-action branch lacks displacement word")
        target_match = re.search(
            r"\((?:\s*)?([A-Za-z_][A-Za-z0-9_]*)-", lines[index + 1]
        ) or re.search(r"dc\.w\s+([A-Za-z_][A-Za-z0-9_]*)-", lines[index + 1])
        if target_match is None:
            raise ValueError(f"entity-action branch target drift at line {index + 1}")
        branch_targets.append(target_match.group(1))
    jump_targets = [
        row["arguments"] for row in commands if row["macro"] == "ac_jump"
    ]
    internal_labels = {row["name"] for row in labels}
    shared_targets = {"eas_Idle", "eas_ControlledCharacter", "eas_Motionless"}
    if set(branch_targets) - (internal_labels | shared_targets):
        raise ValueError(f"entity-action branch has an unknown target: {spec['id']}")
    if set(jump_targets) - (internal_labels | shared_targets):
        raise ValueError(f"entity-action jump has an unknown target: {spec['id']}")

    rom_range = rom[start_address : spec["end"]]
    command_counts = Counter(row["macro"] for row in commands)
    label_kinds = Counter(row["name"].split("_", 1)[0] for row in labels)
    return {
        "id": spec["id"],
        "sourcePath": spec["path"].as_posix(),
        "start": start_address,
        "endExclusive": spec["end"],
        "sha256": hashlib.sha256(rom_range).hexdigest().upper(),
        "summary": {
            "byteCount": byte_count,
            "labelCount": len(labels),
            "entryLabelCount": label_kinds["eas"],
            "internalByteLabelCount": label_kinds["byte"],
            "internalWordLabelCount": label_kinds["word"],
            "commandCount": len(commands),
            "uniqueCommandMacroCount": len(command_counts),
            "directDisplacementWordCount": len(direct_words),
            "absoluteJumpCount": command_counts["ac_jump"],
            "relativeBranchCount": command_counts["ac_branch"],
        },
        "commandCounts": dict(sorted(command_counts.items())),
        "controlFlow": {
            "relativeBranchTargets": branch_targets,
            "absoluteJumpTargets": jump_targets,
        },
        "labels": labels,
        "commands": commands,
    }


def _source_area(path: Path) -> str:
    value = path.as_posix()
    if value.startswith("data/maps/"):
        return "maps"
    if value.startswith("data/battles/"):
        return "battles"
    if value.startswith("data/scripting/"):
        return "scripting-data"
    if value.startswith("code/"):
        return "code"
    raise ValueError(f"unclassified distributed entity-action source: {value}")


def _command_row(
    line_number: int,
    match: re.Match[str],
    macro_contracts: dict[str, dict[str, Any]],
    owner_label: str,
) -> dict[str, Any]:
    inline_label, macro, arguments = match.groups()
    if macro not in macro_contracts:
        raise ValueError(f"undefined entity-action macro at line {line_number}: {macro}")
    return {
        "line": line_number,
        "ownerLabel": inline_label or owner_label,
        "macro": macro,
        "arguments": arguments.strip(),
        "opcode": macro_contracts[macro]["opcode"],
        "encodedBytes": macro_contracts[macro]["encodedBytes"],
    }


def _program_control_flow(
    commands: list[dict[str, Any]], branch_targets: list[str]
) -> dict[str, list[str]]:
    return {
        "relativeBranchTargets": branch_targets,
        "absoluteJumpTargets": [
            row["arguments"] for row in commands if row["macro"] == "ac_jump"
        ],
    }


def _standalone_regions(
    lines: list[str], command_lines: list[int]
) -> list[tuple[int, int]]:
    if not command_lines:
        return []

    def is_separator_payload(raw_line: str) -> bool:
        code = raw_line.split(";", 1)[0].strip()
        return (
            not code
            or LABEL_ONLY_PATTERN.match(code) is not None
            or BRANCH_WORD_PATTERN.match(code) is not None
        )

    regions: list[list[int]] = [[command_lines[0]]]
    for line_number in command_lines[1:]:
        previous = regions[-1][-1]
        between_is_payload = all(
            is_separator_payload(lines[index - 1])
            for index in range(previous + 1, line_number)
        )
        if between_is_payload:
            regions[-1].append(line_number)
        else:
            regions.append([line_number])

    spans = []
    for region in regions:
        start = region[0]
        probe = start - 1
        while probe >= 1 and not lines[probe - 1].split(";", 1)[0].strip():
            probe -= 1
        if probe >= 1:
            label_match = LABEL_ONLY_PATTERN.match(lines[probe - 1].split(";", 1)[0])
            if label_match and label_match.group(1).startswith("eas_"):
                start = probe

        end = region[-1]
        last_command = COMMAND_PATTERN.match(lines[end - 1].split(";", 1)[0])
        if last_command and last_command.group(2) == "ac_branch":
            probe = end + 1
            while probe <= len(lines) and not lines[probe - 1].split(";", 1)[0].strip():
                probe += 1
            if probe > len(lines) or BRANCH_WORD_PATTERN.match(
                lines[probe - 1].split(";", 1)[0]
            ) is None:
                raise ValueError(f"standalone entity-action branch lacks word after line {end}")
            end = probe
        spans.append((start, end))
    return spans


def _parse_standalone_region(
    *,
    path: Path,
    lines: list[str],
    start_line: int,
    end_line: int,
    addresses: dict[str, int],
    macro_contracts: dict[str, dict[str, Any]],
    rom: bytes,
) -> list[dict[str, Any]]:
    labels: list[dict[str, Any]] = []
    commands: list[dict[str, Any]] = []
    branch_targets: list[str] = []
    start_label: str | None = None
    start_address: int | None = None
    active_label = "$range-start"

    for line_number in range(start_line, end_line + 1):
        code = lines[line_number - 1].split(";", 1)[0].rstrip()
        if not code.strip():
            continue
        label_match = LABEL_PATTERN.match(code)
        label = label_match.group(1) if label_match else None
        if label:
            if label not in addresses:
                raise ValueError(f"standalone entity-action label lacks H1 address: {label}")
            if start_label is None:
                start_label = label
                start_address = addresses[label]
            assert start_address is not None
            expected = start_address + sum(row["encodedBytes"] for row in commands) + 2 * len(
                branch_targets
            )
            if label not in addresses or addresses[label] != expected:
                raise ValueError(f"standalone entity-action label address drift: {label}")
            labels.append(
                {"name": label, "address": addresses[label], "line": line_number}
            )
            active_label = label
        if start_label is None or start_address is None:
            raise ValueError(
                f"standalone entity-action payload lacks a labeled start: {path}:{line_number}"
            )
        command_match = COMMAND_PATTERN.match(code)
        if command_match:
            commands.append(
                _command_row(line_number, command_match, macro_contracts, active_label)
            )
            continue
        branch_word = BRANCH_WORD_PATTERN.match(code)
        if branch_word:
            if not commands or commands[-1]["macro"] != "ac_branch":
                raise ValueError(f"orphan entity-action displacement word: {path}:{line_number}")
            target, base = branch_word.groups()
            if commands[-1]["ownerLabel"] != base:
                raise ValueError(f"entity-action branch base drift: {path}:{line_number}")
            branch_targets.append(target)
            continue
        if LABEL_ONLY_PATTERN.match(code):
            continue
        raise ValueError(f"unexpected standalone entity-action source: {path}:{line_number}")
    if not commands:
        raise ValueError(f"standalone entity-action region has no commands: {path}:{start_line}")
    if commands[-1]["macro"] not in {"ac_branch", "ac_jump"}:
        raise ValueError(f"standalone entity-action region has no terminator: {start_label}")
    byte_count = sum(row["encodedBytes"] for row in commands) + 2 * len(branch_targets)
    end_exclusive = start_address + byte_count
    rom_range = rom[start_address:end_exclusive]
    command_counts = Counter(row["macro"] for row in commands)
    return [
        {
            "id": start_label,
            "sourcePath": path.as_posix(),
            "startLine": start_line,
            "endLine": end_line,
            "start": start_address,
            "endExclusive": end_exclusive,
            "sha256": hashlib.sha256(rom_range).hexdigest().upper(),
            "summary": {
                "byteCount": byte_count,
                "labelCount": len(labels),
                "entryLabelCount": sum(
                    label["name"].startswith("eas_") for label in labels
                ),
                "commandCount": len(commands),
                "uniqueCommandMacroCount": len(command_counts),
                "directDisplacementWordCount": len(branch_targets),
                "absoluteJumpCount": command_counts["ac_jump"],
                "relativeBranchCount": command_counts["ac_branch"],
            },
            "commandCounts": dict(sorted(command_counts.items())),
            "controlFlow": _program_control_flow(commands, branch_targets),
            "labels": labels,
            "commands": commands,
        }
    ]


def _parse_distributed_programs(
    disasm: Path,
    rom: bytes,
    addresses: dict[str, int],
    macro_contracts: dict[str, dict[str, Any]],
    shared_corpora: list[dict[str, Any]],
) -> dict[str, Any]:
    shared_paths = {row["sourcePath"] for row in shared_corpora}
    sources: list[tuple[Path, str, list[str]]] = []
    source_paths = [
        *(disasm / "code").rglob("*.asm"),
        *(disasm / "data").rglob("*.asm"),
    ]
    for source_path in sorted(source_paths):
        relative = source_path.relative_to(disasm)
        if relative.as_posix() in shared_paths:
            continue
        source = read_upstream_text(source_path)
        if re.search(r"^\s*(?:[A-Za-z_][A-Za-z0-9_]*:\s*)?ac_[A-Za-z0-9_]+", source, re.MULTILINE):
            sources.append((relative, source, source.splitlines()))

    inline_programs: list[dict[str, Any]] = []
    standalone_programs: list[dict[str, Any]] = []
    file_rows = []
    for relative, source, lines in sources:
        active: dict[str, Any] | None = None
        standalone_command_lines: list[int] = []
        file_inline_count = 0
        file_command_count = 0
        for line_number, raw_line in enumerate(lines, 1):
            code = raw_line.split(";", 1)[0].rstrip()
            starter = CUSTOM_PROGRAM_PATTERN.match(code)
            if starter:
                if active is not None:
                    raise ValueError(f"nested custom actscript: {relative}:{line_number}")
                inline_label, wrapper, entity = starter.groups()
                active = {
                    "id": f"{relative.as_posix()}#L{line_number}",
                    "sourcePath": relative.as_posix(),
                    "startLine": line_number,
                    "endLine": line_number,
                    "wrapper": wrapper,
                    "waitForCompletion": wrapper == "customActscriptWait",
                    "entityExpression": entity.strip(),
                    "ownerLabel": inline_label or f"$inline-{line_number}",
                    "commands": [],
                }
                file_inline_count += 1
                continue
            command_match = COMMAND_PATTERN.match(code)
            if command_match:
                file_command_count += 1
                if active is None:
                    standalone_command_lines.append(line_number)
                    continue
                command = _command_row(
                    line_number, command_match, macro_contracts, active["ownerLabel"]
                )
                active["commands"].append(command)
                active["endLine"] = line_number
                if command["macro"] == "ac_end":
                    command_counts = Counter(row["macro"] for row in active["commands"])
                    inline_programs.append(
                        {
                            key: active[key]
                            for key in (
                                "id",
                                "sourcePath",
                                "startLine",
                                "endLine",
                                "wrapper",
                                "waitForCompletion",
                                "entityExpression",
                            )
                        }
                        | {
                            "summary": {
                                "encodedByteCount": sum(
                                    row["encodedBytes"] for row in active["commands"]
                                ),
                                "commandCount": len(active["commands"]),
                                "uniqueCommandMacroCount": len(command_counts),
                                "absoluteJumpCount": command_counts["ac_jump"],
                                "relativeBranchCount": command_counts["ac_branch"],
                            },
                            "commandCounts": dict(sorted(command_counts.items())),
                            "controlFlow": _program_control_flow(active["commands"], []),
                            "commands": active["commands"],
                        }
                    )
                    active = None
                continue
            if active is not None and code.strip():
                raise ValueError(
                    f"unexpected source inside custom actscript: {relative}:{line_number}"
                )
        if active is not None:
            raise ValueError(f"unterminated custom actscript: {relative}:{active['startLine']}")

        before_program_count = len(standalone_programs)
        for start_line, end_line in _standalone_regions(lines, standalone_command_lines):
            standalone_programs.extend(
                _parse_standalone_region(
                    path=relative,
                    lines=lines,
                    start_line=start_line,
                    end_line=end_line,
                    addresses=addresses,
                    macro_contracts=macro_contracts,
                    rom=rom,
                )
            )
        file_programs = standalone_programs[before_program_count:]
        file_rows.append(
            {
                "sourcePath": relative.as_posix(),
                "area": _source_area(relative),
                "commandCount": file_command_count,
                "inlineProgramCount": file_inline_count,
                "standaloneProgramCount": len(file_programs),
                "namedEntryLabelCount": sum(
                    program["summary"]["entryLabelCount"]
                    for program in file_programs
                ),
                "sha256": hashlib.sha256(source.encode("utf-8")).hexdigest().upper(),
            }
        )

    all_commands = [
        command
        for program in [*inline_programs, *standalone_programs]
        for command in program["commands"]
    ]
    command_counts = Counter(row["macro"] for row in all_commands)
    area_counts = Counter(row["area"] for row in file_rows)
    relative_targets = [
        target
        for program in standalone_programs
        for target in program["controlFlow"]["relativeBranchTargets"]
    ]
    absolute_targets = [
        target
        for program in [*inline_programs, *standalone_programs]
        for target in program["controlFlow"]["absoluteJumpTargets"]
    ]
    all_labels = {
        label["name"]
        for corpus in shared_corpora
        for label in corpus["labels"]
    } | {
        label["name"]
        for program in standalone_programs
        for label in program["labels"]
    }
    unresolved = sorted(
        (set(relative_targets) | set(absolute_targets)) - all_labels
    )
    inline_command_count = sum(row["summary"]["commandCount"] for row in inline_programs)
    standalone_command_count = sum(
        row["summary"]["commandCount"] for row in standalone_programs
    )
    summary = {
        "sourceFileCount": len(file_rows),
        "inlineProgramCount": len(inline_programs),
        "standaloneProgramCount": len(standalone_programs),
        "namedEntryLabelCount": sum(
            program["summary"]["entryLabelCount"]
            for program in standalone_programs
        ),
        "commandCount": len(all_commands),
        "ownedCommandCount": inline_command_count + standalone_command_count,
        "unownedCommandCount": len(all_commands)
        - inline_command_count
        - standalone_command_count,
        "inlineCommandCount": inline_command_count,
        "standaloneCommandCount": standalone_command_count,
        "uniqueCommandMacroCount": len(command_counts),
        "definedCommandMacroCount": len(macro_contracts),
        "unusedCommandMacroCount": len(set(macro_contracts) - set(command_counts)),
        "encodedCommandByteCount": sum(row["encodedBytes"] for row in all_commands),
        "inlineEncodedByteCount": sum(
            row["summary"]["encodedByteCount"] for row in inline_programs
        ),
        "standaloneByteCount": sum(
            row["summary"]["byteCount"] for row in standalone_programs
        ),
        "directDisplacementWordCount": sum(
            row["summary"]["directDisplacementWordCount"]
            for row in standalone_programs
        ),
        "absoluteJumpCount": command_counts["ac_jump"],
        "relativeBranchCount": command_counts["ac_branch"],
    }
    summary["actionByteCount"] = (
        summary["encodedCommandByteCount"]
        + 2 * summary["directDisplacementWordCount"]
    )
    if summary["unownedCommandCount"] != 0:
        raise ValueError("distributed entity-action command ownership drift")
    if any(program["commands"][-1]["macro"] != "ac_end" for program in inline_programs):
        raise ValueError("distributed inline entity-action termination drift")
    return {
        "summary": summary,
        "sourceFileCounts": dict(sorted(area_counts.items())),
        "commandCounts": dict(sorted(command_counts.items())),
        "controlFlowFacts": {
            "absoluteJumpTargets": dict(sorted(Counter(absolute_targets).items())),
            "relativeBranchTargets": dict(sorted(Counter(relative_targets).items())),
            "unresolvedTargets": unresolved,
            "allTargetsResolved": not unresolved,
            "allInlineProgramsTerminateWithAcEnd": True,
        },
        "files": file_rows,
        "inlinePrograms": inline_programs,
        "standalonePrograms": standalone_programs,
    }


def _split_operands(value: str) -> list[str]:
    operands = []
    start = 0
    depth = 0
    for index, character in enumerate(value):
        if character == "(":
            depth += 1
        elif character == ")":
            depth -= 1
        elif character == "," and depth == 0:
            operands.append(value[start:index].strip())
            start = index + 1
    if value[start:].strip():
        operands.append(value[start:].strip())
    return operands


def _operand_mode(mnemonic: str, operand_index: int, operand_count: int) -> str:
    operation = mnemonic.split(".", 1)[0]
    if operation == "lea" and operand_index == 0:
        return "address"
    if operation in {"move", "movea"}:
        return "read" if operand_index == 0 else "write"
    if operation.startswith("cmp") or operation in {"tst", "btst", "chk"}:
        return "read"
    if operation in {"clr", "st", "sf"}:
        return "write"
    if operation in {"bset", "bclr", "bchg"}:
        return "read" if operand_index == 0 and operand_count > 1 else "read-write"
    if operand_count > 1:
        return "read" if operand_index == 0 else "read-write"
    return "read-write"


def _access_rows(
    statements: list[str], token_pattern: re.Pattern[str], *, implicit_entity_x: bool = False
) -> list[dict[str, Any]]:
    accesses: dict[str, dict[str, Any]] = {}
    for statement in statements:
        instruction = re.match(
            r"^(?P<mnemonic>[a-z][A-Za-z0-9]*(?:\.[bwl])?)(?:\s+(?P<operands>.*))?$",
            statement,
        )
        if instruction is None:
            continue
        mnemonic = instruction.group("mnemonic")
        operands = _split_operands(instruction.group("operands") or "")
        for operand_index, operand in enumerate(operands):
            names = [match.group(1) for match in token_pattern.finditer(operand)]
            if implicit_entity_x and re.search(r"(?<![A-Za-z0-9_])\(a0\)", operand):
                names.append("ENTITYDEF_OFFSET_X")
            for name in names:
                row = accesses.setdefault(
                    name,
                    {
                        "name": name,
                        "read": False,
                        "write": False,
                        "addressed": False,
                        "operations": set(),
                    },
                )
                mode = _operand_mode(mnemonic, operand_index, len(operands))
                row["read"] |= mode in {"read", "read-write"}
                row["write"] |= mode in {"write", "read-write"}
                row["addressed"] |= mode == "address"
                row["operations"].add(mnemonic)
    return [
        {**row, "operations": sorted(row["operations"])}
        for _, row in sorted(accesses.items())
    ]


def _script_read_rows(statements: list[str]) -> list[dict[str, Any]]:
    reads: dict[tuple[int, int], set[str]] = {}
    for statement in statements:
        instruction = re.match(
            r"^(?P<mnemonic>[a-z][A-Za-z0-9]*)(?:\.(?P<size>[bwl]))?"
            r"(?:\s+(?P<operands>.*))?$",
            statement,
        )
        if instruction is None:
            continue
        mnemonic = instruction.group("mnemonic")
        size = {"b": 1, "w": 2, "l": 4}.get(instruction.group("size"))
        if size is None:
            continue
        for operand_index, operand in enumerate(
            _split_operands(instruction.group("operands") or "")
        ):
            if _operand_mode(mnemonic, operand_index, 2) not in {"read", "read-write"}:
                continue
            for value in re.findall(r"(?<![A-Za-z0-9_])(\d+)\(a1\)", operand):
                reads.setdefault((int(value), size), set()).add(
                    f"{mnemonic}.{instruction.group('size')}"
                )
    return [
        {"offset": offset, "widthBytes": width, "operations": sorted(operations)}
        for (offset, width), operations in sorted(reads.items())
    ]


def _bit_access_rows(statements: list[str]) -> list[dict[str, Any]]:
    accesses: dict[tuple[str, str], dict[str, Any]] = {}
    for statement in statements:
        match = re.match(
            r"^(?P<operation>btst|bset|bclr|bchg)\s+#(?P<bit>[^,]+),"
            r"(?P<target>.*ENTITYDEF_OFFSET_[A-Z0-9_]+\(a[0-7]\).*)$",
            statement,
        )
        if match is None:
            continue
        field = re.search(r"(ENTITYDEF_OFFSET_[A-Z0-9_]+)", match.group("target"))
        if field is None:
            continue
        key = (field.group(1), match.group("bit"))
        row = accesses.setdefault(
            key,
            {
                "field": key[0],
                "bit": key[1],
                "tested": False,
                "set": False,
                "cleared": False,
                "toggled": False,
            },
        )
        row[
            {
                "btst": "tested",
                "bset": "set",
                "bclr": "cleared",
                "bchg": "toggled",
            }[match.group("operation")]
        ] = True
    return [row for _, row in sorted(accesses.items())]


def _script_pointer_actions(statements: list[str]) -> list[dict[str, Any]]:
    actions: set[tuple[str, int, int]] = set()
    for statement in statements:
        advance = re.match(r"^addq\.l\s+#(\d+),a1$", statement)
        if advance:
            actions.add(("advance", int(advance.group(1)), 0))
        relative = re.match(r"^adda\.w\s+(\d+)\(a1\),a1$", statement)
        if relative:
            actions.add(("relative-branch", int(relative.group(1)), 2))
        absolute = re.match(r"^movea\.l\s+(\d+)\(a1\),a1$", statement)
        if absolute:
            actions.add(("absolute-jump", int(absolute.group(1)), 4))
    rows = []
    for kind, value, width in sorted(actions):
        if kind == "advance":
            rows.append({"kind": kind, "bytes": value})
        else:
            rows.append({"kind": kind, "parameterOffset": value, "widthBytes": width})
    return rows


def _entity_action_handlers(
    disasm: Path,
    addresses: dict[str, int],
    macro_contracts: dict[str, dict[str, Any]],
    command_counts: Counter[str],
) -> dict[str, Any]:
    source = read_upstream_text(disasm / HANDLER_SOURCE_PATH)
    table_match = re.search(
        r"^rjt_EntityScriptCommands:\s*\n(?P<body>.*?)(?=^; =+)",
        source,
        re.MULTILINE | re.DOTALL,
    )
    if table_match is None:
        raise ValueError("entity-action dispatcher table is missing")
    dispatch_targets = re.findall(
        r"^\s*dc\.w\s+([A-Za-z_][A-Za-z0-9_]*)-rjt_EntityScriptCommands\b",
        table_match.group("body"),
        re.MULTILINE,
    )
    if len(dispatch_targets) != 80:
        raise ValueError(f"entity-action dispatcher slot drift: {len(dispatch_targets)}")

    filler_target = "esc_goToNextEntity"
    handler_names = sorted(set(dispatch_targets) - {filler_target})
    handler_opcodes = {
        handler: dispatch_targets.index(handler) for handler in handler_names
    }
    if set(handler_opcodes.values()) != set(HANDLER_FAMILY_BY_OPCODE):
        raise ValueError("entity-action handler family opcode boundary drift")
    handlers = []
    for handler in handler_names:
        body_match = re.search(
            rf"^{re.escape(handler)}:\s*\n(?P<body>.*?)"
            rf"^\s*; End of function {re.escape(handler)}\s*$",
            source,
            re.MULTILINE | re.DOTALL,
        )
        if body_match is None:
            raise ValueError(f"entity-action handler body is missing: {handler}")
        if handler not in addresses:
            raise ValueError(f"entity-action handler lacks H1 address: {handler}")
        body = body_match.group("body")
        body_start_line = source.count("\n", 0, body_match.start("body")) + 1
        body_end_line = source.count("\n", 0, body_match.end("body")) + 1
        statements = []
        for raw_line in body.splitlines():
            code = raw_line.split(";", 1)[0].strip()
            if not code or re.match(r"^[A-Za-z_@][A-Za-z0-9_@]*:$", code):
                continue
            if re.match(r"^[a-z][A-Za-z0-9]*(?:\.[bwl])?(?:\s+|$)", code):
                statements.append(code)
        entity_pattern = re.compile(r"\b(ENTITYDEF_OFFSET_[A-Z0-9_]+)\b")
        global_pattern = re.compile(
            r"\(\(([A-Z][A-Z0-9_]*)-\$1000000\)\)\.w|"
            r"\(([A-Z][A-Z0-9_]*)\)\.l"
        )
        entity_accesses = _access_rows(
            statements, entity_pattern, implicit_entity_x=True
        )
        global_accesses_raw: dict[str, dict[str, Any]] = {}
        for statement in statements:
            normalized = re.sub(
                global_pattern,
                lambda match: f"GLOBAL::{match.group(1) or match.group(2)}",
                statement,
            )
            rows = _access_rows(
                [normalized], re.compile(r"GLOBAL::([A-Z][A-Z0-9_]*)")
            )
            for row in rows:
                existing = global_accesses_raw.setdefault(
                    row["name"],
                    {
                        "name": row["name"],
                        "read": False,
                        "write": False,
                        "addressed": False,
                        "operations": set(),
                    },
                )
                existing["read"] |= row["read"]
                existing["write"] |= row["write"]
                existing["addressed"] |= row["addressed"]
                existing["operations"].update(row["operations"])
        global_accesses = [
            {**row, "operations": sorted(row["operations"])}
            for _, row in sorted(global_accesses_raw.items())
        ]
        entity_fields = [row["name"] for row in entity_accesses]
        global_state = sorted(
            set(re.findall(r"\(\(([A-Z][A-Z0-9_]*)-\$1000000\)\)\.w", body))
            | set(re.findall(r"\(([A-Z][A-Z0-9_]*)\)\.l", body))
        )
        direct_calls = sorted(
            set(
                re.findall(
                    r"\b(?:bsr|jsr)(?:\.[bwl])?\s+\(?([A-Za-z_][A-Za-z0-9_]*)",
                    body,
                )
            )
        )
        exit_routes = sorted(
            set(
                re.findall(
                    r"\b(?:bra|jmp)(?:\.[bwl])?\s+"
                    r"(esc_(?:clearTimerGoToNextCommand|clearTimerGoToNextEntity|goToNextEntity))\b",
                    body,
                )
            )
        )
        script_offsets = sorted(
            {
                int(value)
                for value in re.findall(r"(?<![A-Za-z0-9_])-?(\d+)\(a1\)", body)
            }
        )
        handlers.append(
            {
                "name": handler,
                "opcode": handler_opcodes[handler],
                "family": HANDLER_FAMILY_BY_OPCODE[handler_opcodes[handler]],
                "address": addresses[handler],
                "sourcePath": HANDLER_SOURCE_PATH.as_posix(),
                "startLine": body_start_line,
                "endLine": body_end_line,
                "statementCount": len(statements),
                "entityFields": entity_fields,
                "entityFieldAccesses": entity_accesses,
                "globalState": global_state,
                "globalStateAccesses": global_accesses,
                "scriptParameterOffsets": script_offsets,
                "scriptReads": _script_read_rows(statements),
                "bitAccesses": _bit_access_rows(statements),
                "scriptPointerActions": _script_pointer_actions(statements),
                "directCalls": direct_calls,
                "exitRoutes": exit_routes,
            }
        )

    handlers_by_name = {row["name"]: row for row in handlers}
    macro_bindings = []
    opcode_rows: dict[int, dict[str, Any]] = {}
    for macro, contract in sorted(macro_contracts.items()):
        opcode = contract["opcode"]
        is_terminator = macro == "ac_end"
        if is_terminator:
            handler = None
        else:
            if opcode >= len(dispatch_targets):
                raise ValueError(f"entity-action macro opcode is outside dispatcher: {macro}")
            handler = dispatch_targets[opcode]
            if handler == filler_target:
                raise ValueError(f"entity-action macro maps to filler slot: {macro}")
            row = opcode_rows.setdefault(
                opcode,
                {
                    "opcode": opcode,
                    "handler": handler,
                    "encodedBytes": contract["encodedBytes"],
                    "macros": [],
                    "sourceCommandCount": 0,
                },
            )
            if row["handler"] != handler or row["encodedBytes"] != contract["encodedBytes"]:
                raise ValueError(f"entity-action opcode alias drift: {macro}")
            row["macros"].append(macro)
            row["sourceCommandCount"] += command_counts[macro]
        handler_read_bytes = (
            {
                byte
                for read in handlers_by_name[handler]["scriptReads"]
                for byte in range(read["offset"], read["offset"] + read["widthBytes"])
            }
            if handler is not None
            else set()
        )
        parameter_coverage = []
        for parameter in contract["parameters"]:
            declared = set(
                range(
                    parameter["offset"],
                    parameter["offset"] + parameter["widthBytes"],
                )
            )
            parameter_coverage.append(
                {
                    **parameter,
                    "readOffsets": sorted(declared & handler_read_bytes),
                    "ignoredOffsets": sorted(declared - handler_read_bytes),
                }
            )
        macro_bindings.append(
            {
                "macro": macro,
                "opcode": opcode,
                "encodedBytes": contract["encodedBytes"],
                "parameterBytes": contract["encodedBytes"] - 2,
                "declaredParameters": parameter_coverage,
                "allDeclaredParameterBytesRead": all(
                    not row["ignoredOffsets"] for row in parameter_coverage
                ),
                "handler": handler,
                "isInlineTerminator": is_terminator,
                "sourceCommandCount": command_counts[macro],
            }
        )

    handler_by_opcode = {
        index: target
        for index, target in enumerate(dispatch_targets)
        if target != filler_target
    }
    handler_only_opcodes = {
        opcode: target for opcode, target in handler_by_opcode.items() if opcode not in opcode_rows
    }
    mapped_handlers = {row["handler"] for row in opcode_rows.values()} | set(
        handler_only_opcodes.values()
    )
    if mapped_handlers != set(handler_names):
        raise ValueError("entity-action handler coverage drift")
    filler_indices = [
        index for index, target in enumerate(dispatch_targets) if target == filler_target
    ]
    if any(not row["scriptPointerActions"] for row in handlers):
        raise ValueError("entity-action handler lacks a classified script-pointer action")
    partially_consumed_macros = [
        row["macro"]
        for row in macro_bindings
        if not row["isInlineTerminator"] and not row["allDeclaredParameterBytesRead"]
    ]
    summary = {
        "dispatchSlotCount": len(dispatch_targets),
        "uniqueDispatchTargetCount": len(set(dispatch_targets)),
        "fillerSlotCount": len(filler_indices),
        "handlerCount": len(handler_names),
        "definedMacroCount": len(macro_bindings),
        "runtimeMacroCount": sum(not row["isInlineTerminator"] for row in macro_bindings),
        "runtimeOpcodeCount": len(opcode_rows),
        "handlerOnlyOpcodeCount": len(handler_only_opcodes),
        "terminatorMacroCount": sum(row["isInlineTerminator"] for row in macro_bindings),
        "usedRuntimeMacroCount": sum(
            not row["isInlineTerminator"] and row["sourceCommandCount"] > 0
            for row in macro_bindings
        ),
        "unusedRuntimeMacroCount": sum(
            not row["isInlineTerminator"] and row["sourceCommandCount"] == 0
            for row in macro_bindings
        ),
        "handlerEntityFieldCount": len(
            {field for row in handlers for field in row["entityFields"]}
        ),
        "handlerGlobalStateCount": len(
            {field for row in handlers for field in row["globalState"]}
        ),
        "handlerDirectCallTargetCount": len(
            {target for row in handlers for target in row["directCalls"]}
        ),
        "handlerEntityReadFieldCount": len(
            {
                access["name"]
                for row in handlers
                for access in row["entityFieldAccesses"]
                if access["read"]
            }
        ),
        "handlerEntityWriteFieldCount": len(
            {
                access["name"]
                for row in handlers
                for access in row["entityFieldAccesses"]
                if access["write"]
            }
        ),
        "handlerGlobalReadStateCount": len(
            {
                access["name"]
                for row in handlers
                for access in row["globalStateAccesses"]
                if access["read"]
            }
        ),
        "handlerGlobalWriteStateCount": len(
            {
                access["name"]
                for row in handlers
                for access in row["globalStateAccesses"]
                if access["write"]
            }
        ),
        "handlerFamilyCount": len({row["family"] for row in handlers}),
        "handlerBitAccessCount": sum(len(row["bitAccesses"]) for row in handlers),
        "handlerScriptPointerActionCount": sum(
            len(row["scriptPointerActions"]) for row in handlers
        ),
        "declaredMacroParameterCount": sum(
            len(row["declaredParameters"]) for row in macro_bindings
        ),
        "declaredMacroParameterByteCount": sum(
            parameter["widthBytes"]
            for row in macro_bindings
            for parameter in row["declaredParameters"]
        ),
        "ignoredDeclaredParameterByteCount": sum(
            len(parameter["ignoredOffsets"])
            for row in macro_bindings
            for parameter in row["declaredParameters"]
        ),
        "fullyConsumedRuntimeMacroCount": sum(
            not row["isInlineTerminator"] and row["allDeclaredParameterBytesRead"]
            for row in macro_bindings
        ),
    }
    return {
        "summary": summary,
        "facts": {
            "fillerTarget": filler_target,
            "fillerIndices": filler_indices,
            "handlerOnlyOpcodes": {
                str(opcode): target for opcode, target in sorted(handler_only_opcodes.items())
            },
            "inlineTerminatorMacro": "ac_end",
            "inlineTerminatorWord": macro_contracts["ac_end"]["opcode"],
            "unusedRuntimeMacros": [
                row["macro"]
                for row in macro_bindings
                if not row["isInlineTerminator"] and row["sourceCommandCount"] == 0
            ],
            "allRuntimeMacrosMapToHandlers": True,
            "allNonfillerHandlersOwned": True,
            "allHandlerAccessesClassified": True,
            "allHandlersGrouped": True,
            "allHandlerFlowActionsClassified": True,
            "allDeclaredParameterBytesAccounted": True,
            "partiallyConsumedMacros": partially_consumed_macros,
            "handlerFamilyCounts": dict(
                sorted(Counter(row["family"] for row in handlers).items())
            ),
        },
        "dispatchTable": dispatch_targets,
        "macroBindings": macro_bindings,
        "opcodeBindings": [opcode_rows[opcode] for opcode in sorted(opcode_rows)],
        "handlers": handlers,
    }


def _reference_rows_for_owners(
    disasm: Path, label_owner: dict[str, str]
) -> list[dict[str, Any]]:
    alternatives = "|".join(
        re.escape(label) for label in sorted(label_owner, key=len, reverse=True)
    )
    pattern = re.compile(rf"\b(?:{alternatives})\b")
    external: dict[str, list[dict[str, Any]]] = {label: [] for label in label_owner}
    internal_counts: Counter[str] = Counter()
    paths = sorted([*(disasm / "code").rglob("*.asm"), *(disasm / "data").rglob("*.asm")])
    for path in paths:
        relative_path = path.relative_to(disasm).as_posix()
        for line_number, raw_line in enumerate(read_upstream_text(path).splitlines(), 1):
            code = raw_line.split(";", 1)[0]
            definition = re.match(r"^\s*([A-Za-z_][A-Za-z0-9_]*):", code)
            for match in pattern.finditer(code):
                label = match.group(0)
                if definition and definition.group(1) == label:
                    continue
                if relative_path == label_owner[label]:
                    internal_counts[label] += 1
                else:
                    external[label].append({"path": relative_path, "line": line_number})
    return [
        {
            "label": label,
            "ownerPath": label_owner[label],
            "internalReferenceCount": internal_counts[label],
            "externalReferenceCount": len(external[label]),
            "externalSourceFileCount": len({row["path"] for row in external[label]}),
            "externalReferences": external[label],
        }
        for label in sorted(label_owner)
    ]


def _reference_rows(disasm: Path, corpora: list[dict[str, Any]]) -> list[dict[str, Any]]:
    return _reference_rows_for_owners(
        disasm,
        {
            label["name"]: corpus["sourcePath"]
            for corpus in corpora
            for label in corpus["labels"]
            if label["name"].startswith("eas_")
        },
    )


def build_entity_action_script_contract(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"entity-action H1 listing is missing: {listing_path}")
    addresses = listing_symbol_addresses(listing_path.read_text(encoding="utf-8"))
    rom_identity = inspect_rom(rom_path)
    rom = rom_path.read_bytes()
    macro_contracts = _macro_contracts(disasm)
    corpora = [
        _parse_corpus(disasm, rom, addresses, macro_contracts, spec) for spec in CORPORA
    ]
    references = _reference_rows(disasm, corpora)
    distributed = _parse_distributed_programs(
        disasm, rom, addresses, macro_contracts, corpora
    )
    distributed_table = {
        label["name"]: label["address"]
        for program in distributed["standalonePrograms"]
        for label in program["labels"]
        if label["name"].startswith("eas_")
    }
    distributed_references = _reference_rows_for_owners(
        disasm,
        {
            label["name"]: program["sourcePath"]
            for program in distributed["standalonePrograms"]
            for label in program["labels"]
            if label["name"].startswith("eas_")
        },
    )
    command_counts: Counter[str] = Counter()
    for corpus in corpora:
        command_counts.update(corpus["commandCounts"])
    all_command_counts = command_counts.copy()
    all_command_counts.update(distributed["commandCounts"])
    handler_contract = _entity_action_handlers(
        disasm, addresses, macro_contracts, all_command_counts
    )
    summary = {
        "corpusCount": len(corpora),
        "byteCount": sum(row["summary"]["byteCount"] for row in corpora),
        "labelCount": sum(row["summary"]["labelCount"] for row in corpora),
        "entryLabelCount": sum(row["summary"]["entryLabelCount"] for row in corpora),
        "internalByteLabelCount": sum(
            row["summary"]["internalByteLabelCount"] for row in corpora
        ),
        "internalWordLabelCount": sum(
            row["summary"]["internalWordLabelCount"] for row in corpora
        ),
        "commandCount": sum(row["summary"]["commandCount"] for row in corpora),
        "uniqueCommandMacroCount": len(command_counts),
        "definedCommandMacroCount": len(macro_contracts),
        "unusedCommandMacroCount": len(set(macro_contracts) - set(command_counts)),
        "directDisplacementWordCount": sum(
            row["summary"]["directDisplacementWordCount"] for row in corpora
        ),
        "absoluteJumpCount": sum(row["summary"]["absoluteJumpCount"] for row in corpora),
        "relativeBranchCount": sum(row["summary"]["relativeBranchCount"] for row in corpora),
        "externallyReferencedEntryCount": sum(
            row["externalReferenceCount"] > 0 for row in references
        ),
        "externalReferenceCount": sum(row["externalReferenceCount"] for row in references),
        "externalReferenceSourceFileCount": len(
            {
                reference["path"]
                for row in references
                for reference in row["externalReferences"]
            }
        ),
    }
    unreferenced = [
        row["label"] for row in references if row["externalReferenceCount"] == 0
    ]
    distributed_unreferenced = [
        row["label"]
        for row in distributed_references
        if row["internalReferenceCount"] + row["externalReferenceCount"] == 0
    ]
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "romSha256": rom_identity["sha256"],
        "function": {
            "rjt_EntityScriptCommands": addresses["rjt_EntityScriptCommands"]
        },
        "table": {
            spec["bindingSymbol"]: addresses[spec["bindingSymbol"]]
            for spec in CORPORA
        }
        | distributed_table,
        "summary": summary,
        "romRanges": [
            {
                key: corpus[key]
                for key in ("id", "sourcePath", "start", "endExclusive", "sha256")
            }
            for corpus in corpora
        ],
        "commandCounts": dict(sorted(command_counts.items())),
        "referenceFacts": {
            "externallyUnreferencedEntryLabels": unreferenced,
            "allOtherEntryLabelsHaveExternalReferences": len(unreferenced) == 1,
            "referenceScope": "all pinned code/data ASM, comments and definitions excluded",
        },
        "distributedSummary": distributed["summary"],
        "distributedSourceFileCounts": distributed["sourceFileCounts"],
        "distributedCommandCounts": distributed["commandCounts"],
        "distributedControlFlowFacts": distributed["controlFlowFacts"],
        "distributedReferenceFacts": {
            "unreferencedEntryLabels": distributed_unreferenced,
            "referencedEntryCount": sum(
                row["internalReferenceCount"] + row["externalReferenceCount"] > 0
                for row in distributed_references
            ),
            "referenceCount": sum(
                row["internalReferenceCount"] + row["externalReferenceCount"]
                for row in distributed_references
            ),
            "crossFileReferenceCount": sum(
                row["externalReferenceCount"] for row in distributed_references
            ),
            "referenceSourceFileCount": len(
                {
                    reference["path"]
                    for row in distributed_references
                    for reference in row["externalReferences"]
                }
                | {
                    row["ownerPath"]
                    for row in distributed_references
                    if row["internalReferenceCount"] > 0
                }
            ),
            "referenceScope": "all pinned code/data ASM, comments and definitions excluded",
        },
        "handlerSummary": handler_contract["summary"],
        "handlerFacts": handler_contract["facts"],
        "standaloneRomRanges": [
            {
                key: program[key]
                for key in (
                    "id",
                    "sourcePath",
                    "start",
                    "endExclusive",
                    "sha256",
                )
            }
            for program in distributed["standalonePrograms"]
        ],
        "corpora": corpora,
        "entryReferences": references,
        "distributedFiles": distributed["files"],
        "inlinePrograms": distributed["inlinePrograms"],
        "standalonePrograms": distributed["standalonePrograms"],
        "distributedEntryReferences": distributed_references,
        "handlerDispatchTable": handler_contract["dispatchTable"],
        "handlerMacroBindings": handler_contract["macroBindings"],
        "handlerOpcodeBindings": handler_contract["opcodeBindings"],
        "handlers": handler_contract["handlers"],
        "runtimeQuestions": [
            "What are the exact frame durations and collision effects of movement/action commands?",
            "Which external references are reachable through normal story routes?",
        ],
    }


def verify_entity_action_script_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_entity_action_script_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="entity-action script static contract")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != output["romSha256"]
    ):
        raise ValueError("entity-action script provenance drift")
    for field in (
        "function",
        "table",
        "summary",
        "romRanges",
        "commandCounts",
        "referenceFacts",
        "distributedSummary",
        "distributedSourceFileCounts",
        "distributedCommandCounts",
        "distributedControlFlowFacts",
        "distributedReferenceFacts",
        "handlerSummary",
        "handlerFacts",
        "standaloneRomRanges",
        "runtimeQuestions",
    ):
        if fixture[field] != output[field]:
            raise ValueError(f"entity-action script fixture drift: {field}")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if (
        output["summary"] != manifest["summary"]
        or output["distributedSummary"] != manifest["distributedSummary"]
        or output["handlerSummary"] != manifest["handlerSummary"]
        or digest != manifest["outputSha256"]
    ):
        raise ValueError("entity-action script canonical manifest drift")
    destination = output_path or repo_path("local/derived/entity-action-scripts-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Labels": output["summary"]["labelCount"],
        "Commands": output["summary"]["commandCount"]
        + output["distributedSummary"]["commandCount"],
        "DistributedCommands": output["distributedSummary"]["commandCount"],
        "Status": "PASS",
    }
