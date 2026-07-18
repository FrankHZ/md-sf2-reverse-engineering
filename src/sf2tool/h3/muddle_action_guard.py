from __future__ import annotations

import hashlib
from pathlib import Path
from typing import Any

from sf2tool.h3.bizhawk import (
    DERIVED_ROOT,
    bizhawk_contract,
    run_observer,
    verify_runtime_contract,
)
from sf2tool.h3.growth import _rng_step, _verify_upstream
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path
from sf2tool.rom import mega_drive_checksum

FIXTURE = repo_path("tests/fixtures/h3/muddle-action-guard-v1.json")
SCHEMA = repo_path("schemas/h3-muddle-action-guard-fixture.schema.json")
OBSERVER = repo_path("tools/bizhawk/muddle_action_guard_observer.lua")


def _verify_source_contract(disasm: Path) -> None:
    source = (
        disasm
        / "code/gameflow/battle/ai/command/attack/determinemuddledbattleaction.asm"
    ).read_text(encoding="utf-8")
    required = (
        "DetermineMuddledBattleaction:",
        "btst    #COMBATANT_BIT_ENEMY,d0",
        "cmpi.b  #COMBATANT_ALLIES_START,d1",
        "cmpi.b  #COMBATANT_ENEMIES_START,d1",
        "cmp.b   d0,d1",
        "moveq   #2,d0",
        "jsr     (GenerateRandomOrDebugNumber).w",
        "moveq   #1,d3",
    )
    if any(fragment not in source for fragment in required):
        raise ValueError("MUDDLE action-guard source contract drifted")


def _model_cases(cases: list[dict[str, Any]]) -> list[dict[str, Any]]:
    records = []
    for case in cases:
        rng_calls = 0
        roll = -1
        protected_target = 0x80 if case["actor"] & 0x80 else 0
        if case["target"] == protected_target:
            inaction = 1
        elif case["target"] != case["actor"]:
            inaction = 0
        else:
            rng_calls = 1
            _, roll = _rng_step(case["seed"], 2)
            inaction = int(roll != 0)
        records.append(
            {
                "id": case["id"],
                "actor": case["actor"],
                "target": case["target"],
                "seed": case["seed"],
                "rngCalls": rng_calls,
                "roll": roll,
                "inaction": inaction,
            }
        )
    return records


def _instrument_rom(rom_path: Path, instrumentation: dict[str, Any]) -> Path:
    data = bytearray(rom_path.read_bytes())
    call_site = instrumentation["callSiteAddress"]
    stub_address = instrumentation["stubAddress"]
    original_call = bytes.fromhex(instrumentation["callSiteOriginalHex"])
    original_stub = bytes.fromhex(instrumentation["stubOriginalHex"])
    patched_call = bytes.fromhex(instrumentation["callSitePatchedHex"])
    if data[call_site : call_site + len(original_call)] != original_call:
        raise ValueError("MUDDLE action-guard call-site bytes drifted")
    if data[stub_address : stub_address + len(original_stub)] != original_stub:
        raise ValueError("MUDDLE action-guard padding bytes drifted")
    displacement = stub_address - (call_site + 2)
    if not -0x8000 <= displacement <= 0x7FFF:
        raise ValueError("MUDDLE action-guard instrumentation stub is out of BSR range")
    expected_call = b"\x61\x00" + displacement.to_bytes(
        2, "big", signed=True
    )
    if patched_call != expected_call:
        raise ValueError("MUDDLE action-guard patched BSR target drifted")
    stub = bytes.fromhex(instrumentation["stubHex"])
    data[call_site : call_site + len(patched_call)] = patched_call
    data[stub_address : stub_address + len(stub)] = stub
    data[0x18E:0x190] = int(mega_drive_checksum(bytes(data)), 16).to_bytes(2, "big")
    DERIVED_ROOT.mkdir(parents=True, exist_ok=True)
    output = DERIVED_ROOT / "muddle-action-guard.instrumented.bin"
    output.write_bytes(data)
    return output


def verify_muddle_action_guard(
    rom_path: Path, upstream_path: Path, *, timeout_seconds: int = 120
) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner="MUDDLE action-guard fixture")
    verify_runtime_contract(fixture, rom_path)
    shared = load_json(repo_path(fixture["sharedHarnessFixture"]))
    _verify_source_contract(_verify_upstream(upstream_path))
    instrumented_rom = _instrument_rom(rom_path, fixture["instrumentation"])
    modeled = _model_cases(fixture["cases"])
    for case, record in zip(fixture["cases"], modeled, strict=True):
        expected = (
            case["expectedRngCalls"],
            case["expectedRoll"],
            case["expectedInaction"],
        )
        actual = (record["rngCalls"], record["roll"], record["inaction"])
        if actual != expected:
            raise ValueError(f"MUDDLE action-guard golden disagrees: {case['id']}")
    _, executable = bizhawk_contract()
    user_db = executable.parent / "gamedb" / "gamedb_user.txt"
    prior_user_db = user_db.read_bytes() if user_db.exists() else None
    md5 = hashlib.md5(instrumented_rom.read_bytes()).hexdigest().upper()
    prior_text = prior_user_db.decode("utf-8") if prior_user_db is not None else ""
    separator = "" if not prior_text or prior_text.endswith("\n") else "\n"
    user_db.write_text(
        f"{prior_text}{separator}{md5}\t\tSF2 H3 instrumented action guard\tGEN\n",
        encoding="utf-8",
    )
    try:
        observed = run_observer(
            rom_path=instrumented_rom,
            observer_path=OBSERVER,
            config={
                "fixtureId": fixture["id"],
                "function": {**shared["function"], **fixture["function"]},
                "ram": {**shared["ram"], **fixture["ram"]},
                "harness": shared["harness"],
                "instrumentation": fixture["instrumentation"],
                "cases": fixture["cases"],
            },
            output_name="muddle-action-guard",
            timeout_seconds=timeout_seconds,
        )
    finally:
        if prior_user_db is None:
            user_db.unlink(missing_ok=True)
        else:
            user_db.write_bytes(prior_user_db)
    expected = {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["id"],
        "battle": fixture["battleId"],
        "records": modeled,
    }
    if observed != expected:
        raise ValueError(
            "MUDDLE action-guard runtime observation mismatch\n"
            f"expected={expected!r}\nobserved={observed!r}"
        )
    return {
        "Fixture": fixture["id"],
        "Cases": len(modeled),
        "Inaction": ",".join(str(record["inaction"]) for record in modeled),
        "SelfRolls": ",".join(
            str(record["roll"]) for record in modeled if record["rngCalls"]
        ),
        "Status": "PASS",
    }
