from __future__ import annotations

from copy import deepcopy
from pathlib import Path

import pytest

import sf2tool.h3.map_entity_lifecycle_presentation as lifecycle
from sf2tool.h3.map_entity_lifecycle_presentation import (
    build_map_entity_lifecycle_presentation_contract,
    build_map_entity_lifecycle_presentation_static_contract,
    derive_case_expectations,
)
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

FIXTURE = repo_path("tests/fixtures/h3/map-entity-lifecycle-presentation-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h3-map-entity-lifecycle-presentation-fixture.schema.json")
OBSERVATION_SCHEMA = repo_path(
    "schemas/h3-map-entity-lifecycle-presentation-observation.schema.json"
)
OBSERVER = repo_path("tools/bizhawk/map_entity_lifecycle_presentation_observer.lua")
SET_SIZE_CALLBACK_SOURCE = chr(10).join(
    (
        "                bsr.w   UpdateEntitySprite_0",
        "                jsr     (WaitForVInt).w",
        "                move.w  d6,((SPRITE_SIZE-$1000000)).w",
    )
)
SET_SIZE_BIT_SOURCE = chr(10).join(
    (
        "                move.w  (a6)+,((SPRITE_SIZE-$1000000)).w",
        "                ori.b   #%1000,ENTITYDEF_OFFSET_FLAGS_B(a5)",
    )
)
SET_SIZE_RESTORE_SOURCE = chr(10).join(
    (
        "                jsr     (WaitForVInt).w",
        "                move.w  d6,((SPRITE_SIZE-$1000000)).w",
        "                rts",
    )
)


def test_static_contract_parses_all_eight_lifecycle_presentation_handlers() -> None:
    actual = build_map_entity_lifecycle_presentation_static_contract(
        repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
    )

    assert [row["name"] for row in actual["sourceFacts"]["macroForms"]] == [
        "hide",
        "startEntity",
        "stopEntity",
        "waitIdle",
        "setSprite",
        "setPriority",
        "removeShadow",
        "setSize",
    ]
    assert [row["handler"] for row in actual["sourceFacts"]["handlers"]] == [
        "csc2E_hideEntity",
        "csc1B_startEntityAnim",
        "csc1C_stopEntityAnim",
        "csc16_waitUntilEntityIdle",
        "csc1A_setEntitySprite",
        "csc53_setPriority",
        "csc30_removeEntityShadow",
        "csc50_setEntitySize",
    ]
    assert actual["constants"]["combatantAlliesNumber"] == 30
    assert actual["constants"]["sizeBitMutation"]["bitIndex"] == 3


def test_lifecycle_presentation_eleven_case_derivation_matches_complete_fixture() -> None:
    fixture = load_json(FIXTURE)
    actual = build_map_entity_lifecycle_presentation_contract(
        repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
    )

    assert {key: actual[key] for key in fixture if key in actual} == {
        key: fixture[key] for key in fixture if key in actual
    }
    assert derive_case_expectations(actual, fixture) == [
        case["expected"] for case in fixture["cases"]
    ]
    assert all(
        not (set(case["expected"]) & set(case["runtimeGolden"])) for case in fixture["cases"]
    )
    assert [case["id"] for case in fixture["cases"]] == [
        "hide-basic",
        "start-entity-nonzero-current-hp",
        "stop-entity-nonzero-current-hp",
        "stop-entity-zero-current-hp",
        "wait-idle-controlled-second-compare",
        "set-sprite-below-threshold",
        "set-sprite-at-threshold",
        "set-priority-zero",
        "set-priority-nonzero",
        "remove-shadow-callback-chain",
        "set-size-temporary-word-and-flag",
    ]
    assert [
        callback["instructionTarget"]
        for callback in fixture["cases"][6]["expected"]["effectiveCallbackPlan"]
    ] == ["GetEntityAddressFromCharacter", "WaitForVInt", "UpdateEntitySprite_0"]
    stop_zero = fixture["cases"][3]
    assert stop_zero["expected"]["scriptCursorRamOffsetAfter"] == 6
    assert stop_zero["expected"]["animCounterByteAfter"] == 127
    assert [
        callback["instructionTarget"]
        for callback in stop_zero["expected"]["effectiveCallbackPlan"]
    ] == ["AdjustScriptPointerByCharacterAliveStatus"]
    assert stop_zero["runtimeGolden"]["animCounterWriteObserved"] is False


def test_lifecycle_presentation_derivation_rejects_hp_cursor_and_branch_drift() -> None:
    fixture = load_json(FIXTURE)
    actual = build_map_entity_lifecycle_presentation_contract(
        repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
    )

    hp_width = deepcopy(actual)
    hp_width["constants"]["currentHpStorageTransferByteCount"] = 1
    with pytest.raises(ValueError, match="HP/cursor source derivation"):
        derive_case_expectations(hp_width, fixture)

    cursor_load = deepcopy(actual)
    cursor_load["sourceFacts"]["runtimeSourceUseSites"]["startStopCursorAdjustmentLoads"][1][
        "instruction"
    ] = "moveq #1,d7"
    with pytest.raises(ValueError, match="HP/cursor source derivation"):
        derive_case_expectations(cursor_load, fixture)

    branch = deepcopy(actual)
    branch["sourceFacts"]["runtimeSourceUseSites"]["setSpriteAllyBranch"]["instruction"] = (
        "bcs.s @NotAlly"
    )
    with pytest.raises(ValueError, match="branch source derivation"):
        derive_case_expectations(branch, fixture)


def test_lifecycle_presentation_source_use_site_mutation_fails_before_fixture_comparison(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    upstream = repo_path("local/upstream/SF2DISASM").resolve()
    storage_source = (upstream / lifecycle.COMBATANT_WORD_SOURCE_PATH).resolve()
    original_read_text = Path.read_text

    def altered_read_text(path: Path, *args: object, **kwargs: object) -> str:
        source = original_read_text(path, *args, **kwargs)
        if path.resolve() == storage_source:
            changed = source.replace("move.w  (a0,d7.w),d1", "move.b  (a0,d7.w),d1", 1)
            if changed == source:
                raise AssertionError("current-HP source mutation target drift")
            return changed
        return source

    monkeypatch.setattr(Path, "read_text", altered_read_text)
    listing = (upstream / lifecycle.H1_LISTING_PATH).read_text(encoding="utf-8")
    equates = lifecycle._parse_equates(
        (upstream / lifecycle.ENUMS_PATH).read_text(encoding="utf-8"),
        {"COMBATANT_OFFSET_HP_CURRENT"},
    )
    with pytest.raises(ValueError, match="GetCombatantWord source guard drift"):
        lifecycle._current_hp_storage_source_guard(upstream, listing, equates)


@pytest.mark.parametrize(
    ("symbol", "before", "after"),
    [
        (
            "csc16_waitUntilEntityIdle",
            "cmpi.l  #eas_Idle,ENTITYDEF_OFFSET_ACTSCRIPTADDR(a5)",
            "cmpi.l  #eas_Idle,ENTITYDEF_OFFSET_ANIMCOUNTER(a5)",
        ),
        ("csc16_waitUntilEntityIdle", "bne.s   loc_469A0", "beq.s   loc_469A0"),
        ("csc1A_setEntitySprite", "bcc.s   @NotAlly", "bcs.s   @NotAlly"),
        ("csc53_setPriority", "bne.s   loc_46FD4", "beq.s   loc_46FD4"),
        ("csc53_setPriority", "move.b  #1,(a0,d0.w)", "move.b  #2,(a0,d0.w)"),
        (
            "csc50_setEntitySize",
            SET_SIZE_BIT_SOURCE,
            SET_SIZE_BIT_SOURCE.replace("#%1000", "#%100"),
        ),
        (
            "csc50_setEntitySize",
            SET_SIZE_RESTORE_SOURCE,
            SET_SIZE_RESTORE_SOURCE.replace("move.w  d6", "move.w  d5"),
        ),
        (
            "csc50_setEntitySize",
            SET_SIZE_CALLBACK_SOURCE,
            SET_SIZE_CALLBACK_SOURCE.replace("(WaitForVInt).w", "(Sleep).w"),
        ),
    ],
)
def test_lifecycle_presentation_control_source_mutations_fail_before_fixture_comparison(
    monkeypatch: pytest.MonkeyPatch, symbol: str, before: str, after: str
) -> None:
    upstream = repo_path("local/upstream/SF2DISASM").resolve()
    scoped_source = (upstream / lifecycle.SOURCE_PATH).resolve()
    original_read_text = Path.read_text

    def altered_read_text(path: Path, *args: object, **kwargs: object) -> str:
        source = original_read_text(path, *args, **kwargs)
        if path.resolve() == scoped_source:
            start = source.index(f"{symbol}:")
            end = source.index(f"; End of function {symbol}", start)
            section = source[start:end]
            changed_section = section.replace(before, after, 1)
            if changed_section == section:
                raise AssertionError(f"control-flow source mutation target drift: {before}")
            return source[:start] + changed_section + source[end:]
        return source

    monkeypatch.setattr(Path, "read_text", altered_read_text)
    with pytest.raises(ValueError) as error:
        build_map_entity_lifecycle_presentation_static_contract(
            repo_path("local/roms/sf2-us.bin"), upstream
        )
    assert "fixture/source" not in str(error.value)


def test_lifecycle_presentation_parser_rejects_invalid_width_and_accepts_suffixes() -> None:
    from sf2tool.h3.map_entity_lifecycle_presentation import _instruction_width

    assert [
        _instruction_width(instruction)
        for instruction in ("move.b (a6)+,d0", "move.w (a6)+,d0", "move.l (a6)+,d0")
    ] == [1, 2, 4]
    with pytest.raises(ValueError, match="transfer width"):
        _instruction_width("move.x (a6)+,d0")


def test_lifecycle_presentation_observer_uses_bracketed_function_keys() -> None:
    observer = OBSERVER.read_text(encoding="utf-8")

    assert 'config["function"]' in observer
    assert 'config.harness["function"]' in observer
    assert "config.function" not in observer
    assert "config.harness.function" not in observer


@pytest.mark.parametrize(
    "mutate",
    [
        lambda value: value["cases"][10]["expected"].pop("flagsBByteAfter"),
        lambda value: value["cases"][10].update({"flagsBByteSeed": 16}),
        lambda value: value["sourceFacts"]["handlers"][0].update(
            {"handler_name": value["sourceFacts"]["handlers"][0].pop("handler")}
        ),
        lambda value: value["cases"][3]["runtimeGolden"].update({"unexpected": 1}),
        lambda value: value["cases"].reverse(),
        lambda value: value["cases"][6]["scriptWords"].__setitem__(1, 29),
    ],
)
def test_lifecycle_presentation_fixture_schema_rejects_nested_mutations(mutate: object) -> None:
    value = deepcopy(load_json(FIXTURE))
    assert callable(mutate)
    mutate(value)
    with pytest.raises(ValueError, match="fixture failed schema validation"):
        validate_json(value, FIXTURE_SCHEMA, owner="entity lifecycle fixture")


@pytest.mark.parametrize(
    "mutate",
    [
        lambda value: value["records"][10].pop("flagsBByteAfter"),
        lambda value: value["records"][5].update(
            {"mapsprite_after": value["records"][5].pop("mapspriteByteAfter")}
        ),
        lambda value: value["records"][0].update({"unexpected": 1}),
        lambda value: value["records"].reverse(),
        lambda value: value["records"][6].__setitem__("mapspriteByteAfter", 29),
    ],
)
def test_lifecycle_presentation_observation_schema_rejects_nested_mutations(
    mutate: object,
) -> None:
    fixture = load_json(FIXTURE)
    value = {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["id"],
        "mapTest": fixture["mapTestIndex"],
        "recordOrder": [case["id"] for case in fixture["cases"]],
        "records": [{**case["expected"], **case["runtimeGolden"]} for case in fixture["cases"]],
    }
    assert callable(mutate)
    mutate(value)
    with pytest.raises(ValueError, match="observation failed schema validation"):
        validate_json(value, OBSERVATION_SCHEMA, owner="entity lifecycle observation")
