from __future__ import annotations

import hashlib
import json
import re
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_ai import _parse_source_file
from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.source_text import read_upstream_text

ID = "sf2-map-data-static-v1"
SOURCE_ROOT = Path("data/maps")
ENTRIES_ROOT = SOURCE_ROOT / "entries"
ENTRIES_PATH = SOURCE_ROOT / "entries.asm"
SETUPS_PATH = SOURCE_ROOT / "mapsetups.asm"
STORAGE_PATH = SOURCE_ROOT / "mapsetupsstorage.asm"
MANIFEST = repo_path("manifests/extractions/map-data-static.json")
SCHEMA = repo_path("schemas/map-data-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/map-data-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-map-data-static-fixture.schema.json")
RESEARCH_INDEX = repo_path("manifests/research-index.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")
TOOLCHAIN = repo_path("manifests/toolchain.json")


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _includes(source: str) -> list[tuple[str | None, str]]:
    return [
        (
            match.group("label"),
            match.group("path").replace("\\", "/"),
        )
        for match in re.finditer(
            r'^\s*(?:(?P<label>[A-Za-z_][A-Za-z0-9_]*):)?\s*'
            r'include\s+"(?P<path>data\\maps\\[^"]+)"',
            source,
            re.MULTILINE,
        )
    ]


def _statement_count(source: str, token: str) -> int:
    return len(
        re.findall(
            rf"^\s*(?:[A-Za-z_][A-Za-z0-9_]*:\s*)?{re.escape(token)}\b",
            source,
            re.MULTILINE,
        )
    )


def _index_records_for_map_data_scope() -> dict[str, Any]:
    """Join every research record whose source path belongs to ``data/maps/**``.

    The source-path prefix is the sole membership rule.  The later inventory
    invariant separately proves that every selected path is one of the complete
    discovered map ASM sources; record IDs, subsystem labels, status, documents,
    and evidence do not participate in this join.
    """
    prefix = f"{SOURCE_ROOT.as_posix()}/"
    records_by_source_path: dict[str, list[str]] = {}
    for record in load_json(RESEARCH_INDEX)["records"]:
        source_path = record["sourcePath"]
        if source_path.startswith(prefix):
            records_by_source_path.setdefault(source_path, []).append(record["id"])

    relation = [
        {"sourcePath": source_path, "recordIds": sorted(record_ids)}
        for source_path, record_ids in sorted(records_by_source_path.items())
    ]
    flattened_ids = [record_id for row in relation for record_id in row["recordIds"]]
    if len(flattened_ids) != len(set(flattened_ids)):
        raise ValueError("map data research-index duplicate record ID")
    return {
        "indexedRecordIds": sorted(flattened_ids),
        "indexedSourcePaths": [row["sourcePath"] for row in relation],
        "indexedRecordsBySourcePath": relation,
    }


def _verify_indexed_record_join(
    output: dict[str, Any], *, discovered_source_paths: set[str]
) -> None:
    """Prove the three public index views describe one sorted source-path join."""
    relation = output["indexedRecordsBySourcePath"]
    relation_source_paths = [row["sourcePath"] for row in relation]
    relation_record_ids = [
        record_id for row in relation for record_id in row["recordIds"]
    ]
    if relation_source_paths != sorted(relation_source_paths):
        raise ValueError("map data indexed relation source-path order drift")
    if any(row["recordIds"] != sorted(row["recordIds"]) for row in relation):
        raise ValueError("map data indexed relation record-ID order drift")
    if len(relation_source_paths) != len(set(relation_source_paths)):
        raise ValueError("map data indexed relation duplicate source path")
    if len(relation_record_ids) != len(set(relation_record_ids)):
        raise ValueError("map data indexed relation duplicate record ID")
    if any(
        not path.startswith(f"{SOURCE_ROOT.as_posix()}/")
        for path in relation_source_paths
    ):
        raise ValueError("map data indexed relation outside source scope")
    unknown_paths = sorted(set(relation_source_paths) - discovered_source_paths)
    if unknown_paths:
        raise ValueError(
            "map data indexed relation source path is absent from the discovered inventory: "
            f"{unknown_paths[:3]}"
        )

    if output["indexedRecordIds"] != sorted(relation_record_ids):
        raise ValueError("map data indexedRecordIds relation drift")
    if output["indexedSourcePaths"] != relation_source_paths:
        raise ValueError("map data indexedSourcePaths relation order drift")
    summary = output["summary"]
    if summary["indexedRecordCount"] != len(output["indexedRecordIds"]) or summary[
        "indexedRecordCount"
    ] != len(relation_record_ids):
        raise ValueError("map data summary indexedRecordCount relation drift")
    if summary["indexedFileCount"] != len(output["indexedSourcePaths"]) or summary[
        "indexedFileCount"
    ] != len(relation_source_paths):
        raise ValueError("map data summary indexedFileCount relation drift")


def build_map_data_inventory(upstream_path: Path) -> dict[str, Any]:
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"map-data H1 listing is missing: {listing_path}")
    listing_addresses = listing_symbol_addresses(listing_path.read_text(encoding="utf-8"))
    root = disasm / SOURCE_ROOT
    paths = sorted(root.rglob("*.asm"))
    if len(paths) != 1390:
        raise ValueError(f"map data boundary drift: expected 1390 files, got {len(paths)}")
    files = [_parse_source_file(path, path.relative_to(disasm).as_posix()) for path in paths]
    rows_by_path = {row["path"]: row for row in files}
    sources = {
        path.relative_to(disasm).as_posix(): read_upstream_text(path) for path in paths
    }

    layout = "\n".join(
        path.read_text(encoding="utf-8") for path in sorted((disasm / "layout").glob("*.asm"))
    )
    direct_paths = sorted(
        set(
            match.replace("\\", "/")
            for match in re.findall(r'include\s+"(data\\maps\\[^"]+)"', layout)
        )
    )
    if len(direct_paths) != 8 or set(direct_paths) - set(sources):
        raise ValueError("map data direct layout boundary drift")

    include_edges = [
        {"sourcePath": source_path, "label": label, "targetPath": target}
        for source_path, source in sources.items()
        for label, target in _includes(source)
    ]
    targets = [edge["targetPath"] for edge in include_edges]
    if len(include_edges) != 1382 or len(set(targets)) != 1382:
        raise ValueError("map data include graph boundary drift")
    if set(targets) - set(sources):
        raise ValueError("map data include graph references a source outside the inventory")

    outgoing: dict[str, list[str]] = {}
    for edge in include_edges:
        outgoing.setdefault(edge["sourcePath"], []).append(edge["targetPath"])
    reachable = set(direct_paths)
    queue = list(direct_paths)
    while queue:
        for target in outgoing.get(queue.pop(), []):
            if target not in reachable:
                reachable.add(target)
                queue.append(target)
    if reachable != set(sources):
        raise ValueError("map data include graph does not reach the complete directory")

    representative_symbols = {
        path: row["globalLabels"][0]
        for path, row in rows_by_path.items()
        if row["globalLabels"]
    }
    if len(representative_symbols) != 727:
        raise ValueError("map data internally labeled file boundary drift")
    missing_symbols = sorted(set(representative_symbols.values()) - set(listing_addresses))
    if missing_symbols:
        raise ValueError(f"map data symbols absent from H1 listing: {missing_symbols[:5]}")
    representative_addresses = {
        symbol: listing_addresses[symbol] for symbol in representative_symbols.values()
    }
    indexed_records = _index_records_for_map_data_scope()

    include_site_only_paths = sorted(
        edge["targetPath"]
        for edge in include_edges
        if edge["label"] and not rows_by_path[edge["targetPath"]]["globalLabels"]
    )
    if len(include_site_only_paths) != 662:
        raise ValueError("map data include-site-only boundary drift")
    unlabeled_container_paths = sorted(
        path
        for path, row in rows_by_path.items()
        if not row["globalLabels"] and path not in include_site_only_paths
    )
    if unlabeled_container_paths != [STORAGE_PATH.as_posix()]:
        raise ValueError("map data unlabeled container boundary drift")

    entries_source = sources[ENTRIES_PATH.as_posix()]
    map_pointers = re.findall(
        r"^\s*(?:pt_MapData:\s*)?dc\.l\s+Map\d{2}\s*$",
        entries_source,
        re.MULTILINE,
    )
    if len(map_pointers) != 79:
        raise ValueError("map pointer slot boundary drift")
    setup_paths = sorted(path for path in sources if "/mapsetups/" in path)
    map_content_paths = sorted(
        path
        for path in sources
        if path.startswith(f"{ENTRIES_ROOT.as_posix()}/") and "/mapsetups/" not in path
    )
    if len(setup_paths) != 720 or len(map_content_paths) != 662:
        raise ValueError("map content/setup file classification drift")

    global_facts = {
        "debugModeMapSlots": _statement_count(
            sources[f"{SOURCE_ROOT.as_posix()}/global/debugmodemaps.asm"], "dc.b"
        ),
        "flagSwitchedMapRows": _statement_count(
            sources[f"{SOURCE_ROOT.as_posix()}/global/flagswitchedmaps.asm"],
            "flagSwitchedMap",
        ),
        "overworldMapRows": _statement_count(
            sources[f"{SOURCE_ROOT.as_posix()}/global/overworldmaps.asm"], "dc.b"
        ),
        "raftResetRows": _statement_count(
            sources[f"{SOURCE_ROOT.as_posix()}/global/raftresetmapcoords.asm"],
            "raftResetMapCoordinates",
        ),
        "savePointRows": _statement_count(
            sources[f"{SOURCE_ROOT.as_posix()}/global/savepointmapcoords.asm"],
            "savePointMapCoordinates",
        ),
    }
    setup_source = sources[SETUPS_PATH.as_posix()]
    summary = {
        "fileCount": len(files),
        "sourceLineCount": sum(row["sourceLineCount"] for row in files),
        "statementCount": sum(row["statementCount"] for row in files),
        "globalLabelCount": sum(len(row["globalLabels"]) for row in files),
        "directLayoutFileCount": len(direct_paths),
        "transitiveIncludeFileCount": len(set(targets)),
        "internallyLabeledFileCount": len(representative_symbols),
        "includeSiteOnlyFileCount": len(include_site_only_paths),
        "unlabeledContainerFileCount": len(unlabeled_container_paths),
        "representativeAddressCount": len(representative_addresses),
        "indexedRecordCount": len(indexed_records["indexedRecordIds"]),
        "indexedFileCount": len(indexed_records["indexedSourcePaths"]),
    }
    output = {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "scope": SOURCE_ROOT.as_posix(),
        "summary": summary,
        "directLayoutPaths": direct_paths,
        "includeEdges": include_edges,
        "strictReach": {
            "internallyLabeledPaths": sorted(representative_symbols),
            "includeSiteOnlyPaths": include_site_only_paths,
            "unlabeledContainerPaths": unlabeled_container_paths,
        },
        **indexed_records,
        "representativeSymbols": representative_symbols,
        "representativeAddresses": representative_addresses,
        "facts": {
            "mapSlotCount": len(map_pointers),
            "mapDirectoryCount": sum(path.is_dir() for path in (disasm / ENTRIES_ROOT).iterdir()),
            "mapContentFileCount": len(map_content_paths),
            "mapSetupFileCount": len(setup_paths),
            "includeGraph": {
                "edgeCount": len(include_edges),
                "uniqueTargetCount": len(set(targets)),
                "labeledEdgeCount": sum(edge["label"] is not None for edge in include_edges),
                "unlabeledEdgeCount": sum(edge["label"] is None for edge in include_edges),
            },
            "entryBinaryPayloadCount": _statement_count(entries_source, "incbin"),
            "mapSetupRouting": {
                "mapRows": _statement_count(setup_source, "msMap"),
                "flagRows": _statement_count(setup_source, "msFlag"),
                "mapEndRows": _statement_count(setup_source, "msMapEnd"),
            },
            "globalTables": global_facts,
            "mapContentParsed": True,
            "mapBinaryPayloadsDecoded": True,
            "canonicalMapImportBuilt": True,
            "mapSetupSemanticsParsed": True,
            "mapEventTablesParsed": True,
            "mapDescriptionTablesParsed": True,
            "mapInitFunctionsParsed": True,
            "standaloneMapScriptsParsed": True,
        },
        "runtimeQuestions": [
            "area-description-byte2-d6-condition-meaning",
            "map-transition-state-persistence-and-roof-step-warp-precedence",
        ],
        "files": files,
    }
    _verify_indexed_record_join(output, discovered_source_paths=set(sources))
    return output


def verify_map_data_inventory(
    upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_map_data_inventory(upstream_path)
    validate_json(output, SCHEMA, owner="map data static inventory")
    _verify_indexed_record_join(
        output,
        discovered_source_paths={row["path"] for row in output["files"]},
    )
    authoritative_indexed_records = _index_records_for_map_data_scope()
    for field in (
        "indexedRecordIds",
        "indexedSourcePaths",
        "indexedRecordsBySourcePath",
    ):
        if output[field] != authoritative_indexed_records[field]:
            raise ValueError(f"map data current research-index {field} drift")

    toolchain = load_json(TOOLCHAIN)
    if output["upstream"]["repository"] != toolchain["sf2disasm"]["repository"]:
        raise ValueError("map data upstream repository provenance drift")
    if output["upstream"]["commit"] != toolchain["sf2disasm"]["commit"]:
        raise ValueError("map data upstream commit provenance drift")
    if fixture["upstreamCommit"] != toolchain["sf2disasm"]["commit"]:
        raise ValueError("map data fixture upstream provenance drift")
    if fixture["romSha256"] != load_json(ROM_MANIFEST)["hashes"]["sha256"]:
        raise ValueError("map data fixture ROM provenance drift")
    if output["representativeAddresses"] != fixture["table"]:
        raise ValueError("map data H1 address drift")
    expected = fixture["expected"]
    source_inventory = expected["sourceInventory"]
    for field in (
        "directLayoutPaths",
        "includeEdges",
        "strictReach",
        "representativeSymbols",
    ):
        if output[field] != source_inventory[field]:
            raise ValueError(f"map data source inventory {field} drift")
    for field in (
        "indexedRecordIds",
        "indexedSourcePaths",
        "indexedRecordsBySourcePath",
        "facts",
        "runtimeQuestions",
    ):
        if output[field] != expected[field]:
            raise ValueError(f"map data fixture {field} drift")
    if output["summary"] != manifest["summary"]:
        raise ValueError("map data summary drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"]:
        raise ValueError("map data canonical hash drift")
    destination = output_path or repo_path("local/derived/map-data-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Inventory": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Files": output["summary"]["fileCount"],
        "InternallyLabeledFiles": output["summary"]["internallyLabeledFileCount"],
        "IncludeSiteOnlyFiles": output["summary"]["includeSiteOnlyFileCount"],
        "IndexedFiles": output["summary"]["indexedFileCount"],
        "Status": "PASS",
    }
