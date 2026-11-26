"""Run a native Windows debug process on an unshown desktop."""

import ctypes
import os
import subprocess
from ctypes import wintypes


def run_background(command, cwd, environment, stdout, stderr):
    import msvcrt

    class StartupInfo(ctypes.Structure):
        _fields_ = [
            ("cb", wintypes.DWORD),
            ("reserved", wintypes.LPWSTR),
            ("desktop", wintypes.LPWSTR),
            ("title", wintypes.LPWSTR),
            ("x", wintypes.DWORD),
            ("y", wintypes.DWORD),
            ("width", wintypes.DWORD),
            ("height", wintypes.DWORD),
            ("charsX", wintypes.DWORD),
            ("charsY", wintypes.DWORD),
            ("fill", wintypes.DWORD),
            ("flags", wintypes.DWORD),
            ("show", wintypes.WORD),
            ("reservedSize", wintypes.WORD),
            ("reservedBytes", ctypes.POINTER(ctypes.c_byte)),
            ("stdin", wintypes.HANDLE),
            ("stdout", wintypes.HANDLE),
            ("stderr", wintypes.HANDLE),
        ]

    class ProcessInfo(ctypes.Structure):
        _fields_ = [
            ("process", wintypes.HANDLE),
            ("thread", wintypes.HANDLE),
            ("pid", wintypes.DWORD),
            ("threadId", wintypes.DWORD),
        ]

    user = ctypes.WinDLL("user32", use_last_error=True)
    kernel = ctypes.WinDLL("kernel32", use_last_error=True)
    user.CreateDesktopW.argtypes = [
        wintypes.LPCWSTR,
        ctypes.c_void_p,
        ctypes.c_void_p,
        wintypes.DWORD,
        wintypes.DWORD,
        ctypes.c_void_p,
    ]
    user.CreateDesktopW.restype = wintypes.HANDLE
    user.CloseDesktop.argtypes = [wintypes.HANDLE]
    kernel.CreateProcessW.argtypes = [
        wintypes.LPCWSTR,
        wintypes.LPWSTR,
        ctypes.c_void_p,
        ctypes.c_void_p,
        wintypes.BOOL,
        wintypes.DWORD,
        ctypes.c_void_p,
        wintypes.LPCWSTR,
        ctypes.POINTER(StartupInfo),
        ctypes.POINTER(ProcessInfo),
    ]
    kernel.CreateProcessW.restype = wintypes.BOOL
    kernel.WaitForSingleObject.argtypes = [wintypes.HANDLE, wintypes.DWORD]
    kernel.WaitForSingleObject.restype = wintypes.DWORD
    kernel.GetExitCodeProcess.argtypes = [wintypes.HANDLE, ctypes.POINTER(wintypes.DWORD)]
    kernel.CloseHandle.argtypes = [wintypes.HANDLE]
    name = f"sf2-debug-{os.getpid()}"
    desktop = user.CreateDesktopW(name, None, None, 0, 0x01FF, None)
    if not desktop:
        raise ctypes.WinError(ctypes.get_last_error())
    process = ProcessInfo()
    try:
        with open(os.devnull, "rb") as stdin:
            handles = [msvcrt.get_osfhandle(file.fileno()) for file in (stdin, stdout, stderr)]
            for handle in handles:
                os.set_handle_inheritable(handle, True)
            try:
                startup = StartupInfo(
                    cb=ctypes.sizeof(StartupInfo),
                    desktop=name,
                    flags=0x100,
                    stdin=handles[0],
                    stdout=handles[1],
                    stderr=handles[2],
                )
                block = ctypes.create_unicode_buffer(
                    "\0".join(f"{key}={value}" for key, value in sorted(environment.items()))
                    + "\0\0"
                )
                line = ctypes.create_unicode_buffer(subprocess.list2cmdline(command))
                if not kernel.CreateProcessW(
                    command[0],
                    line,
                    None,
                    None,
                    True,
                    0x400 | 0x08000000,
                    block,
                    str(cwd),
                    ctypes.byref(startup),
                    ctypes.byref(process),
                ):
                    raise ctypes.WinError(ctypes.get_last_error())
            finally:
                for handle in handles:
                    os.set_handle_inheritable(handle, False)
        # Finite waits return to Python so Ctrl+C can stop our hidden process tree.
        while True:
            status = kernel.WaitForSingleObject(process.process, 100)
            if status == 0:
                break
            if status != 258:  # WAIT_TIMEOUT
                raise ctypes.WinError(ctypes.get_last_error())
        code = wintypes.DWORD()
        if not kernel.GetExitCodeProcess(process.process, ctypes.byref(code)):
            raise ctypes.WinError(ctypes.get_last_error())
        return code.value
    except BaseException:
        if process.process and kernel.WaitForSingleObject(process.process, 0) != 0:
            subprocess.run(
                ["taskkill", "/PID", str(process.pid), "/T", "/F"],
                stdout=subprocess.DEVNULL,
                stderr=subprocess.DEVNULL,
                creationflags=subprocess.CREATE_NO_WINDOW,
                check=False,
            )
        raise
    finally:
        if process.thread:
            kernel.CloseHandle(process.thread)
        if process.process:
            kernel.CloseHandle(process.process)
        user.CloseDesktop(desktop)
