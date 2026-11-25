"""Prepare a closed private shrine scene and castle storm passage from pinned inputs."""

from __future__ import annotations

import argparse
import base64
import copy
import hashlib
import json
import shutil
import time
from pathlib import Path

from sf2tool.compression import decode_basic_compressed, decode_stack_compressed
from sf2tool.remake_asset_build import (
    ACCEPTED_ROM_SHA256,
    PLAYER_PALETTE_ADDRESS,
    PLAYER_POINTER_TABLE_ADDRESS,
    _combine_player_halves,
    _render_player_frame,
)
from sf2tool.remake_command_menu import world_dependencies
from sf2tool.remake_exploration_content import checked_canonical, selected_map_palette_bindings
from sf2tool.remake_exploration_maps import compile_maps
from sf2tool.remake_exploration_programs import OriginalPrograms
from sf2tool.remake_exploration_visuals import source_map_atlas
from sf2tool.remake_localization import _jsonl, _topology
from sf2tool.remake_private_package import OPENING_INPUTS, private_output, read_json
from sf2tool.texture_extract import decode_md_4bpp_tile, md_palette_color


def write(path: Path, value) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(
        json.dumps(value, ensure_ascii=False, separators=(",", ":")) + "\n", encoding="utf-8"
    )


def raster(width: int, height: int, data: bytes) -> dict:
    return {
        "width": width,
        "height": height,
        "format": "rgba8",
        "data": base64.b64encode(data).decode("ascii"),
        "sha256": hashlib.sha256(data).hexdigest().upper(),
    }


def indexed_map(rom: bytes, definition: dict) -> dict:
    """Preserve all sixteen slot identities, including duplicate source RGB values."""

    def pointer(address):
        return int.from_bytes(rom[address : address + 4], "big")

    address = pointer(608586 + definition["palette"] * 4)
    colors = [int.from_bytes(rom[n : n + 2], "big") for n in range(address, address + 32, 2)]
    plane = bytearray(128 * 320)
    for bank, index in enumerate(definition["tilesets"]):
        if index == 255:
            continue
        data = decode_stack_compressed(
            rom[pointer(409612 + index * 4) :], expected_output_bytes=4096
        ).output
        for tile in range(128):
            pixels = decode_md_4bpp_tile(data[tile * 32 : tile * 32 + 32])
            for y in range(8):
                for x in range(8):
                    plane[(bank * 64 + tile // 16 * 8 + y) * 128 + tile % 16 * 8 + x] = pixels[
                        y * 8 + x
                    ]
    return {"indices": base64.b64encode(plane).decode("ascii"), "colors": colors}


def effect(rom: bytes, sprite: int) -> dict:
    palette = [
        md_palette_color(int.from_bytes(rom[n : n + 2], "big"))
        for n in range(PLAYER_PALETTE_ADDRESS, PLAYER_PALETTE_ADDRESS + 32, 2)
    ]
    directions = []
    for direction in range(3):
        entry = PLAYER_POINTER_TABLE_ADDRESS + (sprite * 3 + direction) * 4
        address = int.from_bytes(rom[entry : entry + 4], "big")
        decoded = decode_basic_compressed(rom[address:], expected_output_bytes=576).output
        directions.append(
            raster(
                48,
                24,
                bytes(
                    _combine_player_halves(
                        _render_player_frame(decoded[:288], palette),
                        _render_player_frame(decoded[288:], palette),
                    )
                ),
            )
        )
    return {"sprite": sprite, "directions": directions, "portrait": None, "speech": 0}


def prepare(
    base: Path,
    canonical_path: Path,
    upstream: Path,
    rom_path: Path,
    corpus: Path,
    party: Path,
    output: Path,
) -> dict:
    began = time.monotonic()
    output = private_output(output)
    if output.exists():
        raise ValueError("use a fresh output; preserve previous runs")
    canonical = checked_canonical(canonical_path, upstream)
    rom = rom_path.read_bytes()
    if hashlib.sha256(rom).hexdigest().upper() != ACCEPTED_ROM_SHA256:
        raise ValueError("registered ROM identity mismatch")
    document = read_json(base / "world.json")
    original = document["world"]
    compiler = OriginalPrograms(canonical, upstream, scene_maps=[42, 65])
    for number in (1, 2):
        compiler.register_file(f"disasm/data/scripting/map/cs_intro{number}.asm")
        compiler.compile(f"IntroCutscene{number}")
    # The passage ends after the first post-tint pair has restored its bank. The
    # original prefix is unchanged; the controlled host explicitly closes it.
    storm = compiler.programs["IntroCutscene2"]["instructions"]
    tint = next(i for i, op in enumerate(storm) if op["op"] == "scene-palette-fade")
    pulses = [i for i, op in enumerate(storm) if i > tint and op["op"] == "scene-palette-pulse"]
    compiler.programs["IntroCutscene2"]["instructions"] = storm[: pulses[1] + 1] + [{"op": "end"}]
    scene_programs = copy.deepcopy(list(compiler.programs.values()))
    if any(op["op"] == "native-call" for p in scene_programs for op in p["instructions"]):
        raise ValueError("selected scene has an unsupported source operation")

    map42 = next(row for row in original["maps"] if row["id"] == "map-42")
    map65 = compile_maps(compiler, canonical, {"maps": [65], "controlledEntitySpeed": 32}, {})[0]
    maps = []
    palettes = {
        row["map"]: row["base"]
        for row in selected_map_palette_bindings(compiler, [42, 65], rom_path)
    }
    for source in (map42, map65):
        population = next(
            op["population"]
            for p in scene_programs
            for op in p["instructions"]
            if op["op"] == "scene-entities"
        )
        maps.append(
            {
                **{key: copy.deepcopy(source[key]) for key in ("id", "layout", "areas")},
                "entities": [],
                "events": [],
                "onLoad": {
                    "program": "opening-1" if source["id"] == "map-42" else "opening-2",
                    "instruction": 0,
                },
                "battle": None,
                "input": None,
                "setup": None,
                "population": population,
                "basePalette": palettes[source["id"]],
            }
        )
    definitions = {row["id"]: row for row in canonical["maps"]}
    blocks = {row["id"]: row["blocks"] for row in canonical["resources"]["blocksets"]}
    visual_maps = []
    for number in (42, 65):
        definition = definitions[number]
        visual_maps.append(
            {
                "map": f"map-{number}",
                "atlas": raster(128, 320, source_map_atlas(rom, definition)),
                "scale": 1,
                "blocks": blocks[definition["references"]["blockset"]],
                "music": [
                    {"field": 9, "battle": 9}
                    for _ in next(m for m in maps if m["id"] == f"map-{number}")["areas"]
                ],
                "scenePalette": indexed_map(rom, definition),
            }
        )
    needed = {0}
    for program in scene_programs:
        for op in program["instructions"]:
            if op["op"] == "scene-entities":
                population = op["population"]
                ally = {row["character"]: row["sprite"] for row in population["allySprites"]}
                needed.update(ally.get(row["sprite"], row["sprite"]) for row in op["entities"])
                needed.update(ally.values())
                needed.update(
                    row["unjoinedSprite"]
                    for row in population["allySprites"]
                    if row["unjoinedSprite"] is not None
                )
                needed.update(
                    row["sprite"]
                    for row in population["followers"]
                    if row["character"] >= population["allyCount"]
                )
    sprites = [row for row in original["presentation"]["sprites"] if row["sprite"] in needed]
    for sprite in sorted(needed - {row["sprite"] for row in sprites}):
        if sprite not in (183, 187):
            raise ValueError(f"missing selected sprite binding {sprite}")
        sprites.append(effect(rom, sprite))
    portrait_ids = {row["portrait"] for row in sprites if row["portrait"] is not None}
    portraits = [
        row for row in original["presentation"]["portraits"] if row["portrait"] in portrait_ids
    ]
    # Finite modern default recordings are accepted for timer-mismatched SFX. No
    # timer-specific C4 rerecording or historical all-timer audio set is copied.
    audio = [
        row
        for row in original["presentation"]["audio"]
        if row["command"] == 9 or row.get("defaultForCommand") in (65, 73, 74, 75, 77, 89, 117, 118)
    ]
    for program in scene_programs:
        for op in program["instructions"]:
            if op["op"] == "present" and op["kind"] == "Sound":
                command = compiler.number(op["resource"])
                candidates = [row for row in audio if row["command"] == command]
                if len(candidates) != 1:
                    raise ValueError(f"missing selected sound binding {command}")
                op["resource"] = candidates[0]["cue"]
    for number in (1, 2):
        scene_programs.append(
            {
                "id": f"opening-{number}",
                "entitiesRunning": True,
                "instructions": [
                    {
                        "op": "present",
                        "kind": "Sound",
                        "resource": "MUSIC_INTRO",
                        "entity": None,
                        "position": None,
                    },
                    {
                        "op": "call",
                        "target": {"program": f"introcutscene{number}", "instruction": 0},
                    },
                    {"op": "close-text"},
                    {"op": "end-scene"},
                ],
            }
        )
    texts = [row for row in original["texts"] if 4186 <= row["id"] <= 4266]
    authority = _jsonl(corpus / "source.jsonl", lambda row: (row["namespace"], row["id"]))
    translated = []
    for row in texts:
        accepted = authority[("world.text", row["id"])]
        if (
            accepted["source"] != row["text"]
            or not isinstance(accepted["translation"], str)
            or not _topology(row["text"], accepted["translation"])
        ):
            raise ValueError(f"accepted text identity/topology {row['id']}")
        translated.append(
            {"namespace": "world.text", "id": row["id"], "text": accepted["translation"]}
        )
    if len(translated) != 81:
        raise ValueError("opening text selection must contain 81 identities")
    name_source = _jsonl(corpus / "names.zh-CN.jsonl", lambda row: (row["namespace"], row["index"]))
    names = [
        {
            "namespace": "name.ally",
            "index": index,
            "text": name_source[("name.ally", index)]["translation"],
        }
        for index in (0, 5)
    ]
    world = {
        "gameplayRevision": original["gameplayRevision"],
        "maps": maps,
        "programs": scene_programs,
        "texts": texts,
        "memberNames": original["memberNames"],
        "textFont": original["textFont"],
        "presentation": {
            "maps": visual_maps,
            "sprites": sprites,
            "portraits": portraits,
            "audio": audio,
        },
    }
    document.pop("encounters", None)
    document["world"] = world
    document["provenance"]["controlledBoundary"] = (
        "Complete IntroCutscene1 and source IntroCutscene2 prefix through the first "
        "post-tint storm pair. "
        "Explicit host terminal; no natural boot or persistent-party claim."
    )
    sources = {row["path"]: row for row in document["provenance"]["sources"]}
    for path in compiler.sources:
        sources[path] = {
            "path": path,
            "sha256": hashlib.sha256((upstream / path).read_bytes()).hexdigest().upper(),
        }
    document["provenance"]["sources"] = list(sources.values())
    output.mkdir(parents=True)
    write(output / "world.json", document)
    write(
        output / "world.zh-CN.json",
        {
            "package": document["package"],
            "locale": "zh-CN",
            "texts": translated,
            "names": names,
            "source": {
                key: document["provenance"][key] for key in ("repository", "commit", "romSha256")
            },
        },
    )
    for number, map_id in ((1, 42), (2, 65)):
        write(
            output / f"scene-{number}.json",
            {
                "formatVersion": 1,
                "controlledBoundary": document["provenance"]["controlledBoundary"],
                "start": {
                    "map": f"map-{map_id}",
                    "player": "entity-0",
                    "position": {"x": 8, "y": 8},
                    "facing": 3,
                    "speed": 32,
                    "flags": [],
                    "display": {
                        "period": 6,
                        "base": palettes[f"map-{map_id}"],
                        "current": {"color2": 0, "color3": 0},
                        "visibility": "black",
                    },
                    "textSettings": {"messageSpeed": 2, "mouthControl": 0, "viewSpeed": 0},
                },
            },
        )
    shutil.copyfile(party, output / "party.json")
    for name in OPENING_INPUTS:
        target = output / "opening-inputs" / (name + ".json")
        target.parent.mkdir(exist_ok=True)
        shutil.copyfile(base / "opening-inputs" / target.name, target)
    for relative in world_dependencies(document, base / "world.json"):
        source = (base / relative).resolve(strict=True)
        if not source.is_relative_to(base.resolve()):
            raise ValueError("package dependency escapes base")
        target = output / relative
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(source, target)
    sizes = {
        path.relative_to(output).as_posix(): path.stat().st_size
        for path in output.rglob("*")
        if path.is_file()
    }
    report = {
        "files": sizes,
        "bytes": sum(sizes.values()),
        "elapsedSeconds": round(time.monotonic() - began, 3),
        "maps": [42, 65],
        "sprites": sorted(needed),
        "texts": len(texts),
        "programOperations": {p["id"]: len(p["instructions"]) for p in scene_programs},
    }
    write(output / "preparation.json", report)
    return report


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ("base", "canonical", "upstream", "rom", "corpus", "party", "output"):
        parser.add_argument("--" + name, required=True, type=Path)
    args = parser.parse_args()
    print(
        json.dumps(
            prepare(
                args.base,
                args.canonical,
                args.upstream,
                args.rom,
                args.corpus,
                args.party,
                args.output,
            )
        )
    )


if __name__ == "__main__":
    main()
