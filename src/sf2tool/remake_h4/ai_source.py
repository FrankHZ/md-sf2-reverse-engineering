"""Pinned AI tables and original decisions for the admitted enemy caller domain.

Source calculation consumes matched live operands; it does not choose evidence,
classify a report or use candidate decisions as its expected values.
"""

import re
from pathlib import Path

from sf2tool.remake_h4.physical_source import source_operands


def source_rules(source_root):
    from sf2tool.h2.battle_ai import _parse_action_choice, _parse_standby

    root = Path(source_root)
    s = source_operands(root, [])
    d = root / "disasm"
    ai = d / "code/gameflow/battle/ai"
    s["standby"] = _parse_standby(
        d,
        (ai / "determineaistandbymovement_1.asm").read_text(),
        (ai / "determineaistandbymovement_2.asm").read_text(),
    )
    s["choice"] = _parse_action_choice(d)
    text = (d / "data/battles/spritesets/spriteset01.asm").read_text()
    s["orders"] = {
        f"enemy-{i}": list(map(int, m))
        for i, m in enumerate(
            re.findall(
                r"enemyCombatant[^\n]*\n\s*combatantAiAndItem[^\n]*\n"
                r"\s*combatantBehavior NONE, (\d+), NONE, (\d+), (\d+),",
                text,
            )
        )
    }
    return s


def source_decision(s, actors, who, seed):
    from sf2tool.h2.battlefield import build_weighted_movement_model
    from sf2tool.h3.random_services import _signed_byte_step

    a = actors[who]
    origin = (int(a["x"]), int(a["y"]))
    word = int(a["activationWord"])
    memory = int(a["aiMemory"])
    effects = []
    eq = s["equates"]
    profile = s["profiles"][who]
    mover = eq["MOVETYPE_" + profile["mover"]]
    costs = [
        -1 if entry == "OBSTRUCTED" else int(entry.split("|")[1])
        for entry in s["land"][mover * 16 : (mover + 1) * 16]
    ]
    live = [x for x in actors.values() if x["hp"] > 0 and x["x"] is not None and x["y"] is not None]
    targets = sorted(
        [x for x in live if x["id"].startswith("ally-")], key=lambda x: int(x["id"].split("-")[1])
    )
    assert who.startswith("enemy-") and a["hp"] > 0 and a["status"] == 0
    assert a["primaryOrder"] == a["secondaryOrder"] == 255 and a["commandset"] in (6, 7)
    assert a["items"] == [127] * 4 and a["spells"] == [63] * 4
    assert all(x["status"] == 0 for x in live), "matched status0 target domain"

    def pos(x):
        return int(x["x"]), int(x["y"])

    def offset(p):
        return p[1] * 48 + p[0]

    def grid(start, budget, block=False):
        terrain = list(s["terrain"])
        if block:
            for x in targets:
                terrain[offset(pos(x))] |= 128
        g = build_weighted_movement_model(terrain, costs, start_offset=offset(start), budget=budget)
        return {int(k): v for k, v in g["reachableCosts"].items()}

    def occupied(p):
        return any(pos(x) == p and (int(x["activationWord"]) & 8) == 0 for x in live)

    def attack_position(g, p, radius):
        best = None
        value = 255
        for dy in range(-radius, radius + 1):
            for dx in range(-radius + abs(dy), radius - abs(dy) + 1):
                if abs(dx) + abs(dy) != radius:
                    continue
                xy = p[0] + dx, p[1] + dy
                if not all(0 <= v < 48 for v in xy):
                    continue
                cost = g.get(offset(xy))
                if cost == 0:
                    return xy
                if cost is not None and cost < value and not occupied(xy):
                    best, value = xy, cost
        return best

    def walk(g, start, stop):
        current = offset(start)
        previous = 0
        path = [start]
        while g[current] > stop:
            threshold = g[current] - 1
            mask = 0
            for delta, bit in ((1, 1), (-1, 4), (-48, 2), (48, 8)):
                neighbor = current + delta
                if neighbor in g and g[neighbor] <= threshold:
                    assert (
                        abs(neighbor % 48 - current % 48) + abs(neighbor // 48 - current // 48) == 1
                    ), "source flat-grid edge"
                    mask |= bit
                    threshold = g[neighbor]
            choice = mask ^ previous if mask & previous and mask ^ previous else mask
            assert choice, "no descending source move-string"
            direction = next(i for i in range(4) if choice & (1 << i))
            previous = 1 << direction
            current += (1, -48, -1, 48)[direction]
            path.append((current % 48, current // 48))
            assert len(path) <= 2304
        return path

    def route(g, destination):
        return list(reversed(walk(g, destination, 0)))

    def effect(kind, before=None, after=None, target=None, **extra):
        effects.append(
            dict(
                Kind=kind,
                Actor={"Value": who},
                Target=None if target is None else {"Value": target},
                Before=before,
                After=after,
                **extra,
            )
        )

    def roll(bound, target=None):
        nonlocal seed
        before = seed
        word = seed >> 16
        for _ in range(256):
            word = _signed_byte_step(word)
            value = word >> 8
            if bound <= 1:
                value = 0
                break
            if value < bound:
                break
        else:
            raise ValueError("thinking RNG cycle")
        seed = (word << 16) | (seed & 65535)
        effect("thinking-rng", before, seed, target, RandomRange=bound, RandomValue=value)
        return value

    if not word & 1:
        draw = roll(8)
        destination = origin
        path = [origin]
        if draw not in s["standby"]["immediateStayRolls"]:
            pr, sr, _ = s["orders"][who]
            if pr != 15 or sr != 15:
                g = grid(origin, int(a["move"]) * 2)
                if memory & 15 == 0:
                    memory = 4 if roll(2) == 0 else 3
                count, old = memory & 15, memory >> 4
                table = next(
                    t["coordinates"]
                    for t in s["standby"]["movementTables"]
                    if t["moveCount"] == count
                )
                candidates = []
                for i, (dx, dy) in enumerate(table):
                    xy = int(a["anchorX"]) + dx, int(a["anchorY"]) + dy
                    if (
                        all(0 <= v < 48 for v in xy)
                        and attack_position(g, xy, 0) is not None
                        and i != old
                    ):
                        candidates.append((i, xy))
                if candidates:
                    index, destination = candidates[roll(len(candidates))]
                    memory = (index << 4) | count
                    path = route(g, destination)
                else:
                    memory = 0
        effect("ai-memory", int(a["aiMemory"]), memory)
        effect("source-standby")
        return dict(
            effects=effects,
            seed=seed,
            memory=memory,
            lastTarget=a["lastTarget"],
            path=path,
            kind="standby",
            target=None,
        )
    if a["commandset"] == 7:
        effect("ai-command-move-order1", after=-1)
    legal = grid(origin, int(a["move"]) * 2, True)
    candidates = []
    for target in targets:
        position = attack_position(legal, pos(target), 1)
        if position is not None:
            candidates.append((target, position, legal[offset(position)]))
    if candidates:
        effect("ai-command-attack1", after=0)
        priorities = {}
        assert word >> 12 == 2, "script3 activation column"
        for i in reversed(range(len(candidates))):
            target, position, cost = candidates[i]
            target_mover = eq["MOVETYPE_" + s["profiles"][target["id"]]["mover"]]
            terrain = s["terrain"][offset(pos(target))]
            land = s["land"][target_mover * 16 + terrain]
            multiplier = (
                256 if land.startswith("LE0|") else 230 if land.startswith("LE15|") else 205
            )
            potential = max(int(a["attack"]) - int(target["defense"]), 1) * multiplier // 256
            draw = roll(3, target["id"])
            priority = 1 + 15 * (potential >= target["hp"]) if draw == 0 else max(19 - 2 * cost, 1)
            priorities[i] = priority
            effect("ai-candidate", cost, priority, target["id"])
        maximum = max(priorities.values())
        cohort = [i for i in reversed(range(len(candidates))) if priorities[i] == maximum]
        if maximum >= 15:
            table = s["choice"]["criticalEnemyClassTieBreak"]
            order = table["classOrderTables"][table["movetypePointerTargets"][mover]]
            ranks = {
                i: order.index(eq["CLASS_" + s["profiles"][candidates[i][0]["id"]]["classCode"]])
                for i in cohort
            }
            cohort = [i for i in cohort if ranks[i] == min(ranks.values())]
        chosen = cohort[0]
        for i in cohort:
            if candidates[i][2] >= candidates[chosen][2]:
                chosen = i
        target, destination, _ = candidates[chosen]
        effect("ai-target", maximum, min(maximum, 15), target["id"])
        return dict(
            effects=effects,
            seed=seed,
            memory=memory,
            lastTarget=target["id"],
            path=route(legal, destination),
            kind="attack",
            target=target["id"],
        )
    for kind in ("ai-command-attack1", "ai-command-heal1", "ai-command-support"):
        effect(kind, after=-1)
    raw = grid(origin, 128)
    target_costs = [raw[offset(pos(x))] for x in targets]
    assert target_costs and all(0 <= v < 128 for v in target_costs), (
        "move target class-reorder domain"
    )
    selected = min(range(len(targets)), key=lambda i: target_costs[i])
    target = targets[selected]
    reverse = grid(pos(target), 128)
    preliminary = walk(reverse, origin, max(0, reverse[offset(origin)] - 4))
    destination = (
        origin
        if len(preliminary) == 1
        else attack_position(legal, preliminary[-1], 0)
        or attack_position(legal, preliminary[-1], 1)
        or origin
    )
    effect("ai-move-target", target_costs[selected], legal[offset(destination)], target["id"])
    effect("ai-move-stay" if destination == origin else "ai-move", target=target["id"])
    effect("ai-command-move1", after=0)
    return dict(
        effects=effects,
        seed=seed,
        memory=memory,
        lastTarget=a["lastTarget"],
        path=route(legal, destination),
        kind="pursuit",
        target=None,
    )
