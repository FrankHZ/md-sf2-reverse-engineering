"""Last living battle pose, explicit outcome transfer and enclosing return."""

from sf2tool.remake_h4.operation_flow_indexes import entity
from sf2tool.remake_h4.operation_flow_tail import operation_flow_tail


def operation_flow_outcome(
    actual,
    ordered,
    maps,
    warp_requests,
    states,
    state_of,
    motion,
    flags_at,
    join_effect,
    write_flags,
    names,
    result,
    check,
    _bounded_list,
    _bounded_sorted,
):
    outcome_start = next((e for e in ordered if e["Kind"] == "outcome-program-started"), None)
    returned = next((e for e in ordered if e["Kind"] == "battle-returned"), None)
    battle_snapshots = _bounded_list(
        row["state"]
        for row in actual.get("warpRecords", [])
        if row["result"].get("mode") == "Battle"
        and row["result"].get("boundary") == "submit"
        and outcome_start
        and row["result"].get("observationSequence", float("inf")) < outcome_start["Sequence"]
    )
    last_battle = battle_snapshots[-1] if battle_snapshots else None
    transfers = _bounded_list(
        e for e in ordered if e["Kind"] == "map-transferred" and e["Sequence"] not in warp_requests
    )
    transfer = transfers[-1] if transfers else None
    battle_map = next(
        (
            m
            for m in maps.values()
            if m.get("battle") and m["battle"].get("encounter") == "battle-1"
        ),
        None,
    )
    first_living = (
        next(
            (
                p
                for p in _bounded_sorted(
                    last_battle.get("actors", []), key=lambda p: int(p["id"].split("-")[1])
                )
                if p["id"].startswith("ally-") and p.get("hp", 0) > 0
            ),
            None,
        )
        if last_battle
        and all(
            p.get("hp") is not None
            for p in last_battle.get("actors", [])
            if p["id"].startswith("ally-")
        )
        else None
    )
    destination = (
        None
        if first_living is None
        or battle_map is None
        or first_living.get("x") is None
        or first_living.get("y") is None
        else dict(
            map=battle_map["id"],
            position=dict(x=first_living["x"], y=first_living["y"]),
            facing=battle_map["battle"]["outcome"]["victoryFacing"],
        )
    )
    result["warps"].append(
        dict(
            kind="explicit-outcome-return",
            sequence=transfer["Sequence"] if transfer else None,
            request=destination,
        )
    )
    for family in (names[3], names[4]):
        check(
            family,
            "source victory outcome kind",
            None if outcome_start is None else outcome_start["Detail"] == "Victory",
        )
        check(
            family,
            "source outcome destination map",
            None
            if transfer is None or battle_map is None
            else transfer["Detail"] == battle_map["id"],
        )
        check(
            family,
            "outcome transfer independently bound to last living battle pose",
            None
            if transfer is None or destination is None
            else transfer["Detail"] == destination["map"],
        )
    if transfer and destination:
        held = next(
            (x for x in states if x[0] >= transfer["Sequence"] and entity(state_of(x), "entity-0")),
            None,
        )
        player = entity(state_of(held), "entity-0") if held else None
        for family in (names[3], names[4]):
            check(
                family,
                "source outcome return position/facing effect",
                None
                if player is None
                else player["x"] == destination["position"]["x"] * 384
                and player["y"] == destination["position"]["y"] * 384
                and player["facing"] == destination["facing"],
                transfer["Sequence"],
            )
        fades = [
            item
            for item in motion.get("occurrences", [])
            if (item.get("location") or {}).get("Program") == "source-outcome-return"
        ]
        check(
            names[4],
            "source outcome helper/transfer/init/visible return pairing",
            None
            if len(fades) != 2 or returned is None
            else fades[0]["token"] < transfer["Sequence"] < fades[1]["token"] < returned["Sequence"]
            and fades[0]["operation"] is True
            and fades[1]["operation"] is True,
        )
        ready = (
            next(
                (
                    x
                    for x in states
                    if x[0] >= returned["Sequence"] and state_of(x).get("canWaitAtInput") is True
                ),
                None,
            )
            if returned
            else None
        )
        check(
            names[3],
            "outcome visible field readiness after enclosing return",
            True if ready else None,
        )
    if battle_map and outcome_start:
        operation_flow_tail(
            battle_map,
            outcome_start,
            ordered,
            returned,
            states,
            state_of,
            flags_at,
            join_effect,
            write_flags,
            names,
            check,
        )
