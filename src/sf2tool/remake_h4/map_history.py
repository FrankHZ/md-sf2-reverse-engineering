"""Admit retained applicability, ordered producers and covered source classes."""

from collections import Counter


def admit_history(actual, context, check):
    history = actual.get("mapHistory", context.get("mapHistory", []))
    receipt = actual.get("mapHistoryReceipt", context.get("mapHistoryReceipt", {}))
    session = context.get("historicalSession")
    sessions = {s.get("state", {}).get("sessionId") for s in actual.get("samples", [])} - {None}
    if actual.get("sessionId"):
        sessions.add(actual["sessionId"])
    sessions.update(actual.get("resourceScope", {}).get("contextSessions", []))
    check(
        "historical session boundary",
        None if not sessions or not session else sessions == {session},
    )
    check(
        "complete retained applicability selection",
        None
        if not receipt or session is None
        else receipt.get("failure") is None
        and receipt.get("sourceUnchanged") is True
        and receipt.get("sessions") == [session],
    )
    check("historical context present", True if history else None)
    return history, receipt, session


def collect_history(history, receipt, maps, check):
    # Retained change runs cover their declared selected records without inventing
    # values at omitted post-Submit fields. Missing ranges never imply no mutation.
    states, events = [], []
    for channel in ("samples", "warpRecords"):
        current, count, previous = {}, 0, -1
        for row in history:
            if row.get("channel") != channel:
                continue
            try:
                if row["kind"] == "run":
                    first, last = row["first"][0], row["last"][0]
                    check(
                        "ordered complete context runs",
                        first > previous and row["count"] == last - first + 1,
                        channel,
                    )
                    current = dict(current, **row["change"])
                    states.append((row["first"], current))
                    count += row["count"]
                    previous = last
                elif row["kind"] == "events" and channel == "warpRecords":
                    events.extend(row["events"])
            except KeyError:
                check("context operands", None, channel)
        check(
            "all selected context records accounted",
            None
            if channel not in receipt.get("relevant", {})
            else True
            if count == receipt["relevant"][channel]
            else False
            if count > receipt["relevant"][channel]
            else None,
            channel,
        )
    for _, state in states:
        if state.get("map") not in maps:
            continue
        flags = state.get("flags")
        if flags is not None:
            alternate = (
                {506, 543, 609} if state["map"] == "map-3" else {501, 506, 507, 543, 609, 982}
            )
            check(
                "default source setup applicability",
                not alternate.intersection(flags),
                state["map"],
            )
    retained_kinds = {
        "map-transferred",
        "door-opened",
        "warp-started",
        "warp-program",
        "full-fade-started",
        "full-fade-completed",
        "warp-visible",
        "zone-entered",
        "movement-started",
        "movement-blocked",
        "program-instruction",
    }
    event_counts = Counter(
        (e.get("Kind"), e.get("Detail") if e.get("Kind") == "program-instruction" else None)
        for e in events
    )
    expected_counts = {
        (e["kind"], e["detail"]): e["count"]
        for e in receipt.get("events", [])
        if e["kind"] in retained_kinds
    }
    check(
        "complete retained mutation and movement producers",
        None
        if not expected_counts
        else False
        if any(event_counts[k] > expected_counts.get(k, 0) for k in event_counts)
        else True
        if event_counts == expected_counts
        else None,
    )
    for axis in ("Revision", "Sequence"):
        values = [e.get(axis) for e in events]
        check(
            "ordered unique historical " + axis,
            None if None in values else values == sorted(set(values)),
        )
    return states, events


def classify_coverage(states, maps, coverage, check):
    # Only this accepted cohort's observed classes are admitted. Unvisited tables
    # are source/history Inferred base, never established by sparse draw absence.
    roof_hits, door_hits = set(), set()
    for _, state in states:
        m = state.get("map")
        if m not in maps or not state.get("cell") or None in state["cell"][:2]:
            continue
        cell = tuple(state["cell"][:2])
        for i, row in enumerate(maps[m]["roofs"]):
            if row["trigger"] == cell:
                roof_hits.add((m, i))
        for i, row in enumerate(maps[m]["doors"]):
            if row["trigger"] == cell:
                door_hits.add((m, i))
    known_roofs = {("map-3", 0), ("map-3", 9), *(("map-19", i) for i in range(6))}
    known_doors = {("map-3", 0), ("map-3", 5)}
    check("every reached roof class covered", None if roof_hits - known_roofs else True)
    check("every reached door class covered", None if door_hits - known_doors else True)
    for m, source in maps.items():
        for table in ("doors", "roofs", "flags"):
            for i, row in enumerate(source[table]):
                coverage.append(
                    dict(
                        map=m,
                        table=table,
                        ordinal=i + 1,
                        rect=row["rect"],
                        applicability="Inferred",
                        disposition="controlled-class"
                        if (m, i)
                        in (
                            known_doors
                            if table == "doors"
                            else known_roofs
                            if table == "roofs"
                            else {("map-3", 0), ("map-3", 1)}
                        )
                        else "unchanged source/history",
                    )
                )
