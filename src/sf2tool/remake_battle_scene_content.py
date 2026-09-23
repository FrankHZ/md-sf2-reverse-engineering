"""Selected, source-composed battle-scene candidate; canonical inputs are read-only.

Run with --rom, --upstream and a new ignored --output directory. This writes an
ordinary pack-v1 candidate plus selected scene content, never promotes a library.
"""

from __future__ import annotations

import argparse
import base64
import hashlib
import json
import re
import subprocess
from pathlib import Path

from jsonschema import Draft202012Validator

from sf2tool.compression import decode_stack_compressed
from sf2tool.paths import repo_path
from sf2tool.remake_asset_build import (
    ACCEPTED_ROM_SHA256,
    ACCEPTED_ROM_SIZE,
    ACCEPTED_UPSTREAM_COMMIT,
    ACCEPTED_UPSTREAM_REPOSITORY,
    _composite_generator_fingerprint,
    _scale_rgba_nearest,
)
from sf2tool.remake_assets import (
    MANIFEST_RELATIVE_PATH,
    PACK_CAPABILITY,
    PACK_SCHEMA,
    PACKAGE_ID,
    PROFILE,
    REPOSITORY_ID,
)
from sf2tool.texture_extract import (
    decode_md_4bpp_tile,
    md_palette_color,
    palette_index_rgba,
    write_png_rgba,
)


def digest(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest().upper()


def write_json(path: Path, value: object) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(
        json.dumps(value, ensure_ascii=True, separators=(",", ":")) + "\n", encoding="utf-8"
    )


def compose(
    data: bytes, palette: list, width: int, height: int, layout: list[int], *, opaque=False
) -> list[int]:
    """Compose a row-major tile layout from column-major hardware sprite tiles."""
    pixels = [0] * (width * height * 4)
    assert len(layout) == width * height // 64
    for cell, word in enumerate(layout):
        index = word & 2047
        tile = decode_md_4bpp_tile(data[index * 32 : (index + 1) * 32])
        for y in range(8):
            for x in range(8):
                color = tile[(7 - y if word & 4096 else y) * 8 + (7 - x if word & 2048 else x)]
                rgba = (*palette[color], 255) if opaque else palette_index_rgba(palette, color)
                offset = (
                    ((cell // (width // 8)) * 8 + y) * width + (cell % (width // 8)) * 8 + x
                ) * 4
                pixels[offset : offset + 4] = rgba
    return pixels


def sprite_layout(columns: int, rows: int) -> list[int]:
    # Each 4x4 tile sprite stores columns first; sprite chunks also run down first.
    return [
        ((x // 4) * ((rows + 3) // 4) + (y // 4)) * 16 + (x % 4) * 4 + y % 4
        for y in range(rows)
        for x in range(columns)
    ]


def build(rom_path: Path, upstream: Path, output: Path) -> dict:
    root = repo_path("")
    output = output.resolve()
    if not output.is_relative_to(root / "local") or output.exists():
        raise ValueError("output must be a new worktree-local ignored directory")
    ignored = subprocess.run(["git", "check-ignore", "--quiet", str(output)], cwd=root, check=False)
    if ignored.returncode != 0:
        raise ValueError("output is not ignored")
    commit = subprocess.check_output(
        ["git", "-C", str(upstream), "rev-parse", "HEAD"], text=True
    ).strip()
    if commit != ACCEPTED_UPSTREAM_COMMIT:
        raise ValueError("upstream identity drift")
    rom = rom_path.read_bytes()
    if len(rom) != ACCEPTED_ROM_SIZE or digest(rom) != ACCEPTED_ROM_SHA256:
        raise ValueError("ROM identity drift")
    output.mkdir(parents=True)
    spans = []

    def read(address: int, count: int) -> bytes:
        data = rom[address : address + count]
        if len(data) != count:
            raise ValueError("ROM span outside input")
        spans.append(
            {"address": address, "byteLength": count, "data": base64.b64encode(data).decode()}
        )
        return data

    def word(address):
        return int.from_bytes(read(address, 2), "big")

    def pointer(address):
        return int.from_bytes(read(address, 4), "big")

    # CRAM ignores the low bit of each channel. The retail base palette actually
    # contains 0x0DB0/0x0E50; retain raw words in provenance, mask at projection.
    def palette(address):
        return [md_palette_color(word(address + i * 2) & 0x0EEE) for i in range(16)]

    def decode(address, size):
        result = decode_stack_compressed(rom[address:], expected_output_bytes=size)
        read(address, (result.input_bits_consumed + 7) // 8)
        return result.output

    def table(name):
        return json.loads(
            repo_path("tests/fixtures/h2/" + name + "-v1.json").read_text(encoding="utf-8")
        )["table"]

    components = {
        name: repo_path(name).read_bytes()
        for name in (
            "src/sf2tool/remake_battle_scene_content.py",
            "src/sf2tool/remake_asset_build.py",
            "src/sf2tool/texture_extract.py",
            "src/sf2tool/compression.py",
        )
    }
    fingerprint = _composite_generator_fingerprint(components)
    assets, rasters = [], {}

    def raster(name, data, pal, width, height, layout, *, opaque=False):
        pixels = compose(data, pal, width, height, layout, opaque=opaque)
        master = output / ("masters/battle-scenes/" + name + ".png")
        master.parent.mkdir(parents=True, exist_ok=True)
        write_png_rgba(master, width, height, pixels)
        buckets = []
        for scale in (2, 4):
            relative = "runtime/battle-scenes/" + name + f"@{scale}x.png"
            target = output / relative
            target.parent.mkdir(parents=True, exist_ok=True)
            write_png_rgba(
                target,
                width * scale,
                height * scale,
                _scale_rgba_nearest(tuple(pixels), width, height, scale),
            )
            encoded = target.read_bytes()
            buckets.append(
                dict(
                    scale=scale,
                    runtimePath=relative,
                    width=width * scale,
                    height=height * scale,
                    byteLength=len(encoded),
                    sha256=digest(encoded),
                    mediaType="image/png",
                    filter="nearest",
                    mipmaps=False,
                    repeat=False,
                    colorSpace="srgb",
                    alphaMode="straight",
                )
            )
            if scale == 2:
                rasters[name] = dict(
                    width=width * 2,
                    height=height * 2,
                    format="png",
                    data=base64.b64encode(encoded).decode(),
                    sha256=digest(encoded),
                )
        assets.append(
            dict(
                assetId="battle.scene." + name.replace("/", "."),
                kind="raster-image",
                logicalSize=dict(width=width, height=height),
                source=dict(assetId="source.battle.scene.selection", sha256=""),
                derivation=dict(
                    policyId="source-layout-nearest-v1",
                    generatorId="sf2tool-remake-battle-scene-content",
                    generatorVersion="1",
                    generatorArtifactSha256=fingerprint,
                ),
                buckets=buckets,
            )
        )
        return name

    sprites = table("battle-sprite-decode")
    animations = table("battle-sprite-animation-static")
    weapons = table("battle-weapon-ground-decode")
    actor_rows = []
    for side, index, pal_index, weapon_item in (
        ("ally", 0, 0, 71),
        ("ally", 1, 0, 85),
        ("ally", 2, 0, 72),
        ("enemy", 22, 2, None),
    ):
        base = pointer(sprites[side + "BattlespriteTableAddress"] + index * 4)
        pal_offset = base + 4 + word(base + 4)
        pal = palette(pal_offset + pal_index * 32)
        frames = []
        for frame in range((pal_offset - base - 6) // 2):
            offset = base + 6 + frame * 2
            data = decode(offset + word(offset), 4608 if side == "ally" else 6144)
            frames.append(
                raster(
                    f"{side}{index}/frame{frame}",
                    data,
                    pal,
                    96 if side == "ally" else 128,
                    96,
                    sprite_layout(12 if side == "ally" else 16, 12),
                )
            )
        sequences = {}
        for purpose, animation in (
            ("attack", 80 if weapon_item == 72 else index),
            ("dodge", index + (40 if side == "ally" else 60)),
            ("idle", index),
        ):
            address = pointer(animations["pt_" + side.title() + "Animations"] + animation * 4)
            count = rom[address]
            size = 8 if side == "ally" else 4
            data = read(address, size + count * size)

            def signed(value):
                return value if value < 128 else value - 256

            def weapon(raw):
                return dict(frame=raw[0], layer=raw[1], x=signed(raw[2]), y=signed(raw[3]))

            rows = []
            for offset in range(size, len(data), size):
                raw = data[offset : offset + size]
                rows.append(
                    dict(
                        frame=raw[0],
                        ticks=raw[1],
                        x=signed(raw[2]),
                        y=signed(raw[3]),
                        weapon=weapon(raw[4:]) if side == "ally" else None,
                    )
                )
            sequences[purpose] = dict(
                index=animation,
                trigger=data[1],
                spell=data[2],
                terminate=data[3],
                idleWeapon=weapon(data[4:8]) if side == "ally" else None,
                frames=rows,
            )
        weapon_frames = []
        if weapon_item is not None:
            # table_WeaponGraphics at 0x1F9E2, source getweaponspriteandpalette.asm.
            wid, pid = read(0x1F9E2 + (weapon_item - 26) * 2, 2)
            weapon_pal = list(pal)
            weapon_pal[14:16] = [
                md_palette_color(word(weapons["weaponPaletteAddress"] + pid * 4 + i * 2))
                for i in range(2)
            ]
            data = decode(pointer(weapons["weaponSpriteTableAddress"] + wid * 4), 8192)
            for frame in range(4):
                weapon_frames.append(
                    raster(
                        f"weapon{weapon_item}/frame{frame}",
                        data[frame * 2048 : (frame + 1) * 2048],
                        weapon_pal,
                        64,
                        64,
                        sprite_layout(8, 8),
                    )
                )
        actor_rows.append(
            dict(
                side=side,
                sprite=index,
                palette=pal_index,
                item=weapon_item,
                frames=frames,
                weaponFrames=weapon_frames,
                sequences=sequences,
            )
        )

    base_palette = palette(0x198A8)  # palette_BattlesceneBase, battlesceneengine_1.asm.
    ground = pointer(weapons["groundTableAddress"] + 9 * 4)
    for offset, color in enumerate((3, 4, 8)):
        base_palette[color] = md_palette_color(word(ground + offset * 2))
    ground_asset = raster(
        "ground9",
        decode(ground + 6 + word(ground + 6), 1536),
        base_palette,
        96,
        32,
        sprite_layout(12, 4),
    )
    backgrounds = table("battle-background-decode")
    background = pointer(backgrounds["backgroundTableAddress"] + 9 * 4)
    bgdata = decode(background + word(background), 6144) + decode(
        background + 2 + word(background + 2), 6144
    )
    bgpal = palette(background + 4 + word(background + 4))
    layout = [int.from_bytes(read(0x1FAEA + i * 2, 2), "big") for i in range(384)]
    layout = [(value & 0x1800) | ((value & 2047) - 928) for value in layout]
    background_asset = raster("background9", bgdata, bgpal, 256, 96, layout, opaque=True)

    def source_text(relative):
        return subprocess.check_output(
            ["git", "-C", str(upstream), "show", commit + ":" + relative]
        ).decode("utf-8")

    script = source_text("disasm/data/scripting/text/gamescript.txt")
    text_ids = {
        244,
        263,
        266,
        267,
        268,
        269,
        270,
        271,
        273,
        284,
        285,
        286,
        287,
        288,
        290,
        291,
        292,
        293,
        393,
    }
    texts = {
        str(int(line[:4], 16)): line[5:]
        for line in script.splitlines()
        if re.match(r"^[0-9A-Fa-f]{4}=", line) and int(line[:4], 16) in text_ids
    }
    names_source = source_text("disasm/data/stats/allies/allynames.asm")
    names = re.findall(r'allyName\s+"([^"]+)"', names_source)
    bundle = dict(
        romSha256=ACCEPTED_ROM_SHA256,
        upstreamRepository=ACCEPTED_UPSTREAM_REPOSITORY,
        upstreamCommit=commit,
        spans=spans,
        texts=texts,
        memberNames=names,
        textSource="disasm/data/scripting/text/gamescript.txt",
        nameSource="disasm/data/stats/allies/allynames.asm",
    )
    bundle_path = output / "source/battle-scenes/selection.json"
    write_json(bundle_path, bundle)
    source_hash = digest(bundle_path.read_bytes())
    for asset in assets:
        asset["source"]["sha256"] = source_hash
    manifest = dict(
        schemaVersion=1,
        packageId=PACKAGE_ID,
        repositoryId=REPOSITORY_ID,
        profile=PROFILE,
        capabilities=[PACK_CAPABILITY],
        logicalPresentation=dict(width=960, height=540),
        assets=assets,
    )
    Draft202012Validator(json.loads(PACK_SCHEMA.read_text(encoding="utf-8"))).validate(manifest)
    write_json(output / MANIFEST_RELATIVE_PATH, manifest)
    write_json(
        output / "battle-scenes.json",
        dict(
            version=1,
            encounter="battle-1",
            background=background_asset,
            ground=ground_asset,
            actors=actor_rows,
            rasters=rasters,
            texts=texts,
            memberNames=names,
        ),
    )
    report = dict(
        status="candidate-only",
        upstreamCommit=commit,
        romSha256=ACCEPTED_ROM_SHA256,
        generatorArtifactSha256=fingerprint,
        manifestSha256=digest((output / MANIFEST_RELATIVE_PATH).read_bytes()),
        sourceSha256=source_hash,
        assetCount=len(assets),
        sceneContentBytes=(output / "battle-scenes.json").stat().st_size,
        totalBytes=sum(path.stat().st_size for path in output.rglob("*") if path.is_file()),
    )
    write_json(output / "candidate-report.json", report)
    return report


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--rom", type=Path, required=True)
    parser.add_argument("--upstream", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    print(json.dumps(build(args.rom, args.upstream, args.output)))
