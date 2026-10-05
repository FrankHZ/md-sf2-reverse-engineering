"""Compose complete reached source bodies and five ordered effect families."""

from sf2tool.remake_h4.operation_flow_branches import operation_flow_branches
from sf2tool.remake_h4.operation_flow_checks import operation_flow_checks
from sf2tool.remake_h4.operation_flow_choices import operation_flow_choices
from sf2tool.remake_h4.operation_flow_control import operation_flow_control
from sf2tool.remake_h4.operation_flow_effects import operation_flow_effects
from sf2tool.remake_h4.operation_flow_flags import operation_flow_flags
from sf2tool.remake_h4.operation_flow_indexes import event_index, state_index
from sf2tool.remake_h4.operation_flow_outcome import operation_flow_outcome
from sf2tool.remake_h4.operation_flow_programs import instruction_reader, operation_flow_programs
from sf2tool.remake_h4.operation_flow_roster import operation_flow_roster
from sf2tool.remake_h4.operation_flow_source import operation_flow_source
from sf2tool.remake_h4.operation_flow_warps import operation_flow_warps


def operation_flow_binding(
    actual,
    selection,
    source_root,
    motion,
    text,
    *,
    read,
    _bounded_list,
    _occurrence_map,
    _bounded_sorted,
    _occurrence_dict,
):
    """Bind complete reached source bodies to dynamic control and ordered effects."""
    names, result, check, common, finish = operation_flow_checks(_bounded_list)

    selected = operation_flow_source(selection, source_root, read, common)
    if selected is None:
        return finish()
    (
        world,
        programs,
        maps,
        source_root,
        tracked,
        compiler,
        routes,
        _encode_source,
        _decode_source_table,
        _tokens,
    ) = selected

    events, records, warp_records, ordered = event_index(
        actual, common, _occurrence_map, _bounded_list, _bounded_sorted
    )

    instruction = instruction_reader(programs)

    executed = _bounded_list((e, instruction(e)) for e in ordered if e.get("Program"))
    common("complete logical source occurrence inventory", True if executed else None)
    operation_flow_programs(
        executed,
        programs,
        source_root,
        compiler,
        tracked,
        routes,
        result,
        common,
        _encode_source,
        _decode_source_table,
    )

    # Finite source fades publish a wait producer rather than program-instruction.
    # Reuse the independently source-bound producer locations, never host counts.
    trace = _occurrence_dict((e["Sequence"], e["Program"]) for e, _ in executed)
    for occurrence in motion.get("occurrences", []):
        if occurrence.get("location"):
            trace.setdefault(occurrence["token"], occurrence["location"])
    trace = _bounded_sorted(trace.items())
    for index, (seq, loc) in enumerate(trace[:-1]):
        ins = instruction(dict(Program=loc))
        if ins is None or ins["op"] in (
            "call",
            "jump",
            "branch-flag",
            "branch-coordinates",
            "end",
            "end-map-script",
            "return",
        ):
            continue
        following_loc = trace[index + 1][1]
        expected = dict(Program=loc["Program"], Instruction=loc["Instruction"] + 1)
        # Explicit outcome-map transfer has no Program field; its map effect is
        # bound below rather than treated as an omitted source instruction.
        skipped = instruction(dict(Program=expected))
        if skipped and skipped["op"] == "battle-return-map":
            continue
        value = following_loc == expected
        if not value and any(
            number not in events for number in range(int(seq) + 1, int(trace[index + 1][0]))
        ):
            value = None
        for family in names:
            check(family, "complete ordered source successor", value, seq)

    states, state_of, anchor = state_index(actual, _bounded_list)

    control_reads = operation_flow_control(
        actual, events, instruction, names, check, _occurrence_map
    )

    flags_at, join_effect, write_flags = operation_flow_flags(
        world, maps, ordered, instruction, anchor, state_of, _occurrence_map
    )

    warp_requests = operation_flow_warps(
        actual,
        ordered,
        warp_records,
        records,
        states,
        state_of,
        anchor,
        instruction,
        executed,
        source_root,
        compiler,
        maps,
        routes,
        tracked,
        flags_at,
        names,
        result,
        check,
        _occurrence_map,
        _bounded_list,
        _bounded_sorted,
        _encode_source,
        _decode_source_table,
        _tokens,
    )

    for n, (e, ins) in enumerate(executed):
        seq, loc = e["Sequence"], e["Program"]
        if ins is None:
            common("unmapped logical source occurrence", None)
            continue
        op = ins["op"]
        operation_flow_branches(
            n,
            e,
            ins,
            seq,
            loc,
            op,
            executed,
            ordered,
            instruction,
            flags_at,
            anchor,
            state_of,
            warp_requests,
            states,
            control_reads,
            names,
            check,
        )
        operation_flow_roster(
            op,
            seq,
            ins,
            anchor,
            state_of,
            ordered,
            instruction,
            join_effect,
            write_flags,
            names,
            check,
        )
        operation_flow_choices(op, seq, ins, executed, ordered, anchor, state_of, names, check)

    check(names[1], "complete source dialogue/control material", text["value"])
    check(names[4], "complete awaited source effects", motion["operation"])
    operation_flow_outcome(
        actual,
        ordered,
        maps,
        warp_requests,
        states,
        state_of,
        motion,
        flags_at,
        join_effect,
        write_flags,
        names,
        result,
        check,
        _bounded_list,
        _bounded_sorted,
    )
    operation_flow_effects(
        actual,
        executed,
        ordered,
        warp_records,
        records,
        states,
        state_of,
        anchor,
        names,
        check,
        _bounded_list,
    )
    return finish()
