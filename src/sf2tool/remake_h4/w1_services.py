"""Check the W1 poll preamble and ordered portrait RNG before the accepting read."""

from sf2tool.remake_h4.w1_checks import merge
from sf2tool.remake_h4.w1_source import random


def compare_preamble(submit, before, accepting, ordinal, checks):
    check, match, one, absent = checks.check, checks.match, checks.one, checks.absent
    events = submit.get("observations") or []
    kinds = ["rng-text-w1", "text-seed-copy", "text-w1-wait", "text-w1-input"] + (
        ["text-w1-accepted"] if accepting else []
    )
    found, positions = {}, []
    for kind in kinds:
        entries = [(i, e) for i, e in enumerate(events) if e.get("Kind") == kind]
        found[kind] = one(kind, [e for _, e in entries], ordinal)
        positions.extend(i for i, _ in entries)
    check("mandatory draw/copy/wait/read order", positions == sorted(set(positions)), ordinal)
    check(
        "preamble before service",
        None
        if any(not found[k] for k in kinds[:3])
        else [e.get("Kind") for e in events[:3]] == kinds[:3],
        ordinal,
    )
    check(
        "input mask",
        match("accept" if accepting else "none", found["text-w1-input"].get("Detail", absent)),
        ordinal,
    )
    check(
        "neutral remains suspended",
        accepting
        or not any(e.get("Kind") in ("program-instruction", "text-w1-accepted") for e in events),
        ordinal,
    )
    seed, byte = random(before.get("mainSeed"), 256)
    check(
        "independent range256 draw",
        match(
            dict(
                Before=before.get("mainSeed", absent),
                After=seed if seed is not None else absent,
                RandomRange=256,
                RandomValue=byte if byte is not None else absent,
            ),
            found["rng-text-w1"],
        ),
        ordinal,
    )
    check(
        "copy before all service RNG",
        match(byte if byte is not None else absent, found["text-seed-copy"].get("After", absent)),
        ordinal,
    )
    return events, seed, byte


def compare_portrait(ready, after, events, seed, ordinal, checks):
    check, match, one, absent = checks.check, checks.match, checks.one, checks.absent
    portrait = ready.get("portraitWork")
    open_portrait = ready.get("portraitWindow") == "OpenPortraitWindow"
    check(
        "portrait mouth-control gate",
        match(dict(MouthControl=0), ready.get("textSettings", absent)),
        ordinal,
    )
    check(
        "known portrait service state",
        ready.get("portraitWindow") in ("OpenPortraitWindow", "ClosedPortraitWindow")
        if "portraitWindow" in ready
        else None,
        ordinal,
    )
    portrait_draws = []
    expected_portrait = None
    if open_portrait:
        check(
            "registered portrait gate",
            match(
                dict(Registered=True, Moving=False, Closing=False),
                portrait if portrait is not None else absent,
            ),
            ordinal,
        )
        if portrait is not None and all(
            k in portrait for k in ("Blink", "Mouth", "EyesClosed", "MouthOpen")
        ):
            blink, mouth = int(portrait["Blink"]) - 1, int(portrait["Mouth"])
            eyes = portrait["EyesClosed"]
            mouth_open = portrait["MouthOpen"]
            if blink == 3:
                eyes = True
            if blink == 0:
                old = seed
                seed, value = random(seed, 120)
                blink = None if value is None else value + 30
                eyes = False
                portrait_draws.append(
                    dict(
                        Kind="rng-portrait-blink",
                        Before=old,
                        After=seed,
                        RandomRange=120,
                        RandomValue=value,
                    )
                )
            if mouth <= 5:
                old = seed
                seed, value = random(seed, 5)
                mouth = None if value is None else value + 10
                mouth_open = False
                portrait_draws.append(
                    dict(
                        Kind="rng-portrait-mouth",
                        Before=old,
                        After=seed,
                        RandomRange=5,
                        RandomValue=value,
                    )
                )
            expected_portrait = dict(
                Blink=blink, Mouth=mouth, EyesClosed=eyes, MouthOpen=mouth_open
            )
        else:
            check("portrait clock operands", None, ordinal)
            seed = None
    else:
        check(
            "closed portrait registration",
            None if "portraitWork" not in ready else portrait is None,
            ordinal,
        )
    observed_draws = [
        e
        for e in events
        if str(e.get("Kind", "")).startswith("rng-") and e.get("Kind") != "rng-text-w1"
    ]
    # Gate omission cannot hide an independently impossible observed draw.
    # Check its known operation/range/arithmetic even when its admission or
    # incoming service-chain seed still lacks an operand.
    for observed_draw in observed_draws:
        range_ = {"rng-portrait-blink": 120, "rng-portrait-mouth": 5}.get(observed_draw.get("Kind"))
        check("known W1 service RNG operation", range_ is not None, ordinal)
        if range_ is not None:
            draw_seed, draw_value = random(observed_draw.get("Before"), range_)
            check(
                "available service RNG arithmetic",
                match(
                    dict(
                        RandomRange=range_,
                        After=draw_seed if draw_seed is not None else absent,
                        RandomValue=draw_value if draw_value is not None else absent,
                    ),
                    observed_draw,
                ),
                ordinal,
            )
    expected_kinds = [e["Kind"] for e in portrait_draws]
    observed_kinds = [e["Kind"] for e in observed_draws]
    service_gates_known = (
        ready.get("portraitWindow") == "ClosedPortraitWindow" or expected_portrait is not None
    )
    ranks = [expected_kinds.index(k) for k in observed_kinds if k in expected_kinds]
    check("conditional RNG known order", ranks == sorted(ranks), ordinal)
    check(
        "complete conditional service draw inventory",
        None
        if not service_gates_known
        else merge(
            [
                not (set(observed_kinds) - set(expected_kinds)),
                len(observed_kinds) == len(set(observed_kinds)),
                True
                if observed_kinds == expected_kinds
                else None
                if not (set(observed_kinds) - set(expected_kinds))
                else False,
            ]
        ),
        ordinal,
    )
    for expected in portrait_draws:
        actual_draw = one(
            "conditional " + expected["Kind"],
            [e for e in observed_draws if e["Kind"] == expected["Kind"]],
            ordinal,
        )
        check(
            "independent portrait draw",
            match({k: v if v is not None else absent for k, v in expected.items()}, actual_draw),
            ordinal,
        )
    read_positions = [i for i, e in enumerate(events) if e.get("Kind") == "text-w1-input"]
    check(
        "portrait RNG between copy and read",
        None
        if not read_positions
        else all(2 < i < read_positions[0] for i, e in enumerate(events) if e in observed_draws),
        ordinal,
    )
    check(
        "complete main seed after service",
        match(seed if seed is not None else absent, after.get("mainSeed", absent)),
        ordinal,
    )
    return open_portrait, expected_portrait, read_positions
