"""Orchestrate the selected physical family without owning evidence transport."""

import subprocess

from sf2tool.h3.rng import _rng_step
from sf2tool.remake_h4.physical_checks import PhysicalChecks, absent, merge, number
from sf2tool.remake_h4.physical_evidence import select_evidence
from sf2tool.remake_h4.physical_occurrences import compare_occurrences
from sf2tool.remake_h4.physical_source import source_operands
from sf2tool.remake_h4_reference import UPSTREAM


def physical_consumer_binding(actual, context, source_root):
    """Bind the selected physical census to source rules and persistent effects."""
    result = dict(
        value=None,
        checks=[],
        occurrences=[],
        sourceRules=dict(
            upstream=UPSTREAM,
            owner="docs/design/contracts/combat-resolution.md",
            construction="docs/design/contracts/battle-action-construction.md",
            rng="docs/design/contracts/randomness.md",
            binding="source initial party/battle01 placements, matched operands and scene effects",
        ),
        unknown=[
            "whole historical A RNG/AI and turn-generation trajectory",
            "unreached physical branches and complete rendering/hardware timing",
        ],
    )
    context = context or {}
    session = context.get("sessionId")
    checks = PhysicalChecks(session)
    result["checks"] = checks.rows
    check, eq = checks.check, checks.eq
    selected, events, event_rows = select_evidence(actual, context, checks)
    warps = selected["warpRecords"]
    for event in events.values():
        if event.get("Kind") not in {
            "rng-" + kind
            for kind in ("dodge", "critical", "spread-1", "spread-2", "double", "counter")
        }:
            continue
        values = [event.get(k) for k in ("Before", "After", "RandomRange", "RandomValue")]
        if not all(number(v) for v in values):
            check("physical RNG arithmetic operands", None)
            continue
        before_seed, after_seed, bound, value = map(int, values)
        check(
            "physical RNG operand bounds",
            0 <= before_seed <= 0xFFFFFFFF
            and 0 <= after_seed <= 0xFFFFFFFF
            and 0 < bound <= 65535
            and 0 <= value < bound,
        )
        word, random = _rng_step(before_seed >> 16, (bound * 2) & 65535)
        eq(
            "physical RNG arithmetic",
            dict(After=(word << 16) | (before_seed & 65535), RandomValue=random >> 1),
            event,
        )

    source = None
    try:
        item_ids = {
            int(word) & 127
            for row in warps.values()
            for actor in (row.get("state") or {}).get("actors") or []
            for word in actor.get("items") or []
            if int(word) & 128
        }
        source = source_operands(source_root, item_ids) if source_root else None
        check("pinned independent source operands", True if source else None)
    except (
        OSError,
        ValueError,
        KeyError,
        TypeError,
        IndexError,
        subprocess.SubprocessError,
    ) as error:
        check(
            "pinned independent source operands",
            False
            if str(error) in ("physical source pin", "physical source modifications")
            else None,
        )
        result["unknown"].append(str(error))
    initial = (selected["samples"].get(context.get("initialSampleIndex")) or {}).get("state") or {}
    eq("initial deployment session", session, initial.get("sessionId", absent))
    eq(
        "accepted source-initial party declaration",
        context.get("profileDeclaration", absent),
        (initial.get("initializationPolicy") or {}).get("Declaration", absent),
    )
    eq(
        "source-initial party profile",
        "private-local-map3-r1-party-v1",
        context.get("profileDeclaration", absent),
    )
    if source:
        initial_actors = {a.get("id"): a for a in initial.get("actors") or []}
        check(
            "source deployment roster",
            set(initial_actors) == set(source["profiles"]) if initial_actors else None,
        )
        for actor, profile in source["profiles"].items():
            row = initial_actors.get(actor, {})
            eq(
                "source deployment " + actor,
                dict(x=profile["placement"][0], y=profile["placement"][1]),
                row,
            )
            eq(
                "source mover " + actor,
                profile["mover"].replace("_", "").lower(),
                str(row["mover"]).lower() if "mover" in row else absent,
            )

    result["rejectedRangeAttempts"] = []
    for index, row in warps.items():
        if not (row.get("result") or {}).get("failure"):
            continue
        state = row.get("state") or {}
        result["rejectedRangeAttempts"].append(dict(index=index, failure=row["result"]["failure"]))
        try:
            actors = {a["id"]: a for a in state["actors"]}
            actor, target = actors[state["actor"]], actors[state["candidate"]]
            equipped = [int(w) & 127 for w in actor["items"] if int(w) & 128]
            limits = source["items"][equipped[0]]["range"] if equipped else (1, 1)
            distance = abs(state["previewX"] - target["x"]) + abs(state["previewY"] - target["y"])
            check("rejected target is outside source range", not limits[0] <= distance <= limits[1])
        except (KeyError, TypeError, IndexError):
            check("rejected target range operands", None)

    census = context.get("census") or []
    check("independent complete physical census", bool(census) or None)
    prepares = {e.get("sequence") for e in census}
    actual_prepares = {
        seq
        for seq, e in events.items()
        if e.get("Kind") == "scene-prepared"
        and any(
            x.get("Kind") in ("rng-dodge", "rng-critical")
            for x in (warps[event_rows[seq]].get("result") or {}).get("observations", [])
        )
    }
    check("physical preparation census has no extras", actual_prepares <= prepares)
    check(
        "physical preparation census coverage",
        True if actual_prepares == prepares and prepares else None,
    )
    eq("physical census unique scenes", len(census), len(prepares))
    for boundary in context.get("battleBounds") or []:
        row = warps.get(boundary.get("index"), {})
        for expected in boundary.get("events") or []:
            eq(
                "battle admission/outcome boundary",
                expected,
                next(
                    (
                        e
                        for e in (row.get("result") or {}).get("observations", [])
                        if e.get("Sequence") == expected.get("Sequence")
                    ),
                    absent,
                ),
            )
    kinds = {
        e.get("Kind")
        for boundary in context.get("battleBounds") or []
        for e in boundary.get("events") or []
    }
    check(
        "battle has initialization and terminal boundary",
        {"battle-initialized", "battle-outcome"} <= kinds if kinds else None,
    )

    occurrences, unknown, covered = compare_occurrences(
        selected, events, event_rows, census, source, checks
    )
    result["occurrences"].extend(occurrences)
    result["unknown"].extend(unknown)
    result["fieldDeathProjectionDiagnostics"] = 0
    for projection in selected["sceneObservations"].values():
        scene = projection.get("scene") or {}
        if (
            scene.get("phase") in ("FieldSpin", "FieldExit", "FieldSettle")
            and scene.get("visible") is False
        ):
            result["fieldDeathProjectionDiagnostics"] += 1
            continue
        token = scene.get("waitToken")
        check(
            "selected scene belongs to physical census",
            any(
                number(token)
                and number(o.get("sequence"))
                and number((o.get("end") or {}).get("sequence"))
                and o["sequence"] <= token < o["end"]["sequence"]
                for o in census
            )
            if census
            else None,
        )
    actual_physical = {
        k
        for k, e in events.items()
        if e.get("Kind") in ("physical-first", "physical-second", "physical-counter")
    }
    check("all physical effects belong to census", actual_physical <= covered)
    result["value"] = merge([c["value"] for c in result["checks"]])
    return result
