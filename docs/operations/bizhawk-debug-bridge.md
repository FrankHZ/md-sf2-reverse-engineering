# Bounded BizHawk localhost/Lua debug bridge experiment

**Confirmed:** the pinned Windows BizHawk 2.11.1 / Genesis Plus GX installation can
serve repeated agent queries while paused, advance an exact small number of neutral
frames, and return an execution callback event over a loopback TCP connection. A
single warm process avoids a new emulator launch for each observation. This is a
tool communication experiment, not original-game behavior evidence or an H3 fixture.

**Confirmed limitation:** abrupt TCP EOF can strand the upstream receive loop.
The controller bounds containment by terminating its own process; graceful Lua
unregistration on this path is **Unknown**. This prototype is useful for bounded
investigation but does not establish a production debugger or unattended service.

## Ownership and reproduction

The implementation is [the standalone Python module](../../src/sf2tool/bizhawk_debug_bridge.py),
[the fixed Lua script](../../tools/bizhawk/debug_bridge.lua), and
[focused tests](../../tests/python/test_bizhawk_debug_bridge.py). It does not register
an `sf2` command or change existing H3 observers, harnesses, fixtures, or schemas.

Use an isolated worktree and its own pristine extracted emulator, as described in
[local private inputs](./local-private-inputs.md). Resolve the registered ROM and
pristine release archive; verify the archive size/SHA-256 and extracted executable
against `manifests/toolchain.json` before extraction/use. Never copy another task's
configuration or save state. Extract the archive to its manifest-owned local path.
`uv sync --locked` owns the Python environment. The bridge uses the standard library
for TCP, existing ROM validation, the existing Lua syntax check, and the existing
project JSON encoder. The imported H3 syntax helper does not launch an H3 observer.

Before launching, inspect current EmuHawk processes and ensure the extracted
installation belongs to this worktree and is idle. The API never attaches to an
existing process. Run sequentially, with a **new** output directory every time:

```powershell
uv sync --locked
uv run python -m sf2tool.bizhawk_debug_bridge --output local/derived/debug-bridge/smoke-01
uv run python -m sf2tool.bizhawk_debug_bridge --output local/derived/debug-bridge/eof-01 --mode disconnect
uv run python -m sf2tool.bizhawk_debug_bridge --output local/derived/debug-bridge/idle-01 --mode idle-timeout
uv run pytest tests/python/test_bizhawk_debug_bridge.py
uv run sf2 verify
```

The module creates the loopback listener **before** starting its own `Popen` child.
It passes `--socket-ip=127.0.0.1`, an ephemeral `--socket-port`, `--lua`, and a fresh
`--config`. Configuration disables single-instance forwarding, sets
`LastWrittenFrom=2.11.1`, selects `Genplus-gx`, disables sound/update checks, and
starts paused. The window starts minimized without activation. Working directory,
configuration, copied ROM, SaveRAM and defaults remain inside this worktree's
ignored local installation/output. This is a Windows GUI application, not headless
emulation; startup modal errors can still require bounded process termination.

The controller validates the ROM and executable before launch, copies the ROM into
the fresh output directory, preserves raw requests/responses, errors, timing,
process PID/return code and Lua status there, and refuses to overwrite a previous
launch directory. Private paths, RAM and register values in those receipts must
not be attached to a PR. An exception or timeout never authorizes another launch;
diagnose the recorded failure and retry only the invalidated check in a new output.

For programmatic control from the same Python process:

```python
from pathlib import Path
from sf2tool.bizhawk_debug_bridge import DebugBridge

with DebugBridge(Path("local/derived/debug-bridge/manual-01")) as bridge:
    hello = bridge.start()
    state = bridge.command("state")
    data = bridge.command("read", "68K RAM", 0, 16)
    state = bridge.command("advance", 3)
    bridge.command("quit")
```

Issue the next command within five seconds. Use one serial controller, no concurrent
calls. Always call `quit` for a graceful close; leaving the context closes TCP and
then waits at most three seconds before killing only the retained `Popen` process
handle, with another three-second bounded wait. No name-based or global PID cleanup
is used. No child-process tree was observed in this experiment; arbitrary future
emulator helpers/child processes are outside this cleanup contract.

## Wire and command contract

BizHawk is the TCP **client** despite the `socketServer*` API names. Both directions
use `decimal UTF-8 byte length`, one ASCII space, then payload. There is no newline
terminator. Python handles split prefixes, split multibyte strings, combined frames,
invalid lengths, invalid UTF-8, EOF and an absolute receive deadline (default ten
seconds). Its payload bound is 65,536 bytes. Lua verifies the complete send byte
count; a short upstream send is fatal, never silently retried.

The initial JSON hello contains protocol `1`, a fresh per-launch token, runtime
version, system, actual loaded core name/type, domain names/sizes and paused state.
The token distinguishes the owned launch from an accidental local connection; this
is not a hardened security boundary against other processes running as the user.
There is no remote listener, discovery, reconnect, multiplexer, or arbitrary eval.

Controller messages are printable ASCII fields separated by tabs:
`sequence<TAB>operation<TAB>arguments...`. Sequence numbers increase from 1 to
1,000,000; command text is at most 512 bytes. Replies are JSON with `id`, `ok`,
`result`, and `error`. Validation errors return `ok=false`; malformed framing or a
bad response envelope closes the connection rather than risking stream resumption.
The Lua parser rejects unexpected arity, unknown commands and non-decimal/range
violations. Domain membership is checked before the BizHawk API, which otherwise
can silently fall back to the current domain for an unknown name.

| Operation | Arguments and behavior |
| --- | --- |
| `ping`, `state` | No arguments; frame count, pause flag, all available registers, callback state/count |
| `read` | `68K RAM`, decimal offset, count 1–64; complete range must fit the observed domain size |
| `advance` | 1–120 frames; clear every exposed controller button, unpause/advance/pause per frame; verify exact delta |
| `watch` | `M68K BUS`, even 24-bit execution address; at most one active callback; keep only the first event and a count |
| `run` | 1–120 frames with a fresh watch; advance until first hit or budget exhaustion, then unregister; event can be null |
| `clear` | Explicitly unregister the owned execution callback; no other callbacks are touched |
| `quit` | Unregister, reply, write `closed` status and exit 0 |

Queries remain possible at pause because the script calls `emu.yield()`. Frame
advance requires `client.unpause()` before `emu.frameadvance()` in this pinned
runtime; `frameadvance()` alone while paused stalled in launch 07. Query replies
are not CPU instruction snapshots promised by a debugger: a `run` event captures
callback-time PC/frame, but the returned paused state is at **frame end**.

The public Lua functions expose system ID, not loaded core name. A fixed read of
the current process's `System.Windows.Forms.Application.OpenForms` locates
`MainForm.Emulator`, then reads its `PortedCoreAttribute` and CLR type. This was
observed as `Genplus-gx` and `BizHawk.Emulation.Cores.Consoles.Sega.gpgx.GPGX`.
It is a Windows/NLua/version-specific dependency, not a config echo or a general
reflection command exposed to the controller.

## Observed acceptance and launch accounting

Execution date: 2026-09-05 America/Chicago. Baseline ROM identity and BizHawk archive,
release executable size and SHA-256 matched the tracked manifests. The registered
source ROM/archive were read-only, copied into this worktree, and rechecked after
the experiment. No source input was changed. A pre-existing foreign EmuHawk process
was left running and was never attached, configured, sent input, or terminated.

| Check | Result and boundary |
| --- | --- |
| Hello/ping/core | **PASS** actual runtime `2.11.1`, system `GEN`, `Genplus-gx`; startup about 1.7 seconds |
| Pause/query | **PASS** initial frame 0 remained 0 across ping and a query delayed 250 ms |
| Registers | **PASS** 35 available M68K/Z80 entries including `M68K PC`; raw values stay local |
| Named RAM | **PASS** `68K RAM` size 65,536, read 16 bytes; reject end overflow, bus domain, and count 65 |
| Exact frames | **PASS** frame 0→3 with neutral input; another 2-frame advance after automatic callback removal |
| Execution callback | **PASS** accepted `VInt` at `0x000594`; first event at frame 14 with PC equal to address; reply paused at frame 15 after 12 advanced frames |
| Callback removal | **PASS** automatic unregister returned true; later advance left count unchanged; explicit watch/clear also returned inactive |
| Normal quit | **PASS** `closed`, inactive callback, exit 0, no forced termination |
| Idle timeout | **PASS** five-second receive timeout, callback cleared, `failed` status and exit 1 without forced termination |
| Abrupt EOF | **FAIL** graceful Lua recovery; **PASS** controller containment by owned process termination after three seconds; last Lua status still marked callback active |
| Callback exception | **PASS** production Lua against injected API failure: error retained in status, callback cleared, exit 1; this fault was not injected into the real core |

The single-launch smoke stopped before any full game scenario. The callback address
comes from accepted `tests/fixtures/h3/controller-input-v1.json` →
`sourceContext.vIntEntryAddress`, owned by
[technical interrupts](../research/technical-interrupts.md), with upstream
`ShiningForceCentral/SF2DISASM` commit `c834c652b6862bc5679fd7f69a38a7093206efc6`.
No ROM modification, seeded CPU register, savestate, or old #303/#309 route was used.
This does not promote that callback to evidence about natural game progression.

All ten launches completed; none is pending or an interrupted historical H3 run:

| Local launch | Outcome and correction |
| --- | --- |
| 01–03 | **FAIL** before TCP connection: missing config `LastWrittenFrom` caused version modal. 01 used ten-second deadline; 02 retained a 45-second diagnostic deadline; 03 made the previously hidden modal targetable. Each owned process was terminated. |
| 04 | **FAIL** after connect: .NET assembly not loaded into NLua; Lua reported error and exited 1. |
| 05 | **FAIL** after connect: overloaded FormCollection numeric lookup returned nil; exited 1. |
| 06 | **FAIL** after connect: `luanet.each` unavailable; replaced with CLR enumerators; exited 1. |
| 07 | **FAIL** advance while paused stalled after successful query/read checks; owned process terminated. |
| 08 | **PASS** smoke, normal exit 0. |
| 09 | **PASS** bounded EOF containment; graceful recovery **FAIL** as above. |
| 10 | **PASS** idle timeout cleanup, deliberate exit 1. |

The first Python test run completed with 20 passed and two setup errors, including
an oversized parameter ID in its output; the corrected compact-ID/socket test run
passed 21. The expanded focused suite passed 27, including injected Lua execution.
The Lua tests require the pinned local `lua54.dll` and visibly skip without it;
they never launch EmuHawk. Raw launches and test receipts remain ignored.

The normal `uv run sf2 verify` completed its 148 commit-critical Python tests,
Ruff, documentation traceability, research index and H0 ROM checks successfully,
then exited 1 at toolchain provenance because the new worktree has no registered
local `SF2DISASM` checkout. The aggregate command is **not a pass**; that dependency
is unavailable here. No complete H3, H1 rebuild, or `--full` profile was run.

The clean committed planner classified `tools/bizhawk/debug_bridge.lua` as
`unknown H3 input`, selecting all six H3 partitions plus public core and tooling
Python. This is a real placement/ownership problem, not permission to ignore its
selection. The focused public-CI node
`tests/python/test_verification_plan.py::test_every_tracked_h2_h3_artifact_has_closed_exact_ownership`
was run and **failed** at line 817 because that exact Lua path is outside the
closed H3 owner sets. Its completed result is retained; no historical H3 suite or
complete Python suite was run in response. Main-gate must decide the smallest
ownership correction before acceptance. The proposed correction is moving this
communication-only script to `tools/debug_bridge.lua`, outside the H3-specific
directory, with the Python script reference and this document updated; it requires
the explicitly assigned Lua path ownership to be extended first. Do not weaken the
planner, the closed-owner assertion, or an existing H3 fixture to admit this tool.

Rough timings from launch 08: warm queries roughly 0.5–18 ms, three frames about
51 ms, callback run about 167 ms. These are one local operational sample, including
UI scheduling, not a benchmark, latency guarantee, or measured agent quota saving.

## Unsupported and Unknown

- **Unsupported:** CPU instruction stepping, register writes, total-cycle counts,
  instruction-precise breakpoint suspension, arbitrary memory writes, arbitrary Lua
  evaluation, state loading, uncontrolled run, input automation beyond neutral frames,
  remote clients, reconnect, and more than one callback/controller.
- **Unknown:** graceful Lua callback unregistration after EOF. The retained process
  handle was killed and waited successfully, so that core cannot continue executing
  callbacks; the last status file does not prove `event.unregisterbyid` ran.
- **Unknown:** sustained sessions with periodic keepalives, other cores/releases/OSes,
  external script cancellation, hostile local clients, and sustained throughput.
  A stalled receive can block the emulator UI; do not use the bridge as a production
  debugger or a replacement for accepted batch H3 evidence.

## Primary source boundaries

BizHawk sources are pinned to commit `bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5`
(2.11.1), read 2026-09-05:

- [SocketServer.cs](https://github.com/TASEmulators/BizHawk/blob/bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5/src/BizHawk.Client.Common/Api/SocketServer.cs): `Connect`, `PrefixWithLength`, `ReceiveString`, `SendString`; the prefix receive ignores zero-byte EOF and send does not loop on a short write.
- [CommLuaLibrary.cs](https://github.com/TASEmulators/BizHawk/blob/bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5/src/BizHawk.Client.Common/lua/CommonLibs/CommLuaLibrary.cs): `socketServerSend`, `socketServerResponse`, `socketServerSetTimeout`.
- [GPGX.IDebuggable.cs](https://github.com/TASEmulators/BizHawk/blob/bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5/src/BizHawk.Emulation.Cores/Consoles/Sega/gpgx64/GPGX.IDebuggable.cs): actual registers and M68K BUS callbacks; `CanStep=false`, stepping and register writes unimplemented.
- [EventsLuaLibrary.cs](https://github.com/TASEmulators/BizHawk/blob/bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5/src/BizHawk.Client.Common/lua/LuaHelperLibs/EventsLuaLibrary.cs): execution callback registration/empty-ID failure and explicit unregister.
- [MainForm.cs](https://github.com/TASEmulators/BizHawk/blob/bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5/src/BizHawk.Client.EmuHawk/MainForm.cs): public loaded `Emulator` and command-line Lua startup on `Shown`.
- [ConfigService.cs](https://github.com/TASEmulators/BizHawk/blob/bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5/src/BizHawk.Client.Common/config/ConfigService.cs): `IsFromSameVersion` requires the matching `LastWrittenFrom` to avoid a modal warning.
