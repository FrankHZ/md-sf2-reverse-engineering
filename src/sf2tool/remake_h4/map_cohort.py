"""Define source-derived region obligations for the admitted delivery cohort."""


def required_regions(maps):
    sources = {
        "house": "map-3",
        "school": "map-3",
        "castle-walk": "map-19",
        "castle-rebuild": "map-19",
    }
    wanted = {}

    def region(role, label, table, ordinal, state, roof):
        copy = maps[sources[role]][table][ordinal]
        wanted[role, label] = (copy["rect"], state, roof, copy)

    for label in ("door-before", "door-opened", "door-repeat-read", "door-revisited"):
        region("house", label, "doors", 0, "base" if label == "door-before" else "copy", 1)
    for label in (
        "roof-restored",
        "roof-repeat-read",
        "roof-cleared",
        "roof-clear-repeat",
        "roof-restored-again",
    ):
        clear = label in ("roof-cleared", "roof-clear-repeat")
        region("house", label, "roofs", 0, "clear" if clear else "base", 1 if clear else 0)
    for label in ("flag-off-load", "flag-off-repeat"):
        region("house", label, "flags", 0, "base", 6)
    for label in (
        "school-door-before",
        "school-door-open",
        "school-door-preserved-away",
        "school-door-retained",
    ):
        region(
            "school",
            label,
            "doors",
            5,
            "base" if label.endswith("before") else "copy",
            0 if label.endswith(("before", "retained")) else 10,
        )
    for label in (
        "school-roof-before",
        "school-roof-clear",
        "school-preserved-away",
        "school-preserved-return",
        "school-roof-restored",
        "school-roof-repeat",
    ):
        clear = label in ("school-roof-clear", "school-preserved-away", "school-preserved-return")
        region("school", label, "roofs", 9, "clear" if clear else "base", 10 if clear else 0)
    for role, names in (
        (
            "castle-walk",
            (
                ("castle-base", "base", 0),
                ("castle-clear", "clear", 2),
                ("castle-restore", "base", 0),
            ),
        ),
        (
            "castle-rebuild",
            (("castle-initial-inside", "clear", 1), ("castle-rebuilt-inside", "clear", 1)),
        ),
    ):
        copy = maps["map-19"]["roofs"][0]
        x, y, w, h = copy["rect"]
        for prefix, state, roof in names:
            for offset in range(0, h, 6):
                wanted[role, f"{prefix}-{y + offset}"] = (
                    (x, y + offset, w, min(6, h - offset)),
                    state,
                    roof,
                    copy,
                )
    return sources, wanted
