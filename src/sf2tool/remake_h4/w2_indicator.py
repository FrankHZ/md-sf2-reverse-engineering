"""Compare W2 retained copy and distinct same-submit or later-state indicators."""


def compare_indicator(
    owner, sample_indices, found, session, program, revision, after, ordinal, checks
):
    check, match, one, absent = checks.check, checks.match, checks.one, checks.absent
    later = owner.get("text") in (2299, 2303)
    indicator = one(
        "indicator witness", sample_indices.get(owner.get("indicatorIndex"), []), ordinal
    )
    check(
        "indicator occurrence identity",
        match(
            dict(label=owner.get("afterSample"), state=owner.get("indicatorIdentity", absent)),
            indicator,
        ),
        ordinal,
    )
    visible = indicator.get("state") or {}
    check(
        "actual retained text copy",
        match(found["text-seed-copy"].get("After", absent), visible.get("randomSeedCopy", absent)),
        ordinal,
    )
    check(
        "actual indicator clear",
        match(
            dict(
                sessionId=session,
                logicalText=dict(Indicator=0, IndicatorVisible=False),
            ),
            visible,
        ),
        ordinal,
    )
    if later:
        check(
            "later next-display witness",
            match(
                dict(
                    cursor=dict(Program=program),
                    fieldText=dict(Text=owner["text"] + 1),
                    wait="FieldTextWait",
                    canWaitForText=True,
                ),
                visible,
            ),
            ordinal,
        )
        check(
            "later witness follows accepted result",
            None
            if visible.get("revision") is None or revision is None
            else visible["revision"] > revision,
            ordinal,
        )
    else:
        check(
            "same-submit indicator identity",
            match(
                dict(
                    revision=revision,
                    token=after.get("token", absent),
                    cursor=after.get("cursor", absent),
                ),
                visible,
            ),
            ordinal,
        )
    return later
