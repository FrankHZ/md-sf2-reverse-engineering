"""Compare accepting W2 token release and independent source resumed producers."""

from sf2tool.remake_h4.w2_checks import merge


def resumed_producers(expected, observed, checks):
    if expected is None:
        return None
    positions = {
        (e["Program"]["Program"], e["Program"]["Instruction"]): i for i, e in enumerate(expected)
    }
    indices, values = [], []
    for event in observed:
        producer = event.get("Program") or {}
        if producer.get("Program") is None or producer.get("Instruction") is None:
            values.append(None)
            continue
        position = positions.get((producer["Program"], producer["Instruction"]))
        if position is None:
            values.append(False)
            continue
        indices.append(position)
        values.append(checks.match(expected[position], event))
    return merge(
        values + [indices == sorted(set(indices)), True if len(indices) == len(expected) else None]
    )


def compare_continuation(owner, after, events, token, source, ordinal, checks):
    check, match, absent = checks.check, checks.match, checks.absent
    check(
        "actual continuation token and wait",
        match(dict(token=owner.get("nextToken"), wait=owner.get("nextWait")), after),
        ordinal,
    )
    expected_resume, terminal_cursor = source.continuation(owner)
    check(
        "source resumed producers and order",
        resumed_producers(
            expected_resume, [e for e in events if e.get("Kind") == "program-instruction"], checks
        ),
        ordinal,
    )
    check(
        "source continuation terminal cursor",
        None if terminal_cursor is None else match(terminal_cursor, after.get("cursor", absent)),
        ordinal,
    )
    check(
        "accepted token released",
        None if after.get("token") is None else after["token"] != token,
        ordinal,
    )
    return expected_resume
