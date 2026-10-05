"""Join scene fade requests, playback replacement and actual phase release."""


def compare_scene_releases(
    actual, session, events, fades, instances, receipts, wrappers, releases, check
):
    phase_producers = {
        e["Sequence"]: e
        for e in events.values()
        if e["Kind"] == "scene-prepared"
        or e["Kind"] == "scene-step-started"
        and e["Detail"] == "End"
    }
    phases = {}
    for row in actual.get("sceneObservations", []):
        scene = row["scene"]
        if scene.get("phase") in ("Initialize", "End") or scene.get("waitToken") in phase_producers:
            check(
                "scene consumer identity",
                row.get("sessionId") == session and scene.get("error") is None,
            )
            phases.setdefault(scene["waitToken"], []).append(row)
    check(
        "complete reached fade/scene dependency inventory",
        bool(phases)
        and len(fades) == len(phases)
        and {f["WaitToken"] for f in fades} == set(phases) == set(phase_producers),
    )
    for token, rows_for_token in phases.items():
        controls = [f for f in fades if f["WaitToken"] == token]
        before = [row for row in rows_for_token if not row["scene"]["completed"]]
        after = [row for row in rows_for_token if row["scene"]["completed"]]
        if len(controls) != 1 or not before or not after:
            check(
                "fade has start and actual completion",
                None if not before or not after else False,
                token=token,
            )
            continue
        control, first, completed = controls[0], before[0], after[0]
        producer = phase_producers.get(token)
        if producer is None:
            check("scene phase producer absent", None, token=token)
            continue
        phase = "Initialize" if producer["Kind"] == "scene-prepared" else "End"
        phase_fields = ("phase", "actionKind")
        if any(key not in row["scene"] for row in rows_for_token for key in phase_fields):
            check("actual phase consumer identity absent", None, token=token)
            actions = [
                row["scene"]["actionKind"] for row in rows_for_token if "actionKind" in row["scene"]
            ]
            if any(
                "phase" in row["scene"] and row["scene"]["phase"] != phase for row in rows_for_token
            ) or any(action != actions[0] for action in actions):
                check(
                    "actual completed phase retains the same consumer identity", False, token=token
                )
            continue
        check(
            "actual completed phase retains the same consumer identity",
            all(row["scene"]["phase"] == phase for row in rows_for_token)
            and all(
                row["scene"][key] == first["scene"][key]
                for row in rows_for_token
                for key in phase_fields
            ),
            token=token,
        )
        release = events.get(token + 1)
        old = [
            i
            for i in instances.values()
            if i["start"]["Cue"] == control["Cue"]
            and i["start"]["Sequence"] < control["Sequence"]
            and i["end"] is not None
            and i["end"]["Sequence"] > control["Sequence"]
        ]
        restored = [
            r
            for r in receipts
            if r["Operation"] == "started"
            and r["Command"] < 65
            and r["WaitToken"] == token
            and r["Sequence"] > control["Sequence"]
        ]
        if len(old) != 1 or len(restored) != 1 or release is None:
            check(
                "fade stop/replacement/logical release operands",
                None if release is None else False,
                token=token,
            )
            continue
        stopped, new = old[0]["end"], restored[0]
        new_poll = wrappers[int(new["Sequence"]) - 1]["poll"]
        check(
            "actual fade stop and music restore precede matching release",
            stopped["Operation"] == "stopped"
            and control["Revision"] == first["revision"] == stopped["Revision"] == new["Revision"]
            and control["Sequence"] < stopped["Sequence"] < new["Sequence"]
            and stopped["WaitToken"] == token
            and first["hostUpdate"] <= new_poll["hostUpdate"] <= completed["hostUpdate"]
            and release["Kind"]
            == (
                "scene-delivery"
                if first["scene"]["actionKind"] == "heal"
                else "scene-step-completed"
            )
            and release["Detail"] == phase
            and new["Revision"] < release["Revision"] <= completed["revision"]
            and (
                new["Command"] == (2 if producer["Actor"]["Value"].startswith("ally-") else 5)
                if phase == "Initialize"
                else new["Command"] == 34
            ),
            token=token,
        )
        releases.append(
            dict(
                token=token,
                phase=phase,
                fade=control["Sequence"],
                stop=stopped["Sequence"],
                restart=new["Sequence"],
                release=release["Sequence"],
            )
        )
