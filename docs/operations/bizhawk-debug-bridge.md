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
