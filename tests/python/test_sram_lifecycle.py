from __future__ import annotations

import copy
import json
from pathlib import Path
from typing import Any

import pytest

from sf2tool.h3 import sram_lifecycle
from sf2tool.jsonio import load_json, validate_json


def _fixture() -> dict[str, object]:
    return load_json(sram_lifecycle.FIXTURE)


def _static(fixture: dict[str, object]) -> dict[str, object]:
    return sram_lifecycle.build_static_contract(
        fixture, sram_lifecycle.repo_path("local/upstream/SF2DISASM")
    )


def _write_h2_owner(tmp_path: Path, owner: dict[str, Any]) -> Path:
    path = tmp_path / "tech-services-static-owner.json"
    path.write_text(json.dumps(owner), encoding="utf-8")
    return path


def test_fixture_and_observation_schemas_are_recursively_closed_and_exact() -> None:
    fixture = _fixture()
    validate_json(fixture, sram_lifecycle.FIXTURE_SCHEMA, owner="fixture")
    static = _static(fixture)
    assert fixture["function"]["checkSramAddress"] == static["functionEntries"]["CheckSram"]
    observed = sram_lifecycle.expected_observation(fixture, static)
    validate_json(observed, sram_lifecycle.OBSERVATION_SCHEMA, owner="observation")
    sram_lifecycle._assert_observation(fixture, static, observed)

    bad_fixture = copy.deepcopy(fixture)
    del bad_fixture["cases"][0]["setup"]["ramSeed"]
    with pytest.raises(ValueError, match="required property"):
        validate_json(bad_fixture, sram_lifecycle.FIXTURE_SCHEMA, owner="fixture")
    bad_fixture = copy.deepcopy(fixture)
    bad_fixture["cases"][0]["setup"]["renamedSeed"] = bad_fixture["cases"][0]["setup"].pop(
        "slot1Seed"
    )
    with pytest.raises(ValueError, match="Additional properties"):
        validate_json(bad_fixture, sram_lifecycle.FIXTURE_SCHEMA, owner="fixture")
    bad_fixture = copy.deepcopy(fixture)
    bad_fixture["caseOrder"][0], bad_fixture["caseOrder"][1] = (
        bad_fixture["caseOrder"][1],
        bad_fixture["caseOrder"][0],
    )
    with pytest.raises(ValueError, match="signature-mismatch-init"):
        validate_json(bad_fixture, sram_lifecycle.FIXTURE_SCHEMA, owner="fixture")
    bad_fixture = copy.deepcopy(fixture)
    bad_fixture["cases"][6]["operation"] = "load"
    with pytest.raises(ValueError, match="operation/selector drift"):
        sram_lifecycle.expected_observation(bad_fixture, static)

    bad_observation = copy.deepcopy(observed)
    bad_observation["records"][0]["slotFacts"][0]["span"]["extra"] = True
    with pytest.raises(ValueError, match="Additional properties"):
        validate_json(bad_observation, sram_lifecycle.OBSERVATION_SCHEMA, owner="observation")
    bad_observation = copy.deepcopy(observed)
    bad_observation["records"][0]["slotFacts"][0]["span"]["sentinels"][0][
        "logicalOffset"
    ] = 2
    with pytest.raises(ValueError, match="is not one of"):
        validate_json(bad_observation, sram_lifecycle.OBSERVATION_SCHEMA, owner="observation")
    bad_observation = copy.deepcopy(observed)
    bad_observation["records"][0]["resultD0"] = 2
    with pytest.raises(ValueError, match="is not one of"):
        validate_json(bad_observation, sram_lifecycle.OBSERVATION_SCHEMA, owner="observation")
    bad_observation = copy.deepcopy(observed)
    bad_observation["records"][10]["slotFacts"][1]["span"]["checksumByte"] ^= 1
    with pytest.raises(ValueError, match="runtime matrix mismatch"):
        sram_lifecycle._assert_observation(fixture, static, bad_observation)


def test_static_derivation_uses_h2_owner_h1_entries_and_source_use_sites() -> None:
    fixture = _fixture()
    static = _static(fixture)
    assert static["functionEntries"] == {
        "CheckSram": 28326,
        "SaveGame": 28522,
        "LoadGame": 28588,
        "CopySave": 28634,
        "ClearSaveSlotFlag": 28652,
        "CopyBytesToSram": 28676,
        "CopyBytesFromSram": 28700,
    }
    assert static["layout"]["logicalBytesPerSlot"] == 4016
    assert static["layout"]["physicalAddressIntervalPerSlot"] == 8032
    assert static["layout"]["physicalAddressStepPerLogicalByte"] == 2
    assert static["layout"]["fullClearLogicalByteCount"] == 8192
    assert static["layout"]["occupiedFlagBits"] == {"slot1": 0, "slot2": 1}
    assert static["copyFlow"] == {
        "loadCallPc": 28636,
        "loadReturnPc": 28638,
        "saveCallPc": 28646,
        "saveReturnPc": 28648,
    }
    assert bytes(static["signatureBytes"]) == b"Taguchi New Supra"


@pytest.mark.parametrize(
    ("mutation", "error"),
    (
        ("rom-sha", "provenance disagrees"),
        ("upstream-commit", "provenance disagrees"),
        ("function-entry", "H2/H1 entry derivation drift: SaveGame"),
        ("logical-size", "logical payload size"),
        ("physical-stride", "storage stride"),
        ("full-clear", "full-clear counter"),
        ("flag-bit", "semantic drift in CheckSram"),
        ("copy-operation", "CopySave operation fact disagrees"),
    ),
)
def test_wrong_but_structurally_valid_h2_owner_facts_fail_before_runtime(
    tmp_path: Path, mutation: str, error: str
) -> None:
    fixture = _fixture()
    owner = load_json(sram_lifecycle.H2_FIXTURE)
    facts = owner["expected"]["sramFacts"]
    if mutation == "rom-sha":
        owner["romSha256"] = "0" * 64
    elif mutation == "upstream-commit":
        owner["upstreamCommit"] = "0" * 40
    elif mutation == "function-entry":
        facts["functionEntries"]["SaveGame"] += 2
    elif mutation == "logical-size":
        facts["layout"]["logicalBytesPerSlot"] -= 1
    elif mutation == "physical-stride":
        facts["layout"]["physicalAddressStepPerLogicalByte"] = 1
    elif mutation == "full-clear":
        facts["layout"]["fullClearLogicalByteCount"] -= 1
    elif mutation == "flag-bit":
        facts["layout"]["occupiedFlagBits"]["slot1"] = 1
    elif mutation == "copy-operation":
        facts["operations"]["copyLoadsSelectedSlotThenSavesToOtherSlot"] = False
    else:
        raise AssertionError(f"uncovered mutation: {mutation}")

    owner_path = _write_h2_owner(tmp_path, owner)
    with pytest.raises(ValueError, match=error):
        sram_lifecycle.build_static_contract(
            fixture,
            sram_lifecycle.repo_path("local/upstream/SF2DISASM"),
            h2_fixture_path=owner_path,
        )


def test_source_guard_rejects_instruction_mutation_and_ignores_comment_near_miss() -> None:
    fixture = _fixture()
    root = sram_lifecycle.repo_path("local/upstream/SF2DISASM")
    source = (root / "disasm" / sram_lifecycle.SRAM_SOURCE_RELATIVE).read_text(encoding="utf-8")
    h2_fixture = load_json(sram_lifecycle.H2_FIXTURE)
    facts = sram_lifecycle._owner_facts(fixture, h2_fixture)
    with pytest.raises(ValueError, match="CheckSram"):
        sram_lifecycle._require_source_shape(
            facts, source.replace("bclr    #1,(SAVE_FLAGS).l", "bset    #1,(SAVE_FLAGS).l", 1)
        )
    near_miss = source + "\n; bclr    #1,(SAVE_FLAGS).l\nNotClearSaveSlotFlag: rts\n"
    assert sram_lifecycle._require_source_shape(facts, near_miss) == b"Taguchi New Supra"


def test_lua_dispatch_coalesces_reused_function_pcs_into_one_event_per_pc() -> None:
    fixture = _fixture()
    static = _static(fixture)
    source = sram_lifecycle.OBSERVER.read_text(encoding="utf-8")
    function_entries = static["functionEntries"]
    function_pcs = [
        function_entries[sram_lifecycle.FUNCTION_FOR_OPERATION[case["operation"]]]
        for case in fixture["cases"]
    ]

    assert len(function_pcs) == 14
    assert function_pcs.count(function_entries["CheckSram"]) == 6
    assert len(set(function_pcs)) == 5
    requested = [function_entries["CheckSram"], *function_pcs]
    coalesced: dict[int, list[str]] = {}
    for pc in requested:
        coalesced.setdefault(pc, [])
        if "function-entry" not in coalesced[pc]:
            coalesced[pc].append("function-entry")
    assert coalesced[function_entries["CheckSram"]] == ["function-entry"]
    assert all(roles == ["function-entry"] for roles in coalesced.values())

    assert source.count("event.on_bus_exec(function()") == 1
    assert "if not callbacks[address] then" in source
    assert "callbacks[address]={}" in source
    assert "for _,entry in ipairs(callbacks[address]) do" in source
    assert "if entry.role==role then return end" in source
    assert "callbacks[address][#callbacks[address]+1]={role=role,index=index}" in source
    assert 'error("unknown deterministic dispatch role: "..entry.role)' in source
    assert source.index('status("milestone:observer-loaded")') < source.index(
        'register_exec(f.CheckSram,"function-entry",0)'
    )
    assert source.index('if entry.role=="case-entry"') < source.index(
        'elseif entry.role=="function-entry"'
    ) < source.index('elseif entry.role=="case-result"')


def test_expected_model_covers_all_fourteen_cases_and_full_spans() -> None:
    fixture = _fixture()
    static = _static(fixture)
    observed = sram_lifecycle.expected_observation(fixture, static)
    records = {record["id"]: record for record in observed["records"]}
    assert list(record["id"] for record in observed["records"]) == fixture["caseOrder"]
    assert len(records) == 14
    assert records["signature-mismatch-init"]["fullSramFact"] == {
        "logicalByteCount": 8192,
        "checksumByte": sum(b"Taguchi New Supra") & 0xFF,
        "mismatchCount": 0,
        "boundary": {"first": 0, "last": 0},
    }
    assert records["valid-slot1"]["resultD0"] == 1
    assert records["valid-slot2"]["resultD1"] == 1
    assert records["invalid-slot1-clears-bit0"]["resultD0"] == -1
    assert records["invalid-slot2-clears-bit1"]["resultD1"] == -1
    assert records["copy-save-1-to-2"]["slotFacts"][1]["span"] == records[
        "copy-save-1-to-2"
    ]["combatantFacts"]
    assert records["copy-save-2-to-1"]["slotFacts"][0]["span"] == records[
        "copy-save-2-to-1"
    ]["combatantFacts"]
    for record in observed["records"]:
        for slot in record["slotFacts"]:
            assert slot["span"]["logicalByteCount"] == 4016
            assert slot["span"]["mismatchCount"] == 0
            assert [item["logicalOffset"] for item in slot["span"]["sentinels"]] == [
                0,
                1,
                2007,
                4015,
            ]


def _failure_payload() -> dict[str, object]:
    return {
        "owner": "sram-lifecycle",
        "caseId": "copy-save-1-to-2",
        "phase": "function-entry",
        "role": "CopySave",
        "actualPc": 28634,
        "expectedEventPc": 28634,
        "expectedCallPc": 16738542,
        "expectedTargetPc": 28634,
        "expectedReturnPc": 16738548,
        "pendingCallback": {
            "active": True,
            "caseIndex": 11,
            "copyLoadSeen": False,
            "copySaveSeen": False,
            "expectedFunctionPc": 28634,
            "pendingReturnPc": 16738548,
            "rolesAtPc": ["function-entry"]
        },
        "error": "expected callback state"
    }


def _write_failure_status(
    path: Path, payload: dict[str, object], *preceding_lines: str
) -> None:
    path.write_text(
        "\n".join([*preceding_lines, sram_lifecycle.STATUS_PREFIX + json.dumps(payload)])
        + "\n",
        encoding="utf-8",
    )


def test_callback_failure_status_is_closed_and_promoted(tmp_path: Path) -> None:
    status = tmp_path / "sram-lifecycle.status.txt"
    payload = _failure_payload()
    assert payload["role"] == "CopySave"
    assert payload["pendingCallback"]["rolesAtPc"] == ["function-entry"]
    assert payload["pendingCallback"]["copyLoadSeen"] is False
    assert payload["pendingCallback"]["copySaveSeen"] is False
    _write_failure_status(status, payload, "milestone:direct-function-probe")
    diagnostic = sram_lifecycle._failure_diagnostic(status) or ""
    for expected in (
        "'caseId': 'copy-save-1-to-2'",
        "'phase': 'function-entry'",
        "'role': 'CopySave'",
        "'actualPc': 28634",
        "'expectedEventPc': 28634",
        "'expectedCallPc': 16738542",
        "'expectedTargetPc': 28634",
        "'expectedReturnPc': 16738548",
        "'expectedFunctionPc': 28634",
        "'pendingReturnPc': 16738548",
        "'rolesAtPc': ['function-entry']",
        "'error': 'expected callback state'",
    ):
        assert expected in diagnostic
    with pytest.raises(RuntimeError, match="sram-lifecycle observer callback failure"):
        sram_lifecycle._assert_status(status)

    _write_failure_status(status, payload)
    with pytest.raises(ValueError, match="lacks preceding milestone"):
        sram_lifecycle._failure_diagnostic(status)
    status.write_text(
        "\n".join(
            [
                "milestone:direct-function-probe",
                sram_lifecycle.STATUS_PREFIX + json.dumps(payload),
                "late-observer-row",
            ]
        )
        + "\n",
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="terminal exact failure line"):
        sram_lifecycle._failure_diagnostic(status)


@pytest.mark.parametrize("mutation", ("missing", "renamed", "extra", "wrong-type"))
def test_callback_failure_pending_state_rejects_exact_shape_drift(
    tmp_path: Path, mutation: str
) -> None:
    payload = _failure_payload()
    pending = payload["pendingCallback"]
    if mutation == "missing":
        del pending["expectedFunctionPc"]
    elif mutation == "renamed":
        pending["renamedFunctionPc"] = pending.pop("expectedFunctionPc")
    elif mutation == "extra":
        pending["extra"] = True
    elif mutation == "wrong-type":
        pending["caseIndex"] = "11"
    else:
        raise AssertionError(f"uncovered pending mutation: {mutation}")
    status = tmp_path / "sram-lifecycle.status.txt"
    _write_failure_status(status, payload, "milestone:direct-function-probe")
    with pytest.raises(ValueError, match="failed schema validation"):
        sram_lifecycle._failure_diagnostic(status)


def test_callback_failure_pending_roles_reject_duplicate_dispatch_role(tmp_path: Path) -> None:
    payload = _failure_payload()
    payload["pendingCallback"]["rolesAtPc"] = ["function-entry", "function-entry"]
    status = tmp_path / "sram-lifecycle.status.txt"
    _write_failure_status(status, payload, "milestone:direct-function-probe")
    with pytest.raises(ValueError, match="failed schema validation"):
        sram_lifecycle._failure_diagnostic(status)


@pytest.mark.parametrize("owner", ("wrong-owner", "random-services"))
def test_callback_failure_rejects_wrong_and_cross_owner(tmp_path: Path, owner: str) -> None:
    payload = _failure_payload()
    payload["owner"] = owner
    status = tmp_path / "sram-lifecycle.status.txt"
    _write_failure_status(status, payload, "milestone:direct-function-probe")
    with pytest.raises(ValueError, match="failed schema validation"):
        sram_lifecycle._failure_diagnostic(status)


def test_verifier_uses_one_launch_and_compares_complete_observation(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    fixture = _fixture()
    static = _static(fixture)
    observed = sram_lifecycle.expected_observation(fixture, static)
    launches: list[dict[str, object]] = []
    monkeypatch.setattr(sram_lifecycle, "verify_runtime_contract", lambda *_: None)
    monkeypatch.setattr(sram_lifecycle, "validate_static_contract", lambda *_: static)
    monkeypatch.setattr(sram_lifecycle, "_assert_status", lambda *_: None)
    monkeypatch.setattr(
        sram_lifecycle,
        "run_observer",
        lambda **kwargs: launches.append(kwargs) or observed,
    )
    result = sram_lifecycle.verify_sram_lifecycle(
        tmp_path / "input.bin", tmp_path, timeout_seconds=1
    )
    assert len(launches) == 1
    assert launches[0]["output_name"] == "sram-lifecycle"
    assert (
        launches[0]["config"]["observerFailureContract"]
        == sram_lifecycle.OBSERVER_FAILURE_CONTRACT
    )
    assert result["Cases"] == 14
    assert result["BizHawkLaunches"] == 1
