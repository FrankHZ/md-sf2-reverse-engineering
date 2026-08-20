#!/usr/bin/env python3
"""Run the disposable Godot C# probe from a fresh ignored scratch copy.

Usage:
    uv run python tools/godot-ai-probe/run_probe.py [--work-dir PATH]

Environment:
    GODOT_BIN   path to the exact accepted Godot .NET editor executable
    DOTNET_BIN  dotnet executable (default: ``dotnet`` from PATH)

The Godot version preflight runs before scratch creation or any build/import.
Exit code 0 means every gate passed; otherwise 1 with bounded diagnostics.
"""

from __future__ import annotations

import argparse
import os
import shutil
import signal
import subprocess
import sys
from collections.abc import Callable, Mapping, Sequence
from contextlib import suppress
from datetime import UTC, datetime
from pathlib import Path

PROBE_DIR = Path(__file__).resolve().parent
REPO_ROOT = PROBE_DIR.parents[1]
PROJECT_FILES = (
    Path("Main.tscn"),
    Path("probe.csproj"),
    Path("project.godot"),
    Path("src/Domain.cs"),
    Path("src/Domain.cs.uid"),
    Path("src/Hud.cs"),
    Path("src/Hud.cs.uid"),
    Path("src/Player.cs"),
    Path("src/Player.cs.uid"),
)

EXPECTED_GODOT_VERSION = "4.7.2.stable.mono.official.ed1daf0bf"
EXPECTED_READY = "PROBE_READY seed=42"
EXPECTED_DONE = "PROBE_DONE frames=60 x=5 y=0 score=364"
QUIT_AFTER_ITERATIONS = 120

VERSION_TIMEOUT_SECONDS = 15.0
BUILD_TIMEOUT_SECONDS = 120.0
EDITOR_TIMEOUT_SECONDS = 60.0
RUN_TIMEOUT_SECONDS = 60.0
TREE_TERMINATION_TIMEOUT_SECONDS = 5.0
TERMINATOR_REAP_TIMEOUT_SECONDS = 1.0
PROCESS_REAP_TIMEOUT_SECONDS = 5.0
DIAGNOSTIC_LIMIT = 2_000


class ProbeError(RuntimeError):
    """Expected probe failure with a concise user-facing diagnostic."""


CommandRunner = Callable[
    [list[str], Path, float, str], subprocess.CompletedProcess[str]
]
TreeTerminator = Callable[[subprocess.Popen[str]], list[str]]


def _tail(value: str | bytes | None) -> str:
    if isinstance(value, bytes):
        value = value.decode("utf-8", errors="replace")
    return (value or "")[-DIAGNOSTIC_LIMIT:]


def _taskkill_path() -> Path | None:
    system_root = os.environ.get("SYSTEMROOT")
    if system_root:
        candidate = Path(system_root) / "System32" / "taskkill.exe"
        if candidate.is_file():
            return candidate
    discovered = shutil.which("taskkill.exe")
    return Path(discovered) if discovered else None


def _terminate_process_tree(
    process: subprocess.Popen[str],
    *,
    platform: str | None = None,
    taskkill_path: Path | None = None,
    popen_factory: Callable[..., subprocess.Popen[str]] | None = None,
) -> list[str]:
    """Request bounded tree termination and always retain a direct-kill fallback."""

    platform = os.name if platform is None else platform
    popen = subprocess.Popen if popen_factory is None else popen_factory
    notes: list[str] = []

    if platform == "nt":
        taskkill = _taskkill_path() if taskkill_path is None else taskkill_path
        if taskkill is None:
            notes.append("Windows process-tree terminator was not found")
        else:
            command = [str(taskkill), "/PID", str(process.pid), "/T", "/F"]
            try:
                terminator = popen(
                    command,
                    stdin=subprocess.DEVNULL,
                    stdout=subprocess.DEVNULL,
                    stderr=subprocess.DEVNULL,
                    shell=False,
                )
                try:
                    exit_code = terminator.wait(timeout=TREE_TERMINATION_TIMEOUT_SECONDS)
                    notes.append(f"Windows process-tree termination exit={exit_code}")
                except subprocess.TimeoutExpired:
                    notes.append(
                        "Windows process-tree terminator timed out after "
                        f"{TREE_TERMINATION_TIMEOUT_SECONDS:g}s"
                    )
                    try:
                        terminator.kill()
                    except OSError as exc:
                        notes.append(f"terminator kill failed: {exc}")
                    try:
                        terminator.wait(timeout=TERMINATOR_REAP_TIMEOUT_SECONDS)
                    except subprocess.TimeoutExpired:
                        notes.append(
                            "terminator did not reap within "
                            f"{TERMINATOR_REAP_TIMEOUT_SECONDS:g}s"
                        )
            except OSError as exc:
                notes.append(f"Windows process-tree terminator failed to start: {exc}")
    else:
        kill_group = getattr(os, "killpg", None)
        if kill_group is not None:
            try:
                kill_group(process.pid, getattr(signal, "SIGKILL", signal.SIGTERM))
                notes.append("POSIX process-group termination requested")
            except OSError as exc:
                notes.append(f"POSIX process-group termination failed: {exc}")

    if process.poll() is None:
        try:
            process.kill()
            notes.append("direct process kill requested as fallback")
        except OSError as exc:
            notes.append(f"direct process kill failed: {exc}")
    return notes


def _close_capture_pipes(process: subprocess.Popen[str]) -> None:
    for stream in (process.stdout, process.stderr):
        if stream is None:
            continue
        with suppress(OSError):
            stream.close()


def _run(
    cmd: list[str],
    cwd: Path,
    timeout_seconds: float,
    step: str,
    *,
    popen_factory: Callable[..., subprocess.Popen[str]] | None = None,
    tree_terminator: TreeTerminator | None = None,
) -> subprocess.CompletedProcess[str]:
    """Run one process with bounded execution, tree cleanup, and pipe reaping."""

    print("+", subprocess.list2cmdline(cmd), flush=True)
    popen = subprocess.Popen if popen_factory is None else popen_factory
    platform_options: dict[str, int | bool]
    if os.name == "nt":
        platform_options = {
            "creationflags": getattr(subprocess, "CREATE_NEW_PROCESS_GROUP", 0)
        }
    else:
        platform_options = {"start_new_session": True}
    try:
        process = popen(
            cmd,
            cwd=cwd,
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            text=True,
            encoding="utf-8",
            errors="replace",
            shell=False,
            **platform_options,
        )
    except OSError as exc:
        raise ProbeError(f"FAIL {step}: could not start process: {exc}") from exc

    try:
        stdout, stderr = process.communicate(timeout=timeout_seconds)
    except subprocess.TimeoutExpired as exc:
        terminate = _terminate_process_tree if tree_terminator is None else tree_terminator
        notes = terminate(process)
        try:
            stdout, stderr = process.communicate(timeout=PROCESS_REAP_TIMEOUT_SECONDS)
        except subprocess.TimeoutExpired as reap_exc:
            _close_capture_pipes(process)
            detail = (
                _tail(reap_exc.stderr)
                or _tail(reap_exc.output)
                or _tail(exc.stderr)
                or _tail(exc.output)
            )
            suffix = f"\n{detail}" if detail else ""
            raise ProbeError(
                f"FAIL {step}: timed out after {timeout_seconds:g}s; "
                f"termination notes: {'; '.join(notes)}; pipe reap did not finish "
                f"within {PROCESS_REAP_TIMEOUT_SECONDS:g}s and capture handles were closed"
                f"{suffix}"
            ) from reap_exc
        detail = _tail(stderr) or _tail(stdout)
        suffix = f"\n{detail}" if detail else ""
        raise ProbeError(
            f"FAIL {step}: timed out after {timeout_seconds:g}s; "
            f"termination notes: {'; '.join(notes)}; process was reaped{suffix}"
        ) from exc

    return subprocess.CompletedProcess(cmd, process.returncode, stdout, stderr)


def _check(proc: subprocess.CompletedProcess[str], step: str) -> None:
    if proc.returncode == 0:
        return
    raise ProbeError(
        f"FAIL {step}: exit={proc.returncode}\n"
        f"stdout tail:\n{_tail(proc.stdout)}\n"
        f"stderr tail:\n{_tail(proc.stderr)}"
    )


def _require_godot_version(proc: subprocess.CompletedProcess[str]) -> None:
    _check(proc, "Godot version preflight")
    observed = proc.stdout.strip()
    if observed != EXPECTED_GODOT_VERSION:
        raise ProbeError(
            "FAIL Godot version preflight: "
            f"expected {EXPECTED_GODOT_VERSION!r}, observed {observed!r}"
        )


def _fresh_run_id() -> str:
    stamp = datetime.now(UTC).strftime("%Y%m%dT%H%M%S%fZ")
    return f"run-{stamp}-{os.getpid()}"


def _prepare_scratch(
    requested: Path | None,
    *,
    source_dir: Path = PROBE_DIR,
    repo_root: Path = REPO_ROOT,
    run_id: str | None = None,
) -> Path:
    """Copy the project inputs into a new ignored directory without deleting paths."""

    source_dir = source_dir.resolve()
    repo_root = repo_root.resolve()
    local_root = (repo_root / "local").resolve()
    default_root = local_root / "derived" / "godot-ai-probe"

    if requested is None:
        candidate = default_root / (run_id or _fresh_run_id())
    else:
        candidate = requested.expanduser()
        if not candidate.is_absolute():
            candidate = repo_root / candidate
    candidate = candidate.resolve()

    if candidate == local_root or not candidate.is_relative_to(local_root):
        raise ProbeError(
            "FAIL scratch safety: --work-dir must be a fresh child of "
            f"{local_root}"
        )
    if candidate.exists():
        raise ProbeError(f"FAIL scratch freshness: path already exists: {candidate}")

    missing = [str(path) for path in PROJECT_FILES if not (source_dir / path).is_file()]
    if missing:
        raise ProbeError(f"FAIL scratch source inventory: missing {', '.join(missing)}")

    candidate.parent.mkdir(parents=True, exist_ok=True)
    try:
        candidate.mkdir()
        for relative in PROJECT_FILES:
            destination = candidate / relative
            destination.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(source_dir / relative, destination)
    except OSError as exc:
        raise ProbeError(
            f"FAIL scratch copy: {exc}; partial scratch, if any, was not deleted: {candidate}"
        ) from exc

    return candidate


def _parse_run_output(proc: subprocess.CompletedProcess[str], run_number: int) -> str:
    _check(proc, f"headless run {run_number}")
    lines = proc.stdout.splitlines()
    expected_prefix = f"Godot Engine v{EXPECTED_GODOT_VERSION}"
    if not lines or not lines[0].startswith(expected_prefix):
        observed = lines[0] if lines else "<no stdout>"
        raise ProbeError(
            f"FAIL run {run_number}: unexpected first stdout line: {observed!r}"
        )
    if EXPECTED_READY not in proc.stdout:
        raise ProbeError(f"FAIL run {run_number}: missing {EXPECTED_READY!r}")
    if EXPECTED_DONE not in proc.stdout:
        raise ProbeError(f"FAIL run {run_number}: missing {EXPECTED_DONE!r}")
    if "ERROR" in proc.stderr:
        raise ProbeError(
            f"FAIL run {run_number}: stderr contains ERROR\n{_tail(proc.stderr)}"
        )
    return proc.stdout


def _parse_args(argv: Sequence[str] | None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "--work-dir",
        type=Path,
        help=(
            "fresh ignored scratch directory under local/; default creates a unique "
            "directory under local/derived/godot-ai-probe"
        ),
    )
    return parser.parse_args(argv)


def main(
    argv: Sequence[str] | None = None,
    *,
    environ: Mapping[str, str] | None = None,
    runner: CommandRunner | None = None,
) -> int:
    args = _parse_args(argv)
    environment = os.environ if environ is None else environ
    run_command = _run if runner is None else runner

    godot = environment.get("GODOT_BIN")
    if not godot or not Path(godot).is_file():
        print("GODOT_BIN must point to the Godot 4.7.2 .NET editor executable")
        return 1
    godot = str(Path(godot).resolve())
    dotnet = environment.get("DOTNET_BIN", "dotnet")

    try:
        version = run_command(
            [godot, "--version"], PROBE_DIR, VERSION_TIMEOUT_SECONDS, "Godot version"
        )
        _require_godot_version(version)

        scratch = _prepare_scratch(args.work_dir)
        project = scratch / "probe.csproj"
        print(f"SCRATCH_DIR={scratch}")

        build = run_command(
            [dotnet, "build", str(project)],
            scratch,
            BUILD_TIMEOUT_SECONDS,
            "dotnet build",
        )
        _check(build, "dotnet build")

        editor = run_command(
            [
                godot,
                "--headless",
                "--path",
                str(scratch),
                "--editor",
                "--quit-after",
                "1",
            ],
            scratch,
            EDITOR_TIMEOUT_SECONDS,
            "editor import",
        )
        _check(editor, "editor import")

        outputs: list[str] = []
        for run_number in (1, 2):
            run = run_command(
                [
                    godot,
                    "--headless",
                    "--path",
                    str(scratch),
                    "--quit-after",
                    str(QUIT_AFTER_ITERATIONS),
                ],
                scratch,
                RUN_TIMEOUT_SECONDS,
                f"headless run {run_number}",
            )
            outputs.append(_parse_run_output(run, run_number))

        if outputs[0] != outputs[1]:
            raise ProbeError("FAIL determinism: run outputs differ")
    except ProbeError as exc:
        print(str(exc), file=sys.stderr)
        return 1

    print(
        "PASS godot-ai-probe "
        f"(godot={EXPECTED_GODOT_VERSION}, frames=60, "
        f"quit_after_iterations={QUIT_AFTER_ITERATIONS}, scratch={scratch})"
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
