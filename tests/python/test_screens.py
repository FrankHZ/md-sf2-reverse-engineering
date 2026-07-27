from __future__ import annotations

from copy import deepcopy

import pytest

from sf2tool.h2.screens import (
    _direct_call_sites,
    _read_equates,
    _witch_actions,
    _witch_dispatch_table,
    _witch_main_menu_facts,
    _witch_save_menu_facts_from_sources,
    _witch_save_menu_provenance,
    build_special_screen_inventory,
)
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path
from sf2tool.source_text import read_upstream_text

UPSTREAM = repo_path("local/upstream/SF2DISASM")


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_witch_dispatch_reorder_or_target_mutation_fails_construction() -> None:
    source = read_upstream_text(UPSTREAM / "disasm/code/specialscreens/witch/witchstart.asm")
    listing = (UPSTREAM / "build/sf2build-h1.lst").read_text(encoding="utf-8")

    records = _witch_dispatch_table(source, listing)
    assert records == [
        {
            "index": 0,
            "sourceLine": 136,
            "target": "witchMenuAction_New",
            "targetAddress": 29702,
        },
        {
            "index": 1,
            "sourceLine": 137,
            "target": "witchMenuAction_Load",
            "targetAddress": 29922,
        },
        {
            "index": 2,
            "sourceLine": 138,
            "target": "witchMenuAction_Del",
            "targetAddress": 30068,
        },
        {
            "index": 3,
            "sourceLine": 139,
            "target": "witchMenuAction_Copy",
            "targetAddress": 30028,
        },
    ]

    reordered = (
        source.replace("witchMenuAction_Load-rjt", "witchMenuAction_Temporary-rjt")
        .replace("witchMenuAction_Del-rjt", "witchMenuAction_Load-rjt")
        .replace("witchMenuAction_Temporary-rjt", "witchMenuAction_Del-rjt")
    )
    with pytest.raises(ValueError, match="dispatch target/order"):
        _witch_dispatch_table(reordered, listing)

    renamed_target = source.replace(
        "dc.w witchMenuAction_Copy-rjt_WitchMenuActions",
        "dc.w witchMenuAction_Renamed-rjt_WitchMenuActions",
    )
    with pytest.raises(ValueError, match="dispatch target/order"):
        _witch_dispatch_table(renamed_target, listing)


def test_witch_direct_call_parser_ignores_comments_labels_and_operands() -> None:
    assert _direct_call_sites(
        """\
                bsr.s   CheckSram
label:          bsr.w   SaveGame
                jsr     (LoadGame).w
                jsr.l   (CopySave)
                bsr.w   ClearSaveSlotFlag(pc)
;               bsr.w   CopySave
                move.w  #ClearSaveSlotFlag,d0
                dc.b    'bsr.w CopySave'
macro           bsr.w   CopySave
                jmp     ClearSaveSlotFlag
                bsr.x   CheckSram
                xbsr.w  SaveGame
"""
    ) == [
        {"line": 1, "instructionTarget": "CheckSram"},
        {"line": 2, "instructionTarget": "SaveGame"},
        {"line": 3, "instructionTarget": "LoadGame"},
        {"line": 4, "instructionTarget": "CopySave"},
        {"line": 5, "instructionTarget": "ClearSaveSlotFlag"},
    ]


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_witch_save_menu_contract_is_complete_and_schema_rejects_nested_mutations() -> None:
    fixture = load_json(repo_path("tests/fixtures/h2/special-screens-static-v1.json"))
    output = build_special_screen_inventory(UPSTREAM)
    facts = output["screenFacts"]["witchSaveMenu"]

    assert facts == fixture["expected"]["screenFacts"]["witchSaveMenu"]
    assert facts["sramServiceCalls"]["internalEffectiveTargetSiteCounts"] == {
        "CheckSram": 0,
        "SaveGame": 0,
        "LoadGame": 0,
        "CopySave": 0,
        "ClearSaveSlotFlag": 0,
    }
    assert facts["sramServiceCalls"]["externalEffectiveTargetSiteCounts"] == {
        "CheckSram": 1,
        "SaveGame": 1,
        "LoadGame": 1,
        "CopySave": 1,
        "ClearSaveSlotFlag": 1,
    }
    assert facts["runtimeQuestions"] == ["witch-save-menu-and-suspend-presentation"]
    assert len(facts["provenance"]["useSites"]) == 118
    assert {
        use_site_id
        for use_site_ids in facts["provenance"]["summaryUseSiteIds"].values()
        for use_site_id in use_site_ids
    } == {site["id"] for site in facts["provenance"]["useSites"]}

    fixture_schema = repo_path("schemas/h2-special-screens-static-fixture.schema.json")
    output_schema = repo_path("schemas/special-screens-static.schema.json")
    assert load_json(output_schema)["definitions"] == load_json(fixture_schema)["definitions"]
    malformed_fixture = deepcopy(fixture)
    del malformed_fixture["expected"]["screenFacts"]["witchSaveMenu"]["mainMenu"]["cancel"][
        "returnValue"
    ]
    with pytest.raises(ValueError, match="screenFacts"):
        validate_json(malformed_fixture, fixture_schema, owner="missing nested witch field")

    malformed_fixture = deepcopy(fixture)
    cancel = malformed_fixture["expected"]["screenFacts"]["witchSaveMenu"]["mainMenu"]["cancel"]
    cancel["renamedReturnValue"] = cancel.pop("returnValue")
    with pytest.raises(ValueError, match="screenFacts"):
        validate_json(malformed_fixture, fixture_schema, owner="renamed nested witch field")

    malformed_fixture = deepcopy(fixture)
    malformed_fixture["expected"]["screenFacts"]["witchSaveMenu"]["mainMenu"]["cancel"]["extra"] = (
        True
    )
    with pytest.raises(ValueError, match="screenFacts"):
        validate_json(malformed_fixture, fixture_schema, owner="extra nested witch field")

    malformed_output = deepcopy(output)
    dispatcher = malformed_output["screenFacts"]["witchSaveMenu"]["dispatcher"]
    dispatcher[1], dispatcher[2] = dispatcher[2], dispatcher[1]
    with pytest.raises(ValueError, match="screenFacts"):
        validate_json(malformed_output, output_schema, owner="reordered witch dispatch")

    malformed_output = deepcopy(output)
    malformed_output["screenFacts"]["witchSaveMenu"]["mainMenu"]["navigation"][
        "availableBitPositions"
    ][3] = 4
    with pytest.raises(ValueError, match="screenFacts"):
        validate_json(malformed_output, output_schema, owner="out-of-bound witch option")

    malformed_fixture = deepcopy(fixture)
    del malformed_fixture["expected"]["screenFacts"]["witchSaveMenu"]["provenance"]["useSites"][0][
        "operand"
    ]
    with pytest.raises(ValueError, match="screenFacts"):
        validate_json(malformed_fixture, fixture_schema, owner="missing witch provenance field")

    malformed_output = deepcopy(output)
    malformed_output["screenFacts"]["witchSaveMenu"]["provenance"]["useSites"][0]["extra"] = True
    with pytest.raises(ValueError, match="screenFacts"):
        validate_json(malformed_output, output_schema, owner="extra witch provenance field")

    malformed_output = deepcopy(output)
    use_sites = malformed_output["screenFacts"]["witchSaveMenu"]["provenance"]["useSites"]
    use_sites[0], use_sites[1] = use_sites[1], use_sites[0]
    with pytest.raises(ValueError, match="screenFacts"):
        validate_json(malformed_output, output_schema, owner="reordered witch provenance")

    malformed_output = deepcopy(output)
    malformed_output["screenFacts"]["witchSaveMenu"]["provenance"]["useSites"][0][
        "sourceLine"
    ] = 0
    with pytest.raises(ValueError, match="screenFacts"):
        validate_json(malformed_output, output_schema, owner="out-of-bound witch provenance")


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_witch_use_site_mutations_fail_before_fixture_comparison() -> None:
    start_source = read_upstream_text(UPSTREAM / "disasm/code/specialscreens/witch/witchstart.asm")
    main_source = read_upstream_text(
        UPSTREAM / "disasm/code/specialscreens/witch/witchmainmenu.asm"
    )
    listing = (UPSTREAM / "build/sf2build-h1.lst").read_text(encoding="utf-8")
    constants = _read_equates(
        UPSTREAM / "disasm/sf2enums.asm",
        (
            "BYTE_LOWER_NIBBLE_MASK",
            "GAMESTART_MAP",
            "GAMESTART_SAVEPOINT_X",
            "GAMESTART_SAVEPOINT_Y",
            "GAMESTART_FACING",
        ),
    )
    dispatch_indices = {
        record["target"]: record["index"] for record in _witch_dispatch_table(start_source, listing)
    }

    with pytest.raises(ValueError, match="selector inversion"):
        _witch_actions(
            start_source.replace("eori.w  #3,d2", "eori.w  #2,d2", 1),
            listing,
            constants,
            dispatch_indices,
        )
    with pytest.raises(ValueError, match="source order|call order"):
        _witch_actions(
            start_source.replace("bsr.w   SaveGame", "bsr.w   LoadGame", 1),
            listing,
            constants,
            dispatch_indices,
        )
    with pytest.raises(ValueError, match="navigation mask"):
        _witch_main_menu_facts(
            main_source.replace("andi.w  #3,d0", "andi.w  #2,d0", 1), listing, constants
        )


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("source_path", "before", "after"),
    [
        ("witchstart.asm", "bpl.s   @IsSaveSlot2Corrupted", "bne.s   @IsSaveSlot2Corrupted"),
        ("witchstart.asm", "moveq   #%110,d2", "moveq   #%101,d2"),
        (
            "witchstart.asm",
            "dc.w witchMenuAction_Load-rjt_WitchMenuActions",
            "dc.w witchMenuAction_Copy-rjt_WitchMenuActions",
        ),
        ("witchmainmenu.asm", "#BYTE_LOWER_NIBBLE_MASK,d0", "#BYTE_UPPER_NIBBLE_MASK,d0"),
        ("witchmainmenu.asm", "bne.w   loc_16756", "beq.w   loc_16756"),
        ("witchmainmenu.asm", "andi.w  #3,d0", "andi.w  #2,d0"),
        ("witchmainmenu.asm", "btst    #3,d6", "btst    #4,d6"),
        ("witchmainmenu.asm", "@Page3_Difficulties:", "@Page3_Mutated:"),
        ("witchstart.asm", "eori.w  #3,d2", "eori.w  #2,d2"),
        ("witchstart.asm", "setFlg  78", "setFlg  77"),
        ("witchstart.asm", "bsr.w   SaveGame", "bsr.w   LoadGame"),
        ("witchstart.asm", "#GAMESTART_SAVEPOINT_X,d1", "#GAMESTART_SAVEPOINT_Y,d1"),
        ("witchstart.asm", "subq.w  #1,d0", "subq.w  #2,d0"),
        ("witchstart.asm", "andi.w  #3,d2", "andi.w  #2,d2"),
        ("witchstart.asm", "beq.s   @loc_16", "bne.s   @loc_16"),
        ("witchstart.asm", "chkFlg  88", "chkFlg  87"),
        ("witchstart.asm", "move.b  (SAVE_FLAGS).l,d0", "move.b  (SAVE_FLAGS).l,d1"),
        (
            "witchstart.asm",
            "bne.w   byte_73C2       \n                move.b  (SAVE_FLAGS).l,d0",
            "beq.w   byte_73C2       \n                move.b  (SAVE_FLAGS).l,d0",
        ),
        ("witchstart.asm", "beq.s   @loc_19", "bne.s   @loc_19"),
        ("witchstart.asm", "bsr.w   ClearSaveSlotFlag", "bsr.w   OtherSaveService"),
    ],
)
def test_witch_combined_use_site_mutations_fail_before_fixture_comparison(
    source_path: str, before: str, after: str
) -> None:
    source_root = UPSTREAM / "disasm/code/specialscreens/witch"
    sources = {
        "code/specialscreens/witch/witchstart.asm": read_upstream_text(
            source_root / "witchstart.asm"
        ),
        "code/specialscreens/witch/witchmainmenu.asm": read_upstream_text(
            source_root / "witchmainmenu.asm"
        ),
        "code/specialscreens/witch/witchfunctions.asm": read_upstream_text(
            source_root / "witchfunctions.asm"
        ),
    }
    key = f"code/specialscreens/witch/{source_path}"
    sources[key] = sources[key].replace(
        before.replace("\n", "\r\n"), after.replace("\n", "\r\n"), 1
    )
    listing = (UPSTREAM / "build/sf2build-h1.lst").read_text(encoding="utf-8")
    with pytest.raises(ValueError, match="witch"):
        _witch_save_menu_facts_from_sources(UPSTREAM / "disasm", sources, listing)


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_witch_provenance_requires_exact_pinned_source_line_and_instruction() -> None:
    source_root = UPSTREAM / "disasm/code/specialscreens/witch"
    sources = {
        "code/specialscreens/witch/witchstart.asm": read_upstream_text(
            source_root / "witchstart.asm"
        ),
        "code/specialscreens/witch/witchmainmenu.asm": read_upstream_text(
            source_root / "witchmainmenu.asm"
        ),
        "code/specialscreens/witch/witchfunctions.asm": read_upstream_text(
            source_root / "witchfunctions.asm"
        ),
    }
    sources["code/specialscreens/witch/witchstart.asm"] = sources[
        "code/specialscreens/witch/witchstart.asm"
    ].replace("moveq   #1,d4", "moveq   #2,d4", 1)
    with pytest.raises(ValueError, match="provenance use-site drift: new.handoff.d4"):
        _witch_save_menu_provenance(sources)
