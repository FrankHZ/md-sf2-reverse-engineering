from __future__ import annotations

import copy
from pathlib import Path
from typing import Any

import pytest

from sf2tool.h2.map3_optional_interactions import (
    FIXTURE,
    SCHEMA,
    _parse_area_descriptions,
    _parse_entity_definitions,
    _parse_item_placements,
    build_map3_optional_interactions,
    canonical_json_bytes,
)
from sf2tool.jsonio import load_json, validate_json


def _write(root: Path, relative_path: str, content: str) -> None:
    path = root / relative_path
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(content.lstrip(), encoding="utf-8")


def _synthetic_upstream(tmp_path: Path) -> Path:
    """Build a minimal project-owned source tree with every owned input seam."""
    root = tmp_path / "disasm"
    _write(
        root,
        "data/maps/mapsetups.asm",
        """
        MapSetups:
            msMap 3, ms_map3
            msFlag 609, ms_map3_variant_a
            msFlag 506, ms_map3_variant_b
            msFlag 543, ms_map3_variant_c
            msMapEnd
        """,
    )
    _write(
        root,
        "data/maps/entries/map03/mapsetups/pointertable.asm",
        """
        ms_map3: dc.l ms_map3_Entities
            dc.l ms_map3_EntityEvents
            dc.l ms_map3_ZoneEvents
            dc.l ms_map3_AreaDescriptions
            dc.l ms_map3_Section5
            dc.l ms_map3_InitFunction
        """,
    )
    _write(
        root,
        "data/maps/entries/map03/mapsetups/s1_entities.asm",
        """
        ms_map3_Entities:
            msFixedEntity 1, 2, DOWN, MAPSPRITE_TEST_FIXED, eas_TestInit
            msWalkingEntity 3, 4, UP, MAPSPRITE_TEST_WALKING, 3, 4, 2
        """,
    )
    _write(
        root,
        "data/maps/entries/map03/mapsetups/s2_entityevents.asm",
        """
        ms_map3_EntityEvents:
            msEntityEvent ALLY_SARAH, DOWN, Map3_EntityEvent0-ms_map3_EntityEvents
            msDefaultEntityEvent Map3_DefaultEntityEvent-ms_map3_EntityEvents

        Map3_EntityEvent0:
            chkFlg 1
            bne.s map3_done
            txt 7
            setFlg 1
        map3_done:
            rts
        ; End of function Map3_EntityEvent0

        Map3_DefaultEntityEvent:
            rts
        """,
    )
    _write(
        root,
        "data/maps/entries/map03/mapsetups/s4_descriptions.asm",
        """
        ms_map3_AreaDescriptions:
            move.w #$FC3,d3
        synthetic_description: msDesc 3, 4, 2, 9
            msDescEnd
        """,
    )
    _write(
        root,
        "data/maps/entries/map03/mapsetups/s5_itemevents.asm",
        """
        ms_map3_Section5:
            msDefaultItemEvent Map3_DefaultItemEvent0-ms_map3_Section5

        Map3_DefaultItemEvent0:
            rts
        ; End of function Map3_DefaultItemEvent0
        """,
    )
    _write(
        root,
        "data/maps/entries/map03/7-chest-items.asm",
        """
            mapItem 1, 2, 3, TEST_CHEST_ITEM
            endWord
        """,
    )
    _write(
        root,
        "data/maps/entries/map03/8-other-items.asm",
        """
            mapItem 4, 5, 6, TEST_OTHER_ITEM
            endWord
        """,
    )
    _write(
        root,
        "sf2mapsetupmacros.asm",
        """
        msEntityEvent: macro
            dc.b \\1
            dc.b \\2
            dc.w \\3
        endm
        msDefaultEntityEvent: macro
            dc.b $FD
            dc.b 0
            dc.w \\1
        endm
        msDesc: macro
            dc.b \\1
            dc.b \\2
            dc.b 0
            dc.b 0
            dc.b \\3
            dc.b \\4
        endm
        msDefaultItemEvent: macro
            dc.l $FD000000
            dc.w \\1
        endm
        """,
    )
    _write(
        root,
        "sf2mapmacros.asm",
        """
        mapItem: macro
            dc.b \\1
            dc.b \\2
            dc.b \\3
            defineShorthand.b ITEM_,\\4
        endm
        endWord: macro
            dc.w $FFFF
        endm
        """,
    )
    _write(
        root,
        "code/common/scripting/map/mapsetupsfunctions_1.asm",
        """
        RunMapSetupEntityEvent:
            cmpi.b #$FD,(a0,d7.w)
            adda.w 2(a0,d7.w),a0
            addq.w #4,d7
            jsr (a0)
        ; End of function RunMapSetupEntityEvent

        RunMapSetupItemEvent:
            cmpi.b #$FD,(a0,d7.w)
            adda.w 4(a0,d7.w),a0
            addq.w #6,d7
            jsr (a0)
        ; End of function RunMapSetupItemEvent

        DisplayAreaDescription:
            addi.w #423,d0
            addq.w #6,d7
            jsr (DisplayText).w
            jsr (DisplayText).w
        ; End of function DisplayAreaDescription
        """,
    )
    _write(
        root,
        "code/common/tech/jumpinterfaces/s05_jumpinterface.asm",
        """
        j_ChurchMenu:
            jmp ChurchMenu(pc)
        """,
    )
    return root


def _read(root: Path, relative_path: str) -> str:
    return (root / relative_path).read_text(encoding="utf-8")


def _replace(root: Path, relative_path: str, old: str, new: str) -> None:
    source = _read(root, relative_path)
    assert old in source
    (root / relative_path).write_text(source.replace(old, new), encoding="utf-8")


def test_synthetic_complete_inventory_is_deterministic_and_structural(tmp_path: Path) -> None:
    root = _synthetic_upstream(tmp_path)

    first = build_map3_optional_interactions(root)
    second = build_map3_optional_interactions(root)

    assert first == second
    assert canonical_json_bytes(first) == canonical_json_bytes(second)
    assert first["pointerSetup"] == {
        "mapId": 3,
        "defaultPointerTable": "ms_map3",
        "variantFlagsInSourceOrder": [609, 506, 543],
        "pointerSlotsInSourceOrder": [
            "ms_map3_Entities",
            "ms_map3_EntityEvents",
            "ms_map3_ZoneEvents",
            "ms_map3_AreaDescriptions",
            "ms_map3_Section5",
            "ms_map3_InitFunction",
        ],
    }
    assert first["entityDefinitions"] == {
        "sourcePath": "data/maps/entries/map03/mapsetups/s1_entities.asm",
        "recordCount": 2,
        "recordMacroCounts": {"msFixedEntity": 1, "msWalkingEntity": 1},
        "actionKindCounts": {"fixed": 1, "walking": 1},
    }
    assert first["entityEventRoutes"] == [
        {
            "recordIndex": 0,
            "sourceLine": 2,
            "recordMacro": "msEntityEvent",
            "entityId": "ALLY_SARAH",
            "facing": "DOWN",
            "program": {
                "target": "Map3_EntityEvent0",
                "operationOrder": ["chkFlg", "bne.s", "txt", "setFlg", "rts"],
                "textIndices": [7],
                "flagConditions": [
                    {
                        "flag": 1,
                        "operationIndex": 0,
                        "branchMnemonic": "bne.s",
                        "branchTarget": "map3_done",
                    }
                ],
                "flagEffects": [{"flag": 1, "operationIndex": 3}],
                "scriptTargets": [],
                "menuCall": None,
            },
            "routeRelevance": {
                "evidence": "Confirmed",
                "classification": "mandatory-observed-opening",
            },
        },
        {
            "recordIndex": 1,
            "sourceLine": 3,
            "recordMacro": "msDefaultEntityEvent",
            "entityId": "$FD",
            "facing": "0",
            "program": {
                "target": "Map3_DefaultEntityEvent",
                "operationOrder": ["rts"],
                "textIndices": [],
                "flagConditions": [],
                "flagEffects": [],
                "scriptTargets": [],
                "menuCall": None,
            },
            "routeRelevance": {"evidence": "Unknown", "classification": "unknown"},
        },
    ]
    assert first["areaDescriptions"] == {
        "sourcePath": "data/maps/entries/map03/mapsetups/s4_descriptions.asm",
        "recordCount": 1,
        "recordMacro": "msDesc",
        "recordStrideBytes": 6,
        "effectShape": "two-display-text-calls",
        "routeRelevanceCounts": {"unknown": 1},
    }
    assert first["itemPlacements"] == {
        "recordCount": 2,
        "sourceOwners": [
            {
                "sourcePath": "data/maps/entries/map03/7-chest-items.asm",
                "sourceKind": "chest",
                "recordCount": 1,
            },
            {
                "sourcePath": "data/maps/entries/map03/8-other-items.asm",
                "sourceKind": "other",
                "recordCount": 1,
            },
        ],
        "recordMacro": "mapItem",
        "terminatorMacro": "endWord",
        "unknownRouteRelevanceCount": 2,
    }
    assert first["defaultItemEvent"]["targetOperationOrder"] == ["rts"]
    assert first["summary"] == {
        "sourcePathCount": 12,
        "defaultMap3SourcePathCount": 8,
        "genericSourcePathCount": 4,
        "entityDefinitionCount": 2,
        "entityEventRouteCount": 2,
        "mandatoryObservedOpeningRouteCount": 1,
        "unknownEntityEventRouteCount": 1,
        "areaDescriptionCount": 1,
        "itemPlacementCount": 2,
    }


def test_comments_and_legal_branch_suffix_are_not_misparsed(tmp_path: Path) -> None:
    root = _synthetic_upstream(tmp_path)
    baseline = build_map3_optional_interactions(root)
    event_path = "data/maps/entries/map03/mapsetups/s2_entityevents.asm"
    (root / event_path).write_text(
        _read(root, event_path)
        + "; msEntityEvent GHOST, UP, Fake-ms_map3_EntityEvents\n"
        + "; txt 999 is a comment, not an event operation\n",
        encoding="utf-8",
    )
    assert build_map3_optional_interactions(root) == baseline
    _replace(root, event_path, "bne.s map3_done", "bne.w map3_done")
    assert build_map3_optional_interactions(root)["entityEventRoutes"][0]["program"][
        "flagConditions"
    ][0]["branchMnemonic"] == "bne.w"
    _replace(root, event_path, "bne.w map3_done", "bne.x map3_done")
    with pytest.raises(ValueError, match="flag branch drift"):
        build_map3_optional_interactions(root)


def test_menu_alias_call_shape_and_near_miss_target_are_guarded(tmp_path: Path) -> None:
    root = _synthetic_upstream(tmp_path)
    event_path = "data/maps/entries/map03/mapsetups/s2_entityevents.asm"
    _replace(
        root,
        event_path,
        "    msDefaultEntityEvent Map3_DefaultEntityEvent-ms_map3_EntityEvents\n",
        "    msEntityEvent 141, UP, Map3_EntityEvent13-ms_map3_EntityEvents\n"
        "    msDefaultEntityEvent Map3_DefaultEntityEvent-ms_map3_EntityEvents\n",
    )
    (root / event_path).write_text(
        _read(root, event_path)
        + "\nMap3_EntityEvent13:\n"
        + "    jsr j_ChurchMenu\n"
        + "    rts\n"
        + "; End of function Map3_EntityEvent13\n",
        encoding="utf-8",
    )
    output = build_map3_optional_interactions(root)
    assert output["entityEventRoutes"][1]["program"]["menuCall"] == {
        "instructionMnemonic": "jsr",
        "instructionTarget": "j_ChurchMenu",
        "effectiveTarget": "ChurchMenu",
        "operationIndex": 0,
    }
    _replace(
        root,
        "code/common/tech/jumpinterfaces/s05_jumpinterface.asm",
        "jmp ChurchMenu(pc)",
        "jmp ChurchMenuNearMiss(pc)",
    )
    with pytest.raises(ValueError, match="Church menu alias drift"):
        build_map3_optional_interactions(root)


@pytest.mark.parametrize(
    ("relative_path", "old", "new", "message"),
    [
        (
            "sf2mapsetupmacros.asm",
            "dc.b $FD",
            "dc.b $FC",
            "macro shape drift: msDefaultEntityEvent",
        ),
        (
            "code/common/scripting/map/mapsetupsfunctions_1.asm",
            "adda.w 2(a0,d7.w),a0",
            "adda.w 4(a0,d7.w),a0",
            "entity default target offset source-use drift",
        ),
        (
            "data/maps/entries/map03/mapsetups/s2_entityevents.asm",
            "Map3_EntityEvent0-ms_map3_EntityEvents",
            "Map3_EntityEvent0-ms_other_EntityEvents",
            "entity event target relation drift",
        ),
        (
            "data/maps/entries/map03/7-chest-items.asm",
            "endWord",
            "endWordNearMiss",
            "item placement terminator is missing",
        ),
    ],
)
def test_source_shape_mutations_fail_at_the_smallest_owned_seam(
    tmp_path: Path, relative_path: str, old: str, new: str, message: str
) -> None:
    root = _synthetic_upstream(tmp_path)
    _replace(root, relative_path, old, new)
    with pytest.raises(ValueError, match=message):
        build_map3_optional_interactions(root)


@pytest.mark.parametrize(
    ("relative_path", "old", "new", "private_parser"),
    [
        (
            "data/maps/entries/map03/mapsetups/s1_entities.asm",
            "msFixedEntity 1, 2",
            "msFixedEntity 2, 2",
            _parse_entity_definitions,
        ),
        (
            "data/maps/entries/map03/mapsetups/s4_descriptions.asm",
            "msDesc 3, 4, 2, 9",
            "msDesc 4, 4, 2, 9",
            _parse_area_descriptions,
        ),
        (
            "data/maps/entries/map03/7-chest-items.asm",
            "mapItem 1, 2, 3, TEST_CHEST_ITEM",
            "mapItem 2, 2, 3, TEST_CHEST_ITEM",
            _parse_item_placements,
        ),
    ],
)
def test_private_row_mutation_is_observed_only_in_transient_parse(
    tmp_path: Path,
    relative_path: str,
    old: str,
    new: str,
    private_parser: Any,
) -> None:
    root = _synthetic_upstream(tmp_path)
    baseline = build_map3_optional_interactions(root)
    before_source = _read(root, relative_path)
    _replace(root, relative_path, old, new)
    mutated = build_map3_optional_interactions(root)

    if private_parser is _parse_entity_definitions:
        before_private = private_parser(before_source)
        after_private = private_parser(_read(root, relative_path))
    elif private_parser is _parse_area_descriptions:
        before_private = private_parser(before_source, 423)
        after_private = private_parser(_read(root, relative_path), 423)
    else:
        before_private = private_parser(
            before_source,
            source_kind="chest",
            source_path=relative_path,
        )
        after_private = private_parser(
            _read(root, relative_path),
            source_kind="chest",
            source_path=relative_path,
        )

    assert before_private != after_private
    assert mutated == baseline


def test_closed_fixture_schema_rejects_missing_extra_renamed_order_and_value_changes() -> None:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner="tracked Map 3 optional fixture")

    missing = copy.deepcopy(fixture)
    del missing["entityEventRoutes"][0]["program"]["target"]
    with pytest.raises(ValueError):
        validate_json(missing, SCHEMA, owner="missing nested field")

    extra = copy.deepcopy(fixture)
    extra["areaDescriptions"]["unexpected"] = True
    with pytest.raises(ValueError):
        validate_json(extra, SCHEMA, owner="extra nested field")

    renamed = copy.deepcopy(fixture)
    renamed["entityEventRoutes"][0]["program"]["flags"] = renamed[
        "entityEventRoutes"
    ][0]["program"].pop("flagConditions")
    with pytest.raises(ValueError):
        validate_json(renamed, SCHEMA, owner="renamed nested field")

    reversed_routes = copy.deepcopy(fixture)
    reversed_routes["entityEventRoutes"].reverse()
    with pytest.raises(ValueError):
        validate_json(reversed_routes, SCHEMA, owner="reordered corpus")

    changed_count = copy.deepcopy(fixture)
    changed_count["areaDescriptions"]["recordCount"] += 1
    with pytest.raises(ValueError):
        validate_json(changed_count, SCHEMA, owner="changed exact value")

    out_of_range_count = copy.deepcopy(fixture)
    out_of_range_count["areaDescriptions"]["recordCount"] = -1
    with pytest.raises(ValueError):
        validate_json(out_of_range_count, SCHEMA, owner="count boundary")


def test_tracked_fixture_is_canonical_and_contains_only_structural_text_references() -> None:
    fixture = load_json(FIXTURE)
    assert FIXTURE.read_bytes() == canonical_json_bytes(fixture)

    def assert_no_prose(value: Any) -> None:
        if isinstance(value, dict):
            assert "dialogue" not in value
            assert "textPayload" not in value
            for key, child in value.items():
                assert "prose" not in key.lower()
                assert_no_prose(child)
        elif isinstance(value, list):
            for child in value:
                assert_no_prose(child)

    assert_no_prose(fixture)
    assert isinstance(fixture["entityDefinitions"], dict)
    assert isinstance(fixture["areaDescriptions"], dict)
    assert isinstance(fixture["itemPlacements"], dict)
    assert {"entities", "records", "rows"}.isdisjoint(fixture["entityDefinitions"])
    assert {"descriptions", "records", "rows", "textIndices"}.isdisjoint(
        fixture["areaDescriptions"]
    )
    assert {"placements", "records", "rows"}.isdisjoint(fixture["itemPlacements"])

    private_keys = {
        "actionShape",
        "associatedFlag",
        "firstTextIndex",
        "interactionKind",
        "itemIdentifier",
        "mapSprite",
        "secondTextIndex",
        "x",
        "y",
    }

    def assert_private_keys_absent(value: Any) -> None:
        if isinstance(value, dict):
            assert private_keys.isdisjoint(value)
            for child in value.values():
                assert_private_keys_absent(child)
        elif isinstance(value, list):
            for child in value:
                assert_private_keys_absent(child)

    assert_private_keys_absent(fixture["entityDefinitions"])
    assert_private_keys_absent(fixture["areaDescriptions"])
    assert_private_keys_absent(fixture["itemPlacements"])

    def assert_no_private_fingerprint(value: Any) -> None:
        if isinstance(value, dict):
            for key, child in value.items():
                assert "digest" not in key.lower()
                assert "hash" not in key.lower()
                assert_no_private_fingerprint(child)
        elif isinstance(value, list):
            for child in value:
                assert_no_private_fingerprint(child)

    assert_no_private_fingerprint(fixture["entityDefinitions"])
    assert_no_private_fingerprint(fixture["areaDescriptions"])
    assert_no_private_fingerprint(fixture["itemPlacements"])
    assert all(
        isinstance(index, int)
        for route in fixture["entityEventRoutes"]
        for index in route["program"]["textIndices"]
    )
