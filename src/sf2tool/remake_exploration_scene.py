"""Lower source cutscene layout, actor-position and sound commands."""

import re
from typing import Any

from sf2tool.h2.battle_global_data import _tokens
from sf2tool.remake_exploration_actions import action_stream, walking_actions


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
        if (
            sprite >= compiler.equates["MAPSPRITES_SPECIALS_START"]
            and sprite != compiler.equates["MAPSPRITE_KRAKEN_HEAD"]
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
