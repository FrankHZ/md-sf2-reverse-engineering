from __future__ import annotations

from pathlib import Path
from typing import Any

from sf2tool.h3.bizhawk import run_observer, verify_runtime_contract
from sf2tool.h3.growth import _rng_step, _verify_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

FIXTURE = repo_path("tests/fixtures/h3/battle-ai-action-choice-v1.json")
SCHEMA = repo_path("schemas/h3-battle-ai-action-choice-fixture.schema.json")
OBSERVER = repo_path("tools/bizhawk/battle_ai_action_choice_observer.lua")


def _thinking_rng_step(seed: int, range_: int) -> tuple[int, int]:
    while True:
        seed = (seed * 541 + 12345) & 0xFF
        if range_ <= 1 or seed < range_:
            return seed, 0 if range_ <= 1 else seed


def _model_case(case: dict[str, Any]) -> dict[str, Any]:
    attack, spell, item = (len(case[name]["targets"]) > 0 for name in ("attack", "spell", "item"))
    roll, final_seed = -1, case["seed"]
    final_thinking_seed = case["thinkingSeed"]
    if not (attack or spell or item):
        action = 3
    elif not (spell or item):
        action = 0
    elif spell and item:
        final_thinking_seed, roll = _thinking_rng_step(case["thinkingSeed"], 2)
        action = 2 if roll == 1 else 1
    elif spell:
        if case["spellEntry"] == 40:
            action = 1
        else:
            final_seed, roll = _rng_step(case["seed"], 6)
            action = 1 if roll in (2, 4) or not attack else 0
    else:
        final_seed, roll = _rng_step(case["seed"], 6)
        action = 2 if roll in (3, 5) or not attack else 0
    selected = (case["attack"], case["spell"], case["item"])[action] if action < 3 else None
    if selected is None:
        target, priority = -1, 0
    else:
        maximum = max(selected["priorities"])
        priority = min(maximum, 15)
        candidates = [
            (target, movement)
            for target, movement, value in zip(
                selected["targets"], selected["movements"], selected["priorities"], strict=True
            )
            if value == maximum
        ][::-1]
        target = candidates[0][0]
        if len(candidates) > 1:
            best = -1
            for candidate, movement in candidates:
                if best <= movement:
                    best, target = movement, candidate
    return {
        "id": case["id"],
        "seed": case["seed"],
        "thinkingSeed": case["thinkingSeed"],
        "roll": roll,
        "finalSeed": final_seed,
        "finalThinkingSeed": final_thinking_seed,
        "target": target,
        "priority": priority,
        "action": action,
    }


def verify_battle_ai_action_choice(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 120
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner="battle AI action-choice fixture")
    verify_runtime_contract(fixture, rom_path)
    disasm = _verify_upstream(upstream_path)
    source = (
        disasm / "code/gameflow/battle/ai/command/attack/determinebattleaction.asm"
    ).read_text(encoding="utf-8")
    if (
        "DetermineBattleactionForAttackAiCommand:" not in source
        or "jsr     j_GenerateRandomNumberUnderD6" not in source
    ):
        raise ValueError("battle AI action-choice source contract drift")
    modeled = [_model_case(case) for case in fixture["cases"]]
    observed = run_observer(
        rom_path=rom_path,
        observer_path=OBSERVER,
        config={
            "fixtureId": fixture["id"],
            "battleId": fixture["battleId"],
            "function": fixture["function"],
            "ram": fixture["ram"],
            "harness": load_json(repo_path(fixture["sharedHarnessFixture"]))["harness"],
            "cases": fixture["cases"],
        },
        output_name="battle-ai-action-choice",
        timeout_seconds=timeout_seconds,
    )
    expected = {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["id"],
        "battle": fixture["battleId"],
        "records": modeled,
    }
    if observed != expected:
        raise ValueError(
            "battle AI action-choice runtime mismatch\n"
            f"expected={expected!r}\nobserved={observed!r}"
        )
    return {
        "Fixture": fixture["id"],
        "Cases": len(modeled),
        "Actions": ",".join(str(row["action"]) for row in modeled),
        "Status": "PASS",
    }
