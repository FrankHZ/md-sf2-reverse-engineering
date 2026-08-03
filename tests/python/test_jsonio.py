from __future__ import annotations

import json
from pathlib import Path

import pytest
from referencing.exceptions import NoSuchResource

from sf2tool import jsonio
from sf2tool.jsonio import (
    SCHEMA_ROOT,
    build_schema_registry,
    load_json,
    tracked_schema_registry,
    validate_json,
)


def test_tracked_schema_validator_is_cached_per_worker(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    tracked_schema_registry()
    jsonio._tracked_schema_validator.cache_clear()
    original_load_json = jsonio.load_json
    loads: list[Path] = []

    def recording_load_json(path: Path) -> object:
        loads.append(path.resolve())
        return original_load_json(path)

    monkeypatch.setattr(jsonio, "load_json", recording_load_json)
    schema_path = SCHEMA_ROOT / "rom-manifest.schema.json"
    instance = load_json(Path("manifests/roms/sf2-us.json"))

    try:
        validate_json(instance, schema_path, owner="first cached validation")
        validate_json(instance, schema_path, owner="second cached validation")

        assert loads.count(schema_path.resolve()) == 1
    finally:
        jsonio._tracked_schema_validator.cache_clear()


def test_temporary_schema_validation_does_not_use_the_tracked_cache(
    tmp_path: Path,
) -> None:
    schema_path = tmp_path / "mutable.schema.json"
    _write_schema(schema_path, _draft7_schema("urn:sf2:test:mutable", type="integer"))
    validate_json(1, schema_path, owner="initial temporary schema")

    _write_schema(schema_path, _draft7_schema("urn:sf2:test:mutable", type="string"))
    with pytest.raises(ValueError, match="1 is not of type 'string'"):
        validate_json(1, schema_path, owner="mutated temporary schema")


def _write_schema(path: Path, schema: dict[str, object]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(schema, indent=2) + "\n", encoding="utf-8")


def _draft7_schema(schema_id: str, **keywords: object) -> dict[str, object]:
    return {
        "$schema": "http://json-schema.org/draft-07/schema#",
        "$id": schema_id,
        **keywords,
    }


def test_tracked_schema_registry_preloads_every_declared_id() -> None:
    registry = tracked_schema_registry()
    declared_ids: dict[str, Path] = {}
    for schema_path in sorted(SCHEMA_ROOT.rglob("*.schema.json")):
        schema = load_json(schema_path)
        schema_id = schema.get("$id")
        if schema_id is None:
            continue
        registry_id = schema_id.rstrip("#")
        assert registry_id not in declared_ids
        declared_ids[registry_id] = schema_path
        assert registry.contents(registry_id) == schema

    assert declared_ids


def test_schema_registry_rejects_missing_root_and_duplicate_ids(tmp_path: Path) -> None:
    with pytest.raises(ValueError, match="schema registry root does not exist"):
        build_schema_registry(tmp_path / "missing")

    duplicate_id = "urn:sf2:test:duplicate"
    _write_schema(tmp_path / "one.schema.json", _draft7_schema(duplicate_id, type="integer"))
    _write_schema(tmp_path / "nested" / "two.schema.json", _draft7_schema(duplicate_id + "#"))

    with pytest.raises(ValueError, match=r"duplicate schema \$id 'urn:sf2:test:duplicate'"):
        build_schema_registry(tmp_path)


@pytest.mark.parametrize(
    ("reference", "reason"),
    [
        ("urn:sf2:test:missing", "unregistered schema resource"),
        (
            "https://schemas.example.invalid/sf2/missing.schema.json",
            "unregistered schema resource; network retrieval is disabled",
        ),
    ],
)
def test_schema_validation_rejects_unknown_and_network_references(
    tmp_path: Path,
    reference: str,
    reason: str,
) -> None:
    registry_root = tmp_path / "registry"
    _write_schema(
        registry_root / "known.schema.json",
        _draft7_schema("urn:sf2:test:known", type="object"),
    )
    invalid_registry_path = registry_root / "invalid.schema.json"
    _write_schema(
        invalid_registry_path,
        _draft7_schema("urn:sf2:test:invalid", **{"$ref": reference}),
    )
    with pytest.raises(ValueError, match=reason):
        build_schema_registry(registry_root)
    invalid_registry_path.unlink()

    root_path = tmp_path / "root.schema.json"
    _write_schema(root_path, _draft7_schema("urn:sf2:test:root", **{"$ref": reference}))
    registry = build_schema_registry(registry_root)

    with pytest.raises(ValueError, match=reason):
        validate_json({}, root_path, owner="temporary root", registry=registry)

    if reference.startswith("https://"):
        with pytest.raises(NoSuchResource):
            registry.get_or_retrieve(reference)


def test_schema_registry_resolves_bounded_cyclic_references(tmp_path: Path) -> None:
    node_a = _draft7_schema(
        "urn:sf2:test:node-a",
        type="object",
        required=["value", "next"],
        additionalProperties=False,
        properties={
            "value": {"type": "integer"},
            "next": {"anyOf": [{"type": "null"}, {"$ref": "urn:sf2:test:node-b"}]},
        },
    )
    node_b = _draft7_schema(
        "urn:sf2:test:node-b",
        type="object",
        required=["value", "next"],
        additionalProperties=False,
        properties={
            "value": {"type": "string"},
            "next": {"anyOf": [{"type": "null"}, {"$ref": "urn:sf2:test:node-a"}]},
        },
    )
    node_a_path = tmp_path / "node-a.schema.json"
    _write_schema(node_a_path, node_a)
    _write_schema(tmp_path / "node-b.schema.json", node_b)
    registry = build_schema_registry(tmp_path)

    validate_json(
        {"value": 1, "next": {"value": "two", "next": {"value": 3, "next": None}}},
        node_a_path,
        owner="cyclic instance",
        registry=registry,
    )
    with pytest.raises(ValueError, match=r"next: .* is not valid under any of the given schemas"):
        validate_json(
            {
                "value": 1,
                "next": {"value": "two", "next": {"value": "three", "next": None}},
            },
            node_a_path,
            owner="invalid cyclic instance",
            registry=registry,
        )
