from __future__ import annotations

import hashlib
import json
import re
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.h2.map_entities import build_map_entities_contract
from sf2tool.h2.map_setup import build_map_setup_contract
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.source_text import read_upstream_text

ID = "sf2-map-events-static-v1"
SOURCE_ROOT = Path("data/maps/entries")
MANIFEST = repo_path("manifests/extractions/map-events-static.json")
SCHEMA = repo_path("schemas/map-events-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/map-events-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-map-events-static-fixture.schema.json")

CATEGORY_CONFIG = {
    "entityEvents": {
        "glob": "s2_entityevents*.asm",
        "recordBytes": 4,
        "specificMacros": ("msEntityEvent",),
        "defaultMacros": ("msDefaultEntityEvent", "msDftEntityEvent"),
        "stubSymbols": ("ms_map52_EntityEvents", "ms_map55_EntityEvents"),
    },
    "zoneEvents": {
        "glob": "s3_zoneevents*.asm",
        "recordBytes": 4,
        "specificMacros": ("msZoneEvent",),
        "defaultMacros": ("msDefaultZoneEvent",),
        "stubSymbols": (),
    },
    "itemEvents": {
        "glob": "s5_itemevents*.asm",
        "recordBytes": 6,
        "specificMacros": ("msItemEvent",),
        "defaultMacros": ("msDefaultItemEvent",),
        "stubSymbols": (),
    },
}

RAW_ZONE_DEFAULT_SYMBOL = "ms_map44_ZoneEvents"
FUNCTION_SYMBOLS = (
    "RunMapSetupEntityEvent",
    "RunMapSetupZoneEvent",
    "RunMapSetupItemEvent",
)
REACHABILITY_FUNCTION_SYMBOLS = (
    "ProcessPlayerAction",
    "GetActivatedEntity",
    "GetEntityEventIndex",
)
SELECTION_INPUTS = (
    ("entity-specific-after-scan", "entityEvents", 3, (), {"entity": 128}),
    ("entity-default", "entityEvents", 3, (), {"entity": 135}),
    ("zone-exact", "zoneEvents", 3, (), {"x": 27, "y": 5}),
    ("zone-wildcard-y", "zoneEvents", 3, (), {"x": 2, "y": 42}),
    ("zone-first-overlapping-match", "zoneEvents", 3, (609,), {"x": 2, "y": 23}),
    ("zone-default", "zoneEvents", 3, (), {"x": 10, "y": 10}),
    (
        "item-index-mask",
        "itemEvents",
        8,
        (),
        {"x": 15, "y": 19, "facing": 1, "item": 240},
    ),
    (
        "item-facing-mismatch-default",
        "itemEvents",
        8,
        (),
        {"x": 15, "y": 19, "facing": 2, "item": 112},
    ),
    (
        "item-wildcard-facing",
        "itemEvents",
        22,
        (),
        {"x": 35, "y": 24, "facing": 3, "item": 125},
    ),
)


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _decode_event_record(
    category: str, table_address: int, record_address: int, data: bytes
) -> dict[str, Any]:
    expected_size = CATEGORY_CONFIG[category]["recordBytes"]
    if len(data) != expected_size:
        raise ValueError(f"{category} record must contain {expected_size} bytes")
    relative_offset = int.from_bytes(data[-2:], "big", signed=True)
    record: dict[str, Any] = {
        "address": record_address,
        "kind": "default" if data[0] == 0xFD else "specific",
        "relativeOffset": relative_offset,
        "resolvedTargetAddress": table_address + relative_offset,
    }
    if category == "entityEvents":
        record.update({"entity": data[0], "flags": data[1]})
    elif category == "zoneEvents":
        record.update({"x": data[0], "y": data[1]})
    elif category == "itemEvents":
        record.update(
            {"x": data[0], "y": data[1], "facing": data[2], "item": data[3]}
        )
    else:
        raise ValueError(f"unknown map event category: {category}")
    return record


def _instruction_tokens(source: str) -> list[str]:
    tokens: list[str] = []
    for raw_line in source.splitlines():
        line = raw_line.split(";", 1)[0].strip()
        line = re.sub(r"^[A-Za-z_][A-Za-z0-9_]*:\s*", "", line).strip()
        if line:
            tokens.append(line)
    return tokens


def _event_matches(category: str, record: dict[str, Any], query: dict[str, int]) -> bool:
    if record["kind"] == "default":
        return True
    if category == "entityEvents":
        return record["entity"] == (query["entity"] & 0xFF)
    if category == "zoneEvents":
        return all(
            record[field] == 0xFF or record[field] == (query[field] & 0xFF)
            for field in ("x", "y")
        )
    if category == "itemEvents":
        coordinates_match = all(
            record[field] == 0xFF or record[field] == (query[field] & 0xFF)
            for field in ("x", "y", "facing")
        )
        return coordinates_match and record["item"] == (query["item"] & 0x7F)
    raise ValueError(f"unknown map event category: {category}")


def _selected_setup_symbol(
    setup: dict[str, Any], map_index: int, set_flags: set[int]
) -> str | None:
    route = next((row for row in setup["routes"] if row["map"] == map_index), None)
    if route is None:
        return None
    selected = route["defaultPointer"]
    for variant in route["flagVariants"]:
        if variant["flag"] in set_flags:
            selected = variant["pointer"]
    return selected


def _selection_cases(
    setup: dict[str, Any], categories: dict[str, dict[str, Any]]
) -> list[dict[str, Any]]:
    pointer_tables = {row["symbol"]: row for row in setup["pointerTables"]}
    event_tables = {
        category: {row["symbol"]: row for row in value["tables"]}
        for category, value in categories.items()
    }
    cases: list[dict[str, Any]] = []
    for case_id, category, map_index, flags, query in SELECTION_INPUTS:
        setup_symbol = _selected_setup_symbol(setup, map_index, set(flags))
        if setup_symbol is None:
            raise ValueError(f"selection case unexpectedly uses a missing map: {case_id}")
        table_symbol = pointer_tables[setup_symbol]["targets"][category]["symbol"]
        table = event_tables[category].get(table_symbol)
        if table is None:
            raise ValueError(f"selection case uses a direct-return event stub: {case_id}")
        selected = next(
            (row for row in table["records"] if _event_matches(category, row, query)),
            None,
        )
        if selected is None:
            raise ValueError(f"selection case has no default record: {case_id}")
        cases.append(
            {
                "id": case_id,
                "category": category,
                "map": map_index,
                "setFlags": list(flags),
                "query": query,
                "selectedSetup": setup_symbol,
                "selectedTable": table_symbol,
                "selectedRecordAddress": selected["address"],
                "selectedRecordKind": selected["kind"],
                "eventFlags": selected.get("flags"),
                "resolvedTargetAddress": selected["resolvedTargetAddress"],
            }
        )
    return cases


def _source_rows(
    disasm: Path, addresses: dict[str, int], category: str
) -> tuple[list[dict[str, Any]], dict[int, str]]:
    config = CATEGORY_CONFIG[category]
    paths = sorted(
        (
            path
            for path in (disasm / SOURCE_ROOT).rglob(config["glob"])
            if "mapsetups" in path.parts
        ),
        key=lambda path: path.as_posix(),
    )
    files: list[dict[str, Any]] = []
    record_kinds: dict[int, str] = {}
    known_macros = {
        **{macro: "specific" for macro in config["specificMacros"]},
        **{macro: "default" for macro in config["defaultMacros"]},
    }
    for path in paths:
        source = read_upstream_text(path)
        labels = re.findall(r"^([A-Za-z_][A-Za-z0-9_]*):", source, re.MULTILINE)
        if not labels or labels[0] not in addresses:
            raise ValueError(f"{category} source has no H1-bound entry label: {path}")
        symbol = labels[0]
        address = addresses[symbol]
        is_stub = symbol in config["stubSymbols"]
        if is_stub and _instruction_tokens(source) != ["rts"]:
            raise ValueError(f"{category} direct-return stub shape drift: {symbol}")

        kinds: list[str] = []
        macro_counts: Counter[str] = Counter()
        for raw_line in source.splitlines():
            line = raw_line.split(";", 1)[0].strip()
            line = re.sub(r"^[A-Za-z_][A-Za-z0-9_]*:\s*", "", line)
            match = re.match(r"([A-Za-z_][A-Za-z0-9_]*)\b", line)
            if match and match.group(1) in known_macros:
                macro = match.group(1)
                kinds.append(known_macros[macro])
                macro_counts[macro] += 1
        is_raw_default = category == "zoneEvents" and symbol == RAW_ZONE_DEFAULT_SYMBOL
        if is_raw_default:
            if kinds or "dc.w $FD00" not in source or "byte_54868+4" not in source:
                raise ValueError("map 44 raw zone-default exception shape drift")
            kinds.append("default")
        if is_stub and kinds:
            raise ValueError(f"direct-return stub unexpectedly owns table records: {symbol}")
        if not is_stub and (not kinds or kinds[-1] != "default"):
            raise ValueError(f"{category} table lacks a final default record: {symbol}")

        for index, kind in enumerate(kinds):
            record_address = address + index * config["recordBytes"]
            if record_address in record_kinds:
                raise ValueError(
                    f"overlapping source-owned map event record at 0x{record_address:X}"
                )
            record_kinds[record_address] = kind
        files.append(
            {
                "path": path.relative_to(disasm).as_posix(),
                "symbol": symbol,
                "address": address,
                "recordCount": len(kinds),
                "specificRecordCount": kinds.count("specific"),
                "defaultRecordCount": kinds.count("default"),
                "macroCounts": dict(sorted(macro_counts.items())),
                "directReturnStub": is_stub,
                "rawDefaultException": is_raw_default,
            }
        )
    return files, record_kinds


def _category_contract(
    disasm: Path,
    addresses: dict[str, int],
    rom: bytes,
    setup: dict[str, Any],
    category: str,
) -> dict[str, Any]:
    config = CATEGORY_CONFIG[category]
    files, source_record_kinds = _source_rows(disasm, addresses, category)
    targets = [table["targets"][category] for table in setup["pointerTables"]]
    target_counts = Counter(target["symbol"] for target in targets)
    unique_targets = {target["symbol"]: target["address"] for target in targets}
    if set(unique_targets) != {row["symbol"] for row in files}:
        raise ValueError(f"map setup pointers do not own the complete {category} source boundary")

    source_by_symbol = {row["symbol"]: row for row in files}
    tables: list[dict[str, Any]] = []
    physical_records: dict[int, dict[str, Any]] = {}
    for symbol, address in sorted(unique_targets.items()):
        source_row = source_by_symbol[symbol]
        if source_row["directReturnStub"]:
            if rom[address : address + 2] != b"\x4E\x75":
                raise ValueError(f"{category} direct-return stub ROM drift: {symbol}")
            continue
        records: list[dict[str, Any]] = []
        cursor = address
        while True:
            raw = rom[cursor : cursor + config["recordBytes"]]
            if len(raw) != config["recordBytes"] or len(records) >= 48:
                raise ValueError(f"{category} table has no bounded default record: {symbol}")
            decoded = _decode_event_record(category, address, cursor, raw)
            expected_kind = source_record_kinds.get(cursor)
            if expected_kind != decoded["kind"]:
                raise ValueError(f"{category} source/ROM record drift at 0x{cursor:X}")
            if cursor in physical_records:
                raise ValueError(f"{category} physical records overlap at 0x{cursor:X}")
            physical_records[cursor] = decoded
            records.append(decoded)
            cursor += config["recordBytes"]
            if decoded["kind"] == "default":
                break
        if len(records) != source_row["recordCount"]:
            raise ValueError(f"{category} source/ROM table length drift: {symbol}")
        tables.append(
            {
                "symbol": symbol,
                "address": address,
                "recordCount": len(records),
                "records": records,
            }
        )
    if set(physical_records) != set(source_record_kinds):
        raise ValueError(f"{category} source records are not exactly covered by setup tables")

    physical_kinds = Counter(record["kind"] for record in physical_records.values())
    setup_kinds: Counter[str] = Counter()
    table_by_symbol = {row["symbol"]: row for row in tables}
    for target in targets:
        table = table_by_symbol.get(target["symbol"])
        if table is not None:
            setup_kinds.update(record["kind"] for record in table["records"])
    source_macro_counts: Counter[str] = Counter()
    for row in files:
        source_macro_counts.update(row["macroCounts"])
    summary = {
        "sourceFileCount": len(files),
        "setupPointerReferenceCount": len(targets),
        "uniqueTargetCount": len(unique_targets),
        "decodedTableCount": len(tables),
        "aliasedTargetCount": sum(count > 1 for count in target_counts.values()),
        "physicalRecordCount": len(physical_records),
        "specificPhysicalRecordCount": physical_kinds["specific"],
        "defaultPhysicalRecordCount": physical_kinds["default"],
        "setupRecordReferenceCount": sum(setup_kinds.values()),
        "specificSetupRecordReferenceCount": setup_kinds["specific"],
        "defaultSetupRecordReferenceCount": setup_kinds["default"],
        "directReturnStubCount": sum(row["directReturnStub"] for row in files),
        "directReturnStubReferenceCount": sum(
            target_counts[row["symbol"]] for row in files if row["directReturnStub"]
        ),
        "rawDefaultExceptionCount": sum(row["rawDefaultException"] for row in files),
        "maximumTableRecordCount": max(row["recordCount"] for row in tables),
    }
    return {
        "summary": summary,
        "sourceMacroCounts": dict(sorted(source_macro_counts.items())),
        "duplicatePointerTargets": [
            {"symbol": symbol, "setupReferenceCount": count}
            for symbol, count in sorted(target_counts.items())
            if count > 1
        ],
        "sourceFiles": files,
        "tables": tables,
    }


def _consumer_facts(setup: dict[str, Any]) -> dict[str, Any]:
    dispatch = setup["sourceFacts"]["dispatch"]
    return {
        "defaultMarker": 0xFD,
        "relativeOffsetsResolveFromTableBase": True,
        "firstMatchingEntryWins": True,
        "entityEvents": dispatch["entityEvent"],
        "zoneEvents": dispatch["zoneEvent"],
        "itemEvents": {**dispatch["itemEvent"], "itemIndexMask": 0x7F},
    }


def _entity_event_reachability_facts(
    disasm: Path, addresses: dict[str, int]
) -> dict[str, Any]:
    sources = {
        "ProcessPlayerAction": read_upstream_text(
            disasm / "code/gameflow/exploration/explorationvints.asm"
        ),
        "GetActivatedEntity": read_upstream_text(
            disasm / "code/gameflow/exploration/explorationfunctions_0.asm"
        ),
        "GetEntityEventIndex": read_upstream_text(
            disasm / "code/gameflow/battle/battlefunctions/battlefunctions_0.asm"
        ),
    }
    required = {
        "ProcessPlayerAction": (
            "bsr.w   GetActivatedEntity",
            "tst.w   d0",
            "bsr.w   GetEntityEventIndex",
            "jsr     j_RunMapSetupEntityEvent",
        ),
        "GetActivatedEntity": (
            "moveq   #$2F,d7",
            "bsr.w   IsFollowerEntity",
            "cmpi.w  #MAP_TILE_SIZE,d5",
            "moveq   #-1,d0",
        ),
        "GetEntityEventIndex": (
            "moveq   #BATTLE_ALL_ENTITIES_NUMBER,d7",
            "lea     ((ENTITY_INDEX_LIST-$1000000)).w,a0",
            "cmpi.w  #BATTLE_ALLY_ENTITIES_NUMBER,d0",
            "move.w  #$80,d0",
        ),
    }
    for symbol, fragments in required.items():
        if any(fragment not in sources[symbol] for fragment in fragments):
            raise ValueError(f"entity event reachability source-shape drift: {symbol}")
    return {
        "functionAddresses": {
            symbol: addresses[symbol] for symbol in REACHABILITY_FUNCTION_SYMBOLS
        },
        "activatedEntityScanSlots": 48,
        "followersAreSkipped": True,
        "adjacentDistanceIsStrictlyBelowMapTileSize": True,
        "entityIndexListSlotsScanned": 65,
        "enemyEventIndexBase": 128,
        "processActionCallsWrapperAfterNonnegativeActivation": True,
    }


def _clean_state_event_indices(records: list[dict[str, Any]]) -> list[int]:
    enemy_ordinal = 0
    event_indices: list[int] = []
    for record in records:
        if record["mapSprite"] >= 240:
            raise ValueError("direct-return reachability model does not cover special map sprites")
        if record["mapSprite"] < 30:
            event_indices.append(record["mapSprite"])
        else:
            event_indices.append(128 + enemy_ordinal)
            enemy_ordinal += 1
    return event_indices


def build_map_events_contract(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"map events H1 listing is missing: {listing_path}")
    addresses = listing_symbol_addresses(listing_path.read_text(encoding="utf-8"))
    rom = rom_path.read_bytes()
    setup = build_map_setup_contract(rom_path, upstream_path)
    entities = build_map_entities_contract(rom_path, upstream_path)
    if setup["upstream"]["commit"] != commit:
        raise ValueError("map events/setup provenance drift")

    categories = {
        category: _category_contract(disasm, addresses, rom, setup, category)
        for category in CATEGORY_CONFIG
    }
    entity_target_refs = [
        table["targets"]["entityEvents"]["symbol"] for table in setup["pointerTables"]
    ]
    entity_lists = {row["symbol"]: row for row in entities["lists"]}
    direct_return_stubs: list[dict[str, Any]] = []
    for symbol in CATEGORY_CONFIG["entityEvents"]["stubSymbols"]:
        owners = [
            table
            for table in setup["pointerTables"]
            if table["targets"]["entityEvents"]["symbol"] == symbol
        ]
        pairings: list[dict[str, Any]] = []
        for table in owners:
            entity_symbol = table["targets"]["entities"]["symbol"]
            entity_list = entity_lists[entity_symbol]
            event_indices = _clean_state_event_indices(entity_list["records"])
            pairings.append(
                {
                    "setupSymbol": table["symbol"],
                    "entityListSymbol": entity_symbol,
                    "entityRecordCount": entity_list["recordCount"],
                    "cleanStateEventIndices": event_indices,
                    "wrapperReachableWithAdjacentNonFollower": bool(event_indices),
                    "normalStoryRouteReachability": (
                        "unknown" if event_indices else "not-applicable-empty-list"
                    ),
                }
            )
        paired_record_counts = [row["entityRecordCount"] for row in pairings]
        direct_return_stubs.append(
            {
                "symbol": symbol,
                "address": addresses[symbol],
                "setupReferenceCount": entity_target_refs.count(symbol),
                "pairedEntityListRecordCounts": paired_record_counts,
                "nonEmptyPairedEntityListReferenceCount": sum(
                    record_count > 0 for record_count in paired_record_counts
                ),
                "setupPairings": pairings,
            }
        )
    raw_record = next(
        record
        for table in categories["zoneEvents"]["tables"]
        if table["symbol"] == RAW_ZONE_DEFAULT_SYMBOL
        for record in table["records"]
    )
    raw_zone_default = {
        "symbol": RAW_ZONE_DEFAULT_SYMBOL,
        "address": addresses[RAW_ZONE_DEFAULT_SYMBOL],
        "relativeOffset": raw_record["relativeOffset"],
        "resolvedTargetAddress": raw_record["resolvedTargetAddress"],
        "pointsInsideCutsceneEntityList": raw_record["resolvedTargetAddress"]
        == addresses["byte_54868"] + 4,
    }
    if not raw_zone_default["pointsInsideCutsceneEntityList"]:
        raise ValueError("map 44 raw zone-default target drift")

    category_summaries = {
        category: value["summary"] for category, value in categories.items()
    }
    summary = {
        "sourceFileCount": sum(row["sourceFileCount"] for row in category_summaries.values()),
        "setupPointerReferenceCount": sum(
            row["setupPointerReferenceCount"] for row in category_summaries.values()
        ),
        "uniqueTargetCount": sum(row["uniqueTargetCount"] for row in category_summaries.values()),
        "physicalRecordCount": sum(
            row["physicalRecordCount"] for row in category_summaries.values()
        ),
        "specificPhysicalRecordCount": sum(
            row["specificPhysicalRecordCount"] for row in category_summaries.values()
        ),
        "defaultPhysicalRecordCount": sum(
            row["defaultPhysicalRecordCount"] for row in category_summaries.values()
        ),
        "setupRecordReferenceCount": sum(
            row["setupRecordReferenceCount"] for row in category_summaries.values()
        ),
        "specificSetupRecordReferenceCount": sum(
            row["specificSetupRecordReferenceCount"] for row in category_summaries.values()
        ),
        "defaultSetupRecordReferenceCount": sum(
            row["defaultSetupRecordReferenceCount"] for row in category_summaries.values()
        ),
        "directReturnStubCount": sum(
            row["directReturnStubCount"] for row in category_summaries.values()
        ),
        "directReturnStubReferenceCount": sum(
            row["directReturnStubReferenceCount"] for row in category_summaries.values()
        ),
        "rawDefaultExceptionCount": sum(
            row["rawDefaultExceptionCount"] for row in category_summaries.values()
        ),
        "maximumTableRecordCount": max(
            row["maximumTableRecordCount"] for row in category_summaries.values()
        ),
        "selectionCaseCount": len(SELECTION_INPUTS),
    }
    selection_cases = _selection_cases(setup, categories)
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "romSha256": setup["romSha256"],
        "scope": f"{SOURCE_ROOT.as_posix()}/*/mapsetups/s[235]_*.asm",
        "function": {symbol: addresses[symbol] for symbol in FUNCTION_SYMBOLS},
        "summary": summary,
        "categorySummaries": category_summaries,
        "sourceMacroCounts": {
            category: value["sourceMacroCounts"] for category, value in categories.items()
        },
        "consumerFacts": _consumer_facts(setup),
        "entityEventReachabilityFacts": _entity_event_reachability_facts(
            disasm, addresses
        ),
        "directReturnStubs": direct_return_stubs,
        "rawZoneDefaultException": raw_zone_default,
        "selectionCases": selection_cases,
        "runtimeQuestions": [
            "entity-event-direct-return-stub-normal-story-route-reachability",
            "event-script-side-effects-and-transition-persistence",
            "event-portrait-facing-and-presentation-timing",
        ],
        "categories": categories,
    }


def verify_map_events_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_map_events_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="map events static contract")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != output["romSha256"]
        or fixture["function"] != output["function"]
    ):
        raise ValueError("map events provenance/address drift")
    for field in (
        "summary",
        "categorySummaries",
        "sourceMacroCounts",
        "consumerFacts",
        "entityEventReachabilityFacts",
        "directReturnStubs",
        "rawZoneDefaultException",
        "selectionCases",
        "runtimeQuestions",
    ):
        if fixture["expected"][field] != output[field]:
            raise ValueError(f"map events {field} drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"] or output["summary"] != manifest["summary"]:
        raise ValueError("map events canonical output drift")
    destination = output_path or repo_path("local/derived/map-events-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "SourceFiles": output["summary"]["sourceFileCount"],
        "UniqueTables": output["summary"]["uniqueTargetCount"],
        "PhysicalRecords": output["summary"]["physicalRecordCount"],
        "SetupReferences": output["summary"]["setupRecordReferenceCount"],
        "SelectionCases": output["summary"]["selectionCaseCount"],
        "Status": "PASS",
    }
