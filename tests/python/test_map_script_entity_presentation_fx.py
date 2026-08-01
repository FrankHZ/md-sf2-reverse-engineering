from __future__ import annotations

from copy import deepcopy
from pathlib import Path

import pytest

from sf2tool.h2.map_script_engine import build_map_script_engine_contract
from sf2tool.h3 import map_script_entity_presentation_fx as fx
from sf2tool.h3.bizhawk import bizhawk_contract, validate_lua_syntax
from sf2tool.jsonio import load_json, validate_json

ROM = Path("local/roms/sf2-us.bin")
UPSTREAM = Path("local/upstream/SF2DISASM")


def _static() -> dict:
    return fx.build_map_script_entity_presentation_fx_contract(ROM, UPSTREAM)


def _fixture() -> dict:
    return load_json(fx.FIXTURE)


def _observation_from_fixture(fixture: dict) -> dict:
    return {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["id"],
        "mapTest": fixture["mapTestIndex"],
        "recordOrder": [case["id"] for case in fixture["cases"]],
        "records": [{**case["expected"], **case["runtimeGolden"]} for case in fixture["cases"]],
    }


def test_complete_runtime_contract_is_source_derived_and_exact() -> None:
    fixture = _fixture()
    static = _static()
    assert fixture["provenance"] == static["provenance"]
    assert fixture["function"] == static["function"]
    assert fixture["ram"] == static["ram"]
    assert fixture["constants"] == static["constants"]
    assert fixture["sourceContract"] == static["sourceContract"]
    assert fixture["runtimeQuestions"] == fx.RUNTIME_QUESTIONS
    assert static["sourceContract"]["sourceObservedTransitionSelectorValues"] == [2, 3, 4, 5, 6, 7]
    assert static["sourceContract"]["sourceObservedFlashDurationValues"] == [
        10,
        20,
        30,
        40,
        50,
        57,
        60,
        70,
        80,
        100,
        120,
        180,
    ]
    assert static["sourceContract"]["sourceSpecialTransitionBranches"] == [
        {"selectorValue": 6, "d1WordValue": 0},
        {"selectorValue": 7, "d1WordValue": 65535},
    ]
    assert [case["id"] for case in fixture["cases"]] == [
        "transition-source-2",
        "transition-source-3",
        "transition-source-4",
        "transition-source-5",
        "transition-source-6",
        "transition-source-7",
        "headshake-source-min-entity",
        "flash-duration-10",
        "flash-duration-57",
        "flash-duration-180",
    ]
    derived = fx.derive_case_expectations(static, fixture)
    assert derived == [case["expected"] for case in fixture["cases"]]
    assert [case["loopIterationCount"] for case in derived] == [
        23,
        23,
        23,
        23,
        16,
        16,
        7,
        3,
        15,
        46,
    ]
    assert [
        len(fx._expand_callback_segments(case["callbackPlanSegments"])) for case in derived
    ] == [
        116,
        116,
        119,
        116,
        96,
        96,
        99,
        19,
        91,
        277,
    ]
    assert (
        sum(
            segment["repeatCountObserved"] * len(segment["callbackSitesObserved"])
            for case in fixture["cases"]
            for segment in case["runtimeGolden"]["callbackPlanSegmentsObserved"]
        )
        == 1145
    )
    assert fixture["cases"][4]["runtimeGolden"]["specialTransitionD1WordAtBitTestObserved"] == 0
    assert fixture["cases"][5]["runtimeGolden"]["specialTransitionD1WordAtBitTestObserved"] == 65535
    assert fixture["cases"][7]["runtimeGolden"]["flagsBByteAfterSetWriteObserved"] == 165
    assert fixture["cases"][7]["runtimeGolden"]["flagsBByteAfterClearWriteObserved"] == 161
    assert fixture["cases"][6]["runtimeGolden"]["animCounterByteAfterInitialWriteObserved"] == 255
    assert fixture["cases"][6]["runtimeGolden"]["animCounterByteAfterFinalWriteObserved"] == 0


def test_observed_callback_compaction_derives_repeat_counts_and_order() -> None:
    fixture = _fixture()
    static = _static()
    derived = fx.derive_case_expectations(static, fixture)
    observed_dispatches = []
    for case in fixture["cases"]:
        for segment in case["runtimeGolden"]["callbackPlanSegmentsObserved"]:
            for _ in range(segment["repeatCountObserved"]):
                observed_dispatches.extend(deepcopy(segment["callbackSitesObserved"]))
    assert len(observed_dispatches) == 1145

    offset = 0
    for expected_case, fixture_case in zip(derived, fixture["cases"], strict=True):
        event_count = len(fx._expand_callback_segments(expected_case["callbackPlanSegments"]))
        case_dispatches = observed_dispatches[offset : offset + event_count]
        assert fx._compact_observed_callback_dispatches(
            case_dispatches,
            expected_case["callbackPlanSegments"],
            expected_event_count=event_count,
        ) == fixture_case["runtimeGolden"]["callbackPlanSegmentsObserved"]
        offset += event_count
    assert offset == len(observed_dispatches)

    first_case_dispatches = observed_dispatches[:116]
    mutated_pattern_counts = deepcopy(derived[0]["callbackPlanSegments"])
    mutated_pattern_counts[0]["repeatCount"] = 99
    mutated_pattern_counts[1]["repeatCount"] = 1
    assert fx._compact_observed_callback_dispatches(
        first_case_dispatches,
        mutated_pattern_counts,
        expected_event_count=116,
    ) == fixture["cases"][0]["runtimeGolden"]["callbackPlanSegmentsObserved"]
    observer = fx.OBSERVER.read_text(encoding="utf-8")
    assert "segment.repeatCount" not in observer
    assert "repeat_count=repeat_count+1" in observer

    missing_dispatch = first_case_dispatches[:-1]
    with pytest.raises(ValueError, match="event-count"):
        fx._compact_observed_callback_dispatches(
            missing_dispatch,
            derived[0]["callbackPlanSegments"],
            expected_event_count=116,
        )

    reordered_dispatches = deepcopy(first_case_dispatches)
    reordered_dispatches[0], reordered_dispatches[1] = (
        reordered_dispatches[1],
        reordered_dispatches[0],
    )
    with pytest.raises(ValueError, match="identity/order"):
        fx._compact_observed_callback_dispatches(
            reordered_dispatches,
            derived[0]["callbackPlanSegments"],
            expected_event_count=116,
        )


def test_research_index_headshake_return_has_its_own_address_binding() -> None:
    index = load_json(Path("manifests/research-index.json"))
    record = next(
        row for row in index["records"] if row["id"] == "map.entity-presentation-fx.headshake"
    )
    assert [(row["id"], row["value"]) for row in record["addresses"]] == [
        ("entry", 289972),
        ("operand-after", 289974),
        ("loop", 289992),
        ("initial-anim-after", 289984),
        ("final-anim-after", 290062),
        ("return", 290062),
        ("entity-data", 16754946),
    ]
    bindings = record["evidence"][0]["bindings"]
    assert [(row["addressId"], row["fixtureField"]) for row in bindings] == [
        ("entry", "function.csc27_entityShakeHeadAddress"),
        ("operand-after", "function.csc27FirstOperandReadAfterAddress"),
        ("loop", "function.csc27LoopAddress"),
        ("initial-anim-after", "function.csc27InitialAnimAfterWriteAddress"),
        ("final-anim-after", "function.csc27FinalAnimAfterWriteAddress"),
        ("entity-data", "ram.entityDataAddress"),
        ("return", "function.csc27ReturnAddress"),
    ]
    assert len({row["id"] for row in record["addresses"]}) == len(record["addresses"])
    assert len({row["addressId"] for row in bindings}) == len(bindings)


def test_fixture_and_observation_schemas_are_recursively_closed_and_exact() -> None:
    fixture = _fixture()
    observation = _observation_from_fixture(fixture)
    validate_json(fixture, fx.FIXTURE_SCHEMA, owner="entity-presentation FX fixture")
    validate_json(observation, fx.OBSERVATION_SCHEMA, owner="entity-presentation FX observation")

    missing = deepcopy(fixture)
    del missing["cases"][0]["sourceInput"]["operandValues"][0]["rawValue"]
    with pytest.raises(ValueError):
        validate_json(
            missing, fx.FIXTURE_SCHEMA, owner="entity-presentation FX missing nested field"
        )
    renamed = deepcopy(fixture)
    renamed["cases"][0]["sourceInput"]["operandValues"][0]["renamedRawValue"] = renamed["cases"][0][
        "sourceInput"
    ]["operandValues"][0].pop("rawValue")
    with pytest.raises(ValueError):
        validate_json(
            renamed, fx.FIXTURE_SCHEMA, owner="entity-presentation FX renamed nested field"
        )
    extra = deepcopy(fixture)
    extra["cases"][0]["sourceInput"]["operandValues"][0]["unexpected"] = True
    with pytest.raises(ValueError):
        validate_json(extra, fx.FIXTURE_SCHEMA, owner="entity-presentation FX extra nested field")
    missing_transition_label = deepcopy(fixture)
    del missing_transition_label["constants"]["entityTransitionValuesBySourceLabel"][
        "ENTITY_TRANSITION_MOSAIC_OUT"
    ]
    with pytest.raises(ValueError):
        validate_json(
            missing_transition_label,
            fx.FIXTURE_SCHEMA,
            owner="entity-presentation FX missing nested source label",
        )
    extra_transition_label = deepcopy(fixture)
    extra_transition_label["constants"]["entityTransitionValuesBySourceLabel"]["UNEXPECTED"] = 8
    with pytest.raises(ValueError):
        validate_json(
            extra_transition_label,
            fx.FIXTURE_SCHEMA,
            owner="entity-presentation FX extra nested source label",
        )
    reordered = deepcopy(fixture)
    (
        reordered["sourceContract"]["sourceSpecialTransitionBranches"][0],
        reordered["sourceContract"]["sourceSpecialTransitionBranches"][1],
    ) = (
        reordered["sourceContract"]["sourceSpecialTransitionBranches"][1],
        reordered["sourceContract"]["sourceSpecialTransitionBranches"][0],
    )
    with pytest.raises(ValueError):
        validate_json(reordered, fx.FIXTURE_SCHEMA, owner="entity-presentation FX reordered corpus")
    out_of_bounds = deepcopy(fixture)
    out_of_bounds["cases"][0]["sourceInput"]["handlerInputWords"][0] = 65536
    with pytest.raises(ValueError):
        validate_json(
            out_of_bounds, fx.FIXTURE_SCHEMA, owner="entity-presentation FX word boundary"
        )

    observation_missing = deepcopy(observation)
    del observation_missing["records"][0]["callbackPlanSegmentsObserved"][0][
        "callbackSitesObserved"
    ][0]["returnAddressObserved"]
    with pytest.raises(ValueError):
        validate_json(
            observation_missing,
            fx.OBSERVATION_SCHEMA,
            owner="entity-presentation FX missing observed callback",
        )
    observation_extra = deepcopy(observation)
    observation_extra["records"][0]["callbackPlanSegmentsObserved"][0]["callbackSitesObserved"][0][
        "unexpected"
    ] = True
    with pytest.raises(ValueError):
        validate_json(
            observation_extra,
            fx.OBSERVATION_SCHEMA,
            owner="entity-presentation FX extra observed callback",
        )
    observation_reordered = deepcopy(observation)
    observation_reordered["recordOrder"][0], observation_reordered["recordOrder"][1] = (
        observation_reordered["recordOrder"][1],
        observation_reordered["recordOrder"][0],
    )
    with pytest.raises(ValueError):
        validate_json(
            observation_reordered,
            fx.OBSERVATION_SCHEMA,
            owner="entity-presentation FX reordered observation",
        )
    observation_boundary = deepcopy(observation)
    observation_boundary["records"][9]["loopIterationCountObserved"] = 47
    with pytest.raises(ValueError):
        validate_json(
            observation_boundary,
            fx.OBSERVATION_SCHEMA,
            owner="entity-presentation FX loop boundary",
        )


def test_h2_guard_and_table_mutations_fail_before_runtime_golden(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    original = build_map_script_engine_contract(ROM, UPSTREAM)
    mutated = deepcopy(original)
    mutated["entityPresentationFxCommandFacts"]["handlers"][0]["sectionGuard"][
        "orderedInstructions"
    ][11] = "lsl.w #4,d0"
    monkeypatch.setattr(fx, "build_map_script_engine_contract", lambda *_: mutated)
    with pytest.raises(ValueError, match="H2 named-section guard drift"):
        fx.build_map_script_entity_presentation_fx_contract(ROM, UPSTREAM)
    monkeypatch.setattr(fx, "build_map_script_engine_contract", build_map_script_engine_contract)

    source_site_mutation = deepcopy(original)
    source_command = next(
        command
        for site in source_site_mutation["entityPresentationFxCommandFacts"]["sourceSites"]
        for command in site["commands"]
        if command["macro"] == "headshake"
    )
    source_command["operandValues"][0]["rawValue"] = "SOURCE_ROW_MUTATION"
    mutated_facts = source_site_mutation["entityPresentationFxCommandFacts"]
    monkeypatch.setattr(
        fx,
        "build_map_script_engine_contract",
        lambda *_: source_site_mutation,
    )
    with pytest.raises(ValueError, match="complete H2 source-site hash drift"):
        fx.build_map_script_entity_presentation_fx_contract(ROM, UPSTREAM)

    mutated_facts["sourceSitesSha256"] = fx.hashlib.sha256(
        fx._canonical_bytes({"sourceSites": mutated_facts["sourceSites"]})
    ).hexdigest().upper()
    with pytest.raises(ValueError, match="H2 compact fixture/source drift"):
        fx.build_map_script_entity_presentation_fx_contract(ROM, UPSTREAM)
    monkeypatch.setattr(fx, "build_map_script_engine_contract", build_map_script_engine_contract)

    source_section = fx._source_section

    def mutated_source_section(*args: object, **kwargs: object) -> list[dict]:
        rows = source_section(*args, **kwargs)
        if args[1] == "csc22_animateEntityFadeInOrOut":
            return [
                {
                    **row,
                    "instruction": row["instruction"].replace("lsl.w #3,d0", "lsl.w #4,d0"),
                }
                for row in rows
            ]
        return rows

    monkeypatch.setattr(fx, "_source_section", mutated_source_section)
    with pytest.raises(ValueError, match="H1/source instruction identity drift"):
        fx.build_map_script_entity_presentation_fx_contract(ROM, UPSTREAM)
    monkeypatch.setattr(fx, "_source_section", source_section)

    static = _static()
    source = (UPSTREAM / fx.SOURCE_PATH).read_text(encoding="utf-8")
    table_start = source.index("table_EntityFadingDefinitions:")
    changed = source[:table_start] + source[table_start:].replace(
        "                dc.w 1\n                dc.w 0\n",
        "                dc.w 2\n                dc.w 0\n",
        1,
    )
    with pytest.raises(ValueError, match="source/ROM parity"):
        fx._transition_table(
            changed,
            static["function"] | {"table_EntityFadingDefinitions": 289714},
            ROM,
            record_byte_count=static["constants"]["transitionTableRecordByteCount"],
        )


def test_instruction_parser_accepts_suffixes_and_rejects_non_instructions() -> None:
    rows = [
        {"instruction": "bsr.w LoadMapsprite", "sourceLine": 1},
        {"instruction": "jsr (WaitForVInt).w", "sourceLine": 2},
        {"instruction": "bsr.s sub_45D46", "sourceLine": 3},
        {"instruction": "beq.s LoadMapsprite", "sourceLine": 4},
        {"instruction": "LoadMapsprite:", "sourceLine": 5},
        {"instruction": "dc.b 'LoadMapsprite'", "sourceLine": 6},
    ]
    assert [(call["opcode"], call["instructionTarget"]) for call in fx._direct_calls(rows)] == [
        ("bsr", "LoadMapsprite"),
        ("jsr", "WaitForVInt"),
        ("bsr", "sub_45D46"),
    ]
    source_rows = fx._source_section(
        "\n".join(
            (
                "TestInstructionParsing:",
                "    bsr.s LoadMapsprite ; this comment must not become an operand",
                "    jsr.w (WaitForVInt).w",
                "    ; End of function TestInstructionParsing",
            )
        ),
        "TestInstructionParsing",
    )
    assert [
        (call["opcode"], call["instructionTarget"]) for call in fx._direct_calls(source_rows)
    ] == [
        ("bsr", "LoadMapsprite"),
        ("jsr", "WaitForVInt"),
    ]


def test_session_shims_are_preflight_validated_before_lua_callbacks() -> None:
    fixture = _fixture()
    static = _static()
    patches = fx._service_interception(static, fixture, ROM)
    assert [patch["targetIdentity"] for patch in patches] == list(fx.TARGET_IDENTITIES[:-5]) + list(
        fx.TARGET_IDENTITIES[-4:]
    )
    assert "WaitForVInt" not in {patch["targetIdentity"] for patch in patches}
    assert (
        fixture["instrumentation"]["callSiteAddress"]
        == static["function"]["entryInjectionCallSiteAddress"]
    )
    text = fx.OBSERVER.read_text(encoding="utf-8")
    assert "memory.write_u8(" not in text.split("local function setup_case()", 1)[0]
    assert (
        "memory.write_u8("
        not in text.split("local function setup_case()", 1)[1].split("event.on_bus_exec", 1)[1]
    )
    assert "serviceInterception.entryHooks" in text
    _, executable = bizhawk_contract()
    validate_lua_syntax(fx.OBSERVER, executable)

    drifted = deepcopy(fixture)
    drifted["instrumentation"]["serviceInterception"]["patches"][0]["originalHex"] = "00"
    with pytest.raises(ValueError, match="ROM byte drift"):
        fx._service_interception(static, drifted, ROM)
