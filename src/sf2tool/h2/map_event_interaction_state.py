"""Static H2 interaction-state contract for map-event fixed-RAM joins.

This narrow owner joins the two remaining fixed-RAM symbols across bounded
callers, producers, and consumers.  It deliberately does not reopen the
retained map-event tables, selection routines, callee bodies, or runtime
meaning.
"""

from __future__ import annotations

import hashlib
import json
import re
from pathlib import Path
from typing import Any

from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path

ID = "sf2-map-event-interaction-state-static-v1"
FIXTURE = repo_path("tests/fixtures/h2/map-event-interaction-state-static-v1.json")
SCHEMA = repo_path("schemas/h2/map-event-interaction-state-static-fixture.schema.json")

_ROM_SHA256 = "9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9"
_UPSTREAM_COMMIT = "c834c652b6862bc5679fd7f69a38a7093206efc6"
_SYMBOLS = ("ENTITY_FACING", "EVENT_RELATIVE_POSITION")
_SOURCE_PATHS = (
    "sf2const.asm",
    "code/gameflow/exploration/explorationfunctions_0.asm",
    "code/gameflow/exploration/explorationvints.asm",
    "code/gameflow/battle/battlefunctions/battlefunctions_0.asm",
    "code/common/tech/jumpinterfaces/s07_jumpinterface.asm",
    "code/common/menus/getplayerentityposition.asm",
    "code/common/menus/main/mainactions.asm",
    "code/common/scripting/map/mapsetupsfunctions_1.asm",
    "code/gameflow/exploration/explorationfunctions_2.asm",
    "data/maps/entries/map06/mapsetups/s2_entityevents_701.asm",
    "data/maps/entries/map09/mapsetups/s2_entityevents.asm",
    "data/maps/entries/map28/mapsetups/s3_zoneevents.asm",
)
_UNKNOWN_KEYS = (
    "naturalEntityInteractionReachability",
    "activatedEntityIdentity",
    "entityFacingValue",
    "playerFacingValue",
    "eventRelativePositionRuntimeValue",
    "predicateBranchTaken",
    "itemInvocationReachability",
    "itemPlayerYRuntimeValue",
    "itemHandlerSelectionAndOutcome",
    "zoneInvocationReachability",
    "zoneBranchAndScript",
    "stateLifetimePersistenceTimingPresentationStoryMeaning",
)

_INDEX_FIXTURE = "tests/fixtures/h2/map-event-interaction-state-static-v1.json"
_INDEX_DOCUMENT = "docs/research/map-event-interaction-state.md"
_INDEX_DELTA = {
    "gameflow.exploration.interaction": (
        (),
        (("entry", "interactionState.dispatchContexts.entityAcquisition.entryAddress"),),
    ),
    "gameflow.exploration.actions": (
        (("entity-event-dispatch", 154538),),
        (
            (
                "entity-event-dispatch",
                "interactionState.dispatchContexts.entityDispatch.processPlayerActionAddress",
            ),
        ),
    ),
    "battle.functions.control-cursor": (
        (("entity-event-index", 143178),),
        (
            (
                "entity-event-index",
                "interactionState.dispatchContexts.entityDispatch.getEntityEventIndexAddress",
            ),
        ),
    ),
    "tech.interfaces.jump-s07": (
        (
            ("run-entity-event", 278652),
            ("run-zone-event", 278656),
            ("run-entity-event-0", 278660),
            ("run-item-event", 278664),
        ),
        (
            (
                "run-entity-event",
                "interactionState.jumpInterfaces.runMapSetupEntityEvent.entryAddress",
            ),
            ("run-zone-event", "interactionState.jumpInterfaces.runMapSetupZoneEvent.entryAddress"),
            (
                "run-entity-event-0",
                "interactionState.jumpInterfaces.runMapSetupEntityEvent0.entryAddress",
            ),
            ("run-item-event", "interactionState.jumpInterfaces.runMapSetupItemEvent.entryAddress"),
        ),
    ),
    "menus.player-position": (
        (),
        (
            (
                "entry",
                "interactionState.dispatchContexts.itemInvocation.getPlayerEntityPositionAddress",
            ),
        ),
    ),
    "menus.field-main": (
        (("run-map-setup-item-event-call", 136490),),
        (
            (
                "run-map-setup-item-event-call",
                "interactionState.dispatchContexts.itemInvocation.runMapSetupItemEventCallAddress",
            ),
        ),
    ),
    "map.setup.item-event": (
        (),
        (("entry", "interactionState.producerWrites.itemEventD2.entryAddress"),),
    ),
    "map.setup.entity-event": (
        (("get-rhode-facing", 292914),),
        (
            ("entry", "interactionState.producerWrites.entityEventD2.entryAddress"),
            ("get-rhode-facing", "interactionState.consumerReads.getRhodeFacing.entryAddress"),
        ),
    ),
    "gameflow.exploration.loop": (
        (("zone-event-dispatch", 154236),),
        (
            (
                "zone-event-dispatch",
                "interactionState.dispatchContexts.zoneFacing.processMapEventType6Address",
            ),
        ),
    ),
    "map.setup.zone-event": (
        (),
        (("entry", "interactionState.dispatchContexts.zoneFacing.runMapSetupZoneEventAddress"),),
    ),
    "map.data.ms-map6-flag701-entityevents": (
        (),
        (("entry", "interactionState.consumerReads.map6.tableEntryAddress"),),
    ),
    "map.data.ms-map9-entityevents": (
        (),
        (("entry", "interactionState.consumerReads.map9.tableEntryAddress"),),
    ),
    "map.data.ms-map28-zoneevents": (
        (),
        (("entry", "interactionState.consumerReads.map28.tableEntryAddress"),),
    ),
}

_H1_INSTRUCTION = re.compile(
    r"^([0-9A-Fa-f]{8})\s+([0-9A-Fa-f]{2,4}(?: [0-9A-Fa-f]{2,4})*)\s{2,}(.+)$"
)
_H1_LABEL = re.compile(r"^([0-9A-Fa-f]{8})\s+([A-Za-z_@][A-Za-z0-9_@]*):\s*$")
_EQUATE = re.compile(r"^\s*([A-Za-z_][A-Za-z0-9_]*)\s*:\s+equ\s+([^\s;]+)")
_LABEL = re.compile(r"^(?:[A-Za-z_@][A-Za-z0-9_@]*):\s*(.*)$")


def canonical_json_bytes(value: dict[str, Any]) -> bytes:
    """Return the public fixture's canonical UTF-8 serialization."""
    return (
        json.dumps(value, ensure_ascii=False, sort_keys=True, separators=(",", ":")).encode("utf-8")
        + b"\n"
    )


def normalize_interaction_state_later_owner_index(index: dict[str, Any]) -> dict[str, Any]:
    """Remove only this accepted slice's exact index delta for legacy-owner tests."""
    normalized = json.loads(json.dumps(index))
    seen: set[str] = set()
    for record in normalized["records"]:
        record_id = record["id"]
        evidence = [item for item in record["evidence"] if item["fixtureId"] == ID]
        if not evidence:
            continue
        if record_id not in _INDEX_DELTA or len(evidence) != 1:
            raise ValueError("interaction-state later-owner record drift")
        addresses, bindings = _INDEX_DELTA[record_id]
        expected = {
            "level": "H2",
            "fixture": _INDEX_FIXTURE,
            "fixtureId": ID,
            "verifier": "src/sf2tool/h2/map_event_interaction_state.py",
            "bindings": [
                {"addressId": address_id, "fixtureField": field} for address_id, field in bindings
            ],
        }
        if (
            evidence[0] != expected
            or record["documents"].count(_INDEX_DOCUMENT) != 1
            or record["documents"][-1] != _INDEX_DOCUMENT
        ):
            raise ValueError("interaction-state later-owner evidence/document drift")
        for address_id, value in addresses:
            matches = [item for item in record["addresses"] if item["id"] == address_id]
            if matches != [
                {"id": address_id, "space": "rom", "kind": "observation", "value": value}
            ]:
                raise ValueError("interaction-state later-owner address drift")
            record["addresses"].remove(matches[0])
        record["evidence"].remove(evidence[0])
        record["documents"].remove(_INDEX_DOCUMENT)
        seen.add(record_id)
    if seen != set(_INDEX_DELTA):
        raise ValueError("interaction-state later-owner coverage drift")
    return normalized


def _normalise(statement: str) -> str:
    normalised = re.sub(r"\s*,\s*", ",", re.sub(r"\s+", " ", statement.split(";", 1)[0].strip()))
    return re.sub(r"^M ", "", normalised)


def _disasm_root(upstream_path: Path) -> Path:
    root = upstream_path.resolve(strict=True)
    return root / "disasm" if (root / "disasm").is_dir() else root


def _parse_equates(lines: list[str]) -> dict[str, dict[str, int]]:
    output: dict[str, dict[str, int]] = {}
    for line_number, line in enumerate(lines, start=1):
        match = _EQUATE.match(line)
        if match is None or not match.group(2).startswith("$"):
            continue
        output[match.group(1)] = {
            "sourceLine": line_number,
            "value": int(match.group(2)[1:], 16),
        }
    return output


def _parse_h1(listing_text: str) -> tuple[dict[int, tuple[bytes, str]], dict[str, list[int]]]:
    rows: dict[int, tuple[bytes, str]] = {}
    labels: dict[str, list[int]] = {}
    for raw_line in listing_text.splitlines():
        instruction = _H1_INSTRUCTION.match(raw_line)
        if instruction is not None:
            address = int(instruction.group(1), 16)
            row = (
                bytes.fromhex(instruction.group(2).replace(" ", "")),
                _normalise(instruction.group(3)),
            )
            if address in rows and rows[address] != row:
                raise ValueError(
                    f"map-event interaction-state ambiguous H1 instruction: {address:#x}"
                )
            rows[address] = row
            continue
        label = _H1_LABEL.match(raw_line)
        if label is not None:
            name = label.group(2)
            address = int(label.group(1), 16)
            labels.setdefault(name, []).append(address)
    return rows, labels


def _label_target(
    labels: dict[str, list[int]], name: str | None, source_address: int
) -> int | None:
    candidates = labels.get(name or "", [])
    return (
        min(candidates, key=lambda candidate: abs(candidate - source_address))
        if candidates
        else None
    )


def _source_statement(line: str) -> str | None:
    statement = line.split(";", 1)[0].strip()
    if not statement:
        return None
    label = _LABEL.match(statement)
    if label is not None:
        statement = label.group(1).strip()
    return _normalise(statement) if statement else None


def _source_operations(lines: list[str], source_lines: tuple[int, ...]) -> list[tuple[int, str]]:
    output: list[tuple[int, str]] = []
    for line_number in source_lines:
        if not 1 <= line_number <= len(lines):
            raise ValueError("map-event interaction-state source line range drift")
        statement = _source_statement(lines[line_number - 1])
        if statement is not None:
            output.append((line_number, statement))
    return output


def _expand_source_statement(statement: str) -> list[str]:
    # The source macro is one source operation but intentionally expands to the
    # two H1 instructions whose bytes are included in each function interval.
    if statement == "clsTxt":
        return ["trap #textbox", "dc.w $ffff"]
    return [statement]


def _segment(
    *,
    spec: dict[str, Any],
    source_text: dict[str, list[str]],
    h1_rows: dict[int, tuple[bytes, str]],
    h1_labels: dict[str, list[int]],
    rom: bytes,
) -> dict[str, Any]:
    operations = _source_operations(source_text[spec["sourcePath"]], spec["sourceLines"])
    if len(operations) != spec["sourceOperationCount"]:
        raise ValueError(f"map-event interaction-state source operation count drift: {spec['id']}")
    tail_count = spec.get("sourceTailOperationCount", 0)
    if tail_count:
        tails = [statement for _line, statement in operations[-tail_count:]]
        if tails != list(spec["sourceTailStatements"]):
            raise ValueError(f"map-event interaction-state source terminal drift: {spec['id']}")
    expected_h1 = [
        item
        for _line, statement in operations[: len(operations) - tail_count]
        for item in _expand_source_statement(statement)
    ]
    selected_rows: list[tuple[int, bytes, str]] = []
    for start, end in spec["h1Ranges"]:
        cursor = start
        rows = [
            (address, *h1_rows[address]) for address in sorted(h1_rows) if start <= address < end
        ]
        if not rows:
            raise ValueError(f"map-event interaction-state H1 range missing: {spec['id']}")
        for address, encoded, statement in rows:
            if address != cursor:
                raise ValueError(f"map-event interaction-state H1 range gap/reorder: {spec['id']}")
            cursor += len(encoded)
            selected_rows.append((address, encoded, statement))
        if cursor != end:
            raise ValueError(f"map-event interaction-state H1 range boundary drift: {spec['id']}")
    actual_h1 = [statement for _address, _encoded, statement in selected_rows]
    if actual_h1 != expected_h1:
        raise ValueError(
            f"map-event interaction-state source/H1 operation-order drift: {spec['id']}"
        )
    if tail_count:
        terminal = h1_rows.get(spec["h1Ranges"][-1][1])
        if terminal is None or terminal[1] != spec["sourceTailStatements"][0]:
            raise ValueError(f"map-event interaction-state H1 terminal drift: {spec['id']}")
    h1_bytes = b"".join(encoded for _address, encoded, _statement in selected_rows)
    rom_rows: list[bytes] = []
    for address, encoded, statement in selected_rows:
        rom_bytes = rom[address : address + len(encoded)]
        if len(rom_bytes) != len(encoded):
            raise ValueError(f"map-event interaction-state ROM interval drift: {spec['id']}")
        if rom_bytes != encoded:
            target_name = _control_target_name(statement)
            target_address = _label_target(h1_labels, target_name, address)
            if (
                target_address is None
                or _rom_control_target(address, rom_bytes) != target_address
                or not _relocation_opcode_matches(encoded, rom_bytes)
            ):
                raise ValueError(f"map-event interaction-state H1/ROM byte drift: {spec['id']}")
        rom_rows.append(rom_bytes)
    rom_bytes = b"".join(rom_rows)
    expected_bytes = sum(end - start for start, end in spec["h1Ranges"])
    if len(h1_bytes) != expected_bytes:
        raise ValueError(f"map-event interaction-state H1 byte count drift: {spec['id']}")
    return {
        "id": spec["id"],
        "sourcePath": spec["sourcePath"],
        "entrySymbol": spec["entrySymbol"],
        "startAddress": spec["h1Ranges"][0][0],
        "endAddressExclusive": spec["h1Ranges"][-1][1],
        "sourceOperationCount": len(operations),
        "h1RomByteLength": len(h1_bytes),
        "h1InstructionSha256": hashlib.sha256(h1_bytes).hexdigest().upper(),
        "romInstructionSha256": hashlib.sha256(rom_bytes).hexdigest().upper(),
    }


def _control_target_name(statement: str) -> str | None:
    mnemonic, *operand_parts = statement.split(maxsplit=1)
    if not operand_parts or not (mnemonic.startswith("b") or mnemonic in {"jsr", "jmp"}):
        return None
    operand = operand_parts[0].split(",", 1)[-1].strip()
    return operand.removesuffix("(pc)")


def _rom_control_target(address: int, encoded: bytes) -> int | None:
    if len(encoded) < 2:
        return None
    opcode = int.from_bytes(encoded[:2], byteorder="big")
    if opcode & 0xF000 == 0x6000:
        if len(encoded) == 2 and encoded[1] != 0:
            return address + 2 + int.from_bytes(encoded[1:], byteorder="big", signed=True)
        if len(encoded) == 4 and encoded[1] == 0:
            return address + 2 + int.from_bytes(encoded[2:], byteorder="big", signed=True)
    if opcode in {0x4EBA, 0x4EFA} and len(encoded) == 4:
        return address + 2 + int.from_bytes(encoded[2:], byteorder="big", signed=True)
    if opcode in {0x4EB9, 0x4EF9} and len(encoded) == 6:
        return int.from_bytes(encoded[2:], byteorder="big")
    return None


def _relocation_opcode_matches(h1_bytes: bytes, rom_bytes: bytes) -> bool:
    return (
        h1_bytes[:1] == rom_bytes[:1]
        if len(h1_bytes) == 2 and h1_bytes[0] & 0xF0 == 0x60
        else h1_bytes[:2] == rom_bytes[:2]
    )


_FUNCTION_SPECS = (
    {
        "id": "get-activated-entity",
        "sourcePath": _SOURCE_PATHS[1],
        "entrySymbol": "GetActivatedEntity",
        "sourceLines": tuple(range(12, 91)),
        "sourceOperationCount": 57,
        "sourceTailOperationCount": 1,
        "sourceTailStatements": ("rts",),
        "h1Ranges": ((0x2379A, 0x23844),),
    },
    {
        "id": "get-entity-event-index",
        "sourcePath": _SOURCE_PATHS[3],
        "entrySymbol": "GetEntityEventIndex",
        "sourceLines": tuple(range(333, 357)),
        "sourceOperationCount": 15,
        "h1Ranges": ((0x22F4A, 0x22F76),),
    },
    {
        "id": "get-player-entity-position",
        "sourcePath": _SOURCE_PATHS[5],
        "entrySymbol": "GetPlayerEntityPosition",
        "sourceLines": tuple(range(10, 21)),
        "sourceOperationCount": 9,
        "h1Ranges": ((0x22C60, 0x22C84),),
    },
    {
        "id": "run-map-setup-item-event",
        "sourcePath": _SOURCE_PATHS[7],
        "entrySymbol": "RunMapSetupItemEvent",
        "sourceLines": tuple(range(90, 153)),
        "sourceOperationCount": 39,
        "h1Ranges": ((0x47586, 0x4761A),),
    },
    {
        "id": "run-map-setup-entity-event",
        "sourcePath": _SOURCE_PATHS[7],
        "entrySymbol": "RunMapSetupEntityEvent",
        "sourceLines": tuple(range(162, 246)),
        "sourceOperationCount": 59,
        "h1Ranges": ((0x4761A, 0x476DC),),
    },
    {
        "id": "get-rhode-facing",
        "sourcePath": _SOURCE_PATHS[7],
        "entrySymbol": "GetRhodeFacing",
        "sourceLines": tuple(range(488, 500)),
        "sourceOperationCount": 10,
        "h1Ranges": ((0x47832, 0x47856),),
    },
    {
        "id": "process-map-event-type6-zone-event",
        "sourcePath": _SOURCE_PATHS[8],
        "entrySymbol": "ProcessMapEventType6_ZoneEvent",
        "sourceLines": tuple(range(390, 398)),
        "sourceOperationCount": 6,
        "h1Ranges": ((0x25A7C, 0x25A94),),
    },
)

_SEAM_SPECS = (
    {
        "id": "process-player-action-entity-success",
        "sourcePath": _SOURCE_PATHS[2],
        "entrySymbol": "ProcessPlayerAction",
        "sourceLines": tuple(range(83, 89)),
        "sourceOperationCount": 6,
        "h1Ranges": ((0x25BAA, 0x25BC0),),
    },
    {
        "id": "field-item-invocation",
        "sourcePath": _SOURCE_PATHS[6],
        "entrySymbol": "FieldMenu",
        "sourceLines": tuple(range(250, 256)),
        "sourceOperationCount": 6,
        "h1Ranges": ((0x2151E, 0x21536),),
    },
    {
        "id": "s07-map-setup-jump-interfaces",
        "sourcePath": _SOURCE_PATHS[4],
        "entrySymbol": "j_RunMapSetupEntityEvent",
        "sourceLines": (320, 330, 340, 350),
        "sourceOperationCount": 4,
        "h1Ranges": ((0x4407C, 0x4408C),),
    },
    {
        "id": "map9-rhode-facing-caller",
        "sourcePath": _SOURCE_PATHS[10],
        "entrySymbol": "Map9_EntityEvent0",
        "sourceLines": (193, 194),
        "sourceOperationCount": 2,
        "h1Ranges": ((0x56830, 0x5683A),),
    },
    {
        "id": "map6-relative-position-predicate-pairs",
        "sourcePath": _SOURCE_PATHS[9],
        "entrySymbol": "Map6_EntityEvent0",
        "sourceLines": (36, 37, 49, 50, 193, 194, 236, 237, 250, 251),
        "sourceOperationCount": 10,
        "h1Ranges": (
            (0x549D6, 0x549DE),
            (0x549F2, 0x549FA),
            (0x54ABA, 0x54AC2),
            (0x54AFA, 0x54B02),
            (0x54B1A, 0x54B22),
        ),
    },
    {
        "id": "map9-relative-position-transform",
        "sourcePath": _SOURCE_PATHS[10],
        "entrySymbol": "Map9_EntityEvent8",
        "sourceLines": tuple(range(105, 109)),
        "sourceOperationCount": 4,
        "h1Ranges": ((0x567AC, 0x567BC),),
    },
    {
        "id": "map28-facing-wait-predicate",
        "sourcePath": _SOURCE_PATHS[11],
        "entrySymbol": "Map28_DefaultZoneEvent",
        "sourceLines": tuple(range(16, 19)),
        "sourceOperationCount": 3,
        "h1Ranges": ((0x5F37A, 0x5F386),),
    },
)


def _assert_instruction(
    h1_rows: dict[int, tuple[bytes, str]], *, address: int, expected: str, context: str
) -> tuple[bytes, str]:
    row = h1_rows.get(address)
    if row is None or row[1] != _normalise(expected):
        raise ValueError(f"map-event interaction-state instruction drift: {context}")
    return row


def _source_line_assert(lines: list[str], line_number: int, expected: str, context: str) -> None:
    actual = _source_statement(lines[line_number - 1]) if 1 <= line_number <= len(lines) else None
    if actual != _normalise(expected):
        raise ValueError(f"map-event interaction-state source use-site drift: {context}")


def _retained_fixture_descriptor(fixture_path: Path, fixture: dict[str, Any]) -> dict[str, str]:
    """Return the raw accepted fixture identity after its fresh guard has passed."""
    return {
        "fixtureId": fixture["id"],
        "fixtureSha256": hashlib.sha256(fixture_path.read_bytes()).hexdigest().upper(),
    }


def _fresh_map_setup_retained_owner(rom_path: Path, upstream_path: Path) -> dict[str, str]:
    """Validate Map Setup's public projection against its canonical contract."""
    from sf2tool.h2.map_setup import (
        FIXTURE as map_setup_fixture_path,
    )
    from sf2tool.h2.map_setup import (
        FIXTURE_SCHEMA as map_setup_fixture_schema,
    )
    from sf2tool.h2.map_setup import (
        MANIFEST as map_setup_manifest_path,
    )
    from sf2tool.h2.map_setup import (
        SCHEMA as map_setup_schema,
    )
    from sf2tool.h2.map_setup import (
        _canonical_bytes as map_setup_canonical_bytes,
    )
    from sf2tool.h2.map_setup import (
        build_map_setup_contract,
    )

    fixture = load_json(map_setup_fixture_path)
    validate_json(fixture, map_setup_fixture_schema, owner=str(map_setup_fixture_path))
    manifest = load_json(map_setup_manifest_path)
    output = build_map_setup_contract(rom_path, upstream_path)
    validate_json(output, map_setup_schema, owner="map setup static contract")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != output["romSha256"]
    ):
        raise ValueError("map-event interaction-state retained owner drift: mapSetup provenance")
    if output["function"] != fixture["function"] or output["table"] != fixture["table"]:
        raise ValueError("map-event interaction-state retained owner drift: mapSetup addresses")
    for field in (
        "summary",
        "sourceFacts",
        "aliasFlagRoutes",
        "selectionCases",
        "runtimeQuestions",
    ):
        if output[field] != fixture["expected"][field]:
            raise ValueError(f"map-event interaction-state retained owner drift: mapSetup {field}")
    digest = hashlib.sha256(map_setup_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"] or output["summary"] != manifest["summary"]:
        raise ValueError(
            "map-event interaction-state retained owner drift: mapSetup canonical output"
        )
    return _retained_fixture_descriptor(map_setup_fixture_path, fixture)


def _fresh_gameflow_core_retained_owner(upstream_path: Path) -> dict[str, str]:
    """Validate Gameflow Core's accepted projection without writing a derivative."""
    from sf2tool.h2.gameflow import (
        FIXTURE as gameflow_fixture_path,
    )
    from sf2tool.h2.gameflow import (
        FIXTURE_SCHEMA as gameflow_fixture_schema,
    )
    from sf2tool.h2.gameflow import (
        MANIFEST as gameflow_manifest_path,
    )
    from sf2tool.h2.gameflow import (
        ROM_MANIFEST as gameflow_rom_manifest_path,
    )
    from sf2tool.h2.gameflow import (
        SCHEMA as gameflow_schema,
    )
    from sf2tool.h2.gameflow import (
        _canonical_bytes as gameflow_canonical_bytes,
    )
    from sf2tool.h2.gameflow import (
        build_gameflow_inventory,
    )

    fixture = load_json(gameflow_fixture_path)
    validate_json(fixture, gameflow_fixture_schema, owner=str(gameflow_fixture_path))
    manifest = load_json(gameflow_manifest_path)
    output = build_gameflow_inventory(upstream_path)
    validate_json(output, gameflow_schema, owner="gameflow core static inventory")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != load_json(gameflow_rom_manifest_path)["hashes"]["sha256"]
    ):
        raise ValueError(
            "map-event interaction-state retained owner drift: gameflowCore provenance"
        )
    if output["summary"] != manifest["summary"]:
        raise ValueError("map-event interaction-state retained owner drift: gameflowCore summary")
    if output["representativeAddresses"] != fixture["function"]:
        raise ValueError("map-event interaction-state retained owner drift: gameflowCore addresses")
    for field in (
        "indexedRecordIds",
        "indexedSourcePaths",
        "startupFacts",
        "explorationFacts",
        "runtimeQuestions",
    ):
        if output[field] != fixture["expected"][field]:
            raise ValueError(
                f"map-event interaction-state retained owner drift: gameflowCore {field}"
            )
    digest = hashlib.sha256(gameflow_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"]:
        raise ValueError(
            "map-event interaction-state retained owner drift: gameflowCore canonical output"
        )
    return _retained_fixture_descriptor(gameflow_fixture_path, fixture)


def _fresh_common_menus_retained_owner(upstream_path: Path) -> dict[str, str]:
    """Validate Common Menus' accepted projection without writing a derivative."""
    from sf2tool.h2.menus import (
        FIXTURE as menus_fixture_path,
    )
    from sf2tool.h2.menus import (
        FIXTURE_SCHEMA as menus_fixture_schema,
    )
    from sf2tool.h2.menus import (
        MANIFEST as menus_manifest_path,
    )
    from sf2tool.h2.menus import (
        ROM_MANIFEST as menus_rom_manifest_path,
    )
    from sf2tool.h2.menus import (
        SCHEMA as menus_schema,
    )
    from sf2tool.h2.menus import (
        _canonical_bytes as menus_canonical_bytes,
    )
    from sf2tool.h2.menus import (
        _verify_menu_fixture_owner,
        build_menu_inventory,
    )

    fixture = load_json(menus_fixture_path)
    validate_json(fixture, menus_fixture_schema, owner=str(menus_fixture_path))
    manifest = load_json(menus_manifest_path)
    output = build_menu_inventory(upstream_path)
    validate_json(output, menus_schema, owner="common menus static inventory")
    _verify_menu_fixture_owner(
        fixture,
        output,
        rom_manifest=load_json(menus_rom_manifest_path),
    )
    if output["summary"] != manifest["summary"]:
        raise ValueError("map-event interaction-state retained owner drift: commonMenus summary")
    digest = hashlib.sha256(menus_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"]:
        raise ValueError(
            "map-event interaction-state retained owner drift: commonMenus canonical output"
        )
    return _retained_fixture_descriptor(menus_fixture_path, fixture)


def _fresh_battle_functions_retained_owner(upstream_path: Path) -> dict[str, str]:
    """Validate Battle Functions' accepted projection without writing a derivative."""
    from sf2tool.h2.battle_functions import (
        FIXTURE as battle_functions_fixture_path,
    )
    from sf2tool.h2.battle_functions import (
        FIXTURE_SCHEMA as battle_functions_fixture_schema,
    )
    from sf2tool.h2.battle_functions import (
        MANIFEST as battle_functions_manifest_path,
    )
    from sf2tool.h2.battle_functions import (
        SCHEMA as battle_functions_schema,
    )
    from sf2tool.h2.battle_functions import (
        SOURCE_ROOT as battle_functions_source_root,
    )
    from sf2tool.h2.battle_functions import (
        _canonical_bytes as battle_functions_canonical_bytes,
    )
    from sf2tool.h2.battle_functions import (
        _index_records_for_source_root,
        _verify_fixture_provenance,
        _verify_indexed_record_join,
        build_battle_functions_inventory,
    )
    from sf2tool.h2.battle_functions import (
        _resolve_upstream as resolve_battle_functions_upstream,
    )

    fixture = load_json(battle_functions_fixture_path)
    validate_json(
        fixture, battle_functions_fixture_schema, owner=str(battle_functions_fixture_path)
    )
    manifest = load_json(battle_functions_manifest_path)
    output = build_battle_functions_inventory(upstream_path)
    validate_json(output, battle_functions_schema, owner="battle-functions static inventory")
    disasm, _, _ = resolve_battle_functions_upstream(upstream_path)
    discovered_source_paths = sorted(
        path.relative_to(disasm).as_posix()
        for path in (disasm / battle_functions_source_root).glob("*.asm")
    )
    _verify_indexed_record_join(
        output,
        _index_records_for_source_root(set(discovered_source_paths)),
        discovered_source_paths,
    )
    _verify_fixture_provenance(fixture, output)
    if fixture["function"] != output["function"]:
        raise ValueError(
            "map-event interaction-state retained owner drift: battleFunctions addresses"
        )
    for field in (
        "indexedRecordIds",
        "indexedSourcePaths",
        "indexedRecordsBySourcePath",
    ):
        if output[field] != fixture["expected"][field]:
            raise ValueError(
                f"map-event interaction-state retained owner drift: battleFunctions {field}"
            )
    if output["summary"] != manifest["summary"]:
        raise ValueError(
            "map-event interaction-state retained owner drift: battleFunctions summary"
        )
    by_name = {Path(row["path"]).name: row for row in output["files"]}
    for filename, symbol in fixture["expected"]["representativeSymbols"].items():
        if symbol not in by_name[filename]["globalLabels"]:
            raise ValueError(
                "map-event interaction-state retained owner drift: battleFunctions "
                "representative symbol"
            )
    if output["functionFacts"] != fixture["expected"]["functionFacts"]:
        raise ValueError("map-event interaction-state retained owner drift: battleFunctions model")
    if output["playerControl"]["summary"] != fixture["expected"]["playerControlSummary"]:
        raise ValueError(
            "map-event interaction-state retained owner drift: battleFunctions "
            "player-control summary"
        )
    if output["playerControl"]["behaviorFacts"] != fixture["expected"]["playerControlFacts"]:
        raise ValueError(
            "map-event interaction-state retained owner drift: battleFunctions "
            "player-control behavior"
        )
    for fixture_field, output_field in (
        ("playerControlInputBits", "inputBits"),
        ("playerControlBattleActions", "battleActionConstants"),
        ("playerControlMenus", "menuConstants"),
        ("playerControlSelectedCallEdges", "selectedCallEdges"),
    ):
        if fixture["expected"][fixture_field] != output["playerControl"][output_field]:
            raise ValueError(
                f"map-event interaction-state retained owner drift: battleFunctions {output_field}"
            )
    digest = hashlib.sha256(battle_functions_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"]:
        raise ValueError(
            "map-event interaction-state retained owner drift: battleFunctions canonical output"
        )
    return _retained_fixture_descriptor(battle_functions_fixture_path, fixture)


def _fresh_tech_interfaces_retained_owner(upstream_path: Path) -> dict[str, str]:
    """Validate Tech Interfaces' accepted projection without writing a derivative."""
    from sf2tool.h2.interfaces import (
        FIXTURE as interfaces_fixture_path,
    )
    from sf2tool.h2.interfaces import (
        FIXTURE_SCHEMA as interfaces_fixture_schema,
    )
    from sf2tool.h2.interfaces import (
        MANIFEST as interfaces_manifest_path,
    )
    from sf2tool.h2.interfaces import (
        SCHEMA as interfaces_schema,
    )
    from sf2tool.h2.interfaces import (
        _canonical_bytes as interfaces_canonical_bytes,
    )
    from sf2tool.h2.interfaces import (
        _verify_fixture_provenance,
        _verify_indexed_record_join,
        build_interface_inventory,
    )

    fixture = load_json(interfaces_fixture_path)
    validate_json(fixture, interfaces_fixture_schema, owner=str(interfaces_fixture_path))
    manifest = load_json(interfaces_manifest_path)
    output = build_interface_inventory(upstream_path)
    validate_json(output, interfaces_schema, owner="tech interfaces static inventory")
    _verify_indexed_record_join(output)
    _verify_fixture_provenance(fixture, output)
    for field in (
        "indexedRecordIds",
        "indexedSourcePaths",
        "indexedRecordsBySourcePath",
        "interfaceFacts",
    ):
        if output[field] != fixture["expected"][field]:
            raise ValueError(
                f"map-event interaction-state retained owner drift: techInterfaces {field}"
            )
    if output["representativeAddresses"] != fixture["function"]:
        raise ValueError(
            "map-event interaction-state retained owner drift: techInterfaces addresses"
        )
    if output["summary"] != manifest["summary"]:
        raise ValueError("map-event interaction-state retained owner drift: techInterfaces summary")
    digest = hashlib.sha256(interfaces_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"]:
        raise ValueError(
            "map-event interaction-state retained owner drift: techInterfaces canonical output"
        )
    return _retained_fixture_descriptor(interfaces_fixture_path, fixture)


def _fresh_retained_owners(rom_path: Path, upstream_path: Path) -> dict[str, dict[str, str]]:
    """Fresh-build the accepted owners this narrow relation depends upon."""
    from sf2tool.h2.field_menu_control import build_field_menu_control_static
    from sf2tool.h2.map_event_dialogue_state import build_map_event_dialogue_state_contract
    from sf2tool.h2.map_event_direct_control import build_map_event_direct_control_contract
    from sf2tool.h2.map_event_direct_handoff import build_map_event_direct_handoff_contract
    from sf2tool.h2.map_event_direct_state import build_map_event_direct_state_contract
    from sf2tool.h2.map_event_predicate_results import build_map_event_predicate_results_contract
    from sf2tool.h2.map_events import build_map_events_contract
    from sf2tool.h2.map_events_fixture import load_map_events_fixture

    map_events_path = repo_path("tests/fixtures/h2/map-events-static-v1.json")
    map_events_fixture = load_map_events_fixture()
    if build_map_events_contract(rom_path, upstream_path) != map_events_fixture["expected"]:
        raise ValueError("map-event interaction-state retained owner drift: mapEvents")
    result: dict[str, dict[str, str]] = {
        "mapEvents": {
            "fixtureId": map_events_fixture["id"],
            "fixtureSha256": hashlib.sha256(map_events_path.read_bytes()).hexdigest().upper(),
        }
    }
    owners: tuple[tuple[str, Path, Any], ...] = (
        (
            "mapEventDirectState",
            repo_path("tests/fixtures/h2/map-event-direct-state-static-v1.json"),
            lambda: build_map_event_direct_state_contract(rom_path, upstream_path),
        ),
        (
            "mapEventDirectControl",
            repo_path("tests/fixtures/h2/map-event-direct-control-static-v1.json"),
            lambda: build_map_event_direct_control_contract(rom_path, upstream_path),
        ),
        (
            "mapEventDirectHandoff",
            repo_path("tests/fixtures/h2/map-event-direct-handoff-static-v1.json"),
            lambda: build_map_event_direct_handoff_contract(rom_path, upstream_path),
        ),
        (
            "mapEventPredicateResults",
            repo_path("tests/fixtures/h2/map-event-predicate-results-static-v1.json"),
            lambda: build_map_event_predicate_results_contract(rom_path, upstream_path),
        ),
        (
            "mapEventDialogueState",
            repo_path("tests/fixtures/h2/map-event-dialogue-state-static-v1.json"),
            lambda: build_map_event_dialogue_state_contract(rom_path, upstream_path),
        ),
        (
            "fieldMenuControl",
            repo_path("tests/fixtures/h2/field-menu-control-static-v1.json"),
            lambda: build_field_menu_control_static(rom_path, upstream_path),
        ),
    )
    for name, fixture_path, builder in owners:
        fixture = load_json(fixture_path)
        if builder() != fixture:
            raise ValueError(f"map-event interaction-state retained owner drift: {name}")
        result[name] = _retained_fixture_descriptor(fixture_path, fixture)
    result["mapSetup"] = _fresh_map_setup_retained_owner(rom_path, upstream_path)
    result["gameflowCore"] = _fresh_gameflow_core_retained_owner(upstream_path)
    result["commonMenus"] = _fresh_common_menus_retained_owner(upstream_path)
    result["battleFunctions"] = _fresh_battle_functions_retained_owner(upstream_path)
    result["techInterfaces"] = _fresh_tech_interfaces_retained_owner(upstream_path)
    direct_state = load_json(repo_path("tests/fixtures/h2/map-event-direct-state-static-v1.json"))
    if direct_state["summary"] != {
        "sourceIdentityCount": 40,
        "programContextCount": 914,
        "positiveDirectProgramContextCount": 65,
        "zeroDirectProgramContextCount": 849,
        "contextInstructionSiteCount": 127,
        "physicalInstructionSiteCount": 124,
        "contextAccessSiteCount": 152,
        "physicalAccessSiteCount": 148,
        "symbolDefinitionCount": 13,
        "sourceFileCount": 38,
    }:
        raise ValueError("map-event interaction-state retained direct-state summary drift")
    return result


def _interaction_projection(
    rom_path: Path, upstream_path: Path
) -> tuple[dict[str, Any], dict[str, Any]]:
    disasm = _disasm_root(upstream_path)
    listing_path = upstream_path.resolve(strict=True) / "build/sf2build-h1.lst"
    listing_text = listing_path.read_text(encoding="utf-8")
    h1_rows, h1_labels = _parse_h1(listing_text)
    rom = rom_path.resolve(strict=True).read_bytes()
    if hashlib.sha256(rom).hexdigest().upper() != _ROM_SHA256:
        raise ValueError("map-event interaction-state ROM identity drift")

    source_text: dict[str, list[str]] = {}
    source_identities: list[dict[str, str]] = []
    for source_path in _SOURCE_PATHS:
        payload = (disasm / source_path).read_bytes()
        source_text[source_path] = payload.decode("utf-8").splitlines()
        source_identities.append(
            {"path": source_path, "sha256": hashlib.sha256(payload).hexdigest().upper()}
        )
    if tuple(row["path"] for row in source_identities) != _SOURCE_PATHS:
        raise ValueError("map-event interaction-state source identity order drift")
    constants = _parse_equates(source_text["sf2const.asm"])
    expected_constants = {"ENTITY_FACING": 0xFFA912, "EVENT_RELATIVE_POSITION": 0xFFB651}
    if {
        symbol: constants.get(symbol, {}).get("value") for symbol in _SYMBOLS
    } != expected_constants:
        raise ValueError("map-event interaction-state symbol definition drift")

    function_ranges = [
        _segment(
            spec=spec,
            source_text=source_text,
            h1_rows=h1_rows,
            h1_labels=h1_labels,
            rom=rom,
        )
        for spec in _FUNCTION_SPECS
    ]
    seams = [
        _segment(
            spec=spec,
            source_text=source_text,
            h1_rows=h1_rows,
            h1_labels=h1_labels,
            rom=rom,
        )
        for spec in _SEAM_SPECS
    ]
    if [row["sourceOperationCount"] for row in function_ranges] != [57, 15, 9, 39, 59, 10, 6]:
        raise ValueError("map-event interaction-state function operation denominator drift")
    if [row["h1RomByteLength"] for row in function_ranges] != [170, 44, 36, 148, 194, 36, 24]:
        raise ValueError("map-event interaction-state function byte denominator drift")
    if [row["sourceOperationCount"] for row in seams] != [6, 6, 4, 2, 10, 4, 3]:
        raise ValueError("map-event interaction-state seam operation denominator drift")
    if [row["h1RomByteLength"] for row in seams] != [22, 24, 16, 10, 40, 16, 12]:
        raise ValueError("map-event interaction-state seam byte denominator drift")

    symbol_definitions = [
        {
            "symbol": symbol,
            "address": constants[symbol]["value"],
            "width": "b",
            "sourcePath": "sf2const.asm",
            "sourceLine": constants[symbol]["sourceLine"],
        }
        for symbol in _SYMBOLS
    ]
    writer_specs = (
        (
            "item-event-d2",
            0x47592,
            "move.b d2,((EVENT_RELATIVE_POSITION-$1000000)).w",
            "item-invocation",
        ),
        (
            "entity-event-d2",
            0x4761E,
            "move.b d2,((EVENT_RELATIVE_POSITION-$1000000)).w",
            "entity-dispatch",
        ),
    )
    producer_write_rows: list[dict[str, Any]] = []
    for order, (identifier, address, statement, context_id) in enumerate(writer_specs):
        encoded, _statement = _assert_instruction(
            h1_rows, address=address, expected=statement, context=identifier
        )
        if len(encoded) != 4:
            raise ValueError("map-event interaction-state writer width drift")
        producer_write_rows.append(
            {
                "writeOrder": order,
                "id": identifier,
                "contextId": context_id,
                "symbol": "EVENT_RELATIVE_POSITION",
                "address": constants["EVENT_RELATIVE_POSITION"]["value"],
                "romPc": address,
                "width": "b",
                "sourceRegister": "d2",
                "entryAddress": 0x47586 if identifier == "item-event-d2" else 0x4761A,
            }
        )

    read_specs = (
        (
            "get-player-entity-position/entity-facing",
            0x22C6C,
            "move.b (ENTITY_FACING).l,d3",
            "ENTITY_FACING",
            "d3",
            "player-position",
        ),
        (
            "get-rhode-facing/relative-position",
            0x4783C,
            "move.b ((EVENT_RELATIVE_POSITION-$1000000)).w,d1",
            "EVENT_RELATIVE_POSITION",
            "d1",
            "rhode-facing",
        ),
        (
            "map6/relative-position-0",
            0x549D6,
            "cmpi.b #1,((EVENT_RELATIVE_POSITION-$1000000)).w",
            "EVENT_RELATIVE_POSITION",
            None,
            "map6-predicate",
        ),
        (
            "map6/relative-position-1",
            0x549F2,
            "cmpi.b #1,((EVENT_RELATIVE_POSITION-$1000000)).w",
            "EVENT_RELATIVE_POSITION",
            None,
            "map6-predicate",
        ),
        (
            "map6/relative-position-2",
            0x54ABA,
            "cmpi.b #1,((EVENT_RELATIVE_POSITION-$1000000)).w",
            "EVENT_RELATIVE_POSITION",
            None,
            "map6-predicate",
        ),
        (
            "map6/relative-position-3",
            0x54AFA,
            "cmpi.b #1,((EVENT_RELATIVE_POSITION-$1000000)).w",
            "EVENT_RELATIVE_POSITION",
            None,
            "map6-predicate",
        ),
        (
            "map6/relative-position-4",
            0x54B1A,
            "cmpi.b #1,((EVENT_RELATIVE_POSITION-$1000000)).w",
            "EVENT_RELATIVE_POSITION",
            None,
            "map6-predicate",
        ),
        (
            "map9/relative-position-transform",
            0x567AC,
            "move.b ((EVENT_RELATIVE_POSITION-$1000000)).w,d1",
            "EVENT_RELATIVE_POSITION",
            "d1",
            "map9-transform",
        ),
    )
    consumer_read_rows: list[dict[str, Any]] = []
    for order, (identifier, address, statement, symbol, target_register, role) in enumerate(
        read_specs
    ):
        _assert_instruction(h1_rows, address=address, expected=statement, context=identifier)
        row: dict[str, Any] = {
            "readOrder": order,
            "id": identifier,
            "symbol": symbol,
            "address": constants[symbol]["value"],
            "romPc": address,
            "width": "b",
            "role": role,
        }
        if target_register is not None:
            row["targetRegister"] = target_register
        consumer_read_rows.append(row)

    predicate_specs = (
        (
            "map6-relative-position-0",
            "map6/relative-position-0",
            0x549DC,
            "bne.s byte_549EC",
            0x549EC,
            0x549DE,
        ),
        (
            "map6-relative-position-1",
            "map6/relative-position-1",
            0x549F8,
            "bne.s byte_54A08",
            0x54A08,
            0x549FA,
        ),
        (
            "map6-relative-position-2",
            "map6/relative-position-2",
            0x54AC0,
            "bne.s byte_54ACA",
            0x54ACA,
            0x54AC2,
        ),
        (
            "map6-relative-position-3",
            "map6/relative-position-3",
            0x54B00,
            "bne.s byte_54B10",
            0x54B10,
            0x54B02,
        ),
        (
            "map6-relative-position-4",
            "map6/relative-position-4",
            0x54B20,
            "bne.s byte_54B30",
            0x54B30,
            0x54B22,
        ),
        ("map28-entity-facing", None, 0x5F384, "bne.s byte_5F38E", 0x5F38E, 0x5F386),
    )
    predicate_joins: list[dict[str, Any]] = []
    for order, (identifier, read_id, address, statement, target, fallthrough) in enumerate(
        predicate_specs
    ):
        encoded, actual = _assert_instruction(
            h1_rows, address=address, expected=statement, context=identifier
        )
        target_name = actual.split(maxsplit=1)[1]
        if (
            _label_target(h1_labels, target_name, address) != target
            or address + len(encoded) != fallthrough
        ):
            raise ValueError(
                f"map-event interaction-state predicate target/fallthrough drift: {identifier}"
            )
        row: dict[str, Any] = {
            "predicateOrder": order,
            "id": identifier,
            "branchMnemonic": "bne",
            "branchAddress": address,
            "targetAddress": target,
            "fallthroughAddress": fallthrough,
        }
        if read_id is not None:
            row["producerReadId"] = read_id
            row["immediateValue"] = 1
        else:
            row["producerSymbol"] = "ENTITY_FACING"
            row["producerAddress"] = constants["ENTITY_FACING"]["value"]
            _assert_instruction(
                h1_rows,
                address=0x5F380,
                expected="tst.b ((ENTITY_FACING-$1000000)).w",
                context=identifier,
            )
        predicate_joins.append(row)

    jump_specs = (
        (
            "run-map-setup-entity-event",
            0x4407C,
            "j_RunMapSetupEntityEvent",
            "RunMapSetupEntityEvent",
            0x4761A,
        ),
        (
            "run-map-setup-zone-event",
            0x44080,
            "j_RunMapSetupZoneEvent",
            "RunMapSetupZoneEvent",
            0x4751A,
        ),
        (
            "run-map-setup-entity-event-0",
            0x44084,
            "j_RunMapSetupEntityEvent_0",
            "RunMapSetupEntityEvent",
            0x4761A,
        ),
        (
            "run-map-setup-item-event",
            0x44088,
            "j_RunMapSetupItemEvent",
            "RunMapSetupItemEvent",
            0x47586,
        ),
    )
    jump_interface_rows: list[dict[str, Any]] = []
    for order, (
        identifier,
        address,
        instruction_target,
        effective_target,
        effective_address,
    ) in enumerate(jump_specs):
        _assert_instruction(
            h1_rows, address=address, expected=f"jmp {effective_target}(pc)", context=identifier
        )
        if _label_target(h1_labels, instruction_target, address) != address:
            raise ValueError(f"map-event interaction-state alias entry drift: {identifier}")
        jump_interface_rows.append(
            {
                "interfaceOrder": order,
                "id": identifier,
                "entryAddress": address,
                "instructionTarget": instruction_target,
                "effectiveTarget": effective_target,
                "effectiveTargetAddress": effective_address,
            }
        )

    producer_writes = {
        "itemEventD2": producer_write_rows[0],
        "entityEventD2": producer_write_rows[1],
    }
    consumer_reads = {
        "getPlayerEntityPosition": {
            "entryAddress": 0x22C60,
            "reads": [consumer_read_rows[0]],
        },
        "getRhodeFacing": {
            "entryAddress": 0x47832,
            "reads": [consumer_read_rows[1]],
        },
        "map6": {
            "tableEntryAddress": 0x54984,
            "reads": consumer_read_rows[2:7],
        },
        "map9": {
            "tableEntryAddress": 0x56722,
            "reads": [consumer_read_rows[7]],
        },
        "map28": {"tableEntryAddress": 0x5F36C, "reads": []},
    }
    jump_interfaces = {
        "runMapSetupEntityEvent": jump_interface_rows[0],
        "runMapSetupZoneEvent": jump_interface_rows[1],
        "runMapSetupEntityEvent0": jump_interface_rows[2],
        "runMapSetupItemEvent": jump_interface_rows[3],
    }

    _source_line_assert(
        source_text[_SOURCE_PATHS[1]],
        27,
        "move.b ENTITYDEF_OFFSET_FACING(a0,d0.w),d3",
        "entity-facing-input",
    )
    _source_line_assert(source_text[_SOURCE_PATHS[1]], 85, "move.w d3,d2", "entity-facing-result")
    _source_line_assert(
        source_text[_SOURCE_PATHS[5]], 13, "move.w (ENTITY_Y).l,d2", "item-player-y-input"
    )
    _source_line_assert(
        source_text[_SOURCE_PATHS[2]], 83, "bsr.w GetActivatedEntity", "entity-dispatch-order"
    )
    _source_line_assert(
        source_text[_SOURCE_PATHS[2]], 86, "bsr.w GetEntityEventIndex", "entity-dispatch-order"
    )
    _source_line_assert(
        source_text[_SOURCE_PATHS[2]], 87, "jsr j_RunMapSetupEntityEvent", "entity-dispatch-order"
    )
    _source_line_assert(
        source_text[_SOURCE_PATHS[6]], 250, "bsr.w GetPlayerEntityPosition", "item-invocation-order"
    )
    _source_line_assert(
        source_text[_SOURCE_PATHS[6]], 253, "jsr j_RunMapSetupItemEvent", "item-invocation-order"
    )
    _source_line_assert(
        source_text[_SOURCE_PATHS[8]], 396, "jsr j_RunMapSetupZoneEvent", "zone-dispatch"
    )

    dispatch_contexts = {
        "entityAcquisition": {
            "entryAddress": 0x2379A,
            "rangeId": "get-activated-entity",
            "resultRegister": "d2",
        },
        "entityDispatch": {
            "processPlayerActionAddress": 0x25BAA,
            "getEntityEventIndexAddress": 0x22F4A,
            "runEntityEventCallAddress": 0x25BB6,
        },
        "itemInvocation": {
            "getPlayerEntityPositionAddress": 0x22C60,
            "runMapSetupItemEventCallAddress": 0x2152A,
        },
        "zoneFacing": {
            "processMapEventType6Address": 0x25A7C,
            "runMapSetupZoneEventAddress": 0x4751A,
        },
    }
    multiplexed_roles = [
        {
            "id": "event-relative-position/entity-event-d2",
            "symbol": "EVENT_RELATIVE_POSITION",
            "producerWriteId": "entity-event-d2",
            "dispatchContextId": "entityDispatch",
            "inputRegister": "d2",
            "inputRole": "player-facing",
        },
        {
            "id": "event-relative-position/item-event-d2",
            "symbol": "EVENT_RELATIVE_POSITION",
            "producerWriteId": "item-event-d2",
            "dispatchContextId": "itemInvocation",
            "inputRegister": "d2",
            "inputRole": "player-y-tile",
        },
        {
            "id": "entity-facing/map28-consumer-only",
            "symbol": "ENTITY_FACING",
            "consumerReadId": "map28-entity-facing",
            "boundedWriterCount": 0,
        },
    ]
    anchor_order = [row["id"] for row in [*function_ranges, *seams]]
    interaction_state = {
        "symbolDefinitions": symbol_definitions,
        "functionRanges": function_ranges,
        "dispatchContexts": dispatch_contexts,
        "jumpInterfaces": jump_interfaces,
        "producerWrites": producer_writes,
        "consumerReads": consumer_reads,
        "predicateJoins": predicate_joins,
        "multiplexedRoles": multiplexed_roles,
        "anchorOrder": anchor_order,
        "digests": {
            "functionRangesSha256": hashlib.sha256(
                canonical_json_bytes({"functionRanges": function_ranges})
            )
            .hexdigest()
            .upper(),
            "seamRangesSha256": hashlib.sha256(canonical_json_bytes({"seamRanges": seams}))
            .hexdigest()
            .upper(),
            "dispatchContextsSha256": hashlib.sha256(
                canonical_json_bytes({"dispatchContexts": dispatch_contexts})
            )
            .hexdigest()
            .upper(),
            "producerWritesSha256": hashlib.sha256(
                canonical_json_bytes({"producerWrites": producer_writes})
            )
            .hexdigest()
            .upper(),
            "consumerReadsSha256": hashlib.sha256(
                canonical_json_bytes({"consumerReads": consumer_reads})
            )
            .hexdigest()
            .upper(),
            "predicateJoinsSha256": hashlib.sha256(
                canonical_json_bytes({"predicateJoins": predicate_joins})
            )
            .hexdigest()
            .upper(),
        },
    }
    summary = {
        "sourceIdentityCount": len(source_identities),
        "functionRangeCount": len(function_ranges),
        "seamGroupCount": len(seams),
        "sourceOperationCount": sum(
            row["sourceOperationCount"] for row in [*function_ranges, *seams]
        ),
        "h1RomByteCount": sum(row["h1RomByteLength"] for row in [*function_ranges, *seams]),
        "symbolDefinitionCount": len(symbol_definitions),
        "producerWriteCount": len(producer_write_rows),
        "consumerReadCount": len(consumer_read_rows),
        "predicateJoinCount": len(predicate_joins),
        "jumpInterfaceCount": len(jump_interface_rows),
    }
    expected_summary = {
        "sourceIdentityCount": 12,
        "functionRangeCount": 7,
        "seamGroupCount": 7,
        "sourceOperationCount": 230,
        "h1RomByteCount": 792,
        "symbolDefinitionCount": 2,
        "producerWriteCount": 2,
        "consumerReadCount": 8,
        "predicateJoinCount": 6,
        "jumpInterfaceCount": 4,
    }
    if summary != expected_summary:
        raise ValueError(f"map-event interaction-state denominator drift: {summary}")
    return interaction_state, {
        "sourceContext": {
            "h1Listing": {
                "path": "build/sf2build-h1.lst",
                "sha256": hashlib.sha256(listing_text.encode("utf-8")).hexdigest().upper(),
            },
            "sourceIdentities": source_identities,
            "seamRanges": seams,
        },
        "summary": summary,
    }


def build_map_event_interaction_state_contract(
    rom_path: Path, upstream_path: Path
) -> dict[str, Any]:
    """Build the public source/H1/ROM interaction-state contract."""
    retained_owners = _fresh_retained_owners(rom_path, upstream_path)
    interaction_state, shared = _interaction_projection(rom_path, upstream_path)
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": "ShiningForceCentral/SF2DISASM", "commit": _UPSTREAM_COMMIT},
        "romSha256": _ROM_SHA256,
        "sourceContext": shared["sourceContext"],
        "retainedOwners": retained_owners,
        "interactionState": interaction_state,
        "unknowns": {key: "Unknown" for key in _UNKNOWN_KEYS},
        "summary": shared["summary"],
    }


def verify_map_event_interaction_state_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    """Rebuild, validate, and compare the public static fixture."""
    output = build_map_event_interaction_state_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="map-event interaction-state rebuilt contract")
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner="map-event interaction-state fixture")
    if output != fixture:
        raise ValueError("map-event interaction-state complete semantic fixture drift")
    if output_path is not None:
        output_path.parent.mkdir(parents=True, exist_ok=True)
        output_path.write_bytes(canonical_json_bytes(output))
    return {
        "Contract": ID,
        "Output": display_path(output_path or FIXTURE),
        "SHA256": hashlib.sha256(canonical_json_bytes(output)).hexdigest().upper(),
        "Operations": output["summary"]["sourceOperationCount"],
        "Bytes": output["summary"]["h1RomByteCount"],
        "Status": "PASS",
    }
