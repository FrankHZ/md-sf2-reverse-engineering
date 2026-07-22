from __future__ import annotations

import hashlib
import json
import re
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.h2.map_scripts import _script_programs
from sf2tool.h2.map_setup import build_map_setup_contract
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.source_text import read_upstream_text

ID = "sf2-map-init-static-v1"
SOURCE_ROOT = Path("data/maps/entries")
DISPATCH_PATH = Path("code/common/scripting/map/mapsetupsfunctions_1.asm")
ENUMS_PATH = Path("sf2enums.asm")
TRAP_MACROS_PATH = Path("sf2macros.asm")
CUTSCENE_MACROS_PATH = Path("sf2cutscenemacros.asm")
JUMP_INTERFACE_ROOT = Path("code/common/tech/jumpinterfaces")
MANIFEST = repo_path("manifests/extractions/map-init-static.json")
SCHEMA = repo_path("schemas/map-init-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/map-init-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-map-init-static-fixture.schema.json")

FAMILY_ORDER = (
    "flag-read",
    "flag-write",
    "script-invocation",
    "direct-call",
    "entity-or-position-command",
    "warp-or-transition-command",
    "presentation-audio-text-command",
    "arithmetic-or-data-movement",
    "branch-or-jump",
    "terminal",
)


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _function_body(source: str, symbol: str, owner_symbol: str) -> str:
    start = re.search(rf"(?m)^{re.escape(symbol)}:\s*$", source)
    if start is None:
        raise ValueError(f"map init entry label missing: {symbol}")
    end = source.find(f"End of function {owner_symbol}", start.end())
    if end < 0:
        raise ValueError(f"map init function boundary missing: {symbol}")
    return source[start.end() : end]


def _operation_rows(body: str) -> list[dict[str, Any]]:
    rows: list[dict[str, Any]] = []
    pending_labels: list[str] = []
    for raw_line in body.splitlines():
        line = raw_line.split(";", 1)[0].strip()
        while line and (label_match := re.match(r"^(@?[A-Za-z_][A-Za-z0-9_]*):\s*", line)):
            pending_labels.append(label_match.group(1))
            line = line[label_match.end() :].strip()
        if not line:
            continue
        parts = line.split(None, 1)
        opcode = parts[0]
        operand_text = parts[1] if len(parts) == 2 else ""
        rows.append(
            {
                "index": len(rows),
                "labels": pending_labels,
                "opcode": opcode,
                "operandText": operand_text.strip(),
                "branchTargetSymbol": None,
                "branchTargetAddress": None,
                "localBranchTargetIndex": None,
            }
        )
        pending_labels = []
    if pending_labels:
        raise ValueError(f"map init labels have no following operation: {pending_labels}")
    label_indices: dict[str, int] = {}
    for row in rows:
        for label in row["labels"]:
            if label in label_indices:
                raise ValueError(f"duplicate map init local label: {label}")
            label_indices[label] = row["index"]
    for row in rows:
        if not re.fullmatch(r"b(?!sr)[a-z]{2}(?:\.[bswl])?", row["opcode"]):
            continue
        target = row["operandText"].rsplit(",", 1)[-1].strip()
        target = re.sub(r"\(pc\)$", "", target)
        row["branchTargetSymbol"] = target
        row["localBranchTargetIndex"] = label_indices.get(target)
    return rows


def _parse_equates(source: str) -> dict[str, int]:
    """Return numeric enum declarations without treating a literal as authority."""
    values: dict[str, int] = {}
    for name, token in re.findall(
        r"^([A-Z][A-Z0-9_]*):\s+equ\s+(\$[0-9A-Fa-f]+|\d+)", source, re.MULTILINE
    ):
        value = int(token[1:], 16) if token.startswith("$") else int(token)
        if name in values and values[name] != value:
            raise ValueError(f"conflicting map-init enum declaration: {name}")
        values[name] = value
    return values


def _init_function_pointer_layout_row(
    pointer_layout: list[dict[str, Any]],
) -> dict[str, Any]:
    """Select the init-function row from the independently parsed setup pointer layout."""
    matches = [
        (source_order, row)
        for source_order, row in enumerate(pointer_layout)
        if row.get("name") == "initFunction"
    ]
    if len(matches) != 1:
        raise ValueError("map init pointer layout lacks one initFunction row")
    source_order, row = matches[0]
    if set(row) != {"name", "offset"} or not isinstance(row["offset"], int):
        raise ValueError("map init pointer layout row shape drift")
    return {
        "sourceOrder": source_order,
        "name": row["name"],
        "offset": row["offset"],
    }


def _dispatcher_use_sites(
    source: str, constants: dict[str, int], pointer_layout_row: dict[str, Any]
) -> dict[str, Any]:
    """Parse the init wrapper and cross-check its enum/layout/load pointer relation."""
    required_constant = "MAPSETUP_OFFSET_INIT_FUNCTION"
    if required_constant not in constants:
        raise ValueError("map init dispatcher offset constant is missing")
    if (
        set(pointer_layout_row) != {"sourceOrder", "name", "offset"}
        or pointer_layout_row["name"] != "initFunction"
        or not isinstance(pointer_layout_row["sourceOrder"], int)
        or not isinstance(pointer_layout_row["offset"], int)
    ):
        raise ValueError("map init dispatcher pointer-layout row drift")
    match = re.search(
        r"(?ms)^RunMapSetupInitFunction:\s*$.*?^\s*;\s*End of function RunMapSetupInitFunction\s*$",
        source,
    )
    if match is None:
        raise ValueError("map init dispatcher function boundary is missing")
    operations = _operation_rows(match.group(0))
    expected = (
        ("save-registers", "movem.l", "d0-a1,-(sp)"),
        ("select-setup", "bsr.w", "GetCurrentMapSetup"),
        ("missing-setup-compare", "cmpi.w", "#-1,(a0)"),
        ("non-missing-branch", "bne.s", None),
        ("missing-setup-branch", "bra.w", None),
        ("load-init-pointer", "movea.l", f"{required_constant}(a0),a0"),
        ("indirect-init-call", "jsr", "(a0)"),
        ("restore-registers", "movem.l", "(sp)+,d0-a1"),
        ("return", "rts", ""),
    )
    if len(operations) != len(expected):
        raise ValueError(f"map init dispatcher operation count drift: {len(operations)}")
    records: list[dict[str, Any]] = []
    for operation, (role, opcode, operand) in zip(operations, expected, strict=True):
        if operation["opcode"] != opcode or (
            operand is not None and operation["operandText"] != operand
        ):
            raise ValueError(
                "map init dispatcher use-site drift: "
                f"{role} expected {opcode} {operand!r}, got "
                f"{operation['opcode']} {operation['operandText']!r}"
            )
        records.append(
            {
                "role": role,
                "index": operation["index"],
                "opcode": operation["opcode"],
                "operandText": operation["operandText"],
            }
        )
    non_missing_target = operations[3]["branchTargetSymbol"]
    missing_target = operations[4]["branchTargetSymbol"]
    pointer_labels = operations[5]["labels"]
    restore_labels = operations[7]["labels"]
    if non_missing_target not in pointer_labels or missing_target not in restore_labels:
        raise ValueError("map init dispatcher branch-target relation drift")
    load_operation = operations[5]
    load_match = re.fullmatch(
        r"([A-Z][A-Z0-9_]*)\(a0\),a0", load_operation["operandText"]
    )
    if load_match is None or load_match.group(1) != required_constant:
        raise ValueError("map init dispatcher pointer-load operand drift")
    resolved_offset = constants[load_match.group(1)]
    if resolved_offset != pointer_layout_row["offset"]:
        raise ValueError(
            "map init dispatcher enum/layout pointer-offset relation drift: "
            f"{resolved_offset} != {pointer_layout_row['offset']}"
        )
    return {
        "pointerOffsetConstant": {
            "name": required_constant,
            "value": constants[required_constant],
        },
        "pointerLayoutRow": pointer_layout_row,
        "pointerLoadUseSite": {
            "role": "load-init-pointer",
            "index": load_operation["index"],
            "opcode": load_operation["opcode"],
            "operandText": load_operation["operandText"],
            "offsetConstantName": load_match.group(1),
            "resolvedOffset": resolved_offset,
        },
        "useSites": records,
    }


def _statement_rows(body: str) -> list[str]:
    return [
        row["opcode"] + (f" {row['operandText']}" if row["operandText"] else "")
        for row in _operation_rows(body)
    ]


def _call_targets(statements: list[str]) -> list[str]:
    targets: list[str] = []
    for statement in statements:
        match = re.match(r"(?:jsr|bsr)(?:\.[bwl])?\s+\(?([A-Za-z_][A-Za-z0-9_]*)", statement)
        if match:
            targets.append(match.group(1))
    return targets


def _source_bodies(
    disasm: Path, addresses: dict[str, int], target_symbols: set[str]
) -> tuple[list[dict[str, Any]], dict[str, str], dict[str, str]]:
    """Parse every physical init source body once; pointer profiles are slices of these rows."""
    paths = sorted(
        (
            path
            for path in (disasm / SOURCE_ROOT).rglob("s6_initfunction*.asm")
            if "mapsetups" in path.parts
        ),
        key=lambda path: path.as_posix(),
    )
    if len(paths) != 84:
        raise ValueError(f"map init source boundary drift: {len(paths)} files")
    bodies: list[dict[str, Any]] = []
    definitions: dict[str, str] = {}
    sources: dict[str, str] = {}
    for path in paths:
        source = read_upstream_text(path)
        relative_path = path.relative_to(disasm).as_posix()
        sources[relative_path] = source
        labels = re.findall(r"^([A-Za-z_][A-Za-z0-9_]*):", source, re.MULTILINE)
        if not labels or labels[0] not in addresses:
            raise ValueError(f"map init source has no H1-bound entry label: {path}")
        owner_symbol = labels[0]
        if owner_symbol not in target_symbols:
            raise ValueError(f"map init source owns no primary setup target: {path}")
        for label in labels:
            if label not in addresses:
                raise ValueError(f"map init source label is absent from H1 listing: {label}")
            if label in definitions:
                raise ValueError(f"duplicate map init source label: {label}")
            definitions[label] = relative_path
        body = _function_body(source, owner_symbol, owner_symbol)
        operations = _operation_rows(body)
        if not operations:
            raise ValueError(f"map init source has no statements: {owner_symbol}")
        for operation in operations:
            target = operation["branchTargetSymbol"]
            if target is not None:
                address = addresses.get(target)
                if address is None and operation["localBranchTargetIndex"] is None:
                    raise ValueError(f"map init branch target lacks H1 address: {target}")
                operation["branchTargetAddress"] = address
        statements = _statement_rows(body)
        bodies.append(
            {
                "sourceOrder": len(bodies),
                "path": relative_path,
                "sourceOwnerSymbol": owner_symbol,
                "address": addresses[owner_symbol],
                "statementCount": len(statements),
                "bodySha256": hashlib.sha256(("\n".join(statements) + "\n").encode())
                .hexdigest()
                .upper(),
                "operations": operations,
            }
        )
    if not target_symbols <= set(definitions):
        missing = sorted(target_symbols - set(definitions))
        raise ValueError(f"map init pointer targets lack source labels: {missing}")
    return bodies, definitions, sources


def _macro_block(source: str, name: str) -> str:
    match = re.search(
        rf"(?ms)^{re.escape(name)}:\s+macro\s*$.*?^\s*endm\s*$", source
    )
    if match is None:
        raise ValueError(f"map init macro definition is missing: {name}")
    return match.group(0)


def _require_macro_fragments(source: str, name: str, fragments: tuple[str, ...]) -> None:
    block = _macro_block(source, name)
    lines = [
        re.sub(r"\s+", " ", line.split(";", 1)[0].strip())
        for line in block.splitlines()
        if line.split(";", 1)[0].strip()
    ]
    cursor = 0
    for fragment in fragments:
        expected = re.sub(r"\s+", " ", fragment.strip())
        try:
            cursor = lines.index(expected, cursor) + 1
        except ValueError:
            raise ValueError(
                f"map init macro shape drift: {name} lacks {fragment!r}"
            ) from None


def _validate_macro_sources(trap_macros: str, cutscene_macros: str) -> None:
    """Guard source forms that define each modeled macro family."""
    for name, fragments in {
        "chkFlg": ("trap #CHECK_FLAG", "dc.w \\1"),
        "setFlg": ("trap #SET_FLAG", "dc.w \\1"),
        "clrFlg": ("trap #CLEAR_FLAG", "dc.w \\1"),
        "script": ("lea \\1(pc), a0", "trap #MAPSCRIPT"),
        "sndCom": ("trap #SOUND_COMMAND", "dc.w \\1"),
        "txt": ("trap #TEXTBOX", "dc.w \\1"),
        "clsTxt": ("trap #TEXTBOX", "dc.w $FFFF"),
    }.items():
        _require_macro_fragments(trap_macros, name, fragments)
    for name, fragments in {
        "warp": ("dc.w $07", "dc.b \\1", "dc.b \\4"),
        "setStoryFlag": ("dc.w $13", "dc.w \\1"),
        "setPos": ("dc.w $19", "dc.b \\1", "dc.b \\4"),
        "csc_end": ("dc.w $FFFF",),
    }.items():
        _require_macro_fragments(cutscene_macros, name, fragments)


def _macro_source_contract(disasm: Path) -> dict[str, str]:
    trap_macros = read_upstream_text(disasm / TRAP_MACROS_PATH)
    cutscene_macros = read_upstream_text(disasm / CUTSCENE_MACROS_PATH)
    _validate_macro_sources(trap_macros, cutscene_macros)
    return {
        "trapMacros": TRAP_MACROS_PATH.as_posix(),
        "cutsceneMacros": CUTSCENE_MACROS_PATH.as_posix(),
    }


def _operation_family(opcode: str) -> str:
    if opcode == "chkFlg":
        return "flag-read"
    if opcode in {"setFlg", "clrFlg", "setStoryFlag"}:
        return "flag-write"
    if opcode == "script":
        return "script-invocation"
    if _is_direct_call_opcode(opcode):
        return "direct-call"
    if opcode == "setPos":
        return "entity-or-position-command"
    if opcode == "warp":
        return "warp-or-transition-command"
    if opcode in {"sndCom", "txt", "clsTxt"}:
        return "presentation-audio-text-command"
    if opcode in {
        "cmp.l",
        "cmpi.l",
        "divs.w",
        "ext.l",
        "move.b",
        "move.l",
        "move.w",
        "moveq",
        "nop",
        "swap",
        "tst.w",
    }:
        return "arithmetic-or-data-movement"
    if opcode == "jmp" or re.fullmatch(r"b(?!sr)[a-z]{2}(?:\.[bswl])?", opcode):
        return "branch-or-jump"
    if opcode in {"rts", "csc_end"}:
        return "terminal"
    raise ValueError(f"unclassified map init operation: {opcode}")


def _is_direct_call_opcode(opcode: str) -> bool:
    return re.fullmatch(r"(?:jsr|bsr)(?:\.[bswl])?", opcode) is not None


def _source_value(operand_text: str, constants: dict[str, int]) -> int:
    token = operand_text.strip()
    if token in constants:
        return constants[token]
    if token.startswith("$"):
        return int(token[1:], 16)
    return int(token, 0)


def _operand_symbol(operand_text: str) -> str:
    match = re.fullmatch(
        r"\(?([A-Za-z_][A-Za-z0-9_]*)\)?(?:\(pc\))?(?:\.[bswl])?",
        operand_text.strip(),
    )
    if match is None:
        raise ValueError(f"map init direct operand has no unique symbol: {operand_text!r}")
    return match.group(1)


def _parse_jump_interface_aliases(
    sources: dict[str, str], targets: set[str], addresses: dict[str, int], rom: bytes
) -> dict[str, str]:
    """Resolve aliases and cross-check each source jump target against the ROM instruction."""
    aliases: dict[str, str] = {}
    for _path, source in sorted(sources.items()):
        current_label: str | None = None
        for raw_line in source.splitlines():
            line = raw_line.split(";", 1)[0].strip()
            if not line:
                continue
            label_match = re.match(r"^([A-Za-z_][A-Za-z0-9_]*):\s*", line)
            if label_match:
                current_label = label_match.group(1)
                line = line[label_match.end() :].strip()
                if not line:
                    continue
            if current_label not in targets:
                continue
            parts = line.split(None, 1)
            if parts[0] != "jmp" or len(parts) != 2:
                raise ValueError(f"map init jump-interface alias shape drift: {current_label}")
            target = _operand_symbol(parts[1])
            if target not in addresses:
                raise ValueError(f"map init alias target lacks H1 address: {target}")
            if current_label not in addresses:
                raise ValueError(f"map init alias lacks H1 address: {current_label}")
            alias_address = addresses[current_label]
            instruction = rom[alias_address : alias_address + 4]
            if len(instruction) != 4 or instruction[:2] != b"\x4e\xfa":
                raise ValueError(f"map init alias ROM opcode drift: {current_label}")
            rom_target_address = alias_address + 2 + int.from_bytes(
                instruction[2:], "big", signed=True
            )
            if rom_target_address != addresses[target]:
                raise ValueError(
                    "map init alias source/ROM target drift: "
                    f"{current_label} -> {target}"
                )
            if current_label in aliases and aliases[current_label] != target:
                raise ValueError(f"conflicting map init jump-interface alias: {current_label}")
            aliases[current_label] = target
            current_label = None
    expected_aliases = {target for target in targets if target.startswith("j_")}
    if expected_aliases != set(aliases):
        raise ValueError(
            "map init jump-interface alias coverage drift: "
            f"expected {sorted(expected_aliases)}, got {sorted(aliases)}"
        )
    return aliases


def _jump_interface_aliases(
    disasm: Path, targets: set[str], addresses: dict[str, int], rom: bytes
) -> dict[str, str]:
    paths = sorted(
        (disasm / JUMP_INTERFACE_ROOT).rglob("*.asm"), key=lambda item: item.as_posix()
    )
    sources = {
        path.relative_to(disasm).as_posix(): read_upstream_text(path) for path in paths
    }
    return _parse_jump_interface_aliases(sources, targets, addresses, rom)


def _family_counts(operations: list[dict[str, Any]]) -> dict[str, int]:
    counts = Counter(operation["family"] for operation in operations)
    unexpected = set(counts) - set(FAMILY_ORDER)
    if unexpected:
        raise ValueError(f"map init family vocabulary drift: {sorted(unexpected)}")
    return {family: counts[family] for family in FAMILY_ORDER}


def _weighted_family_counts(
    profiles: list[dict[str, Any]], reference_counts: dict[str, int]
) -> dict[str, int]:
    """Weight each parsed profile family map by its parsed join-reference count."""
    if set(reference_counts) != {profile["symbol"] for profile in profiles}:
        raise ValueError("map init weighted-family target coverage drift")
    weighted = {family: 0 for family in FAMILY_ORDER}
    for profile in profiles:
        for family, count in profile["familyCounts"].items():
            weighted[family] += count * reference_counts[profile["symbol"]]
    return weighted


def _script_target_profiles(
    primary_bodies: list[dict[str, Any]],
    embedded_programs: list[dict[str, Any]],
    standalone_programs: list[dict[str, Any]],
    addresses: dict[str, int],
) -> dict[str, dict[str, Any]]:
    targets = sorted(
        {
            _operand_symbol(operation["operandText"])
            for body in primary_bodies
            for operation in body["operations"]
            if operation["opcode"] == "script"
        }
    )
    embedded = {program["id"]: program for program in embedded_programs}
    standalone = {program["id"]: program for program in standalone_programs}
    profiles: dict[str, dict[str, Any]] = {}
    for target in targets:
        if target in embedded:
            program = embedded[target]
            resolution = "embedded-init-source"
            address: int | None = program["address"]
            source_path: str | None = program["path"]
        elif target in standalone:
            program = standalone[target]
            resolution = "standalone-map-setup"
            address = program["address"]
            source_path = program["path"]
        else:
            resolution = "unresolved"
            address = addresses.get(target)
            source_path = None
        profiles[target] = {
            "symbol": target,
            "address": address,
            "resolution": resolution,
            "sourcePath": source_path,
            "callSiteCount": 0,
        }
    unresolved = [
        symbol
        for symbol, profile in profiles.items()
        if profile["resolution"] == "unresolved"
    ]
    if unresolved:
        raise ValueError(f"map init script targets lack definitions: {unresolved}")
    return profiles


def _enrich_operations(
    primary_bodies: list[dict[str, Any]],
    constants: dict[str, int],
    script_profiles: dict[str, dict[str, Any]],
    aliases: dict[str, str],
    addresses: dict[str, int],
) -> None:
    for body in primary_bodies:
        for operation in body["operations"]:
            family = _operation_family(operation["opcode"])
            operation.update(
                {
                    "family": family,
                    "flagOperand": None,
                    "scriptTargetSymbol": None,
                    "scriptTargetAddress": None,
                    "scriptTargetResolution": None,
                    "directCallInstructionTargetSymbol": None,
                    "directCallInstructionTargetAddress": None,
                    "directCallEffectiveTargetSymbol": None,
                    "directCallEffectiveTargetAddress": None,
                    "jumpTargetSymbol": None,
                    "jumpTargetAddress": None,
                }
            )
            if family == "flag-read" or operation["opcode"] in {
                "setFlg",
                "clrFlg",
                "setStoryFlag",
            }:
                try:
                    operation["flagOperand"] = _source_value(operation["operandText"], constants)
                except ValueError as error:
                    raise ValueError(
                        "map init flag macro operand is not a source value: "
                        f"{operation['operandText']!r}"
                    ) from error
            if family == "script-invocation":
                target = _operand_symbol(operation["operandText"])
                profile = script_profiles.get(target)
                if profile is None:
                    raise ValueError(f"map init script target profile is missing: {target}")
                operation["scriptTargetSymbol"] = target
                operation["scriptTargetAddress"] = profile["address"]
                operation["scriptTargetResolution"] = profile["resolution"]
            if family == "direct-call":
                target = _operand_symbol(operation["operandText"])
                if target not in addresses:
                    raise ValueError(f"map init direct call target lacks H1 address: {target}")
                effective = aliases.get(target, target)
                if effective not in addresses:
                    raise ValueError(
                        f"map init effective call target lacks H1 address: {effective}"
                    )
                operation["directCallInstructionTargetSymbol"] = target
                operation["directCallInstructionTargetAddress"] = addresses[target]
                operation["directCallEffectiveTargetSymbol"] = effective
                operation["directCallEffectiveTargetAddress"] = addresses[effective]
            if operation["opcode"] == "jmp":
                target = _operand_symbol(operation["operandText"])
                if target not in addresses:
                    raise ValueError(f"map init jump target lacks H1 address: {target}")
                operation["jumpTargetSymbol"] = target
                operation["jumpTargetAddress"] = addresses[target]


def _target_profiles(
    primary_bodies: list[dict[str, Any]],
    definitions: dict[str, str],
    target_symbols: set[str],
    addresses: dict[str, int],
    rom: bytes,
) -> list[dict[str, Any]]:
    by_path = {body["path"]: body for body in primary_bodies}
    profiles: list[dict[str, Any]] = []
    seen: set[str] = set()
    for body in primary_bodies:
        label_indices = {
            label: operation["index"]
            for operation in body["operations"]
            for label in operation["labels"]
        }
        for symbol, path in definitions.items():
            if path != body["path"] or symbol not in target_symbols:
                continue
            start_index = 0 if symbol == body["sourceOwnerSymbol"] else label_indices.get(symbol)
            if start_index is None:
                raise ValueError(f"map init target has no operation boundary: {symbol}")
            operations = body["operations"][start_index:]
            statements = [
                operation["opcode"]
                + (f" {operation['operandText']}" if operation["operandText"] else "")
                for operation in operations
            ]
            if not statements:
                raise ValueError(f"map init target has no statements: {symbol}")
            direct_return_stub = statements == ["rts"]
            address = addresses[symbol]
            if direct_return_stub and rom[address : address + 2] != b"\x4e\x75":
                raise ValueError(f"map init direct-return stub ROM drift: {symbol}")
            if not direct_return_stub and rom[address : address + 2] == b"\x4e\x75":
                raise ValueError(f"active map init unexpectedly starts with rts: {symbol}")
            profiles.append(
                {
                    "profileOrder": len(profiles),
                    "path": body["path"],
                    "symbol": symbol,
                    "address": address,
                    "sourceOwnerSymbol": body["sourceOwnerSymbol"],
                    "primarySourceEntry": symbol == body["sourceOwnerSymbol"],
                    "directReturnStub": direct_return_stub,
                    "firstOperationIndex": start_index,
                    "lastOperationIndex": operations[-1]["index"],
                    "statementCount": len(operations),
                    "operationIndices": [operation["index"] for operation in operations],
                    "bodySha256": hashlib.sha256(("\n".join(statements) + "\n").encode())
                    .hexdigest()
                    .upper(),
                    "tokenCounts": dict(
                        sorted(Counter(operation["opcode"] for operation in operations).items())
                    ),
                    "familyCounts": _family_counts(operations),
                    "flagOperands": [
                        {
                            "operationIndex": operation["index"],
                            "opcode": operation["opcode"],
                            "value": operation["flagOperand"],
                        }
                        for operation in operations
                        if operation["flagOperand"] is not None
                    ],
                    "scriptTargets": [
                        operation["scriptTargetSymbol"]
                        for operation in operations
                        if operation["scriptTargetSymbol"] is not None
                    ],
                    "directCallTargets": [
                        operation["directCallInstructionTargetSymbol"]
                        for operation in operations
                        if operation["directCallInstructionTargetSymbol"] is not None
                    ],
                    "directCallOperationIndices": [
                        operation["index"]
                        for operation in operations
                        if operation["family"] == "direct-call"
                    ],
                    "jumpOperationIndices": [
                        operation["index"]
                        for operation in operations
                        if operation["opcode"] == "jmp"
                    ],
                }
            )
            seen.add(symbol)
    if seen != target_symbols:
        raise ValueError(
            "map init target-profile coverage drift: "
            f"expected {sorted(target_symbols)}, got {sorted(seen)}"
        )
    if len(by_path) != len(primary_bodies):
        raise ValueError("map init primary-source path collision")
    return profiles


def _standalone_programs(
    disasm: Path, addresses: dict[str, int]
) -> list[dict[str, Any]]:
    paths = sorted(
        (
            path
            for path in (disasm / SOURCE_ROOT).rglob("scripts*.asm")
            if "mapsetups" in path.parts
        ),
        key=lambda path: path.as_posix(),
    )
    if len(paths) != 47:
        raise ValueError(f"standalone map script boundary drift: {len(paths)} files")
    sources = {
        path.relative_to(disasm).as_posix(): read_upstream_text(path) for path in paths
    }
    definitions: dict[str, str] = {}
    for path, source in sources.items():
        for symbol in re.findall(r"^([A-Za-z_][A-Za-z0-9_]*):", source, re.MULTILINE):
            if symbol not in addresses:
                raise ValueError(f"standalone init-script label lacks H1 address: {symbol}")
            if symbol in definitions:
                raise ValueError(f"duplicate standalone init-script label: {symbol}")
            definitions[symbol] = path
    return [
        program
        for path in sorted(sources)
        for program in _script_programs(path, sources[path], definitions, addresses)
    ]


def _script_call_sites(
    primary_bodies: list[dict[str, Any]], script_profiles: dict[str, dict[str, Any]]
) -> list[dict[str, Any]]:
    sites: list[dict[str, Any]] = []
    for body in primary_bodies:
        for operation in body["operations"]:
            target = operation["scriptTargetSymbol"]
            if target is None:
                continue
            profile = script_profiles[target]
            profile["callSiteCount"] += 1
            sites.append(
                {
                    "siteOrder": len(sites),
                    "sourceOwnerSymbol": body["sourceOwnerSymbol"],
                    "sourcePath": body["path"],
                    "operationIndex": operation["index"],
                    "targetSymbol": target,
                    "targetAddress": profile["address"],
                    "targetResolution": profile["resolution"],
                    "targetSourcePath": profile["sourcePath"],
                }
            )
    return sites


def _direct_call_sites(primary_bodies: list[dict[str, Any]]) -> list[dict[str, Any]]:
    sites: list[dict[str, Any]] = []
    for body in primary_bodies:
        for operation in body["operations"]:
            target = operation["directCallInstructionTargetSymbol"]
            if target is None:
                continue
            sites.append(
                {
                    "siteOrder": len(sites),
                    "sourceOwnerSymbol": body["sourceOwnerSymbol"],
                    "sourcePath": body["path"],
                    "operationIndex": operation["index"],
                    "instructionTargetSymbol": target,
                    "instructionTargetAddress": operation["directCallInstructionTargetAddress"],
                    "effectiveTargetSymbol": operation["directCallEffectiveTargetSymbol"],
                    "effectiveTargetAddress": operation["directCallEffectiveTargetAddress"],
                }
            )
    return sites


def _route_joins(
    setup: dict[str, Any], target_profiles: list[dict[str, Any]]
) -> tuple[list[dict[str, Any]], list[dict[str, Any]]]:
    profiles = {profile["symbol"]: profile for profile in target_profiles}
    tables = {table["symbol"]: table for table in setup["pointerTables"]}
    route_joins: list[dict[str, Any]] = []
    for route in setup["routes"]:
        selectors: list[tuple[str, int | None, str]] = [
            ("default", None, route["defaultPointer"])
        ]
        selectors.extend(
            ("flag", variant["flag"], variant["pointer"])
            for variant in route["flagVariants"]
        )
        for selector_kind, selector_flag, pointer_symbol in selectors:
            table = tables.get(pointer_symbol)
            if table is None:
                raise ValueError(f"map init route points at unknown setup table: {pointer_symbol}")
            target = table["targets"]["initFunction"]
            profile = profiles.get(target["symbol"])
            if profile is None:
                raise ValueError(f"map init route target profile is missing: {target['symbol']}")
            if target["address"] != profile["address"]:
                raise ValueError(
                    "map init route target/profile address identity drift: "
                    f"{target['symbol']}"
                )
            route_joins.append(
                {
                    "sourceOrder": len(route_joins),
                    "map": route["map"],
                    "selectorKind": selector_kind,
                    "selectorFlag": selector_flag,
                    "pointerTableSymbol": pointer_symbol,
                    "pointerTableAddress": table["address"],
                    "initTargetSymbol": target["symbol"],
                    "initTargetAddress": target["address"],
                    "targetProfileSymbol": profile["symbol"],
                    "targetProfileAddress": profile["address"],
                }
            )
    pointer_joins: list[dict[str, Any]] = []
    for table in setup["pointerTables"]:
        target = table["targets"]["initFunction"]
        profile = profiles.get(target["symbol"])
        if profile is None:
            raise ValueError(f"map init setup target profile is missing: {target['symbol']}")
        if target["address"] != profile["address"]:
            raise ValueError(
                "map init setup target/profile address identity drift: "
                f"{target['symbol']}"
            )
        route_orders = [
            route["sourceOrder"]
            for route in route_joins
            if route["pointerTableSymbol"] == table["symbol"]
        ]
        if not route_orders:
            raise ValueError(f"map init setup table has no route join: {table['symbol']}")
        pointer_joins.append(
            {
                "sourceOrder": len(pointer_joins),
                "path": table["path"],
                "pointerTableSymbol": table["symbol"],
                "pointerTableAddress": table["address"],
                "initTargetSymbol": target["symbol"],
                "initTargetAddress": target["address"],
                "targetProfileSymbol": profile["symbol"],
                "targetProfileAddress": profile["address"],
                "routeReferenceSourceOrders": route_orders,
            }
        )
    return route_joins, pointer_joins


def build_map_init_contract(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"map init H1 listing is missing: {listing_path}")
    addresses = listing_symbol_addresses(listing_path.read_text(encoding="utf-8"))
    rom = rom_path.read_bytes()
    setup = build_map_setup_contract(rom_path, upstream_path)
    if setup["upstream"]["commit"] != commit:
        raise ValueError("map init/setup provenance drift")
    pointer_layout_row = _init_function_pointer_layout_row(
        setup["sourceFacts"]["pointerLayout"]
    )
    targets = [table["targets"]["initFunction"] for table in setup["pointerTables"]]
    target_counts = Counter(target["symbol"] for target in targets)
    target_symbols = set(target_counts)
    primary_bodies, init_definitions, init_sources = _source_bodies(
        disasm, addresses, target_symbols
    )
    constants = _parse_equates(read_upstream_text(disasm / ENUMS_PATH))
    macro_sources = _macro_source_contract(disasm)
    init_paths = sorted(init_sources)
    source_programs = [
        program
        for path in init_paths
        for program in _script_programs(path, init_sources[path], init_definitions, addresses)
    ]
    embedded_programs = [
        program for program in source_programs if program["id"] not in target_symbols
    ]
    standalone_programs = _standalone_programs(disasm, addresses)
    script_profiles = _script_target_profiles(
        primary_bodies, embedded_programs, standalone_programs, addresses
    )
    direct_instruction_targets = {
        _operand_symbol(operation["operandText"])
        for body in primary_bodies
        for operation in body["operations"]
        if _is_direct_call_opcode(operation["opcode"])
    }
    aliases = _jump_interface_aliases(disasm, direct_instruction_targets, addresses, rom)
    _enrich_operations(primary_bodies, constants, script_profiles, aliases, addresses)
    profiles = _target_profiles(
        primary_bodies, init_definitions, target_symbols, addresses, rom
    )
    profiles_by_symbol = {profile["symbol"]: profile for profile in profiles}
    script_sites = _script_call_sites(primary_bodies, script_profiles)
    direct_sites = _direct_call_sites(primary_bodies)
    route_joins, pointer_joins = _route_joins(setup, profiles)
    primary_operations = [
        operation for body in primary_bodies for operation in body["operations"]
    ]
    token_counts = Counter(operation["opcode"] for operation in primary_operations)
    script_counts = Counter(site["targetSymbol"] for site in script_sites)
    direct_instruction_counts = Counter(
        site["instructionTargetSymbol"] for site in direct_sites
    )
    direct_effective_counts = Counter(site["effectiveTargetSymbol"] for site in direct_sites)
    target_setup_counts = {profile["symbol"]: 0 for profile in profiles}
    for pointer in pointer_joins:
        target_setup_counts[pointer["targetProfileSymbol"]] += 1
    target_route_counts = {profile["symbol"]: 0 for profile in profiles}
    for route in route_joins:
        target_route_counts[route["targetProfileSymbol"]] += 1
    if Counter(target_setup_counts.values()).get(0, 0):
        raise ValueError("map init target lacks a pointer-table reference")
    family_counts = _family_counts(primary_operations)
    setup_weighted_family_counts = _weighted_family_counts(profiles, target_setup_counts)
    route_weighted_family_counts = _weighted_family_counts(profiles, target_route_counts)
    setup_statement_reference_count = sum(
        profiles_by_symbol[symbol]["statementCount"] * count
        for symbol, count in target_setup_counts.items()
    )
    route_statement_reference_count = sum(
        profiles_by_symbol[symbol]["statementCount"] * count
        for symbol, count in target_route_counts.items()
    )
    if sum(setup_weighted_family_counts.values()) != setup_statement_reference_count:
        raise ValueError("map init setup weighted-family identity drift")
    if sum(route_weighted_family_counts.values()) != route_statement_reference_count:
        raise ValueError("map init route weighted-family identity drift")
    embedded_program_ids = {program["id"] for program in embedded_programs}
    embedded_script_targets = set(script_profiles) & embedded_program_ids
    script_target_profiles = [script_profiles[symbol] for symbol in sorted(script_profiles)]
    unclassified_operations: list[dict[str, Any]] = []
    summary = {
        "sourceFileCount": len(primary_bodies),
        "setupPointerReferenceCount": len(pointer_joins),
        "routeReferenceCount": len(route_joins),
        "uniqueTargetCount": len(profiles),
        "internalEntryTargetCount": sum(
            not profile["primarySourceEntry"] for profile in profiles
        ),
        "aliasedTargetCount": sum(count > 1 for count in target_counts.values()),
        "activeFunctionCount": sum(not profile["directReturnStub"] for profile in profiles),
        "directReturnStubCount": sum(profile["directReturnStub"] for profile in profiles),
        "activeSetupReferenceCount": sum(
            target_setup_counts[profile["symbol"]]
            for profile in profiles
            if not profile["directReturnStub"]
        ),
        "directReturnStubReferenceCount": sum(
            target_setup_counts[profile["symbol"]]
            for profile in profiles
            if profile["directReturnStub"]
        ),
        "sourceStatementCount": len(primary_operations),
        "targetProfileStatementCount": sum(profile["statementCount"] for profile in profiles),
        "setupStatementReferenceCount": setup_statement_reference_count,
        "routeStatementReferenceCount": route_statement_reference_count,
        "maximumFunctionStatementCount": max(
            profile["statementCount"] for profile in profiles
        ),
        "flagReadCount": family_counts["flag-read"],
        "flagWriteCount": family_counts["flag-write"],
        "scriptCallCount": len(script_sites),
        "uniqueScriptTargetCount": len(script_target_profiles),
        "embeddedInitSourceScriptTargetCount": sum(
            profile["resolution"] == "embedded-init-source"
            for profile in script_target_profiles
        ),
        "standaloneScriptTargetCount": sum(
            profile["resolution"] == "standalone-map-setup"
            for profile in script_target_profiles
        ),
        "unresolvedScriptTargetCount": sum(
            profile["resolution"] == "unresolved" for profile in script_target_profiles
        ),
        "directCallCount": len(direct_sites),
        "uniqueDirectCallInstructionTargetCount": len(direct_instruction_counts),
        "uniqueDirectCallEffectiveTargetCount": len(direct_effective_counts),
        "jumpOperationCount": token_counts["jmp"],
        "labeledOperationCount": sum(bool(operation["labels"]) for operation in primary_operations),
        "branchOperationCount": sum(
            bool(re.fullmatch(r"b(?!sr)[a-z]{2}(?:\.[bswl])?", operation["opcode"]))
            for operation in primary_operations
        ),
        "resolvedLocalBranchCount": sum(
            operation["localBranchTargetIndex"] is not None for operation in primary_operations
        ),
        "resolvedBranchTargetCount": sum(
            operation["localBranchTargetIndex"] is not None
            or operation["branchTargetAddress"] is not None
            for operation in primary_operations
        ),
        "externalBranchTargetCount": sum(
            operation["localBranchTargetIndex"] is None
            and operation["branchTargetAddress"] is not None
            for operation in primary_operations
        ),
        "sourceGlobalLabelCount": len(init_definitions),
        "embeddedProgramCount": len(embedded_programs),
        "embeddedOperationCount": sum(len(row["operations"]) for row in embedded_programs),
        "embeddedTargetReferenceCount": sum(
            len(operation["targetSymbols"])
            for row in embedded_programs
            for operation in row["operations"]
        ),
        "embeddedScriptTargetCount": len(embedded_script_targets),
        "unclassifiedOperationCount": len(unclassified_operations),
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "romSha256": setup["romSha256"],
        "scope": f"{SOURCE_ROOT.as_posix()}/*/mapsetups/s6_initfunction*.asm",
        "sources": {
            "dispatcher": DISPATCH_PATH.as_posix(),
            "constants": ENUMS_PATH.as_posix(),
            **macro_sources,
            "jumpInterfaceRoot": JUMP_INTERFACE_ROOT.as_posix(),
        },
        "function": {"RunMapSetupInitFunction": addresses["RunMapSetupInitFunction"]},
        "summary": summary,
        "entryTokenCounts": dict(sorted(token_counts.items())),
        "operationFamilyCounts": family_counts,
        "setupWeightedOperationFamilyCounts": setup_weighted_family_counts,
        "routeWeightedOperationFamilyCounts": route_weighted_family_counts,
        "targetSetupReferenceCounts": target_setup_counts,
        "targetRouteReferenceCounts": target_route_counts,
        "directCallInstructionTargetCounts": dict(sorted(direct_instruction_counts.items())),
        "directCallEffectiveTargetCounts": dict(sorted(direct_effective_counts.items())),
        "scriptTargetCounts": dict(sorted(script_counts.items())),
        "duplicatePointerTargets": [
            {"symbol": symbol, "setupReferenceCount": count}
            for symbol, count in sorted(target_counts.items())
            if count > 1
        ],
        "dispatcher": _dispatcher_use_sites(
            read_upstream_text(disasm / DISPATCH_PATH), constants, pointer_layout_row
        ),
        "runtimeQuestions": [
            {
                "id": "map-init-effects-and-presentation",
                "status": "unknown",
                "questions": [
                    "init-script-side-effects-and-transition-persistence",
                    "init-entity-mutation-order-and-visibility-timing",
                    "init-fade-audio-and-presentation-sequencing",
                ],
            }
        ],
        "primarySourceOrder": [body["sourceOwnerSymbol"] for body in primary_bodies],
        "primaryOperationOrder": [
            f"{body['sourceOwnerSymbol']}:{operation['index']}"
            for body in primary_bodies
            for operation in body["operations"]
        ],
        "targetProfileOrder": [profile["symbol"] for profile in profiles],
        "routeJoinOrder": [
            f"{route['map']}:{route['selectorKind']}:{route['selectorFlag']}:{route['pointerTableSymbol']}"
            for route in route_joins
        ],
        "pointerTableJoinOrder": [pointer["pointerTableSymbol"] for pointer in pointer_joins],
        "scriptTargetProfileOrder": [profile["symbol"] for profile in script_target_profiles],
        "scriptCallSiteOrder": [
            f"{site['sourceOwnerSymbol']}:{site['operationIndex']}:{site['targetSymbol']}"
            for site in script_sites
        ],
        "directCallSiteOrder": [
            f"{site['sourceOwnerSymbol']}:{site['operationIndex']}:{site['instructionTargetSymbol']}"
            for site in direct_sites
        ],
        "unclassifiedOperations": unclassified_operations,
        "primarySourceBodies": primary_bodies,
        "sourceFiles": profiles,
        "routeJoins": route_joins,
        "pointerTableJoins": pointer_joins,
        "scriptTargetProfiles": script_target_profiles,
        "scriptCallSites": script_sites,
        "directCallSites": direct_sites,
        "embeddedPrograms": embedded_programs,
    }


def verify_map_init_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_map_init_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="map init static contract")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != output["romSha256"]
        or fixture["function"] != output["function"]
    ):
        raise ValueError("map init provenance/address drift")
    expected = {
        field: value
        for field, value in output.items()
        if field not in {"schemaVersion", "id", "upstream", "romSha256", "function"}
    }
    if fixture["expected"] != expected:
        raise ValueError("map init complete semantic contract drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"] or output["summary"] != manifest["summary"]:
        raise ValueError("map init canonical output drift")
    destination = output_path or repo_path("local/derived/map-init-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "SourceFiles": output["summary"]["sourceFileCount"],
        "ActiveFunctions": output["summary"]["activeFunctionCount"],
        "DirectReturnStubs": output["summary"]["directReturnStubCount"],
        "Statements": output["summary"]["sourceStatementCount"],
        "RouteJoins": output["summary"]["routeReferenceCount"],
        "ScriptCalls": output["summary"]["scriptCallCount"],
        "Status": "PASS",
    }
