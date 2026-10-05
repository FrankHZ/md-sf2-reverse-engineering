"""Compare local candidate scores and signed sorting at recorded operands."""

import json


def compare_scores(generation, by_actor, identity, checks):
    check, match = checks.check, checks.match
    unsorted = generation.get("Unsorted")
    if isinstance(unsorted, list):
        for actor_key, live in by_actor.items():
            # Within a candidate, source insertion is primary then extra.
            # Do not assume an unknown preceding candidate's slot count or
            # initial seed; the recorded local draws suffice for this rule.
            if live.get("Actor") is None:
                continue
            slots = [s for s in unsorted if json.dumps(s.get("Actor"), sort_keys=True) == actor_key]
            skipped = live.get("Placed") is False or live.get("Hp") == 0
            eligible = live.get("Placed") is True and type(live.get("Hp")) is int and live["Hp"] > 0
            extra_known = type(live.get("ExtraRoundAction")) is bool
            turns = 2 if live.get("ExtraRoundAction") is True else 1
            if skipped or eligible and extra_known:
                expected_count = 0 if skipped else turns
                check(
                    "candidate unsorted entry coverage",
                    False
                    if len(slots) > expected_count
                    else None
                    if len(slots) < expected_count
                    else True,
                    **identity,
                )
            # HP/placement determine admission, not the score formula. A
            # reached, unambiguously associated score can still contradict
            # known agility/draws when its admission input is unavailable.
            if not extra_known or len(slots) != turns or type(live.get("Agility")) is not int:
                continue
            for turn in range(turns):
                local = [
                    d
                    for d in generation.get("Draws") or []
                    if json.dumps(d.get("Actor"), sort_keys=True) == actor_key
                    and d.get("Turn") == turn
                ]
                count = 3 if turn == 0 else 2
                if len(local) != count or {d.get("Index") for d in local} != set(range(count)):
                    check("candidate score requires its local draws", None, **identity)
                    continue
                local.sort(key=lambda d: d["Index"])
                values = [d.get("Value") for d in local]
                if any(type(v) is not int for v in values):
                    check("candidate score requires its local draw values", None, **identity)
                    continue
                basis = live["Agility"] if turn == 0 else live["Agility"] * 5 // 6
                score = (basis + values[0] - values[1] + (values[2] - 1 if turn == 0 else 0)) & 255
                check(
                    "source score from matched candidate/local draws",
                    match(score, slots[turn].get("Score")),
                    **identity,
                )
    # Sorting is a rule at the recorded unsorted operands even when another
    # candidate's agility is absent. It cannot establish the missing score
    # construction, but a known wrong sorted buffer still contradicts it.
    if (
        isinstance(unsorted, list)
        and len(unsorted) == 64
        and all(
            "Actor" in s and type(s.get("Score")) is int and 0 <= s["Score"] <= 255
            for s in unsorted
        )
    ):
        source_sorted = list(unsorted)
        for _ in range(62):
            for index in range(63):
                left, right = source_sorted[index]["Score"], source_sorted[index + 1]["Score"]
                if (right if right < 128 else right - 256) > (left if left < 128 else left - 256):
                    source_sorted[index], source_sorted[index + 1] = (
                        source_sorted[index + 1],
                        source_sorted[index],
                    )
        check(
            "source signed stable sort at recorded unsorted operands",
            match(source_sorted, generation.get("Sorted")),
            **identity,
        )
