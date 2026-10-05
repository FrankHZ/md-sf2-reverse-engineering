"""Source motion destinations and occurrence-local predicate release."""

from sf2tool.remake_h4.motion_wait import entity


def motion_commands(
    release_by_token, token, ins, subject, entry, states, occurrence, release_boundary, loc, operand
):
    release = release_by_token.get(token)
    end = release
    payload = (release or {}).get("EntityWaitRelease") or {}
    expected_policy = 0 if ins.get("installation") == "Preserve" else 1
    for family in ("operation", "consumer"):
        operand(
            family,
            token,
            "released subject",
            [(payload.get("Subject") or {}).get("Value")],
            lambda a, subject=subject: a == subject,
        )
        operand(
            family,
            token,
            "released wait policy",
            [payload.get("Completion")],
            lambda a, expected_policy=expected_policy: a == expected_policy,
        )
        operand(
            family,
            token,
            "source wait predicate",
            [payload.get("IsScriptIdle") if expected_policy else payload.get("Busy")],
            lambda p, expected_policy=expected_policy: p is bool(expected_policy),
        )
    idle = next((i for i, a in enumerate(ins["actions"]) if a["op"] == "idle"), None)
    if expected_policy:
        operand(
            "operation",
            token,
            "source Idle action cursor",
            [payload.get("ActionCursor"), idle],
            lambda a, b: a == b,
        )
    entry_entity = entity(entry or {}, subject)
    # The release can immediately hide/reposition/reinstall this slot.
    # Source destinations belong to the held wait; ScriptIdle is the
    # actual release predicate, not a terminal raster/arrival quota.
    held_entity = next((entity(s, subject) for s in reversed(states) if entity(s, subject)), None)
    occurrence["motionOperands"] = dict(
        entry=entry_entity, held=held_entity, actions=ins["actions"], release=payload
    )
    moves = [a for a in ins["actions"] if a["op"] == "move"]
    if any(m.get("wait") is False for m in moves):
        # ac_moveRel does not await arrival. Its next command starts
        # from the then-current pose, not a sum of prior destinations.
        for index, action in enumerate(ins["actions"]):
            if action["op"] != "move":
                continue
            phase = next(
                (
                    entity(s, subject)
                    for s in states
                    if (entity(s, subject) or {}).get("actionCursor") == index + 1
                ),
                None,
            )
            if action["x"] == action["y"] == 0 and release:
                phase = (
                    entity(release_boundary[release["Sequence"]]["state"], subject)
                    if release["Sequence"] in release_boundary
                    else None
                )
            for axis in ("x", "y"):
                operand(
                    "operation",
                    token,
                    "nonwaiting source command destination " + str(index) + axis,
                    [
                        (phase or {}).get(axis),
                        (phase or {}).get("target" + axis.upper()),
                    ],
                    lambda a, b, axis=axis, action=action: b == a + 384 * action[axis],
                )
    else:
        for axis in ("x", "y"):
            operand(
                "operation",
                token,
                "source relative destination " + axis,
                [
                    (entry_entity or {}).get(axis),
                    (held_entity or {}).get("target" + axis.upper()),
                ],
                lambda a, b, axis=axis, moves=moves: b == a + 384 * sum(m[axis] for m in moves),
            )
    operand(
        "operation",
        token,
        "held caller until release",
        [payload.get("Caller") or {} if "Caller" in payload else None, loc or {}],
        lambda a, b: a == b,
    )
    return end
