"""Static map-event flag candidate topology through map-setup selection.

This H2 rail deliberately joins accepted public owner projections.  It proves
only source-shaped program-to-record, record-to-pointer, and pointer-to-map
selector relationships; it does not evaluate a selector or assert execution.
"""

from __future__ import annotations

import json
import re
from collections import Counter, defaultdict
from copy import deepcopy
from pathlib import Path
from typing import Any

from sf2tool.h2.map_event_cross_program_flag_state import (
    FIXTURE as CROSS_FIXTURE,
)
from sf2tool.h2.map_event_cross_program_flag_state import (
    _h1_instruction_rows,
    _root,
    _sha,
    build_map_event_cross_program_flag_state_contract,
    canonical_json_bytes,
    normalize_map_event_cross_program_flag_state_later_owner_index,
)
from sf2tool.h2.map_setup import build_map_setup_contract
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path

ID = "sf2-map-event-flag-route-selection-static-v1"
FIXTURE = repo_path("tests/fixtures/h2/map-event-flag-route-selection-static-v1.json")
SCHEMA = repo_path("schemas/h2/map-event-flag-route-selection-static-fixture.schema.json")
RESEARCH_INDEX = repo_path("manifests/research-index.json")
ROUTING_FIXTURE = repo_path("tests/fixtures/h2/map-events/routing-setup.json")
ROUTING_SCHEMA = repo_path("schemas/h2/map-events-routing-setup.schema.json")
MAP_SETUP_FIXTURE = repo_path("tests/fixtures/h2/map-setup-static-v1.json")
MAP_SETUP_SCHEMA = repo_path("schemas/h2-map-setup-static-fixture.schema.json")

_UPSTREAM_COMMIT = "c834c652b6862bc5679fd7f69a38a7093206efc6"
_ROM_SHA256 = "9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9"
_CATEGORIES = ("entityEvents", "zoneEvents", "itemEvents")
_OFFSETS = {"entityEvents": 4, "zoneEvents": 8, "itemEvents": 16}
_RECORD_WIDTHS = {"entityEvents": 4, "zoneEvents": 4, "itemEvents": 6}
_UNKNOWN_KEYS = (
    "naturalProgramReachability",
    "callerEntryFlagState",
    "actualFlagValueAtRead",
    "actualConditionalBranchSelection",
    "actualMutationReachability",
    "runtimeFlagValueAfterMutation",
    "producerConsumerTemporalOrder",
    "interveningFlagMutations",
    "actualMapSetupSelectorEvaluation",
    "actualSelectedPointerTable",
    "actualSelectedEventRecord",
    "saveLoadAndCrossMapPersistence",
    "calleeScriptAndServiceEffects",
    "dialogueAudioPresentationAndStoryMeaning",
)
_TOPOLOGY_ORDER = (
    "sameEventTable",
    "sameSelectedSetupDifferentEventTable",
    "sameMapDifferentSelector",
    "crossMapOnly",
)
_EXPECTED_TOPOLOGY = {
    "sameEventTable": 20,
    "sameSelectedSetupDifferentEventTable": 54,
    "sameMapDifferentSelector": 11,
    "crossMapOnly": 635,
}
_RETAINED_OWNER_EXPECTED = {
    "crossProgramFlagState": {
        "fixtureId": "sf2-map-event-cross-program-flag-state-static-v1",
        "fixtureSha256": "BA52F032344E9C0B7E240B44DACB419BFB0150726C1620086460E595FA283767",
        "verifierPath": "src/sf2tool/h2/map_event_cross_program_flag_state.py",
        "verifierSha256": "A9CA3FE7D3AB31A7C3EC0C8CE1BA2ACF7D04550DAE1BE8F24FD90A4F19C7561A",
        "semanticSha256": "2D8D019CCDAE6AB59FBF930F09ED2D908745BFAD8B14D82B8E81A20BD9ECC625",
    },
    "routingSetup": {
        "fixtureId": "sf2-map-events-static-v1:routing-setup",
        "fixtureSha256": "3B70A8040A4245D73815BCD14D2C5BB38FAAEC433C11063C9E5C6C27C9A26A8B",
        "verifierPath": "src/sf2tool/h2/map_events.py",
        "verifierSha256": "5A5193DECF494292C679A17A51D10ADD481F62948803FC1CF6250FC05F32B5EE",
        "semanticSha256": "F822B60AE8F2BEAFA8023CC0DA22ACC95EE655A43685FC761031E64F8B609106",
    },
    "mapSetup": {
        "fixtureId": "sf2-map-setup-static-v1",
        "fixtureSha256": "37FDA5E30320D65398F5D770B914CA86D501E97DB36472C1F5798BE8D3CBCAE6",
        "verifierPath": "src/sf2tool/h2/map_setup.py",
        "verifierSha256": "9E9436F68C13F93C1FDC8C9F3F1E39996194689743945ED1EAAB154696F25BFA",
        "semanticSha256": "FFB017B16236158BF7A5A8306CCF7DCBE66600EEBA7A77AD2D29240B1CF5F405",
    },
}


def _receipt(path: Path, verifier_path: str, value: dict[str, Any]) -> dict[str, str]:
    return {
        "fixtureId": value["id"],
        "fixtureSha256": _sha(path.read_bytes()),
        "verifierPath": verifier_path,
        "verifierSha256": _sha(repo_path(verifier_path).read_bytes()),
        "semanticSha256": _sha(canonical_json_bytes(value)),
    }


def _h1_bytes(rows: dict[int, tuple[bytes, str]], address: int, length: int) -> bytes:
    cursor = address
    chunks: list[bytes] = []
    end = address + length
    while cursor < end:
        row = rows.get(cursor)
        if row is None:
            raise ValueError("map-event flag route selection H1 anchor missing")
        encoded = row[0]
        if cursor + len(encoded) > end:
            raise ValueError("map-event flag route selection H1 anchor width drift")
        chunks.append(encoded)
        cursor += len(encoded)
    return b"".join(chunks)


def _source_line(disasm: Path, path: str, line_number: int, token: str) -> None:
    lines = (disasm / path).read_text(encoding="utf-8").splitlines()
    if not 1 <= line_number <= len(lines) or token not in lines[line_number - 1].split(";", 1)[0]:
        raise ValueError("map-event flag route selection source record drift")


def _record_bytes(category: str, record: dict[str, Any], *, relocated: bool) -> bytes:
    """Derive one source macro emission before or after its relocation is resolved."""
    if category == "entityEvents":
        prefix = bytes((record["entity"], record["flags"]))
    elif category == "zoneEvents":
        prefix = bytes((record["x"], record["y"]))
    elif category == "itemEvents":
        prefix = bytes((record["x"], record["y"], record["facing"], record["item"]))
    else:
        raise ValueError("map-event flag route selection record category drift")
    relative_offset = record["relativeOffset"] if relocated else 0
    return prefix + (relative_offset & 0xFFFF).to_bytes(2, "big")


def _record_h1_bytes(category: str, record: dict[str, Any]) -> bytes:
    """Keep explicit source-masked words while representing H1 relocations as zero."""
    expression = record["targetExpression"].replace(" ", "")
    return _record_bytes(category, record, relocated=expression.endswith("&$FFFF"))


def _load_owners(
    rom_path: Path, upstream_path: Path
) -> tuple[dict[str, Any], dict[str, Any], dict[str, Any], dict[str, Any]]:
    """Fresh-guard the retained public owners before constructing this join."""
    cross = build_map_event_cross_program_flag_state_contract(rom_path, upstream_path)
    routing_envelope = load_json(ROUTING_FIXTURE)
    routing = routing_envelope["expected"]
    validate_json(routing, ROUTING_SCHEMA, owner=str(ROUTING_FIXTURE))
    map_setup_envelope = load_json(MAP_SETUP_FIXTURE)
    validate_json(map_setup_envelope, MAP_SETUP_SCHEMA, owner=str(MAP_SETUP_FIXTURE))
    map_setup = build_map_setup_contract(rom_path, upstream_path)
    expected_setup = map_setup_envelope["expected"]
    if (
        map_setup["upstream"]["commit"] != map_setup_envelope["upstreamCommit"]
        or map_setup["romSha256"] != map_setup_envelope["romSha256"]
        or map_setup["function"] != map_setup_envelope["function"]
        or map_setup["table"] != map_setup_envelope["table"]
        or any(
            map_setup[field] != expected_setup[field]
            for field in (
                "summary",
                "sourceFacts",
                "aliasFlagRoutes",
                "selectionCases",
                "runtimeQuestions",
            )
        )
    ):
        raise ValueError("map-event flag route selection retained map-setup semantic drift")
    if routing["upstream"]["commit"] != _UPSTREAM_COMMIT or routing["romSha256"] != _ROM_SHA256:
        raise ValueError("map-event flag route selection routing provenance drift")
    owners = {
        "crossProgramFlagState": _receipt(
            CROSS_FIXTURE,
            "src/sf2tool/h2/map_event_cross_program_flag_state.py",
            cross,
        ),
        "routingSetup": _receipt(ROUTING_FIXTURE, "src/sf2tool/h2/map_events.py", routing_envelope),
        "mapSetup": _receipt(MAP_SETUP_FIXTURE, "src/sf2tool/h2/map_setup.py", map_setup),
    }
    if owners != _RETAINED_OWNER_EXPECTED:
        raise ValueError("map-event flag route selection retained owner identity/hash drift")
    return cross, routing, map_setup, owners


def _selector_rows(
    disasm: Path,
    map_setup: dict[str, Any],
    pointer_addresses: dict[str, int],
    h1: dict[int, tuple[bytes, str]],
    rom: bytes,
) -> list[dict[str, Any]]:
    path = "data/maps/mapsetups.asm"
    address = map_setup["table"]["MapSetups"]
    current_map: int | None = None
    rows: list[dict[str, Any]] = []
    for source_line, raw in enumerate((disasm / path).read_text(encoding="utf-8").splitlines(), 1):
        code = raw.split(";", 1)[0]
        match = re.search(r"\b(msMap|msFlag)\s+(\d+)\s*,\s*([A-Za-z_][A-Za-z0-9_]*)", code)
        if match:
            kind, numeric, pointer = match.groups()
            if kind == "msMap":
                current_map = int(numeric)
                selector_kind = "default"
                flag: int | None = None
            else:
                if current_map is None:
                    raise ValueError("map-event flag route selection selector map drift")
                selector_kind = "flag"
                flag = int(numeric)
            if pointer not in pointer_addresses:
                raise ValueError("map-event flag route selection selector pointer drift")
            expected_h1 = int(numeric).to_bytes(2, "big") + b"\0" * 4
            expected_rom = int(numeric).to_bytes(2, "big") + pointer_addresses[pointer].to_bytes(
                4, "big"
            )
            h1_bytes = _h1_bytes(h1, address, 6)
            if h1_bytes != expected_h1:
                raise ValueError("map-event flag route selection selector H1 byte drift")
            if rom[address : address + 6] != expected_rom:
                raise ValueError("map-event flag route selection selector ROM byte drift")
            rows.append(
                {
                    "selectorSourceOrder": len(rows),
                    "routeMap": current_map,
                    "selectorKind": selector_kind,
                    "flag": flag,
                    "pointerTableSymbol": pointer,
                    "pointerTableAddress": pointer_addresses[pointer],
                    "address": address,
                    "sourceLine": source_line,
                }
            )
            address += 6
            continue
        if re.search(r"\bmsMapEnd\b", code):
            if current_map is None:
                raise ValueError("map-event flag route selection selector terminator drift")
            if (
                _h1_bytes(h1, address, 2) != b"\xff\xfd"
                or rom[address : address + 2] != b"\xff\xfd"
            ):
                raise ValueError("map-event flag route selection map terminator parity drift")
            address += 2
            current_map = None
            continue
        if re.search(r"\bmsEnd\b", code):
            if (
                current_map is not None
                or _h1_bytes(h1, address, 2) != b"\xff\xff"
                or rom[address : address + 2] != b"\xff\xff"
            ):
                raise ValueError("map-event flag route selection table terminator parity drift")
            address += 2
    if current_map is not None or len(rows) != 130:
        raise ValueError("map-event flag route selection selector surface drift")
    return rows


def _project(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    rom = rom_path.read_bytes()
    if _sha(rom) != _ROM_SHA256:
        raise ValueError("map-event flag route selection ROM identity drift")
    cross, routing, map_setup, owners = _load_owners(rom_path, upstream_path)
    disasm = _root(upstream_path)
    listing = (upstream_path / "build/sf2build-h1.lst").read_text(encoding="utf-8")
    h1 = _h1_instruction_rows(listing)
    pointer_addresses = {row["symbol"]: row["address"] for row in map_setup["pointerTables"]}
    selector_rows = _selector_rows(disasm, map_setup, pointer_addresses, h1, rom)

    readers = cross["crossProgramFlagState"]["readerCohorts"]
    writers = cross["crossProgramFlagState"]["writerCohorts"]
    participants = {
        (row["category"], row["programSymbol"], row["programEntryAddress"])
        for row in readers + writers
    }
    if len(participants) != 195:
        raise ValueError("map-event flag route selection program domain drift")

    records_by_program: dict[tuple[str, str, int], list[dict[str, Any]]] = defaultdict(list)
    event_tables: dict[tuple[str, str, int], dict[str, Any]] = {}
    for category in _CATEGORIES:
        for table in routing["categories"][category]["tables"]:
            event_tables[(category, table["symbol"], table["address"])] = table
            for record in table["records"]:
                key = (category, record["targetCanonicalSymbol"], record["resolvedTargetAddress"])
                if key in participants:
                    _source_line(
                        disasm, record["sourcePath"], record["sourceLine"], record["macro"]
                    )
                    width = _RECORD_WIDTHS[category]
                    expected_h1 = _record_h1_bytes(category, record)
                    expected_rom = _record_bytes(category, record, relocated=True)
                    h1_bytes = _h1_bytes(h1, record["address"], width)
                    if h1_bytes != expected_h1:
                        raise ValueError(
                            "map-event flag route selection event-record H1 byte drift"
                        )
                    if rom[record["address"] : record["address"] + width] != expected_rom:
                        raise ValueError(
                            "map-event flag route selection event-record ROM byte drift"
                        )
                    records_by_program[key].append(
                        {
                            "category": category,
                            "eventTableSymbol": table["symbol"],
                            "eventTableAddress": table["address"],
                            "address": record["address"],
                            "sourcePath": record["sourcePath"],
                            "sourceLine": record["sourceLine"],
                        }
                    )
    if set(records_by_program) != participants:
        raise ValueError("map-event flag route selection missing route contexts")

    pointers_by_table: dict[tuple[str, str, int], list[dict[str, Any]]] = defaultdict(list)
    pointer_paths = {row["symbol"]: row["path"] for row in map_setup["pointerTables"]}
    for join in routing["setupCategoryJoins"]:
        key = (join["category"], join["eventTableSymbol"], join["eventTableAddress"])
        if key not in event_tables:
            continue
        if not any(
            record["eventTableSymbol"] == join["eventTableSymbol"]
            and record["eventTableAddress"] == join["eventTableAddress"]
            for values in records_by_program.values()
            for record in values
        ):
            continue
        pointer_path = pointer_paths.get(join["pointerTableSymbol"])
        if pointer_path is None:
            raise ValueError("map-event flag route selection pointer-table owner drift")
        source = (disasm / pointer_path).read_text(encoding="utf-8")
        if join["eventTableSymbol"] not in source or join["pointerTableSymbol"] not in source:
            raise ValueError("map-event flag route selection pointer source identity drift")
        entry = join["pointerTableAddress"] + _OFFSETS[join["category"]]
        expected_h1 = b"\0" * 4
        expected_rom = join["eventTableAddress"].to_bytes(4, "big")
        h1_bytes = _h1_bytes(h1, entry, 4)
        if h1_bytes != expected_h1:
            raise ValueError("map-event flag route selection category-pointer H1 byte drift")
        if rom[entry : entry + 4] != expected_rom:
            raise ValueError("map-event flag route selection category-pointer ROM byte drift")
        pointers_by_table[key].append(
            {
                "pointerTableSymbol": join["pointerTableSymbol"],
                "pointerTableAddress": join["pointerTableAddress"],
                "address": entry,
                "sourcePath": pointer_path,
            }
        )
    if any(
        not pointers_by_table[
            (record["category"], record["eventTableSymbol"], record["eventTableAddress"])
        ]
        for values in records_by_program.values()
        for record in values
    ):
        raise ValueError("map-event flag route selection ambiguous route contexts")

    selectors_by_pointer: dict[tuple[str, int], list[dict[str, Any]]] = defaultdict(list)
    for row in selector_rows:
        selectors_by_pointer[(row["pointerTableSymbol"], row["pointerTableAddress"])].append(row)
    selected_table_keys = set(pointers_by_table)
    selected_pointers = {
        (pointer["pointerTableSymbol"], pointer["pointerTableAddress"])
        for key in selected_table_keys
        for pointer in pointers_by_table[key]
    }
    selected_selectors = [
        row
        for row in selector_rows
        if (row["pointerTableSymbol"], row["pointerTableAddress"]) in selected_pointers
    ]
    if (
        len(selected_pointers),
        len(selected_selectors),
        len({row["routeMap"] for row in selected_selectors}),
    ) != (91, 94, 51):
        raise ValueError("map-event flag route selection selector denominator drift")

    route_contexts: list[dict[str, Any]] = []
    context_by_program: dict[tuple[str, str, int], dict[str, Any]] = {}
    for order, key in enumerate(sorted(participants)):
        record_rows = sorted(records_by_program[key], key=lambda row: row["address"])
        table_keys = {
            (row["category"], row["eventTableSymbol"], row["eventTableAddress"])
            for row in record_rows
        }
        pointer_rows = sorted(
            {
                (row["pointerTableSymbol"], row["pointerTableAddress"])
                for table_key in table_keys
                for row in pointers_by_table[table_key]
            }
        )
        maps = sorted(
            {row["routeMap"] for pointer in pointer_rows for row in selectors_by_pointer[pointer]}
        )
        row = {
            "routeContextOrder": order,
            "category": key[0],
            "programSymbol": key[1],
            "programEntryAddress": key[2],
            "eventTableAddresses": sorted({record["eventTableAddress"] for record in record_rows}),
            "pointerTableAddresses": [address for _symbol, address in pointer_rows],
            "routeMaps": maps,
        }
        route_contexts.append(row)
        context_by_program[key] = row
    if any(
        not row["eventTableAddresses"] or not row["pointerTableAddresses"] or not row["routeMaps"]
        for row in route_contexts
    ):
        raise ValueError("map-event flag route selection incomplete route context")

    candidates: list[dict[str, Any]] = []
    topology = Counter()
    for candidate in cross["crossProgramFlagState"]["crossProgramCandidates"]:
        writer = writers[candidate["writerCohortOrder"]]
        reader = readers[candidate["readerCohortOrder"]]
        writer_context = context_by_program[
            (writer["category"], writer["programSymbol"], writer["programEntryAddress"])
        ]
        reader_context = context_by_program[
            (reader["category"], reader["programSymbol"], reader["programEntryAddress"])
        ]
        if set(writer_context["eventTableAddresses"]) & set(reader_context["eventTableAddresses"]):
            classification = "sameEventTable"
        elif set(writer_context["pointerTableAddresses"]) & set(
            reader_context["pointerTableAddresses"]
        ):
            classification = "sameSelectedSetupDifferentEventTable"
        elif set(writer_context["routeMaps"]) & set(reader_context["routeMaps"]):
            classification = "sameMapDifferentSelector"
        else:
            classification = "crossMapOnly"
        topology[classification] += 1
        candidates.append(
            {
                "edgeOrder": candidate["edgeOrder"],
                "flagNumber": candidate["flagNumber"],
                "writerRouteContextOrder": writer_context["routeContextOrder"],
                "readerRouteContextOrder": reader_context["routeContextOrder"],
                "classification": classification,
            }
        )
    if dict(topology) != _EXPECTED_TOPOLOGY or len(candidates) != 720:
        raise ValueError("map-event flag route selection precedence/count drift")

    candidate_writer_orders = {
        row["writerCohortOrder"] for row in cross["crossProgramFlagState"]["crossProgramCandidates"]
    }
    selector_relations = []
    for writer_order, writer in enumerate(writers):
        if writer_order not in candidate_writer_orders:
            continue
        writer_context = context_by_program[
            (writer["category"], writer["programSymbol"], writer["programEntryAddress"])
        ]
        for selector in selected_selectors:
            if selector["selectorKind"] == "flag" and selector["flag"] == writer["flagNumber"]:
                selector_relations.append(
                    {
                        "relationOrder": len(selector_relations),
                        "flagNumber": writer["flagNumber"],
                        "writerRouteContextOrder": writer_context["routeContextOrder"],
                        "selectorSourceOrder": selector["selectorSourceOrder"],
                    }
                )
    selector_relations.sort(
        key=lambda row: (
            row["flagNumber"],
            row["writerRouteContextOrder"],
            row["selectorSourceOrder"],
        )
    )
    for order, row in enumerate(selector_relations):
        row["relationOrder"] = order
    if (
        len(selector_relations),
        len({row["flagNumber"] for row in selector_relations}),
        len({(row["flagNumber"], row["writerRouteContextOrder"]) for row in selector_relations}),
    ) != (15, 11, 11):
        raise ValueError("map-event flag route selection writer-selector construction drift")

    new_anchors = []
    for values in records_by_program.values():
        for row in values:
            new_anchors.append((row["address"], _RECORD_WIDTHS[row["category"]], "eventRecord"))
    for values in pointers_by_table.values():
        for row in values:
            new_anchors.append((row["address"], 4, "categoryPointer"))
    for row in selected_selectors:
        new_anchors.append((row["address"], 6, "routeSelector"))
    if len({address for address, _length, _cohort in new_anchors}) != len(new_anchors):
        raise ValueError("map-event flag route selection new physical cohort overlap")
    cross_anchors = cross["crossProgramFlagState"]["physicalContextCoverage"]["physicalAnchors"]
    if {address for address, _length, _cohort in new_anchors} & {
        row["address"] for row in cross_anchors
    }:
        raise ValueError("map-event flag route selection retained/new anchor overlap")
    anchors = [
        {"address": row["address"], "h1ByteLength": row["h1ByteLength"], "cohort": "crossProgram"}
        for row in cross_anchors
    ] + [
        {"address": address, "h1ByteLength": length, "cohort": cohort}
        for address, length, cohort in new_anchors
    ]
    anchors.sort(key=lambda row: row["address"])
    coverage = {
        "retainedCrossProgramAnchorPcCount": len(cross_anchors),
        "retainedCrossProgramAnchorByteCount": sum(row["h1ByteLength"] for row in cross_anchors),
        "eventRecordAnchorPcCount": sum(
            cohort == "eventRecord" for _address, _length, cohort in new_anchors
        ),
        "eventRecordAnchorByteCount": sum(
            length for _address, length, cohort in new_anchors if cohort == "eventRecord"
        ),
        "categoryPointerAnchorPcCount": sum(
            cohort == "categoryPointer" for _address, _length, cohort in new_anchors
        ),
        "categoryPointerAnchorByteCount": sum(
            length for _address, length, cohort in new_anchors if cohort == "categoryPointer"
        ),
        "routeSelectorAnchorPcCount": sum(
            cohort == "routeSelector" for _address, _length, cohort in new_anchors
        ),
        "routeSelectorAnchorByteCount": sum(
            length for _address, length, cohort in new_anchors if cohort == "routeSelector"
        ),
        "physicalAnchorPcCount": len(anchors),
        "physicalAnchorByteCount": sum(row["h1ByteLength"] for row in anchors),
        "physicalAnchors": anchors,
    }
    if tuple(coverage[key] for key in coverage if key != "physicalAnchors") != (
        804,
        2592,
        284,
        1150,
        139,
        556,
        94,
        564,
        1321,
        4862,
    ):
        raise ValueError("map-event flag route selection physical coverage drift")

    cross_paths = {row["path"] for row in cross["sourceContext"]["sourceIdentities"]}
    record_paths = {row["sourcePath"] for values in records_by_program.values() for row in values}
    pointer_source_paths = {
        row["sourcePath"] for values in pointers_by_table.values() for row in values
    }
    source_paths = (
        cross_paths
        | record_paths
        | pointer_source_paths
        | {"data/maps/mapsetups.asm", "sf2mapsetupmacros.asm"}
    )
    if (
        len(cross_paths),
        len(record_paths),
        len(cross_paths & record_paths),
        len(record_paths - cross_paths),
        len(pointer_source_paths),
        len(source_paths),
    ) != (91, 96, 88, 8, 91, 192):
        raise ValueError("map-event flag route selection source identity denominator drift")
    macro_source = (disasm / "sf2mapsetupmacros.asm").read_text(encoding="utf-8")
    if not all(
        token in macro_source
        for token in ("msMap: macro", "msFlag: macro", "msMapEnd: macro", "msEnd: macro")
    ):
        raise ValueError("map-event flag route selection map setup macro drift")
    source_context = {
        "sourceIdentities": [
            {"path": path, "sha256": _sha((disasm / path).read_bytes())}
            for path in sorted(source_paths)
        ],
        "h1Listing": {
            "path": "build/sf2build-h1.lst",
            "sha256": _sha((upstream_path / "build/sf2build-h1.lst").read_bytes()),
        },
        "identityDenominators": {
            "crossProgramIdentityCount": 91,
            "matchingEventRecordOwnerFileCount": 96,
            "crossProgramRecordOwnerOverlapCount": 88,
            "additionalEventRecordOwnerFileCount": 8,
            "pointerTableOwnerFileCount": 91,
            "sourceIdentityCount": 192,
        },
    }
    facts = {
        "retainedIdentities": {
            "entityEvents": cross["crossProgramFlagState"]["categoryRoles"]["entityEvents"],
            "zoneEvents": cross["crossProgramFlagState"]["categoryRoles"]["zoneEvents"],
            "itemEvents": cross["crossProgramFlagState"]["categoryRoles"]["itemEvents"],
            "trapEntryAddress": cross["crossProgramFlagState"]["serviceJoin"]["trapEntryAddress"],
            "selectorEntryAddress": map_setup["function"]["GetCurrentMapSetup"],
            "mapSetupsEntryAddress": map_setup["table"]["MapSetups"],
        },
        "programRouteContexts": route_contexts,
        "classifiedCandidates": candidates,
        "topologyCategoryTotals": {
            "classificationTotals": [
                {"classification": key, "candidateCount": topology[key]} for key in _TOPOLOGY_ORDER
            ],
            "categoryPairTotals": cross["crossProgramFlagState"]["categoryPairTotals"],
        },
        "selectorWriterRelations": selector_relations,
        "domainDenominators": {
            "programRouteContextCount": 195,
            "crossProgramCandidateCount": 720,
            "matchingEventRecordCount": 284,
            "matchingEventTableCount": 96,
            "selectedPointerTableCount": 91,
            "physicalCategoryPointerEntryCount": 139,
            "selectedSelectorRowCount": 94,
            "routeMapCount": 51,
        },
        "physicalCoverage": coverage,
        "digests": {
            "programRouteContextSha256": _sha(
                canonical_json_bytes({"programRouteContexts": route_contexts})
            ),
            "classifiedCandidateSha256": _sha(
                canonical_json_bytes({"classifiedCandidates": candidates})
            ),
            "selectorWriterRelationSha256": _sha(
                canonical_json_bytes({"selectorWriterRelations": selector_relations})
            ),
            "physicalCoverageSha256": _sha(canonical_json_bytes({"physicalCoverage": coverage})),
        },
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstreamCommit": _UPSTREAM_COMMIT,
        "romSha256": _ROM_SHA256,
        "sourceContext": source_context,
        "retainedOwners": owners,
        "flagRouteSelection": facts,
        "unknowns": {key: "Unknown" for key in _UNKNOWN_KEYS},
        "summary": {
            "sourceIdentityCount": 192,
            "crossProgramCandidateCount": 720,
            "matchingEventRecordCount": 284,
            "matchingEventTableCount": 96,
            "selectedPointerTableCount": 91,
            "physicalCategoryPointerEntryCount": 139,
            "selectedSelectorRowCount": 94,
            "routeMapCount": 51,
            "selectorWriterRelationCount": 15,
            "selectorWriterFlagCount": 11,
            "selectorWriterProgramCount": 11,
            "physicalAnchorPcCount": 1321,
            "physicalAnchorByteCount": 4862,
        },
    }


def _validate_order(value: dict[str, Any]) -> None:
    if tuple(value) != (
        "schemaVersion",
        "id",
        "upstreamCommit",
        "romSha256",
        "sourceContext",
        "retainedOwners",
        "flagRouteSelection",
        "unknowns",
        "summary",
    ):
        raise ValueError("map-event flag route selection root order drift")
    facts = value["flagRouteSelection"]
    if tuple(facts) != (
        "retainedIdentities",
        "programRouteContexts",
        "classifiedCandidates",
        "topologyCategoryTotals",
        "selectorWriterRelations",
        "domainDenominators",
        "physicalCoverage",
        "digests",
    ):
        raise ValueError("map-event flag route selection facts order drift")
    if tuple(value["unknowns"]) != _UNKNOWN_KEYS or set(value["unknowns"].values()) != {"Unknown"}:
        raise ValueError("map-event flag route selection Unknown queue drift")
    if [row["routeContextOrder"] for row in facts["programRouteContexts"]] != list(range(195)):
        raise ValueError("map-event flag route selection route-context order drift")
    if [row["edgeOrder"] for row in facts["classifiedCandidates"]] != list(range(720)):
        raise ValueError("map-event flag route selection candidate order drift")
    if [row["relationOrder"] for row in facts["selectorWriterRelations"]] != list(range(15)):
        raise ValueError("map-event flag route selection relation order drift")
    if {
        row["classification"]: row["candidateCount"]
        for row in facts["topologyCategoryTotals"]["classificationTotals"]
    } != _EXPECTED_TOPOLOGY:
        raise ValueError("map-event flag route selection classification total drift")
    anchors = facts["physicalCoverage"]["physicalAnchors"]
    if len(anchors) != 1321 or [row["address"] for row in anchors] != sorted(
        row["address"] for row in anchors
    ):
        raise ValueError("map-event flag route selection physical anchor order drift")


def build_map_event_flag_route_selection_contract(
    rom_path: Path, upstream_path: Path
) -> dict[str, Any]:
    output = _project(rom_path.resolve(strict=True), upstream_path.resolve(strict=True))
    _validate_order(output)
    return output


def verify_map_event_flag_route_selection_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner=str(FIXTURE))
    _validate_order(fixture)
    output = build_map_event_flag_route_selection_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="map-event flag route selection static contract")
    if fixture != output:
        raise ValueError("map-event flag route selection complete semantic fixture drift")
    destination = output_path or repo_path(
        "local/derived/map-event-flag-route-selection-static.json"
    )
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(
        json.dumps(output, ensure_ascii=False, indent=2).encode("utf-8") + b"\n"
    )
    return {
        "Contract": ID,
        "SHA256": _sha(canonical_json_bytes(output)),
        "Candidates": 720,
        "Status": "PASS",
        "Output": display_path(destination),
    }


def _remove_map_event_flag_route_selection_later_owner_index_delta(
    index: dict[str, Any],
) -> dict[str, Any]:
    normalized = deepcopy(index)
    records = normalized.get("records")
    if not isinstance(records, list):
        raise ValueError("map-event flag route selection index record surface drift")
    record_ids = [row.get("id") for row in records if isinstance(row, dict)]
    if len(record_ids) != len(records) or len(record_ids) != len(set(record_ids)):
        raise ValueError("map-event flag route selection index record identity drift")
    bindings = {
        "map.setup.entity-event": [("entry", "flagRouteSelection.retainedIdentities.entityEvents")],
        "map.setup.zone-event": [("entry", "flagRouteSelection.retainedIdentities.zoneEvents")],
        "map.setup.item-event": [("entry", "flagRouteSelection.retainedIdentities.itemEvents")],
        "tech.interrupts.trap-flags": [
            ("entry", "flagRouteSelection.retainedIdentities.trapEntryAddress")
        ],
        "map.setup.selector": [
            ("entry", "flagRouteSelection.retainedIdentities.selectorEntryAddress")
        ],
        "map.data.mapsetups": [
            ("entry", "flagRouteSelection.retainedIdentities.mapSetupsEntryAddress")
        ],
    }
    document = "docs/research/map-event-flag-route-selection.md"
    seen: set[str] = set()
    for record in records:
        expected_bindings = bindings.get(record.get("id"))
        if expected_bindings is None:
            continue
        expected = {
            "level": "H2",
            "fixture": "tests/fixtures/h2/map-event-flag-route-selection-static-v1.json",
            "fixtureId": ID,
            "verifier": "src/sf2tool/h2/map_event_flag_route_selection.py",
            "bindings": [
                {"addressId": address_id, "fixtureField": field}
                for address_id, field in expected_bindings
            ],
        }
        evidence = record.get("evidence")
        if not isinstance(evidence, list):
            raise ValueError("map-event flag route selection index evidence surface drift")
        matches = [row for row in evidence if row.get("fixtureId") == ID]
        documents = record.get("documents")
        if (
            matches != [expected]
            or not isinstance(documents, list)
            or documents.count(document) != 1
            or documents[-1] != document
        ):
            raise ValueError("map-event flag route selection index delta drift")
        evidence.remove(expected)
        documents.remove(document)
        seen.add(record["id"])
    if seen != set(bindings):
        raise ValueError("map-event flag route selection index coverage drift")
    return normalized


def normalize_map_event_flag_route_selection_later_owner_index(
    index: dict[str, Any],
) -> dict[str, Any]:
    return normalize_map_event_cross_program_flag_state_later_owner_index(
        _remove_map_event_flag_route_selection_later_owner_index_delta(index)
    )
