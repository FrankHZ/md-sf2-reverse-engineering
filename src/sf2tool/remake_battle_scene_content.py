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


TARGET_CURSOR_RESOURCE = "battle-cursor.single-cell"


def special_battle_sprite_frames(decode, palette, read, base):
    tiles = decode(base + 32, 2304)
    for frame, address in enumerate((0x2786C, 0x2784C)):
        layout = [0] * 36
        for piece in range(4):
            row = read(address + piece * 8, 8)
            y, x = row[1] - 0x5C, row[7] - 0x74
            first = (int.from_bytes(row[4:6], "big") & 2047) - 0x578
            for dx in range(3):
                for dy in range(3):
                    layout[(y // 8 + dy) * 6 + x // 8 + dx] = first + dx * 3 + dy
        yield frame, tiles, palette(base), layout


def single_cell_cursor(rom: bytes) -> tuple[dict, dict]:
    """The accepted radius-zero sprite, independent of preview paths or a mutable library."""
    if len(rom) != ACCEPTED_ROM_SIZE or digest(rom) != ACCEPTED_ROM_SHA256:
        raise ValueError("ROM identity drift")
    decoded = decode_stack_compressed(rom[0x23658:0x23758], expected_output_bytes=2048)
    palette = [
        md_palette_color(int.from_bytes(rom[i : i + 2], "big") & 0x0EEE)
        for i in range(0x309E, 0x30BE, 2)
    ]
    pixels = bytes(compose(decoded.output, palette, 32, 32, sprite_layout(4, 4)))
    return dict(
        width=32,
        height=32,
        format="rgba8",
        data=base64.b64encode(pixels).decode(),
        sha256=digest(pixels),
    ), dict(
        romSha256=ACCEPTED_ROM_SHA256,
        upstreamRepository=ACCEPTED_UPSTREAM_REPOSITORY,
        upstreamCommit=ACCEPTED_UPSTREAM_COMMIT,
        resource=TARGET_CURSOR_RESOURCE,
        source="disasm/data/graphics/tech/cursortiles.bin",
        address=0x23658,
        compressedBytes=256,
        inputBitsConsumed=decoded.input_bits_consumed,
        decodedBytes=2048,
        paletteAddress=0x309E,
        paletteBytes=32,
        layout="first-column-major-4x4",
        width=32,
        height=32,
        rgbaSha256=digest(pixels),
        filter="nearest",
    )


def campaign_ally_visuals(battle, read, ally_table, stats_table):
    # First deployment of each unpromoted member, including optional Kiwi and May.
    joins = (1, 1, 1, 2, 4, 6, 7, 9, 10, 13, 14)
    # First encounter following access to Granseal, Galam, New Granseal, Ribble, Polca, Bedoe.
    shop_entry = (2, 5, 9, 10, 12, 13)
    cursor = stats_table["list_ShopInventories"]
    carried = set()
    for first_battle in shop_entry:
        if battle < first_battle:
            break
        count = read(cursor, 1)[0]
        carried.update(read(cursor + 1, count))
        cursor += 1 + count

    members = []
    for member, first_battle in enumerate(joins):
        if battle < first_battle:
            continue
        start = read(ally_table["table_AllyStartDefinitions"] + 6 * member, 6)
        kind, sprite, palette = read(ally_table["table_AllyBattlesprites"] + 9 * member, 3)
        assert kind == start[0]
        initial = next((word & 127 for word in start[2:] if word & 128), None)
        if initial is not None:
            carried.add(initial)
        members.append((kind, sprite, palette, initial))

    masks = {
        item: int.from_bytes(read(stats_table["table_ItemDefinitions"] + 16 * item, 4), "big")
        for item in sorted(carried)
    }
    for kind, sprite, palette, initial in members:
        # The class default supplies the unarmed body/ordinary sequence; the host hides its weapon.
        yield ("ally", sprite, palette, initial)
        for item, mask in masks.items():
            if item != initial and mask & (1 << kind):
                yield ("ally", sprite, palette, item)


def build(rom_path: Path, upstream: Path, output: Path, battle: int = 1) -> dict:
    if battle not in (*range(1, 15), 16, 17):
        raise ValueError("unsupported battle-scene selection")
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
        if name in rasters:
            return name
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

    cursor, cursor_provenance = single_cell_cursor(rom)
    cursor_tiles = decode_stack_compressed(read(0x23658, 256), expected_output_bytes=2048).output
    raster(TARGET_CURSOR_RESOURCE, cursor_tiles, palette(0x309E), 32, 32, sprite_layout(4, 4))
    rasters[TARGET_CURSOR_RESOURCE] = cursor
    sprites = table("battle-sprite-decode")
    animations = table("battle-sprite-animation-static")
    weapons = table("battle-weapon-ground-decode")
    actor_rows = []
    actor_selections = list(
        campaign_ally_visuals(
            battle, read, table("ally-data-static"), table("core-stats-data-static")
        )
    )
    # Preserve the existing selected enemy resources independently of carried ally weapons.
    if battle == 1:
        enemies = [(22, 2)]
    elif battle in (2, 3):
        enemies = [(0, 1), (1, 0)] + ([(23, 0)] if battle == 3 else [])
    elif battle == 4:
        enemies = [(2, 0), (31, 0), (23, 0)]
    elif battle <= 7:
        enemies = [(2, 0), (3, 0), (31, 0), (36, 0)]
        if battle >= 6:
            enemies.append((40, 0))
        if battle == 7:
            enemies.append((22, 0))
    elif battle <= 10:
        enemies = [(32, 0), (37, 0), (4, 0), (0, 0)]
        if battle >= 9:
            enemies.append((23, 2))
        if battle == 10:
            enemies.append((5, 2))
    else:
        enemies = []
        source = (upstream / f"disasm/data/battles/spritesets/spriteset{battle:02d}.asm").read_text(
            encoding="utf-8"
        )
        species = set(re.findall(r"enemyCombatant\s+(\w+)", source))
        enums = (upstream / "disasm/sf2enums.asm").read_text(encoding="utf-8")
        for code in sorted(species):
            enemy = int(re.search(rf"^ENEMY_{code}:\s+equ (\d+)", enums, re.MULTILINE)[1])
            sprite, palette_index = read(0x1F914 + enemy * 2, 2)
            enemies.append((sprite, palette_index))
    actor_selections.extend(("enemy", sprite, palette, None) for sprite, palette in enemies)
    for side, index, pal_index, weapon_item in actor_selections:
        graphic = read(0x1F9E2 + (weapon_item - 26) * 2, 2) if weapon_item is not None else None
        draw_weapon = graphic is not None and graphic[0] != 255
        base = pointer(sprites[side + "BattlespriteTableAddress"] + index * 4)
        pal_offset = base + 4 + word(base + 4)
        pal = palette(pal_offset + pal_index * 32)
        frames = []
        for frame in range((pal_offset - base - 6) // 2):
            offset = base + 6 + frame * 2
            data = decode(offset + word(offset), 4608 if side == "ally" else 6144)
            frames.append(
                raster(
                    f"{side}{index}"
                    + (f"-palette{pal_index}" if battle >= 11 else "")
                    + f"/frame{frame}",
                    data,
                    pal,
                    96 if side == "ally" else 128,
                    96,
                    sprite_layout(12 if side == "ally" else 16, 12),
                )
            )
        sequences = {}
        # GetAllyAnimation specializes KNTE attacks for graphics SPEAR..JAVELIN.
        throw_spear = (
            side == "ally" and index == 2 and graphic is not None and 12 <= graphic[0] <= 13
        )
        for purpose, animation in (
            ("attack", 80 if throw_spear else index),
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
                        weapon=weapon(raw[4:]) if draw_weapon else None,
                    )
                )
            sequences[purpose] = dict(
                index=animation,
                trigger=data[1],
                spell=data[2],
                terminate=data[3],
                # GetWeaponspriteAndPalette returns -1 for an unarmed ally;
                # battlesceneengine_0 skips its weapon animation operands.
                idleWeapon=weapon(data[4:8]) if draw_weapon else None,
                frames=rows,
            )
        if side == "ally":
            # USE_ITEM=2 bypasses the KNTE/spear attack specialization and preserves
            # the caller's NONE selector in sub_184B0; GetAllyAnimation selects index.
            sequences["item"] = dict(sequences["idle"])
            if index == 1:
                # PRST CAST_SPELL uses its ordinary class animation, retaining the
                # caller's HEALING_FAIRY selector (header FF), not item NONE.
                sequences["cast"] = dict(sequences["idle"])
        else:
            # The selected early enemies have no special casting animation.
            # createbattlesceneanimation.asm selects ATTACK for them; GetEnemyAnimation
            # resolves that through the same species index as the ordinary attack.
            sequences["cast"] = dict(sequences["attack"])
        weapon_frames = []
        if draw_weapon:
            # table_WeaponGraphics at 0x1F9E2, source getweaponspriteandpalette.asm.
            wid, pid = graphic
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
                idleTicks=word(base),
            )
        )

    base_palette = palette(0x198A8)  # palette_BattlesceneBase, battlesceneengine_1.asm.
    if battle == 16:
        # LoadSpecialSprite selects palette 4, then 72 tiles at VRAM AF00.
        # UpdateSpecialSprite builds each pose from four column-major 3x3 sprites.
        for frame, tiles, colors, layout in special_battle_sprite_frames(
            decode, palette, read, 0x261A6
        ):
            raster(f"field-kraken-head/frame{frame}", tiles, colors, 48, 48, layout)
    backgrounds = table("battle-background-decode")
    layout = [int.from_bytes(read(0x1FAEA + i * 2, 2), "big") for i in range(384)]
    layout = [(value & 0x1800) | ((value & 2047) - 928) for value in layout]
    terrain_scenes = []
    backdrop = {}
    fixed = read(0x1FA8A + battle, 1)[0] if battle >= 11 else 9 if battle == 1 else None
    if fixed == 255:  # OVERWORLD selects the backdrop by terrain.
        fixed = None
    indices = [fixed] if fixed is not None else sorted(set(read(0x19E5E, 16)))
    for index in indices:
        ground = pointer(weapons["groundTableAddress"] + index * 4)
        ground_palette = list(base_palette)
        for offset, color in enumerate((3, 4, 8)):
            ground_palette[color] = md_palette_color(word(ground + offset * 2))
        ground_asset = raster(
            "ground" + str(index),
            decode(ground + 6 + word(ground + 6), 1536),
            ground_palette,
            96,
            32,
            sprite_layout(12, 4),
        )
        background = pointer(backgrounds["backgroundTableAddress"] + index * 4)
        bgdata = decode(background + word(background), 6144) + decode(
            background + 2 + word(background + 2), 6144
        )
        bgpal = palette(background + 4 + word(background + 4))
        background_asset = raster(
            "background" + str(index), bgdata, bgpal, 256, 96, layout, opaque=True
        )
        backdrop[index] = dict(background=background_asset, ground=ground_asset)
    if fixed is None:
        terrain_scenes = [
            dict(terrain=index, **backdrop[value]) for index, value in enumerate(read(0x19E5E, 16))
        ]
    background_asset, ground_asset = backdrop[fixed if fixed is not None else 0].values()

    # LoadSpellTileset: section-14 pointer, HEALING=6, length and three CRAM
    # replacements precede compressed tiles. Preserve all operands as ROM spans.
    healing_base = pointer(pointer(0x1B800C) + 6 * 4)
    healing_palette = list(base_palette)
    for offset, color in enumerate((9, 13, 14)):
        healing_palette[color] = md_palette_color(word(healing_base + 2 + offset * 2) & 0x0EEE)
    healing_tiles = decode(healing_base + 8, word(healing_base))

    # healingfairy.asm table_LightFairy_offsets: two 4x4 bodies, two 4x2 wings,
    # then the dust tile decrements once per six updates, through five frames.
    def healing_raster(name, tile, columns, rows):
        return raster(
            "healing/" + name,
            healing_tiles,
            healing_palette,
            columns * 8,
            rows * 8,
            [tile + x * rows + y for y in range(rows) for x in range(columns)],
        )

    healing = dict(
        bodies=[healing_raster(f"body{i}", tile, 4, 4) for i, tile in enumerate((0, 16))],
        wings=[healing_raster(f"wing{i}", tile, 4, 2) for i, tile in enumerate((32, 40))],
        dust=[healing_raster(f"dust{i}", 52 - i, 1, 1) for i in range(5)],
    )

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
        274,
        275,
        298,
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
    if battle >= 3:
        text_ids.update((306, 338, 354))
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
        targetCursor=cursor_provenance,
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
            encounter="battle-" + str(battle),
            background=background_asset,
            ground=ground_asset,
            actors=actor_rows,
            rasters=rasters,
            texts=texts,
            memberNames=names,
            healing=healing,
            **({"terrainScenes": terrain_scenes} if fixed is None else {}),
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


def add_field_death(rom_path: Path, upstream: Path, output: Path, scene_path: Path) -> dict:
    """Add only field selectors and effect63 to an existing selected scene document."""
    from sf2tool.compression import decode_basic_compressed
    from sf2tool.remake_asset_build import (
        PLAYER_PALETTE_ADDRESS,
        PLAYER_POINTER_TABLE_ADDRESS,
        _combine_player_halves,
        _render_player_frame,
    )
    from sf2tool.remake_exploration_content import OriginalPrograms

    root = repo_path("")
    output = output.resolve()
    if not output.is_relative_to(root / "local") or output.exists():
        raise ValueError("output must be a new worktree-local ignored directory")
    subprocess.run(["git", "check-ignore", "--quiet", str(output)], cwd=root, check=True)
    commit = subprocess.check_output(
        ["git", "-C", str(upstream), "rev-parse", "HEAD"], text=True
    ).strip()
    rom = rom_path.read_bytes()
    if (
        commit != ACCEPTED_UPSTREAM_COMMIT
        or len(rom) != ACCEPTED_ROM_SIZE
        or digest(rom) != ACCEPTED_ROM_SHA256
    ):
        raise ValueError("canonical input identity drift")
    previous = json.loads(
        (scene_path.parent / "source/battle-scenes/selection.json").read_text(encoding="utf-8")
    )
    if previous["upstreamCommit"] != commit or previous["romSha256"] != ACCEPTED_ROM_SHA256:
        raise ValueError("selected scene provenance drift")
    scene = json.loads(scene_path.read_text(encoding="utf-8"))
    if (
        scene["version"] != 1
        or scene["encounter"]
        not in (
            "battle-1",
            "battle-2",
            "battle-3",
            "battle-4",
            "battle-5",
            "battle-6",
            "battle-7",
            "battle-8",
            "battle-9",
            "battle-10",
            "battle-11",
            "battle-12",
            "battle-13",
            "battle-14",
            "battle-16",
            "battle-17",
        )
        or "fieldDeath" in scene
    ):
        raise ValueError("selected scene field boundary")
    compiler = OriginalPrograms(
        {"resources": {"standaloneScriptPrograms": [], "initSourcePrograms": []}}, upstream
    )
    allies = [
        {"character": r["character"], "sprite": r["sprite"]}
        for r in compiler.initial_ally_sprites()[
            : {
                "battle-1": 3,
                "battle-2": 4,
                "battle-3": 12,
                "battle-4": 5,
                "battle-5": 6,
                "battle-6": 6,
                "battle-7": 7,
                "battle-8": 13,
                "battle-9": 12,
                "battle-10": 12,
                "battle-11": 12,
                "battle-12": 12,
                "battle-13": 12,
                "battle-14": 12,
                "battle-16": 12,
                "battle-17": 12,
            }[scene["encounter"]]
        ]
    ]
    enemy_table = json.loads(
        repo_path("tests/fixtures/h2/enemy-map-sprites-static-v1.json").read_text(encoding="utf-8")
    )["table"]["table_EnemyMapsprites"]
    species = ["GIZMO"] if scene["encounter"] == "battle-1" else ["HUGE_RAT", "OOZE"]
    if scene["encounter"] == "battle-3":
        species.append("HUGE_BAT")
    if scene["encounter"] == "battle-4":
        species = ["GALAM_ARCHER", "GALAM_SOLDIER", "HUGE_BAT"]
    if scene["encounter"] in ("battle-5", "battle-6", "battle-7"):
        species = ["GALAM_ARCHER", "GALAM_SOLDIER", "GALAM_KNIGHT", "GALAM_MAGE"]
        if scene["encounter"] in ("battle-6", "battle-7"):
            species.append("DARK_CLERIC")
    if scene["encounter"] == "battle-7":
        species.append("DARK_SMOKE")
    if scene["encounter"] in ("battle-8", "battle-9", "battle-10"):
        species = ["HUNTER_GOBLIN", "WITCH", "GOBLIN", "GREEN_OOZE"]
    if scene["encounter"] in ("battle-9", "battle-10"):
        species.append("VAMPIRE_BAT")
    if scene["encounter"] == "battle-10":
        species.remove("GREEN_OOZE")
        species.append("DARK_DWARF")
    if scene["encounter"] in (
        "battle-11",
        "battle-12",
        "battle-13",
        "battle-14",
        "battle-16",
        "battle-17",
    ):
        battle = int(scene["encounter"].split("-")[1])
        source = (upstream / f"disasm/data/battles/spritesets/spriteset{battle:02d}.asm").read_text(
            encoding="utf-8"
        )
        species = sorted(set(re.findall(r"enemyCombatant\s+(\w+)", source)))
    enemy_sprites = [
        dict(code=code, sprite=rom[enemy_table + compiler.equates["ENEMY_" + code]])
        for code in species
    ]
    palette = [
        md_palette_color(int.from_bytes(rom[i : i + 2], "big"))
        for i in range(PLAYER_PALETTE_ADDRESS, PLAYER_PALETTE_ADDRESS + 32, 2)
    ]
    effect = compiler.equates["MAPSPRITE_EFFECT1"]
    frames, spans = [], []
    for direction in range(3):
        entry = PLAYER_POINTER_TABLE_ADDRESS + (effect * 3 + direction) * 4
        address = int.from_bytes(rom[entry : entry + 4], "big")
        decoded = decode_basic_compressed(rom[address:], expected_output_bytes=576)
        pixels = bytes(
            _combine_player_halves(
                _render_player_frame(decoded.output[:288], palette),
                _render_player_frame(decoded.output[288:], palette),
            )
        )
        name = f"field-death-{direction}"
        scene["rasters"][name] = dict(
            width=48,
            height=24,
            format="rgba8",
            data=base64.b64encode(pixels).decode("ascii"),
            sha256=digest(pixels),
        )
        frames.append(name)
        spans.append(
            dict(pointerAddress=entry, address=address, byteLength=decoded.input_bytes_consumed)
        )
    scene["fieldDeath"] = dict(allies=allies, enemies=enemy_sprites, exitFrames=frames)
    output.mkdir(parents=True)
    write_json(output / "battle-scenes.json", scene)
    report = dict(
        status="private-local-candidate",
        upstreamCommit=commit,
        romSha256=ACCEPTED_ROM_SHA256,
        effectSprite=effect,
        spans=spans,
        allyAssignments=allies,
        enemyTableAddress=enemy_table,
        enemyAssignments=enemy_sprites,
        paletteAddress=PLAYER_PALETTE_ADDRESS,
        sceneContentBytes=(output / "battle-scenes.json").stat().st_size,
        sources=sorted(
            compiler.sources
            | {
                "disasm/data/stats/enemies/enemymapsprites.asm",
                "disasm/code/gameflow/battle/battleloop/processkilledcombatants.asm",
                "disasm/code/common/scripting/entity/entityscriptengine_2.asm",
            }
        ),
    )
    write_json(output / "field-death-provenance.json", report)
    return report


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--rom", type=Path, required=True)
    parser.add_argument("--upstream", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--battle", type=int, choices=(*range(1, 15), 16, 17), default=1)
    parser.add_argument(
        "--field-death-base",
        type=Path,
        help="Existing selected scene document; add field selectors/effect63 only",
    )
    args = parser.parse_args()
    result = (
        add_field_death(args.rom, args.upstream, args.output, args.field_death_base)
        if args.field_death_base
        else build(args.rom, args.upstream, args.output, args.battle)
    )
    print(json.dumps(result))
