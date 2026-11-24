"""Publish the prepared DEBUFF1 cloud group for private DISPEL playback."""

import argparse
import json
from pathlib import Path

from PIL import Image

from sf2tool.remake_asset_build import ACCEPTED_ROM_SHA256, ACCEPTED_UPSTREAM_COMMIT
from sf2tool.remake_battle_scene_content import compose
from sf2tool.remake_detox_art import publish
from sf2tool.remake_hd_actors import encode
from sf2tool.remake_hd_battle_rasters import _check_masks, derive_frame
from sf2tool.remake_map3_ramp import load_filter
from sf2tool.remake_private_package import prepared_spell_tiles, read_json
from sf2tool.texture_extract import md_palette_color


def artwork(directory, scale):
    index = read_json(directory / "index.json")
    expected = dict(family="spell", id=9, tiles="spell-9.4bpp", palette="084e088e0cae")
    if (
        index["rom"] != ACCEPTED_ROM_SHA256
        or index["upstream"] != ACCEPTED_UPSTREAM_COMMIT
        or [row for row in index["records"] if row.get("family") == "spell" and row["id"] == 9]
        != [expected]
    ):
        raise ValueError("registered DEBUFF effects identity differs")
    tiles, palette = prepared_spell_tiles(directory, 9)
    if len(tiles) != 1536:
        raise ValueError("registered DEBUFF tile stream length differs")
    # spellanimationSetup_Debuff1 replaces the tileset's default colors.
    for slot, word in zip((9, 13, 14), (0x888, 0xBBB, 0xDDD), strict=True):
        palette[slot] = md_palette_color(word & 0xEEE)
    originals, variants = {}, {}
    for frame in range(3):
        key = f"debuff-1/frame-{frame}"
        layout = [frame * 16 + x * 4 + y for y in range(4) for x in range(4)]
        pixels = compose(tiles, palette, 32, 32, layout)
        with Image.frombytes("RGBA", (32, 32), bytes(pixels)) as original:
            hd, _ = derive_frame(original, 1, scale)
            with hd:
                _check_masks(original, hd, key)
                originals[key], variants[key] = encode(original), encode(hd)
    return originals, variants


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ("base", "effects", "output", "gfx-root"):
        parser.add_argument("--" + name, type=Path, required=True)
    args = parser.parse_args()
    originals, variants = artwork(args.effects, load_filter(args.gfx_root)[1])
    print(json.dumps(publish(args.base, args.output, originals, variants)))


if __name__ == "__main__":
    main()
