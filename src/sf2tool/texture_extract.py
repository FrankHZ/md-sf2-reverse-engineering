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

from sf2tool.compression import decode_stack_compressed
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
