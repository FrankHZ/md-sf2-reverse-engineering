"""Check result identities, declared events, typed state and partial predecessors."""

from sf2tool.remake_h4.field_source import _FIELD_MOTION, _FIELD_SERVICES
from sf2tool.remake_h4.field_values import _field_join


def compare_state(
    before, after, facts, events, prior, initial, initial_npc, programs, session, index, check
):
    allowed_kinds = _FIELD_SERVICES | {
        "program-instruction",
        "entity-sprite-ready",
        "portrait-window-moving",
        "portrait-service-registered",
        "portrait-closed",
        "text-work-advanced",
        "text-revealed",
        "rng-text-w1",
        "text-seed-copy",
        "text-w1-input",
        "text-w1-accepted",
        "rng-portrait-blink",
        "rng-portrait-mouth",
    }
    check(
        "complete declared event/writer family",
        all(e["Kind"] in allowed_kinds for e in events),
        index,
    )
    instruction_kinds = {
        "sprite": "SetEntitySprite",
        "open-portrait": "OpenPortrait",
        "close-portrait": "ClosePortrait",
        "text-cursor": "SetTextCursor",
        "show-text": "ShowText",
        "close-text": "CloseText",
        "end": "EndProgram",
    }
    for event in events:
        if event["Kind"] == "program-instruction":
            loc = event["Program"]
            check(
                "legal instruction cursor",
                isinstance(loc["Instruction"], int)
                and not isinstance(loc["Instruction"], bool)
                and loc["Instruction"] >= 0,
                index,
            )
            declared = programs[loc["Program"]]["instructions"][int(loc["Instruction"])]
            check(
                "declared program operation",
                event["Detail"] == instruction_kinds.get(declared["op"]),
                index,
            )
    for state in (before, after):
        if not state:
            continue
        operands = [
            state[k] for k in ("revision", "observationSequence", "simulationTick", "mainSeed")
        ]
        operands += [
            e[k] for e in state["entities"] for k in (*_FIELD_MOTION, "slot", "actionCursor")
        ]
        operands += (
            [state["portraitWork"][k] for k in ("Blink", "Mouth")] if state["portraitWork"] else []
        )
        check(
            "integral service operands",
            all(
                isinstance(v, (int, float)) and not isinstance(v, bool) and v == int(v)
                for v in operands
            ),
            index,
        )
        check(
            "typed live gates",
            None
            if any("waitingForMotion" not in e for e in state["entities"])
            else isinstance(state["typewriting"], bool)
            and all(
                isinstance(e["waitingForMotion"], bool) and isinstance(e["Visible"], bool)
                for e in state["entities"]
            ),
            index,
        )
        check(
            "portrait state admission",
            (state["portraitWork"] is None and state["portraitWindow"] == "ClosedPortraitWindow")
            or (
                isinstance(state["portraitWork"], dict)
                and state["portraitWindow"] == "OpenPortraitWindow"
                and all(
                    isinstance(state["portraitWork"][k], bool)
                    for k in ("Registered", "Closing", "EyesClosed", "MouthOpen")
                )
            ),
            index,
        )
        check(
            "motion gate agrees with coordinates",
            all(
                isinstance(e["moving"], bool)
                and e["moving"] == (e["x"] != e["targetX"] or e["y"] != e["targetY"])
                for e in state["entities"]
            ),
            index,
        )
        check(
            "unique ordered actor slots",
            [e["slot"] for e in state["entities"]]
            == sorted({e["slot"] for e in initial["entities"]})
            and {e["id"] for e in state["entities"]} == {e["id"] for e in initial["entities"]},
            index,
        )
    for entity in after["entities"]:
        if entity["id"] in initial_npc:
            check(
                "unchanged bound action program:" + entity["id"],
                entity["actionProgram"] == initial_npc[entity["id"]]["actionProgram"],
                index,
            )
    check(
        "result identity",
        all(facts[k] == after[k] for k in ("sessionId", "revision", "observationSequence", "mode"))
        and after["sessionId"] == session,
        index,
    )
    check(
        "accepted and error-free",
        facts["failure"] is None and after["failure"] is None,
        index,
    )
    if prior is not None:
        check(
            "result predecessor",
            _field_join(
                {
                    k: before[k]
                    for k in (
                        "sessionId",
                        "revision",
                        "observationSequence",
                        "simulationTick",
                        "mainSeed",
                        "entities",
                        "portraitWork",
                        "typewriting",
                        "cursor",
                    )
                    if k in before
                },
                {
                    k: prior[k]
                    for k in (
                        "sessionId",
                        "revision",
                        "observationSequence",
                        "simulationTick",
                        "mainSeed",
                        "entities",
                        "portraitWork",
                        "typewriting",
                        "cursor",
                    )
                    if k in prior
                },
            ),
            index,
        )
