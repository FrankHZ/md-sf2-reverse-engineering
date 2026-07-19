from __future__ import annotations

import hashlib
import json
import subprocess
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_ai import _parse_source_file
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path

ID = "sf2-battlefield-static-v1"
SOURCE_ROOT = Path("code/gameflow/battle/battlefield")
MANIFEST = repo_path("manifests/extractions/battlefield-static.json")
SCHEMA = repo_path("schemas/battlefield-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/battlefield-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-battlefield-static-fixture.schema.json")
TOOLCHAIN = repo_path("manifests/toolchain.json")
RESEARCH_INDEX = repo_path("manifests/research-index.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")

REPRESENTATIVE_SYMBOLS = {
    "battlefieldengine.asm": "ConvertCoordinatesToAddress",
    "buildactionrangegrids.asm": "BuildMovementRangeGrid",
    "buildmovementarrays.asm": "BuildMovementArrays",
    "buildmovestringfunctions.asm": "BuildCancelMoveString",
    "buildtargetsarray.asm": "BuildTargetsArrayWithTeammatesOfTarget",
    "checkfortrappedchest.asm": "CheckForTrappedChest",
    "determineattackposition.asm": "DetermineAttackPosition",
    "determineattackpositionformoveorder.asm": "DetermineAttackPositionForMoveOrder",
    "getactionrange.asm": "GetAttackRange",
    "getcurrentterraintypeformoveorder.asm": "GetCurrentTerrainTypeForMoveOrder",
    "getmoveorderposition.asm": "GetMoveOrderPosition",
    "getmovestringdestination.asm": "GetMoveStringDestination",
    "getreachabletargets.asm": "GetTargetsReachableByAttack",
    "initializemovementarrays.asm": "InitializeMovementArrays",
    "populatetargetslist.asm": "PopulateTargetsListForItemUse",
    "updatemovablegrid.asm": "UpdateMovableGrid",
    "updateoccupiedterrainfunctions.asm": "UpdateOccupiedByOpponentsTerrain",
}


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _resolve_upstream(upstream_path: Path) -> tuple[Path, str, dict[str, Any]]:
    upstream_path = upstream_path.resolve(strict=True)
    toolchain = load_json(TOOLCHAIN)
    commit = subprocess.run(
        ["git", "rev-parse", "HEAD"],
        cwd=upstream_path,
        check=True,
        capture_output=True,
        text=True,
        encoding="utf-8",
    ).stdout.strip()
    expected = toolchain["sf2disasm"]["commit"]
    if commit != expected:
        raise ValueError(f"battlefield inventory requires SF2DISASM {expected}, got {commit}")
    disasm = upstream_path / "disasm"
    if not (disasm / SOURCE_ROOT).is_dir():
        raise ValueError(f"battlefield source root is missing: {disasm / SOURCE_ROOT}")
    return disasm, commit, toolchain


def build_battlefield_inventory(upstream_path: Path) -> dict[str, Any]:
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    source_root = disasm / SOURCE_ROOT
    source_paths = sorted(source_root.rglob("*.asm"), key=lambda path: path.as_posix())
    files = [_parse_source_file(path, path.relative_to(disasm).as_posix()) for path in source_paths]
    if {Path(row["path"]).name for row in files} != set(REPRESENTATIVE_SYMBOLS):
        raise ValueError("battlefield source file set drift")

    all_labels = {label for row in files for label in row["globalLabels"]}
    direct_calls: Counter[str] = Counter()
    for row in files:
        for call in row["directCalls"]:
            direct_calls[call["target"]] += call["siteCount"]

    index = load_json(RESEARCH_INDEX)
    indexed_records = sorted(
        record["id"]
        for record in index["records"]
        if Path(record["sourcePath"]).is_relative_to(SOURCE_ROOT)
    )
    indexed_files = sorted(
        {
            record["sourcePath"]
            for record in index["records"]
            if Path(record["sourcePath"]).is_relative_to(SOURCE_ROOT)
        }
    )
    summary = {
        "fileCount": len(files),
        "sourceLineCount": sum(row["sourceLineCount"] for row in files),
        "statementCount": sum(row["statementCount"] for row in files),
        "globalLabelCount": sum(len(row["globalLabels"]) for row in files),
        "localLabelCount": sum(row["localLabelCount"] for row in files),
        "directCallSiteCount": sum(direct_calls.values()),
        "indirectCallSiteCount": sum(row["indirectCallSiteCount"] for row in files),
        "uniqueDirectTargetCount": len(direct_calls),
        "internalDirectTargetCount": sum(target in all_labels for target in direct_calls),
        "externalDirectTargetCount": sum(target not in all_labels for target in direct_calls),
        "indexedRecordCount": len(indexed_records),
        "indexedFileCount": len(indexed_files),
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {
            "repository": toolchain["sf2disasm"]["repository"],
            "commit": commit,
        },
        "scope": SOURCE_ROOT.as_posix(),
        "summary": summary,
        "indexedRecordIds": indexed_records,
        "indexedSourcePaths": indexed_files,
        "internalDirectCallTargets": sorted(
            target for target in direct_calls if target in all_labels
        ),
        "externalDirectCallTargets": sorted(
            target for target in direct_calls if target not in all_labels
        ),
        "files": files,
    }


def verify_battlefield_inventory(
    upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    rom_manifest = load_json(ROM_MANIFEST)
    output = build_battlefield_inventory(upstream_path)
    validate_json(output, SCHEMA, owner="battlefield static inventory")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != rom_manifest["hashes"]["sha256"]
    ):
        raise ValueError("battlefield fixture provenance drift")
    if output["summary"] != manifest["summary"]:
        raise ValueError(
            f"battlefield static summary drift: expected {manifest['summary']}, "
            f"got {output['summary']}"
        )
    by_name = {Path(row["path"]).name: row for row in output["files"]}
    for filename, symbol in fixture["expected"]["representativeSymbols"].items():
        if symbol not in by_name[filename]["globalLabels"]:
            raise ValueError(f"battlefield representative symbol drift: {filename}::{symbol}")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"]:
        raise ValueError(
            f"battlefield static inventory hash mismatch: expected "
            f"{manifest['outputSha256']}, got {digest}"
        )
    destination = output_path or repo_path("local/derived/battlefield-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Inventory": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Files": output["summary"]["fileCount"],
        "GlobalLabels": output["summary"]["globalLabelCount"],
        "DirectCallSites": output["summary"]["directCallSiteCount"],
        "IndexedRecords": output["summary"]["indexedRecordCount"],
        "Status": "PASS",
    }
