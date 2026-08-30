from __future__ import annotations

import json
from copy import deepcopy
from pathlib import Path

import pytest

import sf2tool.h3.original_reference_scenario as scenario
from sf2tool.h3.original_reference_transport import canonical_utf8_lf_bytes, sha256
from sf2tool.jsonio import validate_json

SAMPLE_ARTIFACT_DIGESTS = {
    "generic-protocol-bk2": "F36B268F300FF30FE4EB59E4ABF8947E3293FE55A8E4ECB8704DC80B9680EF19",
    "generic-protocol-input-log": (
        "0D06BCD9CC9BCB0E9D7AF9037BF0C40CE25A42BE0B433D8430A470F778EEBD77"
    ),
    "generic-protocol-header": "6AA931653A7AC4C7D27C7064EDFA38EE20441EE58477EC438E7FA4AE4BE5C519",
    "generic-protocol-sync-settings": (
        "7BC4FB51B0CA45F20567DF0E095D8FA5F38DDEB2EF6275445637BE771F7870C6"
    ),
}


def _write_json(path: Path, value: object) -> None:
    path.write_text(json.dumps(value), encoding="utf-8")


def _sample_descriptor() -> dict[str, object]:
    return scenario.load_scenario_descriptor()


def test_public_scenario_preflight_emits_the_complete_typed_contract() -> None:
    descriptor = scenario.load_scenario_descriptor()
    receipt = scenario.run_original_reference_scenario(preflight_only=True)

    assert receipt == {
        "schemaVersion": 1,
        "scenarioApiId": scenario.SCENARIO_API_ID,
        "mode": "PREFLIGHT",
        "status": "PASS",
        "ProcessStarts": 0,
        "descriptor": {
            "status": "validated",
            "identity": scenario.file_identity(scenario.FIXTURE_PATH),
        },
        "scenario": {
            "scenarioId": "generic-protocol-sample",
            "caseId": "generic-protocol-preflight",
            "classification": "Unknown",
            "staticFixtures": descriptor["staticFixtures"],
        },
        "transport": {
            "startState": "power-on",
            "inputArtifacts": descriptor["inputArtifacts"],
            "observer": {
                "observerId": "original-reference-scenario-observer-v1",
                "sha256": "BA35D6F0DEC2DB79856CA1A71998E831BF13DD15120192791A9AEAB9504EDF85",
            },
        },
        "checkpoints": descriptor["checkpoints"],
        "terminalObservation": descriptor["terminalObservation"],
        "candidateLineage": descriptor["candidateLineage"],
        "unknowns": descriptor["unknowns"],
        "observerStatus": None,
        "failure": None,
    }
    validate_json(receipt, scenario.RECEIPT_SCHEMA, owner="scenario preflight receipt")
    assert receipt["ProcessStarts"] == 0
    assert receipt["candidateLineage"]["availability"] == "not-accessed-preflight"
    assert receipt["terminalObservation"]["consoleCheckRequired"] is True


def test_sample_artifact_digests_are_public_domain_separated_identities() -> None:
    descriptor = _sample_descriptor()
    digests = {
        artifact["artifactId"]: artifact["sha256"] for artifact in descriptor["inputArtifacts"]
    }

    assert digests == SAMPLE_ARTIFACT_DIGESTS
    assert {
        artifact_id: scenario.synthetic_artifact_sha256(artifact_id)
        for artifact_id in SAMPLE_ARTIFACT_DIGESTS
    } == SAMPLE_ARTIFACT_DIGESTS
    assert scenario.SYNTHETIC_ARTIFACT_DOMAIN == (
        "sf2tool/original-reference-replay-scenario-api/public-synthetic-artifact/v1:"
    )


def test_generic_descriptor_uses_injected_catalog_and_observer_without_core_changes(
    tmp_path: Path,
) -> None:
    descriptor = _sample_descriptor()
    static_fixture = tmp_path / "generic-static.json"
    static_fixture.write_text('{"public":"synthetic"}\n', encoding="utf-8")
    observer = tmp_path / "generic-observer.lua"
    observer.write_text("local function noop() end\n", encoding="utf-8")
    fixture_id = "future-static-fixture"
    descriptor["scenarioId"] = "future-generic-protocol"
    descriptor["caseId"] = "future-generic-preflight"
    descriptor["staticFixtures"] = [
        {"fixtureId": fixture_id, "sha256": sha256(static_fixture.read_bytes())}
    ]
    descriptor["checkpoints"] = [
        {"role": "future-entry", "address": "0x12345", "staticFixtureId": fixture_id},
        {"role": "future-terminal", "address": "0x12345", "staticFixtureId": fixture_id},
    ]
    for index, artifact in enumerate(descriptor["inputArtifacts"], start=1):
        artifact["artifactId"] = f"future-{artifact['role']}"
        artifact["sha256"] = f"{index:X}" * 64
    descriptor["terminalObservation"]["roleOrder"] = ["future-entry", "future-terminal"]
    descriptor["limits"] = {"maxCheckpoints": 2, "maxTimeoutSeconds": 120}
    descriptor["candidateLineage"]["ledgerId"] = "future-ledger"
    descriptor["passiveObserverPolicy"]["observerId"] = "future-observer"
    descriptor["passiveObserverPolicy"]["observerSha256"] = sha256(
        canonical_utf8_lf_bytes(observer)
    )
    fixture_path = tmp_path / "future-descriptor.json"
    _write_json(fixture_path, descriptor)

    loaded = scenario.load_scenario_descriptor(
        fixture_path,
        static_fixture_catalog={fixture_id: static_fixture},
        observer_path=observer,
    )
    receipt = scenario.preflight_original_reference_scenario(
        fixture_path,
        static_fixture_catalog={fixture_id: static_fixture},
        observer_path=observer,
    )

    assert loaded["scenarioId"] == "future-generic-protocol"
    assert [artifact["artifactId"] for artifact in loaded["inputArtifacts"]] == [
        "future-movie",
        "future-input-log",
        "future-header",
        "future-sync-settings",
    ]
    assert [checkpoint["role"] for checkpoint in receipt["checkpoints"]] == [
        "future-entry",
        "future-terminal",
    ]
    assert receipt["ProcessStarts"] == 0


@pytest.mark.parametrize(
    ("name", "mutate"),
    [
        (
            "duplicate-fixture",
            lambda value: value["staticFixtures"].append(deepcopy(value["staticFixtures"][0])),
        ),
        (
            "duplicate-artifact-id",
            lambda value: value["inputArtifacts"][1].update(
                artifactId=value["inputArtifacts"][0]["artifactId"]
            ),
        ),
        (
            "duplicate-artifact-role",
            lambda value: value["inputArtifacts"][1].update(role="movie"),
        ),
        (
            "duplicate-role",
            lambda value: value["checkpoints"][1].update(role="turn-finalization-resume"),
        ),
        (
            "missing-checkpoint-fixture",
            lambda value: value["checkpoints"][0].update(staticFixtureId="missing-fixture"),
        ),
        (
            "role-order-mismatch",
            lambda value: value["terminalObservation"].update(
                roleOrder=list(reversed(value["terminalObservation"]["roleOrder"]))
            ),
        ),
        (
            "noncontiguous-shared-pc",
            lambda value: value["checkpoints"][2].update(address="0x24106"),
        ),
    ],
)
def test_generic_descriptor_cross_field_guards_fail_closed(
    tmp_path: Path, name: str, mutate: object
) -> None:
    descriptor = _sample_descriptor()
    mutate(descriptor)  # type: ignore[operator]
    fixture_path = tmp_path / f"{name}.json"
    _write_json(fixture_path, descriptor)

    receipt = scenario.preflight_original_reference_scenario(fixture_path)

    assert receipt["status"] == "FAIL"
    assert receipt["ProcessStarts"] == 0
    assert receipt["failure"]["code"] == "descriptor-contract"


def test_descriptor_rejects_path_values_extra_keys_and_raw_payloads(tmp_path: Path) -> None:
    descriptor = _sample_descriptor()
    descriptor["inputArtifacts"][0]["artifactId"] = "C:\\private-movie"
    path_value = tmp_path / "path-value.json"
    _write_json(path_value, descriptor)
    assert scenario.preflight_original_reference_scenario(path_value)["failure"]["code"] == (
        "descriptor-schema"
    )

    descriptor = _sample_descriptor()
    descriptor["inputArtifacts"][0]["payload"] = "not-allowed"
    raw_payload = tmp_path / "raw-payload.json"
    _write_json(raw_payload, descriptor)
    assert scenario.preflight_original_reference_scenario(raw_payload)["failure"]["code"] == (
        "descriptor-schema"
    )

    descriptor = _sample_descriptor()
    descriptor["unknowns"][0]["question"] = "file:///private/capture"
    path_question = tmp_path / "path-question.json"
    _write_json(path_question, descriptor)
    assert scenario.preflight_original_reference_scenario(path_question)["failure"]["code"] == (
        "descriptor-schema"
    )


@pytest.mark.parametrize(
    ("name", "mutate"),
    [
        (
            "ordinal-zero",
            lambda value: value["candidateLineage"].update(
                availability="available-runtime",
                runClass="diagnostic",
                launchOrdinal=0,
            ),
        ),
        (
            "ordinal-four",
            lambda value: value["candidateLineage"].update(
                availability="available-runtime",
                runClass="diagnostic",
                launchOrdinal=4,
            ),
        ),
        (
            "runtime-without-run-class",
            lambda value: value["candidateLineage"].update(
                availability="available-runtime",
                runClass=None,
                launchOrdinal=1,
            ),
        ),
        (
            "later-runtime-without-prior-receipt",
            lambda value: value["candidateLineage"].update(
                availability="available-runtime",
                runClass="diagnostic",
                launchOrdinal=2,
                priorReceiptSha256=None,
            ),
        ),
    ],
)
def test_lineage_stop_loss_rejects_out_of_window_and_missing_predecessor(
    tmp_path: Path, name: str, mutate: object
) -> None:
    descriptor = _sample_descriptor()
    mutate(descriptor)  # type: ignore[operator]
    fixture_path = tmp_path / f"lineage-{name}.json"
    _write_json(fixture_path, descriptor)

    receipt = scenario.preflight_original_reference_scenario(fixture_path)

    assert receipt["status"] == "FAIL"
    assert receipt["ProcessStarts"] == 0
    assert receipt["failure"]["code"] == "descriptor-schema"


@pytest.mark.parametrize(
    ("source", "message"),
    [
        ('event["onmemoryexecute"]()', "dynamic member access"),
        ('local callback = event["onmemoryexecute"]', "dynamic member access"),
        ("local callback = event.onmemoryexecute\ncallback()", "aliases API member"),
        ("api = event\napi.onmemoryexecute()", "aliases API namespace"),
        ("event:onmemoryexecute()", "unallowed API"),
        ("memory.write_u8(0, 1)", "memory."),
        ("joypad.set(1, {})", "joypad."),
        ("require('route')", "require"),
    ],
)
def test_generic_observer_rejects_aliases_dynamic_access_and_all_write_surfaces(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path, source: str, message: str
) -> None:
    observer = tmp_path / "observer.lua"
    observer.write_text(source + "\n", encoding="utf-8")
    descriptor = _sample_descriptor()
    descriptor["passiveObserverPolicy"]["observerSha256"] = sha256(
        canonical_utf8_lf_bytes(observer)
    )
    monkeypatch.setattr(scenario, "DEFAULT_OBSERVER_PATH", observer)

    with pytest.raises(scenario.PassiveObserverPolicyError, match=message):
        scenario._validate_observer_policy(descriptor, observer_path=observer)


@pytest.mark.parametrize(
    ("field", "mutate"),
    [
        ("allowedApis", lambda value: value.append("emu.frameadvance")),
        ("allowedBareCalls", lambda value: value.__setitem__(0, "print")),
        ("forbiddenCapabilities", lambda value: value.append("trace-write")),
    ],
)
def test_descriptor_cannot_relax_immutable_passive_policy(
    tmp_path: Path, field: str, mutate: object
) -> None:
    descriptor = _sample_descriptor()
    mutate(descriptor["passiveObserverPolicy"][field])  # type: ignore[operator]
    fixture_path = tmp_path / f"unapproved-{field}.json"
    _write_json(fixture_path, descriptor)

    receipt = scenario.preflight_original_reference_scenario(fixture_path)

    assert receipt["status"] == "FAIL"
    assert receipt["ProcessStarts"] == 0
    assert receipt["failure"]["code"] == "descriptor-schema"
    with pytest.raises(scenario.PassiveObserverPolicyError, match="immutable policy"):
        scenario._validate_observer_policy(
            descriptor,
            observer_path=scenario.DEFAULT_OBSERVER_PATH,
        )


def test_observer_status_contract_is_typed_and_requires_outer_console_check() -> None:
    source = scenario.DEFAULT_OBSERVER_PATH.read_text(encoding="utf-8")

    assert '{ address = 0x23CBA, roles = { "victory-entry", "declared-terminal" } }' in source
    assert "for _, role in ipairs(group.roles) do" in source
    assert "local callback_ok, callback_error = pcall(function()" in source
    assert "local cleanup_ok, remaining = pcall(clear_callbacks)" in source
    assert "client.exitCode(finished and 0 or 1)" in source
    for required in (
        '\\"phase\\"',
        '\\"code\\"',
        '\\"caseId\\"',
        '\\"currentRole\\"',
        '\\"callbacksRemaining\\"',
        '\\"cleanupResult\\"',
        '\\"observedRoles\\"',
        '\\"detail\\"',
        '\\"exitCode\\"',
        '\\"consoleCheckRequired\\":true',
        '"callback-exception"',
        '"callback-cleanup"',
    ):
        assert required in source
    assert "consoleClean" not in source
    assert "json_optional(case_id)" in source
    assert '"<unknown>"' not in source
    for required in (
        "local JSON_TEXT_MAX_BYTES = 500",
        "local function utf8_safe_prefix(text, max_bytes)",
        "text = utf8_safe_prefix(text, text_limit) .. JSON_TEXT_TRUNCATION",
        "elseif byte == 47 or byte == 92 or byte < 32 or byte == 127 then",
        "escaped[#escaped + 1] = string.char(byte)",
    ):
        assert required in source
    for api_name in ("string.byte", "string.sub", "string.char"):
        assert api_name in scenario.ALLOWED_LUA_API_NAMES

    runtime_failure = {
        "status": "FAIL",
        "phase": "callback",
        "code": "callback-exception",
        "caseId": "generic-protocol-preflight",
        "currentRole": "victory-entry",
        "expected": "callback-success",
        "actual": "synthetic failure",
        "callbacksRemaining": 2,
        "cleanupResult": "protected-cleanup-failed",
        "observedRoles": ["victory-entry"],
        "detail": "synthetic status-shape test",
        "exitCode": 1,
        "consoleCheckRequired": True,
    }
    receipt = scenario.preflight_original_reference_scenario()
    receipt.update(mode="RUNTIME", status="FAIL", ProcessStarts=1, observerStatus=runtime_failure)
    receipt["candidateLineage"].update(
        availability="available-runtime",
        runClass="diagnostic",
        launchOrdinal=1,
    )
    receipt.update(
        failure={
            "phase": "runtime",
            "code": "observer-status",
            "expected": "typed observer failure",
            "actual": "synthetic status-shape test",
        },
    )
    validate_json(receipt, scenario.RECEIPT_SCHEMA, owner="runtime observer failure shape")
    receipt["ProcessStarts"] = 2
    with pytest.raises(ValueError):
        validate_json(receipt, scenario.RECEIPT_SCHEMA, owner="runtime observer process count")
    receipt["ProcessStarts"] = 1
    receipt["observerStatus"]["exitCode"] = 0
    with pytest.raises(ValueError):
        validate_json(receipt, scenario.RECEIPT_SCHEMA, owner="runtime observer failure exit")
    receipt["observerStatus"]["exitCode"] = 1
    receipt["observerStatus"]["extra"] = "not-allowed"
    with pytest.raises(ValueError):
        validate_json(receipt, scenario.RECEIPT_SCHEMA, owner="runtime observer extra field")
    del receipt["observerStatus"]["extra"]
    receipt["observerStatus"]["detail"] = "C:\\private\\observer.lua"
    with pytest.raises(ValueError):
        validate_json(receipt, scenario.RECEIPT_SCHEMA, owner="runtime observer path detail")
    receipt["observerStatus"]["detail"] = "synthetic status-shape test"
    receipt["observerStatus"]["caseId"] = None
    with pytest.raises(ValueError):
        validate_json(receipt, scenario.RECEIPT_SCHEMA, owner="runtime observer missing case")
    receipt["observerStatus"]["caseId"] = "generic-protocol-preflight"

    runtime_success = deepcopy(receipt)
    runtime_success.update(status="PASS", observerStatus=deepcopy(runtime_failure), failure=None)
    runtime_success["descriptor"] = {
        "status": "validated",
        "identity": scenario.file_identity(scenario.FIXTURE_PATH),
    }
    runtime_success["scenario"] = {
        "scenarioId": "generic-protocol-sample",
        "caseId": "generic-protocol-preflight",
        "classification": "Unknown",
        "staticFixtures": _sample_descriptor()["staticFixtures"],
    }
    runtime_success["transport"] = {
        "startState": "power-on",
        "inputArtifacts": _sample_descriptor()["inputArtifacts"],
        "observer": {
            "observerId": "original-reference-scenario-observer-v1",
            "sha256": _sample_descriptor()["passiveObserverPolicy"]["observerSha256"],
        },
    }
    runtime_success["checkpoints"] = _sample_descriptor()["checkpoints"]
    runtime_success["terminalObservation"] = _sample_descriptor()["terminalObservation"]
    runtime_success["unknowns"] = _sample_descriptor()["unknowns"]
    runtime_success["observerStatus"].update(
        status="PASS",
        phase="terminal",
        code="pass",
        callbacksRemaining=0,
        cleanupResult="protected-cleanup-ok",
        exitCode=0,
    )
    validate_json(runtime_success, scenario.RECEIPT_SCHEMA, owner="runtime observer pass shape")
    runtime_success["observerStatus"]["callbacksRemaining"] = 1
    with pytest.raises(ValueError):
        validate_json(
            runtime_success,
            scenario.RECEIPT_SCHEMA,
            owner="runtime observer pass cleanup",
        )


@pytest.mark.parametrize(
    ("name", "mutate"),
    [
        ("ordinal-zero", lambda value: value.update(launchOrdinal=0)),
        ("ordinal-four", lambda value: value.update(launchOrdinal=4)),
        ("missing-run-class", lambda value: value.update(runClass=None)),
        (
            "later-missing-prior-receipt",
            lambda value: value.update(launchOrdinal=2, priorReceiptSha256=None),
        ),
    ],
)
def test_runtime_receipt_lineage_stop_loss_is_schema_closed(name: str, mutate: object) -> None:
    receipt = scenario.preflight_original_reference_scenario()
    receipt.update(
        mode="RUNTIME",
        status="FAIL",
        ProcessStarts=1,
        observerStatus={
            "status": "FAIL",
            "phase": "callback",
            "code": "callback-exception",
            "caseId": "generic-protocol-preflight",
            "currentRole": "victory-entry",
            "callbacksRemaining": 0,
            "cleanupResult": "protected-cleanup-ok",
            "observedRoles": ["victory-entry"],
            "expected": "callback-success",
            "actual": "synthetic failure",
            "detail": "lineage stop-loss test",
            "exitCode": 1,
            "consoleCheckRequired": True,
        },
        failure={
            "phase": "runtime",
            "code": "observer-status",
            "expected": "typed observer failure",
            "actual": "lineage stop-loss test",
        },
    )
    receipt["candidateLineage"].update(
        availability="available-runtime",
        runClass="diagnostic",
        launchOrdinal=1,
        priorReceiptSha256=None,
    )
    validate_json(receipt, scenario.RECEIPT_SCHEMA, owner="runtime lineage baseline")

    frozen_acceptance = deepcopy(receipt)
    frozen_acceptance["candidateLineage"].update(
        runClass="frozen-acceptance",
        launchOrdinal=3,
        priorReceiptSha256="A" * 64,
    )
    validate_json(
        frozen_acceptance,
        scenario.RECEIPT_SCHEMA,
        owner="frozen acceptance lineage",
    )

    mutate(receipt["candidateLineage"])  # type: ignore[operator]
    with pytest.raises(ValueError):
        validate_json(receipt, scenario.RECEIPT_SCHEMA, owner=f"runtime lineage {name}")


def test_scenario_facade_has_no_emulator_launch_or_private_ledger_path() -> None:
    source = Path(scenario.__file__).read_text(encoding="utf-8")

    for forbidden in ("run_native_bizhawk_process", "subprocess", "DERIVED_ROOT", "launch-ledger"):
        assert forbidden not in source
    with pytest.raises(scenario.ScenarioError, match="--preflight-only"):
        scenario.run_original_reference_scenario(preflight_only=False)
