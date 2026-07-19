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

ID = "sf2-variable-width-font-static-v1"
MANIFEST = repo_path("manifests/extractions/variable-width-font-static.json")
SCHEMA = repo_path("schemas/variable-width-font-static.schema.json")
FIXTURE = repo_path("tests/fixtures/h2/variable-width-font-static-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-variable-width-font-static-fixture.schema.json")
ROM_MANIFEST = repo_path("manifests/roms/sf2-us.json")

FONT_SYMBOL = "font_VariableWidth"
FONT_PATH = "data/graphics/tech/fonts/variablewidthfont.bin"
FONT_NOTE_PATH = "data/graphics/tech/fonts/variablewidthfont.txt"
FONT_OWNER_PATH = "code/common/tech/incbins/s06_incbins_graphics.asm"
FONT_POINTER_SYMBOL = "p_font_VariableWidth"
FONT_POINTER_PATH = "code/common/tech/pointers/s06_pointers.asm"
ASCII_TABLE_SYMBOL = "table_AsciiToTextSymbolMap"
ASCII_TABLE_PATH = "data/scripting/text/asciitotextsymbolmap.asm"
TEXT_FUNCTIONS_1_PATH = "code/common/scripting/text/textfunctions_1.asm"
TEXT_FUNCTIONS_2_PATH = "code/common/scripting/text/textfunctions_2.asm"

ADDRESS_SYMBOLS = (
    ASCII_TABLE_SYMBOL,
    "GetNextTextSymbol",
    "SymbolsToGraphics",
    "LoadVariableWidthFont",
    FONT_POINTER_SYMBOL,
    FONT_SYMBOL,
)


def _canonical_bytes(value: dict[str, Any]) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode()


def _parse_ascii_map(source: str) -> list[int]:
    values = [
        int(token)
        for token in re.findall(r"^\s*dc\.b\s+(\d+)\b", source, re.MULTILINE)
    ]
    if len(values) != 256:
        raise ValueError(f"ASCII-to-font map drift: expected 256 entries, got {len(values)}")
    if any(not 1 <= value <= 80 for value in values):
        raise ValueError("ASCII-to-font map contains an out-of-range glyph ID")
    return values


def _glyph_metadata(font: bytes, base_address: int) -> list[dict[str, Any]]:
    if len(font) != 80 * 32:
        raise ValueError(f"variable-width font size drift: expected 2560, got {len(font)}")
    rows = []
    for index in range(80):
        glyph = font[index * 32 : (index + 1) * 32]
        if glyph[0] != 0 or glyph[1] & 0xF0:
            raise ValueError(f"glyph {index + 1} width header padding drift")
        stored_width = glyph[1] & 0x0F
        advance = stored_width + 1 if stored_width else 0
        set_pixels: list[tuple[int, int]] = []
        rows_with_pixels = 0
        for row_index in range(15):
            left = glyph[2 + row_index * 2]
            right = glyph[3 + row_index * 2]
            if right & 0x0F:
                raise ValueError(f"glyph {index + 1} row padding drift")
            bits = (left << 4) | (right >> 4)
            row_pixels = [
                column for column in range(12) if bits & (1 << (11 - column))
            ]
            rows_with_pixels += bool(row_pixels)
            set_pixels.extend((row_index, column) for column in row_pixels)
        rows.append(
            {
                "symbolId": index + 1,
                "address": base_address + index * 32,
                "storedWidth": stored_width,
                "advancePixels": advance,
                "setPixelCount": len(set_pixels),
                "rowsWithPixels": rows_with_pixels,
                "hasAdvanceOverhang": any(column >= advance for _, column in set_pixels),
                "sha256": hashlib.sha256(glyph).hexdigest().upper(),
            }
        )
    return rows


def _assert_source_shape(disasm: Path) -> None:
    owner = read_upstream_text(disasm / FONT_OWNER_PATH)
    if re.search(
        rf'^{FONT_SYMBOL}:\s*\n\s*incbin\s+"{re.escape(FONT_PATH)}"',
        owner,
        re.MULTILINE,
    ) is None:
        raise ValueError("variable-width font incbin ownership drift")

    pointer = read_upstream_text(disasm / FONT_POINTER_PATH)
    if re.search(
        rf"^{FONT_POINTER_SYMBOL}:\s*\n\s*dc\.l\s+{FONT_SYMBOL}\b",
        pointer,
        re.MULTILINE,
    ) is None:
        raise ValueError("variable-width font pointer ownership drift")

    text_1 = read_upstream_text(disasm / TEXT_FUNCTIONS_1_PATH)
    for fragment in (
        "GetNextTextSymbol:",
        "tst.l   ((CURRENT_DIALOGUE_ASCII_BYTE_ADDRESS-$1000000)).w",
        f"lea     {ASCII_TABLE_SYMBOL}(pc), a1",
        "move.b  (a1,d0.w),d0",
    ):
        if fragment not in text_1:
            raise ValueError(f"ASCII-to-font consumer drift: missing {fragment!r}")

    text_2 = read_upstream_text(disasm / TEXT_FUNCTIONS_2_PATH)
    for fragment in (
        "SymbolsToGraphics:",
        "LoadVariableWidthFont:",
        "subq.w  #1,d7",
        "lsl.w   #5,d7",
        f"movea.l ({FONT_POINTER_SYMBOL}).l,a0",
        "move.w  (a0)+,d4",
        "andi.w  #BYTE_LOWER_NIBBLE_MASK,d4",
        "beq.s   loc_6BBC",
        "addq.w  #1,d4",
    ):
        if fragment not in text_2:
            raise ValueError(f"variable-width font consumer drift: missing {fragment!r}")

    symbols_block = text_2.split("SymbolsToGraphics:", 1)[1].split(
        "; End of function SymbolsToGraphics", 1
    )[0]
    if symbols_block.count("LoadVariableWidthFont") != 3:
        raise ValueError("SymbolsToGraphics font-call topology drift")


def build_variable_width_font_contract(
    rom_path: Path, upstream_path: Path
) -> dict[str, Any]:
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, toolchain = _resolve_upstream(upstream_path)
    listing_path = upstream_path / "build/sf2build-h1.lst"
    if not listing_path.is_file():
        raise ValueError(f"variable-width font H1 listing is missing: {listing_path}")
    addresses = listing_symbol_addresses(listing_path.read_text(encoding="utf-8"))
    table = {symbol: addresses[symbol] for symbol in ADDRESS_SYMBOLS}

    rom = rom_path.read_bytes()
    rom_hash = hashlib.sha256(rom).hexdigest().upper()
    if rom_hash != load_json(ROM_MANIFEST)["hashes"]["sha256"]:
        raise ValueError("variable-width font input ROM identity drift")

    _assert_source_shape(disasm)
    font = (disasm / FONT_PATH).read_bytes()
    font_address = table[FONT_SYMBOL]
    if rom[font_address : font_address + len(font)] != font:
        raise ValueError("variable-width font source/ROM parity drift")

    pointer_address = table[FONT_POINTER_SYMBOL]
    pointer_bytes = font_address.to_bytes(4, "big")
    if rom[pointer_address : pointer_address + 4] != pointer_bytes:
        raise ValueError("variable-width font pointer ROM parity drift")

    ascii_source = read_upstream_text(disasm / ASCII_TABLE_PATH)
    ascii_values = _parse_ascii_map(ascii_source)
    ascii_bytes = bytes(ascii_values)
    ascii_address = table[ASCII_TABLE_SYMBOL]
    if rom[ascii_address : ascii_address + len(ascii_bytes)] != ascii_bytes:
        raise ValueError("ASCII-to-font map source/ROM parity drift")

    glyphs = _glyph_metadata(font, font_address)
    widths = Counter(row["storedWidth"] for row in glyphs)
    mapped_ids = set(ascii_values)
    summary = {
        "fontByteCount": len(font),
        "glyphCount": len(glyphs),
        "glyphRecordByteCount": 32,
        "glyphRowCount": 15,
        "glyphPixelColumnCount": 12,
        "blankGlyphCount": sum(row["setPixelCount"] == 0 for row in glyphs),
        "setPixelCount": sum(row["setPixelCount"] for row in glyphs),
        "rowsWithPixelsCount": sum(row["rowsWithPixels"] for row in glyphs),
        "minimumStoredWidth": min(widths),
        "maximumStoredWidth": max(widths),
        "distinctStoredWidthCount": len(widths),
        "advanceOverhangGlyphCount": sum(row["hasAdvanceOverhang"] for row in glyphs),
        "headerPaddingViolationCount": 0,
        "rowPaddingViolationCount": 0,
        "pointerByteCount": len(pointer_bytes),
        "asciiMapEntryCount": len(ascii_values),
        "asciiMapUniqueGlyphCount": len(mapped_ids),
        "asciiMapDefaultGlyphCount": ascii_values.count(1),
        "asciiMapMissingGlyphCount": 80 - len(mapped_ids),
        "printableAsciiNonDefaultCount": sum(
            ascii_values[index] != 1 for index in range(32, 127)
        ),
        "extendedAsciiNonDefaultCount": sum(
            ascii_values[index] != 1 for index in range(127, 256)
        ),
        "sourceRomParityByteCount": len(font) + len(pointer_bytes) + len(ascii_bytes),
    }
    return {
        "schemaVersion": 1,
        "id": ID,
        "upstream": {
            "repository": toolchain["sf2disasm"]["repository"],
            "commit": commit,
        },
        "romSha256": rom_hash,
        "table": table,
        "summary": summary,
        "font": {
            "symbol": FONT_SYMBOL,
            "sourcePath": FONT_PATH,
            "ownerPath": FONT_OWNER_PATH,
            "formatNotePath": FONT_NOTE_PATH,
            "address": font_address,
            "byteCount": len(font),
            "sha256": hashlib.sha256(font).hexdigest().upper(),
            "formatNoteSha256": hashlib.sha256(
                (disasm / FONT_NOTE_PATH).read_bytes()
            ).hexdigest().upper(),
        },
        "pointer": {
            "symbol": FONT_POINTER_SYMBOL,
            "sourcePath": FONT_POINTER_PATH,
            "address": pointer_address,
            "targetSymbol": FONT_SYMBOL,
            "targetAddress": font_address,
            "byteCount": len(pointer_bytes),
            "sha256": hashlib.sha256(pointer_bytes).hexdigest().upper(),
        },
        "asciiMap": {
            "symbol": ASCII_TABLE_SYMBOL,
            "sourcePath": ASCII_TABLE_PATH,
            "address": ascii_address,
            "entryCount": len(ascii_values),
            "uniqueGlyphCount": len(mapped_ids),
            "defaultGlyphId": 1,
            "defaultGlyphCount": ascii_values.count(1),
            "missingGlyphIds": sorted(set(range(1, 81)) - mapped_ids),
            "sha256": hashlib.sha256(ascii_bytes).hexdigest().upper(),
        },
        "storedWidthHistogram": {
            str(width): count for width, count in sorted(widths.items())
        },
        "glyphs": glyphs,
        "consumerFacts": {
            "asciiPathFunction": "GetNextTextSymbol",
            "asciiPathUsesMapOnlyWhenRamAsciiPointerIsNonzero": True,
            "huffmanPathBypassesAsciiMap": True,
            "renderFunction": "SymbolsToGraphics",
            "glyphLoaderFunction": "LoadVariableWidthFont",
            "glyphAddressFormula": "font_VariableWidth + (symbolId - 1) * 32",
            "storedWidthZeroAdvance": 0,
            "storedWidthNonzeroAdvanceAdjustment": 1,
            "regularDialogueFontLoadCount": 1,
            "nonRegularDialogueFontLoadCount": 2,
        },
        "runtimeQuestions": [
            "Do direct Huffman symbols make glyph IDs 70 and 71 reachable even though "
            "the ASCII conversion table never emits them?",
            "Do glyph overlap, palette choice, typewriter timing, and DMA reproduce the "
            "original rendered dialogue for every mapped symbol?",
        ],
    }


def verify_variable_width_font_contract(
    rom_path: Path, upstream_path: Path, *, output_path: Path | None = None
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    manifest = load_json(MANIFEST)
    output = build_variable_width_font_contract(rom_path, upstream_path)
    validate_json(output, SCHEMA, owner="variable-width font static contract")
    if (
        fixture["upstreamCommit"] != output["upstream"]["commit"]
        or fixture["romSha256"] != output["romSha256"]
    ):
        raise ValueError("variable-width font provenance drift")
    for field in (
        "table",
        "summary",
        "storedWidthHistogram",
        "fontHashes",
        "asciiMapFacts",
        "consumerFacts",
        "runtimeQuestions",
    ):
        actual: Any
        if field == "fontHashes":
            actual = {
                "fontSha256": output["font"]["sha256"],
                "formatNoteSha256": output["font"]["formatNoteSha256"],
                "pointerSha256": output["pointer"]["sha256"],
                "asciiMapSha256": output["asciiMap"]["sha256"],
            }
        elif field == "asciiMapFacts":
            actual = {
                key: output["asciiMap"][key]
                for key in (
                    "entryCount",
                    "uniqueGlyphCount",
                    "defaultGlyphId",
                    "defaultGlyphCount",
                    "missingGlyphIds",
                )
            }
        else:
            actual = output[field]
        if fixture[field] != actual:
            raise ValueError(f"variable-width font fixture drift: {field}")
    digest = hashlib.sha256(_canonical_bytes(output)).hexdigest().upper()
    if output["summary"] != manifest["summary"] or digest != manifest["outputSha256"]:
        raise ValueError("variable-width font canonical manifest drift")
    destination = output_path or repo_path("local/derived/variable-width-font-static.json")
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_bytes(_canonical_bytes(output))
    return {
        "Contract": ID,
        "Output": display_path(destination),
        "SHA256": digest,
        "Glyphs": output["summary"]["glyphCount"],
        "AsciiMapEntries": output["summary"]["asciiMapEntryCount"],
        "ParityBytes": output["summary"]["sourceRomParityByteCount"],
        "Status": "PASS",
    }
