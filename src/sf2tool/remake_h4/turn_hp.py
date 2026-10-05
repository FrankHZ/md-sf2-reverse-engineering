"""Carry HP and placement knowledge, distinguishing writes from missing observations."""

from sf2tool.remake_h4.turn_consumer_checks import merge


class HPKnowledge:
    def __init__(self, initial, absent):
        self.absent = absent
        self.hp = {}
        self.positions = {}
        for a in initial.get("actors") or []:
            self.hp[a.get("id")] = a["hp"] if a.get("hp") is not None else absent
            self.positions[a.get("id")] = (a.get("x", absent), a.get("y", absent))

    def apply_write(self, name, event, sequence, checks):
        hp, absent = self.hp, self.absent
        check, match = checks.check, checks.match
        check(
            "live HP before update",
            match(hp.get(name, absent), event.get("Before", absent)),
            sequence=sequence,
        )
        hp[name] = event["After"] if event.get("After") is not None else absent

    def cleanup(self, name, event, sequence, checks):
        hp, positions, absent = self.hp, self.positions, self.absent
        check, match = checks.check, checks.match
        check("cleanup follows zero HP", match(0, hp.get(name, absent)), sequence=sequence)
        positions[name] = (None, None) if event else (absent, absent)

    def observe(self, state, factions, index, checks):
        hp, positions, absent = self.hp, self.positions, self.absent
        check, match = checks.check, checks.match
        state_actors = {a.get("id"): a for a in state.get("actors") or []}
        check(
            "live roster covers independent factions",
            None
            if not state_actors
            else merge([True if name in state_actors else None for name in factions]),
            index=index,
        )
        check(
            "delivered HP agrees with ordered writes",
            merge(
                [
                    match(hp[name], a.get("hp", absent))
                    for name, a in state_actors.items()
                    if name in hp
                ]
            ),
            index=index,
        )
        for name, a in state_actors.items():
            # An omitted observation leaf does not erase prior knowledge.
            # A missing authoritative hp event After above DOES invalidate
            # the ledger until an actual subsequent observation restores it.
            if a.get("hp") is not None:
                hp[name] = a["hp"]
            old_position = positions.get(name, (absent, absent))
            check(
                "delivered placement leaves observed",
                merge([True if k in a else None for k in ("x", "y")]),
                index=index,
                actor=name,
            )
            positions[name] = (a.get("x", old_position[0]), a.get("y", old_position[1]))
