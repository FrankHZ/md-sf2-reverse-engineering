"""Source warp requests, setup selection and destination release ordering."""

from sf2tool.remake_h4.operation_flow_indexes import entity
from sf2tool.remake_h4.operation_flow_initialization import operation_flow_initialization


def operation_flow_warps(
    actual,
    ordered,
    warp_records,
    records,
    states,
    state_of,
    anchor,
    instruction,
    executed,
    source_root,
    compiler,
    maps,
    routes,
    tracked,
    flags_at,
    names,
    result,
    check,
    _occurrence_map,
    _bounded_list,
    _bounded_sorted,
    _encode_source,
    _decode_source_table,
    _tokens,
):
    warp_requests = _occurrence_map()
    starts = _bounded_list(e for e in ordered if e["Kind"] == "warp-started")
    for n, start in enumerate(starts):
        seq = start["Sequence"]
        stop = starts[n + 1]["Sequence"] if n + 1 < len(starts) else float("inf")
        region = _bounded_list(e for e in ordered if seq < e["Sequence"] < stop)
        battle = next((e for e in region if e["Kind"] == "battle-selected"), None)
        source_map = warp_records[records[seq]].get("state", {}).get("map")
        held = next(
            (
                x
                for x in states
                if x[0] >= seq
                and state_of(x).get("map") == source_map
                and (state_of(x).get("fade") or {}).get("Purpose") == (2 if battle else 0)
                and entity(state_of(x), "entity-0")
            ),
            None,
        )
        if held is None:
            held = anchor(seq, "entities") if battle else None
        player = entity(state_of(held), "entity-0") if held else None
        request = None
        if player and source_map in maps:
            path = f"disasm/data/maps/entries/map{int(source_map[4:]):02d}/6-warp-events.asm"
            try:
                data, count, tail = _encode_source(
                    source_root / path, "warpEvents", compiler.equates
                )
                rows = _decode_source_table("warpEvents", data, count, tail)
                tx, ty = player["targetX"] // 384, player["targetY"] // 384
                source_row = next(
                    (
                        row
                        for row in rows
                        if row["trigger"]["x"] in (255, tx) and row["trigger"]["y"] in (255, ty)
                    ),
                    None,
                )
                if source_row:
                    destination = (
                        int(source_map[4:])
                        if source_row["targetMap"] == 255
                        else source_row["targetMap"]
                    )
                    request = dict(
                        map="map-" + str(destination),
                        position=source_row["destination"],
                        facing=source_row["facing"],
                        loadMode="preserve" if source_row["targetMap"] == 255 else "rebuild",
                    )
                    check(
                        names[3],
                        "source warp mode",
                        source_row["scrollMode"] == 0 and not source_row["retainsCoordinates"],
                        seq,
                    )
                    selected = next(
                        (
                            row
                            for row in maps[source_map]["events"]
                            if row["kind"] == "warp"
                            and (row["x"] is None or row["x"] == tx)
                            and (row["y"] is None or row["y"] == ty)
                        ),
                        None,
                    )
                    check(
                        names[3],
                        "source first-match request",
                        {k: selected.get(k) for k in request} == request if selected else None,
                        seq,
                    )
            except (KeyError, OSError, ValueError):
                check(names[3], "source warp table absent", None, seq)
        check(names[3], "held requested cell", True if request else None, seq)
        result["warps"].append(
            dict(sequence=seq, request=request, kind="battle-entry" if battle else "ordinary")
        )
        if not request:
            continue
        if battle:
            check(
                names[3],
                "direct battle entry selection",
                battle["Detail"] == maps[request["map"]]["battle"]["encounter"],
                seq,
            )
            continue
        transfer = next((e for e in region if e["Kind"] == "map-transferred"), None)
        check(names[3], "ordinary transfer occurrence", True if transfer else None, seq)
        if transfer is None:
            continue
        warp_requests[transfer["Sequence"]] = request
        check(names[3], "requested destination map", transfer.get("Detail") == request["map"], seq)
        service = [
            e
            for e in region
            if e["Sequence"] < transfer["Sequence"] and e["Kind"] == "map-load-service"
        ]
        check(names[3], "two disabled load services", len(service) == 2, seq)
        target = maps[request["map"]]
        flags = flags_at(transfer["Sequence"] - 1)
        route = routes.get(request["map"])
        expected_setup = (
            None
            if route is None
            else dict(
                default=route["defaultPointer"].lower().replace("_", "-"),
                variants=[
                    dict(flag=v["flag"], setup=v["pointer"].lower().replace("_", "-"))
                    for v in route["flagVariants"]
                ],
            )
        )
        check(names[0], "source ordered setup route", target.get("setup") == expected_setup, seq)
        selected_setup = expected_setup["default"] if expected_setup else None
        if expected_setup and flags is not None:
            for variant in expected_setup["variants"]:
                if variant["flag"] in flags:
                    selected_setup = variant["setup"]
        for family in (names[0], names[3]):
            check(
                family,
                "entry flags select admitted source setup",
                None
                if flags is None
                else expected_setup is None or selected_setup == expected_setup["default"],
                seq,
            )
        post = next(
            (
                x
                for x in states
                if x[0] >= transfer["Sequence"]
                and x[0] < stop
                and state_of(x).get("map") == request["map"]
                and entity(state_of(x), "entity-0")
            ),
            None,
        )
        check(names[3], "destination physical initialization held", True if post else None, seq)
        if post:
            operation_flow_initialization(
                post,
                request,
                transfer,
                stop,
                target,
                route,
                flags,
                ordered,
                states,
                state_of,
                anchor,
                instruction,
                compiler,
                source_root,
                tracked,
                names,
                check,
                seq,
                _bounded_list,
                _bounded_sorted,
                _tokens,
            )
        on_load = target.get("onLoad")
        first_instruction = next(
            (e for e, _ in executed if e["Sequence"] > transfer["Sequence"]), None
        )
        if on_load:
            check(
                names[3],
                "selected setup initialization starts at source caller",
                None
                if first_instruction is None
                else first_instruction["Program"]
                == dict(Program=on_load["program"], Instruction=on_load["instruction"]),
                seq,
            )
        ready = next(
            (
                x
                for x in states
                if transfer["Sequence"] <= x[0] < stop
                and state_of(x).get("map") == request["map"]
                and state_of(x).get("canWaitAtInput") is True
            ),
            None,
        )
        check(
            names[3], "field release after destination initialization", True if ready else None, seq
        )
        if ready:
            check(
                names[3],
                "destination field control has no pending caller or wait",
                all(
                    key in state_of(ready)
                    for key in ("cursor", "wait", "callers", "callerReturning")
                )
                and state_of(ready)["cursor"] is None
                and state_of(ready)["wait"] is None
                and state_of(ready)["callers"] == []
                and not state_of(ready)["callerReturning"],
                seq,
            )
    return warp_requests
