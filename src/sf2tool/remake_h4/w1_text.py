"""Compare source caller/speaker ancestry and the displayed complete token stream."""


def compare_text(poll, owner, ready, owners, polls, states, source, session, checks):
    check, match, absent = checks.check, checks.match, checks.absent
    ordinal = poll.get("ordinal")
    cur, token = owner.get("cursor") or {}, poll.get("token")
    programs, texts, portrait_ids = source.programs, source.texts, source.portrait_ids
    program = cur.get("Program")
    suppressed = program in ("byte-50f6a", "byte-51052", "byte-53ec8")
    caller = (
        "EntityEventContext"
        if suppressed or program in ("cs-52f0c", "cs-52f24", "cs-52f40")
        else (None if program in ("cs-53996", "bbcs-01", "abcs-battle01") else "ZoneEventContext")
    )
    check(
        "source caller and W1 eligibility",
        match(
            dict(
                sessionId=session,
                token=token,
                cursor=cur,
                tickDebt=0,
                eventCaller=caller,
                entitiesRunning=not suppressed,
                typewriting=False,
                canWaitForText=True,
                logicalView=dict(HideWindows=False, Scrolling=False),
                wait="FieldTextWait",
                fieldText=dict(
                    Text=owner.get("text"),
                    Wait2=False,
                    Revealed=True,
                    LogicalDone=True,
                    Phase=7,
                    Token=dict(Value=token),
                    End=owner.get("position"),
                ),
            ),
            ready,
        ),
        ordinal,
    )
    field = ready.get("fieldText") or {}
    instructions = programs.get(program, {}).get("instructions", [])
    n = cur.get("Instruction")
    op = instructions[int(n)] if isinstance(n, (int, float)) and 0 <= n < len(instructions) else {}
    check("source text producer", match(dict(op="show-text"), op), ordinal)
    speaker = (
        ((ready.get("entityEvent") or {}).get("Entity") or {}).get("Value")
        if op.get("useEventSpeaker")
        else op.get("speaker")
    )
    flags = op.get("speakerFlags", 0)
    check(
        "source speaker and flags",
        None if not op else match(dict(speaker=speaker, speakerFlags=flags), ready),
        ordinal,
    )
    speaker_entity = next((e for e in ready.get("entities", []) if e.get("id") == speaker), {})
    if speaker is None and flags == 255:
        portrait_id = -1
    elif speaker_entity.get("sprite") is not None and portrait_ids:
        portrait_id = portrait_ids.get(speaker_entity["sprite"], -1)
    else:
        portrait_id = None
    portrait_flags = flags
    if program == "cs-51614" and n == 3:
        # OpenPortraitWindow returns immediately for an already open window.
        # The continued531 yes/no branch retains the original mirrored side.
        parent_ops = programs.get("cs-5149a", {}).get("instructions", [])
        branch = next(
            (
                i
                for i, x in enumerate(parent_ops)
                if x.get("op") == "branch-flag"
                and (x.get("target") or {}).get("program") == program
            ),
            None,
        )
        preceding = (
            [x for x in parent_ops[:branch] if x.get("op") == "show-text"]
            if branch is not None
            else []
        )
        portrait_flags = preceding[-1].get("speakerFlags") if preceding else None
        donors = [o for o in owners if o.get("text") == 531]
        donor_states = [
            states.get((ref.get("channel"), ref.get("index")), {})
            for o in donors
            for p in polls
            if p.get("token") == o.get("token")
            for ref in p.get("ready", [])
        ]
        check(
            "continued portrait source ancestry",
            None
            if not donor_states or not preceding
            else preceding[-1].get("mode") == "continued"
            and all(
                match(
                    dict(
                        portraitId=portrait_id,
                        portraitFlags=portrait_flags,
                        portraitWindow="OpenPortraitWindow",
                    ),
                    d,
                )
                is True
                for d in donor_states
            ),
            ordinal,
        )
    check(
        "source portrait selection",
        None
        if portrait_id is None
        else match(
            dict(
                portraitId=None if portrait_id < 0 else portrait_id,
                portraitFlags=None if portrait_id < 0 else portrait_flags,
                portraitWindow="ClosedPortraitWindow" if portrait_id < 0 else "OpenPortraitWindow",
            ),
            ready,
        ),
        ordinal,
    )
    expected_text = None
    if op:
        prior = instructions[: int(n) + 1]
        resets = [i for i, x in enumerate(prior) if x.get("op") == "text-cursor"]
        if resets:
            i = resets[-1]
            expected_text = (
                prior[i]["text"] + sum(x.get("op") == "show-text" for x in prior[i + 1 :]) - 1
            )
    check(
        "source active text identity",
        None if expected_text is None else match(expected_text, field.get("Text", absent)),
        ordinal,
    )
    expected_units = (
        source.source_units(texts[owner["text"]]) if owner.get("text") in texts else None
    )
    units = field.get("Units")
    check(
        "complete ordered source token stream",
        None
        if expected_units is None or units is None
        else [(u.get("Kind"), u.get("Text")) for u in units] == expected_units,
        ordinal,
    )
    end_unit = field.get("End")
    check(
        "selected W1 position",
        None
        if expected_units is None or end_unit is None
        else 0 <= end_unit < len(expected_units) and expected_units[int(end_unit)][0] == 3,
        ordinal,
    )
    return suppressed, field
