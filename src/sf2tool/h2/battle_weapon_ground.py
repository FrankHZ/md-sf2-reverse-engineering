from __future__ import annotations

import hashlib
import json
import re
from pathlib import Path
from typing import Any

from sf2tool.compression import decode_stack_compressed
from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.source_text import read_upstream_text

ID = "sf2-battle-weapon-ground-decode-v1"
WEAPON_PATH = Path("data/graphics/battles/weapons/entries.asm")
WEAPON_PALETTE_PATH = Path("data/graphics/battles/weapons/palettes/entries.asm")
GROUND_PATH = Path("data/graphics/battles/grounds/entries.asm")
LOADER_PATH = Path("code/gameflow/battle/battlescenes/battlesceneengine_1.asm")
MANIFEST = repo_path("manifests/extractions/battle-weapon-ground-decode.json")
SCHEMA = repo_path("schemas/battle-weapon-ground-decode.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/battle-weapon-ground-decode-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-battle-weapon-ground-decode-fixture.schema.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _stream_record(data: bytes, *, expected_bytes: int) -> dict[str, Any]:
    decoded = decode_stack_compressed(data, expected_output_bytes=expected_bytes)
    return {
        "compressedBytes": len(data),
        "inputBitsConsumed": decoded.input_bits_consumed,
        "trailingBits": len(data) * 8 - decoded.input_bits_consumed,
        "decodedBytes": len(decoded.output),
        "decodedSha256": hashlib.sha256(decoded.output).hexdigest().upper(),
        "commandGroupCount": decoded.command_group_count,
        "literalWordCount": decoded.literal_word_count,
        "copyCommandCount": decoded.copy_command_count,
        "copiedWordCount": decoded.copied_word_count,
        "maximumCopyOffsetWords": decoded.maximum_copy_offset_words,
        "maximumCopyLengthWords": decoded.maximum_copy_length_words,
    }


def _stream_summary(rows: list[dict[str, Any]]) -> dict[str, Any]:
    return {
        "streamCount": len(rows),
        "compressedByteCount": sum(row["compressedBytes"] for row in rows),
        "decodedByteCount": sum(row["decodedBytes"] for row in rows),
        "commandGroupCount": sum(row["commandGroupCount"] for row in rows),
        "literalWordCount": sum(row["literalWordCount"] for row in rows),
        "copyCommandCount": sum(row["copyCommandCount"] for row in rows),
        "copiedWordCount": sum(row["copiedWordCount"] for row in rows),
        "maximumCopyOffsetWords": max(row["maximumCopyOffsetWords"] for row in rows),
        "maximumCopyLengthWords": max(row["maximumCopyLengthWords"] for row in rows),
        "minimumTrailingBits": min(row["trailingBits"] for row in rows),
        "maximumTrailingBits": max(row["trailingBits"] for row in rows),
    }


def build_battle_weapon_ground_contract(
    rom_path: Path, upstream_path: Path
) -> dict[str, Any]:
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"battle weapon/ground H1 listing is missing: {listing_path}")
    addresses = listing_symbol_addresses(listing_path.read_text(encoding="utf-8"))
    rom = rom_path.read_bytes()
    rom_hash = hashlib.sha256(rom).hexdigest().upper()
    if rom_hash != load_json(ROM_MANIFEST)["hashes"]["sha256"]:
        raise ValueError("battle weapon/ground input ROM identity drift")

    weapon_source = read_upstream_text(disasm / WEAPON_PATH)
    weapon_definitions = re.findall(
        r'^\s*(Weaponsprite\d{2}):\s*incbin\s+"([^"]+)"',
        weapon_source,
        re.MULTILINE | re.IGNORECASE,
    )
    weapon_references = re.findall(
        r"\bdc\.l\s+(Weaponsprite\d{2})\b",
        weapon_source[: weapon_source.find("Weaponsprite00:")],
    )
    if len(weapon_references) != 23 or weapon_references != [
        symbol for symbol, _ in weapon_definitions
    ]:
        raise ValueError("weapon sprite pointer/definition shape drift")
    weapon_table_address = addresses["pt_Weaponsprites"]
    weapon_pointer_bytes = b"".join(
        addresses[symbol].to_bytes(4, "big") for symbol in weapon_references
    )
    if (
        rom[weapon_table_address : weapon_table_address + len(weapon_pointer_bytes)]
        != weapon_pointer_bytes
    ):
        raise ValueError("weapon sprite pointer-table ROM parity drift")
    weapon_sprites = []
    for index, (symbol, relative_path) in enumerate(weapon_definitions):
        data = (disasm / relative_path).read_bytes()
        source_address = addresses[symbol]
        if rom[source_address : source_address + len(data)] != data:
            raise ValueError(f"weapon sprite ROM parity drift: {symbol}")
        weapon_sprites.append(
            {
                "index": index,
                "symbol": symbol,
                "sourcePath": relative_path,
                "sourceAddress": source_address,
                **_stream_record(data, expected_bytes=8192),
            }
        )

    palette_source = read_upstream_text(disasm / WEAPON_PALETTE_PATH)
    palette_definitions = re.findall(
        r'^\s*(WeaponPalette\d{2}):\s*incbin\s+"([^"]+)"',
        palette_source,
        re.MULTILINE | re.IGNORECASE,
    )
    if len(palette_definitions) != 42:
        raise ValueError(f"weapon palette count drift: {len(palette_definitions)}")
    weapon_palettes = []
    for index, (symbol, relative_path) in enumerate(palette_definitions):
        data = (disasm / relative_path).read_bytes()
        source_address = addresses[symbol]
        if len(data) != 4 or rom[source_address : source_address + 4] != data:
            raise ValueError(f"weapon palette ROM parity drift: {symbol}")
        weapon_palettes.append(
            {
                "index": index,
                "symbol": symbol,
                "sourcePath": relative_path,
                "sourceAddress": source_address,
                "paletteBytes": 4,
                "paletteSha256": hashlib.sha256(data).hexdigest().upper(),
            }
        )
    if [row["sourceAddress"] for row in weapon_palettes] != list(
        range(addresses["WeaponPalette00"], addresses["WeaponPalette00"] + 42 * 4, 4)
    ):
        raise ValueError("weapon palette contiguous layout drift")

    ground_source = read_upstream_text(disasm / GROUND_PATH)
    ground_references = re.findall(
        r"\bdc\.l\s+(Ground\d{2})\b", ground_source[: ground_source.find("Ground00:")]
    )
    ground_definitions = re.findall(
        r'^\s*(Ground\d{2}):\s*incbin\s+"([^"]+)"\s*\n'
        r'\s*(bsg\d{2}_rpbase):\s*dc\.w\s+(GroundTiles\d{2})-\3',
        ground_source,
        re.MULTILINE | re.IGNORECASE,
    )
    tile_definitions = re.findall(
        r'^\s*(GroundTiles\d{2}):\s*incbin\s+"([^"]+)"',
        ground_source,
        re.MULTILINE | re.IGNORECASE,
    )
    if len(ground_references) != 30 or len(ground_definitions) != 27 or len(tile_definitions) != 10:
        raise ValueError(
            "ground source-shape drift: "
            f"{len(ground_references)} pointers, {len(ground_definitions)} headers, "
            f"{len(tile_definitions)} tilesets"
        )
    ground_table_address = addresses["pt_Grounds"]
    ground_pointer_bytes = b"".join(
        addresses[symbol].to_bytes(4, "big") for symbol in ground_references
    )
    if (
        rom[ground_table_address : ground_table_address + len(ground_pointer_bytes)]
        != ground_pointer_bytes
    ):
        raise ValueError("ground pointer-table ROM parity drift")

    ground_headers = []
    for symbol, palette_path, relative_base, tile_symbol in ground_definitions:
        palette = (disasm / palette_path).read_bytes()
        source_address = addresses[symbol]
        relative_base_address = addresses[relative_base]
        tile_address = addresses[tile_symbol]
        if len(palette) != 6 or relative_base_address != source_address + 6:
            raise ValueError(f"ground palette/header layout drift: {symbol}")
        relative_offset = tile_address - relative_base_address
        if not 0 <= relative_offset <= 0xFFFF:
            raise ValueError(f"ground relative tileset offset drift: {symbol}")
        encoded = palette + relative_offset.to_bytes(2, "big")
        if rom[source_address : source_address + 8] != encoded:
            raise ValueError(f"ground header ROM parity drift: {symbol}")
        ground_headers.append(
            {
                "ground": int(symbol[-2:]),
                "symbol": symbol,
                "sourceAddress": source_address,
                "palettePath": palette_path,
                "paletteBytes": 6,
                "paletteSha256": hashlib.sha256(palette).hexdigest().upper(),
                "relativeBaseSymbol": relative_base,
                "relativeTilesetOffset": relative_offset,
                "tilesetSymbol": tile_symbol,
                "tilesetAddress": tile_address,
            }
        )
    owner_by_symbol = {row["symbol"]: row["ground"] for row in ground_headers}
    aliases = [
        {"ground": index, "payloadOwnerGround": owner_by_symbol[symbol]}
        for index, symbol in enumerate(ground_references)
        if index != owner_by_symbol[symbol]
    ]
    expected_aliases = [
        {"ground": 21, "payloadOwnerGround": 12},
        {"ground": 22, "payloadOwnerGround": 12},
        {"ground": 29, "payloadOwnerGround": 13},
    ]
    if aliases != expected_aliases:
        raise ValueError(f"ground alias drift: {aliases}")

    ground_tiles = []
    for symbol, relative_path in tile_definitions:
        data = (disasm / relative_path).read_bytes()
        source_address = addresses[symbol]
        if rom[source_address : source_address + len(data)] != data:
            raise ValueError(f"ground tileset ROM parity drift: {symbol}")
        ground_tiles.append(
            {
                "symbol": symbol,
                "sourcePath": relative_path,
                "sourceAddress": source_address,
                **_stream_record(data, expected_bytes=1536),
            }
        )

    loader = read_upstream_text(disasm / LOADER_PATH)
    for fragment in (
        "LoadWeaponPalette:",
        "LoadWeaponsprite:",
        "jsr     (LoadStackCompressedData).w",
        "LoadBattlesceneGroundToVram:",
        "move.w  #$300,d0",
        "jmp     (ApplyImmediateVramDmaOnCompressedTiles).w",
    ):
        if fragment not in loader:
            raise ValueError(f"weapon/ground loader source-shape drift: {fragment!r}")

    all_streams = weapon_sprites + ground_tiles
    stream_summary = _stream_summary(all_streams)
    summary = {
        "pointerCount": len(weapon_references) + len(ground_references),
        "graphicPayloadCount": len(all_streams),
        "paletteCount": len(weapon_palettes) + len(ground_headers),
        "paletteByteCount": 42 * 4 + 27 * 6,
        "groundHeaderByteCount": len(ground_headers) * 8,
        "sourcePayloadByteCount": sum(row["compressedBytes"] for row in all_streams)
        + 42 * 4
        + len(ground_headers) * 8,
        **stream_summary,
        "pointerTableRomParityCount": len(weapon_references) + len(ground_references),
        "sourcePayloadRomParityCount": len(weapon_sprites)
        + len(weapon_palettes)
        + len(ground_headers)
        + len(ground_tiles),
    }
    side_summaries = [
        {
            "kind": "weapon",
            "pointerCount": 23,
            "graphicPayloadCount": 23,
            "paletteCount": 42,
            "paletteByteCount": 168,
            "decodedBytesPerPayload": 8192,
            **_stream_summary(weapon_sprites),
        },
        {
            "kind": "ground",
            "pointerCount": 30,
            "headerCount": 27,
            "aliasPointerCount": 3,
            "graphicPayloadCount": 10,
            "paletteCount": 27,
            "paletteByteCount": 162,
            "decodedBytesPerPayload": 1536,
            **_stream_summary(ground_tiles),
        },
    ]
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "romSha256": rom_hash,
        "function": {
            "loadStackAddress": addresses["LoadStackCompressedData"],
            "loadWeaponPaletteAddress": addresses["LoadWeaponPalette"],
            "loadWeaponspriteAddress": addresses["LoadWeaponsprite"],
            "loadGroundAddress": addresses["LoadBattlesceneGroundToVram"],
        },
        "table": {
            "groundTableAddress": ground_table_address,
            "weaponSpriteTableAddress": weapon_table_address,
            "weaponPaletteAddress": addresses["WeaponPalette00"],
        },
        "summary": summary,
        "sideSummaries": side_summaries,
        "groundAliases": aliases,
        "weaponSprites": weapon_sprites,
        "weaponPalettes": weapon_palettes,
        "groundHeaders": ground_headers,
        "groundTiles": ground_tiles,
        "runtimeQuestions": [],
    }


def verify_battle_weapon_ground_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_battle_weapon_ground_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="battle weapon/ground decode contract")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != output["romSha256"]
    ):
        raise ValueError("battle weapon/ground provenance drift")
    for field in (
        "function",
        "table",
        "summary",
        "sideSummaries",
        "groundAliases",
        "runtimeQuestions",
    ):
        if fixture[field] != output[field]:
            raise ValueError(f"battle weapon/ground {field} drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"] or output["summary"] != manifest["summary"]:
        raise ValueError("battle weapon/ground canonical output drift")
    destination = output_path or repo_path("local/derived/battle-weapon-ground-decode.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "GraphicPayloads": output["summary"]["graphicPayloadCount"],
        "Palettes": output["summary"]["paletteCount"],
        "DecodedBytes": output["summary"]["decodedByteCount"],
        "Status": "PASS",
    }
