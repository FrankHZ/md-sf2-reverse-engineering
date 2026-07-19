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
from sf2tool.source_text import read_upstream_text

ID = "sf2-battle-global-data-static-v1"
SOURCE_ROOT = Path("data/battles/global")
LAYOUT_EXCLUDED = SOURCE_ROOT / "afterbattlejoins.asm"
MANIFEST = repo_path("manifests/extractions/battle-global-data-static.json")
SCHEMA = repo_path("schemas/battle-global-data-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/battle-global-data-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-battle-global-data-static-fixture.schema.json")
RESEARCH_INDEX = repo_path("manifests/research-index.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _statements(source: str) -> list[str]:
    statements: list[str] = []
    pending = ""
    for raw_line in source.splitlines():
        line = raw_line.split(";", 1)[0].strip()
        if not line:
            continue
        line = re.sub(r"^[A-Za-z_][A-Za-z0-9_]*:\s*", "", line)
        if not line:
            continue
        pending = f"{pending} {line}".strip()
        if pending.endswith("&"):
            pending = pending[:-1].rstrip()
            continue
        statements.append(pending)
        pending = ""
    if pending:
        raise ValueError("unterminated source continuation")
    return statements


def _arguments(source: str, directive: str) -> list[str]:
    prefix = f"{directive} "
    bare = directive
    values: list[str] = []
    for statement in _statements(source):
        if statement == bare:
            values.append("")
        elif statement.startswith(prefix):
            values.append(statement[len(prefix) :].strip())
    return values


def _tokens(expression: str) -> list[str]:
    return [token.strip() for token in expression.split(",") if token.strip()]


def _integer(expression: str) -> int:
    expression = expression.strip()
    if expression.startswith("$"):
        return int(expression[1:], 16)
    return int(expression, 10)


def _byte_values(source: str) -> list[int | str]:
    values: list[int | str] = []
    for expression in _arguments(source, "dc.b"):
        for token in _tokens(expression):
            try:
                values.append(_integer(token))
            except ValueError:
                values.append(token)
    return values


def _label_block(source: str, label: str) -> str:
    match = re.search(
        rf"^{re.escape(label)}:\s*(.*?)(?=^[A-Za-z_][A-Za-z0-9_]*:\s*|\Z)",
        source,
        re.MULTILINE | re.DOTALL,
    )
    if not match:
        raise ValueError(f"missing battle-global label: {label}")
    return match.group(1)


def _source_facts(sources: dict[str, str]) -> dict[str, Any]:
    def source(name: str) -> str:
        return sources[f"{SOURCE_ROOT.as_posix()}/{name}"]

    unused_joins = _byte_values(source("afterbattlejoins.asm"))
    after_position_source = source("afterbattlepositions.asm")
    after_position_battles = [
        value
        for value in _arguments(after_position_source, "dc.w")
        if value.startswith("BATTLE_")
    ]
    after_position_pointers = _arguments(after_position_source, "dc.l")
    after_positions = _byte_values(after_position_source)
    if len(after_position_battles) != len(after_position_pointers):
        raise ValueError("after-battle position pointer table drift")
    map_rows = [
        [_integer(token) for token in _tokens(row)]
        for row in _arguments(source("battlemapcoords.asm"), "battleMapCoordinates")
    ]
    if any(len(row) != 7 for row in map_rows):
        raise ValueError("battle map-coordinate row width drift")

    neutral_source = source("battleneutralentities.asm")
    neutral_battles = _arguments(neutral_source, "battle.w")
    neutral_positions = _arguments(neutral_source, "position")
    neutral_facings = _arguments(neutral_source, "facing")
    neutral_sprites = _arguments(neutral_source, "mapsprite")
    neutral_scripts = [
        value
        for value in _arguments(neutral_source, "dc.l")
        if value.startswith("eas_")
    ]
    neutral_entity_count = len(neutral_positions)
    if not (
        neutral_entity_count
        == len(neutral_facings)
        == len(neutral_sprites)
        == len(neutral_scripts)
    ):
        raise ValueError("neutral battle-entity tuple width drift")

    switch_values = _byte_values(source("backgroundenemyswitch.asm"))
    if not all(isinstance(value, int) for value in switch_values):
        raise ValueError("background enemy-switch table is no longer numeric")
    custom_backgrounds = [
        token
        for row in _arguments(source("custombackgrounds.asm"), "background")
        for token in _tokens(row)
    ]
    leader_flags = _byte_values(source("enemyleaderpresence.asm"))
    if not all(isinstance(value, int) for value in leader_flags):
        raise ValueError("enemy leader-presence table is no longer numeric")

    halved_battles = [
        token
        for row in _arguments(source("halvedexpearnedbattles.asm"), "battle")
        for token in _tokens(row)
    ]
    land_entries = _arguments(
        source("landeffectsettingsandmovecosts.asm"), "landEffectAndMoveCost"
    )
    if len(land_entries) % 16:
        raise ValueError("land-effect/move-cost matrix width drift")
    land_rows = [land_entries[index : index + 16] for index in range(0, len(land_entries), 16)]

    laser_source = source("laserbattles.asm")
    laser_battles = _tokens(_arguments(laser_source, "battles")[0])
    laser_labels = (
        "table_LaserEnemyFacingForBattle_VersusPrismFlowers",
        "table_LaserEnemyFacingForBattle_VersusZeon",
        "table_LaserEnemyFacingForBattle_VersusAllBosses",
    )
    laser_rows = [_byte_values(_label_block(laser_source, label)) for label in laser_labels]

    random_source = source("randombattles.asm")
    random_battles = _tokens(_arguments(random_source, "battles")[0])
    upgrade_ranges = [_tokens(value) for value in _arguments(random_source, "upgradeRange")]
    upgrade_exclusions = [
        _tokens(value) for value in _arguments(random_source, "excludedEnemies")
    ]
    if len(upgrade_ranges) != len(upgrade_exclusions):
        raise ValueError("random-battle enemy-upgrade category drift")

    terrain_backgrounds = [
        token
        for row in _arguments(source("terrainbackgrounds.asm"), "background")
        for token in _tokens(row)
    ]

    return {
        "afterBattle": {
            "excludedUnusedJoinByteCount": len(unused_joins),
            "excludedUnusedJoinNonzeroCount": sum(value != 0 for value in unused_joins),
            "positionBattleCount": len(after_position_battles),
            "positionEntryCount": len(after_positions) // 4,
            "positionEntrySizeBytes": 4,
        },
        "battleMaps": {
            "rowCount": len(map_rows),
            "fieldCount": 7,
            "distinctMapCount": len({row[0] for row in map_rows}),
            "triggerOverrideBattleIndexes": [
                index for index, row in enumerate(map_rows) if row[-2:] != [255, 255]
            ],
            "rows": map_rows,
        },
        "neutralEntities": {
            "battleCount": len(neutral_battles),
            "entityCount": neutral_entity_count,
            "battles": neutral_battles,
            "scriptUsage": dict(sorted(Counter(neutral_scripts).items())),
        },
        "backgrounds": {
            "customBattleCount": len(custom_backgrounds),
            "customDistinctCount": len(set(custom_backgrounds)),
            "customByBattle": custom_backgrounds,
            "enemySwitchEntryCount": len(switch_values),
            "enemySwitchEnabledIndexes": [
                index for index, value in enumerate(switch_values) if value == 1
            ],
            "terrainEntryCount": len(terrain_backgrounds),
            "terrainDistinctCount": len(set(terrain_backgrounds)),
            "terrainByType": terrain_backgrounds,
        },
        "enemyLeaders": {
            "battleCount": len(leader_flags),
            "presentBattleIndexes": [
                index for index, value in enumerate(leader_flags) if value == -1
            ],
        },
        "experience": {
            "halvedBattleCount": len(halved_battles),
            "halvedBattles": halved_battles,
        },
        "movement": {
            "moveTypeCount": len(land_rows),
            "terrainSlotCount": 16,
            "entryCount": len(land_entries),
            "obstructedEntryCount": sum(value == "OBSTRUCTED" for value in land_entries),
            "matrix": land_rows,
        },
        "lasers": {
            "battleCount": len(laser_battles),
            "battles": laser_battles,
            "enemyFacingRowLengths": [len(row) for row in laser_rows],
            "activeFacingCounts": [
                sum(value != "LASER_NONE" for value in row) for row in laser_rows
            ],
        },
        "randomBattles": {
            "battleCount": len(random_battles),
            "battles": random_battles,
            "upgradeCategoryCount": len(upgrade_ranges),
            "upgradeRanges": upgrade_ranges,
            "excludedEnemyCounts": [len(row) for row in upgrade_exclusions],
        },
        "existingRailOwnership": {
            "aicommandsets.asm": "battle-ai",
            "aipriority.asm": "battle-ai",
            "aistandbymovements.asm": "battle-ai",
            "enemyitemdrops.asm": "enemy-drops",
            "krakenmovecosts.asm": "battle-ai",
            "swarmbattles.asm": "battle-ai",
        },
    }


def _fact_summary(facts: dict[str, Any]) -> dict[str, Any]:
    return {
        "afterBattle": facts["afterBattle"],
        "battleMaps": {
            key: facts["battleMaps"][key]
            for key in (
                "rowCount",
                "fieldCount",
                "distinctMapCount",
                "triggerOverrideBattleIndexes",
            )
        },
        "neutralEntities": {
            "battleCount": facts["neutralEntities"]["battleCount"],
            "entityCount": facts["neutralEntities"]["entityCount"],
            "distinctScriptCount": len(facts["neutralEntities"]["scriptUsage"]),
        },
        "backgrounds": {
            key: facts["backgrounds"][key]
            for key in (
                "customBattleCount",
                "customDistinctCount",
                "enemySwitchEntryCount",
                "enemySwitchEnabledIndexes",
                "terrainEntryCount",
                "terrainDistinctCount",
            )
        },
        "enemyLeaders": {
            "battleCount": facts["enemyLeaders"]["battleCount"],
            "presentBattleCount": len(facts["enemyLeaders"]["presentBattleIndexes"]),
        },
        "experience": facts["experience"],
        "movement": {
            key: facts["movement"][key]
            for key in (
                "moveTypeCount",
                "terrainSlotCount",
                "entryCount",
                "obstructedEntryCount",
            )
        },
        "lasers": {
            key: facts["lasers"][key]
            for key in (
                "battleCount",
                "enemyFacingRowLengths",
                "activeFacingCounts",
            )
        },
        "randomBattles": {
            key: facts["randomBattles"][key]
            for key in (
                "battleCount",
                "upgradeCategoryCount",
                "excludedEnemyCounts",
            )
        },
        "existingRailOwnership": facts["existingRailOwnership"],
    }


def build_battle_global_data_inventory(upstream_path: Path) -> dict[str, Any]:
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"battle-global H1 listing is missing: {listing_path}")
    listing = listing_path.read_text(encoding="utf-8")
    root = disasm / SOURCE_ROOT
    paths = sorted(root.glob("*.asm"))
    if len(paths) != 18:
        raise ValueError(
            f"battle-global directory boundary drift: expected 18 files, got {len(paths)}"
        )
    files = [_parse_source_file(path, path.relative_to(disasm).as_posix()) for path in paths]
    if any(not row["globalLabels"] for row in files):
        raise ValueError("battle-global data unexpectedly contains an unlabeled file")

    layout = "\n".join(
        path.read_text(encoding="utf-8") for path in sorted((disasm / "layout").glob("*.asm"))
    )
    included_paths = [
        row["path"] for row in files if row["path"].replace("/", "\\") in layout
    ]
    excluded_paths = sorted({row["path"] for row in files} - set(included_paths))
    expected_excluded = [LAYOUT_EXCLUDED.as_posix()]
    if excluded_paths != expected_excluded:
        raise ValueError(
            "battle-global layout exclusion drift: "
            f"expected {expected_excluded}, got {excluded_paths}"
        )

    included_rows = [row for row in files if row["path"] in included_paths]
    representative_symbols = {row["path"]: row["globalLabels"][0] for row in included_rows}
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
    summary = {
        "fileCount": len(files),
        "sourceLineCount": sum(row["sourceLineCount"] for row in files),
        "statementCount": sum(row["statementCount"] for row in files),
        "globalLabelCount": sum(len(row["globalLabels"]) for row in files),
        "layoutIncludedFileCount": len(included_paths),
        "layoutExcludedFileCount": len(excluded_paths),
        "representativeAddressCount": len(representative_addresses),
        "indexedRecordCount": len(records),
        "indexedFileCount": len({record["sourcePath"] for record in records}),
    }
    facts = _source_facts(sources)
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "scope": SOURCE_ROOT.as_posix(),
        "summary": summary,
        "layoutIncludedPaths": included_paths,
        "layoutExcludedPaths": excluded_paths,
        "layoutExclusionReason": (
            "The global afterbattlejoins source is an unused all-zero alternate; the original "
            "layout includes data/battles/cutscenes/afterbattlejoins.asm instead."
        ),
        "indexedRecordIds": sorted(record["id"] for record in records),
        "indexedSourcePaths": sorted({record["sourcePath"] for record in records}),
        "representativeSymbols": representative_symbols,
        "representativeAddresses": representative_addresses,
        "factSummary": _fact_summary(facts),
        "facts": facts,
        "runtimeQuestions": [
            "after-battle-position-consumer-and-ignored-byte-behavior",
            "neutral-entity-script-timing-and-presentation",
            "background-switch-visual-orientation",
            "random-battle-upgrade-caller-boundaries",
        ],
        "files": files,
    }


def verify_battle_global_data_inventory(
    upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_battle_global_data_inventory(upstream_path)
    validate_json(output, SCHEMA, owner="battle-global data static inventory")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != load_json(ROM_MANIFEST)["hashes"]["sha256"]
    ):
        raise ValueError("battle-global data provenance drift")
    if output["summary"] != manifest["summary"]:
        raise ValueError("battle-global data summary drift")
    if output["representativeAddresses"] != fixture["table"]:
        raise ValueError("battle-global H1 address drift")
    for field in ("factSummary", "runtimeQuestions"):
        if output[field] != fixture["expected"][field]:
            raise ValueError(f"battle-global data {field} drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"]:
        raise ValueError("battle-global data canonical hash drift")
    destination = output_path or repo_path("local/derived/battle-global-data-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Inventory": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Files": output["summary"]["fileCount"],
        "LayoutIncluded": output["summary"]["layoutIncludedFileCount"],
        "IndexedFiles": output["summary"]["indexedFileCount"],
        "RuntimeQuestions": len(output["runtimeQuestions"]),
        "Status": "PASS",
    }
