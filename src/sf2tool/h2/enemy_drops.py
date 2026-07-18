from __future__ import annotations

import hashlib
import json
import re
import subprocess
from pathlib import Path
from typing import Any

from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.rom import inspect_rom

FIXTURE = repo_path("tests/fixtures/h2/enemy-item-drops-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-enemy-item-drops-fixture.schema.json")
SCHEMA = repo_path("schemas/enemy-item-drops-data.schema.json")
MANIFEST = repo_path("manifests/extractions/enemy-item-drops-data.json")
SOURCE_PATH = Path("data/battles/global/enemyitemdrops.asm")
CONSUMER_PATH = Path("code/gameflow/battle/battleactions/dropenemyitem.asm")
EQUATE_PATTERN = re.compile(r"^([A-Z][A-Z0-9_]+):\s+equ\s+([^\s;]+)", re.MULTILINE)


def _integer(value: str) -> int:
    if value.startswith("$"):
        return int(value[1:], 16)
    return int(value, 10)


def _equates(disasm: Path) -> dict[str, int]:
    source = (disasm / "sf2enums.asm").read_text(encoding="utf-8")
    values: dict[str, int] = {}
    for name, expression in EQUATE_PATTERN.findall(source):
        try:
            values[name] = _integer(expression)
        except ValueError:
            continue
    return values


def _parse_source(disasm: Path, equates: dict[str, int]) -> list[dict[str, int | None]]:
    source = (disasm / SOURCE_PATH).read_text(encoding="utf-8")
    fields = {
        "battle": re.findall(r"^\s*battle\s+([A-Z0-9_]+)", source, re.MULTILINE),
        "entity": re.findall(r"^\s*enemyEntity\s+(\d+)", source, re.MULTILINE),
        "item": re.findall(r"^\s*item\s+([A-Z0-9_]+)", source, re.MULTILINE),
        "flag": re.findall(r"^\s*droppedFlag\s+(\d+)", source, re.MULTILINE),
    }
    lengths = {len(values) for values in fields.values()}
    if lengths != {30} or "tableEnd.w" not in source:
        raise ValueError(f"enemy item drop source shape drift: {lengths}")
    random_items = {
        equates["ITEM_TAROS_SWORD"],
        equates["ITEM_IRON_BALL"],
        equates["ITEM_COUNTER_SWORD"],
    }
    entries = []
    for index in range(30):
        item = equates[f"ITEM_{fields['item'][index]}"]
        entity = int(fields["entity"][index])
        entries.append(
            {
                "battle": equates[f"BATTLE_{fields['battle'][index]}"],
                "entity": entity,
                "combatant": 128 + entity,
                "item": item,
                "flag": int(fields["flag"][index]),
                "randomChanceRange": 32 if item in random_items else None,
            }
        )
    return entries


def _verify_consumer(disasm: Path) -> None:
    source = (disasm / CONSUMER_PATH).read_text(encoding="utf-8")
    required = (
        "battlesceneScript_DropEnemyItem:",
        "lea     table_EnemyItemDrops(pc), a0",
        "moveq   #ENEMYITEMDROP_RANDOM_CHANCE,d0",
        "lea     ((ENEMY_ITEM_DROPPED_FLAGS-$1000000)).w,a0",
        "jsr     RemoveItemBySlot",
        "jsr     AddItemToDeals",
        "jsr     j_IsBattleUpgradable ; unreachable code",
    )
    if any(fragment not in source for fragment in required):
        raise ValueError("enemy item drop consumer source contract drift")


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def verify_enemy_item_drops(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    upstream_path = upstream_path.resolve(strict=True)
    rom_path = rom_path.resolve(strict=True)
    commit = subprocess.run(
        ["git", "rev-parse", "HEAD"], cwd=upstream_path, check=True, capture_output=True,
        text=True, encoding="utf-8"
    ).stdout.strip()
    rom_identity = inspect_rom(rom_path)
    if commit != fixture["upstreamCommit"] or rom_identity["sha256"] != fixture["romSha256"]:
        raise ValueError("enemy item drop input identity mismatch")

    disasm = upstream_path / "disasm"
    entries = _parse_source(disasm, _equates(disasm))
    _verify_consumer(disasm)
    addresses = fixture["function"]
    data = rom_path.read_bytes()[addresses["tableAddress"] : addresses["endAddress"]]
    source_bytes = bytearray()
    for entry in entries:
        source_bytes.extend((entry["battle"], entry["combatant"], entry["item"], entry["flag"]))
    source_bytes.extend(b"\xFF\xFF")
    if data != source_bytes:
        mismatch = next(
            index for index, (source, rom) in enumerate(zip(source_bytes, data, strict=True))
            if source != rom
        )
        raise ValueError(
            f"enemy item drop source-ROM parity mismatch at byte {mismatch}: "
            f"source={source_bytes[mismatch]}, ROM={data[mismatch]}"
        )

    facts = {
        "entryCount": len(entries),
        "uniqueBattleCount": len({entry["battle"] for entry in entries}),
        "minimumFlag": min(entry["flag"] for entry in entries),
        "maximumFlag": max(entry["flag"] for entry in entries),
        "randomDropCount": sum(entry["randomChanceRange"] == 32 for entry in entries),
        "randomDropRange": 32,
        "maximumEntity": max(entry["entity"] for entry in entries),
        "terminator": int.from_bytes(data[-2:], "big"),
    }
    if facts != fixture["expected"]:
        raise ValueError("enemy item drop table shape disagrees with fixture")
    if addresses["terminatorAddress"] != addresses["tableAddress"] + len(entries) * 4:
        raise ValueError("enemy item drop terminator boundary drift")

    output = {
        "schemaVersion": 1,
        "id": fixture["id"],
        "upstreamCommit": commit,
        "romSha256": rom_identity["sha256"],
        "sourcePath": SOURCE_PATH.as_posix(),
        "romRange": {
            "start": addresses["tableAddress"],
            "terminatorAddress": addresses["terminatorAddress"],
            "endExclusive": addresses["endAddress"],
            "entrySize": 4,
        },
        "entries": entries,
        "terminator": facts["terminator"],
    }
    validate_json(output, SCHEMA, owner="enemy item drop extraction")
    encoded = _canonical_bytes(output)
    digest = hashlib.sha256(encoded).hexdigest().upper()
    if manifest["outputSha256"] != "PENDING" and digest != manifest["outputSha256"]:
        raise ValueError(
            "enemy item drop extraction hash mismatch: "
            f"expected {manifest['outputSha256']}, got {digest}"
        )
    destination = output_path or repo_path(manifest["outputPath"])
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(encoded)
    return {
        "Fixture": fixture["id"],
        "Output": display_path(destination),
        "SHA256": digest,
        "Entries": len(entries),
        "Battles": facts["uniqueBattleCount"],
        "RandomDrops": facts["randomDropCount"],
        "SourceRomMismatches": 0,
        "Status": "PASS",
    }
