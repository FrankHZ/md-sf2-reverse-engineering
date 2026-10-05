"""Join controlled input readiness and transfer events to their case and region state."""


def compare_delivery(role, samples, map_id, check):
    if role != "house":
        starts = [s for s in samples if s.get("label") == "start"]
        ends = [s for s in samples if s.get("label") == "case-end"]
        check("controlled case boundaries", True if len(starts) == len(ends) == 1 else None, role)
        if starts and ends:
            case_events = ends[0].get("events", [])
            destinations = [
                e.get("Detail") for e in case_events if e.get("Kind") == "map-transferred"
            ]
            expected_destinations = {
                "school": ["map-3", "map-3"],
                "castle-walk": [],
                "castle-rebuild": ["map-20", "map-19"],
            }[role]
            check(
                "complete preserving/rebuild transfer lineage",
                False
                if len(destinations) > len(expected_destinations)
                or any(d not in expected_destinations for d in destinations)
                else None
                if len(destinations) < len(expected_destinations)
                else destinations == expected_destinations,
                role,
            )
            check(
                "no blocked local input",
                not any(e.get("Kind") == "movement-blocked" for e in case_events),
                role,
            )
            initial_state = starts[0].get("state", {})
            check("controlled initial map", initial_state.get("map") == map_id, role)
            check(
                "controlled start/end enclose samples",
                samples[0] is starts[0] and samples[-1] is ends[0],
                role,
            )

            def same_state(name, left, right, label):
                for field in (
                    "sessionId",
                    "map",
                    "revision",
                    "observationSequence",
                    "simulationTick",
                    "flags",
                    "player",
                    "logicalView",
                ):
                    check(
                        name + " " + field,
                        None
                        if field not in left or field not in right
                        else left[field] == right[field],
                        label,
                    )

            ready, previous_state = initial_state, initial_state
            input_intervals = []
            for s in samples:
                state = s.get("state")
                label = s.get("label")
                if not state:
                    continue
                check(
                    "observation belongs to controlled start",
                    None
                    if "sessionId" not in state or "sessionId" not in initial_state
                    else state["sessionId"] == initial_state["sessionId"],
                    label,
                )
                for axis in ("revision", "observationSequence", "simulationTick"):
                    check(
                        "ordered controlled observation " + axis,
                        None
                        if axis not in state or axis not in previous_state
                        else 0 <= previous_state[axis] <= state[axis],
                        label,
                    )
                if "layout" in s:
                    same_state("layout follows input-ready state", state, ready, label)
                elif label == "ordinary-input":
                    before = s.get("before", {})
                    same_state("input starts at previous ready state", before, ready, label)
                    input_intervals.append((before, state))
                    for axis in ("revision", "observationSequence"):
                        check(
                            "ordinary input advances " + axis,
                            None
                            if axis not in state or axis not in before
                            else 0 <= before[axis] < state[axis],
                            label,
                        )
                if (
                    "layout" not in s
                    and state.get("stop") == "PlayerInput"
                    and state.get("wait") is None
                ):
                    ready = state
                previous_state = state
                if state and s.get("label") == "ordinary-input":
                    check(
                        "ordinary input state continuity",
                        state.get("sessionId")
                        == initial_state.get("sessionId")
                        == s.get("before", {}).get("sessionId"),
                        role,
                    )
                    check(
                        "ordinary input reached readiness",
                        state.get("failure") is None
                        and state.get("stop") == "PlayerInput"
                        and state.get("wait") is None,
                        role,
                    )
            # Event records have no separate session field. Join their two
            # monotonic axes to this case's ordinary-input intervals and the
            # ready state consumed by each preserve/rebuild witness.
            for event_axis, state_axis in (
                ("Revision", "revision"),
                ("Sequence", "observationSequence"),
            ):
                previous = initial_state.get(state_axis)
                end = ready.get(state_axis)
                for event in case_events:
                    value = event.get(event_axis)
                    check(
                        "ordered bounded case event " + event_axis,
                        None
                        if value is None or previous is None or end is None
                        else 0 <= previous < value <= end,
                        role,
                    )
                    if value is not None:
                        previous = value
            transfers = [e for e in case_events if e.get("Kind") == "map-transferred"]
            anchors = {
                "school": ["school-preserved-away", "school-preserved-return"],
                "castle-walk": [],
                "castle-rebuild": [None, "castle-rebuilt-inside-32"],
            }[role]
            used_intervals = set()
            for transfer, anchor in zip(transfers, anchors, strict=False):
                try:
                    matches = [
                        (i, before, after)
                        for i, (before, after) in enumerate(input_intervals)
                        if before["revision"] < transfer["Revision"] <= after["revision"]
                        and before["observationSequence"]
                        < transfer["Sequence"]
                        <= after["observationSequence"]
                    ]
                    check("transfer belongs to one ordinary input", len(matches) == 1, role)
                    if len(matches) != 1:
                        continue
                    index, before, after = matches[0]
                    check("distinct transfer input intervals", index not in used_intervals, role)
                    used_intervals.add(index)
                    check(
                        "transfer destination reaches input state",
                        transfer.get("Detail") == after.get("map"),
                        role,
                    )
                    warp_starts = [
                        e
                        for e in case_events
                        if e.get("Kind") == "warp-started"
                        and before["revision"] < e["Revision"] < transfer["Revision"]
                        and before["observationSequence"] < e["Sequence"] < transfer["Sequence"]
                    ]
                    check(
                        "transfer follows ordinary warp start",
                        True if len(warp_starts) == 1 else None if not warp_starts else False,
                        role,
                    )
                    if anchor is not None:
                        witness = next((s for s in samples if s.get("label") == anchor), {})
                        same_state(
                            "transfer reaches named region state",
                            after,
                            witness.get("state", {}),
                            anchor,
                        )
                except KeyError:
                    check("transfer interval operands", None, role)
            for i, (before, after) in enumerate(input_intervals):
                if before.get("map") != after.get("map"):
                    check(
                        "map-changing input has transfer",
                        True if i in used_intervals else None,
                        role,
                    )
