"""Admit the retained cohort and select only indexed input/result/state operands."""

from sf2tool.remake_h4.w1_checks import merge
from sf2tool.remake_h4.w1_source import _W1_COHORT


def selected(channel, positions, container):
    rows = container.get(channel, [])
    if not rows:
        return []
    if "_index" in rows[0]:
        return rows
    return [
        dict(rows[int(i)], _index=int(i))
        for i in sorted(set(positions))
        if isinstance(i, (int, float)) and 0 <= i < len(rows)
    ]


def rows_by_index(rows, label, checks):
    check = checks.check
    out = {}
    for row in rows:
        i = row.get("_index")
        check(label + " unique original index", i not in out)
        out[i] = row
    return out


def admit_context(actual, context, checks):
    check, match, absent = checks.check, checks.match, checks.absent
    supplement = context.get("actualSupplement") or {}
    for key in ("outcomeRecords", "w1NpcHistory", "w1NpcWorld", "w1OutcomeFinal"):
        if key in actual and key in supplement:
            check("duplicate actual supplement agrees: " + key, actual[key] == supplement[key])
    check(
        "independent selected scope", match("retained-keyboard-A-w1", context.get("scope", absent))
    )
    session = context.get("sessionId")
    check("independent session", True if isinstance(session, str) and session else None)
    owners, polls = context.get("occurrences", []), context.get("polls", [])
    check(
        "independent occurrence inventory",
        None
        if not owners
        else [
            (
                o.get("cursor", {}).get("Program"),
                o.get("cursor", {}).get("Instruction"),
                o.get("text"),
            )
            for o in owners
        ]
        == list(_W1_COHORT),
    )
    check(
        "selected poll inventory",
        None
        if not polls
        else len(polls) == 120 and sum(p.get("accepting") is True for p in polls) == 68,
    )
    check("unique occurrence tokens", len({o.get("token") for o in owners}) == len(owners))
    check(
        "unique poll edges",
        len({p.get("resultIndex") for p in polls}) == len(polls)
        and len({p.get("inputIndex") for p in polls}) == len(polls),
    )
    for owner in owners:
        check("occurrence session identity", match(session, owner.get("sessionId", absent)))
        owner_polls = [p for p in polls if p.get("token") == owner.get("token")]
        check(
            "one acceptance per selected token",
            None if not owner_polls else sum(p.get("accepting") is True for p in owner_polls) == 1,
        )
    return supplement, session, owners, polls


def select_records(actual, supplement, context, checks):
    check = checks.check
    indices = context.get("indices", {})
    inputs = rows_by_index(
        selected("inputRecords", indices.get("inputRecords", []), actual), "input", checks
    )
    records = rows_by_index(
        selected("warpRecords", indices.get("warpRecords", []), actual), "result", checks
    )
    samples = rows_by_index(
        selected("samples", indices.get("samples", []), actual), "sample", checks
    )
    outcomes = rows_by_index(
        selected(
            "outcomeRecords",
            context.get("outcomeIndices", []),
            actual if "outcomeRecords" in actual else supplement,
        ),
        "outcome",
        checks,
    )
    states = {("samples", i): r.get("state", {}) for i, r in samples.items()}
    states.update({("outcomeRecords", i): r.get("state", {}) for i, r in outcomes.items()})
    for channel, indexed_rows in (
        ("samples", samples),
        ("warpRecords", records),
        ("inputRecords", inputs),
    ):
        expected = set(indices.get(channel, []))
        check(
            channel + " selection coverage",
            merge(
                [
                    None if not expected else not (set(indexed_rows) - expected),
                    True if expected and expected <= set(indexed_rows) else None,
                ]
            ),
        )
    return inputs, records, states
