from __future__ import annotations

import re
from copy import deepcopy
from hashlib import sha256
from json import dumps

import pytest

from sf2tool.h3.bizhawk import bizhawk_contract, validate_lua_syntax
from sf2tool.h3.witch_save_actions import (
    FIXTURE,
    OBSERVER,
    SCHEMA,
    _source_use_sites,
    build_witch_save_actions_source_contract,
)
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path


def _sources() -> tuple[str, str, str]:
    sram = """\
SaveGame:
 tst.b d0
 bne.s @Slot2
 lea (SAVE1_DATA).l,a1
 lea (SAVE1_CHECKSUM).l,a2
 clr.w d1
 lea (SAVE2_DATA).l,a1
 lea (SAVE2_CHECKSUM).l,a2
 moveq #1,d1
 move.w #SAVE_SLOT_REAL_SIZE,d7
 bsr.w CopyBytesToSram
 move.b d0,(a2)
 bset d1,(SAVE_FLAGS).l
 ; End of function SaveGame
LoadGame:
 lea (COMBATANT_DATA).l,a1
 tst.b d0
 bne.s @Slot2
 lea (SAVE1_DATA).l,a0
 clr.w d1
 lea (SAVE2_DATA).l,a0
 moveq #1,d1
 move.w #SAVE_SLOT_REAL_SIZE,d7
 bsr.w CopyBytesFromSram
 ; End of function LoadGame
CopySave:
 bsr.s LoadGame
 eori.w #1,d0
 andi.w #1,d0
 bsr.s SaveGame
 ; End of function CopySave
ClearSaveSlotFlag:
 tst.b d0
 bne.s @Slot2
 bclr #0,(SAVE_FLAGS).l
 bclr #1,(SAVE_FLAGS).l
 ; End of function ClearSaveSlotFlag
"""
    witch = """\
witchMenuAction_Load:
 bsr.w LoadGame
 chkFlg 88
 beq.s @loc_18
 jsr j_BattleLoop
 clr.w d0
 jsr GetSavepointForMap(pc)
 ; End of function witchMenuAction_Load
"""
    jump = """\
j_BattleLoop:
 jmp BattleLoop(pc)
 ; End of function j_BattleLoop
"""
    return sram, witch, jump


def _fixture() -> dict[str, object]:
    return load_json(FIXTURE)


def _canonical_digest(value: object) -> str:
    encoded = dumps(value, ensure_ascii=False, separators=(",", ":"), sort_keys=True)
    return sha256(encoded.encode("utf-8")).hexdigest()


def _lua_function(source: str, name: str) -> str:
    match = re.search(
        rf"^local function {re.escape(name)}\([^)]*\)\n(?P<body>.*?)^end$",
        source,
        re.MULTILINE | re.DOTALL,
    )
    assert match, f"missing Lua function {name}"
    return match.group("body")


def test_witch_save_fixture_schema_locks_complete_ordered_semantic_object() -> None:
    fixture = _fixture()
    schema = load_json(SCHEMA)
    validate_json(fixture, SCHEMA, owner="witch save actions fixture")

    properties = schema["properties"]
    assert fixture["cases"] == properties["cases"]["allOf"][1]["const"]
    assert (
        fixture["expectedObservation"]
        == properties["expectedObservation"]["allOf"][1]["const"]
    )
    assert fixture["runtimeQuestions"] == properties["runtimeQuestions"]["allOf"][1]["const"]
    assert fixture["runtimeQuestions"] == [
        "witch-save-actions/cross-process-persistence-and-recovery",
        "witch-save-menu-suspend/presentation-and-input-timing",
    ]
    assert [case["id"] for case in fixture["cases"]["directService"]] == [
        "save-slot1-source",
        "save-slot2-source",
        "load-slot1",
        "load-slot2",
        "copy-slot1-to-slot2",
        "restore-slot2-source",
        "copy-slot2-to-slot1",
        "delete-slot1",
        "delete-slot2",
    ]
    assert [case["id"] for case in fixture["cases"]["loadControlFlow"]] == [
        "normal",
        "suspend",
    ]
    for definition in schema["definitions"].values():
        assert definition["additionalProperties"] is False


def test_witch_save_handoff_removes_only_the_promoted_question() -> None:
    fixture_siblings = deepcopy(_fixture())
    removed_questions = fixture_siblings.pop("runtimeQuestions")
    assert removed_questions == [
        "witch-save-actions/cross-process-persistence-and-recovery",
        "witch-save-menu-suspend/presentation-and-input-timing",
    ]
    assert _canonical_digest(fixture_siblings) == (
        "6a26e60c0aa7b3bc4460485473a963f39a90296e085bbd315bd11170a3d50a56"
    )

    schema_siblings = deepcopy(load_json(SCHEMA))
    del schema_siblings["properties"]["runtimeQuestions"]
    schema_siblings["required"].remove("runtimeQuestions")
    assert _canonical_digest(schema_siblings) == (
        "5988cc076ff5a81cfb09e5130cfaa30289bcc3a869d963313348c0863865a3c6"
    )
    assert _canonical_digest(schema_siblings["definitions"]) == (
        "417db7d440dd77ec479b49ffba6d79827d74ff798e3e03e735c25672ce908903"
    )


@pytest.mark.parametrize(
    ("name", "mutation"),
    [
        (
            "missing-required-nested-field",
            lambda value: value["expectedObservation"]["directServiceResults"][0][
                "storedPayloadSamples"
            ][0].pop("physicalAddress"),
        ),
        (
            "renamed-required-nested-field",
            lambda value: value["expectedObservation"]["directServiceResults"][0][
                "storedPayloadSamples"
            ][0].update(
                {
                    "physical_address": value["expectedObservation"][
                        "directServiceResults"
                    ][0]["storedPayloadSamples"][0].pop("physicalAddress")
                }
            ),
        ),
        (
            "extra-nested-field",
            lambda value: value["expectedObservation"]["directServiceResults"][0][
                "restoredPayloadSamples"
            ][0].update({"unexpected": True}),
        ),
        (
            "reordered-direct-case",
            lambda value: value["cases"]["directService"].reverse(),
        ),
        (
            "out-of-bound-offset",
            lambda value: value["expectedObservation"]["directServiceResults"][0][
                "storedPayloadSamples"
            ][0].update({"logicalOffset": 4016}),
        ),
    ],
)
def test_witch_save_schema_rejects_nested_and_order_mutations(
    name: str, mutation: object
) -> None:
    fixture = deepcopy(_fixture())
    mutation(fixture)
    with pytest.raises(ValueError, match="failed schema validation"):
        validate_json(fixture, SCHEMA, owner=name)


def test_witch_save_source_guard_rejects_use_site_mutations_before_fixture() -> None:
    sram, witch, jump = _sources()
    assert _source_use_sites(sram, witch, jump)["copy"][1]["operand"] == "#1,d0"

    with pytest.raises(ValueError, match="CopySave"):
        _source_use_sites(sram.replace("eori.w #1,d0", "eori.w #2,d0"), witch, jump)
    with pytest.raises(ValueError, match="CopySave"):
        _source_use_sites(
            sram.replace(
                "eori.w #1,d0\n andi.w #1,d0", "andi.w #1,d0\n eori.w #1,d0"
            ),
            witch,
            jump,
        )
    with pytest.raises(ValueError, match="ClearSaveSlotFlag"):
        _source_use_sites(
            sram.replace("bclr #1,(SAVE_FLAGS).l", "bclr #0,(SAVE_FLAGS).l"),
            witch,
            jump,
        )
    with pytest.raises(ValueError, match="witchMenuAction_Load"):
        _source_use_sites(sram, witch.replace("beq.s @loc_18", "bne.s @loc_18"), jump)
    with pytest.raises(ValueError, match="j_BattleLoop"):
        _source_use_sites(sram, witch, jump.replace("jmp BattleLoop(pc)", "jsr BattleLoop(pc)"))


def test_witch_save_source_parser_ignores_comments_labels_and_near_miss_operands() -> None:
    sram, witch, jump = _sources()
    sram = sram.replace(
        "CopySave:\n bsr.s LoadGame",
        "CopySave:\n"
        " fakebsr.s LoadGame\n"
        " LoadGameMention:\n"
        " move.l #LoadGame,d1\n"
        " ; bsr.s LoadGame\n"
        " bsr.s LoadGame ; executable short-suffix call",
    )
    assert _source_use_sites(sram, witch, jump)["copy"][0]["opcode"] == "bsr.s"


def test_witch_save_observer_lua_syntax_preflight() -> None:
    _, executable = bizhawk_contract()
    validate_lua_syntax(OBSERVER, executable)


def test_witch_save_observer_normal_return_advances_to_suspend_case() -> None:
    fixture = _fixture()
    control_cases = fixture["cases"]["loadControlFlow"]
    assert control_cases == [
        {"id": "normal", "flag88Set": False},
        {"id": "suspend", "flag88Set": True},
    ]
    source = OBSERVER.read_text(encoding="utf-8")
    start_control = _lua_function(source, "start_control_case")
    advance = _lua_function(source, "advance_after_normal_return")
    assert start_control.index('phase = "load-control"') < start_control.index("active =")
    assert advance.index("complete_control_case(") < advance.index("start_control_case()")
    assert "control_index <= #config.cases.loadControlFlow" in advance


def test_witch_save_physical_window_base_stays_distinct_from_first_stored_byte() -> None:
    fixture = _fixture()
    source_contract = build_witch_save_actions_source_contract(
        repo_path("local/upstream/SF2DISASM")
    )
    storage = source_contract["storage"]
    assert storage == fixture["storage"]
    assert "saveFlagsAddress" not in fixture["ram"]
    assert storage["saveFlagsAddress"] == 2105397
    assert storage["physicalWindowBaseAddress"] == storage["firstStoredPhysicalByteAddress"] - 1
    assert (
        storage["physicalAddressIntervalPerSlot"]
        == storage["logicalPayloadByteCountPerSlot"]
        * storage["physicalAddressStepPerLogicalByte"]
    )
    mapping = _lua_function(OBSERVER.read_text(encoding="utf-8"), "sram_domain_offset")
    assert "config.storage.physicalWindowBaseAddress" in mapping
    assert "firstStoredPhysicalByteAddress" not in mapping
    observer = OBSERVER.read_text(encoding="utf-8")
    assert "config.storage.saveFlagsAddress" in observer
    assert "config.ram.saveFlagsAddress" not in observer
