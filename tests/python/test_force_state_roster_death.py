from __future__ import annotations

import hashlib
import json
from copy import deepcopy
from pathlib import Path

import pytest

import sf2tool.h3.force_state_roster_death as roster_death
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path
from sf2tool.research_index import listing_symbol_addresses


def _static() -> dict:
    return roster_death.build_force_state_roster_death_static_contract(
        repo_path("local/upstream/SF2DISASM")
    )


@pytest.fixture(scope="module")
def _static_runtime() -> tuple[dict, dict]:
    static = _static()
    return static, roster_death._runtime_contract(static, repo_path("local/upstream/SF2DISASM"))


def test_static_contract_derives_the_closed_roster_death_boundary() -> None:
    static = _static()
    cases = roster_death.derive_force_state_roster_death_cases(static)
    assert [row["id"] for row in cases] == list(roster_death.CASE_ORDER)
    assert len(cases) == 14
    assert [row["scope"] for row in cases].count("persistence") == 4
    assert [row["scope"] for row in cases].count("local-only") == 10
    storage = static["storage"]
    assert storage["defeatedList"]["entryBytes"] == 1
    assert storage["defeatedList"]["logicalSavedDomain"] is False
    assert storage["rosterMembership"]["logicalSavedDomain"] is True
    assert storage["currentHp"]["logicalSavedDomain"] is True
    assert storage["saveLoad"]["logicalRam"]["logicalByteCount"] == 4016
    assert storage["saveLoad"]["physicalByteStride"] == 2
    assert storage["currentX"]["firstEnemyDataEntry"] == 32
    runtime = roster_death._runtime_contract(static, repo_path("local/upstream/SF2DISASM"))
    csc20 = roster_death._case_inputs(cases, runtime)[8]
    assert csc20["state"]["listTouchedByteCount"] == 33
    assert len(csc20["state"]["combatantXAddresses"]) == 32
    assert csc20["state"]["combatantXAddresses"][0] == 16772910


def test_frame_budget_rejects_the_observed_map_host_reentry_shortfall() -> None:
    static = _static()
    runtime = roster_death._runtime_contract(static, repo_path("local/upstream/SF2DISASM"))
    cases = roster_death.derive_force_state_roster_death_cases(static)
    fixture = {
        "staticContractSha256": roster_death._canonical_sha256(static),
        "runtimeContract": runtime,
        "function": {"runMapSetupInitFunctionAddress": runtime["entryAddress"]},
        "cases": cases,
        "caseInputs": roster_death._case_inputs(cases, runtime),
        "maxFrames": 2400,
        "observation": {"recordOrder": list(roster_death.CASE_ORDER)},
        "scopeClassification": {
            "localOnly": [case["id"] for case in cases if case["scope"] == "local-only"],
            "persistence": [case["id"] for case in cases if case["scope"] == "persistence"],
        },
    }
    with pytest.raises(ValueError, match="frame budget cannot accommodate all map-host reentries"):
        roster_death.validate_force_state_roster_death_fixture_semantics(
            deepcopy(fixture), static, runtime
        )


def test_csc20_observation_allows_exact_seed_plus_32_appends_but_not_more() -> None:
    fixture = load_json(roster_death.FIXTURE)
    observation = deepcopy(fixture["observation"])
    record = observation["records"][8]
    record["listAfter"] = [3] + list(range(128, 160))
    validate_json(observation, roster_death.OBSERVATION_SCHEMA, owner="csc20 exact range")
    record["listAfter"].append(160)
    with pytest.raises(ValueError, match="is too long"):
        validate_json(observation, roster_death.OBSERVATION_SCHEMA, owner="csc20 overrun")


def test_csc20_source_derived_branch_result_cannot_be_swapped() -> None:
    static = _static()
    runtime = roster_death._runtime_contract(static, repo_path("local/upstream/SF2DISASM"))
    fixture = load_json(roster_death.FIXTURE)
    fixture["observation"]["records"][8]["listAfter"] = [3, 128]
    fixture["observation"]["records"][8]["after"]["length"] = 2
    with pytest.raises(ValueError, match="csc20 branch/list result drift"):
        roster_death.validate_force_state_roster_death_fixture_semantics(fixture, static, runtime)


def test_observation_rejects_invented_list_fields_and_scoped_range_drift() -> None:
    static = _static()
    runtime = roster_death._runtime_contract(static, repo_path("local/upstream/SF2DISASM"))
    fixture = load_json(roster_death.FIXTURE)

    non_list = deepcopy(fixture)
    non_list["observation"]["records"][5]["listBefore"] = []
    non_list["observation"]["records"][5]["listAfter"] = []
    non_list["observation"]["records"][5]["before"]["length"] = 0
    non_list["observation"]["records"][5]["after"]["length"] = 0
    with pytest.raises(ValueError, match="invented list observation"):
        roster_death.validate_force_state_roster_death_fixture_semantics(non_list, static, runtime)

    cases = roster_death.derive_force_state_roster_death_cases(static)
    scoped_inputs = roster_death._case_inputs(cases, runtime)
    scoped_inputs[7]["state"]["listTouchedByteCount"] = 2
    with pytest.raises(ValueError, match="list scoped range drift"):
        roster_death._validate_fixture_observation(
            fixture["observation"], cases, scoped_inputs, runtime
        )


def test_active_party_fixture_remains_the_retained_nine_record_projection() -> None:
    active = load_json(repo_path("tests/fixtures/h3/force-state-active-party-v1.json"))
    assert active["id"] == "sf2-force-state-active-party-runtime-v1"
    assert len(active["observation"]["records"]) == 9
    canonical = json.dumps(active["observation"], sort_keys=True, separators=(",", ":"))
    assert hashlib.sha256(canonical.encode("utf-8")).hexdigest().upper() == (
        "9339FB969A6338259D3E5E4CD937B908D356F49237CAD61151B4245B54A4CB9E"
    )


def test_observer_config_preserves_runtime_inputs_without_fixture_golden(
    _static_runtime: tuple[dict, dict],
) -> None:
    fixture = load_json(roster_death.FIXTURE)
    static, runtime = _static_runtime
    config = roster_death.build_force_state_roster_death_observer_config(fixture, static, runtime)

    assert {"observation", "recordOrder", "records"}.isdisjoint(config)
    assert config["cases"] == fixture["cases"]
    assert config["caseInputs"] == fixture["caseInputs"]
    assert config["runtimeContract"] == fixture["runtimeContract"]
    assert config["scopeClassification"] == fixture["scopeClassification"]


def test_fixture_schema_recursively_closes_roster_death_inputs() -> None:
    fixture = load_json(roster_death.FIXTURE)

    top_level_extra = deepcopy(fixture)
    top_level_extra["unexpected"] = True
    with pytest.raises(ValueError, match="Additional properties"):
        validate_json(top_level_extra, roster_death.FIXTURE_SCHEMA, owner="roster/death top-level")

    nested_extra = deepcopy(fixture)
    nested_extra["caseInputs"][0]["state"]["joinedFlag"]["unexpected"] = True
    with pytest.raises(ValueError, match="Additional properties"):
        validate_json(nested_extra, roster_death.FIXTURE_SCHEMA, owner="roster/death nested extra")

    nested_missing = deepcopy(fixture)
    del nested_missing["caseInputs"][0]["state"]["joinedFlag"]["mask"]
    with pytest.raises(ValueError, match="mask.*required property"):
        validate_json(
            nested_missing, roster_death.FIXTURE_SCHEMA, owner="roster/death nested missing"
        )


@pytest.mark.parametrize(
    ("drift", "message"),
    [
        ("case-order", "case/input matrix drift"),
        ("case-id", "case/input matrix drift"),
        ("case-scope", "case/input matrix drift"),
        ("record-order", "fixture observation matrix drift"),
        ("golden-list-range", "csc20 branch/list result drift"),
    ],
)
def test_verify_rejects_schema_valid_semantic_drift_before_run_observer(
    monkeypatch: pytest.MonkeyPatch,
    drift: str,
    message: str,
    _static_runtime: tuple[dict, dict],
) -> None:
    fixture = load_json(roster_death.FIXTURE)
    if drift == "case-order":
        fixture["cases"][0], fixture["cases"][1] = fixture["cases"][1], fixture["cases"][0]
    elif drift == "case-id":
        fixture["cases"][0]["id"] = "csc08-join-renamed"
    elif drift == "case-scope":
        fixture["cases"][0]["scope"] = "local-only"
    elif drift == "record-order":
        order = fixture["observation"]["recordOrder"]
        order[0], order[1] = order[1], order[0]
    else:
        record = fixture["observation"]["records"][9]
        record["listAfter"] = [3] + list(range(128, 160))
        record["after"]["length"] = len(record["listAfter"])

    static, runtime = _static_runtime
    original_load_json = roster_death.load_json
    observer_called = False

    def fixture_load(path: Path) -> dict:
        if Path(path).resolve() == roster_death.FIXTURE.resolve():
            return fixture
        return original_load_json(path)

    def forbidden_observer(**_: object) -> dict:
        nonlocal observer_called
        observer_called = True
        raise AssertionError("run_observer must not run after prelaunch drift")

    monkeypatch.setattr(roster_death, "load_json", fixture_load)
    monkeypatch.setattr(roster_death, "verify_runtime_contract", lambda *_: None)
    monkeypatch.setattr(
        roster_death, "build_force_state_roster_death_static_contract", lambda *_: static
    )
    monkeypatch.setattr(roster_death, "_runtime_contract", lambda *_: runtime)
    monkeypatch.setattr(roster_death, "run_observer", forbidden_observer)
    with pytest.raises(ValueError, match=message):
        roster_death.verify_force_state_roster_death(
            repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
        )
    assert observer_called is False


@pytest.mark.parametrize(
    ("relative", "before", "after", "message"),
    [
        (
            roster_death.MAP_SOURCE_1,
            (
                "move.b  d0,(a1)\n"
                "                addq.w  #1,((DEAD_COMBATANTS_LIST_LENGTH-$1000000)).w"
            ),
            (
                "addq.w  #1,((DEAD_COMBATANTS_LIST_LENGTH-$1000000)).w\n"
                "                move.b  d0,(a1)"
            ),
            "source guard drift",
        ),
        (
            roster_death.MAP_SOURCE_1,
            "beq.s   loc_46B0E",
            "bne.s   loc_46B0E",
            "source guard drift",
        ),
    ],
)
def test_source_use_site_mutations_fail_before_any_fixture(
    relative: Path, before: str, after: str, message: str
) -> None:
    upstream = repo_path("local/upstream/SF2DISASM")
    source = upstream / relative
    text = source.read_text(encoding="utf-8")
    assert before in text
    source_1 = (upstream / roster_death.MAP_SOURCE_1).read_text(encoding="utf-8")
    source_2 = (upstream / roster_death.MAP_SOURCE_2).read_text(encoding="utf-8")
    if relative == roster_death.MAP_SOURCE_1:
        source_1 = source_1.replace(before, after, 1)
    elif relative == roster_death.MAP_SOURCE_2:
        source_2 = source_2.replace(before, after, 1)
    h2 = load_json(repo_path("tests/fixtures/h2/map-script-engine-static-v1.json"))["expected"][
        "forceStateCommandFacts"
    ]
    listing = (upstream / roster_death.H1_LISTING).read_text(encoding="utf-8")
    with pytest.raises(ValueError, match=message):
        roster_death._handler_records(
            h2, source_1, source_2, listing, listing_symbol_addresses(listing)
        )


def test_instruction_parser_ignores_comments_near_misses_and_accepts_suffixes() -> None:
    assert roster_death._calls(
        [
            "jsr.w j_JoinForce",
            "bsr.s j_GetCurrentHp",
            "note_j_JoinForce",
            "move.w j_JoinForce,d0",
            "jsr d0",
        ]
    ) == ["jsr.w j_JoinForce", "bsr.s j_GetCurrentHp"]


@pytest.mark.parametrize(
    ("before", "after", "message"),
    [
        (
            "moveq #$1F,d7",
            "moveq #$1E,d7",
            "csc20 loop counter does not match COMBATANT_ENEMIES_COUNTER",
        ),
        (
            "moveq #$FFFFFF80,d0",
            "moveq #$FFFFFF81,d0",
            "csc20 enemy start does not match COMBATANT_ENEMIES_START",
        ),
    ],
)
def test_csc20_operand_derivations_reject_before_fixture_construction(
    before: str, after: str, message: str
) -> None:
    upstream = repo_path("local/upstream/SF2DISASM")
    update = roster_death._section(
        (upstream / roster_death.MAP_SOURCE_1).read_text(encoding="utf-8"),
        "csc20_updateDefeatedAllies",
    )
    assert before in update
    mutated = [after if row == before else row for row in update]
    constants = roster_death._require_constants(roster_death._equates(upstream))
    with pytest.raises(ValueError, match=message):
        roster_death._derive_csc20_storage_shape(mutated, constants)
