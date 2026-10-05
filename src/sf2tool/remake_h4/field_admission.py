"""Admit a case receipt, absolute clocks, process and declared initial state."""

from sf2tool.remake_h4.field_source import _field_action_program
from sf2tool.remake_h4.field_values import _field_equal


def admit_case(case, profile, records, process, check):
    def clocks(name, value, keys, row):
        value = value if isinstance(value, dict) else {}
        for key in keys:
            clock = value.get(key)
            check(
                name + ":" + key,
                None
                if key not in value
                else isinstance(clock, (int, float))
                and not isinstance(clock, bool)
                and clock >= 0
                and clock % 1 == 0,
                row,
            )

    receipts = [i for i, r in enumerate(records) if r.get("kind") == "receipt"]
    check("single terminal receipt boundary", receipts == [len(records) - 1] if receipts else None)
    for index in receipts:
        receipt = records[index]
        check(
            "terminal receipt case identity",
            receipt["case"] == case["id"] if "case" in receipt else None,
            index,
        )
    # Validate absolute clock domains before joins or missing downstream operands can
    # short-circuit a row. Motion and portrait counters deliberately remain signed.
    state_clocks = ("revision", "observationSequence", "simulationTick")
    for index, row in enumerate(records):
        if row.get("kind") == "result":
            if row.get("before"):
                clocks("before clock domain", row["before"], state_clocks, index)
            clocks("after clock domain", row.get("after"), state_clocks, index)
            facts = row.get("facts") or {}
            clocks("result clock domain", facts, state_clocks[:2], index)
            for event in facts.get("observations") or []:
                clocks("event clock domain", event, ("Revision", "Sequence"), index)
        elif row.get("kind") == "input":
            clocks("input clock domain", row.get("before"), state_clocks, index)
        elif row.get("kind") == "draw":
            state = row.get("state") or {}
            clocks("draw clock domain", state, state_clocks, index)
            if state.get("portraitResourceProjection") is not None:
                clocks(
                    "resource clock domain",
                    state["portraitResourceProjection"],
                    state_clocks,
                    index,
                )

    check(
        "native exit and bounded process",
        process["exit"] == 0
        and process["stopped"] is None
        and process["inputUnchanged"] is True
        and 0 < process["seconds"] <= 45
        and 0 < process["peakBytes"] <= 1536 * 1024 * 1024,
    )
    check("native stderr", not case.get("errors"))
    check("native error channel available", True if "errors" in case else None)
    check(
        "completed capture receipt", records[-1]["kind"] == "receipt" and records[-1]["exit"] == 0
    )
    results = [(i, x) for i, x in enumerate(records) if x["kind"] == "result"]
    initial = results[0][1]["after"]
    session = initial["sessionId"]
    check("selected session identity", session == case.get("sessionId"))
    check("capture row completeness", True if records[-1].get("rows") == len(records) - 1 else None)
    check("bounded capture rows", len(records) <= 769)
    check("bounded driver elapsed", 0 < records[-1]["elapsedUsec"] <= 30_000_000)
    terminal = results[-1][1]["after"]
    check(
        "ordinary completed field endpoint",
        terminal["portraitWindow"] == "ClosedPortraitWindow"
        and terminal["wait"] is None
        and terminal["cursor"] is None
        and terminal["canWaitAtInput"] is True,
    )
    expected_entities = profile["world"]["maps"][0]["entities"]
    programs = {p["id"]: p for p in profile["world"]["programs"]}
    initial_npc = {e["id"]: e for e in initial["entities"] if e["id"] != "traveler"}
    check("declared actor census", set(initial_npc) == {e["id"] for e in expected_entities})
    check("declared main seed", initial["mainSeed"] == profile["battle"]["start"]["mainSeed"])
    for e in expected_entities:
        check(
            "declared structural program:" + e["id"],
            _field_equal(
                _field_action_program(e["actions"]), initial_npc[e["id"]]["actionProgram"]
            ),
        )
        start = dict(
            x=e["position"]["x"] * 384,
            y=e["position"]["y"] * 384,
            targetX=e["position"]["x"] * 384,
            targetY=e["position"]["y"] * 384,
            velocityX=0,
            velocityY=0,
            travelX=0,
            travelY=0,
            speedX=e["speed"],
            speedY=e["speed"],
            accelerationX=0,
            accelerationY=0,
            flagsA=32 if e["obstruction"] else 0,
            flagsB=64,
            facing=e["facing"],
            layer=0,
            animationCounter=0,
            waitTimer=0,
            actionCursor=0,
            waitingForMotion=False,
        )
        for phase in profile["start"].get("entityPhases", []):
            if phase["entity"] != e["id"]:
                continue
            mapping = dict(
                xDestination="targetX",
                yDestination="targetY",
                xVelocity="velocityX",
                yVelocity="velocityY",
                xTravel="travelX",
                yTravel="travelY",
                xSpeed="speedX",
                ySpeed="speedY",
                xAcceleration="accelerationX",
                yAcceleration="accelerationY",
            )
            start.update({mapping.get(k, k): v for k, v in phase["motion"].items()})
            start.update(
                actionCursor=phase["actionCursor"], waitingForMotion=phase["waitingForMotion"]
            )
        check(
            "declared initial NPC motion/cursor:" + e["id"],
            all(initial_npc[e["id"]][k] == v for k, v in start.items()),
        )
    return results, initial, session, programs, initial_npc
