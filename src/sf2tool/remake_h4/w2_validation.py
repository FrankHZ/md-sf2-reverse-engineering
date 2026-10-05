"""Compare every matching W2 validation start and its whole Submit receipt."""

from sf2tool.remake_h4.w2_checks import merge


def compare_validation(actual, owner, session, revision, receipts, ordinal, checks):
    check, match, one, absent = checks.check, checks.match, checks.one, checks.absent
    audio_sequences = owner.get("validationSequences") or []
    validation_starts = [
        a
        for a in actual.get("audioReceipts", [])
        if (a.get("poll") or {}).get("sessionId") == session
        and (a.get("receipt") or {}).get("Revision") == revision
        and (a.get("receipt") or {}).get("Command") == 67
        and (a.get("receipt") or {}).get("Operation") == "started"
    ]
    check(
        "complete Submit validation start coverage",
        None
        if not validation_starts
        else merge(
            [
                len(validation_starts) == 1,
                *[
                    match(
                        audio_sequences[0] if audio_sequences else absent,
                        (a.get("receipt") or {}).get("Sequence", absent),
                    )
                    for a in validation_starts
                ],
            ]
        ),
        ordinal,
    )
    check(
        "one expected validation receipt",
        None if not audio_sequences else len(audio_sequences) == 1,
        ordinal,
    )
    for sequence in audio_sequences:
        audio = one("actual validation playback", receipts.get(sequence, []), ordinal)
        check(
            "validation whole Submit binding",
            match(
                dict(
                    poll=dict(sessionId=session),
                    receipt=dict(
                        Sequence=sequence,
                        Revision=revision,
                        Command=67,
                        Operation="started",
                        Playing=True,
                    ),
                ),
                audio,
            ),
            ordinal,
        )
