"""Complete the independent opening's HD banks from its published indexed inputs."""

from __future__ import annotations

import argparse
import base64
import copy
import json
import shutil
import time
from pathlib import Path

from PIL import Image

from sf2tool.remake_hd_actors import strip
from sf2tool.remake_map3_hd import block_images
from sf2tool.remake_map3_ramp import TerrainFilter, load_filter, png_raster, publish_terrain
from sf2tool.remake_opening_scene import raster, write
from sf2tool.remake_private_package import private_output, read_json
from sf2tool.texture_extract import md_palette_color


def offset(words, value):
    return [
        sum(max(0, min(14, ((word >> shift) & 14) + value * 2)) << shift for shift in (0, 4, 8))
        for word in words
    ]


def reduced(previous, target):
    return [
        sum(
            ((((a >> shift) & 14) * 3 + ((b >> shift) & 14)) // 4 & 14) << shift
            for shift in (0, 4, 8)
        )
        for a, b in zip(previous, target, strict=True)
    ]


class IndexedTerrain(TerrainFilter):
    """Freeze base-palette geometry; recolor ramp endpoints by their original slots.

    RGB equality cannot identify a slot. The pinned filter operates on these actual
    indices, and its ordered levels_added result retains each generated ramp's pair.
    Palette changes only recolor this fixed geometry; they never reclassify edges.
    """

    def __init__(self, visual, algorithms, colors):
        self.expand, self.scale = algorithms
        self.hole = 0
        self.base = visual["scenePalette"]["colors"]
        self.colors = [md_palette_color(word)[:3] for word in self.base]
        self.target = colors
        plane = base64.b64decode(visual["scenePalette"]["indices"], validate=True)
        indexed = dict(
            visual,
            atlas=raster(128, 320, bytes(c for slot in plane for c in (slot, slot, slot, 255))),
        )
        self.blocks = [block.convert("L") for block in block_images(indexed)]

    def render(self, indices):
        from remaster.color import lerp_oklch, oklch_to_srgb, srgb_to_oklch

        width, height = indices.size
        raw = indices.tobytes()
        ramp = self.expand(
            [raw[y : y + width] for y in range(0, len(raw), width)],
            self.colors,
            exclude_indices={0},
        )
        base_lch = [srgb_to_oklch(tuple(c / 255 for c in rgb)) for rgb in self.colors]
        colors = [
            (*md_palette_color(word)[:3], 0 if i == 0 else 255)
            for i, word in enumerate(self.target)
        ]
        target_lch = [srgb_to_oklch(tuple(c / 255 for c in rgb[:3])) for rgb in colors]
        for (a, b), count in ramp.levels_added.items():
            dark, light = (a, b) if base_lch[a][0] <= base_lch[b][0] else (b, a)
            for level in range(count):
                rgb = oklch_to_srgb(
                    lerp_oklch(target_lch[dark], target_lch[light], (level + 1) / (count + 1))
                )
                colors.append((*[round(c * 255) for c in rgb], 255))
        rows = self.scale(ramp.new_grid)
        image = Image.frombytes("P", (width * 2, height * 2), b"".join(rows))
        image.putpalette(bytes(c for color in colors for c in color), rawmode="RGBA")
        return image.convert("RGBA")


def prepare(base: Path, output: Path, graphics: Path, pilot: bool):
    began = time.monotonic()
    output = private_output(output)
    if output.exists():
        raise ValueError("use a fresh output; preserve completed runs")
    algorithms = load_filter(graphics)
    document = read_json(base / "world.json")
    world = document["world"]
    visual = next(row for row in world["presentation"]["maps"] if row["map"] == "map-65")
    map_def = copy.deepcopy(next(row for row in world["maps"] if row["id"] == "map-65"))
    map_def["layoutEvents"] = {"roofs": [], "doors": [], "flags": []}
    base_colors = visual["scenePalette"]["colors"]
    pulses = {
        tuple(op["colors"])
        for p in world["programs"]
        for op in p["instructions"]
        if op["op"] == "scene-palette-pulse"
    }
    # Cover every admitted fade offset, including reduced-flash pulses entered
    # from a dark bank. Runtime selection remains exact sixteen-word equality.
    targets = set(pulses)
    for pulse in pulses:
        targets.update(tuple(reduced(offset(base_colors, step), pulse)) for step in range(-7, 1))
    targets.discard(tuple(base_colors))
    palette_banks = []
    samples = []
    for colors in [base_colors, *sorted(targets)]:
        filter_ = IndexedTerrain(visual, algorithms, colors)
        if pilot:
            rows = [png_raster(filter_.render(filter_.blocks[i])) for i in (1, 64, 126)]
            samples.append({"colors": colors, "blocks": rows})
        else:
            bank = {}
            publish_terrain(bank, map_def, filter_, None)
            if colors == base_colors:
                visual.update(bank)
            else:
                palette_banks.append({"colors": colors, **bank})
    sprites = [row for row in world["presentation"]["sprites"] if row["sprite"] in (183, 187)]
    if len(sprites) != 2:
        raise ValueError("opening requires both 183 and 187 inputs")
    for sprite in sprites:
        sprite["hdDirections"] = [strip(row, algorithms[1]) for row in sprite["directions"]]
    output.mkdir(parents=True)
    if pilot:
        write(output / "pilot.json", {"terrain": samples, "effects": sprites})
    else:
        visual["scenePalette"]["hdBanks"] = palette_banks
        # Published opening is already a closed package. Copy regular files only,
        # retaining its relative audio, Map42 and independent catalog selections.
        for source in base.rglob("*"):
            if source.is_symlink():
                raise ValueError("opening input contains a link")
            if source.is_file():
                target = output / source.relative_to(base)
                target.parent.mkdir(parents=True, exist_ok=True)
                shutil.copyfile(source, target)
        write(output / "world.json", document)
    report = {
        "pilot": pilot,
        "paletteBanks": len(targets),
        "map65Blocks": len(visual["blocks"]),
        "effects": [183, 187],
        "elapsedSeconds": round(time.monotonic() - began, 3),
        "bytes": sum(p.stat().st_size for p in output.rglob("*") if p.is_file()),
    }
    write(output / "hd-preparation.json", report)
    return report


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ("base", "output", "graphics"):
        parser.add_argument("--" + name, type=Path, required=True)
    parser.add_argument("--pilot", action="store_true")
    args = parser.parse_args()
    print(json.dumps(prepare(args.base, args.output, args.graphics, args.pilot)))


if __name__ == "__main__":
    main()
