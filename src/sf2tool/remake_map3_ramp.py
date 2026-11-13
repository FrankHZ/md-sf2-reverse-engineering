"""Publish Map3 L1 terrain from the selected world and pinned project graphics checkout."""

from __future__ import annotations

import argparse
import base64
import hashlib
import io
import json
import shutil
import subprocess
import sys
import tempfile
from collections import defaultdict
from pathlib import Path

from PIL import Image

from sf2tool.remake_command_menu import world_dependencies, write_json
from sf2tool.remake_map3_hd import NEAREST, block_images, comparison, compose

GFX_COMMIT = "172662709b31cb296b55f51894754ce810c30f6c"


def load_filter(root):
    # The gfx project owns these algorithms. Import the pinned checkout offline only.
    def git(*args):
        return subprocess.check_output(["git", "-C", root, *args], text=True).strip()

    if git("rev-parse", "HEAD") != GFX_COMMIT or git("status", "--porcelain", "--", "src/remaster"):
        raise ValueError("select the fixed, clean graphics implementation")
    sys.dont_write_bytecode = True
    sys.path.insert(0, str(root.resolve() / "src"))
    from remaster.ramp import expand_ramp_dither
    from remaster.scale2x import edge_aware_2x

    return expand_ramp_dither, edge_aware_2x


def png_raster(image):
    # Exact PNG: keep every used intermediate and the distinct opaque black / hole.
    stream = io.BytesIO()
    image.resize((96, 96), NEAREST).save(stream, format="PNG")
    payload = stream.getvalue()
    return dict(
        width=96,
        height=96,
        format="png",
        sha256=hashlib.sha256(payload).hexdigest(),
        data=base64.b64encode(payload).decode("ascii"),
    )


class TerrainFilter:
    def __init__(self, originals, algorithms):
        self.expand, self.scale = algorithms
        self.palette = sorted(set().union(*(set(block.getdata()) for block in originals)))
        index = {rgba: i for i, rgba in enumerate(self.palette)}
        self.hole = index[(0, 0, 0, 0)]
        self.colors = [rgba[:3] for rgba in self.palette]
        self.blocks = [
            Image.frombytes("L", block.size, bytes(index[p] for p in block.getdata()))
            for block in originals
        ]

    def render(self, indices):
        width, height = indices.size
        flat = indices.tobytes()
        grid = [flat[y : y + width] for y in range(0, len(flat), width)]
        ramp = self.expand(grid, self.colors, exclude_indices={self.hole})
        rows = self.scale(ramp.new_grid)
        colors = [
            *self.palette,
            *[(*rgb, 255) for rgb in ramp.expanded_palette[len(self.palette) :]],
        ]
        result = Image.frombytes("P", (width * 2, height * 2), b"".join(rows))
        result.putpalette(bytes(channel for color in colors for channel in color), rawmode="RGBA")
        return result.convert("RGBA")

    def plane(self, ids):
        image = Image.new("L", (len(ids[0]) * 24, len(ids) * 24), self.hole)
        for y, row in enumerate(ids):
            for x, block in enumerate(row):
                image.paste(self.blocks[block], (x * 24, y * 24))
        return self.render(image)


def neighborhood(ids, x, y):
    # Outside the plane is -1, distinct from a real transparent cell (block zero).
    return tuple(
        ids[ny][nx] if 0 <= nx < len(ids[0]) and 0 <= ny < len(ids) else -1
        for ny in range(y - 1, y + 2)
        for nx in range(x - 1, x + 2)
    )


def publish_terrain(visual, map_def, filter_, review):
    base = [filter_.render(block) for block in filter_.blocks]
    visual["hdBlocks"] = [png_raster(image) for image in base]
    # One texture may serve many neighborhoods. Intern exact pixels, independent of PNG encoding.
    variants = {}
    uses = defaultdict(set)
    initial = map_def["layout"]
    closed = [row.copy() for row in initial]
    for event in map_def["layoutEvents"]["roofs"]:
        if event["clear"]:
            continue
        copy = event["copy"]
        sx, sy = copy["source"]["x"], copy["source"]["y"]
        dx, dy = copy["destination"]["x"], copy["destination"]["y"]
        for i in range(copy["height"]):
            closed[dy + i][dx : dx + copy["width"]] = initial[sy + i][sx : sx + copy["width"]]

    def plane(layout, area, ox, oy, mutations=()):
        x0, y0 = area["minX"] + ox, area["minY"] + oy
        x1, y1 = area["maxX"] + ox + 1, area["maxY"] + oy + 1
        ids = [[word & 1023 for word in row[x0:x1]] for row in layout[y0:y1]]

        def collect(x, y, tile):
            block = ids[y][x]
            if (block == 0 and (ox or oy)) or tile == base[block]:
                return
            key = (block, tile.tobytes())
            if key not in variants:
                variants[key] = png_raster(tile)
            uses[key].add(neighborhood(ids, x, y))

        def collect_region(left, top, right, bottom, image, origin_x, origin_y):
            for y in range(top, bottom):
                for x in range(left, right):
                    px, py = (x - origin_x) * 48, (y - origin_y) * 48
                    collect(x, y, image.crop((px, py, px + 48, py + 48)))

        image = filter_.plane(ids)
        cols, rows = len(ids[0]), len(ids)
        collect_region(0, 0, cols, rows, image, 0, 0)
        # A source-defined layout copy changes the destination and one cell of neighbors.
        # Filter only that patch plus its one-cell sampling halo, on the same plane.
        for event in mutations:
            copy = event["copy"]
            dx = copy["destination"]["x"] - x0
            dy = copy["destination"]["y"] - y0
            width, height = copy["width"], copy["height"]
            if not (0 <= dx <= cols - width and 0 <= dy <= rows - height):
                continue
            previous = [row[dx : dx + width] for row in ids[dy : dy + height]]
            sx, sy = copy["source"]["x"], copy["source"]["y"]
            for j in range(height):
                ids[dy + j][dx : dx + width] = (
                    [0] * width
                    if event.get("clear")
                    else [v & 1023 for v in initial[sy + j][sx : sx + width]]
                )
            left, top = max(0, dx - 1), max(0, dy - 1)
            right, bottom = min(cols, dx + width + 1), min(rows, dy + height + 1)
            origin_x, origin_y = max(0, left - 1), max(0, top - 1)
            patch = [row[origin_x : right + 1] for row in ids[origin_y : bottom + 1]]
            collect_region(left, top, right, bottom, filter_.plane(patch), origin_x, origin_y)
            for j, old in enumerate(previous):
                ids[dy + j][dx : dx + width] = old
        return image

    for area in map_def["areas"]:
        events = map_def["layoutEvents"]
        ground = plane(initial, area, 0, 0, [*events["doors"], *events["flags"]])
        ox, oy = area["overlay"]["x"], area["overlay"]["y"]
        if ox or oy:
            plane(initial, area, ox, oy, events["roofs"])
            roof = plane(closed, area, ox, oy)
            ground.alpha_composite(roof)
        if area == map_def["areas"][0]:
            ground.save(review / "exterior-l1.png")
    visual["hdBlockContexts"] = [
        {"raster": raster, "neighbors": sorted(uses[key])} for key, raster in variants.items()
    ]


def build(world, scene, out, algorithms):
    document = json.loads(world.read_bytes())
    world_data = document["world"]
    visual = next(row for row in world_data["presentation"]["maps"] if row["map"] == "map-3")
    map_def = next(row for row in world_data["maps"] if row["id"] == "map-3")
    originals = list(block_images(visual))
    review = out / "review"
    review.mkdir(parents=True)
    publish_terrain(visual, map_def, TerrainFilter(originals, algorithms), review)

    original = compose(map_def, originals, map_def["areas"][0])
    with Image.open(review / "exterior-l1.png") as hd:
        for name, bounds in (
            ("grove", (10, 15, 16, 21)),
            ("riverbank", (20, 14, 28, 18)),
            ("roof", (29, 8, 34, 15)),
            ("continue", (26, 9, 38, 17)),
        ):
            left = original.crop(tuple(v * 24 for v in bounds))
            right = hd.crop(tuple(v * 48 for v in bounds))
            backing = Image.new("RGB", right.size)
            backing.paste(right, mask=right.getchannel("A"))
            comparison(
                left.resize(right.size, NEAREST), backing, review / f"{name}.png", name.upper()
            )
    dependencies = world_dependencies(document, world)
    if dependencies.intersection({"world.json", "scene.json", "review"}):
        raise ValueError("dependency collides with publication output")
    for relative in sorted(dependencies):
        target = out / relative
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(world.parent / relative, target)
    shutil.copyfile(scene, out / "scene.json")
    write_json(out / "world.json", document)
    if (out / "world.json").stat().st_size > 128 * 1024 * 1024:
        raise ValueError("ramp world exceeds the private document limit")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ("world", "scene", "gfx-root", "out"):
        parser.add_argument("--" + name, type=Path, required=True)
    args = parser.parse_args()
    if args.out.exists():
        raise ValueError("choose a fresh private output")
    algorithms = load_filter(args.gfx_root)
    args.out.parent.mkdir(parents=True, exist_ok=True)
    with tempfile.TemporaryDirectory(dir=args.out.parent, prefix=".map3-ramp-") as folder:
        stage = Path(folder) / "pack"
        build(args.world, args.scene, stage, algorithms)
        stage.rename(args.out)


if __name__ == "__main__":
    main()
