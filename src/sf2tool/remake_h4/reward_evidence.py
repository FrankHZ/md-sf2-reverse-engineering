"""Selected reward census and causal input/result/scene-projection evidence.

The caller keeps channel/SQLite ownership. This function reads selected rows while
that transport is open and returns detached indexes; it opens no resources itself.
"""

from sf2tool.remake_h4.reward_checks import absent, match, number
from sf2tool.remake_h4_reference import UPSTREAM


def select_evidence(actual, context, checks):
    """Append evidence checks; return selected channels, events, owners and census."""
    check, eq = checks.check, checks.eq
    clocks, precedes, join = checks.clocks, checks.precedes, checks.join
    session = checks.session
    eq("scope", "retained-keyboard-A-reward", context.get("scope", absent))
    eq("source pin", UPSTREAM, context.get("upstream", absent))
    check("independent session", bool(session) or None)
    receipts = context.get("selectionReceipts") or []
    check("selection receipts available", bool(receipts) or None)
    for receipt in receipts:
        eq("completed immutable selection", dict(sourceUnchanged=True, failure=None), receipt)
    selected = {}
    for channel in ("warpRecords", "inputRecords", "sceneObservations", "samples"):
        wanted = set((context.get("indices") or {}).get(channel, []))
        records, order = {}, []
        for index, row in enumerate(actual.get(channel, [])):
            index = row.get("_index", index)
            if index in wanted:
                check(channel + " unique index", index not in records)
                records[index] = dict(row, _index=index)
                order.append(index)
        check(channel + " selected indices", True if wanted and wanted == set(records) else None)
        check(channel + " source order", order == sorted(set(order)))
        selected[channel] = records
    warps, inputs = selected["warpRecords"], list(selected["inputRecords"].values())
    events, event_rows = {}, {}
    prior_result = None
    for index, row in warps.items():
        envelope, state = row.get("result") or {}, row.get("state") or {}
        clocks("result", envelope)
        if prior_result:
            precedes("result source chronology", prior_result, envelope)
        prior_result = envelope
        if state:
            clocks("state", state)
            join("result/state identity", envelope, state)
        # Range rejections are an accepted physical dependency, not reward commits.
        kinds = {e.get("Kind") for e in envelope.get("observations", [])}
        if kinds & {
            "exp",
            "gold",
            "level",
            "after-turn",
            "kills",
            "battle-outcome",
            "after-battle-join",
            "battle-unlock-cleared",
            "battle-completed-set",
            "exploration-return-started",
            "map-transferred",
            "battle-returned",
        }:
            eq("reward result accepted", None, envelope.get("failure", absent), index)
        previous = None
        for event in envelope.get("observations", []):
            sequence = event.get("Sequence")
            check(
                "event nonnegative clocks",
                number(sequence) and number(event.get("Revision")),
                index,
            )
            if previous:
                check(
                    "events source chronology",
                    all(
                        number(x.get(k))
                        for x in (event, previous)
                        for k in ("Sequence", "Revision")
                    )
                    and event["Sequence"] > previous["Sequence"]
                    and event["Revision"] >= previous["Revision"],
                    index,
                )
            previous = event
            check(
                "event within result clocks",
                number(sequence)
                and number(event.get("Revision"))
                and number(envelope.get("observationSequence"))
                and number(envelope.get("revision"))
                and sequence <= envelope["observationSequence"]
                and event["Revision"] <= envelope["revision"],
                index,
            )
            if sequence in events:
                eq("duplicate event payload", events[sequence], event, sequence)
            else:
                events[sequence], event_rows[sequence] = event, index
    for n, row in enumerate(inputs):
        before, after = row.get("before") or {}, row.get("after") or {}
        clocks("input before", before)
        clocks("input after", after)
        precedes("input span clocks", before, after)
        start, end = row.get("resultStart"), row.get("resultEnd")
        check("input result span", number(start) and number(end) and start <= end)
        if n:
            precedes("input chronology", inputs[n - 1].get("after") or {}, before)
            check(
                "input result chronology",
                start >= inputs[n - 1].get("resultEnd", 0) if number(start) else False,
            )
        if start == end:
            join("empty input span", before, after)
        for label, snapshot, boundary in (
            ("before", before, start - 1 if number(start) else None),
            ("after", after, end - 1 if number(end) else None),
        ):
            if boundary in warps:
                join("input " + label + " result", snapshot, warps[boundary].get("result") or {})
    missing_inputs = set((context.get("indices") or {}).get("inputRecords", [])) - set(
        selected["inputRecords"]
    )
    for index, row in warps.items():
        eligible = [x for x in inputs if number(x.get("resultStart")) and x["resultStart"] <= index]
        later = [x for x in inputs if number(x.get("resultStart")) and x["resultStart"] > index]
        if not eligible:
            continue  # Pre-battle sparse provenance is not a physical input delivery claim.
        owner = eligible[-1]
        next_index = (
            later[0]["_index"]
            if later
            else max((context.get("indices") or {}).get("inputRecords", [-1])) + 1
        )
        ambiguous = any(owner["_index"] < x < next_index for x in missing_inputs)
        if not ambiguous:
            eq("latest causal input", owner.get("ordinal"), row.get("inputOrdinal", absent), index)
        else:
            check("latest causal input", None, index)
        envelope = row.get("result") or {}
        precedes("input before result", owner.get("before") or {}, envelope)
        if number(owner.get("resultEnd")):
            if index < owner["resultEnd"]:
                precedes("direct result within input", envelope, owner.get("after") or {})
            else:
                precedes("automatic result after input", owner.get("after") or {}, envelope)
        if later:
            precedes("result before next input", envelope, later[0].get("before") or {})

    semantic = {
        "exp",
        "gold",
        "exp-threshold",
        "level",
        "kills",
        "defeats",
        "death-cleanup",
        "after-turn",
        "action-committed",
        "battle-selected",
        "battle-initialized",
        "battle-loaded",
        "battle-outcome",
        "battle-returned",
        "scene-prepared",
        "scene-ended",
        "hp",
        "mp",
        "heal",
        "physical-first",
        "physical-second",
        "physical-counter",
        "dodge",
        "critical",
        "field-death-started",
        "field-death-ended",
        "after-battle-join",
        "battle-unlock-cleared",
        "battle-completed-set",
        "exploration-return-started",
        "map-transferred",
    }

    def relevant(kind):
        return kind in semantic or bool(
            kind
            and kind.startswith(
                ("rng-exp", "rng-growth", "level-", "status", "learn", "poison", "regen")
            )
        )

    census = context.get("census") or []
    check("independent census", bool(census) or None)
    wanted_events = {row[2]: row for row in census}
    check(
        "unique ordered census",
        len(wanted_events) == len(census) and [x[2] for x in census] == sorted(wanted_events),
    )
    for sequence, event in events.items():
        if relevant(event.get("Kind")):
            check("no unaccounted reward event", sequence in wanted_events, sequence)
    for index, revision, sequence, kind, who, target in census:
        event = events.get(sequence)
        eq(
            "census event",
            dict(
                Revision=revision,
                Sequence=sequence,
                Kind=kind,
                Actor=None if who is None else {"Value": who},
                Target=None if target is None else {"Value": target},
            ),
            event if event is not None else absent,
            sequence,
        )
        if event is not None:
            eq("owning result index", index, event_rows[sequence], sequence)

    reward_phases = {"Reward", "RewardMessage", "GrowthMessage", "GoldMessage"}
    projected_tokens = set()
    for projection in selected["sceneObservations"].values():
        scene = projection.get("scene") or {}
        eq("selected scene session", session, projection.get("sessionId", absent))
        eq("selected scene error", None, scene.get("error", absent))
        if scene.get("phase") not in reward_phases:
            continue
        clocks("reward projection", projection)
        token = scene.get("waitToken")
        projected_tokens.add(token)
        token_event = events.get(token)
        eq(
            "reward phase token",
            dict(Kind="scene-step-started", Detail=scene["phase"]),
            token_event if token_event else absent,
            token,
        )
        if token_event:
            precedes(
                "phase token precedes projection",
                dict(
                    revision=token_event.get("Revision"),
                    observationSequence=token_event.get("Sequence"),
                ),
                projection,
            )
        matching = [
            row
            for row in warps.values()
            if all(
                (row.get("result") or {}).get(k) == projection.get(k)
                for k in ("sessionId", "revision", "observationSequence")
            )
        ]
        check("reward projection result retained", bool(matching) or None, token)
        for row in warps.values():
            envelope = row.get("result") or {}
            if envelope.get("observationSequence") == projection.get("observationSequence"):
                eq(
                    "reward projection revision join",
                    envelope.get("revision"),
                    projection.get("revision", absent),
                    token,
                )
        for row in matching:
            eq(
                "reward projection input join",
                row.get("inputOrdinal"),
                projection.get("inputOrdinal", absent),
                token,
            )
        stage = projection.get("projectionStage")
        check(
            "reward projection stage",
            stage in ("host-poll", "signal-before-Present") if stage is not None else None,
            token,
        )
        if stage == "host-poll":
            phases = [
                k
                for k, e in events.items()
                if number(k)
                and k <= projection.get("observationSequence", -1)
                and e.get("Kind") in ("scene-prepared", "scene-step-started")
            ]
            current = max(phases) if phases else absent
            if (
                token_event is None
                and number(token)
                and (current is absent or current < token)
                and token <= projection.get("observationSequence", -1)
            ):
                current = absent  # A missing newer start is not an observed stale token.
            eq("reward host poll current token", current, token, token)
        if stage == "signal-before-Present":
            completions = [
                e
                for row in matching
                for e in row.get("result", {}).get("observations", [])
                if e.get("Kind") == "scene-step-completed"
            ]
            check(
                "reward projection completion receipt",
                any(e.get("Detail") == scene["phase"] for e in completions)
                if completions
                else None,
                token,
            )
            for completion in completions:
                if completion.get("Detail") != scene["phase"]:
                    continue
                # The signal is emitted before Present replaces the old view. Its
                # token owns the completed phase, not the next phase in this result.
                preceding = [
                    e
                    for e in events.values()
                    if number(e.get("Sequence"))
                    and e["Sequence"] < completion["Sequence"]
                    and e.get("Kind")
                    in (
                        "scene-prepared",
                        "scene-step-started",
                        "scene-step-completed",
                        "scene-ended",
                    )
                ]
                start = max(preceding, key=lambda e: e["Sequence"]) if preceding else None
                if (
                    token_event is None
                    and number(token)
                    and (start is None or start["Sequence"] < token)
                    and token < completion["Sequence"]
                ):
                    start = None
                eq(
                    "reward completion owns exact phase token",
                    dict(
                        Kind="scene-step-started",
                        Sequence=token,
                        Detail=scene["phase"],
                        Actor=completion.get("Actor"),
                    ),
                    start if start else absent,
                    token,
                )
                completion_clocks = dict(
                    revision=completion.get("Revision"),
                    observationSequence=completion.get("Sequence"),
                )
                precedes(
                    "completion precedes before-Present projection", completion_clocks, projection
                )
                if token_event:
                    precedes(
                        "completed token precedes completion",
                        dict(
                            revision=token_event.get("Revision"),
                            observationSequence=token_event.get("Sequence"),
                        ),
                        completion_clocks,
                    )
                occurrences = [
                    s
                    for s in context.get("scenes", [])
                    if number(s.get("sequence"))
                    and number((s.get("end") or {}).get("sequence"))
                    and s["sequence"] < completion["Sequence"] < s["end"]["sequence"]
                ]
                check(
                    "reward completion scene occurrence",
                    True if len(occurrences) == 1 else False if occurrences else None,
                    token,
                )
                for occurrence in occurrences:
                    if token_event:
                        precedes(
                            "completed token belongs to scene occurrence",
                            dict(
                                revision=occurrence.get("revision"),
                                observationSequence=occurrence.get("sequence"),
                            ),
                            dict(
                                revision=token_event.get("Revision"),
                                observationSequence=token_event.get("Sequence"),
                            ),
                        )
                    end = occurrence.get("end") or {}
                    precedes(
                        "completion within scene occurrence",
                        completion_clocks,
                        dict(revision=end.get("revision"), observationSequence=end.get("sequence")),
                    )
            eq("reward completion delivered", True, scene.get("completed", absent), token)
        host_inputs = [
            i
            for i in inputs
            if number(i.get("hostUpdate"))
            and number(projection.get("hostUpdate"))
            and i["hostUpdate"] <= projection["hostUpdate"]
        ]
        if host_inputs:
            owner = host_inputs[-1]
            next_inputs = [i for i in inputs if i["_index"] > owner["_index"]]
            next_index = (
                next_inputs[0]["_index"]
                if next_inputs
                else max((context.get("indices") or {}).get("inputRecords", [-1])) + 1
            )
            ambiguous = any(owner["_index"] < i < next_index for i in missing_inputs)
            check(
                "reward projection latest host input",
                None
                if ambiguous
                else match(owner.get("ordinal"), projection.get("inputOrdinal", absent)),
                token,
            )
            precedes("reward projection after input", owner.get("before") or {}, projection)
            if next_inputs:
                precedes(
                    "reward projection before next input",
                    projection,
                    next_inputs[0].get("before") or {},
                )
        else:
            check("reward projection host input retained", None, token)
    for sequence, event in events.items():
        if event.get("Kind") == "scene-step-started" and event.get("Detail") in reward_phases:
            check(
                "reward phase projection exists",
                True if sequence in projected_tokens else None,
                sequence,
            )

    return selected, events, event_rows, census
