from __future__ import annotations

import importlib.util
import subprocess
import sys
from pathlib import Path
from types import ModuleType

import pytest

REPO_ROOT = Path(__file__).resolve().parents[2]
RUNNER_PATH = REPO_ROOT / "tools" / "godot-ai-probe" / "run_probe.py"


def _load_runner() -> ModuleType:
    spec = importlib.util.spec_from_file_location("godot_ai_probe_runner", RUNNER_PATH)
    assert spec is not None and spec.loader is not None
    module = importlib.util.module_from_spec(spec)
    previous = sys.dont_write_bytecode
    sys.dont_write_bytecode = True
    try:
        spec.loader.exec_module(module)
    finally:
        sys.dont_write_bytecode = previous
    return module


PROBE = _load_runner()


def _completed(
    *,
    stdout: str = "",
    stderr: str = "",
    returncode: int = 0,
) -> subprocess.CompletedProcess[str]:
    return subprocess.CompletedProcess([], returncode, stdout, stderr)


def _valid_run_output() -> str:
    return (
        f"Godot Engine v{PROBE.EXPECTED_GODOT_VERSION} - https://godotengine.org\n"
        f"{PROBE.EXPECTED_READY}\n"
        f"{PROBE.EXPECTED_DONE}\n"
    )


def _make_probe_source(root: Path) -> None:
    for relative in PROBE.PROJECT_FILES:
        path = root / relative
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(f"owned input: {relative.as_posix()}\n", encoding="utf-8")
    generated = root / ".godot" / "generated.txt"
    generated.parent.mkdir()
    generated.write_text("must not be copied\n", encoding="utf-8")


def test_exact_godot_version_is_required() -> None:
    PROBE._require_godot_version(_completed(stdout=f"{PROBE.EXPECTED_GODOT_VERSION}\n"))

    with pytest.raises(PROBE.ProbeError, match="expected .* observed"):
        PROBE._require_godot_version(
            _completed(stdout="4.7.1.stable.mono.official.some_other_build\n")
        )


def test_wrong_version_fails_before_scratch_or_build(
    tmp_path: Path, monkeypatch: pytest.MonkeyPatch
) -> None:
    godot = tmp_path / "Godot.exe"
    godot.write_bytes(b"fake")
    calls: list[tuple[list[str], str]] = []

    def fake_runner(
        cmd: list[str], _cwd: Path, _timeout: float, step: str
    ) -> subprocess.CompletedProcess[str]:
        calls.append((cmd, step))
        return _completed(stdout="4.7.2.stable.mono.official.wrong_hash\n")

    def forbidden_scratch(_requested: Path | None) -> Path:
        raise AssertionError("wrong-version preflight must not create scratch")

    monkeypatch.setattr(PROBE, "_prepare_scratch", forbidden_scratch)
    result = PROBE.main([], environ={"GODOT_BIN": str(godot)}, runner=fake_runner)

    assert result == 1
    assert calls == [([str(godot), "--version"], "Godot version")]


def test_each_step_has_an_explicit_timeout_and_deterministic_result(
    tmp_path: Path, monkeypatch: pytest.MonkeyPatch
) -> None:
    godot = tmp_path / "Godot.exe"
    godot.write_bytes(b"fake")
    scratch = tmp_path / "repo" / "local" / "derived" / "probe"
    calls: list[tuple[list[str], float, str]] = []

    def fake_prepare(_requested: Path | None) -> Path:
        assert calls and calls[0][2] == "Godot version"
        scratch.mkdir(parents=True)
        return scratch

    def fake_runner(
        cmd: list[str], _cwd: Path, timeout: float, step: str
    ) -> subprocess.CompletedProcess[str]:
        calls.append((cmd, timeout, step))
        if step == "Godot version":
            return _completed(stdout=f"{PROBE.EXPECTED_GODOT_VERSION}\n")
        if step.startswith("headless run"):
            return _completed(stdout=_valid_run_output())
        return _completed()

    monkeypatch.setattr(PROBE, "_prepare_scratch", fake_prepare)
    result = PROBE.main([], environ={"GODOT_BIN": str(godot)}, runner=fake_runner)

    assert result == 0
    assert [(timeout, step) for _, timeout, step in calls] == [
        (PROBE.VERSION_TIMEOUT_SECONDS, "Godot version"),
        (PROBE.BUILD_TIMEOUT_SECONDS, "dotnet build"),
        (PROBE.EDITOR_TIMEOUT_SECONDS, "editor import"),
        (PROBE.RUN_TIMEOUT_SECONDS, "headless run 1"),
        (PROBE.RUN_TIMEOUT_SECONDS, "headless run 2"),
    ]
    run_commands = [cmd for cmd, _, step in calls if step.startswith("headless run")]
    assert all(
        command[-2:] == ["--quit-after", str(PROBE.QUIT_AFTER_ITERATIONS)]
        for command in run_commands
    )


def test_timeout_kills_and_reaps_process(
    tmp_path: Path, monkeypatch: pytest.MonkeyPatch
) -> None:
    class FakeProcess:
        returncode = -9

        def __init__(self) -> None:
            self.killed = False
            self.calls = 0

        def communicate(self, timeout: float | None = None) -> tuple[str, str]:
            self.calls += 1
            if timeout is not None:
                raise subprocess.TimeoutExpired(["fake"], timeout)
            return "partial stdout", "partial stderr"

        def kill(self) -> None:
            self.killed = True

    process = FakeProcess()
    monkeypatch.setattr(PROBE.subprocess, "Popen", lambda *_args, **_kwargs: process)

    with pytest.raises(PROBE.ProbeError, match="timed out after 3s; process was killed and reaped"):
        PROBE._run(["fake"], tmp_path, 3.0, "fake step")

    assert process.killed
    assert process.calls == 2


def test_scratch_must_be_fresh_safe_and_contains_only_project_inputs(tmp_path: Path) -> None:
    repo_root = tmp_path / "repo"
    source = tmp_path / "source"
    _make_probe_source(source)
    scratch = repo_root / "local" / "derived" / "godot-ai-probe" / "case-one"

    created = PROBE._prepare_scratch(
        scratch, source_dir=source, repo_root=repo_root, run_id="unused"
    )
    copied = {
        path.relative_to(created).as_posix() for path in created.rglob("*") if path.is_file()
    }

    assert created == scratch.resolve()
    assert copied == {path.as_posix() for path in PROBE.PROJECT_FILES}
    assert not (created / ".godot").exists()

    sentinel = created / "sentinel.txt"
    sentinel.write_text("preserve", encoding="utf-8")
    with pytest.raises(PROBE.ProbeError, match="path already exists"):
        PROBE._prepare_scratch(scratch, source_dir=source, repo_root=repo_root)
    assert sentinel.read_text(encoding="utf-8") == "preserve"

    with pytest.raises(PROBE.ProbeError, match="must be a fresh child"):
        PROBE._prepare_scratch(
            repo_root / "outside-local", source_dir=source, repo_root=repo_root
        )

    default = PROBE._prepare_scratch(
        None, source_dir=source, repo_root=repo_root, run_id="default-case"
    )
    assert default == (
        repo_root / "local" / "derived" / "godot-ai-probe" / "default-case"
    ).resolve()


def test_run_output_parser_enforces_version_markers_errors_and_stability() -> None:
    first = PROBE._parse_run_output(_completed(stdout=_valid_run_output()), 1)
    second = PROBE._parse_run_output(_completed(stdout=_valid_run_output()), 2)
    assert first == second

    wrong_version = _valid_run_output().replace(PROBE.EXPECTED_GODOT_VERSION, "4.7.3.other")
    with pytest.raises(PROBE.ProbeError, match="unexpected first stdout line"):
        PROBE._parse_run_output(_completed(stdout=wrong_version), 1)
    with pytest.raises(PROBE.ProbeError, match="missing 'PROBE_DONE"):
        PROBE._parse_run_output(
            _completed(stdout=_valid_run_output().replace(PROBE.EXPECTED_DONE, "")), 1
        )
    with pytest.raises(PROBE.ProbeError, match="stderr contains ERROR"):
        PROBE._parse_run_output(
            _completed(stdout=_valid_run_output(), stderr="ERROR: synthetic"), 1
        )
