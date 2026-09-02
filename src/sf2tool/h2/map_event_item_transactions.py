"""Static H2 item-transaction choreography inside selected map events.

This owner starts with the accepted complete map-event program corpus and
narrows it to seven table sources and eight source contexts.  It models caller
choreography only: service entries, item setup, predicate shapes, the Map 6
nested tail, item-event ``d6`` writes, and the FieldMenu return seam.  It never
opens an inventory service body or asserts a runtime result.
"""

from __future__ import annotations

import hashlib
import json
import re
from collections import Counter
from copy import deepcopy
from pathlib import Path
from typing import Any

from sf2tool.h2.map_event_direct_handoff import _h1_instruction_rows, _normalise_statement
from sf2tool.h2.map_events import _canonical_bytes as _map_events_canonical_bytes
from sf2tool.h2.map_events import build_map_events_contract
from sf2tool.h2.map_events_fixture import load_map_events_fixture
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path

ID = "sf2-map-event-item-transactions-static-v1"
FIXTURE = repo_path("tests/fixtures/h2/map-event-item-transactions-static-v1.json")
SCHEMA = repo_path("schemas/h2/map-event-item-transactions-static-fixture.schema.json")
MAP_EVENTS_MANIFEST = repo_path("manifests/extractions/map-events-static.json")

_ROM_SHA256 = "9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9"
_UPSTREAM_COMMIT = "c834c652b6862bc5679fd7f69a38a7093206efc6"
_SOURCE_PATHS = (
    "sf2const.asm",
    "sf2enums.asm",
    "sf2macros.asm",
    "code/common/menus/main/mainactions.asm",
    "code/common/scripting/map/mapsetupsfunctions_1.asm",
    "code/common/tech/jumpinterfaces/s02_jumpinterface.asm",
    "code/common/tech/jumpinterfaces/s07_jumpinterface.asm",
    "code/common/stats/itemstats.asm",
    "code/common/stats/iteminventory.asm",
    "data/maps/entries/map06/mapsetups/s2_entityevents_701.asm",
    "data/maps/entries/map09/mapsetups/s2_entityevents.asm",
    "data/maps/entries/map63/mapsetups/s2_entityevents.asm",
    "data/maps/entries/map72/mapsetups/s3_zoneevents.asm",
    "data/maps/entries/map08/mapsetups/s5_itemevents.asm",
    "data/maps/entries/map22/mapsetups/s5_itemevents.asm",
    "data/maps/entries/map63/mapsetups/s5_itemevents.asm",
)
_UNKNOWN_KEYS = (
    "naturalProgramReachability",
    "selectedProgramContext",
    "callerEntryRegistersAndState",
    "actualInventoryContents",
    "actualItemLocationResult",
    "actualMandatoryReceiveResult",
    "actualRemoveBySlotResult",
    "actualRemoveByItemResult",
    "actualPredicateValuesAndBranches",
    "actualItemEventD6ValueAtCaller",
    "actualItemConsumptionOrAcquisition",
    "actualFlagsAndMapScriptEffects",
    "persistenceAcrossMapSwitchSaveLoad",
    "inputDialogueAudioPresentationTimingAndStoryMeaning",
)
_PROGRAM_SPECS = (
    ("map6-entity-event13", "entityEvents", "Map6_EntityEvent13", None),
    ("map6-default-entity-event", "entityEvents", "Map6_EntityEvent13", "Map6_DefaultEntityEvent"),
    ("map9-entity-event0", "entityEvents", "Map9_EntityEvent0", None),
    ("map63-entity-event0", "entityEvents", "Map63_EntityEvent0", None),
    ("map72-zone-event3", "zoneEvents", "Map72_ZoneEvent3", None),
    ("map8-item-event0", "itemEvents", "Map8_ItemEvent0", None),
    ("map22-item-event0", "itemEvents", "Map22_ItemEvent0", None),
    ("map63-item-event0", "itemEvents", "Map63_ItemEvent0", None),
)
_SOURCE_FILE_IDS = (
    "ms_map6_flag701_EntityEvents",
    "ms_map9_EntityEvents",
    "ms_map63_EntityEvents",
    "ms_map72_ZoneEvents",
    "ms_map8_Section5",
    "ms_map22_Section5",
    "ms_map63_Section5",
)
_PROGRAM_CONTEXT_IDS = tuple(spec[0] for spec in _PROGRAM_SPECS)
_PHYSICAL_PROGRAM_IDS = tuple(
    context_id
    for context_id, _category, _symbol, nested_start in _PROGRAM_SPECS
    if nested_start is None
)
_SERVICE_ENTRY_IDS = (
    "GetItemInventoryLocation",
    "ReceiveMandatoryItem",
    "RemoveItemBySlot",
    "RemoveItemFromInventory",
)
_SERVICE_TARGETS = {
    "GetItemInventoryLocation": {
        "id": "GetItemInventoryLocation",
        "instructionTarget": "j_GetItemInventoryLocation",
        "instructionTargetAddress": 33232,
        "entryAddress": 37190,
        "sourcePath": "code/common/stats/itemstats.asm",
        "entrySymbol": "GetItemInventoryLocation",
    },
    "ReceiveMandatoryItem": {
        "id": "ReceiveMandatoryItem",
        "instructionTarget": "ReceiveMandatoryItem",
        "instructionTargetAddress": 324746,
        "entryAddress": 324746,
        "sourcePath": "code/common/stats/iteminventory.asm",
        "entrySymbol": "ReceiveMandatoryItem",
    },
    "RemoveItemBySlot": {
        "id": "RemoveItemBySlot",
        "instructionTarget": "j_RemoveItemBySlot",
        "instructionTargetAddress": 33180,
        "entryAddress": 36470,
        "sourcePath": "code/common/stats/itemstats.asm",
        "entrySymbol": "RemoveItemBySlot",
    },
    "RemoveItemFromInventory": {
        "id": "RemoveItemFromInventory",
        "instructionTarget": "RemoveItemFromInventory",
        "instructionTargetAddress": 324930,
        "entryAddress": 324930,
        "sourcePath": "code/common/stats/iteminventory.asm",
        "entrySymbol": "RemoveItemFromInventory",
    },
}
_SERVICE_INSTRUCTION_SOURCES = {
    "GetItemInventoryLocation": (
        "code/common/tech/jumpinterfaces/s02_jumpinterface.asm",
        "j_GetItemInventoryLocation",
    ),
    "RemoveItemBySlot": (
        "code/common/tech/jumpinterfaces/s02_jumpinterface.asm",
        "j_RemoveItemBySlot",
    ),
}
_SERVICE_SEAM_IDENTITIES = {
    "GetItemInventoryLocation": (33232, 37190),
    "ReceiveMandatoryItem": (324746, 324746),
    "RemoveItemBySlot": (33180, 36470),
    "RemoveItemFromInventory": (324930, 324930),
}
_RETAINED_FIXTURES = (
    ("mapEvents", "sf2-map-events-static-v1", "tests/fixtures/h2/map-events-static-v1.json"),
    (
        "directState",
        "sf2-map-event-direct-state-static-v1",
        "tests/fixtures/h2/map-event-direct-state-static-v1.json",
    ),
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
        "predicateResults",
        "sf2-map-event-predicate-results-static-v1",
        "tests/fixtures/h2/map-event-predicate-results-static-v1.json",
    ),
    (
        "interactionState",
        "sf2-map-event-interaction-state-static-v1",
        "tests/fixtures/h2/map-event-interaction-state-static-v1.json",
    ),
    (
        "dialogueState",
        "sf2-map-event-dialogue-state-static-v1",
        "tests/fixtures/h2/map-event-dialogue-state-static-v1.json",
    ),
    (
        "requestState",
        "sf2-map-event-request-state-static-v1",
        "tests/fixtures/h2/map-event-request-state-static-v1.json",
    ),
    (
        "requestConsumption",
        "sf2-map-event-request-consumption-static-v1",
        "tests/fixtures/h2/map-event-request-consumption-static-v1.json",
    ),
    ("commonStats", "sf2-common-stats-static-v1", "tests/fixtures/h2/common-stats-static-v1.json"),
    ("mapSetup", "sf2-map-setup-static-v1", "tests/fixtures/h2/map-setup-static-v1.json"),
    (
        "techInterfaces",
        "sf2-tech-interfaces-static-v1",
        "tests/fixtures/h2/tech-interfaces-static-v1.json",
    ),
    (
        "fieldMenuControl",
        "sf2-field-menu-control-static-v1",
        "tests/fixtures/h2/field-menu-control-static-v1.json",
    ),
)
_MAP_SETUP_RETAINED_FIXTURE = (
    "sf2-map-setup-static-v1",
    "tests/fixtures/h2/map-setup-static-v1.json",
)
_EQUATE = re.compile(r"^\s*([A-Za-z_][A-Za-z0-9_]*)\s*:\s+equ\s+([^\s;]+)")


def canonical_json_bytes(value: dict[str, Any]) -> bytes:
    """Return deterministic public JSON bytes."""
    return (
        json.dumps(value, ensure_ascii=False, sort_keys=True, separators=(",", ":")).encode("utf-8")
        + b"\n"
    )


def _disasm_root(upstream_path: Path) -> Path:
    root = upstream_path.resolve(strict=True)
    return root / "disasm" if (root / "disasm").is_dir() else root


def _fixture_digest(path: str) -> str:
    return hashlib.sha256(repo_path(path).read_bytes()).hexdigest().upper()


def _parse_item_enums(lines: list[str]) -> dict[str, dict[str, int]]:
    expected = {
        "ITEM_ACHILLES_SWORD": 0x3D,
        "ITEM_WOODEN_PANEL": 0x70,
        "ITEM_CANNON": 0x72,
        "ITEM_DYNAMITE": 0x74,
        "ITEM_ARM_OF_GOLEM": 0x75,
        "ITEM_COTTON_BALLOON": 0x7D,
    }
    occurrences: dict[str, list[dict[str, int | None]]] = {key: [] for key in expected}
    for line_number, line in enumerate(lines, start=1):
        match = _EQUATE.match(line)
        if match is None or match.group(1) not in expected:
            continue
        raw = match.group(2)
        if raw.startswith("$"):
            value = int(raw[1:], 16)
        elif raw.isdigit():
            value = int(raw)
        else:
            value = None
        occurrences[match.group(1)].append({"sourceLine": line_number, "value": value})
    counts = {key: len(rows) for key, rows in occurrences.items()}
    if counts != {key: 1 for key in expected}:
        raise ValueError(f"map-event item transactions item enum drift: occurrences={counts}")
    values = {key: rows[0] for key, rows in occurrences.items()}
    actual = {key: values[key]["value"] for key in expected}
    if actual != expected:
        raise ValueError(f"map-event item transactions item enum drift: {actual}")
    return {
        key: {"sourceLine": values[key]["sourceLine"], "value": expected[key]} for key in expected
    }


def _validate_service_seam_identities() -> None:
    if tuple(_SERVICE_TARGETS) != _SERVICE_ENTRY_IDS:
        raise ValueError("map-event item transactions service entry identity drift")
    for effective_target, service in _SERVICE_TARGETS.items():
        if (
            service["id"] != effective_target
            or (service["instructionTargetAddress"], service["entryAddress"])
            != _SERVICE_SEAM_IDENTITIES[effective_target]
        ):
            raise ValueError(
                "map-event item transactions service alias/effective entry drift: "
                f"{effective_target}"
            )


def _operand_shapes(operation: dict[str, Any]) -> list[str]:
    shapes: list[str] = []
    for operand in operation["operandTexts"]:
        if operand.startswith("#ITEM_"):
            shapes.append("item-immediate")
        elif operand.startswith("#-") or operand.startswith("#0"):
            shapes.append("numeric-immediate")
        elif re.fullmatch(r"[da][0-7]", operand, flags=re.IGNORECASE):
            shapes.append("register")
        elif operand.startswith("#"):
            shapes.append("symbolic-immediate")
        elif operand.startswith("j_"):
            shapes.append("jump-interface")
        elif re.fullmatch(r"[A-Za-z_][A-Za-z0-9_@]*", operand):
            shapes.append("symbol")
        else:
            shapes.append("addressing-mode")
    return shapes


def _source_statement(operation: dict[str, Any]) -> str:
    operands = operation["operandTexts"]
    return _normalise_statement(
        operation["sourceMnemonic"] + (" " + ",".join(operands) if operands else "")
    )


def _anchor(
    operation: dict[str, Any],
    h1_rows: dict[int, tuple[bytes, str]],
    rom: bytes,
    rom_byte_count: int,
) -> dict[str, Any]:
    address = operation["address"]
    row = h1_rows.get(address)
    if row is None:
        raise ValueError(f"map-event item transactions missing H1 instruction: {address:#x}")
    encoded, h1_statement = row
    if rom_byte_count <= 0:
        raise ValueError(f"map-event item transactions source span drift: {address:#x}")
    rom_bytes = rom[address : address + rom_byte_count]
    if len(rom_bytes) != rom_byte_count:
        raise ValueError(f"map-event item transactions ROM boundary drift: {address:#x}")
    return {
        "address": address,
        "mnemonic": operation["mnemonic"],
        "h1ListedByteCount": len(encoded),
        "romEncodedByteCount": rom_byte_count,
        "romInstructionSha256": hashlib.sha256(rom_bytes).hexdigest().upper(),
        "h1StatementSha256": hashlib.sha256(h1_statement.encode("utf-8")).hexdigest().upper(),
    }


def _source_seam_anchor(
    *,
    address: int,
    role: str,
    source_path: str,
    source_line: int,
    source_statement: str,
    source_text: dict[str, list[str]],
    h1_rows: dict[int, tuple[bytes, str]],
    rom: bytes,
    extra: dict[str, Any],
) -> dict[str, Any]:
    """Anchor a non-program source seam to its H1 row and exact ROM bytes."""
    lines = source_text.get(source_path)
    if lines is None or source_line <= 0 or source_line > len(lines):
        raise ValueError(f"map-event item transactions seam source boundary drift: {role}")
    if _normalise_statement(lines[source_line - 1]) != _normalise_statement(source_statement):
        raise ValueError(f"map-event item transactions seam source drift: {role}")
    row = h1_rows.get(address)
    if row is None:
        raise ValueError(f"map-event item transactions seam H1 row missing: {role}")
    encoded, h1_statement = row
    if not encoded:
        raise ValueError(f"map-event item transactions seam H1 width drift: {role}")
    rom_bytes = rom[address : address + len(encoded)]
    if len(rom_bytes) != len(encoded):
        raise ValueError(f"map-event item transactions seam ROM boundary drift: {role}")
    return {
        "address": address,
        "role": role,
        "sourcePath": source_path,
        "sourceLine": source_line,
        "sourceStatementSha256": hashlib.sha256(
            _normalise_statement(source_statement).encode("utf-8")
        )
        .hexdigest()
        .upper(),
        "h1ListedByteCount": len(encoded),
        "h1EncodedSha256": hashlib.sha256(encoded).hexdigest().upper(),
        "romEncodedByteCount": len(encoded),
        "romInstructionSha256": hashlib.sha256(rom_bytes).hexdigest().upper(),
        "h1StatementSha256": hashlib.sha256(h1_statement.encode("utf-8")).hexdigest().upper(),
        **extra,
    }


def _label_line(lines: list[str], symbol: str, source_path: str) -> int:
    matches = [
        line_number
        for line_number, line in enumerate(lines, start=1)
        if re.fullmatch(rf"\s*{re.escape(symbol)}:\s*(?:;.*)?", line)
    ]
    if len(matches) != 1:
        raise ValueError(f"map-event item transactions source label drift: {source_path}:{symbol}")
    return matches[0]


def _find_program(map_events: dict[str, Any], category: str, symbol: str) -> dict[str, Any]:
    field = {
        "entityEvents": "entityTargetPrograms",
        "zoneEvents": "zoneTargetPrograms",
        "itemEvents": "itemTargetPrograms",
    }[category]
    matches = [row for row in map_events[field] if row["canonicalSymbol"] == symbol]
    if len(matches) != 1:
        raise ValueError(f"map-event item transactions program selection drift: {symbol}")
    return matches[0]


def _source_table_rows(map_events: dict[str, Any]) -> dict[tuple[str, str], dict[str, Any]]:
    rows: dict[tuple[str, str], dict[str, Any]] = {}
    for category in ("entityEvents", "zoneEvents", "itemEvents"):
        for row in map_events["categories"][category]["sourceFiles"]:
            rows[(category, row["path"])] = row
    return rows


def _program_slice(
    program: dict[str, Any], start_symbol: str | None
) -> tuple[list[dict[str, Any]], list[dict[str, Any]], int]:
    operations = program["operations"]
    labels = program["labels"]
    if start_symbol is None:
        return operations, labels, program["entryAddress"]
    starts = [label for label in labels if label["symbol"] == start_symbol]
    if len(starts) != 1:
        raise ValueError(f"map-event item transactions nested start drift: {start_symbol}")
    start_address = starts[0]["address"]
    return (
        [row for row in operations if row["address"] >= start_address],
        [row for row in labels if row["address"] >= start_address],
        start_address,
    )


def _item_setup(
    operation: dict[str, Any], enums: dict[str, dict[str, int]]
) -> dict[str, Any] | None:
    operands = operation["operandTexts"]
    if len(operands) != 2 or not operands[0].startswith("#ITEM_"):
        return None
    symbol = operands[0][1:]
    if symbol not in enums:
        return None
    return {
        "itemSymbol": symbol,
        "itemValue": enums[symbol]["value"],
        "register": operands[1],
        "sourceLine": operation["sourceLine"],
        "address": operation["address"],
    }


def _retained_owners(map_events: dict[str, Any], *, check_manifest: bool = True) -> dict[str, Any]:
    map_setup = next(
        (fixture_id, path) for name, fixture_id, path in _RETAINED_FIXTURES if name == "mapSetup"
    )
    if map_setup != _MAP_SETUP_RETAINED_FIXTURE:
        raise ValueError("map-event item transactions retained mapSetup owner drift")
    fixture = load_map_events_fixture()
    if fixture["expected"] != map_events:
        raise ValueError("map-event item transactions retained map-events fixture drift")
    digest = hashlib.sha256(_map_events_canonical_bytes(map_events)).hexdigest().upper()
    if check_manifest:
        manifest = load_json(MAP_EVENTS_MANIFEST)
        if digest != manifest["outputSha256"] or map_events["summary"] != manifest["summary"]:
            raise ValueError("map-event item transactions retained map-events digest drift")
    return {
        name: {"fixtureId": fixture_id, "fixtureSha256": _fixture_digest(path)}
        for name, fixture_id, path in _RETAINED_FIXTURES
    }


def _validate_order(value: dict[str, Any]) -> None:
    expected_root = [
        "schemaVersion",
        "id",
        "upstream",
        "romSha256",
        "retainedOwners",
        "sourceContext",
        "eventItemTransactions",
        "unknowns",
        "summary",
    ]
    if list(value) != expected_root:
        raise ValueError("map-event item transactions root field order drift")
    transactions = value["eventItemTransactions"]
    expected = [
        "sourceFileOrder",
        "sourceFiles",
        "programContextOrder",
        "programContexts",
        "physicalProgramOrder",
        "physicalPrograms",
        "serviceEntryOrder",
        "serviceEntries",
        "serviceCallOrder",
        "serviceCalls",
        "resultPredicateOrder",
        "resultPredicates",
        "transactionChainOrder",
        "transactionChains",
        "itemEventReturnHandoff",
        "digests",
    ]
    if list(transactions) != expected:
        raise ValueError("map-event item transactions field order drift")
    for records, order in (
        ("sourceFiles", "sourceFileOrder"),
        ("programContexts", "programContextOrder"),
        ("physicalPrograms", "physicalProgramOrder"),
        ("serviceEntries", "serviceEntryOrder"),
        ("serviceCalls", "serviceCallOrder"),
        ("resultPredicates", "resultPredicateOrder"),
        ("transactionChains", "transactionChainOrder"),
    ):
        if list(transactions[records]) != transactions[order]:
            raise ValueError(f"map-event item transactions record order drift: {records}")
    expected_orders = {
        "sourceFileOrder": _SOURCE_FILE_IDS,
        "programContextOrder": _PROGRAM_CONTEXT_IDS,
        "physicalProgramOrder": _PHYSICAL_PROGRAM_IDS,
        "serviceEntryOrder": _SERVICE_ENTRY_IDS,
        "transactionChainOrder": _PROGRAM_CONTEXT_IDS,
    }
    for field, expected_order in expected_orders.items():
        if tuple(transactions[field]) != expected_order:
            raise ValueError(f"map-event item transactions declared order drift: {field}")
    source_context = value["sourceContext"]
    if tuple(item["path"] for item in source_context["sourceIdentities"]) != _SOURCE_PATHS:
        raise ValueError("map-event item transactions source identity order drift")
    physical_anchors = source_context["physicalOperationAnchors"]
    if [item["address"] for item in physical_anchors] != sorted(
        item["address"] for item in physical_anchors
    ):
        raise ValueError("map-event item transactions physical anchor order drift")
    if (
        tuple(item["sourceFileId"] for item in source_context["eventTableAnchors"])
        != _SOURCE_FILE_IDS
    ):
        raise ValueError("map-event item transactions table anchor order drift")


def build_map_event_item_transactions_contract(
    rom_path: Path,
    upstream_path: Path,
    *,
    map_events_override: dict[str, Any] | None = None,
) -> dict[str, Any]:
    """Build the item-service caller choreography from the retained source corpus."""
    rom = rom_path.resolve(strict=True).read_bytes()
    if hashlib.sha256(rom).hexdigest().upper() != _ROM_SHA256:
        raise ValueError("map-event item transactions ROM identity drift")
    map_events = (
        build_map_events_contract(rom_path, upstream_path)
        if map_events_override is None
        else deepcopy(map_events_override)
    )
    retained = _retained_owners(map_events, check_manifest=map_events_override is None)
    _validate_service_seam_identities()
    root = _disasm_root(upstream_path)
    source_text = {
        path: (root / path).read_text(encoding="utf-8").splitlines() for path in _SOURCE_PATHS
    }
    enums = _parse_item_enums(source_text["sf2enums.asm"])
    listing_text = (upstream_path.resolve(strict=True) / "build/sf2build-h1.lst").read_text(
        encoding="utf-8"
    )
    h1_rows = _h1_instruction_rows(listing_text)
    table_rows = _source_table_rows(map_events)
    table_records = {
        (category, table["symbol"]): table["records"][0]
        for category in ("entityEvents", "zoneEvents", "itemEvents")
        for table in map_events["categories"][category]["tables"]
        if len(table["records"]) > 0
    }

    source_files: dict[str, Any] = {}
    program_contexts: dict[str, Any] = {}
    physical_programs: dict[str, Any] = {}
    context_operations: dict[str, list[dict[str, Any]]] = {}
    physical_by_address: dict[int, dict[str, Any]] = {}
    for context_order, (context_id, category, symbol, nested_start) in enumerate(_PROGRAM_SPECS):
        program = _find_program(map_events, category, symbol)
        operations, labels, entry_address = _program_slice(program, nested_start)
        if [row["address"] for row in operations] != sorted(row["address"] for row in operations):
            raise ValueError(f"map-event item transactions operation order drift: {context_id}")
        if [row["address"] for row in labels] != sorted(row["address"] for row in labels):
            raise ValueError(f"map-event item transactions label order drift: {context_id}")
        table = table_rows[(category, program["sourcePath"])]
        source_id = table["symbol"]
        source_files.setdefault(
            source_id,
            {
                "category": category,
                "sourcePath": program["sourcePath"],
                "tableEntryAddress": table["address"],
                "tableSymbol": table["symbol"],
            },
        )
        physical_id = (
            "map6-entity-event13" if context_id == "map6-default-entity-event" else context_id
        )
        if physical_id not in physical_programs:
            physical_programs[physical_id] = {
                "sourcePath": program["sourcePath"],
                "programSymbol": symbol,
                "entryAddress": program["entryAddress"],
                "endAddressExclusive": program["endAddressExclusive"],
                "operationAddresses": [row["address"] for row in program["operations"]],
                "labelAddresses": [row["address"] for row in program["labels"]],
            }
        operation_rows: list[dict[str, Any]] = []
        for operation_order, operation in enumerate(operations):
            source_line = operation["sourceLine"]
            statement = _source_statement(operation)
            if (
                _normalise_statement(source_text[program["sourcePath"]][source_line - 1])
                != statement
            ):
                raise ValueError(
                    "map-event item transactions source operation drift: "
                    f"{context_id}:{source_line}"
                )
            operation_id = f"{physical_id}:{operation['address']:06X}"
            target = operation["target"]
            public_target = (
                None
                if target is None
                else {
                    "instructionTarget": target["instructionTargetSymbol"],
                    "instructionTargetAddress": target["instructionTargetAddress"],
                    "effectiveTarget": target["effectiveTargetSymbol"],
                    "effectiveTargetAddress": target["effectiveTargetAddress"],
                    "scope": target["effectiveTargetScope"],
                }
            )
            row = {
                "operationOrder": operation_order,
                "operationId": operation_id,
                "sourceLine": source_line,
                "address": operation["address"],
                "mnemonic": operation["mnemonic"],
                "sizeSuffix": operation["sizeSuffix"],
                "operandShapes": _operand_shapes(operation),
                "controlFlowKind": operation["controlFlowKind"],
                "target": public_target,
            }
            operation_rows.append(row)
            physical_by_address.setdefault(operation["address"], operation)
        program_contexts[context_id] = {
            "contextOrder": context_order,
            "category": category,
            "sourceFileId": source_id,
            "programSymbol": nested_start or symbol,
            "physicalProgramId": physical_id,
            "entryAddress": entry_address,
            "endAddressExclusive": program["endAddressExclusive"],
            "operationIds": [row["operationId"] for row in operation_rows],
            "labelAddresses": [row["address"] for row in labels],
            "operations": operation_rows,
        }
        context_operations[context_id] = operations

    physical_sizes: dict[int, int] = {}
    for physical in physical_programs.values():
        addresses = physical["operationAddresses"]
        for index, address in enumerate(addresses):
            next_address = (
                addresses[index + 1]
                if index + 1 < len(addresses)
                else physical["endAddressExclusive"]
            )
            if address in physical_sizes and physical_sizes[address] != next_address - address:
                raise ValueError("map-event item transactions physical operation span ambiguity")
            physical_sizes[address] = next_address - address
    physical_anchors = [
        _anchor(operation, h1_rows, rom, physical_sizes[operation["address"]])
        for _address, operation in sorted(physical_by_address.items())
    ]
    if len(physical_anchors) != 150:
        raise ValueError(
            "map-event item transactions physical operation denominator drift: "
            f"{len(physical_anchors)}"
        )

    service_calls: dict[str, Any] = {}
    predicates: dict[str, Any] = {}
    chains: dict[str, Any] = {}
    for context_id, operations in context_operations.items():
        chain_ids: list[str] = []
        last_item: dict[str, Any] | None = None
        for index, operation in enumerate(operations):
            setup = _item_setup(operation, enums)
            if setup is not None:
                last_item = setup
            target = operation["target"]
            if target is None or target["effectiveTargetSymbol"] not in _SERVICE_TARGETS:
                continue
            service = _SERVICE_TARGETS[target["effectiveTargetSymbol"]]
            call_id = f"{context_id}:{service['id']}:{operation['address']:06X}"
            if last_item is None:
                raise ValueError(f"map-event item transactions missing item setup: {call_id}")
            service_calls[call_id] = {
                "serviceCallOrder": len(service_calls),
                "contextId": context_id,
                "physicalProgramId": program_contexts[context_id]["physicalProgramId"],
                "serviceEntryId": service["id"],
                "sourceLine": operation["sourceLine"],
                "callAddress": operation["address"],
                "instructionTarget": target["instructionTargetSymbol"],
                "instructionTargetAddress": target["instructionTargetAddress"],
                "effectiveTarget": target["effectiveTargetSymbol"],
                "effectiveTargetAddress": target["effectiveTargetAddress"],
                "item": last_item,
            }
            chain_ids.append(call_id)
            if service["id"] in {"GetItemInventoryLocation", "ReceiveMandatoryItem"}:
                producer = operations[index + 1] if index + 1 < len(operations) else None
                branch = operations[index + 2] if index + 2 < len(operations) else None
                expected = (
                    ("cmpi", "inventory-location")
                    if service["id"] == "GetItemInventoryLocation"
                    else ("btst", "mandatory-receive")
                )
                allowed_branches = (
                    {"bne", "beq"} if service["id"] == "GetItemInventoryLocation" else {"bne"}
                )
                if (
                    producer is None
                    or branch is None
                    or producer["mnemonic"] != expected[0]
                    or branch["mnemonic"] not in allowed_branches
                    or branch["target"] is None
                ):
                    raise ValueError(
                        f"map-event item transactions result predicate shape drift: {call_id}"
                    )
                if service["id"] == "GetItemInventoryLocation" and producer["operandTexts"] != [
                    "#-1",
                    "d0",
                ]:
                    raise ValueError(
                        f"map-event item transactions location sentinel drift: {call_id}"
                    )
                if service["id"] == "ReceiveMandatoryItem" and producer["operandTexts"] != [
                    "#0",
                    "d0",
                ]:
                    raise ValueError(f"map-event item transactions mandatory bit drift: {call_id}")
                encoded, _ = h1_rows[branch["address"]]
                predicates[f"{call_id}:predicate"] = {
                    "predicateOrder": len(predicates),
                    "serviceCallId": call_id,
                    "predicateKind": expected[1],
                    "producerAddress": producer["address"],
                    "producerMnemonic": producer["mnemonic"],
                    "branchAddress": branch["address"],
                    "branchMnemonic": branch["mnemonic"],
                    "targetAddress": branch["target"]["effectiveTargetAddress"],
                    "fallthroughAddress": branch["address"] + len(encoded),
                }
        if chain_ids:
            chains[context_id] = {
                "contextId": context_id,
                "serviceCallIds": chain_ids,
                "serviceEntryIds": [
                    service_calls[call_id]["serviceEntryId"] for call_id in chain_ids
                ],
            }

    service_entries: dict[str, Any] = {}
    for _effective_target, service in _SERVICE_TARGETS.items():
        service_entries[service["id"]] = {"entryOrder": len(service_entries), **service}

    d6_specs = {
        "map22-item-event0": ((366122, "moveq", -1),),
        "map63-item-event0": ((379462, "move", -1), (379468, "clr", 0)),
        "map8-item-event0": ((353016, "moveq", 0), (353038, "move", -1)),
    }
    d6_writes: list[dict[str, Any]] = []
    for context_id, expected_writes in d6_specs.items():
        found = [
            row
            for row in context_operations[context_id]
            if row["operandTexts"] and row["operandTexts"][-1] == "d6"
        ]
        actual = tuple(
            (
                row["address"],
                row["mnemonic"],
                0 if row["mnemonic"] == "clr" else -1 if "#-1" in row["operandTexts"] else 0,
            )
            for row in found
        )
        if actual != expected_writes:
            raise ValueError(f"map-event item transactions d6 write drift: {context_id}")
        d6_writes.extend(
            {"contextId": context_id, "address": address, "mnemonic": mnemonic, "value": value}
            for address, mnemonic, value in actual
        )
    if len(d6_writes) != 5:
        raise ValueError("map-event item transactions d6 write denominator drift")

    main_lines = source_text["code/common/menus/main/mainactions.asm"]
    for line, expected in ((253, "jsr j_RunMapSetupItemEvent"), (254, "tst.w d6")):
        if _normalise_statement(main_lines[line - 1]) != expected:
            raise ValueError("map-event item transactions FieldMenu source seam drift")
    call_encoded, _ = h1_rows[136490]
    test_encoded, _ = h1_rows[136496]
    if not call_encoded or not test_encoded or not rom[136490:136492] or not rom[136496:136498]:
        raise ValueError("map-event item transactions FieldMenu ROM seam drift")
    item_event_return_handoff = {
        "callAddress": 136490,
        "callInstructionTarget": "j_RunMapSetupItemEvent",
        "callInstructionTargetAddress": 278664,
        "runMapSetupItemEventEntryAddress": 292230,
        "resultTestAddress": 136496,
        "resultTestMnemonic": "tst",
        "resultRegister": "d6",
        "d6Writes": d6_writes,
    }

    # Source table anchors, service aliases/effective entries, and caller-handoff seams.
    table_anchors: list[dict[str, Any]] = []
    for source_id in source_files:
        row = source_files[source_id]
        table_record = table_records.get((row["category"], row["tableSymbol"]))
        if table_record is None or table_record["address"] != row["tableEntryAddress"]:
            raise ValueError(
                f"map-event item transactions table source relation drift: {source_id}"
            )
        table_statement = table_record["macro"] + (
            " " + ",".join(table_record["operandTexts"]) if table_record["operandTexts"] else ""
        )
        table_anchors.append(
            _source_seam_anchor(
                address=row["tableEntryAddress"],
                role="event-table-entry",
                source_path=row["sourcePath"],
                source_line=table_record["sourceLine"],
                source_statement=table_statement,
                source_text=source_text,
                h1_rows=h1_rows,
                rom=rom,
                extra={"sourceFileId": source_id},
            )
        )
    seams: list[dict[str, Any]] = []
    for entry in service_entries.values():
        for role, address in (
            ("instruction-target", entry["instructionTargetAddress"]),
            ("effective-entry", entry["entryAddress"]),
        ):
            if address not in {item["address"] for item in seams}:
                if role == "instruction-target":
                    source_path, symbol = _SERVICE_INSTRUCTION_SOURCES.get(
                        entry["id"], (entry["sourcePath"], entry["instructionTarget"])
                    )
                else:
                    source_path, symbol = entry["sourcePath"], entry["entrySymbol"]
                seams.append(
                    _source_seam_anchor(
                        address=address,
                        role=f"service-{role}",
                        source_path=source_path,
                        source_line=_label_line(source_text[source_path], symbol, source_path),
                        source_statement=f"{symbol}:",
                        source_text=source_text,
                        h1_rows=h1_rows,
                        rom=rom,
                        extra={"serviceEntryId": entry["id"]},
                    )
                )
    handoff_anchors = [
        _source_seam_anchor(
            address=136490,
            role="field-menu-call",
            source_path="code/common/menus/main/mainactions.asm",
            source_line=253,
            source_statement="jsr j_RunMapSetupItemEvent",
            source_text=source_text,
            h1_rows=h1_rows,
            rom=rom,
            extra={},
        ),
        _source_seam_anchor(
            address=278664,
            role="jump-interface-item-entry",
            source_path="code/common/tech/jumpinterfaces/s07_jumpinterface.asm",
            source_line=_label_line(
                source_text["code/common/tech/jumpinterfaces/s07_jumpinterface.asm"],
                "j_RunMapSetupItemEvent",
                "code/common/tech/jumpinterfaces/s07_jumpinterface.asm",
            ),
            source_statement="j_RunMapSetupItemEvent:",
            source_text=source_text,
            h1_rows=h1_rows,
            rom=rom,
            extra={},
        ),
        _source_seam_anchor(
            address=292230,
            role="map-setup-item-entry",
            source_path="code/common/scripting/map/mapsetupsfunctions_1.asm",
            source_line=_label_line(
                source_text["code/common/scripting/map/mapsetupsfunctions_1.asm"],
                "RunMapSetupItemEvent",
                "code/common/scripting/map/mapsetupsfunctions_1.asm",
            ),
            source_statement="RunMapSetupItemEvent:",
            source_text=source_text,
            h1_rows=h1_rows,
            rom=rom,
            extra={},
        ),
        _source_seam_anchor(
            address=136496,
            role="field-menu-result-test",
            source_path="code/common/menus/main/mainactions.asm",
            source_line=254,
            source_statement="tst.w d6",
            source_text=source_text,
            h1_rows=h1_rows,
            rom=rom,
            extra={},
        ),
    ]
    source_context = {
        "h1Listing": {
            "path": "build/sf2build-h1.lst",
            "sha256": hashlib.sha256(listing_text.encode("utf-8")).hexdigest().upper(),
        },
        "sourceIdentities": [
            {"path": path, "sha256": hashlib.sha256((root / path).read_bytes()).hexdigest().upper()}
            for path in _SOURCE_PATHS
        ],
        "physicalOperationAnchors": physical_anchors,
        "eventTableAnchors": table_anchors,
        "serviceSeamAnchors": seams,
        "handoffAnchors": handoff_anchors,
    }
    if (
        len(table_anchors) != 7
        or len(seams) != 6
        or len(handoff_anchors) != 4
        or 150 + len(table_anchors) + len(seams) + len(handoff_anchors) != 167
    ):
        raise ValueError("map-event item transactions anchor denominator drift")

    physical_ops = list(physical_by_address.values())
    counts = Counter(op["family"] for op in physical_ops)
    controls = Counter(op["controlFlowKind"] for op in physical_ops)
    summary = {
        "sourceIdentityCount": len(_SOURCE_PATHS),
        "programContextCount": len(program_contexts),
        "physicalProgramCount": len(physical_programs),
        "contextOperationCount": sum(
            len(value["operations"]) for value in program_contexts.values()
        ),
        "physicalOperationCount": len(physical_ops),
        "contextLabelCount": sum(
            len(value["labelAddresses"]) for value in program_contexts.values()
        ),
        "physicalLabelCount": len(
            {label for value in physical_programs.values() for label in value["labelAddresses"]}
        ),
        "contextEncodedByteCount": sum(
            value["endAddressExclusive"] - value["entryAddress"]
            for value in program_contexts.values()
        ),
        "physicalEncodedByteCount": sum(
            value["endAddressExclusive"] - value["entryAddress"]
            for value in physical_programs.values()
        ),
        "eventServiceMacroPhysicalOperationCount": counts["event-service-macro"],
        "rawInstructionPhysicalOperationCount": counts["raw-68000-instruction"],
        "rawControlPhysicalOperationCount": counts["raw-68000-control-flow"],
        "ordinaryPhysicalControlCount": controls["ordinary"],
        "conditionalPhysicalControlCount": controls["conditional-branch"],
        "unconditionalPhysicalControlCount": controls["unconditional-branch"],
        "directCallPhysicalControlCount": controls["direct-call"],
        "returnPhysicalControlCount": controls["return"],
        "contextServiceCallCount": len(service_calls),
        "physicalServiceCallCount": len({row["callAddress"] for row in service_calls.values()}),
        "contextPredicateCount": len(predicates),
        "physicalPredicateCount": len({row["branchAddress"] for row in predicates.values()}),
        "transactionChainCount": len(chains),
        "d6WriteCount": len(d6_writes),
        "anchorCount": 167,
    }
    expected_summary = {
        "sourceIdentityCount": 16,
        "programContextCount": 8,
        "physicalProgramCount": 7,
        "contextOperationCount": 190,
        "physicalOperationCount": 150,
        "contextLabelCount": 42,
        "physicalLabelCount": 34,
        "contextEncodedByteCount": 708,
        "physicalEncodedByteCount": 558,
        "eventServiceMacroPhysicalOperationCount": 53,
        "rawInstructionPhysicalOperationCount": 37,
        "rawControlPhysicalOperationCount": 60,
        "ordinaryPhysicalControlCount": 90,
        "conditionalPhysicalControlCount": 20,
        "unconditionalPhysicalControlCount": 12,
        "directCallPhysicalControlCount": 20,
        "returnPhysicalControlCount": 8,
        "contextServiceCallCount": 15,
        "physicalServiceCallCount": 13,
        "contextPredicateCount": 9,
        "physicalPredicateCount": 7,
        "transactionChainCount": 8,
        "d6WriteCount": 5,
        "anchorCount": 167,
    }
    if summary != expected_summary:
        raise ValueError(f"map-event item transactions denominator drift: {summary}")

    transactions = {
        "sourceFileOrder": list(source_files),
        "sourceFiles": source_files,
        "programContextOrder": list(program_contexts),
        "programContexts": program_contexts,
        "physicalProgramOrder": list(physical_programs),
        "physicalPrograms": physical_programs,
        "serviceEntryOrder": list(service_entries),
        "serviceEntries": service_entries,
        "serviceCallOrder": list(service_calls),
        "serviceCalls": service_calls,
        "resultPredicateOrder": list(predicates),
        "resultPredicates": predicates,
        "transactionChainOrder": list(chains),
        "transactionChains": chains,
        "itemEventReturnHandoff": item_event_return_handoff,
        "digests": {
            "programContextsSha256": hashlib.sha256(
                canonical_json_bytes({"programContexts": program_contexts})
            )
            .hexdigest()
            .upper(),
            "serviceCallsSha256": hashlib.sha256(
                canonical_json_bytes({"serviceCalls": service_calls})
            )
            .hexdigest()
            .upper(),
            "resultPredicatesSha256": hashlib.sha256(
                canonical_json_bytes({"resultPredicates": predicates})
            )
            .hexdigest()
            .upper(),
            "transactionChainsSha256": hashlib.sha256(
                canonical_json_bytes({"transactionChains": chains})
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
        "sourceContext": source_context,
        "eventItemTransactions": transactions,
        "unknowns": {key: "Unknown" for key in _UNKNOWN_KEYS},
        "summary": summary,
    }
    _validate_order(output)
    return output


def verify_map_event_item_transactions_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner=str(FIXTURE))
    _validate_order(fixture)
    output = build_map_event_item_transactions_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="map-event item transactions static contract")
    _validate_order(output)
    if fixture != output:
        raise ValueError("map-event item transactions complete semantic fixture drift")
    destination = output_path or repo_path("local/derived/map-event-item-transactions-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(
        json.dumps(output, ensure_ascii=False, indent=2).encode("utf-8") + b"\n"
    )
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": hashlib.sha256(canonical_json_bytes(output)).hexdigest().upper(),
        "ServiceCalls": output["summary"]["contextServiceCallCount"],
        "Status": "PASS",
    }


_INDEX_DOCUMENT = "docs/research/map-event-item-transactions.md"
_INDEX_FIXTURE = "tests/fixtures/h2/map-event-item-transactions-static-v1.json"
_INDEX_DELTA = {
    "map.data.ms-map6-flag701-entityevents": (
        (
            "entry",
            "eventItemTransactions.sourceFiles.ms_map6_flag701_EntityEvents.tableEntryAddress",
        ),
    ),
    "map.data.ms-map8-section5": (
        ("entry", "eventItemTransactions.sourceFiles.ms_map8_Section5.tableEntryAddress"),
    ),
    "map.data.ms-map9-entityevents": (
        ("entry", "eventItemTransactions.sourceFiles.ms_map9_EntityEvents.tableEntryAddress"),
    ),
    "map.data.ms-map22-section5": (
        ("entry", "eventItemTransactions.sourceFiles.ms_map22_Section5.tableEntryAddress"),
    ),
    "map.data.ms-map63-entityevents": (
        ("entry", "eventItemTransactions.sourceFiles.ms_map63_EntityEvents.tableEntryAddress"),
    ),
    "map.data.ms-map63-section5": (
        ("entry", "eventItemTransactions.sourceFiles.ms_map63_Section5.tableEntryAddress"),
    ),
    "map.data.ms-map72-zoneevents": (
        ("entry", "eventItemTransactions.sourceFiles.ms_map72_ZoneEvents.tableEntryAddress"),
    ),
    "stats.item-stats": (
        (
            "get-item-inventory-location",
            "eventItemTransactions.serviceEntries.GetItemInventoryLocation.entryAddress",
        ),
        (
            "remove-item-by-slot",
            "eventItemTransactions.serviceEntries.RemoveItemBySlot.entryAddress",
        ),
    ),
    "stats.item-inventory": (
        ("entry", "eventItemTransactions.serviceEntries.ReceiveMandatoryItem.entryAddress"),
        (
            "remove-item-from-inventory",
            "eventItemTransactions.serviceEntries.RemoveItemFromInventory.entryAddress",
        ),
    ),
    "map.setup.item-event": (
        ("entry", "eventItemTransactions.itemEventReturnHandoff.runMapSetupItemEventEntryAddress"),
    ),
    "menus.field-main": (
        (
            "run-map-setup-item-event-call",
            "eventItemTransactions.itemEventReturnHandoff.callAddress",
        ),
        (
            "run-map-setup-item-event-result-test",
            "eventItemTransactions.itemEventReturnHandoff.resultTestAddress",
        ),
    ),
}
_INDEX_ADDRESS_DELTA = {
    "stats.item-stats": (
        ("get-item-inventory-location", 37190),
        ("remove-item-by-slot", 36470),
    ),
    "stats.item-inventory": (("remove-item-from-inventory", 324930),),
    "menus.field-main": (("run-map-setup-item-event-result-test", 136496),),
}
_INDEX_RECORD_COUNT = 1625
_INDEX_RECORD_IDS_SHA256 = "684FF6E743B5D197561845C05516F28770E5E362EE775F560E934F9F134D54D5"
_INDEX_PREDECESSOR_SHA256 = "09E54BB6001CFAB23FE3DD034807B4F76EC961931ACEA97F4177F30F96BDE360"
_INDEX_PREDECESSOR_RECORD_SHA256 = {
    "map.data.ms-map6-flag701-entityevents": (
        "DBD2E53C15C64E0BE64C69C89CF14355E83583884F64E859D4F3253CBC1B4730"
    ),
    "map.data.ms-map8-section5": "D15DE5234B1A7B1E854C8125131A442441DD9DC07B05BD981F66AFF8ABB9C6C5",
    "map.data.ms-map9-entityevents": (
        "9827EA287F76176587F4553A670DE0969059AAC4043D7CB8AA6F9B74AED64A3C"
    ),
    "map.data.ms-map22-section5": (
        "69BBC49BE466DCBDE0F0B68ABC6F6DB05CCB8D4192CE93A8F39BC9FAD93A0ADC"
    ),
    "map.data.ms-map63-entityevents": (
        "C357012D407163A0B8B7E7D44D5E0A1B483A34AB43BE96DA9BAE500C7E5C631E"
    ),
    "map.data.ms-map63-section5": (
        "F6BE593B4A13AA1D5E03DE6DBC00AA5993278830724A61E04F0A77F4CACCBA7E"
    ),
    "map.data.ms-map72-zoneevents": (
        "E15B5ED67FE76CA552919D2BB23D0E63DEF44B0D5AA37F3180CEAE639CDFA320"
    ),
    "stats.item-stats": "85A5AABDD4A1A8DA8847C29B15EF1AF24A883F38B45A6D54A70A889EAD34677F",
    "stats.item-inventory": "94EEEE25E36C1D5F829E52A3F0F5D1513DA30B75468649F4A683AD41958931C3",
    "map.setup.item-event": "B5034345FCF1E91B3DD48D1697286B5A611BE10AA9A22F50EA4545E5BB6FDB22",
    "menus.field-main": "338DBF4940ABC70E5F86EA515B494FC6EE5915E849CFF0F09576176E4C7C6E60",
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


def _remove_map_event_item_transactions_index_delta(index: dict[str, Any]) -> dict[str, Any]:
    """Prove and remove exactly this owner delta without touching its predecessor."""
    normalized = deepcopy(index)
    records = normalized.get("records")
    if not isinstance(records, list) or len({record.get("id") for record in records}) != len(
        records
    ):
        raise ValueError("map-event item transactions later-owner record identity drift")
    record_ids_digest = (
        hashlib.sha256(
            json.dumps([record["id"] for record in records], separators=(",", ":")).encode("utf-8")
        )
        .hexdigest()
        .upper()
    )
    if len(records) != _INDEX_RECORD_COUNT or record_ids_digest != _INDEX_RECORD_IDS_SHA256:
        raise ValueError("map-event item transactions later-owner record set/order drift")
    seen: set[str] = set()
    address_count = 0
    binding_count = 0
    for record in records:
        evidence = [item for item in record.get("evidence", []) if item.get("fixtureId") == ID]
        if not evidence:
            continue
        expected_bindings = [
            {"addressId": address_id, "fixtureField": field}
            for address_id, field in _INDEX_DELTA.get(record["id"], ())
        ]
        expected = {
            "level": "H2",
            "fixture": _INDEX_FIXTURE,
            "fixtureId": ID,
            "verifier": "src/sf2tool/h2/map_event_item_transactions.py",
            "bindings": expected_bindings,
        }
        if (
            record["id"] not in _INDEX_DELTA
            or evidence != [expected]
            or record["documents"].count(_INDEX_DOCUMENT) != 1
            or record["documents"][-1] != _INDEX_DOCUMENT
        ):
            raise ValueError("map-event item transactions later-owner evidence/document drift")
        binding_count += len(expected_bindings)
        expected_addresses = {
            address_id: {
                "id": address_id,
                "space": "rom",
                "kind": "observation",
                "value": value,
            }
            for address_id, value in _INDEX_ADDRESS_DELTA.get(record["id"], ())
        }
        for address_id, _field in _INDEX_DELTA[record["id"]]:
            matches = [row for row in record["addresses"] if row["id"] == address_id]
            if len(matches) != 1:
                raise ValueError("map-event item transactions later-owner address identity drift")
            if address_id in expected_addresses:
                if matches[0] != expected_addresses[address_id]:
                    raise ValueError("map-event item transactions later-owner address object drift")
                record["addresses"].remove(matches[0])
                address_count += 1
        record["evidence"].remove(evidence[0])
        record["documents"].remove(_INDEX_DOCUMENT)
        if _index_record_digest(record) != _INDEX_PREDECESSOR_RECORD_SHA256[record["id"]]:
            raise ValueError("map-event item transactions later-owner unrelated object drift")
        seen.add(record["id"])
    if seen != set(_INDEX_DELTA) or address_count != 4 or binding_count != 14:
        raise ValueError("map-event item transactions later-owner coverage drift")
    if (
        hashlib.sha256(canonical_json_bytes(normalized)).hexdigest().upper()
        != _INDEX_PREDECESSOR_SHA256
    ):
        raise ValueError("map-event item transactions later-owner unrelated index drift")
    return normalized


def normalize_map_event_item_transactions_later_owner_index(
    index: dict[str, Any],
) -> dict[str, Any]:
    """Strictly normalize the current index through this owner's predecessor."""
    from sf2tool.research_index import normalize_current_index_to_owner_predecessor

    return normalize_current_index_to_owner_predecessor(
        index, owner_id=ID
    )
