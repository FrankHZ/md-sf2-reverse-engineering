from __future__ import annotations

import hashlib
import json
import re
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_global_data import _arguments, _tokens
from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.h3.growth import _parse_equates
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.rom import inspect_rom
from sf2tool.source_text import read_upstream_text

ID = "sf2-enemy-map-sprites-static-v1"
MANIFEST = repo_path("manifests/extractions/enemy-map-sprites-static.json")
SCHEMA = repo_path("schemas/enemy-map-sprites-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/enemy-map-sprites-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-enemy-map-sprites-static-fixture.schema.json")

TABLE_PATH = Path("data/stats/enemies/enemymapsprites.asm")
CONSUMER_PATH = Path("code/common/scripting/entity/getcombatantmapsprite.asm")
INITIALIZER_PATH = Path("code/gameflow/battle/battleloop/initializecombatants.asm")
UPGRADE_PATH = Path("data/battles/global/randombattles.asm")
SPRITESET_ROOT = Path("data/battles/spritesets")
TABLE_SYMBOL = "table_EnemyMapsprites"
CONSUMER_SYMBOL = "GetCombatantMapsprite"


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _named_value(code: str, prefix: str, equates: dict[str, int]) -> dict[str, Any]:
    name = f"{prefix}{code}"
    if name not in equates:
        raise ValueError(f"enemy map-sprite token is undefined: {name}")
    return {"code": code, "value": equates[name]}


def _assert_fragments(source: str, owner: str, fragments: tuple[str, ...]) -> None:
    missing = [fragment for fragment in fragments if fragment not in source]
    if missing:
        raise ValueError(f"{owner} source contract drift: {missing}")


def build_enemy_map_sprites_contract(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"enemy map-sprite H1 listing is missing: {listing_path}")
    addresses = listing_symbol_addresses(listing_path.read_text(encoding="utf-8"))
    equates = _parse_equates(disasm)
    rom_identity = inspect_rom(rom_path)

    source = read_upstream_text(disasm / TABLE_PATH)
    sprite_codes = [
        token
        for expression in _arguments(source, "mapsprite")
        for token in _tokens(expression)
    ]
    sprites = [_named_value(code, "MAPSPRITE_", equates) for code in sprite_codes]
    if len(sprites) != 166:
        raise ValueError(f"enemy map-sprite row count drift: {len(sprites)}")

    enemy_equates = sorted(
        (
            (value, name.removeprefix("ENEMY_"))
            for name, value in equates.items()
            if name.startswith("ENEMY_") and value <= 102
        ),
        key=lambda row: row[0],
    )
    if [value for value, _ in enemy_equates] != list(range(103)):
        raise ValueError("enemy definition enum range is not exactly 0..102")
    definition_rows = [
        {
            "enemyIndex": enemy_index,
            "enemyCode": enemy_code,
            "mapSprite": sprites[enemy_index],
        }
        for enemy_index, enemy_code in enemy_equates
    ]
    tail_rows = [
        {"tableIndex": index, "mapSprite": sprites[index]} for index in range(103, 166)
    ]
    tail_values = [row["mapSprite"]["value"] for row in tail_rows]
    tail_counts = Counter(tail_values)

    table_address = addresses[TABLE_SYMBOL]
    expected_bytes = bytes(sprite["value"] for sprite in sprites)
    actual_bytes = rom_path.read_bytes()[table_address : table_address + len(expected_bytes)]
    if actual_bytes != expected_bytes:
        mismatch = next(
            index
            for index, (expected, actual) in enumerate(
                zip(expected_bytes, actual_bytes, strict=True)
            )
            if expected != actual
        )
        raise ValueError(
            f"enemy map-sprite source-ROM mismatch at +{mismatch}: "
            f"source={expected_bytes[mismatch]}, ROM={actual_bytes[mismatch]}"
        )

    consumer = read_upstream_text(disasm / CONSUMER_PATH)
    _assert_fragments(
        consumer,
        "enemy map-sprite consumer",
        (
            f"{CONSUMER_SYMBOL}:",
            "jsr     j_GetEnemy",
            "move.b  table_EnemyMapsprites(pc,d1.w),d4",
        ),
    )
    consumer_body = consumer.split("@GetEnemyMapsprite:", 1)[1].split("@Done:", 1)[0]
    if any(token in consumer_body for token in ("cmp", "andi", "bhi", "bcc", "bcs")):
        raise ValueError("enemy map-sprite consumer unexpectedly gained an index guard")

    spriteset_sources = [
        read_upstream_text(path)
        for path in sorted((disasm / SPRITESET_ROOT).glob("spriteset[0-9][0-9].asm"))
    ]
    battle_enemy_codes = [
        match
        for spriteset in spriteset_sources
        for match in re.findall(r"^\s*enemyCombatant\s+([A-Z0-9_]+)", spriteset, re.MULTILINE)
    ]
    battle_enemy_indexes = [equates[f"ENEMY_{code}"] for code in battle_enemy_codes]
    if len(battle_enemy_indexes) != 627 or max(battle_enemy_indexes) > 102:
        raise ValueError("battle spriteset enemy-index boundary drift")

    upgrade_source = read_upstream_text(disasm / UPGRADE_PATH)
    upgrade_ranges = []
    for expression in _arguments(upgrade_source, "upgradeRange"):
        step, first_code, last_code = _tokens(expression)
        upgrade_ranges.append(
            {
                "step": int(step),
                "first": _named_value(first_code, "ENEMY_", equates),
                "last": _named_value(last_code, "ENEMY_", equates),
            }
        )
    if len(upgrade_ranges) != 5:
        raise ValueError("enemy upgrade-range count drift")
    upgrade_maximum = max(row["last"]["value"] for row in upgrade_ranges)

    code_sources = [
        read_upstream_text(path) for path in sorted((disasm / "code").rglob("*.asm"))
    ]
    setter_calls = sum(source.count("jsr     j_SetEnemyIndex") for source in code_sources)
    initializer = read_upstream_text(disasm / INITIALIZER_PATH)
    _assert_fragments(
        initializer,
        "enemy-index initializer",
        (
            "bsr.w   UpgradeRandomBattleEnemies",
            "move.w  d1,d6",
            "move.b  d6,d1",
            "jsr     j_SetEnemyIndex",
        ),
    )
    if setter_calls != 1:
        raise ValueError(f"named enemy-index writer call count drift: {setter_calls}")

    definition_values = {row["mapSprite"]["value"] for row in definition_rows}
    tail_missing = sorted(set(range(min(tail_values), max(tail_values) + 1)) - set(tail_values))
    tail_duplicates = {
        str(value): count for value, count in sorted(tail_counts.items()) if count > 1
    }
    summary = {
        "tableRowCount": len(sprites),
        "definitionRowCount": len(definition_rows),
        "tailRowCount": len(tail_rows),
        "tableByteCount": len(expected_bytes),
        "battleSpritesetReferenceCount": len(battle_enemy_indexes),
        "battleSpritesetUniqueEnemyCount": len(set(battle_enemy_indexes)),
        "battleSpritesetMaximumEnemyIndex": max(battle_enemy_indexes),
        "upgradeRangeCount": len(upgrade_ranges),
        "upgradeMaximumEnemyIndex": upgrade_maximum,
        "namedSetterCallCount": setter_calls,
        "tailUniqueMapSpriteCount": len(set(tail_values)),
        "tailMinimumMapSprite": min(tail_values),
        "tailMaximumMapSprite": max(tail_values),
        "tailDefinitionSpriteOverlapCount": len(definition_values & set(tail_values)),
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "romSha256": rom_identity["sha256"],
        "table": {
            TABLE_SYMBOL: table_address,
            CONSUMER_SYMBOL: addresses[CONSUMER_SYMBOL],
        },
        "summary": summary,
        "romRange": {
            "sourcePath": TABLE_PATH.as_posix(),
            "start": table_address,
            "endExclusive": table_address + len(expected_bytes),
            "sha256": hashlib.sha256(expected_bytes).hexdigest().upper(),
        },
        "definitionRows": definition_rows,
        "tailRows": tail_rows,
        "tailFacts": {
            "firstTableIndex": 103,
            "lastTableIndex": 165,
            "missingMapSpriteValuesInsideTailRange": tail_missing,
            "duplicateMapSpriteValues": tail_duplicates,
            "normalBattleInitializationCanReachTail": False,
            "consumerHasBoundsCheck": False,
            "tailRequiresEnemyIndexRange": {"minimum": 103, "maximum": 165},
        },
        "normalSourceDomain": {
            "enemyDefinitionIndexRange": {"minimum": 0, "maximum": 102},
            "battleSpritesetMissingEnemyIndexes": sorted(
                set(range(103)) - set(battle_enemy_indexes)
            ),
            "upgradeRanges": upgrade_ranges,
            "onlyNamedSetterCaller": INITIALIZER_PATH.as_posix(),
        },
        "consumerRules": {
            "enemyIndexSource": "GetEnemy reads the combatant enemy-index byte",
            "lookup": "unsigned byte table_EnemyMapsprites[enemyIndex]",
            "normalBoundary": (
                "pinned battle spritesets and upgrade ranges remain within enemy definitions 0..102"
            ),
            "tailBoundary": (
                "rows 103..165 require a nonstandard raw/debug/corrupt enemy-index byte"
            ),
        },
        "runtimeQuestions": [
            "Can any original debug, malformed save-state, or raw RAM path intentionally assign an "
            "enemy index in 103..165 and expose the NPC-sprite tail?"
        ],
    }


def verify_enemy_map_sprites_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_enemy_map_sprites_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="enemy map-sprites static contract")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != output["romSha256"]
    ):
        raise ValueError("enemy map-sprite provenance drift")
    for field in (
        "table",
        "summary",
        "romRange",
        "tailFacts",
        "normalSourceDomainFacts",
        "consumerRules",
        "runtimeQuestions",
    ):
        if field == "normalSourceDomainFacts":
            actual = {
                "enemyDefinitionIndexRange": output["normalSourceDomain"][
                    "enemyDefinitionIndexRange"
                ],
                "battleSpritesetMissingEnemyIndexes": output["normalSourceDomain"][
                    "battleSpritesetMissingEnemyIndexes"
                ],
                "onlyNamedSetterCaller": output["normalSourceDomain"][
                    "onlyNamedSetterCaller"
                ],
            }
        else:
            actual = output[field]
        if fixture[field] != actual:
            raise ValueError(f"enemy map-sprite fixture drift: {field}")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if output["summary"] != manifest["summary"] or digest != manifest["outputSha256"]:
        raise ValueError("enemy map-sprite canonical manifest drift")
    destination = output_path or repo_path("local/derived/enemy-map-sprites-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Rows": output["summary"]["tableRowCount"],
        "DefinitionRows": output["summary"]["definitionRowCount"],
        "TailRows": output["summary"]["tailRowCount"],
        "Status": "PASS",
    }
