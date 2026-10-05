"""Held logical wait identity, caller, readiness policy and cue operands."""


def entity(s, subject):
    return next((x for x in s.get("entities") or [] if x.get("id") == subject), None)


def motion_wait(entry, r, e, role, states, loc, ins, subject, token, check, operand):
    check("operation", "logical wait entry retained", True if entry else None, token)
    check("consumer", "actual wait context retained", True if entry else None, token)
    if entry:
        operand(
            "operation",
            token,
            "same session at producer",
            [entry.get("sessionId"), r["result"].get("sessionId")],
            lambda a, b: a == b,
        )
        operand(
            "operation",
            token,
            "revision at producer",
            [entry.get("revision"), e.get("Revision")],
            lambda a, b: a >= b,
        )
    expected_wait = {
        "motion": "EntityWait",
        "nod": "NodWait",
        "full-fade": "FullFadeWait",
        "present": "PresentationWait",
    }[role]
    for s in states:
        for family in ("operation", "consumer"):
            operand(
                family,
                token,
                "held wait kind",
                [s.get("wait")],
                lambda a, expected_wait=expected_wait: a == expected_wait,
            )
            operand(
                family,
                token,
                "held source cursor",
                [s.get("cursor") or {} if "cursor" in s else None, loc or {}],
                lambda a, b: a == b,
            )
            operand(
                family,
                token,
                "held caller stack",
                [s.get("callers"), (entry or {}).get("callers")],
                lambda a, b: a == b,
            )
        wait = s.get("entityWait") if role == "motion" else s.get("nod") if role == "nod" else None
        if role == "motion":
            operand(
                "operation",
                token,
                "held source wait policy",
                [(wait or {}).get("Completion")],
                lambda policy, ins=ins: (
                    policy == (0 if ins.get("installation") == "Preserve" else 1)
                ),
            )
        if role in ("motion", "nod"):
            for family in ("operation", "consumer"):
                operand(
                    family,
                    token,
                    "logical wait subject",
                    [((wait or {}).get("Entity") or {}).get("Value")],
                    lambda a, subject=subject: a == subject,
                )
        if role == "present":
            cue = (s.get("presentationWait") or {}).get("Cue")
            operand(
                "operation",
                token,
                "source cue resource",
                [(cue or {}).get("Resource"), ins.get("resource")],
                lambda a, b: a == b,
            )
            check(
                "operation",
                "source cue entity",
                None
                if cue is None or "Entity" not in cue
                else cue["Entity"] == (dict(Value=subject) if subject else None),
                token,
            )
