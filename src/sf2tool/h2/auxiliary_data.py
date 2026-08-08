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

ID = "sf2-auxiliary-data-static-v1"
ROOT_PATHS = (Path("data/graphics"), Path("data/scripting"), Path("data/tech"))
SPRITE_DIALOG_PATH = Path("data/spritedialogproperties.asm")
WINDOW_BORDER_ALTERNATE = Path("data/graphics/tech/windowborder/entries.asm")
FIGHTER_MINI_ALTERNATE = Path(
    "data/graphics/tech/windowlayouts/fighterministatuswindowlayout.asm"
)
MANIFEST = repo_path("manifests/extractions/auxiliary-data-static.json")
SCHEMA = repo_path("schemas/auxiliary-data-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/auxiliary-data-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-auxiliary-data-static-fixture.schema.json")
RESEARCH_INDEX = repo_path("manifests/research-index.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _statement_count(source: str, token: str) -> int:
    return len(
        re.findall(
            rf"^\s*(?:[A-Za-z_][A-Za-z0-9_]*:\s*)?{re.escape(token)}\b",
            source,
            re.MULTILINE,
        )
    )


def _index_records_for_auxiliary_scope(source_paths: set[str]) -> dict[str, Any]:
    """Join every research-index record to the discovered auxiliary source surface.

    The auxiliary owner is the complete source inventory, not a subsystem prefix or
    a fixed record-ID list.  A source path is therefore selected only when it is an
    exact member of the 65-file inventory discovered from the pinned upstream tree.
    """
    records_by_source_path: dict[str, list[str]] = {}
    for record in load_json(RESEARCH_INDEX)["records"]:
        source_path = record["sourcePath"]
        if source_path not in source_paths:
            continue
        records_by_source_path.setdefault(source_path, []).append(record["id"])

    indexed_records_by_source_path = [
        {"sourcePath": source_path, "recordIds": sorted(record_ids)}
        for source_path, record_ids in sorted(records_by_source_path.items())
    ]
    indexed_record_ids = [
        record_id
        for row in indexed_records_by_source_path
        for record_id in row["recordIds"]
    ]
    if len(indexed_record_ids) != len(set(indexed_record_ids)):
        raise ValueError("auxiliary data research-index duplicate record ID")
    return {
        "indexedRecordIds": sorted(indexed_record_ids),
        "indexedSourcePaths": [
            row["sourcePath"] for row in indexed_records_by_source_path
        ],
        "indexedRecordsBySourcePath": indexed_records_by_source_path,
    }


def _verify_indexed_record_join(output: dict[str, Any]) -> None:
    """Reject schema-valid drift between the auxiliary join's related fields."""
    relation = output["indexedRecordsBySourcePath"]
    relation_source_paths = [row["sourcePath"] for row in relation]
    relation_record_ids = [record_id for row in relation for record_id in row["recordIds"]]
    if len(relation_source_paths) != len(set(relation_source_paths)):
        raise ValueError("auxiliary data indexed relation duplicate source path")
    if len(relation_record_ids) != len(set(relation_record_ids)):
        raise ValueError("auxiliary data indexed relation duplicate record ID")

    indexed_record_ids = output["indexedRecordIds"]
    indexed_source_paths = output["indexedSourcePaths"]
    if indexed_record_ids != sorted(relation_record_ids):
        raise ValueError("auxiliary data indexedRecordIds relation drift")
    if indexed_source_paths != relation_source_paths:
        raise ValueError("auxiliary data indexedSourcePaths relation order drift")

    summary = output["summary"]
    if summary["indexedRecordCount"] != len(indexed_record_ids) or summary[
        "indexedRecordCount"
    ] != len(relation_record_ids):
        raise ValueError("auxiliary data summary indexedRecordCount relation drift")
    if summary["indexedFileCount"] != len(indexed_source_paths) or summary[
        "indexedFileCount"
    ] != len(relation_source_paths):
        raise ValueError("auxiliary data summary indexedFileCount relation drift")


def build_auxiliary_data_inventory(upstream_path: Path) -> dict[str, Any]:
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"auxiliary-data H1 listing is missing: {listing_path}")
    listing_addresses = listing_symbol_addresses(listing_path.read_text(encoding="utf-8"))
    paths = sorted(
        [path for root in ROOT_PATHS for path in (disasm / root).rglob("*.asm")]
        + [disasm / SPRITE_DIALOG_PATH]
    )
    if len(paths) != 65:
        raise ValueError(f"auxiliary data boundary drift: expected 65 files, got {len(paths)}")
    files = [_parse_source_file(path, path.relative_to(disasm).as_posix()) for path in paths]
    rows_by_path = {row["path"]: row for row in files}
    sources = {
        path.relative_to(disasm).as_posix(): read_upstream_text(path) for path in paths
    }
    if any(not row["globalLabels"] for row in files):
        raise ValueError("auxiliary data unexpectedly contains an unlabeled ASM file")

    layout = "\n".join(
        path.read_text(encoding="utf-8") for path in sorted((disasm / "layout").glob("*.asm"))
    )
    layout_owned_paths = sorted(
        path for path in sources if path.replace("/", "\\") in layout
    )
    excluded_paths = sorted(set(sources) - set(layout_owned_paths))
    expected_excluded = sorted(
        [WINDOW_BORDER_ALTERNATE.as_posix(), FIGHTER_MINI_ALTERNATE.as_posix()]
    )
    if len(layout_owned_paths) != 63 or excluded_paths != expected_excluded:
        raise ValueError("auxiliary data layout ownership drift")

    representative_symbols: dict[str, str] = {}
    for path in layout_owned_paths:
        candidates = [
            symbol
            for symbol in rows_by_path[path]["globalLabels"]
            if symbol in listing_addresses
        ]
        if not candidates:
            raise ValueError(f"auxiliary data has no H1-listed symbol: {path}")
        representative_symbols[path] = candidates[0]
    representative_addresses = {
        symbol: listing_addresses[symbol] for symbol in representative_symbols.values()
    }
    if len(representative_addresses) != 63:
        raise ValueError("auxiliary data representative symbol collision")
    indexed_records = _index_records_for_auxiliary_scope(set(sources))

    incbin_targets = [
        match.replace("\\", "/")
        for source in sources.values()
        for match in re.findall(
            r'^\s*(?:[A-Za-z_][A-Za-z0-9_]*:\s*)?incbin\s+"([^"]+)"',
            source,
            re.MULTILINE,
        )
    ]
    sprite_dialog_source = sources[SPRITE_DIALOG_PATH.as_posix()]
    dialog_counts = {
        token: _statement_count(sprite_dialog_source, token)
        for token in ("mapsprite", "portrait", "speechSfx")
    }
    if set(dialog_counts.values()) != {119}:
        raise ValueError("sprite dialogue property row alignment drift")
    category_counts = {
        root.name: sum(path.startswith(f"{root.as_posix()}/") for path in sources)
        for root in ROOT_PATHS
    }
    category_counts["root"] = 1

    summary = {
        "fileCount": len(files),
        "sourceLineCount": sum(row["sourceLineCount"] for row in files),
        "statementCount": sum(row["statementCount"] for row in files),
        "globalLabelCount": sum(len(row["globalLabels"]) for row in files),
        "layoutOwnedFileCount": len(layout_owned_paths),
        "alternateFileCount": len(excluded_paths),
        "representativeAddressCount": len(representative_addresses),
        "indexedRecordCount": len(indexed_records["indexedRecordIds"]),
        "indexedFileCount": len(indexed_records["indexedSourcePaths"]),
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "scope": [root.as_posix() for root in ROOT_PATHS] + [SPRITE_DIALOG_PATH.as_posix()],
        "summary": summary,
        "inventoryPaths": sorted(sources),
        "layoutOwnedPaths": layout_owned_paths,
        "excludedPaths": excluded_paths,
        "exclusions": {
            WINDOW_BORDER_ALTERNATE.as_posix(): (
                "unassembled compressed-window-border aggregate with no H1 symbol"
            ),
            FIGHTER_MINI_ALTERNATE.as_posix(): (
                "unassembled alternate whose first symbol is owned by the built mini-status layout"
            ),
        },
        **indexed_records,
        "representativeSymbols": representative_symbols,
        "representativeAddresses": representative_addresses,
        "facts": {
            "categoryFileCounts": category_counts,
            "privateIncbinReferenceCount": len(incbin_targets),
            "uniquePrivateIncbinTargetCount": len(set(incbin_targets)),
            "spriteDialoguePropertyRows": 119,
            "spriteDialogueDirectiveCounts": dialog_counts,
            "graphicsPayloadsParsed": False,
            "scriptContentParsed": False,
        },
        "runtimeQuestions": [
            "window-layout-and-vdp-presentation-behavior",
            "map-and-battle-sprite-animation-frame-timing",
            "entity-action-and-global-cutscene-dispatch-effects",
            "configuration-debug-and-fading-data-runtime-consumers",
        ],
        "files": files,
    }


def verify_auxiliary_data_inventory(
    upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_auxiliary_data_inventory(upstream_path)
    validate_json(output, SCHEMA, owner="auxiliary data static inventory")
    _verify_indexed_record_join(output)
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != load_json(ROM_MANIFEST)["hashes"]["sha256"]
    ):
        raise ValueError("auxiliary data provenance drift")
    if output["summary"] != manifest["summary"]:
        raise ValueError("auxiliary data summary drift")
    if output["representativeAddresses"] != fixture["table"]:
        raise ValueError("auxiliary data H1 address drift")
    for field in ("indexedRecordsBySourcePath", "facts", "runtimeQuestions"):
        if output[field] != fixture["expected"][field]:
            raise ValueError(f"auxiliary data {field} drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"]:
        raise ValueError("auxiliary data canonical hash drift")
    destination = output_path or repo_path("local/derived/auxiliary-data-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Inventory": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Files": output["summary"]["fileCount"],
        "LayoutOwnedFiles": output["summary"]["layoutOwnedFileCount"],
        "ExcludedFiles": len(output["excludedPaths"]),
        "IndexedFiles": output["summary"]["indexedFileCount"],
        "Status": "PASS",
    }
