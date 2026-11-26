"""Pinned NEPTUN invocation and water composition for the closed private publisher."""

import json

from sf2tool.remake_asset_build import ACCEPTED_ROM_SHA256, ACCEPTED_UPSTREAM_COMMIT
from sf2tool.remake_battle_scene_content import compose
from sf2tool.texture_extract import write_png_rgba


def mount_neptun(directory, scratch, package, scenes, scale):
    from sf2tool.remake_private_package import installed_png, prepared_spell_tiles, read_json

    index = read_json(directory / "index.json")
    invocation = dict(
        family="invocation",
        id=2,
        streams=[f"invocation-2-{frame}-{part}.4bpp" for frame in range(2) for part in range(2)],
        palette="06660eee0000042208660caa046206a60aca0024024406880acc042008640ca8",
    )
    water = dict(family="spell", id=14, tiles="spell-14.4bpp", palette="0ea606ac0e0e")
    if (
        index["rom"] != ACCEPTED_ROM_SHA256
        or index["upstream"] != ACCEPTED_UPSTREAM_COMMIT
        or [r for r in index["records"] if r.get("family") == "invocation" and r["id"] == 2]
        != [invocation]
        or [r for r in index["records"] if r.get("family") == "spell" and r["id"] == 14] != [water]
    ):
        raise ValueError("registered NEPTUN effects identity differs")
    from sf2tool.remake_invocation_mount import invocation_sprites

    sprites = invocation_sprites(directory, 2, 2, invocation["palette"])
    tiles, palette = prepared_spell_tiles(directory, 14)
    if len(tiles) != 1344:
        raise ValueError("registered NEPTUN water stream length differs")
    # Invocation load maps first24 tiles to VRAM1920; normal load maps all42
    # tiles to SPELLTILE1. Rain19/27 and sizzle31 are one-based tile names.
    parts = [(f"rise-{i}", i * 6, 2, 3) for i in range(3)]
    parts += [("rain-top", 18, 2, 4), ("rain-bottom", 26, 2, 2)]
    parts += [(f"sizzle-{i}", 30 + i * 4, 2, 2) for i in range(3)]
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
        key, leaf = f"neptun/{name}", f"neptun-{name}.png"
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
            raise ValueError("NEPTUN encounter differs")
        for group, entries in (("rasters", rasters), ("hdRasters", variants)):
            for key, raster in entries.items():
                if scene.setdefault(group, {}).setdefault(key, raster) != raster:
                    raise ValueError(f"conflicting NEPTUN frame {key}")
        path.write_text(json.dumps(scene, separators=(",", ":")), encoding="utf-8")
