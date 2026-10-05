"""Reached before/after physical writes and full party HP/MP restoration."""

from sf2tool.remake_h4.operation_flow_indexes import entity
from sf2tool.remake_h4.operation_flow_scene import operation_flow_scene


def operation_flow_effects(
    actual,
    executed,
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
    for pid in ("bbcs-01", "abcs-battle01"):
        body_events = _bounded_list(
            ((e, ins) for e, ins in executed if e["Program"]["Program"] == pid)
        )
        check(names[4], "before/after body reached " + pid, True if body_events else None)
        for e, ins in body_events:
            seq = e["Sequence"]
            if ins and ins["op"] in ("position", "face", "sprite", "hide"):
                held = anchor(seq, "entities", False)
                actor = entity(state_of(held), ins["entity"]) if held else None
                expected_effect = dict(ins)
                for other, effect in body_events:
                    if (
                        held
                        and seq < other["Sequence"] <= held[0]
                        and effect
                        and effect.get("entity") == ins["entity"]
                        and effect["op"] == ins["op"]
                    ):
                        expected_effect = effect
                value = None
                if actor:
                    if ins["op"] == "position":
                        value = (
                            actor["x"] == expected_effect["position"]["x"] * 384
                            and actor["y"] == expected_effect["position"]["y"] * 384
                        )
                    elif ins["op"] == "face":
                        value = actor["facing"] == expected_effect["facing"]
                    elif ins["op"] == "sprite":
                        value = actor["sprite"] == expected_effect["sprite"]
                    else:
                        value = actor["Visible"] is False
                check(names[4], "source physical effect " + ins["op"], value, seq)
            if ins and ins["op"] == "reset-party-battle-stats":
                held = anchor(seq, "party", False)
                party = state_of(held)["party"] if held else None
                value = None
                if party:
                    allies = [p for p in party if p["Actor"]["Value"].startswith("ally-")]
                    admitted = (
                        actual.get("admissionSnapshot", {})
                        .get("state", {})
                        .get("admittedParty", {})
                    )
                    encounter = next(
                        (
                            row
                            for row in admitted.get("encounters", [])
                            if row.get("encounter") == admitted.get("encounter")
                        ),
                        {},
                    )
                    definitions = {
                        p["actor"]: p["definition"] for p in encounter.get("deployments", [])
                    }
                    values = []
                    for member in allies:
                        progress = member.get("Progress")
                        maximum = progress or definitions.get(member["Actor"]["Value"])
                        if maximum is None:
                            values.append(None)
                            continue
                        hp, mp = (
                            maximum.get(k)
                            for k in (("MaxHp", "MaxMp") if progress else ("maxHp", "maxMp"))
                        )
                        values.append(
                            None
                            if hp is None or mp is None
                            else member["Hp"] == hp and member["Mp"] == mp
                        )
                    value = (
                        False if False in values else None if None in values or not values else True
                    )
                check(names[4], "source full ally HP/MP reset", value, seq)
            operation_flow_scene(
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
            )
