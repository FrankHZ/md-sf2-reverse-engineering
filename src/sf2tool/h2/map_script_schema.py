from __future__ import annotations

from functools import cache
from pathlib import Path
from typing import Any

from sf2tool.jsonio import load_json
from sf2tool.paths import repo_path

CORE_SCHEMA = repo_path("schemas/h2/map-script-engine-core.schema.json")
DIALOGUE_SCHEMA = repo_path("schemas/h2/map-script-engine-dialogue.schema.json")
STATE_BLOCK_SCHEMA = repo_path("schemas/h2/map-script-engine-state-block.schema.json")
ENTITY_SCHEMA = repo_path("schemas/h2/map-script-engine-entity.schema.json")
PRESENTATION_UI_SCHEMA = repo_path(
    "schemas/h2/map-script-engine-presentation-ui.schema.json"
)
MAP_CONTROL_SCHEMA = repo_path("schemas/h2/map-script-engine-map-control.schema.json")
SCRIPT_CONTROL_SCHEMA = repo_path("schemas/h2/map-script-engine-script-control.schema.json")
COMPONENT_SCHEMAS = (
    CORE_SCHEMA,
    DIALOGUE_SCHEMA,
    STATE_BLOCK_SCHEMA,
    ENTITY_SCHEMA,
    PRESENTATION_UI_SCHEMA,
    MAP_CONTROL_SCHEMA,
    SCRIPT_CONTROL_SCHEMA,
)
OUTPUT_SCHEMA = repo_path("schemas/h2/map-script-engine-output.schema.json")
FIXTURE_SCHEMA = repo_path("schemas/h2/map-script-engine-fixture.schema.json")
COMPOSED_SCHEMAS = (*COMPONENT_SCHEMAS, OUTPUT_SCHEMA, FIXTURE_SCHEMA)


@cache
def map_script_schema_definitions() -> dict[str, tuple[Path, dict[str, Any]]]:
    """Return the unique tracked owner and body for each composed definition."""
    definitions: dict[str, tuple[Path, dict[str, Any]]] = {}
    for schema_path in COMPONENT_SCHEMAS:
        for name, definition in load_json(schema_path)["definitions"].items():
            if name in definitions:
                raise ValueError(f"duplicate map-script schema definition: {name}")
            definitions[name] = (schema_path, definition)
    return definitions


def map_script_schema_definition(name: str) -> dict[str, Any]:
    """Load one shared definition by its unique component-owned name."""
    try:
        return map_script_schema_definitions()[name][1]
    except KeyError as error:
        raise ValueError(f"unknown map-script schema definition: {name}") from error
