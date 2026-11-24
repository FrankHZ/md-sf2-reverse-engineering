"""Prepare switchable shared actor and portrait banks from a closed private package."""

from __future__ import annotations

import argparse
import base64
import hashlib
import io
import json
import shutil
import tempfile
from pathlib import Path

from PIL import Image

from sf2tool.remake_command_menu import write_json
from sf2tool.remake_map3_hd import decode
from sf2tool.remake_map3_ramp import load_filter
from sf2tool.remake_private_package import package_files, private_output


def frame(image, scale):
    """Preserve source coverage/black masks; interpolate the remaining palette colors."""
    pixels = list(image.getdata())
    palette = sorted(set(pixels))
    if len(palette) > 256:
        raise ValueError("actor source exceeds its indexed palette")
    indexes = {color: i for i, color in enumerate(palette)}
    w, h = image.size
    grid = [[indexes[pixels[y * w + x]] for x in range(w)] for y in range(h)]
    expanded = scale(grid)
    out = Image.new("RGBA", (w * 2, h * 2))
    result = []
    black = (0, 0, 0, 255)
    for y, row in enumerate(expanded):
        for x, index in enumerate(row):
            source = pixels[(y // 2) * w + x // 2]
            color = palette[index]
            # The renderer's actor coverage and shadow word masks are source-pixel
            # masks. Keep those boundaries exact; don't manufacture a dark fringe.
            if source[3] == 0 or source == black or color[3] != source[3] or color == black:
                color = source
            result.append(color)
    out.putdata(result)
    return out.resize((w * 4, h * 4), Image.Resampling.NEAREST)


def encode(image):
    stream = io.BytesIO()
    image.save(stream, format="PNG")
    payload = stream.getvalue()
    return dict(
        width=image.width,
        height=image.height,
        format="png",
        sha256=hashlib.sha256(payload).hexdigest(),
        data=base64.b64encode(payload).decode("ascii"),
    )


def strip(row, scale):
    original = decode(row)
    size = original.height
    if size not in (24, 48) or original.width != size * 2:
        raise ValueError("expected two square actor frames")
    out = Image.new("RGBA", (original.width * 4, size * 4))
    for half in range(2):
        out.paste(
            frame(original.crop((half * size, 0, (half + 1) * size, size)), scale),
            (half * size * 4, 0),
        )
    return encode(out)


def portrait(row, scale):
    original = decode(row)
    if original.size != (64, 64):
        raise ValueError("expected the admitted eight-by-eight portrait atlas")
    out = Image.new("RGBA", (256, 256))
    # Each base/replacement tile has the same local treatment. Composition and
    # mirroring remain with the existing runtime, including both spare columns.
    for y in range(0, 64, 8):
        for x in range(0, 64, 8):
            out.paste(frame(original.crop((x, y, x + 8, y + 8)), scale), (x * 4, y * 4))
    return encode(out)


def prepare(base, stage, scale, pilot):
    files = package_files(
        base / "world.json",
        base / "main-scenes.json",
        base / "entry/entry.json",
        base / "opening-inputs",
    )
    document = json.loads((base / "world.json").read_bytes())
    presentation = document["world"]["presentation"]
    counts = dict(sprites=0, directionBindings=0, alternateGroups=0, portraits=0)
    # The immutable shared bank is already admitted. Covering it is cheaper and
    # less fragile than a second conditional story/appearance selector registry.
    strips, portraits = {}, {}
    for actor in presentation["sprites"]:
        if pilot and actor["sprite"] not in (0, 59, 252):
            continue

        def directions(rows):
            if len(rows) != 3:
                raise ValueError("expected a complete three-direction group")
            result = []
            for row in rows:
                identity = row["sha256"]
                if identity not in strips:
                    strips[identity] = strip(row, scale)
                result.append(strips[identity])
            counts["directionBindings"] += len(rows)
            return result

        actor["hdDirections"] = directions(actor["directions"])
        if actor.get("graphics"):
            actor["hdGraphics"] = {key: directions(rows) for key, rows in actor["graphics"].items()}
            counts["alternateGroups"] += len(actor["graphics"])
        counts["sprites"] += 1
    for face in presentation["portraits"]:
        if pilot and face["portrait"] != 5:
            continue
        row = face["raster"]
        identity = row["sha256"]
        if identity not in portraits:
            portraits[identity] = portrait(row, scale)
        face["hdRaster"] = portraits[identity]
        counts["portraits"] += 1
    write_json(stage / "world.json", document)
    size = (stage / "world.json").stat().st_size
    if size > 128 * 1024 * 1024:
        raise ValueError("world exceeds the existing 128 MiB admission limit")
    for relative, source in files.items():
        if relative == Path("world.json"):
            continue
        destination = stage / relative
        destination.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(source, destination)
    closed = package_files(
        stage / "world.json",
        stage / "main-scenes.json",
        stage / "entry/entry.json",
        stage / "opening-inputs",
    )
    print(
        json.dumps(
            {
                **counts,
                "uniqueStrips": len(strips),
                "uniquePortraits": len(portraits),
                "worldBytes": size,
                "files": len(closed),
                "packageBytes": sum(path.stat().st_size for path in closed.values()),
            }
        )
    )


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ("base", "gfx-root", "out"):
        parser.add_argument("--" + name, type=Path, required=True)
    parser.add_argument("--pilot", action="store_true")
    args = parser.parse_args()
    base, out = args.base.resolve(), private_output(args.out.resolve())
    if out.exists() or out.is_relative_to(base):
        raise ValueError("choose a fresh private destination outside the input")
    _, scale = load_filter(args.gfx_root)
    out.parent.mkdir(parents=True, exist_ok=True)
    with tempfile.TemporaryDirectory(dir=out.parent, prefix=".hd-actors-") as temp:
        stage = Path(temp) / "pack"
        stage.mkdir()
        prepare(base, stage, scale, args.pilot)
        stage.rename(out)


if __name__ == "__main__":
    main()
