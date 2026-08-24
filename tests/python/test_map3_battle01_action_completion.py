from __future__ import annotations

from copy import deepcopy
from pathlib import Path

import pytest

from sf2tool.h2 import map3_battle01_action_completion as completion
from sf2tool.jsonio import load_json, validate_json

ROOT = Path(__file__).resolve().parents[2]
ROM = ROOT / "local/roms/sf2-us.bin"
UPSTREAM = ROOT / "local/upstream/SF2DISASM"
RESEARCH_INDEX = ROOT / "manifests/research-index.json"
RESEARCH_INDEX_SCHEMA = ROOT / "schemas/research-index.schema.json"


def _source_text() -> dict[str, str]:
    return completion._read_source_surface(UPSTREAM / "disasm")[0]


def test_source_parser_ignores_comment_near_misses_and_rejects_completion_mutations() -> None:
    text = _source_text()
    comments = deepcopy(text)
    comments["code/gameflow/battle/battleactions/battleactionsengine_1.asm"] += (
        "\n; bsr.w battlesceneScript_ValidateCounterAttack\n"
    )
    assert completion._validate_source_contract(comments) == {"sourceContract": "confirmed"}

    mutations = (
        (
            "code/gameflow/battle/battleactions/battleactionsengine_1.asm",
            "dbf     d7,@ApplyActionOnTargets_Loop",
            "dbf     d6,@ApplyActionOnTargets_Loop",
        ),
        (
            "code/gameflow/battle/battleactions/battleactionsengine_1.asm",
            "beq.s   @CounterAttack",
            "bne.s   @CounterAttack",
        ),
        (
            "code/gameflow/battle/battleactions/battleactionsengine_1.asm",
            "makeActorIdleAndEndAnimation",
            "missingActorIdleAndEndAnimation",
        ),
        (
            "code/gameflow/battle/battleactions/battleactionsengine_1.asm",
            "move.b  explodingActor(a2),(a4)",
            "move.b  missingExplodingActor(a2),(a4)",
        ),
        (
            "code/gameflow/battle/battleactions/battleactionsengine_2.asm",
            "bsr.w   battlesceneScript_GiveExpAndGold",
            "bsr.w   MissingGiveExpAndGold",
        ),
        (
            "code/gameflow/battle/battleactions/battleactionsengine_2.asm",
            "cmpi.w  #COMBATANT_ALLIES_END,d0",
            "cmpi.w  #COMBATANT_ENEMIES_END,d0",
        ),
        (
            "code/gameflow/battle/battleactions/battleactionsengine_2.asm",
            "bra.s   loc_A3BC",
            "bra.s   loc_A3D4",
        ),
        (
            "code/gameflow/battle/battleactions/battleactionsengine_2.asm",
            "move.w  -(a0),d1\n                jsr     SetCurrentHp",
            "move.w  -(a0),d1\n                jsr     MissingSetCurrentHp",
        ),
        (
            "code/gameflow/battle/battleactions/animateaction.asm",
            "makeEnemyIdle",
            "MissingEnemyIdle",
        ),
        (
            "code/gameflow/battle/battleactions/breakuseditem.asm",
            "btst    #ITEMTYPE_BIT_BREAKABLE,ITEMDEF_OFFSET_TYPE(a0)",
            "btst    #ITEMTYPE_BIT_BREAKABLE,ITEMDEF_OFFSET_FLAGS(a0)",
        ),
        (
            "code/gameflow/battle/battleactions/isabletocounterattack.asm",
            "tst.b   counterAttack(a2)",
            "tst.b   doubleAttack(a2)",
        ),
        (
            "code/gameflow/battle/battleactions/createbattlescenemessage.asm",
            "battlesceneScript_DisplayActionMessage:",
            "MissingDisplayActionMessage:",
        ),
        (
            "code/gameflow/battle/battleactions/createbattlesceneanimation.asm",
            "battlesceneScript_PerformAnimation:",
            "MissingPerformAnimation:",
        ),
        (
            "code/gameflow/battle/battleactions/giveexpandgold.asm",
            "battlesceneScript_GiveExpAndGold:",
            "MissingGiveExpAndGold:",
        ),
        (
            "code/gameflow/battle/battlefunctions/executeindividualturn.asm",
            "jsr     j_WriteBattlesceneScript",
            "jsr     MissingWriteBattlesceneScript",
        ),
    )
    for path, old, new in mutations:
        mutated = deepcopy(text)
        mutated[path] = mutated[path].replace(old, new, 1)
        with pytest.raises(ValueError, match="source-use drift"):
            completion._validate_source_contract(mutated)


def test_source_parser_does_not_match_duplicates_after_function_boundaries() -> None:
    text = _source_text()
    cases = (
        (
            "code/gameflow/battle/battleactions/battleactionsengine_1.asm",
            "bsr.w   battlesceneScript_BreakUsedItem",
            "bsr.w   MissingBreakUsedItem",
            "bsr.w   battlesceneScript_BreakUsedItem",
        ),
        (
            "code/gameflow/battle/battleactions/battleactionsengine_2.asm",
            "move.w  -(a0),d1\n                jsr     SetCurrentHp",
            "move.w  -(a0),d1\n                jsr     MissingSetCurrentHp",
            "move.w  -(a0),d1\n                jsr     SetCurrentHp",
        ),
        (
            "code/gameflow/battle/battleactions/animateaction.asm",
            "makeEnemyIdle",
            "missingEnemyIdle",
            "makeEnemyIdle",
        ),
    )
    for path, old, new, duplicate_after_boundary in cases:
        mutated = deepcopy(text)
        mutated[path] = mutated[path].replace(old, new, 1) + f"\n{duplicate_after_boundary}\n"
        with pytest.raises(ValueError, match="source-use drift"):
            completion._validate_source_contract(mutated)


def test_secondary_label_symbols_match_the_bounded_source_and_h1_listing() -> None:
    source = _source_text()
    labels = {
        "battlesceneScript_DetermineTargetsByAction": (
            "code/gameflow/battle/battleactions/battleactionsengine_1.asm",
            0x9DD6,
        ),
        "battlesceneScript_SwitchTargets": (
            "code/gameflow/battle/battleactions/animateaction.asm",
            0xA702,
        ),
        "battlesceneScript_MakeActorIdle": (
            "code/gameflow/battle/battleactions/animateaction.asm",
            0xA7D0,
        ),
    }
    listing = completion.listing_symbol_addresses(
        (UPSTREAM / "build/sf2build-h1.lst").read_text(encoding="utf-8")
    )
    for symbol, (path, address) in labels.items():
        section = completion._function_section(source[path], f"{symbol}:", symbol)
        assert completion._normalized(section).splitlines()[0] == f"{symbol}:"
        assert listing[symbol] == completion._FUNCTIONS[symbol] == address


def test_h1_parser_derives_full_completion_spine_and_rejects_use_site_mutations() -> None:
    h1 = (UPSTREAM / "build/sf2build-h1.bin").read_bytes()
    spine = completion._parse_action_completion(h1)
    assert spine["startResumes"] == {
        "primaryTargetLoop": 0x9CD8,
        "secondAttack": 0x9D3A,
        "counterAttack": 0x9D98,
    }
    assert spine["primaryTargetLoop"] == {
        "resumeAddress": 0x9CD8,
        "orderedSteps": ["targetAdvance", "directionSet", "dbfBackedge"],
        "backedgeAddress": 0x9CDC,
        "backedgeTarget": 0x9CB6,
        "counterRegister": "d7",
    }
    assert spine["followupBranches"]["doubleAttack"] == {
        "validatorCallAddress": 0x9CF0,
        "validatorRangeEndExclusive": 0xA49C,
        "decisionRange": [0x9CF4, 0x9CFA],
        "zeroBranchTarget": 0x9D3E,
        "attackType": "SECOND",
        "blockRange": [0x9CFA, 0x9D3E],
        "orderedCalls": [
            "SwitchTargets",
            "DisplayActionMessage",
            "PerformAnimation",
            "SwitchTargets",
            "ApplyActionEffect",
            "DropEnemyItem",
            "MakeActorIdle",
        ],
    }
    assert spine["followupBranches"]["counterAttack"] == {
        "validatorCallAddress": 0x9D46,
        "validatorReturnAddress": 0xA54C,
        "decisionRange": [0x9D4A, 0x9D50],
        "zeroBranchTarget": 0x9D9C,
        "attackType": "COUNTER",
        "blockRange": [0x9D50, 0x9D9C],
        "orderedCalls": [
            "SwitchTargets",
            "DisplayActionMessage",
            "PerformAnimation",
            "SwitchTargets",
            "ApplyActionEffect",
            "DropEnemyItem",
            "MakeActorIdle",
        ],
    }
    assert spine["explosionBackedge"] == {
        "range": [0x9D9C, 0x9DC4],
        "zeroBranchTarget": 0x9DC4,
        "orderedSteps": [
            "clearExplode",
            "setBurstRock",
            "restoreExplodingActor",
            "idleAndEndAnimation",
            "DetermineTargetsByAction",
            "backedge",
        ],
        "backedgeAddress": 0x9DC0,
        "backedgeTarget": 0x9C7E,
    }
    assert spine["endSequence"] == {
        "writeRange": [0x9DC4, 0x9DD6],
        "battlesceneEndRange": [0xA34E, 0xA3F4],
        "orderedWriteSteps": [
            "restoreActorCopy",
            "battlesceneScript_End",
            "stackRelease",
            "return",
        ],
        "orderedEndSteps": [
            "endAnimation",
            "SwitchTargets",
            "rewardGate",
            "GiveExpAndGold",
            "currentHpReplay",
            "hideTextBox",
            "endCommand",
            "return",
        ],
        "returnAddress": 0x9DD4,
        "determineTargetsEntry": 0x9DD6,
        "currentHpReplayEndAddress": 0xA3E6,
    }
    assert spine["executeIndividualTurnHandoff"] == {
        "callAddress": 0x24100,
        "resumeAddress": 0x24106,
        "instructionTarget": "j_WriteBattlesceneScript",
        "instructionTargetAddress": 0x820C,
        "effectiveTarget": "WriteBattlesceneScript",
        "effectiveTargetAddress": 0x9B92,
    }

    mutations = (
        (0x9CDC, "primary loop resume drift"),
        (0x9CF8, "branch (?:opcode|target) drift"),
        (0x9CFC, "immediate move drift"),
        (0x9D4E, "branch (?:opcode|target) drift"),
        (0x9DA4, "explosion test drift"),
        (0x9DC0, "(?:opcode|target) drift"),
        (0x9DCE, "write return drift"),
        (0xA3AE, "opcode drift"),
        (0x24100, "write handoff instruction drift"),
        (0x820C, "write alias opcode drift"),
    )
    for address, message in mutations:
        drifted = bytearray(h1)
        drifted[address] ^= 1
        with pytest.raises(ValueError, match=message):
            completion._parse_action_completion(bytes(drifted))


def test_fixture_is_complete_closed_public_static_contract() -> None:
    fixture = load_json(completion.FIXTURE)
    assert list(fixture) == [
        "schemaVersion",
        "id",
        "upstream",
        "romSha256",
        "system",
        "summary",
        "retainedR3b",
        "retainedBattleActions",
        "sourceContext",
        "actionCompletionSpine",
        "unknowns",
    ]
    assert fixture["summary"] == {
        "sourceFiles": 9,
        "h1RomAnchors": 26,
        "indexObjects": 10,
        "indexBindings": 20,
        "battleActionsIndexedRecords": 47,
        "battleActionsIndexedPaths": 29,
        "unknowns": 33,
    }
    assert fixture["actionCompletionSpine"] == completion._parse_action_completion(
        (UPSTREAM / "build/sf2build-h1.bin").read_bytes()
    ) | {"ownerRecordIds": list(completion._OWNER_RECORD_IDS)}
    assert fixture["unknowns"] == {key: "Unknown" for key in completion._UNKNOWN_KEYS}
    public = completion.canonical_json_bytes(fixture).decode("utf-8").lower()
    for forbidden in (
        "raw source",
        "rom bytes",
        "h1 bytes",
        "runtime state",
        "capture",
        "movie",
        "save",
        "emulator",
        "lua",
    ):
        assert forbidden not in public


def test_fresh_h2_derivation_matches_the_complete_fixture() -> None:
    assert completion.build_map3_battle01_action_completion_static(ROM, UPSTREAM) == load_json(
        completion.FIXTURE
    )


def test_schema_is_recursively_closed_and_rejects_boundary_mutations() -> None:
    schema = load_json(completion.SCHEMA)
    fixture = load_json(completion.FIXTURE)
    validate_json(fixture, completion.SCHEMA, owner="fixture")

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
    for mutation in (
        lambda value: value.__setitem__("extra", True),
        lambda value: value["sourceContext"]["sourceIdentities"][0].__setitem__("extra", True),
        lambda value: value["actionCompletionSpine"].__setitem__("extra", True),
        lambda value: value["actionCompletionSpine"]["functionAddresses"].__setitem__("Nope", 0),
        lambda value: value["actionCompletionSpine"]["followupBranches"]["doubleAttack"][
            "orderedCalls"
        ].reverse(),
        lambda value: value["actionCompletionSpine"]["executeIndividualTurnHandoff"].pop(
            "effectiveTarget"
        ),
        lambda value: value["unknowns"].pop("nextTurnDispatch"),
        lambda value: value["unknowns"].__setitem__("extra", "Unknown"),
    ):
        mutated = deepcopy(fixture)
        mutation(mutated)
        with pytest.raises(ValueError):
            validate_json(mutated, completion.SCHEMA, owner="mutation")


def test_every_h1_rom_anchor_rejects_mutation() -> None:
    h1 = (UPSTREAM / "build/sf2build-h1.bin").read_bytes()
    rom = ROM.read_bytes()
    guard_order = completion._anchor_guard_order()
    for identifier, address, width, _end in guard_order:
        mutation_address = next(
            candidate
            for candidate in range(address, address + width)
            if not any(
                prior_address <= candidate < prior_address + prior_width
                for _prior_id, prior_address, prior_width, _prior_end in guard_order[
                    : guard_order.index((identifier, address, width, _end))
                ]
            )
        )
        for binary_name in ("H1", "ROM"):
            mutated = bytearray(h1 if binary_name == "H1" else rom)
            mutated[mutation_address] ^= 1
            with pytest.raises(ValueError, match=identifier):
                completion._anchor_projection(
                    bytes(mutated) if binary_name == "H1" else h1,
                    bytes(mutated) if binary_name == "ROM" else rom,
                )


def test_retained_projections_reject_before_construction(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    r3b = load_json(completion.R3B_FIXTURE)
    r3b["unknowns"]["playerReady"] = "Inferred"
    monkeypatch.setattr(completion, "build_map3_battle01_action_effect_static", lambda *_: r3b)
    with pytest.raises(ValueError, match="retained R3b fixture projection drift"):
        completion._retained_r3b(ROM, UPSTREAM)

    baseline = completion.battle_actions.build_battle_actions_inventory(UPSTREAM)
    for field, value, error in (
        ("indexedRecordCount", 46, "indexed record count drift"),
        ("indexedFileCount", 28, "indexed path count drift"),
    ):
        drifted = deepcopy(baseline)
        drifted["summary"][field] = value
        monkeypatch.setattr(
            completion.battle_actions,
            "build_battle_actions_inventory",
            lambda *_, fresh=drifted: fresh,
        )
        with pytest.raises(ValueError, match=error):
            completion._retained_battle_actions(UPSTREAM)

    for record_id in (
        "battle.actions.apply-effect-dispatch",
        "battle.actions.cast-spell",
    ):
        drifted = deepcopy(baseline)
        drifted["indexedRecordIds"].remove(record_id)
        monkeypatch.setattr(
            completion.battle_actions,
            "build_battle_actions_inventory",
            lambda *_, fresh=drifted: fresh,
        )
        with pytest.raises(ValueError, match=record_id):
            completion._retained_battle_actions(UPSTREAM)


def test_retained_r3b_projection_drift_at_golden_boundary_rejects(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    fixture = load_json(completion.FIXTURE)
    r3b = fixture["retainedR3b"]
    actions = fixture["retainedBattleActions"]
    drifted_r3b = deepcopy(r3b)
    drifted_r3b["sha256"] = "0" * 64
    r3b_projections = iter((r3b, drifted_r3b))
    action_projections = iter((actions, actions))
    monkeypatch.setattr(completion, "_retained_r3b", lambda *_: next(r3b_projections))
    monkeypatch.setattr(completion, "_retained_battle_actions", lambda *_: next(action_projections))
    monkeypatch.setattr(
        completion, "build_map3_battle01_action_completion_static", lambda *_: fixture
    )
    with pytest.raises(ValueError, match="golden-boundary projection drift"):
        completion.verify_map3_battle01_action_completion_static(ROM, UPSTREAM)


def test_retained_battle_actions_projection_drift_at_golden_boundary_rejects(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    fixture = load_json(completion.FIXTURE)
    r3b = fixture["retainedR3b"]
    actions = fixture["retainedBattleActions"]
    drifted_actions = deepcopy(actions)
    drifted_actions["sha256"] = "0" * 64
    r3b_projections = iter((r3b, r3b))
    action_projections = iter((actions, drifted_actions))
    monkeypatch.setattr(completion, "_retained_r3b", lambda *_: next(r3b_projections))
    monkeypatch.setattr(completion, "_retained_battle_actions", lambda *_: next(action_projections))
    monkeypatch.setattr(
        completion, "build_map3_battle01_action_completion_static", lambda *_: fixture
    )
    with pytest.raises(ValueError, match="golden-boundary projection drift"):
        completion.verify_map3_battle01_action_completion_static(ROM, UPSTREAM)


def test_research_index_has_only_the_exact_action_completion_registration() -> None:
    index = load_json(RESEARCH_INDEX)
    validate_json(index, RESEARCH_INDEX_SCHEMA, owner="research index")
    assert len(index["records"]) == 1625

    fixture_id = "sf2-map3-battle01-action-completion-static-v1"
    document = "docs/research/map3-battle01-action-completion.md"
    expected = {
        "battle.actions.engine": {
            "addresses": {
                "target-loop-drop-return": 0x9CD8,
                "second-attack-drop-return": 0x9D3A,
                "counter-attack-drop-return": 0x9D98,
                "return": 0x9DD4,
                "determine-targets": 0x9DD6,
            },
            "fields": {
                "actionCompletionSpine.functionAddresses.WriteBattlesceneScript",
                "actionCompletionSpine.startResumes.primaryTargetLoop",
                "actionCompletionSpine.startResumes.secondAttack",
                "actionCompletionSpine.startResumes.counterAttack",
                "actionCompletionSpine.endSequence.returnAddress",
                "actionCompletionSpine.endSequence.determineTargetsEntry",
            },
        },
        "battle.actions.break-used-item": {
            "addresses": {},
            "fields": {"actionCompletionSpine.functionAddresses.battlesceneScript_BreakUsedItem"},
        },
        "battle.actions.perform-animation": {
            "addresses": {},
            "fields": {
                "actionCompletionSpine.functionAddresses.battlesceneScript_PerformAnimation"
            },
        },
        "battle.actions.display-message": {
            "addresses": {},
            "fields": {
                "actionCompletionSpine.functionAddresses.battlesceneScript_DisplayActionMessage"
            },
        },
        "battle.actions.animate": {
            "addresses": {"switch-targets": 0xA702, "make-actor-idle": 0xA7D0},
            "fields": {
                "actionCompletionSpine.functionAddresses.battlesceneScript_SwitchTargets",
                "actionCompletionSpine.functionAddresses.battlesceneScript_MakeActorIdle",
            },
        },
        "battle.followup.validate-double": {
            "addresses": {},
            "fields": {
                "actionCompletionSpine.functionAddresses.battlesceneScript_ValidateDoubleAttack",
                "actionCompletionSpine.followupBranches.doubleAttack.validatorRangeEndExclusive",
            },
        },
        "battle.followup.validate-counter": {
            "addresses": {},
            "fields": {
                "actionCompletionSpine.functionAddresses.battlesceneScript_ValidateCounterAttack",
                "actionCompletionSpine.followupBranches.counterAttack.validatorReturnAddress",
            },
        },
        "battle.replay.end": {
            "addresses": {},
            "fields": {
                "actionCompletionSpine.functionAddresses.battlesceneScript_End",
                "actionCompletionSpine.endSequence.currentHpReplayEndAddress",
            },
        },
        "battle.replay.give-exp-and-gold": {
            "addresses": {},
            "fields": {"actionCompletionSpine.functionAddresses.battlesceneScript_GiveExpAndGold"},
        },
        "battle.functions.execute-turn": {
            "addresses": {
                "write-battlescene-call": 0x24100,
                "write-battlescene-resume": 0x24106,
            },
            "fields": {
                "actionCompletionSpine.executeIndividualTurnHandoff.callAddress",
                "actionCompletionSpine.executeIndividualTurnHandoff.resumeAddress",
            },
        },
    }
    expected_address_ids = {
        "battle.actions.engine": {
            "entry",
            "target-loop-drop-return",
            "second-attack-drop-return",
            "counter-attack-drop-return",
            "return",
            "determine-targets",
        },
        "battle.actions.break-used-item": {"entry"},
        "battle.actions.perform-animation": {"entry"},
        "battle.actions.display-message": {"entry"},
        "battle.actions.animate": {"entry", "switch-targets", "make-actor-idle"},
        "battle.followup.validate-double": {"entry", "clear", "rejected-return", "return"},
        "battle.followup.validate-counter": {"entry", "return"},
        "battle.replay.end": {"entry", "hp-restore"},
        "battle.replay.give-exp-and-gold": {
            "entry",
            "target-side-decision",
            "exp-halved",
            "exp-first-roll",
            "exp-second-roll",
            "exp-final",
            "battle-scene-gold",
            "battle-scene-exp",
            "random-seed",
        },
        "battle.functions.execute-turn": {
            "entry",
            "write-battlescene-call",
            "write-battlescene-resume",
            "initialize-battlescene-call",
            "execute-battlescene-call",
            "end-battlescene-call",
            "leader-death-positions-call",
            "reload-battle-call",
            "return",
        },
    }
    expected_new_address_shapes = {
        "battle.actions.engine": [
            {
                "id": "target-loop-drop-return",
                "space": "rom",
                "kind": "observation",
                "value": 0x9CD8,
            },
            {
                "id": "second-attack-drop-return",
                "space": "rom",
                "kind": "observation",
                "value": 0x9D3A,
            },
            {
                "id": "counter-attack-drop-return",
                "space": "rom",
                "kind": "observation",
                "value": 0x9D98,
            },
            {"id": "return", "space": "rom", "kind": "observation", "value": 0x9DD4},
            {
                "id": "determine-targets",
                "space": "rom",
                "kind": "observation",
                "value": 0x9DD6,
                "symbol": "battlesceneScript_DetermineTargetsByAction",
            },
        ],
        "battle.actions.animate": [
            {
                "id": "switch-targets",
                "space": "rom",
                "kind": "observation",
                "value": 0xA702,
                "symbol": "battlesceneScript_SwitchTargets",
            },
            {
                "id": "make-actor-idle",
                "space": "rom",
                "kind": "observation",
                "value": 0xA7D0,
                "symbol": "battlesceneScript_MakeActorIdle",
            },
        ],
        "battle.functions.execute-turn": [
            {
                "id": "write-battlescene-call",
                "space": "rom",
                "kind": "observation",
                "value": 0x24100,
            },
            {
                "id": "write-battlescene-resume",
                "space": "rom",
                "kind": "observation",
                "value": 0x24106,
            },
        ],
    }
    expected_design_contracts = {
        "battle.actions.engine": ["docs/design/contracts/battle-action-construction.md"],
        "battle.actions.break-used-item": ["docs/design/contracts/battle-action-construction.md"],
        "battle.actions.perform-animation": ["docs/design/contracts/battle-action-construction.md"],
        "battle.actions.display-message": ["docs/design/contracts/battle-action-construction.md"],
        "battle.actions.animate": ["docs/design/contracts/battle-action-construction.md"],
        "battle.followup.validate-double": ["docs/design/contracts/combat-resolution.md"],
        "battle.followup.validate-counter": ["docs/design/contracts/combat-resolution.md"],
        "battle.replay.end": ["docs/design/contracts/combat-resolution.md"],
        "battle.replay.give-exp-and-gold": [
            "docs/design/contracts/combat-resolution.md",
            "docs/design/contracts/spell-resolution.md",
        ],
        "battle.functions.execute-turn": ["docs/design/contracts/battle-functions-control-flow.md"],
    }

    records = {record["id"]: record for record in index["records"]}
    registered = {
        record["id"]
        for record in index["records"]
        if any(evidence["fixtureId"] == fixture_id for evidence in record["evidence"])
    }
    assert registered == set(expected)
    assert {record["id"] for record in index["records"] if document in record["documents"]} == set(
        expected
    )

    all_fields: list[str] = []
    for record_id, expectation in expected.items():
        record = records[record_id]
        address_values = {address["id"]: address["value"] for address in record["addresses"]}
        assert set(address_values) == expected_address_ids[record_id]
        if record_id in expected_new_address_shapes:
            expected_new_addresses = expected_new_address_shapes[record_id]
            new_address_ids = {address["id"] for address in expected_new_addresses}
            assert [
                address for address in record["addresses"] if address["id"] in new_address_ids
            ] == expected_new_addresses
        assert (
            expectation["addresses"].items()
            <= {key: address_values[key] for key in expectation["addresses"]}.items()
        )
        assert record["designContracts"] == expected_design_contracts[record_id]
        evidence = next(item for item in record["evidence"] if item["fixtureId"] == fixture_id)
        assert evidence["level"] == "H2"
        assert (
            evidence["fixture"]
            == "tests/fixtures/h2/map3-battle01-action-completion-static-v1.json"
        )
        assert evidence["verifier"] == "src/sf2tool/h2/map3_battle01_action_completion.py"
        fields = {binding["fixtureField"] for binding in evidence["bindings"]}
        assert fields == expectation["fields"]
        all_fields.extend(fields)

    assert len(all_fields) == 20
    assert len(set(all_fields)) == 20
    assert sum(len(value["addresses"]) for value in expected.values()) == 9
    assert all(field.startswith("actionCompletionSpine.") for field in all_fields)

    alias = deepcopy(index)
    alias_record = next(
        record for record in alias["records"] if record["id"] == "battle.actions.engine"
    )
    alias_evidence = next(
        item for item in alias_record["evidence"] if item["fixtureId"] == fixture_id
    )
    alias_evidence["bindings"][0]["fixtureField"] = (
        "sourceContext.actionCompletionSpine.functionAddresses.WriteBattlesceneScript"
    )
    with pytest.raises(ValueError):
        validate_json(alias, RESEARCH_INDEX_SCHEMA, owner="sourceContext alias")

    unknown_root = deepcopy(index)
    unknown_record = next(
        record for record in unknown_root["records"] if record["id"] == "battle.actions.engine"
    )
    unknown_evidence = next(
        item for item in unknown_record["evidence"] if item["fixtureId"] == fixture_id
    )
    unknown_evidence["bindings"][0]["fixtureField"] = "actionCompletionSpine.unknown"
    with pytest.raises(ValueError):
        validate_json(unknown_root, RESEARCH_INDEX_SCHEMA, owner="unknown action-completion root")
