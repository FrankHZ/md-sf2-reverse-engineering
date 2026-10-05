"""Compare W2 draw/copy/read ordering, Submit bounds and neutral retention."""

from sf2tool.h3.rng import _rng_step
from sf2tool.remake_h4.w2_checks import merge


def compare_service(submission, before, accepting, ordinal, checks):
    check, match, one, absent = checks.check, checks.match, checks.one, checks.absent
    events = submission.get("observations") or []
    expected_kinds = ["rng-text-w2", "text-seed-copy", "text-w2-wait", "text-w2-input"] + (
        ["text-w2-accepted"] if accepting else []
    )
    positions = []
    found = {}
    for kind in expected_kinds:
        matches = [(i, e) for i, e in enumerate(events) if e.get("Kind") == kind]
        e = one(kind, [e for _, e in matches], ordinal)
        found[kind] = e
        if matches:
            positions.append(matches[0][0])
    check("draw copy wait read accept order", positions == sorted(positions), ordinal)
    check(
        "draw copy wait precede service work",
        None if len(positions) < len(expected_kinds) else positions[:3] == [0, 1, 2],
        ordinal,
    )
    if accepting:
        check(
            "read accepts before caller resumes",
            None
            if len(positions) < 5
            else positions[4] == positions[3] + 1
            and not any(e.get("Kind") == "program-instruction" for e in events[: positions[4]]),
            ordinal,
        )
    else:
        check(
            "neutral keeps caller suspended",
            not any(e.get("Kind") == "program-instruction" for e in events),
            ordinal,
        )
    check(
        "masked accepting versus neutral read",
        match("accept" if accepting else "none", found["text-w2-input"].get("Detail", absent)),
        ordinal,
    )
    check(
        "neutral does not accept",
        accepting or not any(e.get("Kind") == "text-w2-accepted" for e in events),
        ordinal,
    )
    draw = found["rng-text-w2"]
    check(
        "draw uses input main seed and range",
        match(dict(Before=before.get("mainSeed", absent), RandomRange=256), draw),
        ordinal,
    )
    if draw.get("Before") is not None:
        word, value = _rng_step(int(draw["Before"]) >> 16, 512)
        check(
            "independent range256 main draw",
            match(
                dict(After=(word << 16) | (int(draw["Before"]) & 65535), RandomValue=value >> 1),
                draw,
            ),
            ordinal,
        )
    check(
        "copy precedes service",
        match(draw.get("RandomValue", absent), found["text-seed-copy"].get("After", absent)),
        ordinal,
    )
    sequences = [e.get("Sequence") for e in events]
    event_revisions = [e.get("Revision") for e in events]
    available_revisions = [r for r in event_revisions if r is not None]
    check(
        "event revisions inside Submit progression",
        merge(
            [
                None if len(available_revisions) != len(event_revisions) else True,
                available_revisions == sorted(available_revisions),
                *[
                    None
                    if before.get("revision") is None or submission.get("revision") is None
                    else before["revision"] < r <= submission["revision"]
                    for r in available_revisions
                ],
            ]
        ),
        ordinal,
    )
    check(
        "event sequence inside Submit",
        None
        if any(s is None for s in sequences)
        or before.get("observationSequence") is None
        or submission.get("observationSequence") is None
        else sequences == sorted(set(sequences))
        and all(
            before["observationSequence"] < s <= submission["observationSequence"]
            for s in sequences
        ),
        ordinal,
    )
    return events, found


def compare_neutral(actual, revision, token, cursor, after, ordinal, checks):
    check, match = checks.check, checks.match
    check(
        "neutral has no validation playback",
        not any(
            a.get("receipt", {}).get("Revision") == revision
            and a.get("receipt", {}).get("Command") == 67
            and a.get("receipt", {}).get("Operation") == "started"
            for a in actual.get("audioReceipts", [])
        ),
        ordinal,
    )
    check(
        "neutral retains token and input eligibility",
        match(dict(token=token, canWaitForText=True, cursor=cursor), after),
        ordinal,
    )
