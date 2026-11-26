"""Produce reusable original resources from the registered USA input, one family at a time."""

import argparse
import csv
import hashlib
import json
import re
import shutil
import tempfile
import time
from pathlib import Path

from sf2tool import texture_extract as texture
from sf2tool.compression import decode_stack_compressed
from sf2tool.graphics_source import _verified_source
from sf2tool.h2.map_content import (
    SECTION_LAYOUT,
    _encode_source,
    _entry_pointer_expressions,
    _parse_equates,
)
from sf2tool.h2.map_import import _decode_source_table
from sf2tool.h2.map_layouts import decode_map_blocks, decode_map_layout
from sf2tool.h2.portraits import _read_animation_entries
from sf2tool.h2.special_screen_graphics import RESOURCES as SCREEN_RESOURCES
from sf2tool.paths import repo_path
from sf2tool.private_inputs import ROM_INPUT_IDENTITY, private_input_path
from sf2tool.remake_authored_audio import is_authored_defeat
from sf2tool.remake_battle_scene_content import (
    build,
    campaign_selection,
    compose,
    sprite_layout,
    write_json,
)
from sf2tool.remake_exploration_visuals import source_map_atlas
from sf2tool.remake_identity_icons import identity_assets
from sf2tool.remake_private_inventory import PIN, fixture, prepared_coverage
from sf2tool.remake_private_package import mount, private_output, read_json
from sf2tool.remake_retail_audio import prepare_commands
from sf2tool.rom_split_lookup import read_splits

FAMILIES = (
    "icons",
    "walking",
    "portraits",
    "maps",
    "bodies",
    "weapons",
    "terrain",
    "effects",
    "ui",
    "entry",
    "sfx",
    "music",
    "pldn",
)


def word(data, pos):
    return int.from_bytes(data[pos : pos + 2], "big")


def pointer(data, pos):
    return int.from_bytes(data[pos : pos + 4], "big")


def colors(data):
    return [texture.md_palette_color(word(data, i) & 0xEEE) for i in range(0, len(data), 2)]


class RetailSource:
    """Use pinned split extents with H1 addresses; no mutable SF2DISASM split checkout."""

    def __init__(self, upstream, listing):
        self.upstream = upstream
        self.listing = listing
        self.path = private_input_path(ROM_INPUT_IDENTITY)
        source, self.pin, identity = _verified_source(
            self.path, upstream, owner="retail preparation", listing_path=listing
        )
        self.sha = identity["hashes"]["sha256"]
        self.disasm, self.rom, self.addresses = source.disasm, source.rom, source.addresses
        self.splits = read_splits(upstream)

    def read(self, path, *, symbol, expected_sha256=None):
        span = self.splits[Path(path).as_posix()]
        if self.addresses[symbol] != span.start or span.end > len(self.rom):
            raise ValueError(f"split and listing disagree: {symbol}")
        data = self.rom[span.start : span.end]
        if expected_sha256 and hashlib.sha256(data).hexdigest().upper() != expected_sha256:
            raise ValueError(f"split digest mismatch: {symbol}")
        return data

    def slots(self, name, count):
        base = self.addresses[name]
        return [pointer(self.rom, base + 4 * i) for i in range(count)]


def dump(out, name, value):
    write_json(out / name, value)
    return name


def png(out, name, width, height, pixels):
    path = out / name
    path.parent.mkdir(parents=True, exist_ok=True)
    texture.write_png_rgba(path, width, height, pixels)
    return name


def icons(src, out):
    images, receipt = identity_assets(src.path, src.upstream, src.listing, True, all_storage=True)
    if len(images) != 326:
        raise ValueError("assembled icon count drift")
    dump(out, "identity-icons.json", images)
    return receipt["resources"]


def walking(src, out):
    files = texture.extract_map_sprites(src, out)
    names = {src.addresses[Path(r["file"]).stem]: r["file"] for r in files}
    sentinel = src.addresses["Mapsprite237_0"]
    slots = src.slots("pt_Mapsprites", 720)
    dialogue = {}
    address = src.addresses["table_MapspriteDialogueProperties"]
    while word(src.rom, address) != 65535:
        sprite, portrait, speech = src.rom[address : address + 3]
        dialogue.setdefault(sprite, (portrait if portrait < 128 else None, speech))
        address += 4
    if len(dialogue) != 119:
        raise ValueError("mapsprite dialogue table boundary drift")
    records = []
    for sprite in range(240):
        directions = [
            None if a == sentinel else names[a] for a in slots[sprite * 3 : sprite * 3 + 3]
        ]
        portrait, speech = dialogue.get(sprite, (None, 74))
        records.append(dict(sprite=sprite, directions=directions, portrait=portrait, speech=speech))
    # Source retains the separate routed action streams, including animation-only variants.
    bindings = []
    for member in range(30):
        base = src.rom[src.addresses["table_AllyMapsprites"] + member]
        first = src.addresses["table_AllyBattlesprites"] + 9 * member
        for offset in (0, 3, 6):
            kind, body, variant = src.rom[first + offset : first + offset + 3]
            if kind == 255:
                continue
            if kind == 12 or kind >= 23:
                field = base
            elif kind == 0 or 6 <= kind <= 11:
                field = base - 1
            elif kind <= 5:
                field = base - 2
            else:
                field = base - (kind & 1)
            bindings.append(
                dict(
                    member=member,
                    slot=offset // 3,
                    sourceClass=kind,
                    sprite=field,
                    battleSprite=body,
                    battlePalette=variant,
                )
            )
    extra = {}
    for path, span in src.splits.items():
        if path.startswith("data/graphics/specialsprites/"):
            data = src.rom[span.start : span.end]
            offset = 0 if span.start == src.addresses["SpecialSprite_EvilSpiritAlt"] else 32
            name = Path(path).stem + ".4bpp"
            (out / name).write_bytes(decode_stack_compressed(data[offset:]).output)
            extra[path] = dict(tiles=name, paletteWords=data[:offset].hex())
    return dict(
        sprites=records,
        classes=bindings,
        special=extra,
        specialSlots=src.slots("pt_SpecialSprites", 10),
        deadFieldSprite=190,
        unjoinedRohdeSprite=170,
    )


def portraits(src, out):
    files = texture.extract_portraits(src, out)
    records = {}
    for row in files:
        symbol = Path(row["file"]).stem
        address = src.addresses[symbol]
        eyes, offset = _read_animation_entries(src.rom, address, "eye")
        mouth, _ = _read_animation_entries(src.rom, offset, "mouth")
        records[address] = dict(raster=row["file"], eyes=eyes, mouth=mouth)
    return [dict(portrait=i, **records[a]) for i, a in enumerate(src.slots("pt_Portraits", 56))]


def maps(src, out):
    text = (src.disasm / "data/maps/entries.asm").read_text()
    equates = _parse_equates(src.disasm)
    refs = {
        m[1]: m[2].replace(chr(92), "/")
        for m in re.finditer(r'^(\w+):\s*(?:incbin|include)\s+"([^"]+)"', text, re.M)
    }
    palettes = {}
    for i, addr in enumerate(src.slots("pt_MapPalettes", 16)):
        palettes[i] = [word(src.rom, addr + n * 2) for n in range(16)]
    for i, addr in enumerate(src.slots("pt_MapTilesets", 115)):
        tiles = decode_stack_compressed(src.rom[addr:], expected_output_bytes=4096).output
        (out / f"tileset-{i}.4bpp").write_bytes(tiles)
    records = []
    for i, addr in enumerate(src.slots("pt_MapData", 79)):
        palette, *tilesets = src.rom[addr : addr + 6]
        definition = dict(id=i, palette=palette, tilesets=tilesets)
        symbols = _entry_pointer_expressions(text, i)
        blocks_symbol, layout_symbol = symbols[:2]
        blocks_path = out / (blocks_symbol + ".json")
        if not blocks_path.exists():
            words, _, _ = decode_map_blocks(src.read(refs[blocks_symbol], symbol=blocks_symbol))
            write_json(blocks_path, [words[j : j + 9] for j in range(0, len(words), 9)])
        layout_path = out / (layout_symbol + ".json")
        if not layout_path.exists():
            blocks = read_json(blocks_path)
            layout, _, _, _ = decode_map_layout(
                src.read(refs[layout_symbol], symbol=layout_symbol), len(blocks)
            )
            write_json(layout_path, layout)
        layers = {}
        for slot in (2, 3, 4, 5, 9):
            symbol = symbols[slot]
            if symbol not in refs:
                if symbol != "$FFFFFFFF":
                    raise ValueError(f"unknown map component: {symbol}")
                continue
            kind = SECTION_LAYOUT[slot - 1][0]
            encoded, count, tail = _encode_source(src.disasm / refs[symbol], kind, equates)
            a = src.addresses[symbol]
            if src.rom[a : a + len(encoded)] != encoded:
                raise ValueError(f"map component ROM mismatch: {symbol}")
            layers[kind] = _decode_source_table(kind, encoded, count, tail)
        atlas = png(out, f"map-{i}.png", 128, 320, source_map_atlas(src.rom, definition))
        records.append(
            dict(
                **definition,
                atlas=atlas,
                blocks=blocks_path.name,
                layout=layout_path.name,
                **layers,
            )
        )
    return dict(palettes=palettes, maps=records)


def bodies(src, out):
    enemy_base = src.addresses["table_EnemyBattlesprites"]
    dump(
        out,
        "enemy-species.json",
        [list(src.rom[a : a + 2]) for a in range(enemy_base, enemy_base + 103 * 2, 2)],
    )
    records = []
    anim = fixture("battle-sprite-animation-static")
    for side, count in (("ally", 32), ("enemy", 54)):
        width = 96 if side == "ally" else 128
        slots = src.slots("pt_" + side.title() + "Battlesprites", count)
        for i, a in enumerate(slots):
            if side == "ally" and i == 13:
                continue
            start = a + 4 + word(src.rom, a + 4)
            offsets = [j + word(src.rom, j) for j in range(a + 6, start, 2)]
            palettes = [colors(src.rom[j : j + 32]) for j in range(start, offsets[0], 32)]
            frames = [
                decode_stack_compressed(src.rom[j:], expected_output_bytes=width * 96 // 2).output
                for j in offsets
            ]
            for variant, pal in enumerate(palettes):
                paths = [
                    png(
                        out,
                        f"{side}{i}-palette{variant}/frame{n}.png",
                        width,
                        96,
                        compose(data, pal, width, 96, sprite_layout(width // 8, 12)),
                    )
                    for n, data in enumerate(frames)
                ]
                records.append(
                    dict(
                        side=side,
                        sprite=i,
                        palette=variant,
                        frames=paths,
                        idleTicks=word(src.rom, a),
                        statusOffset=list(src.rom[a + 2 : a + 4]),
                    )
                )
        size = 8 if side == "ally" else 4
        sequences = []
        for address in src.slots(
            "pt_" + side.title() + "Animations", anim["summary"][side]["animationCount"]
        ):
            n = src.rom[address]
            sequences.append(
                [
                    list(src.rom[j : j + size])
                    for j in range(address, address + (n + 1) * size, size)
                ]
            )
        dump(out, side + "-animations.json", sequences)
    return records


def weapons(src, out):
    table = fixture("battle-weapon-ground-decode")["table"]
    for i, a in enumerate(src.slots("pt_Weaponsprites", 23)):
        data = decode_stack_compressed(src.rom[a:], expected_output_bytes=8192).output
        (out / f"weapon-{i}.4bpp").write_bytes(data)
    base = table["weaponPaletteAddress"]
    return dict(
        palettes=[[word(src.rom, a), word(src.rom, a + 2)] for a in range(base, base + 168, 4)],
        items={
            i: list(src.rom[0x1F9E2 + (i - 26) * 2 : 0x1F9E4 + (i - 26) * 2])
            for i in range(26, 26 + 84)
        },
    )


def terrain(src, out):
    bg_layout = [word(src.rom, 0x1FAEA + i * 2) for i in range(384)]
    bg_layout = [(v & 0x1800) | ((v & 2047) - 928) for v in bg_layout]
    resources = {}
    for kind, name in (("ground", "pt_Grounds"), ("background", "pt_Backgrounds")):
        slots = src.slots(name, 30)
        results = {}
        for i, a in enumerate(slots):
            if a in results:
                continue
            if kind == "ground":
                pal = colors(src.rom[0x198A8:0x198C8])
                for idx, color in zip((3, 4, 8), colors(src.rom[a : a + 6]), strict=True):
                    pal[idx] = color
                data = decode_stack_compressed(
                    src.rom[a + 6 + word(src.rom, a + 6) :], expected_output_bytes=1536
                ).output
                w, h, layout = 96, 32, sprite_layout(12, 4)
            else:
                p = a + 4 + word(src.rom, a + 4)
                pal = colors(src.rom[p : p + 32])
                data = b"".join(
                    decode_stack_compressed(
                        src.rom[j + word(src.rom, j) :], expected_output_bytes=6144
                    ).output
                    for j in (a, a + 2)
                )
                w, h, layout = 256, 96, bg_layout
            results[a] = png(
                out,
                f"{kind}-{i}.png",
                w,
                h,
                compose(data, pal, w, h, layout, opaque=kind == "background"),
            )
        resources[kind] = [results[a] for a in slots]
    return resources


def effects(src, out):
    records = []
    base = src.addresses["palette_BattlesceneBase"]
    dump(out, "base-palette.json", [word(src.rom, base + n * 2) for n in range(16)])
    # The contiguous Bolt VDP operand tables end at the ally animation pointer table.
    source = (src.disasm / "data/graphics/battles/spells/animations/boltanimdata.asm").read_text()
    symbols = re.findall(r"^(BoltAnimData_\w+):", source, re.M)
    addresses = [src.addresses[key] for key in symbols] + [src.addresses["pt_AllyAnimations"]]
    for i, symbol in enumerate(symbols):
        start, end = addresses[i : i + 2]
        if end <= start:
            raise ValueError("Bolt animation table boundary drift")
        name = symbol + ".bin"
        (out / name).write_bytes(src.rom[start:end])
        records.append(dict(family="bolt-sprite-operands", symbol=symbol, file=name))
    for i, a in enumerate(src.slots("pt_SpellGraphics", 23)):
        size = word(src.rom, a)
        data = decode_stack_compressed(src.rom[a + 8 :], expected_output_bytes=size).output
        name = f"spell-{i}.4bpp"
        (out / name).write_bytes(data)
        records.append(dict(family="spell", id=i, tiles=name, palette=src.rom[a + 2 : a + 8].hex()))
    for i, a in enumerate(src.slots("pt_InvocationSprites", 4)):
        p = a + 6 + word(src.rom, a + 6)
        names = []
        for n, j in enumerate(range(a + 8, p, 2)):
            name = f"invocation-{i}-{n // 2}-{n % 2}.4bpp"
            (out / name).write_bytes(
                decode_stack_compressed(
                    src.rom[j + word(src.rom, j) :], expected_output_bytes=4096
                ).output
            )
            names.append(name)
        records.append(
            dict(family="invocation", id=i, streams=names, palette=src.rom[p : p + 32].hex())
        )
    for i, a in enumerate(
        [src.addresses["tiles_StatusAnimation"], *src.slots("pt_BattlesceneTransitionTiles", 2)]
    ):
        name = ("status" if i == 0 else f"transition-{i - 1}") + ".4bpp"
        (out / name).write_bytes(
            decode_stack_compressed(
                src.rom[a:], expected_output_bytes=1248 if i == 0 else 6144
            ).output
        )
        records.append(dict(tiles=name))
    return records


def ui(src, out):
    files = texture.extract_ui_resources(src, out)
    files.append(texture.extract_font_sheet(src, out))
    for spec in SCREEN_RESOURCES:
        data = src.read(spec["sourcePath"], symbol=spec["symbol"])
        decoded = decode_stack_compressed(data, expected_output_bytes=spec["expectedDecodedBytes"])
        name = spec["symbol"] + ".4bpp"
        (out / name).write_bytes(decoded.output)
        files.append(dict(file=name, symbol=spec["symbol"], sizeBytes=len(decoded.output)))
    # Preserve the remaining title, witch, ending, menu and window source components by split ID.
    for path, span in src.splits.items():
        if path.startswith(("data/graphics/specialscreens/", "data/graphics/tech/")):
            target = out / path
            target.parent.mkdir(parents=True, exist_ok=True)
            target.write_bytes(src.rom[span.start : span.end])
            files.append(dict(file=path, sizeBytes=span.end - span.start))
    return files


def pldn(src, out, members):
    # Use the established Battle26 producer and retain only the requested appearance rows.
    selected = campaign_selection()
    selected["growthClasses"] = [r for r in selected["growthClasses"] if r["member"] in members]
    selected["promotionClasses"] = [{"member": m, "class": "PLDN"} for m in members]
    with tempfile.TemporaryDirectory(dir=repo_path("local"), prefix="retail-pldn-") as temp:
        work = Path(temp)
        selection = work / "selection.json"
        write_json(selection, selected)
        build(src.path, src.upstream, work / "body", battle=26, selection=selection)
        scene = read_json(work / "body/battle-scenes.json")
        actors = [
            r
            for r in scene["actors"]
            if r["side"] == "ally" and r["sprite"] == 13 and r["palette"] in members.values()
        ]
        for m, p in members.items():
            if not any(m in row["members"] and row["palette"] == p for row in actors):
                raise ValueError(f"missing PLDN member {m}")
        for row in actors:
            item = row["item"]
            graphic = src.rom[0x1F9E2 + (item - 26) * 2] if item is not None else 255
            attack = 81 if graphic in (12, 13) else 13
            if row["sequences"]["attack"]["index"] != attack or len(row["frames"]) != 5:
                raise ValueError("PLDN attack or body frame binding differs")
        ids = {k for row in actors for k in row["frames"] + row["weaponFrames"]}
        dump(
            out,
            "scene-additions.json",
            dict(actors=actors, rasters={k: scene["rasters"][k] for k in ids}),
        )
    return dict(members=members, actors=len(actors), rasterCount=len(ids))


PRODUCERS = {
    "icons": icons,
    "walking": walking,
    "portraits": portraits,
    "maps": maps,
    "bodies": bodies,
    "weapons": weapons,
    "terrain": terrain,
    "effects": effects,
    "ui": ui,
    "pldn-party": lambda src, out: pldn(src, out, {2: 0, 12: 1}),
    "pldn-eric": lambda src, out: pldn(src, out, {14: 3}),
}


def prepare(src, family, root, base, inventory, commands):
    if family == "pldn":
        for part in ("pldn-party", "pldn-eric"):
            prepare(src, part, root, base, inventory, commands)
        return
    destination = root / family
    if family in ("sfx", "music"):
        destination.mkdir(exist_ok=True)
        selected = read_json(base / "world.json")["world"]["presentation"]["audio"]
        selected += read_json(base / "entry/entry.json")["presentation"]["audio"]
        available = {r["command"] for r in selected}
        slots = {int(r["id"]): r["status"] for r in inventory if r["family"] == family}
        expected = range(1, 65) if family == "music" else range(65, 121)
        if set(slots) != set(expected) or any(
            (status == "present") != (c in available) for c, status in slots.items()
        ):
            raise ValueError(f"selected package and pinned {family} inventory differ")
        prepare_commands(
            family,
            inventory,
            destination,
            src.path,
            src.rom,
            commands=commands,
            authored_commands={r["command"] for r in selected if is_authored_defeat(r)},
        )
        return
    missing = []
    if destination.exists():
        manifest = read_json(destination / "index.json")
        if manifest["upstream"] != PIN or manifest["rom"] != src.sha:
            raise ValueError(f"{family} prepared input differs")
        for name, size in manifest["files"].items():
            p = (destination / name).resolve()
            if not p.is_relative_to(destination):
                raise ValueError(f"{family} invalid output path: {name}")
            if not p.exists():
                missing.append(name)
            elif p.stat().st_size != size:
                raise ValueError(f"{family} changed output: {name}")
        if not missing:
            return
    with tempfile.TemporaryDirectory(prefix="." + family + "-", dir=root) as temp:
        work = Path(temp) / family
        work.mkdir()
        start = time.monotonic()
        if family == "entry":
            entry = read_json(base / "entry/entry.json")
            files = ["entry.json", *[r["file"] for r in entry["resources"]]]
            for name in files:
                if Path(name).name != name:
                    raise ValueError("unexpected entry resource path")
                shutil.copyfile(base / "entry" / name, work / name)
            records = files
        else:
            records = PRODUCERS[family](src, work)
        sizes = {
            p.relative_to(work).as_posix(): p.stat().st_size for p in work.rglob("*") if p.is_file()
        }
        dump(work, "index.json", dict(rom=src.sha, upstream=src.pin, records=records, files=sizes))
        if missing:
            if read_json(work / "index.json") != manifest:
                raise ValueError(f"{family} no longer reproduces the selected version")
            for name in missing:
                target = destination / name
                target.parent.mkdir(parents=True, exist_ok=True)
                (work / name).rename(target)
        else:
            work.rename(destination)
        print(
            f"{family}: {len(sizes)} payload files, {sum(sizes.values())} bytes, "
            f"{time.monotonic() - start:.2f}s",
            flush=True,
        )


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--gfx-root", type=Path)
    for name in ("root", "base"):
        parser.add_argument("--" + name, type=Path, required=True)
    for name in ("upstream", "listing", "inventory", "mount", "coverage", "canonical"):
        parser.add_argument("--" + name, type=Path)
    parser.add_argument(
        "--family",
        action="append",
        choices=(
            *FAMILIES,
            "pldn-party",
            "pldn-eric",
            "pgnt",
            "brn",
            "brgn",
            "mmnk",
            "sorc",
            "wiz",
            "melee",
            "ranged",
            "vicar",
            "innate",
            "ninja",
            "monster",
            "phoenix-wave",
            "blaze",
            "blaze-2",
            "flame-family",
            "freeze-family",
            "bolt-family",
            "blast-family",
            "desoul-family",
            "dao-family",
            "apollo-family",
            "neptun-family",
            "atlas-family",
            "blast-1",
            "indra",
            "static-equipment",
            "equipment",
            "south-shrine",
            "all",
        ),
        required=True,
    )
    parser.add_argument("--command", type=int, action="append", default=[])
    args = parser.parse_args()
    if {
        "pgnt",
        "brn",
        "brgn",
        "mmnk",
        "sorc",
        "wiz",
        "melee",
        "ranged",
        "vicar",
        "innate",
        "ninja",
        "monster",
        "phoenix-wave",
        "blaze",
        "blaze-2",
        "flame-family",
        "freeze-family",
        "bolt-family",
        "blast-family",
        "desoul-family",
        "dao-family",
        "apollo-family",
        "neptun-family",
        "atlas-family",
        "blast-1",
        "indra",
        "static-equipment",
        "equipment",
        "south-shrine",
    }.intersection(args.family) and not args.mount:
        parser.error("this family selects already prepared assets for --mount")
    if args.coverage:
        if args.inventory is None:
            parser.error("coverage needs the base --inventory")
        output = private_output(args.coverage)
        print(json.dumps(prepared_coverage(args.inventory, args.root, args.base, output)))
        return
    if args.mount:
        mount(
            args.root,
            args.base,
            args.mount,
            args.family,
            args.command,
            upstream=args.upstream,
            canonical_path=args.canonical,
            gfx_root=args.gfx_root,
        )
        return
    if not all((args.upstream, args.listing, args.inventory)):
        parser.error("production requires --upstream, --listing and --inventory")
    root = private_output(args.root)
    root.mkdir(parents=True, exist_ok=True)
    if shutil.disk_usage(root).free < 1 << 30:
        raise OSError("retail preparation needs 1 GiB free headroom")
    src = RetailSource(args.upstream, args.listing)
    with args.inventory.open(encoding="utf-8-sig") as f:
        inventory = list(csv.DictReader(f))
    selected = FAMILIES if "all" in args.family else dict.fromkeys(args.family)
    if root.is_relative_to(args.base.resolve()) or args.base.resolve().is_relative_to(root):
        parser.error("select separate base and output directories")
    if args.command and (
        set(selected) - {"music", "sfx"}
        or any(
            not any(
                r["family"] in selected and r["status"] != "not-applicable" and r["id"] == str(c)
                for r in inventory
            )
            for c in args.command
        )
    ):
        parser.error("select applicable audio commands in the named audio family")
    errors = {}
    for family in selected:
        try:
            prepare(src, family, root, args.base, inventory, args.command)
        except (ValueError, OSError, KeyError) as error:
            errors[family] = str(error)
            print(f"{family} FAILED: {error}", flush=True)
    if errors:
        raise SystemExit(json.dumps(errors))


if __name__ == "__main__":
    main()
