from __future__ import annotations

import ctypes
import json
import socket
import subprocess
from pathlib import Path

import pytest

import sf2tool.bizhawk_debug_bridge as bridge


class FragmentedPeer:
    def __init__(self, data: bytes):
        self.data = bytearray(data)
        self.timeouts: list[float] = []

    def recv(self, count: int) -> bytes:
        chunk = bytes(self.data[: min(count, 1)])
        del self.data[: len(chunk)]
        return chunk

    def settimeout(self, timeout: float) -> None:
        self.timeouts.append(timeout)


def test_fragmented_utf8_and_coalesced_messages() -> None:
    peer = FragmentedPeer("3 桥2 ok".encode())
    assert bridge.receive_frame(peer, timeout=1) == "桥"
    assert bridge.receive_frame(peer, timeout=1) == "ok"
    assert not peer.data


@pytest.mark.parametrize("wire", [b"", b"1", b"3 ab"])
def test_eof_never_spins(wire: bytes) -> None:
    with pytest.raises(EOFError):
        bridge.receive_frame(FragmentedPeer(wire), timeout=1)


@pytest.mark.parametrize("wire", [b"  ", b"0 ", b"01 a", b"-1 ", b"x ", b"65537 ", b"123456"])
def test_bad_lengths_rejected_before_payload_allocation(wire: bytes) -> None:
    with pytest.raises(ValueError):
        bridge.receive_frame(FragmentedPeer(wire), timeout=1)


def test_invalid_utf8_is_not_silently_replaced() -> None:
    with pytest.raises(UnicodeDecodeError):
        bridge.receive_frame(FragmentedPeer(b"1 \xff"), timeout=1)


def test_deadline_covers_whole_message(monkeypatch) -> None:
    times = iter([0, 0.2, 0.9, 1.1])
    monkeypatch.setattr(bridge.time, "monotonic", lambda: next(times))
    peer = FragmentedPeer(b"2 ok")
    with pytest.raises(TimeoutError, match="deadline"):
        bridge.receive_frame(peer, timeout=1)
    assert peer.timeouts == pytest.approx([0.8, 0.1])


def test_real_socket_send_length_and_idle_timeout() -> None:
    left, right = socket.socketpair()
    with left, right:
        bridge.send_frame(left, "桥", timeout=1)
        assert right.recv(100) == "3 桥".encode()
        with pytest.raises(TimeoutError):
            bridge.receive_frame(left, timeout=0.02)


@pytest.mark.parametrize("payload", ["", "x" * 65537], ids=["empty", "oversize"])
def test_send_length_bound(payload: str) -> None:
    with pytest.raises(ValueError):
        bridge.send_frame(None, payload, timeout=1)


@pytest.mark.parametrize(
    "op,args",
    [
        ("eval", ("print(1)",)),
        ("ping", (1,)),
        ("read", ("68K RAM\t0", 0, 1)),
        ("read", ("68K RAM\nquit", 0, 1)),
    ],
)
def test_command_allowlist_arity_and_delimiter_injection(op, args) -> None:
    with pytest.raises(ValueError):
        bridge.command_text(1, op, *args)


def test_output_cannot_escape_own_ignored_local(monkeypatch, tmp_path: Path) -> None:
    monkeypatch.setattr(bridge, "repo_path", lambda _: tmp_path / "local")
    with pytest.raises(ValueError):
        bridge.DebugBridge(tmp_path / "other")
    with pytest.raises(ValueError):
        bridge.DebugBridge(tmp_path / "local")
    output = tmp_path / "local" / "launch"
    with bridge.DebugBridge(output):
        pass
    before = (output / "receipt.json").read_bytes()
    with pytest.raises(FileExistsError), bridge.DebugBridge(output):
        pass
    assert (output / "receipt.json").read_bytes() == before


def test_main_preserves_existing_launch(monkeypatch, tmp_path: Path) -> None:
    monkeypatch.setattr(bridge, "repo_path", lambda _: tmp_path / "local")
    output = tmp_path / "local" / "launch"
    output.mkdir(parents=True)
    receipt = output / "receipt.json"
    receipt.write_text("preserved failure", encoding="utf-8")
    monkeypatch.setattr("sys.argv", ["bridge", "--output", str(output)])
    assert bridge.main() == 1
    assert receipt.read_text(encoding="utf-8") == "preserved failure"


def test_bad_response_closes_connection(monkeypatch, tmp_path: Path) -> None:
    monkeypatch.setattr(bridge, "repo_path", lambda _: tmp_path / "local")
    left, right = socket.socketpair()
    with right, bridge.DebugBridge(tmp_path / "local" / "launch") as controller:
        controller.connection = left
        bridge.send_frame(right, '{"id":99,"ok":true}', timeout=1)
        with pytest.raises(ValueError, match="envelope"):
            controller.command("ping")
        assert controller.connection is None
        assert left.fileno() == -1


def test_cleanup_only_kills_retained_process_handle(monkeypatch, tmp_path: Path) -> None:
    monkeypatch.setattr(bridge, "repo_path", lambda _: tmp_path / "local")

    class OwnedProcess:
        returncode = None
        killed = False

        def wait(self, timeout):
            if not self.killed:
                raise subprocess.TimeoutExpired("owned", timeout)
            self.returncode = 1

        def kill(self):
            self.killed = True

    process = OwnedProcess()
    with bridge.DebugBridge(tmp_path / "local" / "launch") as controller:
        controller.process = process
    assert process.killed
    assert controller.receipt["forcedTermination"]
    assert controller.receipt["processTerminated"]


def _lua_contract(commands: list[str], *, callback_fault: bool = False) -> dict:
    """Execute the real Lua script with bounded fake emulator APIs, never a game."""
    manifest = bridge.load_json(bridge.repo_path("manifests/toolchain.json"))["bizhawk"]
    dll = bridge.repo_path(manifest["localExecutablePath"]).parent / "dll/lua54.dll"
    if not dll.is_file():
        pytest.skip("requires the local pinned BizHawk Lua DLL; no emulator is launched")
    library = ctypes.CDLL(str(dll))
    pointer = ctypes.c_void_p
    library.luaL_newstate.restype = pointer
    library.luaL_openlibs.argtypes = [pointer]
    library.luaL_loadbufferx.argtypes = [
        pointer,
        ctypes.c_char_p,
        ctypes.c_size_t,
        ctypes.c_char_p,
        ctypes.c_char_p,
    ]
    library.lua_pcallk.argtypes = [
        pointer,
        ctypes.c_int,
        ctypes.c_int,
        ctypes.c_int,
        ctypes.c_ssize_t,
        pointer,
    ]
    library.lua_getglobal.argtypes = [pointer, ctypes.c_char_p]
    library.lua_tolstring.argtypes = [pointer, ctypes.c_int, ctypes.POINTER(ctypes.c_size_t)]
    library.lua_tolstring.restype = pointer
    library.lua_close.argtypes = [pointer]
    script_path = bridge.SCRIPT.as_posix()
    json_path = bridge.repo_path("tools/bizhawk/json.lua").as_posix()
    prefix = f"local wires = {{{','.join(json.dumps(c) for c in commands)}}}\n"
    prefix += f"local real_json = dofile({json.dumps(json_path)})\n"
    prefix += f"local callback_fault = {str(callback_fault).lower()}\n"
    source = (
        prefix
        + r"""
local reports, responses, active, frame, paused, calls = {}, {}, nil, 0, true, 0
local core = {FullName='BizHawk.Emulation.Cores.Consoles.Sega.gpgx.GPGX'}
local function enumeration(value)
  local done = false
  return {GetEnumerator=function() return {Current=value, MoveNext=function()
    if done then return false end; done=true; return true end} end}
end
core.GetCustomAttributes=function() return enumeration({CoreName='Genplus-gx',
  GetType=function() return {Name='PortedCoreAttribute'} end}) end
local application = {OpenForms=enumeration({GetType=function()
  return {FullName='BizHawk.Client.EmuHawk.MainForm'} end,
  Emulator={GetType=function() return core end}})}
luanet={load_assembly=function() end, import_type=function() return application end}
local fake_json={encode=real_json.encode, null=real_json.null,
  write=function(_, value) reports[#reports+1]=value end}
local original_dofile = dofile
dofile=function(_) return fake_json end
os.getenv=function(key) return key end
client={pause=function() paused=true end, unpause=function() paused=false end,
  ispaused=function() return paused end, getversion=function() return '2.11.1' end,
  exitCode=function(code) __result=real_json.encode({exit=code, reports=reports,
    responses=responses, active=active~=nil, frame=frame, readCalls=calls}) end}
emu={getregisters=function() return {['M68K PC']=1428} end,
  getregister=function()
    if callback_fault then error('injected callback failure') end; return 1428 end,
  getsystemid=function() return 'GEN' end, framecount=function() return frame end,
  yield=function() end, frameadvance=function()
    assert(not paused); if active then active(1428,0,16384) end; frame=frame+1 end}
joypad={get=function() return {A=true} end, set=function(buttons) assert(buttons.A==false) end}
memory={getmemorydomainlist=function() return {[0]='68K RAM'} end,
  getmemorydomainsize=function() return 65536 end,
  read_u8=function(address, domain)
    assert(domain=='68K RAM' and address>=0 and address<65536); calls=calls+1; return 0 end}
event={on_bus_exec=function(fn) active=fn; return 'owned-id' end,
  unregisterbyid=function(id) assert(id=='owned-id'); active=nil; return true end}
comm={socketServerSetTimeout=function() end,
  socketServerSend=function(payload) responses[#responses+1]=payload;
    return #tostring(#payload)+1+#payload end,
  socketServerResponse=function() return table.remove(wires,1) or '' end}
"""
    )
    source += f"\noriginal_dofile({json.dumps(script_path)})"
    data = source.encode("utf-8")
    state = library.luaL_newstate()
    assert state
    try:
        library.luaL_openlibs(state)
        assert library.luaL_loadbufferx(state, data, len(data), b"@bridge-test", b"t") == 0
        assert library.lua_pcallk(state, 0, 0, 0, 0, None) == 0
        library.lua_getglobal(state, b"__result")
        length = ctypes.c_size_t()
        result = library.lua_tolstring(state, -1, ctypes.byref(length))
        assert result
        return json.loads(ctypes.string_at(result, length.value).decode("utf-8"))
    finally:
        library.lua_close(state)


def test_lua_ranges_pause_exact_frames_and_unregister() -> None:
    result = _lua_contract(
        [
            "1\tread\t68K RAM\t65535\t2",
            "2\tread\t68K RAM\t0\t65",
            "3\tread\tM68K BUS\t0\t1",
            "4\tread\t68K RAM\t-1\t1",
            "5\tadvance\t121",
            "6\twatch\tM68K BUS\t1428",
            "7\trun\t3",
            "8\tadvance\t2",
            "9\tquit",
        ]
    )
    assert result["exit"] == 0 and not result["active"]
    assert result["readCalls"] == 0 and result["frame"] == 3
    responses = [json.loads(s) for s in result["responses"]][1:]
    assert all(not r["ok"] for r in responses[:5])
    assert responses[6]["result"]["advanced"] == 1
    assert responses[7]["result"]["callbackCalls"] == 1
    assert responses[7]["result"]["paused"]


def test_lua_callback_failure_reaches_status_and_exit() -> None:
    result = _lua_contract(["1\twatch\tM68K BUS\t1428", "2\trun\t3"], callback_fault=True)
    assert result["exit"] == 1 and not result["active"]
    assert result["reports"][-1]["state"] == "failed"
    assert "injected callback failure" in result["reports"][-1]["callbackError"]


def test_lua_idle_timeout_clears_callback_and_fails() -> None:
    result = _lua_contract(["1\twatch\tM68K BUS\t1428"])
    assert result["exit"] == 1 and not result["active"]
    assert "socket timeout" in result["reports"][-1]["detail"]
