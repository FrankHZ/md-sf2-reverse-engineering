"""Actual finite JOIN completion, helper services and previous-music restart."""


def join_audio(
    actual,
    records,
    late,
    logical,
    install,
    generation,
    world_path,
    plain,
    li,
    pi,
    observations,
    request,
    early_world,
    source_operation,
    held,
    completed,
    released,
    previous,
    pressed,
    plain_value,
    result,
    set_plain,
    _bounded_list,
):
    music = logical["music"] if late else None
    helper = logical["musicWait"] if late else None
    helper_token = helper["Token"]["Value"] if late else install["Sequence"]
    receipts = _bounded_list(r["receipt"] for r in actual["audioReceipts"])
    previous_starts = [
        r
        for r in receipts
        if r["Operation"] == "started"
        and r["Cue"].startswith("MUSIC_")
        and r["Revision"] < generation
    ]
    if not late and not previous_starts:
        return None
    previous_cue = music["Previous"][0] if late else previous_starts[-1]["Cue"]
    starts = [
        r
        for r in receipts
        if r["Cue"] == "MUSIC_JOIN" and r["Operation"] == "started" and r["Revision"] == generation
    ]
    finishes = [
        r
        for r in receipts
        if r["Cue"] == "MUSIC_JOIN"
        and r["Operation"] == "finished"
        and (
            r["Revision"] == logical["revision"]
            if late
            else generation <= r["Revision"] < plain["revision"]
        )
    ]
    if not starts or not finishes:
        return None
    start, finish = starts[0], finishes[0]
    finish_context = next(
        (
            row["state"]
            for row in reversed(records)
            if row.get("state", {}).get("revision") == finish["Revision"]
        ),
        None,
    )
    if not late and (finish_context is None or "token" not in finish_context):
        return None
    restarts = [
        r
        for r in receipts
        if r["Cue"] == previous_cue
        and r["Operation"] == "started"
        and finish["Sequence"] < r["Sequence"]
        and r["Revision"] < plain["revision"]
    ]
    transitional = _bounded_list(
        s["state"]
        for s in (actual["samples"][li + 1 : pi] if late else [])
        if s["state"]["audio"]["musicGeneration"] == generation
    )
    if not restarts or (late and not transitional):
        return None
    restart = restarts[0]
    interval = [
        r
        for r in receipts
        if start["Sequence"] <= r["Sequence"] <= restart["Sequence"]
        and r["Cue"].startswith("MUSIC_")
    ]
    if not late:
        if not world_path.is_file():
            return None
        request_op, install_op = source_operation(request), source_operation(install)
        profile = next(
            (row for row in early_world["presentation"]["audio"] if row["cue"] == "MUSIC_JOIN"),
            None,
        )
        if profile is None or profile.get("modernEndStep") is None:
            return None
        end = profile["modernEndStep"]
        before_steps = [
            o
            for _, o in observations
            if generation < o["Sequence"] < helper_token
            and o["Kind"] == "music-step"
            and o.get("Detail") == "MUSIC_JOIN"
        ]
        initial_step = min(end, len(before_steps))
        progress = [
            o
            for _, o in observations
            if helper_token < o["Sequence"] < released[0][1]["Sequence"]
            and o["Kind"] in ("music-step", "music-wait-armed", "music-previous-eligible")
            and o.get("Detail") == "MUSIC_JOIN"
        ]
        services = [
            o
            for _, o in observations
            if helper_token < o["Sequence"] < released[0][1]["Sequence"]
            and o["Kind"] == "music-helper-service"
        ]
        armed = [o for o in progress if o["Kind"] == "music-wait-armed"]
        eligible = [o for o in progress if o["Kind"] == "music-previous-eligible"]
        if not progress or not services or not armed or not eligible:
            return None
        if completed[0][1]["Sequence"] > services[-1]["Sequence"]:
            # A real late-held interval requires its own retained state and
            # attempted-input/no-debt operands; its missing sample is not early.
            return None
        needed = max(2, end - initial_step)
        groups = ((needed + 2) // 3) * 3
        attempts = [
            r
            for r in pressed
            if helper_token <= r["before"]["revision"] < released[0][1]["Sequence"]
        ]
        plain_value = plain_value and all(
            r["resultStart"] == r["resultEnd"] and r["before"] == r["after"] for r in attempts
        )
        set_plain(plain_value)
        result["audio"] = (
            result["audio"] is not False
            and len(starts) == len(finishes) == len(restarts) == 1
            and interval == [start, finish, restart]
            and request["Kind"] == install["Kind"] == "program-instruction"
            and request_op is not None
            and request_op.get("op") == "present"
            and request_op.get("kind") == "Sound"
            and request_op.get("resource") == "MUSIC_JOIN"
            and install_op is not None
            and install_op.get("op") == "present"
            and install_op.get("kind") == "SoundWait"
            and install["Program"] == logical["cursor"]
            and len(progress) == len(services) == groups
            and len(armed) == len(eligible) == 1
            and progress[0] == armed[0]
            and eligible[0]["Sequence"] == progress[int(needed) - 1]["Sequence"]
            and all(
                p["Sequence"] < v["Sequence"]
                and (index + 1 == len(progress) or v["Sequence"] < progress[index + 1]["Sequence"])
                for index, (p, v) in enumerate(zip(progress, services, strict=False))
            )
            and all(
                state["token"] == helper_token
                and state["cursor"] == logical["cursor"]
                and state["sessionId"] == plain["sessionId"]
                and state["simulationTick"]
                == logical["simulationTick"]
                - sum(v["Sequence"] <= logical["revision"] for v in services)
                + sum(v["Sequence"] <= state["revision"] for v in services)
                for _, state in held
            )
            and finish_context["sessionId"] == plain["sessionId"]
            and finish["WaitToken"] == finish_context["token"]
            and start["PcmSha256"] == finish["PcmSha256"]
            and not finish["Playing"]
            and restart["Playing"]
            and finish["Revision"] < completed[0][1]["Sequence"]
            and restart["Revision"] < previous[0][1]["Sequence"]
            and plain["audio"]["musicCue"] == restart["Cue"]
            and plain["audio"]["musicPlaying"]
            and plain["audio"]["error"] is None
        )
        result["anchors"]["earlyLogicalWork"] = dict(
            request=generation,
            helper=helper_token,
            endStep=end,
            initialStep=initial_step,
            progressSequences=[p["Sequence"] for p in progress],
            serviceSequences=[v["Sequence"] for v in services],
            eligible=eligible[0]["Sequence"],
            previousCue=previous_cue,
        )
    else:
        result["audio"] = (
            len(starts) == len(finishes) == len(restarts) == 1
            and interval == [start, finish, restart]
            and music["Cue"] == "MUSIC_JOIN"
            and music["Step"] == music["EndStep"]
            and music["PreviousEligible"]
            and not music["ActualDone"]
            and helper["Generation"] == generation
            and helper["LogicalDone"]
            and helper["Armed"]
            and helper["Cleared"]
            and logical["audio"]["musicGeneration"] == generation
            and logical["audio"]["musicPlaying"]
            and not logical["audio"]["musicFinished"]
            and finish["WaitToken"] == helper["Token"]["Value"]
            and start["PcmSha256"] == finish["PcmSha256"]
            and not finish["Playing"]
            and restart["Playing"]
            and all(
                s["audio"]["musicFinished"]
                and not s["audio"]["musicPlaying"]
                and s["audio"]["error"] is None
                for s in transitional
            )
            and plain["audio"]["musicCue"] == restart["Cue"]
            and plain["audio"]["musicPlaying"]
            and plain["audio"]["musicPosition"] > 0
            and plain["audio"]["error"] is None
            and completed[0][1]["Sequence"] > finish["Revision"]
            and restart["Revision"] < previous[0][1]["Sequence"]
        )
    return helper_token, interval
