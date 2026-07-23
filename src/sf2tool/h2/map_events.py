from __future__ import annotations

import hashlib
import json
import re
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.h2.map_entities import build_map_entities_contract
from sf2tool.h2.map_setup import build_map_setup_contract
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.source_text import read_upstream_text

ID = "sf2-map-events-static-v1"
SOURCE_ROOT = Path("data/maps/entries")
MAP_SETUP_MACROS_PATH = Path("sf2mapsetupmacros.asm")
MANIFEST = repo_path("manifests/extractions/map-events-static.json")
SCHEMA = repo_path("schemas/map-events-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/map-events-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-map-events-static-fixture.schema.json")

CATEGORY_CONFIG = {
    "entityEvents": {
        "glob": "s2_entityevents*.asm",
        "recordBytes": 4,
        "specificMacros": ("msEntityEvent",),
        "defaultMacros": ("msDefaultEntityEvent", "msDftEntityEvent"),
        "stubSymbols": ("ms_map52_EntityEvents", "ms_map55_EntityEvents"),
    },
    "zoneEvents": {
        "glob": "s3_zoneevents*.asm",
        "recordBytes": 4,
        "specificMacros": ("msZoneEvent",),
        "defaultMacros": ("msDefaultZoneEvent",),
        "stubSymbols": (),
    },
    "itemEvents": {
        "glob": "s5_itemevents*.asm",
        "recordBytes": 6,
        "specificMacros": ("msItemEvent",),
        "defaultMacros": ("msDefaultItemEvent",),
        "stubSymbols": (),
    },
}

RAW_ZONE_DEFAULT_SYMBOL = "ms_map44_ZoneEvents"
FUNCTION_SYMBOLS = (
    "RunMapSetupEntityEvent",
    "RunMapSetupZoneEvent",
    "RunMapSetupItemEvent",
)
REACHABILITY_FUNCTION_SYMBOLS = (
    "ProcessPlayerAction",
    "GetActivatedEntity",
    "GetEntityEventIndex",
)
SELECTION_INPUTS = (
    ("entity-specific-after-scan", "entityEvents", 3, (), {"entity": 128}),
    ("entity-default", "entityEvents", 3, (), {"entity": 135}),
    ("zone-exact", "zoneEvents", 3, (), {"x": 27, "y": 5}),
    ("zone-wildcard-y", "zoneEvents", 3, (), {"x": 2, "y": 42}),
    ("zone-first-overlapping-match", "zoneEvents", 3, (609,), {"x": 2, "y": 23}),
    ("zone-default", "zoneEvents", 3, (), {"x": 10, "y": 10}),
    (
        "item-index-mask",
        "itemEvents",
        8,
        (),
        {"x": 15, "y": 19, "facing": 1, "item": 240},
    ),
    (
        "item-facing-mismatch-default",
        "itemEvents",
        8,
        (),
        {"x": 15, "y": 19, "facing": 2, "item": 112},
    ),
    (
        "item-wildcard-facing",
        "itemEvents",
        22,
        (),
        {"x": 35, "y": 24, "facing": 3, "item": 125},
    ),
)

_PROGRAM_LABEL = re.compile(r"^\s*([A-Za-z_][A-Za-z0-9_]*):\s*(.*)$")
_PROGRAM_OPERATION = re.compile(
    r"^(?P<mnemonic>[A-Za-z][A-Za-z0-9_]*)(?P<suffix>\.[bBwWlLsS])?"
    r"(?:\s+(?P<operands>.+))?$"
)
_PROGRAM_END = re.compile(r"^\s*;\s*End of function ([A-Za-z_][A-Za-z0-9_]*)\s*$")
_LISTING_LINE = re.compile(r"^([0-9A-Fa-f]{8})(.*)$")
_PARENTHESIZED_TARGET = re.compile(r"^\(([A-Za-z_][A-Za-z0-9_]*)\)\.[bBwWlL]$")
_PLAIN_TARGET = re.compile(r"^[A-Za-z_][A-Za-z0-9_]*$")
_PC_RELATIVE_TARGET = re.compile(r"^([A-Za-z_][A-Za-z0-9_]*)\([pP][cC]\)$")

_CONTROL_FLOW_KINDS = {
    "beq": "conditional-branch",
    "bne": "conditional-branch",
    "bra": "unconditional-branch",
    "bsr": "direct-call",
    "jsr": "direct-call",
    "jmp": "direct-jump",
    "rts": "return",
}
_CONTROL_FLOW_COUNT_FIELDS = (
    "conditionalBranchSiteCount",
    "unconditionalBranchSiteCount",
    "directCallSiteCount",
    "directJumpSiteCount",
)


def _normalise_asm_statement(value: str) -> str:
    """Compare source and H1 statements without treating comments as code."""
    return re.sub(r"\s+", " ", value.split(";", 1)[0].strip())


def _listing_statement(raw_line: str) -> tuple[int, str] | None:
    """Return one H1 address/source statement, excluding macro-expansion rows."""
    line_match = _LISTING_LINE.match(raw_line)
    if line_match is None:
        return None
    address = int(line_match.group(1), 16)
    remainder = line_match.group(2)
    byte_match = re.match(
        r"^\s*(?:[0-9A-Fa-f]{2,4})(?:\s+[0-9A-Fa-f]{2,4})*\s{2,}(.*)$",
        remainder,
    )
    text = byte_match.group(1) if byte_match is not None else remainder
    statement = _normalise_asm_statement(text)
    if statement.startswith("M "):
        return None
    return address, statement


def _operation_target_symbol(operand_texts: list[str], source_line: int) -> str:
    if len(operand_texts) != 1:
        raise ValueError(
            f"map entity-event control-flow operand drift at source line {source_line}"
        )
    operand = operand_texts[0]
    plain_match = _PLAIN_TARGET.fullmatch(operand)
    if plain_match is not None:
        return operand
    parenthesized_match = _PARENTHESIZED_TARGET.fullmatch(operand)
    if parenthesized_match is not None:
        return parenthesized_match.group(1)
    pc_relative_match = _PC_RELATIVE_TARGET.fullmatch(operand)
    if pc_relative_match is not None:
        return pc_relative_match.group(1)
    raise ValueError(
        f"map entity-event control-flow target form drift at source line {source_line}"
    )


def _parse_program_operation(
    statement: str, *, source_line: int, source_order: int
) -> dict[str, Any]:
    """Parse one source operation while retaining its raw mnemonic and operands."""
    match = _PROGRAM_OPERATION.fullmatch(statement)
    if match is None:
        raise ValueError(f"map entity-event operation syntax drift at source line {source_line}")
    raw_mnemonic = match.group("mnemonic") + (match.group("suffix") or "")
    operand_text = match.group("operands")
    operand_texts = _split_macro_operands(operand_text) if operand_text else []
    mnemonic = match.group("mnemonic").lower()
    control_flow_kind = _CONTROL_FLOW_KINDS.get(mnemonic, "ordinary")
    target_symbol = (
        _operation_target_symbol(operand_texts, source_line)
        if control_flow_kind
        in {"conditional-branch", "unconditional-branch", "direct-call", "direct-jump"}
        else None
    )
    return {
        "sourceOrder": source_order,
        "sourceLine": source_line,
        "sourceMnemonic": raw_mnemonic,
        "mnemonic": mnemonic,
        "sizeSuffix": match.group("suffix").lower() if match.group("suffix") else None,
        "operandTexts": operand_texts,
        "controlFlowKind": control_flow_kind,
        "instructionTargetSymbol": target_symbol,
    }


def _source_program_block(
    disasm: Path, profile: dict[str, Any], addresses: dict[str, int]
) -> dict[str, Any]:
    """Parse one entity-event source block through its annotated function boundary."""
    source_path = profile["ownerSourcePath"]
    lines = read_upstream_text(disasm / source_path).splitlines()
    entry_line = profile["ownerSourceLine"]
    if not 1 <= entry_line <= len(lines):
        raise ValueError(f"map entity-event entry line is out of range: {source_path}")
    entry_match = _PROGRAM_LABEL.fullmatch(lines[entry_line - 1])
    if entry_match is None or entry_match.group(1) != profile["canonicalSymbol"]:
        raise ValueError(f"map entity-event entry label drift: {profile['canonicalSymbol']}")
    if profile["targetH1Address"] != addresses[profile["canonicalSymbol"]]:
        raise ValueError(f"map entity-event entry H1 address drift: {profile['canonicalSymbol']}")

    end_index: int | None = None
    end_symbol: str | None = None
    for index in range(entry_line, len(lines)):
        end_match = _PROGRAM_END.fullmatch(lines[index])
        if end_match is not None:
            end_index = index
            end_symbol = end_match.group(1)
            break
    if end_index is None or end_symbol is None:
        raise ValueError(f"map entity-event source function boundary is missing: {source_path}")

    labels: list[dict[str, Any]] = []
    operations: list[dict[str, Any]] = []
    for index in range(entry_line - 1, end_index):
        raw_line = lines[index]
        statement = _normalise_asm_statement(raw_line)
        if not statement:
            continue
        label_match = _PROGRAM_LABEL.fullmatch(statement)
        trailing_operation: str | None = None
        if label_match is None:
            inline_label = _PROGRAM_LABEL.match(statement)
            if inline_label is not None:
                label_match = inline_label
                trailing_operation = inline_label.group(2).strip()
        if label_match is not None:
            symbol = label_match.group(1)
            if symbol not in addresses:
                raise ValueError(f"map entity-event label lacks H1 address: {symbol}")
            labels.append(
                {
                    "sourceOrder": len(labels),
                    "sourceLine": index + 1,
                    "symbol": symbol,
                    "address": addresses[symbol],
                }
            )
            statement = (
                trailing_operation
                if trailing_operation is not None
                else label_match.group(2).strip()
            )
        if statement:
            operation = _parse_program_operation(
                statement, source_line=index + 1, source_order=len(operations)
            )
            operation["sourceStatement"] = statement
            operations.append(operation)
    if not labels or labels[0]["symbol"] != profile["canonicalSymbol"]:
        raise ValueError(
            f"map entity-event entry label coverage drift: {profile['canonicalSymbol']}"
        )
    if not operations:
        raise ValueError(f"map entity-event has no operations: {profile['canonicalSymbol']}")
    return {
        "labels": labels,
        "operations": operations,
        "endFunctionSymbol": end_symbol,
        "endSourceLine": end_index + 1,
    }


def _listing_entry_index(
    listing_index: dict[str, dict[Any, Any]], *, symbol: str, address: int
) -> int:
    entry_index = listing_index["entries"].get((symbol, address))
    if entry_index is None:
        raise ValueError(f"map entity-event H1 entry listing drift: {symbol}")
    return entry_index


def _listing_program_end(
    listing_index: dict[str, dict[Any, Any]], *, entry_index: int, end_function_symbol: str
) -> tuple[int, int]:
    boundary = listing_index["ends"].get(end_function_symbol)
    if boundary is None or boundary[0] <= entry_index:
        raise ValueError(f"map entity-event H1 function end drift: {end_function_symbol}")
    return boundary


def _h1_program_index(listing_lines: list[str]) -> dict[str, dict[Any, Any]]:
    """Index H1 labels and source function-end markers once for the full program corpus."""
    entries: dict[tuple[str, int], int] = {}
    ends: dict[str, tuple[int, int]] = {}
    for index, raw_line in enumerate(listing_lines):
        row = _listing_statement(raw_line)
        if row is not None and row[1].endswith(":"):
            symbol = row[1][:-1]
            if _PLAIN_TARGET.fullmatch(symbol) is not None:
                identity = (symbol, row[0])
                if identity in entries:
                    raise ValueError(f"map entity-event duplicate H1 label entry: {symbol}")
                entries[identity] = index
        line_match = _LISTING_LINE.match(raw_line)
        if line_match is None:
            continue
        end_match = _PROGRAM_END.fullmatch(line_match.group(2))
        if end_match is None:
            continue
        symbol = end_match.group(1)
        if symbol in ends:
            raise ValueError(f"map entity-event duplicate H1 function end: {symbol}")
        ends[symbol] = (index, int(line_match.group(1), 16))
    return {"entries": entries, "ends": ends}


def _bind_operations_to_h1(
    listing_lines: list[str],
    listing_index: dict[str, dict[Any, Any]],
    *,
    profile: dict[str, Any],
    block: dict[str, Any],
) -> int:
    """Guard source opcode/operand/order against the pinned H1 listing before fixtures."""
    entry_address = profile["targetH1Address"]
    entry_index = _listing_entry_index(
        listing_index, symbol=profile["canonicalSymbol"], address=entry_address
    )
    end_index, end_address = _listing_program_end(
        listing_index,
        entry_index=entry_index,
        end_function_symbol=block["endFunctionSymbol"],
    )
    if end_address <= entry_address:
        raise ValueError(
            f"map entity-event H1 nonpositive program span: {profile['canonicalSymbol']}"
        )
    cursor = entry_index + 1
    for operation in block["operations"]:
        expected_statement = operation["sourceStatement"]
        matched: tuple[int, int] | None = None
        for index in range(cursor, end_index):
            row = _listing_statement(listing_lines[index])
            if row is not None and row[1] == expected_statement:
                matched = (index, row[0])
                break
        if matched is None:
            raise ValueError(
                "map entity-event source/H1 operation relationship drift: "
                f"{profile['canonicalSymbol']}:{operation['sourceLine']}"
            )
        cursor, operation["address"] = matched[0] + 1, matched[1]
        if not entry_address <= operation["address"] < end_address:
            raise ValueError(
                f"map entity-event operation address falls outside program span: "
                f"{profile['canonicalSymbol']}:{operation['sourceLine']}"
            )
        del operation["sourceStatement"]
    return end_address


def _alias_target_symbol(operand_texts: list[str], source_line: int) -> str:
    if len(operand_texts) != 1:
        raise ValueError(
            f"map entity-event jump-interface operand drift at source line {source_line}"
        )
    operand = operand_texts[0]
    target_match = _PC_RELATIVE_TARGET.fullmatch(operand)
    if target_match is not None:
        return target_match.group(1)
    return _operation_target_symbol(operand_texts, source_line)


def _parse_jump_interface_aliases(
    disasm: Path,
    addresses: dict[str, int],
    listing_lines: list[str],
    listing_index: dict[str, dict[Any, Any]],
    label_owners: dict[int, list[dict[str, Any]]],
    aliases: list[str],
) -> dict[str, dict[str, Any]]:
    """Resolve each called `j_` interface through its source/H1 jump definition."""
    definitions: dict[str, dict[str, Any]] = {}
    for alias in aliases:
        if alias not in addresses:
            raise ValueError(f"map entity-event jump-interface lacks H1 address: {alias}")
        owner_matches = [
            owner for owner in label_owners.get(addresses[alias], []) if owner["symbol"] == alias
        ]
        if len(owner_matches) != 1:
            raise ValueError(f"map entity-event jump-interface owner drift: {alias}")
        owner = owner_matches[0]
        source_lines = read_upstream_text(disasm / owner["sourcePath"]).splitlines()
        source_index = owner["sourceLine"] - 1
        label_match = _PROGRAM_LABEL.fullmatch(source_lines[source_index])
        if label_match is None or label_match.group(1) != alias:
            raise ValueError(f"map entity-event jump-interface label drift: {alias}")
        operation: dict[str, Any] | None = None
        for index in range(source_index + 1, len(source_lines)):
            statement = _normalise_asm_statement(source_lines[index])
            if not statement:
                continue
            if _PROGRAM_LABEL.fullmatch(statement) is not None:
                break
            operation = _parse_program_operation(statement, source_line=index + 1, source_order=0)
            break
        if operation is None or operation["controlFlowKind"] != "direct-jump":
            raise ValueError(f"map entity-event jump-interface definition drift: {alias}")
        target_symbol = _alias_target_symbol(operation["operandTexts"], operation["sourceLine"])
        if target_symbol not in addresses:
            raise ValueError(f"map entity-event jump-interface target lacks H1 address: {alias}")
        entry_index = _listing_entry_index(
            listing_index, symbol=alias, address=addresses[alias]
        )
        expected_statement = _normalise_asm_statement(
            source_lines[operation["sourceLine"] - 1]
        )
        h1_row: tuple[int, str] | None = None
        for raw_line in listing_lines[entry_index + 1 :]:
            row = _listing_statement(raw_line)
            if row is not None and row[1].endswith(":"):
                break
            if row is not None and row[1] == expected_statement:
                h1_row = row
                break
        if h1_row is None:
            raise ValueError(f"map entity-event jump-interface source/H1 drift: {alias}")
        definitions[alias] = {
            "aliasSymbol": alias,
            "aliasAddress": addresses[alias],
            "sourcePath": owner["sourcePath"],
            "sourceLine": owner["sourceLine"],
            "definitionSourceLine": operation["sourceLine"],
            "sourceMnemonic": operation["sourceMnemonic"],
            "mnemonic": operation["mnemonic"],
            "sizeSuffix": operation["sizeSuffix"],
            "operandTexts": operation["operandTexts"],
            "directTargetSymbol": target_symbol,
            "directTargetAddress": addresses[target_symbol],
            "listingAddress": h1_row[0],
        }
    return definitions


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _macro_block(source: str, macro: str) -> tuple[str, int]:
    match = re.search(rf"(?ms)^{re.escape(macro)}:\s+macro\s*$.*?^\s*endm\s*$", source)
    if match is None:
        raise ValueError(f"map event macro definition is missing: {macro}")
    return match.group(0), source[: match.start()].count("\n") + 1


def _directive_width(directive: str) -> int:
    return {"dc.b": 1, "dc.w": 2, "dc.l": 4}[directive]


def _macro_definition(source: str, macro: str, kind: str) -> dict[str, Any]:
    """Parse byte-emitting macro positions that bind a source use site to a record."""
    block, definition_line = _macro_block(source, macro)
    directives: list[dict[str, Any]] = []
    for raw_line in block.splitlines():
        line = raw_line.split(";", 1)[0].strip()
        if not line or line == f"{macro}: macro" or line == "endm":
            continue
        parts = line.split(None, 1)
        if len(parts) != 2 or parts[0] not in {"dc.b", "dc.w", "dc.l"}:
            raise ValueError(f"map event macro directive drift: {macro}: {line!r}")
        positions = [int(value) for value in re.findall(r"\\(\d+)", parts[1])]
        directives.append(
            {
                "sourceOrder": len(directives),
                "directive": parts[0],
                "operandText": parts[1],
                "widthBytes": _directive_width(parts[0]),
                "argumentPositions": positions,
            }
        )
    if not directives:
        raise ValueError(f"map event macro has no byte-emitting directives: {macro}")
    target_directives = [
        directive
        for directive in directives
        if directive["directive"] == "dc.w" and re.fullmatch(r"\\(\d+)", directive["operandText"])
    ]
    if len(target_directives) != 1:
        raise ValueError(f"map event macro target operand drift: {macro}")
    if target_directives[0]["sourceOrder"] != len(directives) - 1:
        raise ValueError(f"map event macro target directive order drift: {macro}")
    target_position = target_directives[0]["argumentPositions"][0]
    argument_positions = sorted(
        {position for directive in directives for position in directive["argumentPositions"]}
    )
    if argument_positions != list(range(1, max(argument_positions, default=0) + 1)):
        raise ValueError(f"map event macro argument positions drift: {macro}")
    marker: int | None = None
    if kind == "default":
        first_operand = directives[0]["operandText"]
        marker_match = re.fullmatch(r"\$([0-9A-Fa-f]+)", first_operand)
        if marker_match is None:
            raise ValueError(f"map event default macro marker drift: {macro}")
        literal = int(marker_match.group(1), 16)
        marker = literal >> ((directives[0]["widthBytes"] - 1) * 8)
    return {
        "macro": macro,
        "kind": kind,
        "definitionLine": definition_line,
        "argumentCount": max(argument_positions, default=0),
        "targetOperandPosition": target_position,
        "defaultMarker": marker,
        "encodedRecordBytes": sum(directive["widthBytes"] for directive in directives),
        "emittedDirectives": directives,
    }


def _event_macro_definitions(disasm: Path) -> dict[str, list[dict[str, Any]]]:
    source = read_upstream_text(disasm / MAP_SETUP_MACROS_PATH)
    definitions: dict[str, list[dict[str, Any]]] = {}
    for category, config in CATEGORY_CONFIG.items():
        category_definitions = [
            _macro_definition(source, macro, "specific") for macro in config["specificMacros"]
        ] + [_macro_definition(source, macro, "default") for macro in config["defaultMacros"]]
        if any(
            definition["encodedRecordBytes"] != config["recordBytes"]
            for definition in category_definitions
        ):
            raise ValueError(f"map event macro record width drift: {category}")
        definitions[category] = category_definitions
    return definitions


def _split_macro_operands(text: str) -> list[str]:
    operands: list[str] = []
    start = 0
    depth = 0
    for index, character in enumerate(text):
        if character == "(":
            depth += 1
        elif character == ")":
            depth -= 1
            if depth < 0:
                raise ValueError(f"map event macro operand has unmatched ')': {text!r}")
        elif character == "," and depth == 0:
            operands.append(text[start:index].strip())
            start = index + 1
    if depth != 0:
        raise ValueError(f"map event macro operand has unmatched '(': {text!r}")
    operands.append(text[start:].strip())
    if not all(operands):
        raise ValueError(f"map event macro has empty operand: {text!r}")
    return operands


def _relative_target_expression(expression: str, table_symbol: str) -> dict[str, Any]:
    """Parse the source expression whose signed word is decoded from the table base."""
    compact = re.sub(r"\s+", "", expression)
    masked_to_16_bits = False
    mask_match = re.fullmatch(r"\((.+)\)&\$FFFF", compact, re.IGNORECASE)
    if mask_match is not None:
        compact = mask_match.group(1)
        masked_to_16_bits = True
    target_match = re.fullmatch(
        rf"(?P<symbol>[A-Za-z_][A-Za-z0-9_]*)(?P<adjustment>[+-](?:\$[0-9A-Fa-f]+|\d+))?"
        rf"-(?P<base>{re.escape(table_symbol)})",
        compact,
    )
    if target_match is None:
        raise ValueError(
            f"map event target expression does not resolve from its table base: {expression!r}"
        )
    adjustment_text = target_match.group("adjustment")
    adjustment = 0
    if adjustment_text:
        sign = -1 if adjustment_text.startswith("-") else 1
        token = adjustment_text[1:]
        adjustment = sign * (int(token[1:], 16) if token.startswith("$") else int(token))
    return {
        "targetExpression": expression,
        "targetBaseSymbol": target_match.group("symbol"),
        "targetBaseAdjustment": adjustment,
        "relativeBaseSymbol": target_match.group("base"),
        "maskedTo16Bits": masked_to_16_bits,
    }


def _event_macro_use_sites(
    source: str,
    *,
    category: str,
    path: str,
    table_symbol: str,
    definitions: list[dict[str, Any]],
) -> list[dict[str, Any]]:
    """Parse exact event-macro source rows without matching comments or near-miss names."""
    by_macro = {definition["macro"]: definition for definition in definitions}
    sites: list[dict[str, Any]] = []
    for line_number, raw_line in enumerate(source.splitlines(), start=1):
        line = raw_line.split(";", 1)[0].strip()
        line = re.sub(r"^[A-Za-z_][A-Za-z0-9_]*:\s*", "", line).strip()
        if not line:
            continue
        match = re.fullmatch(r"([A-Za-z_][A-Za-z0-9_]*)\s+(.+)", line)
        if match is None or match.group(1) not in by_macro:
            continue
        macro = match.group(1)
        definition = by_macro[macro]
        operands = _split_macro_operands(match.group(2))
        if len(operands) != definition["argumentCount"]:
            raise ValueError(f"map event macro operand count drift: {path}:{line_number}")
        expression = operands[definition["targetOperandPosition"] - 1]
        sites.append(
            {
                "sourceOrder": len(sites),
                "sourcePath": path,
                "sourceLine": line_number,
                "sourceTableSymbol": table_symbol,
                "macro": macro,
                "kind": definition["kind"],
                "operandTexts": operands,
                "sourceDefaultMarker": definition["defaultMarker"],
                "sourceMarkerWord": None,
                **_relative_target_expression(expression, table_symbol),
            }
        )
    if category == "zoneEvents" and table_symbol == RAW_ZONE_DEFAULT_SYMBOL:
        raw_rows = [
            (line_number, raw_line.split(";", 1)[0].strip())
            for line_number, raw_line in enumerate(source.splitlines(), start=1)
            if raw_line.split(";", 1)[0].strip()
        ]
        if len(raw_rows) < 3:
            raise ValueError("map 44 raw zone-default marker is missing")
        marker_match = re.fullmatch(r"dc\.w\s+\$([0-9A-Fa-f]{1,4})", raw_rows[1][1])
        if marker_match is None:
            raise ValueError("map 44 raw zone-default marker form drift")
        marker_word = int(marker_match.group(1), 16)
        marker_operand = f"${marker_match.group(1)}"
        target_line, target_statement = raw_rows[2]
        raw_match = re.fullmatch(r"dc\.w\s+(.+)", target_statement)
        if raw_match is None:
            raise ValueError("map 44 raw zone-default target expression drift")
        sites.append(
            {
                "sourceOrder": len(sites),
                "sourcePath": path,
                "sourceLine": target_line,
                "sourceTableSymbol": table_symbol,
                "macro": "raw-zone-default-expression",
                "kind": "default",
                "operandTexts": [marker_operand, raw_match.group(1)],
                "sourceDefaultMarker": marker_word >> 8,
                "sourceMarkerWord": marker_word,
                **_relative_target_expression(raw_match.group(1), table_symbol),
            }
        )
    return sites


def _decode_event_record(
    category: str, table_address: int, record_address: int, data: bytes
) -> dict[str, Any]:
    expected_size = CATEGORY_CONFIG[category]["recordBytes"]
    if len(data) != expected_size:
        raise ValueError(f"{category} record must contain {expected_size} bytes")
    relative_offset = int.from_bytes(data[-2:], "big", signed=True)
    record: dict[str, Any] = {
        "address": record_address,
        "kind": "default" if data[0] == 0xFD else "specific",
        "relativeOffset": relative_offset,
        "resolvedTargetAddress": table_address + relative_offset,
    }
    if category == "entityEvents":
        record.update({"entity": data[0], "flags": data[1]})
    elif category == "zoneEvents":
        record.update({"x": data[0], "y": data[1]})
    elif category == "itemEvents":
        record.update({"x": data[0], "y": data[1], "facing": data[2], "item": data[3]})
    else:
        raise ValueError(f"unknown map event category: {category}")
    return record


def _instruction_tokens(source: str) -> list[str]:
    tokens: list[str] = []
    for raw_line in source.splitlines():
        line = raw_line.split(";", 1)[0].strip()
        line = re.sub(r"^[A-Za-z_][A-Za-z0-9_]*:\s*", "", line).strip()
        if line:
            tokens.append(line)
    return tokens


def _event_matches(category: str, record: dict[str, Any], query: dict[str, int]) -> bool:
    if record["kind"] == "default":
        return True
    if category == "entityEvents":
        return record["entity"] == (query["entity"] & 0xFF)
    if category == "zoneEvents":
        return all(
            record[field] == 0xFF or record[field] == (query[field] & 0xFF) for field in ("x", "y")
        )
    if category == "itemEvents":
        coordinates_match = all(
            record[field] == 0xFF or record[field] == (query[field] & 0xFF)
            for field in ("x", "y", "facing")
        )
        return coordinates_match and record["item"] == (query["item"] & 0x7F)
    raise ValueError(f"unknown map event category: {category}")


def _selected_setup_symbol(
    setup: dict[str, Any], map_index: int, set_flags: set[int]
) -> str | None:
    route = next((row for row in setup["routes"] if row["map"] == map_index), None)
    if route is None:
        return None
    selected = route["defaultPointer"]
    for variant in route["flagVariants"]:
        if variant["flag"] in set_flags:
            selected = variant["pointer"]
    return selected


def _selection_cases(
    setup: dict[str, Any], categories: dict[str, dict[str, Any]]
) -> list[dict[str, Any]]:
    pointer_tables = {row["symbol"]: row for row in setup["pointerTables"]}
    event_tables = {
        category: {row["symbol"]: row for row in value["tables"]}
        for category, value in categories.items()
    }
    cases: list[dict[str, Any]] = []
    for case_id, category, map_index, flags, query in SELECTION_INPUTS:
        setup_symbol = _selected_setup_symbol(setup, map_index, set(flags))
        if setup_symbol is None:
            raise ValueError(f"selection case unexpectedly uses a missing map: {case_id}")
        table_symbol = pointer_tables[setup_symbol]["targets"][category]["symbol"]
        table = event_tables[category].get(table_symbol)
        if table is None:
            raise ValueError(f"selection case uses a direct-return event stub: {case_id}")
        selected = next(
            (row for row in table["records"] if _event_matches(category, row, query)),
            None,
        )
        if selected is None:
            raise ValueError(f"selection case has no default record: {case_id}")
        cases.append(
            {
                "id": case_id,
                "category": category,
                "map": map_index,
                "setFlags": list(flags),
                "query": query,
                "selectedSetup": setup_symbol,
                "selectedTable": table_symbol,
                "selectedRecordAddress": selected["address"],
                "selectedRecordKind": selected["kind"],
                "eventFlags": selected.get("flags"),
                "resolvedTargetAddress": selected["resolvedTargetAddress"],
            }
        )
    return cases


def _source_rows(
    disasm: Path,
    addresses: dict[str, int],
    category: str,
    definitions: list[dict[str, Any]],
) -> tuple[list[dict[str, Any]], dict[int, dict[str, Any]]]:
    config = CATEGORY_CONFIG[category]
    paths = sorted(
        (
            path
            for path in (disasm / SOURCE_ROOT).rglob(config["glob"])
            if "mapsetups" in path.parts
        ),
        key=lambda path: path.as_posix(),
    )
    files: list[dict[str, Any]] = []
    source_records: dict[int, dict[str, Any]] = {}
    for path in paths:
        source = read_upstream_text(path)
        relative_path = path.relative_to(disasm).as_posix()
        labels = re.findall(r"^([A-Za-z_][A-Za-z0-9_]*):", source, re.MULTILINE)
        if not labels or labels[0] not in addresses:
            raise ValueError(f"{category} source has no H1-bound entry label: {path}")
        symbol = labels[0]
        address = addresses[symbol]
        is_stub = symbol in config["stubSymbols"]
        if is_stub and _instruction_tokens(source) != ["rts"]:
            raise ValueError(f"{category} direct-return stub shape drift: {symbol}")

        use_sites = _event_macro_use_sites(
            source,
            category=category,
            path=relative_path,
            table_symbol=symbol,
            definitions=definitions,
        )
        kinds = [site["kind"] for site in use_sites]
        macro_counts: Counter[str] = Counter(site["macro"] for site in use_sites)
        is_raw_default = category == "zoneEvents" and symbol == RAW_ZONE_DEFAULT_SYMBOL
        if is_raw_default and [site["macro"] for site in use_sites] != [
            "raw-zone-default-expression"
        ]:
            raise ValueError("map 44 raw zone-default exception shape drift")
        if is_stub and use_sites:
            raise ValueError(f"direct-return stub unexpectedly owns table records: {symbol}")
        if not is_stub and (not kinds or kinds[-1] != "default"):
            raise ValueError(f"{category} table lacks a final default record: {symbol}")

        source_order_start = len(source_records)
        for index, site in enumerate(use_sites):
            record_address = address + index * config["recordBytes"]
            if record_address in source_records:
                raise ValueError(
                    f"overlapping source-owned map event record at 0x{record_address:X}"
                )
            source_records[record_address] = {
                **site,
                "recordSourceOrder": len(source_records),
                "tableRecordIndex": index,
                "recordAddress": record_address,
            }
        files.append(
            {
                "sourceOrder": len(files),
                "path": relative_path,
                "symbol": symbol,
                "address": address,
                "recordCount": len(kinds),
                "encodedRecordBytes": len(kinds) * config["recordBytes"],
                "recordSpanStartAddress": address if kinds else None,
                "recordSpanEndAddressExclusive": (
                    address + len(kinds) * config["recordBytes"] if kinds else None
                ),
                "recordSourceOrderStart": source_order_start if kinds else None,
                "recordSourceOrderEndInclusive": (len(source_records) - 1 if kinds else None),
                "specificRecordCount": kinds.count("specific"),
                "defaultRecordCount": kinds.count("default"),
                "macroCounts": dict(sorted(macro_counts.items())),
                "directReturnStub": is_stub,
                "rawDefaultException": is_raw_default,
            }
        )
    return files, source_records


def _join_source_rom_record(
    category: str,
    decoded: dict[str, Any],
    source_record: dict[str, Any],
    addresses: dict[str, int],
) -> dict[str, Any]:
    """Guard the source operand/ROM-relative-target relationship for one record."""
    if source_record["kind"] != decoded["kind"]:
        raise ValueError(f"{category} source/ROM record kind drift at 0x{decoded['address']:X}")
    if source_record["recordAddress"] != decoded["address"]:
        raise ValueError(f"{category} source/ROM record address drift at 0x{decoded['address']:X}")
    source_marker = source_record["sourceDefaultMarker"]
    if decoded["kind"] == "default":
        if source_marker is None:
            raise ValueError(f"{category} source default marker is missing")
        decoded_marker = decoded["entity"] if category == "entityEvents" else decoded["x"]
        if source_marker != decoded_marker:
            raise ValueError(
                f"{category} source/ROM default marker relationship drift at "
                f"0x{decoded['address']:X}"
            )
    elif source_marker is not None:
        raise ValueError(f"{category} specific source unexpectedly declares default marker")
    source_marker_word = source_record["sourceMarkerWord"]
    if source_marker_word is not None and (
        category != "zoneEvents" or source_marker_word != ((decoded["x"] << 8) | decoded["y"])
    ):
        raise ValueError(f"{category} raw source marker word/ROM relationship drift")
    target_base = source_record["targetBaseSymbol"]
    if target_base not in addresses:
        raise ValueError(f"{category} source target lacks H1 base label: {target_base}")
    source_target_address = addresses[target_base] + source_record["targetBaseAdjustment"]
    if source_target_address != decoded["resolvedTargetAddress"]:
        raise ValueError(
            f"{category} source/ROM target relationship drift at 0x{decoded['address']:X}"
        )
    return {**decoded, **source_record, "category": category}


def _category_contract(
    disasm: Path,
    addresses: dict[str, int],
    rom: bytes,
    setup: dict[str, Any],
    category: str,
    definitions: list[dict[str, Any]],
) -> dict[str, Any]:
    config = CATEGORY_CONFIG[category]
    files, source_records = _source_rows(disasm, addresses, category, definitions)
    targets = [table["targets"][category] for table in setup["pointerTables"]]
    target_counts = Counter(target["symbol"] for target in targets)
    unique_targets = {target["symbol"]: target["address"] for target in targets}
    if set(unique_targets) != {row["symbol"] for row in files}:
        raise ValueError(f"map setup pointers do not own the complete {category} source boundary")

    source_by_symbol = {row["symbol"]: row for row in files}
    tables: list[dict[str, Any]] = []
    physical_records: dict[int, dict[str, Any]] = {}
    for symbol, address in sorted(unique_targets.items()):
        source_row = source_by_symbol[symbol]
        if source_row["directReturnStub"]:
            if rom[address : address + 2] != b"\x4e\x75":
                raise ValueError(f"{category} direct-return stub ROM drift: {symbol}")
            continue
        records: list[dict[str, Any]] = []
        cursor = address
        while True:
            raw = rom[cursor : cursor + config["recordBytes"]]
            if len(raw) != config["recordBytes"] or len(records) >= 48:
                raise ValueError(f"{category} table has no bounded default record: {symbol}")
            decoded = _decode_event_record(category, address, cursor, raw)
            source_record = source_records.get(cursor)
            if source_record is None:
                raise ValueError(f"{category} source/ROM record drift at 0x{cursor:X}")
            if cursor in physical_records:
                raise ValueError(f"{category} physical records overlap at 0x{cursor:X}")
            joined = _join_source_rom_record(category, decoded, source_record, addresses)
            physical_records[cursor] = joined
            records.append(joined)
            cursor += config["recordBytes"]
            if decoded["kind"] == "default":
                break
        if len(records) != source_row["recordCount"]:
            raise ValueError(f"{category} source/ROM table length drift: {symbol}")
        tables.append(
            {
                "symbol": symbol,
                "address": address,
                "sourcePath": source_row["path"],
                "directReturnStub": False,
                "recordCount": len(records),
                "encodedRecordBytes": len(records) * config["recordBytes"],
                "recordSpanStartAddress": records[0]["address"],
                "recordSpanEndAddressExclusive": cursor,
                "recordSourceOrderStart": records[0]["recordSourceOrder"],
                "recordSourceOrderEndInclusive": records[-1]["recordSourceOrder"],
                "records": records,
            }
        )
    if set(physical_records) != set(source_records):
        raise ValueError(f"{category} source records are not exactly covered by setup tables")

    physical_kinds = Counter(record["kind"] for record in physical_records.values())
    setup_kinds: Counter[str] = Counter()
    table_by_symbol = {row["symbol"]: row for row in tables}
    for target in targets:
        table = table_by_symbol.get(target["symbol"])
        if table is not None:
            setup_kinds.update(record["kind"] for record in table["records"])
    source_macro_counts: Counter[str] = Counter()
    for row in files:
        source_macro_counts.update(row["macroCounts"])
    summary = {
        "sourceFileCount": len(files),
        "setupPointerReferenceCount": len(targets),
        "uniqueTargetCount": len(unique_targets),
        "decodedTableCount": len(tables),
        "aliasedTargetCount": sum(count > 1 for count in target_counts.values()),
        "physicalRecordCount": len(physical_records),
        "specificPhysicalRecordCount": physical_kinds["specific"],
        "defaultPhysicalRecordCount": physical_kinds["default"],
        "setupRecordReferenceCount": sum(setup_kinds.values()),
        "specificSetupRecordReferenceCount": setup_kinds["specific"],
        "defaultSetupRecordReferenceCount": setup_kinds["default"],
        "directReturnStubCount": sum(row["directReturnStub"] for row in files),
        "directReturnStubReferenceCount": sum(
            target_counts[row["symbol"]] for row in files if row["directReturnStub"]
        ),
        "rawDefaultExceptionCount": sum(row["rawDefaultException"] for row in files),
        "maximumTableRecordCount": max(row["recordCount"] for row in tables),
    }
    return {
        "summary": summary,
        "sourceMacroCounts": dict(sorted(source_macro_counts.items())),
        "duplicatePointerTargets": [
            {"symbol": symbol, "setupReferenceCount": count}
            for symbol, count in sorted(target_counts.items())
            if count > 1
        ],
        "sourceFiles": files,
        "tables": tables,
    }


def _source_label_owners(
    disasm: Path, addresses: dict[str, int]
) -> dict[int, list[dict[str, Any]]]:
    """Index every source/H1 label once, retaining same-address aliases."""
    owners: dict[int, list[dict[str, Any]]] = {}
    for path in sorted(disasm.rglob("*.asm")):
        relative_path = path.relative_to(disasm).as_posix()
        for line_number, line in enumerate(read_upstream_text(path).splitlines(), start=1):
            match = re.match(r"^\s*([A-Za-z_][A-Za-z0-9_]*):", line)
            if match is None:
                continue
            symbol = match.group(1)
            address = addresses.get(symbol)
            if address is None:
                continue
            owners.setdefault(address, []).append(
                {
                    "symbol": symbol,
                    "sourcePath": relative_path,
                    "sourceLine": line_number,
                }
            )
    for address in owners:
        owners[address].sort(
            key=lambda owner: (owner["sourcePath"], owner["sourceLine"], owner["symbol"])
        )
    return owners


def _label_owners(
    disasm: Path,
    addresses: dict[str, int],
    wanted_addresses: set[int],
    source_label_owners: dict[int, list[dict[str, Any]]] | None = None,
) -> dict[int, list[dict[str, Any]]]:
    """Select target labels from the reusable complete source/H1 label index."""
    all_owners = source_label_owners or _source_label_owners(disasm, addresses)
    return {address: list(all_owners.get(address, [])) for address in wanted_addresses}


def _program_key(symbol: str, address: int) -> str:
    return f"{symbol}:{address}"


def _control_flow_count_field(kind: str) -> str:
    fields = {
        "conditional-branch": "conditionalBranchSiteCount",
        "unconditional-branch": "unconditionalBranchSiteCount",
        "direct-call": "directCallSiteCount",
        "direct-jump": "directJumpSiteCount",
    }
    if kind not in fields:
        raise ValueError(f"map entity-event non-target control-flow kind: {kind}")
    return fields[kind]


def _target_identity(
    symbol: str, addresses: dict[str, int], owners: dict[int, list[dict[str, Any]]]
) -> dict[str, Any]:
    if symbol not in addresses:
        raise ValueError(f"map entity-event control-flow target lacks H1 address: {symbol}")
    address = addresses[symbol]
    labels = owners.get(address, [])
    if not labels:
        raise ValueError(f"map entity-event control-flow target lacks source owner: {symbol}")
    return {"symbol": symbol, "address": address, "addressLabels": labels}


def _entity_target_program_control_flow(
    programs: list[dict[str, Any]],
    alias_definitions: list[dict[str, Any]],
) -> tuple[dict[str, Any], dict[str, Any]]:
    """Build zero-inclusive instruction/effective target totals from parsed operations."""
    instruction_targets: dict[tuple[str, int], dict[str, Any]] = {}
    effective_targets: dict[tuple[str, int], dict[str, Any]] = {}
    counts: Counter[tuple[str, str, tuple[str, int], str]] = Counter()
    for program in programs:
        for operation in program["operations"]:
            target = operation["target"]
            if target is None:
                continue
            field = _control_flow_count_field(operation["controlFlowKind"])
            scope = target["effectiveTargetScope"]
            instruction_identity = (
                target["instructionTargetSymbol"],
                target["instructionTargetAddress"],
            )
            effective_identity = (
                target["effectiveTargetSymbol"],
                target["effectiveTargetAddress"],
            )
            instruction_targets.setdefault(
                instruction_identity,
                {
                    "symbol": instruction_identity[0],
                    "address": instruction_identity[1],
                    "addressLabels": target["instructionTargetAddressLabels"],
                },
            )
            effective_targets.setdefault(
                effective_identity,
                {
                    "symbol": effective_identity[0],
                    "address": effective_identity[1],
                    "addressLabels": target["effectiveTargetAddressLabels"],
                },
            )
            counts[("instruction", scope, instruction_identity, field)] += 1
            counts[("effective", scope, effective_identity, field)] += 1

    def target_rows(
        identity_kind: str,
        scope: str,
        declared_targets: dict[tuple[str, int], dict[str, Any]],
    ) -> list[dict[str, Any]]:
        rows: list[dict[str, Any]] = []
        for identity, target in declared_targets.items():
            row = dict(target)
            for field in _CONTROL_FLOW_COUNT_FIELDS:
                row[field] = counts[(identity_kind, scope, identity, field)]
            row["totalSiteCount"] = sum(row[field] for field in _CONTROL_FLOW_COUNT_FIELDS)
            rows.append(row)
        return rows

    totals = {
        "aliasDefinitions": alias_definitions,
        "targetTotals": {
            "instructionTargets": {
                "internal": target_rows("instruction", "internal", instruction_targets),
                "external": target_rows("instruction", "external", instruction_targets),
            },
            "effectiveTargets": {
                "internal": target_rows("effective", "internal", effective_targets),
                "external": target_rows("effective", "external", effective_targets),
            },
        },
    }
    def total_order(rows: list[dict[str, Any]]) -> list[str]:
        return [
            f"{row['symbol']}:{row['address']}:"
            f"{row['conditionalBranchSiteCount']}:"
            f"{row['unconditionalBranchSiteCount']}:"
            f"{row['directCallSiteCount']}:"
            f"{row['directJumpSiteCount']}"
            for row in rows
        ]

    orders = {
        "aliasOrder": [definition["aliasSymbol"] for definition in alias_definitions],
        "instructionTargetOrder": [
            _program_key(symbol, address) for symbol, address in instruction_targets
        ],
        "effectiveTargetOrder": [
            _program_key(symbol, address) for symbol, address in effective_targets
        ],
        "instructionInternalTargetTotalOrder": total_order(
            totals["targetTotals"]["instructionTargets"]["internal"]
        ),
        "instructionExternalTargetTotalOrder": total_order(
            totals["targetTotals"]["instructionTargets"]["external"]
        ),
        "effectiveInternalTargetTotalOrder": total_order(
            totals["targetTotals"]["effectiveTargets"]["internal"]
        ),
        "effectiveExternalTargetTotalOrder": total_order(
            totals["targetTotals"]["effectiveTargets"]["external"]
        ),
    }
    return totals, orders


def _reconcile_entity_target_programs(
    profiles: list[dict[str, Any]],
    programs: list[dict[str, Any]],
    summary: dict[str, Any],
    control_flow: dict[str, Any],
    target_orders: dict[str, Any],
) -> None:
    """Reconcile profile weights, source operations, and zero-inclusive target totals."""
    profile_by_identity = {
        (profile["canonicalSymbol"], profile["targetAddress"]): profile for profile in profiles
    }
    program_by_identity = {
        (program["canonicalSymbol"], program["entryAddress"]): program for program in programs
    }
    if set(program_by_identity) != set(profile_by_identity):
        raise ValueError("map entity-event program/profile identity coverage drift")
    if len(program_by_identity) != len(programs):
        raise ValueError("map entity-event duplicate program identity")

    totals: Counter[str] = Counter()
    observed_control: Counter[tuple[str, str, tuple[str, int], str]] = Counter()
    kind_summary_fields = {
        "conditional-branch": "conditionalBranchCount",
        "unconditional-branch": "unconditionalBranchCount",
        "direct-call": "directCallCount",
        "direct-jump": "directJumpCount",
        "return": "returnCount",
        "ordinary": "ordinaryOperationCount",
    }
    for identity, program in program_by_identity.items():
        profile = profile_by_identity[identity]
        expected_weights = {
            "physicalRecordCount": profile["physicalRecordCount"],
            "setupRecordReferenceCount": profile["setupRecordReferenceCount"],
            "routeRecordReferenceCount": profile["routeRecordReferenceCount"],
        }
        if program["referenceCounts"] != expected_weights:
            raise ValueError(f"map entity-event program reference-count drift: {identity}")
        if program["encodedSpanBytes"] != program["endAddressExclusive"] - program["entryAddress"]:
            raise ValueError(f"map entity-event program span relationship drift: {identity}")
        if program["termination"]["sourceOrder"] != program["operations"][-1]["sourceOrder"]:
            raise ValueError(f"map entity-event termination order drift: {identity}")
        if program["termination"]["controlFlowKind"] not in {"return", "direct-jump"}:
            raise ValueError(f"map entity-event termination kind drift: {identity}")
        totals["programCount"] += 1
        totals["labelCount"] += len(program["labels"])
        totals["operationCount"] += len(program["operations"])
        totals["encodedSpanBytes"] += program["encodedSpanBytes"]
        for field, value in expected_weights.items():
            totals[field] += value
        for operation in program["operations"]:
            kind = operation["controlFlowKind"]
            totals[kind_summary_fields[kind]] += 1
            target = operation["target"]
            if target is None:
                continue
            field = _control_flow_count_field(kind)
            scope = target["effectiveTargetScope"]
            instruction_identity = (
                target["instructionTargetSymbol"],
                target["instructionTargetAddress"],
            )
            effective_identity = (
                target["effectiveTargetSymbol"],
                target["effectiveTargetAddress"],
            )
            observed_control[("instruction", scope, instruction_identity, field)] += 1
            observed_control[("effective", scope, effective_identity, field)] += 1
            totals[f"{scope}ControlFlowSiteCount"] += 1
    totals["sourceFileCount"] = len({program["sourcePath"] for program in programs})
    totals["instructionTargetCount"] = len(target_orders["instructionTargetOrder"])
    totals["effectiveTargetCount"] = len(target_orders["effectiveTargetOrder"])
    totals["jumpInterfaceAliasCount"] = len(control_flow["aliasDefinitions"])
    if {field: totals[field] for field in summary} != summary:
        raise ValueError("map entity-event program summary reconciliation drift")

    target_totals = control_flow["targetTotals"]
    expected_orders = {
        "instructionTargets": target_orders["instructionTargetOrder"],
        "effectiveTargets": target_orders["effectiveTargetOrder"],
    }
    for identity_kind, identity_key in (
        ("instruction", "instructionTargets"),
        ("effective", "effectiveTargets"),
    ):
        for scope in ("internal", "external"):
            rows = target_totals[identity_key][scope]
            observed_order = [_program_key(row["symbol"], row["address"]) for row in rows]
            if observed_order != expected_orders[identity_key]:
                raise ValueError(
                    f"map entity-event {identity_kind} target zero-inclusive order drift: {scope}"
                )
            for row in rows:
                identity = (row["symbol"], row["address"])
                for field in _CONTROL_FLOW_COUNT_FIELDS:
                    if row[field] != observed_control[(identity_kind, scope, identity, field)]:
                        raise ValueError(
                            "map entity-event "
                            f"{identity_kind} target total drift: {scope}:{identity}"
                        )
                if row["totalSiteCount"] != sum(row[field] for field in _CONTROL_FLOW_COUNT_FIELDS):
                    raise ValueError(
                        "map entity-event "
                        f"{identity_kind} target aggregate drift: {scope}:{identity}"
                    )
            order_key = (
                f"{identity_kind}{scope.title()}TargetTotalOrder"
            )
            observed_total_order = [
                f"{row['symbol']}:{row['address']}:"
                f"{row['conditionalBranchSiteCount']}:"
                f"{row['unconditionalBranchSiteCount']}:"
                f"{row['directCallSiteCount']}:"
                f"{row['directJumpSiteCount']}"
                for row in rows
            ]
            if target_orders[order_key] != observed_total_order:
                raise ValueError(
                    f"map entity-event {identity_kind} target count-order drift: {scope}"
                )


def _entity_target_program_contract(
    disasm: Path,
    addresses: dict[str, int],
    listing_lines: list[str],
    listing_index: dict[str, dict[Any, Any]],
    record_target_profiles: list[dict[str, Any]],
    source_label_owners: dict[int, list[dict[str, Any]]],
) -> tuple[
    list[dict[str, Any]],
    dict[str, Any],
    dict[str, Any],
    dict[str, Any],
    list[dict[str, Any]],
    list[dict[str, Any]],
]:
    """Inventory entity target bodies, leaving zone/item target bodies unopened."""
    profiles = [
        profile for profile in record_target_profiles if profile["categories"] == ["entityEvents"]
    ]
    raw_blocks: list[tuple[dict[str, Any], dict[str, Any], int]] = []
    instruction_symbols: list[str] = []
    for profile in profiles:
        block = _source_program_block(disasm, profile, addresses)
        end_address = _bind_operations_to_h1(
            listing_lines,
            listing_index,
            profile=profile,
            block=block,
        )
        raw_blocks.append((profile, block, end_address))
        instruction_symbols.extend(
            operation["instructionTargetSymbol"]
            for operation in block["operations"]
            if operation["instructionTargetSymbol"] is not None
        )

    alias_symbols = list(
        dict.fromkeys(symbol for symbol in instruction_symbols if symbol.startswith("j_"))
    )
    initial_owner_addresses = {addresses[symbol] for symbol in instruction_symbols}
    initial_owners = _label_owners(
        disasm, addresses, initial_owner_addresses, source_label_owners
    )
    aliases_by_symbol = _parse_jump_interface_aliases(
        disasm,
        addresses,
        listing_lines,
        listing_index,
        initial_owners,
        alias_symbols,
    )
    if any(
        definition["directTargetSymbol"].startswith("j_")
        for definition in aliases_by_symbol.values()
    ):
        raise ValueError("map entity-event jump-interface alias chain drift")
    target_owner_addresses = initial_owner_addresses | {
        definition["directTargetAddress"] for definition in aliases_by_symbol.values()
    }
    target_owners = _label_owners(
        disasm, addresses, target_owner_addresses, source_label_owners
    )
    alias_definitions = []
    for alias in alias_symbols:
        definition = aliases_by_symbol[alias]
        direct_target = _target_identity(
            definition["directTargetSymbol"], addresses, target_owners
        )
        alias_definitions.append(
            {**definition, "directTargetAddressLabels": direct_target["addressLabels"]}
        )

    programs: list[dict[str, Any]] = []
    label_orders: list[dict[str, Any]] = []
    operation_orders: list[dict[str, Any]] = []
    for profile, block, end_address in raw_blocks:
        entry_address = profile["targetH1Address"]
        operations: list[dict[str, Any]] = []
        for raw_operation in block["operations"]:
            operation = dict(raw_operation)
            instruction_symbol = operation.pop("instructionTargetSymbol")
            if instruction_symbol is None:
                operation["target"] = None
            else:
                instruction_target = _target_identity(instruction_symbol, addresses, target_owners)
                alias = aliases_by_symbol.get(instruction_symbol)
                effective_symbol = (
                    alias["directTargetSymbol"] if alias is not None else instruction_symbol
                )
                effective_target = _target_identity(effective_symbol, addresses, target_owners)
                operation["target"] = {
                    "instructionTargetSymbol": instruction_target["symbol"],
                    "instructionTargetAddress": instruction_target["address"],
                    "instructionTargetAddressLabels": instruction_target["addressLabels"],
                    "effectiveTargetSymbol": effective_target["symbol"],
                    "effectiveTargetAddress": effective_target["address"],
                    "effectiveTargetAddressLabels": effective_target["addressLabels"],
                    "effectiveTargetScope": (
                        "internal"
                        if entry_address <= effective_target["address"] < end_address
                        else "external"
                    ),
                }
            operations.append(operation)
        if operations[-1]["controlFlowKind"] not in {"return", "direct-jump"}:
            raise ValueError(
                "map entity-event program lacks stable termination: "
                f"{profile['canonicalSymbol']}"
            )
        termination_operation = operations[-1]
        termination = {
            field: termination_operation[field]
            for field in (
                "sourceOrder",
                "sourceLine",
                "address",
                "sourceMnemonic",
                "mnemonic",
                "sizeSuffix",
                "operandTexts",
                "controlFlowKind",
                "target",
            )
        }
        program = {
            "programOrder": len(programs),
            "canonicalSymbol": profile["canonicalSymbol"],
            "entryAddress": entry_address,
            "sourcePath": profile["ownerSourcePath"],
            "entrySourceLine": profile["ownerSourceLine"],
            "endFunctionSymbol": block["endFunctionSymbol"],
            "endSourceLine": block["endSourceLine"],
            "endAddressExclusive": end_address,
            "encodedSpanBytes": end_address - entry_address,
            "referenceCounts": {
                "physicalRecordCount": profile["physicalRecordCount"],
                "setupRecordReferenceCount": profile["setupRecordReferenceCount"],
                "routeRecordReferenceCount": profile["routeRecordReferenceCount"],
            },
            "labels": block["labels"],
            "operations": operations,
            "termination": termination,
        }
        programs.append(program)
        key = _program_key(program["canonicalSymbol"], program["entryAddress"])
        label_orders.append(
            {
                "programKey": key,
                "labelOrder": [
                    f"{label['sourceOrder']}:{label['sourceLine']}:{label['symbol']}:{label['address']}"
                    for label in program["labels"]
                ],
            }
        )
        operation_orders.append(
            {
                "programKey": key,
                "operationOrder": [
                    f"{operation['sourceOrder']}:{operation['sourceLine']}:"
                    f"{operation['address']}"
                    for operation in program["operations"]
                ],
            }
        )
    control_flow, target_orders = _entity_target_program_control_flow(programs, alias_definitions)
    summary = {
        "programCount": len(programs),
        "sourceFileCount": len({program["sourcePath"] for program in programs}),
        "labelCount": sum(len(program["labels"]) for program in programs),
        "operationCount": sum(len(program["operations"]) for program in programs),
        "ordinaryOperationCount": sum(
            operation["controlFlowKind"] == "ordinary"
            for program in programs
            for operation in program["operations"]
        ),
        "conditionalBranchCount": sum(
            operation["controlFlowKind"] == "conditional-branch"
            for program in programs
            for operation in program["operations"]
        ),
        "unconditionalBranchCount": sum(
            operation["controlFlowKind"] == "unconditional-branch"
            for program in programs
            for operation in program["operations"]
        ),
        "directCallCount": sum(
            operation["controlFlowKind"] == "direct-call"
            for program in programs
            for operation in program["operations"]
        ),
        "directJumpCount": sum(
            operation["controlFlowKind"] == "direct-jump"
            for program in programs
            for operation in program["operations"]
        ),
        "returnCount": sum(
            operation["controlFlowKind"] == "return"
            for program in programs
            for operation in program["operations"]
        ),
        "encodedSpanBytes": sum(program["encodedSpanBytes"] for program in programs),
        "physicalRecordCount": sum(profile["physicalRecordCount"] for profile in profiles),
        "setupRecordReferenceCount": sum(
            profile["setupRecordReferenceCount"] for profile in profiles
        ),
        "routeRecordReferenceCount": sum(
            profile["routeRecordReferenceCount"] for profile in profiles
        ),
        "internalControlFlowSiteCount": sum(
            operation["target"] is not None
            and operation["target"]["effectiveTargetScope"] == "internal"
            for program in programs
            for operation in program["operations"]
        ),
        "externalControlFlowSiteCount": sum(
            operation["target"] is not None
            and operation["target"]["effectiveTargetScope"] == "external"
            for program in programs
            for operation in program["operations"]
        ),
        "instructionTargetCount": len(target_orders["instructionTargetOrder"]),
        "effectiveTargetCount": len(target_orders["effectiveTargetOrder"]),
        "jumpInterfaceAliasCount": len(alias_definitions),
    }
    _reconcile_entity_target_programs(profiles, programs, summary, control_flow, target_orders)
    return programs, summary, control_flow, target_orders, label_orders, operation_orders


def _ownership_class(record: dict[str, Any], owner_path: str) -> str:
    if record["macro"] == "raw-zone-default-expression":
        return "raw-expression-boundary"
    if owner_path == record["sourcePath"]:
        return "same-event-source"
    if owner_path.startswith("data/maps/entries/"):
        return "other-map-source"
    if owner_path.startswith("code/"):
        return "common-code"
    return "other-source"


def _record_target_ownership(
    record: dict[str, Any],
    addresses: dict[str, int],
    owners: dict[int, list[dict[str, Any]]],
) -> dict[str, Any]:
    """Resolve one decoded record to an exact owner, keeping raw map44 distinct."""
    target_address = record["resolvedTargetAddress"]
    labels = owners.get(target_address, [])
    base_address = addresses[record["targetBaseSymbol"]]
    if record["macro"] == "raw-zone-default-expression":
        base_labels = owners.get(base_address, [])
        if not base_labels:
            raise ValueError("map event target ownership unresolved raw expression base")
        owner = base_labels[0]
        canonical_symbol = "raw-map44-zone-default-expression-boundary"
        target_h1_address: int | None = None
    else:
        if not labels:
            raise ValueError("map event target ownership unresolved exact target")
        owner_paths = {label["sourcePath"] for label in labels}
        if len(owner_paths) != 1:
            raise ValueError("map event target ownership ambiguous exact target")
        owner = labels[0]
        canonical_symbol = owner["symbol"]
        target_h1_address = target_address
    return {
        "targetCanonicalSymbol": canonical_symbol,
        "targetAddressLabels": labels,
        "targetH1Address": target_h1_address,
        "targetBaseH1Address": base_address,
        "targetOwnerSourcePath": owner["sourcePath"],
        "targetOwnerSourceLine": owner["sourceLine"],
        "targetOwnershipClass": _ownership_class(record, owner["sourcePath"]),
    }


def _join_target_ownership(
    disasm: Path,
    addresses: dict[str, int],
    categories: dict[str, dict[str, Any]],
    source_label_owners: dict[int, list[dict[str, Any]]] | None = None,
) -> tuple[list[dict[str, Any]], list[dict[str, Any]], list[dict[str, Any]]]:
    """Join each source/ROM event record to an exact source/H1 target owner."""
    records = [
        record
        for category in categories.values()
        for table in category["tables"]
        for record in table["records"]
    ]
    target_addresses = {record["resolvedTargetAddress"] for record in records}
    target_addresses.update(addresses[record["targetBaseSymbol"]] for record in records)
    owners = _label_owners(disasm, addresses, target_addresses, source_label_owners)
    unresolved: list[dict[str, Any]] = []
    ambiguous: list[dict[str, Any]] = []
    for record in records:
        try:
            record.update(_record_target_ownership(record, addresses, owners))
        except ValueError as error:
            issue = {
                "recordAddress": record["address"],
                "targetExpression": record["targetExpression"],
            }
            if "ambiguous" in str(error):
                issue["resolvedTargetAddress"] = record["resolvedTargetAddress"]
                issue["targetAddressLabels"] = owners.get(record["resolvedTargetAddress"], [])
                ambiguous.append(issue)
            else:
                unresolved.append(issue)
    if unresolved or ambiguous:
        raise ValueError(
            "map event target ownership is incomplete: "
            f"unresolved={len(unresolved)}, ambiguous={len(ambiguous)}"
        )
    profiles_by_identity: dict[tuple[int, str], dict[str, Any]] = {}
    for record in records:
        identity = (record["resolvedTargetAddress"], record["targetCanonicalSymbol"])
        profile = profiles_by_identity.setdefault(
            identity,
            {
                "profileOrder": len(profiles_by_identity),
                "canonicalSymbol": record["targetCanonicalSymbol"],
                "targetAddress": record["resolvedTargetAddress"],
                "targetH1Address": record["targetH1Address"],
                "targetBaseH1Address": record["targetBaseH1Address"],
                "targetAddressLabels": record["targetAddressLabels"],
                "ownerSourcePath": record["targetOwnerSourcePath"],
                "ownerSourceLine": record["targetOwnerSourceLine"],
                "ownershipClass": record["targetOwnershipClass"],
                "physicalRecordCount": 0,
                "categories": [],
            },
        )
        profile["physicalRecordCount"] += 1
        if record["category"] not in profile["categories"]:
            profile["categories"].append(record["category"])
    profiles = list(profiles_by_identity.values())
    return profiles, unresolved, ambiguous


def _event_table_profiles(categories: dict[str, dict[str, Any]]) -> dict[str, dict[str, Any]]:
    """Index the complete declared category target surface, including RTS stubs."""
    profiles: dict[str, dict[str, Any]] = {}
    for category, value in categories.items():
        for source_file in value["sourceFiles"]:
            symbol = source_file["symbol"]
            if symbol in profiles:
                raise ValueError(f"map event table profile duplicates symbol: {symbol}")
            source_file["category"] = category
            profiles[symbol] = source_file
    return profiles


def _setup_category_joins(
    setup: dict[str, Any], categories: dict[str, dict[str, Any]]
) -> tuple[list[dict[str, Any]], list[dict[str, Any]]]:
    """Retain each setup-table category target without duplicating physical records."""
    profiles = _event_table_profiles(categories)
    pointer_tables = {table["symbol"]: table for table in setup["pointerTables"]}
    joins: list[dict[str, Any]] = []
    route_joins: list[dict[str, Any]] = []
    for pointer_order, pointer_table in enumerate(setup["pointerTables"]):
        for category in CATEGORY_CONFIG:
            target = pointer_table["targets"][category]
            profile = profiles.get(target["symbol"])
            if profile is None or profile["category"] != category:
                raise ValueError(
                    f"map event setup target lacks category profile: {target['symbol']}"
                )
            if target["address"] != profile["address"]:
                raise ValueError(f"map event setup target address drift: {target['symbol']}")
            joins.append(
                {
                    "sourceOrder": len(joins),
                    "pointerTableSourceOrder": pointer_order,
                    "pointerTableSymbol": pointer_table["symbol"],
                    "pointerTableAddress": pointer_table["address"],
                    "category": category,
                    "eventTableSymbol": target["symbol"],
                    "eventTableAddress": target["address"],
                    "directReturnStub": profile["directReturnStub"],
                    "physicalRecordCount": profile["recordCount"],
                }
            )
    route_selector_order = 0
    for route_order, route in enumerate(setup["routes"]):
        selectors = [("default", None, route["defaultPointer"])] + [
            ("flag", variant["flag"], variant["pointer"]) for variant in route["flagVariants"]
        ]
        for selector_order, (selector_kind, flag, pointer_symbol) in enumerate(selectors):
            pointer_table = pointer_tables.get(pointer_symbol)
            if pointer_table is None:
                raise ValueError(f"map event route lacks setup pointer table: {pointer_symbol}")
            for category in CATEGORY_CONFIG:
                target = pointer_table["targets"][category]
                profile = profiles.get(target["symbol"])
                if profile is None or profile["category"] != category:
                    raise ValueError(
                        f"map event route target lacks category profile: {target['symbol']}"
                    )
                if target["address"] != profile["address"]:
                    raise ValueError(f"map event route target address drift: {target['symbol']}")
                route_joins.append(
                    {
                        "sourceOrder": len(route_joins),
                        "routeSourceOrder": route_order,
                        "routeSelectorSourceOrder": route_selector_order,
                        "routeMap": route["map"],
                        "selectorSourceOrder": selector_order,
                        "selectorKind": selector_kind,
                        "flag": flag,
                        "pointerTableSymbol": pointer_symbol,
                        "pointerTableAddress": pointer_table["address"],
                        "category": category,
                        "eventTableSymbol": target["symbol"],
                        "eventTableAddress": target["address"],
                        "directReturnStub": profile["directReturnStub"],
                        "physicalRecordCount": profile["recordCount"],
                    }
                )
            route_selector_order += 1
    expected_pointer_category_joins = len(setup["pointerTables"]) * len(CATEGORY_CONFIG)
    expected_route_category_joins = setup["summary"]["routePointerReferenceCount"] * len(
        CATEGORY_CONFIG
    )
    if len(joins) != expected_pointer_category_joins:
        raise ValueError("map event setup category join cardinality drift")
    if len(route_joins) != expected_route_category_joins:
        raise ValueError("map event route category join cardinality drift")
    if route_selector_order != setup["summary"]["routePointerReferenceCount"]:
        raise ValueError("map event route selector source-order drift")
    return joins, route_joins


def _apply_reference_counts(
    setup: dict[str, Any],
    categories: dict[str, dict[str, Any]],
    setup_joins: list[dict[str, Any]],
    route_joins: list[dict[str, Any]],
    target_profiles: list[dict[str, Any]],
) -> None:
    """Derive table and target multiplicities from parsed joins, not record duplication."""
    table_profiles = _event_table_profiles(categories)
    tables_by_category = {
        category: {table["symbol"]: table for table in value["tables"]}
        for category, value in categories.items()
    }

    def join_records(join: dict[str, Any]) -> list[dict[str, Any]]:
        table = tables_by_category[join["category"]].get(join["eventTableSymbol"])
        if table is None:
            if join["directReturnStub"]:
                return []
            raise ValueError(
                "map event category join lacks a decoded non-stub table: "
                f"{join['eventTableSymbol']}"
            )
        if join["directReturnStub"] or table["address"] != join["eventTableAddress"]:
            raise ValueError(f"map event category join identity drift: {join['eventTableSymbol']}")
        return table["records"]

    profile_by_identity = {
        (profile["targetAddress"], profile["canonicalSymbol"]): profile
        for profile in target_profiles
    }
    setup_counts = Counter(join["eventTableSymbol"] for join in setup_joins)
    route_counts = Counter(join["eventTableSymbol"] for join in route_joins)
    route_category_counts = Counter(join["category"] for join in route_joins)
    for table in table_profiles.values():
        symbol = table["symbol"]
        table["setupReferenceCount"] = setup_counts[symbol]
        table["routeReferenceCount"] = route_counts[symbol]
    for profile in target_profiles:
        profile["setupRecordReferenceCount"] = 0
        profile["routeRecordReferenceCount"] = 0
    setup_record_counts: Counter[str] = Counter()
    for join in setup_joins:
        for event_record in join_records(join):
            identity = (
                event_record["resolvedTargetAddress"],
                event_record["targetCanonicalSymbol"],
            )
            if identity not in profile_by_identity:
                raise ValueError("map event setup record lacks a target profile")
            profile_by_identity[identity]["setupRecordReferenceCount"] += 1
            setup_record_counts[join["category"]] += 1
    route_record_counts: Counter[str] = Counter()
    for join in route_joins:
        for event_record in join_records(join):
            identity = (
                event_record["resolvedTargetAddress"],
                event_record["targetCanonicalSymbol"],
            )
            if identity not in profile_by_identity:
                raise ValueError("map event route record lacks a target profile")
            profile_by_identity[identity]["routeRecordReferenceCount"] += 1
            route_record_counts[join["category"]] += 1
    route_stub_counts = Counter(
        join["category"] for join in route_joins if join["directReturnStub"]
    )
    for category, value in categories.items():
        summary = value["summary"]
        summary["routeSelectorReferenceCount"] = setup["summary"]["routePointerReferenceCount"]
        summary["routeCategoryJoinCount"] = route_category_counts[category]
        summary["routeRecordReferenceCount"] = route_record_counts[category]
        summary["routeDirectReturnStubReferenceCount"] = route_stub_counts[category]


def _reconcile_event_reference_counts(
    categories: dict[str, dict[str, Any]],
    target_profiles: list[dict[str, Any]],
    setup_joins: list[dict[str, Any]],
    route_joins: list[dict[str, Any]],
    summary: dict[str, Any],
) -> None:
    """Cross-check every physical and weighted count from parsed records and joins."""
    tables_by_category = {
        category: {table["symbol"]: table for table in value["tables"]}
        for category, value in categories.items()
    }

    def join_records(join: dict[str, Any]) -> list[dict[str, Any]]:
        table = tables_by_category[join["category"]].get(join["eventTableSymbol"])
        if table is None:
            if join["directReturnStub"]:
                return []
            raise ValueError(
                "map event reconciliation lacks a decoded non-stub table: "
                f"{join['eventTableSymbol']}"
            )
        if join["directReturnStub"] or table["address"] != join["eventTableAddress"]:
            raise ValueError(
                f"map event reconciliation join identity drift: {join['eventTableSymbol']}"
            )
        return table["records"]

    physical_counts: Counter[str] = Counter()
    physical_kinds: dict[str, Counter[str]] = {category: Counter() for category in categories}
    profile_physical_counts: Counter[tuple[int, str]] = Counter()
    for category, value in categories.items():
        for table in value["tables"]:
            for record in table["records"]:
                identity = (record["resolvedTargetAddress"], record["targetCanonicalSymbol"])
                physical_counts[category] += 1
                physical_kinds[category][record["kind"]] += 1
                profile_physical_counts[identity] += 1

    setup_join_counts = Counter(join["category"] for join in setup_joins)
    route_join_counts = Counter(join["category"] for join in route_joins)
    setup_record_counts: Counter[str] = Counter()
    route_record_counts: Counter[str] = Counter()
    setup_kind_counts: dict[str, Counter[str]] = {category: Counter() for category in categories}
    route_kind_counts: dict[str, Counter[str]] = {category: Counter() for category in categories}
    profile_setup_counts: Counter[tuple[int, str]] = Counter()
    profile_route_counts: Counter[tuple[int, str]] = Counter()
    for join in setup_joins:
        for record in join_records(join):
            identity = (record["resolvedTargetAddress"], record["targetCanonicalSymbol"])
            setup_record_counts[join["category"]] += 1
            setup_kind_counts[join["category"]][record["kind"]] += 1
            profile_setup_counts[identity] += 1
    for join in route_joins:
        for record in join_records(join):
            identity = (record["resolvedTargetAddress"], record["targetCanonicalSymbol"])
            route_record_counts[join["category"]] += 1
            route_kind_counts[join["category"]][record["kind"]] += 1
            profile_route_counts[identity] += 1

    profiles_by_identity = {
        (profile["targetAddress"], profile["canonicalSymbol"]): profile
        for profile in target_profiles
    }
    if set(profiles_by_identity) != set(profile_physical_counts):
        raise ValueError("map event target profile physical identity coverage drift")
    for identity, profile in profiles_by_identity.items():
        expected = (
            profile_physical_counts[identity],
            profile_setup_counts[identity],
            profile_route_counts[identity],
        )
        observed = (
            profile["physicalRecordCount"],
            profile["setupRecordReferenceCount"],
            profile["routeRecordReferenceCount"],
        )
        if observed != expected:
            raise ValueError(f"map event target profile weighted-count drift: {identity}")

    profile_totals: Counter[str] = Counter()
    for profile in target_profiles:
        profile_totals["physical"] += profile["physicalRecordCount"]
        profile_totals["setup"] += profile["setupRecordReferenceCount"]
        profile_totals["route"] += profile["routeRecordReferenceCount"]
    parsed_totals = (
        sum(physical_counts.values()),
        sum(setup_record_counts.values()),
        sum(route_record_counts.values()),
    )
    if (
        profile_totals["physical"],
        profile_totals["setup"],
        profile_totals["route"],
    ) != parsed_totals:
        raise ValueError("map event target profile aggregate reconciliation drift")

    selector_orders = {join["routeSelectorSourceOrder"] for join in route_joins}
    route_stub_counts = Counter(
        join["category"] for join in route_joins if join["directReturnStub"]
    )
    for category, value in categories.items():
        category_summary = value["summary"]
        expected_summary = {
            "physicalRecordCount": physical_counts[category],
            "specificPhysicalRecordCount": physical_kinds[category]["specific"],
            "defaultPhysicalRecordCount": physical_kinds[category]["default"],
            "setupPointerReferenceCount": setup_join_counts[category],
            "setupRecordReferenceCount": setup_record_counts[category],
            "specificSetupRecordReferenceCount": setup_kind_counts[category]["specific"],
            "defaultSetupRecordReferenceCount": setup_kind_counts[category]["default"],
            "routeSelectorReferenceCount": len(selector_orders),
            "routeCategoryJoinCount": route_join_counts[category],
            "routeRecordReferenceCount": route_record_counts[category],
            "routeDirectReturnStubReferenceCount": route_stub_counts[category],
        }
        for field, expected in expected_summary.items():
            if category_summary[field] != expected:
                raise ValueError(f"map event category reconciliation drift: {category}.{field}")

    global_expected = {
        "physicalRecordCount": sum(physical_counts.values()),
        "setupPointerReferenceCount": len(setup_joins),
        "setupRecordReferenceCount": sum(setup_record_counts.values()),
        "routeSelectorReferenceCount": len(selector_orders),
        "routeCategoryJoinCount": len(route_joins),
        "routeRecordReferenceCount": sum(route_record_counts.values()),
        "recordTargetProfileCount": len(profiles_by_identity),
        "setupCategoryJoinCount": len(setup_joins),
    }
    for field, expected in global_expected.items():
        if summary[field] != expected:
            raise ValueError(f"map event global reconciliation drift: {field}")


def _consumer_facts(setup: dict[str, Any]) -> dict[str, Any]:
    dispatch = setup["sourceFacts"]["dispatch"]
    return {
        "defaultMarker": 0xFD,
        "relativeOffsetsResolveFromTableBase": True,
        "firstMatchingEntryWins": True,
        "entityEvents": dispatch["entityEvent"],
        "zoneEvents": dispatch["zoneEvent"],
        "itemEvents": {**dispatch["itemEvent"], "itemIndexMask": 0x7F},
    }


def _entity_event_reachability_facts(disasm: Path, addresses: dict[str, int]) -> dict[str, Any]:
    sources = {
        "ProcessPlayerAction": read_upstream_text(
            disasm / "code/gameflow/exploration/explorationvints.asm"
        ),
        "GetActivatedEntity": read_upstream_text(
            disasm / "code/gameflow/exploration/explorationfunctions_0.asm"
        ),
        "GetEntityEventIndex": read_upstream_text(
            disasm / "code/gameflow/battle/battlefunctions/battlefunctions_0.asm"
        ),
    }
    required = {
        "ProcessPlayerAction": (
            "bsr.w   GetActivatedEntity",
            "tst.w   d0",
            "bsr.w   GetEntityEventIndex",
            "jsr     j_RunMapSetupEntityEvent",
        ),
        "GetActivatedEntity": (
            "moveq   #$2F,d7",
            "bsr.w   IsFollowerEntity",
            "cmpi.w  #MAP_TILE_SIZE,d5",
            "moveq   #-1,d0",
        ),
        "GetEntityEventIndex": (
            "moveq   #BATTLE_ALL_ENTITIES_NUMBER,d7",
            "lea     ((ENTITY_INDEX_LIST-$1000000)).w,a0",
            "cmpi.w  #BATTLE_ALLY_ENTITIES_NUMBER,d0",
            "move.w  #$80,d0",
        ),
    }
    for symbol, fragments in required.items():
        if any(fragment not in sources[symbol] for fragment in fragments):
            raise ValueError(f"entity event reachability source-shape drift: {symbol}")
    return {
        "functionAddresses": {
            symbol: addresses[symbol] for symbol in REACHABILITY_FUNCTION_SYMBOLS
        },
        "activatedEntityScanSlots": 48,
        "followersAreSkipped": True,
        "adjacentDistanceIsStrictlyBelowMapTileSize": True,
        "entityIndexListSlotsScanned": 65,
        "enemyEventIndexBase": 128,
        "processActionCallsWrapperAfterNonnegativeActivation": True,
    }


def _clean_state_event_indices(records: list[dict[str, Any]]) -> list[int]:
    enemy_ordinal = 0
    event_indices: list[int] = []
    for record in records:
        if record["mapSprite"] >= 240:
            raise ValueError("direct-return reachability model does not cover special map sprites")
        if record["mapSprite"] < 30:
            event_indices.append(record["mapSprite"])
        else:
            event_indices.append(128 + enemy_ordinal)
            enemy_ordinal += 1
    return event_indices


def build_map_events_contract(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"map events H1 listing is missing: {listing_path}")
    listing_text = listing_path.read_text(encoding="utf-8")
    listing_lines = listing_text.splitlines()
    listing_index = _h1_program_index(listing_lines)
    addresses = listing_symbol_addresses(listing_text)
    rom = rom_path.read_bytes()
    setup = build_map_setup_contract(rom_path, upstream_path)
    entities = build_map_entities_contract(rom_path, upstream_path)
    if setup["upstream"]["commit"] != commit:
        raise ValueError("map events/setup provenance drift")

    macro_definitions = _event_macro_definitions(disasm)
    categories = {
        category: _category_contract(
            disasm, addresses, rom, setup, category, macro_definitions[category]
        )
        for category in CATEGORY_CONFIG
    }
    source_label_owners = _source_label_owners(disasm, addresses)
    record_target_profiles, unresolved_record_targets, ambiguous_record_targets = (
        _join_target_ownership(disasm, addresses, categories, source_label_owners)
    )
    setup_category_joins, route_category_joins = _setup_category_joins(setup, categories)
    _apply_reference_counts(
        setup,
        categories,
        setup_category_joins,
        route_category_joins,
        record_target_profiles,
    )
    (
        entity_target_programs,
        entity_target_program_summary,
        entity_target_program_control_flow,
        entity_target_program_control_flow_target_orders,
        entity_target_program_label_orders,
        entity_target_program_operation_orders,
    ) = _entity_target_program_contract(
        disasm,
        addresses,
        listing_lines,
        listing_index,
        record_target_profiles,
        source_label_owners,
    )
    entity_target_refs = [
        table["targets"]["entityEvents"]["symbol"] for table in setup["pointerTables"]
    ]
    entity_lists = {row["symbol"]: row for row in entities["lists"]}
    direct_return_stubs: list[dict[str, Any]] = []
    for symbol in CATEGORY_CONFIG["entityEvents"]["stubSymbols"]:
        owners = [
            table
            for table in setup["pointerTables"]
            if table["targets"]["entityEvents"]["symbol"] == symbol
        ]
        pairings: list[dict[str, Any]] = []
        for table in owners:
            entity_symbol = table["targets"]["entities"]["symbol"]
            entity_list = entity_lists[entity_symbol]
            event_indices = _clean_state_event_indices(entity_list["records"])
            pairings.append(
                {
                    "setupSymbol": table["symbol"],
                    "entityListSymbol": entity_symbol,
                    "entityRecordCount": entity_list["recordCount"],
                    "cleanStateEventIndices": event_indices,
                    "wrapperReachableWithAdjacentNonFollower": bool(event_indices),
                    "normalStoryRouteReachability": (
                        "unknown" if event_indices else "not-applicable-empty-list"
                    ),
                }
            )
        paired_record_counts = [row["entityRecordCount"] for row in pairings]
        direct_return_stubs.append(
            {
                "symbol": symbol,
                "address": addresses[symbol],
                "setupReferenceCount": entity_target_refs.count(symbol),
                "pairedEntityListRecordCounts": paired_record_counts,
                "nonEmptyPairedEntityListReferenceCount": sum(
                    record_count > 0 for record_count in paired_record_counts
                ),
                "setupPairings": pairings,
            }
        )
    raw_record = next(
        record
        for table in categories["zoneEvents"]["tables"]
        if table["symbol"] == RAW_ZONE_DEFAULT_SYMBOL
        for record in table["records"]
    )
    raw_zone_default = {
        "symbol": RAW_ZONE_DEFAULT_SYMBOL,
        "address": addresses[RAW_ZONE_DEFAULT_SYMBOL],
        "relativeOffset": raw_record["relativeOffset"],
        "resolvedTargetAddress": raw_record["resolvedTargetAddress"],
        "targetExpression": raw_record["targetExpression"],
        "targetBaseSymbol": raw_record["targetBaseSymbol"],
        "targetBaseH1Address": raw_record["targetBaseH1Address"],
        "targetBaseAdjustment": raw_record["targetBaseAdjustment"],
        "targetOwnerSourcePath": raw_record["targetOwnerSourcePath"],
        "targetOwnerSourceLine": raw_record["targetOwnerSourceLine"],
        "pointsInsideCutsceneEntityList": raw_record["resolvedTargetAddress"]
        == addresses["byte_54868"] + 4,
    }
    if not raw_zone_default["pointsInsideCutsceneEntityList"]:
        raise ValueError("map 44 raw zone-default target drift")

    category_summaries = {category: value["summary"] for category, value in categories.items()}
    summary = {
        "sourceFileCount": sum(row["sourceFileCount"] for row in category_summaries.values()),
        "setupPointerReferenceCount": sum(
            row["setupPointerReferenceCount"] for row in category_summaries.values()
        ),
        "uniqueTargetCount": sum(row["uniqueTargetCount"] for row in category_summaries.values()),
        "physicalRecordCount": sum(
            row["physicalRecordCount"] for row in category_summaries.values()
        ),
        "specificPhysicalRecordCount": sum(
            row["specificPhysicalRecordCount"] for row in category_summaries.values()
        ),
        "defaultPhysicalRecordCount": sum(
            row["defaultPhysicalRecordCount"] for row in category_summaries.values()
        ),
        "setupRecordReferenceCount": sum(
            row["setupRecordReferenceCount"] for row in category_summaries.values()
        ),
        "specificSetupRecordReferenceCount": sum(
            row["specificSetupRecordReferenceCount"] for row in category_summaries.values()
        ),
        "defaultSetupRecordReferenceCount": sum(
            row["defaultSetupRecordReferenceCount"] for row in category_summaries.values()
        ),
        "directReturnStubCount": sum(
            row["directReturnStubCount"] for row in category_summaries.values()
        ),
        "directReturnStubReferenceCount": sum(
            row["directReturnStubReferenceCount"] for row in category_summaries.values()
        ),
        "rawDefaultExceptionCount": sum(
            row["rawDefaultExceptionCount"] for row in category_summaries.values()
        ),
        "maximumTableRecordCount": max(
            row["maximumTableRecordCount"] for row in category_summaries.values()
        ),
        "selectionCaseCount": len(SELECTION_INPUTS),
        "recordTargetProfileCount": len(record_target_profiles),
        "setupCategoryJoinCount": len(setup_category_joins),
        "routeCategoryJoinCount": len(route_category_joins),
        "routeSelectorReferenceCount": setup["summary"]["routePointerReferenceCount"],
        "routeRecordReferenceCount": sum(
            row["routeRecordReferenceCount"] for row in category_summaries.values()
        ),
    }
    _reconcile_event_reference_counts(
        categories,
        record_target_profiles,
        setup_category_joins,
        route_category_joins,
        summary,
    )
    selection_cases = _selection_cases(setup, categories)
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "romSha256": setup["romSha256"],
        "scope": f"{SOURCE_ROOT.as_posix()}/*/mapsetups/s[235]_*.asm",
        "function": {symbol: addresses[symbol] for symbol in FUNCTION_SYMBOLS},
        "summary": summary,
        "categorySummaries": category_summaries,
        "sourceMacroCounts": {
            category: value["sourceMacroCounts"] for category, value in categories.items()
        },
        "eventMacroDefinitions": {
            "sourcePath": MAP_SETUP_MACROS_PATH.as_posix(),
            "categories": macro_definitions,
        },
        "consumerFacts": _consumer_facts(setup),
        "entityEventReachabilityFacts": _entity_event_reachability_facts(disasm, addresses),
        "entityTargetProgramSummary": entity_target_program_summary,
        "entityTargetPrograms": entity_target_programs,
        "entityTargetProgramOrder": [
            _program_key(program["canonicalSymbol"], program["entryAddress"])
            for program in entity_target_programs
        ],
        "entityTargetProgramLabelOrders": entity_target_program_label_orders,
        "entityTargetProgramOperationOrders": entity_target_program_operation_orders,
        "entityTargetProgramControlFlow": entity_target_program_control_flow,
        "entityTargetProgramControlFlowTargetOrders": (
            entity_target_program_control_flow_target_orders
        ),
        "directReturnStubs": direct_return_stubs,
        "rawZoneDefaultException": raw_zone_default,
        "unresolvedRecordTargets": unresolved_record_targets,
        "ambiguousRecordTargets": ambiguous_record_targets,
        "recordTargetProfiles": record_target_profiles,
        "setupCategoryJoins": setup_category_joins,
        "routeCategoryJoins": route_category_joins,
        "categorySourceFileOrders": {
            category: [
                f"{source_file['sourceOrder']}:{source_file['symbol']}:{source_file['address']}"
                for source_file in value["sourceFiles"]
            ]
            for category, value in categories.items()
        },
        "categoryDecodedTableOrders": {
            category: [f"{table['symbol']}:{table['address']}" for table in value["tables"]]
            for category, value in categories.items()
        },
        "physicalRecordOrder": [
            f"{category}:{record['recordSourceOrder']}:{record['address']}"
            for category, value in categories.items()
            for record in sorted(
                (record for table in value["tables"] for record in table["records"]),
                key=lambda record: record["recordSourceOrder"],
            )
        ],
        "recordTargetProfileOrder": [
            f"{profile['canonicalSymbol']}:{profile['targetAddress']}"
            for profile in record_target_profiles
        ],
        "setupCategoryJoinOrder": [
            f"{join['pointerTableSymbol']}:{join['category']}:{join['eventTableSymbol']}"
            for join in setup_category_joins
        ],
        "routeCategoryJoinOrder": [
            f"{join['routeSourceOrder']}:{join['selectorSourceOrder']}:"
            f"{join['category']}:{join['eventTableSymbol']}"
            for join in route_category_joins
        ],
        "selectionCases": selection_cases,
        "runtimeQuestions": [
            "entity-event-direct-return-stub-normal-story-route-reachability",
            "event-script-side-effects-and-transition-persistence",
            "event-portrait-facing-and-presentation-timing",
        ],
        "categories": categories,
    }


def _verify_complete_map_events_fixture(fixture: dict[str, Any], output: dict[str, Any]) -> None:
    """Reject a legal-shape fixture/output replacement that changes canonical evidence."""
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != output["romSha256"]
        or fixture["function"] != output["function"]
    ):
        raise ValueError("map events provenance/address drift")
    if fixture["expected"] != output:
        raise ValueError("map events complete semantic fixture drift")


def verify_map_events_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_map_events_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="map events static contract")
    _verify_complete_map_events_fixture(fixture, output)
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"] or output["summary"] != manifest["summary"]:
        raise ValueError("map events canonical output drift")
    destination = output_path or repo_path("local/derived/map-events-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "SourceFiles": output["summary"]["sourceFileCount"],
        "UniqueTables": output["summary"]["uniqueTargetCount"],
        "PhysicalRecords": output["summary"]["physicalRecordCount"],
        "SetupReferences": output["summary"]["setupRecordReferenceCount"],
        "SelectionCases": output["summary"]["selectionCaseCount"],
        "Status": "PASS",
    }
