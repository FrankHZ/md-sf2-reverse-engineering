"""Choice and roster flag writers replayed from independent state anchors."""


def operation_flow_flags(world, maps, ordered, instruction, anchor, state_of, _occurrence_map):
    choices = _occurrence_map()
    for n, e in enumerate(ordered):
        if e["Kind"] == "choice-result-flag":
            producer = next(
                (
                    x
                    for x in reversed(ordered[:n])
                    if instruction(x) and instruction(x)["op"] == "yes-no"
                ),
                None,
            )
            accepted = next(
                (
                    x
                    for x in reversed(ordered[:n])
                    if x["Kind"] == "choice-accepted"
                    and producer
                    and x["Sequence"] > producer["Sequence"]
                ),
                None,
            )
            choices[e["Sequence"]] = (
                int(e["Detail"]),
                accepted["Detail"] == "yes"
                if accepted and accepted.get("Detail") in ("yes", "no")
                else None,
            )

    layout = world["partyFlags"]

    def join_effect(flags, member):
        flags.add(layout["joinedStart"] + member)
        joined = [i for i in range(layout["memberCount"]) if layout["joinedStart"] + i in flags]
        active = [i for i in joined if layout["activeStart"] + i in flags]
        reserve = [i for i in joined if i not in active]
        # Source JoinForce publishes counted prefixes before its active-flag store.
        if len(active) < layout["capacity"]:
            flags.add(layout["activeStart"] + member)
        return dict(Joined=joined, Active=active, Reserve=reserve)

    def write_flags(flags, e):
        ins = instruction(e)
        if ins and ins["op"] == "set-flag":
            (flags.add if ins["value"] else flags.discard)(ins["flag"])
        elif ins and ins["op"] == "join-party":
            join_effect(flags, ins["member"])
        elif e["Sequence"] in choices:
            flag, value = choices[e["Sequence"]]
            if value is None:
                return False
            (flags.add if value else flags.discard)(flag)
        elif e["Kind"] == "map-transferred" and e.get("Detail") in maps:
            for write in maps[e["Detail"]].get("entryFlags", []):
                (flags.add if write["value"] else flags.discard)(write["flag"])
        elif e["Kind"] in ("battle-unlock-cleared", "battle-completed-set"):
            battle = next(
                (
                    m["battle"]
                    for m in maps.values()
                    if m.get("battle") and m["battle"].get("encounter") == "battle-1"
                ),
                None,
            )
            key = "unlockedFlag" if e["Kind"] == "battle-unlock-cleared" else "completedFlag"
            if battle is None or battle.get(key) is None:
                return False
            (flags.discard if key == "unlockedFlag" else flags.add)(battle[key])
        elif e["Kind"] == "after-battle-join":
            battle = next(
                (
                    m["battle"]
                    for m in maps.values()
                    if m.get("battle") and m["battle"].get("encounter") == "battle-1"
                ),
                None,
            )
            if battle is None:
                return False
            join_effect(flags, battle["outcome"]["joinMember"])
        return True

    def flags_at(seq):
        before = anchor(seq, "flags")
        if before is None:
            return None
        flags = set(state_of(before)["flags"])
        for e in ordered:
            if before[0] < e["Sequence"] <= seq and not write_flags(flags, e):
                return None
        return flags

    return flags_at, join_effect, write_flags
