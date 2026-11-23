"""Prepare or verify the registered campaign package with corrected fixed battle backdrops."""

from __future__ import annotations

import argparse
import base64
import json
import re
import struct
import subprocess
import zlib
from pathlib import Path
from tempfile import TemporaryDirectory

from sf2tool.h2.battle_global_data import _arguments
from sf2tool.paths import repo_path
from sf2tool.remake_asset_build import (
    ACCEPTED_ROM_SHA256,
    ACCEPTED_ROM_SIZE,
    ACCEPTED_UPSTREAM_COMMIT,
    _scale_rgba_nearest,
)
from sf2tool.remake_battle_scene_content import battle_scene_backdrops, digest
from sf2tool.remake_private_package import installed_png, package_files, private_output, read_json
from sf2tool.remake_retail_assets import colors, pointer, word
from sf2tool.texture_extract import write_png_rgba


def _require(condition, message: str) -> None:
    if not condition:
        raise ValueError(message)


def _source_tables(upstream: Path, rom: bytes) -> tuple[tuple[int, ...], tuple[int, ...]]:
    git = ["git", "-C", str(upstream)]
    pin = ACCEPTED_UPSTREAM_COMMIT
    head = subprocess.check_output([*git, "rev-parse", "HEAD"], text=True).strip()
    _require(head == pin, "upstream identity differs")

    def source(path: str) -> str:
        return subprocess.check_output([*git, "show", f"{pin}:disasm/{path}"], encoding="utf-8")

    enums = {
        name: int(value)
        for name, value in re.findall(
            r"^BATTLEBACKGROUND_(\w+):[ \t]+equ[ \t]+(\d+)", source("sf2enums.asm"), re.M
        )
    }
    custom, terrain = (
        tuple(
            enums[name]
            for name in _arguments(source(f"data/battles/global/{table}.asm"), "background")
        )
        for table in ("custombackgrounds", "terrainbackgrounds")
    )
    selector = source("code/gameflow/battle/battlescenes/getbattlescenebackground.asm")
    _require(
        selector.index("#ENEMY_ZEON")
        < selector.index("table_CustomBackgrounds(pc)")
        < selector.index("#BATTLEBACKGROUND_OVERWORLD"),
        "original backdrop selector precedence differs",
    )
    _require(enums["OVERWORLD"] == 0xFF, "OVERWORLD selector differs")
    _require(tuple(rom[0x1FA8A : 0x1FA8A + len(custom)]) == custom, "custom source/ROM difference")
    _require(tuple(rom[0x19E5E:0x19E6E]) == terrain, "terrain source/ROM difference")
    return custom, terrain


def _prepared_pixels(path: Path, width: int, height: int) -> bytes:
    # The registered terrain family uses the project's RGBA, filter-0 PNG writer.
    png = path.read_bytes()
    _require(
        png.startswith(b"\x89PNG\r\n\x1a\n")
        and png[37:41] == b"IDAT"
        and png[16:29] == struct.pack(">IIBBBBB", width, height, 8, 6, 0, 0, 0),
        f"unexpected prepared PNG format: {path.name}",
    )
    size = int.from_bytes(png[33:37], "big")
    stride = width * 4 + 1
    raw = zlib.decompressobj().decompress(png[41 : 41 + size], stride * height + 1)
    _require(
        len(raw) == stride * height and not any(raw[::stride]),
        f"unexpected prepared PNG scanlines: {path.name}",
    )
    return b"".join(raw[i + 1 : i + stride] for i in range(0, len(raw), stride))


def _raster(kind: str, backdrop: int, rom: bytes, library: Path, index: dict, scratch: Path):
    name = index["records"][kind][backdrop]
    path = (library / name).resolve(strict=True)
    _require(path.parent == library and path.suffix == ".png", "invalid terrain index path")
    _require(path.stat().st_size == index["files"][name], "terrain index size differs")

    if kind == "background":
        table = read_json(repo_path("tests/fixtures/h2/battle-background-decode-v1.json"))["table"]
        address = pointer(rom, table["backgroundTableAddress"] + backdrop * 4)
        palette = address + 4 + word(rom, address + 4)
        allowed = {(*color, 255) for color in colors(rom[palette : palette + 32])}
        width, height = 256, 96
    else:
        table = read_json(repo_path("tests/fixtures/h2/battle-weapon-ground-decode-v1.json"))[
            "table"
        ]
        address = pointer(rom, table["groundTableAddress"] + backdrop * 4)
        ground_colors = colors(rom[0x198A8:0x198C8])
        for slot, color in zip((3, 4, 8), colors(rom[address : address + 6]), strict=True):
            ground_colors[slot] = color
        allowed = {(*color, 255) for color in ground_colors} | {(0, 0, 0, 0)}
        width, height = 96, 32

    pixels = _prepared_pixels(path, width, height)
    rgba = set(zip(*(pixels[channel::4] for channel in range(4)), strict=True))
    _require(rgba <= allowed and len(rgba) > 1, f"{name} palette differs from the retail ROM")
    scaled = _scale_rgba_nearest(pixels, width, height, 2)
    resource = f"{kind}{backdrop}.png"
    write_png_rgba(scratch / resource, width * 2, height * 2, scaled)
    raster = installed_png(scratch, resource)
    return raster, {
        "width": raster["width"],
        "height": raster["height"],
        "colorsUsed": len(rgba),
        "pngSha256": raster["sha256"],
        # Compare the actual loaded Godot texture with these palette-checked RGBA bytes.
        "imageSha256": digest(bytes(scaled)).upper(),
    }


def _scene_paths(files: dict[Path, Path]) -> dict[int, Path]:
    scenes = {1: Path("main-scenes.json")}
    for encounter in read_json(files[Path("world.json")])["encounters"]:
        placement = read_json(files[Path(encounter["placement"])])
        _require(
            all(
                entity["identityExpression"] not in ("ZEON", "ENEMY_ZEON", "98")
                for entity in placement["entities"]
                if entity["kind"] == "enemy"
            ),
            "Zeon needs a scene override",
        )
        battle = placement["battle"]["id"]
        _require(battle not in scenes, f"duplicate campaign scene: {battle}")
        scenes[battle] = Path(encounter["battleScenes"])
    _require(set(scenes) == set(range(1, 27)), "expected the registered 26-encounter campaign")
    return scenes


def prepare(
    base: Path, library: Path, destination: Path, upstream: Path, rom_path: Path, *, check=False
):
    base = base.resolve(strict=True)
    library = (library / "terrain").resolve(strict=True)
    destination = private_output(destination)
    _require(
        destination.is_dir() if check else not destination.exists(), "invalid destination state"
    )
    _require(
        not any(destination.is_relative_to(path) for path in (base, library.parent)),
        "destination overlaps an input package",
    )
    rom = rom_path.read_bytes()
    _require(
        len(rom) == ACCEPTED_ROM_SIZE and digest(rom).upper() == ACCEPTED_ROM_SHA256,
        "retail ROM identity differs",
    )
    custom, terrain = _source_tables(upstream, rom)
    index = read_json(library / "index.json")
    _require(
        index["rom"] == ACCEPTED_ROM_SHA256 and index["upstream"] == ACCEPTED_UPSTREAM_COMMIT,
        "registered terrain family identity differs",
    )
    files = package_files(
        base / "world.json",
        base / "main-scenes.json",
        base / "entry/entry.json",
        base / "opening-inputs",
    )
    scenes = _scene_paths(files)
    changes, report = {}, {}

    def read(address: int, count: int) -> bytes:
        return rom[address : address + count]

    destination.parent.mkdir(parents=True, exist_ok=True)
    with TemporaryDirectory(prefix=".sf2-fixed-backdrops-", dir=destination.parent) as temporary:
        scratch = Path(temporary)
        for battle, relative in sorted(scenes.items()):
            original = files[relative].read_bytes()
            document = json.loads(original)
            _require(
                document["encounter"] == f"battle-{battle}", f"scene encounter differs: {battle}"
            )
            expected = (0, terrain) if custom[battle] == 0xFF else (custom[battle], ())
            backdrop, terrain_indices = battle_scene_backdrops(read, battle)
            _require((backdrop, terrain_indices) == expected, f"export selector differs: {battle}")
            bindings = {"background": f"background{backdrop}", "ground": f"ground{backdrop}"}
            rows = [
                {"terrain": i, "background": f"background{n}", "ground": f"ground{n}"}
                for i, n in enumerate(terrain_indices)
            ]
            detail = {"backdrop": backdrop, "terrainRows": len(rows)}
            report[battle] = detail
            if not rows and (battle in (5, 7)):
                _require("terrainScenes" in document, f"expected an old terrain binding: {battle}")
                del document["terrainScenes"]
                for kind, resource in bindings.items():
                    document[kind] = resource
                    document["rasters"][resource], detail[kind] = _raster(
                        kind, backdrop, rom, library, index, scratch
                    )
                    # These rasters belonged to the removed terrain bindings.
                    for old_index in set(terrain):
                        document["rasters"].pop(f"{kind}{old_index}", None)
                encoded = json.dumps(document, separators=(",", ":"), ensure_ascii=True).encode()
                _require(len(encoded) < 4 * 1024 * 1024, f"scene exceeds reader limit: {battle}")
                changes[relative] = (original, encoded)
            else:
                _require(
                    (
                        document.get("terrainScenes") == rows
                        if rows
                        else "terrainScenes" not in document
                    )
                    and all(document[key] == name for key, name in bindings.items()),
                    f"base backdrop bindings differ: {battle}",
                )
            for row in rows or [bindings]:
                for kind in bindings:
                    raster = document["rasters"][row[kind]]
                    _require(
                        digest(base64.b64decode(raster["data"])).upper()
                        == raster["sha256"].upper(),
                        f"invalid raster reference: {battle}:{row[kind]}",
                    )

        _require(
            set(changes) == {scenes[5], scenes[7]}, "expected two independent scene replacements"
        )
        candidate = destination if check else scratch / "candidate"
        for relative, source in files.items():
            target = candidate / relative
            if relative in changes:
                original, replacement = changes[relative]
                if not check:
                    target.parent.mkdir(parents=True, exist_ok=True)
                    target.write_bytes(replacement)
                _require(source.read_bytes() == original, f"base scene changed: {relative}")
                _require(
                    not target.samefile(source) and target.read_bytes() == replacement,
                    f"independent scene replacement differs: {relative}",
                )
            else:
                if not check:
                    target.parent.mkdir(parents=True, exist_ok=True)
                    target.hardlink_to(source)
                _require(target.samefile(source), f"expected immutable source link: {relative}")
            _require(
                target.resolve().is_relative_to(candidate), f"package path escapes: {relative}"
            )
        _require(
            {path.relative_to(candidate) for path in candidate.rglob("*") if path.is_file()}
            == files.keys(),
            "candidate file set differs from package closure",
        )
        if not check:
            candidate.rename(destination)

    return {
        "checked": check,
        "worldEncounters": report,
        "files": len(files),
        "immutableLinks": len(files) - len(changes),
        "newBytes": sum(len(replacement) for _, replacement in changes.values()),
    }


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    for flag in ("base", "library", "destination", "upstream", "rom"):
        parser.add_argument(f"--{flag}", type=Path, required=True)
    parser.add_argument("--check", action="store_true", help="Verify the existing candidate")
    args = parser.parse_args()
    report = prepare(
        args.base, args.library, args.destination, args.upstream, args.rom, check=args.check
    )
    print(json.dumps(report, indent=2))


if __name__ == "__main__":
    main()
