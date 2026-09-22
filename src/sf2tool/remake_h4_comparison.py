"""Compare the accepted original route with actual ordinary-host H4 observations.

This bounded driver reaches first control and diagnoses the first STAY/next actor.
It cannot certify the complete battle, endpoint, presentation or accessibility.
Use the accepted read-only reference projector first. All outputs remain private.
"""

from __future__ import annotations

import argparse
import json
from bisect import bisect_right
from collections import Counter
from pathlib import Path

from sf2tool.paths import repo_path
from sf2tool.remake_h4_reference import ROM, SOURCE, UPSTREAM, location, require, rows

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


def compare(ref, plan, actual_path, host_log):
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
        check(
            1,
            f"ally-{ally['id']}.items",
            [x["raw"] for x in ally["items"]],
            actual.get("SourceLoadout"),
            admission,
            original["source"],
            "Admission SourceLoadout is null; later inventories cannot fill this boundary",
            True,
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
                int(flag) in state.get("flags", []),
                sample,
                step.get("beforeSource", step["source"]),
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
    check(2, "host-process-errors", [], errors, last, {}, "Inspect actual Godot process errors")
    for group in ref["coverage"]:
        if "RA-12" in group["fields"]:
            check(
                7,
                group["fields"],
                {"map": 57, "input": "Down", "from": [5, 12], "to": [5, 13]},
                None,
                last,
                {
                    "owner": "docs/research/map3-messenger-acceptance.md"
                    "#native-post-victory-ordinary-input-result-issue-515",
                    "commit": "26e9d3ab988bd4440146eec8da2e7a97a53dffa8",
                },
                "Accepted original extension exists; old projector has no binding and actual "
                "run did not reach this boundary. No new-chain payload was consumed",
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
        remainder="Battle/return/endpoint and 9A variants not established; "
        "diagnostic extension stops at actor divergence",
    )


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("mode", choices=("plan", "compare"))
    parser.add_argument("--reference", required=True, type=Path)
    parser.add_argument("--output", required=True, type=Path)
    parser.add_argument("--evidence-root", type=Path)
    parser.add_argument("--plan", type=Path)
    parser.add_argument("--actual", type=Path)
    parser.add_argument("--host-log", type=Path)
    args = parser.parse_args()
    ref = reference(args.reference)
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
        require(
            args.plan is not None and args.actual is not None and args.host_log is not None,
            "compare requires plan, actual and host-log",
        )
        result = compare(ref, read(args.plan), args.actual, args.host_log)
        write(args.output, result)
        print(
            json.dumps(
                {"result": result["result"], "counts": result["counts"], "milestonePass": False}
            )
        )
        raise SystemExit(1 if result["result"] == "FAIL" else 2)


if __name__ == "__main__":
    main()
