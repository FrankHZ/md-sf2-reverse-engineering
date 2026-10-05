"""Check available use facts, reconstruct working/saved words and admit snapshot identities."""


def compare_use_facts(layout, source, map_id, label, check):
    # Available source/resource contradictions survive an unrelated missing
    # layout, roof, actor or use operand earlier in this same region.
    for use in (layout.get("draw") or {}).get("uses", []):
        try:
            block, tile = use["block"], use["tile"]
            valid = (
                isinstance(block, int)
                and 0 <= block < len(source["blocks"])
                and (tile is None or isinstance(tile, int) and 0 <= tile < 9)
            )
            check("valid original block/tile operand", valid, label)
            if not valid:
                continue
            words = source["blocks"][block] if tile is None else [source["blocks"][block][tile]]
            check(
                "available actual source words",
                None if "words" not in use else use["words"] == words,
                label,
            )
            check(
                "available actual resource selector",
                None
                if "selector" not in use
                else use["selector"] == dict(kind="map-block", map=map_id, block=block),
                label,
            )
            check(
                "actual resource identity present",
                True if use.get("resourceIdentity") else None,
                label,
            )
            check(
                "positive executed destination and texture extent",
                None
                if not {"width", "height", "textureWidth", "textureHeight"} <= use.keys()
                else all(use[k] > 0 for k in ("width", "height", "textureWidth", "textureHeight")),
                label,
            )
        except (KeyError, TypeError):
            check("draw use source operand absent", None, label)


def compare_layout(layout, obligation, map_id, source, label, check):
    rect_, state, roof, copy = obligation
    x, y, w, h = rect_
    check(
        "source-derived region",
        layout["map"] == map_id
        and tuple(layout[k] for k in ("x", "y", "width", "height")) == rect_,
        label,
    )
    expected = [
        0
        if state == "clear"
        else source["layout"][(copy["source"][1] + dy) * 64 + copy["source"][0] + dx]
        if state == "copy"
        else source["layout"][(y + dy) * 64 + x + dx]
        for dy in range(h)
        for dx in range(w)
    ]
    check(
        "working words equal source operation",
        None if "words" not in layout else layout["words"] == expected,
        label,
    )
    actual_roof = layout["roof"]
    check(
        "busy roof record",
        actual_roof is None
        if roof == 0
        else actual_roof is not None and actual_roof["RecordOrdinal"] == roof,
        label,
    )
    if actual_roof is not None:
        roof_copy = source["roofs"][int(actual_roof["RecordOrdinal"]) - 1]
        rx, ry, rw, rh = roof_copy["rect"]
        check(
            "source saved rectangle",
            tuple(actual_roof[k] for k in ("DestinationX", "DestinationY", "Width", "Height"))
            == roof_copy["rect"],
            label,
        )
        expected_saved = [
            dict(x=cx, y=cy, word=source["layout"][cy * 64 + cx])
            for cy in range(y, y + h)
            for cx in range(x, x + w)
            if rx <= cx < rx + rw and ry <= cy < ry + rh and roof_copy["source"][1] >= 128
        ]
        check(
            "saved original words and full intersecting inventory",
            actual_roof["saved"] == expected_saved,
            label,
        )
    check("flag-off operand", 506 not in layout["flags"], label)
    return expected


def compare_identity(layout, draw, session, role, label, role_sessions, identities, check):
    identity = tuple(
        layout[k]
        for k in (
            "sessionId",
            "map",
            "revision",
            "observationSequence",
            "x",
            "y",
            "width",
            "height",
        )
    )
    check(
        "same snapshot consumer",
        identity
        == tuple(
            draw[k]
            for k in (
                "sessionId",
                "map",
                "revision",
                "observationSequence",
                "x",
                "y",
                "width",
                "height",
            )
        ),
        label,
    )
    check(
        "controlled session distinct from history",
        layout["sessionId"] != session,
        label,
    )
    role_key = (role, 1 if label.startswith("flag-off") else 0)
    if role_key in role_sessions:
        check(
            "controlled case session continuity",
            role_sessions[role_key] == layout["sessionId"],
            label,
        )
    role_sessions[role_key] = layout["sessionId"]
    prior = identities.get(layout["sessionId"])
    check(
        "fresh draw request and execution",
        prior is None or draw["request"] > prior[0] and draw["drawSequence"] > prior[1],
        label,
    )
    identities[layout["sessionId"]] = (draw["request"], draw["drawSequence"])
    check(
        "bounded complete draw channel",
        draw["limit"] == 4096 and draw["overflow"] is False and len(draw["uses"]) <= 4096,
        label,
    )
