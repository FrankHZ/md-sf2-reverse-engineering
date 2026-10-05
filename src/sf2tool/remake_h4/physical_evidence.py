"""Select physical evidence and bind result, input, event and projection identities.

Consume the caller's channel iterables while their transport is still open. Only
selected rows and event indexes are retained; this module opens no resources and
never takes ownership of the caller's stream context.
"""

from sf2tool.remake_h4.physical_checks import (
    IDENTITY_KEYS,
    SNAPSHOT_KEYS,
    absent,
    match,
    merge,
    number,
)
from sf2tool.remake_h4_reference import UPSTREAM


def select_evidence(actual, context, checks):
    """Return selected channel maps, events and event-to-result indexes in source order."""
    check, eq = checks.check, checks.eq
    clocks, precedes, join = checks.clocks, checks.precedes, checks.join
    session = checks.session
    identity_keys, snapshot_keys = IDENTITY_KEYS, SNAPSHOT_KEYS
    eq("selected scope", "retained-keyboard-A-physical", context.get("scope", absent))
    check("independent session", bool(session) or None)
    eq("source revision", UPSTREAM, context.get("upstream", absent))
    receipt = context.get("selectionReceipt") or {}
    eq("completed immutable selection", dict(sourceUnchanged=True, failure=None), receipt)
    selected = {}
    for channel in ("warpRecords", "inputRecords", "sceneObservations", "samples"):
        wanted = set((context.get("indices") or {}).get(channel, []))
        records = {}
        order = []
        for index, row in enumerate(actual.get(channel, [])):
            index = row.get("_index", index)
            if index not in wanted:
                continue
            order.append(index)
            check(channel + " unique source index", index not in records)
            records[index] = dict(row, _index=index)
        check(channel + " declared selection", bool(wanted) or None)
        check(
            channel + " complete selected indices",
            True if set(records) == wanted and wanted else None,
        )
        check(channel + " source order", order == sorted(set(order)))
        selected[channel] = records
    warps = selected["warpRecords"]
    input_rows = list(selected["inputRecords"].values())
    declared_inputs = set((context.get("indices") or {}).get("inputRecords", []))
    missing_inputs = declared_inputs - set(selected["inputRecords"])

    def causal_result(index, row):
        eligible = [
            i for i in input_rows if number(i.get("resultStart")) and i["resultStart"] <= index
        ]
        later = [i for i in input_rows if number(i.get("resultStart")) and i["resultStart"] > index]
        if not eligible:
            check("causal result input available", None)
            return
        owner = eligible[-1]
        next_index = later[0]["_index"] if later else max(declared_inputs, default=-1) + 1
        ambiguous = any(owner["_index"] < i < next_index for i in missing_inputs)
        check(
            "result belongs to latest causal input",
            None if ambiguous else match(owner.get("ordinal"), row.get("inputOrdinal", absent)),
        )
        envelope = row.get("result") or {}
        precedes("causal input before precedes result", owner.get("before") or {}, envelope)
        if number(owner.get("resultEnd")):
            if index < owner["resultEnd"]:
                precedes("direct result within input snapshots", envelope, owner.get("after") or {})
                if index == owner["resultEnd"] - 1:
                    join("direct result/input after", envelope, owner.get("after") or {})
                    if row.get("state"):
                        join(
                            "direct state/input after",
                            row["state"],
                            owner.get("after") or {},
                            snapshot_keys,
                        )
            else:
                precedes(
                    "automatic result follows completed input", owner.get("after") or {}, envelope
                )
        if later:
            precedes("result precedes next physical input", envelope, later[0].get("before") or {})

    previous_result = None
    for row in warps.values():
        envelope = row.get("result") or {}
        if previous_result:
            precedes("result clocks progress in source-index order", previous_result, envelope)
        previous_result = envelope

    previous_input = None
    for row in input_rows:
        before_input, after_input = row.get("before") or {}, row.get("after") or {}
        clocks("input before", before_input)
        clocks("input after", after_input)
        precedes("input snapshot clocks progress", before_input, after_input)
        if previous_input:
            precedes(
                "physical input clocks progress", previous_input.get("after") or {}, before_input
            )
            check(
                "physical input result spans progress",
                row.get("resultStart", -1) >= previous_input.get("resultEnd", 0),
            )
        previous_input = row
        start, end = row.get("resultStart"), row.get("resultEnd")
        if number(start) and number(end):
            if start == end:
                join(
                    "empty input span preserves snapshot", before_input, after_input, snapshot_keys
                )
            for name, input_snapshot, result_index in (
                ("before", before_input, start - 1),
                ("after", after_input, end - 1),
            ):
                anchor = warps.get(result_index)
                if anchor:
                    join(
                        "input " + name + " joins result boundary",
                        input_snapshot,
                        anchor.get("result") or {},
                    )
                    if anchor.get("state"):
                        join(
                            "input " + name + " joins state boundary",
                            input_snapshot,
                            anchor["state"],
                            snapshot_keys,
                        )

    census_for_joins = context.get("census") or []
    physical_rows = {
        i: row
        for i, row in warps.items()
        if any(
            number(o.get("index"))
            and number((o.get("end") or {}).get("index"))
            and o["index"] <= i <= o["end"]["index"]
            for o in census_for_joins
        )
    }
    for index, row in physical_rows.items():
        causal_result(index, row)
    events, event_rows = {}, {}
    for index, row in warps.items():
        envelope, state = row.get("result") or {}, row.get("state") or {}
        for label, value in (("result", envelope), *(([("state", state)]) if state else [])):
            eq(label + " session", session, value.get("sessionId", absent))
            check(
                label + " integer clocks",
                merge(
                    [
                        number(value[k]) if k in value else None
                        for k in ("revision", "observationSequence")
                    ]
                ),
            )
        if state:
            eq(
                "result/state clocks",
                {k: envelope[k] for k in ("revision", "observationSequence") if k in envelope},
                state,
            )
        if envelope.get("failure") is not None:
            eq(
                "retained rejected range attempt",
                dict(
                    failure=dict(Code="target-range", Field="target"),
                    stopReason="Rejected",
                    observations=[],
                ),
                envelope,
            )
        observed = envelope.get("observations")
        if not isinstance(observed, list):
            check("result observations", None)
            continue
        positions = []
        for event in observed:
            seq, revision = event.get("Sequence"), event.get("Revision")
            valid = (
                None
                if "Sequence" not in event or "Revision" not in event
                else number(seq) and number(revision)
            )
            check("event clocks", valid)
            if not valid:
                continue
            positions.append(seq)
            check(
                "event inside result envelope",
                seq <= envelope.get("observationSequence", -1)
                and revision <= envelope.get("revision", -1),
            )
            if seq in events:
                eq("repeated event identity and payload", events[seq], event)
            else:
                events[seq] = event
                event_rows[seq] = index
        check("event order within Submit", positions == sorted(set(positions)))
        if positions:
            eq(
                "result terminal event axis",
                positions[-1],
                envelope.get("observationSequence", absent),
            )
    previous_event = None
    for _, event in sorted(events.items()):
        if previous_event:
            check(
                "event revision progresses with sequence",
                event["Revision"] >= previous_event["Revision"],
            )
        previous_event = event
    for row in input_rows:
        check(
            "input result interval",
            number(row.get("resultStart"))
            and number(row.get("resultEnd"))
            and row["resultStart"] <= row["resultEnd"],
        )
    for projection in selected["sceneObservations"].values():
        scene = projection.get("scene") or {}
        if (
            scene.get("phase") in ("FieldSpin", "FieldExit", "FieldSettle")
            and scene.get("visible") is False
        ):
            continue
        clocks("physical projection", projection)
        check(
            "physical projection host clock",
            number(projection.get("hostUpdate")) if "hostUpdate" in projection else None,
        )
        check(
            "physical projection stage",
            projection.get("projectionStage") in ("host-poll", "signal-before-Present")
            if "projectionStage" in projection
            else None,
        )
        matching = [
            row
            for row in physical_rows.values()
            if all((row.get("result") or {}).get(k) == projection.get(k) for k in identity_keys)
        ]
        for row in physical_rows.values():
            identity = row.get("result") or {}
            if identity.get("observationSequence") == projection.get("observationSequence"):
                eq(
                    "projection revision joins known result sequence",
                    identity.get("revision"),
                    projection.get("revision", absent),
                )
        token = scene.get("waitToken")
        if projection.get("projectionStage") == "host-poll" and token in events:
            phases = [
                k
                for k, e in events.items()
                if k <= projection.get("observationSequence", -1)
                and e.get("Kind") in ("scene-prepared", "scene-step-started")
            ]
            check(
                "host poll displays current phase token", token == max(phases) if phases else None
            )
        if projection.get("projectionStage") == "signal-before-Present":
            completions = [
                e
                for row in matching
                for e in (row.get("result") or {}).get("observations", [])
                if e.get("Kind") == "scene-step-completed"
            ]
            check(
                "before-Present projection belongs to completion",
                any(e.get("Detail") == scene.get("phase") for e in completions)
                if completions
                else None,
            )
            eq("before-Present completion flag", True, scene.get("completed", absent))
        check("physical projection result snapshot retained", bool(matching) or None)
        for row in matching:
            eq(
                "projection belongs to actual result input",
                row.get("inputOrdinal"),
                projection.get("inputOrdinal", absent),
            )
        host_inputs = [
            i
            for i in input_rows
            if number(i.get("hostUpdate"))
            and number(projection.get("hostUpdate"))
            and i["hostUpdate"] <= projection["hostUpdate"]
        ]
        if host_inputs:
            owner = host_inputs[-1]
            next_inputs = [i for i in input_rows if i["_index"] > owner["_index"]]
            next_index = (
                next_inputs[0]["_index"] if next_inputs else max(declared_inputs, default=-1) + 1
            )
            ambiguous = any(owner["_index"] < i < next_index for i in missing_inputs)
            check(
                "projection follows latest host input",
                None
                if ambiguous
                else match(owner.get("ordinal"), projection.get("inputOrdinal", absent)),
            )
            precedes(
                "projection follows input before snapshot", owner.get("before") or {}, projection
            )
            if next_inputs:
                precedes(
                    "projection precedes next input snapshot",
                    projection,
                    next_inputs[0].get("before") or {},
                )
        else:
            check("projection causal host input retained", None)

    for row in selected["sceneObservations"].values():
        eq("scene projection session", session, row.get("sessionId", absent))
        eq("scene projection error", None, (row.get("scene") or {}).get("error", absent))

    return selected, events, event_rows
