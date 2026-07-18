from __future__ import annotations

import re
from pathlib import Path
from typing import Any

from sf2tool.h3.bizhawk import run_observer, verify_runtime_contract
from sf2tool.h3.growth import _verify_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

FIXTURE = repo_path("tests/fixtures/h3/muddle-confusion-v1.json")
SCHEMA = repo_path("schemas/h3-muddle-confusion-fixture.schema.json")
OBSERVER = repo_path("tools/bizhawk/muddle_confusion_observer.lua")


def _equate(source: str, name: str) -> int:
    match = re.search(
        rf"^{re.escape(name)}:\s+equ\s+(\$?)([0-9A-F]+)(?:\s|$)",
        source,
        re.MULTILINE,
    )
    if not match:
        raise ValueError(f"missing pinned equate: {name}")
    return int(match.group(2), 16 if match.group(1) else 10)


def _verify_source_contract(disasm: Path) -> tuple[int, int]:
    enums = (disasm / "sf2enums.asm").read_text(encoding="utf-8")
    counter_mask = _equate(enums, "STATUSEFFECT_MUDDLE")
    level2_flag = _equate(enums, "STATUSEFFECT_MUDDLE2")
    source = (disasm / "code/gameflow/battle/ai/iscombatantconfused.asm").read_text(
        encoding="utf-8"
    )
    required = (
        "IsCombatantConfused:",
        "andi.w  #STATUSEFFECT_MUDDLE,d1",
        "beq.s   @NotMuddled1",
        "andi.w  #STATUSEFFECT_MUDDLE2,d1",
        "beq.s   @NotMuddled2",
        "move.w  #1,d1",
    )
    if any(fragment not in source for fragment in required):
        raise ValueError("MUDDLE confusion source contract drifted")
    return counter_mask, level2_flag


def _model_cases(
    cases: list[dict[str, Any]], counter_mask: int, level2_flag: int
) -> list[dict[str, Any]]:
    return [
        {
            "id": case["id"],
            "status": case["status"],
            "confused": int(
                bool(case["status"] & counter_mask)
                and bool(case["status"] & level2_flag)
            ),
        }
        for case in cases
    ]


def verify_muddle_confusion(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 75
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner="MUDDLE confusion fixture")
    verify_runtime_contract(fixture, rom_path)
    shared = load_json(repo_path(fixture["sharedHarnessFixture"]))
    masks = _verify_source_contract(_verify_upstream(upstream_path))
    modeled = _model_cases(fixture["cases"], *masks)
    for case, record in zip(fixture["cases"], modeled, strict=True):
        if record["confused"] != case["expectedConfused"]:
            raise ValueError(f"MUDDLE confusion golden disagrees: {case['id']}")
    observed = run_observer(
        rom_path=rom_path,
        observer_path=OBSERVER,
        config={
            "fixtureId": fixture["id"],
            "function": {**shared["function"], **fixture["function"]},
            "ram": {**shared["ram"], **fixture["ram"]},
            "harness": shared["harness"],
            "cases": fixture["cases"],
        },
        output_name="muddle-confusion",
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
            "MUDDLE confusion runtime observation mismatch\n"
            f"expected={expected!r}\nobserved={observed!r}"
        )
    return {
        "Fixture": fixture["id"],
        "Cases": len(modeled),
        "Results": ",".join(str(record["confused"]) for record in modeled),
        "RequiredState": f"0x{masks[0] | masks[1]:04X}",
        "Status": "PASS",
    }
