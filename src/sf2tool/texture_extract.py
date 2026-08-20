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
    transparent_first: bool = False,
) -> list[int]:
    """Render a block region of a map layout into RGBA pixels.

    The layout holds block indices on a 64x64 grid; each block is nine tile words
    (3x3 tiles of 8x8 pixels). Tile words store the VRAM tile number plus 0x100 with
    the priority/flip/mirror flags in the high bits; the tile number maps to a
    tileset slot via ``index // 128``. With ``transparent_first``, palette index 0
    is transparent (the game's second-layer holes).
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
                            index = tile[row * TILE_SIZE + col]
                            if transparent_first and index == 0:
                                continue
                            color = palette[index]
                            offset = ((pixel_y + row) * width + (pixel_x + col)) * 4
                            if color == (0, 0, 0):
                                pixels[offset : offset + 4] = [0, 0, 0, 0]
                            else:
                                pixels[offset : offset + 4] = [*color, 255]
    return pixels


def area_overlay_delta(area: MapArea) -> tuple[int, int]:
    """Layout-block offset of the second-layer content over the main layer.

    `SetViewDestination` (`display.asm`) scrolls Plane A at the camera plus the
    second-layer foreground start and Plane B at the camera plus the background
    start, so second-layer block ``(x, y) + (fg - bg)`` is displayed over main
    block ``(x, y)``. A zero delta means the second layer is in place.
    """
    return (
        area.foreground_start[0] - area.background_start[0],
        area.foreground_start[1] - area.background_start[1],
    )


def clip_block_rect(
    x0: int, y0: int, x1: int, y1: int, side: int = MAP_LAYOUT_SIDE
) -> tuple[int, int, int, int] | None:
    """Intersect a block rectangle with the ``side`` x ``side`` layout grid."""
    x0, y0 = max(x0, 0), max(y0, 0)
    x1, y1 = min(x1, side - 1), min(y1, side - 1)
    if x0 > x1 or y0 > y1:
        return None
    return x0, y0, x1, y1


def composite_overlay(
    main_pixels: list[int],
    overlay_pixels: list[int],
    *,
    width: int,
    height: int,
    overlay_width: int,
    offset_x: int,
    offset_y: int,
) -> list[int]:
    """Overlay second-layer pixels over the main-layer buffer (alpha 0 = keep main).

    ``width``/``height`` are the main buffer pixel dimensions; ``overlay_width`` is
    the overlay buffer's row stride; ``offset_x``/``offset_y`` place the overlay
    buffer's top-left inside the main buffer.
    """
    if len(main_pixels) != width * height * 4:
        raise ValueError("main pixel buffer size drift")
    if overlay_width <= 0 or len(overlay_pixels) % (overlay_width * 4):
        raise ValueError("overlay pixel buffer size drift")
    overlay_height = len(overlay_pixels) // (overlay_width * 4)
    composed = list(main_pixels)
    for overlay_y in range(overlay_height):
        target_y = offset_y + overlay_y
        if not 0 <= target_y < height:
            continue
        for overlay_x in range(overlay_width):
            target_x = offset_x + overlay_x
            if not 0 <= target_x < width:
                continue
            source = (overlay_y * overlay_width + overlay_x) * 4
            if overlay_pixels[source + 3] == 0:
                continue
            target = (target_y * width + target_x) * 4
            composed[target : target + 4] = overlay_pixels[source : source + 4]
    return composed


Layer2Copy = tuple[
    tuple[int, int], tuple[int, int], tuple[int, int], tuple[int, int]
]


def parse_layer2_copies(disasm: Path, map_index: int) -> list[Layer2Copy]:
    """Parse the roof/layer-2 block-copy records of one map (`5-roof-events.asm`).

    Returns ``(trigger, source, size, dest)`` block tuples. Records whose source is
    ``(255, 255)`` are clear records: the content already sits at the destination in
    the static layout, so they do not change a static render.
    """
    source_path = MAP_ENTRY_ROOT / f"map{map_index:02}" / "5-roof-events.asm"
    source = read_upstream_text(disasm / source_path)
    triggers = re.findall(r"slbc\s+(\d+),\s*(\d+)", source)
    sources = re.findall(r"slbcSource\s+(\d+),\s*(\d+)", source)
    sizes = re.findall(r"slbcSize\s+(\d+),\s*(\d+)", source)
    dests = re.findall(r"slbcDest\s+(\d+),\s*(\d+)", source)
    if not (len(triggers) == len(sources) == len(sizes) == len(dests)):
        raise ValueError(f"map layer-2 copy field-count drift: {source_path}")
    return [
        (
            (int(trigger[0]), int(trigger[1])),
            (int(source[0]), int(source[1])),
            (int(size[0]), int(size[1])),
            (int(dest[0]), int(dest[1])),
        )
        for trigger, source, size, dest in zip(
            triggers, sources, sizes, dests, strict=True
        )
    ]


def apply_layer2_copies(layout: list[int], copies: list[Layer2Copy]) -> list[int]:
    """Apply source-to-destination layer-2 block copies to a working layout copy.

    Each record snapshots its source rectangle before writing, matching the game's
    snapshot-then-copy path; clear records (source `(255, 255)`) are skipped.
    """
    working = list(layout)
    for _trigger, source, size, dest in copies:
        if source == (255, 255):
            continue
        width, height = size
        snapshot = [
            layout[(source[1] + row) * MAP_LAYOUT_SIDE + source[0] + col]
            for row in range(height)
            for col in range(width)
        ]
        for row in range(height):
            for col in range(width):
                dx = dest[0] + col
                dy = dest[1] + row
                if not (0 <= dx < MAP_LAYOUT_SIDE and 0 <= dy < MAP_LAYOUT_SIDE):
                    raise ValueError("map layer-2 copy destination out of range")
                working[dy * MAP_LAYOUT_SIDE + dx] = snapshot[row * width + col]
    return working


def extract_map_renders(
    rom_path: Path,
    upstream_path: Path,
    *,
    out_dir: Path,
    maps: str,
) -> dict[str, object]:
    """Render map main-layer regions as private PNG images.

    For every requested map index, each parsed area's main layer is rendered with the
    map palette under ``<out-dir>/maps/mapNN/``. Areas with a non-zero overlay offset
    additionally get the second-layer region rendered alone (``...-overlay.png``) and
    composed over the main layer (``...-composed.png``); the overlay passes first apply
    the map's roof/layer-2 copy records (source regions such as the map-3 cell bars are
    stored outside the areas and copied to their display position at runtime). The
    second/background layer's exploration-mode palette source is not yet evidenced;
    rendering uses the map palette.
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
    maps_dir = out_dir / "maps"
    maps_dir.mkdir(parents=True, exist_ok=True)
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
        copies = parse_layer2_copies(disasm, map_index)
        overlay_layout = apply_layer2_copies(layout, copies) if copies else layout
        map_dir = maps_dir / f"map{map_index:02}"
        map_dir.mkdir(parents=True, exist_ok=True)
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
            rel = map_dir / name
            write_png_rgba(rel, width_blocks * BLOCK_PIXELS, height_blocks * BLOCK_PIXELS, pixels)
            files.append(
                {
                    "file": f"map{map_index:02}/{name}",
                    "sha256": _sha256(rel.read_bytes()),
                    "sizeBytes": rel.stat().st_size,
                    "map": map_index,
                    "area": area.index,
                    "palette": map_data.palette_index,
                    "layer": "main",
                    "widthBlocks": width_blocks,
                    "heightBlocks": height_blocks,
                }
            )
            delta = area_overlay_delta(area)
            if delta != (0, 0):
                rect = clip_block_rect(
                    area.main_start[0] + delta[0],
                    area.main_start[1] + delta[1],
                    area.main_end[0] + delta[0],
                    area.main_end[1] + delta[1],
                )
                if rect is not None:
                    ox0, oy0, ox1, oy1 = rect
                    overlay = render_map_blocks(
                        overlay_layout,
                        blocks,
                        tile_pool,
                        palette,
                        origin=(ox0, oy0),
                        width_blocks=ox1 - ox0 + 1,
                        height_blocks=oy1 - oy0 + 1,
                        transparent_first=True,
                    )
                    name = f"map{map_index:02}-area{area.index}-overlay.png"
                    rel = map_dir / name
                    write_png_rgba(
                        rel,
                        (ox1 - ox0 + 1) * BLOCK_PIXELS,
                        (oy1 - oy0 + 1) * BLOCK_PIXELS,
                        overlay,
                    )
                    files.append(
                        {
                            "file": f"map{map_index:02}/{name}",
                            "sha256": _sha256(rel.read_bytes()),
                            "sizeBytes": rel.stat().st_size,
                            "map": map_index,
                            "area": area.index,
                            "palette": map_data.palette_index,
                            "layer": "overlay",
                            "overlayDelta": list(delta),
                            "widthBlocks": ox1 - ox0 + 1,
                            "heightBlocks": oy1 - oy0 + 1,
                        }
                    )
                    composed = composite_overlay(
                        pixels,
                        overlay,
                        width=width_blocks * BLOCK_PIXELS,
                        height=height_blocks * BLOCK_PIXELS,
                        overlay_width=(ox1 - ox0 + 1) * BLOCK_PIXELS,
                        offset_x=(ox0 - delta[0] - area.main_start[0]) * BLOCK_PIXELS,
                        offset_y=(oy0 - delta[1] - area.main_start[1]) * BLOCK_PIXELS,
                    )
                    name = f"map{map_index:02}-area{area.index}-composed.png"
                    rel = map_dir / name
                    write_png_rgba(
                        rel, width_blocks * BLOCK_PIXELS, height_blocks * BLOCK_PIXELS, composed
                    )
                    files.append(
                        {
                            "file": f"map{map_index:02}/{name}",
                            "sha256": _sha256(rel.read_bytes()),
                            "sizeBytes": rel.stat().st_size,
                            "map": map_index,
                            "area": area.index,
                            "palette": map_data.palette_index,
                            "layer": "main+overlay",
                            "overlayDelta": list(delta),
                            "widthBlocks": width_blocks,
                            "heightBlocks": height_blocks,
                        }
                    )
    composed_count = sum(1 for row in files if row["layer"] == "main+overlay")
    overlay_count = sum(1 for row in files if row["layer"] == "overlay")
    manifest = {
        "schemaVersion": 1,
        "tool": "sf2 texture map",
        "romId": expected_rom["id"],
        "romSha256": expected_rom["hashes"]["sha256"],
        "upstream": {"repository": "ShiningForceCentral/SF2DISASM", "commit": commit},
        "notes": [
            "main layer, per-area overlay layer, and composed main+overlay renders; the "
            "overlay passes apply the map's roof/layer-2 copy records (slbc source "
            "regions are copied to their display positions, e.g. the map-3 cell bars); "
            "tile priority is ignored in static rendering; the second/background layer "
            "palette source is not yet evidenced, rendering uses the map palette",
        ],
        "summary": {
            "areaRenderCount": sum(1 for row in files if row["layer"] == "main"),
            "overlayRenderCount": overlay_count,
            "composedRenderCount": composed_count,
            "totalRenderCount": len(files),
        },
        "files": files,
    }
    manifest_path = maps_dir / "map-manifest.json"
    manifest_json = json.dumps(manifest, ensure_ascii=False, indent=2) + "\n"
    manifest_path.write_text(manifest_json, encoding="utf-8")
    return {
        "Maps": len(wanted),
        "Areas": len(files) - composed_count - overlay_count,
        "Overlays": overlay_count,
        "Composed": composed_count,
        "Output": str(maps_dir),
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
    """Render the base/diamond-menu/yes-no tile sets and the main-menu icons.

    The six diamond-menu tile sets (2,304 bytes each) hold 24 strips of 24x8 pixels
    (96 bytes = three 8x8 tiles side by side), so they are rendered as 24-wide strips
    rather than as bare 8x8 tile grids.
    """
    palette = _load_base_palette(disasm)
    out_dir.mkdir(parents=True, exist_ok=True)
    files = []
    for symbol, relative_path, expected, family in UI_RESOURCE_SPECS:
        data = (disasm / relative_path).read_bytes()
        decoded = decode_stack_compressed(data, expected_output_bytes=expected)
        if family == "diamond-menu":
            strip_bytes = 3 * TILE_BYTES_4BPP
            strip_count = len(decoded.output) // strip_bytes
            columns = 6
            rows = (strip_count + columns - 1) // columns
            width = columns * 24
            height = rows * 8
            pixels = [0] * (width * height * 4)
            for strip_index in range(strip_count):
                origin_x = (strip_index % columns) * 24
                origin_y = (strip_index // columns) * 8
                for tile_index in range(3):
                    start = strip_index * strip_bytes + tile_index * TILE_BYTES_4BPP
                    tile = decode_md_4bpp_tile(decoded.output[start : start + TILE_BYTES_4BPP])
                    for row in range(TILE_SIZE):
                        for col in range(TILE_SIZE):
                            color = palette[tile[row * TILE_SIZE + col]]
                            offset = (
                                (origin_y + row) * width + (origin_x + tile_index * 8 + col)
                            ) * 4
                            if color == (0, 0, 0):
                                pixels[offset : offset + 4] = [0, 0, 0, 0]
                            else:
                                pixels[offset : offset + 4] = [*color, 255]
        else:
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


# ---------------------------------------------------------------------------
# UI window composition (frame layout + icon slots)
# ---------------------------------------------------------------------------

UI_VDP_ENUM_PATH = Path("enums/vdp.asm")
UI_LAYOUT_SOURCE_ROOT = Path("data/graphics/tech/menus")
UI_BASE_TILES_PATH = Path("data/graphics/tech/basetiles.bin")
UI_ITEM_MENU_TILES_PATH = Path("data/graphics/tech/menus/itemmenutiles.bin")
UI_ICON_HIGHLIGHT_PATH = Path("data/graphics/tech/iconhighlighttiles.bin")
UI_DIAMOND_BORDER_PATH = Path("data/graphics/tech/menus/diamondmenubordertiles.asm")
UI_MENU_TILE_BASE = 0x5C0
UI_MENU_TILE_COUNT = 64
UI_LAYOUT_FLAG_VALUES = {
    "PALETTE2": 0x2000,
    "PALETTE3": 0x4000,
    "PALETTE4": 0x6000,
    "PRIORITY": 0x8000,
    "MIRROR": 0x800,
    "FLIP": 0x1000,
}

# Battlefield item menu icon slots (itemmenu.asm): MENUTILE ranges and DMA lengths.
ITEM_MENU_SLOTS = {
    "up": {"first": 0x5C0, "count": 6},
    "left": {"first": 0x5C6, "count": 8},
    "down": {"first": 0x5CE, "count": 6},
    "right": {"first": 0x5D4, "count": 8},
}


def parse_vdptile_enums(source: str) -> dict[str, int]:
    """Parse ``VDPTILE_NAME: equ $XXXX`` lines from ``enums/vdp.asm``."""
    enums = {}
    for name, value in re.findall(
        r"VDPTILE_([A-Z0-9_]+):\s*equ\s*\$([0-9A-F]+)", source, re.IGNORECASE
    ):
        enums[name] = int(value, 16)
    return enums


def parse_window_layout(
    source: str, enums: dict[str, int], *, width: int = 18
) -> list[list[int]]:
    """Parse a ``vdpTile`` layout grid (one tile per line, rows in source order)."""
    rows: list[list[int]] = []
    row: list[int] = []
    for line in source.splitlines():
        match = re.match(r"\s*vdpTile\s*(.*)$", line)
        if match is None:
            continue
        tokens = [
            token.strip()
            for token in match.group(1).split("|")
            if token.strip() and not token.strip().startswith(";")
        ]
        word = 0
        for token in tokens:
            if token in enums:
                word |= enums[token]
            elif token in UI_LAYOUT_FLAG_VALUES:
                word |= UI_LAYOUT_FLAG_VALUES[token]
            else:
                raise ValueError(f"unknown vdpTile token: {token}")
        row.append(word)
        if len(row) == width:
            rows.append(row)
            row = []
    if row:
        raise ValueError(f"incomplete window layout row: {len(row)} of {width} tiles")
    if not rows:
        raise ValueError("window layout has no rows")
    return rows


def parse_dc_b_tiles(source: str) -> bytes:
    """Parse ``dc.b`` byte lists (used by the diamond-menu border assets)."""
    values = []
    for line in source.splitlines():
        match = re.match(r"\s*dc\.b\s+(.+)$", line)
        if match is None:
            continue
        for token in match.group(1).split(","):
            token = token.strip()
            if not token:
                continue
            if token.startswith("$"):
                values.append(int(token[1:], 16))
            else:
                values.append(int(token))
    return bytes(values)


def build_bordered_icon(icon: bytes, border_top: bytes, border_bottom: bytes) -> bytes:
    """Replicate the battlefield item menu's left/right bordered icon buffer.

    Mirrors `itemmenu.asm` ``sub_10874``/``sub_108CA``: the border tiles' first
    longwords land at offsets ``0``/``0x20`` and the icon tile rows are spread with
    their 16-byte halves at ``0`` and ``0x30`` per 0x20-stride row; the bottom border
    is written after the spread. The result is DMA'd as eight contiguous tiles.
    """
    if len(icon) != ICON_BYTES:
        raise ValueError(f"icon must be {ICON_BYTES} bytes, got {len(icon)}")
    if len(border_top) != 48 or len(border_bottom) != 48:
        raise ValueError("diamond border tiles must be 48 bytes")
    buf = bytearray(256)
    for index in range(4):
        start = index * 4
        buf[start : start + 4] = border_top[start : start + 4]
        buf[0x20 + start : 0x24 + start] = border_top[0x20 + start : 0x24 + start]
    for row in range(6):
        base = row * 0x20
        buf[base : base + 16] = icon[row * 32 : row * 32 + 16]
        buf[base + 0x30 : base + 0x40] = icon[row * 32 + 16 : row * 32 + 32]
    for index in range(4):
        start = index * 4
        buf[0xC0 + start : 0xC4 + start] = border_bottom[start : start + 4]
        buf[0xE0 + start : 0xE4 + start] = border_bottom[0x20 + start : 0x24 + start]
    return bytes(buf)


def extract_ui_windows(
    rom_path: Path,
    upstream_path: Path,
    *,
    out_dir: Path,
) -> dict[str, object]:
    """Render the composed battle action menu (diamond menu) window.

    The action menu is `ExecuteDiamondMenu` with `MENU_BATTLE_WITH_STAY`: an 18x6
    window whose frame comes from `layout_DiamondMenu` (VDP attribute words
    referencing `tiles_Base` frame tiles and `tiles_ItemMenu` MENUTILE tiles). The
    four diamond slots are filled like the game's DMA path: up/down take the plain
    24x24 half of the main-menu icon (indices 0/3 = ATTACK/STAY for the battle menu),
    left/right take the bordered 24x32 build of `diamondmenu.asm`
    ``sub_10484``/``sub_104E6`` (main-menu icons 1/2 wrapped with
    `tiles_DiamondMenuBorder1-4`), and the selected option's name ("ATTACK") is
    written at (11,4) with the base-tiles font like `WriteTilesFromAsciiWithRegularFont`.
    """
    rom_path = rom_path.resolve(strict=True)
    upstream_path = upstream_path.resolve(strict=True)
    disasm, commit, _toolchain = _resolve_upstream(upstream_path)
    expected_rom = load_json(repo_path("manifests/roms/sf2-us.json"))
    rom = rom_path.read_bytes()
    if len(rom) != expected_rom["sizeBytes"] or _sha256(rom) != expected_rom["hashes"]["sha256"]:
        raise ValueError("ui window render ROM identity drift")

    palette = _load_base_palette(disasm)
    base_tiles = decode_stack_compressed(
        (disasm / UI_BASE_TILES_PATH).read_bytes(), expected_output_bytes=8192
    ).output
    menu_tiles = decode_stack_compressed(
        (disasm / UI_ITEM_MENU_TILES_PATH).read_bytes(), expected_output_bytes=2304
    ).output
    enums = parse_vdptile_enums(read_upstream_text(disasm / UI_VDP_ENUM_PATH))
    layout = parse_window_layout(
        read_upstream_text(disasm / UI_LAYOUT_SOURCE_ROOT / "diamondmenulayout.asm"), enums
    )

    main_menu = (disasm / MAIN_MENU_PATH).read_bytes()
    if len(main_menu) % MAIN_MENU_ICON_BYTES:
        raise ValueError(f"main menu payload size drift: {len(main_menu)}")
    main_icons = [
        main_menu[offset : offset + MAIN_MENU_ICON_BYTES]
        for offset in range(0, len(main_menu), MAIN_MENU_ICON_BYTES)
    ]
    if len(main_icons) < 4:
        raise ValueError(f"main menu icon count drift: {len(main_icons)}")

    border_source = read_upstream_text(disasm / UI_DIAMOND_BORDER_PATH)
    border_parts = border_source.split("tiles_DiamondMenuBorder")
    borders = {
        "1": parse_dc_b_tiles(border_parts[1].split(":")[1]),
        "2": parse_dc_b_tiles(border_parts[2].split(":")[1]),
        "3": parse_dc_b_tiles(border_parts[3].split(":")[1]),
        "4": parse_dc_b_tiles(border_parts[4].split(":")[1]),
    }

    def render_window(slot_tiles: dict[int, bytes], text: str = "") -> tuple[list[int], int, int]:
        width = len(layout[0]) * TILE_SIZE
        height = len(layout) * TILE_SIZE
        pixels = [0] * (width * height * 4)
        for row_index, row in enumerate(layout):
            for col_index, word in enumerate(row):
                tile_number = word & 0x7FF
                if tile_number == 0:
                    # blank word: the game's tile 0 shows the dark blue interior
                    for row in range(TILE_SIZE):
                        for col in range(TILE_SIZE):
                            offset = (
                                (row_index * TILE_SIZE + row) * width
                                + (col_index * TILE_SIZE + col)
                            ) * 4
                            pixels[offset : offset + 4] = [0, 36, 146, 255]
                    continue
                if tile_number in slot_tiles:
                    tile = decode_md_4bpp_tile(slot_tiles[tile_number])
                elif tile_number >= UI_MENU_TILE_BASE:
                    local = tile_number - UI_MENU_TILE_BASE
                    if local * TILE_BYTES_4BPP + TILE_BYTES_4BPP > len(menu_tiles):
                        raise ValueError(f"menu tile out of range: {tile_number:04X}")
                    tile = decode_md_4bpp_tile(
                        menu_tiles[local * TILE_BYTES_4BPP : (local + 1) * TILE_BYTES_4BPP]
                    )
                else:
                    local = tile_number
                    if local * TILE_BYTES_4BPP + TILE_BYTES_4BPP > len(base_tiles):
                        raise ValueError(f"base tile out of range: {tile_number:04X}")
                    tile = decode_md_4bpp_tile(
                        base_tiles[local * TILE_BYTES_4BPP : (local + 1) * TILE_BYTES_4BPP]
                    )
                tile = transform_tile_pixels(tile, word)
                origin_x = col_index * TILE_SIZE
                origin_y = row_index * TILE_SIZE
                for row in range(TILE_SIZE):
                    for col in range(TILE_SIZE):
                        color = palette[tile[row * TILE_SIZE + col]]
                        offset = ((origin_y + row) * width + (origin_x + col)) * 4
                        if color == (0, 0, 0):
                            pixels[offset : offset + 4] = [0, 0, 0, 0]
                        else:
                            pixels[offset : offset + 4] = [*color, 255]
        if text:
            # WriteTilesFromAsciiWithRegularFont: the base-tiles glyphs sit one tile
            # below the ASCII value ('A' 0x41 -> tile 0x40, 'T' 0x54 -> 0x53, ...)
            origin_x = 11 * TILE_SIZE
            origin_y = 4 * TILE_SIZE
            for char_index, char in enumerate(text):
                tile_number = ord(char) - 1
                if tile_number >= 0x60:
                    tile_number += 32
                local = tile_number
                if local * TILE_BYTES_4BPP + TILE_BYTES_4BPP > len(base_tiles):
                    raise ValueError(f"text glyph tile out of range: {tile_number:02X}")
                tile = decode_md_4bpp_tile(
                    base_tiles[local * TILE_BYTES_4BPP : (local + 1) * TILE_BYTES_4BPP]
                )
                for row in range(TILE_SIZE):
                    for col in range(TILE_SIZE):
                        color = palette[tile[row * TILE_SIZE + col]]
                        if color == (0, 0, 0):
                            continue
                        offset = (
                            (origin_y + row) * width + (origin_x + char_index * 8 + col)
                        ) * 4
                        pixels[offset : offset + 4] = [*color, 255]
        return pixels, width, height

    def build_bordered_main_icon(icon: bytes, border_top: bytes, border_bottom: bytes) -> bytes:
        """Replicate the diamond menu's left/right bordered slot buffer.

        Mirrors `diamondmenu.asm` ``sub_10484``/``sub_104E6``: the border asset's three
        16-byte groups land at offsets ``0``/``0x20``/``0x40`` and the icon's 288-byte
        plain half is spread with 16-byte pieces at ``0`` and ``0x50`` per 0x20-stride
        row (9 rows); the bottom border follows at the end. DMA'd as twelve tiles.
        """
        if len(icon) != 0x120:
            raise ValueError(f"main menu icon half must be 288 bytes, got {len(icon)}")
        buf = bytearray(0x180)
        for index in range(4):
            start = index * 4
            buf[start : start + 4] = border_top[start : start + 4]
            buf[0x20 + start : 0x24 + start] = border_top[0x10 + start : 0x14 + start]
            buf[0x40 + start : 0x44 + start] = border_top[0x20 + start : 0x24 + start]
        for row in range(9):
            base = 0x10 + row * 0x20
            buf[base : base + 16] = icon[row * 32 : row * 32 + 16]
            buf[base + 0x50 : base + 0x60] = icon[row * 32 + 16 : row * 32 + 32]
        for index in range(4):
            start = index * 4
            buf[0x130 + start : 0x134 + start] = border_bottom[start : start + 4]
            buf[0x150 + start : 0x154 + start] = border_bottom[0x10 + start : 0x14 + start]
            buf[0x170 + start : 0x174 + start] = border_bottom[0x20 + start : 0x24 + start]
        return bytes(buf)

    def slot_overrides(highlight_up: bool) -> dict[int, bytes]:
        overrides = {}
        for index in range(9):
            overrides[0x5C0 + index] = main_icons[0][index * 32 : (index + 1) * 32]
        for index in range(12):
            payload = build_bordered_main_icon(main_icons[1][:0x120], borders["1"], borders["2"])
            overrides[0x5C9 + index] = payload[index * 32 : (index + 1) * 32]
        for index in range(9):
            overrides[0x5E1 + index] = main_icons[3][index * 32 : (index + 1) * 32]
        for index in range(12):
            payload = build_bordered_main_icon(main_icons[2][:0x120], borders["3"], borders["4"])
            overrides[0x5D5 + index] = payload[index * 32 : (index + 1) * 32]
        if highlight_up:
            for index in range(9):
                start = 0x120 + index * TILE_BYTES_4BPP
                overrides[0x5C0 + index] = main_icons[0][start : start + TILE_BYTES_4BPP]
        return overrides

    out_dir = out_dir.resolve()
    window_dir = out_dir / "windows"
    window_dir.mkdir(parents=True, exist_ok=True)
    files = []
    variants = [
        ("actionmenu-frame.png", {}, ""),
        ("actionmenu.png", slot_overrides(highlight_up=False), "ATTACK"),
        ("actionmenu-selected.png", slot_overrides(highlight_up=True), "ATTACK"),
    ]
    for name, overrides, text in variants:
        pixels, width, height = render_window(overrides, text)
        rel = window_dir / name
        write_png_rgba(rel, width, height, pixels)
        files.append(
            {"file": name, "sha256": _sha256(rel.read_bytes()), "sizeBytes": rel.stat().st_size}
        )

    manifest = {
        "schemaVersion": 1,
        "tool": "sf2 texture ui",
        "romId": expected_rom["id"],
        "romSha256": expected_rom["hashes"]["sha256"],
        "upstream": {"repository": "ShiningForceCentral/SF2DISASM", "commit": commit},
        "notes": [
            "battle action menu (ExecuteDiamondMenu, MENU_BATTLE_WITH_STAY): "
            "layout_DiamondMenu frame with the four diamond slots filled per the DMA "
            "path (up/down = main-menu icon halves, left/right = bordered builds of "
            "diamondmenu.asm sub_10484/sub_104E6) and the selected option name written "
            "at (11,4) with the base-tiles font",
        ],
        "summary": {"windowRenderCount": len(files)},
        "files": files,
    }
    manifest_path = window_dir / "windows-manifest.json"
    manifest_json = json.dumps(manifest, ensure_ascii=False, indent=2) + "\n"
    manifest_path.write_text(manifest_json, encoding="utf-8")
    return {
        "Windows": len(files),
        "Output": str(window_dir),
        "Manifest": _sha256(manifest_path.read_bytes()),
        "Status": "PASS",
    }


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
