from __future__ import annotations

import json
from copy import deepcopy
from pathlib import Path
from typing import Any

import pytest

from sf2tool.h2 import interfaces
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

UPSTREAM = repo_path("local/upstream/SF2DISASM")
OUTPUT_SCHEMA = repo_path("schemas/tech-interfaces-static.schema.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-tech-interfaces-static-fixture.schema.json")
FIXTURE_PATH = repo_path("tests/fixtures/h2/tech-interfaces-static-v1.json")


def _write_json(path: Path, value: object) -> Path:
    path.write_bytes((json.dumps(value, indent=2) + "\n").encode("utf-8"))
    return path


def _write_fixture(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path, fixture: dict[str, Any]
) -> Path:
    path = _write_json(tmp_path / "tech-interfaces-fixture.json", fixture)
    monkeypatch.setattr(interfaces, "FIXTURE", path)
    return path


def _write_fixture_schema(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path, schema: dict[str, Any]
) -> Path:
    path = _write_json(tmp_path / "tech-interfaces-fixture.schema.json", schema)
    monkeypatch.setattr(interfaces, "FIXTURE_SCHEMA", path)
    return path


def _source_record(record_id: str, source_path: str) -> dict[str, Any]:
    index = load_json(repo_path("manifests/research-index.json"))
    record = deepcopy(
        next(item for item in index["records"] if item["id"] == "tech.interfaces.jump-s02")
    )
    record["id"] = record_id
    record["sourcePath"] = source_path
    return record


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_interface_source_root_join_keeps_26_records_over_25_paths() -> None:
    fixture = load_json(FIXTURE_PATH)
    output = interfaces.build_interface_inventory(UPSTREAM)

    assert output["indexedRecordIds"] == fixture["expected"]["indexedRecordIds"]
    assert output["indexedSourcePaths"] == fixture["expected"]["indexedSourcePaths"]
    assert output["indexedRecordsBySourcePath"] == fixture["expected"][
        "indexedRecordsBySourcePath"
    ]
    assert output["summary"]["indexedRecordCount"] == len(output["indexedRecordIds"]) == 26
    assert output["summary"]["indexedFileCount"] == len(output["indexedSourcePaths"]) == 25
    assert output["summary"]["jumpInterfaceFileCount"] == 10
    assert output["summary"]["pointerFileCount"] == 15
    assert output["interfaceFacts"]["jumpStubCount"] == 331
    assert output["interfaceFacts"]["pointerEntryCount"] == 60
    assert output["indexedRecordsBySourcePath"][9] == {
        "sourcePath": "code/common/tech/jumpinterfaces/s13_jumpinterface.asm",
        "recordIds": [
            "tech.interfaces.jump-s13",
            "tech.services.thinking-rng-alias",
        ],
    }


def test_interface_schemas_keep_26_record_corpus_in_fixture_and_verifier() -> None:
    output_schema = load_json(OUTPUT_SCHEMA)
    fixture_schema = load_json(FIXTURE_SCHEMA)

    for field in (
        "indexedRecordIds",
        "indexedSourcePaths",
        "indexedRecordsBySourcePath",
        "interfaceFacts",
    ):
        assert "const" not in output_schema["properties"][field]
        assert "const" not in fixture_schema["properties"]["expected"]["properties"][field]
    assert output_schema["properties"]["indexedRecordIds"].get("maxItems") != 26
    assert fixture_schema["properties"]["expected"]["properties"][
        "indexedRecordIds"
    ].get("maxItems") != 26
    assert output_schema["properties"]["indexedSourcePaths"] == {
        "type": "array",
        "minItems": 25,
        "maxItems": 25,
        "uniqueItems": True,
        "items": {"$ref": "#/definitions/sourcePath"},
    }
    assert output_schema["definitions"]["summary"]["properties"][
        "indexedFileCount"
    ] == {"const": 25}
    assert fixture_schema["properties"]["romSha256"] == {
        "const": "9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9"
    }
    assert fixture_schema["properties"]["upstreamCommit"] == {
        "const": "c834c652b6862bc5679fd7f69a38a7093206efc6"
    }
    assert output_schema["properties"]["schemaVersion"] == {"const": 1}
    assert output_schema["properties"]["id"] == {
        "const": "sf2-tech-interfaces-static-v1"
    }
    assert output_schema["properties"]["scopes"]["const"] == [
        "code/common/tech/jumpinterfaces",
        "code/common/tech/pointers",
    ]
    for schema in (output_schema, fixture_schema):
        definitions = schema["definitions"]
        relation = definitions["indexedRecordsBySourcePath"]
        assert relation["additionalProperties"] is False
        assert relation["required"] == ["sourcePath", "recordIds"]
        assert relation["properties"]["recordIds"]["uniqueItems"] is True
        assert definitions["interfaceFacts"]["additionalProperties"] is False


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
        "missing-interface-fact",
        "extra-interface-fact",
    ],
)
def test_interface_schemas_reject_recursive_join_and_fact_mutations(mutation: str) -> None:
    fixture = deepcopy(load_json(FIXTURE_PATH))
    relation = fixture["expected"]["indexedRecordsBySourcePath"][0]
    facts = fixture["expected"]["interfaceFacts"]
    if mutation == "missing-source-path":
        del relation["sourcePath"]
    elif mutation == "extra-relation-field":
        relation["unexpected"] = True
    elif mutation == "renamed-source-path":
        relation["renamedSourcePath"] = relation.pop("sourcePath")
    elif mutation == "source-path-type":
        relation["sourcePath"] = 7
    elif mutation == "bad-source-path":
        relation["sourcePath"] = "code/common/tech/services/not-an-interface.asm"
    elif mutation == "record-ids-type":
        relation["recordIds"] = "tech.interfaces.jump-s02"
    elif mutation == "duplicate-record-id":
        relation["recordIds"] = ["tech.interfaces.jump-s02", "tech.interfaces.jump-s02"]
    elif mutation == "missing-interface-fact":
        del facts["jumpStubCount"]
    elif mutation == "extra-interface-fact":
        facts["unexpected"] = True
    else:
        raise AssertionError(f"unknown mutation: {mutation}")

    with pytest.raises(ValueError):
        validate_json(fixture, FIXTURE_SCHEMA, owner=f"interface fixture {mutation}")


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize("field", ["indexedRecordIds", "indexedSourcePaths"])
def test_interface_output_schema_rejects_duplicate_join_lists(field: str) -> None:
    output = interfaces.build_interface_inventory(UPSTREAM)
    output[field][1] = output[field][0]

    with pytest.raises(ValueError, match=field):
        validate_json(output, OUTPUT_SCHEMA, owner=f"interface output duplicate {field}")


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("field", "value"),
    [
        ("indexedRecordIds", "tech.interfaces/not-valid"),
        ("indexedSourcePaths", "code/common/tech/services/not-valid.asm"),
    ],
)
def test_interface_output_schema_rejects_malformed_join_lists(field: str, value: str) -> None:
    output = interfaces.build_interface_inventory(UPSTREAM)
    output[field][0] = value

    with pytest.raises(ValueError, match=field):
        validate_json(output, OUTPUT_SCHEMA, owner=f"interface output malformed {field}")


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
            "indexedSourcePaths relation order drift",
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
            lambda output: output["indexedRecordsBySourcePath"][9]["recordIds"].reverse(),
            "indexed relation record order drift",
        ),
        (
            "summary-record-count",
            lambda output: output["summary"].update(indexedRecordCount=25),
            "summary indexedRecordCount relation drift",
        ),
    ],
)
def test_interface_verifier_rejects_schema_valid_join_drift_before_write(
    monkeypatch: pytest.MonkeyPatch,
    tmp_path: Path,
    suffix: str,
    mutate: Any,
    error: str,
) -> None:
    output = interfaces.build_interface_inventory(UPSTREAM)
    mutate(output)
    validate_json(output, OUTPUT_SCHEMA, owner=f"schema-valid interface output {suffix}")
    destination = tmp_path / f"interface-{suffix}.json"
    monkeypatch.setattr(interfaces, "build_interface_inventory", lambda _: output)

    with pytest.raises(ValueError, match=error):
        interfaces.verify_interface_inventory(UPSTREAM, output_path=destination)
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
                "tech.interfaces.future-record"
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
                0, "tech.interfaces.wrong-but-schema-valid"
            ),
            "indexedRecordIds drift",
        ),
        (
            "reordered-source",
            lambda fixture: fixture["expected"]["indexedSourcePaths"].reverse(),
            "indexedSourcePaths drift",
        ),
        (
            "wrong-relation-record",
            lambda fixture: fixture["expected"]["indexedRecordsBySourcePath"][0].update(
                recordIds=["tech.interfaces.wrong-but-schema-valid"]
            ),
            "indexedRecordsBySourcePath drift",
        ),
        (
            "interface-fact",
            lambda fixture: fixture["expected"]["interfaceFacts"].update(jumpStubCount=330),
            "interfaceFacts drift",
        ),
        (
            "h1-address",
            lambda fixture: fixture["function"].update(j_GetCombatantName=32769),
            "H1 address drift",
        ),
    ],
)
def test_interface_verifier_rejects_fixture_corpus_and_h1_drift_before_write(
    monkeypatch: pytest.MonkeyPatch,
    tmp_path: Path,
    suffix: str,
    mutate: Any,
    error: str,
) -> None:
    fixture = deepcopy(load_json(FIXTURE_PATH))
    mutate(fixture)
    validate_json(fixture, FIXTURE_SCHEMA, owner=f"schema-valid interface fixture {suffix}")
    _write_fixture(monkeypatch, tmp_path, fixture)
    destination = tmp_path / f"fixture-{suffix}.json"

    with pytest.raises(ValueError, match=error):
        interfaces.verify_interface_inventory(UPSTREAM, output_path=destination)
    assert not destination.exists()


@pytest.mark.parametrize(
    ("field", "value"),
    [
        ("upstreamCommit", "0" * 40),
        ("romSha256", "0" * 64),
    ],
)
def test_interface_fixture_schema_preserves_legacy_provenance_constants(
    field: str, value: str
) -> None:
    fixture = deepcopy(load_json(FIXTURE_PATH))
    fixture[field] = value

    with pytest.raises(ValueError, match=field):
        validate_json(fixture, FIXTURE_SCHEMA, owner=f"legacy provenance fixture {field}")


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("field", "value", "error"),
    [
        ("upstreamCommit", "0" * 40, "fixture upstream provenance drift"),
        ("romSha256", "0" * 64, "fixture ROM provenance drift"),
    ],
)
def test_interface_verifier_derives_pinned_provenance_when_fixture_schema_agrees(
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
    validate_json(fixture, fixture_schema_path, owner=f"schema-aligned interface {field}")
    _write_fixture(monkeypatch, tmp_path, fixture)
    destination = tmp_path / f"provenance-{field}.json"

    with pytest.raises(ValueError, match=error):
        interfaces.verify_interface_inventory(UPSTREAM, output_path=destination)
    assert not destination.exists()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_interface_output_schema_preserves_25_source_path_cardinality() -> None:
    output = interfaces.build_interface_inventory(UPSTREAM)
    output["indexedSourcePaths"].pop()
    with pytest.raises(ValueError, match="indexedSourcePaths"):
        validate_json(output, OUTPUT_SCHEMA, owner="interface source-path cardinality")

    output = interfaces.build_interface_inventory(UPSTREAM)
    output["summary"]["indexedFileCount"] = 24
    with pytest.raises(ValueError, match="indexedFileCount"):
        validate_json(output, OUTPUT_SCHEMA, owner="interface source-count cardinality")


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("suffix", "mutate", "error"),
    [
        (
            "summary",
            lambda manifest: manifest["summary"].update(indexedRecordCount=25),
            "summary drift",
        ),
        (
            "digest",
            lambda manifest: manifest.update(outputSha256="0" * 64),
            "canonical hash drift",
        ),
    ],
)
def test_interface_verifier_rejects_stale_manifest_before_write(
    monkeypatch: pytest.MonkeyPatch,
    tmp_path: Path,
    suffix: str,
    mutate: Any,
    error: str,
) -> None:
    manifest = deepcopy(load_json(interfaces.MANIFEST))
    mutate(manifest)
    manifest_path = _write_json(tmp_path / f"manifest-{suffix}.json", manifest)
    monkeypatch.setattr(interfaces, "MANIFEST", manifest_path)
    destination = tmp_path / f"manifest-{suffix}-output.json"

    with pytest.raises(ValueError, match=error):
        interfaces.verify_interface_inventory(UPSTREAM, output_path=destination)
    assert not destination.exists()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("root_source", "record_id"),
    [
        ("code/common/tech/jumpinterfaces/s02_jumpinterface.asm", "tech.services.future-jump"),
        ("code/common/tech/pointers/s02_pointers.asm", "tech.services.future-pointer"),
    ],
)
def test_interface_root_membership_accepts_new_records_under_either_root_before_fixture_check(
    monkeypatch: pytest.MonkeyPatch,
    tmp_path: Path,
    root_source: str,
    record_id: str,
) -> None:
    index = deepcopy(load_json(repo_path("manifests/research-index.json")))
    index["records"].append(_source_record(record_id, root_source))
    index_path = _write_json(tmp_path / f"index-{record_id}.json", index)
    monkeypatch.setattr(interfaces, "RESEARCH_INDEX", index_path)

    output = interfaces.build_interface_inventory(UPSTREAM)
    assert record_id in output["indexedRecordIds"]
    assert output["summary"]["indexedRecordCount"] == 27
    assert output["summary"]["indexedFileCount"] == 25
    destination = tmp_path / f"new-member-{record_id}.json"
    with pytest.raises(ValueError, match="indexedRecordIds drift"):
        interfaces.verify_interface_inventory(UPSTREAM, output_path=destination)
    assert not destination.exists()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_interface_root_membership_excludes_outside_records(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    index = deepcopy(load_json(repo_path("manifests/research-index.json")))
    index["records"].append(
        _source_record("tech.services.outside-interface-root", "code/common/tech/services/rng.asm")
    )
    index_path = _write_json(tmp_path / "outside-root-index.json", index)
    monkeypatch.setattr(interfaces, "RESEARCH_INDEX", index_path)

    output = interfaces.build_interface_inventory(UPSTREAM)
    assert "tech.services.outside-interface-root" not in output["indexedRecordIds"]
    assert output["summary"]["indexedRecordCount"] == 26
    destination = tmp_path / "outside-root-output.json"
    result = interfaces.verify_interface_inventory(UPSTREAM, output_path=destination)
    assert result["IndexedRecords"] == 26
    assert destination.is_file()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_interface_build_does_not_read_golden_fixture(monkeypatch: pytest.MonkeyPatch) -> None:
    monkeypatch.setattr(interfaces, "FIXTURE", Path("does-not-exist.json"))
    output = interfaces.build_interface_inventory(UPSTREAM)
    assert output["summary"]["indexedRecordCount"] == 26


def test_interface_parsers_accept_real_shapes_and_reject_near_misses(tmp_path: Path) -> None:
    jump_path = tmp_path / "jump.asm"
    jump_path.write_text(
        "j_Real:\n    jmp Target(pc)\n; j_Comment:\n; jmp Wrong(pc)\n",
        encoding="utf-8",
    )
    pointer_path = tmp_path / "pointers.asm"
    pointer_path.write_text(
        "p_Real:\n    dc.l Target\np_Inline: dc.l Other\n; p_Comment: dc.l Wrong\n",
        encoding="utf-8",
    )
    assert interfaces._jump_targets([jump_path]) == {"j_Real": "Target"}
    assert interfaces._pointer_targets([pointer_path]) == {
        "p_Inline": "Other",
        "p_Real": "Target",
    }

    jump_path.write_text("j_Real:\n    jsr Target(pc)\n", encoding="utf-8")
    with pytest.raises(ValueError, match="jump-interface non-stub shape drift"):
        interfaces._jump_targets([jump_path])
    pointer_path.write_text("p_Real:\n    dc.w Target\n", encoding="utf-8")
    with pytest.raises(ValueError, match="pointer-table entry shape drift"):
        interfaces._pointer_targets([pointer_path])
