"""Join portrait lifecycle effects and actual input/Draw resource consumption."""

from sf2tool.remake_h4.field_values import _field_equal


def compare_lifecycle(before, after, events, records, results, index, check):
    first_work, last_work = before["portraitWork"], after["portraitWork"]
    opening = any(e["Kind"] == "portrait-window-moving" and e["Detail"] == "open" for e in events)
    closing = any(e["Kind"] == "portrait-window-moving" and e["Detail"] == "close" for e in events)
    registered = any(e["Kind"] == "portrait-service-registered" for e in events)
    closed = any(e["Kind"] == "portrait-closed" for e in events)
    if opening:
        check(
            "source portrait initialization",
            first_work is None
            and last_work is not None
            and all(
                _field_equal(v, last_work[k])
                for k, v in dict(
                    Registered=False,
                    Closing=False,
                    Blink=20,
                    Mouth=6,
                    EyesClosed=False,
                    MouthOpen=False,
                ).items()
            ),
            index,
        )
    elif first_work is not None and last_work is not None:
        check(
            "ordered registration/closure gate",
            last_work["Registered"]
            is (False if closing else True if registered else first_work["Registered"])
            and last_work["Closing"] is (True if closing else first_work["Closing"]),
            index,
        )
    elif first_work is not None:
        check(
            "source closed window boundary",
            closed
            and first_work["Closing"] is True
            and first_work["Registered"] is False
            and after["portraitWindow"] == "ClosedPortraitWindow",
            index,
        )
    else:
        check("no unannounced portrait admission", last_work is None, index)
    if first_work is not None and (
        last_work is None or any(first_work[k] != last_work[k] for k in ("EyesClosed", "MouthOpen"))
    ):
        next_result = next((i for i, _ in results if i > index), len(records))
        drawn = [r["state"] for r in records[index + 1 : next_result] if r["kind"] == "draw"]
        check(
            "changed portrait consumed before next result",
            True
            if any(
                all(
                    s[k] == after[k]
                    for k in (
                        "sessionId",
                        "revision",
                        "observationSequence",
                        "simulationTick",
                        "token",
                        "portraitWork",
                    )
                )
                for s in drawn
            )
            else None,
            index,
        )


def compare_projection(records, results, session, profile, coverage, check):
    for index, row in enumerate(records):
        if row["kind"] == "input":
            state = row["before"]
            check("input belongs to admitted session", state["sessionId"] == session, index)
            earlier = [x["after"] for i, x in results if i < index]
            check(
                "input joins actual preceding result",
                bool(earlier)
                and all(
                    state[k] == earlier[-1][k]
                    for k in (
                        "revision",
                        "observationSequence",
                        "simulationTick",
                        "token",
                        "wait",
                        "mainSeed",
                    )
                ),
                index,
            )
        if row["kind"] != "draw":
            continue
        try:
            s = row["state"]
            work = s["portraitWork"]
            resource = s["portraitResourceProjection"]
            check("actual draw error-free", s["failure"] is None, index)
            earlier = [x["after"] for i, x in results if i < index]
            check(
                "draw joins actual preceding result",
                bool(earlier)
                and all(
                    s[k] == earlier[-1][k]
                    for k in (
                        "sessionId",
                        "revision",
                        "observationSequence",
                        "simulationTick",
                        "token",
                        "portraitWork",
                    )
                ),
                index,
            )
            if work is None:
                check("closed actual portrait resource", resource is None, index)
                coverage["closed-draw"] += 1
                continue
            projection = s["portraitProjection"]
            tiles = list(range(64))
            portrait_asset = next(p for p in profile["world"]["portraits"] if p["portrait"] == 7)
            for changes in (
                portrait_asset["eyes"] if work["EyesClosed"] else [],
                portrait_asset["mouth"] if work["MouthOpen"] else [],
            ):
                for x, y, alternate_x, alternate_y in changes:
                    tiles[y * 8 + x] = alternate_y * 8 + alternate_x
            check(
                "actual portrait draw identity",
                all(
                    resource[k] == s[k]
                    for k in (
                        "sessionId",
                        "revision",
                        "observationSequence",
                        "simulationTick",
                        "token",
                    )
                )
                and s["sessionId"] == session,
                index,
            )
            check(
                "source mapped tile projection",
                projection["id"] == 7
                and projection["flags"] == 0
                and projection["eyesClosed"] == work["EyesClosed"]
                and projection["mouthOpen"] == work["MouthOpen"]
                and projection["tiles"] == tiles
                and resource["selector"]
                == dict(
                    kind="portrait",
                    portrait=7,
                    mirror=False,
                    eyes=work["EyesClosed"],
                    mouth=work["MouthOpen"],
                    tiles=tiles,
                )
                and resource["texturePresent"] is True
                and bool(resource["resourceIdentity"])
                and (resource["width"], resource["height"]) == (48, 56),
                index,
            )
            coverage["eyes-closed" if work["EyesClosed"] else "eyes-open"] += 1
            coverage["mouth-open" if work["MouthOpen"] else "mouth-closed"] += 1
        except (KeyError, TypeError) as error:
            check("missing actual projection:" + str(error), None, index)
