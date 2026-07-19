from __future__ import annotations

import hashlib
import json
import re
from collections import Counter, defaultdict
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.h2.map_setup import build_map_setup_contract
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.source_text import read_upstream_text

ID = "sf2-map-entities-static-v1"
SOURCE_ROOT = Path("data/maps/entries")
CONSUMER_PATH = Path("code/common/scripting/map/mapfunctions.asm")
MANIFEST = repo_path("manifests/extractions/map-entities-static.json")
SCHEMA = repo_path("schemas/map-entities-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/map-entities-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-map-entities-static-fixture.schema.json")

RECORD_MACROS = {
    "msFixedEntity": "fixed",
    "entity": "fixed",
    "msWalkingEntity": "walking",
    "entityRandomWalk": "walking",
    "msSequencedEntity": "sequenced",
}


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _record_kind(data: bytes) -> str:
    if len(data) != 8:
        raise ValueError("map entity record must contain eight bytes")
    if data[4] == 0xFF:
        return "walking"
    if data[4] == 0xFE:
        return "sequenced"
    return "fixed"


def _decode_record(address: int, data: bytes) -> dict[str, Any]:
    kind = _record_kind(data)
    decoded: dict[str, Any] = {
        "address": address,
        "kind": kind,
        "rawX": data[0],
        "rawY": data[1],
        "x": data[0] & 0x3F,
        "y": data[1] & 0x3F,
        "facing": data[2],
        "mapSprite": data[3],
    }
    if kind == "walking":
        decoded["walking"] = {
            "originX": data[5],
            "originY": data[6],
            "range": data[7],
        }
    else:
        decoded["actionValue"] = int.from_bytes(data[4:], "big")
    return decoded


def _source_record_rows(
    disasm: Path, addresses: dict[str, int]
) -> tuple[list[dict[str, Any]], dict[int, str], list[dict[str, Any]]]:
    paths = sorted(
        (
            path
            for path in (disasm / SOURCE_ROOT).rglob("s1_entities*.asm")
            if "mapsetups" in path.parts
        ),
        key=lambda path: path.as_posix(),
    )
    if len(paths) != 125:
        raise ValueError(f"map entity source boundary drift: expected 125 files, got {len(paths)}")
    files: list[dict[str, Any]] = []
    record_kinds: dict[int, str] = {}
    for path in paths:
        source = read_upstream_text(path)
        labels = re.findall(r"^([A-Za-z_][A-Za-z0-9_]*):", source, re.MULTILINE)
        if not labels or labels[0] not in addresses:
            raise ValueError(f"map entity source has no H1-bound entry label: {path}")
        symbol = labels[0]
        kinds: list[str] = []
        macro_counts: Counter[str] = Counter()
        for raw_line in source.splitlines():
            line = raw_line.split(";", 1)[0].strip()
            line = re.sub(r"^[A-Za-z_][A-Za-z0-9_]*:\s*", "", line)
            token_match = re.match(r"([A-Za-z_][A-Za-z0-9_]*)\b", line)
            if token_match and token_match.group(1) in RECORD_MACROS:
                token = token_match.group(1)
                kinds.append(RECORD_MACROS[token])
                macro_counts[token] += 1
        base_address = addresses[symbol]
        for index, kind in enumerate(kinds):
            address = base_address + index * 8
            if address in record_kinds:
                raise ValueError(f"overlapping source-owned map entity record at 0x{address:X}")
            record_kinds[address] = kind
        files.append(
            {
                "path": path.relative_to(disasm).as_posix(),
                "symbol": symbol,
                "address": base_address,
                "recordCount": len(kinds),
                "recordKinds": dict(sorted(Counter(kinds).items())),
                "macroCounts": dict(sorted(macro_counts.items())),
                "hasTerminator": bool(re.search(r"\bmsEntitiesEnd\b", source)),
            }
        )

    by_address = {row["address"]: row for row in files}
    fallthroughs: list[dict[str, Any]] = []
    for row in files:
        if row["hasTerminator"]:
            continue
        fallthrough_address = row["address"] + row["recordCount"] * 8
        target = by_address.get(fallthrough_address)
        if target is None:
            raise ValueError(f"unterminated entity fragment has no adjacent suffix: {row['path']}")
        fallthroughs.append(
            {
                "path": row["path"],
                "symbol": row["symbol"],
                "recordCount": row["recordCount"],
                "fallthroughAddress": fallthrough_address,
                "fallthroughSymbol": target["symbol"],
                "suffixRecordCount": target["recordCount"],
            }
        )
    return files, record_kinds, fallthroughs


def _consumer_facts(disasm: Path) -> dict[str, Any]:
    source = read_upstream_text(disasm / CONSUMER_PATH)
    fragments = (
        "InitializeMapEntities:",
        "mulu.w  #MAP_TILE_SIZE,d1",
        "mulu.w  #MAP_TILE_SIZE,d2",
        "move.b  (a0)+,d1",
        "cmpi.b  #-1,d1",
        "andi.w  #$3F,d1",
        "move.b  (a0)+,d2",
        "andi.w  #$3F,d2",
        "move.b  (a0)+,d3",
        "move.b  (a0)+,d4",
        "cmpi.b  #MAPSPRITES_SPECIALS_START,d4",
        "move.l  (a0)+,d5",
        "bsr.w   DeclareNewEntity",
    )
    position = -1
    for fragment in fragments:
        position = source.find(fragment, position + 1)
        if position < 0:
            raise ValueError(f"map entity consumer source-shape drift: {fragment!r}")
    return {
        "recordBytes": 8,
        "terminatorFirstByte": 0xFF,
        "coordinateMask": 0x3F,
        "coordinatesScaleByMapTileSize": True,
        "recordFieldOrder": ["x", "y", "facing", "mapSprite", "actionOrWalkingPayload"],
        "specialMapSpritesUseSpecialEntityDeclaration": True,
        "recordsAreDeclaredInStreamOrder": True,
    }


def build_map_entities_contract(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"map entity H1 listing is missing: {listing_path}")
    addresses = listing_symbol_addresses(listing_path.read_text(encoding="utf-8"))
    rom = rom_path.read_bytes()
    setup = build_map_setup_contract(rom_path, upstream_path)
    if setup["upstream"]["commit"] != commit:
        raise ValueError("map entity/setup provenance drift")

    source_files, source_record_kinds, fallthroughs = _source_record_rows(disasm, addresses)
    entity_targets = [table["targets"]["entities"] for table in setup["pointerTables"]]
    unique_targets = {target["symbol"]: target["address"] for target in entity_targets}
    source_symbols = {row["symbol"] for row in source_files}
    if set(unique_targets) != source_symbols:
        raise ValueError("map setup entity pointers do not own the complete entity source boundary")

    lists: list[dict[str, Any]] = []
    physical_records: dict[int, dict[str, Any]] = {}
    record_owners: defaultdict[int, list[str]] = defaultdict(list)
    terminator_addresses: set[int] = set()
    reference_kind_counts: Counter[str] = Counter()
    for symbol, address in sorted(unique_targets.items()):
        records: list[dict[str, Any]] = []
        cursor = address
        while rom[cursor] != 0xFF:
            raw = rom[cursor : cursor + 8]
            if len(raw) != 8 or len(records) >= 48:
                raise ValueError(f"map entity list has no bounded terminator: {symbol}")
            decoded = _decode_record(cursor, raw)
            expected_kind = source_record_kinds.get(cursor)
            if expected_kind is None or expected_kind != decoded["kind"]:
                raise ValueError(f"map entity source/ROM record drift at 0x{cursor:X}")
            existing = physical_records.setdefault(cursor, decoded)
            if existing != decoded:
                raise ValueError(f"conflicting map entity decode at 0x{cursor:X}")
            record_owners[cursor].append(symbol)
            reference_kind_counts[decoded["kind"]] += 1
            records.append(decoded)
            cursor += 8
        if rom[cursor : cursor + 2] != b"\xFF\xFF":
            raise ValueError(f"map entity source terminator drift: {symbol}")
        terminator_addresses.add(cursor)
        lists.append(
            {
                "symbol": symbol,
                "address": address,
                "recordCount": len(records),
                "terminatorAddress": cursor,
                "records": records,
            }
        )
    if set(physical_records) != set(source_record_kinds):
        raise ValueError("map entity source records are not exactly covered by setup list streams")

    source_macro_counts: Counter[str] = Counter()
    for row in source_files:
        source_macro_counts.update(row["macroCounts"])
    physical_kind_counts = Counter(record["kind"] for record in physical_records.values())
    shared_record_references = sum(len(owners) - 1 for owners in record_owners.values())
    summary = {
        "sourceFileCount": len(source_files),
        "entityPointerReferenceCount": len(entity_targets),
        "uniqueEntityListCount": len(unique_targets),
        "sourcePhysicalRecordCount": len(source_record_kinds),
        "listRecordReferenceCount": sum(row["recordCount"] for row in lists),
        "sharedSuffixRecordReferenceCount": shared_record_references,
        "uniqueTerminatorAddressCount": len(terminator_addresses),
        "unterminatedSourceFragmentCount": len(fallthroughs),
        "terminatorOnlySuffixCount": sum(row["suffixRecordCount"] == 0 for row in fallthroughs),
        "recordBearingSuffixCount": sum(row["suffixRecordCount"] > 0 for row in fallthroughs),
        "emptyListCount": sum(row["recordCount"] == 0 for row in lists),
        "maximumListRecordCount": max(row["recordCount"] for row in lists),
        "fixedPhysicalRecordCount": physical_kind_counts["fixed"],
        "walkingPhysicalRecordCount": physical_kind_counts["walking"],
        "sequencedPhysicalRecordCount": physical_kind_counts["sequenced"],
        "fixedRecordReferenceCount": reference_kind_counts["fixed"],
        "walkingRecordReferenceCount": reference_kind_counts["walking"],
        "sequencedRecordReferenceCount": reference_kind_counts["sequenced"],
    }
    duplicate_pointer_targets = sorted(
        symbol for symbol, count in Counter(target["symbol"] for target in entity_targets).items()
        if count > 1
    )
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "romSha256": setup["romSha256"],
        "scope": f"{SOURCE_ROOT.as_posix()}/*/mapsetups/s1_entities*.asm",
        "function": {"InitializeMapEntities": addresses["InitializeMapEntities"]},
        "summary": summary,
        "sourceMacroCounts": dict(sorted(source_macro_counts.items())),
        "physicalRecordKinds": dict(sorted(physical_kind_counts.items())),
        "referenceRecordKinds": dict(sorted(reference_kind_counts.items())),
        "duplicatePointerTargets": duplicate_pointer_targets,
        "fallthroughFragments": fallthroughs,
        "consumerFacts": _consumer_facts(disasm),
        "runtimeQuestions": [
            "sequenced-entity-orientation-stream-consumption",
            "follower-and-map-entity-declaration-collision-state",
            "walking-special-sprite-and-entity-presentation-timing",
        ],
        "sourceFiles": source_files,
        "lists": lists,
    }


def verify_map_entities_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_map_entities_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="map entities static contract")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != output["romSha256"]
        or fixture["function"] != output["function"]
    ):
        raise ValueError("map entities provenance/address drift")
    for field in (
        "summary",
        "sourceMacroCounts",
        "physicalRecordKinds",
        "referenceRecordKinds",
        "duplicatePointerTargets",
        "fallthroughFragments",
        "consumerFacts",
        "runtimeQuestions",
    ):
        if fixture["expected"][field] != output[field]:
            raise ValueError(f"map entities {field} drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"] or output["summary"] != manifest["summary"]:
        raise ValueError("map entities canonical output drift")
    destination = output_path or repo_path("local/derived/map-entities-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "SourceFiles": output["summary"]["sourceFileCount"],
        "EntityLists": output["summary"]["uniqueEntityListCount"],
        "PhysicalRecords": output["summary"]["sourcePhysicalRecordCount"],
        "ListReferences": output["summary"]["listRecordReferenceCount"],
        "FallthroughFragments": output["summary"]["unterminatedSourceFragmentCount"],
        "Status": "PASS",
    }
