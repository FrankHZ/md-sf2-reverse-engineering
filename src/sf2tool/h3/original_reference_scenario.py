"""Public-safe, data-only original-reference scenario API preflight.

This facade validates a generic protocol descriptor and emits no files.  It has
no ROM path, candidate reservation, private receipt, or emulator launch path;
separately admitted scenario evidence must add those concerns in its own slice.
"""

from __future__ import annotations

import json
import re
from collections.abc import Mapping
from pathlib import Path
from typing import Any

from sf2tool.h3.original_reference_transport import (
    TransportError,
    file_identity,
    sha256,
    validate_passive_lua_source,
)
from sf2tool.jsonio import load_json, validate_json
from sf2tool.paths import repo_path

SCENARIO_API_ID = "sf2-original-reference-replay-scenario-api-v1"
SYNTHETIC_ARTIFACT_DOMAIN = (
    "sf2tool/original-reference-replay-scenario-api/public-synthetic-artifact/v1:"
)
FIXTURE_PATH = repo_path("tests/fixtures/core/original-reference-replay-scenario-api-v1.json")
SCENARIO_SCHEMA = repo_path("schemas/core/original-reference-replay-scenario-api.schema.json")
RECEIPT_SCHEMA = repo_path("schemas/core/original-reference-replay-scenario-receipt.schema.json")
DEFAULT_OBSERVER_PATH = repo_path("tools/bizhawk/original_reference_scenario_observer.lua")
DEFAULT_STATIC_FIXTURE_CATALOG = {
    "sf2-map3-battle01-turn-finalization-static-v1": repo_path(
        "tests/fixtures/h2/map3-battle01-turn-finalization-static-v1.json"
    ),
    "sf2-map3-battle01-victory-return-static-v1": repo_path(
        "tests/fixtures/h2/map3-battle01-victory-return-static-v1.json"
    ),
}
REQUIRED_ARTIFACT_ROLES = frozenset({"movie", "input-log", "header", "sync-settings"})
PATH_LIKE_VALUE = re.compile(r"(?:[A-Za-z]:[\\/]|\\\\|file://|(?:^|\s)/\S*)", re.IGNORECASE)
ALLOWED_LUA_API_NAMES = (
    "os.getenv",
    "io.open",
    "event.onmemoryexecute",
    "event.onexit",
    "event.unregisterbyid",
    "client.exitCode",
    "string.byte",
    "string.char",
    "string.gsub",
    "string.format",
    "string.sub",
    "table.concat",
)
ALLOWED_BARE_CALLS = ("ipairs", "pcall", "tostring")
FORBIDDEN_CAPABILITIES = (
    "adaptive-input",
    "controller-write",
    "dynamic-call",
    "dynamic-load",
    "gameplay-mechanics",
    "memory-write",
    "movie-mutation",
    "register-write",
    "rom-control",
    "savestate",
    "shell-process",
    "sram-write",
    "state-write",
)
FORBIDDEN_LUA_SURFACES = (
    "joypad.",
    "input.",
    "memory.",
    "mainmemory.",
    "register.",
    "movie.",
    "savestate.",
    "sram.",
    "client.openrom",
    "client.closerom",
    "client.reboot",
    "os.execute",
    "io.popen",
    "require",
    "dofile",
    "loadfile",
    "load(",
    "package.",
    "debug.",
    "_G",
    "_ENV",
    "rawget",
    "setmetatable",
    "getmetatable",
)


class ScenarioError(ValueError):
    """A closed scenario descriptor or public preflight error."""


class DescriptorContractError(ScenarioError):
    """A generic descriptor violates a cross-field protocol invariant."""


class StaticFixtureIdentityError(ScenarioError):
    """An injected static-fixture catalog does not match a descriptor identity."""


class PassiveObserverPolicyError(ScenarioError):
    """The injected observer does not satisfy its declared passive policy."""


def synthetic_artifact_sha256(artifact_id: str) -> str:
    """Return a public synthetic identity without materializing an artifact payload."""

    return sha256(f"{SYNTHETIC_ARTIFACT_DOMAIN}{artifact_id}".encode())


def _public_detail(value: str) -> str:
    if PATH_LIKE_VALUE.search(value):
        return "path-redacted"
    return value[:500]


def _failure(code: str, expected: str, actual: str) -> dict[str, Any]:
    receipt = {
        "schemaVersion": 1,
        "scenarioApiId": SCENARIO_API_ID,
        "mode": "PREFLIGHT",
        "status": "FAIL",
        "ProcessStarts": 0,
        "descriptor": {"status": "unavailable", "identity": None},
        "scenario": None,
        "transport": None,
        "checkpoints": [],
        "terminalObservation": None,
        "candidateLineage": None,
        "unknowns": [],
        "observerStatus": None,
        "failure": {
            "phase": "preflight",
            "code": code,
            "expected": _public_detail(expected),
            "actual": _public_detail(actual),
        },
    }
    validate_json(receipt, RECEIPT_SCHEMA, owner="original-reference scenario preflight receipt")
    return receipt


def _validate_no_path_or_payload_values(value: Any, *, field: str = "descriptor") -> None:
    if isinstance(value, dict):
        for key, child in value.items():
            if key in {"path", "payload", "raw", "bytes", "content"}:
                raise DescriptorContractError(f"{field} declares a forbidden raw/path key: {key}")
            _validate_no_path_or_payload_values(child, field=f"{field}.{key}")
    elif isinstance(value, list):
        for index, child in enumerate(value):
            _validate_no_path_or_payload_values(child, field=f"{field}[{index}]")
    elif isinstance(value, str) and PATH_LIKE_VALUE.search(value):
        raise DescriptorContractError(f"{field} contains a path-like value")


def _require_unique(values: list[str], *, label: str) -> None:
    if len(values) != len(set(values)):
        raise DescriptorContractError(f"{label} must be unique")


def _validate_descriptor_contract(descriptor: dict[str, Any]) -> None:
    _validate_no_path_or_payload_values(descriptor)
    fixtures = descriptor["staticFixtures"]
    checkpoints = descriptor["checkpoints"]
    artifacts = descriptor["inputArtifacts"]
    _require_unique([fixture["fixtureId"] for fixture in fixtures], label="static fixture IDs")
    _require_unique([artifact["artifactId"] for artifact in artifacts], label="artifact IDs")
    artifact_roles = [artifact["role"] for artifact in artifacts]
    _require_unique(artifact_roles, label="artifact roles")
    if set(artifact_roles) != REQUIRED_ARTIFACT_ROLES:
        raise DescriptorContractError("input artifacts must declare all four required roles")
    roles = [checkpoint["role"] for checkpoint in checkpoints]
    _require_unique(roles, label="checkpoint role names")
    if descriptor["terminalObservation"]["roleOrder"] != roles:
        raise DescriptorContractError("terminal roleOrder must exactly equal checkpoint order")
    fixture_ids = {fixture["fixtureId"] for fixture in fixtures}
    for checkpoint in checkpoints:
        if checkpoint["staticFixtureId"] not in fixture_ids:
            raise DescriptorContractError("checkpoint staticFixtureId is not declared")
    if len(checkpoints) > descriptor["limits"]["maxCheckpoints"]:
        raise DescriptorContractError("checkpoint count exceeds descriptor limit")
    if descriptor["timeoutSeconds"] > descriptor["limits"]["maxTimeoutSeconds"]:
        raise DescriptorContractError("timeout exceeds descriptor limit")
    previous_address: str | None = None
    closed_addresses: set[str] = set()
    for checkpoint in checkpoints:
        address = checkpoint["address"]
        if address != previous_address:
            if address in closed_addresses:
                raise DescriptorContractError("same-address checkpoints must be contiguous")
            if previous_address is not None:
                closed_addresses.add(previous_address)
            previous_address = address


def _validate_static_fixture_identities(
    descriptor: dict[str, Any], *, static_fixture_catalog: Mapping[str, Path]
) -> None:
    for static_fixture in descriptor["staticFixtures"]:
        fixture_id = static_fixture["fixtureId"]
        try:
            path = static_fixture_catalog[fixture_id]
        except KeyError as error:
            raise StaticFixtureIdentityError(
                f"injected static-fixture catalog lacks: {fixture_id}"
            ) from error
        actual = sha256(path.read_bytes())
        if actual != static_fixture["sha256"]:
            raise StaticFixtureIdentityError(
                f"static fixture hash drift: {fixture_id}: expected "
                f"{static_fixture['sha256']}, got {actual}"
            )


def _validate_observer_policy(descriptor: dict[str, Any], *, observer_path: Path) -> str:
    policy = descriptor["passiveObserverPolicy"]
    if tuple(policy["allowedApis"]) != ALLOWED_LUA_API_NAMES:
        raise PassiveObserverPolicyError(
            "passive observer allowedApis must equal the immutable policy"
        )
    if tuple(policy["allowedBareCalls"]) != ALLOWED_BARE_CALLS:
        raise PassiveObserverPolicyError(
            "passive observer allowedBareCalls must equal the immutable policy"
        )
    if tuple(policy["forbiddenCapabilities"]) != FORBIDDEN_CAPABILITIES:
        raise PassiveObserverPolicyError(
            "passive observer forbiddenCapabilities must equal the immutable policy"
        )
    try:
        return validate_passive_lua_source(
            path=observer_path,
            expected_sha256=policy["observerSha256"],
            allowed_api_names=ALLOWED_LUA_API_NAMES,
            allowed_bare_calls=ALLOWED_BARE_CALLS,
            forbidden_patterns=FORBIDDEN_LUA_SURFACES,
        )
    except TransportError as error:
        raise PassiveObserverPolicyError(str(error)) from error


def load_scenario_descriptor(
    path: Path | None = None,
    *,
    static_fixture_catalog: Mapping[str, Path] | None = None,
    observer_path: Path | None = None,
) -> dict[str, Any]:
    """Load a closed generic descriptor through injected outer composition inputs."""

    resolved = (FIXTURE_PATH if path is None else path).resolve(strict=True)
    catalog = (
        DEFAULT_STATIC_FIXTURE_CATALOG if static_fixture_catalog is None else static_fixture_catalog
    )
    resolved_observer = DEFAULT_OBSERVER_PATH if observer_path is None else observer_path
    descriptor = load_json(resolved)
    validate_json(
        descriptor,
        SCENARIO_SCHEMA,
        owner="original-reference replay scenario API descriptor",
    )
    if descriptor["scenarioApiId"] != SCENARIO_API_ID:
        raise ScenarioError("scenario API ID drift")
    _validate_descriptor_contract(descriptor)
    _validate_static_fixture_identities(descriptor, static_fixture_catalog=catalog)
    _validate_observer_policy(descriptor, observer_path=resolved_observer)
    return descriptor


def preflight_original_reference_scenario(
    fixture_path: Path | None = None,
    *,
    static_fixture_catalog: Mapping[str, Path] | None = None,
    observer_path: Path | None = None,
) -> dict[str, Any]:
    """Validate the data-only descriptor without creating a ledger or process."""

    resolved = (FIXTURE_PATH if fixture_path is None else fixture_path).resolve()
    try:
        descriptor = load_scenario_descriptor(
            resolved,
            static_fixture_catalog=static_fixture_catalog,
            observer_path=observer_path,
        )
    except (FileNotFoundError, json.JSONDecodeError, ValueError) as error:
        if isinstance(error, StaticFixtureIdentityError):
            code = "static-fixture-identity"
        elif isinstance(error, DescriptorContractError):
            code = "descriptor-contract"
        elif isinstance(error, PassiveObserverPolicyError):
            code = "passive-observer-policy"
        else:
            code = "descriptor-schema"
        return _failure(code, "closed public scenario descriptor", str(error))
    observer_sha256 = descriptor["passiveObserverPolicy"]["observerSha256"]
    receipt = {
        "schemaVersion": 1,
        "scenarioApiId": SCENARIO_API_ID,
        "mode": "PREFLIGHT",
        "status": "PASS",
        "ProcessStarts": 0,
        "descriptor": {"status": "validated", "identity": file_identity(resolved)},
        "scenario": {
            "scenarioId": descriptor["scenarioId"],
            "caseId": descriptor["caseId"],
            "classification": descriptor["classification"],
            "staticFixtures": descriptor["staticFixtures"],
        },
        "transport": {
            "startState": descriptor["startState"],
            "inputArtifacts": descriptor["inputArtifacts"],
            "observer": {
                "observerId": descriptor["passiveObserverPolicy"]["observerId"],
                "sha256": observer_sha256,
            },
        },
        "checkpoints": descriptor["checkpoints"],
        "terminalObservation": descriptor["terminalObservation"],
        "candidateLineage": {
            "ledgerId": descriptor["candidateLineage"]["ledgerId"],
            "availability": "not-accessed-preflight",
            "runClass": None,
            "launchOrdinal": None,
            "priorReceiptSha256": None,
        },
        "unknowns": descriptor["unknowns"],
        "observerStatus": None,
        "failure": None,
    }
    validate_json(receipt, RECEIPT_SCHEMA, owner="original-reference scenario preflight receipt")
    return receipt


def run_original_reference_scenario(*, preflight_only: bool) -> dict[str, Any]:
    """Expose only the approved public preflight; no runtime launch is implemented."""

    if not preflight_only:
        raise ScenarioError("original-reference scenario API requires --preflight-only")
    return preflight_original_reference_scenario()
