"""HEAL source scalar obligations and selection/preparation consumers."""

from sf2tool.h3.rng import _rng_step
from sf2tool.remake_h4.heal_checks import match, missing, ordered_kinds
from sf2tool.remake_h4.heal_evidence import event_envelope, scoped_events


def compare_preparation(warps, rows, by_revision, inputs, owner, ordinal, session, checks):
    """Compare source scalar effects, physical selection and prepared state."""
    check, one = checks.check, checks.one
    actor, target = owner.get("actor"), owner.get("expectedTarget")
    begin, end = owner.get("preparedRevision"), owner.get("sceneEndResultRevision")
    start_sequence, end_sequence = owner.get("sceneStartSequence"), owner.get("sceneEndSequence")
    pre = one(
        "preparation before state",
        [
            w
            for w in warps
            if (w.get("result") or {}).get("revision") == owner.get("beforeRevision")
        ],
        ordinal,
    )
    before = pre.get("state") or {}
    actor_before = one(
        "live caster", [a for a in before.get("actors", []) if a.get("id") == actor], ordinal
    )
    target_before = one(
        "live target", [a for a in before.get("actors", []) if a.get("id") == target], ordinal
    )
    check(
        "live pre-action session",
        match(dict(sessionId=session, revision=owner.get("beforeRevision")), before),
        ordinal,
    )
    hp, maximum = target_before.get("hp"), target_before.get("maxHp")
    mp, exp = actor_before.get("mp"), actor_before.get("exp")
    check("living target", None if hp is None or maximum is None else 0 < hp <= maximum, ordinal)
    check("sufficient MP", None if mp is None else mp >= 3, ordinal)
    recovery = None if hp is None or maximum is None else min(15, maximum - hp)
    seed = before.get("mainSeed")
    scalar_draws = []
    if seed is not None:
        seed = int(seed)
        for kind in ("rng-exp-plus", "rng-exp-minus"):
            word, value = _rng_step(seed >> 16, 32)
            after = (word << 16) | (seed & 65535)
            scalar_draws.append(
                dict(
                    Kind=kind,
                    Actor=dict(Value=actor),
                    Before=seed,
                    After=after,
                    RandomRange=16,
                    RandomValue=value >> 1,
                )
            )
            seed = after
    award = None
    if recovery is not None and maximum and scalar_draws:
        award = max(
            1,
            min(25, max(10, 25 * int(recovery) // int(maximum)))
            + (scalar_draws[0]["RandomValue"] == 0)
            - (scalar_draws[1]["RandomValue"] == 0),
        )
    scalar = dict(
        hp=hp, mp=mp, exp=exp, recovery=recovery, award=award, draws=scalar_draws, seed=seed
    )

    events = [(w, e) for w in rows for e in scoped_events(w, end, end_sequence)]
    for kind in ("spell-selected", "target-selected"):
        candidates = [
            w
            for w in warps
            if owner.get("beforeRevision", 0)
            < w.get("result", {}).get("revision", -1)
            < (begin or 0)
            and (kind != "target-selected" or w["result"]["revision"] == begin - 1)
            and any(e.get("Kind") == kind for e in w.get("result", {}).get("observations", []))
        ]
        chosen = one(kind, candidates, ordinal)
        check(
            "selected HEAL1 and actor",
            match(
                dict(sessionId=session, actor=actor, spell=dict(Value="heal", Level=1)),
                chosen.get("state", missing),
            ),
            ordinal,
        )
        if kind == "target-selected":
            check(
                "selected target",
                match(target, (chosen.get("state") or {}).get("target", missing)),
                ordinal,
            )
    resources = {
        "mp": (actor, mp, None if mp is None else mp - 3, "ActionMessage", "SpellCost"),
        "hp": (
            target,
            hp,
            None if recovery is None else hp + recovery,
            "TargetEnter" if actor != target else "ActionAnimation",
            "Reaction",
        ),
        "exp": (
            actor,
            exp,
            None if exp is None or award is None else min(200, exp + award),
            "ActorEnter" if actor != target else "SpellStop",
            "Reward",
        ),
    }
    effects = {}
    for kind in ("heal", "mp", "hp", "exp"):
        found = one(kind, [dict(w=w, e=e) for w, e in events if e.get("Kind") == kind], ordinal)
        wr, e = found.get("w", {}), found.get("e", {})
        effects[kind] = found
        if not e:
            check(kind + " effect unavailable", None, ordinal)
            continue
        check(
            "effect identity",
            match(dict(Actor=dict(Value=target if kind == "hp" else actor)), e),
            ordinal,
        )
        if kind == "heal":
            check("effect target", match(dict(Target=dict(Value=target)), e), ordinal)
        elif scalar:
            field = {"mp": "mp", "hp": "hp", "exp": "exp"}[kind]
            value = resources[kind][2]
            check(
                "source " + kind + " effect",
                match(
                    dict(
                        Before=scalar[field] if scalar[field] is not None else missing,
                        After=value if value is not None else missing,
                    ),
                    e,
                ),
                ordinal,
            )
            live = one(
                "effect state actor",
                [
                    a
                    for a in wr.get("state", {}).get("actors", [])
                    if a.get("id") == e.get("Actor", {}).get("Value")
                ],
                ordinal,
            )
            check(
                "effect actually applied",
                match(e.get("After", missing), live.get(field, missing)),
                ordinal,
            )
    if scalar:
        check(
            "source reward draws",
            None
            if not scalar["draws"]
            else ordered_kinds(
                scalar["draws"],
                [e for _, e in events if e.get("Kind") in ("rng-exp-plus", "rng-exp-minus")],
            ),
            ordinal,
        )
    prepared = one("prepared Submit", by_revision.get(begin, []), ordinal)
    check(
        "prepared Submit identity",
        match(
            dict(
                result=dict(
                    sessionId=session,
                    revision=begin,
                    observationSequence=start_sequence,
                    failure=None,
                ),
                state=dict(
                    sessionId=session,
                    revision=begin,
                    mainSeed=seed if seed is not None else missing,
                ),
            ),
            prepared,
        ),
        ordinal,
    )
    for who, values in ((actor, dict(mp=mp, exp=exp)), (target, dict(hp=hp))):
        live = one(
            "prepared deferred resources",
            [a for a in prepared.get("state", {}).get("actors", []) if a.get("id") == who],
            ordinal,
        )
        check(
            "resources remain deferred at preparation",
            match({k: v if v is not None else missing for k, v in values.items()}, live),
            ordinal,
        )
    confirms = [i for i in inputs if i.get("_index") == owner.get("inputIndex")]
    inp = one("physical target confirmation", confirms, ordinal)
    check(
        "confirm result join",
        match(
            dict(
                action="confirm",
                pressed=True,
                ordinal=owner.get("initializeInput"),
                resultStart=owner.get("firstSubmitResultIndex"),
                before=dict(
                    sessionId=session, revision=begin - 1 if begin is not None else missing
                ),
                after=dict(sessionId=session, revision=begin),
            ),
            inp,
        ),
        ordinal,
    )

    event_envelope(
        prepared,
        scoped_events(prepared, end, end_sequence),
        inp.get("before", {}).get("revision"),
        inp.get("before", {}).get("observationSequence"),
        ordinal,
        actor,
        target,
        checks,
    )
    for wr, event in events:
        if event.get("Kind") in ("rng-exp-plus", "rng-exp-minus"):
            check(
                "reward draw prepared command",
                match(dict(Revision=begin), event),
                ordinal,
                begin,
            )
            check(
                "reward draw prepared Submit",
                match(begin, wr.get("result", {}).get("revision", missing)),
                ordinal,
                begin,
            )
    return scalar, resources, effects, prepared, events
