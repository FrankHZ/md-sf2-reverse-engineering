from __future__ import annotations

import hashlib
import json
import re
from collections import Counter
from pathlib import Path
from typing import Any

from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import display_path, repo_path
from sf2tool.research_index import listing_symbol_addresses
from sf2tool.source_text import read_upstream_text

ID = "sf2-map-palette-static-v1"
MANIFEST = repo_path("manifests/extractions/map-palette-static.json")
SCHEMA = repo_path("schemas/map-palette-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/map-palette-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-map-palette-static-fixture.schema.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")

PALETTE_COUNT = 16
MAP_COUNT = 79
PALETTE_BYTES = 32
COLOR_MASK = 0x0EEE
PALETTE_SOURCE = Path("data/graphics/maps/mappalettes/entries.asm")
MAP_ENTRY_ROOT = Path("data/maps/entries")
MAPLOAD_SOURCE = Path("code/common/maps/mapload.asm")


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def build_map_palette_contract(rom_path: Path, upstream_path: Path) -> dict[str, Any]:
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"map-palette H1 listing is missing: {listing_path}")
    addresses = listing_symbol_addresses(listing_path.read_text(encoding="utf-8"))
    rom = rom_path.read_bytes()
    rom_hash = hashlib.sha256(rom).hexdigest().upper()
    if rom_hash != load_json(ROM_MANIFEST)["hashes"]["sha256"]:
        raise ValueError("map-palette input ROM identity drift")

    mapload = read_upstream_text(disasm / MAPLOAD_SOURCE)
    for fragment in (
        "movea.l (p_pt_MapPalettes).l,a0",
        "lea     (PALETTE_1_BASE).l,a1",
        "move.w  #CRAM_PALETTE_SIZE,d7",
        "bsr.w   CopyBytes",
        "clr.w   (PALETTE_1_BASE).l",
    ):
        if fragment not in mapload:
            raise ValueError(f"map-palette consumer drift: missing {fragment!r}")
    enums = read_upstream_text(disasm / "sf2enums.asm")
    if "CRAM_PALETTE_SIZE: equ 32" not in enums:
        raise ValueError("map-palette byte-count constant drift")

    source = read_upstream_text(disasm / PALETTE_SOURCE)
    symbols = [f"MapPalette{index:02}" for index in range(PALETTE_COUNT)]
    table_symbols = re.findall(r"^\s*dc\.l\s+(MapPalette\d{2})", source, re.MULTILINE)
    first = re.search(r"^pt_MapPalettes:\s*dc\.l\s+(MapPalette\d{2})", source, re.MULTILINE)
    if first is None or [first.group(1), *table_symbols] != symbols:
        raise ValueError("map-palette pointer table source drift")

    rows = []
    all_words = []
    for index, symbol in enumerate(symbols):
        path = f"data/graphics/maps/mappalettes/mappalette{index:02}.bin"
        if not re.search(rf'{symbol}:\s*incbin\s+"{re.escape(path)}"', source):
            raise ValueError(f"map-palette resource definition drift: {symbol}")
        data = (disasm / path).read_bytes()
        if len(data) != PALETTE_BYTES:
            raise ValueError(f"map-palette size drift: {symbol}")
        address = addresses[symbol]
        if rom[address : address + len(data)] != data:
            raise ValueError(f"map-palette payload ROM parity drift: {symbol}")
        words = [int.from_bytes(data[offset : offset + 2], "big") for offset in range(0, 32, 2)]
        if any(word & ~COLOR_MASK for word in words):
            raise ValueError(f"map-palette color mask drift: {symbol}")
        all_words.extend(words)
        rows.append(
            {
                "index": index,
                "symbol": symbol,
                "sourcePath": path,
                "sourceAddress": address,
                "byteCount": len(data),
                "colorCount": len(words),
                "sourceFirstColor": words[0],
                "effectiveFirstColor": 0,
                "sourceSha256": hashlib.sha256(data).hexdigest().upper(),
                "effectiveSha256": hashlib.sha256(b"\0\0" + data[2:]).hexdigest().upper(),
            }
        )

    table_address = addresses["pt_MapPalettes"]
    table_bytes = b"".join(addresses[symbol].to_bytes(4, "big") for symbol in symbols)
    if rom[table_address : table_address + len(table_bytes)] != table_bytes:
        raise ValueError("map-palette pointer table ROM parity drift")
    pointer_address = addresses["p_pt_MapPalettes"]
    if rom[pointer_address : pointer_address + 4] != table_address.to_bytes(4, "big"):
        raise ValueError("map-palette top-level pointer ROM parity drift")

    map_rows = []
    references = []
    for map_index in range(MAP_COUNT):
        map_name = f"Map{map_index:02}"
        path = MAP_ENTRY_ROOT / f"map{map_index:02}/00-tilesets.asm"
        map_source = read_upstream_text(disasm / path)
        match = re.search(r"mapPalette\s+(\d+)", map_source)
        if match is None:
            raise ValueError(f"map palette header drift: {path}")
        palette_index = int(match.group(1))
        if not 0 <= palette_index < PALETTE_COUNT:
            raise ValueError(f"map palette index out of range: {path}")
        map_address = addresses[map_name]
        if rom[map_address] != palette_index:
            raise ValueError(f"map palette header ROM parity drift: {map_name}")
        references.append(palette_index)
        map_rows.append(
            {
                "mapIndex": map_index,
                "sourcePath": path.as_posix(),
                "mapAddress": map_address,
                "paletteIndex": palette_index,
            }
        )

    usage = Counter(references)
    summary = {
        "paletteCount": len(rows),
        "paletteByteCount": sum(row["byteCount"] for row in rows),
        "colorsPerPalette": 16,
        "sourceColorWordCount": len(all_words),
        "uniqueSourceColorCount": len(set(all_words)),
        "validColorMaskCount": len(all_words),
        "nonzeroSourceFirstColorCount": sum(row["sourceFirstColor"] != 0 for row in rows),
        "clearedEffectiveFirstColorCount": len(rows),
        "pointerTableRomParityCount": len(rows),
        "payloadRomParityCount": len(rows),
        "mapCount": len(map_rows),
        "mapHeaderRomParityCount": len(map_rows),
        "mapPaletteReferenceCount": len(references),
        "usedPaletteCount": len(usage),
        "unusedPaletteCount": PALETTE_COUNT - len(usage),
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {"repository": toolchain["sf2disasm"]["repository"], "commit": commit},
        "romSha256": rom_hash,
        "function": {"LoadMap": addresses["LoadMap"], "CopyBytes": addresses["CopyBytes"]},
        "table": {"pt_MapPalettes": table_address, "p_pt_MapPalettes": pointer_address},
        "summary": summary,
        "usageCounts": {str(index): usage[index] for index in range(PALETTE_COUNT)},
        "palettes": rows,
        "maps": map_rows,
        "runtimeQuestions": [
            "Do all sixteen effective map palettes render with original fade, transition, and "
            "per-map presentation behavior after color zero is cleared?"
        ],
    }


def verify_map_palette_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_map_palette_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="map-palette contract")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != output["romSha256"]
    ):
        raise ValueError("map-palette provenance drift")
    for field in ("function", "table", "summary", "usageCounts", "runtimeQuestions"):
        if fixture[field] != output[field]:
            raise ValueError(f"map-palette {field} drift")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if digest != manifest["outputSha256"] or output["summary"] != manifest["summary"]:
        raise ValueError("map-palette canonical output drift")
    destination = output_path or repo_path("local/derived/map-palette-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Palettes": output["summary"]["paletteCount"],
        "Maps": output["summary"]["mapCount"],
        "UsedPalettes": output["summary"]["usedPaletteCount"],
        "Status": "PASS",
    }
