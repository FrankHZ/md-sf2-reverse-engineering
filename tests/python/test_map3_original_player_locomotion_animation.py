"""Static and adversarial tests for the Map 3 locomotion/animation H3 rail."""

from __future__ import annotations

import json
import re
from collections.abc import Callable
from copy import deepcopy
from pathlib import Path
from typing import Any

import pytest

from sf2tool.h3 import map3_original_player_locomotion_animation as rail
from sf2tool.jsonio import load_json, validate_json


def _fixture() -> dict[str, Any]:
    return load_json(rail.FIXTURE)


def _failure_payload() -> dict[str, Any]:
    return {
        "owner": rail.OWNER,
        "caseId": "attempt-left",
        "phase": "await-sprite-counter-after",
        "role": "sprite-counter-after",
        "actualPc": 0x4D5C,
        "expectedPc": 0x4D5C,
        "callbackCount": 0,
        "callbacksCleared": True,
        "outputRemoved": True,
        "restoration": {
            "scopeArmed": True,
            "bootstrapStateRestored": True,
            "playerEntity": True,
            "mapAndBattleState": True,
            "generatedRam": True,
            "sessionCartPatches": True,
            "callbacksCleared": True,
            "outputRemoved": True,
            "sessionRomDeleted": False,
        },
        "restorationMismatch": None,
        "error": "forced callback failure",
    }


def test_fixture_schemas_exact_matrix_static_join_and_index_bindings() -> None:
    fixture = _fixture()
    validate_json(fixture, rail.FIXTURE_SCHEMA, owner=rail.OWNER)
    validate_json(fixture["expectedObservation"], rail.OBSERVATION_SCHEMA, owner=rail.OWNER)
    rail._assert_expected_matrix(fixture)
    rail._assert_index_bindings()

    rules = fixture["static"]["staticFacingJoin"]["directionRules"]
    assert [
        (row["direction"], row["facing"], row["sourceSlot"], row["horizontalMirror"])
        for row in rules
    ] == [
        ("UP", 1, 0, False),
        ("LEFT", 2, 1, False),
        ("RIGHT", 0, 1, True),
        ("DOWN", 3, 2, False),
    ]

    for case_id in ("attempt-left", "attempt-down"):
        record = next(
            row for row in fixture["expectedObservation"]["records"] if row["caseId"] == case_id
        )
        assert [tick["sprite"]["counterAtSelection"] for tick in record["ticks"]] == [
            26,
            28,
            30,
            1,
            3,
            5,
            7,
            9,
            11,
            13,
            15,
            17,
            19,
        ]
        assert [tick["sprite"]["selectedHalf"] for tick in record["ticks"]] == [
            1,
            1,
            1,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            1,
            1,
            1,
        ]


def test_structural_schemas_reject_nested_extra_properties() -> None:
    fixture = _fixture()
    invalid = deepcopy(fixture)
    invalid["expectedObservation"]["records"][0]["ticks"][0]["sprite"]["extra"] = 1
    with pytest.raises(ValueError, match="Additional properties"):
        validate_json(invalid, rail.FIXTURE_SCHEMA, owner=rail.OWNER)

    observation = deepcopy(fixture["expectedObservation"])
    observation["admission"]["entity"]["extra"] = 1
    with pytest.raises(ValueError, match="Additional properties"):
        validate_json(observation, rail.OBSERVATION_SCHEMA, owner=rail.OWNER)

    failure = _failure_payload()
    failure["restoration"]["extra"] = True
    with pytest.raises(ValueError, match="Additional properties"):
        validate_json(failure, rail.FAILURE_SCHEMA, owner=rail.OWNER)


def _change_admission_counter(fixture: dict[str, Any]) -> None:
    fixture["expectedObservation"]["admission"]["sprite"]["counterAtSelection"] = 24


def _change_selected_half(fixture: dict[str, Any]) -> None:
    fixture["expectedObservation"]["records"][1]["ticks"][3]["sprite"]["selectedHalf"] = 1


def _change_counter_after(fixture: dict[str, Any]) -> None:
    fixture["expectedObservation"]["records"][1]["ticks"][2]["sprite"]["counterAfter"] = 1


def _change_cross_vint_state(fixture: dict[str, Any]) -> None:
    fixture["expectedObservation"]["records"][1]["ticks"][1]["beforeEntities"]["animCounter"] = 26


def _change_blocked_motion(fixture: dict[str, Any]) -> None:
    fixture["expectedObservation"]["records"][0]["ticks"][0]["inputAttempt"]["after"][
        "xVelocity"
    ] = 32


def _change_success_install(fixture: dict[str, Any]) -> None:
    fixture["expectedObservation"]["records"][1]["ticks"][0]["inputAttempt"]["after"]["xDest"] += (
        384
    )


def _change_settled_half(fixture: dict[str, Any]) -> None:
    fixture["expectedObservation"]["records"][3]["settled"]["animCounter"] = 19


@pytest.mark.parametrize(
    "mutate",
    (
        _change_admission_counter,
        _change_selected_half,
        _change_counter_after,
        _change_cross_vint_state,
        _change_blocked_motion,
        _change_success_install,
        _change_settled_half,
    ),
)
def test_exact_runtime_semantics_reject_schema_valid_drift(
    mutate: Callable[[dict[str, Any]], None],
) -> None:
    fixture = _fixture()
    mutate(fixture)
    validate_json(fixture, rail.FIXTURE_SCHEMA, owner=rail.OWNER)
    with pytest.raises(ValueError, match="Map 3 locomotion"):
        rail._assert_expected_matrix(fixture)


def test_observer_roles_are_typed_and_golden_data_is_not_injected() -> None:
    rail._assert_lua_roles()
    source = rail.OBSERVER.read_text(encoding="utf-8")
    roles = set(re.findall(r'add_callback\([^,\n]+,\s*"([^"]+)"', source))
    schema_roles = set(load_json(rail.FAILURE_SCHEMA)["properties"]["role"]["enum"])
    assert roles <= schema_roles
    assert schema_roles >= rail.REQUIRED_LUA_ROLES
    for forbidden in ("expectedObservation", "staticFacingJoin", "sourceSlot"):
        assert forbidden not in source
    main_loop = source[source.index("while true do") :]
    assert "if pending_bootstrap_save then" in main_loop
    assert "if pending_admission_save then" in main_loop
    assert main_loop.count("memorysavestate.savecorestate()") == 2


def test_typed_callback_failure_requires_closed_cleanup(
    tmp_path: Path, monkeypatch: pytest.MonkeyPatch
) -> None:
    payload = _failure_payload()
    validate_json(payload, rail.FAILURE_SCHEMA, owner=rail.OWNER)
    status = tmp_path / "failure.status.txt"
    status.write_text(
        "failure:observer-callback:" + json.dumps(payload, sort_keys=True) + "\n",
        encoding="utf-8",
    )
    monkeypatch.setattr(rail, "STATUS_PATH", status)
    assert rail._failure_diagnostic() == payload

    payload["callbackCount"] = 1
    status.write_text(
        "failure:observer-callback:" + json.dumps(payload, sort_keys=True) + "\n",
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="residual callback"):
        rail._failure_diagnostic()


def test_source_contract_when_private_inputs_are_registered() -> None:
    rom = rail.repo_path("local/roms/sf2-us.bin")
    upstream = rail.repo_path("local/upstream/SF2DISASM")
    if not rom.is_file() or not upstream.is_dir():
        pytest.skip("registered local private inputs are unavailable")
    contract = rail.build_source_contract(rom, upstream)
    rail._assert_fixture(_fixture(), contract)
    assert contract["facts"]["updateOrder"] == [
        "movement-state-and-counter",
        "control-input-attempt",
        "sprite-half-selection",
        "sprite-counter-increment",
    ]
