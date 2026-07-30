from __future__ import annotations

import re
from pathlib import Path
from typing import Any

from sf2tool.h2.map_content import (
    _encode_source,
    build_map_content_contract,
)
from sf2tool.h2.map_content import (
    _parse_equates as _map_content_equates,
)
from sf2tool.h2.map_import import _decode_copy_events
from sf2tool.h2.map_script_engine import build_map_script_engine_contract
from sf2tool.h3.bizhawk import run_observer, verify_runtime_contract
from sf2tool.h3.map_lifecycle import _instrument_rom, _with_instrumented_rom_database
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path
from sf2tool.research_index import listing_symbol_addresses

FIXTURE = repo_path("tests/fixtures/h3/map-interaction-trigger-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h3-map-interaction-trigger-fixture.schema.json")
OBSERVATION_SCHEMA = repo_path("schemas/h3-map-interaction-trigger-observation.schema.json")
OBSERVER = repo_path("tools/bizhawk/map_interaction_trigger_observer.lua")

CALLEE_SOURCE_PATH = Path("code/gameflow/exploration/exploration.asm")
HANDLER_SOURCE_PATH = Path("code/common/scripting/map/mapscriptengine_1.asm")
HASH_SOURCE_PATH = Path("code/common/tech/graphics/display.asm")
HASH_DATA_PATH = Path("data/maps/global/mapoffsethashtable.bin")


def _parse_equates(source: str, names: tuple[str, ...]) -> dict[str, int]:
    """Parse one authoritative constants map for all runtime-derived fields."""
    values: dict[str, int] = {}
    for name in names:
        match = re.search(
            rf"^{re.escape(name)}:\s+equ\s+(?P<value>\$[0-9A-Fa-f]+|\d+)\b",
            source,
            re.MULTILINE,
        )
        if match is None:
            raise ValueError(f"map interaction trigger source constant is missing: {name}")
        encoded = match.group("value")
        values[name] = int(encoded[1:], 16) if encoded.startswith("$") else int(encoded)
    return values


def _section(source: str, symbol: str) -> list[tuple[str, str, int]]:
    """Return comment-free instructions inside one named source function only."""
    start = re.search(rf"^{re.escape(symbol)}:\s*$", source, re.MULTILINE)
    if start is None:
        raise ValueError(f"map interaction trigger function is missing: {symbol}")
    end = source.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"map interaction trigger function end marker is missing: {symbol}")
    records: list[tuple[str, str, int]] = []
    first_line = source[: start.start()].count("\n")
    for line_offset, raw in enumerate(source[start.start() : end].splitlines(), 1):
        line = raw.split(";", 1)[0].strip()
        if not line or line.endswith(":"):
            continue
        match = re.fullmatch(
            r"(?P<opcode>[A-Za-z][A-Za-z0-9]*(?:\.[bwls])?)(?:\s+(?P<operand>.*))?",
            line,
        )
        if match is None:
            raise ValueError(f"map interaction trigger cannot parse {symbol} source line: {raw!r}")
        records.append(
            (
                match.group("opcode").lower(),
                re.sub(r"\s+", "", match.group("operand") or "").lower(),
                first_line + line_offset,
            )
        )
    return records


def _require_order(
    section: list[tuple[str, str, int]],
    required: tuple[tuple[str, str], ...],
    *,
    name: str,
) -> list[dict[str, Any]]:
    """Guard operand identity, instruction width, branch polarity, and order."""
    cursor = 0
    records: list[dict[str, Any]] = []
    for opcode, operand in required:
        while cursor < len(section) and section[cursor][:2] != (opcode, operand):
            cursor += 1
        if cursor == len(section):
            raise ValueError(
                f"map interaction trigger semantic drift in {name}: "
                f"expected {opcode} {operand} in order"
            )
        observed_opcode, observed_operand, source_line = section[cursor]
        records.append(
            {
                "opcode": observed_opcode,
                "operand": observed_operand,
                "sourceLine": source_line,
            }
        )
        cursor += 1
    return records


def _single_instruction(
    section: list[tuple[str, str, int]],
    opcode: str,
    operand: str,
    *,
    name: str,
) -> dict[str, Any]:
    matches = [
        {"opcode": value[0], "operand": value[1], "sourceLine": value[2]}
        for value in section
        if value[:2] == (opcode, operand)
    ]
    if len(matches) != 1:
        raise ValueError(
            f"map interaction trigger use-site drift in {name}: "
            f"expected one {opcode} {operand}, found {len(matches)}"
        )
    return matches[0]


def _immediate_value(operand: str) -> int:
    """Decode an immediate operand retained by an owning guarded instruction."""
    immediate = operand.split(",", 1)[0]
    if not immediate.startswith("#"):
        raise ValueError(f"map interaction trigger expected immediate operand: {operand}")
    encoded = immediate[1:]
    return int(encoded[1:], 16) if encoded.startswith("$") else int(encoded)


def _instruction_width_byte_count(opcode: str) -> int:
    """Resolve a 68000 explicit instruction-size suffix to its source width."""
    match = re.fullmatch(r"[a-z0-9]+\.([bwl])", opcode)
    if match is None:
        raise ValueError(f"map interaction trigger instruction width is not explicit: {opcode}")
    return {"b": 1, "w": 2, "l": 4}[match.group(1)]


def _shared_immediate(
    rows: list[dict[str, Any]], *, name: str, expected_count: int
) -> tuple[int, list[dict[str, Any]]]:
    if len(rows) != expected_count:
        raise ValueError(
            f"map interaction trigger {name} use-site count drift: "
            f"expected {expected_count}, found {len(rows)}"
        )
    values = {_immediate_value(row["operand"]) for row in rows}
    if len(values) != 1:
        raise ValueError(f"map interaction trigger {name} immediate disagreement")
    return values.pop(), rows


def _derived_callee_constants(use_sites: dict[str, dict[str, Any]]) -> dict[str, Any]:
    """Derive reported widths, shifts, masks, and strides from named guarded use sites."""
    step = use_sites["step"]
    roof = use_sites["roof"]
    coordinate_shift, coordinate_shift_sites = _shared_immediate(
        [
            row
            for selector in (step["selector"], roof["selector"])
            for row in selector
            if row["opcode"] == "lsr.w" and row["operand"].split(",", 1)[1] in {"d0", "d1"}
        ],
        name="coordinate shift",
        expected_count=4,
    )
    coordinate_mask, coordinate_mask_sites = _shared_immediate(
        [
            row
            for selector in (step["selector"], roof["selector"])
            for row in selector
            if row["opcode"] == "andi.w" and row["operand"].rsplit(",", 1)[1] in {"d0", "d1"}
        ],
        name="coordinate hash mask",
        expected_count=4,
    )
    record_stride, record_stride_sites = _shared_immediate(
        [
            row
            for match in (step["match"], roof["match"])
            for row in match
            if row["opcode"] == "addq.l" and row["operand"].endswith(",a2")
        ],
        name="event record stride",
        expected_count=2,
    )
    layout_row_shift, layout_row_shift_sites = _shared_immediate(
        [
            row
            for mutation in (step["mutation"], roof["mutation"])
            for row in mutation
            if row["opcode"] == "lsl.w" and row["operand"].rsplit(",", 1)[1] in {"d1", "d3"}
        ],
        name="layout row shift",
        expected_count=4,
    )
    terminator_sites = [
        row
        for match in (step["match"], roof["match"])
        for row in match
        if row["opcode"].startswith("tst.") and row["operand"] == "(a2)+"
    ]
    if len(terminator_sites) != 2:
        raise ValueError("map interaction trigger terminator-width use-site count drift")
    terminator_widths = {_instruction_width_byte_count(row["opcode"]) for row in terminator_sites}
    if len(terminator_widths) != 1:
        raise ValueError("map interaction trigger terminator-width use-site disagreement")
    return {
        "values": {
            "coordinateInputShiftBits": coordinate_shift,
            "coordinateHashMask": coordinate_mask,
            "recordStrideByteCount": record_stride,
            "recordTerminatorByteCount": terminator_widths.pop(),
            "layoutRowShiftBits": layout_row_shift,
        },
        "useSites": {
            "coordinateInputShiftBits": coordinate_shift_sites,
            "coordinateHashMask": coordinate_mask_sites,
            "recordStrideByteCount": record_stride_sites,
            "recordTerminatorByteCount": terminator_sites,
            "layoutRowShiftBits": layout_row_shift_sites,
        },
    }


def _handler_contract(static: dict[str, Any]) -> dict[str, dict[str, Any]]:
    facts = static["mapInteractionTriggerCommandFacts"]
    handlers = {row["macro"]: row for row in facts["handlers"]}
    expected = {
        "roofEvent": ("csc43_RoofEvent", "PerformMapBlockCopyScript"),
        "stepEvent": ("csc47_StepEvent", "OpenDoor"),
    }
    if set(handlers) != set(expected):
        raise ValueError("map interaction trigger H2 handler set drift")
    for macro, (symbol, target) in expected.items():
        handler = handlers[macro]
        if handler["handler"] != symbol:
            raise ValueError(f"map interaction trigger H2 handler identity drift: {macro}")
        guard = handler["sectionGuard"]
        expected_order = [
            "move.w (a6)+,d0",
            "move.w (a6)+,d1",
            "mulu.w #MAP_TILE_SIZE,d0",
            "mulu.w #MAP_TILE_SIZE,d1",
            f"jsr ({target}).w",
            "rts",
        ]
        if guard["orderedInstructions"] != expected_order:
            raise ValueError(f"map interaction trigger H2 handler order drift: {macro}")
        if [row["instructionTarget"] for row in handler["directCalls"]] != [target]:
            raise ValueError(f"map interaction trigger H2 direct target drift: {macro}")
    return handlers


def _callee_use_sites(callee_source: str) -> dict[str, dict[str, Any]]:
    """Guard only the reported guards, selectors, match boundary, and mutations."""
    step = _section(callee_source, "OpenDoor")
    roof = _section(callee_source, "PerformMapBlockCopyScript")
    return {
        "step": {
            "battleGate": _require_order(
                step,
                (
                    ("cmpi.b", "#not_currently_in_battle,((current_battle-$1000000)).w"),
                    ("bne.w", "@return"),
                ),
                name="OpenDoor battle gate",
            ),
            "selector": _require_order(
                step,
                (
                    ("lsr.w", "#7,d0"),
                    ("lsr.w", "#7,d1"),
                    ("lea", "table_mapoffsethash(pc),a2"),
                    ("add.w", "d0,d0"),
                    ("move.b", "(a2,d0.w),d0"),
                    ("andi.w", "#$3f,d0"),
                    ("add.w", "d1,d1"),
                    ("move.b", "(a2,d1.w),d1"),
                    ("andi.w", "#$3f,d1"),
                    ("move.b", "((current_map-$1000000)).w,d7"),
                    ("lsl.w", "#index_shift_count,d7"),
                    ("movea.l", "mapdata_offset_event_step(a2),a2"),
                ),
                name="OpenDoor coordinate and step-table selector",
            ),
            "match": _require_order(
                step,
                (
                    ("tst.b", "(a2)"),
                    ("bmi.w", "@done"),
                    ("cmp.b", "(a2),d0"),
                    ("bne.w", "@nextevent"),
                    ("cmp.b", "1(a2),d1"),
                    ("bne.w", "@nextevent"),
                    ("tst.w", "(a2)+"),
                    ("addq.l", "#8,a2"),
                    ("addq.w", "#1,d2"),
                    ("bra.w", "@main_loop"),
                ),
                name="OpenDoor terminator, match, and record stride",
            ),
            "mutation": _require_order(
                step,
                (
                    ("lsl.w", "#6,d3"),
                    ("add.w", "d3,d2"),
                    ("add.w", "d2,d2"),
                    ("lsl.w", "#6,d1"),
                    ("add.w", "d1,d0"),
                    ("add.w", "d0,d0"),
                    ("tst.w", "d1"),
                    ("blt.s", "loc_3eec"),
                    ("move.w", "(a2,d0.w),(a2,d2.w)"),
                    ("clr.w", "(a2,d2.w)"),
                ),
                name="OpenDoor copy or clear mutation",
            ),
            "updateToggle": _require_order(
                step,
                (
                    ("tst.b", "((map_area_layer_type-$1000000)).w"),
                    ("beq.s", "@updateplaneb"),
                    (
                        "bset",
                        "#0,((view_plane_update_toggle_bitfield-$1000000)).w",
                    ),
                    ("bra.s", "@done"),
                    (
                        "bset",
                        "#1,((view_plane_update_toggle_bitfield-$1000000)).w",
                    ),
                ),
                name="OpenDoor update-toggle branch",
            ),
        },
        "roof": {
            "busyGate": _require_order(
                roof,
                (
                    ("tst.w", "((word_ffaf42-$1000000)).w"),
                    ("bne.w", "loc_40e6"),
                ),
                name="PerformMapBlockCopyScript busy gate",
            ),
            "selector": _require_order(
                roof,
                (
                    ("lsr.w", "#7,d0"),
                    ("lsr.w", "#7,d1"),
                    ("lea", "table_mapoffsethash(pc),a3"),
                    ("add.w", "d0,d0"),
                    ("move.b", "(a3,d0.w),d0"),
                    ("andi.w", "#$3f,d0"),
                    ("add.w", "d1,d1"),
                    ("move.b", "(a3,d1.w),d1"),
                    ("andi.w", "#$3f,d1"),
                    ("move.b", "((current_map-$1000000)).w,d7"),
                    ("lsl.w", "#index_shift_count,d7"),
                    ("movea.l", "mapdata_offset_event_roof(a2),a2"),
                ),
                name="PerformMapBlockCopyScript coordinate and roof-table selector",
            ),
            "match": _require_order(
                roof,
                (
                    ("tst.b", "(a2)"),
                    ("bmi.w", "loc_40e6"),
                    ("cmp.b", "(a2),d0"),
                    ("bne.w", "loc_40ea"),
                    ("cmp.b", "1(a2),d1"),
                    ("bne.w", "loc_40ea"),
                    ("move.w", "d2,((word_ffaf42-$1000000)).w"),
                    ("tst.w", "(a2)+"),
                    ("addq.l", "#8,a2"),
                    ("addq.w", "#1,d2"),
                    ("bra.w", "loc_4028"),
                ),
                name="PerformMapBlockCopyScript terminator, match, and record stride",
            ),
            "mutation": _require_order(
                roof,
                (
                    ("lsl.w", "#6,d3"),
                    ("add.w", "d3,d2"),
                    ("add.w", "d2,d2"),
                    ("lsl.w", "#6,d1"),
                    ("add.w", "d1,d0"),
                    ("add.w", "d0,d0"),
                    ("tst.w", "d1"),
                    ("blt.s", "loc_40ba"),
                    ("move.w", "(a2,d0.w),(a2,d2.w)"),
                    ("clr.w", "(a2,d2.w)"),
                    (
                        "bset",
                        "#0,((view_plane_update_toggle_bitfield-$1000000)).w",
                    ),
                ),
                name="PerformMapBlockCopyScript copy or clear mutation and toggle",
            ),
        },
    }


def _direct_call_site(listing: str, handler: str, target: str) -> int:
    start = re.search(rf"^[0-9A-F]{{8}}\s+{re.escape(handler)}:\s*$", listing, re.MULTILINE)
    if start is None:
        raise ValueError(f"map interaction trigger H1 handler section is missing: {handler}")
    end = listing.find(f"; End of function {handler}", start.end())
    if end < 0:
        raise ValueError(f"map interaction trigger H1 handler section end is missing: {handler}")
    pattern = re.compile(
        r"^(?P<address>[0-9A-F]{8})\s+.*?\bjsr\s+"
        rf"\(?{re.escape(target)}\)?(?:\.w)?\s*$",
        re.IGNORECASE,
    )
    matches = []
    for raw in listing[start.start() : end].splitlines():
        instruction = raw.split(";", 1)[0].rstrip()
        match = pattern.fullmatch(instruction)
        if match is not None:
            matches.append(match.group("address"))
    if len(matches) != 1:
        raise ValueError(
            f"map interaction trigger H1 direct call-site drift for {handler}: {len(matches)}"
        )
    return int(matches[0], 16)


def _listing_instruction_address(listing: str, symbol: str, pattern: str) -> int:
    """Resolve one instruction callback inside the exact owning H1 function."""
    start = re.search(rf"^[0-9A-F]{{8}}\s+{re.escape(symbol)}:\s*$", listing, re.MULTILINE)
    if start is None:
        raise ValueError(f"map interaction trigger H1 function section is missing: {symbol}")
    end = listing.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"map interaction trigger H1 function end is missing: {symbol}")
    matches = re.findall(
        rf"^(?P<address>[0-9A-F]{{8}})\s+.*?{pattern}\s*$",
        listing[start.start() : end],
        re.MULTILINE | re.IGNORECASE,
    )
    if len(matches) != 1:
        raise ValueError(
            f"map interaction trigger H1 instruction callback drift in {symbol}: {len(matches)}"
        )
    return int(matches[0], 16)


def _event_table(
    content: dict[str, Any],
    upstream_path: Path,
    kind: str,
    *,
    record_stride_byte_count: int,
    record_terminator_byte_count: int,
) -> dict[str, Any]:
    section = next(
        (row for row in content["sourceSections"] if row["map"] == 2 and row["kind"] == kind),
        None,
    )
    if section is None:
        raise ValueError(f"map interaction trigger map 02 table is missing: {kind}")
    disasm = upstream_path.resolve(strict=True) / "disasm"
    encoded, record_count, trailing_rts = _encode_source(
        disasm / section["path"], kind, _map_content_equates(disasm)
    )
    if trailing_rts or record_count != section["recordCount"]:
        raise ValueError(f"map interaction trigger map 02 record count drift: {kind}")
    source_record_bytes = content["sourceFacts"]["recordBytes"]
    source_record_kind = kind.removesuffix("s")
    if (
        source_record_bytes[source_record_kind] != record_stride_byte_count
        or source_record_bytes["terminator"] != record_terminator_byte_count
    ):
        raise ValueError(f"map interaction trigger source/callee width disagreement: {kind}")
    terminator = encoded[record_count * record_stride_byte_count :]
    if (
        len(terminator) != record_terminator_byte_count
        or len(encoded) != record_count * record_stride_byte_count + record_terminator_byte_count
    ):
        raise ValueError(f"map interaction trigger map 02 terminator drift: {kind}")
    records = _decode_copy_events(encoded, record_count)
    if len(records) != record_count:
        raise ValueError(f"map interaction trigger map 02 record decode drift: {kind}")
    return {
        "mapIndex": section["map"],
        "kind": kind,
        "symbol": section["symbol"],
        "address": section["address"],
        "recordCount": record_count,
        "recordStrideByteCount": record_stride_byte_count,
        "terminatorByteCount": len(terminator),
        "records": records,
    }


def _hash_table(disasm: Path) -> bytes:
    source = (disasm / HASH_SOURCE_PATH).read_text(encoding="utf-8")
    if not re.search(
        r'^table_MapOffsetHash:\s*\n\s*incbin\s+"data/maps/global/mapoffsethashtable\.bin"\s*$',
        source,
        re.MULTILINE,
    ):
        raise ValueError("map interaction trigger hash table source boundary drift")
    data = (disasm / HASH_DATA_PATH).read_bytes()
    if len(data) < 256:
        raise ValueError("map interaction trigger hash table is too small")
    return data


def _layout_word_address(
    *,
    point: dict[str, int],
    layout_base: int,
    row_shift_bits: int,
) -> int | None:
    if point["y"] >= 0x80:
        return None
    word_offset = ((point["y"] << row_shift_bits) + point["x"]) * 2
    return layout_base + word_offset


def _case_static_expectation(
    case: dict[str, Any],
    tables: dict[str, dict[str, Any]],
    handlers: dict[str, dict[str, Any]],
    constants: dict[str, int],
    hash_table: bytes,
    direct_sites: dict[str, int],
) -> dict[str, Any]:
    event_kind = case["eventKind"]
    table = tables[case["table"]["kind"]]
    if case["table"] != {
        "mapIndex": table["mapIndex"],
        "kind": table["kind"],
        "symbol": table["symbol"],
        "recordIndex": case["table"]["recordIndex"],
        "recordCount": table["recordCount"],
        "terminatorByteOffset": table["recordCount"] * table["recordStrideByteCount"],
    }:
        raise ValueError(f"map interaction trigger table identity drift: {case['id']}")
    record_index = case["table"]["recordIndex"]
    record = table["records"][record_index]
    trigger = case["triggerTile"]
    if case["matchBoundary"] in {"selected-record", "busy-gate", "battle-gate"}:
        if trigger != record["trigger"]:
            raise ValueError(f"map interaction trigger selected record mismatch: {case['id']}")
    elif any(row["trigger"] == trigger for row in table["records"]):
        raise ValueError(f"map interaction trigger miss coordinate matches a record: {case['id']}")
    tile_size = constants["MAP_TILE_SIZE"]
    d0_word = trigger["x"] * tile_size
    d1_word = trigger["y"] * tile_size
    coordinate_shift = constants["coordinateInputShiftBits"]
    hash_mask = constants["coordinateHashMask"]
    hashed_trigger = {
        "x": hash_table[(d0_word >> coordinate_shift) + (d0_word >> coordinate_shift)] & hash_mask,
        "y": hash_table[(d1_word >> coordinate_shift) + (d1_word >> coordinate_shift)] & hash_mask,
    }
    if case["matchBoundary"] == "selected-record" and hashed_trigger != record["trigger"]:
        raise ValueError(f"map interaction trigger hash selection drift: {case['id']}")
    macro = "roofEvent" if event_kind == "roof" else "stepEvent"
    return {
        "id": case["id"],
        "handlerAddress": handlers[macro]["address"],
        "directCalleeCallSiteAddress": direct_sites[event_kind],
        "calleeName": ("PerformMapBlockCopyScript" if event_kind == "roof" else "OpenDoor"),
        "calleeD0WordAtDirectCall": d0_word,
        "calleeD1WordAtDirectCall": d1_word,
        "currentMapSeed": case["initialState"]["currentMap"],
        "hashedTriggerTile": hashed_trigger,
        "layoutSourceWordAddress": _layout_word_address(
            point=record["source"],
            layout_base=constants["FF0000_RAM_START"],
            row_shift_bits=constants["layoutRowShiftBits"],
        ),
        "layoutDestinationWordAddress": _layout_word_address(
            point=record["destination"],
            layout_base=constants["FF0000_RAM_START"],
            row_shift_bits=constants["layoutRowShiftBits"],
        ),
        "selectedTableAddress": table["address"],
        "recordStrideByteCount": table["recordStrideByteCount"],
        "terminatorAddress": (
            table["address"] + table["recordCount"] * table["recordStrideByteCount"]
        ),
    }


def build_map_interaction_trigger_contract(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    """Build the static facts needed for the six-case H3 interaction replay."""
    disasm = upstream_path.resolve(strict=True) / "disasm"
    handler_source = (disasm / HANDLER_SOURCE_PATH).read_text(encoding="utf-8")
    callee_source = (disasm / CALLEE_SOURCE_PATH).read_text(encoding="utf-8")
    h2 = build_map_script_engine_contract(rom_path, upstream_path)
    handlers = _handler_contract(h2)
    content = build_map_content_contract(rom_path, upstream_path)
    callee_use_sites = _callee_use_sites(callee_source)
    callee_derived = _derived_callee_constants(callee_use_sites)
    callee_values = callee_derived["values"]
    constants = _parse_equates(
        (disasm / "sf2const.asm").read_text(encoding="utf-8")
        + "\n"
        + (disasm / "sf2enums.asm").read_text(encoding="utf-8"),
        (
            "FF0000_RAM_START",
            "CURRENT_MAP",
            "CURRENT_BATTLE",
            "VIEW_PLANE_UPDATE_TOGGLE_BITFIELD",
            "word_FFAF42",
            "MAP_AREA_LAYER_TYPE",
            "INDEX_SHIFT_COUNT",
            "MAP_TILE_SIZE",
            "MAPDATA_OFFSET_EVENT_STEP",
            "MAPDATA_OFFSET_EVENT_ROOF",
            "NOT_CURRENTLY_IN_BATTLE",
        ),
    )
    listing = (upstream_path.resolve(strict=True) / "build/sf2build-h1.lst").read_text(
        encoding="utf-8"
    )
    addresses = listing_symbol_addresses(listing)
    hash_table = _hash_table(disasm)
    rom = rom_path.resolve(strict=True).read_bytes()
    hash_table_address = addresses["table_MapOffsetHash"]
    if rom[hash_table_address : hash_table_address + len(hash_table)] != hash_table:
        raise ValueError("map interaction trigger hash table source/ROM parity drift")
    for macro, symbol in (("roofEvent", "csc43_RoofEvent"), ("stepEvent", "csc47_StepEvent")):
        if handlers[macro]["address"] != addresses[symbol]:
            raise ValueError(f"map interaction trigger H1 handler address drift: {symbol}")
    if not re.search(
        r"^csc43_RoofEvent:\s*\n(?:.*\n)*?\s*jsr\s+\(PerformMapBlockCopyScript\)\.w\s*$",
        handler_source,
        re.MULTILINE,
    ) or not re.search(
        r"^csc47_StepEvent:\s*\n(?:.*\n)*?\s*jsr\s+\(OpenDoor\)\.w\s*$",
        handler_source,
        re.MULTILINE,
    ):
        raise ValueError("map interaction trigger handler/callee source join drift")
    return {
        "function": {
            "entryAddress": addresses["RunMapSetupInitFunction"],
            "roofHandlerAddress": handlers["roofEvent"]["address"],
            "stepHandlerAddress": handlers["stepEvent"]["address"],
            "roofDirectCalleeCallSiteAddress": _direct_call_site(
                listing, "csc43_RoofEvent", "PerformMapBlockCopyScript"
            ),
            "stepDirectCalleeCallSiteAddress": _direct_call_site(
                listing, "csc47_StepEvent", "OpenDoor"
            ),
            "roofTableScanAddress": _listing_instruction_address(
                listing, "PerformMapBlockCopyScript", r"\btst\.b\s+\(a2\)"
            ),
            "roofSelectedRecordAddress": _listing_instruction_address(
                listing, "PerformMapBlockCopyScript", r"\btst\.w\s+\(a2\)\+"
            ),
            "stepTableScanAddress": _listing_instruction_address(
                listing, "OpenDoor", r"\btst\.b\s+\(a2\)"
            ),
            "stepSelectedRecordAddress": _listing_instruction_address(
                listing, "OpenDoor", r"\btst\.w\s+\(a2\)\+"
            ),
        },
        "ram": {
            "layoutBaseAddress": constants["FF0000_RAM_START"],
            "currentMapAddress": constants["CURRENT_MAP"],
            "currentBattleAddress": constants["CURRENT_BATTLE"],
            "busyWordAddress": constants["word_FFAF42"],
            "mapAreaLayerTypeAddress": constants["MAP_AREA_LAYER_TYPE"],
            "updateToggleBitfieldAddress": constants["VIEW_PLANE_UPDATE_TOGGLE_BITFIELD"],
        },
        "constants": {
            "mapTileSize": constants["MAP_TILE_SIZE"],
            "coordinateInputShiftBits": callee_values["coordinateInputShiftBits"],
            "coordinateHashMask": callee_values["coordinateHashMask"],
            "mapIndexShiftBits": constants["INDEX_SHIFT_COUNT"],
            "recordStrideByteCount": callee_values["recordStrideByteCount"],
            "recordTerminatorByteCount": callee_values["recordTerminatorByteCount"],
            "layoutRowShiftBits": callee_values["layoutRowShiftBits"],
            "notCurrentlyInBattleByte": constants["NOT_CURRENTLY_IN_BATTLE"],
        },
        "tables": {
            "roofEvents": _event_table(
                content,
                upstream_path,
                "roofEvents",
                record_stride_byte_count=callee_values["recordStrideByteCount"],
                record_terminator_byte_count=callee_values["recordTerminatorByteCount"],
            ),
            "stepEvents": _event_table(
                content,
                upstream_path,
                "stepEvents",
                record_stride_byte_count=callee_values["recordStrideByteCount"],
                record_terminator_byte_count=callee_values["recordTerminatorByteCount"],
            ),
        },
        "calleeUseSites": callee_use_sites,
        "derivedUseSites": callee_derived["useSites"],
    }


def derive_case_expectations(
    static: dict[str, Any], fixture: dict[str, Any], upstream_path: Path
) -> list[dict[str, Any]]:
    """Derive fixture case facts from guarded source, table, and H1 identities."""
    direct_sites = {
        "roof": static["function"]["roofDirectCalleeCallSiteAddress"],
        "step": static["function"]["stepDirectCalleeCallSiteAddress"],
    }
    tables = static["tables"]
    handlers = {
        "roofEvent": {"address": static["function"]["roofHandlerAddress"]},
        "stepEvent": {"address": static["function"]["stepHandlerAddress"]},
    }
    constants = {
        "MAP_TILE_SIZE": static["constants"]["mapTileSize"],
        "FF0000_RAM_START": static["ram"]["layoutBaseAddress"],
        "coordinateInputShiftBits": static["constants"]["coordinateInputShiftBits"],
        "coordinateHashMask": static["constants"]["coordinateHashMask"],
        "layoutRowShiftBits": static["constants"]["layoutRowShiftBits"],
    }
    hash_table = _hash_table(upstream_path.resolve(strict=True) / "disasm")
    result = [
        _case_static_expectation(
            case,
            tables,
            handlers,
            constants,
            hash_table,
            direct_sites,
        )
        for case in fixture["cases"]
    ]
    if [case["expected"] for case in fixture["cases"]] != result:
        raise ValueError("map interaction trigger fixture/static disagreement")
    return result


def verify_map_interaction_trigger(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 120
) -> dict[str, Any]:
    """Run and compare the one-launch roof/step trigger runtime matrix."""
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner="map interaction trigger fixture")
    verify_runtime_contract(fixture, rom_path)
    static = build_map_interaction_trigger_contract(rom_path, upstream_path)
    fixture_function = fixture["function"]
    static_function_subset = {
        field: static["function"][field] for field in fixture_function
    }
    if fixture_function != static_function_subset:
        raise ValueError("map interaction trigger fixture/function source drift")
    derived = derive_case_expectations(static, fixture, upstream_path)
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
                "instrumentation": fixture["instrumentation"],
                "maxFrames": fixture["maxFrames"],
                "harness": load_json(repo_path(fixture["sharedHarnessFixture"]))["harness"],
                "cases": fixture["cases"],
                "derived": derived,
            },
            output_name="map-interaction-trigger",
            timeout_seconds=timeout_seconds,
        )

    observed = _with_instrumented_rom_database(
        instrumented_rom, "SF2 H3 instrumented map interaction trigger", observe
    )
    validate_json(observed, OBSERVATION_SCHEMA, owner="map interaction trigger observation")
    expected = {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["id"],
        "mapTest": fixture["mapTestIndex"],
        "recordOrder": [case["id"] for case in fixture["cases"]],
        "records": [
            {
                **{
                    name: value
                    for name, value in case["expected"].items()
                    if name != "currentMapSeed"
                },
                "handlerReturned": True,
                **case["runtimeGolden"],
            }
            for case in fixture["cases"]
        ],
    }
    if observed != expected:
        raise ValueError(
            "map interaction trigger runtime matrix mismatch\n"
            f"static={derived!r}\nexpected={expected!r}\nobserved={observed!r}"
        )
    return {
        "Fixture": fixture["id"],
        "Cases": len(derived),
        "Handlers": len({case["eventKind"] for case in fixture["cases"]}),
        "BizHawkLaunches": 1,
        "Instrumentation": "session-only",
        "Status": "PASS",
    }
