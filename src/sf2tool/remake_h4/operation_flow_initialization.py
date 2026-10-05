"""First destination pose, source allocation, overrides and saved request."""

import re
from pathlib import Path

from sf2tool.remake_h4.operation_flow_indexes import entity, signature


def operation_flow_initialization(
    post,
    request,
    transfer,
    stop,
    target,
    route,
    flags,
    ordered,
    states,
    state_of,
    anchor,
    instruction,
    compiler,
    source_root,
    tracked,
    names,
    check,
    seq,
    _bounded_list,
    _bounded_sorted,
    _tokens,
):
    pose = entity(state_of(post), "entity-0")
    expected_position = request["position"]
    expected_facing = request["facing"]
    for e in ordered:
        ins = instruction(e)
        if (
            transfer["Sequence"] < e["Sequence"] <= post[0]
            and ins
            and ins["op"] == "position"
            and ins["entity"] == "entity-0"
        ):
            expected_position = ins["position"]
        if (
            transfer["Sequence"] < e["Sequence"] <= post[0]
            and ins
            and ins["op"] == "face"
            and ins["entity"] == "entity-0"
        ):
            expected_facing = ins["facing"]
    initialized_states = _bounded_list(
        state_of(x)
        for x in states
        if x[0] == post[0]
        and state_of(x).get("map") == request["map"]
        and state_of(x).get("entities")
    )
    for initialized in initialized_states:
        player = entity(initialized, "entity-0") or {}
        check(
            names[3],
            "source requested or initialized player facing",
            None if player.get("facing") is None else player["facing"] == expected_facing,
            seq,
        )
    # Pinned setup entity declarations and the existing population lowering
    # supply allocation identity/sprites, independently of the actual list.
    population = None
    templates = None
    try:
        population = compiler.population()
        check(
            names[3],
            "source allocation population",
            target.get("population") == population,
            seq,
        )
        map_number = int(request["map"].split("-")[1])
        folder = f"disasm/data/maps/entries/map{map_number:02d}/mapsetups/"
        pointer = route["defaultPointer"] if route else None
        table = (
            next(
                (
                    path
                    for path in _bounded_sorted(tracked)
                    if path.startswith(folder)
                    and Path(path).name.startswith("pointertable")
                    and re.search(
                        rf"^{re.escape(pointer)}:",
                        (source_root / path).read_text(encoding="utf-8"),
                        re.MULTILINE,
                    )
                ),
                None,
            )
            if pointer
            else None
        )
        templates = []
        if table:
            symbol = compiler.source_operations(table, pointer)[0]["operandText"]
            entity_path = next(
                path
                for path in _bounded_sorted(tracked)
                if path.startswith(folder)
                and Path(path).name.startswith("s1_entities")
                and re.search(
                    rf"^{re.escape(symbol)}:",
                    (source_root / path).read_text(encoding="utf-8"),
                    re.MULTILINE,
                )
            )
            npc = population["nonAllyStart"]
            for row in compiler.source_operations(entity_path, symbol):
                if row["opcode"] == "msEntitiesEnd":
                    break
                if row["opcode"] not in ("msFixedEntity", "msWalkingEntity"):
                    raise ValueError("unbound setup entity declaration")
                args = _tokens(row["operandText"])
                sprite = compiler.number(args[3])
                if sprite >= compiler.equates["MAPSPRITES_SPECIALS_START"]:
                    raise ValueError("unbound special allocation")
                identity = sprite if sprite < population["allyCount"] else npc
                if identity == npc:
                    npc += 1
                templates.append(("entity-" + str(identity), sprite))
        check(
            names[3],
            "source setup entity declarations",
            [(p["id"], p.get("sprite")) for p in target["entities"]] == templates,
            seq,
        )
    except (KeyError, OSError, ValueError, StopIteration):
        templates = None
        check(names[3], "source allocation operands absent", None, seq)
    expected_signature = None
    if request["loadMode"] == "preserve":
        prior = anchor(transfer["Sequence"] - 1, "entities")
        if prior:
            expected_signature = signature(state_of(prior))
    elif templates is not None and population and flags is not None:
        sprites = {
            p["character"]: (
                p["unjoinedSprite"]
                if p["joinedFlag"] is not None and p["joinedFlag"] not in flags
                else p["sprite"]
            )
            for p in population["allySprites"]
        }
        identities = [("entity-0", sprites[0])]
        identities.extend(
            ("entity-" + str(p["character"]), sprites.get(p["character"], p["sprite"]))
            for p in population["followers"]
            if p["flag"] in flags
        )
        for identity, sprite in templates:
            if identity not in {p[0] for p in identities}:
                identities.append((identity, sprites.get(int(identity.split("-")[1]), sprite)))
        expected_signature = [
            (identity, slot, sprite) for slot, (identity, sprite) in enumerate(identities)
        ]
    if expected_signature is not None:
        for e in ordered:
            ins = instruction(e)
            if transfer["Sequence"] < e["Sequence"] <= post[0] and ins and ins["op"] == "sprite":
                expected_signature = [
                    (identity, slot, ins["sprite"] if identity == ins["entity"] else sprite)
                    for identity, slot, sprite in expected_signature
                ]
    for initialized in initialized_states:
        check(
            names[3],
            "actual initialized physical allocation and sprites",
            None
            if expected_signature is None
            or any(
                p.get(k) is None for p in initialized["entities"] for k in ("id", "slot", "sprite")
            )
            else signature(initialized) == expected_signature,
            seq,
        )
    check(
        names[3],
        "destination or intervening source initialization pose",
        pose["x"] == expected_position["x"] * 384 and pose["y"] == expected_position["y"] * 384,
        seq,
    )
    latch = next(
        (
            state_of(x).get("warp")
            for x in states
            if x[0] >= transfer["Sequence"]
            and x[0] < stop
            and state_of(x).get("map") == request["map"]
            and state_of(x).get("warp")
        ),
        None,
    )
    if expected_position != request["position"]:
        check(
            names[3],
            "overwritten destination retains independent requested operand",
            True if latch else None,
            seq,
        )
    if latch:
        check(
            names[3],
            "retained request independent of later initialized pose",
            latch["Map"]["Value"] == request["map"]
            and latch["Position"] == dict(X=request["position"]["x"], Y=request["position"]["y"])
            and latch["Facing"] == request["facing"],
            seq,
        )
