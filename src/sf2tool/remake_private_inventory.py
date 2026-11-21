"""Inventory pinned source IDs and compare them with explicitly selected private inputs."""

from __future__ import annotations

import argparse
import csv
import json
import re
import subprocess
from collections import Counter
from pathlib import Path

from sf2tool.h2.map_content import _entry_pointer_expressions
from sf2tool.paths import repo_path
from sf2tool.remake_command_menu import world_dependencies
from sf2tool.remake_private_package import private_output, read_json

PIN = "c834c652b6862bc5679fd7f69a38a7093206efc6"
GRAPHICS = "data/graphics/"
PLDN = {2: (6, 0), 12: (28, 1), 14: (34, 3), 19: (None, 2)}


def git(root: Path, *args: str) -> str:
    return subprocess.check_output(["git", "-C", str(root), *args]).decode(
        "utf-8", errors="replace"
    )


def fixture(name: str) -> dict:
    return read_json(repo_path(f"tests/fixtures/h2/{name}-v1.json"))


def inventory(
    upstream: Path, package: Path, asset_root: Path, asset_commit: str, catalog: Path
) -> list[tuple]:
    if git(upstream, "rev-parse", "HEAD").strip() != PIN:
        raise ValueError("upstream checkout does not select the pinned commit")
    if not re.fullmatch(r"[0-9a-f]{40}", asset_commit):
        raise ValueError("select the full private asset Git commit")
    manifest = json.loads(
        git(asset_root, "show", f"{asset_commit}:manifests/presentation-assets-v1.json")
    )

    def source(path: str) -> str:
        return git(upstream, "show", f"{PIN}:disasm/{path}")

    enums = source("sf2enums.asm")
    world = package / "world.json"
    document = read_json(world)
    world_dependencies(document, world)
    presentation = document["world"]["presentation"]
    installed_maps = {int(row["map"].removeprefix("map-")) for row in presentation["maps"]}
    installed_sprites = {
        row["sprite"] for row in presentation["sprites"] if len(row["directions"]) == 3
    }
    installed_portraits = {row["portrait"] for row in presentation["portraits"]}
    battle = None
    for row in reversed(document["encounters"]):
        if row["battleScenes"] is not None:
            scene = read_json(package / row["battleScenes"])
            if scene["encounter"] == "battle-26":
                battle = scene
                break
    if battle is None:
        raise ValueError("selected package does not have the Battle26 inventory consumer")
    entry = read_json(package / "entry/entry.json")
    names = {(row["namespace"], row["index"]) for row in read_json(catalog)["names"]}
    world_audio = presentation.get("audio", []) + entry["presentation"]["audio"]
    library_audio = [row for row in manifest["assets"] if row["kind"] == "audio"]
    del document, entry

    rows = []

    def add(family, index, symbol, owner, status="source-only", prepared=""):
        rows.append((family, index, symbol, owner, status, prepared))

    def pointers(family, path, table, count, classify=None):
        # These pinned tables use contiguous single-symbol dc.l entries. Read only that table.
        text = source(path).partition(f"\n{table}:")[2]
        entries = []
        for line in text.splitlines():
            operand = line.split(";", 1)[0].strip()
            if not operand:
                continue
            match = re.fullmatch(r"(?:\w+:\s*)?dc\.l\s+(\w+)", operand)
            if not match:
                break
            entries.append(match[1])
        if len(entries) != count:
            raise ValueError(f"{table} pointer count drift: {len(entries)} versus {count}")
        for index, symbol in enumerate(entries):
            status, prepared = classify(index) if classify else ("source-only", "")
            add(family, index, symbol, f"{path}#{table}", status, prepared)

    def coverage(installed, index, key):
        return ("present", key) if index in installed else ("missing", key)

    map_path = "data/maps/entries.asm"
    map_source = source(map_path)
    map_count = fixture("canonical-map-import")["summary"]["mapCount"]
    pointers(
        "map",
        map_path,
        "pt_MapData",
        map_count,
        lambda i: coverage(installed_maps, i, f"world.presentation.maps[map-{i}]"),
    )
    for index in range(map_count):
        metadata = f"data/maps/entries/map{index:02}/00-tilesets.asm"
        header = source(metadata)
        palette = re.search(r"\bmapPalette\s+(\d+)", header)
        tiles = re.findall(r"\bmapTileset([1-5])\s+(\d+)", header)
        if palette is None or [slot for slot, _ in tiles] != list("12345"):
            raise ValueError(f"map header shape drift: {metadata}")
        add("map-palette-reference", index, f"palette:{palette[1]}", metadata)
        for slot, tileset in tiles:
            add(
                "map-tileset-reference",
                f"{index}:{slot}",
                tileset,
                metadata,
                "not-applicable" if tileset == "255" else "source-only",
            )
        for offset, family in (
            (0, "map-blocks"),
            (1, "map-layout"),
            (2, "map-areas"),
            (5, "map-roof-foreground"),
            (9, "map-tile-animation"),
        ):
            symbol = _entry_pointer_expressions(map_source, index)[offset]
            absent = symbol in ("0", "$0", "$00000000", "$FFFFFFFF")
            add(
                family,
                index,
                symbol,
                map_path,
                "not-applicable" if absent else "source-only",
            )

    palettes = fixture("map-palette-static")["summary"]["paletteCount"]
    tilesets = fixture("map-tileset-decode")["summary"]["tilesetCount"]
    pointers("map-palette", GRAPHICS + "maps/mappalettes/entries.asm", "pt_MapPalettes", palettes)
    pointers("map-tileset", GRAPHICS + "maps/maptilesets/entries.asm", "pt_MapTilesets", tilesets)
    walking = fixture("map-sprite-decode")
    sentinels = set(walking["sentinel"]["pointerSlots"])
    pointers(
        "walk-direction-strip",
        GRAPHICS + "mapsprites/entries.asm",
        "pt_Mapsprites",
        walking["summary"]["pointerCount"],
        lambda i: (
            ("not-applicable", "special routing or sentinel")
            if i in sentinels
            else coverage(installed_sprites, i // 3, f"world.sprite[{i // 3}].directions[{i % 3}]")
        ),
    )
    pointers(
        "portrait",
        GRAPHICS + "portraits/entries.asm",
        "pt_Portraits",
        fixture("portrait-graphics-decode")["summary"]["portraitPointerCount"],
        lambda i: coverage(installed_portraits, i, f"world.portrait[{i}]"),
    )
    special = fixture("special-sprite-decode")["summary"]
    pointers(
        "special-map-sprite",
        GRAPHICS + "specialsprites/pointers.asm",
        "pt_SpecialSprites",
        special["pointerCount"],
    )

    sprites = fixture("battle-sprite-decode")["sideSummaries"]
    animations = fixture("battle-sprite-animation-static")["summary"]
    for summary in sprites:
        side = summary["side"]
        location = GRAPHICS + f"battles/battlesprites/{'allies' if side == 'ally' else 'enemies'}/"
        pointers(
            f"battle-{side}-body",
            location + "entries.asm",
            f"pt_{side.title()}Battlesprites",
            summary["pointerCount"],
        )
        pointers(
            f"battle-{side}-animation",
            location + "animations/entries.asm",
            f"pt_{side.title()}Animations",
            animations[side]["animationCount"],
        )
    scene_counts = fixture("battle-weapon-ground-decode")["sideSummaries"]
    counts = {row["kind"]: row for row in scene_counts}
    pointers(
        "battle-weapon",
        GRAPHICS + "battles/weapons/entries.asm",
        "pt_Weaponsprites",
        counts["weapon"]["pointerCount"],
    )
    weapon_palettes = GRAPHICS + "battles/weapons/palettes/entries.asm"
    for index, symbol in enumerate(
        re.findall(r"^(WeaponPalette\d+):", source(weapon_palettes), re.M)
    ):
        add("battle-weapon-palette", index, symbol, weapon_palettes)
    weapon_items = "data/stats/items/weapongraphics.asm"
    for index, (sprite, palette) in enumerate(
        re.findall(r"^\s+weaponGraphics\s+(\w+),\s*(\w+)", source(weapon_items), re.M), 26
    ):
        add(
            "battle-weapon-item",
            index,
            f"{sprite}:{palette}",
            weapon_items,
            "not-applicable" if sprite == "NONE" else "source-only",
        )
    pointers(
        "battle-ground",
        GRAPHICS + "battles/grounds/entries.asm",
        "pt_Grounds",
        counts["ground"]["pointerCount"],
    )
    pointers(
        "battle-background",
        GRAPHICS + "battles/backgrounds/entries.asm",
        "pt_Backgrounds",
        fixture("battle-background-decode")["summary"]["backgroundPointerCount"],
    )
    effects = fixture("battle-effect-graphics-decode")
    pointers(
        "battle-effect",
        GRAPHICS + "battles/spells/entries.asm",
        "pt_SpellGraphics",
        effects["summary"]["spellContainerCount"],
    )
    pointers(
        "battle-invocation",
        GRAPHICS + "battles/spells/invocations/entries.asm",
        "pt_InvocationSprites",
        effects["summary"]["invocationContainerCount"],
    )
    for key in ("tiles_StatusAnimation", "pt_BattlesceneTransitionTiles"):
        add("battle-effect-control", key, key, "battle-effect-graphics-decode-v1.json")

    ally_path = "data/stats/allies/allybattlesprites.asm"
    ally_slots = re.findall(
        r"^\s+allyBattleSprAndPlt\s+(\w+),\s*(\w+),\s*(\d+)", source(ally_path), re.M
    )
    for slot, (class_name, sprite_name, palette) in enumerate(ally_slots):
        add(
            "battle-ally-class",
            f"{slot // 3}:{class_name}:{slot % 3}",
            f"{sprite_name}:{palette}",
            ally_path,
            "not-applicable" if class_name == "NONE" else "source-only",
        )
    map_ally_path = "data/stats/allies/allymapsprites.asm"
    map_ally = re.findall(r"^\s+mapsprite\s+(\w+)", source(map_ally_path), re.M)
    for member, symbol in enumerate(map_ally):
        add("walk-ally-class-base", member, f"MAPSPRITE_{symbol}", map_ally_path)
    enemy_path = "data/stats/enemies/enemybattlesprites.asm"
    for enemy, (sprite_name, palette) in enumerate(
        re.findall(r"^\s+enemyBattleSprAndPlt\s+(\w+),\s*(\d+)", source(enemy_path), re.M)
    ):
        add("battle-enemy-species", enemy, f"{sprite_name}:{palette}", enemy_path)
    raster_ids = set(battle["rasters"])
    for member, (map_sprite, palette) in PLDN.items():
        if ("PLDN", "PLDN", str(palette)) not in ally_slots[member * 3 : member * 3 + 3]:
            raise ValueError(f"PLDN source binding drift for member {member}")
        visuals = [
            row
            for row in battle["actors"]
            if row["side"] == "ally"
            and row["sprite"] == 13
            and row["palette"] == palette
            and member in row.get("members", [])
        ]
        if any(not set(row["frames"] + row["weaponFrames"]) <= raster_ids for row in visuals):
            raise ValueError(f"unresolved Battle26 raster for member {member}")
        add(
            "priority-pldn-battle",
            member,
            f"PLDN:13:{palette}",
            ally_path,
            "present" if visuals else "missing",
            f"battle-26: {len(visuals)} item bindings",
        )
        if map_sprite is not None:
            status, key = coverage(installed_sprites, map_sprite, f"world.sprite[{map_sprite}]")
            add("priority-pldn-field", member, map_sprite, map_ally_path, status, key)

    icons = GRAPHICS + "icons/entries.asm"
    icon_symbols = re.findall(r"^(\w+Icon\d+):\s*incbin", source(icons), re.M)
    if len(icon_symbols) != fixture("icon-graphics-static")["summary"]["assembledIconCount"]:
        raise ValueError("icon storage count drift")
    field_icons = set(presentation.get("identityIcons", {}))
    for index, symbol in enumerate(icon_symbols):
        add("icon-storage", index, symbol, icons)
        if index in (127, 128, 129):
            continue  # Empty and other icons have no item/spell consumer identity.
        kind, identity = ("spell", index - 130) if index >= 130 else ("item", index)
        for state in ("normal", "selected"):
            key = f"identity-icon.{kind}.{identity}.{state}"
            for owner, available in (("field", field_icons), ("battle26", raster_ids)):
                status, selected = coverage(available, key, f"{owner}:{key}")
                add("icon-variant", selected, symbol, icons, status)
    for index in (7, 8, 9):
        add(
            "priority-display-name",
            index,
            "name.spell",
            "selected zh-CN catalog",
            "present" if ("name.spell", index) in names else "missing",
        )
    for key in fixture("ui-graphics-decode")["table"]:
        if key.startswith("tiles_"):
            add("ui-tiles", key, key, "ui-graphics-decode-v1.json")
    for menu in ("fieldMenu", "churchMenu", "itemMenu", "shopMenu"):
        for key in presentation.get(menu, {}):
            add("ui-mounted", f"{menu}.{key}", menu, "selected world", "present")
    for key in fixture("special-screen-graphics-decode")["table"]:
        add("special-screen", key, key, "special-screen-graphics-decode-v1.json")
    for key in ("palette_WitchChoice", "table_WitchBubbleAnimation"):
        add("witch-menu", key, key, "witch-menu-graphics-static-v1.json")
    entry_images = {file.stem for file in (package / "entry").glob("*.png")}
    for key in ("title", "numeral", "witch-arrival", "witch", "witch-blink", "witch-mouth"):
        status, selected = coverage(entry_images, key, f"entry/{key}.png")
        add("entry-mounted", key, key, "selected entry", status, selected)

    commands = {
        int(number): name
        for name, number in re.findall(r"^((?:MUSIC|SFX)_\w+):\s+equ\s+(\d+)\s*$", enums, re.M)
    }
    music = fixture("sound-data-static")["expected"]["commandModel"]["bankSelection"]
    silent = {
        command: row["targetSymbol"]
        for row in music["aliasTargets"]
        for command in row["commandIds"]
    }
    silent.update({32: "Music_32", 64: "Music_64"})
    for command in range(1, 121):
        family = "music" if command <= 64 else "sfx"
        if command in silent:
            add(
                family,
                command,
                silent[command],
                "sound-data-static-v1.json",
                "not-applicable",
                "silent target",
            )
            continue
        variants = sorted(
            {
                f"{owner}:timer-{row['timerB']:02x}"
                for owner, records in (("world/entry", world_audio), ("asset-git", library_audio))
                for row in records
                if row["command"] == command
            }
        )
        add(
            family,
            command,
            commands[command],
            "sf2enums.asm",
            "present" if variants else "missing",
            " ".join(variants),
        )
    return rows


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ("upstream", "package", "asset-root", "catalog", "output"):
        parser.add_argument(f"--{name}", required=True, type=Path)
    parser.add_argument("--asset-commit", required=True)
    args = parser.parse_args()
    output = private_output(args.output)
    rows = inventory(args.upstream, args.package, args.asset_root, args.asset_commit, args.catalog)
    output.parent.mkdir(parents=True, exist_ok=True)
    with output.open("x", newline="", encoding="utf-8") as stream:
        writer = csv.writer(stream)
        writer.writerow(
            ("family", "id", "source_symbol", "source_owner", "status", "prepared_selection")
        )
        writer.writerows(rows)
    print(json.dumps({"rows": len(rows), "status": Counter(row[4] for row in rows)}))


if __name__ == "__main__":
    main()
