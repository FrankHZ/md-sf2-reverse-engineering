from __future__ import annotations

import json
from copy import deepcopy
from pathlib import Path
from typing import Any

import pytest

from sf2tool.h2 import battle_functions
from sf2tool.h2.battlefield import _require_ordered_fragments
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

UPSTREAM = repo_path("local/upstream/SF2DISASM")
OUTPUT_SCHEMA = repo_path("schemas/battle-functions-static.schema.json")
FIXTURE_SCHEMA = repo_path("schemas/h2-battle-functions-static-fixture.schema.json")
FIXTURE_PATH = repo_path("tests/fixtures/h2/battle-functions-static-v1.json")


def _write_json(path: Path, value: object) -> Path:
    path.write_bytes((json.dumps(value, indent=2) + "\n").encode("utf-8"))
    return path


def _write_fixture(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path, fixture: dict[str, Any]
) -> Path:
    path = _write_json(tmp_path / "battle-functions-fixture.json", fixture)
    monkeypatch.setattr(battle_functions, "FIXTURE", path)
    return path


def _write_fixture_schema(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path, schema: dict[str, Any]
) -> Path:
    path = _write_json(tmp_path / "battle-functions-fixture.schema.json", schema)
    monkeypatch.setattr(battle_functions, "FIXTURE_SCHEMA", path)
    return path


def _source_record(record_id: str, source_path: str) -> dict[str, Any]:
    index = load_json(repo_path("manifests/research-index.json"))
    record = deepcopy(
        next(
            item
            for item in index["records"]
            if item["id"] == "battle.functions.pulsating-grid"
        )
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
def test_battle_functions_source_root_join_keeps_16_records_over_7_paths() -> None:
    fixture = load_json(FIXTURE_PATH)
    output = battle_functions.build_battle_functions_inventory(UPSTREAM)

    assert output["indexedRecordIds"] == fixture["expected"]["indexedRecordIds"]
    assert output["indexedSourcePaths"] == fixture["expected"]["indexedSourcePaths"]
    assert output["indexedRecordsBySourcePath"] == fixture["expected"][
        "indexedRecordsBySourcePath"
    ]
    assert output["summary"]["indexedRecordCount"] == len(output["indexedRecordIds"]) == 16
    assert output["summary"]["indexedFileCount"] == len(output["indexedSourcePaths"]) == 7
    assert output["summary"]["fileCount"] == len(output["files"]) == 7
    assert output["summary"]["playerControlFunctionCount"] == 6
    assert output["summary"]["playerControlStatementCount"] == 1039
    assert output["indexedRecordsBySourcePath"][0] == {
        "sourcePath": "code/gameflow/battle/battlefunctions/battlefunctions_0.asm",
        "recordIds": [
            "battle.functions.choose-target",
            "battle.functions.control-cursor",
            "battle.functions.pulsating-grid",
            "battle.functions.set-cursor-target",
            "map.camera-control.destination-service",
        ],
    }


def test_battle_function_schemas_keep_the_16_record_corpus_in_fixture_and_verifier() -> None:
    output_schema = load_json(OUTPUT_SCHEMA)
    fixture_schema = load_json(FIXTURE_SCHEMA)

    for field in (
        "indexedRecordIds",
        "indexedSourcePaths",
        "indexedRecordsBySourcePath",
    ):
        assert "const" not in output_schema["properties"][field]
        assert "const" not in fixture_schema["properties"]["expected"]["properties"][field]
    assert output_schema["properties"]["indexedRecordIds"].get("maxItems") != 16
    assert fixture_schema["properties"]["expected"]["properties"][
        "indexedRecordIds"
    ].get("maxItems") != 16
    assert output_schema["properties"]["indexedSourcePaths"] == {
        "type": "array",
        "minItems": 7,
        "maxItems": 7,
        "uniqueItems": True,
        "items": {"$ref": "#/definitions/sourcePath"},
    }
    assert output_schema["definitions"]["summary"]["properties"][
        "indexedFileCount"
    ] == {"const": 7}
    assert output_schema["definitions"]["summary"]["properties"][
        "playerControlStatementCount"
    ] == {"type": "integer", "minimum": 0}
    assert fixture_schema["properties"]["romSha256"] == {
        "const": "9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9"
    }
    assert fixture_schema["properties"]["upstreamCommit"] == {
        "const": "c834c652b6862bc5679fd7f69a38a7093206efc6"
    }
    for schema in (output_schema, fixture_schema):
        relation = schema["definitions"]["indexedRecordsBySourcePath"]
        assert relation["additionalProperties"] is False
        assert relation["required"] == ["sourcePath", "recordIds"]
        assert relation["properties"]["recordIds"]["uniqueItems"] is True
        assert schema["definitions"]["functionAddresses"]["additionalProperties"] is False
        assert len(schema["definitions"]["functionAddresses"]["required"]) == 15
        assert len(schema["definitions"]["functionFacts"]["required"]) == 6
    assert output_schema["definitions"]["playerControl"]["properties"]["functions"] == {
        "type": "array",
        "minItems": 6,
        "maxItems": 6,
        "items": {"$ref": "#/definitions/playerControlFunction"},
    }
    assert _schema_consts(output_schema) == {
        "$.properties.schemaVersion": 1,
        "$.properties.id": "sf2-battle-functions-static-v1",
        "$.properties.scope": "code/gameflow/battle/battlefunctions",
        "$.definitions.summary.properties.fileCount": 7,
        "$.definitions.summary.properties.indexedFileCount": 7,
    }
    assert _schema_consts(fixture_schema) == {
        "$.properties.schemaVersion": 1,
        "$.properties.id": "sf2-battle-functions-static-v1",
        "$.properties.romSha256": (
            "9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9"
        ),
        "$.properties.upstreamCommit": "c834c652b6862bc5679fd7f69a38a7093206efc6",
    }


@pytest.mark.parametrize(
    ("owner", "mutate"),
    [
        ("fixture", lambda value: value["expected"].pop("indexedRecordIds")),
        (
            "fixture",
            lambda value: value["expected"].update(indexedRecordIDz=[]),
        ),
        (
            "fixture",
            lambda value: value["expected"]["indexedRecordsBySourcePath"][0].update(
                unexpected=True
            ),
        ),
        ("fixture", lambda value: value["function"].update(pulsatingGridAddress="wrong")),
        (
            "fixture",
            lambda value: value["expected"]["indexedRecordIds"].__setitem__(
                0, "not/a-record-id"
            ),
        ),
        (
            "fixture",
            lambda value: value["expected"]["indexedRecordIds"].__setitem__(
                1, value["expected"]["indexedRecordIds"][0]
            ),
        ),
        (
            "fixture",
            lambda value: value["expected"]["indexedRecordsBySourcePath"][0][
                "recordIds"
            ].append(value["expected"]["indexedRecordsBySourcePath"][0]["recordIds"][0]),
        ),
        (
            "fixture",
            lambda value: value["expected"]["indexedSourcePaths"].pop(),
        ),
        (
            "fixture",
            lambda value: value["expected"]["indexedSourcePaths"].append(
                "code/gameflow/battle/battlefunctions/extra.asm"
            ),
        ),
        (
            "fixture",
            lambda value: value["expected"]["indexedRecordsBySourcePath"].pop(),
        ),
        ("output", lambda value: value["summary"].pop("fileCount")),
        (
            "output",
            lambda value: value["summary"].update(indexedRecordCount="sixteen"),
        ),
        (
            "output",
            lambda value: value["indexedSourcePaths"].__setitem__(
                0, "code/gameflow/battle/battlefunctions/not-valid.txt"
            ),
        ),
        (
            "output",
            lambda value: value["indexedRecordIds"].__setitem__(
                1, value["indexedRecordIds"][0]
            ),
        ),
    ],
)
def test_battle_function_schemas_reject_recursive_structural_mutations(
    owner: str, mutate: Any
) -> None:
    if owner == "fixture":
        value = deepcopy(load_json(FIXTURE_PATH))
        schema = FIXTURE_SCHEMA
    else:
        if not UPSTREAM.is_dir():
            pytest.skip("pinned upstream checkout is unavailable")
        value = battle_functions.build_battle_functions_inventory(UPSTREAM)
        schema = OUTPUT_SCHEMA
    mutate(value)

    with pytest.raises(ValueError):
        validate_json(value, schema, owner=f"battle-functions {owner} structural mutation")


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize("field", ["indexedRecordIds", "indexedSourcePaths"])
def test_battle_function_output_schema_rejects_duplicate_join_lists(field: str) -> None:
    output = battle_functions.build_battle_functions_inventory(UPSTREAM)
    output[field][1] = output[field][0]

    with pytest.raises(ValueError, match=field):
        validate_json(output, OUTPUT_SCHEMA, owner=f"battle-functions duplicate {field}")


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
            "same-row-duplicate-record",
            lambda output: output["indexedRecordsBySourcePath"][0]["recordIds"].append(
                output["indexedRecordsBySourcePath"][0]["recordIds"][0]
            ),
            "indexedRecordsBySourcePath.0.recordIds",
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
            lambda output: output["indexedRecordsBySourcePath"][0]["recordIds"].reverse(),
            "indexed relation record order drift",
        ),
        (
            "relation-path",
            lambda output: output["indexedRecordsBySourcePath"][0].update(
                sourcePath="code/gameflow/battle/battlefunctions/not-discovered.asm"
            ),
            "indexed relation source inventory drift",
        ),
        (
            "summary-record-count",
            lambda output: output["summary"].update(indexedRecordCount=15),
            "summary indexedRecordCount relation drift",
        ),
        (
            "static-summary",
            lambda output: output["summary"].update(sourceLineCount=3181),
            "static summary drift",
        ),
        (
            "player-control-summary",
            lambda output: output["playerControl"]["summary"].update(
                statementCount=1038
            ),
            "player-control summary drift",
        ),
        (
            "source-inventory",
            lambda output: output["files"][0].update(
                path="code/gameflow/battle/battlefunctions/not-discovered.asm"
            ),
            "source inventory drift",
        ),
    ],
)
def test_battle_function_verifier_rejects_schema_valid_join_drift_before_write(
    monkeypatch: pytest.MonkeyPatch,
    tmp_path: Path,
    suffix: str,
    mutate: Any,
    error: str,
) -> None:
    output = battle_functions.build_battle_functions_inventory(UPSTREAM)
    mutate(output)
    if suffix != "same-row-duplicate-record":
        validate_json(output, OUTPUT_SCHEMA, owner=f"schema-valid battle-functions {suffix}")
    destination = tmp_path / f"battle-functions-{suffix}.json"
    monkeypatch.setattr(battle_functions, "build_battle_functions_inventory", lambda _: output)

    with pytest.raises(ValueError, match=error):
        battle_functions.verify_battle_functions_inventory(UPSTREAM, output_path=destination)
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
                "battle.functions.future-record"
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
                0, "battle.functions.wrong-but-schema-valid"
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
                0, "code/gameflow/battle/battlefunctions/not-discovered.asm"
            ),
            "indexedSourcePaths drift",
        ),
        (
            "wrong-relation-record",
            lambda fixture: fixture["expected"]["indexedRecordsBySourcePath"][0].update(
                recordIds=["battle.functions.wrong-but-schema-valid"]
            ),
            "indexedRecordsBySourcePath drift",
        ),
        (
            "function-address",
            lambda fixture: fixture["function"].update(pulsatingGridAddress=32769),
            "function address drift",
        ),
        (
            "function-fact",
            lambda fixture: fixture["expected"]["functionFacts"]["moveSfx"].update(
                outsideBattle=1
            ),
            "model drift",
        ),
        (
            "player-control-summary",
            lambda fixture: fixture["expected"]["playerControlSummary"].update(
                statementCount=1038
            ),
            "player-control summary drift",
        ),
    ],
)
def test_battle_function_verifier_rejects_fixture_drift_before_write(
    monkeypatch: pytest.MonkeyPatch,
    tmp_path: Path,
    suffix: str,
    mutate: Any,
    error: str,
) -> None:
    fixture = deepcopy(load_json(FIXTURE_PATH))
    mutate(fixture)
    validate_json(fixture, FIXTURE_SCHEMA, owner=f"battle-functions fixture {suffix}")
    _write_fixture(monkeypatch, tmp_path, fixture)
    destination = tmp_path / f"fixture-{suffix}.json"

    with pytest.raises(ValueError, match=error):
        battle_functions.verify_battle_functions_inventory(UPSTREAM, output_path=destination)
    assert not destination.exists()


@pytest.mark.parametrize(
    ("field", "value"),
    [
        ("upstreamCommit", "0" * 40),
        ("romSha256", "0" * 64),
    ],
)
def test_battle_function_fixture_schema_preserves_legacy_provenance_constants(
    field: str, value: str
) -> None:
    fixture = deepcopy(load_json(FIXTURE_PATH))
    fixture[field] = value

    with pytest.raises(ValueError, match=field):
        validate_json(fixture, FIXTURE_SCHEMA, owner=f"battle-functions provenance {field}")


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("field", "value", "error"),
    [
        ("upstreamCommit", "0" * 40, "fixture upstream provenance drift"),
        ("romSha256", "0" * 64, "fixture ROM provenance drift"),
    ],
)
def test_battle_function_verifier_derives_pinned_provenance_when_fixture_schema_agrees(
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
    validate_json(fixture, fixture_schema_path, owner=f"schema-aligned battle-functions {field}")
    _write_fixture(monkeypatch, tmp_path, fixture)
    destination = tmp_path / f"provenance-{field}.json"

    with pytest.raises(ValueError, match=error):
        battle_functions.verify_battle_functions_inventory(UPSTREAM, output_path=destination)
    assert not destination.exists()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_battle_function_verifier_rejects_wrong_toolchain_commit_before_write(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    toolchain = deepcopy(load_json(battle_functions.TOOLCHAIN))
    toolchain["sf2disasm"]["commit"] = "0" * 40
    toolchain_path = _write_json(tmp_path / "wrong-toolchain.json", toolchain)
    monkeypatch.setattr(battle_functions, "TOOLCHAIN", toolchain_path)
    destination = tmp_path / "wrong-toolchain-output.json"

    with pytest.raises(ValueError, match="requires SF2DISASM"):
        battle_functions.verify_battle_functions_inventory(UPSTREAM, output_path=destination)
    assert not destination.exists()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
@pytest.mark.parametrize(
    ("suffix", "mutate", "error"),
    [
        (
            "summary",
            lambda manifest: manifest["summary"].update(indexedRecordCount=15),
            "summary drift",
        ),
        (
            "digest",
            lambda manifest: manifest.update(outputSha256="0" * 64),
            "canonical hash drift",
        ),
    ],
)
def test_battle_function_verifier_rejects_stale_manifest_before_write(
    monkeypatch: pytest.MonkeyPatch,
    tmp_path: Path,
    suffix: str,
    mutate: Any,
    error: str,
) -> None:
    manifest = deepcopy(load_json(battle_functions.MANIFEST))
    mutate(manifest)
    manifest_path = _write_json(tmp_path / f"manifest-{suffix}.json", manifest)
    monkeypatch.setattr(battle_functions, "MANIFEST", manifest_path)
    destination = tmp_path / f"manifest-{suffix}-output.json"

    with pytest.raises(ValueError, match=error):
        battle_functions.verify_battle_functions_inventory(UPSTREAM, output_path=destination)
    assert not destination.exists()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_battle_function_source_membership_accepts_new_owned_record_before_fixture_check(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    index = deepcopy(load_json(repo_path("manifests/research-index.json")))
    record_id = "independent.owner.battle-camera-service"
    index["records"].append(
        _source_record(
            record_id,
            "code/gameflow/battle/battlefunctions/battlefunctions_0.asm",
        )
    )
    index_path = _write_json(tmp_path / "new-owned-record-index.json", index)
    monkeypatch.setattr(battle_functions, "RESEARCH_INDEX", index_path)

    output = battle_functions.build_battle_functions_inventory(UPSTREAM)
    assert record_id in output["indexedRecordIds"]
    assert output["summary"]["indexedRecordCount"] == 17
    assert output["summary"]["indexedFileCount"] == 7
    assert record_id in output["indexedRecordsBySourcePath"][0]["recordIds"]
    destination = tmp_path / "new-owned-record-output.json"
    with pytest.raises(ValueError, match="indexedRecordIds drift"):
        battle_functions.verify_battle_functions_inventory(UPSTREAM, output_path=destination)
    assert not destination.exists()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_battle_function_source_membership_has_no_record_metadata_exception(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    index = deepcopy(load_json(repo_path("manifests/research-index.json")))
    record_id = "independent.owner.metadata-does-not-filter"
    record = _source_record(
        record_id,
        "code/gameflow/battle/battlefunctions/battlefunctions_0.asm",
    )
    record["subsystem"] = "independent.owner"
    record["status"] = "inferred"
    record["documents"] = ["docs/research/unrelated.md"]
    record["evidence"] = []
    index["records"].append(record)
    index_path = _write_json(tmp_path / "metadata-independent-index.json", index)
    monkeypatch.setattr(battle_functions, "RESEARCH_INDEX", index_path)

    output = battle_functions.build_battle_functions_inventory(UPSTREAM)
    assert record_id in output["indexedRecordIds"]


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_battle_function_source_membership_excludes_outside_record(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    index = deepcopy(load_json(repo_path("manifests/research-index.json")))
    index["records"].append(
        _source_record(
            "independent.owner.outside-battle-functions",
            "code/common/stats/flags.asm",
        )
    )
    index_path = _write_json(tmp_path / "outside-root-index.json", index)
    monkeypatch.setattr(battle_functions, "RESEARCH_INDEX", index_path)

    output = battle_functions.build_battle_functions_inventory(UPSTREAM)
    assert "independent.owner.outside-battle-functions" not in output["indexedRecordIds"]
    assert output["summary"]["indexedRecordCount"] == 16
    destination = tmp_path / "outside-root-output.json"
    result = battle_functions.verify_battle_functions_inventory(UPSTREAM, output_path=destination)
    assert result["IndexedRecords"] == 16
    assert destination.is_file()


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_battle_function_rejects_under_root_record_absent_from_discovered_inventory(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    index = deepcopy(load_json(repo_path("manifests/research-index.json")))
    index["records"].append(
        _source_record(
            "independent.owner.missing-battle-functions-file",
            "code/gameflow/battle/battlefunctions/not-discovered.asm",
        )
    )
    index_path = _write_json(tmp_path / "missing-root-file-index.json", index)
    monkeypatch.setattr(battle_functions, "RESEARCH_INDEX", index_path)

    with pytest.raises(ValueError, match="absent from the discovered root inventory"):
        battle_functions.build_battle_functions_inventory(UPSTREAM)


@pytest.mark.skipif(not UPSTREAM.is_dir(), reason="pinned upstream checkout is unavailable")
def test_battle_function_build_does_not_read_golden_fixture(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    monkeypatch.setattr(battle_functions, "FIXTURE", Path("does-not-exist.json"))
    output = battle_functions.build_battle_functions_inventory(UPSTREAM)
    assert output["summary"]["indexedRecordCount"] == 16


def test_battle_function_parsers_reject_comment_and_near_miss_shapes(
    tmp_path: Path,
) -> None:
    statements = battle_functions._control_statements(
        "Real:\n"
        "    jsr.w Target(pc) ; jmp CommentOnly\n"
        "; bsr.s CommentOnly\n"
        "    bsr.s (Second).w\n"
        "    move.w #1,d0 &\n"
        "    ; continuation comment\n"
        "    addq.w #1,d0\n"
    )
    assert statements == [
        "jsr.w Target(pc)",
        "bsr.s (Second).w",
        "move.w #1,d0 addq.w #1,d0",
    ]
    assert battle_functions._direct_calls(statements) == {"Target": 1, "Second": 1}
    assert battle_functions._direct_calls(["move.w jmp NotAnInstruction,d0"]) == {}

    source = tmp_path / "function.asm"
    source.write_text(
        "RealFunction:\n    nop\n; End of function RealFunction\n",
        encoding="utf-8",
    )
    assert battle_functions._function_segments(source.read_text(encoding="utf-8"), "RealFunction")
    source.write_text("RealFunction:\n    nop\n", encoding="utf-8")
    with pytest.raises(ValueError, match="function body is missing"):
        battle_functions._function_segments(source.read_text(encoding="utf-8"), "RealFunction")


def test_battle_function_ordered_use_site_guard_rejects_operand_mutation(
    tmp_path: Path,
) -> None:
    source = tmp_path / "guard.asm"
    source.write_text(
        "ControlCursorEntity:\nandi.w  #INPUT_B|INPUT_C|INPUT_A,d0\n",
        encoding="utf-8",
    )
    _require_ordered_fragments(
        source,
        ["ControlCursorEntity:", "andi.w  #INPUT_B|INPUT_C|INPUT_A,d0"],
    )
    source.write_text(
        "ControlCursorEntity:\nandi.w  #INPUT_B|INPUT_C,d0\n",
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="instruction sequence drift"):
        _require_ordered_fragments(
            source,
            ["ControlCursorEntity:", "andi.w  #INPUT_B|INPUT_C|INPUT_A,d0"],
        )
    source.write_text(
        "ControlCursorEntity:\nori.w  #INPUT_B|INPUT_C|INPUT_A,d0\n",
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="instruction sequence drift"):
        _require_ordered_fragments(
            source,
            ["ControlCursorEntity:", "andi.w  #INPUT_B|INPUT_C|INPUT_A,d0"],
        )
    source.write_text(
        "andi.w  #INPUT_B|INPUT_C|INPUT_A,d0\nControlCursorEntity:\n",
        encoding="utf-8",
    )
    with pytest.raises(ValueError, match="instruction sequence drift"):
        _require_ordered_fragments(
            source,
            ["ControlCursorEntity:", "andi.w  #INPUT_B|INPUT_C|INPUT_A,d0"],
        )
