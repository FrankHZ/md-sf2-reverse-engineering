from __future__ import annotations

from pathlib import Path
from typing import Any

from sf2tool.h3.bizhawk import bizhawk_contract, run_observer
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path
from sf2tool.rom import inspect_rom

BASE_FIXTURE = repo_path("tests/fixtures/h3/rng-v1.json")
BASE_SCHEMA = repo_path("schemas/h3-rng-fixture.schema.json")
DEBUG_FIXTURE = repo_path("tests/fixtures/h3/debug-rng-v1.json")
DEBUG_SCHEMA = repo_path("schemas/h3-debug-rng-fixture.schema.json")
OBSERVER = repo_path("tools/bizhawk/rng_observer.lua")


def _verify_contract(fixture: dict[str, Any], rom_path: Path) -> None:
    actual_hash = inspect_rom(rom_path.resolve(strict=True))["sha256"]
    if actual_hash != fixture["romSha256"]:
        raise ValueError(
            f"H3 RNG fixture ROM mismatch: expected {fixture['romSha256']}, got {actual_hash}"
        )
    bizhawk, _ = bizhawk_contract()
    emulator = fixture["emulator"]
    if (
        emulator["name"] != "BizHawk"
        or emulator["version"] != bizhawk["release"]
        or emulator["core"] != bizhawk["core"]
    ):
        raise ValueError("H3 RNG fixture execution-engine contract mismatch")


def _assert_observation(
    fixture: dict[str, Any], observed: dict[str, Any], *, debug: bool
) -> None:
    expected_core = fixture["emulator"]["core"]
    if observed.get("system") != "GEN" or observed.get("core") != expected_core:
        raise ValueError("unexpected H3 RNG execution system or core")
    if len(observed.get("results", [])) != len(fixture["cases"]):
        raise ValueError("H3 RNG observation count mismatch")
    for expected, actual in zip(fixture["cases"], observed["results"], strict=True):
        expected_values = {
            "id": expected["id"],
            "seed": expected["seed"],
            "entryRange": expected["range"],
            "observedSeed": expected["expectedSeed"],
            "observedValue": expected["expectedValue"],
        }
        failures = [
            f"{key}: expected {value}, got {actual.get(key)}"
            for key, value in expected_values.items()
            if actual.get(key) != value
        ]
        if debug and (actual.get("d6After"), actual.get("d7After")) != (
            actual.get("d6Before"),
            actual.get("d7Before"),
        ):
            failures.append("D6/D7 were not preserved across the debug-aware wrapper")
        if not debug and actual.get("d6After") != actual.get("d6Before"):
            failures.append("D6 range register was not restored by the base RNG")
        if debug and actual.get("rngFallback") != expected["rngConsumed"]:
            failures.append(
                f"RNG fallback: expected {expected['rngConsumed']}, "
                f"got {actual.get('rngFallback')}"
            )
        if failures:
            raise ValueError(f"H3 RNG mismatch for {expected['id']}: " + "; ".join(failures))


def _rng_step(seed: int, range_: int) -> tuple[int, int]:
    updated = (seed * 13 + 7) & 0xFFFF
    return updated, (updated * range_) // 0x10000


def _verify_expected_models(base: dict[str, Any], debug: dict[str, Any]) -> None:
    for case in base["cases"]:
        seed, value = _rng_step(case["seed"], case["range"])
        if (seed, value) != (case["expectedSeed"], case["expectedValue"]):
            raise ValueError(f"base RNG golden disagrees with the static model: {case['id']}")

    masks = debug["inputMasks"]
    priorities = (
        (masks["right"], 0),
        (masks["up"], 1),
        (masks["left"], 2),
        (masks["down"], 3),
    )
    for case in debug["cases"]:
        override = next(
            (value for mask, value in priorities if case["inputMask"] & mask), None
        )
        consumed = not case["debugEnabled"] or override is None
        if consumed:
            seed, value = _rng_step(case["seed"], case["range"])
        else:
            seed, value = case["seed"], override
        if (consumed, seed, value) != (
            case["rngConsumed"],
            case["expectedSeed"],
            case["expectedValue"],
        ):
            raise ValueError(
                f"debug-aware RNG golden disagrees with the static model: {case['id']}"
            )


def _base_config(fixture: dict[str, Any]) -> dict[str, Any]:
    function = fixture["function"]
    return {
        "mode": "base",
        "entryAddress": function["entryAddress"],
        "returnAddress": function["observeAddress"],
        "seedAddress": function["seedAddress"],
        "rangeRegister": function["rangeRegister"],
        "resultRegister": function["seedRegister"],
        "cases": fixture["cases"],
    }


def _debug_config(fixture: dict[str, Any]) -> dict[str, Any]:
    function = fixture["function"]
    ram = fixture["ram"]
    return {
        "mode": "debug",
        "entryAddress": function["entryAddress"],
        "returnAddress": function["returnAddress"],
        "fallbackAddress": function["rngFallbackAddress"],
        "seedAddress": ram["seedAddress"],
        "debugModeAddress": ram["debugModeToggleAddress"],
        "playerInputAddress": ram["playerOneInputAddress"],
        "rangeRegister": function["rangeRegister"],
        "resultRegister": function["resultRegister"],
        "cases": fixture["cases"],
    }


def observe_debug_cases(
    rom_path: Path, cases: list[dict[str, Any]], *, timeout_seconds: int = 60
) -> dict[str, Any]:
    """Exploration helper: observe debug-wrapper cases before a golden is committed."""
    fixture = {
        "function": {
            "entryAddress": 0x1674,
            "returnAddress": 0x16BC,
            "rngFallbackAddress": 0x16B2,
            "rangeRegister": "M68K D0",
            "resultRegister": "M68K D0",
        },
        "ram": {
            "debugModeToggleAddress": 0xFFB0A9,
            "playerOneInputAddress": 0xFFDE97,
            "seedAddress": 0xFFDEA4,
        },
        "cases": cases,
    }
    return run_observer(
        rom_path=rom_path,
        observer_path=OBSERVER,
        config=_debug_config(fixture),
        output_name="debug-rng-exploration",
        timeout_seconds=timeout_seconds,
    )


def verify_rng(rom_path: Path, *, timeout_seconds: int = 60) -> dict[str, Any]:
    base = load_json(BASE_FIXTURE)
    debug = load_json(DEBUG_FIXTURE)
    validate_json(base, BASE_SCHEMA, owner=str(BASE_FIXTURE))
    validate_json(debug, DEBUG_SCHEMA, owner=str(DEBUG_FIXTURE))
    _verify_expected_models(base, debug)
    _verify_contract(base, rom_path)
    _verify_contract(debug, rom_path)
    base_observed = run_observer(
        rom_path=rom_path,
        observer_path=OBSERVER,
        config=_base_config(base),
        output_name="base-rng",
        timeout_seconds=timeout_seconds,
    )
    _assert_observation(base, base_observed, debug=False)
    debug_observed = run_observer(
        rom_path=rom_path,
        observer_path=OBSERVER,
        config=_debug_config(debug),
        output_name="debug-rng",
        timeout_seconds=timeout_seconds,
    )
    _assert_observation(debug, debug_observed, debug=True)
    return {
        "BaseFixture": base["id"],
        "DebugFixture": debug["id"],
        "Engine": f"BizHawk {base['emulator']['version']} / {base['emulator']['core']}",
        "BaseCases": len(base["cases"]),
        "DebugCases": len(debug["cases"]),
        "Status": "PASS",
    }
