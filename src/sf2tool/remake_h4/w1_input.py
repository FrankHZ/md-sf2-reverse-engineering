"""Join physical input, complete choice delivery and selected ready/post snapshots."""

from sf2tool.remake_h4.w1_checks import merge

_IDENTITY_KEYS = (
    "sessionId",
    "revision",
    "observationSequence",
    "simulationTick",
    "mainSeed",
    "token",
    "cursor",
    "wait",
    "map",
    "mode",
)


def snapshot(states, refs, ordinal, label, checks):
    check, match = checks.check, checks.match
    observed = [
        states[(r.get("channel"), r.get("index"))]
        for r in refs
        if (r.get("channel"), r.get("index")) in states
    ]
    check(label + " present", True if observed else None, ordinal)
    if observed:
        check(
            label + " duplicates agree",
            merge([match(observed[0], s) for s in observed]),
            ordinal,
        )
    return observed[0] if observed else {}


def compare_input(poll, owner, inputs, records, states, session, checks):
    check, match, absent = checks.check, checks.match, checks.absent
    ordinal, accepting = poll.get("ordinal"), poll.get("accepting")
    cur, token = owner.get("cursor") or {}, poll.get("token")
    inp = inputs.get(poll.get("inputIndex"), {})
    before, after = inp.get("before") or {}, inp.get("after") or {}
    check(
        "physical input identity",
        match(
            dict(
                ordinal=ordinal,
                pressed=True,
                action="confirm" if accepting else "wait",
                delivery=dict(kind="key", code=4194309 if accepting else 86),
                before=dict(
                    sessionId=session,
                    token=token,
                    cursor=cur,
                    wait="FieldTextWait",
                    canWaitForText=True,
                    revision=poll.get("beforeRevision"),
                ),
            ),
            inp,
        ),
        ordinal,
    )
    record = records.get(poll.get("resultIndex"), {})
    submit, state = record.get("result") or {}, record.get("state") or {}
    start, end = inp.get("resultStart"), inp.get("resultEnd")
    check(
        "whole Submit in physical input span",
        None if start is None or end is None else start <= poll["resultIndex"] < end,
        ordinal,
    )
    check(
        "whole Submit identity",
        match(
            dict(
                inputOrdinal=ordinal,
                result=dict(
                    boundary="submit",
                    sessionId=session,
                    revision=poll.get("resultRevision"),
                    failure=None,
                ),
                state=dict(sessionId=session, revision=poll.get("resultRevision")),
            ),
            record,
        ),
        ordinal,
    )
    span = (
        [records.get(i, {}) for i in range(int(start), int(end))]
        if start is not None and end is not None
        else []
    )
    tail = span[-1].get("state", {}) if span else {}
    check(
        "input after joins complete delivery span",
        match({k: tail.get(k, absent) for k in _IDENTITY_KEYS}, after),
        ordinal,
    )
    for extra in span[1:]:
        extra_events = (extra.get("result") or {}).get("observations", [])
        check(
            "source choice delivery preserves W1 state",
            match(
                {
                    k: state.get(k, absent)
                    for k in _IDENTITY_KEYS
                    if k not in ("revision", "observationSequence")
                },
                extra.get("state", absent),
            ),
            ordinal,
        )
        check(
            "separate choice delivery identity",
            match(
                dict(
                    inputOrdinal=ordinal,
                    result=dict(boundary="submit", sessionId=session, failure=None),
                ),
                extra,
            ),
            ordinal,
        )
        check(
            "separate choice delivery has no text poll",
            merge(
                [
                    match("ChoiceWait", state.get("wait", absent)),
                    *[
                        None
                        if e.get("Kind") is None
                        else e["Kind"] in ("choice-opened", "choice-sound", "choice-window-moving")
                        for e in extra_events
                    ],
                ]
            ),
            ordinal,
        )
    ready = snapshot(states, poll.get("ready", []), ordinal, "ready", checks)
    post = (
        snapshot(states, poll.get("after", []), ordinal, "after", checks)
        if poll.get("after")
        else {}
    )
    check(
        "ready joins input",
        match({k: before.get(k, absent) for k in _IDENTITY_KEYS}, ready),
        ordinal,
    )
    if post:
        check(
            "after joins result",
            match({k: after.get(k, absent) for k in _IDENTITY_KEYS}, post),
            ordinal,
        )
    check(
        "one admitted logical service",
        None
        if before.get("simulationTick") is None or after.get("simulationTick") is None
        else after["simulationTick"] == before["simulationTick"] + 1,
        ordinal,
    )
    return before, after, submit, ready, post
