"""Bind finite music return to the same session and Confirm transition."""


def compare_confirm(actual, session, start, release, restart, check):
    plain = [s["state"] for s in actual.get("samples", []) if s["label"] == "music-plain-input"]
    polled = [s["state"] for s in actual.get("samples", []) if s["label"] == "music-plain-poll"]
    acked = [s["state"] for s in actual.get("samples", []) if s["label"] == "music-plain-accepted"]
    confirms = [
        r
        for r in actual.get("inputRecords", [])
        if r.get("pressed")
        and r.get("action") == "confirm"
        and plain
        and (
            r["before"].get("token") == plain[0]["token"]
            or polled
            and r["before"].get("revision") == polled[0]["revision"]
        )
    ]
    ready = [s["state"] for s in actual.get("samples", []) if s["label"] == "join-field-return"]
    wait_fields = ("sessionId", "revision", "token", "wait", "simulationTick", "mainSeed")
    if not confirms or not polled or not acked:
        check("matching plain input wait identity absent", None, start=start["Sequence"])
    else:
        input_sides = (confirms[0]["before"], confirms[0]["after"])
        check(
            "plain input and samples belong to this session",
            False
            if any(
                "sessionId" in s and s["sessionId"] != session
                for s in (*input_sides, polled[0], acked[0])
            )
            else None
            if any("sessionId" not in s for s in (*input_sides, polled[0], acked[0]))
            else all(s["sessionId"] == session for s in (*input_sides, polled[0], acked[0])),
            start=start["Sequence"],
        )
        if any(
            key not in s
            for s in (*input_sides, plain[0], polled[0], acked[0])
            for key in wait_fields
        ):
            check("plain input before/after wait fields absent", None, start=start["Sequence"])
            return False
        check(
            "plain Confirm belongs to the same before/after wait",
            len(confirms) == len(polled) == len(acked) == 1
            and all(s["sessionId"] == session for s in (*input_sides, polled[0], acked[0]))
            and all(
                side[key] == sample[key]
                for side, sample in zip(input_sides, (polled[0], acked[0]), strict=True)
                for key in wait_fields
            )
            and polled[0]["token"] == plain[0]["token"]
            and polled[0]["wait"] == "DialogueWait"
            and acked[0]["wait"] == "TextCloseWait",
            start=start["Sequence"],
        )
    check(
        "restarted playback precedes plain input and caller return",
        None
        if not plain or not confirms or not ready
        else len(plain) == len(confirms) == len(ready) == 1
        and len(restart) == 1
        and release["Sequence"] < restart[0]["Revision"] < plain[0]["revision"]
        and plain[0]["sessionId"] == ready[0]["sessionId"] == session
        and plain[0]["wait"] == "DialogueWait"
        and plain[0]["audio"]["musicPlaying"]
        and plain[0]["audio"]["musicCue"] == restart[0]["Cue"]
        and confirms[0]["after"]["wait"] == "TextCloseWait"
        and confirms[0]["after"]["revision"] < ready[0]["revision"]
        and ready[0]["canWaitAtInput"]
        and ready[0]["wait"] is None,
        start=start["Sequence"],
    )
    return True
