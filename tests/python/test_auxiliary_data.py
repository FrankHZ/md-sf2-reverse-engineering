from __future__ import annotations

import json
from collections.abc import Callable
from copy import deepcopy
from pathlib import Path
from typing import Any

import pytest

from sf2tool.h2 import auxiliary_data
from sf2tool.h2.auxiliary_data import build_auxiliary_data_inventory
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

UPSTREAM = repo_path("local/upstream/SF2DISASM")
OUTPUT_SCHEMA = repo_path("schemas/auxiliary-data-static.schema.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-auxiliary-data-static-fixture.schema.json")
FIXTURE_PATH = repo_path("tests/fixtures/h2/auxiliary-data-static-v1.json")


def _write_json(path: Path, value: object) -> Path:
    path.write_bytes((json.dumps(value, indent=2) + "\n").encode("utf-8"))
    return path


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
        row["sourcePath"] = "graphics/not-an-auxiliary-source.asm"
    elif mutation == "record-ids-type":
        row["recordIds"] = "auxiliary.data.not-a-list"
    elif mutation == "record-id-pattern":
        row["recordIds"] = ["auxiliary.data/not-a-valid-id"]
    elif mutation == "duplicate-record-id":
        row["recordIds"] = [row["recordIds"][0], row["recordIds"][0]]
    else:
        raise AssertionError(f"unknown output join-row mutation: {mutation}")


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_auxiliary_source_membership_join_keeps_eighty_records_over_sixty_three_paths() -> None:
    fixture = load_json(FIXTURE_PATH)
    output = build_auxiliary_data_inventory(UPSTREAM)
    expected_join = fixture["expected"]["indexedRecordsBySourcePath"]

    assert output["indexedRecordsBySourcePath"] == expected_join
    assert output["indexedSourcePaths"] == [row["sourcePath"] for row in expected_join]
    assert output["indexedRecordIds"] == sorted(
        record_id for row in expected_join for record_id in row["recordIds"]
    )
    assert output["summary"]["indexedRecordCount"] == len(output["indexedRecordIds"]) == 80
    assert output["summary"]["indexedFileCount"] == len(output["indexedSourcePaths"]) == 63
    assert {
        "entity.actions.eas-493a2",
        "map.entity-action-bridge.eas-idle",
        "map.entity-lifecycle-presentation.eas-idle",
    } <= set(output["indexedRecordIds"])


def test_auxiliary_index_schemas_leave_golden_values_to_fixture_and_verifier() -> None:
    output_schema = load_json(OUTPUT_SCHEMA)
    fixture_schema = load_json(FIXTURE_SCHEMA)

    for field in ("indexedRecordIds", "indexedSourcePaths", "facts"):
        assert "const" not in output_schema["properties"][field]
    assert "const" not in output_schema["properties"]["runtimeQuestions"]
    assert "const" not in output_schema["properties"]["scope"]
    assert "const" not in output_schema["properties"]["excludedPaths"]
    assert "const" not in output_schema["properties"]["exclusions"]
    for field in ("repository", "commit"):
        assert "const" not in output_schema["definitions"]["upstream"]["properties"][field]
    for field in ("sourceLineCount", "statementCount", "globalLabelCount"):
        assert "const" not in output_schema["definitions"]["summary"]["properties"][field]
    summary_properties = output_schema["definitions"]["summary"]["properties"]
    assert "const" not in summary_properties["indexedRecordCount"]
    assert "const" not in summary_properties["indexedFileCount"]
    assert "const" not in fixture_schema["properties"]["romSha256"]
    assert "const" not in fixture_schema["properties"]["upstreamCommit"]
    assert "const" not in fixture_schema["properties"]["expected"]["properties"]["facts"]
    assert "const" not in fixture_schema["properties"]["expected"]["properties"]["runtimeQuestions"]
    for field, cardinality in (("indexedRecordIds", 80), ("indexedSourcePaths", 63)):
        field_schema = output_schema["properties"][field]
        assert field_schema["minItems"] == field_schema["maxItems"] == cardinality
        assert field_schema["uniqueItems"] is True
    for schema in (output_schema, fixture_schema):
        relation_schema = schema["properties"].get("indexedRecordsBySourcePath")
        if relation_schema is None:
            relation_schema = schema["properties"]["expected"]["properties"][
                "indexedRecordsBySourcePath"
            ]
        assert relation_schema["minItems"] == relation_schema["maxItems"] == 63
    for schema in (output_schema, fixture_schema):
        row = schema["definitions"]["indexedRecordsBySourcePath"]
        assert row["additionalProperties"] is False
        assert row["required"] == ["sourcePath", "recordIds"]
        assert row["properties"]["recordIds"]["uniqueItems"] is True


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
def test_auxiliary_fixture_schema_rejects_structural_join_mutations(mutation: str) -> None:
    fixture = deepcopy(load_json(FIXTURE_PATH))
    _mutate_join_row(fixture["expected"]["indexedRecordsBySourcePath"][0], mutation)

    with pytest.raises(ValueError, match="indexedRecordsBySourcePath"):
        validate_json(fixture, FIXTURE_SCHEMA, owner=f"auxiliary fixture {mutation}")


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
def test_auxiliary_output_schema_rejects_structural_join_mutations(mutation: str) -> None:
    output = build_auxiliary_data_inventory(UPSTREAM)
    _mutate_join_row(output["indexedRecordsBySourcePath"][0], mutation)

    with pytest.raises(ValueError, match="indexedRecordsBySourcePath"):
        validate_json(output, OUTPUT_SCHEMA, owner=f"auxiliary output {mutation}")


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("field", "value"),
    [
        ("indexedRecordIds", "auxiliary.data/not-a-valid-id"),
        ("indexedSourcePaths", "graphics/not-an-auxiliary-source.asm"),
    ],
)
def test_auxiliary_output_schema_rejects_malformed_join_lists(field: str, value: str) -> None:
    output = build_auxiliary_data_inventory(UPSTREAM)
    output[field][0] = value

    with pytest.raises(ValueError, match=field):
        validate_json(output, OUTPUT_SCHEMA, owner=f"malformed auxiliary {field}")


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize("field", ["indexedRecordIds", "indexedSourcePaths"])
def test_auxiliary_output_schema_rejects_duplicate_join_lists(field: str) -> None:
    output = build_auxiliary_data_inventory(UPSTREAM)
    output[field][1] = output[field][0]

    with pytest.raises(ValueError, match=field):
        validate_json(output, OUTPUT_SCHEMA, owner=f"duplicate auxiliary {field}")


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
            "relation-record-id",
            lambda output: output["indexedRecordsBySourcePath"][0].update(
                recordIds=["auxiliary.data.wrong-but-schema-valid"]
            ),
            "indexedRecordIds relation drift",
        ),
        (
            "relation-source-path",
            lambda output: output["indexedRecordsBySourcePath"][0].update(
                sourcePath="data/graphics/wrong-but-schema-valid.asm"
            ),
            "indexedSourcePaths relation order drift",
        ),
        (
            "relation-order",
            lambda output: output["indexedRecordsBySourcePath"].reverse(),
            "indexedSourcePaths relation order drift",
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
            lambda output: output["summary"].update(indexedRecordCount=79),
            "summary indexedRecordCount relation drift",
        ),
        (
            "summary-file-count",
            lambda output: output["summary"].update(indexedFileCount=62),
            "summary indexedFileCount relation drift",
        ),
    ],
)
def test_auxiliary_verifier_rejects_schema_valid_join_invariant_mutations_before_write(
    monkeypatch: pytest.MonkeyPatch,
    tmp_path: Path,
    suffix: str,
    mutate: Callable[[dict[str, Any]], None],
    error: str,
) -> None:
    output = build_auxiliary_data_inventory(UPSTREAM)
    mutate(output)
    validate_json(output, OUTPUT_SCHEMA, owner=f"schema-valid auxiliary {suffix}")
    output_path = tmp_path / f"auxiliary-{suffix}-output.json"
    monkeypatch.setattr(auxiliary_data, "build_auxiliary_data_inventory", lambda _: output)

    with pytest.raises(ValueError, match=error):
        auxiliary_data.verify_auxiliary_data_inventory(UPSTREAM, output_path=output_path)
    assert not output_path.exists()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("suffix", "mutate", "error"),
    [
        (
            "record-id",
            lambda fixture: fixture["expected"]["indexedRecordsBySourcePath"][0].update(
                recordIds=["auxiliary.data.wrong-but-schema-valid"]
            ),
            "indexedRecordsBySourcePath drift",
        ),
        (
            "source-path",
            lambda fixture: fixture["expected"]["indexedRecordsBySourcePath"][0].update(
                sourcePath="data/graphics/wrong-but-schema-valid.asm"
            ),
            "indexedRecordsBySourcePath drift",
        ),
        (
            "order",
            lambda fixture: fixture["expected"]["indexedRecordsBySourcePath"].reverse(),
            "indexedRecordsBySourcePath drift",
        ),
        (
            "fact-value",
            lambda fixture: fixture["expected"]["facts"].update(privateIncbinReferenceCount=0),
            "facts drift",
        ),
        (
            "runtime-question-order",
            lambda fixture: fixture["expected"]["runtimeQuestions"].reverse(),
            "runtimeQuestions drift",
        ),
        (
            "upstream-commit",
            lambda fixture: fixture.update(upstreamCommit="0" * 40),
            "provenance drift",
        ),
        (
            "rom-sha256",
            lambda fixture: fixture.update(romSha256="0" * 64),
            "provenance drift",
        ),
    ],
)
def test_auxiliary_verifier_rejects_schema_valid_golden_mutations_before_write(
    monkeypatch: pytest.MonkeyPatch,
    tmp_path: Path,
    suffix: str,
    mutate: Callable[[dict[str, Any]], None],
    error: str,
) -> None:
    fixture = deepcopy(load_json(FIXTURE_PATH))
    mutate(fixture)
    fixture_path = _write_json(tmp_path / f"auxiliary-{suffix}.json", fixture)
    validate_json(fixture, FIXTURE_SCHEMA, owner=f"schema-valid auxiliary {suffix}")
    output_path = tmp_path / f"auxiliary-{suffix}-output.json"
    monkeypatch.setattr(auxiliary_data, "FIXTURE", fixture_path)

    with pytest.raises(ValueError, match=error):
        auxiliary_data.verify_auxiliary_data_inventory(UPSTREAM, output_path=output_path)
    assert not output_path.exists()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("suffix", "mutate", "error"),
    [
        (
            "upstream-commit",
            lambda output: output["upstream"].update(commit="0" * 40),
            "provenance drift",
        ),
        (
            "representative-address",
            lambda output: output["representativeAddresses"].update(pt_Backgrounds=0),
            "H1 address drift",
        ),
        (
            "fact-value",
            lambda output: output["facts"].update(privateIncbinReferenceCount=0),
            "facts drift",
        ),
        (
            "runtime-question-order",
            lambda output: output["runtimeQuestions"].reverse(),
            "runtimeQuestions drift",
        ),
        (
            "exclusion-value",
            lambda output: output["exclusions"].update(
                {next(iter(output["exclusions"])): "wrong-but-schema-valid"}
            ),
            "canonical hash drift",
        ),
    ],
)
def test_auxiliary_verifier_rejects_schema_valid_output_value_mutations_before_write(
    monkeypatch: pytest.MonkeyPatch,
    tmp_path: Path,
    suffix: str,
    mutate: Callable[[dict[str, Any]], None],
    error: str,
) -> None:
    output = build_auxiliary_data_inventory(UPSTREAM)
    mutate(output)
    validate_json(output, OUTPUT_SCHEMA, owner=f"schema-valid auxiliary {suffix}")
    output_path = tmp_path / f"auxiliary-{suffix}-output.json"
    monkeypatch.setattr(auxiliary_data, "build_auxiliary_data_inventory", lambda _: output)

    with pytest.raises(ValueError, match=error):
        auxiliary_data.verify_auxiliary_data_inventory(UPSTREAM, output_path=output_path)
    assert not output_path.exists()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize("mutation", ["outside-scope", "new-in-scope"])
def test_auxiliary_source_membership_join_rejects_index_drift_before_write(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path, mutation: str
) -> None:
    index = deepcopy(load_json(repo_path("manifests/research-index.json")))
    records = index["records"]
    original = next(
        record for record in records if record["id"] == "auxiliary.data.pt-backgrounds"
    )
    if mutation == "outside-scope":
        original["sourcePath"] = "data/graphics/not-an-inventory-path.asm"
    else:
        added = deepcopy(original)
        added["id"] = "auxiliary.data.future-source-membership-record"
        records.append(added)
    index_path = _write_json(tmp_path / f"research-index-{mutation}.json", index)
    output_path = tmp_path / f"auxiliary-{mutation}-output.json"
    monkeypatch.setattr(auxiliary_data, "RESEARCH_INDEX", index_path)

    with pytest.raises(ValueError, match="indexedRecordIds"):
        auxiliary_data.verify_auxiliary_data_inventory(UPSTREAM, output_path=output_path)
    assert not output_path.exists()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("field", "value", "error"),
    [
        ("summary", {"indexedRecordCount": 81}, "summary drift"),
        ("outputSha256", "0" * 64, "canonical hash drift"),
    ],
)
def test_auxiliary_verifier_rejects_stale_manifest_before_write(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path, field: str, value: object, error: str
) -> None:
    manifest = deepcopy(load_json(auxiliary_data.MANIFEST))
    if field == "summary":
        manifest["summary"].update(value)
    else:
        manifest[field] = value
    manifest_path = _write_json(tmp_path / f"auxiliary-{field}-manifest.json", manifest)
    output_path = tmp_path / f"auxiliary-{field}-output.json"
    monkeypatch.setattr(auxiliary_data, "MANIFEST", manifest_path)

    with pytest.raises(ValueError, match=error):
        auxiliary_data.verify_auxiliary_data_inventory(UPSTREAM, output_path=output_path)
    assert not output_path.exists()
