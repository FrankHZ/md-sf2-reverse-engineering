from __future__ import annotations

import re
from copy import deepcopy
from pathlib import Path

import pytest

import sf2tool.h3.map_script_entity_clone as clone
from sf2tool.h2.map_script_engine import build_map_script_engine_contract
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

FIXTURE = repo_path("tests/fixtures/h3/map-script-entity-clone-v1.json")
FIXTURE_SCHEMA = repo_path("schemas/h3-map-script-entity-clone-fixture.schema.json")
OBSERVATION_SCHEMA = repo_path("schemas/h3-map-script-entity-clone-observation.schema.json")
OBSERVER = repo_path("tools/bizhawk/map_script_entity_clone_observer.lua")


def _static() -> dict[str, object]:
    return clone.build_map_script_entity_clone_contract(
        repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
    )


def _observation(fixture: dict[str, object]) -> dict[str, object]:
    cases = fixture["cases"]
    assert isinstance(cases, list)
    return {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["id"],
        "mapTest": fixture["mapTestIndex"],
        "recordOrder": [case["id"] for case in cases],
        "records": [case["expected"] for case in cases],
    }


def test_entity_clone_static_contract_and_complete_nine_case_fixture_match() -> None:
    fixture = load_json(FIXTURE)
    actual = _static()
    h2 = build_map_script_engine_contract(
        repo_path("local/roms/sf2-us.bin"), repo_path("local/upstream/SF2DISASM")
    )["entityCloneCommandFacts"]

    assert {key: actual[key] for key in fixture if key in actual} == {
        key: fixture[key] for key in fixture if key in actual
    }
    assert fixture["sourceContract"] == actual["sourceFacts"]["compactH2Boundary"]
    assert actual["sourceFacts"]["compactH2Boundary"]["callerBreakdown"] == h2["callerBreakdown"]
    assert actual["sourceFacts"]["compactH2Boundary"]["callerBreakdown"] == {
        "callerHandlers": [
            {
                "handler": "csc25_cloneEntity",
                "instructionTargetSiteCounts": {"GetEntityAddressFromCharacter": 2},
                "effectiveTargetSiteCounts": {"GetEntityAddressFromCharacter": 2},
            }
        ],
        "targetResolutions": [
            {
                "instructionTarget": "GetEntityAddressFromCharacter",
                "effectiveTarget": "GetEntityAddressFromCharacter",
                "aliasSourcePath": None,
                "effectiveTargetScope": "external",
            }
        ],
        "instructionTargetTotals": {"GetEntityAddressFromCharacter": 2},
        "effectiveTargetTotals": {"GetEntityAddressFromCharacter": 2},
        "internalInstructionTargetTotals": {"GetEntityAddressFromCharacter": 0},
        "externalInstructionTargetTotals": {"GetEntityAddressFromCharacter": 2},
        "internalEffectiveTargetTotals": {"GetEntityAddressFromCharacter": 0},
        "externalEffectiveTargetTotals": {"GetEntityAddressFromCharacter": 2},
    }
    assert actual["runtimeQuestions"] == [
        "map-script-entity-clone/further-runtime-state-matrix",
        "map-script-entity-clone/further-runtime-external-consumer-matrix",
        "map-script-entity-clone/further-runtime-context-matrix",
    ]
    assert actual["function"] == {
        "handlerEntryAddress": 289882,
        "sourceOperandReadAddress": 289882,
        "destinationOperandReadAddress": 289892,
        "sourceLookupCallSiteAddress": 289884,
        "destinationLookupCallSiteAddress": 289894,
        "sourceLookupReturnAddress": 289888,
        "destinationLookupReturnAddress": 289898,
        "sourceFieldReadAddress": 289888,
        "destinationFieldWriteAddress": 289898,
        "handlerRtsAddress": 289902,
        "lookupEntryAddress": 290890,
        "runMapSetupInitFunctionAddress": 292092,
    }
    assert actual["constants"]["entityRecordByteCount"] == 32
    assert actual["constants"]["entnumByteOffset"] == 18
    assert actual["constants"]["lookupIndexDifference"] == 96

    derived = clone.derive_case_expectations(actual, fixture)
    assert derived == [case["expected"] for case in fixture["cases"]]
    assert [case["sourceInput"]["sourceWords"] for case in fixture["cases"]] == [
        [129, 130],
        [131, 132],
        [131, 133],
        [131, 134],
        [131, 135],
        [131, 136],
        [131, 137],
        [131, 138],
        [132, 131],
    ]
    assert [row["lookupCallSequence"] for row in derived] == [
        [
            {
                "ordinal": 1,
                "callSitePc": 289884,
                "targetEntryPc": 290890,
                "returnPc": 289888,
                "lookupIndexByteOffsetObserved": first - 96,
            },
            {
                "ordinal": 2,
                "callSitePc": 289894,
                "targetEntryPc": 290890,
                "returnPc": 289898,
                "lookupIndexByteOffsetObserved": second - 96,
            },
        ]
        for first, second in [
            (129, 130),
            (131, 132),
            (131, 133),
            (131, 134),
            (131, 135),
            (131, 136),
            (131, 137),
            (131, 138),
            (132, 131),
        ]
    ]
    assert all(row["cursorAdvanceByteCountObserved"] == 4 for row in derived)
    assert all(row["destinationEntnumWrite"]["byteOffset"] == 18 for row in derived)
    assert all(
        [item["byteValueBeforeObserved"] for item in row["destinationAdjacentBytes"]]
        == [item["byteValueAfterObserved"] for item in row["destinationAdjacentBytes"]]
        for row in derived
    )


@pytest.mark.parametrize(
    ("before", "after"),
    [
        ("move.b d1,ENTITYDEF_OFFSET_ENTNUM(a5)", "move.w d1,ENTITYDEF_OFFSET_ENTNUM(a5)"),
        ("bsr.w GetEntityAddressFromCharacter", "bsr.w GetEntityAddressFromCharacterElse"),
        ("bpl.s @Ally", "bmi.s @Ally"),
        ("lsl.w #ENTITYDEF_SIZE_BITS,d0", "lsl.b #ENTITYDEF_SIZE_BITS,d0"),
    ],
)
def test_entity_clone_source_use_site_mutations_fail_before_fixture_comparison(
    monkeypatch: pytest.MonkeyPatch, before: str, after: str
) -> None:
    upstream = repo_path("local/upstream/SF2DISASM").resolve()
    source_path = (upstream / clone.SOURCE_PATH).resolve()
    original_read_bytes = Path.read_bytes

    def altered_read_bytes(path: Path, *args: object, **kwargs: object) -> bytes:
        value = original_read_bytes(path, *args, **kwargs)
        if path.resolve() == source_path:
            source = value.decode("latin-1")
            pattern = re.escape(before).replace(r"\ ", r"\s+")
            changed, count = re.subn(pattern, after, source, count=1)
            if count != 1:
                raise AssertionError(f"entity clone source mutation target drift: {before}")
            return changed.encode("latin-1")
        return value

    monkeypatch.setattr(Path, "read_bytes", altered_read_bytes)
    with pytest.raises(ValueError) as error:
        clone.build_map_script_entity_clone_contract(repo_path("local/roms/sf2-us.bin"), upstream)
    assert "fixture/source" not in str(error.value)


def test_entity_clone_parser_accepts_legal_width_suffixes_and_rejects_near_misses() -> None:
    assert [
        clone._instruction_width(instruction)
        for instruction in ("move.b (a6)+,d0", "move.w (a6)+,d0", "move.l (a6)+,d0")
    ] == [1, 2, 4]
    with pytest.raises(ValueError, match="transfer width"):
        clone._instruction_width("move.x (a6)+,d0")
    with pytest.raises(ValueError, match="transfer width"):
        clone._instruction_width("cloneEntity")

    source = "\n".join(
        (
            "csc25_cloneEntity:",
            "  move.w (a6)+,d0 ; GetEntityAddressFromCharacter comment only",
            "comment_target:",
            "  bsr.w GetEntityAddressFromCharacter",
            "; move.b d1,ENTITYDEF_OFFSET_ENTNUM(a5)",
            "; End of function csc25_cloneEntity",
        )
    )
    assert clone._source_section(source, clone.HANDLER) == [
        {"instruction": "move.w (a6)+,d0", "sourceLine": 2},
        {"instruction": "bsr.w GetEntityAddressFromCharacter", "sourceLine": 4},
    ]


def test_entity_clone_instrumentation_rejects_original_byte_mutation_before_runtime() -> None:
    fixture = deepcopy(load_json(FIXTURE))
    fixture["instrumentation"]["callSiteOriginalHex"] = "000000000000"
    with pytest.raises(ValueError, match="original-byte drift"):
        clone._instrument_entity_clone_rom(repo_path("local/roms/sf2-us.bin"), fixture)


def test_entity_clone_observer_uses_bracketed_function_keys_and_runtime_pc_reads() -> None:
    observer = OBSERVER.read_text(encoding="utf-8")
    assert 'config["function"]' in observer
    assert 'config.harness["function"]' in observer
    assert "config.function" not in observer
    assert "config.harness.function" not in observer
    assert 'emu.getregister("M68K PC")' in observer
    assert "targetEntryPc=actual" in observer
    assert "returnPc=actual" in observer
    assert "handler_entry_a6_offset=a6_offset()" in observer
    assert "scriptCursorRamOffsetBefore=handler_entry_a6_offset" in observer
    assert "cursorAdvanceByteCountObserved=after-handler_entry_a6_offset" in observer
    assert "case.scriptCursorRamOffsetBefore" not in observer


@pytest.mark.parametrize(
    "mutate",
    [
        lambda value: value["cases"][0]["expected"]["operandReads"][0].pop("wordObserved"),
        lambda value: value["cases"][1]["sourceInput"].update(
            {"sourceWordsRenamed": value["cases"][1]["sourceInput"].pop("sourceWords")}
        ),
        lambda value: value["cases"][2]["harnessControls"]["destinationAdjacentByteSeeds"][
            0
        ].update({"unexpected": 1}),
        lambda value: value["cases"].reverse(),
        lambda value: value["cases"].__setitem__(1, deepcopy(value["cases"][0])),
        lambda value: value["cases"][0]["sourceInput"]["sourceWords"].__setitem__(0, 128),
    ],
)
def test_entity_clone_fixture_schema_rejects_nested_mutations_and_boundary(
    mutate: object,
) -> None:
    value = deepcopy(load_json(FIXTURE))
    assert callable(mutate)
    mutate(value)
    with pytest.raises(ValueError, match="fixture failed schema validation"):
        validate_json(value, FIXTURE_SCHEMA, owner="map-script entity clone fixture")


@pytest.mark.parametrize(
    "mutate",
    [
        lambda value: value["records"][0]["destinationEntnumWrite"].pop("byteValueAfterObserved"),
        lambda value: value["records"][1].update(
            {"handlerEntryPcRenamed": value["records"][1].pop("handlerEntryPc")}
        ),
        lambda value: value["records"][2]["lookupCallSequence"][0].update({"unexpected": 1}),
        lambda value: value["records"].reverse(),
        lambda value: value["records"].__setitem__(1, deepcopy(value["records"][0])),
        lambda value: value["records"][0]["operandReads"][0].__setitem__("wordObserved", 128),
        lambda value: value["records"][0].__setitem__("handlerEntryPc", 289883),
        lambda value: value["records"][0].__setitem__("scriptCursorRamOffsetBefore", 5),
    ],
)
def test_entity_clone_observation_schema_rejects_nested_mutations_and_boundary(
    mutate: object,
) -> None:
    value = _observation(load_json(FIXTURE))
    assert callable(mutate)
    mutate(value)
    with pytest.raises(ValueError, match="observation failed schema validation"):
        validate_json(value, OBSERVATION_SCHEMA, owner="map-script entity clone observation")
