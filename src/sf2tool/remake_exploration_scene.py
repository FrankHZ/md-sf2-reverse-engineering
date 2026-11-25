"""Lower source cutscene layout, actor-position and sound commands."""

import re
from typing import Any

from sf2tool.h2.battle_global_data import _arguments, _tokens
from sf2tool.remake_exploration_actions import action_stream, walking_actions


def promotion_sprite_fields(bindings: dict[int, int]) -> dict:
    if len(bindings) == 1:
        role, sprite = next(iter(bindings.items()))
        return {"promotionClass": role, "promotionSprite": sprite}
    if bindings:
        return {
            "promotionSprites": [
                {"classId": role, "sprite": sprite} for role, sprite in sorted(bindings.items())
            ]
        }
    return {}


def promotion_sprites(appearance: dict) -> dict[int, int]:
    if "promotionClass" in appearance:
        return {appearance["promotionClass"]: appearance["promotionSprite"]}
    return {row["classId"]: row["sprite"] for row in appearance.get("promotionSprites", ())}


def ally_sprites(compiler) -> list[dict[str, Any]]:
    classes_path = "disasm/data/stats/allies/allystartdefs.asm"
    sprites_path = "disasm/data/stats/allies/allymapsprites.asm"
    compiler.sources.update(
        (classes_path, sprites_path, "disasm/code/common/scripting/entity/getallymapsprite.asm")
    )
    classes = _arguments(
        (compiler.upstream / classes_path).read_text(encoding="utf-8"), "startClass"
    )
    sprites = _arguments(
        (compiler.upstream / sprites_path).read_text(encoding="utf-8"), "mapsprite"
    )
    promoted: dict[int, list[str]] = {}
    for row in compiler.promotion_classes:
        promoted.setdefault(row["member"], []).append(row["class"])
    ordinary = {
        "SDMN": "HERO",
        "KNTE": "PLDN",
        "WARR": "GLDT",
        "MAGE": "WIZ",
        "PRST": "VICR",
        "ACHR": "SNIP",
        "BDMN": "BDBT",
        "WFMN": "WFBR",
        "RNGR": "BWNT",
        "PHNK": "PHNX",
        "THIF": "NINJ",
        "TORT": "MNST",
    }

    special = {"KNTE": "PGNT", "WARR": "BRN", "MAGE": "SORC", "PRST": "MMNK", "ACHR": "BRGN"}

    def class_sprite(table_sprite, role):
        # GetAllyMapsprite's offsets are relative to the member's table entry.
        if role == compiler.equates["CLASS_HERO"] or role >= compiler.equates["CLASS_BDBT"]:
            return table_sprite
        if role == compiler.equates["CLASS_SDMN"]:
            return table_sprite - 1
        if role <= compiler.equates["CLASS_ACHR"]:
            return table_sprite - 2
        if role <= compiler.equates["CLASS_TORT"] or role & 1:
            return table_sprite - 1
        return table_sprite

    result = []
    for character in range(compiler.equates["COMBATANT_ALLIES_NUMBER"]):
        role = compiler.number("CLASS_" + classes[character])
        table_sprite = compiler.number("MAPSPRITE_" + sprites[character])
        sprite = class_sprite(table_sprite, role)
        bindings = {}
        for destination in promoted.get(character, ()):
            base = classes[character]
            if destination not in (ordinary.get(base), special.get(base)):
                raise ValueError("unsupported promotion mapsprite")
            class_id = compiler.number("CLASS_" + destination)
            if class_id in bindings:
                raise ValueError("duplicate promotion class")
            bindings[class_id] = class_sprite(table_sprite, class_id)
        variant = promotion_sprite_fields(bindings)
        rohde = character == compiler.equates["ALLY_ROHDE"]
        result.append(
            {
                "character": character,
                "sprite": sprite,
                "joinedFlag": 11 if rohde else None,
                **variant,
                "unjoinedSprite": compiler.equates["MAPSPRITE_NPC_ROHDE"] if rohde else None,
            }
        )
    return result


def raft_placement(compiler, args, tail):
    if [row["opcode"] for row in tail] != ["move.b", "move.b"]:
        raise ValueError("incomplete raft placement")
    x, y = [_tokens(row["operandText"]) for row in tail]
    if (x[1], y[1]) != ("((RAFT_X-$1000000)).w", "((RAFT_Y-$1000000)).w") or not all(
        operand.startswith("#") for operand in (args[0], x[0], y[0])
    ):
        raise ValueError("unbound raft placement operands")
    return {
        "op": "park-raft",
        "map": f"map-{compiler.number(args[0][1:])}",
        "position": {"x": compiler.number(x[0][1:]), "y": compiler.number(y[0][1:])},
    }


def scene_operation(compiler, op, args):
    if op == "setSize":
        return [
            {
                "op": "resize-entity",
                "entity": compiler.entity(args[0]),
                "size": compiler.number(args[1]),
            }
        ]
    if op == "setQuake":
        word = compiler.number(args[0])
        amount = word & 0x3FFF
        if amount > 255 or (word & 0xC000 and amount == 0):
            raise ValueError("unbound quake amplitude")
        if not word & 0xC000:
            return [{"op": "quake", "amount": amount}]
        ramp = range(1, amount + 1) if word & 0x8000 else reversed(range(amount))
        return [
            row
            for step in ramp
            for row in ({"op": "quake", "amount": step}, {"op": "wait-ticks", "ticks": 40})
        ]
    if op == "cloneEntity":
        return [
            {
                "op": "copy-sprite-slot",
                "source": compiler.entity(args[0]),
                "target": compiler.entity(args[1]),
            }
        ]
    if op in ("clr.b", "move.b"):
        if op == "clr.b" and len(args) == 1:
            return [{"op": "travel-mode", "mode": "walking"}]
        if op == "move.b" and args[0] == "#PLAYERTYPE_RAFT":
            return [{"op": "travel-mode", "mode": "raft"}]
        raise ValueError("unbound field travel mode write")
    if op == "setSprite":
        sprite = compiler.number(args[1])
        return [
            {
                "op": "ally-sprite" if sprite < 30 else "sprite",
                "entity": compiler.entity(args[0]),
                "sprite": sprite,
            }
        ]
    if op == "setDest":
        # csc29 preserves the action script, speeds and flags, writes signed 32
        # velocities directly, and waits unless the character word has bit 15.
        character = (
            compiler.number(args[0][1:-1].split("+")[0]) | 0x8000
            if args[0].endswith("+$8000)")
            else compiler.number(args[0])
        )
        return [
            {
                "op": "entity-destination",
                "entity": compiler.entity(str(character & 0x7FFF)),
                "position": {"x": compiler.number(args[1]), "y": compiler.number(args[2])},
                "wait": not bool(character & 0x8000),
            }
        ]
    if op == "setBlocks":
        sx, sy, width, height, dx, dy = map(compiler.number, args)
        return [
            {
                "op": "copy-layout",
                "copy": {
                    "source": {"x": sx, "y": sy},
                    "destination": {"x": dx, "y": dy},
                    "width": width,
                    "height": height,
                },
            }
        ]

    if op in ("roofEvent", "stepEvent"):
        return [
            {
                "op": "roof-at" if op == "roofEvent" else "door-at",
                "position": {"x": compiler.number(args[0]), "y": compiler.number(args[1])},
            }
        ]
    entity = compiler.entity(args[0])
    position = {
        "op": "position",
        "entity": entity,
        "position": {"x": compiler.number(args[1]), "y": compiler.number(args[2])},
        "facing": compiler.number(args[3]),
    }
    # csc17 flashes at the old position, then branches to csc19.
    return (
        [{"op": "flash-entity-position", "entity": entity}, position]
        if op == "setPosFlash"
        else [position]
    )


def scene_palette_operation(compiler, op, args):
    fades = {
        "fadeInFromBlackHalf": (15, 14),
        "fadeOutToBlackHalf": (15, 15),
        "tintMap": (1, 9),
    }
    if op in fades:
        banks, setting = fades[op]
        compiler.sources.add("disasm/code/common/scripting/map/mapscriptengine_1.asm")
        compiler.sources.add("disasm/code/common/tech/interrupts/fadingcommands.asm")
        values = [
            compiler.number(value)
            for row in compiler.source_operations(
                "disasm/data/tech/fadingdata.asm", "table_FadingData"
            )
            if row["opcode"] == "dc.b"
            for value in _tokens(row["operandText"])
        ]
        tail = values[(setting - 1) * 8 :]
        offsets = [value if value < 128 else value - 256 for value in tail[: tail.index(128)]]
        if not 0 < len(offsets) <= 8 or any(not -7 <= value <= 7 for value in offsets):
            raise ValueError("scene palette fade binding changed")
        return {"op": "scene-palette-fade", "banks": banks, "offsets": offsets, "period": 6}
    if op == "executeSubroutine" and args == ["ApplyStormEffect"]:
        path = "disasm/data/scripting/map/cs_intro_stormeffect.asm"
        colors = [
            compiler.number(value)
            for row in compiler.source_operations(path, "palette_IntroStormEffect")
            if row["opcode"] == "dc.w"
            for value in _tokens(row["operandText"])
        ]
        if len(colors) != 16:
            raise ValueError("storm palette binding changed")
        return {"op": "scene-palette-pulse", "bank": 0, "colors": colors, "ticks": 10}
    return None


def sound_operation(command):
    control = {"SOUND_COMMAND_FADE_OUT": "SoundFade", "SOUND_COMMAND_CUT_OUT": "SoundCut"}.get(
        command
    )
    return {
        "op": "present",
        "kind": control or "Sound",
        "resource": None if control else command,
        "entity": None,
        "position": None,
    }


def portrait_operations(compiler, speaker, event_speaker, source):
    if speaker is None and not event_speaker:
        return [{"op": "speaker", "entity": None}, compiler.native("jsr", source)]
    portrait = {"op": "open-portrait", "entity": speaker, "flags": 0}
    if speaker is None and event_speaker:
        portrait["useEventSpeaker"] = True
    return [{"op": "speaker", "entity": speaker}, portrait]


def scene_entities(compiler, relative: str, symbol: str) -> dict[str, Any]:
    rows = compiler.source_operations(relative, symbol)
    if len(rows) == 1 and rows[0]["opcode"] == "mainEntity":
        storage = "disasm/data/maps/mapsetupsstorage.asm"
        compiler.sources.add(storage)
        includes = re.findall(r'include\s+"([^"]+)"', (compiler.upstream / storage).read_text())
        paths = ["disasm/" + path.replace(chr(92), "/") for path in includes]
        tail = paths[paths.index(relative) + 1]
        first = re.search(r"^(\w+):", (compiler.upstream / tail).read_text(), re.MULTILINE)
        rows.extend(compiler.source_operations(tail, first[1]))
    if rows[0]["opcode"] != "mainEntity" or rows[-1]["opcode"] not in (
        "cscEntitiesEnd",
        "msEntitiesEnd",
    ):
        raise ValueError("source cutscene entity table boundary")
    x, y, facing = map(compiler.number, _tokens(rows[0]["operandText"]))
    entities = []
    non_ally = 128
    for row in rows[1:-1]:
        if row["opcode"] not in ("entity", "entityRandomWalk"):
            raise ValueError("source cutscene entity record")
        values = _tokens(row["operandText"])
        ex, ey, ef, sprite = map(compiler.number, values[:4])
        if sprite >= compiler.equates["MAPSPRITES_SPECIALS_START"] and sprite not in (
            compiler.equates["MAPSPRITE_KRAKEN_HEAD"],
            compiler.equates["MAPSPRITE_TAROS"],
        ):
            raise ValueError("special cutscene entity not admitted")
        identity = sprite if sprite < 30 else non_ally
        non_ally += int(sprite >= 30)
        entities.append(
            {
                "id": f"entity-{identity}",
                "position": {"x": ex & 63, "y": ey & 63},
                "facing": ef,
                "sprite": sprite,
                "speed": compiler.entity_speed,
                "visible": True,
                "obstruction": False,
                "actions": (
                    walking_actions(compiler, *map(compiler.number, values[4:]))
                    if row["opcode"] == "entityRandomWalk"
                    else action_stream(compiler, values[4])
                ),
            }
        )
    return {
        "op": "scene-entities",
        "position": {"x": x, "y": y},
        "facing": facing,
        "population": compiler.population(),
        "entities": entities,
    }


def graphics_helper(compiler, symbol):
    variants = {"csub_5BFD0": "evil-spirit", "sub_5BFDA": "evil-spirit-alt", "sub_5BFE4": "zeon"}
    if symbol in variants:
        path = "disasm/data/maps/entries/map34/mapsetups/scripts.asm"
        compiler.sources.update((path, "disasm/code/common/tech/graphics/specialsprites.asm"))
        rows = compiler.source_operations(path, symbol)
        selection = list(variants).index(symbol)
        if [(r["opcode"], _tokens(r["operandText"])) for r in rows] != [
            ("moveq", [f"#{selection}", "d0"]),
            ("jsr", ["j_AnimateSpecialSprite"]),
            ("rts", []),
        ]:
            raise ValueError("shared special graphics helper changed")
        return [{"op": "shared-sprite-graphics", "slot": 32, "resource": variants[symbol]}]
    if symbol not in ("csub_55EF4", "sub_55F82"):
        return None
    path = "disasm/data/maps/entries/map07/mapsetups/scripts.asm"
    compiler.sources.update((path, "disasm/code/common/tech/graphics/updatefadingpalette.asm"))
    if symbol == "sub_55F82":
        return [{"op": "restore-palette-copy"}]
    rows = compiler.source_operations(path, "palette_55F6A")
    tables = [
        byte
        for r in rows
        if r["opcode"] == "dc.w"
        for v in _tokens(r["operandText"])
        for byte in compiler.number(v).to_bytes(2, "big")
    ]
    if len(tables) != 24:
        raise ValueError("palette remap table changed")
    lookup = []
    for index in range(512):
        word = ((index & 448) << 3) | ((index & 56) << 2) | ((index & 7) << 1)
        total = ((word & 0xE00) >> 9) + (word & 14) + ((word & 0xE0) >> 3)
        q, remainder = divmod(total, 7)
        q += remainder >= 4
        lookup.append((tables[16 + q] << 8) | (tables[7 - q] << 4) | tables[8 + 7 - q])
    return [{"op": "remap-palette", "lookup": lookup}]
