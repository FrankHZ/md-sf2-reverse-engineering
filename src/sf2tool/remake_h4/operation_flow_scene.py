"""Post-load logical service, entity replacement and explicit camera detach."""

from sf2tool.remake_h4.operation_flow_indexes import signature


def operation_flow_scene(
    ins,
    e,
    seq,
    pid,
    body_events,
    ordered,
    warp_records,
    records,
    states,
    state_of,
    anchor,
    names,
    check,
    _bounded_list,
):
    if ins and ins["op"] == "scene-map":
        loaded_entities = next(
            (
                x
                for x, i in body_events
                if x["Sequence"] > seq and i and i["op"] == "scene-entities"
            ),
            None,
        )
        wait = next(
            (
                x
                for x, i in body_events
                if x["Sequence"] > seq
                and i
                and i["op"] == "wait-ticks"
                and x["Program"]["Instruction"] == e["Program"]["Instruction"] + 1
            ),
            None,
        )
        services = _bounded_list(
            x
            for x in ordered
            if loaded_entities
            and seq < x["Sequence"] < loaded_entities["Sequence"]
            and x["Kind"] == "simulation-tick"
        )
        service = services[0] if services else None
        check(
            names[4],
            "distinct post-load service before entity replacement",
            None
            if not loaded_entities or not wait or not service
            else len(services) == 1
            and seq < wait["Sequence"] < service["Sequence"] < loaded_entities["Sequence"],
        )
        if wait and loaded_entities:
            entry = warp_records[records[wait["Sequence"]]].get("state", {})
            if "entities" not in entry:
                entry = next(
                    (
                        state_of(x)
                        for x in states
                        if x[0] == wait["Sequence"]
                        and "entities" in state_of(x)
                        and "entitiesRunning" in state_of(x)
                    ),
                    entry,
                )
            replacement = warp_records[records[loaded_entities["Sequence"]]].get("state", {})
            old = anchor(seq - 1, "entities")
            check(
                names[4],
                "post-load wait retains old physical set with enabled services",
                None
                if not entry.get("entities") or old is None
                else signature(entry) == signature(state_of(old))
                and entry.get("entitiesRunning") is True
                and entry.get("wait") == "TickWait"
                and entry.get("canWaitAtInput") is False,
                seq,
            )
            check(
                names[4],
                "one real post-load logical service",
                None
                if entry.get("simulationTick") is None or replacement.get("simulationTick") is None
                else replacement["simulationTick"] == entry["simulationTick"] + 1,
                seq,
            )
    if ins and ins["op"] == "camera-entity" and ins["entity"] is None:
        held = warp_records[records[seq]].get("state", {})
        observed = (
            None
            if held.get("logicalView") is None or "TargetSlot" not in held["logicalView"]
            else held["logicalView"].get("TargetSlot") is None
        )
        if observed is None and pid == "bbcs-01":
            # The returning battle view needs the logical channel; the earlier
            # mounted field already exposes its actual bound target on draw.
            stop = next(
                (
                    x["Sequence"]
                    for x, i in body_events
                    if x["Sequence"] > seq
                    and i
                    and i["op"] in ("scene-map", "camera-entity", "camera-target")
                ),
                float("inf"),
            )
            projection = next(
                (
                    state_of(x)["cameraProjection"]
                    for x in states
                    if seq <= x[0] < stop
                    and state_of(x).get("cameraProjection") is not None
                    and "targetSlot" in state_of(x)["cameraProjection"]
                    and state_of(x)["cameraProjection"].get("observationSequence", -1) >= seq
                ),
                None,
            )
            if projection:
                observed = projection["targetSlot"] is None
        check(
            names[4],
            "actual pre-fade detach",
            observed,
            seq,
        )
