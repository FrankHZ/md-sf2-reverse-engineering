"""Join supplied events, repeated publications and retained consumer identities."""

from sf2tool.remake_h4.turn_consumer_checks import merge, number


def compare_evidence(
    context, channels, indices, census, expected, queues, session, diagnostics, checks
):
    check, match, absent = checks.check, checks.match, checks.absent
    supplied, publications = {}, {}
    required_rows = {}
    census_kinds = {c[3] for c in census}
    owning_indices = context.get("owningResultIndices")
    check(
        "independent owning-result inventory",
        None if owning_indices is None else {c[0] for c in census}.issubset(owning_indices),
    )
    for index in indices.get("warpRecords") or []:
        row = channels["warpRecords"].get(index)
        if row is None:
            continue
        required_rows[index] = row
        body, state = row.get("result") or {}, row.get("state") or {}
        owning = (
            index in {c[0] for c in census}
            or index in (owning_indices or [])
            or any(e.get("Kind") in census_kinds for e in body.get("observations") or [])
        )
        if not owning and body.get("failure") is not None:
            diagnostics.append(dict(index=index, failure=body["failure"], turnConsumed=False))
        check(
            "owning result/state acceptance and identity",
            merge(
                [
                    match(session, body.get("sessionId", absent)),
                    match(None, body.get("failure", absent)) if owning else True,
                    match(session, state["sessionId"]) if "sessionId" in state else True,
                    match(None, state["failure"]) if owning and "failure" in state else True,
                    match(
                        (context.get("callerFields") or {}).get(str(index), {}),
                        {
                            k: row.get(k, absent)
                            for k in (context.get("callerFields") or {}).get(str(index), {})
                        },
                    ),
                ]
            ),
            index=index,
        )
        state_fields = (context.get("stateFields") or {}).get(str(index))
        check(
            "supplied state leaves at applicable seam",
            None
            if state_fields is None
            else merge([True if key in state else None for key in state_fields]),
            index=index,
        )
        events = body.get("observations")
        check(
            "owning ordered events",
            None
            if events is None
            else merge(
                [None if e.get("Sequence") is None else number(e["Sequence"]) for e in events]
                + [
                    [e["Sequence"] for e in events if number(e.get("Sequence"))]
                    == sorted({e["Sequence"] for e in events if number(e.get("Sequence"))})
                ]
            ),
            index=index,
        )
        for event in events or []:
            sequence = event.get("Sequence")
            if not number(sequence):
                continue
            if sequence in supplied:
                check(
                    "repeated publication payload agrees",
                    match(supplied[sequence], event),
                    sequence=sequence,
                )
            else:
                supplied[sequence] = event
            publications.setdefault(sequence, set()).add(index)
            if event.get("Kind") in census_kinds:
                check(
                    "consumer belongs to independent census",
                    sequence in expected,
                    sequence=sequence,
                )
    for sequence, c in expected.items():
        event = supplied.get(sequence)
        check(
            "selected event owning source index",
            None if event is None else c[0] in publications[sequence],
            sequence=sequence,
        )
        check(
            "selected event identity",
            match(
                dict(
                    Revision=c[1],
                    Sequence=c[2],
                    Kind=c[3],
                    Actor=None if c[4] is None else dict(Value=c[4]),
                    Target=None if c[5] is None else dict(Value=c[5]),
                ),
                event if event is not None else absent,
            ),
            sequence=sequence,
        )
    for index, row in channels["warpRecords"].items():
        state = row.get("state") or {}
        if "turnOrder" in state and state.get("round") in queues:
            check(
                "available supplied queue at declared round",
                match(queues[state["round"]], state["turnOrder"]),
                index=index,
            )
    return supplied, publications, required_rows, census_kinds
