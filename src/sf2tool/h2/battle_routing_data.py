from __future__ import annotations

import hashlib
import json
import re
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_ai import _parse_source_file
from sf2tool.h2.battle_scene_animations import _listing_address
from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.source_text import read_upstream_text

ID = "sf2-battle-routing-data-static-v1"
SOURCE_ROOT = Path("data/battles")
CUTSCENE_ROOT = SOURCE_ROOT / "cutscenes"
TERRAIN_PATH = SOURCE_ROOT / "terrainentries.asm"
LEGACY_SPRITESET_PATH = SOURCE_ROOT / "spritesetentries.asm"
MANIFEST = repo_path("manifests/extractions/battle-routing-data-static.json")
SCHEMA = repo_path("schemas/battle-routing-data-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/battle-routing-data-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-battle-routing-data-static-fixture.schema.json")
RESEARCH_INDEX = repo_path("manifests/research-index.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _directive_values(source: str, directive: str) -> list[str]:
    return [
        match.group(1).strip()
        for match in re.finditer(
            rf"^\s*{re.escape(directive)}\s+([^;\r\n]+)", source, re.MULTILINE
        )
    ]


def build_battle_routing_data_inventory(upstream_path: Path) -> dict[str, Any]:
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"battle-routing-data H1 listing is missing: {listing_path}")
    listing = listing_path.read_text(encoding="utf-8")
    paths = sorted((disasm / CUTSCENE_ROOT).glob("*.asm")) + [
        disasm / LEGACY_SPRITESET_PATH,
        disasm / TERRAIN_PATH,
    ]
    if len(paths) != 8 or any(not path.is_file() for path in paths):
        raise ValueError("battle routing data boundary drift")
    files = [_parse_source_file(path, path.relative_to(disasm).as_posix()) for path in paths]
    rows_by_path = {row["path"]: row for row in files}
    sources = {
        path.relative_to(disasm).as_posix(): read_upstream_text(path) for path in paths
    }

    layout = "\n".join(
        path.read_text(encoding="utf-8") for path in sorted((disasm / "layout").glob("*.asm"))
    )
    built_paths = sorted(
        path
        for path in sources
        if path.replace("/", "\\") in layout
    )
    expected_built = sorted(
        [path for path in sources if path != LEGACY_SPRITESET_PATH.as_posix()]
    )
    if built_paths != expected_built:
        raise ValueError("battle routing layout ownership drift")

    expected_symbols = {
        f"{CUTSCENE_ROOT.as_posix()}/afterbattlecutscenes.asm": "rpt_AfterBattleCutscenes",
        f"{CUTSCENE_ROOT.as_posix()}/afterbattlejoins.asm": "table_AfterBattleJoins",
        f"{CUTSCENE_ROOT.as_posix()}/battleendcutscenes.asm": "rpt_EnemyDefeatedCutscenes",
        f"{CUTSCENE_ROOT.as_posix()}/battlestartcutscenes.asm": "rpt_BattleStartCutscenes",
        f"{CUTSCENE_ROOT.as_posix()}/beforebattlecutscenes.asm": "rpt_BeforeBattleCutscenes",
        f"{CUTSCENE_ROOT.as_posix()}/regionactivatedcutscenes.asm": (
            "table_BattleRegionCutscenes"
        ),
        TERRAIN_PATH.as_posix(): "pt_BattleTerrainData",
    }
    for path, symbol in expected_symbols.items():
        if symbol not in rows_by_path[path]["globalLabels"]:
            raise ValueError(f"battle routing representative label drift: {path}")
    representative_addresses = {
        symbol: _listing_address(listing, symbol) for symbol in expected_symbols.values()
    }
    records = [
        record
        for record in load_json(RESEARCH_INDEX)["records"]
        if record["sourcePath"] in sources
    ]

    relative_tables = {
        "beforeBattle": f"{CUTSCENE_ROOT.as_posix()}/beforebattlecutscenes.asm",
        "battleStart": f"{CUTSCENE_ROOT.as_posix()}/battlestartcutscenes.asm",
        "enemyDefeated": f"{CUTSCENE_ROOT.as_posix()}/battleendcutscenes.asm",
        "afterBattle": f"{CUTSCENE_ROOT.as_posix()}/afterbattlecutscenes.asm",
    }
    cutscene_routes: dict[str, dict[str, int]] = {}
    for name, path in relative_tables.items():
        words = _directive_values(sources[path], "dc.w")
        if len(words) != 48:
            raise ValueError(f"battle cutscene route slot drift: {path}")
        cutscene_routes[name] = {
            "slotCount": len(words),
            "nonEmptySlotCount": sum("ms_Empty" not in word for word in words),
        }

    joins = _directive_values(
        sources[f"{CUTSCENE_ROOT.as_posix()}/afterbattlejoins.asm"], "dc.b"
    )
    if len(joins) != 52 or any(value != "0" for value in joins):
        raise ValueError("unused after-battle join table drift")
    region_source = sources[f"{CUTSCENE_ROOT.as_posix()}/regionactivatedcutscenes.asm"]
    region_longs = _directive_values(region_source, "dc.l")
    if len(region_longs) != 4 or _directive_values(region_source, "dc.w")[-1] != "TERMINATOR_WORD":
        raise ValueError("region-activated cutscene table drift")

    terrain_source = sources[TERRAIN_PATH.as_posix()]
    terrain_pointers = [
        value
        for value in _directive_values(terrain_source, "dc.l")
        if value.startswith("BattleTerrain")
    ]
    terrain_incbins = re.findall(
        r'^BattleTerrain(\d{2}):incbin\s+"([^"]+)"', terrain_source, re.MULTILINE
    )
    if len(terrain_pointers) != 45 or len(terrain_incbins) != 43:
        raise ValueError("battle terrain pointer/incbin boundary drift")
    terrain_aliases = {
        str(index): int(symbol.removeprefix("BattleTerrain"))
        for index, symbol in enumerate(terrain_pointers)
        if symbol != f"BattleTerrain{index:02d}"
    }

    legacy_source = sources[LEGACY_SPRITESET_PATH.as_posix()]
    legacy_pointers = _directive_values(legacy_source, "dc.l")
    legacy_incbins = re.findall(r'^\s*incbin\s+"([^"]+)"', legacy_source, re.MULTILINE)
    if len(legacy_pointers) != 45 or len(legacy_incbins) != 45:
        raise ValueError("legacy spriteset aggregate shape drift")

    summary = {
        "fileCount": len(files),
        "sourceLineCount": sum(row["sourceLineCount"] for row in files),
        "statementCount": sum(row["statementCount"] for row in files),
        "globalLabelCount": sum(len(row["globalLabels"]) for row in files),
        "layoutOwnedFileCount": len(built_paths),
        "alternateFileCount": 1,
        "representativeAddressCount": len(representative_addresses),
        "indexedRecordCount": len(records),
        "indexedFileCount": len({record["sourcePath"] for record in records}),
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "scope": SOURCE_ROOT.as_posix(),
        "summary": summary,
        "inventoryPaths": sorted(sources),
        "builtPaths": built_paths,
        "excludedPaths": [LEGACY_SPRITESET_PATH.as_posix()],
        "exclusions": {
            LEGACY_SPRITESET_PATH.as_posix(): (
                "unassembled binary aggregate replaced by data/battles/spritesets/entries.asm"
            )
        },
        "indexedRecordIds": sorted(record["id"] for record in records),
        "indexedSourcePaths": sorted({record["sourcePath"] for record in records}),
        "representativeSymbols": expected_symbols,
        "representativeAddresses": representative_addresses,
        "facts": {
            "cutsceneRoutes": cutscene_routes,
            "unusedAfterBattleJoinSlots": len(joins),
            "unusedAfterBattleJoinNonzeroSlots": 0,
            "regionActivatedRouteCount": len(region_longs),
            "terrain": {
                "slotCount": len(terrain_pointers),
                "uniquePayloadCount": len(terrain_incbins),
                "aliasedSlots": terrain_aliases,
            },
            "legacySpritesetAggregate": {
                "pointerCount": len(legacy_pointers),
                "incbinCount": len(legacy_incbins),
                "layoutOwned": False,
            },
            "routeTargetsParsed": False,
        },
        "runtimeQuestions": [
            "cutscene-route-admission-and-empty-slot-fallback",
            "region-cutscene-flag-lifecycle-and-repeatability",
            "battle-terrain-alias-selection-and-runtime-consumers",
        ],
        "files": files,
    }


def verify_battle_routing_data_inventory(
    upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_battle_routing_data_inventory(upstream_path)
    validate_json(output, SCHEMA, owner="battle routing data static inventory")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != load_json(ROM_MANIFEST)["hashes"]["sha256"]
    ):
        raise ValueError("battle routing data provenance drift")
    if output["summary"] != manifest["summary"]:
        raise ValueError("battle routing data summary drift")
    if output["representativeAddresses"] != fixture["table"]:
        raise ValueError("battle routing data H1 address drift")
    for field in ("facts", "runtimeQuestions"):
        if output[field] != fixture["expected"][field]:
            raise ValueError(f"battle routing data {field} drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"]:
        raise ValueError("battle routing data canonical hash drift")
    destination = output_path or repo_path("local/derived/battle-routing-data-static.json")
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
