"""Select original scalar seams and join progression to both R1 epochs."""


def select_readback(rows, epochs, checks):
    check, match, merge, one, absent = (
        checks.check,
        checks.match,
        checks.merge,
        checks.one,
        checks.absent,
    )
    selected = []
    for name, pc in (
        ("r1", 0x2591C),
        ("view-read", 0x46B4),
        ("view-selected", 0x46BE),
        ("mouth-glyph", 0x68E6),
        ("view-clear-before", 0x4723E),
        ("view-clear-after", 0x47242),
        ("program-return", 0x58C),
    ):
        row = one(
            "original " + name,
            [
                r
                for r in rows
                if r.get("kind") == "opening:" + name
                and (r.get("facts") or {}).get("stage") == "admitted"
            ],
        )
        selected.append(row)
        check(
            "original " + name + " seam",
            match(
                dict(
                    pc=pc,
                    state=dict(map=3),
                    facts=dict(values=dict(MOUTH_CONTROL_TOGGLE=0, VIEW_SCROLLING_SPEED=0)),
                ),
                row,
            ),
        )
    for axis, strict in (("order", True), ("frame", False), ("inputFrame", False)):
        values = [r.get(axis) for r in selected]
        known = [v for v in values if v is not None]
        check(
            "original " + axis + " progression",
            merge(
                [
                    True if len(known) == len(values) else None,
                    all(v >= 0 for v in known),
                    all(
                        a < b if strict else a <= b for a, b in zip(known, known[1:], strict=False)
                    ),
                ]
            ),
        )
    check("R1 precedes controller delivery", match(0, selected[0].get("inputFrame", absent)))
    r1 = selected[0]
    for axis, values in epochs.items():
        for value in values:
            check("record epoch joins R1 " + axis, match(r1.get(axis, absent), value))
    for row in rows:
        if (row.get("facts") or {}).get("stage") == "pre-r1":
            for axis, strict in (("order", True), ("frame", False), ("emulatorFrame", False)):
                value, upper = row.get(axis), r1.get(axis)
                check(
                    "pre-R1 " + axis + " precedes admission",
                    None
                    if value is None or upper is None
                    else value < upper
                    if strict
                    else value <= upper,
                )
    return selected
