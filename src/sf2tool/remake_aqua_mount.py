"""Pinned BUBBLE_BREATH frames and rank palettes for the private publisher."""

import json

from sf2tool.remake_asset_build import ACCEPTED_ROM_SHA256, ACCEPTED_UPSTREAM_COMMIT
from sf2tool.remake_battle_scene_content import compose
from sf2tool.texture_extract import md_palette_color, write_png_rgba


def mount_aqua(directory, scratch, package, scenes, scale):
    from sf2tool.remake_private_package import installed_png, prepared_spell_tiles, read_json

    index = read_json(directory / "index.json")
    expected = dict(family="spell", id=0, tiles="spell-0.4bpp", palette="0aca08a60602")
    if (
        index["rom"] != ACCEPTED_ROM_SHA256
        or index["upstream"] != ACCEPTED_UPSTREAM_COMMIT
        or [r for r in index["records"] if r.get("family") == "spell" and r["id"] == 0]
        != [expected]
    ):
        raise ValueError("registered AQUA effects identity differs")
    tiles, original_palette = prepared_spell_tiles(directory, 0)
    if len(tiles) != 33 * 32:
        raise ValueError("registered AQUA stream length differs")
    rasters, variants = {}, {}
    # graphic_Bubble/table_1E78E: zero-based first tile and square sprite size.
    for rank in (1, 2):
        palette = list(original_palette)
        if rank == 2:
            for slot, word in zip((9, 13, 14), (0xCBF, 0xB9A, 0xB46), strict=True):
                palette[slot] = md_palette_color(word & 0xEEE)
        for frame, (first, size) in enumerate(
            ((0, 1), (1, 1), (2, 1), (3, 1), (4, 2), (8, 3), (17, 4))
        ):
            key, leaf = f"aqua-{rank}/frame-{frame}", f"aqua-{rank}-{frame}.png"
            layout = [first + x * size + y for y in range(size) for x in range(size)]
            write_png_rgba(
                scratch / leaf,
                size * 8,
                size * 8,
                compose(tiles, palette, size * 8, size * 8, layout),
            )
            rasters[key] = installed_png(scratch, leaf)
            if scale is not None:
                from PIL import Image

                from sf2tool.remake_hd_actors import encode
                from sf2tool.remake_hd_battle_rasters import _check_masks, derive_frame

                with Image.open(scratch / leaf) as original:
                    hd, _ = derive_frame(original, 1, scale)
                    with hd:
                        _check_masks(original, hd, key)
                        variants[key] = encode(hd)
    for battle, relative in scenes.items():
        path = package / relative
        scene = read_json(path)
        if scene["encounter"] != f"battle-{battle}":
            raise ValueError("AQUA encounter differs")
        for group, entries in (("rasters", rasters), ("hdRasters", variants)):
            for key, raster in entries.items():
                if scene.setdefault(group, {}).setdefault(key, raster) != raster:
                    raise ValueError(f"conflicting AQUA frame {key}")
        path.write_text(json.dumps(scene, separators=(",", ":")), encoding="utf-8")
