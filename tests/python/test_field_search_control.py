from __future__ import annotations

import hashlib
import json
import re
from copy import deepcopy
from pathlib import Path
from typing import Any

import pytest

import sf2tool.h2.field_search_control as field_search
from sf2tool.h2.map_event_flag_lifecycle_state import (
    _remove_map_event_flag_lifecycle_state_later_owner_index_delta,
)
from sf2tool.h2.map_event_scripted_transition_state import (
    _remove_map_event_scripted_transition_state_later_owner_index_delta,
)
from sf2tool.h2.map_event_tactical_base_quote_state import (
    normalize_map_event_tactical_base_quote_state_later_owner_index as _normalize_later_owner_index,
)
from sf2tool.jsonio import load_json as _load_json
from sf2tool.jsonio import validate_json
from sf2tool.paths import repo_path

UPSTREAM = repo_path("local/upstream/SF2DISASM")
ROM = repo_path("local/roms/sf2-us.bin")
FIXTURE = field_search.FIXTURE
SCHEMA = field_search.SCHEMA
INDEX = repo_path("manifests/research-index.json")
INDEX_SCHEMA = repo_path("schemas/research-index.schema.json")
_PRE_SLICE_INDEX_SHA256 = "841468CA1B4A75120BFEE0A363254839B019E23C0A704DFA2B873574E2033457"
_DIRECT_STATE_FIXTURE_ID = "sf2-map-event-direct-state-static-v1"
_DIRECT_STATE_VERIFIER = "src/sf2tool/h2/map_event_direct_state.py"
_DIRECT_STATE_DOCUMENT = "docs/research/map-event-direct-state.md"
_DIRECT_CONTROL_FIXTURE_ID = "sf2-map-event-direct-control-static-v1"
_DIRECT_CONTROL_DOCUMENT = "docs/research/map-event-direct-control.md"
_HANDOFF_FIXTURE_ID = "sf2-map-event-direct-handoff-static-v1"
_HANDOFF_DOCUMENT = "docs/research/map-event-direct-handoff.md"
_PREDICATE_FIXTURE_ID = "sf2-map-event-predicate-results-static-v1"
_PREDICATE_DOCUMENT = "docs/research/map-event-predicate-results.md"
_DIALOGUE_STATE_FIXTURE_ID = "sf2-map-event-dialogue-state-static-v1"
_DIALOGUE_STATE_DOCUMENT = "docs/research/map-event-dialogue-state.md"
_REQUEST_STATE_FIXTURE_ID = "sf2-map-event-request-state-static-v1"
_REQUEST_STATE_DOCUMENT = "docs/research/map-event-request-state.md"


def normalize_later_owner_index(index):
    return _normalize_later_owner_index(
        _remove_map_event_scripted_transition_state_later_owner_index_delta(
            _remove_map_event_flag_lifecycle_state_later_owner_index_delta(index)
        )
    )


def load_json(path):
    value = _load_json(path)
    return normalize_later_owner_index(value) if path == INDEX else value
_DIALOGUE_STATE_OWNER_IDS = {
    "map.data.ms-map3-flag506-entityevents",
    "map.data.ms-map3-zoneevents",
    "map.data.ms-map5-flag530-entityevents",
    "map.data.ms-map5-flag650-entityevents",
    "map.data.ms-map6-flag701-entityevents",
    "map.data.ms-map16-flag530-entityevents",
    "map.data.ms-map18-entityevents",
    "map.data.ms-map19-flag506-entityevents",
    "map.data.ms-map20-flag543-zoneevents",
    "map.data.ms-map21-flag506-entityevents",
    "map.data.ms-map25-entityevents",
    "map.data.ms-map37-section5",
    "map.data.ms-map40-entityevents",
    "map.data.ms-map44-flag507-entityevents",
    "map.data.ms-map63-entityevents",
    "map.data.ms-map72-zoneevents",
    "map.data.ms-map77-section5",
}
_DIRECT_STATE_OWNER_IDS = {
    "map.data.ms-map2-entityevents", "map.data.ms-map3-flag506-entityevents",
    "map.data.ms-map3-flag609-entityevents", "map.data.ms-map5-flag530-entityevents",
    "map.data.ms-map5-flag650-entityevents", "map.data.ms-map6-flag701-entityevents",
    "map.data.ms-map8-entityevents", "map.data.ms-map9-entityevents",
    "map.data.ms-map10-entityevents", "map.data.ms-map13-entityevents",
    "map.data.ms-map13-flag513-entityevents", "map.data.ms-map15-entityevents",
    "map.data.ms-map16-entityevents", "map.data.ms-map16-flag530-entityevents",
    "map.data.ms-map18-entityevents", "map.data.ms-map19-flag506-entityevents",
    "map.data.ms-map21-flag506-entityevents", "map.data.ms-map25-entityevents",
    "map.data.ms-map29-entityevents", "map.data.ms-map31-flag830-entityevents",
    "map.data.ms-map38-entityevents", "map.data.ms-map40-entityevents",
    "map.data.ms-map44-flag507-entityevents", "map.data.ms-map63-entityevents",
    "map.data.ms-map3-zoneevents", "map.data.ms-map16-zoneevents",
    "map.data.ms-map20-flag543-zoneevents", "map.data.ms-map22-zoneevents",
    "map.data.ms-map28-zoneevents", "map.data.ms-map66-zoneevents",
    "map.data.ms-map69-zoneevents", "map.data.ms-map70-zoneevents",
    "map.data.ms-map72-zoneevents", "map.data.ms-map74-zoneevents",
    "map.data.ms-map76-zoneevents", "map.data.ms-map77-zoneevents",
    "map.data.ms-map37-section5", "map.data.ms-map77-section5",
}


def _without_request_state(index):
    normalized = deepcopy(index)
    removed: set[str] = set()
    for record in normalized["records"]:
        evidence = [
            item for item in record["evidence"] if item["fixtureId"] == _REQUEST_STATE_FIXTURE_ID
        ]
        if not evidence:
            continue
        assert evidence == [
            {
                "level": "H2",
                "fixture": "tests/fixtures/h2/map-event-request-state-static-v1.json",
                "fixtureId": _REQUEST_STATE_FIXTURE_ID,
                "verifier": "src/sf2tool/h2/map_event_request_state.py",
                "bindings": [
                    {
                        "addressId": "entry",
                        "fixtureField": (
                            "eventRequestState.sourceFiles."
                            f"{record['symbol']}.tableEntryAddress"
                        ),
                    }
                ],
            }
        ]
        assert record["documents"][-1] == _REQUEST_STATE_DOCUMENT
        record["evidence"].remove(evidence[0])
        record["documents"].pop()
        removed.add(record["id"])
    assert len(removed) == 24
    return normalized


def _without_request_consumption(index: dict[str, Any]) -> dict[str, Any]:
    for record in index["records"]:
        evidence = [
            item
            for item in record["evidence"]
            if item["fixtureId"] == "sf2-map-event-request-consumption-static-v1"
        ]
        if not evidence:
            continue
        assert len(evidence) == 1
        assert record["documents"].count("docs/research/map-event-request-consumption.md") == 1
        record["evidence"] = [item for item in record["evidence"] if item not in evidence]
        record["documents"].remove("docs/research/map-event-request-consumption.md")
        record["addresses"] = [
            address
            for address in record["addresses"]
            if address["id"]
            not in {
                "get-shop-inventory-address",
                "process-map-event",
                "declare-raft-entity",
                "raft-refresh",
            }
        ]
    return index

def _without_dialogue_state(index: dict[str, Any]) -> dict[str, Any]:
    normalized = deepcopy(index)
    removed: set[str] = set()
    for record in normalized["records"]:
        evidence = [
            item for item in record["evidence"] if item["fixtureId"] == _DIALOGUE_STATE_FIXTURE_ID
        ]
        if not evidence:
            continue
        assert record["id"] in _DIALOGUE_STATE_OWNER_IDS
        assert evidence == [
            {
                "level": "H2",
                "fixture": "tests/fixtures/h2/map-event-dialogue-state-static-v1.json",
                "fixtureId": _DIALOGUE_STATE_FIXTURE_ID,
                "verifier": "src/sf2tool/h2/map_event_dialogue_state.py",
                "bindings": [
                    {
                        "addressId": "entry",
                        "fixtureField": (
                            "eventDialogueState.sourceFiles."
                            f"{record['symbol']}.tableEntryAddress"
                        ),
                    }
                ],
            }
        ]
        assert record["documents"][-1] == _DIALOGUE_STATE_DOCUMENT
        record["evidence"].remove(evidence[0])
        record["documents"].pop()
        removed.add(record["id"])
    assert removed == _DIALOGUE_STATE_OWNER_IDS
    return normalized


def _without_predicate_results(index: dict[str, Any]) -> dict[str, Any]:
    normalized = deepcopy(index)
    removed: set[str] = set()
    for record in normalized["records"]:
        evidence = [
            item for item in record["evidence"] if item["fixtureId"] == _PREDICATE_FIXTURE_ID
        ]
        if not evidence:
            continue
        assert evidence == [
            {
                "level": "H2",
                "fixture": "tests/fixtures/h2/map-event-predicate-results-static-v1.json",
                "fixtureId": _PREDICATE_FIXTURE_ID,
                "verifier": "src/sf2tool/h2/map_event_predicate_results.py",
                "bindings": [
                    {
                        "addressId": "entry",
                        "fixtureField": (
                            "eventPredicateResults.sourceFiles."
                            f"{record['symbol']}.tableEntryAddress"
                        ),
                    }
                ],
            }
        ]
        assert record["documents"][-1] == _PREDICATE_DOCUMENT
        record["evidence"].remove(evidence[0])
        record["documents"].pop()
        removed.add(record["id"])
    assert len(removed) == 15
    return normalized


def _canonical_sha256(value: object) -> str:
    return (
        hashlib.sha256(json.dumps(value, sort_keys=True, separators=(",", ":")).encode("utf-8"))
        .hexdigest()
        .upper()
    )


def _assert_recursively_closed(schema: dict[str, Any], node: dict[str, Any]) -> None:
    if "$ref" in node:
        reference = node["$ref"]
        assert reference.startswith("#/$defs/")
        _assert_recursively_closed(schema, schema["$defs"][reference.rsplit("/", 1)[-1]])
    for child in node.get("allOf", []):
        _assert_recursively_closed(schema, child)
    if node.get("type") == "object":
        assert node["additionalProperties"] is False
        for child in node.get("properties", {}).values():
            _assert_recursively_closed(schema, child)
    if node.get("type") == "array" and isinstance(node.get("items"), dict):
        _assert_recursively_closed(schema, node["items"])


def test_field_search_fixture_is_canonical_recursively_closed_and_public_safe() -> None:
    fixture = load_json(FIXTURE)
    schema = load_json(SCHEMA)

    assert FIXTURE.read_bytes() == field_search.canonical_json_bytes(fixture)
    validate_json(fixture, SCHEMA, owner="field-search fixture")
    _assert_recursively_closed(schema, schema["$defs"]["fixture"])
    assert set(fixture) == {
        "schemaVersion",
        "id",
        "system",
        "romSha256",
        "upstream",
        "sourceContext",
        "retainedOwners",
        "fieldSearchSpine",
        "unknowns",
        "summary",
    }
    assert set(fixture["fieldSearchSpine"]) == {
        "callers",
        "functionAddresses",
        "targetCoordinate",
        "blockDispatch",
        "areaDescriptionFallback",
        "contentClassification",
        "goldPath",
        "itemRecipientPath",
        "fullInventoryRollback",
        "returnContract",
        "publicTextIds",
    }
    assert fixture["summary"] == {
        "sourceFiles": 17,
        "h1RomAnchors": 22,
        "callers": 2,
        "unknowns": 14,
    }
    assert field_search._UNKNOWN_KEYS == (
        "natural-search-reachability",
        "actual-caller-entry-state",
        "actual-view-target-entity",
        "actual-facing-and-target-coordinate",
        "actual-block-kind",
        "actual-area-description-row-or-callback",
        "actual-chest-or-nonchest-content",
        "actual-gold-before-and-after",
        "actual-item-recipient-and-capacity",
        "actual-item-flag-open-close-refill-state",
        "actual-return-code-and-caller-branch",
        "input-text-sound-and-fade-cadence",
        "persistence-after-map-switch-save-load",
        "route-specific-search-outcome",
    )
    assert fixture["unknowns"] == {key: "Unknown" for key in field_search._UNKNOWN_KEYS}
    assert len(fixture["sourceContext"]["sourceIdentities"]) == 17
    assert len(fixture["sourceContext"]["h1RomAnchors"]) == 22
    assert fixture["fieldSearchSpine"]["publicTextIds"] == [
        403,
        408,
        404,
        409,
        405,
        410,
        427,
        412,
        423,
        412,
        423,
        412,
        414,
        413,
        415,
        416,
        417,
    ]
    serialized = json.dumps(fixture, sort_keys=True).lower()
    for forbidden in ("local/", "capture", "savestate", "movie", "bizhawk", "lua"):
        assert forbidden not in serialized


def test_field_search_fixture_exact_static_semantics() -> None:
    spine = load_json(FIXTURE)["fieldSearchSpine"]
    assert spine["functionAddresses"] == field_search._FUNCTION_ADDRESSES
    assert spine["callers"] == {
        "fieldMenu": {
            "callAddress": 137694,
            "returnAddress": 137700,
            "instructionTarget": "j_CheckArea",
            "effectiveTarget": "CheckArea",
            "d6Value": 0,
            "returnTarget": "ExitMain",
        },
        "processPlayerActionNoEntity": {
            "callAddress": 154562,
            "returnAddress": 154568,
            "instructionTarget": "CheckArea",
            "effectiveTarget": "CheckArea",
            "d6Value": 1,
            "nonzeroBranch": "return_25BF2",
        },
    }
    assert spine["targetCoordinate"] == {
        "viewTargetEntity": "VIEW_TARGET_ENTITY",
        "negativeBranch": "bpl",
        "negativeReturn": "rts",
        "entityScaleShift": "ENTITYDEF_SIZE_BITS",
        "directionMask": 3,
        "directionOffsetIndexShift": 2,
        "pixelOffsetTable": "table_PixelOffsets_X",
        "tileSize": 384,
        "layoutRowWidth": 64,
        "layoutWordScale": 2,
        "layoutBase": "FF0000_RAM_START",
        "blockMask": 0x3C00,
    }
    assert spine["blockDispatch"] == {
        "itemEntryMask": 127,
        "itemNothing": 127,
        "kinds": [
            {
                "kind": "chest",
                "mask": 0x1800,
                "contentTarget": "OpenChest",
                "actionTextId": 403,
                "emptyTextId": 408,
            },
            {
                "kind": "vase",
                "mask": 0x2C00,
                "contentTarget": "CheckNonChestItem",
                "actionTextId": 404,
                "emptyTextId": 409,
            },
            {
                "kind": "barrel",
                "mask": 0x3000,
                "contentTarget": "CheckNonChestItem",
                "actionTextId": 405,
                "emptyTextId": 410,
            },
            {
                "kind": "bookshelf",
                "mask": 0x3400,
                "contentTarget": "CheckNonChestItem",
                "actionTextId": 427,
                "emptyTextId": 412,
            },
            {
                "kind": "genericSearchable",
                "mask": 0x1C00,
                "contentTarget": "CheckNonChestItem",
                "actionTextId": 423,
                "emptyTextId": 412,
            },
        ],
    }
    assert spine["areaDescriptionFallback"] == {
        "instructionTarget": "j_RunMapSetupAreaDescription",
        "effectiveTarget": "RunMapSetupAreaDescription",
        "handledBranch": "bne",
        "d6Test": "tst",
        "d6OneReturn": 0,
        "d6ZeroDefaultTextIds": [423, 412],
        "defaultReturn": -1,
        "closeText": "clsTxt",
    }
    assert spine["contentClassification"] == {
        "goldChestStart": 128,
        "goldBranch": "bge",
        "itemBranch": "blt",
        "itemEntryMask": 127,
        "itemNothing": 127,
    }
    assert spine["goldPath"] == {
        "functionAddress": 145820,
        "tableAddress": 145838,
        "indexTransform": [
            "subtractGoldChestStart",
            "maskItemEntryIndex",
            "doubleWordIndex",
        ],
        "tableValues": list(range(10, 131, 10)),
        "increaseGoldInstructionTarget": "j_IncreaseGold",
        "increaseGoldEffectiveTarget": "IncreaseGold",
        "callOrder": [
            "GetChestGoldAmount",
            "IncreaseGold",
            "MUSIC_ITEM",
            "txt414",
            "FadeOut_WaitForP1Input",
        ],
    }
    assert spine["itemRecipientPath"] == {
        "leaderIndex": 0,
        "inventoryCapacity": 4,
        "leaderFirst": True,
        "forceUpdateInstructionTarget": "j_UpdateForce",
        "forceUpdateEffectiveTarget": "UpdateForce",
        "recipientList": "OTHER_FORCE_MEMBERS_LIST",
        "counter": "TARGETS_LIST_LENGTH-2",
        "loop": "DBF",
        "firstEligibleAction": "AddItem",
        "textIds": [413, 415, 416],
    }
    assert spine["fullInventoryRollback"] == {
        "textId": 417,
        "callOrder": ["CloseChest", "RefillNonChestItem"],
        "returnTarget": "byte_23994",
    }
    assert spine["returnContract"] == {
        "negativeViewTarget": "rts",
        "areaDescriptionHandled": -1,
        "areaDescriptionUnhandledD6One": 0,
        "areaDescriptionUnhandledD6Zero": -1,
        "processPlayerActionNonzeroBranch": "return_25BF2",
    }


@pytest.mark.parametrize(
    ("path", "mutator"),
    (
        (
            "sourceContext.unexpected",
            lambda value: value["sourceContext"].update({"unexpected": 1}),
        ),
        (
            "fieldSearchSpine.unexpected",
            lambda value: value["fieldSearchSpine"].update({"unexpected": 1}),
        ),
        ("unknowns.missing", lambda value: value["unknowns"].pop(field_search._UNKNOWN_KEYS[-1])),
        (
            "gold.table",
            lambda value: value["fieldSearchSpine"]["goldPath"]["tableValues"].__setitem__(0, 11),
        ),
        ("public.text", lambda value: value["fieldSearchSpine"]["publicTextIds"].reverse()),
    ),
)
def test_field_search_schema_rejects_extra_or_semantic_drift(path: str, mutator: Any) -> None:
    broken = deepcopy(load_json(FIXTURE))
    mutator(broken)
    with pytest.raises(ValueError, match="schema"):
        validate_json(broken, SCHEMA, owner=path)


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_field_search_source_contract_parses_the_complete_assigned_surface() -> None:
    source, identities = field_search._read_source_surface(UPSTREAM / "disasm")
    parsed = field_search._validate_source_contract(source)
    fixture = load_json(FIXTURE)

    assert len(identities) == 17
    assert parsed["sourceContext"] == {
        "layoutIncludes": [
            "code\\gameflow\\exploration\\explorationfunctions_0.asm",
            "data\\stats\\items\\chestgoldamounts.asm",
            "code\\gameflow\\exploration\\explorationfunctions_1.asm",
        ],
        "callerInventory": {
            "instructionTargetSiteCounts": {"j_CheckArea": 1, "CheckArea": 1},
            "effectiveTargetSiteCounts": {"CheckArea": 2},
        },
    }
    assert parsed["fieldSearchSpine"] == fixture["fieldSearchSpine"]


def test_field_search_direct_call_parser_rejects_comments_labels_and_near_misses() -> None:
    source = """label: jsr j_CheckArea
; jsr CheckArea
jsr.w (j_CheckArea).l
bsr.s CheckArea
jsr CheckArea trailing
dc.l CheckArea
"""
    assert field_search._direct_calls(source) == ["j_CheckArea", "CheckArea"]


def _mutated_source(
    source: dict[str, str], path: str, old: str, new: str, occurrence: int = 0
) -> dict[str, str]:
    assert source[path].count(old) > occurrence
    altered = dict(source)
    prefix, found, suffix = altered[path].partition(old)
    for _ in range(occurrence):
        prefix += found
        prefix, found, suffix = suffix.partition(old)
    assert found == old
    altered[path] = prefix + new + suffix
    return altered


def _mutated_region(
    source: dict[str, str], path: str, start: str, end: str, old: str, new: str
) -> dict[str, str]:
    start_at = source[path].find(start)
    end_at = source[path].find(end, start_at + len(start))
    assert start_at >= 0 and end_at >= 0
    region = source[path][start_at:end_at]
    assert region.count(old) == 1
    altered = dict(source)
    altered[path] = source[path][:start_at] + region.replace(old, new) + source[path][end_at:]
    return altered


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("old", "new"),
    (
        ("lea     ((ENTITY_DATA-$1000000)).w,a0", "lea     ((ENTITY_DATA-$1000000)).w,a1"),
        ("adda.w  d0,a0", "adda.w  d0,a1"),
        ("move.w  d2,d0", "move.w  d2,d1"),
        ("move.w  d3,d5", "move.w  d3,d4"),
        ("ext.l   d0", "ext.l   d2"),
        ("ext.l   d1", "ext.l   d2"),
        ("move.w  d0,d4", "move.w  d0,d5"),
        ("move.w  d1,d5", "move.w  d1,d4"),
        ("move.w  d1,d3", "move.w  d1,d2"),
        (
            "move.w  d0,d4\n                move.w  d1,d5",
            "move.w  d1,d5\n                move.w  d0,d4",
        ),
    ),
)
def test_field_search_coordinate_data_connections_are_function_local_and_required(
    old: str, new: str
) -> None:
    source, _ = field_search._read_source_surface(UPSTREAM / "disasm")
    altered = _mutated_region(
        source,
        "code/gameflow/exploration/explorationfunctions_0.asm",
        "CheckArea:",
        "cmpi.w  #$1800,d3",
        old,
        new,
    )
    with pytest.raises(ValueError, match="Field search control"):
        field_search._validate_source_contract(altered)


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("kind", "start", "end"),
    (
        ("chest", "CheckArea:", "loc_238E8:"),
        ("vase", "loc_238E8:", "loc_2390C:"),
        ("barrel", "loc_2390C:", "loc_23930:"),
        ("bookshelf", "loc_23930:", "loc_23954:"),
        ("generic", "loc_23954:", "loc_23978:"),
    ),
)
def test_field_search_each_block_has_local_content_and_empty_exit_branches(
    kind: str, start: str, end: str
) -> None:
    source, _ = field_search._read_source_surface(UPSTREAM / "disasm")
    path = "code/gameflow/exploration/explorationfunctions_0.asm"
    for old, new in (
        ("bne.w   loc_239C8", "beq.w   loc_239C8"),
        ("bra.w   byte_23994", "bra.w   loc_239C8"),
    ):
        with pytest.raises(ValueError, match="Field search control"):
            field_search._validate_source_contract(
                _mutated_region(source, path, start, end, old, new)
            )


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("start", "end", "old", "new"),
    (
        ("loc_239C8:", "loc_239EE:", "bra.s   byte_23994", "bra.s   loc_239EE"),
        (
            "loc_239EE:",
            "loc_23A1E:",
            "move.w  d2,((DIALOGUE_NAME_INDEX_2-$1000000)).w",
            "move.w  d2,((DIALOGUE_NAME_INDEX_3-$1000000)).w",
        ),
        (
            "loc_239EE:",
            "loc_23A1E:",
            "move.w  ((DIALOGUE_NAME_INDEX_2-$1000000)).w,d1",
            "move.w  ((DIALOGUE_NAME_INDEX_2-$1000000)).w,d0",
        ),
        (
            "loc_239EE:",
            "loc_23A1E:",
            "sndCom  MUSIC_ITEM\n                txt     415",
            "txt     415\n                sndCom  MUSIC_ITEM",
        ),
        ("loc_239EE:", "loc_23A1E:", "bra.w   byte_23994", "bra.w   loc_23A1E"),
        (
            "loc_23A32:",
            "loc_23A62:",
            "move.w  ((DIALOGUE_NAME_INDEX_2-$1000000)).w,d1",
            "move.w  ((DIALOGUE_NAME_INDEX_2-$1000000)).w,d0",
        ),
        (
            "loc_23A32:",
            "loc_23A62:",
            "move.w  d0,((DIALOGUE_NAME_INDEX_3-$1000000)).w",
            "move.w  d1,((DIALOGUE_NAME_INDEX_3-$1000000)).w",
        ),
        (
            "loc_23A32:",
            "loc_23A62:",
            "sndCom  MUSIC_ITEM\n                txt     416",
            "txt     416\n                sndCom  MUSIC_ITEM",
        ),
        ("loc_23A32:", "loc_23A62:", "bra.w   byte_23994", "bra.w   loc_23A62"),
        ("loc_23A66:", "END OF FUNCTION CHUNK FOR CheckArea", "move.w  d4,d0", "move.w  d4,d1"),
        ("loc_23A66:", "END OF FUNCTION CHUNK FOR CheckArea", "move.w  d5,d1", "move.w  d5,d0"),
        (
            "loc_23A66:",
            "END OF FUNCTION CHUNK FOR CheckArea",
            "bra.w   byte_23994",
            "bra.w   loc_23A66",
        ),
    ),
)
def test_field_search_handoff_data_and_return_paths_are_function_local_and_required(
    start: str, end: str, old: str, new: str
) -> None:
    source, _ = field_search._read_source_surface(UPSTREAM / "disasm")
    altered = _mutated_region(
        source,
        "code/gameflow/exploration/explorationfunctions_1.asm",
        start,
        end,
        old,
        new,
    )
    with pytest.raises(ValueError, match="Field search control"):
        field_search._validate_source_contract(altered)


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize("relative", field_search._SOURCE_SURFACE)
def test_field_search_every_assigned_source_identity_rejects_a_byte_mutation(
    tmp_path: Path, relative: str
) -> None:
    source_root = UPSTREAM / "disasm"
    for source_path in field_search._SOURCE_SURFACE:
        destination = tmp_path / source_path
        destination.parent.mkdir(parents=True, exist_ok=True)
        destination.write_bytes((source_root / source_path).read_bytes())
    drifted = tmp_path / relative
    data = bytearray(drifted.read_bytes())
    data[0] ^= 1
    drifted.write_bytes(data)
    with pytest.raises(ValueError, match="source hash drift"):
        field_search._read_source_surface(tmp_path)


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("path", "old", "new"),
    (
        ("sf2enums.asm", "MAP_TILE_SIZE: equ 384", "MAP_TILE_SIZE: equ 256"),
        (
            "layout/sf2-05-0x020000-0x028000.asm",
            "explorationfunctions_1.asm",
            "explorationfunctions_x.asm",
        ),
        (
            "code/common/tech/jumpinterfaces/s02_jumpinterface.asm",
            "jmp     IncreaseGold(pc)",
            "jmp     AddItem(pc)",
        ),
        ("code/common/menus/main/mainactions.asm", "clr.w   d6", "moveq   #1,d6"),
        ("code/gameflow/exploration/explorationvints.asm", "moveq   #1,d6", "clr.w   d6"),
        (
            "code/gameflow/exploration/explorationvints.asm",
            "bne.w   return_25BF2",
            "beq.w   return_25BF2",
        ),
        (
            "code/gameflow/exploration/explorationfunctions_0.asm",
            "bpl.s   loc_2386C",
            "bmi.s   loc_2386C",
        ),
        ("code/gameflow/exploration/explorationfunctions_0.asm", "andi.w  #3,d3", "andi.w  #1,d3"),
        (
            "code/gameflow/exploration/explorationfunctions_0.asm",
            "divs.w  #MAP_TILE_SIZE,d0",
            "divs.w  #$100,d0",
        ),
        ("code/gameflow/exploration/explorationfunctions_0.asm", "lsl.w   #6,d3", "lsl.w   #5,d3"),
        (
            "code/gameflow/exploration/explorationfunctions_0.asm",
            "andi.w  #$3C00,d3",
            "andi.w  #$1C00,d3",
        ),
        (
            "code/gameflow/exploration/explorationfunctions_0.asm",
            "cmpi.w  #$1800,d3",
            "cmpi.w  #$1A00,d3",
        ),
        (
            "code/gameflow/exploration/explorationfunctions_0.asm",
            "jsr     (OpenChest).w",
            "jsr     (CheckNonChestItem).w",
        ),
        (
            "code/gameflow/exploration/explorationfunctions_0.asm",
            "cmpi.w  #$2C00,d3",
            "cmpi.w  #$2A00,d3",
        ),
        (
            "code/gameflow/exploration/explorationfunctions_0.asm",
            "cmpi.w  #$3000,d3",
            "cmpi.w  #$3200,d3",
        ),
        (
            "code/gameflow/exploration/explorationfunctions_0.asm",
            "cmpi.w  #$3400,d3",
            "cmpi.w  #$3600,d3",
        ),
        (
            "code/gameflow/exploration/explorationfunctions_0.asm",
            "cmpi.w  #$1C00,d3",
            "cmpi.w  #$1E00,d3",
        ),
        (
            "code/gameflow/exploration/explorationfunctions_0.asm",
            "cmpi.b  #ITEM_NOTHING,d0",
            "cmpi.b  #126,d0",
        ),
        (
            "code/gameflow/exploration/explorationfunctions_0.asm",
            "bne.w   byte_23994",
            "beq.w   byte_23994",
        ),
        (
            "code/gameflow/exploration/explorationfunctions_0.asm",
            "jsr     j_RunMapSetupAreaDescription",
            "jsr     j_CheckArea",
        ),
        (
            "code/gameflow/exploration/explorationfunctions_0.asm",
            "tst.w   d6",
            "tst.w   d5",
        ),
        (
            "code/gameflow/exploration/explorationfunctions_0.asm",
            "beq.s   byte_2398C",
            "bne.s   byte_2398C",
        ),
        (
            "code/gameflow/exploration/explorationfunctions_0.asm",
            "clr.w   d0\n                bra.w   return_2399A",
            "moveq   #-1,d0\n                bra.w   return_2399A",
        ),
        (
            "code/gameflow/exploration/explorationfunctions_1.asm",
            "clr.w   d0\n                move.w  d0,((DIALOGUE_NAME_INDEX_1-$1000000)).w",
            "moveq   #1,d0\n                move.w  d0,((DIALOGUE_NAME_INDEX_1-$1000000)).w",
        ),
        (
            "code/gameflow/exploration/explorationfunctions_1.asm",
            "cmpi.w  #COMBATANT_ITEMSLOTS,d2",
            "cmpi.w  #3,d2",
        ),
        ("code/gameflow/exploration/explorationfunctions_1.asm", "subq.w  #2,d7", "subq.w  #1,d7"),
        (
            "code/gameflow/exploration/explorationfunctions_1.asm",
            "dbf     d7,loc_23A32",
            "dbf     d6,loc_23A32",
        ),
        (
            "code/gameflow/exploration/explorationfunctions_1.asm",
            "jsr     (CloseChest).w",
            "jsr     (RefillNonChestItem).w",
        ),
        (
            "code/gameflow/exploration/explorationfunctions_0.asm",
            "add.w   d2,d2",
            "sub.w   d2,d2",
        ),
        (
            "code/gameflow/exploration/explorationfunctions_0.asm",
            "subi.w  #ITEMINDEX_GOLDCHESTS_START,d2",
            "subi.w  #127,d2",
        ),
        ("code/gameflow/exploration/exploration.asm", "jsr     j_ClearFlag", "jsr     j_SetFlag"),
        (
            "code/gameflow/battle/battlefunctions/battlefunctions_0.asm",
            "jsr     (WaitForPlayerInput).w",
            "jsr     (WaitForVInt).w",
        ),
        ("code/common/scripting/map/mapsetupsfunctions_1.asm", "tst.w   d7", "tst.w   d6"),
        (
            "code/common/stats/gold.asm",
            "add.l   ((CURRENT_GOLD-$1000000)).w,d1",
            "sub.l   ((CURRENT_GOLD-$1000000)).w,d1",
        ),
        (
            "code/common/stats/itemstats.asm",
            "moveq   #COMBATANT_ITEMSLOTS_COUNTER,d3",
            "moveq   #2,d3",
        ),
        ("code/common/stats/battleparty.asm", "TARGETS_LIST_LENGTH", "TARGETS_LIST_COUNT"),
        ("data/stats/items/chestgoldamounts.asm", "dc.w 10", "dc.w 11"),
        (
            "code/common/tech/jumpinterfaces/s05_jumpinterface.asm",
            "jmp     CheckArea(pc)",
            "jmp     OpenChest(pc)",
        ),
        (
            "code/common/tech/jumpinterfaces/s07_jumpinterface.asm",
            "jmp     RunMapSetupAreaDescription(pc)",
            "jmp     CheckArea(pc)",
        ),
    ),
)
def test_field_search_source_use_mutations_are_rejected(path: str, old: str, new: str) -> None:
    source, _ = field_search._read_source_surface(UPSTREAM / "disasm")
    with pytest.raises(ValueError, match="Field search control"):
        field_search._validate_source_contract(_mutated_source(source, path, old, new))


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_field_search_every_public_text_use_site_mutation_is_rejected() -> None:
    source, _ = field_search._read_source_surface(UPSTREAM / "disasm")
    regions = (
        (
            "code/gameflow/exploration/explorationfunctions_0.asm",
            "CheckArea:",
            "End of function CheckArea",
        ),
        (
            "code/gameflow/exploration/explorationfunctions_1.asm",
            "loc_239C8:",
            "END OF FUNCTION CHUNK FOR CheckArea",
        ),
    )
    for path, start, end in regions:
        region = field_search._source_region(source[path], start, end, "test")
        for match in re.finditer(r"(?m)^\s*txt\s+(\d+)\b", region):
            old = match.group(0)
            new = old.replace(match.group(1), str(int(match.group(1)) + 1000))
            occurrence = source[path][: source[path].find(start)].count(old) + region[
                : match.start()
            ].count(old)
            with pytest.raises(ValueError, match="Field search control"):
                field_search._validate_source_contract(
                    _mutated_source(source, path, old, new, occurrence)
                )


@pytest.mark.skipif(not ROM.is_file(), reason="canonical ROM is unavailable")
def test_field_search_every_h1_rom_anchor_rejects_one_byte_drift() -> None:
    rom = ROM.read_bytes()
    assert (
        field_search._anchor_projection(rom, rom)
        == load_json(FIXTURE)["sourceContext"]["h1RomAnchors"]
    )
    for _, address, _ in field_search._ANCHORS:
        drifted = bytearray(rom)
        drifted[address] ^= 1
        with pytest.raises(ValueError, match="H1/ROM anchor drift"):
            field_search._anchor_projection(bytes(drifted), rom)


def test_field_search_retained_owner_digests_are_exact() -> None:
    fixture = load_json(FIXTURE)
    assert fixture["retainedOwners"] == field_search._retained_owners()
    drifted = deepcopy(fixture)
    drifted["retainedOwners"]["fieldMenuControl"]["fixtureSha256"] = "0" * 64
    assert drifted["retainedOwners"] != field_search._retained_owners()


@pytest.mark.skipif(
    not (UPSTREAM / "build/sf2build-h1.bin").is_file() or not ROM.is_file(),
    reason="pinned H1 artifact or canonical ROM is unavailable",
)
def test_field_search_retained_owner_digest_drift_rejects_fixture_comparison(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    drifted = field_search._retained_owners()
    drifted["fieldMenuControl"]["fixtureSha256"] = "0" * 64
    monkeypatch.setattr(field_search, "_retained_owners", lambda: drifted)
    with pytest.raises(ValueError, match="complete semantic fixture drift"):
        field_search.verify_field_search_control_static(ROM, UPSTREAM)


def test_field_search_index_delta_is_exact_and_rejects_unknown_roots() -> None:
    index = _without_request_consumption(
        _without_predicate_results(
            _without_dialogue_state(_without_request_state(load_json(INDEX)))
        )
    )
    records = {record["id"]: record for record in index["records"]}
    expected = {
        (
            "tech.interfaces.jump-s02",
            "increase-gold",
            "fieldSearchSpine.functionAddresses.j_IncreaseGold",
        ),
        (
            "tech.interfaces.jump-s02",
            "get-item-slots",
            "fieldSearchSpine.functionAddresses.j_GetItemBySlotAndHeldItemsNumber",
        ),
        ("tech.interfaces.jump-s02", "add-item", "fieldSearchSpine.functionAddresses.j_AddItem"),
        (
            "tech.interfaces.jump-s02",
            "update-force",
            "fieldSearchSpine.functionAddresses.j_UpdateForce",
        ),
        (
            "tech.interfaces.jump-s05",
            "check-area",
            "fieldSearchSpine.functionAddresses.j_CheckArea",
        ),
        (
            "tech.interfaces.jump-s07",
            "run-area-description",
            "fieldSearchSpine.functionAddresses.j_RunMapSetupAreaDescription",
        ),
        ("menus.field-main", "search-call", "fieldSearchSpine.callers.fieldMenu.callAddress"),
        (
            "gameflow.exploration.actions",
            "check-area-call",
            "fieldSearchSpine.callers.processPlayerActionNoEntity.callAddress",
        ),
        (
            "gameflow.exploration.interaction",
            "check-area",
            "fieldSearchSpine.functionAddresses.CheckArea",
        ),
        (
            "gameflow.exploration.interaction",
            "get-chest-gold",
            "fieldSearchSpine.functionAddresses.GetChestGoldAmount",
        ),
        (
            "gameflow.exploration.item-handoff",
            "entry",
            "fieldSearchSpine.functionAddresses.itemHandoff",
        ),
        (
            "gameflow.exploration.engine",
            "open-chest",
            "fieldSearchSpine.functionAddresses.OpenChest",
        ),
        (
            "gameflow.exploration.engine",
            "close-chest",
            "fieldSearchSpine.functionAddresses.CloseChest",
        ),
        (
            "gameflow.exploration.engine",
            "check-nonchest-item",
            "fieldSearchSpine.functionAddresses.CheckNonChestItem",
        ),
        (
            "gameflow.exploration.engine",
            "refill-nonchest-item",
            "fieldSearchSpine.functionAddresses.RefillNonChestItem",
        ),
        (
            "map.setup.area-description",
            "entry",
            "fieldSearchSpine.functionAddresses.RunMapSetupAreaDescription",
        ),
        ("stats.party", "entry", "fieldSearchSpine.functionAddresses.UpdateForce"),
        ("battle.replay.increase-gold", "entry", "fieldSearchSpine.functionAddresses.IncreaseGold"),
        (
            "stats.item-stats",
            "get-item-slots",
            "fieldSearchSpine.functionAddresses.GetItemBySlotAndHeldItemsNumber",
        ),
        ("stats.item-stats", "add-item", "fieldSearchSpine.functionAddresses.AddItem"),
        ("stats.data.chest-gold", "entry", "fieldSearchSpine.goldPath.tableAddress"),
    }
    actual = {
        (record_id, binding["addressId"], binding["fixtureField"])
        for record_id, record in records.items()
        for evidence in record["evidence"]
        if evidence["fixtureId"] == field_search.ID
        for binding in evidence["bindings"]
    }
    assert actual == expected
    assert len(actual) == 21
    owner_ids = {record_id for record_id, _, _ in expected}
    assert len(owner_ids) == 13
    assert {
        record_id
        for record_id, record in records.items()
        if "docs/research/field-search-control.md" in record["documents"]
    } == owner_ids
    assert all(
        records[record_id]["documents"].count("docs/research/field-search-control.md") == 1
        for record_id in owner_ids
    )
    assert records["menus.field-main"]["documents"][-1] == ("docs/research/field-item-effects.md")
    expected_new_addresses = {
        ("tech.interfaces.jump-s02", "increase-gold", 33116),
        ("tech.interfaces.jump-s02", "get-item-slots", 33140),
        ("tech.interfaces.jump-s02", "add-item", 33176),
        ("tech.interfaces.jump-s02", "update-force", 33392),
        ("tech.interfaces.jump-s05", "check-area", 131148),
        ("tech.interfaces.jump-s07", "run-area-description", 278708),
        ("menus.field-main", "search-call", 137694),
        ("gameflow.exploration.actions", "check-area-call", 154562),
        ("gameflow.exploration.interaction", "get-chest-gold", 145820),
        ("gameflow.exploration.engine", "open-chest", 16726),
        ("gameflow.exploration.engine", "close-chest", 16788),
        ("gameflow.exploration.engine", "check-nonchest-item", 16886),
        ("gameflow.exploration.engine", "refill-nonchest-item", 16922),
        ("stats.item-stats", "get-item-slots", 35834),
        ("stats.item-stats", "add-item", 36002),
    }
    actual_addresses = {
        (record_id, address["id"], address["value"])
        for record_id, record in records.items()
        for address in record["addresses"]
        if (record_id, address["id"]) in {(item[0], item[1]) for item in expected_new_addresses}
    }
    assert actual_addresses == expected_new_addresses

    normalized = deepcopy(index)
    removed_evidence: set[str] = set()
    removed_addresses: set[tuple[str, str, int]] = set()
    removed_documents: set[str] = set()
    removed_direct_state_evidence: set[str] = set()
    removed_direct_state_documents: set[str] = set()
    removed_direct_control_records: set[str] = set()
    removed_handoff_records: set[str] = set()
    for record in normalized["records"]:
        record_id = record["id"]
        handoff_evidence = [
            evidence
            for evidence in record["evidence"]
            if evidence["fixtureId"] == _HANDOFF_FIXTURE_ID
        ]
        if handoff_evidence:
            assert handoff_evidence == [
                {
                    "level": "H2",
                    "fixture": "tests/fixtures/h2/map-event-direct-handoff-static-v1.json",
                    "fixtureId": _HANDOFF_FIXTURE_ID,
                    "verifier": "src/sf2tool/h2/map_event_direct_handoff.py",
                    "bindings": [
                        {
                            "addressId": "entry",
                            "fixtureField": (
                                f"eventDirectHandoff.sourceFiles.{record['symbol']}.tableEntryAddress"
                            ),
                        }
                    ],
                }
            ]
            assert record["documents"].count(_HANDOFF_DOCUMENT) == 1
            assert record["documents"][-1] == _HANDOFF_DOCUMENT
            record["evidence"] = [
                evidence
                for evidence in record["evidence"]
                if evidence["fixtureId"] != _HANDOFF_FIXTURE_ID
            ]
            record["documents"].remove(_HANDOFF_DOCUMENT)
            removed_handoff_records.add(record_id)
        direct_control_evidence = [
            evidence
            for evidence in record["evidence"]
            if evidence["fixtureId"] == _DIRECT_CONTROL_FIXTURE_ID
        ]
        if direct_control_evidence:
            assert len(direct_control_evidence) == 1
            assert direct_control_evidence[0] == {
                "level": "H2",
                "fixture": "tests/fixtures/h2/map-event-direct-control-static-v1.json",
                "fixtureId": _DIRECT_CONTROL_FIXTURE_ID,
                "verifier": "src/sf2tool/h2/map_event_direct_control.py",
                "bindings": [
                    {
                        "addressId": "entry",
                        "fixtureField": (
                            f"eventDirectControl.sourceFiles.{record['symbol']}.tableEntryAddress"
                        ),
                    }
                ],
            }
            assert record["documents"].count(_DIRECT_CONTROL_DOCUMENT) == 1
            assert record["documents"][-1] == _DIRECT_CONTROL_DOCUMENT
            record["evidence"] = [
                evidence
                for evidence in record["evidence"]
                if evidence["fixtureId"] != _DIRECT_CONTROL_FIXTURE_ID
            ]
            record["documents"].remove(_DIRECT_CONTROL_DOCUMENT)
            removed_direct_control_records.add(record_id)
        direct_state_evidence = [
            evidence
            for evidence in record["evidence"]
            if evidence["fixtureId"] == _DIRECT_STATE_FIXTURE_ID
        ]
        if direct_state_evidence:
            assert record_id in _DIRECT_STATE_OWNER_IDS
            assert len(direct_state_evidence) == 1
            assert direct_state_evidence[0]["level"] == "H2"
            assert direct_state_evidence[0]["fixture"] == (
                "tests/fixtures/h2/map-event-direct-state-static-v1.json"
            )
            assert direct_state_evidence[0]["verifier"] == _DIRECT_STATE_VERIFIER
            assert direct_state_evidence[0]["bindings"] == [
                {
                    "addressId": "entry",
                    "fixtureField": (
                        "eventDirectState.sourceFiles."
                        f"{record['symbol']}.tableEntryAddress"
                    ),
                }
            ]
            removed_direct_state_evidence.add(record_id)
        record["evidence"] = [
            item
            for item in record["evidence"]
            if item["fixtureId"] != _DIRECT_STATE_FIXTURE_ID
        ]
        evidence = [item for item in record["evidence"] if item["fixtureId"] == field_search.ID]
        if evidence:
            assert record_id in owner_ids and len(evidence) == 1 and evidence[0]["level"] == "H2"
            removed_evidence.add(record_id)
        record["evidence"] = [
            item for item in record["evidence"] if item["fixtureId"] != field_search.ID
        ]
        retained_addresses = []
        for address in record["addresses"]:
            key = (record_id, address["id"], address["value"])
            if key in expected_new_addresses:
                removed_addresses.add(key)
            else:
                retained_addresses.append(address)
        record["addresses"] = retained_addresses
        if _DIRECT_STATE_DOCUMENT in record["documents"]:
            assert record_id in _DIRECT_STATE_OWNER_IDS
            assert record["documents"].count(_DIRECT_STATE_DOCUMENT) == 1
            assert record["documents"][-1] == _DIRECT_STATE_DOCUMENT
            record["documents"].remove(_DIRECT_STATE_DOCUMENT)
            removed_direct_state_documents.add(record_id)
        if "docs/research/field-search-control.md" in record["documents"]:
            assert record_id in owner_ids
            assert record["documents"].count("docs/research/field-search-control.md") == 1
            record["documents"].remove("docs/research/field-search-control.md")
            removed_documents.add(record_id)
    assert removed_evidence == owner_ids
    assert removed_addresses == expected_new_addresses
    assert removed_documents == owner_ids
    assert removed_direct_state_evidence == _DIRECT_STATE_OWNER_IDS
    assert removed_direct_state_documents == _DIRECT_STATE_OWNER_IDS
    assert len(removed_direct_control_records) == 53
    assert len(removed_handoff_records) == 53
    assert _canonical_sha256(normalized) == _PRE_SLICE_INDEX_SHA256

    def invalid(field: str) -> None:
        broken = deepcopy(index)
        binding = next(
            binding
            for record in broken["records"]
            for evidence in record["evidence"]
            if evidence["fixtureId"] == field_search.ID
            for binding in evidence["bindings"]
        )
        binding["fixtureField"] = field
        with pytest.raises(ValueError, match="schema"):
            validate_json(broken, INDEX_SCHEMA, owner="field-search index")

    invalid("unknownRoot.fieldSearchSpine")
    invalid("sourceContext.fieldSearchSpine.callers")


@pytest.mark.skipif(
    not (UPSTREAM / "build/sf2build-h1.bin").is_file() or not ROM.is_file(),
    reason="pinned H1 artifact or canonical ROM is unavailable",
)
def test_field_search_complete_verifier_matches_fixture() -> None:
    assert field_search.verify_field_search_control_static(ROM, UPSTREAM) == load_json(FIXTURE)
