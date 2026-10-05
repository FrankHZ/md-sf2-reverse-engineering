"""Source-derived EXP/gold obligations at matched physical or HEAL scene operands."""

from sf2tool.h3.rng import _rng_step
from sf2tool.remake_h4.physical_source import source_action
from sf2tool.remake_h4.reward_checks import absent, actor, number


def compare_scenes(warps, events, event_rows, ordered_events, scenes, census, operands, checks):
    """Return expected EXP/gold/kill obligations, occurrence summaries and Unknowns."""
    check, eq, count = checks.check, checks.eq, checks.count
    source, equates, gold_table = operands["physical"], operands["equates"], operands["gold"]
    occurrences, unknown = [], []
    preparations = [e for e in ordered_events if e.get("Kind") == "scene-prepared"]
    check(
        "scene census coverage",
        {s.get("sequence") for s in scenes} == {e["Sequence"] for e in preparations}
        if preparations
        else None,
    )
    expected_exp, expected_gold, kills = {}, {}, {}
    for scene in scenes:
        sequence = scene.get("sequence")
        end = scene.get("end") or {}
        endseq = end.get("sequence")
        preparation = events.get(sequence)
        ended = events.get(endseq)
        eq(
            "preparation clocks",
            dict(Sequence=sequence, Revision=scene.get("revision"), Kind="scene-prepared"),
            preparation if preparation is not None else absent,
            sequence,
        )
        eq(
            "scene end clocks",
            dict(Sequence=endseq, Revision=end.get("revision"), Kind="scene-ended"),
            ended if ended is not None else absent,
            sequence,
        )
        if ended:
            eq("scene end owning result", end.get("index"), event_rows[endseq], sequence)
        if not (preparation and number(endseq) and source):
            check("scene reward operands", None, sequence)
            continue
        check(
            "scene ends after preparation",
            endseq > sequence and end.get("revision", -1) >= scene.get("revision", 0),
            sequence,
        )
        try:
            index = event_rows[sequence]
            row = warps[index]
            before = warps[index - 1]["state"]
            actors = {a["id"]: dict(a) for a in before["actors"]}
            preparing = [e for e in row["result"]["observations"] if e["Sequence"] <= sequence]
            who = actor(preparation)
            for e in preparing:
                if e["Kind"] == "movement" and actor(e) in actors:
                    actors[actor(e)].update(x=e["To"]["X"], y=e["To"]["Y"])
            effect_events = [e for e in ordered_events if sequence < e["Sequence"] <= endseq]
            draws = [e for e in preparing if e["Kind"] in ("rng-exp-plus", "rng-exp-minus")]
            physical = [e for e in preparing if e["Kind"] == "rng-dodge"]
            accumulator = gold = 0
            target = None
            if physical:
                target = actor(physical[0], "Target")
                model = source_action(source, actors, who, target, int(physical[0]["Before"]))
                expected_hp = [
                    dict(
                        Actor={"Value": s["target"]},
                        Target=None,
                        Before=s["beforeHp"],
                        After=s["afterHp"],
                    )
                    for s in model["strikes"]
                    if not s["dodge"]
                ]
                eq(
                    "reward damage operand matches source physical effect",
                    expected_hp,
                    [e for e in effect_events if e["Kind"] == "hp"],
                    sequence,
                )
                if who.startswith("ally-"):
                    difference = int(actors[who]["level"]) - int(actors[target]["level"])
                    kill_exp = 50 if difference < 3 else max(0, 70 - 10 * difference)
                    for strike in model["strikes"]:
                        if strike["actor"] != who:
                            check(
                                "unreached counter reward needs separate source rule",
                                None,
                                sequence,
                            )
                            continue
                        if not strike["dodge"]:
                            accumulator = min(
                                equates["PER_ACTION_EXP_CAP"],
                                accumulator
                                + kill_exp
                                * min(strike["damage"], strike["beforeHp"])
                                // int(actors[target]["maxHp"]),
                            )
                            if strike["afterHp"] == 0:
                                accumulator = min(
                                    equates["PER_ACTION_EXP_CAP"], accumulator + kill_exp
                                )
                                gold += gold_table[source["profiles"][target]["enemyId"]]
                                kills[target] = who
                    accumulator >>= 1  # Battle01 is in the accepted halved-EXP source table.
                seed = model["seed"]
            else:
                healing = next(e for e in effect_events if e["Kind"] == "heal")
                target = actor(healing, "Target")
                eq(
                    "HEAL reward source class",
                    "PRST",
                    source["profiles"][who].get("classCode"),
                    sequence,
                )
                recovery = min(15, int(actors[target]["maxHp"]) - int(actors[target]["hp"]))
                accumulator = min(
                    equates["HEALING_ACTION_EXP_CAP"],
                    max(
                        equates["HEALING_SPELL_EXP_MIN"],
                        equates["HEALING_SPELL_EXP_MAX"] * recovery // int(actors[target]["maxHp"]),
                    ),
                )
                seed = int(before["mainSeed"])
            eligible = who.startswith("ally-") and actors[who]["hp"] > 0
            count("eligible EXP draw count", 2 if eligible else 0, len(draws), sequence)
            if eligible:
                adjustment = 0
                for purpose, sign in (("rng-exp-plus", 1), ("rng-exp-minus", -1)):
                    observed_draw = next((e for e in draws if e.get("Kind") == purpose), absent)
                    word, value = _rng_step(seed >> 16, 32)
                    value >>= 1
                    after = (word << 16) | (seed & 65535)
                    eq(
                        "source EXP draw",
                        dict(
                            Kind=purpose,
                            Actor={"Value": who},
                            Target=None,
                            Before=seed,
                            After=after,
                            RandomRange=16,
                            RandomValue=value,
                        ),
                        observed_draw,
                        sequence,
                    )
                    adjustment += sign if value == 0 else 0
                    seed = after
                award = max(1, accumulator + adjustment)
                awards = [e for e in effect_events if e["Kind"] == "exp"]
                count("one deferred EXP command", 1, len(awards), sequence)
                planned = [x for x in census if sequence < x[2] <= endseq and x[3] == "exp"]
                count("one census EXP command", 1, len(planned), sequence)
                if planned:
                    expected_exp[planned[0][2]] = (who, award)
                eq(
                    "preparation leaves EXP uncommitted",
                    actors[who]["exp"],
                    next(a for a in row["state"]["actors"] if a["id"] == who).get("exp", absent),
                    sequence,
                )
            else:
                eq(
                    "enemy action awards no EXP",
                    [],
                    [e for e in effect_events if e["Kind"] == "exp"],
                    sequence,
                )
            gold_events = [e for e in preparing if e["Kind"] == "gold"]
            count("gold preparation count", 1 if gold else 0, len(gold_events), sequence)
            planned_gold = [
                x for x in census if x[0] == index and x[2] <= sequence and x[3] == "gold"
            ]
            count("one lethal census gold", 1 if gold else 0, len(planned_gold), sequence)
            if planned_gold:
                expected_gold[planned_gold[0][2]] = (who, gold)
            occurrences.append(
                dict(
                    sequence=sequence,
                    end=endseq,
                    actor=who,
                    target=target,
                    eligible=eligible,
                    accumulator=accumulator,
                    gold=gold,
                )
            )
        except (KeyError, TypeError, IndexError, StopIteration, ValueError) as error:
            check("scene reward operands", None, sequence)
            unknown.append(str(error))

    return dict(exp=expected_exp, gold=expected_gold, kills=kills), occurrences, unknown
