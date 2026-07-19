from __future__ import annotations

import hashlib
import json
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_ai import _parse_source_file
from sf2tool.h2.battle_global_data import _arguments, _byte_values, _label_block, _tokens
from sf2tool.h2.battle_scene_animations import _listing_address
from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.source_text import read_upstream_text

ID = "sf2-core-stats-data-static-v1"
SOURCE_ROOTS = (
    Path("data/stats/items"),
    Path("data/stats/spells"),
    Path("data/stats/enemies"),
)
MANIFEST = repo_path("manifests/extractions/core-stats-data-static.json")
SCHEMA = repo_path("schemas/core-stats-data-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/core-stats-data-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-core-stats-data-static-fixture.schema.json")
RESEARCH_INDEX = repo_path("manifests/research-index.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _directive_count(source: str, directive: str) -> int:
    return len(_arguments(source, directive))


def _list_tokens(source: str, directive: str) -> list[str]:
    return [token for expression in _arguments(source, directive) for token in _tokens(expression)]


def _source_facts(sources: dict[str, str]) -> dict[str, Any]:
    def source(relative: str) -> str:
        return sources[f"data/stats/{relative}"]

    item_def_count = _directive_count(source("items/itemdefs.asm"), "equipFlags")
    item_field_counts = {
        directive: _directive_count(source("items/itemdefs.asm"), directive)
        for directive in ("range", "price", "itemType", "useSpell", "equipEffects")
    }
    if any(count != item_def_count for count in item_field_counts.values()):
        raise ValueError("item definition field cardinality drift")

    mithril_groups = _arguments(source("items/mithrilweapons.asm"), "classes")
    mithril_rows = _arguments(source("items/mithrilweapons.asm"), "mithrilWeapons")
    mithril_widths = [len(_tokens(row)) // 2 for row in mithril_rows]
    if any(len(_tokens(row)) % 2 for row in mithril_rows):
        raise ValueError("mithril weapon chance/item pair drift")

    spell_def_count = _directive_count(source("spells/spelldefs.asm"), "entry")
    spell_field_counts = {
        directive: _directive_count(source("spells/spelldefs.asm"), directive)
        for directive in ("mpCost", "animation", "properties", "range", "radius", "power")
    }
    if any(count != spell_def_count for count in spell_field_counts.values()):
        raise ValueError("spell definition field cardinality drift")
    spell_range_source = source("spells/spellranges.asm")
    spell_range_labels = ("SpellRange0", "SpellRange1", "SpellRange2", "SpellRange3")
    spell_range_sizes = [
        _byte_values(_label_block(spell_range_source, label))[0] for label in spell_range_labels
    ]

    enemy_def_count = _directive_count(source("enemies/enemydefs.asm"), "unknownByte")
    enemy_fields = (
        "spellPower",
        "level",
        "maxHp",
        "maxMp",
        "baseAtt",
        "baseDef",
        "baseAgi",
        "baseMov",
        "baseResistance",
        "baseProwess",
        "items",
        "spells",
        "initialStatus",
        "movetype",
        "aiBitfield",
    )
    enemy_field_counts = {
        directive: _directive_count(source("enemies/enemydefs.asm"), directive)
        for directive in enemy_fields
    }
    if any(count != enemy_def_count for count in enemy_field_counts.values()):
        raise ValueError("enemy definition field cardinality drift")
    enemy_gold_count = len(_list_tokens(source("enemies/enemygold.asm"), "dc.w"))

    return {
        "items": {
            "definitionCount": item_def_count,
            "nameCount": _directive_count(source("items/itemnames.asm"), "itemName"),
            "shopInventoryCount": _directive_count(
                source("items/shopinventories.asm"), "shopInventory"
            ),
            "debugShopItemCount": len(_list_tokens(source("items/debugshop.asm"), "dc.b")) - 1,
            "chestGoldTierCount": len(
                _list_tokens(source("items/chestgoldamounts.asm"), "dc.w")
            ),
            "breakMessageCount": _directive_count(
                source("items/itembreakmessages.asm"), "itemBreakMessage"
            ),
            "mithrilClassGroupCount": len(mithril_groups),
            "mithrilWeaponRowCount": len(mithril_rows),
            "mithrilPicksPerRow": mithril_widths,
            "specialCaravanDescriptionCount": _directive_count(
                source("items/specialcaravandescriptions.asm"), "specialCaravanDescription"
            ),
            "usableOutsideBattleCount": _directive_count(
                source("items/usableoutsidebattleitems.asm"), "item"
            ),
            "weaponGraphicsCount": _directive_count(
                source("items/weapongraphics.asm"), "weaponGraphics"
            ),
        },
        "spells": {
            "nameCount": _directive_count(source("spells/spellnames.asm"), "spellName"),
            "elementCount": _directive_count(
                source("spells/spellelements.asm"), "spellElement"
            ),
            "levelDefinitionCount": spell_def_count,
            "rangePointerCount": len(_list_tokens(spell_range_source, "dc.l")),
            "rangeRingSizes": spell_range_sizes,
        },
        "enemies": {
            "nameCount": _directive_count(source("enemies/enemynames.asm"), "enemyName"),
            "definitionCount": enemy_def_count,
            "battleSpriteCount": _directive_count(
                source("enemies/enemybattlesprites.asm"), "enemyBattleSprAndPlt"
            ),
            "mapSpriteCount": _directive_count(
                source("enemies/enemymapsprites.asm"), "mapsprite"
            ),
            "goldWordCount": enemy_gold_count,
            "usedGoldWordCount": enemy_def_count,
            "unusedGoldTailWordCount": enemy_gold_count - enemy_def_count,
        },
        "existingRailOwnership": {
            "static-core-data": {
                "manifest": "manifests/extractions/static-data.json",
                "ownsItemAndSpellDefinitions": True,
            },
            "enemy-promotions": {
                "manifest": "manifests/extractions/enemy-promotion-data.json",
                "ownsEnemyNamesAndDefinitions": True,
            },
            "enemy-gold": {
                "manifest": "manifests/extractions/enemy-gold-data.json",
                "ownsUsedAndUnusedGoldBoundary": True,
            },
            "battlefield": {
                "manifest": "manifests/extractions/battlefield-static.json",
                "ownsSpellRangeSemantics": True,
            },
        },
    }


def build_core_stats_data_inventory(upstream_path: Path) -> dict[str, Any]:
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"core-stats-data H1 listing is missing: {listing_path}")
    listing = listing_path.read_text(encoding="utf-8")
    paths = sorted(path for root in SOURCE_ROOTS for path in (disasm / root).rglob("*.asm"))
    if len(paths) != 19:
        raise ValueError(f"core-stats-data boundary drift: expected 19 files, got {len(paths)}")
    files = [_parse_source_file(path, path.relative_to(disasm).as_posix()) for path in paths]
    if any(not row["globalLabels"] for row in files):
        raise ValueError("core stats data unexpectedly contains an unlabeled file")

    layout = "\n".join(
        path.read_text(encoding="utf-8") for path in sorted((disasm / "layout").glob("*.asm"))
    )
    layout_paths = sorted(
        row["path"] for row in files if row["path"].replace("/", "\\") in layout
    )
    if len(layout_paths) != len(files):
        missing = sorted({row["path"] for row in files} - set(layout_paths))
        raise ValueError(f"core stats data is absent from the original layout: {missing}")

    representative_symbols = {row["path"]: row["globalLabels"][0] for row in files}
    representative_addresses = {
        symbol: _listing_address(listing, symbol) for symbol in representative_symbols.values()
    }
    source_prefixes = tuple(f"{root.as_posix()}/" for root in SOURCE_ROOTS)
    records = [
        record
        for record in load_json(RESEARCH_INDEX)["records"]
        if record["sourcePath"].startswith(source_prefixes)
    ]
    sources = {
        path.relative_to(disasm).as_posix(): read_upstream_text(path) for path in paths
    }
    summary = {
        "fileCount": len(files),
        "itemFileCount": sum(row["path"].startswith("data/stats/items/") for row in files),
        "spellFileCount": sum(row["path"].startswith("data/stats/spells/") for row in files),
        "enemyFileCount": sum(row["path"].startswith("data/stats/enemies/") for row in files),
        "sourceLineCount": sum(row["sourceLineCount"] for row in files),
        "statementCount": sum(row["statementCount"] for row in files),
        "globalLabelCount": sum(len(row["globalLabels"]) for row in files),
        "layoutIncludedFileCount": len(layout_paths),
        "representativeAddressCount": len(representative_addresses),
        "indexedRecordCount": len(records),
        "indexedFileCount": len({record["sourcePath"] for record in records}),
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "scopes": [root.as_posix() for root in SOURCE_ROOTS],
        "summary": summary,
        "layoutPaths": layout_paths,
        "indexedRecordIds": sorted(record["id"] for record in records),
        "indexedSourcePaths": sorted({record["sourcePath"] for record in records}),
        "representativeSymbols": representative_symbols,
        "representativeAddresses": representative_addresses,
        "facts": _source_facts(sources),
        "runtimeQuestions": [
            "enemy-map-sprite-entries-beyond-definition-count",
            "special-caravan-description-presentation",
            "shop-and-debug-shop-admission-and-ordering",
        ],
        "files": files,
    }


def verify_core_stats_data_inventory(
    upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_core_stats_data_inventory(upstream_path)
    validate_json(output, SCHEMA, owner="core stats data static inventory")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != load_json(ROM_MANIFEST)["hashes"]["sha256"]
    ):
        raise ValueError("core stats data provenance drift")
    if output["summary"] != manifest["summary"]:
        raise ValueError("core stats data summary drift")
    if output["representativeAddresses"] != fixture["table"]:
        raise ValueError("core stats data H1 address drift")
    for field in ("facts", "runtimeQuestions"):
        if output[field] != fixture["expected"][field]:
            raise ValueError(f"core stats data {field} drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"]:
        raise ValueError("core stats data canonical hash drift")
    destination = output_path or repo_path("local/derived/core-stats-data-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Inventory": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Files": output["summary"]["fileCount"],
        "Items": output["summary"]["itemFileCount"],
        "Spells": output["summary"]["spellFileCount"],
        "Enemies": output["summary"]["enemyFileCount"],
        "IndexedFiles": output["summary"]["indexedFileCount"],
        "Status": "PASS",
    }
