"""Compare independent live roster with actual generation candidate operands."""

import json

from sf2tool.remake_h4.turn_generation_checks import merge


def compare_candidates(state, generation, identity, checks):
    check, match = checks.check, checks.match
    candidates = state.get("candidates")
    fields = ("Actor", "ProcessingOrder", "Placed", "Hp", "Agility", "ExtraRoundAction")
    valid = []
    actors, orders = [], []
    for c in candidates or []:
        missing = any(k not in c or c[k] is None for k in fields)
        known = []
        for k, limit in (("ProcessingOrder", None), ("Hp", 65535), ("Agility", 127)):
            if c.get(k) is not None:
                known.append(type(c[k]) is int and c[k] >= 0 and (limit is None or c[k] <= limit))
        for k in ("Placed", "ExtraRoundAction"):
            if c.get(k) is not None:
                known.append(type(c[k]) is bool)
        actor = c.get("Actor")
        if actor is not None:
            known.append(
                isinstance(actor, dict)
                and isinstance(actor.get("Value"), str)
                and bool(actor["Value"])
            )
            actors.append(json.dumps(actor, sort_keys=True))
        if c.get("ProcessingOrder") is not None:
            orders.append(c["ProcessingOrder"])
        valid.append(merge(known + ([None] if missing else [])))
    check("complete live candidate operands", merge(valid) if candidates else None, **identity)
    check(
        "unique candidate identities/source orders",
        len(actors) == len(set(actors)) and len(orders) == len(set(orders)),
        **identity,
    )
    actual_candidates = generation.get("Candidates") or []
    actual_actors = [json.dumps(c.get("Actor"), sort_keys=True) for c in actual_candidates]
    actual_orders = [
        c.get("ProcessingOrder") for c in actual_candidates if c.get("ProcessingOrder") is not None
    ]
    check(
        "actual candidate duplicates rejected",
        len(actual_actors) == len(set(actual_actors))
        and len(actual_orders) == len(set(actual_orders)),
        **identity,
    )
    if state.get("turnOrder") is not None:
        check(
            "generation buffer agrees with independent state queue",
            match(state["turnOrder"], generation.get("Sorted")),
            **identity,
        )
    by_actor = {json.dumps(c.get("Actor"), sort_keys=True): c for c in candidates or []}
    # Complete operands are needed for the whole seed stream, not for a
    # known field on another identified candidate. Check those first.
    candidate_order = []
    for actual_candidate in actual_candidates:
        actor = actual_candidate.get("Actor")
        live = by_actor.get(json.dumps(actor, sort_keys=True)) if actor is not None else None
        check(
            "actual candidate belongs to independent live roster",
            None
            if actor is None or not candidates or any(c.get("Actor") is None for c in candidates)
            else live is not None,
            **identity,
        )
        if live is not None:
            if type(live.get("ProcessingOrder")) is int:
                candidate_order.append(live["ProcessingOrder"])
            known_fields = {k: live[k] for k in fields if live.get(k) is not None}
            check(
                "independently available candidate fields",
                match(known_fields, actual_candidate),
                **identity,
            )
    check(
        "available candidate source order",
        candidate_order == sorted(candidate_order),
        **identity,
    )
    return candidates, valid, by_actor
