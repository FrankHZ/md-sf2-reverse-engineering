"""Terminal field-settle inference and actual release/attach/return consumers."""

from sf2tool.remake_h4.scene_checks import absent, actor


def compare_terminal(
    terminal,
    field_batches,
    warps,
    inputs,
    events,
    owners,
    ordered,
    physical_context,
    bytoken,
    session,
    checks,
):
    """Compose only the admitted terminal release/attach/field-return boundary."""
    check, eq = checks.check, checks.eq
    identity, precedes = checks.identity, checks.precedes
    compositions = []
    for pair in terminal:
        token, owner = pair["token"], pair["owner"]
        batch = field_batches[token]
        row = warps[owner]
        envelope = row["result"]
        eq("terminal actual non-input transport", False, row.get("inputDelivery", absent), token)
        eq(
            "terminal pending field transport",
            "field-view-pending",
            row.get("projection", absent),
            token,
        )
        eq("terminal Exploration result", "Exploration", envelope.get("mode", absent), token)
        ending = [events[q] for q in ordered if owners[q] == owner and q >= pair["end"]["Sequence"]]
        if ending:
            identity(
                "terminal final nested result boundary",
                dict(
                    sessionId=session,
                    revision=ending[-1].get("Revision"),
                    observationSequence=ending[-1].get("Sequence"),
                ),
                envelope,
                token,
            )
        initial_actor = next(
            (
                c.get("actor", absent)
                for c in physical_context.get("census", [])
                if c.get("sequence") == batch["scene"]
            ),
            absent,
        )
        eq(
            "terminal independent action actor",
            initial_actor,
            events.get(batch["scene"], {}).get("Actor", absent),
            token,
        )
        for q in ordered:
            if batch["scene"] < q < token and events[q]["Kind"] in (
                "scene-ended",
                "field-death-started",
            ):
                eq(
                    "terminal action release actor",
                    initial_actor,
                    events[q].get("Actor", absent),
                    q,
                )
        expected_end = [
            dict(
                Kind="scene-step-completed", Detail="FieldSettle", Actor=pair["start"].get("Actor")
            ),
            dict(Kind="field-death-ended", Actor=pair["start"].get("Actor")),
            dict(Kind="battle-outcome", Detail="Victory"),
            dict(Kind="action-committed", Actor=initial_actor),
            dict(Kind="outcome-program-started", Detail="Victory"),
            dict(Kind="program-instruction", Detail="SetTextCursor"),
            dict(Kind="program-instruction", Detail="ResetPartyBattleStats"),
            dict(Kind="program-instruction", Detail="SetCameraEntity"),
            dict(Kind="full-fade-started", Detail="FadeOut"),
        ]
        cursor = 0
        for event in ending:
            candidate = next(
                (
                    n
                    for n in range(cursor, len(expected_end))
                    if expected_end[n]["Kind"] == event.get("Kind")
                ),
                None,
            )
            if candidate is None:
                check("terminal ordered release without competing writer", False, token)
                continue
            if candidate > cursor:
                check("terminal required release events present", None, token)
            eq("terminal source release operand", expected_end[candidate], event, token)
            cursor = candidate + 1
        check(
            "terminal required release events present",
            True if cursor == len(expected_end) else None,
            token,
        )
        prior = batch["phases"][-2] if len(batch["phases"]) >= 2 else {}
        eq("terminal preceding phase", "FieldExit", prior.get("phase", absent), token)
        prior_rows = bytoken.get(prior.get("token"), [])
        check(
            "terminal prior exit delivered",
            any(r["scene"].get("completed") is True for r in prior_rows) or None,
            token,
        )
        cleanup = [
            events[q]
            for q in ordered
            if prior.get("end", {}).get("Sequence", float("inf")) < q < token
            and events[q]["Kind"] == "death-cleanup"
        ]
        eq("terminal prior cleanup census", batch["actors"], [actor(e) for e in cleanup], token)
        before_rows = bytoken.get(token, [])
        check("terminal prestate present", bool(before_rows) or None, token)
        for r in before_rows:
            eq("terminal prestate unfinished", False, r["scene"].get("completed", absent), token)
            precedes("terminal prestate before completion", r, envelope, token)
            eq(
                "terminal uninterrupted causal input",
                row.get("inputOrdinal", absent),
                r.get("inputOrdinal", absent),
                token,
            )
            for inp in inputs.values():
                if (
                    r.get("hostUpdate", float("inf")) < inp.get("hostUpdate", -1)
                    and inp.get("resultStart", float("inf")) <= owner
                ):
                    check("terminal no intervening physical input", False, token)
        attached = [
            (i, r)
            for i, r in warps.items()
            if i > owner
            and r["result"].get("boundary") == "attach"
            and r["result"].get("observationSequence") == envelope.get("observationSequence")
        ]
        check("terminal actual field attach", bool(attached) or None, token)
        for i, r in attached:
            eq("terminal attach accepted", None, r["result"].get("failure", absent), i)
            identity("terminal attach/result identity", envelope, r["result"], i)
            eq("terminal attach field mode", "Exploration", r["result"].get("mode", absent), i)
        check(
            "terminal has no intervening writer",
            not any(token < q < pair["end"]["Sequence"] for q in ordered),
            token,
        )
        returned = [
            events[q]
            for q in ordered
            if q > pair["end"]["Sequence"] and events[q]["Kind"] == "battle-returned"
        ]
        check("terminal accepted returned boundary", bool(returned) or None, token)
        for e in returned:
            return_row = warps[owners[e["Sequence"]]]
            eq("returned result accepted", None, return_row["result"].get("failure", absent), token)
            state = return_row["state"]
            eq(
                "returned field ownership",
                dict(
                    mode="Exploration",
                    map="map-57",
                    stop="PlayerInput",
                    wait=None,
                    sessionId=session,
                ),
                state,
                token,
            )
            identity("returned state/result", return_row["result"], state, token)
            pressed = [
                i
                for i in inputs.values()
                if i.get("pressed") is True and i.get("resultStart", -1) > owners[e["Sequence"]]
            ]
            check("returned actual field control", bool(pressed) or None, token)
            if pressed:
                first = pressed[0]
                delivered = warps.get(first.get("resultStart"), {})
                eq(
                    "returned input accepted",
                    dict(failure=None, mode="Exploration", sessionId=session),
                    delivered.get("result", absent),
                    token,
                )
                check(
                    "returned input actual movement request",
                    any(
                        owners[q] == first.get("resultStart")
                        and events[q]["Kind"] == "movement-requested"
                        for q in ordered
                    )
                    or None,
                    token,
                )
                if first.get("resultEnd") == first.get("resultStart", -2) + 1:
                    identity(
                        "returned input endpoint/result",
                        first.get("after") or {},
                        delivered.get("result") or {},
                        token,
                    )
                eq(
                    "returned physical input map",
                    "map-57",
                    first.get("before", {}).get("map", absent),
                    token,
                )
                precedes("return before field input", state, first.get("before") or {}, token)
        compositions.append(
            dict(
                token=token,
                kind="terminal-no-new-visual-effect",
                completed="Inferred",
                delay="Unknown",
                directCompletedSnapshot=False,
            )
        )

    return compositions
