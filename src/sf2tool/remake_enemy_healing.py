"""Publish the prepared enemy healing family into a complete private package."""

import argparse
import json
import shutil
from pathlib import Path
from tempfile import TemporaryDirectory

from PIL import Image

from sf2tool.remake_asset_build import ACCEPTED_ROM_SHA256, ACCEPTED_UPSTREAM_COMMIT
from sf2tool.remake_battle_scene_content import compose
from sf2tool.remake_hd_actors import encode
from sf2tool.remake_hd_battle_rasters import _check_masks, derive_frame
from sf2tool.remake_map3_ramp import load_filter
from sf2tool.remake_private_package import (
    package_files,
    prepared_spell_tiles,
    private_output,
    read_json,
)


def artwork(directory, scale):
    index = read_json(directory / "index.json")
    expected = dict(family="spell", id=6, tiles="spell-6.4bpp", palette="08880aaa0ace")
    if (
        index["rom"] != ACCEPTED_ROM_SHA256
        or index["upstream"] != ACCEPTED_UPSTREAM_COMMIT
        or [r for r in index["records"] if r.get("family") == "spell" and r["id"] == 6]
        != [expected]
    ):
        raise ValueError("registered healing effects identity differs")
    tiles, palette = prepared_spell_tiles(directory, 6)
    if len(tiles) != 3392:
        raise ValueError("registered healing tile stream length differs")
    # c834c652 healingfairy.asm table_EvilFairy_offsets, zero-based tile indexes.
    frames = [("body0", 53, 4, 4), ("body1", 69, 4, 4), ("wing0", 85, 4, 2), ("wing1", 93, 4, 2)]
    frames += [(f"dust{i}", 105 - i, 1, 1) for i in range(5)]
    originals, variants = {}, {}
    for name, first, columns, rows in frames:
        key = "healing-enemy/" + name
        layout = [first + x * rows + y for y in range(rows) for x in range(columns)]
        image = Image.frombytes(
            "RGBA",
            (columns * 8, rows * 8),
            bytes(compose(tiles, palette, columns * 8, rows * 8, layout)),
        )
        # Existing healing rasters carry doubled source pixels.
        original = image.resize((image.width * 2, image.height * 2), Image.Resampling.NEAREST)
        hd, _ = derive_frame(original, 2, scale)
        _check_masks(original, hd, key)
        originals[key], variants[key] = encode(original), encode(hd)
        image.close()
        original.close()
        hd.close()
    binding = {
        kind: ["healing-enemy/" + prefix + str(i) for i in range(count)]
        for kind, prefix, count in (
            ("bodies", "body", 2),
            ("wings", "wing", 2),
            ("dust", "dust", 5),
        )
    }
    return originals, variants, binding


def publish(base, effects, output, scale):
    base = base.resolve(strict=True)
    output = private_output(output)
    if output.exists() or output.is_relative_to(base):
        raise ValueError("select a fresh output outside the base package")
    originals, variants, binding = artwork(effects, scale)
    files = package_files(
        base / "world.json",
        base / "main-scenes.json",
        base / "entry/entry.json",
        base / "opening-inputs",
    )
    world = read_json(base / "world.json")
    scenes = {Path("main-scenes.json")} | {
        Path(row["battleScenes"]) for row in world.get("encounters", []) if row.get("battleScenes")
    }
    output.parent.mkdir(parents=True, exist_ok=True)
    mounted = []
    with TemporaryDirectory(dir=output.parent) as temporary:
        package = Path(temporary) / "package"
        for relative, source in files.items():
            target = package / relative
            target.parent.mkdir(parents=True, exist_ok=True)
            shutil.copyfile(source, target)
        for relative in sorted(scenes):
            path = package / relative
            scene = read_json(path)
            if not scene.get("healing"):
                continue
            if "enemy" in scene["healing"]:
                raise ValueError("base already declares enemy healing")
            for bank, additions in (("rasters", originals), ("hdRasters", variants)):
                current = scene.setdefault(bank, {})
                for key, value in additions.items():
                    if current.setdefault(key, value) != value:
                        raise ValueError("conflicting enemy healing raster")
            scene["healing"]["enemy"] = binding
            path.write_text(json.dumps(scene, separators=(",", ":")), encoding="utf-8")
            if path.stat().st_size > 4 * 1024 * 1024:
                raise ValueError("scene exceeds the existing 4 MiB reader limit")
            mounted.append(relative.as_posix())
        if not mounted:
            raise ValueError("base has no healing consumer")
        package.rename(output)
    return {
        "scenes": mounted,
        "uniqueOriginals": len(originals),
        "uniqueHd": len(variants),
        "inputBytes": sum(p.stat().st_size for p in files.values()),
        "outputBytes": sum(p.stat().st_size for p in output.rglob("*") if p.is_file()),
    }


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ("base", "effects", "output", "gfx-root"):
        parser.add_argument("--" + name, type=Path, required=True)
    args = parser.parse_args()
    print(json.dumps(publish(args.base, args.effects, args.output, load_filter(args.gfx_root)[1])))


if __name__ == "__main__":
    main()
