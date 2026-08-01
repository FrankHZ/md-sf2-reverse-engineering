from __future__ import annotations

from copy import deepcopy
from pathlib import Path

import pytest

import sf2tool.h3.map_script_screen_presentation as presentation
from sf2tool.h2.map_script_engine import build_map_script_engine_contract
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

ROM = repo_path("local/roms/sf2-us.bin")
UPSTREAM = repo_path("local/upstream/SF2DISASM")
FIXTURE = repo_path("tests/fixtures/h3/map-script-screen-presentation-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h3-map-script-screen-presentation-fixture.schema.json")
OBSERVATION_SCHEMA = repo_path("schemas/h3-map-script-screen-presentation-observation.schema.json")


def _static() -> dict:
    return presentation.build_map_script_screen_presentation_contract(ROM, UPSTREAM)


def test_complete_runtime_matrix_is_source_derived_and_exact() -> None:
    fixture = load_json(FIXTURE)
    static = _static()
    derived = presentation.derive_case_expectations(static, fixture)

    assert fixture["sourceContract"] == {
        "sourceSitesSha256": "EE24CB393511FD9640AC96E427815CBC1851B2A6384A9D045FE74CC7E28F0948",
        "programTotalsSha256": "DB8AFFDF9AE1FE4B119CF916EB1F9792A383F5BD7FE6B7F95B7FD7CBE8F3107F",
        "sourceSiteOrderKeyCount": 459,
        "programTotalOrderKeyCount": 304,
    }
    assert [row["id"] for row in derived] == [
        "quake-direct-source",
        "quake-4000-source",
        "quake-8000-source",
        "fade-in-source",
        "fade-out-source",
        "slow-fade-in-source",
        "slow-fade-out-controlled",
        "tint-source",
        "flicker-source",
        "fade-out-white-source",
        "fade-in-white-source",
        "flash-duration-2-source",
        "flash-duration-10-source",
        "flash-duration-20-source",
        "flash-duration-30-source",
        "flash-duration-40-source",
        "flash-duration-50-source",
        "flash-duration-60-source",
        "flash-duration-70-source",
        "flash-duration-90-source",
        "fade-in-half-source",
        "fade-out-half-source",
    ]
    assert [
        (
            row["macro"],
            row["handlerInputWord"],
            row["quakeAmplitudeWordWrites"],
            row["flashLoopIterationCount"],
        )
        for row in derived
    ] == [
        ("setQuake", 0, [0], None),
        ("setQuake", 16386, [1, 0], None),
        ("setQuake", 32770, [1, 2], None),
        ("fadeInB", None, None, None),
        ("fadeOutB", None, None, None),
        ("slowFadeInB", None, None, None),
        ("slowFadeOutB", None, None, None),
        ("tintMap", None, None, None),
        ("flickerOnce", None, None, None),
        ("mapFadeOutToWhite", None, None, None),
        ("mapFadeInFromWhite", None, None, None),
        ("flashScreenWhite", 2, None, 1),
        ("flashScreenWhite", 10, None, 2),
        ("flashScreenWhite", 20, None, 3),
        ("flashScreenWhite", 30, None, 4),
        ("flashScreenWhite", 40, None, 6),
        ("flashScreenWhite", 50, None, 7),
        ("flashScreenWhite", 60, None, 8),
        ("flashScreenWhite", 70, None, 9),
        ("flashScreenWhite", 90, None, 12),
        ("fadeInFromBlackHalf", None, None, None),
        ("fadeOutToBlackHalf", None, None, None),
    ]
    assert [len(row["directCallPlan"]) for row in derived] == [
        0,
        2,
        2,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        2,
        3,
        4,
        5,
        7,
        8,
        9,
        10,
        13,
        1,
        1,
    ]
    assert static["sourceFacts"]["callerBreakdown"]["effectiveTargetTotals"] == {
        "Sleep": 1,
        "FadeInFromBlack": 2,
        "FadeOutToBlack": 2,
        "LaunchFading": 7,
        "DuplicatePalettes": 1,
    }
    assert static["runtimeQuestions"] == presentation.RUNTIME_QUESTIONS


def test_h2_source_row_mutation_fails_before_h3_fixture_comparison() -> None:
    facts = build_map_script_engine_contract(ROM, UPSTREAM)["screenPresentationCommandFacts"]
    drifted = deepcopy(facts)
    for site in drifted["sourceSites"]:
        for command in site["commands"]:
            if command["sourceOrderKey"] == "bbcs_16:72:setQuake":
                command["operandValues"][0]["resolvedValue"] = 0x4003
                with pytest.raises(ValueError, match="source-site hash drift"):
                    presentation._source_inputs(drifted)
                return
    raise AssertionError("test source row is absent")


def test_handler_use_site_operand_mutation_fails_before_runtime_golden() -> None:
    facts = build_map_script_engine_contract(ROM, UPSTREAM)["screenPresentationCommandFacts"]
    handler = next(row for row in facts["handlers"] if row["macro"] == "flashScreenWhite")
    drifted = deepcopy(handler)
    site = next(
        row
        for row in drifted["sectionGuard"]["sourceImmediateUseSites"]
        if row["instruction"] == "lsr.w #3,d7"
    )
    site["instruction"] = "lsr.w #4,d7"
    with pytest.raises(ValueError, match="source use-site drift"):
        presentation._use_site_relations(drifted)


def test_instruction_parser_accepts_suffix_and_rejects_comments_and_near_misses() -> None:
    addresses = {target: index + 1 for index, target in enumerate(presentation.TARGET_IDENTITIES)}
    source = [{"instruction": "jsr (Sleep).w", "sourceLine": 1}]
    h1 = [{"instruction": "jsr (Sleep).w", "address": 100}, {"instruction": "rts", "address": 104}]
    assert presentation._calls(source, h1, addresses) == [
        {
            "instructionTarget": "Sleep",
            "effectiveTarget": "Sleep",
            "targetRole": "effective",
            "callSiteAddress": 100,
            "targetAddress": 1,
            "returnAddress": 104,
            "addressingForm": "direct",
        }
    ]
    assert (
        presentation._calls(
            [
                {"instruction": "label_Sleep:", "sourceLine": 1},
                {"instruction": "; jsr (Sleep).w", "sourceLine": 2},
                {"instruction": "move.w #Sleep,d0", "sourceLine": 3},
            ],
            [
                {"instruction": "label_Sleep:", "address": 1},
                {"instruction": "; jsr (Sleep).w", "address": 2},
                {"instruction": "move.w #Sleep,d0", "address": 3},
                {"instruction": "rts", "address": 5},
            ],
            addresses,
        )
        == []
    )


def test_fixture_and_observation_schemas_reject_nested_mutations_and_reorder() -> None:
    fixture = load_json(FIXTURE)
    validate_json(fixture, FIXTURE_SCHEMA, owner="screen-presentation fixture")
    for mutation in ("missing", "renamed", "extra", "reordered", "value"):
        drifted = deepcopy(fixture)
        if mutation == "missing":
            del drifted["instrumentation"]["fadingCounterSeed"]
        elif mutation == "renamed":
            drifted["instrumentation"]["counterSeed"] = drifted["instrumentation"].pop(
                "fadingCounterSeed"
            )
        elif mutation == "extra":
            drifted["instrumentation"]["unexpectedNestedField"] = 1
        elif mutation == "value":
            drifted["maxFrames"] += 1
        else:
            drifted["cases"][0], drifted["cases"][1] = drifted["cases"][1], drifted["cases"][0]
        with pytest.raises(ValueError):
            validate_json(drifted, FIXTURE_SCHEMA, owner=mutation)

    def observed_record(case_id: str) -> dict:
        return {
            "id": case_id,
            "handlerEntryPcObserved": 0,
            "handlerReturnPcObserved": 0,
            "handlerReturned": True,
            "scriptCursorRamOffsetAfterObserved": 4,
            "stackPointerDeltaBytesObserved": 0,
            "directCallsObserved": [],
            "effectiveTargetCountsObserved": {
                target: 0 for target in presentation.TARGET_IDENTITIES
            },
            "quakeAmplitudeWordWritesObserved": None,
            "fadingCounterByteReadsObserved": None,
            "fadingCounterByteWritesObserved": None,
            "flashDurationWordAfterShiftObserved": None,
            "flashLoopIterationCountObserved": None,
            "directCallRegisterWordsObserved": [],
        }
    order = [case["id"] for case in fixture["cases"]]
    observed = {
        "system": "GEN",
        "core": "Genesis Plus GX",
        "id": fixture["id"],
        "mapTest": 0,
        "recordOrder": order,
        "records": [observed_record(case_id) for case_id in order],
    }
    validate_json(observed, OBSERVATION_SCHEMA, owner="observation")
    swapped = deepcopy(observed)
    swapped["records"][0], swapped["records"][1] = swapped["records"][1], swapped["records"][0]
    with pytest.raises(ValueError):
        validate_json(swapped, OBSERVATION_SCHEMA, owner="observation swapped records")
    duplicated = deepcopy(observed)
    duplicated["records"][1] = deepcopy(duplicated["records"][0])
    with pytest.raises(ValueError):
        validate_json(duplicated, OBSERVATION_SCHEMA, owner="observation duplicate record")
    observed["records"][0]["unexpectedNestedField"] = 1
    with pytest.raises(ValueError):
        validate_json(observed, OBSERVATION_SCHEMA, owner="observation nested extra")


def test_session_service_shims_are_source_addressed_and_rom_preflighted(tmp_path: Path) -> None:
    static = _static()
    fixture = load_json(FIXTURE)
    patches = presentation._service_patches(static, ROM, fixture)
    assert [(row["targetIdentity"], row["address"]) for row in patches] == [
        ("Sleep", 3844),
        ("FadeInFromBlack", 3286),
        ("FadeOutToBlack", 3296),
        ("LaunchFading", 288676),
        ("DuplicatePalettes", 3170),
    ]
    drifted = tmp_path / "drifted.bin"
    data = bytearray(ROM.read_bytes())
    data[patches[0]["address"]] ^= 0xFF
    drifted.write_bytes(data)
    with pytest.raises(ValueError, match="preflight drift"):
        presentation._service_patches(static, drifted, fixture)
