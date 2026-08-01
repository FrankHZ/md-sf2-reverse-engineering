"""Bounded runtime observation for the source-named ``cloneEntity`` command.

The H2 contract owns the complete source corpus.  This H3 rail deliberately
observes only the named handler's two word inputs, two lookup calls, and one
source-named byte transfer.  It does not assign an entity lifecycle or a
whole-record meaning to that byte transfer.
"""

from __future__ import annotations

import hashlib
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
from sf2tool.source_text import read_upstream_text

H2_FIXTURE = repo_path("tests/fixtures/h2/map-script-engine-static-v1.json")
FIXTURE = repo_path("tests/fixtures/h3/map-script-entity-clone-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h3-map-script-entity-clone-fixture.schema.json")
OBSERVATION_SCHEMA = repo_path("schemas/h3-map-script-entity-clone-observation.schema.json")
OBSERVER = repo_path("tools/bizhawk/map_script_entity_clone_observer.lua")

H1_LISTING_PATH = Path("build/sf2build-h1.lst")
SOURCE_PATH = Path("disasm/code/common/scripting/map/mapscriptengine_1.asm")
ENUMS_PATH = Path("disasm/sf2enums.asm")
CONSTANTS_PATH = Path("disasm/sf2const.asm")

HANDLER = "csc25_cloneEntity"
LOOKUP = "GetEntityAddressFromCharacter"
RUNTIME_QUESTIONS = (
    "map-script-entity-clone/further-runtime-state-matrix",
    "map-script-entity-clone/further-runtime-external-consumer-matrix",
    "map-script-entity-clone/further-runtime-context-matrix",
)
HANDLER_INSTRUCTIONS = (
    "move.w (a6)+,d0",
    "bsr.w GetEntityAddressFromCharacter",
    "move.b ENTITYDEF_OFFSET_ENTNUM(a5),d1",
    "move.w (a6)+,d0",
    "bsr.w GetEntityAddressFromCharacter",
    "move.b d1,ENTITYDEF_OFFSET_ENTNUM(a5)",
    "rts",
)
LOOKUP_INSTRUCTIONS = (
    "lea ((ENTITY_INDEX_LIST-$1000000)).w,a5",
    "andi.w #COMBATANT_MASK_ALL,d0",
    "tst.b d0",
    "bpl.s @Ally",
    "subi.b #ENTITY_ENEMY_INDEX_DIFFERENCE,d0",
    "move.b (a5,d0.w),d0",
    "move.l d0,-(sp)",
    "lsl.w #ENTITYDEF_SIZE_BITS,d0",
    "lea ((ENTITY_DATA-$1000000)).w,a5",
    "adda.w d0,a5",
    "move.l (sp)+,d0",
    "rts",
)
WIDTHS = {"b": 1, "w": 2, "l": 4}


def _literal(text: str) -> int:
    value = text.strip()
    if re.fullmatch(r"\$[0-9A-Fa-f]+", value):
        return int(value[1:], 16)
    if re.fullmatch(r"%[01]+", value):
        return int(value[1:], 2)
    if re.fullmatch(r"-?\d+", value):
        return int(value)
    raise ValueError(f"map-script entity clone source literal is invalid: {text}")


def _instruction_width(instruction: str) -> int:
    match = re.fullmatch(r"[a-z]+\.(?P<size>[bwl])\s+.+", instruction)
    if match is None:
        raise ValueError(
            f"map-script entity clone instruction has no transfer width: {instruction}"
        )
    return WIDTHS[match.group("size")]


def _source_section(source: str, symbol: str) -> list[dict[str, Any]]:
    """Parse one named, comment-stripped source section with line provenance."""
    start = re.search(rf"^{re.escape(symbol)}:\s*$", source, re.MULTILINE)
    if start is None:
        raise ValueError(f"map-script entity clone source section is missing: {symbol}")
    end = source.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"map-script entity clone source section end is missing: {symbol}")
    initial_line = source[: start.start()].count("\n")
    rows: list[dict[str, Any]] = []
    for offset, raw in enumerate(source[start.start() : end].splitlines(), 1):
        instruction = re.sub(r"\s+", " ", raw.split(";", 1)[0].strip())
        if instruction and not instruction.endswith(":"):
            rows.append({"instruction": instruction, "sourceLine": initial_line + offset})
    return rows


def _exact_section(source: str, symbol: str, expected: tuple[str, ...]) -> list[dict[str, Any]]:
    rows = _source_section(source, symbol)
    actual = tuple(row["instruction"] for row in rows)
    if actual != expected:
        raise ValueError(f"map-script entity clone source guard drift: {symbol}")
    return rows


def _parse_equates(upstream_path: Path, names: set[str]) -> dict[str, int]:
    """Read each authoritative numeric constant once, rejecting conflicting owners."""
    values: dict[str, int] = {}
    for relative in (CONSTANTS_PATH, ENUMS_PATH):
        source = read_upstream_text(upstream_path / relative)
        for match in re.finditer(
            r"^(?P<name>[A-Za-z_][A-Za-z0-9_]*):\s+equ\s+"
            r"(?P<value>\$[0-9A-Fa-f]+|%[01]+|-?\d+)\b",
            source,
            re.MULTILINE,
        ):
            name = match.group("name")
            if name not in names:
                continue
            value = _literal(match.group("value"))
            prior = values.setdefault(name, value)
            if prior != value:
                raise ValueError(f"map-script entity clone source equate conflict: {name}")
    missing = sorted(names - values.keys())
    if missing:
        raise ValueError(f"map-script entity clone source equates are missing: {missing}")
    return values


def _listing_function_instructions(listing: str, symbol: str) -> list[dict[str, Any]]:
    start = re.search(rf"^[0-9A-F]{{8}}\s+{re.escape(symbol)}:\s*$", listing, re.MULTILINE)
    if start is None:
        raise ValueError(f"map-script entity clone H1 function is missing: {symbol}")
    end = listing.find(f"; End of function {symbol}", start.end())
    if end < 0:
        raise ValueError(f"map-script entity clone H1 function end is missing: {symbol}")
    rows: list[dict[str, Any]] = []
    for raw in listing[start.start() : end].splitlines():
        match = re.fullmatch(r"(?P<address>[0-9A-F]{8})\s+(?P<body>.*)", raw)
        if match is None:
            continue
        body = match.group("body").split(";", 1)[0].strip()
        body = re.sub(r"^(?:[0-9A-F]{2,8}\s+)+", "", body).strip()
        if not body or body.endswith(":"):
            continue
        instruction = re.sub(r"\s+", " ", body)
        if re.fullmatch(r"[A-Za-z][A-Za-z0-9_]*(?:\.[bwls])?(?:\s+.+)?", instruction) is None:
            raise ValueError(f"map-script entity clone H1 instruction parse drift: {raw}")
        rows.append({"address": int(match.group("address"), 16), "instruction": instruction})
    return rows


def _handler_h1_addresses(listing: str) -> dict[str, Any]:
    rows = _listing_function_instructions(listing, HANDLER)
    if tuple(row["instruction"] for row in rows) != HANDLER_INSTRUCTIONS:
        raise ValueError("map-script entity clone H1 handler instruction/order drift")
    return {
        "handlerEntryAddress": rows[0]["address"],
        "sourceOperandReadAddress": rows[0]["address"],
        "destinationOperandReadAddress": rows[3]["address"],
        "sourceLookupCallSiteAddress": rows[1]["address"],
        "destinationLookupCallSiteAddress": rows[4]["address"],
        "sourceLookupReturnAddress": rows[2]["address"],
        "destinationLookupReturnAddress": rows[5]["address"],
        "sourceFieldReadAddress": rows[2]["address"],
        "destinationFieldWriteAddress": rows[5]["address"],
        "handlerRtsAddress": rows[6]["address"],
    }


def _lookup_guard(
    source: str, equates: dict[str, int]
) -> tuple[list[dict[str, Any]], dict[str, Any]]:
    rows = _exact_section(source, LOOKUP, LOOKUP_INSTRUCTIONS)
    if _instruction_width(rows[1]["instruction"]) != 2:
        raise ValueError("map-script entity clone lookup mask width drift")
    if (
        _instruction_width(rows[2]["instruction"]) != 1
        or _instruction_width(rows[4]["instruction"]) != 1
    ):
        raise ValueError("map-script entity clone lookup signed-byte control drift")
    if rows[3]["instruction"] != "bpl.s @Ally":
        raise ValueError("map-script entity clone lookup branch polarity drift")
    if rows[7]["instruction"] != "lsl.w #ENTITYDEF_SIZE_BITS,d0":
        raise ValueError("map-script entity clone lookup stride use-site drift")
    stride = 1 << equates["ENTITYDEF_SIZE_BITS"]
    if stride != equates["ENTITYDEF_SIZE"]:
        raise ValueError("map-script entity clone lookup stride/equate cross-check drift")
    return rows, {
        "lookupMask": equates["COMBATANT_MASK_ALL"],
        "lookupIndexDifference": equates["ENTITY_ENEMY_INDEX_DIFFERENCE"],
        "lookupIndexTransferByteCount": _instruction_width(rows[5]["instruction"]),
        "entityRecordByteCount": stride,
        "entnumByteOffset": equates["ENTITYDEF_OFFSET_ENTNUM"],
        "lookupMaskUseSite": rows[1],
        "lookupSignedTestUseSite": rows[2],
        "lookupPositiveBranchUseSite": rows[3],
        "lookupDifferenceUseSite": rows[4],
        "lookupIndexReadUseSite": rows[5],
        "lookupStrideUseSite": rows[7],
    }


def _handler_guard(source: str, equates: dict[str, int]) -> list[dict[str, Any]]:
    rows = _exact_section(source, HANDLER, HANDLER_INSTRUCTIONS)
    cursor_rows = (rows[0], rows[3])
    if any(_instruction_width(row["instruction"]) != 2 for row in cursor_rows):
        raise ValueError("map-script entity clone A6 word-read width drift")
    if (
        _instruction_width(rows[2]["instruction"]) != 1
        or _instruction_width(rows[5]["instruction"]) != 1
    ):
        raise ValueError("map-script entity clone named byte-transfer width drift")
    return rows


def _flatten_source_inputs(facts: dict[str, Any]) -> list[dict[str, Any]]:
    rows: list[dict[str, Any]] = []
    for site in facts.get("sourceSites", []):
        if set(site) != {"programId", "commands"}:
            raise ValueError("map-script entity clone H2 source-site shape drift")
        for command in site["commands"]:
            required = {
                "commandIndex",
                "sourceLine",
                "macro",
                "arguments",
                "sourceOrderKey",
                "operandValues",
            }
            if set(command) != required or command["macro"] != "cloneEntity":
                raise ValueError("map-script entity clone H2 command identity drift")
            operands = command["operandValues"]
            if (
                not isinstance(operands, list)
                or len(operands) != 2
                or [row.get("parameterOrdinal") for row in operands] != [1, 2]
                or any(row.get("widthBytes") != 2 for row in operands)
                or any(row.get("resolution") != "literal" for row in operands)
            ):
                raise ValueError("map-script entity clone H2 operand layout drift")
            words = [row.get("resolvedValue") for row in operands]
            if not all(isinstance(word, int) and 0 <= word <= 0xFFFF for word in words):
                raise ValueError("map-script entity clone H2 operand word boundary drift")
            rows.append(
                {
                    "sourceOrderKey": command["sourceOrderKey"],
                    "programId": site["programId"],
                    "commandIndex": command["commandIndex"],
                    "sourceLine": command["sourceLine"],
                    "sourceWords": words,
                }
            )
    if len(rows) != 9:
        raise ValueError("map-script entity clone complete source command denominator drift")
    return rows


def _lookup_index_byte_offset(word: int, constants: dict[str, Any]) -> int:
    """Derive the original lookup byte index from its guarded mask/test/subtract sequence."""
    if not isinstance(word, int) or not 0 <= word <= 0xFFFF:
        raise ValueError(f"map-script entity clone lookup word boundary drift: {word}")
    masked = word & constants["lookupMask"]
    sign_bit = 1 << (constants["lookupIndexTransferByteCount"] * 8 - 1)
    return masked if not (masked & sign_bit) else masked - constants["lookupIndexDifference"]


def _compact_h2_boundary(facts: dict[str, Any], compact: dict[str, Any]) -> dict[str, Any]:
    fields = (
        "macros",
        "sourceSiteOrderKeys",
        "sourceSitesSha256",
        "programTotalsSha256",
        "callerBreakdown",
        "sourceIdentityJoins",
        "runtimeQuestions",
    )
    if set(compact) != set(fields) | {"handlers", "programTotalOrderKeys"}:
        raise ValueError("map-script entity clone compact H2 field inventory drift")
    actual = {field: facts[field] for field in fields}
    expected = {field: compact[field] for field in fields}
    if actual != expected:
        raise ValueError("map-script entity clone H2 compact fixture/source drift")
    return actual


def _canonical_sha256(value: object) -> str:
    import json

    return (
        hashlib.sha256((json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode("utf-8"))
        .hexdigest()
        .upper()
    )


def build_map_script_entity_clone_contract(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    """Construct a source-guarded H3 seam before a golden fixture is considered."""
    upstream = upstream_path.resolve(strict=True)
    equates = _parse_equates(
        upstream,
        {
            "COMBATANT_MASK_ALL",
            "ENTITY_ENEMY_INDEX_DIFFERENCE",
            "ENTITYDEF_OFFSET_ENTNUM",
            "ENTITYDEF_SIZE",
            "ENTITYDEF_SIZE_BITS",
            "ENTITY_DATA",
            "ENTITY_INDEX_LIST",
        },
    )
    source = read_upstream_text(upstream / SOURCE_PATH)
    handler_rows = _handler_guard(source, equates)
    lookup_rows, constants = _lookup_guard(source, equates)
    constants["entnumTransferByteCount"] = _instruction_width(handler_rows[5]["instruction"])
    listing = read_upstream_text(upstream / H1_LISTING_PATH)
    function = _handler_h1_addresses(listing)
    addresses = listing_symbol_addresses(listing)
    required_symbols = {HANDLER, LOOKUP, "RunMapSetupInitFunction"}
    if not required_symbols <= addresses.keys():
        raise ValueError("map-script entity clone H1 symbol inventory drift")
    if function["handlerEntryAddress"] != addresses[HANDLER]:
        raise ValueError("map-script entity clone H1 handler entry drift")
    function["lookupEntryAddress"] = addresses[LOOKUP]
    function["runMapSetupInitFunctionAddress"] = addresses["RunMapSetupInitFunction"]

    h2_fixture = load_json(H2_FIXTURE)
    facts = build_map_script_engine_contract(rom_path, upstream)["entityCloneCommandFacts"]
    compact = h2_fixture["expected"]["entityCloneCommandFacts"]
    compact_boundary = _compact_h2_boundary(facts, compact)
    if tuple(facts["runtimeQuestions"]) != RUNTIME_QUESTIONS:
        raise ValueError("map-script entity clone H2 runtime-question refinement drift")
    source_inputs = _flatten_source_inputs(facts)
    if facts["sourceSiteOrderKeys"] != [row["sourceOrderKey"] for row in source_inputs]:
        raise ValueError("map-script entity clone H2 source-order derivation drift")

    return {
        "evidenceDate": "2026-08-01",
        "provenance": {
            "upstreamRepository": "https://github.com/ShiningForceCentral/SF2DISASM",
            "upstreamBranch": "master",
            "upstreamCommit": h2_fixture["upstreamCommit"],
            "h2FixturePath": "tests/fixtures/h2/map-script-engine-static-v1.json",
            "h2FixtureId": h2_fixture["id"],
            "h2FieldPath": "expected.entityCloneCommandFacts",
            "command": "uv run sf2 h2 map-script-engine",
        },
        "romSha256": h2_fixture["romSha256"],
        "function": function,
        "ram": {
            "entityDataAddress": equates["ENTITY_DATA"],
            "entityIndexListAddress": equates["ENTITY_INDEX_LIST"],
        },
        "constants": constants,
        "sourceFacts": {
            "compactH2Boundary": {
                **compact_boundary,
                "sourceInputRowsSha256": _canonical_sha256(source_inputs),
            },
            "handlerUseSites": handler_rows,
            "lookupUseSites": lookup_rows,
            "evidenceLabels": {
                "staticFindings": "Confirmed",
                "runtimeObservations": "Confirmed",
            },
        },
        "runtimeQuestions": list(facts["runtimeQuestions"]),
    }


def _case_expected(case: dict[str, Any], static: dict[str, Any]) -> dict[str, Any]:
    source_input = case["sourceInput"]
    words = source_input["sourceWords"]
    if not isinstance(words, list) or len(words) != 2:
        raise ValueError(f"map-script entity clone case word arity drift: {case['id']}")
    controls = case["harnessControls"]
    required_controls = {
        "sourceRecordIndexSeed",
        "destinationRecordIndexSeed",
        "sourceEntnumByteSeed",
        "destinationEntnumByteSeed",
        "destinationAdjacentByteSeeds",
    }
    if set(controls) != required_controls:
        raise ValueError(f"map-script entity clone harness-control shape drift: {case['id']}")
    source_index = controls["sourceRecordIndexSeed"]
    destination_index = controls["destinationRecordIndexSeed"]
    if (
        not isinstance(source_index, int)
        or not isinstance(destination_index, int)
        or source_index == destination_index
        or source_index < 0
        or destination_index < 0
    ):
        raise ValueError(f"map-script entity clone distinct record-control drift: {case['id']}")
    source_value = controls["sourceEntnumByteSeed"]
    destination_before = controls["destinationEntnumByteSeed"]
    if (
        not isinstance(source_value, int)
        or not isinstance(destination_before, int)
        or not 0 <= source_value <= 0xFF
        or not 0 <= destination_before <= 0xFF
        or source_value == destination_before
    ):
        raise ValueError(f"map-script entity clone byte-boundary control drift: {case['id']}")
    adjacent = controls["destinationAdjacentByteSeeds"]
    offset = static["constants"]["entnumByteOffset"]
    if (
        not isinstance(adjacent, list)
        or [row.get("byteOffset") for row in adjacent] != [offset - 1, offset + 1]
        or any(set(row) != {"byteOffset", "byteValue"} for row in adjacent)
        or any(
            not isinstance(row["byteValue"], int) or not 0 <= row["byteValue"] <= 0xFF
            for row in adjacent
        )
    ):
        raise ValueError(f"map-script entity clone adjacent-byte control drift: {case['id']}")
    cursor_before = case["scriptCursorRamOffsetBefore"]
    word_read_widths = [
        _instruction_width(row["instruction"])
        for row in static["sourceFacts"]["handlerUseSites"]
        if row["instruction"] == "move.w (a6)+,d0"
    ]
    if word_read_widths != [2, 2]:
        raise ValueError("map-script entity clone source cursor use-site derivation drift")
    cursor_advance = sum(word_read_widths)
    lookup_offsets = [_lookup_index_byte_offset(word, static["constants"]) for word in words]
    function = static["function"]
    expected = {
        "id": case["id"],
        "sourceOrderKey": source_input["sourceOrderKey"],
        "handlerEntryPc": function["handlerEntryAddress"],
        "handlerRtsPc": function["handlerRtsAddress"],
        "scriptCursorRamOffsetBefore": cursor_before,
        "scriptCursorRamOffsetAfter": cursor_before + cursor_advance,
        "cursorAdvanceByteCountObserved": cursor_advance,
        "operandReads": [
            {
                "ordinal": ordinal,
                "instructionPc": (
                    function["sourceOperandReadAddress"]
                    if ordinal == 1
                    else function["destinationOperandReadAddress"]
                ),
                "a6RamOffsetBefore": cursor_before + (ordinal - 1) * word_read_widths[ordinal - 1],
                "wordObserved": word,
            }
            for ordinal, word in enumerate(words, 1)
        ],
        "lookupCallSequence": [
            {
                "ordinal": ordinal,
                "callSitePc": (
                    function["sourceLookupCallSiteAddress"]
                    if ordinal == 1
                    else function["destinationLookupCallSiteAddress"]
                ),
                "targetEntryPc": function["lookupEntryAddress"],
                "returnPc": (
                    function["sourceLookupReturnAddress"]
                    if ordinal == 1
                    else function["destinationLookupReturnAddress"]
                ),
                "lookupIndexByteOffsetObserved": lookup_offsets[ordinal - 1],
            }
            for ordinal in (1, 2)
        ],
        "sourceEntnumRead": {
            "instructionPc": function["sourceFieldReadAddress"],
            "recordIndexObserved": source_index,
            "byteOffset": offset,
            "byteValueObserved": source_value,
        },
        "destinationEntnumWrite": {
            "instructionPc": function["destinationFieldWriteAddress"],
            "recordIndexObserved": destination_index,
            "byteOffset": offset,
            "byteValueBeforeObserved": destination_before,
            "byteValueAfterObserved": source_value,
        },
        "destinationAdjacentBytes": [
            {
                "byteOffset": row["byteOffset"],
                "byteValueBeforeObserved": row["byteValue"],
                "byteValueAfterObserved": row["byteValue"],
            }
            for row in adjacent
        ],
        "handlerReturned": True,
    }
    if case["expected"] is not None and case["expected"] != expected:
        raise ValueError(f"map-script entity clone fixture/static disagreement: {case['id']}")
    return expected


def derive_case_expectations(
    static: dict[str, Any], fixture: dict[str, Any]
) -> list[dict[str, Any]]:
    """Derive every result from guarded use sites and case controls before golden comparison."""
    if fixture["sourceContract"] != static["sourceFacts"]["compactH2Boundary"]:
        raise ValueError("map-script entity clone fixture/source compact-boundary drift")
    cases = fixture["cases"]
    source_inputs = [case["sourceInput"] for case in cases]
    if [row["sourceOrderKey"] for row in source_inputs] != static["sourceFacts"][
        "compactH2Boundary"
    ]["sourceSiteOrderKeys"]:
        raise ValueError("map-script entity clone complete case/source-order drift")
    if (
        _canonical_sha256(source_inputs)
        != static["sourceFacts"]["compactH2Boundary"]["sourceInputRowsSha256"]
    ):
        raise ValueError("map-script entity clone case/source-input hash drift")
    derived = [_case_expected(case, static) for case in cases]
    if [case["expected"] for case in cases] != derived:
        raise ValueError("map-script entity clone complete fixture/static disagreement")
    return derived


def _instrument_entity_clone_rom(rom_path: Path, fixture: dict[str, Any]) -> Path:
    """Build the bounded session-only trampoline after preflighting original bytes."""
    original_hash = inspect_rom(rom_path.resolve(strict=True))["sha256"]
    data = bytearray(rom_path.read_bytes())
    patch = fixture["instrumentation"]
    call_site = patch["callSiteAddress"]
    stub_address = patch["stubAddress"]
    original_call = bytes.fromhex(patch["callSiteOriginalHex"])
    patched_call = bytes.fromhex(patch["callSitePatchedHex"])
    original_stub = bytes.fromhex(patch["stubOriginalHex"])
    stub = bytes.fromhex(patch["stubHex"])
    if data[call_site : call_site + len(original_call)] != original_call:
        raise ValueError("map-script entity clone trampoline call-site original-byte drift")
    if data[stub_address : stub_address + len(original_stub)] != original_stub:
        raise ValueError("map-script entity clone trampoline padding original-byte drift")
    if patched_call != b"\x4e\xb9" + stub_address.to_bytes(4, "big"):
        raise ValueError("map-script entity clone trampoline call shape drift")
    if len(stub) > len(original_stub):
        raise ValueError("map-script entity clone trampoline exceeds preflight padding")
    if patch["postHandlerAddress"] != stub_address + len(stub) - 2:
        raise ValueError("map-script entity clone trampoline return PC drift")
    data[call_site : call_site + len(patched_call)] = patched_call
    data[stub_address : stub_address + len(stub)] = stub
    data[0x18E:0x190] = int(mega_drive_checksum(bytes(data)), 16).to_bytes(2, "big")
    if inspect_rom(rom_path.resolve(strict=True))["sha256"] != original_hash:
        raise ValueError("map-script entity clone instrumentation altered the original ROM")
    DERIVED_ROOT.mkdir(parents=True, exist_ok=True)
    output = DERIVED_ROOT / "map-script-entity-clone.instrumented.bin"
    output.write_bytes(data)
    return output


def _observer_cases(fixture: dict[str, Any]) -> list[dict[str, Any]]:
    """Pass controls and source identities only; runtime results are never observer config."""
    return [
        {
            "id": case["id"],
            "sourceInput": case["sourceInput"],
            "scriptCursorRamOffsetBefore": case["scriptCursorRamOffsetBefore"],
            "harnessControls": case["harnessControls"],
        }
        for case in fixture["cases"]
    ]


def verify_map_script_entity_clone(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 180
) -> dict[str, Any]:
    """Run all nine source-ordered clone command inputs in one Map Test 0 session."""
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner="map-script entity clone fixture")
    verify_runtime_contract(fixture, rom_path)
    static = build_map_script_entity_clone_contract(rom_path, upstream_path)
    for field in (
        "evidenceDate",
        "provenance",
        "romSha256",
        "function",
        "ram",
        "constants",
        "sourceFacts",
        "runtimeQuestions",
    ):
        if fixture[field] != static[field]:
            raise ValueError(f"map-script entity clone fixture/source identity drift: {field}")
    derived = derive_case_expectations(static, fixture)
    instrumented_rom = _instrument_entity_clone_rom(rom_path, fixture)

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
                "cases": _observer_cases(fixture),
            },
            output_name="map-script-entity-clone",
            timeout_seconds=timeout_seconds,
        )

    observed = _with_instrumented_rom_database(
        instrumented_rom, "SF2 H3 instrumented map-script entity clone", observe
    )
    validate_json(observed, OBSERVATION_SCHEMA, owner="map-script entity clone observation")
    expected = {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["id"],
        "mapTest": fixture["mapTestIndex"],
        "recordOrder": [case["id"] for case in fixture["cases"]],
        "records": derived,
    }
    if observed != expected:
        raise ValueError(
            "map-script entity clone runtime matrix mismatch\n"
            f"static={static!r}\nexpected={expected!r}\nobserved={observed!r}"
        )
    return {
        "Fixture": fixture["id"],
        "Cases": len(derived),
        "Handlers": 1,
        "LookupCalls": sum(len(row["lookupCallSequence"]) for row in derived),
        "BizHawkLaunches": 1,
        "Instrumentation": "session-only trampoline; original lookup body executes",
        "Status": "PASS",
    }
