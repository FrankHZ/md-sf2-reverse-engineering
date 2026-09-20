"""Bounded localhost/Lua experiment; deliberately outside the sf2 CLI and H3 rails."""

from __future__ import annotations

import argparse
import hashlib
import json
import math
import os
import queue
import secrets
import shutil
import socket
import subprocess
import sys
import threading
import time
from contextlib import suppress
from pathlib import Path
from typing import Any

from sf2tool.h3.bizhawk import bizhawk_contract, materialize_bizhawk_launch, validate_lua_syntax
from sf2tool.jsonio import load_json
from sf2tool.paths import repo_path
from sf2tool.private_inputs import ROM_INPUT_IDENTITY, private_input_path
from sf2tool.rom import verify_rom

MAX_PAYLOAD = 65536
SCRIPT = repo_path("tools/debug_bridge.lua")


def send_frame(connection: socket.socket, payload: str, *, timeout: float) -> None:
    data = payload.encode("utf-8")
    if not 1 <= len(data) <= MAX_PAYLOAD:
        raise ValueError("payload length out of range")
    connection.settimeout(timeout)
    connection.sendall(str(len(data)).encode("ascii") + b" " + data)


def receive_frame(connection: socket.socket, *, timeout: float) -> str:
    """One absolute deadline, strict byte length, no assumptions about TCP chunks."""
    deadline = time.monotonic() + timeout

    def read(count: int) -> bytes:
        remaining = deadline - time.monotonic()
        if remaining <= 0:
            raise TimeoutError("bridge receive deadline exceeded")
        connection.settimeout(remaining)
        chunk = connection.recv(count)
        if not chunk:
            raise EOFError("bridge peer closed an incomplete message")
        return chunk

    prefix = bytearray()
    while True:
        byte = read(1)
        if byte == b" ":
            break
        if not b"0" <= byte <= b"9" or len(prefix) >= 5:
            raise ValueError("invalid bridge length prefix")
        prefix.extend(byte)
    if not prefix or prefix.startswith(b"0") or not 1 <= int(prefix) <= MAX_PAYLOAD:
        raise ValueError("bridge payload length out of range")
    payload = bytearray()
    size = int(prefix)
    while len(payload) < size:
        payload.extend(read(size - len(payload)))
    return payload.decode("utf-8", errors="strict")


def command_text(sequence: int, operation: str, *arguments: str | int) -> str:
    arities = {
        "ping": 0,
        "state": 0,
        "read": 3,
        "advance": 1,
        "step": 2,
        "abort": 0,
        "save": 0,
        "watch": 2,
        "run": 1,
        "clear": 0,
        "quit": 0,
    }
    if operation not in arities or len(arguments) != arities[operation]:
        raise ValueError("unknown command or wrong argument count")
    if type(sequence) is not int or not 1 <= sequence <= 1000000:
        raise ValueError("command sequence out of range")
    if operation == "step":
        count, button = arguments
        if type(count) is not int or not 1 <= count <= 120:
            raise ValueError("step requires 1..120 frames")
        if button not in ("neutral", "Up", "Down", "Left", "Right", "A", "B", "C"):
            raise ValueError("unsupported step button")
    parts = [str(sequence), operation, *(str(arg) for arg in arguments)]
    if any(not part or any(ord(c) < 32 or ord(c) > 126 for c in part) for part in parts):
        raise ValueError("command fields must be nonempty printable ASCII")
    result = "\t".join(parts)
    if len(result) > 512:
        raise ValueError("command exceeds 512 bytes")
    return result


def _local_destination(path: Path) -> Path:
    resolved = path.resolve()
    local = repo_path("local")
    if not resolved.is_relative_to(local) or resolved == local:
        raise ValueError("bridge writes require a destination beneath this worktree's local/")
    return resolved


class DebugBridge:
    """Own exactly one newly launched process and one loopback connection.

    Call start() inside a with block. An explicit new output directory preserves
    each launch, including failures. Raw receipts are private, ignored local data.
    """

    def __init__(self, output: Path, *, timeout: float = 10):
        if not 0 < timeout <= 60:
            raise ValueError("timeout must be in (0, 60] seconds")
        self.output = _local_destination(output)
        self.timeout = timeout
        self.connection: socket.socket | None = None
        self.listener: socket.socket | None = None
        self.process: subprocess.Popen | None = None
        self.sequence = 0
        self.created = False
        self.receipt: dict[str, Any] = {"started": False, "commands": [], "outcome": "running"}
        self.log = None
        self.deadline: float | None = None
        self.watchdog: threading.Timer | None = None
        self.expired = threading.Event()
        self.started_at: float | None = None
        self.idle_deadline: float | None = None
        self.idle_watchdog: threading.Timer | None = None
        self.acquisition_limits: dict[str, int] | None = None

    def __enter__(self) -> DebugBridge:
        self.output.mkdir(parents=True, exist_ok=False)
        self.created = True
        self._save()
        return self

    def _save(self) -> None:
        (self.output / "receipt.json").write_text(
            json.dumps(self.receipt, indent=2) + "\n", encoding="utf-8"
        )

    def remaining(self) -> float:
        remaining = (
            self.deadline - time.monotonic()
            if self.deadline
            else (
                self.acquisition_limits["idleSeconds"] if self.acquisition_limits else self.timeout
            )
        )
        if self.idle_deadline is not None:
            remaining = min(remaining, self.idle_deadline - time.monotonic())
        if remaining <= 0 or self.expired.is_set():
            if self.acquisition_limits:
                reason = self.receipt.get("stopReason") or (
                    "operator-idle-limit"
                    if self.idle_deadline is not None and time.monotonic() >= self.idle_deadline
                    else "wall-limit"
                )
                self.receipt["stopReason"] = reason
                raise TimeoutError(reason)
            raise TimeoutError("interactive wall budget exhausted")
        return remaining

    def _operator_idle(self, *, advancing: bool = False, deadline: float | None = None) -> None:
        if self.idle_watchdog is not None:
            self.idle_watchdog.cancel()
        self.idle_deadline = None
        if self.acquisition_limits is None or advancing:
            return
        self.idle_deadline = (
            deadline
            if deadline is not None
            else (time.monotonic() + self.acquisition_limits["idleSeconds"])
        )

        idle_deadline = self.idle_deadline

        def expire_idle() -> None:
            if self.idle_deadline != idle_deadline:
                return
            self.expired.set()
            self.receipt["stopReason"] = "operator-idle-limit"
            if self.process is not None and self.process.poll() is None:
                self.process.kill()

        self.idle_watchdog = threading.Timer(max(0, idle_deadline - time.monotonic()), expire_idle)
        self.idle_watchdog.daemon = True
        self.idle_watchdog.start()

    def start(
        self,
        *,
        observer: Path = SCRIPT,
        observer_config: Path | None = None,
        rom_path: Path | None = None,
        wall_seconds: int | None = None,
        acquisition_limits: dict[str, int] | None = None,
        prior_active_seconds: float = 0,
        historical_starts: int | None = None,
        expected_settings: str | None = None,
    ) -> dict[str, Any]:
        if self.listener is not None:
            raise RuntimeError("bridge already started")
        if acquisition_limits is not None:
            from sf2tool.h3.map3_messenger_acceptance import (
                NATURAL_CONTINUATION,
                _interactive_limits,
            )

            if (
                acquisition_limits != _interactive_limits(NATURAL_CONTINUATION)
                or wall_seconds != acquisition_limits["wallSeconds"]
                or observer_config is None
                or observer != repo_path("tools/bizhawk/map3_messenger_acceptance_observer.lua")
            ):
                raise ValueError("natural acquisition bounds/composition mismatch")
            self.acquisition_limits = dict(acquisition_limits)
        if (
            not math.isfinite(prior_active_seconds)
            or prior_active_seconds < 0
            or (not self.acquisition_limits and prior_active_seconds >= (wall_seconds or 1))
        ):
            raise ValueError("cumulative active wall budget exhausted")
        if prior_active_seconds and not self.acquisition_limits:
            raise ValueError("continuation accounting requires natural acquisition")
        ceiling = 7200 if self.acquisition_limits else 1800
        if wall_seconds is not None and (
            type(wall_seconds) is not int or not 1 <= wall_seconds <= ceiling
        ):
            raise ValueError("wall budget exceeds selected composition ceiling")
        toolchain, executable = bizhawk_contract()
        if (
            executable.stat().st_size != toolchain["executableSizeBytes"]
            or hashlib.sha256(executable.read_bytes()).hexdigest().upper()
            != toolchain["executableSha256"]
        ):
            raise ValueError("BizHawk executable identity mismatch")
        rom = rom_path or private_input_path(ROM_INPUT_IDENTITY)
        verify_rom(rom)
        if observer_config is None:
            shutil.copyfile(rom, self.output / "input.bin")
            rom = self.output / "input.bin"
        validate_lua_syntax(observer, executable)
        config = {
            "LastWrittenFrom": toolchain["release"],
            "PreferredCores": {"GEN": "Genplus-gx"},
            "SingleInstanceMode": False,
            "FirstBoot": False,
            "SoundEnabled": False,
            "StartPaused": True,
            "UpdateAutoCheckEnabled": False,
            "RACheevosActive": False,
            "UseRecentForRoms": False,
            "AutoLoadLastSaveSlot": False,
            "AutoSaveLastSaveSlot": False,
            "AutosaveSaveRAM": False,
            "BackupSaveram": False,
        }
        launch = materialize_bizhawk_launch(self.output, config=config)
        executable = Path(launch["executable"])
        config_path = Path(launch["config"])
        settings_identity = hashlib.sha256(config_path.read_bytes()).hexdigest().upper()
        if expected_settings is not None and settings_identity != expected_settings:
            raise ValueError("continuation runtime settings identity mismatch")
        self.receipt["runtimeSettingsSha256"] = settings_identity
        token = secrets.token_hex(16)
        environment = {
            **os.environ,
            **launch["environment"],
            "SF2_BRIDGE_TOKEN": token,
            "SF2_BRIDGE_JSON": str(repo_path("tools/bizhawk/json.lua")),
            "SF2_BRIDGE_STATUS": str(self.output / "lua-status.json"),
        }
        if observer_config is not None:
            validate_lua_syntax(observer_config, executable)
            environment["SF2_H3_CONFIG"] = str(observer_config)
        self.receipt["launch"] = launch
        self.listener = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.listener.bind(("127.0.0.1", 0))
        self.listener.listen(1)
        self.listener.settimeout(self.timeout)
        port = self.listener.getsockname()[1]
        self.log = (self.output / "process.log").open("wb")
        startup = None
        if os.name == "nt":
            startup = subprocess.STARTUPINFO()
            startup.dwFlags |= subprocess.STARTF_USESHOWWINDOW
            # WinForms startup diagnostics require a targetable window. Minimize
            # without activating it; SW_HIDE can conceal pre-Lua modal failures.
            startup.wShowWindow = 7
        started_at = time.monotonic()
        self.started_at = started_at
        self.receipt["startedAtUnix"] = time.time()
        if self.acquisition_limits:
            environment["SF2_BRIDGE_LAUNCH_EPOCH"] = str(self.receipt["startedAtUnix"])
        self.process = subprocess.Popen(
            [
                str(executable),
                "--gdi",
                f"--config={config_path}",
                f"--lua={observer}",
                "--socket-ip=127.0.0.1",
                f"--socket-port={port}",
                str(rom),
            ],
            cwd=launch["cwd"],
            env=environment,
            stdin=subprocess.DEVNULL,
            stdout=self.log,
            stderr=self.log,
            startupinfo=startup,
        )
        if wall_seconds is not None and not self.acquisition_limits:
            self.deadline = started_at + wall_seconds - prior_active_seconds

            def expire() -> None:
                self.expired.set()
                self.receipt["stopReason"] = "wall-limit"
                # Retained process handle only; no retry, attach, or name-based kill.
                if self.process is not None and self.process.poll() is None:
                    self.process.kill()

            self.watchdog = threading.Timer(max(0, self.deadline - time.monotonic()), expire)
            self.watchdog.daemon = True
            self.watchdog.start()
        self.receipt.update(started=True, pid=self.process.pid, port=port, wallSeconds=wall_seconds)
        if historical_starts is not None:
            self.receipt["historicalStarts"] = historical_starts + 1
        self._save()
        self.connection, address = self.listener.accept()
        self.receipt["connected"] = True
        self._save()
        self.listener.close()
        if address[0] != "127.0.0.1":
            raise ValueError("non-loopback peer")
        startup_remaining = self.timeout
        if self.acquisition_limits:
            startup_remaining -= time.monotonic() - started_at
            if startup_remaining <= 0:
                raise TimeoutError("startup-exchange-limit")
        hello = json.loads(
            receive_frame(self.connection, timeout=min(startup_remaining, self.remaining()))
        )
        if hello.get("protocol") != 1 or hello.pop("token", None) != token:
            raise ValueError("bridge handshake identity mismatch")
        self.receipt.update(hello=hello, startupSeconds=time.monotonic() - started_at)
        self._save()
        self._operator_idle()
        return hello

    def command(self, operation: str, *arguments: str | int) -> dict[str, Any]:
        if self.connection is None:
            raise RuntimeError("bridge is not connected")
        wire = command_text(self.sequence + 1, operation, *arguments)
        self.remaining()
        previous_idle_deadline = self.idle_deadline
        if self.acquisition_limits and operation == "step":
            self._operator_idle(advancing=True)
        self.sequence += 1
        record: dict[str, Any] = {"request": wire}
        self.receipt["commands"].append(record)
        self._save()
        started_at = time.monotonic()
        try:
            send_frame(self.connection, wire, timeout=min(self.timeout, self.remaining()))
            response = json.loads(
                receive_frame(self.connection, timeout=min(self.timeout, self.remaining()))
            )
            if (
                not isinstance(response, dict)
                or response.get("id") != self.sequence
                or type(response.get("ok")) is not bool
            ):
                raise ValueError("bridge response envelope mismatch")
        except (OSError, EOFError, ValueError):
            self.disconnect()
            raise
        if self.acquisition_limits and operation == "step" and response["ok"]:
            # A typed zero-frame rejection is inspection, not progress or an idle renewal.
            self._operator_idle(
                deadline=(
                    previous_idle_deadline
                    if response.get("result", {}).get("advanced", 0) == 0
                    else None
                )
            )
        record.update(response=response, seconds=time.monotonic() - started_at)
        self._save()
        if not response["ok"]:
            raise ValueError(f"bridge command rejected: {response.get('error')}")
        return response["result"]

    def interact(self) -> None:
        """One stdin request at a time; waiting consumes the owned wall budget.

        JSON arrays such as ["state"] or ["step", 1, "C"]. No gameplay policy.
        The daemon reads only one line per request, so piped input cannot queue
        unbounded commands. EOF, Ctrl-C and invalid commands abort the session.
        """
        while True:
            lines: queue.Queue = queue.Queue(maxsize=1)
            dispatched = False

            def read_line(destination=lines) -> None:
                try:
                    destination.put(sys.stdin.readline(1025))
                except Exception as error:
                    destination.put(error)

            threading.Thread(target=read_line, daemon=True).start()
            try:
                try:
                    line = lines.get(timeout=self.remaining())
                except queue.Empty as error:
                    if self.acquisition_limits:
                        self.remaining()
                    raise TimeoutError("operator wait exceeded wall budget") from error
                if isinstance(line, Exception):
                    raise line
                if not line:
                    raise EOFError("operator disconnected")
                request = json.loads(line)
                if (
                    not isinstance(request, list)
                    or not request
                    or request[0] not in ("state", "ping", "step", "abort", "save")
                ):
                    raise ValueError("expected state, ping, step, save or abort JSON array")
                command_text(self.sequence + 1, *request)
                dispatched = True
                result = self.command(*request)
                print(json.dumps(result), flush=True)
                if result.get("terminal"):
                    return
            except BaseException:
                if not dispatched and self.connection is not None and not self.expired.is_set():
                    with suppress(Exception):
                        self.command("abort")
                raise

    def disconnect(self) -> None:
        """Deliberately exercise abrupt EOF. No reconnect/retry of partial messages."""
        if self.connection is not None:
            self.connection.close()
            self.connection = None

    def __exit__(self, kind, error, traceback) -> None:
        if error is not None:
            self.receipt.update(outcome="failed", error=str(error))
        else:
            self.receipt["outcome"] = "completed"
        self.disconnect()
        if self.listener is not None:
            self.listener.close()
        if self.process is not None:
            try:
                self.process.wait(timeout=3)
                self.receipt["forcedTermination"] = False
            except subprocess.TimeoutExpired:
                # Popen's retained process handle on Windows avoids PID reuse.
                # This executable does not launch child processes in this experiment.
                self.process.kill()
                self.process.wait(timeout=3)
                self.receipt["forcedTermination"] = True
            self.receipt.update(returncode=self.process.returncode, processTerminated=True)
            self.receipt["endedAtUnix"] = time.time()
        if self.log is not None:
            self.log.close()
        if self.watchdog is not None:
            self.watchdog.cancel()
        if self.idle_watchdog is not None:
            self.idle_watchdog.cancel()
        self.receipt["timedOut"] = self.expired.is_set()
        if self.started_at is not None:
            self.receipt["elapsedSeconds"] = time.monotonic() - self.started_at
        if self.expired.is_set() or self.receipt.get("returncode", 0) != 0:
            self.receipt["outcome"] = "failed"
        status_path = self.output / "lua-status.json"
        self.receipt["luaStatus"] = load_json(status_path) if status_path.exists() else None
        self._save()


def experiment(bridge: DebugBridge, mode: str) -> None:
    hello = bridge.start()
    assert hello["system"] == "GEN" and hello["version"] == "2.11.1"
    assert hello["state"]["paused"]
    assert "M68K PC" in hello["state"]["registers"]
    assert hello["domains"]["68K RAM"] == 65536
    assert bridge.command("ping")["frame"] == hello["state"]["frame"]
    if mode == "smoke":
        time.sleep(0.25)
        assert bridge.command("state")["frame"] == hello["state"]["frame"]
        assert len(bridge.command("read", "68K RAM", 0, 16)["bytes"]) == 16
        for args in (("68K RAM", 65535, 2), ("M68K BUS", 0, 1), ("68K RAM", 0, 65)):
            try:
                bridge.command("read", *args)
            except ValueError as failure:
                assert "command rejected" in str(failure)
            else:
                raise AssertionError("unsafe RAM read accepted")
        initial = bridge.command("state")["frame"]
        assert bridge.command("advance", 3)["frame"] == initial + 3
        # Accepted controller fixture supplies VInt; this does not run its H3 scenario.
        fixture = load_json(repo_path("tests/fixtures/h3/controller-input-v1.json"))
        address = fixture["sourceContext"]["vIntEntryAddress"]
        bridge.command("watch", "M68K BUS", address)
        result = bridge.command("run", 120)
        assert result["event"] and result["event"]["address"] == address
        assert not result["state"]["callbackActive"]
        calls = result["state"]["callbackCalls"]
        assert bridge.command("advance", 2)["callbackCalls"] == calls
        bridge.command("watch", "M68K BUS", address)
        assert not bridge.command("clear")["callbackActive"]
        bridge.command("quit")
    elif mode == "disconnect":
        bridge.command("watch", "M68K BUS", 1428)
        bridge.disconnect()
    elif mode == "idle-timeout":
        bridge.command("watch", "M68K BUS", 1428)
        assert bridge.process is not None
        bridge.process.wait(timeout=8)
    else:
        raise ValueError("unknown experiment mode")


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--mode", choices=("smoke", "disconnect", "idle-timeout"), default="smoke")
    arguments = parser.parse_args()
    bridge = DebugBridge(arguments.output)
    try:
        with bridge:
            experiment(bridge, arguments.mode)
        status = bridge.receipt["luaStatus"]
        if arguments.mode == "smoke":
            assert bridge.receipt["returncode"] == 0
            assert status["state"] == "closed" and not status["callbackActive"]
            assert not bridge.receipt["forcedTermination"]
        elif arguments.mode == "idle-timeout":
            assert bridge.receipt["returncode"] == 1
            assert status["state"] == "failed" and not status["callbackActive"]
        assert bridge.receipt["processTerminated"]
    except Exception as failure:
        # Detailed failures, including potentially private paths, stay in local receipts.
        if bridge.created:
            bridge.receipt.update(outcome="failed", error=str(failure))
            bridge._save()
        print(f"FAIL {arguments.mode}: {type(failure).__name__}; inspect local receipt")
        return 1
    print(f"PASS {arguments.mode}; forced termination={bridge.receipt['forcedTermination']}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
