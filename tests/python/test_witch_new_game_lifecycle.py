from __future__ import annotations

from copy import deepcopy

import pytest

from sf2tool.h3.bizhawk import bizhawk_contract, validate_lua_syntax
from sf2tool.h3.witch_new_game_lifecycle import (
    FIXTURE,
    FIXTURE_SCHEMA,
    OBSERVER,
    OUTPUT_SCHEMA,
    _source_use_sites,
    build_witch_new_game_lifecycle_source_contract,
)
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path


def _fixture() -> dict[str, object]:
    return load_json(FIXTURE)


def _sources() -> tuple[str, str, str, str, str, str]:
    witch = """\
witchMenuAction_New:
 move.b (SAVE_FLAGS).l,d2
 andi.w #3,d2
 eori.w #3,d2
 lsl.w #1,d2
 btst #1,d2
 beq.s @loc_8
 moveq #1,d0
 bra.s @loc_9
@loc_8:
 moveq #2,d0
@loc_9:
 moveq #1,d1
 jsr j_ExecuteWitchMainMenu
 tst.w d0
 bmi.s byte_73C2
 subq.w #1,d0
 move.w d0,((CURRENT_SAVE_SLOT-$1000000)).w
 jsr j_NewGame
 clr.w d0
 jsr j_NameAlly
 bsr.w CheatModeConfiguration
 clr.w d0
 moveq #3,d1
 moveq #%1111,d2
 jsr j_ExecuteWitchMainMenu
 tst.w d0
 bpl.s @loc_13
 clr.w d0
@loc_13:
 btst #0,d0
 beq.s @loc_14
 setFlg 78
@loc_14:
 btst #1,d0
 beq.s @loc_15
 setFlg 79
@loc_15:
 addi.w #233,d0
 bsr.w DisplayText
 move.w ((CURRENT_SAVE_SLOT-$1000000)).w,d0
 move.b #GAMESTART_MAP,((CURRENT_MAP-$1000000)).w
 move.b #GAMESTART_MAP,((EGRESS_MAP-$1000000)).w
 bsr.w SaveGame
 move.b #GAMESTART_MAP,d0
 move.w #GAMESTART_SAVEPOINT_X,d1
 move.w #GAMESTART_SAVEPOINT_Y,d2
 move.w #GAMESTART_FACING,d3
 moveq #1,d4
 bra.w MainLoop
 ; End of function witchMenuAction_New
"""
    settings = """\
InitializeGameSettings:
 moveq #0,d0
 move.b d0,((CURRENT_MAP-$1000000)).w
 move.b d0,((EGRESS_MAP-$1000000)).w
 ; End of function InitializeGameSettings
"""
    configuration = """\
CheatModeConfiguration:
 btst #INPUT_BIT_START,((PLAYER_1_INPUT-$1000000)).w
 beq.w @Return
 ; End of function CheatModeConfiguration
"""
    new_alias = "j_NewGame:\n jmp NewGame(pc)\n ; End of function j_NewGame\n"
    name_alias = "j_NameAlly:\n jmp NameAlly(pc)\n ; End of function j_NameAlly\n"
    menu_alias = (
        "j_ExecuteWitchMainMenu:\n jmp ExecuteWitchMainMenu(pc)\n"
        " ; End of function j_ExecuteWitchMainMenu\n"
    )
    return witch, settings, configuration, new_alias, name_alias, menu_alias


def test_witch_new_source_guard_rejects_operand_order_and_alias_mutations() -> None:
    sources = _sources()
    assert _source_use_sites(*sources)["newAction"][3]["operand"] == "#1,d2"

    with pytest.raises(ValueError, match="witchMenuAction_New"):
        _source_use_sites(sources[0].replace("eori.w #3,d2", "eori.w #2,d2"), *sources[1:])
    with pytest.raises(ValueError, match="witchMenuAction_New"):
        _source_use_sites(sources[0].replace("bpl.s @loc_13", "bmi.s @loc_13"), *sources[1:])
    with pytest.raises(ValueError, match="witchMenuAction_New"):
        _source_use_sites(
            sources[0].replace(
                "move.b #GAMESTART_MAP,((CURRENT_MAP-$1000000)).w\n"
                " move.b #GAMESTART_MAP,((EGRESS_MAP-$1000000)).w",
                "move.b #GAMESTART_MAP,((EGRESS_MAP-$1000000)).w\n"
                " move.b #GAMESTART_MAP,((CURRENT_MAP-$1000000)).w",
            ),
            *sources[1:],
        )
    with pytest.raises(ValueError, match="j_NewGame"):
        _source_use_sites(*sources[:3], sources[3].replace("jmp", "jsr"), *sources[4:])
    with pytest.raises(ValueError, match="CheatModeConfiguration"):
        _source_use_sites(
            *sources[:2],
            sources[2].replace("beq.w @Return", "bne.w @Return"),
            *sources[3:],
        )


@pytest.mark.parametrize(
    ("before", "after"),
    (
        ("andi.w #3,d2", "andi.w #2,d2"),
        ("eori.w #3,d2", "eori.w #2,d2"),
        ("lsl.w #1,d2", "lsl.w #2,d2"),
        ("subq.w #1,d0", "subq.w #2,d0"),
        ("moveq #1,d1\n jsr j_ExecuteWitchMainMenu", "moveq #2,d1\n jsr j_ExecuteWitchMainMenu"),
        ("moveq #3,d1\n moveq #%1111,d2", "moveq #2,d1\n moveq #%1111,d2"),
        ("moveq #%1111,d2", "moveq #%0111,d2"),
        ("setFlg 78", "setFlg 77"),
        ("setFlg 79", "setFlg 80"),
        ("move.b #GAMESTART_MAP,d0", "move.b #GAMESTART_SAVEPOINT_Y,d0"),
        ("move.w #GAMESTART_SAVEPOINT_X,d1", "move.w #GAMESTART_FACING,d1"),
        ("move.w #GAMESTART_SAVEPOINT_Y,d2", "move.w #GAMESTART_MAP,d2"),
        ("move.w #GAMESTART_FACING,d3", "move.w #GAMESTART_MAP,d3"),
        ("moveq #1,d4", "moveq #2,d4"),
    ),
)
def test_witch_new_source_constructor_rejects_each_summary_operand_mutation(
    before: str, after: str
) -> None:
    sources = _sources()
    with pytest.raises(ValueError, match="witchMenuAction_New"):
        _source_use_sites(sources[0].replace(before, after), *sources[1:])


def test_witch_new_parser_ignores_comments_labels_and_near_miss_operands() -> None:
    sources = _sources()
    witch = sources[0].replace(
        "moveq #1,d1\n jsr j_ExecuteWitchMainMenu",
        "moveq #1,d1\n"
        " fakejsr j_ExecuteWitchMainMenu\n"
        " j_ExecuteWitchMainMenuMention:\n"
        " move.l #j_ExecuteWitchMainMenu,d7\n"
        " ; jsr j_ExecuteWitchMainMenu\n"
        " jsr j_ExecuteWitchMainMenu ; actual injected seam",
    )
    assert _source_use_sites(witch, *sources[1:])["newAction"][10]["opcode"] == "jsr"


def _sample(logical_offset: int, physical_address: int, stored_byte: int) -> dict[str, int]:
    return {
        "logicalOffset": logical_offset,
        "physicalAddress": physical_address,
        "storedPhysicalByte": stored_byte,
    }


def _saved_payload_samples(selector: int) -> list[dict[str, int]]:
    if selector == 0:
        return [
            _sample(0, 2097329, 66),
            _sample(1, 2097331, 79),
            _sample(2007, 2101343, 0),
            _sample(4015, 2105359, 0),
        ]
    assert selector == 1
    return [
        _sample(0, 2105403, 66),
        _sample(1, 2105405, 79),
        _sample(2007, 2109417, 0),
        _sample(4015, 2113433, 0),
    ]


def _runtime_record(
    *,
    case_id: str,
    precondition_save_flags: int,
    initial_selector: int,
    initial_availability: int,
    initial_return: int,
    difficulty_return: int,
    current_save_slot: int,
    flag_78_set: bool,
    flag_79_set: bool,
    saved_selector: int,
    save_flags_byte: int,
    checksum_byte: int,
) -> dict[str, object]:
    return {
        "id": case_id,
        "preconditionSaveFlags": precondition_save_flags,
        "initialMenu": {
            "observedInitialSelector": initial_selector,
            "observedPage": 1,
            "observedAvailability": initial_availability,
            "injectedReturn": initial_return,
        },
        "difficultyMenu": {
            "observedSelector": 0,
            "observedPage": 3,
            "observedAvailability": 15,
            "injectedReturn": difficulty_return,
        },
        "seams": {
            "initialMenuAliasBypassed": True,
            "difficultyMenuAliasBypassed": True,
            "nameAllyAliasBypassed": True,
            "cheatModeConfigurationExecuted": True,
            "displayTextBypassCalls": 5,
            "newGameAliasExecuted": True,
            "newGameEffectiveTargetExecuted": True,
        },
        "handoff": {
            "currentSaveSlot": current_save_slot,
            "currentMap": 3,
            "egressMap": 3,
            "d0": 3,
            "d1": 56,
            "d2": 3,
            "d3": 3,
            "d4": 1,
        },
        "difficultyFlags": {"flag78Set": flag_78_set, "flag79Set": flag_79_set},
        "savedSlot": {
            "selector": saved_selector,
            "saveFlagsByte": save_flags_byte,
            "storedChecksumByte": checksum_byte,
            "computedChecksumByte": checksum_byte,
            "storedPayloadSamples": _saved_payload_samples(saved_selector),
        },
    }


def _readback(
    label: str,
    bus_after_bus_write: int,
    cart_after_bus_write: int,
    bus_after_cart_write: int,
    cart_after_cart_write: int,
) -> dict[str, object]:
    return {
        "label": label,
        "busAfterBusWrite": bus_after_bus_write,
        "cartAfterBusWrite": cart_after_bus_write,
        "busAfterCartWrite": bus_after_cart_write,
        "cartAfterCartWrite": cart_after_cart_write,
    }


def test_witch_new_fixture_matches_complete_static_and_runtime_contract() -> None:
    fixture = _fixture()
    source = build_witch_new_game_lifecycle_source_contract(repo_path("local/upstream/SF2DISASM"))
    for field in ("function", "ram", "storage", "newAction"):
        assert fixture[field] == source[field]
    assert "sourceUseSites" not in fixture
    assert {name: len(records) for name, records in source["sourceUseSites"].items()} == {
        "newAction": 44,
        "newGameReset": 3,
        "cheatMode": 2,
        "newGameAlias": 1,
        "nameAllyAlias": 1,
        "menuAlias": 1,
    }
    assert [
        (record["opcode"], record["operand"]) for record in source["sourceUseSites"]["cheatMode"]
    ] == [
        ("btst", "#input_bit_start,((player_1_input-$1000000)).w"),
        ("beq.w", "@return"),
    ]
    assert [
        records[0]["operand"]
        for records in (
            source["sourceUseSites"]["newGameAlias"],
            source["sourceUseSites"]["nameAllyAlias"],
            source["sourceUseSites"]["menuAlias"],
        )
    ] == ["newgame(pc)", "nameally(pc)", "executewitchmainmenu(pc)"]
    assert fixture["harness"] == {"maxFrames": 4800}
    assert fixture["cases"] == {
        "sampleOffsets": [0, 1, 2007, 4015],
        "matrix": [
            {
                "id": "save-flags-0-slot1-difficulty-0",
                "preconditionSaveFlags": 0,
                "injectedInitialMenuReturn": 1,
                "injectedDifficultyMenuReturn": 0,
            },
            {
                "id": "save-flags-1-slot2-difficulty-1",
                "preconditionSaveFlags": 1,
                "injectedInitialMenuReturn": 2,
                "injectedDifficultyMenuReturn": 1,
            },
            {
                "id": "save-flags-2-slot1-difficulty-2",
                "preconditionSaveFlags": 2,
                "injectedInitialMenuReturn": 1,
                "injectedDifficultyMenuReturn": 2,
            },
            {
                "id": "save-flags-0-slot1-difficulty-3",
                "preconditionSaveFlags": 0,
                "injectedInitialMenuReturn": 1,
                "injectedDifficultyMenuReturn": 3,
            },
        ],
    }
    assert fixture["expectedObservation"] == {
        "system": "GEN",
        "core": "Genesis Plus GX",
        "id": "sf2-witch-new-game-lifecycle-runtime-v1",
        "harness": {
            "checkSramReturnTrampoline": True,
            "maxFrames": 4800,
            "romPatchDomain": "MD CART",
            "textWaitHarnessControl": "C-pulse-after-new-action-entry",
            "romPatchReadbacks": [
                _readback("menu-alias-opcode", 20218, 20218, 20217, 20217),
                _readback("menu-alias-target-high", 26030, 26030, 255, 255),
                _readback("menu-alias-target-low", 20218, 20218, 26658, 26658),
                _readback("name-ally-alias", 20218, 20218, 20085, 20085),
                _readback("display-text", 18663, 18663, 20085, 20085),
                _readback("main-loop-opcode", 16952, 16952, 20217, 20217),
                _readback("main-loop-target-high", 45465, 45465, 255, 255),
                _readback("main-loop-target-low", 24832, 24832, 26690, 26690),
            ],
        },
        "records": [
            _runtime_record(
                case_id="save-flags-0-slot1-difficulty-0",
                precondition_save_flags=0,
                initial_selector=1,
                initial_availability=6,
                initial_return=1,
                difficulty_return=0,
                current_save_slot=0,
                flag_78_set=False,
                flag_79_set=False,
                saved_selector=0,
                save_flags_byte=1,
                checksum_byte=89,
            ),
            _runtime_record(
                case_id="save-flags-1-slot2-difficulty-1",
                precondition_save_flags=1,
                initial_selector=2,
                initial_availability=4,
                initial_return=2,
                difficulty_return=1,
                current_save_slot=1,
                flag_78_set=True,
                flag_79_set=False,
                saved_selector=1,
                save_flags_byte=3,
                checksum_byte=91,
            ),
            _runtime_record(
                case_id="save-flags-2-slot1-difficulty-2",
                precondition_save_flags=2,
                initial_selector=1,
                initial_availability=2,
                initial_return=1,
                difficulty_return=2,
                current_save_slot=0,
                flag_78_set=False,
                flag_79_set=True,
                saved_selector=0,
                save_flags_byte=3,
                checksum_byte=90,
            ),
            _runtime_record(
                case_id="save-flags-0-slot1-difficulty-3",
                precondition_save_flags=0,
                initial_selector=1,
                initial_availability=6,
                initial_return=1,
                difficulty_return=3,
                current_save_slot=0,
                flag_78_set=True,
                flag_79_set=True,
                saved_selector=0,
                save_flags_byte=1,
                checksum_byte=92,
            ),
        ],
    }
    assert fixture["runtimeQuestions"] == [
        "witch-save-menu/player-driven-name-entry-and-editing",
        "witch-save-menu/player-driven-menu-presentation-and-input-cadence",
    ]


@pytest.mark.parametrize(
    ("name", "schema", "mutation"),
    (
        (
            "missing nested handoff field",
            FIXTURE_SCHEMA,
            lambda fixture: fixture["expectedObservation"]["records"][0]["handoff"].pop("d4"),
        ),
        (
            "renamed nested handoff field",
            FIXTURE_SCHEMA,
            lambda fixture: fixture["expectedObservation"]["records"][0]["handoff"].update(
                {"handoffD4": fixture["expectedObservation"]["records"][0]["handoff"].pop("d4")}
            ),
        ),
        (
            "extra nested saved-slot field",
            FIXTURE_SCHEMA,
            lambda fixture: fixture["expectedObservation"]["records"][0]["savedSlot"].update(
                {"unexpected": 0}
            ),
        ),
        (
            "reordered exact matrix",
            FIXTURE_SCHEMA,
            lambda fixture: fixture["expectedObservation"]["records"].reverse(),
        ),
        (
            "out-of-bound precondition",
            FIXTURE_SCHEMA,
            lambda fixture: fixture["cases"]["matrix"][2].update({"preconditionSaveFlags": 4}),
        ),
        (
            "out-of-bound harness deadline",
            FIXTURE_SCHEMA,
            lambda fixture: fixture["harness"].update({"maxFrames": 0}),
        ),
    ),
)
def test_witch_new_fixture_schema_rejects_nested_shape_order_and_boundary_mutations(
    name: str, schema: object, mutation: object
) -> None:
    fixture = deepcopy(_fixture())
    mutation(fixture)
    with pytest.raises(ValueError, match="failed schema validation"):
        validate_json(fixture, schema, owner=name)


def test_witch_new_observation_schema_rejects_extra_and_renamed_nested_fields() -> None:
    observation = deepcopy(_fixture()["expectedObservation"])
    observation["records"][0]["initialMenu"]["extra"] = 1
    with pytest.raises(ValueError, match="failed schema validation"):
        validate_json(observation, OUTPUT_SCHEMA, owner="extra observation field")
    observation = deepcopy(_fixture()["expectedObservation"])
    observation["harness"]["romPatchReadbacks"][0]["cartWrite"] = observation["harness"][
        "romPatchReadbacks"
    ][0].pop("cartAfterCartWrite")
    with pytest.raises(ValueError, match="failed schema validation"):
        validate_json(observation, OUTPUT_SCHEMA, owner="renamed readback field")


def test_witch_new_observer_lua_syntax_and_safe_core_state_seam() -> None:
    _, executable = bizhawk_contract()
    validate_lua_syntax(OBSERVER, executable)
    source = OBSERVER.read_text(encoding="utf-8")
    verifier_source = repo_path("src/sf2tool/h3/witch_new_game_lifecycle.py").read_text(
        encoding="utf-8"
    )
    assert _fixture()["harness"] == {"maxFrames": 4800}
    assert '"MD CART"' in source
    assert "patch-readback:" in source
    assert "milestone:checkpoint-entered-before-core-state" in source
    assert source.index("memorysavestate.savecorestate()") > source.index("while true do")
    assert "observedInitialSelector" in source
    assert "memorysavestate.loadcorestate(replay_state)" in source
    assert "config.harness.maxFrames" in source
    assert '"harness": fixture["harness"]' in verifier_source
    assert "milestone:timeout:frame=" in source
    assert "client.exitCode(1)" in source
    assert "client.exitCode(0)" in source
