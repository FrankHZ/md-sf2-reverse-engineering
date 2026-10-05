"""Compare wait/input frontier projections and additional applicable supplied evidence."""

from bisect import bisect_right

from sf2tool.remake_h4.turn_consumer_checks import merge, number


def compare_projection(
    context,
    channels,
    indices,
    session,
    queues,
    census,
    expected,
    supplied,
    required_rows,
    census_kinds,
    state_at,
    hp_images,
    checks,
):
    check, match, absent = checks.check, checks.match, checks.absent
    keys = sorted(state_at)
    for index, row in required_rows.items():
        state = row.get("state") or {}
        sequence = (row.get("result") or {}).get("observationSequence")
        pos = bisect_right(keys, sequence) - 1 if number(sequence) else -1
        if state.get("round") is not None and pos >= 0:
            r, slot = state_at[keys[pos]]
            check(
                "zero-event/wait result retains queue frontier",
                match(dict(round=r, queueCursor=slot), state),
                index=index,
            )
            image = hp_images.get(sequence)
            check(
                "supplied wait/input roster HP at its frontier",
                None
                if image is None
                else merge(
                    [
                        match(image.get(a.get("id"), absent), a.get("hp", absent))
                        for a in state.get("actors") or []
                    ]
                ),
                index=index,
            )
    for index in indices.get("inputRecords") or []:
        row = channels["inputRecords"].get(index) or {}
        values = [
            match(
                (context.get("inputOrdinals") or {}).get(str(index), absent),
                row.get("ordinal", absent),
            )
        ]
        fields = (context.get("inputFields") or {}).get(str(index))
        values.append(
            None
            if fields is None
            else merge(
                [
                    True if key in (row.get(side) or {}) else None
                    for side, names in fields.items()
                    for key in names
                ]
            )
        )
        for side in ("before", "after"):
            state = row.get(side) or {}
            values.append(match(session, state.get("sessionId", absent)))
            sequence = state.get("observationSequence")
            pos = bisect_right(keys, sequence) - 1 if number(sequence) else -1
            if pos >= 0 and state.get("actor") is not None:
                r, slot = state_at[keys[pos]]
                queue = queues.get(r)
                values.append(
                    match(
                        absent
                        if queue is None or not 0 <= slot < len(queue)
                        else queue[slot].get("actor", absent),
                        state["actor"],
                    )
                )
        check("input owns live queue without consuming on cancel/wait", merge(values), index=index)
    # Supplied modern channels may contain more records than the compact selection.
    # At its applicable seam, available evidence must not be ignored or replaced
    # by retained dependency records.
    lower = min((c[2] for c in census if c[3] == "round-started"), default=None)
    upper = max(expected, default=None)
    for index, row in channels["warpRecords"].items():
        if index in required_rows:
            continue
        body, state = row.get("result") or {}, row.get("state") or {}
        sequence = state.get("observationSequence", body.get("observationSequence"))
        events = [e for e in body.get("observations") or [] if e.get("Kind") in census_kinds]
        if lower is None or upper is None:
            continue
        available = []
        for value in (body, state):
            if "sessionId" in value:
                available.append(match(session, value["sessionId"]))
        queue = queues.get(state.get("round"))
        if queue is not None and "turnOrder" in state:
            available.append(match(queue, state["turnOrder"]))
        slot = state.get("queueCursor")
        if (
            queue is not None
            and isinstance(slot, (int, float))
            and slot == int(slot)
            and state.get("actor") is not None
        ):
            available.append(
                match(
                    absent if not 0 <= slot < len(queue) else queue[int(slot)].get("actor", absent),
                    state["actor"],
                )
            )
        if available:
            check(
                "additional independently available supplied leaves", merge(available), index=index
            )
        if not number(sequence) and (events or state.get("round") is not None):
            check("additional supplied applicability", None, index=index)
            continue
        applicable = (
            number(sequence)
            and lower <= sequence <= upper
            or any(number(e.get("Sequence")) and lower <= e["Sequence"] <= upper for e in events)
        )
        if not applicable:
            continue
        values = [match(session, body.get("sessionId", absent))]
        if events:
            values.append(match(None, body.get("failure", absent)))
        if "sessionId" in state:
            values.append(match(session, state["sessionId"]))
        if "turnOrder" in state and state.get("round") in queues:
            values.append(match(queues[state["round"]], state["turnOrder"]))
        if state.get("round") is not None and number(sequence):
            pos = bisect_right(keys, sequence) - 1
            if pos >= 0:
                r, slot = state_at[keys[pos]]
                values.append(match(dict(round=r, queueCursor=slot), state))
                if state.get("actor") is not None:
                    queue = queues.get(r)
                    values.append(
                        match(
                            absent
                            if queue is None or not 0 <= slot < len(queue)
                            else queue[slot].get("actor", absent),
                            state["actor"],
                        )
                    )
        for event in events:
            seq = event.get("Sequence")
            if not number(seq):
                values.append(None)
            elif lower <= seq <= upper:
                values.append(seq in expected)
                if seq in expected:
                    c = expected[seq]
                    values.append(
                        match(
                            dict(
                                Revision=c[1],
                                Sequence=c[2],
                                Kind=c[3],
                                Actor=None if c[4] is None else dict(Value=c[4]),
                                Target=None if c[5] is None else dict(Value=c[5]),
                            ),
                            event,
                        )
                    )
                    if seq in supplied:
                        values.append(match(supplied[seq], event))
        check("additional supplied evidence at applicable seam", merge(values), index=index)
