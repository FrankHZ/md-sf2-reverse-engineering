from __future__ import annotations

import os
import sys
from pathlib import Path

import pytest

from sf2tool.dotnet_environment import (
    prevent_dotnet_path_changes,
    shared_dotnet_environment,
)


@pytest.mark.parametrize("inherited", [None, "true", "TRUE", "false"])
def test_path_opt_out_overrides_inheritance_without_mutating_input(inherited: str | None) -> None:
    original = {"KEEP": "caller-owned"}
    if inherited is not None:
        original["DOTNET_ADD_GLOBAL_TOOLS_TO_PATH"] = inherited
    before = original.copy()
    result = prevent_dotnet_path_changes(original)
    assert result == {**original, "DOTNET_ADD_GLOBAL_TOOLS_TO_PATH": "false"}
    assert original == before and result is not original


def test_worktrees_share_only_explicit_cli_and_keep_default_caches_separate(tmp_path: Path) -> None:
    original = {
        "DOTNET_BIN": sys.executable,
        "DOTNET_CLI_HOME": str(tmp_path / "shared-cli"),
        "DOTNET_ADD_GLOBAL_TOOLS_TO_PATH": "true",
        "PATH": "caller-path",
    }
    before = original.copy()
    first = shared_dotnet_environment(original, tmp_path / "worktree-one")
    second = shared_dotnet_environment(original, tmp_path / "worktree-two")
    assert first["DOTNET_CLI_HOME"] == second["DOTNET_CLI_HOME"] == original["DOTNET_CLI_HOME"]
    assert first["DOTNET_BIN"] == second["DOTNET_BIN"] == str(Path(sys.executable).resolve())
    assert first["PATH"].split(os.pathsep)[0] == str(Path(sys.executable).resolve().parent)
    for key in ("NUGET_PACKAGES", "NUGET_HTTP_CACHE_PATH"):
        assert first[key] != second[key]
        assert Path(first[key]).is_relative_to(tmp_path / "worktree-one" / "local")
        assert Path(second[key]).is_relative_to(tmp_path / "worktree-two" / "local")
    assert original == before
    assert not (tmp_path / "shared-cli").exists()  # Only the SDK creates its own state.


def test_explicit_worktree_caches_and_temp_are_preserved(tmp_path: Path) -> None:
    original = {"DOTNET_BIN": sys.executable, "DOTNET_CLI_HOME": str(tmp_path / "shared-cli")}
    local = {key: str(tmp_path / "worktree" / key) for key in (
        "NUGET_PACKAGES", "NUGET_HTTP_CACHE_PATH", "UV_PROJECT_ENVIRONMENT",
        "UV_CACHE_DIR", "TEMP", "TMP"
    )}
    original.update(local)
    result = shared_dotnet_environment(original, tmp_path / "worktree")
    assert {key: result[key] for key in local} == local


def test_windows_environment_aliases_cannot_reintroduce_an_opt_in(tmp_path: Path) -> None:
    original = {"DOTNET_BIN": sys.executable, "DOTNET_CLI_HOME": str(tmp_path / "shared-cli"),
        "dotnet_add_global_tools_to_path": "true", "Path": "caller-path"}
    before = original.copy()
    result = shared_dotnet_environment(original, tmp_path)
    assert result["DOTNET_ADD_GLOBAL_TOOLS_TO_PATH"] == "false"
    assert "dotnet_add_global_tools_to_path" not in result
    if os.name == "nt":
        assert "Path" not in result
        assert result["PATH"].endswith(os.pathsep + "caller-path")
    assert original == before


@pytest.mark.parametrize("key", ["DOTNET_BIN", "DOTNET_CLI_HOME"])
@pytest.mark.parametrize("value", [None, "", "relative/path"])
def test_unconfigured_or_relative_cli_selection_fails_explicitly(
    tmp_path: Path, key: str, value: str | None
) -> None:
    original = {"DOTNET_BIN": sys.executable, "DOTNET_CLI_HOME": str(tmp_path / "shared-cli")}
    if value is None:
        original.pop(key)
    else:
        original[key] = value
    with pytest.raises(ValueError, match=key):
        shared_dotnet_environment(original, tmp_path)


def test_missing_binary_and_file_as_cli_home_are_rejected(tmp_path: Path) -> None:
    with pytest.raises(ValueError, match="existing shared dotnet"):
        shared_dotnet_environment(
            {"DOTNET_BIN": str(tmp_path / "absent"), "DOTNET_CLI_HOME": str(tmp_path)}, tmp_path
        )
    with pytest.raises(ValueError, match="directory"):
        shared_dotnet_environment(
            {"DOTNET_BIN": sys.executable, "DOTNET_CLI_HOME": sys.executable}, tmp_path
        )
