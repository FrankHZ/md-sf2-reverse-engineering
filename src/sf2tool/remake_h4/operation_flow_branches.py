"""Dynamic branch targets and callers bounded to their enclosing invocation."""

from sf2tool.remake_h4.operation_flow_indexes import entity


def operation_flow_branches(
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
):
    nextloc = executed[n + 1][0]["Program"] if n + 1 < len(executed) else None
    target = None
    if op in ("call", "jump"):
        target = ins["target"]
    elif op == "branch-flag":
        flags = flags_at(seq)
        if flags is not None:
            target = (
                ins["target"]
                if (ins["flag"] in flags) == ins["whenSet"]
                else dict(program=loc["Program"], instruction=loc["Instruction"] + 1)
            )
    elif op == "branch-coordinates":
        held = anchor(seq, "entities")
        player = entity(state_of(held), ins["entity"]) if held else None
        coordinates = (player["x"], player["y"]) if player else None
        if held:
            for earlier in ordered:
                if not held[0] < earlier["Sequence"] <= seq:
                    continue
                previous = instruction(earlier)
                if earlier["Sequence"] in warp_requests:
                    position = warp_requests[earlier["Sequence"]]["position"]
                    coordinates = position["x"] * 384, position["y"] * 384
                elif (
                    previous
                    and previous["op"] == "position"
                    and previous["entity"] == ins["entity"]
                ):
                    coordinates = (
                        previous["position"]["x"] * 384,
                        previous["position"]["y"] * 384,
                    )
        if coordinates is not None:
            target = (
                ins["target"]
                if (coordinates == (ins["x"], ins["y"])) == ins["whenEqual"]
                else dict(program=loc["Program"], instruction=loc["Instruction"] + 1)
            )
    if op in ("call", "jump", "branch-flag", "branch-coordinates"):
        expected = (
            dict(Program=target["program"], Instruction=target["instruction"]) if target else None
        )
        check(
            names[0],
            "source evaluated " + op,
            None if expected is None or nextloc is None else expected == nextloc,
            seq,
        )
    if op == "call":
        continuation = dict(Program=loc["Program"], Instruction=loc["Instruction"] + 1)
        depth, later = 1, None
        for j in range(n + 1, len(executed)):
            nested = executed[j][1]
            if nested is None:
                break
            if nested["op"] == "call":
                depth += 1
            elif nested["op"] in ("end", "end-map-script", "return"):
                depth -= 1
            if depth == 0:
                later = j + 1 if j + 1 < len(executed) else None
                break
        check(names[0], "caller continuation returned", True if later is not None else None, seq)
        if later:
            check(
                names[0],
                "first enclosing return reaches source continuation",
                executed[later][0]["Program"] == continuation,
                seq,
            )
            check(
                names[0],
                "callee source return precedes continuation",
                executed[later - 1][1]["op"] in ("end", "end-map-script", "return"),
                seq,
            )
            held = next(
                (
                    x
                    for x in states
                    if seq <= x[0] < executed[later][0]["Sequence"] and "callers" in state_of(x)
                ),
                None,
            )
            call_read = control_reads.get(seq)
            return_reads = [
                c
                for c in control_reads.values()
                if seq < c.get("Sequence", -1) < executed[later][0]["Sequence"]
                and c.get("Source") == executed[later - 1][0]["Program"]
                and c.get("Operation") in ("EndProgram", "ReturnProgram", "ScriptReturn")
            ]
            if call_read is not None:
                check(
                    names[0],
                    "actual call read source identity",
                    call_read.get("Source") == loc and call_read.get("Operation") == "CallProgram",
                    seq,
                )
                check(
                    names[0],
                    "actual enclosing return operand",
                    True if return_reads else None,
                    seq,
                )
                for returned in return_reads:
                    check(
                        names[0],
                        "actual enclosing return restores full pre-call stack",
                        None
                        if any(
                            k not in c
                            for c in (call_read, returned)
                            for k in ("Callers", "CallersBefore")
                        )
                        else returned["CallersBefore"] == call_read["Callers"]
                        and returned["Callers"] == call_read["CallersBefore"]
                        and returned.get("Cursor") == continuation,
                        seq,
                    )
            if held is None and call_read is None:
                check(names[0], "held caller operand absent", None, seq)
            if held and held[0] < executed[later][0]["Sequence"]:
                check(
                    names[0],
                    "held caller frame matches source continuation",
                    continuation in state_of(held)["callers"],
                    seq,
                )
