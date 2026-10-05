"""Admit the independent W2 cohort and index only retained or selected evidence."""

from sf2tool.remake_h4.w2_checks import merge
from sf2tool.remake_h4.w2_source import _W2_COHORT


def index(records, key):
    groups = {}
    for row in records:
        groups.setdefault(key(row), []).append(row)
    return groups


def selected_rows(actual, channel, positions):
    channel_rows = actual.get(channel, [])
    if not channel_rows:
        return []
    # Retained selections already preserve original indices. Full captures
    # support indexed reads: never materialize their complete sample/results.
    if "_index" in channel_rows[0]:
        return channel_rows
    return [
        dict(channel_rows[int(i)], _index=int(i))
        for i in sorted(i for i in positions if i is not None)
        if 0 <= i < len(channel_rows)
    ]


def admit_context(context, checks):
    check, match = checks.check, checks.match
    context = context or {}
    check("explicit accepted cohort", match("retained-keyboard-A-w2", context.get("scope")))
    session = context.get("sessionId")
    check("independent session", True if isinstance(session, str) and session else None)
    inventory = context.get("occurrences") or []
    expected_inventory = list(_W2_COHORT)
    observed_inventory = [
        (o.get("program"), o.get("instruction"), o.get("text")) for o in inventory
    ]
    check(
        "accepted source occurrence inventory",
        None if not inventory else observed_inventory == expected_inventory,
    )
    check(
        "unique occurrence identity",
        len({(o.get("ordinal"), o.get("token")) for o in inventory}) == len(inventory),
    )
    return context, session, inventory


def select_evidence(actual, context, inventory, checks):
    check, one = checks.check, checks.one
    neutrals = context.get("neutral") or []
    selected_polls = inventory + neutrals

    input_rows = selected_rows(
        actual, "inputRecords", {o.get("inputIndex") for o in selected_polls}
    )
    inputs = index(input_rows, lambda i: i.get("ordinal"))
    sample_positions = {i for o in selected_polls for i in o.get("readyIndices", [])}
    sample_positions.update(o.get("indicatorIndex") for o in inventory)
    samples = selected_rows(actual, "samples", sample_positions)
    sample_revisions = index(samples, lambda s: (s.get("state") or {}).get("revision"))
    sample_indices = index(samples, lambda s: s["_index"])
    records = [
        (r["_index"], r)
        for r in selected_rows(
            actual, "warpRecords", {o.get("resultIndex") for o in selected_polls}
        )
    ]
    receipts = index(
        actual.get("audioReceipts", []), lambda a: (a.get("receipt") or {}).get("Sequence")
    )
    check(
        "retained neutral inventory",
        None
        if not neutrals
        else len(neutrals) == 14 and len({n.get("ordinal") for n in neutrals}) == 14,
    )
    polls = [(o, o, True) for o in inventory]
    for n in neutrals:
        parents = [o for o in inventory if o.get("token") == n.get("token")]
        parent = one("neutral belongs to one source occurrence", parents, n.get("ordinal"))
        polls.append((n, parent, False))
    poll_ordinals = [p.get("ordinal") for p, _, _ in polls]
    check("distinct selected polls", len(set(poll_ordinals)) == len(poll_ordinals))
    reached = [
        r.get("inputOrdinal")
        for _, r in records
        if any(
            e.get("Kind") == "text-w2-input"
            for e in (r.get("result") or {}).get("observations", [])
        )
    ]
    check(
        "candidate poll inventory",
        merge(
            [
                not (set(reached) - set(poll_ordinals)) if polls else None,
                len(reached) == len(set(reached)),
                True if set(reached) == set(poll_ordinals) and polls else None,
            ]
        ),
    )

    return inputs, sample_revisions, sample_indices, records, receipts, neutrals, polls
