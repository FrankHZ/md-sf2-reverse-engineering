"""Compare accepting source continuations or retention of the neutral consumer."""

from sf2tool.remake_h4.w1_checks import merge


def compare_continuation(
    accepting,
    cur,
    token,
    ready,
    after,
    events,
    open_portrait,
    read_positions,
    source,
    ordinal,
    checks,
):
    check, match, absent = checks.check, checks.match, checks.absent
    if accepting:
        paths = (
            (
                source.resume_paths(cur, False) + source.resume_paths(cur, True)
                if "portraitWindow" not in ready
                else source.resume_paths(cur, open_portrait)
            )
            if cur
            else []
        )
        observed = [e for e in events if e.get("Kind") == "program-instruction"]
        compatible = []
        for path, terminal, wait in paths:
            identities = {
                (e["Program"]["Program"], e["Program"]["Instruction"]): i
                for i, e in enumerate(path)
            }
            ranks = []
            values = []
            for e in observed:
                position = e.get("Program") or {}
                rank = identities.get((position.get("Program"), position.get("Instruction")))
                if rank is None:
                    values.append(False)
                else:
                    ranks.append(rank)
                    values.append(match(path[rank], e))
            values.extend(
                [
                    ranks == sorted(set(ranks)),
                    True if len(ranks) == len(path) else None,
                    match(terminal, after.get("cursor", absent)),
                    match(wait, after.get("wait", absent)),
                ]
            )
            compatible.append(merge(values))
        check(
            "source continuation producers and terminal",
            True if True in compatible else None if None in compatible or not paths else False,
            ordinal,
        )
        check(
            "accepted token released",
            None if after.get("token") is None else after["token"] != token,
            ordinal,
        )
        accepts = [i for i, e in enumerate(events) if e.get("Kind") == "text-w1-accepted"]
        check(
            "accept before source continuation",
            None
            if not accepts or not read_positions
            else accepts[0] == read_positions[0] + 1
            and all(
                i > accepts[0]
                for i, e in enumerate(events)
                if e.get("Kind") == "program-instruction"
            ),
            ordinal,
        )
    else:
        check(
            "neutral retains exact consumer",
            match(dict(token=token, cursor=cur, wait="FieldTextWait", canWaitForText=True), after),
            ordinal,
        )
