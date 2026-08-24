from __future__ import annotations

from copy import deepcopy
from pathlib import Path

import pytest
from jsonschema import Draft7Validator

from sf2tool.h2 import map3_battle01_turn_finalization as finalization
from sf2tool.jsonio import load_json

ROOT = Path(__file__).resolve().parents[2]
ROM = ROOT / "local/roms/sf2-us.bin"
UPSTREAM = ROOT / "local/upstream/SF2DISASM"
INDEX = ROOT / "manifests/research-index.json"


def test_scoped_source_inventory_and_h1_rom_anchors_are_complete() -> None:
    text, identities = finalization._read_source_surface(UPSTREAM / "disasm")
    assert len(text) == 11
    assert len(identities) == 11
    assert [identity["path"] for identity in identities] == list(finalization._SOURCE_SURFACE)

    anchors = finalization._anchor_projection(
        (UPSTREAM / "build/sf2build-h1.bin").read_bytes(), ROM.read_bytes()
    )
    assert len(anchors) == 34
    assert anchors[0]["id"] == "turnFinalizationSpine.replay.resume"
    assert anchors[-1]["id"] == "turnFinalizationSpine.outcomeBoundaries.defeat"


def test_source_parser_ignores_comment_near_misses() -> None:
    text = finalization._read_source_surface(UPSTREAM / "disasm")[0]
    comments = deepcopy(text)
    comments[finalization._SOURCE_SURFACE[5]] += "\n; bsr.w ProcessAfterTurnEffects\n"
    assert finalization._validate_source_contract(comments) == {"sourceContract": "confirmed"}


def test_source_parser_rejects_a_required_mutation_in_every_scoped_source() -> None:
    text = finalization._read_source_surface(UPSTREAM / "disasm")[0]
    for path, old, new in (
        (
            finalization._SOURCE_SURFACE[0],
            "jsr     j_EndBattlescene",
            "jsr     MissingEndBattlescene",
        ),
        (finalization._SOURCE_SURFACE[1], "InitializeBattlescene:", "MissingInitialize:"),
        (
            finalization._SOURCE_SURFACE[2],
            "ExecuteBattlesceneScript:",
            "MissingExecuteScript:",
        ),
        (
            finalization._SOURCE_SURFACE[3],
            "ApplyPositionsAfterEnemyLeaderDies:",
            "MissingLeaderPositions:",
        ),
        (
            finalization._SOURCE_SURFACE[4],
            "jsr     j_LoadBattleTerrainData",
            "jsr     MissingTerrainData",
        ),
        (
            finalization._SOURCE_SURFACE[5],
            "bsr.w   ProcessAfterTurnEffects",
            "bsr.w   MissingAfterTurn",
        ),
        (
            finalization._SOURCE_SURFACE[6],
            "ExecuteBattleCutscene_Defeated:",
            "MissingDefeatedCutscene:",
        ),
        (
            finalization._SOURCE_SURFACE[7],
            "ProcessKilledCombatants:",
            "MissingKilledCleanup:",
        ),
        (finalization._SOURCE_SURFACE[8], "CountRemainingCombatants:", "MissingCount:"),
        (
            finalization._SOURCE_SURFACE[9],
            "ProcessAfterTurnEffects:",
            "MissingAfterTurnEffects:",
        ),
        (finalization._SOURCE_SURFACE[10], "BattleLoop_Victory:", "MissingVictory:"),
    ):
        mutated = deepcopy(text)
        assert old in mutated[path]
        mutated[path] = mutated[path].replace(old, new, 1)
        with pytest.raises(ValueError, match="source-use drift"):
            finalization._validate_source_contract(mutated)


def test_h1_parser_rejects_effective_target_call_and_backedge_mutations() -> None:
    h1 = bytearray((UPSTREAM / "build/sf2build-h1.bin").read_bytes())
    for address in (0x241C8, 0x241F4, 0x23B70, 0x23BB2):
        mutated = bytearray(h1)
        mutated[address] ^= 1
        with pytest.raises(ValueError):
            finalization._parse_turn_finalization(bytes(mutated))


def test_h1_parser_rejects_call_targets_aliases_and_backedge_target_mutations() -> None:
    h1 = bytearray((UPSTREAM / "build/sf2build-h1.bin").read_bytes())
    for address in (0x241CD, 0x1800F, 0x23B5D, 0x23B7F, 0x23BB3):
        mutated = bytearray(h1)
        mutated[address] ^= 1
        with pytest.raises(ValueError):
            finalization._parse_turn_finalization(bytes(mutated))


def test_every_required_h1_rom_anchor_rejects_a_rom_mutation() -> None:
    h1 = (UPSTREAM / "build/sf2build-h1.bin").read_bytes()
    rom = bytearray(ROM.read_bytes())
    for identifier, address, _width, _end in finalization._ANCHORS:
        mutated = bytearray(rom)
        mutated[address] ^= 1
        with pytest.raises(ValueError, match=identifier):
            finalization._anchor_projection(h1, bytes(mutated))


def test_source_parser_rejects_outcome_polarity_and_finalization_order_mutations() -> None:
    text = finalization._read_source_surface(UPSTREAM / "disasm")[0]
    for old, new in (
        ("beq.w   BattleLoop_Defeat", "bne.w   BattleLoop_Defeat"),
        ("tst.w   d2", "tst.w   d4"),
        (
            "jsr     ProcessKilledCombatants(pc)\n"
            "                nop\n"
            "                bsr.w   CountRemainingCombatants",
            "bsr.w   CountRemainingCombatants\n"
            "                nop\n"
            "                jsr     ProcessKilledCombatants(pc)",
        ),
    ):
        mutated = deepcopy(text)
        path = finalization._SOURCE_SURFACE[5]
        mutated[path] = mutated[path].replace(old, new, 1)
        with pytest.raises(ValueError, match="source-use drift"):
            finalization._validate_source_contract(mutated)


def test_fixture_is_closed_and_has_exact_unknown_register() -> None:
    fixture = load_json(ROOT / "tests/fixtures/h2/map3-battle01-turn-finalization-static-v1.json")
    assert list(fixture) == [
        "schemaVersion",
        "id",
        "upstream",
        "romSha256",
        "system",
        "summary",
        "retainedR3c",
        "retainedOwners",
        "sourceContext",
        "turnFinalizationSpine",
        "unknowns",
    ]
    assert list(fixture["unknowns"]) == list(finalization._UNKNOWN_KEYS)
    assert set(fixture["unknowns"].values()) == {"Unknown"}

    for mutation in (
        lambda value: value.__setitem__("unexpected", "not-public"),
        lambda value: value["sourceContext"]["h1RomAnchors"][0].__setitem__("bytes", "private"),
    ):
        mutated = deepcopy(fixture)
        mutation(mutated)
        with pytest.raises(ValueError, match="structural schema validation failed"):
            finalization._validate_structural_output(mutated)


def test_summary_is_derived_from_each_constructed_collection() -> None:
    fixture = load_json(finalization.FIXTURE)
    _text, source_identities = finalization._read_source_surface(UPSTREAM / "disasm")
    anchors = finalization._anchor_projection(
        (UPSTREAM / "build/sf2build-h1.bin").read_bytes(), ROM.read_bytes()
    )
    index = load_json(INDEX)
    owner_ids = finalization._owner_record_ids(index)
    owner_evidence = finalization._owner_evidence(index, owner_ids)
    unknowns = {key: "Unknown" for key in finalization._UNKNOWN_KEYS}
    summary = finalization._summary(source_identities, anchors, owner_evidence, unknowns)
    assert fixture["summary"] == summary
    assert (
        finalization._summary(source_identities[:-1], anchors, owner_evidence, unknowns)[
            "sourceFiles"
        ]
        == summary["sourceFiles"] - 1
    )
    assert (
        finalization._summary(source_identities, anchors[:-1], owner_evidence, unknowns)[
            "h1RomAnchors"
        ]
        == summary["h1RomAnchors"] - 1
    )
    assert (
        finalization._summary(source_identities, anchors, owner_evidence[:-1], unknowns)[
            "indexObjects"
        ]
        == summary["indexObjects"] - 1
    )
    reduced_bindings = deepcopy(owner_evidence)
    reduced_bindings[0]["bindings"].pop()
    assert (
        finalization._summary(source_identities, anchors, reduced_bindings, unknowns)[
            "indexBindings"
        ]
        == summary["indexBindings"] - 1
    )
    assert finalization._summary(source_identities, anchors, owner_evidence, {})["unknowns"] == 0


def test_retained_projection_and_golden_boundary_drifts_are_rejected(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    r3c = iter(({"sha256": "A"}, {"sha256": "B"}))
    monkeypatch.setattr(finalization, "_retained_r3c", lambda *_args: next(r3c))
    monkeypatch.setattr(finalization, "_retained_owners", lambda: {"stable": "owner"})
    with pytest.raises(ValueError, match="pre-construction retained projection drift"):
        finalization.build_map3_battle01_turn_finalization_static(ROM, UPSTREAM)

    fixture = load_json(finalization.FIXTURE)
    generated = deepcopy(fixture)
    generated["retainedR3c"]["sha256"] = "0" * 64
    monkeypatch.setattr(
        finalization, "build_map3_battle01_turn_finalization_static", lambda *_args: generated
    )
    with pytest.raises(ValueError, match="retained golden-boundary projection drift"):
        finalization.verify_map3_battle01_turn_finalization_static(ROM, UPSTREAM)


def test_research_index_has_exact_turn_finalization_object_delta() -> None:
    fixture_id = "sf2-map3-battle01-turn-finalization-static-v1"
    document = "docs/research/map3-battle01-turn-finalization.md"
    expected = {
        "battle.functions.execute-turn": 7,
        "battle.replay.execute-script": 3,
        "battle.scene.initialize": 1,
        "battle.cutscene.leader-death-positions": 1,
        "battle.functions.load-battle": 1,
        "battle.control.main-loop": 9,
        "battle.cutscene.battle-end-start": 1,
        "battle.loop.process-killed": 1,
        "battle.loop.count-remaining": 1,
        "battle.status.after-turn-expiry": 1,
        "battle.control.outcomes": 2,
    }
    index = load_json(INDEX)
    records = {record["id"]: record for record in index["records"]}
    registered = {
        record["id"]
        for record in index["records"]
        if any(item["fixtureId"] == fixture_id for item in record["evidence"])
    }
    assert registered == set(expected)
    assert {record["id"] for record in index["records"] if document in record["documents"]} == set(
        expected
    )
    evidence = {
        record_id: next(
            item for item in records[record_id]["evidence"] if item["fixtureId"] == fixture_id
        )
        for record_id in expected
    }
    fields = [binding["fixtureField"] for item in evidence.values() for binding in item["bindings"]]
    assert {key: len(item["bindings"]) for key, item in evidence.items()} == expected
    assert len(fields) == len(set(fields)) == 28
    assert all(field.startswith("turnFinalizationSpine.") for field in fields)
    address_ids = {
        address["id"]
        for record in records.values()
        for address in record["addresses"]
        if address["id"]
        in {
            "initialize-battlescene-call",
            "execute-battlescene-call",
            "end-battlescene-call",
            "leader-death-positions-call",
            "reload-battle-call",
            "execute-turn-resume",
            "defeated-cutscene-call",
            "process-killed-first-call",
            "count-first-call",
            "after-turn-call",
            "after-turn-resume",
            "count-second-call",
            "next-turn-dispatch",
            "end-battlescene-entry",
            "defeat-entry",
            "return",
        }
    }
    assert len(address_ids) == 16

    schema = load_json(ROOT / "schemas/research-index.schema.json")
    validator = Draft7Validator(schema)
    assert not list(validator.iter_errors(index))
    for invalid_root in ("sourceContext", "unknowns"):
        invalid = deepcopy(index)
        target = next(
            evidence
            for record in invalid["records"]
            for evidence in record["evidence"]
            if evidence["fixtureId"] == fixture_id
        )
        target["bindings"][0]["fixtureField"] = (
            f"{invalid_root}.turnFinalizationSpine.replay.resumeAddress"
        )
        assert list(validator.iter_errors(invalid))
