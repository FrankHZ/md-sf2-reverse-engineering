"""Typed battle action, reaction and reward source-template operands."""

from bisect import bisect_right


def actor(e, key="Actor"):
    return (e.get(key) or {}).get("Value")


def battle_operands(pair, si, phase, hp, critical, body, token, p, events, check, _bounded_list):
    tid = value = who = None
    if phase in (
        "ActionMessage",
        "ResultMessage",
        "DeathMessage",
        "SpellCost",
        "MakeIdle",
        "SpellStop",
    ):
        check(
            f"battle typed action {token}",
            None if pair is None else si["actionKind"] == pair["Kind"],
        )
    healing = bool(si.get("healing"))
    if pair and phase in ("ActionMessage", "SpellCost"):
        who = actor(pair)
        value = int(si["spell"]["Level"]) if healing else 0
        tid = (
            274
            if healing
            else {
                "physical-first": 273,
                "physical-second": 293,
                "physical-counter": 292,
            }.get(pair["Kind"])
        )
    elif pair and phase in ("ResultMessage", "MakeIdle", "SpellStop"):
        who = actor(pair, "Target")
        tid = (
            298
            if healing
            else 286
            if si["reactionKind"] == "Dodge"
            else (287 if actor(pair).startswith("ally-") else 288)
            if critical
            else (284 if actor(pair).startswith("ally-") else 285)
        )
        value = 0 if si["reactionKind"] == "Dodge" else si.get("reactionAmount")
        if value is not None and si["reactionKind"] != "Dodge":
            check(
                f"reaction clipping {token}",
                None
                if len(hp) != 1
                else hp[0]["After"] - hp[0]["Before"] == value
                if healing
                else hp[0]["After"] == max(0, hp[0]["Before"] - value),
            )
    elif pair and phase == "DeathMessage":
        who = actor(pair, "Target")
        tid = 291 if who.startswith("ally-") else 290
        value = 0
    elif phase in ("RewardMessage", "GrowthMessage"):
        exp = [e for e in body if e["Kind"] == "exp"]
        if len(exp) == 1:
            who = actor(exp[0])
            if phase == "RewardMessage":
                tid = 263
                value = exp[0]["After"] - exp[0]["Before"] if exp[0]["After"] < 200 else None
            else:
                notices = []
                for kind, template in (
                    ("level", 244),
                    ("level-max-hp", 266),
                    ("level-max-mp", 267),
                    ("level-base-attack", 268),
                    ("level-defense", 269),
                    ("level-agility", 270),
                ):
                    for e in body:
                        if e["Kind"] == kind and (kind == "level" or e["After"] > e["Before"]):
                            notices.append(
                                (
                                    template,
                                    e["After"] if kind == "level" else e["After"] - e["Before"],
                                )
                            )
                growthstarts = [
                    e["Sequence"]
                    for e in body
                    if e["Kind"] == "scene-step-started" and e["Detail"] == "GrowthMessage"
                ]
                ni = bisect_right(growthstarts, token) - 1
                if 0 <= ni < len(notices):
                    tid, value = notices[ni]
    elif phase == "GoldMessage":
        gold = _bounded_list(
            e
            for e in events
            if e["record"] == p["record"]
            and e["Sequence"] <= p["Sequence"]
            and e["Kind"] == "gold"
            and actor(e) == actor(p)
        )
        if gold:
            tid = 393
            who = actor(p)
            value = sum(e["After"] - e["Before"] for e in gold)
    return tid, value, who, healing
