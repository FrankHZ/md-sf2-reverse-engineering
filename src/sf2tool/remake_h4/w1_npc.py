"""Admit stationary source walking, collision retries and actual destination effects."""

from sf2tool.remake_h4.w1_checks import merge
from sf2tool.remake_h4.w1_source import random


def compare_npc(
    ready, post, poll, suppressed, installations, npc_history_events, source, seed, checks
):
    check, match, one, absent = checks.check, checks.match, checks.one, checks.absent
    ordinal = poll.get("ordinal")
    layout, mutable = source.layout, source.mutable
    # esc00/esc01 plus the selected walking stream admit only these two
    # ready actors. Expected retries come from source collision gates below.
    entities = ready.get("entities")
    check("actual entity service operands", True if entities is not None else None, ordinal)
    expected_entities = set(poll.get("entityIds", []))
    observed_entities = [e.get("id") for e in entities or []]
    check(
        "complete live entity inventory",
        merge(
            [
                not (set(observed_entities) - expected_entities),
                len(observed_entities) == len(set(observed_entities)),
                True if expected_entities and set(observed_entities) == expected_entities else None,
            ]
        ),
        ordinal,
    )
    for entity in entities or []:
        check(
            "entity live service gates present",
            True
            if all(
                k in entity
                for k in (
                    "id",
                    "slot",
                    "x",
                    "y",
                    "targetX",
                    "targetY",
                    "actionCursor",
                    "waitTimer",
                    "moving",
                    "flagsA",
                    "flagsB",
                )
            )
            else None,
            ordinal,
        )
    if entities is None or any(
        not all(k in e for k in ("moving", "waitTimer", "flagsA")) for e in entities
    ):
        seed = None
    candidates = (
        [
            e
            for e in entities or []
            if e.get("moving") is False
            and isinstance(e.get("waitTimer"), (int, float))
            and e["waitTimer"] >= 20
            and isinstance(e.get("flagsA"), (int, float))
            and int(e["flagsA"]) & 64
        ]
        if not suppressed
        else []
    )
    npc_attempts = []
    for entity in candidates:
        actions = installations.get(entity.get("id"))
        for ri, source_op in npc_history_events:
            if ri >= poll.get("resultIndex", 0):
                break
            if source_op.get("entity") == entity.get("id"):
                if source_op.get("op") == "motion":
                    actions = source_op.get("actions")
                elif source_op.get("op") in ("hide", "follow"):
                    actions = None
        check(
            "NPC installed source walking stream",
            True if actions and any(x.get("op") == "random-walk" for x in actions) else None,
            ordinal,
        )
        walk = next((x for x in actions or [] if x.get("op") == "random-walk"), {})
        # entityscriptengine_2.asm:VInt_UpdateEntities calls
        # UpdateEntityData before dispatching the action script. Admit the
        # selected stationary edge from geometry/carried travel, not merely
        # the projection's moving flag. Residual velocity is legal at rest;
        # this does not claim that esc01 runs again once wait20 is reached.
        check(
            "NPC stationary geometry before source script",
            match(
                dict(
                    targetX=entity.get("x", absent),
                    targetY=entity.get("y", absent),
                    travelX=0,
                    travelY=0,
                ),
                entity,
            ),
            ordinal,
        )
        check(
            "NPC source wait20 gate",
            match(
                dict(
                    actionCursor=9,
                    waitTimer=20,
                    moving=False,
                    flagsA=239,
                    flagsB=64,
                    speedX=0,
                    speedY=0,
                    accelerationX=1,
                    accelerationY=1,
                ),
                entity,
            ),
            ordinal,
        )
        if not walk or layout is None or seed is None:
            seed = None
            continue
        x, y = entity.get("x"), entity.get("y")
        if x is None or y is None:
            check("NPC live position", None, ordinal)
            seed = None
            continue
        target = None
        for _ in range(4):
            seed, direction = random(seed, 4)
            dx, dy = ((384, 0), (0, -384), (-384, 0), (0, 384))[direction]
            tx, ty = x + dx, y + dy
            radius_ok = (
                (direction != 0 or x < (walk["x"] + walk["radius"]) * 384)
                and (direction != 1 or y > (walk["y"] - walk["radius"]) * 384)
                and (direction != 2 or x > (walk["x"] - walk["radius"]) * 384)
                and (direction != 3 or y < (walk["y"] + walk["radius"]) * 384)
            )
            blocked = not radius_ok
            cells = [(int(x // 384), int(y // 384)), (int(tx // 384), int(ty // 384))]
            check(
                "NPC unchanged source collision cells",
                all(
                    not any(mx <= cx < mx + mw and my <= cy < my + mh for mx, my, mw, mh in mutable)
                    for cx, cy in cells
                ),
                ordinal,
            )
            # These admitted horizontal moves start on flat cells. A slope
            # needs its own neighbor operand, never the flat-cell fallback.
            check(
                "NPC admitted flat departure",
                direction in (1, 3) or layout[int(y // 384) * 64 + int(x // 384)] & 0xC000 == 0,
                ordinal,
            )
            tile = layout[int(ty // 384) * 64 + int(tx // 384)]
            blocked |= tile >= 0xC000
            occupancy = []
            for other in entities or []:
                if other.get("id") == entity.get("id") or other.get("x") == 0x7000:
                    continue
                ox, oy = other.get("targetX"), other.get("targetY")
                if ox is None or oy is None:
                    check("NPC occupancy operand", None, ordinal)
                    continue
                occupancy.append(abs(ox - tx) + abs(oy - ty) < 384)
            blocked |= any(occupancy)
            npc_attempts.append(
                dict(entity=entity.get("id"), direction=direction, tile=tile, blocked=blocked)
            )
            if not blocked:
                target = (tx, ty)
                break
        actual_entity = one(
            "NPC destination witness",
            [e for e in post.get("entities", []) if e.get("id") == entity.get("id")],
            ordinal,
        )
        if target:
            check(
                "NPC source accepted destination",
                match(
                    dict(
                        x=x,
                        y=y,
                        targetX=target[0],
                        targetY=target[1],
                        moving=True,
                        waitTimer=0,
                        actionCursor=9,
                        velocityX=0,
                        velocityY=0,
                    ),
                    actual_entity,
                ),
                ordinal,
            )
        else:
            check("NPC exhausted attempts", match(dict(moving=False), actual_entity), ordinal)
    return seed, npc_attempts
