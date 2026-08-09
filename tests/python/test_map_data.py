from __future__ import annotations

import json
from collections.abc import Callable
from copy import deepcopy
from pathlib import Path
from typing import Any

import pytest

from sf2tool.h2 import map_data
from sf2tool.h2.map_data import build_map_data_inventory
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

UPSTREAM = repo_path("local/upstream/SF2DISASM")
OUTPUT_SCHEMA = repo_path("schemas/map-data-static.schema.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-map-data-static-fixture.schema.json")
FIXTURE_PATH = repo_path("tests/fixtures/h2/map-data-static-v1.json")


def _write_json(path: Path, value: object) -> Path:
    path.write_bytes((json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode("utf-8"))
    return path


def _source_record(record_id: str, source_path: str) -> dict[str, Any]:
    index = load_json(repo_path("manifests/research-index.json"))
    record = deepcopy(
        next(item for item in index["records"] if item["id"] == "map.data.ms-map45-entities")
    )
    record["id"] = record_id
    record["sourcePath"] = source_path
    return record


def _schema_consts(value: Any, trail: str = "$") -> dict[str, Any]:
    constants: dict[str, Any] = {}
    if isinstance(value, dict):
        if "const" in value:
            constants[trail] = value["const"]
        for key, child in value.items():
            constants.update(_schema_consts(child, f"{trail}.{key}"))
    elif isinstance(value, list):
        for index, child in enumerate(value):
            constants.update(_schema_consts(child, f"{trail}[{index}]"))
    return constants


def _replace_record_id(value: list[str], old: str, new: str) -> list[str]:
    return [new if record_id == old else record_id for record_id in value]


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_map_data_source_path_membership_keeps_731_records_over_727_paths() -> None:
    fixture = load_json(FIXTURE_PATH)
    output = build_map_data_inventory(UPSTREAM)
    expected = fixture["expected"]

    assert output["indexedRecordIds"] == expected["indexedRecordIds"]
    assert output["indexedSourcePaths"] == expected["indexedSourcePaths"]
    assert output["indexedRecordsBySourcePath"] == expected["indexedRecordsBySourcePath"]
    assert output["summary"]["indexedRecordCount"] == len(output["indexedRecordIds"]) == 731
    assert output["summary"]["indexedFileCount"] == len(output["indexedSourcePaths"]) == 727
    assert output["indexedSourcePaths"] == [
        row["sourcePath"] for row in output["indexedRecordsBySourcePath"]
    ]
    assert output["indexedRecordIds"] == sorted(
        record_id
        for row in output["indexedRecordsBySourcePath"]
        for record_id in row["recordIds"]
    )

    relation = {
        row["sourcePath"]: row["recordIds"]
        for row in output["indexedRecordsBySourcePath"]
        if len(row["recordIds"]) > 1
    }
    assert relation == {
        "data/maps/entries/map45/mapsetups/s1_entities.asm": [
            "entity.actions.eas-5ffc4",
            "entity.actions.eas-5ffc8",
            "map.data.ms-map45-entities",
        ],
        "data/maps/entries/map55/mapsetups/scripts.asm": [
            "entity.actions.eas-5e2c4",
            "map.data.cs-5e27c",
        ],
        "data/maps/entries/map59/mapsetups/s6_initfunction.asm": [
            "entity.actions.eas-5ef46",
            "map.data.ms-map59-initfunction",
        ],
    }
    assert {
        "entity.actions.eas-5ffc4",
        "entity.actions.eas-5ffc8",
        "entity.actions.eas-5e2c4",
        "entity.actions.eas-5ef46",
    } <= set(output["indexedRecordIds"])


def test_map_data_schemas_are_closed_and_keep_golden_corpora_out_of_consts() -> None:
    output_schema = load_json(OUTPUT_SCHEMA)
    fixture_schema = load_json(FIXTURE_SCHEMA)

    assert output_schema["$id"] == "urn:sf2:schema:h2:map-data-static"
    assert (
        fixture_schema["properties"]["table"]["$ref"]
        == "urn:sf2:schema:h2:map-data-static#/definitions/representativeAddresses"
    )
    for schema in (output_schema, fixture_schema):
        assert all(
            not isinstance(constant, (dict, list))
            for constant in _schema_consts(schema).values()
        )
    for field in ("indexedRecordIds", "indexedSourcePaths", "indexedRecordsBySourcePath"):
        assert "const" not in output_schema["properties"][field]
        assert "const" not in fixture_schema["definitions"]["expected"]["properties"][field]
    assert "const" not in output_schema["definitions"]["summary"]["properties"][
        "indexedRecordCount"
    ]
    assert output_schema["definitions"]["indexedRecordsBySourcePath"][
        "additionalProperties"
    ] is False
    assert output_schema["definitions"]["sourceFile"]["additionalProperties"] is False
    assert output_schema["definitions"]["facts"]["additionalProperties"] is False


def _mutate_join_row(row: dict[str, Any], mutation: str) -> None:
    if mutation == "missing":
        del row["sourcePath"]
    elif mutation == "extra":
        row["unexpected"] = True
    elif mutation == "renamed":
        row["renamedSourcePath"] = row.pop("sourcePath")
    elif mutation == "source-type":
        row["sourcePath"] = 7
    elif mutation == "source-pattern":
        row["sourcePath"] = "data/not-maps/not-a-map-source.asm"
    elif mutation == "record-ids-type":
        row["recordIds"] = "map.data.not-a-list"
    elif mutation == "record-id-pattern":
        row["recordIds"] = ["map.data/not-a-valid-id"]
    elif mutation == "duplicate-record-id":
        row["recordIds"] = [row["recordIds"][0], row["recordIds"][0]]
    else:
        raise AssertionError(f"unknown join-row mutation: {mutation}")


@pytest.mark.parametrize(
    "mutation",
    [
        "missing",
        "extra",
        "renamed",
        "source-type",
        "source-pattern",
        "record-ids-type",
        "record-id-pattern",
        "duplicate-record-id",
    ],
)
def test_map_data_fixture_schema_rejects_structural_join_mutations(mutation: str) -> None:
    fixture = deepcopy(load_json(FIXTURE_PATH))
    _mutate_join_row(fixture["expected"]["indexedRecordsBySourcePath"][0], mutation)

    with pytest.raises(ValueError, match="indexedRecordsBySourcePath"):
        validate_json(fixture, FIXTURE_SCHEMA, owner=f"map-data fixture {mutation}")


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    "mutation",
    [
        "missing",
        "extra",
        "renamed",
        "source-type",
        "source-pattern",
        "record-ids-type",
        "record-id-pattern",
        "duplicate-record-id",
    ],
)
def test_map_data_output_schema_rejects_structural_join_mutations(mutation: str) -> None:
    output = build_map_data_inventory(UPSTREAM)
    _mutate_join_row(output["indexedRecordsBySourcePath"][0], mutation)

    with pytest.raises(ValueError, match="indexedRecordsBySourcePath"):
        validate_json(output, OUTPUT_SCHEMA, owner=f"map-data output {mutation}")


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_map_data_schemas_recursively_reject_nested_source_inventory_mutations() -> None:
    output = build_map_data_inventory(UPSTREAM)
    output["files"][0]["unexpected"] = True
    with pytest.raises(ValueError, match="files"):
        validate_json(output, OUTPUT_SCHEMA, owner="map-data output nested extra")

    fixture = deepcopy(load_json(FIXTURE_PATH))
    fixture["expected"]["sourceInventory"]["includeEdges"][0]["unexpected"] = True
    with pytest.raises(ValueError, match="sourceInventory"):
        validate_json(fixture, FIXTURE_SCHEMA, owner="map-data fixture nested extra")


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("suffix", "mutate", "error"),
    [
        (
            "record-id-order",
            lambda output: output["indexedRecordIds"].reverse(),
            "indexedRecordIds relation drift",
        ),
        (
            "source-path-order",
            lambda output: output["indexedSourcePaths"].reverse(),
            "indexedSourcePaths relation order drift",
        ),
        (
            "relation-record-id-order",
            lambda output: next(
                row
                for row in output["indexedRecordsBySourcePath"]
                if len(row["recordIds"]) > 1
            )["recordIds"].reverse(),
            "indexed relation record-ID order drift",
        ),
        (
            "relation-row-order",
            lambda output: output["indexedRecordsBySourcePath"].reverse(),
            "indexed relation source-path order drift",
        ),
        (
            "duplicate-relation-source-path",
            lambda output: output["indexedRecordsBySourcePath"][1].update(
                sourcePath=output["indexedRecordsBySourcePath"][0]["sourcePath"]
            ),
            "indexed relation duplicate source path",
        ),
        (
            "duplicate-relation-record-id",
            lambda output: output["indexedRecordsBySourcePath"][1]["recordIds"].append(
                output["indexedRecordsBySourcePath"][0]["recordIds"][0]
            ),
            "indexed relation duplicate record ID",
        ),
        (
            "summary-record-count",
            lambda output: output["summary"].update(indexedRecordCount=730),
            "summary indexedRecordCount relation drift",
        ),
        (
            "summary-file-count",
            lambda output: output["summary"].update(indexedFileCount=726),
            "summary indexedFileCount relation drift",
        ),
    ],
)
def test_map_data_verifier_rejects_schema_valid_join_mutations_before_write(
    monkeypatch: pytest.MonkeyPatch,
    tmp_path: Path,
    suffix: str,
    mutate: Callable[[dict[str, Any]], None],
    error: str,
) -> None:
    output = build_map_data_inventory(UPSTREAM)
    mutate(output)
    validate_json(output, OUTPUT_SCHEMA, owner=f"schema-valid map-data {suffix}")
    destination = tmp_path / f"map-data-{suffix}.json"
    monkeypatch.setattr(map_data, "build_map_data_inventory", lambda _: output)

    with pytest.raises(ValueError, match=error):
        map_data.verify_map_data_inventory(UPSTREAM, output_path=destination)
    assert not destination.exists()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_map_data_verifier_rejects_coordinated_output_and_fixture_membership_drift_before_write(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    output = build_map_data_inventory(UPSTREAM)
    fixture = deepcopy(load_json(FIXTURE_PATH))
    old_record_id = output["indexedRecordsBySourcePath"][0]["recordIds"][0]
    new_record_id = "map.data.coordinated-membership-drift"
    output["indexedRecordsBySourcePath"][0]["recordIds"][0] = new_record_id
    output["indexedRecordIds"] = sorted(
        _replace_record_id(output["indexedRecordIds"], old_record_id, new_record_id)
    )
    fixture["expected"]["indexedRecordsBySourcePath"][0]["recordIds"][0] = new_record_id
    fixture["expected"]["indexedRecordIds"] = sorted(
        _replace_record_id(fixture["expected"]["indexedRecordIds"], old_record_id, new_record_id)
    )
    validate_json(output, OUTPUT_SCHEMA, owner="coordinated map-data output drift")
    validate_json(fixture, FIXTURE_SCHEMA, owner="coordinated map-data fixture drift")
    fixture_path = _write_json(tmp_path / "coordinated-fixture.json", fixture)
    destination = tmp_path / "coordinated-output.json"
    monkeypatch.setattr(map_data, "FIXTURE", fixture_path)
    monkeypatch.setattr(map_data, "build_map_data_inventory", lambda _: output)

    with pytest.raises(ValueError, match="current research-index indexedRecordIds drift"):
        map_data.verify_map_data_inventory(UPSTREAM, output_path=destination)
    assert not destination.exists()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_map_data_verifier_rejects_coordinated_output_and_fixture_path_drift_before_write(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    output = build_map_data_inventory(UPSTREAM)
    fixture = deepcopy(load_json(FIXTURE_PATH))
    indexed_paths = output["indexedSourcePaths"]
    unindexed_paths = set(output["strictReach"]["includeSiteOnlyPaths"])
    replacement: tuple[int, str] | None = None
    for index in range(1, len(indexed_paths) - 1):
        candidate = next(
            (
                path
                for path in sorted(unindexed_paths)
                if indexed_paths[index - 1] < path < indexed_paths[index + 1]
            ),
            None,
        )
        if candidate is not None:
            replacement = (index, candidate)
            break
    assert replacement is not None
    row_index, replacement_path = replacement
    output["indexedRecordsBySourcePath"][row_index]["sourcePath"] = replacement_path
    output["indexedSourcePaths"][row_index] = replacement_path
    fixture["expected"]["indexedRecordsBySourcePath"][row_index][
        "sourcePath"
    ] = replacement_path
    fixture["expected"]["indexedSourcePaths"][row_index] = replacement_path
    validate_json(output, OUTPUT_SCHEMA, owner="coordinated map-data output path drift")
    validate_json(fixture, FIXTURE_SCHEMA, owner="coordinated map-data fixture path drift")
    fixture_path = _write_json(tmp_path / "coordinated-path-fixture.json", fixture)
    destination = tmp_path / "coordinated-path-output.json"
    monkeypatch.setattr(map_data, "FIXTURE", fixture_path)
    monkeypatch.setattr(map_data, "build_map_data_inventory", lambda _: output)

    with pytest.raises(ValueError, match="current research-index indexedSourcePaths drift"):
        map_data.verify_map_data_inventory(UPSTREAM, output_path=destination)
    assert not destination.exists()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("suffix", "mutate", "error"),
    [
        (
            "record-id",
            lambda fixture: fixture["expected"]["indexedRecordsBySourcePath"][0].update(
                recordIds=["map.data.wrong-but-schema-valid"]
            ),
            "fixture indexedRecordsBySourcePath drift",
        ),
        (
            "source-path",
            lambda fixture: fixture["expected"]["indexedRecordsBySourcePath"][0].update(
                sourcePath="data/maps/entries/map00/mapsetups/s1_entities.asm"
            ),
            "fixture indexedRecordsBySourcePath drift",
        ),
        (
            "source-path-order",
            lambda fixture: fixture["expected"]["indexedSourcePaths"].reverse(),
            "fixture indexedSourcePaths drift",
        ),
        (
            "relation-order",
            lambda fixture: fixture["expected"]["indexedRecordsBySourcePath"].reverse(),
            "fixture indexedRecordsBySourcePath drift",
        ),
        (
            "source-inventory",
            lambda fixture: fixture["expected"]["sourceInventory"]["directLayoutPaths"].reverse(),
            "source inventory directLayoutPaths drift",
        ),
        (
            "representative-symbol",
            lambda fixture: fixture["expected"]["sourceInventory"]["representativeSymbols"].update(
                {"data/maps/entries.asm": "MapSetups"}
            ),
            "source inventory representativeSymbols drift",
        ),
        (
            "fact-value",
            lambda fixture: fixture["expected"]["facts"].update(mapContentParsed=False),
            "fixture facts drift",
        ),
        (
            "runtime-question-order",
            lambda fixture: fixture["expected"]["runtimeQuestions"].reverse(),
            "fixture runtimeQuestions drift",
        ),
    ],
)
def test_map_data_verifier_rejects_fixture_exactness_drift_before_write(
    monkeypatch: pytest.MonkeyPatch,
    tmp_path: Path,
    suffix: str,
    mutate: Callable[[dict[str, Any]], None],
    error: str,
) -> None:
    fixture = deepcopy(load_json(FIXTURE_PATH))
    mutate(fixture)
    fixture_path = _write_json(tmp_path / f"fixture-{suffix}.json", fixture)
    validate_json(fixture, FIXTURE_SCHEMA, owner=f"schema-valid map-data fixture {suffix}")
    destination = tmp_path / f"fixture-{suffix}-output.json"
    monkeypatch.setattr(map_data, "FIXTURE", fixture_path)

    with pytest.raises(ValueError, match=error):
        map_data.verify_map_data_inventory(UPSTREAM, output_path=destination)
    assert not destination.exists()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("field", "value", "error"),
    [
        ("upstreamCommit", "0" * 40, "fixture upstream provenance drift"),
        ("romSha256", "0" * 64, "fixture ROM provenance drift"),
    ],
)
def test_map_data_verifier_rejects_format_valid_fixture_provenance_drift_before_write(
    monkeypatch: pytest.MonkeyPatch,
    tmp_path: Path,
    field: str,
    value: str,
    error: str,
) -> None:
    fixture = deepcopy(load_json(FIXTURE_PATH))
    fixture[field] = value
    fixture_path = _write_json(tmp_path / f"fixture-{field}.json", fixture)
    validate_json(fixture, FIXTURE_SCHEMA, owner=f"schema-valid map-data fixture {field}")
    destination = tmp_path / f"fixture-{field}-output.json"
    monkeypatch.setattr(map_data, "FIXTURE", fixture_path)

    with pytest.raises(ValueError, match=error):
        map_data.verify_map_data_inventory(UPSTREAM, output_path=destination)
    assert not destination.exists()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("suffix", "mutate", "error"),
    [
        (
            "repository",
            lambda output: output["upstream"].update(repository="https://invalid.example/SF2DISASM"),
            "upstream repository provenance drift",
        ),
        (
            "commit",
            lambda output: output["upstream"].update(commit="0" * 40),
            "upstream commit provenance drift",
        ),
        (
            "representative-address",
            lambda output: output["representativeAddresses"].update(pt_MapData=0),
            "H1 address drift",
        ),
    ],
)
def test_map_data_verifier_rejects_schema_valid_output_provenance_and_h1_drift_before_write(
    monkeypatch: pytest.MonkeyPatch,
    tmp_path: Path,
    suffix: str,
    mutate: Callable[[dict[str, Any]], None],
    error: str,
) -> None:
    output = build_map_data_inventory(UPSTREAM)
    mutate(output)
    validate_json(output, OUTPUT_SCHEMA, owner=f"schema-valid map-data output {suffix}")
    destination = tmp_path / f"output-{suffix}.json"
    monkeypatch.setattr(map_data, "build_map_data_inventory", lambda _: output)

    with pytest.raises(ValueError, match=error):
        map_data.verify_map_data_inventory(UPSTREAM, output_path=destination)
    assert not destination.exists()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_map_data_source_path_membership_accepts_an_in_scope_record_before_fixture_check(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    index = deepcopy(load_json(repo_path("manifests/research-index.json")))
    index["records"].append(
        _source_record(
            "independent.owner.additional-map-source-record",
            "data/maps/entries/map45/mapsetups/s1_entities.asm",
        )
    )
    index_path = _write_json(tmp_path / "research-index-in-scope.json", index)
    monkeypatch.setattr(map_data, "RESEARCH_INDEX", index_path)
    output = build_map_data_inventory(UPSTREAM)
    assert output["summary"]["indexedRecordCount"] == 732
    assert output["summary"]["indexedFileCount"] == 727
    assert "independent.owner.additional-map-source-record" in output["indexedRecordIds"]
    destination = tmp_path / "in-scope-output.json"

    with pytest.raises(ValueError, match="fixture indexedRecordIds drift"):
        map_data.verify_map_data_inventory(UPSTREAM, output_path=destination)
    assert not destination.exists()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_map_data_source_path_membership_excludes_outside_root_records(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    index = deepcopy(load_json(repo_path("manifests/research-index.json")))
    index["records"].append(
        _source_record(
            "independent.owner.outside-map-root-record",
            "data/not-maps/entries.asm",
        )
    )
    index_path = _write_json(tmp_path / "research-index-outside-root.json", index)
    monkeypatch.setattr(map_data, "RESEARCH_INDEX", index_path)
    output = build_map_data_inventory(UPSTREAM)
    assert output["summary"]["indexedRecordCount"] == 731
    assert "independent.owner.outside-map-root-record" not in output["indexedRecordIds"]
    destination = tmp_path / "outside-root-output.json"

    result = map_data.verify_map_data_inventory(UPSTREAM, output_path=destination)
    assert result["Status"] == "PASS"
    assert destination.is_file()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_map_data_rejects_under_root_index_record_missing_from_discovered_inventory(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    index = deepcopy(load_json(repo_path("manifests/research-index.json")))
    index["records"].append(
        _source_record(
            "independent.owner.missing-map-source-record",
            "data/maps/not-discovered/future.asm",
        )
    )
    index_path = _write_json(tmp_path / "research-index-missing-source.json", index)
    monkeypatch.setattr(map_data, "RESEARCH_INDEX", index_path)

    with pytest.raises(ValueError, match="absent from the discovered inventory"):
        build_map_data_inventory(UPSTREAM)


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("suffix", "mutate", "error"),
    [
        (
            "summary",
            lambda manifest: manifest["summary"].update(indexedRecordCount=730),
            "summary drift",
        ),
        (
            "digest",
            lambda manifest: manifest.update(outputSha256="0" * 64),
            "canonical hash drift",
        ),
    ],
)
def test_map_data_verifier_rejects_stale_manifest_before_write(
    monkeypatch: pytest.MonkeyPatch,
    tmp_path: Path,
    suffix: str,
    mutate: Callable[[dict[str, Any]], None],
    error: str,
) -> None:
    manifest = deepcopy(load_json(map_data.MANIFEST))
    mutate(manifest)
    manifest_path = _write_json(tmp_path / f"manifest-{suffix}.json", manifest)
    destination = tmp_path / f"manifest-{suffix}-output.json"
    monkeypatch.setattr(map_data, "MANIFEST", manifest_path)

    with pytest.raises(ValueError, match=error):
        map_data.verify_map_data_inventory(UPSTREAM, output_path=destination)
    assert not destination.exists()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_map_data_build_does_not_read_the_golden_fixture(monkeypatch: pytest.MonkeyPatch) -> None:
    monkeypatch.setattr(map_data, "FIXTURE", Path("does-not-exist.json"))
    output = build_map_data_inventory(UPSTREAM)
    assert output["summary"]["indexedRecordCount"] == 731
    assert output["summary"]["indexedFileCount"] == 727
