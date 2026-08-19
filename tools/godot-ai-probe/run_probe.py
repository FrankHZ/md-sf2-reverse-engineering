#!/usr/bin/env python3
"""Godot AI-probe runner.

Verifies the disposable Godot C# probe project end to end:
  1. dotnet build (plain C# gate)
  2. Godot headless editor import step
  3. two bounded headless runs whose stdout must match (determinism)

Usage:
    python tools/godot-ai-probe/run_probe.py

Environment:
    GODOT_BIN   path to the Godot .NET editor executable (required)
                e.g. the exe under a Godot_v4.7.2-stable_mono_win64 directory
    DOTNET_BIN  dotnet executable (default: "dotnet" from PATH)

Exit code 0 means every gate passed; otherwise 1 with diagnostics.
"""

from __future__ import annotations

import os
import re
import subprocess
import sys
from pathlib import Path

PROBE_DIR = Path(__file__).resolve().parent
PROJECT = PROBE_DIR / "probe.csproj"
FRAMES = 120
EXPECTED_READY = "PROBE_READY seed=42"
EXPECTED_DONE = "PROBE_DONE frames=60 x=5 y=0 score=364"
GODOT_VERSION_RE = re.compile(r"^Godot Engine v\d+\.\d+\.\d+\.stable\.mono")


def _run(cmd: list[str], cwd: Path) -> subprocess.CompletedProcess[str]:
    print("+", subprocess.list2cmdline(cmd), flush=True)
    return subprocess.run(
        cmd,
        cwd=cwd,
        capture_output=True,
        text=True,
        encoding="utf-8",
        errors="replace",
    )


def _check(proc: subprocess.CompletedProcess[str], step: str) -> None:
    if proc.returncode != 0:
        print(f"FAIL {step}: exit={proc.returncode}")
        print(proc.stdout[-2000:])
        print(proc.stderr[-2000:], file=sys.stderr)
        sys.exit(1)


def main() -> int:
    godot = os.environ.get("GODOT_BIN")
    if not godot or not Path(godot).is_file():
        print("GODOT_BIN must point to the Godot .NET editor executable")
        return 1
    dotnet = os.environ.get("DOTNET_BIN", "dotnet")

    _check(_run([dotnet, "build", str(PROJECT)], PROBE_DIR), "dotnet build")

    editor = _run(
        [godot, "--headless", "--path", str(PROBE_DIR), "--editor", "--quit-after", "1"],
        PROBE_DIR,
    )
    if editor.returncode != 0:
        print(f"FAIL editor import: exit={editor.returncode}")
        print(editor.stdout[-2000:])
        print(editor.stderr[-2000:], file=sys.stderr)
        return 1

    outputs: list[str] = []
    for i in (1, 2):
        run = _run(
            [godot, "--headless", "--path", str(PROBE_DIR), "--quit-after", str(FRAMES)],
            PROBE_DIR,
        )
        _check(run, f"headless run {i}")
        output = run.stdout
        if not GODOT_VERSION_RE.match(output.splitlines()[0]):
            print(f"FAIL run {i}: unexpected first stdout line: {output.splitlines()[0]!r}")
            return 1
        if EXPECTED_READY not in output:
            print(f"FAIL run {i}: missing {EXPECTED_READY!r}")
            return 1
        if EXPECTED_DONE not in output:
            print(f"FAIL run {i}: missing {EXPECTED_DONE!r}")
            return 1
        if "ERROR" in run.stderr:
            print(f"FAIL run {i}: stderr contains ERROR")
            print(run.stderr, file=sys.stderr)
            return 1
        outputs.append(output)

    if outputs[0] != outputs[1]:
        print("FAIL determinism: run outputs differ")
        return 1

    print(f"PASS godot-ai-probe (godot={godot.splitlines()[-1] if godot else '?'})")
    return 0


if __name__ == "__main__":
    sys.exit(main())
