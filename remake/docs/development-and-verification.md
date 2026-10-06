# Remake Development and Verification

## Scope

### Retired bulk frame workflow

The user's 2026-10-04 decision retires the previous whole-route per-frame H4 capture,
processing and audit workflow. Its historical recipes below do not authorize another bulk run,
processing pass or frame-by-frame audit. The old C comparison was terminated by user retirement;
D was already cancelled. Large raw frame captures and reader-derived databases from the named
#534 C/D and #605 allocations are authorized for disposal under the exact inventory in #615.
Cleanup status belongs to that Issue; authorization is not proof that deletion completed.
Small failure/acceptance records, necessary reproduction slices and published compact reports
remain retained. Original ROMs, save states, source and asset libraries are outside that cleanup.

Future work starts from the missing behavior or evidence relation, reuses accepted static rules
and bounded results, and observes necessary logical actions, state changes and actual consumer
boundaries. Use the [existing scale plan](#observation-and-comparison-planning) before acquisition.
Per-frame measurements remain appropriate only for a named timing claim over a bounded interval;
they are not the default format for whole-route state or resource evidence. Retiring this workflow
does not turn unresolved assertions into PASS or erase the completed historical failures.

On this host, future mutable execution uses the selected H-drive checkout and its local outputs,
TEMP, Python/NuGet environments and caches. Exact paths stay in ignored machine configuration.
Existing shared read-only inputs and tool installations are reused. The former C-drive execution
checkout is retained history, not a destination for new work; do not migrate its retired bulk data
into the new checkout merely to continue the old workflow.

Current product acceptance uses the [explicit default-keyboard scope](#current-keyboard-comparison-scope).
Older four-variant recipes below are retained history, not authorization to run B/D or require C.

This document owns current remake commands and verification selection. The user's test policy,
recorded in [ADR 0019](../../docs/decisions/0019-state-and-content-driven-remake-engine.md), controls
over older blanket gate and test-preservation recipes:

- Add small unit tests of actual new-engine behavior, with independent expected values and meaningful
  state/configuration variation. A small session using real reducers is a useful unit boundary.
- Reference comparisons, validation, probes, fixture drivers, planners, gates, reports and test helpers
  are themselves verification. Use them directly when needed; do not add tests of those programs.
- Migrate useful behavior assertions or retire obsolete tests with their owning capability. Test
  counts, a replacement for every deletion, and a green old aggregate are not acceptance targets.
- Documentation-only changes use direct link/anchor/fence/table/example, scope and private-boundary
  checks. They do not require a new normal/full, .NET, Godot or H3 run.

The engine unit project covers actual Domain/Application/Content behavior for four authored battle
packages, two connected exploration packages and the bounded private entries, with scoped engine/adapter entries and public jobs. The native observation directly drives
the same session with Godot input and reads real state/HUD/nodes; it emits no images. Main-gate owns required-check configuration during integration; a candidate must report its
actual CI outcome and that configuration boundary explicitly.
[ADR 0012](../../docs/decisions/0012-dependency-aware-partitioned-verification.md) continues to own
affected research evidence selection. Shared research changes keep their separate owning requirements.

Godot acceptance observes the actual running instance: Application snapshots, commands, presenter
projections, node geometry/visibility/focus where relevant, and process errors. Screenshots, image
comparison and frame-inspection recipes are prohibited. Existing completed images remain historical
artifacts. A probe that still emits images needs that side effect disabled in an owned change before
a future required run. A remote debug server is not an established dependency; first use an existing
native probe or suitable debug/state interface that answers the concrete question.

Reuse the same verified Godot installation, owning project and running debug instance. A concrete
failure, necessary code/import restart, observed contamination, or test of startup/export/cleanup can
justify a bounded restart or isolated instance; record the reason. A new slice or comparison does not
justify copying the project, extracting another editor, or building a debug framework.

## Observation and Comparison Planning

Before a large or expanded capture/comparison, apply the
[scale-planning owner](../../docs/operations/bounded-inspection-and-review.md#plan-before-scaling).
Declare the claim and minimum sufficient granularity, input/intermediate/output cardinalities,
algorithmic cost, resource budgets, stages, concurrency and publication headroom. Reuse relevant
measurements or a representative small pilot before the full run. Evaluate native collection and
offline comparison separately: acceptable frame time, an indexed database or a bounded writer queue
does not establish acceptable whole-run storage or join cost. Include repeated resource uses,
requirement/candidate products, copies, journals and final report companions in the estimate.

For future probes, start ordinary CPU/memory/progress and coarse state sampling around1Hz, while
retaining necessary semantic and actual-consumption events at their originating boundaries with
occurrence/order identity. Choose finer sampling for a named claim; do not miss a short event by
sampling once per second. A draw callback may itself occur every frame for many resources, so
event-driven collection still needs an explicit total-volume estimate.

Reuse stable descriptors and compact unchanged uses into counts or delimited spans only when the
accepted predicate permits it and completeness, changes and contradictions remain auditable.
Per-frame motion/camera/draw evidence needs a bounded interval and byte budget. Frame-time
percentiles may require per-frame timing statistics, not per-frame full-state serialization.
Sampling, serialization and flush rates are separate: batched writes preserve all required events,
overflow/error behavior and complete terminal records.

Select offline scope before expensive import and derivation, preserving required context and
explicitly marking unselected or missing obligations. Estimate peak logical bytes separately from
physical volume headroom; compression does not remove redundant work. Preserve diagnostics and
replan when observed costs materially exceed the estimate. These design rules do not alter existing
captures, acceptance predicates, running-process control or cleanup authority. A guidance-only change
uses direct document checks and does not trigger native, SDK, normal/full or H3 verification.

## Locked .NET Workflow

Load the existing ignored host/worktree environment before every SDK command, including informational
commands. Select one existing absolute `DOTNET_BIN` and one absolute `DOTNET_CLI_HOME` shared by this
project's worktrees. Force `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false` at the actual child launch.
The ignored machine configuration places complete .NET and Godot installations beneath its selected
shared tool root; `GODOT_BIN` selects the editor and `$godotBinary = $env:GODOT_BIN` supplies the
examples below. Reuse those installations without repeating setup or gates merely to change paths.
The maintained .NET helper preserves valid local NuGet selections, rejects external cache paths and
selects worktree-local TEMP/TMP. See the [shared tool owner](../../docs/operations/local-private-inputs.md)
for the BizHawk runtime-copy exception and preparation/runtime distinction.
The maintained entrypoints reject missing/relative selections. No launch may change registry PATH.

The SDK otherwise adds its tools directory by default; a new CLI home can repeat first-run setup.
`DOTNET_SKIP_FIRST_TIME_EXPERIENCE` does not supply the PATH protection. The
[official variable reference](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-environment-variables#dotnet_add_global_tools_to_path)
owns that SDK behavior. Use the SDK pinned by [global.json](../global.json); do not install another one
to configure sharing or make a check pass. Keep NuGet packages/HTTP cache worktree-local, outside the
shared CLI home, and preserve the tracked package versions and locks.

For an affected current product project, run from `remake/` after that setup. For example, when
Domain itself is affected, select its existing project explicitly:

```powershell
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
$project = 'src/Sf2.Remake.Domain/Sf2.Remake.Domain.csproj'
& $env:DOTNET_BIN restore $project --locked-mode
& $env:DOTNET_BIN build $project --configuration Release --no-restore
```

From the repository root after loading that environment, the maintained entries are:

```powershell
uv run sf2 verify engine
uv run sf2 verify adapter
```

`verify engine` restores in locked mode, builds and tests
`remake/tests/Sf2.Remake.Engine.Tests/Sf2.Remake.Engine.Tests.csproj`; it references production
Domain/Application/Content and copies the actual `remake/content/authored/*.json` inputs.
`verify adapter` restores in locked mode and builds `remake/game/Sf2.Remake.Godot.csproj` without tests
or native Godot. These commands launch the selected executable from `remake/`, honoring `global.json`
and the protected environment. Neither needs private inputs. `Sf2.Remake.sln` contains the four
production projects and Engine.Tests; the legacy test projects and reference host were retired at M5. Use formatting only for affected product projects when needed.

## Ordinary field action and warp service

An admitted field `Move` records its pending direction in the existing `EntityWait`
and emits `movement-requested`. The next `AdvanceSimulation` consumes it at the
player's physical slot in `EntityActionRunner`, after that slot's motion update.
It rechecks current/reserved entity obstruction, opens any reached door, resolves
the warp marker before passability, and installs allowed travel. Remaining slots
use that old scene and the player's new reservation. A reached warp is dispatched
only after the complete pass; it does not add a second entity update.
The [source owner](../../docs/research/map3-controlled-start-egress-transition.md#entity-service-before-the-warp-caller)
records the original caller order separately from this engine implementation.

Destination admission reuses the pure `MapTransfer.Apply` construction before
queueing a direct warp, including the candidate door/layout state. Invalid
destinations preserve the original active world/story/RNG and publish no partial
movement or warp observations; the existing failure contract stops the session.
Preserve retains the updated old population. Rebuild carries its resulting party
seed into the destination population, whose actors have not yet received a pass.
Destination onLoad and existing scripted `TransferToMap` keep their program order.
A transfer instruction itself contributes no field pass; a field-triggered warp
program follows the producing pass before executing that instruction.

Pending moves reject cancellation, replacement and unrelated acknowledgements.
The current revision and wait token guard delivery; consuming the direction clears
it while ordinary travel retains the same entity-wait token until arrival. A warp,
a newly obstructed action or arrival stops the current simulation batch at its
control boundary. Repeated/stale tokens cannot start the action again. A move
already obstructed at submission retains the existing immediate blocked response
without an entity opportunity. This admission policy does not claim the original
idle-input clock.

**Confirmed (engine behavior):** `ExplorationSessionTests` varies NPC wait expiry,
draw/no-draw/idle state, seeds, physical slots, collision reservations, blocked
marker travel, invalid destinations, Preserve/Rebuild, batch size, pending/stale
delivery and scripted/onLoad callers. Existing `MapEntityLifecycleTests` and
`CommandsetContinuationTests` cover consumers of the reused entity wait.
From `remake/`, after the protected environment above, the affected selection is:

```powershell
& $env:DOTNET_BIN test tests/Sf2.Remake.Engine.Tests/Sf2.Remake.Engine.Tests.csproj `
  -p:RestoreLockedMode=true `
  --filter 'FullyQualifiedName~ExplorationSessionTests|FullyQualifiedName~MapEntityLifecycleTests|FullyQualifiedName~CommandsetContinuationTests'
```

## Ordinary warp visible return

A direct ordinary warp now keeps its old scene through finite full-black FadeOut.
It idles the player's action script without clearing travel. Each enabled fade
service runs before the physical entity slots, including the terminator. Logical
completion and actual black delivery join before the two explicit disabled-context
load waits and Preserve/Rebuild transfer. The new scene is mounted black; existing
onLoad programs run before the BASE/CURRENT colors 2+3 comparison selects FadeIn.
That helper includes ExecuteFading's extra service. Field control requires both
logical and actual visible completion. These services implement the
[source helper contract](../../docs/research/map3-controlled-start-egress-transition.md#finite-full-black-helpers-and-visible-return),
not a fixed total interrupt quota.

`FullFadeWait` records period/countdown/table entry, the helper's extra service,
and separate logical/actual completion. Early actual receipts cannot skip work;
late receipts add no entity/RNG work. Duplicate, wrong-kind and stale tokens are
rejected. A batch stops at each new token/phase. The host clears unused time at
those boundaries and after hide, tree pause or real focus loss. Ordinary WarpOut
has no extra service; script full-black helpers and WarpIn do. Temporary periods
restore after the extra service. The logical pair is derived from live BASE;
Preserve retains BASE and Rebuild loads the target map's bound BASE.

Inputs explicitly supply `start.display` (`period`, `base`, `current`,
`visibility`), with pairs shaped as `{color2, color3}` and visibility `black` or
`base-restored`. Period must be 1..255 and words use supported CRAM bits. Missing
state does not default to period 3. Authored maps may embed `basePalette`.
For a retained private world, the small start may instead contain
`mapPalettes: [{map, base}]`; the reader normalizes this once into the same typed
map definitions. Duplicate/unknown entries and any embedded-plus-start binding
are rejected, even when equal. There is no runtime overlay lookup or reference
checkpoint dependency. Bind only maps required by the supported entry.

The selected R1 input has a separately proven explicit period 3, restored initial
pair and visibility; see the source owner's bounded retained-RAM/writer join and
initialization-prefix proof. `selected_map_palette_bindings` in the existing
compiler reads only two palette words and adds its source paths to existing
provenance. It exports no textures/audio and does not copy the private world.
Direct reproduction after selecting the clean pinned source as `upstream`:

```python
from sf2tool.remake_exploration_content import OriginalPrograms, selected_map_palette_bindings
compiler = OriginalPrograms({"resources": {"standaloneScriptPrograms": [], "initSourcePrograms": []}}, upstream)
map_palettes = selected_map_palette_bindings(compiler, [3])
# A real source producer exercising normal and temporary-period forms:
compiler.register_file("disasm/data/maps/entries/map07/mapsetups/s6_initfunction.asm")
compiler.compile("cs_55832")
full_black = [row for row in compiler.programs["cs_55832"]["instructions"] if "fullBlack" in row]
assert [row["fullBlack"]["period"] for row in full_black] == [6, None]
```

The compiler retains `fadeInB`, `fadeOutB`, `slowFadeInB`, `slowFadeOutB` as explicit
synchronous full-black cues, with current or temporary 6 period. Generic visual
cues do not imply logical palette effects. Bound white FadeIn/FadeOut uses its finite map-white
profile below; partial/tint/restoration cues remain Unsupported; asynchronous
scene-load composition inside ordinary onLoad is also Unsupported. Known static
missing transition bindings fail before the producing move. A reached dynamic
failure preserves consumed ticks, map, flags, cursor and pending transition state;
a separate unfaded CanvasLayer keeps its explanation readable. Equal-but-black
return is Unsupported. An onLoad full FadeIn may establish equal-and-restored and
skip a duplicate helper. Explicit transfer/battle callers keep their own path;
onLoad replacement retires the abandoned ordinary continuation and token.

**Confirmed (engine and actual host):** the affected tests cover periods 1/3/6,
NPC phases/slots and terminal RNG draws, Preserve/Rebuild, color-3-only inequality,
equal-black rejection, init waits, temporary-period restore, replacement,
retained failures and early/late/duplicate/stale/batched delivery. The bounded
`warp-transition*` cases in the existing input observer drive real Main/input and
read live state/projection. Cases include normal/reduced private R1 first warp,
authored rebuild, period-1 late actual receipt, period-6 early receipt, an onLoad
restoration with no duplicate fade, black failure text, and hide/tree-pause/real
native-window focus transfer plus a delayed callback. No screenshots, original
emulator run or full-route claim is involved. Private normal/reduced runs have
identical ordered observations, entities, flags, party/resources and RNG at visible
return. An authored package never substitutes for private acceptance.

After loading the owning private/tool environment, Debug-build the existing
adapter and run in its owned project. Each run needs a fresh ignored directory.
`SF2_WARP_BINDING` selects the small `{display,mapPalettes}` JSON produced from the
source/R1 proof above; existing `SF2_PRIVATE_*` selections still point to the
accepted private content and party inputs. The observer writes a fresh start from
the accepted controlled R1 input plus this binding, without modifying either.

```powershell
$env:SF2_INPUT_CASE = 'warp-transition-private' # Also warp-transition-private-reduced.
$env:SF2_WARP_PERIOD = '3' # Authored variation only; private consumes its explicit binding.
$env:SF2_EXPLORATION_OBSERVATION_OUTPUT = Join-Path $run 'observation.json'
& $godotBinary --path remake/game --script res://probes/engine_input_accessibility_observation.gd -- `
  --private-exploration-start (Join-Path $run 'start.json') --input-settings (Join-Path $run 'settings.json')
```

Use `--authored-package` and cases `warp-transition` (period 1),
`warp-transition-pause` (6), `warp-transition-restored` (3), or
`warp-transition-failure` (3) for the other bounded observations. All probe
outputs/settings/generated starts remain fresh and worktree-local.

**Unknown:** CPU interruption phases around helpers, tileset/sprite load,
re-enablement and init remain unenumerated. This implementation neither selects
an expected seed nor reseeds. The retained first-warp `75DA` versus `C632`,
completed JOIN timeout, next-actor/order and index29 occupancy failures,
H4 5,340 PASS / 2 FAIL / 40 Unavailable and HEAL recovery gap remain discovery
records in their owners below; this bounded change does not close H4. Use affected
engine checks, adapter compilation, direct compiler/source checks, the bounded
actual host observation and the committed scope plan, without an automatic full
research suite or full-route replay.

### First-warp Motion phase observation

`ExplorationSessionView.ReadObservationJson` now includes `velocityX/Y`,
`travelX/Y`, `speedY`, `accelerationX/Y`, `layer`, `animationCounter` and `waitTimer`
from the same `entity.Motion` as the existing position, destination, speedX and
flags fields. It adds no state authority or gameplay mutation. `actionCursor` remains
the engine instruction index, distinct from a source script address.

The admitted direct observation at accepted base
`5232a1762efcdba384bb4dcf735dd13fa634573d` uses the existing normal/reduced private
first-warp cases above. Adapter Release and native Debug builds pass; both actual
Godot runs pass with no failure/unavailable and exit0. No instance remained before
startup; the probe exits each run normally, so the second setting needs a new process
in the same project. No asset generation, world copy or original emulator run occurs.

Retained outputs are `local/issue534/warp-phase-observation/normal/observation.json`
and `reduced/observation.json`; `readback.py` / `comparison.json` retain the bounded
readback. The already-used local `run.py` reuses the previous first-warp launcher,
changing only the output root and referring to the existing PR564 `binding.json`.
After loading `local/private-inputs.ps1` and forcing
`DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`, reproduction for fresh destinations is:

```powershell
uv run --no-sync sf2 verify adapter
uv run --no-sync python -X utf8 local/issue534/warp-phase-observation/run.py build debug-build-new
uv run --no-sync python -X utf8 local/issue534/warp-phase-observation/run.py host normal-new warp-transition-private 3
uv run --no-sync python -X utf8 local/issue534/warp-phase-observation/run.py host reduced-new warp-transition-private-reduced 3
uv run --no-sync python -X utf8 local/issue534/warp-phase-observation/readback.py
```

The readback defaults to retained `normal`/`reduced`; select the fresh receipt paths
when inspecting a rerun. The launcher uses the existing shared .NET/Godot selections,
worktree-local caches and previously prepared inputs, not additional installations.
No tests of the probe or engine suite are required for these observation fields.

**Confirmed:** the two settings have97 identical ordered observations and equal
entity/flags/party/resource/RNG/tick/control/map fields at all85 result records.
Entry, first-move completion, visible return and next explicit Wait preserve every
pre-existing selected gameplay field compared with the accepted PR564 normal receipt.
The old receipt's entity-sprite-ready delivery is interleaved differently with early
simulation ticks (first difference at event4); its raw event order is therefore not
claimed equal. Removing only sprite-ready events and sequence/revision metadata
leaves the same ordered gameplay events. Both raw histories remain retained.
The initial stricter historical-event equality assertion failed and is preserved
as readback discovery, not a failed native run or permission to repeat it.

The [source/phase owner](../../docs/research/map3-controlled-start-egress-transition.md#first-warp-entity-phase-readback)
records all25 first-return field differences and the earlier R1 binding gaps. To
reproduce directly without the local report script, run the following Python after
loading that environment; it prints every differing common field from the retained
first-return arrays:

```python
import json
from pathlib import Path
load = lambda p: json.loads(Path(p).read_text(encoding="utf-8"))
original = load("local/issue534/warp-field-return-source/prepared-review/runtime/observer.observed.json")
original = original["diagnostic"]["firstReturn"]["entities"]
remake = load("local/issue534/warp-phase-observation/normal/observation.json")
remake = next(s["state"] for s in remake["samples"] if s["label"] == "warp-visible-return")
for e in remake["entities"]:
    slot = int(e["slot"])
    b = bytes(original[slot]["bytes"])
    word = lambda o: int.from_bytes(b[o:o+2], "big", signed=True)
    unsigned = lambda o: int.from_bytes(b[o:o+2], "big")
    fields = dict(x=word(0), y=word(2), velocityX=word(4), velocityY=word(6),
                  travelX=unsigned(8), travelY=unsigned(10), targetX=word(12), targetY=word(14),
                  facing=b[16] & 3, layer=b[17], sprite=b[19], accelerationX=b[24],
                  accelerationY=b[25], speedX=b[26], speedY=b[27], flagsA=b[28],
                  flagsB=b[29], animationCounter=b[30], waitTimer=b[31])
    for key, value in fields.items():
        if value != e[key]:
            print(slot, key, value, e[key])
```

The observation does not establish full entity/CPU phase parity or a service quota.
Original costs remain138 starts /15583.68017570005 active seconds /429625 frames /
16332 batches; no original runtime cost is added. Preserve prior public aggregate /
h3-witch nonruns, H4 5340 PASS /2 FAIL /40 Unavailable, HEAL recovery2vs3 and later
RNG FAIL, JOIN coupling Unknown, and old21/41/61/67 cleanup Unknowns. #534/#437 remain
open. That observation-only slice leaves production behavior unchanged; the bounded
control correction below uses its retained comparison.

### Ordinary source-population control handoff

`ProgramRunner` supersedes the source-population player's action/follower/motion-wait
continuation only on successful ordinary field return, preserving Motion. The next
existing `EntityActionRunner` service integrates movement, establishes speed32/32,
acceleration0/0, flagsA `(prior & 0x10) | 0xEF` and wait0, then handles field input.
Physical travel, velocity, facing, flagsB and animation survive setup. Authored
configuration and NPC services are unaffected. Program/dialogue/init/load/fade or
failed returns do not acquire this control. `ExplorationDispatcher` uses the same
collision policy for preview, without committing setup early; blocked moves retain
only their normal facing change. See the [source and remaining gaps](../../docs/research/map3-controlled-start-egress-transition.md#ordinary-source-population-control-handoff).

The affected `ExplorationSessionTests` names start with `ControlledSetup`,
`ControlledHandoff` and `ControlledPreview`. They exercise actual reducers under
changed slots, speed/acceleration/collision settings, neutral service, existing travel,
NPC order/RNG, action/follower retirement, ownership exclusions and warp validation.
The initial selected run also includes `WarpProducingPassRetainsOldPopulationBeforeFade`,
`LaterSlotSeesPlayerTravelReservationBeforeWarpRelocation`,
`OrdinaryWarpJoinsFiniteServicesAndActualVisibility`,
`WarpMarkerPrecedesTravelButEntityObstructionPrecedesMarker` and
`EarlierSlotCanObstructPendingWarp`. It completed30 PASS /1 FAIL; the dialogue case of
`ControlledSetupDoesNotRunDuringOtherOwners` lacked its required ClosedPortraitWindow
fixture state and returned `text-input-unavailable`. After fixing that precondition,
the owning five-case method passes. Both TRX results are retained under
`local/issue534/field-control-handoff/{behavior-01,behavior-correction}/`; the completed
failure is not relabeled as interrupted or erased by a full rerun.

The committed planner's dedicated engine gate completed657 PASS /1 FAIL /30 SKIP.
The failed node was `SourceDoorChecksEntityObstructionBeforeCopyAndTraversal` with
`shape: "mover-ignores", x: 4, y: 3, blocked: False`: that old ordinary-control
expectation retained script-specific collision disabling. The source-controlled rule
requires obstruction after handoff, so the case now expects blockage and also asserts
that preview leaves Motion unchanged except facing. The corrected eleven-case method
passes in `door-correction/`; `engine.log` retains the completed aggregate failure.
The correction changes tests/documentation only and does not invalidate the native run.

Adapter Release and actual Debug builds pass. One authorized normal first-warp run
passes with exit0, no failures/unavailable, using the existing project, assets,
private inputs and PR564 binding. No instance was available before startup; the probe
exits normally and leaves no owned Godot process. No original emulator, asset
regeneration, world copy, reduced rerun or whole-route run is involved. PR568's paired
settings evidence remains applicable because no presentation/settings behavior changes.
After loading `local/private-inputs.ps1` and forcing the shared .NET environment's
`DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`, the retained launcher supports fresh outputs:

```powershell
uv run --no-sync python -X utf8 local/issue534/field-control-handoff/run.py test behavior-new 'FullyQualifiedName~ControlledSetup|FullyQualifiedName~ControlledHandoff|FullyQualifiedName~ControlledPreview|FullyQualifiedName~WarpProducingPassRetainsOldPopulationBeforeFade|FullyQualifiedName~LaterSlotSeesPlayerTravelReservationBeforeWarpRelocation|FullyQualifiedName~OrdinaryWarpJoinsFiniteServicesAndActualVisibility|FullyQualifiedName~WarpMarkerPrecedesTravelButEntityObstructionPrecedesMarker|FullyQualifiedName~EarlierSlotCanObstructPendingWarp'
uv run --no-sync sf2 verify adapter
uv run --no-sync python -X utf8 local/issue534/field-control-handoff/run.py build debug-build-new
uv run --no-sync python -X utf8 local/issue534/field-control-handoff/run.py host normal-new warp-transition-private 3
uv run --no-sync python -X utf8 local/issue534/field-control-handoff/readback.py
```

`normal/observation.json`, `comparison.json` and `readback.log` retain the actual run
and comparison in that output root; select a fresh receipt explicitly when rereading
a rerun. There are97 raw events and84 result records. Against PR568 normal, ordered
gameplay events match after excluding only `entity-sprite-ready` and sequence/revision
metadata; raw event histories are retained and are not equal. At entry, first-move
completion, visible return and next explicit Wait, every entity field and selected
flags/party/resources/RNG/tick/control/map field matches except the intended player
flagsA E0 to EF at the three postservice samples. The initial state remains unchanged.
Visible return is tick65/F01B. The same19-field/20-slot original readback above now
reports24 differences, removing only the player flagsA gap; all NPC and other phase
gaps remain. This is not an NPC timing repair or an inferred service quota.

Original costs and all preserved failures/nonruns/Unknowns immediately above remain
unchanged. Idle-to-walk timer carryover and caller-specific script timer initialization
are separate excluded gaps. Committed planner output, exact CI and the frozen Draft
PR belong to the slice handoff; this result does not grant integration or H4 acceptance.

### Source idle and caller verification

The [source/caller owner](../../docs/research/map3-controlled-start-egress-transition.md#source-idle-completion-and-caller-installation)
and [content contract](./exploration-programs.md#definitions-and-execution) define the current
installation policies, terminal idle and script-completion boundary. Engine behaviors in
`ExplorationSessionTests` named `SourceIdle*` and `SourceScriptInstallation*` exercise timer7
preservation before service, jump-to-idle timer1, leading-wait/RNG causality, slots3/7 and aliases,
distinct collision policy, resetting first actions, residual physical travel, sprite/Unsupported/
authored-Stop noncompletion, actual Content-driven execution and control/follower/NPC separation.
The existing `ControlledSetup*`, `ControlledHandoff*`, `ControlledPreview*` and affected warp
service checks retain the accepted ordinary-control boundary.

Fresh retained output root is `local/issue534/idle-callers/`. The initial selected run completed
42 PASS /2 FAIL: `SourceIdleCompletionReleasesTheScriptWhilePreservingPhysicalTravelForItsNextCaller`
incorrectly expected animation1 for ten fixed units of movement, and
`SourceIdleContentCompletesARealScriptAndLeavesTheNpcAvailable` used `flag` instead of the
existing `set-flag` JSON opcode. Corrected expectations pass both failed methods; a further
repeated-idle/control assertion passes its owning method. `behavior-01`, `behavior-correction`
and `idle-repeat` retain their TRX/logs. Adapter Release and actual Debug builds pass with
zero warnings/errors. Completed failures are retained rather than relabeled as interrupted.

The changed compiler directly prepares one nonvisual world from the registered canonical
import, pinned upstream and `remake/reference/inputs/map3-programs.json`, without ROM/visual
arguments. It produces6 maps/158 programs/118 source entries. The first comparison against
retained `local/issue534/inputs/private-world-audio-accepted.json` fails because that old
input also predates accepted producer changes. The explicitly authorized additional baseline
uses exact compiler source from accepted `8095ce6b634038a094be4fa663e084b431345b4a` and the
same inputs, without checkout, source rebuild or asset generation.

`three-way.py` / `three-way-comparison.json` prove candidate versus accepted baseline equality
after removing only96 motion installation properties and137 exact jump/idle tails. Walking
streams and their cursor indices are unchanged. Compared with the older retained world,
75 programs also carry already accepted changes from PR556 (`f8238978`: explicit portrait/text
window lifecycle) and PR564 (`2f24f28b`: finite black-fade bindings and entities-running context).
Maps, texts, memberNames, growth and partyFlags compare equal. The additional accepted source
entries are portraitwindow.asm and trap5_textbox.asm. The7 old source entries used by the reused
presentation are retained in the final provenance union; common entries compare exactly.
The accepted presentation block compares equal, with no atlas/audio export or world-tree copy.
`world-with-accepted-presentation.json` is the sole candidate native input. The initial comparison
FAIL is retained, as is the first three-way readback's assumption that all new source entries
were already listed by the old input. The corrected readback reuses the same generated JSONs.

After loading `local/private-inputs.ps1`, forcing `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false` and
using existing shared tools/worktree-local writable state, the narrow commands are:

```powershell
uv run --no-sync python -X utf8 local/issue534/idle-callers/run.py test behavior-new 'FullyQualifiedName~SourceIdle|FullyQualifiedName~SourceScriptInstallation|FullyQualifiedName~ControlledSetup|FullyQualifiedName~ControlledHandoff|FullyQualifiedName~ControlledPreview'
uv run --no-sync sf2 verify adapter
uv run --no-sync python -X utf8 local/issue534/idle-callers/run.py build debug-build-new
uv run --no-sync python -X utf8 local/issue534/idle-callers/read-native.py
```

The direct producer command is `uv run --no-sync python -X utf8 -m sf2tool.remake_exploration_content`
with `--canonical`, `--upstream`, the selection above and a fresh ignored `--output`, omitting
`--rom-path`/`--presentation-root`. The local three-way helper retains exact accepted compiler
source and private selections; it adds only the existing presentation block and its provenance.
Do not overwrite prior receipts when reproducing: select fresh output names in the local helper.

One authorized actual `engine_h4_observation.gd` run uses the first3 unchanged Left/Left/Right
steps of the retained ordinary prefix, real dialogue confirmations, existing project/assets and
the accepted R1 start with PR564 display binding. No instance remained before authorized startup.
`native-01/actual.jsonl`, `godot.log` and `process.json` retain exit0,169 result signals,205 events,
six actual inputs and no session/Godot failures; the probe exits normally. The readback requires
one session, flag601 false after step2 and true after step3, player(4,4), usable control/flagsAEF,
and entity128/slot3 at(5,4), walking cursor0/wait30/timer1 with Y velocity-32/travel0 retained.
Tick142/seed75DA are reported observations, not original goldens. A local readback initially used
the JSON numeric cursor directly as a Python list index; integer conversion corrected that helper
and the same receipt passes. No native rerun was needed.

This probe has no Wait-key mapping. It stops after step3 and does not establish the later random
draw schedule, JOIN, school/castle/battle or H4. Exact leading-wait/draw causality belongs to the
small engine behaviors. No screenshot, original capture, original frame/service conversion,
source/H1 rebuild or asset export occurs. High-bit wait fidelity and unimplemented native/waitIdle
producers remain explicit boundaries. All earlier failures/nonruns/cleanup Unknowns and original
totals138 /15583.68017570005s /429625 frames /16332 batches remain unchanged. Committed planner,
producer identity and exact CI belong to the frozen slice handoff; this is not a claim that the
earlier first-warp NPC phase gap has been fixed.

## Explicit field-input gameplay Wait

`WaitAtInput` is an engine command for one deliberate opportunity at a settled field-input
consumer under [Option A](../../docs/decisions/0010-map3-battle01-product-acceptance.md#evidenced-gameplay-waits-accepted-option-a).
It carries neither a wait token nor a batch count. The current session/revision identifies the
occurrence. Admission requires exploration `PlayerInput`, no program cursor/callers/story wait,
field continuation with no pending battle entry/entity interaction, a closed text window, and a
player with no motion/actions or pending sprite readiness. NPC busy state alone does not reject it.

The command reuses the existing entity update path once, preserving physical-slot order, movement
before actions, action wait/expiry, collision and shared RNG rules. A successful opportunity emits
`gameplay-wait` and increments the simulation tick; ordinary `AdvanceSimulation` still emits
`simulation-tick`. Entity failures retain their existing committed update and stop result. Every
new Wait rechecks the boundary; a result is not guaranteed to remain ready. Battle control/scenes,
dialogue/choice/audio consumers and moving players reject without advancing. `Acknowledge` and
`CompletePresentation` do not substitute for this input or supply extra field opportunities.

The engine guard and host share `SessionSnapshot.CanWaitAtInput`. The host binds Wait to **V / Pad
RightStick**, configurable through the existing `bindings.wait` setting. Its field help derives the
effective binding and explains that releasing pauses field time, **including NPCs midway through
motion**. This is an explicit modern interaction policy, not a claim about the original idle clock.
A fresh press submits one `WaitAtInput`; holding submits at most one per host process callback and
at most 60 repeats per second. A monotonic 16,667-microsecond deadline drops missed opportunities;
there is no catch-up batch or accumulated field debt. Different hold durations/frame rates can
produce different input counts. Comparisons use the same accepted semantic Wait count, not equal
milliseconds of holding.

At an eligible field boundary, no Wait means no autonomous entity update. Already committed player
movement continues through `AdvanceSimulation`; arrival at field input clears unused tick budget.
Each result rechecks eligibility. Release, another action, loss of window focus, hide/show, actual
tree pause/unpause, failure or departure from this consumer disarms the hold. Returning requires a
mapped release/neutral event and a fresh press. Reveal-only Confirm does not supply a field Wait.
This is not a global pause action: presentation delivery continues. Existing autonomous updates at
ordinary dialogue, audio and other non-field consumers remain unreconciled legacy behavior, not newly proven
mandatory work or whole-host Option A conformance. The first-warp, JOIN logical-audio-end and HEAL
natural-order evidence gaps remain Unknown; preserve the [H4 diagnostic](#continuous-h4-comparison)
results. This input does not normalize the old 260-step trace.

**Confirmed:** the bounded `field-wait-*` cases in the existing
[input observer](../game/probes/engine_input_accessibility_observation.gd) exercise actual native
input through ordinary `Main.tscn`, including tap/hold/release, repeated press suppression, a missed
callback interval, NPC pause during motion, other-action/hide/tree-pause cancellation, and mandatory
movement reaching a debt-free input boundary. A native sibling window transfers OS focus; checks
read actual window focus and session state, never manually emit notifications. Dialogue/choice,
actual presentation input and reveal-only Confirm produce no field Wait receipts; returning to field cannot resume
a canceled hold. Hardware controller driver/hot-plug behavior remains outside injected events.

After the protected environment above and a Debug adapter build, reproduce in the existing owned
Godot project. Visible startup is required for this OS-focus observation. Every case writes its
authored input, settings and output to a fresh ignored directory; changed startup settings justify
separate runs, without copying the project or installation:

The `field-wait-*` entry requires exactly one `--authored-package` and `--input-settings` argument
and a nonempty `SF2_EXPLORATION_OBSERVATION_OUTPUT`. Before any destination is opened it normalizes
all three absolute paths, requires distinct fresh destinations under this worktree's ignored
`local/`, existing real parent directories, and rejects file/directory/link reuse. It opens output
and writes/flushes/checks/closes the generated inputs before instantiating Main. I/O errors exit2
with the names of this run's newly created files; a partial fresh file may remain as failed-run
evidence. Existing destinations are rejected. Only this run's created package can be
rewritten for the later presentation scenario. Final output also flushes/checks/closes before exit.
This guard covers the field-wait and text-wait entries; legacy observer cases retain their prior lifecycle.

```powershell
$run = Join-Path (Get-Location) 'local/field-wait-keyboard'
New-Item -ItemType Directory -Path $run | Out-Null
$env:SF2_INPUT_CASE = 'field-wait-keyboard'
# Also field-wait-gamepad, field-wait-remapped-keyboard, field-wait-remapped-gamepad.
# field-wait-remapped-axis-gamepad uses otherwise unbound LeftX+ for Wait.
$env:SF2_INPUT_VARIANT = 'random' # Repeat remapped-gamepad with 'collision'.
$env:SF2_EXPLORATION_OBSERVATION_OUTPUT = Join-Path $run 'observation.json'
& $godotBinary --path remake/game --script res://probes/engine_input_accessibility_observation.gd -- `
  --authored-package (Join-Path $run 'package.json') --input-settings (Join-Path $run 'settings.json')
```

## Plain text-input gameplay Wait

The [portrait/input contract](./exploration-programs.md#portrait-lifecycle-and-plain-current-input-wait)
defines the admitted source producer and engine boundary. `SessionSnapshot.CanWaitForText` is the
shared eligibility query; `WaitForText(token)` uses the current session/revision and exact plain
consumer token. It contributes one zero-input helper iteration, while Ack contributes none.
This follows `input.asm:WaitForPlayerInput` (input test before WaitForVInt), reached by
`battlefunctions_0.asm:FadeOut_WaitForP1Input` after previous-music handling. It does not apply the
different W1/W2 RNG preamble to JOIN.

At this consumer the host reuses the field Wait binding, monotonic repeat and cancellation rules.
Delivery time always produces zero gameplay opportunities, including incomplete reveal. Wait during
reveal is discarded and disarmed; reveal-only Confirm changes display only. A fresh press after
reveal can Wait. Entry/exit clears elapsed tick debt, and a changed consumer token disarms a held
Wait. Actual acknowledgement may reach mandatory subsequent work; that work is separate from an
accepting input poll. Ordinary ShowText, choice, audio and active/unknown portrait consumers are
outside this admission and retain their explicit unresolved legacy behavior.

**Confirmed:** `ExplorationSessionTests` exercises immediate Ack, enabled/disabled NPC service,
different seeds/wait phases, real close tails, calls/branches, absent/missing/skipped portrait
lookups, active portrait preservation, stale envelopes/tokens, rejected consumer types and retained
entity failures. The existing input observer's `text-wait-*` cases load real compiler-produced
single-text instructions and a small subset of existing private visual assets. They observe actual
portrait draw identity/flags across raw text and close, the source close/sleep tail, default instant
keyboard versus adjustable/remapped gamepad, reveal-only no-update, matching tick/RNG/entities after
the same four semantic Waits, pause mid-motion and consumer-change rearm. Remapped keyboard with
competing destinations exercises a changed legal state. No screenshots or original emulator runs.

For direct producer reproduction, select the registered canonical import, existing prepared private
world and pinned read-only upstream through `SF2_PRIVATE_CANONICAL_MAP_IMPORT`,
`SF2_PRIVATE_EXPLORATION_CONTENT` and `SF2_PORTRAIT_WAIT_SOURCE`. Choose a fresh ignored file in
`SF2_PORTRAIT_WAIT_INPUT`. Run this Python body with `uv run --locked python -X utf8` after loading
the owning environment. It writes only the lowered programs and the small native observation input;
it does not copy a world or regenerate/promote an asset library.

```python
import json, os, subprocess
from pathlib import Path
from sf2tool.remake_exploration_content import OriginalPrograms
upstream = Path(os.environ["SF2_PORTRAIT_WAIT_SOURCE"])
canonical = json.loads(Path(os.environ["SF2_PRIVATE_CANONICAL_MAP_IMPORT"]).read_bytes())
assert subprocess.check_output(["git", "rev-parse", "HEAD"], cwd=upstream, text=True).strip() == canonical["upstream"]["commit"]
compiler = OriginalPrograms(canonical, upstream)
for path in sorted((upstream / "disasm/data/maps/entries/map03/mapsetups").glob("*.asm")):
    compiler.register_file(path.relative_to(upstream).as_posix())
compiler.compile("cs_51614")
for symbol, row in list(compiler.raw.items()):
    if row["path"].startswith("data/maps/entries/map03/mapsetups/") and any(
        op["opcode"] in ("closeTxt", "clsTxt") for op in row["operations"]):
        compiler.compile(symbol)
target = Path(os.environ["SF2_PORTRAIT_WAIT_INPUT"])
assert target.resolve().is_relative_to((Path.cwd() / "local").resolve())
with target.with_suffix(".programs.json").open("x", encoding="utf-8") as stream:
    json.dump(list(compiler.programs.values()), stream, indent=2)
world = json.loads(Path(os.environ["SF2_PRIVATE_EXPLORATION_CONTENT"]).read_bytes())["world"]
source = compiler.programs["cs_51614"]
end = next(n for n, op in enumerate(source["instructions"]) if op["op"] == "wait-ticks") + 1
visuals = world["presentation"]
sprites = [s for s in visuals["sprites"] if s["sprite"] in (2, 30)]
data = {"sourceProgram": {"id": "source-single-text", "entitiesRunning": source["entitiesRunning"],
        "instructions": source["instructions"][:end] + [{"op": "end"}]},
    "sourceText": next(t for t in world["texts"] if t["id"] == 535),
    "presentation": {"maps": [next(m for m in visuals["maps"] if m["map"] == "map-3")],
        "sprites": sprites, "portraits": [p for p in visuals["portraits"]
            if p["portrait"] in {s["portrait"] for s in sprites}]}}
with target.open("x", encoding="utf-8") as stream:
    json.dump(data, stream)
```

Inspect the direct producer output: each acknowledged single-text operation is followed by
`close-portrait`, `close-text`, `wait-ticks(10)`; raw text has no such tail; raw `clsTxt` preserves
portrait; JOIN has SoundWait/PreviousMusic followed by plain input and text-only close. These
selected source names are observation fixtures; production admission contains none of them.

Build the current Debug adapter with the protected locked SDK workflow, then use the same native
command and fresh destination guard shown above with `SF2_INPUT_CASE=text-wait-keyboard`,
`text-wait-remapped-gamepad`, or `text-wait-remapped-keyboard` (last with
`SF2_INPUT_VARIANT=collision`). Leave `SF2_PORTRAIT_WAIT_INPUT` pointing to the generated input.
Startup settings differ between these bounded runs; reuse the owned project and installation.

**Unknown / retained failures:** natural post-JOIN entry, logical audio end, live VInt service gates
and H4 chronology are not established by this source/engine/host observation. Earlier completed
#531/#549 failures, #534 H4 5340 PASS / 2 FAIL / 40 Unavailable, index29 occupancy mismatch and the
first-control timeout/next-actor failure remain evidence. The old 260-step plan is not a Wait stream.

Require zero exit, `passed:true`, empty failures/unavailable and no native errors. Product assertion
failure exits1; an unavailable OS-focus observation exits2 with an explicit `unavailable` entry and
stops before dependent checks. Never replace a rejected foreground activation with a simulated
notification or report the environmental refusal as a product failure. Compare the
`semantic-four-waits` checkpoint across the four random cases: simulation tick, main seed and all
entity state agree despite 120/30 FPS caps, instant/adjustable text, swapped Confirm/Cancel,
reduced-flash settings and remapped keyboard/gamepad bindings. This checkpoint begins with a
two-opportunity NPC wait, then a random walk. The independent `collision` variation checks two
physical slots competing for one destination. OS focus is **Unknown** for a headless-only run;
successful headless process exit cannot substitute for the visible observation.

The axis case additionally covers fresh positive deflection, held duplicate suppression, opposite
direction releasing the mapped Wait, neutral followed by a fresh deflection, and actual tree pause
requiring new input. **Confirmed:** its four-Wait checkpoint equals the button/keyboard cases;
axis/hide/pause, mandatory movement, dialogue/presentation, and native focus return/rearm checks
pass. Independent consumer checks run before the final focus transfer so an environmental focus
refusal does not hide their results. Preserve earlier completed focus-unavailable runs: later
success does not make OS foreground activation universally available. The earlier five complete
button/keyboard cases remain separate evidence; hardware driver/hot-plug behavior remains Unknown.

Direct acceptance uses `ExplorationSessionTests`: independent seed/target/counter assertions for
different valid states, physical-slot collision, Wait followed by Move, stale/wrong consumer inputs
and entity failure. After the locked .NET environment above and locked restore, run from `remake/`:

```powershell
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
& $env:DOTNET_BIN test tests/Sf2.Remake.Engine.Tests/Sf2.Remake.Engine.Tests.csproj `
  --configuration Release --no-restore --filter 'FullyQualifiedName~ExplorationSessionTests'
```

For subsequent corrections, select the changed test methods rather than repeating a completed
broader run. This engine-only boundary does not require adapter, Godot or original-emulator work.

## Bound opening field-text observation

Use the [execution profile](./exploration-programs.md#bound-field-text-work) and its
[source/settings provenance](../../docs/research/map3-messenger-acceptance.md#opening-field-text-settings-and-view-binding).
`ExplorationTextWaitTests` exercises actual glyph/window/view reducers and session commands:
fresh/reused windows, changed widths and names, scroll/wrap, repeated W1/W2 and tails, source
speed/mouth/input versus reveal, optional/accepting polls with NPC RNG, indicator phases,
first-service-started view scrolling, threshold/clamp/axis completion, signed counter,
window completion pass, suppressed wrapper return and unsupported/stale contexts. The normal
engine unit gate also covers plain input/JOIN and ordinary field/control/warp behavior.
Run `uv run sf2 verify engine` and `uv run sf2 verify adapter` with the locked environment.
A skipped private battle fact remains unavailable, not a passed private comparison.

The producer change owns new consumed source metadata and helper encoding. Run the direct
metadata preparation and compare all other sections with the retained accepted world; use its
existing presentation without rebuilding assets. With private configuration loaded, run
`uv run sf2 h2 variable-width-font --upstream $upstream --output-path local/<fresh>/font.json`.
Run the committed default planner and its direct research-public source/document checks;
this change does not invalidate original H3 or request the slow research/full aggregate.
The admitted font comparison is metadata/source-ROM verification, not a new parser test.

The existing `engine_input_accessibility_observation.gd` accepts `opening-private-instant`,
`opening-private-adjusted`, `opening-private-adjusted-reveal` and
`opening-private-instant-suppressed`. Select the usual private battle/party inputs, candidate
world via `SF2_PRIVATE_EXPLORATION_CONTENT`, and a fresh controlled-start file carrying the
explicit text settings. Keep the retained inputs read-only. Set a fresh ignored `$run`, then:

```powershell
$env:SF2_INPUT_CASE = 'opening-private-adjusted'
$env:SF2_INPUT_RATE = '20'
$env:SF2_EXPLORATION_OBSERVATION_OUTPUT = [IO.Path]::GetFullPath("$run/actual.json")
& $env:GODOT_BIN --headless --path remake/game --fixed-fps 60 `
  --script res://probes/engine_input_accessibility_observation.gd -- `
  --private-exploration-start $boundStart --input-settings "$run/settings.json" `
  *> "$run/godot.log"
```

The probe performs ordinary Left/Left/Right opening input, one explicit nonaccepting Wait per
occurrence followed by Ack, and stops at actual caller return. Natural adjustable never sends
reveal-only Confirm; the reveal variant sends it while characters remain. Loop limits report
failure rather than complete a state. The suppressed variant additionally faces/interacts with
the nearby live actor through ordinary input. State/result/audio readback checks enabled NPC
work, pure reveal, zero input-boundary debt, W2 validation67, W1 absence of67, and actual close.
No screenshots, reseed, checkpoint injection or receipt-count endpoint is used.

**Confirmed (remake native state/input/audio):** retained
`local/issue534/opening-implementation/native-05/06/07` have exit0, empty failures/unavailable
and no application/adapter/script errors. `read-native.py` compares17 common entry/input/return
boundaries across instant, natural20-character/second and reveal-only40-character/second runs:
map, flags, program cursor/wait, every exposed entity, RNG/copy, ticks, view/window/settings and
control eligibility agree. Ready ticks are233/292/409; the ordinary close/Sleep10/zone return
finishes at432 with F601 and main imageC0BF0000/copyD2. These are results of the supplied semantic
inputs, not runtime admission constants. In the text511 nonaccepting poll, draw/copy produces
C6290000/C6 and subsequent enabled NPC work leavesA2DE0000 while copy remainsC6.

`native-09` retains event context through the suppressed close at549→558, with unchanged
entities/seed and both window authorities closed before return. The existing unbound W1
regression is retained at `local/issue534/w1-implementation/opening-regression-01`; its accepted
no-service behavior still passes. Earlier compile/assertion/preparation failures remain in the
slice handoff: no result was replaced or described as interrupted. The old raw-byte private font
Git check failed before world output; the source-ROM reader corrected its provenance.

For the host projection lifetime, use the same probe with `field-projection-adjusted` or
`field-projection-instant`. It writes an entirely authored package (transparent graphics and
generated PCM) to the fresh ignored destination; no private content is needed. After building
the Debug adapter and loading the locked runtime environment, set a fresh `$run` and execute:

```powershell
$env:SF2_INPUT_CASE = 'field-projection-adjusted'
$env:SF2_INPUT_RATE = '20'
$env:SF2_EXPLORATION_OBSERVATION_OUTPUT = [IO.Path]::GetFullPath("$run/actual.json")
& $env:GODOT_BIN --headless --path remake/game --fixed-fps 60 `
  --script res://probes/engine_input_accessibility_observation.gd -- `
  --authored-package "$run/package.json" --input-settings "$run/settings.json" `
  *> "$run/godot.log"
```

**Confirmed (authored host observation):** active member1 resolves `{LEADER}` to the literal
name `Name{W2}`. Ack retains that projection, visible characters and speech-start count across
`ViewWait`/`TickWait`; the next display starts its own reveal. Its Ack retains `Name{W2}!`
through `TextCloseWait`, then actual closure clears the text. The host uses the existing open
window identity for this lifetime. Both delivery settings pass in
`local/issue534/opening-implementation/projection-after-01/02`; natural delivery starts actual
authored speech audio, with no additional speech starts during either continuation. The
pre-correction `projection-before-06` records the name reverting to member0 and reveal resetting.
The natural run's ObjectDB cleanup warning remains recorded; it is not a clean-exit claim.

**Unknown:** these runs do not establish OS focus-loss behavior, exact original hardware text
or view timing, portrait/quake schedules, whole-route9A/H4 or pending#517 speech policy. Existing
speech projection is preserved. Prior cleanup warnings/failures remain attempt-specific;
original acquisition totals and original-run delta0 are unchanged.

## Bound portrait entity-event observation

The [portrait consumer](exploration-programs.md#bound-entity-event-portrait) extends the bound
field-text profile through an actual entity wrapper return. Its
[source owner](../../docs/research/map3-messenger-acceptance.md#classroom-portrait-entity-event-binding)
separates static rules from original runtime evidence. Engine behavior tests in
`ExplorationTextWaitTests` cover legal changed speakers/flags and first/repeat branches, absent,
fresh, retained and Unknown portraits, register/remove boundaries, source counters, no-W tails,
fresh/reused dialogue creation versus first-glyph service, incoming typing preservation,
empty/W-only text and W continuation, script versus native returns, enabled versus suppressed closing and poll-copy preservation across
NPC then blink/mouth draws. For seed12341234 the independent poll/four rejected NPC candidates/
blink/mouth sequence endsE0291234 while copyEC remains. Use `uv run sf2 verify engine` and
`uv run sf2 verify adapter`; native settings runs below exercise actual projection/input/audio.

Prepare one fresh nonvisual world and add only eye/mouth mapping metadata from the registered ROM,
reusing accepted rasters/audio and the retained bound start read-only. The private preparation at
`local/issue534/portrait-event/prepare.py` and `candidate/comparison.json` verify25 source script ends,
25 activating calls and36 portrait mappings plus named new source provenance; every other world
section, raster/audio payload and shared provenance is unchanged. No asset/H1/ROM rebuild is needed.
The maintained source producer/reader are `remake_exploration_content.py` and
`ExplorationAssetReader`; raw source/ROM content stays ignored.

Use the existing protected environment and owned Godot project, with no other owned process running:

```powershell
$env:SF2_INPUT_CASE = 'portrait-event-private-adjusted'
$env:SF2_INPUT_RATE = '20'
$env:SF2_PRIVATE_EXPLORATION_PLAN = (Resolve-Path 'tests/fixtures/h3/map3-battle01-natural-route-v1.json').Path
# Select existing private battle/party inputs, the fresh candidate world, and fresh output/settings.
& $env:GODOT_BIN --headless --path remake/game --fixed-fps 60 `
  --script res://probes/engine_input_accessibility_observation.gd -- `
  --private-exploration-start $retainedBoundStart --input-settings $freshSettings
```

Select `portrait-event-private-instant` or `portrait-event-private-adjusted-reveal` (rate40) for
the other settings. The probe consumes accepted spatial waypoints with live position/occupancy,
one explicit Wait then Ack per reached W boundary, and no optional mother repeat or frozen C quota.
It stops after Sarah's movement/flag/window work returns actual control, before stairs/Astral.
The existing instance had exited; startup was needed to observe the changed adapter, using the
retained installation/project. No screenshot or original launch/capture is part of this command.

**Confirmed (remake observation):** private `native-06/07/08` each exit0 with empty failures and
Unavailable arrays. `native-corrected-comparison.json` compares17 common input/entry/return boundaries and
all1423 settled logical states (tick0 through1422) across complete entities, flags, party, main RNG,
copied byte, cursor, text/view/portrait state, settings and display. At return the main seed is
EDCD0000, copy95, Sarah is(41,7), F256 is set, facing restored, windows closed and entities enabled.
The only reached input texts are510/511/483/512/481;480 completes automatically.

Actual rendering metadata is compared only with its recorded simulation tick:40/41/43 samples
match source eye/mouth tile selection, identity/flags and movement geometry. A signal callback can
precede the frame's draw. Audio receipts retain real started PCM playback:65 open,67 W2 validation,
65 close, at gameplay revisions1697/1836/2136 for all settings. Natural speech creates additional
existing delivery receipts; collect receipts across samples because the host keeps a bounded ring.
No adapter/audio/process error was reported in these three completed runs. Speech policy#517 remains
pending. Host geometry is the existing presentation scale, not an original hardware pixel claim.

`creation-source-check.json` independently checks the source creation boundary in all three runs:
tick1205 enters ClearFirst with typewriting=false, blink19/mouth6; through the ten creation services
mouth stays6 and blink advances to9. At tick1215 creation returns and typing becomes true without
another portrait opportunity; tick1216's first glyph service opens mouth at5. The first mouth
draw is1221,749B0000→EBE60000/range5/value4, checked with the source16-bit LCG formula. Blink closes
eyes at3 on that same opportunity. Same-tick creation projection samples show closed mouths.

Pre-correction `native-03/04/05` and `native-comparison.json` remain completed settings-equality
evidence, not source-correct creation results. Independent review found typing enabled at1205,
mouth5 at1206 and an early draw at1211. The six focused behavior cases preserve creation's incoming
typing value and verify the transition, continued blink and non-typing mouth reset. The initial
`review-correction-red.log` has three lifecycle failures plus an empty-string reader rejection;
the empty-token case now directly exercises engine state after admitted startup. The corrected
affected text/session selection passes195 in `review-correction-tests.log`; Debug and Release
adapter builds pass in `review-correction-debug.log` and `review-correction-adapter.log`. These runs
reuse the unchanged candidate/start and semantic input stream; the changed copy/RNG history was
not fitted to the old endpoint. No asset preparation or original runtime work was repeated.

Preserve completed diagnostic failures: native01 failed GDScript type inference; native02 rejected
the new case's startup argument classification before Main started. Both probe errors were corrected
before native03. `engine-behavior-02.log` completed45PASS/1FAIL because the new closed-window script
test attempted a prohibited nonzero start override; the corrected test changes live state after
admitted startup, and `engine-correction-01.log` passes that node. `engine-gate.log` then passes
724 with30 private skips. The first frame-sampling comparison remains in
`frame-sampling-comparison-failure.json`: two480 sample labels selected different moments around
actual delivery. Raw service callbacks also expose the legitimate delivery join at tick1351;
comparing each tick's final actual callback after delivery and before the next opportunity gives
equal state without injected services or altered evidence. Input boundaries are compared directly.

The prior completed `opening-implementation/normal-verify-01.log` remains exit1 at missing
worktree-local research provenance after148 passing Python checks; its accepted applicability
decision is retained. Recheck dependencies changed here, not the prior passing aggregate. Original
cost delta is0. Opening ancestry remains Inferred, and the eight warp gaps, H4/HEAL/JOIN/next-actor,
nonrun/cleanup boundaries retain their existing owners. This stops at the portrait entity-event
return and grants no admission to later zone/camera/JOIN/battle consumers.

## Source zone caller observation

The [source-zone consumer](exploration-programs.md#source-zone-caller) includes generic opening
Zone6, Sarah and the first Astral introduction through caller return. Its
[source owner](../../docs/research/map3-messenger-acceptance.md#first-introduction-source-zone-caller)
separates static source order from actual remake evidence. Engine cases cover legal slopes,
map-blocked markers versus entity obstruction, producing-pass completion, preserved motion/timer,
live service flags, empty/skipped handlers, registered/absent/Unknown portraits, changed speakers,
seeds and flags, typing creation and NPC→portrait RNG/copy order, unconditional return service,
physical arrival independent of Busy/script idle, stale/control gating and authored steps.

After loading `local/private-inputs.ps1`, using the protected SDK environment above, run the
affected `ExplorationSessionTests|ExplorationTextWaitTests` filter and `uv run sf2 verify adapter`.
Prepare one fresh nonvisual world with `local/issue534/zone-caller/prepare.py`; its
`candidate/comparison.json` verifies14 source-zone/init records as the only content difference,
unchanged shared provenance and all previous rasters/audio/mappings. No ROM/asset/H1 extraction
or environment copy is part of this preparation. Preserve the retained bound start unchanged.

Actual Godot startup is needed to observe the changed adapter; use the existing owned
installation/project and no simultaneous owned process. With the same private selections as the
portrait observation, the maintained probe cases are `portrait-event-zone-private-instant`,
`portrait-event-zone-private-adjusted` (rate20), and
`portrait-event-zone-private-adjusted-reveal` (rate40). Each consumes spatial edges with live
occupancy and one Wait then Ack per reached W boundary. No historical frame/C count is used.

```powershell
# After private selections and a current Debug adapter build, choose a fresh run name.
uv run --locked python -X utf8 local/issue534/zone-caller/run-native.py native-new instant 1 40
# Repeat in fresh outputs for adjusted 1 20 and adjusted-reveal 1 40.
```

**Confirmed (remake observation):** retained `native-01/02/03` each exit0/PASS with empty failures
and Unavailable arrays and no adapter/audio/process errors. `compare-native.py` reproduces21
common semantic boundaries and1,674 settled logical states across complete selected entities,
flags/party, RNG/copy, view, caller, windows, typing, settings and display. Its191 same-tick
portrait projection samples match identity, placement and mapped tiles. Window/validation audio
from Sarah entry through introduction return is65→67→65→65→65 with real started PCM receipts.
The old results remain evidence of their own implementation; the current prefix is not fitted
to them. Current Sarah return is tick1411/6BA00000/copy93; introduction returns at
1673/A9240000/copyA9 with flags[0,32,256,601] and ordinary control at(58,13).

`check-lifecycle.py` reproduces the two source-zone entries and actual lifecycle boundaries in
all three runs. Introduction entry at1529 preserves moving player and pending init cursor0;
portrait31 registers at1534 while the player still moves. W1 acceptance starts return at1658,
portrait closes at1663, dialogue closes at1672, then the mandatory return service completes at
1673 with physical arrival. These are observations, not runtime admission conditions. The stop
is before Down from(58,13), later entity142 text500/501, F602 and subsequent zone branches.

`behavior-01.log` completed205PASS/2FAIL: both failures in
`ZoneReturnAlwaysWaitsOnceThenChecksPhysicalArrivalRegardlessOfScript` omitted nonzero travel in
the test motion. The corrected four cases pass in `behavior-correction-01.log`; the owning
behavior files pass212 in `behavior-final.log`. Debug/Release adapter builds pass with zero
warnings/errors. Preserve these completed failures. No helper tests or blanket full/H4 rerun
was added. The prior completed normal-verify provenance failure after148passes retains its
accepted applicability. Original runtime delta0; all eight warp gaps, H4/HEAL/JOIN/next-actor,
publicaggregate/h3-witch NONRUN, cleanup21/41/61/67 and speech-policy#517 remain open.

## Source-bound camera observation

The [static source/data owner](../../docs/research/map3-messenger-acceptance.md#source-bound-camera-lifecycle),
[contract](../../docs/design/contracts/map-exploration.md#bound-field-camera-lifecycle) and
[consumer](exploration-programs.md#source-bound-camera) bind destination/scroll/wait/hold and
ordinary-control restoration. Existing producer target/wait pairs and common services are reused;
no Content/schema/input reconstruction is required. Logical B/A origins drive the whole bound
renderer. Tile priority and alpha provide teacher/wall occlusion through the retained textures.

### Reproduce and inspect

Load current `local/private-inputs.ps1` in the launching process and use the locked .NET workflow
above. Reuse `local/issue534/zone-caller/candidate/world.json`,
`local/issue534/opening-implementation/start.json`, the owned installation/project and maintained
`game/probes/engine_input_accessibility_observation.gd`. The case is
`portrait-event-zone-nod-camera-private-*`; follow the route fixture's spatial edges and actual
Wait/Ack boundaries, without historical frame/C quotas or an injected text521 snapshot.

```powershell
. ./local/private-inputs.ps1
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
uv run sf2 verify engine
uv run sf2 verify adapter
uv run --locked python -X utf8 local/issue534/camera-lifecycle/run-dotnet.py debug-new.log build game/Sf2.Remake.Godot.csproj --no-restore --configuration Debug
uv run --locked python -X utf8 local/issue534/camera-lifecycle/run-native.py native-new instant 1 40
# Separate fresh destinations: natural 1 20; adjusted-reveal 1 40.
uv run --locked python -X utf8 local/issue534/camera-lifecycle/compare-native.py
uv run --locked python -X utf8 local/issue534/camera-lifecycle/check-teacher.py
```

The ignored wrappers select the existing shared SDK environment, retained inputs and a fresh output;
they do not install/copy assets or environments. The native wrapper uses fixed-fps60 and the visible
window, a180-second process bound, and the probe has an8000-render-iteration guard for this longer
case. Neither bound defines successful completion. Success requires genuine text531 W1 at
`cs-5149a[127]`, **before Wait/Ack**, with both cameras returned, no-follow still held, F603 clear
and the zone/script caller active. Yes/no128, choice/JOIN and full Messenger remain outside this slice.

### Observed result and projection boundary

**Confirmed (remake only):** `local/issue534/camera-lifecycle/final-native-01/02/03` are
instant/natural20/reveal40, all exit0/PASS with empty failure/unavailable lists and no adapter/audio
or Godot log error. They retain722/725/750 samples and every audio receipt. Comparison covers82
semantic boundaries and5096 settled ticks across24 gameplay fields. For each setting, all3485
prior nod-run settled states match exactly through text521, including entities, RNG/copy, flags,
logical view/windows/portrait and caller/service state. Only projection changes: source origins
replace host centering/clamp/smoothing, and tile priority changes visible occlusion.

| Boundary | Logical service tick / result |
| --- | --- |
| First destination installed / helper return | 3983 /4033; inherited counter0, speed24;48 scroll services plus recheck/final2 |
| First settled origin | B `(912,168)` and A `(912,936)` pixels |
| Second destination installed / helper return | 4250 /4268; counter0, speed24;16 scroll services plus recheck/final2 |
| Second settled origin | B `(912,144)` and A `(912,912)` pixels |
| Final text531 W1 input | 5095, main`6CF70000`, copy`BB`; flags`0,32,256,260,261,601,602`; active ZoneEventContext |

These durations are observations of distance/speed and the helper state machine, not prescribed
route quotas. Earlier introduction1673/A924, entity142 return1979/DA18, second Zone7 return2421/EA7E,
stair reload2473/9B5B and nod3294→3334→3374 remain unchanged. Field control restore is covered by
engine behavior tests; this native endpoint intentionally does not return field control.

`cameraDraws` records5077/5074/5073 actual post-draw states, including3475/3472/3471 prefix draws.
Only equal tick/token projections are admitted. Readback verifies both plane origins, zero duplicate
source offsets,8px source/clip transforms, actor rectangles, float32 viewport culling,44 resource
selection keys and unchanged mutual sprite order. Actual `DrawTextureRectRegion` calls expose their
pass number, priority word, source cell/tile, resource and sprite-ink intersection. Focus loss and
hide/show during both nod and camera work preserve state and zero clock debt; physical Wait/Ack
cannot discharge either consumer.

At genuine text525 input tick4150/token6829, teacher entity142/slot17/sprite209 is `(42,12)`,
layer0/low display priority, facing1, with projected rectangle `(333.3333,327,55,55)` in the
960×640 observation window. His sprite draws at pass21; wall block120 at source `(42,12)` draws
75 masked8px fragments at pass22, with priority words`8272,8275,8A66,9272`.
Decoding the referenced existing resource alpha confirms all362 nontransparent sprite pixels are
covered by opaque wall pixels;214 transparent sprite pixels cause no repair draw. This specific
wall overlap has zero transparent wall pixels. General map alpha is preserved by the same texture
region draws, not a solid rectangle. The three settings reproduce these facts. The final review also retains mosaic sample coverage in the same ink mask; the three continuous
routes execute normal/nod sprites and do not claim a mosaic observation. High display priority
sprites were not reached in this route: their no-repair path and independence from relative sprite
ordering are code-reviewed behavior, not an additional natural observation. Hardware sprite-link
ordering, shadow/highlight, raster/DMA/interrupt timing and full VDP fidelity remain **Unknown**.

All started PCM receipts have sample frames and active playback. Non-speech command/revision order
matches; natural20 has574 speech starts (`70:114,72:59,73:136,74:265`) while instant/reveal40 have0.
This retains the unanswered #517 display/speech policy distinction; it does not normalize audio
receipt sequences or change gameplay RNG.

### Checks and preserved discoveries

Owning behavior files pass234 cases; `uv run sf2 verify engine` passes769 with30 explicit private
skips. The actual adapter Release gate and required Debug build pass. Engine behavior covers varied
coordinates/directions/axes, equality with active bits, retargeting, inherited0/6/7/negative-word
counters,24/32 speed, overshoot, held camera, repeated/nested commands, helper restart/tail,
ordinary-control restoration, enabled/suppressed busy/idle entities, live portrait/window/RNG/copy
order and invalid completions/controls. Dispatcher/TextRunner remain unchanged.

Preserve these completed local discoveries in `local/issue534/camera-lifecycle/`:

- `behavior-01.log`:227PASS. `behavior-02.log`: compilation failed because a new test omitted the
  required ViewWait Recheck argument; `behavior-03.log` passes its three corrected cases and
  `behavior-final.log` passes234. No production rule was weakened.
- `native-02`: exit1 at tick4965/instruction126, application failure null, because the original
  5000-render guard was insufficient for the extended observation. `native-02-correction`: exit1
  at tick397/text483, `focused=false`, zero debt and no application error. Preserve the distinction
  between probe budget and lost native focus. Final runs use the actual focus-controlled window.
- Initial projection runs `native-01` and `priority-native-01` reached531 successfully; the latter
  preceded the correction that keeps sprite-to-sprite order independent of display priority.
  Their outputs remain, and final three runs load the corrected adapter.
- `comparison-failure.json`/`culling-inspection.log`: twelve offline culling discrepancies (four per
  run) at exact viewport-edge contact. Recompute the recorded Godot rectangles with float32 adds;
  double-precision reconstruction from logical coordinates was the report error. Exact float32
  comparison passes without tolerance or runtime changes; independent geometry checks remain.
- The initial teacher checker incorrectly demanded a transparent wall pixel in this particular
  overlap. Direct alpha readback yields362 opaque/0 transparent wall-over-sprite pixels and214
  transparent sprite pixels. The corrected checker asserts the actual asset/coverage facts.

Document scope uses direct links/fences/ownership/private-boundary review, `sf2 design-contracts test`
and `sf2 research-index test`. On clean committed HEAD, obtain `sf2 verify plan --base origin/main
--head HEAD`; record its selection and exact remote CI in the Draft PR/handoff. Source-only prose
adds no new research execution dependency; the prior normal verification completed with missing
local provenance after148 passes and is neither interrupted nor passed. No blanket normal/full/H3
rerun follows from this slice. Preserve all earlier completed failures in their named sections.

No original runtime was added:138 starts /15583.68017570005 seconds /429625 frames /16332 batches,
delta0. Eight first-warp gaps, H4 5340PASS/2FAIL/40Unavailable, HEAL2vs3/later RNG, JOIN's completed
5000 timeout then5313services/572minimal advances and cross-clock Unknown, next-actor1vs0/index29,
public aggregate/h3-witch NONRUN and cleanup21/41/61/67 Unknowns remain. #534/#437 stay open;
#523 stays stopped and investigator ownership remains user-exclusive. No cleanup/archive is implied.

## Source-bound nod observation

The [source owner](../../docs/research/map3-messenger-acceptance.md#source-bound-nod-lifecycle) and
[consumer](exploration-programs.md#source-bound-nod) bind forty explicit services, independently
of presentation latency. `BoundNodsOwnFortyServicesAndResetBeforeTheNextCommand` covers actor
selection, changed seeds/speeds, idle and physically moving actors, enabled/suppressed services,
ten/twenty/ten phases, consecutive commands, reset, early/late/duplicate/stale/wrong completions
and control gating. `NodServicesLiveEntitiesThenWindowAndPortraitWithoutOverwritingPollCopy`
checks live entity→window→portrait/RNG order, moving-window completion and poll-copy retention;
`UnboundNodKeepsLegacyPresentationCompletion` protects the previous profile.

Reuse the retained `local/issue534/zone-caller/candidate/world.json` and unchanged
`local/issue534/opening-implementation/start.json`; no producer, asset, source or environment
copy is needed. The maintained input probe selects `portrait-event-zone-nod-private-*`, follows
the existing spatial trace with real keys and live occupancy, and retains the blocked Left facing
entity142. One explicit Wait then Ack consumes each reached W boundary until text521. At genuine
text521 W1 it stops **before Wait/Ack**, after both nods, with `cs-5149a` instruction42 and
`ZoneEventContext` still active. It does not claim ordinary field return or full Messenger.

After current private selections and protected SDK setup, run the two affected behavior files,
`uv run sf2 verify engine`, `uv run sf2 verify adapter`, and a Debug adapter build for native use.
The retained launcher reads the existing private input selections and starts the owned Godot
installation/project with `--fixed-fps 60 --script res://probes/engine_input_accessibility_observation.gd`,
existing `--private-exploration-start`, fresh `--input-settings`, `SF2_EXPLORATION_OBSERVATION_OUTPUT`,
and the current content/party/spatial-plan selections. It sets `SF2_INPUT_CASE` to the case above,
`SF2_INPUT_RATE` to20 or40 and `SF2_NOD_FOCUS=1` for the instant run's actual native sibling-window
focus transfer. No screenshot or injected gameplay state is used.

```powershell
uv run --locked python -X utf8 local/issue534/nod-lifecycle/run-native.py native-new instant 1 20
# Fresh outputs for natural 1 20 and adjusted-reveal 1 40; keep the same content/start.
uv run --locked python -X utf8 local/issue534/nod-lifecycle/compare-native.py
```

**Confirmed (remake):** retained `native-01/02/03` each exit0/PASS, with empty failures and
Unavailable arrays and clean Godot/audio logs. Direct comparison matches52 semantic boundaries
and3,485 settled tick states across24 fields, including complete selected entities, flags/party,
RNG/copy, logical view/text/portrait, live service flag, caller and nod state. Actual resource
readbacks include elapsed0/9/10/29/30/39/40 for both tokens, entity143/slot18/sprite206/facing1,
animationFF and visible24×24 sprites; lowered textures differ from normal, restored textures
match normal. Hidden-view and real OS focus pauses retain state and zero debt; Wait/Ack during
nod has no effect. Each `nod-returned` observation records counter0, including the transient
reset before the next adjacent nod writes FF.

| Boundary | Logical tick | Main seed / poll copy |
| --- | --- | --- |
| First introduction return | 1673 | A9240000 / A9 |
| Entity142 return | 1979 | DA180000 / DA |
| Second Zone7 return | 2421 | EA7E0000 / AF |
| Stair reload return | 2473 | 9B5B0000 / AF |
| First nod start / lowered / restored / return | 3294 / 3304 / 3324 / 3334 | Live state retained in each receipt |
| Second nod start / lowered / restored / return | 3334 / 3344 / 3364 / 3374 | Live state retained in each receipt |
| Text521 W1 before input | 3484 | 92610000 / 56 |

The endpoint retains flags[0,32,256,260,261,601,602], actor animation0 and active caller; F603 is
clear. All started PCM receipts have real sample frames/playback and no audio error. Non-speech
command/revision order matches across settings. The initial comparison wrongly required every
audio receipt to match: natural20 alone emits speech70/72/73/74 under unresolved policy#517
(332 started speech receipts). `comparison-failure.json` and `comparison-inspection.log` retain
that completed assertion failure; `native-comparison.json` retains **all** receipts. The corrected
comparison separates this known display-policy difference from semantic state and non-speech
receipts. No runtime was repeated or policy changed to hide it.

`behavior-01.log` completed212PASS/6FAIL: four nod cases lacked a logical view in their test state,
and two portrait expectations used +64 instead of the accepted +30. `behavior-02.log` then
completed2PASS/4FAIL because the source-population test used authored actor names; `behavior-03.log`
completed3PASS/1FAIL because it expected enabled idle WaitTimer0 instead of the source idle1.
The corrected cases pass in `behavior-04.log`; final affected files pass219 in `behavior-final.log`,
including varied physical motion. `engine-01.log` passes754 with30 private-input skips; the final
motion assertion refinement is covered by the narrow final run and exact PR CI. Debug/Release
adapter builds have zero warnings/errors. Preserve these completed test-setup failures.

Run the clean committed planner and report exact remote CI. Documentation/source binding adds no
research runner/fixture change and does not authorize a blanket normal/full/H3 repeat. The prior
normal-verify provenance failure after148passes retains its accepted applicability. Original-runtime
delta0; hardware-frame/DMA timing, original natural continuity, later camera/choice/JOIN and all
preserved warp/H4/HEAL/next-actor/cleanup gaps remain open.

## W1 entity-event input

The [execution owner](./exploration-programs.md#w1-in-a-suppressed-entity-event) defines the
admitted gates, token continuation and copy-byte authority. This consumer shares `CanWaitForText`
and the real Wait binding/repeat/rearm/focus gate with plain input. Its accepting poll is distinct
from JOIN. Delivery and reveal add no logical tick debt; changing a W1 occurrence disarms Wait.

Run the affected engine behaviors from `remake/` with the protected SDK environment:

```powershell
& $env:DOTNET_BIN test tests/Sf2.Remake.Engine.Tests/Sf2.Remake.Engine.Tests.csproj `
  -p:RestoreLockedMode=true `
  --filter 'FullyQualifiedName~ExplorationTextWaitTests|FullyQualifiedName~ExplorationSessionTests|FullyQualifiedName~MapEntityLifecycleTests|FullyQualifiedName~CommandsetContinuationTests'
```

`ExplorationTextWaitTests` uses authored text/actors and real session commands, varied seeds and
NPC wait phases, multiple/consecutive W1s, names/newlines/trailing literals, delivery delay,
stale revisions/tokens, wrong callers, Open/Unknown/missing portrait gates, enabled entities and
unsupported controls. The companion classes retain plain input/JOIN, ordinary control, follower,
idle and commandset coverage. For adapter changes run `uv run sf2 verify adapter`; refresh the
Debug DLL for the retained Godot project before native observation. No original emulator or
research/full-route suite is selected by this bounded capability.

The owned `engine_input_accessibility_observation.gd` has `w1-private-instant`,
`w1-private-normal` and `w1-private-adjusted` cases. Reuse the accepted current world/assets,
controlled party and **existing** display-bound R1 start; do not generate or copy inputs. Select
the usual private environment from [exploration reproduction](./exploration-programs.md#reproduction),
then run from the repository root with a fresh ignored output directory `$run`:

```powershell
$env:SF2_INPUT_CASE = 'w1-private-adjusted'
$env:SF2_INPUT_RATE = '20'
$env:SF2_W1_POLLS = '3'
$env:SF2_W1_REVEAL = 'natural' # omit/use confirm to exercise reveal-all input
$env:SF2_EXPLORATION_OBSERVATION_OUTPUT = [IO.Path]::GetFullPath("$run/actual.json")
& $env:GODOT_BIN --headless --path remake/game --fixed-fps 60 `
  --script res://probes/engine_input_accessibility_observation.gd -- `
  --private-exploration-start $selectedR1Start --input-settings "$run/settings.json" `
  *> "$run/godot.log"
$hostExit = $LASTEXITCODE
```

The start is read-only; settings/output must be fresh worktree-local paths. The observer uses only
the existing Left/Left/Right opening setup, then faces and interacts with the live nearby actor128.
That legacy prefix is unadmitted setup, retained with its receipts. The observed W1 must have
text483, F602 clear, entity-event context, Closed portrait and disabled entity service. Read the
actual program/sprite metadata too: sprite195 explicitly has no portrait. The output retains every
command result in `warpRecords` (the observer's existing result collection), state samples and
audio receipts. Check each `rng-text-w1` → `text-seed-copy` → `text-w1-wait` → `text-w1-input`
sequence against its own entry seed; the last input must be `accept`. Require unchanged entities,
one tick per poll, zero delivery debt, no accepting SFX67 and usable ordinary return. Nonzero-Wait
cases hold the final press across Ack and verify it cannot advance the returned field.

**Confirmed (actual input/state/audio):** the retained `local/issue534/w1-implementation/`
launcher/readback and native-10 through native-16 outputs pass these checks with native exit0 and
no application/adapter/script error. Instant and naturally revealed 20-character/second cases cover 0/1/3 optional
polls. The 40-character/second case uses reveal-only Confirm; natural delivery and reveal-all
preserve the admitted state before polling. Each enters at main image `75DA0000`, with copy byte initially unknown; return images are
`FC190000`, `CD4C0000`, `878E0000`, and copy bytes `FC`, `CD`, `87`, respectively. The 20 versus
40-character/second three-Wait cases have equal gameplay entry and return state, including full
entity fields, flags, party resources, seed/copy, windows, PC, tick and event context. They use
the same ordinary startup and semantic inputs without state injection or normalization.

**Unknown:** instant versus adjustable whole-route equivalence remains unproven. Their legacy
prefixes enter W1 at tick142 versus145; entity130 Y, entity131 Y/velocityY and entity133 X/velocityX
differ despite equal seeds. Per-entry poll correctness cannot make those entries equal. The
20/40 comparison is the bounded equal-entry observation. Headless runs do not establish OS
focus-loss behavior or close original W2/typewrite timing, portrait/quake service, 9A or H4.
The pending #517 speech policy is unchanged; this work grants no speech waiver.

Preserved failures: engine-01 stopped at compile errors in new assertions; engine-02 completed
with 183 pass/4 fail (three assertions selected the player instead of the serviced NPC; one
compared separately loaded action-program references). Corrected engine-03 reran only the new
owner, 16 pass; the other 171 behaviors already passed. Native-01 stopped at a probe type-inference
parse error. Native-05 completed with three inapplicable legacy reveal/revision assertions and a
zero-Wait probe that accidentally issued a fresh field Wait; native-06 corrects those observer
errors without changing legacy gameplay. Readback also corrected a compiled program-ID lookup
to the actual cursor. Native-03/05/06/10/13/14/15 retain a warning for six ObjectDB instances
at exit; cleanup cause is **Unknown**, not a passed lifecycle gate. No original launch/capture
was added; original totals, the eight first-warp differences, H4 5340/2/40, HEAL/JOIN and retained
nonrun/cleanup boundaries remain unchanged.

## Ordinary Host Startup

After loading the retained worktree SDK/Godot environment, ordinary play uses:

```powershell
& $godotBinary --path remake/game
& $godotBinary --path remake/game -- --authored-package (Resolve-Path -LiteralPath 'remake/content/authored/garden-watch.json').Path
```

The same project accepts `--private-battle-start` and `--private-exploration-start` with the selected private inputs described below. These and `--authored-package` select mutually exclusive session entries.
An optional `--input-settings <path>` selects the versioned product settings below, independently of
the entry and in either argument order. Each option takes one path and may appear only once.
Unknown/positional, duplicate, conflicting entry and missing-path arguments report
startup ContentError in the common view before creating a session. They cannot select legacy play.

The external observer owns `SF2_OBSERVATION_CASE`, `OUTPUT`, `FOLLOWUP_SHAPE`, `TARGET_SHAPE`,
`ENEMY_SHAPE`, `CONTINUATION_SHAPE`, `LONG_PATH` and `REVERSE_KILLS` (all with the same prefix).
Unset/empty CASE runs the default observation; flags use1. Former `--observation-*`/shape options are
not game arguments. Each example assigns its case/output; optional shapes belong to that named case.
Diagnostics do not affect ordinary launches without the script. Direct startup observation uses:

```powershell
$env:SF2_OBSERVATION_CASE = 'startup'
$env:SF2_OBSERVATION_EXPECT_FAILURE = ''
$env:SF2_OBSERVATION_EXPECT_ORIGIN = 'public-authored-controlled-start'
$env:SF2_OBSERVATION_OUTPUT = Join-Path $env:SF2_RUN_OUTPUT 'startup.json'
& $godotBinary --headless --path remake/game --script res://probes/engine_battle_observation.gd
```

For a rejected real-argument launch, set EXPECT_FAILURE to `unknown-startup-option`,
`duplicate-startup-option`, `conflicting-startup-options` or `missing-startup-path` and append the
corresponding arguments after Godot’s `--`. Private success uses EXPECT_ORIGIN
`private-local-controlled-start`. The script observes actual Main/GameRoot/BattleSessionView and
published startup state; it does not set engine state. Preserve process output and require passed:true.

The legacy `remake/reference/game` host, its `--map3-smoke`/profile arguments and their observations
were retired at M5. Ordinary export configuration excludes `probes/*`; complete ordinary export and
package contents remain unverified.

## Logical Input and Accessibility (ADR 0010 9A)

`GameRoot` loads one `InputSettings`/`GameInput` owner before starting a session, installs its
`sf2_*` Godot InputMap actions, and dispatches each physical event once to the current view.
Exploration, dialogue/choice, battle and return use the same semantic actions. Defaults are:

| Action identity | Keyboard keys | Standard gamepad buttons / axes |
| --- | --- | --- |
| `up` | Up, W | DpadUp, LeftY- |
| `right` | Right, D | DpadRight, LeftX+ |
| `down` | Down, S | DpadDown, LeftY+ |
| `left` | Left, A | DpadLeft, LeftX- |
| `confirm` | Enter, KpEnter, Z | South |
| `cancel` | Escape, X | East |
| `attack` | F | West |
| `spell` | H | North |
| `item` | I | Back |
| `target` | Tab | RightShoulder |
| `stay` | Space | LeftShoulder |

Confirm talks, acknowledges dialogue, answers Yes, or selects/commits in battle. Cancel answers No
or cancels battle provisional choices; it does not acknowledge dialogue. Spell cycles learned spells
and levels with self as the initial target. Item selects and cycles carried item slots, initially
targeting self; Target cycles living candidates for the selected action.
All accepted commands still use Application's existing legality and state. F replaces the former X
attack binding; former C/Y/N exploration shortcuts are replaced by Confirm/Cancel. View help and
spell/item hints read effective bindings, including after a convention swap.

Keyboard echo is ignored. A stick deflection crossing magnitude0.5 submits one action; it must return
below that threshold or change sign to submit another. Unrelated axes and releases cannot repeat a
held direction. All standard Godot-mapped gamepads are accepted. This is product input behavior,
not a claim about original hardware scancodes or repeat cadence; physical controller-driver mapping
and hot-plug behavior are outside the injected native-event observations.

For example, save this UTF-8 JSON in a local settings file and pass its absolute path:

```json
{
  "formatVersion": 1,
  "confirmCancel": "swapped",
  "reducedFlash": true,
  "textMode": "adjustable",
  "charactersPerSecond": 40,
  "bindings": {
    "attack": { "keys": ["R"], "buttons": ["West"], "axes": [] }
  }
}
```

```powershell
& $godotBinary --path remake/game -- --input-settings $settingsPath
```

`formatVersion` is required and currently1. Other fields default to `standard`, false, `instant`,
40 and an empty binding override map. `confirmCancel` accepts `standard` or `swapped`; swapping
exchanges the **whole effective Confirm and Cancel binding sets after overrides**, across both
keyboard and gamepad, without changing their semantic roles. `textMode` accepts `instant` or
`adjustable`; `charactersPerSecond` must be finite in1–240 even in instant mode.

Each binding override replaces one complete action and requires all three arrays (`keys`, `buttons`,
`axes`), at least one key and at least one button or axis. Key identities are case-sensitive Godot
`Key` enum names (for example `I`, `F1`, `Space`, `Escape`); `None` and standalone modifier keys
`Shift`/`Ctrl`/`Alt`/`Meta` are rejected, and key chords are not part of version1. Button identities
are the names in the table plus `Start`, `LeftStick` and `RightStick`. Signed axis identities
are `LeftX-`, `LeftX+`, `LeftY-`, `LeftY+`, `RightX-`, `RightX+`, `RightY-`, `RightY+`.
Unknown fields/actions/identities, conflicting bindings, missing device access and invalid values
produce `ContentError: invalid-input-settings` before a session is published. A missing or unreadable
explicit file also fails; it never silently restores defaults. No file selects documented defaults.
Settings are loaded once at startup; there is no in-game settings UI or automatic settings save.

Adjustable text uses the actual Label's visible-character count. Time reveals characters but never
submits a gameplay command. Confirm while text is incomplete reveals the rest and preserves the real
wait token; a subsequent Confirm acknowledges. A choice with incomplete text likewise reveals first
and still requires a semantic Yes/No input. Instant text displays everything and still awaits its
normal acknowledgement and choice. A completed reveal cannot complete a presentation cue.

Reduced-flash keeps white FadeOut/FadeIn overlay opacity at0 for the same service duration. It still
submits `CompletePresentation` with the reached token and cue kind. Black fades and other services
retain their existing behavior. This intentional visual deviation, logical remapping and modern text
progression belong to ADR0010 9A/10A; they do not establish original visual/timing parity or 8C/H4.

### Native 9A observation

After the locked environment and Debug adapter build, select a fresh ignored output directory. The
bounded external observer writes an authored variation/settings into the specified destinations,
then instantiates ordinary `Main.tscn` with those actual startup arguments. It never sets session
state. It adds white presentation cues, a second spell, ordinary physical definitions/rewards and
an adjacent durable opponent to the selected existing authored world.

```powershell
$env:SF2_INPUT_CASE = 'keyboard' # gamepad, remapped-keyboard, remapped-gamepad
$env:SF2_INPUT_VARIANT = 'harbor' # hill exercises another world, actor and spellbook
$env:SF2_INPUT_RATE = '20'       # also observe80 with the hill variation
$env:SF2_EXPLORATION_OBSERVATION_OUTPUT = Join-Path $env:SF2_RUN_OUTPUT 'input.json'
$package = Join-Path $env:SF2_RUN_OUTPUT 'input-package.json'
$settings = Join-Path $env:SF2_RUN_OUTPUT 'input-settings.json'
$arguments = @('--headless', '--path', 'remake/game', '--fixed-fps', '60',
    '--script', 'res://probes/engine_input_accessibility_observation.gd', '--',
    '--authored-package', $package)
if ($env:SF2_INPUT_CASE.StartsWith('remapped-')) { $arguments += @('--input-settings', $settings) }
& $godotBinary @arguments
```

The remapped cases replace every action binding, move stick input to the right stick, swap the
Confirm/Cancel convention and select reduced-flash/adjustable text. Require `passed:true`, empty
failures and clean process errors. Observe actual field movement, decline/re-prompt/acceptance,
partial/full text reveal with retained waits, both real completed white cue tokens, the same session
at first battle control, movement/cancel, two spells, another allied target, actual HEAL, another
actor's Stay, next-round attack and binding-derived help. Gamepad cases also check repeated axis
values, unrelated trigger events and release edges against the actual session revision/preview.
Compare default keyboard/gamepad and remapped harbor outputs at the matching semantic checkpoints:
flags, actor, round, resources, seeds and white token/kind completion must agree; only the intended
presentation/settings differ. The hill case checks independent valid content rather than a fixed
receipt sequence. Default white opacity reaches1; reduced-flash stays0.

For the current private continuous outcome/return observation, reuse the existing private selection,
R1 start and opening navigation setup from [exploration reproduction](./exploration-programs.md#reproduction).
Select `SF2_INPUT_CASE=private-remapped-gamepad`, `SF2_BATTLE_OUTCOME_CASE=victory`, and the new observer
script, with `--private-exploration-start <selected-R1-start> --input-settings <ignored-settings-path>`.
It reuses the existing actual-input outcome driver, including its single-frame input cadence, with
all commands mapped to the remapped/swapped gamepad. This case uses instant text and ordinary flashes;
the authored cases own the paired accessibility comparison. Require the existing main/admission/
outcome reports to pass through usable return movement, retained session identity and valid node
projection. The settings startup matrix uses the existing `engine_battle_observation.gd` startup
case with EXPECT_FAILURE=`invalid-input-settings`; require no session/actors/RNG on rejected input.
Each launch tests an actual startup configuration in the retained project/installation; no editor,
project copy, screenshot, original emulator, reference replay or test of the observer is needed.

## Continuous H4 Comparison

The external [comparison module](../../src/sf2tool/remake_h4_comparison.py) and
[ordinary-input probe](../game/probes/engine_h4_observation.gd) execute the accepted original field
route through natural Battle01 first control. Their current battle scope is the first STAY and the
next selected actor, explicitly a diagnostic extension after divergent initial state/order. They
never chase an actor, reseed, inject state, or substitute the older outcome probe's winning policy.
The [ten-layer contract](../../docs/design/contracts/map3-battle01-continuous-scenario.md) still owns
whole-run acceptance. This bounded comparison always reports `milestonePass: false`.

Use the retained environment, shared Godot and current Debug adapter build. Load
`local/private-inputs.ps1` in the launching process and retain the existing worktree-local SDK/cache
selections. After an adapter change, build `game/Sf2.Remake.Godot.csproj --configuration Debug
--no-restore` from `remake/`, with absolute `DOTNET_BIN` and `DOTNET_CLI_HOME` and
`DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`. No research suite or helper tests are required.
Select the seven private world/battle/static/enemy inputs and R1 controlled party/start described
in [exploration reproduction](./exploration-programs.md#reproduction). Product inputs supply actual
content; they are not original expected values. Reuse a compatible prepared world, or regenerate it
with the accepted compiler and pinned clean upstream into this worktree's fresh ignored output.
Never edit its semantics or the controlled party to make comparison pass.

First produce the accepted read-only reference using the contract's projector command, or reuse
that completed projection. `$acceptedEvidenceRoot` below selects only its prepared-68..86 lineage.
Choose a fresh worktree-local `$run` directory; all plans, JSONL observations, logs and reports are
private. After an owned debug process has exited, the direct bounded invocation is:

```powershell
uv run --locked python -m sf2tool.remake_h4_comparison plan `
  --reference $reference --evidence-root $acceptedEvidenceRoot --output "$run/plan.json"
$env:SF2_H4_PLAN = [IO.Path]::GetFullPath("$run/plan.json")
$env:SF2_H4_ACTUAL = [IO.Path]::GetFullPath("$run/actual.jsonl")
& $env:GODOT_BIN --headless --path remake/game --fixed-fps 60 `
  --script res://probes/engine_h4_observation.gd -- `
  --private-exploration-start $selectedR1Start *> "$run/godot.log"
$hostExit = $LASTEXITCODE
uv run --locked python -m sf2tool.remake_h4_comparison compare `
  --reference $reference --plan "$run/plan.json" --actual "$run/actual.jsonl" `
  --host-log "$run/godot.log" --host-exit $hostExit --output "$run/comparison.json"
$comparisonExit = $LASTEXITCODE
```

The plan derives each field movement from a consumed original displacement and matching cardinal
request, adds evidenced stationary facing changes, and identifies field Confirm from idle
`WaitForEvent-action` request/result boundaries. It does not convert held-frame counts to modern
steps. The first battle movement uses actual one-frame nonneutral `battle:movement-input` polls
before the committed decision. The plan reads only the needed accepted checkpoint files to bind
those polls and shared `DisplayText:entry` IDs; the latter includes direct callers omitted by the
projector's narrower cutscene-text callbacks. Acknowledging actual dialogue/choices uses ordinary
keys. Original shimmed DisplayText completion remains insufficient for natural reveal/ack equality.

The probe subscribes to `SessionResultObserved` from `node_added` before Begin/Attach, retaining
every result, rejection and error alongside live snapshots. Reparented views retain their existing
subscription. The comparator checks session identity, monotonic records, observation watermarks
and repeated attach records. Every key press, including acknowledgements, has an actual input
ordinal; logical route steps and original request ordinals remain separate. Screenshots are unused.

Read both the report and process log. Comparison exit 1 means an observed FAIL; exit 2 means only
Unavailable assertions. Host exit 2 can be an explicitly recorded next-actor divergence, a route
failure, or a probe error; its terminal reason and error log distinguish them. Pass the recorded
process exit even when re-comparing a completed run. Abnormal exits, including crashes after a
complete terminal record, and explicit probe failures/timeouts are FAIL with observation scope.
The controlled next-actor stop is classified as diagnostic only when its actual comparison also
fails. Missing actual flags or loadout fields are Unavailable; an available typed loadout Items
array is compared at admission. A clean process exit alone is not H4 acceptance.
The report records layer/assertion, original record/Git owner, actual
sequence/input, expected/actual/result/reason and diagnostic scope. Unreached or unbound groups and
unexecuted 9A variants remain Unavailable, alongside observed failures.

**Confirmed (retained #528 baseline):** the prior R1 product selection admitted gold 0 where the selected original readback
has 60. Natural first control exposed different carried item slots and first-round order. The corrected
connected start selects source NewGame gold 60 and all four source item words for each of the first
three allies; the separate controlled R1 fixture's gold 0 and four item bytes do not establish those values.
The input chain is `map3-opening-party.json` → `ControlledBattleStartReader` (slot order and gold preserved) →
`PrivateBattleScenarioReader` (definition SourceLoadout and BattleStartInput) →
`EngineBattleActor` (loadout fallback to its definition) → live inventories. The old input/actual
differences did not authorize changing expected evidence. Admission `SourceLoadout`/`Progress` were
null in the existing observation; later inventories cannot retrospectively prove that boundary.
NPC phase differences and timing-to-RNG mapping remain separately Unknown. A different seed
readback is not an established RNG algorithm defect. Independent route/state assertions remain
usable after an admission difference; subsequent battle decisions cease to be a continuous
original comparison after turn-order divergence.

**Confirmed (corrected input, bounded actual run):** the retained original reference and plan,
recompared with a new host run and recorded exit 2, report 5,340 PASS, 2 FAIL and 40 Unavailable.
Admission gold is 60 on both sides. At natural first control, each of the three complete live
item arrays matches. First-round turn order still differs, and the diagnostic first STAY reaches
a different next actor; the host stops there. Admission `SourceLoadout` remains null in the actual
projection, so the later matching inventories do not fill that unavailable assertion. RNG timing
and the later NPC contribution remain Unknown; this is not an H4 pass.

**Confirmed (bounded #534 order diagnosis):** `local/issue525/reference.json` and
`local/issue530/actual-corrected.jsonl` agree at R1 admission on main seed
`0x9917` (32-bit image `0x99170000`). The earliest matching movement-position
readbacks then differ: at tile `(55,3)` before original movement acceptance 2,
the seed is `0xC632` (`local/issue525/plan-v2.json` step 2,
`prepared-68/runtime/checkpoints.jsonl:9`, order 745, frame 368, input ordinal 1),
while the probe's logical `after:1` at the same tile still reads `0x9917`
(actual sequence 52, input 1). `0xC632` is one advance of the pinned
`(seed * 13 + 7) & 0xFFFF` generator from `0x9917`. Separately, the original
30-frame Left request spans two movements and ends at tile `(54,3)` with seed
`0x1091` (`prepared-68/runtime/actual-inputs.jsonl:772`, frame 385, order 787),
two advances from admission. It is not the paired boundary for actual `after:1`.
The original field segment did not capture either caller. The probe presses and
releases each logical movement and settles it, without reproducing the original
held-frame schedule. Equal tile position does not prove equal elapsed poll/caller
opportunities. This persistent-state difference precedes the comparator's first
scored FAIL at battle control, but does not establish a movement or RNG rule defect.

**Confirmed (R1 entity readback and pinned source shape):** the original R1 projection
at `prepared-68/runtime/checkpoints.jsonl:2` (order 711, frame 355) has Map 3
walking entities in physical slots 5, 6 and 8. Slot 5 at `(20,13)` has an
`ACTSCRIPTWAITTIMER` of 30 and action pointer `0xFF5600`, the first command of
`eas_Walking`; slots 6 and 8 have zero wait timers and pointers `0xFF565A` and
`0xFF568C`, respectively, each 40 bytes into its 50-byte copy of the script at
`ac_waitDest`. Slot 8 is mid-movement (raw Y 3453 toward destination 3072).
`s1_entities.asm` defines those three `msWalkingEntity` entries;
`SetWalkingActscript` copies the 50-byte script per walker. In pinned SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6`, `eas_Walking` orders
`ac_wait 30`, `ac_randomWalk`, `ac_waitDest`, and `ac_wait 20`; the entity update
visits occupied physical slots in order without a viewport filter. A completed
wait redispatches in the same update, and `esc06_walkRandomly` calls the main
generator with range 4 for attempted directions. The first range-4 draw from
`0x9917` produces `0xC632` and direction 3 (Down), a legal slot-5 candidate.

**Inferred (bounded first-field caller):** slot 5's wait is already armed at R1,
while slots 6 and 8 must finish destination and wait phases before another random
walk. This makes slot 5 the source-supported walking-NPC candidate for the first
field advance, but the retained field segment did not capture a caller PC or an
entity update at that exact draw. Other source callers at that boundary have not
been excluded. The retained `local/issue530/actual-corrected.jsonl` run predates
the selected-start phase binding: its admission sequence 2 shows slots 5, 6 and 8
stationary at action cursor 0; its first walker draws occur at sequence 79/tick 23
for slot 8, sequence 83/tick 25 for slot 6, and sequence 85/tick 26 for slot 5.

**Confirmed (bounded selected R1 phase correction):** the original R1 observer
retained each 32-byte entity record; pinned `disasm/sf2enums.asm` offsets decode
all 18 fields consumed by `EntityMotionState` for the three walkers. The selected
controlled start binds those fields to existing action cursors and motion gates
after scene allocation, before the on-load program. The first eligible actual
engine update preserves allocation and aliases, advances the seed `0x9917` to
`0xC632` when slot 5 walks Down toward `(20,14)`, begins slot 6's wait 20, and
moves slot 8 from raw Y 3453 to 3450. This source-rule match does not locate the
uncaptured original caller or align host ticks to original frames. The affected
private `FullOriginalOpeningConsumesR1InputsAndMatchesR2AndR2aThroughStableFieldControl`
test still fails at original logical input index 29: after the preceding Right,
the expected player tile is `(20,14)` but actual remains `(19,14)`. The actual
command reports `movement-blocked`; host traversal allows `(20,14)`, while slot
5 is settled there with flags A `0xEF`, and the host occupancy predicate is true.
The selected route's autonomous/input opportunity mapping and any later movement
rule difference remain **Unknown**. This correction is not an H4 pass; the
original route assertion and fixture remain unchanged.

**Confirmed (bounded ordinary host prefix):** with the selected R1 phase input,
the closed accepted #517 audio world (asset-library commit `7219d9c6`) and the
first 30 unchanged logical steps of
`local/issue525/plan-v2.json`, the existing ordinary-input observer retained one
session, 514 result signals, no reported session-result failure and native exit 0.
At actual `after:1`, player tile `(55,3)` and seed `0xC632` match the earliest
original same-tile boundary above; slot 5 is already moving Down. At `after:30`,
the player has reached `(21,14)` and slot 5 has vacated `(20,14)`. Thus this host
prefix does not reproduce the minimal-tick test's occupancy rejection. It does
not exercise JOIN or establish a complete original-to-host opportunity schedule.
The first ignored run (`local/issue534/host-prefix-01/`) also logged two
post-terminal `Parameter "f" is null` errors: the observer still subscribed to
result signals after closing its output. The observer now disconnects after its
terminal state read and before writing/closing the terminal record. Repeating
the same prefix in `local/issue534/host-prefix-02/` retained 1,109 JSONL records,
all 514 result signals, the same first seed and end tile, native exit 0 and no
Godot log errors. The prefix plan is `local/issue534/ordinary-prefix-plan.json`,
derived by retaining steps 1–30 from the prior plan and removing only its later
battle-decision fields; each run uses a fresh worktree-local output destination.
Neither run establishes finite-JOIN compatibility or an H4 milestone pass.

**Confirmed (selected-R1 first-control diagnostic):** the former 5,000-frame
observer settle cap expired while finite `MUSIC_JOIN` was still playing. A
single 30-second monotonic deadline per settle, with frames/elapsed reported on
timeout, lets the unchanged 260-step plan finish JOIN naturally: the music has
start/finish receipts, flag 603 is present at the next field input, and the
route reaches first battle control. The one corrected ordinary-input run in
`local/issue534/first-control-02/` has native exit 2 for its intentional
next-actor stop, no host errors, one session and all 12,404 result signals.
Direct comparison gives 5,340 PASS, 40 Unavailable and two FAIL. The first
failure is the first-round scored turn order; the actual first actor remains
ally 2 and the first STAY at `(9,16)` is consumed, but the next actor is ally 1
versus original ally 0. This reaches only the existing diagnostic boundary,
not the full battle or an H4 milestone pass.

### Retained first-control opportunity alignment

**Confirmed (retained first-control opportunity alignment):** the 260 paired field
`before` boundaries in that completed run match map/tile and the checked logical input-idle
predicate; only steps 1 and 2 match seed. At step 3 after the first warp, original
`prepared-68/runtime/checkpoints.jsonl:23` (order 1039/frame 506) reads `0x75DA`, while
actual sequence 62/tick 16 reads `0xC632`. The original held-input endpoint `(54,3)` above is
not this post-warp `(3,3)` boundary. Actual's next advance `0xC632` to `0x1091` is at
sequence 75/tick 21 when slot 6 begins an East walk; the original per-draw caller/slot is
still Unknown. Walking remains a source-supported candidate, not a captured caller receipt.

The retained finite JOIN SoundWait begins at actual sequence 4871/tick 2114 and its natural
finish receipt appears at sequence 15498/tick 7427: 5,313 simulation-tick results accompany
the 429,803-sample/44,100-Hz cue (about 9.746 seconds). The 373 observed seed transitions
total 572 minimal LCG-equivalent advances; no per-draw entity identity is emitted. The
host's `ExplorationSessionView.NeedsTicks` permits busy entities to advance during a
presentation wait, while `SessionAudio` supplies actual finite completion. **Inferred:**
host-speed or accessibility-dependent delivery latency can change the carried seed.
No cross-speed/settings rerun has established that dependence empirically.

The [accepted Option A policy](../../docs/decisions/0010-map3-battle01-product-acceptance.md#evidenced-gameplay-waits-accepted-option-a)
and [Wait admission contract](../../docs/design/contracts/map3-battle01-continuous-scenario.md#evidenced-gameplay-waits)
now require evidenced gameplay waits and distinguish mandatory source work from player
waiting and host delivery latency. This is a policy boundary, not an implemented or verified
schedule. Early warp caller/phase ordering, source audio logical end/interleaving and natural
dialogue/scene scheduling remain Unknown. The current 260-step plan has no automatically
admitted Wait mapping. Keep 5,340 PASS / 2 FAIL / 40 Unavailable, the completed JOIN timeout
and index-29 occupancy failure above; do not repeat them solely because policy changed.
Next conformance work must first bind its named caller/gates/order, preserve source-required
logical work during cues and require both logical and actual finite completion. Same semantic
Wait/ack streams across 9A settings must preserve gameplay state/RNG; an additional evidenced
player Wait may differ. No neutral-frame padding, PCM-to-tick conversion, desired-seed solve,
golden rewrite or H4/`0x6DC1` guarantee follows from this decision.

At the original before-battle boundary (`prepared-72/runtime/checkpoints.jsonl:4013`,
order 55395), the seed is `0x6DC1`. The retained projection contains 885
base-generator draws in that before-battle consumer scope. The complete captured
caller/range partition is 690 at return PC `0x65A8`/range 256 (`symbol_wait1`),
75 at `0x647E`/range 256 (`@wait2`), 95 at `0x155BC`/range 5 (portrait mouth
counter), and 25 at `0x15576`/range 120 (portrait blink counter). Pinned
SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6` places the first two
in `disasm/code/common/scripting/text/textfunctions_1.asm` and the latter two in
`disasm/code/common/menus/portraitfunctions.asm:VInt_PerformPortraitBlinking`.
Each text-wait poll advances `GenerateRandomNumber(256)`, copies its result to the
high byte of `RANDOM_SEED_COPY`, waits for VInt, then tests directional and A/B/C
input; the accepting poll draws too. The portrait service advances the same main
seed when an active portrait's counters require a blink or mouth reset. These are
presentation callers with gameplay-visible shared-seed effects; the 120 portrait
draws are classified by pinned source rather than left as unknown callers.
The selected-R1 actual `round-rng` result at sequence 25798 starts from
`0xD9E2`. Read-only
replay using the same nine first-round agility values `4,5,7,5,5,5,5,5,5`, pinned
`GenerateRandomNumber` in `disasm/code/common/tech/randomnumbergenerator.asm` and
`GenerateBattleTurnOrder`/`AddCombatantAndRandomizedAgiToTurnOrder` in
`disasm/code/gameflow/battle/battleloop/turnorderfunctions.asm` reproduces both complete
scored arrays in `local/issue534/first-control-02/comparison.json`: 27 draws take original
`0x6DC1` to `0x79CE` and actual `0xD9E2` to `0x10E3`. The first-dispatch and
actual `round-rng` end seeds match those calculations. From this checkout, reproduce
the bounded source-rule replay without loading private inputs:

```powershell
@'
actors = [(0,4),(1,5),(2,7)] + [(i,5) for i in range(128,134)]
def roll(seed, n):
    seed = (seed * 13 + 7) & 0xffff
    return seed, (((seed * ((n * 2) & 0xffff)) >> 16) >> 1)
for name, seed in (("original",0x6dc1),("actual",0xd9e2)):
    rows = []
    for actor, agility in actors:
        r = agility >> 3
        seed, plus = roll(seed,r)
        seed, minus = roll(seed,r)
        seed, tie = roll(seed,3)
        rows.append((actor,(agility + plus - minus + tie - 1) & 255))
    rows.sort(key=lambda row: (row[1] + 128) % 256 - 128, reverse=True)
    print(name,hex(seed),rows)
'@ | uv run --locked python -X utf8 -
```

To reproduce the comparison report from completed evidence without a host run,
load the selected private environment as above and choose a fresh ignored output:

```powershell
uv run --locked python -m sf2tool.remake_h4_comparison compare `
  --reference local/issue525/reference.json --plan local/issue534/first-control-02/plan.json `
  --actual local/issue534/first-control-02/actual.jsonl `
  --host-log local/issue534/first-control-02/godot.log --host-exit 2 `
  --output local/issue534/comparison-reread.json
```

To reproduce the bounded caller partition and R1 entity phase from retained
private readbacks without a native run (after selecting the private environment):

```powershell
@'
import json
from collections import Counter
r = json.load(open('local/issue525/reference.json', encoding='utf-8'))
cut = r['encounter'][0]['source']['order']
print(Counter((hex(x['caller']['returnPc']), x['range']) for x in r['rng']
              if x['source']['order'] < cut
              and any(s['name'] == 'battle:before' for s in x['consumerScopes'])))
print([(i, hex(r['inherited']['entities'][i]['actionScript']),
        r['inherited']['entities'][i]['waitTimer'],
        r['inherited']['entities'][i]['y'], r['inherited']['entities'][i]['destinationY'])
       for i in (5, 6, 8)])
for line in open('local/issue530/actual-corrected.jsonl', encoding='utf-8'):
    row = json.loads(line)
    if row.get('sequence') in (2, 52, 79, 83, 85) and 'state' in row:
        state = row['state']
        print(row['sequence'], state['simulationTick'],
              hex(int(state['mainSeed']) >> 16),
              [(int(e['slot']), int(e['actionCursor']))
               for e in state['entities'] if e['slot'] in (5, 6, 8)])
'@ | uv run --locked python -X utf8 -
```

**Inferred:** differing pre-round seeds fully explain the two scored arrays under
the same source rule. Exploration/dialogue caller scheduling is a credible source
of the seed difference, but the complete call correspondence is not established.
The matching per-seed replays provide no evidence of a turn-order or main-generator
rule defect. The comparator correctly leaves first-control `mainSeed readback`
Unavailable because its timing-to-RNG mapping is not established; scored order and diagnostic next actor
remain FAIL. The exact caller of the early original field draw, complete
exploration-to-battle call correspondence, and later NPC phase contribution remain
**Unknown**. Use the pinned source call sites and retained request/checkpoint
records to narrow the remaining caller and semantic event. If that still leaves a
material gap for H4, separately admit a focused original caller/range/seed observation at
the first field divergence and pair it with actual simulation/input events before
changing a rule; this is a conditional evidence need, not an automatic emulator run.
The deterministic comparison condition is also unresolved: the selected original
route includes elapsed polling and caller opportunities while the probe applies
immediate logical inputs. A behavioral contract must say which gameplay-affecting
opportunities the host must preserve, including acknowledgement and route results,
without equating original and host clocks or frame counts. Reassess this against
accepted wait/consumer behavior before a timing-dependent production correction.
The accepted contract compares reached turn scores/order and gameplay decisions;
ADR 0010/0019 do not impose original held-frame or text-wait timing on the host.
Do not reseed, pad waits, alter the selected golden, or assert a general engine
defect from this trace. Whole H4 remains open.

The accepted [post-victory original extension](../../docs/research/map3-messenger-acceptance.md#native-post-victory-ordinary-input-result-issue-515)
supplies Down from Map57 `(5,12)` to `(5,13)`. This driver does not reach that boundary or bind its
new-chain payload; its Unavailable result means missing comparison/actual execution, not absent
original evidence. Complete battle, return, resource/consumer provenance and 9A acceptance remain open.

## Exploration and Program Observations

Use [the exploration/program owner](./exploration-programs.md#reproduction) for the current
Content preparation, controlled starts, selected private comparisons and ordinary native recipes.
The native observers exercise real input and read the same live session across mode changes. `engine_map3_opening_observation.gd` reads the R2 fixture only as an external input trace and covers complete acceptance or decline/re-prompt. `map3-opening-party.json` supplies the named opening party. The
private source programs retain their exact unsupported native/population frontiers; a stopped prefix
is not a completed original route. `EntityMotionTests` directly compares the extracted core and
destination admission to the13 owned H3 cases; it does not test a verification program.

## Authored Start State Observation

The format-v7 reader returns immutable definitions and explicit start input. After loading the existing
worktree environment, refresh the actual Debug adapter assembly before the existing no-image probe.
Run the four tracked packages through the same public session: the two HEAL packages use the default
observer (eight checkpoints each), and the two physical packages use `SF2_OBSERVATION_CASE=physical`
(seven checkpoints each). Existing integer/RNG/action expectations remain unchanged.

A differing controlled start reuses the yard's definitions and deployment; only its start object changes:

```powershell
$data = Get-Content -LiteralPath 'remake/content/authored/practice-yard.json' -Raw | ConvertFrom-Json
$data.start.actors[0].positionOverride = [pscustomobject]@{x=4; y=3}
$data.start.actors[0].mp = 9
$data.start.actors[0].kills = 12
$data.start.actors[0].defeats = 3
$data.start.gold = 1234
$data.start.thinkingSeed = [uint32]0x12344321
$packagePath = Join-Path $env:SF2_RUN_OUTPUT 'controlled-start.json'
$data | ConvertTo-Json -Depth 30 | Set-Content -LiteralPath $packagePath -Encoding utf8NoBOM
$outputPath = Join-Path $env:SF2_RUN_OUTPUT 'controlled-start-observation.json'
$env:SF2_OBSERVATION_CASE = ''
$env:SF2_OBSERVATION_REVERSE_KILLS = '0'
$env:SF2_OBSERVATION_LONG_PATH = '0'
$env:SF2_OBSERVATION_OUTPUT = $outputPath
& $godotBinary --headless --path remake/game --script res://probes/engine_battle_observation.gd -- --authored-package $packagePath
```

Inspect all eight checkpoints and the process log, not just exit0: the actual actor starts at(4,3),
MP9 becomes6 through HEAL, gold1234/kills12/defeats3 and thinking seed12344321h remain carried. The
probe still drives movement/cancel, HEAL/STAY, next round and attributed rejection. It changes no live
state and emits no images. `BattleStartStateTests` separately reuses the same admitted definition
object with two explicit typed starts and independent histories; no tests of this observer are added.

All mutation examples below keep actor/rule maxima in `actors`, current resources in `start.actors`,
and actual encounter layout changes in `encounters[].placements`. Adding a deployed actor also
requires its explicit start record; no default counter or runtime actor is synthesized from a definition.

## Authored Extra Round Action Observation

Use the same installed editor/project after the affected Debug build. This format-v7 input changes only
one actor's explicit eligibility; its numerical agility remains12. Outputs stay in a fresh ignored run:

```powershell
$data = Get-Content -LiteralPath 'remake/content/authored/practice-yard.json' -Raw | ConvertFrom-Json
$data.actors[0].extraRoundAction = $true
$packagePath = Join-Path $env:SF2_RUN_OUTPUT 'extra-turn-input.json'
$data | ConvertTo-Json -Depth 16 | Set-Content -LiteralPath $packagePath -Encoding utf8NoBOM
$outputPath = Join-Path $env:SF2_RUN_OUTPUT 'extra-turn-observation.json'
$env:SF2_OBSERVATION_CASE = 'extra-turn'
$env:SF2_OBSERVATION_REVERSE_KILLS = '0'
$env:SF2_OBSERVATION_LONG_PATH = '0'
$env:SF2_OBSERVATION_OUTPUT = $outputPath
& $godotBinary --headless --path remake/game --script res://probes/engine_battle_observation.gd -- --authored-package $packagePath
```

Require all seven checkpoints and clean logs: initial control, uncommitted STAY selection, consumption
of the first and second entries, natural round2, and both entries again. Real Enter/Space input reaches
the same actor at queue cursor1, then the other ally at3 after automatic Stay AI. From the configured
high word1234, eleven draws end atFF4D and twenty-two at887A; low word1234 and thinking seed are carried.
The ordinary package still consumes nine draws. STAY leaves HP/MP, gold and actor placement unchanged.
The script reads the existing result/state/node interface; it adds no gameplay setter or new adapter
observation fields. Use the four package observations below for the preserved ordinary configurations.

`BattleAgilityTurnsTests` owns equal-agility definition variation, actual Content validation, capacity
and cross-round behavior. `TurnOrderRulesTests` keeps the independent signed0/127 fixture and scalar
seed expectations, including zero-range draws and the truncated secondary basis. `EnemyActionTests`
retains death/counter results and skips already-generated extra entries for dead actors. The selected
existing first-round reference methods named below exercise the actual source projection; no old
aggregate, new H3 or test of the observer is required.

## Authored Faction and Order Observation

Run the four package observations below, then prepare a changed-order physical input in a fresh ignored
run directory. This format-v7 variant keeps independent accepted combat/RNG expectations while moving
all orders beyond the original byte-side boundary and reversing all three JSON arrays:

```powershell
$data = Get-Content -LiteralPath 'remake/content/authored/stone-court.json' -Raw | ConvertFrom-Json
$orders = @(255, 1000, 70000, 80000, [int]::MaxValue)
$ordered = @($data.encounters[0].placements | Sort-Object processingOrder)
for ($i = 0; $i -lt $ordered.Count; $i++) { $ordered[$i].processingOrder = $orders[$i] }
[array]::Reverse($data.actors)
[array]::Reverse($data.encounters[0].placements)
[array]::Reverse($data.start.actors)
$packagePath = Join-Path $env:SF2_RUN_OUTPUT 'sparse-order-input.json'
$data | ConvertTo-Json -Depth 16 | Set-Content -LiteralPath $packagePath -Encoding utf8NoBOM
$outputPath = Join-Path $env:SF2_RUN_OUTPUT 'sparse-order-observation.json'
$env:SF2_OBSERVATION_CASE = 'physical'
$env:SF2_OBSERVATION_REVERSE_KILLS = '0'
$env:SF2_OBSERVATION_LONG_PATH = '0'
$env:SF2_OBSERVATION_OUTPUT = $outputPath
& $godotBinary --headless --path remake/game --script res://probes/engine_battle_observation.gd -- --authored-package $packagePath
```

Require seven physical checkpoints, real target input/node removal/next control and clean process logs.
Apply the same order/array transform after constructing the `secondary` multi-target input below to
observe actual AI selection with sparse orders; its five independent checkpoints and RNG expectations
are unchanged. `BattleFactionOrderTests` separately exercises a low-order enemy, renamed high-order
ally, healing/opposing movement, atomic rejection and rewards. `TurnOrderRulesTests` retains the signed
boundary fixture with `ActorRef` identities, a real order255 and separate null sentinels. The selected
existing reference methods `BaselineTestsThreeRegionsWithoutActivatingThemAndComputesTheAcceptedRound`
and `RoundGenerationRetainsTheIndependentSeedCopyWithoutUsingIt` exercise the actual source projection;
no old aggregate or new H3 observation is required.

## Authored Battle Observation

After loading the existing worktree environment, build the actual Debug assembly once when needed
by the existing Godot instance. Release adapter verification alone does not refresh Godot's Debug DLL.
From `remake/`:

```powershell
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
& $env:DOTNET_BIN build game/Sf2.Remake.Godot.csproj --configuration Debug --no-restore
```

No-argument local Godot startup opens the authored yard. To select either real package, use the
existing verified Godot executable with `--path remake/game -- --authored-package <package-path>`.
Input is WASD/arrows for preview, Enter for action choice/commit, H for the known HEAL with self
initially selected, Tab for living allied targets, Space for STAY and Escape for cancel. Physical attack selection defaults to F; Tab then selects living opponents. Packages without physical
definitions report Unsupported. Application runs AI and rounds automatically.

Tab cycles from the last attempted UI candidate, including a rejected one. A range rejection preserves
the session's accepted target and battle state; subsequent Tab input can reach later living targets of the selected action. The HUD
shows both attempted and accepted target after rejection. Cancel or the next action clears the UI cursor.
The map viewport keeps the acting origin, preview and target visible with clipping/pan/zoom. Wide
windows place a scrollable HUD beside the map; narrow windows place it below. Authored UI uses actual
window dimensions.

From the repository root, the direct observation command is:

```powershell
$packageName = 'practice-yard' # or garden-watch
$packagePath = (Resolve-Path -LiteralPath "remake/content/authored/$packageName.json").Path
$outputPath = Join-Path $env:SF2_RUN_OUTPUT 'observation.json'
$env:SF2_OBSERVATION_CASE = ''
$env:SF2_OBSERVATION_REVERSE_KILLS = '0'
$env:SF2_OBSERVATION_LONG_PATH = '0'
$env:SF2_OBSERVATION_OUTPUT = $outputPath
& $godotBinary --headless --path remake/game --script res://probes/engine_battle_observation.gd -- --authored-package $packagePath
```

`$godotBinary` is the already verified local installation, and `SF2_RUN_OUTPUT` is an explicit ignored
worktree-local destination from the existing environment. Run packages serially in that same project;
their startup selection is the concrete reason for separate observation processes. The script injects
real `InputEventKey` events, reads the common session result and actual HUD/node geometry/visibility,
checks cancellation, HEAL/STAY, carried RNG and automatic progression, and exits nonzero on failure.
Its JSON/log output is local generated evidence; it never emits images or modifies session state.
No tests of this observation script are required.

For the connected physical chain use the same installation/project and an ignored output directory:

```powershell
$packagePath = (Resolve-Path -LiteralPath 'remake/content/authored/stone-court.json').Path
$outputPath = Join-Path $env:SF2_RUN_OUTPUT 'physical.json'
$env:SF2_OBSERVATION_CASE = 'physical'
$env:SF2_OBSERVATION_REVERSE_KILLS = '0'
$env:SF2_OBSERVATION_LONG_PATH = '0'
$env:SF2_OBSERVATION_OUTPUT = $outputPath
& $godotBinary --headless --path remake/game --script res://probes/engine_battle_observation.gd -- --authored-package $packagePath
```

Repeat with `river-post.json`; `SF2_OBSERVATION_REVERSE_KILLS=1` selects the opposite legal kill order. The observation
uses actual F/Tab/Enter/Space input, including a rejected distant target followed by a valid target,
then checks exact carried RNG/resources, removed coordinates/nodes, next living control and the next
round. All four package/order combinations use common product commands. Reached unsupported
level/leader/outcome atomicity and independent arithmetic expectations belong to
`PhysicalBattleTests`, not tests of this observer. The legacy Domain comparisons of the shared scalar
extraction were retired at M5; shared strike, counter and reward changes use `PhysicalBattleTests`,
`PhysicalRuleConfigurationTests` and, with private inputs required, `PrivateActionBindingTests`.

For follow-up observation create one supported variant in the existing ignored output directory.
The four shapes use `stone-court` with initial high seed73 for `sticky`/`second-death`, seed55 for
`ally-death`, and `river-post` with seed55 for `counter`. All use low seed word0x1234, unchanged
placements and Stay AI. This is controlled startup input; the running session is never reseeded.

```powershell
$shape = 'sticky' # counter, ally-death, second-death
$packageName = if ($shape -eq 'counter') { 'river-post' } else { 'stone-court' }
$initialSeed = if ($shape -in @('sticky', 'second-death')) { 73 } else { 55 }
$data = Get-Content -Raw -LiteralPath "remake/content/authored/$packageName.json" | ConvertFrom-Json -AsHashtable
$data.start.mainSeed = [uint32]($initialSeed * 65536 + 4660)
foreach ($index in @(0, 2)) { $data.start.actors[$index].hp = 500; $data.actors[$index].maxHp = 500 }
$data.actors[2].attack = if ($shape -eq 'counter') { 26 } else { 18 }
if ($shape -eq 'ally-death') {
    $data.start.actors[0].hp = 1; $data.start.actors[0].exp = 99; $data.start.actors[0].defeats = 6
}
if ($shape -eq 'second-death') { $data.start.actors[2].hp = 35 }
$packagePath = Join-Path $env:SF2_RUN_OUTPUT 'package.json'
$data | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $packagePath -Encoding utf8NoBOM
$outputPath = Join-Path $env:SF2_RUN_OUTPUT 'followups.json'
$env:SF2_OBSERVATION_CASE = 'followups'
$env:SF2_OBSERVATION_REVERSE_KILLS = '0'
$env:SF2_OBSERVATION_LONG_PATH = '0'
$env:SF2_OBSERVATION_OUTPUT = $outputPath
$env:SF2_OBSERVATION_FOLLOWUP_SHAPE = $shape
& $godotBinary --headless --path remake/game --script res://probes/engine_battle_observation.gd -- --authored-package $packagePath
```

Use a fresh output directory per shape and run serially in the same installed project. The observer
presses actual Enter/F/Tab/Space, checks Actor/Target reversal and ordered first/second/counter
observations, exact draw count/seed/resources, death visibility/accounting and subsequent control.
The ally-death shape checks that EXP99 stays99 and no award draws occur. The second-death shape
checks cancellation of the first hit's already-set counter. The observer is executed directly,
without tests of the observer or helper.

The same direct script also accepts `SF2_OBSERVATION_CASE=target-cycle` and `SF2_OBSERVATION_CASE=layout`.
It sets a representative 960×540 host window because a headless SceneTree script otherwise starts a
64×64 physical window. The layout case resizes the actual window to 640×480 and 1280×720, checks real
map/HUD and actor/preview rectangles, scrolls the HUD with real mouse-wheel events and commits movement
with real keys. `SF2_OBSERVATION_LONG_PATH=1` additionally traverses a valid long preview before that observation.

Prepare minimal public authored variants in a fresh ignored output directory, without editing packages:

```powershell
$caseDirectory = Join-Path $env:SF2_RUN_OUTPUT 'inputs'
New-Item -ItemType Directory -Path $caseDirectory | Out-Null
$cycle = Get-Content -LiteralPath 'remake/content/authored/practice-yard.json' -Raw | ConvertFrom-Json
$laterAlly = $cycle.actors[1] | ConvertTo-Json -Depth 8 | ConvertFrom-Json
$laterAlly.id = 'guard-c'
$laterAlly.agility = 1
$cycle.actors += $laterAlly
$laterStart = $cycle.start.actors[1] | ConvertTo-Json -Depth 8 | ConvertFrom-Json
$laterStart.actor = 'guard-c'
$cycle.start.actors += $laterStart
$cycle.encounters[0].placements += [pscustomobject]@{actor='guard-c'; faction='ally'; processingOrder=10; control='player'; aiStrategy=$null; x=3; y=4}
$cycle | ConvertTo-Json -Depth 15 | Set-Content -LiteralPath (Join-Path $caseDirectory 'three-allies.json') -Encoding utf8NoBOM
$layout = Get-Content -LiteralPath 'remake/content/authored/practice-yard.json' -Raw | ConvertFrom-Json
$layout.terrains[0].rows = @(1..48 | ForEach-Object { 'g' * 48 })
$layout.actors[0].move = 255
$layout.encounters[0].placements[0].x = 47
$layout | ConvertTo-Json -Depth 15 | Set-Content -LiteralPath (Join-Path $caseDirectory 'full-square.json') -Encoding utf8NoBOM
```

Use those respective paths with `--authored-package`, the selected observation case and a fresh
`SF2_OBSERVATION_OUTPUT` path. The targeting variant places the rejected ally before a legal adjacent
ally; the layout variant spans both admitted dimensions. Both execute the same production session and view,
with no runtime seed/state injection. Preserve original failed observations and run only the affected
case after a correction; adapter-only fixes do not require replaying unchanged engine/reference suites.

Private trust remains in the transitional reference reader until its capability migrates. Its affected
direct comparison is the existing `PrivateCanonicalMap3ImportReaderTests.UnknownShapeAndProvenanceDriftFailClosed`
case, selected alone from the Content.Tests project; it passes after relocation without selecting the
old aggregate or adding a dependency from Engine.Tests to Reference. Do not turn this selected reference
check into an automatic legacy-suite obligation for every new engine change.
The extracted weighted rule also retains the selected Domain.Tests
`Battle01PlayerMovementTests.WeightedPropagationMatchesTheAcceptedRuntimeMatrix` comparison, including
its flat-row and bucket-wrap cases. It passes through the reference wrapper; the authored engine's
logical row-edge behavior has its own actual movement unit assertion. No original fixture changed.

### Semantic Terrain Observation

Format-v7 terrain rows reference explicit local `legend` definitions. Current packages use
`g = {surface: open, protection: light}`, `p = {surface: open, protection: none}`,
`b = {surface: brush, protection: heavy}` and `# = {surface: barrier, protection: none}` where used.
The weighted AI recipe explicitly adds `d = {surface: deep, protection: heavy}` before using that glyph.
These are authored references, not original terrain indexes; missing glyph definitions reject.

`BattleTerrainTests` varies surface through actual preview/cancel/commit, and target protection through
AI lethality selection and physical settlement. Existing movement, critical and moved-original-actor
counter cases preserve their independent cost/HP/RNG expectations. Run the four package recipes and
the enemy/target/continuation variants below with the existing probe, including weighted, occupied,
unreachable and startup shapes. Require complete checkpoints, actual input/state and clean logs.
No new native branch, screenshot or state setter is needed for these consumers.

Run affected references together: `WeightedPropagationMatchesTheAcceptedRuntimeMatrix` (all five
controlled source cases), `ClassZeroUsesSourceRegularCostsWithBowiesTwelvePointBudgetAndObstructedSky`,
`HealerTerrainWeightsChangeBudgetAdmissionAndLandEffectRemainsSeparate`,
`CentaurForestHillsAndDesertCostsChangeReachabilityUnderTheSameBudget`,
`AlliesAreTraversableButOnlyVacantDestinationsCanBeConfirmedAndEnemiesBlockPropagation`,
`ActualOrderIncludesTheInactivePrefixOccupiedFallbackAndBothCommandsets`,
`SourceDestinationFailureCompletesOriginStayAndKeepsTheThinkingHistory`,
`RadiusSearchUsesStrictLowerCostFirstTieAndOwnCellZeroBeforeOccupancy`,
`SourceArithmeticKeepsZeroIntermediateAndBothDownwardDrawsAtTheOriginalRange`,
`HoveringTerrainZeroUsesUnreducedDamageAndTheSameLethalEarlyReturn`,
`CounterHalvesBeforeSpreadAndConsumesItsOwnFlagsWithoutAnotherAttack`, and
`ScriptThreeAndSelectionRetainLethalityBranchClassCohortAndMovementTieOrder`.
Source mover tables, occupancy bits and explicit flat-row behavior stay at the real reference
projection into the same kernel. These bounded comparisons do not complete private battle
continuation, broader mover admission or full ADR0009/0010 acceptance.

### Independent Control and AI Strategy

Current format-v7 `encounters[].placements[]` owns both choices, separately from actor capabilities:

| control | aiStrategy | Executed behavior |
| --- | --- | --- |
| `player` | `null` | Application yields actual player input; currently ally-only. |
| `automatic` | `stay` | Explicitly consume the automatic entry without attack or movement; currently enemy-only. |
| `automatic` | `attack-then-approach` | Attempt the supported physical attack; if none can be selected, run the accepted approach or origin-Stay continuation and end the action. |

Missing or incompatible pairs reject; no unknown strategy defaults to Stay. The existing engine
control/AI tests exercise shared actor definitions under distinct encounter assignments, actual
player/automatic results, invalid Content and reusable starts. Existing enemy, target-selection and
commandset-continuation cases retain independent damage, queue, memory and seed expectations.

Use the player HEAL/STAY and physical recipes plus the enemy-actions, target-selection and
commandset-continuation variants below with the retained editor/project. Their configuration selects
`placements[2].aiStrategy`; source command observations remain ATTACK1/script3, HEAL1, SUPPORT and MOVE1.
Observe complete startup, attack/counter, no-target pursuit, occupied origin Stay, next input and
Unsupported atomicity checkpoints with clean logs. The existing native probe is unchanged.

Run related control/target/continuation references as one selected group, including
`ActualOrderIncludesTheInactivePrefixOccupiedFallbackAndBothCommandsets`,
`SourceDestinationFailureCompletesOriginStayAndKeepsTheThinkingHistory`,
`RadiusSearchUsesStrictLowerCostFirstTieAndOwnCellZeroBeforeOccupancy`,
`ScriptThreeAndSelectionRetainLethalityBranchClassCohortAndMovementTieOrder`,
`CompletedEnemyPrefixHandsOnlyActualBowieHisOwnRegularRangeAndCancelOrigin`, and
`CounterReversesRolesAndCommitsPrimaryThenCounterAndExpAsOneEnemyReceipt`.
These comparisons preserve their source domains; they do not admit activation, wider AI or private
initialized entry and do not constitute complete ADR0009 acceptance.

### Physical Critical Configuration

Format-v7 physical definitions explicitly pair `critical.chance` and `critical.damageBonus`.
The stone package uses `one-in-16` / `quarter`; the river package uses `one-in-32` / `half`.
The existing physical, follow-up and enemy-action recipes select these fields directly and use the
same installed Godot project/probe. For this configuration boundary, observe both ordinary physical
packages, all four follow-up shapes (`sticky`, `counter`, `ally-death`, `second-death`), and the enemy
`counter` shape with both packages. Require all38 checkpoints across those eight cases and clean logs.
No new native branch, screenshot or runtime seed setter is needed.

`PhysicalRuleConfigurationTests` also varies both rules for the same player/enemy attack, preserving
independent damage/RNG/reward values and checking the reversed counter's own critical range. Its
controlled start gives critical seed1 after the ordinary round and dodge; the next spread words20/267
both yield zero. Base78 therefore becomes117 for a half bonus or97 for a quarter bonus. Existing
seed55 counter expectations remain; the test does not derive expectations from product output.

Run the affected reference group together: `SourceArithmeticKeepsZeroIntermediateAndBothDownwardDrawsAtTheOriginalRange`,
`MissAndCriticalUseRealSeedsAndPreserveSourceCallOrder`,
`CounterHalvesBeforeSpreadAndConsumesItsOwnFlagsWithoutAnotherAttack`,
`CounterReversesRolesAndCommitsPrimaryThenCounterAndExpAsOneEnemyReceipt`,
`SecondDefeatPreservesTheFirstCorpseAndCreditsOnlyTheNewTarget`,
`RealSeedsExerciseMissCriticalAndIndependentExpVariance` and
`KillAccountingUsesSourceCapsAndKeepsUnknownInputsUnknown`. These existing tests compare real scalar,
reaction, reward and death boundaries without a reference aggregate or private input regeneration.

### Enemy action observation

The enemy branch uses the same reader, session and existing native observer. Start from either
physical package and prepare this controlled input in an ignored worktree-local output directory:

```powershell
$packageName = 'stone-court' # river-post also supports the counter shape
$shape = 'counter' # movement, enemy-death, unsupported use stone-court
$data = Get-Content -Raw -LiteralPath "remake/content/authored/$packageName.json" | ConvertFrom-Json -AsHashtable
$data.start.mainSeed = [uint32](55 * 65536 + 4660)
foreach ($index in @(0, 2)) {
    $data.start.actors[$index].hp = 500
    $data.actors[$index].maxHp = 500
    $data.actors[$index].defense = 4
}
$data.actors[0].attack = 18
$data.actors[0].physical.critical = @{chance='one-in-32'; damageBonus='half'}
$data.actors[2].attack = 30
$data.actors[2].physical.critical = @{chance='one-in-16'; damageBonus='quarter'}
$data.encounters[0].placements[2].aiStrategy = 'attack-then-approach'
$data.actors[2].move = 1
if ($shape -eq 'movement') {
    $data.actors[2].move = 3
    $data.encounters[0].placements[2].x = 7
    $data.encounters[0].placements[3].x = 6
    $data.encounters[0].placements[3].y = 5
}
if ($shape -in @('enemy-death', 'unsupported')) { $data.start.actors[2].hp = 1 }
if ($shape -eq 'unsupported') { $data.start.actors[0].exp = 99 }
$packagePath = Join-Path $env:SF2_RUN_OUTPUT 'enemy-input.json'
$data | ConvertTo-Json -Depth 16 | Set-Content -LiteralPath $packagePath -Encoding utf8NoBOM
$outputPath = Join-Path $env:SF2_RUN_OUTPUT 'enemy-observation.json'
$env:SF2_OBSERVATION_CASE = 'enemy-actions'
$env:SF2_OBSERVATION_REVERSE_KILLS = '0'
$env:SF2_OBSERVATION_LONG_PATH = '0'
$env:SF2_OBSERVATION_OUTPUT = $outputPath
$env:SF2_OBSERVATION_ENEMY_SHAPE = $shape
& $godotBinary --headless --path remake/game --script res://probes/engine_battle_observation.gd -- --authored-package $packagePath
```

The four checkpoints cover initial state, real player STAY selection, automatic enemy action and
stable next control/Unsupported. The counter hits the ally for22 and the enemy for7, awards the
ally1 EXP, and carries main0x557E1234/thinking0x02EF0042. Counter kill instead awards24 EXP and19 gold,
removes the enemy node and ends at main0xB1BC1234. The late level-up shape preserves all enemy-action
state while retaining the preceding player's committed turn. Movement chooses (4,3) from (7,3).
These are reference expectations for the supplied configurations, never production dispatch rules.
Inspect native logs for script/process errors and require all four checkpoints, as well as the
reported pass and exit status; a script that stops before its final checkpoint is incomplete.

`EnemyActionTests` owns independent behavior checks for changed targets/configurations, continued
history, dead secondary queue entries, automatic startup and per-enemy failure boundaries. For the
shared thinking calculation, select existing reference methods
`SignedRangeBoundariesStillAdvanceTheHighByteOnce` and `HighByteSignedEdgesMatchTheExistingThinkingHelper`.
`ScriptThreeAndSelectionRetainLethalityBranchClassCohortAndMovementTieOrder` and the affected scalar/
counter methods above retain the existing source comparison. No new H3 or legacy aggregate is required.

### Multi-target selection observation

Use the same installed editor, project and fresh ignored output with competing configured actors:

```powershell
$packageName = 'stone-court' # river-post also supports secondary
$shape = 'secondary' # primary, movement, missing-class use stone-court
$data = Get-Content -Raw -LiteralPath "remake/content/authored/$packageName.json" | ConvertFrom-Json -AsHashtable
$data.start.mainSeed = [uint32](55 * 65536 + 4660)
$data.start.thinkingSeed = [uint32]0x00EF0042
foreach ($index in @(0, 1, 2)) {
    $data.start.actors[$index].hp = 500
    $data.actors[$index].maxHp = 500
    $data.actors[$index].defense = 4
    $data.actors[$index].attack = if ($index -eq 2) { 30 } else { 18 }
    $data.actors[$index].physical.critical = if ($index -eq 2) { @{chance='one-in-16'; damageBonus='quarter'} } else { @{chance='one-in-32'; damageBonus='half'} }
}
$data.actors[0].classRule = if ($shape -eq 'primary') { 'unpromoted-swordsman' } else { 'unpromoted-warrior' }
$data.actors[1].classRule = if ($shape -eq 'primary') { 'unpromoted-warrior' } else { 'unpromoted-swordsman' }
$data.encounters[0].placements[2].aiStrategy = 'attack-then-approach'
$data.actors[2].move = 1
$data.encounters[0].placements[1].x = $data.encounters[0].placements[2].x - 1
$data.encounters[0].placements[1].y = $data.encounters[0].placements[2].y
if ($shape -eq 'movement') {
    $data.actors[0].classRule = 'ordinary'
    $data.actors[1].classRule = 'ordinary'
    $data.actors[2].move = 8
    $data.encounters[0].placements[0].x = 1; $data.encounters[0].placements[0].y = 1
    $data.encounters[0].placements[1].x = 1; $data.encounters[0].placements[1].y = 4
    $data.encounters[0].placements[2].x = 7; $data.encounters[0].placements[2].y = 3
}
if ($shape -eq 'missing-class') { $data.actors[0].classRule = 'ordinary' }
$packagePath = Join-Path $env:SF2_RUN_OUTPUT 'targets-input.json'
$data | ConvertTo-Json -Depth 16 | Set-Content -LiteralPath $packagePath -Encoding utf8NoBOM
$outputPath = Join-Path $env:SF2_RUN_OUTPUT 'targets-observation.json'
$env:SF2_OBSERVATION_CASE = 'target-selection'
$env:SF2_OBSERVATION_REVERSE_KILLS = '0'
$env:SF2_OBSERVATION_LONG_PATH = '0'
$env:SF2_OBSERVATION_OUTPUT = $outputPath
$env:SF2_OBSERVATION_TARGET_SHAPE = $shape
& $godotBinary --headless --path remake/game --script res://probes/engine_battle_observation.gd -- --authored-package $packagePath
```

Both adjacent candidates consume thinking draws in reverse processing order: 1 then2 produce raw19 each,
reported cap15, so changing only the class definitions changes the chosen actor. The movement shape
has costs12/14 and priority1, selecting the farther target without requiring class metadata. Each
successful shape has five checkpoints through actual player commands into round2: main RNG carries
0x557E1234 → 0x97231234 → 0xE0E11234, thinking carries0x02EF0042 → 0x01EF0042, and the second enemy
action selects the primary actor from the evolving candidates. Missing class in the critical cohort
has four checkpoints and preserves the entire failed enemy action. Require the exact checkpoint
count, clean logs and successful result. JSON numbers are floats in GDScript; nested expected numeric
arrays must preserve that representation rather than failing an otherwise equal observed value.

`TargetSelectionTests` owns actual rule/content/session expectations, including raw/capped and
signed-byte edges, class tables, same-class/equal-movement ties, reordered configuration/processing orders and
atomic missing-data/late-settlement rejection. Direct affected reference selections are
`ScriptThreeAndSelectionRetainLethalityBranchClassCohortAndMovementTieOrder`,
`ActualRoundSixAttackReplaysHpOnceAndAdvancesToBowieWithAllRandomChannels`,
`ActualRoundEightChesterHitReplaysTheSelectedProfileAndPreservesFirstDefeatAccounting`,
`DamagedEnemyAfterChesterAttackSelectsBowieAndPreservesEarnedExp`,
`CounterReversesRolesAndCommitsPrimaryThenCounterAndExpAsOneEnemyReceipt` and
`PostHealBowieCounterUsesItsOwnPermissionAndOneEnemyReceipt`. These execute the retained real ranking
consumers through the shared selector; no reference runner or native-probe tests are added.

### Commandset06 continuation observation

Use the existing installed editor/project and fresh ignored output. Both physical packages support
`move-attack`; the other shapes below use `stone-court`. No live state or seed setter is used.

```powershell
$packageName = 'stone-court' # river-post also supports move-attack
$shape = 'move-attack' # occupied, unreachable, startup, weighted, secondary
$data = Get-Content -Raw -LiteralPath "remake/content/authored/$packageName.json" | ConvertFrom-Json -AsHashtable
$data.start.mainSeed = [uint32](42 * 65536 + 4660)
foreach ($index in @(0, 2)) {
    $data.start.actors[$index].hp = 500
    $data.actors[$index].maxHp = 500
    $data.actors[$index].defense = 4
    $data.actors[$index].attack = if ($index -eq 2) { 30 } else { 18 }
    $data.actors[$index].physical.critical = if ($index -eq 2) { @{chance='one-in-16'; damageBonus='quarter'} } else { @{chance='one-in-32'; damageBonus='half'} }
}
$data.encounters[0].placements[2].aiStrategy = 'attack-then-approach'
$data.actors[2].move = 3
$placements = $data.encounters[0].placements
$placements[0].x = 1
$placements[0].y = if ($packageName -eq 'stone-court') { 3 } else { 4 }
$placements[2].x = 7
$placements[2].y = $placements[0].y
if ($shape -eq 'occupied') {
    $data.actors[2].move = 1
    $placements[3].x = 6; $placements[3].y = 3
}
if ($shape -eq 'unreachable') {
    foreach ($y in 1..5) { $data.terrains[0].rows[$y] = '#pp#pppp#' }
}
if ($shape -eq 'startup') { $data.actors[2].agility = 60 }
if ($shape -eq 'secondary') { $placements[0].y = 1; $placements[1].y = 3 }
if ($shape -eq 'weighted') {
    $data.actors[2].move = 1
    $placements[0].x = 4; $placements[0].y = 3
    $placements[1].x = 7; $placements[1].y = 1
    $placements[3].x = 2; $placements[3].y = 5
    $data.terrains[0].legend.d = @{surface='deep'; protection='heavy'}
    foreach ($y in 1..2) { $data.terrains[0].rows[$y] = '#ppppppd#' }
}
$packagePath = Join-Path $env:SF2_RUN_OUTPUT 'commandset-package.json'
$data | ConvertTo-Json -Depth 30 | Set-Content -LiteralPath $packagePath -Encoding utf8NoBOM
$outputPath = Join-Path $env:SF2_RUN_OUTPUT 'commandset-observation.json'
$env:SF2_OBSERVATION_CASE = 'commandset-continuation'
$env:SF2_OBSERVATION_REVERSE_KILLS = '0'
$env:SF2_OBSERVATION_LONG_PATH = '0'
$env:SF2_OBSERVATION_OUTPUT = $outputPath
$env:SF2_OBSERVATION_CONTINUATION_SHAPE = $shape
& $godotBinary --headless --path remake/game --script res://probes/engine_battle_observation.gd -- --authored-package $packagePath
```

Require exit0, `passed: true`, clean logs without script errors, and all checkpoints: six for
`move-attack`, three for `startup`, four for each other shape. The ordinary move ends at x5 after
fixed cost4 and keeps main0xDC7F1234/thinking0xBEEF0042. Next-round input produces main0xEE281234,
then ATTACK1 at x2 with allyHP478/enemyHP493/allyEXP1 and final main0xDAA61234/thinking0x02EF0042.
Occupied MOV1 ends at origin x7 but MOVE1 returns0. Weighted costs prefer the farther swordsman and
correct the station to x6. Changed positions select the secondary actor without a thinking draw.
Unreachable costs preserve the player commit and failed enemy queue entry as Unsupported.

`CommandsetContinuationTests` owns Content/session behavior, source walk/unsigned-cost boundaries,
missing rewards at the reached attack, and the independently specified continued RNG results.
Affected actual reference methods are `ActualOrderIncludesTheInactivePrefixOccupiedFallbackAndBothCommandsets`,
`RepeatedPursuitStopsAtTheActualRoundSixAttackCohortBeforeAnyMutation`,
`SourceDestinationFailureCompletesOriginStayAndKeepsTheThinkingHistory`,
`RadiusSearchUsesStrictLowerCostFirstTieAndOwnCellZeroBeforeOccupancy`,
`PreliminaryWalkRetainsAccumulatedBitsAndSupportsAValidEmptyPath`,
`IncompleteOrHighTargetCostsRejectBeforeTheDisputedClassBranch`,
`LaterPursuitAcceptsTheRewoundPhysicalMainSeedAndRetainsDamagedAlly`,
`FirstAllyDefeatRemovesChesterFromBlockingAndBothTargetCohorts`,
`SourceDirectionMaskRetainsAnEarlierHigherCostBranchAndRejectsAnIncompletePath`,
`RemainingEnemiesUseTheirOwnMemoryAndEvolvingOccupancyBeforeActualBowieEntry` and
`ActualRoundSixAttackReplaysHpOnceAndAdvancesToBowieWithAllRandomChannels` in the existing Domain test
project. Select those methods with `dotnet test --filter`; no new helper/probe tests, H3 replay or
legacy aggregate is needed. The committed planner and engine/adapter entries remain the gate owners.

## Optional Private .NET Checks

Private engine checks use xUnit's explicit private-input selection through `PrivateInputFact`.
`PrivateBattleEncounterTests` and `PrivateBattleInitializationTests` are ordinary facts over in-memory
authored records and always run.

| Engine.Tests family | Required selections |
| --- | --- |
| `PrivateBattleScenarioTests`, `PrivateSourceAiTests`, `PrivateActionBindingTests` | `SF2_PRIVATE_BATTLE01_DATA`, `SF2_PRIVATE_BATTLE01_SCENE`, `SF2_PRIVATE_BATTLE01_TERRAIN`, `SF2_PRIVATE_STATIC_DATA`, `SF2_PRIVATE_ENEMY_DATA`, `SF2_PRIVATE_ENEMY_GOLD`, plus `SF2_PRIVATE_CONTROLLED_START` for the scenario and source-AI checks |
| `PrivateExplorationTests`, `PrivateBattleEntryProgramTests`, `PrivateBattleOutcomeProgramTests` | the six battle inputs above plus `SF2_PRIVATE_EXPLORATION_CONTENT` |

`PrivatePresentationBytesAndRequiredSpriteLinksAreAdmittedBeforeStartup` additionally requires a world
prepared with `--rom-path` and `--presentation-root`; a world prepared without the local asset pack
cannot exercise it. The Battle01 exports and enemy gold come from the pinned H2 rails below; the
canonical import from `uv run sf2 h2 map-import`; the world from the
[exploration preparation](./exploration-programs.md#reproduction).

With no selected inputs, an optional check reports **skipped / no private assertions ran**. Partial
selection fails. For a required check set `SF2_REQUIRE_PRIVATE_TESTS=1` before discovery; missing inputs
then fail. `--filter` does not opt into private execution and other switch values do not require it.
Scope variables to that run and keep logs/output local. Do not interpret public success with private
skips as private acceptance.

[Local Private Inputs](../../docs/operations/local-private-inputs.md) owns registered read-only input
selection. Load it in the same process as any command that needs those inputs. Check the configured
shared ROM and its narrow identity command before reporting it missing; keep configuration, missing
input, identity mismatch, and later upstream/toolchain failure distinct. No copied input is required
solely to repair the appearance of an aggregate result.

## Selected Battle01 Startup Inputs

The production `PrivateBattleEncounterReader` consumes the selected Battle01 data and scene JSON
exports plus compressed terrain. Their exact identities remain in
[battle01-data](../../manifests/extractions/battle01-data.json),
[battle01-scene](../../manifests/extractions/battle01-scene.json), and that Content reader. These
transport identities describe fixed private admission; they are not the general authored-content
schema or a gameplay legality condition. The compressed terrain input is the H1 split output
`disasm/data/battles/entries/battle01/terrain.bin`, so it exists only after a passing H1 rebuild.

Reuse accepted exports read-only after checking identity. If the owning task actually needs to
reproduce them, the existing `scripts/Export-Battle01Data.ps1` and `Export-Battle01Scene.ps1` consume
the pinned upstream and explicit ignored destinations. New maintained tooling still belongs in
`src/sf2tool/`; this is a frozen compatibility route, not permission to add more PowerShell rails.

The [Map 3 implementation/reference record](./map03-playability-plan.md) retains selected inputs,
exact control histories, source-derived expectations, test method names and completed failures.
Use the named seam needed by a comparison. Its historical instructions to replay prefixes, preserve
every refusal, run full managed suites or inspect frame totals are not current engine acceptance rules.
Fixed traces remain external comparison data; obsolete trace guards and their tests can retire through
the separately owned engine migration. Preserve original H2/H3 goldens and their provenance.

## Repository Planner

On a clean committed head, inspect current selection without executing gates:

```powershell
uv run sf2 verify plan --base origin/main --head HEAD
```

The [planner](../../src/sf2tool/verification_plan.py) automatically selects `engine-unit` and/or
`adapter-build` for ordinary product/test paths, without the old solution or always-run `public-core`.
Non-research documentation and retired engine-test paths add no execution. Engine test changes select
unit tests; ordinary game changes select ordinary compilation. Controlled inputs under
`remake/reference/inputs/` select `engine-unit`, because Engine.Tests copies them and actual private
engine facts consume them. Shared product/build inputs select both actual downstream builds.

Shared CLI/harness/planner changes retain conservative research selection by default. For a declared
engine-only wiring change in those shared modules,
inspect the actual diff and use:

```powershell
uv run sf2 verify plan --scope engine --base origin/main --head HEAD
```

That explicit scope adds `research-public` direct lint/traceability/index checks for the wiring and
rejects research artifacts or other shared inputs. Path admission cannot detect a semantic research
change inside an allowed module: such a change must retain its research plan/dependency. Unknown
research paths keep conservative fanout. Retired verification tests do not fall back to the old suite.

The existing normal `uv run sf2 verify` command still performs its public stages followed by selected
private identity/provenance stages. It remains the research lane's normal route under its owner.
It is not a new-engine or documentation-only default. Preserve completed stage results and failures;
do not rerun it just to replace a known upstream-availability failure with green.

`uv run sf2 verify --full` remains exceptional for an applicable research milestone, materially shared
evidence-harness change, release boundary, or explicit full-parity request. An ordinary engine slice
does not inherit it. Test/gate wall time and earlier SHA changes do not by themselves invalidate results.

## Retired Official Godot Gate

The former `sf2tool.remake_godot` import/export gate verified only the explicit public-synthetic
reference host, not ordinary game packaging, and is retired together with that profile. Its bounded
native process runner now lives in `sf2tool.bounded_process` for the asset candidate builder. No
maintained gate verifies ordinary package/export contents; claim none without an actually executed,
separately owned export check. Repeated behavior observations use the existing
installation/project/instance.

## GitHub Public

Current [public-checks.yml](../../.github/workflows/public-checks.yml) remains triggered on every pull
request and main push. Its Windows jobs are:

| Job | Current execution |
| --- | --- |
| `scope` | Compare the actual event Git range; fail on a missing/invalid range or failed comparison. Emit the three affected-path booleans. |
| `engine-unit` | Locked restore/build/test of Engine.Tests and its actual production dependencies, using the pinned .NET SDK/runtime. No Python, native Godot, private input or legacy test project. |
| `adapter-build` | Locked restore/build of the ordinary game C# project. No Godot tests or native editor launch. |
| `research-public` | Locked Python/uv dependencies, Ruff, direct design-contract traceability and research-index checks. No engine or verification-tool pytest families. |

Shared product/build inputs select engine and adapter; engine tests select engine; `remake/game/`
selects the ordinary adapter. `remake/reference/inputs/` selects engine for its Engine.Tests consumers. Research/contracts/source/fixtures/manifests/schema inputs select
research. Workflow and shared CLI/harness/planner changes select
all three. Non-research documentation and legacy remake test edits select no product jobs. Other non-remake inputs conservatively select research. The full
predicate lives in the workflow; add a genuinely consumed external unit input when that dependency
is introduced. M1's consumed authored JSON lives under `remake/content/`, which conservatively selects all product/host builds.

Main-gate owns required-check configuration. M5 removes the `reference-host-build` job, so its
required-check context must be removed at integration.
Verify the actual candidate CI/check state at integration; changing configuration remains main-gate
authority. Path-irrelevant jobs report skipped; applicable jobs need a
real successful result. Review the diff and first applicable CI outcome directly, without tests of
job selection, workflow text or the migration. Research/private protections remain independently owned.

## Process, Path, and Artifact Safety

### Worktree Environment and Run Evidence

Reuse the dedicated isolated worktree after its previous topic is merged, tracked state is clean and
owned processes are settled. Start the next topic at accepted main in that worktree. Separate writers
and a specifically required reproduction environment justify another worktree; a slice number does not.

| State | Lifetime |
| --- | --- |
| Python environment and uv cache | Reuse worktree-local `UV_PROJECT_ENVIRONMENT` and `UV_CACHE_DIR`; sync the lock only when needed. |
| NuGet packages and HTTP cache | Reuse worktree-local selections; only SDK-owned CLI state is shared. |
| .NET CLI | Existing absolute `DOTNET_BIN`, shared absolute `DOTNET_CLI_HOME`, forced PATH opt-out at every controlled launch. |
| Godot | Existing verified installation/project/debug instance unless a concrete exception applies. |
| Outputs | Separate logs, receipts and failure evidence for actual new runs; reuse build/import state unless that state is the reason for isolation. |

Keep configuration in the existing ignored worktree environment script. Historical per-slice
launchers are evidence, not current environment setup. No user/machine PATH edits, populated-cache
copies, or environment installation merely because a topic changes. Registered immutable inputs keep
their read-only selection rules. Writable emulator/import/export state stays with its owner.

Use argument lists, bounded timeouts and owned process-tree cleanup. Inspect actual errors as well as
exit status. A zero-exit process or source-only reading does not prove a running adapter observation.
Preserve the first failure and its node/command IDs, logs and process-completion state; after a
correction run only affected engine behavior or directly invalidated verification. No full rerun
solely to replace a completed red aggregate, and no timeout increase as the first fix.

Cleanup is separately authorized work: inspect live references/processes and exact paths, record the
delete/retain list, and preserve private inputs, completed evidence and required reproduction state.
Do not replay earlier cleanup scripts. Keep private/generated inputs, tools, `.godot/`, `bin/`, `obj/`,
exports and receipts out of Git; inspect staged paths and any actually produced export boundary.

## Proportional Gate Guide

| Change | Required kind of evidence |
| --- | --- |
| New-engine rule/state/content behavior | Small meaningful engine unit tests; affected product build; direct reference comparison only for its affected claim |
| Godot input/presentation | Affected adapter build and needed actual-instance observation; no new probe/helper tests or screenshots |
| Verification/probe/planner/report code | Execute and inspect the needed verification directly; add no tests of verification infrastructure |
| Documentation/instructions | Direct document and scope checks, committed planner inspection, honest existing CI outcome |
| Original research or genuinely shared evidence input | Owning research requirements and affected dependencies; keep original evidence/provenance intact |

These scope rules do not waive accepted 8C/H4 evidence. A migration handoff names any pending remote
required-check change and unsupported product capability. Use
[Bounded Inspection and Review](../../docs/operations/bounded-inspection-and-review.md) for exact
identity, semantic review and completed-result handoff.

## Private Initialized Entry Observation

After loading the retained SDK/Godot/worktree environment, select the existing read-only encounter
inputs in `SF2_PRIVATE_BATTLE01_DATA`, `SF2_PRIVATE_BATTLE01_SCENE` and `SF2_PRIVATE_BATTLE01_TERRAIN`.
Select pinned existing static-data and enemy-promotion exports in `SF2_PRIVATE_STATIC_DATA` and
`SF2_PRIVATE_ENEMY_DATA`, plus the existing enemy-gold export in `SF2_PRIVATE_ENEMY_GOLD`; their identities remain owned by the extraction manifests. If missing,
reproduce them with the existing export owners and explicit ignored destinations from the pinned
read-only source. Never change the registered source or upload exports. The
[trust owner](./runtime-profiles-and-trust.md#private-initialized-common-battle) describes the seven-input
contract and controlled policy.

```powershell
$env:SF2_PRIVATE_CONTROLLED_START = (Resolve-Path -LiteralPath 'remake/reference/inputs/battle01-player-ready.json').Path
$env:SF2_REQUIRE_PRIVATE_TESTS = '1'
foreach ($variable in @('SF2_PRIVATE_BATTLE01_DATA', 'SF2_PRIVATE_BATTLE01_SCENE', 'SF2_PRIVATE_BATTLE01_TERRAIN', 'SF2_PRIVATE_STATIC_DATA', 'SF2_PRIVATE_ENEMY_DATA', 'SF2_PRIVATE_ENEMY_GOLD')) {
    if ([string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable($variable))) { throw "Required private input not selected: $variable" }
}
uv run sf2 verify engine
```

This invokes the actual common-engine private facts. With no private inputs and no mandatory flag,
public CI explicitly skips them; any partial selection or mandatory flag makes missing required inputs
fail. A skipped public result never supplies private acceptance. The real comparison checks initialized
HP/MP/effective versus source ATT, class/movers/equipment/spells, unknown accounting, full first queue,
region words, independent RNG and first-player control against the existing H3 PlayerReady fixture.
It then uses common commands for movement/cancel, the next Centaur player and actual inactive enemy
standby back to Bowie. `PrivateSourceAiTests` continues real player commands through region activation
and set7 pursuit through the first actual enemy attack to player control, then atomic rejection of
a player attack with Unknown EXP.
The comparison preserves source anchors/orders, evolving positions/memory, activation/tested words,
resources/loadouts/unknown accounting, last target and both RNG channels. Meaningful authored variations
cover other IDs, movers, occupancy, memory, commandsets and transaction rejection; no live setter or
legacy session drives this private comparison.

Refresh the affected Debug Godot assembly, then run the existing native observer. The restart is for
the changed startup composition and assembly, using the retained editor/project:

```powershell
& $env:DOTNET_BIN build remake/game/Sf2.Remake.Godot.csproj --configuration Debug --no-restore
$outputPath = Join-Path $env:SF2_RUN_OUTPUT 'private-initialized-observation.json'
$env:SF2_OBSERVATION_CASE = 'private-initialized'
$env:SF2_OBSERVATION_REVERSE_KILLS = '0'
$env:SF2_OBSERVATION_LONG_PATH = '0'
$env:SF2_OBSERVATION_OUTPUT = $outputPath
& $godotBinary --headless --path remake/game --script res://probes/engine_battle_observation.gd -- --private-battle-start $env:SF2_PRIVATE_CONTROLLED_START
$outputPath = Join-Path $env:SF2_RUN_OUTPUT 'private-source-ai-observation.json'
$env:SF2_OBSERVATION_CASE = 'private-source-ai'
$env:SF2_OBSERVATION_REVERSE_KILLS = '0'
$env:SF2_OBSERVATION_LONG_PATH = '0'
$env:SF2_OBSERVATION_OUTPUT = $outputPath
& $godotBinary --headless --path remake/game --script res://probes/engine_battle_observation.gd -- --private-battle-start $env:SF2_PRIVATE_CONTROLLED_START
```

Require all seven initialized-entry checkpoints or all thirteen continuous source-AI checkpoints,
`passed:true`, no failures and clean process logs. The source-AI case includes the entry checks, then
real movement/STAY through activation, set7 pursuit and the first actual physical attack, followed by
stable Unknown-EXP rejection on the selected player attack. These observe actual
Godot input, shared session state, actor nodes, private-origin HUD and explicit Unknown accounting;
no images are emitted. Private output stays ignored. This comparison preserves the fixture's
non-natural R2a→R2b bridge, explicit intro skip and candidate-only missing-word policy. It does not
claim natural map programs, original presentation or a completed battle.

For changes to these shared initialization seams, run `PrivateBattleInitializationTests`,
`PrivateBattleScenarioTests` and the affected movement/turn-order engine tests with private inputs
required. For shared standby/pursuit changes, run `SourceEnemyAiTests` and `PrivateSourceAiTests`.
The legacy reference groups that previously compared these calculators were retired at M5; the actual
common private facts and the native continuous-input observation supply acceptance. Do not run new H3
work for this bounded entry. Preserve any completed failure and rerun its owning nodes after
correction. The clean committed planner selects engine/adapter; documentation uses direct
links/anchors/fences/tables/examples and scope/private-boundary checks.
The private action binding and its actual enemy/player/kill/HEAL continuation are implemented at the
bounded scope below. Wider effects, natural map programs and outcome/return retain their stated
Unsupported or Unknown boundaries.


### Private action and spell selection observations

The seven-input private selection additionally binds the existing `enemy-gold-data` export. If absent,
use `uv run sf2 h2 enemy-gold --upstream-path <selected-pinned-source> --output-path <ignored-output>`
with the registered ROM selection; this existing narrow extractor checks source/ROM parity and the
manifest digest. No ROM or generated export is committed or packaged.

After the existing environment setup and Debug adapter build, run:

```powershell
$env:SF2_OBSERVATION_CASE = 'private-actions'
$env:SF2_OBSERVATION_OUTPUT = Join-Path $env:SF2_RUN_OUTPUT 'private-actions.json'
$start = (Resolve-Path -LiteralPath 'remake/reference/inputs/battle01-actions.json').Path
& $env:SF2_CASTLE_REVIEW_EDITOR --headless --path remake/game --script res://probes/engine_battle_observation.gd -- --private-battle-start $start
```

Require passed:true, exit0 and five checkpoints: initial state, actual enemy hit, player hit/next
control, first kill/rewards/death/next control, and HEAL/next control. Exact first player and kill
HP/EXP/gold/main/thinking values match the retained `ManualAttackCommitsReceipt52AndActualDispatchReachesRoundSevenPlayerTwo`
and `FirstEnemyDefeatCommitsOrderedAwardsCleanupAndActualPlayerOneControl` reference comparisons.
The older `private-source-ai` case now observes the first hit and a subsequent Unknown-EXP rejection;
its thirteen checkpoints still use the unchanged PlayerReady input.

`PrivateActionBindingTests` covers actual source/effective stats and equipment, source gold, all three
party attackers, exact hovering land reduction, nonleader death/unknown defeat count, HEAL and
lower learned levels. Its explicitly constructed rule seams are unit inputs, not natural reach claims
or running-session setters. Keep natural accounting/seed producers Unknown. The former grouped
reference comparison in the legacy Domain test project was retired at M5.

For G6, make an ignored copy of `practice-yard.json`, set medic-a start HP60, and append a second
learned spell `restore` level2, cost5, range0–2, ordinary heal power30/fullRecovery:false after `mend`.
Run the ordinary authored path with `SF2_OBSERVATION_CASE=spell-selection`. The four-checkpoint probe
uses H twice, observes the selected second spell and HUD, then commits HP90/MP15 and next control.
`SpellSelectionTests` exercises the same independent costs, target reset, cancellation and rejection
semantics. The public package order remains unchanged; diagnostics stay in the external probe.

## Battle message operand and font observation

`BattleSceneView.Observe()` retains `reactionAmount` directly from the current scene's `Amount`.
This is the raw reaction/message operand, not a damage calculation or the effective HP difference.
An overkill result can therefore retain damage greater than the target's remaining HP. The existing
message, phase, wait token and visible-character fields identify the corresponding mounted Label.
`messageFont` reads that Label's resolved settings/theme font and size, its visibility, resource class,
and ordered font-cache face identities (family, style, face index and system-fallback permission).
The existing battle scene probe retains these fields through its generic scene projection.

The [Font API](https://docs.godotengine.org/en/4.7/classes/class_font.html) and
[TextServer API](https://docs.godotengine.org/en/4.7/classes/class_textserver.html) supply those face
identities; an available file or opaque RID does not identify the mounted consumer. These are the
resolved configured font/fallback caches. **Unknown:** which face, including an automatic system
fallback, shapes each individual character; the public Label API does not expose its shaped buffers.
Modern font/layout remains permitted. Exact shaped glyph runs, original bitmaps and pixels are not
new acceptance gates. Observation performs no input, logical update or RNG operation.

For the bounded direct check, load the current ignored private configuration in the same launching
process. Reuse the registered installations and owning project. A Debug rebuild/restart is needed
to load the changed adapter when no suitable owned instance is running:

```powershell
. ./local/private-inputs.ps1
uv run sf2 verify adapter
# The local launcher uses shared_dotnet_environment for Debug/native child launches.
uv run python -X utf8 local/issue534/battle-message-observation-01/run.py build
uv run python -X utf8 local/issue534/battle-message-observation-01/run.py check
uv run python -X utf8 local/issue534/battle-message-observation-01/run.py lethal <fresh-lethal-run>
uv run python -X utf8 local/issue534/battle-message-observation-01/run.py heal <fresh-heal-run>
```

This local check uses `GameRoot --private-battle-start`, the retained selected private inputs and an
ignored script extending `engine_battle_scene_observation.gd`. It reuses ordinary movement/target/
attack and HEAL input, scene settling, actual node reads and process errors. Only disclosed controlled
ally starts differ: the lethal actor has increased attack/movement and EXP0; HEAL selects the caster
at full HP. Read the raw amount beside the committed HP effect and token. The lethal observation
retains Amount52 against HP5→0; the full-HP HEAL retains recovery0, one MP payment and both message
timeout paths. The mounted consumer reports Open Sans SemiBold / SemiBold, face0, size9, with system
fallback allowed. Consecutive synchronous observation reads preserve session/revision/observation
sequence, actor state, semantic observations and both RNG images. These checks prove the observation
mechanism, not independently calculated original damage or per-character fallback selection.

The direct reader does not provide ally Growth definitions. The completed EXP99 attempt is a
preserved `level-up` Unsupported failure, not an interrupted run or passing growth check. Direct
battle initialization also refreshes HP to maximum; the completed injured-HEAL assertion failure is
preserved, and the corrected window explicitly checks full-HP recovery. No Domain/Content fix or
world-route extension follows from these probe-input limitations. Existing retained typed growth
effects remain the growth evidence dependency; this check supplies no new GrowthMessage witness.

Keep fresh outputs under `local/issue534/battle-message-observation-01`; retain the completed failure
logs, process receipts, actual projections, input/effect stream and read-pair invariants. Do not alter
old captures: new windows cannot backfill six missing lethal operands or the previously unobserved
font of new-A02. The complete binding uses its separately allocated continuous A in the
[comparison route](#continuous-text-material-comparison). No partial PASS, subset, count change, full winning
route, screenshot, emulator, export or broad/helper test is part of this observation patch.

## Continuous text material comparison

The real comparator entry delegates to the [text material module owners](../../docs/operations/bounded-inspection-and-review.md#h4-text-material-module-route).
The explicit reader/storage interfaces preserve source admission, independent field/battle required
inventories, typed reaction operands, ordered reports and publication lifetime. Structural acceptance
uses the complete source correspondence and bounded source-derived/constructed controls under
`local/issue638/text-material-modules-01`, including the real modern caller and detached publication.
Those controls establish extraction equivalence only; they do not represent historical A, natural
reach or a new full comparison. The retained complete A and its failures below are unchanged;
its eager historical recipes are not the structural slice's acceptance commands.

The configured-font check rejects boolean `faceIndex` values while retaining numeric0/0.0 and
the existing missing-value and strict `allowSystemFallback` rules. Separate complete before/after
reports under `local/issue638/text-material-font-type-01` cover both consumers and missing-plus-boolean
contradictions. This corrects an existing false PASS independently of the preserved movement proof;
it does not rerun or revise the accepted historical A comparison.

`remake_h4_comparison` accepts `--text-source-root` as an explicit read-only pinned SF2DISASM
checkout. Relative selections resolve from the repository root, including package working
directories. Existing same-run world/scene/process and scene/asset provenance selections remain
required; no new corpus, manifest, schema or subset is introduced. The existing whole
`displayed text tokens/font/glyph private binding` child receives the result, with occurrence and
source checks retained in `actualObservations.textMaterialBinding`.

The independently reproducible source boundary is the accepted SF2DISASM commit, gamescript,
ally/enemy names, Battle01 spriteset, ASCII map and font bytes checked against the existing
source/ROM-parity fixture. The recorded Battle01 source digest represents the historical CRLF
checkout; derive that exact representation from its pinned Git object, without rewriting evidence.
The [continuous contract](../../docs/design/contracts/map3-battle01-continuous-scenario.md#complete-reached-displayed-text-material-binding)
defines all field span and typed battle action/effect joins. Missing fonts, raw operands, source
selections or unpaired occurrences are Unavailable; content/operand/face contradictions are FAIL.
Mixed missing evidence and an independent contradiction remain FAIL. No per-character shaping,
original bitmap/pixel, W2 consumer or additional motion/audio closure follows from material PASS.

The retained complete A is `local/issue534/h4-text-material-01/new-A-01`: session
`f003bc6c-f18b-4e52-8921-ed8be904589a`, native exit0 and no process/probe errors. It reuses accepted
party/start/world/scene/pins/settings and the existing legal adaptive route. Actual source joins
cover86 field spans/596 projections, including outcome2305..2310, and103 battle tokens/725 nonempty
projections. Field/battle mounted Open Sans SemiBold/SemiBold/face0 use sizes16/9; raw lethal
reaction amounts are present. Input/probe/DLL identities and new release/Nod/fade facts remain in
that session's private output. No input, asset or old capture is modified. A native launch was
necessary because no owned instance existed and old A lacked these consumer observations; the
successful complete A is not repeated. The separately retained adjustable PR600 window owns its
reveal audio mechanism; instant A supplies no incremental reveal-tail interval.

For local reproduction, load current private configuration and use the retained explicit selections:

```powershell
. ./local/private-inputs.ps1
uv run python -X utf8 local/issue534/h4-text-material-01/compare.py <fresh-comparison-name>
uv run python -X utf8 local/issue534/h4-text-material-01/direct.py <fresh-direct-name> <comparison-name>
```

Those ignored recipes call `compare_modern`/`compare_matrix` with accepted reference, normal05,
same-run inputs, pinned source/asset roots and retained B/C/D reports. The maintained CLI equivalent
is `uv run python -m sf2tool.remake_h4_comparison compare --profile modern-continuous`, with
the existing explicit arguments plus `--text-source-root <configured-checkout>` and a fresh ignored
output. Recalculation always uses a new output filename; retained completed failures stay unchanged.
Direct checks exercise missing/drifting font, Label, glyph advance, raw Amount, source selection,
mixed absence/contradiction, the #595 report-integrity boundary and unchanged assertions outside
this child. Source and consumer files remain read-only. Engine/adapter binaries are unchanged, so
their accepted checks are reused rather than repeated.

For the whole-occurrence and mixed-evidence boundary, run the ignored retained correction reader
with a fresh output name:

```powershell
. ./local/private-inputs.ps1
uv run python -X utf8 local/issue534/h4-text-material-01/correction-cases.py <fresh-readback-name>
```

It removes battle28578 from both scene projection channels while retaining its logical start, and
field continuation18004 from every projection while retaining source producer17751 and its sibling
span. Both must be Unavailable. These tokens locate retained examples, never production restrictions
or fixed completeness counts. The inventories derive from reached logical starts and source span
semantics. Cross-section, later-occurrence and same-occurrence missing/contradiction pairs must retain
False in both directions; independent font/phase and available source checks continue after a local
operand absence. `requiredField`/`requiredBattle` retain that independent inventory in the existing
binding result. No new report framework or unvisited requirement is introduced.

The named A's complete displayed-text material child passes, including the independent logical/source
occurrence inventory and mixed missing/contradiction boundaries. Its required comparison has2238
PASS/23 Unavailable; after matrix self-closure, A has22 remaining children. Common-gameplay equivalence
and all four settings variants pass. Retained B/C/D each have37 unavailable children and their own
missing observations, with no backfill. Three other resource families and five broad parents remain
incomplete; full H4 is Unavailable. The existing historical
original trajectory diagnostics, normal05, prior captures, source/clock/JOIN/HEAL/RNG and other
failures/Unknowns remain unchanged.

The compatibility projection excludes only additive EntityWaitRelease from common event operands;
raw typed evidence is retained. Paired baseline comparison omits only newly exposed isScriptIdle
when the corresponding old entity lacks it, retaining identity/order and every other field. If both
sides expose idle, compare its value; expected idle with missing actual idle fails. Existing field
cursor/busy/moving and RNG drift still fail. Old baseline PASS proves no unavailable idle evidence.
The initial six baseline representation failures and later historical-CRLF selector-check failure
are preserved in report-A-01/02 and matrix-01/02; only offline comparison was corrected. Preflight's
missing prior DLL-copy assumption and direct01's completed assertion failure remain recorded.

## Consolidated field consumer boundary observation

The existing input probe retains `fieldLabel` from the mounted field `Dialogue` node: text,
visibility/reveal and resolved font size/class/face identities, using the same configured-font
boundary as the battle message observation above. Imported Units/symbols/advances and matching
Label text alone do not prove that resolved font. Per-character fallback remains Unknown; no
original bitmap, exact shaping or pixel gate is added. Delivery reads must distinguish the callback's
`signal-before-Present` nodes from the subsequent actual presented/drawn nodes.

`ExplorationSessionView` exposes current `entityWait` and each entity's existing `isScriptIdle`.
For release, the dispatcher attaches optional typed `EntityWaitRelease` to the existing service
`SessionObservation`: old token, subject, completion policy, idle/busy/moving, action cursor and
caller/after-motion location. It adds no commit, ordinal, revision, tick or RNG operation. The
selected `cs-5145c` instructions0→1 reinstall `entity-128` immediately after its first wait returns
inside the same Submit; the adapter's post-submit entity already belongs to the next script.
Therefore that state cannot substitute for predicate-time idle. This payload retains the predicate
state before caller execution, without changing the completion test or program order.

`consumerBoundaries` retains current wait/caller/subject/presentation and release payloads before
the probe's prefix trimming, plus relevant existing Nod/fade actual `frame-post-draw` states.
Existing `nod-started`, `nod-presentation-completed`, `nod-returned`, logical Nod progress,
`full-fade-*` and `presentation-completed` remain their original events. No new completion counter
or observation bus is introduced. A request, culled node or callback before Present does not prove
delivered drawing; join the actual mounted state at its stated stage.

`revealAudioPairs` brackets a real incomplete-text Confirm dispatch synchronously: live audio and
logical context before input, then after `Input.flush_buffered_events()`, before any yielded frame.
Require the same voice `startSequence`/cue/slots Playing across full reveal, unchanged logical token/
revision/tick/RNG and result range, and no reveal-induced receipt. Preserve later real Finished or
legitimate replacement with complete uniquely bindable receipt ordering. Playback position may
advance on the audio clock. A natural finish inside the pair or an ambiguous ending remains
unobserved for the still-playing-tail claim; neighboring samples cannot fill this interval.

The short controlled check reuses admitted private world/audio/font/sprites and the two selected
`cs-5145c` motion instructions, followed by controlled Nod/black fades/text481 with the player as
speaker. It changes only ignored program/start/settings selections, not assets or runtime state.
It is a mechanism witness, not natural caller reach, whole-route collection or full-row acceptance:

```powershell
. ./local/private-inputs.ps1
uv run sf2 verify adapter
# Protected shared SDK environment; narrow real wait/caller cases, Debug build and probe check.
uv run python -X utf8 local/issue534/consumer-boundary-observation-01/run.py test
uv run python -X utf8 local/issue534/consumer-boundary-observation-01/run.py build
uv run python -X utf8 local/issue534/consumer-boundary-observation-01/run.py check
uv run python -X utf8 local/issue534/consumer-boundary-observation-01/run.py window <fresh-run>
```

Load changed code only when a suitable owned instance is unavailable or requires that build; reuse
the registered installation/project and worktree-local inputs/output. The real wait tests cover
NotBusy and ScriptIdle completion, immediate caller reinstallation, next-wait legality and unchanged
service/ordinal/RNG boundaries. Direct reads preserve session/revision/tick/entity/observation state.
The observed field font is Open Sans SemiBold / SemiBold, face0, size16, system fallback allowed;
the speech73 voice retains startSequence2 across reveal and then finishes without replacement.
The first native release is idle at action cursor13 while its post-submit subject is already busy
under the next script. Existing Nod delivery and two fades are retained with actual node state.

Keep all completed temporary-script failures and native process/results under the owning ignored
run root; corrected checks do not turn previous failures into PASS. No helper tests, full new A/A-D,
whole winning route, screenshot, emulator/acquisition/export/cleanup or comparator change follows.
Old sessions remain immutable: this does not backfill six old lethal amounts, field/battle fonts or
old reveal intervals. Complete material/motion/audio occurrence bindings need a separate collection
allocation; original W2/control/audio/source gaps and full H4 Unavailable remain explicit.

### Actual motion and generic cue draw/handoff

The same projection mechanism retains a per-view `cameraProjection.drawSequence`, process frame,
session/revision/observation sequence, logical tick/token and the actual cue state used by Draw.
Actor records expose the existing lowered-sprite flag, shiver X offset and mosaic block size.
`presentationCue` reads the live adapter cue token/resource/age, actor binding and real modulation/
white-overlay opacity and visibility; it is separate from the last drawn projection. Nod elapsed
remains in its existing Nod projection, rather than borrowing the generic presentation timer.
These fields observe rendering operands; they add no draw, logical service, RNG operation or command.

The probe retains post-draw entity waits and generic shiver/mosaic/black/white cue states, with
logical subject, projection identity, token/tick agreement and drawn/culled/missing/no-subject status.
Logical subject visibility is retained independently. A missing actor is not assumed culled;
`frame-post-draw` alone does not make an older projection current. One draw per simulation tick,
instruction or release is not required. A rendered actor record represents issued draw operands,
not original VDP/DMA completion or hardware timing.

Immediately before an existing `CompletePresentation` is submitted, the existing result signal
emits `presentation-completion-before-submit` with its token/kind/process frame and unchanged
session/revision/observation sequence. Its observations list is empty. The probe stores this only
as a `completion-before-submit` consumer boundary, so it cannot duplicate gameplay results or
events. The live cue/last draw/caller/readiness then precede the ordinary submit result. Preserve
the actual order even when the last draw predates the completion callback; never delay completion,
force visibility or invent a terminal draw to fill evidence.

For the bounded mechanism check, reuse the existing selected world/assets and source instructions:
two `cs-5145c` Init installs, `cs-513d6` motion remapped to the player, visible and offscreen shiver,
both mosaic directions, and generic black/white fades. The ignored start selects that appended
program and omits only logical display to exercise generic fades; settings remain accepted A.
It is a controlled observation, not natural source reach or continuous-route acceptance.

```powershell
. ./local/private-inputs.ps1
uv run sf2 verify adapter
uv run python -X utf8 local/issue534/field-consumer-observation-01/run.py build
uv run python -X utf8 local/issue534/field-consumer-observation-01/run.py check
uv run python -X utf8 local/issue534/field-consumer-observation-01/run.py native <fresh-run>
uv run python -X utf8 local/issue534/field-consumer-observation-01/readback.py
```

The readback names its immutable `window-01` capture. It checks visible movement, immediate caller
reinstallation, all generic cue handoffs, real shiver offsets/mosaic block classes, fade properties,
culled state and unchanged logical state across repeated reads. Retain initial zero offset/null
block when Draw precedes installation of the next cue in Update. The last draw and handoff have
distinct frames and ages; terminal operand availability is assessed from those facts, not renamed.
The existing probe exits at the window endpoint and offers no in-instance reset; a later required
window must name its startup necessity. Unaffected Nod/full-fade captures remain their own evidence.
No engine/helper tests, normal/full/H3 suite, source acquisition or full successful-route rerun is
required by this instrumentation. This bounded window alone leaves both whole field-consumer
obligations Unavailable. Their separately allocated complete join is described below; full H4
retains the other outstanding families.

## Complete field motion consumer comparison

The maintained entry is composed by the [motion module owners](../../docs/operations/bounded-inspection-and-review.md#h4-field-motion-module-route).
For its structural extraction, compare complete ordered reports on the saved bounded pilot and
explicitly constructed missing/contradictory controls, audit the entire source-region map and
exercise the real modern entry, all five injected dependencies, occurrence isolation and stream
publication. The private direct recipes are `local/issue638/motion-modules-01/observe.py`,
`interfaces.py` and `boundaries.py`; use fresh ignored destinations for repeat observations.
Shape-derived rows carry controlled channel assignments, not invented historical indices. Reuse
saved complete world programs/provenance through the original-path read view, retaining the
original process receipt and resolving its relative world path against its producer root.

This acceptance omits a new full historical A-02 run and permits no additional raw actual/world
reads or selection pass. Source-body equivalence covers unexercised branches; runtime behavior
outside the named controlled cases remains Unknown. The accepted PR603 evidence at
`ed8591713ccf6329307de78ed7fecf43623be35f` and its failures stay unchanged. Use affected lint,
design-contract/research-index checks, direct document/private/scope checks, committed planning
and actual CI. This structural scope does not require helper tests, a native/SDK launch, full H4,
route/matrix, normal/full aggregate or a new environment.

The maintained modern comparator binds exactly the existing two whole field-motion/gesture/fade
children described by the [continuous contract](../../docs/design/contracts/map3-battle01-continuous-scenario.md#complete-reached-field-motion-and-consumer-binding).
`actualObservations.fieldMotionBinding` retains the independently inventoried producers, source
macro/native-call spans, typed locations and occurrence-local source/actual checks. The source
reader verifies the selected pin and tracked source before reusing `OriginalPrograms`; its ordered
producer operands must equal the selected same-run world. Existing material/original-witness
selections remain explicit. The source witnesses corroborate their named seams; no winning-trace
frame alignment or new original run is required.

The current retained continuous observation is `local/issue534/field-motion-join-01/new-A-02`.
It reuses the accepted Debug binaries, frozen party/start/settings/world/scene and legal A route
at fixed60/poll1. Its process completed exit0, internal route checks passed, and its Godot log has
no errors. The completed `new-A-01` route remains a failed observation: explicit-null `entities`
caused70 iteration errors and aborted64 postdraw,3 before-submit and3 pre-Present callbacks.
Those missing continuous-session rows cannot be repaired offline or backfilled from a short window.

The probe now skips iteration for an explicitly-null entity collection while retaining that raw
null, cue/projection identity and no-subject/missing/culling distinctions. Camera actor collections
are non-null arrays when that projection exists. The bounded `null-window-07` uses the selected
Map57 source warp destination and before/load programs; it preserves all70 explicitly-null callback
records without errors or fabricated entities. Its endpoint is loader return after scene mount
and FadeIn handoff, not first battle input. Earlier controlled-start/endpoint failures are preserved
under the same ignored owner. A GDScript-only correction needs no SDK rebuild. A clean complete
observation is reused for every offline correction and is not repeated.

```powershell
. ./local/private-inputs.ps1
uv run python -X utf8 local/issue534/field-motion-join-01/compare.py <fresh-comparison-name>
```

This recipe calls the maintained `compare_modern`/`compare_matrix` interfaces with the explicit
accepted reference, normal05 baseline, source/material/original witness selections and retained
B/C/D reports. The maintained CLI route is the existing modern-continuous compare command with
those selections and a fresh ignored output. Relative selections resolve from the repository root.
Direct counterexamples remove every consumer channel of motion, Nod, shiver, mosaic, full fade and
loader fade while retaining logical producers. They also exercise wrong source/subject/policy/
destination/phase/restore/period/endpoint/caller/order and missing-plus-false within one or across
independent occurrences. False must dominate missing in both directions. These are direct verifier
checks, not a helper unit suite. Exact common gameplay, unaffected assertion values/applicability,
closed inventory/#595 integrity and each variant's own evidence remain required.

The phase checks include partial consumer loss while logical producers and phase contexts survive:
missing Nod lowered use, shiver alternating use or a mosaic block cannot be replaced by a surviving
normal/pre-Update draw. Logical visibility and viewport geometry preserve hidden/culled applicability.
Direct corrections also check held policy, premature field-input readiness, entry-saved period and
palette continuity, shiver saved-state continuity, and independent contradictions alongside missing
phase/operand evidence. The original PR603 candidate and root's completed readbacks remain discovery
evidence; corrected reports use fresh destinations and the same immutable clean `new-A-02` capture.

The candidate comparison closes these two children only. The retained text-material A's22 and
B/C/D's37 remaining-child results stay attached to their own immutable observations. Independent
review owns acceptance of this bounded candidate; its retained five broad families/full-report
result remains Unavailable. Current milestone acceptance is recorded in the
[composition review](#accepted-composition-review). Run affected Python lint, direct document/private/diff checks, committed dependency
planning and actual CI. Reuse accepted unchanged binaries; no normal/full/H3/engine/adapter rerun
is required solely for this offline comparison and GDScript correction.

## HEAL scene verification

Use the current locked environment/private-input configuration before controlled .NET or Godot
launches, including the shared CLI-home and `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false` policy above.
The bounded HEAL path and its completed original-comparison failure are owned by
[HEAL spell scenes](./presentation-and-assets.md#heal-spell-scenes). No new original launch,
screenshot, full route, aggregate rerun or asset promotion is needed for this boundary.

The affected engine checks are `HealingRulesTests`, `HealingFairyTests`, `BattleSceneTests`, the
three existing completed-action cases `NaturalHistoriesHealThroughTheSameContentAndSessionPath`,
`SharedDefinitionSupportsIndependentStartsAndNaturalActionHistories`,
`SecondLearnedSpellUsesItsOwnCostAndPowerWithoutReorderingContent`, and `PrivateActionBindingTests`.
`HealingNeutralTimeoutNeedsDeliveryButNoAdditionalAcknowledgement` covers both action/recovery
messages and both readiness orders: exhaust the source65 neutral polls, then complete without any
extra Ack or RNG/update opportunity. The neutral completed-scene helper uses presentation delivery
instead of hiding a missing timeout behind an Ack. Early input-first acknowledgement remains covered
by `TimedInputTestsAcknowledgementBeforeItsNextFairyOpportunity`.
Construction expectations still assert the original two award draws; final assertions follow the
real scene and independently check each main-seed transition. Growth starts from the seed carried
through fairy retirement. `PlayerWait*` in `ExplorationSessionTests` preserves field/plain-text guard
coverage. Existing `PrivateBattleOutcomeProgramTests.Settle` reuses the real scene drain, but its
long whole-route tests were not rerun for this slice. Compile acceptance is not a route PASS.

```powershell
& $env:DOTNET_BIN test remake/tests/Sf2.Remake.Engine.Tests/Sf2.Remake.Engine.Tests.csproj --no-restore --configuration Release --filter 'FullyQualifiedName~HealingRulesTests|FullyQualifiedName~HealingFairyTests|FullyQualifiedName~BattleSceneTests|FullyQualifiedName~PrivateActionBindingTests|FullyQualifiedName~NaturalHistoriesHealThroughTheSameContentAndSessionPath|FullyQualifiedName~SharedDefinitionSupportsIndependentStartsAndNaturalActionHistories|FullyQualifiedName~SecondLearnedSpellUsesItsOwnCostAndPowerWithoutReorderingContent|FullyQualifiedName~PlayerWait'
uv run sf2 verify adapter
```

Private binding checks require the existing selected private battle inputs together with
`SF2_PRIVATE_BATTLE_SCENE_CONTENT` pointing at the newly prepared HEAL sidecar. An older physical/
Herb sidecar remains valid for those actions, but cannot silently supply authored HEAL graphics.
The maintained `uv run python -m sf2tool.remake_battle_scene_content --rom <selected-rom> --upstream <pinned-checkout> --output <fresh-ignored-directory>` command creates the candidate; retain source/generator provenance and leave accepted assets untouched.

The existing `engine_battle_scene_observation.gd` accepts `SF2_BATTLE_SCENE_HEAL=1` with world/wounded
observation enabled. The retained local launcher `local/issue523/heal-scene/run-heal.ps1` selects
source world/audio/scene inputs and a disclosed controlled party: Chester HP/maxHP40 DEF20; Sarah
MP/maxMP32 and learned HEAL3. It uses actual field/battle input and enemy injury; no runtime state
setter or original-playthrough claim. Run fresh normal and `-ReducedFlash` outputs, then compare
ordered semantic events (kind, actor/target, before/after, random range/value, detail) and each final
main/thinking seed, resources and turn cursor. Host timestamps/revisions are delivery metadata.
A source logical-work or host-readiness change invalidates this pair; documentation/test-only changes
do not. The first HEAL additionally records `healTimeoutCases` for both action and recovery text:
natural reveal/readiness, idle callbacks with no logical progress,65 actual V inputs, no Confirm,
and completion on the last neutral input. The final-input receipt proves no manufactured delivery/
Ack; subsequent scene work remains a separate operation. Later HEAL cases retain early acknowledgements.

Retained read-only reference comparisons after building the production assemblies:

```powershell
& $env:DOTNET_BIN run --project local/issue523/heal-scene/comparison/Compare.csproj -- $selectedPrepared04Checkpoints
& $env:DOTNET_BIN run --project local/issue523/heal-scene/cursor-comparison/Compare.csproj -- $env:SF2_PRIVATE_BATTLE_SCENE_CONTENT
```

These ignored inspection aids use the exact production kernel/cursor; the tracked source and named
original records own the facts. The first supplies the trace's update/control schedule and passes244
returns. The second supplies only the post-award entry seed and neutral input, and retains a completed
**FAIL** at recovery CP2059–2091. Its final coincident seed is not a passed continuous comparison.
Do not replace that failure with native settings equality or call it unavailable/interrupted.

Completed correction records remain under `local/issue523/heal-scene/`: the initial xUnit analyzer
failure, one invalid test target placement, incomplete private-environment selection, and the idle
helper test's obsolete sleep-only drain. Each received a narrow correction/rerun. PR557's independent
review also found that neutral timeout incorrectly required an extra Ack; that completed review
failure remains retained even after the focused engine/host correction. The earlier
completed slow-suite failure record remains unchanged; this work neither reruns nor relabels it.


## Battlefield death consumer

The field-death scope is status-free, ATT-only post-action processing. `BattleSceneTests` checks the
persistent HP/position/stat boundaries, twelve/three whole-batch stages, token rejection, empty
paths, capped ordered cleanup and victory/defeat release; `PhysicalBattleTests` also checks lethal
counter continuation. Use the locked environment above and the narrow filter
`FullyQualifiedName~BattleSceneTests|FullyQualifiedName~PhysicalBattleTests`, then the affected
engine and adapter entries. Do not run the retired aggregate or full H4 merely for this consumer.

A selected scene without `fieldDeath` still admits its existing close-up resources, but private
field-death delivery rejects missing content explicitly. Prepare only the bounded addition from a
previous selected scene with its adjacent `source/battle-scenes/selection.json` provenance:

```powershell
uv run python -m sf2tool.remake_battle_scene_content --rom $selectedRom --upstream $pinnedSource `
  --field-death-base $selectedScene --output local/issue523/field-death/candidate-new
```

The output must be a fresh ignored worktree-local directory. The producer checks retail ROM and
pinned source identity and previous scene provenance; it reuses the established Basic decoder,
palette and map-sprite raster composer. It writes a selected scene document with three63 sheets,
source ally/Gizmo selectors and a bounded provenance report. It does not copy/alter the selected
world/audio package, promote a material library or raise the scene reader's four-MiB limit.
Select its `battle-scenes.json` through `SF2_PRIVATE_BATTLE_SCENE_CONTENT`; keep
`SF2_PRIVATE_EXPLORATION_CONTENT` on the existing world with116/BD.

The actual `res://probes/engine_battle_scene_observation.gd` has a bounded
`SF2_BATTLE_SCENE_FIELD_DEATH=1` observation mode. Retain the existing ordinary world start and
`SF2_BATTLE_SCENE_WORLD=1`; the ordinary reward case (`SF2_BATTLE_SCENE_REWARD=1`) exercises enemy
death. `SF2_FIELD_DEATH_COUNTER=1` selects a live adjacent enemy from observed terrain and submits
ordinary approach/target/attack inputs. A declared controlled Chester HP/maxHP1, ATT3, DEF0, AGI99,
MOVE63 and initial mainSeed1048576 reaches an actual enemy counter against him; these are input
conditions, not runtime overrides or a natural original-route claim. The retained missing81/C6
failure is resolved by the accepted modern finite-PCM selection policy: actual receipts show81/CC
with requestedC6, followed by116/BD. PCM identities and the selected world/audio pack are unchanged.

Observe actual nodes/resources/facing/cells and hidden close-up, input/turn blocking, the single
116/BD AudioStreamPlayer start and finite completion, cleanup and subsequent usable input/outcome.
Normal60FPS and reduced30FPS enemy-death and counter-death observations must agree on complete ordered semantic events
(excluding delivery revision/sequence) and gameplay state. No screenshots, injected death lists,
mid-session seed changes, extra logical ticks or original-emulator launches are part of this gate.
Natural multi-death and after-turn death have no admitted producer and cannot be claimed from the
multi-member domain test. The [timing and asset boundary](./presentation-and-assets.md#battlefield-death-batches)
retains the unresolved dependencies and prior HEAL/H4 failures.

With `SF2_AUDIO_SELECTION=1` and field-death mode disabled, the same actual adapter probe checks
exact65/BD preference among variants, unique81/CC reuse in music2/C6, consistent named-resource
playback, missing80 and ambiguous65/C6 rejection, actual Started/Finished receipts and unchanged
gameplay state. This is a direct consumer observation, not a unit test of the probe. The normal and
reduced counter pair also confirms exact83/C6 and116/BD selections.

## Battlefield movement consumer

`BattleMovementTests` exercises per-segment completion, wrong/duplicate tokens, busy commands,
provisional placement, friendly traversal and decreasing-cost return distinct from input reversal.
`EnemyActionTests` checks one thinking decision, unchanged main RNG/resources during delivery and
construction only after arrival. Source/commandset tests retain target, memory, legal-stop and
Unsupported boundaries. Callers that need completed movement explicitly use `FinishMovement`;
`Accept` and `Start` never drain it implicitly. Use the affected engine files and adapter compilation
under the locked environment. Preserve completed failing runs and rerun only failed files after
call-site migration; touching an outcome helper does not require its long complete-route observation.

For actual host acceptance, reuse the selected world containing79/BD and the existing field-selector
scene, with `SF2_PRIVATE_EXPLORATION_CONTENT` and `SF2_PRIVATE_BATTLE_SCENE_CONTENT`. The
`engine_battle_scene_observation.gd` mode `SF2_BATTLE_MOVEMENT=1`, together with
`SF2_BATTLE_SCENE_WORLD=1`, drives ordinary movement, bend, Cancel, blocked input and live-board
approach/Stay inputs through AI movement and attack. Other scene-specific modes must be disabled.
Use the existing ordinary world start and a declared external controlled party: HP/maxHP100,
DEF40, ATT5 and EXP0 for allies, Chester AGI99/MOV6, initial mainSeed2568421376. These initial inputs
make a bounded survivable observation; they do not establish a natural original route.

The observer records actual intermediate node position/facing/walking frames, stable gameplay
within each token, busy-input rejection, segment start/arrival and actual79/BD AudioStreamPlayer
receipts. Check one cue per segment revision, no cue on blocked/origin operations, retained AI
multistep Stay and move-then-attack, and finite PCM completion/replacement. Run normal60FPS and
reduced30FPS from the same declared inputs; compare complete ordered semantic events excluding
delivery revision/sequence, and final gameplay state including positions, resources, memory, turn
and both seeds. Probes and launch wrappers are observations, not engine behavior to unit-test.
No screenshots, injected paths, runtime seed changes, original runtime, new assets or resource
copies are required. The [source audit and limits](./presentation-and-assets.md#battlefield-movement-and-walking-audio)
remain separate from native-consumer claims.

## Source-bound choice observation

The [execution owner](./exploration-programs.md#source-bound-yesno-lifecycle) admits fresh conditioned
semantic input with separately captured mapped held release. Test actual engine behavior in
`ExplorationTextWaitTests` and retain the companion legacy choice behaviors in
`ExplorationSessionTests`. Affected classes are `ExplorationTextWaitTests`, `ExplorationSessionTests`,
`MapEntityLifecycleTests` and `CommandsetContinuationTests`; use their existing dotnet filter with
the locked SDK/private environment. Run the affected behavior classes, `uv run sf2 verify adapter`
and refresh Debug before actual adapter observations. Required CI covers the committed engine tree;
a review correction does not require repeating a completed full local engine suite. No tests of probes/reports and no blanket
normal/full/H3 rerun are required by this bounded source binding. The prior completed normal-verify
provenance failure after148passes is retained under the existing applicability decision.

The maintained input observer selects
`portrait-event-zone-nod-camera-choice-yes-private-instant` (or `choice-no`, `natural`, `reveal`).
Set `SF2_INPUT_RATE` to20 for natural or40 for reveal, and retain the same world, controlled party,
spatial fixture and unchanged bound opening start used by the camera observation above. Select
fresh ignored output/settings destinations. `gamepad` and `remapped` suffixes exercise actual
installed input bindings, including swapped Confirm/Cancel. These cases must not generate candidate
content, inject the old text531 endpoint, use historical frame/C quotas or capture the original.

Maintain continuous evidence for both answers across all three text settings. Recheck the accepted
prefix and actual camera/wall projection with explicit field applicability when a source correction
changes the model. Start with one continuous representative run, measure service/RNG/continuation and
projection differences, then rerun affected settings/answers only when the observed dependency needs it.
New refresh counters must be explained, not removed to claim full state equality. At531 hold Ack through opening; verify release across mapped
Confirm plus a mapped stick, then hidden/focus rearm, one neutral Wait and Right→Left→Right→Left.
Confirm returnsYes; Cancel returnsNo. Record actual menu Control geometry/visibility/selection/color
at matching logical tick/token, full source65/66 requests/playback, no additional67, deletion,
flag write, ten services and instruction return. Keep actual speech differences under unanswered#517.
A focus-loss recovery in the observer records the loss and restoration and adds no semantic input;
unavailable OS focus restoration is an observation failure, never a passed gate.

Record first following genuine text input (Yes535/W1 or No532/W2) with choice deleted, correct flag,
full return delay and active zone caller/F603 clear. Continue supported content. Yes stops at536/W1
before Wait/Ack/F600/F66/JOIN. No completes decline and stops at the first ordinary field-control
return, F89 clear/F603 set/caller cleared, with no extra input or join/party writes. Probe endpoints
never gate production engine legality. These observations do not establish original naturalNo,
fullYes Messenger/H4 or JOIN audio/input/live-service coupling.

**Confirmed (bounded remake):** the six observations at initial choice head `2b7a7542` cover both
answers continuously under instant/natural20/reveal40 from the unchanged bound start. Their retained
5096-state prefixes match the prior camera build; within each answer, all25 selected fields match
across5325 Yes or5386 No settled states. The shared-window correction below changes refresh state
and counters, so these records retain their named initial-build applicability rather than becoming
raw state records for the corrected build.
Choice enters at5097, opens through5102, performs the declared five polls through5107, deletes at5112
and returns at5122 after ten post services. These are measured boundaries, not engine quotas.
Yes stops at536/W1 tick5324/main55350000/copyEF; No returns first field control at5385/main61880000/copy14.
Actual menu phases/geometry/labels/variants and non-speech audio agree; natural-only speech remains
under#517. Six actual camera/alpha checks retain362 opaque teacher pixels covered and214 transparent
pixels unpainted. FullYes Messenger/JOIN and original naturalNo remain unproven.

The shared-window review reproduced four failing actual engine cases:
`ChoiceWindowWaitIncludesPostScrollFixAfterItsFourthMovementPass` (open/close × portrait absent/present).
The corrected cases pass with the sixth-service boundary. Companion behavior cases cover geometric
settlement versus unfinished stationary counters, genuinely displaced windows, pending fix across
empty passes/map initialization/repeated scrolling, later registered RNG, and global text/portrait
close waits. Run these in `ExplorationTextWaitTests`; its completed affected-class run retained one
new fixture failure (missing event context at the next instruction), corrected by the owning two-case
method. No completed failure was relabeled interrupted or erased.

One corrected continuous Yes/instant observation, `local/issue534/choice-lifecycle/window-fix-review/
native-yes-instant`, completed at the same5325 states. `representative-equivalence.json` compares all71
prior state fields and the new pending readback; `full-field-differences.json` and
`leaf-differences.json` enumerate changes. Dialogue gains explicit geometry without changes to its
prior fields. Pending is true on1189–1193: portrait creation at1188 occurs after that tick's window
stage, so a final-state hidden/window scan alone overcounts one serviced pass. At1194 the portrait
resets to stationary length1/counter0;1195 has counter1/busytrue;1196 clears busy. Counter1 persists
until close at1397, which reinstalls the normal geometry. Its Y, blink/mouth and all service/RNG/
input/choice/continuation fields remain equal. Actual portrait projection is equal; camera/nod
projection differs only in per-process texture identities. The other delivery differences are
session identity, PCM playback clocks and completion/preemption receipts. All50 ordered sound-start
requests/revisions/PCM facts match. Actual5302 camera/72 menu draws and teacher362 opaque/214
transparent pixels pass the existing geometric/alpha checks with explicitly expected model changes.

The retained No route has the same sole1194 fix and none in its later continuation; previous
within-answer setting equivalence remains independently checked. Therefore the initial six records
continue to support both answer returns, all three settings, mapped controls and effective projection;
only the corrected representative supplies new refresh-state readbacks. Neither answer's later
choice has a fix transition. No gameplay/RNG/service-timing difference was observed to invalidate
and repeat the remaining five native observations. This is bounded reuse, not six corrected-build
runs or original-runtime evidence. The corrected source predicate is verified under changed legal
states by the engine cases above.

Preserved completed observation failures: an initial Yes run exhausted its observer guard at text524,
tick3828, focusedfalse/debt0 and no application error. A later No/natural run passed its logical
probe but lacked menu draw callbacks, so offline projection acceptance failed. Its correction
retains all5386 states/25fields and50 non-speech receipts, and passes complete menu/camera/alpha checks.
The observer now rejects missing phase draws itself. Other passing runs and the completed failing
aggregate were retained; only the affected native observation and comparisons were repeated.

Local results and exact CI are retained in the Issue handoff; original-runtime delta0. Preserve
completed native/gate failures, eight warp differences, H4/HEAL/JOIN/next-actor and cleanupUnknowns.
#517 remains unanswered, #523 stopped and investigator user-exclusive.

## Raw field text observation

This retained expected-Unsupported gate uses content without `modernEndStep`. For the admitted
modern profile and complete JOIN return, use the [modern finite-music observation](#modern-finite-music-observation).


For the [raw display capability](./exploration-programs.md#raw-field-display), run affected
`ExplorationTextWaitTests` and `ExplorationSessionTests`, `uv run sf2 verify adapter`, and Debug
before the direct native observation. Engine cases vary text/name/font width/message speed,
speaker presence, W1/W2, early/late reveal and batching. They check live entity/portrait services,
global window refresh, no implicit Ack/copy, invalid speaker rejection, and completed state retained
at Unsupported. No tests of the observer or blanket normal/full/H3 rerun are required.

Use the existing observer with `SF2_INPUT_CASE=portrait-event-zone-nod-camera-choice-yes-private-instant-raw-text`
and a companion `portrait-event-zone-nod-camera-choice-yes-private-reveal-raw-text` at
`SF2_INPUT_RATE=20`. Reuse the same unchanged bound opening start, world/resources, controlled party
and spatial fixture as the choice observation. Only fresh ignored settings/output destinations are
created. The companion uses reveal input before raw text, then lets raw text reveal naturally to
observe logical End waiting for actual delivery. Startup may retain EntityWait for another frame;
the observer waits for actual field control without navigation input or state replacement.

Both routes continue536's real W1 through close/delay, flags, view wait, actual music start and
membership to raw447. Record the accepted prefix, every common service, live entity/RNG/copy,
window/portrait/caller/party state, and actual Label visibility/text/reveal/geometry after draw.
Require silent raw display, no raw Ack/Wait, and no logical work during late reveal. Compare semantic
states across settings separately from reveal flags, speech policy and audio playback clocks.
The endpoint must be `field-music-progress-unbound`, `UnsupportedCapability`, visible Unsupported
at `cs-51614[22]`, with raw text retained and603 clear. Extra host input and wall time cannot advance
it. Only this named failure is expected; all other errors still fail the observation.

`rawTextBoundary.expectedUnsupported` names that result and `fullJoinComplete=false` explicitly
limits it. `passed=true` means this expected-boundary observation passed, never full JOIN/H4.
The actual AudioStreamPlayer start/PCM facts establish playback consumption only; later completion
does not establish logical music progress. Do not add a checkpoint, source capture, screenshot,
driver/service quota, host callback continuation or extra music/input policy to pass this gate.

**Confirmed (bounded remake):** instant and late-reveal native observations retain the accepted
5325-state prefix and agree across5475 logical states in the selected engine fields, excluding the
delivery receipt flag. Raw text starts at5352 and finishes at5474; these are observed results, not
quotas. Both draw the fully revealed Label before Unsupported. The late-reveal run holds the same
End tick/entity/RNG/window state until delivery. Camera/menu geometry checks use actual draw
callbacks; a SessionResult readback may still name an older draw. Compare those projections at
their recorded draw tick, rather than assuming callback and rendering are synchronous.

Local run identities, comparison results, preserved failures and exact CI belong in the Issue
handoff. The prior263-case affected run had one new out-of-range font-width fixture failure;
after changing32 to the admitted16, its owning three-case theory passes. Preserve that completed
result. Native development also retained a GDScript parse failure and an initial startup-readiness
failure; corrected observations do not erase them. All prior failure/Unknown applicability remains.


## Modern Finite-Music Observation

The [accepted clock](../../docs/design/contracts/music-wait-service.md#accepted-modern-finite-music-policy)
changes the affected historical trajectory comparisons under
[ADR0010](../../docs/decisions/0010-map3-battle01-product-acceptance.md#accepted-modern-finite-music-clock).
Preserve original expected values and completed failures. The existing H4 projector/comparator has
not been cut over to a newly reviewed modern continuous winning trace; do not report that gate as
passed or run the old aggregate merely to replace its red result.

After loading the current private configuration and locked SDK environment, run the actual behavior
classes `ExplorationMusicTests|ExplorationTextWaitTests|ExplorationSessionTests` using the existing
`dotnet test` filter, plus `uv run sf2 verify adapter`. The music cases exercise endpoint phases,
three-service grouping, preceding varied text, enabled/suppressed entities and portrait RNG,
batching, early/late/stale completion, duplicate/replacement generations and plain input/close/return.
Changed valid programs also cover ordinary finite and looping replacement → previous → duplicate →
profiled request/helper, plus map/area round trips that invalidate old history and require explicit
requests to establish a new known stack. These execute ordinary program/presentation commands.
No tests of the reader, observer or comparison scripts are required. Run direct design-contract and
research-index document checks and inspect the committed dependency plan under this engine scope.

For native acceptance, reuse the owned Godot project/installation and retained bound opening,
party, world and Yes spatial route. Use a fresh ignored output. Add only `modernEndStep:505` to
command19's finite audio row through the metadata preparation; directly compare the entire world
with that one field removed, including every unchanged PCM/content field. No extraction or source
capture is needed. Start the existing `engine_input_accessibility_observation.gd` with
`SF2_INPUT_CASE=portrait-event-zone-nod-camera-choice-yes-private-instant-modern-music`, then use
`adjustable-modern-music` and `adjustable-modern-music-reveal` for natural/reveal delivery. Set the
same semantic Wait/choice stream and existing startup input arguments. Each probe exits; the next
startup is necessary to compare settings from the unchanged opening, not a new environment.

The observer records actual generation/cue, helper state, playback start/Finished/previous restart,
raw Label draws, plain Wait/Ack and first field return in `musicLogicalEnd`, `musicPlainInput` and
`joinReturn`. Require no errors; F600/F66/F603, joined[0,1,2]/active[0,1]/reserve[2], source followers
and positions, closed windows, no cursor/wait/caller and ordinary input. Stop before additional field
input. The bounded late-completion hold checks zero service/RNG/debt after logical end. Compare
accepted prefix and new semantic per-service states across settings, excluding delivery receipt flags
and host clocks/resource identities. Actual playback must start once, finish, then restart the prior
cue with the same logical identity; counters alone are insufficient. Compare projections at their
own draw tick when a result callback precedes redraw. Screenshots and live state/RNG injection are
prohibited. Measured counts/seeds are results, never production quotas. This is bounded JOIN
acceptance; full #437/H4, original clock mapping and unrelated recorded failures remain open.

## Bound map-initialization observation

Use the existing locked SDK and private environment. Actual engine cases live in
`ExplorationTextWaitTests` (nested init camera/portrait/text, readiness, source255 follow/scroll,
D1 neutral/skip/placement/restoration/batching with portrait RNG, and sourceFF narration)
and `CastleTowerProgramTests` (live first/repeat init selection). Run changed cases and affected
field readiness directly, `uv run sf2 verify adapter`, and the committed engine planner selection.
Preserve completed red results and rerun corrected nodes; remote engine CI supplies its full gate.

Reuse the installed ordinary Godot host, complete retained modern world, controlled bound opening
and party. Its original small start binds only Map3. For this route, use the existing
`selected_map_palette_bindings(verified_compiler, [19,20])` with clean pinned source to add only the
two required map pairs to a fresh ignored start. Retain Map3 and directly compare all other start
fields unchanged. Do not regenerate the world/assets, infer the pair from another map, or normalize
source255. The accepted producer reads two words per map and records selection paths.

Extend the existing opening probe by selecting
`SF2_INPUT_CASE=portrait-event-zone-nod-camera-choice-yes-private-instant-modern-music-map-init`;
compare with `adjustable-modern-music-map-init-reveal`, keeping the same semantic Wait/choice/Ack
stream. Existing input settings and startup arguments apply. The probe uses the existing castle
navigation and bound-wait consumer, stops at first palace completion followed by Map19 field
return, and records actual state/input/audio/draw projections/errors. No screenshot or session,
flag, position or RNG injection is permitted.

Require exit0/passed:true/no errors, source gate guards out/back and F604, exact royal entrance,
palace entity changes and F605, source255 retained and equal A/B origins through scroll, and
projection agreement at matching draw ticks. Narration text2192 must retain null speaker/flags255
and request no speech even if a portrait is still present. Final Map19 must have closed windows, no caller,
cursor or wait, and usable ordinary input. Compare accepted JOIN prefix and matched semantic
states across settings directly, excluding delivery/host identity fields; measure seeds rather
than targeting old history. Each next startup is necessary to compare from unchanged opening
because the probe exits. Preserve earlier missing-palette and unsupported-view failures. Original
natural timing, later bound scene/before-battle capability, continuous winning trace and executable
H4 policy cutover remain open.

## Bound field parallax observation

Use the retained private world/party/music/environment and a fresh start with only the existing
palette producer's [21,40] bindings added to the accepted map-init start. Compare all other input
fields directly; no world export or environment copy is required. Build the owned adapter Debug
for the ordinary host. Use the existing accessibility observer with
`SF2_INPUT_CASE=portrait-event-zone-nod-camera-choice-yes-private-instant-modern-music-field-parallax`
and `adjustable-modern-music-field-parallax-reveal` at rate20. Reuse opening/JOIN and first palace as
same-session setup, then continue the existing castle graph after its royal-return segment.
Observe Astral refusal/re-prompt/acceptance, guard sprite completion before F401/F256, repeat and
Map21 field readiness. Consume45 of46 player-ready extension inputs, stopping at Map40(14,13)
before the final Up into marked Y12 battle exit. No checkpoint injection or target seed/tick applies.

Compare logical states and distinct same-tick transitions across settings. Preserve the accepted
pre-JOIN completed raw-text End delivery difference separately: it introduces no tick/RNG/entity
service or eligible input; all post-JOIN gameplay transitions must match. Read actual independent
A/B draws and changing separation, main-plane actor origins/resources/geometry, clipping and signed
window priority; verify stable input and carried party/RNG. Engine cases belong to
`CastleTowerProgramTests`; adapter compile and direct source/contract checks own the other boundaries.
Scene-map, camera-entity and bound before-battle windows remain outside this acceptance. Record the
committed default planner and any engine-scope refusal; the shared contract alone does not authorize
local normal/full/H3. Preserve completed failure records and obtain independent main-gate review.

## Bound before-battle windows observation

Load current private/tool selections and the locked SDK environment. Reuse the accepted private
party/assets/music, owned installation/project and existing observer/navigation. Recompile only
bbcs_01 using OriginalPrograms from the pinned source into a fresh ignored world. Its graph is
only bbcs_01, adding explicit null detach before the black fade and `wait-ticks:1` after scene-map;
removing those two instructions
reproduces the old row. All other world/provenance/asset/audio fields must compare directly equal.
No handwritten PC edit, full world/asset export or post-load patch is part of preparation. Produce
only Map57 palette metadata with `selected_map_palette_bindings` into a fresh ignored start;
removing that one binding must leave the accepted field-parallax start directly equal. Verify
existing Map57 atlas, sprite209/portrait31 and texts2292/2293; no general export is needed.

Run affected `BattleEntryProgramTests` and `ExplorationTextWaitTests`, `uv run sf2 verify adapter`,
a controlled Debug host build and `uv run sf2 design-contracts test`. One representative ordinary case
is `portrait-event-zone-nod-camera-choice-yes-private-instant-modern-music-before-battle`.
The retained current wrapper is `local/issue534/before-battle-windows/run-native-post-load.py native-new instant 1 40 yes modern-music-before-battle`.
Earlier opening/palace/tower/parallax steps are setup for this same session. Continue the final
player-ready fixture Up; inspect actual scene/palette/entity readiness/main camera/portrait/text,
real BeforeBattleFinished/EnteringBattle/callers, ordinary W2 confirm and next W1 readiness.
Stop observation at that genuine input wait. No forced PC, injected seed/party/capture or screenshots.
That observation ends before target tracking; later bounded tracking/white observations have separate owners. Record committed default planner and actual CI;
shared contract prose alone does not require local normal/full/H3. Preserve completed failures.

**Confirmed (remake observation of prior detach ordering):** the retained `native-03` representative instant run exits0/PASS
with empty failure/Unavailable lists and1,552 samples. Actual Main input crosses2292 W2 confirmation
to2293 W1 readiness in the original opening session; Map57 tiles/actors, explicit main camera,
restored base/current palette, real135 sprite/portrait and BeforeBattleFinished/route/callers are
read from the running adapter. The observer stops at ordinary input without reaching non-null camera-entity. That run predates
the explicit source detach lowering and does not accept its enabled-service ordering.
The preceding failed/default-fade and incorrect-case runs, cursor-replay behavior failure, fixture
construction and readback inspector failures remain in the ignored owning handoff. Older acceptance
and original/H4/HEAL/provenance/warp/cleanup Unknowns are retained; original runtime delta0.
**Confirmed (detach boundary, prior post-load ordering, remake observation):** `native-04` uses the narrowly
recompiled explicit-detach world and then-current adapter, exits0/PASS with empty failure/Unavailable
lists and1,552 samples. It reaches the same ordinary2292 W2→2293 W1 boundary with actual scene,
text/portrait/camera and route/caller readback. Detach ordering is accepted at the finite helper's
first-service boundary; source CPU cycles, hardware cadence and full battle entry remain Unknown.
The initial CI848PASS/1FAIL/30SKIP is retained: the obsolete missing-FullBlack rejection now has an
explicit zero-period fixture. Its four theory cases pass; moving/counter detach and standalone fade
cases pass after correcting a party-instance assertion to compare actual party values. No local
aggregate rerun replaces that completed red result.

For a retained-player renderer correction, run a minimal authored startup package through actual
Main with a valid explicit target origin and no target area containing the retained player. Observe
both layer0 and layer255 profiles, independent A/B draw origins, actual tile/actor passes and the
unchanged player position; never constrain/recenter the player to make drawing legal. The retained
`run-authored-scene.py authored-new 0` (or `255`) uses the owned project/installation, authored raster
bindings and `observe-authored-scene.gd`; its inputs and readback remain ignored. The source post-load
wait also needs the current ordinary route because it services old entities before replacement,
potentially advancing RNG. Compare the accepted approach and report new measured ticks/seeds as
results, preserving prior-order observations. No full engine aggregate or paired settings campaign
is needed for these corrections.

**Confirmed (current remake observation):** `native-05` uses the fresh source lowering with explicit
detach and post-load wait, and the corrected adapter. It exits0/PASS with empty failures/Unavailable,
reaches ordinary2292 W2 confirmation then2293 W1 readiness, and preserves the accepted Map40 approach.
Readback observes TickWait at13096 with three old entities, then EntitySetSpriteWait at13097 with
eleven replacement entities. Subsequent input checkpoints advance by one tick relative to native04;
this representative's measured main/copy seeds remain unchanged. These are results, not targets.
The two authored layer profiles also draw actual A/B tiles and actors while retaining a player
outside every new area. Enabled/disabled random old-entity behavior separately verifies the general
post-load RNG rule. Original cadence/pixels and later full battle admission remain open.


## Bound physical camera tracking observation

Use the accepted black-scene inputs and post-load source wait unchanged. No source/world/asset export
or new start is required. Run affected BattleEntryProgramTests.BoundCameraTracking cases and the
updated BoundSceneFadeLoadFade case through the selected absolute SDK/shared CLI environment;
`uv run sf2 verify adapter`, controlled Debug build and `uv run sf2 design-contracts test` own the
local gates. Keep completed failures and rerun only invalidated nodes. Record clean committed
`uv run sf2 verify plan --base origin/main --head HEAD` and actual exact-head CI; no local full
engine/normal/full/H3 campaign follows solely from this bounded consumer.

Reuse the owned installation/project for ONE ordinary same-session observation, case
`portrait-event-zone-nod-camera-choice-yes-private-instant-modern-music-before-battle-tracking`.
The retained wrapper is `local/issue534/bound-camera-tracking/run-native.py native-new instant 1 40 yes modern-music-before-battle-tracking`.
Opening/palace/tower/Map40 and2292 W2→2293 W1 are setup. Physical Confirm at2293 must unregister
and close the portrait, close text and run source Sleep10 before binding the real source135 slot.
Read actual moving entity, view axes/counter/speed, matching main camera/tile/actor projection,
settled text/portrait delivery and real route/callers at ordinary2294/95/96 W1. Idle input must add
no service/debt. Confirm2296 and observe the first still-unadmitted white FadeOut:
`full-black-fade-binding` Unsupported before display/helper publication and a stable stopped state.
An earlier failure is discovery evidence, never bypassed. Operand labels identify observation only;
no production endpoint or injected state/seed/capture is permitted. Compare the accepted approach
and old2293 boundary, reporting newly measured consequences; no paired settings run or screenshots.
Bound white has the observation below; later effects/complete before-body/battle load/first input and original cadence remain open.


**Confirmed (bounded semantic readback):** the ordinary native01 continuation reaches real physical
tracking, later2294/95/96 W1 inputs and the first white capability stop. Its raw probe exits1 with
`passed:false` and exactly two observer assertion errors: the generic no-error sampler at the expected
Unsupported boundary and a failure-kind comparison against Unsupported instead of UnsupportedCapability.
Both records remain failures. Independent inspection of immutable actual output establishes
`full-black-fade-binding`/`UnsupportedCapability`, unchanged visible display/no palette helper,
719 tracked draws/7,909 actor projections and610 adjacent live-entity-before-view service checks.
The accepted Map40 approach and2293 W1 gameplay fields compare directly equal. The tested expected-stop
branch executes three idle frames, physical Confirm and the stable stopped-state check before returning;
its exact raw failure set contains no failure of that check. The tested probe version and its two-line
correction provenance are retained. Main-gate approved this semantic acceptance without repeating the
ordinary route solely to turn its report green. The corrected observer was not executed. See the
ignored owning handoff for commands, measured states and complete failures; no original timing claim
or full before-body/battle-entry admission follows.

### Bound white palette observation

Focused `BattleEntryProgramTests.BoundWhiteFade*` engine behaviors cover independent channel extremes,
base-derived offsets from black/restored/white, zero-base visibility, forced1/prior-period restore,
terminator/extra service, early/late/stale/wrong-kind receipts, single/batch equivalence, opposite
program/entity overrides, moving tracked slot and registered portrait/window service. Affected black
scene/warp and camera behaviors remain necessary. Run the focused behavior filter with the configured
absolute SDK through `shared_dotnet_environment`; run `uv run sf2 verify adapter`, the controlled
Debug adapter build and `uv run sf2 design-contracts test`. On clean committed HEAD review
`uv run sf2 verify plan --base origin/main --head HEAD` under current engine scope; record actual CI.
No original/H3 or normal/full legacy aggregate is implied.

The existing native input probe case `portrait-event-zone-nod-camera-choice-yes-private-instant-modern-music-before-battle-white`
uses unchanged retained world-post-load/start, normal60FPS and the established one optional W1 poll.
It takes the ordinary route through existing approach/W1 checkpoints, six finite white services and
intervening waits to Chester2297 W1, observing actual positive white delivery, matching completion,
base/period restoration, continued entity/view/portrait projection and no battle publication.
The endpoint is an observation, not a production legality rule. Compare accepted approach/2293
and subsequent pre-white W1 gameplay checkpoints directly; legitimate new services may change later
clock/RNG. Read state/input/projection/errors, never screenshots or injected state/seeds.

A separate narrow authored startup case `palette-mixed-warp-transition` and its `remapped-reduced`
variant uses the same Main, project and installation, covering black→white→black and repeated white
from settled black/restored/white. It reads the actual ColorRect/parent properties, complete tokens,
normal positive opacity, reduced opacity0 and final input with base/period restored. Newly published
waits can precede their first actual projection by one host frame; assertions read the delivered
projection after that initial handoff. No source export or second clock is involved.

Keep every native output variant, command, process exit and exact failure in fresh ignored
`local/issue534/bound-white-palette/`; a green report on a different branch of the route is not this
acceptance. The older tracking native01 raw FAIL/semantic readback PASS/corrected observer NOT RUN
remains unchanged. The bound mosaic/shiver and first-input continuation is described below; winning and executable H4
applicability remain open; persistent source-fidelity failures and Unknowns are not waived.

## Bound before-body and battle-entry observation

Run focused `BattleEntryProgramTests` through the protected SDK environment, affected
`uv run sf2 verify adapter`, a controlled Debug adapter build and `uv run sf2 design-contracts test`.
Engine cases cover genuine before-context completion, bound/unbound profiles, varied valid palette
words/periods/seeds/accounting/flags, all loader receipts and first Confirm/Cancel. Wrong/stale receipts
and explicit field-only FullBlack outside exploration reject without partial publication. Existing
field helper behaviors remain selected. Review the default planner on clean committed HEAD and actual
CI under current engine scope; no normal/full/H3 or legacy aggregate follows from this slice.

The existing native case adds suffix `modern-music-before-battle-white-entry` to the ordinary
portrait/event/zone/nod/camera/choice-yes private instant route. Load the same ignored environment and
use `local/issue534/bound-battle-entry/run-native.py native-new instant 1 40 yes modern-music-before-battle-white-entry`.
It uses unchanged retained world-post-load/start and normal60 FPS, real inputs through accepted
Chester W1, remaining windows, physical mosaic, five shivers, joins and script return, then the real
battle load/start and first Movement → Confirm/ActionChoice → Cancel/Movement. Stop without committing
an action or turn. During a generic effect hide/show the owned view once and read state/RNG/token,
actual delivery and debt to establish pause/resume. Read existing state/input/projection/errors,
never screenshots or injected/reseeded checkpoints. Retain only bounded generic-token projections
plus delivery/input/mount states to avoid the preserved oversized-output failure.

Keep exact tested probe/wrapper, logs, process result and actual JSON in fresh ignored
`local/issue534/bound-battle-entry/`. Compare prior accepted checkpoints directly; later RNG is a new
observation, not an invented original target. A genuine missing capability stops for allocation.

PR586's six-helper/mixed acceptance and every completed failure stay preserved; original/input delta
is zero.

**Confirmed (bounded remake, PR587 bound battle entry):** `ordinary-normal-02` reports PASS/exit0 with 1569 samples and no
script/process errors. It delivers one mosaic and five shiver tokens, restores sprite-size24 and
physical128's animation counter, projects remaining text2298..2304, six positioned/scripted entities,
and black out/mount/in through first actual Movement/Confirm/Cancel. One hidden mosaic interval
retains tick14931/main3361406976/token and zero debt, then resumes without catch-up. The loader keeps
logical tick15929 and the frozen field facade; first round selects ally2 with main1151074304 and
gold60 under existing initialization/queue rules. These are observations, never legality constants
or claimed original targets. Six prior accepted checkpoints, including Chester, are unchanged;
the old2293 inspector omitted derived PalettePair.White metadata, so comparison normalizes only that
previously omitted derived field. The owning handoff retains exact token/draw/party readbacks.

`ordinary-normal-01` reached the same semantic endpoint with report PASS/exit0 but six script errors
in two old draw hooks after mode handoff; it is not clean native acceptance. Guards now stop field
hooks on a battle observation. The original log/report and first inspector's derived-metadata
comparison failure are preserved, as are initial test CS1501, two test-setup NullReference failures and nullable-assertion CS1503.
Focused corrected engine selection passes51; those corrections did not broaden the verification scope.

## Bound victory and return observation

Run the affected `BattleOutcomeProgramTests`, field text/portrait and entry behavior cases through
the protected SDK environment; run `uv run sf2 verify adapter`, the controlled Debug build and
`uv run sf2 design-contracts test`. Review the default planner at clean committed HEAD and actual
CI under engine scope. Outcome cases use a real last-action scene result with the facade's carried
story semantics, changed positions and dead membership, then drain genuine outcome/return windows
and nested map init before exercising a field Move. Invalid ownership and stale fade receipts reject
before publication. This selection does not require normal/full/H3 or the retired aggregate.

For the continuous bound winning baseline use the existing ordinary probe wrapper with
`ordinary-new instant 1 40 yes modern-music-before-battle-white-entry-victory`. Fixed 60 FPS comes
from `--fixed-fps 60`; keep the accepted 40-character rate, optional poll and unchanged start/world.
The existing external policy chooses legal actions from current actors and board, checking actual
preview, stage, target, session identity and failures before commits. Settle actual scene messages,
W1/W2/plain input and presentation deliveries, then require genuine returned field input plus a
real settled Move. Strategy failure or an observability/capability gap is retained and reported.

The remaining before-body observer uses actual readiness, logical tick/cursor/wait/text phase and
delivery progress for the winning suffix, with a finite no-progress timeout and the outer wall
timeout. Process frames and redraw counts alone cannot exhaust an allowed focus suspension. Reuse
real focus detection/restoration, retain pre/post state and verify zero-debt/no-catch-up at that
boundary. Terminal records include full state, focus/visibility, wait/readiness and receipt fields
with elapsed time and the stopping reason. A stalled or unavailable run remains a failed gate.

Keep every result signal and new combat/outcome state boundary. Prefix result records retain compact
identity/tick/cursor/RNG state; full prior samples and the zone/choice/nod states consumed by existing
assertions remain. Attach payloads are retained before initialized projection reads. Battle terrain
is retained at first input instead of repeated at every result. Save tested probes/wrapper, output,
log and process status in fresh ignored `local/issue534/bound-victory-return/`. Compare prior accepted
checkpoints directly. This observation does not make unavailable H4 variants pass or waive HEAL,
warp, original accounting or presentation gaps.

Select the accepted cumulative battle scene through `SF2_PRIVATE_BATTLE_SCENE_CONTENT`, including
healing and fieldDeath movement/death sprite bindings. `SF2_PRIVATE_BATTLE01_SCENE` selects encounter
placement and does not substitute for this scene document. Copy accepted private scene/provenance
bytes into this worktree's ignored input directory and compare directly; use the existing reader
to check party/world admission and retained scene bindings before the route. Engine outcome cases
and a passing adapter build do not satisfy native winning acceptance.

Inspect all result failures directly. Only an actual TAB target-browse `IllegalCommand/target-range`
may be retained under the [existing menu contract](architecture.md#production-assemblies): compare session,
actor, accepted selection/preview, party resources/progress/loadout, queue and RNG with the prior
state, require no action/turn observations, then confirm a subsequently accepted legal target.
Disposable actor-node geometry and attempted UI candidate may change; accepted gameplay state may
not. Other errors remain failures. A green report with callback/script errors is not clean acceptance.
The continuous winning and bound victory/return scope above has native acceptance;
executable H4 policy cutover and unobserved outcome/return variants remain open.

## Bounded H4 capture

The H4 input probe defaults to `sf2-observation-jsonl-v1`. Its existing ignored output destination
contains a header, ordered records and a successful terminal after the writer drains, flushes and
closes. Each envelope has `captureSequence`, `channel`, zero-based channel `index`, and `payload`.
A missing terminal, duplicate/gapped ordinal, detached descriptor or record after terminal fails
readback. An output prefix is discovery evidence, never a completed capture. The observation does
not alter gameplay clocks, logical services, RNG, rendering decisions or #517 speech policy.

The probe installs `game/src/Observation/ObservationCapture.cs` before Main. Existing result,
audio and actual draw boundaries supply immutable primitive facts. Main-thread Godot dictionaries
are copied before enqueue; newly built CLR facts are transferred without a JSON/Godot round trip.
Field consumer and camera callbacks expose only their control operands to GDScript. Their full
state/projection evidence is snapshotted directly into owned CLR primitives on the game thread;
lazy collections are materialized before enqueue, so later live changes cannot alter queued facts.
One worker serializes UTF-8 and writes in capture order. No live Godot object crosses that boundary.
Resource descriptors are emitted before their uses and reset when the owning view detaches;
native session/revision/token/draw identity remains in the use record. Actual uses are retained,
with at most 64 uses per draw envelope. The reader expands this transport batching to the existing
logical resource-use records. It does not merge actual uses with independent requirements.

Capture limits are 32MiB of conservative queued encoded size, 1MiB per encoded record, and 64MiB of
charged pending storage including the fixed writer/control reserve. Active requirement keys,
descriptors and route-control witnesses have explicit lifetime/count/memory limits. Overflow,
serialization/I/O error, cancellation or session replacement fails the capture and promptly
removes observation callbacks. Success requires an intact terminal and successful process/status
contract; a performance sample from a failed capture cannot establish evidence or performance PASS.

`SF2_CAPTURE_LEGACY=1` retains the historical JSON path for explicit compatibility diagnostics.
`SF2_CAPTURE_STREAM=1` explicitly selects the stream. For a current H4 capture pass the same stream
to `--actual` and `--outcome`; outcome records and summary are embedded channels. Historical
JSON plus its separate outcome file remains readable. Existing comparison predicates, raw
ordered differences, source provenance and Unavailable boundaries still apply.

The complete H4 probe publishes its five retained snapshots (`admissionSnapshot`,
`rawTextBoundary`, `musicLogicalEnd`, `musicPlainInput`, `joinReturn`) as separate
`captureMetadata` records with `key` and `value`, then declares exactly those keys in terminal
`captureMetadataKeys`. Each record still obeys the existing size and queue limits. The reader
restores the original top-level fields, accepts the earlier inline terminal format, and rejects
missing, duplicate, unknown or conflicting split fields. Generic short captures need no H4 metadata.
Battle policy selection explicitly reads full current state for terrain; per-frame observations
remain lightweight. Native integer flags and party IDs compare numerically with historical JSON
numbers, preserving exact values, sequence order and nonnumeric types. Zone-return control retains
only the four scalar timing operands it consumes; the stream retains the complete source record.
Nod texture identities use decimal strings, like other native resource identities, preserving
unsigned instance IDs without a signed conversion.

The current-format reader uses Python's standard-library SQLite for private derived records
and explicit ordinal/native/resource joins. Reads are detached values: complete a mutable record
before publishing it. Group children within a working database remain explicitly appendable;
foreign published stores are copied when incorporated into a new report. Numeric equality and
first-seen/native order remain comparison rules, not SQLite type-affinity or collation rules.
Event-to-state associations and state sorting keep row references instead of repeating snapshots.
Resource validation consumes bounded records without rebuilding a whole-history Python list.

Each working database uses DELETE journaling with FULL synchronization. Transactions admit at
most 256 record/index publications and 1MiB of encoded values; keys are independently record-bounded.
The SQLite page-cache target is 8MiB, not a total-process memory guarantee. An encoded row has a
1MiB limit and its decoded representation has a 16MiB limit. Cursors and bounded sort batches avoid
whole-run indexes in Python. Closing without an explicit successful flush rolls back the remaining
partial transaction; committed scratch prefixes remain discovery evidence. Historical plain JSON
inputs retain their existing read path.

CLI scratch directories are fresh `reader-*` children beside the explicitly selected report
output, beneath the owning worktree's `local/`. Direct capture reads default to that input's parent.
Relative paths resolve from the repository root, independently of root/package cwd. Opening a
published report is read-only and does not create scratch; deriving new results creates a fresh
working database only when needed.

A current report uses `streamReportFormat=sf2-h4-sqlite-report-v1` and one companion named
`<report filename>.sqlite` in the same directory. Its `streamDatabase` name resolves relative to
the report. Publication copies all referenced derived records, including nested groups and records
from other reports, into that companion using bounded operations. References inside the companion
are store IDs, not source-database paths. The complete database commits and closes before the JSON
entry point is published. A failed write does not publish a successful entry point; retain its
partial artifacts and choose a fresh output name for a later run.

Preserve or move the report and its companion together. Reopen the pair through the maintained
reader; previous scratch or input-report databases are not needed for derived-row access, including
matrix output. Historical evidence/provenance paths remain provenance and are not rewritten by
this storage transport. Earlier private `sf2-h4-stream-report-v1` diagnostics require their retained
reader source version; there is no second maintained write backend. Databases, captures, reports
and extracted inputs remain private and are never committed.

Use affected adapter compilation, probe check-only, direct writer failures/bounds, retained-record
comparison and the explicitly allocated native performance window. These are direct verification
drives, not tests of verification helpers. Capture availability does not close missing original
map working-layout operands, C/D observations or full H4 acceptance. The bounded performance and
correction evidence belongs to [Issue #605](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/605).

For bounded route timing, `ReadCaptureWitness(true)` supplies actual entity position/facing/busy,
text/portrait, focus and draw identities without serializing unused resource history. Preserve full
initial/final snapshots outside measurement and field equality against the full reader in both
capture modes. A changed timing reader requires a matched off/on pair with the same helper, build
and settings. GDScript calls pass the witness mode explicitly; C# optional defaults are not supplied
by Godot's dynamic `call` bridge. Production capture callbacks use `false` and retain full evidence
through their separate synchronous snapshot path.

## Modern continuous H4 comparison

The maintained module retains its default legacy JSONL/plan diagnostic. Select the explicit modern
profile for a current capture stream (also passed as outcome), or historical sample/result JSON
plus its outcome file, with settings/log and recorded process exit:

```powershell
uv run python -m sf2tool.remake_h4_comparison compare --profile modern-continuous `
  --reference local/issue534/modern-h4-applicability/inputs-01/reference.json `
  --actual $actual --outcome $outcome --settings $settings --host-log $hostLog --host-exit $recordedExit `
  --controlled-start remake/reference/inputs/map3-opening-party.json `
  --output $freshReport
# Repeat --variant-report for each actual A/B/C/D report; missing named variants stay Unavailable.
uv run python -m sf2tool.remake_h4_comparison matrix `
  --reference local/issue534/modern-h4-applicability/inputs-01/reference.json `
  --variant-report $reportA --variant-report $reportB --variant-report $reportC --variant-report $reportD `
  --output $freshMatrixReport
```

Reference/provenance inputs are exact copies of retained accepted projection and extension records
in this worktree's ignored inputs; select them explicitly rather than depending on another removable
worktree's mutable report. The original projector, legacy driver/goldens and production are read-only.
Modern reports name original and actual record locations, applicability and reason before equality.
Historical trajectory mismatches retain raw results and separate counts; required observed errors are
FAIL, absent required original/actual fields are Unavailable with the missing side identified.
Exit1 means required FAIL, exit2 incomplete/Unavailable, exit0 complete applicable acceptance. Neither
a successful report command nor a variant-pair PASS implies milestone acceptance.

The optional controlled-start argument selects candidate definition values explicitly. Reports
separate that input from `admissionSnapshot`, captured before the first host frame/input in a new
H4 session. Its `admittedParty` reads the actual session's package, origin, existing provenance and
explicitly named encounter/deployments, alongside the party override/progress operands. Field startup
has no instantiated battle actor: these are admitted definitions. Item comparison uses the existing
override → progress → selected deployment precedence. Definition identity requires matching actor/member,
class/stats/spell words and party input to the selected frozen input in that same session; multiple
encounters or deployments never permit an arbitrary first match. Effective stat progress beyond this
bound null-progress admission remains unobserved. Later loaded/ready actor fields are corroboration,
not a replacement for the startup record. Retained sessions lacking the snapshot remain Unavailable;
new-session observations never backfill their missing fields. Walking timer expectations come
directly from selected original `inherited.entities[].waitTimer`. The corresponding `actionScript`
pointer is retained, but cursor/moving remains required-unobserved until a precise source
template/base/offset and movement-gate binding is executable. Existing bounded runtime phase-correction
evidence is not a replacement for that original expected-value binding; neither remake start nor
coordinate differences prove it. Use only A's optional normal05
baseline arguments: C/D's different presentation settings are intentional; the four-report matrix
owns their gameplay equivalence.

`coverageObligations` retains all eight parents and their required child lists/counts. Main assertion
counts evaluate children, without counting those parent summaries again. The
[contract's child families](../../docs/design/contracts/map3-battle01-continuous-scenario.md#admission-and-required-child-obligations)
own the complete reached winning scope. Required-unobserved children identify original, actual or
semantic-join gaps even when candidate metadata is available. A parent needs every required child;
selected witnessed fields cannot close unobserved sibling obligations.

The comparator checks the frozen required family/child declarations before returning a modern
report. The matrix also checks each supplied report's required children, unique rows/parents,
parent child lists/counts/verdicts, and assertion/historical/report summaries. The selected accepted
reference also supplies required admission/return flags, occupied slots and admitted ally identities.
The independent initial map/position/facing/gold/seed and each admitted ally's HP/MP/status
comparisons are required even when summaries are reconciled after their omission. Missing evidence
represented by a required-unobserved row remains Unavailable; omitted required rows or families,
duplicate rows/parents and contradictory results are malformed reports and make the matrix FAIL,
with `remaining[*].integrityErrors` identifying the inconsistency. Valid report fields and order
remain unchanged. Optional material/JOIN subsets and historical diagnostics do not enlarge the
frozen required child set; retained reports lacking newer subset rows remain valid.

Historically, `complete named continuous settings matrix` closed only after all four named variants satisfied
settings and gameplay equivalence. The current keyboard scope supersedes that requirement. Every
other required A child and coverage parent still gates acceptance, using FAIL before Unavailable
before PASS. For a direct integrity reproduction, omit A's Unavailable assertion rows while retaining
its original summaries and coverage parents, then call `compare_matrix(paths, selected_reference)`.
The result must be FAIL with `milestonePass=false`; unmodified A retains 13 other Unavailable children.
Also inspect missing required families/children,
duplicate or conflicting rows/parents/summaries, known FAIL with missing evidence, and the single
matrix-self closure on otherwise complete synthetic copies. Keep those synthetic cases separate
from actual game evidence and preserve the source files and completed historical failures.

Reuse retained A-D02 process receipts/logs/settings/actual/outcome and accepted original/Down inputs
for fresh offline reports. Read each recorded native exit from its own receipt, load the ignored
private-input configuration in the launching process, and preserve previous reports/failures. The
direct legacy comparison must remain unchanged. Observation changes use affected adapter compilation,
probe check-only, direct comparator/lint/document checks and committed plan/actual CI. A required new
native observation needs its named boundary and fresh outputs; no verification-helper tests or
normal/full/H3 suite follows from report changes.

### Same-session actual observation

The existing H4 input probe captures `admissionSnapshot` immediately after Main constructs its
session and retains the settled initial sample separately. Freeze the selected party/start/settings
under the owning ignored run root; record their paths relative to the explicit repository root in
the existing process receipt. The settings destination must be fresh for the probe to create; compare
the generated settings with the separately frozen expected selection. Explicitly select the accepted
A world instead of deriving selection from an output-directory name. Package/provenance alone does
not identify a file; verify admitted values against the recorded frozen selection.

`sceneObservations` retains changed mounted resource/phase/completion facts with session, revision,
observation sequence, token, input ordinal and host update. `signal-before-Present` identifies a result
callback's preceding node projection; `host-poll` identifies actual subsequently polled nodes. Token
and phase must be read from the projection, not assumed from the newly published result. Missing
transient completion remains unobserved. Hidden node metadata does not prove consumption.

All H4 variants can retain the existing global audio API's unseen receipts in `audioReceipts` with
separate poll context. Startup, each process frame, inputs and terminal sampling check sequence1
through `audioTerminal.sequence`, including an overflow before the first poll. `audioReceiptGaps`
retains missed ranges. The terminal read precedes host teardown; ongoing field music needs no invented
end receipt. Cue start/stop/finish totals classify actual playback, without treating a wait token as
a playback ID or assigning invented generations to overlapping same-cue voices. D's existing speech
subset remains separate. Polling hooks disconnect at finish; production's 64-receipt buffer is unchanged.

One new complete A can join admission, battle resources/audio and after/return in one session.
A startup-only run cannot provide those later observations. Keep accepted A-D02 equivalence and D02
speech evidence identified as their historical sessions; compare new A's common gameplay to A02,
without claiming that B/C/D observed new fields. Instant A does not observe D's natural-reveal speech.
Actual resource identities or contiguous receipts supply only their actual side; original resource,
mailbox, rule and service/consumer joins remain independently required.

For the ordinary winning suffix set `SF2_H4_VARIANT` to A/B/C/D while retaining the accepted normal05
start/world/scene/content/party/RNG, fixed60 FPS, optionalpoll1 and adaptive legal battle policy:

| Variant | Physical input and actual settings |
| --- | --- |
| A | Default keyboard; standard Confirm/Cancel; normal flash; instant40 |
| B | Default gamepad with one intended direction delivered via supported left stick; standard; normal; instant40 |
| C | Remapped keyboard; swapped Confirm/Cancel; reduced flash; adjustable20 with reveal-only Confirm |
| D | Remapped gamepad with admitted right stick; swapped; reduced; adjustable20 natural reveal |

Each run records actual press/release identity, synchronous dispatch intervals and result consumer
ordinals. Only a submit observed inside that interval establishes consumed physical input; the latest
ordinal on a later automatic result does not. Actual field and battle Label readiness separates
reveal-only Confirm from acknowledgement; D waits natural reveal before Confirm without gameplay Wait.
Compare ordered logical Wait/ack/command observations, action states, admission and reached endpoint
resources/state. Preserve text-revealed and single automatic healing scene-delivery notifications in a
separate raw list and require unchanged gameplay/RNG service. Their host timing may interleave with
mandatory work; acknowledgements and scene continuation remain ordered. Reveal-only Confirm adds no
semantic input or service. The first stable returned endpoint has two
separate host-update reads before Left; preserve actual Left, observe two settled pre-Down updates,
then ordinary Down and two post-Down updates. Host observation does not issue a gameplay Wait.
Observe player/camera, genuine readiness, cleared callers/waits/transfer/battle/modal and zero debt.
Reject a blocked Down honestly; no injected position or substitute direction. Four actual reports are
required for the named matrix, with each report's outstanding original/host bindings still retained.

Accepted normal05 remains the prior ordinary continuous winning/Left evidence. Its duplicated final
sample/outcome reads lack independent update identities and cannot supply the named Down or full
extended settings stream. Existing authored-only pairs also cannot supply continuous variants.
Direct comparison, affected Python lint, probe check-only and direct contract/document checks verify
this verification-tool change; no tests of probes/comparator or normal/full/H3 suite are added.

For A, optional `--baseline-actual` and `--baseline-outcome` compare the accepted normal05
shared checkpoints and Left endpoint directly; they do not substitute an old authored pair for A-D.

The bounded continuous A-D matrix is observed: four variant PASS results, with overall Unavailable
and milestonePass=false because eight required original/actual bindings per variant remain open.
A/B/C retain the earlier world; D uses a fresh world adding only exact speech70/73 BD from the
[audio owner's pinned pack](presentation-and-assets.md#audio-boundary). Preserve direct equality with the prior world after removing the two new rows, and the dependency
check for actual used resources when reusing those runs.
D receipts distinguish 23/8 starts, 22/7 replacements/stops and one Finished per command, with no
sequence gap or added gameplay Wait. Observer-only comparisons and partial historic receipt windows
must not be treated as complete original consumer evidence. The preserved D01 audio error belongs
to actual host presentation even though its SessionResult has no failure.

### Offline reached material comparison

Use the retained actual session without a new native run. Select every material input explicitly;
relative material paths resolve from this repository root, absolute paths from the explicitly chosen
data root. The recorded process selection must match the selected world and scene. Do not follow
foreign absolute paths in copy receipts. When authorized, freeze only the existing base candidate's
`battle-scenes.json`, `candidate-report.json`, `source/battle-scenes/selection.json` and
`manifests/presentation-assets-v1.json`, preserving relative names and byte equality. Keep the source
read-only and record selection in the ignored run handoff. No extraction/export/library promotion.

```powershell
# Existing read-only checkout validator; keep asset pins from the accepted selection.
uv run python -m sf2tool.remake_assets checkout --asset-root $assetRoot `
  --expected-commit $assetCommit --expected-tree $assetTree --expected-manifest-sha256 $assetManifestSha
uv run python -m sf2tool.remake_h4_comparison compare --profile modern-continuous `
  --reference $reference --actual $actual --outcome $outcome --settings $settings `
  --host-log $hostLog --host-exit $recordedExit --controlled-start $frozenParty `
  --selected-world $world --selected-scene $scene --process-receipt $processReceipt `
  --scene-evidence-root $sceneEvidenceRoot --asset-root $assetRoot `
  --expected-asset-commit $assetCommit --expected-asset-tree $assetTree `
  --expected-asset-manifest-sha256 $assetManifestSha --output $freshReport
```

The optional material arguments are supplied together. Missing selections or input files remain
Unavailable; observed identity/content drift is FAIL. Reports retain selected inputs, per-start
requested/recording timers, source records and actual scene/receipt joins. The existing base42 source
and manifest pins are required; its fingerprint-v1 reproduces historical CRLF bytes from the named
Git object, reporting current and Git LF fingerprints separately. Evidence bytes are never normalized.
Original audio capture cuts must equal runtime PCM and the selected world's raw PCM; container hashes
are checked independently by checkout preflight. Continuous starts/stops/finishes/fades and terminal
voices retain the already observed lifecycle boundary.

Run `matrix --matrix-scope current-keyboard` with the fresh A report and optional supplemental C.
Retained B/D reports stay outside required acceptance. Compare all gameplay
equivalence fields to the accepted A report; keep old sessions' absent fields Unavailable. Direct
legacy applicability must remain unchanged. Use affected lint/design-contract checks and committed
planner/actual CI. This offline comparator slice needs no SDK/native/capture, new source acquisition,
normal/full/H3 or verification-helper tests. Background/ground and audio material closure plus an
actor/weapon subset alone leave other resource families and operation/consumer assertions
unavailable in that retained report. Their current composed acceptance is recorded
[below](#accepted-composition-review).

### Scoped physical consumer comparison

The implementation route is [H4 physical modules](../../docs/operations/bounded-inspection-and-review.md#h4-physical-module-route).
The CLI and `sf2tool.remake_h4_comparison.physical_consumer_binding` import remain available;
direct physical observations now import `sf2tool.remake_h4.physical_binding`. Value/ordered matching
observations import `match`, `ordered_match` and `absent` from `sf2tool.remake_h4.physical_checks`,
instead of extracting the old function's private closures. Source operands/action live in
`physical_source`; AI/scene/reward consumers retain explicit aliases in the existing entry module.


Use the [selected physical contract](../../docs/design/contracts/map3-battle01-continuous-scenario.md#selected-physical-rule-and-consumer-binding)
with the existing compact actual and independent context. It shares the modern physical child and
does not require a full H4 run, SDK, host, emulator, world export or reference reconstruction.

```powershell
. ./local/private-inputs.ps1
uv run python -X utf8 -m sf2tool.remake_h4_comparison physical `
  --actual local/issue534/physical-binding-01/actual.json `
  --physical-context local/issue534/physical-binding-01/context.json `
  --text-source-root $pinnedSource `
  --output local/physical-consumers/fresh-report.json
```

The context has `scope=retained-keyboard-A-physical`, `sessionId`, the producer/source revisions,
`initialSampleIndex`, `profileDeclaration`, complete `census`, `battleBounds`, immutable-selection
receipt and per-channel `indices`. Every census member retains original preparation index,
revision/sequence, first physical draw sequence, actor/target, input ordinal and scene-end identity.
Actual `warpRecords`, `inputRecords`, `sceneObservations` and `samples` retain original `_index`
values; selected Submit observation arrays and input result intervals remain complete. Missing
indices and contradictory repeated events cannot silently reduce coverage. Modern comparison
accepts the same `--physical-context`; no separate physical oracle is used there.

To reproduce the selected numeric/boolean counterexample directly, run this after the same-process
private-input setup and selection of `$pinnedSource`. Each call uses a fresh in-memory copy of the
compact selection; neither input file is rewritten. The genuine baseline and equal numeric/null
representations return `true`, type/null contradictions return `false`, missing-only controls return
`null` (Unavailable), and a contradiction with another missing field still returns `false`.

```powershell
$physicalControl = @'
import copy, json, sys
from pathlib import Path
from sf2tool.remake_h4.physical_binding import physical_consumer_binding
p = Path('local/issue534/physical-binding-01')
a0 = json.loads((p / 'actual.json').read_bytes())
c0 = json.loads((p / 'context.json').read_bytes())
def reaction(a):
    return next(p['scene'] for p in a['sceneObservations']
                if p['_index'] == 1703 and p['scene']['waitToken'] == 28901)
for name in ('baseline', 'number-as-bool', 'bool-as-number', 'equal-number',
             'null-as-zero', 'missing-observed', 'missing-expected', 'mixed'):
    a, c = copy.deepcopy(a0), copy.deepcopy(c0)
    if name in ('number-as-bool', 'mixed'):
        reaction(a)['reactionAmount'] = False
    if name == 'bool-as-number': c['selectionReceipt']['sourceUnchanged'] = 1
    if name == 'equal-number': reaction(a)['reactionAmount'] = 0
    if name == 'null-as-zero': c['selectionReceipt']['failure'] = 0
    if name == 'missing-observed': reaction(a).pop('reactionAmount')
    if name in ('missing-expected', 'mixed'): c.pop('profileDeclaration')
    result = physical_consumer_binding(a, c, sys.argv[1])
    print(json.dumps(dict(case=name, value=result['value'],
        nonpass=list(dict.fromkeys(x['name'] for x in result['checks']
                                 if x['value'] is not True))[:6])))
'@
uv run --locked python -X utf8 -c $physicalControl $pinnedSource
```

This is a bounded direct control of the complete physical child, not a maintained verifier/test
suite or a full modern/CLI-schema acceptance claim. Numeric equivalence does not relax the separate
integer-domain checks. Explicit null cannot replace zero or either side's missing marker. Reuse
the retained physical clock/input/projection controls with fresh output names; keep their prior
failures and the original type counterexample. Object subset, list-length and ordered-event
semantics retain the [physical contract](../../docs/design/contracts/map3-battle01-continuous-scenario.md#selected-physical-rule-and-consumer-binding).

For extraction review, compare the complete ordered result before/after, including every check,
occurrence, source rule, diagnostic and Unknown. Reuse the retained #534 physical controls, #629
clock/input/projection counterexamples and #637 value controls; write fresh local results in batches
of at most 16 for the initial 1MiB output budget. Lossless compression is permitted; truncated
nonpass summaries alone do not establish equivalence. Check the retained CLI's PASS/FAIL/Unavailable
exit codes (0/1/2), real source/import aliases and a selected SQLite-backed channel observation whose
caller closes the context after comparison. This is direct verification, not a new verifier test
suite; it grants no full-route rerun or cleanup of historical inputs/results/failures.

The one admitted raw selection traversed the registered 1,375,851,198-byte historical A once in
25.8418791 seconds, with 3,522,560 bytes incremental peak memory and unchanged source size/mtime.
It retained 3,966,366 bytes of JSONL; normalized actual is 3,895,810 bytes. The full battle census
includes automatic enemies and nested next-preparation events, beyond the outcome companion's
14 player decisions. It preserves the final attack's record68 association and all prior compact
preflight failures. Reproduction uses these compact files; this recipe grants no new raw scan.

Acquisition ceilings remain 120s/128MiB incremental, 2MiB per raw record, selected output10MiB with
64KiB receipt headroom, normalized input/output10MiB, total new output24MiB. Preserve incomplete
attempts and obtain a concrete revised allocation on a genuine cap/schema/identity failure.
The scoped CLI limits actual plus context to10MiB, context to1MiB and report to10MiB; output must
be fresh beneath the owning worktree's ignored `local/`. Exit0/1/2 means PASS/FAIL/Unavailable,
and `milestonePass` remains false. Pinned source parsing reads only matched class/enemy/item
operands and the battle01 compressed terrain, using the existing decoder; it does not rebuild assets.

Direct controls cover wrong damage and live HP, premature effects, command-phase errors, RNG/seed
changes, range, source identity, input press/result joins, missing operands/records, foreign session,
wrong event order, census coverage and scene strike/token/reaction mismatches. Wrong HP or RNG must
still fail without source operands; absence alone remains Unavailable. Independent review controls
also reject effects reassigned to existing future inputs, corrupt automatic-input after snapshots,
foreign projection input ordinals and negative completion revisions even when the census is changed
to agree. Positive clock values still require progression and result bounds. Reuse span/snapshot
joins for player, automatic and effect results; join projections to result identity and host-input
chronology, preserving host-poll and before-Present semantics. Preserve the first pilot's
wrong helper-type exception, the later overbroad rejection/HP diagnostic failures and the missing
press classification failure, initial CLI exit failure and independent review's four false-PASS
counterexamples alongside corrected results. No tests of this verifier are introduced.
Run scoped Ruff, direct contracts/docs/private-boundary checks, the committed planner interpreted
under current verification policy, and exact-head CI. Existing normal-verification provenance FAIL,
historical A FAIL and original elapsed-time Unknown remain unchanged. Independent main-gate
acceptance owns obligation closure.

### Scoped reward and outcome comparison

Direct observations import `reward_consumer_binding` from `sf2tool.remake_h4.reward_binding`;
the old import and CLI below remain available. The [reward module route](../../docs/operations/bounded-inspection-and-review.md#h4-reward-module-route)
separates evidence, source obligations, the persistent resource ledger and return consumers.
For extraction review, use `joined-actual.json` (the older `actual.json` lacks return), retain the
accepted complete baseline after checking its current equality, and compare every serialized field,
check/count/order, occurrence and Unknown in small batches. Include return-gold, rejected JOIN,
stale completion-token and missing-plus-contradiction controls; a matching PASS alone is insufficient.

The [selected reward contract](../../docs/design/contracts/map3-battle01-continuous-scenario.md#selected-reward-growth-and-outcome-consumer-binding)
uses existing compact retained evidence and pinned source rules. It shares the modern
reward child; no full H4 run, world export, SDK, host or original-runtime launch is needed.

```powershell
. ./local/private-inputs.ps1
uv run python -X utf8 -m sf2tool.remake_h4_comparison reward `
  --actual local/issue534/reward-binding-01/joined-actual.json `
  --reward-context local/issue534/reward-binding-01/context.json `
  --text-source-root $pinnedSource `
  --output local/reward-consumers/fresh-report.json
```

Resolve `$pinnedSource` through the local private-input owner. The context declares
`scope=retained-keyboard-A-reward`, session, producer/source revisions, original per-channel
`indices`, complete semantic `census` and `scenes`, selection receipts, `initialSample`,
`battleSample`, `preBattlePartySample` and the exact `firstOutcomeParty` boundary. Census
rows carry result index, revision, sequence, kind, actor and target. Complete selected
Submit arrays, input intervals and scene projection identities retain their original
`_index`; neither observed subsets nor missing channels redefine the declared census.
Modern comparison accepts this same `--reward-context` and predicate.

Reproduction reads the retained selection; this recipe authorizes no new raw scan. The
first authorized semantic selection completed within its budget but failed sufficiency:
five post-battle transition kinds were counted without their envelopes, and an immediate
previous-actor condition missed the first party behind an empty view bridge. The authorized
correction retains those envelopes and their neighbors only within the same admitted
Battle01 outcome/return, stopping after its following neighbor. It preserves the first
party as observed, including empty/partial operands, rather than seeking a later adequate
state. Retain `selection-sufficiency-failure.json` and both selection receipts beside the
joined input. Initial selection used26.5580872s/4,075,520B incremental peak; correction
used20.1643785s/2,207,744B. Joined actual is5,116,740B. Original source metadata stayed
unchanged, and normalization stayed within its10MiB input/output bundle.

Acquisition ceilings are120s/128MiB incremental and2MiB per raw record, selected10MiB,
normalized10MiB, total fresh output24MiB; the correction has1MiB selected/1MiB normalized
and3MiB fresh limits. These are allocation records, not automatic retry authority. The
scoped CLI accepts actual plus context at most10MiB, context at most1MiB and report at
most10MiB, with fresh output beneath this worktree's ignored `local/`. Repeated successful
checks are summarized by name/count to preserve publication headroom; every failure,
missing operand and scene occurrence stays explicit. Exit0/1/2 means
PASS/FAIL/Unavailable; `milestonePass` remains false.

Direct controls exercise reward values/recipients, source RNG and growth, deferred EXP,
missing records, first-party progress and membership, victory healing, outcome flags,
source/session identity, automatic input ownership, projection phase/token/completion,
event ordering and census coverage. Preserve the initial controls' two classification
failures (missing first-party fields and shortened membership), the later missing-source
cascade failure, and the pilot's incorrect HP-event actor mapping alongside their corrected
results. HP events identify the recipient; they do not carry the attacking actor as their
Actor. Known wrong effects must still fail
when a separate field is missing. No tests of this verification program are introduced.

Independent review controls also require a later before-Present Reward completion to
reject an earlier scene's Reward token, a rejected outcome JOIN to fail even with missing
first-party gold, and a corrupted return gold snapshot to fail. Preserve the original
false-PASS/Unavailable results beside targeted corrections. Exact completion ownership
retains the producer's before-Present semantics; owned outcome results require success,
and gold is checked through return. EXP applicability remains the reached source-initial
level1/2 allies versus level0 GIZMO cohort, not a general overkill/level-difference model.

Run scoped Ruff, direct contracts/docs/private-boundary checks, the committed planner under
the current verification policy and exact-head CI. Historical A FAIL, original elapsed
Unknown and the completed normal-verification provenance failure remain unchanged. Main-gate
owns independent integration and obligation closure; native/full routes and adjacent
accepted predicates are outside this slice.

### Scoped battle-scene consumer comparison

Direct observations call `sf2tool.remake_h4.scene_binding.battle_scene_consumer_binding` with an
explicit document reader; the old entry passes its existing `read` callable. Selection/source aliases
remain available. Use the [module route](../../docs/operations/bounded-inspection-and-review.md#h4-battle-scene-module-route)
for state and loader ownership. Adapt retained candidate-reader controls at this explicit seam,
without replacing module globals or executing historical writers. Reuse the accepted complete
baseline after equality and compare all report fields, counts, order, occurrences and Unknowns.
Keep absent-census animation diagnostics separate from movement equivalence: missing `rewardContext`
previously reached an uninitialized animation variable; phase-local progress now retains the existing
census contradictions as FAIL. A source-only omission still returns Unavailable. Observe those
complete results and actual CLI diagnostics, without turning every missing dependency into FAIL.

The [selected scene contract](../../docs/design/contracts/map3-battle01-continuous-scenario.md#selected-battle-scene-command-and-consumer-binding)
binds independent source construction/resources to actual scene consumption. Reuse the
retained selection; a scoped comparison does not read the whole capture or run Godot:

```powershell
. ./local/private-inputs.ps1
uv run python -X utf8 -m sf2tool.remake_h4_comparison battle-scene `
  --actual local/issue534/battle-scene-binding-01/selected-01/records.jsonl `
  --scene-context local/issue534/battle-scene-binding-01/context.json `
  --text-source-root $pinnedSource `
  --output local/battle-scene/fresh-report.json
```

Resolve `$pinnedSource` through the private-input owner. Context scope is
`retained-modern-A-battle-scene`, with `sessionId`, tested `producer`, `upstream`, the
`selection` receipt, `materials` metadata and existing canonical `selectedScene`.
`physicalActual`, `healActual`, `rewardActual` and `aiActual` reference the existing
compact selections; `physicalContext`, `healContext` and `rewardContext` supply their
independent census. Relative paths resolve from this checkout's repository root.
The selected JSONL preserves `{index, record}`; full in-memory channels also retain their
original array indices. HEAL cursors join their already retained original indices instead
of being archived twice. Reduced actor lists merge by identity and agree on common fields;
attach may republish the preceding submit's observations with matching clocks.

Bounds: selection 8MiB, context/material metadata 512KiB, receipt 128KiB, each referenced
compact input 10MiB and output 1MiB. Output must be fresh under this worktree's `local/`.
The predicate aggregates repeated checks while retaining failing/missing examples and
coverage; the report never embeds the private capture. The measured selected baseline
uses about 89MiB incremental peak and one second; direct controls run serially under the
128MiB/120-second per-process allocation. These are measurements, not gameplay limits.
No raw traversal, acquisition, asset export, native/SDK launch, full H4 or matrix is needed.

`compare --profile modern-continuous --scene-context ...` calls the same predicate and
replaces only the named scene child. It does not substitute the selected observations
for a supplied modern channel: current scene projections are checked against the same
independent census and retained dependencies. Other obligation results and
`milestonePass` retain their normal aggregation. Scoped exit codes are 0 PASS, 1 FAIL,
2 Unavailable; scoped `milestonePass` is always false.

Direct controls cover stale/foreign tokens, actors, clocks and causal inputs; animation,
message, background, weapon, fairy and field-death resources/effects; missing phase and
completion evidence; hidden or missing message labels; independent critical text/effect
expectations; unready/non-Confirm acknowledgements; ordinary non-input completion and
missing contiguous input brackets; terminal commit actor and non-input transport,
cleanup, ordered end events, attach and returned field control. Missing source/evidence
must not hide known contradictions. Test the ordinary FieldSettle missing-completion case
separately: it cannot borrow the terminal composition. Keep all completed failures and
rerun only corrected cases and affected gates. Do not add tests of this verifier.

Acceptance uses scoped Ruff, direct contract/docs/private checks, the clean committed
planner under the current verification scope, and exact-head CI. Previously accepted
predicates and production consumers stay unchanged. The terminal flag is Inferred and
delay Unknown; source natural timing, historical A seed latch and HEAL timing failures
remain explicit. Freeze a clean Draft PR for main-gate's independent review.


### Scoped field-service comparison

The [selected field-service contract](../../docs/design/contracts/map3-battle01-continuous-scenario.md#selected-field-service-rule-and-consumer-binding)
composes pinned original rules, accepted executed mechanisms and bounded current native
input/state/Draw observations. Reuse the retained six cases; no raw-A selection or
continuous route is needed:

```powershell
. ./local/private-inputs.ps1
uv run python -X utf8 -m sf2tool.remake_h4_comparison field-service `
  --actual local/issue534/field-service-native-01/actual.json `
  --field-context local/issue534/field-service-native-01/context-v2.json `
  --text-source-root $pinnedSource `
  --output local/field-service/fresh-report.json
```

Resolve `$pinnedSource` through the private-input owner. The actual descriptor has
`scope=field-service-local-composition-v1` and `fieldServiceCases`; each names its original
profile, JSONL observation, process/launch receipt, tested view source and native error log.

The [module route](../../docs/operations/bounded-inspection-and-review.md#h4-field-service-module-route)
separates case admission, Wait ownership, source service evolution and Draw consumption.
Direct observations may call `sf2tool.remake_h4.field_binding.field_service_binding` with
an explicit fourth `read_document` argument, or use the existing three-argument wrapper.
`field_evidence.case_input(case, read_document)` has the same bounded path/error behavior as
`_field_case_input`; document readers remain caller-owned. Relative evidence paths resolve
from the repository root, including when invoked from `remake/`. Compare complete reports,
check order, coverage and Unknowns when changing these boundaries; preserve the final receipt,
clock and Wait-release negative controls below.

The independent context declares compact `profiles`, selected `sessions`, `assemblies`,
`nativeBase`, accepted `serviceTrx`, and the existing `seedActual`/`seedContext`.
`historicalSessionId` and `actualSupplement` bind modern `--field-context` applicability;
that route replaces only the field child with this same predicate. The local structural
program observation serializes concrete action types/operands plus `waitingForMotion`;
null programs remain distinct. It adds no engine rule, event stream or clock policy.

The retained cases are portrait-disabled-06, portrait-enabled-01, npc-radius-01,
npc-entity-01, npc-wall-01 and npc-phase-01. Their source/current comparisons and actual
projection checks pass. Release adapter and Debug builds passed for the exact additive
view source:13.347s,796,549,120B aggregate peak,222,624B incremental build storage.
No engine aggregate is required by this observation-only change. Reuse these runs while
the declared dependencies remain unchanged.

Native allocation is serial: driver30s/240 services/768 records plus receipt,2MiB JSONL
with8KiB reserved for its receipt; launcher45s/1.5GiB aggregate process-tree peak;
profile768KiB, logs512KiB and metadata128KiB. Total retained native/profile/log/comparison
allocation24MiB includes failures and4MiB publication headroom; planning has a separate
3MiB cap. Each compact input/output bundle stays below10MiB. Comparison allocation is
120s/128MiB incremental, each case report512KiB; CLI context/report hard limits are1MiB.
The CLI reads one bounded original case at a time and writes a fresh report under this
worktree's ignored `local/`; exit0/1/2 means PASS/FAIL/Unavailable. Its bounded joins may
scan preceding records quadratically in the capped record count; no raw archive is read.

Preserve all completed attempts. The first four portrait pilots failed before service;
the fourth diagnosed `entity-sprite-binding`, falsifying focus/main-loop explanations.
Legal player sprite setup through the existing operation corrected the authored input.
Pilot01's aggregate memory is Unknown; its parent-only sample is not aggregate evidence.
Pilot05 completed Unavailable at the unregistered entry batch; the accepted semantic
composition is a separate narrower claim. The initial successful-capture comparison
failed because JSON serialized integral numbers as floats; normalization on read corrected
it without changing evidence. The first direct control run misclassified a missing
`waitingForMotion` leaf as a later predecessor contradiction. Its corrected join preserves
missing evidence as Unavailable and known contradictions as FAIL. Retain those original
results and targeted corrections. The input-ownership pilot incorrectly equated V wait
with `gameplayHeld`; the adapter uses its separate WaitHeld path. Bind the recorded V
press/ready state and actual wait result, preserving that failed pilot. Do not rerun
passing native cases to replace failures.

Independent review of candidate `3c6a5e94` additionally reproduced three false PASS results:
a foreign terminal case, consistently negative clock axes, and a V release after the first
valid wait followed by illegal repeats. Foreign receipt plus a missing Draw also incorrectly
became Unavailable. Preserve those original controls and rerun their exact four negatives
plus the genuine baseline after correction. Receipt identity/single termination and absolute
clock domains are checked before downstream missing fields. Wait ownership follows press,
release and cancellation edges; the immediate action is distinguished from later repeats,
and duplicate held presses do not rearm a canceled hold. Reuse all successful native/build
observations; these comparator corrections require no new capture or SDK run.

Direct controls cover premature registration, extra RNG, counters/gates/programs,
disabled NPC effects, destination/collision/wait/travel, service count/order/continuation,
projection and draw clocks, consistently foreign session/build identity, missing fields,
missing transition Draw and registered batches. Wrong-plus-missing controls must remain
FAIL, including missing accepted seed proof. Scoped Ruff, design-contract checks,
private/diff scope, the committed planner under the current policy and exact-head CI
complete this tooling slice. Adjacent predicates remain frozen. Historical A FAIL,
original timing Unknown and the completed normal-verification provenance failure remain;
main-gate owns independent integration and obligation closure.

### Scoped AI consumer comparison

Implementation follows the [AI/seed module route](../../docs/operations/bounded-inspection-and-review.md#h4-ai-and-seed-module-route).
Direct consumers import `ai_consumer_binding` from `sf2tool.remake_h4.ai_binding`; direct original-rule
observations import `source_rules`/`source_decision` from `ai_source`, and value observations use
`ai_checks`. The old comparison-module imports remain available, while the CLI below is unchanged.
AI shorter-list matching accepts a retained subsequence as missing evidence; do not substitute the
physical pairwise matcher. Private controls that patched the monolith's source helper should instead
run the actual uncached source path; no mutable forwarding compatibility is provided.

The [selected AI contract](../../docs/design/contracts/map3-battle01-continuous-scenario.md#selected-ai-rule-and-consumer-binding)
uses retained caller/movement evidence and pinned original rule parsers. Run:

```powershell
. ./local/private-inputs.ps1
uv run python -X utf8 -m sf2tool.remake_h4_comparison ai `
  --actual local/issue534/ai-binding-01/actual.json `
  --ai-context local/issue534/ai-binding-01/context.json `
  --text-source-root $pinnedSource `
  --output local/ai-consumers/fresh-report.json
```

Resolve `$pinnedSource` through the local private-input owner. Context declares
`scope=retained-keyboard-A-ai-composed`, session, producer/source revisions, original
`indices`, `battleSample`, semantic `census`, completed selection receipt, retained
`inputs` with independent `inputIndices`, and the accepted `seedActual`/`seedContext`.
`actionRecords` reuses the accepted physical first-strike owning envelopes by original
index/session/clocks. Their compact projections omit the supplementary static AI fields
and input-delivery flag; causal input intervals still bind their result ownership.
Supplement-owned rows require their retained delivery flag. Modern `--ai-context`
replaces only the AI child with this same predicate.

Source-rule comparison uses the actual immediate caller roster, memory, thinking image,
activation/orders/anchors and original terrain/movement tables. It checks every declared
semantic occurrence and preserves missing-versus-wrong evidence. Logical movement is
composed from ordered segment effects and committed position, not an unrecorded render
path. The current seed-lifetime dependency reuses the exact accepted PR621 behavior TRX
and adapter exit through the admission-seed predicate, plus focused producer/accepted/current
consumer comparisons. It authorizes no SDK rerun or corrected route observation.

For extraction review, reuse the compact AI mutations, the retained movement/action/region review
counterexamples and direct seed controls. Compare complete serialized results before/after, including
`historical`, `seedMechanism`, occurrence order, compressed PASS counts, nonpass order and Unknowns.
Begin with one uncached baseline to measure current cost, retain it, then use batches of at most eight
controls under the initial 1MiB report/128MiB incremental-memory/120s budgets. The retained baseline
is for implementation equivalence, not new original-game evidence. Inspect actual import aliases and
selected stream lifetime, and run the real CLI PASS/FAIL/Unavailable exits without a full H4 run.

Reproduction reads the retained compact supplement; this recipe authorizes no raw scan.
The initial selection completed with a cap FAIL after12.9697317s,7,696,384B incremental
peak and4,060,849B payload: an admission marker incorrectly enabled the entire
pre-initialization actor bridge. Preserve that failure, its partial records and the
already retained text-copy interval. The one authorized correction starts bridges at
initialized/loaded/outcome/returned seams and retains all AI/movement/failure owning
envelopes and neighbors. It completed in17.5820325s with16,936,960B incremental peak,
3,552,557B selected payload, two samples and459 results; normalized input is3,536,920B.
Source metadata stayed unchanged. No further raw pass is allocated.

Selection limits are120s/128MiB incremental and2MiB per raw record, selected5MiB
including128KiB receipt reserve, normalized5MiB, context/report1MiB and3MiB publication
headroom; total retained AI directory cap20MiB includes the prior failure/preflight.
Each serial input/output bundle stays within10MiB. The scoped CLI enforces actual plus
context10MiB, context1MiB, report1MiB and a fresh output beneath this worktree's ignored
`local/`. Exit0/1/2 means PASS/FAIL/Unavailable; `milestonePass` remains false.

Use direct controls for source RNG/lane preservation, memory before/after/exit, commands,
candidate order and target delivery, missing records/fields/inputs, source/session identity,
clocks, movement segments/commit, terrain, seed-execution evidence and wrong-plus-missing
combinations. Preserve completed pilot failures alongside corrections: the initial
target-delivery check assumed a target on `scene-prepared`, the next applied supplement
field requirements to compact physical rows, and a later check used the wrong terrain
surface mapping. Also retain the direct controls' missing-target propagation and absent
seed-context classification failures, and the two controls corrected to mutate automatic
movement and the physical-first target. Targeted reruns check each correction. Initial
memory/seed last-writer checks extend through selected gaps. Missing clocks/action indices
remain Unavailable; negative clocks and known wrong effects still fail.
Independent review additionally requires rejected automatic-arrival and physical-first
owning results to fail, including the rejected-arrival case with seed proof missing.
Preserve those original false-PASS/Unavailable results and the wrong delivered-regions
counterexample beside their targeted corrections. Required movement/action/commit results
must be accepted. Ordered region-test writes bind the actual poststate and later caller
continuity, including the observed clear0 followed by region-test7 in one result; unrelated
rejected-input diagnostics are not reclassified as AI failures.
No verifier unit tests or aggregate/native/full-route runs are required.
Run narrow Ruff, contract/docs/private checks, the committed planner interpreted under
current verification policy, and exact-head CI. Historical A FAIL, original elapsed
Unknown and completed normal-verification provenance failure remain unchanged. Independent
main-gate review owns integration and obligation closure.

### Scoped HEAL consumer comparison

Direct observations use `sf2tool.remake_h4.heal_binding.heal_consumer_binding` and
`sf2tool.remake_h4.heal_source.fairy_source_step`; existing imports/CLI remain available. The
[HEAL module route](../../docs/operations/bounded-inspection-and-review.md#h4-heal-module-route)
identifies source, resource, opportunity and work owners. For extraction review, reuse a retained
complete baseline after equality, compare full reports in bounded batches, and preserve the constants
following the old HEAL function. Check numeric spell-level boolean counterexamples separately from
movement, including integer/float equivalence, expected booleans, explicit `None` and mixed missing
plus contradiction. Complete reports must retain check order, historical failures and Unknowns.

The [selected HEAL contract](../../docs/design/contracts/map3-battle01-continuous-scenario.md#selected-heal-rule-and-consumer-binding)
reuses the accepted HEAL rules and PR618 logical recovery proof. Use retained actual effects and
live fairy observations; expected values never come from a remake-generated result or a later
bracketing actor snapshot. Original prepared04 interrupt/cursor failures remain explicit diagnostics.

```powershell
. ./local/private-inputs.ps1
uv run python -m sf2tool.remake_h4_comparison heal `
  --actual $selectedHealActual --heal-context $acceptedHealContext `
  --text-source-root $pinnedSource --output local/heal-consumers/fresh-report.json
```

The selected actual object retains `warpRecords`, `sceneObservations` and `inputRecords` with their
zero-based original `_index`. Keep result envelopes and complete events, minimal live caster/target
stats, and scene phase/token/spell/message/healing fields. Every logical opportunity, including
no-draw work, needs its actual state. Bind host-poll scene observations after Present; preserve
signal-before-Present observations as a distinct category. A Submit's additional enemy/AI events
must retain their separate identity and cannot contribute to the HEAL proof.

Independent context has `scope="retained-keyboard-A-heal"`, `sessionId`, original `warpIndices`,
`sceneIndices` and `inputIndices`, and three `occurrences`. Each occurrence retains actor/target,
source target sprite, before/prepared/terminal result revisions, scene start/end sequences,
physical confirmation ordinal/index/result index and the complete selected result-index inventory.
Keep context separate from candidate data so deleting a candidate cannot redefine coverage.
Full-capture channels use these indices directly; the predicate does not scan or materialize them.
Modern `compare --heal-context` calls this identical child; do not run full H4 just to prove wiring.

The authorized one-pass selection budget is120s/128MiB incremental memory, at most10MiB newly
selected output and20MiB combined compact inputs. Read-only source size/mtime must agree before
and after; record timing, peak memory and output size. Preserve partial output on budget failure
and resolve the precise remaining boundary before another scan. The scoped command limits its
combined input to20MiB, context to1MiB and report to10MiB, with a fresh worktree-local output.
Exit0/1/2 means PASS/FAIL/Unavailable; every report keeps `milestonePass=false`.

Direct controls cover wrong recovery/cost/reward, source caps, draw range/result and fairy state,
missing draws/results/projections, duplicate and foreign identities, caller/order errors, physical
acknowledgement and mixed missing-plus-known-wrong evidence. Resource controls include reordered
MP/HP/EXP effects with coherent live values, premature or reverted resources between commands,
preparation draw envelopes, missing phase markers and the mixed terminal Submit boundary. Phase
identity and source command order determine effect timing; historical revision numbers do not.
Controller cleanup controls verify no update/draw occurs before a direct cleanup dispatch.
Preserve completed failures; correct
them with affected controls. Use scoped Ruff, direct contracts/docs/private checks, committed planner
and actual CI. No verifier unit tests, SDK/native/H3 acquisition, new route, full H4 or matrix are
implied. Accepted audio, W2 and turn predicates keep their own boundaries.

### Scoped W1 consumer comparison

The [selected W1 contract](../../docs/design/contracts/map3-battle01-continuous-scenario.md#selected-w1-consumer-binding)
uses the existing compact retained-A selection, original outcome records69–74, its final copy witness,
and the bounded map3/NPC-history supplement. No SDK, host, emulator, full H4, matrix or repeated raw
capture scan is needed.

The [module route](../../docs/operations/bounded-inspection-and-review.md#h4-w1-module-route)
names the selected input, source and service owners. Direct observations can import
`w1_consumer_binding` from `sf2tool.remake_h4.w1_binding`; the signature remains
`(actual, context, source_root)` and the old import is the same callable. Full-capture containers
use indexed selection without timeline iteration. Already selected streams belong to the caller,
and the complete report remains readable after they close. Relative source/input paths resolve
from the repository root, including CLI invocation from `remake/`. Compare full ordered checks,
poll occurrences and Unknowns, retaining the Submit/event, stationary NPC and mixed service
counterexamples below.

```powershell
. ./local/private-inputs.ps1
uv run python -X utf8 -m sf2tool.remake_h4_comparison w1 `
  --actual $selectedW1Actual --w1-context $independentW1Context `
  --text-source-root $pinnedSource --output local/w1-consumers/fresh-report.json
```

Candidate `samples`, `inputRecords` and `warpRecords` retain original `_index` values and complete
selected observations. Keep the independent context separate:

- `scope=retained-keyboard-A-w1`, `sessionId`, and ordered `occurrences` identify each source
  `cursor`, `text`, control-token `position` and live `token`.
- `polls` retain `ordinal`, `token`, `accepting`, original `inputIndex`/`resultIndex`,
  `beforeRevision`/`resultRevision`/`afterRevision`, ready/after references (`channel`, `index`),
  and the independently inventoried `entityIds`. A physical delivery can include another Submit.
- `indices` select the original sample/input/result positions. `outcomeIndices` identify actual
  outcome records as their own channel, never as sample indices. `copyWitnesses` retain each later
  witness's channel, index where applicable, and exact session/revision/sequence/map/cursor/wait/token.
- `npcHistoryIndices` and `npcPrograms` select the complete bounded installation/replacement history
  and registered historical source programs. They carry identities, not expected RNG results.
- `actualSupplement` may hold the retained `outcomeRecords`, `w1OutcomeFinal`, `w1NpcHistory` and
  `w1NpcWorld` observations. These are actual evidence, separate from expected source rules. Scoped
  candidates may instead carry these four fields directly; duplicate copies must agree. The world
  supplement contains only provenance, map3 and the selected28 source programs, with no media assets.

The current compact input is about6.3MiB. The command enforces10MiB combined input,1MiB context and
10MiB report limits, plus a fresh worktree-local ignored output. Budget120s and128MiB incremental
memory. Work is bounded by the selected rows, entity population and fixed source programs; full
capture arrays support indexed reads and must not be iterated to reconstruct this cohort. Preserve
selection receipts, failed attempts and original metadata checks. Any additional large selection
needs its owning scope/budget decision; a missing field is not permission to scan again.

Exit0/1/2 means PASS/FAIL/Unavailable. Every report retains `milestonePass=false`. Modern
`compare --w1-context` invokes the same child, using the compact actual supplement alongside its
indexed full capture. Verify this wiring directly; do not launch full comparison to test it.

Direct controls should cover missing/duplicate polls, wrong source token or producer, foreign
session and result identity, release/reveal misuse, missing plus contradictory evidence, event order,
copy retention, conditional portrait range/seed/registration, NPC installation/collision/destination,
and the separate choice-window Submit. Include result/state sequence and revision disagreements,
negative or overlapping extra-event envelopes, orphan W1 operations on reveal/delivery rows, and
stationary NPC geometry/travel contradictions beside missing operands. The strengthened movement
counterexample changes both destination and carried travel while retaining `moving=false`; a changed
target alone with zero travel is not evidence of coordinate movement. Preserve a completed failed
control, then rerun its correction
and any newly affected checks. No unit-test suite of the comparator is required. Exact original live
service timing, removed intermediate clocks/typewriting, wider branch-flag truth, historical HEAL
failures and whole-A CPU/AI/battle-scene boundaries remain explicit in the contract and handoff.

## Scoped W2 consumer comparison

Use the retained `current-A-w2.jsonl`, `current-A-w2-tail.jsonl`, `w2-occurrences.json` and selected A
audio receipts described by [the accepted policy](../../docs/design/contracts/dialogue-system.md#w2-composed-semantic-acceptance).
The `w2` mode evaluates only the
[composed W2 child](../../docs/design/contracts/map3-battle01-continuous-scenario.md#composed-w2-consumer-binding);
no SDK, native host, original capture, whole-A scan, full H4 or matrix is needed.

The [W2 module route](../../docs/operations/bounded-inspection-and-review.md#h4-w2-module-route)
names the source, selection and comparison owners. Direct observations may import
`w2_consumer_binding` from `sf2tool.remake_h4.w2_binding`; the old import is the same callable
with signature `(actual, context, source_root)`. Selected streams remain caller-owned and reports
remain readable after they close. Compare complete ordered checks, occurrence identities and
Unknowns, including numeric/boolean, missing and mixed evidence. Relative input/source paths
resolve from the repository root, including invocation from `remake/`.

```powershell
. ./local/private-inputs.ps1
uv run python -m sf2tool.remake_h4_comparison w2 `
  --actual $selectedW2Actual --w2-context $acceptedW2Context `
  --text-source-root $pinnedSource --output local/w2-consumers/fresh-report.json
```

Normalize only the existing small selections into `samples`, `inputRecords` and `warpRecords` arrays:
each selected row retains its payload and `_index` equal to its original `index`, not its JSONL line
number. Append the four tail samples and retain the existing `audioReceipts` wrappers with their poll
context. Do not copy the world, PCM or full timeline. Preserve the original selection receipts locally.

Keep the independently accepted context separate from candidate data. It has `scope` equal to
`retained-keyboard-A-w2`, the selected `sessionId`, and these two inventories:

- `occurrences`: the existing16 accepted rows' `ordinal`, `token`, `program`, `instruction`, `text`,
  `resultRevision`, `nextToken`, `nextWait`, `afterSample` and `validationSequences`. Add original
  `inputIndex`, `resultIndex`, `readyIndices` and `indicatorIndex` from the retained locators. Keep
  `indicatorIdentity` with the witness's revision, observationSequence, token, cursor and wait;
  this is occurrence identity, not expected indicator behavior. The predicate derives clear/hidden,
  draw/copy, caller/service and token-span requirements from the accepted source policy.
- `neutral`: the14 retained neutral rows' `ordinal`, `token`, `resultRevision`, `inputIndex`,
  `resultIndex` and `readyIndices`. Text2292's accepting `readyIndices` refer to its pre-neutral
  full state; the actual neutral input/result edge must bridge to the accepting before state.

The current prepared selection is about2MiB plus a small context. The command enforces10MiB combined
input and10MiB report limits, and a fresh output beneath this checkout's ignored `local/`. Plan120s
and128MiB incremental memory; retain measured runtime and peak working set with each control run.
Input/result/sample/receipt indexes bound matching to this fixed cohort; full-capture
access uses original array positions. Do not reuse this scope as permission to import a larger route.
Exit0/1/2 means PASS/FAIL/Unavailable; every report sets `milestonePass=false` and keeps the original
internal-read and two intermediate-draw Unknowns explicit. The existing modern `compare` mode accepts
`--w2-context` for the identical child predicate; do not launch full comparison just to prove wiring.

Direct controls cover occurrence deletion/duplication, foreign session/token/result/audio, crossed
whole Submit revisions, wrong validation/caller/service/indicator/copy, premature caller resume and
missing plus known contradictory evidence. Check all validation starts for each accepting Submit,
event revision range/order, and source-derived resumed producer/instruction/operation order and
terminal cursor. Include missing events, an extra validation start, an out-of-range event revision,
a foreign resumed program, and the genuine source jumps and nonterminal-text continuations.
The existing `OriginalPrograms` compiler reads pinned, clean source for a bounded continuation;
unhandled source call/return/branch paths stay Unknown rather than forcing all events into one program.
A contradiction dominates missing; a missing witness is
not converted into a fabricated negative. Two575 labels are not unique keys, and the final two later
indicator states must not become same-submit observations. Preserve failed reports and distinguish repeated projections of one revision from repeated
occurrences. A resumed text's typewriting is not the transient restored W2 value. Existing PR618
behavior results are reused, not rerun. Use scoped Ruff, direct contract/document checks, committed planner and actual CI;
no tests of this verifier or broad legacy suites. Audio and turn-order predicates remain unchanged.

### Scoped turn-order rule comparison

The completed generator exposes its actual ordered candidates, Roll results and unsorted/sorted
buffers through `SessionObservation.TurnGeneration` on the existing `round-rng` event. These facts
come from the one generation that updates the battle queue. `BattleTurnFlow` passes the ephemeral
result to Application; snapshots store no generation history. Existing activation/spawn admission,
seed carry and event identities are preserved. Candidate/draw storage is O(roster size + draws);
64 slots permit at most 192 draws, and each buffer has 64 entries. The completed event copies those
immutable values; it does not replay arithmetic or create another battle-state authority.

The legal local start is the authored `practice-yard` scenario, reached through the ordinary
`AuthoredScenarioPackageReader` and `GameSession.Start`. Enable only medic-a's extraRoundAction and
use Confirm → ChooseAction(Stay) → Confirm until the third round. This is the same normal flow
covered by `BattleAgilityTurnsTests.EqualAgilityDefinitionsConsumeExtraEntryAndCarrySeedsAcrossRounds`.
Record each result's round-rng event with its independently read snapshot, before any later action
mutation: sessionId/revision/observationSequence/round/mainSeed, complete live candidates as
`BattleTurnCandidate` and the whole `turnOrder` as `BattleTurnSlot`. The compact input has
`evidenceScope="controlled-application"`, `sessionId`, independent `selectedRounds` and `rounds`
rows containing `event` / `state`. Generation payload fields retain their public DTO names;
state envelope fields use the names above. No Godot, original emulator or private route is needed.

```powershell
. ./local/private-inputs.ps1
uv run python -m sf2tool.remake_h4_comparison turn-order `
  --actual $selectedGenerations --text-source-root $pinnedSource `
  --output local/turn-order/fresh-report.json
```

Without a turn context, the command accepts at most three selected generations and 1 MiB input, enforces a fresh output
under this checkout's local/, and caps the report at 1 MiB. It reports a controlled comparison scope
and `milestonePass=false`. These limits scope the pilot, not gameplay legality. The ordinary H4
child keeps its missing actual generation/state boundary; a controlled report cannot close it.
The [contract](../../docs/design/contracts/map3-battle01-continuous-scenario.md#reached-turn-order-rule-and-result-binding)
owns this distinction and the retained A limitations.

Independent expectations use pinned original candidate eligibility, score construction, H3 word
RNG and the full stable signed sort; modern arithmetic does not create expected values. Candidate
and draw joins retain coverage/order even when an occurrence is omitted. Direct controls include
missing whole generations, live agility, candidates or draws; foreign session/round/candidate;
duplicates and reordered draws; wrong range/value/score/seed/extra-turn; and an internally agreeing
wrong queue. A known contradiction remains FAIL beside missing operands/occurrences. Read the
accepted boundary fixture directly to check negative scores, sentinel participation and ties.
Do not add verifier tests or manufacture actual fields from the source expectation.

The completed three-round Application pilot produced 23,210 bytes in 0.088 seconds after build;
its source-rule binding PASS is separately scoped. Selection limits were 60 seconds / 1 MiB output,
with no native launch. Narrow SDK commands use the existing protected selections and worktree-local
outputs, estimated 15 minutes / 3 GiB memory / 2 GiB generated output per command. Memory is an
estimate, not a measured peak. The changed shared observation DTO requires adapter compilation,
not a host startup. Actual engine checks select `TurnOrderRulesTests` and `BattleAgilityTurnsTests`;
rerun a completed failing node narrowly. Use scoped Ruff, document checks, the committed plan and
actual CI; no normal/full suite, complete H4 comparison, full route or settings matrix follows.

#### Composed turn rule and consumer verification

The [turn module route](../../docs/operations/bounded-inspection-and-review.md#h4-turn-module-route)
names the source, generation, dependency and consumer owners. Direct observations may import
`turn_order_binding` from `sf2tool.remake_h4.turn_generation` and `turn_order_consumer_binding`
from `sf2tool.remake_h4.turn_consumer`; old imports remain the same callables. Preserve full ordered
reports, grouped counts/examples, generation rounds, frontier and terminal fields when comparing.
Selected streams remain caller-owned and detached reports survive closure. Relative paths resolve
from the repository root, including invocation from `remake/`.

The explicit [composed boundary](../../docs/design/contracts/map3-battle01-continuous-scenario.md#composed-current-turn-rule-and-queue-consumption)
uses the same scoped mode with a selected actual consumer input and independent context:

```powershell
. ./local/private-inputs.ps1
uv run python -X utf8 -m sf2tool.remake_h4_comparison turn-order `
  --actual $selectedTurnConsumers --turn-context $turnContext `
  --text-source-root $pinnedSource --output local/turn-order/fresh-composed-report.json
```

The combined actual/context bundle is limited to 10 MiB; the fresh H-local report is limited to
1 MiB including its serialized formatting. Comparison budgets remain 120 seconds / 128 MiB
incremental. The result has separate generation and consumer legs, `historicalDiagnostic=Unavailable`
and `milestonePass=false`. No raw capture, world/reference export, SDK/native/emulator/observer
change, route, full H4 or matrix is needed.

The context declares `scope=retained-keyboard-A-turn-composed`, original session/producer/source
revision, accepted `generationCommit` and genuine `generationActual`, actual retained `queues`
with source channel/index/one-based ordinal/round, independent `indices` and semantic `census`,
completed `selectionReceipts` and `roundSelection`, source `factions`, independent
`owningResultIndices`, `callerFields`/`inputOrdinals`, and available `stateFields`/`inputFields`.
It references the PR622 executed generation proof,
the prior fourteen-record/twelve-generation queue inventory, and the accepted compact AI/reward
selections. Preparation selects only needed fields and reconciles overlapping result/state
identities. Keep the scene inputs with null actors as well as manual-control inputs: they carry
the ordinal and dispatch boundaries of automatic/scene results. Do not copy whole archives or
reconstruct missing old generation from expected math.

Selected actual channels use `evidenceScope=selected-turn-consumers` and explicit zero-based
`_index` values. Modern supplied channels use their original channel indices at the same applicable
seam. Both must provide their own installed queues, owning result acceptance, ordered events,
live HP/cursor/selection states and causal inputs. Missing supplied evidence stays Unavailable;
retained dependency records cannot fill it. The frontier is generated from the independent census
and queues, with each missing witness retained as missing. Actual HP writes/death cleanup, queue
admission and terminal enemy HP justify live action, dead skip and terminal remainder. The `heal`
event is a named effect, not a caster HP write; actual `hp` observations govern HP propagation.
No-event rejected inputs remain diagnostics and cannot advance the queue. Passing checks are
grouped with counts/bounded identity examples; every failed/missing class remains in the verdict.
Validate both clock domains before missing-leg exits, ordered event clocks/result bounds and the
exact delivered poststate session/revision/sequence. The initial queue sample joins its first-control
result. Only an empty attach projection with matching preceding result/events uses the observer's
before-view exception. Input intervals are nonnegative integral source indices, ordered and
nonoverlapping; before/after clocks progress, join supplied exact result boundaries and are bounded
by selected neighbors. Direct/automatic results require their actual causal input ordinal and
appropriate before/after/next-input bounds. Missing operands remain Unavailable; independently
known negative/fractional clocks, wrong identities or inverted/contradicted intervals remain FAIL.
Omitted snapshot HP/placement leaves preserve carried knowledge while reporting missing observation;
missing authoritative HP writes invalidate it until a later actual observation supplies the value.

The seven tested/current source guards distinguish a byte difference from a failed read of the
tested Git object or current file. Each unavailable check names its dependency and records the
reason/error type in its bounded examples. Byte equality remains mandatory for proof reuse; a
source-only difference requires renewed review and yields Unavailable, without claiming actual
behavior failed. Wrong producer/source/tested identities or forged receipts remain FAIL, and
consumer contradictions are still evaluated beside an unavailable dependency.

For a direct complete-predicate control, select the retained
`consumer-clock-correction-01/actual.json` and `context.json` as `$selectedTurnConsumers` and
`$turnContext` through their owning environment, and select `$pinnedSource`. After same-process
private-input setup, this substitutes only one dependency read in memory. It does not edit a
protected production file, Git object or retained input. Expected results are `true`, `null`,
`null`, `false`; the last result retains both the unavailable dependency and clock contradiction.

```powershell
$dependencyControl = @'
import copy, json, sys
from pathlib import Path
from types import SimpleNamespace
from unittest.mock import patch
import sf2tool.remake_h4_comparison as m
from sf2tool.remake_h4 import turn_dependencies as dependencies
a0 = json.loads(Path(sys.argv[1]).read_bytes())
c0 = json.loads(Path(sys.argv[2]).read_bytes())
dependency = 'remake/src/Sf2.Remake.Domain/Battles/Rules/TurnOrderRules.cs'
original = dependencies.repo_path
for mode in ('baseline', 'changed', 'missing', 'changed-plus-clock'):
    a, c = copy.deepcopy(a0), copy.deepcopy(c0)
    if mode == 'changed-plus-clock':
        row = next(r for r in a['warpRecords'] if r['_index'] == 16477)
        row['result']['revision'] = row['state']['revision'] = -1
    def read_dependency():
        if mode == 'missing': raise FileNotFoundError('direct missing dependency')
        return original(dependency).read_bytes() + b'// direct changed-read control\n'
    def selected_path(path='.'):
        if str(path) == dependency and mode != 'baseline':
            return SimpleNamespace(read_bytes=read_dependency)
        return original(path)
    with patch.object(dependencies, 'repo_path', side_effect=selected_path):
        r = m.turn_order_consumer_binding(a, c, sys.argv[3])
    print(json.dumps(dict(case=mode, value=r['value'], nonpass=[
        x for x in r['checks'] if x['value'] is not True][:8])))
'@
uv run --locked python -X utf8 -c $dependencyControl $selectedTurnConsumers $turnContext $pinnedSource
```

Reuse the existing standalone generation input and retained consumer clock/input/HP controls with
fresh output names. Direct changed/missing controls should cover all seven guards, failed tested
reads and mixed queue/HP/input/identity/selection contradictions as well. Preserve both diagnostics;
do not short-circuit consumer evaluation or turn an unexpected non-I/O exception into Unavailable.
No helper-test suite, production-source mutation or new environment is needed for these controls.

Use direct genuine, omission, contradictory, foreign, duplicate/order and complete-frontier controls:
missing commit/skip/rollover/terminal, shortened census, failed owning consumer, wrong consumer
beside a missing controlled draw, terminal remainder, changed or absent supplied queue/state/input,
and Cancel/wait cursor persistence. Add no verifier tests. Use scoped Ruff/range formatting,
affected documents/contracts/private checks, the clean committed planner under current scope and
actual CI. Reuse completed engine/adapter checks and all failures; no local SDK or normal/full
aggregate rerun follows this comparator/document change. The independent main gate accepts the
semantic child and retains full-route/H4/milestone decisions.

### Scoped admission seed comparison

The unchanged seed composition now lives in `sf2tool.remake_h4.admission_seed`; direct observations
import `admission_seed_binding` there. The old import remains the same callable for field-service,
modern and scoped CLI consumers. Its matching and historical/current proof roles stay independent
from AI and physical matching; the [module route](../../docs/operations/bounded-inspection-and-review.md#h4-ai-and-seed-module-route)
records that dependency boundary. Extracting this dependency does not change opening controls.

Load the current ignored private-input configuration in the invoking process. The scoped command
uses existing compact W2 actual data and an explicitly selected original/accepted-mechanism context:

```powershell
uv run python -X utf8 -m sf2tool.remake_h4_comparison admission-seed `
  --actual local/issue534/w2-binding-01/actual.json `
  --admission-context local/issue534/admission-binding-01/seed-context.json `
  --text-source-root $pinnedSource `
  --output local/issue534/admission-binding-01/seed-review.json
```

`$pinnedSource` is an explicitly selected read-only pinned source checkout. Output must be fresh
under the current worktree's `local/`; selected actual/context input is capped at10MiB. The same
predicate is used by modern comparison's optional `--admission-context`. A seed-only context replaces
only the seed-copy child. An explicit `opening` selection also evaluates the separate opening-control
child described below. Neither command grants full H4 acceptance.

The private `select-seed.py` selects original checkpoint lines201/204/310/312/4011/4124 from
registered `issue496/prepared-72`, retaining scalar facts, flag88, order and original line numbers.
It reads6,924,228B and emits a compact context. Context includes the existing candidate identities,
the historical A session, PR621 merge identity, retained correction TRX and adapter exit receipt.
Use the [contract](../../docs/design/contracts/map3-battle01-continuous-scenario.md#admission-seed-copy-composition)
for their distinct proof roles. Do not regenerate a corrected trace from test assertions.

Direct controls exercise the real predicate: baseline; missing original loop/copy/behavior receipt;
flag88 resume; original reader before write; foreign actual session/original cohort/correction;
wrong source caller; changed copy; reversed draw/copy; and known wrong copy or flag88 beside a
missing operand. Also check original frame/order agreement, both event axes, physical result-span
ownership and input-after/result/state joins. Reverse revisions without reordering the array,
use a negative/duplicate sequence, or contradict two ending channels while omitting the third.
Contradictions must remain FAIL beside missing evidence. These are direct
verification runs, not a new test suite of the comparator. Preserve completed PR621 failures and
narrow corrections; no SDK/native/whole-route rerun follows merely from composing this evidence.

#### Selected opening controls

The [opening module route](../../docs/operations/bounded-inspection-and-review.md#h4-opening-admission-module-route)
separates original identity/restoration, producer epochs, scalar readback, terminal joins and actual
admission/input/settings. `admission_opening_binding` remains the same callable at its old import;
the scoped admission-seed and modern callers retain the separate seed child. Preserve complete
ordered reports when changing these owners. Verify both children and the combined exit precedence,
including a missing child beside a failed child, from repository and remake working directories.
Caller-owned selected streams stay open and returned reports survive their closure.

Complete-predicate controls distinguish boolean substitutes from numeric opening operands, preserve
integer/float equivalence and strict expected booleans, and retain None/mixed-evidence behavior.
Keep original and corrected reports separate for a demonstrated matching defect. These are direct
observations; no verification-helper unit suite, original acquisition or installation audit rerun
follows from a module change.

The admitted original start139 is retained under `admission-binding-01/opening-prepared-02`.
Its source/configuration/runner/observer/input identities,11 records, completed status and restoration
are owned by the [original readback](../../docs/research/map3-messenger-acceptance.md#controlled-opening-scalar-readback).
Do not rerun that successful observation. `correction-01/audit-opening.py` checks retained results and
the450 registered installation files without launching the emulator. It preserves the original
preparation-budget failure and the native receipt's absent exact monotonic duration.

The separately authorized one-pass historical A selector is
`correction-01/select-opening-actual.py`, with its plan and receipt in the same private owner.
It streamed1,005,792,097B of the registered1,375,851,198B input, stopped each channel at the opening
boundary, and wrote49,056B of selected actual data in9.1947s/1,642,496B incremental peak. Source
size/mtime remained unchanged. It recovered initial revision4 settings, first-input/result joins,
and selected opening results; sparse results genuinely omit some per-glyph/view operands.
Do not scan again to fill absent producer fields or substitute later settings for admission.

`compose-opening.py` merges these selected fields with the existing seed evidence, rejecting
contradictory overlaps. Actual/context total2,107,507B remains below10MiB. Reproduce the scoped
comparison using `correction-01/composed-actual.json` and `composed-context.json` as the two inputs
to `admission-seed`, with a fresh output. Its seed and opening results are separate, and an explicitly
selected opening FAIL/Unavailable produces a nonzero exit. Any selected child FAIL takes precedence
over another child's Unavailable result in the command's combined verdict and exit. Modern comparison uses the same function
only for the opening child; no accepted service/map/audio/HEAL/turn predicate changes.

Direct controls cover baseline, missing original/actual/context/settings, wrong mouth/view/input,
foreign cohort/session, original order/frame/input-axis inversions, first input span and before/after
joins, result/event axes, original exit/restoration failure, and known contradictions beside unrelated
missing evidence. The opening child binds admission values and source-side readers; it does not
claim a distinct historical first-glyph read event, source hardware cadence or full service replay.
Run scoped lint/contracts and the committed planner under current scope, then record exact-head CI.
No SDK/full H4/route/cleanup or repeat native observation follows from these changes.

The complete compact opening context also retains the existing host `reviewedMaterial` and
`runtimeIdentities`; these join the frozen candidate without recollection. Validate the observer's
kind/input/completion/terminal, every required named restoration flag, count/order/seen-set agreement
and the observer/emulator frame equations relative to the R1 input epoch. Keep `outputRemoved=false`
as normal evidence retention. Direct controls include failed completion with missing R1, foreign
observed input, failed `gameFlags` restoration and a view frame inconsistent with its input/emulator
epoch. Preserve the pre-correction context and independent failing counterexamples; never rerun the
successful native attempt to repair a comparator omission.

### Historical A turn-order inventory

The retained required-keyboard A inventory remains separate from the controlled proof above.
Its missing actual operands keep the required child Unavailable; do not repeat the completed scan
or relabel historical A as execution of the current seed-copy correction.

For the retained A producer `4d1d1b05f143ed872ceca6ff258cfca2b4087d90`, stream the pretty JSON
record arrays without loading the whole capture. Select only `warpRecords` whose
`result.observations.Kind` is `round-started` or `round-rng`, preserving the containing session/result
and source channel/ordinal; retain the `bound-first-battle-input` sample. Inspect the small outcome
companion's action-state actor keys and queue separately. Read the process/settings receipts to
identify the compiled source and A scope. Preserve source size/mtime before/after, selection recipe,
counts, output bytes, elapsed time and incremental peak memory under the owning worktree's ignored
`local/`; raw inputs remain read-only. Equal repeated result snapshots are retained as snapshots,
not counted as new rounds or candidate draws.

The completed pilot scanned the retained 1,375,851,198-byte A capture and selected 14 records with
12 distinct round generations, including the repeated first-round result. Output was 326,233 bytes,
elapsed 34.639 seconds and incremental peak working set 3,538,944 bytes. Its selection ceilings were
120 seconds, 128 MiB incremental memory and 10 MiB retained output. Streaming reads O(input bytes);
this pilot's repeated selected-size accounting additionally costs O(record count × selected bytes).
Memory is bounded by the current record and selected records. The 4,814,568-byte outcome companion
has 76 records and can be inspected separately within the same output budget. These measurements
are evidence inventory, not acceptance counts. No database import, source acquisition, native/SDK
launch or comparison report was generated.

The projection in that exact producer's `BattleMapViewport.ObserveActors` lacks live agility,
extra-turn and processing-order operands; its `BattleAdvancer.Advance` publishes only the whole-round
seed edge. Neither outcome snapshots nor original H3 fixtures recover the missing actual draws.
The new completed-generation payload proves current controlled execution only. Keep historical
aggregate results and the original 12 open obligations intact; no corrected whole-route result or
required-A child closure is claimed.

### Scoped audio consumer comparison

Use the complete retained default-keyboard A audio channel and only its necessary scene/music/input
dependencies. `audio` evaluates the existing replacement/fade/stop/resume child and always reports
`milestonePass=false`; it does not load the reference, outcomes, resources or settings matrix. The
[contract](../../docs/design/contracts/map3-battle01-continuous-scenario.md#composed-reached-audio-consumer-binding)
owns the composed proof and remaining Unknowns. No route, emulator, native host or SDK build is needed.

The [audio module route](../../docs/operations/bounded-inspection-and-review.md#h4-audio-module-route)
owns source classification, selected identity, playback lifetimes, scene/finite releases and Confirm
joins. The old three-argument entry passes its existing bounded-list factory explicitly; scoped and
modern callers keep the same entry, and `_audio_context` aliases the selection owner. Compare complete
ordered reports, including playback/release anchors. Verify caller-owned selected streams stay open,
all three report lists use the existing factory, and published companion reports reopen after their
original store closes. Exercise actual CLI exits 0/1/2 from repository and remake directories. These
are direct tool observations, not verification-helper unit tests or full H4 acceptance.

```powershell
. ./local/private-inputs.ps1
uv run python -m sf2tool.remake_h4_comparison audio `
  --actual $selectedAudioDependencies --audio-context $selectedAudioContext `
  --text-source-root $pinnedSource --output local/audio-consumers/fresh-report.json
```

`$selectedAudioDependencies` retains h4Variant, complete audioReceipts/gaps/sequenceSeen/terminal,
scene Initialize/End start-to-completed transitions, music/scene result observations with session
context, semantic Confirm/Wait input records and music-plain-input/join-field-return samples.
`$selectedAudioContext` is a selection from that same run's admitted world: original provenance,
sessionId from its reached producer results, audio metadata without pcm16,
audio presentation operands as id/instruction/operation/source, and
their reached program-instruction events. Preserve source path/size/mtime and the selection recipe
locally. The normal modern comparator derives the identical context from its explicit selected world;
other twelve children are unaffected. An absent channel or completion stays Unavailable.

The current selected inputs total about2.6MiB plus a30KiB context; the command enforces a10MiB input
ceiling and a fresh output under this worktree's local/. The bounded readback is under two seconds,
with an estimated128MiB memory ceiling and under1MiB report. Receipt/phase searches are quadratic in
selected event count; these limits authorize the selected1049-receipt inventory, not a scaled run.
Existing source-driver slot parsing is reused; no new registry, PCM copy or capture database is made.

Direct counterexamples exercise receipt gaps, wrong session/PCM/timer/helper/generation, missing
finite finish or actual phase completion, premature release, illegal stopped-slot replacement,
co-loss of a fade and its actual phase while the logical producer survives, and a non-playing terminal
loop. Actual completed phase/action identity must agree with its logical producer. Helper progress
must retain that music cue, including armed/eligible events; foreign work cannot fill its clock.
Plain Confirm before/after session, revision, token, wait, tick and RNG must match the retained
poll/accepted samples. Contradictory records are checked rather than filtered away; missing identity
fields remain Unavailable. Known phase/action, helper/completion cue or Confirm-session contradictions
must still fail when another selected field is missing; retain complete before/after counterexamples
for such mixed-evidence corrections. Missing selections and nonrequired C remain Unavailable. The accepted window-06 controlled tail
is read directly and its reveal/speech/input methods compared with its accepted Git object; historical
C is not relabeled. Use affected lint/format, document checks, committed planner and actual CI.
Do not run a full H4 comparison, route, matrix, normal/full aggregate or verification-helper tests for
this scoped command. Preserve the earlier Herb overlap FAIL, missing81/C6 failure and original JOIN
completion/channel/F0/queue/interleaving Unknowns.

### Offline plain JOIN consumer comparison

The real JOIN entry delegates to the [JOIN/walking module owners](../../docs/operations/bounded-inspection-and-review.md#h4-join-and-walking-module-route).
The complete source map and bounded ordered controls under `local/issue638/join-walking-modules-01`
verify structural equivalence, real CLI/modern dispatch and caller-owned reader/publication lifetime.
The four compact witness files retain their existing seals and byte identity in a worktree-local
selection. Constructed reference operands and actual/content channels prove only the extraction
boundary; they do not replace the retained full comparison below. No raw historical capture/world,
large reference/report, eager recipe or full historical run is required for structural acceptance.

Add `--original-join-evidence-root $selectedWitness` to the complete modern material comparison
above. Select the four-file, byte-identical prepared68 witness explicitly in this worktree's ignored
inputs, retaining `candidate.json` and the three `runtime/` relative filenames named by the
[research owner](../../docs/research/map3-messenger-acceptance.md#winning-lineage-plain-join-witness).
The comparator checks the existing accepted-reference pair pin, candidate material seal, two raw
file seals and original ROM/upstream/observer/runner identity. It reads no embedded foreign paths,
state file or binary and does not invoke the resume validator.

Use the retained same-session actual observation, frozen party/settings, selected world/scene,
complete material selections, baseline and reference. Reports retain original row anchors and actual
sample, input ordinal, playback generation, helper token, receipt sequence and result-record anchors.
The existing plain JOIN child requires all three actual bindings: completion/restart, plain input
and dependent caller return. Bounded audio/caller subsets do not close the full audio or operation
parents. Missing selections/required observations remain Unavailable; sealed-identity drift or an
observed consumer contradiction is FAIL. Original completion remains Unknown.

Run a fresh report/matrix with retained separate B/C/D reports; compare prior gameplay and all
assertions outside this JOIN child/new subsets exactly. Directly check absent/drifted witness and
actual-generation/input/caller contradictions, plus same-input legacy applicability. Use affected
lint/design-contract checks, committed planner and actual CI. No new SDK/native/H3/capture,
extraction/export, broad normal/full suite or verification-helper tests are required for this offline
boundary. Preserve completed historical failures and full H4 Unavailable.

### Offline walking admission comparison

The [walking module route](../../docs/operations/bounded-inspection-and-review.md#h4-join-and-walking-module-route)
preserves the independent source/phase/motion/gate contributions and per-slot exception boundaries.
Its bounded source-derived/constructed controls remain separate from the accepted historical
admission evidence and earlier missing-plus-contradiction corrections described below.

Separate typed-operand controls under `local/issue638/walking-types-01` retain complete before/after
reports for cursor, moving, raw motion, content template and consumed-gate comparisons. Boolean
substitutes for numeric operands and numeric substitutes for moving are contradictions; valid
integer/float equality and existing missing/None contributions remain. Type contradictions enter
before normalization and cannot be hidden by another missing operand. The unchanged JOIN reports
and existing witness seals are checked directly; the completed structural batch is not repeated.

Use the complete modern material comparison and the same explicitly selected, sealed
`--original-join-evidence-root` witness above. Its R1 checkpoint supplies the three live walking
records; the [research owner](../../docs/research/map3-messenger-acceptance.md#walking-admission-continuation)
owns the pinned allocator, 50-byte template, map declarations and movement consumers. The selected
world must retain the matching original identity and compiled walking actions. Modern start inputs
and program names do not supply original expectations.

The existing three cursor/moving children bind R1 pointers, physical/character lookup, wait and
geometry to the selected content and actual admission/sample-zero phase. The existing motion child
compares active axes, destination and velocity direction, carried total travel, remaining distance,
acceleration/deceleration, obstruction, map/entity collision and auto-facing gates. Total travel is
independent of remaining distance. Original velocity magnitudes and hardware-frame equality are
excluded. A later same-session sample inside the source next-wait duration must demonstrate
consumption without a caller reinstall; neither a fixed sample index nor a measured tick count
defines gameplay. The hidden `WaitingForMotion` value remains Inferred; its observed consumption
effects are Confirmed only at this seam. Full awaited/entity motion, gesture and fade families remain
Unavailable.

Run a fresh modern report and matrix with the retained B/C/D reports. Compare gameplay and every
assertion outside these four children exactly, allowing their dependent summaries to change. Direct
readback must cover missing selections/content/phase/gates, contradictory pointer/template/content,
actual phase/motion/gates, and mixed contradictory plus missing observations: known False dominates
missing evidence. Evaluate source, phase, each normalized motion contribution and consumed gate
independently; a missing operand blocks only its dependent contribution. Preserve report completeness and the matrix-only self-check boundary. Use affected
lint/document/contract checks, the committed planner and exact-head CI. This offline readback adds
no capture, SDK/native launch, acquisition, export or verification-helper tests; preserve completed
legacy, JOIN and source results and their failures.

### Complete operation-flow comparison

The existing comparator entry delegates to the [operation-flow module owners](../../docs/operations/bounded-inspection-and-review.md#h4-operation-flow-module-route),
whose source-region map preserves every moved branch, closure bridge, ordered check and early exit.
For this structural boundary, use complete old/new reports from compact source-derived/constructed
controls, the actual wrapper/modern seam and real reader/list/map/sort/dict factories. Observe
per-call/per-invocation isolation, caller-owned streams and detached publication. Source/program
seeds from the older saved motion pilot do not become the corrected A03 world; newly constructed
selection receipts and states have no historical process/channel identity.

The private reproduction owner is `local/issue638/operation-flow-modules-01` (`controls.py`,
`cases.py`, `compare.py`, `extras.py`, `interfaces.py` and the source-region map). Retain completed
reports/failures and use fresh output destinations. Compare every ordered field while retaining
one full baseline plus equality receipts for identical outputs. Keep missing-only, wrong-only and
mixed cases distinct, including explicit-null camera, invocation-bounded callers, rebuild/preserve
allocation with overrides, and independent shared-tail flags/counts with intervening writers.

No new full historical A03 comparison, raw actual/world read, eager legacy script, native/SDK
launch or acquisition is required for this extraction. Historical acceptance stays at PR604 merge
`4d2251316fb19f5e6acfb2a7c023eeabca8df561` and final candidate
`f569549a80411ba73dcb2eb42ee76554c366d3a9`; calls4107/12608 remain Unknown. Use affected Ruff,
design-contract/research-index, document/private/scope checks, committed planning and actual CI.
This scope adds no helper-unit, normal/full aggregate, route/matrix or full-H4 run.

Use the complete modern material comparison above, with `--text-source-root $sourceRoot`,
`--original-join-evidence-root $selectedWitness`, `--baseline-actual $normal05Actual` and
`--baseline-outcome $normal05Outcome`. The explicitly selected read-only source checkout must be
clean for `disasm` at `c834c652b6862bc5679fd7f69a38a7093206efc6`. Relative selections resolve from
the chosen repository root; shared immutable inputs remain outside removable worktrees. All fresh
writable selections, runs and reports belong to this worktree's ignored output root.

For the corrected world, use the existing `OriginalPrograms` compiler on the pinned source. In a
local Python driver launched with `uv run python -X utf8`, take explicit repository, upstream,
old-world and fresh-output paths, and perform these operations:

```python
compiler = OriginalPrograms(
    {"resources": {"standaloneScriptPrograms": [], "initSourcePrograms": []}},
    upstream, scene_maps=[57],
)
compiler.register_file("disasm/data/battles/entries/battle01/cs_afterbattle.asm")
compiler.compile("abcs_battle01")
new = copy.deepcopy(old)
index = next(i for i, p in enumerate(old["world"]["programs"])
             if p["id"] == "abcs-battle01")
row = compiler.programs["abcs_battle01"]
assert row["instructions"][2] == {"op": "camera-entity", "entity": None}
assert row["instructions"][5] == {"op": "wait-ticks", "ticks": 1}
reduced = copy.deepcopy(row)
del reduced["instructions"][5]
del reduced["instructions"][2]
assert reduced == old["world"]["programs"][index]
new["world"]["programs"][index] = row
restored = copy.deepcopy(new)
restored["world"]["programs"][index] = old["world"]["programs"][index]
assert restored == old
```

Import `copy` and `OriginalPrograms` from `sf2tool.remake_exploration_content`; read and write UTF-8
JSON. Before compiling, verify the source HEAD and `git diff --quiet <pin> -- disasm`. Reject an
existing output path, preserve the old input, and read back the fresh file. Keep the original ROM
identity/provenance and every other world/asset/audio selection unchanged. This is a selected-content
correction using the existing compiler, not an engine correction or implicit recipe switch. The
retained preparation recipe/proof is under `local/issue534/operation-flow-preparation-01/`.

The [five whole children](../../docs/design/contracts/map3-battle01-continuous-scenario.md#complete-reached-operation-flow-binding)
require complete source/effect joins. Reuse a successful corrected A for offline corrections. The
bounded adapter observation exposes live session/revision/observation identity and nested
`pendingFieldReturn` logical view/tick/cursor after battle mode changes; the probe checks these
identities against its actual result while field projection remains unavailable. Its unattended
winning scenario requests genuine window focus once per observed loss and records restoration;
application focus guards and ordinary intentional focus-loss scenarios retain their behavior.

Current local evidence is `local/issue534/operation-flow-01/new-A-03`: successful complete corrected
A, exit0, no Godot errors, unchanged frozen party/start/settings, and no focus-recovery request in
this run. The retained process receipt, tested Debug DLLs, C# and probe copies and patches identify
the accepted base plus actual observation delta. Failed native01 (observer errors), native02 (white
wait timeout), null-window01 (real focus loss) and null-window02 (missing post-draw assertion despite
observed focus/service recovery) remain completed failures, not successful continuous evidence.

The bounded direct readback gives four operation-flow children PASS with 2,244 PASS / 17 Unavailable
assertions and no required FAIL. The route/setup/caller child remains Unavailable because calls
at4107 and12608 lack retained in-call stack operands. Fifteen other children and the matrix
obligation remain open in that retained report; its full H4 verdict remains Unavailable. Historical matrix evidence retains its own
content and observation boundary.
Nine normal05 pre-AB checkpoints and Left remain exact. Gameplay equivalence changes only its raw
inputs/observations components: six post-AB input locations carry the two source PC additions,
and actual service/effect events remain unnormalized. Other equivalence components remain exact.
The retained raw-delta proof identifies exactly two new instruction events and their real simulation
service; a source-PC readback recovers the old projection only for diagnostic comparison, while
reports preserve the new raw stream. Forty RNG draw labels retain their source kind and values with
three later observation ordinals. Sixteen audio receipt labels have actual host receipt identities;
their material, assertion values and results remain exact, with no uniform receipt-offset rule.
The corrected A-only current cohort is Unavailable; combining it with old B/C/D is a failing mixed
content diagnostic, not a same-content matrix. Preserve both reports and historical matrix.

Acceptance uses direct whole-flow comparison; wrong/missing branch, caller, choice, roster/write,
destination/setup/latch, load service, camera and enclosing-return cases, including mixed missing
and contradictory evidence; closed mandatory inventory and corruption checks; exact unaffected
assertions and baselines; affected Ruff/docs/private-boundary checks; actual adapter build; committed
dependency plan and exact-head CI. No verifier-unit suite or normal/full/H3 run is implied. New
original captures or B/C/D recollection require their own concrete allocation.

The original operation-flow candidate's four review failures are retained in the Issue/PR handoff
and local `root-counterexamples-01.json` / `root-effects-01.json`. Corrections require explicit null
camera operands, caller lookup inside each invocation/return interval, actual first-destination
facing/allocation/slot/sprite effects and independent shared-tail flags/counts. Use existing source
population/setup lowering; account for source overrides and preserve-mode retention. Never borrow a
later stack or substitute event labels for flag effects. In this unchanged A03, the calls at4107 and
12608 lack in-call stack observations; the whole route child is honestly Unavailable. Reproduce with
`report-corrected-final-01.json`, `direct-correction-02.json` and their fresh current/mixed matrix
reports under the same ignored output root. The 24 original direct cases and four integrity
corruptions remain required alongside targeted wrong/missing/mixed cases for these four corrections.
Reuse A03 and its exact tested adapter/probe identity; these comparator corrections require no SDK,
native, B/C/D, original-source acquisition or broad suite rerun.

### Caller and reached visual resource cohort

The bounded caller/resource allocation reuses the successful current A02/B02 observations.
Freeze the accepted party/start, source world with its two AB operations, cumulative scene45 and
material selection. Keep fixed60, optional poll1 and the legal adaptive winning route. Successful
captures are reused for offline corrections; every failed attempt retains its process, log, tested
source/binaries and failure record. The subsequent [current C/D allocation](#current-cd-settings-capture)
consumes the accepted performance work in Issue #605; successful A/B observations remain frozen.
No original runtime acquisition or complete H4 acceptance is implied.

The application publishes `ProgramControlReads` separately from semantic observations. Each actual
call/return retains its full copied before/after stack and resulting cursor at the producing Commit.
The finite source-map-script return is read after its final real service. The observer does not add
yields, events, services or RNG draws. Direct production-assembly checks cover nested/synchronous
returns, empty callers, waits, failure and the instruction budget, delayed script return and battle
transition.

The same session's post-draw channel records `resourceRequirements` and `resourceUses`. The requirement
comes from the logical map layer/block/tile or visible entity/portrait; use comes from the actual
assigned/drawn cached typed texture selector. Identity retains session/revision/observation sequence,
simulation tick/token, logical map visit and presentation phase. Retain priority passes and occlusion subjects,
normal/lowered gesture frames, portrait alternate tiles, visible fairy raster bindings and actual
field-death actor textures. A selected resource absent from the route is not an additional obligation.
The full source/use result is retained as `actualObservations.reachedVisualMaterialBinding`.

Add `--canonical-content $selectedCanonical --tileset-metadata $selectedTilesetMetadata
--palette-metadata $selectedPaletteMetadata` to the existing explicit modern material comparison.
These are read-only accepted private exports, resolved from the repository root when relative;
verify their existing canonical/metadata identities and the registered ROM before source decoding.
The existing atlas recipe and sprite/portrait decoder reproduce the selected source bytes in memory.
The field-death extension is checked separately from frozen base42 using its existing provenance and
ROM spans. No new export, manifest, cache or private acquisition is required.

Missing independent requirements, actual uses, caller operands or source prerequisites remain
Unavailable. Contradictory evidence dominates missing evidence. The existing whole children and
closed mandatory inventory remain unchanged; no partial subset passes or new mandatory rows are
introduced. Keep historical and mixed-content diagnostic matrices separate from the current cohort.
Full H4 and original natural timing/history remain open until their own evidence and independent
acceptance boundaries are met.

The JOIN reader supports both accepted completion orders. A real late-held logical-end interval
retains its state, attempted input, no added service/debt and transitional playback operands. Early
completion may leave no such sample: use the actual source music request and SoundWait installation,
held session/token/cursor, declared finite profile, arm/eligible progress and complete three-service
groups, real completion/finish receipt and previous-track restart before plain input and caller return.
The group count follows that session's actual pre-helper progress and declared end step. No missing
terminal held state is fabricated. Missing necessary receipt/source/state operands stay Unavailable;
a wrong helper/generation, premature return or illegal held input remains FAIL independently of
unrelated missing evidence. Retained late observations remain positive and negative coverage.

The matrix retains both ordered raw streams and their exact differences. Its bounded causal rule
can pair one matching finite `MUSIC_JOIN` actual-completion across only the same occurrence's
`music-step`/`music-helper-service` records. Each side independently proves source/request,
session/generation, receipt, arm/eligible progress, complete helper groups, both gates and dependent
caller return from its actual files. Report integrity, all other observations and state/input/seed
components remain mandatory. Wrong payload, shifted gameplay, another cue/generation, duplicate or
missing completion, crossing arm/eligible/release/restart/return, or false source provenance is FAIL.
Missing necessary proof is Unavailable; observed contradictions dominate unrelated missing evidence.
This correspondence neither sorts nor deletes events and does not change `semantic_value`.

Detailed field resource collection is enabled only while the actual Godot user signal
`ResourceDrawObserved` has a connection. The observer installs that subscription before the first
draw. Texture creation caches typed selectors; unsubscription produces no detailed requirement/use
collections or actor/portrait selectors. The bounded `guard-startup-04` check observes the first draw,
unsubscription, callback replacement and a replacement host/session. All eight predicates pass;
two ObjectDB instances reported at temporary-helper exit remain a recorded warning, not a clean-log
or memory-performance claim. Preserve the preceding completed helper/managed-signal failures.
The production `run_portrait_event` startup also guards its later session-result registration with
`is_connected`, because the early node-added hook has already installed that callback for winning
observations. The separate bounded `production-startup-03` window executes the production startup
and stops before its first navigation key submission: first draw requirements/uses327, exactly one
`record_warp_result` callback alongside the existing managed signal bridge, one actual startup
result delivery, zero submitted inputs and no host errors/warnings. The earlier lifecycle helper
overrides `run` and does not prove this production registration path. Preserve production-startup01's
helper count failure and production-startup02's custom-callable diagnostic error; no full route,
subscription lifecycle repeat or C# rebuild follows from this GDScript-only correction.

The independent resource inventory derives block/tile keys from retained layer geometry, source
layout and camera coverage, and portrait poses from logical identity/flags/work plus source alternate
tiles. Removing the same item from requirement and use channels therefore cannot define away the
obligation. Retained first/occlusion tile operands supplement coverage independently of those channels.
Missing occurrence operands are evaluated locally so later available contradictions still fail.
**Unknown:** A02/B02 do not retain the current mutable working layout for door/flag/roof copy regions.
Their historical exact per-Submit diagnostic remains Unavailable. A static base layout cannot replace
those missing runtime operands. The optional [composed mutable-map delivery](#composed-mutable-map-verification)
input establishes its own behavior-class boundary without rewriting those reports. Entity/portrait
and scene source/use children retain their results.

The winning tracking observer uses the same 15-second no-progress boundary as the remaining
before-body route, with actual logical state and mounted text delivery as progress. A fixed host-frame
limit can expire while source text services continue when collection or presentation consumes extra
frames. On a genuine stall, retain the full terminal state. This observer boundary changes no engine
clock, source service, input cadence, playback or focus policy. Non-winning bounded tracking retains
its existing frame limit. Preserve failed `caller-resource-cohort-01/variant-B-01` as a completed
observer failure; its identical 2293/94/95 checkpoints and advancing 2296 tail do not prove a stall.

The corrected candidate reports `report-A-10.json` and `report-B-02.json` each retain
2247 PASS / 14 Unavailable / zero required FAIL. All five operation children pass; whole map resources
remain Unavailable at the missing working-layout boundary. The original strict `matrix-AB-01.json`
and `matrix-AB-reproduction.json` remain immutable FAIL evidence: only the matching asynchronous
JOIN completion moved, across positions 10336–10340 of 34571 observations. All other operands match.
The bounded correspondence yields A/B PASS with C/D Unavailable, overall Unavailable and no milestone
PASS. Reproduce using the corrected reports, loading private configuration in the same process and
choosing a fresh ignored output:

```powershell
. ./local/private-inputs.ps1
uv run python -m sf2tool.remake_h4_comparison matrix `
  --reference local/issue534/modern-h4-applicability/inputs-01/reference.json `
  --variant-report local/issue534/caller-resource-cohort-01/report-A-10.json `
  --variant-report local/issue534/caller-resource-cohort-01/report-B-02.json `
  --output local/issue534/caller-resource-cohort-01/matrix-AB-causal-resource-review.json
```

Expected exit 2: A/B PASS, C/D Unavailable, `rawOrderEqual=false` for B with retained differences and
independent causal occurrence proofs. Direct review covers joint block/tile and portrait-pose omissions,
missing/contradictory occurrences in both directions, prior caller/resource negatives, early/late JOIN
and the matrix exclusions. Obsolete historical reports still preserve raw exact equality but fail
current mandatory report-integrity checks; they cannot stand in for current reports. Freeze this
bounded result in the same Draft PR. Complete settings/H4, missing original timing/history and #605
performance acceptance remain separate.

### Composed mutable-map verification

The current keyboard-A mutable-map boundary is owned by the
[continuous contract](../../docs/design/contracts/map3-battle01-continuous-scenario.md#composed-mutable-map-delivery).
Use the shared `map_consumer_binding` through the bounded standalone command:

```powershell
. ./local/private-inputs.ps1
uv run python -m sf2tool.remake_h4_comparison map `
  --actual local/map-delivery/actual.json `
  --map-context local/map-delivery/context.json `
  --text-source-root $pinnedDisassembly `
  --output local/map-delivery/report-new.json
```

Paths are selected per checkout; `$pinnedDisassembly` names the registered read-only pinned source.
Prepare the input from retained compact selections and frozen native observations, not a new full
route. Combined input and output each have a 10 MiB bound; output must be fresh under this worktree's
ignored `local/`. Runtime and output are linear in selected history plus region/draw occurrences;
the controlled rectangles bound cell work, and each observed draw has a 4096-use overflow limit.
No SDK, emulator, full H4/matrix or new source acquisition is implied by this command.

The [module route](../../docs/operations/bounded-inspection-and-review.md#h4-mutable-map-module-route)
names the source, history, delivery, layout and Draw owners. Direct observations can import
`map_consumer_binding` from `sf2tool.remake_h4.map_binding`; its signature remains
`(actual, context, source_root)` and the old import is the same callable. Source-region and
draw-cell observation aliases remain available. Inputs are selected objects, including
caller-owned streams; the binding neither closes them nor reads a raw capture. Full reports
remain readable after the caller closes those streams. Source paths resolve from the repository
root when relative, including invocation from `remake/`; CLI outputs remain worktree-local.
Compare complete checks, witness details, coverage and historical fields when changing these
boundaries, including the independent actor/plane and transfer-lineage controls below.

The selected actual carries `sessionId`, `mapHistory` change-run/event records and
`mapHistoryReceipt` (completed scan, relevant counts and producer inventory). Each run retains
`channel`, `first`, `last`, `count`, and `change`; each event row retains its index/identity and ordered
events. Context carries `historicalSession` and `witnesses`, with `role` and frozen `samples` for
`house`, `school`, `castle-walk`, and `castle-rebuild`. Original labels locate the named region states;
they do not define gameplay legality. The source tables define expected regions and contents.
Context can also carry the same compact `mapHistory`/`mapHistoryReceipt` when the ordinary actual
capture supplies the session in a resource or modern comparison. New controlled sessions must remain
distinct from the historical one and internally consistent; a passing report flag is not evidence.
Join their layouts to the start and ordinary-input ready states, and bound transfer revision/sequence
by the initiating input, warp start and resulting region state. Actor visibility and size are checked
against viewport intersection and scaled 24-pixel bounds; pass allocation follows renderer actor and
high-plane restoration order, independently of supplied mask passes. Older non-overlapping actor
records can leave priority alternatives unresolved without inventing a mask observation.

Supplying `--map-context` to `resources` or `compare --profile modern-continuous` calls the same
predicate. It changes only the Map3/Map19 mutable-layout prerequisite; other resource checks and
known historical contradictions still contribute. Omission retains the existing Unavailable
boundary. Standalone PASS sets `milestonePass:false`; exact historical layout/draw diagnostics remain
Unavailable. Do not rerun a large historical capture merely to demonstrate CLI wiring.

Verification is direct: source decoding, complete applicability accounting, actual word/saved-state
and draw-multiset joins, plus positive/missing/extra/wrong/session/coverage controls. Exercise an
available contradiction alongside an earlier missing region and within a partially missing region.
Include mutually changed actor/mask passes, hidden onscreen actors with removed masks, foreign but
internally consistent snapshot sessions, out-of-order/out-of-range transfers, and missing actor
fields alongside independently wrong draw geometry. Missing fields alone remain Unavailable.
These are comparison controls, not new engine unit tests. Run affected Ruff, contract/private-boundary
checks, normal public verification and the committed dependency planner. Preserve completed failing
controls and native receipts; corrections rerun only the affected comparison/control, not successful
native scenarios. Recollect only a concretely missing consumer observation when existing APIs expose
it; production/API changes or broader acquisition need their own ownership decision.

### Selected offline resource comparison

`remake_h4_comparison resources` selects only the reached visual resource binding. Supply the same
explicit world/scene/process/asset pins and read-only source/canonical/metadata arguments as the
complete material command. `--resource-family` selects `map`, `entity`, `scene`, or `all`;
`--session-id`, `--visit`, and `--occurrence` select exact session, logical visit and
`observationSequence` values. Selection precedes import and derivation. No selected result performs
the audio, input, motion, operation-flow or ordered equivalence comparisons.

Use a fresh worktree-local output directory for each command. First record a separate source-only
loaded baseline, with no capture import or derived-store cache. For example, retain the complete
material arguments in a PowerShell argument array:

```powershell
$resourceArgs = @('run', 'python', '-m', 'sf2tool.remake_h4_comparison', 'resources',
  '--actual', $actual, '--selected-world', $world, '--selected-scene', $scene,
  '--process-receipt', $processReceipt, '--scene-evidence-root', $sceneEvidenceRoot,
  '--asset-root', $assetRoot, '--expected-asset-commit', $assetCommit,
  '--expected-asset-tree', $assetTree, '--expected-asset-manifest-sha256', $assetManifestSha,
  '--text-source-root', $sourceRoot, '--canonical-content', $selectedCanonical,
  '--tileset-metadata', $selectedTilesetMetadata, '--palette-metadata', $selectedPaletteMetadata)
& uv @resourceArgs --source-only --output $freshSourceReport
$sourcePrivate = (Get-Content -LiteralPath $freshSourceReport -Raw -Encoding utf8 |
  ConvertFrom-Json).sourceOnlyPrivateBytes
& uv @resourceArgs --resource-family map --session-id $sessionId --visit $visit `
  --occurrence $observationSequence --source-only-private-bytes $sourcePrivate --output $freshReport
```

The selected reader scans sealed JSONL envelopes once for sequence/channel/descriptor/terminal
integrity. It retains selected requirements, exact use operand variants and necessary independent
state/visit/start/terminal context. Referenced descriptor lifetimes and reduced context contribute
to `selectedDependencyBytes`; unrelated channels and projections do not enlarge this allowance.
Missing context or channels remain Unavailable. The process receipt's selected start and source
selections remain read-only prerequisites. The source-only baseline loads those source inputs;
record its loaded private bytes and separately measure the actual interpreter's absolute peak,
including transient source preparation. A Python launcher alone is not that interpreter.

Resource reduction retains the existing candidate key, Python numeric equality, repeated requirement
and actual-use multiplicities, every distinct contradictory operand variant, and original
captureSequence/channel/index/useIndex locators. Validation groups use only their own predicate's
operands; phase membership is unique. Ordered compact runs preserve the old first-exception prefix.
Theoretical `candidatePairCount` and legacy `executedPairCount` are separate. Source recipe checks are
evaluated per distinct expected material and retain their logical multiplicity. Other predicates,
temporal joins and ordered equivalence streams retain their existing rules.

`reachedVisualMaterialBinding.format` is `sf2-resource-binding-counts-v1`. `checks` contains counted
family/name/outcome rows; `familyCounts` supplies PASS/FAIL/Unavailable totals. `joins` contains compact
requirement results and exact executed outcome counts, rather than one dictionary per candidate pair.
`candidateVariants` retains all distinct operands/counts and first locators. Missing/failure witnesses
are deterministic and limited to eight per check/outcome; this limit is explicit and does not truncate
operand variants or counts. Do not use collection length as the logical check/pair count. FAIL still
dominates Unavailable, including caught malformed prefixes; historical fatal exception boundaries
remain fatal. Legacy JSON/SQLite reports remain readable without conversion.

The published JSON and its named SQLite companion reopen and relocate independently of scratch.
Raw locators are provenance; raw captures are not embedded. `resource_pair_details(capture,
requirement_result, limit=...)` lazily previews at most 1,000 original pairs without derived stores;
it supplies no new acceptance or whole-capture integrity verdict. The capture must be the retained,
previously verified input. Selected reports use profile `modern-resource-scope`, explicit
`comparisonScope` and `milestonePass=false`; matrix/integrity reject them as full H4 evidence.

Resource commands enforce a 20-minute scan/source/reduction/publication limit and 256MiB private
increment over the separately recorded baseline. A whole-capture resource run permits at most one raw
capture's logical bytes; an explicit family/session/visit/occurrence scope permits selected dependency
bytes plus64MiB. Preflight reserves at least6GiB of physical space after conservative allocation.
Count scratch, journals and publication together; compression does not satisfy the logical budget.
Progress is coarse by stage. Both JSON entries and their SQLite companions stay provisional until
the detached report reopens and the complete publication set passes its budget check. A final check
also covers entry promotion. The `*.resources.json` counters explicitly snapshot the stage before
that receipt is written; final command output and an external absolute-memory/peak-output receipt
cover its tail bytes and the complete run. On a budget miss, withdraw any promoted entries to
`*.partial`, preserve their companions and scratch, and publish only an explicit incomplete entry.
Correct the responsible stage before another large run. Remove only that new run's owned reader
scratch after successful final publication. Existing evidence is never cleaned here.

Use direct retained small positive/negative and scope/relocation drives, affected lint and document
checks, the committed dependency plan and actual public CI. Preserve a completed normal-verification
failure; this offline-only allocation does not authorize a generic aggregate, SDK/native/H3/full run.

### Current keyboard comparison scope

The [current product scope](../../docs/decisions/0010-map3-battle01-product-acceptance.md#current-keyboard-scope)
requires default keyboard A. Historical B/D reports remain readable but do not contribute to current
counts, required reports or remaining obligations. Existing C is a supplemental keyboard diagnostic;
its actual comparison FAIL/Unknown and raw differences remain separately visible. Remapping, swapped
buttons, reduced flash and adjustable20 do not become mandatory full-route variants because they were
implemented or captured. The explicitly selected #517 fast-text speech behavior remains required.

`matrix` declares `scope`, `requiredVariants`, `excludedVariants` and `supplementalVariants` in its
output. It does not infer requirements from the supplied files. A missing/failed/malformed baseline or
missing required child cannot pass. In the retained A10 report, closing its legacy-named matrix
row leaves thirteen other children Unavailable. Their later independently accepted local compositions
are mapped in the [readiness ledger](../../docs/design/synthesis/map3-battle01-readiness.md#current-required-comparison-boundary).
This historical scope reproduction neither consumes those optional contexts nor revises its source
report; its Unavailable result is not a new failure of the accepted compositions. Main-gate has
accepted the complete current private milestone through independent review of parent coverage,
shared evidence joins and explicit deviations, as recorded in the
[accepted boundary](../../docs/design/synthesis/map3-battle01-readiness.md#accepted-current-milestone).

The retained historical scope check used A10 as follows. This is reproduction documentation,
not authorization to rerun it during a compact closure audit:

```powershell
. ./local/private-inputs.ps1
uv run python -m sf2tool.remake_h4_comparison matrix --matrix-scope current-keyboard `
  --reference local/issue534/modern-h4-applicability/inputs-01/reference.json `
  --variant-report local/issue534/caller-resource-cohort-01/report-A-10.json `
  --output local/issue534/settings-cd-current-01/matrix-keyboard-reproduction.json
```

Historical reproduction result: exit 2 / Unavailable, required A scope comparison PASS and
thirteen unavailable report children. Preserve that result alongside the accepted scoped proofs. Supplying
B/D cannot expand this scope or contaminate its gate; supplying C adds a separately labeled diagnostic.
Historical four-profile results retain their original meaning and files, rather than becoming current
requirements. Direct scope verification uses compact synthetic summaries and retained report entries,
not raw stream import or resource joins. The plan allows at most about208MB of retained JSON input,
1GiB peak memory, 2MiB output and a two-minute diagnostic threshold; network/model costs are not
applicable. Unexpected materialization or growth requires replanning before expanding the run.

### Accepted composition review

The [readiness owner](../../docs/design/synthesis/map3-battle01-readiness.md#accepted-current-milestone)
records final independent acceptance against `acaaba60a49357b8842e152d5e490981fe19d0c1`.
Review the existing continuous route/return and parent evidence, each linked child contract's
source/consumer proof and its reproduction recipe, plus unchanged current dependencies and explicit
deviations. The exact accepted [parent integrity](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/606#issuecomment-5965838128)
and [keyboard scope](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/609#issuecomment-5976283697)
reviews remain dependencies. No fresh full executable modern report or corrected whole-A trajectory
was produced; historical A10/default/matrix Unavailable and scoped `milestonePass=false` remain intact.

Retained private supporting inspections are under worktree-local
`local/issue534/integration-check-01/`: `compatibility-final.json` and `edge-parent-table.md` account
for checked/reused/unavailable joins; `parent-accounting.json` identifies exact proof/dependency
objects. Independent `root-pilot.json`, `root-scenes.json` and `root-parents.json` confirm common
physical/reward/turn compatibility, selected reward/audio scene matches and complete parent/child
accounting. These compact inspections supplement the named accepted evidence; their scripts are
retained local reproducers, not a maintained verifier or replacement evidence registry.

For a concrete review need, the corresponding read-only reproductions from the owning repository
root require retained compact inputs and a fresh output name. This documentation does not request
another run or authorize raw import, a route/matrix/full H4 run, native/SDK work or cleanup:

```powershell
. ./local/private-inputs.ps1
uv run python -X utf8 local/issue534/integration-check-01/overlap.py review-pilot.json
uv run python -X utf8 local/issue534/integration-check-01/scene-overlaps-corrected.py review-scenes.json
uv run python -X utf8 local/issue534/integration-check-01/parent-accounting.py review-parents.json
```

The retained handoff identifies other focused result/input/receipt inspectors and their exact
outputs. Use corrected scene matching; the old `scene-receipt-overlaps.json` has a valid receipt
comparison but an invalid later scene result, preserved with its correction. Missing audio original
array indices remain missing; logical/host/token groups supply the declared scene join. W2/audio
share accepted receipt evidence, not direct result overlap. Terminal internal completion remains
Inferred and its delay Unknown; missing historical turn operands, original timing and seed/HEAL/
provenance failures keep their owners. C remains supplemental and B/D excluded.

Resource history is preserved: the earlier whole-module AST inspection reached 163,532,800 bytes,
exceeding its 128 MiB bound. Integration inspection used about 128 MiB cumulative reads against an
80 MiB estimate; independent root review added 29,133,140 bytes separately. Integration and root
per-command limits held. These completed results do not require repeated evidence work absent a
concrete new defect. No historical evidence or failed result is replaced by the accepted composition.

### Current C/D settings capture

These retained captures predate the keyboard-scope correction. The D offline job was first safely
suspended and then **cancelled-by-scope**, exit1223, after matching its PID/image/command/creation
identity through a held process handle. Preserve `D-suspend-01.json`, `D-cancel-01.json`, raw capture,
partial SQLite/journal and logs; cancellation is not a completed comparison or PASS. Do not resume or
rerun D. No gamepad implementation or historical evidence was deleted.

The first old-comparator C job remains a supplemental diagnostic, with its loaded module content
preserved by Git object `9a55fe2c:src/sf2tool/remake_h4_comparison.py` (last source change `85e926c7`),
`C-runtime-comparator-9a55fe2c.py`, `C-runtime-helper-frozen.py` and `C-runtime-source-01.json` in the
output root. Its helper has no late reload or subprocess comparator reentry. Subsequent on-disk
matrix-scope edits do not relabel that running module. Pair generation, finish and report-companion
publication are distinct stages; an unpublished report is not complete. The old full C/D reports and
four-profile matrix are not prerequisites for this scope correction. Query-complexity work belongs
to #610, outside this change; no new full comparison or native capture is authorized by this recipe.

**Confirmed native capture:** `local/issue534/settings-cd-current-01/variant-C-05` at
`85e926c71581fe37d95973209d96ff35e40ffd95` and `variant-D-01` at
`31f3caf4bdf1ca3a2951e53d3d93f2d000f5d2f8` (only verification documentation changed)
complete the ordinary winning route and Left/Down return at fixed60/poll1. C uses remapped keyboard,
swapped Confirm/Cancel, adjustable20, reduced flash and reveal-only Confirm; D uses the admitted
remapped gamepad/right stick with the same settings and natural reveal. Each process exits0 with
zero host errors, unchanged settings/start/party and no remaining owned process. Their actual
streams contain269932/293080 contiguous records and intact terminals; the five separate metadata
records each remain below93KB and terminals below28KB. The maintained comparison reads the same
`actual.jsonl` as actual and outcome. Successful C/D and retained A10/B02 are immutable inputs.

Retain the completed discovery failures in this output root: C01 (202.170s) failed native numeric
membership and the omitted zone-control state; C02 (68.111s) failed unsigned Nod texture identity
conversion; C04 (169.172s, exit1) failed omitted policy terrain and oversized combined terminal.
C03 did not launch: its preflight compared assembly bytes despite unchanged Application source and
different build commit metadata. C05/D01 took248.191/324.231s. These failures are not interrupted or
replaced by the successful captures. The fixes preserve input legality, adaptive commands,
gameplay/RNG, all evidence fields and the original capture bounds.

Direct verification includes16 native numeric controls,11 metadata shape/equality/negative cases,
actual C05 terminal equivalence between inline/split formats, actual normal/lowered unsigned Nod
identities, probe check-only, controlled Debug build and adapter compilation. Default planner
selection is adapter-build/public-core/tooling-python, with no unclassified path or changed shared
execution semantics; `--scope engine` correctly rejects the shared H4 comparator. The normal
`uv run sf2 verify` completed148 checks plus document/index/ROM checks, then failed toolchain
provenance because this worktree has no default `local/upstream/SF2DISASM`. Preserve that completed
FAIL; the explicit accepted read-only source used by H4 does not satisfy the toolchain's owning-local
checkout requirement. No new upstream copy, generic aggregate, performance benchmark or original
runtime acquisition is part of this slice. Offline H4 comparison and remaining obligations retain
their independent result.

Reproduce offline with the existing explicitly selected read-only `$pinnedSource`, `$canonical`,
`$tilesetMetadata` and `$paletteMetadata` from the material evidence owner. The retained C05 recipe uses its own
process/settings/party/stream; D is cancelled and must not be rerun. Omit A-only normal05 baseline arguments. The frozen A report supplies
the unchanged scene/resource selection and asset pins, not C/D observation facts:

```powershell
. ./local/private-inputs.ps1
$run = 'local/issue534/settings-cd-current-01/variant-C-05' # retained supplemental recipe only
$process = Get-Content -LiteralPath "$run/process.json" -Raw | ConvertFrom-Json
$material = (Get-Content -LiteralPath 'local/issue534/caller-resource-cohort-01/report-A-10.json' -Raw |
  ConvertFrom-Json).evidence.materialSelection
uv run python -m sf2tool.remake_h4_comparison compare --profile modern-continuous `
  --reference local/issue534/modern-h4-applicability/inputs-01/reference.json `
  --actual "$run/actual.jsonl" --outcome "$run/actual.jsonl" `
  --settings "$run/settings.json" --host-log "$run/godot.log" --host-exit $process.exit `
  --controlled-start "$run/inputs/party.json" `
  --selected-world $process.selectedInputs.SF2_PRIVATE_EXPLORATION_CONTENT `
  --selected-scene $material.scene --process-receipt "$run/process.json" `
  --scene-evidence-root $material.sceneEvidenceRoot --asset-root $material.assetRoot `
  --expected-asset-commit $material.assetCommit --expected-asset-tree $material.assetTree `
  --expected-asset-manifest-sha256 $material.assetManifestSha256 `
  --original-join-evidence-root local/issue534/h4-join-consumer-01/inputs/original-join-01 `
  --text-source-root $pinnedSource --canonical-content $canonical `
  --tileset-metadata $tilesetMetadata --palette-metadata $paletteMetadata `
  --output local/issue534/settings-cd-current-01/report-C-reproduction.json
```

Choose a fresh output for each reproduction and preserve each JSON/SQLite report pair together.
