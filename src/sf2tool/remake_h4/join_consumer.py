"""Ordered actual JOIN sample/input consumer with original partial finalization."""

from .join_audio import join_audio
from .join_caller import join_caller
from .join_generation import join_generation


def join_consumer(actual, world_path, result, finalize, set_plain, read, _bounded_list):
    try:
        labels = (
            "music-plain-input",
            "music-plain-poll",
            "music-plain-accepted",
            "join-field-return",
        )
        selected = [
            [(i, s["state"]) for i, s in enumerate(actual["samples"]) if s["label"] == label]
            for label in labels
        ]
        if any(not group for group in selected):
            return finalize()
        if any(len(group) != 1 for group in selected):
            result.update(plain=False, audio=False, caller=False)
            return finalize()
        (pi, plain), (wi, polled), (ai, acked), (ri, ready) = (group[0] for group in selected)
        records = actual["warpRecords"]
        observations = _bounded_list(
            ((i, o) for i, r in enumerate(records) for o in r["result"]["observations"])
        )
        late = _bounded_list(
            (
                (i, row["state"])
                for i, row in enumerate(actual["samples"])
                if row["label"] == "music-logical-end"
            )
        )
        if len(late) > 1:
            result.update(plain=False, audio=False, caller=False)
            return finalize()
        held = _bounded_list(
            (
                (i, row["state"])
                for i, row in enumerate(records)
                if row.get("state", {}).get("wait") == "MusicWait"
                and row["state"]["revision"] < plain["revision"]
            )
        )
        if not held:
            return finalize()
        li, logical = late[0] if late else (None, held[0][1])
        selected = join_generation(
            actual, late, logical, plain, world_path, observations, result, read, _bounded_list
        )
        if selected is None:
            return finalize()
        generation, early_world, source_operation, request, install = selected

        def event(kind, detail=None):
            lower = (
                acked["revision"]
                if kind in ("simulation-tick", "zone-finished")
                else generation - 1
                if kind == "music-actual-completed"
                else logical["revision"]
            )
            found = [
                (i, o)
                for i, o in observations
                if o["Kind"] == kind
                and (detail is None or o["Detail"] == detail)
                and lower < o["Sequence"] <= ready["revision"]
            ]
            if not found:
                raise KeyError(kind)
            return found

        released = event("music-wait-returned", "MUSIC_JOIN")
        if not late and any(state["revision"] >= released[0][1]["Sequence"] for _, state in held):
            result["audio"] = False
        completed = event("music-actual-completed", "MUSIC_JOIN")
        previous = event("presentation-completed", "PreviousMusic")
        acknowledged = event("presentation-acknowledged")
        pressed = _bounded_list(r for r in actual["inputRecords"] if r["pressed"])
        early = [
            r
            for r in pressed
            if r["before"]["revision"] == logical["revision"]
            and r["before"]["token"] == logical["token"]
        ]
        wait = [
            r
            for r in pressed
            if r["before"]["revision"] == plain["revision"] and r["action"] == "wait"
        ]
        confirm = [
            r
            for r in pressed
            if r["before"]["revision"] == polled["revision"] and r["action"] == "confirm"
        ]
        if (late and not early) or not wait or not confirm:
            return finalize()
        wait, confirm = wait[0], confirm[0]

        def delivered(record, kind):
            return any(
                o["Kind"] == kind
                for r in records[record["resultStart"] : record["resultEnd"]]
                for o in r["result"]["observations"]
            )

        plain_value = (
            (
                li < pi < wi < ai < ri
                if late
                else logical["revision"] < plain["revision"] and pi < wi < ai < ri
            )
            and len(completed) == len(released) == len(previous) == len(acknowledged) == 1
            and completed[0][1]["Sequence"]
            < released[0][1]["Sequence"]
            < previous[0][1]["Sequence"]
            < plain["revision"]
            < acknowledged[0][1]["Sequence"]
            < acked["revision"]
            and plain["wait"] == polled["wait"] == "DialogueWait"
            and plain["token"] == polled["token"] == confirm["before"]["token"]
            and plain["cursor"] == polled["cursor"] == confirm["before"]["cursor"]
            and acked["wait"] == confirm["after"]["wait"] == "TextCloseWait"
            and all(
                side[key] == state[key]
                for side, state in (
                    (wait["before"], plain),
                    (wait["after"], polled),
                    (confirm["before"], polled),
                    (confirm["after"], acked),
                )
                for key in ("revision", "simulationTick", "mainSeed", "token", "cursor")
            )
            and (not late or {r["action"] for r in early} == {"wait", "confirm"})
            and all(r["resultStart"] == r["resultEnd"] and r["before"] == r["after"] for r in early)
            and polled["simulationTick"] == plain["simulationTick"] + 1
            and acked["simulationTick"] == polled["simulationTick"]
            and plain["mainSeed"] == polled["mainSeed"] == acked["mainSeed"]
            and delivered(wait, "gameplay-wait")
            and delivered(confirm, "presentation-acknowledged")
            and all(
                s["w1"] is None and s["fieldText"] is None and 603 not in s["flags"]
                for s in (plain, polled, acked)
            )
        )
        set_plain(plain_value)
        audio = join_audio(
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
        )
        if audio is None:
            return finalize()
        helper_token, interval = audio
        join_caller(
            actual,
            world_path,
            plain,
            acked,
            ready,
            observations,
            event,
            ai,
            ri,
            li,
            pi,
            wi,
            late,
            held,
            generation,
            helper_token,
            interval,
            early,
            wait,
            confirm,
            completed,
            released,
            previous,
            result,
            read,
            _bounded_list,
        )
    except (KeyError, IndexError, StopIteration):
        pass
    return finalize()
