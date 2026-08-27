"""Static H2 contract for map-event ``CheckRandomBattle`` caller and state shape."""

from __future__ import annotations

import hashlib
import json
import re
from copy import deepcopy
from pathlib import Path
from typing import Any

from sf2tool.h2.map_event_combatant_state import (
    canonical_json_bytes,
    normalize_map_event_combatant_state_later_owner_index,
)
from sf2tool.h2.map_event_direct_handoff import _h1_instruction_rows, _normalise_statement
from sf2tool.h2.map_events_fixture import load_map_events_fixture
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path

ID = "sf2-map-event-random-battle-state-static-v1"
FIXTURE = repo_path("tests/fixtures/h2/map-event-random-battle-state-static-v1.json")
SCHEMA = repo_path("schemas/h2/map-event-random-battle-state-static-fixture.schema.json")
_PREDECESSOR_INDEX_SHA256 = "9848602E14474EFD9C16FD8E846E14937D09B93F6447E806DEDAE9BE0A17E94A"
_ROM_SHA256 = "9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9"
_UPSTREAM_COMMIT = "c834c652b6862bc5679fd7f69a38a7093206efc6"
_FUNCTION_PATH = "code/common/scripting/map/mapsetupsfunctions_1.asm"
_SOURCE_PATHS = (
    "sf2const.asm",
    "sf2enums.asm",
    "sf2macros.asm",
    "layout/sf2-01-0x000000-0x008000.asm",
    "layout/sf2-02-0x008000-0x010000.asm",
    "layout/sf2-07-0x044000-0x064000.asm",
    _FUNCTION_PATH,
    "code/common/tech/jumpinterfaces/s02_jumpinterface.asm",
    "code/common/stats/gameflags.asm",
    "code/common/tech/randomnumbergenerator.asm",
    "code/common/maps/camerafunctions.asm",
    "code/common/tech/graphics/flashwhite.asm",
    "data/maps/entries/map66/mapsetups/s3_zoneevents.asm",
    "data/maps/entries/map67/mapsetups/s3_zoneevents.asm",
    "data/maps/entries/map68/mapsetups/s3_zoneevents.asm",
    "data/maps/entries/map69/mapsetups/s3_zoneevents.asm",
    "data/maps/entries/map70/mapsetups/s3_zoneevents.asm",
    "data/maps/entries/map72/mapsetups/s3_zoneevents.asm",
)
_CALLERS = (
    (
        "map66-default-battle3",
        "Map66_DefaultZoneEvent",
        "ms_map66_ZoneEvents",
        "BATTLE_TO_HAWEL_HOUSE",
        3,
        0x4FAD4,
        0x4FAD8,
        0x4FADE,
    ),
    (
        "map67-default-battle21",
        "Map67_DefaultZoneEvent",
        "ms_map67_ZoneEvents",
        "BATTLE_DEVIL_TAIL",
        21,
        0x4FB58,
        0x4FB5C,
        0x4FB62,
    ),
    (
        "map68-default-battle19",
        "Map68_DefaultZoneEvent",
        "ms_map68_ZoneEvents",
        "BATTLE_OUTSIDE_ELVEN_VILLAGE",
        19,
        0x4FD70,
        0x4FD74,
        0x4FD7A,
    ),
    (
        "map69-event0-battle17",
        "Map69_ZoneEvent0",
        "ms_map69_ZoneEvents",
        "BATTLE_TO_TAROS_SHRINE",
        17,
        0x4FDB2,
        0x4FDB6,
        0x4FDBC,
    ),
    (
        "map70-event0-battle14",
        "Map70_ZoneEvent0",
        "ms_map70_ZoneEvents",
        "BATTLE_SOUTHEAST_DESERT",
        14,
        0x4FE12,
        0x4FE16,
        0x4FE1C,
    ),
    (
        "map72-event0-battle26",
        "Map72_ZoneEvent0",
        "ms_map72_ZoneEvents",
        "BATTLE_OUTSIDE_KETTO",
        26,
        0x4FE8C,
        0x4FE90,
        0x4FE96,
    ),
    (
        "map72-default-north-cliff-battle8",
        "Map72_DefaultZoneEvent",
        "ms_map72_ZoneEvents",
        "BATTLE_NORTH_CLIFF",
        8,
        0x4FF0C,
        0x4FF10,
        0x4FF16,
    ),
    (
        "map72-default-north-parmecia-battle24",
        "Map72_DefaultZoneEvent",
        "ms_map72_ZoneEvents",
        "BATTLE_TO_NORTH_PARMECIA",
        24,
        0x4FF18,
        0x4FF1C,
        0x4FF22,
    ),
)
_SERVICES = (
    ("j_CheckFlag", 0x8264, "CheckFlag", 0x98B4, "code/common/stats/gameflags.asm"),
    ("j_SetFlag", 0x8268, "SetFlag", 0x98C4, "code/common/stats/gameflags.asm"),
    (
        "GenerateRandomNumber",
        0x1600,
        "GenerateRandomNumber",
        0x1600,
        "code/common/tech/randomnumbergenerator.asm",
    ),
    (
        "WaitForViewScrollEnd",
        0x4708,
        "WaitForViewScrollEnd",
        0x4708,
        "code/common/maps/camerafunctions.asm",
    ),
    (
        "ExecuteFlashScreenScript",
        0x47EEA,
        "ExecuteFlashScreenScript",
        0x47EEA,
        "code/common/tech/graphics/flashwhite.asm",
    ),
)
_UNKNOWN_KEYS = (
    "naturalCallerReachability",
    "actualSelectedCaller",
    "actualBattleId",
    "completedFlagValueResult",
    "entryStepCounter",
    "firstRngResult",
    "secondRngResult",
    "selectedReturnPathResultRegister",
    "unlockedFlagBeforeAfter",
    "mapEventTypeBeforeAfter",
    "stepCounterRuntimeResult",
    "waitSoundFlashRuntimeCompletionPresentation",
    "downstreamCheckBattleBattleLoopAdmission",
    "persistenceMapSaveLoad",
)
_EQUATE = re.compile(r"^\s*([A-Za-z_][A-Za-z0-9_]*)\s*:\s+equ\s+([^\s;]+)")


def _root(upstream_path: Path) -> Path:
    root = upstream_path.resolve(strict=True)
    return root / "disasm" if (root / "disasm").is_dir() else root


def _fixture_digest(path: str) -> str:
    return hashlib.sha256(repo_path(path).read_bytes()).hexdigest().upper()


def _parse_equates(lines: list[str], names: set[str]) -> dict[str, int]:
    found: dict[str, list[int]] = {name: [] for name in names}
    for line in lines:
        match = _EQUATE.match(line)
        if match is None or match.group(1) not in found:
            continue
        text = match.group(2)
        if text.startswith("$"):
            found[match.group(1)].append(int(text[1:], 16))
        elif text.isdecimal():
            found[match.group(1)].append(int(text))
    if any(len(values) != 1 for values in found.values()):
        raise ValueError("map-event random-battle equate occurrence drift")
    return {name: values[0] for name, values in found.items()}


def _source_line(lines: list[str], symbol: str) -> int:
    matches = [
        index
        for index, line in enumerate(lines, 1)
        if re.fullmatch(rf"\s*{re.escape(symbol)}:\s*(?:;.*)?", line)
    ]
    if len(matches) != 1:
        raise ValueError(f"map-event random-battle label drift: {symbol}")
    return matches[0]


def _anchor(
    address: int, statement: str, h1: dict[int, tuple[bytes, str]], rom: bytes
) -> dict[str, Any]:
    encoded, h1_statement = h1.get(address, (b"", ""))
    rom_encoded = rom[address : address + len(encoded)]
    if not encoded or len(rom_encoded) != len(encoded):
        raise ValueError(f"map-event random-battle H1/ROM anchor drift: {address:#x}")
    return {
        "address": address,
        "sourceStatement": statement,
        "h1ListedByteCount": len(encoded),
        "romEncodedByteCount": len(encoded),
        "h1InstructionSha256": hashlib.sha256(encoded).hexdigest().upper(),
        "h1StatementSha256": hashlib.sha256(h1_statement.encode()).hexdigest().upper(),
        "romInstructionSha256": hashlib.sha256(rom_encoded).hexdigest().upper(),
    }


def _operation_row(operation: dict[str, Any]) -> dict[str, Any]:
    target = operation["target"]
    return {
        "address": operation["address"],
        "sourceLine": operation["sourceLine"],
        "mnemonic": operation["mnemonic"],
        "sizeSuffix": operation["sizeSuffix"],
        "operandTexts": operation["operandTexts"],
        "family": operation["family"],
        "controlFlowKind": operation["controlFlowKind"],
        "instructionTarget": None if target is None else target["instructionTargetSymbol"],
        "effectiveTarget": None if target is None else target["effectiveTargetSymbol"],
    }


def _validate_order(value: dict[str, Any]) -> None:
    expected = [
        "schemaVersion",
        "id",
        "system",
        "upstream",
        "romSha256",
        "sourceContext",
        "retainedOwners",
        "eventRandomBattleState",
        "unknowns",
        "summary",
    ]
    if list(value) != expected:
        raise ValueError("map-event random-battle root order drift")
    facts = value["eventRandomBattleState"]
    expected_facts = [
        "functionAddresses",
        "battleIdentities",
        "callerSites",
        "completionFlagGate",
        "stepCounterGate",
        "randomCadence",
        "requestWriteSequence",
        "returnContract",
        "serviceEntries",
    ]
    if list(facts) != expected_facts:
        raise ValueError("map-event random-battle field order drift")


def build_map_event_random_battle_state_contract(
    rom_path: Path, upstream_path: Path, *, map_events_override: dict[str, Any] | None = None
) -> dict[str, Any]:
    rom = rom_path.resolve(strict=True).read_bytes()
    if hashlib.sha256(rom).hexdigest().upper() != _ROM_SHA256:
        raise ValueError("map-event random-battle ROM identity drift")
    root = _root(upstream_path)
    source = {
        path: (root / path).read_text(encoding="utf-8").splitlines() for path in _SOURCE_PATHS
    }
    listing = (upstream_path.resolve(strict=True) / "build/sf2build-h1.lst").read_text(
        encoding="utf-8"
    )
    h1 = _h1_instruction_rows(listing)
    map_events = deepcopy(
        load_map_events_fixture()["expected"]
        if map_events_override is None
        else map_events_override
    )
    all_programs = [
        *map_events["entityTargetPrograms"],
        *map_events["zoneTargetPrograms"],
        *map_events["itemTargetPrograms"],
    ]
    positives = [
        p
        for p in all_programs
        if any(
            op.get("target", {}).get("effectiveTargetSymbol") == "CheckRandomBattle"
            for op in p["operations"]
            if op.get("target")
        )
    ]
    if len(all_programs) != 914 or len(positives) != 7 or len(all_programs) - len(positives) != 907:
        raise ValueError("map-event random-battle mother corpus drift")
    programs = {p["canonicalSymbol"]: p for p in positives}
    zone_tables = {row["symbol"]: row for row in map_events["categories"]["zoneEvents"]["tables"]}
    enums = _parse_equates(source["sf2enums.asm"], {row[3] for row in _CALLERS})
    consts = _parse_equates(source["sf2const.asm"], {"STEP_COUNTER", "MAP_EVENT_TYPE"})
    caller_sites: dict[str, Any] = {}
    caller_anchors: list[dict[str, Any]] = []
    caller_rows: list[dict[str, Any]] = []
    for order, (
        caller_id,
        symbol,
        table_symbol,
        battle_symbol,
        battle_id,
        setup,
        call,
        continuation,
    ) in enumerate(_CALLERS):
        program = programs.get(symbol)
        table = zone_tables.get(table_symbol)
        if (
            program is None
            or table is None
            or table["sourcePath"] != program["sourcePath"]
            or enums[battle_symbol] != battle_id
        ):
            raise ValueError(f"map-event random-battle caller identity drift: {caller_id}")
        operations = {op["address"]: op for op in program["operations"]}
        selected = [operations.get(address) for address in (setup, call, continuation)]
        if any(op is None for op in selected):
            raise ValueError(f"map-event random-battle caller operation drift: {caller_id}")
        setup_op, call_op, continuation_op = selected
        if (
            setup_op["mnemonic"],
            setup_op["operandTexts"],
            call_op["mnemonic"],
            call_op["target"]["effectiveTargetSymbol"],
        ) != ("move", [f"#{battle_symbol}", "d0"], "jsr", "CheckRandomBattle"):
            raise ValueError(f"map-event random-battle setup/call drift: {caller_id}")
        for operation in selected:
            statement = operation["sourceMnemonic"] + (
                " " + ",".join(operation["operandTexts"]) if operation["operandTexts"] else ""
            )
            if _normalise_statement(
                source[program["sourcePath"]][operation["sourceLine"] - 1]
            ) != _normalise_statement(statement):
                raise ValueError(
                    "map-event random-battle caller source drift: "
                    f"{caller_id}:{operation['sourceLine']}"
                )
            caller_anchors.append(_anchor(operation["address"], statement, h1, rom))
            caller_rows.append(_operation_row(operation))
        caller_sites[caller_id] = {
            "callerOrder": order,
            "tableSymbol": table_symbol,
            "tableEntryAddress": table["address"],
            "programSymbol": symbol,
            "sourcePath": program["sourcePath"],
            "battleIdentity": battle_symbol,
            "battleId": battle_id,
            "setup": _operation_row(setup_op),
            "call": _operation_row(call_op),
            "continuation": _operation_row(continuation_op),
        }
    if len(caller_rows) != 24 or len(caller_anchors) != 24:
        raise ValueError("map-event random-battle caller row denominator drift")
    function_addresses = {
        address: statement
        for address, statement in (
            (0x47856, "movem.l d1/d6-d7,-(sp)"),
            (0x4785A, "move.w #BATTLE_COMPLETED_FLAGS_START,d1"),
            (0x4785E, "add.w d0,d1"),
            (0x47860, "jsr j_CheckFlag"),
            (0x47866, "bne.s loc_4786E"),
            (0x47868, "moveq #-1,d1"),
            (0x4786A, "bra.w loc_47896"),
            (0x4786E, "tst.w ((STEP_COUNTER-$1000000)).w"),
            (0x47872, "beq.s loc_4787A"),
            (0x47874, "clr.w d1"),
            (0x47876, "bra.w loc_47896"),
            (0x4787A, "moveq #8,d6"),
            (0x4787C, "jsr (GenerateRandomNumber).w"),
            (0x47880, "tst.w d7"),
            (0x47882, "bne.s loc_47888"),
            (0x47884, "moveq #-1,d1"),
            (0x47886, "bra.s loc_47896"),
            (0x47888, "clr.w d1"),
            (0x4788A, "moveq #4,d6"),
            (0x4788C, "jsr (GenerateRandomNumber).w"),
            (0x47890, "addq.l #2,d7"),
            (0x47892, "move.w d7,((STEP_COUNTER-$1000000)).w"),
            (0x47896, "tst.w d1"),
            (0x47898, "beq.s loc_478C0"),
            (0x4789A, "move.w #BATTLE_UNLOCKED_FLAGS_START,d1"),
            (0x4789E, "add.w d0,d1"),
            (0x478A0, "jsr j_SetFlag"),
            (0x478A6, "move.l #$100FF,((MAP_EVENT_TYPE-$1000000)).w"),
            (0x478AE, "move.w #30000,((STEP_COUNTER-$1000000)).w"),
            (0x478B4, "jsr (WaitForViewScrollEnd).w"),
            (0x478B8, "sndCom SFX_BOOST"),
            (0x478BC, "bsr.w ExecuteFlashScreenScript"),
            (0x478C0, "movem.l (sp)+,d1/d6-d7"),
            (0x478C4, "rts"),
        )
    }
    function_source: list[str] = []
    for line in source[_FUNCTION_PATH][_source_line(source[_FUNCTION_PATH], "CheckRandomBattle") :]:
        if "End of function CheckRandomBattle" in line:
            break
        statement = _normalise_statement(line)
        if statement and not statement.endswith(":"):
            function_source.append(statement)
    if function_source != list(function_addresses.values()):
        raise ValueError("map-event random-battle function source body drift")
    function_anchors = [
        _anchor(address, statement, h1, rom) for address, statement in function_addresses.items()
    ]
    if len(function_addresses) != 34 or len(function_anchors) != 34:
        raise ValueError("map-event random-battle body statement denominator drift")
    sound_extension = _anchor(0x478BA, "dc.w SFX_BOOST", h1, rom)
    service_entries: dict[str, Any] = {}
    service_anchors: list[dict[str, Any]] = []
    for order, (instruction, instruction_address, effective, effective_address, path) in enumerate(
        _SERVICES
    ):
        service_entries[instruction] = {
            "serviceOrder": order,
            "instructionTarget": instruction,
            "instructionTargetAddress": instruction_address,
            "effectiveTarget": effective,
            "effectiveTargetAddress": effective_address,
            "effectiveSourcePath": path,
        }
        for address, symbol in ((instruction_address, instruction), (effective_address, effective)):
            service_anchors.append(_anchor(address, f"{symbol}:", h1, rom))
    # The two unaliased services have identical instruction/effective entries;
    # retain seven seams, not duplicates.
    service_anchors = service_anchors[:4] + [
        service_anchors[4],
        service_anchors[6],
        service_anchors[8],
    ]
    if len(service_anchors) != 7:
        raise ValueError("map-event random-battle retained-service anchor drift")
    facts = {
        "functionAddresses": {
            "entryAddress": 0x47856,
            "endAddressExclusive": 0x478C6,
            "sourceStatementCount": 34,
            "h1InstructionRowCount": 35,
            "ownedByteCount": 112,
            "statements": [
                {"address": address, "sourceStatement": statement}
                for address, statement in function_addresses.items()
            ],
        },
        "battleIdentities": {
            symbol: {"value": enums[symbol]} for symbol in [row[3] for row in _CALLERS]
        },
        "callerSites": caller_sites,
        "completionFlagGate": {
            "baseSymbol": "BATTLE_COMPLETED_FLAGS_START",
            "callAddress": 0x47860,
            "branchAddress": 0x47866,
            "completedBranchTarget": 0x4786E,
            "notCompletedReturnFlagValue": -1,
        },
        "stepCounterGate": {
            "symbol": "STEP_COUNTER",
            "address": consts["STEP_COUNTER"],
            "testAddress": 0x4786E,
            "nonzeroBranchAddress": 0x47872,
            "nonzeroReturnFlagValue": 0,
        },
        "randomCadence": {
            "firstRange": 8,
            "firstCallAddress": 0x4787C,
            "firstZeroReturnFlagValue": -1,
            "secondRange": 4,
            "secondCallAddress": 0x4788C,
            "secondResultIncrement": 2,
            "stepCounterWriteAddress": 0x47892,
        },
        "requestWriteSequence": {
            "unlockedFlagBaseSymbol": "BATTLE_UNLOCKED_FLAGS_START",
            "setFlagCallAddress": 0x478A0,
            "mapEventTypeSymbol": "MAP_EVENT_TYPE",
            "mapEventTypeAddress": consts["MAP_EVENT_TYPE"],
            "mapEventTypeValue": 0x100FF,
            "mapEventTypeWriteAddress": 0x478A6,
            "stepCounterValue": 30000,
            "stepCounterWriteAddress": 0x478AE,
            "waitCallAddress": 0x478B4,
            "soundCommandAddress": 0x478B8,
            "soundOperandAddress": 0x478BA,
            "soundSymbol": "SFX_BOOST",
            "flashCallAddress": 0x478BC,
        },
        "returnContract": {
            "selectionTestAddress": 0x47896,
            "selectedReturnBranchAddress": 0x47898,
            "selectedReturnTarget": 0x478C0,
            "restoreAddress": 0x478C0,
            "returnAddress": 0x478C4,
        },
        "serviceEntries": service_entries,
    }
    summary = {
        "sourceIdentityCount": len(_SOURCE_PATHS),
        "motherProgramContextCount": len(all_programs),
        "positiveProgramContextCount": len(positives),
        "zeroProgramContextCount": len(all_programs) - len(positives),
        "callerContextCount": len(caller_sites),
        "physicalTableCount": 6,
        "sourceOperationCount": 58,
        "h1InstructionRowCount": 59,
        "ownedByteCount": 208,
        "retainedServiceJoinCount": 7,
        "anchorCount": 66,
    }
    if summary != {
        "sourceIdentityCount": 18,
        "motherProgramContextCount": 914,
        "positiveProgramContextCount": 7,
        "zeroProgramContextCount": 907,
        "callerContextCount": 8,
        "physicalTableCount": 6,
        "sourceOperationCount": 58,
        "h1InstructionRowCount": 59,
        "ownedByteCount": 208,
        "retainedServiceJoinCount": 7,
        "anchorCount": 66,
    }:
        raise ValueError("map-event random-battle summary denominator drift")
    output = {
        "schemaVersion": 1,
        "id": ID,
        "system": "map-event-random-battle-state",
        "upstream": {
            "repository": "https://github.com/ShiningForceCentral/SF2DISASM",
            "commit": _UPSTREAM_COMMIT,
        },
        "romSha256": _ROM_SHA256,
        "sourceContext": {
            "h1Listing": {
                "path": "build/sf2build-h1.lst",
                "sha256": hashlib.sha256(listing.encode()).hexdigest().upper(),
            },
            "motherFixture": {
                "id": "sf2-map-events-static-v1",
                "sha256": _fixture_digest("tests/fixtures/h2/map-events-static-v1.json"),
            },
            "sourceIdentities": [
                {
                    "path": path,
                    "sha256": hashlib.sha256((root / path).read_bytes()).hexdigest().upper(),
                }
                for path in _SOURCE_PATHS
            ],
            "callerAnchors": caller_anchors,
            "functionAnchors": function_anchors + [sound_extension],
            "retainedServiceAnchors": service_anchors,
        },
        "retainedOwners": {
            "mapEvents": {
                "fixtureId": "sf2-map-events-static-v1",
                "fixtureSha256": _fixture_digest("tests/fixtures/h2/map-events-static-v1.json"),
            }
        },
        "eventRandomBattleState": facts,
        "unknowns": {key: "Unknown" for key in _UNKNOWN_KEYS},
        "summary": summary,
    }
    if (
        len(output["sourceContext"]["callerAnchors"])
        + len(output["sourceContext"]["functionAnchors"])
        + len(output["sourceContext"]["retainedServiceAnchors"])
        != 66
    ):
        raise ValueError("map-event random-battle complete anchor denominator drift")
    _validate_order(output)
    return output


def verify_map_event_random_battle_state_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner=str(FIXTURE))
    _validate_order(fixture)
    output = build_map_event_random_battle_state_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="map-event random-battle static contract")
    if fixture != output:
        raise ValueError("map-event random-battle complete semantic fixture drift")
    destination = output_path or repo_path(
        "local/derived/map-event-random-battle-state-static.json"
    )
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(
        json.dumps(output, ensure_ascii=False, indent=2).encode("utf-8") + b"\n"
    )
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": hashlib.sha256(canonical_json_bytes(output)).hexdigest().upper(),
    }


def _remove_map_event_random_battle_state_later_owner_index_delta(
    index: dict[str, Any],
) -> dict[str, Any]:
    """Remove the exact random-battle later-owner delta before predecessor normalization."""
    normalized = deepcopy(index)
    records = normalized.get("records")
    if not isinstance(records, list) or len({row.get("id") for row in records}) != len(records):
        raise ValueError("map-event random-battle later-owner record shape drift")
    binding_map = {
        "map.data.ms-map66-zoneevents": [
            ("entry", "eventRandomBattleState.callerSites.map66-default-battle3.tableEntryAddress")
        ],
        "map.data.ms-map67-zoneevents": [
            ("entry", "eventRandomBattleState.callerSites.map67-default-battle21.tableEntryAddress")
        ],
        "map.data.ms-map68-zoneevents": [
            ("entry", "eventRandomBattleState.callerSites.map68-default-battle19.tableEntryAddress")
        ],
        "map.data.ms-map69-zoneevents": [
            ("entry", "eventRandomBattleState.callerSites.map69-event0-battle17.tableEntryAddress")
        ],
        "map.data.ms-map70-zoneevents": [
            ("entry", "eventRandomBattleState.callerSites.map70-event0-battle14.tableEntryAddress")
        ],
        "map.data.ms-map72-zoneevents": [
            ("entry", "eventRandomBattleState.callerSites.map72-event0-battle26.tableEntryAddress")
        ],
        "tech.interfaces.jump-s02": [
            (
                "check-flag",
                "eventRandomBattleState.serviceEntries.j_CheckFlag.instructionTargetAddress",
            ),
            (
                "set-flag",
                "eventRandomBattleState.serviceEntries.j_SetFlag.instructionTargetAddress",
            ),
        ],
        "stats.flags": [
            ("entry", "eventRandomBattleState.serviceEntries.j_CheckFlag.effectiveTargetAddress"),
            ("set-flag", "eventRandomBattleState.serviceEntries.j_SetFlag.effectiveTargetAddress"),
        ],
        "rng.generate-random-number": [
            (
                "entry",
                "eventRandomBattleState.serviceEntries.GenerateRandomNumber.effectiveTargetAddress",
            )
        ],
        "map.camera-control.wait-for-view-scroll-end": [
            (
                "entry",
                "eventRandomBattleState.serviceEntries.WaitForViewScrollEnd.effectiveTargetAddress",
            )
        ],
        "tech.graphics.flash-white": [
            (
                "entry",
                "eventRandomBattleState.serviceEntries.ExecuteFlashScreenScript.effectiveTargetAddress",
            )
        ],
    }
    added_addresses = {
        "tech.interfaces.jump-s02": [
            {"id": "check-flag", "space": "rom", "kind": "observation", "value": 0x8264},
            {"id": "set-flag", "space": "rom", "kind": "observation", "value": 0x8268},
        ],
        "stats.flags": [{"id": "set-flag", "space": "rom", "kind": "observation", "value": 0x98C4}],
    }
    expected_owner = {
        "id": "map.setup.check-random-battle",
        "subsystem": "map.setup",
        "status": "confirmed",
        "symbol": "CheckRandomBattle",
        "sourcePath": _FUNCTION_PATH,
        "addresses": [
            {"id": "entry", "space": "rom", "kind": "symbol", "value": 0x47856},
            {"id": "completion-flag-gate", "space": "rom", "kind": "observation", "value": 0x47860},
            {"id": "request-write", "space": "rom", "kind": "observation", "value": 0x478A0},
        ],
        "evidence": [
            {
                "level": "H2",
                "fixture": "tests/fixtures/h2/map-event-random-battle-state-static-v1.json",
                "fixtureId": ID,
                "verifier": "src/sf2tool/h2/map_event_random_battle_state.py",
                "bindings": [
                    {
                        "addressId": "entry",
                        "fixtureField": "eventRandomBattleState.functionAddresses.entryAddress",
                    },
                    {
                        "addressId": "completion-flag-gate",
                        "fixtureField": "eventRandomBattleState.completionFlagGate.callAddress",
                    },
                    {
                        "addressId": "request-write",
                        "fixtureField": (
                            "eventRandomBattleState.requestWriteSequence.setFlagCallAddress"
                        ),
                    },
                ],
            }
        ],
        "documents": ["docs/research/map-event-random-battle-state.md"],
    }
    seen: set[str] = set()
    for record in records[:]:
        record_id = record.get("id")
        if record_id == "map.setup.check-random-battle":
            if record != expected_owner:
                raise ValueError("map-event random-battle new index record drift")
            records.remove(record)
            continue
        if record_id not in binding_map:
            continue
        evidence = record.get("evidence")
        documents = record.get("documents")
        addresses = record.get("addresses")
        expected = {
            "level": "H2",
            "fixture": "tests/fixtures/h2/map-event-random-battle-state-static-v1.json",
            "fixtureId": ID,
            "verifier": "src/sf2tool/h2/map_event_random_battle_state.py",
            "bindings": [
                {"addressId": address_id, "fixtureField": field}
                for address_id, field in binding_map[record_id]
            ],
        }
        matches = (
            [row for row in evidence if row.get("fixtureId") == ID]
            if isinstance(evidence, list)
            else []
        )
        if (
            matches != [expected]
            or not isinstance(documents, list)
            or documents.count("docs/research/map-event-random-battle-state.md") != 1
            or documents[-1] != "docs/research/map-event-random-battle-state.md"
            or not isinstance(addresses, list)
        ):
            raise ValueError("map-event random-battle later-owner record fields drift")
        for address in added_addresses.get(record_id, []):
            if addresses.count(address) != 1:
                raise ValueError("map-event random-battle index address delta drift")
            addresses.remove(address)
        evidence.remove(expected)
        documents.remove("docs/research/map-event-random-battle-state.md")
        seen.add(record_id)
    if seen != set(binding_map) or any(
        row.get("id") == "map.setup.check-random-battle" for row in records
    ):
        raise ValueError("map-event random-battle index coverage drift")
    if hashlib.sha256(canonical_json_bytes(normalized)).hexdigest().upper() != (
        _PREDECESSOR_INDEX_SHA256
    ):
        raise ValueError("map-event random-battle predecessor index drift")
    return normalized


def normalize_map_event_random_battle_state_later_owner_index(
    index: dict[str, Any],
) -> dict[str, Any]:
    """Strictly remove this exact later-owner delta then delegate to predecessors."""
    return normalize_map_event_combatant_state_later_owner_index(
        _remove_map_event_random_battle_state_later_owner_index_delta(index)
    )
