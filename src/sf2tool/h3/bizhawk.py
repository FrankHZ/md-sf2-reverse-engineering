from __future__ import annotations

import os
import subprocess
from pathlib import Path
from typing import Any

from sf2tool.jsonio import load_json
from sf2tool.paths import repo_path
from sf2tool.rom import inspect_rom

TOOLCHAIN_MANIFEST = repo_path("manifests/toolchain.json")
DERIVED_ROOT = repo_path("local/derived/h3")


def _lua_literal(value: Any) -> str:
    if value is None:
        return "nil"
    if isinstance(value, bool):
        return "true" if value else "false"
    if isinstance(value, (int, float)):
        return str(value)
    if isinstance(value, str):
        escaped = value.replace("\\", "\\\\").replace('"', '\\"').replace("\n", "\\n")
        return f'"{escaped}"'
    if isinstance(value, list):
        return "{ " + ", ".join(_lua_literal(item) for item in value) + " }"
    if isinstance(value, dict):
        fields = ", ".join(
            f"[{_lua_literal(str(key))}] = {_lua_literal(item)}"
            for key, item in value.items()
        )
        return "{ " + fields + " }"
    raise TypeError(f"cannot serialize {type(value).__name__} as a Lua literal")


def bizhawk_contract(manifest_path: Path = TOOLCHAIN_MANIFEST) -> tuple[dict[str, Any], Path]:
    manifest = load_json(manifest_path.resolve(strict=True))
    contract = manifest["bizhawk"]
    executable = repo_path(contract["localExecutablePath"]).resolve(strict=True)
    return contract, executable


def verify_runtime_contract(fixture: dict[str, Any], rom_path: Path) -> None:
    actual_hash = inspect_rom(rom_path.resolve(strict=True))["sha256"]
    if actual_hash != fixture["romSha256"]:
        raise ValueError(
            f"H3 fixture ROM mismatch: expected {fixture['romSha256']}, got {actual_hash}"
        )
    bizhawk, _ = bizhawk_contract()
    emulator = fixture["emulator"]
    if (
        emulator["name"] != "BizHawk"
        or emulator["version"] != bizhawk["release"]
        or emulator["core"] != bizhawk["core"]
    ):
        raise ValueError("H3 fixture execution-engine contract mismatch")


def run_observer(
    *,
    rom_path: Path,
    observer_path: Path,
    config: dict[str, Any],
    output_name: str,
    timeout_seconds: int,
) -> dict[str, Any]:
    """Run a tracked Lua observer with JSON configuration and return its JSON facts."""
    rom_path = rom_path.resolve(strict=True)
    observer_path = observer_path.resolve(strict=True)
    _, executable = bizhawk_contract()
    DERIVED_ROOT.mkdir(parents=True, exist_ok=True)
    config_path = DERIVED_ROOT / f"{output_name}.config.lua"
    output_path = DERIVED_ROOT / f"{output_name}.observed.json"
    status_path = DERIVED_ROOT / f"{output_name}.status.txt"
    for path in (output_path, status_path):
        path.unlink(missing_ok=True)
    runtime_config = {
        **config,
        "outputPath": output_path.as_posix(),
        "statusPath": status_path.as_posix(),
    }
    config_path.write_text("return " + _lua_literal(runtime_config) + "\n", encoding="utf-8")
    environment = os.environ.copy()
    environment["SF2_H3_CONFIG"] = str(config_path)
    process = subprocess.Popen(
        [str(executable), f"--lua={observer_path}", str(rom_path)],
        cwd=executable.parent,
        env=environment,
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
        text=True,
        encoding="utf-8",
        errors="replace",
    )
    try:
        stdout, stderr = process.communicate(timeout=timeout_seconds)
    except subprocess.TimeoutExpired as error:
        if os.name == "nt":
            subprocess.run(
                ["taskkill.exe", "/PID", str(process.pid), "/T", "/F"],
                check=False,
                capture_output=True,
            )
        else:
            process.kill()
        stdout, stderr = process.communicate()
        status = (
            status_path.read_text(encoding="utf-8").strip()
            if status_path.exists()
            else "no status"
        )
        diagnostic = (stdout + "\n" + stderr).strip()[-4000:]
        raise RuntimeError(
            f"BizHawk observation timed out after {timeout_seconds}s ({status}).\n{diagnostic}"
        ) from error
    if process.returncode != 0:
        raise RuntimeError(
            f"BizHawk observation failed with exit code {process.returncode}.\n"
            f"STDOUT:\n{stdout}\nSTDERR:\n{stderr}"
        )
    if not output_path.is_file():
        raise RuntimeError(
            "BizHawk observer exited without writing its observation file.\n"
            f"STDOUT:\n{stdout[-4000:]}\nSTDERR:\n{stderr[-4000:]}"
        )
    return load_json(output_path)
