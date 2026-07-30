from __future__ import annotations

from copy import deepcopy
from pathlib import Path

import pytest

import sf2tool.h3.map_camera_control as map_camera_control
from sf2tool.h2.map_script_engine import build_map_script_engine_contract
from sf2tool.h3.bizhawk import bizhawk_contract, validate_lua_syntax
from sf2tool.h3.map_camera_control import (
    FIXTURE,
    FIXTURE_SCHEMA,
    OBSERVATION_SCHEMA,
    OBSERVER,
    SERVICE_SOURCE_PATH,
    _direct_call_site,
    _section,
    _service_use_sites,
    build_map_camera_control_contract,
    derive_case_expectations,
)
from sf2tool.jsonio import load_json, validate_json

ROM = Path("local/roms/sf2-us.bin")
UPSTREAM = Path("local/upstream/SF2DISASM")


def _fixture() -> dict[str, object]:
    return load_json(FIXTURE)


def _observation() -> dict[str, object]:
    fixture = _fixture()
    return {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["id"],
        "mapTest": fixture["mapTestIndex"],
        "recordOrder": [case["id"] for case in fixture["cases"]],
        "records": [{**case["expected"], **case["runtimeGolden"]} for case in fixture["cases"]],
    }


@pytest.fixture(scope="module")
def h2_contract() -> dict[str, object]:
    return build_map_script_engine_contract(ROM, UPSTREAM)


def test_map_camera_control_static_contract_fixture_and_queue_are_complete() -> None:
    fixture = _fixture()
    validate_json(fixture, FIXTURE_SCHEMA, owner="map camera control fixture")
    static = build_map_camera_control_contract(ROM, UPSTREAM)
    assert static == {
        "function": fixture["function"],
        "ram": fixture["ram"],
        "constants": {
            "enemyIndexDifference": 96,
            "byteMask": 255,
            "mapTileSize": 384,
        },
        "serviceUseSites": {
            "multiplication": [
                {"opcode": "mulu.w", "operand": "#MAP_TILE_SIZE,d2", "sourceLine": 942},
                {"opcode": "mulu.w", "operand": "#MAP_TILE_SIZE,d3", "sourceLine": 943},
            ],
            "transfer": [
                {"opcode": "movem.w", "operand": "d2-d3,-(sp)", "sourceLine": 944},
                {"opcode": "movem.w", "operand": "(sp)+,d0-d1", "sourceLine": 945},
            ],
            "callAndReturn": [
                {"opcode": "jsr", "operand": "(SetViewDestination).w", "sourceLine": 946},
                {"opcode": "rts", "operand": "", "sourceLine": 947},
            ],
        },
        "transferWidths": {
            "targetInputWordByteCount": 2,
            "targetInputSignBit": 32768,
            "targetByteCount": 1,
            "targetByteSignBit": 128,
            "destinationInputWordByteCount": 2,
            "destinationWordByteCount": 2,
            "destinationTargetByteCount": 1,
            "speedWordByteCount": 2,
        },
        "sourceStateValues": {"destinationViewTargetEntityLiteral": -1},
    }
    assert derive_case_expectations(static, fixture) == [
        case["expected"] for case in fixture["cases"]
    ]
    assert fixture["runtimeQuestions"] == [
        "map-script-camera-control/normal-story-reachability",
        "map-script-camera-control/vdp-player-visible-behavior",
    ]
    assert [case["id"] for case in fixture["cases"]] == [
        "target-negative-direct",
        "target-ally-index",
        "target-enemy-index",
        "destination-one-two",
        "destination-word-wrap",
        "speed-eight",
        "speed-sixty-four",
    ]
    assert [case["expected"] for case in fixture["cases"]] == [
        {
            "id": "target-negative-direct",
            "kind": "target",
            "targetMode": "negative-direct",
            "handlerAddress": 289848,
            "operandWord": 65408,
            "targetEntityLookupAddress": None,
            "entityIndexListLookupIndex": None,
            "viewTargetEntityByteAfter": 128,
        },
        {
            "id": "target-ally-index",
            "kind": "target",
            "targetMode": "ally-index",
            "handlerAddress": 289848,
            "operandWord": 2,
            "targetEntityLookupAddress": 289870,
            "entityIndexListLookupIndex": 2,
            "viewTargetEntityByteAfter": 42,
        },
        {
            "id": "target-enemy-index",
            "kind": "target",
            "targetMode": "enemy-index",
            "handlerAddress": 289848,
            "operandWord": 225,
            "targetEntityLookupAddress": 289870,
            "entityIndexListLookupIndex": 129,
            "viewTargetEntityByteAfter": 43,
        },
        {
            "id": "destination-one-two",
            "kind": "destination",
            "handlerAddress": 288006,
            "setCameraDestinationCallSiteAddress": 288018,
            "waitForViewScrollEndCallSiteAddress": 288024,
            "setCameraDestinationServiceAddress": 144526,
            "setViewDestinationAddress": 13994,
            "setViewDestinationCallSiteAddress": 144542,
            "inputWords": [1, 2],
            "setViewDestinationD0Word": 384,
            "setViewDestinationD1Word": 768,
            "viewTargetEntityByteAfter": 255,
        },
        {
            "id": "destination-word-wrap",
            "kind": "destination",
            "handlerAddress": 288006,
            "setCameraDestinationCallSiteAddress": 288018,
            "waitForViewScrollEndCallSiteAddress": 288024,
            "setCameraDestinationServiceAddress": 144526,
            "setViewDestinationAddress": 13994,
            "setViewDestinationCallSiteAddress": 144542,
            "inputWords": [257, 2],
            "setViewDestinationD0Word": 33152,
            "setViewDestinationD1Word": 768,
            "viewTargetEntityByteAfter": 255,
        },
        {
            "id": "speed-eight",
            "kind": "speed",
            "handlerAddress": 288512,
            "operandWord": 8,
            "viewScrollingSpeedWordAfter": 8,
        },
        {
            "id": "speed-sixty-four",
            "kind": "speed",
            "handlerAddress": 288512,
            "operandWord": 64,
            "viewScrollingSpeedWordAfter": 64,
        },
    ]


@pytest.mark.parametrize(
    ("name", "mutation"),
    [
        ("missing-nested-field", lambda value: value["cases"][0]["expected"].pop("targetMode")),
        (
            "renamed-nested-field",
            lambda value: value["function"].update(
                {"entry_address": value["function"].pop("entryAddress")}
            ),
        ),
        (
            "extra-nested-field",
            lambda value: value["cases"][3]["expected"].update({"unexpected": 1}),
        ),
        ("reordered-cases", lambda value: value["cases"].reverse()),
        ("word-boundary", lambda value: value["cases"][5].__setitem__("operandWord", 65536)),
    ],
)
def test_map_camera_control_fixture_schema_rejects_full_object_mutations(
    name: str, mutation: object
) -> None:
    del name
    mutated = deepcopy(_fixture())
    assert callable(mutation)
    mutation(mutated)
    with pytest.raises(ValueError, match="fixture failed schema validation"):
        validate_json(mutated, FIXTURE_SCHEMA, owner="map camera control fixture")


@pytest.mark.parametrize(
    ("name", "mutation"),
    [
        ("missing-nested-field", lambda value: value["records"][0].pop("callbackOrder")),
        (
            "renamed-nested-field",
            lambda value: value["records"][3].update(
                {"handler_address": value["records"][3].pop("handlerAddress")}
            ),
        ),
        ("extra-nested-field", lambda value: value["records"][1].update({"unexpected": 1})),
        ("reordered-records", lambda value: value["records"].reverse()),
        (
            "word-boundary",
            lambda value: value["records"][4].__setitem__("setViewDestinationD0Word", 65536),
        ),
    ],
)
def test_map_camera_control_observation_schema_rejects_full_object_mutations(
    name: str, mutation: object
) -> None:
    del name
    mutated = _observation()
    assert callable(mutation)
    mutation(mutated)
    with pytest.raises(ValueError, match="observation failed schema validation"):
        validate_json(mutated, OBSERVATION_SCHEMA, owner="map camera control observation")


def test_map_camera_control_observation_schema_accepts_complete_semantic_object() -> None:
    validate_json(_observation(), OBSERVATION_SCHEMA, owner="map camera control observation")


def test_map_camera_control_service_guard_rejects_source_use_site_drift() -> None:
    source = (UPSTREAM / "disasm" / SERVICE_SOURCE_PATH).read_text(encoding="utf-8")
    assert _service_use_sites(source)["multiplication"][1]["operand"] == "#MAP_TILE_SIZE,d3"
    service_offset = source.index("SetCameraDestination:")
    prefix, service = source[:service_offset], source[service_offset:]
    with pytest.raises(ValueError, match="SetCameraDestination multiplication"):
        _service_use_sites(prefix + service.replace("#MAP_TILE_SIZE,d3", "#MAP_TILE_SIZE,d4", 1))
    with pytest.raises(ValueError, match="SetCameraDestination multiplication"):
        _service_use_sites(
            prefix + service.replace("mulu.w  #MAP_TILE_SIZE,d2", "mulu.l  #MAP_TILE_SIZE,d2", 1)
        )
    with pytest.raises(ValueError, match="SetCameraDestination multiplication"):
        _service_use_sites(
            prefix
            + service.replace("jsr     (SetViewDestination).w", "bsr.w   SetViewDestination", 1)
        )


@pytest.mark.parametrize(
    ("name", "mutate", "message"),
    [
        (
            "negative-branch-polarity",
            lambda facts: facts["handlers"][0]["sectionGuard"]["branchRecords"][0].update(
                {"branchInstruction": "bpl.w loc_46C52"}
            ),
            "target negative branch polarity",
        ),
        (
            "ally-branch-polarity",
            lambda facts: facts["handlers"][0]["sectionGuard"]["branchRecords"][1].update(
                {"branchInstruction": "bmi.s @Ally"}
            ),
            "target ally branch polarity",
        ),
        (
            "target-read-width",
            lambda facts: facts["handlers"][0]["sectionGuard"]["scriptCursorReadUseSites"][
                0
            ].update({"transferredByteCount": 1}),
            "target script cursor read transfer-width",
        ),
        (
            "target-state-write-width",
            lambda facts: facts["handlers"][0]["sectionGuard"]["sourceStateWrites"][0].update(
                {"instruction": "move.w d0,((VIEW_TARGET_ENTITY-$1000000)).w"}
            ),
            "target state write relation",
        ),
        (
            "destination-write-literal",
            lambda facts: facts["handlers"][1]["sectionGuard"]["sourceStateWrites"][0].update(
                {"valueReference": 0}
            ),
            "destination target state write literal",
        ),
        (
            "speed-write-width",
            lambda facts: facts["handlers"][2]["sectionGuard"]["scriptCursorWriteUseSites"][
                0
            ].update({"transferredByteCount": 1}),
            "speed script cursor write transfer-width",
        ),
    ],
)
def test_map_camera_control_h2_use_site_mutations_fail_before_fixture_comparison(
    monkeypatch: pytest.MonkeyPatch,
    h2_contract: dict[str, object],
    name: str,
    mutate: object,
    message: str,
) -> None:
    del name
    mutated = deepcopy(h2_contract)
    assert callable(mutate)
    mutate(mutated["mapCameraControlCommandFacts"])
    monkeypatch.setattr(map_camera_control, "build_map_script_engine_contract", lambda *_: mutated)
    with pytest.raises(ValueError, match=message):
        build_map_camera_control_contract(ROM, UPSTREAM)


def test_map_camera_control_instruction_parsers_ignore_comments_and_accept_suffixes() -> None:
    source = """Example:
    move.w  d0,d1 ; SetCameraDestination is only a comment
    bra.s   @done
    rts
    ; End of function Example
"""
    assert _section(source, "Example") == [
        {"opcode": "move.w", "operand": "d0,d1", "sourceLine": 2},
        {"opcode": "bra.s", "operand": "@done", "sourceLine": 3},
        {"opcode": "rts", "operand": "", "sourceLine": 4},
    ]
    with pytest.raises(ValueError, match="cannot parse"):
        _section(source.replace("move.w", "move.wx", 1), "Example")
    listing = """00000000 Example:
00000010 4EB8 0000                                  jsr     (SetViewDestination).w
00000016                                             ; jsr (SetViewDestination).w
00000018                                ; End of function Example
"""
    assert _direct_call_site(listing, "Example", "SetViewDestination") == 16
    with pytest.raises(ValueError, match="direct call-site drift"):
        _direct_call_site(listing, "Example", "SetViewDestinations")


def test_map_camera_control_lua_syntax_preflight() -> None:
    _, executable = bizhawk_contract()
    validate_lua_syntax(OBSERVER, executable)
