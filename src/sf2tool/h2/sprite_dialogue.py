from __future__ import annotations

import hashlib
import json
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

ID = "sf2-sprite-dialogue-static-v1"
MANIFEST = repo_path("manifests/extractions/sprite-dialogue-static.json")
SCHEMA = repo_path("schemas/sprite-dialogue-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/sprite-dialogue-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-sprite-dialogue-static-fixture.schema.json")

TABLE_PATH = Path("data/spritedialogproperties.asm")
CONSUMER_PATH = Path("code/common/scripting/entity/getentityportaitandspeechsfx.asm")
TABLE_SYMBOL = "table_MapspriteDialogueProperties"
CONSUMER_SYMBOL = "GetEntityPortaitAndSpeechSfx"


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _named_value(code: str, prefix: str, equates: dict[str, int]) -> dict[str, Any]:
    name = f"{prefix}{code}"
    if name not in equates:
        raise ValueError(f"sprite-dialogue token is undefined: {name}")
    return {"code": code, "value": equates[name]}


def _assert_fragments(source: str, owner: str, fragments: tuple[str, ...]) -> None:
    missing = [fragment for fragment in fragments if fragment not in source]
    if missing:
        raise ValueError(f"{owner} source contract drift: {missing}")


def build_sprite_dialogue_contract(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"sprite-dialogue H1 listing is missing: {listing_path}")
    addresses = listing_symbol_addresses(listing_path.read_text(encoding="utf-8"))
    equates = _parse_equates(disasm)
    rom_identity = inspect_rom(rom_path)

    source = read_upstream_text(disasm / TABLE_PATH)
    columns = {
        "mapSprite": [
            token
            for expression in _arguments(source, "mapsprite")
            for token in _tokens(expression)
        ],
        "portrait": [
            token
            for expression in _arguments(source, "portrait")
            for token in _tokens(expression)
        ],
        "speechSfx": [
            token
            for expression in _arguments(source, "speechSfx")
            for token in _tokens(expression)
        ],
    }
    column_lengths = {name: len(values) for name, values in columns.items()}
    if set(column_lengths.values()) != {119}:
        raise ValueError(f"sprite-dialogue column alignment drift: {column_lengths}")

    rows = []
    encoded = bytearray()
    for index, (map_sprite, portrait, speech_sfx) in enumerate(
        zip(columns["mapSprite"], columns["portrait"], columns["speechSfx"], strict=True)
    ):
        row = {
            "index": index,
            "mapSprite": _named_value(map_sprite, "MAPSPRITE_", equates),
            "portrait": _named_value(portrait, "PORTRAIT_", equates),
            "speechSfx": _named_value(speech_sfx, "SFX_", equates),
            "reserved": 0,
        }
        rows.append(row)
        encoded.extend(
            (
                row["mapSprite"]["value"],
                row["portrait"]["value"],
                row["speechSfx"]["value"],
                row["reserved"],
            )
        )
    encoded.extend((0xFF, 0xFF))

    table_address = addresses[TABLE_SYMBOL]
    actual = rom_path.read_bytes()[table_address : table_address + len(encoded)]
    if actual != encoded:
        mismatch = next(
            index
            for index, (expected_byte, actual_byte) in enumerate(
                zip(encoded, actual, strict=True)
            )
            if expected_byte != actual_byte
        )
        raise ValueError(
            f"sprite-dialogue source-ROM mismatch at +{mismatch}: "
            f"source={encoded[mismatch]}, ROM={actual[mismatch]}"
        )

    consumer = read_upstream_text(disasm / CONSUMER_PATH)
    _assert_fragments(
        consumer,
        "sprite-dialogue consumer",
        (
            f"{CONSUMER_SYMBOL}:",
            "andi.w  #COMBATANT_MASK_ALL,d0",
            "move.b  ENTITYDEF_OFFSET_MAPSPRITE(a5),d0",
            "cmp.b   (a0),d0",
            "move.b  MAPSPRITEDIALOGUEDEF_OFFSET_PORTRAIT(a0),d1",
            "ext.w   d1",
            "move.b  MAPSPRITEDIALOGUEDEF_OFFSET_SPEECHSFX(a0),d2",
            "adda.w  #MAPSPRITEDIALOGUEDEF_ENTRY_SIZE,a0",
            "cmpi.w  #-1,(a0)",
            "move.w  #PORTRAIT_DEFAULT,d1",
            "move.w  #SFX_DIALOG_BLEEP_6,d2",
        ),
    )

    map_sprite_counts = Counter(row["mapSprite"]["value"] for row in rows)
    portrait_counts = Counter(row["portrait"]["value"] for row in rows)
    speech_counts = Counter(row["speechSfx"]["value"] for row in rows)
    duplicate_sprites = {
        str(value): count for value, count in sorted(map_sprite_counts.items()) if count > 1
    }
    speech_histogram = {
        next(row["speechSfx"]["code"] for row in rows if row["speechSfx"]["value"] == value): count
        for value, count in sorted(speech_counts.items())
    }
    portrait_none = equates["PORTRAIT_NONE"]
    summary = {
        "rowCount": len(rows),
        "recordByteCount": len(rows) * 4,
        "tableByteCount": len(encoded),
        "uniqueMapSpriteCount": len(map_sprite_counts),
        "duplicateMapSpriteValueCount": len(duplicate_sprites),
        "uniquePortraitValueCount": len(portrait_counts),
        "portraitNoneRowCount": portrait_counts[portrait_none],
        "portraitBearingRowCount": len(rows) - portrait_counts[portrait_none],
        "uniqueSpeechSfxCount": len(speech_counts),
        "reservedZeroByteCount": sum(row["reserved"] == 0 for row in rows),
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
            "endExclusive": table_address + len(encoded),
            "sha256": hashlib.sha256(encoded).hexdigest().upper(),
        },
        "rows": rows,
        "tableFacts": {
            "recordSize": 4,
            "terminatorWord": 65535,
            "duplicateMapSpriteValues": duplicate_sprites,
            "speechSfxHistogram": speech_histogram,
            "reservedByteIsAlwaysZero": True,
        },
        "consumerRules": {
            "lookupKey": (
                "entity map-sprite byte after masking character index with "
                "COMBATANT_MASK_ALL"
            ),
            "matchPolicy": "linear scan; first matching map-sprite row wins",
            "portraitResult": "portrait byte is sign-extended, so PORTRAIT_NONE (255) returns -1",
            "speechSfxResult": "speech-SFX byte is returned unsigned; the reserved byte is ignored",
            "termination": (
                "after each miss, advance four bytes and stop when the next word is 0xFFFF"
            ),
            "fallback": "PORTRAIT_DEFAULT (-1) and SFX_DIALOG_BLEEP_6 (74)",
        },
        "runtimeQuestions": [
            "Which dialogue call paths rely on fallback values for map sprites absent from this "
            "table?",
            "How do portrait suppression and speech-SFX timing interact with the text "
            "presentation loop?",
        ],
    }


def verify_sprite_dialogue_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_sprite_dialogue_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="sprite-dialogue static contract")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != output["romSha256"]
    ):
        raise ValueError("sprite-dialogue provenance drift")
    for field in (
        "table",
        "summary",
        "romRange",
        "tableFacts",
        "consumerRules",
        "runtimeQuestions",
    ):
        if fixture[field] != output[field]:
            raise ValueError(f"sprite-dialogue fixture drift: {field}")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if output["summary"] != manifest["summary"] or digest != manifest["outputSha256"]:
        raise ValueError("sprite-dialogue canonical manifest drift")
    destination = output_path or repo_path("local/derived/sprite-dialogue-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Rows": output["summary"]["rowCount"],
        "PortraitBearingRows": output["summary"]["portraitBearingRowCount"],
        "Status": "PASS",
    }
