"""Construct source cell geometry and match the executed resource multiset."""

from sf2tool.remake_h4.map_actors import SpriteInk, admit_actors
from sf2tool.remake_h4.map_geometry import add, div, f, intersect, mul, rect, sub


def expected_cells(
    layout,
    projection,
    expected_words,
    source,
    screen,
    scale,
    actor_rows,
    missing_actor,
    unknown_subjects,
    sprite_ink,
):
    expected = []
    for layer in projection["layers"]:
        overlay, high = layer["name"].startswith("foreground"), layer["highPriority"]
        for index, word in enumerate(expected_words):
            sx = int(layout["x"] + index % layout["width"])
            sy = int(layout["y"] + index // layout["width"])
            block = int(word) & 1023
            if overlay and block == 0:
                continue
            x, y = sx - layer["offsetX"], sy - layer["offsetY"]
            block_rect = (
                add(projection["x"], mul(sub(x * 24, layer["x"]), scale)),
                add(projection["y"], mul(sub(y * 24, layer["y"]), scale)),
                mul(24, scale),
                mul(24, scale),
            )
            if not intersect(block_rect, screen):
                continue
            masks = [(None, int(layer["pass"]), [screen])]
            if high:
                for actor, bounds, visible, actor_high, actor_pass in actor_rows:
                    if bounds is None or not visible or not intersect(block_rect, bounds):
                        continue
                    if actor_high is None or actor_pass is None:
                        missing_actor = True
                        unknown_subjects.add(actor["entity"])
                        continue
                    if not actor_high:
                        try:
                            ink = sprite_ink.regions(actor)
                        except KeyError:
                            missing_actor = True
                            unknown_subjects.add(actor["entity"])
                            continue
                        masks.append(
                            (
                                actor["entity"],
                                actor_pass + (2 if overlay else 1),
                                ink,
                            )
                        )
            for tile in range(1 if high is None else 9):
                tile_words = (
                    source["blocks"][block] if high is None else [source["blocks"][block][tile]]
                )
                if high is not None and bool(tile_words[0] & 0x8000) != high:
                    continue
                tx, ty, size = (0, 0, 24) if high is None else (tile % 3 * 8, tile // 3 * 8, 8)
                origin = (
                    add(block_rect[0], mul(tx, scale)),
                    add(block_rect[1], mul(ty, scale)),
                    mul(size, scale),
                    mul(size, scale),
                )
                clipped = intersect(origin, screen)
                if not clipped:
                    continue
                for subject, pass_, regions in masks:
                    for mask in regions:
                        covered = intersect(clipped, mask)
                        if covered:
                            texture = (
                                add(tx, div(sub(covered[0], origin[0]), scale)),
                                add(ty, div(sub(covered[1], origin[1]), scale)),
                                div(covered[2], scale),
                                div(covered[3], scale),
                            )
                            key = (
                                sx,
                                sy,
                                block,
                                None if high is None else tile,
                                tuple(tile_words),
                                "occlusion"
                                if subject
                                else "foreground"
                                if overlay
                                else "background",
                                high,
                                pass_,
                                subject,
                                overlay,
                                layer["offsetX"],
                                layer["offsetY"],
                            )
                            expected.append((key, covered, texture))
    return expected, missing_actor


def match_cells(layout, expected, actor_ok, missing_actor, unknown_subjects):
    groups, selector_ok, missing_use = {}, True, False
    required = {
        "sourceX",
        "sourceY",
        "block",
        "tile",
        "words",
        "layer",
        "highPriority",
        "pass",
        "subject",
        "overlay",
        "offsetX",
        "offsetY",
        "resourceIdentity",
        "selector",
        "x",
        "y",
        "width",
        "height",
        "textureX",
        "textureY",
        "textureWidth",
        "textureHeight",
    }
    for use in layout["draw"]["uses"]:
        if not required <= use.keys():
            missing_use = True
            continue
        key = (
            tuple(use[k] for k in ("sourceX", "sourceY", "block", "tile"))
            + (tuple(use["words"]),)
            + tuple(
                use[k]
                for k in (
                    "layer",
                    "highPriority",
                    "pass",
                    "subject",
                    "overlay",
                    "offsetX",
                    "offsetY",
                )
            )
        )
        groups.setdefault(key, []).append(use)
        selector_ok &= bool(use["resourceIdentity"]) and use["selector"] == dict(
            kind="map-block", map=layout["map"], block=use["block"]
        )
    missing = 0
    for key, covered, texture in expected:
        candidates = groups.get(key, [])
        found = next(
            (
                i
                for i, use in enumerate(candidates)
                if max(abs(a - b) for a, b in zip(covered, rect(use), strict=True)) < 0.002
                and max(
                    abs(a - b)
                    for a, b in zip(
                        texture,
                        (use[k] for k in ("textureX", "textureY", "textureWidth", "textureHeight")),
                        strict=True,
                    )
                )
                < 0.002
            ),
            None,
        )
        if found is None:
            missing += 1
        else:
            candidates.pop(found)
    extra = sum(len(uses) for key, uses in groups.items() if key[8] not in unknown_subjects)
    return dict(
        value=False
        if extra or not selector_ok or not actor_ok
        else None
        if missing or missing_actor or missing_use
        else True,
        expected=len(expected),
        actual=len(layout["draw"]["uses"]),
        missing=missing,
        extra=extra,
        missingActor=missing_actor,
        missingUse=missing_use,
        selectors=selector_ok,
        actorAdmission=actor_ok,
    )


def draw_cells(sample, expected_words, source, root, *, actor_population_complete=True):
    """Compare independent source cells/masks with actual clipped draw occurrences."""
    layout, projection = sample["layout"], sample["projection"]
    screen, scale = rect(projection), f(projection["scale"])
    actor_ok, actor_rows, missing_actor, unknown_subjects = admit_actors(
        projection, screen, scale, actor_population_complete
    )
    expected, missing_actor = expected_cells(
        layout,
        projection,
        expected_words,
        source,
        screen,
        scale,
        actor_rows,
        missing_actor,
        unknown_subjects,
        SpriteInk(root, scale),
    )
    return match_cells(layout, expected, actor_ok, missing_actor, unknown_subjects)
