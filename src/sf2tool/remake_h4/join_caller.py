"""Selected JOIN source tail, input return and dependent field readiness."""


def join_caller(
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
):
    if not world_path.is_file():
        return None
    world = read(world_path)["world"]
    program = next(p for p in world["programs"] if p["id"] == "cs-51614")
    instructions = program["instructions"]
    begin = plain["cursor"]["Instruction"]
    tail = instructions[int(begin) :]
    actual_tail = [
        (i, o)
        for i, o in observations
        if o["Kind"] == "program-instruction"
        and o["Program"]["Program"] == program["id"]
        and acked["revision"] <= o["Sequence"] <= ready["revision"]
    ]
    ticks = event("simulation-tick")
    flag = [
        (i, o)
        for i, o in observations
        if o["Kind"] == "program-instruction"
        and o["Detail"] == "WriteFlag"
        and acked["revision"] < o["Sequence"] <= ready["revision"]
    ]
    zone_finished = event("zone-finished")
    arrivals = _bounded_list(
        s["state"]
        for s in actual["samples"][ai + 1 : ri]
        if s["state"]["wait"] == "ZoneArrivalWait"
    )
    if not arrivals:
        return None
    zone = next(p for p in world["programs"] if p["id"] == "map3-zoneevent8")
    entities = {e["id"]: e for e in ready["entities"]}
    effects = all(
        entities[i["entity"]]["follower"]
        == dict(
            LeaderSlot=int(i["leader"].removeprefix("entity-")),
            OffsetX=i["x"],
            OffsetY=i["y"],
        )
        for i in tail[3:5]
    ) and all(
        entities[i["entity"]]["x"] == i["position"]["x"] * 384
        and entities[i["entity"]]["y"] == i["position"]["y"] * 384
        and entities[i["entity"]]["facing"] == i["facing"]
        for i in tail[5:7]
    )
    result["caller"] = (
        effects
        and program["source"] == "disasm/data/maps/entries/map03/mapsetups/scripts_1.asm:cs_51614"
        and instructions[int(begin) - 4]["text"] == 447
        and instructions[int(begin) - 3]["waitForAcknowledgement"] is False
        and instructions[int(begin) - 2]["kind"] == "SoundWait"
        and instructions[int(begin) - 1]["kind"] == "PreviousMusic"
        and [i["op"] for i in tail]
        == [
            "wait-text-input",
            "close-text",
            "wait-ticks",
            "follow",
            "follow",
            "position",
            "position",
            "jump",
        ]
        and tail[2]["ticks"] == 10
        and [o["Program"]["Instruction"] for _, o in actual_tail]
        == _bounded_list(range(int(begin) + 1, len(instructions)))
        and [o["Detail"] for _, o in actual_tail]
        == [
            "CloseText",
            "WaitProgramTicks",
            "FollowEntity",
            "FollowEntity",
            "SetEntityPosition",
            "SetEntityPosition",
            "JumpProgram",
        ]
        and len(ticks) == tail[2]["ticks"]
        and actual_tail[1][1]["Sequence"]
        < ticks[0][1]["Sequence"]
        <= ticks[-1][1]["Sequence"]
        < actual_tail[2][1]["Sequence"]
        and len(flag) == len(zone_finished) == 1
        and flag[0][1]["Program"]["Program"] == zone["id"]
        and zone["instructions"][int(flag[0][1]["Program"]["Instruction"])]
        == dict(op="set-flag", flag=603, value=True)
        and actual_tail[-1][1]["Sequence"]
        < flag[0][1]["Sequence"]
        < zone_finished[0][1]["Sequence"]
        == ready["revision"]
        and 603 in ready["flags"]
        and ready["wait"] is None
        and ready["cursor"] is None
        and ready["canWaitAtInput"]
        and all(603 in s["flags"] and not s["canWaitAtInput"] for s in arrivals)
        and {1, 2}.issubset(ready["partyLists"]["Joined"])
    )
    result["anchors"]["actual"] = dict(
        samples=[li, pi, wi, ai, ri],
        completionOrder="late" if late else "early",
        heldHelperRecords=[i for i, _ in held],
        generation=generation,
        helperToken=helper_token,
        receiptSequences=[r["Sequence"] for r in interval],
        inputOrdinals=[r["ordinal"] for r in early] + [wait["ordinal"], confirm["ordinal"]],
        completionRecords=[completed[0][0], released[0][0], previous[0][0]],
        callerRecords=[i for i, _ in actual_tail] + [flag[0][0], zone_finished[0][0]],
    )
