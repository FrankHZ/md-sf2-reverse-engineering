from __future__ import annotations

from copy import deepcopy
from pathlib import Path

import pytest

from sf2tool.h2 import map3_battle01_turn_control as turn_control
from sf2tool.jsonio import load_json, validate_json

ROOT = Path(__file__).resolve().parents[2]
ROM = ROOT / "local/roms/sf2-us.bin"
UPSTREAM = ROOT / "local/upstream/SF2DISASM"


def _source_text() -> dict[str, str]:
    return turn_control._read_source_surface(UPSTREAM / "disasm")[0]


def test_fixture_is_complete_closed_public_static_contract() -> None:
    fixture = load_json(turn_control.FIXTURE)
    assert list(fixture) == [
        "aiConstructionHandoff",
        "battle01ControlInputs",
        "commonActionConstruction",
        "controlDispatch",
        "id",
        "playerConstructionHandoff",
        "preResolutionHandoff",
        "retainedR2c",
        "romSha256",
        "schemaVersion",
        "sourceContext",
        "summary",
        "system",
        "turnOrderConsumer",
        "unknowns",
        "upstream",
    ]
    assert fixture["summary"] == {
        "battle01EnemyAssignments": 6,
        "h1RomAnchors": 27,
        "sourceFiles": 10,
        "unknowns": 15,
    }
    assert fixture["turnOrderConsumer"] == {
        "functionAddresses": {"BattleLoop": 0x23A84},
        "orderedSteps": [
            "currentBattleTurn",
            "turnOrderBase",
            "actorRead",
            "ffSentinel",
            "restartGeneration",
            "executeIndividualTurn",
        ],
        "sentinel": "FF",
    }
    assert fixture["battle01ControlInputs"]["commandsets"] == {
        "6": ["ATTACK1", "HEAL1", "SUPPORT", "MOVE1", "STAY"],
        "7": ["MOVE_ORDER1", "ATTACK1", "HEAL1", "SUPPORT", "MOVE1", "STAY"],
    }
    assert fixture["controlDispatch"]["passes"] == {
        "preparation": {
            "MUDDLE": "AI",
            "AI_CONTROLLED": "AI",
            "enemyOpponentControl": {"false": "AI", "true": "player"},
            "allyAutoBattle": {"false": "player", "true": "AI"},
        },
        "execution": {
            "MUDDLE": "AI",
            "AI_CONTROLLED": "AI",
            "enemyOpponentControl": {"false": "AI", "true": "player"},
            "allyAutoBattle": {"false": "player", "true": "AI"},
        },
    }
    assert fixture["aiConstructionHandoff"]["commandsetTraversal"] == [
        "enemyGetAiCommandsetToD5",
        "ptAiCommandsetsLookup",
        "boundedCommandLoop",
        "ExecuteAiCommand",
        "firstSuccessExit",
    ]
    assert fixture["unknowns"] == {key: "Unknown" for key in turn_control._UNKNOWN_KEYS}
    public = turn_control.canonical_json_bytes(fixture).decode("utf-8").lower()
    for forbidden in (
        "callback",
        "observation",
        "checkpoint",
        "emulator",
        "lua",
        "h3",
        "movie",
        "capture",
    ):
        assert forbidden not in public


def test_fresh_h2_derivation_matches_the_complete_fixture() -> None:
    assert turn_control.build_map3_battle01_turn_control_static(ROM, UPSTREAM) == load_json(
        turn_control.FIXTURE
    )


def test_schema_is_recursively_closed_and_rejects_boundary_mutations() -> None:
    schema = load_json(turn_control.SCHEMA)
    validate_json(load_json(turn_control.FIXTURE), turn_control.SCHEMA, owner="fixture")

    def assert_closed(value: object) -> None:
        if not isinstance(value, dict):
            return
        if value.get("type") == "object":
            assert value.get("additionalProperties") is False
        for child in value.values():
            if isinstance(child, list):
                for item in child:
                    assert_closed(item)
            else:
                assert_closed(child)

    assert_closed(schema["$defs"]["fixture"])
    fixture = load_json(turn_control.FIXTURE)
    for mutation in (
        lambda value: value["sourceContext"]["h1RomAnchors"][0].__setitem__("extra", True),
        lambda value: value["battle01ControlInputs"]["commandsets"]["6"].reverse(),
        lambda value: value["unknowns"].pop("playerReady"),
        lambda value: value["unknowns"].__setitem__("extra", "Unknown"),
    ):
        mutated = deepcopy(fixture)
        mutation(mutated)
        with pytest.raises(ValueError):
            validate_json(mutated, turn_control.SCHEMA, owner="mutation")


def test_retained_r2c_projection_drift_rejects_before_output(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    retained = load_json(turn_control.R2C_FIXTURE)
    drifted = deepcopy(retained)
    drifted["static"]["loadAndTurnOrder"]["turnOrder"]["currentBattleTurn"] = 1
    monkeypatch.setattr(turn_control, "build_map3_battle01_admission_static", lambda *_: drifted)
    with pytest.raises(ValueError, match="retained R2c fixture projection drift"):
        turn_control._retained_r2c(ROM, UPSTREAM)


def test_every_h1_rom_anchor_rejects_mutation() -> None:
    h1 = (UPSTREAM / "build/sf2build-h1.bin").read_bytes()
    rom = (ROM).read_bytes()
    for _identifier, address, _width, _end in turn_control._ANCHORS:
        for binary_name in ("H1", "ROM"):
            mutated = bytearray(h1 if binary_name == "H1" else rom)
            mutated[address] ^= 1
            with pytest.raises(ValueError, match="H1/ROM anchor drift"):
                turn_control._anchor_projection(
                    bytes(mutated) if binary_name == "H1" else h1,
                    bytes(mutated) if binary_name == "ROM" else rom,
                )
    for identifier in (
        "controlDispatch.executeIndividualTurnRange",
        "battle01ControlInputs.battleSpriteset01Range",
    ):
        _id, address, width, _end = next(
            anchor for anchor in turn_control._ANCHORS if anchor[0] == identifier
        )
        for binary_name in ("H1", "ROM"):
            mutated = bytearray(h1 if binary_name == "H1" else rom)
            mutated[address + width // 2] ^= 1
            with pytest.raises(ValueError, match=identifier):
                turn_control._anchor_projection(
                    bytes(mutated) if binary_name == "H1" else h1,
                    bytes(mutated) if binary_name == "ROM" else rom,
                )


def test_retained_r2c_drift_at_the_golden_boundary_rejects(monkeypatch: pytest.MonkeyPatch) -> None:
    fixture = load_json(turn_control.FIXTURE)
    retained = fixture["retainedR2c"]
    drifted = deepcopy(retained)
    drifted["sha256"] = "0" * 64
    projections = iter((retained, drifted))
    monkeypatch.setattr(turn_control, "_retained_r2c", lambda *_: next(projections))
    monkeypatch.setattr(turn_control, "build_map3_battle01_turn_control_static", lambda *_: fixture)
    with pytest.raises(ValueError, match="golden-boundary projection drift"):
        turn_control.verify_map3_battle01_turn_control_static(ROM, UPSTREAM)


def test_source_parser_ignores_comment_near_misses_but_rejects_control_mutations() -> None:
    text = _source_text()
    comments = deepcopy(text)
    comments["code/gameflow/battle/battleloop_1.asm"] += (
        "\n; move.b ((CURRENT_BATTLE_TURN-$1000000)).w,d0\n"
    )
    assert turn_control._validate_source_contract(comments)["turnOrderConsumer"]["sentinel"] == "FF"

    mutations = (
        ("code/gameflow/battle/battleloop_1.asm", "move.b  (a0,d0.w),d0", "move.b  (a0,d1.w),d0"),
        ("code/gameflow/battle/battleloop_1.asm", "cmpi.b  #-1,d0", "cmpi.b  #0,d0"),
        (
            "code/gameflow/battle/battleloop_1.asm",
            "bsr.w   ExecuteIndividualTurn",
            "bsr.w   MissingTurn",
        ),
        (
            "code/gameflow/battle/battlefunctions/executeindividualturn.asm",
            "bpl.s   @CheckAutoBattleCheat1",
            "bmi.s   @CheckAutoBattleCheat1",
        ),
        (
            "code/gameflow/battle/battlefunctions/executeindividualturn.asm",
            "beq.w   @Call_StartAiControl",
            "bne.w   @Call_StartAiControl",
        ),
        (
            "code/gameflow/battle/battlefunctions/executeindividualturn.asm",
            "bne.w   @Call_StartAiControl",
            "beq.w   @Call_StartAiControl",
        ),
        (
            "code/gameflow/battle/battlefunctions/executeindividualturn.asm",
            "bpl.s   @CheckAutoBattleCheat2",
            "bmi.s   @CheckAutoBattleCheat2",
        ),
        (
            "code/gameflow/battle/battlefunctions/executeindividualturn.asm",
            "beq.w   @Call_ExecuteAiControl",
            "bne.w   @Call_ExecuteAiControl",
        ),
        (
            "code/gameflow/battle/battlefunctions/executeindividualturn.asm",
            "bne.w   @Call_ExecuteAiControl",
            "beq.w   @Call_ExecuteAiControl",
        ),
        (
            "code/gameflow/battle/battlefunctions/executeindividualturn.asm",
            "bsr.w   ProcessBattleEntityControlPlayerInput",
            "bsr.w   MissingPlayerInput",
        ),
        (
            "code/gameflow/battle/battlefunctions/executeindividualturn.asm",
            "@CheckBattleaction_CastEgress:",
            "@MissingConvergence:",
        ),
        (
            "code/gameflow/battle/battlefunctions/battlefunctions_2.asm",
            "ExecuteAiControl:",
            "MissingAiExecutionEntry:",
        ),
        (
            "code/gameflow/battle/ai/startaicontrol.asm",
            "bsr.w   GetAiCommandset",
            "bsr.w   MissingAiCommandset",
        ),
        (
            "code/gameflow/battle/ai/startaicontrol.asm",
            "move.w  #AICOMMANDSET_ATTACKER1,d5",
            "move.w  #AICOMMANDSET_ATTACKER2,d5",
        ),
        ("code/gameflow/battle/ai/startaicontrol.asm", "move.w  d1,d5", "move.w  d1,d4"),
        (
            "code/gameflow/battle/ai/startaicontrol.asm",
            "lea     pt_AiCommandsets(pc), a0",
            "lea     MissingCommandsets(pc), a0",
        ),
        (
            "code/gameflow/battle/ai/startaicontrol.asm",
            "bne.s   @NextAiCommand",
            "beq.s   @NextAiCommand",
        ),
        (
            "code/gameflow/battle/ai/startaicontrol.asm",
            "dbf     d2,@HandleAiCommandset_Loop",
            "dbf     d2,@MissingAiCommandset_Loop",
        ),
        (
            "data/battles/global/aicommandsets.asm",
            "dc.l AiCommandset06     ; ATTACKER1",
            "dc.l AiCommandset07     ; ATTACKER1",
        ),
        (
            "data/battles/spritesets/spriteset01.asm",
            "combatantAiAndItem ATTACKER2, NOTHING",
            "combatantAiAndItem ATTACKER1, NOTHING",
        ),
        (
            "code/gameflow/battle/battlefunctions/executeindividualturn.asm",
            "jsr     (WaitForVInt).w",
            "jsr     MissingVInt",
        ),
        (
            "code/gameflow/battle/battleactions/battleactionsengine_1.asm",
            "bsr.w   battlesceneScript_InitializeActors",
            "bsr.w   MissingActors",
        ),
        (
            "code/gameflow/battle/battleactions/battleactionsengine_2.asm",
            "battlesceneScript_ApplyActionEffect:",
            "MissingApplyActionEffect:",
        ),
    )
    for path, old, new in mutations:
        mutated = deepcopy(text)
        mutated[path] = mutated[path].replace(old, new, 1)
        with pytest.raises(ValueError, match="source.*drift|assignment drift|sequence drift"):
            turn_control._validate_source_contract(mutated)

    reordered = deepcopy(text)
    actions_path = "code/gameflow/battle/battleactions/battleactionsengine_1.asm"
    target = (
        "bsr.w   battlesceneScript_DetermineTargetsByAction\n"
        "                bsr.w   battlesceneScript_InitializeBattlesceneProperties"
    )
    replacement = (
        "bsr.w   battlesceneScript_InitializeBattlesceneProperties\n"
        "                bsr.w   battlesceneScript_DetermineTargetsByAction"
    )
    assert target in reordered[actions_path]
    reordered[actions_path] = reordered[actions_path].replace(target, replacement, 1)
    with pytest.raises(ValueError, match="source.*drift"):
        turn_control._validate_source_contract(reordered)


def test_research_index_has_exactly_the_eight_new_h2_bindings() -> None:
    index = load_json(ROOT / "manifests/research-index.json")
    expected = {
        "battle.control.main-loop": "turnOrderConsumer.functionAddresses.BattleLoop",
        "battle.functions.execute-turn": "controlDispatch.functionAddresses.ExecuteIndividualTurn",
        "battle.functions.player-input": (
            "controlDispatch.functionAddresses.ProcessBattleEntityControlPlayerInput"
        ),
        "battle.ai.remaining.control-loop": (
            "aiConstructionHandoff.functionAddresses.StartAiControl"
        ),
        "battle.ai.remaining.dispatcher": (
            "aiConstructionHandoff.functionAddresses.ExecuteAiCommand"
        ),
        "battle.ai.control.commandset-pointers": (
            "battle01ControlInputs.tableAddresses.pt_AiCommandsets"
        ),
        "battle.actions.engine": (
            "commonActionConstruction.functionAddresses.WriteBattlesceneScript"
        ),
        "battle.spriteset.data.slot-01": "battle01ControlInputs.tableAddresses.BattleSpriteset01",
    }
    expected_documents = {
        "battle.control.main-loop": [
            "docs/research/battle-loop.md",
            "docs/research/map3-battle01-admission.md",
            "docs/research/map3-battle01-turn-control.md",
            "docs/research/map3-battle01-turn-finalization.md",
        ],
        "battle.functions.execute-turn": [
            "docs/research/battle-functions.md",
            "docs/research/map3-battle01-admission.md",
            "docs/research/map3-battle01-turn-control.md",
            "docs/research/map3-battle01-action-completion.md",
            "docs/research/map3-battle01-turn-finalization.md",
        ],
        "battle.functions.player-input": [
            "docs/research/battle-functions.md",
            "docs/research/map3-battle01-turn-control.md",
        ],
        "battle.ai.remaining.control-loop": [
            "docs/research/battle-ai.md",
            "docs/research/map3-battle01-turn-control.md",
        ],
        "battle.ai.remaining.dispatcher": [
            "docs/research/battle-ai.md",
            "docs/research/map3-battle01-turn-control.md",
        ],
        "battle.ai.control.commandset-pointers": [
            "docs/research/battle-ai.md",
            "docs/research/map3-battle01-turn-control.md",
        ],
        "battle.actions.engine": [
            "docs/research/battle-actions.md",
            "docs/research/map3-battle01-turn-control.md",
            "docs/research/map3-battle01-action-effect.md",
            "docs/research/map3-battle01-action-completion.md",
        ],
        "battle.spriteset.data.slot-01": [
            "docs/research/battle-spriteset-data.md",
            "docs/research/map3-battle01-admission.md",
            "docs/research/map3-battle01-turn-control.md",
        ],
    }
    expected_design_contracts = {
        "battle.control.main-loop": ["docs/design/contracts/battle-control-lifecycle.md"],
        "battle.functions.execute-turn": ["docs/design/contracts/battle-functions-control-flow.md"],
        "battle.functions.player-input": ["docs/design/contracts/battle-functions-control-flow.md"],
        "battle.ai.remaining.control-loop": ["docs/design/contracts/battle-ai-decision.md"],
        "battle.ai.remaining.dispatcher": ["docs/design/contracts/battle-ai-decision.md"],
        "battle.ai.control.commandset-pointers": ["docs/design/contracts/battle-ai-decision.md"],
        "battle.actions.engine": ["docs/design/contracts/battle-action-construction.md"],
        "battle.spriteset.data.slot-01": ["docs/design/contracts/battle-encounter-definition.md"],
    }
    found: dict[str, dict[str, list[str]]] = {}
    for record in index["records"]:
        evidence = [
            evidence for evidence in record["evidence"] if evidence["fixtureId"] == turn_control.ID
        ]
        if not evidence:
            continue
        found[record["id"]] = {
            "bindings": [
                binding["fixtureField"] for item in evidence for binding in item["bindings"]
            ],
            "documents": record["documents"],
            "designContracts": record["designContracts"],
        }
    assert found == {
        record_id: {
            "bindings": [field],
            "documents": expected_documents[record_id],
            "designContracts": expected_design_contracts[record_id],
        }
        for record_id, field in expected.items()
    }


def test_research_index_schema_admits_only_the_authorized_new_fixture_roots() -> None:
    index = load_json(ROOT / "manifests/research-index.json")
    schema = ROOT / "schemas/research-index.schema.json"
    expected_fields = (
        "turnOrderConsumer.functionAddresses.BattleLoop",
        "controlDispatch.functionAddresses.ExecuteIndividualTurn",
        "controlDispatch.functionAddresses.ProcessBattleEntityControlPlayerInput",
        "aiConstructionHandoff.functionAddresses.StartAiControl",
        "aiConstructionHandoff.functionAddresses.ExecuteAiCommand",
        "battle01ControlInputs.tableAddresses.pt_AiCommandsets",
        "commonActionConstruction.functionAddresses.WriteBattlesceneScript",
        "battle01ControlInputs.tableAddresses.BattleSpriteset01",
    )

    def with_fixture_field(field: str) -> dict[str, object]:
        mutated = deepcopy(index)
        for record in mutated["records"]:
            for evidence in record["evidence"]:
                if evidence["fixtureId"] == turn_control.ID:
                    evidence["bindings"][0]["fixtureField"] = field
                    return mutated
        raise AssertionError("turn/control index evidence is missing")

    for field in expected_fields:
        validate_json(with_fixture_field(field), schema, owner="authorized fixture root")
    for field in ("unknownRoot.functionAddresses.Nope", "turnOrderConsumer"):
        with pytest.raises(ValueError):
            validate_json(with_fixture_field(field), schema, owner="unauthorized fixture root")
