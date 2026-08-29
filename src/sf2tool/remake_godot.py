"""Official-CLI Godot gate for the public-synthetic remake profile."""

from __future__ import annotations

import argparse
import ctypes
import hashlib
import json
import os
import shutil
import signal
import subprocess
import threading
import time
import uuid
import zipfile
from collections.abc import Callable, Iterable, Mapping
from contextlib import suppress
from ctypes import wintypes
from dataclasses import dataclass
from pathlib import Path, PurePosixPath
from typing import Any

from sf2tool.paths import REPO_ROOT, display_path, repo_path

DEFAULT_MANIFEST = repo_path("remake/toolchain.json")
DEFAULT_TOOLCHAIN_ROOT = repo_path("local/toolchains/godot-4.7.2")
DEFAULT_PROJECT = repo_path("remake/game")
DEFAULT_SCRATCH_PARENT = repo_path("local/gates/godot")
SMOKE_MARKER = "SF2_MAP3_SMOKE "
MAX_DIAGNOSTIC_CHARACTERS = 8192
_JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE = 0x00002000
_JOB_OBJECT_BASIC_ACCOUNTING_INFORMATION_CLASS = 1
_JOB_OBJECT_EXTENDED_LIMIT_INFORMATION_CLASS = 9
_THREAD_SUSPEND_RESUME = 0x0002
_TH32CS_SNAPTHREAD = 0x00000004
_TH32CS_SNAPPROCESS = 0x00000002
_PROCESS_TERMINATE = 0x0001
_PROCESS_QUERY_LIMITED_INFORMATION = 0x1000
_SYNCHRONIZE = 0x00100000
_WAIT_OBJECT_0 = 0x00000000
_WAIT_TIMEOUT = 0x00000102


class _JobObjectBasicAccountingInformation(ctypes.Structure):
    _fields_ = [
        ("TotalUserTime", ctypes.c_int64),
        ("TotalKernelTime", ctypes.c_int64),
        ("ThisPeriodTotalUserTime", ctypes.c_int64),
        ("ThisPeriodTotalKernelTime", ctypes.c_int64),
        ("TotalPageFaultCount", wintypes.DWORD),
        ("TotalProcesses", wintypes.DWORD),
        ("ActiveProcesses", wintypes.DWORD),
        ("TotalTerminatedProcesses", wintypes.DWORD),
    ]


class _JobObjectBasicLimitInformation(ctypes.Structure):
    _fields_ = [
        ("PerProcessUserTimeLimit", ctypes.c_int64),
        ("PerJobUserTimeLimit", ctypes.c_int64),
        ("LimitFlags", wintypes.DWORD),
        ("MinimumWorkingSetSize", ctypes.c_size_t),
        ("MaximumWorkingSetSize", ctypes.c_size_t),
        ("ActiveProcessLimit", wintypes.DWORD),
        ("Affinity", ctypes.c_size_t),
        ("PriorityClass", wintypes.DWORD),
        ("SchedulingClass", wintypes.DWORD),
    ]


class _IoCounters(ctypes.Structure):
    _fields_ = [
        ("ReadOperationCount", ctypes.c_uint64),
        ("WriteOperationCount", ctypes.c_uint64),
        ("OtherOperationCount", ctypes.c_uint64),
        ("ReadTransferCount", ctypes.c_uint64),
        ("WriteTransferCount", ctypes.c_uint64),
        ("OtherTransferCount", ctypes.c_uint64),
    ]


class _JobObjectExtendedLimitInformation(ctypes.Structure):
    _fields_ = [
        ("BasicLimitInformation", _JobObjectBasicLimitInformation),
        ("IoInfo", _IoCounters),
        ("ProcessMemoryLimit", ctypes.c_size_t),
        ("JobMemoryLimit", ctypes.c_size_t),
        ("PeakProcessMemoryUsed", ctypes.c_size_t),
        ("PeakJobMemoryUsed", ctypes.c_size_t),
    ]


class _ThreadEntry32(ctypes.Structure):
    _fields_ = [
        ("dwSize", wintypes.DWORD),
        ("cntUsage", wintypes.DWORD),
        ("th32ThreadID", wintypes.DWORD),
        ("th32OwnerProcessID", wintypes.DWORD),
        ("tpBasePri", wintypes.LONG),
        ("tpDeltaPri", wintypes.LONG),
        ("dwFlags", wintypes.DWORD),
    ]


class _ProcessEntry32W(ctypes.Structure):
    _fields_ = [
        ("dwSize", wintypes.DWORD),
        ("cntUsage", wintypes.DWORD),
        ("th32ProcessID", wintypes.DWORD),
        ("th32DefaultHeapID", ctypes.c_size_t),
        ("th32ModuleID", wintypes.DWORD),
        ("cntThreads", wintypes.DWORD),
        ("th32ParentProcessID", wintypes.DWORD),
        ("pcPriClassBase", wintypes.LONG),
        ("dwFlags", wintypes.DWORD),
        ("szExeFile", wintypes.WCHAR * 260),
    ]


@dataclass(frozen=True)
class ArtifactSpec:
    """One immutable official release artifact."""

    file_name: str
    url: str
    sha256: str
    size: int


@dataclass(frozen=True)
class GateTimeouts:
    """Finite wall-clock budgets for every external-process stage."""

    version: int
    restore: int
    build: int
    import_project: int
    run: int
    export: int
    termination: int
    reap: int


@dataclass(frozen=True)
class GodotToolchain:
    """Validated tracked identity for the editor and export templates."""

    version_output: str
    template_directory: str
    public_export_preset: str
    editor: ArtifactSpec
    export_templates: ArtifactSpec
    export_template_members: tuple[str, ...]
    timeouts: GateTimeouts


@dataclass(frozen=True)
class ProcessReceipt:
    """Bounded result of one native process."""

    step: str
    command: tuple[str, ...]
    exit_code: int | None
    timed_out: bool
    cleanup_status: str
    stdout_tail: str
    stderr_tail: str

    @property
    def passed(self) -> bool:
        return self.exit_code == 0 and not self.timed_out and self.cleanup_status == "clean"

    def as_dict(self) -> dict[str, Any]:
        return {
            "step": self.step,
            "command": list(self.command),
            "exitCode": self.exit_code,
            "timedOut": self.timed_out,
            "cleanupStatus": self.cleanup_status,
            "stdoutTail": self.stdout_tail,
            "stderrTail": self.stderr_tail,
            "status": "Pass" if self.passed else "Fail",
        }


def _required_mapping(value: object, field: str) -> Mapping[str, object]:
    if not isinstance(value, dict):
        raise ValueError(f"{field} must be an object")
    return value


def _required_string(mapping: Mapping[str, object], field: str) -> str:
    value = mapping.get(field)
    if not isinstance(value, str) or not value.strip():
        raise ValueError(f"{field} must be a non-empty string")
    return value


def _required_positive_int(mapping: Mapping[str, object], field: str) -> int:
    value = mapping.get(field)
    if not isinstance(value, int) or isinstance(value, bool) or value < 1:
        raise ValueError(f"{field} must be a positive integer")
    return value


def _artifact(mapping: Mapping[str, object], field: str) -> ArtifactSpec:
    artifact = _required_mapping(mapping.get(field), field)
    sha256 = _required_string(artifact, "sha256").lower()
    if len(sha256) != 64 or any(character not in "0123456789abcdef" for character in sha256):
        raise ValueError(f"{field}.sha256 must be a lowercase SHA-256 digest")
    return ArtifactSpec(
        _required_string(artifact, "fileName"),
        _required_string(artifact, "url"),
        sha256,
        _required_positive_int(artifact, "size"),
    )


def load_toolchain_manifest(path: Path = DEFAULT_MANIFEST) -> GodotToolchain:
    """Load and validate the tracked Godot artifact lock."""

    try:
        document = json.loads(path.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError) as error:
        raise ValueError(f"cannot read Godot toolchain manifest: {error}") from error
    root = _required_mapping(document, "manifest")
    if root.get("schemaVersion") != 1:
        raise ValueError("unsupported Godot toolchain manifest schemaVersion")
    release_repository = _required_string(root, "releaseRepository")
    if release_repository != "https://github.com/godotengine/godot-builds":
        raise ValueError("Godot artifacts must come from the official godot-builds repository")
    if _required_string(root, "releaseTag") != "4.7.2-stable":
        raise ValueError("Godot releaseTag must remain 4.7.2-stable")

    templates = _required_mapping(root.get("exportTemplates"), "exportTemplates")
    members = templates.get("members")
    if not isinstance(members, list) or not members or not all(
        isinstance(member, str) and member.startswith("templates/") for member in members
    ):
        raise ValueError("exportTemplates.members must be a non-empty templates/ list")
    if len(set(members)) != len(members):
        raise ValueError("exportTemplates.members must not contain duplicates")

    timeouts = _required_mapping(root.get("timeoutsSeconds"), "timeoutsSeconds")
    return GodotToolchain(
        _required_string(root, "godotVersionOutput"),
        _required_string(root, "templateDirectory"),
        _required_string(root, "publicExportPreset"),
        _artifact(root, "editor"),
        _artifact(root, "exportTemplates"),
        tuple(members),
        GateTimeouts(
            _required_positive_int(timeouts, "version"),
            _required_positive_int(timeouts, "restore"),
            _required_positive_int(timeouts, "build"),
            _required_positive_int(timeouts, "import"),
            _required_positive_int(timeouts, "run"),
            _required_positive_int(timeouts, "export"),
            _required_positive_int(timeouts, "termination"),
            _required_positive_int(timeouts, "reap"),
        ),
    )


def _sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        while chunk := stream.read(1024 * 1024):
            digest.update(chunk)
    return digest.hexdigest()


def verify_artifact(path: Path, spec: ArtifactSpec) -> dict[str, object]:
    """Fail closed unless a local artifact exactly matches its tracked lock."""

    if not path.is_file():
        raise ValueError(f"required Godot artifact is unavailable: {spec.file_name}")
    size = path.stat().st_size
    if size != spec.size:
        raise ValueError(
            f"Godot artifact size mismatch for {spec.file_name}: expected {spec.size}, got {size}"
        )
    digest = _sha256(path)
    if digest != spec.sha256:
        raise ValueError(
            f"Godot artifact SHA-256 mismatch for {spec.file_name}: "
            f"expected {spec.sha256}, got {digest}"
        )
    return {
        "fileName": spec.file_name,
        "url": spec.url,
        "size": size,
        "sha256": digest,
        "status": "Pass",
    }


def _safe_member_path(member: str) -> PurePosixPath:
    normalized = PurePosixPath(member.replace("\\", "/"))
    if normalized.is_absolute() or ".." in normalized.parts or not normalized.parts:
        raise ValueError(f"unsafe archive member: {member}")
    return normalized


def extract_zip_members(
    archive: Path,
    destination: Path,
    *,
    members: Iterable[str] | None = None,
    strip_prefix: str | None = None,
) -> tuple[Path, ...]:
    """Extract exact safe ZIP members into a fresh caller-owned directory."""

    if destination.exists():
        raise ValueError(f"archive destination already exists: {destination}")
    requested = None if members is None else tuple(members)
    requested_set = None if requested is None else set(requested)
    extracted: list[Path] = []
    destination.mkdir(parents=True)
    with zipfile.ZipFile(archive) as source:
        names = set(source.namelist())
        if requested_set is not None:
            missing = sorted(requested_set - names)
            if missing:
                raise ValueError(f"archive is missing locked member(s): {', '.join(missing)}")
        for info in source.infolist():
            if requested_set is not None and info.filename not in requested_set:
                continue
            member = _safe_member_path(info.filename)
            if strip_prefix is not None:
                prefix = PurePosixPath(strip_prefix)
                if member.parts[: len(prefix.parts)] != prefix.parts:
                    raise ValueError(f"archive member does not use {strip_prefix}: {info.filename}")
                member = PurePosixPath(*member.parts[len(prefix.parts) :])
                if not member.parts:
                    continue
            target = destination.joinpath(*member.parts)
            if not target.resolve().is_relative_to(destination.resolve()):
                raise ValueError(f"archive member escapes destination: {info.filename}")
            if info.is_dir():
                target.mkdir(parents=True, exist_ok=True)
                continue
            target.parent.mkdir(parents=True, exist_ok=True)
            with source.open(info) as input_stream, target.open("wb") as output_stream:
                shutil.copyfileobj(input_stream, output_stream)
            extracted.append(target)
    return tuple(extracted)


def _bounded_tail(value: str) -> str:
    return value[-MAX_DIAGNOSTIC_CHARACTERS:]


class _WindowsJobObject:
    """Suspended-launch Job plus ancestry handles for an ownership-proven Windows tree."""

    def __init__(self, process: subprocess.Popen[str]) -> None:
        kernel32 = ctypes.WinDLL("kernel32", use_last_error=True)
        kernel32.CreateJobObjectW.argtypes = [ctypes.c_void_p, wintypes.LPCWSTR]
        kernel32.CreateJobObjectW.restype = wintypes.HANDLE
        kernel32.SetInformationJobObject.argtypes = [
            wintypes.HANDLE,
            ctypes.c_int,
            ctypes.c_void_p,
            wintypes.DWORD,
        ]
        kernel32.SetInformationJobObject.restype = wintypes.BOOL
        kernel32.AssignProcessToJobObject.argtypes = [wintypes.HANDLE, wintypes.HANDLE]
        kernel32.AssignProcessToJobObject.restype = wintypes.BOOL
        kernel32.QueryInformationJobObject.argtypes = [
            wintypes.HANDLE,
            ctypes.c_int,
            ctypes.c_void_p,
            wintypes.DWORD,
            ctypes.POINTER(wintypes.DWORD),
        ]
        kernel32.QueryInformationJobObject.restype = wintypes.BOOL
        kernel32.TerminateJobObject.argtypes = [wintypes.HANDLE, wintypes.UINT]
        kernel32.TerminateJobObject.restype = wintypes.BOOL
        kernel32.CreateToolhelp32Snapshot.argtypes = [wintypes.DWORD, wintypes.DWORD]
        kernel32.CreateToolhelp32Snapshot.restype = wintypes.HANDLE
        kernel32.Thread32First.argtypes = [wintypes.HANDLE, ctypes.POINTER(_ThreadEntry32)]
        kernel32.Thread32First.restype = wintypes.BOOL
        kernel32.Thread32Next.argtypes = [wintypes.HANDLE, ctypes.POINTER(_ThreadEntry32)]
        kernel32.Thread32Next.restype = wintypes.BOOL
        kernel32.OpenThread.argtypes = [wintypes.DWORD, wintypes.BOOL, wintypes.DWORD]
        kernel32.OpenThread.restype = wintypes.HANDLE
        kernel32.ResumeThread.argtypes = [wintypes.HANDLE]
        kernel32.ResumeThread.restype = wintypes.DWORD
        kernel32.Process32FirstW.argtypes = [
            wintypes.HANDLE,
            ctypes.POINTER(_ProcessEntry32W),
        ]
        kernel32.Process32FirstW.restype = wintypes.BOOL
        kernel32.Process32NextW.argtypes = [
            wintypes.HANDLE,
            ctypes.POINTER(_ProcessEntry32W),
        ]
        kernel32.Process32NextW.restype = wintypes.BOOL
        kernel32.OpenProcess.argtypes = [wintypes.DWORD, wintypes.BOOL, wintypes.DWORD]
        kernel32.OpenProcess.restype = wintypes.HANDLE
        kernel32.TerminateProcess.argtypes = [wintypes.HANDLE, wintypes.UINT]
        kernel32.TerminateProcess.restype = wintypes.BOOL
        kernel32.WaitForSingleObject.argtypes = [wintypes.HANDLE, wintypes.DWORD]
        kernel32.WaitForSingleObject.restype = wintypes.DWORD
        kernel32.CloseHandle.argtypes = [wintypes.HANDLE]
        kernel32.CloseHandle.restype = wintypes.BOOL

        handle = kernel32.CreateJobObjectW(None, None)
        if not handle:
            raise ctypes.WinError(ctypes.get_last_error())
        self._kernel32 = kernel32
        self._handle = handle
        self._root_pid = process.pid
        self._descendant_handles: dict[int, int] = {}
        self._tracker_failed = False
        self._tracker_stop = threading.Event()
        self._tracker: threading.Thread | None = None
        try:
            limits = _JobObjectExtendedLimitInformation()
            limits.BasicLimitInformation.LimitFlags = _JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE
            if not kernel32.SetInformationJobObject(
                handle,
                _JOB_OBJECT_EXTENDED_LIMIT_INFORMATION_CLASS,
                ctypes.byref(limits),
                ctypes.sizeof(limits),
            ):
                raise ctypes.WinError(ctypes.get_last_error())
            process_handle = wintypes.HANDLE(int(process._handle))
            if not kernel32.AssignProcessToJobObject(handle, process_handle):
                raise ctypes.WinError(ctypes.get_last_error())
        except Exception:
            self.close()
            raise

    def resume(self, process: subprocess.Popen[str]) -> None:
        snapshot = self._kernel32.CreateToolhelp32Snapshot(_TH32CS_SNAPTHREAD, 0)
        if snapshot == wintypes.HANDLE(-1).value:
            raise ctypes.WinError(ctypes.get_last_error())
        resumed = 0
        try:
            entry = _ThreadEntry32()
            entry.dwSize = ctypes.sizeof(entry)
            available = self._kernel32.Thread32First(snapshot, ctypes.byref(entry))
            while available:
                if entry.th32OwnerProcessID == process.pid:
                    thread = self._kernel32.OpenThread(
                        _THREAD_SUSPEND_RESUME,
                        False,
                        entry.th32ThreadID,
                    )
                    if not thread:
                        raise ctypes.WinError(ctypes.get_last_error())
                    try:
                        if self._kernel32.ResumeThread(thread) == 0xFFFFFFFF:
                            raise ctypes.WinError(ctypes.get_last_error())
                        resumed += 1
                    finally:
                        self._kernel32.CloseHandle(thread)
                available = self._kernel32.Thread32Next(snapshot, ctypes.byref(entry))
        finally:
            self._kernel32.CloseHandle(snapshot)
        if resumed != 1:
            raise RuntimeError(
                "expected one suspended launch thread for owned process "
                f"{process.pid}; got {resumed}"
            )
        self._tracker = threading.Thread(
            target=self._track_descendants,
            name=f"sf2-owned-process-tree-{process.pid}",
            daemon=True,
        )
        self._tracker.start()

    def _process_parents(self) -> dict[int, int]:
        snapshot = self._kernel32.CreateToolhelp32Snapshot(_TH32CS_SNAPPROCESS, 0)
        if snapshot == wintypes.HANDLE(-1).value:
            raise ctypes.WinError(ctypes.get_last_error())
        parents: dict[int, int] = {}
        try:
            entry = _ProcessEntry32W()
            entry.dwSize = ctypes.sizeof(entry)
            available = self._kernel32.Process32FirstW(snapshot, ctypes.byref(entry))
            while available:
                parents[int(entry.th32ProcessID)] = int(entry.th32ParentProcessID)
                available = self._kernel32.Process32NextW(snapshot, ctypes.byref(entry))
        finally:
            self._kernel32.CloseHandle(snapshot)
        return parents

    def _discover_descendants(self) -> int:
        parents = self._process_parents()
        owned = {self._root_pid, *self._descendant_handles}
        discovered: set[int] = set()
        changed = True
        while changed:
            changed = False
            for process_id, parent_id in parents.items():
                if process_id not in owned and parent_id in owned:
                    owned.add(process_id)
                    discovered.add(process_id)
                    changed = True
        opened = 0
        for process_id in sorted(discovered):
            handle = self._kernel32.OpenProcess(
                _PROCESS_TERMINATE | _PROCESS_QUERY_LIMITED_INFORMATION | _SYNCHRONIZE,
                False,
                process_id,
            )
            if handle:
                self._descendant_handles[process_id] = handle
                opened += 1
        return opened

    def _track_descendants(self) -> None:
        while not self._tracker_stop.wait(0.02):
            try:
                self._discover_descendants()
            except OSError:
                self._tracker_failed = True
                return

    def _active_processes(self) -> int:
        accounting = _JobObjectBasicAccountingInformation()
        returned_length = wintypes.DWORD()
        if not self._kernel32.QueryInformationJobObject(
            self._handle,
            _JOB_OBJECT_BASIC_ACCOUNTING_INFORMATION_CLASS,
            ctypes.byref(accounting),
            ctypes.sizeof(accounting),
            ctypes.byref(returned_length),
        ):
            raise ctypes.WinError(ctypes.get_last_error())
        return int(accounting.ActiveProcesses)

    def cleanup(self, process: subprocess.Popen[str], timeout: int, reap_timeout: int) -> str:
        try:
            self._tracker_stop.set()
            if self._tracker is not None:
                self._tracker.join(timeout=timeout)
                if self._tracker.is_alive():
                    return "survivor"
            self._discover_descendants()
            if self._active_processes() > 0 and not self._kernel32.TerminateJobObject(
                self._handle, 1
            ):
                return "survivor"
            deadline = time.monotonic() + timeout
            stable_empty_scans = 0
            while time.monotonic() < deadline:
                opened = self._discover_descendants()
                alive_handles = []
                for handle in self._descendant_handles.values():
                    result = self._kernel32.WaitForSingleObject(handle, 0)
                    if result == _WAIT_TIMEOUT:
                        alive_handles.append(handle)
                    elif result != _WAIT_OBJECT_0:
                        self._tracker_failed = True
                for handle in alive_handles:
                    if (
                        not self._kernel32.TerminateProcess(handle, 1)
                        and self._kernel32.WaitForSingleObject(handle, 0) == _WAIT_TIMEOUT
                    ):
                        self._tracker_failed = True
                tree_empty = self._active_processes() == 0 and not alive_handles
                if tree_empty and opened == 0:
                    stable_empty_scans += 1
                    if stable_empty_scans >= 2:
                        break
                else:
                    stable_empty_scans = 0
                time.sleep(0.02)
            descendants_clean = all(
                self._kernel32.WaitForSingleObject(handle, 0) == _WAIT_OBJECT_0
                for handle in self._descendant_handles.values()
            )
            tree_clean = (
                self._active_processes() == 0
                and descendants_clean
                and not self._tracker_failed
            )
            try:
                process.wait(timeout=reap_timeout)
            except subprocess.TimeoutExpired:
                process.kill()
                try:
                    process.wait(timeout=reap_timeout)
                except subprocess.TimeoutExpired:
                    return "survivor"
            return "clean" if tree_clean and process.poll() is not None else "survivor"
        except OSError:
            return "survivor"

    def close(self) -> None:
        self._tracker_stop.set()
        if self._tracker is not None:
            self._tracker.join(timeout=1)
        for handle in self._descendant_handles.values():
            self._kernel32.CloseHandle(handle)
        self._descendant_handles.clear()
        if self._handle:
            self._kernel32.CloseHandle(self._handle)
            self._handle = None


class _PosixProcessGroup:
    """Launch-owned POSIX process group used by portable unit tests."""

    def __init__(self, process: subprocess.Popen[str]) -> None:
        self._process_group = process.pid

    def _exists(self) -> bool:
        try:
            os.killpg(self._process_group, 0)
        except ProcessLookupError:
            return False
        except PermissionError:
            return True
        return True

    def resume(self, process: subprocess.Popen[str]) -> None:
        del process

    def cleanup(self, process: subprocess.Popen[str], timeout: int, reap_timeout: int) -> str:
        if self._exists():
            with suppress(ProcessLookupError):
                os.killpg(self._process_group, signal.SIGTERM)
        try:
            process.wait(timeout=reap_timeout)
        except subprocess.TimeoutExpired:
            if self._exists():
                with suppress(ProcessLookupError):
                    os.killpg(self._process_group, signal.SIGKILL)
            try:
                process.wait(timeout=reap_timeout)
            except subprocess.TimeoutExpired:
                return "survivor"
        deadline = time.monotonic() + timeout
        while self._exists() and time.monotonic() < deadline:
            time.sleep(0.02)
        return "clean" if not self._exists() and process.poll() is not None else "survivor"

    def close(self) -> None:
        return


def _owned_process_tree(
    process: subprocess.Popen[str],
) -> _WindowsJobObject | _PosixProcessGroup:
    tree = _WindowsJobObject(process) if os.name == "nt" else _PosixProcessGroup(process)
    try:
        tree.resume(process)
    except Exception:
        tree.close()
        raise
    return tree


def run_bounded_process(
    step: str,
    command: Iterable[str | Path],
    *,
    cwd: Path,
    environment: Mapping[str, str],
    timeout: int,
    termination_timeout: int,
    reap_timeout: int,
) -> ProcessReceipt:
    """Run one native command with finite timeout and process-tree cleanup."""

    arguments = tuple(str(item) for item in command)
    creationflags = 0
    if os.name == "nt":
        creationflags = (
            getattr(subprocess, "CREATE_NEW_PROCESS_GROUP", 0)
            | getattr(subprocess, "CREATE_SUSPENDED", 0x00000004)
        )
    process = subprocess.Popen(
        arguments,
        cwd=cwd,
        env=dict(environment),
        stdin=subprocess.DEVNULL,
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
        text=True,
        encoding="utf-8",
        errors="replace",
        creationflags=creationflags,
        start_new_session=os.name != "nt",
    )
    try:
        process_tree = _owned_process_tree(process)
    except Exception:
        process.kill()
        process.wait(timeout=reap_timeout)
        raise
    timed_out = False
    cleanup_status = "clean"
    try:
        try:
            stdout, stderr = process.communicate(timeout=timeout)
        except subprocess.TimeoutExpired as error:
            timed_out = True
            stdout = error.stdout or ""
            stderr = error.stderr or ""
        except BaseException:
            process_tree.cleanup(process, termination_timeout, reap_timeout)
            raise
        cleanup_status = process_tree.cleanup(
            process,
            termination_timeout,
            reap_timeout,
        )
        if timed_out:
            try:
                stdout, stderr = process.communicate(timeout=reap_timeout)
            except subprocess.TimeoutExpired:
                cleanup_status = "survivor"
                if process.stdout is not None:
                    process.stdout.close()
                if process.stderr is not None:
                    process.stderr.close()
    finally:
        process_tree.close()
    if isinstance(stdout, bytes):
        stdout = stdout.decode("utf-8", errors="replace")
    if isinstance(stderr, bytes):
        stderr = stderr.decode("utf-8", errors="replace")
    return ProcessReceipt(
        step,
        arguments,
        process.poll(),
        timed_out,
        cleanup_status,
        _bounded_tail(stdout),
        _bounded_tail(stderr),
    )


def _tracked_remake_files() -> tuple[Path, ...]:
    completed = subprocess.run(
        ["git", "ls-files", "-z", "--", "remake"],
        cwd=REPO_ROOT,
        check=True,
        capture_output=True,
    )
    return tuple(
        REPO_ROOT / Path(value.decode("utf-8"))
        for value in completed.stdout.split(b"\0")
        if value
    )


def _copy_tracked_remake(destination_root: Path) -> Path:
    workspace = destination_root / "workspace"
    if workspace.exists():
        raise ValueError(f"workspace destination already exists: {workspace}")
    workspace.mkdir(parents=True)
    tracked_files = _tracked_remake_files()
    if not tracked_files:
        raise ValueError("tracked remake project is empty")
    for source in tracked_files:
        relative = source.relative_to(REPO_ROOT)
        target = workspace / relative
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(source, target)
    return workspace


def _validate_scratch_parent(path: Path) -> Path:
    resolved = path.resolve()
    if resolved == REPO_ROOT or resolved == Path(resolved.anchor):
        raise ValueError("scratch parent cannot be a repository or filesystem root")
    if resolved.is_relative_to(REPO_ROOT) and not resolved.is_relative_to(repo_path("local")):
        raise ValueError("repository-local Godot scratch must stay under ignored local/")
    resolved.mkdir(parents=True, exist_ok=True)
    return resolved


def _new_scratch(scratch_parent: Path) -> Path:
    parent = _validate_scratch_parent(scratch_parent)
    scratch = parent / f"public-synthetic-{uuid.uuid4().hex}"
    scratch.mkdir()
    return scratch


def _gate_environment(scratch: Path) -> dict[str, str]:
    environment = dict(os.environ)
    environment.update(
        {
            "APPDATA": str(scratch / "appdata"),
            "LOCALAPPDATA": str(scratch / "localappdata"),
            "DOTNET_CLI_HOME": str(scratch / "dotnet-home"),
            "DOTNET_CLI_TELEMETRY_OPTOUT": "1",
            "DOTNET_CLI_UI_LANGUAGE": "en-US",
            "DOTNET_CLI_USE_MSBUILD_SERVER": "false",
            "DOTNET_NOLOGO": "1",
            "GODOT_SILENCE_ROOT_WARNING": "1",
            "MSBUILDDISABLENODEREUSE": "1",
            "MSBUILDUSESERVER": "0",
            "UseSharedCompilation": "false",
        }
    )
    return environment


def _find_editor(editor_root: Path) -> Path:
    matches = sorted(editor_root.rglob("Godot_v4.7.2-stable_mono_win64.exe"))
    if len(matches) != 1:
        raise ValueError("editor archive must contain exactly one locked Windows .NET executable")
    return matches[0]


def _write_receipt(path: Path, receipt: Mapping[str, object]) -> None:
    path.write_text(
        json.dumps(receipt, ensure_ascii=False, indent=2) + "\n",
        encoding="utf-8",
    )


def _require_passed(result: ProcessReceipt) -> None:
    if not result.passed:
        raise RuntimeError(
            f"Godot gate step {result.step} failed: exit={result.exit_code}, "
            f"timedOut={result.timed_out}, cleanup={result.cleanup_status}"
        )


def _scan_export(export_root: Path) -> dict[str, object]:
    files = sorted(path for path in export_root.rglob("*") if path.is_file())
    if not files:
        raise RuntimeError("Godot export produced no files")
    forbidden_suffixes = {".bin", ".rom", ".srm", ".sav", ".state", ".trace"}
    required_names = {
        "sf2-map3-public-synthetic.exe",
        "sf2-map3-public-synthetic.pck",
        "Sf2.Remake.Application.dll",
        "Sf2.Remake.Content.dll",
        "Sf2.Remake.Domain.dll",
        "Sf2.Remake.Godot.dll",
    }
    required_rows: dict[str, dict[str, object]] = {}
    manifest_digest = hashlib.sha256()
    total_size = 0
    for path in files:
        relative = path.relative_to(export_root).as_posix()
        lowered = relative.lower()
        if path.suffix.lower() in forbidden_suffixes or "local/" in lowered:
            raise RuntimeError(
                f"public-synthetic export contains a forbidden payload path: {relative}"
            )
        size = path.stat().st_size
        digest = _sha256(path)
        total_size += size
        manifest_digest.update(f"{relative}\0{size}\0{digest}\n".encode())
        if path.name in required_names:
            if path.name in required_rows:
                raise RuntimeError(f"Godot export contains duplicate required output: {path.name}")
            required_rows[path.name] = {
                "path": relative,
                "size": size,
                "sha256": digest,
            }
    missing = sorted(required_names - required_rows.keys())
    if missing:
        raise RuntimeError(f"Godot export is missing required output(s): {', '.join(missing)}")
    return {
        "fileCount": len(files),
        "totalSize": total_size,
        "manifestSha256": manifest_digest.hexdigest(),
        "requiredFiles": [required_rows[name] for name in sorted(required_rows)],
    }


def _parse_smoke_receipt(output: str) -> dict[str, object]:
    lines = [line for line in output.splitlines() if SMOKE_MARKER in line]
    if len(lines) != 1:
        raise RuntimeError("Godot smoke must emit exactly one typed SF2_MAP3_SMOKE receipt")
    payload = lines[0].split(SMOKE_MARKER, 1)[1]
    try:
        receipt = json.loads(payload)
    except json.JSONDecodeError as error:
        raise RuntimeError("Godot smoke receipt is not valid JSON") from error
    if not isinstance(receipt, dict):
        raise RuntimeError("Godot smoke receipt must be a JSON object")
    expected = {
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
    if receipt != expected:
        raise RuntimeError(
            "Godot smoke receipt does not match the closed public-synthetic contract"
        )
    return receipt


def _require_clean_export_output(result: ProcessReceipt) -> None:
    combined = result.stdout_tail + "\n" + result.stderr_tail
    if "ERROR:" in combined or "completed with warnings" in combined.lower():
        raise RuntimeError("Godot export reported an error or warning despite a zero exit code")


def verify_remake_godot(
    *,
    toolchain_root: Path = DEFAULT_TOOLCHAIN_ROOT,
    project_path: Path = DEFAULT_PROJECT,
    scratch_parent: Path = DEFAULT_SCRATCH_PARENT,
    manifest_path: Path = DEFAULT_MANIFEST,
    process_runner: Callable[..., ProcessReceipt] = run_bounded_process,
) -> dict[str, object]:
    """Run import, smoke, and export against only tracked public-synthetic inputs."""

    toolchain = load_toolchain_manifest(manifest_path)
    editor_archive = toolchain_root / toolchain.editor.file_name
    template_archive = toolchain_root / toolchain.export_templates.file_name
    artifacts = [
        verify_artifact(editor_archive, toolchain.editor),
        verify_artifact(template_archive, toolchain.export_templates),
    ]
    resolved_project = project_path.resolve()
    if not resolved_project.is_relative_to(REPO_ROOT) or not resolved_project.is_dir():
        raise ValueError("Godot project must be an existing path inside this repository")
    project_relative = resolved_project.relative_to(REPO_ROOT)
    if project_relative.as_posix() != "remake/game":
        raise ValueError("the maintained Godot gate owns exactly remake/game")

    scratch = _new_scratch(scratch_parent)
    receipt_path = scratch / "receipt.json"
    steps: list[ProcessReceipt] = []
    receipt: dict[str, object] = {
        "schemaVersion": 1,
        "profile": "public-synthetic",
        "status": "Fail",
        "godotVersion": toolchain.version_output,
        "artifacts": artifacts,
        "steps": [],
        "exportFiles": {},
        "cleanupStatus": "NotRun",
        "scratch": display_path(scratch),
    }
    try:
        editor_root = scratch / "editor"
        extract_zip_members(editor_archive, editor_root)
        editor = _find_editor(editor_root)

        template_root = (
            scratch
            / "appdata"
            / "Godot"
            / "export_templates"
            / toolchain.template_directory
        )
        extracted_templates = extract_zip_members(
            template_archive,
            template_root,
            members=toolchain.export_template_members,
            strip_prefix="templates",
        )
        version_file = template_root / "version.txt"
        if version_file.read_text(encoding="utf-8").strip() != toolchain.template_directory:
            raise RuntimeError("export template version.txt does not match the locked directory")
        if len(extracted_templates) != len(toolchain.export_template_members):
            raise RuntimeError("export template extraction did not close the locked member set")

        workspace = _copy_tracked_remake(scratch)
        scratch_project = workspace / project_relative
        export_root = scratch / "export"
        export_root.mkdir()
        export_path = export_root / "sf2-map3-public-synthetic.exe"
        environment = _gate_environment(scratch)
        def run(step: str, arguments: Iterable[str | Path], timeout: int) -> ProcessReceipt:
            result = process_runner(
                step,
                arguments,
                cwd=scratch_project,
                environment=environment,
                timeout=timeout,
                termination_timeout=toolchain.timeouts.termination,
                reap_timeout=toolchain.timeouts.reap,
            )
            steps.append(result)
            _require_passed(result)
            return result

        version = run("version", [editor, "--version"], toolchain.timeouts.version)
        if version.stdout_tail.strip() != toolchain.version_output:
            raise RuntimeError(
                "Godot version mismatch after extraction: "
                f"expected {toolchain.version_output}, got {version.stdout_tail.strip()}"
            )
        solution = workspace / "remake" / "Sf2.Remake.sln"
        run(
            "restore",
            [
                "dotnet",
                "restore",
                solution,
                "--locked-mode",
                "--disable-build-servers",
            ],
            toolchain.timeouts.restore,
        )
        run(
            "build",
            [
                "dotnet",
                "build",
                solution,
                "--configuration",
                "Debug",
                "--no-restore",
                "--disable-build-servers",
            ],
            toolchain.timeouts.build,
        )
        run(
            "import",
            [editor, "--headless", "--path", scratch_project, "--editor", "--quit-after", "1"],
            toolchain.timeouts.import_project,
        )
        smoke = run(
            "run",
            [editor, "--headless", "--path", scratch_project, "--", "--map3-smoke"],
            toolchain.timeouts.run,
        )
        smoke_receipt = _parse_smoke_receipt(smoke.stdout_tail)
        export_result = run(
            "export",
            [
                editor,
                "--headless",
                "--path",
                scratch_project,
                "--export-release",
                toolchain.public_export_preset,
                export_path,
            ],
            toolchain.timeouts.export,
        )
        _require_clean_export_output(export_result)
        export_files = _scan_export(export_root)
        exported_smoke = run(
            "export-run",
            [export_path, "--headless", "--", "--map3-smoke"],
            toolchain.timeouts.run,
        )
        export_smoke_receipt = _parse_smoke_receipt(exported_smoke.stdout_tail)
        receipt.update(
            {
                "status": "Pass",
                "steps": [step.as_dict() for step in steps],
                "smokeReceipt": smoke_receipt,
                "exportSmokeReceipt": export_smoke_receipt,
                "exportFiles": export_files,
                "cleanupStatus": "clean",
            }
        )
        _write_receipt(receipt_path, receipt)
        return receipt
    except Exception:
        cleanup_status = (
            "clean" if all(step.cleanup_status == "clean" for step in steps) else "survivor"
        )
        receipt.update(
            {
                "steps": [step.as_dict() for step in steps],
                "cleanupStatus": cleanup_status,
            }
        )
        _write_receipt(receipt_path, receipt)
        raise


def main(argv: list[str] | None = None) -> int:
    """Parse the dedicated non-evidence gate command line."""

    parser = argparse.ArgumentParser(
        description="Verify the public-synthetic remake through official Godot CLI gates."
    )
    parser.add_argument(
        "--toolchain-root",
        type=Path,
        default=DEFAULT_TOOLCHAIN_ROOT,
        help="directory containing the two hash-locked official Godot archives",
    )
    parser.add_argument(
        "--project-path",
        type=Path,
        default=DEFAULT_PROJECT,
        help="tracked Godot project (maintained value: remake/game)",
    )
    parser.add_argument(
        "--scratch-parent",
        type=Path,
        default=DEFAULT_SCRATCH_PARENT,
        help="ignored or ephemeral parent for a fresh gate output directory",
    )
    parser.add_argument(
        "--manifest-path",
        type=Path,
        default=DEFAULT_MANIFEST,
        help="tracked Godot toolchain identity manifest",
    )
    arguments = parser.parse_args(argv)
    receipt = verify_remake_godot(
        toolchain_root=arguments.toolchain_root,
        project_path=arguments.project_path,
        scratch_parent=arguments.scratch_parent,
        manifest_path=arguments.manifest_path,
    )
    print(json.dumps(receipt, ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
