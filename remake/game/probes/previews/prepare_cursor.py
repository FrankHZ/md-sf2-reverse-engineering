"""Prepare the private radius-zero sprite for the Godot design preview."""

from sf2tool.compression import decode_stack_compressed
from sf2tool.paths import repo_path
from sf2tool.private_inputs import ROM_INPUT_IDENTITY, private_input_path
from sf2tool.remake_battle_scene_content import compose, sprite_layout
from sf2tool.rom import verify_rom
from sf2tool.texture_extract import md_palette_color, write_png_rgba


def main() -> None:
    source = private_input_path(ROM_INPUT_IDENTITY)
    verify_rom(source)
    rom = source.read_bytes()
    # US sf2splits.txt: cursortiles.bin and basepalette.bin. Sprite 1664 is
    # the first 4x4-tile column-major hardware sprite uploaded to VRAM $D000.
    tiles = decode_stack_compressed(rom[0x23658:0x23758], expected_output_bytes=2048).output
    base = rom[0x309E:0x30BE]
    palette = [
        md_palette_color(int.from_bytes(base[i : i + 2], "big") & 0x0EEE)
        for i in range(0, len(base), 2)
    ]
    pixels = compose(tiles, palette, 32, 32, sprite_layout(4, 4))
    target = repo_path("local/design-preview/cell-cursor.png")
    target.parent.mkdir(parents=True, exist_ok=True)
    write_png_rgba(target, 32, 32, pixels)


if __name__ == "__main__":
    main()
