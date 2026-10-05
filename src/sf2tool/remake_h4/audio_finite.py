"""Join finite music source requests, logical/actual completion and prior cue resume."""

from sf2tool.remake_h4.audio_confirm import compare_confirm


def compare_finite_releases(
    actual, context, session, events, assets, instances, receipts, releases, check
):
    # Finite music is the accepted modern clock. Original F0/channel/interleaving
    # Unknowns remain separate; mailbox dispatch never supplies this completion.
    ops = {(p["id"], p["instruction"]): p["operation"] for p in context["programs"]}
    producers = context["events"]
    check(
        "generic fade requires its own selected service consumer",
        None
        if any(
            ops.get((e["Program"]["Program"], e["Program"]["Instruction"]), {}).get("kind")
            == "SoundFade"
            for e in producers
        )
        else True,
    )
    check(
        "finite music dependency classification",
        all(
            i["start"]["Command"] == 19
            for i in instances.values()
            if i["start"]["Command"] < 65 and i["start"]["LoopBegin"] is None
        ),
    )
    joins = [i for i in instances.values() if i["start"]["Command"] == 19]
    check("reached finite music dependency present", bool(joins))
    for instance in joins:
        start, ended = instance["start"], instance["end"]
        if start["Cue"] not in assets:
            continue
        request = [
            e
            for e in producers
            if e["Sequence"] == start["Revision"]
            and ops.get((e["Program"]["Program"], e["Program"]["Instruction"]), {}).get("kind")
            == "Sound"
            and ops.get((e["Program"]["Program"], e["Program"]["Instruction"]), {}).get("resource")
            == start["Cue"]
        ]
        waits = [
            e
            for e in producers
            if e["Sequence"] > start["Revision"]
            and ops.get((e["Program"]["Program"], e["Program"]["Instruction"]), {}).get("kind")
            == "SoundWait"
        ]
        actual_done = [
            e
            for e in events.values()
            if e["Kind"] == "music-actual-completed" and e["Sequence"] > start["Revision"]
        ]
        returned = [
            e
            for e in events.values()
            if e["Kind"] == "music-wait-returned" and e["Sequence"] > start["Revision"]
        ]
        eligible = [
            e
            for e in events.values()
            if e["Kind"] == "music-previous-eligible" and e["Sequence"] > start["Revision"]
        ]
        if not ended or not request or not waits or not actual_done or not returned or not eligible:
            check(
                "finite generation completion/release operands absent",
                None,
                start=start["Sequence"],
            )
            continue
        helper, done, release, logical_done = waits[0], actual_done[0], returned[0], eligible[0]
        check(
            "finite completion and release retain this cue",
            False
            if any(
                "Detail" in e and e["Detail"] != start["Cue"]
                for e in actual_done + returned + eligible
            )
            else None
            if any("Detail" not in e for e in actual_done + returned + eligible)
            else all(e["Detail"] == start["Cue"] for e in actual_done + returned + eligible),
            start=start["Sequence"],
        )
        check(
            "wait remains owned by the requesting source program",
            request[0]["Program"]["Program"] == helper["Program"]["Program"],
            start=start["Sequence"],
        )
        progress = sorted(
            (
                e
                for e in events.values()
                if helper["Sequence"] < e["Sequence"] < release["Sequence"]
                and e["Kind"] in ("music-step", "music-wait-armed", "music-previous-eligible")
            ),
            key=lambda e: e["Sequence"],
        )
        initial_steps = [
            e
            for e in events.values()
            if start["Revision"] < e["Sequence"] < helper["Sequence"]
            and e["Kind"] == "music-step"
            and e["Detail"] == start["Cue"]
        ]
        armed = [e for e in progress if e["Kind"] == "music-wait-armed"]
        check(
            "helper logical progress belongs to this cue",
            False
            if any("Detail" in e and e["Detail"] != start["Cue"] for e in progress)
            else None
            if any("Detail" not in e for e in progress)
            else all(e["Detail"] == start["Cue"] for e in progress),
            start=start["Sequence"],
        )
        check(
            "accepted finite logical clock reaches end before release",
            assets[start["Cue"]].get("modernEndStep") == 505
            and len(armed) == 1
            and len(initial_steps)
            + sum(e["Sequence"] <= logical_done["Sequence"] for e in progress)
            == assets[start["Cue"]]["modernEndStep"]
            and progress[0] == armed[0],
            start=start["Sequence"],
        )
        previous = [
            e
            for e in producers
            if e["Sequence"] > release["Sequence"]
            and ops.get((e["Program"]["Program"], e["Program"]["Instruction"]), {}).get("kind")
            == "PreviousMusic"
        ]
        restart = [
            r
            for r in receipts
            if previous
            and r["Revision"] == previous[0]["Sequence"]
            and r["Operation"] == "started"
            and r["Command"] < 65
        ]
        prior = [
            r
            for r in receipts
            if r["Operation"] == "started"
            and r["Command"] < 65
            and r["Sequence"] < start["Sequence"]
        ]
        check(
            "finite generation owns actual finish before logical/actual joined release",
            len(request) == len(waits) == len(actual_done) == len(returned) == len(eligible) == 1
            and ended["Operation"] == "finished"
            and ended["WaitToken"] == helper["Sequence"]
            and start["Revision"] < helper["Sequence"] < release["Sequence"]
            and ended["Revision"] < done["Sequence"] < release["Sequence"]
            and logical_done["Sequence"] < release["Sequence"],
            start=start["Sequence"],
        )
        check(
            "previous music restarts same prior cue",
            bool(prior)
            and len(previous) == len(restart) == 1
            and restart[0]["Cue"] == prior[-1]["Cue"]
            and restart[0]["PlaybackPosition"] < 0.1,
            start=start["Sequence"],
        )
        if not compare_confirm(actual, session, start, release, restart, check):
            continue
        releases.append(
            dict(
                phase="finite-music",
                generation=start["Revision"],
                helper=helper["Sequence"],
                actualDone=done["Sequence"],
                logicalDone=logical_done["Sequence"],
                release=release["Sequence"],
            )
        )
