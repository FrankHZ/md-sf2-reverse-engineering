from __future__ import annotations

from copy import deepcopy
from pathlib import Path

import pytest

from sf2tool.h2 import map3_battle01_action_effect as action_effect
from sf2tool.jsonio import load_json, validate_json

ROOT = Path(__file__).resolve().parents[2]
ROM = ROOT / "local/roms/sf2-us.bin"
UPSTREAM = ROOT / "local/upstream/SF2DISASM"


def _source_text() -> dict[str, str]:
    return action_effect._read_source_surface(UPSTREAM / "disasm")[0]


def test_source_parser_ignores_comment_near_misses_but_rejects_dispatch_mutations() -> None:
    text = _source_text()
    comments = deepcopy(text)
    comments["code/gameflow/battle/battleactions/battleactionsengine_2.asm"] += (
        "\n; cmpi.w #BATTLEACTION_PRISM_LASER,(a3)\n"
    )
    assert action_effect._validate_source_contract(comments)["sourceContract"] == "confirmed"

    mutations = (
        ("battlesceneScript_ApplyActionEffect:", "MissingApplyActionEffect:"),
        ("bne.s   @IsCastSpell", "beq.s   @IsCastSpell"),
        ("move.w  #BATTLEACTION_BURST_ROCK_POWER,d6", "move.w  #17,d6"),
        ("tst.b   targetDies(a2)", "tst.b   dodge(a2)"),
        ("bsr.w   battlesceneScript_DropEnemyItem", "bsr.w   MissingDropEnemyItem"),
    )
    for old, new in mutations:
        mutated = deepcopy(text)
        path = "code/gameflow/battle/battleactions/battleactionsengine_2.asm"
        if old == "bsr.w   battlesceneScript_DropEnemyItem":
            path = "code/gameflow/battle/battleactions/battleactionsengine_1.asm"
        mutated[path] = mutated[path].replace(old, new, 1)
        with pytest.raises(ValueError, match="source(?:-use| label/line) drift"):
            action_effect._validate_source_contract(mutated)


def test_h1_dispatch_derives_values_targets_polarity_and_three_caller_resumes() -> None:
    h1 = (UPSTREAM / "build/sf2build-h1.bin").read_bytes()
    dispatch, powers = action_effect._parse_h1_dispatch(h1)
    assert dispatch == [
        {
            "selector": 0,
            "sourceSelector": "BATTLEACTION_ATTACK",
            "action": "Attack",
            "power": None,
            "primaryCall": "battlesceneScript_Attack",
            "targetDiesFalse": None,
            "targetDiesTrue": None,
        },
        {
            "selector": 1,
            "sourceSelector": "BATTLEACTION_CAST_SPELL",
            "action": "CastSpell",
            "power": None,
            "primaryCall": "battlesceneScript_CastSpell",
            "targetDiesFalse": None,
            "targetDiesTrue": None,
        },
        {
            "selector": 2,
            "sourceSelector": "BATTLEACTION_USE_ITEM",
            "action": "UseItem",
            "power": None,
            "primaryCall": "battlesceneScript_UseItem",
            "targetDiesFalse": None,
            "targetDiesTrue": None,
        },
        {
            "selector": 4,
            "sourceSelector": "BATTLEACTION_BURST_ROCK",
            "action": "BurstRock",
            "power": 18,
            "primaryCall": "battlesceneScript_InflictDamage",
            "targetDiesFalse": "Done",
            "targetDiesTrue": "battlesceneScript_DisplayDeathMessage",
        },
        {
            "selector": 5,
            "sourceSelector": "BATTLEACTION_MUDDLED",
            "action": "Muddled",
            "power": None,
            "primaryCall": None,
            "targetDiesFalse": None,
            "targetDiesTrue": None,
        },
        {
            "selector": 6,
            "sourceSelector": "BATTLEACTION_PRISM_LASER",
            "action": "PrismLaser",
            "power": 16,
            "primaryCall": "battlesceneScript_InflictDamage",
            "targetDiesFalse": "Done",
            "targetDiesTrue": "battlesceneScript_DisplayDeathMessage",
        },
        {
            "selector": "default",
            "sourceSelector": None,
            "action": "Done",
            "power": None,
            "primaryCall": None,
            "targetDiesFalse": None,
            "targetDiesTrue": None,
        },
    ]
    assert powers == {
        "BATTLEACTION_BURST_ROCK_POWER": 18,
        "BATTLEACTION_PRISM_LASER_POWER": 16,
    }
    assert action_effect._parse_caller_contexts(h1) == [
        {
            "id": "targetLoop",
            "applyCallAddress": 0x9CD0,
            "dropCallAddress": 0x9CD4,
            "resumeAddress": 0x9CD8,
        },
        {
            "id": "secondAttack",
            "applyCallAddress": 0x9D32,
            "dropCallAddress": 0x9D36,
            "resumeAddress": 0x9D3A,
        },
        {
            "id": "counterAttack",
            "applyCallAddress": 0x9D90,
            "dropCallAddress": 0x9D94,
            "resumeAddress": 0x9D98,
        },
    ]

    mutations = (
        (0xA3FA, "selector values drift"),
        (0xA429, "call target drift"),
        (0xA42E, "polarity drift"),
        (0xA458, "return convergence drift"),
    )
    for address, message in mutations:
        drifted = bytearray(h1)
        drifted[address] ^= 1
        with pytest.raises(ValueError, match=message):
            action_effect._parse_h1_dispatch(bytes(drifted))
    caller_drift = bytearray(h1)
    caller_drift[0x9D36] ^= 1
    with pytest.raises(ValueError, match="call opcode drift"):
        action_effect._parse_caller_contexts(bytes(caller_drift))


def test_fixture_is_complete_closed_public_static_contract() -> None:
    fixture = load_json(action_effect.FIXTURE)
    assert list(fixture) == [
        "actionEffectSpine",
        "id",
        "retainedBattleActions",
        "retainedR3a",
        "romSha256",
        "schemaVersion",
        "sourceContext",
        "summary",
        "system",
        "unknowns",
        "upstream",
    ]
    assert fixture["summary"] == {
        "sourceFiles": 8,
        "h1RomAnchors": 21,
        "callerContexts": 3,
        "actionSelectors": 7,
        "indexObjects": 8,
        "indexBindings": 8,
        "battleActionsIndexedRecords": 47,
        "battleActionsIndexedPaths": 29,
        "unknowns": 23,
    }
    assert fixture["actionEffectSpine"]["functionAddresses"] == {
        "WriteBattlesceneScript": 0x9B92,
        "battlesceneScript_ApplyActionEffect": 0xA3F4,
        "battlesceneScript_Attack": 0xAAB6,
        "battlesceneScript_CastSpell": 0xB0A8,
        "battlesceneScript_UseItem": 0xBBB8,
        "battlesceneScript_InflictDamage": 0xACEA,
        "battlesceneScript_DisplayDeathMessage": 0xB080,
        "battlesceneScript_DropEnemyItem": 0xBD24,
    }
    assert fixture["actionEffectSpine"]["rewardConvergence"] == {
        "dropOwnerRecordId": "battle.reward.drop-enemy-item",
        "resumeAddresses": [0x9CD8, 0x9D3A, 0x9D98],
        "boundary": "DropEnemyItemReturn",
    }
    assert fixture["actionEffectSpine"]["ownerRecordIds"] == list(action_effect._OWNER_RECORD_IDS)
    assert fixture["unknowns"] == {key: "Unknown" for key in action_effect._UNKNOWN_KEYS}
    public = action_effect.canonical_json_bytes(fixture).decode("utf-8").lower()
    for forbidden in (
        "raw source",
        "rom bytes",
        "h1 bytes",
        "runtime state",
        "rng",
        "capture",
        "movie",
        "save",
        "emulator",
        "lua",
    ):
        assert forbidden not in public


def test_fresh_h2_derivation_matches_the_complete_fixture() -> None:
    assert action_effect.build_map3_battle01_action_effect_static(ROM, UPSTREAM) == load_json(
        action_effect.FIXTURE
    )


def test_schema_is_recursively_closed_and_rejects_boundary_mutations() -> None:
    schema = load_json(action_effect.SCHEMA)
    fixture = load_json(action_effect.FIXTURE)
    validate_json(fixture, action_effect.SCHEMA, owner="fixture")

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
        lambda value: value["actionEffectSpine"]["functionAddresses"].__setitem__("Nope", 0),
        lambda value: value["actionEffectSpine"]["dispatch"].reverse(),
        lambda value: value["actionEffectSpine"]["callerContexts"][0].pop("resumeAddress"),
        lambda value: value["unknowns"].pop("playerReady"),
        lambda value: value["unknowns"].__setitem__("extra", "Unknown"),
    ):
        mutated = deepcopy(fixture)
        mutation(mutated)
        with pytest.raises(ValueError):
            validate_json(mutated, action_effect.SCHEMA, owner="mutation")


def test_every_h1_rom_anchor_rejects_mutation() -> None:
    h1 = (UPSTREAM / "build/sf2build-h1.bin").read_bytes()
    rom = ROM.read_bytes()
    for identifier, address, _width, _end in action_effect._ANCHORS:
        for binary_name in ("H1", "ROM"):
            mutated = bytearray(h1 if binary_name == "H1" else rom)
            mutated[address] ^= 1
            expected = (
                "actionEffectSpine.applyActionEffectRange"
                if 0xA3F4 <= address < 0xA45E
                else identifier
            )
            with pytest.raises(ValueError, match=expected):
                action_effect._anchor_projection(
                    bytes(mutated) if binary_name == "H1" else h1,
                    bytes(mutated) if binary_name == "ROM" else rom,
                )


def test_retained_projections_reject_before_output(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    r3a = load_json(action_effect.R3A_FIXTURE)
    r3a["unknowns"]["playerReady"] = "Inferred"
    monkeypatch.setattr(action_effect, "build_map3_battle01_turn_control_static", lambda *_: r3a)
    with pytest.raises(ValueError, match="retained R3a fixture projection drift"):
        action_effect._retained_r3a(ROM, UPSTREAM)

    actions = load_json(action_effect.battle_actions.FIXTURE)
    drifted = deepcopy(actions)
    drifted["expected"]["actionFacts"]["engine"]["perTargetOrder"].reverse()
    monkeypatch.setattr(
        action_effect.battle_actions,
        "build_battle_actions_inventory",
        lambda *_: {
            "summary": {"indexedRecordCount": 47, "indexedFileCount": 29},
            "indexedRecordIds": [
                "battle.actions.apply-effect-dispatch",
                "battle.actions.cast-spell",
            ],
            "actionFacts": {"engine": drifted["expected"]["actionFacts"]["engine"]},
        },
    )
    with pytest.raises(ValueError, match="retained battle-actions engine projection drift"):
        action_effect._retained_battle_actions(UPSTREAM)


def test_retained_battle_actions_rejects_index_relation_drift(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    baseline = action_effect.battle_actions.build_battle_actions_inventory(UPSTREAM)
    mutations = (
        ("indexedRecordCount", 46, "indexed record count drift"),
        ("indexedFileCount", 28, "indexed path count drift"),
    )
    for field, value, error in mutations:
        drifted = deepcopy(baseline)
        drifted["summary"][field] = value
        monkeypatch.setattr(
            action_effect.battle_actions,
            "build_battle_actions_inventory",
            lambda *_, fresh=drifted: fresh,
        )
        with pytest.raises(ValueError, match=error):
            action_effect._retained_battle_actions(UPSTREAM)

    for missing_record_id in (
        "battle.actions.apply-effect-dispatch",
        "battle.actions.cast-spell",
    ):
        drifted = deepcopy(baseline)
        drifted["indexedRecordIds"].remove(missing_record_id)
        monkeypatch.setattr(
            action_effect.battle_actions,
            "build_battle_actions_inventory",
            lambda *_, fresh=drifted: fresh,
        )
        with pytest.raises(ValueError, match="indexed record IDs drift"):
            action_effect._retained_battle_actions(UPSTREAM)


def test_retained_projection_drift_at_golden_boundary_rejects(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    fixture = load_json(action_effect.FIXTURE)
    r3a = fixture["retainedR3a"]
    actions = fixture["retainedBattleActions"]
    drifted_r3a = deepcopy(r3a)
    drifted_r3a["sha256"] = "0" * 64
    r3a_projections = iter((r3a, drifted_r3a))
    action_projections = iter((actions, actions))
    monkeypatch.setattr(action_effect, "_retained_r3a", lambda *_: next(r3a_projections))
    monkeypatch.setattr(
        action_effect, "_retained_battle_actions", lambda *_: next(action_projections)
    )
    monkeypatch.setattr(
        action_effect, "build_map3_battle01_action_effect_static", lambda *_: fixture
    )
    with pytest.raises(ValueError, match="golden-boundary projection drift"):
        action_effect.verify_map3_battle01_action_effect_static(ROM, UPSTREAM)


def test_research_index_has_exactly_eight_new_h2_bindings_and_no_design_drift() -> None:
    index = load_json(ROOT / "manifests/research-index.json")
    expected = {
        "battle.actions.apply-effect-dispatch": {
            "field": "actionEffectSpine.functionAddresses.battlesceneScript_ApplyActionEffect",
            "sourcePath": "code/gameflow/battle/battleactions/battleactionsengine_2.asm",
            "symbol": "battlesceneScript_ApplyActionEffect",
            "entry": 41972,
            "documents": ["docs/research/map3-battle01-action-effect.md"],
            "designContracts": None,
        },
        "battle.actions.cast-spell": {
            "field": "actionEffectSpine.functionAddresses.battlesceneScript_CastSpell",
            "sourcePath": "code/gameflow/battle/battleactions/castspell.asm",
            "symbol": "battlesceneScript_CastSpell",
            "entry": 45224,
            "documents": ["docs/research/map3-battle01-action-effect.md"],
            "designContracts": None,
        },
        "battle.actions.engine": {
            "field": "actionEffectSpine.functionAddresses.WriteBattlesceneScript",
            "sourcePath": "code/gameflow/battle/battleactions/battleactionsengine_1.asm",
            "symbol": "WriteBattlesceneScript",
            "entry": 39826,
            "documents": [
                "docs/research/battle-actions.md",
                "docs/research/map3-battle01-turn-control.md",
                "docs/research/map3-battle01-action-effect.md",
                "docs/research/map3-battle01-action-completion.md",
            ],
            "designContracts": ["docs/design/contracts/battle-action-construction.md"],
        },
        "battle.actions.attack": {
            "field": "actionEffectSpine.functionAddresses.battlesceneScript_Attack",
            "sourcePath": "code/gameflow/battle/battleactions/attack.asm",
            "symbol": "battlesceneScript_Attack",
            "entry": 43702,
            "documents": [
                "docs/research/battle-actions.md",
                "docs/research/map3-battle01-action-effect.md",
            ],
            "designContracts": ["docs/design/contracts/battle-action-construction.md"],
        },
        "battle.actions.use-item": {
            "field": "actionEffectSpine.functionAddresses.battlesceneScript_UseItem",
            "sourcePath": "code/gameflow/battle/battleactions/useitem.asm",
            "symbol": "battlesceneScript_UseItem",
            "entry": 48056,
            "documents": [
                "docs/research/battle-actions.md",
                "docs/research/map3-battle01-action-effect.md",
            ],
            "designContracts": ["docs/design/contracts/battle-action-construction.md"],
        },
        "battle.damage.inflict": {
            "field": "actionEffectSpine.functionAddresses.battlesceneScript_InflictDamage",
            "sourcePath": "code/gameflow/battle/battleactions/inflictdamage.asm",
            "symbol": "battlesceneScript_InflictDamage",
            "entry": 44266,
            "documents": [
                "docs/research/runtime-rng-and-battle-math.md",
                "docs/research/map3-battle01-action-effect.md",
            ],
            "designContracts": [
                "docs/design/contracts/combat-resolution.md",
                "docs/design/contracts/spell-resolution.md",
            ],
        },
        "battle.actions.display-death": {
            "field": "actionEffectSpine.functionAddresses.battlesceneScript_DisplayDeathMessage",
            "sourcePath": "code/gameflow/battle/battleactions/displaydeathmessage.asm",
            "symbol": "battlesceneScript_DisplayDeathMessage",
            "entry": 45184,
            "documents": [
                "docs/research/battle-actions.md",
                "docs/research/map3-battle01-action-effect.md",
            ],
            "designContracts": ["docs/design/contracts/battle-action-construction.md"],
        },
        "battle.reward.drop-enemy-item": {
            "field": "actionEffectSpine.functionAddresses.battlesceneScript_DropEnemyItem",
            "sourcePath": "code/gameflow/battle/battleactions/dropenemyitem.asm",
            "symbol": "battlesceneScript_DropEnemyItem",
            "entry": 48420,
            "documents": [
                "docs/research/enemy-promotions.md",
                "docs/research/runtime-rng-and-battle-math.md",
                "docs/research/map3-battle01-action-effect.md",
            ],
            "designContracts": ["docs/design/contracts/combat-resolution.md"],
        },
    }
    found: dict[str, list[str]] = {}
    for record in index["records"]:
        evidence = [item for item in record["evidence"] if item["fixtureId"] == action_effect.ID]
        if evidence:
            found[record["id"]] = [
                binding["fixtureField"] for item in evidence for binding in item["bindings"]
            ]
        if record["id"] not in expected:
            assert "docs/research/map3-battle01-action-effect.md" not in record["documents"]
    assert found == {record_id: [details["field"]] for record_id, details in expected.items()}
    records = {record["id"]: record for record in index["records"]}
    for record_id, details in expected.items():
        record = records[record_id]
        assert record["sourcePath"] == details["sourcePath"]
        assert record["symbol"] == details["symbol"]
        assert (
            next(address for address in record["addresses"] if address["id"] == "entry")["value"]
            == details["entry"]
        )
        assert record["documents"] == details["documents"]
        assert record.get("designContracts") == details["designContracts"]


def test_research_index_schema_admits_only_authorized_action_effect_fixture_roots() -> None:
    index = load_json(ROOT / "manifests/research-index.json")
    schema = ROOT / "schemas/research-index.schema.json"
    expected_fields = tuple(
        binding
        for record in index["records"]
        for evidence in record["evidence"]
        if evidence["fixtureId"] == action_effect.ID
        for binding in (entry["fixtureField"] for entry in evidence["bindings"])
    )

    def with_fixture_field(field: str) -> dict[str, object]:
        mutated = deepcopy(index)
        for record in mutated["records"]:
            for evidence in record["evidence"]:
                if evidence["fixtureId"] == action_effect.ID:
                    evidence["bindings"][0]["fixtureField"] = field
                    return mutated
        raise AssertionError("action/effect index evidence is missing")

    for field in expected_fields:
        validate_json(with_fixture_field(field), schema, owner="authorized fixture root")
    for field in (
        "unknownRoot.functionAddresses.Nope",
        "actionEffectSpineUnknown.functionAddresses.Nope",
        "actionEffectSpine",
    ):
        with pytest.raises(ValueError):
            validate_json(with_fixture_field(field), schema, owner="unauthorized fixture root")
