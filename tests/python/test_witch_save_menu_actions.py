from __future__ import annotations

import json
from copy import deepcopy
from pathlib import Path

import pytest

from sf2tool.h3 import witch_save_menu_actions
from sf2tool.h3.bizhawk import bizhawk_contract, validate_lua_syntax
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

UPSTREAM = repo_path("local/upstream/SF2DISASM")
ROM = repo_path("local/roms/sf2-us.bin")


def _fixture() -> dict[str, object]:
    return load_json(witch_save_menu_actions.FIXTURE)


def _static(**changes: str) -> dict[str, object]:
    witch = (UPSTREAM / "disasm" / witch_save_menu_actions.WITCH_SOURCE).read_text(encoding="utf-8")
    for before, after in changes.items():
        witch = witch.replace(before, after)
    return witch_save_menu_actions.build_static_contract(ROM, UPSTREAM, witch_source=witch)


def _failure_payload() -> dict[str, object]:
    return {
        "owner": "witch-save-menu-actions",
        "caseId": "delete-slot1-confirm",
        "phase": "service-return",
        "role": "service-return",
        "actualPc": 30140,
        "expectedEventPc": 30140,
        "expectedCallPc": 30136,
        "expectedTargetPc": 28652,
        "expectedReturnPc": 30140,
        "pendingCallback": {
            "active": True,
            "caseIndex": 9,
            "caseId": "delete-slot1-confirm",
            "expectedEventPc": 30140,
            "expectedCallPc": 30136,
            "expectedTargetPc": 28652,
            "expectedReturnPc": 30140,
            "pendingKind": "service",
            "serviceEntrySeen": True,
            "serviceReturnSeen": False,
            "rolesAtPc": ["service-return"],
        },
        "callbacksRemaining": 0,
        "restoration": {
            "currentSaveSlotRestored": True,
            "gameFlag88Restored": True,
            "saveFlagsRestored": True,
            "slotDataRestored": True,
            "generatedBytesRestored": True,
            "stackRestored": True,
            "frameRestored": True,
            "cartPatchesRestored": True,
        },
        "cleanup": {"outputRemoved": True, "callbacksCleared": True},
        "error": "forced callback failure",
    }


def _write_failure_status(path: Path, payload: dict[str, object], *before: str) -> None:
    path.write_text(
        "\n".join([*before, witch_save_menu_actions.STATUS_PREFIX + json.dumps(payload)]) + "\n",
        encoding="utf-8",
    )


def _write_success_status(path: Path) -> None:
    path.write_text(
        "\n".join(
            (
                "milestone:observer-loaded",
                "milestone:action-probe-armed",
                "milestone:action-cases-entered",
                "milestone:callbacks-cleared:0",
                "milestone:observer-finished",
            )
        )
        + "\n",
        encoding="utf-8",
    )


def test_fixture_is_exact_ten_case_matrix_and_static_derivation() -> None:
    fixture = _fixture()
    static = _static()
    validate_json(fixture, witch_save_menu_actions.FIXTURE_SCHEMA, owner="witch menu fixture")
    assert [case["id"] for case in fixture["cases"]] == list(fixture["caseOrder"])
    assert (
        tuple(
            (
                case["id"],
                case["action"],
                case["saveFlags"],
                case["menuResult"],
                case["promptResult"],
                case["flag88Set"],
            )
            for case in fixture["cases"]
        )
        == witch_save_menu_actions.CASE_MATRIX
    )
    assert fixture["instrumentation"]["observedActionEntries"] == {
        action: static["function"][f"{action}ActionAddress"]
        for action in ("load", "copy", "delete")
    }
    assert static["harness"]["bootstrapToFirstCaseFrameBudget"] == 600
    assert static["harness"]["caseFrameBudget"] == 600
    assert (
        static["function"]["copySaveNestedLoadCallAddress"]
        < static["function"]["copySaveAddress"] + 32
    )
    assert (
        witch_save_menu_actions.expected_observation(fixture, static)
        == fixture["expectedObservation"]
    )


@pytest.mark.parametrize(
    ("before", "after", "error"),
    (
        ("andi.w  #3,d2", "andi.w  #2,d2", "witchMenuAction_Load"),
        ("lsl.w   #1,d2", "lsl.w   #2,d2", "witchMenuAction_Load"),
        ("moveq   #2,d1", "moveq   #1,d1", "witchMenuAction_Load"),
        ("bmi.w   byte_73C2", "bpl.w   byte_73C2", "witchMenuAction_Load"),
        ("subq.w  #1,d0", "subq.w  #2,d0", "witchMenuAction_Load"),
        ("bne.w   byte_73C2", "beq.w   byte_73C2", "witchMenuAction_Copy"),
    ),
)
def test_source_guard_rejects_action_operand_polarity_and_scale_mutations(
    before: str, after: str, error: str
) -> None:
    with pytest.raises(ValueError, match=error):
        _static(**{before: after})


def test_source_parser_ignores_comments_labels_and_near_miss_calls() -> None:
    source = (UPSTREAM / "disasm" / witch_save_menu_actions.WITCH_SOURCE).read_text(
        encoding="utf-8"
    )
    source = source.replace(
        "jsr     j_alt_YesNoPrompt",
        "fakejsr j_alt_YesNoPrompt\n"
        "j_alt_YesNoPromptMention:\n"
        " move.l #j_alt_YesNoPrompt,d7\n"
        " ; jsr j_alt_YesNoPrompt\n"
        " jsr     j_alt_YesNoPrompt ; exact executable call",
        1,
    )
    static = witch_save_menu_actions.build_static_contract(ROM, UPSTREAM, witch_source=source)
    assert static["sourceUseSites"]["copy"][0]["opcode"] == "jsr"


@pytest.mark.parametrize("mutation", ("missing", "extra", "reordered", "renamed-observation"))
def test_fixture_and_observation_schemas_close_nested_shape_and_order(mutation: str) -> None:
    fixture = deepcopy(_fixture())
    if mutation == "missing":
        del fixture["instrumentation"]["observedActionEntries"]["copy"]
    elif mutation == "extra":
        fixture["expectedObservation"]["records"][0]["menu"]["unexpected"] = True
    elif mutation == "reordered":
        fixture["caseOrder"].reverse()
    else:
        record = fixture["expectedObservation"]["records"][1]
        record["service"]["entry"] = record["service"].pop("entryAddress")
    if mutation == "reordered":
        with pytest.raises(ValueError, match="exact ten-case matrix/order drift"):
            witch_save_menu_actions.expected_observation(fixture, _static())
    else:
        with pytest.raises(ValueError, match="failed schema validation"):
            validate_json(fixture, witch_save_menu_actions.FIXTURE_SCHEMA, owner=mutation)


def test_cancel_and_confirm_expected_records_close_service_admission() -> None:
    records = {row["id"]: row for row in _fixture()["expectedObservation"]["records"]}
    for case_id in (
        "load-menu-cancel",
        "copy-prompt-cancel",
        "delete-menu-cancel",
        "delete-slot1-prompt-cancel",
    ):
        assert records[case_id]["service"] is None
        roles = [row["role"] for row in records[case_id]["callbackChronology"]]
        assert "service-entry" not in roles
    for case_id in (
        "load-slot1-savepoint-route",
        "load-slot2-battle-route",
        "copy-slot1-to-slot2",
        "copy-slot2-to-slot1",
        "delete-slot1-confirm",
        "delete-slot2-confirm",
    ):
        roles = [row["role"] for row in records[case_id]["callbackChronology"]]
        assert (
            roles.index("service-call")
            < roles.index("service-entry")
            < roles.index("service-return")
        )


def test_active_unexpected_service_entries_fail_closed_for_cancel_and_wrong_entry(
    tmp_path: Path,
) -> None:
    static = _static()
    source = witch_save_menu_actions.OBSERVER.read_text(encoding="utf-8")
    handler = source[
        source.index("local function service_entry_callback") : source.index(
            "local function service_return"
        )
    ]
    assert "if not active then return end" in handler
    assert "if not active or active.pendingKind" not in handler
    assert 'set_expectation("service-entry","service-entry",target' in handler
    assert 'expect(active.pendingKind=="service" and active.service~=nil' in handler
    assert "unexpected original service entry" in handler
    assert "unregister_exec(f.loadGameAddress)" in handler
    assert "f.copySaveNestedLoadCallAddress~=nil" in handler
    service_return = source[
        source.index("local function service_return") : source.index("local function load_handoff")
    ]
    assert 'register_exec(f.loadGameAddress,"service-entry",0)' in service_return

    cancel = _failure_payload()
    cancel.update(
        {
            "caseId": "load-menu-cancel",
            "phase": "service-entry",
            "role": "service-entry",
            "actualPc": static["function"]["loadGameAddress"],
            "expectedEventPc": static["function"]["loadGameAddress"],
            "expectedCallPc": static["calls"]["service"]["load"]["callSiteAddress"],
            "expectedTargetPc": static["function"]["loadGameAddress"],
            "expectedReturnPc": static["calls"]["service"]["load"]["returnAddress"],
            "error": "unexpected original service entry",
        }
    )
    cancel["pendingCallback"].update(
        {
            "caseIndex": 1,
            "caseId": "load-menu-cancel",
            "expectedEventPc": static["function"]["loadGameAddress"],
            "expectedCallPc": static["calls"]["service"]["load"]["callSiteAddress"],
            "expectedTargetPc": static["function"]["loadGameAddress"],
            "expectedReturnPc": static["calls"]["service"]["load"]["returnAddress"],
            "pendingKind": "none",
            "rolesAtPc": ["service-entry"],
        }
    )
    validate_json(cancel, witch_save_menu_actions.FAILURE_SCHEMA, owner="cancel service entry")

    wrong = deepcopy(cancel)
    wrong.update(
        {
            "caseId": "load-slot1-savepoint-route",
            "actualPc": static["function"]["copySaveAddress"],
            "error": "unexpected original service entry",
        }
    )
    wrong["pendingCallback"].update(
        {
            "caseIndex": 2,
            "caseId": "load-slot1-savepoint-route",
            "pendingKind": "service",
        }
    )
    validate_json(wrong, witch_save_menu_actions.FAILURE_SCHEMA, owner="wrong service entry")

    for name, payload in (("cancel", cancel), ("wrong", wrong)):
        status = tmp_path / f"{name}.status.txt"
        _write_failure_status(
            status,
            payload,
            "milestone:action-probe-armed",
            "milestone:action-cases-entered",
        )
        assert witch_save_menu_actions._failure_diagnostic(status) == payload
        with pytest.raises(RuntimeError, match="observer callback failure"):
            witch_save_menu_actions._assert_status(status)


def test_watchdogs_are_bounded_structured_and_output_suppressing(tmp_path: Path) -> None:
    static = _static()
    source = witch_save_menu_actions.OBSERVER.read_text(encoding="utf-8")
    assert 'frame_budget(h.bootstrapToFirstCaseFrameBudget,"bootstrap-to-first-case")' in source
    assert 'frame_budget(h.caseFrameBudget,"case")' in source
    assert "local function enforce_watchdogs()" in source
    assert (
        'set_expectation("bootstrap-to-first-case-watchdog",'
        '"bootstrap-to-first-case-watchdog-timeout"'
    ) in source
    assert 'set_expectation("case-watchdog","case-watchdog-timeout"' in source
    assert (
        'fail_callback("bootstrap-to-first-case frame budget exhausted before first '
        'generated case entry")'
    ) in source
    assert 'fail_callback("action case frame budget exhausted before terminal")' in source
    assert "emu.frameadvance();enforce_watchdogs()" in source
    failure = source[
        source.index("local function fail_callback") : source.index("local function expect")
    ]
    assert failure.index("os.remove(config.outputPath)") < failure.index("status(diagnostic)")
    assert "cleanup_events()" in failure
    assert "client.exitCode(config.observerFailureContract.exitCode)" in failure

    payload = _failure_payload()
    payload.update(
        {
            "caseId": "load-slot1-savepoint-route",
            "phase": "case-watchdog",
            "role": "case-watchdog-timeout",
            "actualPc": static["function"]["loadActionAddress"],
            "expectedEventPc": static["harness"]["baseAddress"]
            + static["harness"]["caseStride"]
            + static["harness"]["caseResultOffset"],
            "expectedCallPc": None,
            "expectedTargetPc": static["harness"]["terminalStubAddress"],
            "expectedReturnPc": static["harness"]["baseAddress"]
            + static["harness"]["caseStride"]
            + static["harness"]["caseResultOffset"],
            "error": "action case frame budget exhausted before terminal",
        }
    )
    payload["pendingCallback"].update(
        {
            "caseIndex": 2,
            "caseId": "load-slot1-savepoint-route",
            "expectedEventPc": payload["expectedEventPc"],
            "expectedCallPc": None,
            "expectedTargetPc": payload["expectedTargetPc"],
            "expectedReturnPc": payload["expectedReturnPc"],
            "pendingKind": "service",
            "serviceEntrySeen": True,
            "rolesAtPc": [],
        }
    )
    validate_json(payload, witch_save_menu_actions.FAILURE_SCHEMA, owner="case watchdog failure")
    bootstrap = deepcopy(payload)
    bootstrap.update(
        {
            "caseId": "load-menu-cancel",
            "phase": "bootstrap-to-first-case-watchdog",
            "role": "bootstrap-to-first-case-watchdog-timeout",
            "actualPc": static["function"]["checkSramAddress"],
            "expectedEventPc": static["harness"]["baseAddress"],
            "expectedTargetPc": static["harness"]["baseAddress"],
            "expectedReturnPc": None,
            "error": (
                "bootstrap-to-first-case frame budget exhausted before first generated case entry"
            ),
        }
    )
    bootstrap["pendingCallback"].update(
        {
            "active": False,
            "caseIndex": 0,
            "caseId": "load-menu-cancel",
            "expectedEventPc": bootstrap["expectedEventPc"],
            "expectedTargetPc": bootstrap["expectedTargetPc"],
            "expectedReturnPc": None,
            "pendingKind": "none",
            "serviceEntrySeen": False,
            "serviceReturnSeen": False,
        }
    )
    validate_json(
        bootstrap, witch_save_menu_actions.FAILURE_SCHEMA, owner="bootstrap watchdog failure"
    )
    status = tmp_path / "watchdog.status.txt"
    _write_failure_status(
        status,
        payload,
        "milestone:action-probe-armed",
        "milestone:action-cases-entered",
    )
    assert witch_save_menu_actions._failure_diagnostic(status) == payload
    with pytest.raises(RuntimeError, match="observer callback failure"):
        witch_save_menu_actions._assert_status(status)


def test_restore_state_writes_captured_frame_and_stack_before_readback() -> None:
    source = witch_save_menu_actions.OBSERVER.read_text(encoding="utf-8")
    restore = source[
        source.index("local function restore_state") : source.index("local function fail_callback")
    ]
    stack_write = 'emu.setregister("M68K A7",saved.a7)'
    frame_write = 'emu.setregister("M68K A6",saved.a6)'
    assert stack_write in restore and frame_write in restore
    assert restore.index(stack_write) < restore.index("restoration.stackRestored")
    assert restore.index(frame_write) < restore.index("restoration.frameRestored")


def test_callback_failure_status_is_terminal_closed_and_nonzero_contract(tmp_path: Path) -> None:
    status = tmp_path / "witch-save-menu-actions.status.txt"
    payload = _failure_payload()
    validate_json(payload, witch_save_menu_actions.FAILURE_SCHEMA, owner="callback failure")
    _write_failure_status(status, payload, "milestone:action-probe-armed")
    diagnostic = witch_save_menu_actions._failure_diagnostic(status)
    assert diagnostic is not None
    with pytest.raises(RuntimeError, match="witch-save-menu-actions observer callback failure"):
        witch_save_menu_actions._assert_status(status)

    bad = deepcopy(payload)
    bad["callbacksRemaining"] = 1
    _write_failure_status(status, bad, "milestone:action-probe-armed")
    with pytest.raises(ValueError, match="failed schema validation"):
        witch_save_menu_actions._failure_diagnostic(status)


def test_success_status_requires_singleton_ordered_action_case_milestones(tmp_path: Path) -> None:
    status = tmp_path / "witch-save-menu-actions.status.txt"
    _write_success_status(status)
    witch_save_menu_actions._assert_status(status)

    missing = status.read_text(encoding="utf-8").replace("milestone:action-cases-entered\n", "")
    status.write_text(missing, encoding="utf-8")
    with pytest.raises(RuntimeError, match="required milestone drift"):
        witch_save_menu_actions._assert_status(status)

    duplicated = [
        "milestone:observer-loaded",
        "milestone:action-probe-armed",
        "milestone:action-cases-entered",
        "milestone:action-cases-entered",
        "milestone:callbacks-cleared:0",
        "milestone:observer-finished",
    ]
    status.write_text("\n".join(duplicated) + "\n", encoding="utf-8")
    with pytest.raises(RuntimeError, match="multiplicity drift"):
        witch_save_menu_actions._assert_status(status)

    swapped = [
        "milestone:observer-loaded",
        "milestone:action-cases-entered",
        "milestone:action-probe-armed",
        "milestone:callbacks-cleared:0",
        "milestone:observer-finished",
    ]
    status.write_text("\n".join(swapped) + "\n", encoding="utf-8")
    with pytest.raises(RuntimeError, match="order drift"):
        witch_save_menu_actions._assert_status(status)


@pytest.mark.parametrize("rejection", ("status", "observation"))
def test_rejected_post_launch_candidate_observation_is_unlinked(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path, rejection: str
) -> None:
    fixture = _fixture()
    static = _static()
    expected = witch_save_menu_actions.expected_observation(fixture, static)
    observed_path = tmp_path / "witch-save-menu-actions.observed.json"
    status_path = tmp_path / "witch-save-menu-actions.status.txt"
    observed_path.write_text(json.dumps(expected), encoding="utf-8")
    if rejection == "status":
        _write_failure_status(
            status_path,
            _failure_payload(),
            "milestone:action-probe-armed",
            "milestone:action-cases-entered",
        )
        observed = expected
        error = "observer callback failure"
    else:
        _write_success_status(status_path)
        observed = {"system": "GEN"}
        error = "failed schema validation"

    monkeypatch.setattr(witch_save_menu_actions, "DERIVED_ROOT", tmp_path)
    monkeypatch.setattr(witch_save_menu_actions, "verify_runtime_contract", lambda *_: None)
    monkeypatch.setattr(witch_save_menu_actions, "build_static_contract", lambda *_: static)
    monkeypatch.setattr(witch_save_menu_actions, "run_observer", lambda **_: observed)

    with pytest.raises((RuntimeError, ValueError), match=error):
        witch_save_menu_actions.verify_witch_save_menu_actions(
            tmp_path / "input.bin", tmp_path, timeout_seconds=1
        )
    assert not observed_path.exists()


def test_verifier_uses_one_launch_without_golden_or_result_corpus_in_lua_config(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    fixture = _fixture()
    static = _static()
    expected = witch_save_menu_actions.expected_observation(fixture, static)
    launches: list[dict[str, object]] = []
    monkeypatch.setattr(witch_save_menu_actions, "verify_runtime_contract", lambda *_: None)
    monkeypatch.setattr(witch_save_menu_actions, "build_static_contract", lambda *_: static)
    monkeypatch.setattr(witch_save_menu_actions, "_assert_status", lambda *_: None)
    monkeypatch.setattr(
        witch_save_menu_actions,
        "run_observer",
        lambda **kwargs: launches.append(kwargs) or expected,
    )

    result = witch_save_menu_actions.verify_witch_save_menu_actions(
        tmp_path / "input.bin", tmp_path, timeout_seconds=1
    )
    assert result["Status"] == "PASS"
    assert len(launches) == 1
    config = launches[0]["config"]
    assert config["cases"] == fixture["cases"]
    assert config["caseOrder"] == fixture["caseOrder"]

    def keys(value: object) -> set[str]:
        if isinstance(value, dict):
            return set(value) | set().union(*(keys(item) for item in value.values()))
        if isinstance(value, list):
            return set().union(*(keys(item) for item in value)) if value else set()
        return set()

    assert not {
        "acceptedObservation",
        "expectedObservation",
        "golden",
        "result",
        "results",
        "records",
    } & keys(config)


def test_observer_lua_syntax_and_deterministic_dispatch_preflight() -> None:
    _, executable = bizhawk_contract()
    validate_lua_syntax(witch_save_menu_actions.OBSERVER, executable)
    source = witch_save_menu_actions.OBSERVER.read_text(encoding="utf-8")
    assert "pcall(function() current_pc=address" in source
    assert "callbacks[address]" in source
    assert 'callbacksRemaining":0' in source
    assert "cleanup_events();expect(#event_ids==0" in source
    assert 'status("milestone:action-cases-entered")' in source
    assert "bootstrap_to_first_case_budget" in source
    assert "case_frame_budget" in source
    assert "client.exitCode(config.observerFailureContract.exitCode)" in source
    assert "client.exitCode(0)" in source
