from __future__ import annotations

import hashlib
import json
import re
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_ai import _parse_source_file
from sf2tool.h2.battle_scene_animations import _listing_address
from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path

ID = "sf2-tech-interfaces-static-v1"
JUMP_ROOT = Path("code/common/tech/jumpinterfaces")
POINTER_ROOT = Path("code/common/tech/pointers")
SOURCE_ROOTS = (JUMP_ROOT, POINTER_ROOT)
MANIFEST = repo_path("manifests/extractions/tech-interfaces-static.json")
SCHEMA = repo_path("schemas/tech-interfaces-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/tech-interfaces-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-tech-interfaces-static-fixture.schema.json")
RESEARCH_INDEX = repo_path("manifests/research-index.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _jump_targets(paths: list[Path]) -> dict[str, str]:
    targets: dict[str, str] = {}
    pattern = re.compile(
        r"^(?P<label>[A-Za-z_][A-Za-z0-9_]*):\s*\n\s*"
        r"jmp\s+(?P<target>[A-Za-z_][A-Za-z0-9_]*)\(pc\)",
        re.MULTILINE,
    )
    for path in paths:
        source = path.read_text(encoding="utf-8")
        labels = re.findall(r"^[A-Za-z_][A-Za-z0-9_]*:", source, re.MULTILINE)
        matches = list(pattern.finditer(source))
        if len(matches) != len(labels):
            raise ValueError(f"jump-interface non-stub shape drift: {path.name}")
        for match in matches:
            label = match.group("label")
            if label in targets:
                raise ValueError(f"duplicate jump-interface label: {label}")
            targets[label] = match.group("target")
    return dict(sorted(targets.items()))


def _pointer_targets(paths: list[Path]) -> dict[str, str]:
    targets: dict[str, str] = {}
    pattern = re.compile(
        r"^(?P<label>[A-Za-z_][A-Za-z0-9_]*):(?:[ \t]*)"
        r"(?:\n[ \t]*)?dc\.l[ \t]+(?P<target>[A-Za-z_][A-Za-z0-9_]*)",
        re.MULTILINE,
    )
    for path in paths:
        source = path.read_text(encoding="utf-8")
        labels = re.findall(r"^[A-Za-z_][A-Za-z0-9_]*:", source, re.MULTILINE)
        matches = list(pattern.finditer(source))
        if len(matches) != len(labels):
            raise ValueError(f"pointer-table entry shape drift: {path.name}")
        for match in matches:
            label = match.group("label")
            if label in targets:
                raise ValueError(f"duplicate pointer label: {label}")
            targets[label] = match.group("target")
    return dict(sorted(targets.items()))


def _section_numbers(paths: list[Path]) -> list[int]:
    return sorted(
        {
            int(match.group(1))
            for path in paths
            if (match := re.match(r"s(\d+)_", path.name))
        }
    )


def build_interface_inventory(upstream_path: Path) -> dict[str, Any]:
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"tech interfaces H1 listing is missing: {listing_path}")
    listing = listing_path.read_text(encoding="utf-8")
    jump_paths = sorted((disasm / JUMP_ROOT).glob("*.asm"), key=lambda path: path.as_posix())
    pointer_paths = sorted((disasm / POINTER_ROOT).glob("*.asm"), key=lambda path: path.as_posix())
    if len(jump_paths) != 10 or len(pointer_paths) != 15:
        raise ValueError("tech interface file-count drift")
    paths = jump_paths + pointer_paths
    files = [_parse_source_file(path, path.relative_to(disasm).as_posix()) for path in paths]
    layout = "\n".join(
        path.read_text(encoding="utf-8")
        for path in sorted((disasm / "layout").glob("*.asm"))
    )
    representative_symbols: dict[str, str] = {}
    representative_addresses: dict[str, int] = {}
    calls: Counter[str] = Counter()
    labels: set[str] = set()
    for row in files:
        if row["path"].replace("/", "\\") not in layout:
            raise ValueError(f"tech interface source is absent from layout: {row['path']}")
        if not row["globalLabels"]:
            raise ValueError(f"unexpected unlabeled tech interface file: {row['path']}")
        symbol = row["globalLabels"][0]
        representative_symbols[row["path"]] = symbol
        representative_addresses[symbol] = _listing_address(listing, symbol)
        labels.update(row["globalLabels"])
        for call in row["directCalls"]:
            calls[call["target"]] += call["siteCount"]
    records = [
        record
        for record in load_json(RESEARCH_INDEX)["records"]
        if any(Path(record["sourcePath"]).parent == root for root in SOURCE_ROOTS)
    ]
    jump_targets = _jump_targets(jump_paths)
    pointer_targets = _pointer_targets(pointer_paths)
    summary = {
        "fileCount": len(files),
        "jumpInterfaceFileCount": len(jump_paths),
        "pointerFileCount": len(pointer_paths),
        "sourceLineCount": sum(row["sourceLineCount"] for row in files),
        "statementCount": sum(row["statementCount"] for row in files),
        "globalLabelCount": sum(len(row["globalLabels"]) for row in files),
        "localLabelCount": sum(row["localLabelCount"] for row in files),
        "directCallSiteCount": sum(calls.values()),
        "indirectCallSiteCount": sum(row["indirectCallSiteCount"] for row in files),
        "uniqueDirectTargetCount": len(calls),
        "internalDirectTargetCount": sum(target in labels for target in calls),
        "externalDirectTargetCount": sum(target not in labels for target in calls),
        "layoutIncludedFileCount": len(files),
        "indexedRecordCount": len(records),
        "indexedFileCount": len({record["sourcePath"] for record in records}),
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "scopes": [root.as_posix() for root in SOURCE_ROOTS],
        "summary": summary,
        "indexedRecordIds": sorted(record["id"] for record in records),
        "indexedSourcePaths": sorted({record["sourcePath"] for record in records}),
        "representativeSymbols": representative_symbols,
        "representativeAddresses": representative_addresses,
        "internalDirectCallTargets": sorted(target for target in calls if target in labels),
        "externalDirectCallTargets": sorted(target for target in calls if target not in labels),
        "interfaceFacts": {
            "jumpStubCount": len(jump_targets),
            "prefixedJumpStubCount": sum(label.startswith("j_") for label in jump_targets),
            "nonPrefixedJumpStubCount": sum(not label.startswith("j_") for label in jump_targets),
            "pointerEntryCount": len(pointer_targets),
            "allJumpEntriesAreSinglePcRelativeJumps": True,
            "allPointerEntriesAreSingleLongwordTargets": True,
            "jumpSections": _section_numbers(jump_paths),
            "pointerSections": _section_numbers(pointer_paths),
            "runtimeQuestionsRequired": False,
        },
        "jumpTargets": jump_targets,
        "pointerTargets": pointer_targets,
        "files": files,
    }


def verify_interface_inventory(
    upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_interface_inventory(upstream_path)
    validate_json(output, SCHEMA, owner="tech interfaces static inventory")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != load_json(ROM_MANIFEST)["hashes"]["sha256"]
    ):
        raise ValueError("tech interfaces provenance drift")
    if output["summary"] != manifest["summary"]:
        raise ValueError("tech interfaces summary drift")
    if output["representativeAddresses"] != fixture["function"]:
        raise ValueError("tech interfaces H1 address drift")
    if output["interfaceFacts"] != fixture["expected"]["interfaceFacts"]:
        raise ValueError("tech interfaces model drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"]:
        raise ValueError("tech interfaces canonical hash drift")
    destination = output_path or repo_path("local/derived/tech-interfaces-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Inventory": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Files": output["summary"]["fileCount"],
        "JumpStubs": output["interfaceFacts"]["jumpStubCount"],
        "PointerEntries": output["interfaceFacts"]["pointerEntryCount"],
        "IndexedRecords": output["summary"]["indexedRecordCount"],
        "Status": "PASS",
    }
