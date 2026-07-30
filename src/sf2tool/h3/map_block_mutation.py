"""Static provenance and bounded BizHawk verification for map-script block mutation.

The parser guards the source/H1 contract; the session-only observer executes the
unmodified handlers and records the actual layout-copy events.  The Python copy
model is only an independently checked expected result, never runtime evidence.
"""

from __future__ import annotations

import re
from pathlib import Path
from typing import Any

from sf2tool.h2.map_script_engine import build_map_script_engine_contract
from sf2tool.h3.bizhawk import DERIVED_ROOT, run_observer, verify_runtime_contract
from sf2tool.h3.map_lifecycle import _with_instrumented_rom_database
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.rom import inspect_rom, mega_drive_checksum

H1_LISTING_PATH = Path("build/sf2build-h1.lst")
HANDLER_SOURCE_PATH = Path("code/common/scripting/map/mapscriptengine_1.asm")
HELPER_SOURCE_PATH = Path("code/gameflow/exploration/exploration.asm")
EQUATE_PATHS = (Path("sf2const.asm"), Path("sf2enums.asm"))
FIXTURE = repo_path("tests/fixtures/h3/map-block-mutation-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h3-map-block-mutation-fixture.schema.json")
OBSERVATION_SCHEMA = repo_path("schemas/h3-map-block-mutation-observation.schema.json")
OBSERVER = repo_path("tools/bizhawk/map_block_mutation_observer.lua")


def _statement(line: str) -> str:
    """Return one comment-free normalised ASM instruction or label."""
    return re.sub(r"\s+", " ", line.split(";", 1)[0].strip())


def _source_section(source: str, symbol: str) -> list[str]:
    """Parse one stable named source section without accepting outside text."""
    start = re.search(rf"^{re.escape(symbol)}:\s*$", source, re.MULTILINE)
    if start is None:
        raise ValueError(f"map block mutation source section is missing: {symbol}")
    end_marker = f"; End of function {symbol}"
    end = source.find(end_marker, start.end())
    if end < 0:
        raise ValueError(f"map block mutation source section end is missing: {symbol}")
    return [
        parsed
        for raw in source[start.end() : end].splitlines()
        if (parsed := _statement(raw)) and not parsed.endswith(":")
    ]


def _literal(text: str) -> int:
    if re.fullmatch(r"\$[0-9A-Fa-f]+", text):
        return int(text[1:], 16)
    if re.fullmatch(r"-?\d+", text):
        return int(text)
    raise ValueError(f"map block mutation equate literal is unsupported: {text}")


def _parse_equates(upstream_path: Path, names: set[str]) -> dict[str, int]:
    """Resolve the selected authoritative constants once, including aliases."""
    sources = [
        (upstream_path / "disasm" / relative_path).read_text(encoding="utf-8")
        for relative_path in EQUATE_PATHS
    ]
    declaration_pattern = re.compile(
        r"^(?P<name>[A-Za-z_][A-Za-z0-9_]*):\s+equ\s+"
        r"(?P<value>\$[0-9A-Fa-f]+|-?\d+|[A-Za-z_][A-Za-z0-9_]*)\b",
        re.MULTILINE,
    )
    raw_values: dict[str, str] = {}

    def declaration(name: str) -> str:
        if name in raw_values:
            return raw_values[name]
        matches = [
            match.group("value")
            for source in sources
            for match in declaration_pattern.finditer(source)
            if match.group("name") == name
        ]
        if not matches or len(set(matches)) != 1:
            raise ValueError(f"map block mutation equate cannot resolve: {name}")
        raw_values[name] = matches[0]
        return matches[0]

    resolved: dict[str, int] = {}

    def resolve(name: str, stack: tuple[str, ...] = ()) -> int:
        if name in resolved:
            return resolved[name]
        if name in stack:
            raise ValueError(f"map block mutation equate cannot resolve: {name}")
        text = declaration(name)
        value = (
            _literal(text)
            if re.fullmatch(r"\$[0-9A-Fa-f]+|-?\d+", text)
            else resolve(text, (*stack, name))
        )
        resolved[name] = value
        return value

    return {name: resolve(name) for name in sorted(names)}


def _h1_function_instructions(listing: str, symbol: str) -> list[tuple[int, str]]:
    """Parse only executable rows in one H1 function, stripping comments."""
    start = re.search(rf"^[0-9A-F]{{8}}\s+{re.escape(symbol)}:\s*$", listing, re.MULTILINE)
    if start is None:
        raise ValueError(f"map block mutation H1 function is missing: {symbol}")
    end = listing.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"map block mutation H1 function end is missing: {symbol}")
    rows: list[tuple[int, str]] = []
    for raw in listing[start.start() : end].splitlines():
        match = re.fullmatch(r"(?P<address>[0-9A-F]{8})\s+(?P<body>.*)", raw)
        if match is None:
            continue
        body = _statement(match.group("body"))
        body = re.sub(r"^(?:[0-9A-F]{2,8}\s+)+", "", body).strip()
        if not body or body.endswith(":"):
            continue
        if re.fullmatch(r"[A-Za-z][A-Za-z0-9_]*(?:\.[bwls])?(?:\s+.+)?", body) is None:
            raise ValueError(f"map block mutation H1 instruction parse drift: {raw}")
        rows.append((int(match.group("address"), 16), re.sub(r"\s+", " ", body)))
    return rows


def _h1_instruction_address(listing: str, symbol: str, instruction: str) -> int:
    matches = [
        address
        for address, actual in _h1_function_instructions(listing, symbol)
        if actual == instruction
    ]
    if len(matches) != 1:
        raise ValueError(
            "map block mutation H1 instruction identity drift: "
            f"{symbol}/{instruction}: {len(matches)}"
        )
    return matches[0]


def _h1_direct_call_addresses(listing: str, symbol: str, target: str) -> list[int]:
    """Count only direct JSR/BSR target instructions in one named H1 section."""
    pattern = re.compile(rf"(?:jsr|bsr)(?:\.[bwls])?\s+\(?{re.escape(target)}\)?(?:\.w)?$")
    return [
        address
        for address, instruction in _h1_function_instructions(listing, symbol)
        if pattern.fullmatch(instruction)
    ]


def _require_exact_source_order(source: str, symbol: str, expected: list[str]) -> list[str]:
    actual = _source_section(source, symbol)
    if actual != expected:
        raise ValueError(f"map block mutation source use-site/order drift: {symbol}")
    return actual


def _closed(value: object, required: set[str], *, name: str) -> list[dict[str, Any]]:
    if not isinstance(value, list) or not all(isinstance(row, dict) for row in value):
        raise ValueError(f"map block mutation H2 {name} container drift")
    rows = list(value)
    if any(set(row) != required for row in rows):
        raise ValueError(f"map block mutation H2 {name} record shape drift")
    return rows


def _copy_helper_contract(facts: dict[str, Any], helper_source: str) -> dict[str, Any]:
    helper = facts["copyMapBlocksHelperFacts"]
    required = {
        "helper",
        "address",
        "orderedInstructions",
        "inputByteShiftConstantUses",
        "addressRowShiftUses",
        "wordCopyByteStrideUses",
        "rowByteStrideUses",
        "copyInstruction",
        "innerLoop",
        "outerLoop",
        "derivedAddressStride",
    }
    if set(helper) != required:
        raise ValueError("map block mutation H2 CopyMapBlocks shape drift")
    ordered = helper["orderedInstructions"]
    if not isinstance(ordered, list) or not all(isinstance(row, str) for row in ordered):
        raise ValueError("map block mutation H2 CopyMapBlocks instructions drift")
    _require_exact_source_order(helper_source, helper["helper"], ordered)
    byte_shifts = _closed(
        helper["inputByteShiftConstantUses"],
        {"constant", "value", "instruction"},
        name="input byte shift",
    )
    row_shifts = _closed(helper["addressRowShiftUses"], {"value", "instruction"}, name="row shift")
    word_strides = _closed(
        helper["wordCopyByteStrideUses"],
        {"value", "instruction"},
        name="word stride",
    )
    row_strides = _closed(helper["rowByteStrideUses"], {"value", "instruction"}, name="row stride")
    if (
        not byte_shifts
        or any(row["constant"] != "BYTE_SHIFT_COUNT" for row in byte_shifts)
        or len({row["value"] for row in byte_shifts}) != 1
        or len({row["value"] for row in row_shifts}) != 1
        or len({row["value"] for row in word_strides}) != 1
        or len({row["value"] for row in row_strides}) != 1
    ):
        raise ValueError("map block mutation helper use-site value disagreement")
    byte_shift = byte_shifts[0]["value"]
    row_shift = row_shifts[0]["value"]
    word_stride = word_strides[0]["value"]
    row_stride = row_strides[0]["value"]
    if [row["instruction"] for row in byte_shifts] != [
        "lsr.w #BYTE_SHIFT_COUNT,d6",
        "lsr.w #BYTE_SHIFT_COUNT,d2",
        "lsr.w #BYTE_SHIFT_COUNT,d0",
    ]:
        raise ValueError("map block mutation helper packed-byte use-site order drift")
    if [row["instruction"] for row in row_shifts] != [
        "lsl.w #6,d3",
        "lsl.w #6,d1",
    ]:
        raise ValueError("map block mutation helper row-shift use-site order drift")
    if [row["instruction"] for row in word_strides] != [
        "addq.w #2,d0",
        "addq.w #2,d2",
    ]:
        raise ValueError("map block mutation helper word-stride use-site order drift")
    if [row["instruction"] for row in row_strides] != [
        "addi.w #128,d0",
        "addi.w #128,d2",
    ]:
        raise ValueError("map block mutation helper row-stride use-site order drift")
    if helper["derivedAddressStride"] != {
        "addressRowShiftBits": row_shift,
        "wordCopyByteStride": word_stride,
        "rowByteStride": row_stride,
    }:
        raise ValueError("map block mutation helper derived stride record drift")
    if row_stride != word_stride * (1 << row_shift):
        raise ValueError("map block mutation helper row/word stride relation drift")
    if helper["copyInstruction"] not in ordered:
        raise ValueError("map block mutation helper copy instruction drift")
    if helper["copyInstruction"] != "move.w (a2,d0.w),(a2,d2.w)":
        raise ValueError("map block mutation helper copy operand direction drift")
    for name, register in (("innerLoop", "d6"), ("outerLoop", "d7")):
        loop = helper[name]
        if (
            not isinstance(loop, dict)
            or set(loop)
            != {"counterRegister", "seedInstruction", "decrementInstruction", "loopInstruction"}
            or loop["counterRegister"] != register
            or any(loop[field] not in ordered for field in loop if field != "counterRegister")
        ):
            raise ValueError(f"map block mutation helper {name} drift")
    if helper["innerLoop"] != {
        "counterRegister": "d6",
        "seedInstruction": "move.w d1,d6",
        "decrementInstruction": "subq.w #1,d6",
        "loopInstruction": "dbf d6,loc_3DE2",
    }:
        raise ValueError("map block mutation helper inner-loop use-site order drift")
    if helper["outerLoop"] != {
        "counterRegister": "d7",
        "seedInstruction": "move.b d1,d7",
        "decrementInstruction": "subq.w #1,d7",
        "loopInstruction": "dbf d7,loc_3DDE",
    }:
        raise ValueError("map block mutation helper outer-loop use-site order drift")
    return {
        "helper": helper["helper"],
        "helperAddress": helper["address"],
        "packedInputByteShiftBits": byte_shift,
        "addressRowShiftBits": row_shift,
        "layoutWordColumnCount": 1 << row_shift,
        "wordCopyByteStride": word_stride,
        "rowByteStride": row_stride,
        "copyInstruction": helper["copyInstruction"],
        "innerLoop": helper["innerLoop"],
        "outerLoop": helper["outerLoop"],
    }


def _layout_storage_contract(equates: dict[str, int], exploration_source: str) -> dict[str, int]:
    """Derive the bounded working-layout span from ResetCurrentMap's use sites."""
    reset = _source_section(exploration_source, "ResetCurrentMap")
    required = [
        "lea (FF0000_RAM_START).l,a2",
        "move.w #MAP_LAYOUT_LONGS_COUNTER,d7",
        "clr.l (a2)+",
        "dbf d7,@Clear_Loop",
    ]
    try:
        indexes = [reset.index(instruction) for instruction in required]
    except ValueError as error:
        raise ValueError("map block mutation layout-span use-site drift") from error
    if indexes != sorted(indexes):
        raise ValueError("map block mutation layout-span use-site order drift")
    long_count = equates["MAP_LAYOUT_LONGS_COUNTER"] + 1
    stored_byte_count = long_count * 4
    return {
        "layoutClearLongCounter": equates["MAP_LAYOUT_LONGS_COUNTER"],
        "layoutStoredByteCount": stored_byte_count,
        "layoutStoredWordCount": stored_byte_count // 2,
    }


def build_map_block_mutation_contract(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    """Bind the H3 matrix seam to the existing H2 handler/helper evidence."""
    static = build_map_script_engine_contract(rom_path, upstream_path)
    facts = static["mapBlockMutationCommandFacts"]
    handler_source = (upstream_path / "disasm" / HANDLER_SOURCE_PATH).read_text(encoding="utf-8")
    helper_source = (upstream_path / "disasm" / HELPER_SOURCE_PATH).read_text(encoding="utf-8")
    equates = _parse_equates(
        upstream_path,
        {
            "BYTE_SHIFT_COUNT",
            "FF0000_RAM_START",
            "MAP_TILE_SIZE",
            "MAP_LAYOUT_LONGS_COUNTER",
            "VIEW_PLANE_UPDATE_TOGGLE_BITFIELD",
        },
    )
    helper = _copy_helper_contract(facts, helper_source)
    layout_storage = _layout_storage_contract(equates, helper_source)
    if helper["packedInputByteShiftBits"] != equates["BYTE_SHIFT_COUNT"]:
        raise ValueError("map block mutation BYTE_SHIFT_COUNT use-site drift")
    listing_text = (upstream_path / H1_LISTING_PATH).read_text(encoding="utf-8")
    listing_addresses = listing_symbol_addresses(listing_text)
    macros = {row["name"]: row for row in facts["macros"]}
    handlers: list[dict[str, Any]] = []
    for row in facts["handlers"]:
        guard = row["sectionGuard"]
        expected_guard = {
            "orderedInstructions",
            "cursorReadUseSites",
            "directCallOrder",
            "postCallBitSetUseSites",
            "returnInstruction",
        }
        if set(guard) != expected_guard or guard["orderedInstructions"] != row["guardedStatements"]:
            raise ValueError(f"map block mutation handler guard drift: {row['handler']}")
        _require_exact_source_order(handler_source, row["handler"], guard["orderedInstructions"])
        macro_name = row["macros"][0]
        macro = macros.get(macro_name)
        if macro is None or macro["handler"] != row["handler"]:
            raise ValueError(f"map block mutation macro/handler identity drift: {row['handler']}")
        cursor_reads = _closed(
            guard["cursorReadUseSites"],
            {"handlerRegister", "transferredByteCount", "instruction"},
            name=f"{row['handler']} cursor",
        )
        if [record["handlerRegister"] for record in cursor_reads] != ["d0", "d1", "d2"] or any(
            record["transferredByteCount"] != 2 for record in cursor_reads
        ):
            raise ValueError(
                f"map block mutation handler input word use-site drift: {row['handler']}"
            )
        if sum(record["transferredByteCount"] for record in cursor_reads) != macro["operandBytes"]:
            raise ValueError(
                f"map block mutation handler operand/cursor relation drift: {row['handler']}"
            )
        calls = _h1_direct_call_addresses(listing_text, row["handler"], "CopyMapBlocks")
        if len(calls) != 1 or row["directCalls"] != [
            {"opcode": "jsr", "instructionTarget": "CopyMapBlocks"}
        ]:
            raise ValueError(f"map block mutation H1 direct call drift: {row['handler']}")
        if listing_addresses.get(row["handler"]) != row["address"]:
            raise ValueError(f"map block mutation H1 handler address drift: {row['handler']}")
        fields = macro["sourceOperandFields"]
        if len(fields) != len(cursor_reads) * 2:
            raise ValueError(f"map block mutation packed field group drift: {row['handler']}")
        input_groups = [
            {
                "handlerRegister": cursor["handlerRegister"],
                "highByteSourceLabel": fields[index * 2]["sourceLabel"],
                "lowByteSourceLabel": fields[index * 2 + 1]["sourceLabel"],
            }
            for index, cursor in enumerate(cursor_reads)
        ]
        bit_sets = guard["postCallBitSetUseSites"]
        if not isinstance(bit_sets, list) or any(
            set(record) != {"bitIndex", "sourceTarget", "instruction"} for record in bit_sets
        ):
            raise ValueError(
                f"map block mutation post-call update use-site drift: {row['handler']}"
            )
        bit_set_addresses = [
            _h1_instruction_address(listing_text, row["handler"], record["instruction"])
            for record in bit_sets
        ]
        if macro_name == "setBlocks":
            if [record["bitIndex"] for record in bit_sets] != [0, 1] or any(
                record["sourceTarget"] != "VIEW_PLANE_UPDATE_TOGGLE_BITFIELD" for record in bit_sets
            ):
                raise ValueError("map block mutation csc34 update-bit use-site order drift")
        elif macro_name == "setBlocksVar":
            if bit_sets:
                raise ValueError("map block mutation csc35 unexpected update-bit use site")
        else:
            raise ValueError(f"map block mutation macro identity drift: {macro_name}")
        handlers.append(
            {
                "macro": macro_name,
                "opcode": row["opcode"],
                "handler": row["handler"],
                "handlerAddress": row["address"],
                "sourceCommandCount": row["sourceCommandCount"],
                "operandByteCount": macro["operandBytes"],
                "cursorInputWordCount": len(cursor_reads),
                "inputWordGroups": input_groups,
                "copyMapBlocksCallSiteAddress": calls[0],
                "postCallUpdateBitSetUseSites": [
                    {**record, "instructionAddress": address}
                    for record, address in zip(bit_sets, bit_set_addresses, strict=True)
                ],
            }
        )
    if [row["macro"] for row in handlers] != ["setBlocks", "setBlocksVar"]:
        raise ValueError("map block mutation handler source order drift")
    if listing_addresses.get(helper["helper"]) != helper["helperAddress"]:
        raise ValueError("map block mutation H1 helper address drift")
    helper["copyInstructionAddress"] = _h1_instruction_address(
        listing_text, helper["helper"], helper["copyInstruction"]
    )
    if layout_storage["layoutStoredWordCount"] % helper["layoutWordColumnCount"] != 0:
        raise ValueError("map block mutation layout row domain relation drift")
    return {
        "function": {
            "runMapSetupInitFunctionAddress": listing_addresses["RunMapSetupInitFunction"],
            "setBlocksHandlerAddress": handlers[0]["handlerAddress"],
            "setBlocksVarHandlerAddress": handlers[1]["handlerAddress"],
            "copyMapBlocksAddress": helper["helperAddress"],
            "copyInstructionAddress": helper["copyInstructionAddress"],
        },
        "ram": {
            "layoutBaseAddress": equates["FF0000_RAM_START"],
            "updateToggleBitfieldAddress": equates["VIEW_PLANE_UPDATE_TOGGLE_BITFIELD"],
        },
        "constants": {
            "byteShiftCount": helper["packedInputByteShiftBits"],
            "layoutRowShiftBits": helper["addressRowShiftBits"],
            "layoutWordColumnCount": helper["layoutWordColumnCount"],
            "wordCopyByteStride": helper["wordCopyByteStride"],
            "rowByteStride": helper["rowByteStride"],
            "mapTileSize": equates["MAP_TILE_SIZE"],
            **layout_storage,
            "layoutWordRowCount": layout_storage["layoutStoredWordCount"]
            // helper["layoutWordColumnCount"],
        },
        "sourceFacts": {
            "provenance": {
                "h2FixturePath": "tests/fixtures/h2/map-script-engine-static-v1.json",
                "h2FixtureId": "sf2-map-script-engine-static-v1",
                "h2FieldPath": "expected.mapBlockMutationCommandFacts",
            },
            "handlers": handlers,
            "copyHelper": helper,
            "callerBreakdown": facts["callerBreakdown"],
            "runtimeQuestions": facts["runtimeQuestions"],
        },
    }


def _coordinate_address(
    ram: dict[str, Any], constants: dict[str, Any], coordinate: dict[str, Any]
) -> int:
    """Resolve one source-shaped block coordinate to the guarded RAM layout."""
    if set(coordinate) != {"x", "y"}:
        raise ValueError("map block mutation coordinate shape drift")
    x = coordinate["x"]
    y = coordinate["y"]
    if not isinstance(x, int) or not isinstance(y, int):
        raise ValueError("map block mutation coordinate type drift")
    if not (0 <= x < constants["layoutWordColumnCount"]):
        raise ValueError("map block mutation x coordinate boundary drift")
    if not (0 <= y < constants["layoutWordRowCount"]):
        raise ValueError("map block mutation y coordinate boundary drift")
    return (
        ram["layoutBaseAddress"]
        + (y * constants["layoutWordColumnCount"] + x) * constants["wordCopyByteStride"]
    )


def _pack_case_input_words(case: dict[str, Any], byte_shift_count: int) -> list[int]:
    """Pack the three cursor words from the source-labelled macro fields."""
    highest_component = (1 << byte_shift_count) - 1
    input_groups = (
        (case["source"], "source"),
        (case["dimensions"], "dimensions"),
        (case["destination"], "destination"),
    )
    words: list[int] = []
    for component, name in input_groups:
        if set(component) != ({"x", "y"} if name != "dimensions" else {"width", "height"}):
            raise ValueError(f"map block mutation {name} input shape drift: {case['id']}")
        high = component["x"] if name != "dimensions" else component["width"]
        low = component["y"] if name != "dimensions" else component["height"]
        if (
            not isinstance(high, int)
            or not isinstance(low, int)
            or not (0 <= high <= highest_component and 0 <= low <= highest_component)
        ):
            raise ValueError(f"map block mutation {name} byte boundary drift: {case['id']}")
        words.append((high << byte_shift_count) | low)
    return words


def _copy_operation_byte_offsets(
    constants: dict[str, Any], case: dict[str, Any]
) -> list[dict[str, int]]:
    """Derive forward CopyMapBlocks word moves from its parsed offset use sites."""
    dimensions = case["dimensions"]
    width = dimensions["width"]
    height = dimensions["height"]
    if not isinstance(width, int) or not isinstance(height, int) or width < 1 or height < 1:
        raise ValueError(f"map block mutation rectangle boundary drift: {case['id']}")
    source = case["source"]
    destination = case["destination"]
    columns = constants["layoutWordColumnCount"]
    word_stride = constants["wordCopyByteStride"]
    operations: list[dict[str, int]] = []
    for row in range(height):
        for column in range(width):
            source_offset = ((source["y"] + row) * columns + source["x"] + column) * word_stride
            destination_offset = (
                (destination["y"] + row) * columns + destination["x"] + column
            ) * word_stride
            if (
                source_offset < 0
                or destination_offset < 0
                or source_offset + word_stride > constants["layoutStoredByteCount"]
                or destination_offset + word_stride > constants["layoutStoredByteCount"]
            ):
                raise ValueError(
                    f"map block mutation rectangle layout boundary drift: {case['id']}"
                )
            operations.append(
                {
                    "sourceByteOffset": source_offset,
                    "destinationByteOffset": destination_offset,
                }
            )
    return operations


def _independent_copy_model(static: dict[str, Any], case: dict[str, Any]) -> list[dict[str, Any]]:
    """Model forward RAM word copy from fixture inputs; runtime observation remains evidence."""
    ram = static["ram"]
    constants = static["constants"]
    words: dict[int, int] = {}
    for record in case["initialWords"]:
        address = _coordinate_address(ram, constants, record["coordinate"])
        if address in words:
            raise ValueError(f"map block mutation duplicate initial word: {case['id']}")
        value = record["value"]
        if not isinstance(value, int) or not 0 <= value <= 0xFFFF:
            raise ValueError(f"map block mutation word boundary drift: {case['id']}")
        words[address] = value
    for operation in _copy_operation_byte_offsets(constants, case):
        source_address = ram["layoutBaseAddress"] + operation["sourceByteOffset"]
        destination_address = ram["layoutBaseAddress"] + operation["destinationByteOffset"]
        if source_address not in words:
            raise ValueError(f"map block mutation missing model source word: {case['id']}")
        words[destination_address] = words[source_address]
    records = []
    for coordinate in case["readbackCoordinates"]:
        address = _coordinate_address(ram, constants, coordinate)
        if address not in words:
            raise ValueError(f"map block mutation missing readback word: {case['id']}")
        records.append({"coordinate": coordinate, "value": words[address]})
    return records


def _derive_case_expectations(
    static: dict[str, Any], fixture: dict[str, Any]
) -> list[dict[str, Any]]:
    """Bind each runtime row to the parsed handler/copy instruction use sites."""
    handlers = {row["macro"]: row for row in static["sourceFacts"]["handlers"]}
    expected_rows = []
    for case in fixture["cases"]:
        handler = handlers.get(case["macro"])
        if handler is None:
            raise ValueError(f"map block mutation unknown macro: {case['id']}")
        input_words = _pack_case_input_words(case, static["constants"]["byteShiftCount"])
        operations = _copy_operation_byte_offsets(static["constants"], case)
        source_start = _coordinate_address(static["ram"], static["constants"], case["source"])
        destination_start = _coordinate_address(
            static["ram"], static["constants"], case["destination"]
        )
        bit_indices = [record["bitIndex"] for record in handler["postCallUpdateBitSetUseSites"]]
        expected = {
            "id": case["id"],
            "macro": case["macro"],
            "handlerAddress": handler["handlerAddress"],
            "copyMapBlocksCallSiteAddress": handler["copyMapBlocksCallSiteAddress"],
            "copyInstructionAddress": static["function"]["copyInstructionAddress"],
            "inputWords": input_words,
            "sourceStartAddress": source_start,
            "destinationStartAddress": destination_start,
            "copyInstructionExecutionCount": len(operations),
            "copyOperationByteOffsets": operations,
            "wordCopyByteStride": static["constants"]["wordCopyByteStride"],
            "rowByteStride": static["constants"]["rowByteStride"],
            "updateBitIndices": bit_indices,
            "updateToggleByteSeed": case["updateToggleByteSeed"],
        }
        if case["expected"] != expected:
            raise ValueError(f"map block mutation fixture/static disagreement: {case['id']}")
        model_readback = _independent_copy_model(static, case)
        update_value = case["updateToggleByteSeed"]
        first_destination_value = model_readback[
            case["readbackCoordinates"].index(case["destination"])
        ]["value"]
        update_observations = []
        for bit_use_site in handler["postCallUpdateBitSetUseSites"]:
            bit_index = bit_use_site["bitIndex"]
            update_observations.append(
                {
                    "bitIndex": bit_index,
                    "instructionAddressObserved": bit_use_site["instructionAddress"],
                    "updateToggleByteBefore": update_value,
                    "firstDestinationWordBefore": first_destination_value,
                }
            )
            update_value |= 1 << bit_index
        runtime_model = {
            "handlerReturned": True,
            "directCallInputWordsObserved": input_words,
            "copyInstructionByteOffsetsObserved": operations,
            "postCopyUpdateBitObservations": update_observations,
            "updateToggleByteAfter": update_value,
            "readbackWordRecords": model_readback,
        }
        if case["runtimeGolden"] != runtime_model:
            raise ValueError(f"map block mutation fixture/model disagreement: {case['id']}")
        expected_rows.append({**expected, **runtime_model})
    return expected_rows


def _update_bit_use_sites(static: dict[str, Any]) -> list[dict[str, Any]]:
    """Expose parsed post-copy bit instruction identities in source order."""
    result = []
    for handler in static["sourceFacts"]["handlers"]:
        for use_site in handler["postCallUpdateBitSetUseSites"]:
            result.append({"macro": handler["macro"], **use_site})
    return result


def _case_inputs(
    fixture: dict[str, Any], derived_rows: list[dict[str, Any]], static: dict[str, Any]
) -> list[dict[str, Any]]:
    """Provide only invocation setup and bounded event identities to the observer."""
    update_sites_by_macro: dict[str, list[dict[str, Any]]] = {}
    for use_site in _update_bit_use_sites(static):
        update_sites_by_macro.setdefault(use_site["macro"], []).append(use_site)
    inputs = []
    for case, derived in zip(fixture["cases"], derived_rows, strict=True):
        inputs.append(
            {
                "id": case["id"],
                "macro": case["macro"],
                "handlerAddress": derived["handlerAddress"],
                "copyMapBlocksCallSiteAddress": derived["copyMapBlocksCallSiteAddress"],
                "copyInstructionExecutionCount": derived["copyInstructionExecutionCount"],
                "updateBitUseSites": update_sites_by_macro.get(case["macro"], []),
                "inputWords": derived["inputWords"],
                "destinationCoordinate": case["destination"],
                "updateToggleByteSeed": case["updateToggleByteSeed"],
                "initialWords": case["initialWords"],
                "readbackCoordinates": case["readbackCoordinates"],
            }
        )
    return inputs


def _expected_observation_records(derived_rows: list[dict[str, Any]]) -> list[dict[str, Any]]:
    """Join parsed static expectations and model values to the compact observed shape."""
    return [
        {
            "id": row["id"],
            "macro": row["macro"],
            "handlerAddressObserved": row["handlerAddress"],
            "copyMapBlocksCallSiteAddressObserved": row["copyMapBlocksCallSiteAddress"],
            "copyInstructionAddressObserved": row["copyInstructionAddress"],
            "handlerReturned": row["handlerReturned"],
            "directCallInputWordsObserved": row["directCallInputWordsObserved"],
            "copyInstructionByteOffsetsObserved": row["copyInstructionByteOffsetsObserved"],
            "postCopyUpdateBitObservations": row["postCopyUpdateBitObservations"],
            "updateToggleByteAfter": row["updateToggleByteAfter"],
            "readbackWordRecords": row["readbackWordRecords"],
        }
        for row in derived_rows
    ]


def _instrument_rom(rom_path: Path, fixture: dict[str, Any]) -> Path:
    """Build this slice's session-only trampoline without altering the input ROM."""
    original = rom_path.resolve(strict=True)
    original_hash = inspect_rom(original)["sha256"]
    data = bytearray(original.read_bytes())
    patch = fixture["instrumentation"]
    call_site = patch["callSiteAddress"]
    stub_address = patch["stubAddress"]
    original_call = bytes.fromhex(patch["callSiteOriginalHex"])
    patched_call = bytes.fromhex(patch["callSitePatchedHex"])
    original_stub = bytes.fromhex(patch["stubOriginalHex"])
    stub = bytes.fromhex(patch["stubHex"])
    if data[call_site : call_site + len(original_call)] != original_call:
        raise ValueError("map block mutation call-site bytes drifted")
    if data[stub_address : stub_address + len(original_stub)] != original_stub:
        raise ValueError("map block mutation trampoline padding bytes drifted")
    expected_call = b"\x4e\xb9" + stub_address.to_bytes(4, "big")
    if patched_call != expected_call:
        raise ValueError("map block mutation trampoline call shape drifted")
    if len(stub) > len(original_stub):
        raise ValueError("map block mutation trampoline exceeds verified padding")
    if patch["postHandlerAddress"] != stub_address + len(stub) - 2:
        raise ValueError("map block mutation trampoline return boundary drifted")
    data[call_site : call_site + len(patched_call)] = patched_call
    data[stub_address : stub_address + len(stub)] = stub
    data[0x18E:0x190] = int(mega_drive_checksum(bytes(data)), 16).to_bytes(2, "big")
    if inspect_rom(original)["sha256"] != original_hash:
        raise ValueError("map block mutation instrumentation altered the original ROM")
    DERIVED_ROOT.mkdir(parents=True, exist_ok=True)
    output = DERIVED_ROOT / "map-block-mutation.instrumented.bin"
    output.write_bytes(data)
    return output


def verify_map_block_mutation(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 180
) -> dict[str, Any]:
    """Run one bounded BizHawk matrix against unmodified block-copy handlers."""
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner="map block mutation runtime fixture")
    verify_runtime_contract(fixture, rom_path)
    static = build_map_block_mutation_contract(rom_path, upstream_path)
    for key in ("function", "ram", "constants", "sourceFacts"):
        if fixture[key] != static[key]:
            raise ValueError(f"map block mutation static contract drift: {key}")
    derived_rows = _derive_case_expectations(static, fixture)
    instrumented_rom = _instrument_rom(rom_path, fixture)

    def observe() -> dict[str, Any]:
        return run_observer(
            rom_path=instrumented_rom,
            observer_path=OBSERVER,
            config={
                "fixtureId": fixture["id"],
                "mapTestIndex": fixture["mapTestIndex"],
                "function": static["function"],
                "ram": static["ram"],
                "constants": static["constants"],
                "instrumentation": fixture["instrumentation"],
                "maxFrames": fixture["maxFrames"],
                "jsonModulePath": OBSERVER.with_name("json.lua").as_posix(),
                "harness": load_json(repo_path(fixture["sharedHarnessFixture"]))["harness"],
                "updateBitUseSites": _update_bit_use_sites(static),
                "caseInputs": _case_inputs(fixture, derived_rows, static),
            },
            output_name="map-block-mutation",
            timeout_seconds=timeout_seconds,
        )

    observed = _with_instrumented_rom_database(
        instrumented_rom, "SF2 H3 instrumented map block mutation", observe
    )
    validate_json(observed, OBSERVATION_SCHEMA, owner="map block mutation runtime observation")
    expected = {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["id"],
        "mapTest": fixture["mapTestIndex"],
        "recordOrder": [row["id"] for row in derived_rows],
        "records": _expected_observation_records(derived_rows),
    }
    if observed != expected:
        raise ValueError(
            "map block mutation runtime matrix mismatch\n"
            f"expected={expected!r}\nobserved={observed!r}"
        )
    return {
        "Fixture": fixture["id"],
        "Cases": len(derived_rows),
        "Handlers": len({row["macro"] for row in derived_rows}),
        "BizHawkLaunches": 1,
        "Instrumentation": "session-only",
        "Status": "PASS",
    }
