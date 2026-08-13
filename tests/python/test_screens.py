from __future__ import annotations

import json
from copy import deepcopy
from pathlib import Path
from typing import Any

import pytest

from sf2tool.h2 import screens
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
OUTPUT_SCHEMA = repo_path("schemas/special-screens-static.schema.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-special-screens-static-fixture.schema.json")
FIXTURE_PATH = repo_path("tests/fixtures/h2/special-screens-static-v1.json")


def _write_json(path: Path, value: object) -> Path:
    path.write_bytes((json.dumps(value, indent=2) + "\n").encode("utf-8"))
    return path


def _write_fixture(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path, fixture: dict[str, Any]
) -> Path:
    path = _write_json(tmp_path / "special-screens-fixture.json", fixture)
    monkeypatch.setattr(screens, "FIXTURE", path)
    return path


def _write_fixture_schema(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path, schema: dict[str, Any]
) -> Path:
    path = _write_json(tmp_path / "special-screens-fixture.schema.json", schema)
    monkeypatch.setattr(screens, "FIXTURE_SCHEMA", path)
    return path


def _source_record(record_id: str, source_path: str) -> dict[str, Any]:
    index = load_json(repo_path("manifests/research-index.json"))
    record = deepcopy(
        next(item for item in index["records"] if item["id"] == "screens.witch.start")
    )
    record["id"] = record_id
    record["sourcePath"] = source_path
    return record


def _schema_consts(value: Any, trail: str = "$") -> dict[str, Any]:
    constants: dict[str, Any] = {}
    if isinstance(value, dict):
        if "const" in value:
            constants[trail] = value["const"]
        for key, child in value.items():
            constants.update(_schema_consts(child, f"{trail}.{key}"))
    elif isinstance(value, list):
        for index, child in enumerate(value):
            constants.update(_schema_consts(child, f"{trail}[{index}]"))
    return constants


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
    assert load_json(fixture_schema)["definitions"] == {
        name: definition
        for name, definition in load_json(output_schema)["definitions"].items()
        if name not in {"upstream", "summary", "directCall", "sourceFile"}
    }
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
    validate_json(malformed_output, output_schema, owner="reordered witch dispatch")

    malformed_output = deepcopy(output)
    malformed_output["screenFacts"]["witchSaveMenu"]["mainMenu"]["navigation"][
        "availableBitPositions"
    ][3] = 4
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
    validate_json(malformed_output, output_schema, owner="reordered witch provenance")

    malformed_output = deepcopy(output)
    malformed_output["screenFacts"]["witchSaveMenu"]["provenance"]["useSites"][0]["sourceLine"] = 0
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


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_screens_source_path_membership_keeps_twenty_two_records_over_nineteen_paths() -> None:
    fixture = load_json(FIXTURE_PATH)
    output = screens.build_special_screen_inventory(UPSTREAM)
    expected = fixture["expected"]

    for field in (
        "indexedRecordIds",
        "indexedSourcePaths",
        "indexedRecordsBySourcePath",
        "representativeSymbols",
    ):
        assert output[field] == expected[field]
    assert output["summary"]["indexedRecordCount"] == len(output["indexedRecordIds"]) == 22
    assert output["summary"]["indexedFileCount"] == len(output["indexedSourcePaths"]) == 19
    assert output["summary"]["fileCount"] == len(output["files"]) == 19
    relation = {row["sourcePath"]: row["recordIds"] for row in output["indexedRecordsBySourcePath"]}
    assert relation["code/specialscreens/title/graphics.asm"] == [
        "screens.title.compressed-tiles",
        "screens.title.resources",
    ]
    assert relation["code/specialscreens/witch/witchstart.asm"] == [
        "screens.witch.new-game-lifecycle",
        "screens.witch.save-menu-actions",
        "screens.witch.start",
    ]


def test_screens_schemas_keep_exact_corpora_in_fixture_and_verifier() -> None:
    output_schema = load_json(OUTPUT_SCHEMA)
    fixture_schema = load_json(FIXTURE_SCHEMA)
    fixture_expected = fixture_schema["definitions"]["expected"]["properties"]

    for field in (
        "indexedRecordIds",
        "indexedSourcePaths",
        "indexedRecordsBySourcePath",
        "representativeSymbols",
        "screenFacts",
        "resourceTargets",
        "runtimeQuestions",
    ):
        assert "const" not in output_schema["properties"][field]
        assert "const" not in fixture_expected[field]
    assert output_schema["properties"]["indexedRecordIds"].get("maxItems") != 21
    assert fixture_expected["indexedRecordIds"].get("maxItems") != 21
    assert fixture_schema["definitions"] == {
        name: definition
        for name, definition in output_schema["definitions"].items()
        if name not in {"upstream", "summary", "directCall", "sourceFile"}
    }

    for schema in (output_schema, fixture_schema):
        definitions = schema["definitions"]
        relation = definitions["indexedRecordsBySourcePath"]
        assert relation["additionalProperties"] is False
        assert relation["required"] == ["sourcePath", "recordIds"]
        assert relation["properties"]["recordIds"]["uniqueItems"] is True
        assert definitions["representativeSymbols"]["additionalProperties"] is False
        assert definitions["witchShape1"]["additionalProperties"] is False
    assert output_schema["definitions"]["sourceFile"]["additionalProperties"] is False

    assert _schema_consts(output_schema) == {
        "$.properties.schemaVersion": 1,
        "$.properties.id": "sf2-special-screens-static-v1",
        "$.properties.scope": "code/specialscreens",
        "$.definitions.summary.properties.fileCount": 19,
        "$.definitions.summary.properties.screenGroupCount": 7,
        "$.definitions.summary.properties.layoutIncludedFileCount": 19,
        "$.definitions.summary.properties.indexedFileCount": 19,
    }
    assert _schema_consts(fixture_schema) == {
        "$.properties.schemaVersion": 1,
        "$.properties.id": "sf2-special-screens-static-v1",
        "$.properties.romSha256": (
            "9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9"
        ),
        "$.properties.upstreamCommit": "c834c652b6862bc5679fd7f69a38a7093206efc6",
    }


@pytest.mark.parametrize(
    "mutation",
    [
        "missing-expected-field",
        "extra-expected-field",
        "record-ids-type",
        "missing-source-path",
        "renamed-source-path",
        "extra-relation-field",
        "source-path-type",
        "duplicate-same-row-record",
        "missing-relation-row",
        "extra-relation-row",
        "missing-representative-symbol",
        "extra-representative-symbol",
        "representative-symbol-type",
        "missing-screen-fact",
        "renamed-screen-fact",
        "extra-screen-fact",
        "wrong-screen-fact-type",
        "missing-witch-source-path",
        "extra-witch-source-field",
    ],
)
def test_screens_fixture_schema_rejects_recursive_shape_mutations(mutation: str) -> None:
    fixture = deepcopy(load_json(FIXTURE_PATH))
    expected = fixture["expected"]
    relation = expected["indexedRecordsBySourcePath"][0]
    symbols = expected["representativeSymbols"]
    if mutation == "missing-expected-field":
        del expected["indexedRecordIds"]
    elif mutation == "extra-expected-field":
        expected["unexpected"] = True
    elif mutation == "record-ids-type":
        expected["indexedRecordIds"] = "screens.endkiss.engine"
    elif mutation == "missing-source-path":
        del relation["sourcePath"]
    elif mutation == "renamed-source-path":
        relation["renamedSourcePath"] = relation.pop("sourcePath")
    elif mutation == "extra-relation-field":
        relation["unexpected"] = True
    elif mutation == "source-path-type":
        relation["sourcePath"] = 7
    elif mutation == "duplicate-same-row-record":
        relation["recordIds"] = ["screens.endkiss.engine", "screens.endkiss.engine"]
    elif mutation == "missing-relation-row":
        expected["indexedRecordsBySourcePath"].pop()
    elif mutation == "extra-relation-row":
        expected["indexedRecordsBySourcePath"].append(deepcopy(relation))
    elif mutation == "missing-representative-symbol":
        del symbols["code/specialscreens/title/title.asm"]
    elif mutation == "extra-representative-symbol":
        symbols["code/specialscreens/title/extra.asm"] = "ExtraSymbol"
    elif mutation == "representative-symbol-type":
        symbols["code/specialscreens/title/title.asm"] = 7
    elif mutation == "missing-screen-fact":
        del expected["screenFacts"]["witchSaveMenu"]
    elif mutation == "renamed-screen-fact":
        facts = expected["screenFacts"]["witchSaveMenu"]["mainMenu"]["cancel"]
        facts["renamedReturnValue"] = facts.pop("returnValue")
    elif mutation == "extra-screen-fact":
        expected["screenFacts"]["witchSaveMenu"]["mainMenu"]["cancel"]["extra"] = True
    elif mutation == "wrong-screen-fact-type":
        expected["screenFacts"]["witchSaveMenu"]["mainMenu"]["cancel"]["returnValue"] = "-1"
    elif mutation == "missing-witch-source-path":
        del expected["screenFacts"]["witchSaveMenu"]["sourcePaths"]["start"]
    elif mutation == "extra-witch-source-field":
        expected["screenFacts"]["witchSaveMenu"]["sourcePaths"]["extra"] = "path"
    else:
        raise AssertionError(f"unknown mutation: {mutation}")

    with pytest.raises(ValueError):
        validate_json(fixture, FIXTURE_SCHEMA, owner=f"special-screens fixture {mutation}")


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    "mutation",
    [
        "missing-membership-field",
        "extra-membership-field",
        "missing-relation-field",
        "extra-relation-field",
        "wrong-representative-type",
        "missing-file-field",
        "extra-direct-call-field",
        "wrong-file-path-type",
        "wrong-screen-fact-type",
    ],
)
def test_screens_output_schema_rejects_recursive_shape_mutations(mutation: str) -> None:
    output = screens.build_special_screen_inventory(UPSTREAM)
    if mutation == "missing-membership-field":
        del output["indexedRecordsBySourcePath"]
    elif mutation == "extra-membership-field":
        output["unexpected"] = True
    elif mutation == "missing-relation-field":
        del output["indexedRecordsBySourcePath"][0]["recordIds"]
    elif mutation == "extra-relation-field":
        output["indexedRecordsBySourcePath"][0]["unexpected"] = True
    elif mutation == "wrong-representative-type":
        output["representativeSymbols"]["code/specialscreens/title/title.asm"] = 7
    elif mutation == "missing-file-field":
        del output["files"][0]["sha256"]
    elif mutation == "extra-direct-call-field":
        output["files"][0]["directCalls"][0]["unexpected"] = True
    elif mutation == "wrong-file-path-type":
        output["files"][0]["path"] = 7
    elif mutation == "wrong-screen-fact-type":
        output["screenFacts"]["witchSaveMenu"]["mainMenu"]["cancel"]["returnValue"] = "-1"
    else:
        raise AssertionError(f"unknown mutation: {mutation}")

    with pytest.raises(ValueError):
        validate_json(output, OUTPUT_SCHEMA, owner=f"special-screens output {mutation}")


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("suffix", "mutate", "error"),
    [
        (
            "record-order",
            lambda output: output["indexedRecordIds"].reverse(),
            "indexedRecordIds relation drift",
        ),
        (
            "source-order",
            lambda output: output["indexedSourcePaths"].reverse(),
            "indexedSourcePaths relation order drift",
        ),
        (
            "relation-order",
            lambda output: output["indexedRecordsBySourcePath"].reverse(),
            "indexed relation source order drift",
        ),
        (
            "coherent-relation-source-order",
            lambda output: (
                output["indexedRecordsBySourcePath"].reverse(),
                output["indexedSourcePaths"].reverse(),
            ),
            "indexed relation source order drift",
        ),
        (
            "relation-record-order",
            lambda output: next(
                row
                for row in output["indexedRecordsBySourcePath"]
                if row["sourcePath"] == "code/specialscreens/title/graphics.asm"
            )["recordIds"].reverse(),
            "indexed relation record order drift",
        ),
        (
            "duplicate-relation-source",
            lambda output: output["indexedRecordsBySourcePath"][1].update(
                sourcePath=output["indexedRecordsBySourcePath"][0]["sourcePath"]
            ),
            "indexed relation duplicate source path",
        ),
        (
            "summary-record-count",
            lambda output: output["summary"].update(indexedRecordCount=20),
            "summary indexedRecordCount relation drift",
        ),
        (
            "source-inventory",
            lambda output: output["files"].__setitem__(1, deepcopy(output["files"][0])),
            "source inventory duplicate path",
        ),
        (
            "missing-record",
            lambda output: output["indexedRecordIds"].pop(),
            "indexedRecordIds relation drift",
        ),
    ],
)
def test_screens_verifier_rejects_schema_valid_relation_drift_before_write(
    monkeypatch: pytest.MonkeyPatch,
    tmp_path: Path,
    suffix: str,
    mutate: Any,
    error: str,
) -> None:
    output = screens.build_special_screen_inventory(UPSTREAM)
    mutate(output)
    validate_json(output, OUTPUT_SCHEMA, owner=f"schema-valid special-screens output {suffix}")
    destination = tmp_path / f"special-screens-{suffix}.json"
    monkeypatch.setattr(screens, "build_special_screen_inventory", lambda _: output)

    with pytest.raises(ValueError, match=error):
        screens.verify_special_screen_inventory(UPSTREAM, output_path=destination)
    assert not destination.exists()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("suffix", "mutate"),
    [
        (
            "dispatcher-order",
            lambda output: output["screenFacts"]["witchSaveMenu"]["dispatcher"].__setitem__(
                slice(1, 3),
                list(reversed(output["screenFacts"]["witchSaveMenu"]["dispatcher"][1:3])),
            ),
        ),
        (
            "navigation-boundary",
            lambda output: output["screenFacts"]["witchSaveMenu"]["mainMenu"]["navigation"][
                "availableBitPositions"
            ].__setitem__(3, 4),
        ),
        (
            "provenance-order",
            lambda output: output["screenFacts"]["witchSaveMenu"]["provenance"][
                "useSites"
            ].__setitem__(
                slice(0, 2),
                list(
                    reversed(output["screenFacts"]["witchSaveMenu"]["provenance"]["useSites"][:2])
                ),
            ),
        ),
    ],
)
def test_screens_verifier_rejects_schema_valid_exact_screen_facts_before_write(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path, suffix: str, mutate: Any
) -> None:
    output = screens.build_special_screen_inventory(UPSTREAM)
    mutate(output)
    validate_json(output, OUTPUT_SCHEMA, owner=f"schema-valid special-screens {suffix}")
    monkeypatch.setattr(screens, "build_special_screen_inventory", lambda _: output)
    destination = tmp_path / f"screen-facts-{suffix}.json"

    with pytest.raises(ValueError, match="screenFacts drift"):
        screens.verify_special_screen_inventory(UPSTREAM, output_path=destination)
    assert not destination.exists()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("row_index", "target_index", "error"),
    [
        (8, 8, "indexed relation duplicate record ID"),
        (8, 9, "indexed relation duplicate record ID"),
    ],
)
def test_screens_join_invariant_rejects_duplicate_records_before_fixture_comparison(
    row_index: int, target_index: int, error: str
) -> None:
    output = screens.build_special_screen_inventory(UPSTREAM)
    duplicate = output["indexedRecordsBySourcePath"][row_index]["recordIds"][0]
    output["indexedRecordsBySourcePath"][target_index]["recordIds"].append(duplicate)
    discovered_source_paths = [row["path"] for row in output["files"]]
    expected_index_membership = screens._index_records_for_source_root(set(discovered_source_paths))

    with pytest.raises(ValueError, match=error):
        screens._verify_indexed_record_join(
            output, expected_index_membership, discovered_source_paths
        )


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_screens_verifier_rejects_extra_coherent_membership_before_hash_or_write(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    output = screens.build_special_screen_inventory(UPSTREAM)
    extra_id = "screens.witch.extra-schema-valid"
    output["indexedRecordIds"].append(extra_id)
    output["indexedRecordIds"].sort()
    row = next(
        row
        for row in output["indexedRecordsBySourcePath"]
        if row["sourcePath"] == "code/specialscreens/witch/witchstart.asm"
    )
    row["recordIds"].append(extra_id)
    row["recordIds"].sort()
    output["summary"]["indexedRecordCount"] += 1
    validate_json(output, OUTPUT_SCHEMA, owner="schema-valid extra special-screens membership")
    destination = tmp_path / "extra-membership.json"
    monkeypatch.setattr(screens, "build_special_screen_inventory", lambda _: output)

    with pytest.raises(ValueError, match="indexedRecordIds source-membership drift"):
        screens.verify_special_screen_inventory(UPSTREAM, output_path=destination)
    assert not destination.exists()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_screens_verifier_rejects_coordinated_output_and_fixture_membership_drift_before_write(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    output = screens.build_special_screen_inventory(UPSTREAM)
    fixture = deepcopy(load_json(FIXTURE_PATH))
    wrong_record_id = "screens.witch.new-game-lifecyclx"
    record_index = output["indexedRecordIds"].index("screens.witch.new-game-lifecycle")
    output["indexedRecordIds"][record_index] = wrong_record_id
    output["indexedRecordsBySourcePath"][15]["recordIds"][0] = wrong_record_id
    fixture["expected"]["indexedRecordIds"][record_index] = wrong_record_id
    fixture["expected"]["indexedRecordsBySourcePath"][15]["recordIds"][0] = wrong_record_id
    validate_json(output, OUTPUT_SCHEMA, owner="schema-valid coordinated special-screens output")
    validate_json(fixture, FIXTURE_SCHEMA, owner="schema-valid coordinated special-screens fixture")
    _write_fixture(monkeypatch, tmp_path, fixture)
    monkeypatch.setattr(screens, "build_special_screen_inventory", lambda _: output)
    destination = tmp_path / "coordinated-membership-drift.json"

    # RESEARCH_INDEX remains the unmodified authoritative membership source.
    with pytest.raises(ValueError, match="indexedRecordIds source-membership drift"):
        screens.verify_special_screen_inventory(UPSTREAM, output_path=destination)
    assert not destination.exists()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("suffix", "mutate", "error"),
    [
        (
            "missing-record",
            lambda fixture: fixture["expected"]["indexedRecordIds"].pop(),
            "fixture indexedRecordIds drift",
        ),
        (
            "extra-record",
            lambda fixture: fixture["expected"]["indexedRecordIds"].append("screens.future-record"),
            "fixture indexedRecordIds drift",
        ),
        (
            "reordered-record",
            lambda fixture: fixture["expected"]["indexedRecordIds"].reverse(),
            "fixture indexedRecordIds drift",
        ),
        (
            "wrong-record",
            lambda fixture: fixture["expected"]["indexedRecordIds"].__setitem__(
                0, "screens.wrong-but-schema-valid"
            ),
            "fixture indexedRecordIds drift",
        ),
        (
            "reordered-source",
            lambda fixture: fixture["expected"]["indexedSourcePaths"].reverse(),
            "fixture indexedSourcePaths drift",
        ),
        (
            "wrong-source",
            lambda fixture: fixture["expected"]["indexedSourcePaths"].__setitem__(
                0, "code/specialscreens/endkiss/notdiscovered.asm"
            ),
            "fixture indexedSourcePaths drift",
        ),
        (
            "reordered-relation",
            lambda fixture: fixture["expected"]["indexedRecordsBySourcePath"].reverse(),
            "fixture indexedRecordsBySourcePath drift",
        ),
        (
            "wrong-relation-path",
            lambda fixture: fixture["expected"]["indexedRecordsBySourcePath"][0].update(
                sourcePath="code/specialscreens/endkiss/notdiscovered.asm"
            ),
            "fixture indexedRecordsBySourcePath drift",
        ),
        (
            "wrong-relation-record",
            lambda fixture: fixture["expected"]["indexedRecordsBySourcePath"][15].update(
                recordIds=["screens.witch.wrong-but-schema-valid"]
            ),
            "fixture indexedRecordsBySourcePath drift",
        ),
        (
            "screen-fact",
            lambda fixture: fixture["expected"]["screenFacts"].update(titleScrollLoopCount=3),
            "screenFacts drift",
        ),
        (
            "resource-target",
            lambda fixture: fixture["expected"]["resourceTargets"].update(
                tiles_Witch="data/graphics/specialscreens/witchscreen/other.bin"
            ),
            "resourceTargets drift",
        ),
        (
            "runtime-question",
            lambda fixture: fixture["expected"]["runtimeQuestions"].__setitem__(
                0, "wrong-but-schema-valid"
            ),
            "runtimeQuestions drift",
        ),
        (
            "h1-address",
            lambda fixture: fixture["function"].update(StartWitchScreen=29263),
            "H1 address drift",
        ),
        (
            "representative-symbol",
            lambda fixture: fixture["expected"]["representativeSymbols"].update(
                **{"code/specialscreens/title/title.asm": "EndTitleScreen"}
            ),
            "representative symbol drift",
        ),
    ],
)
def test_screens_verifier_rejects_fixture_corpus_and_h1_drift_before_write(
    monkeypatch: pytest.MonkeyPatch,
    tmp_path: Path,
    suffix: str,
    mutate: Any,
    error: str,
) -> None:
    fixture = deepcopy(load_json(FIXTURE_PATH))
    mutate(fixture)
    validate_json(fixture, FIXTURE_SCHEMA, owner=f"schema-valid special-screens fixture {suffix}")
    _write_fixture(monkeypatch, tmp_path, fixture)
    destination = tmp_path / f"fixture-{suffix}.json"

    with pytest.raises(ValueError, match=error):
        screens.verify_special_screen_inventory(UPSTREAM, output_path=destination)
    assert not destination.exists()


@pytest.mark.parametrize(
    ("field", "value"),
    [
        ("upstreamCommit", "0" * 40),
        ("romSha256", "0" * 64),
    ],
)
def test_screens_fixture_schema_preserves_pinned_provenance_constants(
    field: str, value: str
) -> None:
    fixture = deepcopy(load_json(FIXTURE_PATH))
    fixture[field] = value

    with pytest.raises(ValueError, match=field):
        validate_json(fixture, FIXTURE_SCHEMA, owner=f"special-screens provenance fixture {field}")


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("field", "value", "error"),
    [
        ("upstreamCommit", "0" * 40, "fixture upstream provenance drift"),
        ("romSha256", "0" * 64, "fixture ROM provenance drift"),
    ],
)
def test_screens_verifier_rejects_schema_valid_provenance_drift_before_write(
    monkeypatch: pytest.MonkeyPatch,
    tmp_path: Path,
    field: str,
    value: str,
    error: str,
) -> None:
    fixture = deepcopy(load_json(FIXTURE_PATH))
    fixture_schema = deepcopy(load_json(FIXTURE_SCHEMA))
    fixture[field] = value
    fixture_schema["properties"][field]["const"] = value
    fixture_schema_path = _write_fixture_schema(monkeypatch, tmp_path, fixture_schema)
    validate_json(fixture, fixture_schema_path, owner=f"schema-valid provenance fixture {field}")
    _write_fixture(monkeypatch, tmp_path, fixture)
    destination = tmp_path / f"provenance-{field}.json"

    with pytest.raises(ValueError, match=error):
        screens.verify_special_screen_inventory(UPSTREAM, output_path=destination)
    assert not destination.exists()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_screens_verifier_rejects_schema_valid_output_repository_drift_before_write(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    output = screens.build_special_screen_inventory(UPSTREAM)
    output["upstream"]["repository"] = "https://example.invalid/SF2DISASM.git"
    validate_json(output, OUTPUT_SCHEMA, owner="schema-valid special-screens repository drift")
    monkeypatch.setattr(screens, "build_special_screen_inventory", lambda _: output)
    destination = tmp_path / "repository-drift.json"

    with pytest.raises(ValueError, match="output upstream provenance drift"):
        screens.verify_special_screen_inventory(UPSTREAM, output_path=destination)
    assert not destination.exists()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_screens_verifier_rejects_representative_source_model_drift_before_write(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    output = screens.build_special_screen_inventory(UPSTREAM)
    fixture = deepcopy(load_json(FIXTURE_PATH))
    source_path = "code/specialscreens/title/title.asm"
    fixture["expected"]["representativeSymbols"][source_path] = "NotASourceSymbol"
    output["representativeSymbols"][source_path] = "NotASourceSymbol"
    validate_json(output, OUTPUT_SCHEMA, owner="schema-valid special-screens source-model output")
    validate_json(
        fixture, FIXTURE_SCHEMA, owner="schema-valid special-screens source-model fixture"
    )
    _write_fixture(monkeypatch, tmp_path, fixture)
    monkeypatch.setattr(screens, "build_special_screen_inventory", lambda _: output)
    destination = tmp_path / "representative-source-model-drift.json"

    with pytest.raises(ValueError, match="representative source model drift"):
        screens.verify_special_screen_inventory(UPSTREAM, output_path=destination)
    assert not destination.exists()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_screens_source_path_membership_accepts_another_owned_record_before_fixture_check(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    index = deepcopy(load_json(repo_path("manifests/research-index.json")))
    record_id = "independent.owner.nested-special-screen-record"
    index["records"].append(_source_record(record_id, "code/specialscreens/witch/witchstart.asm"))
    index_path = _write_json(tmp_path / "nested-special-screens-index.json", index)
    monkeypatch.setattr(screens, "RESEARCH_INDEX", index_path)

    output = screens.build_special_screen_inventory(UPSTREAM)
    assert record_id in output["indexedRecordIds"]
    assert output["summary"]["indexedRecordCount"] == 23
    relation = next(
        row
        for row in output["indexedRecordsBySourcePath"]
        if row["sourcePath"] == "code/specialscreens/witch/witchstart.asm"
    )
    assert record_id in relation["recordIds"]
    destination = tmp_path / "new-member-output.json"
    with pytest.raises(ValueError, match="fixture indexedRecordIds drift"):
        screens.verify_special_screen_inventory(UPSTREAM, output_path=destination)
    assert not destination.exists()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_screens_source_membership_ignores_metadata_but_excludes_outside_root(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    index = deepcopy(load_json(repo_path("manifests/research-index.json")))
    metadata_neutral_id = "independent.owner.metadata-neutral-special-screen-record"
    metadata_neutral = _source_record(
        metadata_neutral_id, "code/specialscreens/witch/witchstart.asm"
    )
    metadata_neutral["subsystem"] = "unrelated.subsystem"
    metadata_neutral["status"] = "inferred"
    metadata_neutral["documents"] = ["docs/research/unrelated.md"]
    metadata_neutral["evidence"] = []
    index["records"].append(metadata_neutral)
    index["records"].append(
        _source_record("independent.owner.outside-special-screens", "code/common/stats/flags.asm")
    )
    index_path = _write_json(tmp_path / "metadata-special-screens-index.json", index)
    monkeypatch.setattr(screens, "RESEARCH_INDEX", index_path)

    output = screens.build_special_screen_inventory(UPSTREAM)
    assert metadata_neutral_id in output["indexedRecordIds"]
    assert "independent.owner.outside-special-screens" not in output["indexedRecordIds"]
    assert output["summary"]["indexedRecordCount"] == 23


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_screens_rejects_under_root_record_missing_from_discovered_inventory(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    index = deepcopy(load_json(repo_path("manifests/research-index.json")))
    index["records"].append(
        _source_record(
            "independent.owner.missing-special-screens-file",
            "code/specialscreens/witch/not-discovered.asm",
        )
    )
    index_path = _write_json(tmp_path / "missing-special-screens-index.json", index)
    monkeypatch.setattr(screens, "RESEARCH_INDEX", index_path)

    with pytest.raises(ValueError, match="absent from the discovered root inventory"):
        screens.build_special_screen_inventory(UPSTREAM)


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_screens_build_does_not_read_golden_fixture(monkeypatch: pytest.MonkeyPatch) -> None:
    monkeypatch.setattr(screens, "FIXTURE", Path("does-not-exist.json"))
    output = screens.build_special_screen_inventory(UPSTREAM)
    assert output["summary"]["indexedRecordCount"] == 22
    assert output["indexedRecordsBySourcePath"][15]["recordIds"] == [
        "screens.witch.new-game-lifecycle",
        "screens.witch.save-menu-actions",
        "screens.witch.start",
    ]


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("field", "value", "error"),
    [
        ("summary", {"indexedRecordCount": 20}, "summary drift"),
        ("outputSha256", "0" * 64, "canonical hash drift"),
    ],
)
def test_screens_verifier_rejects_manifest_drift_before_write(
    monkeypatch: pytest.MonkeyPatch,
    tmp_path: Path,
    field: str,
    value: Any,
    error: str,
) -> None:
    manifest = deepcopy(load_json(repo_path("manifests/extractions/special-screens-static.json")))
    if field == "summary":
        manifest["summary"] = value
    else:
        manifest[field] = value
    manifest_path = _write_json(tmp_path / "special-screens-manifest.json", manifest)
    monkeypatch.setattr(screens, "MANIFEST", manifest_path)
    destination = tmp_path / f"manifest-{field}.json"

    with pytest.raises(ValueError, match=error):
        screens.verify_special_screen_inventory(UPSTREAM, output_path=destination)
    assert not destination.exists()
