from __future__ import annotations

import os
from collections.abc import Mapping
from pathlib import Path, PureWindowsPath

from sf2tool.paths import repo_path

SHARED_INPUT_ROOT_ENV = "SF2_SHARED_INPUT_ROOT"
ROM_INPUT_IDENTITY = Path("roms/sf2-us.bin")

_REPO_LOCAL_FALLBACKS = {
    ROM_INPUT_IDENTITY: Path("local/roms/sf2-us.bin"),
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
    if identity not in _REPO_LOCAL_FALLBACKS:
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
    values = os.environ if environment is None else environment
    configured = values.get(SHARED_INPUT_ROOT_ENV)
    if configured is None:
        return repo_path(_REPO_LOCAL_FALLBACKS[normalized])
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
    return resolved_candidate
