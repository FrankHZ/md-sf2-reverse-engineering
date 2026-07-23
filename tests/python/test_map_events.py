import copy
from pathlib import Path
from typing import Any

import pytest
from jsonschema import Draft7Validator, FormatChecker

from sf2tool.h2.map_events import (
    FIXTURE,
    FIXTURE_SCHEMA,
    RAW_ZONE_DEFAULT_SYMBOL,
    SCHEMA,
    _bind_operations_to_h1,
    _decode_event_record,
    _derived_action_payload_context_specs,
    _event_macro_use_sites,
    _guard_macro_emission,
    _h1_program_index,
    _join_source_rom_record,
    _listing_statement,
    _macro_definition,
    _normalise_asm_statement,
    _parse_jump_interface_aliases,
    _parse_program_operation,
    _payload_context_contract,
    _reconcile_event_reference_counts,
    _reconcile_operation_weight_contract,
    _record_target_ownership,
    _setup_category_joins,
    _source_macro_catalog,
    _target_program_contract,
    _verify_complete_map_events_fixture,
    build_map_events_contract,
)
from sf2tool.jsonio import load_json, validate_json


@pytest.fixture(scope="module")
def complete_output() -> dict[str, Any]:
    """Build the complete static contract once; mutation tests receive deep copies."""
    return build_map_events_contract(
        Path("local/roms/sf2-us.bin"), Path("local/upstream/SF2DISASM")
    )


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


def test_source_macro_emission_guard_rejects_definition_opcode_and_operand_order(
    tmp_path: Path,
) -> None:
    (tmp_path / "sf2macros.asm").write_text(
        "\n".join(
            (
                "; ignored: macro",
                "service: macro",
                "    trap #SERVICE ; source comment",
                "    dc.w \\1",
                "endm",
                "pair: macro",
                "    dc.b \\1",
                "    dc.b \\2",
                "endm",
                "",
            )
        ),
        encoding="utf-8",
    )
    catalog = _source_macro_catalog(tmp_path, (Path("sf2macros.asm"),))
    assert set(catalog) == {"service", "pair"}

    service_listing = [
        "00001000                            service 7",
        "00001000 4E4F                     M  trap #SERVICE",
        "00001002 0007                     M  dc.w 7",
        "00001004                            rts",
    ]
    service_operation = {
        "sourceMnemonic": "service",
        "operandTexts": ["7"],
        "sourceLine": 2,
        "_h1ListingSourceIndex": 0,
        "address": 0x1000,
    }
    _guard_macro_emission(
        service_listing,
        catalog,
        operation=service_operation,
        next_address=0x1004,
    )
    changed_opcode = copy.deepcopy(catalog)
    changed_opcode["service"]["body"][1] = (4, "dc.b \\1")
    with pytest.raises(ValueError, match="emission statement/order drift"):
        _guard_macro_emission(
            service_listing,
            changed_opcode,
            operation=service_operation,
            next_address=0x1004,
        )

    pair_listing = [
        "00002000                            pair 1,2",
        "00002000 01                       M  dc.b 1",
        "00002001 02                       M  dc.b 2",
        "00002002                            rts",
    ]
    pair_operation = {
        "sourceMnemonic": "pair",
        "operandTexts": ["1", "2"],
        "sourceLine": 6,
        "_h1ListingSourceIndex": 0,
        "address": 0x2000,
    }
    _guard_macro_emission(
        pair_listing,
        catalog,
        operation=pair_operation,
        next_address=0x2002,
    )
    changed_order = copy.deepcopy(catalog)
    changed_order["pair"]["body"] = [(7, "dc.b \\2"), (8, "dc.b \\1")]
    with pytest.raises(ValueError, match="emission statement/order drift"):
        _guard_macro_emission(
            pair_listing,
            changed_order,
            operation=pair_operation,
            next_address=0x2002,
        )


def test_payload_context_parser_preserves_inherited_and_nested_action_payloads(
    tmp_path: Path,
) -> None:
    source_path = Path("data/maps/entries/map44/mapsetups/scripts.asm")
    source_file = tmp_path / source_path
    source_file.parent.mkdir(parents=True)
    source_file.write_text(
        "\n".join(
            (
                "; entityActions ignored",
                "entityActions ACTOR",
                "moveRight 1",
                "customActscriptWait ACTOR",
                "ac_setSpeed 20,20",
                "ac_end",
                "endActions",
                "",
            )
        ),
        encoding="utf-8",
    )
    operations = [{"sourceLine": line} for line in range(3, 8)]
    program = {
        "sourcePath": source_path.as_posix(),
        "entrySourceLine": 3,
        "operations": operations,
    }
    contexts, payload_macro_families = _payload_context_contract(
        tmp_path,
        {"zoneEvents": [program], "entityEvents": [], "itemEvents": []},
        payload_context_specs={
            "entityActions": {
                "contextFamily": "entity-action-payload",
                "terminatorMnemonic": "endActions",
            },
            "customActscriptWait": {
                "contextFamily": "entity-action-command-payload",
                "terminatorMnemonic": "ac_end",
            },
        },
        action_command_macros={"ac_setSpeed", "ac_end"},
    )
    first_context, second_context = contexts
    assert first_context == {
        "contextId": "data/maps/entries/map44/mapsetups/scripts.asm:2:entity-action-payload",
        "sourcePath": "data/maps/entries/map44/mapsetups/scripts.asm",
        "openerSourceLine": 2,
        "openerSourceMnemonic": "entityActions",
        "contextFamily": "entity-action-payload",
        "parentContextId": None,
        "terminatorMnemonic": "endActions",
        "terminatorSourceLine": 7,
    }
    assert second_context == {
        "contextId": (
            "data/maps/entries/map44/mapsetups/scripts.asm:4:entity-action-command-payload"
        ),
        "sourcePath": "data/maps/entries/map44/mapsetups/scripts.asm",
        "openerSourceLine": 4,
        "openerSourceMnemonic": "customActscriptWait",
        "contextFamily": "entity-action-command-payload",
        "parentContextId": first_context["contextId"],
        "terminatorMnemonic": "ac_end",
        "terminatorSourceLine": 6,
    }
    assert program["inheritedPayloadContextIds"] == [first_context["contextId"]]
    assert operations[0]["payloadContextIds"] == [first_context["contextId"]]
    assert operations[2]["payloadContextIds"] == [
        first_context["contextId"],
        second_context["contextId"],
    ]
    assert operations[3]["payloadContextIds"] == [
        first_context["contextId"],
        second_context["contextId"],
    ]
    assert operations[4]["payloadContextIds"] == [first_context["contextId"]]
    assert payload_macro_families == {
        "moveRight": "entity-action-payload-command",
        "endActions": "entity-action-payload-command",
    }


def test_payload_context_specs_guard_alias_handler_cursor_and_terminator_evidence(
    tmp_path: Path,
) -> None:
    (tmp_path / "macros.asm").write_text(
        """
csc14: macro
    dc.w $14
    dc.b \\1
    dc.b \\2
endm
customActscript: macro
    csc14 \\1,0
endm
customActscriptWait: macro
    csc14 \\1,$FF
endm
csc2D: macro
    dc.w $2D
    dc.b \\1
    dc.b \\2
endm
entityActions: macro
    csc2D \\1,0
endm
entityActionsWait: macro
    csc2D \\1,$FF
endm
endActions: macro
    dc.w $8080
endm
cscNop: macro
endm
csc_end: macro
    dc.w $FFFF
endm
ac_end: macro
    dc.w $8080
endm
""".lstrip(),
        encoding="utf-8",
    )
    engine_source = """
csc2D_handler:
    move.b (a6)+,d1
    bmi.w csc2D_end
    move.b (a6)+,d2
    rts
; START OF FUNCTION CHUNK FOR csc2D_handler
csc2D_end:
    addq.l #1,a6
    rts
; END OF FUNCTION CHUNK FOR csc2D_handler
csc14_handler:
    cmpi.w #$8080,(a6)+
    bne.s csc14_continue
    rts
csc14_continue:
    rts
""".lstrip()
    (tmp_path / "engine.asm").write_text(engine_source, encoding="utf-8")
    catalog = _source_macro_catalog(tmp_path, (Path("macros.asm"),))
    map_engine = {
        "macroContracts": {
            "csc14": {"kind": "command", "aliasOf": None},
            "customActscript": {"kind": "command", "aliasOf": "csc14"},
            "customActscriptWait": {"kind": "command", "aliasOf": "csc14"},
            "csc2D": {"kind": "command", "aliasOf": None},
            "entityActions": {"kind": "command", "aliasOf": "csc2D"},
            "entityActionsWait": {"kind": "command", "aliasOf": "csc2D"},
            "csc_end": {"kind": "terminator", "aliasOf": None},
        },
        "handlers": [
            {
                "name": "csc2D_handler",
                "macroNames": ["csc2D", "entityActions", "entityActionsWait"],
                "cursorFlow": "sequential",
                "sourcePath": "engine.asm",
                "startLine": 1,
                "endLine": 5,
            },
            {
                "name": "csc14_handler",
                "macroNames": ["csc14", "customActscript", "customActscriptWait"],
                "cursorFlow": "inline-action-program",
                "sourcePath": "engine.asm",
                "startLine": 11,
                "endLine": 16,
            },
        ],
    }
    entity_actions = {
        "handlerFacts": {"inlineTerminatorMacro": "ac_end", "inlineTerminatorWord": 0x8080},
        "handlerMacroBindings": [
            {"macro": "ac_end", "isInlineTerminator": True, "opcode": 0x8080}
        ],
    }
    assert _derived_action_payload_context_specs(
        tmp_path, catalog, map_engine, entity_actions
    ) == {
        "customActscript": {
            "contextFamily": "entity-action-command-payload",
            "terminatorMnemonic": "ac_end",
        },
        "customActscriptWait": {
            "contextFamily": "entity-action-command-payload",
            "terminatorMnemonic": "ac_end",
        },
        "entityActions": {
            "contextFamily": "entity-action-payload",
            "terminatorMnemonic": "endActions",
        },
        "entityActionsWait": {
            "contextFamily": "entity-action-payload",
            "terminatorMnemonic": "endActions",
        },
    }

    broken_alias = copy.deepcopy(map_engine)
    broken_alias["macroContracts"]["entityActions"]["aliasOf"] = "csc14"
    with pytest.raises(ValueError, match="alias identity"):
        _derived_action_payload_context_specs(tmp_path, catalog, broken_alias, entity_actions)

    broken_handler = copy.deepcopy(map_engine)
    broken_handler["handlers"][0]["macroNames"].remove("entityActions")
    with pytest.raises(ValueError, match="alias identity"):
        _derived_action_payload_context_specs(tmp_path, catalog, broken_handler, entity_actions)

    broken_cursor_flow = copy.deepcopy(map_engine)
    broken_cursor_flow["handlers"][1]["cursorFlow"] = "sequential"
    with pytest.raises(ValueError, match="cursor-flow"):
        _derived_action_payload_context_specs(tmp_path, catalog, broken_cursor_flow, entity_actions)

    broken_terminator = copy.deepcopy(catalog)
    broken_terminator["endActions"]["body"] = [(20, "dc.w $0000")]
    with pytest.raises(ValueError, match="terminator definition"):
        _derived_action_payload_context_specs(
            tmp_path, broken_terminator, map_engine, entity_actions
        )

    (tmp_path / "engine.asm").write_text(
        engine_source.replace(
            "bne.s csc14_continue\n    rts\ncsc14_continue:\n    rts",
            "bne.s csc14_continue\n    bra.s csc14_continue\ncsc14_continue:\n    rts",
        ),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="terminator-use"):
        _derived_action_payload_context_specs(tmp_path, catalog, map_engine, entity_actions)

    (tmp_path / "engine.asm").write_text(
        engine_source.replace(
            "csc2D_end:\n    addq.l #1,a6\n    rts",
            "csc2D_end:\n    rts\ncsc2D_after:\n    addq.l #1,a6",
        ),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="terminator-use"):
        _derived_action_payload_context_specs(tmp_path, catalog, map_engine, entity_actions)

    (tmp_path / "engine.asm").write_text(
        engine_source.replace("bmi.w", "bpl.w"),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="terminator-use"):
        _derived_action_payload_context_specs(tmp_path, catalog, map_engine, entity_actions)


def test_complete_map_event_contract_matches_full_fixture(complete_output: dict[str, Any]) -> None:
    output = complete_output
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
    for field in (
        "operationVocabularySummary",
        "operationDefinitions",
        "operationDefinitionOrder",
        "operationPayloadContexts",
        "operationPayloadContextOrder",
        "operationVocabulary",
        "operationVocabularyOrder",
        "operationFamilyOrder",
        "operationFamilyCounts",
        "operationFamilyCountOrder",
        "entityTargetProgramOperationWeightOrders",
        "entityTargetProgramPayloadContextOrders",
        "zoneTargetProgramOperationWeightOrders",
        "zoneTargetProgramPayloadContextOrders",
        "itemTargetProgramOperationWeightOrders",
        "itemTargetProgramPayloadContextOrders",
    ):
        assert output[field] == fixture["expected"][field]
    assert len(output["operationVocabulary"]) == 54
    assert len(output["operationDefinitions"]) == 34
    assert output["operationVocabularySummary"] == {
        "uniqueMnemonicCount": 54,
        "definitionJoinCount": 34,
        "unclassifiedOperationCount": 0,
        "ambiguousMnemonicFamilyDefinitionCount": 0,
        "categoryPhysicalOperationCounts": {
            "entityEvents": 2624,
            "zoneEvents": 809,
            "itemEvents": 146,
        },
        "definitionJoinCounts": {
            "data-directive": 0,
            "entity-action-command": 3,
            "entity-action-payload-command": 5,
            "entity-action-wrapper": 2,
            "event-service-macro": 7,
            "map-script-macro": 16,
            "raw-68000-control-flow": 0,
            "raw-68000-instruction": 0,
            "stream-terminator": 1,
        },
        "weightCounts": {
            "uniquePhysicalOperationCount": 3579,
            "physicalRecordWeightedOperationCount": 5115,
            "setupRecordReferenceWeightedOperationCount": 6911,
            "routeRecordReferenceWeightedOperationCount": 7377,
        },
    }
    assert output["operationFamilyCounts"] == [
        {
            "family": "data-directive",
            "categoryOperationCounts": {"entityEvents": 0, "zoneEvents": 2, "itemEvents": 0},
            "weightCounts": {
                "uniquePhysicalOperationCount": 2,
                "physicalRecordWeightedOperationCount": 2,
                "setupRecordReferenceWeightedOperationCount": 2,
                "routeRecordReferenceWeightedOperationCount": 2,
            },
        },
        {
            "family": "entity-action-command",
            "categoryOperationCounts": {"entityEvents": 0, "zoneEvents": 3, "itemEvents": 0},
            "weightCounts": {
                "uniquePhysicalOperationCount": 3,
                "physicalRecordWeightedOperationCount": 3,
                "setupRecordReferenceWeightedOperationCount": 12,
                "routeRecordReferenceWeightedOperationCount": 12,
            },
        },
        {
            "family": "entity-action-payload-command",
            "categoryOperationCounts": {"entityEvents": 0, "zoneEvents": 16, "itemEvents": 0},
            "weightCounts": {
                "uniquePhysicalOperationCount": 16,
                "physicalRecordWeightedOperationCount": 16,
                "setupRecordReferenceWeightedOperationCount": 64,
                "routeRecordReferenceWeightedOperationCount": 64,
            },
        },
        {
            "family": "entity-action-wrapper",
            "categoryOperationCounts": {"entityEvents": 0, "zoneEvents": 3, "itemEvents": 0},
            "weightCounts": {
                "uniquePhysicalOperationCount": 3,
                "physicalRecordWeightedOperationCount": 3,
                "setupRecordReferenceWeightedOperationCount": 12,
                "routeRecordReferenceWeightedOperationCount": 12,
            },
        },
        {
            "family": "event-service-macro",
            "categoryOperationCounts": {"entityEvents": 1303, "zoneEvents": 318, "itemEvents": 28},
            "weightCounts": {
                "uniquePhysicalOperationCount": 1649,
                "physicalRecordWeightedOperationCount": 2333,
                "setupRecordReferenceWeightedOperationCount": 3102,
                "routeRecordReferenceWeightedOperationCount": 3366,
            },
        },
        {
            "family": "map-script-macro",
            "categoryOperationCounts": {"entityEvents": 0, "zoneEvents": 64, "itemEvents": 0},
            "weightCounts": {
                "uniquePhysicalOperationCount": 64,
                "physicalRecordWeightedOperationCount": 64,
                "setupRecordReferenceWeightedOperationCount": 256,
                "routeRecordReferenceWeightedOperationCount": 256,
            },
        },
        {
            "family": "raw-68000-control-flow",
            "categoryOperationCounts": {"entityEvents": 1138, "zoneEvents": 332, "itemEvents": 99},
            "weightCounts": {
                "uniquePhysicalOperationCount": 1569,
                "physicalRecordWeightedOperationCount": 2260,
                "setupRecordReferenceWeightedOperationCount": 2963,
                "routeRecordReferenceWeightedOperationCount": 3161,
            },
        },
        {
            "family": "raw-68000-instruction",
            "categoryOperationCounts": {"entityEvents": 183, "zoneEvents": 70, "itemEvents": 19},
            "weightCounts": {
                "uniquePhysicalOperationCount": 272,
                "physicalRecordWeightedOperationCount": 433,
                "setupRecordReferenceWeightedOperationCount": 496,
                "routeRecordReferenceWeightedOperationCount": 500,
            },
        },
        {
            "family": "stream-terminator",
            "categoryOperationCounts": {"entityEvents": 0, "zoneEvents": 1, "itemEvents": 0},
            "weightCounts": {
                "uniquePhysicalOperationCount": 1,
                "physicalRecordWeightedOperationCount": 1,
                "setupRecordReferenceWeightedOperationCount": 4,
                "routeRecordReferenceWeightedOperationCount": 4,
            },
        },
    ]
    assert output["entityTargetProgramSummary"] == {
        "programCount": 684,
        "sourceFileCount": 87,
        "labelCount": 1015,
        "operationCount": 2624,
        "ordinaryOperationCount": 1486,
        "conditionalBranchCount": 208,
        "unconditionalBranchCount": 147,
        "directCallCount": 96,
        "directJumpCount": 61,
        "returnCount": 626,
        "encodedSpanBytes": 8928,
        "physicalRecordCount": 850,
        "setupRecordReferenceCount": 998,
        "routeRecordReferenceCount": 1031,
        "internalControlFlowSiteCount": 355,
        "externalControlFlowSiteCount": 157,
        "instructionTargetCount": 332,
        "effectiveTargetCount": 332,
        "jumpInterfaceAliasCount": 9,
    }
    assert output["entityTargetPrograms"][0] == {
        "programOrder": 0,
        "canonicalSymbol": "Map0_DefaultEntityEvent",
        "entryAddress": 385954,
        "sourcePath": "data/maps/entries/map00/mapsetups/s2_entityevents.asm",
        "entrySourceLine": 10,
        "endFunctionSymbol": "Map0_DefaultEntityEvent",
        "endSourceLine": 14,
        "endAddressExclusive": 385956,
        "encodedSpanBytes": 2,
        "referenceCounts": {
            "physicalRecordCount": 1,
            "setupRecordReferenceCount": 1,
            "routeRecordReferenceCount": 1,
        },
        "operationWeightCounts": {
            "uniquePhysicalOperationCount": 1,
            "physicalRecordWeightedOperationCount": 1,
            "setupRecordReferenceWeightedOperationCount": 1,
            "routeRecordReferenceWeightedOperationCount": 1,
        },
        "payloadContextIds": [],
        "inheritedPayloadContextIds": [],
        "labels": [
            {
                "sourceOrder": 0,
                "sourceLine": 10,
                "symbol": "Map0_DefaultEntityEvent",
                "address": 385954,
            }
        ],
        "operations": [
            {
                "sourceOrder": 0,
                "sourceLine": 12,
                "sourceMnemonic": "rts",
                "mnemonic": "rts",
                "sizeSuffix": None,
                "operandTexts": [],
                "controlFlowKind": "return",
                "address": 385954,
                "target": None,
                "family": "raw-68000-control-flow",
                "definitionId": None,
                "payloadContextIds": [],
            }
        ],
        "termination": {
            "sourceOrder": 0,
            "sourceLine": 12,
            "sourceMnemonic": "rts",
            "mnemonic": "rts",
            "sizeSuffix": None,
            "operandTexts": [],
            "controlFlowKind": "return",
            "address": 385954,
            "target": None,
            "family": "raw-68000-control-flow",
            "definitionId": None,
            "payloadContextIds": [],
        },
    }
    control_flow = output["entityTargetProgramControlFlow"]
    assert output["entityTargetProgramControlFlowTargetOrders"]["aliasOrder"] == [
        "j_ShopMenu",
        "j_ChurchMenu",
        "j_YesNoPrompt",
        "j_CaravanMenu",
        "j_ClosePortraitWindow",
        "j_ClosePortraitEyes",
        "j_BlacksmithMenu",
        "j_NameAlly",
        "j_GetItemInventoryLocation",
    ]
    for identity in ("instructionTargets", "effectiveTargets"):
        assert {
            scope: sum(
                row["totalSiteCount"] for row in control_flow["targetTotals"][identity][scope]
            )
            for scope in ("internal", "external")
        } == {"internal": 355, "external": 157}
    assert output["zoneTargetProgramSummary"] == {
        "programCount": 150,
        "sourceFileCount": 76,
        "labelCount": 251,
        "operationCount": 809,
        "ordinaryOperationCount": 477,
        "conditionalBranchCount": 123,
        "unconditionalBranchCount": 18,
        "directCallCount": 41,
        "directJumpCount": 1,
        "returnCount": 149,
        "encodedSpanBytes": 2934,
        "physicalRecordCount": 201,
        "setupRecordReferenceCount": 309,
        "routeRecordReferenceCount": 322,
        "internalControlFlowSiteCount": 141,
        "externalControlFlowSiteCount": 42,
        "instructionTargetCount": 119,
        "effectiveTargetCount": 119,
        "jumpInterfaceAliasCount": 7,
        "profileCount": 151,
        "explicitNonProgramExclusionCount": 1,
        "functionEndBoundaryCount": 149,
        "sourceStreamTerminatorCount": 1,
        "excludedPhysicalRecordCount": 1,
        "excludedSetupRecordReferenceCount": 4,
        "excludedRouteRecordReferenceCount": 4,
    }
    assert output["itemTargetProgramSummary"] == {
        "programCount": 80,
        "sourceFileCount": 73,
        "labelCount": 94,
        "operationCount": 146,
        "ordinaryOperationCount": 47,
        "conditionalBranchCount": 9,
        "unconditionalBranchCount": 4,
        "directCallCount": 6,
        "directJumpCount": 0,
        "returnCount": 80,
        "encodedSpanBytes": 414,
        "physicalRecordCount": 82,
        "setupRecordReferenceCount": 140,
        "routeRecordReferenceCount": 144,
        "internalControlFlowSiteCount": 13,
        "externalControlFlowSiteCount": 6,
        "instructionTargetCount": 18,
        "effectiveTargetCount": 18,
        "jumpInterfaceAliasCount": 2,
        "profileCount": 80,
        "explicitNonProgramExclusionCount": 0,
        "functionEndBoundaryCount": 80,
        "sourceStreamTerminatorCount": 0,
        "excludedPhysicalRecordCount": 0,
        "excludedSetupRecordReferenceCount": 0,
        "excludedRouteRecordReferenceCount": 0,
    }
    zone_stream = next(
        program
        for program in output["zoneTargetPrograms"]
        if program["canonicalSymbol"] == "Map21_DefaultZoneEvent"
    )
    assert {
        field: zone_stream[field]
        for field in (
            "programOrder",
            "canonicalSymbol",
            "entryAddress",
            "sourcePath",
            "entrySourceLine",
            "endFunctionSymbol",
            "endSourceLine",
            "endAddressExclusive",
            "encodedSpanBytes",
            "referenceCounts",
        )
    } == {
        "programOrder": 43,
        "canonicalSymbol": "Map21_DefaultZoneEvent",
        "entryAddress": 345526,
        "sourcePath": "data/maps/entries/map44/mapsetups/scripts.asm",
        "entrySourceLine": 24,
        "endFunctionSymbol": None,
        "endSourceLine": 111,
        "endAddressExclusive": 345876,
        "encodedSpanBytes": 350,
        "referenceCounts": {
            "physicalRecordCount": 1,
            "setupRecordReferenceCount": 4,
            "routeRecordReferenceCount": 4,
        },
    }
    assert zone_stream["operationWeightCounts"] == {
        "uniquePhysicalOperationCount": 87,
        "physicalRecordWeightedOperationCount": 87,
        "setupRecordReferenceWeightedOperationCount": 348,
        "routeRecordReferenceWeightedOperationCount": 348,
    }
    assert zone_stream["payloadContextIds"] == [
        "data/maps/entries/map44/mapsetups/scripts.asm:22:entity-action-payload",
        "data/maps/entries/map44/mapsetups/scripts.asm:65:entity-action-payload",
        "data/maps/entries/map44/mapsetups/scripts.asm:80:entity-action-command-payload",
        "data/maps/entries/map44/mapsetups/scripts.asm:84:entity-action-payload",
    ]
    assert zone_stream["inheritedPayloadContextIds"] == [
        "data/maps/entries/map44/mapsetups/scripts.asm:22:entity-action-payload"
    ]
    assert [
        {
            field: operation[field]
            for field in (
                "sourceLine",
                "sourceMnemonic",
                "family",
                "definitionId",
                "payloadContextIds",
            )
        }
        for operation in zone_stream["operations"]
        if operation["sourceLine"] in {25, 29, 65, 66, 68, 80, 81, 82, 83, 84, 92}
    ] == [
        {
            "sourceLine": 25,
            "sourceMnemonic": "moveDown",
            "family": "entity-action-payload-command",
            "definitionId": "entity-action-payload-command:moveDown",
            "payloadContextIds": [
                "data/maps/entries/map44/mapsetups/scripts.asm:22:entity-action-payload"
            ],
        },
        {
            "sourceLine": 29,
            "sourceMnemonic": "endActions",
            "family": "entity-action-payload-command",
            "definitionId": "entity-action-payload-command:endActions",
            "payloadContextIds": [
                "data/maps/entries/map44/mapsetups/scripts.asm:22:entity-action-payload"
            ],
        },
        {
            "sourceLine": 65,
            "sourceMnemonic": "entityActions",
            "family": "entity-action-wrapper",
            "definitionId": "entity-action-wrapper:entityActions",
            "payloadContextIds": [],
        },
        {
            "sourceLine": 66,
            "sourceMnemonic": "moveUp",
            "family": "entity-action-payload-command",
            "definitionId": "entity-action-payload-command:moveUp",
            "payloadContextIds": [
                "data/maps/entries/map44/mapsetups/scripts.asm:65:entity-action-payload"
            ],
        },
        {
            "sourceLine": 68,
            "sourceMnemonic": "endActions",
            "family": "entity-action-payload-command",
            "definitionId": "entity-action-payload-command:endActions",
            "payloadContextIds": [
                "data/maps/entries/map44/mapsetups/scripts.asm:65:entity-action-payload"
            ],
        },
        {
            "sourceLine": 80,
            "sourceMnemonic": "customActscriptWait",
            "family": "entity-action-wrapper",
            "definitionId": "entity-action-wrapper:customActscriptWait",
            "payloadContextIds": [],
        },
        {
            "sourceLine": 81,
            "sourceMnemonic": "ac_setSpeed",
            "family": "entity-action-command",
            "definitionId": "entity-action-command:ac_setSpeed",
            "payloadContextIds": [
                "data/maps/entries/map44/mapsetups/scripts.asm:80:entity-action-command-payload"
            ],
        },
        {
            "sourceLine": 82,
            "sourceMnemonic": "ac_jump",
            "family": "entity-action-command",
            "definitionId": "entity-action-command:ac_jump",
            "payloadContextIds": [
                "data/maps/entries/map44/mapsetups/scripts.asm:80:entity-action-command-payload"
            ],
        },
        {
            "sourceLine": 83,
            "sourceMnemonic": "ac_end",
            "family": "entity-action-command",
            "definitionId": "entity-action-command:ac_end",
            "payloadContextIds": [
                "data/maps/entries/map44/mapsetups/scripts.asm:80:entity-action-command-payload"
            ],
        },
        {
            "sourceLine": 84,
            "sourceMnemonic": "entityActions",
            "family": "entity-action-wrapper",
            "definitionId": "entity-action-wrapper:entityActions",
            "payloadContextIds": [],
        },
        {
            "sourceLine": 92,
            "sourceMnemonic": "endActions",
            "family": "entity-action-payload-command",
            "definitionId": "entity-action-payload-command:endActions",
            "payloadContextIds": [
                "data/maps/entries/map44/mapsetups/scripts.asm:84:entity-action-payload"
            ],
        },
    ]
    assert zone_stream["termination"] == {
        "sourceOrder": 86,
        "sourceLine": 111,
        "address": 345874,
        "sourceMnemonic": "csc_end",
        "mnemonic": "csc_end",
        "sizeSuffix": None,
        "operandTexts": [],
        "controlFlowKind": "ordinary",
        "target": None,
        "family": "stream-terminator",
        "definitionId": "stream-terminator:csc_end",
        "payloadContextIds": [],
    }
    assert output["zoneTargetProgramExclusions"] == [
        {
            "exclusionOrder": 0,
            "canonicalSymbol": "raw-map44-zone-default-expression-boundary",
            "targetAddress": 346220,
            "targetH1Address": None,
            "targetBaseH1Address": 346216,
            "targetAddressLabels": [],
            "sourcePath": "data/maps/entries/map06/mapsetups/s1_entities.asm",
            "sourceLine": 19,
            "ownershipClass": "raw-expression-boundary",
            "referenceCounts": {
                "physicalRecordCount": 1,
                "setupRecordReferenceCount": 4,
                "routeRecordReferenceCount": 4,
            },
        }
    ]
    for control_flow, expected_sites in (
        (output["zoneTargetProgramControlFlow"], {"internal": 141, "external": 42}),
        (output["itemTargetProgramControlFlow"], {"internal": 13, "external": 6}),
    ):
        for identity in ("instructionTargets", "effectiveTargets"):
            assert {
                scope: sum(
                    row["totalSiteCount"] for row in control_flow["targetTotals"][identity][scope]
                )
                for scope in ("internal", "external")
            } == expected_sites


def test_map_events_schemas_reject_nested_missing_extra_order_and_boundary_mutations(
    complete_output: dict[str, Any],
) -> None:
    output = complete_output
    output_validator = Draft7Validator(load_json(SCHEMA), format_checker=FormatChecker())
    fixture_validator = Draft7Validator(load_json(FIXTURE_SCHEMA), format_checker=FormatChecker())

    def output_rejects(instance: dict[str, Any]) -> None:
        assert next(output_validator.iter_errors(instance), None) is not None

    def fixture_rejects(instance: dict[str, Any]) -> None:
        assert next(fixture_validator.iter_errors(instance), None) is not None

    missing = copy.deepcopy(output)
    del missing["categories"]["entityEvents"]["tables"][0]["records"][0]["targetCanonicalSymbol"]
    output_rejects(missing)

    extra = copy.deepcopy(output)
    extra["categories"]["zoneEvents"]["tables"][0]["records"][0]["unexpected"] = True
    output_rejects(extra)

    reordered = copy.deepcopy(output)
    reordered["physicalRecordOrder"].reverse()
    output_rejects(reordered)

    boundary = copy.deepcopy(output)
    boundary["routeCategoryJoins"][0]["pointerTableAddress"] = -1
    output_rejects(boundary)

    operation_definition_missing = copy.deepcopy(output)
    del operation_definition_missing["operationDefinitions"][0]["emissionStatementTemplates"]
    output_rejects(operation_definition_missing)

    operation_definition_extra = copy.deepcopy(output)
    operation_definition_extra["operationDefinitions"][0]["engineCatalog"]["unexpected"] = True
    output_rejects(operation_definition_extra)

    operation_vocabulary_reordered = copy.deepcopy(output)
    operation_vocabulary_reordered["operationVocabulary"].reverse()
    output_rejects(operation_vocabulary_reordered)

    operation_definition_boundary = copy.deepcopy(output)
    operation_definition_boundary["operationDefinitions"][0]["definitionSourceLine"] = 0
    output_rejects(operation_definition_boundary)

    operation_family_mutation = copy.deepcopy(output)
    operation_family_mutation["entityTargetPrograms"][0]["operations"][0]["family"] = (
        "raw-68000-instruction"
    )
    output_rejects(operation_family_mutation)

    fixture = load_json(FIXTURE)
    semantic = copy.deepcopy(output)
    semantic["recordTargetProfiles"][0]["canonicalSymbol"] = "wrong-owner"
    validate_json(semantic, SCHEMA, owner="wrong-shape-valid output target owner")
    with pytest.raises(ValueError, match="complete semantic fixture drift"):
        _verify_complete_map_events_fixture(fixture, semantic)

    renamed = copy.deepcopy(fixture)
    record = renamed["expected"]["categories"]["itemEvents"]["tables"][0]["records"][0]
    record["renamedTargetCanonicalSymbol"] = record.pop("targetCanonicalSymbol")
    fixture_rejects(renamed)

    fixture_extra = copy.deepcopy(fixture)
    fixture_extra["expected"]["recordTargetProfiles"][0]["unexpected"] = 1
    fixture_rejects(fixture_extra)

    fixture_semantic = copy.deepcopy(fixture)
    fixture_semantic["expected"]["recordTargetProfiles"][0]["canonicalSymbol"] = "wrong-owner"
    validate_json(fixture_semantic, FIXTURE_SCHEMA, owner="wrong-shape-valid fixture target owner")
    with pytest.raises(ValueError, match="complete semantic fixture drift"):
        _verify_complete_map_events_fixture(fixture_semantic, output)

    fixture_order = copy.deepcopy(fixture)
    fixture_order["expected"]["routeCategoryJoinOrder"].reverse()
    fixture_rejects(fixture_order)

    fixture_boundary = copy.deepcopy(fixture)
    fixture_boundary["expected"]["categories"]["zoneEvents"]["tables"][0]["records"][0]["x"] = -1
    fixture_rejects(fixture_boundary)

    fixture_operation_definition_renamed = copy.deepcopy(fixture)
    definition = fixture_operation_definition_renamed["expected"]["operationDefinitions"][0]
    definition["renamedDefinitionSourceLine"] = definition.pop("definitionSourceLine")
    fixture_rejects(fixture_operation_definition_renamed)

    fixture_payload_context_extra = copy.deepcopy(fixture)
    fixture_payload_context_extra["expected"]["operationPayloadContexts"][0]["unexpected"] = 1
    fixture_rejects(fixture_payload_context_extra)

    program_missing = copy.deepcopy(output)
    del program_missing["entityTargetPrograms"][0]["termination"]["sourceMnemonic"]
    output_rejects(program_missing)

    program_extra = copy.deepcopy(output)
    target_operation = next(
        operation
        for program in program_extra["entityTargetPrograms"]
        for operation in program["operations"]
        if operation["target"] is not None
    )
    target_operation["target"]["unexpected"] = True
    output_rejects(program_extra)

    program_order = copy.deepcopy(output)
    program_order["entityTargetProgramOperationOrders"].reverse()
    output_rejects(program_order)

    program_boundary = copy.deepcopy(output)
    program_boundary["entityTargetPrograms"][0]["encodedSpanBytes"] = -1
    output_rejects(program_boundary)

    fixture_program_renamed = copy.deepcopy(fixture)
    reference_counts = fixture_program_renamed["expected"]["entityTargetPrograms"][0][
        "referenceCounts"
    ]
    reference_counts["renamedPhysicalRecordCount"] = reference_counts.pop("physicalRecordCount")
    fixture_rejects(fixture_program_renamed)

    zone_program_missing = copy.deepcopy(output)
    del zone_program_missing["zoneTargetPrograms"][0]["termination"]["sourceMnemonic"]
    output_rejects(zone_program_missing)

    item_program_extra = copy.deepcopy(output)
    item_target_operation = next(
        operation
        for program in item_program_extra["itemTargetPrograms"]
        for operation in program["operations"]
        if operation["target"] is not None
    )
    item_target_operation["target"]["unexpected"] = True
    output_rejects(item_program_extra)

    zone_program_order = copy.deepcopy(output)
    zone_program_order["zoneTargetProgramOperationOrders"].reverse()
    output_rejects(zone_program_order)

    item_program_boundary = copy.deepcopy(output)
    item_program_boundary["itemTargetPrograms"][0]["encodedSpanBytes"] = -1
    output_rejects(item_program_boundary)

    zone_exclusion_missing = copy.deepcopy(output)
    del zone_exclusion_missing["zoneTargetProgramExclusions"][0]["targetH1Address"]
    output_rejects(zone_exclusion_missing)

    fixture_zone_exclusion_renamed = copy.deepcopy(fixture)
    exclusion_counts = fixture_zone_exclusion_renamed["expected"]["zoneTargetProgramExclusions"][0][
        "referenceCounts"
    ]
    exclusion_counts["renamedPhysicalRecordCount"] = exclusion_counts.pop("physicalRecordCount")
    fixture_rejects(fixture_zone_exclusion_renamed)


def test_entity_target_program_parser_guards_comments_suffixes_and_h1_use_sites() -> None:
    branch = _parse_program_operation("bne.s   Next", source_line=7, source_order=0)
    assert branch == {
        "sourceOrder": 0,
        "sourceLine": 7,
        "sourceMnemonic": "bne.s",
        "mnemonic": "bne",
        "sizeSuffix": ".s",
        "operandTexts": ["Next"],
        "controlFlowKind": "conditional-branch",
        "instructionTargetSymbol": "Next",
    }
    assert (
        _parse_program_operation("jsr (Sleep).w", source_line=8, source_order=1)[
            "instructionTargetSymbol"
        ]
        == "Sleep"
    )
    assert (
        _parse_program_operation("bneX Target", source_line=9, source_order=2)["controlFlowKind"]
        == "ordinary"
    )
    assert (
        _parse_program_operation("move.w #Target,d0", source_line=10, source_order=3)[
            "instructionTargetSymbol"
        ]
        is None
    )
    assert _normalise_asm_statement("; bne.s Target") == ""
    assert _normalise_asm_statement("rts ; Target") == "rts"
    assert _listing_statement("00001000 51CF FFF4                  dbf     d7, loc_1000") == (
        0x1000,
        "dbf d7,loc_1000",
    )
    with pytest.raises(ValueError, match="operation syntax drift"):
        _parse_program_operation("Target:", source_line=11, source_order=4)
    with pytest.raises(ValueError, match="operation syntax drift"):
        _parse_program_operation("bne.x Target", source_line=12, source_order=5)

    listing_lines = [
        "00001000                            Entry:",
        "00001000 6602                        bne.s   Next",
        "00001002 4E75                        rts",
        "00001004                                ; End of function Entry",
    ]

    def block() -> dict[str, object]:
        return {
            "endFunctionSymbol": "Entry",
            "operations": [
                {"sourceLine": 2, "sourceStatement": "bne.s Next"},
                {"sourceLine": 3, "sourceStatement": "rts"},
            ],
        }

    assert (
        _bind_operations_to_h1(
            listing_lines,
            _h1_program_index(listing_lines),
            profile={"canonicalSymbol": "Entry", "targetH1Address": 0x1000},
            block=block(),
        )
        == 0x1004
    )
    altered_opcode = block()
    altered_opcode["operations"][0]["sourceStatement"] = "beq.s Next"
    with pytest.raises(ValueError, match="source/H1 operation relationship drift"):
        _bind_operations_to_h1(
            listing_lines,
            _h1_program_index(listing_lines),
            profile={"canonicalSymbol": "Entry", "targetH1Address": 0x1000},
            block=altered_opcode,
        )
    altered_operand = block()
    altered_operand["operations"][0]["sourceStatement"] = "bne.s Other"
    with pytest.raises(ValueError, match="source/H1 operation relationship drift"):
        _bind_operations_to_h1(
            listing_lines,
            _h1_program_index(listing_lines),
            profile={"canonicalSymbol": "Entry", "targetH1Address": 0x1000},
            block=altered_operand,
        )
    altered_order = block()
    altered_order["operations"].reverse()
    with pytest.raises(ValueError, match="source/H1 operation relationship drift"):
        _bind_operations_to_h1(
            listing_lines,
            _h1_program_index(listing_lines),
            profile={"canonicalSymbol": "Entry", "targetH1Address": 0x1000},
            block=altered_order,
        )
    altered_end_symbol = block()
    altered_end_symbol["endFunctionSymbol"] = "OtherEnd"
    with pytest.raises(ValueError, match="H1 function end drift"):
        _bind_operations_to_h1(
            listing_lines,
            _h1_program_index(listing_lines),
            profile={"canonicalSymbol": "Entry", "targetH1Address": 0x1000},
            block=altered_end_symbol,
        )
    boundary_drift_listing = [*listing_lines]
    boundary_drift_listing[-1] = "00001000                                ; End of function Entry"
    with pytest.raises(ValueError, match="H1 nonpositive program span"):
        _bind_operations_to_h1(
            boundary_drift_listing,
            _h1_program_index(boundary_drift_listing),
            profile={"canonicalSymbol": "Entry", "targetH1Address": 0x1000},
            block=block(),
        )


def test_jump_interface_alias_parser_guards_source_and_h1_target_relationship(
    tmp_path: Path,
) -> None:
    source_path = tmp_path / "code" / "jumpinterface.asm"
    source_path.parent.mkdir()
    source_path.write_text("j_Alias:\n    jmp Target(pc)\n", encoding="utf-8")
    addresses = {"j_Alias": 0x2000, "Target": 0x3000, "Other": 0x3004}
    owners = {
        0x2000: [
            {
                "symbol": "j_Alias",
                "sourcePath": "code/jumpinterface.asm",
                "sourceLine": 1,
            }
        ]
    }

    def listing(target: str = "Target") -> list[str]:
        return [
            "00002000                            j_Alias:",
            f"00002000 4EF9                        jmp {target}(pc)",
            "00003000                            Target:",
            "00003004                            Other:",
        ]

    definitions = _parse_jump_interface_aliases(
        tmp_path,
        addresses,
        listing("Target"),
        _h1_program_index(listing("Target")),
        owners,
        ["j_Alias"],
    )
    assert definitions == {
        "j_Alias": {
            "aliasSymbol": "j_Alias",
            "aliasAddress": 0x2000,
            "sourcePath": "code/jumpinterface.asm",
            "sourceLine": 1,
            "definitionSourceLine": 2,
            "sourceMnemonic": "jmp",
            "mnemonic": "jmp",
            "sizeSuffix": None,
            "operandTexts": ["Target(pc)"],
            "directTargetSymbol": "Target",
            "directTargetAddress": 0x3000,
            "listingAddress": 0x2000,
        }
    }

    source_path.write_text("j_Alias:\n    jsr Target(pc)\n", encoding="utf-8")
    with pytest.raises(ValueError, match="jump-interface definition drift"):
        _parse_jump_interface_aliases(
            tmp_path,
            addresses,
            listing("Target"),
            _h1_program_index(listing("Target")),
            owners,
            ["j_Alias"],
        )

    source_path.write_text("j_Alias:\n    jmp Other(pc)\n", encoding="utf-8")
    with pytest.raises(ValueError, match="jump-interface source/H1 drift"):
        _parse_jump_interface_aliases(
            tmp_path,
            addresses,
            listing("Target"),
            _h1_program_index(listing("Target")),
            owners,
            ["j_Alias"],
        )

    source_path.write_text("j_Alias:\n    jmp Target(pc)\n", encoding="utf-8")
    with pytest.raises(ValueError, match="jump-interface source/H1 drift"):
        _parse_jump_interface_aliases(
            tmp_path,
            addresses,
            listing("Other"),
            _h1_program_index(listing("Other")),
            owners,
            ["j_Alias"],
        )


def test_zone_and_item_target_program_parser_guards_category_paths(tmp_path: Path) -> None:
    source_path = tmp_path / "data" / "maps" / "entries" / "map00" / "mapsetups" / "events.asm"
    source_path.parent.mkdir(parents=True)
    source_path.write_text(
        "\n".join(
            (
                "ZoneEntry:",
                "    bne.s ZoneTarget",
                "    rts",
                "; End of function ZoneEntry",
                "ItemEntry:",
                "    bsr ItemTarget",
                "    rts",
                "; End of function ItemEntry",
                "ZoneTarget:",
                "    rts",
                "; End of function ZoneTarget",
                "ItemTarget:",
                "    rts",
                "; End of function ItemTarget",
            )
        )
        + "\n",
        encoding="utf-8",
    )
    source_name = "data/maps/entries/map00/mapsetups/events.asm"
    addresses = {
        "ZoneEntry": 0x1000,
        "ItemEntry": 0x1100,
        "ZoneTarget": 0x2000,
        "ItemTarget": 0x2004,
    }
    owners = {
        0x2000: [{"symbol": "ZoneTarget", "sourcePath": source_name, "sourceLine": 9}],
        0x2004: [{"symbol": "ItemTarget", "sourcePath": source_name, "sourceLine": 12}],
    }
    listing_lines = [
        "00001000                            ZoneEntry:",
        "00001000 6600 0FFE                  bne.s ZoneTarget",
        "00001002 4E75                        rts",
        "00001004                                ; End of function ZoneEntry",
        "00001100                            ItemEntry:",
        "00001100 6100 0F00                  bsr ItemTarget",
        "00001104 4E75                        rts",
        "00001106                                ; End of function ItemEntry",
        "00002000                            ZoneTarget:",
        "00002000 4E75                        rts",
        "00002002                                ; End of function ZoneTarget",
        "00002004                            ItemTarget:",
        "00002004 4E75                        rts",
        "00002006                                ; End of function ItemTarget",
    ]

    def profile(category: str, symbol: str, address: int, line: int) -> dict[str, object]:
        return {
            "canonicalSymbol": symbol,
            "targetAddress": address,
            "targetH1Address": address,
            "ownerSourcePath": source_name,
            "ownerSourceLine": line,
            "physicalRecordCount": 1,
            "setupRecordReferenceCount": 2,
            "routeRecordReferenceCount": 3,
            "categories": [category],
        }

    profiles = [
        profile("zoneEvents", "ZoneEntry", 0x1000, 1),
        profile("itemEvents", "ItemEntry", 0x1100, 5),
    ]
    listing_index = _h1_program_index(listing_lines)
    zone = _target_program_contract(
        tmp_path,
        addresses,
        listing_lines,
        listing_index,
        profiles,
        owners,
        category="zoneEvents",
    )
    item = _target_program_contract(
        tmp_path,
        addresses,
        listing_lines,
        listing_index,
        profiles,
        owners,
        category="itemEvents",
    )
    zone_programs, zone_summary, zone_control_flow, zone_orders, _, _, zone_exclusions = zone
    item_programs, item_summary, item_control_flow, item_orders, _, _, item_exclusions = item
    assert zone_summary == {
        "programCount": 1,
        "sourceFileCount": 1,
        "labelCount": 1,
        "operationCount": 2,
        "ordinaryOperationCount": 0,
        "conditionalBranchCount": 1,
        "unconditionalBranchCount": 0,
        "directCallCount": 0,
        "directJumpCount": 0,
        "returnCount": 1,
        "encodedSpanBytes": 4,
        "physicalRecordCount": 1,
        "setupRecordReferenceCount": 2,
        "routeRecordReferenceCount": 3,
        "internalControlFlowSiteCount": 0,
        "externalControlFlowSiteCount": 1,
        "instructionTargetCount": 1,
        "effectiveTargetCount": 1,
        "jumpInterfaceAliasCount": 0,
        "profileCount": 1,
        "explicitNonProgramExclusionCount": 0,
        "functionEndBoundaryCount": 1,
        "sourceStreamTerminatorCount": 0,
        "excludedPhysicalRecordCount": 0,
        "excludedSetupRecordReferenceCount": 0,
        "excludedRouteRecordReferenceCount": 0,
    }
    assert zone_programs[0]["operations"][0]["target"] == {
        "instructionTargetSymbol": "ZoneTarget",
        "instructionTargetAddress": 0x2000,
        "instructionTargetAddressLabels": owners[0x2000],
        "effectiveTargetSymbol": "ZoneTarget",
        "effectiveTargetAddress": 0x2000,
        "effectiveTargetAddressLabels": owners[0x2000],
        "effectiveTargetScope": "external",
    }
    assert zone_control_flow["aliasDefinitions"] == []
    assert zone_orders["instructionExternalTargetTotalOrder"] == ["ZoneTarget:8192:1:0:0:0"]
    assert zone_exclusions == []

    assert item_summary == {
        **zone_summary,
        "conditionalBranchCount": 0,
        "directCallCount": 1,
        "encodedSpanBytes": 6,
    }
    assert item_programs[0]["operations"][0]["target"] == {
        "instructionTargetSymbol": "ItemTarget",
        "instructionTargetAddress": 0x2004,
        "instructionTargetAddressLabels": owners[0x2004],
        "effectiveTargetSymbol": "ItemTarget",
        "effectiveTargetAddress": 0x2004,
        "effectiveTargetAddressLabels": owners[0x2004],
        "effectiveTargetScope": "external",
    }
    assert item_control_flow["aliasDefinitions"] == []
    assert item_orders["effectiveExternalTargetTotalOrder"] == ["ItemTarget:8196:0:0:1:0"]
    assert item_exclusions == []

    source_path.write_text(
        source_path.read_text(encoding="utf-8").replace("bne.s ZoneTarget", "bne.s Other"),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="source/H1 operation relationship drift"):
        _target_program_contract(
            tmp_path,
            addresses,
            listing_lines,
            listing_index,
            profiles,
            owners,
            category="zoneEvents",
        )
    source_path.write_text(
        source_path.read_text(encoding="utf-8")
        .replace("bne.s Other", "bne.s ZoneTarget")
        .replace("bsr ItemTarget", "bsr Other"),
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="source/H1 operation relationship drift"):
        _target_program_contract(
            tmp_path,
            addresses,
            listing_lines,
            listing_index,
            profiles,
            owners,
            category="itemEvents",
        )
    boundary_listing = [*listing_lines]
    boundary_listing[3] = "00001000                                ; End of function ZoneEntry"
    with pytest.raises(ValueError, match="H1 nonpositive program span"):
        _target_program_contract(
            tmp_path,
            addresses,
            boundary_listing,
            _h1_program_index(boundary_listing),
            profiles,
            owners,
            category="zoneEvents",
        )


def test_zone_target_program_source_stream_terminator_is_h1_guarded(tmp_path: Path) -> None:
    source_path = tmp_path / "data" / "maps" / "entries" / "map44" / "mapsetups" / "scripts.asm"
    source_path.parent.mkdir(parents=True)
    source_path.write_text(
        "StreamEntry:\n    csc_end\nNext:\n    rts\n; End of function Next\n",
        encoding="utf-8",
    )
    source_name = "data/maps/entries/map44/mapsetups/scripts.asm"
    addresses = {"StreamEntry": 0x3000, "Next": 0x3002}
    profile = {
        "canonicalSymbol": "StreamEntry",
        "targetAddress": 0x3000,
        "targetH1Address": 0x3000,
        "ownerSourcePath": source_name,
        "ownerSourceLine": 1,
        "physicalRecordCount": 1,
        "setupRecordReferenceCount": 1,
        "routeRecordReferenceCount": 1,
        "categories": ["zoneEvents"],
    }
    listing_lines = [
        "00003000                            StreamEntry:",
        "00003000                            csc_end",
        "00003000 FFFF                     M  dc.w $ffff",
        "00003002                            Next:",
        "00003002 4E75                        rts",
        "00003004                                ; End of function Next",
    ]
    programs, summary, _, _, _, _, exclusions = _target_program_contract(
        tmp_path,
        addresses,
        listing_lines,
        _h1_program_index(listing_lines),
        [profile],
        {},
        category="zoneEvents",
    )
    assert programs[0]["endFunctionSymbol"] is None
    assert programs[0]["endAddressExclusive"] == 0x3002
    assert programs[0]["termination"]["sourceMnemonic"] == "csc_end"
    assert summary["sourceStreamTerminatorCount"] == 1
    assert summary["functionEndBoundaryCount"] == 0
    assert exclusions == []

    altered_listing = [*listing_lines]
    altered_listing[1] = "00003000                            csc_stop"
    with pytest.raises(ValueError, match="source/H1 operation relationship drift"):
        _target_program_contract(
            tmp_path,
            addresses,
            altered_listing,
            _h1_program_index(altered_listing),
            [profile],
            {},
            category="zoneEvents",
        )


def test_reference_reconciliation_rejects_profile_and_category_counter_mutations(
    complete_output: dict[str, Any],
) -> None:
    output = complete_output
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

    broken_operation_weight = copy.deepcopy(output)
    broken_operation_weight["entityTargetPrograms"][0]["referenceCounts"][
        "routeRecordReferenceCount"
    ] += 1
    with pytest.raises(ValueError, match="operation program weight reconciliation drift"):
        _reconcile_operation_weight_contract(
            {
                "entityEvents": broken_operation_weight["entityTargetPrograms"],
                "zoneEvents": broken_operation_weight["zoneTargetPrograms"],
                "itemEvents": broken_operation_weight["itemTargetPrograms"],
            },
            {
                key: broken_operation_weight[key]
                for key in (
                    "operationVocabulary",
                    "operationFamilyCounts",
                    "operationVocabularySummary",
                )
            },
        )


def test_map_events_schema_size_stays_compact_and_reuses_closed_shapes() -> None:
    assert SCHEMA.stat().st_size < 850_000
    assert FIXTURE_SCHEMA.stat().st_size < 850_000
    schema = load_json(SCHEMA)
    assert {
        "entityEventRecord",
        "zoneEventRecord",
        "itemEventRecord",
        "entityTargetSourceOwnerLabel",
        "entityTargetProgramLabel",
        "entityTargetProgramTarget",
        "entityTargetProgramOperation",
        "entityTargetProgramReferenceCounts",
        "entityTargetProgram",
        "entityTargetProgramLabelOrder",
        "entityTargetProgramOperationOrder",
        "entityTargetJumpInterfaceAlias",
        "entityTargetControlFlowTargetTotal",
        "entityTargetControlFlowScopeTotals",
        "entityTargetControlFlowTotals",
        "entityTargetProgramControlFlow",
        "entityTargetProgramControlFlowTargetOrders",
        "entityTargetProgramSummary",
        "mapEventTargetProgram",
        "mapEventTargetProgramSummary",
        "mapEventTargetProgramExclusion",
    } <= set(schema["definitions"])
    assert {
        "mapEventOperationWeightCounts",
        "mapEventOperationEngineCatalog",
        "mapEventOperationDefinition",
        "mapEventPayloadContext",
        "mapEventOperationVocabularyCounts",
        "mapEventOperationVocabulary",
        "mapEventOperationFamilyCount",
        "mapEventOperationVocabularySummary",
    } <= set(schema["definitions"])
    for category, definition in (
        ("entityEvents", "entityEventRecord"),
        ("zoneEvents", "zoneEventRecord"),
        ("itemEvents", "itemEventRecord"),
    ):
        records = schema["properties"]["categories"]["properties"][category]["properties"][
            "tables"
        ]["items"]["properties"]["records"]
        assert records["items"] == {"$ref": f"#/definitions/{definition}"}
    assert schema["properties"]["entityTargetPrograms"]["items"] == {
        "$ref": "#/definitions/entityTargetProgram"
    }
    assert schema["definitions"]["entityTargetProgram"]["properties"]["operations"]["items"] == {
        "$ref": "#/definitions/entityTargetProgramOperation"
    }
    assert schema["definitions"]["entityTargetProgramOperation"]["properties"]["target"]["anyOf"][
        1
    ] == {"$ref": "#/definitions/entityTargetProgramTarget"}
    for category in ("zone", "item"):
        assert schema["properties"][f"{category}TargetPrograms"]["items"] == {
            "$ref": "#/definitions/mapEventTargetProgram"
        }
        assert schema["properties"][f"{category}TargetProgramOperationOrders"]["const"]
        assert schema["properties"][f"{category}TargetProgramBoundaryOrders"]["const"]
    assert schema["definitions"]["mapEventTargetProgram"]["properties"]["operations"]["items"] == {
        "$ref": "#/definitions/entityTargetProgramOperation"
    }
    assert schema["definitions"]["mapEventTargetProgramExclusion"]["additionalProperties"] is False
    for definition in (
        "mapEventOperationWeightCounts",
        "mapEventOperationDefinition",
        "mapEventPayloadContext",
        "mapEventOperationVocabularyCounts",
        "mapEventOperationVocabulary",
        "mapEventOperationFamilyCount",
    ):
        assert schema["definitions"][definition]["additionalProperties"] is False
    operation = schema["definitions"]["entityTargetProgramOperation"]
    assert operation["additionalProperties"] is False
    assert {"family", "definitionId", "payloadContextIds"} <= set(operation["required"])
    assert len(operation["allOf"]) == 54
    for category in ("entity", "zone", "item"):
        for suffix in ("OperationWeightOrders", "PayloadContextOrders"):
            assert schema["properties"][f"{category}TargetProgram{suffix}"]["const"]
    fixture_schema = load_json(FIXTURE_SCHEMA)
    fixture_output = fixture_schema["definitions"]["outputContract"]
    for category in ("zone", "item"):
        assert fixture_output["properties"][f"{category}TargetPrograms"]["items"] == {
            "$ref": "#/definitions/mapEventTargetProgram"
        }
        assert fixture_output["properties"][f"{category}TargetProgramExclusions"]["items"] == {
            "$ref": "#/definitions/mapEventTargetProgramExclusion"
        }
    assert (
        fixture_schema["definitions"]["mapEventOperationDefinition"]["additionalProperties"]
        is False
    )
