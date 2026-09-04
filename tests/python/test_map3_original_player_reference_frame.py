from __future__ import annotations

import copy
import json
import re
from dataclasses import replace
from pathlib import Path

import pytest
from jsonschema import Draft7Validator

import sf2tool.research_index as research_index
from sf2tool.h2 import map3_original_player_reference_frame as reference_frame
from sf2tool.jsonio import validate_json
from sf2tool.research_index import verify_index

ROOT = Path(__file__).resolve().parents[2]
ROM = ROOT / "local/roms/sf2-us.bin"
UPSTREAM = ROOT / "local/upstream/SF2DISASM"
DISASM = UPSTREAM / "disasm"


def _fixture() -> dict[str, object]:
    return json.loads(reference_frame.FIXTURE.read_text(encoding="utf-8"))


def _schema() -> dict[str, object]:
    return json.loads(reference_frame.SCHEMA.read_text(encoding="utf-8"))


def _mutate_source(
    monkeypatch: pytest.MonkeyPatch,
    relative: Path,
    old: str,
    new: str,
) -> None:
    original = reference_frame._source_text

    def changed(disasm: Path, candidate: Path) -> str:
        text = original(disasm, candidate)
        if candidate == relative:
            assert old in text
            return text.replace(old, new)
        return text

    monkeypatch.setattr(reference_frame, "_source_text", changed)


def test_build_reproduces_the_closed_public_fixture() -> None:
    fixture = _fixture()
    assert reference_frame.build_map3_original_player_reference_frame(ROM, UPSTREAM) == fixture
    validate_json(fixture, reference_frame.FIXTURE_SCHEMA, owner="fixture")
    validate_json(fixture, reference_frame.SCHEMA, owner="output")


def test_static_selection_and_import_policy_are_exact() -> None:
    static = _fixture()["static"]
    assert static["controlledPlayer"] == {
        "sourceFunction": "InitializeMapEntities",
        "controlledEntityIndex": 0,
        "allyIndex": 0,
        "selectionBasis": "explicit-ally-zero",
        "selectionNotEntityRowOrder": True,
        "allyTable": "table_AllyMapsprites",
        "allyRowValue": 1,
        "classSymbol": "CLASS_SDMN",
        "classValue": 0,
        "classTransform": "subtract-one",
        "regularMapSpriteId": 0,
        "storageFunction": "DeclareNewEntity",
        "storedField": "ENTITYDEF_OFFSET_MAPSPRITE",
        "controlledFacing": 3,
        "controlledDirection": "DOWN",
    }
    assert static["directionSelection"]["rules"] == [
        {"direction": "UP", "facing": 1, "sourceSlot": 0, "horizontalMirror": False},
        {"direction": "LEFT", "facing": 2, "sourceSlot": 1, "horizontalMirror": False},
        {"direction": "RIGHT", "facing": 0, "sourceSlot": 1, "horizontalMirror": True},
        {"direction": "DOWN", "facing": 3, "sourceSlot": 2, "horizontalMirror": False},
    ]
    assert static["selectedPayload"] == {
        "pointerTable": "pt_Mapsprites",
        "sourceSlot": 2,
        "symbol": "Mapsprite000_2",
        "codec": "Basic",
        "sourceH1RomParity": True,
        "decodedBytes": 576,
        "halfCount": 2,
        "halfBytes": 288,
        "framePixels": [24, 24],
        "frameTiles": [3, 3],
        "tileBytes": 32,
        "bitsPerPixel": 4,
        "tileOrder": "column-major",
    }
    assert static["framePolicy"] == {
        "label": "initial-reference-frame",
        "selectedHalf": 0,
        "classification": "project-import-policy",
        "sourceRoots": [
            "DeclareNewEntity-animation-counter-initialization",
            "VInt_UpdateSprites-first-frame-branch",
        ],
        "observedStandingOrIdleFrame": False,
        "observedVisibleAtFirstWaitForEvent": False,
    }


@pytest.mark.parametrize(
    ("relative", "old", "new", "message"),
    (
        (
            reference_frame._MAP_FUNCTIONS,
            "clr.w   d0",
            "moveq   #1,d0",
            "controlled-player declaration",
        ),
        (
            reference_frame._ENUMS,
            "CLASS_SDMN: equ 0",
            "CLASS_SDMN: equ 1",
            "ally/class/mapsprite enum",
        ),
        (
            reference_frame._ENTITY_ENGINE_2,
            "dc.b UP",
            "dc.b RIGHT",
            "facing table",
        ),
        (
            reference_frame._ENTITY_ENGINE_1,
            "cmpi.b  #15,d4",
            "cmpi.b  #14,d4",
            "VInt first-frame",
        ),
    ),
    ids=("source-selection", "class-transform", "facing-transform", "frame-policy-root"),
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
        reference_frame.build_map3_original_player_reference_frame(ROM, UPSTREAM)


def test_selected_slot_and_symbol_near_miss_is_rejected(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    original = Path.read_text

    def changed(path: Path, *args: object, **kwargs: object) -> str:
        text = original(path, *args, **kwargs)
        if path.as_posix().endswith(reference_frame._MAPSPRITE_ENTRIES.as_posix()):
            text = text.replace("dc.l Mapsprite000_2", "dc.l Mapsprite000_1", 1)
        return text

    monkeypatch.setattr(Path, "read_text", changed)
    with pytest.raises(ValueError, match="source slot/symbol drift"):
        reference_frame.build_map3_original_player_reference_frame(ROM, UPSTREAM)


def test_decode_half_denominator_is_rejected(monkeypatch: pytest.MonkeyPatch) -> None:
    original = reference_frame.decode_basic_compressed

    def shortened(data: bytes, *, expected_output_bytes: int | None = None):
        result = original(data, expected_output_bytes=expected_output_bytes)
        return replace(result, output=result.output[:-2])

    monkeypatch.setattr(reference_frame, "decode_basic_compressed", shortened)
    with pytest.raises(ValueError, match="decoded-half denominator drift"):
        reference_frame.build_map3_original_player_reference_frame(ROM, UPSTREAM)


@pytest.mark.parametrize(
    ("old", "new"),
    (
        ("lea     palette_Base(pc), a0", "lea     palette_1(pc), a0"),
        ("lea     (PALETTE_3_BASE).l,a1", "lea     (PALETTE_2_BASE).l,a1"),
    ),
    ids=("palette-source", "palette3-destination"),
)
def test_palette_source_and_destination_near_misses_are_rejected(
    monkeypatch: pytest.MonkeyPatch, old: str, new: str
) -> None:
    _mutate_source(monkeypatch, reference_frame._DISPLAY_INIT, old, new)
    with pytest.raises(ValueError, match="palette_Base to palette3 copy"):
        reference_frame.build_map3_original_player_reference_frame(ROM, UPSTREAM)


@pytest.mark.parametrize(
    "mutation",
    (
        lambda value: value["static"]["directionSelection"].update({"selectedSlot": 1}),
        lambda value: value["static"]["selectedPayload"].update({"halfBytes": 287}),
        lambda value: value["static"]["selectedPayload"].update({"tileOrder": "row-major"}),
        lambda value: value["static"]["framePolicy"].update({"label": "observed-standing-frame"}),
        lambda value: value["static"]["palettePolicy"].update({"sourceSymbol": "palette_1"}),
        lambda value: value["static"]["palettePolicy"].update(
            {"rgbExpansionPolicy": "original-display-parity"}
        ),
        lambda value: value["static"]["unknowns"].pop("dmaCacheCompletion"),
        lambda value: value["static"].update({"runtimeObservation": True}),
    ),
    ids=(
        "slot",
        "half-size",
        "tile-order",
        "frame-label",
        "palette-source",
        "rgb-policy",
        "unknown-set",
        "runtime-extra",
    ),
)
def test_schema_rejects_adversarial_contract_mutations(mutation: object) -> None:
    malformed = copy.deepcopy(_fixture())
    mutation(malformed)  # type: ignore[operator]
    assert list(Draft7Validator(_schema()).iter_errors(malformed))


def test_public_fixture_has_no_private_payload_surface() -> None:
    fixture = _fixture()
    forbidden_keys = {
        "compressedBytes",
        "decodedHex",
        "decodedSha256",
        "paletteBytes",
        "paletteHex",
        "resourceSha256",
        "sourceRange",
        "sourceEnd",
    }

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


def test_later_owner_index_normalizer_is_deep_exact_and_fail_closed() -> None:
    current = json.loads((ROOT / "manifests/research-index.json").read_text(encoding="utf-8"))
    original = copy.deepcopy(current)
    remove_delta = (
        reference_frame._remove_map3_original_player_reference_frame_later_owner_index_delta
    )
    owner_state = research_index._normalize_current_index_to_owner_state(
        current, owner_id=reference_frame.ID
    )
    normalized = remove_delta(owner_state)
    assert current == original
    registry_step = next(
        step for step in research_index._LATER_OWNER_STEPS if step.owner_id == reference_frame.ID
    )
    assert registry_step.predecessor_owner_id == "sf2-map-event-flag-route-selection-static-v1"
    assert registry_step.remover == (
        "sf2tool.h2.map3_original_player_reference_frame:"
        "_remove_map3_original_player_reference_frame_later_owner_index_delta"
    )
    assert registry_step.state_sha256 == research_index._canonical_index_sha256(owner_state)
    assert registry_step.predecessor_sha256 == research_index._canonical_index_sha256(normalized)
    assert (
        research_index.normalize_current_index_to_owner_predecessor(
            current, owner_id=reference_frame.ID
        )
        == normalized
    )
    assert (
        reference_frame._canonical_digest(normalized) == reference_frame._PREDECESSOR_INDEX_SHA256
    )
    for record in normalized["records"]:
        assert all(item.get("fixtureId") != reference_frame.ID for item in record["evidence"])
        assert reference_frame._INDEX_DOCUMENT not in record["documents"]
        assert not {address["id"] for address in record["addresses"]} & {
            address["id"] for address in reference_frame._INDEX_ADDRESSES.values()
        }

    def record_for(value: dict[str, object], record_id: str) -> dict[str, object]:
        return next(record for record in value["records"] if record["id"] == record_id)

    malformed = copy.deepcopy(owner_state)
    record = record_for(malformed, "scripting.map.mapfunctions")
    record["evidence"].append(copy.deepcopy(record["evidence"][-1]))
    with pytest.raises(ValueError, match="evidence/document drift"):
        remove_delta(malformed)

    malformed = copy.deepcopy(owner_state)
    record = record_for(malformed, "scripting.entity.declarenewentity")
    record["documents"].remove(reference_frame._INDEX_DOCUMENT)
    with pytest.raises(ValueError, match="evidence/document drift"):
        remove_delta(malformed)

    malformed = copy.deepcopy(owner_state)
    record = record_for(malformed, "auxiliary.data.pt-mapsprites")
    next(address for address in record["addresses"] if address["id"] == "selected-payload")[
        "value"
    ] += 1
    with pytest.raises(ValueError, match="index address drift"):
        remove_delta(malformed)

    malformed = copy.deepcopy(owner_state)
    record_for(malformed, "scripting.map.mapfunctions")["symbol"] = "NearMiss"
    with pytest.raises(ValueError, match="predecessor record drift"):
        remove_delta(malformed)

    malformed = copy.deepcopy(owner_state)
    record_for(malformed, "battle.activation.activate-enemies")["status"] = "inferred"
    with pytest.raises(ValueError, match="predecessor index drift"):
        remove_delta(malformed)


def test_index_has_exact_existing_owner_delta_and_public_totals() -> None:
    index = json.loads((ROOT / "manifests/research-index.json").read_text(encoding="utf-8"))
    fixture_id = reference_frame.ID
    evidence = [
        (record, item)
        for record in index["records"]
        for item in record["evidence"]
        if item.get("fixtureId") == fixture_id
    ]
    assert len(index["records"]) == 1627
    assert len(evidence) == 10
    assert sum(len(item["bindings"]) for _, item in evidence) == 12
    assert (
        sum(
            record["documents"].count("docs/research/map3-original-player-reference-frame.md")
            for record, _ in evidence
        )
        == 10
    )
    new_addresses = {
        (record["id"], address["id"], address["value"])
        for record, _ in evidence
        for address in record["addresses"]
        if address["id"] in {"selected-payload", "palette-base", "dma-entity-mapsprite"}
    }
    assert new_addresses == {
        ("auxiliary.data.pt-mapsprites", "selected-payload", 822782),
        ("tech.graphics.display-init", "palette-base", 12446),
        ("scripting.entity.entityscriptengine-2", "dma-entity-mapsprite", 24970),
    }
    result = verify_index()
    assert result["Records"] == 1627
    assert result["H2Fixtures"] == 103
    assert result["H3Fixtures"] == result["H3FixtureFiles"] == 95
    assert result["AddressBindings"] == 3109
    assert result["ResearchDocuments"] == 66
    assert result["DesignContracts"] == 68
