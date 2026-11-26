"""Prepared private attack-spell rasters for the existing package publisher."""

import json
import struct
from pathlib import Path

from sf2tool.remake_asset_build import ACCEPTED_ROM_SHA256, ACCEPTED_UPSTREAM_COMMIT
from sf2tool.remake_battle_scene_content import compose
from sf2tool.texture_extract import write_png_rgba


def mount_attack_spells(
    directory: Path,
    scratch: Path,
    package: Path,
    scenes: dict[int, Path],
    families: set[str],
    scale=None,
) -> None:
    from sf2tool.remake_private_package import installed_png, prepared_spell_tiles, read_json

    if "atlas-family" in families:
        from sf2tool.remake_atlas_mount import mount_atlas

        mount_atlas(directory, scratch, package, scenes, scale)
    if "neptun-family" in families:
        from sf2tool.remake_neptun_mount import mount_neptun

        mount_neptun(directory, scratch, package, scenes, scale)
    if "apollo-family" in families:
        from sf2tool.remake_apollo_mount import mount_apollo

        mount_apollo(directory, scratch, package, scenes, scale)
    if "dao-family" in families:
        from sf2tool.remake_dao_mount import mount_dao

        mount_dao(directory, scratch, package, scenes, scale)
    if "desoul-family" in families:
        from sf2tool.remake_desoul_mount import mount_desoul

        mount_desoul(directory, scratch, package, scenes, scale)
    if "bolt-family" in families:
        mount_bolt(directory, scratch, package, scenes, scale)
    if "freeze-family" in families:
        mount_freeze(directory, scratch, package, scenes, scale)
    if families & {"blast-1", "blast-family"}:
        mount_blast(directory, scratch, package, scenes, scale, "blast-family" in families)
    if not families & {"blaze", "blaze-2", "flame-family"}:
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
    if families & {"blaze", "flame-family"}:
        # table_1BE2A. Both 2x4 entries share position and tiles.
        sprites.update(
            {
                "blaze-1/frame-0": (2, ((33, 4),)),
                "blaze-1/frame-1": (2, ((41, 4),)),
            }
        )
    if families & {"blaze-2", "flame-family"}:
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
    if "flame-family" in families:
        # table_1BE2A and table_1BE42+32; graphic_BlazeDragon (zero-based tiles).
        sprites.update(
            {
                "blaze-large/flame-0": (4, ((85, 4), (101, 4))),
                "blaze-large/flame-1": (4, ((117, 4), (133, 4))),
                "blaze-large/meteor-0": (3, ((172, 3),)),
                "blaze-large/meteor-1": (3, ((181, 3),)),
                "blaze-large/impact-0": (4, ((190, 4),)),
                "blaze-large/impact-1": (4, ((206, 4),)),
                "blaze-dragon/head": (4, ((17, 4),)),
                "blaze-dragon/body": (3, ((8, 3),)),
                "blaze-dragon/tail-0": (2, ((4, 2),)),
                "blaze-dragon/tail-1": (2, ((0, 2),)),
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
    variants = {}
    if scale is not None:
        from PIL import Image

        from sf2tool.remake_hd_actors import encode
        from sf2tool.remake_hd_battle_rasters import _check_masks, derive_frame

        for key in rasters:
            with Image.open(scratch / (key.replace("/", "-") + ".png")) as original:
                hd, _ = derive_frame(original, 1, scale)
                with hd:
                    _check_masks(original, hd, key)
                    variants[key] = encode(hd)
    for battle, relative in scenes.items():
        path = package / relative
        scene = read_json(path)
        if scene["encounter"] != f"battle-{battle}":
            raise ValueError(f"BLAZE scene does not select Battle{battle:02d}")
        current = scene["rasters"]
        for key, raster in rasters.items():
            if current.setdefault(key, raster) != raster:
                raise ValueError(f"conflicting prepared BLAZE frame {key}")
        for key, raster in variants.items():
            # Keep accepted variants when extending a complete predecessor.
            scene.setdefault("hdRasters", {}).setdefault(key, raster)
        path.write_text(json.dumps(scene, separators=(",", ":")), encoding="utf-8")


def mount_freeze(directory, scratch, package, scenes, scale):
    from sf2tool.remake_private_package import installed_png, prepared_spell_tiles, read_json

    index = read_json(directory / "index.json")
    expected = dict(family="spell", id=11, tiles="spell-11.4bpp", palette="0e880eca0eee")
    if (
        index["rom"] != ACCEPTED_ROM_SHA256
        or index["upstream"] != ACCEPTED_UPSTREAM_COMMIT
        or [r for r in index["records"] if r.get("family") == "spell" and r["id"] == 11]
        != [expected]
    ):
        raise ValueError("registered FREEZE effects identity differs")
    tiles, palette = prepared_spell_tiles(directory, 11)
    if len(tiles) != 2752:
        raise ValueError("registered FREEZE tile stream length differs")
    # table_1C18A/table_1C19A: six sizes, two brightness banks. v4 adds tiles80/83.
    sprites = [
        (first + bank, size, size)
        for bank in (0, 40)
        for first, size in ((0, 1), (1, 1), (2, 2), (6, 3), (15, 3), (24, 4))
    ]
    sprites += [(80, 1, 3), (83, 1, 3)]
    rasters, variants = {}, {}
    for slot, (first, columns, rows) in enumerate(sprites):
        leaf, key = f"freeze-{slot}.png", f"freeze/frame-{slot}"
        layout = [first + x * rows + y for y in range(rows) for x in range(columns)]
        write_png_rgba(
            scratch / leaf,
            columns * 8,
            rows * 8,
            compose(tiles, palette, columns * 8, rows * 8, layout),
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
            raise ValueError("FREEZE encounter differs")
        for key, raster in rasters.items():
            if scene["rasters"].setdefault(key, raster) != raster:
                raise ValueError(f"conflicting prepared FREEZE frame {key}")
        for key, raster in variants.items():
            scene.setdefault("hdRasters", {}).setdefault(key, raster)
        path.write_text(json.dumps(scene, separators=(",", ":")), encoding="utf-8")


def mount_blast(directory, scratch, package, scenes, scale=None, family=False):
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
    keys = [f"blast-1/frame-{i}" for i in range(15)]
    if family:
        # Even variations grow to the second bank; v3/v4 stack two 4x4
        # cyclone parts, then attach three 2x2 dust frames (source tile offsets).
        sprites += [(11, 3, 4), (35, 4, 2)]
        sprites += [(first, 4, 8) for first in (43, 75, 107)]
        sprites += [(first, 2, 2) for first in (139, 143, 147)]
        keys += ["blast/large-diagonal", "blast/large-horizontal"]
        keys += [f"blast/cyclone-{i}" for i in range(3)]
        keys += [f"blast/dust-{i}" for i in range(3)]
    rasters, variants = {}, {}
    for key, (first, columns, rows) in zip(keys, sprites, strict=True):
        leaf = key.replace("/", "-") + ".png"
        # The cyclone is two consecutive 4x4 VDP sprites, not one 4x8 tile grid.
        part_rows = 4 if rows == 8 else rows
        layout = [
            first + part * columns * part_rows + x * part_rows + y
            for part in range(rows // part_rows)
            for y in range(part_rows)
            for x in range(columns)
        ]
        write_png_rgba(
            scratch / leaf,
            columns * 8,
            rows * 8,
            compose(tiles, palette, columns * 8, rows * 8, layout),
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
            raise ValueError("BLAST encounter differs")
        for key, raster in rasters.items():
            if scene["rasters"].setdefault(key, raster) != raster:
                raise ValueError(f"conflicting prepared BLAST frame {key}")
        for key, raster in variants.items():
            scene.setdefault("hdRasters", {}).setdefault(key, raster)
        path.write_text(json.dumps(scene, separators=(",", ":")), encoding="utf-8")


def mount_bolt(directory, scratch, package, scenes, scale):
    from PIL import Image

    from sf2tool.remake_hd_actors import encode
    from sf2tool.remake_hd_battle_rasters import _check_masks, derive_frame
    from sf2tool.remake_private_package import prepared_spell_tiles, read_json

    index = read_json(directory / "index.json")
    expected = dict(family="spell", id=4, tiles="spell-4.4bpp", palette="0ec80ca00e40")
    if (
        index["rom"] != ACCEPTED_ROM_SHA256
        or index["upstream"] != ACCEPTED_UPSTREAM_COMMIT
        or [r for r in index["records"] if r.get("family") == "spell" and r["id"] == 4]
        != [expected]
    ):
        raise ValueError("registered BOLT effects identity differs")
    tiles, palette = prepared_spell_tiles(directory, 4)
    if len(tiles) != 5728:
        raise ValueError("registered BOLT tile stream length differs")
    rasters, variants = {}, {}

    def install(key, original):
        rasters[key] = encode(original)
        if scale is not None:
            hd, _ = derive_frame(original, 1, scale)
            with hd:
                _check_masks(original, hd, key)
                variants[key] = encode(hd)

    def sprite(first, columns, rows):
        layout = [first + x * rows + y for y in range(rows) for x in range(columns)]
        return Image.frombytes(
            "RGBA",
            (columns * 8, rows * 8),
            bytes(compose(tiles, palette, columns * 8, rows * 8, layout)),
        )

    # BoltAnimData_A/B: eleven five-sprite frames each. ConstructComplexGraphic
    # applies the low property bits as H/V flips; graphic tile1 is VDP $520.
    # The fixed canvas origin (-24,48) preserves source coordinates and extents.
    for bank, name in enumerate(("A", "B")):
        data = (directory / f"BoltAnimData_{name}.bin").read_bytes()
        if len(data) != 440:
            raise ValueError("registered BOLT operand table length differs")
        for frame in range(11):
            with Image.new("RGBA", (64, 96)) as canvas:
                # Earlier VDP sprites have priority at overlap.
                for slot in reversed(range(5)):
                    x, y, tile, prop = struct.unpack_from(">hhHH", data, frame * 40 + slot * 8)
                    if tile == 0:
                        continue
                    columns, rows = ((prop >> 10) & 3) + 1, ((prop >> 8) & 3) + 1
                    first = (tile & 2047) - 0x520
                    if first < 0 or (first + columns * rows) * 32 > len(tiles):
                        raise ValueError("BOLT sprite tile outside prepared stream")
                    with sprite(first, columns, rows) as raw:
                        part = raw
                        if prop & 1:
                            part = part.transpose(Image.Transpose.FLIP_LEFT_RIGHT)
                        if prop & 2:
                            part = part.transpose(Image.Transpose.FLIP_TOP_BOTTOM)
                        if not (0 <= x + 24 <= 64 - part.width and 0 <= y - 48 <= 96 - part.height):
                            raise ValueError("BOLT sprite outside source composition canvas")
                        canvas.alpha_composite(part, (x + 24, y - 48))
                        if part is not raw:
                            part.close()
                install(f"bolt/frame-{bank * 11 + frame}", canvas)
    # table_1AB5E: 4x4 orb tiles148/164 (one-based). All eight orientations
    # are prepared so runtime uses the same raster/HD mechanism as other effects.
    for phase in range(8):
        with sprite(147 + (16 if phase & 2 else 0), 4, 4) as raw:
            part = raw
            if phase & 1:
                part = part.transpose(Image.Transpose.FLIP_LEFT_RIGHT)
            if phase & 4:
                part = part.transpose(Image.Transpose.FLIP_TOP_BOTTOM)
            install(f"bolt/frame-{22 + phase}", part)
            if part is not raw:
                part.close()
    for battle, relative in scenes.items():
        path = package / relative
        scene = read_json(path)
        if scene["encounter"] != f"battle-{battle}":
            raise ValueError("BOLT encounter differs")
        for key, raster in rasters.items():
            if scene["rasters"].setdefault(key, raster) != raster:
                raise ValueError(f"conflicting prepared BOLT frame {key}")
        for key, raster in variants.items():
            scene.setdefault("hdRasters", {}).setdefault(key, raster)
        path.write_text(json.dumps(scene, separators=(",", ":")), encoding="utf-8")
