"""Compare candidate draw identities, local arithmetic and consecutive seeds."""

import json

from sf2tool.h3.rng import _rng_step
from sf2tool.remake_h4.turn_generation_checks import merge


def compare_draws(generation, candidates, by_actor, identity, checks):
    check, match = checks.check, checks.match
    draw_keys, draw_order = [], []
    identified_draws = {}
    for draw in generation.get("Draws") or []:
        actor_key = json.dumps(draw.get("Actor"), sort_keys=True)
        draw_keys.append((actor_key, draw.get("Turn"), draw.get("Index")))
        candidate = by_actor.get(actor_key)
        check(
            "draw candidate belongs to live roster",
            None if draw.get("Actor") is None or not candidates else candidate is not None,
            **identity,
        )
        if candidate:
            check(
                "draw requires placed/living candidate",
                merge(
                    [
                        None if candidate.get("Placed") is None else candidate["Placed"] is True,
                        None
                        if candidate.get("Hp") is None
                        else type(candidate["Hp"]) is int and candidate["Hp"] > 0,
                    ]
                ),
                **identity,
            )
        turn, index = draw.get("Turn"), draw.get("Index")
        check(
            "draw turn/index operands",
            None
            if turn is None or index is None
            else type(turn) is int
            and turn in (0, 1)
            and type(index) is int
            and 0 <= index <= (2 if turn == 0 else 1),
            **identity,
        )
        if (
            draw.get("Actor") is not None
            and type(turn) is int
            and turn in (0, 1)
            and type(index) is int
            and 0 <= index <= (2 if turn == 0 else 1)
        ):
            identified_draws[(actor_key, turn, index)] = draw
            if candidate and type(candidate.get("ProcessingOrder")) is int:
                draw_order.append((candidate["ProcessingOrder"], turn, index))
            if candidate and turn == 1 and candidate.get("ExtraRoundAction") is not None:
                check(
                    "secondary draw requires extra action",
                    candidate["ExtraRoundAction"] is True,
                    **identity,
                )
        if (
            candidate
            and type(candidate.get("Agility")) is int
            and turn in (0, 1)
            and type(index) is int
        ):
            basis = candidate["Agility"] if turn == 0 else candidate["Agility"] * 5 // 6
            check(
                "draw source agility range",
                match(3 if turn == 0 and index == 2 else basis >> 3, draw.get("Range")),
                **identity,
            )
        operands = [draw.get(k) for k in ("Before", "After", "Range", "Value")]
        if all(v is not None for v in operands):
            old, new, range_, value = operands
            valid_draw = all(type(v) is int and 0 <= v <= 65535 for v in operands)
            check(
                "draw word/range/result arithmetic",
                valid_draw and _rng_step(old, range_) == (new, value),
                **identity,
            )
        else:
            check("draw word/range/result arithmetic", None, **identity)
    check(
        "duplicate candidate draw identities rejected",
        len(draw_keys) == len(set(draw_keys)),
        **identity,
    )
    check("available draw source order", draw_order == sorted(draw_order), **identity)
    for (actor_key, turn, index), draw in identified_draws.items():
        predecessor = (
            (actor_key, turn, index - 1) if index > 0 else (actor_key, 0, 2) if turn == 1 else None
        )
        prior = identified_draws.get(predecessor)
        if prior is not None:
            check(
                "candidate consecutive draw seed chain",
                None if prior.get("After") is None else match(prior["After"], draw.get("Before")),
                **identity,
            )
