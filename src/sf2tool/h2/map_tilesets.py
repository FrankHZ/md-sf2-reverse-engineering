from __future__ import annotations

import hashlib
import json
import re
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.compression import decode_stack_compressed
from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.source_text import read_upstream_text

ID = "sf2-map-tileset-decode-v1"
MANIFEST = repo_path("manifests/extractions/map-tileset-decode.json")
SCHEMA = repo_path("schemas/map-tileset-decode.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/map-tileset-decode-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-map-tileset-decode-fixture.schema.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")

TILESET_COUNT = 115
MAP_COUNT = 79
TILESET_SOURCE = Path("data/graphics/maps/maptilesets/entries.asm")
MAP_ENTRY_ROOT = Path("data/maps/entries")
MAPLOAD_SOURCE = Path("code/common/maps/mapload.asm")


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _parse_map_usage(
    disasm: Path, addresses: dict[str, int], rom: bytes
) -> tuple[list[dict[str, Any]], list[dict[str, Any]]]:
    maps = []
    animations = []
    for map_index in range(MAP_COUNT):
        map_name = f"Map{map_index:02}"
        tileset_path = MAP_ENTRY_ROOT / f"map{map_index:02}/00-tilesets.asm"
        source = read_upstream_text(disasm / tileset_path)
        palette_match = re.search(r"mapPalette\s+(\d+)", source)
        slots = [
            int(value)
            for value in re.findall(r"mapTileset[1-5]\s+(\d+)", source)
        ]
        if palette_match is None or len(slots) != 5:
            raise ValueError(f"map tileset header drift: {tileset_path}")
        palette = int(palette_match.group(1))
        if any(value != 255 and not 0 <= value < TILESET_COUNT for value in slots):
            raise ValueError(f"map tileset index out of range: {tileset_path}")
        map_address = addresses[map_name]
        expected = bytes([palette, *slots])
        if rom[map_address : map_address + 6] != expected:
            raise ValueError(f"map tileset header ROM parity drift: {map_name}")
        maps.append(
            {
                "mapIndex": map_index,
                "sourcePath": tileset_path.as_posix(),
                "mapAddress": map_address,
                "paletteIndex": palette,
                "tilesetSlots": slots,
            }
        )

        animation_path = MAP_ENTRY_ROOT / f"map{map_index:02}/9-animations.asm"
        if not (disasm / animation_path).is_file():
            continue
        animation_source = read_upstream_text(disasm / animation_path)
        match = re.search(r"mapAnimation\s+(\d+)\s*,\s*(\d+)", animation_source)
        if match is None:
            raise ValueError(f"map animation header drift: {animation_path}")
        tileset_index, tile_count = map(int, match.groups())
        if not 0 <= tileset_index < TILESET_COUNT:
            raise ValueError(f"map animation tileset index out of range: {animation_path}")
        symbol = f"Map{map_index:02}s9_Animations"
        address = addresses[symbol]
        expected_header = tileset_index.to_bytes(2, "big") + tile_count.to_bytes(2, "big")
        if rom[address : address + 4] != expected_header:
            raise ValueError(f"map animation header ROM parity drift: {symbol}")
        animations.append(
            {
                "mapIndex": map_index,
                "sourcePath": animation_path.as_posix(),
                "address": address,
                "tilesetIndex": tileset_index,
                "tileCount": tile_count,
            }
        )
    return maps, animations


def build_map_tileset_contract(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"map-tileset H1 listing is missing: {listing_path}")
    addresses = listing_symbol_addresses(listing_path.read_text(encoding="utf-8"))
    rom = rom_path.read_bytes()
    rom_hash = hashlib.sha256(rom).hexdigest().upper()
    if rom_hash != load_json(ROM_MANIFEST)["hashes"]["sha256"]:
        raise ValueError("map-tileset input ROM identity drift")

    mapload = read_upstream_text(disasm / MAPLOAD_SOURCE)
    if mapload.count("bsr.w   LoadStackCompressedData") != 6:
        raise ValueError("map-tileset Stack consumer count drift")
    for fragment in (
        "LoadMapTilesets:",
        "movea.l (p_pt_MapTilesets).l,a0",
        "LoadMapArea:",
        "move.l  $18(a5),((TILE_ANIMATION_DATA_ADDRESS-$1000000)).w",
    ):
        if fragment not in mapload:
            raise ValueError(f"map-tileset consumer drift: missing {fragment!r}")

    source = read_upstream_text(disasm / TILESET_SOURCE)
    symbols = [f"MapTileset{index:03}" for index in range(TILESET_COUNT)]
    table_symbols = re.findall(r"^\s*dc\.l\s+(MapTileset\d{3})", source, re.MULTILINE)
    first = re.search(r"^pt_MapTilesets:\s*dc\.l\s+(MapTileset\d{3})", source, re.MULTILINE)
    if first is None or [first.group(1), *table_symbols] != symbols:
        raise ValueError("map-tileset pointer table source drift")

    rows = []
    for index, symbol in enumerate(symbols):
        path = f"data/graphics/maps/maptilesets/maptileset{index:03}.bin"
        if not re.search(rf'{symbol}:\s*incbin\s+"{re.escape(path)}"', source):
            raise ValueError(f"map-tileset resource definition drift: {symbol}")
        data = (disasm / path).read_bytes()
        address = addresses[symbol]
        if rom[address : address + len(data)] != data:
            raise ValueError(f"map-tileset payload ROM parity drift: {symbol}")
        decoded = decode_stack_compressed(data)
        if len(decoded.output) != 4096:
            raise ValueError(f"map-tileset output-size drift: {symbol}")
        rows.append(
            {
                "index": index,
                "symbol": symbol,
                "sourcePath": path,
                "sourceAddress": address,
                "compressedBytes": len(data),
                "decodedBytes": len(decoded.output),
                "sourceSha256": hashlib.sha256(data).hexdigest().upper(),
                "decodedSha256": hashlib.sha256(decoded.output).hexdigest().upper(),
                "inputBitsConsumed": decoded.input_bits_consumed,
                "trailingBits": len(data) * 8 - decoded.input_bits_consumed,
                "commandGroupCount": decoded.command_group_count,
                "literalWordCount": decoded.literal_word_count,
                "copyCommandCount": decoded.copy_command_count,
                "copiedWordCount": decoded.copied_word_count,
                "maximumCopyOffsetWords": decoded.maximum_copy_offset_words,
                "maximumCopyLengthWords": decoded.maximum_copy_length_words,
            }
        )

    table_address = addresses["pt_MapTilesets"]
    table_bytes = b"".join(addresses[symbol].to_bytes(4, "big") for symbol in symbols)
    if rom[table_address : table_address + len(table_bytes)] != table_bytes:
        raise ValueError("map-tileset pointer table ROM parity drift")
    pointer_address = addresses["p_pt_MapTilesets"]
    if rom[pointer_address : pointer_address + 4] != table_address.to_bytes(4, "big"):
        raise ValueError("map-tileset top-level pointer ROM parity drift")

    maps, animations = _parse_map_usage(disasm, addresses, rom)
    normal_references = [
        value for row in maps for value in row["tilesetSlots"] if value != 255
    ]
    animation_references = [row["tilesetIndex"] for row in animations]
    used = set(normal_references) | set(animation_references)
    unused = sorted(set(range(TILESET_COUNT)) - used)
    tile_counts = Counter(row["tileCount"] for row in animations)
    stream_totals = {
        "commandGroupCount": sum(row["commandGroupCount"] for row in rows),
        "literalWordCount": sum(row["literalWordCount"] for row in rows),
        "copyCommandCount": sum(row["copyCommandCount"] for row in rows),
        "copiedWordCount": sum(row["copiedWordCount"] for row in rows),
        "minimumTrailingBits": min(row["trailingBits"] for row in rows),
        "maximumTrailingBits": max(row["trailingBits"] for row in rows),
        "maximumCopyOffsetWords": max(row["maximumCopyOffsetWords"] for row in rows),
        "maximumCopyLengthWords": max(row["maximumCopyLengthWords"] for row in rows),
    }
    summary = {
        "tilesetCount": len(rows),
        "fixedDecodedBytesPerTileset": 4096,
        "compressedByteCount": sum(row["compressedBytes"] for row in rows),
        "decodedByteCount": sum(row["decodedBytes"] for row in rows),
        "tableRomParityCount": len(rows),
        "payloadRomParityCount": len(rows),
        "mapCount": len(maps),
        "mapSlotCount": len(maps) * 5,
        "mapTilesetReferenceCount": len(normal_references),
        "absentMapSlotCount": len(maps) * 5 - len(normal_references),
        "uniqueMapTilesetReferenceCount": len(set(normal_references)),
        "animationMapCount": len(animations),
        "animationTilesetReferenceCount": len(animation_references),
        "uniqueAnimationTilesetReferenceCount": len(set(animation_references)),
        "combinedUsedTilesetCount": len(used),
        "unusedTilesetCount": len(unused),
        **stream_totals,
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "romSha256": rom_hash,
        "function": {
            "loadStackAddress": addresses["LoadStackCompressedData"],
            "LoadMapTilesets": addresses["LoadMapTilesets"],
            "LoadMapArea": addresses["LoadMapArea"],
        },
        "table": {
            "pt_MapTilesets": table_address,
            "p_pt_MapTilesets": pointer_address,
        },
        "summary": summary,
        "unusedTilesetIndices": unused,
        "animationTileCountDistribution": {
            str(key): value for key, value in sorted(tile_counts.items())
        },
        "tilesets": rows,
        "maps": maps,
        "animations": animations,
        "runtimeQuestions": [
            "Do all map-tileset slots and animated replacement ranges render with original "
            "palette selection, VRAM placement, and frame composition?",
            "Is MapTileset029 unreachable through dynamic or encoded writes as indicated by the "
            "complete static map-header and animation-header scan?",
        ],
    }


def verify_map_tileset_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_map_tileset_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="map-tileset decode contract")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != output["romSha256"]
    ):
        raise ValueError("map-tileset provenance drift")
    for field in (
        "function",
        "table",
        "summary",
        "unusedTilesetIndices",
        "animationTileCountDistribution",
        "runtimeQuestions",
    ):
        if fixture[field] != output[field]:
            raise ValueError(f"map-tileset {field} drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"] or output["summary"] != manifest["summary"]:
        raise ValueError("map-tileset canonical output drift")
    destination = output_path or repo_path("local/derived/map-tileset-decode.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Tilesets": output["summary"]["tilesetCount"],
        "DecodedBytes": output["summary"]["decodedByteCount"],
        "UsedTilesets": output["summary"]["combinedUsedTilesetCount"],
        "Status": "PASS",
    }
