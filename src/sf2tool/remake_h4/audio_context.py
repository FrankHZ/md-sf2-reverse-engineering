"""Select reached audio metadata and script operands without PCM."""


def _audio_context(document, actual):
    world = document["world"]
    programs = [
        dict(id=p["id"], source=p.get("source"), instruction=index, operation=op)
        for p in world["programs"]
        for index, op in enumerate(p["instructions"])
        if op.get("op") == "present"
        and op.get("kind") in ("Sound", "SoundWait", "PreviousMusic", "SoundFade")
    ]
    locations = {(p["id"], p["instruction"]) for p in programs}
    return dict(
        provenance=document["provenance"],
        sessionId=actual["audioReceipts"][0]["poll"]["sessionId"]
        if actual.get("audioReceipts")
        else None,
        audio=[
            {k: v for k, v in a.items() if k != "pcm16"} for a in world["presentation"]["audio"]
        ],
        programs=programs,
        events=[
            e
            for r in actual.get("warpRecords", [])
            for e in r["result"].get("observations", [])
            if e["Kind"] == "program-instruction"
            and e.get("Program")
            and (e["Program"]["Program"], e["Program"]["Instruction"]) in locations
        ],
    )
