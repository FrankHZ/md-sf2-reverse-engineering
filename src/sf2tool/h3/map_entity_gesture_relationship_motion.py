"""Static provenance for the grouped map-script entity gesture/motion H3 matrix.

Source command labels and RAM-field labels remain source labels.  This module
does not assign player-visible, collision, persistence, or story meaning.
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
FIXTURE = repo_path("tests/fixtures/h3/map-entity-gesture-relationship-motion-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h3-map-entity-gesture-relationship-motion-fixture.schema.json")
OBSERVATION_SCHEMA = repo_path(
    "schemas/h3-map-entity-gesture-relationship-motion-observation.schema.json"
)
OBSERVER = repo_path("tools/bizhawk/map_entity_gesture_relationship_motion_observer.lua")
H1_LISTING_PATH = Path("build/sf2build-h1.lst")
SOURCE_PATH = Path("disasm/code/common/scripting/map/mapscriptengine_1.asm")
ENUMS_PATH = Path("disasm/sf2enums.asm")
CURRENT_HP_SOURCE_PATH = Path("disasm/code/common/stats/combatantstats_1.asm")
COMBATANT_WORD_SOURCE_PATH = Path("disasm/code/common/stats/combatantstats_3.asm")

HANDLER_FORMS = (
    ("shiver", "csc2A_entityShiver"),
    ("nod", "csc26_entityNodHead"),
    ("followEntity", "csc2C_followEntity"),
    ("faceEntity", "csc52_faceEntity"),
    ("moveNextToPlayer", "csc28_moveEntityNextToPlayer"),
    ("fly", "csc2F_fly"),
    ("moveEntityAboveAnother", "csc31_moveEntityAboveEntity"),
)
FOLLOWER_TABLE_LABEL = "table_FollowerPositions"


def _literal(text: str) -> int:
    if re.fullmatch(r"\$[0-9A-Fa-f]+", text):
        return int(text[1:], 16)
    if re.fullmatch(r"%[01]+", text):
        return int(text[1:], 2)
    if re.fullmatch(r"-?\d+", text):
        return int(text)
    raise ValueError(f"entity gesture source literal is not numeric: {text}")


def _source_section(source: str, symbol: str) -> list[dict[str, Any]]:
    start = re.search(rf"^{re.escape(symbol)}:\s*$", source, re.MULTILINE)
    if start is None:
        raise ValueError(f"entity gesture source section is missing: {symbol}")
    end = source.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"entity gesture source section end is missing: {symbol}")
    first_line = source[: start.start()].count("\n")
    records: list[dict[str, Any]] = []
    for offset, raw in enumerate(source[start.start() : end].splitlines(), 1):
        instruction = re.sub(r"\s+", " ", raw.split(";", 1)[0].strip())
        if instruction and not instruction.endswith(":"):
            records.append({"instruction": instruction, "sourceLine": first_line + offset})
    return records


def _h1_function_instructions(listing: str, symbol: str) -> list[tuple[int, str]]:
    start = re.search(rf"^[0-9A-F]{{8}}\s+{re.escape(symbol)}:\s*$", listing, re.MULTILINE)
    if start is None:
        raise ValueError(f"entity gesture H1 section is missing: {symbol}")
    end = listing.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"entity gesture H1 section end is missing: {symbol}")
    records: list[tuple[int, str]] = []
    for raw in listing[start.start() : end].splitlines():
        match = re.fullmatch(r"(?P<address>[0-9A-F]{8})\s+(?P<body>.*)", raw)
        if match is None:
            continue
        body = re.sub(r"^(?:[0-9A-F]{2,8}\s+)+", "", match["body"].split(";", 1)[0].strip())
        if body and not body.endswith(":"):
            records.append((int(match["address"], 16), re.sub(r"\s+", "", body)))
    return records


def _h1_instruction_address(
    listing: str, symbol: str, instruction: str, *, occurrence: int = 0
) -> int:
    normalized = re.sub(r"\s+", "", instruction)
    matches = [
        address
        for address, actual in _h1_function_instructions(listing, symbol)
        if actual == normalized
    ]
    if occurrence >= len(matches):
        raise ValueError(f"entity gesture H1 instruction identity drift: {symbol}/{instruction}")
    return matches[occurrence]


def _h1_instruction_after_address(listing: str, symbol: str, instruction: str) -> int:
    records = _h1_function_instructions(listing, symbol)
    normalized = re.sub(r"\s+", "", instruction)
    matches = [index for index, (_, actual) in enumerate(records) if actual == normalized]
    if len(matches) != 1 or matches[0] + 1 >= len(records):
        raise ValueError(f"entity gesture H1 post-write instruction drift: {symbol}/{instruction}")
    return records[matches[0] + 1][0]


def _direct_calls(section: list[dict[str, Any]]) -> list[dict[str, Any]]:
    records: list[dict[str, Any]] = []
    for row in section:
        match = re.fullmatch(
            r"(?P<opcode>bsr|jsr)(?:\.[bwls])?\s+"
            r"(?P<operand>\([A-Za-z_][A-Za-z0-9_]*\)|[A-Za-z_][A-Za-z0-9_]*)"
            r"(?:\.[bwl])?",
            row["instruction"],
        )
        if match is None:
            continue
        operand = match["operand"]
        target = operand[1:-1] if operand.startswith("(") else operand
        records.append(
            {
                "instruction": row["instruction"],
                "opcode": match["opcode"],
                "instructionTarget": target,
                "sourceLine": row["sourceLine"],
            }
        )
    return records


def _parse_equates(source: str, names: set[str]) -> dict[str, int]:
    values: dict[str, int] = {}
    for name in sorted(names):
        match = re.search(
            rf"^{re.escape(name)}:\s+equ\s+(?P<value>\$[0-9A-Fa-f]+|%[01]+|-?\d+)\b",
            source,
            re.MULTILINE,
        )
        if match is None:
            raise ValueError(f"entity gesture source equate is missing: {name}")
        values[name] = _literal(match["value"])
    return values


def _follower_table(source: str) -> list[int]:
    start = re.search(rf"^{FOLLOWER_TABLE_LABEL}:\s*$", source, re.MULTILINE)
    if start is None:
        raise ValueError("entity gesture follower table is missing")
    end = source.find("; ===============", start.end())
    if end < 0:
        raise ValueError("entity gesture follower table boundary is missing")
    values: list[int] = []
    for raw in source[start.end() : end].splitlines():
        match = re.fullmatch(r"\s*dc\.b\s+(?P<value>-?\d+)\s*", raw)
        if match is not None:
            values.append(_literal(match["value"]))
    if not values or len(values) % 2:
        raise ValueError("entity gesture follower table pair boundary drift")
    return values


def _closed_rows(value: object, fields: set[str], *, name: str) -> list[dict[str, Any]]:
    if not isinstance(value, list) or not all(isinstance(row, dict) for row in value):
        raise ValueError(f"entity gesture H2 {name} container drift")
    if any(set(row) != fields for row in value):
        raise ValueError(f"entity gesture H2 {name} record shape drift")
    return list(value)


def build_map_entity_gesture_relationship_motion_static_contract(
    rom_path: Path, upstream_path: Path
) -> dict[str, Any]:
    """Parse the full seven-handler surface before runtime fixture comparison."""
    h2_fixture = load_json(H2_FIXTURE)
    expected = h2_fixture["expected"]["entityGestureRelationshipMotionCommandFacts"]
    handlers = _closed_rows(
        expected["handlers"],
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
    if [(row["macro"], row["handler"]) for row in handlers] != list(HANDLER_FORMS):
        raise ValueError("entity gesture H2 handler identity/order drift")
    upstream = upstream_path.resolve(strict=True)
    source = (upstream / SOURCE_PATH).read_text(encoding="utf-8")
    listing = (upstream / H1_LISTING_PATH).read_text(encoding="utf-8")
    addresses = listing_symbol_addresses(listing)
    needed = {
        "AddFollower",
        "AdjustScriptPointerByCharacterAliveStatus",
        "DmaMapsprite",
        "GetEntityAddressFromCharacter",
        "LoadMapsprite",
        "Sleep",
        "UpdateEntitySprite_0",
        "WaitForEntityToStopMoving",
        "WaitForVInt",
        "RunMapSetupInitFunction",
        "sub_45D70",
        *(handler for _, handler in HANDLER_FORMS),
    }
    if not needed <= addresses.keys():
        raise ValueError("entity gesture H1 symbol inventory drift")
    sections = {row["handler"]: _source_section(source, row["handler"]) for row in handlers}
    for row in handlers:
        actual = [record["instruction"] for record in sections[row["handler"]]]
        expected_order = row["sectionGuard"]["orderedInstructions"]
        if actual != expected_order:
            raise ValueError(f"entity gesture source section guard drift: {row['handler']}")
    h2_output = build_map_script_engine_contract(rom_path, upstream_path)
    facts = h2_output["entityGestureRelationshipMotionCommandFacts"]
    if {key: facts[key] for key in expected} != expected:
        raise ValueError("entity gesture H2 fixture/source drift")
    enum_values = _parse_equates(
        (upstream / ENUMS_PATH).read_text(encoding="utf-8"),
        {"DIRECTION_MASK", "DOWN", "LEFT", "MAP_TILE_SIZE", "RIGHT", "UP"},
    )
    records = []
    for row in handlers:
        section = sections[row["handler"]]
        direct_calls = _direct_calls(section)
        if [item["instructionTarget"] for item in direct_calls] != [
            item["instructionTarget"] for item in row["directCalls"]
        ]:
            raise ValueError(f"entity gesture direct-call order drift: {row['handler']}")
        occurrences: dict[str, int] = {}
        for item in direct_calls:
            instruction = item["instruction"]
            occurrence = occurrences.get(instruction, 0)
            item["callSiteAddress"] = _h1_instruction_address(
                listing, row["handler"], instruction, occurrence=occurrence
            )
            item["targetAddress"] = addresses[item["instructionTarget"]]
            occurrences[instruction] = occurrence + 1
        records.append(
            {
                "macro": row["macro"],
                "handler": row["handler"],
                "handlerAddress": row["address"],
                "opcode": row["opcode"],
                "sourceCommandCount": row["sourceCommandCount"],
                "sourceInstructions": section,
                "directCalls": direct_calls,
                "scriptCursorReadUseSites": row["sectionGuard"][
                    "scriptCursorReadUseSites"
                ],
                "branchRecords": row["sectionGuard"]["branchRecords"],
                "loopRecords": row["sectionGuard"]["loopRecords"],
            }
        )
    return {
        "provenance": {
            "upstreamRepository": "https://github.com/ShiningForceCentral/SF2DISASM",
            "upstreamBranch": "master",
            "upstreamCommit": h2_fixture["upstreamCommit"],
            "h2FixturePath": "tests/fixtures/h2/map-script-engine-static-v1.json",
            "h2FixtureId": h2_fixture["id"],
            "h2FieldPath": "expected.entityGestureRelationshipMotionCommandFacts",
        },
        "romSha256": h2_fixture["romSha256"],
        "function": {f"{row['handler']}Address": row["handlerAddress"] for row in records},
        "constants": enum_values,
        "followerPositionSignedByteTable": _follower_table(source),
        "sourceFacts": {"handlers": records, "h2RuntimeQuestions": facts["runtimeQuestions"]},
    }


def _handler(static: dict[str, Any], macro: str) -> dict[str, Any]:
    records = [row for row in static["sourceFacts"]["handlers"] if row["macro"] == macro]
    if len(records) != 1:
        raise ValueError(f"entity gesture handler identity drift: {macro}")
    return records[0]


def _callback_plan(handler: dict[str, Any]) -> list[dict[str, Any]]:
    return [
        {
            "instructionTarget": row["instructionTarget"],
            "callSiteAddress": row["callSiteAddress"],
            "targetAddress": row["targetAddress"],
        }
        for row in handler["directCalls"]
    ]


def _source_use(section: list[dict[str, Any]], instruction: str) -> dict[str, Any]:
    rows = [row for row in section if row["instruction"] == instruction]
    if len(rows) != 1:
        raise ValueError(f"entity gesture source use-site drift: {instruction}")
    return rows[0]


def _moveq_value(instruction: str, register: str) -> int:
    match = re.fullmatch(rf"moveq #(?P<literal>[^,]+),{re.escape(register)}", instruction)
    if match is None:
        raise ValueError(f"entity gesture moveq source use-site drift: {instruction}")
    value = _literal(match["literal"])
    return value - 0x100000000 if value & 0x80000000 else value


def _instruction_width(instruction: str) -> int:
    match = re.fullmatch(r"[a-z]+\.(?P<width>[bwl])\s+.+", instruction)
    if match is None:
        raise ValueError(f"entity gesture instruction width is missing: {instruction}")
    return {"b": 1, "w": 2, "l": 4}[match["width"]]


def _field_layout(
    equates: dict[str, int], section: list[dict[str, Any]], operand: str
) -> dict[str, int]:
    operand_pattern = r"(?<![A-Za-z0-9_])\(a5\)" if operand == "(a5)" else re.escape(operand)
    rows = [row for row in section if re.search(operand_pattern, row["instruction"])]
    if not rows:
        raise ValueError(f"entity gesture field use-site is missing: {operand}")
    widths = {_instruction_width(row["instruction"]) for row in rows if "." in row["instruction"]}
    if len(widths) != 1:
        raise ValueError(f"entity gesture field width relation drift: {operand}")
    if operand == "(a5)":
        return {"byteOffset": 0, "transferByteCount": widths.pop()}
    symbol = operand.removesuffix("(a5)")
    if symbol not in equates:
        raise ValueError(f"entity gesture field equate is missing: {symbol}")
    return {"byteOffset": equates[symbol], "transferByteCount": widths.pop()}


def _instruction_callback(
    handler: dict[str, Any], target: str, occurrence: int = 0
) -> dict[str, Any]:
    rows = [row for row in handler["directCalls"] if row["instructionTarget"] == target]
    if occurrence >= len(rows):
        raise ValueError(
            f"entity gesture callback source relation drift: {handler['macro']}/{target}"
        )
    return rows[occurrence]


def _field_value_seed(case: dict[str, Any], field: str) -> int:
    value = case["primaryEntityStateSeed"].get(field)
    if not isinstance(value, int):
        raise ValueError(f"entity gesture primary field seed drift: {case['id']}/{field}")
    return value


def _u16(value: object, *, name: str) -> int:
    if not isinstance(value, int) or not 0 <= value <= 0xFFFF:
        raise ValueError(f"entity gesture {name} is not a word: {value}")
    return value


def _signed_word(value: int) -> int:
    return value - 0x10000 if value & 0x8000 else value


def _word(value: int) -> int:
    return value & 0xFFFF


def _immediate_move_value(instruction: str, register: str) -> int:
    match = re.fullmatch(rf"move\.w #(?P<literal>[^,]+),{re.escape(register)}", instruction)
    if match is None:
        raise ValueError(f"entity gesture immediate move source use-site drift: {instruction}")
    return _literal(match["literal"])


def _immediate_store_value(instruction: str, operand: str) -> int:
    match = re.fullmatch(rf"move\.[bw] #(?P<literal>[^,]+),{re.escape(operand)}", instruction)
    if match is None:
        raise ValueError(f"entity gesture immediate store source use-site drift: {instruction}")
    return _literal(match["literal"])


def _current_hp_storage_guard(
    upstream: Path, listing: str, equates: dict[str, int]
) -> dict[str, Any]:
    current = _source_section(
        (upstream / CURRENT_HP_SOURCE_PATH).read_text(encoding="utf-8"), "GetCurrentHp"
    )
    word = _source_section(
        (upstream / COMBATANT_WORD_SOURCE_PATH).read_text(encoding="utf-8"), "GetCombatantWord"
    )
    current_expected = [
        "movem.l d7-a0,-(sp)",
        "moveq #COMBATANT_OFFSET_HP_CURRENT,d7",
        "bsr.w GetCombatantWord",
        "movem.l (sp)+,d7-a0",
        "rts",
    ]
    word_expected = ["bsr.s GetCombatantEntryAddress", "move.w (a0,d7.w),d1", "rts"]
    if [row["instruction"] for row in current] != current_expected:
        raise ValueError("entity gesture GetCurrentHp source guard drift")
    if [row["instruction"] for row in word] != word_expected:
        raise ValueError("entity gesture GetCombatantWord source guard drift")
    if [instruction for _, instruction in _h1_function_instructions(listing, "GetCurrentHp")] != [
        re.sub(r"\s+", "", instruction) for instruction in current_expected
    ]:
        raise ValueError("entity gesture GetCurrentHp H1/source guard drift")
    if [
        instruction for _, instruction in _h1_function_instructions(listing, "GetCombatantWord")
    ] != [re.sub(r"\s+", "", instruction) for instruction in word_expected]:
        raise ValueError("entity gesture GetCombatantWord H1/source guard drift")
    offset_load = _source_use(current, "moveq #COMBATANT_OFFSET_HP_CURRENT,d7")
    storage_read = _source_use(word, "move.w (a0,d7.w),d1")
    return {
        "byteOffset": equates["COMBATANT_OFFSET_HP_CURRENT"],
        "storageTransferByteCount": _instruction_width(storage_read["instruction"]),
        "offsetLoadSourceUseSite": offset_load,
        "storageReadSourceUseSite": storage_read,
    }


def _first_script_word_byte_probe(section: list[dict[str, Any]]) -> dict[str, Any]:
    source_use = _source_use(section, "move.b (a6),d0")
    match = re.fullmatch(r"move\.b \(a6\),(?P<register>d[0-7])", source_use["instruction"])
    if match is None:
        raise ValueError("entity gesture alive-status byte probe source relation drift")
    return {
        "sourceUseSite": source_use,
        "scriptCursorByteOffset": 0,
        "transferByteCount": _instruction_width(source_use["instruction"]),
        "scriptWordByteLane": "high",
        "destinationRegister": match["register"],
        "advancesScriptCursor": False,
    }


def build_map_entity_gesture_relationship_motion_contract(
    rom_path: Path, upstream_path: Path
) -> dict[str, Any]:
    """Derive runtime seams only from the parsed seven-handler source surface."""
    static = build_map_entity_gesture_relationship_motion_static_contract(rom_path, upstream_path)
    upstream = upstream_path.resolve(strict=True)
    source = (upstream / SOURCE_PATH).read_text(encoding="utf-8")
    listing = (upstream / H1_LISTING_PATH).read_text(encoding="utf-8")
    enum_values = _parse_equates(
        (upstream / ENUMS_PATH).read_text(encoding="utf-8"),
        {
            "COMBATANT_ALLIES_NUMBER",
            "COMBATANT_BIT_ENEMY",
            "COMBATANT_DATA_ENTRY_REAL_SIZE",
            "COMBATANT_MASK_ALL",
            "COMBATANT_OFFSET_HP_CURRENT",
            "DIRECTION_MASK",
            "DOWN",
            "ENTITYDEF_OFFSET_ANIMCOUNTER",
            "ENTITYDEF_OFFSET_FACING",
            "ENTITYDEF_OFFSET_FLAGS_B",
            "ENTITYDEF_OFFSET_LAYER",
            "ENTITYDEF_OFFSET_X",
            "ENTITYDEF_OFFSET_XDEST",
            "ENTITYDEF_OFFSET_XTRAVEL",
            "ENTITYDEF_OFFSET_XVELOCITY",
            "ENTITYDEF_OFFSET_Y",
            "ENTITYDEF_OFFSET_YDEST",
            "ENTITYDEF_OFFSET_YTRAVEL",
            "ENTITYDEF_OFFSET_YVELOCITY",
            "ENTITYDEF_SIZE",
            "LEFT",
            "MAP_TILE_SIZE",
            "RIGHT",
            "UP",
        },
    )
    ram_values = _parse_equates(
        (upstream / Path("disasm/sf2const.asm")).read_text(encoding="utf-8"),
        {"COMBATANT_DATA", "ENTITY_DATA", "ENTITY_INDEX_LIST", "SPRITE_SIZE"},
    )
    sections = {handler: _source_section(source, handler) for _, handler in HANDLER_FORMS}
    helper = _source_section(source, "AdjustScriptPointerByCharacterAliveStatus")
    current_hp = _current_hp_storage_guard(upstream, listing, enum_values)
    helper_expected = [
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
    if [row["instruction"] for row in helper] != helper_expected:
        raise ValueError("entity gesture alive-status helper source guard drift")
    by_macro = {macro: _handler(static, macro) for macro, _ in HANDLER_FORMS}
    shiver, nod, follow, face, move, fly, above = (
        by_macro["shiver"],
        by_macro["nod"],
        by_macro["followEntity"],
        by_macro["faceEntity"],
        by_macro["moveNextToPlayer"],
        by_macro["fly"],
        by_macro["moveEntityAboveAnother"],
    )
    first_script_word_byte_probe = _first_script_word_byte_probe(sections[follow["handler"]])
    shiver_loop = _moveq_value(
        _source_use(sections[shiver["handler"]], "moveq #2,d7")["instruction"], "d7"
    )
    nod_loop = _moveq_value(
        _source_use(sections[nod["handler"]], "moveq #0,d7")["instruction"], "d7"
    )
    shiver_size = _source_use(sections[shiver["handler"]], "move.w #21,((SPRITE_SIZE-$1000000)).w")
    shiver_restore_size = _source_use(
        sections[shiver["handler"]], "move.w d6,((SPRITE_SIZE-$1000000)).w"
    )
    shiver_restore_anim = _source_use(
        sections[shiver["handler"]], "move.b d5,ENTITYDEF_OFFSET_ANIMCOUNTER(a5)"
    )
    shiver_temporary_anim = _source_use(
        sections[shiver["handler"]], "move.b #-1,ENTITYDEF_OFFSET_ANIMCOUNTER(a5)"
    )
    shiver_sleep_values = [
        _moveq_value(row["instruction"], "d0")
        for row in sections[shiver["handler"]]
        if re.fullmatch(r"moveq #[^,]+,d0", row["instruction"])
    ]
    if not shiver_sleep_values or len(set(shiver_sleep_values)) != 1:
        raise ValueError("entity gesture shiver sleep source relation drift")
    shiver_sleep = shiver_sleep_values[0]
    nod_sleep = [
        _moveq_value(row["instruction"], "d0")
        for row in sections[nod["handler"]]
        if re.fullmatch(r"moveq #\d+,d0", row["instruction"])
    ]
    table_scale = _source_use(sections[follow["handler"]], "add.w d2,d2")
    if table_scale["instruction"] != "add.w d2,d2":
        raise ValueError("entity gesture follower selector-scale source drift")
    table_loads = [
        row
        for row in sections[follow["handler"]]
        if row["instruction"] in {"move.b (a0)+,d2", "move.b (a0)+,d3"}
    ]
    if [row["instruction"] for row in table_loads] != ["move.b (a0)+,d2", "move.b (a0)+,d3"]:
        raise ValueError("entity gesture follower table record-width source order drift")
    table_record_byte_count = sum(_instruction_width(row["instruction"]) for row in table_loads)
    if table_record_byte_count != _instruction_width(
        table_loads[0]["instruction"]
    ) + _instruction_width(table_loads[1]["instruction"]):
        raise ValueError("entity gesture follower table byte-stride derivation drift")
    follow_adjustment = _moveq_value(
        _source_use(sections[follow["handler"]], "moveq #6,d7")["instruction"], "d7"
    )
    face_writes = [
        row
        for row in sections[face["handler"]]
        if re.fullmatch(r"move\.b #[A-Z]+,ENTITYDEF_OFFSET_FACING\(a5\)", row["instruction"])
    ]
    if [row["instruction"] for row in face_writes] != [
        "move.b #RIGHT,ENTITYDEF_OFFSET_FACING(a5)",
        "move.b #LEFT,ENTITYDEF_OFFSET_FACING(a5)",
        "move.b #DOWN,ENTITYDEF_OFFSET_FACING(a5)",
        "move.b #UP,ENTITYDEF_OFFSET_FACING(a5)",
    ]:
        raise ValueError("entity gesture face direction write source order drift")
    move_direction_use_sites = [
        row
        for row in sections[move["handler"]]
        if row["instruction"]
        in {
            "addi.w #MAP_TILE_SIZE,d1",
            "subi.w #MAP_TILE_SIZE,d2",
            "subi.w #MAP_TILE_SIZE,d1",
            "addi.w #MAP_TILE_SIZE,d2",
        }
    ]
    if [row["instruction"] for row in move_direction_use_sites] != [
        "addi.w #MAP_TILE_SIZE,d1",
        "subi.w #MAP_TILE_SIZE,d2",
        "subi.w #MAP_TILE_SIZE,d1",
        "addi.w #MAP_TILE_SIZE,d2",
    ]:
        raise ValueError("entity gesture move-next direction source order drift")
    velocity_rows = [
        row for row in sections[move["handler"]] if row["instruction"] == "move.w #48,d4"
    ]
    velocity_y_rows = [
        row for row in sections[move["handler"]] if row["instruction"] == "move.w #48,d5"
    ]
    if len(velocity_rows) != 1 or len(velocity_y_rows) != 1:
        raise ValueError("entity gesture move-next velocity source use-site drift")
    velocity_magnitude = _immediate_move_value(velocity_rows[0]["instruction"], "d4")
    if velocity_magnitude != _immediate_move_value(velocity_y_rows[0]["instruction"], "d5"):
        raise ValueError("entity gesture x/y velocity source relation drift")
    fly_nonzero = _source_use(sections[fly["handler"]], "move.b #16,ENTITYDEF_OFFSET_LAYER(a5)")
    fly_zero = _source_use(sections[fly["handler"]], "clr.b ENTITYDEF_OFFSET_LAYER(a5)")
    nod_final_anim = _source_use(
        sections[nod["handler"]], "move.b #0,ENTITYDEF_OFFSET_ANIMCOUNTER(a5)"
    )
    above_offset = _moveq_value(
        _source_use(sections[above["handler"]], "moveq #$FFFFFFE8,d2")["instruction"], "d2"
    )
    above_vertical = _moveq_value(
        _source_use(sections[above["handler"]], "moveq #0,d3")["instruction"], "d3"
    )
    fields = {
        "animCounter": _field_layout(
            enum_values, sections[shiver["handler"]], "ENTITYDEF_OFFSET_ANIMCOUNTER(a5)"
        ),
        "flagsB": _field_layout(
            enum_values, sections[shiver["handler"]], "ENTITYDEF_OFFSET_FLAGS_B(a5)"
        ),
        "xWord": _field_layout(enum_values, sections[move["handler"]], "(a5)"),
        "yWord": _field_layout(enum_values, sections[move["handler"]], "ENTITYDEF_OFFSET_Y(a5)"),
        "xDest": _field_layout(
            enum_values, sections[move["handler"]], "ENTITYDEF_OFFSET_XDEST(a5)"
        ),
        "yDest": _field_layout(
            enum_values, sections[move["handler"]], "ENTITYDEF_OFFSET_YDEST(a5)"
        ),
        "xTravel": _field_layout(
            enum_values, sections[move["handler"]], "ENTITYDEF_OFFSET_XTRAVEL(a5)"
        ),
        "yTravel": _field_layout(
            enum_values, sections[move["handler"]], "ENTITYDEF_OFFSET_YTRAVEL(a5)"
        ),
        "xVelocity": _field_layout(
            enum_values, sections[move["handler"]], "ENTITYDEF_OFFSET_XVELOCITY(a5)"
        ),
        "yVelocity": _field_layout(
            enum_values, sections[move["handler"]], "ENTITYDEF_OFFSET_YVELOCITY(a5)"
        ),
        "facing": _field_layout(
            enum_values, sections[face["handler"]], "ENTITYDEF_OFFSET_FACING(a5)"
        ),
        "layer": _field_layout(enum_values, sections[fly["handler"]], "ENTITYDEF_OFFSET_LAYER(a5)"),
    }
    if fields["xWord"]["transferByteCount"] != fields["xDest"]["transferByteCount"]:
        raise ValueError("entity gesture x storage width source relation drift")
    if fields["yWord"]["transferByteCount"] != fields["yDest"]["transferByteCount"]:
        raise ValueError("entity gesture y storage width source relation drift")
    if any(
        fields[name]["transferByteCount"] != 2
        for name in ("xTravel", "yTravel", "xVelocity", "yVelocity")
    ):
        raise ValueError("entity gesture motion field width source relation drift")
    function = {
        **static["function"],
        "runMapSetupInitFunctionAddress": listing_symbol_addresses(listing)[
            "RunMapSetupInitFunction"
        ],
        "aliveStatusCursorAdjustmentAddress": _h1_instruction_address(
            listing, "AdjustScriptPointerByCharacterAliveStatus", "adda.w d7,a6"
        ),
        "shiverTemporarySizeWriteAddress": _h1_instruction_address(
            listing, shiver["handler"], shiver_size["instruction"]
        ),
        "shiverTemporarySizeAfterWriteAddress": _h1_instruction_after_address(
            listing, shiver["handler"], shiver_size["instruction"]
        ),
        "shiverRestoredSizeAfterWriteAddress": _h1_instruction_after_address(
            listing, shiver["handler"], shiver_restore_size["instruction"]
        ),
        "shiverFlagsSetAddress": _h1_instruction_address(
            listing, shiver["handler"], "ori.b #%1000,ENTITYDEF_OFFSET_FLAGS_B(a5)"
        ),
        "shiverFlagsSetAfterWriteAddress": _h1_instruction_after_address(
            listing, shiver["handler"], "ori.b #%1000,ENTITYDEF_OFFSET_FLAGS_B(a5)"
        ),
        "shiverFlagsClearAddress": _h1_instruction_address(
            listing, shiver["handler"], "andi.b #%11110111,ENTITYDEF_OFFSET_FLAGS_B(a5)"
        ),
        "shiverFlagsClearAfterWriteAddress": _h1_instruction_after_address(
            listing, shiver["handler"], "andi.b #%11110111,ENTITYDEF_OFFSET_FLAGS_B(a5)"
        ),
        "shiverTemporaryAnimCounterAfterWriteAddress": _h1_instruction_after_address(
            listing, shiver["handler"], shiver_temporary_anim["instruction"]
        ),
        "shiverRestoredAnimCounterAfterWriteAddress": _h1_instruction_after_address(
            listing, shiver["handler"], shiver_restore_anim["instruction"]
        ),
        "nodInitialSleepCallSiteAddress": _instruction_callback(nod, "Sleep", 0)["callSiteAddress"],
        "nodFinalAnimCounterAfterWriteAddress": _h1_instruction_after_address(
            listing, nod["handler"], nod_final_anim["instruction"]
        ),
        "faceUpdateCallSiteAddress": _instruction_callback(face, "UpdateEntitySprite_0")[
            "callSiteAddress"
        ],
        "moveNextFirstWaitCallSiteAddress": _instruction_callback(
            move, "WaitForEntityToStopMoving", 0
        )["callSiteAddress"],
        "flyZeroLayerAfterWriteAddress": _h1_instruction_after_address(
            listing, fly["handler"], fly_zero["instruction"]
        ),
        "flyNonzeroLayerAfterWriteAddress": _h1_instruction_after_address(
            listing, fly["handler"], fly_nonzero["instruction"]
        ),
    }
    return {
        **static,
        "function": function,
        "ram": {
            "combatantDataAddress": ram_values["COMBATANT_DATA"],
            "entityDataAddress": ram_values["ENTITY_DATA"],
            "entityIndexListAddress": ram_values["ENTITY_INDEX_LIST"],
            "spriteSizeWordAddress": ram_values["SPRITE_SIZE"],
        },
        "constants": {
            "entityRecordByteCount": enum_values["ENTITYDEF_SIZE"],
            "combatantEntryByteCount": enum_values["COMBATANT_DATA_ENTRY_REAL_SIZE"],
            "combatantMaskAll": enum_values["COMBATANT_MASK_ALL"],
            "currentHpByteOffset": current_hp["byteOffset"],
            "currentHpStorageTransferByteCount": current_hp["storageTransferByteCount"],
            "mapTileSize": enum_values["MAP_TILE_SIZE"],
            "directionValues": {
                name.lower(): enum_values[name] for name in ("RIGHT", "UP", "LEFT", "DOWN")
            },
            "directionMask": enum_values["DIRECTION_MASK"],
            "shiverLoopIterationCount": shiver_loop + 1,
            "nodLoopIterationCount": nod_loop + 1,
            "shiverSleepFrameCount": shiver_sleep,
            "nodSleepFrameCounts": nod_sleep,
            "shiverTemporarySpriteSizeWord": _immediate_store_value(
                shiver_size["instruction"], "((SPRITE_SIZE-$1000000)).w"
            ),
            "followDeadCursorAdjustmentByteCount": follow_adjustment,
            "followerTableSelectorByteStride": table_record_byte_count,
            "moveVelocityMagnitude": velocity_magnitude,
            "moveVelocityNegativeWord": _word(-velocity_magnitude),
            "flyNonzeroLayerByte": _immediate_store_value(
                fly_nonzero["instruction"], "ENTITYDEF_OFFSET_LAYER(a5)"
            ),
            "aboveFollowerHorizontalOffsetWord": _word(above_offset),
            "aboveFollowerVerticalOffsetWord": _word(above_vertical),
            "entityFieldLayouts": fields,
        },
        "sourceUseSites": {
            "aliveStatusHelper": helper,
            "aliveStatusFirstScriptWordByteProbe": first_script_word_byte_probe,
            "currentHpStorage": current_hp,
            "followerSelector": {
                "scaleInstruction": table_scale,
                "orderedTableByteLoads": table_loads,
                "recordByteStride": table_record_byte_count,
            },
            "faceDirectionWrites": face_writes,
            "moveDirectionWrites": move_direction_use_sites,
            "moveVelocityWrites": velocity_rows + velocity_y_rows,
            "flyNonzeroLayerWrite": fly_nonzero,
        },
        "runtimeQuestions": [
            {
                "group": "Map Test 0 controlled handler seams",
                "label": "Unknown",
                "questions": [
                    "Which normal map-script paths reach each of the seven handlers?",
                    "Do rendered animation, collision, path, or persistence effects follow "
                    "these bounded RAM and callback seams?",
                    "How do unseeded entity, combatant, and follower-table inputs affect "
                    "these seams outside the fixed matrix?",
                ],
            }
        ],
    }


STATE_FIELDS = (
    "xWord",
    "yWord",
    "xDest",
    "yDest",
    "xTravel",
    "yTravel",
    "xVelocity",
    "yVelocity",
    "facingByte",
    "layerByte",
    "animCounterByte",
    "flagsBByte",
)


def _state_seed(case: dict[str, Any], entity_index: int, static: dict[str, Any]) -> dict[str, int]:
    matches = [row for row in case["entityStateSeeds"] if row["entityIndex"] == entity_index]
    if len(matches) != 1 or set(matches[0]) != {"entityIndex", *STATE_FIELDS}:
        raise ValueError(f"entity gesture entity state seed drift: {case['id']}/{entity_index}")
    state = {field: matches[0][field] for field in STATE_FIELDS}
    field_layouts = static["constants"]["entityFieldLayouts"]
    state_layout_names = {
        "xWord": "xWord",
        "yWord": "yWord",
        "xDest": "xDest",
        "yDest": "yDest",
        "xTravel": "xTravel",
        "yTravel": "yTravel",
        "xVelocity": "xVelocity",
        "yVelocity": "yVelocity",
        "facingByte": "facing",
        "layerByte": "layer",
        "animCounterByte": "animCounter",
        "flagsBByte": "flagsB",
    }
    for name, value in state.items():
        width = field_layouts[state_layout_names[name]]["transferByteCount"]
        if not isinstance(value, int) or not 0 <= value < 1 << (width * 8):
            raise ValueError(f"entity gesture entity state seed width drift: {case['id']}/{name}")
    return state


def _entity_index(case: dict[str, Any], character: int) -> int:
    matches = [
        row["entityIndex"] for row in case["entityIndexMappings"] if row["character"] == character
    ]
    if len(matches) != 1:
        raise ValueError(f"entity gesture entity index mapping drift: {case['id']}/{character}")
    return matches[0]


def _callback_targets(plan: list[dict[str, Any]]) -> list[str]:
    return [row["instructionTarget"] for row in plan]


def _expanded_callback_plan(
    macro: str, direct_plan: list[dict[str, Any]], static: dict[str, Any]
) -> list[dict[str, Any]]:
    """Expand only source DBF loop bodies; all entries retain physical call sites."""
    if macro == "shiver":
        return [
            direct_plan[0],
            *(direct_plan[1:] * static["constants"]["shiverLoopIterationCount"]),
        ]
    if macro == "nod":
        return [
            direct_plan[0],
            direct_plan[1],
            *(direct_plan[2:] * static["constants"]["nodLoopIterationCount"]),
        ]
    return direct_plan


def _sub_abs_word(destination: int, source: int) -> int:
    """Model a word-sized sub.w followed by or.w/bge.s and optional neg.w."""
    difference = _word(destination - source)
    return _word(-difference) if difference & 0x8000 else difference


def _face_value(case: dict[str, Any], static: dict[str, Any]) -> int:
    actor = _state_seed(case, _entity_index(case, case["scriptWords"][0]), static)
    target = _state_seed(case, _entity_index(case, case["scriptWords"][1]), static)
    x_distance = _sub_abs_word(target["xDest"], actor["xDest"])
    y_distance = _sub_abs_word(target["yDest"], actor["yDest"])
    directions = static["constants"]["directionValues"]
    if y_distance >= x_distance:
        return directions["up"] if target["yDest"] < actor["yDest"] else directions["down"]
    return directions["left"] if target["xDest"] < actor["xDest"] else directions["right"]


def _move_destinations(case: dict[str, Any], static: dict[str, Any]) -> tuple[int, int]:
    player = _state_seed(case, _entity_index(case, 0), static)
    direction = case["scriptWords"][1]
    directions = static["constants"]["directionValues"]
    step = static["constants"]["mapTileSize"]
    x_dest, y_dest = player["xDest"], player["yDest"]
    if direction == directions["right"]:
        x_dest += step
    elif direction == directions["up"]:
        y_dest -= step
    elif direction == directions["left"]:
        x_dest -= step
    elif direction == directions["down"]:
        y_dest += step
    else:
        raise ValueError(f"entity gesture move direction operand drift: {case['id']}")
    return _word(x_dest), _word(y_dest)


def _signed_velocity(destination: int, current: int, magnitude: int) -> tuple[int, int]:
    """Model the handler's 16-bit sub.w / bpl.s / neg.w sequence exactly."""
    delta = _word(destination - current)
    if delta & 0x8000 == 0:
        return delta, magnitude
    return _word(-delta), _word(-magnitude)


def _current_hp_seed_probe(
    case: dict[str, Any], static: dict[str, Any], *, source_helper_invoked: bool
) -> dict[str, Any]:
    probe = static["sourceUseSites"]["aliveStatusFirstScriptWordByteProbe"]
    word = _u16(case["scriptWords"][0], name=f"first script word: {case['id']}")
    byte_offset = probe["scriptCursorByteOffset"]
    if probe["scriptWordByteLane"] != "high" or byte_offset != 0:
        raise ValueError("entity gesture first script-word byte-lane derivation drift")
    character = word.to_bytes(2, "big")[byte_offset]
    constants = static["constants"]
    storage_address = (
        static["ram"]["combatantDataAddress"]
        + (character & constants["combatantMaskAll"]) * constants["combatantEntryByteCount"]
        + constants["currentHpByteOffset"]
    )
    return {
        "sourceHelperInvoked": source_helper_invoked,
        "firstScriptWordByteOffset": byte_offset,
        "firstScriptWordByteLane": probe["scriptWordByteLane"],
        "characterByte": character,
        "storageAddress": storage_address,
        "storageTransferByteCount": constants["currentHpStorageTransferByteCount"],
    }


def _script_cursor_ram_offset_after(
    case: dict[str, Any], handler: dict[str, Any], fixture: dict[str, Any]
) -> int:
    """Derive the post-handler A6 offset from parsed advancing source reads."""
    instrumentation = fixture.get("instrumentation")
    if not isinstance(instrumentation, dict):
        raise ValueError("entity gesture fixture instrumentation drift")
    input_offset = instrumentation.get("scriptInputRamOffset")
    if not isinstance(input_offset, int) or isinstance(input_offset, bool) or input_offset < 0:
        raise ValueError("entity gesture fixture script-input offset drift")
    use_sites = handler.get("scriptCursorReadUseSites")
    expected_fields = {
        "sourceRegister",
        "destinationOperand",
        "transferredByteCount",
        "cursorAdvanceByteCount",
        "instruction",
    }
    if not isinstance(use_sites, list) or not use_sites:
        raise ValueError("entity gesture cursor use-site inventory drift")
    if any(not isinstance(row, dict) or set(row) != expected_fields for row in use_sites):
        raise ValueError("entity gesture cursor use-site record shape drift")
    source_instructions = handler.get("sourceInstructions")
    if not isinstance(source_instructions, list):
        raise ValueError("entity gesture cursor source-section drift")
    parsed_order = [row["instruction"] for row in use_sites]
    source_order = [
        row["instruction"]
        for row in source_instructions
        if isinstance(row, dict)
        and isinstance(row.get("instruction"), str)
        and re.fullmatch(r"move\.[bwl] \(a6\)\+?,d[0-7]", row["instruction"])
    ]
    if parsed_order != source_order:
        raise ValueError("entity gesture cursor use-site source order drift")
    advancing = [row for row in use_sites if row["cursorAdvanceByteCount"]]
    script_words = case.get("scriptWords")
    if not isinstance(script_words, list) or len(script_words) != len(advancing):
        raise ValueError("entity gesture fixture/source cursor-word order drift")
    for word, use_site in zip(script_words, advancing, strict=True):
        _u16(word, name=f"script word: {case['id']}")
        if not re.fullmatch(r"move\.w \(a6\)\+,d[0-7]", use_site["instruction"]):
            raise ValueError("entity gesture cursor use-site width/order drift")
        if (
            use_site["transferredByteCount"] != use_site["cursorAdvanceByteCount"]
            or use_site["cursorAdvanceByteCount"] <= 0
        ):
            raise ValueError("entity gesture cursor use-site advance drift")
    for use_site in use_sites:
        if use_site["cursorAdvanceByteCount"] == 0:
            if use_site["instruction"] != "move.b (a6),d0":
                raise ValueError("entity gesture non-advancing cursor probe drift")
        elif use_site["sourceRegister"] != "a6":
            raise ValueError("entity gesture advancing cursor source drift")
    return input_offset + sum(row["cursorAdvanceByteCount"] for row in advancing)


def _derived_case_records(
    static: dict[str, Any], fixture: dict[str, Any]
) -> list[dict[str, Any]]:
    """Derive each fixed case from parsed source operands and seeded RAM records."""
    records: list[dict[str, Any]] = []
    handlers = {row["macro"]: row for row in static["sourceFacts"]["handlers"]}
    for case in fixture["cases"]:
        macro = case["macro"]
        if macro not in handlers:
            raise ValueError(f"entity gesture fixture macro drift: {case['id']}")
        handler = handlers[macro]
        direct_plan = _callback_plan(handler)
        primary_index = _entity_index(case, case["scriptWords"][0])
        primary_state = _state_seed(case, primary_index, static)
        effective_plan = _expanded_callback_plan(macro, direct_plan, static)
        cursor_after = _script_cursor_ram_offset_after(case, handler, fixture)
        source_helper_invoked = any(
            row["instructionTarget"] == "AdjustScriptPointerByCharacterAliveStatus"
            for row in direct_plan
        )
        current_hp_probe = (
            _current_hp_seed_probe(case, static, source_helper_invoked=True)
            if source_helper_invoked
            else None
        )
        source_local: dict[str, Any] = {
            "faceFacingByteAtUpdateCall": None,
            "flyLayerByteAfterWrite": None,
            "moveFirstWaitState": None,
            "nodFinalAnimCounterByteAfterWrite": None,
            "shiverTemporarySpriteSizeWordAfterWrite": None,
            "shiverTemporaryAnimCounterByteAfterWrite": None,
            "shiverRestoredSpriteSizeWordAfterWrite": None,
            "shiverRestoredAnimCounterByteAfterWrite": None,
            "shiverFlagsSetWriteCount": 0,
            "shiverFlagsClearWriteCount": 0,
            "shiverFlagsBitSetAfterWrite": None,
            "shiverFlagsBitClearAfterWrite": None,
        }
        if macro == "shiver":
            source_local.update(
                {
                    "shiverTemporarySpriteSizeWordAfterWrite": static["constants"][
                        "shiverTemporarySpriteSizeWord"
                    ],
                    "shiverTemporaryAnimCounterByteAfterWrite": 0xFF,
                    "shiverRestoredSpriteSizeWordAfterWrite": _u16(
                        case["spriteSizeWordSeed"], name=f"sprite-size seed: {case['id']}"
                    ),
                    "shiverRestoredAnimCounterByteAfterWrite": primary_state["animCounterByte"],
                    "shiverFlagsSetWriteCount": static["constants"]["shiverLoopIterationCount"],
                    "shiverFlagsClearWriteCount": static["constants"]["shiverLoopIterationCount"],
                    "shiverFlagsBitSetAfterWrite": True,
                    "shiverFlagsBitClearAfterWrite": False,
                }
            )
        elif macro == "nod":
            source_local["nodFinalAnimCounterByteAfterWrite"] = 0
        elif macro == "followEntity":
            hp_seed = _u16(case["currentHpWordSeed"], name=f"current-HP seed: {case['id']}")
            if hp_seed == 0:
                effective_plan = [direct_plan[0]]
        elif macro == "faceEntity":
            source_local["faceFacingByteAtUpdateCall"] = _face_value(case, static)
        elif macro == "moveNextToPlayer":
            state = dict(primary_state)
            x_dest, y_dest = _move_destinations(case, static)
            x_travel, x_velocity = _signed_velocity(
                x_dest, state["xWord"], static["constants"]["moveVelocityMagnitude"]
            )
            y_travel, y_velocity = _signed_velocity(
                y_dest, state["yWord"], static["constants"]["moveVelocityMagnitude"]
            )
            state.update(
                {
                    "xDest": x_dest,
                    "yDest": y_dest,
                    "xTravel": x_travel,
                    "yTravel": y_travel,
                    "xVelocity": x_velocity,
                    "yVelocity": y_velocity,
                }
            )
            source_local["moveFirstWaitState"] = {
                key: state[key]
                for key in (
                    "xWord",
                    "yWord",
                    "xDest",
                    "yDest",
                    "xTravel",
                    "yTravel",
                    "xVelocity",
                    "yVelocity",
                    "facingByte",
                )
            }
        elif macro == "fly":
            operand = _u16(case["scriptWords"][1], name=f"fly operand: {case['id']}")
            source_local["flyLayerByteAfterWrite"] = (
                static["constants"]["flyNonzeroLayerByte"] if operand else 0
            )
        elif macro != "moveEntityAboveAnother":
            raise ValueError(f"entity gesture fixture macro is not derived: {case['id']}")
        expected = {
            "id": case["id"],
            "handlerAddress": handler["handlerAddress"],
            "scriptCursorRamOffsetAfter": cursor_after,
            "directCallbackPlan": direct_plan,
            "effectiveCallbackPlan": effective_plan,
            "currentHpSeedProbe": current_hp_probe,
            "sourceLocal": source_local,
        }
        records.append(expected)
    return records


def derive_case_expectations(
    static: dict[str, Any], fixture: dict[str, Any]
) -> list[dict[str, Any]]:
    """Require the fixture's entire semantic matrix to equal parsed derivation."""
    expected_records = _derived_case_records(static, fixture)
    expected = [case["expected"] for case in fixture["cases"]]
    if expected != expected_records:
        raise ValueError("entity gesture fixture/source static disagreement")
    for case in fixture["cases"]:
        overlap = set(case["expected"]) & set(case["runtimeGolden"])
        if overlap:
            raise ValueError(f"entity gesture expected/runtime golden overlap: {case['id']}")
    return expected_records


def verify_map_entity_gesture_relationship_motion(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 180
) -> dict[str, Any]:
    """Run all seventeen bounded cases in one session-only Map Test 0 launch."""
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner="entity gesture fixture")
    verify_runtime_contract(fixture, rom_path)
    static = build_map_entity_gesture_relationship_motion_contract(rom_path, upstream_path)
    for field in (
        "romSha256",
        "provenance",
        "function",
        "ram",
        "constants",
        "followerPositionSignedByteTable",
        "sourceFacts",
        "sourceUseSites",
        "runtimeQuestions",
    ):
        if fixture[field] != static[field]:
            raise ValueError(f"entity gesture fixture/source identity drift: {field}")
    derived = derive_case_expectations(static, fixture)
    instrumented_rom = _instrument_rom(rom_path, fixture)

    def observe() -> dict[str, Any]:
        callback_hooks = [
            {
                "callSiteAddress": row["callSiteAddress"],
                "instructionTarget": row["instructionTarget"],
            }
            for handler in static["sourceFacts"]["handlers"]
            for row in handler["directCalls"]
        ]
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
                "callbackHooks": callback_hooks,
            },
            output_name="map-entity-gesture-relationship-motion",
            timeout_seconds=timeout_seconds,
        )

    observed = _with_instrumented_rom_database(
        instrumented_rom, "SF2 H3 instrumented map entity gesture relationship motion", observe
    )
    validate_json(observed, OBSERVATION_SCHEMA, owner="entity gesture observation")
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
            "entity gesture runtime matrix mismatch\n"
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
