"""Saved shiver state and completed-cue restoration readback."""

import itertools

from sf2tool.remake_h4.motion_wait import entity


def motion_shiver(
    entry, states, record_by_event, end, warp_records, actual, token, subject, operand
):
    restore = ((entry or {}).get("presentationWait") or {}).get("Restore") or {}
    for state in states:
        held_restore = (state.get("presentationWait") or {}).get("Restore") or {}
        for field in ("AnimationCounter", "SpriteSize"):
            operand(
                "operation",
                token,
                "saved shiver " + field + " continuity",
                [held_restore.get(field), restore.get(field)],
                lambda a, b: a == b,
            )
    ordinal = record_by_event.get((end or {}).get("Sequence"))
    after = warp_records[ordinal].get("state", {}) if ordinal is not None else {}
    if after.get("spriteSize") is None and end:
        candidates = (
            state
            for x in itertools.chain(
                actual.get("samples", []), actual.get("battleEntryRecords", [])
            )
            if (state := x.get("state", {})).get("revision", -1) >= end["Revision"]
            and (state.get("presentation") or {}).get("completedCueToken") == token
            and state.get("spriteSize") is not None
        )
        selected = min(candidates, key=lambda state: state["revision"], default=None)
        if selected is not None:
            after = selected
    restored = entity(after, subject)
    for family in ("operation", "consumer"):
        operand(
            family,
            token,
            "shiver animation restoration",
            [
                restore.get("AnimationCounter"),
                (restored or {}).get("animationCounter"),
            ],
            lambda a, b: a == b,
        )
        operand(
            family,
            token,
            "shiver flags restoration",
            [(restored or {}).get("flagsB")],
            lambda a: int(a) & 8 == 0,
        )
        operand(
            family,
            token,
            "shiver sprite-size restoration",
            [restore.get("SpriteSize"), after.get("spriteSize")],
            lambda a, b: a == b,
        )
