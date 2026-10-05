"""Join W2 physical input, whole Submit snapshots and retained neutral readiness."""

from sf2tool.remake_h4.w2_checks import merge


def compare_input(
    poll,
    accepting,
    session,
    cursor,
    token,
    ordinal,
    inputs,
    records,
    sample_revisions,
    neutrals,
    checks,
):
    check, match, one, absent = checks.check, checks.match, checks.one, checks.absent
    inp = one("one delivered input", inputs.get(ordinal, []), ordinal)
    before, after = inp.get("before") or {}, inp.get("after") or {}
    check(
        "eligible delivered input",
        match(
            dict(
                pressed=True,
                action="confirm" if accepting else "wait",
                delivery=dict(kind="key", code=4194309 if accepting else 86),
                before=dict(
                    sessionId=session,
                    token=token,
                    cursor=cursor,
                    wait="FieldTextWait",
                    canWaitForText=True,
                ),
            ),
            inp,
        ),
        ordinal,
    )
    start, end = inp.get("resultStart"), inp.get("resultEnd")
    selected = [r for n, r in records if start is not None and end is not None and start <= n < end]
    record = one("one result in input span", selected, ordinal)
    submission = record.get("result") or {}
    state = record.get("state") or {}
    revision = poll.get("resultRevision")
    check(
        "whole Submit result and state identity",
        match(
            dict(
                inputOrdinal=ordinal,
                result=dict(boundary="submit", sessionId=session, revision=revision, failure=None),
                state=dict(sessionId=session, revision=revision),
            ),
            record,
        ),
        ordinal,
    )
    check(
        "input after joins whole Submit",
        match(
            dict(
                sessionId=session,
                revision=revision,
                token=state.get("token", absent),
                wait=state.get("wait", absent),
            ),
            after,
        ),
        ordinal,
    )
    snapshot_keys = (
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
    check(
        "result snapshot joins input after",
        match({k: state.get(k, absent) for k in snapshot_keys}, after),
        ordinal,
    )
    check(
        "one accepting or neutral service",
        None
        if before.get("simulationTick") is None or after.get("simulationTick") is None
        else after["simulationTick"] == before["simulationTick"] + 1,
        ordinal,
    )
    ready_rows = sample_revisions.get(before.get("revision"), [])
    ready_evidence = "same-revision"
    ready_input = before
    if not ready_rows and accepting:
        # The2292 selection retains readiness before its neutral poll. Its
        # complete input/result edge carries those gates to the accept.
        bridges = [
            i
            for n in neutrals
            if n.get("token") == token
            for i in inputs.get(n.get("ordinal"), [])
            if (i.get("after") or {}).get("revision") == before.get("revision")
        ]
        bridge = one("ready neutral edge", bridges, ordinal)
        check(
            "neutral edge joins accepting input",
            match(
                {
                    k: before.get(k, absent)
                    for k in (
                        "sessionId",
                        "token",
                        "revision",
                        "cursor",
                        "wait",
                        "simulationTick",
                        "mainSeed",
                        "observationSequence",
                        "canWaitForText",
                    )
                },
                bridge.get("after", absent),
            ),
            ordinal,
        )
        ready_rows = sample_revisions.get((bridge.get("before") or {}).get("revision"), [])
        ready_evidence = "retained neutral edge"
        ready_input = bridge.get("before") or {}
    check("ready state available", True if ready_rows else None, ordinal)
    ready_state = (ready_rows[0].get("state") or {}) if ready_rows else {}
    check(
        "same-revision ready gate observations agree",
        merge(
            [
                match(
                    {
                        k: ready_state.get(k, absent)
                        for k in (
                            "sessionId",
                            "token",
                            "cursor",
                            "canWaitForText",
                            "entitiesRunning",
                            "eventCaller",
                            "portraitWindow",
                            "typewriting",
                            "logicalView",
                            "fieldText",
                        )
                    },
                    row.get("state", absent),
                )
                for row in ready_rows[1:]
            ]
        ),
        ordinal,
    )
    check(
        "ready snapshot joins input before",
        match({k: ready_input.get(k, absent) for k in snapshot_keys}, ready_state),
        ordinal,
    )
    return before, after, submission, ready_state, ready_evidence, revision
