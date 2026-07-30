import re
from copy import deepcopy
from functools import lru_cache

import pytest

import sf2tool.h3.force_state_active_party as active_party
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path


@lru_cache(maxsize=1)
def _static() -> dict[str, object]:
    return active_party.build_force_state_active_party_static_contract(
        repo_path("local/upstream/SF2DISASM"), repo_path("local/roms/sf2-us.bin")
    )


def test_static_contract_binds_all_four_handlers_and_source_polarities() -> None:
    static = _static()
    assert [row["handler"] for row in static["handlers"]] == list(active_party.HANDLERS)
    assert [row["address"] for row in static["handlers"]] == [290562, 290780, 290816, 290824]
    assert static["handlers"][0]["branchRecords"][0]["branchInstruction"] == "bne.w @Return"
    assert static["handlers"][1]["branchRecords"][0]["branchInstruction"] == "bne.s @SetAiControl"


def test_runtime_contract_derives_reset_and_follower_use_sites() -> None:
    upstream = repo_path("local/upstream/SF2DISASM")
    runtime = active_party._runtime_contract(_static(), upstream)
    reset = runtime["resetServices"]
    assert [row["code"] for row in reset["calls"]] == [
        "GHP",
        "GMAX",
        "SHP",
        "GMP",
        "SMP",
        "GSTATUS",
        "SSTATUS",
        "UPDATE",
    ]
    assert [row["h1Address"] for row in reset["calls"]] == [
        293070,
        293076,
        293082,
        293088,
        293094,
        293100,
        293110,
        293116,
    ]
    assert reset["allyCounter"] == 29
    assert reset["preUpdateStatusRetainMask"] == 7
    assert runtime["follower"] == {"parameterOffsets": [30, 32, 34], "blockBytes": 42}
    assert runtime["ram"]["forceFlagActiveStart"] == 32


def test_call_parser_accepts_suffixes_and_rejects_near_misses() -> None:
    assert active_party._calls(["jsr.w Target", "bsr.s Other", "move.w d0,d1"]) == [
        "jsr.w Target",
        "bsr.s Other",
    ]
    assert active_party._calls(["jsr (a0)", "bsr d0", "jsr.w (a1)"]) == []


def test_runtime_contract_rejects_join_force_owner_source_mutation(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    upstream = repo_path("local/upstream/SF2DISASM")
    static = _static()
    original = active_party.Path.read_text

    def mutated_read_text(path: active_party.Path, *args: object, **kwargs: object) -> str:
        text = original(path, *args, **kwargs)
        if path == upstream / active_party.OWNER_SOURCES["battleParty"]:
            return text.replace("bcc.s   @SkipActiveForce", "bcs.s   @SkipActiveForce", 1)
        return text

    monkeypatch.setattr(active_party.Path, "read_text", mutated_read_text)
    with pytest.raises(ValueError, match="JoinForce owner chain drift"):
        active_party._runtime_contract(static, upstream)


def test_runtime_contract_rejects_reset_call_order_source_mutation(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    upstream = repo_path("local/upstream/SF2DISASM")
    static = _static()
    original = active_party.Path.read_text

    def mutated_read_text(path: active_party.Path, *args: object, **kwargs: object) -> str:
        text = original(path, *args, **kwargs)
        if path == upstream / active_party.H1_LISTING:
            start = text.index("ResetAlliesBattleStats:")
            before, section = text[:start], text[start:]
            return before + re.sub(r"jsr\s+j_GetMaxHp", "jsr     j_GetCurrentHp", section, count=1)
        return text

    monkeypatch.setattr(active_party.Path, "read_text", mutated_read_text)
    with pytest.raises(ValueError, match="reset source use-site drift"):
        active_party._runtime_contract(static, upstream)


@pytest.mark.parametrize(
    ("old", "new", "message"),
    [
        (
            "andi.w  #STATUSEFFECT_STUN|STATUSEFFECT_POISON|STATUSEFFECT_CURSE,d1",
            "andi.w  #STATUSEFFECT_STUN|STATUSEFFECT_POISON,d1",
            "reset status-mask use-site drift",
        ),
        ("moveq   #COMBATANT_ALLIES_COUNTER,d7", "moveq   #28,d7", "reset counter use-site drift"),
    ],
)
def test_runtime_contract_rejects_reset_source_operands(
    monkeypatch: pytest.MonkeyPatch, old: str, new: str, message: str
) -> None:
    upstream = repo_path("local/upstream/SF2DISASM")
    static = _static()
    original = active_party.Path.read_text

    def mutated_read_text(path: active_party.Path, *args: object, **kwargs: object) -> str:
        text = original(path, *args, **kwargs)
        if path == upstream / active_party.H1_LISTING:
            start = text.index("ResetAlliesBattleStats:")
            return text[:start] + text[start:].replace(old, new, 1)
        return text

    monkeypatch.setattr(active_party.Path, "read_text", mutated_read_text)
    with pytest.raises(ValueError, match=message):
        active_party._runtime_contract(static, upstream)


@pytest.mark.parametrize(
    ("owner", "old", "new", "message"),
    [
        ("follower", "addi.l  #42", "addi.l  #43", "follower parameter/block use-site drift"),
        (
            "follower",
            "move.w  d2,$20(a1)",
            "move.w  d2,$24(a1)",
            "follower parameter/block use-site drift",
        ),
    ],
)
def test_runtime_contract_rejects_follower_owner_source_mutations(
    monkeypatch: pytest.MonkeyPatch, owner: str, old: str, new: str, message: str
) -> None:
    upstream = repo_path("local/upstream/SF2DISASM")
    static = _static()
    original = active_party.Path.read_text

    def mutated_read_text(path: active_party.Path, *args: object, **kwargs: object) -> str:
        text = original(path, *args, **kwargs)
        if path == upstream / active_party.OWNER_SOURCES[owner]:
            return text.replace(old, new, 1)
        return text

    monkeypatch.setattr(active_party.Path, "read_text", mutated_read_text)
    with pytest.raises(ValueError, match=message):
        active_party._runtime_contract(static, upstream)


def test_static_contract_rejects_csc54_branch_polarity_mutation(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    upstream = repo_path("local/upstream/SF2DISASM")
    original = active_party.Path.read_text

    def mutated_read_text(path: active_party.Path, *args: object, **kwargs: object) -> str:
        text = original(path, *args, **kwargs)
        if path == upstream / active_party.MAP_SOURCE:
            start = text.index("csc54_joinForceAi:")
            before, section = text[:start], text[start:]
            return before + section.replace("bne.s   @SetAiControl", "beq.s   @SetAiControl", 1)
        return text

    monkeypatch.setattr(active_party.Path, "read_text", mutated_read_text)
    with pytest.raises(ValueError, match="active-party source guard drift"):
        active_party.build_force_state_active_party_static_contract(
            upstream, repo_path("local/roms/sf2-us.bin")
        )


def test_runtime_contract_rejects_csc54_ai_mask_operand_mutation(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    upstream = repo_path("local/upstream/SF2DISASM")
    original = active_party.Path.read_text

    def mutated_read_text(path: active_party.Path, *args: object, **kwargs: object) -> str:
        text = original(path, *args, **kwargs)
        if path == upstream / active_party.MAP_SOURCE:
            start = text.index("csc54_joinForceAi:")
            return text[:start] + text[start:].replace(
                "ori.w   #AIBITFIELD_AI_CONTROLLED,d1", "ori.w   #8,d1", 1
            )
        return text

    monkeypatch.setattr(active_party.Path, "read_text", mutated_read_text)
    with pytest.raises(ValueError):
        active_party.build_force_state_active_party_static_contract(
            upstream, repo_path("local/roms/sf2-us.bin")
        )


@pytest.mark.parametrize(
    ("section", "old", "new"),
    [
        ("csc51_joinBattleParty:", "subq.w  #2,d7", "subq.w  #1,d7"),
        ("csc51_joinBattleParty:", "jsr     j_LeaveBattleParty", "jsr     j_JoinBattleParty"),
        ("csc56_addFollower:", "cmpi.b  #-1,(a0)", "cmpi.b  #-2,(a0)"),
    ],
)
def test_static_contract_rejects_local_handler_order_mutations(
    monkeypatch: pytest.MonkeyPatch, section: str, old: str, new: str
) -> None:
    upstream = repo_path("local/upstream/SF2DISASM")
    original = active_party.Path.read_text

    def mutated_read_text(path: active_party.Path, *args: object, **kwargs: object) -> str:
        text = original(path, *args, **kwargs)
        if path == upstream / active_party.MAP_SOURCE:
            start = text.index(section)
            return text[:start] + text[start:].replace(old, new, 1)
        return text

    monkeypatch.setattr(active_party.Path, "read_text", mutated_read_text)
    with pytest.raises(ValueError):
        active_party.build_force_state_active_party_static_contract(
            upstream, repo_path("local/roms/sf2-us.bin")
        )


def test_case_matrix_and_strict_schemas_reject_boundary_mutations() -> None:
    fixture = load_json(active_party.FIXTURE)
    validate_json(fixture, active_party.FIXTURE_SCHEMA, owner="active-party fixture")
    validate_json(
        fixture["observation"], active_party.OBSERVATION_SCHEMA, owner="active-party observation"
    )
    assert fixture["cases"] == active_party.derive_force_state_active_party_cases(_static())
    runtime = active_party._runtime_contract(_static(), repo_path("local/upstream/SF2DISASM"))
    assert fixture["caseInputs"] == active_party._case_inputs(
        fixture["cases"], runtime["aiControl"]["mask"]
    )
    assert [row["id"] for row in fixture["observation"]["records"]] == fixture["observation"][
        "recordOrder"
    ]
    missing = deepcopy(fixture)
    del missing["cases"][0]["axis"]
    with pytest.raises(ValueError):
        validate_json(missing, active_party.FIXTURE_SCHEMA, owner="missing")
    extra = deepcopy(fixture)
    extra["observation"]["records"][0]["extra"] = True
    with pytest.raises(ValueError):
        validate_json(extra["observation"], active_party.OBSERVATION_SCHEMA, owner="extra")
    missing_observation = deepcopy(fixture["observation"])
    del missing_observation["records"][0]["before"]
    with pytest.raises(ValueError):
        validate_json(
            missing_observation, active_party.OBSERVATION_SCHEMA, owner="missing observation"
        )
    renamed_observation = deepcopy(fixture["observation"])
    renamed_observation["records"][0]["after"]["members"][0]["joinedState"] = renamed_observation[
        "records"
    ][0]["after"]["members"][0].pop("joined")
    with pytest.raises(ValueError):
        validate_json(
            renamed_observation, active_party.OBSERVATION_SCHEMA, owner="renamed observation"
        )
    out_of_bound_observation = deepcopy(fixture["observation"])
    out_of_bound_observation["records"][0]["after"]["members"][0]["member"] = 32
    with pytest.raises(ValueError):
        validate_json(
            out_of_bound_observation,
            active_party.OBSERVATION_SCHEMA,
            owner="out of bound observation",
        )
    changed_input = deepcopy(fixture)
    changed_input["caseInputs"][7]["state"]["followers"] = [0, -1]
    with pytest.raises(ValueError):
        validate_json(changed_input, active_party.FIXTURE_SCHEMA, owner="changed input")
    missing_input = deepcopy(fixture)
    del missing_input["caseInputs"][0]["streamBytes"]
    with pytest.raises(ValueError):
        validate_json(missing_input, active_party.FIXTURE_SCHEMA, owner="missing input")
    extra_input = deepcopy(fixture)
    extra_input["caseInputs"][0]["state"]["extra"] = True
    with pytest.raises(ValueError):
        validate_json(extra_input, active_party.FIXTURE_SCHEMA, owner="extra input")
    renamed_input = deepcopy(fixture)
    renamed_input["caseInputs"][0]["stream"] = renamed_input["caseInputs"][0].pop("streamBytes")
    with pytest.raises(ValueError):
        validate_json(renamed_input, active_party.FIXTURE_SCHEMA, owner="renamed input")
    reordered_input = deepcopy(fixture)
    reordered_input["caseInputs"][0:2] = reversed(reordered_input["caseInputs"][0:2])
    with pytest.raises(ValueError):
        validate_json(reordered_input, active_party.FIXTURE_SCHEMA, owner="reordered input")
    out_of_bound_input = deepcopy(fixture)
    out_of_bound_input["caseInputs"][0]["streamBytes"][0] = 256
    with pytest.raises(ValueError):
        validate_json(out_of_bound_input, active_party.FIXTURE_SCHEMA, owner="out of bound input")
    changed_runtime = deepcopy(fixture)
    changed_runtime["runtimeContract"]["handlers"][0]["calls"][0]["h1Address"] += 2
    with pytest.raises(ValueError):
        validate_json(changed_runtime, active_party.FIXTURE_SCHEMA, owner="changed runtime")
    missing_runtime = deepcopy(fixture)
    del missing_runtime["runtimeContract"]["handlers"]
    with pytest.raises(ValueError):
        validate_json(missing_runtime, active_party.FIXTURE_SCHEMA, owner="missing runtime")
    renamed_runtime = deepcopy(fixture)
    renamed_runtime["runtimeContract"]["ai"] = renamed_runtime["runtimeContract"].pop("aiControl")
    with pytest.raises(ValueError):
        validate_json(renamed_runtime, active_party.FIXTURE_SCHEMA, owner="renamed runtime")
    changed_observation = deepcopy(fixture["observation"])
    changed_observation["records"][1]["after"]["members"][0]["active"] = True
    changed_fixture_observation = deepcopy(fixture)
    changed_fixture_observation["observation"] = changed_observation
    with pytest.raises(ValueError):
        validate_json(
            changed_fixture_observation, active_party.FIXTURE_SCHEMA, owner="changed observation"
        )
    reordered_records = deepcopy(fixture["observation"])
    reordered_records["records"][0:2] = reversed(reordered_records["records"][0:2])
    with pytest.raises(ValueError):
        validate_json(reordered_records, active_party.OBSERVATION_SCHEMA, owner="reordered records")
    reordered_fixture_order = deepcopy(fixture)
    reordered_fixture_order["observation"]["recordOrder"][0:2] = reversed(
        reordered_fixture_order["observation"]["recordOrder"][0:2]
    )
    with pytest.raises(ValueError):
        validate_json(
            reordered_fixture_order, active_party.FIXTURE_SCHEMA, owner="reordered fixture order"
        )
    renamed_record = deepcopy(fixture["observation"])
    renamed_record["records"][0]["id"] = "renamed"
    with pytest.raises(ValueError):
        validate_json(renamed_record, active_party.OBSERVATION_SCHEMA, owner="renamed record")
    reordered = deepcopy(fixture)
    reordered["observation"]["recordOrder"][0:2] = reversed(
        reordered["observation"]["recordOrder"][0:2]
    )
    assert reordered["observation"]["recordOrder"] != [row["id"] for row in fixture["cases"]]
