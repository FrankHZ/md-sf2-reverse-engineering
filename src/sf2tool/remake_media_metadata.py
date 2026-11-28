"""Project the current private media selections to a public, payload-free inventory."""

from __future__ import annotations

import argparse
import csv
import io
import os
import re
import tempfile
from collections import defaultdict
from pathlib import Path

from sf2tool.remake_media_selection import (
    DEFAULT_SELECTION,
    METADATA,
    package_arguments,
    read_selection,
    shared_root,
)
from sf2tool.remake_private_package import read_json
from sf2tool.remake_windows_package import inputs

TOKEN = re.compile(r"[A-Za-z0-9][A-Za-z0-9_./-]{0,119}\Z")


def token(value: str | int) -> str:
    """Pass only bounded identifiers and portable relative selectors to the public table."""
    text = str(value)
    if (
        not TOKEN.fullmatch(text)
        or any(part in {"", ".", ".."} for part in text.split("/"))
        or re.search(r"[0-9a-f]{32}", text, flags=re.IGNORECASE)
    ):
        raise ValueError("unexpected public media identifier or relative selector")
    return text


def variants(hd: bool) -> str:
    return "original+hd" if hd else "original"


def world_rows(world: dict, relative: str, consumer: str):
    presentation = world["presentation"]
    document = f"{relative}#world.presentation"
    for row in presentation["maps"]:
        map_id = token(row["map"])
        hd = "hdBlocks" in row or "hdTerrainFile" in row
        selection = f"{document}.maps[{map_id}]"
        if "hdTerrainFile" in row:
            parent = relative.rsplit("/", 1)[0]
            selection += f" + {parent}/{token(row['hdTerrainFile'])}"
        yield "map-terrain", map_id, variants(hd), consumer, selection
        if row.get("tileAnimation"):
            yield (
                "terrain-animation",
                map_id,
                "original+hd",
                consumer,
                (f"{document}.maps[{map_id}].tileAnimation"),
            )
        if row.get("scenePalette"):
            yield (
                "terrain-palette",
                map_id,
                variants(bool(row["scenePalette"].get("hdBanks"))),
                consumer,
                f"{document}.maps[{map_id}].scenePalette",
            )

    for row in presentation["sprites"]:
        sprite_id = token(row["sprite"])
        locator = f"{document}.sprites[{sprite_id}]"
        yield "sprite", sprite_id, variants("hdDirections" in row), consumer, locator
        for key in row.get("graphics", {}):
            graphic_id = token(key)
            yield (
                "sprite-variant",
                f"{sprite_id}/{graphic_id}",
                variants(key in row.get("hdGraphics", {})),
                consumer,
                f"{locator}.graphics[{graphic_id}]",
            )

    for row in presentation["portraits"]:
        portrait_id = token(row["portrait"])
        yield (
            "portrait",
            portrait_id,
            variants("hdRaster" in row),
            consumer,
            (f"{document}.portraits[{portrait_id}]"),
        )

    menus = ("identityIcons", "fieldMenu", "churchMenu", "itemMenu", "shopMenu")
    for menu in menus:
        hd = presentation.get("hd" + menu[0].upper() + menu[1:], {})
        for key in presentation.get(menu, {}):
            identity = token(key)
            yield (
                "menu-image",
                identity if menu == "identityIcons" else f"{menu}/{identity}",
                variants(key in hd),
                consumer,
                (f"{document}.{menu}[{identity}]"),
            )


def audio_rows(presentation: dict, relative: str, consumer: str, field: str):
    for row in presentation.get("audio", []):
        cue = token(row["cue"])
        command, timer = row["command"], row["timerB"]
        if not isinstance(command, int) or not 1 <= command <= 120:
            raise ValueError("invalid media command")
        if not isinstance(timer, int) or not 0 <= timer <= 255:
            raise ValueError("invalid media timer")
        selection = f"{relative}#{field}.audio[{cue}]"
        if row.get("runtimeWav"):
            parent = relative.rsplit("/", 1)[0]
            selection += (
                f" + {parent}/audio-{command:02x}-{timer:02x}/"
                f"command-{command:02x}-timer-{timer:02x}.wav"
            )
        yield (
            "music" if command < 65 else "sfx",
            f"{cue}/{command}/{timer:02x}",
            "mode-independent",
            consumer,
            selection,
        )


def scene_rows(campaign: Path, relative: str, names: list[str]):
    bindings = defaultdict(list)
    for name in names:
        scene = read_json(campaign / token(name))
        encounter = token(scene["encounter"])
        yield "battle-scene", encounter, "index", encounter, f"{relative}/{name}"
        originals = scene["rasters"]
        hd = scene.get("hdRasters", {})
        if hd.keys() - originals.keys():
            raise ValueError("HD battle raster lacks an original selector")
        groups = (
            [f"kiwi-flame/frame-{i}" for i in range(6)],
            *([f"aqua-{rank}/frame-{i}" for i in range(7)] for rank in (1, 2)),
        )
        expected = {key for group in groups for key in group}
        fallback = {key for group in groups if not hd.keys() >= set(group) for key in group}
        for key in originals.keys() | expected:
            state = variants(key in hd)
            if key not in originals:
                state = "missing"
            elif key in hd and key in fallback:
                state = "original+hd-group-fallback"
            bindings[token(key), state].append(encounter)

    for (key, variant), encounters in bindings.items():
        consumer = " ".join(sorted(encounters, key=lambda name: int(name.removeprefix("battle-"))))
        yield "battle-raster", key, variant, consumer, f"{relative}"


def inventory(root: Path, roles: dict, *, staged_entry=None):
    files = package_arguments(roles, root)
    if staged_entry is not None:
        files["entry"] = staged_entry / "entry.json"
    inputs(**files)
    campaign, entry_doc, opening_doc = files["campaign"], files["entry"], files["opening"]
    relative = roles["gameplay"]
    opening = opening_doc.parent
    opening_root = opening.relative_to(root).as_posix()
    entry_root = roles["entry"]

    campaign_world = read_json(campaign / "world.json")
    yield from world_rows(campaign_world["world"], f"{relative}/world.json", "gameplay")
    yield from audio_rows(
        campaign_world["world"]["presentation"],
        f"{relative}/world.json",
        "gameplay",
        "world.presentation",
    )
    scenes = [
        "main-scenes.json",
        *(row["battleScenes"] for row in campaign_world["encounters"] if row["battleScenes"]),
    ]
    del campaign_world
    yield from scene_rows(campaign, relative, scenes)

    relative_world = token(read_json(opening_doc)["world"])
    opening_world = read_json(opening / relative_world)["world"]
    opening_relative = f"{opening_root}/{relative_world}"
    yield from world_rows(opening_world, opening_relative, "opening")
    yield from audio_rows(
        opening_world["presentation"], opening_relative, "opening", "world.presentation"
    )

    entry_value = read_json(entry_doc)
    yield from entry_images(entry_value, entry_root)
    yield from audio_rows(
        entry_value["presentation"], f"{entry_root}/entry.json", "title-witch", "presentation"
    )


# EntryPresentationReader.ReadCore requires these base names and admits optional HD siblings.
ENTRY_NAMES = ("title", "numeral", "witch-arrival", "witch", "witch-blink", "witch-mouth")
ENTRY_MUSIC = ("MUSIC_TITLE/28/c2", "MUSIC_WITCH/10/c1")
COLUMNS = ("category", "semantic_id", "variant", "consumer", "shared_selection", "recipe")
RECIPES = {
    "entry-image": "presentation-assets.md#switchable-entry-layers",
    "music": "music-command-inventory.md#non-silent-commands",
    "sfx": "presentation-assets.md#prepare-and-mount-retail-resources",
    "map-terrain": "asset-inventory.md#pre-voyage-static-terrain-2175",
    "terrain-animation": "presentation-assets.md#animated-map-tiles",
    "terrain-palette": "hd-rollout.md#opening-terrain-and-palette-banks-2324",
    "sprite": "asset-inventory.md#pre-voyage-actors-and-portraits-2176",
    "sprite-variant": "asset-inventory.md#pre-voyage-actors-and-portraits-2176",
    "portrait": "asset-inventory.md#pre-voyage-actors-and-portraits-2176",
    "menu-image": "presentation-assets.md#ordinary-menu-publication-and-coverage-1666",
    "battle-scene": "hd-rollout.md#private-battle-artwork-acceptance",
    "battle-raster": "hd-rollout.md#private-battle-artwork-acceptance",
}


def entry_images(document, root):
    def names(field):
        rows = document.get(field, [])
        indexed = {token(row["name"]): token(row["file"]) for row in rows}
        if len(rows) != len(indexed) or indexed.keys() - set(ENTRY_NAMES):
            raise ValueError("duplicate or unknown entry image binding")
        return indexed

    originals, hd = names("resources"), names("hdResources")
    if hd.keys() - originals.keys():
        raise ValueError("HD entry image lacks its Original")
    for name in ENTRY_NAMES:
        if name not in originals:
            yield "entry-image", name, "missing", "title-witch", f"{root}/entry.json#resources"
            continue
        location = f"{root}/{originals[name]}"
        if name in hd:
            location += f" + {root}/{hd[name]}"
        yield "entry-image", name, variants(name in hd), "title-witch", location


def recipe_for(kind, identity):
    if kind == "battle-raster":
        if identity.startswith("kiwi-flame/"):
            return "presentation-assets.md#kiwi-successor-2316"
        if identity.startswith(("aqua-1/", "aqua-2/")):
            return "presentation-assets.md#aqua-successor-2315"
    return RECIPES[kind]


def prepared_music():
    owner = (METADATA.parent / "music-command-inventory.md").read_text(encoding="utf-8")
    table = owner.split("## Non-silent commands\n", 1)[1].split("\n## ", 1)[0]
    records = set()
    for line in table.splitlines():
        fields = [cell.strip() for cell in line.split("|")]
        if len(fields) < 6 or not fields[1].isdigit():
            continue
        cue = re.match(r"`(MUSIC_\w+)`", fields[2])
        timers = set(re.findall(r"(?:finite|loop) ([A-F0-9]{2})\b", fields[4]))
        if not cue or len(timers) != 1:
            raise ValueError("non-silent command needs one reviewed preparation timer")
        identity = f"{cue[1]}/{fields[1]}/{timers.pop().lower()}"
        if identity in records:
            raise ValueError("duplicate prepared music identity")
        records.add(identity)
    if not records:
        raise ValueError("missing prepared music table")
    return records


def complete_rows(rows, roles):
    rows = list(rows)
    audio = {identity for kind, identity, _, _, _ in rows if kind == "music"}
    entry_audio = {row[1] for row in rows if row[0] == "music" and row[3] == "title-witch"}
    for identity in set(ENTRY_MUSIC) - entry_audio:
        rows.append(("music", identity, "missing", "title-witch", roles["entry"] + "/entry.json"))
    # The source owner records the prepared material regardless of its past runtime selection.
    for identity in prepared_music():
        if identity not in audio:
            rows.append(("music", identity, "prepared-only", "unbound", "-"))
    return sorted((*row, recipe_for(*row[:2])) for row in rows)


def validate(rows, roles):
    seen = set()
    if not rows or rows != sorted(rows):
        raise ValueError("media metadata must contain sorted rows")
    for kind, identity, variant, contexts, location, recipe in rows:
        token(identity)
        allowed = {"original", "original+hd", "missing"}
        if kind in ("music", "sfx"):
            allowed = {"mode-independent", "missing", "prepared-only"}
        elif kind == "battle-scene":
            allowed = {"index"}
        elif kind == "battle-raster":
            allowed.add("original+hd-group-fallback")
        if recipe != recipe_for(kind, identity) or variant not in allowed:
            raise ValueError("invalid media recipe or variant")
        for context in contexts.split(" "):
            key = kind, identity, token(context)
            if key in seen:
                raise ValueError(f"conflicting resource binding {key}")
            seen.add(key)
        if variant == "prepared-only":
            if (contexts, location) != ("unbound", "-"):
                raise ValueError("unmounted material cannot select runtime content")
            continue
        for item in location.split(" + "):
            path, _, member = item.partition("#")
            token(path)
            if not any(path == root or path.startswith(root + "/") for root in roles.values()):
                raise ValueError("binding outside selected roles")
            if member and not re.fullmatch(r"[A-Za-z0-9_.\[\]/-]+", member):
                raise ValueError("invalid resource selector")

    required = {("entry-image", name, "title-witch") for name in ENTRY_NAMES}
    required.update(("music", identity, "title-witch") for identity in ENTRY_MUSIC)
    if required - seen:
        raise ValueError("required entry bindings must be present or explicitly missing")


def project(root, roles, *, staged_entry=None):
    rows = complete_rows(inventory(root, roles, staged_entry=staged_entry), roles)
    validate(rows, roles)
    stream = io.StringIO()
    writer = csv.writer(stream, delimiter="\t", lineterminator="\n")
    writer.writerow(COLUMNS)
    writer.writerows(rows)
    return stream.getvalue().encode("utf-8")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--selection", type=Path, default=DEFAULT_SELECTION)
    parser.add_argument("--shared-root", type=Path)
    parser.add_argument("--table", type=Path, default=METADATA / "current-media.tsv")
    parser.add_argument(
        "--public-check", action="store_true", help="validate a tracked table offline"
    )
    parser.add_argument("--write", action="store_true", help="write the complete new projection")
    args = parser.parse_args()
    roles = read_selection(args.selection)
    if args.public_check:
        if args.write:
            parser.error("--public-check is read-only")
        with args.table.open(encoding="utf-8", newline="") as source:
            reader = csv.reader(source, delimiter="\t")
            if tuple(next(reader)) != COLUMNS:
                raise ValueError("unknown metadata columns")
            validate([tuple(row) for row in reader], roles)
    else:
        data = project(shared_root(args.shared_root), roles)
        if args.write:
            with tempfile.NamedTemporaryFile(dir=args.table.parent, delete=False) as out:
                out.write(data)
            os.replace(out.name, args.table)
        elif data != args.table.read_bytes():
            raise ValueError("tracked metadata differs from selected runtime inputs")


if __name__ == "__main__":
    main()
