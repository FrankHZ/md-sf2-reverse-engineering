"""Pinned APOLLO invocation and flame composition for the closed private publisher."""

import json

from sf2tool.remake_asset_build import ACCEPTED_ROM_SHA256, ACCEPTED_UPSTREAM_COMMIT
from sf2tool.remake_battle_scene_content import compose
from sf2tool.texture_extract import write_png_rgba


def mount_apollo(directory, scratch, package, scenes, scale):
    from sf2tool.remake_private_package import installed_png, prepared_spell_tiles, read_json

    index = read_json(directory / "index.json")
    invocation = dict(
        family="invocation",
        id=3,
        streams=[f"invocation-3-{frame}-{part}.4bpp" for frame in range(3) for part in range(2)],
        palette="06660eee00000024026806ac06620aa804200666066606660666004600ac06ee",
    )
    flame = dict(family="spell", id=19, tiles="spell-19.4bpp", palette="0eee026e002a")
    if (
        index["rom"] != ACCEPTED_ROM_SHA256
        or index["upstream"] != ACCEPTED_UPSTREAM_COMMIT
        or [r for r in index["records"] if r.get("family") == "invocation" and r["id"] == 3]
        != [invocation]
        or [r for r in index["records"] if r.get("family") == "spell" and r["id"] == 19] != [flame]
    ):
        raise ValueError("registered APOLLO effects identity differs")
    from sf2tool.remake_invocation_mount import invocation_sprites

    sprites = invocation_sprites(directory, 3, 3, invocation["palette"])
    tiles, palette = prepared_spell_tiles(directory, 19)
    if len(tiles) != 2816:
        raise ValueError("registered APOLLO flame stream length differs")
    # Invocation loader maps tiles0..23 to VRAM1920; setup maps four
    # 16-tile banks24/40/56/72 to384/528/1440/1776. Update selects
    # small1940/1936/1932/1928 and large1776/1440/528/384.
    parts = [(f"mouth-{i}", i * 4, 2, 2) for i in range(2)]
    parts += [(f"small-{i}", first, 2, 2) for i, first in enumerate((20, 16, 12, 8))]
    parts += [(f"large-{i}", first, 4, 4) for i, first in enumerate((72, 56, 40, 24))]
    for name, first, columns, rows in parts:
        layout = [first + x * rows + y for y in range(rows) for x in range(columns)]
        sprites.append(
            (
                name,
                columns * 8,
                rows * 8,
                compose(tiles, palette, columns * 8, rows * 8, layout),
            )
        )
    rasters, variants = {}, {}
    for name, width, height, pixels in sprites:
        key, leaf = f"apollo/{name}", f"apollo-{name}.png"
        write_png_rgba(scratch / leaf, width, height, pixels)
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
            raise ValueError("APOLLO encounter differs")
        for group, entries in (("rasters", rasters), ("hdRasters", variants)):
            for key, raster in entries.items():
                if scene.setdefault(group, {}).setdefault(key, raster) != raster:
                    raise ValueError(f"conflicting APOLLO frame {key}")
        path.write_text(json.dumps(scene, separators=(",", ":")), encoding="utf-8")
