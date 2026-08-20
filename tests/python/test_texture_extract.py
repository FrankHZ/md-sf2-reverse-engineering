"""Synthetic unit tests for the map-texture extraction rail (no original payloads)."""

from __future__ import annotations

import hashlib
import struct
import zlib
from pathlib import Path
from types import SimpleNamespace

import pytest

from sf2tool.cli import build_parser
from sf2tool.texture_extract import (
    TILE_BYTES_4BPP,
    TILES_PER_TILESET,
    ProvenanceBoundSource,
    TextureExtractionOptions,
    _require_fresh_outputs,
    decode_md_4bpp_tile,
    extract_map_textures,
    extract_unused_assets,
    md_palette_color,
    parse_map_selection,
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
    tile[1] = 0x80  # pixel 2 -> nonzero black, pixel 3 -> transparent index 0
    tiles = [decode_md_4bpp_tile(bytes(tile))] + [[0] * 64 for _ in range(TILES_PER_TILESET - 1)]
    pixels = render_tileset_sheet(tiles, palette)
    assert len(pixels) == 128 * 64 * 4
    # first tile top-left pixel red, next pixel green
    assert pixels[0:4] == [255, 0, 0, 255]
    assert pixels[4:8] == [0, 255, 0, 255]
    assert pixels[8:12] == [0, 0, 0, 255]
    assert pixels[12:16] == [0, 0, 0, 0]


def test_render_palette_strip():
    palette = [(0, 0, 0), (255, 0, 0)] + [(0, 0, 0)] * 14
    pixels = render_palette_strip(palette)
    assert len(pixels) == 16 * 16 * 16 * 4
    assert pixels[0:4] == [0, 0, 0, 0]
    assert pixels[16 * 4 : 16 * 4 + 4] == [255, 0, 0, 255]
    black_index_8 = 8 * 16 * 4
    assert pixels[black_index_8 : black_index_8 + 4] == [0, 0, 0, 255]


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


def test_render_map_overlay_preserves_nonzero_black():
    from sf2tool.texture_extract import (
        TILE_INDEX_OFFSET,
        decode_md_4bpp_tile,
        render_map_blocks,
    )

    palette = [(0, 0, 0)] + [(255, 0, 0)] * 7 + [(0, 0, 0)] + [(0, 0, 0)] * 7
    tile = bytearray(32)
    tile[0] = 0x80
    pool = [[decode_md_4bpp_tile(bytes(tile))] + [[0] * 64 for _ in range(127)]]
    blocks = [TILE_INDEX_OFFSET] + [0] * 8
    pixels = render_map_blocks(
        [0] * (64 * 64),
        blocks,
        pool,
        palette,
        origin=(0, 0),
        width_blocks=1,
        height_blocks=1,
        transparent_first=True,
    )
    assert pixels[0:4] == [0, 0, 0, 255]
    assert pixels[4:8] == [0, 0, 0, 0]


def test_md_cram_color_decode():
    from sf2tool.texture_extract import md_cram_color

    # candidate 6/5/5 layout: blue bits 0-5, green bits 6-10, red bits 11-15
    assert md_cram_color(0x000F) == (0, 0, 60)
    assert md_cram_color(0x03E0) == (0, 123, 130)  # bit 5 belongs to blue
    assert md_cram_color(0x07C0) == (0, 255, 0)
    assert md_cram_color(0xF800) == (255, 0, 0)
    assert md_cram_color(0x0) == (0, 0, 0)


def test_font_bit_order():
    from sf2tool.texture_extract import FONT_GLYPH_COLUMNS

    # a row with left=0x00, right=0x10 -> (0<<4)|(0x10>>4) = 1 -> bit 11-? = column 11
    bits = (0x00 << 4) | (0x10 >> 4)
    columns = [c for c in range(FONT_GLYPH_COLUMNS) if bits & (1 << (11 - c))]
    assert columns == [11]


def test_area_overlay_delta():
    from sf2tool.texture_extract import MapArea, area_overlay_delta

    area = MapArea(
        index=0,
        main_start=(0, 0),
        main_end=(50, 31),
        foreground_start=(0, 32),
        background_start=(0, 0),
    )
    assert area_overlay_delta(area) == (0, 32)
    in_place = MapArea(
        index=1,
        main_start=(51, 0),
        main_end=(61, 9),
        foreground_start=(0, 0),
        background_start=(0, 0),
    )
    assert area_overlay_delta(in_place) == (0, 0)


def test_clip_block_rect():
    from sf2tool.texture_extract import clip_block_rect

    assert clip_block_rect(0, 32, 50, 63) == (0, 32, 50, 63)
    assert clip_block_rect(-32, 0, -1, 31) is None
    assert clip_block_rect(-5, 0, 10, 31) == (0, 0, 10, 31)
    assert clip_block_rect(0, 0, 70, 70) == (0, 0, 63, 63)


def test_parse_layer2_copies(tmp_path):
    from sf2tool.texture_extract import parse_layer2_copies

    entry = tmp_path / "data" / "maps" / "entries" / "map03"
    entry.mkdir(parents=True)
    (entry / "5-roof-events.asm").write_text(
        "                slbc 4, 8  ; door\n"
        "                  slbcSource 255, 255\n"
        "                  slbcSize   7, 8\n"
        "                  slbcDest   2, 32\n"
        "                slbc 24, 26  ; creature building opening\n"
        "                  slbcSource 51, 20\n"
        "                  slbcSize   9, 7\n"
        "                  slbcDest   22, 51\n"
        "                endWord\n",
        encoding="utf-8",
    )
    records = parse_layer2_copies(tmp_path, 3)
    assert records == [
        ((4, 8), (255, 255), (7, 8), (2, 32)),
        ((24, 26), (51, 20), (9, 7), (22, 51)),
    ]


def test_apply_layer2_copies():
    from sf2tool.texture_extract import apply_layer2_copies

    layout = list(range(64 * 64))
    copies = [
        ((0, 0), (51, 20), (2, 1), (22, 51)),
        ((0, 0), (255, 255), (2, 1), (0, 0)),
    ]
    working = apply_layer2_copies(layout, copies)
    # source (51,20) -> dest (22,51): word at dest equals word at source
    assert working[51 * 64 + 22] == layout[20 * 64 + 51]
    assert working[51 * 64 + 23] == layout[20 * 64 + 52]
    # the clear record is skipped
    assert working[0] == layout[0]
    # untouched elsewhere
    assert working[30 * 64 + 30] == layout[30 * 64 + 30]


def test_composite_overlay():
    from sf2tool.texture_extract import composite_overlay

    main = [0, 0, 0, 0, 255, 0, 0, 255, 0, 0, 0, 0]
    overlay = [255, 255, 255, 255, 0, 0, 0, 0, 0, 0, 0, 0]
    composed = composite_overlay(
        main, overlay, width=3, height=1, overlay_width=3, offset_x=0, offset_y=0
    )
    assert composed == [255, 255, 255, 255, 255, 0, 0, 255, 0, 0, 0, 0]
    placed = composite_overlay(
        main, [0, 0, 0, 0, 0, 0, 0, 0], width=3, height=1, overlay_width=1, offset_x=0, offset_y=0
    )
    assert placed == main
    with pytest.raises(ValueError, match="main pixel buffer size drift"):
        composite_overlay(main, [0] * 4, width=2, height=2, overlay_width=1, offset_x=0, offset_y=0)


def test_provenance_bound_source_rejects_mutated_ignored_payload(tmp_path):
    disasm = tmp_path / "disasm"
    payload_path = disasm / "data" / "asset.bin"
    payload_path.parent.mkdir(parents=True)
    payload_path.write_bytes(b"mutated")
    source = ProvenanceBoundSource(disasm, b"original", {"Asset": 0})
    with pytest.raises(ValueError, match="source/ROM provenance drift"):
        source.read("data/asset.bin", symbol="Asset")


def test_map_selection_rejects_duplicate_and_empty_ids():
    assert parse_map_selection("3, 4") == [3, 4]
    with pytest.raises(ValueError, match="duplicate map IDs"):
        parse_map_selection("3,3")
    with pytest.raises(ValueError, match="empty map ID"):
        parse_map_selection("3,")


def test_tileset_palette_validation_precedes_private_inputs(tmp_path):
    with pytest.raises(ValueError, match="tileset palette must be in 0..15"):
        extract_map_textures(
            Path("missing-rom"),
            Path("missing-upstream"),
            out_dir=tmp_path,
            options=TextureExtractionOptions(tileset_palette=16),
        )


def test_tileset_palette_cli_rejects_values_outside_0_to_15():
    parser = build_parser()
    valid = parser.parse_args(["texture", "extract", "--tileset-palette", "15"])
    assert valid.tileset_palette == 15
    with pytest.raises(SystemExit):
        parser.parse_args(["texture", "extract", "--tileset-palette", "16"])
    with pytest.raises(SystemExit):
        parser.parse_args(["texture", "extract", "--tileset-palette", "-1"])


def test_texture_ui_cli_uses_private_default_output():
    args = build_parser().parse_args(["texture", "ui"])
    assert args.texture_command == "ui"
    assert args.out_dir == Path("local/derived/graphics").resolve()


def test_output_collision_fails_closed(tmp_path):
    (tmp_path / "manifest.json").write_text("stale", encoding="utf-8")
    with pytest.raises(ValueError, match="texture output collision"):
        _require_fresh_outputs(tmp_path, ("manifest.json", "tilesets"))
    (tmp_path / "maps" / "map04").mkdir(parents=True)
    with pytest.raises(ValueError, match="texture output collision"):
        _require_fresh_outputs(tmp_path, ("maps",))


def test_extract_unused_assets_renders_all_four_streams(tmp_path, monkeypatch):
    from sf2tool import texture_extract

    cloud = bytes((1, 2, 3, 4))
    palette = bytes(64)
    disasm = tmp_path / "disasm"
    cloud_path = disasm / "data" / "graphics" / "tech" / "unusedcloudtiles.bin"
    palette_path = disasm / "data" / "graphics" / "tech" / "unusedbasepalettes.bin"
    cloud_path.parent.mkdir(parents=True)
    cloud_path.write_bytes(cloud)
    palette_path.write_bytes(palette)
    source = ProvenanceBoundSource(
        disasm,
        cloud + palette,
        {"tiles_UnusedCloud": 0, "palette_UnusedBase": len(cloud)},
    )
    stream_facts = []
    decoded_by_stream = {}
    for index, value in enumerate(cloud):
        decoded = bytes([value]) * 32
        decoded_by_stream[value] = decoded
        stream_facts.append(
            {
                "index": index,
                "startOffset": index,
                "endOffsetExclusive": index + 1,
                "storedByteCount": 1,
                "sourceSha256": hashlib.sha256(bytes([value])).hexdigest().upper(),
                "decodedByteCount": len(decoded),
                "decodedSha256": hashlib.sha256(decoded).hexdigest().upper(),
            }
        )
    owner = {
        "id": "sf2-unused-technical-assets-static-v1",
        "cloudFacts": {"sha256": hashlib.sha256(cloud).hexdigest().upper()},
        "paletteFacts": {"sha256": hashlib.sha256(palette).hexdigest().upper()},
        "streamFacts": stream_facts,
    }

    def fake_decode(data, *, expected_output_bytes=None):
        output = decoded_by_stream[data[0]]
        assert len(output) == expected_output_bytes
        return SimpleNamespace(output=output)

    monkeypatch.setattr(texture_extract, "decode_stack_compressed", fake_decode)
    rows = extract_unused_assets(source, tmp_path / "out", owner_fixture=owner)
    stream_rows = [row for row in rows if row["resourceType"] == "cloudStream"]
    palette_rows = [row for row in rows if row["resourceType"] == "paletteStrip"]
    assert len(stream_rows) == 8
    assert {row["streamIndex"] for row in stream_rows} == {0, 1, 2, 3}
    assert all(sum(row["streamIndex"] == index for row in stream_rows) == 2 for index in range(4))
    assert len(palette_rows) == 2
    assert len(list((tmp_path / "out").glob("unusedcloud_stream*_palette*.png"))) == 8


def test_parse_vdptile_enums_and_layout():
    from sf2tool.texture_extract import (
        UI_LAYOUT_FLAG_VALUES,
        parse_vdptile_enums,
        parse_window_layout,
    )

    enums = parse_vdptile_enums(
        "VDPTILE_SPACE: equ $20\nVDPTILE_CORNER: equ $60\nVDPTILE_MENUTILE1: equ $5C0\n"
    )
    assert enums == {"SPACE": 0x20, "CORNER": 0x60, "MENUTILE1": 0x5C0}
    layout = parse_window_layout(
        "                vdpTile \n"
        "                vdpTile MENUTILE1|PALETTE3|PRIORITY\n"
        "                vdpTile CORNER|MIRROR\n"
        "                vdpTile SPACE|FLIP\n",
        enums,
        width=2,
    )
    assert layout == [
        [0x0000, 0x5C0 | UI_LAYOUT_FLAG_VALUES["PALETTE3"] | UI_LAYOUT_FLAG_VALUES["PRIORITY"]],
        [0x60 | UI_LAYOUT_FLAG_VALUES["MIRROR"], 0x20 | UI_LAYOUT_FLAG_VALUES["FLIP"]],
    ]
    with pytest.raises(ValueError, match="incomplete window layout row"):
        parse_window_layout("vdpTile \n", enums, width=2)


def test_menu_layout_owner_rejects_consumer_dimension_drift(tmp_path):
    from sf2tool.h2.ui_layouts import (
        MENU_LAYOUT_CONSUMERS,
        WINDOW_ENGINE_SOURCE,
        _verify_menu_layout_consumer_shapes,
    )

    for layout_symbol, relative_path, entry_symbol in MENU_LAYOUT_CONSUMERS:
        path = tmp_path / relative_path
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(
            f"{entry_symbol}:\n"
            "    move.w  #$1206,d0\n"
            "    jsr     (CreateWindow).w\n"
            f"; End of function {entry_symbol}\n"
            f"Load{layout_symbol}:\n"
            f"    lea     {layout_symbol}(pc), a0\n",
            encoding="utf-8",
        )
    window_path = tmp_path / WINDOW_ENGINE_SOURCE
    window_path.parent.mkdir(parents=True, exist_ok=True)
    window_path.write_text(
        "CreateWindow:\n"
        "    move.w  d0,d7\n"
        "    lsr.w   #BYTE_SHIFT_COUNT,d7 ; get width\n"
        "    andi.w  #BYTE_MASK,d0\n"
        "    mulu.w  d7,d0\n"
        "; End of function CreateWindow\n",
        encoding="utf-8",
    )
    _verify_menu_layout_consumer_shapes(tmp_path)
    first_path = tmp_path / MENU_LAYOUT_CONSUMERS[0][1]
    first_path.write_text(
        first_path.read_text(encoding="utf-8").replace("#$1206", "#$0C09"),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="CreateWindow shape drift"):
        _verify_menu_layout_consumer_shapes(tmp_path)


def test_parse_dc_b_tiles():
    from sf2tool.texture_extract import parse_dc_b_tiles

    data = parse_dc_b_tiles("dc.b 2\n                dc.b $22, 3\n                dc.b $AB\n")
    assert data == bytes([2, 0x22, 3, 0xAB])


def test_extract_ui_windows_binds_sources_preserves_black_and_requires_fresh_output(
    tmp_path, monkeypatch
):
    from sf2tool import texture_extract

    disasm = tmp_path / "disasm"
    payloads = {
        texture_extract.BASE_PALETTE_PATH: bytes(32),
        texture_extract.UI_BASE_TILES_PATH: b"base-compressed",
        texture_extract.MAIN_MENU_PATH: bytes(7 * texture_extract.MAIN_MENU_ICON_BYTES),
    }
    addresses: dict[str, int] = {}
    rom = bytearray()

    def add_payload(path: Path, symbol: str, payload: bytes) -> None:
        target = disasm / path
        target.parent.mkdir(parents=True, exist_ok=True)
        target.write_bytes(payload)
        addresses[symbol] = len(rom)
        rom.extend(payload)

    add_payload(
        texture_extract.BASE_PALETTE_PATH,
        "palette_Base",
        payloads[texture_extract.BASE_PALETTE_PATH],
    )
    add_payload(
        texture_extract.UI_BASE_TILES_PATH,
        "tiles_Base",
        payloads[texture_extract.UI_BASE_TILES_PATH],
    )
    add_payload(
        texture_extract.MAIN_MENU_PATH,
        "tiles_MainMenu",
        payloads[texture_extract.MAIN_MENU_PATH],
    )

    enums_path = disasm / texture_extract.UI_VDP_ENUM_PATH
    enums_path.parent.mkdir(parents=True, exist_ok=True)
    dynamic_words = list(range(0x5C0, 0x5EA))
    enums_path.write_text(
        "VDPTILE_CORNER: equ $1\n"
        + "".join(
            f"VDPTILE_SLOT{index}: equ ${word:X}\n"
            for index, word in enumerate(dynamic_words)
        ),
        encoding="utf-8",
    )
    layout_path = disasm / texture_extract.UI_LAYOUT_SOURCE_ROOT / "diamondmenulayout.asm"
    layout_path.parent.mkdir(parents=True, exist_ok=True)
    layout_words = [*dynamic_words, 1, *([0] * (108 - len(dynamic_words) - 1))]
    layout_path.write_text(
        "".join(f"vdpTile SLOT{index}\n" for index in range(len(dynamic_words)))
        + "vdpTile CORNER\n"
        + "vdpTile\n" * (108 - len(dynamic_words) - 1),
        encoding="utf-8",
    )
    layout_payload = b"".join(word.to_bytes(2, "big") for word in layout_words)
    addresses["layout_DiamondMenu"] = len(rom)
    rom.extend(layout_payload)

    border_path = disasm / texture_extract.UI_DIAMOND_BORDER_PATH
    border_path.parent.mkdir(parents=True, exist_ok=True)
    border_path.write_text(
        "\n".join(
            f"tiles_DiamondMenuBorder{index}:\n    dc.b " + ",".join("0" for _ in range(48))
            for index in range(1, 5)
        )
        + "\n",
        encoding="utf-8",
    )
    for index in range(1, 5):
        addresses[f"tiles_DiamondMenuBorder{index}"] = len(rom)
        rom.extend(bytes(48))

    strings_path = disasm / texture_extract.UI_MENU_STRINGS_PATH
    strings_path.parent.mkdir(parents=True, exist_ok=True)
    strings_path.write_text("aAttack_0:      dc.b 'ATTACK',0\n", encoding="utf-8")
    addresses["aAttack_0"] = len(rom)
    rom.extend(b"ATTACK\0")

    source = ProvenanceBoundSource(disasm, bytes(rom), addresses)
    monkeypatch.setattr(
        texture_extract,
        "_verified_source",
        lambda *_args, **_kwargs: (
            source,
            "c834c652b6862bc5679fd7f69a38a7093206efc6",
            {"id": "synthetic", "hashes": {"sha256": hashlib.sha256(rom).hexdigest().upper()}},
        ),
    )
    base_decoded = bytearray(8192)
    base_decoded[32] = 0x80  # tile 1 starts with nonzero palette index 8, whose RGB is black
    def fake_decode(data, *, expected_output_bytes=None):
        assert data == b"base-compressed"
        output = bytes(base_decoded)
        assert len(output) == expected_output_bytes
        return SimpleNamespace(output=output)

    monkeypatch.setattr(texture_extract, "decode_stack_compressed", fake_decode)
    out_dir = tmp_path / "output"
    result = texture_extract.extract_ui_windows(
        tmp_path / "unused-rom-path", tmp_path / "unused-upstream-path", out_dir=out_dir
    )
    assert result["Windows"] == 3
    width, height, depth, color_type, raw = _read_png(out_dir / "windows/actionmenu-frame.png")
    assert (width, height, depth, color_type) == (144, 48, 8, 6)
    black_x = (len(dynamic_words) % 18) * 8
    black_y = (len(dynamic_words) // 18) * 8
    black_offset = black_y * (1 + width * 4) + 1 + black_x * 4
    assert raw[black_offset : black_offset + 4] == b"\x00\x00\x00\xff"
    with pytest.raises(ValueError, match="texture output collision"):
        texture_extract.extract_ui_windows(
            tmp_path / "unused-rom-path", tmp_path / "unused-upstream-path", out_dir=out_dir
        )

    (disasm / texture_extract.UI_BASE_TILES_PATH).write_bytes(b"mutated")
    with pytest.raises(ValueError, match="source/ROM provenance drift"):
        texture_extract.extract_ui_windows(
            tmp_path / "unused-rom-path",
            tmp_path / "unused-upstream-path",
            out_dir=tmp_path / "mutated-output",
        )
