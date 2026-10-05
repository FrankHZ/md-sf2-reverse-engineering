"""Nod and presentation completion handoffs before logical continuation."""

from sf2tool.remake_h4.motion_fade import motion_fade
from sf2tool.remake_h4.motion_shiver import motion_shiver


def motion_completion(
    ins,
    role,
    token,
    following,
    subject,
    rows,
    states,
    helper,
    entry,
    record_by_event,
    warp_records,
    actual,
    check,
    operand,
    _bounded_list,
):
    kind = ins.get("kind")
    end_kind = (
        "nod-returned"
        if role == "nod"
        else "full-fade-completed"
        if role == "full-fade"
        else "presentation-completed"
    )
    end = following(token, end_kind)
    if end is not None and role != "nod":
        operand(
            "operation",
            token,
            "completion kind",
            [end.get("Detail"), kind],
            lambda a, b: a == b,
        )
    if role == "nod":
        operand(
            "operation",
            token,
            "nod normal animation restore",
            [(end or {}).get("After")],
            lambda a: a == 0,
        )
        operand(
            "operation",
            token,
            "nod restored subject",
            [((end or {}).get("Entity") or {}).get("Value")],
            lambda a, subject=subject: a == subject,
        )
    handoffs = [b for b in rows if b.get("projectionStage") == "completion-before-submit"]
    handoff = handoffs[0] if len(handoffs) == 1 else None
    check(
        "consumer",
        "unique actual completion handoff",
        None if not handoffs else len(handoffs) == 1,
        token,
    )
    if handoff:
        completion = handoff.get("result", {}).get("completion") or {}
        cue = handoff["state"].get("presentationCue") or {}
        for name, value, expected in (
            ("actual completion token", completion.get("token"), token),
            ("actual completion kind", completion.get("kind"), kind),
            ("live cue token", cue.get("token"), token),
            ("live cue kind", cue.get("kind"), kind),
        ):
            operand(
                "consumer",
                token,
                name,
                [value],
                lambda a, expected=expected: a == expected,
            )
        operand(
            "consumer",
            token,
            "handoff before logical completion",
            [handoff["state"].get("revision"), (end or {}).get("Revision")],
            lambda a, b: a < b,
        )
        if ins.get("resource") == "shiver":
            operand(
                "consumer",
                token,
                "shiver finite completion",
                [cue.get("elapsed")],
                lambda a: a >= 0.5,
            )
            operand(
                "consumer",
                token,
                "shiver handoff subject",
                [cue.get("gesture")],
                lambda a, subject=subject: a == subject,
            )
            operand(
                "consumer",
                token,
                "shiver handoff flag",
                [cue.get("shivering")],
                lambda a: a is True,
            )
            operand(
                "consumer",
                token,
                "shiver phase remains installed at handoff",
                [cue.get("gesture"), cue.get("shivering"), cue.get("elapsed")],
                lambda a, b, c, subject=subject: a == subject and b is True and c >= 0.5,
            )
        if ins.get("resource") in ("mosaic-in", "mosaic-out"):
            operand(
                "consumer",
                token,
                "mosaic finite completion",
                [cue.get("elapsed")],
                lambda a: a >= 0.5,
            )
            operand(
                "consumer",
                token,
                "mosaic handoff subject",
                [cue.get("mosaic")],
                lambda a, subject=subject: a == subject,
            )
            operand(
                "consumer",
                token,
                "mosaic handoff direction",
                [cue.get("mosaicOut")],
                lambda a, ins=ins: a is (ins["resource"] == "mosaic-out"),
            )
            operand(
                "consumer",
                token,
                "mosaic direction and finite completion",
                [cue.get("mosaic"), cue.get("mosaicOut"), cue.get("elapsed")],
                lambda a, b, c, ins=ins, subject=subject: (
                    a == subject and b is (ins["resource"] == "mosaic-out") and c >= 0.5
                ),
            )
        if ins.get("resource") in ("black", "white"):
            field = "whiteOpacity" if ins["resource"] == "white" else "paletteBrightness"
            expected = int(kind == "FadeOut") if field == "whiteOpacity" else int(kind == "FadeIn")
            operand(
                "consumer",
                token,
                "actual fade endpoint",
                [cue.get(field)],
                lambda a, expected=expected: abs(a - expected) < 1e-6,
            )
            operand(
                "consumer",
                token,
                "fade finite completion",
                [cue.get("elapsed")],
                lambda a: a >= 0.5,
            )
            operand(
                "consumer",
                token,
                "fade owner visible",
                [cue.get("ownerVisible")],
                lambda a: a is True,
            )
    if role == "full-fade":
        motion_fade(states, ins, helper, token, check, operand, _bounded_list)
    if ins.get("resource") == "shiver":
        motion_shiver(
            entry, states, record_by_event, end, warp_records, actual, token, subject, operand
        )
    return end
