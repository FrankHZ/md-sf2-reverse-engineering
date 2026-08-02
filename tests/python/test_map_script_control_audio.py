from __future__ import annotations

import copy
import json
from pathlib import Path

import pytest

from sf2tool.h3.map_script_control_audio import (
    OBSERVER_FAILURE_CONTRACT,
    _assert_success_status,
    _callback_failure_status,
    _failure_expectations,
    _failure_roles,
    _h1_followup_instruction_address,
    _h1_instruction_address,
    _preserved_wrapper_trampoline,
    _validate_failure_expectations,
    derive_case_expectations,
)
from sf2tool.jsonio import validate_json
from sf2tool.paths import repo_path

FIXTURE = repo_path("tests/fixtures/h3/map-script-control-audio-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h3/h3-map-script-control-audio-fixture.schema.json")
OBSERVATION_SCHEMA = repo_path(
    "schemas/h3/h3-map-script-control-audio-observation.schema.json"
)
H2_FIXTURE = repo_path("tests/fixtures/h2/map-script-engine-static-v1.json")


def _load(path: Path) -> dict[str, object]:
    return json.loads(path.read_text(encoding="utf-8"))


def _derived_fixture() -> tuple[dict[str, object], list[dict[str, object]]]:
    fixture = _load(FIXTURE)
    h2 = _load(H2_FIXTURE)
    static = {"scriptControlCommandFacts": h2["expected"]["scriptControlCommandFacts"]}
    return fixture, derive_case_expectations(static, fixture)


def _expected_observation(
    fixture: dict[str, object], derived: list[dict[str, object]]
) -> dict[str, object]:
    return {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["id"],
        "mapTest": fixture["mapTestIndex"],
        "recordOrder": [case["id"] for case in fixture["cases"]],
        "records": [case["expected"] for case in derived],
    }


def test_fixture_schema_and_complete_static_derivation() -> None:
    fixture, derived = _derived_fixture()

    validate_json(fixture, FIXTURE_SCHEMA, owner="control/audio fixture")
    assert len(derived) == 6
    assert [case["expected"] for case in derived] == [case["expected"] for case in fixture["cases"]]
    assert derived[3]["scriptBytes"] == [0, 5, 0, 32, 255, 255]
    assert derived[0]["waitForVIntCalls"] == 1
    assert derived[1]["waitForVIntCalls"] == 0
    assert derived[4]["expected"]["subroutineTargetStackDelta"] == -8
    assert derived[5]["scriptBytes"] == [0, 11, 0, 255, 64, 10, 255, 255]


def test_observation_schema_asserts_the_complete_semantic_matrix() -> None:
    fixture, derived = _derived_fixture()
    observed = _expected_observation(fixture, derived)

    validate_json(observed, OBSERVATION_SCHEMA, owner="control/audio observation")
    assert observed["records"] == [case["expected"] for case in fixture["cases"]]


@pytest.mark.parametrize(
    ("path", "value"),
    [
        (("function", "waitSleepCallAddress"), 0),
        (("instrumentation", "trampolinePostHandlerAddress"), 0),
        (("observationBoundary",), "mutated boundary"),
        (("cases", 2, "expected", "csc06Returned"), False),
        (("cases", 3, "input", "soundSourceSymbol"), "SFX_DOOR_OPEN"),
    ],
)
def test_fixture_schema_rejects_exact_value_mutations(
    path: tuple[object, ...], value: object
) -> None:
    fixture = _load(FIXTURE)
    changed = copy.deepcopy(fixture)
    target: object = changed
    for component in path[:-1]:
        target = target[component]  # type: ignore[index]
    target[path[-1]] = value  # type: ignore[index]

    with pytest.raises(ValueError, match="failed schema validation"):
        validate_json(changed, FIXTURE_SCHEMA, owner="mutated control/audio fixture")


def test_recursive_closure_and_observation_order_mutations_fail() -> None:
    fixture, derived = _derived_fixture()
    changed_fixture = copy.deepcopy(fixture)
    changed_fixture["cases"][0]["expected"]["unexpected"] = True
    with pytest.raises(ValueError, match="failed schema validation"):
        validate_json(changed_fixture, FIXTURE_SCHEMA, owner="nested fixture mutation")

    observed = _expected_observation(fixture, derived)
    changed_observation = copy.deepcopy(observed)
    changed_observation["records"] = list(reversed(changed_observation["records"]))
    with pytest.raises(ValueError, match="failed schema validation"):
        validate_json(changed_observation, OBSERVATION_SCHEMA, owner="ordered observation mutation")

    changed_observation = copy.deepcopy(observed)
    changed_observation["records"][4]["subroutineTargetReturned"] = False
    with pytest.raises(ValueError, match="failed schema validation"):
        validate_json(
            changed_observation,
            OBSERVATION_SCHEMA,
            owner="semantic observation mutation",
        )


def test_static_derivation_rejects_source_contract_mutations() -> None:
    fixture = _load(FIXTURE)
    h2 = _load(H2_FIXTURE)
    static = {
        "scriptControlCommandFacts": copy.deepcopy(
            h2["expected"]["scriptControlCommandFacts"]
        )
    }
    for macro in static["scriptControlCommandFacts"]["macros"]:
        if macro["name"] == "jump":
            macro["encodedBytes"] = 4

    with pytest.raises(ValueError, match="fixture/static expectation drift"):
        derive_case_expectations(static, fixture)


def test_h1_instruction_helpers_exclude_comments_labels_and_near_misses() -> None:
    section = "\n".join(
        [
            "00000010 4EBB 0000 jsr rjt_cutsceneScriptCommands(pc,d0.w)",
            "00000014 loc_47140:",
            "00000014 60C0 bra.s loc_47140",
            "00000016 ; jsr rjt_cutsceneScriptCommands(pc,d0.w)",
            "00000018 csc_jsr_rjt_cutsceneScriptCommands_pc_d0_w:",
            "0000001A 4E40 M trap #sound_command",
        ]
    )
    assert _h1_instruction_address(
        section, "jsr rjt_cutsceneScriptCommands(pc,d0.w)", owner="test"
    ) == 0x10
    assert _h1_followup_instruction_address(
        section,
        "jsr rjt_cutsceneScriptCommands(pc,d0.w)",
        "bra.s loc_47140",
        owner="test",
    ) == 0x14
    assert _h1_instruction_address(section, "trap #sound_command", owner="test") == 0x1A
    with pytest.raises(ValueError, match="0 matches"):
        _h1_instruction_address(section, "jsr (Sleep).w", owner="test")
    with pytest.raises(ValueError, match="followup drift"):
        _h1_followup_instruction_address(
            section.replace("bra.s loc_47140", "bra.w loc_47140"),
            "jsr rjt_cutsceneScriptCommands(pc,d0.w)",
            "bra.s loc_47140",
            owner="test",
        )


def test_preserved_trampoline_rejects_source_call_epilogue_and_width_mutations() -> None:
    fixture = _load(FIXTURE)
    section = "\n".join(
        [
            "00047512 4E90 jsr (a0)",
            "00047514 loc_47514:",
            "00047514",
            "00047514 4CDF 03FF movem.l (sp)+,d0-a1",
            "00047518 4E75 rts",
        ]
    )

    assert _preserved_wrapper_trampoline(section, fixture) == {
        "callSiteAddress": 0x47512,
        "trampolinePostHandlerAddress": 0xFF9A,
    }

    for mutated, match in (
        (section.replace("jsr (a0)", "jmp (a0)"), "0 matches"),
        (section.replace("4E90 jsr", "4E90 4E71 jsr"), "source bytes drifted"),
        (section.replace("movem.l (sp)+,d0-a1", "move.l (sp)+,d0"), "followup drift"),
        (section.replace("4E75 rts", "4E71 nop"), "followup drift"),
    ):
        with pytest.raises(ValueError, match=match):
            _preserved_wrapper_trampoline(mutated, fixture)


def test_failure_expectations_route_shared_pcs_by_case_role() -> None:
    fixture = _load(FIXTURE)
    function = fixture["function"]
    instrumentation = fixture["instrumentation"]
    cases = fixture["cases"]
    service = {"waitForVIntAddress": 3822, "sleepAddress": 3844}
    roles = _failure_roles(cases)
    expectations = _failure_expectations(function, instrumentation, service, cases)

    assert roles == {
        "opcode-dispatch": {
            "csc06-no-op": "opcode-dispatch/no-op",
            "play-sound-dispatch": "opcode-dispatch/sound",
            "execute-subroutine-return": "opcode-dispatch/subroutine",
            "jump-cursor-and-end": "opcode-dispatch/jump",
        },
        "csc06-entry": {
            "csc06-no-op": "csc06-entry/direct-dispatch",
            "execute-subroutine-return": "csc06-entry/subroutine-target",
        },
    }
    csc06_roles = expectations[str(function["csc06DoNothingAddress"])]["roles"]
    assert csc06_roles == {
        "csc06-entry/direct-dispatch": {
            "callSiteAddress": function["opcodeDispatchCallAddress"],
            "targetAddress": function["csc06DoNothingAddress"],
            "returnAddress": function["opcodeDispatchReturnAddress"],
        },
        "csc06-entry/subroutine-target": {
            "callSiteAddress": function["subroutineIndirectCallAddress"],
            "targetAddress": function["csc06DoNothingAddress"],
            "returnAddress": function["subroutineResumeAddress"],
        },
    }
    assert expectations[str(function["subroutineIndirectCallAddress"])]["roles"][
        "csc0a-call"
    ] == {
        "callSiteAddress": function["subroutineIndirectCallAddress"],
        "targetAddress": function["csc06DoNothingAddress"],
        "returnAddress": function["subroutineResumeAddress"],
    }
    dispatch_roles = expectations[str(function["opcodeDispatchCallAddress"])]["roles"]
    assert dispatch_roles == {
        "opcode-dispatch/no-op": {
            "callSiteAddress": function["opcodeDispatchCallAddress"],
            "targetAddress": function["csc06DoNothingAddress"],
            "returnAddress": function["opcodeDispatchReturnAddress"],
        },
        "opcode-dispatch/sound": {
            "callSiteAddress": function["opcodeDispatchCallAddress"],
            "targetAddress": function["csc05PlaySoundAddress"],
            "returnAddress": function["opcodeDispatchReturnAddress"],
        },
        "opcode-dispatch/subroutine": {
            "callSiteAddress": function["opcodeDispatchCallAddress"],
            "targetAddress": function["csc0AExecuteSubroutineAddress"],
            "returnAddress": function["opcodeDispatchReturnAddress"],
        },
        "opcode-dispatch/jump": {
            "callSiteAddress": function["opcodeDispatchCallAddress"],
            "targetAddress": function["csc0BJumpAddress"],
            "returnAddress": function["opcodeDispatchReturnAddress"],
        },
    }
    assert {
        "csc05": expectations[str(function["csc05PlaySoundAddress"])]["roles"][
            "csc05-entry"
        ],
        "csc0a": expectations[str(function["csc0AExecuteSubroutineAddress"])]["roles"][
            "csc0a-entry"
        ],
        "csc0b": expectations[str(function["csc0BJumpAddress"])]["roles"][
            "csc0b-entry"
        ],
    } == {
        "csc05": {
            "callSiteAddress": function["opcodeDispatchCallAddress"],
            "targetAddress": function["csc05PlaySoundAddress"],
            "returnAddress": function["opcodeDispatchReturnAddress"],
        },
        "csc0a": {
            "callSiteAddress": function["opcodeDispatchCallAddress"],
            "targetAddress": function["csc0AExecuteSubroutineAddress"],
            "returnAddress": function["opcodeDispatchReturnAddress"],
        },
        "csc0b": {
            "callSiteAddress": function["opcodeDispatchCallAddress"],
            "targetAddress": function["csc0BJumpAddress"],
            "returnAddress": function["opcodeDispatchReturnAddress"],
        },
    }


def test_failure_expectation_contract_rejects_role_mutations() -> None:
    fixture = _load(FIXTURE)
    function = fixture["function"]
    instrumentation = fixture["instrumentation"]
    cases = fixture["cases"]
    service = {"waitForVIntAddress": 3822, "sleepAddress": 3844}
    roles = _failure_roles(cases)
    expectations = _failure_expectations(function, instrumentation, service, cases)
    csc06 = str(function["csc06DoNothingAddress"])

    missing = copy.deepcopy(expectations)
    del missing[csc06]["roles"]["csc06-entry/subroutine-target"]
    extra = copy.deepcopy(expectations)
    extra[csc06]["roles"]["csc06-entry/unexpected"] = {}
    incorrect = copy.deepcopy(expectations)
    incorrect[csc06]["roles"]["csc06-entry/direct-dispatch"]["returnAddress"] = 0
    missing_route = copy.deepcopy(roles)
    del missing_route["csc06-entry"]["execute-subroutine-return"]
    for mutated in (missing, extra, incorrect):
        with pytest.raises(ValueError, match="failure expectation contract drift"):
            _validate_failure_expectations(
                roles, mutated, function, instrumentation, service, cases
            )
    with pytest.raises(ValueError, match="failure-role routing drift"):
        _validate_failure_expectations(
            missing_route, expectations, function, instrumentation, service, cases
        )


def test_callback_failure_status_and_cleanup_milestone_are_strict(tmp_path: Path) -> None:
    status = tmp_path / "observer.status.txt"
    payload = {
        "caseId": "execute-subroutine-return",
        "phase": "csc0a-call",
        "actualPc": 291854,
        "expectedCallSiteAddress": 291854,
        "expectedTargetAddress": 291712,
        "expectedReturnAddress": 291856,
        "pendingCallback": {
            "phase": "csc0a-call",
            "role": "csc0a-call",
            "active": True,
            "handlerEntriesObserved": ["ExecuteMapScript", "csc0A_executeSubroutine"],
            "scriptWordReadCount": 1,
            "waitForVIntCallCount": 0,
            "subroutineEntryStackPointer": 16776960,
        },
        "error": "forced callback failure",
    }
    status.write_text(
        OBSERVER_FAILURE_CONTRACT["statusPrefix"] + json.dumps(payload) + "\n",
        encoding="utf-8",
    )
    assert _callback_failure_status(status) == payload
    with pytest.raises(RuntimeError, match="callback failure"):
        _assert_success_status(status)

    for pending in (
        {key: value for key, value in payload["pendingCallback"].items() if key != "role"},
        {**payload["pendingCallback"], "unexpected": True},
        {**payload["pendingCallback"], "active": 1},
    ):
        malformed = {**payload, "pendingCallback": pending}
        status.write_text(
            OBSERVER_FAILURE_CONTRACT["statusPrefix"] + json.dumps(malformed) + "\n",
            encoding="utf-8",
        )
        with pytest.raises(ValueError, match="pending"):
            _callback_failure_status(status)

    status.write_text(
        "milestone:callbacks-cleared:0\nmilestone:observer-finished\n", encoding="utf-8"
    )
    _assert_success_status(status)
