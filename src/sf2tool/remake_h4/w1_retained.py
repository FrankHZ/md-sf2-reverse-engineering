"""Bind closing portraits, later copied state, restoration and final event clocks."""

from sf2tool.remake_h4.w1_checks import merge


def compare_retained(
    poll,
    polls,
    actual,
    supplement,
    context,
    states,
    before,
    after,
    submit,
    post,
    field,
    events,
    open_portrait,
    expected_portrait,
    byte,
    checks,
):
    check, match, absent = checks.check, checks.match, checks.absent
    ordinal, accepting = poll.get("ordinal"), poll.get("accepting")
    closing = any(
        e.get("Kind") == "portrait-window-moving" and e.get("Detail") == "close" for e in events
    )
    if open_portrait and not closing and post and expected_portrait is not None:
        check(
            "retained portrait clock state",
            match(expected_portrait, post.get("portraitWork", absent)),
            ordinal,
        )
    if closing:
        check(
            "source close boundary",
            accepting and after.get("wait") == "PortraitMovementWait",
            ordinal,
        )
        if post:
            check(
                "source close unregisters portrait",
                match(dict(Registered=False, Closing=True), post.get("portraitWork", absent)),
                ordinal,
            )
    if poll.get("after"):
        copy_state = post
    else:
        witness = context.get("copyWitnesses", {}).get(str(ordinal), {})
        copy_state = (
            states.get((witness.get("channel"), witness.get("index")), {})
            if witness.get("channel") != "outcomeFinal"
            else actual.get("w1OutcomeFinal", supplement.get("w1OutcomeFinal", {}))
        )
        check(
            "later copy witness identity",
            match(witness.get("identity", absent), copy_state),
            ordinal,
        )
        check(
            "later copy follows submit",
            None
            if copy_state.get("revision") is None or after.get("revision") is None
            else copy_state["revision"] > after["revision"],
            ordinal,
        )
        check(
            "later witness retains copy before next W1 poll",
            not any(
                p.get("beforeRevision", 0) >= poll.get("afterRevision", 0)
                and p.get("afterRevision", 0) <= copy_state.get("revision", 0)
                for p in polls
                if p is not poll
            ),
            ordinal,
        )
        check("close witness is compositional", closing, ordinal)
    check(
        "actual retained poll copy",
        match(byte if byte is not None else absent, copy_state.get("randomSeedCopy", absent)),
        ordinal,
    )
    if poll.get("after"):
        check(
            "same-submit typewriting restoration and debt",
            match(
                dict(
                    **(
                        {}
                        if accepting and closing
                        else dict(
                            typewriting=field.get("SavedTypewriting", absent)
                            if accepting
                            else False
                        )
                    ),
                    tickDebt=0,
                ),
                post,
            ),
            ordinal,
        )
    revisions = [e.get("Revision") for e in events]
    sequences = [e.get("Sequence") for e in events]
    for label, values, low, high in (
        ("revision", revisions, before.get("revision"), submit.get("revision")),
        (
            "sequence",
            sequences,
            before.get("observationSequence"),
            submit.get("observationSequence"),
        ),
    ):
        available = [v for v in values if v is not None]
        check(
            "Submit event " + label + " order",
            merge(
                [
                    True if len(available) == len(values) else None,
                    available == sorted(set(available)),
                    None
                    if low is None or high is None
                    else all(low < v <= high for v in available),
                ]
            ),
            ordinal,
        )
