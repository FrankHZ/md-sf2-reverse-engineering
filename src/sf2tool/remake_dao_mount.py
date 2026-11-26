"""Pinned DAO invocation and rock composition for the closed private publisher."""

import json

from sf2tool.remake_asset_build import ACCEPTED_ROM_SHA256, ACCEPTED_UPSTREAM_COMMIT
from sf2tool.remake_battle_scene_content import compose
from sf2tool.texture_extract import md_palette_color, write_png_rgba


def mount_dao(directory, scratch, package, scenes, scale):
    from sf2tool.remake_private_package import installed_png, prepared_spell_tiles, read_json

    index = read_json(directory / "index.json")
    invocation = dict(
        family="invocation",
        id=0,
        streams=[f"invocation-0-{frame}-{part}.4bpp" for frame in range(8) for part in range(2)],
        palette="06640eee0000002400460268048a08ce08c60484004200220caa086604220e0e",
    )
    rock = dict(family="spell", id=3, tiles="spell-3.4bpp", palette="0024026a048c")
    if (
        index["rom"] != ACCEPTED_ROM_SHA256
        or index["upstream"] != ACCEPTED_UPSTREAM_COMMIT
        or [r for r in index["records"] if r.get("family") == "invocation" and r["id"] == 0]
        != [invocation]
        or [r for r in index["records"] if r.get("family") == "spell" and r["id"] == 3] != [rock]
    ):
        raise ValueError("registered DAO effects identity differs")
    # Header palette word0 is not copied: LoadInvocationSpriteFrameToVram
    # advances2 bytes and writes the remaining15 into palette1 colors1..15.
    words = bytes.fromhex(invocation["palette"])[2:]
    palette = [(0, 0, 0)] + [
        md_palette_color(int.from_bytes(words[i : i + 2], "big")) for i in range(0, 30, 2)
    ]
    # layout_Invocation: four columns of 32x32 sprites. The second stream
    # starts at tile144 after the first128 tiles and16 unused VRAM tiles.
    layout = [
        (x // 4) * 32 + (y // 4 % 2) * 16 + (144 if y >= 8 else 0) + (x % 4) * 4 + y % 4
        for y in range(16)
        for x in range(16)
    ]
    sprites = []
    for frame in range(8):
        streams = [
            (directory / f"invocation-0-{frame}-{part}.4bpp").read_bytes() for part in range(2)
        ]
        if any(len(stream) != 4096 for stream in streams):
            raise ValueError("registered DAO invocation stream length differs")
        sprites.append(
            (
                f"summon-{frame}",
                128,
                128,
                compose(streams[0] + bytes(512) + streams[1], palette, 128, 128, layout),
            )
        )
    tiles, palette = prepared_spell_tiles(directory, 3)
    if len(tiles) != 608:
        raise ValueError("registered DAO rock stream length differs")
    for name, first, columns, rows in (("small", 0, 1, 1), ("medium", 1, 2, 3), ("large", 7, 3, 4)):
        layout = [first + x * rows + y for y in range(rows) for x in range(columns)]
        sprites.append(
            (
                f"rock-{name}",
                columns * 8,
                rows * 8,
                compose(tiles, palette, columns * 8, rows * 8, layout),
            )
        )
    rasters, variants = {}, {}
    for name, width, height, pixels in sprites:
        key, leaf = f"dao/{name}", f"dao-{name}.png"
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
            raise ValueError("DAO encounter differs")
        for group, entries in (("rasters", rasters), ("hdRasters", variants)):
            for key, raster in entries.items():
                if scene.setdefault(group, {}).setdefault(key, raster) != raster:
                    raise ValueError(f"conflicting DAO frame {key}")
        path.write_text(json.dumps(scene, separators=(",", ":")), encoding="utf-8")
