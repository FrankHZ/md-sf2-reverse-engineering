"""Bounded localhost/Lua experiment; deliberately outside the sf2 CLI and H3 rails."""

from __future__ import annotations

import argparse
import hashlib
import json
import os
import secrets
import shutil
import socket
import subprocess
import time
from pathlib import Path
from typing import Any

from sf2tool.h3.bizhawk import validate_lua_syntax
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
        "watch": 2,
        "run": 1,
        "clear": 0,
        "quit": 0,
    }
    if operation not in arities or len(arguments) != arities[operation]:
        raise ValueError("unknown command or wrong argument count")
    if type(sequence) is not int or not 1 <= sequence <= 1000000:
        raise ValueError("command sequence out of range")
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

    def __enter__(self) -> DebugBridge:
        self.output.mkdir(parents=True, exist_ok=False)
        self.created = True
        self._save()
        return self

    def _save(self) -> None:
        (self.output / "receipt.json").write_text(
            json.dumps(self.receipt, indent=2) + "\n", encoding="utf-8"
        )

    def start(self) -> dict[str, Any]:
        if self.listener is not None:
            raise RuntimeError("bridge already started")
        toolchain = load_json(repo_path("manifests/toolchain.json"))["bizhawk"]
        executable = _local_destination(repo_path(toolchain["localExecutablePath"]))
        if (
            executable.stat().st_size != toolchain["executableSizeBytes"]
            or hashlib.sha256(executable.read_bytes()).hexdigest().upper()
            != toolchain["executableSha256"]
        ):
            raise ValueError("BizHawk executable identity mismatch")
        rom = private_input_path(ROM_INPUT_IDENTITY)
        verify_rom(rom)
        shutil.copyfile(rom, self.output / "input.bin")
        validate_lua_syntax(SCRIPT, executable)
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
        config_path = self.output / "config.json"
        config_path.write_text(json.dumps(config), encoding="utf-8")
        token = secrets.token_hex(16)
        environment = {
            **os.environ,
            "SF2_BRIDGE_TOKEN": token,
            "SF2_BRIDGE_JSON": str(repo_path("tools/bizhawk/json.lua")),
            "SF2_BRIDGE_STATUS": str(self.output / "lua-status.json"),
        }
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
        self.process = subprocess.Popen(
            [
                str(executable),
                "--gdi",
                f"--config={config_path}",
                f"--lua={SCRIPT}",
                "--socket-ip=127.0.0.1",
                f"--socket-port={port}",
                str(self.output / "input.bin"),
            ],
            cwd=executable.parent,
            env=environment,
            stdin=subprocess.DEVNULL,
            stdout=self.log,
            stderr=self.log,
            startupinfo=startup,
        )
        self.receipt.update(started=True, pid=self.process.pid, port=port)
        self._save()
        self.connection, address = self.listener.accept()
        self.receipt["connected"] = True
        self._save()
        self.listener.close()
        if address[0] != "127.0.0.1":
            raise ValueError("non-loopback peer")
        hello = json.loads(receive_frame(self.connection, timeout=self.timeout))
        if hello.get("protocol") != 1 or hello.pop("token", None) != token:
            raise ValueError("bridge handshake identity mismatch")
        self.receipt.update(hello=hello, startupSeconds=time.monotonic() - started_at)
        self._save()
        return hello

    def command(self, operation: str, *arguments: str | int) -> dict[str, Any]:
        if self.connection is None:
            raise RuntimeError("bridge is not connected")
        wire = command_text(self.sequence + 1, operation, *arguments)
        self.sequence += 1
        record: dict[str, Any] = {"request": wire}
        self.receipt["commands"].append(record)
        self._save()
        started_at = time.monotonic()
        try:
            send_frame(self.connection, wire, timeout=self.timeout)
            response = json.loads(receive_frame(self.connection, timeout=self.timeout))
            if (
                not isinstance(response, dict)
                or response.get("id") != self.sequence
                or type(response.get("ok")) is not bool
            ):
                raise ValueError("bridge response envelope mismatch")
        except (OSError, EOFError, ValueError):
            self.disconnect()
            raise
        record.update(response=response, seconds=time.monotonic() - started_at)
        self._save()
        if not response["ok"]:
            raise ValueError(f"bridge command rejected: {response.get('error')}")
        return response["result"]

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
        if self.log is not None:
            self.log.close()
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
