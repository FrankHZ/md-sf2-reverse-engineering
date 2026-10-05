"""Associate playback lifetimes and source-compatible replacement causes."""


def compare_playbacks(receipts, assets, terminal, slots, types, playbacks, check):
    metadata = (
        "Command",
        "Cue",
        "TimerB",
        "PcmSha256",
        "SampleFrames",
        "SampleRate",
        "Channels",
        "LoopBegin",
        "LoopEnd",
        "RequestedTimerB",
    )
    fields = dict(
        Command="command",
        TimerB="timerB",
        PcmSha256="sha256",
        SampleFrames="sampleFrames",
        SampleRate="sampleRate",
        Channels="channels",
        LoopBegin="loopBegin",
        LoopEnd="loopEnd",
    )
    pending, instances, fades = {}, {}, []
    music_timer = None
    for index, r in enumerate(receipts):
        cue, operation, command = r["Cue"], r["Operation"], r["Command"]
        check(
            "receipt operation",
            operation in ("started", "stopped", "finished", "fade-command"),
            sequence=r["Sequence"],
        )
        if operation == "fade-command":
            fades.append(r)
            check("asynchronous fade request identity", command == 253, sequence=r["Sequence"])
            continue
        asset = assets.get(cue)
        check(
            "admitted command/timer/PCM operand",
            None
            if asset is None
            else all(r[key] == asset.get(field) for key, field in fields.items()),
            sequence=r["Sequence"],
        )
        if operation == "started":
            check(
                "actual started player",
                r["Playing"] and r["PlaybackPosition"] >= 0,
                sequence=r["Sequence"],
            )
            if cue in pending:
                check("same-cue instance association ambiguous", None, sequence=r["Sequence"])
                return None
            pending[cue] = r
            instances[r["Sequence"]] = dict(start=r, end=None)
            if command < 65:
                music_timer = r["TimerB"]
            else:
                candidates = [a for a in assets.values() if a["command"] == command]
                exact = [a for a in candidates if a["timerB"] == r["RequestedTimerB"]]
                selected = exact if exact else candidates
                check(
                    "exact-or-unique inherited timer selection",
                    r["RequestedTimerB"] == music_timer
                    and len(selected) == 1
                    and selected[0]["cue"] == cue,
                    sequence=r["Sequence"],
                )
                check(
                    "source slot class available", bool(slots.get(command)), sequence=r["Sequence"]
                )
        elif operation in ("finished", "stopped"):
            start = pending.pop(cue, None)
            if start is None:
                check("terminal event has a unique prior start", False, sequence=r["Sequence"])
                continue
            instances[start["Sequence"]]["end"] = r
            check(
                "same actual playback operands",
                all(start[k] == r[k] for k in metadata) and not r["Playing"],
                sequence=r["Sequence"],
                start=start["Sequence"],
            )
            if operation == "finished":
                check(
                    "natural completion belongs to finite playback",
                    start["LoopBegin"] is None and start["LoopEnd"] is None,
                    start=start["Sequence"],
                )
            else:
                # Stop and its replacement share the actual service context. Do not
                # equate current WaitToken with the start's (possibly earlier) token.
                replacements = []
                for following in receipts[index + 1 :]:
                    if (following["Revision"], following["WaitToken"]) != (
                        r["Revision"],
                        r["WaitToken"],
                    ):
                        break
                    if following["Operation"] == "started":
                        new = following["Command"]
                        if (
                            command < 65
                            and new < 65
                            or command >= 65
                            and new >= 65
                            and slots.get(command, set()) <= slots.get(new, set())
                            or command >= 65
                            and types.get(command) == 1
                            and new < 65
                        ):
                            replacements.append(following)
                check(
                    "stop has legitimate replacement/shared-release cause",
                    bool(replacements),
                    start=start["Sequence"],
                    stop=r["Sequence"],
                    replacements=[x["Sequence"] for x in replacements],
                )
    for start_sequence, instance in instances.items():
        start, end = instance["start"], instance["end"]
        playbacks.append(
            dict(
                start=start_sequence,
                end=end["Sequence"] if end else None,
                operation=end["Operation"] if end else "ongoing",
                cue=start["Cue"],
                command=start["Command"],
            )
        )
        if end is None:
            check(
                "ongoing playback is a live admitted loop",
                start["LoopBegin"] is not None
                and start["LoopEnd"] is not None
                and start["Cue"] == terminal.get("musicCue")
                and terminal.get("musicPlaying"),
                start=start_sequence,
            )
    live_sounds = terminal.get("sounds", [])
    check(
        "terminal voices exactly match outstanding instances",
        not live_sounds and len(pending) == int(bool(terminal.get("musicPlaying"))),
    )

    return instances, fades
