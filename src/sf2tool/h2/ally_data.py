from __future__ import annotations

import hashlib
import json
import re
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_ai import _parse_source_file
from sf2tool.h2.battle_global_data import _arguments, _tokens
from sf2tool.h2.battle_scene_animations import _listing_address
from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.source_text import read_upstream_text

ID = "sf2-ally-data-static-v1"
SOURCE_ROOT = Path("data/stats/allies")
MANIFEST = repo_path("manifests/extractions/ally-data-static.json")
GROWTH_MANIFEST = repo_path("manifests/extractions/growth-data.json")
SCHEMA = repo_path("schemas/ally-data-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/ally-data-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-ally-data-static-fixture.schema.json")
RESEARCH_INDEX = repo_path("manifests/research-index.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _directive_count(source: str, directive: str) -> int:
    return len(_arguments(source, directive))


def _list_tokens(source: str, directive: str) -> list[str]:
    return [token for expression in _arguments(source, directive) for token in _tokens(expression)]


def _growth_facts(source: str) -> dict[str, Any]:
    rows = []
    for expression in _arguments(source, "dc.w"):
        tokens = _tokens(expression)
        if len(tokens) != 2:
            raise ValueError("ally growth row width drift")
        rows.append([int(token) for token in tokens])
    if len(rows) % 29:
        raise ValueError("ally growth curve length drift")
    curves = [rows[index : index + 29] for index in range(0, len(rows), 29)]
    for curve in curves:
        previous = 0
        for total, gain in curve:
            if total - previous != gain:
                raise ValueError("ally growth cumulative invariant drift")
            previous = total
        if curve[-1][0] != 256:
            raise ValueError("ally growth terminal projection drift")
    return {
        "curveCount": len(curves),
        "levelsPerCurve": 29,
        "entryCount": len(rows),
        "terminalScale": 256,
        "curves": curves,
    }


def _ally_stats_facts(sources: dict[str, str]) -> dict[str, Any]:
    paths = [f"{SOURCE_ROOT.as_posix()}/stats/allystats{index:02d}.asm" for index in range(30)]
    class_records = 0
    explicit_lists = 0
    inherited_lists = 0
    spell_entries = 0
    per_ally_class_counts: list[int] = []
    for path in paths:
        source = sources[path]
        class_count = _directive_count(source, "forClass")
        explicit_count = _directive_count(source, "spellList")
        inherited_count = _directive_count(source, "useFirstSpellList")
        if class_count != explicit_count + inherited_count:
            raise ValueError(f"ally stats spell-list ownership drift: {path}")
        statements = [
            statement
            for statement in source.splitlines()
            if statement.split(";", 1)[0].strip()
        ]
        first_for_class = next(
            index for index, statement in enumerate(statements) if "forClass" in statement
        )
        second_for_class = next(
            (
                index
                for index in range(first_for_class + 1, len(statements))
                if "forClass" in statements[index]
            ),
            len(statements),
        )
        first_block = "\n".join(statements[first_for_class:second_for_class])
        if not re.search(r"^\s*spellList(?:\s|$)", first_block, re.MULTILINE):
            raise ValueError(f"ally stats first class does not own its spell list: {path}")
        for expression in _arguments(source, "spellList"):
            spell_entries += len(
                re.findall(r"\b\d+\s*,\s*[A-Z0-9_]+(?:\|LV[1-4])?", expression)
            )
        class_records += class_count
        explicit_lists += explicit_count
        inherited_lists += inherited_count
        per_ally_class_counts.append(class_count)
    return {
        "allyFileCount": len(paths),
        "classRecordCount": class_records,
        "explicitSpellListCount": explicit_lists,
        "inheritedSpellListCount": inherited_lists,
        "spellEntryCount": spell_entries,
        "firstClassOwnsSpellListCount": len(paths),
        "perAllyClassCounts": per_ally_class_counts,
    }


def _source_facts(sources: dict[str, str]) -> dict[str, Any]:
    def source(relative: str) -> str:
        return sources[f"{SOURCE_ROOT.as_posix()}/{relative}"]

    entries_source = source("stats/entries.asm")
    pointers = _list_tokens(entries_source, "dc.l")
    stat_includes = _arguments(entries_source, "include")
    if len(pointers) != 32 or len(stat_includes) != 30:
        raise ValueError("ally stats pointer/include boundary drift")

    growth = _growth_facts(source("growthcurves.asm"))
    stats = _ally_stats_facts(sources)
    growth_manifest = load_json(GROWTH_MANIFEST)
    expected_growth = growth_manifest["counts"]
    if (
        growth["curveCount"] != expected_growth["curves"]
        or growth["levelsPerCurve"] != expected_growth["levelsPerCurve"]
        or stats["allyFileCount"] != expected_growth["allies"]
        or stats["classRecordCount"] != expected_growth["classRecords"]
        or stats["spellEntryCount"] != expected_growth["spellEntries"]
    ):
        raise ValueError("ally inventory disagrees with the existing growth extraction rail")

    promotion_sections = [
        _tokens(expression)
        for expression in _arguments(source("classes/promotions.asm"), "promotionSection")
    ]
    promotion_items = _list_tokens(source("classes/promotions.asm"), "promotionItems")
    critical_values = _list_tokens(source("classes/criticalhitdefs.asm"), "dc.b")

    return {
        "presentation": {
            "allyNameCount": _directive_count(source("allynames.asm"), "allyName"),
            "mapSpriteCount": _directive_count(source("allymapsprites.asm"), "mapsprite"),
            "battleSpriteEntryCount": _directive_count(
                source("allybattlesprites.asm"), "allyBattleSprAndPlt"
            ),
            "battleSpriteEntriesPerAlly": 3,
        },
        "startDefinitions": {
            "recordCount": _directive_count(source("allystartdefs.asm"), "startClass"),
            "namedAllyCount": 30,
            "trailingRecordCount": 2,
            "itemsPerRecord": 4,
        },
        "classes": {
            "nameCount": _directive_count(source("classes/classnames.asm"), "className"),
            "typeCount": _directive_count(source("classes/classtypes.asm"), "classType"),
            "definitionCount": _directive_count(source("classes/classdefs.asm"), "mov"),
            "criticalDefinitionCount": len(critical_values) // 2,
            "blacksmithEligibleCount": len(
                _list_tokens(source("classes/blacksmitheligibleclasses.asm"), "classes")
            ),
            "promotionSectionCount": len(promotion_sections),
            "promotionSectionSizes": [len(section) for section in promotion_sections],
            "promotionItemCount": len(promotion_items),
        },
        "growth": growth,
        "stats": stats,
        "pointerTable": {
            "slotCount": len(pointers),
            "uniqueTargetCount": len(set(pointers)),
            "namedAllyCount": 30,
            "trailingReuseTarget": pointers[-1],
            "trailingReuseSlotCount": 2,
            "nestedIncludeCount": len(stat_includes),
        },
        "existingRailOwnership": {
            "growth-data": {
                "manifest": "manifests/extractions/growth-data.json",
                "curveCount": expected_growth["curves"],
                "classRecordCount": expected_growth["classRecords"],
                "spellEntryCount": expected_growth["spellEntries"],
            },
            "static-core-data": {
                "manifest": "manifests/extractions/static-data.json",
                "ownsNamesClassesAndStartDefinitions": True,
            },
        },
    }


def _fact_summary(facts: dict[str, Any]) -> dict[str, Any]:
    return {
        "presentation": facts["presentation"],
        "startDefinitions": facts["startDefinitions"],
        "classes": facts["classes"],
        "growth": {
            key: facts["growth"][key]
            for key in ("curveCount", "levelsPerCurve", "entryCount", "terminalScale")
        },
        "stats": {
            key: facts["stats"][key]
            for key in (
                "allyFileCount",
                "classRecordCount",
                "explicitSpellListCount",
                "inheritedSpellListCount",
                "spellEntryCount",
                "firstClassOwnsSpellListCount",
            )
        },
        "pointerTable": facts["pointerTable"],
        "existingRailOwnership": facts["existingRailOwnership"],
    }


def build_ally_data_inventory(upstream_path: Path) -> dict[str, Any]:
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"ally-data H1 listing is missing: {listing_path}")
    listing = listing_path.read_text(encoding="utf-8")
    root = disasm / SOURCE_ROOT
    paths = sorted(root.rglob("*.asm"))
    if len(paths) != 42:
        raise ValueError(f"ally-data boundary drift: expected 42 files, got {len(paths)}")
    files = [_parse_source_file(path, path.relative_to(disasm).as_posix()) for path in paths]
    if any(len(row["globalLabels"]) != 1 for row in files):
        raise ValueError("ally-data files must each expose one representative label")

    layout = "\n".join(
        path.read_text(encoding="utf-8") for path in sorted((disasm / "layout").glob("*.asm"))
    )
    direct_paths = sorted(
        row["path"] for row in files if row["path"].replace("/", "\\") in layout
    )
    entries_source = read_upstream_text(disasm / SOURCE_ROOT / "stats/entries.asm")
    nested_paths = sorted(
        match.replace("\\", "/")
        for match in re.findall(r'^\s*include\s+"([^"]+)"', entries_source, re.MULTILINE)
    )
    transitive_paths = sorted(set(direct_paths) | set(nested_paths))
    expected_paths = sorted(row["path"] for row in files)
    if transitive_paths != expected_paths:
        missing = sorted(set(expected_paths) - set(transitive_paths))
        extra = sorted(set(transitive_paths) - set(expected_paths))
        raise ValueError(f"ally-data transitive layout drift: missing={missing}, extra={extra}")

    representative_symbols = {row["path"]: row["globalLabels"][0] for row in files}
    representative_addresses = {
        symbol: _listing_address(listing, symbol) for symbol in representative_symbols.values()
    }
    records = [
        record
        for record in load_json(RESEARCH_INDEX)["records"]
        if record["sourcePath"].startswith(f"{SOURCE_ROOT.as_posix()}/")
    ]
    sources = {
        path.relative_to(disasm).as_posix(): read_upstream_text(path) for path in paths
    }
    facts = _source_facts(sources)
    summary = {
        "fileCount": len(files),
        "sourceLineCount": sum(row["sourceLineCount"] for row in files),
        "statementCount": sum(row["statementCount"] for row in files),
        "globalLabelCount": sum(len(row["globalLabels"]) for row in files),
        "directLayoutIncludeCount": len(direct_paths),
        "nestedIncludeCount": len(nested_paths),
        "transitiveLayoutFileCount": len(transitive_paths),
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
        "directLayoutPaths": direct_paths,
        "nestedIncludePaths": nested_paths,
        "transitiveLayoutPaths": transitive_paths,
        "indexedRecordIds": sorted(record["id"] for record in records),
        "indexedSourcePaths": sorted({record["sourcePath"] for record in records}),
        "representativeSymbols": representative_symbols,
        "representativeAddresses": representative_addresses,
        "factSummary": _fact_summary(facts),
        "facts": facts,
        "runtimeQuestions": [
            "trailing-start-definition-record-reachability",
            "battle-sprite-none-entry-fallback-and-presentation",
        ],
        "files": files,
    }


def verify_ally_data_inventory(
    upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_ally_data_inventory(upstream_path)
    validate_json(output, SCHEMA, owner="ally data static inventory")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != load_json(ROM_MANIFEST)["hashes"]["sha256"]
    ):
        raise ValueError("ally data provenance drift")
    if output["summary"] != manifest["summary"]:
        raise ValueError("ally data summary drift")
    if output["representativeAddresses"] != fixture["table"]:
        raise ValueError("ally data H1 address drift")
    for field in ("factSummary", "runtimeQuestions"):
        if output[field] != fixture["expected"][field]:
            raise ValueError(f"ally data {field} drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"]:
        raise ValueError("ally data canonical hash drift")
    destination = output_path or repo_path("local/derived/ally-data-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Inventory": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Files": output["summary"]["fileCount"],
        "DirectIncludes": output["summary"]["directLayoutIncludeCount"],
        "NestedIncludes": output["summary"]["nestedIncludeCount"],
        "IndexedFiles": output["summary"]["indexedFileCount"],
        "Status": "PASS",
    }
