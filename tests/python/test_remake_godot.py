from __future__ import annotations

import ctypes
import hashlib
import json
import os
import subprocess
import sys
import zipfile
from ctypes import wintypes
from pathlib import Path

import pytest

from sf2tool import remake_godot
from sf2tool.remake_godot import (
    ArtifactSpec,
    ProcessReceipt,
    _gate_environment,
    _parse_smoke_receipt,
    _require_clean_export_output,
    _scan_export,
    extract_zip_members,
    load_toolchain_manifest,
    run_bounded_process,
    verify_artifact,
)

ROOT = Path(__file__).resolve().parents[2]


def _pid_is_running(process_id: int) -> bool:
    if os.name == "nt":
        kernel32 = ctypes.WinDLL("kernel32", use_last_error=True)
        kernel32.OpenProcess.argtypes = [wintypes.DWORD, wintypes.BOOL, wintypes.DWORD]
        kernel32.OpenProcess.restype = wintypes.HANDLE
        kernel32.WaitForSingleObject.argtypes = [wintypes.HANDLE, wintypes.DWORD]
        kernel32.WaitForSingleObject.restype = wintypes.DWORD
        kernel32.CloseHandle.argtypes = [wintypes.HANDLE]
        kernel32.CloseHandle.restype = wintypes.BOOL
        handle = kernel32.OpenProcess(0x00100000, False, process_id)
        if not handle:
            return False
        try:
            return kernel32.WaitForSingleObject(handle, 0) == 0x00000102
        finally:
            kernel32.CloseHandle(handle)
    try:
        os.kill(process_id, 0)
    except ProcessLookupError:
        return False
    return True


def _spawn_child_script(pid_path: Path, *, exit_code: int | None) -> str:
    final_statement = "time.sleep(60)" if exit_code is None else f"raise SystemExit({exit_code})"
    return (
        "import subprocess, sys, time; from pathlib import Path; "
        "child = subprocess.Popen("
        "[sys.executable, '-c', 'import time; time.sleep(60)'], "
        "stdin=subprocess.DEVNULL, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL); "
        f"Path({str(pid_path)!r}).write_text(str(child.pid), encoding='utf-8'); "
        "print('output-before-cleanup', flush=True); "
        f"{final_statement}"
    )


def test_tracked_toolchain_manifest_locks_official_godot_dotnet_release() -> None:
    toolchain = load_toolchain_manifest(ROOT / "remake" / "toolchain.json")

    assert toolchain.version_output == "4.7.2.stable.mono.official.ed1daf0bf"
    assert toolchain.template_directory == "4.7.2.stable.mono"
    assert toolchain.public_export_preset == "Public Synthetic Windows"
    assert toolchain.editor.file_name == "Godot_v4.7.2-stable_mono_win64.zip"
    assert toolchain.editor.sha256 == (
        "a2a48473a7414c5f19fab690518caebb738c09ef9601f6bd2388676a7f53b3c0"
    )
    assert toolchain.export_templates.file_name == (
        "Godot_v4.7.2-stable_mono_export_templates.tpz"
    )
    assert toolchain.export_templates.sha256 == (
        "92f8681e349ef1f90891b792da95e3b2b0bd1ed610b78018c58feb2d87e15a9d"
    )
    assert toolchain.export_template_members == (
        "templates/version.txt",
        "templates/windows_debug_x86_64.exe",
        "templates/windows_debug_x86_64_console.exe",
        "templates/windows_release_x86_64.exe",
        "templates/windows_release_x86_64_console.exe",
    )


def test_gate_environment_disables_persistent_dotnet_build_servers(
    tmp_path: Path, monkeypatch: pytest.MonkeyPatch
) -> None:
    monkeypatch.setenv("DOTNET_BIN", sys.executable)
    monkeypatch.setenv("DOTNET_CLI_HOME", str(tmp_path / "shared-cli"))
    environment = _gate_environment(tmp_path)

    assert environment["DOTNET_CLI_USE_MSBUILD_SERVER"] == "false"
    assert environment["MSBUILDUSESERVER"] == "0"
    assert environment["MSBUILDDISABLENODEREUSE"] == "1"
    assert environment["UseSharedCompilation"] == "false"


def test_gate_runs_share_cli_home_but_isolate_scratch(
    tmp_path: Path, monkeypatch: pytest.MonkeyPatch
) -> None:
    monkeypatch.setenv("DOTNET_BIN", sys.executable)
    monkeypatch.setenv("DOTNET_CLI_HOME", str(tmp_path / "shared-cli"))
    monkeypatch.setenv("DOTNET_ADD_GLOBAL_TOOLS_TO_PATH", "true")
    original = dict(os.environ)
    first = _gate_environment(tmp_path / "one")
    second = _gate_environment(tmp_path / "two")
    assert first["DOTNET_CLI_HOME"] == second["DOTNET_CLI_HOME"] == original["DOTNET_CLI_HOME"]
    assert first["DOTNET_ADD_GLOBAL_TOOLS_TO_PATH"] == "false"
    assert second["DOTNET_ADD_GLOBAL_TOOLS_TO_PATH"] == "false"
    for key in ("APPDATA", "LOCALAPPDATA", "TEMP", "TMP"):
        assert first[key] != second[key]
    assert dict(os.environ) == original


@pytest.mark.parametrize("inherited", [None, "DOTNET_ADD_GLOBAL_TOOLS_TO_PATH",
    "dotnet_add_global_tools_to_path"])
def test_actual_non_dotnet_child_receives_path_opt_out_without_cli_configuration(
    tmp_path: Path, inherited: str | None
) -> None:
    environment = {"CALLER_VALUE": "retained"}
    if inherited is not None:
        environment[inherited] = "true"
    before = environment.copy()
    command = [sys.executable, "-c", (
        "import json,os; print(json.dumps({k:os.environ.get(k) for k in "
        "['DOTNET_ADD_GLOBAL_TOOLS_TO_PATH','DOTNET_CLI_HOME','CALLER_VALUE']}))"
    )]
    receipt = run_bounded_process("environment", command, cwd=tmp_path, environment=environment,
        timeout=10, termination_timeout=5, reap_timeout=5)
    assert receipt.passed and receipt.cleanup_status == "clean"
    assert json.loads(receipt.stdout_tail) == {
        "DOTNET_ADD_GLOBAL_TOOLS_TO_PATH": "false", "DOTNET_CLI_HOME": None,
        "CALLER_VALUE": "retained"
    }
    assert environment == before


def test_artifact_verification_is_exact_for_size_and_sha256(tmp_path: Path) -> None:
    artifact = tmp_path / "artifact.zip"
    artifact.write_bytes(b"official bytes")
    digest = hashlib.sha256(artifact.read_bytes()).hexdigest()
    spec = ArtifactSpec("artifact.zip", "https://example.invalid/artifact.zip", digest, 14)

    assert verify_artifact(artifact, spec)["status"] == "Pass"

    with pytest.raises(ValueError, match="size mismatch"):
        verify_artifact(
            artifact,
            ArtifactSpec(spec.file_name, spec.url, spec.sha256, spec.size + 1),
        )
    with pytest.raises(ValueError, match="SHA-256 mismatch"):
        verify_artifact(
            artifact,
            ArtifactSpec(spec.file_name, spec.url, "0" * 64, spec.size),
        )


def test_archive_extraction_closes_locked_members_and_strips_prefix(tmp_path: Path) -> None:
    archive = tmp_path / "templates.tpz"
    with zipfile.ZipFile(archive, "w") as output:
        output.writestr("templates/version.txt", "4.7.2.stable.mono\n")
        output.writestr("templates/windows_release_x86_64.exe", b"template")
        output.writestr("templates/unlocked.exe", b"excluded")

    destination = tmp_path / "templates"
    extracted = extract_zip_members(
        archive,
        destination,
        members=(
            "templates/version.txt",
            "templates/windows_release_x86_64.exe",
        ),
        strip_prefix="templates",
    )

    assert {path.relative_to(destination).as_posix() for path in extracted} == {
        "version.txt",
        "windows_release_x86_64.exe",
    }
    assert not (destination / "unlocked.exe").exists()


def test_archive_extraction_rejects_missing_and_escaping_members(tmp_path: Path) -> None:
    missing_archive = tmp_path / "missing.zip"
    with zipfile.ZipFile(missing_archive, "w") as output:
        output.writestr("templates/version.txt", "version")

    with pytest.raises(ValueError, match="missing locked member"):
        extract_zip_members(
            missing_archive,
            tmp_path / "missing-output",
            members=("templates/release.exe",),
        )

    unsafe_archive = tmp_path / "unsafe.zip"
    with zipfile.ZipFile(unsafe_archive, "w") as output:
        output.writestr("../escape.txt", "escape")

    with pytest.raises(ValueError, match="unsafe archive member"):
        extract_zip_members(unsafe_archive, tmp_path / "unsafe-output")
    assert not (tmp_path / "escape.txt").exists()


def test_bounded_process_records_success_without_shell(tmp_path: Path) -> None:
    receipt = run_bounded_process(
        "success",
        [sys.executable, "-c", "print('bounded')"],
        cwd=tmp_path,
        environment={},
        timeout=10,
        termination_timeout=5,
        reap_timeout=5,
    )

    assert receipt.passed
    assert receipt.stdout_tail.strip() == "bounded"
    assert receipt.cleanup_status == "clean"


def _assert_timeout_cleanup(
    tmp_path: Path, *, expected_cleanup: str = "clean"
) -> None:
    owned_pid_path = tmp_path / "owned-timeout.pid"
    unrelated = subprocess.Popen(
        [sys.executable, "-c", "import time; time.sleep(60)"],
        stdin=subprocess.DEVNULL,
        stdout=subprocess.DEVNULL,
        stderr=subprocess.DEVNULL,
    )
    try:
        receipt = run_bounded_process(
            "timeout",
            [sys.executable, "-c", _spawn_child_script(owned_pid_path, exit_code=None)],
            cwd=tmp_path,
            environment={},
            timeout=1,
            termination_timeout=5,
            reap_timeout=5,
        )

        owned_pid = int(owned_pid_path.read_text(encoding="utf-8"))
        assert receipt.timed_out
        assert "output-before-cleanup" in receipt.stdout_tail
        assert not receipt.passed
        assert not _pid_is_running(owned_pid)
        assert _pid_is_running(unrelated.pid)
        assert receipt.cleanup_status == expected_cleanup, receipt.as_dict()
    finally:
        unrelated.terminate()
        try:
            unrelated.wait(timeout=5)
        except subprocess.TimeoutExpired:
            unrelated.kill()
            unrelated.wait(timeout=5)


def test_bounded_timeout_reaps_owned_descendant_and_preserves_unrelated_process(
    tmp_path: Path,
) -> None:
    _assert_timeout_cleanup(tmp_path)


@pytest.mark.skipif(os.name != "nt", reason="Windows process handle observation")
@pytest.mark.parametrize(
    ("observation", "expected_cleanup"),
    [("eventual-exit", "clean"), ("still-pending", "survivor"), ("wait-failed", "survivor")],
)
def test_bounded_timeout_uses_confirmed_exit_after_termination_race(
    tmp_path: Path,
    monkeypatch: pytest.MonkeyPatch,
    observation: str,
    expected_cleanup: str,
) -> None:
    original_cleanup = remake_godot._WindowsJobObject.cleanup
    observed_apis = []

    class PendingTerminationApi:
        def __init__(self, kernel32, handle):
            self.kernel32 = kernel32
            self.handle = handle
            self.pending_polls = 2
            self.termination_requests = 0

        def __getattr__(self, name):
            return getattr(self.kernel32, name)

        def WaitForSingleObject(self, handle, milliseconds):
            if handle == self.handle:
                if observation == "still-pending":
                    return remake_godot._WAIT_TIMEOUT
                if observation == "wait-failed":
                    ctypes.set_last_error(6)
                    return 0xFFFFFFFF
                if self.pending_polls:
                    self.pending_polls -= 1
                    return remake_godot._WAIT_TIMEOUT
            return self.kernel32.WaitForSingleObject(handle, milliseconds)

        def TerminateProcess(self, handle, exit_code):
            result = self.kernel32.TerminateProcess(handle, exit_code)
            if handle == self.handle:
                self.termination_requests += 1
                # Model the observed ERROR_ACCESS_DENIED while termination is pending.
                # The real request still kills only the original scenario's owned child.
                ctypes.set_last_error(5)
                return 0
            return result

    def cleanup(job, process, timeout, reap_timeout):
        job._tracker_stop.set()
        job._tracker.join(timeout=timeout)
        assert not job._tracker.is_alive()
        job._discover_descendants()
        child_pid = int((tmp_path / "owned-timeout.pid").read_text(encoding="utf-8"))
        api = PendingTerminationApi(job._kernel32, job._descendant_handles[child_pid])
        job._kernel32 = api
        observed_apis.append(api)
        return original_cleanup(job, process, timeout, reap_timeout)

    monkeypatch.setattr(remake_godot._WindowsJobObject, "cleanup", cleanup)
    _assert_timeout_cleanup(tmp_path, expected_cleanup=expected_cleanup)
    assert len(observed_apis) == 1
    if observation == "eventual-exit":
        assert observed_apis[0].pending_polls == 0
        assert observed_apis[0].termination_requests > 0


def test_bounded_failure_reaps_owned_descendant(tmp_path: Path) -> None:
    owned_pid_path = tmp_path / "owned-failure.pid"

    receipt = run_bounded_process(
        "failure",
        [sys.executable, "-c", _spawn_child_script(owned_pid_path, exit_code=7)],
        cwd=tmp_path,
        environment={},
        timeout=10,
        termination_timeout=5,
        reap_timeout=5,
    )

    owned_pid = int(owned_pid_path.read_text(encoding="utf-8"))
    assert receipt.exit_code == 7
    assert not receipt.timed_out
    assert receipt.cleanup_status == "clean"
    assert "output-before-cleanup" in receipt.stdout_tail
    assert not receipt.passed
    assert not _pid_is_running(owned_pid)


def test_smoke_receipt_requires_exact_public_synthetic_projection() -> None:
    payload = {
        "status": "Pass",
        "profile": "public-synthetic",
        "scenarioId": "map3-public-synthetic-smoke",
        "exactControlledAdmission": False,
        "capability": "map3-synthetic-exploration-smoke",
        "evidenceOwner": "sf2-map3-admitted-start-runtime-v1",
        "mapId": "map3",
        "opaqueStartFacing": 3,
        "before": {"x": 56, "y": 3},
        "after": {"x": 57, "y": 3},
        "outcome": "Moved",
        "simulationStep": 1,
        "banner": "PUBLIC SYNTHETIC — NOT ORIGINAL FIDELITY",
    }
    output = "Godot Engine\nSF2_MAP3_SMOKE " + json.dumps(payload) + "\n"

    assert _parse_smoke_receipt(output) == payload

    payload["status"] = "Fail"
    with pytest.raises(RuntimeError, match="closed public-synthetic contract"):
        _parse_smoke_receipt("SF2_MAP3_SMOKE " + json.dumps(payload))


def test_zero_exit_export_still_rejects_godot_error_or_warning() -> None:
    clean = ProcessReceipt("export", ("godot",), 0, False, "clean", "Exported", "")
    errored = ProcessReceipt("export", ("godot",), 0, False, "clean", "", "ERROR: failed")
    warned = ProcessReceipt(
        "export",
        ("godot",),
        0,
        False,
        "clean",
        'Project export for preset "Public" completed with warnings.',
        "",
    )

    _require_clean_export_output(clean)
    with pytest.raises(RuntimeError, match="zero exit code"):
        _require_clean_export_output(errored)
    with pytest.raises(RuntimeError, match="zero exit code"):
        _require_clean_export_output(warned)


def test_export_scan_closes_required_outputs_without_returning_every_runtime_file(
    tmp_path: Path,
) -> None:
    required = (
        "sf2-map3-public-synthetic.exe",
        "sf2-map3-public-synthetic.pck",
        "Sf2.Remake.Application.dll",
        "Sf2.Remake.Content.dll",
        "Sf2.Remake.Domain.dll",
        "Sf2.Remake.Godot.dll",
    )
    for name in required:
        path = tmp_path / ("data" if name.endswith(".dll") else "") / name
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_bytes(name.encode())
    (tmp_path / "data/System.Private.CoreLib.dll").write_bytes(b"runtime")

    receipt = _scan_export(tmp_path)

    assert receipt["fileCount"] == 7
    assert len(receipt["requiredFiles"]) == 6
    assert len(receipt["manifestSha256"]) == 64

    (tmp_path / "leaked.rom").write_bytes(b"private")
    with pytest.raises(RuntimeError, match="forbidden payload"):
        _scan_export(tmp_path)
