import copy
import json
from pathlib import Path
from typing import Any

import pytest

from sf2tool.h2 import map_events as map_events_module
from sf2tool.h2 import map_events_fixture, text_banks
from sf2tool.h2.map_events import (
    RAW_ZONE_DEFAULT_SYMBOL,
    SCHEMA,
    _bind_operations_to_h1,
    _build_sound_command_domain,
    _decode_event_record,
    _derived_action_payload_context_specs,
    _direct_flag_access_sites_for_program,
    _direct_flag_state_contract,
    _event_macro_use_sites,
    _guard_macro_emission,
    _h1_program_index,
    _join_source_rom_record,
    _listing_statement,
    _macro_definition,
    _normalise_asm_statement,
    _parse_jump_interface_aliases,
    _parse_program_operation,
    _parse_textbox_line_operand,
    _payload_context_contract,
    _reconcile_direct_flag_state_contract,
    _reconcile_event_reference_counts,
    _reconcile_operation_weight_contract,
    _reconcile_script_invocation_graph_contract,
    _reconcile_sound_command_reference_contract,
    _reconcile_textbox_reference_contract,
    _record_target_ownership,
    _script_invocation_graph_contract,
    _setup_category_joins,
    _sound_command_enum_values,
    _sound_command_reference_contract,
    _sound_command_service_definition,
    _source_macro_catalog,
    _target_program_contract,
    _textbox_reference_contract,
    _textbox_reference_sites,
    _textbox_service_definitions,
    _verify_complete_map_events_fixture,
    build_map_events_contract,
)
from sf2tool.h2.map_events_fixture import (
    FIXTURE,
    FIXTURE_SCHEMA,
    SECTION_SCHEMAS,
    load_map_events_fixture,
)
from sf2tool.h2.text_banks import build_text_line_domain_contract
from sf2tool.jsonio import load_json, schema_composition_audit, validate_json


@pytest.fixture(scope="module")
def complete_output() -> dict[str, Any]:
    """Build the complete static contract once; mutation tests must preserve it."""
    return build_map_events_contract(
        Path("local/roms/sf2-us.bin"), Path("local/upstream/SF2DISASM")
    )


def _copy_on_write_mutation(
    source: dict[str, Any],
    path: tuple[str | int, ...],
    *,
    delete: bool = False,
    value: Any = None,
) -> dict[str, Any]:
    """Clone only containers on *path* before applying one schema mutation."""
    if not path:
        raise ValueError("mutation path must not be empty")

    mutated = source.copy()
    source_cursor: Any = source
    mutated_cursor: Any = mutated
    for part in path[:-1]:
        child = source_cursor[part]
        if isinstance(child, dict):
            cloned_child = child.copy()
        elif isinstance(child, list):
            cloned_child = list(child)
        else:
            raise ValueError(f"mutation path crosses scalar at {part!r}")
        mutated_cursor[part] = cloned_child
        source_cursor = child
        mutated_cursor = cloned_child

    leaf = path[-1]
    if delete:
        del mutated_cursor[leaf]
    else:
        mutated_cursor[leaf] = value
    return mutated


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
    fixture = load_map_events_fixture()
    validate_json(output, SCHEMA, owner="map events complete output")
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
        "directFlagServiceDefinitions",
        "directFlagServiceDefinitionOrder",
        "directFlagAccessSites",
        "directFlagAccessSiteOrder",
        "directFlagProgramTotals",
        "directFlagProgramTotalOrder",
        "directFlagTotals",
        "directFlagTotalOrder",
        "directFlagStateSummary",
        "scriptInvocationServiceDefinition",
        "scriptInvocationSites",
        "scriptInvocationSiteOrder",
        "scriptInvocationCallerTotals",
        "scriptInvocationCallerTotalOrder",
        "scriptInvocationInstructionTargetTotals",
        "scriptInvocationInstructionTargetTotalOrder",
        "scriptInvocationEffectiveTargetTotals",
        "scriptInvocationEffectiveTargetTotalOrder",
        "scriptInvocationSummary",
        "textboxLineDomain",
        "textboxServiceDefinitions",
        "textboxServiceDefinitionOrder",
        "textboxReferenceSites",
        "textboxReferenceSiteOrder",
        "textboxCallerTotals",
        "textboxCallerTotalOrder",
        "textboxLineTotals",
        "textboxLineTotalOrder",
        "textboxSummary",
        "entityTargetProgramOperationWeightOrders",
        "entityTargetProgramPayloadContextOrders",
        "zoneTargetProgramOperationWeightOrders",
        "zoneTargetProgramPayloadContextOrders",
        "itemTargetProgramOperationWeightOrders",
        "itemTargetProgramPayloadContextOrders",
    ):
        assert output[field] == fixture["expected"][field]
    assert {
        field: output["directFlagStateSummary"][field]
        for field in (
            "serviceDefinitionCount",
            "directFlagAccessSiteCount",
            "observedFlagCount",
            "accessKindCounts",
            "categoryAccessKindCounts",
            "readConditionConsumerCounts",
        )
    } == {
        "serviceDefinitionCount": 3,
        "directFlagAccessSiteCount": 493,
        "observedFlagCount": 151,
        "accessKindCounts": {
            "read": {
                "physicalProgramOccurrenceCount": 316,
                "physicalRecordWeightedSiteCount": 513,
                "setupRecordReferenceWeightedSiteCount": 754,
                "routeRecordReferenceWeightedSiteCount": 839,
            },
            "set": {
                "physicalProgramOccurrenceCount": 169,
                "physicalRecordWeightedSiteCount": 230,
                "setupRecordReferenceWeightedSiteCount": 373,
                "routeRecordReferenceWeightedSiteCount": 404,
            },
            "clear": {
                "physicalProgramOccurrenceCount": 8,
                "physicalRecordWeightedSiteCount": 14,
                "setupRecordReferenceWeightedSiteCount": 21,
                "routeRecordReferenceWeightedSiteCount": 23,
            },
        },
        "categoryAccessKindCounts": {
            "entityEvents": {
                "read": {
                    "physicalProgramOccurrenceCount": 190,
                    "physicalRecordWeightedSiteCount": 314,
                    "setupRecordReferenceWeightedSiteCount": 401,
                    "routeRecordReferenceWeightedSiteCount": 458,
                },
                "set": {
                    "physicalProgramOccurrenceCount": 80,
                    "physicalRecordWeightedSiteCount": 88,
                    "setupRecordReferenceWeightedSiteCount": 120,
                    "routeRecordReferenceWeightedSiteCount": 125,
                },
                "clear": {
                    "physicalProgramOccurrenceCount": 0,
                    "physicalRecordWeightedSiteCount": 0,
                    "setupRecordReferenceWeightedSiteCount": 0,
                    "routeRecordReferenceWeightedSiteCount": 0,
                },
            },
            "zoneEvents": {
                "read": {
                    "physicalProgramOccurrenceCount": 118,
                    "physicalRecordWeightedSiteCount": 190,
                    "setupRecordReferenceWeightedSiteCount": 338,
                    "routeRecordReferenceWeightedSiteCount": 366,
                },
                "set": {
                    "physicalProgramOccurrenceCount": 84,
                    "physicalRecordWeightedSiteCount": 136,
                    "setupRecordReferenceWeightedSiteCount": 244,
                    "routeRecordReferenceWeightedSiteCount": 270,
                },
                "clear": {
                    "physicalProgramOccurrenceCount": 8,
                    "physicalRecordWeightedSiteCount": 14,
                    "setupRecordReferenceWeightedSiteCount": 21,
                    "routeRecordReferenceWeightedSiteCount": 23,
                },
            },
            "itemEvents": {
                "read": {
                    "physicalProgramOccurrenceCount": 8,
                    "physicalRecordWeightedSiteCount": 9,
                    "setupRecordReferenceWeightedSiteCount": 15,
                    "routeRecordReferenceWeightedSiteCount": 15,
                },
                "set": {
                    "physicalProgramOccurrenceCount": 5,
                    "physicalRecordWeightedSiteCount": 6,
                    "setupRecordReferenceWeightedSiteCount": 9,
                    "routeRecordReferenceWeightedSiteCount": 9,
                },
                "clear": {
                    "physicalProgramOccurrenceCount": 0,
                    "physicalRecordWeightedSiteCount": 0,
                    "setupRecordReferenceWeightedSiteCount": 0,
                    "routeRecordReferenceWeightedSiteCount": 0,
                },
            },
        },
        "readConditionConsumerCounts": {
            "immediateConditionConsumerCount": 316,
            "sourceMnemonicCounts": {"beq.s": 49, "bne.s": 264, "bne.w": 3},
            "missingImmediateOperationCount": 0,
            "nonConditionalImmediateOperationCount": 0,
            "nonAdjacentImmediateOperationCount": 0,
            "unrecognizedConditionalMnemonicCount": 0,
            "missingTargetIdentityCount": 0,
        },
    }
    assert output["scriptInvocationSummary"] == {
        "serviceDefinitionCount": 1,
        "siteCount": 147,
        "declaredInstructionTargetCount": 348,
        "observedInstructionTargetCount": 138,
        "declaredEffectiveTargetCount": 304,
        "observedEffectiveTargetCount": 135,
        "weightCounts": {
            "physicalProgramOccurrenceCount": 147,
            "physicalRecordWeightedSiteCount": 204,
            "setupRecordReferenceWeightedSiteCount": 352,
            "routeRecordReferenceWeightedSiteCount": 387,
        },
        "categoryWeightCounts": {
            "entityEvents": {
                "physicalProgramOccurrenceCount": 52,
                "physicalRecordWeightedSiteCount": 57,
                "setupRecordReferenceWeightedSiteCount": 104,
                "routeRecordReferenceWeightedSiteCount": 123,
            },
            "zoneEvents": {
                "physicalProgramOccurrenceCount": 87,
                "physicalRecordWeightedSiteCount": 138,
                "setupRecordReferenceWeightedSiteCount": 233,
                "routeRecordReferenceWeightedSiteCount": 249,
            },
            "itemEvents": {
                "physicalProgramOccurrenceCount": 8,
                "physicalRecordWeightedSiteCount": 9,
                "setupRecordReferenceWeightedSiteCount": 15,
                "routeRecordReferenceWeightedSiteCount": 15,
            },
        },
    }
    assert output["textboxLineDomain"] == {
        "contractId": "sf2-text-banks-static-v1",
        "upstreamCommit": "c834c652b6862bc5679fd7f69a38a7093206efc6",
        "romSha256": "9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9",
        "sourcePath": "data/scripting/text/gamescript.txt",
        "lineIdCount": 4267,
        "firstLineId": 0,
        "lastLineId": 4266,
        "idsAreContiguous": True,
    }
    assert output["textboxServiceDefinitions"] == [
        {
            "sourceKind": "line-reference",
            "definitionId": "event-service-macro:txt",
            "sourceMacro": "txt",
            "sourcePath": "sf2macros.asm",
            "definitionSourceLine": 52,
            "serviceTarget": "#TEXTBOX",
            "formalParameterOrdinals": [1],
            "emissionStatementTemplates": ["trap #textbox", "dc.w \\1"],
            "sentinelEncoding": None,
        },
        {
            "sourceKind": "close-sentinel",
            "definitionId": "event-service-macro:clsTxt",
            "sourceMacro": "clsTxt",
            "sourcePath": "sf2macros.asm",
            "definitionSourceLine": 57,
            "serviceTarget": "#TEXTBOX",
            "formalParameterOrdinals": [],
            "emissionStatementTemplates": ["trap #textbox", "dc.w $ffff"],
            "sentinelEncoding": "$FFFF",
        },
    ]
    assert output["textboxSummary"] == {
        "serviceDefinitionCount": 2,
        "siteCount": 1006,
        "lineReferenceSiteCount": 981,
        "closeSentinelSiteCount": 25,
        "declaredLineIdCount": 4267,
        "observedLineIdCount": 942,
        "minimumObservedLineId": 11,
        "maximumObservedLineId": 4178,
        "weightCounts": {
            "physicalProgramOccurrenceCount": 1006,
            "physicalRecordWeightedSiteCount": 1369,
            "setupRecordReferenceWeightedSiteCount": 1599,
            "routeRecordReferenceWeightedSiteCount": 1710,
        },
        "kindWeightCounts": {
            "line-reference": {
                "physicalProgramOccurrenceCount": 981,
                "physicalRecordWeightedSiteCount": 1328,
                "setupRecordReferenceWeightedSiteCount": 1551,
                "routeRecordReferenceWeightedSiteCount": 1660,
            },
            "close-sentinel": {
                "physicalProgramOccurrenceCount": 25,
                "physicalRecordWeightedSiteCount": 41,
                "setupRecordReferenceWeightedSiteCount": 48,
                "routeRecordReferenceWeightedSiteCount": 50,
            },
        },
        "categoryKindWeightCounts": {
            "entityEvents": {
                "line-reference": {
                    "physicalProgramOccurrenceCount": 958,
                    "physicalRecordWeightedSiteCount": 1302,
                    "setupRecordReferenceWeightedSiteCount": 1522,
                    "routeRecordReferenceWeightedSiteCount": 1631,
                },
                "close-sentinel": {
                    "physicalProgramOccurrenceCount": 23,
                    "physicalRecordWeightedSiteCount": 39,
                    "setupRecordReferenceWeightedSiteCount": 44,
                    "routeRecordReferenceWeightedSiteCount": 46,
                },
            },
            "zoneEvents": {
                "line-reference": {
                    "physicalProgramOccurrenceCount": 18,
                    "physicalRecordWeightedSiteCount": 21,
                    "setupRecordReferenceWeightedSiteCount": 21,
                    "routeRecordReferenceWeightedSiteCount": 21,
                },
                "close-sentinel": {
                    "physicalProgramOccurrenceCount": 0,
                    "physicalRecordWeightedSiteCount": 0,
                    "setupRecordReferenceWeightedSiteCount": 0,
                    "routeRecordReferenceWeightedSiteCount": 0,
                },
            },
            "itemEvents": {
                "line-reference": {
                    "physicalProgramOccurrenceCount": 5,
                    "physicalRecordWeightedSiteCount": 5,
                    "setupRecordReferenceWeightedSiteCount": 8,
                    "routeRecordReferenceWeightedSiteCount": 8,
                },
                "close-sentinel": {
                    "physicalProgramOccurrenceCount": 2,
                    "physicalRecordWeightedSiteCount": 2,
                    "setupRecordReferenceWeightedSiteCount": 4,
                    "routeRecordReferenceWeightedSiteCount": 4,
                },
            },
        },
    }
    assert len(output["textboxCallerTotals"]) == 914
    assert len(output["textboxLineTotals"]) == 4267
    assert output["textboxLineTotals"][0]["lineId"] == 0
    assert output["textboxLineTotals"][-1]["lineId"] == 4266
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


def test_map_events_schemas_reject_nested_missing_extra_and_boundary_mutations(
    complete_output: dict[str, Any],
) -> None:
    output = complete_output
    fixture = load_map_events_fixture()
    validate_json(output, SCHEMA, owner="map events complete output")

    def output_rejects(
        instance: dict[str, Any], section: str, owner: str
    ) -> None:
        with pytest.raises(ValueError, match=owner):
            validate_json(instance, SECTION_SCHEMAS[section], owner=owner)

    textbox_site_index = next(
        index
        for index, site in enumerate(output["textboxReferenceSites"])
        if site["sourceKind"] == "line-reference"
    )
    entity_target_location = next(
        (program_index, operation_index)
        for program_index, program in enumerate(output["entityTargetPrograms"])
        for operation_index, operation in enumerate(program["operations"])
        if operation["target"] is not None
    )
    item_target_location = next(
        (program_index, operation_index)
        for program_index, program in enumerate(output["itemTargetPrograms"])
        for operation_index, operation in enumerate(program["operations"])
        if operation["target"] is not None
    )

    rejection_cases = (
        (
            "routing-setup",
            "missing nested field",
            ("categories", "entityEvents", "tables", 0, "records", 0, "targetCanonicalSymbol"),
            True,
            None,
        ),
        (
            "routing-setup",
            "extra nested field",
            ("categories", "zoneEvents", "tables", 0, "records", 0, "unexpected"),
            False,
            True,
        ),
        (
            "routing-setup",
            "numeric boundary",
            ("routeCategoryJoins", 0, "pointerTableAddress"),
            False,
            -1,
        ),
        (
            "operation-vocabulary",
            "operation definition missing field",
            ("operationDefinitions", 0, "emissionStatementTemplates"),
            True,
            None,
        ),
        (
            "operation-vocabulary",
            "operation definition extra field",
            ("operationDefinitions", 0, "engineCatalog", "unexpected"),
            False,
            True,
        ),
        (
            "operation-vocabulary",
            "operation definition boundary",
            ("operationDefinitions", 0, "definitionSourceLine"),
            False,
            0,
        ),
        (
            "direct-flags",
            "direct flag missing field",
            ("directFlagAccessSites", 0, "conditionConsumer", "branchPolarity"),
            True,
            None,
        ),
        (
            "direct-flags",
            "direct flag extra field",
            ("directFlagAccessSites", 0, "conditionConsumer", "target", "unexpected"),
            False,
            True,
        ),
        (
            "direct-flags",
            "direct flag boundary",
            ("directFlagAccessSites", 0, "flagNumber"),
            False,
            -1,
        ),
        (
            "entity-programs",
            "operation family discriminator",
            ("entityTargetPrograms", 0, "operations", 0, "family"),
            False,
            "raw-68000-instruction",
        ),
        (
            "script-invocation",
            "script invocation missing field",
            (
                "scriptInvocationSites",
                0,
                "weightCounts",
                "routeRecordReferenceWeightedSiteCount",
            ),
            True,
            None,
        ),
        (
            "script-invocation",
            "script invocation extra field",
            ("scriptInvocationSites", 0, "weightCounts", "unexpected"),
            False,
            True,
        ),
        (
            "script-invocation",
            "script invocation boundary",
            ("scriptInvocationSites", 0, "operationAddress"),
            False,
            -1,
        ),
        (
            "textbox",
            "textbox missing field",
            (
                "textboxReferenceSites",
                textbox_site_index,
                "weightCounts",
                "routeRecordReferenceWeightedSiteCount",
            ),
            True,
            None,
        ),
        (
            "textbox",
            "textbox extra field",
            ("textboxReferenceSites", textbox_site_index, "weightCounts", "unexpected"),
            False,
            True,
        ),
        (
            "textbox",
            "textbox boundary",
            ("textboxReferenceSites", textbox_site_index, "lineId"),
            False,
            -1,
        ),
        (
            "textbox",
            "textbox line-total boundary",
            ("textboxLineTotals", 0, "lineId"),
            False,
            -1,
        ),
        (
            "entity-programs",
            "entity program missing field",
            ("entityTargetPrograms", 0, "termination", "sourceMnemonic"),
            True,
            None,
        ),
        (
            "entity-programs",
            "entity program extra field",
            (
                "entityTargetPrograms",
                entity_target_location[0],
                "operations",
                entity_target_location[1],
                "target",
                "unexpected",
            ),
            False,
            True,
        ),
        (
            "entity-programs",
            "entity program boundary",
            ("entityTargetPrograms", 0, "encodedSpanBytes"),
            False,
            -1,
        ),
        (
            "zone-programs",
            "zone program missing field",
            ("zoneTargetPrograms", 0, "termination", "sourceMnemonic"),
            True,
            None,
        ),
        (
            "item-programs",
            "item program extra field",
            (
                "itemTargetPrograms",
                item_target_location[0],
                "operations",
                item_target_location[1],
                "target",
                "unexpected",
            ),
            False,
            True,
        ),
        (
            "item-programs",
            "item program boundary",
            ("itemTargetPrograms", 0, "encodedSpanBytes"),
            False,
            -1,
        ),
        (
            "zone-programs",
            "zone exclusion missing field",
            ("zoneTargetProgramExclusions", 0, "targetH1Address"),
            True,
            None,
        ),
    )
    assert len(rejection_cases) == 24
    for section, owner, path, delete, value in rejection_cases:
        output_rejects(
            _copy_on_write_mutation(output, path, delete=delete, value=value),
            section,
            owner,
        )

    # Ordering and complete corpus values are owner semantics, not reusable shape.
    semantic_cases = (
        ("routing-setup", "physicalRecordOrder"),
        ("operation-vocabulary", "operationVocabulary"),
        ("direct-flags", "directFlagAccessSiteOrder"),
        ("direct-flags", "directFlagTotalOrder"),
        ("script-invocation", "scriptInvocationSiteOrder"),
        ("script-invocation", "scriptInvocationEffectiveTargetTotalOrder"),
        ("textbox", "textboxLineTotalOrder"),
        ("entity-programs", "entityTargetProgramOperationOrders"),
        ("zone-programs", "zoneTargetProgramOperationOrders"),
        ("routing-setup", "routeCategoryJoinOrder"),
    )
    assert len(semantic_cases) == 10
    for section, field in semantic_cases:
        semantic = output.copy()
        semantic[field] = list(reversed(output[field]))
        validate_json(
            semantic,
            SECTION_SCHEMAS[section],
            owner="schema-valid map events semantic drift",
        )
        with pytest.raises(ValueError, match="complete semantic fixture drift"):
            _verify_complete_map_events_fixture(fixture, semantic)

    fixture_semantic = fixture.copy()
    fixture_semantic["expected"] = _copy_on_write_mutation(
        fixture["expected"],
        ("recordTargetProfiles", 0, "canonicalSymbol"),
        value="wrong-owner",
    )
    validate_json(
        fixture_semantic["expected"],
        SECTION_SCHEMAS["routing-setup"],
        owner="schema-valid fixture target-owner drift",
    )
    with pytest.raises(ValueError, match="complete semantic fixture drift"):
        _verify_complete_map_events_fixture(fixture_semantic, output)

    for field, replacement in (
        ("upstreamCommit", "0" * 40),
        ("romSha256", "0" * 64),
        (
            "function",
            {
                **fixture["function"],
                "RunMapSetupEntityEvent": fixture["function"][
                    "RunMapSetupEntityEvent"
                ]
                + 2,
            },
        ),
    ):
        wrong_owner = {**fixture, field: replacement}
        with pytest.raises(ValueError, match="provenance/address drift"):
            _verify_complete_map_events_fixture(wrong_owner, output)

    assert output == fixture["expected"]


def test_map_events_fixture_shards_recompose_exactly_and_reject_inventory_drift(
    complete_output: dict[str, Any],
    tmp_path: Path,
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    index = load_json(FIXTURE)
    validate_json(index, FIXTURE_SCHEMA, owner="map events fixture index")
    assert tuple(shard["section"] for shard in index["shards"]) == tuple(SECTION_SCHEMAS)
    assert FIXTURE.stat().st_size < 12_000
    assert len(index["shards"]) == 9

    fixture = load_map_events_fixture()
    assert list(fixture["expected"]) == index["fieldOrder"]
    assert fixture["expected"] == complete_output

    original_load_json = map_events_fixture.load_json
    index_path = FIXTURE.resolve()
    shard_paths = {
        descriptor["section"]: map_events_fixture.repo_path(descriptor["path"]).resolve()
        for descriptor in index["shards"]
    }

    def expect_loader_rejected(
        mutations: dict[Path, Any],
        message: str,
    ) -> None:
        def fake_load_json(path: Path) -> Any:
            value = original_load_json(path)
            mutate = mutations.get(Path(path).resolve())
            if mutate is not None:
                value = copy.deepcopy(value)
                mutate(value)
            return value

        monkeypatch.setattr(map_events_fixture, "load_json", fake_load_json)
        try:
            with pytest.raises(ValueError, match=message):
                load_map_events_fixture()
        finally:
            monkeypatch.setattr(map_events_fixture, "load_json", original_load_json)

    expect_loader_rejected(
        {index_path: lambda value: value["shards"].reverse()},
        "section order drift",
    )
    expect_loader_rejected(
        {index_path: lambda value: value["fieldOrder"].pop()},
        "complete field coverage drift",
    )
    expect_loader_rejected(
        {
            index_path: lambda value: value["shards"][0].__setitem__(
                "path", "tests/fixtures/h2/map-events/wrong.json"
            )
        },
        "shard path drift",
    )
    expect_loader_rejected(
        {
            shard_paths["routing-setup"]: lambda value: value.__setitem__(
                "section", "entity-programs"
            )
        },
        "shard identity drift",
    )

    first_field = index["shards"][0]["fields"][0]
    expect_loader_rejected(
        {
            shard_paths["routing-setup"]: lambda value: value["expected"].pop(
                first_field
            )
        },
        "field inventory drift",
    )

    duplicate_field = index["shards"][0]["fields"][0]

    def add_duplicate_descriptor(value: dict[str, Any]) -> None:
        value["shards"][1]["fields"].append(duplicate_field)

    def add_duplicate_payload(value: dict[str, Any]) -> None:
        value["expected"][duplicate_field] = copy.deepcopy(
            complete_output[duplicate_field]
        )

    expect_loader_rejected(
        {
            index_path: add_duplicate_descriptor,
            shard_paths["entity-programs"]: add_duplicate_payload,
        },
        "duplicate field drift",
    )

    original_repo_path = map_events_fixture.repo_path

    def missing_shard_path(*parts: str) -> Path:
        if parts == (index["shards"][0]["path"],):
            return tmp_path / "missing-routing-shard.json"
        return original_repo_path(*parts)

    monkeypatch.setattr(map_events_fixture, "repo_path", missing_shard_path)
    try:
        with pytest.raises(FileNotFoundError):
            load_map_events_fixture()
    finally:
        monkeypatch.setattr(map_events_fixture, "repo_path", original_repo_path)

    wrong_index = copy.deepcopy(index)
    wrong_index["romSha256"] = "0" * 64
    wrong_index_path = tmp_path / "map-events-index.json"
    wrong_index_path.write_text(
        json.dumps(wrong_index, indent=2) + "\n", encoding="utf-8"
    )
    wrong_fixture = load_map_events_fixture(wrong_index_path)
    with pytest.raises(ValueError, match="provenance/address drift"):
        _verify_complete_map_events_fixture(wrong_fixture, complete_output)

    semantic_shard = copy.deepcopy(fixture)
    semantic_shard["expected"]["recordTargetProfiles"][0][
        "canonicalSymbol"
    ] = "wrong-owner"
    validate_json(
        semantic_shard["expected"],
        SCHEMA,
        owner="schema-valid sharded map events drift",
    )
    with pytest.raises(ValueError, match="complete semantic fixture drift"):
        _verify_complete_map_events_fixture(semantic_shard, complete_output)


def test_map_events_verifier_rejects_schema_valid_shard_drift_before_write(
    complete_output: dict[str, Any],
    tmp_path: Path,
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    fixture = load_map_events_fixture()
    broken = copy.deepcopy(fixture)
    broken["expected"]["soundCommandSites"].reverse()
    validate_json(broken["expected"], SCHEMA, owner="schema-valid map events golden drift")

    output_path = tmp_path / "map-events.json"
    monkeypatch.setattr(map_events_module, "load_map_events_fixture", lambda: broken)
    monkeypatch.setattr(
        map_events_module,
        "build_map_events_contract",
        lambda _rom_path, _upstream_path: complete_output,
    )
    with pytest.raises(ValueError, match="complete semantic fixture drift"):
        map_events_module.verify_map_events_contract(
            Path("unused-rom.bin"),
            Path("unused-upstream"),
            output_path=output_path,
        )
    assert not output_path.exists()


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


def test_direct_flag_program_parser_derives_reads_writes_and_immediate_consumers() -> None:
    service_accesses = {
        "event-service-macro:chkFlg": {"sourceMacro": "chkFlg", "accessKind": "read"},
        "event-service-macro:setFlg": {"sourceMacro": "setFlg", "accessKind": "set"},
        "event-service-macro:clrFlg": {"sourceMacro": "clrFlg", "accessKind": "clear"},
    }

    def target(symbol: str, address: int) -> dict[str, Any]:
        labels = [
            {
                "symbol": symbol,
                "sourcePath": "data/maps/entries/map00/mapsetups/s2_entityevents.asm",
                "sourceLine": 90,
            }
        ]
        return {
            "instructionTargetSymbol": symbol,
            "instructionTargetAddress": address,
            "instructionTargetAddressLabels": labels,
            "effectiveTargetSymbol": symbol,
            "effectiveTargetAddress": address,
            "effectiveTargetAddressLabels": labels,
            "effectiveTargetScope": "internal",
        }

    def flag_operation(
        source_order: int, macro: str, operand: str, address: int
    ) -> dict[str, Any]:
        return {
            "sourceOrder": source_order,
            "sourceLine": source_order + 10,
            "sourceMnemonic": macro,
            "mnemonic": macro.lower(),
            "sizeSuffix": None,
            "operandTexts": [operand],
            "controlFlowKind": "ordinary",
            "address": address,
            "target": None,
            "family": "event-service-macro",
            "definitionId": f"event-service-macro:{macro}",
        }

    def branch_operation(
        source_order: int, mnemonic: str, address: int, branch_target: dict[str, Any]
    ) -> dict[str, Any]:
        return {
            "sourceOrder": source_order,
            "sourceLine": source_order + 10,
            "sourceMnemonic": mnemonic,
            "mnemonic": mnemonic.split(".", 1)[0],
            "sizeSuffix": f".{mnemonic.split('.', 1)[1]}" if "." in mnemonic else None,
            "operandTexts": [branch_target["instructionTargetSymbol"]],
            "controlFlowKind": "conditional-branch",
            "address": address,
            "target": branch_target,
            "family": "raw-68000-control-flow",
            "definitionId": None,
        }

    def program(operations: list[dict[str, Any]]) -> dict[str, Any]:
        return {
            "canonicalSymbol": "Map0_EntityEvent0",
            "entryAddress": 0x1000,
            "programOrder": 0,
            "sourcePath": "data/maps/entries/map00/mapsetups/s2_entityevents.asm",
            "referenceCounts": {
                "physicalRecordCount": 2,
                "setupRecordReferenceCount": 3,
                "routeRecordReferenceCount": 4,
            },
            "operations": operations,
        }

    first_target = target("first", 0x1010)
    second_target = target("second", 0x1020)
    third_target = target("third", 0x1030)
    sites = _direct_flag_access_sites_for_program(
        "entityEvents",
        program(
            [
                flag_operation(0, "chkFlg", "10", 0x1000),
                branch_operation(1, "bne.s", 0x1004, first_target),
                flag_operation(2, "chkFlg", "$000B", 0x1006),
                branch_operation(3, "beq.s", 0x100A, second_target),
                flag_operation(4, "chkFlg", "12", 0x100C),
                branch_operation(5, "bne.w", 0x1010, third_target),
                flag_operation(6, "setFlg", "13", 0x1014),
                flag_operation(7, "clrFlg", "14", 0x1018),
            ]
        ),
        service_accesses,
    )
    assert [(site["accessKind"], site["flagNumber"]) for site in sites] == [
        ("read", 10),
        ("read", 11),
        ("read", 12),
        ("set", 13),
        ("clear", 14),
    ]
    assert [site["conditionConsumer"] for site in sites[:3]] == [
        {
            "relation": "immediate-next-operation",
            "operationSourceOrder": 1,
            "sourceLine": 11,
            "address": 0x1004,
            "sourceMnemonic": "bne.s",
            "mnemonic": "bne",
            "sizeSuffix": ".s",
            "operandTexts": ["first"],
            "branchPolarity": "not-equal",
            "target": first_target,
        },
        {
            "relation": "immediate-next-operation",
            "operationSourceOrder": 3,
            "sourceLine": 13,
            "address": 0x100A,
            "sourceMnemonic": "beq.s",
            "mnemonic": "beq",
            "sizeSuffix": ".s",
            "operandTexts": ["second"],
            "branchPolarity": "equal",
            "target": second_target,
        },
        {
            "relation": "immediate-next-operation",
            "operationSourceOrder": 5,
            "sourceLine": 15,
            "address": 0x1010,
            "sourceMnemonic": "bne.w",
            "mnemonic": "bne",
            "sizeSuffix": ".w",
            "operandTexts": ["third"],
            "branchPolarity": "not-equal",
            "target": third_target,
        },
    ]
    assert [site["conditionConsumer"] for site in sites[3:]] == [None, None]
    assert sites[0]["referenceWeights"] == {
        "physicalRecordCount": 2,
        "setupRecordReferenceCount": 3,
        "routeRecordReferenceCount": 4,
    }

    bad_operand = program([
        flag_operation(0, "chkFlg", "FlagName", 0x1000),
        branch_operation(1, "bne.s", 0x1004, first_target),
    ])
    with pytest.raises(ValueError, match="operand syntax drift"):
        _direct_flag_access_sites_for_program("entityEvents", bad_operand, service_accesses)

    missing_consumer = program([flag_operation(0, "chkFlg", "10", 0x1000)])
    with pytest.raises(ValueError, match="lacks an immediate condition consumer"):
        _direct_flag_access_sites_for_program("entityEvents", missing_consumer, service_accesses)

    non_conditional_consumer = program([
        flag_operation(0, "chkFlg", "10", 0x1000),
        {**flag_operation(1, "setFlg", "11", 0x1004)},
    ])
    with pytest.raises(ValueError, match="consumer relationship drift"):
        _direct_flag_access_sites_for_program(
            "entityEvents", non_conditional_consumer, service_accesses
        )

    reordered_consumer = program([
        flag_operation(0, "chkFlg", "10", 0x1000),
        branch_operation(2, "bne.s", 0x1004, first_target),
    ])
    with pytest.raises(ValueError, match="consumer relationship drift"):
        _direct_flag_access_sites_for_program("entityEvents", reordered_consumer, service_accesses)

    bad_target = copy.deepcopy(first_target)
    del bad_target["effectiveTargetAddress"]
    with pytest.raises(ValueError, match="target identity drift"):
        _direct_flag_access_sites_for_program(
            "entityEvents",
            program(
                [
                    flag_operation(0, "chkFlg", "10", 0x1000),
                    branch_operation(1, "bne.s", 0x1004, bad_target),
                ]
            ),
            service_accesses,
        )


def test_direct_flag_contract_reconciles_service_use_consumer_and_weight_mutations(
    complete_output: dict[str, Any],
) -> None:
    programs_by_category = {
        "entityEvents": complete_output["entityTargetPrograms"],
        "zoneEvents": complete_output["zoneTargetPrograms"],
        "itemEvents": complete_output["itemTargetPrograms"],
    }
    direct_contract = {
        field: copy.deepcopy(complete_output[field])
        for field in (
            "directFlagServiceDefinitions",
            "directFlagServiceDefinitionOrder",
            "directFlagAccessSites",
            "directFlagAccessSiteOrder",
            "directFlagProgramTotals",
            "directFlagProgramTotalOrder",
            "directFlagTotals",
            "directFlagTotalOrder",
            "directFlagStateSummary",
        )
    }
    _reconcile_direct_flag_state_contract(direct_contract, programs_by_category)

    changed_definition = copy.deepcopy(complete_output["operationDefinitions"])
    chk_definition = next(
        definition for definition in changed_definition if definition["sourceMacro"] == "chkFlg"
    )
    chk_definition["emissionStatementTemplates"].reverse()
    with pytest.raises(ValueError, match="service emission/order drift"):
        _direct_flag_state_contract(changed_definition, programs_by_category)

    changed_service_coverage = copy.deepcopy(complete_output["operationDefinitions"])
    clear_definition = next(
        definition
        for definition in changed_service_coverage
        if definition["sourceMacro"] == "clrFlg"
    )
    clear_definition["serviceTarget"] = "#CLEAR_STATE"
    with pytest.raises(ValueError, match="service coverage/order drift"):
        _direct_flag_state_contract(changed_service_coverage, programs_by_category)

    changed_operand_programs = copy.deepcopy(programs_by_category)
    first_read = next(
        operation
        for program in changed_operand_programs["entityEvents"]
        for operation in program["operations"]
        if operation["sourceMnemonic"] == "chkFlg"
    )
    first_read["operandTexts"] = ["FlagName"]
    with pytest.raises(ValueError, match="operand syntax drift"):
        _direct_flag_state_contract(
            complete_output["operationDefinitions"], changed_operand_programs
        )

    changed_branch_programs = copy.deepcopy(programs_by_category)
    for program in changed_branch_programs["entityEvents"]:
        for index, operation in enumerate(program["operations"][:-1]):
            if operation["sourceMnemonic"] == "chkFlg":
                consumer = program["operations"][index + 1]
                consumer["sourceMnemonic"] = "bra.s"
                consumer["mnemonic"] = "bra"
                consumer["controlFlowKind"] = "unconditional-branch"
                break
        else:
            continue
        break
    with pytest.raises(ValueError, match="consumer relationship drift"):
        _direct_flag_state_contract(
            complete_output["operationDefinitions"], changed_branch_programs
        )

    changed_order_programs = copy.deepcopy(programs_by_category)
    for program in changed_order_programs["entityEvents"]:
        for index, operation in enumerate(program["operations"][:-1]):
            if operation["sourceMnemonic"] == "chkFlg":
                program["operations"][index + 1]["sourceOrder"] += 1
                break
        else:
            continue
        break
    with pytest.raises(ValueError, match="consumer relationship drift"):
        _direct_flag_state_contract(
            complete_output["operationDefinitions"], changed_order_programs
        )

    changed_polarity = copy.deepcopy(direct_contract)
    first_consumer = changed_polarity["directFlagAccessSites"][0]["conditionConsumer"]
    assert first_consumer is not None
    first_consumer["mnemonic"] = "beq" if first_consumer["mnemonic"] == "bne" else "bne"
    first_consumer["sourceMnemonic"] = (
        f"{first_consumer['mnemonic']}{first_consumer['sizeSuffix']}"
    )
    first_consumer["branchPolarity"] = (
        "equal" if first_consumer["mnemonic"] == "beq" else "not-equal"
    )
    with pytest.raises(ValueError, match="source/use-site reconciliation drift"):
        _reconcile_direct_flag_state_contract(changed_polarity, programs_by_category)

    changed_target = copy.deepcopy(direct_contract)
    target = changed_target["directFlagAccessSites"][0]["conditionConsumer"]["target"]
    target["instructionTargetSymbol"] = "wrong-target"
    with pytest.raises(ValueError, match="source/use-site reconciliation drift"):
        _reconcile_direct_flag_state_contract(changed_target, programs_by_category)

    changed_weights = copy.deepcopy(direct_contract)
    changed_weights["directFlagAccessSites"][0]["referenceWeights"][
        "routeRecordReferenceCount"
    ] += 1
    with pytest.raises(ValueError, match="source/use-site reconciliation drift"):
        _reconcile_direct_flag_state_contract(changed_weights, programs_by_category)


def test_script_invocation_graph_preserves_aliases_and_zero_inclusive_totals() -> None:
    definitions = [
        {
            "definitionId": "event-service-macro:script",
            "family": "event-service-macro",
            "sourceMacro": "script",
            "sourcePath": "sf2macros.asm",
            "definitionSourceLine": 62,
            "formalParameterOrdinals": [1],
            "emissionStatementTemplates": ["lea \\1(pc),a0", "trap #mapscript"],
            "serviceTarget": "#MAPSCRIPT",
        }
    ]

    def caller(
        symbol: str,
        entry_address: int,
        order: int,
        weights: tuple[int, int, int],
        operand: str | None,
    ) -> dict[str, Any]:
        operations = []
        if operand is not None:
            operations.append(
                {
                    "sourceOrder": 0,
                    "sourceLine": 20 + order,
                    "sourceMnemonic": "script",
                    "operandTexts": [operand],
                    "address": entry_address + 4,
                    "family": "event-service-macro",
                    "definitionId": "event-service-macro:script",
                }
            )
        return {
            "canonicalSymbol": symbol,
            "entryAddress": entry_address,
            "programOrder": order,
            "sourcePath": f"data/maps/{symbol}.asm",
            "referenceCounts": {
                "physicalRecordCount": weights[0],
                "setupRecordReferenceCount": weights[1],
                "routeRecordReferenceCount": weights[2],
            },
            "operations": operations,
        }

    callers = {
        "entityEvents": [caller("EntityCaller", 0x1000, 0, (2, 3, 4), "TargetAlias")],
        "zoneEvents": [caller("ZoneCaller", 0x1100, 0, (9, 10, 11), None)],
        "itemEvents": [caller("ItemCaller", 0x1200, 0, (5, 6, 7), "TargetEntry")],
    }
    program_corpus = {
        "labelOwners": {
            "TargetAlias": "TargetEntry",
            "TargetEntry": "TargetEntry",
            "UnusedEntry": "UnusedEntry",
        },
        "programs": [
            {
                "id": "TargetEntry",
                "entryLabel": "TargetEntry",
                "address": 0x4000,
                "sourcePath": "code/maps/target.asm",
                "termination": "csc-end",
                "labels": ["TargetEntry", "TargetAlias"],
            },
            {
                "id": "UnusedEntry",
                "entryLabel": "UnusedEntry",
                "address": 0x5000,
                "sourcePath": "code/maps/unused.asm",
                "termination": "absolute-jump",
                "labels": ["UnusedEntry"],
            },
        ],
    }
    addresses = {"TargetEntry": 0x4000, "TargetAlias": 0x4010, "UnusedEntry": 0x5000}
    contract = _script_invocation_graph_contract(
        definitions, callers, program_corpus, addresses
    )
    assert contract["scriptInvocationSites"] == [
        {
            "siteOrder": 0,
            "category": "entityEvents",
            "sourceMacro": "script",
            "definitionId": "event-service-macro:script",
            "callerProgramKey": "EntityCaller:4096",
            "callerProgramCanonicalSymbol": "EntityCaller",
            "callerProgramEntryAddress": 0x1000,
            "callerProgramOrder": 0,
            "callerSourcePath": "data/maps/EntityCaller.asm",
            "operationSourceOrder": 0,
            "sourceLine": 20,
            "operationAddress": 0x1004,
            "rawOperand": "TargetAlias",
            "instructionTargetLabel": "TargetAlias",
            "instructionTargetAddress": 0x4010,
            "effectiveOwnerProgramId": "TargetEntry",
            "effectiveOwnerProgramOrder": 0,
            "effectiveOwnerEntryLabel": "TargetEntry",
            "effectiveOwnerEntryAddress": 0x4000,
            "effectiveOwnerSourcePath": "code/maps/target.asm",
            "effectiveOwnerTermination": "csc-end",
            "weightCounts": {
                "physicalProgramOccurrenceCount": 1,
                "physicalRecordWeightedSiteCount": 2,
                "setupRecordReferenceWeightedSiteCount": 3,
                "routeRecordReferenceWeightedSiteCount": 4,
            },
        },
        {
            "siteOrder": 1,
            "category": "itemEvents",
            "sourceMacro": "script",
            "definitionId": "event-service-macro:script",
            "callerProgramKey": "ItemCaller:4608",
            "callerProgramCanonicalSymbol": "ItemCaller",
            "callerProgramEntryAddress": 0x1200,
            "callerProgramOrder": 0,
            "callerSourcePath": "data/maps/ItemCaller.asm",
            "operationSourceOrder": 0,
            "sourceLine": 20,
            "operationAddress": 0x1204,
            "rawOperand": "TargetEntry",
            "instructionTargetLabel": "TargetEntry",
            "instructionTargetAddress": 0x4000,
            "effectiveOwnerProgramId": "TargetEntry",
            "effectiveOwnerProgramOrder": 0,
            "effectiveOwnerEntryLabel": "TargetEntry",
            "effectiveOwnerEntryAddress": 0x4000,
            "effectiveOwnerSourcePath": "code/maps/target.asm",
            "effectiveOwnerTermination": "csc-end",
            "weightCounts": {
                "physicalProgramOccurrenceCount": 1,
                "physicalRecordWeightedSiteCount": 5,
                "setupRecordReferenceWeightedSiteCount": 6,
                "routeRecordReferenceWeightedSiteCount": 7,
            },
        },
    ]
    assert [row["siteOrders"] for row in contract["scriptInvocationCallerTotals"]] == [
        [0],
        [],
        [1],
    ]
    assert [
        (row["instructionTargetLabel"], row["effectiveOwnerProgramId"], row["siteOrders"])
        for row in contract["scriptInvocationInstructionTargetTotals"]
    ] == [
        ("TargetAlias", "TargetEntry", [0]),
        ("TargetEntry", "TargetEntry", [1]),
        ("UnusedEntry", "UnusedEntry", []),
    ]
    assert [
        (row["effectiveOwnerProgramId"], row["siteOrders"])
        for row in contract["scriptInvocationEffectiveTargetTotals"]
    ] == [("TargetEntry", [0, 1]), ("UnusedEntry", [])]
    assert contract["scriptInvocationSummary"] == {
        "serviceDefinitionCount": 1,
        "siteCount": 2,
        "declaredInstructionTargetCount": 3,
        "observedInstructionTargetCount": 2,
        "declaredEffectiveTargetCount": 2,
        "observedEffectiveTargetCount": 1,
        "weightCounts": {
            "physicalProgramOccurrenceCount": 2,
            "physicalRecordWeightedSiteCount": 7,
            "setupRecordReferenceWeightedSiteCount": 9,
            "routeRecordReferenceWeightedSiteCount": 11,
        },
        "categoryWeightCounts": {
            "entityEvents": {
                "physicalProgramOccurrenceCount": 1,
                "physicalRecordWeightedSiteCount": 2,
                "setupRecordReferenceWeightedSiteCount": 3,
                "routeRecordReferenceWeightedSiteCount": 4,
            },
            "zoneEvents": {
                "physicalProgramOccurrenceCount": 0,
                "physicalRecordWeightedSiteCount": 0,
                "setupRecordReferenceWeightedSiteCount": 0,
                "routeRecordReferenceWeightedSiteCount": 0,
            },
            "itemEvents": {
                "physicalProgramOccurrenceCount": 1,
                "physicalRecordWeightedSiteCount": 5,
                "setupRecordReferenceWeightedSiteCount": 6,
                "routeRecordReferenceWeightedSiteCount": 7,
            },
        },
    }
    _reconcile_script_invocation_graph_contract(
        contract, definitions, callers, program_corpus, addresses
    )

    changed_aliases = copy.deepcopy(program_corpus)
    changed_aliases["labelOwners"]["TargetAlias"] = "UnusedEntry"
    with pytest.raises(ValueError, match="label-owner mapping drift"):
        _script_invocation_graph_contract(definitions, callers, changed_aliases, addresses)

    changed_addresses = {**addresses, "TargetAlias": 0x4012}
    with pytest.raises(ValueError, match="scriptInvocationSites reconciliation drift"):
        _reconcile_script_invocation_graph_contract(
            contract, definitions, callers, program_corpus, changed_addresses
        )

    changed_definition = copy.deepcopy(definitions)
    changed_definition[0]["emissionStatementTemplates"].reverse()
    with pytest.raises(ValueError, match="service emission/order drift"):
        _script_invocation_graph_contract(
            changed_definition, callers, program_corpus, addresses
        )

    changed_operand_callers = copy.deepcopy(callers)
    changed_operand_callers["entityEvents"][0]["operations"][0]["operandTexts"] = ["Missing"]
    with pytest.raises(ValueError, match="label-owner coverage drift"):
        _script_invocation_graph_contract(
            definitions, changed_operand_callers, program_corpus, addresses
        )

    changed_order_callers = copy.deepcopy(callers)
    changed_order_callers["entityEvents"][0]["operations"][0]["sourceOrder"] = 1
    with pytest.raises(ValueError, match="source/use-site drift"):
        _script_invocation_graph_contract(
            definitions, changed_order_callers, program_corpus, addresses
        )

    changed_operation_address_callers = copy.deepcopy(callers)
    changed_operation_address_callers["entityEvents"][0]["operations"][0]["address"] += 2
    with pytest.raises(ValueError, match="scriptInvocationSites reconciliation drift"):
        _reconcile_script_invocation_graph_contract(
            contract,
            definitions,
            changed_operation_address_callers,
            program_corpus,
            addresses,
        )

    changed_owner_corpus = copy.deepcopy(program_corpus)
    changed_owner_corpus["programs"][0]["termination"] = "absolute-jump"
    with pytest.raises(ValueError, match="scriptInvocationSites reconciliation drift"):
        _reconcile_script_invocation_graph_contract(
            contract, definitions, callers, changed_owner_corpus, addresses
        )

    changed_site_identity = copy.deepcopy(contract)
    changed_site_identity["scriptInvocationSites"][0]["siteOrder"] = 7
    with pytest.raises(ValueError, match="scriptInvocationSites reconciliation drift"):
        _reconcile_script_invocation_graph_contract(
            changed_site_identity, definitions, callers, program_corpus, addresses
        )

    for reference_field in (
        "physicalRecordCount",
        "setupRecordReferenceCount",
        "routeRecordReferenceCount",
    ):
        changed_weights = copy.deepcopy(callers)
        changed_weights["entityEvents"][0]["referenceCounts"][reference_field] += 1
        with pytest.raises(ValueError, match="scriptInvocationSites reconciliation drift"):
            _reconcile_script_invocation_graph_contract(
                contract, definitions, changed_weights, program_corpus, addresses
            )


def test_textbox_source_forms_parse_line_references_and_close_sentinels() -> None:
    operation_definitions = [
        {
            "definitionId": "event-service-macro:txt",
            "family": "event-service-macro",
            "sourceMacro": "txt",
            "sourcePath": "sf2macros.asm",
            "definitionSourceLine": 52,
            "formalParameterOrdinals": [1],
            "emissionStatementTemplates": ["trap #textbox", "dc.w \\1"],
            "serviceTarget": "#TEXTBOX",
        },
        {
            "definitionId": "event-service-macro:clsTxt",
            "family": "event-service-macro",
            "sourceMacro": "clsTxt",
            "sourcePath": "sf2macros.asm",
            "definitionSourceLine": 57,
            "formalParameterOrdinals": [],
            "emissionStatementTemplates": ["trap #textbox", "dc.w $ffff"],
            "serviceTarget": "#TEXTBOX",
        },
    ]
    definitions = _textbox_service_definitions(operation_definitions)
    assert definitions == [
        {
            "sourceKind": "line-reference",
            "definitionId": "event-service-macro:txt",
            "sourceMacro": "txt",
            "sourcePath": "sf2macros.asm",
            "definitionSourceLine": 52,
            "serviceTarget": "#TEXTBOX",
            "formalParameterOrdinals": [1],
            "emissionStatementTemplates": ["trap #textbox", "dc.w \\1"],
            "sentinelEncoding": None,
        },
        {
            "sourceKind": "close-sentinel",
            "definitionId": "event-service-macro:clsTxt",
            "sourceMacro": "clsTxt",
            "sourcePath": "sf2macros.asm",
            "definitionSourceLine": 57,
            "serviceTarget": "#TEXTBOX",
            "formalParameterOrdinals": [],
            "emissionStatementTemplates": ["trap #textbox", "dc.w $ffff"],
            "sentinelEncoding": "$FFFF",
        },
    ]
    assert _parse_textbox_line_operand(["0"], source_line=1) == 0
    assert _parse_textbox_line_operand(["$10"], source_line=1) == 16
    with pytest.raises(ValueError, match="line operand syntax drift"):
        _parse_textbox_line_operand(["00"], source_line=1)
    commented_txt = _parse_program_operation(
        _normalise_asm_statement("txt $000B ; clsTxt $FFFF"),
        source_line=10,
        source_order=0,
    )
    assert commented_txt["sourceMnemonic"] == "txt"
    assert commented_txt["operandTexts"] == ["$000B"]
    suffixed_branch = _parse_program_operation(
        _normalise_asm_statement("bne.s Next ; txt $FFFF"),
        source_line=11,
        source_order=1,
    )
    assert suffixed_branch["sizeSuffix"] == ".s"
    assert suffixed_branch["instructionTargetSymbol"] == "Next"

    def caller(
        canonical_symbol: str,
        entry_address: int,
        operations: list[dict[str, Any]],
    ) -> dict[str, Any]:
        return {
            "canonicalSymbol": canonical_symbol,
            "entryAddress": entry_address,
            "programOrder": 0,
            "sourcePath": f"data/maps/{canonical_symbol}.asm",
            "referenceCounts": {
                "physicalRecordCount": 2,
                "setupRecordReferenceCount": 3,
                "routeRecordReferenceCount": 4,
            },
            "operations": operations,
        }

    programs_by_category = {
        "entityEvents": [
            caller(
                "EntityCaller",
                0x1000,
                [
                    {
                        "sourceOrder": 0,
                        "sourceLine": 20,
                        "sourceMnemonic": "txt",
                        "operandTexts": ["$000B"],
                        "address": 0x1004,
                        "family": "event-service-macro",
                        "definitionId": "event-service-macro:txt",
                    },
                    {
                        "sourceOrder": 1,
                        "sourceLine": 21,
                        "sourceMnemonic": "clsTxt",
                        "operandTexts": [],
                        "address": 0x1008,
                        "family": "event-service-macro",
                        "definitionId": "event-service-macro:clsTxt",
                    },
                ],
            )
        ],
        "zoneEvents": [],
        "itemEvents": [],
    }
    sites = _textbox_reference_sites(programs_by_category, definitions, {11})
    assert sites == [
        {
            "siteOrder": 0,
            "sourceKind": "line-reference",
            "sourceMacro": "txt",
            "definitionId": "event-service-macro:txt",
            "category": "entityEvents",
            "callerProgramKey": "EntityCaller:4096",
            "callerProgramCanonicalSymbol": "EntityCaller",
            "callerProgramEntryAddress": 0x1000,
            "callerProgramOrder": 0,
            "callerSourcePath": "data/maps/EntityCaller.asm",
            "operationSourceOrder": 0,
            "sourceLine": 20,
            "operationAddress": 0x1004,
            "rawOperand": "$000B",
            "lineId": 11,
            "sentinelEncoding": None,
            "weightCounts": {
                "physicalProgramOccurrenceCount": 1,
                "physicalRecordWeightedSiteCount": 2,
                "setupRecordReferenceWeightedSiteCount": 3,
                "routeRecordReferenceWeightedSiteCount": 4,
            },
        },
        {
            "siteOrder": 1,
            "sourceKind": "close-sentinel",
            "sourceMacro": "clsTxt",
            "definitionId": "event-service-macro:clsTxt",
            "category": "entityEvents",
            "callerProgramKey": "EntityCaller:4096",
            "callerProgramCanonicalSymbol": "EntityCaller",
            "callerProgramEntryAddress": 0x1000,
            "callerProgramOrder": 0,
            "callerSourcePath": "data/maps/EntityCaller.asm",
            "operationSourceOrder": 1,
            "sourceLine": 21,
            "operationAddress": 0x1008,
            "rawOperand": None,
            "lineId": None,
            "sentinelEncoding": "$FFFF",
            "weightCounts": {
                "physicalProgramOccurrenceCount": 1,
                "physicalRecordWeightedSiteCount": 2,
                "setupRecordReferenceWeightedSiteCount": 3,
                "routeRecordReferenceWeightedSiteCount": 4,
            },
        },
    ]

    changed_definitions = copy.deepcopy(operation_definitions)
    changed_definitions[0]["emissionStatementTemplates"].reverse()
    with pytest.raises(ValueError, match="service emission/order drift"):
        _textbox_service_definitions(changed_definitions)

    changed_operand = copy.deepcopy(programs_by_category)
    changed_operand["entityEvents"][0]["operations"][0]["operandTexts"] = ["line_11"]
    with pytest.raises(ValueError, match="line operand syntax drift"):
        _textbox_reference_sites(changed_operand, definitions, {11})

    changed_domain = copy.deepcopy(programs_by_category)
    changed_domain["entityEvents"][0]["operations"][0]["operandTexts"] = ["12"]
    with pytest.raises(ValueError, match="line-domain coverage drift"):
        _textbox_reference_sites(changed_domain, definitions, {11})

    changed_close = copy.deepcopy(programs_by_category)
    changed_close["entityEvents"][0]["operations"][1]["operandTexts"] = ["11"]
    with pytest.raises(ValueError, match="close-sentinel operand drift"):
        _textbox_reference_sites(changed_close, definitions, {11})

    changed_order = copy.deepcopy(programs_by_category)
    changed_order["entityEvents"][0]["operations"][1]["sourceOrder"] = 2
    with pytest.raises(ValueError, match="source/use-site drift"):
        _textbox_reference_sites(changed_order, definitions, {11})


def test_text_line_domain_parser_is_source_rom_backed_not_golden_fixture(
    complete_output: dict[str, Any], tmp_path: Path, monkeypatch: pytest.MonkeyPatch
) -> None:
    changed_fixture = tmp_path / "text-banks-static-v1.json"
    changed_fixture.write_text('{"id":"wrong-golden"}', encoding="utf-8")
    monkeypatch.setattr(text_banks, "FIXTURE", changed_fixture)
    domain = build_text_line_domain_contract(
        Path("local/roms/sf2-us.bin"), Path("local/upstream/SF2DISASM")
    )
    assert domain == {
        "schemaVersion": 1,
        "id": "sf2-text-banks-static-v1",
        "upstream": {
            "repository": "https://github.com/ShiningForceCentral/SF2DISASM.git",
            "commit": "c834c652b6862bc5679fd7f69a38a7093206efc6",
        },
        "romSha256": "9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9",
        "summary": {"stringCount": 4267},
        "gamescriptFacts": {
            "sourcePath": "data/scripting/text/gamescript.txt",
            "byteCount": 203862,
            "lineIdCount": 4267,
            "firstLineId": 0,
            "lastLineId": 4266,
            "idsAreContiguous": True,
            "sha256": "14EB069436F9F77081AAFF6125312A9C277CB24255BE5F0D1EF108AF53AEA205",
        },
    }
    reconstructed = _textbox_reference_contract(
        complete_output["operationDefinitions"],
        {
            "entityEvents": complete_output["entityTargetPrograms"],
            "zoneEvents": complete_output["zoneTargetPrograms"],
            "itemEvents": complete_output["itemTargetPrograms"],
        },
        text_line_domain_contract=domain,
        upstream_commit=complete_output["upstream"]["commit"],
        rom_sha256=complete_output["romSha256"],
    )
    assert reconstructed["textboxLineDomain"] == complete_output["textboxLineDomain"]


def test_textbox_contract_reconciles_source_uses_weights_and_complete_order(
    complete_output: dict[str, Any],
) -> None:
    programs_by_category = {
        "entityEvents": complete_output["entityTargetPrograms"],
        "zoneEvents": complete_output["zoneTargetPrograms"],
        "itemEvents": complete_output["itemTargetPrograms"],
    }
    text_line_domain_contract = build_text_line_domain_contract(
        Path("local/roms/sf2-us.bin"), Path("local/upstream/SF2DISASM")
    )
    provenance = {
        "text_line_domain_contract": text_line_domain_contract,
        "upstream_commit": complete_output["upstream"]["commit"],
        "rom_sha256": complete_output["romSha256"],
    }
    textbox_fields = (
        "textboxLineDomain",
        "textboxServiceDefinitions",
        "textboxReferenceSites",
        "textboxCallerTotals",
        "textboxLineTotals",
        "textboxSummary",
        "textboxServiceDefinitionOrder",
        "textboxReferenceSiteOrder",
        "textboxCallerTotalOrder",
        "textboxLineTotalOrder",
    )
    textbox_contract = {
        field: copy.deepcopy(complete_output[field]) for field in textbox_fields
    }
    _reconcile_textbox_reference_contract(
        textbox_contract,
        complete_output["operationDefinitions"],
        programs_by_category,
        **provenance,
    )

    for field, value in (
        ("lineIdCount", text_line_domain_contract["gamescriptFacts"]["lineIdCount"] + 1),
        ("firstLineId", 1),
        ("lastLineId", text_line_domain_contract["gamescriptFacts"]["lastLineId"] - 1),
        ("idsAreContiguous", False),
        ("sourcePath", "data/scripting/text/other.txt"),
    ):
        changed_domain = copy.deepcopy(text_line_domain_contract)
        changed_domain["gamescriptFacts"][field] = value
        with pytest.raises(ValueError, match="text-line domain drift"):
            _textbox_reference_contract(
                complete_output["operationDefinitions"],
                programs_by_category,
                text_line_domain_contract=changed_domain,
                upstream_commit=complete_output["upstream"]["commit"],
                rom_sha256=complete_output["romSha256"],
            )

    changed_definition = copy.deepcopy(complete_output["operationDefinitions"])
    txt_definition = next(
        definition for definition in changed_definition if definition["sourceMacro"] == "txt"
    )
    txt_definition["emissionStatementTemplates"].reverse()
    with pytest.raises(ValueError, match="service emission/order drift"):
        _textbox_reference_contract(changed_definition, programs_by_category, **provenance)

    def changed_programs(
        mutate: Any,
    ) -> dict[str, list[dict[str, Any]]]:
        result = {category: list(programs) for category, programs in programs_by_category.items()}
        for category, programs in programs_by_category.items():
            for program_index, program in enumerate(programs):
                for operation_index, operation in enumerate(program["operations"]):
                    if operation["sourceMnemonic"] != "txt":
                        continue
                    operations = list(program["operations"])
                    changed_operation = dict(operation)
                    mutate(changed_operation, operation_index)
                    operations[operation_index] = changed_operation
                    result[category][program_index] = {**program, "operations": operations}
                    return result
        raise AssertionError("expected an in-scope txt use")

    with pytest.raises(ValueError, match="line operand syntax drift"):
        _textbox_reference_contract(
            complete_output["operationDefinitions"],
            changed_programs(
                lambda operation, _: operation.__setitem__("operandTexts", ["Line_11"])
            ),
            **provenance,
        )
    with pytest.raises(ValueError, match="line-domain coverage drift"):
        _textbox_reference_contract(
            complete_output["operationDefinitions"],
            changed_programs(
                lambda operation, _: operation.__setitem__("operandTexts", ["$FFFF"])
            ),
            **provenance,
        )
    with pytest.raises(ValueError, match="source/use-site drift"):
        _textbox_reference_contract(
            complete_output["operationDefinitions"],
            changed_programs(
                lambda operation, _: operation.__setitem__("sourceMnemonic", "clsTxt")
            ),
            **provenance,
        )
    with pytest.raises(ValueError, match="source/use-site drift"):
        _textbox_reference_contract(
            complete_output["operationDefinitions"],
            changed_programs(
                lambda operation, operation_index: operation.__setitem__(
                    "sourceOrder", operation_index + 1
                )
            ),
            **provenance,
        )

    changed_weight = copy.deepcopy(textbox_contract)
    changed_weight["textboxReferenceSites"][0]["weightCounts"][
        "routeRecordReferenceWeightedSiteCount"
    ] += 1
    with pytest.raises(ValueError, match="textboxReferenceSites reconciliation drift"):
        _reconcile_textbox_reference_contract(
            changed_weight,
            complete_output["operationDefinitions"],
            programs_by_category,
            **provenance,
        )

    changed_order = copy.deepcopy(textbox_contract)
    changed_order["textboxLineTotalOrder"].reverse()
    with pytest.raises(ValueError, match="textboxLineTotalOrder reconciliation drift"):
        _reconcile_textbox_reference_contract(
            changed_order,
            complete_output["operationDefinitions"],
            programs_by_category,
            **provenance,
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


def test_map_events_schema_composition_stays_local_and_golden_free() -> None:
    report = schema_composition_audit(list(map_events_fixture.COMPOSED_SCHEMAS))

    assert report["schemaCount"] == 13
    assert report["totalSizeBytes"] < 300_000
    assert report["constCount"] == 6
    assert report["constPayloadBytes"] == 81
    assert report["largeConstCount"] == 0
    assert report["referencedResourceCount"] == 10
    assert report["unresolvedReferences"] == []
    assert report["duplicateBodyGroups"] == []
    assert all(component["constCount"] == 0 for component in report["files"][:10])

    def exact_cardinalities(value: Any) -> list[int]:
        if isinstance(value, list):
            return [count for item in value for count in exact_cardinalities(item)]
        if not isinstance(value, dict):
            return []
        counts = []
        if "minItems" in value and value.get("minItems") == value.get("maxItems"):
            counts.append(value["minItems"])
        return counts + [
            count for child in value.values() for count in exact_cardinalities(child)
        ]

    for component_path in map_events_fixture.COMPOSED_SCHEMAS[:10]:
        assert exact_cardinalities(load_json(component_path)) == []


def test_map_events_roots_reuse_local_section_and_target_program_components() -> None:
    output_schema = load_json(SCHEMA)
    assert "definitions" not in output_schema
    assert [entry["$ref"] for entry in output_schema["allOf"]] == [
        load_json(schema_path)["$id"] for schema_path in SECTION_SCHEMAS.values()
    ]

    target_schema = load_json(map_events_fixture.TARGET_PROGRAM_SCHEMA)
    target_id = target_schema["$id"]
    assert {
        "entityTargetProgram",
        "entityTargetProgramOperation",
        "entityTargetProgramControlFlow",
        "mapEventTargetProgram",
        "mapEventTargetProgramExclusion",
    } <= set(target_schema["definitions"])

    for section, prefix in (
        ("entity-programs", "entity"),
        ("zone-programs", "zone"),
        ("item-programs", "item"),
    ):
        schema = load_json(SECTION_SCHEMAS[section])
        programs = schema["properties"][f"{prefix}TargetPrograms"]
        expected_definition = (
            "entityTargetProgram" if prefix == "entity" else "mapEventTargetProgram"
        )
        assert programs["items"] == {
            "$ref": f"{target_id}#/definitions/{expected_definition}"
        }
        assert schema["properties"][f"{prefix}TargetProgramControlFlow"] == {
            "$ref": f"{target_id}#/definitions/entityTargetProgramControlFlow"
        }

    routing_schema = load_json(SECTION_SCHEMAS["routing-setup"])
    for category, definition in (
        ("entityEvents", "entityEventRecord"),
        ("zoneEvents", "zoneEventRecord"),
        ("itemEvents", "itemEventRecord"),
    ):
        records = routing_schema["properties"]["categories"]["properties"][category][
            "properties"
        ]["tables"]["items"]["properties"]["records"]
        assert records["items"] == {"$ref": f"#/definitions/{definition}"}
        assert routing_schema["definitions"][definition]["additionalProperties"] is False

    for section, definitions in (
        (
            "direct-flags",
            ("directFlagAccessSite", "directFlagProgramTotal", "directFlagTotal"),
        ),
        (
            "script-invocation",
            ("scriptInvocationSite", "scriptInvocationCallerTotal"),
        ),
        ("textbox", ("textboxReferenceSite", "textboxLineTotal")),
        (
            "sound-commands",
            ("soundCommandSite", "soundCommandCallerTotal", "soundCommandSummary"),
        ),
    ):
        schema = load_json(SECTION_SCHEMAS[section])
        for definition in definitions:
            assert schema["definitions"][definition]["additionalProperties"] is False


def _sound_command_inputs(
    complete_output: dict[str, Any],
) -> tuple[list[dict[str, Any]], dict[str, list[dict[str, Any]]], dict[str, Any]]:
    """Return the source-built inputs for the direct ``sndCom`` corpus tests."""
    upstream_path = Path("local/upstream/SF2DISASM")
    return (
        complete_output["operationDefinitions"],
        {
            "entityEvents": complete_output["entityTargetPrograms"],
            "zoneEvents": complete_output["zoneTargetPrograms"],
            "itemEvents": complete_output["itemTargetPrograms"],
        },
        _build_sound_command_domain(
            Path("local/roms/sf2-us.bin"),
            upstream_path,
            disasm=upstream_path / "disasm",
            upstream_commit=complete_output["upstream"]["commit"],
            rom_sha256=complete_output["romSha256"],
        ),
    )


def test_sound_command_enum_parser_ignores_comments_and_near_misses() -> None:
    source = """
; MUSIC_COMMENT: equ $FF
MUSIC_TOWN: equ $08 ; retained direct source enum
SFX_BLAZE: equ 65
SOUND_COMMAND_FADE_OUT: equ $FD
XMUSIC_TOWN: equ $09
MUSIC-TOWN: equ $0A
MUSIC_MISSING_COLON equ $0B
"""

    assert _sound_command_enum_values(source) == {
        "MUSIC_TOWN": 8,
        "SFX_BLAZE": 65,
        "SOUND_COMMAND_FADE_OUT": 253,
    }


def test_sound_command_service_definition_guards_macro_target_emission_and_order(
    complete_output: dict[str, Any],
) -> None:
    definitions, _, _ = _sound_command_inputs(complete_output)
    assert _sound_command_service_definition(definitions) == {
        "definitionId": "event-service-macro:sndCom",
        "sourceMacro": "sndCom",
        "sourcePath": "sf2macros.asm",
        "definitionSourceLine": 27,
        "serviceTarget": "#SOUND_COMMAND",
        "operandOrdinal": 1,
        "emissionStatementTemplates": ["trap #sound_command", "dc.w \\1"],
    }

    missing = [
        definition
        for definition in definitions
        if definition["definitionId"] != "event-service-macro:sndCom"
    ]
    with pytest.raises(ValueError, match="service-definition coverage"):
        _sound_command_service_definition(missing)

    for field, value, expected_error in (
        ("sourceMacro", "sndComNearMiss", "service emission/order"),
        ("serviceTarget", "#TEXTBOX", "service-definition coverage"),
        (
            "emissionStatementTemplates",
            ["dc.w \\1", "trap #sound_command"],
            "service emission/order",
        ),
    ):
        mutated = copy.deepcopy(definitions)
        definition = next(
            row
            for row in mutated
            if row["definitionId"] == "event-service-macro:sndCom"
        )
        definition[field] = value
        with pytest.raises(ValueError, match=expected_error):
            _sound_command_service_definition(mutated)


def test_sound_command_contract_is_complete_and_guards_source_relations(
    complete_output: dict[str, Any],
) -> None:
    definitions, programs_by_category, sound_domain = _sound_command_inputs(complete_output)
    contract = _sound_command_reference_contract(
        definitions,
        programs_by_category,
        sound_domain=sound_domain,
    )
    expected_fields = {
        "soundCommandSites",
        "soundCommandCallerTotals",
        "soundCommandSummary",
    }
    assert set(contract) == expected_fields
    assert contract["soundCommandSummary"] == {
        "soundDataContractId": "sf2-sound-data-static-v1",
        "siteCount": 3,
        "observedSourceSymbolCount": 3,
        "observedResolvedValueCount": 3,
        "completeCallerProgramCount": 914,
        "positiveCallerProgramCount": 1,
        "zeroCallerProgramCount": 913,
        "weightCounts": {
            "physicalProgramOccurrenceCount": 3,
            "physicalRecordWeightedSiteCount": 3,
            "setupRecordReferenceWeightedSiteCount": 3,
            "routeRecordReferenceWeightedSiteCount": 3,
        },
        "sourceCategorySiteCounts": {"music": 1, "sfx": 0, "sound-command": 2},
    }
    assert contract["soundCommandSites"] == [
        {
            "category": "zoneEvents",
            "callerProgramKey": "Map20_21F_ZoneEvent0:406188",
            "operationSourceOrder": 0,
            "sourceOperand": "SOUND_COMMAND_FADE_OUT",
            "resolvedValue": 253,
            "sourceCategory": "sound-command",
            "weightCounts": {
                "physicalProgramOccurrenceCount": 1,
                "physicalRecordWeightedSiteCount": 1,
                "setupRecordReferenceWeightedSiteCount": 1,
                "routeRecordReferenceWeightedSiteCount": 1,
            },
        },
        {
            "category": "zoneEvents",
            "callerProgramKey": "Map20_21F_ZoneEvent0:406188",
            "operationSourceOrder": 12,
            "sourceOperand": "SOUND_COMMAND_INIT_DRIVER",
            "resolvedValue": 32,
            "sourceCategory": "sound-command",
            "weightCounts": {
                "physicalProgramOccurrenceCount": 1,
                "physicalRecordWeightedSiteCount": 1,
                "setupRecordReferenceWeightedSiteCount": 1,
                "routeRecordReferenceWeightedSiteCount": 1,
            },
        },
        {
            "category": "zoneEvents",
            "callerProgramKey": "Map20_21F_ZoneEvent0:406188",
            "operationSourceOrder": 13,
            "sourceOperand": "MUSIC_TOWN",
            "resolvedValue": 8,
            "sourceCategory": "music",
            "weightCounts": {
                "physicalProgramOccurrenceCount": 1,
                "physicalRecordWeightedSiteCount": 1,
                "setupRecordReferenceWeightedSiteCount": 1,
                "routeRecordReferenceWeightedSiteCount": 1,
            },
        },
    ]
    assert contract["soundCommandCallerTotals"] == [
        {
            "callerProgramKey": "Map20_21F_ZoneEvent0:406188",
            "siteCount": 3,
            "weightCounts": {
                "physicalProgramOccurrenceCount": 3,
                "physicalRecordWeightedSiteCount": 3,
                "setupRecordReferenceWeightedSiteCount": 3,
                "routeRecordReferenceWeightedSiteCount": 3,
            },
        }
    ]
    for operand, expected_error in (
        ("MUSIC_UNDECLARED", "enum resolution"),
        ("NOT_A_SOUND_ENUM", "operand namespace"),
    ):
        mutated_programs = copy.deepcopy(programs_by_category)
        mutated_programs["zoneEvents"][39]["operations"][13]["operandTexts"] = [operand]
        with pytest.raises(ValueError, match=expected_error):
            _sound_command_reference_contract(
                definitions,
                mutated_programs,
                sound_domain=sound_domain,
            )

    bad_value_domain = copy.deepcopy(sound_domain)
    bad_value_domain["_enumValues"]["MUSIC_TOWN"] = 65
    with pytest.raises(ValueError, match="music-domain"):
        _sound_command_reference_contract(
            definitions,
            programs_by_category,
            sound_domain=bad_value_domain,
        )

    reordered_programs = copy.deepcopy(programs_by_category)
    reordered_programs["zoneEvents"][39]["operations"][13]["sourceOrder"] = 14
    with pytest.raises(ValueError, match="source/use-site"):
        _sound_command_reference_contract(
            definitions,
            reordered_programs,
            sound_domain=sound_domain,
        )

    reweighted_programs = copy.deepcopy(programs_by_category)
    reweighted_programs["zoneEvents"][39]["referenceCounts"]["physicalRecordCount"] = 2
    with pytest.raises(ValueError, match="sound-command"):
        _reconcile_sound_command_reference_contract(
            contract,
            definitions,
            reweighted_programs,
            sound_domain=sound_domain,
        )

    for mutate in (
        lambda value: value.__setitem__("soundCommandCallerTotals", []),
        lambda value: value["soundCommandSummary"].__setitem__(
            "zeroCallerProgramCount", 912
        ),
        lambda value: value["soundCommandCallerTotals"][0].__setitem__(
            "siteCount", 2
        ),
    ):
        stale = copy.deepcopy(contract)
        mutate(stale)
        with pytest.raises(ValueError, match="sound-command"):
            _reconcile_sound_command_reference_contract(
                stale,
                definitions,
                programs_by_category,
                sound_domain=sound_domain,
            )


def test_sound_command_schema_closes_structure_while_owner_closes_corpus(
    complete_output: dict[str, Any],
) -> None:
    fixture = load_map_events_fixture()
    validate_json(complete_output, SCHEMA, owner="sound-command baseline")

    for owner, mutate in (
        (
            "sound-command missing nested field",
            lambda target: target["soundCommandSites"][0].pop("sourceOperand"),
        ),
        (
            "sound-command extra nested field",
            lambda target: target["soundCommandCallerTotals"][0]["weightCounts"].__setitem__(
                "unexpected",
                {
                    "physicalProgramOccurrenceCount": 0,
                    "physicalRecordWeightedSiteCount": 0,
                    "setupRecordReferenceWeightedSiteCount": 0,
                    "routeRecordReferenceWeightedSiteCount": 0,
                },
            ),
        ),
        (
            "sound-command renamed field",
            lambda target: target["soundCommandSummary"].__setitem__(
                "siteCountRenamed", target["soundCommandSummary"].pop("siteCount")
            ),
        ),
        (
            "sound-command category boundary",
            lambda target: target["soundCommandSites"][2].__setitem__(
                "resolvedValue", 65
            ),
        ),
    ):
        broken = copy.deepcopy(complete_output)
        mutate(broken)
        with pytest.raises(ValueError, match=owner):
            validate_json(broken, SCHEMA, owner=owner)

    for mutate in (
        lambda target: target["soundCommandSites"].reverse(),
        lambda target: target["soundCommandSummary"].__setitem__(
            "zeroCallerProgramCount", 912
        ),
    ):
        semantic = copy.deepcopy(complete_output)
        mutate(semantic)
        validate_json(semantic, SCHEMA, owner="schema-valid sound-command corpus drift")
        with pytest.raises(ValueError, match="complete semantic fixture drift"):
            _verify_complete_map_events_fixture(fixture, semantic)

    assert complete_output["soundCommandSummary"]["siteCount"] == 3
