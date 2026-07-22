import copy
from pathlib import Path

import pytest

from sf2tool.h2.map_events import (
    FIXTURE,
    FIXTURE_SCHEMA,
    RAW_ZONE_DEFAULT_SYMBOL,
    SCHEMA,
    _decode_event_record,
    _event_macro_use_sites,
    _join_source_rom_record,
    _macro_definition,
    _reconcile_event_reference_counts,
    _record_target_ownership,
    _setup_category_joins,
    _verify_complete_map_events_fixture,
    build_map_events_contract,
)
from sf2tool.jsonio import load_json, validate_json


def test_event_macro_definition_derives_target_position_marker_and_width() -> None:
    definition = _macro_definition(
        """
msDefaultZoneEvent: macro
    dc.b $FD
    dc.b 0
    dc.w \\1
    endm
""",
        "msDefaultZoneEvent",
        "default",
    )

    assert definition == {
        "macro": "msDefaultZoneEvent",
        "kind": "default",
        "definitionLine": 2,
        "argumentCount": 1,
        "targetOperandPosition": 1,
        "defaultMarker": 0xFD,
        "encodedRecordBytes": 4,
        "emittedDirectives": [
            {
                "sourceOrder": 0,
                "directive": "dc.b",
                "operandText": "$FD",
                "widthBytes": 1,
                "argumentPositions": [],
            },
            {
                "sourceOrder": 1,
                "directive": "dc.b",
                "operandText": "0",
                "widthBytes": 1,
                "argumentPositions": [],
            },
            {
                "sourceOrder": 2,
                "directive": "dc.w",
                "operandText": "\\1",
                "widthBytes": 2,
                "argumentPositions": [1],
            },
        ],
    }

    with pytest.raises(ValueError, match="target operand drift"):
        _macro_definition(
            """
msDefaultZoneEvent: macro
    dc.b $FD
    dc.b 0
    dc.b \\1
    dc.w \\1
    dc.w \\1
    endm
""",
            "msDefaultZoneEvent",
            "default",
        )
    with pytest.raises(ValueError, match="target directive order drift"):
        _macro_definition(
            """
msDefaultZoneEvent: macro
    dc.w \\1
    dc.b $FD
    dc.b 0
    endm
""",
            "msDefaultZoneEvent",
            "default",
        )


def test_event_macro_use_sites_ignore_comments_and_preserve_expression() -> None:
    definitions = [
        {
            "macro": "msZoneEvent",
            "kind": "specific",
            "argumentCount": 3,
            "targetOperandPosition": 3,
            "defaultMarker": None,
        },
        {
            "macro": "msDefaultZoneEvent",
            "kind": "default",
            "argumentCount": 1,
            "targetOperandPosition": 1,
            "defaultMarker": 0xFD,
        },
    ]
    sites = _event_macro_use_sites(
        """
; msZoneEvent 1, 2, Ghost-ms_map3_ZoneEvents
ms_map3_ZoneEvents: msZoneEvent 2, 3, Map3_ZoneEvent0-ms_map3_ZoneEvents
    msZoneEventX 4, 5, NearMiss-ms_map3_ZoneEvents
    msDefaultZoneEvent (Map3_DefaultZoneEvent-ms_map3_ZoneEvents) & $FFFF
""",
        category="zoneEvents",
        path="data/maps/entries/map03/mapsetups/s3_zoneevents.asm",
        table_symbol="ms_map3_ZoneEvents",
        definitions=definitions,
    )

    assert sites == [
        {
            "sourceOrder": 0,
            "sourcePath": "data/maps/entries/map03/mapsetups/s3_zoneevents.asm",
            "sourceLine": 3,
            "sourceTableSymbol": "ms_map3_ZoneEvents",
            "macro": "msZoneEvent",
            "kind": "specific",
            "operandTexts": ["2", "3", "Map3_ZoneEvent0-ms_map3_ZoneEvents"],
            "sourceDefaultMarker": None,
            "sourceMarkerWord": None,
            "targetExpression": "Map3_ZoneEvent0-ms_map3_ZoneEvents",
            "targetBaseSymbol": "Map3_ZoneEvent0",
            "targetBaseAdjustment": 0,
            "relativeBaseSymbol": "ms_map3_ZoneEvents",
            "maskedTo16Bits": False,
        },
        {
            "sourceOrder": 1,
            "sourcePath": "data/maps/entries/map03/mapsetups/s3_zoneevents.asm",
            "sourceLine": 5,
            "sourceTableSymbol": "ms_map3_ZoneEvents",
            "macro": "msDefaultZoneEvent",
            "kind": "default",
            "operandTexts": ["(Map3_DefaultZoneEvent-ms_map3_ZoneEvents) & $FFFF"],
            "sourceDefaultMarker": 0xFD,
            "sourceMarkerWord": None,
            "targetExpression": "(Map3_DefaultZoneEvent-ms_map3_ZoneEvents) & $FFFF",
            "targetBaseSymbol": "Map3_DefaultZoneEvent",
            "targetBaseAdjustment": 0,
            "relativeBaseSymbol": "ms_map3_ZoneEvents",
            "maskedTo16Bits": True,
        },
    ]

    with pytest.raises(ValueError, match="does not resolve from its table base"):
        _event_macro_use_sites(
            "msZoneEvent 2, 3, Map3_ZoneEvent0-ms_other\n",
            category="zoneEvents",
            path="synthetic.asm",
            table_symbol="ms_map3_ZoneEvents",
            definitions=definitions,
        )


def test_raw_map44_zone_default_preserves_expression_and_adjustment() -> None:
    sites = _event_macro_use_sites(
        """
ms_map44_ZoneEvents:
    dc.w $FD00
    dc.w byte_54868+4-ms_map44_ZoneEvents
""",
        category="zoneEvents",
        path="data/maps/entries/map44/mapsetups/s3_zoneevents.asm",
        table_symbol=RAW_ZONE_DEFAULT_SYMBOL,
        definitions=[],
    )

    assert sites[0]["macro"] == "raw-zone-default-expression"
    assert sites[0]["targetExpression"] == "byte_54868+4-ms_map44_ZoneEvents"
    assert sites[0]["targetBaseSymbol"] == "byte_54868"
    assert sites[0]["targetBaseAdjustment"] == 4
    assert sites[0]["relativeBaseSymbol"] == RAW_ZONE_DEFAULT_SYMBOL

    altered_marker = _event_macro_use_sites(
        """
ms_map44_ZoneEvents:
    dc.w $FC00
    dc.w byte_54868+4-ms_map44_ZoneEvents
""",
        category="zoneEvents",
        path="data/maps/entries/map44/mapsetups/s3_zoneevents.asm",
        table_symbol=RAW_ZONE_DEFAULT_SYMBOL,
        definitions=[],
    )
    assert altered_marker[0]["sourceDefaultMarker"] == 0xFC
    assert altered_marker[0]["sourceMarkerWord"] == 0xFC00


def test_source_rom_target_guard_rejects_offset_and_raw_adjustment_mutations() -> None:
    decoded = _decode_event_record("zoneEvents", 0x1000, 0x1000, b"\x01\x02\x00\x10")
    source_record = {
        "kind": "specific",
        "recordAddress": 0x1000,
        "targetBaseSymbol": "Target",
        "targetBaseAdjustment": 0,
        "sourceDefaultMarker": None,
        "sourceMarkerWord": None,
    }
    assert (
        _join_source_rom_record("zoneEvents", decoded, source_record, {"Target": 0x1010})[
            "resolvedTargetAddress"
        ]
        == 0x1010
    )

    with pytest.raises(ValueError, match="target relationship drift"):
        _join_source_rom_record(
            "zoneEvents", decoded, {**source_record, "targetBaseAdjustment": 4}, {"Target": 0x1010}
        )
    with pytest.raises(ValueError, match="target relationship drift"):
        _join_source_rom_record(
            "zoneEvents",
            _decode_event_record("zoneEvents", 0x1000, 0x1000, b"\x01\x02\x00\x0e"),
            source_record,
            {"Target": 0x1010},
        )

    with pytest.raises(ValueError, match="default marker relationship drift"):
        _join_source_rom_record(
            "zoneEvents",
            _decode_event_record("zoneEvents", 0x1000, 0x1000, b"\xfd\x00\x00\x10"),
            {
                **source_record,
                "kind": "default",
                "sourceDefaultMarker": 0xFC,
                "sourceMarkerWord": 0xFC00,
            },
            {"Target": 0x1010},
        )


def test_target_ownership_rejects_missing_and_ambiguous_exact_labels() -> None:
    record = {
        "address": 0x1000,
        "resolvedTargetAddress": 0x2000,
        "targetBaseSymbol": "Target",
        "macro": "msZoneEvent",
        "sourcePath": "data/maps/entries/map03/mapsetups/s3_zoneevents.asm",
    }
    addresses = {"Target": 0x2000}
    owner = {"symbol": "Target", "sourcePath": record["sourcePath"], "sourceLine": 8}
    resolved = _record_target_ownership(record, addresses, {0x2000: [owner]})
    assert resolved == {
        "targetCanonicalSymbol": "Target",
        "targetAddressLabels": [owner],
        "targetH1Address": 0x2000,
        "targetBaseH1Address": 0x2000,
        "targetOwnerSourcePath": record["sourcePath"],
        "targetOwnerSourceLine": 8,
        "targetOwnershipClass": "same-event-source",
    }
    with pytest.raises(ValueError, match="unresolved"):
        _record_target_ownership(record, addresses, {0x2000: []})
    with pytest.raises(ValueError, match="ambiguous"):
        _record_target_ownership(
            record,
            addresses,
            {
                0x2000: [
                    owner,
                    {"symbol": "Other", "sourcePath": "code/other.asm", "sourceLine": 2},
                ]
            },
        )


def test_route_category_joins_preserve_target_identity_and_selector_order() -> None:
    categories = {}
    for category, address in (
        ("entityEvents", 0x2000),
        ("zoneEvents", 0x2100),
        ("itemEvents", 0x2200),
    ):
        categories[category] = {
            "sourceFiles": [
                {
                    "symbol": f"ms_map3_{category}",
                    "address": address,
                    "directReturnStub": False,
                    "recordCount": 1,
                },
                {
                    "symbol": f"ms_map3_flag9_{category}",
                    "address": address + 0x20,
                    "directReturnStub": category == "entityEvents",
                    "recordCount": 0 if category == "entityEvents" else 1,
                },
            ],
            "tables": [],
        }
    targets = {
        category: {"symbol": f"ms_map3_{category}", "address": address}
        for category, address in (
            ("entityEvents", 0x2000),
            ("zoneEvents", 0x2100),
            ("itemEvents", 0x2200),
        )
    }
    flag_targets = {
        category: {"symbol": f"ms_map3_flag9_{category}", "address": address + 0x20}
        for category, address in (
            ("entityEvents", 0x2000),
            ("zoneEvents", 0x2100),
            ("itemEvents", 0x2200),
        )
    }
    setup = {
        "summary": {"routePointerReferenceCount": 2},
        "pointerTables": [
            {"symbol": "ms_map3", "address": 0x1000, "targets": targets},
            {"symbol": "ms_map3_flag9", "address": 0x1018, "targets": flag_targets},
        ],
        "routes": [
            {
                "map": 3,
                "defaultPointer": "ms_map3",
                "flagVariants": [{"flag": 9, "pointer": "ms_map3_flag9"}],
            }
        ],
    }
    setup_joins, route_joins = _setup_category_joins(setup, categories)
    assert [row["eventTableSymbol"] for row in setup_joins] == [
        "ms_map3_entityEvents",
        "ms_map3_zoneEvents",
        "ms_map3_itemEvents",
        "ms_map3_flag9_entityEvents",
        "ms_map3_flag9_zoneEvents",
        "ms_map3_flag9_itemEvents",
    ]
    assert [
        (row["routeSelectorSourceOrder"], row["selectorKind"], row["eventTableSymbol"])
        for row in route_joins
    ] == [
        (0, "default", "ms_map3_entityEvents"),
        (0, "default", "ms_map3_zoneEvents"),
        (0, "default", "ms_map3_itemEvents"),
        (1, "flag", "ms_map3_flag9_entityEvents"),
        (1, "flag", "ms_map3_flag9_zoneEvents"),
        (1, "flag", "ms_map3_flag9_itemEvents"),
    ]
    broken = copy.deepcopy(setup)
    broken["pointerTables"][1]["targets"]["zoneEvents"]["address"] = 0xDEAD
    with pytest.raises(ValueError, match="target address drift"):
        _setup_category_joins(broken, categories)


def test_complete_map_event_contract_matches_full_fixture() -> None:
    output = build_map_events_contract(
        Path("local/roms/sf2-us.bin"), Path("local/upstream/SF2DISASM")
    )
    fixture = load_json(FIXTURE)
    validate_json(output, SCHEMA, owner="map events complete output")
    validate_json(fixture, FIXTURE_SCHEMA, owner="map events complete fixture")
    _verify_complete_map_events_fixture(fixture, output)
    assert fixture["expected"] == output
    assert output["summary"] == {
        "sourceFileCount": 263,
        "setupPointerReferenceCount": 378,
        "uniqueTargetCount": 263,
        "physicalRecordCount": 1134,
        "specificPhysicalRecordCount": 873,
        "defaultPhysicalRecordCount": 261,
        "setupRecordReferenceCount": 1451,
        "specificSetupRecordReferenceCount": 1076,
        "defaultSetupRecordReferenceCount": 375,
        "directReturnStubCount": 2,
        "directReturnStubReferenceCount": 3,
        "rawDefaultExceptionCount": 1,
        "maximumTableRecordCount": 31,
        "selectionCaseCount": 9,
        "recordTargetProfileCount": 915,
        "setupCategoryJoinCount": 378,
        "routeCategoryJoinCount": 390,
        "routeSelectorReferenceCount": 130,
        "routeRecordReferenceCount": 1501,
    }
    assert output["rawZoneDefaultException"] == {
        "symbol": "ms_map44_ZoneEvents",
        "address": 345176,
        "relativeOffset": 1044,
        "resolvedTargetAddress": 346220,
        "targetExpression": "byte_54868+4-ms_map44_ZoneEvents",
        "targetBaseSymbol": "byte_54868",
        "targetBaseH1Address": 346216,
        "targetBaseAdjustment": 4,
        "targetOwnerSourcePath": "data/maps/entries/map06/mapsetups/s1_entities.asm",
        "targetOwnerSourceLine": 19,
        "pointsInsideCutsceneEntityList": True,
    }
    assert output["runtimeQuestions"] == [
        "entity-event-direct-return-stub-normal-story-route-reachability",
        "event-script-side-effects-and-transition-persistence",
        "event-portrait-facing-and-presentation-timing",
    ]


def test_map_events_schemas_reject_nested_missing_extra_order_and_boundary_mutations() -> None:
    output = build_map_events_contract(
        Path("local/roms/sf2-us.bin"), Path("local/upstream/SF2DISASM")
    )
    missing = copy.deepcopy(output)
    del missing["categories"]["entityEvents"]["tables"][0]["records"][0]["targetCanonicalSymbol"]
    with pytest.raises(ValueError):
        validate_json(missing, SCHEMA, owner="missing event target canonical symbol")

    extra = copy.deepcopy(output)
    extra["categories"]["zoneEvents"]["tables"][0]["records"][0]["unexpected"] = True
    with pytest.raises(ValueError):
        validate_json(extra, SCHEMA, owner="extra event record property")

    reordered = copy.deepcopy(output)
    reordered["physicalRecordOrder"].reverse()
    with pytest.raises(ValueError):
        validate_json(reordered, SCHEMA, owner="reordered physical record order")

    boundary = copy.deepcopy(output)
    boundary["routeCategoryJoins"][0]["pointerTableAddress"] = -1
    with pytest.raises(ValueError):
        validate_json(boundary, SCHEMA, owner="negative route pointer address")

    fixture = load_json(FIXTURE)
    semantic = copy.deepcopy(output)
    semantic["recordTargetProfiles"][0]["canonicalSymbol"] = "wrong-owner"
    validate_json(semantic, SCHEMA, owner="wrong-shape-valid output target owner")
    with pytest.raises(ValueError, match="complete semantic fixture drift"):
        _verify_complete_map_events_fixture(fixture, semantic)

    renamed = copy.deepcopy(fixture)
    record = renamed["expected"]["categories"]["itemEvents"]["tables"][0]["records"][0]
    record["renamedTargetCanonicalSymbol"] = record.pop("targetCanonicalSymbol")
    with pytest.raises(ValueError):
        validate_json(renamed, FIXTURE_SCHEMA, owner="renamed fixture target field")

    fixture_extra = copy.deepcopy(fixture)
    fixture_extra["expected"]["recordTargetProfiles"][0]["unexpected"] = 1
    with pytest.raises(ValueError):
        validate_json(fixture_extra, FIXTURE_SCHEMA, owner="extra fixture target profile field")

    fixture_semantic = copy.deepcopy(fixture)
    fixture_semantic["expected"]["recordTargetProfiles"][0]["canonicalSymbol"] = "wrong-owner"
    validate_json(fixture_semantic, FIXTURE_SCHEMA, owner="wrong-shape-valid fixture target owner")
    with pytest.raises(ValueError, match="complete semantic fixture drift"):
        _verify_complete_map_events_fixture(fixture_semantic, output)

    fixture_order = copy.deepcopy(fixture)
    fixture_order["expected"]["routeCategoryJoinOrder"].reverse()
    with pytest.raises(ValueError):
        validate_json(fixture_order, FIXTURE_SCHEMA, owner="reordered fixture route joins")

    fixture_boundary = copy.deepcopy(fixture)
    fixture_boundary["expected"]["categories"]["zoneEvents"]["tables"][0]["records"][0]["x"] = -1
    with pytest.raises(ValueError):
        validate_json(fixture_boundary, FIXTURE_SCHEMA, owner="negative fixture coordinate")


def test_reference_reconciliation_rejects_profile_and_category_counter_mutations() -> None:
    output = build_map_events_contract(
        Path("local/roms/sf2-us.bin"), Path("local/upstream/SF2DISASM")
    )
    broken_profile = copy.deepcopy(output)
    broken_profile["recordTargetProfiles"][0]["routeRecordReferenceCount"] += 1
    with pytest.raises(ValueError, match="target profile weighted-count drift"):
        _reconcile_event_reference_counts(
            broken_profile["categories"],
            broken_profile["recordTargetProfiles"],
            broken_profile["setupCategoryJoins"],
            broken_profile["routeCategoryJoins"],
            broken_profile["summary"],
        )

    broken_category = copy.deepcopy(output)
    broken_category["categories"]["zoneEvents"]["summary"]["routeRecordReferenceCount"] += 1
    with pytest.raises(ValueError, match="category reconciliation drift"):
        _reconcile_event_reference_counts(
            broken_category["categories"],
            broken_category["recordTargetProfiles"],
            broken_category["setupCategoryJoins"],
            broken_category["routeCategoryJoins"],
            broken_category["summary"],
        )


def test_map_events_schema_size_stays_compact_and_reuses_closed_shapes() -> None:
    assert SCHEMA.stat().st_size < 1_000_000
    assert FIXTURE_SCHEMA.stat().st_size < 1_000_000
    schema = load_json(SCHEMA)
    assert set(schema["definitions"]) == {
        "entityEventRecord",
        "zoneEventRecord",
        "itemEventRecord",
    }
    for category, definition in (
        ("entityEvents", "entityEventRecord"),
        ("zoneEvents", "zoneEventRecord"),
        ("itemEvents", "itemEventRecord"),
    ):
        records = schema["properties"]["categories"]["properties"][category]["properties"][
            "tables"
        ]["items"]["properties"]["records"]
        assert records["items"] == {"$ref": f"#/definitions/{definition}"}
