from __future__ import annotations

import copy
import json
import re
from dataclasses import replace
from pathlib import Path

import pytest
from jsonschema import Draft7Validator

from sf2tool.h2 import map3_entity142_interactable_reference as reference
from sf2tool.jsonio import validate_json
from sf2tool.research_index import verify_index

ROOT = Path(__file__).resolve().parents[2]
ROM = ROOT / "local/roms/sf2-us.bin"
UPSTREAM = ROOT / "local/upstream/SF2DISASM"


def _fixture() -> dict[str, object]:
    return json.loads(reference.FIXTURE.read_text(encoding="utf-8"))


def _schema() -> dict[str, object]:
    return json.loads(reference.SCHEMA.read_text(encoding="utf-8"))


def _mutate_source(monkeypatch: pytest.MonkeyPatch, relative: Path, old: str, new: str) -> None:
    original = reference._source_text

    def changed(disasm: Path, candidate: Path) -> str:
        text = original(disasm, candidate)
        if candidate == relative:
            assert old in text
            return text.replace(old, new)
        return text

    monkeypatch.setattr(reference, "_source_text", changed)


def test_build_reproduces_the_closed_public_fixture() -> None:
    fixture = _fixture()
    assert reference.build_map3_entity142_interactable_reference(ROM, UPSTREAM) == fixture
    validate_json(fixture, reference.FIXTURE_SCHEMA, owner="fixture")
    validate_json(fixture, reference.SCHEMA, owner="output")


def test_identity_mapping_event_and_two_half_reference_are_exact() -> None:
    static = _fixture()["static"]
    assert static["sourceRecord"]["oneBasedOrdinal"] == 17
    assert static["sourceRecord"]["zeroBasedIndex"] == 16
    assert static["sourceRecord"]["recordHex"] == "361101D1000460CE"
    assert static["identityMapping"]["logicalEntityId"] == 142
    assert static["identityMapping"]["logicalEncoding"]["entityIndexListOffset"] == 46
    assert static["identityMapping"]["sequentialAllocation"]["resolvedPhysicalSlot"] == 17
    assert static["interactionEvent"]["zeroBasedRecordIndex"] == 15
    assert static["interactionEvent"]["target"] == "Map3_EntityEvent15"
    assert static["interactionEvent"]["eventFacingControl"] == {
        "sourceSymbol": "DOWN",
        "value": 3,
        "loadedRegister": "D6",
        "testedBits": [0, 1],
        "broaderSemantics": "Unknown",
    }
    assert static["retainedRuntime"]["newRuntimeObservation"] is False
    assert static["retainedRuntime"]["dispatch"] == {
        "logicalEntityId": 142,
        "register": "D0",
        "target": "Map3_EntityEvent15",
    }
    drawable = static["drawableReference"]
    assert drawable["direction"] == {
        "policyOwner": "sf2-map3-original-player-reference-frame-static-v1",
        "symbol": "UP",
        "value": 1,
        "sourceSlot": 0,
        "horizontalMirror": False,
    }
    assert drawable["pointer"]["payloadSymbol"] == "Mapsprite209_0"
    assert drawable["decoded"]["bytes"] == 576
    assert drawable["decoded"]["halfBytes"] == 288
    assert [row["index"] for row in drawable["decoded"]["halves"]] == [0, 1]
    assert drawable["palette"]["policyOwner"] == (
        "sf2-map3-original-player-reference-frame-static-v1"
    )
    assert drawable["assetReadiness"] == {
        "classification": "two-half-reference",
        "selectedVisibleHalf": "Unknown",
        "interactionTimeAnimCounter": "Unknown",
    }


@pytest.mark.parametrize(
    ("relative", "old", "new", "message"),
    (
        (
            reference._MAP_FUNCTIONS,
            "lea     NEXT_ENTITYDEF(a1),a2",
            "lea     (a1),a2",
            "sequential allocation",
        ),
        (
            reference._FOLLOWER_FUNCTIONS,
            "move.b  d0,(a1,d6.w)",
            "move.b  d0,(a1)",
            "follower mapping",
        ),
        (
            reference._MAPSCRIPT_FUNCTIONS,
            "subi.b  #ENTITY_ENEMY_INDEX_DIFFERENCE,d0",
            "subi.b  #95,d0",
            "logical-to-physical",
        ),
        (
            reference._MAP3_ENTITIES,
            "msFixedEntity 54, 17, UP, MAPSPRITE_ASTRAL, eas_Init",
            "msFixedEntity 54, 17, DOWN, MAPSPRITE_ASTRAL, eas_Init",
            "selected source record",
        ),
        (
            reference._MAP_SETUP_FUNCTIONS,
            "btst    #0,d6",
            "btst    #2,d6",
            "facing-control operand use",
        ),
    ),
)
def test_source_near_misses_fail_before_fixture_comparison(
    monkeypatch: pytest.MonkeyPatch,
    relative: Path,
    old: str,
    new: str,
    message: str,
) -> None:
    _mutate_source(monkeypatch, relative, old, new)
    with pytest.raises(ValueError, match=message):
        reference.build_map3_entity142_interactable_reference(ROM, UPSTREAM)


def test_decode_must_retain_both_complete_halves(monkeypatch: pytest.MonkeyPatch) -> None:
    original = reference.decode_basic_compressed

    def shortened(data: bytes, *, expected_output_bytes: int | None = None):
        result = original(data, expected_output_bytes=expected_output_bytes)
        return replace(result, output=result.output[:-2])

    monkeypatch.setattr(reference, "decode_basic_compressed", shortened)
    with pytest.raises(ValueError, match="decode denominator"):
        reference.build_map3_entity142_interactable_reference(ROM, UPSTREAM)


def test_retained_runtime_near_miss_is_rejected(monkeypatch: pytest.MonkeyPatch) -> None:
    original = reference.load_json

    def changed(path: Path):
        value = original(path)
        if path == reference._NATURAL_ROUTE_FIXTURE:
            value = copy.deepcopy(value)
            waypoint = next(
                row
                for row in value["static"]["route"]["waypoints"]
                if row["id"] == "map3-entity142"
            )
            waypoint["entityTarget"]["id"] = 17
        return value

    monkeypatch.setattr(reference, "load_json", changed)
    with pytest.raises(ValueError, match="retained waypoint drift"):
        reference.build_map3_entity142_interactable_reference(ROM, UPSTREAM)


def test_retained_player_reference_policy_near_miss_is_rejected(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    original = reference.load_json

    def changed(path: Path):
        value = original(path)
        if path == reference._PLAYER_REFERENCE_FIXTURE:
            value = copy.deepcopy(value)
            up = next(
                row
                for row in value["static"]["directionSelection"]["rules"]
                if row["direction"] == "UP"
            )
            up["sourceSlot"] = 1
        return value

    monkeypatch.setattr(reference, "load_json", changed)
    with pytest.raises(ValueError):
        reference.build_map3_entity142_interactable_reference(ROM, UPSTREAM)


@pytest.mark.parametrize(
    "mutation",
    (
        lambda value: value["static"]["identityMapping"].update({"logicalEntityId": 17}),
        lambda value: value["static"]["sourceRecord"].update({"oneBasedOrdinal": 16}),
        lambda value: value["static"]["interactionEvent"].update({"zeroBasedRecordIndex": 16}),
        lambda value: value["static"]["interactionEvent"]["eventFacingControl"].update(
            {"value": 2}
        ),
        lambda value: value["static"]["interactionEvent"].update(
            {"requiredPlayerFacing": {"symbol": "DOWN", "value": 3}}
        ),
        lambda value: value["static"]["retainedRuntime"].update({"newRuntimeObservation": True}),
        lambda value: value["static"]["drawableReference"]["decoded"].update({"halfBytes": 576}),
        lambda value: value["static"]["drawableReference"]["assetReadiness"].update(
            {"selectedVisibleHalf": 0}
        ),
        lambda value: value["static"]["unknowns"].pop("interactionTimeAnimCounter"),
        lambda value: value["static"]["identityMapping"]["followerReuse"]["rows"][0].update(
            {"extra": True}
        ),
        lambda value: value["static"]["retainedRuntime"]["player"].update({"extra": True}),
        lambda value: value["static"]["retainedRuntime"]["entityTarget"].update({"extra": True}),
        lambda value: value["static"]["retainedRuntime"]["dispatch"].update({"extra": True}),
        lambda value: value["static"]["drawableReference"]["mapSprite"].update({"extra": True}),
        lambda value: value["static"]["drawableReference"]["direction"].update({"extra": True}),
        lambda value: value["static"]["drawableReference"]["pointer"].update({"extra": True}),
        lambda value: value["static"]["drawableReference"]["decoded"]["halves"][0].update(
            {"extra": True}
        ),
        lambda value: value["static"]["drawableReference"]["format"].update({"extra": True}),
        lambda value: value["static"]["drawableReference"]["palette"].update({"extra": True}),
    ),
)
def test_schema_rejects_adversarial_contract_mutations(mutation: object) -> None:
    malformed = copy.deepcopy(_fixture())
    mutation(malformed)  # type: ignore[operator]
    assert list(Draft7Validator(_schema()).iter_errors(malformed))


def test_public_fixture_has_no_private_payload_or_path_surface() -> None:
    fixture = _fixture()
    forbidden_keys = {"decodedHex", "paletteHex", "paletteBytes", "capture", "screenshot", "png"}

    def visit(value: object) -> None:
        if isinstance(value, dict):
            assert forbidden_keys.isdisjoint(value)
            for child in value.values():
                visit(child)
        elif isinstance(value, list):
            for child in value:
                visit(child)
        elif isinstance(value, str):
            assert not re.search(r"^[A-Za-z]:[\\/]", value)
            assert not value.lower().endswith(".png")

    visit(fixture)


def test_index_has_exact_existing_record_delta_and_public_totals() -> None:
    index = json.loads((ROOT / "manifests/research-index.json").read_text(encoding="utf-8"))
    evidence = [
        (record, item)
        for record in index["records"]
        for item in record["evidence"]
        if item.get("fixtureId") == reference.ID
    ]
    assert len(evidence) == 8
    assert sum(len(item["bindings"]) for _, item in evidence) == 8
    assert sum(record["documents"].count(reference._INDEX_DOCUMENT) for record, _ in evidence) == 8
    normalized = reference._remove_map3_entity142_interactable_reference_later_owner_index_delta(
        index
    )
    assert reference._canonical_digest(normalized) == reference._PREDECESSOR_INDEX_SHA256
    result = verify_index()
    assert result["Records"] == 1627
    assert result["H2Fixtures"] == 103
    assert result["H3Fixtures"] == result["H3FixtureFiles"] == 95
    assert result["AddressBindings"] == 3109
    assert result["ResearchDocuments"] == 66
