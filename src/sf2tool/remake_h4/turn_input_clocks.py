"""Compare causal input/result spans and both clock axes including selection gaps."""

from bisect import bisect_right

from sf2tool.remake_h4.turn_consumer_checks import merge, number


def compare_input_clocks(channels, indices, context, session, checks):
    check, match, absent = checks.check, checks.match, checks.absent
    clocks, precedes, join = checks.clocks, checks.precedes, checks.join
    warps = {index: row for index, row in channels["warpRecords"].items() if row}
    warp_keys = sorted(warps)
    input_rows = [
        (index, row)
        for index, row in channels["inputRecords"].items()
        if row or index in (indices.get("inputRecords") or [])
    ]
    previous_input = None
    valid_inputs = []
    for index, row in input_rows:
        before, after = row.get("before") or {}, row.get("after") or {}
        for side, snapshot in (("before", before), ("after", after)):
            clocks("input " + side, snapshot, index)
            check(
                "input " + side + " session",
                match(session, snapshot.get("sessionId", absent)),
                index=index,
            )
        precedes("input snapshots progress", before, after, index)
        clocks("input source span", row, index, ("resultStart", "resultEnd"))
        start, end = row.get("resultStart"), row.get("resultEnd")
        span = start <= end if number(start) and number(end) else None
        check("ordered input source span", span, index=index)
        if previous_input is not None:
            precedes(
                "physical input clocks progress", previous_input.get("after") or {}, before, index
            )
            prev_end = previous_input.get("resultEnd")
            check(
                "physical input spans do not overlap",
                start >= prev_end if number(start) and number(prev_end) else None,
                index=index,
            )
        previous_input = row
        if span is not True:
            continue
        valid_inputs.append((index, row))
        if start == end:
            check(
                "empty input span preserves snapshot",
                merge(
                    [match(before, after)]
                    + [
                        before[k] == after[k]
                        for k in ("actor", "stage", "map")
                        if k in before and k in after
                    ]
                ),
                index=index,
            )
        for side, snapshot, anchor in (("before", before, start - 1), ("after", after, end - 1)):
            pos = bisect_right(warp_keys, anchor) - 1
            if anchor in warps:
                join(
                    "input " + side + " joins selected result boundary",
                    snapshot,
                    warps[anchor].get("result") or {},
                    index,
                )
                state = warps[anchor].get("state") or {}
                if state:
                    join(
                        "input " + side + " joins delivered state boundary", snapshot, state, index
                    )
                    check(
                        "input " + side + " shared state leaves",
                        merge(
                            [match({k: snapshot[k] for k in snapshot.keys() & state.keys()}, state)]
                            + [
                                snapshot[k] == state[k]
                                for k in ("actor", "stage", "map")
                                if k in snapshot and k in state
                            ]
                        ),
                        index=index,
                    )
            else:
                # Selection may omit the direct result. Available neighbors still
                # bound its source position on BOTH axes; they cannot replace it.
                if pos >= 0:
                    precedes(
                        "input boundary follows preceding selected result",
                        warps[warp_keys[pos]].get("result") or {},
                        snapshot,
                        index,
                    )
                if pos + 1 < len(warp_keys):
                    precedes(
                        "input boundary precedes next selected result",
                        snapshot,
                        warps[warp_keys[pos + 1]].get("result") or {},
                        index,
                    )
    census = context.get("census") or []
    first_owner = min((c[0] for c in census), default=None)
    last_owner = max((c[0] for c in census), default=None)
    input_starts = [row["resultStart"] for _, row in valid_inputs]
    ordered_starts = input_starts == sorted(input_starts)
    valid_input_indices = {n for n, _ in valid_inputs}
    missing_inputs = [i for i in indices.get("inputRecords") or [] if i not in valid_input_indices]
    for index, row in warps.items():
        if first_owner is None or not first_owner <= index <= last_owner:
            continue
        pos = bisect_right(input_starts, index) - 1 if ordered_starts else -1
        check("selected result has causal input", True if pos >= 0 else None, index=index)
        if pos < 0:
            continue
        input_index, owner = valid_inputs[pos]
        next_input = valid_inputs[pos + 1] if pos + 1 < len(valid_inputs) else None
        next_index = (
            next_input[0] if next_input else max(indices.get("inputRecords") or [input_index]) + 1
        )
        ambiguous = (
            next_index > input_index + 1
            or any(input_index < i < next_index for i in missing_inputs)
            or next_input is None
            and number(row.get("inputOrdinal"))
            and number(owner.get("ordinal"))
            and row["inputOrdinal"] > owner["ordinal"]
        )
        check(
            "selected result belongs to causal input ordinal",
            None
            if ambiguous
            else match(owner.get("ordinal", absent), row.get("inputOrdinal", absent)),
            index=index,
        )
        body = row.get("result") or {}
        precedes("causal input before precedes result", owner.get("before") or {}, body, index)
        direct = index < owner["resultEnd"]
        precedes(
            "direct result within input span" if direct else "automatic result follows input after",
            body if direct else owner.get("after") or {},
            owner.get("after") or {} if direct else body,
            index,
        )
        if "inputDelivery" in row:
            check(
                "result delivery agrees with direct input span",
                None if ambiguous else match(direct, row["inputDelivery"]),
                index=index,
            )
        if next_input:
            precedes(
                "result precedes next causal input", body, next_input[1].get("before") or {}, index
            )
    return warps, census
