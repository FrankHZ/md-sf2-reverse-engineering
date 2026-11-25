"""Mount the finite source-legal equipment appearances from the prepared retail library."""

from pathlib import Path

from sf2tool.remake_exploration_content import _selected_growth
from sf2tool.remake_exploration_programs import OriginalPrograms
from sf2tool.remake_exploration_scene import ally_sprites, promotion_sprite_fields, promotion_sprites
from sf2tool.remake_promotion_appearances import (
    installed_png,
    mount_walking,
    prepared_ally_sequence,
    read_json,
    shared_color_weapon_frames,
)


def equipment_variants(library, source):
    """Join original starts and promotion destinations; never infer legality from artwork."""
    classes = {row["id"]: row["code"] for row in source["classes"]}
    regular = dict(enumerate((12, 13, 15, 17, 19, 21, 23, 24, 25, 26, 27, 28)))
    special = {1: 14, 2: 16, 3: 18, 4: 20, 5: 22}
    starts = {row["id"]: row["startClass"] for row in source["allies"] if row["id"] < 30}
    expected = set()
    for member, code in starts.items():
        role = next(key for key, value in classes.items() if value == code)
        expected.add((member, role))
        for destination in (regular.get(role), special.get(role)):
            if destination is not None:
                expected.add((member, destination))
    walking = read_json(library / "walking/index.json")["records"]
    variants = [row for row in walking["classes"] if (row["member"], row["sourceClass"]) in expected]
    if {(row["member"], row["sourceClass"]) for row in variants} != expected:
        raise ValueError("prepared bodies do not cover legal member/class pairs")
    return variants


def legal_weapons(source, role):
    code = next(row["code"] for row in source["classes"] if row["id"] == role)
    return [row["id"] for row in source["items"] if "WEAPON" in row["itemTypeTokens"]
            and (row["equipFlagsExpression"] == "ALL" or code in row["equipFlagTokens"])]


def mount_equipment_appearances(library, scratch, scene, source, *, selected=None):
    """One body per member/class and explicit bare/weapon keys, with source-selected sequences."""
    variants = equipment_variants(library, source)
    directory = library / "bodies"
    bodies = {(row["sprite"], row["palette"]): row
              for row in read_json(directory / "index.json")["records"] if row["side"] == "ally"}
    animations = read_json(directory / "ally-animations.json")
    weapons = read_json(library / "weapons/index.json")["records"]
    previous = list(scene["actors"])
    additions = []
    for variant in variants:
        member, role = variant["member"], variant["sourceClass"]
        sprite, palette = variant["battleSprite"], variant["battlePalette"]
        items = [None, *legal_weapons(source, role)]
        if selected is not None:
            items = [item for item in items if (member, role, item) in selected]
        if not items:
            continue
        body = bodies.get((sprite, palette))
        if body is None:
            # PLDN is the existing separately prepared family, deliberately omitted by bodies().
            donor = next(row for row in previous if row["side"] == "ally"
                         and row["sprite"] == sprite and row["palette"] == palette)
            frames, idle_ticks = donor["frames"], donor.get("idleTicks", 0)
        else:
            frames = []
            for path in body["frames"]:
                key = "equipment/" + Path(path).with_suffix("").as_posix()
                scene["rasters"][key] = installed_png(directory, path)
                frames.append(key)
            idle_ticks = body["idleTicks"]
        # Preserve accepted HD selections for an existing body while publishing its original frames.
        donor = next((row for row in previous if row["side"] == "ally" and row["sprite"] == sprite
                      and row["palette"] == palette and len(row["frames"]) == len(frames)), None)
        if donor is not None and "hdRasters" in scene:
            for old, new in zip(donor["frames"], frames, strict=True):
                if old in scene["hdRasters"]:
                    scene["hdRasters"][new] = scene["hdRasters"][old]
        for item in items:
            selector = weapons["items"][str(item)][0] if item is not None else 255
            weapon_frames = []
            if selector != 255:
                layer = {"rasters": {}}
                raw_frames = shared_color_weapon_frames(library / "weapons", weapons, scratch, layer, item, scale=1)
                weapon_frames = ["equipment/" + key for key in raw_frames]
                scene["rasters"].update(("equipment/" + key, value) for key, value in layer["rasters"].items())
            draw = bool(weapon_frames)
            ordinary = prepared_ally_sequence(sprite, animations[sprite], draw)
            attack = {2: 80, 13: 81, 17: 82}.get(sprite, sprite) if selector in (12, 13) else sprite
            sequences = dict(idle=ordinary, item=ordinary,
                             attack=prepared_ally_sequence(attack, animations[attack], draw),
                             dodge=prepared_ally_sequence(sprite + 40, animations[sprite + 40], draw))
            if role in (3, 4, 12, 17, 18, 19, 20, 27):
                sequences["cast"] = ordinary
            if role == 20:
                sequences["special"] = prepared_ally_sequence(83, animations[83], False)
            if role == 28:
                sequences["cast"] = prepared_ally_sequence(84, animations[84], False)
            additions.append(dict(side="ally", sprite=sprite, palette=palette, classId=role,
                                  members=[member], item=item, frames=frames, weaponFrames=weapon_frames,
                                  sequences=sequences, idleTicks=idle_ticks))
    if selected is None:
        scene["actors"] = [row for row in previous if row["side"] != "ally"] + additions
        if "fieldDeath" in scene:
            scene["fieldDeath"]["allies"] = [dict(character=row["member"], sprite=row["sprite"])
                                              for row in variants if row["slot"] == 0]
    else:
        scene["actors"].extend(additions)
    return additions


def bind_equipment_members(library, package, source, upstream):
    """Admit source records and field bodies without adding joins, routes or acquisition."""
    variants = equipment_variants(library, source)
    codes = {row["id"]: row["code"] for row in source["classes"]}
    starts = {row["id"]: row["startClass"] for row in source["allies"] if row["id"] < 30}
    selections = [dict(member=row["member"], **{"class": codes[row["sourceClass"]]}) for row in variants]
    promotions = [row for row in selections if starts[row["member"]] != row["class"]]
    compiler = OriginalPrograms({"resources": {"standaloneScriptPrograms": [], "initSourcePrograms": []}},
                                upstream, promotion_classes=tuple(promotions))
    compiler.sources = {"disasm/sf2enums.asm"}
    world = package["world"]
    current = {(row["actor"], row["classId"]): row for row in world["growth"]}
    for row in _selected_growth(compiler, selections):
        key = row["actor"], row["classId"]
        if key in current and current[key] != row:
            raise ValueError(f"existing growth differs: {key}")
        if key not in current:
            world["growth"].append(row)
    appearances = {row["character"]: row for row in ally_sprites(compiler)}
    walking = read_json(library / "walking/index.json")["records"]
    existing_sprites = {row["sprite"] for row in world["presentation"]["sprites"]}
    mount_walking(library / "walking", world, walking["sprites"],
                  sorted({row["sprite"] for row in variants} - existing_sprites))
    portraits = world["presentation"]["portraits"]
    wanted = {walking["sprites"][row["sprite"]]["portrait"] for row in variants} - {None}
    for row in read_json(library / "portraits/index.json")["records"]:
        if row["portrait"] in wanted and not any(old["portrait"] == row["portrait"] for old in portraits):
            portraits.append(dict(row, raster=installed_png(library / "portraits", row["raster"])))

    def population(value):
        if value is None:
            return
        for old in value["allySprites"]:
            variant = appearances[old["character"]]
            for key in ("promotionClass", "promotionSprite", "promotionSprites"):
                old.pop(key, None)
            old.update(promotion_sprite_fields(promotion_sprites(variant)))

    for row in world["maps"]:
        population(row.get("population"))
        for setup in row.get("setups") or ():
            population(setup.get("population"))
    for program in world["programs"]:
        for instruction in program["instructions"]:
            population(instruction.get("population"))
    return compiler.sources
