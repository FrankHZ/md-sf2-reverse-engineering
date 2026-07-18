from __future__ import annotations

import shutil
import subprocess
from collections.abc import Sequence
from pathlib import Path

from sf2tool.paths import repo_path


class LegacyPowerShellUnavailable(RuntimeError):
    pass


def run_powershell(script: str, arguments: Sequence[str | Path] = ()) -> None:
    """Run a frozen PowerShell rail until that rail is migrated to Python.

    New project logic must not be added here or to the legacy scripts. This adapter exists to keep
    the already-proven H1-H3 evidence executable while migration proceeds subsystem by subsystem.
    """
    executable = shutil.which("pwsh") or shutil.which("pwsh.exe")
    if not executable:
        raise LegacyPowerShellUnavailable(
            f"legacy rail {script} still requires PowerShell 7 during migration"
        )
    script_path = repo_path(f"scripts/{script}").resolve(strict=True)
    command = [
        executable,
        "-NoLogo",
        "-NoProfile",
        "-NonInteractive",
        "-File",
        str(script_path),
        *(str(argument) for argument in arguments),
    ]
    subprocess.run(command, cwd=repo_path("."), check=True)
