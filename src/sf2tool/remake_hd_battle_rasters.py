"""Bind independently prepared PNG variants in a fresh closed battle-scene package."""

from __future__ import annotations

import argparse
import json
import shutil
from pathlib import Path
from tempfile import TemporaryDirectory

from PIL import Image, ImageChops

from sf2tool.remake_command_menu import write_json
from sf2tool.remake_hd_actors import encode
from sf2tool.remake_map3_hd import decode
from sf2tool.remake_private_package import package_files, private_output, read_json

_SCENE_LIMIT = 4 * 1024 * 1024
_RASTER_EDGE_LIMIT = 2048
_OPENING_SCENE = Path("opening-inputs/SF2_PRIVATE_BATTLE01_SCENE.json")


def _files(root: Path) -> dict[Path, Path]:
    return package_files(
        root / "world.json",
        root / "main-scenes.json",
        root / "entry/entry.json",
        root / "opening-inputs",
    )


def _opaque_black(image: Image.Image, alpha: Image.Image) -> Image.Image:
    red, green, blue = (image.getchannel(channel) for channel in "RGB")
    brightest = ImageChops.lighter(ImageChops.lighter(red, green), blue)
    return ImageChops.multiply(brightest.point([255] + [0] * 255), alpha.point([0] * 255 + [255]))


def _check_masks(source: Image.Image, variant: Image.Image, key: str) -> None:
    original_alpha = source.getchannel("A")
    variant_alpha = variant.getchannel("A")
    original_black = _opaque_black(source, original_alpha)
    variant_black = _opaque_black(variant, variant_alpha)
    for original, actual in ((original_alpha, variant_alpha), (original_black, variant_black)):
        expected = original.resize(variant.size, Image.Resampling.NEAREST)
        if ImageChops.difference(expected, actual).getbbox() is not None:
            raise ValueError(f"variant changes the alpha or opaque black mask: {key}")


def bind_variants(source: Path, png_root: Path, target: Path) -> dict[str, int]:
    if source.stat().st_size > _SCENE_LIMIT:
        raise ValueError("input scene exceeds the 4 MiB reader limit")
    scene = read_json(source)
    if "hdRasters" in scene:
        raise ValueError("input already declares battle scene HD rasters")
    originals = scene["rasters"]
    variants = {}
    for path in sorted(png_root.rglob("*.png")):
        rel = path.relative_to(png_root)
        key = rel.with_suffix("").as_posix()
        if path.resolve() != path or key not in originals or key in variants:
            raise ValueError(f"variant must bind a unique original key: {key}")
        row = originals[key]
        width, height = row["width"], row["height"]
        with Image.open(path, formats=["PNG"]) as image:
            if max(image.size) > _RASTER_EDGE_LIMIT or image.size not in (
                (2 * width, 2 * height),
                (4 * width, 4 * height),
            ):
                raise ValueError(f"variant needs equal-axis 2× or 4× dimensions: {key}")
            with decode(row) as original, image.convert("RGBA") as variant:
                _check_masks(original, variant, key)
                variants[key] = encode(variant)
    if not variants:
        raise ValueError("no bound PNG variants")
    scene["hdRasters"] = variants
    write_json(target, scene)
    size = target.stat().st_size
    if size > _SCENE_LIMIT:
        raise ValueError("output scene exceeds the 4 MiB reader limit")
    return {"variants": len(variants), "originalBytes": source.stat().st_size, "sceneBytes": size}


def prepare(base: Path, destination: Path, scenes: list[tuple[str, Path]]) -> dict:
    base, destination = base.resolve(strict=True), private_output(destination)
    files = _files(base)
    allowed = {Path("main-scenes.json"), _OPENING_SCENE} | {
        Path(row["battleScenes"])
        for row in read_json(files[Path("world.json")]).get("encounters", [])
        if row.get("battleScenes")
    }
    bindings = {}
    for name, png_root in scenes:
        path = Path(name)
        if path not in allowed or path in bindings:
            raise ValueError("select each declared battle scene at most once")
        bindings[path] = png_root.resolve(strict=True)
    if destination.exists() or any(
        destination.is_relative_to(root) for root in (base, *bindings.values())
    ):
        raise ValueError("select a fresh output outside input roots")

    destination.parent.mkdir(parents=True, exist_ok=True)
    with TemporaryDirectory(dir=destination.parent, prefix=".hd-battle-") as temp:
        stage = Path(temp) / "pack"
        report = {}
        for relative, source in files.items():
            target = stage / relative
            target.parent.mkdir(parents=True, exist_ok=True)
            if relative in bindings:
                report[relative.as_posix()] = bind_variants(source, bindings[relative], target)
            else:
                shutil.copyfile(source, target)
        closed = _files(stage)
        if closed.keys() != files.keys():
            raise ValueError("output dependency set differs from input package")
        size = sum(path.stat().st_size for path in closed.values())
        stage.rename(destination)
    return {"scenes": report, "files": len(closed), "packageBytes": size}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--base", type=Path, required=True)
    parser.add_argument("--out", type=Path, required=True)
    parser.add_argument(
        "--scene", action="append", nargs=2, metavar=("SCENE", "PNG_ROOT"), required=True
    )
    args = parser.parse_args()
    print(json.dumps(prepare(args.base, args.out, [(key, Path(root)) for key, root in args.scene])))


if __name__ == "__main__":
    main()
