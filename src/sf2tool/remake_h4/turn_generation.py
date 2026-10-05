"""Compose executed generation evidence with independent source expectations."""

import subprocess
from pathlib import Path

from sf2tool.paths import repo_path
from sf2tool.remake_h4.turn_generation_candidates import compare_candidates
from sf2tool.remake_h4.turn_generation_checks import GenerationChecks, merge
from sf2tool.remake_h4.turn_generation_draws import compare_draws
from sf2tool.remake_h4.turn_generation_scores import compare_scores
from sf2tool.remake_h4.turn_source import _source_turn_order
from sf2tool.remake_h4_reference import UPSTREAM


def turn_order_binding(actual, source_root):
    """Bind a selected completed Application generation to source-derived rules.

    The independently read state roster defines candidate coverage. Generation
    payloads are actual one-execution facts, not expected values. This controlled
    scope cannot close the historical A child that lacks those operands.
    """
    result = dict(
        value=None,
        checks=[],
        rounds=[],
        evidenceScope=actual.get("evidenceScope"),
        sourceRules=dict(
            upstream=UPSTREAM,
            owner="docs/design/contracts/battle-control-lifecycle.md",
            symbol="GenerateBattleTurnOrder/AddCombatantAndRandomizedAgiToTurnOrder",
            fixtures=[
                "tests/fixtures/h3/battle01-turn-order-v1.json",
                "tests/fixtures/h3/turn-order-boundaries-v1.json",
            ],
        ),
    )

    checks = GenerationChecks()
    result["checks"] = checks.rows
    check, match = checks.check, checks.match
    ordered_records_match = checks.ordered_records_match

    def finish():
        result["value"] = merge([c["value"] for c in result["checks"]])
        return result

    selected = actual.get("rounds")
    if not selected:
        check(
            "completed generation/state operands absent; historical aggregate is insufficient", None
        )
        return finish()
    check(
        "controlled Application scope", match("controlled-application", actual.get("evidenceScope"))
    )
    try:
        root = Path(source_root) if source_root is not None else None
        if root is None:
            raise ValueError("no pinned source")
        root = root.resolve() if root.is_absolute() else repo_path(root)
        pin = subprocess.check_output(
            ["git", "-C", str(root), "rev-parse", "HEAD"], text=True
        ).strip()
        clean = subprocess.run(
            ["git", "-C", str(root), "diff", "--quiet", UPSTREAM, "--", "disasm"], check=False
        ).returncode
        check("pinned clean original rules", pin == UPSTREAM and clean == 0)
    except (OSError, ValueError, subprocess.SubprocessError):
        check("pinned original rules unavailable", None)
    rounds = actual.get("selectedRounds")
    check(
        "independent selected round inventory",
        None
        if rounds is None
        else isinstance(rounds, list)
        and bool(rounds)
        and all(type(r) is int and r > 0 for r in rounds)
        and len(set(rounds)) == len(rounds),
    )
    session = actual.get("sessionId")
    check(
        "selected session identity available",
        True if isinstance(session, str) and session else None,
    )
    seen, sequences = [], []
    for ordinal, row in enumerate(selected):
        state, event = row.get("state") or {}, row.get("event") or {}
        generation = event.get("TurnGeneration") or {}
        identity = dict(ordinal=ordinal, round=state.get("round"))
        seen.append(state.get("round"))
        if event.get("Sequence") is not None:
            sequences.append(event["Sequence"])
        check(
            "generation joins state session/round and aggregate seed edge",
            merge(
                [
                    match(session, state.get("sessionId")) if session else None,
                    match(session, generation.get("SessionId")) if session else None,
                    match(state.get("round"), generation.get("Round"))
                    if state.get("round") is not None
                    else None,
                    match("round-rng", event.get("Kind")),
                    match(event.get("Before"), generation.get("Before"))
                    if event.get("Before") is not None
                    else None,
                    match(event.get("After"), generation.get("After"))
                    if event.get("After") is not None
                    else None,
                    match(state.get("mainSeed"), generation.get("After"))
                    if state.get("mainSeed") is not None
                    else None,
                ]
            ),
            **identity,
        )
        for event_key, state_key in (("Revision", "revision"), ("Sequence", "observationSequence")):
            old, new = event.get(event_key), state.get(state_key)
            check(
                "generation precedes its state " + event_key,
                None
                if old is None or new is None
                else type(old) is int and type(new) is int and 0 <= old <= new,
                **identity,
            )
        candidates, valid, by_actor = compare_candidates(state, generation, identity, checks)
        compare_draws(generation, candidates, by_actor, identity, checks)
        compare_scores(generation, by_actor, identity, checks)
        if not candidates or merge(valid) is not True:
            check("source score/order expectation requires matched live operands", None, **identity)
            continue
        before = generation.get("Before")
        check(
            "entry seed image available",
            None if before is None else type(before) is int and 0 <= before <= 0xFFFFFFFF,
            **identity,
        )
        if type(before) is not int or not 0 <= before <= 0xFFFFFFFF:
            continue
        capacity = sum(
            2 if c["ExtraRoundAction"] else 1 for c in candidates if c["Placed"] and c["Hp"] > 0
        )
        check("admitted candidate buffer capacity", capacity <= 64, **identity)
        if capacity > 64:
            continue
        expected = _source_turn_order(candidates, before)
        for key in ("Candidates", "Draws", "Unsorted", "Sorted", "After"):
            value = (
                ordered_records_match(
                    expected[key],
                    generation.get(key),
                    ("Actor",) if key == "Candidates" else ("Actor", "Turn", "Index"),
                )
                if key in ("Candidates", "Draws")
                else match(expected[key], generation.get(key))
            )
            check("source-derived " + key, value, **identity)
        check(
            "actual sorted buffer controls state queue",
            match(expected["Sorted"], state.get("turnOrder")),
            **identity,
        )
        result["rounds"].append(dict(**identity, expected=expected))
    present = [r for r in seen if r is not None]
    coverage = None
    if isinstance(rounds, list) and all(type(r) is int for r in rounds + present):
        coverage = (
            False if set(present) - set(rounds) else None if set(rounds) - set(present) else True
        )
    check(
        "duplicate/foreign/omitted round joins",
        merge(
            [
                len(present) == len(set(present)),
                len(sequences) == len(set(sequences)),
                coverage,
                None if None in seen else True,
            ]
        ),
    )
    return finish()
