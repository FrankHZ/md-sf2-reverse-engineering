from __future__ import annotations

import hashlib
import json
from functools import cache
from pathlib import Path
from typing import Any, Never
from urllib.parse import urldefrag, urljoin, urlsplit

from jsonschema import Draft7Validator, FormatChecker
from referencing import Registry, Resource
from referencing.exceptions import NoSuchResource, Unresolvable
from referencing.jsonschema import DRAFT7, SchemaRegistry

from sf2tool.paths import repo_path

SCHEMA_ROOT = repo_path("schemas")


def load_json(path: Path) -> Any:
    with path.open("r", encoding="utf-8") as handle:
        return json.load(handle)


def _reject_unregistered_resource(uri: str) -> Never:
    """Keep schema resolution closed over resources loaded from the repository."""
    raise NoSuchResource(ref=uri)


def _reference_error(*, owner: str, reference: str) -> ValueError:
    scheme = urlsplit(reference).scheme.lower()
    if scheme in {"http", "https"}:
        reason = "unregistered schema resource; network retrieval is disabled"
    else:
        reason = "unregistered schema resource"
    return ValueError(f"{owner} failed schema reference resolution: {reference} ({reason})")


def _verify_registered_references(
    schema: Any,
    *,
    registry: SchemaRegistry,
    owner: str,
    base_uri: str = "",
) -> None:
    if isinstance(schema, list):
        for value in schema:
            _verify_registered_references(
                value,
                registry=registry,
                owner=owner,
                base_uri=base_uri,
            )
        return
    if not isinstance(schema, dict):
        return

    schema_id = schema.get("$id")
    if isinstance(schema_id, str):
        base_uri = urljoin(base_uri, schema_id)
    reference = schema.get("$ref")
    if isinstance(reference, str) and not reference.startswith("#"):
        resolved_reference = urljoin(base_uri, reference)
        resource_uri, _ = urldefrag(resolved_reference)
        try:
            registry[resource_uri]
        except NoSuchResource:
            raise _reference_error(owner=owner, reference=resolved_reference) from None

    for value in schema.values():
        _verify_registered_references(
            value,
            registry=registry,
            owner=owner,
            base_uri=base_uri,
        )


def build_schema_registry(schema_root: Path) -> SchemaRegistry:
    """Load every schema ID below *schema_root* into a network-disabled registry."""
    schema_root = schema_root.resolve()
    if not schema_root.is_dir():
        raise ValueError(f"schema registry root does not exist: {schema_root}")

    resources: list[tuple[str, Resource[Any]]] = []
    schemas: list[tuple[Path, Any]] = []
    owners_by_id: dict[str, Path] = {}
    for schema_path in sorted(schema_root.rglob("*.schema.json")):
        schema = load_json(schema_path)
        schemas.append((schema_path, schema))
        schema_id = schema.get("$id")
        if schema_id is None:
            continue
        if not isinstance(schema_id, str) or not schema_id:
            raise ValueError(f"schema $id must be a non-empty string: {schema_path}")

        registry_id = schema_id.rstrip("#")
        previous_owner = owners_by_id.get(registry_id)
        if previous_owner is not None:
            raise ValueError(
                f"duplicate schema $id {registry_id!r}: {previous_owner} and {schema_path}"
            )
        owners_by_id[registry_id] = schema_path
        resources.append(
            (
                registry_id,
                Resource.from_contents(schema, default_specification=DRAFT7),
            )
        )

    registry = Registry(retrieve=_reject_unregistered_resource).with_resources(resources)
    for schema_path, schema in schemas:
        _verify_registered_references(
            schema,
            registry=registry,
            owner=str(schema_path),
        )
    return registry


@cache
def tracked_schema_registry() -> SchemaRegistry:
    """Return the immutable registry for tracked project schemas."""
    return build_schema_registry(SCHEMA_ROOT)


def schema_composition_audit(
    schema_paths: list[Path],
    *,
    registry: SchemaRegistry | None = None,
    large_const_bytes: int = 1024,
) -> dict[str, Any]:
    """Report reusable-schema composition without treating file size as correctness."""
    active_registry = tracked_schema_registry() if registry is None else registry
    files: list[dict[str, Any]] = []
    unresolved_references: list[dict[str, str]] = []
    body_owners: dict[str, list[str]] = {}
    referenced_resources: set[str] = set()

    for schema_path in schema_paths:
        schema = load_json(schema_path)
        const_count = 0
        const_payload_bytes = 0
        large_const_count = 0

        def visit(
            value: Any,
            *,
            base_uri: str = "",
            owner: str = str(schema_path),
        ) -> None:
            nonlocal const_count, const_payload_bytes, large_const_count
            if isinstance(value, list):
                for item in value:
                    visit(item, base_uri=base_uri)
                return
            if not isinstance(value, dict):
                return

            schema_id = value.get("$id")
            if isinstance(schema_id, str):
                base_uri = urljoin(base_uri, schema_id)
            if "const" in value:
                payload_size = len(
                    json.dumps(
                        value["const"],
                        ensure_ascii=False,
                        separators=(",", ":"),
                    ).encode("utf-8")
                )
                const_count += 1
                const_payload_bytes += payload_size
                large_const_count += payload_size >= large_const_bytes
            reference = value.get("$ref")
            if isinstance(reference, str) and not reference.startswith("#"):
                resolved_reference = urljoin(base_uri, reference)
                resource_uri, _ = urldefrag(resolved_reference)
                referenced_resources.add(resource_uri)
                try:
                    active_registry[resource_uri]
                except NoSuchResource:
                    unresolved_references.append(
                        {"owner": owner, "reference": resolved_reference}
                    )
            for key, child in value.items():
                if key != "const":
                    visit(child, base_uri=base_uri)

        visit(schema)
        structural_body = {
            key: value
            for key, value in schema.items()
            if key not in {"$schema", "$id", "title", "description"}
        }
        digest = hashlib.sha256(
            json.dumps(
                structural_body,
                ensure_ascii=False,
                sort_keys=True,
                separators=(",", ":"),
            ).encode("utf-8")
        ).hexdigest()
        body_owners.setdefault(digest, []).append(str(schema_path))
        files.append(
            {
                "path": str(schema_path),
                "sizeBytes": schema_path.stat().st_size,
                "constCount": const_count,
                "constPayloadBytes": const_payload_bytes,
                "largeConstCount": large_const_count,
            }
        )

    return {
        "schemaCount": len(files),
        "totalSizeBytes": sum(item["sizeBytes"] for item in files),
        "constCount": sum(item["constCount"] for item in files),
        "constPayloadBytes": sum(item["constPayloadBytes"] for item in files),
        "largeConstCount": sum(item["largeConstCount"] for item in files),
        "referencedResourceCount": len(referenced_resources),
        "unresolvedReferences": unresolved_references,
        "duplicateBodyGroups": [
            owners for owners in body_owners.values() if len(owners) > 1
        ],
        "files": files,
    }


def validate_json(
    instance: Any,
    schema_path: Path,
    *,
    owner: str,
    registry: SchemaRegistry | None = None,
) -> None:
    schema = load_json(schema_path)
    uses_tracked_registry = registry is None
    active_registry = tracked_schema_registry() if registry is None else registry
    if not (
        uses_tracked_registry
        and schema_path.resolve().is_relative_to(SCHEMA_ROOT)
    ):
        _verify_registered_references(schema, registry=active_registry, owner=owner)
    validator = Draft7Validator(
        schema,
        format_checker=FormatChecker(),
        registry=active_registry,
    )
    try:
        errors = sorted(
            validator.iter_errors(instance),
            key=lambda error: list(error.absolute_path),
        )
    except Unresolvable as error:
        reference = str(error.ref)
        raise _reference_error(owner=owner, reference=reference) from error
    if not errors:
        return
    preview = []
    for error in errors[:10]:
        location = ".".join(str(part) for part in error.absolute_path) or "<root>"
        preview.append(f"{location}: {error.message}")
    raise ValueError(f"{owner} failed schema validation:\n - " + "\n - ".join(preview))
