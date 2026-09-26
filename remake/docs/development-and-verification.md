# Remake Development and Verification

## Scope

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
cues do not imply logical palette effects. With bound display state, white,
partial/tint/restoration cues without this binding are Unsupported; asynchronous
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
