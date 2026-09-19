"""Bounded native process execution with owned process-tree cleanup.

Extracted unchanged from the retired public-synthetic Godot gate. The local presentation
candidate builder uses it for its pinned external rasterizer.
"""

from __future__ import annotations

import ctypes
import os
import signal
import subprocess
import threading
import time
from collections.abc import Iterable, Mapping
from contextlib import suppress
from ctypes import wintypes
from dataclasses import dataclass
from pathlib import Path
from typing import Any

from sf2tool.dotnet_environment import prevent_dotnet_path_changes

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
                    # A prior asynchronous termination can reject this request before
                    # the handle is signaled. The bounded polls and final handle/job
                    # checks determine completion, not an individual request's result.
                    self._kernel32.TerminateProcess(handle, 1)
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
                self._active_processes() == 0 and descendants_clean and not self._tracker_failed
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
        creationflags = getattr(subprocess, "CREATE_NEW_PROCESS_GROUP", 0) | getattr(
            subprocess, "CREATE_SUSPENDED", 0x00000004
        )
    process = subprocess.Popen(
        arguments,
        cwd=cwd,
        env=prevent_dotnet_path_changes(environment),
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
