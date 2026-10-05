"""Compare physical scene construction, persistent effects and projected reactions.

The independent source calculation predicts draws/strikes. This consumer checks
their causal scene boundaries and state effects using the selected evidence only.
"""

from bisect import bisect_right

from sf2tool.remake_h4.physical_checks import absent, number, ordered_match
from sf2tool.remake_h4.physical_source import source_action


def compare_occurrences(selected, events, event_rows, census, source, checks):
    """Append ordered checks; return occurrence summaries, unknowns and covered effects."""
    check, eq, precedes = checks.check, checks.eq, checks.precedes
    warps = selected["warpRecords"]
    occurrences, unknown = [], []
    physical_kinds = {
        "rng-" + name for name in ("dodge", "critical", "spread-1", "spread-2", "double", "counter")
    }
    effect_kinds = {
        "physical-first",
        "physical-second",
        "physical-counter",
        "hp",
        "dodge",
        "critical",
    }
    covered = set()
    for occurrence in census:
        seq = occurrence.get("sequence")
        index = occurrence.get("index")
        row = warps.get(index, {})
        state = row.get("state") or {}
        envelope = row.get("result") or {}
        end = (occurrence.get("end") or {}).get("sequence")
        first = occurrence.get("firstDrawSequence")
        end_context = occurrence.get("end") or {}
        end_index = end_context.get("index")
        end_row = warps.get(end_index, {})
        end_event = events.get(end)
        eq("end source result index", end_index, event_rows.get(end, absent), seq)
        if end_event:
            check(
                "scene completion follows preparation on both axes",
                number(occurrence.get("revision"))
                and number(end_event.get("Revision"))
                and end_event["Revision"] > occurrence["revision"]
                and end_event["Sequence"] > seq,
                seq,
            )
            check(
                "scene completion follows preparation result",
                number(index) and number(end_index) and end_index >= index,
                seq,
            )
            precedes(
                "scene completion inside its result clocks",
                {"revision": end_event["Revision"], "observationSequence": end_event["Sequence"]},
                end_row.get("result") or {},
            )
        eq(
            "preparation identity",
            dict(
                Kind="scene-prepared",
                Sequence=seq,
                Revision=occurrence.get("revision"),
                Actor=occurrence.get("actor"),
            ),
            events.get(seq, absent),
            seq,
        )
        eq("preparation Submit has no failure", None, envelope.get("failure", absent), seq)
        eq(
            "preparation result revision",
            occurrence.get("revision"),
            envelope.get("revision", absent),
            seq,
        )
        for draw in envelope.get("observations", []):
            if draw.get("Kind") in physical_kinds:
                eq(
                    "construction draw revision",
                    occurrence.get("revision"),
                    draw.get("Revision", absent),
                    seq,
                )
        eq("preparation source index", index, event_rows.get(seq, absent), seq)
        eq(
            "end identity",
            dict(
                Kind="scene-ended",
                Sequence=end,
                Revision=(occurrence.get("end") or {}).get("revision"),
                Actor=occurrence.get("actor"),
            ),
            events.get(end, absent),
            seq,
        )
        check(
            "physical scene axes ordered",
            first < seq < end if all(number(x) for x in (first, seq, end)) else None,
            seq,
        )
        if not all(number(x) for x in (first, seq, end)):
            continue
        within = [e for k, e in sorted(events.items()) if seq < k < end]
        effects = [e for e in within if e.get("Kind") in effect_kinds]
        covered.update(e["Sequence"] for e in effects)
        draws = [
            e
            for e in envelope.get("observations", [])
            if first <= e.get("Sequence", -1) < seq and e.get("Kind") in physical_kinds
        ]
        before = (warps.get(index - 1) or {}).get("state") or {}
        check("immediate preparation predecessor", True if before else None, seq)
        actor = (occurrence.get("actor") or {}).get("Value")
        target = (occurrence.get("target") or {}).get("Value")
        actors = {a.get("id"): a for a in state.get("actors") or []}
        prior_actors = {a.get("id"): a for a in before.get("actors") or []}
        for actor_id in (actor, target):
            current_actor = actors.get(actor_id, {})
            check(
                "physical actor alive " + str(actor_id),
                current_actor["hp"] > 0 if number(current_actor.get("hp")) else None,
                seq,
            )
            check(
                "prior actor operands retained " + str(actor_id),
                True if actor_id in prior_actors else None,
                seq,
            )
            eq(
                "deferred physical vitals and operands " + str(actor_id),
                {
                    k: prior_actors[actor_id][k]
                    for k in ("hp", "attack", "defense", "status", "items")
                    if k in prior_actors[actor_id]
                }
                if actor_id in prior_actors
                else {},
                current_actor,
                seq,
            )
            check(
                "required live physical operands " + str(actor_id),
                True
                if all(
                    current_actor.get(k) is not None
                    for k in ("hp", "attack", "defense", "status", "items", "mover", "x", "y")
                )
                else None,
                seq,
            )
            if source and actor_id in source["profiles"]:
                eq(
                    "matched current source mover " + actor_id,
                    source["profiles"][actor_id]["mover"].replace("_", "").lower(),
                    str(current_actor["mover"]).lower() if "mover" in current_actor else absent,
                    seq,
                )
        positions = {
            a: dict(x=prior_actors[a].get("x"), y=prior_actors[a].get("y"))
            for a in (actor, target)
            if a in prior_actors
        }
        for movement in envelope.get("observations", []):
            moving_actor = (movement.get("Actor") or {}).get("Value")
            if (
                movement.get("Kind") != "movement"
                or moving_actor not in positions
                or movement.get("Sequence", seq) >= seq
            ):
                continue
            eq(
                "physical movement starts at prior placement",
                positions[moving_actor],
                {
                    "x": (movement.get("From") or {}).get("X"),
                    "y": (movement.get("From") or {}).get("Y"),
                },
                seq,
            )
            positions[moving_actor] = {
                "x": (movement.get("To") or {}).get("X"),
                "y": (movement.get("To") or {}).get("Y"),
            }
        for actor_id, position in positions.items():
            eq(
                "physical range uses committed placement " + actor_id,
                position,
                actors.get(actor_id, {}),
                seq,
            )
        ordinal = occurrence.get("inputOrdinal")
        eq("preparation causal input ordinal", ordinal, row.get("inputOrdinal", absent), seq)
        owners = [i for i in selected["inputRecords"].values() if i.get("ordinal") == ordinal]
        check("preparation input occurrence retained", bool(owners) or None, seq)
        if actor and actor.startswith("ally-"):
            committing = [
                i for i in owners if i.get("resultStart", -1) <= index < i.get("resultEnd", -1)
            ]
            check(
                "player physical input-to-Submit", len(committing) == 1 if committing else None, seq
            )
            if committing:
                eq(
                    "player commit identity",
                    dict(action="confirm", pressed=True),
                    committing[0],
                    seq,
                )
                eq(
                    "player selected actor",
                    actor,
                    (committing[0].get("before") or {}).get("actor", absent),
                    seq,
                )
                eq(
                    "player before clocks",
                    {
                        k: before[k]
                        for k in ("revision", "observationSequence", "mainSeed")
                        if k in before
                    },
                    committing[0].get("before", absent),
                    seq,
                )
                eq(
                    "player after clocks",
                    {
                        k: state[k]
                        for k in ("sessionId", "revision", "observationSequence", "mainSeed")
                        if k in state
                    },
                    committing[0].get("after", absent),
                    seq,
                )
        seed = before.get("mainSeed")
        for event in envelope.get("observations", []):
            if event.get("Sequence", first) >= first:
                break
            if str(event.get("Kind", "")).startswith("rng-") and event.get("After") is not None:
                seed = event["After"]
        if draws and draws[0].get("Sequence") == first:
            eq("matched physical seed input", seed, draws[0].get("Before", absent), seq)
        else:
            check("physical construction draws", None, seq)
        for event in effects:
            effect_row = warps[event_rows[event["Sequence"]]]
            previous = (warps.get(event_rows[event["Sequence"]] - 1) or {}).get("state") or {}
            submit_events = effect_row["result"].get("observations", [])
            prior_phases = [
                e
                for e in submit_events
                if e.get("Kind") == "scene-step-completed"
                and e.get("Sequence", -1) < event["Sequence"]
            ]
            next_phases = [
                e
                for e in submit_events
                if e.get("Kind") == "scene-step-started"
                and e.get("Sequence", -1) > event["Sequence"]
            ]
            marker = event["Kind"].startswith("physical-")
            markers = [
                e
                for e in effects
                if e.get("Kind", "").startswith("physical-") and e["Sequence"] <= event["Sequence"]
            ]
            eq(
                "physical effect Submit revision",
                effect_row["result"].get("revision"),
                event.get("Revision", absent),
                seq,
            )
            if prior_phases and next_phases:
                for label, phase, name in (
                    (
                        "preceding",
                        prior_phases[-1],
                        "ActionMessage" if marker else "ActionAnimation",
                    ),
                    ("following", next_phases[0], "ActionAnimation" if marker else "Reaction"),
                ):
                    eq(
                        "physical effect " + label + " command",
                        dict(Detail=name, Revision=event.get("Revision")),
                        phase,
                        seq,
                    )
                if markers:
                    eq(
                        "physical preceding command actor",
                        markers[-1].get("Actor"),
                        prior_phases[-1].get("Actor", absent),
                        seq,
                    )
                    eq(
                        "physical following command actor",
                        markers[-1].get("Actor"),
                        next_phases[0].get("Actor", absent),
                        seq,
                    )
                else:
                    check("physical action marker retained", None, seq)
            else:
                check("physical effect command boundaries", None, seq)
            if event["Kind"] == "hp":
                affected = (event.get("Actor") or {}).get("Value")
                for label, snapshot, value in (
                    ("before", previous, event.get("Before")),
                    ("after", effect_row.get("state") or {}, event.get("After")),
                ):
                    actual_actor = next(
                        (a for a in snapshot.get("actors") or [] if a.get("id") == affected), {}
                    )
                    eq("persistent HP " + label, value, actual_actor.get("hp", absent), seq)
            if event["Kind"] == "dodge":
                affected = (event.get("Actor") or {}).get("Value")
                old = next((a for a in previous.get("actors") or [] if a.get("id") == affected), {})
                new = next(
                    (
                        a
                        for a in (effect_row.get("state") or {}).get("actors") or []
                        if a.get("id") == affected
                    ),
                    {},
                )
                eq(
                    "dodge preserves persistent HP",
                    old.get("hp", absent),
                    new.get("hp", absent),
                    seq,
                )
            eq(
                "effect Submit has no failure",
                None,
                effect_row["result"].get("failure", absent),
                seq,
            )
        carried = [
            e
            for e in envelope.get("observations", [])
            if str(e.get("Kind", "")).startswith("rng-") and e.get("After") is not None
        ]
        if carried:
            eq(
                "preparation carried seed state",
                carried[-1]["After"],
                state.get("mainSeed", absent),
                seq,
            )
        predicted = None
        if source and actor in actors and target in actors and number(seed):
            try:
                predicted = source_action(source, actors, actor, target, int(seed))
            except (ValueError, KeyError, TypeError, IndexError) as error:
                unknown.append(str(error))
        if predicted is None:
            check("source rule operands complete", None, seq)
            continue
        check("source physical range", predicted["rangeLegal"], seq)
        check(
            "eligible ordered physical RNG and seed effects",
            ordered_match(predicted["draws"], draws),
            seq,
        )
        expected_effects = []
        for strike in predicted["strikes"]:
            expected_effects.append(
                dict(
                    Kind=strike["kind"],
                    Actor={"Value": strike["actor"]},
                    Target={"Value": strike["target"]},
                )
            )
            if strike["critical"]:
                expected_effects.append(
                    dict(
                        Kind="critical",
                        Actor={"Value": strike["actor"]},
                        Target={"Value": strike["target"]},
                    )
                )
            expected_effects.append(
                dict(
                    Kind="dodge" if strike["dodge"] else "hp",
                    Actor={"Value": strike["target"]},
                    Before=None if strike["dodge"] else strike["beforeHp"],
                    After=None if strike["dodge"] else strike["afterHp"],
                )
            )
        check(
            "ordered first/second/counter and HP effects",
            ordered_match(expected_effects, effects),
            seq,
        )
        hp_events = [e for e in effects if e.get("Kind") == "hp"]
        expected_hp = [e for e in expected_effects if e["Kind"] == "hp"]
        if hp_events and len(hp_events) == len(expected_hp):
            last_state_sequence = warps[event_rows[hp_events[-1]["Sequence"]]]["result"][
                "observationSequence"
            ]
            affected = {(e.get("Actor") or {}).get("Value") for e in hp_events}
            for boundary_row in warps.values():
                boundary_state = boundary_row.get("state") or {}
                clock = boundary_state.get("observationSequence")
                if not number(clock) or not seq <= clock <= last_state_sequence:
                    continue
                live = {a.get("id"): a for a in boundary_state.get("actors") or []}
                for target_id in affected:
                    expected_hp_value = actors.get(target_id, {}).get("hp", absent)
                    for actual_effect, expected_effect in zip(hp_events, expected_hp, strict=True):
                        if (
                            actual_effect["Sequence"] <= clock
                            and expected_effect["Actor"]["Value"] == target_id
                        ):
                            expected_hp_value = expected_effect["After"]
                    eq(
                        "HP deferred until physical command and retained afterward",
                        expected_hp_value,
                        live.get(target_id, {}).get("hp", absent),
                        seq,
                    )
        message_starts = [
            e["Sequence"]
            for e in within
            if e.get("Kind") == "scene-step-started" and e.get("Detail") == "ActionMessage"
        ]
        eq("physical action message phases", len(predicted["strikes"]), len(message_starts), seq)
        projections = [
            p
            for p in selected["sceneObservations"].values()
            if number((p.get("scene") or {}).get("waitToken"))
            and seq <= p["scene"]["waitToken"] < end
        ]
        for strike in predicted["strikes"]:
            check(
                "strike has actual scene projection " + strike["kind"],
                any(p["scene"].get("actionKind") == strike["kind"] for p in projections) or None,
                seq,
            )
        for projection in projections:
            scene = projection["scene"]
            token = scene["waitToken"]
            token_event = events.get(token)
            if token_event:
                check(
                    "scene token names a phase boundary",
                    token_event.get("Kind") in ("scene-prepared", "scene-step-started"),
                    seq,
                )
                expected_phase = (
                    "Initialize"
                    if token_event.get("Kind") == "scene-prepared"
                    else token_event.get("Detail")
                )
                eq("scene phase token", expected_phase, scene.get("phase", absent), seq)
                check(
                    "scene token before projected clocks",
                    token_event["Revision"] <= projection.get("revision", -1)
                    and token_event["Sequence"] <= projection.get("observationSequence", -1),
                    seq,
                )
            else:
                check("scene phase token retained", None, seq)
            strike_index = max(0, bisect_right(message_starts, token) - 1)
            if strike_index >= len(predicted["strikes"]):
                check("extra physical scene action phase", False, seq)
                continue
            strike = predicted["strikes"][strike_index]
            eq(
                "scene strike and reaction",
                dict(
                    actionKind=strike["kind"],
                    reactionKind="Dodge" if strike["dodge"] else "Damage",
                    reactionAmount=strike["damage"],
                    displayedAlly=actor if actor.startswith("ally-") else target,
                    displayedEnemy=target if actor.startswith("ally-") else actor,
                ),
                scene,
                seq,
            )
        occurrences.append(
            dict(sequence=seq, actor=actor, target=target, strikes=predicted["strikes"])
        )
    return occurrences, unknown, covered
