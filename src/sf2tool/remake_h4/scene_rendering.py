"""Mounted resources, animation entries, fairy cursors, reactions and source messages."""

from sf2tool.remake_h4.scene_checks import absent, actor, number, picked


class AnimationProgress:
    """Observed animation entries for one phase, owned by its caller."""

    def __init__(self):
        self.last_frame = -1
        self.played = set()
        self.sequence = None
        self.frames = []


def compare_rendering(
    row,
    phase,
    info,
    token,
    healing,
    materials,
    sequences,
    events,
    owners,
    ordered,
    source_materials,
    progress,
    checks,
):
    """Compare one mounted actor/animation/fairy/reaction/message projection."""
    check, eq = checks.check, checks.eq
    visual, render = source_materials.visual, source_materials.render
    index, s = row["_index"], row["scene"]
    a, target = info.get("actor"), info.get("target")
    ally = (
        (
            target
            if phase
            in (
                "TargetEnter",
                "Reaction",
                "ResultMessage",
                "MakeIdle",
                "SpellStop",
                "ActorExit",
            )
            else a
        )
        if info.get("recovery")
        else a
        if str(a).startswith("ally-")
        else target
    )
    enemy = None if info.get("recovery") else target if str(a).startswith("ally-") else a
    eq(
        "source displayed actors",
        dict(
            displayedAlly=ally,
            displayedEnemy=enemy,
            enemyVisible=enemy is not None,
            actionKind=info.get("kind"),
            reactionKind=info.get("reaction"),
            reactionAmount=info["amount"] if info.get("amount") is not None else absent,
            item=None,
            spellAnimationSelector=4 if info.get("recovery") else None,
        ),
        s,
        index,
    )
    eq(
        "source spell selector",
        dict(Level=1, Value="heal") if info.get("recovery") else None,
        s.get("spell", absent),
        index,
    )
    for layer, resource in (
        ("background", materials.get("background", absent)),
        ("backgroundWrap", materials.get("background", absent)),
        ("ground", materials.get("ground", absent)),
    ):
        eq(
            "mounted canonical " + layer,
            dict(resource=resource, texturePresent=True, visible=True),
            s.get(layer, absent),
            index,
        )
    for side, who in (("ally", ally), ("enemy", enemy)):
        if who is None:
            eq("absent enemy resource", None, s.get("enemyResource", absent), index)
            continue
        v = visual(who)
        frame = s.get(side + "Frame")
        if number(frame) and frame < len(v.get("frames", [])):
            eq(
                "mounted actor frame resource",
                v["frames"][int(frame)],
                s.get(side + "Resource", absent),
                index,
            )
        else:
            check(
                "mounted actor frame index",
                False if frame is not None and v else None,
                index,
            )
    motion_actor = a if phase == "ActionAnimation" else target
    purpose = "cast" if info.get("recovery") else "attack"
    animation = (
        phase == "ActionAnimation" or phase == "Reaction" and info.get("reaction") == "Dodge"
    )
    if phase == "Reaction":
        purpose = "dodge"
    mv = visual(motion_actor)
    source_sequence = (
        sequences.get((mv.get("side"), mv.get("sprite"), purpose)) if animation else None
    )
    frame_index = s.get("frameIndex")
    if not (animation and mv.get("side") == "ally" and number(frame_index)):
        av = visual(ally)
        idle = sequences.get(("ally", av.get("sprite"), "idle"))
        if idle and idle.get("idleWeapon"):
            w = idle["idleWeapon"]
            wf = int(w["frame"])
            eq(
                "source idle weapon binding",
                dict(
                    weaponResource=av["weaponFrames"][wf & 7],
                    weaponVisible=True,
                    weaponFlipH=bool(wf & 16),
                    weaponFlipV=bool(wf & 32),
                    weaponX=s.get("allyX", 136) + w["x"],
                ),
                s,
                index,
            )
        else:
            check("source idle weapon available", None, index)
    if animation:
        eq(
            "action animation source selector",
            source_sequence["index"]
            if source_sequence and phase == "ActionAnimation"
            else None
            if phase != "ActionAnimation"
            else absent,
            s.get("animationIndex", absent),
            index,
        )
        frames = (
            source_sequence["frames"][1 if mv.get("side") == "ally" else 0 :]
            if source_sequence
            else []
        )
        if number(frame_index):
            check("sequence frame order", frame_index >= progress.last_frame, index)
            progress.last_frame = frame_index
            progress.played.add(int(frame_index))
            check(
                "source frame range",
                frame_index < len(frames) if source_sequence else None,
                index,
            )
            if frame_index < len(frames):
                entry = frames[int(frame_index)]
                # Hold-15 preserves the last source frame even across unsampled ticks.
                source_frame = next(
                    (
                        f["frame"]
                        for f in reversed(frames[: int(frame_index) + 1])
                        if f["frame"] != 15
                    ),
                    0,
                )
                side = mv["side"]
                eq(
                    "source sequence frame consumed",
                    source_frame,
                    s.get(side + "Frame", absent),
                    index,
                )
                eq(
                    "source frame offsets",
                    {
                        side + "X": (136 if side == "ally" else 16) + entry["x"],
                        side + "Y": (64 if side == "ally" else 48) + entry["y"],
                    },
                    s,
                    index,
                )
                if side == "ally" and entry["weapon"]:
                    w = entry["weapon"]
                    wf = int(w["frame"])
                    eq(
                        "source weapon frame/flip/offset",
                        dict(
                            weaponResource=mv["weaponFrames"][wf & 7],
                            weaponVisible=True,
                            weaponFlipH=bool(wf & 16),
                            weaponFlipV=bool(wf & 32),
                            weaponX=136 + entry["x"] + w["x"],
                        ),
                        s,
                        index,
                    )
        elif frame_index != -1:
            check("source frame range", False if frame_index is not None else None, index)
    if info.get("recovery") and index not in healing:
        check("required exact HEAL cursor", None, index)
    elif info.get("recovery"):
        cursor = healing[index].get("scene", {}).get("healing") or {}
        fairy = cursor.get("Fairy") or {}
        needed = {}

        def put(node, resource, x, y, mirror=False, needed=needed):
            needed[node] = dict(
                name=node,
                binding=dict(resource=resource, texturePresent=True, visible=True),
                x=x - 128,
                y=y - 128,
                mirror=mirror,
            )

        if fairy.get("Control"):
            for n, f in enumerate(fairy.get("Fairies", [])):
                if f.get("Active"):
                    for part, key, frame in (
                        ("Body", "bodies", "BodyFrame"),
                        ("Wings", "wings", "WingFrame"),
                    ):
                        put(
                            "Fairy" + part + str(n),
                            picked(materials.get("healing", {}).get(key, []), int(f[frame])),
                            f["X"],
                            f["Y"],
                            f["Mirrored"],
                        )
            for n, dust in enumerate(fairy.get("Dust", [])):
                if dust.get("Age"):
                    put(
                        "FairyDust" + str(n),
                        picked(materials.get("healing", {}).get("dust", []), int(dust["Frame"])),
                        dust["X"],
                        dust["Y"],
                    )
        mounted = {f.get("name"): f for f in s.get("fairySprites", [])}
        check(
            "fairy required node census",
            False
            if mounted.keys() - needed.keys()
            else None
            if needed.keys() - mounted.keys()
            else True,
            index,
        )
        for node, expected in needed.items():
            eq(
                "fairy cursor to mounted resource/effect",
                expected,
                mounted.get(node, absent),
                index,
            )
    if phase == "Reaction" and info.get("reaction") == "Damage":
        rs = s.get("reactionState")
        draws = [
            events[q]
            for q in ordered
            if owners[q] == owners[token]
            and events[q]["Kind"] in ("rng-reaction-x", "rng-reaction-y")
        ]
        if number(rs):
            check("reaction source state range", rs < len(draws) // 2, index)
            if rs < len(draws) // 2:
                x, y = draws[2 * int(rs) : 2 * int(rs) + 2]
                origin = 2 if str(target).startswith("ally-") else 3
                dx = (x["RandomValue"] - origin) * 2
                dy = -(y["RandomValue"] - origin) * 2
                eq(
                    "sampled source reaction offsets",
                    dict(enemyX=16 + dx, enemyY=48 + dy),
                    s,
                    index,
                )
                if str(target).startswith("ally-"):
                    eq(
                        "sampled ally reaction layer offsets",
                        dict(allyX=136 + dx, allyY=64 + dy, groundX=136 + dx, backgroundX=dx),
                        s,
                        index,
                    )
        elif rs != -1:
            check("reaction source state range", False if rs is not None else None, index)
        if s.get("completed") is True:
            eq("reaction reaches final source state", 11, rs, index)
    if not info.get("recovery"):
        eq("no stray fairy consumer", [], s.get("fairySprites", absent), index)
    # Source text identities plus independently accepted effect operands.
    text_id, who, amount, spell = None, target, info.get("amount", 0), ""
    if phase == "ActionMessage" or info.get("recovery") and phase == "SpellCost":
        text_id = (
            274
            if info.get("recovery")
            else 293
            if info["kind"] == "physical-second"
            else 292
            if info["kind"] == "physical-counter"
            else 273
        )
        who = a
        if info.get("recovery"):
            amount, spell = 1, "HEAL"
    elif phase == "ResultMessage" or info.get("recovery") and phase in ("MakeIdle", "SpellStop"):
        text_id = (
            298
            if info.get("recovery")
            else 286
            if info["reaction"] == "Dodge"
            else (287 if str(a).startswith("ally-") else 288)
            if info["critical"]
            else 284
        )
    elif phase == "DeathMessage":
        text_id = 291 if str(target).startswith("ally-") else 290
    elif phase == "RewardMessage" and info["exp"]:
        e = info["exp"][0]
        text_id, who, amount = 263, actor(e), e["After"] - e["Before"]
    elif phase == "GoldMessage" and info["gold"]:
        text_id, amount = 393, sum(e["After"] - e["Before"] for e in info["gold"])
    elif phase == "GrowthMessage" and info.get("growth"):
        e = info["growth"]
        text_id = {
            "level": 244,
            "level-max-hp": 266,
            "level-max-mp": 267,
            "level-base-attack": 268,
            "level-defense": 269,
            "level-agility": 270,
        }[e["Kind"]]
        who, amount = (
            actor(e),
            e["After"] if e["Kind"] == "level" else e["After"] - e["Before"],
        )
    eq(
        "source message and effect operands",
        render(text_id, who, amount, spell) if text_id is not None else "",
        s.get("message", absent),
        index,
    )
    progress.sequence = source_sequence
    if animation:
        progress.frames = frames
