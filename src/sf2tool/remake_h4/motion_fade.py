"""Saved palette, temporary period and terminal full-fade restoration."""


def motion_fade(states, ins, helper, token, check, operand, _bounded_list):
    fade_rows = _bounded_list(s for s in states if s.get("fade") is not None)
    terminal = next((s for s in reversed(fade_rows) if s["fade"].get("LogicalDone")), None)
    f = (terminal or {}).get("fade") or {}
    expected_purpose = (0 if ins["kind"] == "FadeOut" else 1) if helper else 2
    for s in fade_rows:
        operand(
            "operation",
            token,
            "source full-fade purpose",
            [s["fade"].get("Purpose")],
            lambda a, expected_purpose=expected_purpose: a == expected_purpose,
        )
        operand(
            "operation",
            token,
            "source full-fade color",
            [s["fade"].get("Color")],
            lambda a, ins=ins: a == int(ins["resource"] == "white"),
        )
        operand(
            "operation",
            token,
            "source full-fade entry range",
            [s["fade"].get("Entry")],
            lambda a: 0 <= a <= 8,
        )
    operand(
        "operation",
        token,
        "source full-fade purpose/color/finite terminator",
        [f.get("Purpose"), f.get("Color"), f.get("Entry"), f.get("LogicalDone")],
        lambda a, b, c, d, expected_purpose=expected_purpose, ins=ins: (
            a == expected_purpose and b == int(ins["resource"] == "white") and c == 8 and d is True
        ),
    )
    display = (terminal or {}).get("display") or {}
    fade_entry = next((s for s in fade_rows if s["fade"].get("Entry") == 0), None)
    saved = (fade_entry or {}).get("fade") or {}
    temporary = ins["resource"] == "white" or bool((ins.get("fullBlack") or {}).get("period"))
    restored_period = saved.get("RestorePeriod") if temporary else saved.get("Period")
    entry_base = ((fade_entry or {}).get("display") or {}).get("Base")
    for state in fade_rows:
        held = state["fade"]
        check(
            "operation",
            "saved fade period continuity",
            None
            if "RestorePeriod" not in saved or "RestorePeriod" not in held
            else held["RestorePeriod"] == saved["RestorePeriod"],
            token,
        )
        operand(
            "operation",
            token,
            "held fade period continuity",
            [held.get("Period"), saved.get("Period")],
            lambda a, b: a == b,
        )
        operand(
            "operation",
            token,
            "saved palette continuity",
            [(state.get("display") or {}).get("Base"), entry_base],
            lambda a, b: a == b,
        )
    operand(
        "operation",
        token,
        "full-fade period restoration",
        [display.get("Period"), restored_period],
        lambda a, b: a == b,
    )
    if ins["resource"] == "white" or (ins.get("fullBlack") or {}).get("period"):
        expected_period = 1 if ins["resource"] == "white" else ins["fullBlack"]["period"]
        operand(
            "operation",
            token,
            "source temporary fade period",
            [f.get("Period")],
            lambda a, expected_period=expected_period: a == expected_period,
        )
    current = display.get("Current") or {}
    operand(
        "operation",
        token,
        "full-fade logical palette endpoint",
        [current or None, entry_base],
        lambda a, b, ins=ins: (
            a == b
            if ins["kind"] == "FadeIn"
            else a.get("White" if ins["resource"] == "white" else "Black") is True
        ),
    )
