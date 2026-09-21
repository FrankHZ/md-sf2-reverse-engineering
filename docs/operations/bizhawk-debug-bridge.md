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
[the fixed Lua script](../../tools/debug_bridge.lua), and
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

## Map3 interactive composition (offline only)

The [Map3 acquisition owner](../research/map3-messenger-acceptance.md#bounded-interactive-acquisition)
composes `DebugBridge` with the existing candidate observer. `start` accepts the fixed observer,
prepared observer configuration and session ROM supplied by that owner; it retains the same single
`Popen` handle, loopback framing/token, serial `command` receipts and contained teardown. The fixed
`tools/debug_bridge.lua` library entry shares connection, parsing, explicit button validation and
input clearing. The standalone experiment commands and its five-second idle limit retain their
previous scope; they do not execute the Map3 acquisition protocol or prove controller delivery.

Only the explicit acquisition mode accepts `step <count> <button>`, `state`, `ping` and `abort`.
`step` uses 1–120 frames and exactly one of `neutral`, `Up`, `Down`, `Left`, `Right`, `A`, `B`, `C`.
No combinations, Start, reset, memory writes, arbitrary watches or eval are exposed in this mode.
Lua clears all exposed inputs, applies the chosen player-one button and records each completed
frame before advancing again. Unsupported/malformed commands fail before input application and
terminate the acquisition. A callback failure or terminal within a batch skips its remaining frames.

For [Issue #485's segmented stabilization](../research/map3-messenger-acceptance.md#savestate-linked-segments-issue-485),
the same loop also accepts zero-argument `save`. The observer supplies early closed checkpoints,
live entity/consumer C readiness and original FieldMenu B recovery. `saveReadiness` and `inputReadiness`
expose predicates; ordinary rejected saves/steps return typed `ok=true`, nonterminal, zero-frame
results. They do not reset idle time or charge advancing batches. A batch is truncated when recovery
begins or returns; replies report actual delivered frames without automatically finishing the batch.
No new protocol command or generic bridge-library mutation is needed.

The runner publishes immutable native-state/continuation pairs after cleanup and reconciles all actual
consumption. Failed child attempts remain recorded and may be retried from a compatible complete
parent. Earlier complete parents can also be selected after the runner reconciles every completed
branch in the same source lineage, including siblings/cousins; active or incomplete attempts block
resume. Existing pairs/claims are immutable and terminal pairs remain nonresumable.
Runtime/core/observer/runner/source identity
checks remain mandatory. Loaded RAM/register/frame equality and offline checks alone do not establish
complete native continuity. The [actual house save → native load → Sarah save](../research/map3-messenger-acceptance.md#accepted-early-native-save-and-resume)
is now **Confirmed**, including input-before-load exclusion, inherited epochs, forward checkpoint,
process exit and cleanup. The [forward chain](../research/map3-messenger-acceptance.md#resumed-messenger-and-map19-checkpoints)
now reaches independently accepted messenger closure and native Map19 save. Later
[royal/guard saves and loads](../research/map3-messenger-acceptance.md#royal-and-guard-saves-battleloop-return-stack-failure)
are independently accepted; the [final compatible chain](../research/map3-messenger-acceptance.md#accepted-first-battle01-player-ready-acquisition)
now establishes **Confirmed** first Battle01 readiness and its nonresumable evidence pair. Native menu recovery
remains **Unknown**.
A missed CheckBattle return exposed asymmetric 24-bit stack masking in the composed observer. Its
expected return stack now uses the same mask as actual A7. This does not change the shared bridge or
waive parent identities. The [fresh compatible chain](../research/map3-messenger-acceptance.md#corrected-chain-and-player-entry-window-count-failure)
now confirms that wrapped return and battle lifecycle, but fails a different final predicate at
the first input PC: `WINDOW_IS_PRESENT=2` includes the normal mini status display, not necessarily
a blocking menu. The final-battle correction removes that mistaken count test and records the
actual guard fields before asserting; runner/bridge and field/save/menu checks are unchanged.
Its changed observer identity again requires fresh compatible parents. That failed attempt does not
establish passed first-ready; reaching its PC alone does not satisfy the acceptance contract.
The [following chain](../research/map3-messenger-acceptance.md#movement-grid-palette-failure-and-correction)
passes the native window-count condition, then records the remaining rejected field exactly:
`FADING_SETTING=5`, the original movement-grid pulse. The final predicate now admits mode 0 or that
source-selected mode 5; field/save fade checks and the shared bridge remain unchanged. Fresh compatible
chain 33–37 now passes actual final guards and independent review. Callback order 52985 is observer
frame 22368/emulator frame 22367 at `0x22E70`, actor 2; the completed saved frame is emulator 22368.
Window count 2 and palette mode 5 coexist with cleared actual blocking guards. The last neutral120
delivers 6 frames and no later battle action. Its rank-8 pair is final evidence, never resumable.
The original standalone five-second idle experiment and its wall limit remain unchanged.

Historical clock-registration, camera-readiness and wrong-facing failures are retained in the Map3
owner. Their costs are inherited, not reset; the completed initial segment started with totals 6 / active seconds
2080.6977567999857 / delivered frames 13561 / advancing batches 225. Totals after the retained final-segment
callback failure were starts 15 / seconds 4142.950475800084 / frames 32143 / batches 555. After the
compatible chain's window-count failure, totals were 25 / 5629.673442000174 / 60271 / 1121.
After the next chain's palette failure, totals were **30 / 6094.842934200191 / 82639 / 1592**.
After successful final chain 33–37, current totals are **35 / 6562.418724000221 / 105007 / 2063**.
This includes the second context-idle failure (21), whose final restoration/callback cleanup remains
**Unknown**, and the explicit local-operator abort (25), whose loaded-entry cleanup is confirmed.
Full receipts remain local. The improved ignored finite operator checks actual positions, consumer
readiness, prompt choice and caller completion, and reads one full JSON response before the next
meaningful input. It completes whole gate/royal/guard segments without task-context dependence;
messenger 22 requires a documented manual finish at the valid `WaitForPlayerInput` consumer, while
fresh prefix 28 completes that entire prefix automatically through a valid messenger save. All five
final-chain segments complete automatically with exit 0, entry-state restoration, callback removal,
session-ROM deletion and unchanged canonical identity. Final Battle01 actions/victory/5B and H4
remain **Unknown**; no further native input is part of this completed acquisition.
The operator rechecks closure after neutral waits and switches from bounded one-frame approach
inputs to neutral/dialogue immediately on actual battle admission. It extends no watchdog or bridge
protocol and sends no artificial keepalive. The clock still uses explicit
System assembly loading and Stopwatch. Cumulative time/stage/frame/batch/progress thresholds are now
observations under the user's stabilization authorization, not hard stop conditions.

`DebugBridge.interact()` reads one JSON array per stdin line, such as `["state"]` or
`["step", 1, "C"]`, and prints each JSON result. Waiting for a line consumes the same process wall
budget; no keepalive or gameplay input is generated while paused. EOF, Ctrl-C or invalid host input
attempts `abort` if the connection remains usable, then closes through the existing process owner.
An independent timer contains the owned process at 1800 seconds even during operator wait or a
blocked native receive. Startup and individual exchanges retain a shorter 60-second bound. Normal
teardown waits up to three seconds before killing the retained handle, then waits up to three seconds
for termination; this cleanup grace grants no further gameplay frames or starts.

The acquired observer raises its receive idle bound to the reviewed wall limit. This change and
actual button delivery are **Unknown at the native runtime boundary** until an admitted applicable
observation. The earlier neutral-advance experiment does not validate them. Abrupt native TCP EOF
can still strand the upstream receive loop: graceful Lua callback removal/restoration on that path
remains **Unknown**, while host containment is mandatory. At actual execution, independently inspect
owned survivors and retain PID/exit/timeout, host and Lua statuses, restoration, callback removal,
session deletion and canonical identity. Never replace that check with a global process kill.

`bridge/receipt.json` preserves command/response order and process diagnostics; the observer owns
`actual-inputs.jsonl`, `checkpoints.jsonl`, its typed failure/status and final restored observation.
The bridge's own callback fields describe only bridge callbacks; candidate callback cleanup is proved
by the candidate status/restoration, not by an empty bridge callback slot. All these outputs remain
private under the owning worktree's fresh ignored attempt directory.

### Natural Battle01 selection

The [natural continuation capability](../research/map3-messenger-acceptance.md#natural-battle01-continuation-capability-offline-only)
uses the same serial bridge and controller protocol. Only this exact acquisition composition
uses the stabilization policy supplied by `_interactive_limits`; ordinary bridge/Map19 calls retain
their existing wall limits. Cumulative costs and progress remain visible without a historical total
deadline terminating stabilization. No process is restarted automatically by the host.

A host timer contains the owned process after 120 paused seconds without advancement. `state`, `ping`,
save inspection and rejected steps do not renew it. During a step exchange the paused timer is
suspended; a zero-frame rejection restores its previous absolute deadline, while actual advancement
starts a new interval. Cancelled timer callbacks cannot kill a later interval. Startup and each
exchange retain 60 seconds; teardown retains 3+3 seconds. Lua enforces idle/disconnection timeout and
the bridge parser retains the 120-frame per-step maximum. No frames are filled in after a truncated
batch. Native receive/EOF restoration remains **Unknown**; host containment does not prove graceful
Lua cleanup.

The response and host receipt preserve a typed stop reason. Successful restoration after AI-first is
a coverage stop, and after a limit is an incomplete observation; neither is a player-ready result.
Callback-time terminal state, later same-frame callbacks and completed-frame state remain separate.
The additional selection has only offline implementation checks and preparation authorization. No
bridge experiment, native startup, reconnection, replay or new acquisition allowance is implied.

### Battle01 victory continuation (Issue #496)

The [acquisition owner](../research/map3-messenger-acceptance.md#battle01-actions-and-victory-continuation-issue-496)
adds the explicit `natural-battle01-victory-5b` selection to the existing runner/observer. The shared
bridge protocol, standalone limits and transport implementation are unchanged. `state` and step
responses carry private original battlefield/grid/target facts and stage-specific input readiness.
After battle admission each nonneutral step is one frame; neutral steps retain the 120-frame bound.
Consumed input invalidates its previous poll, and the finite operator waits for the new source
consumer before acting again. Heal 1's original icon/level/target input and effects have now been
observed natively. The Medical Herb extension distinguishes the top battle diamond from
the source `MENU_ITEM` diamond, admits only Use=0, then reads the original item poll at `0x10616`.
Original D0 item / D1 slot and the inventory word must agree; only herb 0 after the original index
mask is admitted. It reuses the target consumer and records original effect/slot removal. Nested
short callers must close before save. Equip/Give/Drop confirmation and other manual items/spells
remain explicitly unsupported; source polls/results and input-readiness rejection enforce that
boundary. B cancellation and consumed-poll invalidation retain their original return contexts.

The same `save` command may seal later battle checkpoints only in this selection. The observer
requires a neutral completed movement frame, closed short consumers and the three exact original
long-call return descriptors; load verifies original core state and rebuilds their callbacks before
input. Existing parent identity, forward-checkpoint, exclusive publication and failed-child accounting
rules apply. The old first-player-ready terminal pair cannot resume. The final victory/after-program/
flag/field predicate and nonresumable terminal pair are owned by acquisition, not by a bridge success
receipt. Accepted PR499 source produced native battle save/load and actions through original defeat
in prepared-16, but its 78571-byte terminal reply exceeded the unchanged **65536-byte** bridge
limit. Host FAIL is preserved despite clean native exit/restoration/ROM cleanup. The accepted
correction sends a compact stop descriptor and local observation filename in victory-mode
`terminalCallback`; full original terminal facts stay in existing private evidence files. Direct
replay of the failed reply produces 41926 bytes. Shared framing and limits are unchanged.
At that failure, totals were **46 / 7479.841504100186 / 140632 / 3306** (starts/seconds/frames/batches).
Prepared-17..32 subsequently confirm native corrected replies and Heal input/effects at PR500's
accepted source. Defeats 27/29/32 report INCOMPLETE-OBSERVATION with native exit 0, restored entry
state, cleared callbacks, deleted session ROM and unchanged canonical identity. All costs remain
charged. PR501's 33..44 then confirm original herb use/slot consumption/healing and a later
original defeat. Totals through that chain were **73 / 10247.52403570019 / 229866 / 7193**
(starts / seconds / frames / batches). Snapshot readiness probes C; directional navigation is
still checked independently when the sole unmet reason is `unsupported-battle-selection`.
The old local operator's failure to distinguish these caused 41 to stall. Upstream operator
termination lost the reader; receipt Errno 22, exit 1 and forced containment are preserved.
Session-ROM cleanup is confirmed, final restoration/callback cleanup **Unknown**. The local
operator now has an explicit stdin abort hook that keeps the response reader alive.

Accepted PR502's completed-branch recovery follows parent links and immutable claims through the
whole source lineage, charging terminal/failed descendants without making them loadable. Native 62
loads earlier parent 59 after completed 60 and failed 61, with their costs retained. The selected branch
reaches victory and actual after-program/flag/return/exploration events in 67. Its later map 57 setup
callback fails before stable 5B; native exit 1 and host ROM cleanup are confirmed, final observer
restoration/callback clearing **Unknown**. The explicit stdin abort 61 also lacks that final payload;
a local abort-reader correction does not retroactively establish cleanup.

Totals through that failed source chain were **96 / 12405.304031100066 / 304294 / 10553**
(starts/seconds/frames/batches).
The [acquisition result](../research/map3-messenger-acceptance.md#native-victory-after-program-return-and-post-victory-observer-failure)
owns exact lineage, chronology, native failures and reproduction. Its offline correction binds
map 57's original `ms_Void` only after the observed victory return and treats
`EndAfterBattleCutscene` as a tail of the existing call, eliminating the false independent consumer.
No protocol, framing, shared bridge or original-state mutation is added. Old source pairs remain
incompatible. After independent PR503 source/preparation acceptance, fresh R1 prepared-68 carries
all prior costs; 68..86 complete with 18 resumable checkpoints and one nonresumable terminal pair.
All 19 native invocations exit 0 without timeout/forced termination, restore entry state, clear
callbacks, delete their session ROM and preserve canonical identity; no owned native/operator
process survives. Final host status is `OBSERVATION-COMPLETE-UNREVIEWED`, not H4 acceptance.
The [final acquisition result](../research/map3-messenger-acceptance.md#native-victory-and-stable-field-readiness)
records original victory, actual after-program/shared-tail return, source-correct void setup and
settled neutral frames 61009/61010 with actual field-control polls. Current totals are **115 /
14197.020479699888 / 365304 / 13351** (starts/seconds/frames/batches), including every earlier failure.
The terminal reader rejects resume from 86. No post-endpoint nonneutral input is delivered;
its state effect, full RA-12, remaining 8D and H4 remain **Unknown**. Protocol and framing are unchanged.

## Acquisition performance boundary

**Confirmed:** a short comparison at accepted source
`5102804b98f3574bb87d6ba4b43d88413c501a68` separates the slow acquisition controller
from original emulation. Both runs used the registered BizHawk 2.11.1 / Genplus-gx
executable directly, the manifest US ROM, the same accepted resumable state and
the same 1,030 actually applied input frames from one retained Battle01 segment.
No emulator installation was copied. This is a performance diagnostic, not another
scenario acceptance or a continuation of the completed acquisition.

| Measurement | Observed result |
| --- | --- |
| Continuous Lua input delivery, without acquisition callbacks or host round trips | 1,030 frames in 17.1813 s: 59.95 simulated frames/s |
| Existing acquisition observer and bridge, immediate next request, no operator sleep | 1,030 frames across 228 step requests plus save in 77.2158 s: 13.34 simulated frames per wall second |
| Bridge request/execution/response intervals | 23.8662 s total; includes emulation, observer work, serialization and transport, not just network latency |
| Whole-receipt serialization and writes, timed around `DebugBridge._save` | 53.5420 s across the run; 11.9246 GB cumulative rewritten file lengths, not measured physical-device traffic |
| End-state comparison | All 65,536 bytes of 68K RAM, all reported CPU registers and emulator frame count equal |
| Comparison with retained acquisition | Every request's applied-frame count, frame number, selected gameplay/RNG fields and battlefield accounting equal |

Command time excludes startup and process teardown. The `_save` total includes its
few lifecycle calls outside the command loop, so the two time columns are not an
exact additive partition. Both processes exited 0 without forced termination.
Windows ran them minimized with the retained GDI/throttle settings. These are
simulation/wall-progress measurements, **not screen presentation FPS** or a general
animation-cadence benchmark. No screenshots were taken.

The main measured cost is receipt persistence: `command` rewrites the growing
`commands` array before and after each exchange. Mean `_save` time grew from
20.96 ms in the first 50 commands to 221.42 ms in the last 50. A final receipt is
about 52 MB. The observer additionally pauses and snapshots after each frame;
209 of the 228 step requests advanced only one frame, including neutral requests
shortened at observed boundaries. These safeguards explain why acquisition is not
real-time play, but they do not justify repeatedly rewriting the full history.

**Inferred improvement:** first retain incremental command/results and write the
aggregate receipt once at finalization, while preserving interrupted-command and
failure evidence. Reuse the existing append-log pattern rather than adding a
storage framework. Then profile observer snapshots/stop boundaries before changing
batching; do not remove readiness guards or full input recording just to improve
speed. No performance fix is implemented by this diagnostic. The comparison found
no original-state divergence in this segment; other segments remain unmeasured.

Reproduction uses ignored `local/acquisition-speed-diagnostic/run_comparison.py`
after loading `local/private-inputs.ps1`. It consumes the retained `prepared-73`
state and `prepared-74` requests under `local/issue496`, expands **actual** advanced
frames (not requested counts), delivers them continuously, then repeats the original
requests through the existing observer. A task-local launch substitution returns the
registered executable and local config/TEMP without calling materialization.
`summarize.py` reads the resulting `comparison-report.json`, acquisition `timing.json`
and original segment pairs. Fresh output names and explicit native-run ownership are
required; the completed directories must not be overwritten. Inputs, native states,
RAM/register dumps, receipts and scripts stay private/ignored. This bounded local
recipe is not a new maintained CLI or a dependency of H4.

Installation reuse is a separate question: see
[the direct-execution findings](./local-private-inputs.md#direct-installation-reuse-findings).
Changing executable location alone does not remove receipt or observer overhead.

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

All eleven launches completed; none is pending or an interrupted historical H3 run:

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
| 11 | **PASS** one smoke after the authorized Lua path move, normal exit 0; idle/EOF results retained without rerunning them. |

The first Python test run completed with 20 passed and two setup errors, including
an oversized parameter ID in its output; the corrected compact-ID/socket test run
passed 21. The expanded focused suite passed 27, including injected Lua execution.
The Lua tests require the pinned local `lua54.dll` and visibly skip without it;
they never launch EmuHawk. Raw launches and test receipts remain ignored.

The initial `uv run sf2 verify` completed its 148 commit-critical Python tests,
Ruff, documentation traceability, research index and H0 ROM checks successfully,
then exited 1 at toolchain provenance because the new worktree had no registered
local `SF2DISASM` checkout. That completed result remains a failure. After preparing
an independent checkout at the manifest-pinned commit and copying/verifying the
registered JDK tree into this worktree, normal `uv run sf2 verify` **passed**,
including all 148 commit-critical tests and toolchain provenance. At the initial
Draft boundary, no complete H3, H1 rebuild, or `--full` profile had been run.

The clean committed planner classified `tools/bizhawk/debug_bridge.lua` as
`unknown H3 input`, selecting all six H3 partitions plus public core and tooling
Python. This is a real placement/ownership problem, not permission to ignore its
selection. The focused public-CI node
`tests/python/test_verification_plan.py::test_every_tracked_h2_h3_artifact_has_closed_exact_ownership`
was run and **failed** at line 817 because that exact Lua path is outside the
closed H3 owner sets. Its completed result is retained; no historical H3 suite or
complete Python suite was run in response. The accepted placement correction moves
this communication-only script to `tools/debug_bridge.lua`, outside the H3-specific
directory. The planner explicitly maps that path and
`src/sf2tool/bizhawk_debug_bridge.py` to this tool's focused Python tests, preserving
the source module's normal reverse-dependent selection. A Lua-only change therefore
still selects the owning tests. Existing H3 classifications and the closed-owner
assertion remain intact.

The post-correction command
`uv run pytest tests/python/test_bizhawk_debug_bridge.py tests/python/test_verification_plan.py`
**passed all 113 tests**, including the previously failing closed-ownership node,
source-only and Lua-only bridge mappings, and a synthetic future reverse-dependent
consumer. These do not execute a full Python suite or any H3 scenario.

### Initial Draft verification boundary

The initial code candidate `c64075e718d2fc3a529f0cf2cb7cff18ab0fd5c7` had a clean
committed plan with **no unclassified paths**, but the complete change also edits
the planner and its existing owning test file. Its exact selection is reproducible
with `uv run sf2 verify plan --base origin/main --head HEAD` on this clean candidate
and the subsequent documentation-only handoff commit. The existing classifications
therefore select more than the new bridge's two individual entry paths:

| Selected partition | Exact reason or reason family | Execution status |
| --- | --- | --- |
| `public-core` | `always-run commit gate` | `uv run sf2 verify` **PASS** after the recorded input setup correction |
| `tooling-python` | The bridge source, Lua and tests, plus `src/sf2tool/verification_plan.py` and `tests/python/test_verification_plan.py` | Both focused test files **PASS** (113 combined); generic `uv run pytest` **NOT RUN** |
| `remake-dotnet` | `src/sf2tool/verification_plan.py` | `dotnet restore remake/Sf2.Remake.sln --locked-mode`, `dotnet build remake/Sf2.Remake.sln --configuration Release --no-restore`, and `dotnet test remake/Sf2.Remake.sln --configuration Release --no-build --no-restore` **NOT RUN locally** |
| `remake-godot` | `src/sf2tool/verification_plan.py` | `uv run python -m sf2tool.remake_godot` **NOT RUN** |
| `h1-original` | `src/sf2tool/h3/bootstrap.py reaches sf2tool.harness` | **NOT RUN**; planner has no standalone H1 command |
| `h2-battle-logic`, `h2-stats-items`, `h2-map-scripting`, `h2-services-state` | `src/sf2tool/h3/bootstrap.py reaches sf2tool.h2.<consumer>` | All selected owning commands **NOT RUN** |
| `h3-battle01`, `h3-map-debug`, `h3-direct-seam`, `h3-witch`, `h3-sound`, `h3-original-reference` | `src/sf2tool/h3/bootstrap.py reaches sf2tool.h3.<consumer>` | All selected runtime/preflight commands **NOT RUN** |

The bootstrap selections originate in the **existing**
`from sf2tool.h3.bootstrap import COMMAND_LAUNCHES` in the changed planner test
file: `_select_imports` follows the shared bootstrap's reverse dependents.
Neither that import nor the planner's self-classification was changed to suppress
these selections. The complete consumer command list remains in the reproducible
planner output; the reason families above describe why it is selected.

Main-gate initially limited the experiment to a reviewable Draft PR. The **NOT RUN**
entries above preserve that initial verification stopping boundary; they were not
a waiver or PASS. Merge readiness was not satisfied at that boundary. The subsequent
merge-readiness authorization admitted the selected gates below, while preserving
the eleven completed bridge launches and prohibiting additional bridge smoke runs.

### Merge-readiness follow-up

The expanded change owns the original six bridge/planner paths and five test files:
`test_original_reference_replay.py`, `test_map3_entity142_interactable_reference.py`,
`test_map3_original_player_reference_frame.py`, `test_map3_messenger_acceptance.py`,
and `test_h3_bootstrap_inventory.py`, all under `tests/python/`. Production H2/H3
code, schemas, registries and fixed digests are unchanged by these test corrections.

Direct bridge verification consists of the 27 focused tests and the eleven real
bridge launches accounted above. The combined 113-test run also covers planner
ownership. The subsequent H1, H2, H3 and remake checks exercised the existing
verifiers and launchers; **those H2/H3 commands did not use the bridge**. Their
conservative planner selection and results are regression evidence, not additional
observations of the bridge transport or a measure of its usefulness.

One `pwsh -NoProfile -File scripts/Invoke-Sf2Rebuild.ps1 -KeepBuildArtifacts`
**passed**, reproducing all 2,097,152 bytes of the manifest baseline. Its retained
listing, symbols and binary supply the conventional local H1 consumer filenames.
The initial copy omitted the conventional binary name; the correction copied the
actual retained successful rebuild output and verified its identity. It did not
substitute the original ROM for a rebuild or run a second rebuild.

The single complete Python discovery run used
`uv run pytest -n 4 --dist loadfile --max-worker-restart 0 --durations 25 --tb=short`
with an ignored JUnit destination. It **completed with exit 1: 2,970 passed,
193 failed, 9 skipped in 1,539.69 seconds**. Of those failures, 188 stopped on the
missing conventional H1 binary/denominator. After supplying the retained H1 output,
those exact 188 nodes **passed in 58.92 seconds**. The complete suite was not rerun.

Four of the original nine skips also depended on that missing H1 binary. Once the
input existed, these exact nodes **passed separately in 12.38 seconds**:
`test_field_search_control.py::test_field_search_retained_owner_digest_drift_rejects_fixture_comparison`,
`test_field_search_control.py::test_field_search_complete_verifier_matches_fixture`,
`test_field_item_effects.py::test_field_item_effects_complete_verifier_matches_fixture`,
and `test_field_menu_control.py::test_field_menu_complete_verifier_matches_fixture`,
all under `tests/python/`. The other five original skips remain the explicit resvg
and private-visual environment-variable opt-ins; they were not silently promoted
to passing checks.

The other five completed failures were present in the accepted base and are
preserved here by their exact test nodes:

- `tests/python/test_original_reference_replay.py::test_real_global_ordinal_one_lock_is_consumed_read_only`
  required a private historical launch ledger in a new worktree. The test now
  visibly skips only when its local ledger file is absent. An existing empty,
  malformed or forged ledger still reaches the strict validator. Synthetic missing
  and forged-lock tests remain intact; no historical ledger was copied or invented.
- `tests/python/test_map3_entity142_interactable_reference.py::test_index_has_exact_existing_record_delta_and_public_totals`
  applied an owner-only remover directly to the later current index. It now uses the
  existing strict registered chain to recover that owner's state before checking
  the unchanged predecessor digest, exact bindings and nonmutation.
- `tests/python/test_map3_original_player_reference_frame.py::test_index_has_exact_existing_owner_delta_and_public_totals`
  retained totals from before the accepted player-ready registration. Both affected
  index tests now check 96 H3 fixtures and 3,111 address bindings; exact global and
  owner checks remain.
- `tests/python/test_map3_messenger_acceptance.py::test_observer_config_has_no_accepted_output_and_closed_roles_and_phases`
  counted the shared observer's opt-in player-ready callbacks as messenger callbacks.
  The test now checks the two complete `config.extension` registration blocks,
  preserves messenger enum checks, and closes the extension's 17 registrations and
  ten phases against exact accepted sets. Guard-removal and unknown-role mutations
  must fail. The extension schema permits nonempty role/phase strings: the exact
  extension sets are a **test boundary**, not a newly claimed schema enum.
- `tests/python/test_h3_bootstrap_inventory.py::test_h3_bootstrap_registry_closes_every_registered_owner`
  omitted the accepted player-ready command from command/launch totals. Its one
  launch is checked explicitly: eight witch-menu commands, 72 one-launch commands,
  and 139 registered launches. The seven witch-menu observer count remains unchanged.

All five complete owning test files **passed: 116 passed, one optional historical
ledger skip in 27.09 seconds**, including the new callback-boundary mutations.
These results and the 188-node correction are separate results; they do not turn
the completed 193-failure discovery run into a new complete-suite PASS.

The committed eleven-path planner has no unclassified paths. Compared with the
initial six-path plan it adds only `h2-presentation`, with
`uv run sf2 h2 map3-entity142-interactable-reference` and
`uv run sf2 h2 map3-original-player-reference-frame`, plus the five already executed
owning test commands. The selected total is 33 H2 commands and the original 77 H3
runtime/preflight commands. No broader legacy aggregate was run solely to repeat
the already completed H1 and Python work.

| Selected gate | Follow-up result |
| --- | --- |
| Full Python discovery and corrections | Original **FAIL** retained; 188 corrected nodes **PASS**; five owning files **116 PASS / one skip**; four newly enabled H1 nodes **PASS**; five original opt-in skips retained |
| H1 | **PASS**, one byte-perfect retained rebuild |
| Release .NET restore/build/test | **PASS** in Public CI run `34010394626` at `535ebba09dab2b17347020453594e982f8900af6`; retained across test/document-only corrections |
| Godot | **PASS**, `uv run python -m sf2tool.remake_godot`; all seven steps exit 0, no timeout, cleanup clean |
| H2 | **PASS**, all 33 selected commands; explicit owning PASS or complete saved output/fixture comparison reviewed separately from process completion |
| H3 | **30 semantic PASS / 47 NOT RUN**; queue safely paused after `spell-status`, with no callback failures or remaining owned processes in the completed commands |
| Final public-core, committed planner and CI | Required on the frozen head for independent integration; exact head and results are recorded in [PR #310](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/310) |

The last H3 command, `uv run sf2 h3 spell-status`, completed naturally with exit 0
and semantic PASS at 2026-09-06 00:40:13 America/Chicago. The next command,
`spell-summon`, was held before launch. The queue's exit 1 reflects that deliberate
hold, not an interrupted command or an H3 test failure. No emulator was terminated
to pause the queue. All 30 completed commands have explicit owning PASS results,
saved status snapshots without callback-failure markers, and no residual owned PID.

These **47 commands remain NOT RUN**. Each entry is a suffix of `uv run sf2 h3`:

| Selected partition | Unexecuted command suffixes |
| --- | --- |
| `h3-battle01` (1) | `spell-summon` |
| `h3-map-debug` (25) | `entity-movement`, `entity-population-reload`, `force-state-active-party`, `force-state-roster-death`, `map-animation-vdp`, `map-block-copy-lifecycle`, `map-block-mutation`, `map-camera-control`, `map-entity-action-bridge`, `map-entity-gesture-relationship-motion`, `map-entity-lifecycle-presentation`, `map-entity-placement`, `map-event-dispatch`, `map-init-dispatch`, `map-interaction-trigger`, `map-lifecycle`, `map-script-control-audio`, `map-script-dialogue`, `map-script-entity-clone`, `map-script-entity-presentation-fx`, `map-script-screen-presentation`, `map-script-transition`, `map-script-ui-primary`, `map-setup-selection`, `story-state` |
| `h3-direct-seam` (11) | `blacksmith-mithril`, `church-cure-lifecycle`, `church-raise-lifecycle`, `church-save-lifecycle`, `controller-input`, `growth`, `growth-prowess`, `growth-refresh`, `service-menu-lifecycle`, `sram-lifecycle`, `stat-clamps` |
| `h3-witch` (8) | `map3-admitted-start`, `map3-battle01-natural-route`, `map3-battle01-player-ready`, `map3-messenger-acceptance`, `map3-original-player-locomotion-animation`, `witch-new-game-lifecycle`, `witch-save-actions`, `witch-save-menu-actions` |
| `h3-sound` (1) | `sound-timing` |
| `h3-original-reference` (1) | `original-reference-replay-capability --preflight-only` |

The clean-head planner reproduces the full selection. Ignored per-command logs,
status snapshots, `h2-semantic-review.json`, `h3-semantic-review.json` and
`main-gate-hold-summary.json` are retained under
`local/derived/debug-bridge/merge-gates/`; they contain private/generated data and
must not be attached to the PR. The table above preserves the unexecuted boundary
without requiring access to those local records.

### Bounded merge acceptance for PR #310

On 2026-09-06, the user explicitly authorized merging PR #310 and main-gate
accepted its bounded integration on the recorded direct bridge evidence,
proportional public verification and the disclosed regression boundary. The
remaining 47 H3 commands stay paused; this decision does not require resuming them.
It applies **only to PR #310**. It neither changes the global planner nor relabels
the unexecuted selections as PASS or NotApplicable. The original full Python
failure and its separate correction results remain as recorded above.

Independent main-gate review retains execution of the merge after the frozen
head's normal `uv run sf2 verify`, clean committed-head
`uv run sf2 verify plan --base origin/main --head HEAD`, and public CI are checked.
This acceptance preserves the graceful-EOF cleanup failure and the successful
owned-process containment boundary. It does not establish a production debugger,
original-game behavior evidence, or authorization for worktree/ref cleanup.

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
