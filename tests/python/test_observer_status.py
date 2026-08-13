from __future__ import annotations

import json
from copy import deepcopy
from pathlib import Path

import pytest

from sf2tool.h3.observer_status import (
    CALLBACK_FAILURE_PREFIX,
    SUCCESS_STATUS_TAIL,
    assert_observer_status,
    callback_failure_status,
    observer_failure_contract,
)
from sf2tool.jsonio import load_json, schema_composition_audit, validate_json
from sf2tool.paths import repo_path

OWNER = "map-script-control-audio"
FAILURE_SCHEMA = repo_path("schemas/h3/map-script-control-audio-callback-failure.schema.json")
CALLBACK_AUDIT_SCHEMA = repo_path("schemas/h3/observer-callback-audit.schema.json")
FAILURE_CONTRACT_SCHEMA = repo_path("schemas/h3/observer-failure-contract.schema.json")


def _payload() -> dict[str, object]:
    return {
        "owner": OWNER,
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


def _write_status(path: Path, *lines: str) -> None:
    path.write_text("\n".join(lines) + "\n", encoding="utf-8")


def test_shared_failure_contract_and_schema_close_owner_audit_shape() -> None:
    contract = observer_failure_contract(OWNER)
    assert contract == {
        "owner": OWNER,
        "exitCode": 1,
        "removeOutputBeforeExit": True,
        "statusPrefix": CALLBACK_FAILURE_PREFIX,
    }
    validate_json(contract, FAILURE_CONTRACT_SCHEMA, owner="observer failure contract")
    validate_json(_payload(), FAILURE_SCHEMA, owner="valid shared callback payload")

    wrong_owner = {**_payload(), "owner": "map-script-transition"}
    with pytest.raises(ValueError, match="failed schema validation"):
        validate_json(wrong_owner, FAILURE_SCHEMA, owner="cross-owner callback payload")

    extra_audit = deepcopy(_payload())
    extra_audit["pendingCallback"]["unexpected"] = True
    with pytest.raises(ValueError, match="pendingCallback"):
        validate_json(extra_audit, FAILURE_SCHEMA, owner="extra callback audit field")


def test_shared_callback_audit_schema_closes_physical_pc_role_map() -> None:
    audit = {
        "291854": {
            "roles": {
                "csc0a-call": {
                    "callSiteAddress": 291854,
                    "targetAddress": 291712,
                    "returnAddress": 291856,
                }
            }
        }
    }
    validate_json(audit, CALLBACK_AUDIT_SCHEMA, owner="valid callback audit")

    missing_return = deepcopy(audit)
    del missing_return["291854"]["roles"]["csc0a-call"]["returnAddress"]
    with pytest.raises(ValueError, match="returnAddress.*required property"):
        validate_json(
            missing_return,
            CALLBACK_AUDIT_SCHEMA,
            owner="incomplete callback audit",
        )

    wrong_pc_key = {"0x4740E": audit["291854"]}
    with pytest.raises(ValueError, match="does not match"):
        validate_json(wrong_pc_key, CALLBACK_AUDIT_SCHEMA, owner="non-decimal callback PC")


def test_h3_observer_schema_component_registry_is_closed_and_golden_free() -> None:
    schema_root = repo_path("schemas/h3")
    paths = [
        schema_root / "observer-callback-contract.schema.json",
        schema_root / "observer-callback-audit.schema.json",
        schema_root / "observer-failure-contract.schema.json",
        schema_root / "map-script-control-audio-callback-failure.schema.json",
        schema_root / "map-script-transition-callback-failure.schema.json",
        schema_root / "map-script-entity-presentation-fx-callback-failure.schema.json",
        schema_root / "random-services-callback-failure.schema.json",
        schema_root / "sram-lifecycle-callback-failure.schema.json",
        schema_root / "blacksmith-mithril-callback-failure.schema.json",
        schema_root / "controller-input-callback-failure.schema.json",
        schema_root / "story-state-callback-failure.schema.json",
    ]
    audit = schema_composition_audit(paths)
    assert audit["schemaCount"] == 11
    assert audit["unresolvedReferences"] == []
    assert audit["duplicateBodyGroups"] == []
    assert audit["largeConstCount"] == 0

    component = load_json(paths[0])
    assert set(component["definitions"]) == {
        "pc",
        "nullablePc",
        "nonEmptyString",
        "nullableNonEmptyString",
        "stringArray",
        "observerFailureContract",
        "callbackExpectation",
        "callbackAudit",
        "controlAudioPendingCallback",
        "transitionPendingService",
        "transitionPendingCallback",
        "entityPresentationPendingCallback",
        "randomServicesPendingCallback",
        "sramLifecyclePendingCallback",
        "storyStateRole",
        "storyStatePendingCallback",
        "controllerInputPendingCallback",
        "blacksmithMithrilPendingRngCall",
        "blacksmithMithrilTransactionState",
        "blacksmithMithrilFulfillmentState",
        "blacksmithMithrilPrecommitPendingService",
        "blacksmithMithrilPrecommitState",
        "blacksmithMithrilPromptRoutingState",
        "blacksmithMithrilEquipDecisionPendingService",
        "blacksmithMithrilEquipDecisionState",
        "blacksmithMithrilPendingCallback",
        "controlAudioFailure",
        "transitionFailure",
        "entityPresentationFailure",
        "randomServicesFailure",
        "sramLifecycleFailure",
        "storyStateFailure",
        "controllerInputFailure",
        "blacksmithMithrilFailure",
    }
    serialized = json.dumps(component, sort_keys=True)
    for golden_field in ("cases", "caseOrder", "records", "recordOrder"):
        assert f'"{golden_field}"' not in serialized


def test_story_state_failure_contract_closes_pending_state_and_cleanup() -> None:
    schema = repo_path("schemas/h3/story-state-callback-failure.schema.json")
    payload = {
        "owner": "story-state",
        "caseId": "csc10-set-slot1-save-load-branch",
        "phase": "save-entry",
        "role": "save-entry",
        "actualPc": 28522,
        "expectedCallPc": 16736270,
        "expectedEventPc": 28522,
        "expectedTargetPc": 28522,
        "expectedReturnPc": 16736276,
        "pendingCallback": {
            "active": True,
            "caseIndex": 10,
            "caseKind": "persistence",
            "expectedCallPc": 16736270,
            "expectedEventPc": 28522,
            "expectedTargetPc": 28522,
            "expectedReturnPc": 16736276,
            "rolesAtPc": ["save-entry"],
        },
        "callbacksRemaining": 0,
        "mutationState": {
            "logicalRamMutated": True,
            "sramMutated": True,
            "scratchMutated": True,
        },
        "outputRemoved": True,
        "sessionStateRestored": True,
        "restorationMismatch": None,
        "error": "forced story-state callback failure",
    }
    validate_json(payload, schema, owner="story-state callback payload")
    wrong_role = deepcopy(payload)
    wrong_role["pendingCallback"]["rolesAtPc"] = ["unknown"]
    with pytest.raises(ValueError, match="unknown"):
        validate_json(wrong_role, schema, owner="story-state closed role")
    missing_cleanup = deepcopy(payload)
    missing_cleanup["outputRemoved"] = False
    with pytest.raises(ValueError, match="True was expected"):
        validate_json(missing_cleanup, schema, owner="story-state output cleanup")
    extra_pending = deepcopy(payload)
    extra_pending["pendingCallback"]["extra"] = True
    with pytest.raises(ValueError, match="pendingCallback"):
        validate_json(extra_pending, schema, owner="story-state pending closure")

    pre_probe = deepcopy(payload)
    pre_probe.update(
        {
            "phase": "wrapper-transition",
            "role": "wrapper-bypass",
            "actualPc": 292116,
            "expectedCallPc": 292114,
            "expectedEventPc": 292114,
            "expectedTargetPc": 65416,
            "expectedReturnPc": 292120,
        }
    )
    pre_probe["pendingCallback"].update(
        {
            "expectedCallPc": 292114,
            "expectedEventPc": 292114,
            "expectedTargetPc": 65416,
            "expectedReturnPc": 292120,
            "rolesAtPc": ["wrapper-bypass"],
        }
    )
    pre_probe["mutationState"] = {
        "logicalRamMutated": False,
        "sramMutated": False,
        "scratchMutated": True,
    }
    validate_json(pre_probe, schema, owner="story-state pre-probe cleanup failure")
    assert pre_probe["callbacksRemaining"] == 0
    assert pre_probe["outputRemoved"] is True
    assert pre_probe["sessionStateRestored"] is True

    later_pre_probe = deepcopy(pre_probe)
    later_pre_probe["caseId"] = "csc11-flag89-set-slot1-save-load-branch"
    later_pre_probe["pendingCallback"]["caseIndex"] = 12
    later_pre_probe["pendingCallback"]["caseKind"] = "persistence"
    validate_json(
        later_pre_probe,
        schema,
        owner="story-state later pre-probe session-cleanup failure",
    )
    assert later_pre_probe["mutationState"] == {
        "logicalRamMutated": False,
        "sramMutated": False,
        "scratchMutated": True,
    }
    assert later_pre_probe["callbacksRemaining"] == 0
    assert later_pre_probe["outputRemoved"] is True
    assert later_pre_probe["sessionStateRestored"] is True

    wrong_outer_return = deepcopy(later_pre_probe)
    wrong_outer_return["expectedReturnPc"] = 292116
    wrong_outer_return["pendingCallback"]["expectedReturnPc"] = 292116
    validate_json(
        wrong_outer_return,
        schema,
        owner="story-state structurally valid wrong outer return",
    )

    hybrid_outer_return = deepcopy(later_pre_probe)
    hybrid_outer_return["expectedReturnPc"] = 65432
    hybrid_outer_return["pendingCallback"]["expectedReturnPc"] = 65432
    validate_json(
        hybrid_outer_return,
        schema,
        owner="story-state structurally valid hybrid outer/inner return",
    )

    later_post_probe = deepcopy(later_pre_probe)
    later_post_probe["phase"] = "save-entry"
    later_post_probe["role"] = "save-entry"
    later_post_probe["actualPc"] = 28522
    later_post_probe["expectedCallPc"] = 16730126
    later_post_probe["expectedEventPc"] = 28522
    later_post_probe["expectedTargetPc"] = 28522
    later_post_probe["expectedReturnPc"] = 16730132
    later_post_probe["pendingCallback"].update(
        {
            "expectedCallPc": 16730126,
            "expectedEventPc": 28522,
            "expectedTargetPc": 28522,
            "expectedReturnPc": 16730132,
            "rolesAtPc": ["save-entry"],
        }
    )
    later_post_probe["mutationState"] = {
        "logicalRamMutated": True,
        "sramMutated": True,
        "scratchMutated": True,
    }
    validate_json(
        later_post_probe,
        schema,
        owner="story-state later post-probe session-cleanup failure",
    )

    inner_transition = deepcopy(later_pre_probe)
    inner_transition.update(
        {
            "role": "trampoline-jsr",
            "actualPc": 65430,
            "expectedCallPc": 65430,
            "expectedEventPc": 16730112,
            "expectedTargetPc": 16730112,
            "expectedReturnPc": 65432,
        }
    )
    inner_transition["pendingCallback"].update(
        {
            "expectedCallPc": 65430,
            "expectedEventPc": 16730112,
            "expectedTargetPc": 16730112,
            "expectedReturnPc": 65432,
            "rolesAtPc": ["trampoline-jsr"],
        }
    )
    validate_json(inner_transition, schema, owner="story-state inner transition failure")
    for field, wrong in (
        ("expectedCallPc", 292114),
        ("expectedTargetPc", 65416),
        ("expectedReturnPc", 292120),
    ):
        wrong_inner = deepcopy(inner_transition)
        wrong_inner[field] = wrong
        wrong_inner["pendingCallback"][field] = wrong
        validate_json(
            wrong_inner,
            schema,
            owner=f"story-state structurally valid wrong inner {field}",
        )

    restoration_failure = deepcopy(payload)
    restoration_failure["sessionStateRestored"] = False
    restoration_failure["restorationMismatch"] = {
        "domain": "sram",
        "address": 2105399,
        "expected": 17,
        "actual": 18,
    }
    validate_json(restoration_failure, schema, owner="story-state restoration diagnostic")

    retained_stream_failure = deepcopy(payload)
    retained_stream_failure["sessionStateRestored"] = False
    retained_stream_failure["restorationMismatch"] = {
        "domain": "retainedV1Stream",
        "address": 16728068,
        "expected": 17,
        "actual": 18,
    }
    validate_json(
        retained_stream_failure,
        schema,
        owner="story-state retained-v1 stream restoration diagnostic",
    )

    stack_failure = deepcopy(payload)
    stack_failure["restorationMismatch"] = {
        "domain": "callStack",
        "address": 16776960,
        "expected": 16776960,
        "actual": 16776956,
    }
    validate_json(stack_failure, schema, owner="story-state stack-balance diagnostic")


def test_shared_failure_parser_accepts_append_log_and_rejects_ambiguous_rows(
    tmp_path: Path,
) -> None:
    status = tmp_path / "observer.status.txt"
    failure = CALLBACK_FAILURE_PREFIX + json.dumps(_payload(), sort_keys=True)
    _write_status(status, "milestone:observer-loaded", failure)
    assert (
        callback_failure_status(
            status,
            owner=OWNER,
            schema_path=FAILURE_SCHEMA,
        )
        == _payload()
    )

    _write_status(status, failure, failure)
    with pytest.raises(ValueError, match="multiplicity"):
        callback_failure_status(status, owner=OWNER, schema_path=FAILURE_SCHEMA)

    _write_status(status, "malformed " + failure)
    with pytest.raises(ValueError, match="status line drift"):
        callback_failure_status(status, owner=OWNER, schema_path=FAILURE_SCHEMA)


def test_shared_terminal_status_requires_owner_milestones_and_exact_tail(
    tmp_path: Path,
) -> None:
    status = tmp_path / "observer.status.txt"
    required = "milestone:owner-probe"
    _write_status(status, required, *SUCCESS_STATUS_TAIL)
    assert_observer_status(
        status,
        owner=OWNER,
        schema_path=FAILURE_SCHEMA,
        required_milestones=(required,),
    )

    _write_status(status, *SUCCESS_STATUS_TAIL)
    with pytest.raises(RuntimeError, match="required milestone drift"):
        assert_observer_status(
            status,
            owner=OWNER,
            schema_path=FAILURE_SCHEMA,
            required_milestones=(required,),
        )

    _write_status(status, required, SUCCESS_STATUS_TAIL[1], SUCCESS_STATUS_TAIL[0])
    with pytest.raises(RuntimeError, match="terminal status drift"):
        assert_observer_status(status, owner=OWNER, schema_path=FAILURE_SCHEMA)
