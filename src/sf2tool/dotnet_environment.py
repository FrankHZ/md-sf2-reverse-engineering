"""Explicit shared .NET CLI selection without first-run PATH mutation."""

import os
from collections.abc import Mapping
from pathlib import Path


def prevent_dotnet_path_changes(environment: Mapping[str, str]) -> dict[str, str]:
    """Protect every child launch, including callers with an inherited opt-in."""
    result = {
        name: value for name, value in environment.items()
        if name.upper() != "DOTNET_ADD_GLOBAL_TOOLS_TO_PATH"
    }
    result["DOTNET_ADD_GLOBAL_TOOLS_TO_PATH"] = "false"
    return result


def shared_dotnet_environment(
    environment: Mapping[str, str], worktree: Path
) -> dict[str, str]:
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
    ):
        if not result.get(name):
            result[name] = str(worktree.resolve() / "local" / directory)
    return result
