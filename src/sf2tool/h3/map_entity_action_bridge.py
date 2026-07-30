"""Static provenance required by the entity-action bridge H3 matrix.

This deliberately carries only the H3 observation seam from the maintained H2
bridge contract.  It does not serialize the H2 handler guards wholesale.
"""

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

H2_FIXTURE = repo_path("tests/fixtures/h2/map-script-engine-static-v1.json")
FIXTURE = repo_path("tests/fixtures/h3/map-entity-action-bridge-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h3-map-entity-action-bridge-fixture.schema.json")
OBSERVATION_SCHEMA = repo_path("schemas/h3-map-entity-action-bridge-observation.schema.json")
OBSERVER = repo_path("tools/bizhawk/map_entity_action_bridge_observer.lua")
H1_LISTING_PATH = Path("build/sf2build-h1.lst")
SOURCE_PATH = Path("disasm/code/common/scripting/map/mapscriptengine_1.asm")
CONSTANT_PATHS = (Path("sf2const.asm"), Path("sf2enums.asm"))

HANDLERS = (
    ("csc15_setEntityActscript", ("setActscriptWait", "setActscript")),
    ("csc14_setEntityActscriptManual", ("customActscriptWait", "customActscript")),
    ("csc2D_entityActionSequence", ("entityActionsWait", "entityActions")),
)
_WIDTHS = {"b": 1, "w": 2, "l": 4}


def _sign_bit(width: int) -> int:
    if width not in _WIDTHS.values():
        raise ValueError(f"entity action bridge transfer width has no sign bit: {width}")
    return 1 << (width * 8 - 1)


def _unsigned_width(value: object, width: int, *, name: str) -> int:
    if not isinstance(value, int) or not 0 <= value < (1 << (width * 8)):
        raise ValueError(f"entity action bridge {name} width drift: {value}")
    return value


def _self_add_selector_scale(instruction: str) -> int:
    """Interpret the one source use-site that doubles the table selector."""
    match = re.fullmatch(r"add\.w (?P<source>d[0-7]),(?P<destination>d[0-7])", instruction)
    if match is None or match["source"] != match["destination"]:
        raise ValueError("entity action bridge dispatch selector-scale use-site drift")
    return 2


def _terminal_branch_sign_bit(instruction: str, command_width: int) -> int:
    """Bind the terminal sign bit to the source's negative-branch use site."""
    if instruction != "bmi.w loc_46928":
        raise ValueError("entity action bridge terminal negative-branch use-site drift")
    return _sign_bit(command_width)


def _instruction_width(instruction: str) -> int:
    match = re.fullmatch(r"[a-z]+\.(?P<size>[bwl])\s+.+", instruction)
    if match is None:
        raise ValueError(f"entity action bridge instruction has no transfer width: {instruction}")
    return _WIDTHS[match.group("size")]


def _literal(text: str) -> int:
    if re.fullmatch(r"\$[0-9A-Fa-f]+", text):
        return int(text[1:], 16)
    if re.fullmatch(r"-?\d+", text):
        return int(text)
    raise ValueError(f"entity action bridge source literal is not numeric: {text}")


def _parse_equates(source: str, names: set[str]) -> dict[str, int]:
    values: dict[str, int] = {}
    for name in sorted(names):
        match = re.search(
            rf"^{re.escape(name)}:\s+equ\s+(?P<value>\$[0-9A-Fa-f]+|-?\d+)\b",
            source,
            re.MULTILINE,
        )
        if match is None:
            raise ValueError(f"entity action bridge source equate is missing: {name}")
        values[name] = _literal(match.group("value"))
    return values


def _source_section(source: str, symbol: str) -> list[dict[str, Any]]:
    """Return one comment-stripped named section with stable source line provenance."""
    start = re.search(rf"^{re.escape(symbol)}:\s*$", source, re.MULTILINE)
    if start is None:
        raise ValueError(f"entity action bridge source function is missing: {symbol}")
    end = source.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"entity action bridge source function end is missing: {symbol}")
    first_line = source[: start.start()].count("\n")
    records: list[dict[str, Any]] = []
    for offset, raw in enumerate(source[start.start() : end].splitlines(), 1):
        instruction = re.sub(r"\s+", " ", raw.split(";", 1)[0].strip())
        if instruction and not instruction.endswith(":"):
            records.append({"instruction": instruction, "sourceLine": first_line + offset})
    return records


def _require_ordered_source_use_sites(
    source: str, symbol: str, instructions: tuple[str, ...]
) -> list[dict[str, Any]]:
    """Find each runtime relationship in one named function, in source order."""
    rows = _source_section(source, symbol)
    cursor = 0
    found: list[dict[str, Any]] = []
    for expected in instructions:
        while cursor < len(rows) and rows[cursor]["instruction"] != expected:
            cursor += 1
        if cursor == len(rows):
            raise ValueError(
                f"entity action bridge runtime source relation drift: {symbol}/{expected}"
            )
        found.append(rows[cursor])
        cursor += 1
    return found


def _h1_function_lines(listing: str, symbol: str) -> list[tuple[int, str]]:
    """Parse only real instructions from one H1 listing function."""
    start = re.search(rf"^[0-9A-F]{{8}}\s+{re.escape(symbol)}:\s*$", listing, re.MULTILINE)
    if start is None:
        raise ValueError(f"entity action bridge H1 function is missing: {symbol}")
    end = listing.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"entity action bridge H1 function end is missing: {symbol}")
    rows: list[tuple[int, str]] = []
    for raw in listing[start.start() : end].splitlines():
        match = re.fullmatch(r"(?P<address>[0-9A-F]{8})\s+(?P<body>.*)", raw)
        if match is None:
            continue
        body = match.group("body").split(";", 1)[0].strip()
        body = re.sub(r"^(?:[0-9A-F]{2,8}\s+)+", "", body).strip()
        if not body or body.endswith(":"):
            continue
        if re.fullmatch(r"[A-Za-z][A-Za-z0-9_]*(?:\.[bwls])?(?:\s+.+)?", body) is None:
            raise ValueError(f"entity action bridge H1 instruction parse drift: {raw}")
        rows.append((int(match.group("address"), 16), re.sub(r"\s+", "", body)))
    return rows


def _h1_instruction_address(listing: str, symbol: str, instruction: str) -> int:
    expected = re.sub(r"\s+", "", instruction)
    matches = [
        address for address, actual in _h1_function_lines(listing, symbol) if actual == expected
    ]
    if len(matches) != 1:
        raise ValueError(
            "entity action bridge H1 instruction identity drift for "
            f"{symbol}/{instruction}: {len(matches)}"
        )
    return matches[0]


def _h1_chunk_instruction_address(listing: str, label: str, instruction: str) -> int:
    """Resolve a local terminal chunk instruction kept outside the named H1 function."""
    start = re.search(rf"^[0-9A-F]{{8}}\s+{re.escape(label)}:\s*$", listing, re.MULTILINE)
    if start is None:
        raise ValueError(f"entity action bridge H1 local chunk is missing: {label}")
    end = listing.find("; END OF FUNCTION CHUNK", start.end())
    if end < 0:
        raise ValueError(f"entity action bridge H1 local chunk end is missing: {label}")
    expected = re.sub(r"\s+", "", instruction)
    matches = []
    for raw in listing[start.start() : end].splitlines():
        match = re.fullmatch(r"(?P<address>[0-9A-F]{8})\s+(?P<body>.*)", raw)
        if match is None:
            continue
        body = match.group("body").split(";", 1)[0].strip()
        body = re.sub(r"^(?:[0-9A-F]{2,8}\s+)+", "", body).strip()
        if re.sub(r"\s+", "", body) == expected:
            matches.append(int(match.group("address"), 16))
    if len(matches) != 1:
        raise ValueError(
            "entity action bridge H1 chunk instruction identity drift for "
            f"{label}/{instruction}: {len(matches)}"
        )
    return matches[0]


def _h1_ordered_call_addresses(listing: str, symbol: str, instructions: list[str]) -> list[int]:
    """Keep repeated callbacks distinct and reject comments/near-miss operands."""
    expected = [re.sub(r"\s+", "", instruction) for instruction in instructions]
    observed = [
        (address, instruction)
        for address, instruction in _h1_function_lines(listing, symbol)
        if re.match(r"(?:bsr|jsr)(?:\.[bwls])?", instruction)
    ]
    if [instruction for _, instruction in observed] != expected:
        raise ValueError(f"entity action bridge H1 direct-call order drift: {symbol}")
    return [address for address, _ in observed]


def _closed_list(value: object, required: set[str], *, name: str) -> list[dict[str, Any]]:
    if not isinstance(value, list) or not all(isinstance(row, dict) for row in value):
        raise ValueError(f"entity action bridge H2 {name} container drift")
    rows = list(value)
    if any(set(row) != required for row in rows):
        raise ValueError(f"entity action bridge H2 {name} record shape drift")
    return rows


def _validate_h2_handlers(facts: dict[str, Any]) -> list[dict[str, Any]]:
    handlers = _closed_list(
        facts.get("handlers"),
        {
            "handler",
            "address",
            "opcode",
            "macros",
            "sourceCommandCounts",
            "statementCount",
            "sectionGuard",
            "directCalls",
        },
        name="handlers",
    )
    if [(row["handler"], tuple(row["macros"])) for row in handlers] != list(HANDLERS):
        raise ValueError("entity action bridge H2 handler identity/order drift")
    for row in handlers:
        guard = row["sectionGuard"]
        required = {
            "guardedStatements",
            "cursorUseSites",
            "cursorAdvanceProfile",
            "sourceConstantUseSites",
            "branchRecords",
            "directCallOrder",
            "returnInstruction",
            "tailTransferInstruction",
            "terminalChunk",
            "tailTransferTarget",
        }
        if not isinstance(guard, dict) or set(guard) != required:
            raise ValueError(f"entity action bridge H2 guard shape drift: {row['handler']}")
        if not isinstance(row["directCalls"], list) or not isinstance(
            guard["directCallOrder"], list
        ):
            raise ValueError(f"entity action bridge H2 calls drift: {row['handler']}")
        calls = [
            f"{call['opcode']}.w {call['instructionTarget']}"
            if call["opcode"] == "bsr"
            else f"{call['opcode']} {call['instructionTarget']}(pc,d1.w)"
            for call in row["directCalls"]
        ]
        if calls != guard["directCallOrder"]:
            raise ValueError(f"entity action bridge H2 callback order drift: {row['handler']}")
    return handlers


def _bridge_macro_forms(facts: dict[str, Any]) -> list[dict[str, Any]]:
    macros = _closed_list(
        facts.get("macros"),
        {
            "name",
            "primaryMacro",
            "handler",
            "opcode",
            "primaryCommandByteCount",
            "primaryEncodedCommandByteCount",
            "primaryOperandByteCount",
            "primaryOperandLayout",
            "parameterOrdinals",
            "aliasInvocationExpressions",
            "sourceSelectorField",
            "sourceControlField",
            "sourceCommandCount",
        },
        name="macros",
    )
    expected = [macro for _, names in HANDLERS for macro in names]
    if [row["name"] for row in macros] != expected:
        raise ValueError("entity action bridge H2 macro identity/order drift")
    compact = []
    for row in macros:
        selector = row["sourceSelectorField"]
        control = row["sourceControlField"]
        if (
            set(selector) != {"streamOffset", "widthBytes", "sourceExpression"}
            or set(control) != {"streamOffset", "widthBytes", "sourceExpression", "value"}
            or selector["widthBytes"] != 1
            or control["widthBytes"] != 1
            or selector["streamOffset"] != 2
            or control["streamOffset"] != 3
            or control["value"] not in (0, 0xFF)
        ):
            raise ValueError(f"entity action bridge H2 macro control layout drift: {row['name']}")
        compact.append(
            {
                "name": row["name"],
                "handler": row["handler"],
                "opcode": row["opcode"],
                "selectorTransferByteCount": selector["widthBytes"],
                "controlTransferByteCount": control["widthBytes"],
                "controlByte": control["value"],
                "primaryOperandByteCount": row["primaryOperandByteCount"],
            }
        )
    return compact


def _cursor_record(record: dict[str, Any], *, name: str) -> dict[str, Any]:
    if set(record) != {
        "instruction",
        "operation",
        "destinationOperand",
        "transferredByteCount",
        "cursorAdvanceByteCount",
    }:
        raise ValueError(f"entity action bridge H2 cursor record shape drift: {name}")
    width = _instruction_width(record["instruction"])
    if record["operation"] == "cursor-skip":
        if record["transferredByteCount"] != 0 or record["cursorAdvanceByteCount"] < 1:
            raise ValueError(f"entity action bridge H2 cursor skip drift: {name}")
    elif record["transferredByteCount"] != width or record["cursorAdvanceByteCount"] not in (
        0,
        width,
    ):
        raise ValueError(f"entity action bridge H2 cursor transfer width drift: {name}")
    return record


def _field_layout(source_operand: str, instruction: str, equates: dict[str, int]) -> dict[str, int]:
    match = re.fullmatch(r"(?P<symbol>ENTITYDEF_OFFSET_[A-Z0-9_]+)\(a5\)", source_operand)
    if match is None or match.group("symbol") not in equates:
        raise ValueError(f"entity action bridge state operand drift: {source_operand}")
    if source_operand not in instruction:
        raise ValueError(f"entity action bridge state use-site drift: {source_operand}")
    return {
        "byteOffset": equates[match.group("symbol")],
        "transferByteCount": _instruction_width(instruction),
    }


def _handler_runtime_record(
    row: dict[str, Any], listing: str, addresses: dict[str, int]
) -> dict[str, Any]:
    guard = row["sectionGuard"]
    cursor = [_cursor_record(record, name=row["handler"]) for record in guard["cursorUseSites"]]
    call_addresses = _h1_ordered_call_addresses(listing, row["handler"], guard["directCallOrder"])
    callbacks = []
    for call, instruction, address in zip(
        row["directCalls"], guard["directCallOrder"], call_addresses, strict=True
    ):
        target = call["instructionTarget"]
        if target not in addresses:
            raise ValueError(f"entity action bridge H1 callback missing: {target}")
        callbacks.append(
            {
                "instructionTarget": target,
                "callSiteAddress": address,
                "targetAddress": addresses[target],
                "instruction": instruction,
            }
        )
    branches = _closed_list(
        guard["branchRecords"], {"branchInstruction", "branchTarget"}, name="branches"
    )
    for branch in branches:
        target = branch["branchTarget"]
        if set(target) != {"targetLabel", "targetInstruction"}:
            raise ValueError(f"entity action bridge H2 branch target shape drift: {row['handler']}")
        branch["branchSiteAddress"] = _h1_instruction_address(
            listing, row["handler"], branch["branchInstruction"]
        )
        if row["handler"] == "csc2D_entityActionSequence" and target["targetLabel"] == "loc_46928":
            branch["targetInstructionAddress"] = _h1_chunk_instruction_address(
                listing, target["targetLabel"], target["targetInstruction"]
            )
        else:
            branch["targetInstructionAddress"] = _h1_instruction_address(
                listing, row["handler"], target["targetInstruction"]
            )
    profile = guard["cursorAdvanceProfile"]
    profile_keys = {
        "primaryOperandCursorAdvanceByteCount",
        "payloadCommandReadByteCount",
        "payloadScanTransferByteCount",
        "terminatorCursorAdvanceByteCount",
    }
    if set(profile) != profile_keys or any(
        not isinstance(value, int) or value < 0 for value in profile.values()
    ):
        raise ValueError(f"entity action bridge H2 cursor profile drift: {row['handler']}")
    return {
        "handler": row["handler"],
        "handlerAddress": addresses[row["handler"]],
        "macros": row["macros"],
        "opcode": row["opcode"],
        "cursorUseSites": cursor,
        "cursorAdvanceProfile": profile,
        "branchRecords": branches,
        "callbacks": callbacks,
    }


def _same_layout(records: list[dict[str, Any]], *, name: str) -> dict[str, int]:
    layouts = [record["layout"] for record in records]
    if not layouts or any(layout != layouts[0] for layout in layouts[1:]):
        raise ValueError(f"entity action bridge independent {name} layout drift")
    return layouts[0]


def _h1_label_address(listing: str, label: str) -> int:
    match = re.search(
        rf"^(?P<address>[0-9A-F]{{8}})\s+{re.escape(label)}:\s*$", listing, re.MULTILINE
    )
    if match is None:
        raise ValueError(f"entity action bridge H1 label is missing: {label}")
    return int(match["address"], 16)


def _indexed_dispatch_targets(
    source: str, listing: str, addresses: dict[str, int]
) -> list[dict[str, Any]]:
    start = re.search(r"^rjt_EntityMoveCommands:\s*$", source, re.MULTILINE)
    if start is None:
        raise ValueError("entity action bridge indexed dispatch table is missing")
    targets = []
    first_line = source[: start.start()].count("\n")
    for offset, raw in enumerate(source[start.end() :].splitlines(), 1):
        code = raw.split(";", 1)[0].strip()
        if not code:
            continue
        match = re.fullmatch(
            r"dc\.w\s+(?P<target>[A-Za-z_][A-Za-z0-9_]*)-rjt_EntityMoveCommands",
            code,
        )
        if match is None:
            break
        target = match["target"]
        targets.append(
            {
                "index": len(targets),
                "target": target,
                "targetAddress": addresses.get(target, _h1_label_address(listing, target)),
                "sourceUseSite": {"instruction": code, "sourceLine": first_line + offset},
            }
        )
    if len(targets) != 16:
        raise ValueError("entity action bridge indexed dispatch table width drift")
    return targets


def _indexed_action_template(source: str, target: str) -> list[dict[str, Any]]:
    """Parse the bounded output shape emitted by one selected indexed table target."""
    rows = _source_section(source, target)
    template: list[dict[str, Any]] = []
    for row in rows:
        instruction = row["instruction"]
        immediate = re.fullmatch(
            r"move\.w #(?P<literal>\$[0-9A-Fa-f]+|-?\d+),\(a0\)\+", instruction
        )
        if immediate is not None:
            template.append(
                {"sourceUseSite": row, "kind": "literal", "value": _literal(immediate["literal"])}
            )
        elif instruction == "clr.w (a0)+":
            template.append({"sourceUseSite": row, "kind": "literal", "value": 0})
        elif instruction == "move.w d2,(a0)+":
            template.append({"sourceUseSite": row, "kind": "payload-byte-extended-word"})
        elif instruction != "rts":
            raise ValueError(
                f"entity action bridge indexed target layout drift: {target}/{instruction}"
            )
    if not template or rows[-1]["instruction"] != "rts":
        raise ValueError(f"entity action bridge indexed target return drift: {target}")
    return template


def _source_fields(
    source: str, equates: dict[str, int], listing: str, addresses: dict[str, int]
) -> dict[str, Any]:
    csc15 = _require_ordered_source_use_sites(
        source,
        "csc15_setEntityActscript",
        (
            "move.b (a6)+,d0",
            "move.b d0,ENTITYDEF_OFFSET_ACTSCRIPTWAITTIMER(a5)",
            "move.b (a6)+,d0",
            "move.l (a6)+,ENTITYDEF_OFFSET_ACTSCRIPTADDR(a5)",
            "tst.b d0",
        ),
    )
    csc14 = _require_ordered_source_use_sites(
        source,
        "csc14_setEntityActscriptManual",
        (
            "move.b (a6)+,d0",
            "move.b d0,ENTITYDEF_OFFSET_ACTSCRIPTWAITTIMER(a5)",
            "move.b (a6)+,d0",
            "move.l a6,ENTITYDEF_OFFSET_ACTSCRIPTADDR(a5)",
            "tst.b d0",
        ),
    )
    inline_terminators = [
        row
        for row in _source_section(source, "csc14_setEntityActscriptManual")
        if re.fullmatch(r"cmpi\.w #(?:\$[0-9A-Fa-f]+|-?\d+),\(a6\)\+", row["instruction"])
    ]
    if len(inline_terminators) != 1:
        raise ValueError("entity action bridge inline terminator source use-site drift")
    inline_terminator = inline_terminators[0]
    csc2d = _require_ordered_source_use_sites(
        source,
        "csc2D_entityActionSequence",
        (
            "move.b (a6)+,d0",
            "move.b d0,ENTITYDEF_OFFSET_ACTSCRIPTWAITTIMER(a5)",
            "andi.b #$9F,ENTITYDEF_OFFSET_FLAGS_A(a5)",
            "move.b (a6)+,d4",
            "move.b (a6)+,d1",
            "bmi.w loc_46928",
            "move.b (a6)+,d2",
            "andi.w #BYTE_LOWER_NIBBLE_MASK,d1",
            "add.w d1,d1",
            "move.w rjt_EntityMoveCommands(pc,d1.w),d1",
            "jsr rjt_EntityMoveCommands(pc,d1.w)",
        ),
    )
    terminal = _require_ordered_source_use_sites(
        source,
        "csc2D_entityActionSequence",
        (),
    )
    # The terminal lives after the routine's END marker.  Delimit it explicitly.
    chunk_start = source.find("loc_46928:")
    chunk_end = source.find("; END OF FUNCTION CHUNK FOR csc2D_entityActionSequence", chunk_start)
    if chunk_start < 0 or chunk_end < 0:
        raise ValueError("entity action bridge csc2D terminal source chunk is missing")
    chunk = source[chunk_start:chunk_end]
    base_line = source[:chunk_start].count("\n")
    terminal_rows = []
    for offset, raw in enumerate(chunk.splitlines(), 1):
        instruction = re.sub(r"\s+", " ", raw.split(";", 1)[0].strip())
        if instruction and not instruction.endswith(":"):
            terminal_rows.append({"instruction": instruction, "sourceLine": base_line + offset})
    required_terminal = (
        r"move\.w #(?:\$[0-9A-Fa-f]+|-?\d+),\(a0\)\+",
        r"move\.l #[A-Za-z_][A-Za-z0-9_]*,\(a0\)\+",
        r"addq\.l #\d+,a6",
        r"move\.l a0,\(dword_FFB1A4\)\.l",
        r"move\.l d0,ENTITYDEF_OFFSET_ACTSCRIPTADDR\(a5\)",
        r"tst\.b d4",
    )
    found_terminal = []
    cursor = 0
    for pattern in required_terminal:
        while (
            cursor < len(terminal_rows)
            and re.fullmatch(pattern, terminal_rows[cursor]["instruction"]) is None
        ):
            cursor += 1
        if cursor == len(terminal_rows):
            raise ValueError(f"entity action bridge terminal source relation drift: {pattern}")
        found_terminal.append(terminal_rows[cursor])
        cursor += 1
    # Keep normal-section parsing scoped above; the terminal lives in its own chunk.
    del terminal
    terminator_word = re.fullmatch(
        r"cmpi\.w #(?P<literal>\$[0-9A-Fa-f]+|-?\d+),\(a6\)\+",
        inline_terminator["instruction"],
    )
    if terminator_word is None:
        raise ValueError("entity action bridge inline terminator use-site drift")
    terminal_word = re.fullmatch(
        r"move\.w #(?P<literal>\$[0-9A-Fa-f]+|-?\d+),\(a0\)\+",
        found_terminal[0]["instruction"],
    )
    if terminal_word is None:
        raise ValueError("entity action bridge terminal record-word use-site drift")
    pointer_accesses = [
        {
            "handler": "csc15_setEntityActscript",
            "sourceUseSite": csc15[3],
            "layout": _field_layout(
                "ENTITYDEF_OFFSET_ACTSCRIPTADDR(a5)", csc15[3]["instruction"], equates
            ),
        },
        {
            "handler": "csc14_setEntityActscriptManual",
            "sourceUseSite": csc14[3],
            "layout": _field_layout(
                "ENTITYDEF_OFFSET_ACTSCRIPTADDR(a5)", csc14[3]["instruction"], equates
            ),
        },
        {
            "handler": "csc2D_entityActionSequence",
            "sourceUseSite": found_terminal[4],
            "layout": _field_layout(
                "ENTITYDEF_OFFSET_ACTSCRIPTADDR(a5)",
                found_terminal[4]["instruction"],
                equates,
            ),
        },
    ]
    wait_timer_accesses = [
        {
            "handler": "csc15_setEntityActscript",
            "sourceUseSite": csc15[1],
            "layout": _field_layout(
                "ENTITYDEF_OFFSET_ACTSCRIPTWAITTIMER(a5)", csc15[1]["instruction"], equates
            ),
        },
        {
            "handler": "csc14_setEntityActscriptManual",
            "sourceUseSite": csc14[1],
            "layout": _field_layout(
                "ENTITYDEF_OFFSET_ACTSCRIPTWAITTIMER(a5)", csc14[1]["instruction"], equates
            ),
        },
        {
            "handler": "csc2D_entityActionSequence",
            "sourceUseSite": csc2d[1],
            "layout": _field_layout(
                "ENTITYDEF_OFFSET_ACTSCRIPTWAITTIMER(a5)", csc2d[1]["instruction"], equates
            ),
        },
    ]
    flags = _field_layout("ENTITYDEF_OFFSET_FLAGS_A(a5)", csc2d[2]["instruction"], equates)
    flags_mask = re.fullmatch(
        r"andi\.b #(?P<literal>\$[0-9A-Fa-f]+|-?\d+),.+", csc2d[2]["instruction"]
    )
    if flags_mask is None:
        raise ValueError("entity action bridge flags-A mask source use-site drift")
    mask_match = re.fullmatch(r"andi\.w #(?P<symbol>[A-Z0-9_]+),d1", csc2d[7]["instruction"])
    if mask_match is None or mask_match["symbol"] not in equates:
        raise ValueError("entity action bridge selector-mask source use-site drift")
    return {
        "actscriptPointer": _same_layout(pointer_accesses, name="actscript-pointer"),
        "actscriptWaitTimer": _same_layout(wait_timer_accesses, name="wait-timer"),
        "flagsA": flags,
        "entityStateFieldAccesses": {
            "actscriptPointer": pointer_accesses,
            "actscriptWaitTimer": wait_timer_accesses,
        },
        "selectorTransferByteCount": _instruction_width(csc15[0]["instruction"]),
        "controlTransferByteCount": _instruction_width(csc15[2]["instruction"]),
        "pointerTransferByteCount": _instruction_width(csc15[3]["instruction"]),
        "inlineTerminatorTransferByteCount": _instruction_width(inline_terminator["instruction"]),
        "inlineTerminatorWord": _literal(terminator_word["literal"]),
        "entityActionCommandTransferByteCount": _instruction_width(csc2d[4]["instruction"]),
        "entityActionPayloadTransferByteCount": _instruction_width(csc2d[6]["instruction"]),
        "entityActionSelectorMask": equates[mask_match["symbol"]],
        "entityActionTerminalSignBit": _terminal_branch_sign_bit(
            csc2d[5]["instruction"], _instruction_width(csc2d[4]["instruction"])
        ),
        "entityActionTerminalBranch": {
            "sourceUseSite": csc2d[5],
            "branchPolarity": "negative",
            "targetLabel": "loc_46928",
            "targetAddress": _h1_label_address(listing, "loc_46928"),
        },
        "entityActionDispatchSelectorScale": _self_add_selector_scale(csc2d[8]["instruction"]),
        "entityActionDispatchTableEntryTransferByteCount": _instruction_width(
            csc2d[9]["instruction"]
        ),
        "flagsAAndMask": _literal(flags_mask["literal"]),
        "terminalRecordWord": _literal(terminal_word["literal"]),
        "terminalActionBufferRecordWordTransferByteCount": _instruction_width(
            found_terminal[0]["instruction"]
        ),
        "terminalActionBufferIdlePayloadTransferByteCount": _instruction_width(
            found_terminal[1]["instruction"]
        ),
        "terminalActionBufferPointerUpdateTransferByteCount": _instruction_width(
            found_terminal[3]["instruction"]
        ),
        "terminalEntityActscriptPointerWriteTransferByteCount": _instruction_width(
            found_terminal[4]["instruction"]
        ),
        "terminalCursorSkipByteCount": _literal(
            re.fullmatch(r"addq\.l #(?P<literal>\d+),a6", found_terminal[2]["instruction"])[
                "literal"
            ]
        ),
        "sourceUseSites": {
            "csc15": csc15,
            "csc14": csc14 + [inline_terminator],
            "csc2D": csc2d,
            "terminal": found_terminal,
        },
        "indexedDispatchTargets": _indexed_dispatch_targets(source, listing, addresses),
    }


def _validate_terminal_chunk_source_relation(
    h2_handler: dict[str, Any], source_terminal: list[dict[str, Any]]
) -> None:
    """Cross-check the terminal write sequence against the H2 guarded chunk."""
    terminal = h2_handler["sectionGuard"]["terminalChunk"]
    expected_keys = {
        "guardedStatements",
        "cursorUseSites",
        "branchRecords",
        "returnInstruction",
    }
    if not isinstance(terminal, dict) or set(terminal) != expected_keys:
        raise ValueError("entity action bridge terminal H2 guard shape drift")
    source_instructions = [record["instruction"] for record in source_terminal]
    statements = terminal["guardedStatements"]
    if (
        not isinstance(statements, list)
        or statements[: len(source_instructions)] != source_instructions
    ):
        raise ValueError("entity action bridge terminal source/H2 write order drift")


def _validate_inline_terminator_source_relation(
    h2_handler: dict[str, Any], source_use_site: dict[str, Any]
) -> None:
    """Require the inline terminator literal/order to agree with H2's guarded body."""
    statements = h2_handler["sectionGuard"]["guardedStatements"]
    if not isinstance(statements, list) or source_use_site["instruction"] not in statements:
        raise ValueError("entity action bridge inline terminator source/H2 relation drift")


def _load_map_script_source(upstream: Path) -> str:
    return (upstream / SOURCE_PATH).read_text(encoding="utf-8")


def build_map_entity_action_bridge_static_contract(
    rom_path: Path, upstream_path: Path
) -> dict[str, Any]:
    """Build compact H3 seams from H2 facts, H1 sites, and mutable source use sites."""
    upstream = upstream_path.resolve(strict=True)
    h2_fixture = load_json(H2_FIXTURE)
    h2 = build_map_script_engine_contract(rom_path, upstream)["entityActionBridgeCommandFacts"]
    handlers = _validate_h2_handlers(h2)
    macros = _bridge_macro_forms(h2)
    listing = (upstream / H1_LISTING_PATH).read_text(encoding="utf-8")
    addresses = listing_symbol_addresses(listing)
    for row in handlers:
        if row["address"] != addresses.get(row["handler"]):
            raise ValueError(f"entity action bridge H1 handler address drift: {row['handler']}")
    compact_handlers = [_handler_runtime_record(row, listing, addresses) for row in handlers]
    source = _load_map_script_source(upstream)
    constants_source = "\n".join(
        (upstream / "disasm" / path).read_text(encoding="utf-8") for path in CONSTANT_PATHS
    )
    equates = _parse_equates(
        constants_source,
        {
            "ENTITY_DATA",
            "ENTITY_INDEX_LIST",
            "ENTITYDEF_SIZE",
            "ENTITYDEF_OFFSET_ACTSCRIPTADDR",
            "ENTITYDEF_OFFSET_ACTSCRIPTWAITTIMER",
            "ENTITYDEF_OFFSET_FLAGS_A",
            "BYTE_LOWER_NIBBLE_MASK",
            "dword_FFB1A4",
        },
    )
    fields = _source_fields(source, equates, listing, addresses)
    _validate_inline_terminator_source_relation(handlers[1], fields["sourceUseSites"]["csc14"][-1])
    _validate_terminal_chunk_source_relation(handlers[2], fields["sourceUseSites"]["terminal"])
    if fields["entityActionSelectorMask"] != equates["BYTE_LOWER_NIBBLE_MASK"]:
        raise ValueError("entity action bridge selector-mask use-site relation drift")
    mask_uses = _closed_list(
        handlers[2]["sectionGuard"]["sourceConstantUseSites"],
        {"symbol", "value", "instruction"},
        name="csc2D constant uses",
    )
    mask_instruction = fields["sourceUseSites"]["csc2D"][7]["instruction"]
    if mask_uses != [
        {
            "symbol": "BYTE_LOWER_NIBBLE_MASK",
            "value": equates["BYTE_LOWER_NIBBLE_MASK"],
            "instruction": mask_instruction,
        }
    ]:
        raise ValueError("entity action bridge selector-mask H2/source use-site drift")
    terminal_skip = compact_handlers[2]["cursorUseSites"][-1]["cursorAdvanceByteCount"]
    if fields["terminalCursorSkipByteCount"] != terminal_skip:
        raise ValueError("entity action bridge terminal cursor-skip relation drift")
    terminal_branches = [
        branch
        for branch in compact_handlers[2]["branchRecords"]
        if branch["branchTarget"]["targetLabel"] == "loc_46928"
    ]
    if (
        len(terminal_branches) != 1
        or terminal_branches[0]["branchInstruction"]
        != fields["entityActionTerminalBranch"]["sourceUseSite"]["instruction"]
        or terminal_branches[0]["targetInstructionAddress"]
        != fields["entityActionTerminalBranch"]["targetAddress"]
    ):
        raise ValueError("entity action bridge terminal negative-branch relation drift")
    caller_breakdown = h2["callerBreakdown"]
    caller_keys = {
        "callerHandlers",
        "targetResolutions",
        "instructionTargetTotals",
        "effectiveTargetTotals",
        "internalInstructionTargetTotals",
        "externalInstructionTargetTotals",
        "internalEffectiveTargetTotals",
        "externalEffectiveTargetTotals",
    }
    if not isinstance(caller_breakdown, dict) or set(caller_breakdown) != caller_keys:
        raise ValueError("entity action bridge H2 caller breakdown shape drift")
    return {
        "provenance": {
            "upstreamRepository": "https://github.com/ShiningForceCentral/SF2DISASM",
            "upstreamBranch": "master",
            "upstreamCommit": h2_fixture["upstreamCommit"],
            "h2FixturePath": "tests/fixtures/h2/map-script-engine-static-v1.json",
            "h2FixtureId": h2_fixture["id"],
            "h2FieldPath": "expected.entityActionBridgeCommandFacts",
        },
        "romSha256": h2_fixture["romSha256"],
        "function": {
            "runMapSetupInitFunctionAddress": addresses["RunMapSetupInitFunction"],
            **{f"{row['handler']}Address": row["handlerAddress"] for row in compact_handlers},
            "easIdleAddress": addresses["eas_Idle"],
        },
        "ram": {
            "entityDataAddress": equates["ENTITY_DATA"],
            "entityIndexListAddress": equates["ENTITY_INDEX_LIST"],
            "entityActionBufferPointerAddress": equates["dword_FFB1A4"],
        },
        "constants": {
            "entityRecordByteCount": equates["ENTITYDEF_SIZE"],
            "entityStateFields": {
                key: fields[key] for key in ("actscriptPointer", "actscriptWaitTimer", "flagsA")
            },
            **{
                key: value
                for key, value in fields.items()
                if key not in {"sourceUseSites", "indexedDispatchTargets"}
                and key not in {"actscriptPointer", "actscriptWaitTimer", "flagsA"}
            },
        },
        "sourceFacts": {
            "macroForms": macros,
            "handlers": compact_handlers,
            "callerBreakdown": caller_breakdown,
            "sourceUseSites": fields["sourceUseSites"],
            "indexedDispatchTargets": fields["indexedDispatchTargets"],
            "evidenceLabels": {"staticFindings": "Confirmed", "runtimeObservations": "Unknown"},
        },
        "runtimeQuestions": h2["runtimeQuestions"],
    }


def _handler_by_name(static: dict[str, Any], handler: str) -> dict[str, Any]:
    matches = [row for row in static["sourceFacts"]["handlers"] if row["handler"] == handler]
    if len(matches) != 1:
        raise ValueError(f"entity action bridge handler seam drift: {handler}")
    return matches[0]


def _callback_by_target(handler: dict[str, Any], target: str) -> dict[str, Any]:
    matches = [row for row in handler["callbacks"] if row["instructionTarget"] == target]
    if len(matches) != 1:
        raise ValueError(f"entity action bridge callback seam drift: {handler['handler']}/{target}")
    return matches[0]


def _branch_by_target(handler: dict[str, Any], target: str) -> dict[str, Any]:
    matches = [
        row for row in handler["branchRecords"] if row["branchTarget"]["targetLabel"] == target
    ]
    if len(matches) != 1:
        raise ValueError(f"entity action bridge branch seam drift: {handler['handler']}/{target}")
    return matches[0]


def build_map_entity_action_bridge_contract(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    """Add runtime hook addresses without copying handler guards into the H3 contract."""
    static = build_map_entity_action_bridge_static_contract(rom_path, upstream_path)
    listing = (upstream_path.resolve(strict=True) / H1_LISTING_PATH).read_text(encoding="utf-8")
    csc15 = _handler_by_name(static, "csc15_setEntityActscript")
    csc14 = _handler_by_name(static, "csc14_setEntityActscriptManual")
    csc2d = _handler_by_name(static, "csc2D_entityActionSequence")
    return {
        **static,
        "function": {
            **static["function"],
            "csc15GetEntityCallSiteAddress": _callback_by_target(
                csc15, "GetEntityAddressFromCharacter"
            )["callSiteAddress"],
            "csc14GetEntityCallSiteAddress": _callback_by_target(
                csc14, "GetEntityAddressFromCharacter"
            )["callSiteAddress"],
            "csc2DGetEntityCallSiteAddress": _callback_by_target(
                csc2d, "GetEntityAddressFromCharacter"
            )["callSiteAddress"],
            "csc2DIndexedCallSiteAddress": _callback_by_target(csc2d, "rjt_EntityMoveCommands")[
                "callSiteAddress"
            ],
            "csc15WaitCompareAddress": _branch_by_target(csc15, "loc_4698E")[
                "targetInstructionAddress"
            ],
            "csc15WaitBackEdgeAddress": _branch_by_target(csc15, "loc_4698E")["branchSiteAddress"],
            "csc14WaitCompareAddress": _branch_by_target(csc14, "loc_46966")[
                "targetInstructionAddress"
            ],
            "csc14WaitBackEdgeAddress": _branch_by_target(csc14, "loc_46966")["branchSiteAddress"],
            "csc14InlineTerminatorCompareAddress": _h1_instruction_address(
                listing,
                "csc14_setEntityActscriptManual",
                "cmpi.w #$8080,(a6)+",
            ),
            "csc2DTerminalEntryAddress": _branch_by_target(csc2d, "loc_46928")[
                "targetInstructionAddress"
            ],
            "csc2DTerminalPayloadAfterWriteAddress": _h1_chunk_instruction_address(
                listing, "loc_46928", "addq.l #1,a6"
            ),
            "csc2DTerminalWaitCompareAddress": _h1_chunk_instruction_address(
                listing,
                "loc_46944",
                "cmpi.l #eas_Idle,ENTITYDEF_OFFSET_ACTSCRIPTADDR(a5)",
            ),
            "csc2DTerminalWaitBackEdgeAddress": _h1_chunk_instruction_address(
                listing,
                "loc_46944",
                "bne.s loc_46944",
            ),
        },
    }


def _case_pointer_value(
    case: dict[str, Any], static: dict[str, Any], fixture: dict[str, Any]
) -> int:
    width = static["constants"]["entityStateFields"]["actscriptPointer"]["transferByteCount"]
    kind = case.get("pointerInputKind")
    if kind == "eas-idle":
        return _unsigned_width(static["function"]["easIdleAddress"], width, name="eas-idle pointer")
    if kind == "session-action-buffer":
        pointer = fixture["instrumentation"]["sessionActionBufferAddress"]
        return _unsigned_width(pointer, width, name="session action-buffer pointer")
    raise ValueError(f"entity action bridge pointer input kind drift: {kind}")


def _case_template_words(
    case: dict[str, Any], static: dict[str, Any], upstream_path: Path
) -> tuple[str, int, list[int], int, int]:
    action_byte = _unsigned_width(
        case.get("actionCommandByte"),
        static["constants"]["entityActionCommandTransferByteCount"],
        name="action command",
    )
    payload_byte = _unsigned_width(
        case.get("actionPayloadByte"),
        static["constants"]["entityActionPayloadTransferByteCount"],
        name="action payload",
    )
    index = action_byte & static["constants"]["entityActionSelectorMask"]
    targets = static["sourceFacts"]["indexedDispatchTargets"]
    if not isinstance(targets, list) or not 0 <= index < len(targets):
        raise ValueError("entity action bridge indexed selector/table relation drift")
    selected = targets[index]
    if set(selected) != {"index", "target", "targetAddress", "sourceUseSite"}:
        raise ValueError("entity action bridge indexed target record shape drift")
    target = selected["target"]
    if target != case.get("indexedActionTarget") or selected["targetAddress"] != case.get(
        "indexedActionTargetAddress"
    ):
        raise ValueError("entity action bridge indexed target fixture/source drift")
    source = (upstream_path.resolve(strict=True) / SOURCE_PATH).read_text(encoding="utf-8")
    template = _indexed_action_template(source, target)
    words: list[int] = []
    word_widths: list[int] = []
    written_byte_count = 0
    for record in template:
        transfer_width = _instruction_width(record["sourceUseSite"]["instruction"])
        written_byte_count += transfer_width
        word_widths.append(transfer_width)
        words.append(
            payload_byte if record["kind"] == "payload-byte-extended-word" else record["value"]
        )
    if not word_widths or any(width != word_widths[0] for width in word_widths[1:]):
        raise ValueError("entity action bridge indexed action-buffer word width drift")
    return target, selected["targetAddress"], words, written_byte_count, word_widths[0]


def derive_case_expectations(
    static: dict[str, Any], fixture: dict[str, Any], upstream_path: Path
) -> list[dict[str, Any]]:
    """Derive all six results from parsed cursor/use-site records before golden comparison."""
    forms = static["sourceFacts"]["macroForms"]
    if [case.get("macro") for case in fixture["cases"]] != [row["name"] for row in forms]:
        raise ValueError("entity action bridge six-form case order drift")
    script_offset = fixture["instrumentation"].get("scriptInputRamOffset")
    if not isinstance(script_offset, int) or script_offset < 0:
        raise ValueError("entity action bridge script input offset drift")
    records = []
    for case, form in zip(fixture["cases"], forms, strict=True):
        if (
            case.get("handler") != form["handler"]
            or case.get("controlByte") != form["controlByte"]
            or _unsigned_width(
                case.get("selectorByte"),
                static["constants"]["selectorTransferByteCount"],
                name="selector",
            )
            != case["selectorByte"]
        ):
            raise ValueError(f"entity action bridge case source form drift: {case.get('id')}")
        handler = _handler_by_name(static, form["handler"])
        profile = handler["cursorAdvanceProfile"]
        fields = static["constants"]["entityStateFields"]
        wait_hook = {
            "csc15_setEntityActscript": static["function"]["csc15WaitCompareAddress"],
            "csc14_setEntityActscriptManual": static["function"]["csc14WaitCompareAddress"],
            "csc2D_entityActionSequence": static["function"]["csc2DTerminalWaitCompareAddress"],
        }[form["handler"]]
        wait_back_edge = {
            "csc15_setEntityActscript": static["function"]["csc15WaitBackEdgeAddress"],
            "csc14_setEntityActscriptManual": static["function"]["csc14WaitBackEdgeAddress"],
            "csc2D_entityActionSequence": static["function"]["csc2DTerminalWaitBackEdgeAddress"],
        }[form["handler"]]
        injection = (
            {
                "programCounterAddress": wait_hook,
                "backEdgeInstructionAddress": wait_back_edge,
                "field": "actscriptPointer",
                "value": static["function"]["easIdleAddress"],
                "afterCompareEntryCount": 2,
            }
            if form["controlByte"]
            else None
        )
        if case.get("waitLoopExitInjection") != injection:
            raise ValueError(f"entity action bridge wait-loop injection drift: {case.get('id')}")
        expected = {
            "id": case["id"],
            "macro": form["name"],
            "handlerAddress": handler["handlerAddress"],
            "selectorByte": case["selectorByte"],
            "controlByte": form["controlByte"],
            "actscriptWaitTimerByteAfter": _unsigned_width(
                case["entityIndexByteSeed"],
                fields["actscriptWaitTimer"]["transferByteCount"],
                name="resolved entity index",
            ),
            "actscriptWaitTimerTransferByteCount": fields["actscriptWaitTimer"][
                "transferByteCount"
            ],
            "actscriptPointerTransferByteCount": fields["actscriptPointer"]["transferByteCount"],
            "getEntityCallSiteAddress": _callback_by_target(
                handler, "GetEntityAddressFromCharacter"
            )["callSiteAddress"],
            "waitCompareEntryCount": 2 if form["controlByte"] else 0,
            "waitLoopExitInjection": injection,
        }
        if form["handler"] == "csc15_setEntityActscript":
            pointer = _case_pointer_value(case, static, fixture)
            expected.update(
                {
                    "scriptCursorRamOffsetAfter": script_offset
                    + profile["primaryOperandCursorAdvanceByteCount"],
                    "actscriptPointerLongAfter": static["function"]["easIdleAddress"]
                    if form["controlByte"]
                    else pointer,
                    "inlineTerminatorObserved": False,
                    "indexedCallbackObserved": False,
                    "terminalObserved": False,
                    "flagsAByteAfter": case["entityStateSeed"]["flagsAByte"],
                    "actionBufferWords": [],
                    "actionBufferPointerLongAfter": case["actionBufferPointerLongSeed"],
                }
            )
        elif form["handler"] == "csc14_setEntityActscriptManual":
            terminator = _unsigned_width(
                case.get("inlineTerminatorWord"),
                static["constants"]["inlineTerminatorTransferByteCount"],
                name="inline terminator",
            )
            if terminator != static["constants"]["inlineTerminatorWord"]:
                raise ValueError("entity action bridge inline terminator fixture/source drift")
            inline_pointer = (
                fixture["instrumentation"]["ramInputAddress"]
                + script_offset
                + profile["primaryOperandCursorAdvanceByteCount"]
            )
            expected.update(
                {
                    "scriptCursorRamOffsetAfter": script_offset
                    + profile["primaryOperandCursorAdvanceByteCount"]
                    + profile["terminatorCursorAdvanceByteCount"],
                    "actscriptPointerLongAfter": static["function"]["easIdleAddress"]
                    if form["controlByte"]
                    else inline_pointer,
                    "inlineTerminatorObserved": True,
                    "indexedCallbackObserved": False,
                    "terminalObserved": False,
                    "flagsAByteAfter": case["entityStateSeed"]["flagsAByte"],
                    "actionBufferWords": [],
                    "actionBufferPointerLongAfter": case["actionBufferPointerLongSeed"],
                }
            )
        else:
            target, target_address, words, written_byte_count, word_transfer_byte_count = (
                _case_template_words(case, static, upstream_path)
            )
            pointer = _unsigned_width(
                case["actionBufferPointerLongSeed"],
                fields["actscriptPointer"]["transferByteCount"],
                name="action-buffer base pointer",
            )
            if _case_pointer_value(case, static, fixture) != pointer:
                raise ValueError("entity action bridge session action-buffer pointer/seed drift")
            terminal_command = _unsigned_width(
                case.get("terminalCommandByte"),
                static["constants"]["entityActionCommandTransferByteCount"],
                name="entity action terminal command",
            )
            terminal_skip = _unsigned_width(
                case.get("terminalSkippedByte"),
                static["constants"]["entityActionPayloadTransferByteCount"],
                name="entity action terminal skip",
            )
            if (
                terminal_command != static["constants"]["entityActionTerminalSignBit"]
                or terminal_skip != 0
            ):
                raise ValueError("entity action bridge high-bit terminal fixture/source drift")
            terminal_bytes = (
                static["constants"]["terminalActionBufferRecordWordTransferByteCount"]
                + static["constants"]["terminalActionBufferIdlePayloadTransferByteCount"]
            )
            flags_width = fields["flagsA"]["transferByteCount"]
            flags = _unsigned_width(
                case["entityStateSeed"]["flagsAByte"], flags_width, name="flags-a seed"
            ) & _unsigned_width(case["flagsAAndMask"], flags_width, name="flags-a mask")
            if case["flagsAAndMask"] != static["constants"]["flagsAAndMask"]:
                raise ValueError("entity action bridge flags-A mask fixture/source drift")
            expected.update(
                {
                    "scriptCursorRamOffsetAfter": script_offset
                    + profile["primaryOperandCursorAdvanceByteCount"]
                    + profile["payloadCommandReadByteCount"]
                    + profile["terminatorCursorAdvanceByteCount"],
                    "actscriptPointerLongAfter": static["function"]["easIdleAddress"]
                    if form["controlByte"]
                    else pointer,
                    "inlineTerminatorObserved": False,
                    "indexedCallbackObserved": True,
                    "terminalObserved": True,
                    "flagsAByteAfter": flags,
                    "actionBufferWords": words + [static["constants"]["terminalRecordWord"]],
                    "indexedActionBufferByteCount": written_byte_count,
                    "indexedActionBufferWordTransferByteCount": word_transfer_byte_count,
                    "terminalActionBufferIdlePayloadLong": static["function"]["easIdleAddress"],
                    "terminalActionBufferIdlePayloadTransferByteCount": static["constants"][
                        "terminalActionBufferIdlePayloadTransferByteCount"
                    ],
                    "actionBufferPointerLongAfter": pointer + written_byte_count + terminal_bytes,
                    "indexedActionTarget": target,
                    "indexedActionTargetAddress": target_address,
                    "terminalCommandByte": terminal_command,
                    "terminalSkippedByte": terminal_skip,
                }
            )
        if "expected" in case and case["expected"] != expected:
            raise ValueError(f"entity action bridge expected result drift: {case.get('id')}")
        records.append(expected)
    return records


def verify_map_entity_action_bridge(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 120
) -> dict[str, Any]:
    """Observe one exact case per source alias in one session-only instrumented launch."""
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner="map entity action bridge fixture")
    verify_runtime_contract(fixture, rom_path)
    static = build_map_entity_action_bridge_contract(rom_path, upstream_path)
    for key in ("function", "ram", "constants", "sourceFacts"):
        if fixture[key] != static[key]:
            raise ValueError(f"entity action bridge fixture/source identity drift: {key}")
    derived = derive_case_expectations(static, fixture, upstream_path)
    indexed_target_addresses = {
        row["indexedActionTargetAddress"]
        for row in derived
        if row["handlerAddress"] == static["function"]["csc2D_entityActionSequenceAddress"]
    }
    if len(indexed_target_addresses) != 1:
        raise ValueError("entity action bridge indexed target runtime-hook domain drift")
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
                "indexedTargetAddress": indexed_target_addresses.pop(),
            },
            output_name="map-entity-action-bridge",
            timeout_seconds=timeout_seconds,
        )

    observed = _with_instrumented_rom_database(
        instrumented_rom, "SF2 H3 map entity action bridge", observe
    )
    validate_json(observed, OBSERVATION_SCHEMA, owner="map entity action bridge observation")
    expected = {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["id"],
        "mapTest": fixture["mapTestIndex"],
        "recordOrder": [case["id"] for case in fixture["cases"]],
        "records": [
            {**record, **case["runtimeGolden"]}
            for record, case in zip(derived, fixture["cases"], strict=True)
        ],
    }
    if observed != expected:
        raise ValueError(
            "entity action bridge runtime observation mismatch:\n"
            f"expected={expected!r}\nobserved={observed!r}"
        )
    return {
        "Fixture": fixture["id"],
        "Cases": len(fixture["cases"]),
        "Handlers": len(static["sourceFacts"]["handlers"]),
        "BizHawkLaunches": 1,
        "Instrumentation": "session-only",
        "Status": "PASS",
    }
