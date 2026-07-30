from __future__ import annotations

from copy import deepcopy
from pathlib import Path

import pytest

import sf2tool.h3.map_entity_placement as entity_placement
from sf2tool.h2.map_script_engine import build_map_script_engine_contract
from sf2tool.h3.bizhawk import bizhawk_contract, validate_lua_syntax
from sf2tool.h3.map_entity_placement import (
    FIXTURE,
    FIXTURE_SCHEMA,
    OBSERVATION_SCHEMA,
    OBSERVER,
    _h1_instruction_address,
    _h1_ordered_direct_call_addresses,
    _require_ordered_source_use_sites,
    build_map_entity_placement_contract,
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


def _mutate_destination_x_delta_width(facts: dict[str, object]) -> None:
    handler = facts["handlers"][3]
    for statements in (
        handler["guardedStatements"],
        handler["sectionGuard"]["orderedInstructions"],
    ):
        index = statements.index("sub.w (a5),d1")
        statements[index] = "sub.b (a5),d1"


@pytest.fixture(scope="module")
def h2_contract() -> dict[str, object]:
    return build_map_script_engine_contract(ROM, UPSTREAM)


def test_map_entity_placement_contract_fixture_and_complete_case_matrix() -> None:
    fixture = _fixture()
    validate_json(fixture, FIXTURE_SCHEMA, owner="map entity placement fixture")
    static = build_map_entity_placement_contract(ROM, UPSTREAM)
    assert static == {
        field: fixture[field]
        for field in ("function", "ram", "constants", "sourceUseSites", "sourceFacts")
    }
    assert derive_case_expectations(static, fixture) == [
        case["expected"] for case in fixture["cases"]
    ]
    assert [case["id"] for case in fixture["cases"]] == [
        "set-position-alive",
        "set-position-dead",
        "set-facing-alive",
        "set-facing-dead",
        "set-position-flash-alive",
        "set-destination-positive-x-negative-y-wait",
        "set-destination-negative-x-positive-y-bypass",
    ]
    assert static["constants"] == {
        "mapTileSize": 384,
        "entityRecordByteCount": 32,
        "currentHpByteOffset": 14,
        "combatantEntryByteCount": 56,
        "combatantMaskAll": 255,
        "setPositionDeadCursorAdjustmentByteCount": 4,
        "setFacingDeadCursorAdjustmentByteCount": 2,
        "setPositionFlashLoopIterationCount": 31,
        "waitBypassMask": 32768,
        "destinationVelocityMagnitude": 32,
        "destinationVelocityNegativeWord": 65504,
        "destinationVelocityTransferByteCount": 2,
        "destinationDeltaTransferByteCount": 2,
        "destinationDeltaSignBit": 32768,
        "destinationDeltaMask": 65535,
        "currentHpSeedTransferByteCount": 2,
        "storedCoordinateTransferByteCount": 2,
        "handlerOperandAdvanceByteCounts": {
            "setPosition": 4,
            "setPositionFlash": 4,
            "setFacing": 2,
            "setDestination": 6,
        },
        "entityFieldLayouts": {
            "xWord": {"byteOffset": 0, "transferByteCount": 2},
            "yWord": {"byteOffset": 2, "transferByteCount": 2},
            "xVelocityWord": {"byteOffset": 4, "transferByteCount": 2},
            "yVelocityWord": {"byteOffset": 6, "transferByteCount": 2},
            "xTravelWord": {"byteOffset": 8, "transferByteCount": 2},
            "yTravelWord": {"byteOffset": 10, "transferByteCount": 2},
            "xDestWord": {"byteOffset": 12, "transferByteCount": 2},
            "yDestWord": {"byteOffset": 14, "transferByteCount": 2},
            "facingByte": {"byteOffset": 16, "transferByteCount": 1},
        },
        "destinationInputCursorUseSites": [
            {"destinationOperand": "d0", "scriptInputByteOffset": 0, "transferredByteCount": 2},
            {"destinationOperand": "d1", "scriptInputByteOffset": 2, "transferredByteCount": 2},
            {"destinationOperand": "d2", "scriptInputByteOffset": 4, "transferredByteCount": 2},
        ],
    }
    assert fixture["runtimeQuestions"] == [
        "map-script-entity-placement/normal-story-reachability",
        "map-script-entity-placement/full-animation-visibility-presentation",
        "map-script-entity-placement/collision-pathfinding-persistence",
    ]
    assert "h2SectionGuard" not in repr(fixture)
    flash = fixture["cases"][4]
    assert flash["runtimeGolden"]["callbackOrder"] == (
        ["getEntityAddressForFlash"]
        + ["waitForVInt", "waitForVInt", "sleep"] * 31
        + [
            "sharedTail",
            "adjustScriptPointer",
            "getEntityAddressFromSharedTail",
            "updateEntitySpriteFromSharedTail",
        ]
    )


@pytest.mark.parametrize(
    ("name", "mutation"),
    [
        ("missing-nested-field", lambda value: value["cases"][4]["expected"].pop("xDestWordAfter")),
        (
            "renamed-nested-field",
            lambda value: value["sourceFacts"]["handlers"][0].update(
                {"handler_address": value["sourceFacts"]["handlers"][0].pop("handlerAddress")}
            ),
        ),
        (
            "extra-nested-field",
            lambda value: value["cases"][6]["entityStateSeed"].update({"unexpected": 1}),
        ),
        ("reordered-cases", lambda value: value["cases"].reverse()),
        ("word-boundary", lambda value: value["cases"][6].__setitem__("selectorWord", 32767)),
    ],
)
def test_map_entity_placement_fixture_schema_rejects_complete_mutations(
    name: str, mutation: object
) -> None:
    del name
    mutated = deepcopy(_fixture())
    assert callable(mutation)
    mutation(mutated)
    with pytest.raises(ValueError, match="fixture failed schema validation"):
        validate_json(mutated, FIXTURE_SCHEMA, owner="map entity placement fixture")


@pytest.mark.parametrize(
    ("name", "mutation"),
    [
        ("missing-nested-field", lambda value: value["records"][4].pop("sharedTailObserved")),
        (
            "renamed-nested-field",
            lambda value: value["records"][6].update(
                {"wait_bypassed": value["records"][6].pop("waitBypassed")}
            ),
        ),
        ("extra-nested-field", lambda value: value["records"][0].update({"unexpected": 1})),
        ("reordered-records", lambda value: value["records"].reverse()),
        (
            "exact-flash-order",
            lambda value: value["records"][4]["callbackOrder"].__setitem__(1, "sleep"),
        ),
    ],
)
def test_map_entity_placement_observation_schema_rejects_complete_mutations(
    name: str, mutation: object
) -> None:
    del name
    mutated = _observation()
    assert callable(mutation)
    mutation(mutated)
    with pytest.raises(ValueError, match="observation failed schema validation"):
        validate_json(mutated, OBSERVATION_SCHEMA, owner="map entity placement observation")


def test_map_entity_placement_observation_schema_accepts_complete_semantic_object() -> None:
    validate_json(_observation(), OBSERVATION_SCHEMA, owner="map entity placement observation")


@pytest.mark.parametrize(
    ("name", "mutate", "message"),
    [
        (
            "cursor-width",
            lambda facts: facts["handlers"][0]["sectionGuard"]["scriptCursorReadUseSites"][
                1
            ].update({"transferredByteCount": 2}),
            "setPos cursor width",
        ),
        (
            "field-use-site",
            lambda facts: facts["handlers"][0]["sectionGuard"]["sourceStateWrites"][1].update(
                {"sourceOperand": "ENTITYDEF_OFFSET_YDEST(a5)"}
            ),
            "setPos write field use-site",
        ),
        (
            "field-transfer-width-use-site",
            lambda facts: facts["handlers"][2]["sectionGuard"]["sourceStateWrites"][0].update(
                {"instruction": "move.w (a6)+,ENTITYDEF_OFFSET_FACING(a5)"}
            ),
            "cross-handler field offset drift",
        ),
        (
            "destination-velocity-literal-use-site",
            lambda facts: facts["handlers"][3]["sectionGuard"]["sourceLiteralUseSites"][2].update(
                {"literalText": "31", "value": 31, "instruction": "move.w #31,d3"}
            ),
            "destination velocity use-site",
        ),
        (
            "map-tile-size",
            lambda facts: facts["handlers"][3]["sectionGuard"]["sourceConstantUses"][0].update(
                {"value": 385}
            ),
            "setDest MAP_TILE_SIZE use-site",
        ),
        (
            "branch-polarity",
            lambda facts: facts["handlers"][3]["sectionGuard"]["branchRecords"][0].update(
                {"branchInstruction": "bne.s loc_46DC4"}
            ),
            "setDest branch polarity/order",
        ),
        (
            "destination-delta-width-use-site",
            _mutate_destination_x_delta_width,
            "H1 instruction identity drift",
        ),
        (
            "wait-bit-mask",
            lambda facts: facts["handlers"][3]["sectionGuard"]["sourceLiteralUseSites"][4].update(
                {"literalText": "$E", "value": 14, "instruction": "btst #$E,d6"}
            ),
            "wait-bypass bit/word relation",
        ),
        (
            "shared-tail-target",
            lambda facts: facts["handlers"][1]["sectionGuard"]["sharedTail"].update(
                {"targetHandler": "csc23_setEntityFacing"}
            ),
            "shared-tail target",
        ),
    ],
)
def test_map_entity_placement_h2_use_site_mutations_fail_before_fixture_comparison(
    monkeypatch: pytest.MonkeyPatch,
    h2_contract: dict[str, object],
    name: str,
    mutate: object,
    message: str,
) -> None:
    del name
    mutated = deepcopy(h2_contract)
    assert callable(mutate)
    mutate(mutated["entityPlacementCommandFacts"])
    monkeypatch.setattr(entity_placement, "build_map_script_engine_contract", lambda *_: mutated)
    with pytest.raises(ValueError, match=message):
        build_map_entity_placement_contract(ROM, UPSTREAM)


def test_map_entity_placement_runtime_source_use_site_mutations_fail_before_fixture_comparison(
) -> None:
    source = (UPSTREAM / "disasm/code/common/scripting/map/mapscriptengine_1.asm").read_text(
        encoding="utf-8"
    )
    start = source.index("csc29_setEntityDest:")
    end = source.index("; End of function csc29_setEntityDest", start)
    destination_section = source[start:end]
    changed_destination = destination_section.replace("neg.w   d2", "neg.w   d1", 1)
    assert changed_destination != destination_section
    with pytest.raises(ValueError, match="runtime source relation drift"):
        _require_ordered_source_use_sites(
            source[:start] + changed_destination + source[end:],
            "csc29_setEntityDest",
            ("neg.w d2",),
        )
    flash_start = source.index("csc17_setEntityPosAndFacingWithFlash:")
    flash_end = source.index("; End of function csc17_setEntityPosAndFacingWithFlash", flash_start)
    flash_section = source[flash_start:flash_end]
    changed_flash = flash_section.replace("bra.w   csc19_setEntityPosAndFacing", "rts", 1)
    assert changed_flash != flash_section
    with pytest.raises(ValueError, match="runtime source relation drift"):
        _require_ordered_source_use_sites(
            source[:flash_start] + changed_flash + source[flash_end:],
            "csc17_setEntityPosAndFacingWithFlash",
            ("bra.w csc19_setEntityPosAndFacing",),
        )
    changed_velocity = destination_section.replace("move.w  #32,d3", "move.w  #31,d3", 1)
    assert changed_velocity != destination_section
    with pytest.raises(ValueError, match="runtime source relation drift"):
        _require_ordered_source_use_sites(
            source[:start] + changed_velocity + source[end:],
            "csc29_setEntityDest",
            ("move.w #32,d3", "neg.w d3", "move.w #32,d3", "neg.w d3"),
        )


def test_map_entity_placement_source_use_site_parser_scopes_sections_and_strips_comments() -> None:
    source = (
        "otherHandler:\n"
        " move.w d0,d1\n"
        "; End of function otherHandler\n"
        "targetHandler:\n"
        " ; move.w d0,d1\n"
        " move.w   d0,d1 ; relevant use site\n"
        " bsr.s   NextCallback\n"
        "; End of function targetHandler\n"
        "afterTarget:\n"
        " bsr.s NextCallback\n"
        "; End of function afterTarget\n"
    )
    assert _require_ordered_source_use_sites(
        source, "targetHandler", ("move.w d0,d1", "bsr.s NextCallback")
    ) == [
        {"instruction": "move.w d0,d1", "sourceLine": 6},
        {"instruction": "bsr.s NextCallback", "sourceLine": 7},
    ]
    comment_only_target = source.replace(" bsr.s   NextCallback\n", " ; bsr.s NextCallback\n", 1)
    with pytest.raises(ValueError, match="runtime source relation drift"):
        _require_ordered_source_use_sites(
            comment_only_target, "targetHandler", ("bsr.s NextCallback",)
        )


def test_map_entity_placement_h1_parser_excludes_comments_and_accepts_instruction_suffixes(
) -> None:
    listing = (
        "00000000 testHandler:\n"
        "00000000 4EB8 1234  jsr     (FirstCallback).w ; source callback\n"
        "00000004              ; jsr (FalseCallback).w\n"
        "00000004 303C 0000  move.w  #FirstCallback,d0\n"
        "00000004 6100 0000  bsr.w   SecondCallback\n"
        "; End of function testHandler\n"
    )
    assert _h1_ordered_direct_call_addresses(
        listing, "testHandler", ["jsr (FirstCallback).w", "bsr.w SecondCallback"]
    ) == [0, 4]
    assert _h1_instruction_address(listing, "testHandler", "bsr.w SecondCallback") == 4
    with pytest.raises(ValueError, match="direct-call order drift"):
        _h1_ordered_direct_call_addresses(
            listing, "testHandler", ["jsr (FirstCallback).w", "jsr (FalseCallback).w"]
        )


def test_map_entity_placement_lua_syntax_preflight() -> None:
    _, executable = bizhawk_contract()
    validate_lua_syntax(OBSERVER, executable)
