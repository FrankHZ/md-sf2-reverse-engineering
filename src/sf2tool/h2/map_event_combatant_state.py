"""Static H2 caller contract for the two map-event combatant-state sequences."""

from __future__ import annotations

import hashlib
import json
import re
from collections import Counter
from copy import deepcopy
from pathlib import Path
from typing import Any

from sf2tool.h2.map_event_direct_handoff import _h1_instruction_rows, _normalise_statement
from sf2tool.h2.map_events_fixture import load_map_events_fixture
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path

ID = "sf2-map-event-combatant-state-static-v1"
FIXTURE = repo_path("tests/fixtures/h2/map-event-combatant-state-static-v1.json")
SCHEMA = repo_path("schemas/h2/map-event-combatant-state-static-fixture.schema.json")
_ROM_SHA256 = "9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9"
_UPSTREAM_COMMIT = "c834c652b6862bc5679fd7f69a38a7093206efc6"
_SOURCE_PATHS = (
    "sf2enums.asm",
    "code/common/tech/jumpinterfaces/s02_jumpinterface.asm",
    "code/common/stats/combatantstats_1.asm",
    "code/common/stats/combatantstats_2.asm",
    "data/maps/entries/map20/mapsetups/s3_zoneevents_501.asm",
    "data/maps/entries/map67/mapsetups/s3_zoneevents.asm",
)
_PROGRAM_SPECS = (
    ("map20-zone-event0", "Map20_1F5_ZoneEvent0", "ms_map20_flag501_ZoneEvents"),
    ("map67-zone-event0", "Map67_ZoneEvent0", "ms_map67_ZoneEvents"),
)
_SERVICE_ENTRIES = {
    "GetMaxMp": (
        "j_GetMaxMp",
        0x800C,
        "GetMaxMp",
        0x8346,
        "code/common/stats/combatantstats_1.asm",
    ),
    "GetMaxHp": (
        "j_GetMaxHp",
        0x8010,
        "GetMaxHp",
        0x8326,
        "code/common/stats/combatantstats_1.asm",
    ),
    "GetCurrentHp": (
        "j_GetCurrentHp",
        0x8048,
        "GetCurrentHp",
        0x8336,
        "code/common/stats/combatantstats_1.asm",
    ),
    "SetCurrentMp": (
        "j_SetCurrentMp",
        0x80B8,
        "SetCurrentMp",
        0x85C6,
        "code/common/stats/combatantstats_2.asm",
    ),
    "SetCurrentHp": (
        "j_SetCurrentHp",
        0x80C0,
        "SetCurrentHp",
        0x85A6,
        "code/common/stats/combatantstats_2.asm",
    ),
}
_SERVICE_ORDER = tuple(_SERVICE_ENTRIES)
_SELECTORS = (
    ("Sarah", "ALLY_SARAH", 1),
    ("Chester", "ALLY_CHESTER", 2),
    ("Elric", "ALLY_ELRIC", 13),
)
_UNKNOWNS = (
    "naturalProgramReachability",
    "selectedProgramContext",
    "callerEntryRegistersAndState",
    "actualServiceEntryAndReturnOrder",
    "actualAllyIdentityAtEachService",
    "actualMaxHpAndMpResults",
    "actualCurrentHpResult",
    "actualCurrentHpAndMpWrites",
    "actualHpPredicateValueAndBranch",
    "actualScriptAndFlagEffects",
    "persistenceAcrossMapSwitchSaveLoad",
    "inputDialogueAudioPresentationTimingAndStoryMeaning",
)
_RETAINED = (
    ("mapEvents", "sf2-map-events-static-v1", "tests/fixtures/h2/map-events-static-v1.json"),
    ("commonStats", "sf2-common-stats-static-v1", "tests/fixtures/h2/common-stats-static-v1.json"),
    (
        "techInterfaces",
        "sf2-tech-interfaces-static-v1",
        "tests/fixtures/h2/tech-interfaces-static-v1.json",
    ),
)
_EQUATE = re.compile(r"^\s*([A-Za-z_][A-Za-z0-9_]*)\s*:\s+equ\s+([^\s;]+)")


def canonical_json_bytes(value: dict[str, Any]) -> bytes:
    return (
        json.dumps(value, ensure_ascii=False, sort_keys=True, separators=(",", ":")).encode("utf-8")
        + b"\n"
    )


def _root(upstream_path: Path) -> Path:
    root = upstream_path.resolve(strict=True)
    return root / "disasm" if (root / "disasm").is_dir() else root


def _fixture_digest(path: str) -> str:
    return hashlib.sha256(repo_path(path).read_bytes()).hexdigest().upper()


def _source_line(lines: list[str], symbol: str) -> int:
    found = [
        number
        for number, line in enumerate(lines, 1)
        if re.fullmatch(rf"\s*{re.escape(symbol)}:\s*(?:;.*)?", line)
    ]
    if len(found) != 1:
        raise ValueError(f"map-event combatant state label drift: {symbol}")
    return found[0]


def _parse_selectors(lines: list[str]) -> dict[str, dict[str, int]]:
    expected = {symbol: value for _name, symbol, value in _SELECTORS}
    found: dict[str, list[tuple[int, int]]] = {symbol: [] for symbol in expected}
    for number, line in enumerate(lines, 1):
        match = _EQUATE.match(line)
        if match is None or match.group(1) not in expected:
            continue
        raw = match.group(2)
        value = int(raw[1:], 16) if raw.startswith("$") else int(raw) if raw.isdecimal() else -1
        found[match.group(1)].append((number, value))
    if {key: len(rows) for key, rows in found.items()} != {key: 1 for key in expected}:
        raise ValueError("map-event combatant state ally-selector occurrence drift")
    if {key: rows[0][1] for key, rows in found.items()} != expected:
        raise ValueError("map-event combatant state ally-selector value drift")
    return {
        symbol: {"sourceLine": rows[0][0], "value": rows[0][1]} for symbol, rows in found.items()
    }


def _anchor(
    operation: dict[str, Any], h1: dict[int, tuple[bytes, str]], rom: bytes, width: int
) -> dict[str, Any]:
    address = operation["address"]
    if address not in h1 or width <= 0:
        raise ValueError(f"map-event combatant state H1 span drift: {address:#x}")
    encoded, statement = h1[address]
    actual = rom[address : address + width]
    if len(actual) != width:
        raise ValueError(f"map-event combatant state ROM span drift: {address:#x}")
    return {
        "address": address,
        "mnemonic": operation["mnemonic"],
        "h1ListedByteCount": len(encoded),
        "romEncodedByteCount": width,
        "romInstructionSha256": hashlib.sha256(actual).hexdigest().upper(),
        "h1StatementSha256": hashlib.sha256(statement.encode("utf-8")).hexdigest().upper(),
    }


def _seam_anchor(
    address: int,
    role: str,
    path: str,
    symbol: str,
    source: dict[str, list[str]],
    h1: dict[int, tuple[bytes, str]],
    rom: bytes,
    service_id: str,
) -> dict[str, Any]:
    line = _source_line(source[path], symbol)
    if not re.fullmatch(rf"\s*{re.escape(symbol)}:\s*(?:;.*)?", source[path][line - 1]):
        raise ValueError(f"map-event combatant state seam source drift: {role}")
    encoded, statement = h1.get(address, (b"", ""))
    if not encoded or len(rom[address : address + len(encoded)]) != len(encoded):
        raise ValueError(f"map-event combatant state seam H1/ROM drift: {role}")
    return {
        "address": address,
        "role": role,
        "serviceEntryId": service_id,
        "sourcePath": path,
        "sourceLine": line,
        "sourceStatementSha256": hashlib.sha256(f"{symbol}:".encode()).hexdigest().upper(),
        "h1ListedByteCount": len(encoded),
        "h1EncodedSha256": hashlib.sha256(encoded).hexdigest().upper(),
        "romEncodedByteCount": len(encoded),
        "romInstructionSha256": hashlib.sha256(rom[address : address + len(encoded)])
        .hexdigest()
        .upper(),
        "h1StatementSha256": hashlib.sha256(statement.encode("utf-8")).hexdigest().upper(),
    }


def _operand_shapes(operation: dict[str, Any]) -> list[str]:
    result = []
    for operand in operation["operandTexts"]:
        if operand.startswith("#ALLY_"):
            result.append("ally-immediate")
        elif operand.startswith("#"):
            result.append("numeric-immediate")
        elif re.fullmatch(r"[da][0-7]", operand, re.IGNORECASE):
            result.append("register")
        else:
            result.append("symbol")
    return result


def _public_operation(order: int, physical_id: str, operation: dict[str, Any]) -> dict[str, Any]:
    target = operation["target"]
    return {
        "operationOrder": order,
        "operationId": f"{physical_id}:{operation['address']:06X}",
        "sourceLine": operation["sourceLine"],
        "address": operation["address"],
        "mnemonic": operation["mnemonic"],
        "sizeSuffix": operation["sizeSuffix"],
        "operandTexts": operation["operandTexts"],
        "operandShapes": _operand_shapes(operation),
        "family": operation["family"],
        "controlFlowKind": operation["controlFlowKind"],
        "target": None
        if target is None
        else {
            "instructionTarget": target["instructionTargetSymbol"],
            "instructionTargetAddress": target["instructionTargetAddress"],
            "effectiveTarget": target["effectiveTargetSymbol"],
            "effectiveTargetAddress": target["effectiveTargetAddress"],
            "scope": target["effectiveTargetScope"],
        },
    }


def _retained_owners(map_events: dict[str, Any]) -> dict[str, Any]:
    if load_map_events_fixture()["expected"] != map_events:
        raise ValueError("map-event combatant state retained map-events fixture drift")
    return {
        name: {"fixtureId": fixture_id, "fixtureSha256": _fixture_digest(path)}
        for name, fixture_id, path in _RETAINED
    }


def _validate_order(value: dict[str, Any]) -> None:
    if list(value) != [
        "schemaVersion",
        "id",
        "upstream",
        "romSha256",
        "retainedOwners",
        "sourceContext",
        "eventCombatantState",
        "unknowns",
        "summary",
    ]:
        raise ValueError("map-event combatant state root order drift")
    facts = value["eventCombatantState"]
    expected = [
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
        "serviceCallOrder",
        "serviceCalls",
        "restorationChainOrder",
        "restorationChains",
        "resultPredicate",
        "digests",
    ]
    if list(facts) != expected:
        raise ValueError("map-event combatant state field order drift")
    for records, order in (
        ("sourceFiles", "sourceFileOrder"),
        ("programContexts", "programContextOrder"),
        ("physicalPrograms", "physicalProgramOrder"),
        ("allySelectors", "allySelectorOrder"),
        ("serviceEntries", "serviceEntryOrder"),
        ("serviceCalls", "serviceCallOrder"),
        ("restorationChains", "restorationChainOrder"),
    ):
        if list(facts[records]) != facts[order]:
            raise ValueError(f"map-event combatant state order drift: {records}")


def build_map_event_combatant_state_contract(
    rom_path: Path, upstream_path: Path, *, map_events_override: dict[str, Any] | None = None
) -> dict[str, Any]:
    rom = rom_path.resolve(strict=True).read_bytes()
    if hashlib.sha256(rom).hexdigest().upper() != _ROM_SHA256:
        raise ValueError("map-event combatant state ROM identity drift")
    # The complete event-program corpus is the accepted retained H2 owner.  Loading its
    # tracked public fixture keeps this narrow caller join independent of unrelated
    # private text-bank payloads consumed by the full owner reconstruction.
    map_events = (
        load_map_events_fixture()["expected"]
        if map_events_override is None
        else deepcopy(map_events_override)
    )
    retained = _retained_owners(map_events)
    root = _root(upstream_path)
    source = {
        path: (root / path).read_text(encoding="utf-8").splitlines() for path in _SOURCE_PATHS
    }
    selector_constants = _parse_selectors(source["sf2enums.asm"])
    listing = (upstream_path.resolve(strict=True) / "build/sf2build-h1.lst").read_text(
        encoding="utf-8"
    )
    h1 = _h1_instruction_rows(listing)
    all_programs = [
        *map_events["entityTargetPrograms"],
        *map_events["zoneTargetPrograms"],
        *map_events["itemTargetPrograms"],
    ]
    positives = [
        program
        for program in all_programs
        if program["canonicalSymbol"] in {item[1] for item in _PROGRAM_SPECS}
    ]
    if len(all_programs) != 914 or len(positives) != 2 or len(all_programs) - len(positives) != 912:
        raise ValueError("map-event combatant state mother corpus selection drift")
    tables = {table["symbol"]: table for table in map_events["categories"]["zoneEvents"]["tables"]}
    source_files: dict[str, Any] = {}
    contexts: dict[str, Any] = {}
    physical: dict[str, Any] = {}
    operations_by_context: dict[str, list[dict[str, Any]]] = {}
    physical_operations: dict[int, dict[str, Any]] = {}
    for context_order, (context_id, symbol, table_symbol) in enumerate(_PROGRAM_SPECS):
        matches = [item for item in positives if item["canonicalSymbol"] == symbol]
        if len(matches) != 1:
            raise ValueError(f"map-event combatant state program identity drift: {symbol}")
        program = matches[0]
        table = tables.get(table_symbol)
        if table is None or table["sourcePath"] != program["sourcePath"]:
            raise ValueError(f"map-event combatant state table owner drift: {symbol}")
        source_files[table_symbol] = {
            "sourcePath": program["sourcePath"],
            "tableEntryAddress": table["address"],
            "tableSymbol": table_symbol,
        }
        rows = []
        for order, operation in enumerate(program["operations"]):
            statement = operation["sourceMnemonic"] + (
                " " + ",".join(operation["operandTexts"]) if operation["operandTexts"] else ""
            )
            if _normalise_statement(
                source[program["sourcePath"]][operation["sourceLine"] - 1]
            ) != _normalise_statement(statement):
                detail = f"{symbol}:{operation['sourceLine']}"
                raise ValueError(f"map-event combatant state source operation drift: {detail}")
            rows.append(_public_operation(order, context_id, operation))
            physical_operations[operation["address"]] = operation
        if program["encodedSpanBytes"] != program["endAddressExclusive"] - program["entryAddress"]:
            raise ValueError(f"map-event combatant state range drift: {symbol}")
        contexts[context_id] = {
            "contextOrder": context_order,
            "sourceFileId": table_symbol,
            "programSymbol": symbol,
            "physicalProgramId": context_id,
            "entryAddress": program["entryAddress"],
            "endAddressExclusive": program["endAddressExclusive"],
            "operationIds": [row["operationId"] for row in rows],
            "labelAddresses": [label["address"] for label in program["labels"]],
            "operations": rows,
        }
        physical[context_id] = {
            "sourcePath": program["sourcePath"],
            "programSymbol": symbol,
            "entryAddress": program["entryAddress"],
            "endAddressExclusive": program["endAddressExclusive"],
            "encodedByteCount": program["encodedSpanBytes"],
            "operationAddresses": [row["address"] for row in program["operations"]],
            "labelAddresses": [label["address"] for label in program["labels"]],
        }
        operations_by_context[context_id] = program["operations"]
    spans = {
        address: next_address - address
        for row in physical.values()
        for index, address in enumerate(row["operationAddresses"])
        for next_address in [
            row["operationAddresses"][index + 1]
            if index + 1 < len(row["operationAddresses"])
            else row["endAddressExclusive"]
        ]
    }
    operation_anchors = [
        _anchor(operation, h1, rom, spans[address])
        for address, operation in sorted(physical_operations.items())
    ]
    if len(operation_anchors) != 23:
        raise ValueError("map-event combatant state operation anchor denominator drift")
    selector_rows = {
        name: {
            "selectorOrder": order,
            "sourceSymbol": symbol,
            "value": value,
            "enumSourceLine": selector_constants[symbol]["sourceLine"],
        }
        for order, (name, symbol, value) in enumerate(_SELECTORS)
    }
    service_entries = {
        name: {
            "entryOrder": order,
            "instructionTarget": row[0],
            "instructionTargetAddress": row[1],
            "effectiveEntry": row[2],
            "effectiveEntryAddress": row[3],
            "effectiveSourcePath": row[4],
        }
        for order, (name, row) in enumerate(_SERVICE_ENTRIES.items())
    }
    calls: dict[str, Any] = {}
    selector_for_context: dict[str, str | None] = {key: None for key in contexts}
    for context_id, operations in operations_by_context.items():
        active: str | None = None
        for operation in operations:
            operands = operation["operandTexts"]
            if (
                len(operands) == 2
                and operands[1] == "d0"
                and operation["mnemonic"] in {"moveq", "move"}
            ):
                raw = operands[0].lstrip("#")
                if raw == "1":
                    active = "Sarah"
                elif raw == "2":
                    active = "Chester"
                elif raw == "ALLY_ELRIC":
                    active = "Elric"
            target = operation["target"]
            if target is None or target["effectiveTargetSymbol"] not in service_entries:
                continue
            if active is None:
                raise ValueError("map-event combatant state selector/call order drift")
            service = target["effectiveTargetSymbol"]
            expected = service_entries[service]
            if (
                target["instructionTargetSymbol"],
                target["instructionTargetAddress"],
                target["effectiveTargetAddress"],
            ) != (
                expected["instructionTarget"],
                expected["instructionTargetAddress"],
                expected["effectiveEntryAddress"],
            ):
                raise ValueError("map-event combatant state alias/effective target drift")
            call_id = f"{context_id}:{service}:{operation['address']:06X}"
            calls[call_id] = {
                "serviceCallOrder": len(calls),
                "contextId": context_id,
                "physicalProgramId": context_id,
                "allySelectorId": active,
                "serviceEntryId": service,
                "sourceLine": operation["sourceLine"],
                "callAddress": operation["address"],
                "instructionTarget": target["instructionTargetSymbol"],
                "instructionTargetAddress": target["instructionTargetAddress"],
                "effectiveTarget": target["effectiveTargetSymbol"],
                "effectiveTargetAddress": target["effectiveTargetAddress"],
            }
            selector_for_context[context_id] = active
    if len(calls) != 9:
        raise ValueError("map-event combatant state stat-call denominator drift")
    ordered_calls = list(calls)
    expected_sequence = ("GetMaxHp", "SetCurrentHp", "GetMaxMp", "SetCurrentMp")
    chains: dict[str, Any] = {}
    for name in ("Sarah", "Chester"):
        selected = [
            call_id for call_id in ordered_calls if calls[call_id]["allySelectorId"] == name
        ]
        if tuple(calls[item]["serviceEntryId"] for item in selected) != expected_sequence:
            raise ValueError(f"map-event combatant state restoration sequence drift: {name}")
        chains[name] = {
            "chainOrder": len(chains),
            "contextId": "map20-zone-event0",
            "allySelectorId": name,
            "serviceCallIds": selected,
            "serviceEntryOrder": list(expected_sequence),
        }
    current_call = [row for row in calls.values() if row["serviceEntryId"] == "GetCurrentHp"]
    map67 = operations_by_context["map67-zone-event0"]
    index = next(
        i for i, row in enumerate(map67) if row["address"] == current_call[0]["callAddress"]
    )
    producer, branch = map67[index + 1 : index + 3]
    if (
        producer["mnemonic"] != "tst"
        or producer["sizeSuffix"] != ".w"
        or producer["operandTexts"] != ["d1"]
        or branch["mnemonic"] != "beq"
        or branch["sizeSuffix"] != ".s"
        or branch["target"] is None
    ):
        raise ValueError("map-event combatant state CurrentHp predicate shape drift")
    encoded, _ = h1[branch["address"]]
    predicate = {
        "serviceCallId": next(key for key, row in calls.items() if row is current_call[0]),
        "producerAddress": producer["address"],
        "producerMnemonic": producer["mnemonic"],
        "producerSizeSuffix": producer["sizeSuffix"],
        "producerOperands": producer["operandTexts"],
        "branchAddress": branch["address"],
        "branchMnemonic": branch["mnemonic"],
        "branchSizeSuffix": branch["sizeSuffix"],
        "targetAddress": branch["target"]["effectiveTargetAddress"],
        "fallthroughAddress": branch["address"] + len(encoded),
    }
    table_anchors = []
    for source_id, row in source_files.items():
        record = next(
            (
                item
                for item in tables[row["tableSymbol"]]["records"]
                if item["targetCanonicalSymbol"]
                == contexts[
                    "map20-zone-event0"
                    if source_id == "ms_map20_flag501_ZoneEvents"
                    else "map67-zone-event0"
                ]["programSymbol"]
            ),
            None,
        )
        if record is None or record["address"] != row["tableEntryAddress"]:
            raise ValueError("map-event combatant state source-table mutation")
        encoded, statement = h1[record["address"]]
        table_anchors.append(
            {
                "address": record["address"],
                "role": "event-table-entry",
                "sourceFileId": source_id,
                "sourcePath": record["sourcePath"],
                "sourceLine": record["sourceLine"],
                "h1ListedByteCount": len(encoded),
                "romEncodedByteCount": len(encoded),
                "romInstructionSha256": hashlib.sha256(
                    rom[record["address"] : record["address"] + len(encoded)]
                )
                .hexdigest()
                .upper(),
                "h1StatementSha256": hashlib.sha256(statement.encode("utf-8")).hexdigest().upper(),
            }
        )
    seams = []
    for service_id, entry in service_entries.items():
        seams.append(
            _seam_anchor(
                entry["instructionTargetAddress"],
                "instruction-target",
                "code/common/tech/jumpinterfaces/s02_jumpinterface.asm",
                entry["instructionTarget"],
                source,
                h1,
                rom,
                service_id,
            )
        )
        seams.append(
            _seam_anchor(
                entry["effectiveEntryAddress"],
                "effective-entry",
                entry["effectiveSourcePath"],
                entry["effectiveEntry"],
                source,
                h1,
                rom,
                service_id,
            )
        )
    if len(table_anchors) != 2 or len(seams) != 10 or 23 + len(table_anchors) + len(seams) != 35:
        raise ValueError("map-event combatant state anchor denominator drift")
    counts = Counter(row["family"] for row in physical_operations.values())
    summary = {
        "sourceIdentityCount": len(_SOURCE_PATHS),
        "motherProgramContextCount": len(all_programs),
        "positiveProgramContextCount": len(contexts),
        "zeroProgramContextCount": len(all_programs) - len(contexts),
        "physicalProgramCount": len(physical),
        "contextOperationCount": sum(len(row["operations"]) for row in contexts.values()),
        "physicalOperationCount": len(physical_operations),
        "contextEncodedByteCount": sum(
            row["endAddressExclusive"] - row["entryAddress"] for row in contexts.values()
        ),
        "physicalEncodedByteCount": sum(row["encodedByteCount"] for row in physical.values()),
        "physicalLabelCount": sum(len(row["labelAddresses"]) for row in physical.values()),
        "eventServiceMacroPhysicalOperationCount": counts["event-service-macro"],
        "rawInstructionPhysicalOperationCount": counts["raw-68000-instruction"],
        "rawControlPhysicalOperationCount": counts["raw-68000-control-flow"],
        "statCallCount": len(calls),
        "allySelectorCount": len(selector_rows),
        "restorationChainCount": len(chains),
        "resultPredicateCount": 1,
        "anchorCount": 35,
    }
    expected_summary = {
        "sourceIdentityCount": 6,
        "motherProgramContextCount": 914,
        "positiveProgramContextCount": 2,
        "zeroProgramContextCount": 912,
        "physicalProgramCount": 2,
        "contextOperationCount": 23,
        "physicalOperationCount": 23,
        "contextEncodedByteCount": 98,
        "physicalEncodedByteCount": 98,
        "physicalLabelCount": 3,
        "eventServiceMacroPhysicalOperationCount": 5,
        "rawInstructionPhysicalOperationCount": 4,
        "rawControlPhysicalOperationCount": 14,
        "statCallCount": 9,
        "allySelectorCount": 3,
        "restorationChainCount": 2,
        "resultPredicateCount": 1,
        "anchorCount": 35,
    }
    if summary != expected_summary:
        raise ValueError(f"map-event combatant state denominator drift: {summary}")
    facts = {
        "sourceFileOrder": list(source_files),
        "sourceFiles": source_files,
        "programContextOrder": list(contexts),
        "programContexts": contexts,
        "physicalProgramOrder": list(physical),
        "physicalPrograms": physical,
        "allySelectorOrder": list(selector_rows),
        "allySelectors": selector_rows,
        "serviceEntryOrder": list(service_entries),
        "serviceEntries": service_entries,
        "serviceCallOrder": list(calls),
        "serviceCalls": calls,
        "restorationChainOrder": list(chains),
        "restorationChains": chains,
        "resultPredicate": predicate,
        "digests": {
            "programContextsSha256": hashlib.sha256(
                canonical_json_bytes({"programContexts": contexts})
            )
            .hexdigest()
            .upper(),
            "serviceCallsSha256": hashlib.sha256(canonical_json_bytes({"serviceCalls": calls}))
            .hexdigest()
            .upper(),
            "restorationChainsSha256": hashlib.sha256(
                canonical_json_bytes({"restorationChains": chains})
            )
            .hexdigest()
            .upper(),
            "resultPredicateSha256": hashlib.sha256(
                canonical_json_bytes({"resultPredicate": predicate})
            )
            .hexdigest()
            .upper(),
        },
    }
    output = {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {
            "repository": "https://github.com/ShiningForceCentral/SF2DISASM",
            "commit": _UPSTREAM_COMMIT,
        },
        "romSha256": _ROM_SHA256,
        "retainedOwners": retained,
        "sourceContext": {
            "h1Listing": {
                "path": "build/sf2build-h1.lst",
                "sha256": hashlib.sha256(listing.encode("utf-8")).hexdigest().upper(),
            },
            "sourceIdentities": [
                {
                    "path": path,
                    "sha256": hashlib.sha256((root / path).read_bytes()).hexdigest().upper(),
                }
                for path in _SOURCE_PATHS
            ],
            "physicalOperationAnchors": operation_anchors,
            "eventTableAnchors": table_anchors,
            "serviceSeamAnchors": seams,
        },
        "eventCombatantState": facts,
        "unknowns": {key: "Unknown" for key in _UNKNOWNS},
        "summary": summary,
    }
    _validate_order(output)
    return output


def verify_map_event_combatant_state_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner=str(FIXTURE))
    _validate_order(fixture)
    output = build_map_event_combatant_state_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="map-event combatant state static contract")
    if fixture != output:
        raise ValueError("map-event combatant state complete semantic fixture drift")
    destination = output_path or repo_path("local/derived/map-event-combatant-state-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(
        json.dumps(output, ensure_ascii=False, indent=2).encode("utf-8") + b"\n"
    )
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": hashlib.sha256(canonical_json_bytes(output)).hexdigest().upper(),
        "StatCalls": 9,
        "Status": "PASS",
    }


_INDEX_DOCUMENT = "docs/research/map-event-combatant-state.md"
_INDEX_FIXTURE = "tests/fixtures/h2/map-event-combatant-state-static-v1.json"
_INDEX_DELTA = {
    "map.data.ms-map20-flag501-zoneevents": (
        ("entry", "eventCombatantState.sourceFiles.ms_map20_flag501_ZoneEvents.tableEntryAddress"),
    ),
    "map.data.ms-map67-zoneevents": (
        ("entry", "eventCombatantState.sourceFiles.ms_map67_ZoneEvents.tableEntryAddress"),
    ),
    "tech.interfaces.jump-s02": (
        ("get-max-mp", "eventCombatantState.serviceEntries.GetMaxMp.instructionTargetAddress"),
        ("get-max-hp", "eventCombatantState.serviceEntries.GetMaxHp.instructionTargetAddress"),
        (
            "get-current-hp",
            "eventCombatantState.serviceEntries.GetCurrentHp.instructionTargetAddress",
        ),
        (
            "set-current-mp",
            "eventCombatantState.serviceEntries.SetCurrentMp.instructionTargetAddress",
        ),
        (
            "set-current-hp",
            "eventCombatantState.serviceEntries.SetCurrentHp.instructionTargetAddress",
        ),
    ),
    "stats.combatant-getters": (
        ("entry", "eventCombatantState.serviceEntries.GetMaxHp.effectiveEntryAddress"),
        ("get-current-hp", "eventCombatantState.serviceEntries.GetCurrentHp.effectiveEntryAddress"),
        ("get-max-mp", "eventCombatantState.serviceEntries.GetMaxMp.effectiveEntryAddress"),
    ),
    "stats.combatant-setters": (
        ("set-current-mp", "eventCombatantState.serviceEntries.SetCurrentMp.effectiveEntryAddress"),
        ("set-current-hp", "eventCombatantState.serviceEntries.SetCurrentHp.effectiveEntryAddress"),
    ),
}
_INDEX_ADDRESS_DELTA = {
    "tech.interfaces.jump-s02": (
        ("get-max-mp", 0x800C),
        ("get-max-hp", 0x8010),
        ("get-current-hp", 0x8048),
        ("set-current-mp", 0x80B8),
        ("set-current-hp", 0x80C0),
    ),
    "stats.combatant-getters": (
        ("entry", 0x8326),
        ("get-current-hp", 0x8336),
        ("get-max-mp", 0x8346),
    ),
    "stats.combatant-setters": (("set-current-mp", 0x85C6), ("set-current-hp", 0x85A6)),
}
_INDEX_RECORD_COUNT = 1626
_INDEX_RECORD_IDS_SHA256 = "F16C0FCC0752982BA2262201BCF9EC40D467719460F97DC06562497937E809F5"
_INDEX_PREDECESSOR_SHA256 = "E987286D1D27BA96DE1A5CF0F3F3179C38CCF19048095865DA4934E4C956ECA7"
_INDEX_PREDECESSOR_RECORD_SHA256 = {
    "map.data.ms-map20-flag501-zoneevents": (
        "4C10C11041910B0856267EB65CF5EE9837EAAD2C7286A933B423D8BFB5838130"
    ),
    "map.data.ms-map67-zoneevents": (
        "B6E126D6546438F3132EE84A6F562E1ADCDE8C562E1E713FCD2B876762E4A9B6"
    ),
    "tech.interfaces.jump-s02": (
        "B4A5FF6A0B2B91D99C284AE34DBF4EADF64343DE055FAF7A9AA021D872B5790E"
    ),
    "stats.combatant-setters": ("8C9A721C2575551F0BD6BD9822FF6F4B7E18CC9D1FB44FF21B9A622306942F14"),
}


def _index_record_digest(record: dict[str, Any]) -> str:
    return (
        hashlib.sha256(
            json.dumps(record, ensure_ascii=False, sort_keys=True, separators=(",", ":")).encode(
                "utf-8"
            )
        )
        .hexdigest()
        .upper()
    )


def _remove_map_event_combatant_state_later_owner_index_delta(
    index: dict[str, Any],
) -> dict[str, Any]:
    """Strictly remove this exact later owner, then delegate to its predecessor."""
    normalized = deepcopy(index)
    records = normalized.get("records")
    if not isinstance(records, list) or len({row.get("id") for row in records}) != len(records):
        raise ValueError("map-event combatant state later-owner record identity drift")
    record_ids_digest = (
        hashlib.sha256(
            json.dumps([row["id"] for row in records], separators=(",", ":")).encode("utf-8")
        )
        .hexdigest()
        .upper()
    )
    if len(records) != _INDEX_RECORD_COUNT or record_ids_digest != _INDEX_RECORD_IDS_SHA256:
        raise ValueError("map-event combatant state later-owner record set/order drift")
    seen: set[str] = set()
    addresses = 0
    bindings = 0
    for record in records[:]:
        evidence = [row for row in record.get("evidence", []) if row.get("fixtureId") == ID]
        if not evidence:
            continue
        record_id = record["id"]
        documents = record.get("documents")
        address_rows = record.get("addresses")
        expected_bindings = [
            {"addressId": aid, "fixtureField": field}
            for aid, field in _INDEX_DELTA.get(record_id, ())
        ]
        expected = {
            "level": "H2",
            "fixture": _INDEX_FIXTURE,
            "fixtureId": ID,
            "verifier": "src/sf2tool/h2/map_event_combatant_state.py",
            "bindings": expected_bindings,
        }
        if (
            record_id not in _INDEX_DELTA
            or evidence != [expected]
            or not isinstance(documents, list)
            or documents.count(_INDEX_DOCUMENT) != 1
            or documents[-1] != _INDEX_DOCUMENT
            or not isinstance(address_rows, list)
        ):
            raise ValueError("map-event combatant state later-owner evidence/document drift")
        bindings += len(expected_bindings)
        for aid, value in _INDEX_ADDRESS_DELTA.get(record_id, ()):
            rows = [row for row in address_rows if row["id"] == aid]
            kind = (
                "symbol"
                if record_id == "stats.combatant-getters" and aid == "entry"
                else "observation"
            )
            if rows != [{"id": aid, "space": "rom", "kind": kind, "value": value}]:
                raise ValueError("map-event combatant state later-owner address object drift")
            address_rows.remove(rows[0])
            addresses += 1
        record["evidence"].remove(evidence[0])
        documents.remove(_INDEX_DOCUMENT)
        if record_id == "stats.combatant-getters":
            records.remove(record)
        elif _index_record_digest(record) != _INDEX_PREDECESSOR_RECORD_SHA256[record_id]:
            raise ValueError("map-event combatant state later-owner unrelated object drift")
        seen.add(record_id)
    if seen != set(_INDEX_DELTA) or addresses != 10 or bindings != 12:
        raise ValueError("map-event combatant state later-owner coverage drift")
    if (
        hashlib.sha256(canonical_json_bytes(normalized)).hexdigest().upper()
        != _INDEX_PREDECESSOR_SHA256
    ):
        raise ValueError("map-event combatant state later-owner unrelated index drift")
    return normalized


def normalize_map_event_combatant_state_later_owner_index(index: dict[str, Any]) -> dict[str, Any]:
    """Strictly remove this exact later owner, then delegate to its predecessor."""
    from sf2tool.h2.map_event_item_transactions import (
        normalize_map_event_item_transactions_later_owner_index,
    )

    return normalize_map_event_item_transactions_later_owner_index(
        _remove_map_event_combatant_state_later_owner_index_delta(index)
    )
