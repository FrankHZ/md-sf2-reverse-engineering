from __future__ import annotations

import re
from pathlib import Path
from typing import Any

from sf2tool.h2.map_script_engine import build_map_script_engine_contract
from sf2tool.h3.bizhawk import run_observer, verify_runtime_contract
from sf2tool.h3.map_lifecycle import _instrument_rom, _with_instrumented_rom_database
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path
from sf2tool.research_index import listing_symbol_addresses

FIXTURE = repo_path("tests/fixtures/h3/map-camera-control-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h3-map-camera-control-fixture.schema.json")
OBSERVATION_SCHEMA = repo_path("schemas/h3-map-camera-control-observation.schema.json")
OBSERVER = repo_path("tools/bizhawk/map_camera_control_observer.lua")

HANDLER_SOURCE_PATH = Path("code/common/scripting/map/mapscriptengine_1.asm")
SERVICE_SOURCE_PATH = Path("code/gameflow/battle/battlefunctions/battlefunctions_0.asm")
CONSTANTS_PATHS = (Path("sf2const.asm"), Path("sf2enums.asm"))


def _transfer_input(value: int, width: int, *, name: str) -> int:
    if not isinstance(value, int) or not 0 <= value < 1 << (width * 8):
        raise ValueError(f"map camera control {name} is out of source transfer range: {value}")
    return value


def _parse_equates(source: str, names: tuple[str, ...]) -> dict[str, int]:
    """Read every authority value once from the pinned constants text."""
    values: dict[str, int] = {}
    for name in names:
        match = re.search(
            rf"^{re.escape(name)}:\s+equ\s+(?P<value>\$[0-9A-Fa-f]+|-?\d+)\b",
            source,
            re.MULTILINE,
        )
        if match is None:
            raise ValueError(f"map camera control source constant is missing: {name}")
        encoded = match.group("value")
        values[name] = int(encoded[1:], 16) if encoded.startswith("$") else int(encoded)
    return values


def _section(source: str, symbol: str) -> list[dict[str, Any]]:
    """Return comment-free instructions in precisely one named source function."""
    start = re.search(rf"^{re.escape(symbol)}:\s*$", source, re.MULTILINE)
    if start is None:
        raise ValueError(f"map camera control function is missing: {symbol}")
    end = source.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"map camera control function end is missing: {symbol}")
    first_line = source[: start.start()].count("\n")
    records: list[dict[str, Any]] = []
    for offset, raw in enumerate(source[start.start() : end].splitlines(), 1):
        code = raw.split(";", 1)[0].strip()
        if not code or code.endswith(":"):
            continue
        match = re.fullmatch(
            r"(?P<opcode>[A-Za-z][A-Za-z0-9]*(?:\.[bwls])?)(?:\s+(?P<operand>.*))?",
            code,
        )
        if match is None:
            raise ValueError(f"map camera control cannot parse {symbol} line: {raw!r}")
        records.append(
            {
                "opcode": match.group("opcode").lower(),
                "operand": re.sub(r"\s+", "", match.group("operand") or ""),
                "sourceLine": first_line + offset,
            }
        )
    return records


def _require_order(
    section: list[dict[str, Any]], required: tuple[tuple[str, str], ...], *, name: str
) -> list[dict[str, Any]]:
    """Keep order, operands, and explicit instruction widths source-falsifiable."""
    cursor = 0
    result: list[dict[str, Any]] = []
    for opcode, operand in required:
        while cursor < len(section) and (section[cursor]["opcode"], section[cursor]["operand"]) != (
            opcode,
            operand,
        ):
            cursor += 1
        if cursor == len(section):
            raise ValueError(
                f"map camera control semantic drift in {name}: expected {opcode} {operand}"
            )
        result.append(section[cursor])
        cursor += 1
    return result


def _instruction_width(opcode: str) -> int:
    match = re.fullmatch(r"[a-z0-9]+\.([bwl])", opcode)
    if match is None:
        raise ValueError(f"map camera control needs an explicit width: {opcode}")
    return {"b": 1, "w": 2, "l": 4}[match.group(1)]


def _instruction_width_from_statement(instruction: str) -> int:
    """Read an explicit 68000 transfer width from one H2 source-use record."""
    match = re.fullmatch(r"(?P<opcode>[a-z0-9]+\.[bwl])\s+.+", instruction)
    if match is None:
        raise ValueError(f"map camera control transfer instruction drift: {instruction}")
    return _instruction_width(match.group("opcode"))


def _signed_transfer(value: int, width: int) -> int:
    """Encode an H2 parsed signed literal through its source write width."""
    if not 1 <= width <= 4:
        raise ValueError(f"map camera control transfer width is unsupported: {width}")
    return int.from_bytes(value.to_bytes(width, "big", signed=True), "big")


def _sign_bit(width: int) -> int:
    """Derive a signed-test bit from the parsed width of that source transfer."""
    if not 1 <= width <= 4:
        raise ValueError(f"map camera control sign-test width is unsupported: {width}")
    return 1 << (width * 8 - 1)


def _lower_transfer(value: int, width: int) -> int:
    """Model a parsed 68000 transfer width without a second literal mask truth."""
    if value < 0:
        raise ValueError("map camera control transfer value must be unsigned")
    return int.from_bytes(value.to_bytes(4, "big")[-width:], "big")


def _single_h2_use_site(records: object, *, required_keys: set[str], name: str) -> dict[str, Any]:
    """Require one closed H2 source-use record before using its relation."""
    if not isinstance(records, list) or len(records) != 1 or not isinstance(records[0], dict):
        raise ValueError(f"map camera control H2 {name} cardinality drift")
    record = records[0]
    if set(record) != required_keys:
        raise ValueError(f"map camera control H2 {name} record-shape drift")
    return record


def _cursor_read_use_site(
    records: object, *, destination_register: str, name: str
) -> tuple[dict[str, Any], int]:
    record = _single_h2_use_site(
        records,
        required_keys={
            "sourceRegister",
            "destinationRegister",
            "transferredByteCount",
            "cursorAdvanceByteCount",
            "instruction",
        },
        name=name,
    )
    if (
        record["sourceRegister"] != "a6"
        or record["destinationRegister"] != destination_register
        or record["instruction"] != f"move.w (a6)+,{destination_register}"
    ):
        raise ValueError(f"map camera control H2 {name} identity drift")
    width = _instruction_width_from_statement(record["instruction"])
    if record["transferredByteCount"] != width or record["cursorAdvanceByteCount"] != width:
        raise ValueError(f"map camera control H2 {name} transfer-width drift")
    return record, width


def _cursor_write_use_site(records: object) -> tuple[dict[str, Any], int]:
    record = _single_h2_use_site(
        records,
        required_keys={
            "sourceRegister",
            "destinationOperand",
            "transferredByteCount",
            "cursorAdvanceByteCount",
            "instruction",
        },
        name="speed script cursor write",
    )
    instruction = "move.w (a6)+,((VIEW_SCROLLING_SPEED-$1000000)).w"
    if (
        record["sourceRegister"] != "a6"
        or record["destinationOperand"] != "((VIEW_SCROLLING_SPEED-$1000000)).w"
        or record["instruction"] != instruction
    ):
        raise ValueError("map camera control H2 speed script cursor write identity drift")
    width = _instruction_width_from_statement(record["instruction"])
    if record["transferredByteCount"] != width or record["cursorAdvanceByteCount"] != width:
        raise ValueError("map camera control H2 speed script cursor write transfer-width drift")
    return record, width


def _branch_record(
    records: object,
    *,
    test_instruction: str,
    branch_instruction: str,
    target_label: str,
    target_instruction: str,
    target_statement_index: int,
    name: str,
) -> dict[str, Any]:
    if not isinstance(records, list) or len(records) != 2:
        raise ValueError("map camera control H2 target branch-record cardinality drift")
    matching = [
        row
        for row in records
        if isinstance(row, dict) and row.get("branchInstruction") == branch_instruction
    ]
    if len(matching) != 1:
        raise ValueError(f"map camera control H2 {name} branch polarity drift")
    record = matching[0]
    if set(record) != {
        "testInstruction",
        "branchInstruction",
        "branchTargetLabel",
        "branchTarget",
    } or not isinstance(record["branchTarget"], dict):
        raise ValueError(f"map camera control H2 {name} branch-record shape drift")
    if record != {
        "testInstruction": test_instruction,
        "branchInstruction": branch_instruction,
        "branchTargetLabel": target_label,
        "branchTarget": {
            "targetLabel": target_label,
            "targetInstruction": target_instruction,
            "targetStatementIndex": target_statement_index,
        },
    }:
        raise ValueError(f"map camera control H2 {name} branch relation drift")
    return record


def _constant_use_site(
    records: object, *, symbol: str, value: int, instruction: str
) -> dict[str, Any]:
    if not isinstance(records, list):
        raise ValueError("map camera control H2 target constant-use container drift")
    matching = [row for row in records if isinstance(row, dict) and row.get("symbol") == symbol]
    if len(matching) != 1:
        raise ValueError(f"map camera control H2 {symbol} use-site identity drift")
    record = matching[0]
    if set(record) != {"symbol", "value", "instruction"} or record != {
        "symbol": symbol,
        "value": value,
        "instruction": instruction,
    }:
        raise ValueError(f"map camera control H2 {symbol} use-site relation drift")
    return record


def _state_write(
    records: object,
    *,
    source_symbol: str,
    value_kind: str,
    value_reference: object,
    instruction: str,
    name: str,
) -> tuple[dict[str, Any], int]:
    record = _single_h2_use_site(
        records,
        required_keys={"sourceSymbol", "valueKind", "valueReference", "instruction"},
        name=name,
    )
    if record != {
        "sourceSymbol": source_symbol,
        "valueKind": value_kind,
        "valueReference": value_reference,
        "instruction": instruction,
    }:
        raise ValueError(f"map camera control H2 {name} relation drift")
    return record, _instruction_width_from_statement(record["instruction"])


def _literal_state_write(
    records: object, *, source_symbol: str, name: str
) -> tuple[dict[str, Any], int, int]:
    """Resolve the destination reset literal from its H2 guarded source write."""
    record = _single_h2_use_site(
        records,
        required_keys={"sourceSymbol", "valueKind", "valueReference", "instruction"},
        name=name,
    )
    literal_match = re.fullmatch(
        r"move\.(?P<size>[bwl]) #(?P<literal>-?\d+),\(\(VIEW_TARGET_ENTITY-\$1000000\)\)\.w",
        record["instruction"],
    )
    if (
        record["sourceSymbol"] != source_symbol
        or record["valueKind"] != "literal"
        or literal_match is None
    ):
        raise ValueError(f"map camera control H2 {name} identity drift")
    literal = int(literal_match.group("literal"))
    if record["valueReference"] != literal:
        raise ValueError(f"map camera control H2 {name} literal relation drift")
    return record, _instruction_width_from_statement(record["instruction"]), literal


def _service_use_sites(source: str) -> dict[str, Any]:
    """Guard the only source relation used to derive destination transfer words."""
    section = _section(source, "SetCameraDestination")
    ordered = _require_order(
        section,
        (
            ("mulu.w", "#MAP_TILE_SIZE,d2"),
            ("mulu.w", "#MAP_TILE_SIZE,d3"),
            ("movem.w", "d2-d3,-(sp)"),
            ("movem.w", "(sp)+,d0-d1"),
            ("jsr", "(SetViewDestination).w"),
            ("rts", ""),
        ),
        name="SetCameraDestination multiplication, transfer, call, and return",
    )
    multiplication = ordered[:2]
    transfer = ordered[2:4]
    multiplication_widths = [_instruction_width(row["opcode"]) for row in multiplication]
    transfer_widths = [_instruction_width(row["opcode"]) for row in transfer]
    if len(set(multiplication_widths)) != 1 or multiplication_widths != transfer_widths:
        raise ValueError("map camera control multiplier/transfer width relation drift")
    return {"multiplication": multiplication, "transfer": transfer, "callAndReturn": ordered[4:]}


def _direct_call_site(listing: str, symbol: str, target: str) -> int:
    start = re.search(rf"^[0-9A-F]{{8}}\s+{re.escape(symbol)}:\s*$", listing, re.MULTILINE)
    if start is None:
        raise ValueError(f"map camera control H1 function is missing: {symbol}")
    end = listing.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"map camera control H1 function end is missing: {symbol}")
    target_pattern = re.escape(target)
    pattern = re.compile(
        rf"^(?P<address>[0-9A-F]{{8}})\s+.*?\bjsr\s+\(?{target_pattern}\)?(?:\.w)?\s*$",
        re.IGNORECASE,
    )
    matches = []
    for raw in listing[start.start() : end].splitlines():
        match = pattern.fullmatch(raw.split(";", 1)[0].rstrip())
        if match is not None:
            matches.append(match.group("address"))
    if len(matches) != 1:
        raise ValueError(
            f"map camera control direct call-site drift for {symbol}/{target}: {len(matches)}"
        )
    return int(matches[0], 16)


def _instruction_site(listing: str, symbol: str, pattern: str) -> int:
    start = re.search(rf"^[0-9A-F]{{8}}\s+{re.escape(symbol)}:\s*$", listing, re.MULTILINE)
    if start is None:
        raise ValueError(f"map camera control H1 function is missing: {symbol}")
    end = listing.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"map camera control H1 function end is missing: {symbol}")
    matches = re.findall(
        rf"^(?P<address>[0-9A-F]{{8}})\s+.*?{pattern}\s*$",
        listing[start.start() : end],
        re.MULTILINE | re.IGNORECASE,
    )
    if len(matches) != 1:
        raise ValueError(
            f"map camera control instruction callback drift in {symbol}: {len(matches)}"
        )
    return int(matches[0], 16)


def _handler_facts(h2: dict[str, Any], constants: dict[str, int]) -> dict[str, Any]:
    """Carry H2's named use sites into H3's derived width/sign relationships."""
    facts = h2["mapCameraControlCommandFacts"]
    handlers = {row["macro"]: row for row in facts["handlers"]}
    required = {
        "setCameraEntity": "csc24_setCameraTargetEntity",
        "setCamDest": "csc32_setCameraDestInTiles",
        "cameraSpeed": "csc45_cameraSpeed",
    }
    if set(handlers) != set(required):
        raise ValueError("map camera control H2 handler set drift")
    for macro, symbol in required.items():
        if handlers[macro]["handler"] != symbol:
            raise ValueError(f"map camera control H2 handler identity drift: {macro}")
    target_guard = handlers["setCameraEntity"]["sectionGuard"]
    if target_guard["orderedInstructions"] != [
        "lea ((ENTITY_INDEX_LIST-$1000000)).w,a5",
        "move.w (a6)+,d0",
        "bmi.w loc_46C52",
        "tst.b d0",
        "bpl.s @Ally",
        "subi.b #ENTITY_ENEMY_INDEX_DIFFERENCE,d0",
        "andi.w #BYTE_MASK,d0",
        "move.b (a5,d0.w),d0",
        "move.b d0,((VIEW_TARGET_ENTITY-$1000000)).w",
        "nop",
        "rts",
    ]:
        raise ValueError("map camera control target handler guard drift")
    destination_guard = handlers["setCamDest"]["sectionGuard"]
    if destination_guard["orderedInstructions"] != [
        "move.b #-1,((VIEW_TARGET_ENTITY-$1000000)).w",
        "nop",
        "move.w (a6)+,d2",
        "move.w (a6)+,d3",
        "jsr j_SetCameraDestination",
        "jsr (WaitForViewScrollEnd).w",
        "rts",
    ]:
        raise ValueError("map camera control destination handler guard drift")
    speed_guard = handlers["cameraSpeed"]["sectionGuard"]
    if speed_guard["orderedInstructions"] != [
        "move.w (a6)+,((VIEW_SCROLLING_SPEED-$1000000)).w",
        "nop",
        "rts",
    ]:
        raise ValueError("map camera control speed handler guard drift")
    target_read, target_input_width = _cursor_read_use_site(
        target_guard["scriptCursorReadUseSites"],
        destination_register="d0",
        name="target script cursor read",
    )
    target_branches = [
        _branch_record(
            target_guard["branchRecords"],
            test_instruction=target_read["instruction"],
            branch_instruction="bmi.w loc_46C52",
            target_label="loc_46C52",
            target_instruction="move.b d0,((VIEW_TARGET_ENTITY-$1000000)).w",
            target_statement_index=8,
            name="target negative",
        ),
        _branch_record(
            target_guard["branchRecords"],
            test_instruction="tst.b d0",
            branch_instruction="bpl.s @Ally",
            target_label="@Ally",
            target_instruction="andi.w #BYTE_MASK,d0",
            target_statement_index=6,
            name="target ally",
        ),
    ]
    if len(target_guard["sourceConstantUses"]) != 2:
        raise ValueError("map camera control H2 target constant-use cardinality drift")
    target_constant_uses = [
        _constant_use_site(
            target_guard["sourceConstantUses"],
            symbol="ENTITY_ENEMY_INDEX_DIFFERENCE",
            value=constants["ENTITY_ENEMY_INDEX_DIFFERENCE"],
            instruction="subi.b #ENTITY_ENEMY_INDEX_DIFFERENCE,d0",
        ),
        _constant_use_site(
            target_guard["sourceConstantUses"],
            symbol="BYTE_MASK",
            value=constants["BYTE_MASK"],
            instruction="andi.w #BYTE_MASK,d0",
        ),
    ]
    if [row["symbol"] for row in target_constant_uses] != [
        "ENTITY_ENEMY_INDEX_DIFFERENCE",
        "BYTE_MASK",
    ]:
        raise ValueError("map camera control H2 target constant-use order drift")
    target_write, target_output_width = _state_write(
        target_guard["sourceStateWrites"],
        source_symbol="VIEW_TARGET_ENTITY",
        value_kind="register",
        value_reference="d0",
        instruction="move.b d0,((VIEW_TARGET_ENTITY-$1000000)).w",
        name="target state write",
    )
    if len(destination_guard["scriptCursorReadUseSites"]) != 2:
        raise ValueError("map camera control H2 destination script cursor read cardinality drift")
    destination_reads = [
        _cursor_read_use_site(
            [row], destination_register=register, name=f"destination script cursor read {register}"
        )
        for row, register in zip(
            destination_guard["scriptCursorReadUseSites"], ("d2", "d3"), strict=True
        )
    ]
    destination_read_widths = [width for _, width in destination_reads]
    if len(set(destination_read_widths)) != 1:
        raise ValueError("map camera control H2 destination script cursor read width drift")
    destination_write, destination_output_width, destination_literal = _literal_state_write(
        destination_guard["sourceStateWrites"],
        source_symbol="VIEW_TARGET_ENTITY",
        name="destination target state write",
    )
    speed_write, speed_width = _cursor_write_use_site(speed_guard["scriptCursorWriteUseSites"])
    _state_write(
        speed_guard["sourceStateWrites"],
        source_symbol="VIEW_SCROLLING_SPEED",
        value_kind="script-cursor",
        value_reference="(a6)+",
        instruction=speed_write["instruction"],
        name="speed state write",
    )
    return {
        "handlers": handlers,
        "target": {
            "cursorReadUseSite": target_read,
            "branchRecords": target_branches,
            "constantUseSites": target_constant_uses,
            "stateWrite": target_write,
            "inputByteCount": target_input_width,
            "outputByteCount": target_output_width,
        },
        "destination": {
            "cursorReadUseSites": [row for row, _ in destination_reads],
            "inputByteCount": destination_read_widths[0],
            "stateWrite": destination_write,
            "outputByteCount": destination_output_width,
            "stateLiteral": destination_literal,
        },
        "speed": {"cursorWriteUseSite": speed_write, "outputByteCount": speed_width},
    }


def build_map_camera_control_contract(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    """Build source-falsifiable camera command facts for the grouped H3 matrix."""
    upstream = upstream_path.resolve(strict=True)
    disasm = upstream / "disasm"
    h2 = build_map_script_engine_contract(rom_path, upstream)
    constants_source = "\n".join(
        (disasm / path).read_text(encoding="utf-8") for path in CONSTANTS_PATHS
    )
    constants = _parse_equates(
        constants_source,
        (
            "ENTITY_INDEX_LIST",
            "VIEW_TARGET_ENTITY",
            "VIEW_SCROLLING_SPEED",
            "ENTITY_ENEMY_INDEX_DIFFERENCE",
            "BYTE_MASK",
            "MAP_TILE_SIZE",
        ),
    )
    handler_facts = _handler_facts(h2, constants)
    handlers = handler_facts["handlers"]
    service_use_sites = _service_use_sites(
        (disasm / SERVICE_SOURCE_PATH).read_text(encoding="utf-8")
    )
    multiplier_operands = {
        row["operand"].split(",", 1)[0] for row in service_use_sites["multiplication"]
    }
    if multiplier_operands != {"#MAP_TILE_SIZE"}:
        raise ValueError("map camera control multiplier symbol drift")
    listing = (upstream / "build/sf2build-h1.lst").read_text(encoding="utf-8")
    addresses = listing_symbol_addresses(listing)
    for macro, symbol in (
        ("setCameraEntity", "csc24_setCameraTargetEntity"),
        ("setCamDest", "csc32_setCameraDestInTiles"),
        ("cameraSpeed", "csc45_cameraSpeed"),
    ):
        if handlers[macro]["address"] != addresses[symbol]:
            raise ValueError(f"map camera control H1 handler address drift: {symbol}")
    return {
        "function": {
            "entryAddress": addresses["RunMapSetupInitFunction"],
            "setCameraEntityHandlerAddress": handlers["setCameraEntity"]["address"],
            "setCameraDestinationHandlerAddress": handlers["setCamDest"]["address"],
            "cameraSpeedHandlerAddress": handlers["cameraSpeed"]["address"],
            "targetEntityLookupAddress": _instruction_site(
                listing, "csc24_setCameraTargetEntity", r"\bmove\.b\s+\(a5,d0\.w\),d0"
            ),
            "setCameraDestinationCallSiteAddress": _direct_call_site(
                listing, "csc32_setCameraDestInTiles", "j_SetCameraDestination"
            ),
            "waitForViewScrollEndCallSiteAddress": _direct_call_site(
                listing, "csc32_setCameraDestInTiles", "WaitForViewScrollEnd"
            ),
            "setCameraDestinationServiceAddress": addresses["SetCameraDestination"],
            "setViewDestinationAddress": addresses["SetViewDestination"],
            "setViewDestinationCallSiteAddress": _direct_call_site(
                listing, "SetCameraDestination", "SetViewDestination"
            ),
            "waitForViewScrollEndAddress": addresses["WaitForViewScrollEnd"],
        },
        "ram": {
            "entityIndexListAddress": constants["ENTITY_INDEX_LIST"],
            "viewTargetEntityAddress": constants["VIEW_TARGET_ENTITY"],
            "viewScrollingSpeedAddress": constants["VIEW_SCROLLING_SPEED"],
        },
        "constants": {
            "enemyIndexDifference": constants["ENTITY_ENEMY_INDEX_DIFFERENCE"],
            "byteMask": constants["BYTE_MASK"],
            "mapTileSize": constants["MAP_TILE_SIZE"],
        },
        "serviceUseSites": service_use_sites,
        "transferWidths": {
            "targetInputWordByteCount": handler_facts["target"]["inputByteCount"],
            "targetInputSignBit": _sign_bit(handler_facts["target"]["inputByteCount"]),
            "targetByteCount": handler_facts["target"]["outputByteCount"],
            "targetByteSignBit": _sign_bit(handler_facts["target"]["outputByteCount"]),
            "destinationInputWordByteCount": handler_facts["destination"]["inputByteCount"],
            "destinationWordByteCount": _instruction_width(
                service_use_sites["transfer"][1]["opcode"]
            ),
            "destinationTargetByteCount": handler_facts["destination"]["outputByteCount"],
            "speedWordByteCount": handler_facts["speed"]["outputByteCount"],
        },
        "sourceStateValues": {
            "destinationViewTargetEntityLiteral": handler_facts["destination"]["stateLiteral"]
        },
    }


def _target_expected(case: dict[str, Any], static: dict[str, Any]) -> dict[str, Any]:
    widths = static["transferWidths"]
    operand = _transfer_input(
        case["operandWord"], widths["targetInputWordByteCount"], name="target operand"
    )
    constants = static["constants"]
    byte_width = widths["targetByteCount"]
    raw_byte = _lower_transfer(operand, byte_width)
    mode = case["targetMode"]
    if mode == "negative-direct":
        if (
            not operand & widths["targetInputSignBit"]
            or case["entityIndexListByteSeed"] is not None
        ):
            raise ValueError(f"map camera control negative branch fixture drift: {case['id']}")
        lookup_index = None
        result = raw_byte
    elif mode == "ally-index":
        if (
            operand & widths["targetInputSignBit"]
            or raw_byte & widths["targetByteSignBit"]
            or not isinstance(case["entityIndexListByteSeed"], int)
        ):
            raise ValueError(f"map camera control ally branch fixture drift: {case['id']}")
        lookup_index = raw_byte & constants["byteMask"]
        result = case["entityIndexListByteSeed"]
    elif mode == "enemy-index":
        if (
            operand & widths["targetInputSignBit"]
            or not raw_byte & widths["targetByteSignBit"]
            or not isinstance(case["entityIndexListByteSeed"], int)
        ):
            raise ValueError(f"map camera control enemy branch fixture drift: {case['id']}")
        lookup_index = (raw_byte - constants["enemyIndexDifference"]) & constants["byteMask"]
        result = case["entityIndexListByteSeed"]
    else:
        raise ValueError(f"map camera control target mode is unknown: {mode}")
    if not 0 <= result <= constants["byteMask"]:
        raise ValueError(f"map camera control target result is out of byte range: {case['id']}")
    return {
        "id": case["id"],
        "kind": "target",
        "targetMode": mode,
        "handlerAddress": static["function"]["setCameraEntityHandlerAddress"],
        "operandWord": operand,
        "targetEntityLookupAddress": (
            static["function"]["targetEntityLookupAddress"] if lookup_index is not None else None
        ),
        "entityIndexListLookupIndex": lookup_index,
        "viewTargetEntityByteAfter": result,
    }


def _destination_expected(case: dict[str, Any], static: dict[str, Any]) -> dict[str, Any]:
    widths = static["transferWidths"]
    input_words = [
        _transfer_input(value, widths["destinationInputWordByteCount"], name="destination operand")
        for value in case["inputWords"]
    ]
    if len(input_words) != 2:
        raise ValueError(f"map camera control destination operand count drift: {case['id']}")
    width = widths["destinationWordByteCount"]
    scaled = [
        _lower_transfer(value * static["constants"]["mapTileSize"], width) for value in input_words
    ]
    return {
        "id": case["id"],
        "kind": "destination",
        "handlerAddress": static["function"]["setCameraDestinationHandlerAddress"],
        "setCameraDestinationCallSiteAddress": static["function"][
            "setCameraDestinationCallSiteAddress"
        ],
        "waitForViewScrollEndCallSiteAddress": static["function"][
            "waitForViewScrollEndCallSiteAddress"
        ],
        "setCameraDestinationServiceAddress": static["function"][
            "setCameraDestinationServiceAddress"
        ],
        "setViewDestinationAddress": static["function"]["setViewDestinationAddress"],
        "setViewDestinationCallSiteAddress": static["function"][
            "setViewDestinationCallSiteAddress"
        ],
        "inputWords": input_words,
        "setViewDestinationD0Word": scaled[0],
        "setViewDestinationD1Word": scaled[1],
        "viewTargetEntityByteAfter": _signed_transfer(
            static["sourceStateValues"]["destinationViewTargetEntityLiteral"],
            widths["destinationTargetByteCount"],
        ),
    }


def _speed_expected(case: dict[str, Any], static: dict[str, Any]) -> dict[str, Any]:
    operand = _transfer_input(
        case["operandWord"], static["transferWidths"]["speedWordByteCount"], name="speed operand"
    )
    return {
        "id": case["id"],
        "kind": "speed",
        "handlerAddress": static["function"]["cameraSpeedHandlerAddress"],
        "operandWord": operand,
        "viewScrollingSpeedWordAfter": operand,
    }


def derive_case_expectations(
    static: dict[str, Any], fixture: dict[str, Any]
) -> list[dict[str, Any]]:
    """Derive each fixture's full static semantic record before comparing goldens."""
    expected = []
    for case in fixture["cases"]:
        if case["kind"] == "target":
            expected.append(_target_expected(case, static))
        elif case["kind"] == "destination":
            expected.append(_destination_expected(case, static))
        elif case["kind"] == "speed":
            expected.append(_speed_expected(case, static))
        else:
            raise ValueError(f"map camera control case kind is unknown: {case['kind']}")
    if [case["expected"] for case in fixture["cases"]] != expected:
        raise ValueError("map camera control fixture/static disagreement")
    return expected


def verify_map_camera_control(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 120
) -> dict[str, Any]:
    """Run the source-derived seven-case camera-command matrix in one launch."""
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner="map camera control fixture")
    verify_runtime_contract(fixture, rom_path)
    static = build_map_camera_control_contract(rom_path, upstream_path)
    if fixture["function"] != static["function"] or fixture["ram"] != static["ram"]:
        raise ValueError("map camera control fixture/source identity drift")
    derived = derive_case_expectations(static, fixture)
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
            output_name="map-camera-control",
            timeout_seconds=timeout_seconds,
        )

    observed = _with_instrumented_rom_database(
        instrumented_rom, "SF2 H3 instrumented map camera control", observe
    )
    validate_json(observed, OBSERVATION_SCHEMA, owner="map camera control observation")
    expected = {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["id"],
        "mapTest": fixture["mapTestIndex"],
        "recordOrder": [case["id"] for case in fixture["cases"]],
        "records": [{**case["expected"], **case["runtimeGolden"]} for case in fixture["cases"]],
    }
    if observed != expected:
        raise ValueError(
            "map camera control runtime matrix mismatch\n"
            f"static={derived!r}\nexpected={expected!r}\nobserved={observed!r}"
        )
    return {
        "Fixture": fixture["id"],
        "Cases": len(derived),
        "Handlers": len({case["kind"] for case in fixture["cases"]}),
        "BizHawkLaunches": 1,
        "Instrumentation": "session-only",
        "Status": "PASS",
    }
