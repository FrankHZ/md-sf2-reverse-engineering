"""Admit retained installed queues and the independent semantic event census."""


def compare_installations(context, channels, session, census, checks):
    check, match, absent = checks.check, checks.match, checks.absent
    queues = {}
    for installed in context.get("queues") or []:
        round_, channel, index = (
            installed.get("round"),
            installed.get("channel"),
            installed.get("index"),
        )
        check(
            "explicit one-based/zero-based join",
            match(index + 1, installed.get("ordinal", absent)) if type(index) is int else None,
        )
        queue = installed.get("queue")
        check(
            "independent full64 installed queue",
            None if queue is None else isinstance(queue, list) and len(queue) == 64,
            round=round_,
        )
        check("unique installed round", round_ not in queues, round=round_)
        queues[round_] = queue
        state = (channels.get(channel, {}).get(index) or {}).get("state") or {}
        check(
            "supplied installation at retained seam",
            match(dict(sessionId=session, round=round_, queueCursor=0, turnOrder=queue), state),
            round=round_,
        )
    check("independent installed rounds", True if queues else None)
    check("independent semantic census", True if census else None)
    expected = {c[2]: c for c in census}
    check(
        "admitted source consumer kinds",
        all(
            c[3]
            in (
                "round-started",
                "round-rng",
                "player-control",
                "regions-tested-cleared",
                "action-committed",
                "after-turn",
                "dead-entry-skipped",
                "ai-stay",
                "battle-outcome",
                "hp",
                "heal",
                "death-cleanup",
            )
            for c in census
        ),
    )
    check(
        "unique ordered census identities",
        len(expected) == len(census) and list(expected) == sorted(expected),
    )
    round_sequences = (context.get("roundSelection") or {}).get("roundSequences")
    check(
        "independent completed round selection",
        match(round_sequences, [c[2] for c in census if c[3] in ("round-started", "round-rng")])
        if round_sequences is not None
        else None,
    )
    for receipt in context.get("selectionReceipts") or []:
        check("completed retained selection", match(None, receipt.get("failure", absent)))
        for kind in sorted({c[3] for c in census}):
            count = (receipt.get("eventKinds") or {}).get(kind)
            if count is not None:
                check(
                    "independent census count " + kind, count == sum(c[3] == kind for c in census)
                )
    check("retained selection receipt", True if context.get("selectionReceipts") else None)
    return queues, expected
