"""Pinned-rule seeded portrait and NPC service evolution."""

from sf2tool.h3.rng import _rng_step


def _field_rng(seed: int, bound: int) -> tuple[int, int]:
    word, product = _rng_step(seed >> 16, (bound * 2) & 0xFFFF)
    return (word << 16) | (seed & 0xFFFF), product >> 1


def _field_portrait(seed: int, work: dict | None, typing: bool) -> dict:
    if work is None or not work["registered"]:
        return dict(seed=seed, work=work, draws=[])
    w = dict(work)
    events = []

    def consume(bound: int, kind: str) -> int:
        nonlocal seed
        before = seed
        seed, value = _field_rng(seed, bound)
        events.append(dict(kind=kind, before=before, after=seed, range=bound, value=value))
        return value

    w["blink"] = ((int(w["blink"]) - 1 + 32768) & 0xFFFF) - 32768
    if w["blink"] == 3:
        w["eyesClosed"] = True
    if w["blink"] == 0:
        w["eyesClosed"] = False
        w["blink"] = consume(120, "blink") + 30
    if typing:
        w["mouth"] = ((int(w["mouth"]) - 1 + 32768) & 0xFFFF) - 32768
        if w["mouth"] == 5:
            w["mouthOpen"] = True
    if (typing and w["mouth"] == 0) or (not typing and w["mouth"] <= 5):
        w["mouthOpen"] = False
        w["mouth"] = consume(5, "mouth") + 10
    return dict(seed=seed, work=w, draws=events)


_FIELD_MOTION = dict(
    x="x",
    y="y",
    targetX="xDest",
    targetY="yDest",
    velocityX="xVelocity",
    velocityY="yVelocity",
    travelX="xTravel",
    travelY="yTravel",
    speedX="xSpeed",
    speedY="ySpeed",
    accelerationX="xAccel",
    accelerationY="yAccel",
    flagsA="flagsA",
    flagsB="flagsB",
    facing="facing",
    layer="layer",
    animationCounter="animCounter",
    waitTimer="waitTimer",
)

_FIELD_COUNTERS = dict(Blink="blink", Mouth="mouth", EyesClosed="eyesClosed", MouthOpen="mouthOpen")

_FIELD_SERVICES = {
    "portrait-window-service",
    "text-mandatory-service",
    "text-w1-wait",
    "gameplay-wait",
    "simulation-tick",
}


def _field_action_program(actions):
    result = []
    for a in actions:
        op = a["op"]
        kind, operands = {
            "idle": ("IdleEntityAction", {}),
            "random-walk": (
                "RandomWalkEntity",
                dict(Origin=dict(X=a.get("x"), Y=a.get("y")), Radius=a.get("radius")),
            ),
            "jump": ("JumpEntityAction", dict(Instruction=a.get("instruction"))),
            "wait": ("WaitEntityTicks", dict(Ticks=a.get("ticks"))),
            "flags": (
                "ChangeEntityFlags",
                dict(FlagsB=a.get("field") == "b", Mask=a.get("mask"), Value=a.get("value")),
            ),
        }[op]
        result.append(dict(kind=kind, operands=operands))
    return result + [dict(kind="StopEntityActions", operands={})]


def _field_npc_tick(entities, seed, layout):
    import copy

    from sf2tool.h3.entity_movement import _core_tick

    entities = copy.deepcopy(entities)
    paths = []
    for e in entities:
        if e["id"] == "traveler":
            # This bounded stationary controlled player has no RNG/action program.
            if e["actionProgram"] is not None or e["moving"]:
                raise ValueError("controlled player outside stationary cohort")
            continue
        motion = {v: int(e[k]) for k, v in _FIELD_MOTION.items()}
        _core_tick(motion, layout[int(e["targetY"]) // 384][int(e["targetX"]) // 384])
        e.update({k: motion[v] for k, v in _FIELD_MOTION.items()})
        moving = e["x"] != e["targetX"] or e["y"] != e["targetY"]
        if e["waitingForMotion"] and moving:
            paths.append("motion-wait")
            continue
        e["waitingForMotion"] = False
        program = e["actionProgram"]
        if program is None:
            continue
        for _ in range(8):
            instruction = program[int(e["actionCursor"])]
            kind, a = instruction["kind"], instruction["operands"]
            if kind == "JumpEntityAction":
                e["actionCursor"] = a["Instruction"]
                e["waitTimer"] = 0
                continue
            if kind == "IdleEntityAction":
                e["waitTimer"] = e["waitTimer"] + 1 if e["waitTimer"] < 1 else 1
                paths.append("idle")
                break
            if kind == "WaitEntityTicks":
                if not 0 <= a["Ticks"] < 128 or not 0 <= e["waitTimer"] < 128:
                    raise ValueError("wait outside bounded signed-byte cohort")
                if e["waitTimer"] < a["Ticks"]:
                    e["waitTimer"] += 1
                    paths.append("timer-pending")
                    break
                e["waitTimer"] = 0
                e["actionCursor"] += 1
                paths.append("timer-release")
                continue
            if kind == "ChangeEntityFlags":
                key = "flagsB" if a["FlagsB"] else "flagsA"
                e[key] = (int(e[key]) & ~int(a["Mask"])) | (int(a["Value"]) & int(a["Mask"]))
                e["waitTimer"] = 0
                e["actionCursor"] += 1
                continue
            if kind != "RandomWalkEntity":
                raise ValueError("NPC action outside declared source cohort")
            for _ in range(4):
                seed, direction = _field_rng(seed, 4)
                x, y, r = e["x"], e["y"], a["Radius"]
                ox, oy = a["Origin"]["X"], a["Origin"]["Y"]
                outside = (
                    x >= (ox + r) * 384,
                    y <= (oy - r) * 384,
                    x <= (ox - r) * 384,
                    y >= (oy + r) * 384,
                )[direction]
                if outside:
                    paths.append("radius-rejected")
                    continue
                dx, dy = ((1, 0), (0, -1), (-1, 0), (0, 1))[direction]
                tx, ty = int(x) // 384 + dx, int(y) // 384 + dy
                if not 0 <= tx < len(layout[0]) or not 0 <= ty < len(layout):
                    raise ValueError("candidate outside flat layout")
                if int(e["flagsA"]) & 0x40 and layout[ty][tx] >= 0xC000:
                    paths.append("map-rejected")
                    continue
                if int(e["flagsA"]) & 0x20 and any(
                    q["slot"] != e["slot"]
                    and q["Visible"]
                    and abs(q["targetX"] - tx * 384) + abs(q["targetY"] - ty * 384) < 384
                    for q in entities
                ):
                    paths.append("entity-rejected")
                    continue
                e.update(
                    targetX=tx * 384,
                    targetY=ty * 384,
                    travelX=abs(tx * 384 - x),
                    travelY=abs(ty * 384 - y),
                    velocityX=dx * e["speedX"],
                    velocityY=dy * e["speedY"],
                    waitTimer=0,
                    waitingForMotion=True,
                )
                paths.append("walk-accepted")
                break
            e["actionCursor"] += 1
            break
        else:
            raise ValueError("action composition budget")
    return entities, seed, paths
