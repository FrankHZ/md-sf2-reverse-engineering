"""Actual JOIN generation and source request/helper installation."""


def join_generation(
    actual, late, logical, plain, world_path, observations, result, read, _bounded_list
):
    early_world = source_operation = request = install = None
    initial_starts = _bounded_list(
        row["receipt"]
        for row in actual["audioReceipts"]
        if row["receipt"]["Cue"] == "MUSIC_JOIN"
        and row["receipt"]["Operation"] == "started"
        and row["receipt"]["Revision"] < logical["revision"]
    )
    if late:
        generation = logical["music"]["Generation"]
    else:
        if logical.get("sessionId") != plain.get("sessionId"):
            result["audio"] = False
        if not world_path.is_file():
            return None
        early_world = read(world_path)["world"]
        early_programs = {p["id"]: p for p in early_world["programs"]}

        def source_operation(event):
            location = event.get("Program")
            if location is None:
                return None
            return early_programs[location["Program"]]["instructions"][int(location["Instruction"])]

        helper_sources = [
            o
            for _, o in observations
            if o["Kind"] == "program-instruction"
            and o["Sequence"] <= logical["revision"]
            and (source_operation(o) or {}).get("kind") == "SoundWait"
        ]
        if not helper_sources:
            return None
        install = helper_sources[-1]
        request_sources = [
            o
            for _, o in observations
            if o["Kind"] == "program-instruction"
            and o["Sequence"] < install["Sequence"]
            and (source_operation(o) or {}).get("kind") == "Sound"
            and (source_operation(o) or {}).get("resource") == "MUSIC_JOIN"
        ]
        if not request_sources:
            return None
        request = request_sources[-1]
        generation = request["Sequence"]
        if (
            logical.get("token") != install["Sequence"]
            or logical.get("cursor") != install["Program"]
            or any(row["Revision"] != generation for row in initial_starts)
        ):
            result["audio"] = False

    return generation, early_world, source_operation, request, install
