"""One-launch, handler-local runtime contract for entity-presentation FX commands.

Eight bounded service entries are shimmed at their actual entry PCs in a
session-only ROM copy. WaitForVInt retains its original entry, body, and
return path. This proves the three map-script handlers' operand, branch, loop,
call, return, and two direct entity-byte-write seams without attributing any
player-visible result or service-body effect to them.
"""

from __future__ import annotations

import hashlib
import json
import re
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.h2.map_script_engine import _canonical_bytes, build_map_script_engine_contract
from sf2tool.h3.bizhawk import DERIVED_ROOT, run_observer, verify_runtime_contract
from sf2tool.h3.map_lifecycle import _instrument_rom, _with_instrumented_rom_database
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.rom import mega_drive_checksum

H2_FIXTURE = repo_path("tests/fixtures/h2/map-script-engine-static-v1.json")
FIXTURE = repo_path("tests/fixtures/h3/map-script-entity-presentation-fx-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h3-map-script-entity-presentation-fx-fixture.schema.json")
OBSERVATION_SCHEMA = repo_path(
    "schemas/h3-map-script-entity-presentation-fx-observation.schema.json"
)
OBSERVER = repo_path("tools/bizhawk/map_script_entity_presentation_fx_observer.lua")
H1_LISTING_PATH = Path("build/sf2build-h1.lst")
SOURCE_PATH = Path("disasm/code/common/scripting/map/mapscriptengine_1.asm")
MAP_SETUP_SOURCE_PATH = Path("disasm/code/common/scripting/map/mapsetupsfunctions_1.asm")
ENUMS_PATH = Path("disasm/sf2enums.asm")
CONSTANTS_PATH = Path("disasm/sf2const.asm")

HANDLER_FORMS = (
    ("animEntityFX", "csc22_animateEntityFadeInOrOut"),
    ("headshake", "csc27_entityShakeHead"),
    ("entityFlashWhite", "csc18_flashEntityWhite"),
)
TARGET_IDENTITIES = (
    "GetEntityAddressFromCharacter",
    "LoadMapsprite",
    "ApplySpriteCropEffect",
    "DmaMapsprite",
    "WaitForVInt",
    "sub_45E10",
    "sub_45D1C",
    "UpdateEntitySprite_0",
    "sub_45D46",
)
SPECIAL_TRANSITION_SOURCE_LABELS = (
    "ENTITY_TRANSITION_MOSAIC_OUT",
    "ENTITY_TRANSITION_MOSAIC_IN",
)
RUNTIME_QUESTIONS = [
    "map-script-entity-presentation-fx/normal-story-reachability",
    "map-script-entity-presentation-fx/player-visible-output-timing-completion-repeat",
    "map-script-entity-presentation-fx/service-body-map-entity-state-effects",
    "map-script-entity-presentation-fx/persistence-and-map-entity-interactions",
]
OBSERVER_OUTPUT_NAME = "map-script-entity-presentation-fx"
OBSERVER_FAILURE_CONTRACT = {
    "exitCode": 1,
    "removeOutputBeforeExit": True,
    "statusPrefix": "failure:observer-callback:",
}
_OBSERVER_FAILURE_FIELDS = {
    "actualPc",
    "caseId",
    "error",
    "expectedCallSiteAddress",
    "expectedReturnAddress",
    "expectedTargetAddress",
    "pendingCallback",
    "phase",
}
_OBSERVER_PHASE_ORDER = (
    "callback-return",
    "number-prompt",
    "flag-prompt",
    "setup-case",
    "handler-animEntityFX",
    "handler-headshake",
    "handler-entityFlashWhite",
    "operand-csc22-first",
    "operand-csc22-second",
    "operand-csc27-first",
    "operand-csc18-first",
    "flash-duration-csc18-second",
    "selector-first-compare",
    "selector-second-compare",
    "selector-post-loop-compare",
    "special-transition-d1-bit-test",
    "special-transition-shift",
    "special-transition-add",
    "loop-anim-regular",
    "loop-anim-chunk",
    "loop-headshake",
    "loop-flash",
    "field-headshake-anim-initial",
    "field-headshake-anim-final",
    "field-flash-flags-set",
    "field-flash-flags-clear",
    "handler-return",
    "callback-site",
    "callback-target",
    "post-handler",
)


def _observer_status_path() -> Path:
    return DERIVED_ROOT / f"{OBSERVER_OUTPUT_NAME}.status.txt"


def _observer_output_path() -> Path:
    return DERIVED_ROOT / f"{OBSERVER_OUTPUT_NAME}.observed.json"


def _callback_failure_status(status_path: Path) -> dict[str, Any] | None:
    """Return the observer's structured callback failure sentinel, if present."""
    if not status_path.is_file():
        return None
    prefix = OBSERVER_FAILURE_CONTRACT["statusPrefix"]
    failures = [
        line.removeprefix(prefix)
        for line in status_path.read_text(encoding="utf-8").splitlines()
        if line.startswith(prefix)
    ]
    if not failures:
        return None
    if len(failures) != 1:
        raise ValueError("entity-presentation FX callback failure status multiplicity drift")
    try:
        payload = json.loads(failures[0])
    except json.JSONDecodeError as error:
        raise ValueError("entity-presentation FX callback failure status JSON drift") from error
    if not isinstance(payload, dict) or set(payload) != _OBSERVER_FAILURE_FIELDS:
        raise ValueError("entity-presentation FX callback failure status shape drift")
    if not isinstance(payload["phase"], str) or not isinstance(payload["error"], str):
        raise ValueError("entity-presentation FX callback failure status text drift")
    if payload["caseId"] is not None and not isinstance(payload["caseId"], str):
        raise ValueError("entity-presentation FX callback failure case identity drift")
    for field in (
        "actualPc",
        "expectedCallSiteAddress",
        "expectedTargetAddress",
        "expectedReturnAddress",
    ):
        if payload[field] is not None and (
            not isinstance(payload[field], int) or isinstance(payload[field], bool)
        ):
            raise ValueError(f"entity-presentation FX callback failure {field} drift")
    if payload["pendingCallback"] is not None and not isinstance(payload["pendingCallback"], dict):
        raise ValueError("entity-presentation FX callback failure pending state drift")
    return payload


def _raise_for_callback_failure_status(status_path: Path, output_path: Path) -> None:
    payload = _callback_failure_status(status_path)
    if payload is not None:
        output_path.unlink(missing_ok=True)
        raise RuntimeError(
            "entity-presentation FX observer callback failure status:\n"
            f"{json.dumps(payload, sort_keys=True)}"
        )


def _literal(text: str) -> int:
    if re.fullmatch(r"\$[0-9A-Fa-f]+", text):
        return int(text[1:], 16)
    if re.fullmatch(r"%[01]+", text):
        return int(text[1:], 2)
    if re.fullmatch(r"-?\d+", text):
        return int(text)
    raise ValueError(f"entity-presentation FX literal is not numeric: {text}")


def _closed_rows(value: object, required: set[str], *, name: str) -> list[dict[str, Any]]:
    if not isinstance(value, list) or not all(isinstance(row, dict) for row in value):
        raise ValueError(f"entity-presentation FX {name} container drift")
    if any(set(row) != required for row in value):
        raise ValueError(f"entity-presentation FX {name} record shape drift")
    return list(value)


def _source_section(source: str, symbol: str, *, chunk: bool = False) -> list[dict[str, Any]]:
    start = re.search(rf"^{re.escape(symbol)}:\s*$", source, re.MULTILINE)
    if start is None:
        raise ValueError(f"entity-presentation FX source section is missing: {symbol}")
    marker = (
        "; END OF FUNCTION CHUNK FOR csc22_animateEntityFadeInOrOut"
        if chunk
        else f"; End of function {symbol}"
    )
    end = source.find(marker, start.end())
    if end < 0:
        raise ValueError(f"entity-presentation FX source section end is missing: {symbol}")
    first_line = source[: start.start()].count("\n")
    records: list[dict[str, Any]] = []
    for offset, raw in enumerate(source[start.start() : end].splitlines(), 1):
        instruction = re.sub(r"\s+", " ", raw.split(";", 1)[0].strip())
        if instruction and not instruction.endswith(":"):
            records.append({"instruction": instruction, "sourceLine": first_line + offset})
    return records


def _h1_section(listing: str, symbol: str, *, chunk: bool = False) -> list[dict[str, Any]]:
    start = re.search(rf"^[0-9A-F]{{8}}\s+{re.escape(symbol)}:\s*$", listing, re.MULTILINE)
    if start is None:
        raise ValueError(f"entity-presentation FX H1 section is missing: {symbol}")
    marker = (
        "; END OF FUNCTION CHUNK FOR csc22_animateEntityFadeInOrOut"
        if chunk
        else f"; End of function {symbol}"
    )
    end = listing.find(marker, start.end())
    if end < 0:
        raise ValueError(f"entity-presentation FX H1 section end is missing: {symbol}")
    records: list[dict[str, Any]] = []
    for raw in listing[start.start() : end].splitlines():
        match = re.fullmatch(r"(?P<address>[0-9A-F]{8})\s+(?P<body>.*)", raw)
        if match is None:
            continue
        body = re.sub(r"^(?:[0-9A-F]{2,8}\s+)+", "", match["body"].split(";", 1)[0].strip())
        if body and not body.endswith(":"):
            records.append(
                {"address": int(match["address"], 16), "instruction": re.sub(r"\s+", " ", body)}
            )
    return records


def _assert_h1_source_identity(
    source_rows: list[dict[str, Any]], h1_rows: list[dict[str, Any]], name: str
) -> None:
    source = [re.sub(r"\s+", "", row["instruction"]) for row in source_rows]
    listing = [re.sub(r"\s+", "", row["instruction"]) for row in h1_rows]
    if source != listing:
        raise ValueError(f"entity-presentation FX H1/source instruction identity drift: {name}")


def _direct_calls(rows: list[dict[str, Any]]) -> list[dict[str, Any]]:
    calls = []
    for row in rows:
        match = re.fullmatch(
            r"(?P<opcode>bsr|jsr)(?:\.[bwls])?\s+"
            r"(?P<operand>\([A-Za-z_][A-Za-z0-9_]*\)|[A-Za-z_][A-Za-z0-9_]*)"
            r"(?:\.[bwl])?",
            row["instruction"],
        )
        if match is None:
            continue
        operand = match["operand"]
        calls.append(
            {
                "instruction": row["instruction"],
                "opcode": match["opcode"],
                "instructionTarget": operand[1:-1] if operand.startswith("(") else operand,
                "sourceLine": row["sourceLine"],
            }
        )
    return calls


def _parse_equates(upstream: Path, names: set[str]) -> dict[str, int]:
    values: dict[str, int] = {}
    for relative in (ENUMS_PATH, CONSTANTS_PATH):
        source = (upstream / relative).read_text(encoding="utf-8")
        for match in re.finditer(
            r"^(?P<name>[A-Za-z_][A-Za-z0-9_]*):\s+equ\s+"
            r"(?P<value>\$[0-9A-Fa-f]+|%[01]+|-?\d+)\b",
            source,
            re.MULTILINE,
        ):
            name = match["name"]
            if name not in names:
                continue
            value = _literal(match["value"])
            previous = values.setdefault(name, value)
            if previous != value:
                raise ValueError(f"entity-presentation FX constant conflict: {name}")
    missing = names - values.keys()
    if missing:
        raise ValueError(f"entity-presentation FX constants are missing: {sorted(missing)}")
    return {name: values[name] for name in sorted(names)}


def _instruction_index(rows: list[dict[str, Any]], instruction: str, *, occurrence: int = 0) -> int:
    matches = [index for index, row in enumerate(rows) if row["instruction"] == instruction]
    if occurrence >= len(matches):
        raise ValueError(f"entity-presentation FX source instruction is missing: {instruction}")
    return matches[occurrence]


def _h1_address(h1_rows: list[dict[str, Any]], instruction: str, *, occurrence: int = 0) -> int:
    matches = [row["address"] for row in h1_rows if row["instruction"] == instruction]
    if occurrence >= len(matches):
        raise ValueError(f"entity-presentation FX H1 instruction is missing: {instruction}")
    return matches[occurrence]


def _h1_next_address(
    h1_rows: list[dict[str, Any]], instruction: str, *, occurrence: int = 0
) -> int:
    matches = [index for index, row in enumerate(h1_rows) if row["instruction"] == instruction]
    if occurrence >= len(matches) or matches[occurrence] + 1 >= len(h1_rows):
        raise ValueError(f"entity-presentation FX H1 successor is missing: {instruction}")
    return h1_rows[matches[occurrence] + 1]["address"]


def _callback_records(
    source_rows: list[dict[str, Any]],
    h1_rows: list[dict[str, Any]],
    addresses: dict[str, int],
) -> list[dict[str, Any]]:
    calls = _direct_calls(source_rows)
    h1_calls = _direct_calls(
        [{"instruction": row["instruction"], "sourceLine": -1} for row in h1_rows]
    )
    if [(row["opcode"], row["instructionTarget"]) for row in calls] != [
        (row["opcode"], row["instructionTarget"]) for row in h1_calls
    ]:
        raise ValueError("entity-presentation FX H1 direct-call identity/order drift")
    result = []
    for index, call in enumerate(calls):
        call_site = _h1_address(
            h1_rows,
            call["instruction"],
            occurrence=sum(item["instruction"] == call["instruction"] for item in calls[:index]),
        )
        source_index = next(item for item, row in enumerate(h1_rows) if row["address"] == call_site)
        if source_index + 1 >= len(h1_rows):
            raise ValueError("entity-presentation FX H1 return address drift")
        target = call["instructionTarget"]
        if target not in addresses:
            raise ValueError(f"entity-presentation FX target symbol is missing: {target}")
        result.append(
            {
                **call,
                "effectiveTarget": target,
                "targetRole": "effective",
                "callSiteAddress": call_site,
                "targetAddress": addresses[target],
                "returnAddress": h1_rows[source_index + 1]["address"],
            }
        )
    return result


def _transition_table(
    source: str,
    listing_addresses: dict[str, int],
    rom_path: Path,
    *,
    record_byte_count: int,
) -> list[list[int]]:
    marker = "table_EntityFadingDefinitions:"
    start = source.find(marker)
    end = source.find("; START OF FUNCTION CHUNK", start)
    if start < 0 or end < 0:
        raise ValueError("entity-presentation FX transition table boundary drift")
    table: list[list[int]] = []
    record: list[int] = []
    for raw in source[start:end].splitlines():
        match = re.fullmatch(r"\s*dc\.w\s+(?P<value>\$[0-9A-Fa-f]+|-?\d+)\s*", raw)
        if match is not None:
            value = _literal(match["value"])
            record.append(value if value < 0x8000 else value - 0x10000)
            continue
        if record and (not raw.strip() or raw.lstrip().startswith(";")):
            table.append(record)
            record = []
    if record:
        table.append(record)
    if not table or any(2 * len(row) != record_byte_count for row in table):
        raise ValueError("entity-presentation FX transition table record boundary drift")
    address = listing_addresses.get("table_EntityFadingDefinitions")
    if address is None:
        raise ValueError("entity-presentation FX transition table H1 address is missing")
    rom = rom_path.read_bytes()
    expected = b"".join((value & 0xFFFF).to_bytes(2, "big") for row in table for value in row)
    if rom[address : address + len(expected)] != expected:
        raise ValueError("entity-presentation FX transition table source/ROM parity drift")
    return table


def _source_inputs(
    facts: dict[str, Any], constants: dict[str, int]
) -> tuple[list[dict[str, Any]], list[dict[str, Any]], list[dict[str, Any]]]:
    source_sites = _closed_rows(
        facts["sourceSites"], {"programId", "commands"}, name="source sites"
    )
    full_hash = (
        hashlib.sha256(_canonical_bytes({"sourceSites": facts["sourceSites"]})).hexdigest().upper()
    )
    if full_hash != facts["sourceSitesSha256"]:
        raise ValueError("entity-presentation FX complete H2 source-site hash drift")
    transition_rows, head_rows, flash_rows = [], [], []
    for site in source_sites:
        for command in site["commands"]:
            if set(command) != {
                "commandIndex",
                "sourceLine",
                "macro",
                "arguments",
                "sourceOrderKey",
                "operandValues",
            }:
                raise ValueError("entity-presentation FX source command shape drift")
            operands = _closed_rows(
                command["operandValues"],
                {
                    "parameterOrdinal",
                    "sourceComment",
                    "streamOffset",
                    "widthBytes",
                    "encoding",
                    "rawValue",
                    "resolvedValue",
                    "resolution",
                },
                name="source command operands",
            )
            source_input = {"programId": site["programId"], **command}
            if command["macro"] == "animEntityFX":
                if len(operands) != 2 or operands[1]["encoding"] != "shorthand:ENTITY_TRANSITION_":
                    raise ValueError("entity-presentation FX transition operand layout drift")
                suffix = operands[1]["rawValue"]
                selector = constants.get(f"ENTITY_TRANSITION_{suffix}")
                if selector is None:
                    raise ValueError("entity-presentation FX transition shorthand resolution drift")
                if operands[0]["resolvedValue"] is None:
                    raise ValueError("entity-presentation FX entity source input resolution drift")
                transition_rows.append(
                    {**source_input, "handlerInputWords": [operands[0]["resolvedValue"], selector]}
                )
            elif command["macro"] == "headshake":
                if len(operands) != 1 or operands[0]["resolvedValue"] is None:
                    raise ValueError("entity-presentation FX headshake operand layout drift")
                head_rows.append(
                    {**source_input, "handlerInputWords": [operands[0]["resolvedValue"]]}
                )
            elif command["macro"] == "entityFlashWhite":
                if len(operands) != 2 or any(row["resolvedValue"] is None for row in operands):
                    raise ValueError("entity-presentation FX flash operand layout drift")
                flash_rows.append(
                    {
                        **source_input,
                        "handlerInputWords": [
                            operands[0]["resolvedValue"],
                            operands[1]["resolvedValue"],
                        ],
                    }
                )
    expected_counts = {
        row["macro"]: row["sourceCommandCount"]
        for row in _closed_rows(
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
            name="H2 handler source counts",
        )
    }
    if {
        "animEntityFX": len(transition_rows),
        "headshake": len(head_rows),
        "entityFlashWhite": len(flash_rows),
    } != expected_counts:
        raise ValueError("entity-presentation FX complete H2 macro source-row inventory drift")
    return transition_rows, head_rows, flash_rows


def _special_transition_branches(
    rows: list[dict[str, Any]], constants: dict[str, int]
) -> list[dict[str, Any]]:
    """Parse the paired D1 setup/selector/chunk branches as one source relation."""
    branches = []
    for index, row in enumerate(rows[:-2]):
        setup = re.fullmatch(r"moveq #(?P<value>[^,]+),d1", row["instruction"])
        compare = re.fullmatch(r"cmpi\.w #(?P<selector>[^,]+),d0", rows[index + 1]["instruction"])
        if setup is None or compare is None or rows[index + 2]["instruction"] != "beq.w loc_46BE2":
            continue
        branches.append(
            {
                "selectorValue": _literal(compare["selector"]),
                "d1WordValue": _literal(setup["value"]) & 0xFFFF,
            }
        )
    expected_selectors = [constants[label] for label in SPECIAL_TRANSITION_SOURCE_LABELS]
    if [row["selectorValue"] for row in branches] != expected_selectors:
        raise ValueError("entity-presentation FX special-transition source branch relation drift")
    return branches


def _loop_counter(rows: list[dict[str, Any]], instruction: str) -> int:
    matches = [row["instruction"] for row in rows if row["instruction"] == instruction]
    if len(matches) != 1:
        raise ValueError(f"entity-presentation FX loop-counter use-site drift: {instruction}")
    immediate = re.fullmatch(r"moveq #(?P<value>[^,]+),d7", instruction)
    if immediate is None:
        raise ValueError(f"entity-presentation FX loop-counter syntax drift: {instruction}")
    return _literal(immediate["value"])


def _h2_facts(rom_path: Path, upstream: Path) -> tuple[dict[str, Any], dict[str, Any]]:
    fixture = load_json(H2_FIXTURE)
    h2 = build_map_script_engine_contract(rom_path, upstream)
    return fixture, h2["entityPresentationFxCommandFacts"]


def _assert_h2_fixture(fixture: dict[str, Any], facts: dict[str, Any]) -> None:
    compact = fixture["expected"]["entityPresentationFxCommandFacts"]
    if {key: facts[key] for key in compact} != compact:
        raise ValueError("entity-presentation FX H2 compact fixture/source drift")
    if facts["runtimeQuestions"] != ["map-script-entity-presentation-fx/runtime-effects-matrix"]:
        raise ValueError("entity-presentation FX H2 runtime-question handoff drift")
    return fixture, facts


def build_map_script_entity_presentation_fx_contract(
    rom_path: Path, upstream_path: Path
) -> dict[str, Any]:
    """Parse the whole bounded source surface before fixture comparison."""
    h2_fixture, facts = _h2_facts(rom_path, upstream_path)
    upstream = upstream_path.resolve(strict=True)
    source = (upstream / SOURCE_PATH).read_text(encoding="utf-8")
    map_setup_source = (upstream / MAP_SETUP_SOURCE_PATH).read_text(encoding="utf-8")
    listing = (upstream / H1_LISTING_PATH).read_text(encoding="utf-8")
    addresses = listing_symbol_addresses(listing)
    constants = _parse_equates(
        upstream,
        {
            "ENTITY_DATA",
            "ENTITYDEF_SIZE",
            "ENTITYDEF_OFFSET_ANIMCOUNTER",
            "ENTITYDEF_OFFSET_FLAGS_B",
            "ENTITY_TRANSITION_SCAN_UP",
            "ENTITY_TRANSITION_SCAN_DOWN",
            "ENTITY_TRANSITION_WIPE_OUT",
            "ENTITY_TRANSITION_WIPE_IN",
            "ENTITY_TRANSITION_SLIDE_OUT",
            "ENTITY_TRANSITION_SLIDE_IN",
            "ENTITY_TRANSITION_MOSAIC_OUT",
            "ENTITY_TRANSITION_MOSAIC_IN",
        },
    )
    needed = {
        *TARGET_IDENTITIES,
        *(handler for _, handler in HANDLER_FORMS),
        "RunMapSetupInitFunction",
        "loc_46BE2",
        "table_EntityFadingDefinitions",
    }
    if not needed <= addresses.keys():
        raise ValueError("entity-presentation FX H1 symbol inventory drift")
    h2_handlers = _closed_rows(
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
        name="H2 handlers",
    )
    if [(row["macro"], row["handler"]) for row in h2_handlers] != list(HANDLER_FORMS):
        raise ValueError("entity-presentation FX H2 handler identity/order drift")
    handler_records = []
    for h2_row in h2_handlers:
        symbol = h2_row["handler"]
        source_rows = _source_section(source, symbol)
        h1_rows = _h1_section(listing, symbol)
        _assert_h1_source_identity(source_rows, h1_rows, symbol)
        guard = h2_row["sectionGuard"]
        required_guard = {
            "orderedInstructions",
            "scriptCursorReadUseSites",
            "sourceImmediateUseSites",
            "sourceOperandInstructions",
            "branchRecords",
            "loopRecords",
            "directCallOrder",
            "returnInstruction",
        }
        if (
            set(guard) != required_guard
            or [row["instruction"] for row in source_rows] != guard["orderedInstructions"]
        ):
            raise ValueError(f"entity-presentation FX H2 named-section guard drift: {symbol}")
        calls = _callback_records(source_rows, h1_rows, addresses)
        if [(row["opcode"], row["instructionTarget"]) for row in calls] != [
            (row["opcode"], row["instructionTarget"]) for row in h2_row["directCalls"]
        ]:
            raise ValueError(f"entity-presentation FX H2 direct-call order drift: {symbol}")
        handler_records.append(
            {
                "macro": h2_row["macro"],
                "handler": symbol,
                "handlerAddress": h2_row["address"],
                "sourceRows": source_rows,
                "h1Rows": h1_rows,
                "guard": guard,
                "calls": calls,
            }
        )
    chunk_rows = _source_section(source, "loc_46BE2", chunk=True)
    chunk_h1 = _h1_section(listing, "loc_46BE2", chunk=True)
    _assert_h1_source_identity(chunk_rows, chunk_h1, "loc_46BE2")
    chunk_calls = _callback_records(chunk_rows, chunk_h1, addresses)
    entry_rows = _source_section(map_setup_source, "RunMapSetupInitFunction")
    entry_h1 = _h1_section(listing, "RunMapSetupInitFunction")
    _assert_h1_source_identity(entry_rows, entry_h1, "RunMapSetupInitFunction")
    entry_guard = [
        "movem.l d0-a1,-(sp)",
        "bsr.w GetCurrentMapSetup",
        "cmpi.w #-1,(a0)",
        "bne.s loc_4750E",
        "bra.w loc_47514",
        "movea.l MAPSETUP_OFFSET_INIT_FUNCTION(a0),a0",
        "jsr (a0)",
        "movem.l (sp)+,d0-a1",
        "rts",
    ]
    if [row["instruction"] for row in entry_rows] != entry_guard:
        raise ValueError("entity-presentation FX entry-injection named-section guard drift")
    entry_call_address = _h1_address(entry_h1, "jsr (a0)")
    entry_return_address = _h1_next_address(entry_h1, "jsr (a0)")
    transition_shift = _literal(
        re.fullmatch(
            r"lsl\.w #(?P<value>[^,]+),d0",
            _source_use(handler_records[0]["sourceRows"], "lsl.w #3,d0")["instruction"],
        )["value"]
    )
    transition_record_byte_count = 1 << transition_shift
    table = _transition_table(
        source, addresses, rom_path, record_byte_count=transition_record_byte_count
    )
    transitions, heads, flashes = _source_inputs(facts, constants)
    observed_transition_selectors = sorted({row["handlerInputWords"][1] for row in transitions})
    observed_flash_durations = sorted({row["handlerInputWords"][1] for row in flashes})
    special_transition_branches = _special_transition_branches(
        handler_records[0]["sourceRows"], constants
    )
    flash_shift = _literal(
        re.fullmatch(
            r"lsr\.w #(?P<value>[^,]+),d7",
            _source_use(handler_records[2]["sourceRows"], "lsr.w #2,d7")["instruction"],
        )["value"]
    )
    regular_loop_counter = _loop_counter(handler_records[0]["sourceRows"], "moveq #22,d7")
    headshake_loop_counter = _loop_counter(handler_records[1]["sourceRows"], "moveq #6,d7")
    special_loop_counter = _loop_counter(chunk_rows, "moveq #$F,d7")
    _assert_h2_fixture(h2_fixture, facts)
    return {
        "provenance": {
            "upstreamRepository": "https://github.com/ShiningForceCentral/SF2DISASM",
            "upstreamBranch": "master",
            "upstreamCommit": h2_fixture["upstreamCommit"],
            "h2FixturePath": "tests/fixtures/h2/map-script-engine-static-v1.json",
            "h2FixtureId": h2_fixture["id"],
            "h2FieldPath": "expected.entityPresentationFxCommandFacts",
            "command": "uv run sf2 h2 map-script-engine",
        },
        "romSha256": h2_fixture["romSha256"],
        "function": {
            "entryAddress": addresses["RunMapSetupInitFunction"],
            "entryInjectionCallSiteAddress": entry_call_address,
            "entryInjectionReturnAddress": entry_return_address,
            **{f"{row['handler']}Address": row["handlerAddress"] for row in handler_records},
            "loc_46BE2Address": addresses["loc_46BE2"],
            "csc22ReturnAddress": _h1_address(handler_records[0]["h1Rows"], "rts"),
            "loc_46BE2ReturnAddress": _h1_address(chunk_h1, "rts"),
            "csc27ReturnAddress": _h1_address(handler_records[1]["h1Rows"], "rts"),
            "csc18ReturnAddress": _h1_address(handler_records[2]["h1Rows"], "rts"),
            "csc22FirstOperandReadAfterAddress": _h1_next_address(
                handler_records[0]["h1Rows"], "move.w (a6)+,d0"
            ),
            "csc22SecondOperandReadAfterAddress": _h1_next_address(
                handler_records[0]["h1Rows"], "move.w (a6)+,d0", occurrence=1
            ),
            "csc22FirstCompareAddress": _h1_address(handler_records[0]["h1Rows"], "cmpi.w #6,d0"),
            "csc22SecondCompareAddress": _h1_address(handler_records[0]["h1Rows"], "cmpi.w #7,d0"),
            "csc22PostLoopCompareAddress": _h1_address(
                handler_records[0]["h1Rows"], "cmpi.w #4,d2"
            ),
            "csc22RegularLoopAddress": _h1_address(
                handler_records[0]["h1Rows"], "bsr.w LoadMapsprite"
            ),
            "csc22ChunkLoopAddress": _h1_address(chunk_h1, "bsr.w LoadMapsprite"),
            "csc22ChunkBitTestAddress": _h1_address(chunk_h1, "btst #$F,d1"),
            "csc22ChunkShiftAddress": _h1_address(chunk_h1, "lsr.l #1,d0"),
            "csc22ChunkAddAddress": _h1_address(chunk_h1, "add.l d0,d0"),
            "csc27FirstOperandReadAfterAddress": _h1_next_address(
                handler_records[1]["h1Rows"], "move.w (a6)+,d0"
            ),
            "csc27LoopAddress": _h1_address(handler_records[1]["h1Rows"], "bsr.w LoadMapsprite"),
            "csc27InitialAnimAfterWriteAddress": _h1_next_address(
                handler_records[1]["h1Rows"], "move.b #-1,ENTITYDEF_OFFSET_ANIMCOUNTER(a5)"
            ),
            "csc27FinalAnimAfterWriteAddress": _h1_next_address(
                handler_records[1]["h1Rows"], "move.b #0,ENTITYDEF_OFFSET_ANIMCOUNTER(a5)"
            ),
            "csc18FirstOperandReadAfterAddress": _h1_next_address(
                handler_records[2]["h1Rows"], "move.w (a6)+,d0"
            ),
            "csc18SecondOperandReadAfterAddress": _h1_next_address(
                handler_records[2]["h1Rows"], "move.w (a6)+,d7"
            ),
            "csc18LoopAddress": _h1_address(
                handler_records[2]["h1Rows"], "ori.b #%100,ENTITYDEF_OFFSET_FLAGS_B(a5)"
            ),
            "csc18SetFlagsAfterWriteAddress": _h1_next_address(
                handler_records[2]["h1Rows"], "ori.b #%100,ENTITYDEF_OFFSET_FLAGS_B(a5)"
            ),
            "csc18ClearFlagsAfterWriteAddress": _h1_next_address(
                handler_records[2]["h1Rows"], "andi.b #%11111011,ENTITYDEF_OFFSET_FLAGS_B(a5)"
            ),
        },
        "ram": {"entityDataAddress": constants["ENTITY_DATA"]},
        "constants": {
            "entityRecordByteCount": constants["ENTITYDEF_SIZE"],
            "animCounterByteOffset": constants["ENTITYDEF_OFFSET_ANIMCOUNTER"],
            "flagsBByteOffset": constants["ENTITYDEF_OFFSET_FLAGS_B"],
            "transitionTableRecordByteCount": transition_record_byte_count,
            "transitionTableIndexShiftCount": transition_shift,
            "entityTransitionValuesBySourceLabel": {
                label: constants[label]
                for label in (
                    "ENTITY_TRANSITION_SCAN_UP",
                    "ENTITY_TRANSITION_SCAN_DOWN",
                    "ENTITY_TRANSITION_WIPE_OUT",
                    "ENTITY_TRANSITION_WIPE_IN",
                    "ENTITY_TRANSITION_SLIDE_OUT",
                    "ENTITY_TRANSITION_SLIDE_IN",
                    *SPECIAL_TRANSITION_SOURCE_LABELS,
                )
            },
            "flashDurationShiftCount": flash_shift,
            "flashDurationDivisor": 1 << flash_shift,
            "regularTransitionLoopIterationCount": regular_loop_counter + 1,
            "headshakeLoopIterationCount": headshake_loop_counter + 1,
            "specialTransitionLoopIterationCount": special_loop_counter + 1,
        },
        "sourceContract": {
            "sourceSiteOrderKeys": facts["sourceSiteOrderKeys"],
            "sourceSitesSha256": facts["sourceSitesSha256"],
            "programTotalOrderKeys": facts["programTotalOrderKeys"],
            "programTotalsSha256": facts["programTotalsSha256"],
            "sourceObservedTransitionSelectorValues": observed_transition_selectors,
            "sourceObservedFlashDurationValues": observed_flash_durations,
            "sourceSpecialTransitionBranches": special_transition_branches,
        },
        "sourceFacts": {
            "handlers": handler_records,
            "transitionChunk": {"sourceRows": chunk_rows, "h1Rows": chunk_h1, "calls": chunk_calls},
            "entryInjection": {
                "sourceRows": entry_rows,
                "h1Rows": entry_h1,
                "callSiteAddress": entry_call_address,
                "returnAddress": entry_return_address,
            },
            "transitionTableSignedWords": table,
            "transitionSourceRows": transitions,
            "headshakeSourceRows": heads,
            "flashSourceRows": flashes,
            "callerTargetIdentities": list(TARGET_IDENTITIES),
        },
        "runtimeQuestions": RUNTIME_QUESTIONS,
    }


def _handler(static: dict[str, Any], macro: str) -> dict[str, Any]:
    matches = [row for row in static["sourceFacts"]["handlers"] if row["macro"] == macro]
    if len(matches) != 1:
        raise ValueError(f"entity-presentation FX handler identity drift: {macro}")
    return matches[0]


def _callback_shape(calls: list[dict[str, Any]]) -> list[dict[str, Any]]:
    return [
        {
            "instructionTarget": call["instructionTarget"],
            "effectiveTarget": call["effectiveTarget"],
            "callSiteAddress": call["callSiteAddress"],
            "targetAddress": call["targetAddress"],
            "returnAddress": call["returnAddress"],
            "targetRole": call["targetRole"],
        }
        for call in calls
    ]


def _callback_segment(calls: list[dict[str, Any]], repeat_count: int) -> dict[str, Any]:
    if repeat_count < 1 or not calls:
        raise ValueError("entity-presentation FX callback segment boundary drift")
    return {"repeatCount": repeat_count, "callbacks": _callback_shape(calls)}


def _expand_callback_segments(segments: list[dict[str, Any]]) -> list[dict[str, Any]]:
    expanded = []
    for segment in segments:
        if set(segment) != {"repeatCount", "callbacks"} or not isinstance(
            segment["repeatCount"], int
        ):
            raise ValueError("entity-presentation FX callback segment shape drift")
        for _ in range(segment["repeatCount"]):
            expanded.extend(segment["callbacks"])
    return expanded


def _observer_dispatch_plan(
    static: dict[str, Any],
    fixture: dict[str, Any],
    runtime_derived: list[dict[str, Any]],
    harness: dict[str, Any],
) -> list[dict[str, Any]]:
    """Build the one-callback-per-PC observer plan from parsed runtime facts."""
    phases_by_address: dict[int, set[str]] = {}

    def add(address: int, phase: str) -> None:
        if phase not in _OBSERVER_PHASE_ORDER:
            raise ValueError(f"entity-presentation FX unknown observer phase: {phase}")
        if not isinstance(address, int) or isinstance(address, bool):
            raise ValueError(f"entity-presentation FX observer phase address drift: {phase}")
        phases_by_address.setdefault(address, set()).add(phase)

    function = static["function"]
    for phase, field in (
        ("number-prompt", "numberPromptAddress"),
        ("flag-prompt", "flagPromptAddress"),
    ):
        add(harness["function"][field], phase)
    add(function["entryAddress"], "setup-case")
    for macro, handler in HANDLER_FORMS:
        add(function[f"{handler}Address"], f"handler-{macro}")
    for phase, field in (
        ("operand-csc22-first", "csc22FirstOperandReadAfterAddress"),
        ("operand-csc22-second", "csc22SecondOperandReadAfterAddress"),
        ("operand-csc27-first", "csc27FirstOperandReadAfterAddress"),
        ("operand-csc18-first", "csc18FirstOperandReadAfterAddress"),
        ("flash-duration-csc18-second", "csc18SecondOperandReadAfterAddress"),
        ("selector-first-compare", "csc22FirstCompareAddress"),
        ("selector-second-compare", "csc22SecondCompareAddress"),
        ("selector-post-loop-compare", "csc22PostLoopCompareAddress"),
        ("special-transition-d1-bit-test", "csc22ChunkBitTestAddress"),
        ("special-transition-shift", "csc22ChunkShiftAddress"),
        ("special-transition-add", "csc22ChunkAddAddress"),
        ("loop-anim-regular", "csc22RegularLoopAddress"),
        ("loop-anim-chunk", "csc22ChunkLoopAddress"),
        ("loop-headshake", "csc27LoopAddress"),
        ("loop-flash", "csc18LoopAddress"),
        ("field-headshake-anim-initial", "csc27InitialAnimAfterWriteAddress"),
        ("field-headshake-anim-final", "csc27FinalAnimAfterWriteAddress"),
        ("field-flash-flags-set", "csc18SetFlagsAfterWriteAddress"),
        ("field-flash-flags-clear", "csc18ClearFlagsAfterWriteAddress"),
        ("handler-return", "csc22ReturnAddress"),
        ("handler-return", "loc_46BE2ReturnAddress"),
        ("handler-return", "csc27ReturnAddress"),
        ("handler-return", "csc18ReturnAddress"),
    ):
        add(function[field], phase)
    for derived in runtime_derived:
        for callback in derived["directCallbackPlan"]:
            add(callback["returnAddress"], "callback-return")
            add(callback["callSiteAddress"], "callback-site")
    for hook in fixture["instrumentation"]["serviceInterception"]["entryHooks"]:
        add(hook["address"], "callback-target")
    add(fixture["instrumentation"]["postHandlerAddress"], "post-handler")
    phase_index = {phase: index for index, phase in enumerate(_OBSERVER_PHASE_ORDER)}
    return [
        {
            "address": address,
            "phases": sorted(phases, key=phase_index.__getitem__),
        }
        for address, phases in sorted(phases_by_address.items())
    ]


def _validate_observer_dispatch_plan(
    static: dict[str, Any],
    fixture: dict[str, Any],
    runtime_derived: list[dict[str, Any]],
    harness: dict[str, Any],
    observed: object,
) -> list[dict[str, Any]]:
    expected = _observer_dispatch_plan(static, fixture, runtime_derived, harness)
    if observed != expected:
        raise ValueError("entity-presentation FX observer dispatch plan drift")
    return expected


def _model_callback_dispatch_at_pc(
    *,
    address: int,
    phases: list[str],
    pending_callback: dict[str, Any] | None,
    next_callback: dict[str, Any] | None,
) -> tuple[dict[str, Any] | None, dict[str, Any] | None]:
    """Model the direct-callback state transition for one dispatcher PC.

    A return address may also be the following call-site address.  The model
    makes the required return-before-site transition executable without
    relying on BizHawk's callback ordering.
    """
    completed: dict[str, Any] | None = None
    pending = pending_callback
    for phase in phases:
        if phase == "callback-return":
            if pending is None or pending["returnAddress"] != address:
                raise ValueError("entity-presentation FX callback return model drift")
            completed, pending = pending, None
        elif phase == "callback-site":
            if pending is not None:
                raise ValueError("entity-presentation FX callback site model chronology drift")
            if next_callback is None or next_callback["callSiteAddress"] != address:
                raise ValueError("entity-presentation FX callback site model identity drift")
            pending = next_callback
    return completed, pending


def _compact_observed_callback_dispatches(
    callback_dispatches: list[dict[str, Any]],
    callback_patterns: list[dict[str, Any]],
    *,
    expected_event_count: int,
) -> list[dict[str, Any]]:
    """Derive compact repeat counts from the ordered observed callback stream.

    ``callback_patterns`` supplies only the source-derived callback identities
    that delimit each segment. Its configured repeat counts are deliberately
    not read: the count below comes from contiguous actual dispatch records.
    """
    if len(callback_dispatches) != expected_event_count:
        raise ValueError("entity-presentation FX observed callback event-count drift")

    def matches(observed: dict[str, Any], expected: dict[str, Any]) -> bool:
        return (
            observed["instructionTarget"] == expected["instructionTarget"]
            and observed["effectiveTarget"] == expected["effectiveTarget"]
            and observed["callSiteAddressObserved"] == expected["callSiteAddress"]
            and observed["targetRole"] == expected["targetRole"]
            and observed["targetAddressObserved"] == expected["targetAddress"]
            and observed["returnAddressObserved"] == expected["returnAddress"]
        )

    event_index = 0
    compact: list[dict[str, Any]] = []
    for segment in callback_patterns:
        callbacks = segment["callbacks"]
        if not callbacks:
            raise ValueError("entity-presentation FX observed callback pattern boundary drift")
        if event_index + len(callbacks) > len(callback_dispatches):
            raise ValueError("entity-presentation FX observed callback segment start drift")
        first_iteration = callback_dispatches[event_index : event_index + len(callbacks)]
        if not all(
            matches(observed, expected)
            for observed, expected in zip(first_iteration, callbacks, strict=True)
        ):
            raise ValueError(
                "entity-presentation FX observed callback segment identity/order drift"
            )

        repeat_count = 0
        while event_index + (repeat_count + 1) * len(callbacks) <= len(callback_dispatches):
            start = event_index + repeat_count * len(callbacks)
            iteration = callback_dispatches[start : start + len(callbacks)]
            if not all(
                matches(observed, expected)
                for observed, expected in zip(iteration, callbacks, strict=True)
            ):
                break
            repeat_count += 1
        if repeat_count == 0:
            raise ValueError("entity-presentation FX observed callback repeat boundary drift")
        compact.append(
            {
                "repeatCountObserved": repeat_count,
                "callbackSitesObserved": first_iteration,
            }
        )
        event_index += repeat_count * len(callbacks)
    if event_index != len(callback_dispatches):
        raise ValueError("entity-presentation FX observed callback segment event-count drift")
    return compact


def _source_use(rows: list[dict[str, Any]], instruction: str) -> dict[str, Any]:
    matches = [row for row in rows if row["instruction"] == instruction]
    if len(matches) != 1:
        raise ValueError(f"entity-presentation FX source use-site drift: {instruction}")
    return matches[0]


def _calls_for_range(
    rows: list[dict[str, Any]],
    calls: list[dict[str, Any]],
    start_instruction: str,
    end_instruction: str,
) -> list[dict[str, Any]]:
    start = _instruction_index(rows, start_instruction)
    end = _instruction_index(rows, end_instruction)
    if start >= end:
        raise ValueError("entity-presentation FX callback range order drift")
    occurrences: Counter[str] = Counter()
    indexes = []
    for call in calls:
        occurrence = occurrences[call["instruction"]]
        occurrences[call["instruction"]] += 1
        indexes.append(_instruction_index(rows, call["instruction"], occurrence=occurrence))
    return [call for call, index in zip(calls, indexes, strict=True) if start <= index < end]


def _source_case_input(static: dict[str, Any], case: dict[str, Any]) -> dict[str, Any]:
    source_rows_by_macro = {
        "animEntityFX": static["sourceFacts"]["transitionSourceRows"],
        "headshake": static["sourceFacts"]["headshakeSourceRows"],
        "entityFlashWhite": static["sourceFacts"]["flashSourceRows"],
    }
    rows = source_rows_by_macro.get(case["macro"])
    if rows is None:
        raise ValueError(f"entity-presentation FX source case macro drift: {case['macro']}")
    source_input = case["sourceInput"]
    matches = [row for row in rows if row["sourceOrderKey"] == source_input["sourceOrderKey"]]
    if len(matches) != 1 or matches[0] != source_input:
        raise ValueError("entity-presentation FX source case provenance drift")
    return matches[0]


def _expected_case(static: dict[str, Any], case: dict[str, Any]) -> dict[str, Any]:
    macro = case["macro"]
    handler = _handler(static, macro)
    source_input = _source_case_input(static, case)
    words = source_input["handlerInputWords"]
    cursor_after = case["instrumentation"]["scriptInputRamOffset"] + (2 * len(words))
    targets = static["sourceFacts"]["callerTargetIdentities"]
    if macro == "animEntityFX":
        selector = words[1]
        rows, calls = handler["sourceRows"], handler["calls"]
        loop_instruction = "dbf d7,@Transistion_Loop"
        loop_target_instruction = handler["guard"]["loopRecords"][0]["loopTarget"][
            "targetInstruction"
        ]
        if not re.fullmatch(r"(?:bsr|jsr)(?:\.[bwls])?\s+.+", loop_target_instruction):
            raise ValueError("entity-presentation FX regular-loop callback target syntax drift")
        regular = _calls_for_range(rows, calls, loop_target_instruction, loop_instruction)
        pre = _calls_for_range(rows, calls, rows[0]["instruction"], loop_target_instruction)
        post = _calls_for_range(rows, calls, "cmpi.w #4,d2", "rts")
        special_matches = [
            row
            for row in static["sourceContract"]["sourceSpecialTransitionBranches"]
            if row["selectorValue"] == selector
        ]
        if special_matches:
            if len(special_matches) != 1:
                raise ValueError("entity-presentation FX special-transition selector ambiguity")
            chunk = static["sourceFacts"]["transitionChunk"]
            chunk_rows, chunk_calls = chunk["sourceRows"], chunk["calls"]
            loop = _calls_for_range(
                chunk_rows, chunk_calls, "bsr.w LoadMapsprite", "dbf d7,loc_46BF2"
            )
            initial = _literal(
                re.fullmatch(
                    r"moveq #(?P<n>[^,]+),d7",
                    _source_use(chunk_rows, "moveq #$F,d7")["instruction"],
                )["n"]
            )
            callback_segments = [_callback_segment(pre, 1), _callback_segment(loop, initial + 1)]
            return_address = static["function"]["loc_46BE2ReturnAddress"]
            branch_d1 = special_matches[0]["d1WordValue"]
            branch = "transition-chunk-d1-zero" if branch_d1 == 0 else "transition-chunk-d1-nonzero"
            special = True
        else:
            initial = _literal(
                re.fullmatch(
                    r"moveq #(?P<n>[^,]+),d7", _source_use(rows, "moveq #22,d7")["instruction"]
                )["n"]
            )
            callback_segments = [_callback_segment(pre, 1), _callback_segment(regular, initial + 1)]
            if selector == _literal(
                re.fullmatch(
                    r"cmpi\.w #(?P<n>[^,]+),d2", _source_use(rows, "cmpi.w #4,d2")["instruction"]
                )["n"]
            ):
                callback_segments.append(_callback_segment(post, 1))
                branch = "transition-table-post-equality"
            else:
                branch = "transition-table-no-post-equality"
            return_address = static["function"]["csc22ReturnAddress"]
            branch_d1 = None
            special = False
        callback_plan = _expand_callback_segments(callback_segments)
        expected = {
            "id": case["id"],
            "macro": macro,
            "handlerAddress": handler["handlerAddress"],
            "handlerReturnAddress": return_address,
            "handlerInputWords": words,
            "scriptCursorRamOffsetAfter": cursor_after,
            "stackPointerDeltaBytesAfter": 0,
            "loopIterationCount": initial + 1,
            "callbackPlanSegments": callback_segments,
            "callbackSiteCounts": {
                target: sum(call["effectiveTarget"] == target for call in callback_plan)
                for target in targets
            },
            "branchPartition": branch,
            "specialTransitionD1Word": branch_d1,
            "sourceInput": source_input,
            "entityFieldPlan": None,
            "handlerUsesTransitionChunk": special,
        }
    elif macro == "headshake":
        rows, calls = handler["sourceRows"], handler["calls"]
        initial = _literal(
            re.fullmatch(
                r"moveq #(?P<n>[^,]+),d7", _source_use(rows, "moveq #6,d7")["instruction"]
            )["n"]
        )
        loop_target_instruction = handler["guard"]["loopRecords"][0]["loopTarget"][
            "targetInstruction"
        ]
        loop = _calls_for_range(rows, calls, loop_target_instruction, "dbf d7,loc_46CC8")
        callback_segments = [_callback_segment([calls[0]], 1), _callback_segment(loop, initial + 1)]
        callback_plan = _expand_callback_segments(callback_segments)
        expected = {
            "id": case["id"],
            "macro": macro,
            "handlerAddress": handler["handlerAddress"],
            "handlerReturnAddress": static["function"]["csc27ReturnAddress"],
            "handlerInputWords": words,
            "scriptCursorRamOffsetAfter": cursor_after,
            "stackPointerDeltaBytesAfter": 0,
            "loopIterationCount": initial + 1,
            "callbackPlanSegments": callback_segments,
            "callbackSiteCounts": {
                target: sum(call["effectiveTarget"] == target for call in callback_plan)
                for target in targets
            },
            "branchPartition": "headshake-fixed-loop",
            "specialTransitionD1Word": None,
            "sourceInput": source_input,
            "entityFieldPlan": {
                "field": "animCounterByte",
                "initialWriteValue": 255,
                "finalWriteValue": 0,
            },
            "handlerUsesTransitionChunk": False,
        }
    elif macro == "entityFlashWhite":
        rows, calls = handler["sourceRows"], handler["calls"]
        shift = _literal(
            re.fullmatch(
                r"lsr\.w #(?P<n>[^,]+),d7", _source_use(rows, "lsr.w #2,d7")["instruction"]
            )["n"]
        )
        if not 0 <= words[1] <= 0xFFFF:
            raise ValueError("entity-presentation FX flash duration word boundary drift")
        loop_count = (words[1] >> shift) + 1
        loop_target_instruction = handler["guard"]["loopRecords"][0]["loopTarget"][
            "targetInstruction"
        ]
        loop = _calls_for_range(rows, calls, loop_target_instruction, "dbf d7,loc_469E8")
        callback_segments = [_callback_segment([calls[0]], 1), _callback_segment(loop, loop_count)]
        callback_plan = _expand_callback_segments(callback_segments)
        expected = {
            "id": case["id"],
            "macro": macro,
            "handlerAddress": handler["handlerAddress"],
            "handlerReturnAddress": static["function"]["csc18ReturnAddress"],
            "handlerInputWords": words,
            "scriptCursorRamOffsetAfter": cursor_after,
            "stackPointerDeltaBytesAfter": 0,
            "loopIterationCount": loop_count,
            "callbackPlanSegments": callback_segments,
            "callbackSiteCounts": {
                target: sum(call["effectiveTarget"] == target for call in callback_plan)
                for target in targets
            },
            "branchPartition": "flash-duration-shifted-loop",
            "specialTransitionD1Word": None,
            "sourceInput": source_input,
            "entityFieldPlan": {"field": "flagsBByte", "setMask": 4, "clearMask": 251},
            "handlerUsesTransitionChunk": False,
        }
    else:
        raise ValueError(f"entity-presentation FX unknown case macro: {macro}")
    return expected


def derive_case_expectations(
    static: dict[str, Any], fixture: dict[str, Any]
) -> list[dict[str, Any]]:
    """Derive every summary from parsed source use sites before golden comparison."""
    source_contract = fixture["sourceContract"]
    if source_contract != static["sourceContract"]:
        raise ValueError("entity-presentation FX full H2 source-contract revalidation drift")
    derived = [
        _expected_case(static, {**case, "instrumentation": fixture["instrumentation"]})
        for case in fixture["cases"]
    ]
    if [case["expected"] for case in fixture["cases"]] != derived:
        raise ValueError("entity-presentation FX fixture/static expectation drift")
    return derived


def _service_interception(
    static: dict[str, Any], fixture: dict[str, Any], rom_path: Path
) -> list[dict[str, Any]]:
    interception = fixture["instrumentation"]["serviceInterception"]
    if set(interception) != {"patches", "entryHooks"}:
        raise ValueError("entity-presentation FX service interception shape drift")
    rows = _closed_rows(
        interception["patches"],
        {"targetIdentity", "targetRole", "address", "originalHex", "patchedHex"},
        name="service interception patches",
    )
    expected_addresses = {
        call["effectiveTarget"]: call["targetAddress"]
        for handler in static["sourceFacts"]["handlers"]
        for call in handler["calls"]
    }
    expected_addresses.update(
        {
            call["effectiveTarget"]: call["targetAddress"]
            for call in static["sourceFacts"]["transitionChunk"]["calls"]
        }
    )
    if set(expected_addresses) != set(TARGET_IDENTITIES):
        raise ValueError("entity-presentation FX complete target identity inventory drift")
    hooks = _closed_rows(
        interception["entryHooks"],
        {"targetIdentity", "targetRole", "address"},
        name="service entry hooks",
    )
    if [(row["targetIdentity"], row["targetRole"], row["address"]) for row in hooks] != [
        (target, "effective", expected_addresses[target]) for target in TARGET_IDENTITIES
    ]:
        raise ValueError("entity-presentation FX complete service entry-hook identity drift")
    data = rom_path.read_bytes()
    output = []
    for row in rows:
        if (
            row["targetRole"] != "effective"
            or row["targetIdentity"] not in expected_addresses
            or row["targetIdentity"] == "WaitForVInt"
        ):
            raise ValueError("entity-presentation FX service interception target role drift")
        if row["address"] != expected_addresses[row["targetIdentity"]]:
            raise ValueError("entity-presentation FX service interception target address drift")
        original, patched = bytes.fromhex(row["originalHex"]), bytes.fromhex(row["patchedHex"])
        if (
            not original
            or len(patched) > len(original)
            or data[row["address"] : row["address"] + len(original)] != original
        ):
            raise ValueError("entity-presentation FX service interception ROM byte drift")
        if row["targetIdentity"] == "GetEntityAddressFromCharacter":
            expected = (
                b"\x4b\xf9" + static["ram"]["entityDataAddress"].to_bytes(4, "big") + b"\x4e\x75"
            )
        else:
            expected = b"\x4e\x75"
        if patched != expected:
            raise ValueError("entity-presentation FX service interception shim drift")
        output.append({**row, "originalBytes": original, "patchedBytes": patched})
    if {row["targetIdentity"] for row in output} != set(TARGET_IDENTITIES) - {"WaitForVInt"}:
        raise ValueError("entity-presentation FX service interception completeness drift")
    return output


def _instrument_fx_rom(rom_path: Path, fixture: dict[str, Any], static: dict[str, Any]) -> Path:
    # WaitForVInt remains the original service because it participates in the
    # boot/menu cadence.  Every other bounded service entry is patched in the
    # session-only ROM and preflight-verified here, before Lua can register an
    # execute callback.
    if (
        fixture["instrumentation"]["callSiteAddress"]
        != static["function"]["entryInjectionCallSiteAddress"]
    ):
        raise ValueError("entity-presentation FX entry-injection call-site drift")
    base_rom = _instrument_rom(rom_path, fixture)
    patches = _service_interception(static, fixture, rom_path)
    data = bytearray(base_rom.read_bytes())
    for patch in patches:
        address, original = patch["address"], patch["originalBytes"]
        if data[address : address + len(original)] != original:
            raise ValueError("entity-presentation FX service/generic instrumentation overlap")
        data[address : address + len(patch["patchedBytes"])] = patch["patchedBytes"]
    data[0x18E:0x190] = int(mega_drive_checksum(bytes(data)), 16).to_bytes(2, "big")
    DERIVED_ROOT.mkdir(parents=True, exist_ok=True)
    output = DERIVED_ROOT / "map-script-entity-presentation-fx.instrumented.bin"
    output.write_bytes(data)
    return output


def _observer_cases(fixture: dict[str, Any]) -> list[dict[str, Any]]:
    return [
        {
            "id": case["id"],
            "macro": case["macro"],
            "handlerInputWords": case["sourceInput"]["handlerInputWords"],
        }
        for case in fixture["cases"]
    ]


def verify_map_script_entity_presentation_fx(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 180
) -> dict[str, Any]:
    """Run the bounded ten-case matrix in exactly one BizHawk launch."""
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner="entity-presentation FX runtime fixture")
    verify_runtime_contract(fixture, rom_path)
    static = build_map_script_entity_presentation_fx_contract(rom_path, upstream_path)
    for field in ("provenance", "romSha256", "function", "ram", "constants", "runtimeQuestions"):
        if fixture[field] != static[field]:
            raise ValueError(f"entity-presentation FX fixture/source identity drift: {field}")
    derived = derive_case_expectations(static, fixture)
    runtime_derived = [
        {**case, "directCallbackPlan": _expand_callback_segments(case["callbackPlanSegments"])}
        for case in derived
    ]
    harness = load_json(repo_path(fixture["sharedHarnessFixture"]))["harness"]
    dispatch_plan = _observer_dispatch_plan(static, fixture, runtime_derived, harness)
    instrumented = _instrument_fx_rom(rom_path, fixture, static)

    def observe() -> dict[str, Any]:
        return run_observer(
            rom_path=instrumented,
            observer_path=OBSERVER,
            config={
                "fixtureId": fixture["id"],
                "mapTestIndex": fixture["mapTestIndex"],
                "function": static["function"],
                "ram": static["ram"],
                "constants": static["constants"],
                "targetIdentities": static["sourceFacts"]["callerTargetIdentities"],
                "instrumentation": fixture["instrumentation"],
                "maxFrames": fixture["maxFrames"],
                "harness": harness,
                "cases": _observer_cases(fixture),
                "derived": runtime_derived,
                "outputDerived": derived,
                "observerDispatchPlan": dispatch_plan,
                "observerFailureContract": OBSERVER_FAILURE_CONTRACT,
            },
            output_name=OBSERVER_OUTPUT_NAME,
            timeout_seconds=timeout_seconds,
        )

    try:
        observed = _with_instrumented_rom_database(
            instrumented, "SF2 H3 instrumented entity-presentation FX", observe
        )
    except RuntimeError as error:
        failure = _callback_failure_status(_observer_status_path())
        if failure is None:
            raise
        _observer_output_path().unlink(missing_ok=True)
        raise RuntimeError(
            f"{error}\nEntity-presentation FX callback failure status:\n"
            f"{json.dumps(failure, sort_keys=True)}"
        ) from error
    _raise_for_callback_failure_status(_observer_status_path(), _observer_output_path())
    validate_json(observed, OBSERVATION_SCHEMA, owner="entity-presentation FX runtime observation")
    for runtime_case, observed_case in zip(runtime_derived, observed["records"], strict=True):
        observed_dispatches = [
            callback
            for segment in observed_case["callbackPlanSegmentsObserved"]
            for _ in range(segment["repeatCountObserved"])
            for callback in segment["callbackSitesObserved"]
        ]
        recomputed_segments = _compact_observed_callback_dispatches(
            observed_dispatches,
            runtime_case["callbackPlanSegments"],
            expected_event_count=len(runtime_case["directCallbackPlan"]),
        )
        if recomputed_segments != observed_case["callbackPlanSegmentsObserved"]:
            raise ValueError("entity-presentation FX observed callback compaction drift")
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
            "entity-presentation FX runtime matrix mismatch\n"
            f"static={derived!r}\nexpected={expected!r}\nobserved={observed!r}"
        )
    return {
        "Fixture": fixture["id"],
        "Cases": len(derived),
        "Handlers": len({case["handlerAddress"] for case in derived}),
        "BizHawkLaunches": 1,
        "Instrumentation": (
            "eight session-only service-entry shims; "
            "WaitForVInt original entry/body/return executes "
            "(semantics unclaimed)"
        ),
        "Status": "PASS",
    }
