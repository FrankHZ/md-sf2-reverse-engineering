from __future__ import annotations

import json
from copy import deepcopy
from pathlib import Path
from typing import Any

import pytest

from sf2tool.h2 import scripting
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

UPSTREAM = repo_path("local/upstream/SF2DISASM")
OUTPUT_SCHEMA = repo_path("schemas/common-scripting-static.schema.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-common-scripting-static-fixture.schema.json")
FIXTURE_PATH = repo_path("tests/fixtures/h2/common-scripting-static-v1.json")


def _write_json(path: Path, value: object) -> Path:
    path.write_bytes((json.dumps(value, indent=2) + "\n").encode("utf-8"))
    return path


def _write_fixture(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path, fixture: dict[str, Any]
) -> Path:
    path = _write_json(tmp_path / "common-scripting-fixture.json", fixture)
    monkeypatch.setattr(scripting, "FIXTURE", path)
    return path


def _write_fixture_schema(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path, schema: dict[str, Any]
) -> Path:
    path = _write_json(tmp_path / "common-scripting-fixture.schema.json", schema)
    monkeypatch.setattr(scripting, "FIXTURE_SCHEMA", path)
    return path


def _source_record(record_id: str, source_path: str) -> dict[str, Any]:
    index = load_json(repo_path("manifests/research-index.json"))
    record = deepcopy(
        next(item for item in index["records"] if item["id"] == "scripting.map.ms-empty")
    )
    record["id"] = record_id
    record["sourcePath"] = source_path
    return record


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_scripting_recursive_source_membership_keeps_126_records_over_28_paths() -> None:
    fixture = load_json(FIXTURE_PATH)
    output = scripting.build_scripting_inventory(UPSTREAM)
    expected = fixture["expected"]

    for field in (
        "indexedRecordIds",
        "indexedSourcePaths",
        "indexedRecordsBySourcePath",
    ):
        assert output[field] == expected[field]
    assert output["summary"]["indexedRecordCount"] == len(output["indexedRecordIds"]) == 126
    assert output["summary"]["indexedFileCount"] == len(output["indexedSourcePaths"]) == 28
    assert output["summary"]["fileCount"] == 29
    assert output["summary"]["unlabeledFileCount"] == 1
    assert output["coreFacts"]["mapScript"]["commandCount"] == 90
    assert output["coreFacts"]["entityScript"]["commandCount"] == 80
    assert output["indexedRecordsBySourcePath"][17]["sourcePath"].endswith(
        "map/mapscriptengine_1.asm"
    )
    assert len(output["indexedRecordsBySourcePath"][17]["recordIds"]) == 58
    assert len(output["indexedRecordsBySourcePath"][19]["recordIds"]) == 16
    assert len(output["indexedRecordsBySourcePath"][23]["recordIds"]) == 12


def test_scripting_schemas_leave_record_corpus_to_fixture_and_verifier() -> None:
    output_schema = load_json(OUTPUT_SCHEMA)
    fixture_schema = load_json(FIXTURE_SCHEMA)
    output_summary = output_schema["definitions"]["summary"]["properties"]
    fixture_expected = fixture_schema["properties"]["expected"]["properties"]

    for field in (
        "indexedRecordIds",
        "indexedSourcePaths",
        "indexedRecordsBySourcePath",
    ):
        assert "const" not in output_schema["properties"][field]
        assert "const" not in fixture_expected[field]
    assert output_schema["properties"]["indexedRecordIds"].get("maxItems") != 126
    assert fixture_expected["indexedRecordIds"].get("maxItems") != 126
    assert output_summary["fileCount"] == {"const": 29}
    assert output_summary["indexedFileCount"] == {"const": 28}
    assert output_summary["unlabeledFileCount"] == {"const": 1}
    assert output_schema["properties"]["indexedSourcePaths"]["minItems"] == 28
    assert output_schema["properties"]["indexedSourcePaths"]["maxItems"] == 28
    assert output_schema["definitions"]["dispatchTables"]["properties"]["mapScript"] == {
        "type": "array",
        "minItems": 90,
        "maxItems": 90,
        "items": {"$ref": "#/definitions/symbol"},
    }
    assert output_schema["definitions"]["dispatchTables"]["properties"]["entityScript"] == {
        "type": "array",
        "minItems": 80,
        "maxItems": 80,
        "items": {"$ref": "#/definitions/symbol"},
    }
    for schema in (output_schema, fixture_schema):
        definitions = schema["definitions"]
        relation = definitions["indexedRecordsBySourcePath"]
        assert relation["additionalProperties"] is False
        assert relation["required"] == ["sourcePath", "recordIds"]
        assert relation["properties"]["recordIds"]["uniqueItems"] is True
        assert definitions["coreFacts"]["additionalProperties"] is False
        assert definitions["unlabeledData"]["additionalProperties"] is False


@pytest.mark.parametrize(
    "mutation",
    [
        "missing-source-path",
        "extra-relation-field",
        "renamed-source-path",
        "source-path-type",
        "bad-source-path",
        "record-ids-type",
        "duplicate-record-id",
        "missing-core-fact",
        "extra-core-fact",
        "missing-unlabeled-fact",
        "extra-unlabeled-fact",
    ],
)
def test_scripting_fixture_schema_rejects_recursive_shape_mutations(mutation: str) -> None:
    fixture = deepcopy(load_json(FIXTURE_PATH))
    relation = fixture["expected"]["indexedRecordsBySourcePath"][0]
    if mutation == "missing-source-path":
        del relation["sourcePath"]
    elif mutation == "extra-relation-field":
        relation["unexpected"] = True
    elif mutation == "renamed-source-path":
        relation["renamedSourcePath"] = relation.pop("sourcePath")
    elif mutation == "source-path-type":
        relation["sourcePath"] = 7
    elif mutation == "bad-source-path":
        relation["sourcePath"] = "code/common/stats/not-scripting.asm"
    elif mutation == "record-ids-type":
        relation["recordIds"] = "scripting.endcredits"
    elif mutation == "duplicate-record-id":
        relation["recordIds"] = ["scripting.endcredits", "scripting.endcredits"]
    elif mutation == "missing-core-fact":
        del fixture["expected"]["coreFacts"]["mapScript"]["commandCount"]
    elif mutation == "extra-core-fact":
        fixture["expected"]["coreFacts"]["mapScript"]["unexpected"] = True
    elif mutation == "missing-unlabeled-fact":
        del fixture["expected"]["unlabeledData"]["sizeBytes"]
    elif mutation == "extra-unlabeled-fact":
        fixture["expected"]["unlabeledData"]["unexpected"] = True
    else:
        raise AssertionError(f"unknown mutation: {mutation}")

    with pytest.raises(ValueError):
        validate_json(fixture, FIXTURE_SCHEMA, owner=f"scripting fixture {mutation}")


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize("field", ["indexedRecordIds", "indexedSourcePaths"])
def test_scripting_output_schema_rejects_duplicate_join_lists(field: str) -> None:
    output = scripting.build_scripting_inventory(UPSTREAM)
    output[field][1] = output[field][0]

    with pytest.raises(ValueError, match=field):
        validate_json(output, OUTPUT_SCHEMA, owner=f"scripting output duplicate {field}")


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("field", "value"),
    [
        ("indexedRecordIds", "scripting/not-valid"),
        ("indexedSourcePaths", "code/common/stats/not-scripting.asm"),
    ],
)
def test_scripting_output_schema_rejects_malformed_join_lists(field: str, value: str) -> None:
    output = scripting.build_scripting_inventory(UPSTREAM)
    output[field][0] = value

    with pytest.raises(ValueError, match=field):
        validate_json(output, OUTPUT_SCHEMA, owner=f"scripting output malformed {field}")


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("suffix", "mutate", "error"),
    [
        (
            "record-order",
            lambda output: output["indexedRecordIds"].reverse(),
            "indexedRecordIds relation drift",
        ),
        (
            "source-order",
            lambda output: output["indexedSourcePaths"].reverse(),
            "indexedSourcePaths relation order drift",
        ),
        (
            "relation-order",
            lambda output: output["indexedRecordsBySourcePath"].reverse(),
            "indexed relation source order drift",
        ),
        (
            "coherent-source-order",
            lambda output: (
                output["indexedRecordsBySourcePath"].reverse(),
                output["indexedSourcePaths"].reverse(),
            ),
            "indexed relation source order drift",
        ),
        (
            "duplicate-relation-source",
            lambda output: output["indexedRecordsBySourcePath"][1].update(
                sourcePath=output["indexedRecordsBySourcePath"][0]["sourcePath"]
            ),
            "indexed relation duplicate source path",
        ),
        (
            "cross-row-duplicate-record",
            lambda output: output["indexedRecordsBySourcePath"][1]["recordIds"].append(
                output["indexedRecordsBySourcePath"][0]["recordIds"][0]
            ),
            "indexed relation duplicate record ID",
        ),
        (
            "relation-record-order",
            lambda output: output["indexedRecordsBySourcePath"][17]["recordIds"].reverse(),
            "indexed relation record order drift",
        ),
        (
            "summary-record-count",
            lambda output: output["summary"].update(indexedRecordCount=125),
            "summary indexedRecordCount relation drift",
        ),
        (
            "unlabeled-file-inventory",
            lambda output: output["files"].__setitem__(
                28, deepcopy(output["files"][27])
            ),
            "source inventory path order drift",
        ),
        (
            "indexed-unlabeled-path",
            lambda output: (
                output["indexedRecordsBySourcePath"][-1].update(
                    sourcePath="code/common/scripting/text/unused_textfunctionsdata.asm"
                ),
                output["indexedSourcePaths"].__setitem__(
                    -1, "code/common/scripting/text/unused_textfunctionsdata.asm"
                ),
            ),
            "indexedSourcePaths unlabeled relation drift",
        ),
    ],
)
def test_scripting_verifier_rejects_schema_valid_join_drift_before_write(
    monkeypatch: pytest.MonkeyPatch,
    tmp_path: Path,
    suffix: str,
    mutate: Any,
    error: str,
) -> None:
    output = scripting.build_scripting_inventory(UPSTREAM)
    mutate(output)
    validate_json(output, OUTPUT_SCHEMA, owner=f"schema-valid scripting output {suffix}")
    destination = tmp_path / f"scripting-{suffix}.json"
    monkeypatch.setattr(scripting, "build_scripting_inventory", lambda _: output)

    with pytest.raises(ValueError, match=error):
        scripting.verify_scripting_inventory(UPSTREAM, output_path=destination)
    assert not destination.exists()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("suffix", "mutate", "error"),
    [
        (
            "missing-record",
            lambda fixture: fixture["expected"]["indexedRecordIds"].pop(),
            "indexedRecordIds drift",
        ),
        (
            "extra-record",
            lambda fixture: fixture["expected"]["indexedRecordIds"].append(
                "scripting.future-record"
            ),
            "indexedRecordIds drift",
        ),
        (
            "reordered-record",
            lambda fixture: fixture["expected"]["indexedRecordIds"].reverse(),
            "indexedRecordIds drift",
        ),
        (
            "wrong-record",
            lambda fixture: fixture["expected"]["indexedRecordIds"].__setitem__(
                0, "scripting.wrong-but-schema-valid"
            ),
            "indexedRecordIds drift",
        ),
        (
            "reordered-source",
            lambda fixture: fixture["expected"]["indexedSourcePaths"].reverse(),
            "indexedSourcePaths drift",
        ),
        (
            "wrong-source",
            lambda fixture: fixture["expected"]["indexedSourcePaths"].__setitem__(
                0, "code/common/scripting/text/unused_textfunctionsdata.asm"
            ),
            "indexedSourcePaths drift",
        ),
        (
            "wrong-relation-record",
            lambda fixture: fixture["expected"]["indexedRecordsBySourcePath"][0].update(
                recordIds=["scripting.wrong-but-schema-valid"]
            ),
            "indexedRecordsBySourcePath drift",
        ),
        (
            "missing-relation",
            lambda fixture: fixture["expected"]["indexedRecordsBySourcePath"].pop(),
            "indexedRecordsBySourcePath drift",
        ),
        (
            "extra-relation",
            lambda fixture: fixture["expected"]["indexedRecordsBySourcePath"].append(
                {
                    "sourcePath": "code/common/scripting/text/unused_textfunctionsdata.asm",
                    "recordIds": ["scripting.future-record"],
                }
            ),
            "indexedRecordsBySourcePath drift",
        ),
        (
            "reordered-relation",
            lambda fixture: fixture["expected"]["indexedRecordsBySourcePath"].reverse(),
            "indexedRecordsBySourcePath drift",
        ),
        (
            "wrong-relation-path",
            lambda fixture: fixture["expected"]["indexedRecordsBySourcePath"][-1].update(
                sourcePath="code/common/scripting/text/unused_textfunctionsdata.asm"
            ),
            "indexedRecordsBySourcePath drift",
        ),
        (
            "core-fact",
            lambda fixture: fixture["expected"]["coreFacts"]["mapScript"].update(
                commandCount=89
            ),
            "model drift",
        ),
        (
            "unlabeled-fact",
            lambda fixture: fixture["expected"]["unlabeledData"].update(sizeBytes=287),
            "unlabeled-data drift",
        ),
        (
            "h1-address",
            lambda fixture: fixture["function"].update(ExecuteMapScript=32769),
            "H1 address drift",
        ),
    ],
)
def test_scripting_verifier_rejects_fixture_corpus_and_h1_drift_before_write(
    monkeypatch: pytest.MonkeyPatch,
    tmp_path: Path,
    suffix: str,
    mutate: Any,
    error: str,
) -> None:
    fixture = deepcopy(load_json(FIXTURE_PATH))
    mutate(fixture)
    validate_json(fixture, FIXTURE_SCHEMA, owner=f"schema-valid scripting fixture {suffix}")
    _write_fixture(monkeypatch, tmp_path, fixture)
    destination = tmp_path / f"fixture-{suffix}.json"

    with pytest.raises(ValueError, match=error):
        scripting.verify_scripting_inventory(UPSTREAM, output_path=destination)
    assert not destination.exists()


@pytest.mark.parametrize(
    ("field", "value"),
    [("upstreamCommit", "0" * 40), ("romSha256", "0" * 64)],
)
def test_scripting_fixture_schema_preserves_legacy_provenance_constants(
    field: str, value: str
) -> None:
    fixture = deepcopy(load_json(FIXTURE_PATH))
    fixture[field] = value

    with pytest.raises(ValueError, match=field):
        validate_json(fixture, FIXTURE_SCHEMA, owner=f"scripting provenance fixture {field}")


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("field", "value", "error"),
    [
        ("upstreamCommit", "0" * 40, "fixture upstream provenance drift"),
        ("romSha256", "0" * 64, "fixture ROM provenance drift"),
    ],
)
def test_scripting_verifier_derives_pinned_provenance_when_fixture_schema_agrees(
    monkeypatch: pytest.MonkeyPatch,
    tmp_path: Path,
    field: str,
    value: str,
    error: str,
) -> None:
    fixture = deepcopy(load_json(FIXTURE_PATH))
    fixture[field] = value
    fixture_schema = deepcopy(load_json(FIXTURE_SCHEMA))
    fixture_schema["properties"][field]["const"] = value
    fixture_schema_path = _write_fixture_schema(monkeypatch, tmp_path, fixture_schema)
    validate_json(fixture, fixture_schema_path, owner=f"schema-aligned scripting {field}")
    _write_fixture(monkeypatch, tmp_path, fixture)
    destination = tmp_path / f"provenance-{field}.json"

    with pytest.raises(ValueError, match=error):
        scripting.verify_scripting_inventory(UPSTREAM, output_path=destination)
    assert not destination.exists()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("suffix", "mutate", "error"),
    [
        (
            "summary",
            lambda manifest: manifest["summary"].update(indexedRecordCount=125),
            "summary drift",
        ),
        (
            "digest",
            lambda manifest: manifest.update(outputSha256="0" * 64),
            "canonical hash drift",
        ),
    ],
)
def test_scripting_verifier_rejects_stale_manifest_before_write(
    monkeypatch: pytest.MonkeyPatch,
    tmp_path: Path,
    suffix: str,
    mutate: Any,
    error: str,
) -> None:
    manifest = deepcopy(load_json(scripting.MANIFEST))
    mutate(manifest)
    manifest_path = _write_json(tmp_path / f"manifest-{suffix}.json", manifest)
    monkeypatch.setattr(scripting, "MANIFEST", manifest_path)
    destination = tmp_path / f"manifest-{suffix}-output.json"

    with pytest.raises(ValueError, match=error):
        scripting.verify_scripting_inventory(UPSTREAM, output_path=destination)
    assert not destination.exists()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_scripting_source_membership_accepts_new_nested_record_before_fixture_check(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    index = deepcopy(load_json(repo_path("manifests/research-index.json")))
    record_id = "independent.owner.nested-scripting-record"
    index["records"].append(
        _source_record(record_id, "code/common/scripting/map/mapscriptengine_1.asm")
    )
    index_path = _write_json(tmp_path / "nested-scripting-index.json", index)
    monkeypatch.setattr(scripting, "RESEARCH_INDEX", index_path)

    output = scripting.build_scripting_inventory(UPSTREAM)
    assert record_id in output["indexedRecordIds"]
    assert output["summary"]["indexedRecordCount"] == 127
    assert output["summary"]["indexedFileCount"] == 28
    assert record_id in output["indexedRecordsBySourcePath"][17]["recordIds"]
    destination = tmp_path / "new-member-output.json"
    with pytest.raises(ValueError, match="indexedRecordIds drift"):
        scripting.verify_scripting_inventory(UPSTREAM, output_path=destination)
    assert not destination.exists()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_scripting_source_membership_excludes_outside_record(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    index = deepcopy(load_json(repo_path("manifests/research-index.json")))
    index["records"].append(
        _source_record("independent.owner.outside-scripting", "code/common/stats/flags.asm")
    )
    index_path = _write_json(tmp_path / "outside-scripting-index.json", index)
    monkeypatch.setattr(scripting, "RESEARCH_INDEX", index_path)

    # The source-root predicate is the only routing rule; this record is excluded
    # before any ID, subsystem, document, or evidence metadata could matter.
    output = scripting.build_scripting_inventory(UPSTREAM)
    assert "independent.owner.outside-scripting" not in output["indexedRecordIds"]
    assert output["summary"]["indexedRecordCount"] == 126
    destination = tmp_path / "outside-scripting-output.json"
    result = scripting.verify_scripting_inventory(UPSTREAM, output_path=destination)
    assert result["IndexedRecords"] == 126
    assert destination.is_file()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_scripting_rejects_under_root_record_missing_from_discovered_inventory(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    index = deepcopy(load_json(repo_path("manifests/research-index.json")))
    index["records"].append(
        _source_record(
            "independent.owner.missing-scripting-file",
            "code/common/scripting/map/not-discovered.asm",
        )
    )
    index_path = _write_json(tmp_path / "missing-scripting-index.json", index)
    monkeypatch.setattr(scripting, "RESEARCH_INDEX", index_path)

    with pytest.raises(ValueError, match="absent from the discovered root inventory"):
        scripting.build_scripting_inventory(UPSTREAM)


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_scripting_build_does_not_read_golden_fixture(monkeypatch: pytest.MonkeyPatch) -> None:
    monkeypatch.setattr(scripting, "FIXTURE", Path("does-not-exist.json"))
    output = scripting.build_scripting_inventory(UPSTREAM)
    assert output["summary"]["indexedRecordCount"] == 126


def test_scripting_jump_table_parser_accepts_real_shape_and_rejects_near_misses(
    tmp_path: Path,
) -> None:
    table_path = tmp_path / "table.asm"
    table_path.write_text(
        "rjt_Test:\n    dc.w csc_Real-rjt_Test\n; dc.w csc_Comment-rjt_Test\n"
        "csc_Real:\n",
        encoding="utf-8",
    )
    assert scripting._relative_jump_table(table_path, "rjt_Test") == ["csc_Real"]

    table_path.write_text("rjt_Test:\n    dc.l csc_Real-rjt_Test\n", encoding="utf-8")
    with pytest.raises(ValueError, match="unparsed scripting jump-table row"):
        scripting._relative_jump_table(table_path, "rjt_Test")
    table_path.write_text("rjt_Test:\n; dc.w csc_Comment-rjt_Test\n", encoding="utf-8")
    with pytest.raises(ValueError, match="empty scripting jump table"):
        scripting._relative_jump_table(table_path, "rjt_Test")
