"""Pinned DESOUL composition for the existing closed private publisher."""

import json

from sf2tool.remake_asset_build import ACCEPTED_ROM_SHA256, ACCEPTED_UPSTREAM_COMMIT
from sf2tool.remake_battle_scene_content import compose
from sf2tool.texture_extract import write_png_rgba


def mount_desoul(directory, scratch, package, scenes, scale):
    from sf2tool.remake_private_package import installed_png, prepared_spell_tiles, read_json

    index = read_json(directory / "index.json")
    expected = dict(family="spell", id=13, tiles="spell-13.4bpp", palette="06220c880844")
    if (
        index["rom"] != ACCEPTED_ROM_SHA256
        or index["upstream"] != ACCEPTED_UPSTREAM_COMMIT
        or [r for r in index["records"] if r.get("family") == "spell" and r["id"] == 13]
        != [expected]
    ):
        raise ValueError("registered DESOUL effects identity differs")
    tiles, palette = prepared_spell_tiles(directory, 13)
    if len(tiles) != 3328:
        raise ValueError("registered DESOUL tile stream length differs")
    # graphic_Reaper: each half stacks a 4x4 and a 4x2 sprite. Strike adds48.
    bodies = [
        [
            bank + (0 if x < 4 else 24) + (x % 4) * 4 + y
            if y < 4
            else bank + (16 if x < 4 else 40) + (x % 4) * 2 + y - 4
            for y in range(6)
            for x in range(8)
        ]
        for bank in (0, 48)
    ]
    sprites = [("hover", 64, 48, bodies[0]), ("strike", 64, 48, bodies[1])]
    sprites += [
        (f"soul-{i}", 16, 16, [first + x * 2 + y for y in range(2) for x in range(2)])
        for i, first in enumerate((96, 100))
    ]
    rasters, variants = {}, {}
    for name, width, height, layout in sprites:
        key, leaf = f"desoul/{name}", f"desoul-{name}.png"
        write_png_rgba(
            scratch / leaf, width, height, compose(tiles, palette, width, height, layout)
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
            raise ValueError("DESOUL encounter differs")
        for key, raster in rasters.items():
            if scene["rasters"].setdefault(key, raster) != raster:
                raise ValueError(f"conflicting DESOUL frame {key}")
        for key, raster in variants.items():
            scene.setdefault("hdRasters", {}).setdefault(key, raster)
        path.write_text(json.dumps(scene, separators=(",", ":")), encoding="utf-8")
