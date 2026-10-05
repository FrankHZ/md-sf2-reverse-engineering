"""AI evidence selection, causal joins and persistent last-writer continuity.

The caller retains ownership of channel iterables and their open transport. This
module keeps selected records and indexes only, with no resource or cache lifecycle.
"""

from sf2tool.remake_h4.ai_checks import absent, event_clock, merge, number, who
from sf2tool.remake_h4_reference import UPSTREAM


def select_evidence(actual, context, checks, producer):
    """Return selected rows, events, owning result indexes and independent census."""
    check, eq, clocks, before = checks.check, checks.eq, checks.clocks, checks.before
    semantic = {
        "regions-tested",
        "regions-tested-cleared",
        "region-program-none",
        "spawn-modes-admitted",
        "activation-word",
        "movement",
        "stay-selected",
        "player-control",
        "dead-entry-skipped",
        "after-turn",
        "action-committed",
        "scene-prepared",
        "scene-ended",
        "battle-selected",
        "battle-initialized",
        "battle-loaded",
        "battle-outcome",
        "battle-returned",
    }

    def relevant(kind):
        return kind in semantic or bool(
            kind and kind.startswith(("ai-", "thinking-", "source-standby", "battle-movement-"))
        )

    eq("declared composed scope", "retained-keyboard-A-ai-composed", context.get("scope", absent))
    eq("source pin", UPSTREAM, context.get("upstream", absent))
    eq("retained producer", producer, context.get("producer", absent))
    eq(
        "completed immutable supplement",
        dict(failure=None, sourceUnchanged=True),
        context.get("selectionReceipt", absent),
    )
    rows = {}
    for position, row in enumerate(actual.get("warpRecords", [])):
        index = row.get("_index", position)
        if index in (context.get("indices") or {}).get("warpRecords", []):
            check("unique original result index", index not in rows, index)
            rows[index] = row
    check(
        "selected result coverage",
        True
        if set(rows) == set((context.get("indices") or {}).get("warpRecords", [])) and rows
        else None,
    )
    check("selected source order", list(rows) == sorted(rows))
    for row in context.get("actionRecords") or []:
        index = row.get("_index")
        if not number(index):
            check("reused action original index", None if index is None else False)
            continue
        if index in rows:
            eq(
                "reused action envelope agrees",
                rows[index].get("result"),
                row.get("result", absent),
                index,
            )
        else:
            rows[index] = row
    rows = dict(sorted(rows.items()))
    events = {}
    owners = {}
    prior = None
    for index, row in rows.items():
        envelope = row.get("result") or {}
        state = row.get("state") or {}
        clocks("result", envelope)
        if prior:
            before("result chronology", prior, envelope)
        prior = envelope
        if state:
            clocks("state", state)
            eq(
                "result/state identity",
                {
                    k: envelope.get(k, absent)
                    for k in ("sessionId", "revision", "observationSequence")
                },
                state,
                index,
            )
        previous = None
        for e in envelope.get("observations", []):
            seq = e.get("Sequence")
            ec = event_clock(e)
            check(
                "event clocks",
                merge(
                    [None if e.get(k) is None else number(e[k]) for k in ("Sequence", "Revision")]
                ),
                index,
            )
            before("event within owning result", ec, envelope)
            if not number(seq):
                continue
            if previous:
                before("ordered event revisions", event_clock(previous), ec)
                check(
                    "strict event sequence",
                    seq > previous["Sequence"]
                    if number(seq) and number(previous.get("Sequence"))
                    else False,
                    index,
                )
            previous = e
            if seq in events:
                eq("duplicate payload", events[seq], e, seq)
            else:
                events[seq] = e
                owners[seq] = index
        if any(
            str(e.get("Kind", "")).startswith(
                ("ai-", "thinking-", "source-standby", "regions-tested")
            )
            or (
                str(e.get("Kind", "")).startswith("battle-movement-")
                and e.get("Detail") == "Automatic"
            )
            or (
                e.get("Kind")
                in {
                    "movement",
                    "stay-selected",
                    "scene-prepared",
                    "physical-first",
                    "scene-ended",
                    "after-turn",
                    "action-committed",
                }
                and str(who(e) or "").startswith("enemy-")
            )
            for e in envelope.get("observations", [])
        ):
            eq(
                "AI decision or required consumer accepted",
                None,
                envelope.get("failure", absent),
                index,
            )
    census = context.get("census") or []
    declared = {r[2]: r for r in census if r[3] != "FAILED-RESULT"}
    check("independent AI census", True if declared else None)
    check("unique semantic census", len(declared) == sum(r[3] != "FAILED-RESULT" for r in census))
    for seq, e in events.items():
        if relevant(e.get("Kind")):
            check("no unaccounted semantic event", seq in declared, seq)
    for index, revision, seq, kind, actor, target in census:
        if kind == "FAILED-RESULT":
            eq(
                "retained failure census",
                actor,
                (rows.get(index, {}).get("result") or {}).get("failure", absent),
                index,
            )
            continue
        eq(
            "census event identity",
            dict(
                Sequence=seq,
                Revision=revision,
                Kind=kind,
                Actor=None if actor is None else {"Value": actor},
                Target=None if target is None else {"Value": target},
            ),
            events.get(seq, absent),
            seq,
        )
        if seq in events:
            eq(
                "semantic owning envelope",
                True,
                any(
                    x == events[seq]
                    for x in (rows.get(index, {}).get("result") or {}).get("observations", [])
                )
                if index in rows
                else absent,
                seq,
            )
    inputs = context.get("inputs") or []
    check("causal inputs retained", bool(inputs) or None)
    expected_inputs = context.get("inputIndices")
    missing_inputs = set(expected_inputs or []) - {i.get("_index") for i in inputs}
    check("input census retained", True if expected_inputs else None)
    if expected_inputs:
        eq("input census", expected_inputs, [i.get("_index", absent) for i in inputs])
    for n, i in enumerate(inputs):
        start = i.get("before") or {}
        end = i.get("after") or {}
        clocks("input before", start)
        clocks("input after", end)
        before("input clocks", start, end)
        if n:
            before("input chronology", inputs[n - 1].get("after") or {}, start)
        check(
            "input span",
            number(i.get("resultStart"))
            and number(i.get("resultEnd"))
            and i["resultStart"] <= i["resultEnd"],
        )
    for index, row in rows.items():
        earlier = [i for i in inputs if number(i.get("resultStart")) and i["resultStart"] <= index]
        if not earlier:
            continue
        i = earlier[-1]
        envelope = row.get("result") or {}
        owner = row.get("inputOrdinal", absent)
        next_input_index = min(
            (x.get("_index", float("inf")) for x in inputs if x.get("resultStart", -1) > index),
            default=float("inf"),
        )
        input_gap = any(i.get("_index", -1) < k < next_input_index for k in missing_inputs)
        eq("latest causal input", absent if input_gap else i.get("ordinal", absent), owner, index)
        if input_gap:
            continue
        if index in (context.get("indices") or {}).get("warpRecords", []):
            eq(
                "direct versus automatic delivery",
                index < i.get("resultEnd", 0),
                row.get("inputDelivery", absent),
                index,
            )
        before("input precedes result", i.get("before") or {}, envelope)
        if index < i.get("resultEnd", 0):
            before("direct input encloses result", envelope, i.get("after") or {})
        else:
            before("automatic result follows input", i.get("after") or {}, envelope)
        later = [x for x in inputs if number(x.get("resultStart")) and x["resultStart"] > index]
        if later:
            before("result before next input", envelope, later[0].get("before") or {})
    return rows, events, owners, census


def check_state_continuity(actual, context, rows, events, census, source, checks):
    """Bind source declarations and carry thinking/memory/regions across selected gaps."""
    eq, clocks = checks.eq, checks.clocks
    samples = {r.get("_index", n): r for n, r in enumerate(actual.get("samples") or [])}
    initial = (samples.get(context.get("battleSample")) or {}).get("state") or {}
    clocks("initial battle", initial)
    if source:
        # Adapter surface names identify the original terrain IDs, including the padding sentinel.
        surfaces = {
            0: "Impassable",
            8: "Impassable",
            1: "Open",
            2: "Open",
            3: "Brush",
            4: "Deep",
            5: "Rough",
            6: "Rough",
            7: "Barrier",
            255: "Barrier",
        }
        eq(
            "source initial terrain",
            [surfaces[v] for v in source["terrain"]],
            initial.get("terrain", absent),
        )
        eq("source battle viewport", dict(mapWidth=16, mapHeight=20), initial)
    # Match actual local state to independently parsed profiles; no route/round quota.
    for index, row in rows.items():
        if index not in (context.get("indices") or {}).get("warpRecords", []):
            continue
        roster = (row.get("state") or {}).get("actors")
        if source and roster is not None:
            eq(
                "complete source actor roster",
                sorted(source["profiles"]),
                sorted(a.get("id", "") for a in roster),
                index,
            )
            eq("battle identity", "map57", row["state"].get("map", absent), index)
        for a in roster or []:
            name = a.get("id")
            profile = (source or {}).get("profiles", {}).get(name)
            if not profile:
                continue
            eq(
                "source mover",
                profile["mover"].lower(),
                str(a["mover"]).lower() if "mover" in a else absent,
                index,
            )
            if name.startswith("enemy-"):
                pr, sr, word = source["orders"][name]
                eq(
                    "source AI declaration",
                    dict(
                        anchorX=profile["placement"][0],
                        anchorY=profile["placement"][1],
                        primaryOrder=255,
                        secondaryOrder=255,
                        commandset=word >> 4,
                    ),
                    a,
                    index,
                )
    # All thinking/memory writes are in the independent census. Carry their last writer
    # across selected gaps, including no-op and non-AI rows, rather than trusting a later
    # caller snapshot as a fresh memory authority.
    memory_chain = {
        x.get("actor"): dict(
            aiMemory=x.get("memory", absent), lastTarget=x.get("lastTarget", absent)
        )
        for x in initial.get("aiMemory") or []
    }
    live_image = initial.get("thinkingSeed", absent)
    live_regions = initial.get("regionsTested", absent)
    last_sequence = initial.get("observationSequence")
    if number(last_sequence):
        for index, row in rows.items():
            end = (row.get("result") or {}).get("observationSequence")
            if not number(end) or end < last_sequence:
                continue
            for _, _, seq, kind, actor, _ in sorted(census, key=lambda r: r[2]):
                if not last_sequence < seq <= end:
                    continue
                event = events.get(seq, {})
                if kind in ("regions-tested", "regions-tested-cleared"):
                    if kind == "regions-tested-cleared":
                        eq(
                            "continuous tested regions last writer",
                            live_regions,
                            event.get("Before", absent),
                            seq,
                        )
                        eq("source tested regions clear", 0, event.get("After", absent), seq)
                    live_regions = event.get("After", absent)
                if kind == "thinking-rng":
                    eq(
                        "continuous thinking last writer",
                        live_image,
                        event.get("Before", absent),
                        seq,
                    )
                    live_image = event.get("After", absent)
                if kind in ("ai-memory", "ai-target"):
                    if kind == "ai-memory":
                        eq(
                            "continuous memory last writer",
                            memory_chain.get(actor, {}).get("aiMemory", absent),
                            event.get("Before", absent),
                            seq,
                        )
                    field = "aiMemory" if kind == "ai-memory" else "lastTarget"
                    memory_chain.setdefault(actor, {})[field] = (
                        event.get("After", absent)
                        if kind == "ai-memory"
                        else who(event, "Target")
                        if event
                        else absent
                    )
            last_sequence = end
            state = row.get("state") or {}
            if state.get("actors") and index in (context.get("indices") or {}).get(
                "warpRecords", []
            ):
                eq(
                    "thinking image through retained gaps",
                    live_image,
                    state.get("thinkingSeed", absent),
                    index,
                )
                eq(
                    "tested regions delivered through retained gaps",
                    live_regions,
                    state.get("regionsTested", absent),
                    index,
                )
                for actor in state["actors"]:
                    if str(actor.get("id", "")).startswith("enemy-"):
                        eq(
                            "memory through retained gaps",
                            memory_chain.get(actor["id"], absent),
                            actor,
                            index,
                        )
