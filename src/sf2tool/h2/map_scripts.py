from __future__ import annotations

import hashlib
import json
import re
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_ai import _parse_source_file
from sf2tool.h2.battle_global_data import _statements
from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.source_text import read_upstream_text

ID = "sf2-map-scripts-static-v1"
SOURCE_ROOT = Path("data/maps/entries")
MANIFEST = repo_path("manifests/extractions/map-scripts-static.json")
SCHEMA = repo_path("schemas/map-scripts-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/map-scripts-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-map-scripts-static-fixture.schema.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")
MAP_INIT_FIXTURE = repo_path("tests/fixtures/h2/map-init-static-v1.json")


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _label_kind(symbol: str) -> str:
    for prefix, kind in (
        ("csub_", "cutscene-subroutine"),
        ("cs_", "cutscene"),
        ("eas_", "entity-action-script"),
        ("ce_", "cutscene-entity"),
        ("palette_", "palette-data"),
        ("sub_", "subroutine"),
        ("loc_", "local-control-flow"),
        ("byte_", "byte-data-or-control-flow"),
    ):
        if symbol.startswith(prefix):
            return kind
    return "other"


def _reference_facts(
    definitions: dict[str, str], sources: dict[str, str]
) -> tuple[list[dict[str, Any]], dict[str, int]]:
    stripped_sources = {
        path: "\n".join(line.split(";", 1)[0] for line in source.splitlines())
        for path, source in sources.items()
    }
    rows: list[dict[str, Any]] = []
    totals = Counter()
    for symbol, owner_path in sorted(definitions.items()):
        pattern = re.compile(rf"\b{re.escape(symbol)}\b")
        same_file = len(pattern.findall(stripped_sources[owner_path])) - 1
        external_sources = [
            path
            for path, source in stripped_sources.items()
            if path != owner_path and pattern.search(source)
        ]
        external_count = sum(
            len(pattern.findall(stripped_sources[path])) for path in external_sources
        )
        if external_count:
            totals["externallyReferencedLabelCount"] += 1
        elif same_file:
            totals["internalOnlyLabelCount"] += 1
        else:
            totals["unreferencedLabelCount"] += 1
        totals["sameFileReferenceCount"] += same_file
        totals["externalReferenceCount"] += external_count
        rows.append(
            {
                "symbol": symbol,
                "ownerPath": owner_path,
                "sameFileReferenceCount": same_file,
                "externalReferenceCount": external_count,
                "externalSourcePaths": external_sources,
            }
        )
    return rows, dict(totals)


def build_map_scripts_inventory(upstream_path: Path) -> dict[str, Any]:
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"map scripts H1 listing is missing: {listing_path}")
    addresses = listing_symbol_addresses(listing_path.read_text(encoding="utf-8"))
    script_paths = sorted(
        path
        for path in (disasm / SOURCE_ROOT).rglob("scripts*.asm")
        if "mapsetups" in path.parts
    )
    if len(script_paths) != 47:
        raise ValueError(f"standalone map script boundary drift: {len(script_paths)} files")
    setup_paths = sorted(
        path for path in (disasm / SOURCE_ROOT).rglob("*.asm") if "mapsetups" in path.parts
    )
    if len(setup_paths) != 720:
        raise ValueError(f"complete map setup source boundary drift: {len(setup_paths)} files")
    sources = {
        path.relative_to(disasm).as_posix(): read_upstream_text(path) for path in setup_paths
    }
    files = [
        _parse_source_file(path, path.relative_to(disasm).as_posix()) for path in script_paths
    ]
    definitions: dict[str, str] = {}
    for row in files:
        for symbol in row["globalLabels"]:
            if symbol in definitions:
                raise ValueError(f"duplicate standalone map script label: {symbol}")
            if symbol not in addresses:
                raise ValueError(f"map script label absent from H1 listing: {symbol}")
            definitions[symbol] = row["path"]
    references, reference_summary = _reference_facts(definitions, sources)

    command_counts: Counter[str] = Counter()
    for path in script_paths:
        for statement in _statements(read_upstream_text(path)):
            command = statement.split(None, 1)[0]
            if not command.endswith(":"):
                command_counts[command] += 1
    label_kinds = Counter(_label_kind(symbol) for symbol in definitions)
    first_symbols = {row["path"]: row["globalLabels"][0] for row in files}
    representative_addresses = {
        symbol: addresses[symbol] for symbol in first_symbols.values()
    }

    init_targets = set(load_json(MAP_INIT_FIXTURE)["expected"]["scriptTargetCounts"])
    standalone_init_targets = sorted(init_targets & set(definitions))
    unresolved_init_targets = sorted(
        symbol for symbol in init_targets if symbol not in addresses
    )
    if unresolved_init_targets:
        raise ValueError(f"init script targets absent from H1 listing: {unresolved_init_targets}")

    summary = {
        "fileCount": len(files),
        "sourceLineCount": sum(row["sourceLineCount"] for row in files),
        "statementCount": sum(command_counts.values()),
        "uniqueCommandCount": len(command_counts),
        "globalLabelCount": len(definitions),
        "representativeAddressCount": len(representative_addresses),
        "externallyReferencedLabelCount": reference_summary.get(
            "externallyReferencedLabelCount", 0
        ),
        "internalOnlyLabelCount": reference_summary.get("internalOnlyLabelCount", 0),
        "unreferencedLabelCount": reference_summary.get("unreferencedLabelCount", 0),
        "sameFileReferenceCount": reference_summary.get("sameFileReferenceCount", 0),
        "externalReferenceCount": reference_summary.get("externalReferenceCount", 0),
        "initScriptTargetCount": len(init_targets),
        "standaloneOwnedInitScriptTargetCount": len(standalone_init_targets),
        "nonStandaloneInitScriptTargetCount": len(init_targets - set(definitions)),
        "cutsceneEndCommandCount": command_counts["csc_end"],
        "returnStatementCount": command_counts["rts"],
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "scope": f"{SOURCE_ROOT.as_posix()}/*/mapsetups/scripts*.asm",
        "summary": summary,
        "representativeSymbols": first_symbols,
        "representativeAddresses": representative_addresses,
        "labelKindCounts": dict(sorted(label_kinds.items())),
        "commandCounts": dict(sorted(command_counts.items())),
        "mostFrequentCommands": [
            {"command": command, "count": count}
            for command, count in sorted(
                command_counts.items(), key=lambda item: (-item[1], item[0])
            )[:12]
        ],
        "standaloneOwnedInitScriptTargets": standalone_init_targets,
        "runtimeQuestions": [
            "map-script-story-side-effects-and-transition-persistence",
            "map-script-entity-camera-text-and-wait-timing",
            "map-script-custom-subroutine-and-palette-rendering",
        ],
        "files": files,
        "labelAddresses": {symbol: addresses[symbol] for symbol in sorted(definitions)},
        "references": references,
    }


def verify_map_scripts_inventory(
    upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_map_scripts_inventory(upstream_path)
    validate_json(output, SCHEMA, owner="map scripts static inventory")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != load_json(ROM_MANIFEST)["hashes"]["sha256"]
        or fixture["function"] != output["representativeAddresses"]
    ):
        raise ValueError("map scripts provenance/address drift")
    for field in (
        "summary",
        "labelKindCounts",
        "mostFrequentCommands",
        "standaloneOwnedInitScriptTargets",
        "runtimeQuestions",
    ):
        if fixture["expected"][field] != output[field]:
            raise ValueError(f"map scripts {field} drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"] or output["summary"] != manifest["summary"]:
        raise ValueError("map scripts canonical output drift")
    destination = output_path or repo_path("local/derived/map-scripts-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Inventory": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Files": output["summary"]["fileCount"],
        "Labels": output["summary"]["globalLabelCount"],
        "Statements": output["summary"]["statementCount"],
        "ExternalReferences": output["summary"]["externalReferenceCount"],
        "Status": "PASS",
    }
