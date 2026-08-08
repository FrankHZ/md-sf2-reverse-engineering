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
TOOLCHAIN_MANIFEST = repo_path("manifests/toolchain.json")


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


def _index_records_for_source_roots(source_paths: set[str]) -> dict[str, Any]:
    """Join every index record whose source lies under either owned root.

    Membership is intentionally based only on the record's source path.  The
    same jump-interface source can carry evidence for another subsystem, so a
    record ID, subsystem, document, or evidence level must not affect this
    routing-owner join.
    """
    records_by_source_path: dict[str, list[str]] = {}
    for record in load_json(RESEARCH_INDEX)["records"]:
        source_path = record["sourcePath"]
        if not any(Path(source_path).is_relative_to(root) for root in SOURCE_ROOTS):
            continue
        if source_path not in source_paths:
            raise ValueError(
                "tech interfaces indexed source is absent from the discovered root "
                f"inventory: {source_path}"
            )
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
        raise ValueError("tech interfaces research-index duplicate record ID")
    return {
        "indexedRecordIds": sorted(indexed_record_ids),
        "indexedSourcePaths": [
            row["sourcePath"] for row in indexed_records_by_source_path
        ],
        "indexedRecordsBySourcePath": indexed_records_by_source_path,
    }


def _verify_indexed_record_join(output: dict[str, Any]) -> None:
    """Reject schema-valid drift between the index join's related fields."""
    relation = output["indexedRecordsBySourcePath"]
    relation_source_paths = [row["sourcePath"] for row in relation]
    relation_record_ids = [record_id for row in relation for record_id in row["recordIds"]]
    if len(relation_source_paths) != len(set(relation_source_paths)):
        raise ValueError("tech interfaces indexed relation duplicate source path")
    if len(relation_record_ids) != len(set(relation_record_ids)):
        raise ValueError("tech interfaces indexed relation duplicate record ID")
    if any(row["recordIds"] != sorted(row["recordIds"]) for row in relation):
        raise ValueError("tech interfaces indexed relation record order drift")

    indexed_record_ids = output["indexedRecordIds"]
    indexed_source_paths = output["indexedSourcePaths"]
    if indexed_record_ids != sorted(relation_record_ids):
        raise ValueError("tech interfaces indexedRecordIds relation drift")
    if indexed_source_paths != relation_source_paths:
        raise ValueError("tech interfaces indexedSourcePaths relation order drift")
    file_paths = [row["path"] for row in output["files"]]
    if len(file_paths) != len(set(file_paths)) or file_paths != sorted(file_paths):
        raise ValueError("tech interfaces source inventory path order drift")
    if indexed_source_paths != file_paths:
        raise ValueError("tech interfaces indexedSourcePaths source inventory drift")

    summary = output["summary"]
    if summary["indexedRecordCount"] != len(indexed_record_ids) or summary[
        "indexedRecordCount"
    ] != len(relation_record_ids):
        raise ValueError("tech interfaces summary indexedRecordCount relation drift")
    if summary["indexedFileCount"] != len(indexed_source_paths) or summary[
        "indexedFileCount"
    ] != len(relation_source_paths):
        raise ValueError("tech interfaces summary indexedFileCount relation drift")


def _verify_fixture_provenance(fixture: dict[str, Any], output: dict[str, Any]) -> None:
    """Derive fixture provenance from the pinned toolchain and ROM owners."""
    toolchain = load_json(TOOLCHAIN_MANIFEST)["sf2disasm"]
    fixture_commit = fixture["upstreamCommit"]
    output_upstream = output["upstream"]
    if fixture_commit != toolchain["commit"] or fixture_commit != output_upstream["commit"]:
        raise ValueError("tech interfaces fixture upstream provenance drift")
    if output_upstream["repository"] != toolchain["repository"]:
        raise ValueError("tech interfaces output upstream provenance drift")
    if fixture["romSha256"] != load_json(ROM_MANIFEST)["hashes"]["sha256"]:
        raise ValueError("tech interfaces fixture ROM provenance drift")


def build_interface_inventory(upstream_path: Path) -> dict[str, Any]:
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"tech interfaces H1 listing is missing: {listing_path}")
    listing = listing_path.read_text(encoding="utf-8")
    jump_paths = sorted((disasm / JUMP_ROOT).rglob("*.asm"), key=lambda path: path.as_posix())
    pointer_paths = sorted((disasm / POINTER_ROOT).rglob("*.asm"), key=lambda path: path.as_posix())
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
    indexed_records = _index_records_for_source_roots({row["path"] for row in files})
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
        "indexedRecordCount": len(indexed_records["indexedRecordIds"]),
        "indexedFileCount": len(indexed_records["indexedSourcePaths"]),
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "scopes": [root.as_posix() for root in SOURCE_ROOTS],
        "summary": summary,
        **indexed_records,
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
    _verify_indexed_record_join(output)
    _verify_fixture_provenance(fixture, output)
    for field in (
        "indexedRecordIds",
        "indexedSourcePaths",
        "indexedRecordsBySourcePath",
        "interfaceFacts",
    ):
        if output[field] != fixture["expected"][field]:
            raise ValueError(f"tech interfaces {field} drift")
    if output["representativeAddresses"] != fixture["function"]:
        raise ValueError("tech interfaces H1 address drift")
    if output["summary"] != manifest["summary"]:
        raise ValueError("tech interfaces summary drift")
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
