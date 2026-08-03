from __future__ import annotations

from pathlib import Path
from typing import Any

from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

ID = "sf2-map-events-static-v1"
FIXTURE = repo_path("tests/fixtures/h2/map-events-static-v1.json")
FIXTURE_ROOT = repo_path("tests/fixtures/h2/map-events")
FIXTURE_SCHEMA = repo_path("schemas/h2/map-events-fixture-index.schema.json")
SHARD_SCHEMA = repo_path("schemas/h2/map-events-fixture-shard.schema.json")
OUTPUT_SCHEMA = repo_path("schemas/h2/map-events-output.schema.json")
TARGET_PROGRAM_SCHEMA = repo_path("schemas/h2/map-events-target-program.schema.json")
SECTION_SCHEMAS = {
    "routing-setup": repo_path("schemas/h2/map-events-routing-setup.schema.json"),
    "entity-programs": repo_path("schemas/h2/map-events-entity-programs.schema.json"),
    "zone-programs": repo_path("schemas/h2/map-events-zone-programs.schema.json"),
    "item-programs": repo_path("schemas/h2/map-events-item-programs.schema.json"),
    "operation-vocabulary": repo_path(
        "schemas/h2/map-events-operation-vocabulary.schema.json"
    ),
    "direct-flags": repo_path("schemas/h2/map-events-direct-flags.schema.json"),
    "script-invocation": repo_path(
        "schemas/h2/map-events-script-invocation.schema.json"
    ),
    "textbox": repo_path("schemas/h2/map-events-textbox.schema.json"),
    "sound-commands": repo_path("schemas/h2/map-events-sound-commands.schema.json"),
}
COMPOSED_SCHEMAS = (
    TARGET_PROGRAM_SCHEMA,
    *SECTION_SCHEMAS.values(),
    OUTPUT_SCHEMA,
    FIXTURE_SCHEMA,
    SHARD_SCHEMA,
)


def _tracked_shard_path(section: str, declared_path: str) -> Path:
    expected_relative = f"tests/fixtures/h2/map-events/{section}.json"
    if declared_path != expected_relative:
        raise ValueError(f"map events fixture shard path drift: {section}")
    shard_path = repo_path(declared_path).resolve(strict=True)
    fixture_root = FIXTURE_ROOT.resolve(strict=True)
    if shard_path.parent != fixture_root:
        raise ValueError(f"map events fixture shard escaped tracked root: {section}")
    return shard_path


def load_map_events_fixture(fixture_path: Path | None = None) -> dict[str, Any]:
    """Load, validate, and deterministically recompose the semantic fixture shards."""
    index_path = FIXTURE if fixture_path is None else fixture_path
    index = load_json(index_path)
    validate_json(index, FIXTURE_SCHEMA, owner=str(index_path))
    sections = tuple(descriptor["section"] for descriptor in index["shards"])
    if sections != tuple(SECTION_SCHEMAS):
        raise ValueError("map events fixture shard section order drift")

    combined: dict[str, Any] = {}
    for descriptor in index["shards"]:
        section = descriptor["section"]
        shard_path = _tracked_shard_path(section, descriptor["path"])
        shard = load_json(shard_path)
        validate_json(shard, SHARD_SCHEMA, owner=str(shard_path))
        if (
            shard["id"] != f"{ID}:{section}"
            or shard["fixtureId"] != index["id"]
            or shard["section"] != section
        ):
            raise ValueError(f"map events fixture shard identity drift: {section}")
        shard_fields = list(shard["expected"])
        if shard_fields != descriptor["fields"]:
            raise ValueError(f"map events fixture shard field inventory drift: {section}")
        overlap = set(combined).intersection(shard_fields)
        if overlap:
            raise ValueError(
                f"map events fixture shard duplicate field drift: {section}: {sorted(overlap)}"
            )
        validate_json(
            shard["expected"],
            SECTION_SCHEMAS[section],
            owner=f"{shard_path} expected section",
        )
        combined.update(shard["expected"])

    field_order = index["fieldOrder"]
    if set(combined) != set(field_order):
        raise ValueError("map events fixture shard complete field coverage drift")
    expected = {field: combined[field] for field in field_order}
    validate_json(expected, OUTPUT_SCHEMA, owner="recomposed map events fixture")
    return {
        "schemaVersion": index["schemaVersion"],
        "id": index["id"],
        "upstreamCommit": index["upstreamCommit"],
        "romSha256": index["romSha256"],
        "function": index["function"],
        "expected": expected,
    }
