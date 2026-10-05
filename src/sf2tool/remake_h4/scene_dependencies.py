"""Explicit bounded document loading, provenance admission and dependency unions."""

import subprocess
from functools import partial
from pathlib import Path

from sf2tool.paths import repo_path
from sf2tool.remake_h4.scene_checks import absent, number
from sf2tool.remake_h4.scene_source import source_sequences
from sf2tool.remake_h4_reference import UPSTREAM, require


def load_dependency(context, read_document, checks, key, cap=10 * 1024 * 1024):
    check = checks.check
    value = context.get(key)
    if not value:
        check(key + " available", None)
        return {}
    try:
        path = Path(value)
        path = path if path.is_absolute() else repo_path(path)
        require(path.stat().st_size <= cap, "scene compact dependency exceeds cap")
        return read_document(path)
    except (OSError, ValueError):
        check(key + " available", None)
        return {}


def admit_dependencies(context, source_root, read_document, checks):
    """Validate independent context/source and retain selected dependency tables."""
    check, eq = checks.check, checks.eq
    load = partial(load_dependency, context, read_document, checks)
    session = context.get("sessionId")
    producer = "4d1d1b05f143ed872ceca6ff258cfca2b4087d90"
    eq("scope", "retained-modern-A-battle-scene", context.get("scope", absent))
    eq("source pin", UPSTREAM, context.get("upstream", absent))
    eq("tested producer", producer, context.get("producer", absent))
    check("independent session", bool(session) or None)
    receipt = load("selection", 128 * 1024)
    eq(
        "completed selection",
        dict(sourceUnchanged=True, failure=None, producer=producer, sessionId=session),
        receipt,
    )
    reward_context = load("rewardContext", 512 * 1024)
    physical_context = load("physicalContext", 512 * 1024)
    heal_context = load("healContext", 512 * 1024)
    census = reward_context.get("scenes", [])
    check("independent scene census", bool(census) or None)
    eq("receipt independent census", census, receipt.get("sceneCensus", absent))
    for dep in (reward_context, physical_context, heal_context):
        eq("dependency session", session, dep.get("sessionId", absent))
    for dep in (reward_context, physical_context):
        eq("dependency producer", producer, dep.get("producer", absent))
        eq("dependency source", UPSTREAM, dep.get("upstream", absent))
    materials = load("materials", 512 * 1024)
    selected_materials = load("selectedScene", 512 * 1024)
    eq(
        "accepted material metadata",
        {k: v for k, v in selected_materials.items() if k != "rasters"},
        materials,
    )
    check("accepted material selection available", bool(selected_materials) or None)
    eq(
        "accepted material source selection",
        dict(byteEqual={"battle-scenes.json": True, "field-death-provenance.json": True}),
        materials.get("sourceSelection", {}),
    )
    if not materials and selected_materials:
        materials = selected_materials
    sequences, texts = {}, {}
    try:
        sequences, texts = source_sequences(source_root, materials)
        for visual in materials.get("actors", []):
            for purpose, sequence in visual.get("sequences", {}).items():
                eq(
                    "selected animation equals pinned source",
                    sequences[(visual["side"], visual["sprite"], purpose)],
                    sequence,
                )
        for k, text in materials.get("texts", {}).items():
            eq("selected message equals pinned source", texts.get(k, absent), text, k)
    except (OSError, TypeError, subprocess.CalledProcessError):
        check("selected original source available", None)
    except (ValueError, KeyError, IndexError):
        check("selected original source integrity", False)
    # Reuse the accepted executed bodies; do not turn modern code into an original oracle.
    accepted = "4311e009a4b1eb92ff16415856b8c5638a0fc17d"
    for path in (
        "remake/game/src/Battles/BattleSceneView.cs",
        "remake/game/src/Battles/BattleSessionView.cs",
        "remake/src/Sf2.Remake.Application/Runtime/Battles/BattleSceneContinuation.cs",
        "remake/src/Sf2.Remake.Domain/Battles/Rules/BattleSceneRules.cs",
    ):
        try:
            old = subprocess.check_output(
                ["git", "-C", str(repo_path(".")), "show", accepted + ":" + path],
                text=True,
                encoding="utf-8",
            )
            check(
                "accepted executed consumer dependency",
                True if old == repo_path(path).read_text(encoding="utf-8") else None,
                path,
            )
        except (OSError, subprocess.CalledProcessError):
            check("accepted executed consumer dependency", None, path)

    return receipt, reward_context, physical_context, heal_context, materials, sequences, texts


def collect_dependencies(actual, context, read_document, checks):
    """Union fresh loaded projections; supplied candidate channels remain authoritative."""
    check, eq, clocks = checks.check, checks.eq, checks.clocks
    precedes, identity = checks.precedes, checks.identity
    load = partial(load_dependency, context, read_document, checks)
    warps, inputs, healing, events, owners = {}, {}, {}, {}, {}

    def combine(name, target, row, where):
        for k, v in row.items():
            if k not in target:
                target[k] = v
            elif isinstance(v, dict) and isinstance(target[k], dict):
                combine(name, target[k], v, where)
            elif k == "actors" and isinstance(v, list) and isinstance(target[k], list):
                # HEAL selects only allies; merge keyed partial actor projections.
                by_id = {a.get("id"): a for a in target[k]}
                for actor_row in v:
                    actor_id = actor_row.get("id")
                    if actor_id in by_id:
                        combine(name, by_id[actor_id], actor_row, where)
                    else:
                        target[k].append(actor_row)
            else:
                eq(name, target[k], v, where)

    for key in ("rewardActual", "physicalActual", "healActual", "aiActual"):
        bundle = load(key)
        for n, row in enumerate(bundle.get("warpRecords", [])):
            index = row.get("_index", n)
            envelope = row.get("result") or {}
            clocks("result", envelope, index)
            stored = warps.setdefault(index, dict(result={}, state={}))
            # Selected copies carry subsets of events; union only their exact common identities.
            combine(
                "shared result fields",
                stored["result"],
                {k: v for k, v in envelope.items() if k != "observations"},
                index,
            )
            combine(
                "shared result transport",
                stored,
                {k: v for k, v in row.items() if k not in ("result", "state")},
                index,
            )
            combine("shared actual state", stored["state"], row.get("state") or {}, index)
            previous = None
            for event in envelope.get("observations", []):
                seq = event.get("Sequence")
                check("event clocks", number(seq) and number(event.get("Revision")), index)
                if not number(seq):
                    continue
                if previous is not None:
                    check(
                        "ordered events in owning result",
                        seq > previous["Sequence"]
                        and event.get("Revision", -1) >= previous.get("Revision", 0),
                        index,
                    )
                previous = event
                precedes(
                    "event inside result",
                    dict(revision=event.get("Revision"), observationSequence=seq),
                    envelope,
                    seq,
                )
                if seq in events:
                    eq("same event payload", events[seq], event, seq)
                    if owners[seq] != index:
                        # The attach result republishes the last submit's observations.
                        eq(
                            "republished event is attach",
                            "attach",
                            envelope.get("boundary", absent),
                            seq,
                        )
                        identity(
                            "attach repeats submit identity",
                            warps[owners[seq]]["result"],
                            envelope,
                            seq,
                        )
                else:
                    events[seq], owners[seq] = event, index
        for n, row in enumerate(bundle.get("inputRecords", [])):
            index = row.get("_index", n)
            combine("shared input fields", inputs.setdefault(index, {}), row, index)
        if key == "healActual":
            healing = {
                r.get("_index", n): r for n, r in enumerate(bundle.get("sceneObservations", []))
            }
    # Retained dependencies constrain a modern caller's own channels; they cannot
    # fill a hole in the candidate. The scoped JSONL intentionally contains only scenes.
    if "warpRecords" in actual:
        current = {r.get("_index", n): r for n, r in enumerate(actual["warpRecords"])}
        for index, reference in warps.items():
            row = current.get(index)
            check("candidate selected result present", True if row is not None else None, index)
            if row is None:
                continue
            eq("candidate shared result", reference["result"], row.get("result", absent), index)
            for k in ("inputOrdinal", "inputDelivery", "projection"):
                if k in reference:
                    eq("candidate result transport", reference[k], row.get(k, absent), index)
            observed_events = {
                e.get("Sequence"): e for e in row.get("result", {}).get("observations", [])
            }
            for seq, event in events.items():
                if owners[seq] == index:
                    eq(
                        "candidate effect dependency",
                        event,
                        observed_events.get(seq, absent),
                        index,
                    )
            for seq, event in observed_events.items():
                if seq in events:
                    eq("candidate shared effect", events[seq], event, index)
                elif event.get("Kind", "").startswith(("scene-", "field-death-")):
                    check("candidate extra scene writer", False, index)
            state = row.get("state") or {}
            for k in (
                "sessionId",
                "revision",
                "observationSequence",
                "mode",
                "map",
                "stop",
                "wait",
            ):
                if k in reference["state"]:
                    eq(
                        "candidate state boundary",
                        reference["state"][k],
                        state.get(k, absent),
                        index,
                    )
            actors = {a.get("id"): a for a in state.get("actors", [])}
            for actor_row in reference["state"].get("actors", []):
                eq(
                    "candidate actor effect",
                    actor_row,
                    actors.get(actor_row.get("id"), absent),
                    index,
                )
        if "inputRecords" not in actual:
            check("candidate physical input channel", None)
    if "inputRecords" in actual:
        current = {r.get("_index", n): r for n, r in enumerate(actual["inputRecords"])}
        for index, reference in inputs.items():
            eq(
                "candidate actual input ownership",
                {k: v for k, v in reference.items() if k != "_index"},
                current.get(index, absent),
                index,
            )
    ordered = sorted(events)
    warps = dict(sorted(warps.items()))
    inputs = dict(sorted(inputs.items()))
    for (_, left), (b, right) in zip(list(warps.items()), list(warps.items())[1:], strict=False):
        precedes("result source chronology", left["result"], right["result"], b)
    for index, row in inputs.items():
        for side in ("before", "after"):
            clocks("input " + side, row.get(side) or {}, index)
        precedes("input span", row.get("before") or {}, row.get("after") or {}, index)
        check(
            "input result interval",
            number(row.get("resultStart"))
            and number(row.get("resultEnd"))
            and row["resultStart"] <= row["resultEnd"],
            index,
        )

    return warps, inputs, healing, events, owners, ordered
