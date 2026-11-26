"""Pinned FLAME_BREATH variation3 growth frames for the private publisher."""

import json

from sf2tool.remake_asset_build import ACCEPTED_ROM_SHA256, ACCEPTED_UPSTREAM_COMMIT
from sf2tool.remake_battle_scene_content import compose
from sf2tool.texture_extract import write_png_rgba


def mount_kiwi(directory, scratch, package, scenes, scale):
    from sf2tool.remake_private_package import installed_png, prepared_spell_tiles, read_json

    index = read_json(directory / "index.json")
    expected = dict(family="spell", id=1, tiles="spell-1.4bpp", palette="022e028e08ee")
    if (
        index["rom"] != ACCEPTED_ROM_SHA256
        or index["upstream"] != ACCEPTED_UPSTREAM_COMMIT
        or [r for r in index["records"] if r.get("family") == "spell" and r["id"] == 1]
        != [expected]
    ):
        raise ValueError("registered KIWI effects identity differs")
    tiles, palette = prepared_spell_tiles(directory, 1)
    if len(tiles) != 35 * 32:
        raise ValueError("registered KIWI stream length differs")
    rasters, variants = {}, {}
    # SPELLTILE1 is zero-based0. sub_1B884 adds table offsets at states5/10/15/20/25.
    # The update's byte bit3 toggles horizontal mirroring, not the tile index.
    for frame, (first, size) in enumerate(((0, 1), (1, 1), (2, 2), (6, 2), (10, 3), (19, 4))):
        key, leaf = f"kiwi-flame/frame-{frame}", f"kiwi-flame-frame-{frame}.png"
        layout = [first + x * size + y for y in range(size) for x in range(size)]
        write_png_rgba(
            scratch / leaf, size * 8, size * 8, compose(tiles, palette, size * 8, size * 8, layout)
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
            raise ValueError("KIWI encounter differs")
        for group, entries in (("rasters", rasters), ("hdRasters", variants)):
            for key, raster in entries.items():
                if scene.setdefault(group, {}).setdefault(key, raster) != raster:
                    raise ValueError(f"conflicting KIWI frame {key}")
        path.write_text(json.dumps(scene, separators=(",", ":")), encoding="utf-8")
