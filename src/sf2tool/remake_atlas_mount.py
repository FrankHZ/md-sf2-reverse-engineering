"""Pinned ATLAS two-pose invocation composition for the closed private publisher."""

import json

from sf2tool.remake_asset_build import ACCEPTED_ROM_SHA256, ACCEPTED_UPSTREAM_COMMIT
from sf2tool.texture_extract import write_png_rgba


def mount_atlas(directory, scratch, package, scenes, scale):
    from sf2tool.remake_private_package import installed_png, read_json

    index = read_json(directory / "index.json")
    invocation = dict(
        family="invocation",
        id=1,
        streams=[f"invocation-1-{frame}-{part}.4bpp" for frame in range(2) for part in range(2)],
        palette="08640c400202042406460a680c8a0eaa0000022206660aaa00a600c600ea0eee",
    )
    if (
        index["rom"] != ACCEPTED_ROM_SHA256
        or index["upstream"] != ACCEPTED_UPSTREAM_COMMIT
        or [r for r in index["records"] if r.get("family") == "invocation" and r["id"] == 1]
        != [invocation]
    ):
        raise ValueError("registered ATLAS effects identity differs")
    from sf2tool.remake_invocation_mount import invocation_sprites

    sprites = invocation_sprites(directory, 1, 2, invocation["palette"])
    rasters, variants = {}, {}
    for name, width, height, pixels in sprites:
        key, leaf = f"atlas/{name}", f"atlas-{name}.png"
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
            raise ValueError("ATLAS encounter differs")
        for group, entries in (("rasters", rasters), ("hdRasters", variants)):
            for key, raster in entries.items():
                if scene.setdefault(group, {}).setdefault(key, raster) != raster:
                    raise ValueError(f"conflicting ATLAS frame {key}")
        path.write_text(json.dumps(scene, separators=(",", ":")), encoding="utf-8")
