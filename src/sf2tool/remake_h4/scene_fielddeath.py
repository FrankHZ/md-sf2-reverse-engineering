"""Source field-death batches and mounted dead-actor projections."""

from sf2tool.remake_h4.scene_checks import absent, actor, picked


def field_batches(census, pairs, events, ordered, checks):
    eq = checks.eq
    field_batches = {}
    for c in census:
        first, end = c["sequence"], c["end"]["sequence"]
        next_scene = next((x["sequence"] for x in census if x["sequence"] > end), float("inf"))
        field = [
            p for p in pairs if end < p["token"] < next_scene and p["phase"].startswith("Field")
        ]
        deaths = list(
            dict.fromkeys(
                actor(events[q])
                for q in ordered
                if first < q < end and events[q]["Kind"] == "hp" and events[q].get("After") == 0
            )
        )
        eq(
            "source death batch phases",
            ["FieldSpin"] * 12 + ["FieldExit"] * 3 + ["FieldSettle"] if deaths else [],
            [p["phase"] for p in field],
            first,
        )
        for n, p in enumerate(field):
            field_batches[p["token"]] = dict(
                scene=first,
                actors=deaths,
                phases=field,
                step=n if n < 12 else n - 12 if n < 15 else 0,
            )

    return field_batches


def compare_field_projection(row, phase, batch, materials, checks):
    check, eq = checks.check, checks.eq
    index, s = row["_index"], row["scene"]
    step = batch["step"]
    eq(
        "source field death operands",
        dict(
            actors=batch["actors"],
            step=step,
            facing=(11 - step) & 3 if phase == "FieldSpin" else 1 + step,
            delay=3 if phase == "FieldSpin" else 8 if phase == "FieldExit" else 10,
        ),
        s.get("fieldDeath", absent),
        index,
    )
    eq(
        "no remaining fairy effect at field death",
        [],
        s.get("fairySprites", absent),
        index,
    )
    actors = {a.get("id"): a for a in row.get("fieldActors", [])}
    for dead in batch["actors"]:
        a = actors.get(dead, {})
        eq("dead actor HP", 0, a.get("hp", absent), index)
        check(
            "dead actor remains in roster census",
            dead in row["fieldActorIds"]
            if "fieldActorIds" in row
            else dead in {a.get("id") for a in row["fieldActors"]}
            if "fieldActors" in row
            else None,
            index,
        )
        sprite = a.get("sprite") or {}
        if phase == "FieldSettle":
            eq(
                "settled dead node hidden",
                dict(visible=False, visibleInTree=False),
                sprite,
                index,
            )
        else:
            eq(
                "death node consumed",
                dict(visible=True, visibleInTree=True, texturePresent=True),
                sprite,
                index,
            )
            selector = sprite.get("resourceSelector") or {}
            if phase == "FieldExit":
                eq(
                    "field exit sprite identity",
                    dict(sprite=63, direction=step, frame=0),
                    selector,
                    index,
                )
                eq(
                    "field exit resource",
                    picked(materials.get("fieldDeath", {}).get("exitFrames", []), step),
                    selector.get("raster", absent),
                    index,
                )
            elif phase == "FieldSpin":
                facing = (11 - step) & 3
                original_sprite = next(
                    (
                        v["sprite"]
                        for v in materials.get("fieldDeath", {}).get("allies", [])
                        if dead == "ally-" + str(v["character"])
                    ),
                    materials.get("fieldDeath", {}).get("enemies", [{}])[0].get("sprite", absent),
                )
                eq(
                    "spin original sprite identity",
                    dict(
                        sprite=original_sprite,
                        direction=0 if facing == 1 else 2 if facing == 3 else 1,
                        frame=0,
                        raster=None,
                    ),
                    selector,
                    index,
                )
                eq("spin facing consumed", facing, sprite.get("facing", absent), index)
