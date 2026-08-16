"""Deterministic extraction of the original map textures (tilesets + palettes) into PNG.

Consumes the accepted static facts of the ``sf2-map-tileset`` and ``sf2-map-palette``
contracts: 115 Stack-compressed ``MapTilesetNNN`` streams that each decode to exactly
4096 bytes (128 Mega Drive 4bpp 8x8 tiles), and 16 ``MapPaletteNN`` records of 16
big-endian 9-bit color words (``0x0EEE`` mask, RGB 3 bits each, color 0 transparent).
The compressed streams are decoded with ``sf2tool.compression.decode_stack_compressed``
(the driver-mirroring decoder already proven by the accepted contracts).

Rendering conventions (documented engineering choices, not original evidence):

- tiles are laid out in a 16x8 grid (128 tiles) per tileset; every tileset sheet is
  rendered with map palette 0 by default so the sheets are directly viewable, while all
  16 palettes are also emitted as color-strip PNGs;
- color 0 is transparent (the effective first color is forced to zero by the original
  palette consumer); the other 15 colors map 3-bit channels to 8-bit with the standard
  ``v<<5 | v<<2 | v>>1`` expansion;
- the PNG writer is a minimal stdlib implementation (RGBA 8-bit, no interlace).

The generated PNG files are private/generated graphics payloads: they are written only
under the ignored local output directory and are never tracked. The tracked manifest
contains metadata and hashes only.
"""

from __future__ import annotations

import hashlib
import json
import re
import struct
import zlib
from dataclasses import dataclass
from pathlib import Path

from sf2tool.compression import decode_basic_compressed, decode_stack_compressed
from sf2tool.h2.battle_scene_engine import _resolve_upstream
from sf2tool.h2.map_layouts import decode_map_blocks, decode_map_layout
from sf2tool.h2.map_palettes import COLOR_MASK
from sf2tool.jsonio import load_json
from sf2tool.paths import repo_path
from sf2tool.source_text import read_upstream_text

TILESET_COUNT = 115
PALETTE_COUNT = 16
PALETTE_COLORS = 16
PALETTE_BYTES = 32
TILESET_DECODED_BYTES = 4096
TILE_SIZE = 8
TILE_BYTES_4BPP = 32
TILES_PER_TILESET = TILESET_DECODED_BYTES // TILE_BYTES_4BPP
BLOCK_TILES = 9
BLOCK_TILES_PER_SIDE = 3
BLOCK_PIXELS = BLOCK_TILES_PER_SIDE * TILE_SIZE
MAP_LAYOUT_SIDE = 64
MAP_SLOT_COUNT = 5
MAP_SLOT_TILE_COUNT = TILES_PER_TILESET

TILESET_SOURCE_ROOT = Path("data/graphics/maps/maptilesets")
PALETTE_SOURCE_ROOT = Path("data/graphics/maps/mappalettes")
MAP_ENTRY_ROOT = Path("data/maps/entries")

TILE_PRIORITY = 0x8000
TILE_FLIP = 0x1000
TILE_MIRROR = 0x800
TILE_INDEX_MASK = 0x3FF
TILE_INDEX_OFFSET = 0x100


@dataclass(frozen=True)
class TextureExtractionOptions:
    tileset_palette: int = 0


def _sha256(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest().upper()


def md_palette_color(word: int) -> tuple[int, int, int]:
    """Decode one 9-bit Mega Drive color word (``0x0EEE`` mask) to 8-bit RGB.

    The low bit group (bits 1-3) is red, the middle group (bits 5-7) is green, and
    the high group (bits 9-11) is blue.
    """
    if word & ~COLOR_MASK:
        raise ValueError(f"palette color word outside 0x0EEE mask: {word:04X}")

    def expand(value: int) -> int:
        return value << 5 | value << 2 | value >> 1

    red = expand((word & 0x0E) >> 1)
    green = expand((word & 0x0E0) >> 5)
    blue = expand((word & 0x0E00) >> 9)
    return red, green, blue


def md_cram_color(word: int) -> tuple[int, int, int]:
    """Decode one special-sprite palette word with the candidate 6/5/5 channel layout to 8-bit RGB.

    Six-bit blue in bits 0-5, five-bit green in bits 6-10, five-bit red in bits 11-15.
    This layout is an unconfirmed candidate: the words do not fit the ``0x0EEE`` form
    (e.g. ``0x558`` in `taros.bin`) and the layout is not yet verified against the game.
    """
    blue = (word & 0x3F) << 2 | (word & 0x3F) >> 4
    green5 = (word >> 6) & 0x1F
    red5 = (word >> 11) & 0x1F
    return (
        red5 << 3 | red5 >> 2,
        green5 << 3 | green5 >> 2,
        blue,
    )


def decode_md_4bpp_tile(tile: bytes) -> list[int]:
    """Decode one 32-byte Mega Drive 4bpp tile into 64 pixel indices (row-major).

    Each tile row is four bytes holding eight 4-bit pixels, two pixels per byte with
    the left pixel in the high nibble (most-significant bit first).
    """
    if len(tile) != TILE_BYTES_4BPP:
        raise ValueError(f"4bpp tile must be {TILE_BYTES_4BPP} bytes, got {len(tile)}")
    pixels: list[int] = []
    for row in range(TILE_SIZE):
        base = row * 4
        for column in range(TILE_SIZE):
            byte = tile[base + column // 2]
            index = (byte >> 4) & 0x0F if column % 2 == 0 else byte & 0x0F
            pixels.append(index)
    return pixels


def _png_chunk(tag: bytes, data: bytes) -> bytes:
    return (
        struct.pack(">I", len(data))
        + tag
        + data
        + struct.pack(">I", zlib.crc32(tag + data) & 0xFFFFFFFF)
    )


def write_png_rgba(path: Path, width: int, height: int, pixels: list[int]) -> None:
    """Write an RGBA 8-bit PNG with the minimal stdlib writer."""
    if len(pixels) != width * height * 4:
        raise ValueError(
            f"pixel buffer size drift: {len(pixels)} for {width}x{height} RGBA"
        )
    raw = b"".join(
        b"\x00" + bytes(pixels[row * width * 4 : (row + 1) * width * 4])
        for row in range(height)
    )
    header = struct.pack(">IIBBBBB", width, height, 8, 6, 0, 0, 0)
    png = (
        b"\x89PNG\r\n\x1a\n"
        + _png_chunk(b"IHDR", header)
        + _png_chunk(b"IDAT", zlib.compress(raw))
        + _png_chunk(b"IEND", b"")
    )
    path.write_bytes(png)


def render_tileset_sheet(
    tiles: list[list[int]], palette: list[tuple[int, int, int]]
) -> list[int]:
    """Render 128 decoded tiles into a 16x8 RGBA sheet with the given palette."""
    if len(tiles) != TILES_PER_TILESET:
        raise ValueError(f"tileset must have {TILES_PER_TILESET} tiles, got {len(tiles)}")
    if len(palette) != PALETTE_COLORS:
        raise ValueError(f"palette must have {PALETTE_COLORS} colors, got {len(palette)}")
    sheet_width = 16 * TILE_SIZE
    sheet_height = 8 * TILE_SIZE
    pixels = [0] * (sheet_width * sheet_height * 4)
    for tile_index, tile in enumerate(tiles):
        origin_x = (tile_index % 16) * TILE_SIZE
        origin_y = (tile_index // 16) * TILE_SIZE
        for row in range(TILE_SIZE):
            for column in range(TILE_SIZE):
                color = palette[tile[row * TILE_SIZE + column]]
                offset = (
                    (origin_y + row) * sheet_width + (origin_x + column)
                ) * 4
                if color == (0, 0, 0):
                    pixels[offset : offset + 4] = [0, 0, 0, 0]
                else:
                    pixels[offset : offset + 4] = [*color, 255]
    return pixels


def render_palette_strip(palette: list[tuple[int, int, int]]) -> list[int]:
    """Render 16 colors as a 16x16 RGBA strip (one 16x16 cell per color)."""
    pixels = [0] * (PALETTE_COLORS * 16 * 16 * 4)
    for color_index, color in enumerate(palette):
        for row in range(16):
            base = (color_index * 16 + row) * 16 * 4
            for column in range(16):
                offset = base + column * 4
                if color == (0, 0, 0):
                    pixels[offset : offset + 4] = [0, 0, 0, 0]
                else:
                    pixels[offset : offset + 4] = [*color, 255]
    return pixels


def extract_map_textures(
    rom_path: Path,
    upstream_path: Path,
    *,
    out_dir: Path,
    options: TextureExtractionOptions | None = None,
) -> dict[str, object]:
    """Extract the 115 map tilesets and 16 map palettes as private PNG sheets."""
    if options is None:
        options = TextureExtractionOptions()
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, _toolchain = _resolve_upstream(upstream_path)
    expected_rom = load_json(repo_path("manifests/roms/sf2-us.json"))
    rom = rom_path.read_bytes()
    if len(rom) != expected_rom["sizeBytes"] or _sha256(rom) != expected_rom["hashes"]["sha256"]:
        raise ValueError("map texture extraction ROM identity drift")

    palettes: list[list[tuple[int, int, int]]] = []
    for index in range(PALETTE_COUNT):
        payload = (disasm / PALETTE_SOURCE_ROOT / f"mappalette{index:02}.bin").read_bytes()
        if len(payload) != PALETTE_BYTES:
            raise ValueError(f"map palette size drift: {index}")
        words = [
            int.from_bytes(payload[offset : offset + 2], "big")
            for offset in range(0, PALETTE_BYTES, 2)
        ]
        palettes.append([md_palette_color(word) for word in words])

    palette_dir = out_dir / "palettes"
    tileset_dir = out_dir / "tilesets"
    palette_dir.mkdir(parents=True, exist_ok=True)
    tileset_dir.mkdir(parents=True, exist_ok=True)

    palette_files = []
    for index, palette in enumerate(palettes):
        rel = palette_dir / f"MapPalette{index:02}.png"
        write_png_rgba(rel, PALETTE_COLORS * 16, 16, render_palette_strip(palette))
        palette_files.append(
            {
                "file": f"palettes/MapPalette{index:02}.png",
                "sha256": _sha256(rel.read_bytes()),
                "sizeBytes": rel.stat().st_size,
            }
        )

    reference_palette = palettes[options.tileset_palette]
    tileset_files = []
    for index in range(TILESET_COUNT):
        payload = (
            disasm / TILESET_SOURCE_ROOT / f"maptileset{index:03}.bin"
        ).read_bytes()
        decoded = decode_stack_compressed(payload)
        if len(decoded.output) != TILESET_DECODED_BYTES:
            raise ValueError(f"map tileset decode-size drift: {index}")
        tiles = [
            decode_md_4bpp_tile(
                decoded.output[offset : offset + TILE_BYTES_4BPP]
            )
            for offset in range(0, TILESET_DECODED_BYTES, TILE_BYTES_4BPP)
        ]
        rel = tileset_dir / f"MapTileset{index:03}.png"
        write_png_rgba(rel, 128, 64, render_tileset_sheet(tiles, reference_palette))
        tileset_files.append(
            {
                "file": f"tilesets/MapTileset{index:03}.png",
                "sha256": _sha256(rel.read_bytes()),
                "sizeBytes": rel.stat().st_size,
                "compressedBytes": len(payload),
            }
        )

    manifest = {
        "schemaVersion": 1,
        "tool": "sf2 texture extract",
        "romId": expected_rom["id"],
        "romSha256": expected_rom["hashes"]["sha256"],
        "upstream": {"repository": "ShiningForceCentral/SF2DISASM", "commit": commit},
        "options": {
            "tilesetPalette": options.tileset_palette,
            "tileSize": TILE_SIZE,
            "sheetGrid": "16x8",
            "paletteColorCount": PALETTE_COLORS,
        },
        "notes": [
            "tilesets are rendered with map palette 0 for direct viewing; all 16 "
            "palettes are emitted as color strips; color 0 is transparent",
        ],
        "summary": {
            "tilesetCount": len(tileset_files),
            "paletteCount": len(palette_files),
            "decodedTilesetBytes": TILESET_DECODED_BYTES * len(tileset_files),
            "totalFileCount": len(tileset_files) + len(palette_files),
        },
        "files": [*palette_files, *tileset_files],
    }
    manifest_path = out_dir / "manifest.json"
    manifest_json = json.dumps(manifest, ensure_ascii=False, indent=2) + "\n"
    manifest_path.write_text(manifest_json, encoding="utf-8")
    return {
        "Tilesets": len(tileset_files),
        "Palettes": len(palette_files),
        "DecodedTilesetBytes": manifest["summary"]["decodedTilesetBytes"],
        "Files": manifest["summary"]["totalFileCount"],
        "Output": str(out_dir),
        "Manifest": _sha256(manifest_path.read_bytes()),
        "Status": "PASS",
    }


def transform_tile_pixels(pixels: list[int], flags: int) -> list[int]:
    """Apply the tilemap flags to an 8x8 tile pixel list.

    ``0x1000`` flips vertically, ``0x800`` mirrors horizontally (matching the VDP
    ``VDPTILE_FLIP``/``VDPTILE_MIRROR`` bits). The priority bit (``0x8000``) does not
    change pixels.
    """
    if len(pixels) != 64:
        raise ValueError(f"tile must have 64 pixels, got {len(pixels)}")
    flip = flags & TILE_FLIP
    mirror = flags & TILE_MIRROR
    transformed = []
    for row in range(TILE_SIZE):
        for col in range(TILE_SIZE):
            src_row = (TILE_SIZE - 1) - row if flip else row
            src_col = (TILE_SIZE - 1) - col if mirror else col
            transformed.append(pixels[src_row * TILE_SIZE + src_col])
    return transformed


@dataclass(frozen=True)
class MapArea:
    index: int
    main_start: tuple[int, int]
    main_end: tuple[int, int]
    foreground_start: tuple[int, int]
    background_start: tuple[int, int]


@dataclass(frozen=True)
class MapData:
    map_index: int
    palette_index: int
    tileset_slots: tuple[int, int, int, int, int]
    areas: tuple[MapArea, ...]


def parse_map_entry(disasm: Path, map_index: int) -> MapData:
    """Parse a map entry header (00-tilesets.asm) and area bounds (2-areas.asm)."""
    header_path = MAP_ENTRY_ROOT / f"map{map_index:02}" / "00-tilesets.asm"
    header = read_upstream_text(disasm / header_path)
    palette_match = re.search(r"mapPalette\s+(\d+)", header)
    slots_match = re.findall(r"mapTileset[1-5]\s+(\d+)", header)
    if palette_match is None or len(slots_match) != MAP_SLOT_COUNT:
        raise ValueError(f"map entry header drift: {header_path}")
    palette_index = int(palette_match.group(1))
    slots = tuple(int(value) for value in slots_match)

    areas_path = MAP_ENTRY_ROOT / f"map{map_index:02}" / "2-areas.asm"
    areas_source = read_upstream_text(disasm / areas_path)
    starts = re.findall(r"mainLayerStart\s+(\d+),\s*(\d+)", areas_source)
    ends = re.findall(r"mainLayerEnd\s+(\d+),\s*(\d+)", areas_source)
    foregrounds = re.findall(r"scndLayerFgndStart\s+(\d+),\s*(\d+)", areas_source)
    backgrounds = re.findall(r"scndLayerBgndStart\s+(\d+),\s*(\d+)", areas_source)
    if not (len(starts) == len(ends) == len(foregrounds) == len(backgrounds)):
        raise ValueError(f"map entry area field-count drift: {areas_path}")
    areas = []
    for index, (start, end, foreground, background) in enumerate(
        zip(starts, ends, foregrounds, backgrounds, strict=True)
    ):
        areas.append(
            MapArea(
                index=index,
                main_start=(int(start[0]), int(start[1])),
                main_end=(int(end[0]), int(end[1])),
                foreground_start=(int(foreground[0]), int(foreground[1])),
                background_start=(int(background[0]), int(background[1])),
            )
        )
    if not areas:
        raise ValueError(f"map entry has no parsed areas: {areas_path}")
    return MapData(
        map_index=map_index,
        palette_index=palette_index,
        tileset_slots=slots,
        areas=tuple(areas),
    )


def load_map_tile_pool(
    disasm: Path, slots: tuple[int, ...]
) -> list[list[list[int]] | None]:
    """Decode the tilesets used by one map slot header into a per-slot tile pool."""
    pool: list[list[list[int]] | None] = []
    for slot in slots:
        if slot == 255:
            pool.append(None)
            continue
        payload = (
            disasm / TILESET_SOURCE_ROOT / f"maptileset{slot:03}.bin"
        ).read_bytes()
        decoded = decode_stack_compressed(payload)
        if len(decoded.output) != TILESET_DECODED_BYTES:
            raise ValueError(f"map tileset decode-size drift: {slot}")
        pool.append(
            [
                decode_md_4bpp_tile(decoded.output[o : o + TILE_BYTES_4BPP])
                for o in range(0, TILESET_DECODED_BYTES, TILE_BYTES_4BPP)
            ]
        )
    return pool


def render_map_blocks(
    layout: list[int],
    blocks: list[int],
    tile_pool: list[list[list[int]] | None],
    palette: list[tuple[int, int, int]],
    *,
    origin: tuple[int, int],
    width_blocks: int,
    height_blocks: int,
) -> list[int]:
    """Render a block region of a map layout into RGBA pixels.

    The layout holds block indices on a 64x64 grid; each block is nine tile words
    (3x3 tiles of 8x8 pixels). Tile words store the VRAM tile number plus 0x100 with
    the priority/flip/mirror flags in the high bits; the tile number maps to a
    tileset slot via ``index // 128``.
    """
    width = width_blocks * BLOCK_PIXELS
    height = height_blocks * BLOCK_PIXELS
    pixels = [0] * (width * height * 4)
    for block_y in range(height_blocks):
        for block_x in range(width_blocks):
            layout_offset = (origin[1] + block_y) * MAP_LAYOUT_SIDE + origin[0] + block_x
            block_index = layout[layout_offset] & TILE_INDEX_MASK
            block = blocks[block_index * BLOCK_TILES : block_index * BLOCK_TILES + BLOCK_TILES]
            for tile_y in range(BLOCK_TILES_PER_SIDE):
                for tile_x in range(BLOCK_TILES_PER_SIDE):
                    word = block[tile_y * BLOCK_TILES_PER_SIDE + tile_x]
                    tile_number = word & TILE_INDEX_MASK
                    if tile_number < TILE_INDEX_OFFSET:
                        continue
                    tile_index = tile_number - TILE_INDEX_OFFSET
                    slot = tile_index // MAP_SLOT_TILE_COUNT
                    local = tile_index % MAP_SLOT_TILE_COUNT
                    if slot >= len(tile_pool) or tile_pool[slot] is None:
                        continue
                    tile = transform_tile_pixels(tile_pool[slot][local], word)
                    pixel_x = block_x * BLOCK_PIXELS + tile_x * TILE_SIZE
                    pixel_y = block_y * BLOCK_PIXELS + tile_y * TILE_SIZE
                    for row in range(TILE_SIZE):
                        for col in range(TILE_SIZE):
                            color = palette[tile[row * TILE_SIZE + col]]
                            offset = ((pixel_y + row) * width + (pixel_x + col)) * 4
                            if color == (0, 0, 0):
                                pixels[offset : offset + 4] = [0, 0, 0, 0]
                            else:
                                pixels[offset : offset + 4] = [*color, 255]
    return pixels


def extract_map_renders(
    rom_path: Path,
    upstream_path: Path,
    *,
    out_dir: Path,
    maps: str,
) -> dict[str, object]:
    """Render map main-layer regions as private PNG images.

    For every requested map index, each parsed area's main layer is rendered with the
    map palette. Only the main layer is emitted; the second/background layer uses a
    separate CRAM palette whose exploration-mode source is not yet evidenced.
    """
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, _toolchain = _resolve_upstream(upstream_path)
    expected_rom = load_json(repo_path("manifests/roms/sf2-us.json"))
    rom = rom_path.read_bytes()
    if len(rom) != expected_rom["sizeBytes"] or _sha256(rom) != expected_rom["hashes"]["sha256"]:
        raise ValueError("map render ROM identity drift")

    wanted = []
    for token in maps.split(","):
        value = int(token.strip())
        if not 0 <= value < 128:
            raise ValueError(f"map index out of range: {value}")
        wanted.append(value)
    if not wanted:
        raise ValueError("no maps selected")

    out_dir = out_dir.resolve()
    out_dir.mkdir(parents=True, exist_ok=True)
    files = []
    for map_index in wanted:
        map_data = parse_map_entry(disasm, map_index)
        palette_words = [
            int.from_bytes(
                (
                    disasm
                    / PALETTE_SOURCE_ROOT
                    / f"mappalette{map_data.palette_index:02}.bin"
                ).read_bytes()[o : o + 2],
                "big",
            )
            for o in range(0, PALETTE_BYTES, 2)
        ]
        palette = [md_palette_color(w) for w in palette_words]
        tile_pool = load_map_tile_pool(disasm, map_data.tileset_slots)
        blocks_data = (
            disasm / MAP_ENTRY_ROOT / f"map{map_index:02}" / "0-blocks.bin"
        ).read_bytes()
        layout_data = (
            disasm / MAP_ENTRY_ROOT / f"map{map_index:02}" / "1-layout.bin"
        ).read_bytes()
        blocks, _, _ = decode_map_blocks(blocks_data)
        layout, _, _, _ = decode_map_layout(layout_data, len(blocks) // BLOCK_TILES)
        for area in map_data.areas:
            width_blocks = area.main_end[0] - area.main_start[0] + 1
            height_blocks = area.main_end[1] - area.main_start[1] + 1
            if width_blocks <= 0 or height_blocks <= 0:
                continue
            pixels = render_map_blocks(
                layout,
                blocks,
                tile_pool,
                palette,
                origin=area.main_start,
                width_blocks=width_blocks,
                height_blocks=height_blocks,
            )
            name = f"map{map_index:02}-area{area.index}-mainlayer.png"
            rel = out_dir / name
            write_png_rgba(rel, width_blocks * BLOCK_PIXELS, height_blocks * BLOCK_PIXELS, pixels)
            files.append(
                {
                    "file": name,
                    "sha256": _sha256(rel.read_bytes()),
                    "sizeBytes": rel.stat().st_size,
                    "map": map_index,
                    "area": area.index,
                    "palette": map_data.palette_index,
                    "widthBlocks": width_blocks,
                    "heightBlocks": height_blocks,
                }
            )
    manifest = {
        "schemaVersion": 1,
        "tool": "sf2 texture map",
        "romId": expected_rom["id"],
        "romSha256": expected_rom["hashes"]["sha256"],
        "upstream": {"repository": "ShiningForceCentral/SF2DISASM", "commit": commit},
        "notes": [
            "main layer only; tile priority is ignored in static rendering; the "
            "second/background layer palette source is not yet evidenced",
        ],
        "summary": {"areaRenderCount": len(files)},
        "files": files,
    }
    manifest_path = out_dir / "map-manifest.json"
    manifest_json = json.dumps(manifest, ensure_ascii=False, indent=2) + "\n"
    manifest_path.write_text(manifest_json, encoding="utf-8")
    return {
        "Maps": len(wanted),
        "Areas": len(files),
        "Output": str(out_dir),
        "Manifest": _sha256(manifest_path.read_bytes()),
        "Status": "PASS",
    }

# ---------------------------------------------------------------------------
# Fonts, portraits, icons, and map sprites
# ---------------------------------------------------------------------------

FONT_PATH = Path("data/graphics/tech/fonts/variablewidthfont.bin")
FONT_GLYPH_COUNT = 80
FONT_GLYPH_BYTES = 32
FONT_GLYPH_ROWS = 15
FONT_GLYPH_COLUMNS = 12
FONT_ASCII_TABLE_PATH = Path("data/scripting/text/asciitotextsymbolmap.asm")

PORTRAIT_ENTRIES_PATH = Path("data/graphics/portraits/entries.asm")
PORTRAIT_DECODED_BYTES = 2048
PORTRAIT_TILES_PER_SIDE = 8
PORTRAIT_PALETTE_OFFSET = None  # parsed per payload

ICON_SOURCE_ROOT = Path("data/graphics/icons")
ICON_BYTES = 192
ICON_COLUMNS = 2
ICON_ROWS = 3

MAPSPRITE_ENTRIES_PATH = Path("data/graphics/mapsprites/entries.asm")
MAPSPRITE_DECODED_BYTES = 0x240
MAPSPRITE_FRAME_COUNT = 2
MAPSPRITE_FRAME_TILES = 9
MAPSPRITE_FRAME_TILES_PER_SIDE = 3
MAPSPRITE_FRAME_BYTES = MAPSPRITE_FRAME_TILES * TILE_BYTES_4BPP
MAPSPRITE_SENTINEL_SYMBOL = "Mapsprite237_0"

BASE_PALETTE_PATH = Path("data/graphics/tech/basepalette.bin")


def _load_base_palette(disasm: Path) -> list[tuple[int, int, int]]:
    data = (disasm / BASE_PALETTE_PATH).read_bytes()
    words = [int.from_bytes(data[o : o + 2], "big") for o in range(0, len(data), 2)]
    return [md_palette_color(w) for w in words]


def extract_font_sheet(disasm: Path, out_dir: Path) -> dict[str, object]:
    """Render the 80-glyph variable-width font (1bpp, 12x15) into one PNG sheet."""
    font = (disasm / FONT_PATH).read_bytes()
    if len(font) != FONT_GLYPH_COUNT * FONT_GLYPH_BYTES:
        raise ValueError(f"font size drift: {len(font)}")
    columns, rows = 10, 8
    width, height = columns * FONT_GLYPH_COLUMNS, rows * FONT_GLYPH_ROWS
    pixels = [0] * (width * height * 4)
    for glyph_index in range(FONT_GLYPH_COUNT):
        glyph = font[glyph_index * FONT_GLYPH_BYTES : (glyph_index + 1) * FONT_GLYPH_BYTES]
        origin_x = (glyph_index % columns) * FONT_GLYPH_COLUMNS
        origin_y = (glyph_index // columns) * FONT_GLYPH_ROWS
        for row in range(FONT_GLYPH_ROWS):
            left = glyph[2 + row * 2]
            right = glyph[3 + row * 2]
            bits = (left << 4) | (right >> 4)
            for col in range(FONT_GLYPH_COLUMNS):
                if bits & (1 << (11 - col)):
                    offset = ((origin_y + row) * width + (origin_x + col)) * 4
                    pixels[offset : offset + 4] = [255, 255, 255, 255]
    rel = out_dir / "font-variablewidth.png"
    write_png_rgba(rel, width, height, pixels)
    return {
        "file": "font-variablewidth.png",
        "sha256": _sha256(rel.read_bytes()),
        "sizeBytes": rel.stat().st_size,
    }


def extract_portraits(disasm: Path, out_dir: Path) -> list[dict[str, object]]:
    """Render each portrait (Stack-compressed 2048 bytes = 64 tiles) with its palette."""
    source = read_upstream_text(disasm / PORTRAIT_ENTRIES_PATH)
    definitions = re.findall(
        r"^\s*(Portrait\d{2}):\s*incbin\s+\"([^\"]+)\"", source, re.MULTILINE
    )
    if not definitions:
        raise ValueError("portrait entries have no definitions")
    out_dir.mkdir(parents=True, exist_ok=True)
    files = []
    for symbol, relative_path in definitions:
        data = (disasm / relative_path).read_bytes()
        eye_entries, offset = _portrait_entries(data, 0)
        mouth_entries, offset = _portrait_entries(data, offset)
        del eye_entries, mouth_entries
        header_end = offset + PALETTE_BYTES
        palette_words = [
            int.from_bytes(data[offset + o : offset + o + 2], "big")
            for o in range(0, PALETTE_BYTES, 2)
        ]
        palette = [md_palette_color(w) for w in palette_words]
        decoded = decode_stack_compressed(
            data[header_end:], expected_output_bytes=PORTRAIT_DECODED_BYTES
        )
        tiles = [
            decode_md_4bpp_tile(decoded.output[o : o + TILE_BYTES_4BPP])
            for o in range(0, PORTRAIT_DECODED_BYTES, TILE_BYTES_4BPP)
        ]
        width = PORTRAIT_TILES_PER_SIDE * TILE_SIZE
        pixels = [0] * (width * width * 4)
        for tile_index, tile in enumerate(tiles):
            origin_x = (tile_index % PORTRAIT_TILES_PER_SIDE) * TILE_SIZE
            origin_y = (tile_index // PORTRAIT_TILES_PER_SIDE) * TILE_SIZE
            for row in range(TILE_SIZE):
                for col in range(TILE_SIZE):
                    color = palette[tile[row * TILE_SIZE + col]]
                    offset = ((origin_y + row) * width + (origin_x + col)) * 4
                    if color == (0, 0, 0):
                        pixels[offset : offset + 4] = [0, 0, 0, 0]
                    else:
                        pixels[offset : offset + 4] = [*color, 255]
        name = f"{symbol}.png"
        rel = out_dir / name
        write_png_rgba(rel, width, width, pixels)
        files.append(
            {"file": name, "sha256": _sha256(rel.read_bytes()), "sizeBytes": rel.stat().st_size}
        )
    return files


def _portrait_entries(data: bytes, offset: int) -> tuple[list[list[int]], int]:
    if offset + 2 > len(data):
        raise ValueError("portrait animation entry count is truncated")
    count = int.from_bytes(data[offset : offset + 2], "big")
    offset += 2
    end = offset + count * 4
    if end > len(data):
        raise ValueError("portrait animation entries are truncated")
    entries = [list(data[index : index + 4]) for index in range(offset, end, 4)]
    return entries, end


def extract_icons(disasm: Path, out_dir: Path) -> list[dict[str, object]]:
    """Render the assembled item/spell/other icons (192 bytes = 2x3 tiles)."""
    source = read_upstream_text(disasm / (ICON_SOURCE_ROOT / "entries.asm"))
    definitions = re.findall(
        r"^\s*((?:Item|Other|Spell)Icon\d{3}):\s*incbin\s+\"([^\"]+)\"",
        source,
        re.MULTILINE,
    )
    if not definitions:
        raise ValueError("icon entries have no definitions")
    palette = _load_base_palette(disasm)
    out_dir.mkdir(parents=True, exist_ok=True)
    files = []
    for symbol, relative_path in definitions:
        data = (disasm / relative_path).read_bytes()
        if len(data) != ICON_BYTES:
            raise ValueError(f"icon size drift: {symbol}")
        tiles = [
            decode_md_4bpp_tile(data[o : o + TILE_BYTES_4BPP])
            for o in range(0, ICON_BYTES, TILE_BYTES_4BPP)
        ]
        width = ICON_COLUMNS * TILE_SIZE
        height = ICON_ROWS * TILE_SIZE
        pixels = [0] * (width * height * 4)
        for tile_index, tile in enumerate(tiles):
            origin_x = (tile_index % ICON_COLUMNS) * TILE_SIZE
            origin_y = (tile_index // ICON_COLUMNS) * TILE_SIZE
            for row in range(TILE_SIZE):
                for col in range(TILE_SIZE):
                    color = palette[tile[row * TILE_SIZE + col]]
                    offset = ((origin_y + row) * width + (origin_x + col)) * 4
                    if color == (0, 0, 0):
                        pixels[offset : offset + 4] = [0, 0, 0, 0]
                    else:
                        pixels[offset : offset + 4] = [*color, 255]
        name = f"{symbol}.png"
        rel = out_dir / name
        write_png_rgba(rel, width, height, pixels)
        files.append(
            {"file": name, "sha256": _sha256(rel.read_bytes()), "sizeBytes": rel.stat().st_size}
        )
    return files


def extract_map_sprites(disasm: Path, out_dir: Path) -> list[dict[str, object]]:
    """Render each map sprite (Basic-compressed 0x240 bytes = 2 frames of 3x3 tiles)."""
    source = read_upstream_text(disasm / MAPSPRITE_ENTRIES_PATH)
    definitions = re.findall(
        r"^\s*(Mapsprite\d{3}_[012]):\s*incbin\s+\"([^\"]+)\"", source, re.MULTILINE
    )
    if not definitions:
        raise ValueError("map sprite entries have no definitions")
    palette = _load_base_palette(disasm)
    out_dir.mkdir(parents=True, exist_ok=True)
    files = []
    seen = set()
    for symbol, relative_path in definitions:
        if symbol == MAPSPRITE_SENTINEL_SYMBOL:
            continue
        if symbol in seen:
            continue
        seen.add(symbol)
        data = (disasm / relative_path).read_bytes()
        decoded = decode_basic_compressed(data, expected_output_bytes=MAPSPRITE_DECODED_BYTES)
        # tiles are stored contiguously (32 bytes each); a frame is 9 tiles arranged
        # column-major on a 3x3 grid (tile 0 top-left, 1 middle-left, 2 bottom-left)
        frame_pixels = []
        for frame in range(MAPSPRITE_FRAME_COUNT):
            frame_data = decoded.output[
                frame * MAPSPRITE_FRAME_BYTES : (frame + 1) * MAPSPRITE_FRAME_BYTES
            ]
            tiles = [
                decode_md_4bpp_tile(frame_data[o : o + TILE_BYTES_4BPP])
                for o in range(0, MAPSPRITE_FRAME_BYTES, TILE_BYTES_4BPP)
            ]
            frame_pixels.append(tiles)
        # one PNG with the two frames side by side
        frame_side = MAPSPRITE_FRAME_TILES_PER_SIDE * TILE_SIZE
        width = frame_side * MAPSPRITE_FRAME_COUNT
        height = frame_side
        pixels = [0] * (width * height * 4)
        for frame, tiles in enumerate(frame_pixels):
            for tile_index, tile in enumerate(tiles):
                tile_col = tile_index // MAPSPRITE_FRAME_TILES_PER_SIDE
                origin_x = frame * frame_side + tile_col * TILE_SIZE
                origin_y = (tile_index % MAPSPRITE_FRAME_TILES_PER_SIDE) * TILE_SIZE
                for row in range(TILE_SIZE):
                    for col in range(TILE_SIZE):
                        color = palette[tile[row * TILE_SIZE + col]]
                        offset = ((origin_y + row) * width + (origin_x + col)) * 4
                        if color == (0, 0, 0):
                            pixels[offset : offset + 4] = [0, 0, 0, 0]
                        else:
                            pixels[offset : offset + 4] = [*color, 255]
        name = f"{symbol}.png"
        rel = out_dir / name
        write_png_rgba(rel, width, height, pixels)
        files.append(
            {"file": name, "sha256": _sha256(rel.read_bytes()), "sizeBytes": rel.stat().st_size}
        )
    return files


def extract_graphic_assets(
    rom_path: Path,
    upstream_path: Path,
    *,
    out_dir: Path,
) -> dict[str, object]:
    """Extract the font sheet, portraits, icons, and map sprites as private PNGs."""
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, _toolchain = _resolve_upstream(upstream_path)
    expected_rom = load_json(repo_path("manifests/roms/sf2-us.json"))
    rom = rom_path.read_bytes()
    if len(rom) != expected_rom["sizeBytes"] or _sha256(rom) != expected_rom["hashes"]["sha256"]:
        raise ValueError("graphic asset extraction ROM identity drift")

    out_dir = out_dir.resolve()
    out_dir.mkdir(parents=True, exist_ok=True)
    font_dir = out_dir / "font"
    portrait_dir = out_dir / "portraits"
    icon_dir = out_dir / "icons"
    sprite_dir = out_dir / "mapsprites"
    for directory in (font_dir, portrait_dir, icon_dir, sprite_dir):
        directory.mkdir(parents=True, exist_ok=True)

    font_files = [extract_font_sheet(disasm, font_dir)]
    portrait_files = extract_portraits(disasm, portrait_dir)
    icon_files = extract_icons(disasm, icon_dir)
    sprite_files = extract_map_sprites(disasm, sprite_dir)

    all_files = [
        *[{"kind": "font", **row} for row in font_files],
        *[{"kind": "portrait", **row} for row in portrait_files],
        *[{"kind": "icon", **row} for row in icon_files],
        *[{"kind": "mapsprite", **row} for row in sprite_files],
    ]
    manifest = {
        "schemaVersion": 1,
        "tool": "sf2 texture assets",
        "romId": expected_rom["id"],
        "romSha256": expected_rom["hashes"]["sha256"],
        "upstream": {"repository": "ShiningForceCentral/SF2DISASM", "commit": commit},
        "notes": [
            "font: 80 glyphs, 1bpp 12x15 per glyph, white on transparent",
            "portraits: 64 tiles on an 8x8 grid (64x64) with the per-portrait palette; "
            "eye/mouth animation entries are recorded in the contract, not rendered",
            "icons and map sprites use the base palette "
            "(data/graphics/tech/basepalette.bin, 'Palette for UI and mapsprites'); "
            "icons are 2x3 tiles (16x24), map sprites are 3x6 tiles (24x48)",
        ],
        "summary": {
            "fontCount": len(font_files),
            "portraitCount": len(portrait_files),
            "iconCount": len(icon_files),
            "mapspriteCount": len(sprite_files),
            "totalFileCount": len(all_files),
        },
        "files": all_files,
    }
    manifest_path = out_dir / "assets-manifest.json"
    manifest_json = json.dumps(manifest, ensure_ascii=False, indent=2) + "\n"
    manifest_path.write_text(manifest_json, encoding="utf-8")
    return {
        "Fonts": len(font_files),
        "Portraits": len(portrait_files),
        "Icons": len(icon_files),
        "MapSprites": len(sprite_files),
        "Files": len(all_files),
        "Output": str(out_dir),
        "Manifest": _sha256(manifest_path.read_bytes()),
        "Status": "PASS",
    }

# ---------------------------------------------------------------------------
# UI resources, special sprites, battle backgrounds, unused assets
# ---------------------------------------------------------------------------

UI_RESOURCE_SPECS = [
    ("tiles_Base", "data/graphics/tech/basetiles.bin", 8192, "base"),
    ("tiles_ItemMenu", "data/graphics/tech/menus/itemmenutiles.bin", 2304, "diamond-menu"),
    (
        "tiles_BattleFieldMenu",
        "data/graphics/tech/menus/battlefieldmenutiles.bin",
        2304,
        "diamond-menu",
    ),
    ("tiles_ChurchMenu", "data/graphics/tech/menus/churchmenutiles.bin", 2304, "diamond-menu"),
    ("tiles_ShopMenu", "data/graphics/tech/menus/shopmenutiles.bin", 2304, "diamond-menu"),
    ("tiles_CaravanMenu", "data/graphics/tech/menus/caravanmenutiles.bin", 2304, "diamond-menu"),
    ("tiles_DepotMenu", "data/graphics/tech/menus/depotmenutiles.bin", 2304, "diamond-menu"),
    ("tiles_YesNoPrompt", "data/graphics/tech/menus/yesnoprompttiles.bin", 1152, "yes-no"),
]
MAIN_MENU_PATH = Path("data/graphics/tech/menus/mainmenutiles.bin")
MAIN_MENU_ICON_BYTES = 576

SPECIAL_SPRITE_ENTRIES_PATH = Path("data/graphics/specialsprites/entries.asm")

BATTLE_BACKGROUND_ENTRIES_PATH = Path("data/graphics/battles/backgrounds/entries.asm")
BATTLE_BACKGROUND_TILESET_BYTES = 6144
BATTLE_BACKGROUND_LAYOUT_PATH = Path("data/graphics/tech/backgroundlayout.asm")
BATTLE_BACKGROUND_LAYOUT_TILES = 384
BATTLE_BACKGROUND_LAYOUT_COLUMNS = 32

UNUSED_CLOUD_PATH = Path("data/graphics/tech/unusedcloudtiles.bin")
UNUSED_PALETTE_PATH = Path("data/graphics/tech/unusedbasepalettes.bin")


def _render_tile_pool_sheet(
    tile_data: bytes, palette: list[tuple[int, int, int]], *, sheet_columns: int = 16
) -> tuple[list[int], int, int]:
    """Render decoded 4bpp tile data as one sheet (row-major 8x8 tiles)."""
    tiles = [
        decode_md_4bpp_tile(tile_data[o : o + TILE_BYTES_4BPP])
        for o in range(0, len(tile_data), TILE_BYTES_4BPP)
    ]
    rows = (len(tiles) + sheet_columns - 1) // sheet_columns
    width = sheet_columns * TILE_SIZE
    height = rows * TILE_SIZE
    pixels = [0] * (width * height * 4)
    for tile_index, tile in enumerate(tiles):
        origin_x = (tile_index % sheet_columns) * TILE_SIZE
        origin_y = (tile_index // sheet_columns) * TILE_SIZE
        for row in range(TILE_SIZE):
            for col in range(TILE_SIZE):
                color = palette[tile[row * TILE_SIZE + col]]
                offset = ((origin_y + row) * width + (origin_x + col)) * 4
                if color == (0, 0, 0):
                    pixels[offset : offset + 4] = [0, 0, 0, 0]
                else:
                    pixels[offset : offset + 4] = [*color, 255]
    return pixels, width, height


def extract_ui_resources(disasm: Path, out_dir: Path) -> list[dict[str, object]]:
    """Render the base/diamond-menu/yes-no tile sets and the main-menu icons."""
    palette = _load_base_palette(disasm)
    out_dir.mkdir(parents=True, exist_ok=True)
    files = []
    for symbol, relative_path, expected, _family in UI_RESOURCE_SPECS:
        data = (disasm / relative_path).read_bytes()
        decoded = decode_stack_compressed(data, expected_output_bytes=expected)
        pixels, width, height = _render_tile_pool_sheet(decoded.output, palette)
        name = f"{symbol}.png"
        rel = out_dir / name
        write_png_rgba(rel, width, height, pixels)
        files.append(
            {"file": name, "sha256": _sha256(rel.read_bytes()), "sizeBytes": rel.stat().st_size}
        )
    main_menu = (disasm / MAIN_MENU_PATH).read_bytes()
    for icon_index in range(7):
        icon_start = icon_index * MAIN_MENU_ICON_BYTES
        icon = main_menu[icon_start : icon_start + MAIN_MENU_ICON_BYTES]
        pixels, width, height = _render_tile_pool_sheet(icon, palette, sheet_columns=6)
        name = f"tiles_MainMenu_icon{icon_index}.png"
        rel = out_dir / name
        write_png_rgba(rel, width, height, pixels)
        files.append(
            {"file": name, "sha256": _sha256(rel.read_bytes()), "sizeBytes": rel.stat().st_size}
        )
    return files


def extract_special_sprites(disasm: Path, out_dir: Path) -> list[dict[str, object]]:
    """Render the six special-sprite streams (each with its own 16-color palette).

    The Stack decode sizes are confirmed (72 battle-class tiles, 162 for
    `SpecialSprite_NazcaShip`; `SpecialSprite_EvilSpiritAlt` is animation-only), but the
    palette word layout and the frame assembly are still unconfirmed, so each resource is
    emitted as a raw tile-pool sheet plus candidate composed layouts (3x3, 3x6, 4x4, 6x6)
    for later visual inspection. None of the candidates is verified against a screenshot.
    """
    source = read_upstream_text(disasm / SPECIAL_SPRITE_ENTRIES_PATH)
    definitions = re.findall(
        r"^\s*(SpecialSprite_\w+):\s*incbin\s+\"([^\"]+)\"", source, re.MULTILINE
    )
    if not definitions:
        raise ValueError("special sprite entries have no definitions")
    out_dir.mkdir(parents=True, exist_ok=True)
    files = []
    for symbol, relative_path in definitions:
        data = (disasm / relative_path).read_bytes()
        if symbol == "SpecialSprite_EvilSpiritAlt":
            # animation-only stream without a palette header
            palette_words = [0] * 16
            stream = data
        else:
            palette_words = [
                int.from_bytes(data[o : o + 2], "big") for o in range(0, 32, 2)
            ]
            stream = data[32:]
        palette = [md_cram_color(w) for w in palette_words]
        decoded = decode_stack_compressed(stream)
        pixels, width, height = _render_tile_pool_sheet(decoded.output, palette)
        name = f"{symbol}.png"
        rel = out_dir / name
        write_png_rgba(rel, width, height, pixels)
        files.append(
            {"file": name, "sha256": _sha256(rel.read_bytes()), "sizeBytes": rel.stat().st_size}
        )
        # composed frames: try several plausible frame layouts side by side
        layouts = [
            ("3x6", 18, 3, 6),
            ("3x3", 9, 3, 3),
            ("6x6", 36, 6, 6),
            ("4x4", 16, 4, 4),
        ]
        tile_count = len(decoded.output) // TILE_BYTES_4BPP
        for layout_name, frame_tiles, columns, rows in layouts:
            if tile_count % frame_tiles:
                continue
            frame_count = tile_count // frame_tiles
            frame_side_w = columns * TILE_SIZE
            frame_side_h = rows * TILE_SIZE
            width = frame_side_w * frame_count
            height = frame_side_h
            pixels = [0] * (width * height * 4)
            for frame in range(frame_count):
                frame_data = decoded.output[
                    frame * frame_tiles * TILE_BYTES_4BPP :
                    (frame + 1) * frame_tiles * TILE_BYTES_4BPP
                ]
                tiles = [
                    decode_md_4bpp_tile(frame_data[o : o + TILE_BYTES_4BPP])
                    for o in range(0, len(frame_data), TILE_BYTES_4BPP)
                ]
                for tile_index, tile in enumerate(tiles):
                    origin_x = frame * frame_side_w + (tile_index % columns) * TILE_SIZE
                    origin_y = (tile_index // columns) * TILE_SIZE
                    for row in range(TILE_SIZE):
                        for col in range(TILE_SIZE):
                            color = palette[tile[row * TILE_SIZE + col]]
                            offset = ((origin_y + row) * width + (origin_x + col)) * 4
                            if color == (0, 0, 0):
                                pixels[offset : offset + 4] = [0, 0, 0, 0]
                            else:
                                pixels[offset : offset + 4] = [*color, 255]
            name = f"{symbol}_layout{layout_name}.png"
            rel = out_dir / name
            write_png_rgba(rel, width, height, pixels)
            files.append(
                {
                    "file": name,
                    "sha256": _sha256(rel.read_bytes()),
                    "sizeBytes": rel.stat().st_size,
                }
            )
    return files


def extract_battle_backgrounds(disasm: Path, out_dir: Path) -> list[dict[str, object]]:
    """Render each battle background's two tilesets, palette, and composed sheet."""
    source = read_upstream_text(disasm / BATTLE_BACKGROUND_ENTRIES_PATH)
    definitions = re.findall(
        r"^\s*(Background\d{2}):\s*incbin\s+\"([^\"]+)\"", source, re.MULTILINE
    )
    if not definitions:
        raise ValueError("battle background entries have no definitions")
    layout_source = read_upstream_text(disasm / BATTLE_BACKGROUND_LAYOUT_PATH)
    layout_tiles = [
        int(value)
        for value in re.findall(r"vdpTile\s+(\d+)", layout_source)
    ]
    if len(layout_tiles) != BATTLE_BACKGROUND_LAYOUT_TILES:
        raise ValueError(
            f"battle background layout tile-count drift: {len(layout_tiles)}"
        )
    out_dir.mkdir(parents=True, exist_ok=True)
    files = []
    for symbol, relative_path in definitions:
        data = (disasm / relative_path).read_bytes()
        tileset1_offset = int.from_bytes(data[0:2], "big")
        tileset2_offset = 2 + int.from_bytes(data[2:4], "big")
        palette_offset = 4 + int.from_bytes(data[4:6], "big")
        if not (tileset1_offset == 38 and palette_offset == 6):
            raise ValueError(f"battle background header drift: {symbol}")
        palette_words = [
            int.from_bytes(data[palette_offset + o : palette_offset + o + 2], "big")
            for o in range(0, PALETTE_BYTES, 2)
        ]
        palette = [md_palette_color(w) for w in palette_words]
        first = decode_stack_compressed(
            data[tileset1_offset:tileset2_offset],
            expected_output_bytes=BATTLE_BACKGROUND_TILESET_BYTES,
        )
        second = decode_stack_compressed(data[tileset2_offset:])
        for part, decoded in (("tileset1", first), ("tileset2", second)):
            pixels, width, height = _render_tile_pool_sheet(decoded.output, palette)
            name = f"{symbol}_{part}.png"
            rel = out_dir / name
            write_png_rgba(rel, width, height, pixels)
            files.append(
                {
                    "file": name,
                    "sha256": _sha256(rel.read_bytes()),
                    "sizeBytes": rel.stat().st_size,
                }
            )
        strip = out_dir / f"{symbol}_palette.png"
        write_png_rgba(strip, PALETTE_COLORS * 16, 16, render_palette_strip(palette))
        files.append(
            {
                "file": f"{symbol}_palette.png",
                "sha256": _sha256(strip.read_bytes()),
                "sizeBytes": strip.stat().st_size,
            }
        )
        # composed sheet: the layout lists VRAM tile numbers starting at 928
        pool = first.output + second.output
        pool_tiles = [
            decode_md_4bpp_tile(pool[o : o + TILE_BYTES_4BPP])
            for o in range(0, len(pool), TILE_BYTES_4BPP)
        ]
        base = min(layout_tiles)
        columns = BATTLE_BACKGROUND_LAYOUT_COLUMNS
        rows = (len(layout_tiles) + columns - 1) // columns
        width = columns * TILE_SIZE
        height = rows * TILE_SIZE
        pixels = [0] * (width * height * 4)
        for index, tile_number in enumerate(layout_tiles):
            pool_index = tile_number - base
            if pool_index >= len(pool_tiles):
                continue
            tile = pool_tiles[pool_index]
            origin_x = (index % columns) * TILE_SIZE
            origin_y = (index // columns) * TILE_SIZE
            for row in range(TILE_SIZE):
                for col in range(TILE_SIZE):
                    color = palette[tile[row * TILE_SIZE + col]]
                    offset = ((origin_y + row) * width + (origin_x + col)) * 4
                    if color == (0, 0, 0):
                        pixels[offset : offset + 4] = [0, 0, 0, 0]
                    else:
                        pixels[offset : offset + 4] = [*color, 255]
        name = f"{symbol}_composed.png"
        rel = out_dir / name
        write_png_rgba(rel, width, height, pixels)
        files.append(
            {
                "file": name,
                "sha256": _sha256(rel.read_bytes()),
                "sizeBytes": rel.stat().st_size,
            }
        )
    return files


def extract_unused_assets(disasm: Path, out_dir: Path) -> list[dict[str, object]]:
    """Render the unused cloud streams and the two unused base palettes."""
    out_dir.mkdir(parents=True, exist_ok=True)
    files = []
    cloud = decode_stack_compressed((disasm / UNUSED_CLOUD_PATH).read_bytes()).output
    palette_data = (disasm / UNUSED_PALETTE_PATH).read_bytes()
    palettes = [
        [
            md_palette_color(int.from_bytes(palette_data[o : o + 2], "big"))
            for o in range(p * 32, p * 32 + 32, 2)
        ]
        for p in range(2)
    ]
    for palette_index, palette in enumerate(palettes):
        pixels, width, height = _render_tile_pool_sheet(cloud, palette)
        name = f"unusedcloud_palette{palette_index}.png"
        rel = out_dir / name
        write_png_rgba(rel, width, height, pixels)
        files.append(
            {"file": name, "sha256": _sha256(rel.read_bytes()), "sizeBytes": rel.stat().st_size}
        )
        strip = out_dir / f"unusedbase_palette{palette_index}.png"
        write_png_rgba(strip, PALETTE_COLORS * 16, 16, render_palette_strip(palette))
        files.append(
            {
                "file": f"unusedbase_palette{palette_index}.png",
                "sha256": _sha256(strip.read_bytes()),
                "sizeBytes": strip.stat().st_size,
            }
        )
    return files


def extract_misc_graphics(
    rom_path: Path,
    upstream_path: Path,
    *,
    out_dir: Path,
) -> dict[str, object]:
    """Extract UI resources, special sprites, battle background sheets, and unused assets."""
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, _toolchain = _resolve_upstream(upstream_path)
    expected_rom = load_json(repo_path("manifests/roms/sf2-us.json"))
    rom = rom_path.read_bytes()
    if len(rom) != expected_rom["sizeBytes"] or _sha256(rom) != expected_rom["hashes"]["sha256"]:
        raise ValueError("misc graphic extraction ROM identity drift")

    out_dir = out_dir.resolve()
    out_dir.mkdir(parents=True, exist_ok=True)
    ui_dir = out_dir / "ui"
    sprite_dir = out_dir / "specialsprites"
    background_dir = out_dir / "battlebackgrounds"
    unused_dir = out_dir / "unused"
    for directory in (ui_dir, sprite_dir, background_dir, unused_dir):
        directory.mkdir(parents=True, exist_ok=True)

    ui_files = extract_ui_resources(disasm, ui_dir)
    sprite_files = extract_special_sprites(disasm, sprite_dir)
    background_files = extract_battle_backgrounds(disasm, background_dir)
    unused_files = extract_unused_assets(disasm, unused_dir)

    all_files = [
        *[{"kind": "ui", **row} for row in ui_files],
        *[{"kind": "specialsprite", **row} for row in sprite_files],
        *[{"kind": "battlebackground", **row} for row in background_files],
        *[{"kind": "unused", **row} for row in unused_files],
    ]
    manifest = {
        "schemaVersion": 1,
        "tool": "sf2 texture misc",
        "romId": expected_rom["id"],
        "romSha256": expected_rom["hashes"]["sha256"],
        "upstream": {"repository": "ShiningForceCentral/SF2DISASM", "commit": commit},
        "notes": [
            "UI tile sets and main-menu icons use the base palette",
            "(data/graphics/tech/basepalette.bin); special sprites and battle backgrounds "
            "carry their own 16-color palettes; battle backgrounds are emitted as raw "
            "tileset sheets (the screen assembly layout is not yet reconstructed)",
            "unused cloud tiles are rendered with both unused base palettes",
        ],
        "summary": {
            "uiCount": len(ui_files),
            "specialspriteCount": len(sprite_files),
            "battlebackgroundCount": len(background_files),
            "unusedCount": len(unused_files),
            "totalFileCount": len(all_files),
        },
        "files": all_files,
    }
    manifest_path = out_dir / "misc-manifest.json"
    manifest_json = json.dumps(manifest, ensure_ascii=False, indent=2) + "\n"
    manifest_path.write_text(manifest_json, encoding="utf-8")
    return {
        "Ui": len(ui_files),
        "SpecialSprites": len(sprite_files),
        "BattleBackgrounds": len(background_files),
        "Unused": len(unused_files),
        "Files": len(all_files),
        "Output": str(out_dir),
        "Manifest": _sha256(manifest_path.read_bytes()),
        "Status": "PASS",
    }
