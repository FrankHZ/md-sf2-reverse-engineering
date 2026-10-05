"""Ordinary input brackets, projection identity and phase completion ownership."""

from sf2tool.remake_h4.scene_checks import absent, number
from sf2tool.remake_h4.scene_fielddeath import compare_field_projection
from sf2tool.remake_h4.scene_rendering import AnimationProgress, compare_rendering


def compare_phases(
    pairs,
    bytoken,
    warps,
    inputs,
    owners,
    starts,
    field_batches,
    scene_info,
    reward_context,
    healing,
    materials,
    sequences,
    events,
    ordered,
    source_materials,
    checks,
):
    """Own phase-local animation progress and compare causal delivery/completion."""
    check, eq = checks.check, checks.eq
    precedes, identity = checks.precedes, checks.identity
    result_by_sequence = {r["result"].get("observationSequence"): (i, r) for i, r in warps.items()}
    declared_inputs = set((reward_context.get("indices") or {}).get("inputRecords", []))
    check(
        "independent input interval coverage",
        True if declared_inputs and declared_inputs <= inputs.keys() else None,
    )
    terminal = []
    for pair in pairs:
        token, phase, end, owner = pair["token"], pair["phase"], pair["end"], pair["owner"]
        rows = bytoken.get(token, [])
        check("required phase projection", bool(rows) or None, token)
        envelope = warps[owner]["result"]
        eq(
            "required phase start accepted",
            None,
            warps[owners[token]]["result"].get("failure", absent),
            token,
        )
        eq("required completion accepted", None, envelope.get("failure", absent), token)
        if not phase.endswith("Message"):
            # A renderer completion is outside physical input dispatch. Some retained
            # copies omit inputDelivery: use the complete original input-index census
            # and its two bracketing snapshots, never a default false transport flag.
            if "inputDelivery" in warps[owner]:
                eq(
                    "ordinary completion non-input transport",
                    False,
                    warps[owner]["inputDelivery"],
                    token,
                )
            direct = [
                i
                for i in inputs.values()
                if number(i.get("resultStart"))
                and number(i.get("resultEnd"))
                and i["resultStart"] <= owner < i["resultEnd"]
            ]
            check("automatic completion outside input dispatch", not direct, token)
            earlier = [
                (n, i)
                for n, i in inputs.items()
                if number(i.get("resultEnd")) and i["resultEnd"] <= owner
            ]
            later = [
                (n, i)
                for n, i in inputs.items()
                if number(i.get("resultStart")) and i["resultStart"] > owner
            ]
            if earlier and later:
                before_index, before_input = earlier[-1]
                after_index, after_input = later[0]
                contiguous = (
                    after_index == before_index + 1
                    and before_index in declared_inputs
                    and after_index in declared_inputs
                )
                check("completion contiguous input bracket", True if contiguous else None, token)
                if contiguous:
                    eq(
                        "automatic completion latest causal input",
                        before_input.get("ordinal", absent),
                        warps[owner].get("inputOrdinal", absent),
                        token,
                    )
                    precedes(
                        "completion after prior input dispatch",
                        before_input.get("after") or {},
                        envelope,
                        token,
                    )
                    precedes(
                        "completion before following input dispatch",
                        envelope,
                        after_input.get("before") or {},
                        token,
                    )
                    for r in rows:
                        if r.get("scene", {}).get("completed") is True:
                            hosts = [
                                x.get("hostUpdate", absent) for x in (before_input, r, after_input)
                            ]
                            check(
                                "completion host input bracket",
                                None
                                if any(x is absent for x in hosts)
                                else all(number(x) for x in hosts)
                                and hosts[0] <= hosts[1] <= hosts[2],
                                token,
                            )
            else:
                check("completion contiguous input bracket", None, token)
        batch = field_batches.get(token)
        info = scene_info.get(token, {})
        complete = []
        progress = AnimationProgress()
        for row in rows:
            index, s = row["_index"], row["scene"]
            eq("actual phase for token", phase, s.get("phase", absent), index)
            eq("required scene visibility", batch is None, s.get("visible", absent), index)
            if phase.endswith("Message"):
                eq(
                    "actual message label visible",
                    True,
                    (s.get("messageFont") or {}).get("visible", absent),
                    index,
                )
            if number(row.get("observationSequence")):
                preceding = [q for q in starts if q <= row["observationSequence"]]
                if row.get("projectionStage") == "host-poll":
                    anchor = result_by_sequence.get(row["observationSequence"])
                    check("poll owning result present", True if anchor else None, index)
                    if anchor:
                        identity("poll result identity", anchor[1]["result"], row, index)
                        eq(
                            "poll result causal input",
                            anchor[1].get("inputOrdinal", absent),
                            row.get("inputOrdinal", absent),
                            index,
                        )
                    eq(
                        "poll belongs to latest phase",
                        max(preceding) if preceding else absent,
                        token,
                        index,
                    )
                elif row.get("projectionStage") == "signal-before-Present":
                    check(
                        "old view belongs to owning completion",
                        end["Sequence"] <= row["observationSequence"]
                        and owner == result_by_sequence.get(row["observationSequence"], (None,))[0],
                        index,
                    )
                else:
                    check("known projection seam", False, index)
            causal_inputs = [
                i
                for i in inputs.values()
                if i.get("hostUpdate", float("inf")) <= row.get("hostUpdate", -1)
            ]
            if causal_inputs:
                eq(
                    "projection latest causal input",
                    causal_inputs[-1].get("ordinal", absent),
                    row.get("inputOrdinal", absent),
                    index,
                )
            else:
                check("projection latest causal input", None, index)
            if row.get("projectionStage") == "signal-before-Present" and not phase.endswith(
                "Message"
            ):
                eq("delivered non-message completion flag", True, s.get("completed", absent), index)
            if s.get("completed") is True:
                complete.append(row)
                eq(
                    "completed projection seam",
                    "signal-before-Present",
                    row.get("projectionStage", absent),
                    index,
                )
                identity("completed owning result", envelope, row, index)
                eq(
                    "completed causal ordinal",
                    warps[owner].get("inputOrdinal", absent),
                    row.get("inputOrdinal", absent),
                    index,
                )
            if batch:
                compare_field_projection(row, phase, batch, materials, checks)
                continue
            if not info:
                check("phase independently assigned to scene", None, token)
                continue
            compare_rendering(
                row,
                phase,
                info,
                token,
                healing,
                materials,
                sequences,
                events,
                owners,
                ordered,
                source_materials,
                progress,
                checks,
            )

        if phase.endswith("Message"):
            accepting = [
                i
                for i in inputs.values()
                if number(i.get("resultStart"))
                and number(i.get("resultEnd"))
                and i["resultStart"] <= owner < i["resultEnd"]
            ]
            check("message has owning physical input", bool(accepting) or None, token)
            for inp in accepting:
                eq("message accepted Confirm", dict(action="confirm", pressed=True), inp, token)
                before = inp.get("before") or {}
                visible, total = (
                    before.get("sceneVisibleCharacters"),
                    before.get("sceneTotalCharacters"),
                )
                check(
                    "message ready before accepting input",
                    visible < 0 or visible >= total
                    if isinstance(visible, (int, float)) and number(total)
                    else None,
                    token,
                )
                eq(
                    "message result input owner",
                    inp.get("ordinal", absent),
                    warps[owner].get("inputOrdinal", absent),
                    token,
                )
                check(
                    "message phase live before input",
                    token <= before["observationSequence"] < end["Sequence"]
                    if number(before.get("observationSequence"))
                    else None,
                    token,
                )
                precedes("message completion after input before", before, envelope, token)
                precedes(
                    "message completion before input after", envelope, inp.get("after") or {}, token
                )
        elif not complete:
            if (
                phase == "FieldSettle"
                and batch
                and (
                    envelope.get("mode") == "Exploration"
                    or warps[owner].get("projection") == "field-view-pending"
                )
            ):
                terminal.append(pair)
            else:
                check("actual non-message completion", None, token)
        elif phase == "ActionAnimation" or phase == "Reaction" and info.get("reaction") == "Dodge":
            if progress.sequence:
                check(
                    "every source animation entry observed",
                    set(range(len(progress.frames))) == progress.played or None,
                    token,
                )
                eq(
                    "completed last animation entry",
                    len(progress.frames) - 1,
                    complete[-1]["scene"].get("frameIndex", absent),
                    token,
                )

    return terminal
