from __future__ import annotations

import os
from collections.abc import Mapping
from dataclasses import dataclass
from pathlib import Path, PureWindowsPath
from typing import Literal

from sf2tool.paths import repo_path

SHARED_INPUT_ROOT_ENV = "SF2_SHARED_INPUT_ROOT"
ROM_INPUT_IDENTITY = Path("roms/sf2-us.bin")
JDK_INPUT_IDENTITY = Path("toolchains/jdk-17.0.19+10")
BIZHAWK_ARCHIVE_INPUT_IDENTITY = Path("archives/BizHawk-2.11.1-win-x64.zip")


@dataclass(frozen=True)
class _PrivateInputRegistration:
    fallback: Path
    expected_kind: Literal["file", "directory"]


_PRIVATE_INPUTS = {
    ROM_INPUT_IDENTITY: _PrivateInputRegistration(
        fallback=Path("local/roms/sf2-us.bin"),
        expected_kind="file",
    ),
    JDK_INPUT_IDENTITY: _PrivateInputRegistration(
        fallback=Path("local/toolchains/jdk-17.0.19+10"),
        expected_kind="directory",
    ),
    BIZHAWK_ARCHIVE_INPUT_IDENTITY: _PrivateInputRegistration(
        fallback=Path("local/toolchains/BizHawk-2.11.1-win-x64.zip"),
        expected_kind="file",
    ),
}


def _input_identity(value: str | Path) -> Path:
    raw = str(value)
    if not raw or not raw.strip():
        raise ValueError("private input identity must not be empty")

    windows = PureWindowsPath(raw)
    if windows.is_absolute() or windows.drive or windows.root:
        raise ValueError(f"private input identity must be relative: {raw}")

    segments = raw.replace("\\", "/").split("/")
    if any(segment in {"", ".", ".."} for segment in segments):
        raise ValueError(f"private input identity contains an unsafe segment: {raw}")

    identity = Path(*segments)
    if identity not in _PRIVATE_INPUTS:
        raise ValueError(f"private input identity is not registered: {raw}")
    return identity


def _resolve_existing(path: Path) -> Path:
    return path.resolve(strict=True)


def private_input_path(
    identity: str | Path,
    *,
    environment: Mapping[str, str] | None = None,
) -> Path:
    """Resolve one registered immutable input without creating or reading it."""

    normalized = _input_identity(identity)
    registration = _PRIVATE_INPUTS[normalized]
    values = os.environ if environment is None else environment
    configured = values.get(SHARED_INPUT_ROOT_ENV)
    if configured is None:
        return repo_path(registration.fallback)
    if not configured.strip():
        raise ValueError(f"{SHARED_INPUT_ROOT_ENV} must not be empty")

    configured_root = Path(configured)
    if not configured_root.is_absolute():
        raise ValueError(f"{SHARED_INPUT_ROOT_ENV} must be an absolute path")

    resolved_root = _resolve_existing(configured_root)
    if not resolved_root.is_dir():
        raise NotADirectoryError(f"{SHARED_INPUT_ROOT_ENV} is not a directory")

    lexical_candidate = configured_root.joinpath(*normalized.parts)
    resolved_candidate = _resolve_existing(lexical_candidate)
    if not resolved_candidate.is_relative_to(resolved_root):
        raise ValueError("private input resolves outside SF2_SHARED_INPUT_ROOT")
    if registration.expected_kind == "file" and not resolved_candidate.is_file():
        raise ValueError(f"registered private input must be a file: {normalized.as_posix()}")
    if registration.expected_kind == "directory" and not resolved_candidate.is_dir():
        raise ValueError(
            f"registered private input must be a directory: {normalized.as_posix()}"
        )
    return resolved_candidate
