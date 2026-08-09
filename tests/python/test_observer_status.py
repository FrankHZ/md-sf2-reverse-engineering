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
FAILURE_SCHEMA = repo_path(
    "schemas/h3/map-script-control-audio-callback-failure.schema.json"
)
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
    ]
    audit = schema_composition_audit(paths)
    assert audit["schemaCount"] == 10
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
        "controllerInputPendingCallback",
        "blacksmithMithrilPendingRngCall",
        "blacksmithMithrilTransactionState",
        "blacksmithMithrilFulfillmentState",
        "blacksmithMithrilPendingCallback",
        "controlAudioFailure",
        "transitionFailure",
        "entityPresentationFailure",
        "randomServicesFailure",
        "sramLifecycleFailure",
        "controllerInputFailure",
        "blacksmithMithrilFailure",
    }
    serialized = json.dumps(component, sort_keys=True)
    for golden_field in ("cases", "caseOrder", "records", "recordOrder"):
        assert f'"{golden_field}"' not in serialized


def test_shared_failure_parser_accepts_append_log_and_rejects_ambiguous_rows(
    tmp_path: Path,
) -> None:
    status = tmp_path / "observer.status.txt"
    failure = CALLBACK_FAILURE_PREFIX + json.dumps(_payload(), sort_keys=True)
    _write_status(status, "milestone:observer-loaded", failure)
    assert callback_failure_status(
        status,
        owner=OWNER,
        schema_path=FAILURE_SCHEMA,
    ) == _payload()

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
