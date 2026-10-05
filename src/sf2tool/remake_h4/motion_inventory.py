"""Ordered source producers and ordinary-warp fade helper recovery."""

from sf2tool.remake_h4.motion_programs import target


def motion_inventory(ordered, warp_records, record_by_event, instruction, common, _bounded_list):
    producers = _bounded_list()
    warp_started = transferred = -1
    last_location = None
    for e in ordered:
        seq = e["Sequence"]
        if e["Kind"] == "warp-started":
            warp_started = seq
        if e["Kind"] == "map-transferred":
            transferred = seq
        r = warp_records[record_by_event[seq]]
        s = r.get("state", {})
        loc = e.get("Program")
        if e["Kind"] == "full-fade-started":
            loc = s.get("cursor")
            # The outcome publishes before GameRoot installs the returning view.
            # Recover this producer from preceding logical source instructions,
            # independently of whether any projection channel survived.
            if "cursor" not in s and last_location:
                candidate = dict(
                    Program=last_location["Program"], Instruction=last_location["Instruction"] + 1
                )
                next_ins = instruction(candidate)
                if (
                    next_ins
                    and next_ins.get("op") == "present"
                    and next_ins.get("kind") in ("FadeIn", "FadeOut")
                ):
                    loc = candidate
            ins = instruction(loc)
            helper = "cursor" in s and loc is None
            if helper:
                ins = dict(
                    op="present",
                    kind="FadeOut" if warp_started > transferred else "FadeIn",
                    resource="black",
                    entity=None,
                )
            producers.append((e, loc, ins, "full-fade", helper))
        elif e["Kind"] in ("program-instruction", "nod-started"):
            last_location = loc
            ins = instruction(loc)
            if ins is not None and target(ins):
                producers.append(
                    (e, loc, ins, "nod" if e["Kind"] == "nod-started" else ins["op"], False)
                )
            elif e.get("Detail") == "StartEntityMotion" or e["Kind"] == "nod-started":
                producers.append((e, loc, ins, "motion", False))
    common("logical producer inventory present", True if producers else None)
    return producers
