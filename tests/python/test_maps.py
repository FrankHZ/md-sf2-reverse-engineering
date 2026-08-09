from __future__ import annotations

import json
from copy import deepcopy
from pathlib import Path
from typing import Any

import pytest

from sf2tool.h2 import maps
from sf2tool.h2.battle_ai import _parse_source_file
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

UPSTREAM = repo_path("local/upstream/SF2DISASM")
OUTPUT_SCHEMA = repo_path("schemas/common-maps-static.schema.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-common-maps-static-fixture.schema.json")
FIXTURE_PATH = repo_path("tests/fixtures/h2/common-maps-static-v1.json")


def _write_json(path: Path, value: object) -> Path:
    path.write_bytes((json.dumps(value, indent=2) + "\n").encode("utf-8"))
    return path


def _write_fixture(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path, fixture: dict[str, Any]
) -> Path:
    path = _write_json(tmp_path / "common-maps-fixture.json", fixture)
    monkeypatch.setattr(maps, "FIXTURE", path)
    return path


def _write_fixture_schema(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path, schema: dict[str, Any]
) -> Path:
    path = _write_json(tmp_path / "common-maps-fixture.schema.json", schema)
    monkeypatch.setattr(maps, "FIXTURE_SCHEMA", path)
    return path


def _source_record(record_id: str, source_path: str) -> dict[str, Any]:
    index = load_json(repo_path("manifests/research-index.json"))
    record = deepcopy(next(item for item in index["records"] if item["id"] == "maps.camera"))
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
def test_maps_source_path_membership_keeps_eight_records_over_seven_paths() -> None:
    fixture = load_json(FIXTURE_PATH)
    output = maps.build_map_inventory(UPSTREAM)
    expected = fixture["expected"]

    for field in (
        "indexedRecordIds",
        "indexedSourcePaths",
        "indexedRecordsBySourcePath",
    ):
        assert output[field] == expected[field]
    assert output["summary"]["indexedRecordCount"] == len(output["indexedRecordIds"]) == 8
    assert output["summary"]["indexedFileCount"] == len(output["indexedSourcePaths"]) == 7
    assert output["summary"]["fileCount"] == len(output["files"]) == 7
    assert output["function"] == fixture["function"]
    assert output["mapFacts"] == expected["mapFacts"]
    assert output["indexedRecordsBySourcePath"][1] == {
        "sourcePath": "code/common/maps/camerafunctions.asm",
        "recordIds": [
            "map.camera-control.wait-for-view-scroll-end",
            "maps.camera",
        ],
    }


def test_maps_schemas_keep_exact_corpus_values_in_fixture_and_verifier() -> None:
    output_schema = load_json(OUTPUT_SCHEMA)
    fixture_schema = load_json(FIXTURE_SCHEMA)
    fixture_expected = fixture_schema["definitions"]["expected"]["properties"]

    for field in (
        "indexedRecordIds",
        "indexedSourcePaths",
        "indexedRecordsBySourcePath",
        "mapFacts",
        "function",
    ):
        assert "const" not in output_schema["properties"][field]
    for field in (
        "indexedRecordIds",
        "indexedSourcePaths",
        "indexedRecordsBySourcePath",
        "mapFacts",
    ):
        assert "const" not in fixture_expected[field]
    assert output_schema["properties"]["indexedRecordIds"].get("maxItems") != 8
    assert fixture_expected["indexedRecordIds"].get("maxItems") != 8
    assert output_schema["properties"]["indexedSourcePaths"] == {
        "type": "array",
        "minItems": 7,
        "maxItems": 7,
        "uniqueItems": True,
        "items": {"$ref": "#/definitions/sourcePath"},
    }
    for schema in (output_schema, fixture_schema):
        definitions = schema["definitions"]
        relation = definitions["indexedRecordsBySourcePath"]
        assert relation["additionalProperties"] is False
        assert relation["required"] == ["sourcePath", "recordIds"]
        assert relation["properties"]["recordIds"]["uniqueItems"] is True
        assert definitions["functionAddresses"]["additionalProperties"] is False
        assert len(definitions["functionAddresses"]["required"]) == 7
        assert definitions["mapFacts"]["additionalProperties"] is False
        assert len(definitions["mapFacts"]["required"]) == 6
    assert _schema_consts(output_schema) == {
        "$.properties.schemaVersion": 1,
        "$.properties.id": "sf2-common-maps-static-v1",
        "$.properties.scope": "code/common/maps",
        "$.definitions.summary.properties.fileCount": 7,
        "$.definitions.summary.properties.indexedFileCount": 7,
    }
    assert _schema_consts(fixture_schema) == {
        "$.properties.schemaVersion": 1,
        "$.properties.id": "sf2-common-maps-static-v1",
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
        "missing-map-fact",
        "extra-map-fact",
        "missing-nested-map-fact",
        "extra-nested-map-fact",
        "wrong-map-fact-type",
    ],
)
def test_maps_fixture_schema_rejects_recursive_shape_mutations(mutation: str) -> None:
    fixture = deepcopy(load_json(FIXTURE_PATH))
    relation = fixture["expected"]["indexedRecordsBySourcePath"][0]
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
        relation["sourcePath"] = "code/common/stats/not-common-maps.asm"
    elif mutation == "record-ids-type":
        relation["recordIds"] = "maps.animations"
    elif mutation == "duplicate-same-row-record":
        relation["recordIds"] = ["maps.animations", "maps.animations"]
    elif mutation == "missing-relation-row":
        fixture["expected"]["indexedRecordsBySourcePath"].pop()
    elif mutation == "extra-relation-row":
        fixture["expected"]["indexedRecordsBySourcePath"].append(
            {
                "sourcePath": "code/common/maps/not-discovered.asm",
                "recordIds": ["maps.future-record"],
            }
        )
    elif mutation == "missing-function-address":
        del fixture["function"]["cameraAddress"]
    elif mutation == "extra-function-address":
        fixture["function"]["unexpectedAddress"] = 0
    elif mutation == "function-address-type":
        fixture["function"]["cameraAddress"] = "17858"
    elif mutation == "missing-map-fact":
        del fixture["expected"]["mapFacts"]["vint"]
    elif mutation == "extra-map-fact":
        fixture["expected"]["mapFacts"]["unexpected"] = True
    elif mutation == "missing-nested-map-fact":
        del fixture["expected"]["mapFacts"]["vint"]["planeAToggleBit"]
    elif mutation == "extra-nested-map-fact":
        fixture["expected"]["mapFacts"]["vint"]["unexpected"] = True
    elif mutation == "wrong-map-fact-type":
        fixture["expected"]["mapFacts"]["vint"]["planeAToggleBit"] = "zero"
    else:
        raise AssertionError(f"unknown mutation: {mutation}")

    with pytest.raises(ValueError):
        validate_json(fixture, FIXTURE_SCHEMA, owner=f"common-maps fixture {mutation}")


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    "mutation",
    [
        "missing-function",
        "extra-function-field",
        "missing-file-field",
        "extra-direct-call-field",
        "wrong-file-path-type",
        "wrong-map-fact-type",
    ],
)
def test_maps_output_schema_rejects_recursive_shape_mutations(mutation: str) -> None:
    output = maps.build_map_inventory(UPSTREAM)
    if mutation == "missing-function":
        del output["function"]
    elif mutation == "extra-function-field":
        output["function"]["unexpectedAddress"] = 0
    elif mutation == "missing-file-field":
        del output["files"][0]["sha256"]
    elif mutation == "extra-direct-call-field":
        output["files"][0]["directCalls"][0]["unexpected"] = True
    elif mutation == "wrong-file-path-type":
        output["files"][0]["path"] = 7
    elif mutation == "wrong-map-fact-type":
        output["mapFacts"]["vint"]["planeBToggleBit"] = "one"
    else:
        raise AssertionError(f"unknown mutation: {mutation}")

    with pytest.raises(ValueError):
        validate_json(output, OUTPUT_SCHEMA, owner=f"common-maps output {mutation}")


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
                "maps.camera"
            ),
            "recordIds",
        ),
        (
            "cross-row-duplicate-record",
            lambda output: output["indexedRecordsBySourcePath"][2]["recordIds"].append(
                "maps.camera"
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
            lambda output: output["summary"].update(indexedRecordCount=7),
            "summary indexedRecordCount relation drift",
        ),
        (
            "source-inventory",
            lambda output: output["files"].__setitem__(1, deepcopy(output["files"][0])),
            "source inventory duplicate path",
        ),
    ],
)
def test_maps_verifier_rejects_schema_valid_relation_drift_before_write(
    monkeypatch: pytest.MonkeyPatch,
    tmp_path: Path,
    suffix: str,
    mutate: Any,
    error: str,
) -> None:
    output = maps.build_map_inventory(UPSTREAM)
    mutate(output)
    if suffix == "same-row-duplicate-record":
        with pytest.raises(ValueError, match=error):
            validate_json(output, OUTPUT_SCHEMA, owner=f"common-maps output {suffix}")
        return
    validate_json(output, OUTPUT_SCHEMA, owner=f"schema-valid common-maps output {suffix}")
    destination = tmp_path / f"maps-{suffix}.json"
    monkeypatch.setattr(maps, "build_map_inventory", lambda _: output)

    with pytest.raises(ValueError, match=error):
        maps.verify_map_inventory(UPSTREAM, output_path=destination)
    assert not destination.exists()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_maps_join_invariant_rejects_same_row_duplicate_before_fixture_comparison() -> None:
    output = maps.build_map_inventory(UPSTREAM)
    output["indexedRecordsBySourcePath"][1]["recordIds"].append("maps.camera")
    discovered_source_paths = [row["path"] for row in output["files"]]
    expected_index_membership = maps._index_records_for_source_root(
        set(discovered_source_paths)
    )

    with pytest.raises(ValueError, match="indexed relation duplicate record ID"):
        maps._verify_indexed_record_join(
            output, expected_index_membership, discovered_source_paths
        )


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_maps_verifier_rejects_coordinated_output_and_fixture_membership_drift_before_write(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    output = maps.build_map_inventory(UPSTREAM)
    fixture = deepcopy(load_json(FIXTURE_PATH))
    wrong_record_id = "maps.cameraa"
    record_index = output["indexedRecordIds"].index("maps.camera")
    output["indexedRecordIds"][record_index] = wrong_record_id
    output["indexedRecordsBySourcePath"][1]["recordIds"][1] = wrong_record_id
    fixture["expected"]["indexedRecordIds"][record_index] = wrong_record_id
    fixture["expected"]["indexedRecordsBySourcePath"][1]["recordIds"][1] = (
        wrong_record_id
    )
    validate_json(output, OUTPUT_SCHEMA, owner="schema-valid coordinated common-maps output")
    validate_json(fixture, FIXTURE_SCHEMA, owner="schema-valid coordinated common-maps fixture")
    _write_fixture(monkeypatch, tmp_path, fixture)
    monkeypatch.setattr(maps, "build_map_inventory", lambda _: output)
    destination = tmp_path / "coordinated-membership-drift.json"

    # RESEARCH_INDEX remains the unmodified authoritative membership source.
    with pytest.raises(ValueError, match="indexedRecordIds source-membership drift"):
        maps.verify_map_inventory(UPSTREAM, output_path=destination)
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
                "maps.future-record"
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
                0, "maps.wrong-but-schema-valid"
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
                0, "code/common/maps/notdiscovered.asm"
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
                sourcePath="code/common/maps/notdiscovered.asm"
            ),
            "fixture indexedRecordsBySourcePath drift",
        ),
        (
            "wrong-relation-record",
            lambda fixture: fixture["expected"]["indexedRecordsBySourcePath"][1].update(
                recordIds=["maps.wrong-but-schema-valid"]
            ),
            "fixture indexedRecordsBySourcePath drift",
        ),
        (
            "map-fact",
            lambda fixture: fixture["expected"]["mapFacts"]["vint"].update(
                planeAToggleBit=9
            ),
            "model drift",
        ),
        (
            "h1-address",
            lambda fixture: fixture["function"].update(cameraAddress=17859),
            "H1 address drift",
        ),
        (
            "representative-symbol",
            lambda fixture: fixture["expected"]["representativeSymbols"].update(
                **{"camerafunctions.asm": "WaitForViewScrollEnd"}
            ),
            "representative symbol fixture drift",
        ),
    ],
)
def test_maps_verifier_rejects_fixture_corpus_and_h1_drift_before_write(
    monkeypatch: pytest.MonkeyPatch,
    tmp_path: Path,
    suffix: str,
    mutate: Any,
    error: str,
) -> None:
    fixture = deepcopy(load_json(FIXTURE_PATH))
    mutate(fixture)
    validate_json(fixture, FIXTURE_SCHEMA, owner=f"schema-valid common-maps fixture {suffix}")
    _write_fixture(monkeypatch, tmp_path, fixture)
    destination = tmp_path / f"fixture-{suffix}.json"

    with pytest.raises(ValueError, match=error):
        maps.verify_map_inventory(UPSTREAM, output_path=destination)
    assert not destination.exists()


@pytest.mark.parametrize(
    ("field", "value"),
    [("upstreamCommit", "0" * 40), ("romSha256", "0" * 64)],
)
def test_maps_fixture_schema_preserves_pinned_provenance_constants(
    field: str, value: str
) -> None:
    fixture = deepcopy(load_json(FIXTURE_PATH))
    fixture[field] = value

    with pytest.raises(ValueError, match=field):
        validate_json(fixture, FIXTURE_SCHEMA, owner=f"common-maps provenance fixture {field}")


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("field", "value", "error"),
    [
        ("upstreamCommit", "0" * 40, "fixture upstream provenance drift"),
        ("romSha256", "0" * 64, "fixture ROM provenance drift"),
    ],
)
def test_maps_verifier_derives_pinned_provenance_when_fixture_schema_agrees(
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
    validate_json(fixture, fixture_schema_path, owner=f"schema-aligned common-maps {field}")
    _write_fixture(monkeypatch, tmp_path, fixture)
    destination = tmp_path / f"provenance-{field}.json"

    with pytest.raises(ValueError, match=error):
        maps.verify_map_inventory(UPSTREAM, output_path=destination)
    assert not destination.exists()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("suffix", "mutate", "error"),
    [
        (
            "summary",
            lambda manifest: manifest["summary"].update(indexedRecordCount=7),
            "summary drift",
        ),
        (
            "digest",
            lambda manifest: manifest.update(outputSha256="0" * 64),
            "canonical hash drift",
        ),
    ],
)
def test_maps_verifier_rejects_stale_manifest_before_write(
    monkeypatch: pytest.MonkeyPatch,
    tmp_path: Path,
    suffix: str,
    mutate: Any,
    error: str,
) -> None:
    manifest = deepcopy(load_json(maps.MANIFEST))
    mutate(manifest)
    manifest_path = _write_json(tmp_path / f"manifest-{suffix}.json", manifest)
    monkeypatch.setattr(maps, "MANIFEST", manifest_path)
    destination = tmp_path / f"manifest-{suffix}-output.json"

    with pytest.raises(ValueError, match=error):
        maps.verify_map_inventory(UPSTREAM, output_path=destination)
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
            "map-fact",
            lambda output: output["mapFacts"]["mapSwitch"].update(entryBytes=7),
            "model drift",
        ),
        (
            "h1-address",
            lambda output: output["function"].update(cameraAddress=17859),
            "H1 address drift",
        ),
    ],
)
def test_maps_verifier_rejects_schema_valid_model_drift_before_write(
    monkeypatch: pytest.MonkeyPatch,
    tmp_path: Path,
    suffix: str,
    mutate: Any,
    error: str,
) -> None:
    output = maps.build_map_inventory(UPSTREAM)
    mutate(output)
    validate_json(output, OUTPUT_SCHEMA, owner=f"schema-valid common-maps output {suffix}")
    monkeypatch.setattr(maps, "build_map_inventory", lambda _: output)
    destination = tmp_path / f"model-{suffix}.json"

    with pytest.raises(ValueError, match=error):
        maps.verify_map_inventory(UPSTREAM, output_path=destination)
    assert not destination.exists()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_maps_source_path_membership_accepts_another_owned_record_before_fixture_check(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    index = deepcopy(load_json(repo_path("manifests/research-index.json")))
    record_id = "independent.owner.nested-common-map-record"
    index["records"].append(
        _source_record(record_id, "code/common/maps/camerafunctions.asm")
    )
    index_path = _write_json(tmp_path / "nested-common-maps-index.json", index)
    monkeypatch.setattr(maps, "RESEARCH_INDEX", index_path)

    output = maps.build_map_inventory(UPSTREAM)
    assert record_id in output["indexedRecordIds"]
    assert output["summary"]["indexedRecordCount"] == 9
    assert record_id in output["indexedRecordsBySourcePath"][1]["recordIds"]
    destination = tmp_path / "new-member-output.json"
    with pytest.raises(ValueError, match="fixture indexedRecordIds drift"):
        maps.verify_map_inventory(UPSTREAM, output_path=destination)
    assert not destination.exists()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_maps_source_membership_ignores_metadata_but_excludes_outside_root(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    index = deepcopy(load_json(repo_path("manifests/research-index.json")))
    metadata_neutral_id = "independent.owner.metadata-neutral-map-record"
    metadata_neutral = _source_record(
        metadata_neutral_id, "code/common/maps/camerafunctions.asm"
    )
    metadata_neutral["subsystem"] = "unrelated.subsystem"
    metadata_neutral["status"] = "inferred"
    metadata_neutral["documents"] = ["docs/research/unrelated.md"]
    metadata_neutral["evidence"] = []
    index["records"].append(metadata_neutral)
    index["records"].append(
        _source_record("independent.owner.outside-common-maps", "code/common/stats/flags.asm")
    )
    index_path = _write_json(tmp_path / "metadata-common-maps-index.json", index)
    monkeypatch.setattr(maps, "RESEARCH_INDEX", index_path)

    output = maps.build_map_inventory(UPSTREAM)
    assert metadata_neutral_id in output["indexedRecordIds"]
    assert "independent.owner.outside-common-maps" not in output["indexedRecordIds"]
    assert output["summary"]["indexedRecordCount"] == 9


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_maps_rejects_under_root_record_missing_from_discovered_inventory(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    index = deepcopy(load_json(repo_path("manifests/research-index.json")))
    index["records"].append(
        _source_record(
            "independent.owner.missing-common-maps-file",
            "code/common/maps/not-discovered.asm",
        )
    )
    index_path = _write_json(tmp_path / "missing-common-maps-index.json", index)
    monkeypatch.setattr(maps, "RESEARCH_INDEX", index_path)

    with pytest.raises(ValueError, match="absent from the discovered root inventory"):
        maps.build_map_inventory(UPSTREAM)


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_maps_build_does_not_read_golden_fixture(monkeypatch: pytest.MonkeyPatch) -> None:
    monkeypatch.setattr(maps, "FIXTURE", Path("does-not-exist.json"))
    output = maps.build_map_inventory(UPSTREAM)
    assert output["summary"]["indexedRecordCount"] == 8
    assert output["indexedRecordsBySourcePath"][1]["recordIds"][0] == (
        "map.camera-control.wait-for-view-scroll-end"
    )


def test_maps_shared_source_parser_excludes_comments_and_near_misses(tmp_path: Path) -> None:
    source_path = tmp_path / "maps.asm"
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

    parsed = _parse_source_file(source_path, "code/common/maps/maps.asm")
    assert parsed["globalLabels"] == ["CommentOnly"]
    assert parsed["directCalls"] == [
        {"target": "ByteTarget", "siteCount": 1},
        {"target": "DirectTarget", "siteCount": 1},
        {"target": "LongTarget", "siteCount": 1},
        {"target": "ParenthesizedTarget", "siteCount": 1},
    ]
    assert parsed["indirectCallSiteCount"] == 1


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_maps_map_fact_guard_rejects_a_source_control_flow_near_miss(tmp_path: Path) -> None:
    disasm = tmp_path / "disasm"
    source_root = disasm / maps.SOURCE_ROOT
    source_root.mkdir(parents=True)
    upstream_root = UPSTREAM / "disasm" / maps.SOURCE_ROOT
    for source_path in upstream_root.glob("*.asm"):
        (source_root / source_path.name).write_bytes(source_path.read_bytes())
    mapinit = source_root / "mapinit_0.asm"
    mapinit.write_text(
        mapinit.read_text(encoding="utf-8").replace("bmi.w   @Done", "bpl.w   @Done", 1),
        encoding="utf-8",
    )

    with pytest.raises(ValueError, match="battlefield instruction sequence drift"):
        maps._map_facts(disasm)
