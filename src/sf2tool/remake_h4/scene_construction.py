"""Independent source strikes, critical text operands and constructed scene phases."""

import subprocess

from sf2tool.remake_h4.physical_source import source_action, source_operands
from sf2tool.remake_h4.scene_checks import absent, actor


def construct_scenes(
    source_root,
    physical_context,
    heal_context,
    census,
    warps,
    events,
    owners,
    ordered,
    pairs,
    checks,
):
    """Derive action/text/phase obligations from source strikes and accepted effects."""
    check, eq = checks.check, checks.eq
    occurrences = []
    physical_source = None
    try:
        physical_source = source_operands(source_root, {56, 71, 85})
    except (OSError, TypeError, subprocess.CalledProcessError):
        check("physical source message operands available", None)
    except (ValueError, KeyError):
        check("physical source message operands valid", False)
    predicted_strikes = {}
    for c in physical_context.get("census", []):
        if physical_source is None:
            continue
        try:
            state = warps[c["index"]]["state"]
            actors = {a["id"]: a for a in state["actors"]}
            seed = events[c["firstDrawSequence"]]["Before"]
            prediction = source_action(
                physical_source, actors, actor(c, "actor"), actor(c, "target"), int(seed)
            )
            predicted_strikes[c["sequence"]] = prediction["strikes"]
        except (KeyError, TypeError, ValueError):
            check("physical source message operands available", None, c.get("sequence"))
    scene_info = {}
    for c in census:
        first, last = c["sequence"], c["end"]["sequence"]
        scene_events = [events[q] for q in ordered if first < q < last]
        actions = [
            e
            for e in scene_events
            if e.get("Kind") in ("physical-first", "physical-second", "physical-counter", "heal")
        ]
        expected_actions = predicted_strikes.get(first)
        if expected_actions is not None:
            eq(
                "source action actor/target order",
                [
                    dict(Kind=x["kind"], Actor={"Value": x["actor"]}, Target={"Value": x["target"]})
                    for x in expected_actions
                ],
                actions,
                first,
            )
        else:
            h = next(
                (
                    h
                    for h in heal_context.get("occurrences", [])
                    if h.get("sceneStartSequence") == first
                ),
                None,
            )
            if h:
                eq(
                    "accepted HEAL actor/target",
                    [
                        dict(
                            Kind="heal",
                            Actor={"Value": h["actor"]},
                            Target={"Value": h["expectedTarget"]},
                        )
                    ],
                    actions,
                    first,
                )
        expected_phases = ["Initialize"]
        phase_info = []
        for action in actions:
            a, target, kind = actor(action), actor(action, "Target"), action["Kind"]
            next_action = next(
                (e["Sequence"] for e in actions if e["Sequence"] > action["Sequence"]), last
            )
            reaction_events = [
                e for e in scene_events if action["Sequence"] < e["Sequence"] < next_action
            ]
            hp = next(
                (e for e in reaction_events if e["Kind"] == "hp" and actor(e) == target), None
            )
            dodge = any(e["Kind"] == "dodge" for e in reaction_events)
            critical_events = [e for e in reaction_events if e["Kind"] == "critical"]
            recovery = kind == "heal"
            critical = False if recovery else None
            amount = hp["After"] - hp["Before"] if recovery and hp else 0
            if not recovery:
                strike = next(
                    (
                        x
                        for x in predicted_strikes.get(first, [])
                        if (x["kind"], x["actor"], x["target"]) == (kind, a, target)
                    ),
                    None,
                )
                check(
                    "source strike operands present", True if strike else None, action["Sequence"]
                )
                amount = strike["damage"] if strike else None
                if strike:
                    critical = strike["critical"]
                    eq(
                        "source critical effect",
                        [dict(Kind="critical", Actor={"Value": a}, Target={"Value": target})]
                        if critical
                        else [],
                        critical_events,
                        action["Sequence"],
                    )
                    eq(
                        "source strike effect kind",
                        "Dodge" if strike["dodge"] else "Damage",
                        "Dodge" if dodge else "Damage",
                        action["Sequence"],
                    )
                    if not strike["dodge"]:
                        eq(
                            "source deferred HP effect",
                            dict(
                                Actor={"Value": target},
                                Before=strike["beforeHp"],
                                After=strike["afterHp"],
                            ),
                            hp if hp else absent,
                            action["Sequence"],
                        )
            info = dict(
                actor=a,
                target=target,
                kind=kind,
                hp=hp,
                recovery=recovery,
                reaction="Recovery" if recovery else "Dodge" if dodge else "Damage",
                amount=amount,
                critical=critical,
            )
            phases = ["ActionMessage"] + (["SpellCost"] if recovery else []) + ["ActionAnimation"]
            if recovery and a != target:
                phases += ["TargetExit", "TargetEnter"]
            phases += ["Reaction", "ResultMessage"]
            if recovery:
                phases += ["MakeIdle", "SpellStop"]
                if a != target:
                    phases += ["ActorExit", "ActorEnter"]
            elif hp and hp.get("After") == 0:
                phases += ["DeathMessage"]
            expected_phases += phases
            phase_info += [(p, info) for p in phases]
        exp = [e for e in scene_events if e["Kind"] == "exp"]
        growth = [
            e
            for e in scene_events
            if e["Kind"]
            in (
                "level",
                "level-max-hp",
                "level-max-mp",
                "level-base-attack",
                "level-defense",
                "level-agility",
            )
            and (e["Kind"] == "level" or e.get("After", 0) > e.get("Before", 0))
        ]
        growth.sort(key=lambda e: (0 if e["Kind"] == "level" else 1, e["Sequence"]))
        # Construction gold precedes scene-prepared, within its owning result.
        gold = [
            e
            for e in events.values()
            if owners[e["Sequence"]] == c["index"] and e["Kind"] == "gold"
        ]
        if exp:
            expected_phases += ["Reward", "RewardMessage"] + ["GrowthMessage"] * len(growth)
        if gold:
            expected_phases += ["GoldMessage"]
        expected_phases += ["End"]
        reached = [p for p in pairs if first <= p["token"] < last]
        eq("source constructed phase order", expected_phases, [p["phase"] for p in reached], first)
        check("source action evidence", bool(actions) or None, first)
        current = phase_info[0][1] if phase_info else {}
        queue = iter(phase_info)
        growth_index = 0
        for p in reached:
            if p["phase"] in {x[0] for x in phase_info}:
                _, current = next(queue, (None, current))
            info = dict(current, scene=first, exp=exp, gold=gold)
            if p["phase"] == "GrowthMessage":
                info["growth"] = growth[growth_index] if growth_index < len(growth) else {}
                growth_index += 1
            scene_info[p["token"]] = info
            effect = (
                info.get("hp")
                if p["phase"] == "Reaction"
                else info["exp"][0]
                if p["phase"] == "Reward" and info["exp"]
                else None
            )
            if effect:
                eq(
                    "effect belongs to required consumer transition",
                    owners[p["token"]],
                    owners[effect["Sequence"]],
                    p["token"],
                )
                check(
                    "effect precedes presentation of its result",
                    effect["Sequence"] < p["token"],
                    p["token"],
                )
                post = {
                    x.get("id"): x
                    for x in warps[owners[effect["Sequence"]]]["state"].get("actors", [])
                }
                eq(
                    "effect applied to owning actor state",
                    effect["After"],
                    post.get(actor(effect), {}).get(
                        "hp" if effect["Kind"] == "hp" else "exp", absent
                    ),
                    p["token"],
                )
        occurrences.append(dict(sequence=first, end=last, phases=len(reached)))
    return scene_info, occurrences
