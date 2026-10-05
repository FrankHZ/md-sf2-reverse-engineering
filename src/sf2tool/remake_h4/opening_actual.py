"""Join historical A admission, physical input, Submit and selected settings."""


def compare_actual(actual, original_r1, checks):
    check, match, one, absent = checks.check, checks.match, checks.one, checks.absent
    first = (
        one(
            "historical A admission",
            [s for i, s in enumerate(actual.get("samples", [])) if s.get("_index", i) == 0],
        ).get("state")
        or {}
    )
    session = "99755635-cad5-4fff-bc88-b70fcc5f017b"
    check(
        "actual initial ready state",
        match(
            dict(
                sessionId=session,
                revision=4,
                observationSequence=4,
                simulationTick=0,
                map="map-3",
                mode="Exploration",
                continuation="FieldInput",
                cursor=None,
                wait=None,
                callers=[],
                canWaitAtInput=True,
            ),
            first,
        ),
    )
    check(
        "actual admission controls",
        match(dict(MouthControl=0, ViewSpeed=0), first.get("textSettings", absent)),
    )
    original_values = (original_r1.get("facts") or {}).get("values") or {}
    check(
        "independent admission value correspondence",
        match(
            dict(
                MouthControl=original_values.get("MOUTH_CONTROL_TOGGLE", absent),
                ViewSpeed=original_values.get("VIEW_SCROLLING_SPEED", absent),
            ),
            first.get("textSettings", absent),
        ),
    )
    inp = one(
        "first physical input",
        [r for i, r in enumerate(actual.get("inputRecords", [])) if r.get("_index", i) == 0],
    )
    check(
        "first input joins admission",
        match(
            {
                k: first.get(k, absent)
                for k in (
                    "sessionId",
                    "revision",
                    "observationSequence",
                    "simulationTick",
                    "mode",
                    "map",
                    "cursor",
                    "wait",
                )
            },
            inp.get("before", absent),
        ),
    )
    check(
        "first physical input identity",
        match(dict(ordinal=1, action="left", pressed=True, resultStart=0, resultEnd=1), inp),
    )
    submit = one(
        "first actual Submit",
        [r for i, r in enumerate(actual.get("warpRecords", [])) if r.get("_index", i) == 0],
    )
    check(
        "first Submit identity",
        match(
            dict(
                inputOrdinal=1,
                result=dict(
                    boundary="submit",
                    sessionId=session,
                    revision=5,
                    observationSequence=5,
                    failure=None,
                ),
                state=dict(sessionId=session, revision=5, observationSequence=5),
            ),
            submit,
        ),
    )
    first_events = (submit.get("result") or {}).get("observations") or []
    first_event = one("first input movement observation", first_events)
    check(
        "first Submit event closes both axes",
        match(dict(Kind="movement-requested", Revision=5, Sequence=5), first_event),
    )
    check(
        "first input after joins Submit",
        match(
            {
                k: (submit.get("state") or {}).get(k, absent)
                for k in (
                    "sessionId",
                    "revision",
                    "observationSequence",
                    "mode",
                    "map",
                    "cursor",
                    "wait",
                )
            },
            inp.get("after", absent),
        ),
    )
    # Any selected contradictory settings remain a failure; later agreement cannot
    # replace a missing initial state. Sparse result states do not invent settings.
    for channel in ("samples", "warpRecords"):
        for row in actual.get(channel, []):
            state = row.get("state") or {}
            if (state.get("cursor") or {}).get("Program") != "cs-5145c":
                continue
            check("selected opening session", match(session, state.get("sessionId", absent)))
            if "textSettings" in state:
                check(
                    "selected opening settings",
                    match(dict(MouthControl=0, ViewSpeed=0), state["textSettings"]),
                )
