"""Bind independently prepared PNG variants in a fresh closed battle-scene package."""

from __future__ import annotations

import argparse
import json
import shutil
from pathlib import Path
from tempfile import TemporaryDirectory

from PIL import Image, ImageChops

from sf2tool.remake_command_menu import write_json
from sf2tool.remake_hd_actors import encode, frame
from sf2tool.remake_map3_hd import decode
from sf2tool.remake_map3_ramp import load_filter
from sf2tool.remake_private_package import package_files, private_output, read_json

_SCENE_LIMIT = 4 * 1024 * 1024
_RASTER_EDGE_LIMIT = 2048


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


def selected_groups(
    scene: dict,
    allies_only: bool = False,
    shared_keys: dict[str, str] | None = None,
) -> list[tuple[str, list[str], int]]:
    """Return existing consumer groups with their Original raster density."""
    groups = []
    rasters = scene["rasters"]
    for actor in scene["actors"]:
        if allies_only and actor["side"] != "ally":
            continue
        # Native 96-square allies are the existing consumer's one-scale exception.
        for key in actor["frames"]:
            row = rasters[key]
            density = (
                1 if actor["side"] == "ally" and (row["width"], row["height"]) == (96, 96) else 2
            )
            groups.append(("body", [key], density))
        # Keep the complete actor frame set together in pilot selection.
        groups.append(("body-group", actor["frames"], 0))
        if actor["weaponFrames"]:
            groups.append(("weapon", actor["weaponFrames"], 2))
    # CampaignScenes retains a destination's existing same-key Original even when
    # its local actor rows do not reference that key. Cover that retained donor too.
    for key, family in (shared_keys or {}).items():
        if key not in rasters:
            continue
        row = rasters[key]
        density = 1 if family == "body" and (row["width"], row["height"]) == (96, 96) else 2
        groups.append((family, [key], density))
        if family == "body":
            groups.append(("body-group", [key], 0))
    if allies_only:
        return groups
    scenery = [scene, *scene.get("terrainScenes", [])]
    for row in scenery:
        groups.append(("background", [row["background"]], 2))
        groups.append(("ground", [row["ground"]], 2))
    if scene.get("healing"):
        heal = scene["healing"]
        groups.append(("healing", heal["bodies"] + heal["wings"] + heal["dust"], 2))
        if enemy := heal.get("enemy"):
            groups.append(("healing-enemy", enemy["bodies"] + enemy["wings"] + enemy["dust"], 2))
    if scene.get("fieldDeath"):
        groups.append(("field-death", scene["fieldDeath"]["exitFrames"], 1))
    effects = {
        "blaze-1": ["frame-0", "frame-1"],
        "blaze-2": ["flame-0", "flame-1", "meteor-0", "meteor-1", "impact-0", "impact-1"],
        "blaze-large": ["flame-0", "flame-1", "meteor-0", "meteor-1", "impact-0", "impact-1"],
        "blaze-dragon": ["head", "body", "tail-0", "tail-1"],
        "freeze": [f"frame-{index}" for index in range(14)],
        "blast-1": [f"frame-{index}" for index in range(15)],
        "detox": [f"frame-{index}" for index in range(11)],
        "debuff-1": [f"frame-{index}" for index in range(3)],
    }
    for family, names in effects.items():
        keys = [family + "/" + name for name in names]
        if any(key in rasters for key in keys):
            groups.append((family, keys, 1))
    return groups


def derive_variants(
    source: Path,
    png_root: Path,
    scale,
    pilot: bool,
    *,
    allies_only: bool = False,
    shared_keys: dict[str, str] | None = None,
) -> list[dict]:
    if source.stat().st_size > _SCENE_LIMIT:
        raise ValueError("input scene exceeds the 4 MiB reader limit")
    scene = read_json(source)
    groups = selected_groups(scene, allies_only, shared_keys)
    selected = {}
    for family, keys, density in groups:
        if density:
            for key in keys:
                previous = selected.setdefault(key, (family, density))
                if previous[1] != density:
                    raise ValueError(f"conflicting consumer density: {key}")
    if pilot:
        included, families = set(), set()
        for family, keys, _ in groups:
            if family == "body":
                continue
            if family not in families:
                included.update(keys)
                families.add(family)
        # Close overlapping frame groups so a pilot never mounts half an actor.
        changed = True
        while changed:
            changed = False
            for _, keys, _ in groups:
                if included.intersection(keys) and not set(keys) <= included:
                    included.update(keys)
                    changed = True
        selected = {key: value for key, value in selected.items() if key in included}
    receipts = []
    for key, (family, density) in selected.items():
        parts = key.split("/")
        if any(part in ("", ".", "..") or "\\" in part or ":" in part for part in parts):
            raise ValueError("raster key cannot be represented as a relative PNG")
        path = png_root.joinpath(*parts[:-1], parts[-1] + ".png")
        if key not in scene["rasters"]:
            raise ValueError(f"missing selected original raster: {key}")
        with decode(scene["rasters"][key]) as original:
            factor = 4 // density
            size = (original.width * factor, original.height * factor)
            if max(size) > _RASTER_EDGE_LIMIT:
                raise ValueError(f"derived raster exceeds the existing edge limit: {key}")
            halves = 2 if family == "field-death" else 1
            if original.width % halves:
                raise ValueError("field-death strip needs two equal frames")
            output = Image.new("RGBA", size)
            methods = []
            width = original.width // halves
            for half in range(halves):
                with original.crop((half * width, 0, (half + 1) * width, original.height)) as part:
                    if family == "background":
                        # The consumer wraps horizontally. Give the same filter the
                        # neighboring repeat on both sides before cropping the center.
                        repeated = Image.new("RGBA", (part.width * 3, part.height))
                        for repeat in range(3):
                            repeated.paste(part, (repeat * part.width, 0))
                        expanded, method = derive_frame(repeated, density, scale)
                        transformed = expanded.crop(
                            (
                                part.width * factor,
                                0,
                                part.width * factor * 2,
                                part.height * factor,
                            )
                        )
                        repeated.close()
                        expanded.close()
                    else:
                        transformed, method = derive_frame(part, density, scale)
                    output.paste(transformed, (half * width * factor, 0))
                    transformed.close()
                    methods.append(method)
            _check_masks(original, output, key)
            path.parent.mkdir(parents=True, exist_ok=True)
            output.save(path, format="PNG")
            output.close()
            receipts.append(
                {
                    "key": key,
                    "family": family,
                    "source": list(original.size),
                    "output": list(size),
                    "sourceDensity": density,
                    "methods": methods,
                }
            )
    return receipts


def derive_frame(image: Image.Image, density: int, scale) -> tuple[Image.Image, str]:
    """Check every 2x2 source cell before recovering a doubled logical frame."""
    logical = image
    method = "native-exact-rgba"
    if density == 2:
        if image.width % 2 == 0 and image.height % 2 == 0:
            reduced = image.resize((image.width // 2, image.height // 2), Image.Resampling.NEAREST)
            restored = reduced.resize(image.size, Image.Resampling.NEAREST)
            reversible = restored.tobytes() == image.tobytes()
            restored.close()
            if reversible:
                logical = reduced
                method = "recovered-nearest-2x"
            else:
                reduced.close()
        if logical is image:
            method = "conservative-source-grid-2x"
    filtered = frame(logical, scale)
    if logical is not image:
        logical.close()
    if method == "conservative-source-grid-2x":
        result = filtered.resize((image.width * 2, image.height * 2), Image.Resampling.NEAREST)
        filtered.close()
        return result, method
    return filtered, method


def prepare(
    base: Path,
    destination: Path,
    scenes: list[tuple[str, Path | None]],
    *,
    scale=None,
    pilot: bool = False,
    shared_allies: bool = False,
) -> dict:
    base, destination = base.resolve(strict=True), private_output(destination)
    files = _files(base)
    allowed = {Path("main-scenes.json")} | {
        Path(row["battleScenes"])
        for row in read_json(files[Path("world.json")]).get("encounters", [])
        if row.get("battleScenes")
    }
    bindings = {}
    for name, png_root in scenes:
        path = Path(name)
        if path not in allowed or path in bindings:
            raise ValueError("select each declared battle scene at most once")
        bindings[path] = png_root.resolve(strict=True) if png_root is not None else None
    donors = allowed - bindings.keys() if shared_allies else set()
    shared_keys = {}
    if shared_allies:
        for relative in sorted(allowed):
            source = files[relative]
            if source.stat().st_size > _SCENE_LIMIT:
                raise ValueError("input scene exceeds the 4 MiB reader limit")
            for actor in read_json(source)["actors"]:
                if actor["side"] != "ally":
                    continue
                for field, family in (("frames", "body"), ("weaponFrames", "weapon")):
                    for key in actor[field]:
                        if shared_keys.setdefault(key, family) != family:
                            raise ValueError(f"shared ally resource has conflicting roles: {key}")
    if destination.exists() or any(
        destination.is_relative_to(root) for root in (base, *bindings.values()) if root is not None
    ):
        raise ValueError("select a fresh output outside input roots")

    destination.parent.mkdir(parents=True, exist_ok=True)
    with TemporaryDirectory(dir=destination.parent, prefix=".hd-battle-") as temp:
        stage = Path(temp) / "pack"
        report = {}
        for relative, source in files.items():
            target = stage / relative
            target.parent.mkdir(parents=True, exist_ok=True)
            if relative in bindings or relative in donors:
                png_root = bindings.get(relative)
                receipts = None
                if png_root is None:
                    png_root = Path(temp) / "pngs" / relative
                    receipts = derive_variants(
                        source,
                        png_root,
                        scale,
                        pilot,
                        allies_only=relative in donors,
                        shared_keys=shared_keys,
                    )
                    if not receipts:
                        shutil.copyfile(source, target)
                        continue
                report[relative.as_posix()] = bind_variants(source, png_root, target)
                report[relative.as_posix()]["scope"] = (
                    "shared-allies" if relative in donors else "selected-scene"
                )
                if receipts is not None:
                    report[relative.as_posix()]["derived"] = receipts
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
        "--scene", action="append", nargs="+", metavar="SCENE_OR_PNG_ROOT", required=True
    )
    parser.add_argument("--derive", action="store_true", help="derive selected consumer groups")
    parser.add_argument("--gfx-root", type=Path, help="pinned graphics implementation for --derive")
    parser.add_argument("--pilot", action="store_true", help="derive one complete group per family")
    parser.add_argument(
        "--shared-allies",
        action="store_true",
        help="also derive only ally body/weapon groups in remaining declared scenes",
    )
    args = parser.parse_args()
    expected = 1 if args.derive else 2
    if any(len(row) != expected for row in args.scene):
        parser.error("--scene takes SCENE with --derive, otherwise SCENE PNG_ROOT")
    if (
        args.derive != (args.gfx_root is not None)
        or (args.pilot or args.shared_allies)
        and not args.derive
    ):
        parser.error("--derive requires --gfx-root; --pilot/--shared-allies require --derive")
    scale = load_filter(args.gfx_root)[1] if args.derive else None
    scenes = [(row[0], None if args.derive else Path(row[1])) for row in args.scene]
    print(
        json.dumps(
            prepare(
                args.base,
                args.out,
                scenes,
                scale=scale,
                pilot=args.pilot,
                shared_allies=args.shared_allies,
            )
        )
    )


if __name__ == "__main__":
    main()
