"""Lower source cutscene layout, actor-position and sound commands."""


def scene_operation(compiler, op, args):
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
