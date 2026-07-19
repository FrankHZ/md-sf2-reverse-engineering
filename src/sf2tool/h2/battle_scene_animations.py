from __future__ import annotations

import hashlib
import json
import re
from collections import Counter, defaultdict
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_ai import _parse_source_file
from sf2tool.h2.battle_scene_engine import _jump_table, _resolve_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path

ID = "sf2-battle-scene-animations-static-v1"
SOURCE_ROOT = Path("code/gameflow/battle/battlescenes/animation")
SCENE_ROOT = SOURCE_ROOT.parent
MANIFEST = repo_path("manifests/extractions/battle-scene-animations-static.json")
SCHEMA = repo_path("schemas/battle-scene-animations-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/battle-scene-animations-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-battle-scene-animations-static-fixture.schema.json")
RESEARCH_INDEX = repo_path("manifests/research-index.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _listing_address(listing: str, symbol: str) -> int:
    match = re.search(
        rf"^(?P<address>[0-9A-F]{{8}})(?:[ \t]+[0-9A-F]{{2,8}})*"
        rf"[ \t]+{re.escape(symbol)}:",
        listing,
        re.MULTILINE,
    )
    if not match:
        raise ValueError(f"battle-scene animation symbol is absent from H1 listing: {symbol}")
    return int(match.group("address"), 16)


def _dispatch_facts(disasm: Path, label_paths: dict[str, str]) -> dict[str, Any]:
    root = disasm / SCENE_ROOT
    setup_targets = _jump_table(root / "battlesceneengine_2.asm", "rjt_SpellanimationSetups")
    update_targets = _jump_table(root / "updatespellanimation.asm", "rjt_SpellanimationUpdates")
    root_update_path = (SCENE_ROOT / "updatespellanimation.asm").as_posix()
    root_update_labels = set(
        _parse_source_file(root / "updatespellanimation.asm", root_update_path)["globalLabels"]
    )
    pairs: list[dict[str, Any]] = []
    for index, (setup, update) in enumerate(zip(setup_targets, update_targets, strict=True)):
        update_path = root_update_path if update in root_update_labels else label_paths[update]
        pairs.append(
            {
                "index": index,
                "setupTarget": setup,
                "setupPath": label_paths[setup],
                "updateTarget": update,
                "updatePath": update_path,
            }
        )
    setup_uses: dict[str, list[int]] = defaultdict(list)
    update_uses: dict[str, list[int]] = defaultdict(list)
    for row in pairs:
        setup_uses[row["setupTarget"]].append(row["index"])
        update_uses[row["updateTarget"]].append(row["index"])
    child_paths = set(label_paths.values())
    referenced_paths = {
        row[key] for row in pairs for key in ("setupPath", "updatePath") if row[key] in child_paths
    }
    return {
        "setupFileCount": sum("/update/" not in f"/{path}" for path in child_paths),
        "updateFileCount": sum("/update/" in f"/{path}" for path in child_paths),
        "setupDispatchCount": len(setup_targets),
        "updateDispatchCount": len(update_targets),
        "uniqueSetupTargetCount": len(setup_uses),
        "uniqueUpdateTargetCount": len(update_uses),
        "rootOwnedUpdateTargets": sorted(root_update_labels & set(update_targets)),
        "sharedSetupFiles": sorted(
            {
                row["setupPath"]
                for row in pairs
                if sum(other["setupPath"] == row["setupPath"] for other in pairs) > 1
            }
        ),
        "reusedUpdateTargets": [
            {"target": target, "indices": indices}
            for target, indices in sorted(update_uses.items())
            if len(indices) > 1
        ],
        "allChildFilesDispatched": referenced_paths == child_paths,
        "pairs": pairs,
    }


def build_battle_scene_animation_inventory(upstream_path: Path) -> dict[str, Any]:
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"battle-scene animation H1 listing is missing: {listing_path}")
    listing = listing_path.read_text(encoding="utf-8")
    paths = sorted((disasm / SOURCE_ROOT).rglob("*.asm"), key=lambda path: path.as_posix())
    files = [_parse_source_file(path, path.relative_to(disasm).as_posix()) for path in paths]
    if len(files) != 55:
        raise ValueError(f"battle-scene animation file-count drift: {len(files)}")
    representative_symbols: dict[str, str] = {}
    representative_addresses: dict[str, int] = {}
    label_paths: dict[str, str] = {}
    calls: Counter[str] = Counter()
    for row in files:
        if not row["globalLabels"]:
            raise ValueError(f"battle-scene animation has no global label: {row['path']}")
        relative = Path(row["path"]).relative_to(SOURCE_ROOT).as_posix()
        symbol = row["globalLabels"][0]
        representative_symbols[relative] = symbol
        representative_addresses[symbol] = _listing_address(listing, symbol)
        for label in row["globalLabels"]:
            if label in label_paths:
                raise ValueError(f"duplicate battle-scene animation label: {label}")
            label_paths[label] = row["path"]
        for call in row["directCalls"]:
            calls[call["target"]] += call["siteCount"]
    records = [
        record
        for record in load_json(RESEARCH_INDEX)["records"]
        if Path(record["sourcePath"]).is_relative_to(SOURCE_ROOT)
    ]
    labels = set(label_paths)
    summary = {
        "fileCount": len(files),
        "sourceLineCount": sum(row["sourceLineCount"] for row in files),
        "statementCount": sum(row["statementCount"] for row in files),
        "globalLabelCount": sum(len(row["globalLabels"]) for row in files),
        "localLabelCount": sum(row["localLabelCount"] for row in files),
        "directCallSiteCount": sum(calls.values()),
        "indirectCallSiteCount": sum(row["indirectCallSiteCount"] for row in files),
        "uniqueDirectTargetCount": len(calls),
        "internalDirectTargetCount": sum(target in labels for target in calls),
        "externalDirectTargetCount": sum(target not in labels for target in calls),
        "indexedRecordCount": len(records),
        "indexedFileCount": len({record["sourcePath"] for record in records}),
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "scope": SOURCE_ROOT.as_posix(),
        "summary": summary,
        "indexedRecordIds": sorted(record["id"] for record in records),
        "indexedSourcePaths": sorted({record["sourcePath"] for record in records}),
        "representativeSymbols": representative_symbols,
        "representativeAddresses": representative_addresses,
        "internalDirectCallTargets": sorted(target for target in calls if target in labels),
        "externalDirectCallTargets": sorted(target for target in calls if target not in labels),
        "dispatchFacts": _dispatch_facts(disasm, label_paths),
        "files": files,
    }


def verify_battle_scene_animation_inventory(
    upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_battle_scene_animation_inventory(upstream_path)
    validate_json(output, SCHEMA, owner="battle-scene animation static inventory")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != load_json(ROM_MANIFEST)["hashes"]["sha256"]
    ):
        raise ValueError("battle-scene animation provenance drift")
    if output["summary"] != manifest["summary"]:
        raise ValueError("battle-scene animation summary drift")
    if output["representativeAddresses"] != fixture["function"]:
        raise ValueError("battle-scene animation H1 address drift")
    dispatch_summary = {
        key: value for key, value in output["dispatchFacts"].items() if key != "pairs"
    }
    if dispatch_summary != fixture["expected"]["dispatchSummary"]:
        raise ValueError("battle-scene animation dispatch model drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"]:
        raise ValueError("battle-scene animation canonical hash drift")
    destination = output_path or repo_path("local/derived/battle-scene-animations-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Inventory": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Files": output["summary"]["fileCount"],
        "SetupFiles": output["dispatchFacts"]["setupFileCount"],
        "UpdateFiles": output["dispatchFacts"]["updateFileCount"],
        "DispatchPairs": len(output["dispatchFacts"]["pairs"]),
        "IndexedRecords": output["summary"]["indexedRecordCount"],
        "Status": "PASS",
    }
