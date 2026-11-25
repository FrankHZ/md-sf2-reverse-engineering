"""Mount prepared member-specific promotion bodies, weapon layers and animations."""

import base64
import hashlib
import json
from pathlib import Path

from sf2tool.remake_asset_build import _scale_rgba_nearest
from sf2tool.remake_battle_scene_content import compose, sprite_layout
from sf2tool.texture_extract import md_palette_color, write_png_rgba


def read_json(path: Path) -> dict:
    with path.open(encoding="utf-8-sig") as stream:
        return json.load(stream)


def installed_png(root, name):
    path = (root / name).resolve(strict=True)
    if not path.is_relative_to(root.resolve()):
        raise ValueError("field raster escapes the prepared family")
    data = path.read_bytes()
    if data[:8] != b"\x89PNG\r\n\x1a\n":
        raise ValueError("invalid prepared field PNG")
    return dict(
        width=int.from_bytes(data[16:20], "big"),
        height=int.from_bytes(data[20:24], "big"),
        format="png",
        data=base64.b64encode(data).decode(),
        sha256=hashlib.sha256(data).hexdigest().upper(),
    )


def mount_walking(directory: Path, world: dict, sprites: dict, selected) -> None:
    mounted = world["presentation"]["sprites"]
    for sprite in selected:
        row = dict(sprites[sprite])
        row["directions"] = [installed_png(directory, path) for path in row["directions"]]
        existing = next((entry for entry in mounted if entry["sprite"] == sprite), None)
        if existing is None:
            mounted.append(row)
        elif existing != row:
            raise ValueError(f"conflicting field sprite {sprite}")


def prepared_ally_sequence(index: int, rows: list[list[int]], draw_weapon: bool) -> dict:
    def signed(value):
        return value if value < 128 else value - 256

    def weapon(frame, layer, x, y):
        return dict(frame=frame, layer=layer, x=signed(x), y=signed(y)) if draw_weapon else None

    header, *frames = rows
    count, trigger, spell, terminate = header[:4]
    if count != len(frames):
        raise ValueError("prepared ally animation frame count differs")
    return dict(
        index=index,
        trigger=trigger,
        spell=spell,
        terminate=terminate,
        idleWeapon=weapon(*header[4:]),
        frames=[
            dict(frame=frame, ticks=ticks, x=signed(x), y=signed(y), weapon=weapon(*draw))
            for frame, ticks, x, y, *draw in frames
        ],
    )


def shared_color_weapon_frames(directory, index, scratch, scene, item, *, scale):
    sprite, palette = index["items"][str(item)]
    data = (directory / f"weapon-{sprite}.4bpp").read_bytes()
    if len(data) != 8192 or any(
        (byte >> 4 not in (0, 1, 2, 14, 15) or byte & 15 not in (0, 1, 2, 14, 15)) for byte in data
    ):
        raise ValueError(f"weapon {item} requires colors outside the shared body and weapon slots")
    colors = [md_palette_color(0)] * 16
    colors[1] = md_palette_color(0x0EEE)
    colors[14:16] = [md_palette_color(color) for color in index["palettes"][palette]]
    frames = []
    layout = sprite_layout(8, 8)
    for frame in range(4):
        key = f"weapon{item}/frame{frame}"
        leaf = f"weapon-{item}-{frame}.png"
        pixels = compose(data[frame * 2048 : (frame + 1) * 2048], colors, 64, 64, layout)
        if scale != 1:
            pixels = _scale_rgba_nearest(tuple(pixels), 64, 64, scale)
        write_png_rgba(scratch / leaf, 64 * scale, 64 * scale, pixels)
        raster = installed_png(scratch, leaf)
        if scene["rasters"].setdefault(key, raster) != raster:
            raise ValueError(f"conflicting weapon raster {key}")
        frames.append(key)
    return frames


def ninja_weapon_layers(scene: dict) -> list[dict]:
    # The selected sword banks use shared body palette colors 1 and 2.
    # Steel Sword shares Long Sword's weapon bank and color pair.
    hero_weapons = {
        row["item"]: row["weaponFrames"]
        for row in scene["actors"]
        if row["side"] == "ally" and row["sprite"] == 24 and 0 in row.get("members", [])
    }
    layers = [{"item": None, "weaponFrames": []}]
    for item in (57, 58, 59, 60, 62, 63, 64, 71):
        donor = 59 if item == 60 else item
        if not hero_weapons.get(donor):
            raise ValueError(f"base package lacks Ninja sword layer {donor}")
        layers.append({"item": item, "weaponFrames": hero_weapons[donor]})
    return layers


def pgnt_weapon_layers(library, scratch, scene, member):
    directory = library / "weapons"
    index = read_json(directory / "index.json")["records"]
    selectors = index["items"]
    # Eric's mounted knight palettes share the two body colors used by these weapon banks.
    layers = {
        tuple(selectors[str(row["item"])]): row["weaponFrames"]
        for row in scene["actors"]
        if row["side"] == "ally"
        and row["sprite"] in (2, 13)
        and member in row.get("members", [])
        and row["item"] is not None
    }
    rows = [{"item": None, "weaponFrames": []}]
    for item in (56, *range(72, 80)):
        selector = tuple(selectors[str(item)])
        if selector not in layers:
            layers[selector] = shared_color_weapon_frames(
                directory, index, scratch, scene, item, scale=2
            )
        attack = 82 if selector[0] in (12, 13) else 17
        rows.append({"item": item, "weaponFrames": layers[selector], "attackIndex": attack})
    return rows


def brn_weapon_layers(library, scratch, scene):
    directory = library / "weapons"
    index = read_json(directory / "index.json")["records"]
    selectors = index["items"]
    layers = {
        tuple(selectors[str(row["item"])]): row["weaponFrames"]
        for row in scene["actors"]
        if row["side"] == "ally" and row["sprite"] in (18, 22, 24) and row["item"] is not None
    }
    rows = [{"item": None, "weaponFrames": []}]
    # Use the axes and swords admitted by the current campaign's equipment catalog.
    for item in (*range(33, 39), *range(57, 61), 62, 63, 64, 71):
        selector = tuple(selectors[str(item)])
        if selector not in layers:
            layers[selector] = shared_color_weapon_frames(
                directory, index, scratch, scene, item, scale=2
            )
        rows.append({"item": item, "weaponFrames": layers[selector]})
    return rows


def mount_promotion_appearances(
    library: Path,
    scratch: Path,
    world: dict,
    scene: dict,
    promotions: dict[int, str],
    *,
    mount_field: bool = True,
) -> None:
    # The selected independent weapon layers use body colors 1 and 2 ($0EEE
    # and $0000) shared by both class palettes. None selects an unarmed body.
    rules = {
        "PGNT": (14, set()),
        "WIZ": (17, set()),
        "SORC": (18, set()),
        "MMNK": (20, set()),
        "VICR": (19, set()),
        "GLDT": (15, {33, 34, 35, 36}),
        "BRN": (16, set()),
        "BDBT": (23, {57, 58, 59, 71}),
        "SNIP": (21, {45, 46, 47}),
        "BRGN": (22, {45, 46, 47}),
        "BWNT": (25, {45, 46, 47}),
        "WFBR": (24, {None}),
        "PHNX": (26, {None}),
        "NINJ": (27, set()),
        "MNST": (28, {None}),
    }
    # Additional source-legal weapons already mounted for another promoted member.
    # Arrows and shells use only the selected body animation (weapon selector FF).
    promoted_weapons = {
        "BDBT": (0, {62}),
        "SNIP": (11, {48, 49, 50}),
        "BWNT": (11, {48, 49, 50}),
        "BRGN": (11, {48, 49, 50}),
    }
    walking = read_json(library / "walking/index.json")["records"]
    variants = {
        row["member"]: row
        for row in walking["classes"]
        if row["member"] in promotions and row["sourceClass"] == rules[promotions[row["member"]]][0]
    }
    if variants.keys() != promotions.keys():
        raise ValueError("prepared promotion appearances are incomplete")
    base_sprites = {
        row["member"]: row["battleSprite"] for row in walking["classes"] if row["slot"] == 0
    }
    for member, variant in variants.items():
        if promotions[member] == "MNST":
            # GetAllyPortrait remaps MNST independently of the field sprite's default.
            sprite = variant["sprite"]
            walking["sprites"][sprite] = dict(walking["sprites"][sprite], portrait=51)
    if mount_field:
        mount_walking(
            library / "walking",
            world,
            walking["sprites"],
            [row["sprite"] for row in variants.values()],
        )

    portraits = {
        walking["sprites"][variant["sprite"]]["portrait"]
        for member, variant in variants.items()
        if promotions[member] in {"WFBR", "PHNX", "NINJ", "MNST"}
    }
    if portraits:
        directory = library / "portraits"
        mounted = world["presentation"]["portraits"]
        prepared = {row["portrait"]: row for row in read_json(directory / "index.json")["records"]}
        for portrait in sorted(portraits):
            row = prepared[portrait]
            existing = next((entry for entry in mounted if entry["portrait"] == portrait), None)
            if existing is None:
                mounted.append(dict(row, raster=installed_png(directory, row["raster"])))

    directory = library / "bodies"
    bodies = {
        (row["sprite"], row["palette"]): row
        for row in read_json(directory / "index.json")["records"]
        if row["side"] == "ally"
    }
    animations = read_json(directory / "ally-animations.json")
    for member, variant in variants.items():
        sprite, palette = variant["battleSprite"], variant["battlePalette"]
        body = bodies[sprite, palette]
        frames = []
        for path in body["frames"]:
            key = Path(path).with_suffix("").as_posix()
            raster = installed_png(directory, path)
            if scene["rasters"].setdefault(key, raster) != raster:
                raise ValueError(f"conflicting prepared promoted body {key}")
            frames.append(key)

        class_code = promotions[member]
        if class_code == "PGNT":
            mounted_weapons = pgnt_weapon_layers(library, scratch, scene, member)
        elif class_code == "BRN":
            mounted_weapons = brn_weapon_layers(library, scratch, scene)
        elif class_code == "MMNK":
            mounted_weapons = [dict(item=item, weaponFrames=[]) for item in [None, 26, 27, 28]]
        elif class_code == "NINJ":
            mounted_weapons = ninja_weapon_layers(scene)
        elif class_code in {"WIZ", "SORC", "VICR"}:
            weapon_directory = library / "weapons"
            index = read_json(weapon_directory / "index.json")["records"]
            # Current shops admit Iron Rod, Flail and Indra as well as the
            # starting rods. Use the same prepared layers for all three classes.
            # Put the explicit bare body first: the reader retains the first
            # member/class row as its unarmed fallback after a weapon breaks.
            mounted_weapons = [dict(item=None, weaponFrames=[])]
            for item in (85, 86, 87, 88, 89, 90, 92):
                weapon_frames = next(
                    (
                        row["weaponFrames"]
                        for row in scene["actors"]
                        if row["side"] == "ally" and row["item"] == item
                    ),
                    None,
                )
                if weapon_frames is None:
                    weapon_frames = shared_color_weapon_frames(
                        weapon_directory, index, scratch, scene, item, scale=1
                    )
                mounted_weapons.append(dict(item=item, weaponFrames=weapon_frames))
            scene["actors"] = [
                row
                for row in scene["actors"]
                if not (
                    row["side"] == "ally"
                    and row["sprite"] == sprite
                    and row["palette"] == palette
                    and row.get("members") == [member]
                )
            ]
        else:
            base_items = rules[class_code][1]
            mounted_weapons = [
                row
                for row in scene["actors"]
                if row["side"] == "ally"
                and row["sprite"] == base_sprites[member]
                and member in row.get("members", [])
                and (row["item"] is None) == (None in base_items)
            ]
            if {row["item"] for row in mounted_weapons} != base_items:
                raise ValueError(f"base package lacks the supported {class_code} weapon layers")
            if class_code in promoted_weapons:
                donor, item_ids = promoted_weapons[class_code]
                extra_weapons = [
                    row
                    for row in scene["actors"]
                    if row["side"] == "ally"
                    and donor in row.get("members", [])
                    and row["item"] in item_ids
                ]
                if {row["item"] for row in extra_weapons} != item_ids:
                    raise ValueError(
                        f"base package lacks the promoted-only {class_code} weapon bindings"
                    )
                mounted_weapons.extend(extra_weapons)
        for row in mounted_weapons:
            # Use Item selects the ordinary animation. Dodge selects the second bank.
            draw_weapon = bool(row["weaponFrames"])
            attack = prepared_ally_sequence(sprite, animations[sprite], draw_weapon)
            dodge = prepared_ally_sequence(sprite + 40, animations[sprite + 40], draw_weapon)
            sequences = dict(idle=attack, attack=attack, dodge=dodge, item=attack)
            if (attack_index := row.get("attackIndex", sprite)) != sprite:
                sequences["attack"] = prepared_ally_sequence(
                    attack_index, animations[attack_index], draw_weapon
                )
            if class_code in {"WIZ", "SORC", "VICR", "NINJ", "MMNK"}:
                sequences["cast"] = attack
            if class_code == "MMNK":
                sequences["special"] = prepared_ally_sequence(83, animations[83], False)
            if class_code == "MNST":
                sequences["cast"] = prepared_ally_sequence(84, animations[84], False)
            appearance = dict(
                side="ally",
                sprite=sprite,
                palette=palette,
                members=[member],
                item=row["item"],
                frames=frames,
                weaponFrames=row["weaponFrames"],
                sequences=sequences,
                idleTicks=body["idleTicks"],
            )
            if appearance not in scene["actors"]:
                scene["actors"].append(appearance)
