"""Static caller and branch-shape contract for ``DisplayTacticalBaseQuote``.

The owner deliberately records only source, H1, and canonical-ROM facts.  In
particular, the apparent quote selection is not promoted into a decoded-text or
runtime contract: the selected caller, HP, flag state, service completion, and
presentation all remain in the fixture's explicit Unknown queue.
"""

from __future__ import annotations

import hashlib
import json
import re
from copy import deepcopy
from pathlib import Path
from typing import Any

from sf2tool.h2.map_event_direct_handoff import _h1_instruction_rows, _normalise_statement
from sf2tool.h2.map_event_random_battle_state import (
    canonical_json_bytes,
    normalize_map_event_random_battle_state_later_owner_index,
)
from sf2tool.h2.map_events_fixture import load_map_events_fixture
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path

ID = "sf2-map-event-tactical-base-quote-state-static-v1"
FIXTURE = repo_path("tests/fixtures/h2/map-event-tactical-base-quote-state-static-v1.json")
SCHEMA = repo_path("schemas/h2/map-event-tactical-base-quote-state-static-fixture.schema.json")
_ROM_SHA256 = "9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9"
_UPSTREAM_COMMIT = "c834c652b6862bc5679fd7f69a38a7093206efc6"
_PREDECESSOR_INDEX_SHA256 = "C905CB82A2C310AAAC8A4B40BA7D14BC5750BB4EE9D59AABBF5E68069042630B"
_FUNCTION_PATH = "code/common/scripting/map/headquartersfunctions.asm"
_MAP37_PATH = "data/maps/entries/map37/mapsetups/s2_entityevents.asm"
_MAP46_PATH = "data/maps/entries/map46/mapsetups/s2_entityevents.asm"
_SOURCE_PATHS = (
    "sf2enums.asm",
    "code/common/tech/jumpinterfaces/s02_jumpinterface.asm",
    "code/common/tech/jumpinterfaces/s03_jumpinterface_2.asm",
    "code/common/stats/combatantstats_1.asm",
    "code/common/stats/gameflags.asm",
    "code/common/menus/nameunderportraitwindow.asm",
    "code/common/scripting/text/textfunctions_1.asm",
    _FUNCTION_PATH,
    _MAP37_PATH,
    _MAP46_PATH,
)
_UNKNOWN_KEYS = (
    "naturalProgramReachability",
    "selectedMapAndCaller",
    "runtimeAllySelector",
    "runtimeCurrentHp",
    "livingBranchTaken",
    "runtimeActivePartyFlag",
    "activeOrReserveBranchTaken",
    "runtimeQuoteLineId",
    "textAndNameWindowServiceCompletion",
    "callerTailReturnAndControlOrder",
    "stateLifetimeAndSaveLoadPersistence",
    "inputDialogueAudioPresentationTimingAndStoryMeaning",
)
_FUNCTION_ADDRESSES = (
    0x4790E,
    0x47914,
    0x4791A,
    0x4791C,
    0x4791E,
    0x47922,
    0x47924,
    0x47926,
    0x4792A,
    0x47930,
    0x47932,
    0x47936,
    0x47938,
    0x4793C,
    0x47940,
    0x47946,
)
_FUNCTION_STATEMENTS = (
    "jsr j_OpenNameUnderPortraitWindow",
    "jsr j_GetCurrentHp",
    "tst.w d1",
    "bne.s @LivingMember",
    "move.w #1,d0",
    "bra.s @DisplayQuote",
    "move.w d0,d1",
    "addi.w #FORCEMEMBER_ACTIVE_FLAGS_START,d1",
    "jsr j_CheckFlag",
    "beq.s @InReserve",
    "addi.w #$DC3,d0",
    "bra.s @DisplayQuote",
    "addi.w #$DE1,d0",
    "jsr (DisplayText).w",
    "jsr j_CloseNameUnderPortraitWindow",
    "rts",
)
_SERVICES = (
    (
        "j_OpenNameUnderPortraitWindow",
        0x100AC,
        "OpenNameUnderPortraitWindow",
        0x169AE,
        "code/common/tech/jumpinterfaces/s03_jumpinterface_2.asm",
        "code/common/menus/nameunderportraitwindow.asm",
    ),
    (
        "j_CloseNameUnderPortraitWindow",
        0x100B0,
        "CloseNameUnderPortraitWindow",
        0x16A30,
        "code/common/tech/jumpinterfaces/s03_jumpinterface_2.asm",
        "code/common/menus/nameunderportraitwindow.asm",
    ),
    (
        "j_GetCurrentHp",
        0x8048,
        "GetCurrentHp",
        0x8336,
        "code/common/tech/jumpinterfaces/s02_jumpinterface.asm",
        "code/common/stats/combatantstats_1.asm",
    ),
    (
        "j_CheckFlag",
        0x8264,
        "CheckFlag",
        0x98B4,
        "code/common/tech/jumpinterfaces/s02_jumpinterface.asm",
        "code/common/stats/gameflags.asm",
    ),
    (
        "DisplayText",
        0x6260,
        "DisplayText",
        0x6260,
        "code/common/scripting/text/textfunctions_1.asm",
        "code/common/scripting/text/textfunctions_1.asm",
    ),
)
_ALLY_VALUES = {
    "ALLY_SARAH": 1,
    "ALLY_CHESTER": 2,
    "ALLY_JAHA": 3,
    "ALLY_KAZIN": 4,
    "ALLY_SLADE": 5,
    "ALLY_KIWI": 6,
    "ALLY_PETER": 7,
    "ALLY_MAY": 8,
    "ALLY_GERHALT": 9,
    "ALLY_LUKE": 10,
    "ALLY_ROHDE": 11,
    "ALLY_RICK": 12,
    "ALLY_ELRIC": 13,
    "ALLY_ERIC": 14,
    "ALLY_KARNA": 15,
    "ALLY_RANDOLF": 16,
    "ALLY_TYRIN": 17,
    "ALLY_JANET": 18,
    "ALLY_HIGINS": 19,
    "ALLY_SKREECH": 20,
    "ALLY_TAYA": 21,
    "ALLY_FRAYJA": 22,
    "ALLY_JARO": 23,
    "ALLY_GYAN": 24,
    "ALLY_SHEELA": 25,
    "ALLY_ZYNK": 26,
    "ALLY_CHAZ": 27,
    "ALLY_LEMON": 28,
    "ALLY_CLAUDE": 29,
}
_NON_ROSTER_ALLY_VALUES = {
    "ALLY_BOWIE": 0,
    "ALLY_SADJOIN": 32768,
    "ALLY_MASK_INDEX": 31,
}
_RETAINED_OWNER_FIXTURES = (
    ("mapEvents", "sf2-map-events-static-v1", "tests/fixtures/h2/map-events-static-v1.json"),
    (
        "directControl",
        "sf2-map-event-direct-control-static-v1",
        "tests/fixtures/h2/map-event-direct-control-static-v1.json",
    ),
    (
        "directHandoff",
        "sf2-map-event-direct-handoff-static-v1",
        "tests/fixtures/h2/map-event-direct-handoff-static-v1.json",
    ),
    (
        "commonScripting",
        "sf2-common-scripting-static-v1",
        "tests/fixtures/h2/common-scripting-static-v1.json",
    ),
    ("commonMenus", "sf2-common-menus-static-v1", "tests/fixtures/h2/common-menus-static-v1.json"),
    ("commonStats", "sf2-common-stats-static-v1", "tests/fixtures/h2/common-stats-static-v1.json"),
    (
        "techInterfaces",
        "sf2-tech-interfaces-static-v1",
        "tests/fixtures/h2/tech-interfaces-static-v1.json",
    ),
    ("textBanks", "sf2-text-banks-static-v1", "tests/fixtures/h2/text-banks-static-v1.json"),
)
_EQUATE = re.compile(r"^\s*([A-Za-z_][A-Za-z0-9_]*)\s*:\s+equ\s+([^\s;]+)")
_LABEL = re.compile(r"^\s*([A-Za-z_@][A-Za-z0-9_@]*)\s*:\s*(?:;.*)?$")


def _root(upstream_path: Path) -> Path:
    root = upstream_path.resolve(strict=True)
    return root / "disasm" if (root / "disasm").is_dir() else root


def _fixture_digest(path: str) -> str:
    return hashlib.sha256(repo_path(path).read_bytes()).hexdigest().upper()


def _sha(value: bytes) -> str:
    return hashlib.sha256(value).hexdigest().upper()


def _anchor(
    address: int,
    statement: str,
    h1_rows: dict[int, tuple[bytes, str]],
    rom: bytes,
    *,
    source_path: str,
    source_line: int,
    expected_target_address: int | None = None,
    anchor_order: int,
) -> dict[str, Any]:
    encoded, h1_statement = h1_rows.get(address, (b"", ""))
    if not encoded or _normalise_statement(statement) != h1_statement:
        raise ValueError(f"tactical-base quote source/H1 statement drift: {address:#x}")
    rom_encoded = rom[address : address + len(encoded)]
    relocation_offset = _pc_relative_operand_offset(encoded)
    if relocation_offset is not None and any(encoded[relocation_offset:]):
        raise ValueError(f"tactical-base quote H1 relocation placeholder drift: {address:#x}")
    rom_matches = len(rom_encoded) == len(encoded) and (
        rom_encoded == encoded
        or (
            relocation_offset is not None
            and rom_encoded[:relocation_offset] == encoded[:relocation_offset]
        )
    )
    if not rom_matches:
        raise ValueError(f"tactical-base quote H1/ROM anchor drift: {address:#x}")
    resolved_target = _effective_target(address, rom_encoded)
    if relocation_offset is not None and expected_target_address is None:
        raise ValueError(f"tactical-base quote unowned PC-relative target: {address:#x}")
    if expected_target_address is not None and resolved_target != expected_target_address:
        raise ValueError(f"tactical-base quote effective target drift: {address:#x}")
    shape = _instruction_shape(address, source_line, statement)
    return {
        "anchorOrder": anchor_order,
        "address": address,
        "sourcePath": source_path,
        "sourceLine": source_line,
        "mnemonic": shape["mnemonic"],
        "sizeSuffix": shape["sizeSuffix"],
        "operands": shape["operands"],
        "controlFlowKind": shape["controlFlowKind"],
        "expectedTargetAddress": expected_target_address,
        "h1ListedByteCount": len(encoded),
        "romEncodedByteCount": len(rom_encoded),
        "sourceInstructionSha256": _sha(_normalise_statement(statement).encode("utf-8")),
        "h1InstructionSha256": _sha(encoded),
        "h1StatementSha256": _sha(h1_statement.encode()),
        "romInstructionSha256": _sha(rom_encoded),
    }


def _pc_relative_operand_offset(encoded: bytes) -> int | None:
    if len(encoded) == 2 and encoded[0] & 0xF0 == 0x60:
        return 1
    if len(encoded) == 4 and encoded[:2] in {b"\x4e\xfa", b"\x4e\xba"}:
        return 2
    return None


def _effective_target(address: int, encoded: bytes) -> int | None:
    """Resolve each direct or PC-relative target form in the owned anchor corpus."""
    if len(encoded) == 2 and encoded[0] & 0xF0 == 0x60:
        return address + 2 + int.from_bytes(encoded[1:], byteorder="big", signed=True)
    if len(encoded) == 4 and encoded[:2] in {b"\x4e\xfa", b"\x4e\xba"}:
        return address + 2 + int.from_bytes(encoded[2:], byteorder="big", signed=True)
    if len(encoded) == 4 and encoded[:2] in {b"\x4e\xb8", b"\x4e\xf8"}:
        return int.from_bytes(encoded[2:], byteorder="big")
    if len(encoded) == 6 and encoded[:2] in {b"\x4e\xb9", b"\x4e\xf9"}:
        return int.from_bytes(encoded[2:], byteorder="big")
    return None


def _source_statement_after_label(lines: list[str], symbol: str) -> tuple[int, str]:
    matches = [
        index
        for index, line in enumerate(lines)
        if _LABEL.match(line) and _LABEL.match(line).group(1) == symbol
    ]
    if len(matches) != 1:
        raise ValueError(f"tactical-base quote service label drift: {symbol}")
    for line_no, line in enumerate(lines[matches[0] + 1 :], start=matches[0] + 2):
        statement = _normalise_statement(line)
        if (
            statement
            and statement not in {"module", "endmodule"}
            and not statement.startswith(";")
            and not _LABEL.match(line)
        ):
            return line_no, statement
    raise ValueError(f"tactical-base quote service body drift: {symbol}")


def _function_rows(lines: list[str]) -> tuple[list[tuple[int, str]], dict[str, int]]:
    start = next(
        (
            index
            for index, line in enumerate(lines)
            if _LABEL.match(line) and _LABEL.match(line).group(1) == "DisplayTacticalBaseQuote"
        ),
        None,
    )
    if start is None:
        raise ValueError("tactical-base quote function label drift")
    rows: list[tuple[int, str]] = []
    label_indexes: dict[str, int] = {}
    for line_no, raw in enumerate(lines[start + 1 :], start + 2):
        if "End of function DisplayTacticalBaseQuote" in raw:
            break
        label = _LABEL.match(raw)
        if label:
            if label.group(1) in label_indexes:
                raise ValueError("tactical-base quote function label duplicate drift")
            label_indexes[label.group(1)] = len(rows)
            continue
        statement = _normalise_statement(raw)
        if not statement or statement.startswith(";"):
            continue
        rows.append((line_no, statement))
    if tuple(statement for _, statement in rows) != _FUNCTION_STATEMENTS:
        raise ValueError("tactical-base quote function source body drift")
    label_addresses = {}
    for label, index in label_indexes.items():
        if index >= len(_FUNCTION_ADDRESSES):
            raise ValueError("tactical-base quote function label target drift")
        label_addresses[label] = _FUNCTION_ADDRESSES[index]
    return rows, label_addresses


def _parse_ally_equates(lines: list[str]) -> dict[str, int]:
    values: dict[str, int] = {}
    for raw in lines:
        match = _EQUATE.match(raw)
        if match is None or not match.group(1).startswith("ALLY_"):
            continue
        symbol = match.group(1)
        token = match.group(2)
        if token.startswith("$"):
            value = int(token[1:], 16)
        elif token.isdecimal():
            value = int(token)
        else:
            raise ValueError("tactical-base quote ally alias drift")
        if symbol in _ALLY_VALUES:
            if symbol in values:
                raise ValueError("tactical-base quote ally duplicate drift")
            values[symbol] = value
        elif _NON_ROSTER_ALLY_VALUES.get(symbol) != value:
            raise ValueError("tactical-base quote ally enumeration drift")
    if list(values) != list(_ALLY_VALUES) or values != _ALLY_VALUES:
        raise ValueError("tactical-base quote ally enumeration drift")
    return values


def _instruction_shape(address: int, source_line: int, statement: str) -> dict[str, Any]:
    opcode, _, operand_text = statement.partition(" ")
    mnemonic, separator, size_suffix = opcode.partition(".")
    if mnemonic == "jsr":
        control_flow = "direct-call"
    elif mnemonic == "jmp" or mnemonic == "bra":
        control_flow = "direct-jump"
    elif mnemonic.startswith("b"):
        control_flow = "conditional-branch"
    elif mnemonic == "rts":
        control_flow = "return"
    else:
        control_flow = "fallthrough"
    return {
        "address": address,
        "sourceLine": source_line,
        "mnemonic": mnemonic,
        "sizeSuffix": size_suffix or None if separator else None,
        "operands": operand_text.split(",") if operand_text else [],
        "controlFlowKind": control_flow,
    }


def _public_operation(
    context_id: str, operation_order: int, operation: dict[str, Any]
) -> dict[str, Any]:
    target = operation["target"]
    return {
        "operationOrder": operation_order,
        "operationId": f"{context_id}:{operation['address']:06X}",
        "address": operation["address"],
        "sourceLine": operation["sourceLine"],
        "mnemonic": operation["mnemonic"],
        "sizeSuffix": operation["sizeSuffix"],
        "operands": operation["operandTexts"],
        "controlFlowKind": operation["controlFlowKind"],
        "target": None
        if target is None
        else {
            "instructionTarget": target["instructionTargetSymbol"],
            "instructionTargetAddress": target["instructionTargetAddress"],
            "effectiveTarget": target["effectiveTargetSymbol"],
            "effectiveTargetAddress": target["effectiveTargetAddress"],
        },
    }


def _function_expected_target(
    statement: str,
    label_addresses: dict[str, int],
    service_entries: dict[str, Any],
) -> int | None:
    shape = _instruction_shape(0, 1, statement)
    if shape["controlFlowKind"] not in {"direct-call", "direct-jump", "conditional-branch"}:
        return None
    if len(shape["operands"]) != 1:
        raise ValueError("tactical-base quote function target operand drift")
    symbol = shape["operands"][0].strip()
    match = re.fullmatch(r"\(([^()]+)\)\.(?:b|w|l)", symbol)
    if match is not None:
        symbol = match.group(1)
    if symbol in label_addresses:
        return label_addresses[symbol]
    service = service_entries.get(symbol)
    if service is None:
        raise ValueError("tactical-base quote function target symbol drift")
    return service["instructionTargetAddress"]


def _validate_order(output: dict[str, Any]) -> None:
    expected = [
        "schemaVersion",
        "id",
        "upstream",
        "romSha256",
        "retainedOwners",
        "sourceContext",
        "tacticalBaseQuoteState",
        "unknowns",
        "summary",
    ]
    if list(output) != expected:
        raise ValueError("tactical-base quote root order drift")
    source_context = output["sourceContext"]
    if list(source_context) != [
        "h1Listing",
        "motherFixture",
        "sourceIdentities",
        "callerAnchors",
        "functionAnchors",
        "retainedServiceAnchors",
    ]:
        raise ValueError("tactical-base quote source context order drift")
    if [row["path"] for row in source_context["sourceIdentities"]] != list(_SOURCE_PATHS):
        raise ValueError("tactical-base quote source identity order drift")
    facts = output["tacticalBaseQuoteState"]
    expected_facts = [
        "sourceFileOrder",
        "sourceFiles",
        "programContextOrder",
        "programContexts",
        "physicalProgramOrder",
        "physicalPrograms",
        "allySelectorOrder",
        "allySelectors",
        "serviceEntryOrder",
        "serviceEntries",
        "functionFlow",
        "quoteBranchOrder",
        "quoteBranches",
        "quoteLineDomain",
        "digests",
    ]
    if list(facts) != expected_facts:
        raise ValueError("tactical-base quote fact order drift")
    for records, order in (
        ("sourceFiles", "sourceFileOrder"),
        ("programContexts", "programContextOrder"),
        ("physicalPrograms", "physicalProgramOrder"),
        ("serviceEntries", "serviceEntryOrder"),
        ("quoteBranches", "quoteBranchOrder"),
    ):
        if list(facts[records]) != facts[order]:
            raise ValueError(f"tactical-base quote {records} order drift")
    if facts["allySelectors"]["values"] != list(range(1, 30)):
        raise ValueError("tactical-base quote ally selector order drift")
    if len(facts["programContextOrder"]) != 54 or len(facts["physicalProgramOrder"]) != 54:
        raise ValueError("tactical-base quote caller record order drift")
    if [
        facts["programContexts"][key]["contextOrder"] for key in facts["programContextOrder"]
    ] != list(range(54)) or [
        facts["physicalPrograms"][key]["physicalOrder"] for key in facts["physicalProgramOrder"]
    ] != list(range(54)):
        raise ValueError("tactical-base quote caller ordinal drift")
    for key in facts["programContextOrder"]:
        context = facts["programContexts"][key]
        physical = facts["physicalPrograms"].get(key)
        if (
            physical is None
            or context["physicalProgramId"] != key
            or [row["operationOrder"] for row in context["operations"]] != [0, 1]
            or context["operationIds"] != [row["operationId"] for row in context["operations"]]
        ):
            raise ValueError("tactical-base quote caller record linkage drift")
    if {row["map"] for row in facts["programContexts"].values()} != {37, 46} or {
        row["map"] for row in facts["physicalPrograms"].values()
    } != {37, 46}:
        raise ValueError("tactical-base quote caller map partition drift")
    for group, expected_count in (
        ("callerAnchors", 108),
        ("functionAnchors", 16),
        ("retainedServiceAnchors", 9),
    ):
        anchors = output["sourceContext"][group]
        if len(anchors) != expected_count or [row["anchorOrder"] for row in anchors] != list(
            range(expected_count)
        ):
            raise ValueError(f"tactical-base quote {group} order drift")
    all_anchors = [
        anchor
        for group in ("callerAnchors", "functionAnchors", "retainedServiceAnchors")
        for anchor in output["sourceContext"][group]
    ]
    if len(all_anchors) != 133 or len({row["address"] for row in all_anchors}) != 133:
        raise ValueError("tactical-base quote anchor identity drift")
    if [
        row["expectedTargetAddress"]
        for row in source_context["callerAnchors"]
        if row["controlFlowKind"] == "direct-jump"
    ] != [0x4790E] * 54:
        raise ValueError("tactical-base quote caller target inventory drift")
    if [
        row["expectedTargetAddress"]
        for row in source_context["functionAnchors"]
        if row["expectedTargetAddress"] is not None
    ] != [0x100AC, 0x8048, 0x47924, 0x4793C, 0x8264, 0x47938, 0x4793C, 0x6260, 0x100B0]:
        raise ValueError("tactical-base quote function target inventory drift")
    if [
        row["expectedTargetAddress"]
        for row in source_context["retainedServiceAnchors"]
        if row["expectedTargetAddress"] is not None
    ] != [0x169AE, 0x16A30, 0x8336, 0x98B4]:
        raise ValueError("tactical-base quote service target inventory drift")


def _program_key(path: str, symbol: str) -> str:
    prefix = "map37" if path == _MAP37_PATH else "map46"
    event = re.fullmatch(rf"Map{prefix[3:]}_EntityEvent(\d+)", symbol)
    if event is None:
        raise ValueError("tactical-base quote caller symbol drift")
    return f"{prefix}-event{event.group(1)}"


def _retained_owners() -> dict[str, Any]:
    owners: dict[str, Any] = {}
    for key, fixture_id, fixture_path in _RETAINED_OWNER_FIXTURES:
        payload = load_json(repo_path(fixture_path))
        if payload.get("id") != fixture_id:
            raise ValueError(f"tactical-base quote retained {key} identity drift")
        owners[key] = {"fixtureId": fixture_id, "fixtureSha256": _fixture_digest(fixture_path)}
    return owners


def build_map_event_tactical_base_quote_state_contract(
    rom_path: Path, upstream_path: Path, *, map_events_override: dict[str, Any] | None = None
) -> dict[str, Any]:
    """Build the closed static tactical-base quote contract from immutable inputs."""
    rom = rom_path.resolve(strict=True).read_bytes()
    if _sha(rom) != _ROM_SHA256:
        raise ValueError("tactical-base quote ROM identity drift")
    root = _root(upstream_path)
    listing_bytes = (root.parent / "build" / "sf2build-h1.lst").read_bytes()
    listing = listing_bytes.decode("utf-8")
    h1_rows = _h1_instruction_rows(listing)
    source_bytes = {path: (root / path).read_bytes() for path in _SOURCE_PATHS}
    source_text = {path: value.decode("utf-8") for path, value in source_bytes.items()}
    function_rows, function_label_addresses = _function_rows(
        source_text[_FUNCTION_PATH].splitlines()
    )
    if len(function_rows) != 16 or len(_FUNCTION_ADDRESSES) != 16:
        raise ValueError("tactical-base quote function denominator drift")
    ally_values = _parse_ally_equates(source_text["sf2enums.asm"].splitlines())
    map_events = map_events_override or load_map_events_fixture()["expected"]
    all_programs = (
        map_events["entityTargetPrograms"]
        + map_events["zoneTargetPrograms"]
        + map_events["itemTargetPrograms"]
    )
    if len(all_programs) != 914:
        raise ValueError("tactical-base quote mother corpus drift")
    positives = [
        program
        for program in map_events["entityTargetPrograms"]
        if program["sourcePath"] in {_MAP37_PATH, _MAP46_PATH}
        and len(program["operations"]) == 2
        and program["operations"][1]["target"] is not None
        and program["operations"][1]["target"]["effectiveTargetSymbol"]
        == "DisplayTacticalBaseQuote"
    ]
    if len(positives) != 54:
        raise ValueError("tactical-base quote positive caller denominator drift")

    expected_map37 = {f"Map37_EntityEvent{number}" for number in (*range(24), 25)}
    expected_map46 = {f"Map46_EntityEvent{number}" for number in range(29)}
    by_path = {_MAP37_PATH: [], _MAP46_PATH: []}
    for program in positives:
        source_path = program["sourcePath"]
        first, tail = program["operations"]
        if (
            first["mnemonic"],
            first["operandTexts"][1:] if len(first["operandTexts"]) == 2 else [],
            tail["mnemonic"],
            tail["controlFlowKind"],
            tail["target"]["instructionTargetSymbol"],
            tail["target"]["instructionTargetAddress"],
            tail["target"]["effectiveTargetAddress"],
        ) != (
            "moveq",
            ["d0"],
            "jmp",
            "direct-jump",
            "DisplayTacticalBaseQuote",
            0x4790E,
            0x4790E,
        ):
            raise ValueError("tactical-base quote caller opcode/register/tail drift")
        operand = first["operandTexts"][0]
        if not operand.startswith("#") or ally_values.get(operand[1:]) is None:
            raise ValueError("tactical-base quote caller selector drift")
        by_path[source_path].append(program)
    if {row["canonicalSymbol"] for row in by_path[_MAP37_PATH]} != expected_map37:
        raise ValueError("tactical-base quote Map37 caller set drift")
    if {row["canonicalSymbol"] for row in by_path[_MAP46_PATH]} != expected_map46:
        raise ValueError("tactical-base quote Map46 caller set drift")
    if len(by_path[_MAP37_PATH]) != 25 or len(by_path[_MAP46_PATH]) != 29:
        raise ValueError("tactical-base quote physical table denominator drift")

    source_tables = {
        _MAP37_PATH: ("ms_map37_EntityEvents", 0x5F86C),
        _MAP46_PATH: ("ms_map46_EntityEvents", 0x5C0F8),
    }
    source_files: dict[str, Any] = {}
    contexts: dict[str, Any] = {}
    physical: dict[str, Any] = {}
    caller_anchors: list[dict[str, Any]] = []
    for source_path in (_MAP37_PATH, _MAP46_PATH):
        table_symbol, table_entry = source_tables[source_path]
        source_key = "map37" if source_path == _MAP37_PATH else "map46"
        source_files[source_key] = {
            "sourcePath": source_path,
            "tableSymbol": table_symbol,
            "tableEntryAddress": table_entry,
            "sha256": _sha(source_bytes[source_path]),
        }
        rows = sorted(by_path[source_path], key=lambda row: row["entryAddress"])
        source_lines = source_text[source_path].splitlines()
        for program in rows:
            first, tail = program["operations"]
            expected_first = _normalise_statement("moveq " + ",".join(first["operandTexts"]))
            expected_tail = _normalise_statement("jmp " + ",".join(tail["operandTexts"]))
            if (
                first["sourceLine"] > len(source_lines)
                or tail["sourceLine"] > len(source_lines)
                or _normalise_statement(source_lines[first["sourceLine"] - 1]) != expected_first
                or _normalise_statement(source_lines[tail["sourceLine"] - 1]) != expected_tail
            ):
                raise ValueError("tactical-base quote caller source body drift")
            key = _program_key(source_path, program["canonicalSymbol"])
            if key in contexts or key in physical:
                raise ValueError("tactical-base quote caller key drift")
            operations = [
                _public_operation(key, operation_order, operation)
                for operation_order, operation in enumerate((first, tail))
            ]
            contexts[key] = {
                "contextOrder": len(contexts),
                "map": 37 if source_path == _MAP37_PATH else 46,
                "sourcePath": source_path,
                "programSymbol": program["canonicalSymbol"],
                "physicalProgramId": key,
                "entryAddress": program["entryAddress"],
                "endAddressExclusive": program["endAddressExclusive"],
                "operationIds": [row["operationId"] for row in operations],
                "operations": operations,
            }
            physical[key] = {
                "physicalOrder": len(physical),
                "map": 37 if source_path == _MAP37_PATH else 46,
                "sourcePath": source_path,
                "programSymbol": program["canonicalSymbol"],
                "entryAddress": program["entryAddress"],
                "endAddressExclusive": program["endAddressExclusive"],
                "encodedByteCount": program["encodedSpanBytes"],
                "operationAddresses": [first["address"], tail["address"]],
            }
            caller_anchors.extend(
                [
                    _anchor(
                        first["address"],
                        expected_first,
                        h1_rows,
                        rom,
                        source_path=source_path,
                        source_line=first["sourceLine"],
                        anchor_order=len(caller_anchors),
                    ),
                    _anchor(
                        tail["address"],
                        expected_tail,
                        h1_rows,
                        rom,
                        source_path=source_path,
                        source_line=tail["sourceLine"],
                        expected_target_address=_FUNCTION_ADDRESSES[0],
                        anchor_order=len(caller_anchors) + 1,
                    ),
                ]
            )
    if (
        len(caller_anchors) != 108
        or sum(item["h1ListedByteCount"] for item in caller_anchors) != 432
    ):
        raise ValueError("tactical-base quote caller anchor denominator drift")

    service_entries: dict[str, Any] = {}
    service_anchors: list[dict[str, Any]] = []
    for order, (
        jump,
        jump_address,
        effective,
        effective_address,
        jump_path,
        effective_path,
    ) in enumerate(_SERVICES):
        jump_line, jump_statement = _source_statement_after_label(
            source_text[jump_path].splitlines(), jump
        )
        effective_line, effective_statement = _source_statement_after_label(
            source_text[effective_path].splitlines(), effective
        )
        service_entries[jump] = {
            "serviceOrder": order,
            "instructionTarget": jump,
            "instructionTargetAddress": jump_address,
            "effectiveTarget": effective,
            "effectiveTargetAddress": effective_address,
            "instructionTargetSourcePath": jump_path,
            "effectiveTargetSourcePath": effective_path,
        }
        service_anchors.append(
            _anchor(
                jump_address,
                jump_statement,
                h1_rows,
                rom,
                source_path=jump_path,
                source_line=jump_line,
                expected_target_address=effective_address
                if jump_address != effective_address
                else None,
                anchor_order=len(service_anchors),
            )
        )
        if jump_address != effective_address:
            service_anchors.append(
                _anchor(
                    effective_address,
                    effective_statement,
                    h1_rows,
                    rom,
                    source_path=effective_path,
                    source_line=effective_line,
                    anchor_order=len(service_anchors),
                )
            )
    if len(service_anchors) != 9:
        raise ValueError("tactical-base quote retained service anchor denominator drift")

    function_anchors = [
        _anchor(
            address,
            statement,
            h1_rows,
            rom,
            source_path=_FUNCTION_PATH,
            source_line=source_line,
            expected_target_address=_function_expected_target(
                statement,
                function_label_addresses,
                service_entries,
            ),
            anchor_order=anchor_order,
        )
        for anchor_order, (address, (source_line, statement)) in enumerate(
            zip(_FUNCTION_ADDRESSES, function_rows, strict=True)
        )
    ]
    if sum(anchor["h1ListedByteCount"] for anchor in function_anchors) != 58:
        raise ValueError("tactical-base quote function H1 byte drift")

    selector_order = [symbol for symbol, _ in sorted(ally_values.items(), key=lambda pair: pair[1])]
    quote_branches = {
        "dead": {
            "branchOrder": 0,
            "testAddress": 0x4791A,
            "branchAddress": 0x4791C,
            "lineId": 1,
        },
        "active": {
            "branchOrder": 1,
            "flagOffset": 32,
            "checkFlagCallAddress": 0x4792A,
            "branchAddress": 0x47930,
            "lineOffset": 0xDC3,
            "firstLineId": 0xDC4,
            "lastLineId": 0xDE0,
        },
        "reserve": {
            "branchOrder": 2,
            "lineOffset": 0xDE1,
            "firstLineId": 0xDE2,
            "lastLineId": 0xDFE,
        },
    }
    facts = {
        "sourceFileOrder": list(source_files),
        "sourceFiles": source_files,
        "programContextOrder": list(contexts),
        "programContexts": contexts,
        "physicalProgramOrder": list(physical),
        "physicalPrograms": physical,
        "allySelectorOrder": selector_order,
        "allySelectors": {"values": [ally_values[symbol] for symbol in selector_order]},
        "serviceEntryOrder": list(service_entries),
        "serviceEntries": service_entries,
        "functionFlow": {
            "entryAddress": 0x4790E,
            "endAddressExclusive": 0x47948,
            "sourceStatementCount": 16,
            "h1InstructionRowCount": 16,
            "ownedByteCount": 58,
            "instructions": [
                {
                    "instructionOrder": anchor["anchorOrder"],
                    "address": anchor["address"],
                    "sourceLine": anchor["sourceLine"],
                    "mnemonic": anchor["mnemonic"],
                    "sizeSuffix": anchor["sizeSuffix"],
                    "operands": anchor["operands"],
                    "controlFlowKind": anchor["controlFlowKind"],
                    "expectedTargetAddress": anchor["expectedTargetAddress"],
                }
                for anchor in function_anchors
            ],
            "returnAddress": 0x47946,
        },
        "quoteBranchOrder": list(quote_branches),
        "quoteBranches": quote_branches,
        "quoteLineDomain": {
            "deadLineId": 1,
            "activeFirstLineId": 0xDC4,
            "activeLastLineId": 0xDE0,
            "reserveFirstLineId": 0xDE2,
            "reserveLastLineId": 0xDFE,
            "uniqueLineIdCount": 59,
        },
        "digests": {
            "callerAnchorSha256": _sha(canonical_json_bytes(caller_anchors)),
            "functionAnchorSha256": _sha(canonical_json_bytes(function_anchors)),
            "serviceAnchorSha256": _sha(canonical_json_bytes(service_anchors)),
        },
    }
    summary = {
        "sourceIdentityCount": 10,
        "motherProgramContextCount": len(all_programs),
        "positiveProgramContextCount": len(positives),
        "zeroProgramContextCount": len(all_programs) - len(positives),
        "physicalProgramCount": len(physical),
        "map37CallerContextCount": len(by_path[_MAP37_PATH]),
        "map46CallerContextCount": len(by_path[_MAP46_PATH]),
        "callerInstructionRowCount": len(caller_anchors),
        "functionInstructionRowCount": len(function_anchors),
        "sourceOperationCount": len(caller_anchors) + len(function_anchors),
        "h1InstructionRowCount": len(caller_anchors) + len(function_anchors),
        "ownedByteCount": sum(
            item["h1ListedByteCount"] for item in caller_anchors + function_anchors
        ),
        "retainedServiceJoinCount": 9,
        "anchorCount": len(caller_anchors) + len(function_anchors) + len(service_anchors),
    }
    if summary != {
        "sourceIdentityCount": 10,
        "motherProgramContextCount": 914,
        "positiveProgramContextCount": 54,
        "zeroProgramContextCount": 860,
        "physicalProgramCount": 54,
        "map37CallerContextCount": 25,
        "map46CallerContextCount": 29,
        "callerInstructionRowCount": 108,
        "functionInstructionRowCount": 16,
        "sourceOperationCount": 124,
        "h1InstructionRowCount": 124,
        "ownedByteCount": 490,
        "retainedServiceJoinCount": 9,
        "anchorCount": 133,
    }:
        raise ValueError("tactical-base quote summary denominator drift")
    output = {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {
            "repository": "https://github.com/ShiningForceCentral/SF2DISASM",
            "commit": _UPSTREAM_COMMIT,
        },
        "romSha256": _ROM_SHA256,
        "retainedOwners": _retained_owners(),
        "sourceContext": {
            "h1Listing": {"path": "build/sf2build-h1.lst", "sha256": _sha(listing_bytes)},
            "motherFixture": {
                "id": "sf2-map-events-static-v1",
                "sha256": _fixture_digest("tests/fixtures/h2/map-events-static-v1.json"),
            },
            "sourceIdentities": [
                {"path": path, "sha256": _sha(source_bytes[path])} for path in _SOURCE_PATHS
            ],
            "callerAnchors": caller_anchors,
            "functionAnchors": function_anchors,
            "retainedServiceAnchors": service_anchors,
        },
        "tacticalBaseQuoteState": facts,
        "unknowns": {key: "Unknown" for key in _UNKNOWN_KEYS},
        "summary": summary,
    }
    _validate_order(output)
    return output


def verify_map_event_tactical_base_quote_state_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner=str(FIXTURE))
    _validate_order(fixture)
    output = build_map_event_tactical_base_quote_state_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="map-event tactical-base quote static contract")
    if fixture != output:
        raise ValueError("map-event tactical-base quote complete semantic fixture drift")
    destination = output_path or repo_path(
        "local/derived/map-event-tactical-base-quote-state-static.json"
    )
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(json.dumps(output, indent=2).encode("utf-8") + b"\n")
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": _sha(canonical_json_bytes(output)),
    }


def _remove_map_event_tactical_base_quote_state_later_owner_index_delta(
    index: dict[str, Any],
) -> dict[str, Any]:
    """Remove only this owner delta, rejecting missing, extra, or altered fields."""
    normalized = deepcopy(index)
    records = normalized.get("records")
    if not isinstance(records, list) or len({row.get("id") for row in records}) != len(records):
        raise ValueError("tactical-base quote later-owner record shape drift")
    bindings = {
        "map.data.ms-map37-entityevents": [
            ("entry", "tacticalBaseQuoteState.sourceFiles.map37.tableEntryAddress")
        ],
        "map.data.ms-map46-entityevents": [
            ("entry", "tacticalBaseQuoteState.sourceFiles.map46.tableEntryAddress")
        ],
        "scripting.map.headquartersfunctions": [
            ("entry", "tacticalBaseQuoteState.functionFlow.entryAddress")
        ],
        "tech.interfaces.jump-s03b": [
            (
                "open-name-under-portrait",
                "tacticalBaseQuoteState.serviceEntries.j_OpenNameUnderPortraitWindow.instructionTargetAddress",
            ),
            (
                "close-name-under-portrait",
                "tacticalBaseQuoteState.serviceEntries.j_CloseNameUnderPortraitWindow.instructionTargetAddress",
            ),
        ],
        "menus.name-under-portrait": [
            (
                "entry",
                "tacticalBaseQuoteState.serviceEntries.j_OpenNameUnderPortraitWindow.effectiveTargetAddress",
            ),
            (
                "close-entry",
                "tacticalBaseQuoteState.serviceEntries.j_CloseNameUnderPortraitWindow.effectiveTargetAddress",
            ),
        ],
        "tech.interfaces.jump-s02": [
            (
                "get-current-hp",
                "tacticalBaseQuoteState.serviceEntries.j_GetCurrentHp.instructionTargetAddress",
            ),
            (
                "check-flag",
                "tacticalBaseQuoteState.serviceEntries.j_CheckFlag.instructionTargetAddress",
            ),
        ],
        "stats.combatant-getters": [
            (
                "get-current-hp",
                "tacticalBaseQuoteState.serviceEntries.j_GetCurrentHp.effectiveTargetAddress",
            )
        ],
        "stats.flags": [
            (
                "entry",
                "tacticalBaseQuoteState.serviceEntries.j_CheckFlag.effectiveTargetAddress",
            )
        ],
        "scripting.text.textfunctions-1": [
            (
                "entry",
                "tacticalBaseQuoteState.serviceEntries.DisplayText.effectiveTargetAddress",
            )
        ],
    }
    addresses = {
        "tech.interfaces.jump-s03b": [
            {
                "id": "open-name-under-portrait",
                "space": "rom",
                "kind": "observation",
                "value": 65708,
            },
            {
                "id": "close-name-under-portrait",
                "space": "rom",
                "kind": "observation",
                "value": 65712,
            },
        ],
        "menus.name-under-portrait": [
            {"id": "close-entry", "space": "rom", "kind": "observation", "value": 92720}
        ],
    }
    expected_document = "docs/research/map-event-tactical-base-quote-state.md"
    seen: set[str] = set()
    for record in records:
        record_id = record.get("id")
        expected_bindings = bindings.get(record_id)
        if expected_bindings is None:
            continue
        expected_evidence = {
            "level": "H2",
            "fixture": "tests/fixtures/h2/map-event-tactical-base-quote-state-static-v1.json",
            "fixtureId": ID,
            "verifier": "src/sf2tool/h2/map_event_tactical_base_quote_state.py",
            "bindings": [
                {"addressId": address_id, "fixtureField": fixture_field}
                for address_id, fixture_field in expected_bindings
            ],
        }
        evidence = record.get("evidence")
        documents = record.get("documents")
        record_addresses = record.get("addresses")
        matches = (
            [row for row in evidence if row.get("fixtureId") == ID]
            if isinstance(evidence, list)
            else []
        )
        if (
            matches != [expected_evidence]
            or not isinstance(documents, list)
            or documents.count(expected_document) != 1
            or documents[-1] != expected_document
            or not isinstance(record_addresses, list)
        ):
            raise ValueError("tactical-base quote later-owner record fields drift")
        for address in addresses.get(record_id, []):
            if record_addresses.count(address) != 1:
                raise ValueError("tactical-base quote index address delta drift")
            record_addresses.remove(address)
        evidence.remove(expected_evidence)
        documents.remove(expected_document)
        seen.add(record_id)
    if seen != set(bindings):
        raise ValueError("tactical-base quote later-owner coverage drift")
    if _sha(canonical_json_bytes(normalized)) != _PREDECESSOR_INDEX_SHA256:
        raise ValueError("tactical-base quote predecessor index drift")
    return normalized


def normalize_map_event_tactical_base_quote_state_later_owner_index(
    index: dict[str, Any],
) -> dict[str, Any]:
    """Strictly remove this exact latest-owner delta then delegate to predecessors."""
    return normalize_map_event_random_battle_state_later_owner_index(
        _remove_map_event_tactical_base_quote_state_later_owner_index_delta(index)
    )
