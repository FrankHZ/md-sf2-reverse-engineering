"""First Party.Progress and reward/JOIN/flag consumption through exploration return."""

from sf2tool.remake_h4.reward_checks import absent, actor, number


def compare_outcome(
    selected, context, ordered_events, event_rows, battle, operands, progress, balances, checks
):
    """Read final source/progress balances and bind their persistent return consumers."""
    check, eq, clocks, precedes = checks.check, checks.eq, checks.clocks, checks.precedes
    warps = selected["warpRecords"]
    source, equates, after_joins = operands["physical"], operands["equates"], operands["joins"]
    initial_actors = {a.get("id"): a for a in battle.get("actors") or []}
    ledger, gold = balances["ledger"], balances["gold"]
    gold_known, growth_actors = balances["goldKnown"], balances["growthActors"]
    first = context.get("firstOutcomeParty") or {}
    partyrow = warps.get(first.get("index"))
    post = (partyrow or {}).get("state") or {}
    eq(
        "first party boundary",
        dict(revision=first.get("revision"), observationSequence=first.get("sequence")),
        post,
    )
    final_party = {actor(p): p for p in post.get("party") or []}
    check(
        "first party complete actor roster",
        set(final_party) == set(initial_actors) if final_party else None,
    )
    joined = (post.get("partyLists") or {}).get("Joined")
    check("first party membership available", isinstance(joined, list) or None)
    before_party = (selected["samples"].get(context.get("preBattlePartySample")) or {}).get(
        "state"
    ) or {}
    clocks("pre-battle party", before_party)
    precedes("party admission precedes battle", before_party, battle)
    for membership in ("Active", "Joined", "Reserve"):
        old_members = (before_party.get("partyLists") or {}).get(membership)
        new_members = (post.get("partyLists") or {}).get(membership)
        check(
            "victory preserves actual party membership",
            old_members == new_members
            if isinstance(old_members, list) and isinstance(new_members, list)
            else None,
            membership,
        )
    outcome_sequence = next(
        (e["Sequence"] for e in ordered_events if e.get("Kind") == "battle-outcome"), None
    )
    observed_first = next(
        (
            i
            for i, r in warps.items()
            if number(outcome_sequence)
            and r.get("state", {}).get("observationSequence", -1) > outcome_sequence
            and r.get("state", {}).get("party") is not None
        ),
        None,
    )
    if first.get("index") in warps and "party" in post:
        eq("earliest retained party is declared first boundary", first.get("index"), observed_first)
    else:
        check("earliest retained party is declared first boundary", None)
    outcome_row = next(
        (event_rows[e["Sequence"]] for e in ordered_events if e.get("Kind") == "battle-outcome"),
        None,
    )
    last_battle = (warps.get(outcome_row - 1, {}) if number(outcome_row) else {}).get("state") or {}
    living = {a.get("id"): a for a in last_battle.get("actors") or []}
    for who, expected in ledger.items():
        p = progress[who]
        observed = final_party.get(who, absent)
        expected_party = dict(
            Exp=expected["exp"],
            Kills=expected["kills"],
            Defeats=expected["defeats"],
            Status=0,
            SourceLoadout=p["SourceLoadout"],
            Progress=p if who in growth_actors else None,
        )
        prior_actor = living.get(who)
        check("victory healing eligibility observed", True if prior_actor else None, who)
        if prior_actor and (
            prior_actor.get("hp", 0) > 0
            or int(who.split("-")[1]) in {equates.get("ALLY_PETER"), equates.get("ALLY_LEMON")}
        ):
            expected_party.update(Hp=p["MaxHp"], Mp=p["MaxMp"])
        elif prior_actor:
            expected_party.update(Hp=prior_actor.get("hp"), Mp=prior_actor.get("mp"))
        eq("composed first Party.Progress and victory healing", expected_party, observed, who)
        for i, r in warps.items():
            s = r.get("state") or {}
            if number(first.get("index")) and i > first["index"] and s.get("party") is not None:
                later_party = {actor(p): p for p in s["party"]}
                eq(
                    "outcome persistence through return",
                    expected_party,
                    later_party.get(who, absent),
                    i,
                )
    eq("first party gold", gold if gold_known else absent, post.get("gold", absent))
    for index, row in warps.items():
        if number(first.get("index")) and index > first["index"]:
            state = row.get("state") or {}
            eq(
                "source gold persists through return",
                gold if gold_known else absent,
                state.get("gold", absent),
                index,
            )
            # Independent receipt persistence still detects a changed balance when
            # source operands are absent. This outcome has no admitted gold operation.
            eq(
                "observed outcome gold persists through return",
                post.get("gold", absent),
                state.get("gold", absent),
                index,
            )
    outcome_events = [e for e in ordered_events if e.get("Kind") == "battle-outcome"]
    if outcome_events:
        precedes(
            "outcome precedes first party",
            dict(
                revision=outcome_events[0]["Revision"],
                observationSequence=outcome_events[0]["Sequence"],
            ),
            post,
        )
    tail = [
        e
        for e in ordered_events
        if e.get("Kind")
        in (
            "after-battle-join",
            "battle-unlock-cleared",
            "battle-completed-set",
            "exploration-return-started",
            "map-transferred",
            "battle-returned",
        )
    ]
    eq(
        "outcome operation order",
        [
            "after-battle-join",
            "battle-unlock-cleared",
            "battle-completed-set",
            "exploration-return-started",
            "map-transferred",
            "battle-returned",
        ],
        [e["Kind"] for e in tail],
    )
    for event in tail:
        if event["Kind"] not in (
            "after-battle-join",
            "battle-unlock-cleared",
            "battle-completed-set",
        ):
            continue
        detail = event.get("Detail")
        if not isinstance(detail, str) or not detail.isdecimal():
            check("outcome operation operand", None if detail is None else False, event["Sequence"])
            continue
        operand = int(detail)
        state = warps[event_rows[event["Sequence"]]].get("state") or {}
        if event["Kind"] == "after-battle-join":
            members = (state.get("partyLists") or {}).get("Joined")
            check(
                "JOIN receipt persists membership",
                operand in members if isinstance(members, list) else None,
                event["Sequence"],
            )
        else:
            flags = state.get("flags", state.get("storyFlags"))
            check(
                "flag receipt persists operation",
                (operand in flags) == (event["Kind"] == "battle-completed-set")
                if isinstance(flags, list)
                else None,
                event["Sequence"],
            )
    if source:
        joined_event = next((e for e in tail if e["Kind"] == "after-battle-join"), None)
        eq(
            "source after-battle JOIN operand",
            str(after_joins[1]),
            joined_event.get("Detail", absent) if joined_event else absent,
        )
        if joined_event:
            joined_state = warps[event_rows[joined_event["Sequence"]]].get("state") or {}
            eq(
                "after-battle JOIN membership persists",
                post.get("partyLists", absent),
                joined_state.get("partyLists", absent),
            )
        for kind, flag in [
            ("battle-unlock-cleared", equates["BATTLE_UNLOCKED_FLAGS_START"] + 1),
            ("battle-completed-set", equates["BATTLE_COMPLETED_FLAGS_START"] + 1),
        ]:
            event = next((e for e in tail if e["Kind"] == kind), None)
            if event:
                eq("source battle flag operand", str(flag), event.get("Detail", absent))
                for index, row in warps.items():
                    if index < event_rows[event["Sequence"]]:
                        continue
                    state = row.get("state") or {}
                    flags = state.get("flags", state.get("storyFlags"))
                    check(
                        "battle flag persistence through return",
                        (flag in flags) == (kind == "battle-completed-set")
                        if isinstance(flags, list)
                        else None,
                        index,
                    )
        if joined_event:
            for index, row in warps.items():
                if index < event_rows[joined_event["Sequence"]]:
                    continue
                for membership in ("Active", "Joined", "Reserve"):
                    expected = (post.get("partyLists") or {}).get(membership)
                    actual_members = ((row.get("state") or {}).get("partyLists") or {}).get(
                        membership
                    )
                    check(
                        "post-JOIN membership through return",
                        expected == actual_members
                        if isinstance(expected, list) and isinstance(actual_members, list)
                        else None,
                        index,
                    )
