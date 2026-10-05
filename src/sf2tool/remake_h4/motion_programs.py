"""Per-call source lowering, instruction lookup and source spans."""


def target(i):
    return (
        i.get("op") == "motion"
        or i.get("op") == "present"
        and i.get("kind") in ("Gesture", "EntityEffect", "FadeIn", "FadeOut")
    )


def motion_programs(programs, compiler, tracked_source):
    source_values = {}
    source_spans = {}

    def instruction(location):
        if not location:
            return None
        p = programs.get(location.get("Program"))
        index = location.get("Instruction")
        if (
            p is None
            or index is None
            or int(index) != index
            or not 0 <= index < len(p["instructions"])
        ):
            return None
        return p["instructions"][int(index)]

    def source_value(program):
        if program in source_values:
            return source_values[program]
        p = programs.get(program)
        value = None
        if p is not None:
            selected = [i for i in p["instructions"] if target(i)]
            if program in ("source-battle-load", "source-outcome-return"):
                # Accepted modern wrappers, not an ordinary cutscene macro compilation.
                value = selected == [
                    dict(op="present", kind=kind, resource="black", entity=None, position=None)
                    for kind in ("FadeOut", "FadeIn")
                ]
                owner = (
                    "loadBattle.asm:LoadBattle"
                    if program == "source-battle-load"
                    else "explorationfunctions_2.asm:ExplorationLoop"
                )
                value = value and p.get("source", "").endswith(owner)
            elif compiler is not None and p.get("source", "").startswith("disasm/"):
                try:
                    if p["source"].rsplit(":", 1)[0] not in tracked_source:
                        source_values[program] = False
                        return False
                    symbol = p["source"].rsplit(":", 1)[1]
                    compiler.compile(symbol)
                    value = selected == [
                        i for i in compiler.programs[symbol]["instructions"] if target(i)
                    ]
                    macros = {
                        "entityActions",
                        "entityActionsWait",
                        "customActscript",
                        "customActscriptWait",
                        "setActscript",
                        "setActscriptWait",
                        "entityNodHead",
                        "nod",
                        "shiver",
                        "fadeInB",
                        "fadeOutB",
                        "slowFadeInB",
                        "slowFadeOutB",
                        "mapFadeOutToWhite",
                        "mapFadeInFromWhite",
                        "loadMapFadeIn",
                    }
                    source_spans[program] = [
                        op
                        for op in compiler.source_operations(p["source"].rsplit(":", 1)[0], symbol)
                        if op["opcode"] in macros
                        or op["opcode"] == "jsr"
                        and op["operandText"] == "MakeEntityWalk"
                        or op["opcode"] == "animEntityFX"
                        and any(name in op["operandText"] for name in ("MOSAIC_IN", "MOSAIC_OUT"))
                    ]
                    value = value and len(source_spans[program]) == len(selected)
                except (KeyError, OSError, ValueError, IndexError):
                    value = None
        source_values[program] = value
        return value

    return instruction, source_value, source_spans
