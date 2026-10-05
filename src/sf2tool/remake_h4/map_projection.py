"""Admit legacy/current viewport planes and source actor census for one layout witness."""


def admit_projection(sample, layout, draw, role, label, initial, school_entities, check):
    projected = sample
    actor_population_complete = True
    if role == "house":
        start = initial[1 if label.startswith("flag-off") else 0]
        p = dict(start["mapViewport"], scale=start["mapViewport"]["height"] / 192)
        check("legacy uninitialized view witness", start["logicalView"] is None, label)
        foreground = draw["foreground"]
        layers = [
            dict(
                name="background",
                x=foreground["x"],
                y=foreground["y"],
                offsetX=0,
                offsetY=0,
                highPriority=None,
                pass_=0,
            )
        ]
        if foreground["enabled"]:
            layers.append(
                dict(
                    name="foreground",
                    x=foreground["x"],
                    y=foreground["y"],
                    offsetX=foreground["offsetX"],
                    offsetY=foreground["offsetY"],
                    highPriority=None,
                    pass_=4 + sum(e["Visible"] for e in start["entities"]),
                )
            )
        for layer in layers:
            layer["pass"] = layer.pop("pass_")
        projected = dict(
            sample,
            projection={
                **{k: p[k] for k in ("x", "y", "width", "height", "scale")},
                "layers": layers,
                "actors": [],
            },
        )
    else:
        s, p = sample["state"], sample["projection"]
        if role == "school":
            actor_ids = [a.get("entity") for a in p["actors"]]
            actor_population_complete = set(actor_ids) == school_entities
            check(
                "source school population independent of draw uses",
                None
                if set(actor_ids) < school_entities
                else set(actor_ids) == school_entities and len(actor_ids) == len(school_entities),
                label,
            )
            check(
                "school unjoined source population gates",
                not {1, 2, 602, 603}.intersection(layout["flags"]),
                label,
            )
        check(
            "input-ready snapshot without failure",
            s["failure"] is None and s["wait"] is None and s["stop"] == "PlayerInput",
            label,
        )
        check(
            "projection session/revision",
            all(
                p[k] == layout[k] == s[k]
                for k in ("sessionId", "map", "revision", "observationSequence")
            ),
            label,
        )
        check(
            "actual viewport",
            all(abs(p[k] - s["mapViewport"][k]) < 0.002 for k in ("x", "y", "width", "height")),
            label,
        )
        check(
            "source viewport ratio",
            abs(p["width"] - 320 * p["scale"]) < 0.002
            and abs(p["height"] - 192 * p["scale"]) < 0.002,
            label,
        )
        view = s["logicalView"]
        expected_layers = [
            ("background", False, 0, "BX", "BY"),
            ("foreground", False, 1, "AX", "AY"),
            ("backgroundHigh", True, 2, "BX", "BY"),
            ("foregroundHigh", True, 3, "AX", "AY"),
        ]
        area = view["Area"]
        foreground = any(
            area[a] != area[b]
            for a, b in (
                ("ForegroundX", "BackgroundX"),
                ("ForegroundY", "BackgroundY"),
                ("ParallaxAX", "ParallaxBX"),
                ("ParallaxAY", "ParallaxBY"),
            )
        )
        if not foreground:
            expected_layers = [row for row in expected_layers if row[0].startswith("background")]
        check(
            "complete source plane passes",
            len(p["layers"]) == len(expected_layers),
            label,
        )
        for layer, want in zip(p["layers"], expected_layers, strict=False):
            name, high, pass_, ax, ay = want
            check(
                "source layer/pass/priority/origin",
                layer["name"] == name
                and layer["highPriority"] is high
                and layer["pass"] == pass_
                and layer["offsetX"] == layer["offsetY"] == 0
                and layer["x"] == view[ax]["Position"] / 16
                and layer["y"] == view[ay]["Position"] / 16,
                label,
            )
    return projected, actor_population_complete
