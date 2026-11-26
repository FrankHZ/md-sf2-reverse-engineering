"""Build a short controlled presentation witness from a published opening package.

This is not the natural four-script lifecycle. It reuses admitted maps, populations,
effect actors and palette operands, with explicit short waits and terminal return.
"""

import argparse
import copy
import hashlib
import shutil
from pathlib import Path

from sf2tool.remake_map3_hd import decode
from sf2tool.remake_opening_scene import write
from sf2tool.remake_private_package import private_output, read_json

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("--base", type=Path, required=True)
parser.add_argument("--output", type=Path, required=True)
parser.add_argument("--missing", action="store_true")
parser.add_argument("--missing-art", action="store_true")
args = parser.parse_args()
output = private_output(args.output)
if output.exists():
    raise ValueError("use a fresh witness destination")
shutil.copytree(args.base, output)
document = read_json(output / "world.json")
world = document["world"]
programs = {row["id"]: row for row in world["programs"]}
sequence = [{"op": "camera-entity", "entity": None}]
for number, sprite, camera in ((1, 183, {"x": 2, "y": 5}), (2, 187, {"x": 7, "y": 2})):
    source = programs[f"introcutscene{number}"]["instructions"]
    scene_map = next(op for op in source if op["op"] == "scene-map")
    entities = copy.deepcopy(next(op for op in source if op["op"] == "scene-entities"))
    actor = next(row for row in entities["entities"] if row["sprite"] == sprite)
    actor["position"] = {"x": camera["x"] + 4, "y": camera["y"] + 4}
    entities["entities"] = [actor] + (
        [next(row for row in entities["entities"] if row["id"] == "entity-130")]
        if number == 2
        else []
    )
    sequence += [
        scene_map,
        {"op": "wait-ticks", "ticks": 1},
        entities,
        {"op": "scene-palette-fade", "banks": 15, "offsets": [-7, -2, 0], "period": 6},
        {"op": "wait-ticks", "ticks": 24},
    ]
    if number == 2:
        sequence += [
            {"op": "camera-target", "position": {"x": 7, "y": 4}},
            {"op": "wait-view"},
            {"op": "camera-entity", "entity": "entity-130"},
            {"op": "wait-ticks", "ticks": 24},
            {"op": "camera-entity", "entity": None},
            {"op": "camera-target", "position": {"x": 7, "y": 2}},
            {"op": "wait-view"},
        ]
        pulse = next(op for op in source if op["op"] == "scene-palette-pulse")
        sequence += [
            pulse,
            {"op": "scene-palette-fade", "banks": 1, "offsets": [-1, -2], "period": 6},
            pulse,
            {"op": "scene-palette-fade", "banks": 15, "offsets": [0], "period": 6},
            {"op": "wait-ticks", "ticks": 24},
        ]
    sequence += [{"op": "scene-palette-fade", "banks": 15, "offsets": [-7], "period": 6}]
sequence += [{"op": "close-text"}, {"op": "end-scene"}]
programs["opening-sequence"]["instructions"] = sequence
if args.missing or args.missing_art:
    for visual in world["presentation"]["maps"]:
        visual["scenePalette"].pop("hdBanks", None)
if args.missing_art:
    for visual in world["presentation"]["maps"]:
        if visual["map"] == "map-65":
            visual.pop("hdBlocks", None)
            visual.pop("hdBlockContexts", None)
    for sprite in world["presentation"]["sprites"]:
        if sprite["sprite"] in (183, 187):
            sprite.pop("hdDirections", None)
write(output / "world.json", document)
expected = []
for visual in world["presentation"]["maps"]:
    for bank in visual["scenePalette"].get("hdBanks", []):
        rows = bank["hdBlocks"] + [row["raster"] for row in bank.get("hdBlockContexts", [])]
        expected.append(
            {
                "colors": bank["colors"],
                "hashes": sorted(
                    {hashlib.sha256(decode(row).tobytes()).hexdigest().upper() for row in rows}
                ),
            }
        )
write(output / "expected-hd-pixels.json", expected)
