from __future__ import annotations

import json
import re
from copy import deepcopy
from pathlib import Path

import pytest

from sf2tool.h2 import map3_battle01_admission as admission
from sf2tool.jsonio import load_json, validate_json


def test_parse_warps_reads_all_rows_and_ignores_comment_near_misses() -> None:
    source = """
        mWarp 9, 1
          warpMap MAP_ANCIENT_TOWER_EXTERIOR
          warpDest 4, 30
          warpFacing UP
        ; mWarp 9, 1 warpMap MAP_ANCIENT_TOWER_ENTRANCE warpDest 8, 18 warpFacing UP
        mWarp 3, 16
          warpMap MAP_GRANSEAL_CASTLE_1F
          warpDest 3, 36
          warpFacing RIGHT
        endWord
    """
    constants = {
        "MAP_ANCIENT_TOWER_EXTERIOR": 40,
        "MAP_ANCIENT_TOWER_ENTRANCE": 57,
        "MAP_GRANSEAL_CASTLE_1F": 21,
    }

    assert admission._parse_warps(source, constants, 21) == [
        {"from": [9, 1], "toMap": 40, "to": [4, 30], "facing": "UP"},
        {"from": [3, 16], "toMap": 21, "to": [3, 36], "facing": "RIGHT"},
    ]


def test_parse_warps_rejects_missing_complete_row() -> None:
    with pytest.raises(ValueError, match="warp row denominator"):
        admission._parse_warps(
            "mWarp 9, 1 warpMap MAP_ANCIENT_TOWER_EXTERIOR warpDest 4, 30 warpFacing UP",
            {"MAP_ANCIENT_TOWER_EXTERIOR": 40},
            21,
        )


def test_fixture_is_closed_public_static_contract() -> None:
    fixture = load_json(admission.FIXTURE)
    validate_json(fixture, admission.SCHEMA, owner="Map 3 Battle 01 admission fixture")
    assert admission.canonical_json_bytes(fixture) == admission.FIXTURE.read_bytes()
    assert fixture["summary"] == {
        "battleMapRowIndex": 1,
        "battleSpritesetEntries": 9,
        "beforeBattleCommands": 128,
        "extensionLogicalInputs": 46,
        "extensionRouteNodes": 48,
        "h1Fields": 47,
        "map21WarpRows": 2,
        "map40WarpRows": 2,
        "sourceFiles": 35,
    }
    static = fixture["static"]
    assert fixture["retainedR2b"]["terminal"] == {
        "map": 21,
        "player": [5, 15],
        "facing": "DOWN",
        "program": "cs_53EF4",
        "setFlags": [401, 256],
        "entityPointAfter": [6, 16],
    }
    assert static["warps"] == {
        "map21": [
            {"from": [3, 16], "toMap": 20, "to": [3, 36], "facing": "RIGHT"},
            {"from": [9, 1], "toMap": 40, "to": [4, 30], "facing": "UP"},
        ],
        "map40": [
            {"from": [255, 12], "toMap": 57, "to": [8, 18], "facing": "UP"},
            {"from": [255, 31], "toMap": 21, "to": [9, 2], "facing": "DOWN"},
        ],
    }
    route = static["extensionRoute"]
    assert route["nodeCount"] == 48 and route["inputCount"] == 46
    assert route["sha256"] == "68CBEBD2BF8A69054CCCEF7719BAFF5E1B8190B388E849BB091375DBA1D771AB"
    assert route["map40WildcardCandidates"] == [
        {"point": [14, 12], "inputCount": 28},
        {"point": [15, 12], "inputCount": 29},
    ]
    assert route["segments"][2]["to"] == [14, 12]
    assert route["segments"][3]["from"]["point"] == [14, 12]
    assert route["occupancy"] == [{"map": 21, "point": [6, 16]}, {"map": 40, "entityCount": 0}]
    spine = static["admission"]
    assert spine["mainLoop"]["orderedCalls"] == [
        "SwitchMap",
        "CheckBattle",
        "BattleLoop",
        "ExplorationLoop",
    ]
    assert spine["checkBattle"] == {
        "tableRowIndex": 1,
        "map": 57,
        "area": [0, 0, 16, 20],
        "trigger": [255, 255],
        "unlockedFlag": 401,
        "completedFlag": 501,
        "writes": ["BATTLE_AREA_X", "BATTLE_AREA_Y", "BATTLE_AREA_WIDTH", "BATTLE_AREA_HEIGHT"],
        "resultRegister": {"name": "d7", "value": 1},
    }
    assert spine["newBattle"] == {
        "suspendFlag": 88,
        "newBranchWhenClear": True,
        "currentMap": 57,
        "currentBattle": 1,
        "loadBattleD0": 0,
        "secondsCleared": True,
        "regionFlagRange": [90, 105],
        "orderedSteps": [
            "SetBaseVIntFunctions",
            "ExecuteBeforeBattleCutscene",
            "ClearBattleRegionFlags",
            "HealLivingAndImmortalAllies",
            "InitializeAllAlliesBattlePositions",
            "InitializeAllEnemiesBattlePositions",
            "ClearAiMemory",
            "LoadBattle",
            "ExecuteBattleStartCutscene",
        ],
    }
    assert spine["firstRound"]["endpoint"] == {
        "after": "GenerateBattleTurnOrder",
        "before": ["BATTLE_TURN_ORDER read", "ExecuteIndividualTurn"],
        "notPlayerReady": True,
    }
    cutscenes = static["cutscenes"]
    assert cutscenes["beforeBattle"]["target"] == "bbcs_01"
    assert cutscenes["beforeBattle"]["program"]["address"] == 0x494BC
    assert len(cutscenes["beforeBattle"]["program"]["commands"]) == 128
    assert re.fullmatch(r"[0-9A-F]{64}", cutscenes["beforeBattle"]["program"]["sha256"])
    assert cutscenes["battleStart"] == {
        "tableRow": 1,
        "target": "ms_Empty",
        "introFlag": 451,
        "setsIntroFlagBeforeProgram": True,
    }
    assert cutscenes["spriteset"] == {
        "address": 0x1B32E2,
        "counts": [3, 6, 3, 0],
        "entryCount": 9,
        "allStarting": True,
        "sha256": "CDB59BA7FEEE381EE4517FFF0CB947D1464A93EEB536676A77C1DE90BCFD0955",
    }
    assert cutscenes["regionCutscenes"]["battle01Rows"] == 0
    assert static["loadAndTurnOrder"]["loadBattle"] == {
        "address": 0x25610,
        "currentMap": 57,
        "terrain": {"symbol": "BattleTerrain01", "address": 0x1AD344},
        "orderedSteps": [
            "LoadCurrentMap",
            "FadeOutToBlackAll",
            "LoadMapTilesets",
            "WaitForFadeToFinish",
            "ClearVIntFunctions",
            "WaitForVInt",
            "PositionBattleEntities",
            "InitializeSprites",
            "LoadMap",
            "WaitForVInt",
            "LoadEntityMapsprites",
            "SetBaseVIntFunctions",
            "LoadBattleTerrainData",
            "PlayMapMusic",
            "FadeInFromBlack",
        ],
    }
    assert static["loadAndTurnOrder"]["turnOrder"] == {
        "entryBytes": 2,
        "clearEntries": 64,
        "allyCandidates": [0, 29],
        "enemyCandidates": [128, 157],
        "eligibility": ["placed", "living"],
        "randomizedAgi": True,
        "sortsDescending": True,
        "currentBattleTurn": 0,
    }


def test_schema_and_source_h1_rom_mutations_fail_closed() -> None:
    fixture = load_json(admission.FIXTURE)
    schema = load_json(admission.SCHEMA)

    def assert_closed(value: object) -> None:
        if isinstance(value, dict):
            if value.get("type") == "object":
                assert value["additionalProperties"] is False
            for nested in value.values():
                assert_closed(nested)
        elif isinstance(value, list):
            for nested in value:
                assert_closed(nested)

    assert_closed(schema["$defs"]["fixture"])
    missing = deepcopy(fixture)
    del missing["static"]["extensionRoute"]
    with pytest.raises(ValueError, match="validation"):
        validate_json(missing, admission.SCHEMA, owner="missing route")
    private = deepcopy(fixture)
    private["static"]["runtimeObservation"] = {"playerReady": True}
    with pytest.raises(ValueError, match="validation"):
        validate_json(private, admission.SCHEMA, owner="private runtime")
    changed = deepcopy(fixture)
    changed["static"]["cutscenes"]["beforeBattle"]["program"]["sha256"] = "0" * 64
    with pytest.raises(ValueError, match="validation"):
        validate_json(changed, admission.SCHEMA, owner="digest drift")
    with pytest.raises(ValueError, match="source-use drift"):
        admission._require_order("second\nfirst", ("first", "second"), "near miss")

    end = max(
        address + width
        for _, address, width in [
            *((name, address, 2) for name, address in admission._FUNCTIONS.items()),
            *admission._TABLE_SPANS,
        ]
    )
    h1 = bytes(end)
    assert len(admission._h1_projection(h1, h1)) == 47
    mismatched_rom = bytearray(h1)
    mismatched_rom[admission._TABLE_SPANS[0][1]] = 1
    with pytest.raises(ValueError, match="H1/ROM drift"):
        admission._h1_projection(h1, bytes(mismatched_rom))
    with pytest.raises(ValueError, match="H1 span is incomplete"):
        admission._h1_projection(b"", b"")


def test_route_occupancy_retained_projection_and_private_cutscene_prose_guards(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    controller = {
        "collisionMask": 0xC000,
        "rightStairMask": 0x8000,
        "leftStairMask": 0x4000,
        "stairWordDeltas": [-3, 3, 5, -5],
    }
    surface = {"width": 3, "layout": [0] * 9, "area": (0, 0, 2, 0)}
    assert admission._shortest(surface, (0, 0), (2, 0), controller)[1] == ["Right", "Right"]
    with pytest.raises(ValueError, match="static route is blocked"):
        admission._shortest(surface, (0, 0), (2, 0), controller, frozenset({(1, 0)}))
    surface["layout"][1] = 0xC000
    with pytest.raises(ValueError, match="static route is blocked"):
        admission._shortest(surface, (0, 0), (2, 0), controller)

    event_surface = {
        "width": 3,
        "layout": [0, 0, 0, 0, 0x1000, 0x1400, 0, 0, 0],
        "area": (0, 0, 2, 2),
    }
    assert admission._warp_trigger_points(event_surface, (0xFF, 1)) == [(1, 1)]

    region_rows = """table_BattleRegionCutscenes:
dc.b BATTLE_TO_MOUN
dc.b 0
dc.w 386
dc.l rbcs_battle32
dc.b BATTLE_TO_MOUN
dc.b 1
dc.w 386
dc.l rbcs_battle32
dc.b BATTLE_VERSUS_ODD_EYE
dc.b 0
dc.w 387
dc.l rbcs_battle40_1
dc.b BATTLE_VERSUS_ODD_EYE
dc.b 1
dc.w 388
dc.l rbcs_battle40_2
"""
    assert admission._region_table(region_rows)["battle01Rows"] == 0
    with pytest.raises(ValueError, match="unexpected Battle01"):
        admission._region_table(
            region_rows.replace("BATTLE_TO_MOUN", "BATTLE_INSIDE_ANCIENT_TOWER", 1)
        )

    retained = load_json(Path("tests/fixtures/h2/map3-castle-battle-unlock-static-v1.json"))
    drifted = deepcopy(retained)
    drifted["static"]["routeGraph"]["segments"][-1]["player"] = [5, 14]
    monkeypatch.setattr(admission, "build_map3_castle_battle_unlock_static", lambda *_: drifted)
    with pytest.raises(ValueError, match="retained R2b fixture projection drift"):
        admission._retained_r2b(Path("unused-rom.bin"), Path("unused-upstream"))

    macros = {"textCursor": (4, ()), "csc_end": (65535, ())}
    source = "bbcs_01:\n textCursor 2292 ; private prose\n csc_end\n"
    assert admission._before_battle_commands(source, macros) == admission._before_battle_commands(
        source.replace("private prose", "different private prose"), macros
    )


def _turn_order_source_inputs(
    load_battle: str | None = None, turn_order: str | None = None
) -> dict[str, str]:
    return {
        "code/gameflow/battle/battlefunctions/loadBattle.asm": load_battle
        or """
move.b ((CURRENT_MAP-$1000000)).w,d1
bsr.w FadeOutToBlackAll
jsr (LoadMapTilesets).w
bsr.w WaitForFadeToFinish
trap #VINT_FUNCTIONS
dc.w VINTS_CLEAR
jsr (WaitForVInt).w
jsr j_PositionBattleEntities
jsr (InitializeSprites).w
jsr (LoadMap).w
jsr (WaitForVInt).w
jsr (LoadEntityMapsprites).w
bsr.w SetBaseVIntFunctions
jsr j_LoadBattleTerrainData
jsr (PlayMapMusic).w
jsr (FadeInFromBlack).w
""",
        "code/gameflow/battle/battleloop/loadbattleterraindata.asm": """
LoadBattleTerrainData:
lea pt_BattleTerrainData(pc),a0
move.b (a1),d1
lsl.l #2,d1
movea.l (a0,d1.w),a0
jsr (LoadStackCompressedData).w
""",
        "data/battles/terrainentries.asm": "pt_BattleTerrainData:\ndc.l BattleTerrain01\n",
        "code/gameflow/battle/battleloop/turnorderfunctions.asm": turn_order
        or """
GenerateBattleTurnOrder:
lea ((BATTLE_TURN_ORDER-$1000000)).w,a0
moveq #TURN_ORDER_ENTRIES_COUNTER,d7
move.w #-1,(a0)+
dbf d7,@ClearTurnOrder_Loop
moveq #COMBATANT_ALLIES_COUNTER,d7
bsr.w AddCombatantAndRandomizedAgiToTurnOrder
dbf d7,@AddAllyTurns_Loop
move.w #COMBATANT_ENEMIES_START,d0
moveq #BATTLE_ENEMY_ENTITIES_COUNTER,d7
bsr.w AddCombatantAndRandomizedAgiToTurnOrder
dbf d7,@AddEnemyTurns_Loop
moveq #COMBATANTS_ALL_COUNTER,d6
moveq #TURN_ORDER_ENTRIES_MINUS_ONE_COUNTER,d7
move.w (a0),d0
move.w TURN_ORDER_ENTRY_SIZE(a0),d1
cmp.b d0,d1
ble.s @InOrder
move.w d1,(a0)
move.w d0,TURN_ORDER_ENTRY_SIZE(a0)
addq.l #TURN_ORDER_ENTRY_SIZE,a0
dbf d7,@SortCombatants_InnerLoop
dbf d6,@SortCombatants_OuterLoop
clr.b ((CURRENT_BATTLE_TURN-$1000000)).w
AddCombatantAndRandomizedAgiToTurnOrder:
jsr j_GetCombatantX
tst.b d1
bmi.w @Return
jsr j_GetCurrentHp
tst.w d1
beq.w @Return
jsr j_GetCurrentAgi
move.w d1,d3
andi.w #TURN_AGILITY_MASK,d1
move.w d1,d6
lsr.w #3,d6
jsr (GenerateRandomNumber).w
add.w d7,d1
jsr (GenerateRandomNumber).w
sub.w d7,d1
moveq #3,d6
jsr (GenerateRandomNumber).w
subq.w #1,d7
add.w d7,d1
move.b d0,(a0)+
""",
    }


def test_source_guards_reject_lifecycle_turn_order_and_cutscene_mutations() -> None:
    constants = {
        "TURN_ORDER_ENTRY_SIZE": 2,
        "TURN_ORDER_ENTRIES_COUNTER": 63,
        "COMBATANT_ALLIES_COUNTER": 29,
        "COMBATANT_ENEMIES_START": 128,
        "BATTLE_ENEMY_ENTITIES_COUNTER": 29,
    }
    text = _turn_order_source_inputs()
    result = admission._load_battle_and_turn_order(constants, text)
    assert result["turnOrder"]["currentBattleTurn"] == 0
    assert result["loadBattle"]["orderedSteps"].count("WaitForVInt") == 2

    lifecycle_mutations = (
        ("bsr.w WaitForFadeToFinish", "bsr.w WaitForOtherFade"),
        (
            "trap #VINT_FUNCTIONS\ndc.w VINTS_CLEAR",
            "dc.w VINTS_CLEAR\ntrap #VINT_FUNCTIONS",
        ),
        (
            "jsr (WaitForVInt).w\njsr (LoadEntityMapsprites).w",
            "jsr (LoadEntityMapsprites).w",
        ),
        (
            "bsr.w SetBaseVIntFunctions\njsr j_LoadBattleTerrainData",
            "jsr j_LoadBattleTerrainData\nbsr.w SetBaseVIntFunctions",
        ),
    )
    for old, new in lifecycle_mutations:
        mutated = dict(text)
        source = mutated["code/gameflow/battle/battlefunctions/loadBattle.asm"]
        mutated["code/gameflow/battle/battlefunctions/loadBattle.asm"] = source.replace(old, new)
        with pytest.raises(ValueError, match="source-use drift"):
            admission._load_battle_and_turn_order(constants, mutated)

    turn_mutations = (
        ("ble.s @InOrder", "bge.s @InOrder"),
        ("move.w d1,(a0)", "move.w d0,(a0)"),
        ("dbf d6,@SortCombatants_OuterLoop", "bra.s @SortCombatants_OuterLoop"),
        ("jsr (GenerateRandomNumber).w\nadd.w d7,d1", "add.w d7,d1"),
        ("bmi.w @Return", "bpl.w @Return"),
        (
            "clr.b ((CURRENT_BATTLE_TURN-$1000000)).w",
            "clr.b ((CURRENT_OTHER_TURN-$1000000)).w",
        ),
    )
    for old, new in turn_mutations:
        mutated = dict(text)
        source = mutated["code/gameflow/battle/battleloop/turnorderfunctions.asm"]
        mutated["code/gameflow/battle/battleloop/turnorderfunctions.asm"] = source.replace(
            old, new, 1
        )
        with pytest.raises(ValueError, match="source-use drift"):
            admission._load_battle_and_turn_order(constants, mutated)

    before_rows = "\n".join(
        "dc.w bbcs_01-rpt_BeforeBattleCutscenes"
        if index == 1
        else "dc.w other-rpt_BeforeBattleCutscenes"
        for index in range(45)
    )
    start_rows = "\n".join(
        "dc.w ms_Empty-rpt_BattleStartCutscenes"
        if index == 1
        else "dc.w other-rpt_BattleStartCutscenes"
        for index in range(45)
    )
    before = "rpt_BeforeBattleCutscenes:\n" + before_rows
    start = "rpt_BattleStartCutscenes:\n" + start_rows
    assert admission._battle01_cutscene_targets(before, start) == ("bbcs_01", "ms_Empty")
    with pytest.raises(ValueError, match="cutscene table target drift"):
        admission._battle01_cutscene_targets(before.replace("bbcs_01", "ms_Empty"), start)

    check_battle = """
move.w #BATTLE_UNLOCKED_FLAGS_START,d1
add.w d7,d1
jsr j_CheckFlag
addi.w #BATTLE_UNLOCKED_TO_COMPLETED_FLAGS_OFFSET,d1
jsr j_CheckFlag
subi.w #BATTLE_UNLOCKED_TO_COMPLETED_FLAGS_OFFSET,d1
jsr j_ClearFlag
"""
    expected_flags = (
        "move.w #BATTLE_UNLOCKED_FLAGS_START,d1",
        "add.w d7,d1",
        "jsr j_CheckFlag",
        "addi.w #BATTLE_UNLOCKED_TO_COMPLETED_FLAGS_OFFSET,d1",
        "jsr j_CheckFlag",
        "subi.w #BATTLE_UNLOCKED_TO_COMPLETED_FLAGS_OFFSET,d1",
        "jsr j_ClearFlag",
    )
    admission._require_order(check_battle, expected_flags, "synthetic F401/F501")
    with pytest.raises(ValueError, match="source-use drift"):
        admission._require_order(
            check_battle.replace("addi.w", "subi.w", 1),
            expected_flags,
            "synthetic F401/F501",
        )

    new_battle_and_endpoint = """
chkFlg 88
beq.s @Initialize
bsr.w HealLivingAndImmortalAllies
jsr j_InitializeAllAlliesBattlePositions
jsr j_InitializeAllEnemiesBattlePositions
clr.w d0
bsr.w LoadBattle
bsr.w GenerateBattleTurnOrder
move.b (a0,d0.w),d0
bsr.w ExecuteIndividualTurn
"""
    expected_admission = (
        "chkFlg 88",
        "beq.s @Initialize",
        "bsr.w HealLivingAndImmortalAllies",
        "jsr j_InitializeAllAlliesBattlePositions",
        "jsr j_InitializeAllEnemiesBattlePositions",
        "clr.w d0",
        "bsr.w LoadBattle",
        "bsr.w GenerateBattleTurnOrder",
        "move.b (a0,d0.w),d0",
        "bsr.w ExecuteIndividualTurn",
    )
    admission._require_order(
        new_battle_and_endpoint, expected_admission, "synthetic F88/d0 endpoint"
    )
    with pytest.raises(ValueError, match="source-use drift"):
        admission._require_order(
            new_battle_and_endpoint.replace("clr.w d0", "moveq #1,d0"),
            expected_admission,
            "synthetic F88/d0 endpoint",
        )

    battle_start_route = """
addi.w #BATTLE_INTRO_CUTSCENE_FLAGS_START,d1
jsr j_CheckFlag
jsr j_SetFlag
move.w rpt_BattleStartCutscenes(pc,d0.w),d0
bsr.w ExecuteMapScript
"""
    expected_start = (
        "addi.w #BATTLE_INTRO_CUTSCENE_FLAGS_START,d1",
        "jsr j_CheckFlag",
        "jsr j_SetFlag",
        "move.w rpt_BattleStartCutscenes(pc,d0.w),d0",
        "bsr.w ExecuteMapScript",
    )
    admission._require_order(battle_start_route, expected_start, "synthetic F451 route")
    with pytest.raises(ValueError, match="source-use drift"):
        admission._require_order(
            battle_start_route.replace("jsr j_SetFlag", "jsr j_ClearFlag"),
            expected_start,
            "synthetic F451 route",
        )


def test_research_index_adds_exactly_the_18_admission_objects() -> None:
    expected = {
        "gameflow.main-loop": "sourceContext.functionAddresses.MainLoop",
        "maps.battle-trigger": "sourceContext.functionAddresses.CheckBattle",
        "battle.data.map-coordinates": "sourceContext.functionAddresses.table_BattleMapCoordinates",
        "battle.control.main-loop": "sourceContext.functionAddresses.BattleLoop",
        "battle.cutscene.before-battle": (
            "sourceContext.functionAddresses.ExecuteBeforeBattleCutscene"
        ),
        "battle.routing.before-battle": "sourceContext.functionAddresses.rpt_BeforeBattleCutscenes",
        "battle.cutscene.data.battle01.beforebattle": "sourceContext.functionAddresses.bbcs_01",
        "battle.functions.load-battle": "sourceContext.functionAddresses.LoadBattle",
        "battle.spriteset.data.slot-01": "sourceContext.functionAddresses.BattleSpriteset01",
        "battle.cutscene.battle-start": (
            "sourceContext.functionAddresses.ExecuteBattleStartCutscene"
        ),
        "battle.routing.battle-start": "sourceContext.functionAddresses.rpt_BattleStartCutscenes",
        "scripting.map.ms-empty": "sourceContext.functionAddresses.ms_Empty",
        "battle.activation.activate-enemies": "sourceContext.functionAddresses.ActivateEnemies",
        "battle.cutscene.region": "sourceContext.functionAddresses.ExecuteBattleRegionCutscene",
        "battle.routing.region-activated": (
            "sourceContext.functionAddresses.table_BattleRegionCutscenes"
        ),
        "battle.loop.populate-spawns": (
            "sourceContext.functionAddresses.PopulateTargetsListWithSpawningEnemies"
        ),
        "battle.turn-order.generate": "sourceContext.functionAddresses.GenerateBattleTurnOrder",
        "battle.functions.execute-turn": "sourceContext.functionAddresses.ExecuteIndividualTurn",
    }
    index = json.loads(Path("manifests/research-index.json").read_text(encoding="utf-8"))
    objects = [
        (record, evidence)
        for record in index["records"]
        for evidence in record["evidence"]
        if evidence["fixtureId"] == admission.ID
    ]
    assert len(objects) == 18
    assert {record["id"] for record, _ in objects} == set(expected)
    for record, evidence in objects:
        assert evidence == {
            "level": "H2",
            "fixture": "tests/fixtures/h2/map3-battle01-admission-static-v1.json",
            "fixtureId": admission.ID,
            "verifier": "src/sf2tool/h2/map3_battle01_admission.py",
            "bindings": [{"addressId": "entry", "fixtureField": expected[record["id"]]}],
        }
        assert "docs/research/map3-battle01-admission.md" in record["documents"]
