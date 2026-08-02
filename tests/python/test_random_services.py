from __future__ import annotations

import copy
import json
from pathlib import Path

import pytest

from sf2tool.h3 import random_services
from sf2tool.jsonio import load_json, validate_json


def _fixture() -> dict[str, object]:
    return load_json(random_services.FIXTURE)


def _observation(fixture: dict[str, object]) -> dict[str, object]:
    cases = fixture["cases"]
    assert isinstance(cases, list)
    records = []
    for case in cases:
        assert isinstance(case, dict)
        records.append(
            {
                "id": case["id"],
                **random_services.model_case(case),
                "instructionTargetObserved": True,
                "effectiveTargetObserved": True,
                "sourceCopyWriteSeen": True,
            }
        )
    return {
        "system": "GEN",
        "core": "Genesis Plus GX",
        "id": fixture["id"],
        "caseOrder": [case["id"] for case in cases],
        "records": records,
    }


def _status_payload() -> dict[str, object]:
    return {
        "caseId": "unsigned-range-two-retry",
        "phase": "unsigned-generator-return",
        "role": "unsigned-bounded-generator-return",
        "actualPc": 5746,
        "expectedEventPc": 5746,
        "expectedCallPc": 5678,
        "expectedTargetPc": 5714,
        "expectedReturnPc": 5682,
        "pendingCallback": {
            "active": True,
            "caseIndex": 4,
            "generatorCallCount": 3,
            "entrySeen": True,
            "returnSeen": False,
            "instructionTargetObserved": True,
            "effectiveTargetObserved": True,
            "sourceCopyWriteSeen": False,
        },
        "error": "expected callback state",
    }


def _write_status(path: Path, payload: dict[str, object], *preceding_lines: str) -> None:
    path.write_text(
        "\n".join([*preceding_lines, random_services.STATUS_PREFIX + json.dumps(payload)]),
        encoding="utf-8",
    )


def test_provenance_matches_pinned_toolchain_and_h2_owner(tmp_path: Path) -> None:
    fixture = _fixture()
    random_services.validate_provenance(fixture)

    wrong_fixture = copy.deepcopy(fixture)
    wrong_schema = load_json(random_services.FIXTURE_SCHEMA)
    wrong_commit = "0" * 40
    wrong_fixture["provenance"]["upstreamCommit"] = wrong_commit
    wrong_schema["$defs"]["provenance"]["properties"]["upstreamCommit"]["const"] = wrong_commit
    wrong_schema_path = tmp_path / "wrong-schema.json"
    wrong_schema_path.write_text(json.dumps(wrong_schema), encoding="utf-8")
    validate_json(wrong_fixture, wrong_schema_path, owner="mutually wrong fixture/schema")
    with pytest.raises(ValueError, match="pinned toolchain/H2 owner"):
        random_services.validate_provenance(wrong_fixture)


def test_model_separates_helper_return_from_controlled_copy() -> None:
    fixture = _fixture()
    cases = {case["id"]: case for case in fixture["cases"]}
    assert all(random_services.model_case(case) == case["expected"] for case in cases.values())
    assert cases["unsigned-low-byte-zero"]["expected"]["seedCopyAtHelperReturn"] == 0x53C2
    assert cases["unsigned-low-byte-zero"]["expected"]["seedCopyAfterSourceCopy"] == 0x00C2
    assert cases["thinking-low-byte-zero"]["expected"]["seedCopyAtHelperReturn"] == 0x985D
    assert cases["thinking-low-byte-zero"]["expected"]["seedCopyAfterSourceCopy"] == 0x005D
    assert cases["text-symbol-wait-copy-shape"]["expected"]["seedCopyAtHelperReturn"] == 0xABCD
    assert cases["text-symbol-wait-copy-shape"]["expected"]["seedCopyAfterSourceCopy"] == 0xECCD
    assert cases["unsigned-range-two-retry"]["expected"]["generatorCallCount"] == 3
    assert cases["thinking-range-two-retry-alias"]["expected"]["generatorCallCount"] == 57
    assert all(
        state & 0xFF == 0x5D
        for state in cases["thinking-range-two-retry-alias"]["expected"]["generatorStates"]
    )


def test_fixture_and_observation_schemas_are_closed_and_exact() -> None:
    fixture = _fixture()
    validate_json(fixture, random_services.FIXTURE_SCHEMA, owner="fixture")
    observation = _observation(fixture)
    validate_json(observation, random_services.OBSERVATION_SCHEMA, owner="observation")
    random_services._assert_observation(fixture, observation)

    bad_fixture = copy.deepcopy(fixture)
    bad_fixture["sourceContexts"]["textWaitWritePc"] = 26024
    with pytest.raises(ValueError, match="Additional properties"):
        validate_json(bad_fixture, random_services.FIXTURE_SCHEMA, owner="fixture")

    bad_fixture = copy.deepcopy(fixture)
    del bad_fixture["cases"][0]["expected"]["seedCopyAtHelperReturn"]
    with pytest.raises(ValueError, match="required property"):
        validate_json(bad_fixture, random_services.FIXTURE_SCHEMA, owner="fixture")
    bad_fixture = copy.deepcopy(fixture)
    bad_fixture["cases"][0]["expected"]["renamedHelperState"] = bad_fixture["cases"][0][
        "expected"
    ].pop("seedCopyAtHelperReturn")
    with pytest.raises(ValueError, match="Additional properties"):
        validate_json(bad_fixture, random_services.FIXTURE_SCHEMA, owner="fixture")
    bad_fixture = copy.deepcopy(fixture)
    bad_fixture["cases"][0], bad_fixture["cases"][1] = (
        bad_fixture["cases"][1],
        bad_fixture["cases"][0],
    )
    with pytest.raises(ValueError, match="unsigned-low-byte-zero"):
        validate_json(bad_fixture, random_services.FIXTURE_SCHEMA, owner="fixture")
    bad_fixture = copy.deepcopy(fixture)
    bad_fixture["cases"][3]["expected"]["generatorCallCount"] = 4
    with pytest.raises(ValueError, match="3 was expected"):
        validate_json(bad_fixture, random_services.FIXTURE_SCHEMA, owner="fixture")
    bad_fixture = copy.deepcopy(fixture)
    bad_fixture["cases"][3]["expected"]["resultLowByte"] = 1
    with pytest.raises(ValueError, match="0 was expected"):
        validate_json(bad_fixture, random_services.FIXTURE_SCHEMA, owner="fixture")
    bad_observation = copy.deepcopy(observation)
    bad_observation["records"][0]["resultLowByte"] = 256
    with pytest.raises(ValueError, match="maximum"):
        validate_json(bad_observation, random_services.OBSERVATION_SCHEMA, owner="observation")
    bad_observation = copy.deepcopy(observation)
    bad_observation["records"][0]["extra"] = True
    with pytest.raises(ValueError, match="Additional properties"):
        validate_json(bad_observation, random_services.OBSERVATION_SCHEMA, owner="observation")
    bad_observation = copy.deepcopy(observation)
    bad_observation["caseOrder"][0], bad_observation["caseOrder"][1] = (
        bad_observation["caseOrder"][1],
        bad_observation["caseOrder"][0],
    )
    with pytest.raises(ValueError, match="unsigned-low-byte-zero"):
        validate_json(bad_observation, random_services.OBSERVATION_SCHEMA, owner="observation")
    bad_observation = copy.deepcopy(observation)
    bad_observation["records"][3]["generatorOutputs"][0] = 193
    with pytest.raises(ValueError, match="exact observed case matrix"):
        random_services._assert_observation(fixture, bad_observation)


def test_callback_expectations_cover_early_normal_alias_and_generator_roles() -> None:
    fixture = _fixture()
    expectations = random_services.callback_expectations(fixture)
    function = fixture["function"]
    instrumentation = fixture["instrumentation"]
    early = expectations["cases"][0]
    normal = expectations["cases"][3]
    thinking = expectations["cases"][7]
    assert early["unsigned-early-return"]["allowed"] is True
    assert early["unsigned-normal-return"]["allowed"] is False
    assert early["unsigned-normal-return"]["expectedReturnPc"] == function[
        "unsignedEarlyReturnAddress"
    ]
    assert normal["unsigned-normal-return"]["allowed"] is True
    assert normal["unsigned-early-return"]["allowed"] is False
    assert normal["unsigned-normal-return"]["expectedReturnPc"] == function[
        "unsignedNormalReturnAddress"
    ]
    assert thinking["thinking-alias"]["expectedTargetPc"] == function[
        "thinkingAliasEntryAddress"
    ]
    assert thinking["thinking-entry"]["expectedTargetPc"] == function[
        "thinkingBoundedEntryAddress"
    ]
    assert thinking["thinking-alias"]["expectedCallPc"] == instrumentation["helperCallPc"]
    generator = normal["unsigned-generator-return"]
    assert generator["expectedCallPc"] == function["unsignedGeneratorCallAddress"]
    assert generator["expectedTargetPc"] == function["unsignedGeneratorEntryAddress"]
    assert generator["expectedReturnPc"] == function["unsignedGeneratorReturnToCallerAddress"]
    assert expectations["static"]["host-battle-test"]["expectedCallPc"] is None
    assert expectations["static"]["host-number-prompt"]["expectedTargetPc"] is None
    assert expectations["static"]["host-flag-prompt"]["expectedReturnPc"] is None
    turn_order = expectations["static"]["host-turn-order"]
    assert turn_order["expectedCallPc"] is None
    assert turn_order["expectedTargetPc"] is None
    assert turn_order["expectedReturnPc"] == instrumentation["workRamProbePc"]

    for mutate in (
        lambda value: value["cases"][0].pop("unsigned-entry"),
        lambda value: value["cases"][7].__setitem__("extra", {}),
        lambda value: value["cases"][7]["thinking-alias"].__setitem__("role", "wrong-role"),
    ):
        malformed = copy.deepcopy(expectations)
        mutate(malformed)
        with pytest.raises(ValueError, match="callback expectation drift"):
            random_services._validate_callback_expectations(fixture, malformed)


def test_source_parser_rejects_missing_instruction_and_ignores_near_misses() -> None:
    with pytest.raises(ValueError, match="missing"):
        random_services._require_sequence("GenerateRandomNumber:\n rts", ("missing",), name="test")
    calls = random_services._direct_rng_calls(
        "; bsr.w GenerateRandomNumber\n"
        "bsr.w GenerateRandomNumber\n"
        "jsr (GenerateRandomNumber).w\n"
        "bsr.w GenerateRandomNumberNearMiss\n"
    )
    assert calls == ("bsr.w", "jsr")


def test_callback_status_requires_exact_keys_and_types(tmp_path: Path) -> None:
    path = tmp_path / "status.txt"
    _write_status(path, _status_payload(), "milestone:observer-loaded")
    assert "unsigned-range-two-retry" in (random_services._failure_diagnostic(path) or "")
    host_payload = _status_payload()
    host_payload.update(
        {
            "caseId": None,
            "phase": "host-number-prompt",
            "role": "host-number-prompt",
            "actualPc": 90754,
            "expectedEventPc": 90754,
            "expectedCallPc": None,
            "expectedTargetPc": None,
            "expectedReturnPc": None,
        }
    )
    host_payload["pendingCallback"]["active"] = False
    _write_status(path, host_payload, "milestone:observer-loaded")
    assert "host-number-prompt" in (random_services._failure_diagnostic(path) or "")

    malformed = _status_payload()
    del malformed["pendingCallback"]["active"]
    _write_status(path, malformed)
    with pytest.raises(ValueError, match="pending callback state"):
        random_services._failure_diagnostic(path)
    malformed = _status_payload()
    malformed["pendingCallback"]["renamed"] = malformed["pendingCallback"].pop("active")
    _write_status(path, malformed)
    with pytest.raises(ValueError, match="pending callback state"):
        random_services._failure_diagnostic(path)
    malformed = _status_payload()
    malformed["pendingCallback"]["extra"] = True
    _write_status(path, malformed)
    with pytest.raises(ValueError, match="pending callback state"):
        random_services._failure_diagnostic(path)
    for parent, field, value in (
        (None, "caseId", 1),
        (None, "phase", None),
        (None, "actualPc", True),
        (None, "expectedTargetPc", True),
        ("pendingCallback", "active", 1),
        ("pendingCallback", "caseIndex", True),
        ("pendingCallback", "generatorCallCount", False),
    ):
        malformed = _status_payload()
        container = malformed if parent is None else malformed[parent]
        container[field] = value
        _write_status(path, malformed)
        with pytest.raises(ValueError, match="types"):
            random_services._failure_diagnostic(path)

    _write_status(path, _status_payload(), "milestone:observer-loaded")
    with path.open("a", encoding="utf-8") as handle:
        handle.write("\n" + random_services.STATUS_PREFIX + json.dumps(_status_payload()))
    with pytest.raises(ValueError, match="ambiguous"):
        random_services._failure_diagnostic(path)
    path.write_text(
        "milestone:observer-loaded\nmalformed " + random_services.STATUS_PREFIX + "{}",
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="status line"):
        random_services._failure_diagnostic(path)


def test_verifier_promotes_append_status_callback_failure(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    status_root = tmp_path / "derived"
    status_root.mkdir()
    _write_status(
        status_root / "random-services.status.txt",
        _status_payload(),
        "milestone:observer-loaded",
    )

    def fail_observer(**_: object) -> dict[str, object]:
        raise RuntimeError("BizHawk observation failed with exit code 1")

    monkeypatch.setattr(random_services, "DERIVED_ROOT", status_root)
    monkeypatch.setattr(random_services, "verify_runtime_contract", lambda *_: None)
    monkeypatch.setattr(random_services, "validate_static_contract", lambda *_: None)
    monkeypatch.setattr(random_services, "run_observer", fail_observer)
    with pytest.raises(RuntimeError, match="random-services observer callback failure") as error:
        random_services.verify_random_services(tmp_path / "input.bin", timeout_seconds=1)
    message = str(error.value)
    for expected in (
        "unsigned-range-two-retry",
        "unsigned-generator-return",
        "unsigned-bounded-generator-return",
        '"expectedCallPc": 5678',
        '"expectedTargetPc": 5714',
        '"expectedReturnPc": 5682',
        '"pendingCallback":',
    ):
        assert expected in message


def test_verifier_enforces_one_launch(monkeypatch: pytest.MonkeyPatch, tmp_path: Path) -> None:
    fixture = _fixture()
    observed = _observation(fixture)
    launches: list[object] = []
    monkeypatch.setattr(random_services, "verify_runtime_contract", lambda *_: None)
    monkeypatch.setattr(random_services, "validate_static_contract", lambda *_: None)
    monkeypatch.setattr(random_services, "_assert_status", lambda *_: None)
    monkeypatch.setattr(
        random_services, "run_observer", lambda **kwargs: launches.append(kwargs) or observed
    )
    result = random_services.verify_random_services(tmp_path / "input.bin", timeout_seconds=1)
    assert len(launches) == 1
    assert launches[0]["rom_path"] == tmp_path / "input.bin"
    assert launches[0]["config"]["callbackExpectations"] == random_services.callback_expectations(
        fixture
    )
    assert result["SetupHost"] == "debug Battle Test route only"
    assert result["Launches"] == 1
