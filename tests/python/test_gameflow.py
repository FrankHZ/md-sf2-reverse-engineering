from __future__ import annotations

import json
from copy import deepcopy

import pytest

from sf2tool.h2 import gameflow
from sf2tool.h2.gameflow import build_gameflow_inventory
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

UPSTREAM = repo_path("local/upstream/SF2DISASM")
OUTPUT_SCHEMA = repo_path("schemas/gameflow-core-static.schema.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-gameflow-core-static-fixture.schema.json")
FIXTURE_PATH = repo_path("tests/fixtures/h2/gameflow-core-static-v1.json")


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_gameflow_source_scope_join_keeps_fourteen_records_over_thirteen_files() -> None:
    fixture = load_json(FIXTURE_PATH)
    output = build_gameflow_inventory(UPSTREAM)

    assert output["indexedRecordIds"] == fixture["expected"]["indexedRecordIds"]
    assert output["indexedSourcePaths"] == fixture["expected"]["indexedSourcePaths"]
    assert output["summary"]["indexedRecordCount"] == len(output["indexedRecordIds"]) == 14
    assert output["summary"]["indexedFileCount"] == len(output["indexedSourcePaths"]) == 13
    assert "map.block-mutation.copy-helper" in output["indexedRecordIds"]
    assert output["indexedSourcePaths"] == output["scopes"]


def test_gameflow_index_schemas_pin_the_exact_ordered_scope_and_records() -> None:
    output_schema = load_json(OUTPUT_SCHEMA)
    fixture_schema = load_json(FIXTURE_SCHEMA)
    fixture = load_json(FIXTURE_PATH)

    expected = fixture["expected"]
    assert output_schema["properties"]["scopes"]["const"] == expected["indexedSourcePaths"]
    assert output_schema["properties"]["indexedRecordIds"]["const"] == expected[
        "indexedRecordIds"
    ]
    assert output_schema["properties"]["indexedSourcePaths"]["const"] == expected[
        "indexedSourcePaths"
    ]
    assert fixture_schema["properties"]["expected"]["properties"]["indexedRecordIds"][
        "const"
    ] == expected["indexedRecordIds"]
    assert fixture_schema["properties"]["expected"]["properties"]["indexedSourcePaths"][
        "const"
    ] == expected["indexedSourcePaths"]
    assert output_schema["properties"]["summary"]["additionalProperties"] is False


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_gameflow_index_schemas_reject_missing_or_reordered_copy_helper() -> None:
    fixture = load_json(FIXTURE_PATH)
    missing_field_fixture = deepcopy(fixture)
    del missing_field_fixture["expected"]["indexedSourcePaths"]
    with pytest.raises(ValueError, match="indexedSourcePaths"):
        validate_json(missing_field_fixture, FIXTURE_SCHEMA, owner="missing gameflow index field")

    extra_field_fixture = deepcopy(fixture)
    extra_field_fixture["expected"]["unexpectedIndexedRecord"] = True
    with pytest.raises(ValueError, match="unexpectedIndexedRecord"):
        validate_json(extra_field_fixture, FIXTURE_SCHEMA, owner="extra gameflow index field")

    malformed_fixture = deepcopy(fixture)
    malformed_fixture["expected"]["indexedRecordIds"].remove("map.block-mutation.copy-helper")
    with pytest.raises(ValueError, match="indexedRecordIds"):
        validate_json(malformed_fixture, FIXTURE_SCHEMA, owner="missing gameflow index fixture")

    malformed_output = build_gameflow_inventory(UPSTREAM)
    malformed_output["indexedRecordIds"].reverse()
    with pytest.raises(ValueError, match="indexedRecordIds"):
        validate_json(malformed_output, OUTPUT_SCHEMA, owner="reordered gameflow output")


def test_gameflow_source_scope_join_rejects_missing_layout_file_coverage(
    monkeypatch: pytest.MonkeyPatch, tmp_path
) -> None:
    index = load_json(repo_path("manifests/research-index.json"))
    z80_record = next(
        record for record in index["records"] if record["id"] == "gameflow.start.z80-init"
    )
    z80_record["sourcePath"] = "code/other/z80init.asm"
    mutated_index = tmp_path / "research-index-missing-gameflow-source.json"
    mutated_index.write_bytes((json.dumps(index, indent=2) + "\n").encode("utf-8"))
    monkeypatch.setattr(gameflow, "RESEARCH_INDEX", mutated_index)

    with pytest.raises(ValueError, match="research-index source coverage drift"):
        gameflow._index_records_for_gameflow_scope()
