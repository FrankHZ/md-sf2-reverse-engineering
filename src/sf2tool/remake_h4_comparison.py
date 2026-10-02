"""Compare the accepted original route with actual ordinary-host H4 observations.

The default legacy profile diagnoses first control and the first STAY/next actor.
The modern profile evaluates the continuous winning route and named settings matrix;
missing required original or host bindings keep full H4 acceptance incomplete.
Use the accepted read-only reference projector first. All outputs remain private.
"""

from __future__ import annotations

import argparse
import base64
import hashlib
import json
import subprocess
import wave
from bisect import bisect_right
from collections import Counter
from pathlib import Path

from sf2tool.h3.rng import _rng_step
from sf2tool.paths import repo_path
from sf2tool.remake_asset_build import (
    ACCEPTED_UPSTREAM_REPOSITORY,
    _composite_generator_fingerprint,
)
from sf2tool.remake_assets import AssetPreflightError, inspect_asset_checkout
from sf2tool.remake_h4_reference import (
    EXTENSION_SOURCE,
    OBSERVER,
    ROM,
    RUNNER,
    SOURCE,
    UPSTREAM,
    location,
    require,
    rows,
)

OWNER = "docs/design/contracts/map3-battle01-continuous-scenario.md"
DIRECTIONS = {1: "Up", 2: "Down", 4: "Left", 8: "Right"}


def read(path):
    return json.loads(path.read_bytes())


def write(path, value):
    path = path.resolve()
    require(
        path.is_relative_to(repo_path("local")) and not path.exists(),
        "output must be fresh beneath this worktree's local/",
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("x", encoding="utf-8") as stream:
        json.dump(value, stream, ensure_ascii=False, indent=2)
        stream.write("\n")


def reference(path):
    result = read(path)
    require(
        (result["sourceCommit"], result["romSha256"], result["upstream"])
        == (SOURCE, ROM, UPSTREAM),
        "not the accepted original reference",
    )
    require(
        [row["segment"] for row in result["lineage"]] == list(range(68, 87)),
        "not the selected winning lineage",
    )
    extension = result.get("postVictoryInput")
    if extension is not None:
        require(
            extension["sourceCommit"] == EXTENSION_SOURCE
            and [row["segment"] for row in extension["lineage"]] == list(range(2, 21))
            and extension["before"]["position"]
            == [result["endpoint"]["state"]["x"], result["endpoint"]["state"]["y"]]
            and extension["after"]["source"]["record"].startswith("prepared-20/runtime/"),
            "not the accepted original extension binding",
        )
    return result


def make_plan(ref, evidence):
    """Decode semantic field inputs, including no-displacement facing changes.

    A movement callback is a consumed displacement, not one request or held frame.
    An idle field C starts interaction; C during pending script is not a new one.
    No wall-clock/held-frame duration is used as a remake input duration.
    """
    evidence = evidence.resolve(strict=True)
    require(
        evidence.name == "issue496" and evidence.parent.name == "local",
        "select the accepted local/issue496 evidence root",
    )
    cutoff = ref["encounter"][0]["source"]["order"]
    moves = [m for m in ref["movements"] if m["source"]["order"] < cutoff]
    requests = {q["ordinal"]: q for q in ref["requests"]}
    occupied = {m["source"]["inputOrdinal"] for m in moves}
    events = []
    for move in moves:
        request = requests[move["source"]["inputOrdinal"]]
        require(
            move["direction"] in DIRECTIONS.values()
            and request["physicalFields"][-1] == move["direction"],
            "movement has no matching cardinal controller request",
        )
        events.append(
            dict(
                action=move["direction"],
                source=move["source"],
                before=move["state"],
                boundary="field",
                kind="movement",
            )
        )
    for previous, request in zip(ref["requests"], ref["requests"][1:], strict=False):
        if request["source"]["order"] >= cutoff:
            break
        if request["role"] != "controller-schedule":
            continue
        before, after = previous["resultState"], request["resultState"]
        action = request["physicalFields"][-1]
        kind = None
        if (
            action == "C"
            and before["pendingReturns"] == 0
            and before["lastConsumerPoll"]["kind"] == "WaitForEvent-action"
        ):
            require(
                not any(before["activeConsumers"].values())
                if isinstance(before["activeConsumers"], dict)
                else not before["activeConsumers"],
                "interaction consumer still active",
            )
            action, kind = "Confirm", "interaction"
        elif (
            action in DIRECTIONS.values()
            and request["ordinal"] not in occupied
            and all(before[k] == after[k] for k in ("map", "rawX", "rawY"))
            and before["facing"] != after["facing"]
        ):
            kind = "facing"
        if kind:
            events.append(
                dict(
                    action=action,
                    kind=kind,
                    boundary="field",
                    before=before,
                    source={**request["source"], "inputOrdinal": request["ordinal"]},
                    beforeSource=previous["resultSource"],
                    afterSource=request["resultSource"],
                )
            )
    events.sort(key=lambda e: e["source"]["order"])
    for ordinal, event in enumerate(events, 1):
        event["ordinal"] = ordinal

    # The reference retains the committed decision but not the movement polls.
    # Read only that decision's accepted segment; match actual one-frame direction
    # requests to original nonneutral consumer reads, not an invented shortest path.
    decision = ref["decisions"][0]
    segment = int(decision["source"]["record"].split("/")[0].removeprefix("prepared-"))
    require(68 <= segment <= 86, "decision outside accepted lineage")
    path = evidence / f"prepared-{segment}/runtime/checkpoints.jsonl"
    inputs = []
    for line, row in rows(path):
        if row["order"] >= decision["source"]["order"]:
            break
        if (
            row["kind"] != "battle:movement-input"
            or row["facts"]["poll"]["turn"] != decision["turn"]
        ):
            continue
        value = row["facts"]["input"]
        if value not in DIRECTIONS:
            continue
        request = next(
            q
            for q in ref["requests"]
            if q["source"]["order"] < row["order"] < q["resultSource"]["order"]
        )
        require(
            request["appliedFrames"] == 1 and request["physicalFields"][-1] == DIRECTIONS[value],
            "first battle movement requires additional held-input normalization",
        )
        inputs.append(
            dict(
                action=DIRECTIONS[value],
                source={**location(segment, line, row), "inputOrdinal": request["ordinal"]},
            )
        )
    require(inputs and decision["action"] == "Stay", "bounded first-STAY binding unavailable")
    # csc textbox callbacks omit direct DisplayText callers. Use the actual shared
    # DisplayText entry for both forms, retaining its shimmed-completion limitation.
    text_limit = ref["turns"][0]["source"]["order"]
    last_segment = int(ref["turns"][0]["source"]["record"].split("/")[0].removeprefix("prepared-"))
    request_orders = [q["source"]["order"] for q in ref["requests"]]
    texts = []
    for number in range(68, last_segment + 1):
        for line, row in rows(evidence / f"prepared-{number}/runtime/checkpoints.jsonl"):
            if row["order"] >= text_limit:
                break
            if row["kind"] == "DisplayText:entry":
                index = bisect_right(request_orders, row["order"]) - 1
                texts.append(
                    dict(
                        textId=row["facts"]["target"],
                        source={
                            **location(number, line, row),
                            "inputOrdinal": ref["requests"][index]["ordinal"]
                            if index >= 0
                            else None,
                        },
                    )
                )
    return dict(
        sourceCommit=SOURCE,
        scope="natural-route-and-first-STAY-diagnostic",
        acknowledgeDialogue=True,
        steps=events,
        choices=ref["choices"],
        texts=texts,
        firstDecision=decision,
        firstDecisionInputs=inputs,
        nextDecision=ref["decisions"][1],
        acknowledgementBoundary="Original DisplayText shim: natural acknowledgements Unavailable",
    )


def source_actor(value):
    if value is None:
        return None
    family, index = value.split("-")
    return int(index) + (128 if family == "enemy" else 0)


def compare(ref, plan, actual_path, host_log, host_exit):
    require(plan["sourceCommit"] == SOURCE, "plan/reference source mismatch")
    samples, signals, terminal = [], [], None
    record_sequences = []
    for _, row in rows(actual_path):
        if not row.get("terminal"):
            record_sequences.append(row["sequence"])
        if "result" in row:
            signals.append(row)
        elif row.get("terminal"):
            terminal = row
        else:
            samples.append(row)
    require(samples and terminal, "actual run lacks admission/terminal record")
    labels = {s["label"]: s for s in samples if s["label"]}
    admission = labels["admission"]
    assertions = []

    def check(
        layer,
        name,
        expected,
        actual,
        sample,
        original,
        reason="",
        unavailable=False,
        scope="baseline",
    ):
        unavailable = unavailable or (actual is None and expected is not None)
        result = "Unavailable" if unavailable else "PASS" if expected == actual else "FAIL"
        assertions.append(
            dict(
                layer=layer,
                assertion=name,
                checkpoint=sample.get("label"),
                original=dict(
                    owner=original.get("owner", OWNER),
                    commit=original.get("commit", SOURCE),
                    record=original,
                ),
                actualSequence=sample.get("sequence"),
                inputOrdinal=sample.get("inputOrdinal"),
                logicalStep=sample.get("logicalStep"),
                expected=expected,
                actual=actual,
                result=result,
                reason=reason or ("semantic equality" if result == "PASS" else "observed mismatch"),
                scope=scope,
            )
        )

    a = admission["state"]
    original = ref["admission"]
    o = original["state"]
    player = next((e for e in a.get("entities", []) if e["id"] == "entity-0"), {})
    for key, expected, value in (
        ("map", f"map-{o['map']}", a.get("map")),
        ("player.x", o["x"] * 384, player.get("x")),
        ("player.y", o["y"] * 384, player.get("y")),
        ("player.facing", o["facing"], player.get("facing")),
        ("gold", original["accounting"]["gold"], a.get("gold")),
        ("mainSeed", int.from_bytes(bytes(o["rngBytes"]), "big"), a.get("mainSeed")),
        ("joined", original["accounting"]["joined"], (a.get("partyLists") or {}).get("Joined")),
        ("active", original["accounting"]["active"], (a.get("partyLists") or {}).get("Active")),
    ):
        check(1, key, expected, value, admission, original["source"])
    for ally in original["accounting"]["allies"][:3]:
        actual = next(
            (p for p in a.get("party", []) if p["Actor"]["Value"] == f"ally-{ally['id']}"), {}
        )
        for expected_key, actual_key in (
            ("hpCurrent", "Hp"),
            ("mpCurrent", "Mp"),
            ("statusEffects", "Status"),
        ):
            check(
                1,
                f"ally-{ally['id']}.{expected_key}",
                ally[expected_key],
                actual.get(actual_key),
                admission,
                original["source"],
                unavailable=actual_key not in actual,
            )
        loadout = actual.get("SourceLoadout")
        items = loadout.get("Items") if isinstance(loadout, dict) else None
        if loadout is None:
            item_reason = "Admission SourceLoadout is absent/null; later inventories cannot fill it"
        elif not isinstance(items, list):
            item_reason = "Admission SourceLoadout has no supported typed Items array"
        else:
            item_reason = "Compare the admission typed SourceLoadout.Items slot array"
        check(
            1,
            f"ally-{ally['id']}.items",
            [x["raw"] for x in ally["items"]],
            items if isinstance(items, list) else loadout,
            admission,
            original["source"],
            item_reason,
            not isinstance(items, list),
        )
    npc_differences = []
    for entity in a.get("entities", []):
        expected = next(
            (e for e in ref["inherited"]["entities"] if e["physical"] == entity["slot"]), None
        )
        if expected:
            for key in ("x", "y", "facing"):
                if expected[key] != entity[key]:
                    npc_differences.append(
                        dict(
                            slot=entity["slot"],
                            entity=entity["id"],
                            field=key,
                            expected=expected[key],
                            actual=entity[key],
                        )
                    )
    check(
        1,
        "relevant NPC continuation phase",
        "justified phase mapping",
        npc_differences,
        admission,
        ref["inherited"]["source"],
        "Raw admission deltas retained; relevance and phase/timing normalization remain Unknown",
        True,
    )

    for step in plan["steps"]:
        sample = labels.get(f"before:{step['ordinal']}")
        if sample is None:
            break
        state, expected = sample["state"], step["before"]
        p = next((e for e in state.get("entities", []) if e["id"] == "entity-0"), {})
        for name, e, value in (
            ("map", f"map-{expected['map']}", state.get("map")),
            ("x", expected["x"] * 384, p.get("x")),
            ("y", expected["y"] * 384, p.get("y")),
        ):
            check(2, name, e, value, sample, step.get("beforeSource", step["source"]))
        for flag, value in expected["flags"].items():
            check(
                3,
                f"flag.{flag}",
                value,
                int(flag) in state["flags"] if state.get("flags") is not None else None,
                sample,
                step.get("beforeSource", step["source"]),
                "Actual flags field absent/null" if state.get("flags") is None else "",
            )

    first = labels.get("first-control")
    if first:
        state = first["state"]
        turn = ref["turns"][0]
        for name, expected, value in (
            ("first-actor", turn["actor"], source_actor(state.get("actor"))),
            ("round", turn["round"], state.get("round")),
            (
                "turn-order",
                turn["accounting"]["turnOrder"],
                [
                    dict(actor=source_actor(x["actor"]), score=x["score"])
                    for x in state["turnOrder"]
                    if x["actor"] is not None
                ],
            ),
        ):
            check(4, name, expected, value, first, turn["source"])
        check(
            4,
            "mainSeed readback",
            int.from_bytes(bytes(turn["accounting"]["rngBytes"]), "big"),
            state["mainSeed"],
            first,
            turn["source"],
            "Different readback retained; timing-to-RNG mapping is Unknown. "
            "This does not diagnose the RNG algorithm",
            True,
        )
        for ally in turn["accounting"]["allies"][:3]:
            actor = next(x for x in state["actors"] if x["id"] == f"ally-{ally['id']}")
            for ekey, akey in (
                ("hpCurrent", "hp"),
                ("hpMax", "maxHp"),
                ("mpCurrent", "mp"),
                ("mpMax", "maxMp"),
                ("level", "level"),
                ("attack", "attack"),
                ("defense", "defense"),
                ("move", "move"),
                ("statusEffects", "status"),
                ("x", "x"),
                ("y", "y"),
            ):
                check(
                    4, f"{actor['id']}.{ekey}", ally[ekey], actor.get(akey), first, turn["source"]
                )
            for key in ("items", "spells"):
                check(
                    4,
                    f"{actor['id']}.{key}",
                    [v["raw"] for v in ally[key]],
                    actor.get(key),
                    first,
                    turn["source"],
                    "Read live natural-entry actor; no retrospective admission claim",
                )

    texts = [s for s in samples if s["label"] == "dialogue"]
    expected_texts = plan["texts"]
    for i, expected in enumerate(expected_texts):
        sample = texts[i] if i < len(texts) else samples[-1]
        check(
            3,
            f"dialogue.{i}.textId",
            expected["textId"],
            texts[i]["state"].get("textId") if i < len(texts) else None,
            sample,
            expected["source"],
            "Ordered reached text identity; not original reveal/ack equality",
            i >= len(texts),
        )
    check(
        3,
        "dialogue-count",
        len(expected_texts),
        len(texts),
        samples[-1],
        {"records": "DisplayText:entry before first dispatch"},
        "No extra or missing reached dialogue identities in the executed route",
        first is None,
    )

    for label, name, expected, keys, original_record in (
        (
            "first-destination",
            "first-decision.destination",
            plan["firstDecision"]["destination"],
            ("previewX", "previewY"),
            plan["firstDecision"]["source"],
        ),
    ):
        if label in labels:
            sample = labels[label]
            check(
                5,
                name,
                expected,
                dict(zip(("x", "y"), (sample["state"].get(k) for k in keys), strict=True)),
                sample,
                original_record,
                "Diagnostic continuation after divergent initial order",
                scope="diagnostic",
            )
    if "after-first-decision" in labels:
        sample = labels["after-first-decision"]
        check(
            5,
            "next-decision.actor",
            plan["nextDecision"]["actor"],
            source_actor(sample["state"].get("actor")),
            sample,
            plan["nextDecision"]["source"],
            "Stop on divergence; no actor chasing, reseed or substituted decision",
            scope="diagnostic",
        )
    last = samples[-1]
    check(
        2,
        "single-session",
        [a.get("sessionId")],
        sorted({s["state"].get("sessionId") for s in samples}, key=str),
        last,
        original["source"],
        "Observed session identity across the executed prefix only",
    )
    check(
        2,
        "monotonic-actual-records",
        True,
        record_sequences == list(range(1, len(record_sequences) + 1)),
        last,
        original["source"],
    )
    observed = {}
    signal_problem = None
    prior_revision = -1
    prior_sequence = 0
    for entry in signals:
        result = entry["result"]
        if result["sessionId"] != a["sessionId"] or result["revision"] < prior_revision:
            signal_problem = "session/revision discontinuity"
        prior_revision = result["revision"]
        for observation in result["observations"]:
            number = observation["Sequence"]
            if number in observed:
                if result["boundary"] != "attach" or observed[number] != observation:
                    signal_problem = "duplicate or conflicting non-attach observation"
            elif number != prior_sequence + 1:
                signal_problem = "observation sequence gap"
            else:
                prior_sequence = number
                observed[number] = observation
        if result["observationSequence"] != prior_sequence:
            signal_problem = "result watermark gap"
        if result["failure"] is not None:
            check(
                2,
                "session-result-error",
                None,
                result["failure"],
                {**entry, "label": "signal:" + result["boundary"]},
                {},
                "Actual submitted failure retained",
            )
    check(
        2,
        "every-session-result",
        None,
        signal_problem,
        last,
        original["source"],
        "Subscribed before Begin/Attach; contiguous watermarks; duplicate attaches checked"
        if signals
        else "Latest-result snapshots cannot prove absent overwritten submissions/errors",
        not signals,
    )
    if "after-first-decision" in labels:
        actor = f"ally-{plan['firstDecision']['actor']}"
        kinds = {
            o["Kind"] for o in observed.values() if (o.get("Actor") or {}).get("Value") == actor
        }
        check(
            5,
            "first-STAY-consumed",
            ["action-committed", "after-turn", "stay-selected"],
            sorted(kinds & {"action-committed", "after-turn", "stay-selected"}),
            labels["after-first-decision"],
            plan["firstDecision"]["source"],
            "Actual selection/commit/after-turn in diagnostic extension",
            not signals,
            "diagnostic",
        )
    for sample in samples:
        if sample["state"].get("failure"):
            check(
                2,
                "host-error",
                None,
                sample["state"]["failure"],
                sample,
                {},
                "Actual runtime failure",
            )
    errors = [
        line
        for line in host_log.read_text(encoding="utf-8-sig").splitlines()
        if "ERROR:" in line or "Unhandled exception" in line
    ]
    check(
        2,
        "host-process-errors",
        [],
        errors,
        last,
        {},
        "Inspect actual Godot process errors",
        scope="observation",
    )
    terminal_failure = terminal.get("failure")
    controlled_divergence = terminal_failure == "next-decision-actor-mismatch" and any(
        row["assertion"] == "next-decision.actor" and row["result"] == "FAIL" for row in assertions
    )
    check(
        2,
        "probe-terminal",
        terminal_failure if controlled_divergence else "",
        terminal_failure,
        last,
        {},
        "Controlled stop corroborated by the actual next-actor comparison"
        if controlled_divergence
        else "Explicit probe failures/timeouts are observation failures",
        scope="diagnostic" if controlled_divergence else "observation",
    )
    # A probe-reported failure intentionally exits 2. Its assertion above still fails
    # unless the controlled divergence is corroborated. Any other nonzero exit,
    # including a crash after a complete terminal record, is a process failure.
    expected_exit = 2 if terminal_failure else 0
    check(
        2,
        "host-process-exit",
        expected_exit,
        host_exit,
        last,
        {},
        "Actual completed process exit; terminal/log success cannot hide shutdown failure",
        scope="observation",
    )
    for group in ref["coverage"]:
        if "RA-12" in group["fields"]:
            extension = ref.get("postVictoryInput")
            after = extension["after"] if extension else None
            expected = None
            if after:
                state = after["state"]
                accounts = after["accounting"]
                expected = {
                    "input": extension["input"]["direction"],
                    "map": state["map"],
                    "from": extension["before"]["position"],
                    "to": [state["x"], state["y"]],
                    "facing": state["facing"],
                    "flags": state["flags"],
                    "party": accounts["party"],
                    "gold": accounts["gold"],
                    "allies": accounts["allies"],
                    "rngBytes": state["rngBytes"],
                    "rngCopyByte": state["rngCopyByte"],
                    "readiness": after["readiness"],
                }
            check(
                7,
                group["fields"],
                expected,
                None,
                last,
                {
                    "owner": (
                        "docs/research/map3-messenger-acceptance.md"
                        "#native-post-victory-ordinary-input-result-issue-515"
                    ),
                    "commit": EXTENSION_SOURCE,
                    "record": after["source"] if after else None,
                },
                "Accepted original extension projected; actual run did not reach this boundary"
                if after
                else (
                    "Accepted original extension was not supplied to this projection; "
                    "actual run did not reach this boundary"
                ),
                True,
            )
            continue
        check(
            group["layer"],
            "remaining:" + group["fields"],
            group["binding"],
            None,
            last,
            {"binding": group["binding"]},
            f"Partial group; reference status: {group['referenceStatus']}",
            True,
        )
    for deviation in (
        "1A controlled construction",
        "2A mandatory route",
        "4A manual agency/no reseeding",
        "6A absent save/restart",
        "9A keyboard/gamepad/remap/swap/flash/text variants",
        "10A out-of-domain safety",
    ):
        check(
            10,
            deviation,
            "complete named acceptance",
            None,
            last,
            {},
            "Executed prefix cannot certify the whole deviation or unexecuted variants",
            True,
        )
    counts = Counter(x["result"] for x in assertions)
    failures = [x for x in assertions if x["result"] == "FAIL"]
    return dict(
        sourceCommit=SOURCE,
        scope=plan["scope"],
        assertions=assertions,
        counts=dict(counts),
        earliestDivergence=min(failures, key=lambda x: x["actualSequence"]) if failures else None,
        result="FAIL" if failures else "Unavailable",
        milestonePass=False,
        stop=terminal,
        hostExit=host_exit,
        remainder="Battle/return/endpoint and 9A variants not established; "
        "diagnostic extension stops at actor divergence",
    )


def semantic_value(value):
    """Remove transport counters, retaining every logical observation operand."""
    if isinstance(value, dict):
        return {k: v for k, v in value.items() if k not in ("Revision", "Sequence")}
    return value


def endpoint_state(state):
    player = next((e for e in state.get("entities", []) if e["id"] == "entity-0"), {})
    camera = state.get("cameraProjection") or {}
    return {
        "player": {k: player.get(k) for k in ("x", "y", "facing", "moving", "busy")},
        "camera": {k: camera.get(k) for k in ("x", "y", "targetSlot", "bound")},
        **{
            k: state.get(k)
            for k in (
                "map",
                "canWaitAtInput",
                "wait",
                "cursor",
                "callers",
                "callerReturning",
                "warp",
                "battleMounted",
                "choice",
                "tickDebt",
            )
        },
    }


# This accepted candidate predates field-death additions. Fingerprint-v1 hashes
# checkout bytes, so the recorded Windows CRLF identity differs from Git LF blobs.
SCENE_SOURCE_SHA256 = "A2C7E64C23A858DC7E8EDC890C0279DB4B04F285A2940CBA14C9571055D30DCB"
SCENE_MANIFEST_SHA256 = "166425DC10924FCF47A761CC6D0FC92E315F1C7FAC404DD092A25CD4A559FCE8"
SCENE_GENERATOR_COMMIT = "bfcb819fe61cf6b7f2a3a45822f6de51e411e559"
SCENE_GENERATOR_COMPONENTS = (
    "src/sf2tool/remake_battle_scene_content.py",
    "src/sf2tool/remake_asset_build.py",
    "src/sf2tool/texture_extract.py",
    "src/sf2tool/compression.py",
)


def reached_materials(actual, selection):
    """Offline material origin only; natural dispatch/consumer joins stay separate."""
    result = dict(scene=None, audio=None, actorWeapon=None, checks=[], joins=[])
    if not selection:
        return result

    def check(name, value, source):
        result["checks"].append(dict(name=name, value=value, source=source))
        return value

    def verdict(values):
        return False if False in values else None if None in values else True

    def relative_file(root, name):
        path = (root / name).resolve()
        require(path.is_relative_to(root), "material file escapes explicit selected root")
        return path

    try:
        world_path, scene_path, process_path, scene_root, asset_root, commit, tree, manifest_pin = (
            selection
        )
        world_path, scene_path, process_path, scene_root, asset_root = (
            p.resolve() if p.is_absolute() else repo_path(p)
            for p in (world_path, scene_path, process_path, scene_root, asset_root)
        )
        process = read(process_path)
        selected = process.get("selectedInputs", {})
        binding = (
            all(
                repo_path(selected[key]).resolve() == path
                for key, path in (
                    ("SF2_PRIVATE_EXPLORATION_CONTENT", world_path),
                    ("SF2_PRIVATE_BATTLE_SCENE_CONTENT", scene_path),
                )
            )
            if all(
                selected.get(key)
                for key in ("SF2_PRIVATE_EXPLORATION_CONTENT", "SF2_PRIVATE_BATTLE_SCENE_CONTENT")
            )
            else None
        )
        check("same-run explicit world/scene selection", binding, "process.selectedInputs")
        world, scene = read(world_path), read(scene_path)
        world_identity = check(
            "selected world original identity",
            world["provenance"]["commit"] == UPSTREAM
            and world["provenance"]["romSha256"] == ROM
            and world["provenance"]["repository"] == ACCEPTED_UPSTREAM_REPOSITORY,
            "world.provenance",
        )
        scene_manifest = read(scene_root / "manifests/presentation-assets-v1.json")
        source_path = scene_root / "source/battle-scenes/selection.json"
        source, report = read(source_path), read(scene_root / "candidate-report.json")
        base = read(scene_root / "battle-scenes.json")

        def digest(data):
            return hashlib.sha256(data).hexdigest().upper()

        historical = {
            name: subprocess.check_output(
                ["git", "show", f"{SCENE_GENERATOR_COMMIT}:{name}"], cwd=repo_path("")
            )
            for name in SCENE_GENERATOR_COMPONENTS
        }
        fingerprints = dict(
            gitLf=_composite_generator_fingerprint(historical),
            historicalCrlf=_composite_generator_fingerprint(
                {name: data.replace(b"\n", b"\r\n") for name, data in historical.items()}
            ),
            current=_composite_generator_fingerprint(
                {name: repo_path(name).read_bytes() for name in SCENE_GENERATOR_COMPONENTS}
            ),
        )
        scene_checks = [binding, world_identity]
        scene_checks.append(
            check(
                "scene bundle original pins and recorded file identities",
                source["upstreamCommit"] == report["upstreamCommit"] == UPSTREAM
                and source["romSha256"] == report["romSha256"] == ROM
                and source["upstreamRepository"] == ACCEPTED_UPSTREAM_REPOSITORY
                and digest(source_path.read_bytes())
                == report["sourceSha256"]
                == SCENE_SOURCE_SHA256
                and digest((scene_root / "manifests/presentation-assets-v1.json").read_bytes())
                == report["manifestSha256"]
                == SCENE_MANIFEST_SHA256
                and (scene_root / "battle-scenes.json").stat().st_size
                == report["sceneContentBytes"]
                and len(scene_manifest["assets"]) == report["assetCount"] == 42,
                "scene-source candidate-report/manifest/source selection",
            )
        )
        scene_checks.append(
            check(
                "recorded historical scene extractor fingerprint",
                fingerprints["historicalCrlf"] == report["generatorArtifactSha256"],
                dict(
                    commit=SCENE_GENERATOR_COMMIT,
                    components=SCENE_GENERATOR_COMPONENTS,
                    representation="historical CRLF checkout",
                    fingerprints=fingerprints,
                ),
            )
        )
        scene_checks.append(
            check(
                "selected scene base content equality",
                all(scene.get(k) == v for k, v in base.items() if k != "rasters")
                and all(scene["rasters"].get(k) == v for k, v in base["rasters"].items())
                and all(
                    len(base64.b64decode(span["data"], validate=True)) == span["byteLength"]
                    for span in source["spans"]
                ),
                "selected scene -> frozen base42 and original ROM spans",
            )
        )
        assets = {a["assetId"]: a for a in scene_manifest["assets"]}
        raster_valid = {}
        for name, raster in base["rasters"].items():
            asset = assets.get("battle.scene." + name.replace("/", "."))
            bucket = [b for b in asset["buckets"] if b["scale"] == 2] if asset else []
            payload = base64.b64decode(raster["data"], validate=True)
            raster_valid[name] = bool(
                asset
                and len(bucket) == 1
                and asset["source"]
                == dict(assetId="source.battle.scene.selection", sha256=report["sourceSha256"])
                and asset["derivation"]["generatorArtifactSha256"]
                == report["generatorArtifactSha256"]
                and digest(payload) == raster["sha256"] == bucket[0]["sha256"]
                and len(payload) == bucket[0]["byteLength"]
                and (raster["width"], raster["height"]) == (bucket[0]["width"], bucket[0]["height"])
            )
        scene_checks.append(
            check(
                "base42 embedded PNG/manifest identities",
                len(raster_valid) == 42 and all(raster_valid.values()),
                "scene rasters -> scale2 buckets/source/derivation",
            )
        )
        mounted = [
            (i, r["scene"])
            for i, r in enumerate(actual.get("sceneObservations", []))
            if r["scene"].get("visible") and not r["scene"].get("fieldDeath")
        ]
        backgrounds, actors = [], []
        for i, row in mounted:
            for key in ("background", "backgroundWrap", "ground"):
                node = row.get(key, {})
                if node.get("visible"):
                    name = node.get("resource")
                    backgrounds.append(bool(node.get("texturePresent") and raster_valid.get(name)))
                    result["joins"].append(
                        dict(
                            record=f"sceneObservations[{i}].scene.{key}",
                            resource=name,
                            source="scene-source/base42",
                        )
                    )
            for key in ("allyResource", "enemyResource", "weaponResource"):
                if key == "enemyResource" and not row.get("enemyVisible"):
                    continue
                if key == "weaponResource" and not row.get("weaponVisible"):
                    continue
                if name := row.get(key):
                    actors.append(bool(raster_valid.get(name)))
                    result["joins"].append(
                        dict(
                            record=f"sceneObservations[{i}].scene.{key}",
                            resource=name,
                            source="scene-source/base42",
                        )
                    )
        result["scene"] = verdict(scene_checks + backgrounds) if backgrounds else None
        result["actorWeapon"] = verdict(scene_checks + actors) if actors else None

        inspection = inspect_asset_checkout(
            str(asset_root),
            expected_commit=commit,
            expected_tree=tree,
            expected_manifest_sha256=manifest_pin,
        )
        catalog = json.loads(inspection.manifest_bytes)
        audio_checks = [binding, world_identity]
        audio_checks.append(
            check(
                "explicit pinned clean asset checkout",
                True,
                dict(commit=commit, tree=tree, manifestSha256=manifest_pin),
            )
        )
        provenance = []
        for name, key in (
            ("audio-reached-inventory-provenance.json", "records"),
            ("audio-town-join-provenance.json", "assets"),
        ):
            owner = read(asset_root / "manifests" / name)
            audio_checks.append(
                check(
                    "audio source pins " + name,
                    owner["romSha256"] == ROM
                    and owner["sf2disasmCommit"] == UPSTREAM
                    and owner["emulator"]
                    == dict(
                        name="BizHawk",
                        version="2.11.1",
                        core="Genplus-gx",
                        commit="bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5",
                    ),
                    "manifests/" + name,
                )
            )
            provenance.extend((name, i, r) for i, r in enumerate(owner[key]))
        audio = world["world"]["presentation"]["audio"]
        starts = [
            (i, r["receipt"])
            for i, r in enumerate(actual.get("audioReceipts", []))
            if r["receipt"]["Operation"] == "started"
        ]
        for cue in sorted({r["Cue"] for _, r in starts}):
            selected_audio = [a for a in audio if a["cue"] == cue]
            library = [a for a in catalog["assets"] if a["kind"] == "audio" and a["cue"] == cue]
            records = [(n, i, r) for n, i, r in provenance if r["asset"]["cue"] == cue]
            unique = len(selected_audio) == len(library) == len(records) == 1
            available = bool(selected_audio and library and records)
            audio_checks.append(
                check(
                    "unique reached audio origin " + cue,
                    unique if available else None,
                    "world audio -> library catalog -> provenance record",
                )
            )
            if not unique:
                continue
            selected_audio, asset = selected_audio[0], library[0]
            owner_name, owner_index, record = records[0]
            runtime = asset["runtime"]
            with wave.open(str(relative_file(asset_root, runtime["runtimePath"])), "rb") as wav:
                pcm = wav.readframes(wav.getnframes())
                format_equal = (
                    wav.getsampwidth(),
                    wav.getnchannels(),
                    wav.getframerate(),
                    wav.getnframes(),
                ) == (2, runtime["channels"], runtime["sampleRate"], runtime["sampleFrames"])
            capture_path = relative_file(asset_root, record["sourcePath"])
            with wave.open(str(capture_path), "rb") as capture:
                begin, end = record["captureStartSample"], record["captureEndSample"]
                capture.setpos(begin)
                cut_equal = capture.readframes(end - begin) == pcm and (
                    capture.getsampwidth(),
                    capture.getnchannels(),
                    capture.getframerate(),
                ) == (2, runtime["channels"], runtime["sampleRate"])
            valid = (
                record["asset"] == asset
                and format_equal
                and cut_equal
                and digest(capture_path.read_bytes()) == asset["source"]["sha256"]
                and end - begin == runtime["sampleFrames"]
                and base64.b64decode(selected_audio["pcm16"], validate=True) == pcm
                and digest(pcm) == selected_audio["sha256"]
                and all(
                    selected_audio[k] == runtime[k]
                    for k in ("sampleRate", "channels", "sampleFrames", "loopBegin", "loopEnd")
                )
                and (selected_audio["command"], selected_audio["timerB"])
                == (asset["command"], asset["timerB"])
            )
            audio_checks.append(
                check(
                    "reached original capture cut/runtime PCM " + cue,
                    valid,
                    dict(
                        file="manifests/" + owner_name,
                        record=owner_index,
                        assetId=asset["assetId"],
                        captureStartSample=begin,
                        captureEndSample=end,
                        loopBegin=runtime["loopBegin"],
                        loopEnd=runtime["loopEnd"],
                    ),
                )
            )
            for i, receipt in ((i, r) for i, r in starts if r["Cue"] == cue):
                requested = receipt.get("RequestedTimerB")
                choices = [a for a in audio if a["command"] == receipt["Command"]]
                exact = [a for a in choices if a["timerB"] == requested]
                finite = [a for a in choices if a["loopBegin"] is None]
                policy = requested is None or (
                    len(exact) == 1
                    and exact[0] == selected_audio
                    or not exact
                    and len(finite) == 1
                    and finite[0] == selected_audio
                )
                valid = policy and all(
                    receipt[a] == selected_audio[b]
                    for a, b in (
                        ("Command", "command"),
                        ("TimerB", "timerB"),
                        ("PcmSha256", "sha256"),
                        ("SampleRate", "sampleRate"),
                        ("Channels", "channels"),
                        ("SampleFrames", "sampleFrames"),
                        ("LoopBegin", "loopBegin"),
                        ("LoopEnd", "loopEnd"),
                    )
                )
                audio_checks.append(
                    check(
                        f"audio start material selection receipt{i}",
                        valid,
                        "SessionAudio.Select exact timer/unique finite policy",
                    )
                )
                result["joins"].append(
                    dict(
                        record=f"audioReceipts[{i}].receipt",
                        cue=cue,
                        assetId=asset["assetId"],
                        requestedTimerB=requested,
                        assetTimerB=asset["timerB"],
                        provenance=owner_name,
                        sourceRecord=owner_index,
                        selection="exact"
                        if requested == asset["timerB"]
                        else "named cue"
                        if requested is None
                        else "unique finite fallback",
                    )
                )
        result["audio"] = verdict(audio_checks) if starts else None
    except (FileNotFoundError, subprocess.CalledProcessError):
        check("selected material input availability", None, "explicit selected inputs")
    except AssetPreflightError as error:
        # The existing validator distinguishes absence from an observed drift.
        unavailable = error.code in (
            "RepositoryUnavailable",
            "PayloadUnavailable",
            "SchemaUnavailable",
        )
        check("asset checkout " + error.code, None if unavailable else False, error.field)
        result["audio"] = None if unavailable else False
    except KeyError:
        check("selected material field availability", None, "explicit selected inputs")
    except (ValueError, wave.Error) as error:
        check("selected material evidence shape", False, type(error).__name__)
    return result


def plain_join_binding(ref, actual, evidence_root, world_path):
    """Bind only the accepted JOIN occurrence; indices locate evidence, not legality.

    The four selected files reuse the accepted segment seals. This is an offline
    consumer readback, not resumability validation or original music completion.
    Missing named evidence is unavailable; a present contradiction is false.
    """
    result = dict(original=None, plain=None, audio=None, caller=None, anchors={})
    plain_value = None

    def finalize():
        # Every exit preserves observed contradictions without closing partial evidence.
        values = [plain_value, result["audio"], result["caller"]]
        result["plain"] = False if False in values else None if None in values else True
        return result

    if evidence_root is None or world_path is None:
        return finalize()
    evidence_root, world_path = (
        p.resolve() if p.is_absolute() else repo_path(p) for p in (evidence_root, world_path)
    )
    require(
        evidence_root.is_relative_to(repo_path("local")),
        "JOIN evidence must be selected beneath this worktree's local/",
    )
    paths = [
        evidence_root / name
        for name in (
            "candidate.json",
            "runtime/segment-pair.json",
            "runtime/checkpoints.jsonl",
            "runtime/actual-inputs.jsonl",
        )
    ]
    if not all(p.is_file() for p in paths):
        return finalize()

    def digest(path):
        return hashlib.sha256(path.read_bytes()).hexdigest().upper()

    try:
        candidate, pair = read(paths[0]), read(paths[1])
        sealed = (
            digest(paths[1]) == ref["lineage"][0]["pairSha256"]
            and digest(paths[0]) == pair["material"]
            and all(digest(p) == pair["files"][p.name] for p in paths[2:])
            and [
                candidate[k]
                for k in ("RomSha256", "SourceCommit", "ObserverSha256", "RunnerSha256")
            ]
            == [ROM, UPSTREAM, OBSERVER, RUNNER]
        )
        result["original"] = sealed
        if not sealed:
            result.update(plain=False, audio=False, caller=False)
            return finalize()
        result["original"] = None
        checkpoints = [row for _, row in rows(paths[2])]
        inputs = [row for _, row in rows(paths[3])]
        # One-based locations from the sealed prepared68 witness, selected by the
        # accepted reference operation pair, not a new reference inferred from host output.
        op = ref["operationPairs"][83]
        entry, accepted, close, returned, script, ready = (
            checkpoints[i - 1] for i in (2608, 2609, 2610, 2612, 2621, 2624)
        )
        applying, frame = inputs[17233], inputs[17234]
        original_plain = (
            checkpoints[2572]["kind"] == "audio:request"
            and checkpoints[2572]["facts"]["command"] == 19
            and checkpoints[2573]["kind"] == "DisplayText:entry"
            and checkpoints[2573]["facts"]["target"] == 447
            and checkpoints[2602]["kind"] == "DisplayText:return"
            and checkpoints[2602]["facts"]["target"] == 447
            and [checkpoints[i]["facts"]["command"] for i in (2603, 2604)] == [240, 251]
            and checkpoints[2605]["kind"] == "audio:consumer-dispatch"
            and checkpoints[2606]["kind"] == "audio:mailbox-written"
            and checkpoints[2605]["facts"]["command"] == checkpoints[2606]["facts"]["command"] == 8
            and checkpoints[2606]["order"] < entry["order"]
            and entry["kind"] == "WaitForPlayerInput:entry"
            and accepted["kind"] == "WaitForPlayerInput:return"
            and entry["facts"]["target"] == accepted["facts"]["target"] == 0x1576
            and entry["state"]["input"] == 0
            and accepted["state"]["input"] == 32
            and accepted["facts"]["d0"] == 3
            and applying["kind"] == "applying"
            and frame["kind"] == "frame"
            and applying["button"] == frame["button"] == "C"
            and applying["id"] == frame["id"]
            and applying["order"] < accepted["order"] < frame["order"]
            and close["kind"] == "CloseDialogueWindow:entry"
            and returned["facts"]["operation"] == dict(opcode=8, pc=0x51630)
            and returned["order"] == op["return"]["order"]
            and script["kind"] == "script:return"
            and script["facts"]["target"] == 0x5149A
            and entry["order"]
            < accepted["order"]
            < close["order"]
            < returned["order"]
            < script["order"]
            < ready["order"]
            and not any(c["state"]["flags"]["603"] for c in (entry, accepted, returned, script))
            and ready["state"]["flags"]["603"]
            and ready["state"]["pendingReturns"] == 0
        )
        result["original"] = original_plain
        result["anchors"]["original"] = dict(
            segment=68,
            checkpoints=[2608, 2609, 2610, 2612, 2621, 2624],
            actualInputs=[17234, 17235],
            upstream=UPSTREAM,
            helperReturnOrder="Inferred",
            originalMusicCompletion="Unknown",
        )
        if not original_plain:
            result.update(plain=False, audio=False, caller=False)
            return finalize()
    except json.JSONDecodeError:
        result.update(original=False, plain=False, audio=False, caller=False)
        return finalize()
    except (KeyError, IndexError):
        return finalize()

    try:
        labels = (
            "music-logical-end",
            "music-plain-input",
            "music-plain-poll",
            "music-plain-accepted",
            "join-field-return",
        )
        selected = [
            [(i, s["state"]) for i, s in enumerate(actual["samples"]) if s["label"] == label]
            for label in labels
        ]
        if any(not group for group in selected):
            return finalize()
        if any(len(group) != 1 for group in selected):
            result.update(plain=False, audio=False, caller=False)
            return finalize()
        (li, logical), (pi, plain), (wi, polled), (ai, acked), (ri, ready) = (
            group[0] for group in selected
        )
        records = actual["warpRecords"]
        observations = [(i, o) for i, r in enumerate(records) for o in r["result"]["observations"]]

        def event(kind, detail=None):
            found = [
                (i, o)
                for i, o in observations
                if o["Kind"] == kind
                and (detail is None or o["Detail"] == detail)
                and logical["revision"] < o["Sequence"] <= ready["revision"]
            ]
            if not found:
                raise KeyError(kind)
            return found

        completed = event("music-actual-completed", "MUSIC_JOIN")
        released = event("music-wait-returned", "MUSIC_JOIN")
        previous = event("presentation-completed", "PreviousMusic")
        acknowledged = event("presentation-acknowledged")
        pressed = [r for r in actual["inputRecords"] if r["pressed"]]
        early = [
            r
            for r in pressed
            if r["before"]["revision"] == logical["revision"]
            and r["before"]["token"] == logical["token"]
        ]
        wait = [
            r
            for r in pressed
            if r["before"]["revision"] == plain["revision"] and r["action"] == "wait"
        ]
        confirm = [
            r
            for r in pressed
            if r["before"]["revision"] == polled["revision"] and r["action"] == "confirm"
        ]
        if not early or not wait or not confirm:
            return finalize()
        wait, confirm = wait[0], confirm[0]

        def delivered(record, kind):
            return any(
                o["Kind"] == kind
                for r in records[record["resultStart"] : record["resultEnd"]]
                for o in r["result"]["observations"]
            )

        plain_value = (
            li < pi < wi < ai < ri
            and len(completed) == len(released) == len(previous) == len(acknowledged) == 1
            and completed[0][1]["Sequence"]
            < released[0][1]["Sequence"]
            < previous[0][1]["Sequence"]
            < plain["revision"]
            < acknowledged[0][1]["Sequence"]
            < acked["revision"]
            and plain["wait"] == polled["wait"] == "DialogueWait"
            and plain["token"] == polled["token"] == confirm["before"]["token"]
            and plain["cursor"] == polled["cursor"] == confirm["before"]["cursor"]
            and acked["wait"] == confirm["after"]["wait"] == "TextCloseWait"
            and all(
                side[key] == state[key]
                for side, state in (
                    (wait["before"], plain),
                    (wait["after"], polled),
                    (confirm["before"], polled),
                    (confirm["after"], acked),
                )
                for key in ("revision", "simulationTick", "mainSeed", "token", "cursor")
            )
            and {r["action"] for r in early} == {"wait", "confirm"}
            and all(r["resultStart"] == r["resultEnd"] and r["before"] == r["after"] for r in early)
            and polled["simulationTick"] == plain["simulationTick"] + 1
            and acked["simulationTick"] == polled["simulationTick"]
            and plain["mainSeed"] == polled["mainSeed"] == acked["mainSeed"]
            and delivered(wait, "gameplay-wait")
            and delivered(confirm, "presentation-acknowledged")
            and all(
                s["w1"] is None and s["fieldText"] is None and 603 not in s["flags"]
                for s in (plain, polled, acked)
            )
        )
        music, helper = logical["music"], logical["musicWait"]
        generation = music["Generation"]
        receipts = [r["receipt"] for r in actual["audioReceipts"]]
        starts = [
            r
            for r in receipts
            if r["Cue"] == "MUSIC_JOIN"
            and r["Operation"] == "started"
            and r["Revision"] == generation
        ]
        finishes = [
            r
            for r in receipts
            if r["Cue"] == "MUSIC_JOIN"
            and r["Operation"] == "finished"
            and r["Revision"] == logical["revision"]
        ]
        if not starts or not finishes:
            return finalize()
        start, finish = starts[0], finishes[0]
        restarts = [
            r
            for r in receipts
            if r["Cue"] == music["Previous"][-1]
            and r["Operation"] == "started"
            and finish["Sequence"] < r["Sequence"]
            and r["Revision"] < plain["revision"]
        ]
        transitional = [
            s["state"]
            for s in actual["samples"][li + 1 : pi]
            if s["state"]["audio"]["musicGeneration"] == generation
        ]
        if not restarts or not transitional:
            return finalize()
        restart = restarts[0]
        interval = [
            r
            for r in receipts
            if start["Sequence"] <= r["Sequence"] <= restart["Sequence"]
            and r["Cue"].startswith("MUSIC_")
        ]
        result["audio"] = (
            len(starts) == len(finishes) == len(restarts) == 1
            and interval == [start, finish, restart]
            and music["Cue"] == "MUSIC_JOIN"
            and music["Step"] == music["EndStep"]
            and music["PreviousEligible"]
            and not music["ActualDone"]
            and helper["Generation"] == generation
            and helper["LogicalDone"]
            and helper["Armed"]
            and helper["Cleared"]
            and logical["audio"]["musicGeneration"] == generation
            and logical["audio"]["musicPlaying"]
            and not logical["audio"]["musicFinished"]
            and finish["WaitToken"] == helper["Token"]["Value"]
            and start["PcmSha256"] == finish["PcmSha256"]
            and not finish["Playing"]
            and restart["Playing"]
            and all(
                s["audio"]["musicFinished"]
                and not s["audio"]["musicPlaying"]
                and s["audio"]["error"] is None
                for s in transitional
            )
            and plain["audio"]["musicCue"] == restart["Cue"]
            and plain["audio"]["musicPlaying"]
            and plain["audio"]["musicPosition"] > 0
            and plain["audio"]["error"] is None
            and completed[0][1]["Sequence"] > finish["Revision"]
            and restart["Revision"] < previous[0][1]["Sequence"]
        )
        if not world_path.is_file():
            return finalize()
        world = read(world_path)["world"]
        program = next(p for p in world["programs"] if p["id"] == "cs-51614")
        instructions = program["instructions"]
        begin = plain["cursor"]["Instruction"]
        tail = instructions[int(begin) :]
        actual_tail = [
            (i, o)
            for i, o in observations
            if o["Kind"] == "program-instruction"
            and o["Program"]["Program"] == program["id"]
            and acked["revision"] <= o["Sequence"] <= ready["revision"]
        ]
        ticks = event("simulation-tick")
        flag = [
            (i, o)
            for i, o in observations
            if o["Kind"] == "program-instruction"
            and o["Detail"] == "WriteFlag"
            and acked["revision"] < o["Sequence"] <= ready["revision"]
        ]
        zone_finished = event("zone-finished")
        arrivals = [
            s["state"]
            for s in actual["samples"][ai + 1 : ri]
            if s["state"]["wait"] == "ZoneArrivalWait"
        ]
        if not arrivals:
            return finalize()
        zone = next(p for p in world["programs"] if p["id"] == "map3-zoneevent8")
        entities = {e["id"]: e for e in ready["entities"]}
        effects = all(
            entities[i["entity"]]["follower"]
            == dict(
                LeaderSlot=int(i["leader"].removeprefix("entity-")),
                OffsetX=i["x"],
                OffsetY=i["y"],
            )
            for i in tail[3:5]
        ) and all(
            entities[i["entity"]]["x"] == i["position"]["x"] * 384
            and entities[i["entity"]]["y"] == i["position"]["y"] * 384
            and entities[i["entity"]]["facing"] == i["facing"]
            for i in tail[5:7]
        )
        result["caller"] = (
            effects
            and program["source"]
            == "disasm/data/maps/entries/map03/mapsetups/scripts_1.asm:cs_51614"
            and instructions[int(begin) - 4]["text"] == 447
            and instructions[int(begin) - 3]["waitForAcknowledgement"] is False
            and instructions[int(begin) - 2]["kind"] == "SoundWait"
            and instructions[int(begin) - 1]["kind"] == "PreviousMusic"
            and [i["op"] for i in tail]
            == [
                "wait-text-input",
                "close-text",
                "wait-ticks",
                "follow",
                "follow",
                "position",
                "position",
                "jump",
            ]
            and tail[2]["ticks"] == 10
            and [o["Program"]["Instruction"] for _, o in actual_tail]
            == list(range(int(begin) + 1, len(instructions)))
            and [o["Detail"] for _, o in actual_tail]
            == [
                "CloseText",
                "WaitProgramTicks",
                "FollowEntity",
                "FollowEntity",
                "SetEntityPosition",
                "SetEntityPosition",
                "JumpProgram",
            ]
            and len(ticks) == tail[2]["ticks"]
            and actual_tail[1][1]["Sequence"]
            < ticks[0][1]["Sequence"]
            <= ticks[-1][1]["Sequence"]
            < actual_tail[2][1]["Sequence"]
            and len(flag) == len(zone_finished) == 1
            and flag[0][1]["Program"]["Program"] == zone["id"]
            and zone["instructions"][int(flag[0][1]["Program"]["Instruction"])]
            == dict(op="set-flag", flag=603, value=True)
            and actual_tail[-1][1]["Sequence"]
            < flag[0][1]["Sequence"]
            < zone_finished[0][1]["Sequence"]
            == ready["revision"]
            and 603 in ready["flags"]
            and ready["wait"] is None
            and ready["cursor"] is None
            and ready["canWaitAtInput"]
            and all(603 in s["flags"] and not s["canWaitAtInput"] for s in arrivals)
            and {1, 2}.issubset(ready["partyLists"]["Joined"])
        )
        result["anchors"]["actual"] = dict(
            samples=[li, pi, wi, ai, ri],
            generation=generation,
            helperToken=helper["Token"]["Value"],
            receiptSequences=[r["Sequence"] for r in interval],
            inputOrdinals=[r["ordinal"] for r in early] + [wait["ordinal"], confirm["ordinal"]],
            completionRecords=[completed[0][0], released[0][0], previous[0][0]],
            callerRecords=[i for i, _ in actual_tail] + [flag[0][0], zone_finished[0][0]],
        )
    except (KeyError, IndexError, StopIteration):
        pass
    return finalize()


MATRIX_OBLIGATION = "complete named continuous settings matrix"


def modern_required_children(variant, ref):
    """Frozen winning-profile children; observed subsets do not enlarge this set."""
    ally_ids = [a["id"] for a in ref["admission"]["accounting"]["allies"][:3]]
    families = {
        None: (
            "map",
            "x",
            "y",
            "facing",
            "gold",
            "mainSeed",
            *(f"ally-{ally}.{field}" for ally in ally_ids for field in ("Hp", "Mp", "Status")),
            "admission roster",
            "one continuous session",
            "monotonic observed sequence",
            "full result stream",
            "ordinary route and probe assertions",
            "mandatory reached checkpoints",
            "natural battle first control",
            "adaptive actions and consumed outcome",
            "whole after/return order",
            "victory flags/program/return",
            "accepted original Down extension selection",
            "returned actual field input",
            *(
                label + " two settled host updates"
                for label in ("first-return", "before-down", "after-down")
            ),
            "ordinary Down displacement",
            "clean actual process",
            "actual physical/consumer input records",
            "configured device and release observations",
            MATRIX_OBLIGATION,
            "modern finite-music deviation declaration",
            "observed input dispatch intervals",
            "host delivery adds no gameplay or RNG service",
            *(("actual reveal-only Confirm",) if variant == "C" else ()),
            *(("natural reveal before Confirm",) if variant == "D" else ()),
        ),
        "complete mandatory operation-to-consumption mapping": (
            "after-program join/flag/return causal order",
            "taken route/setup/caller branch operands and occurrence",
            "awaited entity motion/gesture/fade before caller return",
            "dialogue speaker/control-token occurrence and choice effect",
            "route roster/flag writes at their source branch",
            "warp destination/setup initialization before field release",
            "before/after operation effects and shared-tail return pairing",
        ),
        "matched-state rule/RNG/draw-to-effect comparisons": (
            "repeated logical sequence identifies the same observation",
            "recorded main draw operands are available",
            "turn candidate score draws and tie/order result",
            "physical range/dodge/critical/spread/double/counter effects",
            "HEAL recovery/cost/fairy opportunity and seed effects",
            "EXP/gold/growth/spell learning and after-turn/outcome effects",
            "AI thinking draw/choice/memory and movement decision",
            "field text/portrait/NPC service draw-to-effect gates",
        ),
        "complete reached 7C resource/provenance inventory": (
            "reached map3/19/20/21/40/57 atlas and layer identities",
            "reached entity sprites/portraits/gesture resource identities",
            "displayed text tokens/font/glyph private binding",
            "scene actor/weapon/healing/death resources",
            "scene background/ground actual resource identity",
            "reached audio command/timer/PCM provenance and playback lifecycle",
        ),
        "required unshimmed ack and scene consumer binding": (
            "W1 displayed token occurrence/accepting read/service gates",
            "W2 accepting read/validation indicator and token return",
            "plain JOIN input after matching finite completion",
            "entity motion/gesture/fade consumer start/completion before resume",
            "battle scene command/resources/wait/effect/end consumer edges",
            "audio replacement/fade/stop/resume dependent consumer edges",
        ),
        "complete relevant admission phase/field mapping": (
            "admission joined",
            "admission active",
            "admission logical consumer readiness",
            "admission seed-copy byte",
            "opening mouth/view controls before first source write",
            "admission occupied physical slots",
            "effective admission class/level/maxima/stats/spells definition identity",
            "walking motion gate/velocity/travel/flags correspondence",
            *(
                f"admission walking slot {slot} {field}"
                for slot in (5, 6, 8)
                for field in ("wait timer", "cursor/moving source binding")
            ),
            *(f"ally-{ally} candidate class/stats/spell words" for ally in ally_ids),
        ),
    }
    for ally in ally_ids:
        parent = f"ally-{ally}.items"
        families[parent] = tuple(
            parent + "." + field
            for field in (
                "effective four-slot words",
                "candidate definition slots",
                "admission loadout identity",
            )
        )
    for parent, prefix, state in (
        ("complete relevant admission phase/field mapping", "admission", ref["admission"]),
        (
            "complete mandatory operation-to-consumption mapping",
            "returned story",
            ref["endpoint"],
        ),
    ):
        families[parent] += tuple(f"{prefix} flag {flag}" for flag in state["state"]["flags"])
    families["complete relevant admission phase/field mapping"] += tuple(
        f"admission slot {e['physical']} position/destination/facing/layer"
        for e in ref["inherited"]["entities"]
        if not (e["actionScript"] == 0 and e["x"] == e["y"] == 0x7000)
    )
    return families


def verdict(counts):
    return "FAIL" if counts.get("FAIL") else "Unavailable" if counts.get("Unavailable") else "PASS"


def modern_report_integrity(report, ref):
    """Reject omitted obligations and contradictory serialization, not absent evidence."""
    errors = []
    assertions = report.get("assertions", [])
    parents = report.get("coverageObligations", [])
    required = [a for a in assertions if a.get("applicability") != "historical-diagnostic"]
    historical = [a for a in assertions if a.get("applicability") == "historical-diagnostic"]
    names = Counter(a.get("assertion") for a in assertions)
    parent_names = Counter(p.get("assertion") for p in parents)
    for name, count in names.items():
        if count != 1:
            errors.append("duplicate assertion: " + str(name))
    for name, count in parent_names.items():
        if count != 1:
            errors.append("duplicate coverage parent: " + str(name))
    for parent, children in modern_required_children(report.get("variant"), ref).items():
        if parent is not None and parent_names[parent] != 1:
            errors.append("missing required family: " + parent)
        for name in children:
            matches = [
                a for a in required if a.get("assertion") == name and a.get("parent") == parent
            ]
            if len(matches) != 1:
                errors.append("missing required child: " + name)
    for row in assertions:
        if row.get("applicability") not in (
            "applicable",
            "required-unobserved",
            "historical-diagnostic",
        ):
            errors.append("invalid applicability: " + str(row.get("assertion")))
        expected_result = (
            "Unavailable"
            if row.get("actualValue") is None or row.get("applicability") == "required-unobserved"
            else "PASS"
            if row.get("expected") == row.get("actualValue")
            else "FAIL"
        )
        if row.get("result") != expected_result:
            errors.append("inconsistent assertion result: " + str(row.get("assertion")))
        if row.get("parent") and parent_names[row["parent"]] != 1:
            errors.append("missing assertion parent: " + row["parent"])
    for parent in parents:
        children = [a for a in required if a.get("parent") == parent.get("assertion")]
        old = [a for a in historical if a.get("parent") == parent.get("assertion")]
        counts = dict(Counter(a.get("result") for a in children))
        if (
            parent.get("children") != [a.get("assertion") for a in children]
            or parent.get("historicalChildren") != [a.get("assertion") for a in old]
            or parent.get("actualValue") != counts
            or parent.get("result") != verdict(counts)
            or parent.get("applicability")
            != ("required-unobserved" if verdict(counts) == "Unavailable" else "applicable")
        ):
            errors.append("inconsistent coverage parent: " + str(parent.get("assertion")))
    counts = dict(Counter(a.get("result") for a in required))
    if (
        report.get("counts") != counts
        or report.get("result") != verdict(counts)
        or report.get("milestonePass") is not (verdict(counts) == "PASS")
        or report.get("historicalCounts") != dict(Counter(a.get("result") for a in historical))
    ):
        errors.append("inconsistent report summary")
    return errors


def compare_modern(
    ref,
    actual_path,
    outcome_path,
    settings_path,
    host_log,
    host_exit,
    baseline_path=None,
    baseline_outcome=None,
    controlled_start_path=None,
    material_selection=None,
    original_join_evidence_root=None,
):
    actual, outcome, settings = read(actual_path), read(outcome_path), read(settings_path)
    samples = actual.get("samples", [])
    require(samples, "modern actual lacks samples")
    records = actual.get("warpRecords", [])
    assertions = []
    obligations = {}

    def check(
        layer,
        name,
        expected,
        value,
        location,
        original=None,
        applicability="applicable",
        reason="semantic assertion",
        parent=None,
        missing_side=None,
        actual_file=None,
    ):
        # Applicability precedes evaluation. Original diagnostics keep their raw mismatch.
        if value is None and applicability == "applicable":
            applicability = "required-unobserved"
            if reason == "semantic assertion":
                reason = "Actual field absent at " + location
        result = (
            "Unavailable"
            if value is None or applicability == "required-unobserved"
            else "PASS"
            if value == expected
            else "FAIL"
        )
        assertions.append(
            dict(
                layer=layer,
                assertion=name,
                applicability=applicability,
                original=original
                or dict(owner=OWNER, commit="87528953c6ee6d2631e24663a6d2e846be4c22ce"),
                actual=dict(
                    file=actual_file
                    or (
                        outcome_path
                        if location.startswith("outcome.")
                        else host_log
                        if location.startswith("host-log")
                        else actual_path
                    ).as_posix(),
                    record=location,
                ),
                expected=expected,
                actualValue=value,
                result=result,
                reason=reason,
                **({"parent": parent} if parent else {}),
                **({"missingSide": missing_side or "actual"} if result == "Unavailable" else {}),
            )
        )
        if parent:
            obligations.setdefault(parent, []).append(assertions[-1])

    first = samples[0]["state"]
    admission = ref["admission"]
    player = endpoint_state(first)["player"]
    for name, expected, value in (
        ("map", f"map-{admission['state']['map']}", first.get("map")),
        ("x", admission["state"]["x"] * 384, player["x"]),
        ("y", admission["state"]["y"] * 384, player["y"]),
        ("facing", admission["state"]["facing"], player["facing"]),
        ("gold", admission["accounting"]["gold"], first.get("gold")),
        (
            "mainSeed",
            int.from_bytes(bytes(admission["state"]["rngBytes"]), "big"),
            first.get("mainSeed"),
        ),
    ):
        check(1, name, expected, value, "samples[0]", admission["source"])
    candidate = read(controlled_start_path) if controlled_start_path else None
    if candidate:
        require(
            candidate.get("formatVersion") == 1
            and candidate.get("profile") == "private-local-controlled-start",
            "not a controlled party definition input",
        )
        ids = [a["id"] for a in candidate["allies"]]
        require(len(ids) == len(set(ids)), "duplicate controlled party actor")
    candidate_allies = {a["id"]: a for a in candidate["allies"]} if candidate else {}
    startup = actual.get("admissionSnapshot", {})
    startup_state = startup.get("state", {})
    admitted = startup_state.get("admittedParty") or {}
    selected_encounters = [
        row
        for row in admitted.get("encounters", [])
        if row.get("encounter") == admitted.get("encounter")
    ]
    deployments = (
        selected_encounters[0].get("deployments", []) if len(selected_encounters) == 1 else []
    )
    admission_identities = []
    admitted_stats = []
    candidate_definitions = []
    for ally in admission["accounting"]["allies"][:3]:
        actor_id = f"ally-{ally['id']}"
        observed_inputs = [
            p for p in startup_state.get("party", []) if p["Actor"]["Value"] == actor_id
        ]
        bound = [d for d in deployments if d.get("actor") == actor_id]
        observed_input = observed_inputs[0] if len(observed_inputs) == 1 else None
        deployment = bound[0] if len(bound) == 1 else None
        definition = deployment.get("definition", {}) if deployment else {}
        carried = next(
            (p for p in first.get("party", []) if p["Actor"]["Value"] == f"ally-{ally['id']}"), {}
        )
        for source, field in (
            ("hpCurrent", "Hp"),
            ("mpCurrent", "Mp"),
            ("statusEffects", "Status"),
        ):
            check(
                1,
                f"ally-{ally['id']}.{field}",
                ally[source],
                carried.get(field),
                "samples[0].party",
                admission["source"],
            )
        parent = f"ally-{ally['id']}.items"
        explicit = carried.get("SourceLoadout")
        progress = carried.get("Progress")
        inherited = progress.get("SourceLoadout") if isinstance(progress, dict) else None
        loadout = explicit if explicit is not None else inherited
        selected = candidate_allies.get(ally["id"])
        identity = None
        if admitted:
            # Select the named encounter/deployment; ambiguity is not a first-match fallback.
            selected_definition = (
                dict(
                    classRule={
                        0: "UnpromotedSwordsman",
                        1: "UnpromotedKnight",
                        4: "UnpromotedPriest",
                    }.get(selected["classId"]),
                    level=selected["level"],
                    maxHp=selected["maxHp"],
                    maxMp=selected["maxMp"],
                    attack=selected["attack"],
                    defense=selected["defense"],
                    agility=selected["agility"] & 127,
                    extraRoundAction=bool(selected["agility"] & 128),
                    move=selected["move"],
                )
                if selected
                else None
            )
            input_fields = {
                "Hp": "hp",
                "Mp": "mp",
                "Exp": "exp",
                "Kills": "kills",
                "Defeats": "defeats",
                "Status": "status",
            }
            identity = bool(
                candidate
                and selected
                and observed_input
                and deployment
                and startup.get("inputOrdinal") == 0
                and startup_state.get("revision") is not None
                and 0 <= startup_state["revision"] <= first["revision"]
                and startup_state.get("sessionId") == first.get("sessionId")
                and admitted.get("package")
                and admitted.get("provenance")
                and admitted.get("origin") == candidate.get("profile")
                and admitted.get("encounter") == f"battle-{candidate['battle']}"
                and deployment.get("member") == ally["id"]
                and {k: definition.get(k) for k in selected_definition} == selected_definition
                and (definition.get("sourceLoadout") or {}).get("Items") == selected.get("items")
                and (definition.get("sourceLoadout") or {}).get("Spells") == selected.get("spells")
                and {k: observed_input.get(k) for k in input_fields}
                == {k: selected.get(v) for k, v in input_fields.items()}
                and observed_input == carried
            )
            if observed_input and deployment:
                explicit = observed_input.get("SourceLoadout")
                progress = observed_input.get("Progress")
                inherited = progress.get("SourceLoadout") if isinstance(progress, dict) else None
                loadout = (
                    explicit
                    if explicit is not None
                    else (inherited if inherited is not None else definition.get("sourceLoadout"))
                )
        admission_identities.append(identity)
        admitted_stats.append(definition if identity and progress is None else None)
        expected_items = [i["raw"] for i in ally["items"]]
        candidate_definitions.append(
            dict(
                actor=f"ally-{ally['id']}",
                file=controlled_start_path.as_posix() if controlled_start_path else None,
                record=f"allies[id={ally['id']}]",
                items=selected.get("items") if selected else None,
                actualOverride=explicit,
                actualProgress=progress,
                actual=dict(
                    file=actual_path.as_posix(),
                    record=f"admissionSnapshot.state.party[Actor={actor_id}]"
                    if admitted
                    else f"samples[0].party[Actor={actor_id}]",
                ),
                candidateEvidenceOwner=candidate.get("evidenceOwner") if candidate else None,
                effectiveAdmissionObserved=isinstance(loadout, dict) and "Items" in loadout,
                admittedDeployment=deployment,
                admittedDeploymentRecord=f"admissionSnapshot.state.admittedParty.encounters"
                f"[encounter={admitted.get('encounter')}].deployments[actor={actor_id}]"
                if admitted
                else None,
                sameSessionInputConsistency=identity,
                reason="Read-through admitted definition/operands at input ordinal0"
                if admitted
                else "Selected definition is a candidate input, not a same-run deployment snapshot",
            )
        )
        check(
            1,
            parent + ".effective four-slot words",
            expected_items,
            loadout.get("Items") if isinstance(loadout, dict) else None,
            "admissionSnapshot.state.party/admittedParty.encounters"
            if admitted
            else "samples[0].party.SourceLoadout",
            admission["source"],
            parent=parent,
            missing_side="actual deployment definition at admission",
            reason="Resolve explicit override, then Progress.SourceLoadout, then same-run "
            "deployment definition; absent definition cannot be replaced by later inventory",
        )
        check(
            1,
            parent + ".candidate definition slots",
            expected_items,
            selected.get("items") if selected else None,
            f"allies[id={ally['id']}].items",
            admission["source"],
            parent=parent,
            actual_file=controlled_start_path.as_posix() if controlled_start_path else None,
            missing_side="explicit controlled-start selection",
            reason="Candidate input comparison only; equality does not observe loaded "
            "admission slots",
        )
        check(
            1,
            parent + ".admission loadout identity",
            True,
            identity
            if admitted
            else (
                isinstance(loadout, dict) and "Items" in loadout if loadout is not None else None
            ),
            "admissionSnapshot.state.party/admittedParty.encounters"
            if admitted
            else "samples[0].party.SourceLoadout/Progress.SourceLoadout",
            dict(
                owner=OWNER,
                binding="R1 full item words; modern override/progress/definition precedence",
                modernOwner="remake/src/Sf2.Remake.Domain/Battles/State/EngineBattleState.cs:83",
            ),
            parent=parent,
            missing_side="same-run loaded deployment definition identity",
            reason="Same-session pre-command admitted definition and party operands compared "
            "to the explicitly selected input; no instantiated battle is claimed at startup"
            if admitted
            else "Current launcher and selected input do not supply an omitted same-run definition",
        )
    check(
        1,
        "admission roster",
        admission["accounting"]["active"],
        (first.get("partyLists") or {}).get("Active"),
        "samples[0].partyLists",
        admission["source"],
    )

    phase_parent = "complete relevant admission phase/field mapping"
    for flag, expected_flag in admission["state"]["flags"].items():
        check(
            1,
            f"admission flag {flag}",
            expected_flag,
            int(flag) in first["flags"] if "flags" in first else None,
            "samples[0].flags",
            admission["source"],
            parent=phase_parent,
        )
    for source_key, actual_key in (("joined", "Joined"), ("active", "Active")):
        check(
            1,
            "admission " + source_key,
            admission["accounting"][source_key],
            (first.get("partyLists") or {}).get(actual_key),
            "samples[0].partyLists",
            admission["source"],
            parent=phase_parent,
        )
    readiness_fields = (
        "continuation",
        "cursor",
        "wait",
        "callers",
        "callerReturning",
        "warp",
        "choice",
        "battleMounted",
        "canWaitAtInput",
    )
    check(
        1,
        "admission logical consumer readiness",
        dict(
            continuation="FieldInput",
            cursor=None,
            wait=None,
            callers=[],
            callerReturning=None,
            warp=None,
            choice=None,
            battleMounted=False,
            canWaitAtInput=True,
        ),
        {k: first[k] for k in readiness_fields}
        if all(k in first for k in readiness_fields)
        else None,
        "samples[0].continuation/cursor/wait/callers/warp/choice",
        dict(
            **admission["source"],
            owner="docs/design/contracts/map3-controlled-admission.md",
            binding="zero pending returns/active consumers at the selected logical R1 seam",
        ),
        parent=phase_parent,
    )
    check(
        1,
        "admission seed-copy byte",
        admission["state"]["rngCopyByte"],
        first.get("randomSeedCopy"),
        "samples[0].randomSeedCopy",
        admission["source"],
        parent=phase_parent,
        reason="An absent latch is not zero; first-read/write relevance is not yet bound",
    )
    check(
        1,
        "opening mouth/view controls before first source write",
        "original opening readback",
        (first.get("textSettings") or {}),
        "samples[0].textSettings",
        dict(owner="docs/design/contracts/dialogue-system.md", binding="opening service gates"),
        applicability="required-unobserved",
        parent=phase_parent,
        missing_side="original",
        reason="MouthControl/ViewSpeed are ancestry Inferred, not an opening RAM observation",
    )
    # In this selected R1, zero script plus the 0x7000 sentinel denotes an unused physical slot.
    occupied = [
        e
        for e in ref["inherited"]["entities"]
        if not (e["actionScript"] == 0 and e["x"] == e["y"] == 0x7000)
    ]
    check(
        1,
        "admission occupied physical slots",
        [e["physical"] for e in occupied],
        sorted(int(e["slot"]) for e in first["entities"]) if "entities" in first else None,
        "samples[0].entities[*].slot",
        ref["inherited"]["source"],
        parent=phase_parent,
    )
    entity_fields = {
        "x": "x",
        "y": "y",
        "targetX": "destinationX",
        "targetY": "destinationY",
        "facing": "facing",
        "layer": "layer",
    }
    for entity in occupied:
        modern = next((e for e in first.get("entities", []) if e["slot"] == entity["physical"]), {})
        check(
            1,
            f"admission slot {entity['physical']} position/destination/facing/layer",
            {k: entity[v] for k, v in entity_fields.items()},
            {k: modern[k] for k in entity_fields}
            if all(k in modern for k in entity_fields)
            else None,
            f"samples[0].entities[slot={entity['physical']}]",
            ref["inherited"]["source"],
            parent=phase_parent,
        )
    for slot in (5, 6, 8):
        modern = next((e for e in first.get("entities", []) if e["slot"] == slot), {})
        original_entity = next(e for e in ref["inherited"]["entities"] if e["physical"] == slot)
        check(
            1,
            f"admission walking slot {slot} wait timer",
            original_entity["waitTimer"],
            modern.get("waitTimer"),
            f"samples[0].entities[slot={slot}].waitTimer",
            dict(
                **ref["inherited"]["source"],
                binding=f"inherited.entities[physical={slot}].waitTimer",
            ),
            parent=phase_parent,
            reason="Direct original readback; no remake start supplies the expected timer",
        )
        check(
            1,
            f"admission walking slot {slot} cursor/moving source binding",
            "original pointer/template/base/offset to cursor, and source moving-gate readback",
            {k: modern.get(k) for k in ("actionCursor", "moving")},
            f"samples[0].entities[slot={slot}].actionCursor/moving",
            dict(
                **ref["inherited"]["source"],
                binding=f"inherited.entities[physical={slot}].actionScript",
                actionScript=original_entity["actionScript"],
            ),
            applicability="required-unobserved",
            parent=phase_parent,
            missing_side="original pointer/template/offset and moving-gate translation",
            reason="The original pointer is retained but no executable source translation/gate "
            "binding is selected. Actual cursor/moving or coordinate differences cannot supply "
            "original expectations",
        )
    for ally in admission["accounting"]["allies"][:3]:
        selected = candidate_allies.get(ally["id"])
        fields = {
            "classId": "class",
            "level": "level",
            "maxHp": "hpMax",
            "maxMp": "mpMax",
            "attack": "attack",
            "defense": "defense",
            "agility": "agility",
            "move": "move",
        }
        expected_definition = {k: ally[v] for k, v in fields.items()}
        expected_definition["spells"] = [s["raw"] for s in ally["spells"]]
        check(
            1,
            f"ally-{ally['id']} candidate class/stats/spell words",
            expected_definition,
            {k: selected.get(k) for k in expected_definition} if selected else None,
            f"allies[id={ally['id']}]",
            admission["source"],
            parent=phase_parent,
            actual_file=controlled_start_path.as_posix() if controlled_start_path else None,
            missing_side="explicit controlled-start selection",
            reason="Candidate definition only; this does not observe effective admission "
            "stats/spells",
        )
    check(
        1,
        "effective admission class/level/maxima/stats/spells definition identity",
        True,
        (False if not all(admission_identities) else True if all(admitted_stats) else None)
        if admitted
        else None,
        "admissionSnapshot.state.admittedParty.encounters/party",
        admission["source"],
        parent=phase_parent,
        missing_side="actual",
        reason="Same-session admitted class/stats/spell words match the selected input, whose "
        "original comparison is independent; non-null stat Progress needs effective stat binding",
    )
    check(
        1,
        "walking motion gate/velocity/travel/flags correspondence",
        "selected R1 walking motion fields and waiting-for-motion gate",
        None,
        "samples[0].entities[slot=5/6/8]",
        dict(**ref["inherited"]["source"], owner="docs/research/map3-messenger-acceptance.md"),
        parent=phase_parent,
        missing_side="source motion-field join and actual motion gate",
        reason="Selected reference omits the raw motion bytes; actual projection omits "
        "WaitingForMotion. Matched geometry and directly observed wait timers do not fill these "
        "operands or prove the cursor/moving translation",
    )

    events = [
        e
        for r in records
        if r["result"]["boundary"] == "submit"
        for e in r["result"]["observations"]
    ]
    session = first.get("sessionId")
    check(
        2,
        "one continuous session",
        True,
        bool(session)
        and all(r["result"]["sessionId"] == session for r in records)
        and outcome["final"]["sessionId"] == session,
        "warpRecords[*].result/sessionId",
    )
    sequences = [r["result"]["observationSequence"] for r in records]
    check(
        2,
        "monotonic observed sequence",
        True,
        all(x <= y for x, y in zip(sequences, sequences[1:], strict=False)),
        "warpRecords[*].result.observationSequence",
    )
    check(
        2,
        "full result stream",
        True,
        bool(records) and any(r.get("projection") == "field-view-pending" for r in records),
        "warpRecords[*]",
    )
    check(
        3,
        "ordinary route and probe assertions",
        True,
        actual.get("passed") and not actual.get("failures") and not actual.get("unavailable"),
        "passed/failures/unavailable",
    )
    labels = {s["label"]: s["state"] for s in samples}
    required = [
        "parallax-field-return",
        "before-battle-text2293-input",
        "tracking-text2294-input",
        "tracking-text2295-input",
        "tracking-text2296-input",
        "white-chester2297-input",
        "bound-first-battle-input",
        "bound-first-action-choice",
        "bound-first-movement-cancel",
    ]
    check(
        3,
        "mandatory reached checkpoints",
        True,
        all(k in labels for k in required),
        "samples[*].label",
    )
    if baseline_path is not None:
        baseline = {s["label"]: s["state"] for s in read(baseline_path)["samples"]}
        fields = (
            "simulationTick",
            "mainSeed",
            "randomSeedCopy",
            "continuation",
            "wait",
            "token",
            "cursor",
            "callers",
            "enteringBattle",
            "entities",
            "party",
            "flags",
            "gold",
            "logicalText",
            "logicalView",
            "textSettings",
            "fieldText",
            "portraitWork",
            "display",
            "round",
            "actor",
            "stage",
            "storyFlags",
        )
        for label in required:
            check(
                3,
                "accepted modern baseline:" + label,
                {k: baseline.get(label, {}).get(k) for k in fields},
                {k: labels.get(label, {}).get(k) for k in fields},
                "samples:" + label,
                dict(
                    owner="PR588 ordinary-normal-05",
                    commit="87528953c6ee6d2631e24663a6d2e846be4c22ce",
                    file=baseline_path.as_posix(),
                    record="samples:" + label,
                ),
            )
    battle = labels.get("bound-first-battle-input", {})
    check(
        4,
        "natural battle first control",
        ["Movement", "PlayerInput", 1],
        [battle.get("stage"), battle.get("stopReason"), battle.get("round")],
        "samples:bound-first-battle-input",
    )
    turn = ref["turns"][0]
    check(
        4,
        "original first order",
        turn["accounting"]["turnOrder"],
        [
            dict(actor=source_actor(x["actor"]), score=x["score"])
            for x in battle.get("turnOrder", [])
            if x["actor"]
        ],
        "samples:bound-first-battle-input",
        turn["source"],
        "historical-diagnostic",
        "Selected original history under declared modern finite-music policy; "
        "mismatch cause is not attributed",
    )
    check(
        4,
        "original first actor",
        turn["actor"],
        source_actor(battle.get("actor")),
        "samples:bound-first-battle-input",
        turn["source"],
        "historical-diagnostic",
        "Original trajectory, not a modern legality constant",
    )

    # Keep all browsing failures; only the accepted candidate rejection has a bounded allowance.
    actor_render = {"nodeX", "nodeY", "visible", "text", "sprite", "globalRect", "insideMap"}

    def gameplay(s):
        return {
            **{
                k: s.get(k)
                for k in (
                    "sessionId",
                    "revision",
                    "mainSeed",
                    "thinkingSeed",
                    "gold",
                    "queueCursor",
                    "round",
                    "actor",
                    "target",
                    "previewX",
                    "previewY",
                    "stage",
                    "spell",
                    "itemSlot",
                    "inventories",
                    "turnOrder",
                    "storyFlags",
                    "regionFlags",
                    "aiMemory",
                )
            },
            "actors": [
                {k: v for k, v in a.items() if k not in actor_render} for a in s.get("actors", [])
            ],
        }

    previous = None
    rejections = []
    for index, row in enumerate(records):
        state, result = row.get("state", {}), row["result"]
        failure = result.get("failure")
        if failure:
            atomic = previous is not None and gameplay(state) == gameplay(previous)
            living = [
                a["id"]
                for a in state.get("actors", [])
                if a["id"].startswith("enemy-") and a["hp"] > 0
            ]
            candidate = previous.get("candidate") if previous else None
            next_candidate = (
                living[(living.index(candidate) + 1) % len(living)]
                if candidate in living
                else living[0]
                if living
                else None
            )
            permitted = (
                # SessionFailureKind.IllegalCommand is enum0 in the actual result payload.
                failure.get("Kind") == 0
                and failure.get("Code") == "target-range"
                and failure.get("Field") == "target"
                and state.get("candidate") == next_candidate
            )
            later = next(
                (
                    r
                    for r in records[index + 1 :]
                    if r.get("state", {}).get("stage") != "TargetChoice"
                    or r.get("state", {}).get("target") is not None
                ),
                None,
            )
            legal = (
                later is not None
                and later["result"].get("failure") is None
                and later.get("state", {}).get("target") is not None
            )
            confirmed = next(
                (
                    r
                    for r in records[index + 1 :]
                    if any(
                        e["Kind"] == "scene-prepared"
                        or e["Kind"] == "battle-movement-segment-started"
                        and e["Detail"] == "Action"
                        for e in r["result"]["observations"]
                    )
                ),
                None,
            )
            legal = legal and confirmed is not None and confirmed["result"].get("failure") is None
            check(
                5,
                f"rejection[{index}] atomic/accepted later target",
                True,
                permitted and atomic and not result["observations"] and legal,
                f"warpRecords[{index}]",
            )
            rejections.append(
                dict(index=index, code=failure.get("Code"), atomic=atomic, legalLaterTarget=legal)
            )
        if "stage" in state:
            previous = state
    check(
        5,
        "adaptive actions and consumed outcome",
        True,
        bool([r for r in outcome.get("records", []) if r.get("label") == "action-selected"])
        and outcome.get("issue") == "",
        "outcome.records/issue",
    )
    ordered = [
        e["Kind"]
        for e in events
        if e["Kind"]
        in ("after-battle-join", "battle-unlock-cleared", "battle-completed-set", "battle-returned")
    ]
    check(
        6,
        "whole after/return order",
        ["after-battle-join", "battle-unlock-cleared", "battle-completed-set", "battle-returned"],
        ordered,
        "warpRecords[*].result.observations",
    )
    final = outcome["final"]
    check(
        6,
        "victory flags/program/return",
        True,
        501 in final.get("flags", [])
        and 401 not in final.get("flags", [])
        and "abcs-battle01" in outcome.get("programs", [])
        and outcome.get("heldBattle")
        and final.get("map") == "map-57",
        "outcome.final/programs",
    )
    check(
        7,
        "accepted original Down extension selection",
        True,
        True if ref.get("postVictoryInput") else None,
        "reference.postVictoryInput",
        reason="Separate original extension required; absent selection is not absent host data",
    )
    check(
        7,
        "returned actual field input",
        True,
        final.get("canWaitAtInput")
        and final.get("wait") is None
        and final.get("cursor") is None
        and not final.get("battleMounted")
        and final.get("failure") is None,
        "outcome.final",
    )
    endpoints = outcome.get("endpoints", [])
    paired = {}
    for label in ("first-return", "before-down", "after-down"):
        pair = [e for e in endpoints if e["label"] == label]
        if len(pair) == 2:
            states = [endpoint_state(e["state"]) for e in pair]
            ready = all(
                s["canWaitAtInput"]
                and s["wait"] is None
                and s["cursor"] is None
                and not s["callers"]
                and s["callerReturning"] is None
                and s["warp"] is None
                and not s["battleMounted"]
                and s["choice"] is None
                and s["tickDebt"] == 0
                and not s["player"]["moving"]
                and not s["player"]["busy"]
                for s in states
            )
            check(
                7,
                label + " two settled host updates",
                True,
                pair[0]["hostUpdate"] < pair[1]["hostUpdate"] and states[0] == states[1] and ready,
                f"outcome.endpoints:{label}",
            )
            paired[label] = states[1]
        else:
            check(
                7,
                label + " two settled host updates",
                True,
                None,
                f"outcome.endpoints:{label}",
                applicability="required-unobserved",
                reason="Actual independent host update records absent",
            )
    if "before-down" in paired and "after-down" in paired:
        before, after = paired["before-down"]["player"], paired["after-down"]["player"]
        check(
            7,
            "ordinary Down displacement",
            [before["x"], before["y"] + 384, 3],
            [after["x"], after["y"], after["facing"]],
            "outcome.endpoints:after-down",
            (ref.get("postVictoryInput") or {}).get("input"),
        )
        if baseline_outcome is not None:
            check(
                7,
                "accepted modern Left endpoint",
                endpoint_state(read(baseline_outcome)["final"]),
                paired["before-down"],
                "outcome.endpoints:before-down",
                dict(
                    owner="PR588 ordinary-normal-05",
                    commit="87528953c6ee6d2631e24663a6d2e846be4c22ce",
                    file=baseline_outcome.as_posix(),
                    record="final",
                ),
            )
    else:
        check(
            7,
            "ordinary Down displacement",
            "accepted local Down rule",
            None,
            "outcome.endpoints",
            applicability="required-unobserved",
            reason="Left evidence does not supply the named Down boundary",
        )
    expected = ref["endpoint"]
    returned = next((e["state"] for e in endpoints if e["label"] == "first-return"), None)
    returned_player = endpoint_state(returned)["player"] if returned else None
    for field, original, value in (
        ("gold", expected["accounting"]["gold"], returned.get("gold") if returned else None),
        (
            "position",
            [expected["state"]["x"] * 384, expected["state"]["y"] * 384],
            [returned_player["x"], returned_player["y"]] if returned_player else None,
        ),
    ):
        check(
            7,
            "original endpoint " + field,
            original,
            value,
            "outcome.endpoints:first-return",
            expected["source"],
            "historical-diagnostic",
            "Original terminal trajectory remains evidence, not a modern endpoint constant",
        )
    check(
        9,
        "clean actual process",
        True,
        host_exit == 0
        and not any(
            k in host_log.read_text(encoding="utf-8")
            for k in ("ERROR:", "SCRIPT ERROR", "Exception")
        ),
        "host-log/recorded-exit",
    )
    input_rows = actual.get("inputRecords", [])
    check(
        10,
        "actual physical/consumer input records",
        True,
        bool(input_rows) or None,
        "inputRecords",
        applicability="applicable" if input_rows else "required-unobserved",
        reason="Input ordinals are observed, not inferred from snapshots",
    )
    variant = actual.get("h4Variant")
    kinds = {r["delivery"]["kind"] for r in input_rows if r["pressed"]}
    if variant in ("A", "B", "C", "D"):
        check(
            10,
            "configured device and release observations",
            True,
            ({"axis", "button"}.issubset(kinds) if variant in "BD" else kinds == {"key"})
            and all(
                any(
                    not release["pressed"]
                    and release["action"] == press["action"]
                    and release["delivery"]["kind"] == press["delivery"]["kind"]
                    and release["delivery"]["code"] == press["delivery"]["code"]
                    for release in input_rows[index + 1 :]
                )
                for index, press in enumerate(input_rows)
                if press["pressed"]
            ),
            "inputRecords[*].delivery/ordinal",
        )

    def missing(layer, parent, name, binding, actual_location, side, reason, observed=None):
        check(
            layer,
            name,
            binding,
            observed,
            actual_location,
            dict(owner=OWNER, sourceCommit=SOURCE, upstream=UPSTREAM, binding=binding),
            applicability="required-unobserved",
            parent=parent,
            missing_side=side,
            reason=reason,
        )

    operation_parent = "complete mandatory operation-to-consumption mapping"
    for flag, expected_flag in ref["endpoint"]["state"]["flags"].items():
        check(
            3,
            f"returned story flag {flag}",
            expected_flag,
            int(flag) in final["flags"] if "flags" in final else None,
            "outcome.final.flags",
            ref["endpoint"]["source"],
            parent=operation_parent,
        )
    check(
        3,
        "after-program join/flag/return causal order",
        ["after-battle-join", "battle-unlock-cleared", "battle-completed-set", "battle-returned"],
        ordered,
        "warpRecords[*].result.observations",
        dict(
            owner="docs/research/map3-battle01-victory-return.md",
            binding="accepted after-program shared tail before flag writes and enclosing return",
        ),
        parent=operation_parent,
    )
    for name, binding, actual_location in (
        (
            "taken route/setup/caller branch operands and occurrence",
            "operationPairs.entry/return + program/operation.pc/opcode + route/story branch "
            "operands",
            "warpRecords[*].result.observations.Program/Detail",
        ),
        (
            "awaited entity motion/gesture/fade before caller return",
            "story operation subject/destination/wait + operationPairs return",
            "warpRecords[*].state.entities/callers/presentation",
        ),
        (
            "dialogue speaker/control-token occurrence and choice effect",
            "story displayed text/speaker + choices + ordered W1/W2 tokens",
            "samples[*].textId/speaker/fieldText + inputRecords",
        ),
        (
            "route roster/flag writes at their source branch",
            "story joined/follower and F600..608/F66/F89 changes before operation return",
            "warpRecords[*].state.partyLists/flags",
        ),
        (
            "warp destination/setup initialization before field release",
            "route warp/setup operands and pending consumer return",
            "warpRecords[*].state.warp/loadServices/callers",
        ),
        (
            "before/after operation effects and shared-tail return pairing",
            "operationPairs for reached bbcs_01/abcs_battle01, including awaited effects and "
            "shared tail",
            "warpRecords[*].result.observations.Program + outcome.records/programs",
        ),
    ):
        missing(
            3,
            operation_parent,
            name,
            binding,
            actual_location,
            "original/actual semantic join",
            "Source operation PC/operands and typed instruction/consumer occurrence are not "
            "joined; instruction presence or total count cannot establish this edge",
        )

    rule_parent = "matched-state rule/RNG/draw-to-effect comparisons"
    unique_events = {}
    sequence_consistent = True
    for event in events:
        sequence = event["Sequence"]
        if sequence in unique_events and unique_events[sequence] != event:
            sequence_consistent = False
        unique_events[sequence] = event
    check(
        5,
        "repeated logical sequence identifies the same observation",
        True,
        sequence_consistent,
        "warpRecords[*].result.observations.Sequence",
        parent=rule_parent,
        reason="Repeated snapshots cannot duplicate or alter a consumed draw/effect",
    )
    # Reuse the original H3 model, not the remake RNG. Its fixed-range model is sufficient
    # for these recorded ranges; preserve the full image's unmodified low word separately.
    draws = [e for e in unique_events.values() if e["Kind"].startswith("rng-")]
    for event in draws:
        operands = [event.get(k) for k in ("Before", "After", "RandomRange", "RandomValue")]
        if all(v is not None for v in operands):
            before, after, range_, value = map(int, operands)
            word, result = _rng_step(before >> 16, ((range_ * 2) & 65535))
            expected_draw = dict(after=(word << 16) | (before & 65535), value=result >> 1)
            actual_draw = dict(after=after, value=value)
        else:
            expected_draw, actual_draw = "word update, preserved low word, range/result", None
        check(
            5,
            f"main draw {int(event['Sequence'])} {event['Kind']}",
            expected_draw,
            actual_draw,
            f"warpRecords.observations[Sequence={int(event['Sequence'])}]",
            dict(
                owner="docs/design/contracts/randomness.md",
                fixture="tests/fixtures/h3/rng-v1.json",
                upstream=UPSTREAM,
                binding="GenerateRandomNumber word update and doubled-range upper product",
            ),
            parent=rule_parent,
            reason="Independent accepted original generator model at the actual seed/range; "
            "this does not yet associate the draw with a particular source effect",
        )
    check(
        5,
        "recorded main draw operands are available",
        True,
        bool(draws) or None,
        "warpRecords.observations: rng-*",
        parent=rule_parent,
    )
    for name, binding, actual_location, side in (
        (
            "turn candidate score draws and tie/order result",
            "turns.accounting.turnOrder + source agility/extra-round score rule at matched seed",
            "round-rng/round-started + battle turnOrder",
            "actual per-candidate range/draw operands",
        ),
        (
            "physical range/dodge/critical/spread/double/counter effects",
            "scenes.effects.attackType + source action rules at matched "
            "stats/status/equipment/seed",
            "rng-dodge/critical/spread/double/counter + physical-first/second/hp",
            "matched original operands and draw-to-effect join",
        ),
        (
            "HEAL recovery/cost/fairy opportunity and seed effects",
            "scenes.effects + source HEAL MP/recovery and fairy caller/gate rules",
            "spell-selected/mp/heal/rng-fairy-* + scene.healing",
            "original caller opportunities and matched recovery/draw effects",
        ),
        (
            "healing item slot words/removal and resource effects",
            "decisions UseItem zero-based slot/live word + scenes consumed item/HP boundary",
            "itemSlot/inventories + action-committed/scene-ended",
            "matched source item action/resource occurrence",
        ),
        (
            "EXP/gold/growth/spell learning and after-turn/outcome effects",
            "scenes.after accounting + source reward/growth/status and victory rules",
            "exp/gold/level-*/after-turn/battle-outcome + party.Progress",
            "matched source preconditions and draw/effect association",
        ),
        (
            "AI thinking draw/choice/memory and movement decision",
            "StartAiControl/ExecuteAiControl serialized actor memory + source thinking-byte "
            "contract",
            "thinking-rng/ai-memory/ai-target/ai-move + thinkingSeed",
            "original thinking draw stream and decoded matched AI memory",
        ),
        (
            "field text/portrait/NPC service draw-to-effect gates",
            "story W1/W2/portrait/entity caller state plus RNG consumerScopes",
            "rng-text-*/rng-portrait-* + fieldText/portraitWork/entities",
            "source live service gates and individual caller/effect mapping",
        ),
    ):
        item_reached = any(e["Kind"] == "item-consumed" for e in unique_events.values()) or any(
            row["state"].get("itemSlot") is not None
            for row in outcome.get("records", [])
            if row.get("label") == "action-selected"
        )
        if name == "healing item slot words/removal and resource effects" and not item_reached:
            check(
                5,
                name,
                binding,
                None,
                actual_location,
                dict(owner=OWNER, binding="original UseItem history in decisions/scenes"),
                applicability="historical-diagnostic",
                parent=rule_parent,
                reason="No item action is reached in this actual winning profile; retain original "
                "UseItem history without imposing an unvisited consuming branch",
            )
            continue
        missing(
            5,
            rule_parent,
            name,
            binding,
            actual_location,
            side,
            "Compare the reached rule at matched operands; whole historical actor/round "
            "sequence and settings equality cannot supply this binding",
        )

    resource_parent = "complete reached 7C resource/provenance inventory"
    scene_rows = actual.get("sceneObservations", [])
    mounted_scenes = [
        r for r in scene_rows if r["scene"].get("visible") and not r["scene"].get("fieldDeath")
    ]
    mounted_resources = sorted(
        {
            r["scene"][name]["resource"]
            for r in mounted_scenes
            for name in ("background", "backgroundWrap", "ground")
            if r["scene"].get(name, {}).get("visible") and r["scene"][name].get("resource")
        }
    )
    if scene_rows:
        check(
            8,
            "actual mounted scene background/ground identities",
            True,
            bool(mounted_scenes)
            and all(
                r["scene"].get(name, {}).get("visible")
                for r in mounted_scenes
                for name in ("background", "backgroundWrap")
            )
            and all(
                node.get("resource") and node.get("texturePresent")
                for r in mounted_scenes
                for name in ("background", "backgroundWrap", "ground")
                if (node := r["scene"].get(name, {})).get("visible")
            ),
            "sceneObservations[*].scene.background/backgroundWrap/ground",
            parent=resource_parent,
            reason="IDs read from successfully bound visible nodes in this session; "
            "hidden metadata is not consumption",
        )
    receipt_rows = actual.get("audioReceipts", [])
    audio_terminal = actual.get("audioTerminal", {})
    audio_receipts = [r["receipt"] for r in receipt_rows]
    audio_contiguous = None
    audio_balances = Counter()
    audio_lifecycle = {}
    if audio_terminal:
        audio_contiguous = bool(
            not actual.get("audioReceiptGaps")
            and [r["Sequence"] for r in audio_receipts]
            == list(range(1, int(audio_terminal["sequence"]) + 1))
            and actual.get("audioSequenceSeen") == audio_terminal["sequence"]
            and audio_terminal.get("error") is None
            and all(r["poll"]["sessionId"] == first.get("sessionId") for r in receipt_rows)
        )
        operations = Counter(r["Operation"] for r in audio_receipts)
        audio_ordered = True
        for receipt in audio_receipts:
            if receipt["Operation"] == "started":
                audio_balances[receipt["Cue"]] += 1
            elif receipt["Operation"] in ("finished", "stopped"):
                audio_balances[receipt["Cue"]] -= 1
                audio_ordered = audio_ordered and audio_balances[receipt["Cue"]] >= 0
        terminal_voices = Counter(s["cue"] for s in audio_terminal.get("sounds", []))
        if audio_terminal.get("musicPlaying"):
            terminal_voices[audio_terminal["musicCue"]] += 1
        audio_lifecycle = dict(
            operations=dict(operations),
            outstandingByCue=dict(+audio_balances),
            terminalVoices=dict(terminal_voices),
            terminalMusicPlaying=audio_terminal.get("musicPlaying"),
            reason="Cue totals classify starts/stops/finishes; wait tokens are not playback IDs. "
            "Same-cue overlapping voices are not assigned invented generations",
        )
        check(
            8,
            "actual audio receipt stream continuity",
            True,
            audio_contiguous,
            "audioReceipts/audioReceiptGaps/audioSequenceSeen/audioTerminal",
            parent=resource_parent,
            applicability="required-unobserved" if actual.get("audioReceiptGaps") else "applicable",
            missing_side="actual missing receipt ranges",
            reason="First sequence1 through terminal sequence; "
            "missing ranges remain observation failures",
        )
        check(
            8,
            "actual audio lifecycle balance",
            True,
            audio_ordered
            and +audio_balances == terminal_voices
            and not (set(operations) - {"started", "stopped", "finished", "fade-command"}),
            "audioReceipts[*].receipt.Operation/Cue + audioTerminal",
            parent=resource_parent,
            applicability="required-unobserved" if actual.get("audioReceiptGaps") else "applicable",
            missing_side="actual lifecycle across missed receipt ranges",
            reason="Actual cue totals reconcile with terminal voices; "
            "ongoing field music requires no fabricated end",
        )
    materials = reached_materials(actual, material_selection)
    for row in materials["checks"]:
        check(
            8,
            row["name"],
            True,
            row["value"],
            "explicit selected material inputs",
            original=dict(owner=OWNER, binding=row["source"]),
            parent=resource_parent,
            reason="Offline original material identity; "
            "natural dispatch and consumer remain separate",
        )
    if materials["actorWeapon"] is not None:
        check(
            8,
            "reached actor/weapon material subset",
            True,
            materials["actorWeapon"],
            "sceneObservations[*].scene.allyResource/enemyResource/weaponResource",
            parent=resource_parent,
            reason="Base42 original extraction; missing healing/death use keeps full family open",
        )
    for name, binding, actual_location, side in (
        (
            "reached map3/19/20/21/40/57 atlas and layer identities",
            "route/setup map identity -> admitted original map source/extractor -> actual "
            "atlas use",
            "samples[*].map/cameraProjection",
            "selected original asset/provenance to actual mount join",
        ),
        (
            "reached entity sprites/portraits/gesture resource identities",
            "story subject/sprite/portrait/gesture -> ROM/source selection -> actual node resource",
            "samples[*].entities/cameraProjection.actors/portraitProjection/presentation",
            "per-occurrence source asset and actual resource correspondence",
        ),
        (
            "displayed text tokens/font/glyph private binding",
            "story displayed text ID and control tokens -> admitted original text/font -> "
            "Label use",
            "samples[*].textId/fieldText/dialogue + scene.message",
            "private text/font provenance to actual displayed occurrence",
        ),
        (
            "scene actor/weapon/healing/death resources",
            "scenes actor/action/target selectors -> accepted scene source extraction -> "
            "mounted resource",
            "warpRecords[*].state.scene.allyResource/enemyResource/weaponResource/healing/fieldDeath",
            "healing/death bound raster identities and complete per-resource use/provenance join",
        ),
        (
            "scene background/ground actual resource identity",
            "original scene extraction -> selected raster -> mounted resource identity",
            "sceneObservations[*].scene.background/backgroundWrap/ground"
            if scene_rows
            else "warpRecords[*].state.scene.background/ground resource identity",
            "original selector/provenance to actual mounted ID join"
            if scene_rows
            else "actual resource ID (projection records positions but omits these IDs)",
        ),
        (
            "reached audio command/timer/PCM provenance and playback lifecycle",
            "pinned capture/cut/loop -> selected original PCM -> actual start/stop/finish",
            "audioReceipts/audioTerminal"
            if audio_terminal
            else "samples[*].audio.receipts + speechReceipts",
            "explicit selected world/library/capture-cut provenance join"
            if audio_contiguous
            else "complete reached audio/provenance join and exhaustive required playback receipts",
        ),
    ):
        material = None
        if name == "scene background/ground actual resource identity":
            material = materials["scene"]
        elif name == "reached audio command/timer/PCM provenance and playback lifecycle":
            material = (
                materials["audio"]
                and audio_contiguous
                and audio_ordered
                and +audio_balances == terminal_voices
            )
        if material is not None:
            check(
                8,
                name,
                True,
                material,
                actual_location,
                parent=resource_parent,
                original=dict(owner=OWNER, binding=binding),
                reason="Reached original material joins with actual use; natural original "
                "dispatch/mailbox/wait remains operation/consumer work",
            )
            continue
        missing(
            8,
            resource_parent,
            name,
            binding,
            actual_location,
            side,
            "Only reached resources are required; no all-corpus/frame inventory or hardware "
            "equality. Current selected content is not by itself an actual consumption record",
            observed=mounted_resources
            if scene_rows and name == "scene background/ground actual resource identity"
            else dict(contiguous=audio_contiguous, lifecycle=audio_lifecycle)
            if audio_terminal
            and name == "reached audio command/timer/PCM provenance and playback lifecycle"
            else None,
        )

    consumer_parent = "required unshimmed ack and scene consumer binding"
    join = plain_join_binding(
        ref,
        actual,
        original_join_evidence_root,
        material_selection[0] if material_selection else None,
    )
    for name, value, parent, layer in (
        ("bounded JOIN original witness binding", join["original"], consumer_parent, 9),
        ("bounded JOIN finite playback and previous restart", join["audio"], consumer_parent, 9),
        ("bounded JOIN dependent caller return", join["caller"], operation_parent, 7),
    ):
        check(
            layer,
            name,
            True,
            value,
            "samples/inputRecords/warpRecords/audioReceipts",
            original=dict(owner=OWNER, upstream=UPSTREAM, binding=join["anchors"]),
            parent=parent,
            reason="Only the sealed JOIN occurrence; missing selected evidence "
            "is unavailable. Original music completion remains Unknown",
        )
    for name, binding, actual_location, side, reason in (
        (
            "W1 displayed token occurrence/accepting read/service gates",
            "named text483 DisplayText -> symbol_wait1 -> loc_65B4 -> text return",
            "inputRecords Confirm/Wait + fieldText + text-w1-*",
            "remaining original occurrence/service-gate to actual consumer joins",
            "The accepted text483 witness proves that bounded seam; RTS/cursor alone cannot "
            "identify every required natural occurrence",
        ),
        (
            "W2 accepting read/validation indicator and token return",
            "source loc_6472 draw/copy/wait/read then sub_64A8 validation/clear -> token resume",
            "inputRecords + text-w2-* + audio validation receipt",
            "original exact W2 accepting read and actual occurrence correlation",
            "Text510/511 returns and W2 loop entry do not observe the later accepting read",
        ),
        (
            "plain JOIN input after matching finite completion",
            "natural WaitForPlayerInput input-first seam after declared modern music completion",
            "music-actual-completed/music-wait-returned + inputRecords + presentation-acknowledged",
            "named original plain-input occurrence to actual completion/ack join",
            "Plain input is separate from W1/W2; modern finite music is an accepted deviation",
        ),
        (
            "entity motion/gesture/fade consumer start/completion before resume",
            "reached story subject/wait -> real node use/completion -> dependent caller/input "
            "release",
            "samples[*].presentation/entities + warpRecords.result/state",
            "per-occurrence original wait dependency and actual completion identity",
            "A culled subject or request/return counter alone does not prove required delivery",
        ),
        (
            "battle scene command/resources/wait/effect/end consumer edges",
            "Initialize/Execute/End plus source command rules -> mounted action/target "
            "resources -> "
            "each required completion/ack -> committed effect -> field input",
            "scene phase/waitToken/resources/completed + scene-step-*/scene-ended + actors",
            "source dynamic command/operand mapping and actual per-occurrence completion joins",
            "Existing phase snapshots and prepare/end pairs do not alone bind every required "
            "animation/message/resource edge; no original frame/pixel equality is required",
        ),
        (
            "audio replacement/fade/stop/resume dependent consumer edges",
            "source reached audio command/timer and wait/replacement rule -> real player "
            "generation "
            "completion/stop -> matching release",
            "audio receipts/musicWait/token + inputRecords/warpRecords",
            "source wait dependencies and complete actual playback/generation correspondence",
            "Original mailbox dispatch is not playback; persistent music needs no invented end "
            "event",
        ),
    ):
        if (
            name == "plain JOIN input after matching finite completion"
            and join["plain"] is not None
        ):
            check(
                9,
                name,
                True,
                join["plain"],
                actual_location,
                original=dict(owner=OWNER, upstream=UPSTREAM, binding=join["anchors"]),
                parent=consumer_parent,
                reason=reason,
            )
            continue
        missing(9, consumer_parent, name, binding, actual_location, side, reason)

    check(
        10,
        MATRIX_OBLIGATION,
        "actual A-D matrix report",
        None,
        "coverage",
        missing_side="separate actual matrix report",
        reason="The matrix command closes only this obligation after all four reports compare",
    )
    check(
        10,
        "modern finite-music deviation declaration",
        "accepted-modern-finite-music-clock",
        "accepted-modern-finite-music-clock",
        "profile",
        reason="ADR0010; downstream historical mismatches retain unresolved attribution",
    )

    causal_records = all("resultStart" in r and "resultEnd" in r for r in input_rows) and all(
        "inputDelivery" in r for r in records
    )
    check(
        10,
        "observed input dispatch intervals",
        True,
        causal_records or None,
        "inputRecords[*]/warpRecords[*].inputDelivery",
        reason="Only synchronous dispatch results establish physical input consumption",
    )
    inputs = (
        [
            dict(
                action=r["action"],
                wait=r["before"].get("wait"),
                cursor=r["before"].get("cursor"),
                actor=r["before"].get("actor"),
            )
            for r in input_rows
            if r["pressed"]
            and any(
                x.get("inputDelivery")
                and x.get("inputOrdinal") == r["ordinal"]
                and x["result"]["boundary"] == "submit"
                for x in records[r.get("resultStart", 0) : r.get("resultEnd", 0)]
            )
        ]
        if causal_records
        else None
    )
    reveal_inputs = [
        r
        for r in input_rows
        if r["pressed"]
        and r["action"] == "confirm"
        and any(
            r["before"].get(visible, -1) >= 0 and r["before"][visible] < r["before"].get(total, 0)
            for visible, total in (
                ("visibleCharacters", "totalCharacters"),
                ("sceneVisibleCharacters", "sceneTotalCharacters"),
            )
        )
    ]
    if variant == "C":
        check(
            10,
            "actual reveal-only Confirm",
            True,
            bool(reveal_inputs)
            and all(
                r["resultStart"] == r["resultEnd"]
                and all(
                    r["before"].get(k) == r["after"].get(k)
                    for k in (
                        "simulationTick",
                        "mainSeed",
                        "thinkingSeed",
                        "actor",
                        "stage",
                        "wait",
                        "cursor",
                    )
                )
                and all(
                    r["after"].get(visible, -1) < 0
                    or r["after"][visible] >= r["after"].get(total, 0)
                    for visible, total in (
                        ("visibleCharacters", "totalCharacters"),
                        ("sceneVisibleCharacters", "sceneTotalCharacters"),
                    )
                )
                for r in reveal_inputs
            )
            if causal_records
            else None,
            "inputRecords:incomplete-text-confirm",
            reason="Actual incomplete Label becomes ready without submit or logical service",
        )
    elif variant == "D":
        check(
            10,
            "natural reveal before Confirm",
            True,
            not reveal_inputs if causal_records else None,
            "inputRecords:confirm.before",
            reason="Actual field/scene Label ready before every Confirm; no reveal-only press",
        )
    # Delivery notifications may interleave with mandatory work as reveal speed changes.
    # Keep logical operands ordered; retain and independently check host-only deliveries.
    semantic, deliveries, delivery_checks = [], [], []
    previous = None
    for index, row in enumerate(records):
        result, state = row["result"], row.get("state", {})
        observed = result.get("observations", [])
        if result["boundary"] == "submit":
            pure_scene = (
                len(observed) == 1
                and observed[0]["Kind"] == "scene-delivery"
                and row.get("inputDelivery") is False
            )
            for event in observed:
                if event["Kind"] == "text-revealed" or pure_scene:
                    unchanged = previous is not None and all(
                        previous.get(k) == state.get(k)
                        for k in ("simulationTick", "mainSeed", "thinkingSeed", "gold")
                    )
                    if pure_scene:
                        unchanged = unchanged and all(
                            v == gameplay(previous).get(k)
                            for k, v in gameplay(state).items()
                            if k not in ("revision", "sessionId")
                        )
                    delivery_checks.append(unchanged)
                    deliveries.append(dict(record=index, observation=event, unchanged=unchanged))
                else:
                    semantic.append(semantic_value(event))
        if state:
            previous = state
    check(
        10,
        "host delivery adds no gameplay or RNG service",
        True,
        bool(delivery_checks) and all(delivery_checks),
        "warpRecords:delivery-notifications",
        reason="Only text-revealed and single automatic scene-delivery notifications; "
        "acknowledgements and scene continuation remain ordered logical observations",
    )
    battle_states = [
        {k: v for k, v in gameplay(row["state"]).items() if k not in ("sessionId", "revision")}
        for row in outcome.get("records", [])
        if row.get("label") == "action-selected"
    ]
    coverage = []
    for name, children in obligations.items():
        required_children = [c for c in children if c["applicability"] != "historical-diagnostic"]
        child_counts = dict(Counter(child["result"] for child in required_children))
        parent_result = verdict(child_counts)
        coverage.append(
            dict(
                layer=children[0]["layer"],
                assertion=name,
                applicability="required-unobserved"
                if parent_result == "Unavailable"
                else "applicable",
                original=dict(
                    owner=OWNER, binding="required reached winning-profile child obligations"
                ),
                actual=dict(file=actual_path.as_posix(), record="assertions[parent=" + name + "]"),
                expected="all applicable required children PASS",
                actualValue=child_counts,
                result=parent_result,
                reason="Closed required child set is owned by the continuous contract; "
                "candidate input equality cannot substitute for missing actual/source bindings",
                children=[child["assertion"] for child in required_children],
                historicalChildren=[
                    child["assertion"]
                    for child in children
                    if child["applicability"] == "historical-diagnostic"
                ],
            )
        )
    required_rows = [a for a in assertions if a["applicability"] != "historical-diagnostic"]
    counts = dict(Counter(a["result"] for a in required_rows))
    result = verdict(counts)
    report = dict(
        profile="modern-continuous",
        variant=actual.get("h4Variant"),
        sourceCommit=SOURCE,
        originalIdentity=dict(
            rom=ref["romSha256"],
            upstream=ref["upstream"],
            extension=(ref.get("postVictoryInput") or {}).get("sourceCommit"),
        ),
        evidence=dict(
            originalJoinEvidenceRoot=(
                original_join_evidence_root.as_posix() if original_join_evidence_root else None
            ),
            materialSelection=(
                dict(
                    zip(
                        (
                            "world",
                            "scene",
                            "processReceipt",
                            "sceneEvidenceRoot",
                            "assetRoot",
                            "assetCommit",
                            "assetTree",
                            "assetManifestSha256",
                        ),
                        (v.as_posix() if isinstance(v, Path) else v for v in material_selection),
                        strict=True,
                    )
                )
                if material_selection
                else None
            ),
            actual=actual_path.as_posix(),
            outcome=outcome_path.as_posix(),
            settings=settings,
            hostExit=host_exit,
        ),
        assertions=assertions,
        coverageObligations=coverage,
        candidateDefinitions=candidate_definitions,
        actualObservations=dict(
            admissionInputConsistency=admission_identities,
            sceneRecords=len(scene_rows),
            visibleMountedResources=mounted_resources,
            completedSceneTokens=sorted(
                {
                    r["scene"]["waitToken"]
                    for r in scene_rows
                    if r["scene"].get("completed") and r["scene"].get("waitToken") is not None
                }
            ),
            audioReceiptCount=len(audio_receipts),
            audioContiguous=audio_contiguous,
            audioLifecycle=audio_lifecycle,
            reachedMaterialJoins=materials["joins"],
            plainJoinBinding=join,
        ),
        counts=counts,
        historicalCounts=dict(
            Counter(
                a["result"] for a in assertions if a["applicability"] == "historical-diagnostic"
            )
        ),
        result=result,
        milestonePass=result == "PASS",
        rejections=rejections,
        deliveryNotifications=deliveries,
        equivalence=dict(
            admission={
                k: first.get(k) for k in ("map", "party", "partyLists", "flags", "gold", "mainSeed")
            },
            inputs=inputs,
            observations=semantic,
            battleStates=battle_states,
            endpoints=paired,
            party=final.get("party"),
            gold=final.get("gold"),
            mainSeed=final.get("mainSeed"),
        ),
    )

    errors = modern_report_integrity(report, ref)
    require(not errors, "; ".join(errors))
    return report


def compare_matrix(paths, ref):
    reports = [read(p) for p in paths]
    variants = [r.get("variant") for r in reports]
    require(len(variants) == len(set(variants)), "duplicate matrix variant")
    required = ("A", "B", "C", "D")
    require(
        all(
            r.get("profile") == "modern-continuous" and r.get("variant") in required
            for r in reports
        ),
        "matrix requires named modern reports",
    )
    checks = []
    base = next((r for r in reports if r.get("variant") == "A"), None)
    for name in required:
        report = next((r for r in reports if r.get("variant") == name), None)
        settings = report["evidence"]["settings"] if report else {}
        expected = dict(
            confirmCancel="swapped" if name in "CD" else "standard",
            textMode="adjustable" if name in "CD" else "instant",
            reducedFlash=name in "CD",
            charactersPerSecond=20 if name in "CD" else 40,
        )
        observed = {k: settings.get(k) for k in expected}
        differences = []
        if report and base:
            for field, value in base["equivalence"].items():
                other = report["equivalence"].get(field)
                if value == other:
                    continue
                difference = dict(field=field)
                if isinstance(value, list) and isinstance(other, list):
                    index = next(
                        (
                            i
                            for i, pair in enumerate(zip(value, other, strict=False))
                            if pair[0] != pair[1]
                        ),
                        min(len(value), len(other)),
                    )
                    difference.update(
                        index=index,
                        expected=value[index] if index < len(value) else None,
                        actual=other[index] if index < len(other) else None,
                        expectedCount=len(value),
                        actualCount=len(other),
                    )
                else:
                    difference.update(expected=value, actual=other)
                differences.append(difference)
        result = (
            "Unavailable"
            if report is None
            or base is None
            or report["equivalence"].get("inputs") is None
            or base["equivalence"].get("inputs") is None
            else "PASS"
            if observed == expected and report["equivalence"] == base["equivalence"]
            else "FAIL"
        )
        checks.append(
            dict(
                variant=name,
                report=next(
                    (
                        p.as_posix()
                        for p, r in zip(paths, reports, strict=True)
                        if r.get("variant") == name
                    ),
                    None,
                ),
                result=result,
                expectedSettings=expected,
                actualSettings=observed,
                differences=differences,
                reason="Full named settings and semantic/state equivalence to A; "
                "missing variant is not a pair PASS",
            )
        )
    counts = dict(Counter(c["result"] for c in checks))
    integrity_errors = [modern_report_integrity(r, ref) for r in reports]
    inherited_fail = any(r.get("result") == "FAIL" for r in reports) or any(integrity_errors)
    matrix_complete = not counts.get("FAIL") and not counts.get("Unavailable")
    remaining = [
        dict(
            variant=r["variant"],
            coverageObligations=r.get("coverageObligations", []),
            assertions=[
                a
                for a in r["assertions"]
                if a["applicability"] != "historical-diagnostic"
                and a["result"] != "PASS"
                and not (matrix_complete and a["assertion"] == MATRIX_OBLIGATION)
            ],
            **({"integrityErrors": errors} if errors else {}),
        )
        for r, errors in zip(reports, integrity_errors, strict=True)
    ]
    incomplete = any(r["assertions"] for r in remaining) or any(
        p["result"] != "PASS" for r in reports for p in r.get("coverageObligations", [])
    )
    result = (
        "FAIL"
        if counts.get("FAIL") or inherited_fail
        else "Unavailable"
        if counts.get("Unavailable") or incomplete
        else "PASS"
    )
    return dict(
        profile="modern-continuous-matrix",
        variants=checks,
        counts=counts,
        result=result,
        milestonePass=result == "PASS",
        remaining=remaining,
        requiredReports=[
            dict(variant=r["variant"], result=r["result"], counts=r["counts"]) for r in reports
        ],
    )


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("mode", choices=("plan", "compare", "matrix"))
    parser.add_argument("--profile", choices=("legacy", "modern-continuous"), default="legacy")
    parser.add_argument("--reference", required=True, type=Path)
    parser.add_argument("--output", required=True, type=Path)
    parser.add_argument("--evidence-root", type=Path)
    parser.add_argument("--plan", type=Path)
    parser.add_argument("--actual", type=Path)
    parser.add_argument("--host-log", type=Path)
    parser.add_argument("--host-exit", type=int)
    parser.add_argument("--outcome", type=Path)
    parser.add_argument("--settings", type=Path)
    parser.add_argument("--variant-report", type=Path, action="append", default=[])
    parser.add_argument("--baseline-actual", type=Path)
    parser.add_argument("--baseline-outcome", type=Path)
    parser.add_argument(
        "--controlled-start",
        type=Path,
        help="Explicit candidate party definition; not a same-run admission snapshot",
    )
    parser.add_argument("--selected-world", type=Path)
    parser.add_argument("--original-join-evidence-root", type=Path)
    parser.add_argument("--selected-scene", type=Path)
    parser.add_argument("--process-receipt", type=Path)
    parser.add_argument("--scene-evidence-root", type=Path)
    parser.add_argument("--asset-root", type=Path)
    parser.add_argument("--expected-asset-commit")
    parser.add_argument("--expected-asset-tree")
    parser.add_argument("--expected-asset-manifest-sha256")
    args = parser.parse_args()
    material_selection = (
        args.selected_world,
        args.selected_scene,
        args.process_receipt,
        args.scene_evidence_root,
        args.asset_root,
        args.expected_asset_commit,
        args.expected_asset_tree,
        args.expected_asset_manifest_sha256,
    )
    require(
        not any(material_selection) or all(material_selection),
        "material comparison requires all explicit selections and asset pins",
    )
    material_selection = material_selection if all(material_selection) else None
    ref = reference(args.reference)
    if args.mode == "matrix":
        require(args.variant_report, "matrix requires actual variant reports")
        result = compare_matrix(args.variant_report, ref)
        write(args.output, result)
        print(json.dumps({k: result[k] for k in ("result", "counts", "milestonePass")}))
        raise SystemExit(
            1 if result["result"] == "FAIL" else 2 if result["result"] == "Unavailable" else 0
        )
    if args.mode == "plan":
        require(args.evidence_root is not None, "plan requires accepted evidence root")
        result = make_plan(ref, args.evidence_root)
        write(args.output, result)
        print(
            json.dumps(
                {
                    "logicalFieldInputs": len(result["steps"]),
                    "firstDecisionInputs": len(result["firstDecisionInputs"]),
                }
            )
        )
    else:
        if args.profile == "modern-continuous":
            require(
                all(
                    x is not None
                    for x in (
                        args.actual,
                        args.outcome,
                        args.settings,
                        args.host_log,
                        args.host_exit,
                    )
                ),
                "modern comparison requires actual/outcome/settings/host-log/recorded exit",
            )
            result = compare_modern(
                ref,
                args.actual,
                args.outcome,
                args.settings,
                args.host_log,
                args.host_exit,
                args.baseline_actual,
                args.baseline_outcome,
                args.controlled_start,
                material_selection,
                args.original_join_evidence_root,
            )
            write(args.output, result)
            print(json.dumps({k: result[k] for k in ("result", "counts", "milestonePass")}))
            raise SystemExit(
                1 if result["result"] == "FAIL" else 2 if result["result"] == "Unavailable" else 0
            )
        require(
            args.plan is not None
            and args.actual is not None
            and args.host_log is not None
            and args.host_exit is not None,
            "compare requires plan, actual, host-log and the recorded host-exit",
        )
        result = compare(ref, read(args.plan), args.actual, args.host_log, args.host_exit)
        write(args.output, result)
        print(
            json.dumps(
                {"result": result["result"], "counts": result["counts"], "milestonePass": False}
            )
        )
        raise SystemExit(1 if result["result"] == "FAIL" else 2)


if __name__ == "__main__":
    main()
