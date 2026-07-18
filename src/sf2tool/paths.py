from __future__ import annotations

from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parents[2]


def repo_path(relative: str | Path) -> Path:
    """Resolve a repository-relative path without allowing it to escape the root."""
    relative = Path(relative)
    if relative.is_absolute():
        raise ValueError(f"expected a repository-relative path, got: {relative}")
    resolved = (REPO_ROOT / relative).resolve()
    if not resolved.is_relative_to(REPO_ROOT):
        raise ValueError(f"path escapes the repository: {relative}")
    return resolved


def display_path(path: Path) -> str:
    try:
        return path.resolve().relative_to(REPO_ROOT).as_posix()
    except ValueError:
        return str(path.resolve())
