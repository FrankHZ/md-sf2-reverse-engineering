from __future__ import annotations

import hashlib
from copy import deepcopy
from pathlib import Path

import pytest

import sf2tool.h3.map_script_dialogue as dialogue
from sf2tool.cli import build_parser
from sf2tool.h2.map_script_engine import build_map_script_engine_contract
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

ROM = repo_path("local/roms/sf2-us.bin")
UPSTREAM = repo_path("local/upstream/SF2DISASM")


def _static() -> dict:
    return dialogue.build_map_script_dialogue_contract(ROM, UPSTREAM)


def _expected_observation(static: dict, fixture: dict) -> dict:
    rows = dialogue.derive_case_expectations(static, fixture)
    return {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["id"],
        "mapTest": fixture["mapTestIndex"],
        "recordOrder": [case["id"] for case in fixture["cases"]],
        "records": rows,
    }


def _assert_closed_objects(value: object) -> None:
    if isinstance(value, dict):
        if value.get("type") == "object":
            assert value.get("additionalProperties") is False
        for child in value.values():
            _assert_closed_objects(child)
    elif isinstance(value, list):
        for child in value:
            _assert_closed_objects(child)


def test_dialogue_cli_owns_the_narrow_runtime_rail() -> None:
    args = build_parser().parse_args(["h3", "map-script-dialogue"])
    assert args.h3_command == "map-script-dialogue"
    assert args.timeout_seconds == 180


def test_dialogue_static_contract_and_complete_runtime_matrix_are_exact() -> None:
    fixture = load_json(dialogue.FIXTURE)
    static = _static()
    derived = dialogue.derive_case_expectations(static, fixture)

    assert static["sourceFacts"]["sourceContract"] == {
        "sourceSiteReferenceCount": 205,
        "programTotalCount": 304,
        "sourceInputSummary": {
            "count": 2883,
            "sha256": "6243D978C8BAD9960E5E3B1972DC12AF63C02B4DFE3DBD09AC5EDFF14FD826B5",
        },
    }
    assert (
        hashlib.sha256(dialogue._canonical_bytes(static["sourceFacts"]["handlers"]))
        .hexdigest()
        .upper()
        == "E883CD33A1E167ED9BFBC3BEC2BC950F3DB7E6843415D1CF0D48EA9B1BFB5D17"
    )
    assert (
        hashlib.sha256(dialogue._canonical_bytes(static["sourceFacts"]["callerBreakdown"]))
        .hexdigest()
        .upper()
        == "A70C2FEA7CE5084C87991358A6C8A257A9B198D85DD4D619BC886B5C3A89CEA3"
    )
    assert static["constants"] == {
        "sentinelWord": 65535,
        "modifierByteValues": [0, 128, 192, 255],
        "textCursorValueBounds": {
            "minimum": 240,
            "maximum": 4233,
            "domainMinimum": 0,
            "domainMaximum": 4266,
        },
    }
    assert static["runtimeQuestions"] == dialogue.RUNTIME_QUESTIONS
    assert [
        (row["macro"], row["handlerAddress"], row["returnAddress"])
        for row in static["sourceFacts"]["handlers"]
    ] == [
        ("nextSingleText", 291402, 291484),
        ("nextSingleTextVar", 291486, 291564),
        ("nextText", 291566, 291632),
        ("nextTextVar", 291634, 291696),
        ("textCursor", 291698, 291702),
        ("hideText", 291838, 291848),
    ]
    assert [row["id"] for row in derived] == [case["id"] for case in fixture["cases"]]
    assert (
        hashlib.sha256(dialogue._canonical_bytes({"cases": derived})).hexdigest().upper()
        == fixture["caseSemanticsSha256"]
    )
    assert _expected_observation(static, fixture)["records"] == derived


def test_transient_source_join_and_register_use_sites_fail_before_golden_comparison() -> None:
    static = _static()
    fixture = load_json(dialogue.FIXTURE)
    h2 = build_map_script_engine_contract(ROM, UPSTREAM)
    facts = deepcopy(h2["dialogueCommandFacts"])
    facts["sourceInputSummary"]["count"] -= 1
    with pytest.raises(ValueError, match="compact/transient source summary drift"):
        dialogue._full_source_rows(
            facts,
            facts,
            h2["programCorpus"],
            dialogue._parse_source_equates(UPSTREAM),
        )

    handler_row = next(
        row for row in dialogue._h2_dialogue_handlers(facts) if row["macro"] == "nextSingleText"
    )
    handler_source_path = UPSTREAM / "disasm/code/common/scripting/map/mapscriptengine_2.asm"
    handler_source = handler_source_path.read_text(encoding="utf-8")
    drifted_handler_source = handler_source.replace("move.w  (a6),d0", "move.w  (a6),d1", 1)
    assert drifted_handler_source != handler_source
    with pytest.raises(ValueError, match="guarded source section drift"):
        dialogue._handler_record(
            handler_row,
            drifted_handler_source,
            (UPSTREAM / dialogue.H1_LISTING_PATH).read_text(encoding="utf-8"),
            dialogue.listing_symbol_addresses(
                (UPSTREAM / dialogue.H1_LISTING_PATH).read_text(encoding="utf-8")
            ),
            operand_bytes=next(
                row["operandBytes"] for row in facts["macros"] if row["name"] == "nextSingleText"
            ),
        )

    handler = deepcopy(dialogue._handler(static, "nextSingleText"))
    get_entity = next(
        row
        for row in handler["directCallPlan"]
        if row["instructionTarget"] == "GetEntityPortaitAndSpeechSfx"
    )
    get_entity["d0SourceInstruction"] = "moveq #9,d0"
    with pytest.raises(ValueError, match="d0 call-source drift"):
        dialogue._register_words_for_calls(handler["directCallPlan"], [2], fixture)

    trampoline = deepcopy(fixture)
    trampoline["instrumentation"]["registerSeeds"]["d1"] = 3
    with pytest.raises(ValueError, match="register-seed trampoline drift"):
        dialogue._session_register_seeds(trampoline)


def test_direct_call_parser_accepts_suffixes_and_rejects_non_instruction_text() -> None:
    assert dialogue._direct_call_instruction("bsr.w GetEntityPortaitAndSpeechSfx") == (
        "bsr",
        "GetEntityPortaitAndSpeechSfx",
    )
    assert dialogue._direct_call_instruction("jsr (DisplayText).l") == ("jsr", "DisplayText")
    assert dialogue._direct_call_instruction("jsr j_ClosePortraitWindow") == (
        "jsr",
        "j_ClosePortraitWindow",
    )
    for text in (
        "label_DisplayText:",
        "; jsr (DisplayText).l",
        "move.w #DisplayText,d0",
        "jsr (DisplayText).q",
        "jsr (DisplayText).l ; comment",
    ):
        assert dialogue._direct_call_instruction(text) is None


def test_observed_pc_identities_are_validated_before_golden_comparison() -> None:
    static = _static()
    fixture = load_json(dialogue.FIXTURE)
    derived = dialogue.derive_case_expectations(static, fixture)
    observed = _expected_observation(static, fixture)
    dialogue._validate_observed_identities(observed, derived, static)

    handler_pc = deepcopy(observed)
    handler_pc["records"][0]["handlerEntryPcObserved"] += 1
    with pytest.raises(ValueError, match="handler-entry PC drift"):
        dialogue._validate_observed_identities(handler_pc, derived, static)

    call_site_pc = deepcopy(observed)
    call_site_pc["records"][0]["directCallsObserved"][0]["callSiteAddressObserved"] += 1
    with pytest.raises(ValueError, match="call-site PC drift"):
        dialogue._validate_observed_identities(call_site_pc, derived, static)

    target_entry_pc = deepcopy(observed)
    target_entry_pc["records"][0]["directCallsObserved"][0]["targetEntryAddressObserved"] += 1
    with pytest.raises(ValueError, match="target-entry PC drift"):
        dialogue._validate_observed_identities(target_entry_pc, derived, static)

    observer_source = dialogue.OBSERVER.read_text(encoding="utf-8")
    assert "handlerEntryPcObserved=handler_entry_pc" in observer_source
    assert "instructionTargetObserved=target.instructionTarget" in observer_source
    assert "effectiveTargetObserved=target.effectiveTarget" in observer_source
    assert "instructionTargetObserved=call.instructionTarget," not in observer_source


def test_fixture_and_observation_schemas_reject_nested_mutations_and_boundaries() -> None:
    fixture = load_json(dialogue.FIXTURE)
    validate_json(fixture, dialogue.FIXTURE_SCHEMA, owner="dialogue fixture")
    _assert_closed_objects(load_json(dialogue.FIXTURE_SCHEMA))
    for mutation in ("missing", "renamed", "extra", "reordered", "duplicate", "boundary"):
        drifted = deepcopy(fixture)
        if mutation == "missing":
            del drifted["instrumentation"]["registerSeeds"]["d2"]
        elif mutation == "renamed":
            seeds = drifted["instrumentation"]["registerSeeds"]
            seeds["d2Seed"] = seeds.pop("d2")
        elif mutation == "extra":
            drifted["instrumentation"]["registerSeeds"]["unexpected"] = 1
        elif mutation == "reordered":
            drifted["cases"][0], drifted["cases"][1] = drifted["cases"][1], drifted["cases"][0]
        elif mutation == "duplicate":
            drifted["cases"][1] = deepcopy(drifted["cases"][0])
        else:
            drifted["constants"]["textCursorValueBounds"]["maximum"] += 1
        with pytest.raises(ValueError):
            validate_json(drifted, dialogue.FIXTURE_SCHEMA, owner=mutation)

    observed = _expected_observation(_static(), fixture)
    validate_json(observed, dialogue.OBSERVATION_SCHEMA, owner="dialogue observation")
    _assert_closed_objects(load_json(dialogue.OBSERVATION_SCHEMA))
    for mutation in ("missing", "renamed", "extra", "reordered", "duplicate", "boundary"):
        drifted = deepcopy(observed)
        if mutation == "missing":
            del drifted["records"][0]["stateWritesObserved"][0]["wordValueObserved"]
        elif mutation == "renamed":
            state = drifted["records"][0]["stateWritesObserved"][0]
            state["wordValue"] = state.pop("wordValueObserved")
        elif mutation == "extra":
            drifted["records"][0]["directCallsObserved"][0]["unexpected"] = 1
        elif mutation == "reordered":
            drifted["records"][0], drifted["records"][1] = (
                drifted["records"][1],
                drifted["records"][0],
            )
        elif mutation == "duplicate":
            drifted["records"][1] = deepcopy(drifted["records"][0])
        else:
            drifted["records"][0]["directCallRegisterWordsObserved"][0][2] = 65536
        with pytest.raises(ValueError):
            validate_json(drifted, dialogue.OBSERVATION_SCHEMA, owner=mutation)


def test_session_service_shims_are_source_addressed_and_rom_preflighted(tmp_path: Path) -> None:
    static = _static()
    fixture = load_json(dialogue.FIXTURE)
    patches = dialogue._service_patches(static, ROM, fixture)
    assert [(row["instructionTarget"], row["address"]) for row in patches] == [
        ("csc1D_showPortrait", 289432),
        ("GetEntityPortaitAndSpeechSfx", 284216),
        ("WaitForViewScrollEnd", 18184),
        ("DisplayText", 25184),
        ("j_ClosePortraitWindow", 65596),
        ("Sleep", 3844),
    ]
    drifted = tmp_path / "drifted.bin"
    data = bytearray(ROM.read_bytes())
    data[patches[1]["address"]] ^= 0xFF
    drifted.write_bytes(data)
    with pytest.raises(ValueError, match="ROM preflight drift"):
        dialogue._service_patches(static, drifted, fixture)
