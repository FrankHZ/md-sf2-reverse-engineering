"""Compose occurrence-local source operations and field consumers."""

from sf2tool.remake_h4.motion_checks import motion_checks
from sf2tool.remake_h4.motion_commands import motion_commands
from sf2tool.remake_h4.motion_completion import motion_completion
from sf2tool.remake_h4.motion_draws import motion_draws
from sf2tool.remake_h4.motion_events import motion_events
from sf2tool.remake_h4.motion_inventory import motion_inventory
from sf2tool.remake_h4.motion_programs import motion_programs, target
from sf2tool.remake_h4.motion_source import motion_source
from sf2tool.remake_h4.motion_wait import motion_wait


def field_motion_binding(
    actual,
    selection,
    source_root,
    *,
    read,
    _bounded_list,
    _occurrence_map,
    _bounded_sorted,
    _group_rows,
):
    """Join source producers to occurrence-local field waits and actual consumers."""
    result, check, common, aggregate, operand = motion_checks(_bounded_list)

    programs, compiler, tracked_source = motion_source(selection, source_root, read, common)

    (
        warp_records,
        record_by_event,
        ordered,
        release_by_token,
        by_token,
        release_boundary,
        following,
    ) = motion_events(actual, common, _occurrence_map, _bounded_sorted, _group_rows)

    instruction, source_value, source_spans = motion_programs(programs, compiler, tracked_source)

    producers = motion_inventory(
        ordered, warp_records, record_by_event, instruction, common, _bounded_list
    )

    for e, loc, ins, role, helper in producers:
        token = e["Sequence"]
        r = warp_records[record_by_event[token]]
        rows = by_token.get(token, [])
        states = _bounded_list(b["state"] for b in rows)
        entry = next(
            (
                s
                for s in states
                if s.get("wait") in ("EntityWait", "NodWait", "FullFadeWait", "PresentationWait")
            ),
            None,
        )
        provenance = (
            "accepted ordinary-warp fade helper"
            if helper
            else programs.get((loc or {}).get("Program"), {}).get("source")
        )
        occurrence = dict(
            token=token,
            location=loc,
            source=provenance,
            role=role,
            blocking=None,
            operation=None,
            consumer=None,
        )
        start_index = len(result["checks"])
        for family in ("operation", "consumer"):
            check(
                family,
                "source complete ordered producer operands",
                True
                if helper and compiler is not None
                else source_value((loc or {}).get("Program")),
                token,
            )
            check(family, "typed producer present", None if ins is None else target(ins), token)
        if loc and loc["Program"] in source_spans:
            p = programs[loc["Program"]]
            ordinal = sum(target(i) for i in p["instructions"][: int(loc["Instruction"])])
            spans = source_spans[loc["Program"]]
            occurrence["sourceProducerOrdinal"] = ordinal
            occurrence["sourceOperation"] = spans[ordinal] if ordinal < len(spans) else None
        for state in states:
            if (
                role != "motion"
                or (ins or {}).get("wait")
                or state.get("wait")
                in ("EntityWait", "NodWait", "FullFadeWait", "PresentationWait")
            ):
                operand(
                    "operation",
                    token,
                    "pending wait blocks field input readiness",
                    [state.get("canWaitAtInput")],
                    lambda ready: ready is False,
                )
        if ins is None:
            # Absent source operands cannot hide a contradiction between the
            # logical wait and its predicate release in this same session.
            if entry and entry.get("entityWait"):
                release = release_by_token.get(token)
                payload = (release or {}).get("EntityWaitRelease") or {}
                wait = entry["entityWait"]
                for family in ("operation", "consumer"):
                    operand(
                        family,
                        token,
                        "logical/released subject agree",
                        [wait.get("Entity"), payload.get("Subject")],
                        lambda a, b: a == b,
                    )
                    operand(
                        family,
                        token,
                        "logical/released policy agree",
                        [wait.get("Completion"), payload.get("Completion")],
                        lambda a, b: a == b,
                    )
            result["occurrences"].append(occurrence)
            continue
        subject = ins.get("entity")
        blocking = role != "motion" or ins.get("wait")
        occurrence["blocking"] = bool(blocking)
        if not blocking:
            # Source nonwait/perpetual installation has no local completion dependency.
            occurrence["completionApplicability"] = "nonawaited source installation"
            for family in ("operation", "consumer"):
                check(
                    family,
                    "nonawaited instruction installs without local wait",
                    e.get("Detail") == "StartEntityMotion",
                    token,
                )
        else:
            motion_wait(entry, r, e, role, states, loc, ins, subject, token, check, operand)
            end = None
            if role == "motion":
                end = motion_commands(
                    release_by_token,
                    token,
                    ins,
                    subject,
                    entry,
                    states,
                    occurrence,
                    release_boundary,
                    loc,
                    operand,
                )
            else:
                end = motion_completion(
                    ins,
                    role,
                    token,
                    following,
                    subject,
                    rows,
                    states,
                    helper,
                    entry,
                    record_by_event,
                    warp_records,
                    actual,
                    check,
                    operand,
                    _bounded_list,
                )
            check("operation", "logical completion occurrence", True if end else None, token)
            if end:
                dependent = next(
                    (
                        x
                        for x in ordered
                        if x["Sequence"] > token
                        and x["Kind"]
                        in (
                            "program-instruction",
                            "nod-started",
                            "map-transferred",
                            "battle-returned",
                        )
                    ),
                    None,
                )
                operand(
                    "operation",
                    token,
                    "completion before dependent continuation",
                    [end.get("Sequence"), (dependent or {}).get("Sequence")],
                    lambda a, b: a < b,
                )
                if role == "motion":
                    check("consumer", "predicate release observed before continuation", True, token)

            motion_draws(rows, role, ins, subject, states, token, check, operand, _bounded_list)
        local = result["checks"][start_index:]
        for family in ("operation", "consumer"):
            occurrence[family] = aggregate(
                [c["value"] for c in local if family == "consumer" or c["family"] == family]
            )
        result["occurrences"].append(occurrence)
    if compiler is not None:
        common("lowering dependencies owned by source pin", compiler.sources <= tracked_source)
    for family in ("operation", "consumer"):
        result[family] = aggregate(
            _bounded_list(
                c["value"]
                for c in result["checks"]
                if family == "consumer" or c["family"] == family
            )
        )
    return result
