"""Publish prepared DETOX tiles into a complete private battle-scene package."""

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
    expected = dict(family="spell", id=7, tiles="spell-7.4bpp", palette="08880aaa0ccc")
    if (
        index["rom"] != ACCEPTED_ROM_SHA256
        or index["upstream"] != ACCEPTED_UPSTREAM_COMMIT
        or [row for row in index["records"] if row.get("family") == "spell" and row["id"] == 7]
        != [expected]
    ):
        raise ValueError("registered DETOX effects identity differs")
    tiles, palette = prepared_spell_tiles(directory, 7)
    if len(tiles) != 1056:
        raise ValueError("registered DETOX tile stream length differs")
    # Initial SPELLTILE28, growth/shrink tables 1CE0C/1CE2A, then SPELLTILE29–33.
    frames = [(27, 1), (26, 1), (22, 2), (18, 2), (9, 3), (0, 3)]
    frames += [(index, 1) for index in range(28, 33)]
    originals, variants = {}, {}
    for slot, (first, side) in enumerate(frames):
        key = f"detox/frame-{slot}"
        size = side * 8
        layout = [first + x * side + y for y in range(side) for x in range(side)]
        pixels = compose(tiles, palette, size, size, layout)
        with Image.frombytes("RGBA", (size, size), bytes(pixels)) as original:
            hd, _ = derive_frame(original, 1, scale)
            with hd:
                _check_masks(original, hd, key)
                originals[key], variants[key] = encode(original), encode(hd)
    return originals, variants


def publish(base, output, originals, variants):
    base = base.resolve(strict=True)
    output = private_output(output)
    if output.exists() or output.is_relative_to(base):
        raise ValueError("select a fresh output outside the base package")
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
            if relative not in scenes:
                shutil.copyfile(source, target)
                continue
            scene = read_json(source)
            for bank, additions in (("rasters", originals), ("hdRasters", variants)):
                current = scene.setdefault(bank, {})
                for key, value in additions.items():
                    if current.setdefault(key, value) != value:
                        raise ValueError(f"conflicting DETOX raster {key}")
            target.write_text(json.dumps(scene, separators=(",", ":")), encoding="utf-8")
            if target.stat().st_size > 4 * 1024 * 1024:
                raise ValueError("scene exceeds the existing 4 MiB reader limit")
            mounted.append(relative.as_posix())
        package.rename(output)
    return {
        "scenes": sorted(mounted),
        "files": len(files),
        "inputBytes": sum(path.stat().st_size for path in files.values()),
        "outputBytes": sum((output / path).stat().st_size for path in files),
        "maxSceneBytes": max((output / path).stat().st_size for path in mounted),
    }


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ("base", "effects", "output", "gfx-root"):
        parser.add_argument("--" + name, type=Path, required=True)
    args = parser.parse_args()
    originals, variants = artwork(args.effects, load_filter(args.gfx_root)[1])
    result = publish(args.base, args.output, originals, variants)
    print(json.dumps(result))


if __name__ == "__main__":
    main()
