"""Actual control reads, producing commits and full caller-stack transitions."""


def operation_flow_control(actual, events, instruction, names, check, _occurrence_map):
    control_reads = _occurrence_map()
    for row in actual.get("warpRecords", []):
        delivery = row.get("result", {})
        for control in delivery.get("programControlReads") or []:
            seq = control.get("Sequence")
            previous = control_reads.get(seq)
            check(
                names[0],
                "control read duplicate identity",
                previous is None or previous == control,
                seq,
            )
            control_reads.setdefault(seq, control)
            event = events.get(seq)
            source_instruction = instruction(dict(Program=control.get("Source")))
            operands = all(
                k in control
                for k in (
                    "SessionId",
                    "Sequence",
                    "Revision",
                    "Source",
                    "Operation",
                    "Cursor",
                    "CallersBefore",
                    "Callers",
                )
            )
            check(names[0], "control read operands", True if operands else None, seq)
            check(
                names[0],
                "same session control read session",
                None
                if "SessionId" not in control
                else control["SessionId"] == delivery.get("sessionId"),
                seq,
            )
            check(
                names[0],
                "control read producing event revision",
                None
                if event is None or "Revision" not in control
                else control["Revision"] == event.get("Revision"),
                seq,
            )
            check(
                names[0],
                "control read delivered sequence interval",
                None if seq is None else seq <= delivery.get("observationSequence", -1),
                seq,
            )
            op = source_instruction.get("op") if source_instruction else None
            operation = control.get("Operation")
            check(
                names[0],
                "control read source operation",
                None
                if op is None or operation is None
                else (operation == "CallProgram" and op == "call")
                or (operation == "ReturnProgram" and op == "return")
                or (
                    operation in ("EndProgram", "ScriptReturn") and op in ("end", "end-map-script")
                ),
                seq,
            )
            produced = (
                None
                if event is None or operation is None or "Source" not in control
                else (
                    event.get("Kind") == "text-work-advanced"
                    if operation == "ScriptReturn"
                    else event.get("Program") == control["Source"]
                    and event.get("Detail") == operation
                )
            )
            check(names[0], "control read producing Commit", produced, seq)
            if any(k not in control for k in ("CallersBefore", "Callers", "Cursor")):
                continue
            before, after = control["CallersBefore"], control["Callers"]
            if operation == "CallProgram" and source_instruction:
                continuation = dict(
                    Program=control["Source"]["Program"],
                    Instruction=control["Source"]["Instruction"] + 1,
                )
                target = source_instruction["target"]
                check(
                    names[0],
                    "actual call pushes full source continuation stack",
                    after == before + [continuation]
                    and control["Cursor"]
                    == dict(Program=target["program"], Instruction=target["instruction"]),
                    seq,
                )
            elif operation in ("EndProgram", "ReturnProgram", "ScriptReturn"):
                check(
                    names[0],
                    "actual return pops full stack or ends empty caller",
                    after == before[:-1] and control["Cursor"] == (before[-1] if before else None),
                    seq,
                )
    return control_reads
