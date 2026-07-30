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

FIXTURE = repo_path("tests/fixtures/h3/map-script-entity-placement-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h3-map-script-entity-placement-fixture.schema.json")
OBSERVATION_SCHEMA = repo_path("schemas/h3-map-script-entity-placement-observation.schema.json")
OBSERVER = repo_path("tools/bizhawk/map_entity_placement_observer.lua")
H2_FIXTURE = repo_path("tests/fixtures/h2/map-script-engine-static-v1.json")

CONSTANTS_PATHS = (Path("sf2const.asm"), Path("sf2enums.asm"))
H1_LISTING_PATH = Path("build/sf2build-h1.lst")

HANDLER_PROFILES = (
    ("setPos", "csc19_setEntityPosAndFacing"),
    ("setPosFlash", "csc17_setEntityPosAndFacingWithFlash"),
    ("setFacing", "csc23_setEntityFacing"),
    ("setDest", "csc29_setEntityDest"),
)
BRANCH_INSTRUCTION_ORDERS = {
    "setPos": (),
    "setPosFlash": ("bhi.s loc_469D0", "dbf d7,loc_469BA"),
    "setFacing": (),
    "setDest": (
        "bpl.s loc_46DC4",
        "bpl.s loc_46DDA",
        "bne.s return_46DEC",
    ),
}


def _parse_equates(source: str, names: set[str]) -> dict[str, int]:
    """Parse the source authority once for fields and MAP_TILE_SIZE."""
    values: dict[str, int] = {}
    for name in sorted(names):
        match = re.search(
            rf"^{re.escape(name)}:\s+equ\s+(?P<value>\$[0-9A-Fa-f]+|-?\d+)\b",
            source,
            re.MULTILINE,
        )
        if match is None:
            raise ValueError(f"entity placement source equate is missing: {name}")
        text = match.group("value")
        values[name] = int(text[1:], 16) if text.startswith("$") else int(text)
    return values


def _literal_value(text: str) -> int:
    if re.fullmatch(r"\$[0-9A-Fa-f]+", text):
        return int(text[1:], 16)
    if re.fullmatch(r"-?\d+", text):
        return int(text)
    raise ValueError(f"entity placement source literal is not numeric: {text}")


def _instruction_width(instruction: str) -> int:
    match = re.fullmatch(r"[a-z0-9]+\.(?P<size>[bwl])\s+.+", instruction)
    if match is None:
        raise ValueError(f"entity placement instruction needs an explicit width: {instruction}")
    return {"b": 1, "w": 2, "l": 4}[match.group("size")]


def _sign_bit(width: int) -> int:
    if not 1 <= width <= 4:
        raise ValueError(f"entity placement width cannot have a sign bit: {width}")
    return 1 << (width * 8 - 1)


def _width_mask(width: int) -> int:
    return (_sign_bit(width) << 1) - 1


def _unsigned_width(value: int, width: int, *, name: str) -> int:
    if not isinstance(value, int):
        raise ValueError(f"entity placement {name} is not numeric")
    return value & _width_mask(width)


def _signed_width(value: int, width: int, *, name: str) -> int:
    unsigned = _unsigned_width(value, width, name=name)
    sign_bit = _sign_bit(width)
    return unsigned - (sign_bit << 1) if unsigned & sign_bit else unsigned


def _h1_function_lines(listing: str, symbol: str) -> list[tuple[int, str]]:
    """Return comment-free H1 instructions from exactly one named listing section."""
    start = re.search(rf"^[0-9A-F]{{8}}\s+{re.escape(symbol)}:\s*$", listing, re.MULTILINE)
    if start is None:
        raise ValueError(f"entity placement H1 function is missing: {symbol}")
    end = listing.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"entity placement H1 function end is missing: {symbol}")
    records: list[tuple[int, str]] = []
    for raw in listing[start.start() : end].splitlines():
        match = re.fullmatch(r"(?P<address>[0-9A-F]{8})\s+(?P<body>.*)", raw)
        if match is None:
            continue
        body = match.group("body").split(";", 1)[0].strip()
        body = re.sub(r"^(?:[0-9A-F]{2,8}\s+)+", "", body).strip()
        if not body or body.endswith(":"):
            continue
        if re.fullmatch(r"[A-Za-z][A-Za-z0-9_]*(?:\.[bwls])?(?:\s+.+)?", body) is None:
            raise ValueError(f"entity placement H1 instruction parse drift: {raw}")
        records.append((int(match.group("address"), 16), re.sub(r"\s+", "", body)))
    return records


def _h1_instruction_address(listing: str, symbol: str, instruction: str) -> int:
    expected = re.sub(r"\s+", "", instruction)
    matches = [
        address
        for address, actual in _h1_function_lines(listing, symbol)
        if actual == expected
    ]
    if len(matches) != 1:
        raise ValueError(
            "entity placement H1 instruction identity drift for "
            f"{symbol}/{instruction}: {len(matches)}"
        )
    return matches[0]


def _h1_ordered_direct_call_addresses(
    listing: str, symbol: str, instructions: list[str]
) -> list[int]:
    """Keep repeated direct calls distinct while guarding their complete H1 order."""
    expected = [re.sub(r"\s+", "", instruction) for instruction in instructions]
    observed = [
        (address, instruction)
        for address, instruction in _h1_function_lines(listing, symbol)
        if re.match(r"(?:bsr|jsr)(?:\.[bwls])?", instruction)
    ]
    if [instruction for _, instruction in observed] != expected:
        raise ValueError(f"entity placement H1 direct-call order drift: {symbol}")
    return [address for address, _ in observed]


def _field_symbols(records: list[dict[str, Any]]) -> set[str]:
    names: set[str] = {"MAP_TILE_SIZE"}
    for record in records:
        operand = record["sourceOperand"]
        match = re.fullmatch(r"(?P<symbol>ENTITYDEF_OFFSET_[A-Z0-9_]+)\(a5\)", operand)
        if match is not None:
            names.add(match.group("symbol"))
        elif operand != "(a5)":
            raise ValueError(f"entity placement source state operand drift: {operand}")
    return names


def _field_offset(operand: str, equates: dict[str, int]) -> int:
    if operand == "(a5)":
        return 0
    match = re.fullmatch(r"(?P<symbol>ENTITYDEF_OFFSET_[A-Z0-9_]+)\(a5\)", operand)
    if match is None or match.group("symbol") not in equates:
        raise ValueError(f"entity placement field offset use-site drift: {operand}")
    return equates[match.group("symbol")]


def _closed_records(value: object, required: set[str], *, name: str) -> list[dict[str, Any]]:
    if not isinstance(value, list) or not all(isinstance(row, dict) for row in value):
        raise ValueError(f"entity placement H2 {name} container drift")
    rows = list(value)
    if any(set(row) != required for row in rows):
        raise ValueError(f"entity placement H2 {name} record shape drift")
    return rows


def _cursor_records(guard: dict[str, Any], *, name: str) -> list[dict[str, Any]]:
    records = _closed_records(
        guard["scriptCursorReadUseSites"],
        {
            "sourceRegister",
            "destinationOperand",
            "transferredByteCount",
            "cursorAdvanceByteCount",
            "instruction",
        },
        name=f"{name} cursor read",
    )
    for record in records:
        width = _instruction_width(record["instruction"])
        if record["sourceRegister"] != "a6" or record["transferredByteCount"] != width:
            raise ValueError(f"entity placement H2 {name} cursor width drift")
        if record["cursorAdvanceByteCount"] not in (0, width):
            raise ValueError(f"entity placement H2 {name} cursor advance drift")
    return records


def _state_records(
    records: object, equates: dict[str, int], *, access: str, name: str
) -> list[dict[str, Any]]:
    h2_records = _closed_records(
        records, {"sourceOperand", "instruction"}, name=f"{name} {access} state"
    )
    result = []
    for record in h2_records:
        if record["sourceOperand"] not in record["instruction"]:
            raise ValueError(f"entity placement H2 {name} {access} field use-site drift")
        result.append(
            {
                **record,
                "fieldOffset": _field_offset(record["sourceOperand"], equates),
                "transferByteCount": _instruction_width(record["instruction"]),
            }
        )
    return result


def _entity_field_from_use_site(
    records: list[dict[str, Any]], operand: str, *, name: str
) -> dict[str, int]:
    """Resolve one observer field's offset and stored width from its H2 use site."""
    matches = [record for record in records if record["sourceOperand"] == operand]
    if len(matches) != 1:
        raise ValueError(f"entity placement {name} field offset use-site drift")
    return {
        "byteOffset": matches[0]["fieldOffset"],
        "transferByteCount": matches[0]["transferByteCount"],
    }


def _literal_records(records: object, *, name: str) -> list[dict[str, Any]]:
    h2_records = _closed_records(
        records, {"literalText", "value", "instruction"}, name=f"{name} literal use"
    )
    for record in h2_records:
        if record["value"] != _literal_value(record["literalText"]):
            raise ValueError(f"entity placement H2 {name} literal relation drift")
    return h2_records


def _constant_records(
    records: object, equates: dict[str, int], *, name: str
) -> list[dict[str, Any]]:
    h2_records = _closed_records(
        records, {"symbol", "value", "instruction"}, name=f"{name} constant use"
    )
    for record in h2_records:
        if (
            record["symbol"] != "MAP_TILE_SIZE"
            or record["value"] != equates[record["symbol"]]
            or re.fullmatch(r"mulu\.w #MAP_TILE_SIZE,d[0-7]", record["instruction"]) is None
        ):
            raise ValueError(f"entity placement H2 {name} MAP_TILE_SIZE use-site drift")
    return h2_records


def _branch_records(
    records: object, ordered_instructions: list[str], listing: str, handler: str, *, name: str
) -> list[dict[str, Any]]:
    h2_records = _closed_records(
        records, {"branchInstruction", "branchTarget"}, name=f"{name} branch"
    )
    result = []
    for record in h2_records:
        target = record["branchTarget"]
        if set(target) != {"targetLabel", "targetInstruction", "targetStatementIndex"}:
            raise ValueError(f"entity placement H2 {name} branch target shape drift")
        if not re.fullmatch(
            r"(?:b(?:hi|pl|ne)\.[bwls]|dbf)\s+(?:d7,)?[A-Za-z_][A-Za-z0-9_]*",
            record["branchInstruction"],
        ):
            raise ValueError(f"entity placement H2 {name} branch polarity drift")
        try:
            branch_index = ordered_instructions.index(record["branchInstruction"])
        except ValueError as exc:
            raise ValueError(f"entity placement H2 {name} branch polarity/order drift") from exc
        if branch_index == 0:
            raise ValueError(f"entity placement H2 {name} branch predecessor drift")
        predecessor = ordered_instructions[branch_index - 1]
        predecessor_width = (
            _instruction_width(predecessor)
            if re.fullmatch(r"[a-z0-9]+\.[bwl]\s+.+", predecessor)
            else None
        )
        result.append(
            {
                "branchInstruction": record["branchInstruction"],
                "branchSiteAddress": _h1_instruction_address(
                    listing, handler, record["branchInstruction"]
                ),
                "precedingInstruction": predecessor,
                "precedingInstructionAddress": _h1_instruction_address(
                    listing, handler, predecessor
                ),
                "precedingInstructionByteCount": predecessor_width,
                "branchTarget": target,
                "targetInstructionAddress": _h1_instruction_address(
                    listing, handler, target["targetInstruction"]
                ),
            }
        )
    return result


def _direct_callbacks(
    row: dict[str, Any], guard: dict[str, Any], listing: str, addresses: dict[str, int]
) -> list[dict[str, Any]]:
    direct_calls = _closed_records(
        row["directCalls"], {"opcode", "instructionTarget"}, name=f"{row['macro']} direct call"
    )
    instructions = guard["directCallOrder"]
    if not isinstance(instructions, list) or len(instructions) != len(direct_calls):
        raise ValueError(f"entity placement H2 {row['macro']} direct-call order drift")
    call_site_addresses = _h1_ordered_direct_call_addresses(listing, row["handler"], instructions)
    callbacks = []
    for call, instruction, call_site_address in zip(
        direct_calls, instructions, call_site_addresses, strict=True
    ):
        if (
            not isinstance(instruction, str)
            or re.fullmatch(
                rf"{call['opcode']}(?:\.[bwls])? "
                rf"\(?{re.escape(call['instructionTarget'])}\)?(?:\.w)?",
                instruction,
            )
            is None
            or call["instructionTarget"] not in addresses
        ):
            raise ValueError(f"entity placement H2 {row['macro']} direct-call identity drift")
        callbacks.append(
            {
                "opcode": call["opcode"],
                "instructionTarget": call["instructionTarget"],
                "instruction": instruction,
                "callSiteAddress": call_site_address,
                "targetAddress": addresses[call["instructionTarget"]],
            }
        )
    return callbacks


def _shared_tail(
    guard: dict[str, Any], listing: str, addresses: dict[str, int]
) -> dict[str, Any] | None:
    if guard["sharedTail"] is None:
        if guard["sharedTailInstruction"] is not None:
            raise ValueError("entity placement H2 unexpected shared-tail instruction")
        return None
    tail = guard["sharedTail"]
    if set(tail) != {"targetHandler", "targetFirstInstruction", "cursorReadUseSites"}:
        raise ValueError("entity placement H2 shared-tail shape drift")
    instruction = guard["sharedTailInstruction"]
    if (
        not isinstance(instruction, str)
        or re.fullmatch(rf"bra\.w {re.escape(tail['targetHandler'])}", instruction) is None
        or tail["targetHandler"] not in addresses
    ):
        raise ValueError("entity placement H2 shared-tail target drift")
    records = _cursor_records(
        {"scriptCursorReadUseSites": tail["cursorReadUseSites"]}, name="shared-tail"
    )
    return {
        "branchInstruction": instruction,
        "branchSiteAddress": _h1_instruction_address(
            listing, "csc17_setEntityPosAndFacingWithFlash", instruction
        ),
        "targetHandler": tail["targetHandler"],
        "targetHandlerAddress": addresses[tail["targetHandler"]],
        "targetFirstInstruction": tail["targetFirstInstruction"],
        "targetFirstInstructionAddress": _h1_instruction_address(
            listing, tail["targetHandler"], tail["targetFirstInstruction"]
        ),
        "cursorReadUseSites": records,
        "cursorAdvanceByteCount": sum(row["cursorAdvanceByteCount"] for row in records),
    }


def _wait_bypass(
    h2_guard: dict[str, Any],
    callbacks: list[dict[str, Any]],
    branches: list[dict[str, Any]],
) -> dict[str, Any] | None:
    if not any(
        row["instructionTarget"] == "WaitForEntityToStopMoving"
        for row in callbacks
    ):
        return None
    literal_uses = h2_guard["sourceLiteralUseSites"]
    wait_bit = next(
        (
            row
            for row in literal_uses
            if re.fullmatch(r"btst #\$?[0-9A-Fa-f]+,d6", row["instruction"])
        ),
        None,
    )
    if wait_bit is None:
        raise ValueError("entity placement wait-bypass bit use-site drift")
    selector_copy = next(
        (
            instruction
            for instruction in h2_guard["orderedInstructions"]
            if re.fullmatch(r"move\.w d0,d6", instruction)
        ),
        None,
    )
    if selector_copy is None:
        raise ValueError("entity placement wait-bypass selector copy drift")
    selector_width = _instruction_width(selector_copy)
    bit_index = _literal_value(wait_bit["literalText"])
    mask = 1 << bit_index
    branch = next(
        (row for row in branches if row["branchInstruction"].startswith("bne.")), None
    )
    if branch is None or branch["branchTarget"]["targetInstruction"] != "rts":
        raise ValueError("entity placement wait-bypass branch relation drift")
    if mask != _sign_bit(selector_width):
        raise ValueError("entity placement wait-bypass bit/word relation drift")
    callback = next(
        row for row in callbacks if row["instructionTarget"] == "WaitForEntityToStopMoving"
    )
    return {
        "selectorCopyInstruction": selector_copy,
        "selectorWordByteCount": selector_width,
        "selectorWordSignBit": _sign_bit(selector_width),
        "testBitIndex": bit_index,
        "bypassMask": mask,
        "branchRecord": branch,
        "waitCallback": callback,
    }


def _validate_caller_breakdown(value: object, handler_names: list[str]) -> dict[str, Any]:
    if not isinstance(value, dict):
        raise ValueError("entity placement H2 caller breakdown is missing")
    required = {
        "callerHandlers",
        "targetResolutions",
        "instructionTargetTotals",
        "effectiveTargetTotals",
        "internalInstructionTargetTotals",
        "externalInstructionTargetTotals",
        "internalEffectiveTargetTotals",
        "externalEffectiveTargetTotals",
    }
    if set(value) != required:
        raise ValueError("entity placement H2 caller breakdown shape drift")
    target_order = list(value["instructionTargetTotals"])
    if not target_order or set(value["effectiveTargetTotals"]) != set(target_order):
        raise ValueError("entity placement H2 caller target domain drift")
    if [row.get("handler") for row in value["callerHandlers"]] != handler_names:
        raise ValueError("entity placement H2 caller handler order drift")
    for row in value["callerHandlers"]:
        if set(row) != {"handler", "instructionTargetSiteCounts", "effectiveTargetSiteCounts"}:
            raise ValueError("entity placement H2 caller record shape drift")
        if list(row["instructionTargetSiteCounts"]) != target_order:
            raise ValueError("entity placement H2 instruction caller target order drift")
        if list(row["effectiveTargetSiteCounts"]) != target_order:
            raise ValueError("entity placement H2 effective caller target order drift")
    for key in (
        "internalInstructionTargetTotals",
        "externalInstructionTargetTotals",
        "internalEffectiveTargetTotals",
        "externalEffectiveTargetTotals",
    ):
        if list(value[key]) != target_order:
            raise ValueError(f"entity placement H2 {key} zero-inclusive domain drift")
    return value


def _handler_record(
    row: dict[str, Any], listing: str, addresses: dict[str, int], equates: dict[str, int]
) -> dict[str, Any]:
    guard = row["sectionGuard"]
    required_guard = {
        "orderedInstructions",
        "scriptCursorReadUseSites",
        "sourceStateReads",
        "sourceStateWrites",
        "sourceConstantUses",
        "sourceLiteralUseSites",
        "branchRecords",
        "aliveStatusCursorAdjustment",
        "directCallOrder",
        "sharedTailInstruction",
        "sharedTail",
        "returnInstruction",
    }
    if set(guard) != required_guard or guard["orderedInstructions"] != row["guardedStatements"]:
        raise ValueError(f"entity placement H2 {row['macro']} section guard drift")
    cursors = _cursor_records(guard, name=row["macro"])
    reads = _state_records(guard["sourceStateReads"], equates, access="read", name=row["macro"])
    writes = _state_records(guard["sourceStateWrites"], equates, access="write", name=row["macro"])
    constants = _constant_records(guard["sourceConstantUses"], equates, name=row["macro"])
    literals = _literal_records(guard["sourceLiteralUseSites"], name=row["macro"])
    branches = _branch_records(
        guard["branchRecords"],
        guard["orderedInstructions"],
        listing,
        row["handler"],
        name=row["macro"],
    )
    if [record["branchInstruction"] for record in branches] != list(
        BRANCH_INSTRUCTION_ORDERS[row["macro"]]
    ):
        raise ValueError(f"entity placement H2 {row['macro']} branch polarity/order drift")
    callbacks = _direct_callbacks(row, guard, listing, addresses)
    shared_tail = _shared_tail(guard, listing, addresses)
    cursor_advance = sum(record["cursorAdvanceByteCount"] for record in cursors)
    if shared_tail is not None:
        cursor_advance += shared_tail["cursorAdvanceByteCount"]
    operand_byte_count = sum(item["widthBytes"] for item in row["operandAnnotations"])
    if cursor_advance != operand_byte_count:
        raise ValueError(f"entity placement H2 {row['macro']} cursor/operand relation drift")
    adjustment = guard["aliveStatusCursorAdjustment"]
    if adjustment is not None:
        if set(adjustment) != {
            "selectorPreReadInstruction",
            "adjustmentLiteralInstruction",
            "adjustmentLiteralText",
            "adjustmentLiteralValue",
            "callInstruction",
        } or adjustment["adjustmentLiteralValue"] != _literal_value(
            adjustment["adjustmentLiteralText"]
        ):
            raise ValueError(f"entity placement H2 {row['macro']} alive cursor use-site drift")
        if (
            adjustment["selectorPreReadInstruction"] != cursors[0]["instruction"]
            or adjustment["callInstruction"] not in guard["directCallOrder"]
        ):
            raise ValueError(f"entity placement H2 {row['macro']} alive cursor order drift")
    result = {
        "macro": row["macro"],
        "handler": row["handler"],
        "handlerAddress": row["address"],
        "opcode": row["opcode"],
        "sourceCommandCount": row["sourceCommandCount"],
        "operandByteCount": operand_byte_count,
        "scriptCursor": {
            "useSites": cursors,
            "selectorTransferByteCount": cursors[0]["transferredByteCount"],
            "selectorSignBit": _sign_bit(cursors[0]["transferredByteCount"]),
            "operandAdvanceByteCount": cursor_advance,
            "aliveStatusAdjustmentByteCount": (
                adjustment["adjustmentLiteralValue"] if adjustment is not None else None
            ),
        },
        "stateReads": reads,
        "stateWrites": writes,
        "mapTileSizeUseSites": constants,
        "literalUseSites": literals,
        "branchRecords": branches,
        "callbacks": callbacks,
        "sharedTail": shared_tail,
    }
    result["waitBypass"] = _wait_bypass(guard, callbacks, branches)
    return result


def _runtime_case_plan(handlers: list[dict[str, Any]]) -> list[dict[str, Any]]:
    by_macro = {row["macro"]: row for row in handlers}
    set_pos = by_macro["setPos"]
    flash = by_macro["setPosFlash"]
    facing = by_macro["setFacing"]
    destination = by_macro["setDest"]
    wait_bypass = destination["waitBypass"]
    if wait_bypass is None:
        raise ValueError("entity placement destination wait-bypass plan drift")
    delta_branches = destination["branchRecords"][:2]
    delta_widths = [branch["precedingInstructionByteCount"] for branch in delta_branches]
    if (
        len(delta_branches) != 2
        or any(not branch["precedingInstruction"].startswith("sub.w ") for branch in delta_branches)
        or len(set(delta_widths)) != 1
        or delta_widths[0] is None
    ):
        raise ValueError("entity placement destination delta branch/use-site drift")
    return [
        {
            "id": "alive-dead-cursor-consequence",
            "handlerAddresses": [set_pos["handlerAddress"], facing["handlerAddress"]],
            "sourceAdjustmentByteCounts": [
                set_pos["scriptCursor"]["aliveStatusAdjustmentByteCount"],
                facing["scriptCursor"]["aliveStatusAdjustmentByteCount"],
            ],
            "observationKeys": ["cursorAfterAliveSelector", "cursorAfterDeadSelector"],
        },
        {
            "id": "set-pos-state-scaling-facing",
            "handlerAddress": set_pos["handlerAddress"],
            "coordinateInputByteCount": set_pos["scriptCursor"]["useSites"][2][
                "transferredByteCount"
            ],
            "coordinateStoredWordByteCount": set_pos["stateWrites"][0]["transferByteCount"],
            "mapTileSize": set_pos["mapTileSizeUseSites"][0]["value"],
            "facingInputByteCount": set_pos["stateWrites"][-1]["transferByteCount"],
            "fieldOffsets": [row["fieldOffset"] for row in set_pos["stateWrites"]],
            "callbackAddress": set_pos["callbacks"][-1]["callSiteAddress"],
        },
        {
            "id": "set-facing",
            "handlerAddress": facing["handlerAddress"],
            "facingInputByteCount": facing["stateWrites"][0]["transferByteCount"],
            "facingFieldOffset": facing["stateWrites"][0]["fieldOffset"],
            "callbackAddress": facing["callbacks"][-1]["callSiteAddress"],
        },
        {
            "id": "set-pos-flash-local-timing-shared-tail",
            "handlerAddress": flash["handlerAddress"],
            "localWaitCallbackAddresses": [
                row["callSiteAddress"]
                for row in flash["callbacks"]
                if row["instructionTarget"] in {"WaitForVInt", "Sleep"}
            ],
            "literalValues": [row["value"] for row in flash["literalUseSites"]],
            "branchSiteAddresses": [row["branchSiteAddress"] for row in flash["branchRecords"]],
            "sharedTailBranchAddress": flash["sharedTail"]["branchSiteAddress"],
            "sharedTailHandlerAddress": flash["sharedTail"]["targetHandlerAddress"],
        },
        {
            "id": "set-dest-positive-negative-deltas-and-bit15-wait-bypass",
            "handlerAddress": destination["handlerAddress"],
            "coordinateInputWordByteCount": destination["scriptCursor"]["useSites"][1][
                "transferredByteCount"
            ],
            "coordinateStoredWordByteCount": destination["stateWrites"][0]["transferByteCount"],
            "deltaWordSignBit": _sign_bit(delta_widths[0]),
            "deltaBranchSiteAddresses": [
                row["branchSiteAddress"] for row in destination["branchRecords"][:2]
            ],
            "waitBypassMask": wait_bypass["bypassMask"],
            "waitBypassBranchAddress": wait_bypass["branchRecord"]["branchSiteAddress"],
            "waitCallbackAddress": wait_bypass["waitCallback"]["callSiteAddress"],
        },
    ]


def _static_source_facts(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    """Build the compact H2/H1 provenance carried into the H3 runtime matrix."""
    upstream = upstream_path.resolve(strict=True)
    h2_fixture = load_json(H2_FIXTURE)
    h2 = build_map_script_engine_contract(rom_path, upstream)
    facts = h2["entityPlacementCommandFacts"]
    handlers = facts["handlers"]
    if [(row["macro"], row["handler"]) for row in handlers] != list(HANDLER_PROFILES):
        raise ValueError("entity placement H2 handler identity/order drift")
    all_state_records = [
        state
        for row in handlers
        for state in (
            *row["sectionGuard"]["sourceStateReads"],
            *row["sectionGuard"]["sourceStateWrites"],
        )
    ]
    constants_source = "\n".join(
        (upstream / "disasm" / path).read_text(encoding="utf-8") for path in CONSTANTS_PATHS
    )
    equates = _parse_equates(constants_source, _field_symbols(all_state_records))
    listing = (upstream / H1_LISTING_PATH).read_text(encoding="utf-8")
    addresses = listing_symbol_addresses(listing)
    handler_names = [handler for _, handler in HANDLER_PROFILES]
    for row in handlers:
        if row["address"] != addresses[row["handler"]]:
            raise ValueError(f"entity placement H1 handler address drift: {row['handler']}")
    records = [_handler_record(row, listing, addresses, equates) for row in handlers]
    callback_symbols = list(facts["callerBreakdown"]["instructionTargetTotals"])
    if any(symbol not in addresses for symbol in callback_symbols):
        raise ValueError("entity placement H1 callback identity drift")
    caller_breakdown = _validate_caller_breakdown(facts["callerBreakdown"], handler_names)
    return {
        "provenance": {
            "upstreamRepository": "https://github.com/ShiningForceCentral/SF2DISASM",
            "upstreamBranch": "master",
            "upstreamCommit": h2_fixture["upstreamCommit"],
            "h2FixturePath": "tests/fixtures/h2/map-script-engine-static-v1.json",
            "h2FixtureId": h2_fixture["id"],
            "h2FieldPath": "expected.entityPlacementCommandFacts",
        },
        "romSha256": h2_fixture["romSha256"],
        "h1HandlerAddresses": [
            {"symbol": row["handler"], "address": row["handlerAddress"]} for row in records
        ],
        "h1CallbackAddresses": [
            {"symbol": symbol, "address": addresses[symbol]} for symbol in callback_symbols
        ],
        "handlers": records,
        "callerBreakdown": caller_breakdown,
        "runtimeCasePlan": _runtime_case_plan(records),
        "evidenceLabels": {
            "staticFindings": "Confirmed",
            "runtimeObservations": "Unknown",
        },
        "runtimeQuestions": facts["runtimeQuestions"],
    }


def _source_section(source: str, symbol: str) -> list[dict[str, Any]]:
    """Parse one comment-free named source section for runtime-only use sites."""
    start = re.search(rf"^{re.escape(symbol)}:\s*$", source, re.MULTILINE)
    if start is None:
        raise ValueError(f"entity placement source function is missing: {symbol}")
    end = source.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"entity placement source function end is missing: {symbol}")
    first_line = source[: start.start()].count("\n")
    records: list[dict[str, Any]] = []
    for offset, raw in enumerate(source[start.start() : end].splitlines(), 1):
        code = re.sub(r"\s+", " ", raw.split(";", 1)[0].strip())
        if not code or code.endswith(":"):
            continue
        records.append({"instruction": code, "sourceLine": first_line + offset})
    return records


def _require_ordered_source_use_sites(
    source: str, symbol: str, instructions: tuple[str, ...]
) -> list[dict[str, Any]]:
    """Link each emitted runtime relationship to its exact mutable source use site."""
    rows = _source_section(source, symbol)
    cursor = 0
    result: list[dict[str, Any]] = []
    for instruction in instructions:
        while cursor < len(rows) and rows[cursor]["instruction"] != instruction:
            cursor += 1
        if cursor == len(rows):
            raise ValueError(
                f"entity placement runtime source relation drift: {symbol}/{instruction}"
            )
        result.append(rows[cursor])
        cursor += 1
    return result


def _callback_by_target(
    handler: dict[str, Any], target: str, *, occurrence: int = 1
) -> dict[str, Any]:
    matches = [row for row in handler["callbacks"] if row["instructionTarget"] == target]
    if len(matches) < occurrence:
        raise ValueError(f"entity placement callback use-site drift: {handler['macro']}/{target}")
    return matches[occurrence - 1]


def _placement_runtime_facts(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    """Derive the H3 observer hooks from guarded H2 records plus their H1 sites."""
    upstream = upstream_path.resolve(strict=True)
    static = _static_source_facts(rom_path, upstream)
    listing = (upstream / H1_LISTING_PATH).read_text(encoding="utf-8")
    addresses = listing_symbol_addresses(listing)
    constants_source = "\n".join(
        (upstream / "disasm" / path).read_text(encoding="utf-8") for path in CONSTANTS_PATHS
    )
    constants = _parse_equates(
        constants_source,
        {
            "ENTITY_DATA",
            "ENTITY_INDEX_LIST",
            "COMBATANT_DATA",
            "ENTITYDEF_SIZE_BITS",
            "COMBATANT_OFFSET_HP_CURRENT",
            "COMBATANT_DATA_ENTRY_REAL_SIZE",
            "COMBATANT_MASK_ALL",
        },
    )
    records = {row["macro"]: row for row in static["handlers"]}
    if set(records) != {name for name, _ in HANDLER_PROFILES}:
        raise ValueError("entity placement runtime handler identity drift")
    source = (upstream / "disasm/code/common/scripting/map/mapscriptengine_1.asm").read_text(
        encoding="utf-8"
    )
    position_use_sites = _require_ordered_source_use_sites(
        source,
        "csc19_setEntityPosAndFacing",
        (
            "moveq #4,d7",
            "bsr.w AdjustScriptPointerByCharacterAliveStatus",
            "mulu.w #MAP_TILE_SIZE,d0",
            "move.w d0,(a5)",
            "move.w d0,ENTITYDEF_OFFSET_XDEST(a5)",
            "mulu.w #MAP_TILE_SIZE,d0",
            "move.w d0,ENTITYDEF_OFFSET_Y(a5)",
            "move.w d0,ENTITYDEF_OFFSET_YDEST(a5)",
            "move.b (a6)+,ENTITYDEF_OFFSET_FACING(a5)",
            "bsr.w UpdateEntitySprite_0",
        ),
    )
    facing_use_sites = _require_ordered_source_use_sites(
        source,
        "csc23_setEntityFacing",
        (
            "moveq #2,d7",
            "bsr.w AdjustScriptPointerByCharacterAliveStatus",
            "move.b (a6)+,ENTITYDEF_OFFSET_FACING(a5)",
            "bsr.w UpdateEntitySprite_0",
        ),
    )
    destination_use_sites = _require_ordered_source_use_sites(
        source,
        "csc29_setEntityDest",
        (
            "mulu.w #MAP_TILE_SIZE,d1",
            "mulu.w #MAP_TILE_SIZE,d2",
            "sub.w (a5),d1",
            "bpl.s loc_46DC4",
            "neg.w d1",
            "sub.w ENTITYDEF_OFFSET_Y(a5),d2",
            "bpl.s loc_46DDA",
            "neg.w d2",
            "btst #$F,d6",
            "bne.s return_46DEC",
            "bsr.w WaitForEntityToStopMoving",
        ),
    )
    destination_velocity_use_sites = _require_ordered_source_use_sites(
        source,
        "csc29_setEntityDest",
        (
            "move.w #32,d3",
            "neg.w d3",
            "move.w #32,d3",
            "neg.w d3",
        ),
    )
    adjustment_use_sites = _require_ordered_source_use_sites(
        source,
        "AdjustScriptPointerByCharacterAliveStatus",
        (
            "jsr j_GetCurrentHp",
            "tst.w d1",
            "bne.s @Return",
            "adda.w d7,a6",
        ),
    )
    flash_use_sites = _require_ordered_source_use_sites(
        source,
        "csc17_setEntityPosAndFacingWithFlash",
        (
            "jsr (WaitForVInt).w",
            "jsr (WaitForVInt).w",
            "jsr (Sleep).w",
            "dbf d7,loc_469BA",
            "bra.w csc19_setEntityPosAndFacing",
        ),
    )
    position = records["setPos"]
    flash = records["setPosFlash"]
    facing = records["setFacing"]
    destination = records["setDest"]
    loop_literal = next(
        row["value"]
        for row in flash["literalUseSites"]
        if row["instruction"] == "moveq #30,d7"
    )
    velocity_literals = [
        row for row in destination["literalUseSites"] if row["instruction"] == "move.w #32,d3"
    ]
    if (
        len(velocity_literals) != 2
        or velocity_literals[0]["value"] != velocity_literals[1]["value"]
        or [row["instruction"] for row in destination_velocity_use_sites]
        != ["move.w #32,d3", "neg.w d3", "move.w #32,d3", "neg.w d3"]
    ):
        raise ValueError("entity placement destination velocity use-site drift")
    velocity_magnitude = velocity_literals[0]["value"]
    velocity_width = _instruction_width(destination_velocity_use_sites[0]["instruction"])
    if any(
        _instruction_width(row["instruction"]) != velocity_width
        for row in destination_velocity_use_sites
    ):
        raise ValueError("entity placement destination velocity width use-site drift")
    velocity_negative_word = _unsigned_width(
        -velocity_magnitude, velocity_width, name="destination velocity"
    )
    destination_input_cursor_use_sites: list[dict[str, int | str]] = []
    script_input_byte_offset = 0
    for record in destination["scriptCursor"]["useSites"]:
        if (
            record["sourceRegister"] != "a6"
            or record["cursorAdvanceByteCount"] != record["transferredByteCount"]
        ):
            raise ValueError("entity placement destination script input cursor use-site drift")
        destination_input_cursor_use_sites.append(
            {
                "destinationOperand": record["destinationOperand"],
                "scriptInputByteOffset": script_input_byte_offset,
                "transferredByteCount": record["transferredByteCount"],
            }
        )
        script_input_byte_offset += record["cursorAdvanceByteCount"]
    if (
        [record["destinationOperand"] for record in destination_input_cursor_use_sites]
        != ["d0", "d1", "d2"]
        or script_input_byte_offset != destination["scriptCursor"]["operandAdvanceByteCount"]
    ):
        raise ValueError("entity placement destination script input order drift")
    delta_widths = [
        record["precedingInstructionByteCount"] for record in destination["branchRecords"][:2]
    ]
    if (
        len(delta_widths) != 2
        or not all(isinstance(width, int) for width in delta_widths)
        or delta_widths[0] != delta_widths[1]
    ):
        raise ValueError("entity placement destination delta width use-site drift")
    delta_width = delta_widths[0]
    current_hp_seed_transfer_byte_count = _instruction_width(adjustment_use_sites[1]["instruction"])
    entity_field_layouts = {
        "xWord": _entity_field_from_use_site(position["stateWrites"], "(a5)", name="x"),
        "yWord": _entity_field_from_use_site(
            position["stateWrites"], "ENTITYDEF_OFFSET_Y(a5)", name="y"
        ),
        "xVelocityWord": _entity_field_from_use_site(
            destination["stateWrites"], "ENTITYDEF_OFFSET_XVELOCITY(a5)", name="x velocity"
        ),
        "yVelocityWord": _entity_field_from_use_site(
            destination["stateWrites"], "ENTITYDEF_OFFSET_YVELOCITY(a5)", name="y velocity"
        ),
        "xTravelWord": _entity_field_from_use_site(
            destination["stateWrites"], "ENTITYDEF_OFFSET_XTRAVEL(a5)", name="x travel"
        ),
        "yTravelWord": _entity_field_from_use_site(
            destination["stateWrites"], "ENTITYDEF_OFFSET_YTRAVEL(a5)", name="y travel"
        ),
        "xDestWord": _entity_field_from_use_site(
            destination["stateWrites"], "ENTITYDEF_OFFSET_XDEST(a5)", name="x destination"
        ),
        "yDestWord": _entity_field_from_use_site(
            destination["stateWrites"], "ENTITYDEF_OFFSET_YDEST(a5)", name="y destination"
        ),
        "facingByte": _entity_field_from_use_site(
            facing["stateWrites"], "ENTITYDEF_OFFSET_FACING(a5)", name="facing"
        ),
    }
    if (
        entity_field_layouts["xDestWord"]
        != _entity_field_from_use_site(
            position["stateWrites"], "ENTITYDEF_OFFSET_XDEST(a5)", name="position x destination"
        )
        or entity_field_layouts["yDestWord"]
        != _entity_field_from_use_site(
            position["stateWrites"], "ENTITYDEF_OFFSET_YDEST(a5)", name="position y destination"
        )
        or entity_field_layouts["facingByte"]
        != _entity_field_from_use_site(
            position["stateWrites"], "ENTITYDEF_OFFSET_FACING(a5)", name="position facing"
        )
    ):
        raise ValueError("entity placement cross-handler field offset drift")
    stored_coordinate_width = entity_field_layouts["xDestWord"]["transferByteCount"]
    if (
        any(
            entity_field_layouts[field]["transferByteCount"] != stored_coordinate_width
            for field in ("xWord", "yWord", "xDestWord", "yDestWord")
        )
        or any(
            entity_field_layouts[field]["transferByteCount"] != delta_width
            for field in ("xTravelWord", "yTravelWord")
        )
        or any(
            entity_field_layouts[field]["transferByteCount"] != velocity_width
            for field in ("xVelocityWord", "yVelocityWord")
        )
        or entity_field_layouts["facingByte"]["transferByteCount"]
        != _instruction_width(facing_use_sites[2]["instruction"])
    ):
        raise ValueError("entity placement field transfer-width use-site drift")
    if flash_use_sites[3]["instruction"] != "dbf d7,loc_469BA":
        raise ValueError("entity placement flash loop relation drift")
    return {
        "function": {
            "entryAddress": addresses["RunMapSetupInitFunction"],
            "setPositionHandlerAddress": position["handlerAddress"],
            "setPositionFlashHandlerAddress": flash["handlerAddress"],
            "setFacingHandlerAddress": facing["handlerAddress"],
            "setDestinationHandlerAddress": destination["handlerAddress"],
            "setPositionAdjustCallSiteAddress": _callback_by_target(
                position, "AdjustScriptPointerByCharacterAliveStatus"
            )["callSiteAddress"],
            "setFacingAdjustCallSiteAddress": _callback_by_target(
                facing, "AdjustScriptPointerByCharacterAliveStatus"
            )["callSiteAddress"],
            "aliveStatusCursorAdjustmentAddress": _h1_instruction_address(
                listing, "AdjustScriptPointerByCharacterAliveStatus", "adda.w d7,a6"
            ),
            "setPositionGetEntityCallSiteAddress": _callback_by_target(
                position, "GetEntityAddressFromCharacter"
            )["callSiteAddress"],
            "setPositionUpdateSpriteCallSiteAddress": _callback_by_target(
                position, "UpdateEntitySprite_0"
            )["callSiteAddress"],
            "setFacingGetEntityCallSiteAddress": _callback_by_target(
                facing, "GetEntityAddressFromCharacter"
            )["callSiteAddress"],
            "setFacingUpdateSpriteCallSiteAddress": _callback_by_target(
                facing, "UpdateEntitySprite_0"
            )["callSiteAddress"],
            "setPositionFlashGetEntityCallSiteAddress": _callback_by_target(
                flash, "GetEntityAddressFromCharacter"
            )["callSiteAddress"],
            "setPositionFlashWaitForVIntCallOneAddress": _callback_by_target(
                flash, "WaitForVInt", occurrence=1
            )["callSiteAddress"],
            "setPositionFlashWaitForVIntCallTwoAddress": _callback_by_target(
                flash, "WaitForVInt", occurrence=2
            )["callSiteAddress"],
            "setPositionFlashSleepCallSiteAddress": _callback_by_target(flash, "Sleep")[
                "callSiteAddress"
            ],
            "setPositionFlashSharedTailBranchAddress": flash["sharedTail"]["branchSiteAddress"],
            "setDestinationGetEntityCallSiteAddress": _callback_by_target(
                destination, "GetEntityAddressFromCharacter"
            )["callSiteAddress"],
            "setDestinationNegativeXAddress": _h1_instruction_address(
                listing, "csc29_setEntityDest", "neg.w d1"
            ),
            "setDestinationNegativeYAddress": _h1_instruction_address(
                listing, "csc29_setEntityDest", "neg.w d2"
            ),
            "setDestinationWaitCallSiteAddress": _callback_by_target(
                destination, "WaitForEntityToStopMoving"
            )["callSiteAddress"],
        },
        "ram": {
            "entityDataAddress": constants["ENTITY_DATA"],
            "entityIndexListAddress": constants["ENTITY_INDEX_LIST"],
            "combatantDataAddress": constants["COMBATANT_DATA"],
        },
        "constants": {
            "mapTileSize": next(
                row["value"]
                for row in position["mapTileSizeUseSites"]
                if row["instruction"] == "mulu.w #MAP_TILE_SIZE,d0"
            ),
            "entityRecordByteCount": 1 << constants["ENTITYDEF_SIZE_BITS"],
            "currentHpByteOffset": constants["COMBATANT_OFFSET_HP_CURRENT"],
            "combatantEntryByteCount": constants["COMBATANT_DATA_ENTRY_REAL_SIZE"],
            "combatantMaskAll": constants["COMBATANT_MASK_ALL"],
            "setPositionDeadCursorAdjustmentByteCount": position["scriptCursor"][
                "aliveStatusAdjustmentByteCount"
            ],
            "setFacingDeadCursorAdjustmentByteCount": facing["scriptCursor"][
                "aliveStatusAdjustmentByteCount"
            ],
            "setPositionFlashLoopIterationCount": loop_literal + 1,
            "waitBypassMask": destination["waitBypass"]["bypassMask"],
            "destinationVelocityMagnitude": velocity_magnitude,
            "destinationVelocityNegativeWord": velocity_negative_word,
            "destinationVelocityTransferByteCount": velocity_width,
            "destinationDeltaTransferByteCount": delta_width,
            "destinationDeltaSignBit": _sign_bit(delta_width),
            "destinationDeltaMask": _width_mask(delta_width),
            "currentHpSeedTransferByteCount": current_hp_seed_transfer_byte_count,
            "storedCoordinateTransferByteCount": stored_coordinate_width,
            "handlerOperandAdvanceByteCounts": {
                "setPosition": position["scriptCursor"]["operandAdvanceByteCount"],
                "setPositionFlash": flash["scriptCursor"]["operandAdvanceByteCount"],
                "setFacing": facing["scriptCursor"]["operandAdvanceByteCount"],
                "setDestination": destination["scriptCursor"]["operandAdvanceByteCount"],
            },
            "entityFieldLayouts": entity_field_layouts,
            "destinationInputCursorUseSites": destination_input_cursor_use_sites,
        },
        "sourceUseSites": {
            "setPosition": position_use_sites,
            "setFacing": facing_use_sites,
            "setDestination": destination_use_sites,
            "destinationVelocity": destination_velocity_use_sites,
            "aliveStatusAdjustment": adjustment_use_sites,
            "setPositionFlash": flash_use_sites,
        },
        "sourceFacts": {
            "provenance": static["provenance"],
            "handlers": [
                {
                    "macro": row["macro"],
                    "handler": row["handler"],
                    "handlerAddress": row["handlerAddress"],
                    "opcode": row["opcode"],
                    "sourceCommandCount": row["sourceCommandCount"],
                    "operandByteCount": row["operandByteCount"],
                }
                for row in static["handlers"]
            ],
            "callerBreakdown": static["callerBreakdown"],
            "evidenceLabels": {"staticFindings": "Confirmed", "runtimeObservations": "Confirmed"},
        },
    }


# Kept as a narrow source-contract entry point while the runtime fixture is assembled.
# The final verifier below consumes the same compact object, not a copied H2 section guard.
def build_map_entity_placement_static_contract(
    rom_path: Path, upstream_path: Path
) -> dict[str, Any]:
    return _static_source_facts(rom_path, upstream_path)


def build_map_entity_placement_contract(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    """Build all static facts required by the one-launch entity-placement observation."""
    return _placement_runtime_facts(rom_path, upstream_path)


def _u16(value: int, *, name: str) -> int:
    if not isinstance(value, int) or not 0 <= value <= 0xFFFF:
        raise ValueError(f"entity placement {name} is not a word: {value}")
    return value


def _handler_operand_advance(static: dict[str, Any], handler: str) -> int:
    advances = static["constants"]["handlerOperandAdvanceByteCounts"]
    if set(advances) != {"setPosition", "setPositionFlash", "setFacing", "setDestination"}:
        raise ValueError("entity placement handler cursor domain drift")
    value = advances[handler]
    if not isinstance(value, int) or value < 1:
        raise ValueError(f"entity placement {handler} cursor advance drift")
    return value


def _position_expected(
    case: dict[str, Any], static: dict[str, Any], script_input_ram_offset: int
) -> dict[str, Any]:
    kind = case["kind"]
    if kind not in {"position", "facing", "flash"}:
        raise ValueError(f"entity placement position case kind drift: {kind}")
    state = case["entityStateSeed"]
    life_state = case["lifeState"]
    adjustment = {
        "position": static["constants"]["setPositionDeadCursorAdjustmentByteCount"],
        "facing": static["constants"]["setFacingDeadCursorAdjustmentByteCount"],
        "flash": 0,
    }[kind]
    if life_state not in {"alive", "dead"}:
        raise ValueError(f"entity placement life state drift: {case['id']}")
    if kind == "flash" and life_state != "alive":
        raise ValueError("entity placement flash coverage must keep the bounded alive path")
    start = adjustment if life_state == "dead" else 0
    operands = case["scriptBytes"]
    handler = {"position": "setPosition", "facing": "setFacing", "flash": "setPositionFlash"}[kind]
    operand_advance = _handler_operand_advance(static, handler)
    if not isinstance(operands, list) or len(operands) < operand_advance:
        raise ValueError(f"entity placement script byte corpus drift: {case['id']}")
    if any(not isinstance(value, int) or not 0 <= value <= 0xFF for value in operands):
        raise ValueError(f"entity placement script byte range drift: {case['id']}")
    selector = operands[0]
    entity_index = case["entityIndexByteSeed"]
    if (
        selector != case["selectorByte"]
        or not isinstance(entity_index, int)
        or not 0 <= entity_index <= 0xFF
    ):
        raise ValueError(f"entity placement selector/index fixture drift: {case['id']}")
    if life_state == "dead":
        if kind == "facing":
            return {
                "id": case["id"],
                "kind": kind,
                "lifeState": life_state,
                "handlerAddress": static["function"]["setFacingHandlerAddress"],
                "adjustCallSiteAddress": static["function"]["setFacingAdjustCallSiteAddress"],
                "cursorAdjustmentByteCount": adjustment,
                "scriptCursorRamOffsetAfter": script_input_ram_offset + adjustment,
                "selectorByte": selector,
                "facingByteAfter": state["facingByte"],
                "updateSpriteCallSiteAddress": None,
                "entityStateSeed": state,
            }
        return {
            "id": case["id"],
            "kind": kind,
            "lifeState": life_state,
            "handlerAddress": static["function"]["setPositionHandlerAddress"],
            "adjustCallSiteAddress": static["function"]["setPositionAdjustCallSiteAddress"],
            "cursorAdjustmentByteCount": adjustment,
            "scriptCursorRamOffsetAfter": script_input_ram_offset + adjustment,
            "selectorByte": selector,
            "xWordAfter": state["xWord"],
            "xDestWordAfter": state["xDestWord"],
            "yWordAfter": state["yWord"],
            "yDestWordAfter": state["yDestWord"],
            "facingByteAfter": state["facingByte"],
            "updateSpriteCallSiteAddress": None,
            "entityStateSeed": state,
            "flashLoopIterationCount": None,
        }
    if kind == "facing":
        facing = operands[start + 1]
        return {
            "id": case["id"],
            "kind": kind,
            "lifeState": life_state,
            "handlerAddress": static["function"]["setFacingHandlerAddress"],
            "adjustCallSiteAddress": static["function"]["setFacingAdjustCallSiteAddress"],
            "cursorAdjustmentByteCount": adjustment if life_state == "dead" else 0,
            "scriptCursorRamOffsetAfter": script_input_ram_offset + operand_advance,
            "selectorByte": selector,
            "facingByteAfter": facing,
            "updateSpriteCallSiteAddress": (
                static["function"]["setFacingUpdateSpriteCallSiteAddress"]
                if life_state == "alive"
                else None
            ),
            "entityStateSeed": state,
        }
    coordinate_width = static["constants"]["storedCoordinateTransferByteCount"]
    x = _unsigned_width(
        operands[start + 1] * static["constants"]["mapTileSize"],
        coordinate_width,
        name="scaled x",
    )
    y = _unsigned_width(
        operands[start + 2] * static["constants"]["mapTileSize"],
        coordinate_width,
        name="scaled y",
    )
    facing = operands[start + 3]
    if kind == "flash":
        handler_address = static["function"]["setPositionFlashHandlerAddress"]
        cursor_adjustment = 0
    else:
        handler_address = static["function"]["setPositionHandlerAddress"]
        cursor_adjustment = adjustment if life_state == "dead" else 0
    return {
        "id": case["id"],
        "kind": kind,
        "lifeState": life_state,
        "handlerAddress": handler_address,
        "adjustCallSiteAddress": static["function"]["setPositionAdjustCallSiteAddress"],
        "cursorAdjustmentByteCount": cursor_adjustment,
        "scriptCursorRamOffsetAfter": script_input_ram_offset + operand_advance,
        "selectorByte": selector,
        "xWordAfter": x,
        "xDestWordAfter": x,
        "yWordAfter": y,
        "yDestWordAfter": y,
        "facingByteAfter": facing,
        "updateSpriteCallSiteAddress": (
            static["function"]["setPositionUpdateSpriteCallSiteAddress"]
            if life_state == "alive"
            else None
        ),
        "entityStateSeed": state,
        "flashLoopIterationCount": (
            static["constants"]["setPositionFlashLoopIterationCount"] if kind == "flash" else None
        ),
    }


def _destination_expected(
    case: dict[str, Any], static: dict[str, Any], script_input_ram_offset: int
) -> dict[str, Any]:
    if case["kind"] != "destination":
        raise ValueError(f"entity placement destination kind drift: {case['kind']}")
    selector = _u16(case["selectorWord"], name="destination selector")
    x_input = _u16(case["xInputWord"], name="destination x")
    y_input = _u16(case["yInputWord"], name="destination y")
    state = case["entityStateSeed"]
    coordinate_width = static["constants"]["storedCoordinateTransferByteCount"]
    x_dest = _unsigned_width(
        x_input * static["constants"]["mapTileSize"], coordinate_width, name="destination scaled x"
    )
    y_dest = _unsigned_width(
        y_input * static["constants"]["mapTileSize"], coordinate_width, name="destination scaled y"
    )
    delta_width = static["constants"]["destinationDeltaTransferByteCount"]
    delta_sign_bit = static["constants"]["destinationDeltaSignBit"]
    delta_mask = static["constants"]["destinationDeltaMask"]
    if (
        delta_sign_bit != _sign_bit(delta_width)
        or delta_mask != _width_mask(delta_width)
    ):
        raise ValueError("entity placement destination delta width/sign relation drift")
    x_delta = _signed_width(
        x_dest - _u16(state["xWord"], name="seed x"), delta_width, name="x delta"
    )
    y_delta = _signed_width(
        y_dest - _u16(state["yWord"], name="seed y"), delta_width, name="y delta"
    )
    wait_bypassed = bool(selector & static["constants"]["waitBypassMask"])
    velocity_magnitude = static["constants"]["destinationVelocityMagnitude"]
    velocity_negative_word = static["constants"]["destinationVelocityNegativeWord"]
    velocity_width = static["constants"]["destinationVelocityTransferByteCount"]
    if velocity_negative_word != _unsigned_width(
        -velocity_magnitude, velocity_width, name="destination velocity"
    ):
        raise ValueError("entity placement destination velocity sign relation drift")
    return {
        "id": case["id"],
        "kind": "destination",
        "handlerAddress": static["function"]["setDestinationHandlerAddress"],
        "selectorWord": selector,
        "xInputWord": x_input,
        "yInputWord": y_input,
        "scriptCursorRamOffsetAfter": script_input_ram_offset
        + _handler_operand_advance(static, "setDestination"),
        "xDestWordAfter": x_dest,
        "yDestWordAfter": y_dest,
        "xTravelWordAfter": abs(x_delta),
        "yTravelWordAfter": abs(y_delta),
        "xVelocityWordAfter": velocity_magnitude if x_delta >= 0 else velocity_negative_word,
        "yVelocityWordAfter": velocity_magnitude if y_delta >= 0 else velocity_negative_word,
        "negativeXInstructionAddress": (
            static["function"]["setDestinationNegativeXAddress"] if x_delta < 0 else None
        ),
        "negativeYInstructionAddress": (
            static["function"]["setDestinationNegativeYAddress"] if y_delta < 0 else None
        ),
        "waitBypassed": wait_bypassed,
        "waitCallSiteAddress": (
            None if wait_bypassed else static["function"]["setDestinationWaitCallSiteAddress"]
        ),
        "entityStateSeed": state,
    }


def derive_case_expectations(
    static: dict[str, Any], fixture: dict[str, Any]
) -> list[dict[str, Any]]:
    """Derive every static result from parsed H2/H1 source facts before goldens compare."""
    script_input_ram_offset = fixture["instrumentation"].get("scriptInputRamOffset")
    if not isinstance(script_input_ram_offset, int) or script_input_ram_offset < 0:
        raise ValueError("entity placement script input RAM offset drift")
    expected = []
    for case in fixture["cases"]:
        if case["kind"] in {"position", "facing", "flash"}:
            expected.append(_position_expected(case, static, script_input_ram_offset))
        elif case["kind"] == "destination":
            expected.append(_destination_expected(case, static, script_input_ram_offset))
        else:
            raise ValueError(f"entity placement fixture case kind drift: {case['kind']}")
    if [case["expected"] for case in fixture["cases"]] != expected:
        raise ValueError("entity placement fixture/source static disagreement")
    return expected


def verify_map_entity_placement(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 120
) -> dict[str, Any]:
    """Observe the seven exact placement/facing/destination cases in one launch."""
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner="map entity placement fixture")
    verify_runtime_contract(fixture, rom_path)
    static = build_map_entity_placement_contract(rom_path, upstream_path)
    for field in ("function", "ram", "constants", "sourceUseSites", "sourceFacts"):
        if fixture[field] != static[field]:
            raise ValueError(f"entity placement fixture/source identity drift: {field}")
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
                "constants": static["constants"],
                "instrumentation": fixture["instrumentation"],
                "maxFrames": fixture["maxFrames"],
                "harness": load_json(repo_path(fixture["sharedHarnessFixture"]))["harness"],
                "cases": fixture["cases"],
                "derived": derived,
            },
            output_name="map-script-entity-placement",
            timeout_seconds=timeout_seconds,
        )

    observed = _with_instrumented_rom_database(
        instrumented_rom, "SF2 H3 instrumented map entity placement", observe
    )
    validate_json(observed, OBSERVATION_SCHEMA, owner="map entity placement observation")
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
            "map entity placement runtime matrix mismatch\n"
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
