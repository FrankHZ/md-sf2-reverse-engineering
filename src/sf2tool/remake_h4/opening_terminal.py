"""Join script return, complete input prefix and original terminal snapshot."""


def compare_terminal(candidate, observed, diagnostic, rows, rows_available, returned, checks):
    check, match, absent = checks.check, checks.match, checks.absent
    terminal = observed.get("terminal") or {}
    terminal_state = returned.get("state") or {}
    check("producer terminal is opening map", match(3, terminal.get("map", absent)))
    # sample() is called twice in the same returned-script callback, before any
    # frame advance or restoration. Require every named scalar on both sides.
    terminal_fields = (
        "map",
        "layer",
        "x",
        "y",
        "rawX",
        "rawY",
        "facing",
        "mapEventWord",
        "rngBytes",
        "rngCopyByte",
        "speechSfx",
        "portrait",
        "windowState",
        "typewriting",
        "input",
        "flags",
        "rawTime",
    )
    terminal_expected = {k: terminal_state.get(k, absent) for k in terminal_fields}
    for name, fields in (
        (
            "flags",
            (
                "66",
                "600",
                "601",
                "602",
                "603",
                "604",
                "605",
                "607",
                "608",
                "401",
                "256",
                "501",
                "507",
                "982",
            ),
        ),
        ("rawTime", ("frame", "seconds", "secondsFrames")),
    ):
        terminal_expected[name] = {
            k: (terminal_state.get(name) or {}).get(k, absent) for k in fields
        }
    check("producer terminal joins returned-script snapshot", match(terminal_expected, terminal))
    check(
        "returned script is candidate program",
        match(
            (candidate.get("Diagnostic") or {}).get("program", absent),
            ((returned.get("facts") or {}).get("registers") or {}).get("A0", absent),
        ),
    )
    check(
        "returned script is final emitted record",
        None
        if not rows_available
        else match("opening:program-return", rows[-1].get("kind", absent))
        if rows
        else False,
    )
    check(
        "return closes producer record count",
        match(diagnostic.get("records", absent), returned.get("order", absent)),
    )
    check(
        "return consumes selected input prefix",
        match(candidate.get("InputFrames", absent), returned.get("inputFrame", absent)),
    )
