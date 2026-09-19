# Original-reference replay scenario API

Status: **Confirmed** for the public, data-only descriptor, recursively closed schemas, deterministic
preflight receipt, and passive-observer structural policy. Runtime behavior is **Unknown**: this slice
does not start BizHawk, read a private ROM/movie/capture/receipt, or create a ledger. Evidence date:
2026-08-30.

## Confirmed public protocol

`sf2-original-reference-replay-scenario-api-v1` is a reusable protocol descriptor, not a scenario
observation or a claim of natural continuity. The core schemas accept bounded future identifiers,
addresses, fixture hashes, checkpoint roles, and timeout limits without embedding this sample's
values. They recursively close every object and reject path/raw-payload fields and path-like values.
Production cross-field checks fail closed on duplicate fixture IDs, artifact IDs/roles, or checkpoint
roles; missing fixture references; non-contiguous repeated PCs; role-order drift; and declared limits.
Filesystem catalogs and the observer path are injected only at the outer Python composition boundary.

The tracked sample intentionally names only `generic-protocol-sample` /
`generic-protocol-preflight`, marks itself **Unknown**, and carries no local path or payload. Its two
source-backed H2 anchors are a project-authored generic example, not bindings built into the reusable
schema.

The descriptor's two H2 anchors are source-backed static fixtures:

- `sf2-map3-battle01-turn-finalization-static-v1`, SHA-256
  `E50640A8BA47857B5D105FC2294F574616E13451EF919C2A2B12D9290C5DBD23`, declares the turn-finalization
  resume address `0x24106`.
- `sf2-map3-battle01-victory-return-static-v1`, SHA-256
  `84252F22EA387AF6D4D37287E10DB491786FF783A72BD1145C89F2FC92DFFE2D`, declares the victory entry
  address `0x23CBA`.

The final two sample roles share `0x23CBA`; their declared dispatch order is `victory-entry` then
`declared-terminal`. A terminal PASS contract requires `callbacksRemaining: 0`, protected finalization,
and `consoleCheckRequired: true`. Console cleanliness is outer-runner evidence only: neither the
observer nor this preflight claims that it has been observed.

The four sample artifact identities are public synthetic hashes, not movie, Input Log, header, or sync
payloads. Each is SHA-256 over UTF-8
`sf2tool/original-reference-replay-scenario-api/public-synthetic-artifact/v1:` followed by its
`artifactId`. The descriptor carries only those hashes; the deterministic derivation is tested without
materializing any artifact.

The generic observer is canonical UTF-8/LF source, SHA-256
`BA35D6F0DEC2DB79856CA1A71998E831BF13DD15120192791A9AEAB9504EDF85`. Its exact API names, bare
calls, and forbidden-capability list are immutable production constants, mirrored as schema constants;
a descriptor cannot broaden them. Static tests reject direct, aliased, and dynamic member access to
input/controller, memory/register, movie, state/SRAM, ROM, shell, and dynamic loading capabilities.
Each callback is protected; a callback or cleanup failure writes typed `caseId` (or null before
configuration), `phase`, `code`, current role/null, observed roles, expected/actual, callback count,
cleanup result, detail, and a non-zero requested exit code. Every observer text value replaces path
separators plus all ASCII controls including DEL, remains valid UTF-8, and is capped at 500 bytes (within the receipt
schema's 500-character maximum) without splitting a UTF-8 code point before JSON escaping. Runtime
failure receipts retain the validated
descriptor, scenario, transport, checkpoint, lineage, and Unknown context; only a preflight failure has
unavailable descriptor context. PASS requires zero callbacks and protected cleanup. The observer removes
every registered callback before its terminal status and emits `consoleCheckRequired: true`, not a
console-clean assertion. This is a contract implementation only; callback cadence and Lua Console
behavior remain **Unknown** until runtime observation.

## Preflight boundary

Run `uv run sf2 h3 original-reference-replay-scenario-api --preflight-only`. The maintained bootstrap
inventory declares exactly zero expected launches for this command, and its receipt reports
`ProcessStarts: 0`. It writes no scenario ledger, private receipt, movie, emulator configuration, or
derived output. Synthetic invalid-descriptor and passive-policy failures also return typed preflight
receipts with `ProcessStarts: 0`.

Lineage is identity-only: a descriptor/receipt uses `ledgerId`, `availability`, `runClass`, a bounded
`launchOrdinal`, and a `priorReceiptSha256` when one exists. Runtime is stop-loss bounded to ordinals
1–3: ordinal 1 carries no predecessor hash, while ordinals 2–3 require one; runtime declares either
`diagnostic` or `frozen-acceptance`. It never carries a filesystem path or the current receipt's
self-hash. The tracked sample and every preflight receipt remain `not-accessed-preflight` with null
run class, ordinal, and prior receipt identity; this slice creates no ledger.

The compatibility wrapper remains separately owned by
`original-reference-replay-capability`; the shared transport extraction does not alter that command's
fixture/candidate/preflight/ledger behavior. The scenario API neither reads nor modifies its private
ledger.

## H3 question queue

- **Unknown — callback and console lifecycle:** a separately admitted private runtime case must prove
  the declared checkpoint reachability, ordered shared-PC dispatch, exception-to-status/non-zero exit,
  callback removal, and clean Lua Console condition.
- **Unknown — scenario evidence:** a separately admitted case must establish any actual continuity,
  victory, endpoint, R2b observation, capture, or H4 result. This generic sample establishes none.

## Concrete transport gap and frozen material

Evidence date: 2026-09-19. **Confirmed — static implementation:**
[`load_scenario_descriptor`](../../src/sf2tool/h3/original_reference_scenario.py) checks schema,
fixture-file hashes and observer policy, but does not open any of the four input artifacts.
`_validate_static_fixture_identities` also does not resolve a checkpoint address into the referenced
fixture's owning field. The Lua observer embeds its own two addresses; changing an injected descriptor
does not rebind that table. These are missing materialization/binding checks, not runtime playback.
The hashes listed above match the current accepted sample and H2 fixture bytes; none of the four
synthetic artifact hashes is a movie identity.

The smallest real transport consumes a frozen private binding at the outer Python boundary. Extend
the existing scenario descriptor/receipt owners for this explicit variant, preserving the current
sample and its no-files/no-launch preflight. Close these inputs before reserving a launch:

| Material and owner | Binding and required check |
| --- | --- |
| ROM/runtime — existing manifest/private-input owners | Verify selected ROM against the USA manifest and pristine archive/member set against the existing toolchain contract. Record release/core, source revisions and actual binary identities. `MainForm.StartNewMovie` only warns on ROM/movie hash mismatch; the runner must reject it before launch. |
| Start — scenario Research evidence owner | Select an embedded binary-core-state BK2 as specified in the [capability owner](original-reference-replay-capability.md#source-backed-start-and-capture-boundary). Bind actual byte length/hash, producer version/core/configuration, capture boundary and setup interventions. Any separate source state must match extracted `Core` bytes. No independently loaded SRAM or state after admission. |
| Frozen input — scenario Research evidence owner | Supply real immutable BK2 and exact `Header`, `Input Log`, `SyncSettings` member bytes with lengths/hashes and complete admitted member inventory. Freeze logical-action-to-Genesis-port/button mapping, row count, startup/admission/terminal rows and limits. Check every physical row including lag/neutral rows, both ports, reset/power fields and controller/log-key agreement. No live lag compensation or action selection. A logical action list alone does not determine frame-timed BK2 bytes. |
| Configuration — outer runner | Freeze actual host config template and movie sync settings: region/VDP mode, BIOS policy, controllers, FM model/filter, overscan, draw mask, sprite-limit/padding, render/audio and capture settings. Check serialized/read-back equality. Close material identities into the candidate with existing canonical helpers; only declared launch-root path substitutions are non-semantic. |
| Checkpoints — existing R1–R4a/H2/H3 owners | Bind each role to its accepted fixture identity and exact field/source symbol/ROM address, memory domain, width/endian, occurrence and phase. Compare generated observer table against that binding. Sample `0x23CBA` is victory entry, never the 5B endpoint. Repeated PCs need bounded occurrence/order rules; adjacent roles at one PC dispatch together only when they describe the same occurrence. |
| Private output — outer runner | Allocate a new absent, contained directory below the owning worktree's ignored `local/derived/h3/`; map opaque artifact IDs to paths only there. Reject public-descriptor paths, traversal/reparse/case collisions, input/output overlap and existing targets. Inventory bytes after termination. Retain captures and failed diagnostics separately from disposable tools; capability receipt-only cleanup would delete evidence. |

No real frozen movie, accepted full start artifact, winning row sequence or scenario capture set has
been established here. Material preflight must distinguish `synthetic-input`, missing binding/input,
identity mismatch, unsupported start/capture surface and later parser/toolchain failure. Hashing an
artifact name does not verify an artifact. Public descriptors remain path/payload-free; private
bindings resolve files, while public receipts expose only reviewed identities and non-reconstructive
results. Missing material remains **Unknown** / unavailable, never an automatically generated substitute.

## Startup, clocks and passive execution

**Confirmed — static source order:** command-line movie loading precedes Shown-time Lua loading;
normal updates run `FrameAdvance → HandleFrameAfter → UpdateToolsAfter → AvFrameAdvance`.
`ProcessSavestate` resets host counters. GPGX performs `UpdateVideo` / `UpdateAudio` before incrementing
`Frame`; when `Frame == 0`, `UpdateVideo` creates an initial black buffer instead of copying the native
rendered frame. The first post-reset host frame is not an established original pixel capture. An
embedded `Framebuffer` does not fix that next-frame branch.

For this selected stock-host route, freeze a **pre-admission prefix of at least two physical rows**,
with passive registration after the first UpdateAfter and admission no earlier than the second
completed frame. The prefix and its effects belong to controlled setup and stay in provenance. The
second frame is only the earliest source-supported video-copy opportunity, not proof of controllable
Map 3. Research must freeze and verify the actual admission vector/checkpoint and prefix length. If a
required event precedes observer registration or the first rendered frame must be captured, this
route is unavailable until an earlier host hook is independently reviewed. Do not fabricate a frame,
rewrite a seed or generalize the capability's one warm-up row to every scenario.

Keep these clocks separate:

- After movie counter reset, zero-based physical BK2 row `r` drives the step from host frame `r` to
  `r+1`; runtime must verify this mapping. Input/memory callbacks precede the increment. Record
  physical row, host frame before/after, callback ordinal and phase. Logical-action index is separate.
- Frame-end images/state samples use the completed host frame; scenario-relative zero is the declared
  completed admission frame. Retain pre-roll and repeated images. Animation order/timing requires the
  image sequence and source-backed state identities; no SF2 animation-semantic API exists here.
- `gpgx_get_fps` gives NTSC `53693175 / (3420 × 262)` or PAL `53203424 / (3420 × 313)` frames/s.
  Do not round NTSC to 60 Hz. The source explicitly warns about interlacing; reached mode/field
  alignment remains **Unknown**. Host frame, VInt, scanline and master-clock cycle are distinct.
  GPGX `TotalExecutedCycles` is unimplemented; the Lua facade logs an error and returns zero, which
  must not be accepted as a timestamp.
- Native audio initializes at 44,100 sample frames/s. Each stereo sample frame has two signed 16-bit
  channel samples. Preserve each returned `nsamp` and cumulative sample-frame offset, never a fixed
  samples-per-video-frame assumption. Integer frame numbers cannot recover subframe chip-event time.

The movie owns all input. Permit only named read APIs, bounded registration, frame progression,
typed output and exit in the reviewed observer. Existing v1 policy forbids even `memory.*`, `movie.*`
and controller reads; adding read seams needs matching production constants, schema policy and
observer changes, not descriptor-controlled permissions. Every bus/execute callback must return
**no override value**: `EventsLuaLibrary.AddMemCallbackOnCore` interprets a single integer Lua return
as a replacement bus value. Forbid numeric return forwarding as well as writes, reloads, patches,
adaptive inputs and gameplay reconstruction. Validate named domains/bounds before a library fallback
can select a different domain.

## Selected 8C observation seams

All support statements here are **Confirmed only as static source/API facts** at the pinned revisions.
Actual scenario captures, deterministic repetition and natural reach remain **Unknown**. The
[RA-11 owner](map3-battle01-audit.md) and ADR 0010 still require every selected reached domain;
an unavailable domain blocks full 8C. Raw frames, palette/VRAM snapshots, waveform/chip traces and
detailed receipts remain private. The [source inventory](#pinned-source-inventory) identifies each seam.

| Selected domain | Existing seam and data/clock | Deterministic conditions and precise limit |
| --- | --- | --- |
| Pixels/composed palette | `GPGX.IVideoProvider.GetVideoBuffer`, `BufferWidth/Height`: packed 32-bit pixels; consume exactly width × height cells, not spare backing capacity. With OSD off, `client.screenshot(path)` reaches `MakeScreenshotImage`: unscaled core-buffer PNG with alpha discarded. Syncless can write frame-numbered PNGs. | Freeze layers, original sprite limit, padding/overscan/color settings; no overlays, resize, dropped render or accessibility deviation. Compare decoded pixels in a declared channel/alpha order, not PNG encoder bytes. First-frame caveat applies. Digital emulator output does not establish analog hardware equivalence. |
| Palette storage | `memory.read_bytes_as_array(..., "CRAM")`: 1-indexed byte table; 128 bytes converted by GPGX from internal colors to 64 native big-endian words. Key by frame/PC phase. | Check actual domain/size; read only. Frame-end palette cannot prove raster-time writes, application time, shadow/highlight composition or all rendered colors. `genesis.*` has layer/deep-freeze controls, no palette-event API. |
| Frame cadence/animation | `FrameAdvance`, `emu.framecount`, `event.onframeend`, rational Vsync and ordered images; accepted SF2 PC/RAM seams identify animation state. | Force rendering every captured step; no turbo/rewind/run-ahead/skipping. Freeze origin. Game animation identity and interlace changes require source/state bindings; an input poll or image alone is not an animation clock. |
| Waveform/audio timing | `gpgx_advance → audio_update → gpgx_get_audio → UpdateAudio → GetSamplesSync`: interleaved signed 16-bit stereo at 44,100 sample frames/s, variable `nsamp` per step. | Freeze FM/filter/preamp and initial audio state. `GetSamplesSync` consumes the pending count; no second draining consumer. These Lua libraries have no raw PCM getter. Stock export exists but joint frame/sample alignment has the gap below. Mixed PCM does not establish chip history. |
| YM2612/PSG behavior | Native `sound.c` / `memz80.c` have cycle-bearing `fm_write` / `psg_write`; SF2 sound owners provide command/channel seams. GPGX registers include Z80 CPU state, while its callback service advertises only `M68K BUS`. | No complete Z80/FM/PSG timestamped write stream or chip-state schema in `LibGPGX`/Lua. Frame-sampled Z80 RAM/mailboxes cannot recover history. Full selected chip/timing capture is unavailable through this stock public boundary. |
| VInt/interrupt state | `event.onmemoryexecute` at `tests/fixtures/h3/controller-input-v1.json` → `sourceContext.vIntEntryAddress`, cross-checked with H2 `tech-interrupts-static-v1.json` → `function.VInt`, plus register/68K RAM reads; the [debug-bridge owner](../operations/bizhawk-debug-bridge.md) names the seam. Hits have host frame/callback order. | Observe original handler reach, not a synonym for frame end. Native `system_frame_gen` schedules VINT with `vint_cycle` and frame-relative master cycles, but does not export their event timestamps here. Assertion/acknowledgement latency and hardware phase remain unavailable. |
| DMA/CRAM/VDP transactions | Bounded M68K bus write callbacks see CPU requests to source-backed VDP ports. Native `vdp_ctrl.c` / `system.c` own transfers. CDL has a `DMASource` flag ORed per source byte. | Managed events have address/value/access-kind, not width/master-cycle time or DMA destination/completion. CDL loses order/time/multiplicity. `gpgx_peek_m68k_bus` returns `0xFF` where a read handler is needed; polling MMIO is not live VDP status. Exact transaction chronology is unavailable. |
| VRAM/scroll/VDP composition | Read-only `VRAM` (65,536 bytes), `CRAM` (128), `VSRAM` (128 exposed bytes), with GPGX byte-order adapters and domain-relative offsets. Host `UpdateVDPViewContext` exposes VRAM/pattern/color-cache pointers and name-table geometry. | Exposed VSRAM size is emulator storage, not 128 hardware-visible bytes. The VDP view flushes caches and has no Lua binding; it is not a register/event trace. No VDP register domain is declared. Snapshots do not establish raster scroll, sprite fetch, FIFO or DMA timing. |
| Other actually reached surfaces | Reuse accepted SF2 register/RAM/input-poll seams for flags, readiness, timer/RNG spans, scene/audio requests and endpoint predicates; fixture-owned widths/roles. | The complete winning route is unobserved, so its additional hardware inventory is **Unknown**. Freeze required domains/ranges/clocks before capture; unexpected reached behavior leaves coverage incomplete. No unrelated driver/CD/SMS/optional-content audit or electrical-input expansion under ADR 0005/0010. |

### A/V export is not automatically an exact capture

The existing native command can, in a later admitted runner, add `--dump-type=syncless`, a contained
`--dump-name`, fixed `--dump-length` and explicit `--audiosync=true`. With `AviCaptureOsd=false`,
`AviCaptureLua=false` and resize dimensions zero, `GetCaptureProvider` returns the core video provider.
`SynclessRecorder.SetFrame` names each PNG/WAV pair by `Emulator.Frame`; its project file does not
retain rational Vsync or capture origin, so the outer receipt must.

`MainForm.RecordAv` always wraps that writer. Audio-synced `VideoStretcher` passes through synchronous
sample counts but may drop/repeat `AddFrame`; syncless repeats overwrite the same frame path. With
audio sync off, `AudioStretcher` requests rationally sized blocks from `SyncToAsyncProvider`, losing
original per-step sample boundaries. Neither mode alone proves unmodified joint cadence.
`AvFrameAdvance` occurs after Lua/tool updates, so exiting at a terminal callback can omit its capture.
AV exceptions show a modal message and abort the writer, without guaranteeing a typed non-zero result.

**Inferred — minimum exact A/V host change:** copy each raw video rectangle and synchronous audio block
once at the existing completed-step boundary, before stretcher/host sound consumption; stamp both with
host frame and cumulative sample offset; finalize after terminal-frame flush. Reuse PNG/WAV writers,
not a general recording service. The exact usable hook and source/binary pin remain **Unknown** for the
released runtime. Do not guess a reflection hook, call `GetSamplesSync` twice, use wall-clock recording,
remove duplicates after capture or repack the pinned binary in the offline transport slice.

Full 8C also needs bounded chip/VDP/interrupt/DMA event exports with domain, access width, value and a
defined emulated clock. Native `core/debug/cpuhook.h` includes Z80/VRAM/CRAM/VDP-register hook kinds;
enum existence does not expose them through the managed `M68K BUS` service. A native/managed extension
changes the toolchain and requires a separately pinned, licensed and independently reviewed tooling
decision. Launching stock 2.11.1 cannot supply an API feature absent from this boundary.

## Typed lifecycle and containment

Reuse these existing mechanisms with concrete changes, not a second launch framework:

- `original_reference_transport.py` owns canonical JSON, file identity, canonical UTF-8/LF observer
  bytes and passive-source checks. Reuse them without changing their behavior. Reuse compatible
  capability archive/containment helpers by import in the offline step; do not refactor the frozen
  runner merely for API tidiness. `_input_rows`, `materialize_movie`,
  `_config_template`, `_candidate_identity`, terminal checks and `_safe_delete_launch_files` have
  fixed capability assumptions and cannot serve scenarios unchanged.
- Reuse `run_native_bizhawk_process` from `h3/bizhawk.py` for argument-list subprocess execution,
  timeout and process-tree termination. Preserve reservation-before-start and immutable predecessor
  receipt checks. Data-only preflight never creates a ledger, marks a launch or fills missing lineage.
- Generate only a bounded literal checkpoint/read table from the validated descriptor. No gameplay
  logic or `dofile` bootstrap. Validate resulting bytes/policy and compile syntax through the existing
  pinned Lua mechanism. Reject null, empty and all-zero callback IDs; protect every callback/finalizer
  with `pcall`, retain failed unregister IDs and finalize once. Queue terminal reach until its defined
  frame/output boundary; do not exit successfully from a mid-frame callback before capture completes.
- Extend the scenario receipt's observer-only runtime shape with independent materialization, process,
  observer, capture, console and cleanup results. Preserve case/role/phase, expected/actual, first
  failure and subsequent cleanup failures. Distinguish malformed/missing status, callback exception,
  registration/order/timeout failure, requested/actual exit code, missing/truncated/drifted capture,
  residual process/callback and cleanup failure. Unobserved facts are null, never zero/false; start
  failures remain distinct from post-start failures.
- Console acceptance is outer observation of actual Lua Console output and registered functions before
  teardown. `consoleCheckRequired:true`, zero exit, empty stdout and the observer's own callback list
  do not prove it. `NamedLuaFunction.Call` sends sandbox errors to its log callback; `LuaConsole`
  maintains `OutputBox` and per-file function lists. No accepted scenario runner reads that evidence.
  Reuse a suitable owned console observation, or leave it unavailable pending a bounded readback hook;
  do not clear the console to make it clean.
- Check input/archive integrity and collect the completed private capture inventory before cleanup.
  Remove only disposable contained files; preserve inputs, captures, receipt and failures, then record
  residuals. Typed failure, unavailable mandatory surface or cleanup failure prevents runtime PASS.
  A passing original-reference receipt still is not H4.

## Smallest follow-up and admission conditions

The next independently owned implementation can be **offline material binding and observer generation
only**, without recovering the ledger; runtime stays disabled. Proposed exact owned files, after
main-gate transfers any shared paths:

| Files | Concrete change |
| --- | --- |
| `src/sf2tool/h3/original_reference_scenario.py` | Add `preflight_scenario_transport(binding_path, output_root)` at the private outer boundary. Check real movie/member/start/config/checkpoint identities, generate contained observer/config copies and return a material receipt with `ProcessStarts: 0`. Reject name-only synthetic artifacts. Never call the native process helper or ledger writer. |
| Read-only dependencies: `src/sf2tool/h3/original_reference_transport.py`, `src/sf2tool/h3/original_reference_replay.py`, `src/sf2tool/h3/bizhawk.py` | Reuse canonical identity/policy checks, compatible archive/containment helpers and `validate_lua_syntax` by import. Keep scenario orchestration in its owner; no shared-kernel rewrite or new counter is needed for this offline boundary. |
| `schemas/core/original-reference-replay-scenario-api.schema.json`, `schemas/core/original-reference-replay-scenario-receipt.schema.json` | Add an explicit closed material-transport variant for embedded state, concrete configuration/checkpoint bindings and independent material/capture availability. Keep generic v1 power-on/sample receipts unchanged. Define the closed private binding shape in the same owning Python module; no local paths in the public descriptor. |
| `tools/bizhawk/original_reference_scenario_transport_observer.lua` (new bounded template), these two research owners | Generate the read-only transport variant from this template, reusing the current protected-finalizer structure. Keep the old generic observer bytes/hash unchanged; document materialization and unavailable runtime/capture surfaces. No new fixture identity, registry, counter, recording service, production-engine path or test-of-runner layer is needed for this offline step. |

Before that slice becomes Ready, its executor must declare the exact variant fields and trusted
expected-identity source, immutable input/output selection, public/private projection, observer API
allowlist and capability-compatibility checks from the rows above. Missing real state is unavailable
material; synthetic data can exercise rejection/shape checks but cannot pass as usable core state.
Exact state/trace values and a native capture/console hook remain **Unknown**. They gate runtime
readiness, not a correctly bounded offline API.

Direct commands for that proposed implementation (only the first currently exists; the others require
the named new entry point and declared ignored binding files):

```powershell
uv run sf2 h3 original-reference-replay-scenario-api --preflight-only
uv run python -c "from pathlib import Path; from sf2tool.h3.original_reference_scenario import preflight_scenario_transport; print(preflight_scenario_transport(Path('local/scenario-transport/binding.json'), Path('local/derived/h3/scenario-transport-preflight')))"
uv run python -c "from pathlib import Path; from sf2tool.h3.original_reference_scenario import preflight_scenario_transport; print(preflight_scenario_transport(Path('local/scenario-transport/synthetic-rejection.json'), Path('local/derived/h3/scenario-transport-rejection')))"
```

Run once per fresh output directory. The first must retain the generic public receipt. The material
command checks actual bytes or returns typed unavailable material; the rejection command fails closed
with zero launches. Directly exercise missing/wrong member, checkpoint mismatch, forbidden callback
return/API and containment rejection using local variants; do not add tests of validators/runners.
The capability hashes its runner and native helper files: editing them changes its candidate even
when behavior is equivalent. Any necessary later refactor needs declared ownership, a new candidate
identity and preserved lineage, never a claim that the old hash survives. Shared transport behavior
also requires review even though its file is not a separate current candidate component. Keep all of
these read-only for this smallest step; close every consumed dependency into the scenario identity.
These examples authorize no normal/full suite or emulator run;
the committed planner still needs scope interpretation.

### Runtime admission dossier

[ADR 0014](../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)'s three-part gate
applies to each question below. These are conditional dossiers, not launch permission:

| Caller-dependent question | Static insufficiency and acceptance impact | Rail reuse and boundary |
| --- | --- | --- |
| Does the frozen state/prefix admit controllable Map 3 with correct input/frame origin, callbacks and console lifecycle? | Source order cannot prove state-byte compatibility, reached caller or console results; these determine 1A and all reference provenance. | Reuse capability containment and R1/R2 fields; another runner alone does not justify a new fixture. Genuine full capability lineage recovery, independent disposition of the reported ordinal-2 FAIL and accepted capability runtime come first. Scenario execution needs explicit main-gate lineage/budget adjudication and frozen material; stop at first typed failure. |
| Does the original route carry real caller/accounting/RNG state through Battle01 and the natural 5B return? | R2b/R2c/R3/R4a topology and R2d's explicit bridge do not prove natural reach, winning branches or readiness; these determine 2A/3A/4A/5B. | Extend the admitted scenario using R1/R2/R2a/R2d/R4a fields, omitting the state-writing bridge. No separate fixture passes gate part 3 until Research shows why the existing batched rail cannot cover it. Full start/trace/endpoint binding remains required before launch. |
| Are reached pixels/palette/audio/frame clocks aligned and all required hardware events present? | Static APIs do not prove buffer/callback timing or deterministic capture; this determines RA-11/8C separately from gameplay. | Reuse that same scenario and graphics/sound/interrupt owners. Separate per-domain launches lack part-3 justification. Missing stock clocks/exports are a tooling blocker: a bounded independently accepted interface/pin decision must precede observation. Do not spend a diagnostic to rediscover an absent API. |

Preserve consumed capability ordinal 1 and the recovered coordination reports of ordinal-2 timeout FAIL in
the [capability owner](original-reference-replay-capability.md#current-lineage-hard-stop).
Actual complete ledger/receipt bytes remain unavailable, and the contemporaneous prohibition of ordinal
3/retry/reset remains unresolved. Saved output is coordination, not original evidence. No ordinal may
be assumed available; recovery alone cannot convert ordinal-2 FAIL into the same-candidate PASS needed
for ordinal 3. Main-gate must independently resolve the full lineage/budget; no extra diagnostic,
fourth launch or task/runner reset is authorized. Preserve failed
R2b/capability history. #431's completed failures/corrections and #434's independent Chinese-label
failure are neither rerun nor relabeled here. The next boundary is independently reviewed offline
materialization or an explicit missing-interface blocker, not a scenario/H4 launch. Main-gate owns
toolchain/admission decisions; Design owns the later H4 domain/clock/tolerance definition without
lowering 8C.

## Pinned source inventory

The official sources below were read statically on 2026-09-19. BizHawk revision `B` is
`bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5`; its Genesis Plus GX gitlink revision `G` is
`051d430d3d1b54625f9900c8f152d7f232e06daf`. These identify inspected source, not a reproduced binary
build; the manifest identifies the admitted release archive separately.

| Claim / symbols | Official pinned source |
| --- | --- |
| Startup/render/audio/capture order and CLI | [MainForm.cs](https://github.com/TASEmulators/BizHawk/blob/bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5/src/BizHawk.Client.EmuHawk/MainForm.cs), [MainForm.Movie.cs](https://github.com/TASEmulators/BizHawk/blob/bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5/src/BizHawk.Client.EmuHawk/MainForm.Movie.cs), [ArgParser.cs](https://github.com/TASEmulators/BizHawk/blob/bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5/src/BizHawk.Client.Common/ArgParser.cs) |
| BK2 start-state parsing and counter reset | [Bk2Movie.IO.cs](https://github.com/TASEmulators/BizHawk/blob/bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5/src/BizHawk.Client.Common/movie/bk2/Bk2Movie.IO.cs), [IMovie.cs](https://github.com/TASEmulators/BizHawk/blob/bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5/src/BizHawk.Client.Common/movie/interfaces/IMovie.cs), [MovieSession.cs](https://github.com/TASEmulators/BizHawk/blob/bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5/src/BizHawk.Client.Common/movie/MovieSession.cs), [BinaryStateLump.cs](https://github.com/TASEmulators/BizHawk/blob/bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5/src/BizHawk.Client.Common/savestates/BinaryStateLump.cs) |
| Video/audio/frame/state providers | [GPGX.IVideoProvider.cs](https://github.com/TASEmulators/BizHawk/blob/bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5/src/BizHawk.Emulation.Cores/Consoles/Sega/gpgx64/GPGX.IVideoProvider.cs), [GPGX.ISoundProvider.cs](https://github.com/TASEmulators/BizHawk/blob/bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5/src/BizHawk.Emulation.Cores/Consoles/Sega/gpgx64/GPGX.ISoundProvider.cs), [GPGX.IEmulator.cs](https://github.com/TASEmulators/BizHawk/blob/bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5/src/BizHawk.Emulation.Cores/Consoles/Sega/gpgx64/GPGX.IEmulator.cs), [GPGX.IStatable.cs](https://github.com/TASEmulators/BizHawk/blob/bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5/src/BizHawk.Emulation.Cores/Consoles/Sega/gpgx64/GPGX.IStatable.cs) |
| Domains, missing cycle API and settings | [GPGX.IMemoryDomains.cs](https://github.com/TASEmulators/BizHawk/blob/bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5/src/BizHawk.Emulation.Cores/Consoles/Sega/gpgx64/GPGX.IMemoryDomains.cs), [GPGX.IDebuggable.cs](https://github.com/TASEmulators/BizHawk/blob/bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5/src/BizHawk.Emulation.Cores/Consoles/Sega/gpgx64/GPGX.IDebuggable.cs), [GPGX.ISettable.cs](https://github.com/TASEmulators/BizHawk/blob/bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5/src/BizHawk.Emulation.Cores/Consoles/Sega/gpgx64/GPGX.ISettable.cs) |
| VDP view, exported interface and CDL | [GPGX.cs](https://github.com/TASEmulators/BizHawk/blob/bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5/src/BizHawk.Emulation.Cores/Consoles/Sega/gpgx64/GPGX.cs), [LibGPGX.cs](https://github.com/TASEmulators/BizHawk/blob/bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5/src/BizHawk.Emulation.Cores/Consoles/Sega/gpgx64/LibGPGX.cs), [GPGX.ICodeDataLogger.cs](https://github.com/TASEmulators/BizHawk/blob/bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5/src/BizHawk.Emulation.Cores/Consoles/Sega/gpgx64/GPGX.ICodeDataLogger.cs) |
| Lua callbacks/override returns, reads and helpers | [EventsLuaLibrary.cs](https://github.com/TASEmulators/BizHawk/blob/bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5/src/BizHawk.Client.Common/lua/LuaHelperLibs/EventsLuaLibrary.cs), [MemoryLuaLibrary.cs](https://github.com/TASEmulators/BizHawk/blob/bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5/src/BizHawk.Client.Common/lua/CommonLibs/MemoryLuaLibrary.cs), [GenesisLuaLibrary.cs](https://github.com/TASEmulators/BizHawk/blob/bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5/src/BizHawk.Client.Common/lua/LuaHelperLibs/GenesisLuaLibrary.cs), [EmulationApi.cs](https://github.com/TASEmulators/BizHawk/blob/bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5/src/BizHawk.Client.Common/Api/Classes/EmulationApi.cs) |
| A/V writer and conversion | [SynclessRecorder.cs](https://github.com/TASEmulators/BizHawk/blob/bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5/src/BizHawk.Client.EmuHawk/AVOut/SynclessRecorder.cs), [AVSync.cs](https://github.com/TASEmulators/BizHawk/blob/bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5/src/BizHawk.Client.EmuHawk/AVOut/AVSync.cs), [SyncToAsyncProvider.cs](https://github.com/TASEmulators/BizHawk/blob/bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5/src/BizHawk.Client.Common/Sound/Utilities/SyncToAsyncProvider.cs) |
| Console errors and registered functions | [NamedLuaFunction.cs](https://github.com/TASEmulators/BizHawk/blob/bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5/src/BizHawk.Client.Common/lua/NamedLuaFunction.cs), [LuaConsole.cs](https://github.com/TASEmulators/BizHawk/blob/bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5/src/BizHawk.Client.EmuHawk/tools/Lua/LuaConsole.cs) |
| Native FPS/audio/domain/MMIO boundary | [cinterface.c](https://github.com/TASEmulators/BizHawk/blob/bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5/waterbox/gpgx/cinterface/cinterface.c) |
| Native event/port timing; not managed exports | [system.c](https://github.com/TASEmulators/Genesis-Plus-GX/blob/051d430d3d1b54625f9900c8f152d7f232e06daf/core/system.c), [vdp_ctrl.c](https://github.com/TASEmulators/Genesis-Plus-GX/blob/051d430d3d1b54625f9900c8f152d7f232e06daf/core/vdp_ctrl.c), [sound.c](https://github.com/TASEmulators/Genesis-Plus-GX/blob/051d430d3d1b54625f9900c8f152d7f232e06daf/core/sound/sound.c), [memz80.c](https://github.com/TASEmulators/Genesis-Plus-GX/blob/051d430d3d1b54625f9900c8f152d7f232e06daf/core/memz80.c), [cpuhook.h](https://github.com/TASEmulators/Genesis-Plus-GX/blob/051d430d3d1b54625f9900c8f152d7f232e06daf/core/debug/cpuhook.h) |

Reproduce source identity without launching a runtime:

```powershell
gh api repos/TASEmulators/BizHawk/git/ref/tags/2.11.1 --jq '.object'
gh api repos/TASEmulators/BizHawk/contents/waterbox/gpgx/Genesis-Plus-GX?ref=bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5 --jq '{sha,submodule_git_url}'
```

Read the linked files at those exact commits; save research copies only in explicit worktree-local
ignored scratch. Do not vendor or republish source or binaries. For this documentation-only audit,
check links/anchors/tables/fences, sample/H2 identities, source-symbol support, declared path ownership
and private boundary. Inspect clean committed `uv run sf2 verify plan --base origin/main --head HEAD`
under the documentation-only policy; no normal/full, SDK, Godot or H3 run is required. This audit
produces no runtime or H4 result.

## Reproduction

The existing public protocol reproduction remains `uv run sf2 h3 original-reference-replay-scenario-api
--preflight-only` and `uv run pytest tests/python/test_original_reference_scenario.py -q`.
Both use tracked public inputs only. No H3 fixture registration, evidence counter, research-index entry,
or private artifact is added by this slice.
