"""Validate complete producer records, seen set and independent R1 epochs."""


def compare_records(context, candidate, diagnostic, checks):
    check, match, merge, absent = checks.check, checks.match, checks.merge, checks.absent
    rows = context.get("records") or []
    rows_available = context.get("records") is not None
    check(
        "producer count equals complete record set",
        None if not rows_available else match(len(rows), diagnostic.get("records", absent)),
    )
    seen = diagnostic.get("seen")
    keys = []
    epochs = {"frame": [], "emulatorFrame": []}
    for index, row in enumerate(rows):
        kind = row.get("kind")
        stage = (row.get("facts") or {}).get("stage")
        check(
            "producer record order and boundary",
            match(dict(order=index + 1, boundary="callback-time"), row),
        )
        check("producer record phase", None if stage is None else stage in ("pre-r1", "admitted"))
        check("producer record kind", None if kind is None else kind.startswith("opening:"))
        if stage is not None and kind is not None:
            key = stage + ":" + kind.removeprefix("opening:")
            keys.append(key)
            check(
                "record retained in producer seen set", match(True, (seen or {}).get(key, absent))
            )
        input_frame = row.get("inputFrame")
        if stage == "pre-r1":
            check("pre-R1 record has no input epoch", match(False, row.get("inputFrame", absent)))
        elif stage == "admitted":
            for axis in epochs:
                value = row.get(axis)
                check(
                    "admitted " + axis + " and input are nonnegative integers",
                    merge(
                        [
                            None if v is None else type(v) is int and v >= 0
                            for v in (value, input_frame)
                        ]
                    ),
                )
                if type(value) is int and type(input_frame) is int:
                    epochs[axis].append(value - input_frame)
        hook = ((candidate.get("Diagnostic") or {}).get("hooks") or {}).get(
            kind.removeprefix("opening:") if kind else ""
        )
        if hook is not None:
            check(
                "record PC joins candidate hook",
                match(hook.get("pc", absent), row.get("pc", absent)),
            )
    check("producer emitted each phase/hook once", len(keys) == len(set(keys)))
    check(
        "producer seen flags",
        None if seen is None else merge([match(True, value) for value in seen.values()]),
    )
    check(
        "producer seen set equals complete record set",
        None
        if not rows_available or seen is None or len(keys) != len(rows)
        else set(seen) == set(keys),
    )
    for axis, values in epochs.items():
        # Every admitted record carries an independent epoch equation. Known
        # disagreements remain false even if the explicit R1 row is missing.
        check(
            "records agree on R1 " + axis + " epoch",
            None if not values else all(v >= 0 and v == values[0] for v in values),
        )
    return rows, rows_available, epochs
