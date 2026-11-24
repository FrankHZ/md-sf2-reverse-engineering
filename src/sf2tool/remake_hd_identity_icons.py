"""Prepare switchable UI raster variants from an admitted private package."""

from __future__ import annotations

import argparse
import base64
import hashlib
import json
import shutil
import tempfile
from pathlib import Path

from sf2tool.remake_command_menu import world_dependencies, write_json


def herb_frame(raster: dict) -> tuple[dict, dict]:
    """One Scale2x pass on exact RGBA values, then nearest 2x; no palette blending."""
    raw = base64.b64decode(raster["data"], validate=True)
    if hashlib.sha256(raw).hexdigest().lower() != raster["sha256"].lower():
        raise ValueError("original herb payload identity mismatch")
    if (raster["width"], raster["height"], raster["format"]) != (16, 24, "rgba8") or len(
        raw
    ) != 1536:
        raise ValueError("herb derivation requires the admitted original 16x24 RGBA8 frame")
    pixels = [raw[i : i + 4] for i in range(0, len(raw), 4)]
    output = bytearray(64 * 96 * 4)
    for y in range(24):
        for x in range(16):
            center = pixels[y * 16 + x]
            above, below = pixels[max(0, y - 1) * 16 + x], pixels[min(23, y + 1) * 16 + x]
            left, right = pixels[y * 16 + max(0, x - 1)], pixels[y * 16 + min(15, x + 1)]
            values = (center,) * 4
            if above != below and left != right:
                values = (
                    left if left == above else center,
                    right if above == right else center,
                    left if left == below else center,
                    right if below == right else center,
                )
            for dy in range(4):
                for dx in range(4):
                    offset = ((y * 4 + dy) * 64 + x * 4 + dx) * 4
                    output[offset : offset + 4] = values[(dy // 2) * 2 + dx // 2]
    result = dict(
        width=64,
        height=96,
        format="rgba8",
        data=base64.b64encode(output).decode("ascii"),
        sha256=hashlib.sha256(output).hexdigest(),
    )
    return result, dict(originalSha256=raster["sha256"], hdSha256=result["sha256"])


def prepare(world: Path, scene: Path, out_dir: Path) -> None:
    if out_dir.exists():
        raise ValueError("use a fresh output directory; previous publications are read-only")
    document = json.loads(world.read_text(encoding="utf-8-sig"))
    presentation = document.get("world", document)["presentation"]
    if "hdIdentityIcons" in presentation:
        raise ValueError("input already declares HD identity art")
    pair, receipts = {}, {}
    for state in ("normal", "selected"):
        key = f"identity-icon.item.0.{state}"
        pair[key], receipts[key] = herb_frame(presentation["identityIcons"][key])
    dependencies = world_dependencies(document, world)
    if dependencies.intersection({"world.json", "scene.json", "hd-herb-provenance.json"}):
        raise ValueError("dependency collides with publication output")
    if not scene.is_file():
        raise ValueError("opening scene missing")
    # One isolated package. Copy dependency bytes without extracting or regenerating scenes/audio.
    out_dir.mkdir(parents=True, exist_ok=False)
    for relative in sorted(dependencies):
        destination = out_dir / relative
        destination.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(world.resolve().parent / relative, destination)
    shutil.copyfile(scene, out_dir / "scene.json")
    presentation["hdIdentityIcons"] = pair
    write_json(out_dir / "world.json", document)
    write_json(
        out_dir / "hd-herb-provenance.json",
        dict(
            algorithm="Scale2x RGBA equality, clamped borders, then nearest 2x",
            scope="field inventory/preparation only; no battle variants",
            sourceProvenance=document.get("provenance"),
            resources=receipts,
            copiedDependencies=sorted(dependencies),
            humanArtAcceptance="Unknown; Epic #1699",
        ),
    )


_WORLD_UI = {
    "identityIcons": ("hdIdentityIcons", (16, 24)),
    "fieldMenu": ("hdFieldMenu", (24, 24)),
    "itemMenu": ("hdItemMenu", (24, 24)),
    "shopMenu": ("hdShopMenu", (24, 24)),
    "churchMenu": ("hdChurchMenu", (24, 24)),
}
_SCENE_UI = {
    "identity-icon.": (16, 24),
    "command-menu.": (24, 24),
    "battlefield-menu.": (24, 24),
    "battle-cursor.": (32, 32),
}


def prepare_package(base: Path, destination: Path, scenes: list[Path], gfx_root: Path) -> dict:
    from PIL import Image

    from sf2tool.remake_hd_actors import encode, frame
    from sf2tool.remake_map3_hd import decode
    from sf2tool.remake_map3_ramp import load_filter
    from sf2tool.remake_private_package import package_files, private_output, read_json

    def closure(root):
        return package_files(
            root / "world.json",
            root / "main-scenes.json",
            root / "entry/entry.json",
            root / "opening-inputs",
        )

    base, destination = base.resolve(strict=True), private_output(destination)
    if destination.exists() or destination.is_relative_to(base):
        raise ValueError("use a fresh output outside the source package")
    if not scenes or len(set(scenes)) != len(scenes):
        raise ValueError("select each declared scene at most once")
    files = closure(base)
    document = read_json(files[Path("world.json")])
    world = document.get("world", document)
    allowed = {Path("main-scenes.json")} | {
        Path(row["battleScenes"])
        for row in document.get("encounters", [])
        if row.get("battleScenes")
    }
    if any(scene not in allowed or scene not in files for scene in scenes):
        raise ValueError("select a declared render scene")
    scale = load_filter(gfx_root)[1]
    prepared = {}

    def bind(original, size, *, nearest=False):
        if (original["width"], original["height"]) != size:
            raise ValueError("unexpected original UI raster size")
        # Identical source bytes can back different scene keys. Validate each distinct
        # envelope once without trusting a supplied digest on a cache hit.
        key = (size, nearest, original["format"], original["sha256"], original["data"])
        if key not in prepared:
            with decode(original) as image:
                variant = (
                    image.resize((size[0] * 4, size[1] * 4), Image.Resampling.NEAREST)
                    if nearest
                    else frame(image, scale)
                )
                with variant:
                    prepared[key] = encode(variant)
        return prepared[key]

    presentation = world["presentation"]
    world_counts = {}
    for family, (field, size) in _WORLD_UI.items():
        originals = presentation.get(family)
        if originals:
            variants = presentation.setdefault(field, {})
            for key, original in originals.items():
                if key not in variants:
                    variants[key] = bind(original, size)
            world_counts[field] = len(variants)

    destination.parent.mkdir(parents=True, exist_ok=True)
    with tempfile.TemporaryDirectory(dir=destination.parent, prefix=".hd-ui-") as temp:
        stage = Path(temp) / "pack"
        report = {}
        selected = set(scenes)
        for relative, source in files.items():
            target = stage / relative
            target.parent.mkdir(parents=True, exist_ok=True)
            if relative == Path("world.json"):
                write_json(target, document)
                if target.stat().st_size > 120 * 1024 * 1024:
                    raise ValueError("world publication exceeds the 120 MiB planning ceiling")
            elif relative in selected:
                if source.stat().st_size > 4 * 1024 * 1024:
                    raise ValueError("scene input exceeds the 4 MiB reader limit")
                scene = read_json(source)
                variants = scene.setdefault("hdRasters", {})
                count = 0
                for key, original in scene["rasters"].items():
                    prefix = key.partition(".")[0] + "."
                    if prefix not in _SCENE_UI:
                        continue
                    if key not in variants:
                        variants[key] = bind(
                            original, _SCENE_UI[prefix], nearest=prefix == "battle-cursor."
                        )
                    count += 1
                if not count:
                    raise ValueError("selected scene has no mounted UI rasters")
                write_json(target, scene)
                size = target.stat().st_size
                if size > 4 * 1024 * 1024:
                    raise ValueError("scene publication exceeds the 4 MiB reader limit")
                report[relative.as_posix()] = {"variants": count, "sceneBytes": size}
            else:
                shutil.copyfile(source, target)
        world_bytes = (stage / "world.json").stat().st_size
        del document, world, presentation, originals, variants
        closed = closure(stage)
        if closed.keys() != files.keys():
            raise ValueError("output dependency set differs from input package")
        package_bytes = sum(path.stat().st_size for path in closed.values())
        stage.rename(destination)
    return {
        "world": world_counts,
        "worldBytes": world_bytes,
        "scenes": report,
        "uniqueDerived": len(prepared),
        "files": len(files),
        "packageBytes": package_bytes,
    }


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    mode = parser.add_mutually_exclusive_group(required=True)
    mode.add_argument("--world", type=Path)
    mode.add_argument("--base", type=Path)
    parser.add_argument("--scene", type=Path, action="append", required=True)
    parser.add_argument("--out-dir", type=Path, required=True)
    parser.add_argument("--gfx-root", type=Path)
    args = parser.parse_args()
    if args.base is not None:
        if args.gfx_root is None:
            parser.error("--base requires --gfx-root")
        print(json.dumps(prepare_package(args.base, args.out_dir, args.scene, args.gfx_root)))
    else:
        if args.gfx_root is not None or len(args.scene) != 1:
            parser.error("--world takes one --scene and uses the established herb treatment")
        prepare(args.world, args.scene[0], args.out_dir)


if __name__ == "__main__":
    main()
