from __future__ import annotations

import json
from copy import deepcopy
from pathlib import Path
from typing import Any

import pytest

from sf2tool.h2 import battle_cutscenes
from sf2tool.h2.battle_cutscenes import build_battle_cutscene_inventory
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

UPSTREAM = repo_path("local/upstream/SF2DISASM")
OUTPUT_SCHEMA = repo_path("schemas/battle-cutscenes-static.schema.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-battle-cutscenes-static-fixture.schema.json")
FIXTURE_PATH = repo_path("tests/fixtures/h2/battle-cutscenes-static-v1.json")
SOURCE_PATHS = (
    Path("sf2enums.asm"),
    Path("code/gameflow/battle/cutscenes/afterenemyleaderdeathpositions.asm"),
    Path("code/common/stats/combatantstats_2.asm"),
    Path("code/common/stats/combatantstats_3.asm"),
    Path("code/common/tech/jumpinterfaces/s02_jumpinterface.asm"),
)


def _write_json(path: Path, value: object) -> Path:
    path.write_bytes((json.dumps(value, indent=2) + "\n").encode("utf-8"))
    return path


def _copy_leader_death_source_surface(tmp_path: Path) -> Path:
    disasm = tmp_path / "disasm"
    for relative_path in SOURCE_PATHS:
        destination = disasm / relative_path
        destination.parent.mkdir(parents=True, exist_ok=True)
        destination.write_bytes((UPSTREAM / "disasm" / relative_path).read_bytes())
    return disasm


def _replace_once(path: Path, before: str, after: str) -> None:
    source = path.read_text(encoding="utf-8")
    assert before in source
    path.write_text(source.replace(before, after, 1), encoding="utf-8")


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_leader_death_position_facts_keep_the_exact_static_dataflow() -> None:
    facts = battle_cutscenes._leader_death_position_facts(UPSTREAM / "disasm")

    assert facts == {
        "requiresBowieAliveAndLeaderDead": True,
        "battleTableEntryBytes": 6,
        "offscreenLoop": {
            "allySlotRange": [0, 29],
            "allySlotCount": 30,
            "allyX": -1,
            "enemySlotRange": [128, 157],
            "enemySlotCount": 30,
            "enemyX": -1,
        },
        "hpZeroLoop": {
            "enemySlotRange": [128, 157],
            "enemySlotCount": 30,
            "hp": 0,
        },
        "positionOnlyTail": {
            "enemySlots": [158, 159],
            "enemySlotCount": 2,
            "x": 0,
            "hasSetCurrentHp": False,
        },
        "positionEntryBytes": 4,
        "positionTerminator": -1,
        "unreachableDeadListWritePresent": True,
    }
    assert "movesAllSlotsOffscreen" not in facts
    assert "setsAllEnemyHpToZero" not in facts


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_battle_cutscene_inventory_reconciles_h1_and_golden_facts() -> None:
    fixture = load_json(FIXTURE_PATH)
    output = build_battle_cutscene_inventory(UPSTREAM)

    assert output["function"] == fixture["function"]
    assert output["cutsceneFacts"] == fixture["expected"]["cutsceneFacts"]


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("suffix", "relative_path", "before", "after", "error"),
    [
        (
            "loop-counter-use",
            Path("code/gameflow/battle/cutscenes/afterenemyleaderdeathpositions.asm"),
            "moveq   #COMBATANT_ALLIES_COUNTER,d7",
            "moveq   #COMBATANT_ENEMIES_COUNTER,d7",
            "offscreen/HP loop chronology",
        ),
        (
            "loop-counter-enum",
            Path("sf2enums.asm"),
            "COMBATANT_ALLIES_COUNTER: equ 29",
            "COMBATANT_ALLIES_COUNTER: equ 28",
            "tail-count/domain relation",
        ),
        (
            "enemy-counter-expands-domain",
            Path("sf2enums.asm"),
            "COMBATANT_ENEMIES_COUNTER: equ 31",
            "COMBATANT_ENEMIES_COUNTER: equ 32",
            "tail-count/domain relation drift: source calls 2, enum domain tail 3",
        ),
        (
            "enemy-counter-contracts-domain",
            Path("sf2enums.asm"),
            "COMBATANT_ENEMIES_COUNTER: equ 31",
            "COMBATANT_ENEMIES_COUNTER: equ 30",
            "tail-count/domain relation drift: source calls 2, enum domain tail 1",
        ),
        (
            "enemy-bit-mapping",
            Path("code/gameflow/battle/cutscenes/afterenemyleaderdeathpositions.asm"),
            "ori.b   #COMBATANT_MASK_ENEMY_BIT,d0",
            "ori.b   #COMBATANT_MASK_SORT_BIT,d0",
            "offscreen/HP loop chronology",
        ),
        (
            "index-sort-mask",
            Path("sf2enums.asm"),
            "COMBATANT_MASK_INDEX_AND_SORT_BIT: equ $7F",
            "COMBATANT_MASK_INDEX_AND_SORT_BIT: equ $FF",
            "index/sort mask reset relation",
        ),
        (
            "loop-x-input",
            Path("code/gameflow/battle/cutscenes/afterenemyleaderdeathpositions.asm"),
            "move.w  #-1,d1",
            "move.w  #0,d1",
            "offscreen/HP loop chronology",
        ),
        (
            "loop-hp-input",
            Path("code/gameflow/battle/cutscenes/afterenemyleaderdeathpositions.asm"),
            "moveq   #0,d1\n                jsr     j_SetCurrentHp",
            "moveq   #1,d1\n                jsr     j_SetCurrentHp",
            "offscreen/HP loop chronology",
        ),
        (
            "removed-loop-hp-write",
            Path("code/gameflow/battle/cutscenes/afterenemyleaderdeathpositions.asm"),
            "jsr     j_SetCurrentHp\n                andi.b",
            "nop\n                andi.b",
            "offscreen/HP loop chronology",
        ),
        (
            "tail-first-slot",
            Path("code/gameflow/battle/cutscenes/afterenemyleaderdeathpositions.asm"),
            "move.w  #158,d0",
            "move.w  #157,d0",
            "position-only tail/domain relation",
        ),
        (
            "tail-order",
            Path("code/gameflow/battle/cutscenes/afterenemyleaderdeathpositions.asm"),
            (
                "jsr     j_SetCombatantX\n"
                "                addq.w  #1,d0\n"
                "                jsr     j_SetCombatantX\n"
                "                movea.l 2(a0),a0"
            ),
            (
                "addq.w  #1,d0\n"
                "                jsr     j_SetCombatantX\n"
                "                jsr     j_SetCombatantX\n"
                "                movea.l 2(a0),a0"
            ),
            "position-only tail chronology",
        ),
        (
            "injected-tail-d1-reload",
            Path("code/gameflow/battle/cutscenes/afterenemyleaderdeathpositions.asm"),
            (
                "dbf     d7,@MoveAllCombatantOffscreen_Loop\n"
                "                \n"
                "                move.w  #158,d0"
            ),
            (
                "dbf     d7,@MoveAllCombatantOffscreen_Loop\n"
                "                moveq   #-1,d1\n"
                "                move.w  #158,d0"
            ),
            "position-only tail chronology",
        ),
        (
            "injected-tail-hp-write",
            Path("code/gameflow/battle/cutscenes/afterenemyleaderdeathpositions.asm"),
            "jsr     j_SetCombatantX\n                movea.l 2(a0),a0",
            (
                "jsr     j_SetCombatantX\n"
                "                jsr     j_SetCurrentHp\n"
                "                movea.l 2(a0),a0"
            ),
            "position-only tail contains SetCurrentHp",
        ),
        (
            "set-combatant-x-helper",
            Path("code/common/stats/combatantstats_2.asm"),
            "moveq   #COMBATANT_OFFSET_X,d7",
            "moveq   #COMBATANT_OFFSET_Y,d7",
            "SetCombatantX d1-preservation",
        ),
        (
            "set-current-hp-preservation",
            Path("code/common/stats/combatantstats_2.asm"),
            (
                "moveq   #COMBATANT_OFFSET_HP_CURRENT,d7\n"
                "                bsr.w   SetCombatantWord\n"
                "                movem.l (sp)+,d7-a0"
            ),
            (
                "moveq   #COMBATANT_OFFSET_HP_CURRENT,d7\n"
                "                bsr.w   SetCombatantWord\n"
                "                moveq   #0,d1"
            ),
            "SetCurrentHp d1-preservation",
        ),
        (
            "set-current-hp-word-write",
            Path("code/common/stats/combatantstats_3.asm"),
            "move.w  d1,(a0,d7.w)",
            "move.w  d0,(a0,d7.w)",
            "SetCombatantWord d1 writer",
        ),
        (
            "set-combatant-x-byte-write",
            Path("code/common/stats/combatantstats_3.asm"),
            "move.b  d1,(a0,d7.w)",
            "move.b  d0,(a0,d7.w)",
            "SetCombatantByte d1 writer",
        ),
        (
            "entry-address-restores-d1",
            Path("code/common/stats/combatantstats_3.asm"),
            "movem.w (sp)+,d0-d1",
            "movem.w (sp)+,d0",
            "combatant-entry d0/d1 preservation",
        ),
        (
            "jump-interface-alias",
            Path("code/common/tech/jumpinterfaces/s02_jumpinterface.asm"),
            "jmp     SetCurrentHp(pc)",
            "jmp     SetCurrentMp(pc)",
            "j_SetCurrentHp effective target",
        ),
    ],
)
def test_leader_death_position_parser_rejects_source_and_enum_mutations(
    tmp_path: Path,
    suffix: str,
    relative_path: Path,
    before: str,
    after: str,
    error: str,
) -> None:
    disasm = _copy_leader_death_source_surface(tmp_path / suffix)
    _replace_once(disasm / relative_path, before, after)

    with pytest.raises(ValueError, match=error):
        battle_cutscenes._leader_death_position_facts(disasm)


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_leader_death_parser_ignores_comment_near_misses(tmp_path: Path) -> None:
    disasm = _copy_leader_death_source_surface(tmp_path)
    listener = disasm / "code/gameflow/battle/cutscenes/afterenemyleaderdeathpositions.asm"
    source = listener.read_text(encoding="utf-8")
    listener.write_text(
        source.replace(
            "moveq   #0,d1", "moveq   #0,d1 ; jsr     j_SetCurrentHp", 1
        )
        + "\n; moveq   #999,d1\n; jsr     j_SetCurrentHp\n",
        encoding="utf-8",
    )

    facts = battle_cutscenes._leader_death_position_facts(disasm)
    assert facts["positionOnlyTail"] == {
        "enemySlots": [158, 159],
        "enemySlotCount": 2,
        "x": 0,
        "hasSetCurrentHp": False,
    }


def _leader_death_facts(value: dict[str, Any]) -> dict[str, Any]:
    return value["expected"]["cutsceneFacts"]["leaderDeathPositions"]


@pytest.mark.parametrize(
    "mutate",
    [
        lambda fixture: fixture["expected"]["cutsceneFacts"]["leaderDeathPositions"].pop(
            "hpZeroLoop"
        ),
        lambda fixture: fixture["expected"]["cutsceneFacts"]["leaderDeathPositions"][
            "positionOnlyTail"
        ].update(unexpected=True),
        lambda fixture: fixture["expected"]["cutsceneFacts"]["leaderDeathPositions"][
            "offscreenLoop"
        ].update(allySlotRange=[0]),
        lambda fixture: fixture["expected"]["cutsceneFacts"]["leaderDeathPositions"][
            "positionOnlyTail"
        ].update(hasSetCurrentHp=0),
    ],
)
def test_battle_cutscene_fixture_schema_closes_leader_death_position_shape(mutate) -> None:
    fixture = deepcopy(load_json(FIXTURE_PATH))
    mutate(fixture)

    with pytest.raises(ValueError, match="leaderDeathPositions"):
        validate_json(fixture, FIXTURE_SCHEMA, owner="battle cutscene structural fixture mutation")


@pytest.mark.parametrize(
    ("field", "value"),
    [
        ("romSha256", "0" * 64),
        ("upstreamCommit", "0" * 40),
    ],
)
def test_battle_cutscene_fixture_schema_preserves_legacy_provenance_constants(
    field: str, value: str
) -> None:
    fixture_schema = load_json(FIXTURE_SCHEMA)
    fixture = deepcopy(load_json(FIXTURE_PATH))
    fixture[field] = value

    assert fixture_schema["properties"][field]["const"] != value
    with pytest.raises(ValueError, match=field):
        validate_json(fixture, FIXTURE_SCHEMA, owner="battle cutscene provenance fixture mutation")


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    "mutate",
    [
        lambda leader_death: leader_death.pop("hpZeroLoop"),
        lambda leader_death: leader_death["positionOnlyTail"].update(unexpected=True),
        lambda leader_death: leader_death["offscreenLoop"].update(allySlotRange=[0]),
        lambda leader_death: leader_death["positionOnlyTail"].update(hasSetCurrentHp=0),
    ],
)
def test_battle_cutscene_output_schema_closes_leader_death_position_shape(mutate) -> None:
    output = build_battle_cutscene_inventory(UPSTREAM)
    leader_death = output["cutsceneFacts"]["leaderDeathPositions"]
    mutate(leader_death)

    with pytest.raises(ValueError, match="leaderDeathPositions"):
        validate_json(output, OUTPUT_SCHEMA, owner="battle cutscene structural output mutation")


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("path", "value"),
    [
        (("scope",), "code/gameflow/battle/other-cutscenes"),
        (("summary", "fileCount"), 11),
        (("summary", "indexedRecordCount"), 11),
        (("summary", "indexedFileCount"), 11),
    ],
)
def test_battle_cutscene_output_schema_preserves_legacy_scope_and_summary_constants(
    path: tuple[str, ...], value: str | int
) -> None:
    output_schema = load_json(OUTPUT_SCHEMA)
    output = build_battle_cutscene_inventory(UPSTREAM)
    target: dict[str, Any] = output
    for field in path[:-1]:
        target = target[field]
    target[path[-1]] = value

    schema_target: dict[str, Any] = output_schema["properties"]
    for field in path[:-1]:
        if field == "summary":
            schema_target = output_schema["definitions"]["summary"]["properties"]
        else:
            schema_target = schema_target[field]["properties"]
    assert schema_target[path[-1]]["const"] != value
    with pytest.raises(ValueError, match=path[-1]):
        validate_json(output, OUTPUT_SCHEMA, owner="battle cutscene legacy output mutation")


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_battle_cutscene_verifier_rejects_schema_valid_fixture_semantic_mutation_before_write(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    fixture = deepcopy(load_json(FIXTURE_PATH))
    _leader_death_facts(fixture)["positionOnlyTail"]["x"] = -1
    fixture_path = _write_json(tmp_path / "battle-cutscenes-fixture.json", fixture)
    validate_json(fixture, FIXTURE_SCHEMA, owner="schema-valid battle cutscene fixture")
    output_path = tmp_path / "battle-cutscenes-output.json"
    monkeypatch.setattr(battle_cutscenes, "FIXTURE", fixture_path)

    with pytest.raises(ValueError, match="battle cutscene model drift"):
        battle_cutscenes.verify_battle_cutscene_inventory(UPSTREAM, output_path=output_path)
    assert not output_path.exists()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_battle_cutscene_verifier_rejects_schema_valid_output_semantic_mutation_before_write(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    output = build_battle_cutscene_inventory(UPSTREAM)
    output["cutsceneFacts"]["leaderDeathPositions"]["hpZeroLoop"]["enemySlotCount"] = 31
    validate_json(output, OUTPUT_SCHEMA, owner="schema-valid battle cutscene output")
    output_path = tmp_path / "battle-cutscenes-output.json"
    monkeypatch.setattr(battle_cutscenes, "build_battle_cutscene_inventory", lambda _: output)

    with pytest.raises(ValueError, match="battle cutscene model drift"):
        battle_cutscenes.verify_battle_cutscene_inventory(UPSTREAM, output_path=output_path)
    assert not output_path.exists()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_battle_cutscene_verifier_rejects_schema_valid_h1_fixture_mutation_before_write(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    fixture = deepcopy(load_json(FIXTURE_PATH))
    fixture["function"]["leaderDeathPositionsAddress"] += 1
    fixture_path = _write_json(tmp_path / "battle-cutscenes-fixture.json", fixture)
    validate_json(fixture, FIXTURE_SCHEMA, owner="schema-valid battle cutscene H1 fixture")
    output_path = tmp_path / "battle-cutscenes-output.json"
    monkeypatch.setattr(battle_cutscenes, "FIXTURE", fixture_path)

    with pytest.raises(ValueError, match="battle cutscene H1 address drift"):
        battle_cutscenes.verify_battle_cutscene_inventory(UPSTREAM, output_path=output_path)
    assert not output_path.exists()
