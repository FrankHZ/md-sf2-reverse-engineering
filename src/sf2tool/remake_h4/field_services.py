"""Compare ordered service counts, admitted entry batches and source net effects."""

from sf2tool.remake_h4.field_source import (
    _FIELD_COUNTERS,
    _FIELD_MOTION,
    _FIELD_SERVICES,
    _field_action_program,
    _field_npc_tick,
    _field_portrait,
    _field_rng,
)
from sf2tool.remake_h4.field_values import _field_equal


def compare_services(before, after, events, programs, profile, coverage, index, check):
    first_work, last_work = before["portraitWork"], after["portraitWork"]
    check(
        "complete result event span",
        after["observationSequence"] - before["observationSequence"] == len(events)
        and after["revision"] - before["revision"] == len(events),
        index,
    )
    check(
        "ordered event clocks",
        all(
            before["observationSequence"] < e["Sequence"] <= after["observationSequence"]
            and before["revision"] < e["Revision"] <= after["revision"]
            for e in events
        )
        and all(
            a["Sequence"] < b["Sequence"] and a["Revision"] < b["Revision"]
            for a, b in zip(events, events[1:], strict=False)
        ),
        index,
    )
    services = [e for e in events if e["Kind"] in _FIELD_SERVICES]
    n = int(after["simulationTick"] - before["simulationTick"])
    check("complete service count", n == len(services), index)
    if not n:
        check("nonservice seed unchanged", before["mainSeed"] == after["mainSeed"], index)
        if first_work is not None and last_work is not None:
            check(
                "nonservice counters unchanged",
                all(_field_equal(first_work[k], last_work[k]) for k in _FIELD_COUNTERS),
                index,
            )
        check(
            "nonservice no RNG",
            not any(e["Kind"].startswith("rng-") for e in events),
            index,
        )
        return
    coverage["service"] += n
    seed = int(before["mainSeed"])
    poll = [e for e in events if e["Kind"] == "rng-text-w1"]
    if poll:
        updated, value = _field_rng(seed, 256)
        check(
            "source W1 prefix",
            len(poll) == 1
            and all(
                poll[0][k] == v
                for k, v in dict(
                    Before=seed, After=updated, RandomRange=256, RandomValue=value
                ).items()
            )
            and events.index(poll[0]) < events.index(services[0]),
            index,
        )
        seed = updated
        check("accepted current live seed copy", after["randomSeedCopy"] == value, index)
    gate = before["entitiesRunning"]
    if before["cursor"] is not None:
        caller = programs[before["cursor"]["Program"]]
        check(
            "declared caller service gate",
            gate is caller.get("entitiesRunning", True),
            index,
        )
    if gate is None:
        check(
            "ordinary field caller gate",
            before["cursor"] is None and before["wait"] is None and before["canWaitAtInput"],
            index,
        )
        gate = True
    entities = before["entities"]
    work = before["portraitWork"]
    expected_draws = []
    if n > 1:
        allowed = work is not None and work["Registered"] is False and work["Closing"] is False
        if not allowed:
            check("intermediate registered/text operands unavailable", None, index)
            return
        check("unregistered entry batch admission", True, index)
        if gate:
            random_actors = [e for e in entities if e["id"] != "traveler"]
            check("bounded entry NPC census", len(random_actors) == 1, index)
            for actor in random_actors:
                program = actor["actionProgram"]
                expected_program = _field_action_program(
                    [
                        dict(
                            op="random-walk",
                            x=actor["x"] / 384,
                            y=actor["y"] / 384,
                            radius=0,
                        ),
                        dict(op="jump", instruction=0),
                    ]
                )
                check(
                    "stationary radius-zero entry program",
                    _field_equal(expected_program, program)
                    and actor["actionCursor"] in (0, 1)
                    and actor["waitingForMotion"] is False
                    and actor["x"] == actor["targetX"]
                    and actor["y"] == actor["targetY"]
                    and actor["travelX"]
                    == actor["travelY"]
                    == actor["velocityX"]
                    == actor["velocityY"]
                    == 0,
                    index,
                )
        check(
            "exact entry continuation",
            [e["Kind"] for e in events]
            == ["portrait-window-service"] * n
            + ["portrait-service-registered", "program-instruction", "program-instruction"]
            and [e["Detail"] for e in events[-2:]] == ["SetTextCursor", "ShowText"],
            index,
        )
        cursor = before["cursor"]
        instructions = programs[cursor["Program"]]["instructions"]
        start = cursor["Instruction"]
        check(
            "source-declared entry instructions",
            instructions[start] == dict(op="open-portrait", entity="ferryman", flags=0)
            and instructions[start + 1] == dict(op="text-cursor", text=100)
            and instructions[start + 2]
            == dict(op="show-text", mode="single", speaker="ferryman", explicitWindows=True),
            index,
        )
        check(
            "exact continuation locations",
            events[-2]["Program"] == dict(Program=cursor["Program"], Instruction=start + 1)
            and events[-1]["Program"] == dict(Program=cursor["Program"], Instruction=start + 2)
            and after["cursor"] == events[-1]["Program"]
            and after["wait"] == "FieldTextWait"
            and after["fieldText"]["Phase"] == 0,
            index,
        )
        check(
            "entry exit gate",
            after["portraitWork"]["Registered"] is True
            and after["portraitWork"]["Y"] == after["portraitWork"]["DestinationY"]
            and after["entitiesRunning"] == gate,
            index,
        )
        coverage["unregistered-entry-batch"] += 1
    for _ in range(n):
        if gate:
            entities, seed, paths = _field_npc_tick(
                entities, seed, profile["world"]["maps"][0]["layout"]
            )
            coverage.update(paths)
        else:
            coverage["entities-disabled"] += 1
        lower = (
            None
            if work is None
            else dict(
                registered=work["Registered"],
                **{v: work[k] for k, v in _FIELD_COUNTERS.items()},
            )
        )
        expected = _field_portrait(seed, lower, before["typewriting"])
        seed = expected["seed"]
        expected_draws.extend(expected["draws"])
        if work is not None:
            for k, v in _FIELD_COUNTERS.items():
                work = {**work, k: expected["work"][v]}
            coverage[
                "portrait-unregistered"
                if not work["Registered"]
                else "portrait-typing"
                if before["typewriting"]
                else "portrait-not-typing"
            ] += 1
    check("source main seed effect", after["mainSeed"] == seed, index)
    got_draws = [
        dict(
            kind=e["Kind"].removeprefix("rng-portrait-"),
            before=e["Before"],
            after=e["After"],
            range=e["RandomRange"],
            value=e["RandomValue"],
        )
        for e in events
        if e["Kind"].startswith("rng-portrait-")
    ]
    check("ordered source portrait draws", _field_equal(expected_draws, got_draws), index)
    coverage.update("draw-" + e["kind"] for e in expected_draws)
    if work is not None and after["portraitWork"] is not None:
        check(
            "source portrait counters/effects",
            all(_field_equal(work[k], after["portraitWork"][k]) for k in _FIELD_COUNTERS),
            index,
        )
    expected_by_id = {e["id"]: e for e in entities}
    for e in after["entities"]:
        if e["id"] == "traveler" and gate:
            continue
        keys = tuple(_FIELD_MOTION) + (
            "actionCursor",
            "waitingForMotion",
            "actionProgram",
            "slot",
            "Visible",
            "sprite",
        )
        check(
            "source NPC gate/motion/script:" + e["id"],
            all(_field_equal(expected_by_id[e["id"]][k], e[k]) for k in keys),
            index,
        )
