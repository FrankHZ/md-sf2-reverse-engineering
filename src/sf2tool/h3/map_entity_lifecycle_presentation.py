"""Static provenance for the map-script entity lifecycle/presentation H3 matrix.

The runtime rail deliberately observes only bounded handler/callback/state seams.
Source macro labels and field names remain source labels; they do not establish
player-visible lifecycle or presentation semantics.
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
FIXTURE = repo_path("tests/fixtures/h3/map-entity-lifecycle-presentation-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h3-map-entity-lifecycle-presentation-fixture.schema.json")
OBSERVATION_SCHEMA = repo_path(
    "schemas/h3-map-entity-lifecycle-presentation-observation.schema.json"
)
OBSERVER = repo_path("tools/bizhawk/map_entity_lifecycle_presentation_observer.lua")
H1_LISTING_PATH = Path("build/sf2build-h1.lst")
SOURCE_PATH = Path("disasm/code/common/scripting/map/mapscriptengine_1.asm")
ENUMS_PATH = Path("disasm/sf2enums.asm")
CONSTANTS_PATH = Path("disasm/sf2const.asm")
CURRENT_HP_SOURCE_PATH = Path("disasm/code/common/stats/combatantstats_1.asm")
COMBATANT_WORD_SOURCE_PATH = Path("disasm/code/common/stats/combatantstats_3.asm")
MAP_FUNCTIONS_SOURCE_PATH = Path("disasm/code/common/scripting/map/mapfunctions.asm")

HANDLER_FORMS = (
    ("hide", "csc2E_hideEntity"),
    ("startEntity", "csc1B_startEntityAnim"),
    ("stopEntity", "csc1C_stopEntityAnim"),
    ("waitIdle", "csc16_waitUntilEntityIdle"),
    ("setSprite", "csc1A_setEntitySprite"),
    ("setPriority", "csc53_setPriority"),
    ("removeShadow", "csc30_removeEntityShadow"),
    ("setSize", "csc50_setEntitySize"),
)
WIDTHS = {"b": 1, "w": 2, "l": 4}
CONTROL_SECTION_INSTRUCTIONS = {
    "csc2E_hideEntity": (
        "move.w (a6)+,d0",
        "bsr.w GetEntityAddressFromCharacter",
        "jsr HideEntity",
        "rts",
    ),
    "csc1B_startEntityAnim": (
        "move.w (a6),d0",
        "moveq #2,d7",
        "bsr.w AdjustScriptPointerByCharacterAliveStatus",
        "move.w (a6)+,d0",
        "bsr.w GetEntityAddressFromCharacter",
        "move.b #0,ENTITYDEF_OFFSET_ANIMCOUNTER(a5)",
        "rts",
    ),
    "csc1C_stopEntityAnim": (
        "move.w (a6),d0",
        "moveq #2,d7",
        "bsr.w AdjustScriptPointerByCharacterAliveStatus",
        "move.w (a6)+,d0",
        "bsr.w GetEntityAddressFromCharacter",
        "move.b #-1,ENTITYDEF_OFFSET_ANIMCOUNTER(a5)",
        "rts",
    ),
    "csc16_waitUntilEntityIdle": (
        "move.w (a6)+,d0",
        "bsr.w GetEntityAddressFromCharacter",
        "cmpi.l #eas_Idle,ENTITYDEF_OFFSET_ACTSCRIPTADDR(a5)",
        "bne.s loc_469A0",
        "rts",
    ),
    "csc1A_setEntitySprite": (
        "move.w (a6)+,d0",
        "bsr.w GetEntityAddressFromCharacter",
        "move.w (a6)+,d0",
        "cmpi.w #COMBATANT_ALLIES_NUMBER,d0",
        "bcc.s @NotAlly",
        "jsr GetAllyMapsprite",
        "move.w d4,d0",
        "move.b d0,ENTITYDEF_OFFSET_MAPSPRITE(a5)",
        "jsr (WaitForVInt).w",
        "bsr.w UpdateEntitySprite_0",
        "rts",
    ),
    "csc53_setPriority": (
        "move.w (a6)+,d0",
        "bsr.w GetEntityAddressFromCharacter",
        "lea ((byte_FFAFB0-$1000000)).w,a0",
        "nop",
        "move.w (a6)+,d1",
        "bne.s loc_46FD4",
        "clr.b (a0,d0.w)",
        "bra.s return_46FDA",
        "move.b #1,(a0,d0.w)",
        "rts",
    ),
    "csc30_removeEntityShadow": (
        "move.w (a6)+,d0",
        "bsr.w GetEntityAddressFromCharacter",
        "lea (FF6802_LOADING_SPACE).l,a0",
        "bsr.w LoadMapsprite",
        "jsr sub_45A8C",
        "bsr.w DmaMapsprite",
        "jsr (WaitForVInt).w",
        "rts",
    ),
    "csc50_setEntitySize": (
        "move.w (a6)+,d0",
        "bsr.w GetEntityAddressFromCharacter",
        "move.w ((SPRITE_SIZE-$1000000)).w,d6",
        "move.w (a6)+,((SPRITE_SIZE-$1000000)).w",
        "ori.b #%1000,ENTITYDEF_OFFSET_FLAGS_B(a5)",
        "bsr.w UpdateEntitySprite_0",
        "jsr (WaitForVInt).w",
        "move.w d6,((SPRITE_SIZE-$1000000)).w",
        "rts",
    ),
}


def _literal(text: str) -> int:
    if re.fullmatch(r"\$[0-9A-Fa-f]+", text):
        return int(text[1:], 16)
    if re.fullmatch(r"%[01]+", text):
        return int(text[1:], 2)
    if re.fullmatch(r"-?\d+", text):
        return int(text)
    raise ValueError(f"entity lifecycle source literal is not numeric: {text}")


def _instruction_width(instruction: str) -> int:
    match = re.fullmatch(r"[a-z]+\.(?P<size>[bwl])\s+.+", instruction)
    if match is None:
        raise ValueError(f"entity lifecycle instruction has no transfer width: {instruction}")
    return WIDTHS[match.group("size")]


def _parse_equates(source: str, names: set[str]) -> dict[str, int]:
    values: dict[str, int] = {}
    for name in sorted(names):
        match = re.search(
            rf"^{re.escape(name)}:\s+equ\s+(?P<value>\$[0-9A-Fa-f]+|%[01]+|-?\d+)\b",
            source,
            re.MULTILINE,
        )
        if match is None:
            raise ValueError(f"entity lifecycle source equate is missing: {name}")
        values[name] = _literal(match.group("value"))
    return values


def _source_section(
    source: str, symbol: str, *, end_marker: str | None = None
) -> list[dict[str, Any]]:
    start = re.search(rf"^{re.escape(symbol)}:\s*$", source, re.MULTILINE)
    if start is None:
        raise ValueError(f"entity lifecycle source function is missing: {symbol}")
    end = source.find(end_marker or f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"entity lifecycle source function end is missing: {symbol}")
    first_line = source[: start.start()].count("\n")
    rows: list[dict[str, Any]] = []
    for offset, raw in enumerate(source[start.start() : end].splitlines(), 1):
        instruction = re.sub(r"\s+", " ", raw.split(";", 1)[0].strip())
        if instruction and not instruction.endswith(":"):
            rows.append({"instruction": instruction, "sourceLine": first_line + offset})
    return rows


def _immediate_sprite_size_write(
    rows: list[dict[str, Any]], *, source_file: Path, section: str
) -> dict[str, Any]:
    pattern = r"move\.w #(?P<value>\d+),\(\(SPRITE_SIZE-\$1000000\)\)\.w"
    matches = [
        row
        for row in rows
        if re.fullmatch(pattern, row["instruction"])
    ]
    if len(matches) != 1:
        raise ValueError(f"entity lifecycle sprite-size source write drift: {section}")
    row = matches[0]
    literal = re.fullmatch(pattern, row["instruction"])
    if literal is None:
        raise ValueError(f"entity lifecycle sprite-size source literal drift: {section}")
    return {
        "sourceFile": source_file.as_posix(),
        "section": section,
        "sourceLine": row["sourceLine"],
        "instruction": row["instruction"],
        "value": _literal(literal["value"]),
    }


def _h1_function_lines(listing: str, symbol: str) -> list[tuple[int, str]]:
    start = re.search(rf"^[0-9A-F]{{8}}\s+{re.escape(symbol)}:\s*$", listing, re.MULTILINE)
    if start is None:
        raise ValueError(f"entity lifecycle H1 function is missing: {symbol}")
    end = listing.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"entity lifecycle H1 function end is missing: {symbol}")
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
            raise ValueError(f"entity lifecycle H1 instruction parse drift: {raw}")
        rows.append((int(match.group("address"), 16), re.sub(r"\s+", "", body)))
    return rows


def _h1_instruction_address(listing: str, symbol: str, instruction: str) -> int:
    expected = re.sub(r"\s+", "", instruction)
    matches = [
        address for address, actual in _h1_function_lines(listing, symbol) if actual == expected
    ]
    if len(matches) != 1:
        raise ValueError(
            "entity lifecycle H1 instruction identity drift: "
            f"{symbol}/{instruction}: {len(matches)}"
        )
    return matches[0]


def _closed_rows(value: object, required: set[str], *, name: str) -> list[dict[str, Any]]:
    if not isinstance(value, list) or not all(isinstance(row, dict) for row in value):
        raise ValueError(f"entity lifecycle H2 {name} container drift")
    rows = list(value)
    if any(set(row) != required for row in rows):
        raise ValueError(f"entity lifecycle H2 {name} record shape drift")
    return rows


def _field_layout(operand: str, instruction: str, equates: dict[str, int]) -> dict[str, int]:
    match = re.fullmatch(r"(?P<symbol>ENTITYDEF_OFFSET_[A-Z0-9_]+)\(a5\)", operand)
    if match is None or match["symbol"] not in equates:
        raise ValueError(f"entity lifecycle entity-field use-site drift: {operand}")
    return {
        "byteOffset": equates[match["symbol"]],
        "transferByteCount": _instruction_width(instruction),
    }


def _state_rows(
    records: object, equates: dict[str, int], *, handler: str, access: str
) -> list[dict[str, Any]]:
    rows = _closed_rows(records, {"sourceOperand", "instruction"}, name=f"{handler} {access}")
    result: list[dict[str, Any]] = []
    for row in rows:
        if row["sourceOperand"] not in row["instruction"]:
            raise ValueError(f"entity lifecycle {handler} {access} source use-site drift")
        result.append(
            {
                **row,
                "transferByteCount": _instruction_width(row["instruction"]),
                "entityFieldLayout": _field_layout(
                    row["sourceOperand"], row["instruction"], equates
                )
                if row["sourceOperand"].endswith("(a5)")
                else None,
            }
        )
    return result


def _cursor_rows(guard: dict[str, Any], *, handler: str) -> list[dict[str, Any]]:
    rows = _closed_rows(
        guard["scriptCursorReadUseSites"],
        {
            "sourceRegister",
            "destinationOperand",
            "transferredByteCount",
            "cursorAdvanceByteCount",
            "instruction",
        },
        name=f"{handler} cursor reads",
    )
    for row in rows:
        width = _instruction_width(row["instruction"])
        if (
            row["sourceRegister"] != "a6"
            or row["transferredByteCount"] != width
            or row["cursorAdvanceByteCount"] not in (0, width)
        ):
            raise ValueError(f"entity lifecycle {handler} cursor use-site drift")
    return rows


def _source_use_site(
    rows: list[dict[str, Any]], instruction: str, *, handler: str
) -> dict[str, Any]:
    matches = [row for row in rows if row["instruction"] == instruction]
    if len(matches) != 1:
        raise ValueError(
            f"entity lifecycle {handler} source use-site identity drift: {instruction}"
        )
    return matches[0]


def _handler_record(
    h2: dict[str, Any],
    source: str,
    listing: str,
    addresses: dict[str, int],
    equates: dict[str, int],
) -> dict[str, Any]:
    handler = h2["handler"]
    guard = h2["sectionGuard"]
    required_guard = {
        "orderedInstructions",
        "scriptCursorReadUseSites",
        "aliveStatusPointerAdjustment",
        "stateReads",
        "stateWrites",
        "addressSetupUseSites",
        "bitMutationUseSites",
        "sourceConstantUseSites",
        "literalUseSites",
        "branchRecords",
        "directCallOrder",
        "fallthroughInstruction",
        "returnInstruction",
    }
    if set(guard) != required_guard:
        raise ValueError(f"entity lifecycle H2 guard shape drift: {handler}")
    source_rows = _source_section(source, handler)
    source_instructions = [row["instruction"] for row in source_rows]
    if source_instructions != guard["orderedInstructions"]:
        raise ValueError(f"entity lifecycle guarded source order drift: {handler}")
    h1_instructions = [instruction for _, instruction in _h1_function_lines(listing, handler)]
    expected_h1 = [re.sub(r"\s+", "", item) for item in source_instructions]
    if h1_instructions != expected_h1:
        raise ValueError(f"entity lifecycle H1/source instruction order drift: {handler}")
    cursor = _cursor_rows(guard, handler=handler)
    callbacks = []
    for instruction in guard["directCallOrder"]:
        target_match = re.search(r"([A-Za-z_][A-Za-z0-9_]*)\)?(?:\.w)?$", instruction)
        if target_match is None or target_match.group(1) not in addresses:
            raise ValueError(f"entity lifecycle callback target drift: {handler}/{instruction}")
        callbacks.append(
            {
                "instruction": instruction,
                "instructionTarget": target_match.group(1),
                "callSiteAddress": _h1_instruction_address(listing, handler, instruction),
                "targetAddress": addresses[target_match.group(1)],
            }
        )
    if [row["instruction"] for row in callbacks] != guard["directCallOrder"]:
        raise ValueError(f"entity lifecycle callback order drift: {handler}")
    branches = []
    for branch in _closed_rows(
        guard["branchRecords"], {"branchInstruction", "branchTarget"}, name=f"{handler} branches"
    ):
        target = branch["branchTarget"]
        if set(target) != {"targetLabel", "targetInstruction", "targetStatementIndex"}:
            raise ValueError(f"entity lifecycle {handler} branch target shape drift")
        branches.append(
            {
                **branch,
                "branchSiteAddress": _h1_instruction_address(
                    listing, handler, branch["branchInstruction"]
                ),
                "targetInstructionAddress": _h1_instruction_address(
                    listing, handler, target["targetInstruction"]
                ),
            }
        )
    return {
        "macro": h2["macro"],
        "handler": handler,
        "handlerAddress": h2["address"],
        "opcode": h2["opcode"],
        "sourceCommandCount": h2["sourceCommandCount"],
        "cursorUseSites": cursor,
        "aliveStatusPointerAdjustment": guard["aliveStatusPointerAdjustment"],
        "stateReads": _state_rows(guard["stateReads"], equates, handler=handler, access="read"),
        "stateWrites": _state_rows(guard["stateWrites"], equates, handler=handler, access="write"),
        "addressSetupUseSites": guard["addressSetupUseSites"],
        "bitMutationUseSites": guard["bitMutationUseSites"],
        "sourceConstantUseSites": guard["sourceConstantUseSites"],
        "literalUseSites": guard["literalUseSites"],
        "branchRecords": branches,
        "callbacks": callbacks,
        "returnInstruction": guard["returnInstruction"],
        "sourceUseSites": source_rows,
    }


def _field_from_handler(
    handlers: list[dict[str, Any]], handler: str, operand: str
) -> dict[str, int]:
    rows = [
        row["entityFieldLayout"]
        for item in handlers
        if item["handler"] == handler
        for row in item["stateReads"] + item["stateWrites"]
        if row["sourceOperand"] == operand and row["entityFieldLayout"] is not None
    ]
    if len(rows) != 1:
        raise ValueError(f"entity lifecycle field layout relation drift: {handler}/{operand}")
    return rows[0]


def _callback(handler: dict[str, Any], target: str) -> dict[str, Any]:
    rows = [row for row in handler["callbacks"] if row["instructionTarget"] == target]
    if len(rows) != 1:
        raise ValueError(f"entity lifecycle callback seam drift: {handler['handler']}/{target}")
    return rows[0]


def _branch(handler: dict[str, Any], target: str) -> dict[str, Any]:
    rows = [row for row in handler["branchRecords"] if row["branchTarget"]["targetLabel"] == target]
    if len(rows) != 1:
        raise ValueError(f"entity lifecycle branch seam drift: {handler['handler']}/{target}")
    return rows[0]


def _guard_control_sections(source: str) -> None:
    """Keep every promoted branch, mutation, and callback order source-local."""
    for symbol, expected in CONTROL_SECTION_INSTRUCTIONS.items():
        actual = tuple(row["instruction"] for row in _source_section(source, symbol))
        if actual != expected:
            raise ValueError(f"entity lifecycle control-section source guard drift: {symbol}")


def build_map_entity_lifecycle_presentation_static_contract(
    rom_path: Path, upstream_path: Path
) -> dict[str, Any]:
    """Parse every scoped source relationship before any runtime fixture comparison."""
    h2_fixture = load_json(H2_FIXTURE)
    h2_output = build_map_script_engine_contract(rom_path, upstream_path)
    facts = h2_output["entityLifecyclePresentationCommandFacts"]
    expected_forms = list(HANDLER_FORMS)
    handlers_h2 = _closed_rows(
        facts["handlers"],
        {
            "macro",
            "handler",
            "address",
            "opcode",
            "sourceCommandCount",
            "operandAnnotations",
            "statementCount",
            "sectionGuard",
            "directCalls",
        },
        name="handlers",
    )
    if [(row["macro"], row["handler"]) for row in handlers_h2] != expected_forms:
        raise ValueError("entity lifecycle H2 handler identity/order drift")
    upstream = upstream_path.resolve(strict=True)
    source = (upstream / SOURCE_PATH).read_text(encoding="utf-8")
    _guard_control_sections(source)
    map_functions_source = (upstream / MAP_FUNCTIONS_SOURCE_PATH).read_text(encoding="utf-8")
    enum_source = (upstream / ENUMS_PATH).read_text(encoding="utf-8")
    constants_source = (upstream / CONSTANTS_PATH).read_text(encoding="utf-8")
    equates = {
        **_parse_equates(
            enum_source,
            {
                "COMBATANT_ALLIES_NUMBER",
                "ENTITYDEF_OFFSET_ACTSCRIPTADDR",
                "ENTITYDEF_OFFSET_ANIMCOUNTER",
                "ENTITYDEF_OFFSET_FLAGS_B",
                "ENTITYDEF_OFFSET_MAPSPRITE",
                "ENTITYDEF_SIZE",
            },
        ),
        **_parse_equates(
            constants_source, {"ENTITY_DATA", "ENTITY_INDEX_LIST", "SPRITE_SIZE", "byte_FFAFB0"}
        ),
    }
    listing = (upstream / H1_LISTING_PATH).read_text(encoding="utf-8")
    addresses = listing_symbol_addresses(listing)
    needed_symbols = {
        "AdjustScriptPointerByCharacterAliveStatus",
        "DmaMapsprite",
        "GetAllyMapsprite",
        "GetEntityAddressFromCharacter",
        "HideEntity",
        "LoadMapsprite",
        "RunMapSetupInitFunction",
        "UpdateEntitySprite_0",
        "WaitForVInt",
        "eas_Idle",
        "sub_45A8C",
        *(handler for _, handler in HANDLER_FORMS),
    }
    if not needed_symbols <= addresses.keys():
        raise ValueError("entity lifecycle H1 symbol inventory drift")
    handlers = [_handler_record(row, source, listing, addresses, equates) for row in handlers_h2]
    expected_h2 = h2_fixture["expected"]["entityLifecyclePresentationCommandFacts"]
    if {key: facts[key] for key in expected_h2} != expected_h2:
        raise ValueError("entity lifecycle H2 fixture/source drift")
    by_name = {row["handler"]: row for row in handlers}
    sprite = by_name["csc1A_setEntitySprite"]
    priority = by_name["csc53_setPriority"]
    size = by_name["csc50_setEntitySize"]
    sprite_constant = sprite["sourceConstantUseSites"]
    if sprite_constant != [
        {
            "symbol": "COMBATANT_ALLIES_NUMBER",
            "value": equates["COMBATANT_ALLIES_NUMBER"],
            "instruction": "cmpi.w #COMBATANT_ALLIES_NUMBER,d0",
        }
    ]:
        raise ValueError("entity lifecycle ally split source-constant relation drift")
    bit_rows = size["bitMutationUseSites"]
    if len(bit_rows) != 1 or bit_rows[0]["immediateValue"] <= 0:
        raise ValueError("entity lifecycle size bit use-site drift")
    bit_value = bit_rows[0]["immediateValue"]
    if bit_value & (bit_value - 1) or bit_rows[0]["bitIndices"] != [bit_value.bit_length() - 1]:
        raise ValueError("entity lifecycle size bit-index derivation drift")
    priority_setup = priority["addressSetupUseSites"]
    if priority_setup != [
        {
            "sourceOperand": "((byte_FFAFB0-$1000000)).w",
            "instruction": "lea ((byte_FFAFB0-$1000000)).w,a0",
        }
    ]:
        raise ValueError("entity lifecycle priority-base source use-site drift")
    sprite_size_runtime_writes = {
        "initializeMapEntities": _immediate_sprite_size_write(
            _source_section(
                map_functions_source, "InitializeMapEntities", end_marker="modend"
            ),
            source_file=MAP_FUNCTIONS_SOURCE_PATH,
            section="InitializeMapEntities",
        ),
        "csc2AEntityShiver": _immediate_sprite_size_write(
            _source_section(source, "csc2A_entityShiver"),
            source_file=SOURCE_PATH,
            section="csc2A_entityShiver",
        ),
    }
    return {
        "provenance": {
            "upstreamRepository": "https://github.com/ShiningForceCentral/SF2DISASM",
            "upstreamBranch": "master",
            "upstreamCommit": h2_fixture["upstreamCommit"],
            "h2FixturePath": "tests/fixtures/h2/map-script-engine-static-v1.json",
            "h2FixtureId": h2_fixture["id"],
            "h2FieldPath": "expected.entityLifecyclePresentationCommandFacts",
        },
        "romSha256": h2_fixture["romSha256"],
        "function": {
            "runMapSetupInitFunctionAddress": addresses["RunMapSetupInitFunction"],
            "aliveStatusHelperAddress": addresses["AdjustScriptPointerByCharacterAliveStatus"],
            **{f"{row['handler']}Address": row["handlerAddress"] for row in handlers},
            "easIdleAddress": addresses["eas_Idle"],
        },
        "ram": {
            "entityDataAddress": equates["ENTITY_DATA"],
            "entityIndexListAddress": equates["ENTITY_INDEX_LIST"],
            "priorityByteBaseAddress": equates["byte_FFAFB0"],
            "spriteSizeWordAddress": equates["SPRITE_SIZE"],
        },
        "constants": {
            "entityRecordByteCount": equates["ENTITYDEF_SIZE"],
            "combatantAlliesNumber": equates["COMBATANT_ALLIES_NUMBER"],
            "animCounter": _field_from_handler(
                handlers, "csc1B_startEntityAnim", "ENTITYDEF_OFFSET_ANIMCOUNTER(a5)"
            ),
            "actscriptPointer": _field_from_handler(
                handlers, "csc16_waitUntilEntityIdle", "ENTITYDEF_OFFSET_ACTSCRIPTADDR(a5)"
            ),
            "mapsprite": _field_from_handler(
                handlers, "csc1A_setEntitySprite", "ENTITYDEF_OFFSET_MAPSPRITE(a5)"
            ),
            "flagsB": _field_layout(
                bit_rows[0]["sourceOperand"], bit_rows[0]["instruction"], equates
            ),
            "sizeBitMutation": {
                "immediateValue": bit_value,
                "bitIndex": bit_value.bit_length() - 1,
                "transferByteCount": _instruction_width(bit_rows[0]["instruction"]),
                "sourceUseSite": _source_use_site(
                    size["sourceUseSites"], bit_rows[0]["instruction"], handler=size["handler"]
                ),
            },
            "spriteSizeRuntimeWrites": sprite_size_runtime_writes,
        },
        "sourceFacts": {
            "macroForms": facts["macros"],
            "handlers": handlers,
            "callerBreakdown": facts["callerBreakdown"],
            "evidenceLabels": {"staticFindings": "Confirmed", "runtimeObservations": "Unknown"},
        },
        "runtimeQuestions": facts["runtimeQuestions"],
    }


def _handler_by_macro(static: dict[str, Any], macro: str) -> dict[str, Any]:
    matches = [row for row in static["sourceFacts"]["handlers"] if row["macro"] == macro]
    if len(matches) != 1:
        raise ValueError(f"entity lifecycle handler seam drift: {macro}")
    return matches[0]


def _callback_by_target(handler: dict[str, Any], target: str) -> dict[str, Any]:
    matches = [row for row in handler["callbacks"] if row["instructionTarget"] == target]
    if len(matches) != 1:
        raise ValueError(f"entity lifecycle callback seam drift: {handler['handler']}/{target}")
    return matches[0]


def _only_state_layout(handler: dict[str, Any], operand: str) -> dict[str, int]:
    rows = [
        row["entityFieldLayout"]
        for row in handler["stateReads"] + handler["stateWrites"]
        if row["sourceOperand"] == operand and row["entityFieldLayout"] is not None
    ]
    if len(rows) != 1:
        raise ValueError(
            f"entity lifecycle source field relation drift: {handler['handler']}/{operand}"
        )
    return rows[0]


def _helper_alive_status_source_guard(source: str, listing: str) -> dict[str, Any]:
    """Guard the branch/order that moves A6 for the zero-current-HP path."""
    symbol = "AdjustScriptPointerByCharacterAliveStatus"
    rows = _source_section(source, symbol)
    expected = [
        "btst #COMBATANT_BIT_ENEMY,d0",
        "bne.s @Return",
        "cmpi.b #COMBATANT_ALLIES_NUMBER,d0",
        "bge.s @Return",
        "jsr j_GetCurrentHp",
        "tst.w d1",
        "bne.s @Return",
        "adda.w d7,a6",
        "movem.l (sp)+,d7",
        "rts",
    ]
    if [row["instruction"] for row in rows] != expected:
        raise ValueError("entity lifecycle alive-status helper source order/polarity drift")
    h1_rows = _h1_function_lines(listing, symbol)
    h1 = [instruction for _, instruction in h1_rows]
    if h1 != [re.sub(r"\s+", "", item) for item in expected]:
        raise ValueError("entity lifecycle alive-status helper H1/source order drift")
    return {
        "sourceUseSites": [rows[index] for index in (2, 5, 6, 7, 8, 9)],
        "cursorAdjustmentInstruction": "adda.w d7,a6",
        "cursorAdjustmentAddress": h1_rows[7][0],
        "zeroCurrentHpBranchInstruction": "bne.s @Return",
        "zeroCurrentHpBranchAddress": h1_rows[6][0],
    }


def _moveq_literal(instruction: str, register: str) -> int:
    match = re.fullmatch(rf"moveq #(?P<literal>\$[0-9A-Fa-f]+|-?\d+),{register}", instruction)
    if match is None:
        raise ValueError(f"entity lifecycle {register} load use-site drift: {instruction}")
    return _literal(match["literal"])


def _current_hp_storage_source_guard(
    upstream: Path, listing: str, equates: dict[str, int]
) -> dict[str, Any]:
    """Parse the actual current-HP word read instead of assuming observer width."""
    get_current_hp = _source_section(
        (upstream / CURRENT_HP_SOURCE_PATH).read_text(encoding="utf-8"), "GetCurrentHp"
    )
    get_word = _source_section(
        (upstream / COMBATANT_WORD_SOURCE_PATH).read_text(encoding="utf-8"), "GetCombatantWord"
    )
    expected_current = [
        "movem.l d7-a0,-(sp)",
        "moveq #COMBATANT_OFFSET_HP_CURRENT,d7",
        "bsr.w GetCombatantWord",
        "movem.l (sp)+,d7-a0",
        "rts",
    ]
    expected_word = ["bsr.s GetCombatantEntryAddress", "move.w (a0,d7.w),d1", "rts"]
    if [row["instruction"] for row in get_current_hp] != expected_current:
        raise ValueError("entity lifecycle GetCurrentHp source guard drift")
    if [row["instruction"] for row in get_word] != expected_word:
        raise ValueError("entity lifecycle GetCombatantWord source guard drift")
    h1_current = [instruction for _, instruction in _h1_function_lines(listing, "GetCurrentHp")]
    h1_word = [instruction for _, instruction in _h1_function_lines(listing, "GetCombatantWord")]
    if h1_current != [re.sub(r"\s+", "", item) for item in expected_current]:
        raise ValueError("entity lifecycle GetCurrentHp H1/source guard drift")
    if h1_word != [re.sub(r"\s+", "", item) for item in expected_word]:
        raise ValueError("entity lifecycle GetCombatantWord H1/source guard drift")
    offset_load = _source_use_site(
        get_current_hp, "moveq #COMBATANT_OFFSET_HP_CURRENT,d7", handler="GetCurrentHp"
    )
    storage = _source_use_site(get_word, "move.w (a0,d7.w),d1", handler="GetCombatantWord")
    if offset_load["instruction"] != "moveq #COMBATANT_OFFSET_HP_CURRENT,d7":
        raise ValueError("entity lifecycle current-HP offset/use-site relation drift")
    return {
        "byteOffset": equates["COMBATANT_OFFSET_HP_CURRENT"],
        "storageTransferByteCount": _instruction_width(storage["instruction"]),
        "offsetLoadSourceUseSite": offset_load,
        "storageSourceUseSite": storage,
    }


def _unsigned_word(value: object, *, name: str) -> int:
    if not isinstance(value, int) or not 0 <= value <= 0xFFFF:
        raise ValueError(f"entity lifecycle {name} is not a word: {value}")
    return value


def _case_handler(static: dict[str, Any], macro: str) -> dict[str, Any]:
    handler = _handler_by_macro(static, macro)
    address_key = f"{handler['handler']}Address"
    if static["function"].get(address_key) != handler["handlerAddress"]:
        raise ValueError(f"entity lifecycle H1 handler address relation drift: {macro}")
    return handler


def _case_callback_plan(handler: dict[str, Any]) -> list[dict[str, Any]]:
    return [
        {
            "instructionTarget": row["instructionTarget"],
            "callSiteAddress": row["callSiteAddress"],
            "targetAddress": row["targetAddress"],
        }
        for row in handler["callbacks"]
    ]


def _case_cursor_advance(handler: dict[str, Any]) -> int:
    rows = handler["cursorUseSites"]
    advance = sum(row["cursorAdvanceByteCount"] for row in rows)
    if advance < 1:
        raise ValueError(f"entity lifecycle handler cursor advance drift: {handler['handler']}")
    return advance


def _priority_byte_from_use_sites(handler: dict[str, Any], argument_word: int) -> int:
    """Resolve the selected byte write from the guarded nonzero branch and clear path."""
    branches = handler["branchRecords"]
    writes = handler["stateWrites"]
    if [row["branchInstruction"] for row in branches] != [
        "bne.s loc_46FD4",
        "bra.s return_46FDA",
    ] or [row["instruction"] for row in writes] != [
        "clr.b (a0,d0.w)",
        "move.b #1,(a0,d0.w)",
    ]:
        raise ValueError("entity lifecycle priority branch/write relation drift")
    if argument_word == 0:
        return 0
    literal = next(
        row for row in handler["literalUseSites"] if row["instruction"] == writes[1]["instruction"]
    )
    if literal["value"] != _literal(literal["literalText"]):
        raise ValueError("entity lifecycle priority nonzero literal use-site drift")
    return literal["value"]


def _derived_case_records(static: dict[str, Any], fixture: dict[str, Any]) -> list[dict[str, Any]]:
    """Derive source-local records before comparing each runtime golden result."""
    script_offset = fixture["instrumentation"].get("scriptInputRamOffset")
    if not isinstance(script_offset, int) or script_offset < 0:
        raise ValueError("entity lifecycle script input offset drift")
    runtime_sources = static["sourceFacts"].get("runtimeSourceUseSites")
    if not isinstance(runtime_sources, dict):
        raise ValueError("entity lifecycle runtime source-use-site inventory drift")
    current_hp = runtime_sources.get("currentHpStorage")
    helper = runtime_sources.get("aliveStatusHelper")
    cursor_loads = runtime_sources.get("startStopCursorAdjustmentLoads")
    if (
        not isinstance(current_hp, dict)
        or current_hp.get("read", {}).get("instruction") != "move.w (a0,d7.w),d1"
        or not isinstance(helper, list)
        or [row.get("instruction") for row in helper]
        != [
            "cmpi.b #COMBATANT_ALLIES_NUMBER,d0",
            "tst.w d1",
            "bne.s @Return",
            "adda.w d7,a6",
            "movem.l (sp)+,d7",
            "rts",
        ]
        or not isinstance(cursor_loads, list)
        or [_moveq_literal(row.get("instruction", ""), "d7") for row in cursor_loads]
        != [
            static["constants"]["startStopAliveStatusCursorAdjustmentByteCount"],
            static["constants"]["startStopAliveStatusCursorAdjustmentByteCount"],
        ]
        or static["constants"]["currentHpStorageTransferByteCount"]
        != _instruction_width(current_hp["read"]["instruction"])
        or static["constants"]["currentHpStatusTestTransferByteCount"]
        != _instruction_width(helper[1]["instruction"])
    ):
        raise ValueError("entity lifecycle HP/cursor source derivation drift")
    if (
        runtime_sources.get("idleWaitCompare", {}).get("instruction")
        != "cmpi.l #eas_Idle,ENTITYDEF_OFFSET_ACTSCRIPTADDR(a5)"
        or runtime_sources.get("idleWaitBackEdge", {}).get("instruction") != "bne.s loc_469A0"
        or runtime_sources.get("setSpriteAllyBranch", {}).get("instruction") != "bcc.s @NotAlly"
        or [row.get("instruction") for row in runtime_sources.get("setPriorityBranches", [])]
        != ["bne.s loc_46FD4", "bra.s return_46FDA"]
    ):
        raise ValueError("entity lifecycle branch source derivation drift")
    expected: list[dict[str, Any]] = []
    for case in fixture["cases"]:
        macro = case.get("macro")
        if not isinstance(macro, str) or macro not in dict(HANDLER_FORMS):
            raise ValueError(f"entity lifecycle case macro drift: {case.get('id')}")
        handler = _case_handler(static, macro)
        script_words = case.get("scriptWords")
        if not isinstance(script_words, list) or not script_words:
            raise ValueError(f"entity lifecycle script word corpus drift: {case.get('id')}")
        for value in script_words:
            _unsigned_word(value, name="script operand")
        entity_index = case.get("entityIndexByteSeed")
        if not isinstance(entity_index, int) or not 0 <= entity_index <= 0xFF:
            raise ValueError(f"entity lifecycle entity-index byte drift: {case.get('id')}")
        cursor_advance = _case_cursor_advance(handler)
        callback_plan = _case_callback_plan(handler)
        record: dict[str, Any] = {
            "id": case["id"],
            "macro": macro,
            "handlerAddress": handler["handlerAddress"],
            "entityAddress": static["ram"]["entityDataAddress"]
            + entity_index * static["constants"]["entityRecordByteCount"],
            "scriptCursorRamOffsetAfter": script_offset + cursor_advance,
            "declaredCallbackPlan": callback_plan,
            "effectiveCallbackPlan": callback_plan,
        }
        if macro in {"startEntity", "stopEntity"}:
            hp = _unsigned_word(case.get("currentHpWordSeed"), name="current HP seed")
            adjustment = (
                static["constants"]["startStopAliveStatusCursorAdjustmentByteCount"]
                if hp == 0
                else 0
            )
            if hp == 0 and len(script_words) < 2:
                raise ValueError(
                    "entity lifecycle zero-current-HP case lacks skipped selector word"
                )
            anim_counter_after = 0 if macro == "startEntity" else 0xFF
            effective_callbacks = callback_plan
            cursor_after = script_offset + adjustment + cursor_advance
            if hp == 0:
                seed = case.get("entityStateSeed", {}).get("animCounterByte")
                if not isinstance(seed, int) or not 0 <= seed <= 0xFF:
                    raise ValueError("entity lifecycle zero-current-HP animation seed drift")
                anim_counter_after = seed
                effective_callbacks = [callback_plan[0]]
                cursor_after = script_offset + adjustment
            record.update(
                {
                    "currentHpWordSeed": hp,
                    "aliveStatusCursorAdjustmentByteCount": adjustment,
                    "effectiveCallbackPlan": effective_callbacks,
                    "scriptCursorRamOffsetAfter": cursor_after,
                    "animCounterByteAfter": anim_counter_after,
                    "animCounterTransferByteCount": static["constants"]["animCounter"][
                        "transferByteCount"
                    ],
                }
            )
        elif macro == "waitIdle":
            injection_count = case.get("waitExitAtCompareEntryCount")
            if injection_count != 2:
                raise ValueError(
                    "entity lifecycle wait case must preserve the second-compare boundary"
                )
            record.update(
                {
                    "actscriptPointerTransferByteCount": static["constants"]["actscriptPointer"][
                        "transferByteCount"
                    ],
                    "waitLoopExitInjection": {
                        "compareAddress": static["function"]["waitIdleCompareAddress"],
                        "backEdgeAddress": static["function"]["waitIdleBackEdgeAddress"],
                        "field": "actscriptPointer",
                        "value": static["function"]["easIdleAddress"],
                        "atCompareEntryCount": injection_count,
                    },
                }
            )
        elif macro == "setSprite":
            mapsprite = _unsigned_word(
                script_words[1] if len(script_words) > 1 else None, name="mapsprite"
            )
            threshold = static["constants"]["combatantAlliesNumber"]
            split = "below-threshold" if mapsprite < threshold else "at-or-above-threshold"
            record.update(
                {
                    "mapspriteInputWord": mapsprite,
                    "allySplit": split,
                    "effectiveCallbackPlan": [
                        row
                        for row in callback_plan
                        if split == "below-threshold"
                        or row["instructionTarget"] != "GetAllyMapsprite"
                    ],
                    "mapspriteTransferByteCount": static["constants"]["mapsprite"][
                        "transferByteCount"
                    ],
                }
            )
        elif macro == "setPriority":
            argument = _unsigned_word(
                script_words[1] if len(script_words) > 1 else None, name="priority argument"
            )
            record.update(
                {
                    "priorityArgumentWord": argument,
                    "priorityByteAfter": _priority_byte_from_use_sites(handler, argument),
                }
            )
        elif macro == "setSize":
            size = _unsigned_word(script_words[1] if len(script_words) > 1 else None, name="size")
            original = _unsigned_word(case.get("spriteSizeWordSeed"), name="sprite-size seed")
            flags_seed = case.get("entityStateSeed", {}).get("flagsBByte")
            if not isinstance(flags_seed, int) or not 0 <= flags_seed <= 0xFF:
                raise ValueError("entity lifecycle size flags-B seed drift")
            runtime_writes = static["constants"].get("spriteSizeRuntimeWrites")
            if (
                not isinstance(runtime_writes, dict)
                or size != runtime_writes.get("initializeMapEntities", {}).get("value")
                or original != runtime_writes.get("csc2AEntityShiver", {}).get("value")
            ):
                raise ValueError("entity lifecycle source-valid sprite-size word drift")
            bit = static["constants"]["sizeBitMutation"]["immediateValue"]
            record.update(
                {
                    "spriteSizeWordInput": size,
                    "spriteSizeWordAfter": original,
                    "flagsBByteAfter": flags_seed | bit,
                    "sizeBitIndex": static["constants"]["sizeBitMutation"]["bitIndex"],
                }
            )
        expected.append(record)
    return expected


def derive_case_expectations(
    static: dict[str, Any], fixture: dict[str, Any]
) -> list[dict[str, Any]]:
    expected = _derived_case_records(static, fixture)
    if [case.get("expected") for case in fixture["cases"]] != expected:
        raise ValueError("entity lifecycle fixture/source static disagreement")
    for case in fixture["cases"]:
        overlap = set(case["expected"]) & set(case["runtimeGolden"])
        if overlap:
            raise ValueError(
                f"entity lifecycle expected/runtime golden overlap: {case['id']}/{sorted(overlap)}"
            )
    return expected

def build_map_entity_lifecycle_presentation_contract(
    rom_path: Path, upstream_path: Path
) -> dict[str, Any]:
    """Build H3 hooks only after their H2 and source use-site relations hold."""
    static = build_map_entity_lifecycle_presentation_static_contract(rom_path, upstream_path)
    upstream = upstream_path.resolve(strict=True)
    source = (upstream / SOURCE_PATH).read_text(encoding="utf-8")
    listing = (upstream / H1_LISTING_PATH).read_text(encoding="utf-8")
    equates = {
        **_parse_equates(
            (upstream / CONSTANTS_PATH).read_text(encoding="utf-8"), {"COMBATANT_DATA"}
        ),
        **_parse_equates(
            (upstream / ENUMS_PATH).read_text(encoding="utf-8"),
            {"COMBATANT_DATA_ENTRY_REAL_SIZE", "COMBATANT_MASK_ALL", "COMBATANT_OFFSET_HP_CURRENT"},
        ),
    }
    helper = _helper_alive_status_source_guard(source, listing)
    current_hp = _current_hp_storage_source_guard(upstream, listing, equates)
    handlers = {macro: _handler_by_macro(static, macro) for macro, _ in HANDLER_FORMS}
    wait, sprite, start, stop, priority = (
        handlers["waitIdle"],
        handlers["setSprite"],
        handlers["startEntity"],
        handlers["stopEntity"],
        handlers["setPriority"],
    )
    wait_branch = _branch(wait, "loc_469A0")
    wait_compare = _source_use_site(
        wait["sourceUseSites"],
        "cmpi.l #eas_Idle,ENTITYDEF_OFFSET_ACTSCRIPTADDR(a5)",
        handler=wait["handler"],
    )
    sprite_branch = _branch(sprite, "@NotAlly")
    priority_branches = priority["branchRecords"]
    if (
        wait_branch["branchInstruction"] != "bne.s loc_469A0"
        or wait_branch["targetInstructionAddress"]
        != _h1_instruction_address(listing, wait["handler"], wait_compare["instruction"])
        or sprite_branch["branchInstruction"] != "bcc.s @NotAlly"
        or [row["branchInstruction"] for row in priority_branches]
        != ["bne.s loc_46FD4", "bra.s return_46FDA"]
        or _only_state_layout(start, "ENTITYDEF_OFFSET_ANIMCOUNTER(a5)")
        != _only_state_layout(stop, "ENTITYDEF_OFFSET_ANIMCOUNTER(a5)")
    ):
        raise ValueError("entity lifecycle branch/state source relation drift")
    if priority_branches[0]["branchTarget"]["targetInstruction"] != "move.b #1,(a0,d0.w)":
        raise ValueError("entity lifecycle priority nonzero target relation drift")
    start_load = _source_use_site(start["sourceUseSites"], "moveq #2,d7", handler=start["handler"])
    stop_load = _source_use_site(stop["sourceUseSites"], "moveq #2,d7", handler=stop["handler"])
    adjustment = _moveq_literal(start_load["instruction"], "d7")
    if (
        adjustment != _moveq_literal(stop_load["instruction"], "d7")
        or helper["cursorAdjustmentInstruction"] != "adda.w d7,a6"
    ):
        raise ValueError("entity lifecycle d7 cursor-adjustment relation drift")
    status_test = next(row for row in helper["sourceUseSites"] if row["instruction"] == "tst.w d1")
    status_width = _instruction_width(status_test["instruction"])
    if status_width != current_hp["storageTransferByteCount"]:
        raise ValueError("entity lifecycle HP storage/status-test width relation drift")

    def callback_site(macro: str, target: str) -> int:
        return _callback_by_target(handlers[macro], target)["callSiteAddress"]

    function = {
        **static["function"],
        "hideGetEntityCallSiteAddress": callback_site("hide", "GetEntityAddressFromCharacter"),
        "hideCallbackCallSiteAddress": callback_site("hide", "HideEntity"),
        "startAdjustCallSiteAddress": callback_site(
            "startEntity", "AdjustScriptPointerByCharacterAliveStatus"
        ),
        "startGetEntityCallSiteAddress": callback_site(
            "startEntity", "GetEntityAddressFromCharacter"
        ),
        "startAnimCounterWriteAddress": _h1_instruction_address(
            listing, start["handler"], "move.b #0,ENTITYDEF_OFFSET_ANIMCOUNTER(a5)"
        ),
        "stopAdjustCallSiteAddress": callback_site(
            "stopEntity", "AdjustScriptPointerByCharacterAliveStatus"
        ),
        "stopGetEntityCallSiteAddress": callback_site(
            "stopEntity", "GetEntityAddressFromCharacter"
        ),
        "stopAnimCounterWriteAddress": _h1_instruction_address(
            listing, stop["handler"], "move.b #-1,ENTITYDEF_OFFSET_ANIMCOUNTER(a5)"
        ),
        "waitIdleGetEntityCallSiteAddress": callback_site(
            "waitIdle", "GetEntityAddressFromCharacter"
        ),
        "waitIdleCompareAddress": _h1_instruction_address(
            listing, wait["handler"], wait_compare["instruction"]
        ),
        "waitIdleBackEdgeAddress": wait_branch["branchSiteAddress"],
        "setSpriteGetEntityCallSiteAddress": callback_site(
            "setSprite", "GetEntityAddressFromCharacter"
        ),
        "setSpriteAllyCallbackCallSiteAddress": callback_site("setSprite", "GetAllyMapsprite"),
        "setSpriteWaitForVIntCallSiteAddress": callback_site("setSprite", "WaitForVInt"),
        "setSpriteUpdateCallSiteAddress": callback_site("setSprite", "UpdateEntitySprite_0"),
        "setSpriteAllyBranchAddress": sprite_branch["branchSiteAddress"],
        "setPriorityGetEntityCallSiteAddress": callback_site(
            "setPriority", "GetEntityAddressFromCharacter"
        ),
        "setPriorityNonzeroBranchAddress": priority_branches[0]["branchSiteAddress"],
        "setPriorityZeroReturnBranchAddress": priority_branches[1]["branchSiteAddress"],
        "setPriorityNonzeroWriteAddress": priority_branches[0]["targetInstructionAddress"],
        "setPriorityZeroClearAddress": _h1_instruction_address(
            listing, priority["handler"], "clr.b (a0,d0.w)"
        ),
        "removeShadowGetEntityCallSiteAddress": callback_site(
            "removeShadow", "GetEntityAddressFromCharacter"
        ),
        "removeShadowLoadMapspriteCallSiteAddress": callback_site("removeShadow", "LoadMapsprite"),
        "removeShadowHelperCallSiteAddress": callback_site("removeShadow", "sub_45A8C"),
        "removeShadowDmaCallSiteAddress": callback_site("removeShadow", "DmaMapsprite"),
        "removeShadowWaitForVIntCallSiteAddress": callback_site("removeShadow", "WaitForVInt"),
        "setSizeGetEntityCallSiteAddress": callback_site(
            "setSize", "GetEntityAddressFromCharacter"
        ),
        "setSizeUpdateCallSiteAddress": callback_site("setSize", "UpdateEntitySprite_0"),
        "setSizeWaitForVIntCallSiteAddress": callback_site("setSize", "WaitForVInt"),
        "aliveStatusCursorAdjustmentAddress": helper["cursorAdjustmentAddress"],
        "aliveStatusZeroCurrentHpBranchAddress": helper["zeroCurrentHpBranchAddress"],
    }
    return {
        **static,
        "function": function,
        "ram": {**static["ram"], "combatantDataAddress": equates["COMBATANT_DATA"]},
        "constants": {
            **static["constants"],
            "currentHpByteOffset": equates["COMBATANT_OFFSET_HP_CURRENT"],
            "combatantEntryByteCount": equates["COMBATANT_DATA_ENTRY_REAL_SIZE"],
            "combatantMaskAll": equates["COMBATANT_MASK_ALL"],
            "currentHpStorageTransferByteCount": current_hp["storageTransferByteCount"],
            "currentHpStatusTestTransferByteCount": status_width,
            "startStopAliveStatusCursorAdjustmentByteCount": adjustment,
        },
        "sourceFacts": {
            **static["sourceFacts"],
            "runtimeSourceUseSites": {
                "aliveStatusHelper": helper["sourceUseSites"],
                "currentHpStorage": {
                    "offsetLoad": current_hp["offsetLoadSourceUseSite"],
                    "read": current_hp["storageSourceUseSite"],
                },
                "startStopCursorAdjustmentLoads": [start_load, stop_load],
                "idleWaitCompare": wait_compare,
                "idleWaitBackEdge": _source_use_site(
                    wait["sourceUseSites"],
                    wait_branch["branchInstruction"],
                    handler=wait["handler"],
                ),
                "setSpriteAllyBranch": _source_use_site(
                    sprite["sourceUseSites"],
                    sprite_branch["branchInstruction"],
                    handler=sprite["handler"],
                ),
                "setPriorityBranches": [
                    _source_use_site(
                        priority["sourceUseSites"],
                        row["branchInstruction"],
                        handler=priority["handler"],
                    )
                    for row in priority_branches
                ],
            },
        },
    }


def verify_map_entity_lifecycle_presentation(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 120
) -> dict[str, Any]:
    """Run the complete eleven-case matrix in one session-only BizHawk launch."""
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner="entity lifecycle fixture")
    verify_runtime_contract(fixture, rom_path)
    static = build_map_entity_lifecycle_presentation_contract(rom_path, upstream_path)
    for field in (
        "romSha256",
        "provenance",
        "function",
        "ram",
        "constants",
        "sourceFacts",
        "runtimeQuestions",
    ):
        if fixture[field] != static[field]:
            raise ValueError(f"entity lifecycle fixture/source identity drift: {field}")
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
            output_name="map-entity-lifecycle-presentation",
            timeout_seconds=timeout_seconds,
        )

    observed = _with_instrumented_rom_database(
        instrumented_rom, "SF2 H3 instrumented map entity lifecycle presentation", observe
    )
    validate_json(observed, OBSERVATION_SCHEMA, owner="entity lifecycle observation")
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
            "entity lifecycle runtime matrix mismatch\n"
            f"static={derived!r}\nexpected={expected!r}\nobserved={observed!r}"
        )
    return {
        "Fixture": fixture["id"],
        "Cases": len(derived),
        "Handlers": len({case["macro"] for case in fixture["cases"]}),
        "BizHawkLaunches": 1,
        "Instrumentation": "session-only",
        "Status": "PASS",
    }
