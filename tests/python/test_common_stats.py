import json
import shutil
import subprocess
from copy import deepcopy

import pytest

from sf2tool.h2.stats import (
    _combatant_getter_contract,
    _combatant_mutation_contract,
    build_stats_inventory,
)
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

UPSTREAM = repo_path("local/upstream/SF2DISASM")
FIXTURE = repo_path("tests/fixtures/h2/common-stats-static-v1.json")
OUTPUT_SCHEMA = repo_path("schemas/common-stats-static.schema.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-common-stats-static-fixture.schema.json")
MUTATION_ROUTINES = [
    "LoadAllyName",
    "SetClass",
    "SetLevel",
    "SetMaxHp",
    "SetCurrentHp",
    "SetMaxMp",
    "SetCurrentMp",
    "SetBaseAtt",
    "SetCurrentAtt",
    "SetBaseDef",
    "SetCurrentDef",
    "SetBaseAgi",
    "SetCurrentAgi",
    "SetBaseMov",
    "SetCurrentMov",
    "SetBaseResistance",
    "SetCurrentResistance",
    "SetBaseProwess",
    "SetCurrentProwess",
    "SetStatusEffects",
    "SetCombatantX",
    "SetCombatantY",
    "SetCurrentExp",
    "SetMovetypeAndAiCommandset",
    "SetMoveOrders",
    "SetTriggerRegions",
    "SetActivationBitfield",
    "SetEnemyIndex",
    "IncreaseLevel",
    "IncreaseMaxHp",
    "IncreaseCurrentHp",
    "IncreaseMaxMp",
    "IncreaseCurrentMp",
    "IncreaseBaseAtt",
    "IncreaseCurrentAtt",
    "IncreaseBaseDef",
    "IncreaseCurrentDef",
    "IncreaseBaseAgi",
    "IncreaseCurrentAgi",
    "IncreaseBaseMov",
    "IncreaseCurrentMov",
    "IncreaseExp",
    "IncreaseKills",
    "IncreaseDefeats",
    "DecreaseCurrentHp",
    "DecreaseCurrentMp",
    "DecreaseCurrentAtt",
    "DecreaseBaseDef",
    "DecreaseCurrentDef",
    "DecreaseBaseAgi",
    "DecreaseCurrentAgi",
    "DecreaseBaseMov",
    "DecreaseCurrentMov",
]


def _copy_getter_sources(tmp_path):
    upstream = UPSTREAM
    disasm = tmp_path / "disasm"
    for relative in (
        "sf2enums.asm",
        "code/common/stats/combatantstats_1.asm",
        "code/common/stats/combatantstats_2.asm",
        "code/common/stats/combatantstats_3.asm",
    ):
        destination = disasm / relative
        destination.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(upstream / "disasm" / relative, destination)
    listing = tmp_path / "build/sf2build-h1.lst"
    listing.parent.mkdir(parents=True, exist_ok=True)
    shutil.copyfile(upstream / "build/sf2build-h1.lst", listing)
    return disasm


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_combatant_getter_contract_matches_full_fixture_and_boundaries() -> None:
    output = build_stats_inventory(UPSTREAM)
    fixture = load_json(FIXTURE)
    actual = output["statsFacts"]["combatantGetterContract"]
    expected = fixture["expected"]["statsFacts"]["combatantGetterContract"]
    assert actual == expected
    assert actual["routineOrder"] == [
        "GetCombatantName",
        "GetClass",
        "GetLevel",
        "GetMaxHp",
        "GetCurrentHp",
        "GetMaxMp",
        "GetCurrentMp",
        "GetBaseAtt",
        "GetCurrentAtt",
        "GetBaseDef",
        "GetCurrentDef",
        "GetBaseAgi",
        "GetCurrentAgi",
        "GetBaseMov",
        "GetCurrentMov",
        "GetBaseResistance",
        "GetCurrentResistance",
        "GetBaseProwess",
        "GetCurrentProwess",
        "GetStatusEffects",
        "GetCombatantX",
        "GetCombatantY",
        "GetCurrentExp",
        "GetMovetype",
        "GetAiCommandset",
        "GetMoveOrders",
        "GetTriggerRegions",
        "GetActivationBitfield",
        "GetEnemy",
        "GetKills",
        "GetDefeats",
    ]
    assert actual["sourceRange"] == {
        "path": "code/common/stats/combatantstats_1.asm",
        "startAddress": 33488,
        "endAddressExclusive": 34074,
        "physicalSpanBytes": 586,
    }
    assert actual["entryAddressAbi"] == expected["entryAddressAbi"]
    assert {
        name: actual["getters"][name]
        for name in ("GetMovetype", "GetAiCommandset", "GetMoveOrders", "GetTriggerRegions")
    } == {
        name: expected["getters"][name]
        for name in ("GetMovetype", "GetAiCommandset", "GetMoveOrders", "GetTriggerRegions")
    }
    assert actual["getters"]["GetCombatantName"] == expected["getters"]["GetCombatantName"]
    assert actual["getters"]["GetEnemy"] == expected["getters"]["GetEnemy"]
    assert set(actual["internalEffectiveDirectCallSiteCounts"]) == set(actual["routineOrder"])
    assert set(actual["externalEffectiveDirectCallSiteCounts"]) == set(actual["routineOrder"])
    assert actual["staticBoundary"] == {
        "callerVisibleMeaning": "inferred",
        "callerAndRuntimeOutcome": "unknown",
        "setterAndClampFollowup": "unknown",
    }


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_combatant_mutation_contract_matches_full_fixture_and_boundaries() -> None:
    output = build_stats_inventory(UPSTREAM)
    fixture = load_json(FIXTURE)
    actual = output["statsFacts"]["combatantMutationContract"]
    expected = fixture["expected"]["statsFacts"]["combatantMutationContract"]
    assert actual == expected
    assert actual["routineOrder"] == MUTATION_ROUTINES
    assert actual["sourceRange"] == {
        "path": "code/common/stats/combatantstats_2.asm",
        "startAddress": 34138,
        "endAddressExclusive": 35184,
        "physicalSpanBytes": 1046,
    }
    assert actual["wrappers"] == expected["wrappers"]
    assert actual["dependencyAbi"] == expected["dependencyAbi"]
    dependencies = actual["dependencyAbi"]["routines"]
    assert list(dependencies) == [
        "DecreaseAndClampByte",
        "DecreaseAndClampWord",
        "IncreaseAndClamp7Bits",
        "IncreaseAndClampByte",
        "IncreaseAndClampWord",
        "SetCombatantByte",
        "SetCombatantWord",
    ]
    assert {name: fact["accessMode"] for name, fact in dependencies.items()} == {
        "DecreaseAndClampByte": "readModifyWrite",
        "DecreaseAndClampWord": "readModifyWrite",
        "IncreaseAndClamp7Bits": "readModifyWrite",
        "IncreaseAndClampByte": "readModifyWrite",
        "IncreaseAndClampWord": "readModifyWrite",
        "SetCombatantByte": "writeOnly",
        "SetCombatantWord": "writeOnly",
    }
    assert dependencies["SetCombatantByte"]["result"] is None
    assert dependencies["SetCombatantWord"]["result"] is None
    assert actual["classificationCounts"] == {
        "loadAllyName": 1,
        "directSet": 27,
        "increase": 16,
        "decrease": 9,
    }
    assert actual["internalEffectiveDirectCallSiteCounts"] == {
        name: 0 for name in actual["routineOrder"]
    }
    assert sum(actual["externalEffectiveDirectCallSiteCounts"].values()) == 184
    assert len(actual["externalDirectCallerOccurrences"]) == 34
    assert actual["wrappers"]["LoadAllyName"] == expected["wrappers"]["LoadAllyName"]
    assert actual["wrappers"]["SetMoveOrders"] == expected["wrappers"]["SetMoveOrders"]
    assert actual["wrappers"]["SetTriggerRegions"] == expected["wrappers"]["SetTriggerRegions"]
    assert actual["staticBoundary"] == {
        "clampAlgorithm": "unknown",
        "callerAndRuntimeOutcome": "unknown",
    }
    assert set(actual["internalEffectiveDirectCallSiteCounts"]) == set(actual["routineOrder"])
    assert set(actual["externalEffectiveDirectCallSiteCounts"]) == set(actual["routineOrder"])


def test_combatant_mutation_schema_rejects_deep_drift() -> None:
    fixture = load_json(FIXTURE)
    schema = load_json(FIXTURE_SCHEMA)

    def invalid(mutate) -> None:
        broken = deepcopy(fixture)
        mutate(broken["expected"]["statsFacts"]["combatantMutationContract"])
        with pytest.raises(ValueError, match="statsFacts"):
            validate_json(broken, FIXTURE_SCHEMA, owner="statsFacts mutation contract")

    invalid(lambda value: value["wrappers"].pop("SetMoveOrders"))
    invalid(lambda value: value["routineOrder"].reverse())
    invalid(
        lambda value: value["wrappers"]["SetTriggerRegions"]["packedMerge"].__setitem__(
            "resultRegister", "d2"
        )
    )
    invalid(
        lambda value: value["dependencyAbi"]["routines"]["SetCombatantByte"].__setitem__(
            "storedWidthBits", 16
        )
    )
    invalid(lambda value: value["wrappers"]["LoadAllyName"]["copy"].pop("copiedBytes"))
    invalid(lambda value: value["wrappers"]["LoadAllyName"]["copy"].__setitem__("unexpected", True))
    invalid(
        lambda value: value["wrappers"].__setitem__(
            "SetMoveOrdersRenamed", value["wrappers"].pop("SetMoveOrders")
        )
    )
    invalid(lambda value: value["sourceRange"].__setitem__("physicalSpanBytes", 1047))
    invalid(
        lambda value: value["wrappers"]["LoadAllyName"]["copy"].__setitem__("copyIterations", 9)
    )
    invalid(
        lambda value: value["wrappers"]["IncreaseKills"]["guard"].__setitem__(
            "returnBranchTarget", "@Other"
        )
    )
    invalid(
        lambda value: value["dependencyAbi"]["routines"]["IncreaseAndClampByte"].pop(
            "fieldReadInstructionIndex"
        )
    )
    invalid(
        lambda value: value["dependencyAbi"]["routines"]["SetCombatantByte"].__setitem__(
            "accessMode", "readModifyWrite"
        )
    )
    invalid(lambda value: value.__setitem__("unexpected", True))
    assert schema["definitions"]["combatantMutationFacts"]["additionalProperties"] is False
    output_schema = load_json(OUTPUT_SCHEMA)
    assert (
        output_schema["definitions"]["combatantMutationFacts"]
        == schema["definitions"]["combatantMutationFacts"]
    )
    corpus = schema["definitions"]["combatantMutationFacts"]["properties"]["routineOperations"]
    assert all(
        item["allOf"][0]["items"] == {"$ref": "#/definitions/combatantGetterInstructionRecord"}
        for item in corpus["properties"].values()
    )


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("relative", "needle", "replacement"),
    (
        (
            "combatantstats_2.asm",
            "moveq   #COMBATANT_OFFSET_LEVEL,d7",
            "moveq   #COMBATANT_OFFSET_CLASS,d7",
        ),
        ("combatantstats_2.asm", "bsr.w   SetCombatantWord", "bsr.w   SetCombatantByte"),
        ("combatantstats_2.asm", "lsl.w   #BYTE_SHIFT_COUNT,d1", "lsl.w   #NIBBLE_SHIFT_COUNT,d1"),
        ("combatantstats_2.asm", "or.b    d2,d1", "or.b    d1,d2"),
        ("combatantstats_2.asm", "lea     (a0),a1", "lea     (a0),a0"),
        (
            "combatantstats_2.asm",
            "moveq   #ALLYNAME_CHARACTERS_COUNTER,d0",
            "moveq   #BYTE_SHIFT_COUNT,d0",
        ),
        ("combatantstats_2.asm", "dbf     d0,@Loop", "dbf     d0,@Return"),
        ("combatantstats_2.asm", "clr.w   d5", "moveq   #1,d5"),
        (
            "combatantstats_2.asm",
            "move.w  #CHAR_STATCAP_LEVEL,d6",
            "move.w  #CHAR_STATCAP_HP,d6",
        ),
        ("combatantstats_2.asm", "blt.s   @Return", "bge.s   @Return"),
        (
            "combatantstats_2.asm",
            "move.w  COMBATANT_OFFSET_HP_MAX(a0),d6",
            "move.b  COMBATANT_OFFSET_HP_MAX(a0),d6",
        ),
        (
            "combatantstats_2.asm",
            "rts\n\n    ; End of function SetClass",
            "nop\n\n    ; End of function SetClass",
        ),
        ("combatantstats_3.asm", "move.b  d1,(a0,d7.w)", "move.w  d1,(a0,d7.w)"),
        ("combatantstats_3.asm", "add.b   (a0,d7.w),d1", "add.w   (a0,d7.w),d1"),
    ),
)
def test_combatant_mutation_guards_reject_semantic_mutations(
    tmp_path, relative, needle, replacement
) -> None:
    disasm = _copy_getter_sources(tmp_path)
    source = disasm / "code/common/stats" / relative
    source.write_text(
        source.read_text(encoding="utf-8").replace(needle, replacement, 1), encoding="utf-8"
    )
    with pytest.raises(ValueError, match="combatant mutation"):
        _combatant_mutation_contract(disasm)


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("routine", "needle", "replacement"),
    (
        ("IncreaseCurrentHp", "#COMBATANT_OFFSET_HP_CURRENT,d7", "#COMBATANT_OFFSET_HP_MAX,d7"),
        ("DecreaseCurrentMp", "#COMBATANT_OFFSET_MP_CURRENT,d7", "#COMBATANT_OFFSET_MP_MAX,d7"),
        ("SetMoveOrders", "#COMBATANT_OFFSET_MOVE_ORDERS,d7", "#COMBATANT_OFFSET_ALLY_KILLS,d7"),
        ("IncreaseCurrentDef", "movem.l d5-a0,-(sp)", "movem.l d6-a0,-(sp)"),
    ),
)
def test_combatant_mutation_scoped_guards_reject_nonfirst_source_drift(
    tmp_path, routine, needle, replacement
) -> None:
    disasm = _copy_getter_sources(tmp_path)
    source = disasm / "code/common/stats/combatantstats_2.asm"
    text = source.read_text(encoding="utf-8")
    start = text.index(f"{routine}:")
    end = text.index("    ; End of function", start)
    section = text[start:end]
    assert needle in section
    source.write_text(
        text[:start] + section.replace(needle, replacement, 1) + text[end:], encoding="utf-8"
    )
    with pytest.raises(ValueError, match="combatant mutation"):
        _combatant_mutation_contract(disasm)


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("relative", "needle", "replacement"),
    (
        ("combatantstats_1.asm", "bne.s   @Enemy", "beq.s   @Enemy"),
        (
            "combatantstats_1.asm",
            "moveq   #COMBATANT_OFFSET_LEVEL,d7",
            "moveq   #COMBATANT_OFFSET_CLASS,d7",
        ),
        (
            "combatantstats_1.asm",
            "moveq   #COMBATANT_OFFSET_HP_MAX,d7",
            "moveq   #COMBATANT_OFFSET_HP_CURRENT,d7",
        ),
        ("combatantstats_1.asm", "bsr.w   GetCombatantWord", "bsr.w   GetCombatantByte"),
        ("combatantstats_1.asm", "move.w  d1,d2", "move.w  d2,d1"),
        ("combatantstats_1.asm", "lsr.w   #NIBBLE_SHIFT_COUNT,d1", "lsr.w   #BYTE_SHIFT_COUNT,d1"),
        ("combatantstats_1.asm", "andi.w  #BYTE_LOWER_NIBBLE_MASK,d2", "andi.w  #BYTE_MASK,d2"),
        ("combatantstats_1.asm", "move.w  #-1,d1", "move.w  #0,d1"),
        (
            "combatantstats_1.asm",
            "rts\n\n    ; End of function GetEnemy",
            "bra.s   @Continue\n\n    ; End of function GetEnemy",
        ),
        ("combatantstats_3.asm", "bcc.s   @Enemy", "bcs.s   @Enemy"),
        ("combatantstats_3.asm", "clr.w   d1", "nop"),
        ("combatantstats_3.asm", "move.w  (a0,d7.w),d1", "move.b  (a0,d7.w),d1"),
    ),
)
def test_combatant_getter_guards_reject_semantic_mutations(
    tmp_path, relative, needle, replacement
) -> None:
    disasm = _copy_getter_sources(tmp_path)
    source = disasm / "code/common/stats" / relative
    source.write_text(
        source.read_text(encoding="utf-8").replace(needle, replacement, 1), encoding="utf-8"
    )
    with pytest.raises(ValueError, match="combatant|shop"):
        _combatant_getter_contract(disasm)


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("routine", "needle", "replacement"),
    (
        ("GetMovetype", "lsr.w   #NIBBLE_SHIFT_COUNT,d1", "lsr.w   #BYTE_SHIFT_COUNT,d1"),
        ("GetMovetype", "andi.w  #BYTE_LOWER_NIBBLE_MASK,d1", "andi.w  #BYTE_MASK,d1"),
        ("GetAiCommandset", "andi.w  #BYTE_LOWER_NIBBLE_MASK,d1", "andi.w  #BYTE_MASK,d1"),
        ("GetMoveOrders", "move.w  d1,d2", "move.w  d2,d1"),
        ("GetMoveOrders", "lsr.w   #BYTE_SHIFT_COUNT,d1", "lsr.w   #NIBBLE_SHIFT_COUNT,d1"),
        ("GetMoveOrders", "andi.w  #BYTE_MASK,d1", "andi.w  #BYTE_LOWER_NIBBLE_MASK,d1"),
        ("GetMoveOrders", "andi.w  #BYTE_MASK,d2", "andi.w  #BYTE_LOWER_NIBBLE_MASK,d2"),
        ("GetTriggerRegions", "move.w  d1,d2", "move.w  d2,d1"),
        ("GetTriggerRegions", "lsr.w   #NIBBLE_SHIFT_COUNT,d1", "lsr.w   #BYTE_SHIFT_COUNT,d1"),
        ("GetTriggerRegions", "andi.w  #BYTE_LOWER_NIBBLE_MASK,d2", "andi.w  #BYTE_MASK,d2"),
    ),
)
def test_composite_getter_guards_reject_copy_shift_and_mask_mutations(
    tmp_path, routine, needle, replacement
) -> None:
    disasm = _copy_getter_sources(tmp_path)
    source = disasm / "code/common/stats/combatantstats_1.asm"
    text = source.read_text(encoding="utf-8")
    start = text.index(f"{routine}:")
    end = text.index("    ; End of function", start)
    section = text[start:end]
    assert needle in section
    source.write_text(
        text[:start] + section.replace(needle, replacement, 1) + text[end:], encoding="utf-8"
    )
    with pytest.raises(ValueError, match="combatant"):
        _combatant_getter_contract(disasm)


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_combatant_getter_schemas_are_shared_closed_and_reject_drift() -> None:
    fixture = load_json(FIXTURE)
    output_schema = load_json(OUTPUT_SCHEMA)
    fixture_schema = load_json(FIXTURE_SCHEMA)
    for definition in ("combatantGetterInstructionRecord", "combatantGetterFacts"):
        assert output_schema["definitions"][definition] == fixture_schema["definitions"][definition]
    facts = output_schema["definitions"]["combatantGetterFacts"]
    assert facts["additionalProperties"] is False
    assert facts["properties"]["entryAddressAbi"]["additionalProperties"] is False
    assert facts["properties"]["getters"]["additionalProperties"] is False

    def invalid(mutate) -> None:
        broken = deepcopy(fixture)
        mutate(broken["expected"]["statsFacts"]["combatantGetterContract"])
        with pytest.raises(ValueError, match="statsFacts"):
            validate_json(broken, FIXTURE_SCHEMA, owner="statsFacts getter contract")

    invalid(lambda value: value.pop("routineOrder"))
    invalid(lambda value: value["getters"].pop("GetEnemy"))
    invalid(
        lambda value: value["getters"].__setitem__(
            "GetEnemyRenamed", value["getters"].pop("GetEnemy")
        )
    )
    invalid(lambda value: value["routineOrder"].reverse())
    invalid(lambda value: value.__setitem__("unexpected", True))
    invalid(lambda value: value["sourceRange"].pop("physicalSpanBytes"))
    invalid(
        lambda value: value["getters"]["GetMoveOrders"]["splitValue"]["copy"].__setitem__(
            "destinationRegister", "d1"
        )
    )
    invalid(
        lambda value: value["entryAddressAbi"]["routines"]["GetCombatantEntryAddress"][
            "validRoutes"
        ]["ally"].__setitem__("enemyThresholdBranchTaken", True)
    )


def test_combatant_mutation_slice_preserves_stats_siblings_from_head() -> None:
    fixture = load_json(FIXTURE)
    head_fixture = json.loads(
        subprocess.check_output(
            ["git", "show", "HEAD:tests/fixtures/h2/common-stats-static-v1.json"], text=True
        )
    )
    for name, value in head_fixture["expected"]["statsFacts"].items():
        assert fixture["expected"]["statsFacts"][name] == value
    output_schema = load_json(OUTPUT_SCHEMA)
    fixture_schema = load_json(FIXTURE_SCHEMA)
    head_output_schema = json.loads(
        subprocess.check_output(
            ["git", "show", "HEAD:schemas/common-stats-static.schema.json"], text=True
        )
    )
    head_fixture_schema = json.loads(
        subprocess.check_output(
            ["git", "show", "HEAD:schemas/h2-common-stats-static-fixture.schema.json"], text=True
        )
    )
    assert set(output_schema["definitions"]) - set(head_output_schema.get("definitions", {})) == {
        "combatantMutationFacts"
    }
    assert set(fixture_schema["definitions"]) - set(head_fixture_schema.get("definitions", {})) == {
        "combatantMutationFacts"
    }
    assert (
        output_schema["definitions"]["combatantGetterFacts"]
        == (head_output_schema["definitions"]["combatantGetterFacts"])
    )
    assert (
        fixture_schema["definitions"]["combatantGetterFacts"]
        == head_fixture_schema["definitions"]["combatantGetterFacts"]
    )
    assert (
        output_schema["definitions"]["combatantGetterInstructionRecord"]
        == head_output_schema["definitions"]["combatantGetterInstructionRecord"]
    )
    assert (
        fixture_schema["definitions"]["combatantGetterInstructionRecord"]
        == head_fixture_schema["definitions"]["combatantGetterInstructionRecord"]
    )
    assert (
        fixture["expected"]["statsFacts"]["combatantGetterContract"]
        == head_fixture["expected"]["statsFacts"]["combatantGetterContract"]
    )
    assert {
        key: value for key, value in output_schema["properties"].items() if key != "statsFacts"
    } == {
        key: value for key, value in head_output_schema["properties"].items() if key != "statsFacts"
    }
    assert {
        key: value for key, value in fixture_schema["properties"].items() if key != "expected"
    } == {
        key: value for key, value in head_fixture_schema["properties"].items() if key != "expected"
    }
