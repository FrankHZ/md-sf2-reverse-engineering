"""Replay source queue consumption, dead skips, rollovers and terminal support."""

from sf2tool.remake_h4.turn_consumer_checks import merge
from sf2tool.remake_h4.turn_hp import HPKnowledge


def actor(event):
    return (event.get("Actor") or {}).get("Value")


def replay_frontier(
    initial, factions, queues, census, expected, supplied, publications, required_rows, checks
):
    check, match, absent = checks.check, checks.match, checks.absent
    round_, cursor, terminal, terminal_support = 0, 0, None, None
    frontier = []
    knowledge = HPKnowledge(initial, checks.absent)
    hp, positions = knowledge.hp, knowledge.positions
    check(
        "source faction identity namespaces",
        None
        if not factions
        else all(
            value
            == (
                "Ally" if key.startswith("ally-") else "Enemy" if key.startswith("enemy-") else None
            )
            for key, value in factions.items()
        ),
    )
    hp_images = {}
    image_keys = {
        (row.get("result") or {}).get("observationSequence") for row in required_rows.values()
    }
    entered = set()
    consumed = set()
    timeline = sorted(set(supplied) | set(expected))
    state_at = {}

    def queued():
        queue = queues.get(round_)
        return (
            absent
            if queue is None or not 0 <= cursor < len(queue)
            else queue[cursor].get("actor", absent)
        )

    for sequence in timeline:
        c = expected.get(sequence)
        event = supplied.get(sequence) or {}
        if c is not None:
            kind, whom = c[3], actor(event)
            target = queued()
            if kind == "round-started":
                check(
                    "source sentinel permits next round",
                    True if round_ == 0 else match(None, target),
                    sequence=sequence,
                )
                check("no round after terminal", terminal is None, sequence=sequence)
                check(
                    "source consecutive round",
                    match(round_ + 1, event.get("After", absent)),
                    sequence=sequence,
                )
                round_ += 1
                cursor = 0
                check(
                    "declared round input exists",
                    True if round_ in queues else None,
                    sequence=sequence,
                )
            elif kind == "round-rng":
                check(
                    "round queue installed", True if round_ in queues else None, sequence=sequence
                )
            elif kind == "hp":
                knowledge.apply_write(c[4], event, sequence, checks)
            elif kind == "death-cleanup":
                knowledge.cleanup(c[4], event, sequence, checks)
            elif kind in (
                "player-control",
                "regions-tested-cleared",
                "after-turn",
                "action-committed",
                "dead-entry-skipped",
                "ai-stay",
                "heal",
            ):
                check(
                    "source queued identity at consumer",
                    match(target, whom if event else absent),
                    sequence=sequence,
                )
                if kind == "dead-entry-skipped":
                    check(
                        "source dead skip requires zero HP",
                        match(0, hp.get(c[4], absent)),
                        sequence=sequence,
                    )
                elif kind in ("player-control", "regions-tested-cleared", "ai-stay"):
                    value = hp.get(c[4], absent)
                    check(
                        "source action/control requires living caller",
                        None if value is absent else type(value) in (int, float) and value > 0,
                        sequence=sequence,
                    )
                    position = positions.get(c[4], (absent, absent))
                    check(
                        "source action/control requires placement",
                        None if absent in position else all(v is not None for v in position),
                        sequence=sequence,
                    )
                    entered.add((round_, cursor))
                elif kind in ("action-committed", "heal"):
                    check(
                        "action follows admitted queue entry",
                        True if (round_, cursor) in entered else None,
                        sequence=sequence,
                    )
                if kind in ("action-committed", "dead-entry-skipped", "ai-stay"):
                    check(
                        "no duplicated slot consumption",
                        (round_, cursor) not in consumed,
                        sequence=sequence,
                    )
                    consumed.add((round_, cursor))
                    frontier.append(
                        dict(
                            round=round_,
                            slot=cursor,
                            sequence=sequence,
                            kind=kind,
                            actor=whom,
                            observed=bool(event),
                        )
                    )
                    if terminal is None:
                        cursor += 1
            elif kind == "battle-outcome":
                check("unique real terminal", terminal is None, sequence=sequence)
                check(
                    "actual Victory terminal",
                    match("Victory", event.get("Detail", absent)),
                    sequence=sequence,
                )
                enemy_values = [
                    match(0, hp.get(name, absent))
                    for name, faction in factions.items()
                    if faction == "Enemy"
                ]
                enemy_support = merge(enemy_values) if enemy_values else None
                check("terminal source enemy HP", enemy_support, sequence=sequence)
                terminal_support = merge(
                    [
                        match("Victory", event.get("Detail", absent)),
                        enemy_support,
                    ]
                )
                terminal = sequence
        state_at[sequence] = (round_, cursor)
        for index in publications.get(sequence, ()):
            row = required_rows[index]
            body, state = row.get("result") or {}, row.get("state") or {}
            if sequence != body.get("observationSequence"):
                continue
            if state.get("round") is not None:
                check(
                    "delivered cursor/round through waits and inputs",
                    match(dict(round=round_, queueCursor=cursor), state),
                    index=index,
                )
                if state.get("actor") is not None:
                    check(
                        "delivered selection owns queued entry",
                        match(queued(), state["actor"]),
                        index=index,
                    )
                knowledge.observe(state, factions, index, checks)
        if sequence in image_keys:
            hp_images[sequence] = dict(hp)
    check("complete terminal frontier", terminal_support if terminal is not None else None)
    for round_number, queue in queues.items():
        for slot, entry in enumerate(queue or []):
            if entry.get("actor") is None:
                break
            reached = (round_number, slot) in consumed
            if not reached:
                check(
                    "unconsumed slots justified only by terminal",
                    None
                    if terminal_support is None
                    else terminal_support and round_number == round_ and slot > cursor,
                    round=round_number,
                    slot=slot,
                )
    return (
        frontier,
        dict(sequence=terminal, round=round_, cursor=cursor, value=terminal_support),
        state_at,
        hp_images,
    )
