"""Native event/state references and occurrence-local state lookups."""

from bisect import bisect_left, bisect_right


def entity(s, identity):
    return next((x for x in s.get("entities") or [] if x.get("id") == identity), None)


def signature(s):
    return [(x["id"], x["slot"], x["sprite"]) for x in s.get("entities") or []]


def event_index(actual, common, _occurrence_map, _bounded_list, _bounded_sorted):
    events, records = _occurrence_map(), _occurrence_map()
    warp_records = actual.get("warpRecords", [])
    for ordinal, row in enumerate(warp_records):
        for e in row["result"].get("observations", []):
            seq = e["Sequence"]
            if seq in events and events[seq] != e:
                common("logical occurrence identity", False)
            events[seq] = e
            records.setdefault(seq, ordinal)
    ordered = _bounded_list(events[k] for k in _bounded_sorted(events))
    return events, records, warp_records, ordered


def state_index(actual, _bounded_list):
    states = _bounded_list()
    for channel in ("samples", "consumerBoundaries", "warpRecords"):
        for index, row in enumerate(actual.get(channel, [])):
            s = row.get("state", {})
            if s.get("observationSequence") is not None:
                states.append((s["observationSequence"], channel, index))
    states.sort(key=lambda item: item[0])
    state_sequences = _bounded_list(item[0] for item in states)

    def state_of(reference):
        # Sort only native sequence/channel/ordinal, never copies of world snapshots.
        return actual[reference[1]][reference[2]]["state"]

    def anchor(seq, field, before=True):
        positions = (
            range(bisect_right(state_sequences, seq) - 1, -1, -1)
            if before
            else range(bisect_left(state_sequences, seq), len(states))
        )
        return next((states[i] for i in positions if field in state_of(states[i])), None)

    return states, state_of, anchor
