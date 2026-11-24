"""Prepare static L1 terrain or its sparse tile-animation substitutions for the pre-voyage world."""

from __future__ import annotations

import argparse
import json
import shutil
import tempfile
import time
from collections import defaultdict
from pathlib import Path

from PIL import Image, ImageOps

from sf2tool.remake_asset_build import ACCEPTED_ROM_SHA256, ACCEPTED_UPSTREAM_COMMIT
from sf2tool.remake_command_menu import write_json
from sf2tool.remake_hd_actors import encode
from sf2tool.remake_map3_hd import block_images, decode
from sf2tool.remake_map3_ramp import TerrainFilter, load_filter, png_raster
from sf2tool.remake_private_package import package_files, private_output, write_hd_terrain
from sf2tool.remake_terrain_layouts import MAPS, Rect, encounter_regions, projections
from sf2tool.texture_extract import decode_md_4bpp_tile, md_palette_color, palette_index_rgba


def cut(layout, rect):
    return [row[rect.x : rect.x + rect.w] for row in layout[rect.y : rect.y + rect.h]]


def cells(rect):
    for y in range(rect.y, rect.y + rect.h):
        for x in range(rect.x, rect.x + rect.w):
            yield x, y


def crop(image, x, y):
    return image.crop((x * 48, y * 48, (x + 1) * 48, (y + 1) * 48))


class Bank:
    def __init__(self, originals, algorithms, affected=None):
        self.filter = TerrainFilter(originals, algorithms)
        self.affected = affected
        self.centers = [
            self.filter.render(block) if affected is None or i in affected else None
            for i, block in enumerate(self.filter.blocks)
        ]
        self.center_bytes = [im.tobytes() if im is not None else None for im in self.centers]
        self.selected = {}
        self.variants = {}

    def collect(self, layout, plane, dirty, compare):
        pending = defaultdict(list)
        wanted = set()
        for changed in dirty:
            intersection = plane.clipped(changed.halo())
            if intersection:
                wanted.update(cells(intersection))
        whole = self.filter.plane(cut(layout, plane)) if compare and wanted else None
        comparisons = []
        for x, y in sorted(wanted, key=lambda p: (p[1], p[0])):
            key = tuple(
                layout[yy][xx]
                if plane.x <= xx < plane.x + plane.w and plane.y <= yy < plane.y + plane.h
                else -1
                for yy in range(y - 1, y + 2)
                for xx in range(x - 1, x + 2)
            )
            if self.affected is not None and self.affected.isdisjoint(key):
                continue
            if whole is not None:
                comparisons.append((x, y, key))
            if key in self.selected:
                continue
            chunk = Rect(x // 12 * 12, y // 12 * 12, 12, 12)
            pending[chunk].append((x, y, key))
        for samples in pending.values():
            missing = [(x, y, key) for x, y, key in samples if key not in self.selected]
            if missing:
                left, top = min(x for x, _, _ in missing), min(y for _, y, _ in missing)
                right, bottom = max(x for x, _, _ in missing), max(y for _, y, _ in missing)
                patch = Rect(left, top, right - left + 1, bottom - top + 1).halo().clipped(plane)
                image = self.filter.plane(cut(layout, patch))
                for x, y, key in missing:
                    tile = crop(image, x - patch.x, y - patch.y)
                    pixels = tile.tobytes()
                    variant = None if pixels == self.center_bytes[key[4]] else (key[4], pixels)
                    if variant is not None and variant not in self.variants:
                        self.variants[variant] = png_raster(tile)
                    if self.selected.setdefault(key, variant) != variant:
                        raise ValueError("conflicting terrain context")
        for x, y, key in comparisons:
            selected = self.selected[key]
            pixels = self.center_bytes[key[4]] if selected is None else selected[1]
            if pixels != crop(whole, x - plane.x, y - plane.y).tobytes():
                raise ValueError(f"whole-plane comparison failed at {(x, y)}")
        return len(comparisons)

    def publish(self, stage, visual):
        uses = defaultdict(list)
        for key, variant in self.selected.items():
            if variant is not None:
                uses[variant].append(key)
        visual["hdBlocks"] = [png_raster(im) for im in self.centers]
        visual["hdBlockContexts"] = [
            {"raster": raster, "neighbors": sorted(uses[v])} for v, raster in self.variants.items()
        ]
        relative = f"hd/terrain/{visual['map']}.json"
        write_hd_terrain(stage, visual, relative)
        return (stage / relative).stat().st_size


def closure(base):
    return package_files(
        base / "world.json",
        base / "main-scenes.json",
        base / "entry/entry.json",
        base / "opening-inputs",
    )


def build(base, stage, algorithms, pilot):
    files = closure(base)
    doc = json.loads((base / "world.json").read_bytes())
    world = doc["world"]
    maps = {m["id"]: m for m in world["maps"]}
    visuals = {m["map"]: m for m in world["presentation"]["maps"]}
    if Path(visuals["map-3"]["hdTerrainFile"]) not in files:
        raise ValueError("selected base must carry the accepted Map3 companion")
    programs = {p["id"]: p for p in world["programs"]}
    encounters = list(encounter_regions(base, doc))
    for number in (pilot,) if pilot else MAPS[1:]:
        name = f"map-{number}"
        visual = visuals[name]
        if {"hdBlocks", "hdTerrainFile"}.intersection(visual):
            raise ValueError(f"{name} already has HD terrain")
        start = time.perf_counter()
        bank = Bank(list(block_images(visual)), algorithms)
        compared_planes = compared_cells = 0
        checked = set()
        for layout, planes, dirty, label in projections(number, maps[name], programs, encounters):
            for plane in planes:
                # Independently filter actual whole planes for first-visit ordered copies,
                # load-time reconstruction and one large Map66 copy in the calibration.
                check = (
                    pilot
                    and (label.startswith("load-") or label.startswith("script-"))
                    and (label, plane) not in checked
                )
                cells_checked = bank.collect(layout, plane, dirty, check)
                if cells_checked:
                    checked.add((label, plane))
                    compared_planes += 1
                    compared_cells += cells_checked
        size = bank.publish(stage, visual)
        print(
            json.dumps(
                dict(
                    map=name,
                    centers=len(bank.centers),
                    variants=len(bank.variants),
                    keys=len(bank.selected),
                    bytes=size,
                    seconds=round(time.perf_counter() - start, 3),
                    wholePlaneChecks=compared_planes,
                    comparedCells=compared_cells,
                )
            ),
            flush=True,
        )
        del bank
    if pilot:
        return
    write_package(stage, doc, files)


def write_package(stage, doc, files):
    write_json(stage / "world.json", doc)
    if (stage / "world.json").stat().st_size > 128 * 1024 * 1024:
        raise ValueError("world document exceeds admission limit")
    for relative, source in files.items():
        if relative != Path("world.json"):
            target = stage / relative
            target.parent.mkdir(parents=True, exist_ok=True)
            shutil.copyfile(source, target)
    closure(stage)


def phase_blocks(originals, words, entry, stream, colors):
    first, count, target = (
        entry[k] for k in ("replacementStartTile", "tileCount", "targetStartTile")
    )
    tiles = []
    for source in range(first, first + count):
        indices = decode_md_4bpp_tile(stream[source * 32 : (source + 1) * 32])
        tiles.append(Image.frombytes("RGBA", (8, 8), b"".join(colors[i] for i in indices)))
    pieces = list(originals)
    changed = set()
    for block, row in enumerate(words):
        canvas = None
        for pos, word in enumerate(row):
            slot = (word & 1023) - target
            if not 0 <= slot < count:
                continue
            if canvas is None:
                canvas = originals[block].copy()
            tile = tiles[slot]
            if word & 2048:
                tile = ImageOps.mirror(tile)
            if word & 4096:
                tile = ImageOps.flip(tile)
            canvas.paste(tile, (pos % 3 * 8, pos // 3 * 8))
        if canvas is not None and canvas.tobytes() != originals[block].tobytes():
            pieces[block] = canvas
            changed.add(block)
    return pieces, changed


def animation_phase(visual, originals, static, entry, stream, colors, algorithms, footprints):
    pieces, changed = phase_blocks(originals, visual["blocks"], entry, stream, colors)
    phase = dict(
        source=entry["replacementStartTile"], ticks=entry["counter"], blocks=[], contexts=[]
    )
    if not changed:
        return phase
    bank = Bank(pieces, algorithms, changed)
    for block in sorted(changed):
        phase["blocks"].append(
            dict(
                block=block,
                original=encode(pieces[block]),
                hd=png_raster(bank.centers[block]),
            )
        )
    for layout, planes, dirty, _ in footprints:
        for plane in planes:
            bank.collect(layout, plane, dirty, False)
    # A changed center defaults to its new isolated center. Every other center
    # retains the accepted static contextual selection unless its actual edge changed.
    static_context = {
        tuple(key): row["raster"]
        for row in static.get("hdBlockContexts", [])
        for key in row["neighbors"]
    }
    cached = {}
    uses = defaultdict(list)
    for key, variant in bank.selected.items():
        if variant is None:
            continue
        block, pixels = variant
        if block not in changed:
            old = static_context.get(key, static["hdBlocks"][block])
            identity = old["sha256"]
            if identity not in cached:
                image = decode(old)
                cached[identity] = image.resize((48, 48), Image.Resampling.NEAREST).tobytes()
            if pixels == cached[identity]:
                continue
        uses[variant].append(key)
    phase["contexts"] = [
        dict(raster=bank.variants[v], neighbors=sorted(keys)) for v, keys in uses.items()
    ]
    return phase


def build_animations(base, stage, algorithms, library, pilot):
    files = closure(base)
    doc = json.loads((base / "world.json").read_bytes())
    index = json.loads((library / "index.json").read_bytes())
    if (
        index["upstream"] != ACCEPTED_UPSTREAM_COMMIT
        or index["rom"].upper() != ACCEPTED_ROM_SHA256.upper()
    ):
        raise ValueError("prepared map provenance")
    world = doc["world"]
    maps = {m["id"]: m for m in world["maps"]}
    visuals = {m["map"]: m for m in world["presentation"]["maps"]}
    programs = {p["id"]: p for p in world["programs"]}
    encounters = list(encounter_regions(base, doc))
    prepared = {row["id"]: row for row in index["records"]["maps"]}
    for number in (pilot,) if pilot else MAPS:
        row = prepared[number]
        animation = row.get("animations")
        if not animation:
            continue
        name = f"map-{number}"
        visual = visuals[name]
        if "tileAnimation" in visual:
            raise ValueError("map already has tile animation")
        targets = {(e["targetStartTile"], e["tileCount"]) for e in animation["entries"]}
        if len(targets) != 1:
            raise ValueError(
                "this consumer requires a complete replacement of the same range per entry"
            )
        target, count = targets.pop()
        if not any(target <= (w & 1023) < target + count for b in visual["blocks"] for w in b):
            continue
        if visual["blocks"] != json.loads((library / row["blocks"]).read_bytes()):
            raise ValueError("prepared block words differ from the mounted terrain")
        start = time.perf_counter()
        stream = (library / f"tileset-{animation['tileset']}.4bpp").read_bytes()
        capacity = animation["cachedTileCount"]
        if len(stream) != 4096 or not 0 < capacity <= 128:
            raise ValueError("animation cache length")
        stream = stream[: capacity * 32]
        palette = [md_palette_color(w) for w in index["records"]["palettes"][str(row["palette"])]]
        palette[0] = (0, 0, 0)
        colors = [bytes(palette_index_rgba(palette, i)) for i in range(16)]
        static = json.loads((base / visual["hdTerrainFile"]).read_bytes())
        originals = list(block_images(visual))
        phases = [
            animation_phase(
                visual,
                originals,
                static,
                entry,
                stream,
                colors,
                algorithms,
                projections(number, maps[name], programs, encounters),
            )
            for entry in animation["entries"]
        ]
        visual["tileAnimation"] = dict(target=target, count=count, capacity=capacity, phases=phases)
        print(
            json.dumps(
                dict(
                    map=name,
                    phases=[(len(p["blocks"]), len(p["contexts"])) for p in phases],
                    seconds=round(time.perf_counter() - start, 2),
                )
            ),
            flush=True,
        )
    write_package(stage, doc, files)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ("base", "gfx-root", "out"):
        parser.add_argument("--" + name, type=Path, required=True)
    parser.add_argument(
        "--pilot", type=int, choices=(4, 66), help="publish only this measured bank to scratch"
    )
    parser.add_argument(
        "--animation-library",
        type=Path,
        help="prepared retail maps directory for tile substitutions",
    )
    args = parser.parse_args()
    base, out = args.base.resolve(), private_output(args.out.resolve())
    if out.exists() or out.is_relative_to(base):
        raise ValueError("choose a fresh private destination outside the input")
    algorithms = load_filter(args.gfx_root)
    out.parent.mkdir(parents=True, exist_ok=True)
    with tempfile.TemporaryDirectory(dir=out.parent, prefix=".pre-voyage-terrain-") as temp:
        stage = Path(temp) / "pack"
        stage.mkdir()
        if args.animation_library:
            if shutil.disk_usage(stage).free < 1536 * 1024**2:
                raise OSError("insufficient package staging headroom")
            build_animations(base, stage, algorithms, args.animation_library, args.pilot)
        else:
            build(base, stage, algorithms, args.pilot)
        stage.rename(out)


if __name__ == "__main__":
    main()
