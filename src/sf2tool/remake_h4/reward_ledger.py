"""Ordered reward receipts, growth, death/after-turn effects and combatant resources.

Progress is the one mutable party-growth ledger initialized by source admission.
This reducer updates it in source-event order; outcome checks then read that same
ledger. Its balance result contains only resource state, not evidence or transport.
"""

from sf2tool.h3.growth import _calculate_gain
from sf2tool.h3.rng import _rng_step
from sf2tool.remake_h4.reward_checks import absent, actor, number


def reduce_rewards(warps, events, census, initial, operands, progress, rewards, checks):
    """Update progress; return final EXP/kill/gold balances and ordered Unknowns."""
    check, eq, count = checks.check, checks.eq, checks.count
    source, profiles, equates = operands["physical"], operands["profiles"], operands["equates"]
    curves, enemy_operands = operands["curves"], operands["enemies"]
    expected_items, expected_spells = operands["items"], operands["spells"]
    expected_exp, expected_gold, kills = rewards["exp"], rewards["gold"], rewards["kills"]
    growth_actors, unknown = set(), []
    # Only this child's persisted resources; action legality remains with its owner.
    ledger = {who: dict(exp=0, kills=0, defeats=0) for who in profiles}
    gold = initial.get("gold")
    gold_known = source is not None
    check("initial gold operand", number(gold) if gold is not None else None)
    processed = set()
    last_scene_actor = last_cleanup = last_after_turn = None
    ledger_kinds = {
        "exp",
        "gold",
        "exp-threshold",
        "level",
        "kills",
        "death-cleanup",
        "after-turn",
        "action-committed",
        "battle-outcome",
        "scene-ended",
    }
    ledger_indices = set(warps) | {x[0] for x in census if x[3] in ledger_kinds}
    for index in sorted(ledger_indices):
        row = warps.get(index, {})
        state = row.get("state") or {}
        before = (warps.get(index - 1) or {}).get("state") or {}
        actors = {a.get("id"): a for a in state.get("actors") or []}
        previous_actors = {a.get("id"): a for a in before.get("actors") or []}
        current_events = []
        for e in (row.get("result") or {}).get("observations", []):
            if e.get("Sequence") not in processed:
                processed.add(e.get("Sequence"))
                current_events.append(e)
        # Missing effects remain missing observations. Their declared identity still
        # schedules independently calculated resource changes, preventing a lost EXP
        # row from turning every later correct resource snapshot into a false failure.
        for declared in census:
            if (
                declared[0] == index
                and declared[3] in ledger_kinds
                and declared[2] not in processed
            ):
                processed.add(declared[2])
                current_events.append(
                    dict(
                        Kind=declared[3],
                        Sequence=declared[2],
                        Revision=declared[1],
                        Actor=None if declared[4] is None else {"Value": declared[4]},
                    )
                )
        current_events.sort(key=lambda e: e["Sequence"])
        # Receipt/state joins are independent of the source arithmetic. An absent
        # source table cannot hide a contradictory actual command and actual state.
        receipt_fields = {
            "exp": "exp",
            "exp-threshold": "exp",
            "kills": "kills",
            "defeats": "defeats",
            "level": "level",
            "level-max-hp": "maxHp",
            "level-max-mp": "maxMp",
            "level-defense": "defense",
        }
        pending_receipts = {}
        for observed in (row.get("result") or {}).get("observations", []):
            kind = observed.get("Kind")
            if kind != "gold" and kind not in receipt_fields:
                continue
            who = actor(observed)
            field = "gold" if kind == "gold" else receipt_fields[kind]
            key = (None if kind == "gold" else who, field)
            prior_state = before if kind == "gold" else previous_actors.get(who, {})
            old = pending_receipts.get(key, prior_state.get(field, absent))
            eq(
                "reward receipt before joins actual state",
                old,
                observed.get("Before", absent),
                observed.get("Sequence"),
            )
            pending_receipts[key] = observed.get("After", absent)
        for (who, field), value in pending_receipts.items():
            current_state = state if who is None else actors.get(who, {})
            eq(
                "reward receipt after joins actual state",
                value,
                current_state.get(field, absent),
                index,
            )
        for e in current_events:
            kind, seq, who = e.get("Kind"), e.get("Sequence"), actor(e)
            try:
                if kind == "scene-ended":
                    last_scene_actor = who
                if kind == "gold":
                    expected = expected_gold.get(seq)
                    check(
                        "gold belongs to source lethal preparation", True if expected else None, seq
                    )
                    if expected and number(gold):
                        eq(
                            "source kill gold",
                            dict(
                                Actor={"Value": expected[0]},
                                Before=gold,
                                After=min(9999999, gold + expected[1]),
                            ),
                            e,
                            seq,
                        )
                        gold = min(9999999, gold + expected[1])
                    else:
                        gold_known = False
                if kind == "exp":
                    expected = expected_exp.get(seq)
                    check("EXP belongs to eligible source scene", True if expected else None, seq)
                    if expected:
                        recipient, amount = expected
                        old = ledger[recipient]["exp"]
                        new = min(200, old + amount)
                        eq(
                            "source EXP command saturation",
                            dict(Actor={"Value": recipient}, Before=old, After=new),
                            e,
                            seq,
                        )
                        ledger[recipient]["exp"] = new
                        marker = next(
                            (
                                x
                                for x in current_events
                                if x.get("Kind") == "scene-step-started"
                                and x.get("Detail") == "Reward"
                            ),
                            None,
                        )
                        check(
                            "EXP precedes its Reward marker",
                            marker["Sequence"] > seq if marker else None,
                            seq,
                        )
                if kind == "exp-threshold":
                    old = ledger[who]["exp"]
                    check("level threshold reached", old >= 100, seq)
                    eq("one threshold subtraction", dict(Before=old, After=old - 100), e, seq)
                    ledger[who]["exp"] = old - 100
                    growth_actors.add(who)
                    p = progress[who]
                    check("reached unpromoted level below cap", p["Level"] < 40, seq)
                    draws = [
                        x
                        for x in current_events
                        if x.get("Kind") in ("rng-growth-plus", "rng-growth-minus")
                    ]
                    seed = int(before["mainSeed"])
                    cursor = 0
                    field_map = [
                        ("hp", "MaxHp", "level-max-hp"),
                        ("mp", "MaxMp", "level-max-mp"),
                        ("attack", "BaseAttack", "level-base-attack"),
                        ("defense", "Defense", "level-defense"),
                        ("agility", "Agility", "level-agility"),
                    ]
                    for stat, field, event_kind in field_map:
                        definition = profiles[who]["stats"][stat]
                        values = []
                        if definition["curve"]:
                            for purpose in ("rng-growth-plus", "rng-growth-minus"):
                                growth_census = [
                                    x
                                    for x in census
                                    if x[0] == index
                                    and x[3] in ("rng-growth-plus", "rng-growth-minus")
                                ]
                                draw = (
                                    events.get(growth_census[cursor][2], {})
                                    if cursor < len(growth_census)
                                    else {}
                                )
                                cursor += 1
                                word, value = _rng_step(seed >> 16, 256)
                                value >>= 1
                                after = (word << 16) | (seed & 65535)
                                eq(
                                    "source growth draw",
                                    dict(
                                        Kind=purpose,
                                        Actor={"Value": who},
                                        Before=seed,
                                        After=after,
                                        RandomRange=128,
                                        RandomValue=value,
                                    ),
                                    draw,
                                    seq,
                                )
                                values.append(value)
                                seed = after
                            gain, _ = _calculate_gain(
                                current=p[field],
                                start=definition["start"],
                                projected=definition["projected"],
                                curve=definition["curve"],
                                level=p["Level"],
                                first=values[0],
                                second=values[1],
                                curves=curves,
                            )
                        else:
                            gain = 0
                        old_stat = p[field]
                        p[field] = min(100 if field == "Agility" else 200, old_stat + gain)
                        observed = next(
                            (x for x in current_events if x.get("Kind") == event_kind), None
                        )
                        eq(
                            "source growth stat",
                            dict(Actor={"Value": who}, Before=old_stat, After=p[field]),
                            observed if observed else absent,
                            seq,
                        )
                    count("growth draw cardinality", cursor, len(draws), seq)
                    level = next((x for x in current_events if x.get("Kind") == "level"), None)
                    eq(
                        "source level increment",
                        dict(Actor={"Value": who}, Before=p["Level"], After=p["Level"] + 1),
                        level if level else absent,
                        seq,
                    )
                    p["Level"] += 1
                    learning = [s for s in profiles[who]["spells"] if s["level"] == p["Level"]]
                    check("no learned spell applicable at reached level", not learning, seq)
                    eq("growth carries updated seed", seed, state.get("mainSeed", absent), seq)
                    eq(
                        "level does not heal current resources",
                        {k: previous_actors[who][k] for k in ("hp", "mp")},
                        actors.get(who, absent),
                        seq,
                    )
                if kind == "kills":
                    check("kill credited to completed scene actor", who == last_scene_actor, seq)
                    old = ledger[who]["kills"]
                    eq("kill counter increment", dict(Before=old, After=old + 1), e, seq)
                    ledger[who]["kills"] = old + 1
                if kind == "death-cleanup":
                    eq("cleanup work item consumed", dict(Before=1, After=0), e, seq)
                    eq(
                        "lethal source target cleanup recipient",
                        kills.get(who, absent),
                        last_scene_actor,
                        seq,
                    )
                    eq(
                        "cleanup zeroes dead actor and removes placement",
                        dict(hp=0, status=0, x=None, y=None),
                        actors.get(who, absent),
                        seq,
                    )
                    eq(
                        "cleanup starts from dead actor",
                        0,
                        previous_actors.get(who, {}).get("hp", absent),
                        seq,
                    )
                    last_cleanup = seq
                if kind == "after-turn":
                    last_after_turn = (who, seq)
                    a = previous_actors[who]
                    check(
                        "death cleanup precedes after-turn",
                        not any(
                            a["hp"] <= 0 and a.get("x") is not None
                            for a in previous_actors.values()
                        ),
                        seq,
                    )
                    check(
                        "factions still active before after-turn",
                        previous_actors.get("ally-0", {}).get("hp", 0) > 0
                        and any(
                            a["hp"] > 0 and a.get("x") is not None
                            for k, a in previous_actors.items()
                            if k.startswith("enemy-")
                        ),
                        seq,
                    )
                    eq("reached after-turn has no status", 0, a.get("status", absent), seq)
                    if source:
                        forbidden = {
                            equates["ITEM_" + name]
                            for name in ("HOLY_STAFF", "MYSTERY_STAFF", "LIFE_RING")
                        }
                        check(
                            "no equipped after-turn recovery item",
                            not any(
                                int(w) & 128 and (int(w) & 127) in forbidden for w in a["items"]
                            ),
                            seq,
                        )
                    else:
                        check("source after-turn equipment operands", None, seq)
                    eq(
                        "after-turn preserves current resources",
                        {k: a[k] for k in ("hp", "mp", "status")},
                        actors.get(who, absent),
                        seq,
                    )
                    check("after-turn actor alive", a["hp"] > 0, seq)
                    last_after_turn = (who, seq)
                if kind == "action-committed":
                    outcome_here = any(
                        x["Kind"] == "battle-outcome" and x["Sequence"] < seq
                        for x in current_events
                    )
                    check(
                        "post-action after-turn or outcome precedes commit",
                        outcome_here
                        or bool(
                            last_after_turn
                            and last_after_turn[0] == who
                            and last_after_turn[1] < seq
                        ),
                        seq,
                    )
                    last_after_turn = None
                if kind == "battle-outcome":
                    eq("reached outcome", "Victory", e.get("Detail", absent), seq)
                    check(
                        "victory after final cleanup",
                        last_cleanup is not None and last_cleanup < seq,
                        seq,
                    )
                    check(
                        "victory living factions",
                        bool(previous_actors)
                        and any(
                            a["hp"] > 0 and a.get("x") is not None
                            for k, a in previous_actors.items()
                            if k.startswith("ally-")
                        )
                        and previous_actors.get("ally-0", {}).get("hp", 0) > 0
                        and not any(
                            a["hp"] > 0 and a.get("x") is not None
                            for k, a in previous_actors.items()
                            if k.startswith("enemy-")
                        ),
                        seq,
                    )
                    check(
                        "victory does not run final after-turn",
                        not any(x["Kind"] == "after-turn" for x in current_events),
                        seq,
                    )
            except (KeyError, TypeError, ValueError, IndexError) as error:
                check("reward effect operands", None, seq)
                unknown.append(str(error))
        if actors and source:
            for who, operands in enemy_operands.items():
                eq(
                    "source enemy reward operands remain stable",
                    operands,
                    actors.get(who, absent),
                    index,
                )
            for who, expected in ledger.items():
                p = progress[who]
                attack = p["BaseAttack"] + sum(
                    n
                    for w in expected_items[who]
                    if w & 128
                    for code, n in source["items"][w & 127]["effects"]
                    if code == "INCREASE_ATT"
                )
                eq(
                    "persisted combatant reward state",
                    expected
                    | dict(
                        level=p["Level"],
                        maxHp=p["MaxHp"],
                        maxMp=p["MaxMp"],
                        attack=attack,
                        defense=p["Defense"],
                        items=expected_items[who],
                        spells=expected_spells[who],
                        learned=p["Spells"],
                        status=0,
                    ),
                    actors.get(who, absent),
                    index,
                )
            eq(
                "persisted battle gold",
                gold if gold_known else absent,
                state.get("gold", absent),
                index,
            )

    return dict(ledger=ledger, gold=gold, goldKnown=gold_known, growthActors=growth_actors), unknown
