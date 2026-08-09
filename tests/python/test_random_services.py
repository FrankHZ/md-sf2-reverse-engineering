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
                **random_services.model_case(case, fixture["sourceContext"]),
                "instructionTargetObserved": True,
                "effectiveTargetObserved": True,
                "sourceCopyWriteSeen": True,
                "callerExecutionObserved": case["callerExecutionObserved"],
                "callerPreambleSeen": case["callerExecutionObserved"],
                "callerRangeSeen": case["callerExecutionObserved"],
                "callerRngCallSeen": case["callerExecutionObserved"],
                "callerCallSeen": case["callerExecutionObserved"],
                "callerStoreSeen": case["callerExecutionObserved"],
                "callerRestoreSeen": case["callerExecutionObserved"],
                "callerWaitCallSeen": case["callerExecutionObserved"],
                "callerWaitTargetSeen": case["callerExecutionObserved"],
                "callerWaitRtsSeen": case["callerExecutionObserved"],
                "callerContinuationSeen": case["callerExecutionObserved"],
                "callerHelperReturnRedirectSeen": case["callerExecutionObserved"],
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
        "owner": "random-services",
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
            "callerCallSeen": False,
            "callerContinuationPending": False,
            "callerContinuationSeen": False,
            "callerHelperReturnRedirectSeen": False,
            "callerPreambleSeen": False,
            "callerRangeSeen": False,
            "callerRngCallSeen": False,
            "callerRestoreSeen": False,
            "callerStoreSeen": False,
            "callerWaitCallSeen": False,
            "callerWaitRtsSeen": False,
            "callerWaitTargetSeen": False,
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
    source_contexts = fixture["sourceContext"]
    assert all(
        random_services.model_case(case, source_contexts) == case["expected"]
        for case in cases.values()
    )
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
    assert cases["text-symbol-wait-caller-seam-thinking-range-two"]["expected"][
        "seedCopyAfterSourceCopy"
    ] == 0xECCD
    assert cases["text-symbol-wait-caller-seam-thinking-range-two"]["expected"][
        "seedCopyAtHelperReturn"
    ] == 0x00CD
    assert cases["diamond-menu-caller-seam-thinking-range-two"]["expected"][
        "seedCopyAfterSourceCopy"
    ] == 0x6833
    assert cases["diamond-menu-caller-seam-thinking-range-two"]["expected"][
        "seedCopyAtHelperReturn"
    ] == 0x0133


def test_fixture_and_observation_schemas_are_closed_and_exact() -> None:
    fixture = _fixture()
    validate_json(fixture, random_services.FIXTURE_SCHEMA, owner="fixture")
    observation = _observation(fixture)
    validate_json(observation, random_services.OBSERVATION_SCHEMA, owner="observation")
    random_services._assert_observation(fixture, observation)

    bad_fixture = copy.deepcopy(fixture)
    bad_fixture["sourceContext"]["textWaitWritePc"] = 26024
    with pytest.raises(ValueError, match="Additional properties"):
        validate_json(bad_fixture, random_services.FIXTURE_SCHEMA, owner="fixture")

    bad_fixture = copy.deepcopy(fixture)
    bad_fixture["cases"][10]["callerExecutionObserved"] = False
    with pytest.raises(ValueError, match="True was expected"):
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
    bad_observation["records"][10]["callerStoreSeen"] = False
    with pytest.raises(ValueError, match="True was expected"):
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
    text_caller = expectations["cases"][10]
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
    assert "caller-call" not in text_caller
    assert text_caller["base-entry"]["expectedCallPc"] == fixture["sourceContext"][
        "textWaitCallPc"
    ]
    assert text_caller["base-entry"]["expectedTargetPc"] == function["baseEntryAddress"]
    assert text_caller["base-entry"]["expectedReturnPc"] == fixture["sourceContext"][
        "textWaitStorePc"
    ]
    assert text_caller["thinking-alias"]["expectedCallPc"] == instrumentation["helperCallPc"]
    assert text_caller["thinking-alias"]["expectedReturnPc"] == instrumentation["sourceCopyWritePc"]
    assert text_caller["case-result"]["expectedReturnPc"] == instrumentation["resultPc"]
    assert text_caller["case-entry"]["expectedCallPc"] is None
    assert text_caller["case-entry"]["expectedTargetPc"] is None
    assert text_caller["case-entry"]["expectedReturnPc"] is None
    assert text_caller["caller-preamble"] == {
        "phase": "caller-preamble",
        "role": "caller-preamble",
        "expectedEventPc": fixture["sourceContext"]["textWaitPreamblePc"],
        "expectedCallPc": instrumentation["helperCallPc"],
        "expectedTargetPc": fixture["sourceContext"]["textWaitPreamblePc"],
        "expectedReturnPc": instrumentation["sourceCopyWritePc"],
        "allowed": True,
    }
    assert text_caller["caller-range-load"]["expectedEventPc"] == fixture["sourceContext"][
        "textWaitRangePc"
    ]
    assert text_caller["caller-rng-call"] == {
        "phase": "caller-rng-call",
        "role": "caller-rng-call",
        "expectedEventPc": fixture["sourceContext"]["textWaitCallPc"],
        "expectedCallPc": fixture["sourceContext"]["textWaitCallPc"],
        "expectedTargetPc": function["baseEntryAddress"],
        "expectedReturnPc": fixture["sourceContext"]["textWaitStorePc"],
        "allowed": True,
    }
    assert text_caller["caller-post-store"]["role"] == "caller-post-store-restore"
    assert text_caller["caller-wait-call"]["expectedReturnPc"] == fixture["sourceContext"][
        "textWaitVIntReturnPc"
    ]
    assert text_caller["wait-for-vint-target"]["expectedTargetPc"] == fixture[
        "sourceContext"
    ]["waitForVIntEntryPc"]
    assert text_caller["wait-for-vint-rts"]["expectedReturnPc"] == instrumentation[
        "callerContinuationPc"
    ]
    assert text_caller["caller-continuation"]["expectedEventPc"] == instrumentation[
        "caseEntryPc"
    ]
    assert text_caller["caller-continuation"]["expectedReturnPc"] == instrumentation[
        "callerContinuationPc"
    ]

    address_drift = copy.deepcopy(fixture)
    address_drift["sourceContext"]["textWaitCallPc"] += 2
    with pytest.raises(ValueError, match="H1 caller source-context address drift"):
        random_services.callback_expectations(address_drift)
    return_drift = copy.deepcopy(fixture)
    return_drift["sourceContext"]["textWaitStorePc"] += 2
    with pytest.raises(ValueError, match="H1 caller source-context address drift"):
        random_services.callback_expectations(return_drift)
    for key in (
        "textWaitPreamblePc",
        "textWaitRangePc",
        "textWaitVIntCallPc",
        "textWaitVIntReturnPc",
        "diamondPreamblePc",
        "diamondRangePc",
        "diamondVIntCallPc",
        "diamondVIntReturnPc",
        "waitForVIntEntryPc",
        "waitForVIntRtsPc",
    ):
        drift = copy.deepcopy(fixture)
        drift["sourceContext"][key] += 2
        with pytest.raises(ValueError, match="H1 caller source-context address drift"):
            random_services._observer_config(drift)

    for mutate in (
        lambda value: value["cases"][0].pop("unsigned-entry"),
        lambda value: value["cases"][7].__setitem__("extra", {}),
        lambda value: value["cases"][7]["thinking-alias"].__setitem__("role", "wrong-role"),
        lambda value: value["cases"][10].pop("caller-continuation"),
        lambda value: value["cases"][10].__setitem__("extra-continuation", {}),
        lambda value: value["cases"][10]["caller-continuation"].__setitem__(
            "role", "wrong-continuation-role"
        ),
    ):
        malformed = copy.deepcopy(expectations)
        mutate(malformed)
        with pytest.raises(ValueError, match="callback expectation drift"):
            random_services._validate_callback_expectations(fixture, malformed)


def test_lua_caller_dispatch_uses_real_preamble_jsr_and_shared_pc_continuation() -> None:
    source = random_services.OBSERVER.read_text(encoding="utf-8")
    assert 'memory.read_u32_be(stack,"M68K BUS")' in source
    assert '"caller source stack return"' in source
    assert 'emu.setregister("M68K PC"' not in source
    assert 'register_exec(config.sourceContexts.textWaitCallPc,"caller-call"' not in source
    assert 'register_exec(config.sourceContexts.diamondCallPc,"caller-call"' not in source
    assert 'register_exec(i.caseEntryPc,"case-entry",begin_case)' in source
    assert source.count('register_exec(i.caseEntryPc,"case-entry",begin_case)') == 1
    assert 'phase=="case-entry" and active and caller_continuation_pending' in source
    assert 'return "caller-continuation"' in source
    assert 'memory.write_u32_be(stack,i.callerContinuationPc,"M68K BUS")' in source
    assert '"WaitForVInt rewritten stack return"' in source
    assert '"caller continuation range input"' in source
    assert '"caller continuation thinking target"' in source
    assert '"caller helper original probe return"' in source
    assert '"caller helper result return redirect"' in source
    assert (
        'if c.callerExecutionObserved then error("caller reached controlled probe copy") end'
        in source
    )
    for error in (
        "duplicate caller WaitForVInt call",
        "duplicate WaitForVInt target",
        "duplicate WaitForVInt RTS",
    ):
        assert f'error("{error}")' in source

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


def test_caller_source_h1_and_rom_guards_reject_all_bounded_seam_drift(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    fixture = _fixture()
    rom_path = random_services.repo_path("local/roms/sf2-us.bin")

    h1_drift = tmp_path / "caller-h1-drift.lst"
    h1_drift.write_text(
        random_services.H1_LISTING.read_text(encoding="utf-8").replace(
            "0000659C 48E7 0300", "0000659C 48E7 0301", 1
        ),
        encoding="utf-8",
    )
    monkeypatch.setattr(random_services, "H1_LISTING", h1_drift)
    with pytest.raises(ValueError, match="H1 caller context run drift"):
        random_services.validate_caller_source_contexts(fixture)
    h1_listing = random_services.repo_path("local/upstream/SF2DISASM/build/sf2build-h1.lst")
    monkeypatch.setattr(random_services, "H1_LISTING", h1_listing)

    source_drift = tmp_path / "textfunctions_1.asm"
    text_source = random_services.TEXT_SOURCE.read_text(encoding="utf-8")
    symbol_wait1 = text_source.index("symbol_wait1:")
    source_drift.write_text(
        text_source[:symbol_wait1]
        + text_source[symbol_wait1:].replace("movem.l d6-d7,-(sp)", "movem.l d5-d7,-(sp)", 1),
        encoding="utf-8",
    )
    monkeypatch.setattr(random_services, "TEXT_SOURCE", source_drift)
    with pytest.raises(ValueError, match="text symbol_wait1"):
        random_services.validate_static_contract(fixture, rom_path)
    monkeypatch.setattr(
        random_services,
        "TEXT_SOURCE",
        random_services.UPSTREAM / "code/common/scripting/text/textfunctions_1.asm",
    )

    rom_drift = tmp_path / "caller-wait-rom-drift.bin"
    rom = bytearray(rom_path.read_bytes())
    rom[fixture["sourceContext"]["textWaitVIntCallPc"]] ^= 0x01
    rom_drift.write_bytes(rom)
    with pytest.raises(ValueError, match="H1/ROM guard failed for text wait source preamble"):
        random_services.validate_static_contract(fixture, rom_drift)


def test_golden_caller_results_are_independently_derived_before_runtime() -> None:
    fixture = _fixture()
    observed = _observation(fixture)
    golden_drift = copy.deepcopy(fixture)
    golden_drift["cases"][10]["expected"]["seedCopyAtHelperReturn"] += 1
    with pytest.raises(ValueError, match="golden disagrees with model"):
        random_services._assert_observation(golden_drift, observed)


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
    with pytest.raises(ValueError, match="pendingCallback"):
        random_services._failure_diagnostic(path)
    malformed = _status_payload()
    malformed["pendingCallback"]["renamed"] = malformed["pendingCallback"].pop("active")
    _write_status(path, malformed)
    with pytest.raises(ValueError, match="pendingCallback"):
        random_services._failure_diagnostic(path)
    malformed = _status_payload()
    malformed["pendingCallback"]["extra"] = True
    _write_status(path, malformed)
    with pytest.raises(ValueError, match="pendingCallback"):
        random_services._failure_diagnostic(path)
    for parent, field, value in (
        (None, "caseId", 1),
        (None, "phase", None),
        (None, "actualPc", True),
        (None, "actualPc", None),
        (None, "expectedTargetPc", True),
        ("pendingCallback", "active", 1),
        ("pendingCallback", "callerCallSeen", 1),
        ("pendingCallback", "caseIndex", True),
        ("pendingCallback", "generatorCallCount", False),
    ):
        malformed = _status_payload()
        container = malformed if parent is None else malformed[parent]
        container[field] = value
        _write_status(path, malformed)
        with pytest.raises(ValueError, match="failed schema validation"):
            random_services._failure_diagnostic(path)

    _write_status(path, _status_payload(), "milestone:observer-loaded")
    with path.open("a", encoding="utf-8") as handle:
        handle.write("\n" + random_services.STATUS_PREFIX + json.dumps(_status_payload()))
    with pytest.raises(ValueError, match="multiplicity"):
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
    assert all("expected" not in case for case in launches[0]["config"]["cases"])
    assert launches[0]["config"]["sourceContexts"] == fixture["sourceContext"]
    assert "sourceContext" not in launches[0]["config"]
    assert launches[0]["config"]["callbackExpectations"] == random_services.callback_expectations(
        fixture
    )
    assert (
        launches[0]["config"]["observerFailureContract"]
        == random_services.OBSERVER_FAILURE_CONTRACT
    )
    assert result["SetupHost"] == "debug Battle Test route only"
    assert result["Launches"] == 1
