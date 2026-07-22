import copy
from pathlib import Path

import pytest

from sf2tool.h2.map_init import (
    FIXTURE,
    FIXTURE_SCHEMA,
    SCHEMA,
    _dispatcher_use_sites,
    _init_function_pointer_layout_row,
    _operand_symbol,
    _operation_family,
    _operation_rows,
    _parse_equates,
    _parse_jump_interface_aliases,
    _route_joins,
    _script_target_profiles,
    _validate_macro_sources,
    build_map_init_contract,
)
from sf2tool.jsonio import load_json, validate_json


def test_operation_rows_preserve_labels_and_resolve_local_branches() -> None:
    operations = _operation_rows(
        """
@Again:
    chkFlg 42
    beq.s @Again
    script cs_Test
    rts
"""
    )

    assert operations[0]["labels"] == ["@Again"]
    assert operations[0]["opcode"] == "chkFlg"
    assert operations[0]["operandText"] == "42"
    assert operations[1]["branchTargetSymbol"] == "@Again"
    assert operations[1]["localBranchTargetIndex"] == 0
    assert operations[2]["opcode"] == "script"
    assert operations[2]["operandText"] == "cs_Test"


def test_dispatcher_use_sites_require_ordered_branches_and_enum_backed_pointer_load() -> None:
    source = """
RunMapSetupInitFunction:
    movem.l d0-a1,-(sp)
    bsr.w GetCurrentMapSetup
    cmpi.w #-1,(a0)
    bne.s @Call
    bra.w @Return
@Call:
    movea.l MAPSETUP_OFFSET_INIT_FUNCTION(a0),a0
    jsr (a0)
@Return:
    movem.l (sp)+,d0-a1
    rts
; End of function RunMapSetupInitFunction
    """
    constants = _parse_equates("MAPSETUP_OFFSET_INIT_FUNCTION: equ 20\n")
    layout_row = {"sourceOrder": 5, "name": "initFunction", "offset": 20}

    assert _dispatcher_use_sites(source, constants, layout_row) == {
        "pointerOffsetConstant": {"name": "MAPSETUP_OFFSET_INIT_FUNCTION", "value": 20},
        "pointerLayoutRow": {"sourceOrder": 5, "name": "initFunction", "offset": 20},
        "pointerLoadUseSite": {
            "role": "load-init-pointer",
            "index": 5,
            "opcode": "movea.l",
            "operandText": "MAPSETUP_OFFSET_INIT_FUNCTION(a0),a0",
            "offsetConstantName": "MAPSETUP_OFFSET_INIT_FUNCTION",
            "resolvedOffset": 20,
        },
        "useSites": [
            {
                "role": "save-registers",
                "index": 0,
                "opcode": "movem.l",
                "operandText": "d0-a1,-(sp)",
            },
            {
                "role": "select-setup",
                "index": 1,
                "opcode": "bsr.w",
                "operandText": "GetCurrentMapSetup",
            },
            {
                "role": "missing-setup-compare",
                "index": 2,
                "opcode": "cmpi.w",
                "operandText": "#-1,(a0)",
            },
            {"role": "non-missing-branch", "index": 3, "opcode": "bne.s", "operandText": "@Call"},
            {
                "role": "missing-setup-branch",
                "index": 4,
                "opcode": "bra.w",
                "operandText": "@Return",
            },
            {
                "role": "load-init-pointer",
                "index": 5,
                "opcode": "movea.l",
                "operandText": "MAPSETUP_OFFSET_INIT_FUNCTION(a0),a0",
            },
            {"role": "indirect-init-call", "index": 6, "opcode": "jsr", "operandText": "(a0)"},
            {
                "role": "restore-registers",
                "index": 7,
                "opcode": "movem.l",
                "operandText": "(sp)+,d0-a1",
            },
            {"role": "return", "index": 8, "opcode": "rts", "operandText": ""},
        ],
    }

    with pytest.raises(ValueError, match="dispatcher use-site drift"):
        _dispatcher_use_sites(source.replace("jsr (a0)", "jsr (a1)"), constants, layout_row)
    with pytest.raises(ValueError, match="branch-target relation drift"):
        _dispatcher_use_sites(
            source.replace("bra.w @Return", "bra.w @Call"), constants, layout_row
        )
    with pytest.raises(ValueError, match="enum/layout"):
        _dispatcher_use_sites(
            source,
            {"MAPSETUP_OFFSET_INIT_FUNCTION": 24},
            layout_row,
        )
    with pytest.raises(ValueError, match="enum/layout"):
        _dispatcher_use_sites(source, constants, {**layout_row, "offset": 24})
    with pytest.raises(ValueError, match="pointer-layout"):
        _dispatcher_use_sites(source, constants, {**layout_row, "name": "zoneEvents"})
    with pytest.raises(ValueError, match="dispatcher use-site drift"):
        _dispatcher_use_sites(
            source.replace(
                "MAPSETUP_OFFSET_INIT_FUNCTION(a0),a0", "MAPSETUP_OFFSET_ZONE_EVENTS(a0),a0"
            ),
            constants,
            layout_row,
        )
    with pytest.raises(ValueError, match="lacks one initFunction"):
        _init_function_pointer_layout_row(
            [{"name": "entities", "offset": 0}, {"name": "zoneEvents", "offset": 8}]
        )


def test_operation_parser_ignores_comments_and_accepts_legal_call_and_branch_suffixes() -> None:
    operations = _operation_rows(
        """
; jsr GhostTarget must remain a comment
@Start: jsr.w (RealTarget).w ; TargetInComment must not become an operand
    bne.l @Start ; branch SymbolInComment
    move.w #0,d0 ; jsr AnotherGhost
"""
    )

    assert [(row["opcode"], row["operandText"]) for row in operations] == [
        ("jsr.w", "(RealTarget).w"),
        ("bne.l", "@Start"),
        ("move.w", "#0,d0"),
    ]
    assert operations[1]["localBranchTargetIndex"] == 0
    assert _operand_symbol("(RealTarget).w") == "RealTarget"
    assert _operand_symbol("RealTarget(pc)") == "RealTarget"
    assert _operation_family("jsr.w") == "direct-call"
    assert _operation_family("bne.l") == "branch-or-jump"
    with pytest.raises(ValueError, match="unclassified"):
        _operation_family("jsrx.w")
    with pytest.raises(ValueError, match="no unique symbol"):
        _operand_symbol("d0,d1")


def test_route_join_requires_a_resolved_pointer_table_and_target_profile() -> None:
    setup = {
        "routes": [
            {
                "map": 3,
                "defaultPointer": "ms_map3",
                "flagVariants": [{"flag": 609, "pointer": "ms_map3_flag609"}],
            }
        ],
        "pointerTables": [
            {
                "path": "data/maps/entries/map03/mapsetups/pointertable.asm",
                "symbol": "ms_map3",
                "address": 0x1000,
                "targets": {"initFunction": {"symbol": "ms_map3_InitFunction", "address": 0x2000}},
            },
            {
                "path": "data/maps/entries/map03/mapsetups/pointertable_609.asm",
                "symbol": "ms_map3_flag609",
                "address": 0x1018,
                "targets": {
                    "initFunction": {"symbol": "ms_map3_flag609_InitFunction", "address": 0x2020}
                },
            },
        ],
    }
    profiles = [
        {"symbol": "ms_map3_InitFunction", "address": 0x2000},
        {"symbol": "ms_map3_flag609_InitFunction", "address": 0x2020},
    ]

    routes, pointers = _route_joins(setup, profiles)
    assert [row["targetProfileSymbol"] for row in routes] == [
        "ms_map3_InitFunction",
        "ms_map3_flag609_InitFunction",
    ]
    assert pointers[1]["routeReferenceSourceOrders"] == [1]

    broken = copy.deepcopy(setup)
    broken["routes"][0]["flagVariants"][0]["pointer"] = "ms_missing"
    with pytest.raises(ValueError, match="unknown setup table"):
        _route_joins(broken, profiles)

    different_valid_target = copy.deepcopy(setup)
    different_valid_target["pointerTables"][1]["targets"]["initFunction"]["symbol"] = (
        "ms_map3_InitFunction"
    )
    with pytest.raises(ValueError, match="address identity drift"):
        _route_joins(different_valid_target, profiles)


def test_source_form_and_resolution_mutations_fail_before_fixture_comparison() -> None:
    trap_macros = r"""
chkFlg: macro
    trap #CHECK_FLAG
    dc.w \1
endm
setFlg: macro
    trap #SET_FLAG
    dc.w \1
endm
clrFlg: macro
    trap #CLEAR_FLAG
    dc.w \1
endm
script: macro
    lea \1(pc), a0
    trap #MAPSCRIPT
endm
sndCom: macro
    trap #SOUND_COMMAND
    dc.w \1
endm
txt: macro
    trap #TEXTBOX
    dc.w \1
endm
clsTxt: macro
    trap #TEXTBOX
    dc.w $FFFF
endm
"""
    cutscene_macros = r"""
warp: macro
    dc.w $07
    dc.b \1
    dc.b \4
endm
setStoryFlag: macro
    dc.w $13
    dc.w \1
endm
setPos: macro
    dc.w $19
    dc.b \1
    dc.b \4
endm
csc_end: macro
    dc.w $FFFF
endm
"""
    _validate_macro_sources(trap_macros, cutscene_macros)
    for original, replacement in (
        ("trap #CHECK_FLAG", "trap #CHECK_FLAG_CHANGED"),
        ("trap #MAPSCRIPT", "trap #MAPSCRIPT_CHANGED"),
        ("trap #SOUND_COMMAND", "trap #SOUND_COMMAND_CHANGED"),
    ):
        with pytest.raises(ValueError, match="macro shape drift"):
            _validate_macro_sources(
                trap_macros.replace(original, replacement, 1), cutscene_macros
            )
    with pytest.raises(ValueError, match="macro shape drift"):
        _validate_macro_sources(trap_macros, cutscene_macros.replace("dc.w $07", "dc.w $08", 1))

    addresses = {"j_Alias": 0, "Target": 16, "Different": 20}
    rom = b"\x4e\xfa\x00\x0e" + b"\x00" * 20
    aliases = _parse_jump_interface_aliases(
        {"alias.asm": "j_Alias:\n    jmp Target(pc)\n"},
        {"j_Alias"},
        addresses,
        rom,
    )
    assert aliases == {"j_Alias": "Target"}
    with pytest.raises(ValueError, match="source/ROM target drift"):
        _parse_jump_interface_aliases(
            {"alias.asm": "j_Alias:\n    jmp Different(pc)\n"},
            {"j_Alias"},
            addresses,
            rom,
        )

    primary_bodies = [{"operations": [{"opcode": "script", "operandText": "cs_Target"}]}]
    definition = {"id": "cs_Target", "address": 0x4000, "path": "embedded.asm"}
    profiles = _script_target_profiles(primary_bodies, [definition], [], {"cs_Target": 0x4000})
    assert profiles["cs_Target"]["resolution"] == "embedded-init-source"
    with pytest.raises(ValueError, match="lack definitions"):
        _script_target_profiles(primary_bodies, [], [], {"cs_Target": 0x4000})


def test_complete_map_init_contract_matches_the_full_golden_fixture() -> None:
    output = build_map_init_contract(
        Path("local/roms/sf2-us.bin"), Path("local/upstream/SF2DISASM")
    )
    fixture = load_json(FIXTURE)
    validate_json(output, SCHEMA, owner="map init complete output")
    validate_json(fixture, FIXTURE_SCHEMA, owner="map init complete fixture")
    assert fixture["function"] == output["function"]
    assert fixture["expected"] == {
        key: value
        for key, value in output.items()
        if key not in {"schemaVersion", "id", "upstream", "romSha256", "function"}
    }
    assert output["summary"] == fixture["expected"]["summary"]
    assert output["operationFamilyCounts"] == {
        "flag-read": 101,
        "flag-write": 36,
        "script-invocation": 80,
        "direct-call": 45,
        "entity-or-position-command": 12,
        "warp-or-transition-command": 2,
        "presentation-audio-text-command": 20,
        "arithmetic-or-data-movement": 68,
        "branch-or-jump": 131,
        "terminal": 102,
    }
    assert output["setupWeightedOperationFamilyCounts"] == {
        "flag-read": 179,
        "flag-write": 49,
        "script-invocation": 132,
        "direct-call": 65,
        "entity-or-position-command": 29,
        "warp-or-transition-command": 2,
        "presentation-audio-text-command": 22,
        "arithmetic-or-data-movement": 97,
        "branch-or-jump": 236,
        "terminal": 162,
    }
    assert output["routeWeightedOperationFamilyCounts"] == {
        "flag-read": 203,
        "flag-write": 53,
        "script-invocation": 150,
        "direct-call": 65,
        "entity-or-position-command": 43,
        "warp-or-transition-command": 2,
        "presentation-audio-text-command": 22,
        "arithmetic-or-data-movement": 102,
        "branch-or-jump": 280,
        "terminal": 180,
    }
    assert sum(output["setupWeightedOperationFamilyCounts"].values()) == 973
    assert sum(output["routeWeightedOperationFamilyCounts"].values()) == 1100
    assert {
        key: output["summary"][key]
        for key in (
            "embeddedInitSourceScriptTargetCount",
            "standaloneScriptTargetCount",
            "unresolvedScriptTargetCount",
        )
    } == {
        "embeddedInitSourceScriptTargetCount": 63,
        "standaloneScriptTargetCount": 12,
        "unresolvedScriptTargetCount": 0,
    }
    assert output["directCallInstructionTargetCounts"] == {
        "ChangeEntityFacing": 1,
        "FadeInFromBlack": 5,
        "InitializeNazcaShipForceMembers": 2,
        "MoveEntityOutOfMap": 35,
        "j_FadeOut_WaitForP1Input": 1,
        "j_alt_YesNoPrompt": 1,
    }
    assert output["directCallEffectiveTargetCounts"] == {
        "ChangeEntityFacing": 1,
        "FadeInFromBlack": 5,
        "FadeOut_WaitForP1Input": 1,
        "InitializeNazcaShipForceMembers": 2,
        "MoveEntityOutOfMap": 35,
        "alt_YesNoPrompt": 1,
    }
    assert {
        key: output["summary"][key]
        for key in (
            "sourceFileCount",
            "setupPointerReferenceCount",
            "routeReferenceCount",
            "uniqueTargetCount",
            "sourceStatementCount",
            "setupStatementReferenceCount",
            "unresolvedScriptTargetCount",
            "unclassifiedOperationCount",
        )
    } == {
        "sourceFileCount": 84,
        "setupPointerReferenceCount": 126,
        "routeReferenceCount": 130,
        "uniqueTargetCount": 90,
        "sourceStatementCount": 597,
        "setupStatementReferenceCount": 973,
        "unresolvedScriptTargetCount": 0,
        "unclassifiedOperationCount": 0,
    }
    assert len(output["routeJoins"]) == 130
    assert len(output["pointerTableJoins"]) == 126
    assert len(output["scriptCallSites"]) == 80
    assert len(output["directCallSites"]) == 45
    assert output["dispatcher"] == fixture["expected"]["dispatcher"]
    assert output["dispatcher"]["pointerOffsetConstant"] == {
        "name": "MAPSETUP_OFFSET_INIT_FUNCTION",
        "value": 20,
    }
    assert output["dispatcher"]["pointerLayoutRow"] == {
        "sourceOrder": 5,
        "name": "initFunction",
        "offset": 20,
    }
    assert output["dispatcher"]["pointerLoadUseSite"] == {
        "role": "load-init-pointer",
        "index": 5,
        "opcode": "movea.l",
        "operandText": "MAPSETUP_OFFSET_INIT_FUNCTION(a0),a0",
        "offsetConstantName": "MAPSETUP_OFFSET_INIT_FUNCTION",
        "resolvedOffset": 20,
    }
    assert output["runtimeQuestions"] == fixture["expected"]["runtimeQuestions"]


def test_map_init_output_schema_rejects_nested_missing_extra_order_and_boundary_mutations() -> None:
    output = build_map_init_contract(
        Path("local/roms/sf2-us.bin"), Path("local/upstream/SF2DISASM")
    )

    missing = copy.deepcopy(output)
    del missing["primarySourceBodies"][0]["operations"][0]["family"]
    with pytest.raises(ValueError):
        validate_json(missing, SCHEMA, owner="missing nested family")

    extra = copy.deepcopy(output)
    extra["primarySourceBodies"][0]["operations"][0]["unexpected"] = 1
    with pytest.raises(ValueError):
        validate_json(extra, SCHEMA, owner="extra nested operation property")

    reordered = copy.deepcopy(output)
    reordered["routeJoinOrder"].reverse()
    with pytest.raises(ValueError):
        validate_json(reordered, SCHEMA, owner="reordered route join order")

    boundary = copy.deepcopy(output)
    boundary["routeJoins"][1]["selectorFlag"] = -1
    with pytest.raises(ValueError):
        validate_json(boundary, SCHEMA, owner="negative route flag")


def test_map_init_fixture_schema_rejects_nested_mutations() -> None:
    fixture = load_json(FIXTURE)

    renamed = copy.deepcopy(fixture)
    operation = renamed["expected"]["primarySourceBodies"][0]["operations"][0]
    operation["operationFamily"] = operation.pop("family")
    with pytest.raises(ValueError):
        validate_json(renamed, FIXTURE_SCHEMA, owner="renamed fixture nested property")

    extra = copy.deepcopy(fixture)
    extra["expected"]["scriptCallSites"][0]["unexpected"] = "extra"
    with pytest.raises(ValueError):
        validate_json(extra, FIXTURE_SCHEMA, owner="extra fixture nested property")

    reordered = copy.deepcopy(fixture)
    reordered["expected"]["primaryOperationOrder"].reverse()
    with pytest.raises(ValueError):
        validate_json(reordered, FIXTURE_SCHEMA, owner="reordered fixture operation order")

    boundary = copy.deepcopy(fixture)
    boundary["expected"]["sourceFiles"][0]["firstOperationIndex"] = -1
    with pytest.raises(ValueError):
        validate_json(boundary, FIXTURE_SCHEMA, owner="negative fixture operation boundary")
