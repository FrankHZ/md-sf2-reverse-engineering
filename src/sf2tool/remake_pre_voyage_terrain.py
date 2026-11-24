"""Publish the closed pre-voyage static L1 terrain selection or measure pilot banks."""

from __future__ import annotations

import argparse
import json
import shutil
import tempfile
import time
from collections import defaultdict
from pathlib import Path

from sf2tool.remake_command_menu import write_json
from sf2tool.remake_map3_hd import block_images
from sf2tool.remake_map3_ramp import TerrainFilter, load_filter, png_raster
from sf2tool.remake_private_package import package_files, private_output, write_hd_terrain
from sf2tool.remake_terrain_layouts import MAPS, Rect, encounter_regions, projections


def cut(layout, rect):
    return [row[rect.x : rect.x + rect.w] for row in layout[rect.y : rect.y + rect.h]]


def cells(rect):
    for y in range(rect.y, rect.y + rect.h):
        for x in range(rect.x, rect.x + rect.w):
            yield x, y


def crop(image, x, y):
    return image.crop((x * 48, y * 48, (x + 1) * 48, (y + 1) * 48))


class Bank:
    def __init__(self, visual, algorithms):
        self.filter = TerrainFilter(list(block_images(visual)), algorithms)
        self.centers = [self.filter.render(block) for block in self.filter.blocks]
        self.center_bytes = [im.tobytes() for im in self.centers]
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
        bank = Bank(visual, algorithms)
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
    write_json(stage / "world.json", doc)
    if (stage / "world.json").stat().st_size > 128 * 1024 * 1024:
        raise ValueError("world document exceeds admission limit")
    for relative, source in files.items():
        if relative != Path("world.json"):
            target = stage / relative
            target.parent.mkdir(parents=True, exist_ok=True)
            shutil.copyfile(source, target)
    closure(stage)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ("base", "gfx-root", "out"):
        parser.add_argument("--" + name, type=Path, required=True)
    parser.add_argument(
        "--pilot", type=int, choices=(4, 66), help="publish only this measured bank to scratch"
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
        build(base, stage, algorithms, args.pilot)
        stage.rename(out)


if __name__ == "__main__":
    main()
