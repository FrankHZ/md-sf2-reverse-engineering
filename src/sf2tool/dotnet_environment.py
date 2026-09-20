"""Explicit shared .NET CLI selection without first-run PATH mutation."""

import os
from collections.abc import Mapping
from pathlib import Path


def prevent_dotnet_path_changes(environment: Mapping[str, str]) -> dict[str, str]:
    """Protect every child launch, including callers with an inherited opt-in."""
    result = {
        name: value
        for name, value in environment.items()
        if name.upper() != "DOTNET_ADD_GLOBAL_TOOLS_TO_PATH"
    }
    result["DOTNET_ADD_GLOBAL_TOOLS_TO_PATH"] = "false"
    return result


def shared_dotnet_environment(environment: Mapping[str, str], worktree: Path) -> dict[str, str]:
    """Require host-wide CLI configuration; leave other writable state local."""
    result = prevent_dotnet_path_changes(environment)
    for name in ("DOTNET_BIN", "DOTNET_CLI_HOME"):
        value = result.get(name)
        if not value or not Path(value).is_absolute():
            raise ValueError(f"{name} must explicitly select an absolute shared CLI path")
        result[name] = str(Path(value).resolve())
    binary = Path(result["DOTNET_BIN"])
    if not binary.is_file():
        raise ValueError("DOTNET_BIN must select an existing shared dotnet executable")
    if Path(result["DOTNET_CLI_HOME"]).is_file():
        raise ValueError("DOTNET_CLI_HOME must select a directory")
    # Godot's own build subprocesses must find the same installation. This changes
    # only the child environment, never the user or machine PATH.
    path_value = result.get("PATH")
    if os.name == "nt":
        for name in tuple(result):
            if name.upper() == "PATH":
                path_value = result.pop(name)
    result["PATH"] = os.pathsep.join(filter(None, (str(binary.parent), path_value)))
    for name, directory in (
        ("NUGET_PACKAGES", "nuget-packages"),
        ("NUGET_HTTP_CACHE_PATH", "nuget-http-cache"),
        ("NUGET_SCRATCH", "nuget-scratch"),
        ("NUGET_PLUGINS_CACHE_PATH", "nuget-plugins-cache"),
        ("TEMP", "tmp/dotnet"),
        ("TMP", "tmp/dotnet"),
    ):
        local = worktree.resolve() / "local"
        value = result.get(name)
        selected = Path(value) if value else local / directory
        # TEMP/TMP normally inherit the host OS default; replace that default.
        # Explicit cache selections must not silently write to another worktree.
        if name in {"TEMP", "TMP"} and not selected.resolve().is_relative_to(local):
            selected = local / directory
        if not selected.is_absolute() or not selected.resolve().is_relative_to(local):
            raise ValueError(f"{name} must select a directory beneath this worktree's local/")
        selected = selected.resolve()
        selected.mkdir(parents=True, exist_ok=True)
        result[name] = str(selected)
    return result
