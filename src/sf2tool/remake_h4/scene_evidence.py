"""Bounded scene JSONL selection and phase/projection/census joins."""

import json
from collections import defaultdict
from pathlib import Path

from sf2tool.paths import repo_path
from sf2tool.remake_h4.scene_checks import absent, number
from sf2tool.remake_h4_reference import require


def read_selection(path):
    """Read the bounded retained scene selection without reopening the whole capture."""
    path = Path(path)
    path = path if path.is_absolute() else repo_path(path)
    require(path.stat().st_size <= 8 * 1024 * 1024, "Scene selection exceeds 8MiB")
    rows = []
    with path.open(encoding="utf-8") as stream:
        for line in stream:
            require(len(line.encode("utf-8")) <= 2 * 1024 * 1024, "Scene row exceeds 2MiB")
            row = json.loads(line)
            rows.append(dict(row["record"], _index=row["index"]))
    return dict(sceneObservations=rows)


def compare_evidence(actual, receipt, census, healing, events, owners, ordered, checks):
    """Pair source events and index selected projections in original check order."""
    check, eq, clocks = checks.check, checks.eq, checks.clocks
    precedes, identity = checks.precedes, checks.identity
    starts, pairs, active = {}, [], None
    for seq in ordered:
        e = events[seq]
        if e.get("Kind") in ("scene-prepared", "scene-step-started"):
            check("one active phase", active is None, seq)
            active = seq
            starts[seq] = e
        elif e.get("Kind") == "scene-step-completed":
            check("completion has phase start", True if active is not None else None, seq)
            if active is not None:
                start = starts[active]
                phase = "Initialize" if start["Kind"] == "scene-prepared" else start.get("Detail")
                eq("completion phase", phase, e.get("Detail", absent), seq)
                eq("completion actor", start.get("Actor", absent), e.get("Actor", absent), seq)
                pairs.append(dict(token=active, phase=phase, start=start, end=e, owner=owners[seq]))
            active = None
    check("all phases complete", True if active is None else None)
    bytoken = defaultdict(list)
    previous = None
    selected = []
    for n, row in enumerate(actual.get("sceneObservations", [])):
        index = row.get("_index", n)
        if (
            number(receipt.get("firstIndex"))
            and number(receipt.get("lastIndex"))
            and not receipt["firstIndex"] <= index <= receipt["lastIndex"]
        ):
            continue
        selected.append(index)
        clocks("scene", row, index)
        check("scene host clock", number(row.get("hostUpdate")), index)
        if previous:
            check(
                "scene index and host order",
                index > previous["_index"]
                and row.get("hostUpdate", -1) >= previous.get("hostUpdate", 0),
                index,
            )
            precedes("scene clock order", previous, row, index)
        previous = dict(row, _index=index)
        s = row.get("scene") or {}
        eq("mounted scene error", None, s.get("error", absent), index)
        if index in healing:
            h = healing[index]
            identity("HEAL original index join", h, row, index)
            for k in ("inputOrdinal", "hostUpdate", "projectionStage"):
                eq("HEAL transport join", h.get(k, absent), row.get(k, absent), index)
            if "healing" in s:
                eq(
                    "HEAL cursor equality",
                    h.get("scene", {}).get("healing", absent),
                    s["healing"],
                    index,
                )
        token = s.get("waitToken")
        if token is None:
            eq("unmounted phase", None, s.get("phase", absent), index)
            eq("unmounted visibility", False, s.get("visible", absent), index)
            continue
        check("scene token exists", True if token in starts else None, index)
        bytoken[token].append(dict(row, _index=index))
    if number(receipt.get("firstIndex")) and number(receipt.get("lastIndex")):
        wanted_indices = set(range(int(receipt["firstIndex"]), int(receipt["lastIndex"]) + 1))
        check(
            "selected independent indices",
            False
            if set(selected) - wanted_indices
            else None
            if wanted_indices - set(selected)
            else True,
        )
    else:
        check("selected independent indices", None)
    actual_prepared = [e for e in events.values() if e.get("Kind") == "scene-prepared"]
    eq(
        "independent preparation coverage",
        [c["sequence"] for c in census],
        [e["Sequence"] for e in sorted(actual_prepared, key=lambda e: e["Sequence"])],
    )
    for c in census:
        for anchor, kind in ((c, "scene-prepared"), (c.get("end") or {}, "scene-ended")):
            seq = anchor.get("sequence")
            eq(
                "census event",
                dict(Kind=kind, Sequence=seq, Revision=anchor.get("revision")),
                events.get(seq, absent),
                seq,
            )
            eq("census source result", anchor.get("index", absent), owners.get(seq, absent), seq)

    return starts, pairs, bytoken, selected
