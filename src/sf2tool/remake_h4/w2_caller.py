"""Compare source-bound W2 caller, live gates and the displayed token span."""

from sf2tool.remake_h4.w2_checks import merge


def compare_caller(owner, cursor, token, ready_state, texts, session, ordinal, checks):
    check, match = checks.check, checks.match
    program = owner.get("program")
    entity_running = program not in ("map3-entityevent0", "map3-entityevent15")
    caller = (
        "EntityEventContext"
        if program in ("map3-entityevent0", "map3-entityevent15", "cs-52f0c")
        else None
        if program in ("cs-53996", "bbcs-01")
        else "ZoneEventContext"
    )
    portrait = "ClosedPortraitWindow" if program == "cs-5145c" else "OpenPortraitWindow"
    check(
        "source caller and live service gates",
        match(
            dict(
                sessionId=session,
                token=token,
                cursor=cursor,
                canWaitForText=True,
                wait="FieldTextWait",
                entitiesRunning=entity_running,
                eventCaller=caller,
                portraitWindow=portrait,
                typewriting=False,
                logicalView=dict(HideWindows=False, Scrolling=False),
                fieldText=dict(
                    Text=owner.get("text"),
                    Wait2=True,
                    Revealed=True,
                    LogicalDone=True,
                    Token=dict(Value=token),
                ),
            ),
            ready_state,
        ),
        ordinal,
    )
    field = ready_state.get("fieldText") or {}
    units, end_unit = field.get("Units") or [], field.get("End")
    check(
        "source W2 token at actual span",
        merge(
            [
                None if owner.get("text") not in texts else "{W2}" in texts[owner["text"]],
                None
                if end_unit is None or not units
                else 0 <= end_unit < len(units) and units[int(end_unit)].get("Kind") == 4,
            ]
        ),
        ordinal,
    )
    return program
