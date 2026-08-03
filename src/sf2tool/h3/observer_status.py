"""Shared structured callback-failure and terminal-status contracts for H3 observers."""

from __future__ import annotations

import json
from pathlib import Path
from typing import Any

from sf2tool.jsonio import validate_json

CALLBACK_FAILURE_PREFIX = "failure:observer-callback:"
SUCCESS_STATUS_TAIL = (
    "milestone:callbacks-cleared:0",
    "milestone:observer-finished",
)


def observer_failure_contract(owner: str) -> dict[str, object]:
    """Return the fixed Lua failure/cleanup protocol for one observer owner."""
    if not owner:
        raise ValueError("observer failure owner must be non-empty")
    return {
        "owner": owner,
        "exitCode": 1,
        "removeOutputBeforeExit": True,
        "statusPrefix": CALLBACK_FAILURE_PREFIX,
    }


def callback_failure_status(
    status_path: Path,
    *,
    owner: str,
    schema_path: Path,
) -> dict[str, Any] | None:
    """Read and validate exactly one append-style callback failure, if present."""
    if not status_path.is_file():
        return None
    failure_rows: list[str] = []
    for line in status_path.read_text(encoding="utf-8").splitlines():
        if CALLBACK_FAILURE_PREFIX not in line:
            continue
        if not line.startswith(CALLBACK_FAILURE_PREFIX):
            raise ValueError(f"{owner} callback failure status line drift")
        failure_rows.append(line.removeprefix(CALLBACK_FAILURE_PREFIX))
    if not failure_rows:
        return None
    if len(failure_rows) != 1:
        raise ValueError(f"{owner} callback failure status multiplicity drift")
    try:
        payload = json.loads(failure_rows[0])
    except json.JSONDecodeError as error:
        raise ValueError(f"{owner} callback failure status JSON drift") from error
    validate_json(payload, schema_path, owner=f"{owner} callback failure status")
    return payload


def assert_observer_status(
    status_path: Path,
    *,
    owner: str,
    schema_path: Path,
    required_milestones: tuple[str, ...] = (),
) -> None:
    """Require owner milestones followed by exact callback cleanup and completion."""
    if not status_path.is_file():
        raise RuntimeError(f"{owner} observer wrote no status record")
    failure = callback_failure_status(status_path, owner=owner, schema_path=schema_path)
    if failure is not None:
        raise RuntimeError(
            f"{owner} observer callback failure: {json.dumps(failure, sort_keys=True)}"
        )
    lines = status_path.read_text(encoding="utf-8").splitlines()
    missing = [milestone for milestone in required_milestones if milestone not in lines]
    if missing:
        raise RuntimeError(f"{owner} observer required milestone drift: {missing}")
    if tuple(lines[-len(SUCCESS_STATUS_TAIL) :]) != SUCCESS_STATUS_TAIL:
        raise RuntimeError(f"{owner} observer terminal status drift")
