"""Bind complete selected Submit/event envelopes and W1/W2 operation attribution."""

from sf2tool.remake_h4.w1_checks import merge


def compare_envelopes(inputs, records, polls, session, checks):
    check, match, absent = checks.check, checks.match, checks.absent
    w1_operations = {"rng-text-w1", "text-w1-wait", "text-w1-input", "text-w1-accepted"}
    w2_operations = {"rng-text-w2", "text-w2-wait", "text-w2-input", "text-w2-accepted"}
    reached = [
        i
        for i, row in records.items()
        if any(
            e.get("Kind") in w1_operations
            for e in (row.get("result") or {}).get("observations", [])
        )
    ]
    wanted = {p.get("resultIndex") for p in polls}
    check(
        "complete candidate poll coverage",
        merge(
            [
                None if not wanted else not (set(reached) - wanted),
                True if wanted and set(reached) == wanted else None,
            ]
        ),
    )
    # A whole selected Submit binds its result envelope to its own state. This
    # also covers reveal/typewriting/choice rows outside the inventoried W1 polls.
    # Use an adjacent original result or the physical input's before state for
    # the lower bound; a gap in the selection is not a fabricated predecessor.
    input_starts = {}
    for delivered in inputs.values():
        if delivered.get("resultStart") is not None:
            input_starts.setdefault(delivered["resultStart"], []).append(delivered)
    for index_, selected_record in records.items():
        selected_result = selected_record.get("result") or {}
        selected_state = selected_record.get("state") or {}
        ordinal = selected_record.get("inputOrdinal")
        check(
            "selected Submit result/state envelope",
            match(
                dict(
                    boundary="submit",
                    sessionId=session,
                    failure=None,
                    **{
                        key: selected_state.get(key, absent)
                        for key in ("revision", "observationSequence", "mode")
                    },
                ),
                selected_result,
            ),
            ordinal,
        )
        check(
            "selected Submit state session/failure",
            match(dict(sessionId=session, failure=None), selected_state),
            ordinal,
        )
        selected_events = selected_result.get("observations")
        check(
            "selected Submit observations present",
            True if selected_events is not None else None,
            ordinal,
        )
        selected_kinds = {event.get("Kind") for event in selected_events or []}
        if index_ not in wanted and "text-seed-copy" in selected_kinds:
            # The copy operation is shared by W1/W2. A retained W2 operation
            # supplies its attribution; reveal/typewriting/choice is not a poll.
            check(
                "shared text copy has a selected poll owner",
                True
                if selected_kinds & w2_operations
                else False
                if selected_kinds
                & {
                    "text-revealed",
                    "text-mandatory-service",
                    "text-work-advanced",
                    "choice-opened",
                    "choice-sound",
                    "choice-window-moving",
                }
                else None,
                ordinal,
            )
        predecessors = []
        if index_ - 1 in records:
            predecessors.append(records[index_ - 1].get("state") or {})
        predecessors.extend(
            delivered.get("before") or {}
            for delivered in input_starts.get(index_, [])
            if delivered.get("inputOrdinal", delivered.get("ordinal")) == ordinal
        )
        for event_key, state_key in (("Revision", "revision"), ("Sequence", "observationSequence")):
            upper = selected_result.get(state_key)
            values = [event.get(event_key) for event in selected_events or []]
            available = [value for value in values if value is not None]
            checks = [
                True if len(available) == len(values) else None,
                available == sorted(set(available)),
                None
                if upper is None
                else upper >= 0 and all(0 < value <= upper for value in available),
            ]
            for predecessor in predecessors:
                lower = predecessor.get(state_key)
                checks.append(
                    None
                    if lower is None or upper is None
                    else lower <= upper and all(lower < value for value in available)
                )
            check("selected Submit event " + event_key + " envelope", merge(checks), ordinal)
