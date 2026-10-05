"""Bind selected audio metadata, complete receipt channel and producer session."""


def select_identity(actual, context, check):
    assets = {row["cue"]: row for row in context["audio"]}
    wrappers = actual["audioReceipts"]
    receipts = [row["receipt"] for row in wrappers]
    check("unique selected audio cues", len(assets) == len(context["audio"]))
    if any(r["Cue"] not in assets for r in receipts if r["Operation"] != "fade-command"):
        check("selected reached audio metadata absent", None)
        return None
    terminal = actual.get("audioTerminal") or {}
    session_ids = {row["poll"].get("sessionId") for row in wrappers}
    check(
        "one session and complete ordered receipt channel",
        len(session_ids) == 1
        and None not in session_ids
        and not actual.get("audioReceiptGaps")
        and [r["Sequence"] for r in receipts] == list(range(1, len(receipts) + 1))
        and actual.get("audioSequenceSeen") == terminal.get("sequence") == len(receipts),
    )
    check("terminal player error absent", terminal.get("error") is None)
    session = next(iter(session_ids))
    check(
        "selected audio producer session identity",
        None if context.get("sessionId") is None else context["sessionId"] == session,
    )
    return assets, wrappers, receipts, terminal, session
