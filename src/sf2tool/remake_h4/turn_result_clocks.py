"""Compare delivered result, event and sample clock axes before applicability exits."""

from sf2tool.remake_h4.turn_consumer_checks import CLOCK_KEYS as clock_keys
from sf2tool.remake_h4.turn_consumer_checks import number


def compare_result_clocks(channels, indices, session, checks):
    check, match, absent = checks.check, checks.match, checks.absent
    clocks, precedes, join = checks.clocks, checks.precedes, checks.join
    # Check available clock contradictions before generation/census applicability.
    # A missing independent operand must not hide an invalid delivered identity.
    previous = None
    previous_clock = {}
    for index, row in channels["warpRecords"].items():
        if not row and index not in (indices.get("warpRecords") or []):
            continue
        body, state = row.get("result") or {}, row.get("state") or {}
        clocks("result", body, index)
        if previous is not None:
            precedes("result clocks progress in source-index order", previous_clock, body, index)
        events = body.get("observations")
        check(
            "result observation channel",
            None if events is None else isinstance(events, list),
            index=index,
        )
        last = None
        for event in events or []:
            clocks("event", event, index, ("Revision", "Sequence"))
            ec = dict(revision=event.get("Revision"), observationSequence=event.get("Sequence"))
            precedes("event clocks bounded by owning result", ec, body, index)
            if last is not None:
                precedes("ordered event clock axes", last, ec, index)
                check(
                    "strict ordered event sequence",
                    ec["observationSequence"] > last["observationSequence"]
                    if number(ec["observationSequence"]) and number(last["observationSequence"])
                    else None,
                    index=index,
                )
            elif previous is not None and body.get("boundary") != "attach":
                precedes("new events follow preceding result", previous_clock, ec, index)
                check(
                    "new event sequence follows preceding result",
                    ec["observationSequence"] > previous_clock["observationSequence"]
                    if number(ec["observationSequence"])
                    and number(previous_clock.get("observationSequence"))
                    else None,
                    index=index,
                )
            last = ec
        if last is not None:
            end = body.get("observationSequence")
            sequence = last["observationSequence"]
            check(
                "last event closes result sequence",
                None
                if end is None
                or sequence is None
                or number(end)
                and number(sequence)
                and sequence < end
                else match(end, sequence),
                index=index,
            )
        if body.get("boundary") == "attach":
            # The observer publishes attach before building its view, and may
            # republish the preceding Submit. Only that evidenced seam permits {}.
            join("attach republishes preceding result identity", previous or {}, body, index)
            check(
                "attach republishes preceding events",
                match(
                    (previous or {}).get("observations", absent),
                    events if events is not None else absent,
                ),
                index=index,
            )
        if state or body.get("boundary") != "attach":
            clocks("delivered poststate", state, index)
            join("delivered poststate owns result identity", body, state, index)
        previous = body
        previous_clock.update({k: body[k] for k in clock_keys if number(body.get(k))})
    for index, row in channels["samples"].items():
        if not row and index not in (indices.get("samples") or []):
            continue
        state = row.get("state") or {}
        clocks("sample", state, index)
        check("sample session", match(session, state.get("sessionId", absent)), index=index)
