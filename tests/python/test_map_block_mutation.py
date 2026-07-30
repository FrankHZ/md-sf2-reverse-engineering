from __future__ import annotations

from copy import deepcopy
from pathlib import Path

import pytest

import sf2tool.h3.map_block_mutation as map_block_mutation
from sf2tool.h2.map_script_engine import build_map_script_engine_contract
from sf2tool.h3.bizhawk import bizhawk_contract, validate_lua_syntax
from sf2tool.h3.map_block_mutation import (
    FIXTURE,
    FIXTURE_SCHEMA,
    OBSERVATION_SCHEMA,
    OBSERVER,
    _case_inputs,
    _derive_case_expectations,
    _h1_direct_call_addresses,
    _instrument_rom,
    _parse_equates,
    _require_exact_source_order,
    _update_bit_use_sites,
    build_map_block_mutation_contract,
)
from sf2tool.jsonio import load_json, validate_json

ROM = Path("local/roms/sf2-us.bin")
UPSTREAM = Path("local/upstream/SF2DISASM")


def test_map_block_mutation_static_contract_binds_handlers_helper_and_callers() -> None:
    contract = build_map_block_mutation_contract(ROM, UPSTREAM)

    assert contract["function"] == {
        "runMapSetupInitFunctionAddress": 292092,
        "setBlocksHandlerAddress": 288102,
        "setBlocksVarHandlerAddress": 288130,
        "copyMapBlocksAddress": 15792,
        "copyInstructionAddress": 15842,
    }
    assert contract["ram"] == {
        "layoutBaseAddress": 16711680,
        "updateToggleBitfieldAddress": 16754733,
    }
    assert contract["constants"] == {
        "byteShiftCount": 8,
        "layoutRowShiftBits": 6,
        "layoutWordColumnCount": 64,
        "wordCopyByteStride": 2,
        "rowByteStride": 128,
        "mapTileSize": 384,
        "layoutClearLongCounter": 2047,
        "layoutStoredByteCount": 8192,
        "layoutStoredWordCount": 4096,
        "layoutWordRowCount": 64,
    }
    assert contract["sourceFacts"]["handlers"] == [
        {
            "macro": "setBlocks",
            "opcode": 52,
            "handler": "csc34_setBlocks",
            "handlerAddress": 288102,
            "sourceCommandCount": 201,
            "operandByteCount": 6,
            "cursorInputWordCount": 3,
            "inputWordGroups": [
                {
                    "handlerRegister": "d0",
                    "highByteSourceLabel": "source x",
                    "lowByteSourceLabel": "source y",
                },
                {
                    "handlerRegister": "d1",
                    "highByteSourceLabel": "width",
                    "lowByteSourceLabel": "height",
                },
                {
                    "handlerRegister": "d2",
                    "highByteSourceLabel": "destination x",
                    "lowByteSourceLabel": "destination y",
                },
            ],
            "copyMapBlocksCallSiteAddress": 288108,
            "postCallUpdateBitSetUseSites": [
                {
                    "bitIndex": 0,
                    "sourceTarget": "VIEW_PLANE_UPDATE_TOGGLE_BITFIELD",
                    "instruction": "bset #0,(VIEW_PLANE_UPDATE_TOGGLE_BITFIELD).l",
                    "instructionAddress": 288112,
                },
                {
                    "bitIndex": 1,
                    "sourceTarget": "VIEW_PLANE_UPDATE_TOGGLE_BITFIELD",
                    "instruction": "bset #1,(VIEW_PLANE_UPDATE_TOGGLE_BITFIELD).l",
                    "instructionAddress": 288120,
                },
            ],
        },
        {
            "macro": "setBlocksVar",
            "opcode": 53,
            "handler": "csc35_setBlocksVar",
            "handlerAddress": 288130,
            "sourceCommandCount": 7,
            "operandByteCount": 6,
            "cursorInputWordCount": 3,
            "inputWordGroups": [
                {
                    "handlerRegister": "d0",
                    "highByteSourceLabel": "source x",
                    "lowByteSourceLabel": "source y",
                },
                {
                    "handlerRegister": "d1",
                    "highByteSourceLabel": "width",
                    "lowByteSourceLabel": "height",
                },
                {
                    "handlerRegister": "d2",
                    "highByteSourceLabel": "destination x",
                    "lowByteSourceLabel": "destination y",
                },
            ],
            "copyMapBlocksCallSiteAddress": 288136,
            "postCallUpdateBitSetUseSites": [],
        },
    ]
    assert contract["sourceFacts"]["copyHelper"] == {
        "helper": "CopyMapBlocks",
        "helperAddress": 15792,
        "packedInputByteShiftBits": 8,
        "addressRowShiftBits": 6,
        "layoutWordColumnCount": 64,
        "wordCopyByteStride": 2,
        "rowByteStride": 128,
        "copyInstruction": "move.w (a2,d0.w),(a2,d2.w)",
        "innerLoop": {
            "counterRegister": "d6",
            "seedInstruction": "move.w d1,d6",
            "decrementInstruction": "subq.w #1,d6",
            "loopInstruction": "dbf d6,loc_3DE2",
        },
        "outerLoop": {
            "counterRegister": "d7",
            "seedInstruction": "move.b d1,d7",
            "decrementInstruction": "subq.w #1,d7",
            "loopInstruction": "dbf d7,loc_3DDE",
        },
        "copyInstructionAddress": 15842,
    }
    assert contract["sourceFacts"]["callerBreakdown"] == {
        "callerHandlers": [
            {
                "handler": "csc34_setBlocks",
                "instructionTargetSiteCounts": {"CopyMapBlocks": 1},
                "effectiveTargetSiteCounts": {"CopyMapBlocks": 1},
            },
            {
                "handler": "csc35_setBlocksVar",
                "instructionTargetSiteCounts": {"CopyMapBlocks": 1},
                "effectiveTargetSiteCounts": {"CopyMapBlocks": 1},
            },
        ],
        "targetResolutions": [
            {
                "instructionTarget": "CopyMapBlocks",
                "effectiveTarget": "CopyMapBlocks",
                "aliasSourcePath": None,
                "effectiveTargetScope": "external",
            }
        ],
        "instructionTargetTotals": {"CopyMapBlocks": 2},
        "effectiveTargetTotals": {"CopyMapBlocks": 2},
        "internalEffectiveTargetTotals": {"CopyMapBlocks": 0},
        "externalEffectiveTargetTotals": {"CopyMapBlocks": 2},
    }
    assert contract["sourceFacts"]["runtimeQuestions"] == [
        "map-block-mutation/collision-pathfinding-consumer-effects",
        "map-block-mutation/normal-story-reachability-and-map-reload-save-persistence",
        "map-block-mutation/visible-vdp-presentation-and-cycle-pixel-timing",
    ]


def test_map_block_mutation_h2_use_site_mutations_fail_before_fixture_comparison(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    static = build_map_script_engine_contract(ROM, UPSTREAM)
    mutated = deepcopy(static)
    helper = mutated["mapBlockMutationCommandFacts"]["copyMapBlocksHelperFacts"]
    helper["addressRowShiftUses"][1]["value"] = 5
    monkeypatch.setattr(map_block_mutation, "build_map_script_engine_contract", lambda *_: mutated)
    with pytest.raises(ValueError, match="helper use-site value disagreement"):
        build_map_block_mutation_contract(ROM, UPSTREAM)


@pytest.mark.parametrize(
    ("path", "old", "new", "message"),
    [
        (
            "disasm/code/gameflow/exploration/exploration.asm",
            "lsl.w   #6,d1",
            "lsl.w   #5,d1",
            "source use-site/order drift",
        ),
        (
            "disasm/code/gameflow/exploration/exploration.asm",
            "addi.w  #128,d2",
            "addi.w  #126,d2",
            "source use-site/order drift",
        ),
        (
            "disasm/code/gameflow/exploration/exploration.asm",
            "move.w  (a2,d0.w),(a2,d2.w)",
            "move.w  (a2,d2.w),(a2,d0.w)",
            "source use-site/order drift",
        ),
        (
            "disasm/code/gameflow/exploration/exploration.asm",
            "subq.w  #1,d6",
            "subq.w  #2,d6",
            "source use-site/order drift",
        ),
        (
            "disasm/code/gameflow/exploration/exploration.asm",
            "dbf     d7,loc_3DDE",
            "dbf     d6,loc_3DDE",
            "source use-site/order drift",
        ),
        (
            "disasm/code/gameflow/exploration/exploration.asm",
            "move.w  #MAP_LAYOUT_LONGS_COUNTER,d7",
            "move.w  #2046,d7",
            "layout-span use-site drift",
        ),
        (
            "disasm/code/common/scripting/map/mapscriptengine_1.asm",
            "bset    #0,(VIEW_PLANE_UPDATE_TOGGLE_BITFIELD).l\n                bset    #1",
            "bset    #1,(VIEW_PLANE_UPDATE_TOGGLE_BITFIELD).l\n                bset    #0",
            "source use-site/order drift",
        ),
        (
            "disasm/code/common/scripting/map/mapscriptengine_1.asm",
            "csc35_setBlocksVar:\n                \n                move.w",
            "\n".join(
                (
                    "csc35_setBlocksVar:",
                    "                ",
                    "                bset    #0,(VIEW_PLANE_UPDATE_TOGGLE_BITFIELD).l",
                    "                move.w",
                )
            ),
            "source use-site/order drift",
        ),
        (
            "build/sf2build-h1.lst",
            "0004656C 4EB8 3DB0                                  jsr     (CopyMapBlocks).w",
            "0004656C 4E90                                       jsr     (a0)",
            "H1 direct call drift",
        ),
    ],
)
def test_map_block_mutation_actual_source_and_h1_mutations_fail_before_fixture_comparison(
    monkeypatch: pytest.MonkeyPatch,
    path: str,
    old: str,
    new: str,
    message: str,
) -> None:
    static = build_map_script_engine_contract(ROM, UPSTREAM)
    target = UPSTREAM / path
    original_read_text = Path.read_text

    def patched_read_text(self: Path, *args: object, **kwargs: object) -> str:
        text = original_read_text(self, *args, **kwargs)
        if self == target:
            changed = text.replace(old, new, 1)
            assert changed != text
            return changed
        return text

    monkeypatch.setattr(map_block_mutation, "build_map_script_engine_contract", lambda *_: static)
    monkeypatch.setattr(Path, "read_text", patched_read_text)
    with pytest.raises(ValueError, match=message):
        build_map_block_mutation_contract(ROM, UPSTREAM)


def test_map_block_mutation_source_guards_are_smallest_scope_and_comment_safe() -> None:
    source = """\
CopyMapBlocks:
    move.w  (a2,d0.w),(a2,d2.w) ; real copy
    addq.w  #2,d0
; End of function CopyMapBlocks
outside:
    move.w  (a2,d0.w),(a2,d2.w)
"""
    assert _require_exact_source_order(
        source,
        "CopyMapBlocks",
        ["move.w (a2,d0.w),(a2,d2.w)", "addq.w #2,d0"],
    ) == ["move.w (a2,d0.w),(a2,d2.w)", "addq.w #2,d0"]
    with pytest.raises(ValueError, match="source use-site/order drift"):
        _require_exact_source_order(
            source.replace("addq.w  #2,d0", "addq.w  #1,d0"),
            "CopyMapBlocks",
            ["move.w (a2,d0.w),(a2,d2.w)", "addq.w #2,d0"],
        )


def test_map_block_mutation_h1_call_parser_excludes_labels_operands_comments_and_near_misses() -> (
    None
):
    listing = """\
00000000 testHandler:
00000000 4EB8 3DB0 jsr (CopyMapBlocks).w ; one real call
00000004              ; jsr (CopyMapBlocks).w
00000004 303C 0000 move.w #CopyMapBlocks,d0
00000008 6100 0000 bsr.w CopyMapBlocks
0000000C 4E90 jsr (a0)
0000000E 4EB8 0000 jsr (CopyMapBlocksExtra).w
0000000C 4E75 rts
; End of function testHandler
"""
    assert _h1_direct_call_addresses(listing, "testHandler", "CopyMapBlocks") == [0, 8]
    assert _h1_direct_call_addresses(listing, "testHandler", "CopyMapBlock") == []


def test_map_block_mutation_equate_parser_resolves_source_aliases_once(tmp_path: Path) -> None:
    disasm = tmp_path / "disasm"
    disasm.mkdir()
    (disasm / "sf2const.asm").write_text("FF0000_RAM_START: equ $FF0000\n", encoding="utf-8")
    (disasm / "sf2enums.asm").write_text(
        "BYTE_SHIFT_COUNT: equ 8\nMAP_TILE_SIZE: equ 384\nMAP_TILE_PLUS: equ MAP_TILE_SIZE\n",
        encoding="utf-8",
    )
    assert _parse_equates(tmp_path, {"FF0000_RAM_START", "BYTE_SHIFT_COUNT", "MAP_TILE_PLUS"}) == {
        "BYTE_SHIFT_COUNT": 8,
        "FF0000_RAM_START": 16711680,
        "MAP_TILE_PLUS": 384,
    }


def _fixture() -> dict[str, object]:
    return load_json(FIXTURE)


def _observation(fixture: dict[str, object]) -> dict[str, object]:
    cases = fixture["cases"]
    assert isinstance(cases, list)
    records = []
    for case in cases:
        expected = case["expected"]
        runtime = case["runtimeGolden"]
        records.append(
            {
                "id": expected["id"],
                "macro": expected["macro"],
                "handlerAddressObserved": expected["handlerAddress"],
                "copyMapBlocksCallSiteAddressObserved": expected["copyMapBlocksCallSiteAddress"],
                "copyInstructionAddressObserved": expected["copyInstructionAddress"],
                "handlerReturned": runtime["handlerReturned"],
                "directCallInputWordsObserved": runtime["directCallInputWordsObserved"],
                "copyInstructionByteOffsetsObserved": runtime["copyInstructionByteOffsetsObserved"],
                "postCopyUpdateBitObservations": runtime["postCopyUpdateBitObservations"],
                "updateToggleByteAfter": runtime["updateToggleByteAfter"],
                "readbackWordRecords": runtime["readbackWordRecords"],
            }
        )
    return {
        "system": "GEN",
        "core": "Genesis Plus GX",
        "id": fixture["id"],
        "mapTest": fixture["mapTestIndex"],
        "recordOrder": [case["id"] for case in cases],
        "records": records,
    }


def test_map_block_mutation_fixture_contract_is_complete_and_exact() -> None:
    fixture = _fixture()
    validate_json(fixture, FIXTURE_SCHEMA, owner="map block mutation fixture")
    static = build_map_block_mutation_contract(ROM, UPSTREAM)

    assert {key: fixture[key] for key in ("function", "ram", "constants", "sourceFacts")} == static
    derived = _derive_case_expectations(static, fixture)
    assert derived == [{**case["expected"], **case["runtimeGolden"]} for case in fixture["cases"]]
    update_bit_use_sites = _update_bit_use_sites(static)
    assert update_bit_use_sites == [
        {
            "macro": "setBlocks",
            "bitIndex": 0,
            "sourceTarget": "VIEW_PLANE_UPDATE_TOGGLE_BITFIELD",
            "instruction": "bset #0,(VIEW_PLANE_UPDATE_TOGGLE_BITFIELD).l",
            "instructionAddress": 288112,
        },
        {
            "macro": "setBlocks",
            "bitIndex": 1,
            "sourceTarget": "VIEW_PLANE_UPDATE_TOGGLE_BITFIELD",
            "instruction": "bset #1,(VIEW_PLANE_UPDATE_TOGGLE_BITFIELD).l",
            "instructionAddress": 288120,
        },
    ]
    case_inputs = _case_inputs(fixture, derived, static)
    assert case_inputs == [
        {
            "id": case["id"],
            "macro": case["macro"],
            "handlerAddress": case["expected"]["handlerAddress"],
            "copyMapBlocksCallSiteAddress": case["expected"]["copyMapBlocksCallSiteAddress"],
            "copyInstructionExecutionCount": case["expected"]["copyInstructionExecutionCount"],
            "updateBitUseSites": [
                row for row in update_bit_use_sites if row["macro"] == case["macro"]
            ],
            "inputWords": case["expected"]["inputWords"],
            "destinationCoordinate": case["destination"],
            "updateToggleByteSeed": case["updateToggleByteSeed"],
            "initialWords": case["initialWords"],
            "readbackCoordinates": case["readbackCoordinates"],
        }
        for case in fixture["cases"]
    ]
    assert all("runtimeGolden" not in row and "expected" not in row for row in case_inputs)
    observation = _observation(fixture)
    validate_json(observation, OBSERVATION_SCHEMA, owner="map block mutation observation")
    assert (
        observation["records"]
        == load_json(OBSERVATION_SCHEMA)["properties"]["records"]["allOf"][0]["const"]
    )


@pytest.mark.parametrize(
    ("target", "mutate", "match"),
    [
        (
            "fixture",
            lambda value: value["cases"][0]["runtimeGolden"].pop("readbackWordRecords"),
            "required property",
        ),
        (
            "fixture",
            lambda value: value["cases"][0]["expected"].__setitem__("unexpectedNestedField", 1),
            "Additional properties",
        ),
        (
            "fixture",
            lambda value: value["cases"][0]["expected"].__setitem__(
                "handlerAddressRenamed",
                value["cases"][0]["expected"].pop("handlerAddress"),
            ),
            "handlerAddress",
        ),
        (
            "fixture",
            lambda value: value["cases"].reverse(),
            "was expected",
        ),
        (
            "fixture",
            lambda value: value["cases"][0]["source"].__setitem__("x", 64),
            "greater than the maximum of 63",
        ),
        (
            "observation",
            lambda value: value["records"][0].pop("copyInstructionAddressObserved"),
            "required property",
        ),
        (
            "observation",
            lambda value: value["records"][0].__setitem__(
                "handlerAddressRenamed",
                value["records"][0].pop("handlerAddressObserved"),
            ),
            "handlerAddressObserved",
        ),
        (
            "observation",
            lambda value: value["records"][0].__setitem__("unexpectedNestedField", 1),
            "Additional properties",
        ),
        (
            "observation",
            lambda value: value["records"][2]["copyInstructionByteOffsetsObserved"].reverse(),
            "was expected",
        ),
        (
            "observation",
            lambda value: value["records"][0]["directCallInputWordsObserved"].__setitem__(0, 65536),
            "greater than the maximum of 65535",
        ),
    ],
)
def test_map_block_mutation_schemas_reject_missing_extra_reordered_and_boundary_content(
    target: str,
    mutate: object,
    match: str,
) -> None:
    fixture = _fixture()
    value = deepcopy(fixture if target == "fixture" else _observation(fixture))
    assert callable(mutate)
    mutate(value)
    schema = FIXTURE_SCHEMA if target == "fixture" else OBSERVATION_SCHEMA
    with pytest.raises(ValueError, match=match):
        validate_json(value, schema, owner="map block mutation mutation test")


def test_map_block_mutation_instrumentation_is_slice_owned_and_span_guarded(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    fixture = _fixture()
    original = ROM.read_bytes()
    monkeypatch.setattr(map_block_mutation, "DERIVED_ROOT", tmp_path)
    output = _instrument_rom(ROM, fixture)
    assert output == tmp_path / "map-block-mutation.instrumented.bin"
    assert ROM.read_bytes() == original
    assert output.read_bytes()[292114:292120] == bytes.fromhex("4EB90000FF88")

    bad_call = deepcopy(fixture)
    bad_call["instrumentation"]["callSitePatchedHex"] = "4EB90000FF00"
    with pytest.raises(ValueError, match="call shape"):
        _instrument_rom(ROM, bad_call)
    too_small = deepcopy(fixture)
    too_small["instrumentation"]["stubOriginalHex"] = "FFFFFFFFFFFFFFFF"
    with pytest.raises(ValueError, match="exceeds verified padding"):
        _instrument_rom(ROM, too_small)


def test_map_block_mutation_lua_observer_has_one_launch_boundary_and_valid_syntax() -> None:
    source = OBSERVER.read_text(encoding="utf-8")
    assert source.count("memorysavestate.savecorestate") == 1
    assert "memory.write_u16_be(layout_address(record.coordinate)" in source
    assert "observe_copy_instruction" in source
    assert "observe_update_bit" in source
    assert "config.constants.layoutWordColumnCount" in source
    assert "runtimeGolden" not in source
    assert "json.write(config.outputPath" in source
    assert "observe_update_bit(0)" not in source
    assert "setBlocksUpdateBit0Address" not in source
    assert "for _,use_site in ipairs(config.updateBitUseSites)" in source
    assert "config.function" not in source
    assert 'config["function"]' in source
    assert "timeout:frame-budget-exhausted" in source
    _, executable = bizhawk_contract()
    validate_lua_syntax(OBSERVER, executable)
