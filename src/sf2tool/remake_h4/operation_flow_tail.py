"""Shared-tail markers require independent flags/counts and intervening writers."""


def operation_flow_tail(
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
):
    tail = battle_map["battle"]
    effects = [
        ("after-battle-join", tail["outcome"]["joinMember"]),
        ("battle-unlock-cleared", tail["unlockedFlag"]),
        ("battle-completed-set", tail["completedFlag"]),
    ]
    previous = outcome_start["Sequence"]
    for kind, operand in effects:
        effect = next((e for e in ordered if e["Kind"] == kind and e["Sequence"] > previous), None)
        check(
            names[4],
            "source shared-tail operand " + kind,
            None if effect is None else int(effect["Detail"]) == operand,
            previous,
        )
        check(
            names[2],
            "source enclosing roster/flag operand " + kind,
            None if effect is None else int(effect["Detail"]) == operand,
            previous,
        )
        if effect:
            before_flags = flags_at(effect["Sequence"] - 1)
            after = next(
                (
                    x
                    for x in states
                    if effect["Sequence"] <= x[0]
                    and returned
                    and x[0] <= returned["Sequence"]
                    and "flags" in state_of(x)
                ),
                None,
            )
            expected_lists = None
            if before_flags is not None:
                if kind == "after-battle-join":
                    expected_lists = join_effect(before_flags, operand)
                elif kind == "battle-unlock-cleared":
                    before_flags.discard(operand)
                else:
                    before_flags.add(operand)
                if after:
                    for writer in ordered:
                        if effect["Sequence"] < writer["Sequence"] <= after[0] and not write_flags(
                            before_flags, writer
                        ):
                            before_flags = None
                            break
            for family in (names[2], names[4]):
                check(
                    family,
                    "actual shared-tail flag effect " + kind,
                    None
                    if before_flags is None or after is None
                    else before_flags == set(state_of(after)["flags"]),
                    effect["Sequence"],
                )
                if kind == "after-battle-join":
                    lists = next(
                        (
                            x
                            for x in states
                            if effect["Sequence"] <= x[0]
                            and returned
                            and x[0] <= returned["Sequence"]
                            and "partyLists" in state_of(x)
                        ),
                        None,
                    )
                    check(
                        family,
                        "actual shared-tail counted membership",
                        None
                        if lists is None or expected_lists is None
                        else state_of(lists)["partyLists"] == expected_lists,
                        effect["Sequence"],
                    )
            previous = effect["Sequence"]
