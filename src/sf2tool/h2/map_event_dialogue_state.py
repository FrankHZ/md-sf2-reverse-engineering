"""Public H2 dialogue-state flow for the bounded map-event source surface.

This owner deliberately consumes the accepted map-events and direct-state rails
as read-only projections, then builds the smaller 24-program CFG afresh.  It
does not assign dialogue, portrait, audio, timing, or persistence meaning to
the static state reads and writes it records.
"""

from __future__ import annotations

import hashlib
import re
from collections import Counter, deque
from pathlib import Path
from typing import Any

from sf2tool.h2.map_data import build_map_data_inventory
from sf2tool.h2.map_event_direct_control import _UNKNOWN_KEYS as _DIRECT_CONTROL_UNKNOWN_KEYS
from sf2tool.h2.map_event_direct_control import FIXTURE as DIRECT_CONTROL_FIXTURE
from sf2tool.h2.map_event_direct_control import ID as DIRECT_CONTROL_ID
from sf2tool.h2.map_event_direct_control import _assert_source_statement, _direct_control_projection
from sf2tool.h2.map_event_direct_control import (
    _mother_corpus_projection as _direct_control_mother_corpus_projection,
)
from sf2tool.h2.map_event_direct_handoff import _UNKNOWN_KEYS as _DIRECT_HANDOFF_UNKNOWN_KEYS
from sf2tool.h2.map_event_direct_handoff import FIXTURE as DIRECT_HANDOFF_FIXTURE
from sf2tool.h2.map_event_direct_handoff import ID as DIRECT_HANDOFF_ID
from sf2tool.h2.map_event_direct_handoff import _fixture_sha256 as _handoff_fixture_sha256
from sf2tool.h2.map_event_direct_handoff import _h1_instruction_rows, _handoff_projection
from sf2tool.h2.map_event_direct_handoff import (
    _mother_corpus_projection as _direct_handoff_mother_corpus_projection,
)
from sf2tool.h2.map_event_direct_handoff import _normalise_statement as _normalise_statement
from sf2tool.h2.map_event_direct_state import _UNKNOWN_KEYS as _DIRECT_STATE_UNKNOWN_KEYS
from sf2tool.h2.map_event_direct_state import FIXTURE as DIRECT_STATE_FIXTURE
from sf2tool.h2.map_event_direct_state import ID as DIRECT_STATE_ID
from sf2tool.h2.map_event_direct_state import (
    MAP_DATA_FIXTURE,
    MAP_DATA_MANIFEST,
    _direct_state_projection,
    _direct_symbol,
    _parse_equates,
    canonical_json_bytes,
)
from sf2tool.h2.map_event_predicate_results import _UNKNOWN_KEYS as _PREDICATE_RESULTS_UNKNOWN_KEYS
from sf2tool.h2.map_event_predicate_results import FIXTURE as PREDICATE_RESULTS_FIXTURE
from sf2tool.h2.map_event_predicate_results import ID as PREDICATE_RESULTS_ID
from sf2tool.h2.map_event_predicate_results import _fixture_projection, _predicate_projection
from sf2tool.h2.map_events import _canonical_bytes as _map_events_canonical_bytes
from sf2tool.h2.map_events import build_map_events_contract
from sf2tool.h2.map_events_fixture import load_map_events_fixture
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path

ID = "sf2-map-event-dialogue-state-static-v1"
FIXTURE = repo_path("tests/fixtures/h2/map-event-dialogue-state-static-v1.json")
SCHEMA = repo_path("schemas/h2/map-event-dialogue-state-static-fixture.schema.json")
MAP_EVENTS_MANIFEST = repo_path("manifests/extractions/map-events-static.json")

_CATEGORIES = ("entityEvents", "zoneEvents", "itemEvents")
_PROGRAM_FIELDS = {
    "entityEvents": "entityTargetPrograms",
    "zoneEvents": "zoneTargetPrograms",
    "itemEvents": "itemTargetPrograms",
}
_SYMBOLS = (
    "CURRENT_PORTRAIT",
    "CURRENT_SPEECH_SFX",
    "SPEECH_SFX_COPY",
    "DIALOGUE_NAME_INDEX_1",
    "MESSAGE_SPEED",
)
_UNKNOWN_KEYS = (
    "normalStoryProgramReachability",
    "selectedControlFlowPath",
    "entityLookupInputIdentity",
    "entityPortraitLookupResult",
    "entitySpeechSfxLookupResult",
    "zoneItemInheritedEntryState",
    "actualTextServiceExecution",
    "actualDisplayedLineOrder",
    "dialogueNameSubstitutionValue",
    "portraitWindowVisibilityAndPlacement",
    "speechSfxPlaybackAndTiming",
    "messageSpeedCadence",
    "controllerAdvanceTiming",
    "postReturnStateLifetimeAndPersistence",
    "storyMeaning",
)
_MACRO_PARAMETER = re.compile(r"\\([1-9][0-9]*)")
_CS_ADDRESS = re.compile(r"^cs_([0-9A-Fa-f]+)$")
_CS_SYMBOL = re.compile(r"^(cs_[A-Za-z0-9_]+)$", re.IGNORECASE)
_PC_RELATIVE_PARAMETER = re.compile(r"\\([1-9][0-9]*)\(pc\)", re.IGNORECASE)
_H1_SYMBOL = re.compile(r"^([0-9A-Fa-f]{8})\s+([A-Za-z_][A-Za-z0-9_]*):")
_EXPECTED_RELOCATION_KINDS = {
    "byte-identical": 444,
    "short-branch-zero-placeholder": 49,
    "word-branch-zero-placeholder": 3,
    "pc-relative-zero-placeholder": 21,
    "absolute-control-zero-placeholder": 1,
}
_EXPECTED_MACRO_RELOCATION_KINDS = {
    "byte-identical": 267,
    "pc-relative-zero-placeholder": 21,
}
_EXPECTED_TABLE_OWNERS = (
    ("entityEvents", "ms_map3_flag506_EntityEvents", 332234),
    ("zoneEvents", "ms_map3_ZoneEvents", 331084),
    ("entityEvents", "ms_map5_flag530_EntityEvents", 394298),
    ("entityEvents", "ms_map5_flag650_EntityEvents", 334466),
    ("entityEvents", "ms_map6_flag701_EntityEvents", 346500),
    ("entityEvents", "ms_map16_flag530_EntityEvents", 397400),
    ("entityEvents", "ms_map18_EntityEvents", 338618),
    ("entityEvents", "ms_map19_flag506_EntityEvents", 339996),
    ("zoneEvents", "ms_map20_flag543_ZoneEvents", 406170),
    ("entityEvents", "ms_map21_flag506_EntityEvents", 343722),
    ("entityEvents", "ms_map25_EntityEvents", 381476),
    ("itemEvents", "ms_map37_Section5", 391722),
    ("entityEvents", "ms_map40_EntityEvents", 343954),
    ("entityEvents", "ms_map44_flag507_EntityEvents", 345226),
    ("entityEvents", "ms_map63_EntityEvents", 379274),
    ("zoneEvents", "ms_map72_ZoneEvents", 327268),
    ("itemEvents", "ms_map77_Section5", 330380),
)
_SUPPORT_SOURCE_PATHS = (
    "sf2const.asm",
    "sf2enums.asm",
    "sf2macros.asm",
    "code/common/scripting/map/mapsetupsfunctions_1.asm",
    "code/common/scripting/entity/getentityportaitandspeechsfx.asm",
)
_LABEL = re.compile(r"^([A-Za-z_][A-Za-z0-9_]*):\s*(.*)$")


def _fixture_sha256(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest().upper()


def _disasm_root(upstream_path: Path) -> Path:
    root = upstream_path.resolve(strict=True)
    return root / "disasm" if (root / "disasm").is_dir() else root


def _fresh_retained_owners(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    """Fresh-build retained projections once, sharing the mother corpus.

    Goldens are comparison targets only: every retained source/H1/ROM
    projection below is rebuilt before it is accepted.
    """
    map_events = build_map_events_contract(rom_path, upstream_path)
    map_events_fixture = load_map_events_fixture()
    map_events_manifest = load_json(MAP_EVENTS_MANIFEST)
    if map_events != map_events_fixture["expected"]:
        raise ValueError("map-event dialogue-state retained map-events projection drift")
    map_events_digest = hashlib.sha256(_map_events_canonical_bytes(map_events)).hexdigest().upper()
    if (
        map_events_digest != map_events_manifest["outputSha256"]
        or map_events["summary"] != map_events_manifest["summary"]
    ):
        raise ValueError("map-event dialogue-state retained map-events digest drift")

    map_events_owner = {
        "fixtureId": map_events_fixture["id"],
        "fixtureSha256": _fixture_sha256(repo_path("tests/fixtures/h2/map-events-static-v1.json")),
        "outputSha256": map_events_digest,
    }
    map_data = build_map_data_inventory(upstream_path)
    map_data_fixture = load_json(MAP_DATA_FIXTURE)
    map_data_manifest = load_json(MAP_DATA_MANIFEST)
    if map_data["representativeAddresses"] != map_data_fixture["table"]:
        raise ValueError("map-event dialogue-state retained map-data address projection drift")
    map_data_digest = hashlib.sha256(_map_events_canonical_bytes(map_data)).hexdigest().upper()
    if (
        map_data_digest != map_data_manifest["outputSha256"]
        or map_data["summary"] != map_data_manifest["summary"]
    ):
        raise ValueError("map-event dialogue-state retained map-data digest drift")

    direct_state_fixture = load_json(DIRECT_STATE_FIXTURE)
    event_direct_state, direct_state_summary, direct_state_context, mother_corpus = (
        _direct_state_projection(
            map_events,
            upstream_path=upstream_path,
            rom_path=rom_path,
            map_events_output_sha256=map_events_digest,
        )
    )
    direct_state = {
        "schemaVersion": 1,
        "id": DIRECT_STATE_ID,
        "upstream": map_events["upstream"],
        "romSha256": map_events["romSha256"],
        "sourceContext": {**direct_state_context, "motherCorpus": mother_corpus},
        "retainedOwners": {
            "mapEvents": map_events_owner,
            "mapData": {
                "fixtureId": map_data_fixture["id"],
                "fixtureSha256": _fixture_sha256(MAP_DATA_FIXTURE),
                "outputSha256": map_data_digest,
            },
        },
        "eventDirectState": event_direct_state,
        "unknowns": {key: "Unknown" for key in _DIRECT_STATE_UNKNOWN_KEYS},
        "summary": direct_state_summary,
    }
    if direct_state != direct_state_fixture:
        raise ValueError("map-event dialogue-state retained direct-state projection drift")

    direct_control_fixture = load_json(DIRECT_CONTROL_FIXTURE)
    event_direct_control, direct_control_summary, direct_control_context = (
        _direct_control_projection(map_events, upstream_path=upstream_path, rom_path=rom_path)
    )
    direct_control = {
        "schemaVersion": 1,
        "id": DIRECT_CONTROL_ID,
        "upstream": map_events["upstream"],
        "romSha256": map_events["romSha256"],
        "scope": _direct_control_mother_corpus_projection(map_events),
        "sourceContext": direct_control_context,
        "retainedMapEvents": {**map_events_owner, "summary": map_events["summary"]},
        "eventDirectControl": event_direct_control,
        "unknowns": {key: "Unknown" for key in _DIRECT_CONTROL_UNKNOWN_KEYS},
        "summary": direct_control_summary,
    }
    if direct_control != direct_control_fixture:
        raise ValueError("map-event dialogue-state retained direct-control projection drift")

    direct_handoff_fixture = load_json(DIRECT_HANDOFF_FIXTURE)
    event_direct_handoff, direct_handoff_summary, direct_handoff_context = _handoff_projection(
        map_events,
        direct_state,
        direct_control,
        upstream_path=upstream_path,
        rom_path=rom_path,
    )
    direct_handoff = {
        "schemaVersion": 1,
        "id": DIRECT_HANDOFF_ID,
        "upstream": map_events["upstream"],
        "romSha256": map_events["romSha256"],
        "scope": _direct_handoff_mother_corpus_projection(map_events),
        "sourceContext": direct_handoff_context,
        "retainedOwners": {
            "mapEvents": {**map_events_owner, "summary": map_events["summary"]},
            "eventDirectState": {
                "fixtureId": direct_state_fixture["id"],
                "fixtureSha256": _handoff_fixture_sha256(DIRECT_STATE_FIXTURE),
                "outputSha256": hashlib.sha256(canonical_json_bytes(direct_state))
                .hexdigest()
                .upper(),
                "summary": direct_state["summary"],
            },
            "eventDirectControl": {
                "fixtureId": direct_control_fixture["id"],
                "fixtureSha256": _handoff_fixture_sha256(DIRECT_CONTROL_FIXTURE),
                "outputSha256": hashlib.sha256(canonical_json_bytes(direct_control))
                .hexdigest()
                .upper(),
                "summary": direct_control["summary"],
            },
        },
        "eventDirectHandoff": event_direct_handoff,
        "unknowns": {key: "Unknown" for key in _DIRECT_HANDOFF_UNKNOWN_KEYS},
        "summary": direct_handoff_summary,
    }
    if direct_handoff != direct_handoff_fixture:
        raise ValueError("map-event dialogue-state retained direct-handoff projection drift")

    predicate_fixture = load_json(PREDICATE_RESULTS_FIXTURE)
    event_predicates, predicate_summary, predicate_context = _predicate_projection(
        map_events,
        direct_state=direct_state,
        direct_control=direct_control,
        upstream_path=upstream_path,
        rom_path=rom_path,
    )
    predicate_results = {
        "schemaVersion": 1,
        "id": PREDICATE_RESULTS_ID,
        "upstream": map_events["upstream"],
        "romSha256": map_events["romSha256"],
        "retainedOwners": {
            "mapEvents": map_events_owner,
            "directState": _fixture_projection(
                DIRECT_STATE_FIXTURE, direct_state, name="direct-state"
            ),
            "directControl": _fixture_projection(
                DIRECT_CONTROL_FIXTURE, direct_control, name="direct-control"
            ),
            "directHandoff": _fixture_projection(
                DIRECT_HANDOFF_FIXTURE, direct_handoff, name="direct-handoff"
            ),
        },
        "sourceContext": predicate_context,
        "eventPredicateResults": event_predicates,
        "unknowns": {key: "Unknown" for key in _PREDICATE_RESULTS_UNKNOWN_KEYS},
        "summary": predicate_summary,
    }
    if predicate_results != predicate_fixture:
        raise ValueError("map-event dialogue-state retained predicate-results projection drift")
    return {
        "mapEvents": map_events,
        "eventDirectState": event_direct_state,
        "projections": {
            "mapEvents": map_events_owner,
            "directState": {
                "fixtureId": direct_state_fixture["id"],
                "fixtureSha256": _fixture_sha256(DIRECT_STATE_FIXTURE),
                "outputSha256": hashlib.sha256(canonical_json_bytes(direct_state))
                .hexdigest()
                .upper(),
            },
            "directControl": _fixture_projection(
                DIRECT_CONTROL_FIXTURE, direct_control, name="direct-control"
            ),
            "directHandoff": _fixture_projection(
                DIRECT_HANDOFF_FIXTURE, direct_handoff, name="direct-handoff"
            ),
            "predicateResults": _fixture_projection(
                PREDICATE_RESULTS_FIXTURE, predicate_results, name="predicate-results"
            ),
        },
    }


def _table_rows(map_events: dict[str, Any]) -> dict[tuple[str, str], dict[str, Any]]:
    rows: dict[tuple[str, str], dict[str, Any]] = {}
    for category in _CATEGORIES:
        for row in map_events["categories"][category]["sourceFiles"]:
            key = (category, row["path"])
            if key in rows:
                raise ValueError("map-event dialogue-state duplicate table source path")
            rows[key] = row
    return rows


def _canonical_source_file_rows(rows: list[dict[str, Any]]) -> list[dict[str, Any]]:
    """Order the fixed owner set by its source path, not category traversal."""
    canonical = sorted(
        rows,
        key=lambda row: (
            row["sourcePath"],
            row["category"],
            row["tableSymbol"],
            row["tableEntryAddress"],
        ),
    )
    actual = tuple(
        (row["category"], row["tableSymbol"], row["tableEntryAddress"]) for row in canonical
    )
    if actual != _EXPECTED_TABLE_OWNERS:
        raise ValueError(
            "map-event dialogue-state exact table-owner order drift: "
            f"expected={_EXPECTED_TABLE_OWNERS}; actual={actual}"
        )
    return canonical


def _physical_state_access_pcs(rows: list[dict[str, Any]]) -> set[int]:
    """Collapse contextual direct-state edges only after identity agreement."""
    identities: dict[int, dict[tuple[int, str], tuple[Any, ...]]] = {}
    for row in rows:
        rom_pc = row["romPc"]
        edge_key = row["accessOperandIndex"], row["accessKind"]
        identity = (
            row["symbol"],
            row["mnemonic"],
            row["width"],
            tuple(row["operandTexts"]),
            row["address"],
            row["instructionByteLength"],
            row["instructionSha256"],
            row["valueKind"],
            row["valueToken"],
            row["resolvedValue"],
        )
        edges = identities.setdefault(rom_pc, {})
        prior = edges.setdefault(edge_key, identity)
        if prior != identity:
            raise ValueError(
                "map-event dialogue-state conflicting same-PC state edge drift: "
                f"romPc={rom_pc:06X}; edge={edge_key}; expected={prior}; actual={identity}"
            )
    return set(identities)


def _selected_programs(
    map_events: dict[str, Any], direct_state: dict[str, Any]
) -> list[tuple[str, dict[str, Any]]]:
    selected_keys = {
        (row["category"], row["programSymbol"], row["programEntryAddress"])
        for row in direct_state["accessSites"]
        if row["symbol"] in _SYMBOLS
    }
    if len(selected_keys) != 24:
        raise ValueError("map-event dialogue-state positive-program selection drift")
    selected: list[tuple[str, dict[str, Any]]] = []
    for category in _CATEGORIES:
        for program in map_events[_PROGRAM_FIELDS[category]]:
            key = (category, program["canonicalSymbol"], program["entryAddress"])
            if key in selected_keys:
                selected.append((category, program))
    if len(selected) != len(selected_keys):
        raise ValueError("map-event dialogue-state selected program owner join drift")
    return selected


def _operation_statement(operation: dict[str, Any]) -> str:
    source = operation["sourceMnemonic"]
    operands = operation["operandTexts"]
    return _normalise_statement(source + (" " + ", ".join(operands) if operands else ""))


def _operation_shape(operation: dict[str, Any]) -> dict[str, Any]:
    target = operation["target"]
    return {
        "sourceOrder": operation["sourceOrder"],
        "sourceLine": operation["sourceLine"],
        "romPc": operation["address"],
        "family": operation["family"],
        "sourceMnemonic": operation["sourceMnemonic"],
        "mnemonic": operation["mnemonic"],
        "sizeSuffix": operation["sizeSuffix"],
        "operandCount": len(operation["operandTexts"]),
        "controlFlowKind": operation["controlFlowKind"],
        "instructionTargetAddress": None if target is None else target["instructionTargetAddress"],
        "effectiveTargetAddress": None if target is None else target["effectiveTargetAddress"],
    }


def _bound_macro_statement(template: str, operands: list[str]) -> str:
    """Bind one retained macro emission without borrowing handoff byte policy."""

    def replace(match: re.Match[str]) -> str:
        ordinal = int(match.group(1))
        if not 1 <= ordinal <= len(operands):
            raise ValueError("map-event dialogue-state macro operand ordinal drift")
        return operands[ordinal - 1]

    return _MACRO_PARAMETER.sub(replace, template)


def _normalise_h1_macro_statement(statement: str) -> str:
    statement = _normalise_statement(statement)
    return (statement[2:] if statement.startswith("M ") else statement).lower()


def _pc_relative_target(address: int, encoded: bytes) -> int | None:
    if len(encoded) != 4 or encoded[1] != 0xFA:
        return None
    return address + 2 + int.from_bytes(encoded[2:], byteorder="big", signed=True)


def _control_target(address: int, encoded: bytes) -> int | None:
    if len(encoded) < 2:
        return None
    opcode = int.from_bytes(encoded[:2], byteorder="big")
    if opcode & 0xF000 == 0x6000:
        displacement = encoded[1]
        if displacement == 0 and len(encoded) == 4:
            displacement = int.from_bytes(encoded[2:], byteorder="big", signed=True)
            return address + 2 + displacement
        if displacement == 0xFF and len(encoded) == 6:
            displacement = int.from_bytes(encoded[2:], byteorder="big", signed=True)
            return address + 2 + displacement
        return address + 2 + int.from_bytes(encoded[1:2], byteorder="big", signed=True)
    if len(encoded) == 4 and encoded[1] == 0xFA:
        return _pc_relative_target(address, encoded)
    if opcode in {0x4EB9, 0x4EF9} and len(encoded) == 6:
        return int.from_bytes(encoded[2:], byteorder="big")
    return None


def _h1_symbol_addresses(listing_text: str) -> dict[str, int]:
    """Return the H1 label identities needed by source-level ``script`` operands."""
    addresses: dict[str, int] = {}
    for raw_line in listing_text.splitlines():
        match = _H1_SYMBOL.match(raw_line)
        if match is None:
            continue
        symbol = match.group(2).lower()
        address = int(match.group(1), 16)
        prior = addresses.setdefault(symbol, address)
        if prior != address:
            raise ValueError("map-event dialogue-state ambiguous H1 symbol address")
    return addresses


def _source_cs_address(operand: str, h1_symbols: dict[str, int]) -> int:
    numeric = _CS_ADDRESS.fullmatch(operand)
    if numeric is not None:
        return int(numeric.group(1), 16)
    symbolic = _CS_SYMBOL.fullmatch(operand)
    if symbolic is None:
        raise ValueError("map-event dialogue-state non-cs code operand drift")
    address = h1_symbols.get(symbolic.group(1).lower())
    if address is None:
        raise ValueError("map-event dialogue-state unresolved H1 script symbol")
    return address


def _macro_source_target(
    definition: dict[str, Any], operands: list[str], h1_symbols: dict[str, int]
) -> int | None:
    positions = {
        int(match.group(1))
        for template in definition["emissionStatementTemplates"]
        for match in _PC_RELATIVE_PARAMETER.finditer(template)
    }
    if not positions:
        return None
    if len(positions) != 1:
        raise ValueError("map-event dialogue-state ambiguous macro code operand position")
    ordinal = positions.pop()
    if ordinal > len(operands):
        raise ValueError("map-event dialogue-state macro code operand position drift")
    return _source_cs_address(operands[ordinal - 1], h1_symbols)


def _physical_anchor(
    *,
    operation: dict[str, Any],
    next_address: int | None,
    operation_definitions: dict[str, dict[str, Any]],
    h1_rows: dict[int, tuple[bytes, str]],
    h1_symbols: dict[str, int],
    rom: bytes,
    context: str,
) -> dict[str, Any]:
    """Prove one selected operation against source, H1, and ROM.

    This is intentionally smaller than direct-handoff's macro decoder: it
    validates each retained emission statement and permits only source-resolved
    PC/control relocation differences between H1 and ROM.
    """
    address = operation["address"]
    rows: list[tuple[int, bytes, str]] = []
    macro_definition: dict[str, Any] | None = None
    if operation["family"] == "event-service-macro":
        definition = operation_definitions.get(operation["definitionId"])
        if (
            definition is None
            or definition["family"] != "event-service-macro"
            or definition["sourceMacro"] != operation["sourceMnemonic"]
        ):
            raise ValueError("map-event dialogue-state macro definition drift")
        macro_definition = definition
        cursor = address
        for template in definition["emissionStatementTemplates"]:
            expected = _bound_macro_statement(template, operation["operandTexts"])
            row = h1_rows.get(cursor)
            if row is None or _normalise_h1_macro_statement(row[1]) != expected.lower():
                raise ValueError(f"map-event dialogue-state H1 macro emission drift: {context}")
            if next_address is not None and cursor >= next_address:
                raise ValueError(f"map-event dialogue-state macro boundary drift: {context}")
            rows.append((cursor, row[0], expected))
            cursor += len(row[0])
        if next_address is not None and cursor > next_address:
            raise ValueError(f"map-event dialogue-state macro byte span drift: {context}")
    else:
        row = h1_rows.get(address)
        expected = _operation_statement(operation)
        if row is None or row[1] != expected:
            raise ValueError(f"map-event dialogue-state H1 opcode/operand/order drift: {context}")
        rows.append((address, row[0], expected))

    h1_bytes = b""
    rom_bytes = b""
    source_target = None
    if operation["target"] is not None:
        source_target = operation["target"]["instructionTargetAddress"]
    elif macro_definition is not None:
        source_target = _macro_source_target(
            macro_definition, operation["operandTexts"], h1_symbols
        )
    relocation_kinds: list[str] = []
    for cursor, encoded, _expected in rows:
        original = rom[cursor : cursor + len(encoded)]
        if len(original) != len(encoded):
            raise ValueError(f"map-event dialogue-state ROM range drift: {context}")
        if original == encoded:
            relocation_kinds.append("byte-identical")
        else:
            rom_target = _control_target(cursor, original)
            short_branch_placeholder = (
                len(encoded) == 2
                and encoded[0] & 0xF0 == 0x60
                and encoded[1] == 0
                and encoded[:1] == original[:1]
                and source_target is not None
                and rom_target == source_target
            )
            word_branch_placeholder = (
                len(encoded) == 4
                and encoded[0] & 0xF0 == 0x60
                and encoded[1] == 0
                and encoded[2:] == b"\x00\x00"
                and encoded[:2] == original[:2]
                and source_target is not None
                and rom_target == source_target
            )
            pc_relative_placeholder = (
                len(encoded) == 4
                and encoded[1] == 0xFA
                and encoded[2:] == b"\x00\x00"
                and encoded[:2] == original[:2]
                and source_target is not None
                and rom_target == source_target
            )
            absolute_control_placeholder = (
                len(encoded) == 6
                and int.from_bytes(encoded[:2], byteorder="big") in {0x4EB9, 0x4EF9}
                and encoded[2:] == b"\x00\x00\x00\x00"
                and encoded[:2] == original[:2]
                and source_target is not None
                and rom_target == source_target
            )
            if short_branch_placeholder:
                relocation_kinds.append("short-branch-zero-placeholder")
            elif word_branch_placeholder:
                relocation_kinds.append("word-branch-zero-placeholder")
            elif pc_relative_placeholder:
                relocation_kinds.append("pc-relative-zero-placeholder")
            elif absolute_control_placeholder:
                relocation_kinds.append("absolute-control-zero-placeholder")
            else:
                raise ValueError(f"map-event dialogue-state H1/ROM relocation drift: {context}")
        h1_bytes += encoded
        rom_bytes += original
    return {
        "instructionByteLength": len(h1_bytes),
        "h1InstructionSha256": hashlib.sha256(h1_bytes).hexdigest().upper(),
        "romInstructionSha256": hashlib.sha256(rom_bytes).hexdigest().upper(),
        "relocationKinds": relocation_kinds,
    }


def _definition_ids(
    category: str,
    program: dict[str, Any],
    operation: dict[str, Any],
    direct_accesses: dict[tuple[str, str, int, int], list[dict[str, Any]]],
) -> dict[str, str]:
    """Return source-backed state writes, including the bounded lookup-call effect."""
    key = (category, program["canonicalSymbol"], program["entryAddress"], operation["address"])
    definitions: dict[str, str] = {}
    for access in direct_accesses.get(key, []):
        if access["accessKind"] == "write" and access["symbol"] in _SYMBOLS:
            definitions[access["symbol"]] = f"write:{access['romPc']:06X}:{access['symbol']}"
    target = operation["target"]
    if target is not None and target["effectiveTargetSymbol"] == "GetEntityPortaitAndSpeechSfx":
        for symbol in ("CURRENT_PORTRAIT", "CURRENT_SPEECH_SFX"):
            if symbol in definitions:
                raise ValueError("map-event dialogue-state lookup/write overlap drift")
            definitions[symbol] = f"lookup:{operation['address']:06X}:{symbol}"
    return definitions


def _program_edges(program: dict[str, Any]) -> tuple[list[dict[str, Any]], dict[int, list[int]]]:
    operations = program["operations"]
    indices = {operation["sourceOrder"]: index for index, operation in enumerate(operations)}
    if list(indices) != list(range(len(operations))):
        raise ValueError("map-event dialogue-state operation source-order drift")
    by_address = {operation["address"]: operation["sourceOrder"] for operation in operations}
    edges: list[dict[str, Any]] = []
    successors: dict[int, list[int]] = {index: [] for index in indices}
    for operation in operations:
        source = operation["sourceOrder"]
        kind = operation["controlFlowKind"]
        target = operation["target"]
        targets: list[tuple[str, int | None]] = []
        if kind in {"ordinary", "direct-call"}:
            targets.append(("fallthrough", source + 1 if source + 1 < len(operations) else None))
        elif kind == "conditional-branch":
            if target is None:
                raise ValueError("map-event dialogue-state conditional target drift")
            targets.extend(
                (
                    ("fallthrough", source + 1 if source + 1 < len(operations) else None),
                    ("taken", by_address.get(target["instructionTargetAddress"])),
                )
            )
        elif kind in {"unconditional-branch", "direct-jump"}:
            if target is None:
                raise ValueError("map-event dialogue-state transfer target drift")
            targets.append(("transfer", by_address.get(target["instructionTargetAddress"])))
        elif kind == "return":
            targets = []
        else:
            raise ValueError("map-event dialogue-state control-flow kind drift")
        for role, target_order in targets:
            if target_order is None:
                if kind != "direct-jump":
                    raise ValueError("map-event dialogue-state internal CFG target drift")
                edges.append(
                    {
                        "fromSourceOrder": source,
                        "role": role,
                        "toSourceOrder": None,
                        "externalTargetAddress": target["effectiveTargetAddress"],
                    }
                )
                continue
            successors[source].append(target_order)
            edges.append(
                {
                    "fromSourceOrder": source,
                    "role": role,
                    "toSourceOrder": target_order,
                    "externalTargetAddress": None,
                }
            )
    return edges, successors


def _program_key(category: str, program: dict[str, Any]) -> tuple[str, str, int]:
    return category, program["canonicalSymbol"], program["entryAddress"]


def _operation_identity(operation: dict[str, Any]) -> tuple[Any, ...]:
    target = operation["target"]
    return (
        operation["address"],
        operation["family"],
        operation["sourceMnemonic"],
        tuple(operation["operandTexts"]),
        operation["controlFlowKind"],
        None if target is None else target["instructionTargetAddress"],
        None if target is None else target["effectiveTargetAddress"],
    )


def _assert_source_label(lines: list[str], label: dict[str, Any], *, context: str) -> None:
    source_line = label["sourceLine"]
    if not 1 <= source_line <= len(lines):
        raise ValueError(f"map-event dialogue-state label source range drift: {context}")
    statement = lines[source_line - 1].split(";", maxsplit=1)[0].strip()
    match = _LABEL.fullmatch(statement)
    if match is None or match.group(1) != label["symbol"] or match.group(2):
        raise ValueError(f"map-event dialogue-state label source drift: {context}")


def _contextual_continuation_bindings(
    selected: list[tuple[str, dict[str, Any]]], *, disasm: Path
) -> dict[tuple[str, str, int], dict[str, Any]]:
    """Bind the one overlapping selected suffix to its true source entry."""
    bindings: dict[tuple[str, str, int], dict[str, Any]] = {}
    for category, continuation in selected:
        operations = continuation["operations"]
        if len(operations) < 2 or operations[0]["mnemonic"] != "rts":
            continue
        suffix = tuple(_operation_identity(row) for row in operations[1:])
        matches: list[tuple[dict[str, Any], int]] = []
        for owner_category, owner in selected:
            if owner_category != category or owner is continuation:
                continue
            if owner["sourcePath"] != continuation["sourcePath"]:
                continue
            for owner_order, owner_operation in enumerate(owner["operations"]):
                if owner_operation["address"] != operations[1]["address"]:
                    continue
                if (
                    tuple(_operation_identity(row) for row in owner["operations"][owner_order:])
                    == suffix
                ):
                    matches.append((owner, owner_order))
        if len(matches) != 1:
            raise ValueError("map-event dialogue-state contextual continuation owner drift")
        owner, owner_order = matches[0]
        if owner["entryAddress"] >= continuation["entryAddress"]:
            raise ValueError("map-event dialogue-state contextual continuation entry order drift")
        source_lines = (
            (disasm / continuation["sourcePath"]).read_text(encoding="utf-8").splitlines()
        )

        def label_at(
            program: dict[str, Any], address: int, symbol: str | None = None
        ) -> dict[str, Any] | None:
            matches = [
                label
                for label in program["labels"]
                if label["address"] == address and (symbol is None or label["symbol"] == symbol)
            ]
            return matches[0] if len(matches) == 1 else None

        owner_entry = label_at(owner, owner["entryAddress"], owner["canonicalSymbol"])
        continuation_entry = label_at(
            continuation, continuation["entryAddress"], continuation["canonicalSymbol"]
        )
        owner_boundary = label_at(owner, operations[1]["address"])
        continuation_boundary = label_at(continuation, operations[1]["address"])
        owner_boundary_identity = (
            None
            if owner_boundary is None
            else {
                "symbol": owner_boundary["symbol"],
                "address": owner_boundary["address"],
                "sourceLine": owner_boundary["sourceLine"],
            }
        )
        continuation_boundary_identity = (
            None
            if continuation_boundary is None
            else {
                "symbol": continuation_boundary["symbol"],
                "address": continuation_boundary["address"],
                "sourceLine": continuation_boundary["sourceLine"],
            }
        )
        if (
            owner_entry is None
            or continuation_entry is None
            or owner_boundary is None
            or continuation_boundary is None
            or owner_boundary_identity != continuation_boundary_identity
        ):
            raise ValueError(
                "map-event dialogue-state contextual continuation label drift: "
                f"expected={owner_boundary_identity}; actual={continuation_boundary_identity}"
            )
        _assert_source_label(source_lines, owner_entry, context="true-owner-entry")
        _assert_source_label(source_lines, continuation_entry, context="standalone-rts-entry")
        _assert_source_label(source_lines, owner_boundary, context="continuation-boundary")
        bindings[_program_key(category, continuation)] = {
            "ownerKey": _program_key(category, owner),
            "ownerSourceOrder": owner_order,
            "continuationSourceOrder": 1,
            "romPc": operations[1]["address"],
        }
    if len(bindings) != 1:
        raise ValueError("map-event dialogue-state contextual continuation denominator drift")
    return bindings


def _entry_state(category: str) -> dict[str, set[str]]:
    values: dict[str, set[str]] = {}
    for symbol in _SYMBOLS:
        if category == "entityEvents" and symbol == "CURRENT_PORTRAIT":
            values[symbol] = {"entry:entity-dispatch-prefill:CURRENT_PORTRAIT"}
        elif category == "entityEvents" and symbol == "CURRENT_SPEECH_SFX":
            values[symbol] = {"entry:entity-dispatch-prefill:CURRENT_SPEECH_SFX"}
        else:
            values[symbol] = {f"entry:{category}-inherited:{symbol}"}
    return values


def _reaching_definitions(
    category: str,
    program: dict[str, Any],
    definitions_by_order: dict[int, dict[str, str]],
    successors: dict[int, list[int]],
    *,
    continuation_entries: dict[int, tuple[dict[str, set[str]], dict[str, set[str]]]] | None = None,
) -> tuple[dict[int, dict[str, set[str]]], dict[int, dict[str, set[str]]]]:
    """Compute may/must source definitions before each operation in the CFG."""
    count = len(program["operations"])
    predecessors: dict[int, list[int]] = {index: [] for index in range(count)}
    for source, target_orders in successors.items():
        for target in target_orders:
            predecessors[target].append(source)
    entry = _entry_state(category)
    in_may: dict[int, dict[str, set[str]]] = {}
    in_must: dict[int, dict[str, set[str]]] = {}
    out_may: dict[int, dict[str, set[str]]] = {}
    out_must: dict[int, dict[str, set[str]]] = {}
    continuation_entries = continuation_entries or {}
    pending: deque[int] = deque([0, *sorted(continuation_entries)])
    queued = set(pending)
    while pending:
        order = pending.popleft()
        queued.remove(order)
        predecessor_orders = predecessors[order]
        if order == 0:
            predecessor_may = [entry]
            predecessor_must = [entry]
        else:
            predecessor_may = [out_may[item] for item in predecessor_orders if item in out_may]
            predecessor_must = [out_must[item] for item in predecessor_orders if item in out_must]
        if order in continuation_entries:
            continuation_may, continuation_must = continuation_entries[order]
            predecessor_may.append(continuation_may)
            predecessor_must.append(continuation_must)
        if not predecessor_may or not predecessor_must:
            continue
        next_may = {
            symbol: set().union(*(row[symbol] for row in predecessor_may)) for symbol in _SYMBOLS
        }
        next_must = {
            symbol: set.intersection(*(row[symbol] for row in predecessor_must))
            for symbol in _SYMBOLS
        }
        writes = definitions_by_order[order]
        next_out_may = {symbol: set(values) for symbol, values in next_may.items()}
        next_out_must = {symbol: set(values) for symbol, values in next_must.items()}
        for symbol, definition_id in writes.items():
            next_out_may[symbol] = {definition_id}
            next_out_must[symbol] = {definition_id}
        changed = (
            in_may.get(order) != next_may
            or in_must.get(order) != next_must
            or out_may.get(order) != next_out_may
            or out_must.get(order) != next_out_must
        )
        in_may[order] = next_may
        in_must[order] = next_must
        out_may[order] = next_out_may
        out_must[order] = next_out_must
        if changed:
            for successor in successors[order]:
                if successor not in queued:
                    pending.append(successor)
                    queued.add(successor)
    if len(in_may) != count:
        missing = sorted(set(range(count)) - set(in_may))
        missing_rows = [
            {
                "sourceOrder": order,
                "sourceLine": program["operations"][order]["sourceLine"],
                "predecessors": sorted(predecessors[order]),
            }
            for order in missing
        ]
        raise ValueError(
            "map-event dialogue-state unreachable selected operation drift: "
            f"{category}|{program['canonicalSymbol']}|{program['sourcePath']}|"
            f"missing={missing_rows}"
        )
    return in_may, in_must


def _state_set_row(may: dict[str, set[str]], must: dict[str, set[str]]) -> list[dict[str, Any]]:
    return [
        {
            "symbol": symbol,
            "mayDefinitionIds": sorted(may[symbol]),
            "mustDefinitionIds": sorted(must[symbol]),
        }
        for symbol in _SYMBOLS
    ]


def _lookup_call_anchor(
    *,
    helper_address: int,
    call_address: int,
    call_statement: str,
    h1_rows: dict[int, tuple[bytes, str]],
    h1_symbols: dict[str, int],
    rom: bytes,
) -> dict[str, Any]:
    """Validate the helper's one source-declared BSR.W lookup placeholder."""
    call_encoded, h1_statement = h1_rows[call_address]
    if h1_statement != call_statement:
        raise ValueError("map-event dialogue-state entity lookup H1 operand drift")
    call_parts = call_statement.split(" ", maxsplit=1)
    if len(call_parts) != 2 or call_parts[0] != "bsr.w":
        raise ValueError("map-event dialogue-state entity lookup source call operand drift")
    expected_target = h1_symbols.get(call_parts[1].lower())
    if expected_target is None:
        raise ValueError("map-event dialogue-state entity lookup target symbol drift")
    call_rom = rom[call_address : call_address + len(call_encoded)]
    decoded_target = _control_target(call_address, call_rom)
    word_bsr_placeholder = (
        len(call_encoded) == 4
        and call_encoded == b"\x61\x00\x00\x00"
        and call_rom[:2] == call_encoded[:2]
        and decoded_target == expected_target
    )
    if not word_bsr_placeholder:
        raise ValueError(
            "map-event dialogue-state entity lookup ROM call drift: "
            f"entryPc={helper_address:06X}; callPc={call_address:06X}; "
            f"h1Bytes={call_encoded.hex().upper()}; h1Statement={h1_statement!r}; "
            f"romBytes={call_rom.hex().upper()}; expectedTarget={expected_target:06X}; "
            f"decodedTarget={None if decoded_target is None else f'{decoded_target:06X}'}"
        )
    return {
        "id": "call:GetEntityPortaitAndSpeechSfx:lookup",
        "kind": "entity-lookup-call",
        "romPc": call_address,
        "instructionByteLength": len(call_encoded),
        "h1InstructionSha256": hashlib.sha256(call_encoded).hexdigest().upper(),
        "romInstructionSha256": hashlib.sha256(call_rom).hexdigest().upper(),
    }


def _dispatcher_prefill_writes(
    lines: list[str], *, dispatcher_line: int
) -> list[tuple[str, str, int, str]]:
    """Read the two source writes immediately following the entity lookup call."""
    lookup_statement = "bsr.w GetEntityPortaitAndSpeechSfx"
    call_lines = [
        line_number
        for line_number in range(dispatcher_line + 1, len(lines) + 1)
        if _normalise_statement(lines[line_number - 1]) == lookup_statement
    ]
    if len(call_lines) != 1:
        raise ValueError(
            "map-event dialogue-state entity dispatcher lookup source drift: "
            f"expected={lookup_statement!r}; actualLines={call_lines}"
        )
    writes: list[tuple[str, str, int, str]] = []
    for line_number in range(call_lines[0] + 1, len(lines) + 1):
        raw = lines[line_number - 1]
        if _LABEL.match(raw) is not None:
            break
        statement = _normalise_statement(raw)
        if not statement.startswith("move."):
            continue
        parts = statement.split(" ", maxsplit=1)
        operands = [] if len(parts) == 1 else [item.strip() for item in parts[1].split(",")]
        if len(operands) != 2:
            continue
        symbol = _direct_symbol(operands[1])
        if symbol in {"CURRENT_PORTRAIT", "CURRENT_SPEECH_SFX"}:
            writes.append((symbol, operands[0], line_number, statement))
    expected = (("CURRENT_SPEECH_SFX", "d2"), ("CURRENT_PORTRAIT", "d1"))
    actual = tuple((symbol, value) for symbol, value, _line, _statement in writes)
    if actual != expected:
        raise ValueError(
            "map-event dialogue-state entity dispatcher prefill source drift: "
            f"expected={expected}; actual={actual}"
        )
    return writes


def _external_anchors(
    *,
    disasm: Path,
    h1_rows: dict[int, tuple[bytes, str]],
    h1_symbols: dict[str, int],
    rom: bytes,
    map_events: dict[str, Any],
) -> tuple[list[dict[str, Any]], list[dict[str, Any]]]:
    """Guard dispatcher/helper entries, lookup call, and two prefill writes."""
    map_path = "code/common/scripting/map/mapsetupsfunctions_1.asm"
    helper_path = "code/common/scripting/entity/getentityportaitandspeechsfx.asm"
    map_lines = (disasm / map_path).read_text(encoding="utf-8").splitlines()
    helper_lines = (disasm / helper_path).read_text(encoding="utf-8").splitlines()

    def label_line(lines: list[str], symbol: str) -> int:
        matches = [
            index
            for index, line in enumerate(lines, start=1)
            if line.split(";", maxsplit=1)[0].strip() == f"{symbol}:"
        ]
        if len(matches) != 1:
            raise ValueError(f"map-event dialogue-state external label drift: {symbol}")
        return matches[0]

    dispatcher = "RunMapSetupEntityEvent"
    helper = "GetEntityPortaitAndSpeechSfx"
    dispatcher_address = map_events["function"][dispatcher]
    helper_address = 284216
    dispatcher_line = label_line(map_lines, dispatcher)
    helper_line = label_line(helper_lines, helper)

    def first_statement(lines: list[str], line: int) -> tuple[int, str]:
        for index in range(line, len(lines)):
            statement = _normalise_statement(lines[index])
            if statement:
                return index + 1, statement
        raise ValueError("map-event dialogue-state external body missing")

    def basic_anchor(
        anchor_id: str, kind: str, address: int, lines: list[str], line: int
    ) -> dict[str, Any]:
        _statement_line, statement = first_statement(lines, line + 1)
        row = h1_rows.get(address)
        if row is None or row[1] != statement:
            raise ValueError(f"map-event dialogue-state external H1 entry drift: {anchor_id}")
        encoded = row[0]
        rom_bytes = rom[address : address + len(encoded)]
        if rom_bytes != encoded:
            raise ValueError(f"map-event dialogue-state external ROM entry drift: {anchor_id}")
        return {
            "id": anchor_id,
            "kind": kind,
            "romPc": address,
            "instructionByteLength": len(encoded),
            "h1InstructionSha256": hashlib.sha256(encoded).hexdigest().upper(),
            "romInstructionSha256": hashlib.sha256(rom_bytes).hexdigest().upper(),
        }

    anchors = [
        basic_anchor(
            "entry:RunMapSetupEntityEvent",
            "dispatcher-entry",
            dispatcher_address,
            map_lines,
            dispatcher_line,
        ),
        basic_anchor(
            "entry:GetEntityPortaitAndSpeechSfx",
            "entity-lookup-entry",
            helper_address,
            helper_lines,
            helper_line,
        ),
    ]

    # The helper body has exactly one direct-call lookup.  Its result values
    # remain Unknown; this validates only source identity and call placement.
    helper_end = next(
        (
            index
            for index in range(helper_line, len(helper_lines))
            if _LABEL.match(helper_lines[index]) is not None
        ),
        len(helper_lines),
    )
    helper_call_rows = []
    for line_number in range(helper_line + 1, helper_end + 1):
        statement = _normalise_statement(helper_lines[line_number - 1])
        if statement.startswith("jsr ") or statement.startswith("bsr"):
            helper_call_rows.append((line_number, statement))
    if len(helper_call_rows) != 1:
        raise ValueError("map-event dialogue-state entity lookup call count drift")
    call_line, call_statement = helper_call_rows[0]
    # Locate by exact statement in the H1 listing after the helper entry,
    # bounded before the next source label so comments/symbol names cannot match.
    matching_addresses = [
        address
        for address, (_encoded, statement) in h1_rows.items()
        if statement == call_statement and helper_address <= address < helper_address + 256
    ]
    if len(matching_addresses) != 1:
        raise ValueError("map-event dialogue-state entity lookup H1 call drift")
    call_address = matching_addresses[0]
    anchors.append(
        _lookup_call_anchor(
            helper_address=helper_address,
            call_address=call_address,
            call_statement=call_statement,
            h1_rows=h1_rows,
            h1_symbols=h1_symbols,
            rom=rom,
        )
    )

    direct_writes = _dispatcher_prefill_writes(map_lines, dispatcher_line=dispatcher_line)
    for symbol, _value, _line_number, statement in direct_writes:
        matching_addresses = [
            address
            for address, (_encoded, h1_statement) in h1_rows.items()
            if h1_statement == statement
            and dispatcher_address <= address < dispatcher_address + 256
        ]
        if len(matching_addresses) != 1:
            raise ValueError("map-event dialogue-state entity dispatcher prefill H1 drift")
        address = matching_addresses[0]
        encoded = h1_rows[address][0]
        rom_bytes = rom[address : address + len(encoded)]
        if rom_bytes != encoded:
            raise ValueError("map-event dialogue-state entity dispatcher prefill ROM drift")
        anchors.append(
            {
                "id": f"prefill:{symbol}",
                "kind": "entity-dispatch-prefill-write",
                "romPc": address,
                "instructionByteLength": len(encoded),
                "h1InstructionSha256": hashlib.sha256(encoded).hexdigest().upper(),
                "romInstructionSha256": hashlib.sha256(rom_bytes).hexdigest().upper(),
            }
        )
    if len(anchors) != 5:
        raise ValueError("map-event dialogue-state external anchor denominator drift")
    return anchors, [
        {"path": map_path, "sourceLine": dispatcher_line},
        {"path": helper_path, "sourceLine": helper_line},
        {"path": helper_path, "sourceLine": call_line},
    ]


def _projection(
    retained: dict[str, Any], *, upstream_path: Path, rom_path: Path
) -> tuple[dict[str, Any], dict[str, Any], dict[str, Any]]:
    map_events = retained["mapEvents"]
    direct_state = retained["eventDirectState"]
    disasm = _disasm_root(upstream_path)
    listing_path = upstream_path.resolve(strict=True) / "build/sf2build-h1.lst"
    listing_text = listing_path.read_text(encoding="utf-8")
    h1_rows = _h1_instruction_rows(listing_text)
    h1_symbols = _h1_symbol_addresses(listing_text)
    rom = rom_path.resolve(strict=True).read_bytes()
    source_text: dict[str, list[str]] = {}
    for path in _SUPPORT_SOURCE_PATHS:
        source_text[path] = (disasm / path).read_text(encoding="utf-8").splitlines()
    constants = _parse_equates("\n".join(source_text["sf2const.asm"]), source_path="sf2const.asm")
    if set(_SYMBOLS) - set(constants):
        raise ValueError("map-event dialogue-state symbol definition drift")

    table_rows = _table_rows(map_events)
    selected = _selected_programs(map_events, direct_state)
    continuation_bindings = _contextual_continuation_bindings(selected, disasm=disasm)
    direct_accesses: dict[tuple[str, str, int, int], list[dict[str, Any]]] = {}
    for access in direct_state["accessSites"]:
        if access["symbol"] not in _SYMBOLS:
            continue
        key = (
            access["category"],
            access["programSymbol"],
            access["programEntryAddress"],
            access["romPc"],
        )
        direct_accesses.setdefault(key, []).append(access)

    operation_definitions = {row["definitionId"]: row for row in map_events["operationDefinitions"]}
    physical_operations: dict[int, dict[str, Any]] = {}
    physical_relocations: dict[int, tuple[str, ...]] = {}
    relocation_counts: Counter[str] = Counter()
    macro_relocation_counts: Counter[str] = Counter()
    flows: list[dict[str, Any]] = []
    text_sites: list[dict[str, Any]] = []
    return_sites: list[dict[str, Any]] = []
    source_file_rows: list[dict[str, Any]] = []
    seen_tables: set[tuple[str, str, int]] = set()
    analyzed_states: dict[
        tuple[str, str, int],
        tuple[dict[int, dict[str, set[str]]], dict[int, dict[str, set[str]]]],
    ] = {}

    for category, program in selected:
        table = table_rows.get((category, program["sourcePath"]))
        if table is None:
            raise ValueError("map-event dialogue-state table-owner join drift")
        table_key = (category, table["symbol"], table["address"])
        if table_key not in seen_tables:
            seen_tables.add(table_key)
            source_file_rows.append(
                {
                    "category": category,
                    "tableSymbol": table["symbol"],
                    "tableEntryAddress": table["address"],
                    "sourcePath": table["path"],
                }
            )
        if program["sourcePath"] not in source_text:
            source_text[program["sourcePath"]] = (
                (disasm / program["sourcePath"]).read_text(encoding="utf-8").splitlines()
            )
        lines = source_text[program["sourcePath"]]
        definitions_by_order: dict[int, dict[str, str]] = {}
        operation_shapes: list[dict[str, Any]] = []
        for operation_index, operation in enumerate(program["operations"]):
            context = f"{program['canonicalSymbol']}:{operation['sourceLine']}"
            _assert_source_statement(
                lines,
                source_line=operation["sourceLine"],
                expected=_operation_statement(operation),
                context=context,
            )
            definition_ids = _definition_ids(category, program, operation, direct_accesses)
            definitions_by_order[operation["sourceOrder"]] = definition_ids
            operation_shapes.append(
                {
                    **_operation_shape(operation),
                    "definitionIds": [
                        definition_ids[symbol] for symbol in _SYMBOLS if symbol in definition_ids
                    ],
                }
            )
            next_address = (
                program["operations"][operation_index + 1]["address"]
                if operation_index + 1 < len(program["operations"])
                else None
            )
            physical = _physical_anchor(
                operation=operation,
                next_address=next_address,
                operation_definitions=operation_definitions,
                h1_rows=h1_rows,
                h1_symbols=h1_symbols,
                rom=rom,
                context=context,
            )
            anchor = {
                "id": f"operation:{operation['address']:06X}",
                "kind": "target-program-operation",
                "romPc": operation["address"],
                "instructionByteLength": physical["instructionByteLength"],
                "h1InstructionSha256": physical["h1InstructionSha256"],
                "romInstructionSha256": physical["romInstructionSha256"],
            }
            prior = physical_operations.setdefault(operation["address"], anchor)
            if prior != anchor:
                raise ValueError("map-event dialogue-state shared physical operation drift")
            relocation_kinds = tuple(physical["relocationKinds"])
            prior_kinds = physical_relocations.setdefault(operation["address"], relocation_kinds)
            if prior_kinds != relocation_kinds:
                raise ValueError("map-event dialogue-state shared physical relocation drift")
            if prior is anchor:
                relocation_counts.update(relocation_kinds)
                if operation["family"] == "event-service-macro":
                    macro_relocation_counts.update(relocation_kinds)

        edges, successors = _program_edges(program)
        continuation_entry: dict[int, tuple[dict[str, set[str]], dict[str, set[str]]]] = {}
        continuation_binding = continuation_bindings.get(_program_key(category, program))
        continuation_from: dict[str, Any] | None = None
        if continuation_binding is not None:
            if successors[0]:
                raise ValueError("map-event dialogue-state synthetic continuation edge drift")
            owner_states = analyzed_states.get(continuation_binding["ownerKey"])
            if owner_states is None:
                raise ValueError("map-event dialogue-state continuation owner order drift")
            owner_may, owner_must = owner_states
            owner_order = continuation_binding["ownerSourceOrder"]
            continuation_entry[continuation_binding["continuationSourceOrder"]] = (
                owner_may[owner_order],
                owner_must[owner_order],
            )
            _owner_category, owner_symbol, owner_entry_address = continuation_binding["ownerKey"]
            continuation_from = {
                "programSymbol": owner_symbol,
                "programEntryAddress": owner_entry_address,
                "sourceOrder": owner_order,
                "romPc": continuation_binding["romPc"],
            }
        in_may, in_must = _reaching_definitions(
            category,
            program,
            definitions_by_order,
            successors,
            continuation_entries=continuation_entry,
        )
        analyzed_states[_program_key(category, program)] = (in_may, in_must)
        labels = [
            {
                "sourceOrder": row["sourceOrder"],
                "symbol": row["symbol"],
                "romPc": row["address"],
            }
            for row in program["labels"]
        ]
        flows.append(
            {
                "category": category,
                "programSymbol": program["canonicalSymbol"],
                "programEntryAddress": program["entryAddress"],
                "tableSymbol": table["symbol"],
                "sourcePath": program["sourcePath"],
                "labels": labels,
                "operations": operation_shapes,
                "edges": edges,
                "continuationFrom": continuation_from,
            }
        )
        for operation in program["operations"]:
            order = operation["sourceOrder"]
            if operation["sourceMnemonic"] in {"txt", "clsTxt"}:
                if operation["sourceMnemonic"] == "txt":
                    if (
                        len(operation["operandTexts"]) != 1
                        or not operation["operandTexts"][0].isdigit()
                    ):
                        raise ValueError("map-event dialogue-state txt operand drift")
                    text_id = int(operation["operandTexts"][0])
                    if not 0 <= text_id <= 4266:
                        raise ValueError("map-event dialogue-state txt identifier-domain drift")
                    kind = "line-reference"
                else:
                    if operation["operandTexts"]:
                        raise ValueError("map-event dialogue-state clsTxt operand drift")
                    text_id = 65535
                    kind = "close-sentinel"
                text_sites.append(
                    {
                        "siteOrder": len(text_sites),
                        "category": category,
                        "programSymbol": program["canonicalSymbol"],
                        "programEntryAddress": program["entryAddress"],
                        "tableSymbol": table["symbol"],
                        "sourceOrder": order,
                        "sourceLine": operation["sourceLine"],
                        "romPc": operation["address"],
                        "kind": kind,
                        "textIdOrCloseSentinel": text_id,
                        "state": _state_set_row(in_may[order], in_must[order]),
                    }
                )
            if operation["controlFlowKind"] == "return":
                return_sites.append(
                    {
                        "siteOrder": len(return_sites),
                        "category": category,
                        "programSymbol": program["canonicalSymbol"],
                        "programEntryAddress": program["entryAddress"],
                        "tableSymbol": table["symbol"],
                        "sourceOrder": order,
                        "sourceLine": operation["sourceLine"],
                        "romPc": operation["address"],
                        "state": _state_set_row(in_may[order], in_must[order]),
                    }
                )

    source_file_rows = _canonical_source_file_rows(source_file_rows)
    if dict(relocation_counts) != _EXPECTED_RELOCATION_KINDS:
        raise ValueError("map-event dialogue-state relocation-kind denominator drift")
    if dict(macro_relocation_counts) != _EXPECTED_MACRO_RELOCATION_KINDS:
        raise ValueError("map-event dialogue-state macro relocation-kind denominator drift")
    external_anchors, external_source_locations = _external_anchors(
        disasm=disasm,
        h1_rows=h1_rows,
        h1_symbols=h1_symbols,
        rom=rom,
        map_events=map_events,
    )
    anchors = list(physical_operations.values()) + external_anchors
    if len(physical_operations) != 374 or len(anchors) != 379:
        raise ValueError("map-event dialogue-state source/H1/ROM anchor denominator drift")

    symbol_definitions = [
        {
            "symbol": symbol,
            "address": constants[symbol]["value"],
            "sourcePath": "sf2const.asm",
            "sourceLine": constants[symbol]["sourceLine"],
        }
        for symbol in _SYMBOLS
    ]
    entry_state_rules: list[dict[str, Any]] = []
    for category in _CATEGORIES:
        for symbol in _SYMBOLS:
            definition_id = next(iter(_entry_state(category)[symbol]))
            entry_state_rules.append(
                {
                    "id": definition_id,
                    "category": category,
                    "symbol": symbol,
                    "kind": "entity-dispatch-prefill"
                    if definition_id.startswith("entry:entity-dispatch")
                    else "inherited-entry-state",
                    "anchorId": (
                        f"prefill:{symbol}"
                        if definition_id.startswith("entry:entity-dispatch")
                        else None
                    ),
                }
            )
    entry_state_rules.extend(
        [
            {
                "id": "lookup:*:CURRENT_PORTRAIT",
                "category": "entityEvents",
                "symbol": "CURRENT_PORTRAIT",
                "kind": "entity-lookup-call-definition",
                "anchorId": "call:GetEntityPortaitAndSpeechSfx:lookup",
            },
            {
                "id": "lookup:*:CURRENT_SPEECH_SFX",
                "category": "entityEvents",
                "symbol": "CURRENT_SPEECH_SFX",
                "kind": "entity-lookup-call-definition",
                "anchorId": "call:GetEntityPortaitAndSpeechSfx:lookup",
            },
        ]
    )

    context_operation_count = sum(len(row["operations"]) for row in flows)
    context_label_count = sum(len(row["labels"]) for row in flows)
    control_counts = {
        kind: sum(
            operation["controlFlowKind"] == kind for row in flows for operation in row["operations"]
        )
        for kind in (
            "ordinary",
            "conditional-branch",
            "unconditional-branch",
            "direct-call",
            "direct-jump",
            "return",
        )
    }
    selected_program_keys = {
        (category, program["canonicalSymbol"], program["entryAddress"])
        for category, program in selected
    }
    target_accesses = [
        row
        for rows in direct_accesses.values()
        for row in rows
        if (row["category"], row["programSymbol"], row["programEntryAddress"])
        in selected_program_keys
    ]
    physical_state_pcs = _physical_state_access_pcs(target_accesses)
    summary = {
        "motherProgramContextCount": 914,
        "motherOperationCount": 3579,
        "positiveProgramContextCount": len(flows),
        "zeroProgramContextCount": 914 - len(flows),
        "sourceFileCount": len(source_file_rows),
        "sourceIdentityCount": len(source_file_rows) + len(_SUPPORT_SOURCE_PATHS),
        "symbolDefinitionCount": len(symbol_definitions),
        "contextStateAccessCount": len(target_accesses),
        "physicalStateAccessCount": len(physical_state_pcs),
        "textBearingProgramContextCount": len({row["programEntryAddress"] for row in text_sites}),
        "stateOnlyProgramContextCount": len(flows)
        - len({row["programEntryAddress"] for row in text_sites}),
        "contextTextSiteCount": len(text_sites),
        "physicalTextSiteCount": len({row["romPc"] for row in text_sites}),
        "contextLineReferenceCount": sum(row["kind"] == "line-reference" for row in text_sites),
        "contextCloseSentinelCount": sum(row["kind"] == "close-sentinel" for row in text_sites),
        "contextStateOrTextSiteCount": len(target_accesses) + len(text_sites),
        "physicalStateOrTextPcCount": len(
            physical_state_pcs | {row["romPc"] for row in text_sites}
        ),
        "contextOperationCount": context_operation_count,
        "physicalOperationCount": len(physical_operations),
        "contextLabelCount": context_label_count,
        "physicalLabelCount": len({label["romPc"] for row in flows for label in row["labels"]}),
        "ordinaryOperationCount": control_counts["ordinary"],
        "conditionalBranchCount": control_counts["conditional-branch"],
        "unconditionalBranchCount": control_counts["unconditional-branch"],
        "directCallCount": control_counts["direct-call"],
        "directJumpCount": control_counts["direct-jump"],
        "returnCount": control_counts["return"],
        "returnStateSiteCount": len(return_sites),
        "h1RomAnchorCount": len(anchors),
    }
    expected_summary = {
        "motherProgramContextCount": 914,
        "motherOperationCount": 3579,
        "positiveProgramContextCount": 24,
        "zeroProgramContextCount": 890,
        "sourceFileCount": 17,
        "sourceIdentityCount": 22,
        "symbolDefinitionCount": 5,
        "contextStateAccessCount": 100,
        "physicalStateAccessCount": 72,
        "textBearingProgramContextCount": 23,
        "stateOnlyProgramContextCount": 1,
        "contextTextSiteCount": 89,
        "physicalTextSiteCount": 76,
        "contextLineReferenceCount": 69,
        "contextCloseSentinelCount": 20,
        "contextStateOrTextSiteCount": 189,
        "physicalStateOrTextPcCount": 148,
        "contextOperationCount": 414,
        "physicalOperationCount": 374,
        "contextLabelCount": 72,
        "physicalLabelCount": 64,
        "ordinaryOperationCount": 278,
        "conditionalBranchCount": 40,
        "unconditionalBranchCount": 20,
        "directCallCount": 49,
        "directJumpCount": 1,
        "returnCount": 26,
        "returnStateSiteCount": 26,
        "h1RomAnchorCount": 379,
    }
    if summary != expected_summary:
        mismatch = {
            key: {"expected": expected_summary[key], "actual": summary.get(key)}
            for key in expected_summary
            if summary.get(key) != expected_summary[key]
        }
        raise ValueError(
            "map-event dialogue-state bounded denominator drift: "
            f"expected={expected_summary}; actual={summary}; mismatch={mismatch}"
        )
    event_dialogue_state = {
        "symbolDefinitions": symbol_definitions,
        "symbolDefinitionOrder": [
            f"{row['symbol']}:{row['address']}" for row in symbol_definitions
        ],
        "entryStateRules": entry_state_rules,
        "entryStateRuleOrder": [row["id"] for row in entry_state_rules],
        "programFlows": flows,
        "programFlowOrder": [
            f"{row['category']}|{row['programSymbol']}|{row['programEntryAddress']}"
            for row in flows
        ],
        "textStateSites": text_sites,
        "textStateSiteOrder": [
            f"{row['category']}|{row['programSymbol']}|{row['programEntryAddress']}|{row['romPc']}|{row['kind']}|{row['textIdOrCloseSentinel']}"
            for row in text_sites
        ],
        "returnStateSites": return_sites,
        "returnStateSiteOrder": [
            f"{row['category']}|{row['programSymbol']}|{row['programEntryAddress']}|{row['romPc']}"
            for row in return_sites
        ],
        "sourceFiles": {row["tableSymbol"]: row for row in source_file_rows},
        "sourceFileOrder": [
            f"{row['category']}|{row['tableSymbol']}|{row['tableEntryAddress']}"
            for row in source_file_rows
        ],
        "digests": {
            "programFlowsSha256": hashlib.sha256(canonical_json_bytes({"programFlows": flows}))
            .hexdigest()
            .upper(),
            "textStateSitesSha256": hashlib.sha256(
                canonical_json_bytes({"textStateSites": text_sites})
            )
            .hexdigest()
            .upper(),
            "returnStateSitesSha256": hashlib.sha256(
                canonical_json_bytes({"returnStateSites": return_sites})
            )
            .hexdigest()
            .upper(),
        },
    }
    source_identities = [
        {
            "path": row["sourcePath"],
            "sha256": hashlib.sha256((disasm / row["sourcePath"]).read_bytes()).hexdigest().upper(),
        }
        for row in source_file_rows
    ] + [
        {
            "path": path,
            "sha256": hashlib.sha256((disasm / path).read_bytes()).hexdigest().upper(),
        }
        for path in _SUPPORT_SOURCE_PATHS
    ]
    source_context = {
        "h1Listing": {
            "path": "build/sf2build-h1.lst",
            "sha256": hashlib.sha256(listing_text.encode("utf-8")).hexdigest().upper(),
        },
        "sourceIdentities": source_identities,
        "anchors": anchors,
        "externalSourceLocations": external_source_locations,
    }
    return event_dialogue_state, summary, source_context


def build_map_event_dialogue_state_contract(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    retained = _fresh_retained_owners(rom_path, upstream_path)
    state, summary, source_context = _projection(
        retained, upstream_path=upstream_path, rom_path=rom_path
    )
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {
            "repository": "ShiningForceCentral/SF2DISASM",
            "commit": "c834c652b6862bc5679fd7f69a38a7093206efc6",
        },
        "romSha256": "9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9",
        "sourceContext": source_context,
        "retainedOwners": retained["projections"],
        "eventDialogueState": state,
        "unknowns": {key: "Unknown" for key in _UNKNOWN_KEYS},
        "summary": summary,
    }


def verify_map_event_dialogue_state_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    output = build_map_event_dialogue_state_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="map-event dialogue-state rebuilt contract")
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner="map-event dialogue-state fixture")
    if output != fixture:
        raise ValueError("map-event dialogue-state fixture drift")
    destination = output_path or FIXTURE
    if output_path is not None:
        destination.parent.mkdir(parents=True, exist_ok=True)
        destination.write_bytes(canonical_json_bytes(output))
    digest = hashlib.sha256(canonical_json_bytes(output)).hexdigest().upper()
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Programs": output["summary"]["positiveProgramContextCount"],
        "TextSites": output["summary"]["contextTextSiteCount"],
        "Status": "PASS",
    }
