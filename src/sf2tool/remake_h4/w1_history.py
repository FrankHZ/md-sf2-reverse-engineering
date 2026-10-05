"""Admit NPC source provenance, complete producer history and walking installations."""

from sf2tool.remake_h4.w1_checks import merge
from sf2tool.remake_h4.w1_selection import rows_by_index
from sf2tool.remake_h4_reference import ROM, UPSTREAM


def admit_history(actual, supplement, context, session, source, checks):
    check, match, absent = checks.check, checks.match, checks.absent
    programs, compiler, layout = source.programs, source.compiler, source.layout
    # The bounded history contains all motion/replacement producers through the
    # second NPC consumer. Independent indices keep omission distinct from no write.
    history = actual.get("w1NpcHistory", supplement.get("w1NpcHistory")) or {}
    history_rows = rows_by_index(history.get("warpRecords", []), "NPC history", checks)
    history_indices = set(context.get("npcHistoryIndices", []))
    check(
        "NPC history coverage",
        merge(
            [
                not (set(history_rows) - history_indices),
                True if history_indices and history_indices <= set(history_rows) else None,
            ]
        ),
    )
    world = actual.get("w1NpcWorld", supplement.get("w1NpcWorld")) or {}
    check(
        "historical world provenance",
        match(dict(commit=UPSTREAM, romSha256=ROM), world.get("provenance", absent)),
    )
    map3 = world.get("map3") or {}
    check(
        "historical map source layout",
        None
        if layout is None or "layout" not in map3
        else layout == [v for row in map3["layout"] for v in row],
    )
    check("historical default setup", match(dict(id="map-3", setup=dict(default="ms-map3")), map3))
    historical_programs = world.get("programs", [])
    for p in historical_programs:
        check(
            "historical program equals independent source",
            None if p.get("id") not in programs else p == programs[p["id"]],
        )
    check(
        "historical program coverage",
        merge(
            [
                not (
                    {p.get("id") for p in historical_programs} - set(context.get("npcPrograms", []))
                ),
                len({p.get("id") for p in historical_programs}) == len(historical_programs),
                True
                if historical_programs
                and {p.get("id") for p in historical_programs}
                == set(context.get("npcPrograms", []))
                else None,
            ]
        ),
    )
    admissions = history.get("samples", [])
    check(
        "map admission before NPC history",
        None
        if not admissions
        else match(
            dict(sessionId=session, map="map-3", flags=[0, 32]), admissions[0].get("state", absent)
        ),
    )
    for admission in admissions:
        flags = (admission.get("state") or {}).get("flags")
        check(
            "NPC default setup survives observed flags",
            None if flags is None else not ({506, 543, 609} & set(flags)),
        )
    installations = {"entity-130": compiler.walking(20, 13, 3) if compiler else None}
    npc_history_events = []
    for ri, row in sorted(history_rows.items()):
        check(
            "NPC history session",
            match(dict(sessionId=session, map="map-3"), row.get("state", absent)),
        )
        check(
            "NPC history result identity",
            match(
                dict(
                    sessionId=session,
                    revision=(row.get("state") or {}).get("revision", absent),
                    failure=None,
                ),
                row.get("result", absent),
            ),
        )
        history_events = (row.get("result") or {}).get("observations", [])
        for key, terminal in (("Sequence", "observationSequence"), ("Revision", "revision")):
            observed_values = [event.get(key) for event in history_events]
            available = [value for value in observed_values if value is not None]
            limit = (row.get("result") or {}).get(terminal)
            check(
                "NPC producer " + key + " belongs to Submit",
                merge(
                    [
                        True if len(available) == len(observed_values) else None,
                        available == sorted(set(available)),
                        None if limit is None else all(0 < value <= limit for value in available),
                    ]
                ),
            )
        for event in history_events:
            cur = event.get("Program") or {}
            instructions = programs.get(cur.get("Program"), {}).get("instructions", [])
            n = cur.get("Instruction")
            if event.get("Kind") == "program-instruction" and event.get("Detail") in (
                "StartEntityMotion",
                "SetEntityPosition",
                "HideEntity",
                "FollowEntity",
                "LoadSceneMap",
                "WriteFlag",
                "SetEntityFacing",
            ):
                op = (
                    instructions[int(n)]
                    if isinstance(n, (int, float)) and 0 <= n < len(instructions)
                    else {}
                )
                check("source-owned NPC history producer", True if op else None)
                check(
                    "NPC history producer kind",
                    match(
                        {
                            "motion": "StartEntityMotion",
                            "position": "SetEntityPosition",
                            "hide": "HideEntity",
                            "follow": "FollowEntity",
                            "load-map": "LoadSceneMap",
                            "set-flag": "WriteFlag",
                            "face": "SetEntityFacing",
                        }.get(op.get("op"), absent),
                        event.get("Detail", absent),
                    ),
                )
                if op.get("op") == "set-flag" and op.get("flag") in (506, 543, 609):
                    check(
                        "history does not select another map setup",
                        match(False, op.get("value", absent)),
                    )
                npc_history_events.append((ri, op))
    merchant = next((e for e in map3.get("entities", []) if e.get("id") == "entity-130"), {})
    check(
        "source merchant walking admission",
        match(
            dict(
                actions=installations["entity-130"]
                if installations["entity-130"] is not None
                else absent
            ),
            merchant,
        ),
    )

    return installations, npc_history_events
