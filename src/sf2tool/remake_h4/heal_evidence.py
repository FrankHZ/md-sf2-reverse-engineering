"""Selected HEAL rows, event envelopes and post-Present projection identity."""

from sf2tool.remake_h4.heal_checks import match, merge, missing


def select_channels(actual, context, checks):
    """Select original indices without materializing a full capture channel."""
    check = checks.check

    def selected(channel, key):
        records = actual.get(channel) or []
        if not records or "_index" in records[0]:
            return records
        # Full captures use existing indexed channels; do not materialize them.
        return [
            dict(records[int(i)], _index=int(i))
            for i in context.get(key, [])
            if 0 <= i < len(records)
        ]

    warps = selected("warpRecords", "warpIndices")
    expected_warps = context.get("warpIndices") or []
    warp_indices = [w.get("_index") for w in warps]
    check(
        "selected action resource inventory",
        merge(
            [
                True if expected_warps and warp_indices == expected_warps else None,
                not any(i is not None and i not in expected_warps for i in warp_indices),
                len(warp_indices) == len(set(warp_indices)),
            ]
        ),
    )
    scenes = selected("sceneObservations", "sceneIndices")
    inputs = selected("inputRecords", "inputIndices")
    return warps, scenes, inputs


def select_occurrence(warps, owner, ordinal, checks):
    check = checks.check
    begin, end = owner.get("preparedRevision"), owner.get("sceneEndResultRevision")
    rows = [
        w
        for w in warps
        if begin is not None
        and end is not None
        and begin <= (w.get("result") or {}).get("revision", -1) <= end
    ]
    expected_indices = owner.get("resultIndices") or []
    indices = [w.get("_index") for w in rows]
    inventory_complete = bool(expected_indices) and indices == expected_indices
    check(
        "actual Submit inventory",
        merge(
            [
                True if inventory_complete else None,
                not any(i is not None and i not in expected_indices for i in indices),
                len(indices) == len(set(indices)),
                [i for i in indices if i is not None]
                == sorted(i for i in indices if i is not None),
            ]
        ),
        ordinal,
    )
    by_revision = {}
    for w in rows:
        by_revision.setdefault(w["result"]["revision"], []).append(w)
    return rows, by_revision, inventory_complete


def scoped_events(row, end, end_sequence):
    return [
        e
        for e in row.get("result", {}).get("observations", [])
        if row.get("result", {}).get("revision") != end
        or e.get("Sequence") is None
        or e["Sequence"] <= end_sequence
    ]


def event_envelope(row, es, previous_revision, previous_sequence, ordinal, actor, target, checks):
    check = checks.check
    r = row.get("result") or {}
    revision = r.get("revision")

    seqs = [e.get("Sequence") for e in es]
    for e in es:
        sequence = e.get("Sequence")
        check(
            "event sequence inside Submit",
            merge(
                [
                    None if sequence is None else sequence > 0,
                    None
                    if sequence is None or r.get("observationSequence") is None
                    else sequence <= r["observationSequence"],
                    None
                    if sequence is None or previous_sequence is None
                    else previous_sequence < sequence,
                ]
            ),
            ordinal,
            revision,
        )
        check(
            "event belongs to Submit interval",
            merge(
                [
                    None if e.get("Revision") is None else e["Revision"] >= 0,
                    None
                    if e.get("Revision") is None or revision is None
                    else e["Revision"] <= revision,
                    None
                    if e.get("Revision") is None or previous_revision is None
                    else previous_revision < e["Revision"],
                ]
            ),
            ordinal,
            revision,
        )
        check(
            "event actor",
            match(
                dict(Value=target if e.get("Kind") == "hp" else actor),
                e.get("Actor", missing),
            ),
            ordinal,
            revision,
        )
    check(
        "event sequence order",
        None if any(x is None for x in seqs) else seqs == sorted(set(seqs)),
        ordinal,
        revision,
    )


class SceneProjections:
    """Post-Present index and identity joins for one occurrence; no stream ownership."""

    def __init__(self, scenes, owner, by_revision, session, ordinal, checks):
        begin, end = owner.get("preparedRevision"), owner.get("sceneEndResultRevision")
        projection = {}
        for s in scenes:
            if (
                begin is not None
                and end is not None
                and begin <= s.get("revision", -1) < end
                and s.get("projectionStage") == "host-poll"
            ):
                projection.setdefault(s["revision"], []).append(s)

        self.projection = projection
        self.by_revision = by_revision
        self.session = session
        self.ordinal = ordinal
        self.checks = checks

    def at(self, revision):
        projection, by_revision = self.projection, self.by_revision
        session, ordinal, check = self.session, self.ordinal, self.checks.check
        ss = projection.get(revision, [])
        check("actual post-Present scene", True if ss else None, ordinal, revision)
        if not ss:
            return {}
        first = ss[0]
        keys = (
            "phase",
            "waitToken",
            "actionKind",
            "healing",
            "spell",
            "reactionKind",
            "reactionAmount",
            "message",
        )
        for s in ss:
            check(
                "scene session and Submit",
                match(
                    dict(
                        sessionId=session,
                        revision=revision,
                        observationSequence=(by_revision.get(revision) or [{}])[0]
                        .get("result", {})
                        .get("observationSequence", missing),
                        scene=dict(actionKind="heal", spell=dict(Value="heal", Level=1)),
                    ),
                    s,
                ),
                ordinal,
                revision,
            )
            check(
                "repeated projection state agrees",
                match({k: first["scene"].get(k, missing) for k in keys}, s.get("scene", missing)),
                ordinal,
                revision,
            )
        return first.get("scene") or {}
