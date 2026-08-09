from __future__ import annotations

import json
from copy import deepcopy
from pathlib import Path
from typing import Any

import pytest

from sf2tool.h2 import graphics
from sf2tool.h2.battle_ai import _parse_source_file
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

UPSTREAM = repo_path("local/upstream/SF2DISASM")
OUTPUT_SCHEMA = repo_path("schemas/tech-graphics-static.schema.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-tech-graphics-static-fixture.schema.json")
FIXTURE_PATH = repo_path("tests/fixtures/h2/tech-graphics-static-v1.json")


def _write_json(path: Path, value: object) -> Path:
    path.write_bytes((json.dumps(value, indent=2) + "\n").encode("utf-8"))
    return path


def _write_fixture(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path, fixture: dict[str, Any]
) -> Path:
    path = _write_json(tmp_path / "tech-graphics-fixture.json", fixture)
    monkeypatch.setattr(graphics, "FIXTURE", path)
    return path


def _write_fixture_schema(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path, schema: dict[str, Any]
) -> Path:
    path = _write_json(tmp_path / "tech-graphics-fixture.schema.json", schema)
    monkeypatch.setattr(graphics, "FIXTURE_SCHEMA", path)
    return path


def _source_record(record_id: str, source_path: str) -> dict[str, Any]:
    index = load_json(repo_path("manifests/research-index.json"))
    record = deepcopy(
        next(item for item in index["records"] if item["id"] == "tech.graphics.display")
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


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_graphics_source_path_membership_keeps_fourteen_records_over_eleven_paths() -> None:
    fixture = load_json(FIXTURE_PATH)
    output = graphics.build_graphics_inventory(UPSTREAM)
    expected = fixture["expected"]

    for field in (
        "indexedRecordIds",
        "indexedSourcePaths",
        "indexedRecordsBySourcePath",
    ):
        assert output[field] == expected[field]
    assert output["summary"]["indexedRecordCount"] == len(output["indexedRecordIds"]) == 14
    assert output["summary"]["indexedFileCount"] == len(output["indexedSourcePaths"]) == 11
    assert output["summary"]["fileCount"] == len(output["files"]) == 11
    assert output["representativeSymbols"] == expected["representativeSymbols"]
    assert output["representativeAddresses"] == fixture["function"]
    assert output["graphicsFacts"] == expected["graphicsFacts"]
    assert output["indexedRecordsBySourcePath"][0] == {
        "sourcePath": "code/common/tech/graphics/decompression.asm",
        "recordIds": [
            "tech.graphics.decompression",
            "tech.graphics.stack-decompression",
        ],
    }
    assert output["indexedRecordsBySourcePath"][1] == {
        "sourcePath": "code/common/tech/graphics/display.asm",
        "recordIds": [
            "map.camera-control.set-view-destination",
            "tech.graphics.display",
        ],
    }
    assert output["indexedRecordsBySourcePath"][6] == {
        "sourcePath": "code/common/tech/graphics/specialsprites.asm",
        "recordIds": [
            "tech.graphics.animate-special-sprite",
            "tech.graphics.special-sprites",
        ],
    }


def test_graphics_schemas_keep_exact_corpora_in_fixture_and_verifier() -> None:
    output_schema = load_json(OUTPUT_SCHEMA)
    fixture_schema = load_json(FIXTURE_SCHEMA)
    fixture_expected = fixture_schema["definitions"]["expected"]["properties"]

    for field in (
        "indexedRecordIds",
        "indexedSourcePaths",
        "indexedRecordsBySourcePath",
        "graphicsFacts",
        "representativeSymbols",
    ):
        assert "const" not in output_schema["properties"][field]
    for field in (
        "indexedRecordIds",
        "indexedSourcePaths",
        "indexedRecordsBySourcePath",
        "graphicsFacts",
        "representativeSymbols",
    ):
        assert "const" not in fixture_expected[field]
    assert output_schema["properties"]["indexedRecordIds"].get("maxItems") != 14
    assert fixture_expected["indexedRecordIds"].get("maxItems") != 14
    assert output_schema["properties"]["indexedSourcePaths"] == {
        "type": "array",
        "minItems": 11,
        "maxItems": 11,
        "uniqueItems": True,
        "items": {"$ref": "#/definitions/sourcePath"},
    }
    for schema in (output_schema, fixture_schema):
        definitions = schema["definitions"]
        relation = definitions["indexedRecordsBySourcePath"]
        assert relation["additionalProperties"] is False
        assert relation["required"] == ["sourcePath", "recordIds"]
        assert relation["properties"]["recordIds"]["uniqueItems"] is True
        assert definitions["graphicsFacts"]["additionalProperties"] is False
        assert len(definitions["graphicsFacts"]["required"]) == 8
    assert _schema_consts(output_schema) == {
        "$.properties.schemaVersion": 1,
        "$.properties.id": "sf2-tech-graphics-static-v1",
        "$.properties.scope": "code/common/tech/graphics",
        "$.definitions.summary.properties.fileCount": 11,
        "$.definitions.summary.properties.layoutIncludedFileCount": 11,
        "$.definitions.summary.properties.indexedFileCount": 11,
    }
    assert _schema_consts(fixture_schema) == {
        "$.properties.schemaVersion": 1,
        "$.properties.id": "sf2-tech-graphics-static-v1",
        "$.properties.romSha256": (
            "9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9"
        ),
        "$.properties.upstreamCommit": "c834c652b6862bc5679fd7f69a38a7093206efc6",
    }


@pytest.mark.parametrize(
    "mutation",
    [
        "missing-expected-field",
        "extra-expected-field",
        "missing-source-path",
        "renamed-source-path",
        "extra-relation-field",
        "source-path-type",
        "bad-source-path",
        "record-ids-type",
        "duplicate-same-row-record",
        "missing-relation-row",
        "extra-relation-row",
        "missing-function-address",
        "extra-function-address",
        "function-address-type",
        "missing-graphics-fact",
        "extra-graphics-fact",
        "missing-nested-graphics-fact",
        "extra-nested-graphics-fact",
        "wrong-graphics-fact-type",
    ],
)
def test_graphics_fixture_schema_rejects_recursive_shape_mutations(mutation: str) -> None:
    fixture = deepcopy(load_json(FIXTURE_PATH))
    relation = fixture["expected"]["indexedRecordsBySourcePath"][1]
    if mutation == "missing-expected-field":
        del fixture["expected"]["indexedRecordIds"]
    elif mutation == "extra-expected-field":
        fixture["expected"]["unexpected"] = True
    elif mutation == "missing-source-path":
        del relation["sourcePath"]
    elif mutation == "renamed-source-path":
        relation["renamedSourcePath"] = relation.pop("sourcePath")
    elif mutation == "extra-relation-field":
        relation["unexpected"] = True
    elif mutation == "source-path-type":
        relation["sourcePath"] = 7
    elif mutation == "bad-source-path":
        relation["sourcePath"] = "code/common/tech/sound/not-graphics.asm"
    elif mutation == "record-ids-type":
        relation["recordIds"] = "tech.graphics.display"
    elif mutation == "duplicate-same-row-record":
        relation["recordIds"] = ["tech.graphics.display", "tech.graphics.display"]
    elif mutation == "missing-relation-row":
        fixture["expected"]["indexedRecordsBySourcePath"].pop()
    elif mutation == "extra-relation-row":
        fixture["expected"]["indexedRecordsBySourcePath"].append(
            {
                "sourcePath": "code/common/tech/graphics/not-discovered.asm",
                "recordIds": ["tech.graphics.future-record"],
            }
        )
    elif mutation == "missing-function-address":
        del fixture["function"]["sub_30EE"]
    elif mutation == "extra-function-address":
        fixture["function"]["unexpectedAddress"] = 0
    elif mutation == "function-address-type":
        fixture["function"]["sub_30EE"] = "12526"
    elif mutation == "missing-graphics-fact":
        del fixture["expected"]["graphicsFacts"]["viewDestination"]
    elif mutation == "extra-graphics-fact":
        fixture["expected"]["graphicsFacts"]["unexpected"] = True
    elif mutation == "missing-nested-graphics-fact":
        del fixture["expected"]["graphicsFacts"]["viewDestination"][
            "autoscrollPreservesCurrentAxisPosition"
        ]
    elif mutation == "extra-nested-graphics-fact":
        fixture["expected"]["graphicsFacts"]["viewDestination"]["unexpected"] = True
    elif mutation == "wrong-graphics-fact-type":
        fixture["expected"]["graphicsFacts"]["viewDestination"][
            "autoscrollPreservesCurrentAxisPosition"
        ] = "true"
    else:
        raise AssertionError(f"unknown mutation: {mutation}")

    with pytest.raises(ValueError):
        validate_json(fixture, FIXTURE_SCHEMA, owner=f"tech graphics fixture {mutation}")


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    "mutation",
    [
        "missing-representative-symbols",
        "extra-representative-symbol",
        "missing-file-field",
        "extra-direct-call-field",
        "wrong-structural-indexed-file-count",
        "wrong-file-path-type",
        "wrong-graphics-fact-type",
    ],
)
def test_graphics_output_schema_rejects_recursive_shape_mutations(mutation: str) -> None:
    output = graphics.build_graphics_inventory(UPSTREAM)
    if mutation == "missing-representative-symbols":
        del output["representativeSymbols"]
    elif mutation == "extra-representative-symbol":
        output["representativeSymbols"]["unexpected.asm"] = "Unexpected"
    elif mutation == "missing-file-field":
        del output["files"][0]["sha256"]
    elif mutation == "extra-direct-call-field":
        next(row for row in output["files"] if row["directCalls"])["directCalls"][0][
            "unexpected"
        ] = True
    elif mutation == "wrong-structural-indexed-file-count":
        output["summary"]["indexedFileCount"] = 10
    elif mutation == "wrong-file-path-type":
        output["files"][0]["path"] = 7
    elif mutation == "wrong-graphics-fact-type":
        output["graphicsFacts"]["viewDestination"][
            "autoscrollPreservesCurrentAxisPosition"
        ] = "true"
    else:
        raise AssertionError(f"unknown mutation: {mutation}")

    with pytest.raises(ValueError):
        validate_json(output, OUTPUT_SCHEMA, owner=f"tech graphics output {mutation}")


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
            "missing-membership",
            lambda output: (
                output["indexedRecordIds"].remove("tech.graphics.display"),
                output["indexedRecordsBySourcePath"][1]["recordIds"].pop(),
                output["summary"].update(indexedRecordCount=13),
            ),
            "indexedRecordIds source-membership drift",
        ),
        (
            "extra-membership",
            lambda output: (
                output["indexedRecordIds"].append("tech.graphics.display-0"),
                output["indexedRecordIds"].sort(),
                output["indexedRecordsBySourcePath"][1]["recordIds"].append(
                    "tech.graphics.display-0"
                ),
                output["indexedRecordsBySourcePath"][1]["recordIds"].sort(),
                output["summary"].update(indexedRecordCount=15),
            ),
            "indexedRecordIds source-membership drift",
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
            "coherent-relation-source-order",
            lambda output: (
                output["indexedRecordsBySourcePath"].reverse(),
                output["indexedSourcePaths"].reverse(),
            ),
            "indexed relation source order drift",
        ),
        (
            "same-row-duplicate-record",
            lambda output: output["indexedRecordsBySourcePath"][1]["recordIds"].append(
                "tech.graphics.display"
            ),
            "recordIds",
        ),
        (
            "cross-row-duplicate-record",
            lambda output: output["indexedRecordsBySourcePath"][2]["recordIds"].append(
                "tech.graphics.display"
            ),
            "indexed relation duplicate record ID",
        ),
        (
            "duplicate-relation-source",
            lambda output: output["indexedRecordsBySourcePath"][1].update(
                sourcePath=output["indexedRecordsBySourcePath"][0]["sourcePath"]
            ),
            "indexed relation duplicate source path",
        ),
        (
            "relation-record-order",
            lambda output: output["indexedRecordsBySourcePath"][1]["recordIds"].reverse(),
            "indexed relation record order drift",
        ),
        (
            "summary-record-count",
            lambda output: output["summary"].update(indexedRecordCount=13),
            "summary indexedRecordCount relation drift",
        ),
        (
            "source-inventory",
            lambda output: output["files"].__setitem__(1, deepcopy(output["files"][0])),
            "source inventory duplicate path",
        ),
    ],
)
def test_graphics_verifier_rejects_schema_valid_relation_drift_before_write(
    monkeypatch: pytest.MonkeyPatch,
    tmp_path: Path,
    suffix: str,
    mutate: Any,
    error: str,
) -> None:
    output = graphics.build_graphics_inventory(UPSTREAM)
    mutate(output)
    if suffix == "same-row-duplicate-record":
        with pytest.raises(ValueError, match=error):
            validate_json(output, OUTPUT_SCHEMA, owner=f"tech graphics output {suffix}")
        return
    validate_json(output, OUTPUT_SCHEMA, owner=f"schema-valid tech graphics output {suffix}")
    destination = tmp_path / f"graphics-{suffix}.json"
    monkeypatch.setattr(graphics, "build_graphics_inventory", lambda _: output)

    with pytest.raises(ValueError, match=error):
        graphics.verify_graphics_inventory(UPSTREAM, output_path=destination)
    assert not destination.exists()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_graphics_join_invariant_rejects_same_row_duplicate_before_fixture_comparison() -> None:
    output = graphics.build_graphics_inventory(UPSTREAM)
    output["indexedRecordsBySourcePath"][1]["recordIds"].append("tech.graphics.display")
    discovered_source_paths = [row["path"] for row in output["files"]]
    expected_index_membership = graphics._index_records_for_source_root(
        set(discovered_source_paths)
    )

    with pytest.raises(ValueError, match="indexed relation duplicate record ID"):
        graphics._verify_indexed_record_join(
            output, expected_index_membership, discovered_source_paths
        )


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_graphics_verifier_rejects_coordinated_output_and_fixture_membership_drift_before_write(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    output = graphics.build_graphics_inventory(UPSTREAM)
    fixture = deepcopy(load_json(FIXTURE_PATH))
    wrong_record_id = "tech.graphics.display-0"
    record_index = output["indexedRecordIds"].index("tech.graphics.display")
    output["indexedRecordIds"][record_index] = wrong_record_id
    output["indexedRecordsBySourcePath"][1]["recordIds"][1] = wrong_record_id
    fixture["expected"]["indexedRecordIds"][record_index] = wrong_record_id
    fixture["expected"]["indexedRecordsBySourcePath"][1]["recordIds"][1] = (
        wrong_record_id
    )
    validate_json(output, OUTPUT_SCHEMA, owner="schema-valid coordinated graphics output")
    validate_json(fixture, FIXTURE_SCHEMA, owner="schema-valid coordinated graphics fixture")
    _write_fixture(monkeypatch, tmp_path, fixture)
    monkeypatch.setattr(graphics, "build_graphics_inventory", lambda _: output)
    destination = tmp_path / "coordinated-membership-drift.json"

    # RESEARCH_INDEX remains the unmodified authoritative membership source.
    with pytest.raises(ValueError, match="indexedRecordIds source-membership drift"):
        graphics.verify_graphics_inventory(UPSTREAM, output_path=destination)
    assert not destination.exists()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("suffix", "mutate", "error"),
    [
        (
            "missing-record",
            lambda fixture: fixture["expected"]["indexedRecordIds"].pop(),
            "fixture indexedRecordIds drift",
        ),
        (
            "extra-record",
            lambda fixture: fixture["expected"]["indexedRecordIds"].append(
                "tech.graphics.future-record"
            ),
            "fixture indexedRecordIds drift",
        ),
        (
            "reordered-record",
            lambda fixture: fixture["expected"]["indexedRecordIds"].reverse(),
            "fixture indexedRecordIds drift",
        ),
        (
            "wrong-record",
            lambda fixture: fixture["expected"]["indexedRecordIds"].__setitem__(
                0, "tech.graphics.wrong-but-schema-valid"
            ),
            "fixture indexedRecordIds drift",
        ),
        (
            "reordered-source",
            lambda fixture: fixture["expected"]["indexedSourcePaths"].reverse(),
            "fixture indexedSourcePaths drift",
        ),
        (
            "wrong-source",
            lambda fixture: fixture["expected"]["indexedSourcePaths"].__setitem__(
                0, "code/common/tech/graphics/notdiscovered.asm"
            ),
            "fixture indexedSourcePaths drift",
        ),
        (
            "reordered-relation",
            lambda fixture: fixture["expected"]["indexedRecordsBySourcePath"].reverse(),
            "fixture indexedRecordsBySourcePath drift",
        ),
        (
            "wrong-relation-path",
            lambda fixture: fixture["expected"]["indexedRecordsBySourcePath"][0].update(
                sourcePath="code/common/tech/graphics/notdiscovered.asm"
            ),
            "fixture indexedRecordsBySourcePath drift",
        ),
        (
            "wrong-relation-record",
            lambda fixture: fixture["expected"]["indexedRecordsBySourcePath"][1].update(
                recordIds=["tech.graphics.wrong-but-schema-valid"]
            ),
            "fixture indexedRecordsBySourcePath drift",
        ),
        (
            "graphics-fact",
            lambda fixture: fixture["expected"]["graphicsFacts"]["viewDestination"].update(
                autoscrollPreservesCurrentAxisPosition=False
            ),
            "model drift",
        ),
        (
            "h1-address",
            lambda fixture: fixture["function"].update(sub_30EE=12527),
            "H1 address drift",
        ),
        (
            "representative-symbol",
            lambda fixture: fixture["expected"]["representativeSymbols"].update(
                **{"display.asm": "SetViewDestination"}
            ),
            "representative symbol fixture drift",
        ),
    ],
)
def test_graphics_verifier_rejects_fixture_corpus_and_h1_drift_before_write(
    monkeypatch: pytest.MonkeyPatch,
    tmp_path: Path,
    suffix: str,
    mutate: Any,
    error: str,
) -> None:
    fixture = deepcopy(load_json(FIXTURE_PATH))
    mutate(fixture)
    validate_json(fixture, FIXTURE_SCHEMA, owner=f"schema-valid graphics fixture {suffix}")
    _write_fixture(monkeypatch, tmp_path, fixture)
    destination = tmp_path / f"fixture-{suffix}.json"

    with pytest.raises(ValueError, match=error):
        graphics.verify_graphics_inventory(UPSTREAM, output_path=destination)
    assert not destination.exists()


@pytest.mark.parametrize(
    ("field", "value"),
    [("upstreamCommit", "0" * 40), ("romSha256", "0" * 64)],
)
def test_graphics_fixture_schema_preserves_pinned_provenance_constants(
    field: str, value: str
) -> None:
    fixture = deepcopy(load_json(FIXTURE_PATH))
    fixture[field] = value

    with pytest.raises(ValueError, match=field):
        validate_json(fixture, FIXTURE_SCHEMA, owner=f"graphics provenance fixture {field}")


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("field", "value", "error"),
    [
        ("upstreamCommit", "0" * 40, "fixture upstream provenance drift"),
        ("romSha256", "0" * 64, "fixture ROM provenance drift"),
    ],
)
def test_graphics_verifier_derives_pinned_provenance_when_fixture_schema_agrees(
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
    validate_json(fixture, fixture_schema_path, owner=f"schema-aligned graphics {field}")
    _write_fixture(monkeypatch, tmp_path, fixture)
    destination = tmp_path / f"provenance-{field}.json"

    with pytest.raises(ValueError, match=error):
        graphics.verify_graphics_inventory(UPSTREAM, output_path=destination)
    assert not destination.exists()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("suffix", "mutate", "error"),
    [
        (
            "summary",
            lambda manifest: manifest["summary"].update(indexedRecordCount=13),
            "summary drift",
        ),
        (
            "digest",
            lambda manifest: manifest.update(outputSha256="0" * 64),
            "canonical hash drift",
        ),
    ],
)
def test_graphics_verifier_rejects_stale_manifest_before_write(
    monkeypatch: pytest.MonkeyPatch,
    tmp_path: Path,
    suffix: str,
    mutate: Any,
    error: str,
) -> None:
    manifest = deepcopy(load_json(graphics.MANIFEST))
    mutate(manifest)
    manifest_path = _write_json(tmp_path / f"manifest-{suffix}.json", manifest)
    monkeypatch.setattr(graphics, "MANIFEST", manifest_path)
    destination = tmp_path / f"manifest-{suffix}-output.json"

    with pytest.raises(ValueError, match=error):
        graphics.verify_graphics_inventory(UPSTREAM, output_path=destination)
    assert not destination.exists()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("suffix", "mutate", "error"),
    [
        (
            "summary",
            lambda output: output["summary"].update(sourceLineCount=0),
            "summary drift",
        ),
        (
            "graphics-fact",
            lambda output: output["graphicsFacts"]["viewDestination"].update(
                autoscrollPreservesCurrentAxisPosition=False
            ),
            "model drift",
        ),
        (
            "h1-address",
            lambda output: output["representativeAddresses"].update(sub_30EE=12527),
            "H1 address drift",
        ),
        (
            "representative-symbol",
            lambda output: output["representativeSymbols"].update(
                **{"display.asm": "SetViewDestination"}
            ),
            "representative symbol fixture drift",
        ),
    ],
)
def test_graphics_verifier_rejects_schema_valid_model_drift_before_write(
    monkeypatch: pytest.MonkeyPatch,
    tmp_path: Path,
    suffix: str,
    mutate: Any,
    error: str,
) -> None:
    output = graphics.build_graphics_inventory(UPSTREAM)
    mutate(output)
    validate_json(output, OUTPUT_SCHEMA, owner=f"schema-valid graphics output {suffix}")
    monkeypatch.setattr(graphics, "build_graphics_inventory", lambda _: output)
    destination = tmp_path / f"model-{suffix}.json"

    with pytest.raises(ValueError, match=error):
        graphics.verify_graphics_inventory(UPSTREAM, output_path=destination)
    assert not destination.exists()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_graphics_source_path_membership_accepts_another_owned_record_before_fixture_check(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    index = deepcopy(load_json(repo_path("manifests/research-index.json")))
    record_id = "independent.owner.nested-graphics-record"
    index["records"].append(
        _source_record(record_id, "code/common/tech/graphics/display.asm")
    )
    index_path = _write_json(tmp_path / "nested-graphics-index.json", index)
    monkeypatch.setattr(graphics, "RESEARCH_INDEX", index_path)

    output = graphics.build_graphics_inventory(UPSTREAM)
    assert record_id in output["indexedRecordIds"]
    assert output["summary"]["indexedRecordCount"] == 15
    assert record_id in output["indexedRecordsBySourcePath"][1]["recordIds"]
    destination = tmp_path / "new-member-output.json"
    with pytest.raises(ValueError, match="fixture indexedRecordIds drift"):
        graphics.verify_graphics_inventory(UPSTREAM, output_path=destination)
    assert not destination.exists()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_graphics_source_membership_ignores_metadata_but_excludes_outside_root(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    index = deepcopy(load_json(repo_path("manifests/research-index.json")))
    metadata_neutral_id = "independent.owner.metadata-neutral-graphics-record"
    metadata_neutral = _source_record(
        metadata_neutral_id, "code/common/tech/graphics/display.asm"
    )
    metadata_neutral["subsystem"] = "unrelated.subsystem"
    metadata_neutral["status"] = "inferred"
    metadata_neutral["documents"] = ["docs/research/unrelated.md"]
    metadata_neutral["evidence"] = []
    index["records"].append(metadata_neutral)
    index["records"].append(
        _source_record(
            "independent.owner.outside-tech-graphics", "code/common/tech/sound/music.asm"
        )
    )
    index_path = _write_json(tmp_path / "metadata-graphics-index.json", index)
    monkeypatch.setattr(graphics, "RESEARCH_INDEX", index_path)

    output = graphics.build_graphics_inventory(UPSTREAM)
    assert metadata_neutral_id in output["indexedRecordIds"]
    assert "independent.owner.outside-tech-graphics" not in output["indexedRecordIds"]
    assert output["summary"]["indexedRecordCount"] == 15


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_graphics_rejects_under_root_record_missing_from_discovered_inventory(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    index = deepcopy(load_json(repo_path("manifests/research-index.json")))
    index["records"].append(
        _source_record(
            "independent.owner.missing-tech-graphics-file",
            "code/common/tech/graphics/not-discovered.asm",
        )
    )
    index_path = _write_json(tmp_path / "missing-graphics-index.json", index)
    monkeypatch.setattr(graphics, "RESEARCH_INDEX", index_path)

    with pytest.raises(ValueError, match="absent from the discovered root inventory"):
        graphics.build_graphics_inventory(UPSTREAM)


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_graphics_build_does_not_read_golden_fixture(monkeypatch: pytest.MonkeyPatch) -> None:
    monkeypatch.setattr(graphics, "FIXTURE", Path("does-not-exist.json"))
    output = graphics.build_graphics_inventory(UPSTREAM)
    assert output["summary"]["indexedRecordCount"] == 14
    assert output["indexedRecordsBySourcePath"][1]["recordIds"][0] == (
        "map.camera-control.set-view-destination"
    )


def test_graphics_shared_source_parser_excludes_comments_and_near_misses(tmp_path: Path) -> None:
    source_path = tmp_path / "graphics.asm"
    source_path.write_text(
        "CommentOnly: ; jsr.w CommentTarget\n"
        "; bsr.w AnotherCommentTarget\n"
        "    jsr.w DirectTarget ; bsr.w CommentOnly\n"
        "    bsr.w (ParenthesizedTarget)\n"
        "    bsr.b ByteTarget\n"
        "    jsr.l LongTarget\n"
        "    jsr.w (a0)\n"
        "    jsrish NearMiss\n",
        encoding="utf-8",
    )

    parsed = _parse_source_file(source_path, "code/common/tech/graphics/graphics.asm")
    assert parsed["globalLabels"] == ["CommentOnly"]
    assert parsed["directCalls"] == [
        {"target": "ByteTarget", "siteCount": 1},
        {"target": "DirectTarget", "siteCount": 1},
        {"target": "LongTarget", "siteCount": 1},
        {"target": "ParenthesizedTarget", "siteCount": 1},
    ]
    assert parsed["indirectCallSiteCount"] == 1


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_graphics_fact_guard_rejects_a_source_control_flow_near_miss(tmp_path: Path) -> None:
    disasm = tmp_path / "disasm"
    source_root = disasm / graphics.SOURCE_ROOT
    source_root.mkdir(parents=True)
    upstream_root = UPSTREAM / "disasm" / graphics.SOURCE_ROOT
    for source_path in upstream_root.glob("*.asm"):
        (source_root / source_path.name).write_bytes(source_path.read_bytes())
    display = source_root / "display.asm"
    display.write_text(
        display.read_text(encoding="utf-8").replace(
            "mulu.w  ((MAP_AREA_LAYER1_PARALLAX_X-$1000000)).w,d0",
            "mulu.w  ((MAP_AREA_LAYER1_PARALLAX_X-$1000000)).w,d1",
            1,
        ),
        encoding="utf-8",
    )

    with pytest.raises(ValueError, match="instruction sequence drift"):
        graphics._graphics_facts(disasm)
