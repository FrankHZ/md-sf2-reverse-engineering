from __future__ import annotations

import ctypes
import json
import os
import shutil
import subprocess
import tempfile
from collections.abc import Callable
from dataclasses import dataclass
from pathlib import Path
from typing import Any

from sf2tool.h3.bootstrap import BOOTSTRAP_LIBRARY, runtime_bootstrap
from sf2tool.jsonio import load_json
from sf2tool.paths import repo_path
from sf2tool.rom import inspect_rom
from sf2tool.toolchain import shared_bizhawk, verify_bizhawk_installation

TOOLCHAIN_MANIFEST = repo_path("manifests/toolchain.json")
DERIVED_ROOT = repo_path("local/derived/h3")


@dataclass(frozen=True)
class NativeProcessResult:
    """Bounded result from one native BizHawk launch without interpretation."""

    returncode: int | None
    stdout: str
    stderr: str
    timed_out: bool
    started: bool = True
    process_terminated: bool = True
    timeout_tree_killed: bool = False
    pid: int | None = None
    error: str | None = None


def _terminate_process_tree(process: subprocess.Popen[str]) -> bool:
    """Terminate the bounded native launch even when Windows taskkill is unavailable."""

    tree_killed = False
    if os.name == "nt":
        try:
            kill = subprocess.run(
                ["taskkill.exe", "/PID", str(process.pid), "/T", "/F"],
                check=False,
                capture_output=True,
            )
            tree_killed = kill.returncode == 0
        except OSError:
            tree_killed = False
    if process.poll() is None:
        process.kill()
    return tree_killed


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
            f"[{_lua_literal(str(key))}] = {_lua_literal(item)}" for key, item in value.items()
        )
        return "{ " + fields + " }"
    raise TypeError(f"cannot serialize {type(value).__name__} as a Lua literal")


def bizhawk_contract(manifest_path: Path = TOOLCHAIN_MANIFEST) -> tuple[dict[str, Any], Path]:
    manifest = load_json(manifest_path.resolve(strict=True))
    contract = manifest["bizhawk"]
    executable = shared_bizhawk(contract)
    return contract, executable


def materialize_bizhawk_launch(
    output: Path,
    *,
    config: dict[str, Any] | None = None,
) -> dict[str, Any]:
    """Copy the verified release into local state for Windows AppContext writes.

    Windows ignores BIZHAWK_DATA_HOME. This is preparation, not a runtime observation.
    The installation is shared; each launch owns its writable runtime copy.
    """
    output = output.resolve()
    local = repo_path("local")
    if not output.is_relative_to(local) or output == local:
        raise ValueError("BizHawk launch state must be beneath this worktree's local/")
    contract, _ = bizhawk_contract()
    installation, members = verify_bizhawk_installation(contract)
    output.mkdir(parents=True, exist_ok=True)
    state = Path(tempfile.mkdtemp(prefix="bizhawk-", dir=output))
    # Copy release members only, never a previous launch's configuration or saves.
    for relative in members:
        source = installation / relative
        target = state / relative
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(source, target)
    settings = {
        "LastWrittenFrom": contract["release"],
        "PreferredCores": {"GEN": "Genplus-gx"},
        "FirstBoot": False,
        "SingleInstanceMode": False,
        "SoundEnabled": False,
        "UpdateAutoCheckEnabled": False,
        "RACheevosActive": False,
        "AutoLoadLastSaveSlot": False,
        "AutoSaveLastSaveSlot": False,
        "AutosaveSaveRAM": False,
        "BackupSaveram": False,
        **(config or {}),
    }
    config_path = state / "config.ini"
    config_path.write_text(json.dumps(settings, indent=2) + "\n", encoding="utf-8")
    (state / "Temp").mkdir(exist_ok=True)
    environment = {
        "TEMP": str(state / "Temp"),
        "TMP": str(state / "Temp"),
    }
    return {
        "executable": str(state / "EmuHawk.exe"),
        "config": str(config_path),
        "cwd": str(state),
        "environment": environment,
    }


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


def validate_lua_syntax(script_path: Path, executable: Path) -> None:
    """Compile a Lua chunk with BizHawk's bundled runtime without executing it."""
    script_path = script_path.resolve(strict=True)
    lua_library_path = executable.resolve(strict=True).parent / "dll" / "lua54.dll"
    if not lua_library_path.is_file():
        raise FileNotFoundError(f"BizHawk Lua runtime is missing: {lua_library_path}")

    library = ctypes.CDLL(str(lua_library_path))
    library.luaL_newstate.argtypes = []
    library.luaL_newstate.restype = ctypes.c_void_p
    library.luaL_loadbufferx.argtypes = [
        ctypes.c_void_p,
        ctypes.c_char_p,
        ctypes.c_size_t,
        ctypes.c_char_p,
        ctypes.c_char_p,
    ]
    library.luaL_loadbufferx.restype = ctypes.c_int
    library.lua_tolstring.argtypes = [
        ctypes.c_void_p,
        ctypes.c_int,
        ctypes.POINTER(ctypes.c_size_t),
    ]
    library.lua_tolstring.restype = ctypes.c_void_p
    library.lua_close.argtypes = [ctypes.c_void_p]
    library.lua_close.restype = None

    state = library.luaL_newstate()
    if not state:
        raise RuntimeError("BizHawk Lua syntax preflight could not create a Lua state")
    try:
        source = script_path.read_bytes()
        chunk_name = f"@{script_path.as_posix()}".encode()
        status = library.luaL_loadbufferx(state, source, len(source), chunk_name, b"t")
        if status != 0:
            message_length = ctypes.c_size_t()
            message_pointer = library.lua_tolstring(state, -1, ctypes.byref(message_length))
            message = (
                ctypes.string_at(message_pointer, message_length.value).decode(
                    "utf-8", errors="replace"
                )
                if message_pointer
                else f"Lua parser returned status {status}"
            )
            raise ValueError(f"Lua syntax preflight failed: {message}")
    finally:
        library.lua_close(state)


def run_native_bizhawk_process(
    *,
    command: list[str],
    executable: Path,
    environment: dict[str, str],
    timeout_seconds: int,
    on_started: Callable[[int], None] | None = None,
    on_timeout: Callable[[], None] | None = None,
) -> NativeProcessResult:
    """Start a native BizHawk process with no shell and bound its diagnostics."""
    if not executable.resolve().is_relative_to(repo_path("local")):
        raise ValueError("native BizHawk launch requires a local runtime copy")
    process = subprocess.Popen(
        command,
        cwd=executable.parent,
        env=environment,
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
        shell=False,
        text=True,
        encoding="utf-8",
        errors="replace",
    )
    if on_started is not None:
        try:
            on_started(process.pid)
        except Exception as error:
            tree_killed = _terminate_process_tree(process)
            stdout, stderr = process.communicate()
            return NativeProcessResult(
                process.returncode,
                stdout,
                stderr,
                False,
                started=True,
                process_terminated=True,
                timeout_tree_killed=tree_killed,
                pid=process.pid,
                error=f"on-started: {error}",
            )
    try:
        stdout, stderr = process.communicate(timeout=timeout_seconds)
    except subprocess.TimeoutExpired:
        # Persist the known timeout before cleanup, which can itself fail.
        # A diagnostic write failure must still attempt process termination.
        try:
            if on_timeout is not None:
                on_timeout()
        finally:
            tree_killed = _terminate_process_tree(process)
        stdout, stderr = process.communicate()
        return NativeProcessResult(
            process.returncode,
            stdout,
            stderr,
            True,
            started=True,
            process_terminated=True,
            timeout_tree_killed=tree_killed,
            pid=process.pid,
        )
    except OSError as error:
        _terminate_process_tree(process)
        stdout, stderr = process.communicate()
        return NativeProcessResult(
            process.returncode,
            stdout,
            stderr,
            False,
            started=True,
            process_terminated=True,
            pid=process.pid,
            error=str(error),
        )
    return NativeProcessResult(
        process.returncode,
        stdout,
        stderr,
        False,
        started=True,
        process_terminated=True,
        pid=process.pid,
    )


def run_observer(
    *,
    rom_path: Path,
    observer_path: Path,
    config: dict[str, Any],
    output_name: str,
    timeout_seconds: int,
    on_launch: Callable[[dict[str, Any]], None] | None = None,
    on_started: Callable[[int], None] | None = None,
    on_timeout: Callable[[], None] | None = None,
    on_result: Callable[[NativeProcessResult], None] | None = None,
) -> dict[str, Any]:
    """Run a tracked Lua observer with JSON configuration and return its JSON facts."""
    rom_path = rom_path.resolve(strict=True)
    observer_path = observer_path.resolve(strict=True)
    _, executable = bizhawk_contract()
    validate_lua_syntax(observer_path, executable)
    validate_lua_syntax(BOOTSTRAP_LIBRARY, executable)
    bootstrap = runtime_bootstrap(observer_path)
    DERIVED_ROOT.mkdir(parents=True, exist_ok=True)
    config_path = DERIVED_ROOT / f"{output_name}.config.lua"
    output_path = DERIVED_ROOT / f"{output_name}.observed.json"
    status_path = DERIVED_ROOT / f"{output_name}.status.txt"
    for path in (output_path, status_path):
        path.unlink(missing_ok=True)
    runtime_config = {
        **config,
        "bootstrap": bootstrap,
        "bootstrapLibraryPath": BOOTSTRAP_LIBRARY.as_posix(),
        "outputPath": output_path.as_posix(),
        "statusPath": status_path.as_posix(),
    }
    config_path.write_text("return " + _lua_literal(runtime_config) + "\n", encoding="utf-8")
    validate_lua_syntax(config_path, executable)
    environment = os.environ.copy()
    environment["SF2_H3_CONFIG"] = str(config_path)
    launch = materialize_bizhawk_launch(DERIVED_ROOT / output_name)
    executable = Path(launch["executable"])
    environment.update(launch["environment"])
    command = [
        str(executable),
        f"--config={launch['config']}",
        f"--lua={observer_path}",
        str(rom_path),
    ]
    if on_launch is not None:
        on_launch({**launch, "command": command, "observerConfig": str(config_path)})
    if on_result is not None:
        # Candidate-only diagnostic handoff; ordinary observer callers retain
        # their existing process and exception behavior below.
        result = run_native_bizhawk_process(
            command=command,
            executable=executable,
            environment=environment,
            timeout_seconds=timeout_seconds,
            on_started=on_started,
            on_timeout=on_timeout,
        )
        on_result(result)
        stdout, stderr, returncode = result.stdout, result.stderr, result.returncode
        if result.timed_out:
            raise RuntimeError(f"BizHawk observation timed out after {timeout_seconds}s")
        if result.error or not result.started or not result.process_terminated:
            raise RuntimeError(f"BizHawk native process failed: {result.error or result}")
    else:
        process = subprocess.Popen(
            command,
            cwd=launch["cwd"],
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
        returncode = process.returncode
    status_tail = (
        status_path.read_text(encoding="utf-8").strip() if status_path.exists() else "no status"
    )
    if "failure:observer-callback:" in status_tail:
        raise RuntimeError(
            f"BizHawk observer callback failure (exit code {returncode}).\n"
            f"STATUS:\n{status_tail}\nSTDOUT:\n{stdout[-4000:]}\nSTDERR:\n{stderr[-4000:]}"
        )
    if returncode != 0:
        raise RuntimeError(
            f"BizHawk observation failed with exit code {returncode}.\n"
            f"STDOUT:\n{stdout}\nSTDERR:\n{stderr}"
        )
    if not output_path.is_file():
        raise RuntimeError(
            "BizHawk observer exited without writing its observation file.\n"
            f"STDOUT:\n{stdout[-4000:]}\nSTDERR:\n{stderr[-4000:]}"
        )
    return load_json(output_path)
