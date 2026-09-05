"""Contracts for the bounded Map 3 to Battle 01 player-ready H3 seam."""

from __future__ import annotations

import json
from copy import deepcopy
from functools import cache
from pathlib import Path
from typing import Any

import pytest

from sf2tool.h3 import map3_battle01_player_ready as rail
from sf2tool.jsonio import load_json, validate_json

ROOT = Path(__file__).resolve().parents[2]


def _rom() -> Path:
    return rail.repo_path("local/roms/sf2-us.bin")


def _upstream() -> Path:
    return rail.repo_path("local/upstream/SF2DISASM")


@cache
def _static() -> dict[str, Any]:
    return rail._static_contract(_rom(), _upstream())


def _failure_payload() -> dict[str, Any]:
    return {
        "owner": rail.OWNER,
        "caseId": rail.CASE_IDS[0],
        "phase": "extension-before",
        "role": "extension-phase-watchdog",
        "actualPc": 0xF00,
        "expectedPc": None,
        "callbackCount": 0,
        "callbacksCleared": True,
        "outputRemoved": True,
        "restoration": {
            "scopeArmed": True,
            "gameFlags": True,
            "combatantAllyRecords": True,
            "mapAndBattleState": True,
            "playerEntity": True,
            "forceAndParty": True,
            "followerState": True,
            "touchedEntities": True,
            "dialogueAndInput": True,
            "cameraState": True,
            "bootstrapFrame": True,
            "gold": True,
            "generatedRam": True,
            "sessionCartPatches": True,
            "sessionStateRestored": True,
            "callbacksCleared": True,
            "outputRemoved": True,
        },
        "restorationMismatch": None,
        "error": "bounded player-ready progress watchdog",
    }


def test_fixture_rebuilds_from_retained_contracts_and_pinned_sources() -> None:
    fixture = load_json(rail.FIXTURE)

    validate_json(fixture, rail.FIXTURE_SCHEMA, owner=rail.OWNER)
    assert fixture["static"] == _static()
    assert fixture["privateProvenance"] == {
        "romSha256": rail.CANONICAL_ROM_SHA256,
        "upstreamRepository": "https://github.com/ShiningForceCentral/SF2DISASM.git",
        "upstreamCommit": rail.UPSTREAM_COMMIT,
        "runtime": "BizHawk 2.11.1 / Genesis Plus GX",
        "bridgeBoundary": (
            "explicit non-natural R2a follower-ready to R2b terminal state; "
            "original route resumes after seed"
        ),
        "privateBoundary": "private-local-no-ROM-save-state-trace-or-capture-payload",
    }


def test_static_route_requires_real_map40_warp_event_terminal() -> None:
    static = _static()
    plan = static["inputPlan"]

    assert len(plan) == 46
    assert plan[0]["from"] == {"map": 21, "x": 5, "y": 15}
    assert plan[-1] == {
        "waypoint": "map40-entry-to-wildcard-battle-warp",
        "input": "Up",
        "from": {"map": 40, "x": 14, "y": 13},
        "to": {"map": 40, "x": 14, "y": 12},
    }
    assert static["warps"][1] == {
        "id": "map40-to-map57-wildcard-warp",
        "from": {"map": 40, "x": 14, "y": 12},
        "to": {"map": 57, "x": 8, "y": 18, "facing": static["ram"]["UP"]},
    }


def test_expected_observation_closes_admission_scenario_and_first_turn() -> None:
    record = load_json(rail.FIXTURE)["expectedObservation"]["records"][0]

    assert record["admission"] == {
        "map": 57,
        "battle": 1,
        "area": [0, 0, 16, 20],
        "flags": {"f401": True, "f451": True, "f501": False},
        "regionFlags90Through105": [False] * 16,
    }
    assert record["scenario"]["activeParty"] == [0, 1, 2]
    assert [row["id"] for row in record["scenario"]["combatants"]] == [
        0,
        1,
        2,
        128,
        129,
        130,
        131,
        132,
        133,
    ]
    assert record["turnState"] == {
        "currentActor": 1,
        "currentTurnOffset": 0,
        "entries": [
            {"actor": 1, "score": 6},
            {"actor": 2, "score": 6},
            {"actor": 128, "score": 5},
            {"actor": 131, "score": 5},
            {"actor": 133, "score": 5},
            {"actor": 129, "score": 4},
            {"actor": 130, "score": 4},
            {"actor": 132, "score": 4},
            {"actor": 0, "score": 3},
        ],
        "executedActorsBeforeReady": [1],
    }


def test_expected_observation_closes_semantic_player_input_readiness() -> None:
    observed = load_json(rail.FIXTURE)["expectedObservation"]
    record = observed["records"][0]

    assert record["continuity"]["kind"] == "controlled-harness-bridge"
    assert record["continuity"]["naturalR2bContinuity"] is False
    assert record["readiness"] == {
        "boundary": "ControlBattleEntity.after-WaitForVInt-before-input-read",
        "pc": 0x22E70,
        "beforeBattleScriptReturned": True,
        "battleStartScriptReturned": True,
        "turnOrderReturned": True,
        "currentPlayerInput": 0,
        "movingBattleEntity": 1,
        "viewTargetEntity": 1,
        "currentBattleAction": 0,
        "isTargeting": 0,
        "mapEventType": 0,
        "transferPending": False,
        "cutsceneOrMenuModal": False,
        "semanticInputMode": "battle-entity-movement",
    }
    assert record["chronology"][-6:] == [
        "original:PopulateTargetsListWithSpawningEnemies",
        "original:GenerateBattleTurnOrder",
        "original:ExecuteIndividualTurn:1",
        "original:ProcessBattleEntityControlPlayerInput:1",
        "original:ControlBattleEntity",
        "original:ControlBattleEntity:after-WaitForVInt-before-input-read",
    ]
    assert observed["callbacksCleared"] is True
    assert set(observed["restoration"].values()) == {True}


def test_observer_extension_is_opt_in_and_progress_is_original_command_driven() -> None:
    source = rail.OBSERVER.read_text(encoding="utf-8")

    assert "local extension_enabled = config.extension ~= nil" in source
    assert 'phase == "extension-before"' in source
    assert 'config.extension.functions.csc15_setEntityActscript' in source
    assert 'config.extension.functions.csc2A_entityShiver' in source
    assert 'config.extension.functions.csc2D_entityActionSequence' in source
    assert "extension_progress_frame = frame_count" in source
    assert "player-ready seam did not observe neutral current input" in source
    assert "playerReadyPc" in source


def test_observer_config_contains_no_golden_observation() -> None:
    fixture = load_json(rail.FIXTURE)
    config = rail._observer_config(fixture, _static())

    serialized = json.dumps(config, sort_keys=True)
    assert "expectedObservation" not in serialized
    assert config["extension"]["bridge"]["naturalR2bContinuity"] is False
    assert len(config["extension"]["inputPlan"]) == 46


def test_observation_fixture_and_failure_schemas_are_closed() -> None:
    fixture = load_json(rail.FIXTURE)
    observed = fixture["expectedObservation"]
    validate_json(observed, rail.OBSERVATION_SCHEMA, owner=rail.OWNER)

    invalid = deepcopy(observed)
    invalid["records"][0]["readiness"]["extra"] = True
    with pytest.raises(ValueError, match="Additional properties"):
        validate_json(invalid, rail.OBSERVATION_SCHEMA, owner=rail.OWNER)

    invalid = deepcopy(fixture)
    invalid["static"]["inputPlan"] = invalid["static"]["inputPlan"][:-1]
    with pytest.raises(ValueError, match="too short"):
        validate_json(invalid, rail.FIXTURE_SCHEMA, owner=rail.OWNER)

    payload = _failure_payload()
    validate_json(payload, rail.FAILURE_SCHEMA, owner=rail.OWNER)
    payload["callbackCount"] = -1
    with pytest.raises(ValueError, match="minimum"):
        validate_json(payload, rail.FAILURE_SCHEMA, owner=rail.OWNER)


def test_runtime_golden_drift_fails_after_schema_valid_observation(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    observed = deepcopy(load_json(rail.FIXTURE)["expectedObservation"])
    observed["records"][0]["deterministicState"]["ready"]["randomSeed"] ^= 1

    monkeypatch.setattr(rail, "run_observer", lambda **_: observed)
    monkeypatch.setattr(rail, "_assert_status", lambda: None)

    with pytest.raises(ValueError, match="runtime golden drift"):
        rail.verify_map3_battle01_player_ready(_rom(), _upstream())


def test_typed_callback_failure_is_read_from_status(
    tmp_path: Path, monkeypatch: pytest.MonkeyPatch
) -> None:
    payload = _failure_payload()
    status_path = tmp_path / "player-ready.status.txt"
    status_path.write_text(
        "failure:observer-callback:" + json.dumps(payload, sort_keys=True) + "\n",
        encoding="utf-8",
    )
    monkeypatch.setattr(rail, "STATUS_PATH", status_path)

    assert rail._failure_diagnostic() == payload


def test_research_index_delta_is_exact_and_deep() -> None:
    index = load_json(ROOT / "manifests/research-index.json")
    untouched = deepcopy(index)

    predecessor = rail._remove_map3_battle01_player_ready_later_owner_index_delta(index)

    assert rail._index_digest(predecessor) == rail._PREDECESSOR_INDEX_SHA256
    assert index == untouched


def test_research_index_delta_rejects_evidence_drift() -> None:
    index = load_json(ROOT / "manifests/research-index.json")
    record = next(row for row in index["records"] if row["id"] == rail._INDEX_RECORD_ID)
    record["evidence"][-1]["bindings"].reverse()

    with pytest.raises(ValueError, match="evidence drift"):
        rail._remove_map3_battle01_player_ready_later_owner_index_delta(index)
