"""Lower source cutscene layout, actor-position and sound commands."""


def scene_operation(compiler, op, args):
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
