"""Occurrence identity, release and boundary indexes in caller-owned storage."""

from bisect import bisect_right


def motion_events(actual, common, _occurrence_map, _bounded_sorted, _group_rows):
    events, record_by_event = _occurrence_map(), _occurrence_map()
    warp_records = actual.get("warpRecords", [])
    for ordinal, r in enumerate(warp_records):
        for e in r["result"].get("observations", []):
            seq = e["Sequence"]
            if seq in events and events[seq] != e:
                common("logical occurrence identity contradiction", False)
            events[seq] = e
            record_by_event.setdefault(seq, ordinal)
    ordered = _bounded_sorted(events.values(), key=lambda e: e["Sequence"])
    events_by_kind = {}
    release_by_token = _occurrence_map()
    for event in ordered:
        _group_rows(events_by_kind, event["Kind"]).append(event)
        release_token = (event.get("EntityWaitRelease") or {}).get("Token", {}).get("Value")
        if release_token is not None:
            release_by_token.setdefault(release_token, event)
    boundaries = actual.get("consumerBoundaries", [])
    by_token = _occurrence_map()
    release_boundary = _occurrence_map()
    for b in boundaries:
        s = b.get("state", {})
        by_token.setdefault(s.get("token"), []).append(b)
        for release in b.get("releases", []):
            if release.get("Sequence") is not None:
                release_boundary.setdefault(release["Sequence"], b)

    def following(token, kind):
        group = events_by_kind.get(kind, [])
        position = bisect_right(group, token, key=lambda event: event["Sequence"])
        return group[position] if position < len(group) else None

    return (
        warp_records,
        record_by_event,
        ordered,
        release_by_token,
        by_token,
        release_boundary,
        following,
    )
