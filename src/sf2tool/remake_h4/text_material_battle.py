"""Prepared battle message occurrence joins and independent required inventory."""

import re
from bisect import bisect_right

from .text_material_operands import actor, battle_operands
from .text_material_units import font


def text_material_battle(
    actual,
    events,
    event_by_sequence,
    records,
    session,
    names,
    enemy_names,
    texts,
    result,
    check,
    _bounded_list,
    _occurrence_map,
    _occurrence_set,
    _bounded_sorted,
):
    preps = _bounded_list(e for e in events if e["Kind"] == "scene-prepared")
    prepseq = _bounded_list(e["Sequence"] for e in preps)
    event_sequences = _bounded_list(e["Sequence"] for e in events)
    messages = _occurrence_map()
    for i, row in enumerate(actual.get("sceneObservations", [])):
        if row["scene"].get("message"):
            messages.setdefault(row["scene"]["waitToken"], []).append((i, row))

    def name(who):
        if who.startswith("ally-"):
            return names[int(who.split("-")[1])]
        if who.startswith("enemy-"):
            return enemy_names[int(who.split("-")[1])]
        raise ValueError("unsupported actor")

    for token, rows_for_token in messages.items():
        step = event_by_sequence.get(token)
        phase = step.get("Detail") if step and step["Kind"] == "scene-step-started" else None
        for i, row in rows_for_token:
            check(f"battle font {i}", font(row["scene"].get("messageFont"), 9))
            check(
                f"battle phase {i}", None if phase is None else row["scene"].get("phase") == phase
            )
        try:
            si = rows_for_token[0][1]["scene"]
            if si.get("reactionAmount") is None:
                si = next(
                    (
                        row["scene"]
                        for _, row in rows_for_token
                        if row["scene"].get("reactionAmount") is not None
                    ),
                    si,
                )
            gi = bisect_right(prepseq, token) - 1
            if gi < 0:
                check(f"battle prepare {token}", None)
                continue
            p = preps[gi]
            end = prepseq[gi + 1] if gi + 1 < len(preps) else float("inf")
            body = events[
                bisect_right(event_sequences, p["Sequence"] - 1) : bisect_right(
                    event_sequences, end - 1
                )
            ]
            if gi + 1 < len(preps):
                body = [
                    e
                    for e in body
                    if not (
                        e["record"] == preps[gi + 1]["record"]
                        and e["Kind"]
                        in (
                            "gold",
                            "rng-dodge",
                            "rng-critical",
                            "rng-spread-1",
                            "rng-spread-2",
                            "rng-double",
                            "rng-counter",
                        )
                    )
                ]
            starts = [
                e["Sequence"]
                for e in body
                if e["Kind"] == "scene-step-started" and e["Detail"] == "ActionMessage"
            ]
            rx = bisect_right(starts, token) - 1
            rxend = starts[rx + 1] if rx + 1 < len(starts) else float("inf")
            reaction = [e for e in body if rx >= 0 and starts[rx] <= e["Sequence"] < rxend]
            actions = [
                e
                for e in reaction
                if e["Kind"]
                in ("physical-first", "physical-second", "physical-counter", "heal", "item-use")
            ]
            pair = actions[0] if len(actions) == 1 else None
            hp = [
                e
                for e in reaction
                if e["Kind"] == "hp" and pair and actor(e) == actor(pair, "Target")
            ]
            critical = any(e["Kind"] == "critical" for e in reaction)
            step = event_by_sequence.get(token)
            phase = step.get("Detail") if step and step["Kind"] == "scene-step-started" else None
            tid, value, who, healing = battle_operands(
                pair, si, phase, hp, critical, body, token, p, events, check, _bounded_list
            )
            expected = None
            if tid is not None and value is not None and who is not None:
                expected = (
                    texts[tid]
                    .replace("{NAME}", name(who))
                    .replace("{#}", str(int(value)))
                    .replace("{N}", "\n")
                )
                if healing:
                    expected = expected.replace("{SPELL}", si["spell"]["Value"].upper())
                expected = re.sub(r"\{D[0-9]+\}", "", expected)
            for i, row in rows_for_token:
                check(
                    f"battle text {i}",
                    None
                    if expected is None
                    else row["sessionId"] == session and row["scene"]["message"] == expected,
                )
                check(
                    f"battle operand {i}",
                    None
                    if expected is None or row["scene"].get("reactionAmount") is None
                    else row["scene"].get("reactionAmount") == si.get("reactionAmount"),
                )
            mounted = _bounded_list(
                r["state"]["scene"]
                for r in records
                if r.get("state", {}).get("scene", {}).get("waitToken") == token
            )
            check(
                f"battle mounted {token}",
                True
                if any(
                    s.get("messageFont", {}).get("visible")
                    and (
                        s.get("visibleCharacters", 0) < 0
                        or s.get("visibleCharacters", 0) >= len(s.get("message", ""))
                    )
                    for s in mounted
                )
                else None,
            )
            result["battle"].append(
                dict(
                    token=token,
                    phase=phase,
                    sourceTemplate=tid,
                    prepare=p["Sequence"],
                    action=pair["Sequence"] if pair else None,
                    reactionAmount=si.get("reactionAmount"),
                    projections=len(rows_for_token),
                )
            )
        except (KeyError, IndexError, ValueError):
            check(f"missing battle occurrence operand {token}", None)
    message_phases = {
        "ActionMessage",
        "ResultMessage",
        "DeathMessage",
        "SpellCost",
        "MakeIdle",
        "SpellStop",
        "RewardMessage",
        "GoldMessage",
        "GrowthMessage",
    }
    required_battle = _occurrence_set(
        e["Sequence"]
        for e in events
        if e["Kind"] == "scene-step-started" and e.get("Detail") in message_phases
    )
    check(
        "every logical battle message paired",
        True if required_battle and all(token in messages for token in required_battle) else None,
    )
    result["requiredBattle"] = _bounded_sorted(required_battle)
