"""Publish one private field Medical Herb HD pair from an admitted original pack."""

from __future__ import annotations

import argparse
import base64
import hashlib
import json
import shutil
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


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ("world", "scene", "out-dir"):
        parser.add_argument("--" + name, type=Path, required=True)
    args = parser.parse_args()
    prepare(args.world, args.scene, args.out_dir)


if __name__ == "__main__":
    main()
