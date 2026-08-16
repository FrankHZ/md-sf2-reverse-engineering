"""Synthetic unit tests for the map-texture extraction rail (no original payloads)."""

from __future__ import annotations

import struct
import zlib

import pytest

from sf2tool.texture_extract import (
    TILE_BYTES_4BPP,
    TILES_PER_TILESET,
    decode_md_4bpp_tile,
    md_palette_color,
    render_palette_strip,
    render_tileset_sheet,
    write_png_rgba,
)


def test_md_palette_color_decode():
    assert md_palette_color(0x0E00) == (0, 0, 255)
    assert md_palette_color(0x0E0) == (0, 255, 0)
    assert md_palette_color(0x0E) == (255, 0, 0)
    assert md_palette_color(0x0EEE) == (255, 255, 255)
    assert md_palette_color(0x480) == (0, 146, 73)
    with pytest.raises(ValueError, match="0x0EEE"):
        md_palette_color(0xFFFF)


def test_decode_md_4bpp_tile():
    tile = bytearray(TILE_BYTES_4BPP)
    # row 0, byte 0: high nibble = pixel 0, low nibble = pixel 1
    tile[0] = 0xF1
    pixels = decode_md_4bpp_tile(bytes(tile))
    assert len(pixels) == 64
    assert pixels[0] == 15
    assert pixels[1] == 1
    assert pixels[2] == 0
    assert pixels[8 * 7] == 0
    # row 1, byte 4: pixels 8/9
    tile[4] = 0x23
    pixels = decode_md_4bpp_tile(bytes(tile))
    assert pixels[8] == 2
    assert pixels[9] == 3


def test_render_tileset_sheet():
    palette = [(0, 0, 0)] + [(v, v, v) for v in range(15)] * 1
    palette = [(0, 0, 0), (255, 0, 0), (0, 255, 0), (0, 0, 255)] + [(0, 0, 0)] * 12
    tile = bytearray(TILE_BYTES_4BPP)
    tile[0] = 0x12  # pixel 0 -> index 1 (red), pixel 1 -> index 2 (green)
    tiles = [decode_md_4bpp_tile(bytes(tile))] + [
        [0] * 64 for _ in range(TILES_PER_TILESET - 1)
    ]
    pixels = render_tileset_sheet(tiles, palette)
    assert len(pixels) == 128 * 64 * 4
    # first tile top-left pixel red, next pixel green
    assert pixels[0:4] == [255, 0, 0, 255]
    assert pixels[4:8] == [0, 255, 0, 255]
    # color 0 is transparent
    assert pixels[8:12] == [0, 0, 0, 0]


def test_render_palette_strip():
    palette = [(0, 0, 0), (255, 0, 0)] + [(0, 0, 0)] * 14
    pixels = render_palette_strip(palette)
    assert len(pixels) == 16 * 16 * 16 * 4
    assert pixels[0:4] == [0, 0, 0, 0]
    assert pixels[16 * 16 * 4 : 16 * 16 * 4 + 4] == [255, 0, 0, 255]


def _read_png(path):
    data = path.read_bytes()
    assert data[:8] == b"\x89PNG\r\n\x1a\n"
    offset = 8
    chunks = {}
    while offset < len(data):
        length = int.from_bytes(data[offset : offset + 4], "big")
        tag = data[offset + 4 : offset + 8]
        chunks[tag] = data[offset + 8 : offset + 8 + length]
        offset += 12 + length
    width, height, depth, color_type = struct.unpack(">IIBB", chunks[b"IHDR"][:10])
    raw = zlib.decompress(chunks[b"IDAT"])
    return width, height, depth, color_type, raw


def test_write_png_rgba_roundtrip(tmp_path):
    out = tmp_path / "t.png"
    pixels = [0, 0, 0, 0] * 64 + [255, 0, 0, 255] * 64
    write_png_rgba(out, 8, 16, pixels)
    width, height, depth, color_type, raw = _read_png(out)
    assert (width, height, depth, color_type) == (8, 16, 8, 6)
    assert raw[0] == 0  # first scanline filter
    assert raw[1:5] == b"\x00\x00\x00\x00"
    assert raw[8 * 33 + 1 : 8 * 33 + 5] == b"\xff\x00\x00\xff"


def test_write_png_rgba_size_drift(tmp_path):
    with pytest.raises(ValueError, match="pixel buffer size drift"):
        write_png_rgba(tmp_path / "t.png", 8, 8, [0] * 10)


def test_transform_tile_pixels_flags():
    from sf2tool.texture_extract import transform_tile_pixels

    tile = [row * 8 + col for row in range(8) for col in range(8)]
    mirrored = transform_tile_pixels(tile, 0x800)
    assert mirrored[0] == 7
    assert mirrored[1] == 6
    assert mirrored[8] == 15
    flipped = transform_tile_pixels(tile, 0x1000)
    assert flipped[0] == 56
    assert flipped[1] == 57
    both = transform_tile_pixels(tile, 0x1800)
    assert both[0] == 63
    plain = transform_tile_pixels(tile, 0x8000)
    assert plain == tile


def test_render_map_blocks_synthetic():
    from sf2tool.texture_extract import (
        BLOCK_PIXELS,
        TILE_INDEX_OFFSET,
        decode_md_4bpp_tile,
        render_map_blocks,
    )

    palette = [(0, 0, 0), (255, 0, 0)] + [(0, 0, 0)] * 14
    tile = bytearray(32)
    tile[0] = 0x11
    pool = [[decode_md_4bpp_tile(bytes(tile))] + [[0] * 64 for _ in range(127)]]
    blocks = [0] * 9
    blocks[0] = TILE_INDEX_OFFSET
    blocks[1] = 0x8000  # priority flag without a tile reference -> transparent
    layout = [0] * (64 * 64)
    pixels = render_map_blocks(
        layout,
        blocks,
        pool,
        palette,
        origin=(0, 0),
        width_blocks=1,
        height_blocks=1,
    )
    assert len(pixels) == BLOCK_PIXELS * BLOCK_PIXELS * 4
    # tile 0 pixel (0,0) = index 1 -> red
    assert pixels[0:4] == [255, 0, 0, 255]
    # tile 0 pixel (0,1) = index 1 -> red (0x11 high/low nibble)
    assert pixels[4:8] == [255, 0, 0, 255]
    # tile 1 has priority flag only; pixel (0,8) -> transparent (all index 0)
    assert pixels[8 * 4 : 8 * 4 + 4] == [0, 0, 0, 0]


def test_md_cram_color_decode():
    from sf2tool.texture_extract import md_cram_color

    assert md_cram_color(0x000F) == (0, 0, 60)
    assert md_cram_color(0x03E0) == (0, 248, 0)
    assert md_cram_color(0xF800) == (248, 0, 0)
    assert md_cram_color(0x0) == (0, 0, 0)


def test_font_bit_order():
    from sf2tool.texture_extract import FONT_GLYPH_COLUMNS

    # a row with left=0x00, right=0x10 -> (0<<4)|(0x10>>4) = 1 -> bit 11-? = column 11
    bits = (0x00 << 4) | (0x10 >> 4)
    columns = [c for c in range(FONT_GLYPH_COLUMNS) if bits & (1 << (11 - c))]
    assert columns == [11]
