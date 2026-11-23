"""Prepared private attack-spell rasters for the existing package publisher."""

import json
from pathlib import Path

from sf2tool.remake_asset_build import ACCEPTED_ROM_SHA256, ACCEPTED_UPSTREAM_COMMIT
from sf2tool.remake_battle_scene_content import compose
from sf2tool.texture_extract import write_png_rgba


def mount_attack_spells(
    directory: Path, scratch: Path, package: Path, scenes: dict[int, Path], families: set[str]
) -> None:
    from sf2tool.remake_private_package import installed_png, prepared_spell_tiles, read_json

    if "blast-1" in families:
        mount_blast(directory, scratch, package, scenes)
    if not families & {"blaze", "blaze-2"}:
        return
    index = read_json(directory / "index.json")
    expected = dict(family="spell", id=2, tiles="spell-2.4bpp", palette="022e028e08ee")
    if (
        index["rom"] != ACCEPTED_ROM_SHA256
        or index["upstream"] != ACCEPTED_UPSTREAM_COMMIT
        or [row for row in index["records"] if row.get("family") == "spell" and row["id"] == 2]
        != [expected]
    ):
        raise ValueError("registered BLAZE effects identity differs")
    tiles, palette = prepared_spell_tiles(directory, 2)
    if len(tiles) != 7104:
        raise ValueError("registered BLAZE tile stream length differs")
    sprites = {}
    if "blaze" in families:
        # table_1BE2A. Both 2x4 entries share position and tiles.
        sprites.update(
            {
                "blaze-1/frame-0": (2, ((33, 4),)),
                "blaze-1/frame-1": (2, ((41, 4),)),
            }
        )
    if "blaze-2" in families:
        # table_1BE2A stacks a 3x4 top over a 3x2 bottom.
        # table_1BE42, table_1BE52 and table_1BE5A select the small meteor and impact.
        sprites.update(
            {
                "blaze-2/flame-0": (3, ((49, 4), (61, 2))),
                "blaze-2/flame-1": (3, ((67, 4), (79, 2))),
                "blaze-2/meteor-0": (2, ((149, 2),)),
                "blaze-2/meteor-1": (2, ((153, 2),)),
                "blaze-2/impact-0": (3, ((157, 3),)),
                "blaze-2/impact-1": (3, ((166, 2),)),
            }
        )
    rasters = {}
    for key, (columns, parts) in sprites.items():
        leaf = key.replace("/", "-") + ".png"
        layout = [
            first + x * rows + y
            for first, rows in parts
            for y in range(rows)
            for x in range(columns)
        ]
        width, height = columns * 8, sum(rows for _, rows in parts) * 8
        write_png_rgba(
            scratch / leaf, width, height, compose(tiles, palette, width, height, layout)
        )
        rasters[key] = installed_png(scratch, leaf)
    for battle, relative in scenes.items():
        path = package / relative
        scene = read_json(path)
        if scene["encounter"] != f"battle-{battle}":
            raise ValueError(f"BLAZE scene does not select Battle{battle:02d}")
        current = scene["rasters"]
        for key, raster in rasters.items():
            if current.setdefault(key, raster) != raster:
                raise ValueError(f"conflicting prepared BLAZE frame {key}")
        path.write_text(json.dumps(scene, separators=(",", ":")), encoding="utf-8")


def mount_blast(directory, scratch, package, scenes):
    from sf2tool.remake_private_package import installed_png, prepared_spell_tiles, read_json

    index = read_json(directory / "index.json")
    expected = dict(family="spell", id=15, tiles="spell-15.4bpp", palette="0ec8026c0c82")
    if (
        index["rom"] != ACCEPTED_ROM_SHA256
        or index["upstream"] != ACCEPTED_UPSTREAM_COMMIT
        or [r for r in index["records"] if r.get("family") == "spell" and r["id"] == 15]
        != [expected]
    ):
        raise ValueError("registered BLAST effects identity differs")
    tiles, palette = prepared_spell_tiles(directory, 15)
    if len(tiles) != 6816:
        raise ValueError("registered BLAST tile stream length differs")
    # Initial particles, table_1CBDE, and table_1CC44. Zero-based tile operands.
    sprites = [
        (0, 1, 1),
        (1, 2, 2),
        (5, 2, 3),
        (23, 1, 2),
        (25, 2, 2),
        (29, 3, 2),
        (151, 2, 2),
        (155, 2, 4),
        (163, 2, 3),
        (169, 2, 2),
        (173, 4, 2),
        (181, 3, 2),
        (187, 2, 2),
        (191, 4, 4),
        (207, 2, 3),
    ]
    rasters = {}
    for slot, (first, columns, rows) in enumerate(sprites):
        leaf = f"blast-1-{slot}.png"
        layout = [first + x * rows + y for y in range(rows) for x in range(columns)]
        write_png_rgba(
            scratch / leaf,
            columns * 8,
            rows * 8,
            compose(tiles, palette, columns * 8, rows * 8, layout),
        )
        rasters[f"blast-1/frame-{slot}"] = installed_png(scratch, leaf)
    for battle, relative in scenes.items():
        path = package / relative
        scene = read_json(path)
        if scene["encounter"] != f"battle-{battle}":
            raise ValueError("BLAST encounter differs")
        for key, raster in rasters.items():
            if scene["rasters"].setdefault(key, raster) != raster:
                raise ValueError(f"conflicting prepared BLAST frame {key}")
        path.write_text(json.dumps(scene, separators=(",", ":")), encoding="utf-8")
