"""Sequential HEAL logical/delivery opportunities and fairy/seed carry."""

import copy

from sf2tool.h3.rng import _rng_step
from sf2tool.remake_h4.heal_checks import match, missing, ordered_kinds
from sf2tool.remake_h4.heal_evidence import event_envelope, scoped_events
from sf2tool.remake_h4.heal_source import fairy_source_step


def compare_opportunities(
    rows,
    prepared,
    inputs,
    owner,
    ordinal,
    session,
    quarter,
    projections,
    inventory_complete,
    scalar,
    checks,
):
    """Reduce observed services in order; previous scene/seed stay local to this call."""
    check, one, scene_at = checks.check, checks.one, projections.at
    projection, by_revision = projections.projection, projections.by_revision
    actor, target = owner.get("actor"), owner.get("expectedTarget")
    begin, end = owner.get("preparedRevision"), owner.get("sceneEndResultRevision")
    start_sequence, end_sequence = owner.get("sceneStartSequence"), owner.get("sceneEndSequence")
    recovery = scalar["recovery"]
    prior_scene = scene_at(begin)
    complete_work = inventory_complete and all(
        projection.get(r.get("result", {}).get("revision"))
        for r in rows
        if r.get("result", {}).get("revision") < end
    )
    check(
        "prepared scene token",
        match(dict(phase="Initialize", waitToken=start_sequence), prior_scene),
        ordinal,
    )
    if scalar:
        check(
            "prepared recovery",
            match(
                recovery if recovery is not None else missing,
                prior_scene.get("reactionAmount", missing),
            ),
            ordinal,
        )
    phase_steps = {}
    phase_messages = {}
    transitions = []
    draw_count = 0
    opportunities = 0
    previous = prepared
    for row in rows:
        r = row.get("result") or {}
        revision = r.get("revision")
        if revision == begin:
            continue
        s = row.get("state") or {}
        es = scoped_events(row, end, end_sequence)
        check(
            "Submit/state session and result",
            match(
                dict(
                    result=dict(sessionId=session, failure=None),
                    state=dict(sessionId=session, revision=revision),
                ),
                row,
            ),
            ordinal,
            revision,
        )
        check("unique Submit revision", len(by_revision[revision]) == 1, ordinal, revision)
        previous_revision = (previous.get("result") or {}).get("revision")
        event_envelope(
            row,
            es,
            previous_revision,
            previous.get("result", {}).get("observationSequence"),
            ordinal,
            actor,
            target,
            checks,
        )
        current_scene = scene_at(revision) if revision < end else prior_scene
        before_healing = prior_scene.get("healing") or {}
        after_healing = current_scene.get("healing") or {}
        phase = prior_scene.get("phase")
        phase_messages.setdefault(phase, prior_scene.get("message"))
        logical = [e for e in es if e.get("Kind") == "scene-logical-step"]
        delivery = [e for e in es if e.get("Kind") == "scene-delivery"]
        check(
            "one scene service or delivery",
            None if not logical and not delivery else len(logical) + len(delivery) == 1,
            ordinal,
            revision,
        )
        if not logical and not delivery:
            complete_work = False
        for e in logical:
            opportunities += 1
            check(
                "actual caller before logical opportunity",
                None
                if previous_revision is None or revision != previous_revision + 1
                else match(before_healing.get("Caller", missing), e.get("Detail", missing)),
                ordinal,
                revision,
            )
            check(
                "logical opportunity has pending work",
                match(False, before_healing.get("LogicalComplete", missing)),
                ordinal,
                revision,
            )
            phase_steps.setdefault(phase, []).append(
                dict(
                    caller=e.get("Detail"),
                    remaining=before_healing.get("Remaining"),
                    timed=before_healing.get("AtTimedInput"),
                )
            )
        for e in delivery:
            if e.get("Detail") != "bsc10:input-before-vint":
                check(
                    "delivery follows completed logical work",
                    match(True, before_healing.get("LogicalComplete", missing)),
                    ordinal,
                    revision,
                )
            check(
                "delivery actual caller",
                match(before_healing.get("Caller") or phase, e.get("Detail", missing)),
                ordinal,
                revision,
            )
            if e.get("Detail") == "bsc10:input-before-vint":
                key_input = one(
                    "timed acknowledgement input",
                    [
                        i
                        for i in inputs
                        if i.get("resultStart", -1)
                        <= row.get("_index", -2)
                        < i.get("resultEnd", -1)
                    ],
                    ordinal,
                )
                check(
                    "timed input precedes fairy opportunity",
                    match(
                        dict(
                            action="confirm",
                            pressed=True,
                            ordinal=row.get("inputOrdinal"),
                            delivery=dict(kind="key", code=4194309),
                            before=dict(
                                sessionId=session,
                                revision=previous_revision,
                                mainSeed=previous.get("state", {}).get("mainSeed", missing),
                            ),
                            after=dict(
                                sessionId=session,
                                revision=revision,
                                mainSeed=s.get("mainSeed", missing),
                            ),
                        ),
                        key_input,
                    ),
                    ordinal,
                    revision,
                )
                check(
                    "actual timed-input gate",
                    match(True, before_healing.get("AtTimedInput", missing)),
                    ordinal,
                    revision,
                )
        for e in es:
            if e.get("Kind") == "scene-step-started":
                transitions.append(e.get("Detail"))
        previous_seed = (previous.get("state") or {}).get("mainSeed")
        expected_draws = []
        expected_fairy = copy.deepcopy(before_healing.get("Fairy"))
        expected_seed = previous_seed
        try:
            if (
                not before_healing
                or not after_healing
                or not logical
                and not delivery
                or previous_revision is None
                or revision != end
                and revision != previous_revision + 1
            ):
                raise KeyError("missing adjacent scene/service")
            if logical:
                caller = before_healing.get("Caller")
                stop_gate = {
                    "bsc0D:toggle-drain": dict(Control=2, ActiveCount=1, CleanupPending=False),
                    "ReinitializeSceneAfterSpell:wait": dict(
                        Control=0, ActiveCount=0, CleanupPending=True
                    ),
                    "bsc0D:restore-wait": dict(Control=0, ActiveCount=0, CleanupPending=False),
                }.get(caller)
                if stop_gate is not None:
                    check(
                        "source stop/cleanup gate",
                        match(stop_gate, expected_fairy),
                        ordinal,
                        revision,
                    )
                if caller == "ReinitializeSceneAfterSpell:wait":
                    expected_fairy["CleanupPending"] = False
                elif expected_fairy is not None and quarter is not None:
                    expected_fairy, expected_seed, expected_draws = fairy_source_step(
                        expected_fairy, int(previous_seed), quarter
                    )
                if (
                    before_healing.get("Caller") == "LoadSpellTileset:dma"
                    and before_healing.get("Remaining") == 1
                    and quarter is not None
                ):
                    expected_fairy, expected_seed, expected_draws = fairy_source_step(
                        None, int(previous_seed), quarter, True
                    )
            if current_scene.get("phase") == "SpellStop" and phase != "SpellStop":
                expected_fairy["Control"] = 2
            if quarter is None or previous_seed is None:
                raise KeyError("source/seed absent")
            check(
                "source fairy state transition",
                match(expected_fairy, after_healing.get("Fairy", missing)),
                ordinal,
                revision,
            )
            actual_draws = [e for e in es if e.get("Kind", "").startswith("rng-fairy-")]
            check(
                "source conditional fairy draws",
                ordered_kinds(expected_draws, actual_draws),
                ordinal,
                revision,
            )
            if revision < end:
                check(
                    "actual carried scene seed",
                    match(expected_seed, s.get("mainSeed", missing)),
                    ordinal,
                    revision,
                )
            draw_count += len(actual_draws)
        except (KeyError, TypeError, ValueError, IndexError):
            check("fairy transition operands unavailable", None, ordinal, revision)
        for e in es:
            if (
                e.get("Kind", "").startswith("rng-fairy-")
                and e.get("Before") is not None
                and e.get("RandomRange") is not None
            ):
                word, value = _rng_step(int(e["Before"]) >> 16, int(e["RandomRange"]) * 2)
                check(
                    "independent available fairy draw",
                    match(
                        dict(
                            After=(word << 16) | (int(e["Before"]) & 65535),
                            RandomValue=value >> 1,
                        ),
                        e,
                    ),
                    ordinal,
                    revision,
                )
        previous, prior_scene = row, current_scene
    return dict(
        steps=phase_steps,
        messages=phase_messages,
        transitions=transitions,
        complete=complete_work,
        scene=prior_scene,
        opportunities=opportunities,
        draws=draw_count,
    )
