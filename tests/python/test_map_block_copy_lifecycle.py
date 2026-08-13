from __future__ import annotations

from copy import deepcopy
from pathlib import Path
from shutil import copy2

import pytest

from sf2tool.h3 import map_block_copy_lifecycle as rail
from sf2tool.h3.map_block_copy_lifecycle import (
    CASE_IDS,
    OBSERVATION_SCHEMA,
    _case_inputs,
    _derive_cases,
    _roof_records,
    _section,
    _validate_case_matrix,
    build_map_block_copy_lifecycle_contract,
)
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

FIXTURE = repo_path("tests/fixtures/h3/map-block-copy-lifecycle-v1.json")
SCHEMA = repo_path("schemas/h3-map-block-copy-lifecycle-fixture.schema.json")
ROM = Path("local/roms/sf2-us.bin")
UPSTREAM = Path("local/upstream/SF2DISASM")


def _static_from_fixture(fixture: dict[str, object]) -> dict[str, object]:
    return {key: deepcopy(fixture[key]) for key in ("function", "ram", "constants", "sourceFacts")}


def test_fixture_has_the_complete_ten_case_source_model() -> None:
    fixture = load_json(FIXTURE)
    validate_json(fixture, SCHEMA, owner="map block copy lifecycle fixture")
    rows = _derive_cases(_static_from_fixture(fixture), fixture)

    assert [row["id"] for row in rows] == [case["id"] for case in fixture["cases"]]
    assert rows[2]["busyWordAfter"] == 6
    assert rows[7]["busyWordAfter"] == 1
    assert rows[3]["layoutReadbacks"] == rows[9]["layoutReadbacks"]
    assert len(rows[3]["layoutReadbacks"]) == 63
    assert rows[6]["savedBufferSentinelAfter"] == {"address": 16735366, "value": 65535}
    assert rows[7]["savedBufferSentinelAfter"] == {"address": 16735352, "value": 65535}
    assert [row["updateToggleByteAfter"] for row in rows] == [0, 0, 1, 1, 0, 0, 1, 1, 0, 1]


def test_toggle_golden_and_saved_rectangle_golden_reject_near_misses() -> None:
    fixture = load_json(FIXTURE)
    static = _static_from_fixture(fixture)

    wrong_toggle = deepcopy(fixture)
    wrong_toggle["toggleGolden"]["dispatcher-show"] = 0
    with pytest.raises(ValueError, match="toggle golden disagreement"):
        _derive_cases(static, wrong_toggle)

    wrong_rectangle = deepcopy(fixture)
    wrong_rectangle["cases"][9]["expected"]["layoutReadbacks"][62]["value"] = 0
    with pytest.raises(ValueError, match="fixture/model disagreement"):
        _derive_cases(static, wrong_rectangle)


def test_lua_inputs_exclude_accepted_output_and_golden() -> None:
    fixture = load_json(FIXTURE)
    inputs = _case_inputs(_static_from_fixture(fixture), fixture)

    assert len(inputs) == 10
    assert all("expected" not in row for row in inputs)
    assert all("toggleGolden" not in row for row in inputs)
    assert inputs[3]["layoutReadbackAddresses"] == inputs[9]["layoutReadbackAddresses"]
    assert len(inputs[3]["layoutReadbackAddresses"]) == 63
    assert "expectedRecords" not in repo_path(
        "tools/bizhawk/map_block_copy_lifecycle_observer.lua"
    ).read_text(encoding="utf-8")


def test_source_section_and_roof_record_parsers_reject_near_misses() -> None:
    with pytest.raises(ValueError, match="missing source section"):
        _section("other:\n rts\n", "PerformMapBlockCopyScript")

    roof_row = "\n".join(("slbc 1, 2", "slbcSource 3, 4", "slbcSize 5, 6", "slbcDest 7, 8"))
    with pytest.raises(ValueError, match="record inventory drift"):
        _roof_records("\n".join([roof_row] * 9))


def test_real_source_h1_rom_contract_derives_all_seams_and_plan() -> None:
    fixture = load_json(FIXTURE)
    static = build_map_block_copy_lifecycle_contract(ROM, UPSTREAM)

    assert static["function"] == fixture["function"]
    assert static["instrumentation"] == fixture["instrumentation"]
    assert static["sourceFacts"]["selectedRoofRecords"]["negative"]["ordinal"] == 1
    assert static["sourceFacts"]["selectedRoofRecords"]["positive"]["ordinal"] == 6
    assert static["constants"] == fixture["constants"]


@pytest.mark.parametrize(
    ("name", "mutation", "message"),
    [
        ("reordered", lambda value: value["cases"].reverse(), "exact case ID/order/kind"),
        (
            "renamed",
            lambda value: value["cases"][0].__setitem__("id", "fading"),
            "exact case ID/order/kind",
        ),
        (
            "wrong-kind",
            lambda value: value["cases"][9].__setitem__("kind", "csub-inactive-skip"),
            "exact case ID/order/kind",
        ),
        (
            "wrong-roof",
            lambda value: value["cases"][2].__setitem__("roofKind", "negative"),
            "exact case ID/order/kind",
        ),
        ("wrong-scope", lambda value: value["cases"][4].__setitem__("blockWord", 0), "case scope"),
    ],
)
def test_exact_case_matrix_rejects_schema_valid_semantic_drift(
    name: str, mutation: object, message: str
) -> None:
    fixture = load_json(FIXTURE)
    mutation(fixture)
    validate_json(fixture, SCHEMA, owner=name)
    with pytest.raises(ValueError, match=message):
        _validate_case_matrix(fixture)


@pytest.mark.parametrize(
    "mutation",
    [
        lambda value: value["cases"][0].pop("expected"),
        lambda value: value["cases"][0]["expected"].__setitem__("unexpected", 1),
        lambda value: value["sourceFacts"]["selectedRoofRecords"]["positive"].pop("ordinal"),
        lambda value: value["instrumentation"].__setitem__("unexpected", 1),
    ],
)
def test_fixture_schema_recursively_closes_owned_shape(mutation: object) -> None:
    fixture = load_json(FIXTURE)
    mutation(fixture)
    with pytest.raises(ValueError, match="failed schema validation"):
        validate_json(fixture, SCHEMA, owner="recursive fixture mutation")


def test_observation_schema_rejects_recursive_missing_extra_and_order_shape() -> None:
    fixture = load_json(FIXTURE)
    rows = _derive_cases(_static_from_fixture(fixture), fixture)
    observed = {
        "system": "GEN",
        "core": fixture["emulator"]["core"],
        "id": fixture["id"],
        "mapTest": fixture["mapTestIndex"],
        "recordOrder": list(CASE_IDS),
        "records": rows,
    }
    validate_json(observed, OBSERVATION_SCHEMA, owner="observation")
    for mutate in (
        lambda value: value["records"][0].pop("busyWordAfter"),
        lambda value: value["records"][0].__setitem__("unexpected", True),
        lambda value: value["recordOrder"].reverse(),
    ):
        changed = deepcopy(observed)
        mutate(changed)
        if changed["recordOrder"] != list(CASE_IDS):
            # Structural schemas deliberately do not own golden ordering.
            assert changed["recordOrder"] != [row["id"] for row in rows]
        else:
            with pytest.raises(ValueError, match="failed schema validation"):
                validate_json(changed, OBSERVATION_SCHEMA, owner="observation mutation")


def _copy_surface(tmp_path: Path) -> Path:
    root = tmp_path / "upstream"
    for relative in (
        rail.DISPATCH_SOURCE,
        rail.EXPLORATION_SOURCE,
        rail.MACRO_SOURCE,
        rail.MAP_MACRO_SOURCE,
        *rail.EQUATE_PATHS,
        *rail.USE_PATHS,
        rail.ROOF_PATH,
    ):
        destination = root / "disasm" / relative
        destination.parent.mkdir(parents=True, exist_ok=True)
        copy2(UPSTREAM / "disasm" / relative, destination)
    listing = root / rail.H1_LISTING_PATH
    listing.parent.mkdir(parents=True, exist_ok=True)
    copy2(UPSTREAM / rail.H1_LISTING_PATH, listing)
    return root


@pytest.mark.parametrize(
    ("path", "old", "new", "message"),
    [
        (rail.H1_LISTING_PATH, "00005D32 6100 E2B6", "00005D32 6000 E2B6", "H1 BSR opcode/width"),
        (rail.H1_LISTING_PATH, "00005D28 0242 3C00", "00005D28 0242 3D00", "ROM/H1 instruction"),
        (rail.DISPATCH_SOURCE, "cmpi.w  #$800,d2", "cmpi.w  #$801,d2", "source use-site/order"),
        (rail.EXPLORATION_SOURCE, "dbf     d6,loc_4096", "dbf     d7,loc_4096", "source sequence"),
    ],
)
def test_source_h1_opcode_operand_width_and_order_mutations_reject_before_launch(
    tmp_path: Path, path: Path, old: str, new: str, message: str
) -> None:
    upstream = _copy_surface(tmp_path)
    target = upstream / path if path == rail.H1_LISTING_PATH else upstream / "disasm" / path
    text = target.read_text(encoding="utf-8")
    assert old in text
    target.write_text(text.replace(old, new, 1), encoding="utf-8")
    with pytest.raises(ValueError, match=message):
        build_map_block_copy_lifecycle_contract(ROM, upstream)


def test_complete_five_use_inventory_ignores_comments_and_rejects_missing_use(
    tmp_path: Path,
) -> None:
    upstream = _copy_surface(tmp_path)
    uses = upstream / "disasm" / rail.USE_PATHS[0]
    text = uses.read_text(encoding="utf-8")
    uses.write_text(text + "\n; ac_checkMapBlockCopy\n", encoding="utf-8")
    assert (
        build_map_block_copy_lifecycle_contract(ROM, upstream)["sourceFacts"]["fiveUseInventory"][
            0
        ]["instructionSiteCount"]
        == 3
    )
    uses.write_text(
        text.replace("ac_checkMapBlockCopy", "; ac_checkMapBlockCopy", 1), encoding="utf-8"
    )
    with pytest.raises(ValueError, match="five-use inventory"):
        build_map_block_copy_lifecycle_contract(ROM, upstream)


def test_instrumentation_plan_rejects_fixture_drift_and_preserves_canonical_rom() -> None:
    fixture = load_json(FIXTURE)
    static = build_map_block_copy_lifecycle_contract(ROM, UPSTREAM)
    before = ROM.read_bytes()
    drifted = deepcopy(fixture)
    drifted["instrumentation"]["stubHex"] = "4E75"
    with pytest.raises(ValueError, match="fixture instrumentation contract drift"):
        rail._instrument_rom(ROM, static, drifted)
    instrumented = rail._instrument_rom(ROM, static, fixture)
    try:
        assert instrumented.read_bytes() != before
        assert ROM.read_bytes() == before
    finally:
        instrumented.unlink(missing_ok=True)
    overlapping = deepcopy(static)
    overlapping["instrumentation"]["actionScriptAddress"] = overlapping["instrumentation"][
        "generatedProbeAddress"
    ]
    overlapping_fixture = deepcopy(fixture)
    overlapping_fixture["instrumentation"] = overlapping["instrumentation"]
    with pytest.raises(ValueError, match="generated-input overlap"):
        rail._instrument_rom(ROM, overlapping, overlapping_fixture)


def test_rom_corruption_rejects_h1_join_before_instrumentation(tmp_path: Path) -> None:
    corrupted = tmp_path / "sf2-corrupted.bin"
    payload = bytearray(ROM.read_bytes())
    payload[0x5D28] ^= 1
    corrupted.write_bytes(payload)
    with pytest.raises(ValueError, match="ROM/H1 instruction drift"):
        build_map_block_copy_lifecycle_contract(corrupted, UPSTREAM)
    assert ROM.read_bytes() != corrupted.read_bytes()


def test_restoration_plans_are_precise_nonoverlapping_and_cover_generated_inputs() -> None:
    fixture = load_json(FIXTURE)
    inputs = _case_inputs(_static_from_fixture(fixture), fixture)
    assert len(inputs) == 10
    assert all("expected" not in row for row in inputs)
    for row in inputs:
        cells = row["restorationPlan"]
        assert cells
        assert len({cell["address"] for cell in cells}) == len(cells)
        assert all(cell["width"] in {1, 2, 4} for cell in cells)
    direct = next(row for row in inputs if row["id"] == "perform-matched-positive")
    assert {cell["address"] for cell in direct["restorationPlan"]} >= {
        fixture["instrumentation"]["generatedProbeAddress"],
        fixture["instrumentation"]["actionScriptAddress"],
        fixture["ram"]["savedRectangleMetadata"],
        fixture["ram"]["busyWord"],
        fixture["ram"]["updateToggle"],
    }


def test_semantic_rejection_cleans_observed_and_instrumented_outputs(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    fixture = load_json(FIXTURE)
    static = {**_static_from_fixture(fixture), "instrumentation": fixture["instrumentation"]}
    instrumented = rail.DERIVED_ROOT / "map-block-copy-lifecycle.instrumented.bin"
    observed = rail.DERIVED_ROOT / "map-block-copy-lifecycle.observed.json"
    instrumented.parent.mkdir(parents=True, exist_ok=True)
    instrumented.unlink(missing_ok=True)
    observed.unlink(missing_ok=True)

    monkeypatch.setattr(rail, "verify_runtime_contract", lambda *_: None)
    monkeypatch.setattr(rail, "build_map_block_copy_lifecycle_contract", lambda *_: static)
    monkeypatch.setattr(
        rail,
        "_instrument_rom",
        lambda *_: (instrumented.write_bytes(b"instrumented"), instrumented)[1],
    )
    monkeypatch.setattr(
        rail, "_with_instrumented_rom_database", lambda _path, _name, observe: observe()
    )
    monkeypatch.setattr(rail, "assert_observer_status", lambda *_args, **_kwargs: None)
    monkeypatch.setattr(rail, "run_observer", lambda **_: {"wrong": True})
    observed.write_text('{"wrong":true}', encoding="utf-8")
    with pytest.raises(ValueError, match="failed schema validation"):
        rail.verify_map_block_copy_lifecycle(ROM, UPSTREAM)
    assert not instrumented.exists()
    assert not observed.exists()


def test_success_cleanup_removes_only_session_instrumented_rom(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    fixture = load_json(FIXTURE)
    static = {**_static_from_fixture(fixture), "instrumentation": fixture["instrumentation"]}
    rows = _derive_cases(static, fixture)
    instrumented = rail.DERIVED_ROOT / "map-block-copy-lifecycle.instrumented.bin"
    instrumented.parent.mkdir(parents=True, exist_ok=True)
    instrumented.unlink(missing_ok=True)
    monkeypatch.setattr(rail, "verify_runtime_contract", lambda *_: None)
    monkeypatch.setattr(rail, "build_map_block_copy_lifecycle_contract", lambda *_: static)
    monkeypatch.setattr(
        rail,
        "_instrument_rom",
        lambda *_: (instrumented.write_bytes(b"instrumented"), instrumented)[1],
    )
    monkeypatch.setattr(
        rail, "_with_instrumented_rom_database", lambda _path, _name, observe: observe()
    )
    monkeypatch.setattr(rail, "assert_observer_status", lambda *_args, **_kwargs: None)
    monkeypatch.setattr(
        rail,
        "run_observer",
        lambda **_: {
            "system": "GEN",
            "core": fixture["emulator"]["core"],
            "id": fixture["id"],
            "mapTest": fixture["mapTestIndex"],
            "recordOrder": list(CASE_IDS),
            "records": rows,
        },
    )
    assert rail.verify_map_block_copy_lifecycle(ROM, UPSTREAM)["Status"] == "PASS"
    assert not instrumented.exists()


def test_schema_valid_observation_order_drift_rejects_at_golden_boundary(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    fixture = load_json(FIXTURE)
    static = {**_static_from_fixture(fixture), "instrumentation": fixture["instrumentation"]}
    rows = _derive_cases(static, fixture)
    instrumented = rail.DERIVED_ROOT / "map-block-copy-lifecycle.instrumented.bin"
    instrumented.parent.mkdir(parents=True, exist_ok=True)
    instrumented.unlink(missing_ok=True)
    monkeypatch.setattr(rail, "verify_runtime_contract", lambda *_: None)
    monkeypatch.setattr(rail, "build_map_block_copy_lifecycle_contract", lambda *_: static)
    monkeypatch.setattr(
        rail,
        "_instrument_rom",
        lambda *_: (instrumented.write_bytes(b"instrumented"), instrumented)[1],
    )
    monkeypatch.setattr(
        rail, "_with_instrumented_rom_database", lambda _path, _name, observe: observe()
    )
    monkeypatch.setattr(rail, "assert_observer_status", lambda *_args, **_kwargs: None)
    monkeypatch.setattr(
        rail,
        "run_observer",
        lambda **_: {
            "system": "GEN",
            "core": fixture["emulator"]["core"],
            "id": fixture["id"],
            "mapTest": fixture["mapTestIndex"],
            "recordOrder": list(reversed(CASE_IDS)),
            "records": list(reversed(rows)),
        },
    )
    with pytest.raises(ValueError, match="runtime matrix mismatch"):
        rail.verify_map_block_copy_lifecycle(ROM, UPSTREAM)
    assert not instrumented.exists()


def test_prelaunch_case_matrix_rejection_never_instruments_or_launches(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    fixture = load_json(FIXTURE)
    rejected = deepcopy(fixture)
    rejected["cases"].reverse()
    real_load = rail.load_json
    instrumented = False

    def fake_load(path: Path) -> object:
        return rejected if path == rail.FIXTURE else real_load(path)

    def unexpected_instrument(*_args: object) -> Path:
        nonlocal instrumented
        instrumented = True
        raise AssertionError("instrumentation must not run")

    monkeypatch.setattr(rail, "load_json", fake_load)
    monkeypatch.setattr(rail, "_instrument_rom", unexpected_instrument)
    with pytest.raises(ValueError, match="exact case ID/order/kind"):
        rail.verify_map_block_copy_lifecycle(ROM, UPSTREAM)
    assert not instrumented
