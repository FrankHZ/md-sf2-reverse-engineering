"""Independent roster/flag effects, counted prefixes and follower installation."""

from sf2tool.remake_h4.operation_flow_indexes import entity


def operation_flow_roster(
    op, seq, ins, anchor, state_of, ordered, instruction, join_effect, write_flags, names, check
):
    if op in ("set-flag", "join-party"):
        before = anchor(seq - 1, "flags")
        after = anchor(seq, "flags", False)
        expected = set(state_of(before)["flags"]) if before else None
        expected_lists = None
        if expected is not None and after:
            for earlier in ordered:
                prior = instruction(earlier)
                if (
                    before[0] < earlier["Sequence"] <= after[0]
                    and prior
                    and prior["op"] == "join-party"
                ):
                    expected_lists = join_effect(expected, prior["member"])
                    continue
                if before[0] < earlier["Sequence"] <= after[0] and not write_flags(
                    expected, earlier
                ):
                    expected = None
                    break
        check(
            names[2],
            "source writes and non-source writers reach independent flags",
            None
            if expected is None or after is None
            else expected == set(state_of(after)["flags"]),
            seq,
        )
        if op == "join-party" and after:
            lists = state_of(after).get("partyLists")
            check(
                names[2],
                "joined member retained in counted prefix",
                None if lists is None else ins["member"] in lists["Joined"],
                seq,
            )
            check(
                names[2],
                "ordered source counted joined/active/reserve prefixes",
                None if lists is None or expected_lists is None else lists == expected_lists,
                seq,
            )
    if op == "follow":
        held = anchor(seq, "entities", False)
        follower = entity(state_of(held), ins["entity"]) if held else None
        leader = entity(state_of(held), ins["leader"]) if held else None
        check(
            names[2],
            "source follower installation",
            None
            if follower is None or leader is None
            else follower.get("follower")
            == dict(LeaderSlot=leader["slot"], OffsetX=ins["x"], OffsetY=ins["y"]),
            seq,
        )
