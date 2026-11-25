"""Mount prepared member-specific promotion body and animation bindings."""

from pathlib import Path


def mount_promotion_appearances(
    library: Path, scratch: Path, world: dict, scene: dict, promotions: dict[int, str]
) -> None:
    from sf2tool.remake_private_package import (
        brn_weapon_layers,
        installed_png,
        mount_walking,
        ninja_weapon_layers,
        pgnt_weapon_layers,
        prepared_ally_sequence,
        read_json,
    )

    # The selected independent weapon layers use body colors 1 and 2 ($0EEE
    # and $0000) shared by both class palettes. None selects an unarmed body.
    rules = {
        "PGNT": (14, set()),
        "WIZ": (17, {85, 86, 87, 89}),
        "MMNK": (20, set()),
        "VICR": (19, {85, 86, 87, 89}),
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
    mount_walking(
        library / "walking", world, walking["sprites"], [row["sprite"] for row in variants.values()]
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
            if class_code in {"WIZ", "VICR", "NINJ", "MMNK"}:
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
