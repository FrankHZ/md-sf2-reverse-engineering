"""One-launch, post-start RAM-probe observation of the original random services.

The probe is deliberately smaller than a game scenario: it calls unmodified ROM
services from a verified work-RAM probe and records their entry, generator,
return, helper-return seed-copy state, and following source-shaped byte-write
seam. A pre-existing debug Battle Test route is only the post-start host for the
probe. The text and diamond rows model
their documented seed-copy write shape only; they never claim to execute either
caller loop.
"""

from __future__ import annotations

import json
import re
from pathlib import Path
from typing import Any

from sf2tool.h3.bizhawk import (
    DERIVED_ROOT,
    run_observer,
    verify_runtime_contract,
)
from sf2tool.h3.observer_status import (
    CALLBACK_FAILURE_PREFIX,
    assert_observer_status,
    callback_failure_status,
    observer_failure_contract,
)
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

FIXTURE = repo_path("tests/fixtures/h3/random-services-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h3/h3-random-services-fixture.schema.json")
OBSERVATION_SCHEMA = repo_path("schemas/h3/h3-random-services-observation.schema.json")
OBSERVER = repo_path("tools/bizhawk/random_services_observer.lua")
FAILURE_SCHEMA = repo_path("schemas/h3/random-services-callback-failure.schema.json")
TOOLCHAIN_MANIFEST = repo_path("manifests/toolchain.json")
H2_OWNER_FIXTURE = repo_path("tests/fixtures/h2/tech-services-static-v1.json")
H2_OWNER_FIXTURE_RELATIVE = "tests/fixtures/h2/tech-services-static-v1.json"
UPSTREAM = repo_path("local/upstream/SF2DISASM/disasm")
RNG_SOURCE = UPSTREAM / "code/common/tech/randomnumbergenerator.asm"
THINKING_SOURCE = UPSTREAM / "code/common/tech/thinkingairng.asm"
ALIAS_SOURCE = UPSTREAM / "code/common/tech/jumpinterfaces/s13_jumpinterface.asm"
TEXT_SOURCE = UPSTREAM / "code/common/scripting/text/textfunctions_1.asm"
DIAMOND_SOURCE = UPSTREAM / "code/common/menus/diamondmenu.asm"
BATTLE_TEST_SOURCE = UPSTREAM / "code/gameflow/special/battletest.asm"
TURN_ORDER_SOURCE = UPSTREAM / "code/gameflow/battle/battleloop/turnorderfunctions.asm"
CONST_SOURCE = UPSTREAM / "sf2const.asm"

OBSERVER_OUTPUT_NAME = "random-services"
STATUS_PREFIX = CALLBACK_FAILURE_PREFIX
OBSERVER_FAILURE_CONTRACT = observer_failure_contract(OBSERVER_OUTPUT_NAME)


def _noncomment_lines(source: str) -> list[str]:
    return [
        re.sub(r"\s+", " ", line.split(";", 1)[0].strip()).lower()
        for line in source.splitlines()
        if line.split(";", 1)[0].strip()
    ]


def _require_sequence(source: str, sequence: tuple[str, ...], *, name: str) -> None:
    lines = _noncomment_lines(source)
    cursor = 0
    for expected in sequence:
        try:
            cursor = lines.index(expected, cursor) + 1
        except ValueError as error:
            raise ValueError(f"{name} no longer contains instruction {expected!r}") from error


def _direct_rng_calls(source: str) -> tuple[str, ...]:
    """Return real direct calls, excluding comments and look-alike symbols."""
    result = []
    for line in _noncomment_lines(source):
        match = re.fullmatch(r"(?:[a-z0-9_@.]+: )?(bsr(?:\.[a-z]+)?|jsr) (.+)", line)
        if not match:
            continue
        target = re.sub(r"[()\s]", "", match.group(2)).removesuffix(".w")
        if target == "generaterandomnumber":
            result.append(match.group(1))
    return tuple(result)


def validate_provenance(
    fixture: dict[str, Any],
    *,
    toolchain_path: Path = TOOLCHAIN_MANIFEST,
    h2_fixture_path: Path = H2_OWNER_FIXTURE,
) -> None:
    """Tie the H3 fixture to the pinned upstream and its H2 owner independently of schema consts."""
    toolchain = load_json(toolchain_path)
    h2_fixture = load_json(h2_fixture_path)
    pinned = toolchain["sf2disasm"]
    provenance = fixture["provenance"]
    expected_repository = pinned["repository"].removesuffix(".git")
    if (
        provenance["upstreamRepository"] != expected_repository
        or provenance["upstreamCommit"] != pinned["commit"]
        or provenance["upstreamCommit"] != h2_fixture["upstreamCommit"]
        or provenance["h2Fixture"] != H2_OWNER_FIXTURE_RELATIVE
        or provenance["h2FixtureId"] != h2_fixture["id"]
        or fixture["romSha256"] != h2_fixture["romSha256"]
    ):
        raise ValueError("random-services provenance does not match pinned toolchain/H2 owner")


def _word_step(seed: int) -> int:
    return (seed * 541 + 12345) & 0xFFFF


def _signed_byte_step(seed: int) -> int:
    high = seed >> 8
    signed = high if high < 0x80 else high - 0x100
    return (((signed * 541 + 12345) & 0xFF) << 8) | (seed & 0xFF)


_EXPECTATION_FIELDS = {
    "phase",
    "role",
    "allowed",
    "expectedEventPc",
    "expectedCallPc",
    "expectedTargetPc",
    "expectedReturnPc",
}


def _expectation(
    phase: str,
    role: str,
    *,
    event_pc: int,
    call_pc: int | None,
    target_pc: int | None,
    return_pc: int | None,
    allowed: bool = True,
) -> dict[str, Any]:
    return {
        "phase": phase,
        "role": role,
        "allowed": allowed,
        "expectedEventPc": event_pc,
        "expectedCallPc": call_pc,
        "expectedTargetPc": target_pc,
        "expectedReturnPc": return_pc,
    }


def _case_target_and_return(case: dict[str, Any], function: dict[str, Any]) -> tuple[int, int]:
    if case["service"] == "base":
        return function["baseEntryAddress"], function["baseReturnAddress"]
    if case["service"] == "unsigned-bounded":
        return (
            function["unsignedBoundedEntryAddress"],
            function[
                "unsignedEarlyReturnAddress"
                if case["expected"]["returnPath"] == "early"
                else "unsignedNormalReturnAddress"
            ],
        )
    if case["service"] == "thinking-bounded":
        return (
            function["thinkingAliasEntryAddress"],
            function[
                "thinkingEarlyReturnAddress"
                if case["expected"]["returnPath"] == "early"
                else "thinkingNormalReturnAddress"
            ],
        )
    raise ValueError(f"unknown random-service expectation {case['service']}")


def _derive_callback_expectations(fixture: dict[str, Any]) -> dict[str, Any]:
    """Derive one exact diagnostic expectation for every registered physical PC."""
    function = fixture["function"]
    instrumentation = fixture["instrumentation"]
    static = {
        "host-battle-test": _expectation(
            "host-battle-test",
            "host-battle-test",
            event_pc=instrumentation["battleTestEntryPc"],
            call_pc=None,
            target_pc=None,
            return_pc=None,
        ),
        "host-number-prompt": _expectation(
            "host-number-prompt",
            "host-number-prompt",
            event_pc=instrumentation["numberPromptEntryPc"],
            call_pc=None,
            target_pc=None,
            return_pc=None,
        ),
        "host-flag-prompt": _expectation(
            "host-flag-prompt",
            "host-flag-prompt",
            event_pc=instrumentation["flagPromptEntryPc"],
            call_pc=None,
            target_pc=None,
            return_pc=None,
        ),
        "host-turn-order": _expectation(
            "host-turn-order",
            "host-turn-order-return-redirect",
            event_pc=instrumentation["turnOrderEntryPc"],
            call_pc=None,
            target_pc=None,
            return_pc=instrumentation["workRamProbePc"],
        ),
    }
    cases = []
    for case in fixture["cases"]:
        instruction_target_pc, helper_return_pc = _case_target_and_return(case, function)
        effective_target_pc = (
            function["thinkingBoundedEntryAddress"]
            if case["service"] == "thinking-bounded"
            else instruction_target_pc
        )
        instruction_helper = {
            "call_pc": instrumentation["helperCallPc"],
            "target_pc": instruction_target_pc,
            "return_pc": helper_return_pc,
        }
        effective_helper = {
            "call_pc": instrumentation["helperCallPc"],
            "target_pc": effective_target_pc,
            "return_pc": helper_return_pc,
        }
        callbacks = {
            "case-entry": _expectation(
                "case-entry",
                "case-entry",
                event_pc=instrumentation["caseEntryPc"],
                **instruction_helper,
            ),
            "source-shaped-copy-write": _expectation(
                "source-shaped-copy-write",
                "source-shaped-copy-write",
                event_pc=instrumentation["sourceCopyWritePc"],
                **effective_helper,
            ),
            "case-result": _expectation(
                "case-result",
                "case-result",
                event_pc=instrumentation["resultPc"],
                **effective_helper,
            ),
        }
        if case["service"] == "base":
            callbacks.update(
                {
                    "base-entry": _expectation(
                        "base-entry",
                        "base-entry",
                        event_pc=function["baseEntryAddress"],
                        **effective_helper,
                    ),
                    "base-return": _expectation(
                        "base-return",
                        "base-return",
                        event_pc=function["baseReturnAddress"],
                        **effective_helper,
                    ),
                }
            )
        else:
            thinking = case["service"] == "thinking-bounded"
            prefix = "thinking" if thinking else "unsigned"
            service = "thinking-bounded" if thinking else "unsigned-bounded"
            entry_key = "thinkingBoundedEntryAddress" if thinking else "unsignedBoundedEntryAddress"
            generator_entry_key = (
                "thinkingGeneratorEntryAddress" if thinking else "unsignedGeneratorEntryAddress"
            )
            generator_call_key = (
                "thinkingGeneratorCallAddress" if thinking else "unsignedGeneratorCallAddress"
            )
            generator_return_to_caller_key = (
                "thinkingGeneratorReturnToCallerAddress"
                if thinking
                else "unsignedGeneratorReturnToCallerAddress"
            )
            generator_return_key = (
                "thinkingGeneratorReturnAddress" if thinking else "unsignedGeneratorReturnAddress"
            )
            normal_key = (
                "thinkingNormalReturnAddress" if thinking else "unsignedNormalReturnAddress"
            )
            early_key = "thinkingEarlyReturnAddress" if thinking else "unsignedEarlyReturnAddress"
            if thinking:
                callbacks["thinking-alias"] = _expectation(
                    "thinking-alias",
                    "thinking-alias-instruction-target",
                    event_pc=function["thinkingAliasEntryAddress"],
                    **instruction_helper,
                )
            callbacks[f"{prefix}-entry"] = _expectation(
                f"{prefix}-entry",
                f"{service}-effective-target" if thinking else f"{service}-entry",
                event_pc=function[entry_key],
                **effective_helper,
            )
            generator = {
                "call_pc": function[generator_call_key],
                "target_pc": function[generator_entry_key],
                "return_pc": function[generator_return_to_caller_key],
            }
            callbacks[f"{prefix}-generator-entry"] = _expectation(
                f"{prefix}-generator-entry",
                f"{service}-generator-entry",
                event_pc=function[generator_entry_key],
                **generator,
            )
            callbacks[f"{prefix}-generator-return"] = _expectation(
                f"{prefix}-generator-return",
                f"{service}-generator-return",
                event_pc=function[generator_return_key],
                **generator,
            )
            for path, return_key in (("normal", normal_key), ("early", early_key)):
                callbacks[f"{prefix}-{path}-return"] = _expectation(
                    f"{prefix}-{path}-return",
                    f"{service}-{path}-return",
                    event_pc=function[return_key],
                    **effective_helper,
                    allowed=case["expected"]["returnPath"] == path,
                )
        cases.append(callbacks)
    return {"static": static, "cases": cases}


def _validate_callback_expectations(fixture: dict[str, Any], expectations: dict[str, Any]) -> None:
    """Reject missing, extra, or role/value-drifted callback diagnostics."""
    canonical = _derive_callback_expectations(fixture)
    if expectations != canonical:
        raise ValueError("random-services callback expectation drift")
    for expectation in [*expectations["static"].values(), *(
        value for case in expectations["cases"] for value in case.values()
    )]:
        if set(expectation) != _EXPECTATION_FIELDS:
            raise ValueError("random-services callback expectation fields drifted")


def callback_expectations(fixture: dict[str, Any]) -> dict[str, Any]:
    expectations = _derive_callback_expectations(fixture)
    _validate_callback_expectations(fixture, expectations)
    return expectations


def model_case(case: dict[str, Any]) -> dict[str, Any]:
    """Independent arithmetic model for the compact behavioral matrix."""
    seed = case["randomSeedBefore"]
    seed_copy = case["seedCopyBefore"]
    if case["service"] == "base":
        random_seed_after = (seed * 13 + 7) & 0xFFFF
        output = (random_seed_after * case["rangeWord"]) // 0x10000
        return {
            "randomSeedAfter": random_seed_after,
            "seedCopyAtHelperReturn": seed_copy,
            "seedCopyAfterSourceCopy": (output << 8) | (seed_copy & 0xFF),
            "resultLowByte": output,
            "generatorCallCount": 0,
            "generatorOutputs": [],
            "generatorStates": [],
            "returnPath": "base",
        }

    range_byte = case["rangeWord"] & 0xFF
    states: list[int] = []
    outputs: list[int] = []
    while True:
        if case["service"] == "unsigned-bounded":
            seed_copy = _word_step(seed_copy)
        elif case["service"] == "thinking-bounded":
            seed_copy = _signed_byte_step(seed_copy)
        else:
            raise ValueError(f"unknown random-service model {case['service']}")
        output = seed_copy & 0xFF if case["service"] == "unsigned-bounded" else seed_copy >> 8
        states.append(seed_copy)
        outputs.append(output)
        if range_byte in (0, 1) or range_byte >= 0x80:
            result = 0
            return {
                "randomSeedAfter": seed,
                "seedCopyAtHelperReturn": seed_copy,
                "seedCopyAfterSourceCopy": (result << 8) | (seed_copy & 0xFF),
                "resultLowByte": result,
                "generatorCallCount": len(outputs),
                "generatorOutputs": outputs,
                "generatorStates": states,
                "returnPath": "early",
            }
        if output < range_byte:
            result = output
            return {
                "randomSeedAfter": seed,
                "seedCopyAtHelperReturn": seed_copy,
                "seedCopyAfterSourceCopy": (result << 8) | (seed_copy & 0xFF),
                "resultLowByte": result,
                "generatorCallCount": len(outputs),
                "generatorOutputs": outputs,
                "generatorStates": states,
                "returnPath": "normal",
            }


def _require_rom_bytes(rom: bytes, address: int, expected_hex: str, *, name: str) -> None:
    actual = rom[address : address + len(expected_hex) // 2].hex().upper()
    if actual != expected_hex:
        raise ValueError(f"H1/ROM guard failed for {name}: expected {expected_hex}, got {actual}")


def validate_static_contract(fixture: dict[str, Any], rom_path: Path) -> None:
    """Guard every observed entry/return/write seam against assigned source and ROM."""
    const = CONST_SOURCE.read_text(encoding="utf-8")
    for symbol, value in (("RANDOM_SEED", 0xFFDEA4), ("RANDOM_SEED_COPY", 0xFFDFB0)):
        if not re.search(rf"^\s*{symbol}:\s+equ\s+\${value:06X}(?:\s*;.*)?$", const, re.M):
            raise ValueError(f"constant guard failed for {symbol}")

    rng = RNG_SOURCE.read_text(encoding="utf-8")
    thinking = THINKING_SOURCE.read_text(encoding="utf-8")
    alias = ALIAS_SOURCE.read_text(encoding="utf-8")
    text = TEXT_SOURCE.read_text(encoding="utf-8")
    diamond = DIAMOND_SOURCE.read_text(encoding="utf-8")
    battle_test = BATTLE_TEST_SOURCE.read_text(encoding="utf-8")
    turn_order = TURN_ORDER_SOURCE.read_text(encoding="utf-8")
    _require_sequence(
        rng,
        (
            "generaterandomnumber:",
            "move.w (random_seed).l,d7",
            "mulu.w #13,d7",
            "addi.w #7,d7",
            "move.w d7,(random_seed).l",
            "rts",
            "waitforrandomvaluetomatch:",
            "move.b d6,d1",
            "bsr.w generaterandomvalueunsigned",
            "cmpi.b #1,d1",
            "bpl.s loc_163e",
            "cmp.b d1,d7",
            "bra.s loc_162e",
            "generaterandomvalueunsigned:",
            "move.w d7,(a0)",
            "andi.w #byte_mask,d7",
        ),
        name="randomnumbergenerator.asm",
    )
    _require_sequence(
        thinking,
        (
            "generaterandomvaluesigned:",
            "move.b (a0),d7",
            "ext.w d7",
            "move.b d7,(a0)",
            "generaterandomnumberunderd6:",
            "move.b d6,d1",
            "bsr.s generaterandomvaluesigned",
            "cmpi.b #1,d1",
            "bpl.s loc_1ad0c8",
            "cmp.b d1,d7",
            "bra.s loc_1ad0ba",
        ),
        name="thinkingairng.asm",
    )
    _require_sequence(
        alias,
        ("j_generaterandomnumberunderd6:", "jmp generaterandomnumberunderd6(pc)"),
        name="s13_jumpinterface.asm",
    )
    if _direct_rng_calls(text) != ("bsr.w", "bsr.w"):
        raise ValueError("text source direct GenerateRandomNumber inventory changed")
    if _direct_rng_calls(diamond) != ("jsr",):
        raise ValueError("diamond source direct GenerateRandomNumber inventory changed")
    _require_sequence(
        text,
        (
            "symbol_wait1:",
            "loc_659c:",
            "move.w #256,d6",
            "bsr.w generaterandomnumber",
            "move.b d7,((random_seed_copy-$1000000)).w",
            "bsr.w waitforvint",
        ),
        name="text symbol_wait1",
    )
    _require_sequence(
        diamond,
        (
            "@loc_16:",
            "move.w #$100,d6",
            "jsr (generaterandomnumber).w",
            "move.b d7,((random_seed_copy-$1000000)).w",
            "jsr (waitforvint).w",
        ),
        name="diamond menu",
    )
    _require_sequence(
        battle_test,
        (
            "debugmodebattletest:",
            "move.b #-1,((debug_mode_toggle-$1000000)).w",
            "jsr j_numberprompt",
        ),
        name="debug Battle Test setup host",
    )
    _require_sequence(
        turn_order,
        ("generatebattleturnorder:", "lea ((battle_turn_order-$1000000)).w,a0", "move.l a0,-(sp)"),
        name="turn-order setup host",
    )

    function = fixture["function"]
    rom = rom_path.read_bytes()
    guards = {
        "base entry": (
            function["baseEntryAddress"],
            "3E3900FFDEA4CEFC000D0647000702870000FFFF33C700FFDEA4",
        ),
        "base return": (function["baseReturnAddress"], "4E75"),
        "unsigned bounded entry": (
            function["unsignedBoundedEntryAddress"],
            "48E7FCFE1206610000220C01000167026A046000000EBE01650260EA",
        ),
        "unsigned normal return": (function["unsignedNormalReturnAddress"], "4E75"),
        "unsigned early return": (function["unsignedEarlyReturnAddress"], "4E75"),
        "unsigned generator call": (function["unsignedGeneratorCallAddress"], "61000022"),
        "unsigned generator return to caller": (
            function["unsignedGeneratorReturnToCallerAddress"],
            "0C010001",
        ),
        "unsigned generator entry": (
            function["unsignedGeneratorEntryAddress"],
            "48E7FCFE41F900FFDFB042473E10CEFC021D064730393087024700FF",
        ),
        "unsigned generator write": (function["unsignedGeneratorWriteAddress"], "3087"),
        "unsigned generator return": (function["unsignedGeneratorReturnAddress"], "4E75"),
        "thinking alias": (function["thinkingAliasEntryAddress"], "4EFA106E"),
        "thinking bounded entry": (function["thinkingBoundedEntryAddress"], "48E7FCFE120661D4"),
        "thinking normal return": (function["thinkingNormalReturnAddress"], "4E75"),
        "thinking early return": (function["thinkingEarlyReturnAddress"], "4E75"),
        "thinking generator call": (function["thinkingGeneratorCallAddress"], "61D4"),
        "thinking generator return to caller": (
            function["thinkingGeneratorReturnToCallerAddress"],
            "0C010001",
        ),
        "thinking generator entry": (
            function["thinkingGeneratorEntryAddress"],
            "48E7FCFE41F900FFDFB042471E104887CEFC021D06473039024700FF",
        ),
        "thinking generator write": (function["thinkingGeneratorWriteAddress"], "1087"),
        "thinking generator return": (function["thinkingGeneratorReturnAddress"], "4E75"),
        "text wait source call/write": (
            fixture["sourceContexts"]["textWaitCallPc"],
            "6100B05A11C7DFB0",
        ),
        "diamond source call/write": (
            fixture["sourceContexts"]["diamondCallPc"],
            "4EB8160011C7DFB0",
        ),
        "Battle Test setup host": (
            fixture["instrumentation"]["battleTestEntryPc"],
            "11FC00FFB0A911FC00FFB0A8700161000BC8700261000BC2700361000BBC7004",
        ),
        "number-prompt setup seam": (
            fixture["instrumentation"]["numberPromptEntryPc"],
            "48E77FC04E56FFF03D40FFF83D41FFF63D42FFF4303C0703323C20014EB90000",
        ),
        "flag-prompt setup seam": (
            fixture["instrumentation"]["flagPromptEntryPc"],
            "48E7FFC04E56FFF03D40FFF8303C0703323C20014EB9000048023D40FFFA2D49",
        ),
        "turn-order setup host": (
            fixture["instrumentation"]["turnOrderEntryPc"],
            fixture["instrumentation"]["turnOrderEntryHex"],
        ),
    }
    for name, (address, expected) in guards.items():
        _require_rom_bytes(rom, address, expected, name=name)


def _failure_diagnostic(status_path: Path) -> str | None:
    payload = callback_failure_status(
        status_path,
        owner=OBSERVER_OUTPUT_NAME,
        schema_path=FAILURE_SCHEMA,
    )
    if payload is None:
        return None
    return json.dumps(payload, sort_keys=True)


def _assert_status(status_path: Path) -> None:
    assert_observer_status(
        status_path,
        owner=OBSERVER_OUTPUT_NAME,
        schema_path=FAILURE_SCHEMA,
        required_milestones=(
            "milestone:host-turn-order-redirect",
            "milestone:probe-entered",
        ),
    )


def _observer_config(fixture: dict[str, Any]) -> dict[str, Any]:
    return {
        "id": fixture["id"],
        "core": fixture["emulator"]["core"],
        "cases": fixture["cases"],
        "function": fixture["function"],
        "ram": fixture["ram"],
        "instrumentation": fixture["instrumentation"],
        "callbackExpectations": callback_expectations(fixture),
        "observerFailureContract": OBSERVER_FAILURE_CONTRACT,
    }


def _assert_observation(fixture: dict[str, Any], observed: dict[str, Any]) -> None:
    if observed.get("system") != "GEN" or observed.get("core") != fixture["emulator"]["core"]:
        raise ValueError("unexpected random-services execution system or core")
    if observed.get("id") != fixture["id"] or observed.get("caseOrder") != [
        case["id"] for case in fixture["cases"]
    ]:
        raise ValueError("random-services observation identity/order mismatch")
    expected_records = []
    for case in fixture["cases"]:
        expected = model_case(case)
        if expected != case["expected"]:
            raise ValueError(f"random-services golden disagrees with model: {case['id']}")
        expected_records.append(
            {
                "id": case["id"],
                **expected,
                "instructionTargetObserved": True,
                "effectiveTargetObserved": True,
                "sourceCopyWriteSeen": True,
            }
        )
    if observed.get("records") != expected_records:
        raise ValueError("random-services exact observed case matrix mismatch")


def verify_random_services(rom_path: Path, *, timeout_seconds: int = 180) -> dict[str, Any]:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner=str(FIXTURE))
    validate_provenance(fixture)
    verify_runtime_contract(fixture, rom_path)
    validate_static_contract(fixture, rom_path)
    status_path = DERIVED_ROOT / "random-services.status.txt"
    try:
        observed = run_observer(
            rom_path=rom_path,
            observer_path=OBSERVER,
            config=_observer_config(fixture),
            output_name="random-services",
            timeout_seconds=timeout_seconds,
        )
    except RuntimeError as error:
        callback = _failure_diagnostic(status_path)
        if callback is not None:
            raise RuntimeError(f"random-services observer callback failure: {callback}") from error
        raise
    _assert_status(status_path)
    validate_json(observed, OBSERVATION_SCHEMA, owner="random-services observation")
    _assert_observation(fixture, observed)
    return {
        "Fixture": fixture["id"],
        "Engine": f"BizHawk {fixture['emulator']['version']} / {fixture['emulator']['core']}",
        "SetupHost": "debug Battle Test route only",
        "Cases": len(fixture["cases"]),
        "Launches": 1,
        "Status": "PASS",
    }
