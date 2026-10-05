"""Logical visibility applicability and retained consumer phase coverage."""

from sf2tool.remake_h4.motion_phases import phase_reader
from sf2tool.remake_h4.motion_projection import motion_projection
from sf2tool.remake_h4.motion_wait import entity


def motion_draws(rows, role, ins, subject, states, token, check, operand, _bounded_list):
    draws = _bounded_list(b for b in rows if b.get("projectionStage") == "frame-post-draw")
    used = _bounded_list()
    effect = "nod" if role == "nod" else ins.get("resource")
    phases = (
        ("normal-before", "lowered", "normal-after")
        if effect == "nod"
        else (-1, 1)
        if effect == "shiver"
        else (8, 6, 4, 2, 1)
        if effect in ("mosaic-in", "mosaic-out")
        else ()
    )
    applicability = {phase: _bounded_list() for phase in phases}
    phase_use = set()

    semantic_phases = phase_reader(effect, subject, token)

    for state in states:
        logical = entity(state, subject)
        presentation = state.get("presentation") or {}
        visible = None
        x = y = None
        if logical is not None and logical.get("Visible") is False:
            visible = False
        elif logical is not None and all(
            value is not None
            for value in (
                logical.get("x"),
                logical.get("y"),
                logical.get("Visible"),
                presentation.get("cameraX"),
                presentation.get("cameraY"),
            )
        ):
            x = logical["x"] / 16 - presentation["cameraX"]
            y = logical["y"] / 16 - presentation["cameraY"]
            # Semantic coverage follows logical pose and the accepted
            # 320x192 viewport, independently of surviving actor draws.
            visible = logical["Visible"] and x + 24 > 0 and y + 24 > 0 and x < 320 and y < 192
        for phase in semantic_phases(state):
            phase_visible = visible
            if x is not None and y is not None:
                phase_x = x + (phase if effect == "shiver" else 0)
                phase_visible = (
                    logical["Visible"]
                    and phase_x + 24 > 0
                    and y + 24 > 0
                    and phase_x < 320
                    and y < 192
                )
            applicability[phase].append(phase_visible)
    motion_projection(
        draws, subject, token, ins, role, effect, semantic_phases, used, phase_use, check, operand
    )
    for phase, visibility in applicability.items():
        check(
            "consumer",
            "required visible semantic phase " + str(phase),
            True
            if phase in phase_use
            else True
            if visibility and all(value is False for value in visibility)
            else None,
            token,
        )
    # Generic loader fades after mounting have a frozen field camera; their
    # live modulation and handoff are the actual consumer, not a new actor draw.
    if subject is None and role != "full-fade":
        used = [True for b in rows if b.get("projectionStage") == "completion-before-submit"]
    check(
        "consumer",
        "actual use retained without per-tick draw quota",
        True if used else None,
        token,
    )
