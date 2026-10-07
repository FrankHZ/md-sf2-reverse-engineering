# Field and Program Verification

Select the affected program, logical field service, map transition or actual presentation handoff.

Load the owning worktree's private-input configuration and protected SDK/Godot environment from
[development](../development-and-verification.md#locked-net-workflow) in the same launching process.
Unless a recipe explicitly changes directory, commands run from the repository root. Reuse the
existing instance when suitable and choose fresh ignored outputs. These are focused recipes, not
an aggregate checklist. No screenshots; no new tests of probes/helpers. Read completed failure and
Unknown boundaries before reproducing a claim. Full historical receipts remain in the
[pre-reorganization guide](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/1c4c786a6e2c630e1cfadf4daa88dbd7687caa19/remake/docs/development-and-verification.md).

| Affected behavior | Recipe |
| --- | --- |
| World preparation and authored programs | [Preparation](#reproduction), [conditional story](#replaceable-story-observation), [program observers](#exploration-and-program-observations) |
| Entity service and transfer | [Field action](#ordinary-field-action-and-warp-service), [warp](#ordinary-warp-visible-return), [idle callers](#source-idle-and-caller-verification), [zone caller](#source-zone-caller-observation) |
| Input and text | [Field Wait](#explicit-field-input-gameplay-wait), [plain-text Wait](#plain-text-input-gameplay-wait), [opening](#bound-opening-field-text-observation), [portrait](#bound-portrait-entity-event-observation), [W1](#w1-entity-event-input), [choice](#source-bound-choice-observation) |
| Actual field presentation | [Camera](#source-bound-camera-observation), [nod](#source-bound-nod-observation), [consumer handoff](#consolidated-field-consumer-boundary-observation), [parallax](#bound-field-parallax-observation), [draw operands](#reproduce-the-bounded-local-observation) |
| Source services and continuation | [Raw text](#raw-field-text-observation), [finite music](#modern-finite-music-observation), [map init](#bound-map-initialization-observation), [battle entry](#bound-before-body-and-battle-entry-observation), [victory/return](#bound-victory-and-return-observation) |

## Replaceable Story Observation

The [story A](../../content/authored/story-rule-demo-a.json) and
[story B](../../content/authored/story-rule-demo-b.json) public v8 packages establish external conditional
content replacement through the ordinary host. A branches on set flag40 into `repeat`; B branches
on clear40 into `first-visit-b`. Each executes both outcomes through two real interactions, with
different called programs, flags and marker positions. The nested pause presents its own actual
dialogue, consumes acknowledgement and30 logical services before later effects execute. Both
callers return; idle control is stable and real West input moves the player afterward.
The [content owner](../application/session-and-programs.md#authoring-conditional-programs) specifies expected effects
before execution. No producer, new endpoint, private state injection or package-name rule dispatch is used.

[`StoryPolicyReplacementTests`](../../tests/Sf2.Remake.Engine.Tests/StoryPolicyReplacementTests.cs)
assert the four valid branch paths, exact nested return addresses, later-effect blocking through
the penultimate tick, old/duplicate/wrong completion, bad untaken branch/nested call admission,
unsupported reached PC and selected-rule error retention after earlier effects.
[`BattleOutcomeProgramTests`](../../tests/Sf2.Remake.Engine.Tests/BattleOutcomeProgramTests.cs)
explicitly selects SF2 for the scratch operation: exact instruction/map/continuation/window/F603,
live alias, F1/hidden0x7000 tombstone and wrong-context matrix. A differently mapped physical record
keeps its actual slot/aliases during retirement. Authored policy rejects even a correct constructed
source context. These are engine behavior assertions, not reader/observer infrastructure tests.

After loading `local/private-inputs.ps1` in the launching process, force
`DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`. Use the existing `uv` environment, shared installed SDK/
Godot and worktree-local build/cache state. Run `uv run sf2 verify engine` and affected
`uv run sf2 verify adapter`; corrections use the following filter and preserve completed results:

```powershell
& $env:DOTNET_BIN test remake/tests/Sf2.Remake.Engine.Tests/Sf2.Remake.Engine.Tests.csproj `
  --no-restore --configuration Release `
  --filter '(FullyQualifiedName~StoryPolicyReplacementTests|FullyQualifiedName~BattleOutcomeProgramTests|FullyQualifiedName~ExplorationContentTests)&FullyQualifiedName!~PrivateBattleOutcomeProgramTests'
```

For the bounded native pair, select `RuleCompositions.AuthoredStory()` in `ForGame()` for both
inputs, rebuild the existing project, then start the same installed editor serially with the new
observer. There is no retained debug process in this recipe; restart is required to select the
compiled composition/input. If reusing a live owned instance, finish/stop its bounded case first.
Use fresh worktree-local output directories; keep earlier failures/results. For example from this
checkout's repository root (select `b` and a fresh run name for the second case):

```powershell
$repo = (& git rev-parse --show-toplevel).Trim()
. (Join-Path $repo 'local/private-inputs.ps1')
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
$variant = 'a'
$case = Join-Path $repo "local/issue678/native/$variant-NN"
if (Test-Path -LiteralPath $case) { throw 'Preserve prior run' }
New-Item -ItemType Directory -Path $case | Out-Null
foreach ($key in @('APPDATA','LOCALAPPDATA','TEMP','TMP')) {
  $dir = Join-Path $case $key.ToLowerInvariant()
  New-Item -ItemType Directory -Path $dir | Out-Null
  [Environment]::SetEnvironmentVariable($key, $dir, 'Process')
}
# From remake/ so global.json owns the SDK; only needed when building the selection.
Set-Location -LiteralPath (Join-Path $repo 'remake')
& $env:DOTNET_BIN restore game/Sf2.Remake.Godot.csproj --locked-mode
& $env:DOTNET_BIN build game/Sf2.Remake.Godot.csproj --configuration Debug --no-restore
Set-Location -LiteralPath $repo
$env:SF2_OBSERVATION_EXPECT_STORY = $variant # External expectations only.
$env:SF2_EXPLORATION_OBSERVATION_OUTPUT = Join-Path $case 'observation.json'
$package = Join-Path $repo "remake/content/authored/story-rule-demo-$variant.json"
$arguments = @('--headless','--path','remake/game','--log-file',(Join-Path $case 'godot.log'),
  '--script','res://probes/engine_story_rule_observation.gd','--','--authored-package',$package)
& $env:GODOT_BIN @arguments
$nativeExit = $LASTEXITCODE
```

[`engine_story_rule_observation.gd`](../../game/probes/engine_story_rule_observation.gd) extends the
existing exploration observer's input/read/output helpers; the base observer is unchanged. It reads
actual view dialogue/entity/flag/wait/caller projection and ordered `SessionResultObserved` records,
with real key events and a110-second timeout. Expected branch flags/coordinates and return PCs are
derived from the authored programs. Require exit0, `passed:true`, empty failures and no Godot errors
in both outputs/logs. Restore `ForGame()=>Sf2()` and rebuild Debug before freezing the source-default
candidate. A JSON edit requires restart/admission; a C# selection requires compile and restart.

**Confirmed — bounded authored native pair:** both branch outcomes per package, nested call/return,
real dialogue/timer, post-wait effects and returned movement passed through the same compiled
composition and host. The two inputs are about20KiB together; selected observations are about212KiB
per case, below the estimated5MiB/case and two-minute deadline. These measurements describe this
short pair only; runtime equivalence and general performance remain **Unknown**. The initial A
observer failure from integer-versus-JSON-float membership/caller comparison and an incorrect timer
PC expectation is retained in the local handoff; engine behavior was unchanged. This pair does not
exercise source scratch through native state injection, prove private natural reach or replace the
completed private HEAL CP2059–2091 failures/full-world limitations. No full-route/H4 gate follows.

## Ordinary field action and warp service

An admitted field `Move` records its pending direction in the existing `EntityWait`
and emits `movement-requested`. The next `AdvanceSimulation` consumes it at the
player's physical slot in `EntityActionRunner`, after that slot's motion update.
It rechecks current/reserved entity obstruction, opens any reached door, resolves
the warp marker before passability, and installs allowed travel. Remaining slots
use that old scene and the player's new reservation. A reached warp is dispatched
only after the complete pass; it does not add a second entity update.
The [source owner](../../../docs/research/map3-controlled-start-egress-transition.md#entity-service-before-the-warp-caller)
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
[source helper contract](../../../docs/research/map3-controlled-start-egress-transition.md#finite-full-black-helpers-and-visible-return),
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

The [source/phase owner](../../../docs/research/map3-controlled-start-egress-transition.md#first-warp-entity-phase-readback)
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
RNG FAIL, JOIN coupling Unknown, and old21/41/61/67 cleanup Unknowns. Those historical #534/#437 report verdicts remain unchanged. That observation-only slice leaves production behavior unchanged; the bounded
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
only their normal facing change. See the [source and remaining gaps](../../../docs/research/map3-controlled-start-egress-transition.md#ordinary-source-population-control-handoff).

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

The [source/caller owner](../../../docs/research/map3-controlled-start-egress-transition.md#source-idle-completion-and-caller-installation)
and [content contract](../application/session-and-programs.md#definitions-and-execution) define the current
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
consumer under [Option A](../../../docs/decisions/0010-map3-battle01-product-acceptance.md#evidenced-gameplay-waits-accepted-option-a).
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
natural-order evidence gaps remain Unknown; preserve the [H4 diagnostic](../evidence/retained-comparisons.md#continuous-h4-comparison)
results. This input does not normalize the old 260-step trace.

**Confirmed:** the bounded `field-wait-*` cases in the existing
[input observer](../../game/probes/engine_input_accessibility_observation.gd) exercise actual native
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

The [portrait/input contract](../application/session-and-programs.md#portrait-lifecycle-and-plain-current-input-wait)
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

Use the [execution profile](../application/session-and-programs.md#bound-field-text-work) and its
[source/settings provenance](../../../docs/research/map3-messenger-acceptance.md#opening-field-text-settings-and-view-binding).
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

The [portrait consumer](../application/session-and-programs.md#bound-entity-event-portrait) extends the bound
field-text profile through an actual entity wrapper return. Its
[source owner](../../../docs/research/map3-messenger-acceptance.md#classroom-portrait-entity-event-binding)
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

The [source-zone consumer](../application/session-and-programs.md#source-zone-caller) includes generic opening
Zone6, Sarah and the first Astral introduction through caller return. Its
[source owner](../../../docs/research/map3-messenger-acceptance.md#first-introduction-source-zone-caller)
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

The [static source/data owner](../../../docs/research/map3-messenger-acceptance.md#source-bound-camera-lifecycle),
[contract](../../../docs/design/contracts/map-exploration.md#bound-field-camera-lifecycle) and
[consumer](../application/session-and-programs.md#source-bound-camera) bind destination/scroll/wait/hold and
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
This retains the recorded display/speech difference; the later accepted #517 policy does not normalize audio
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
public aggregate/h3-witch NONRUN and cleanup21/41/61/67 Unknowns remain. Historical #534/#437/#523 failures retain their own outcomes; current composed acceptance
and workflow retirement are recorded in the [evidence owner](../evidence/retained-comparisons.md). No cleanup/archive is implied.

## Source-bound nod observation

The [source owner](../../../docs/research/map3-messenger-acceptance.md#source-bound-nod-lifecycle) and
[consumer](../application/session-and-programs.md#source-bound-nod) bind forty explicit services, independently
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

The [execution owner](../application/session-and-programs.md#w1-in-a-suppressed-entity-event) defines the
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
the usual private environment from [exploration reproduction](#reproduction),
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

## Exploration and Program Observations

Use [the exploration/program owner](#reproduction) for the current
Content preparation, controlled starts, selected private comparisons and ordinary native recipes.
The native observers exercise real input and read the same live session across mode changes. `engine_map3_opening_observation.gd` reads the R2 fixture only as an external input trace and covers complete acceptance or decline/re-prompt. `map3-opening-party.json` supplies the named opening party. The
private source programs retain their exact unsupported native/population frontiers; a stopped prefix
is not a completed original route. `EntityMotionTests` directly compares the extracted core and
destination admission to the13 owned H3 cases; it does not test a verification program.

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

## Source-bound choice observation

The [execution owner](../application/session-and-programs.md#source-bound-yesno-lifecycle) admits fresh conditioned
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
flag write, ten services and instruction return. Keep actual speech differences alongside the later accepted #517 fast-text policy.
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
The later accepted #517 fast-text policy and retired #523 investigation do not relabel these
completed failures; see [retained evidence](../evidence/retained-comparisons.md).

## Raw field text observation

This retained expected-Unsupported gate uses content without `modernEndStep`. For the admitted
modern profile and complete JOIN return, use the [modern finite-music observation](#modern-finite-music-observation).


For the [raw display capability](../application/session-and-programs.md#raw-field-display), run affected
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

The [accepted clock](../../../docs/design/contracts/music-wait-service.md#accepted-modern-finite-music-policy)
changes the affected historical trajectory comparisons under
[ADR0010](../../../docs/decisions/0010-map3-battle01-product-acceptance.md#accepted-modern-finite-music-clock).
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
acceptance; full hardware parity, original clock mapping and unrelated recorded failures remain unproved.

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
may be retained under the [existing menu contract](../architecture.md#production-assemblies): compare session,
actor, accepted selection/preview, party resources/progress/loadout, queue and RNG with the prior
state, require no action/turn observations, then confirm a subsequently accepted legal target.
Disposable actor-node geometry and attempted UI candidate may change; accepted gameplay state may
not. Other errors remain failures. A green report with callback/script errors is not clean acceptance.
The continuous winning and bound victory/return scope above has native acceptance;
executable H4 policy cutover and unobserved outcome/return variants remain open.

## Reproduction

Use the current worktree's retained environment, SDK/Godot and registered private input selectors.
Choose fresh ignored output paths. The canonical import (`uv run sf2 h2 map-import`) reads the H1
listing `build/sf2build-h1.lst`, so run the H1 rebuild with `-KeepBuildArtifacts` and install its
listing, symbols and binary under the conventional `sf2build-h1.*` names first; the frozen H1/H2
PowerShell rails require PowerShell 7. The ordinary host needs a world prepared with `--rom-path` and
`--presentation-root`: without presentation data its first sprite mount stops with the adapter error
`entity-sprite-binding`. Preparation and actual opening launch are:

```powershell
uv run --locked python -m sf2tool.remake_exploration_content `
  --canonical $env:SF2_PRIVATE_CANONICAL_MAP_IMPORT `
  --upstream $selectedUpstreamRoot `
  --selection remake/reference/inputs/map3-programs.json `
  --rom-path $registeredRomPath --presentation-root $selectedPresentationRoot `
  --output $env:SF2_PRIVATE_EXPLORATION_CONTENT

$env:SF2_PRIVATE_CONTROLLED_START = (Resolve-Path -LiteralPath 'remake/reference/inputs/map3-opening-party.json').Path
& $godotBinary --path remake/game -- --private-exploration-start (Resolve-Path -LiteralPath 'remake/reference/inputs/map3-opening-start.json').Path
```

The existing private encounter/static/enemy/gold selections are also required. Program data never
reads reference fixtures. For the native comparison, build the ordinary Debug project first and run:

```powershell
$env:SF2_PRIVATE_EXPLORATION_CASE = 'map3-opening' # or map3-decline
$env:SF2_PRIVATE_EXPLORATION_PLAN = (Resolve-Path -LiteralPath 'tests/fixtures/h3/map3-battle01-natural-route-v1.json').Path
$env:SF2_EXPLORATION_OBSERVATION_OUTPUT = Join-Path $env:SF2_RUN_OUTPUT 'opening.json'
& $godotBinary --headless --path remake/game --fixed-fps 60 `
  --script res://probes/engine_map3_opening_observation.gd -- `
  --private-exploration-start (Resolve-Path -LiteralPath 'remake/reference/inputs/map3-opening-start.json').Path
```

Require exit0, `passed: true`, both real sound counters, actual gesture draws, stable input and no
script/process errors. The observer waits briefly for the independent audio thread's stop/free queue
before headless exit; that wait does not advance game state. No screenshots or state setters are used.

```powershell
uv run sf2 verify engine
uv run sf2 verify adapter
$env:SF2_REQUIRE_PRIVATE_TESTS = '1'
& $env:DOTNET_BIN test remake/tests/Sf2.Remake.Engine.Tests/Sf2.Remake.Engine.Tests.csproj `
  --configuration Release --no-restore --filter 'FullyQualifiedName~PrivateExplorationTests'
```

The owning engine project copies R1/R2/R2a comparison fixtures and explicit starts. Private input
checks run only with their required input configuration; public runs report their skips. Use the
committed `verify plan --scope engine` selection and proportionate direct checks. Completed failing
runs remain recorded; rerun their corrected nodes rather than replaying the aggregate. The
remaining narrow observer is `engine_private_exploration_observation.gd`, with `map21-guard`
or the authored missing-presentation case.

For the connected castle group, use the same R1 start and R2 input-plan environment with
`--script res://probes/engine_castle_tower_observation.gd`. The observer continues the same live
instance, reads the H2 castle graph for navigation, revisits the palace, declines/re-prompts Astral,
and writes an adjacent `.castle.json` with checkpoints, actual palette brightness, priority and the
guard's pending/completed sprite service state. `map3-decline` first refuses and later joins Sarah
through actual input. Run the ordinary host at fixed60 and30 FPS, require `passed:true` and exit0,
and retain any completed failures in the local report. The final input sequence repeats the guard
interaction and walks the released passage, then checks stable PlayerInput.
No observer supplies an expected endpoint, completion flag, entity alias or session replacement.

For complete Battle01 admission, keep those same retained environment and R1 input selections and
replace the script with `res://probes/engine_battle01_admission_observation.gd`. Run fixed60 and30
FPS. The observer explicitly sets the root window to 960×640 before starting the route; headless
`--resolution` alone can leave the actual viewport at 64×64. Success requires the observed 960×640
viewport at battle load, first control and after cancel, a positive map viewport inside it with zoom
at least 1, and a positive preview inside the map when control is available. The adjacent
`.admission.json` retains the expected size and actual loaded/ready geometry; the main output retains
the final geometry. A completed 64×64 run is insufficient for this projection acceptance.
The observer reuses the opening/castle route and reads the existing PlayerReady static input
plan only for navigation. Its adjacent `.admission.json` records actual effects, load-before-input,
first battle projection and real confirmation/cancel. Require exit0, `passed: true`, nonzero mosaic
and shiver draws, white/load observations and no Godot script/process errors. Direct comparisons use:

```powershell
& $env:DOTNET_BIN test remake/tests/Sf2.Remake.Engine.Tests/Sf2.Remake.Engine.Tests.csproj `
  --no-restore --filter 'FullyQualifiedName~BattleEntryProgramTests'
```

`BattleEntryProgramTests` covers scene replacement and sprite waits, camera/shiver ownership,
first/seen/completed selection, input gating and initialization failure. With the registered private
bindings, `PrivateBattleEntryProgramTests` covers continuous R1 admission, the independently seeded
H3 comparison, live membership/formation/resources and the F88 boundary. These are engine behavior
checks; they do not test the observers or replay the legacy aggregate.

For the complete outcome group, use `res://probes/engine_battle01_outcome_observation.gd` with the
same retained project, R1 start and opening navigation selection. Set `SF2_BATTLE_OUTCOME_CASE` to
`victory` or `defeat`. The external observer chooses normal movement/action/target keys from the
live battle, then checks source programs, flags, actual presentation, 960×640 geometry and return
movement. It does not restore an expected seed, HP, roster, endpoint or program cursor. Fixed60/30
FPS may reach different battle seeds because real presentation allows background entity ticks.
The adjacent `.outcome.json` and normal output retain actual observations. Require `passed:true`,
exit0 and no Godot errors. Direct engine checks are `BattleOutcomeProgramTests`,
`PrivateBattleOutcomeProgramTests` and `BattleGrowthTests`; the private continuous victory also
crosses a real level threshold. It reads the accepted R4a static spine
(`sf2-map3-battle01-victory-return-static-v1`) for the unlocked/completed flags, the Battle01 join
row and the after-program, clear, set, SwitchMap and exploration order; that static spine proves
source order, not natural original execution. Preserve completed failures and rerun their affected nodes only.

#### Reproduce the bounded local observation

Load the checkout's ignored private-input selections and the existing private exploration/party,
static-data, battle-data, scene, terrain, enemy and gold inputs required by GameRoot. Keep the world
and pinned sources read-only. Supply a small private JSON through `SF2_LAYOUT_OPERANDS` with `blocks`
(the selected Map3 block tile-word table), `roofBaseWords` (the56 pre-load words above), and `starts`
(three absolute controlled-start paths in door/roof, flag-off, flag-on order). Door/roof starts at(4,7)
facing DOWN with flags0/32 and the retained controlled entity phases. Flag starts use(28,24), default
entity phases and flags0/32, adding506 only to the third start. These are explicitly controlled local
seams, not a natural route. Read only the required map metadata and block table from the private world;
normal host loading retains its existing behavior.

Set `SF2_PRIVATE_EXPLORATION_CASE=map-layout`, `SF2_LAYOUT_CASES=0,1` for door/roof plus flag-off, or `2`
to reproduce the unavailable flag-on startup independently. Leaving the selection empty runs all three;
a flag-on success comparison requires the preceding flag-off pre-state from the same run. Set
`SF2_LAYOUT_CONTROLLED_START` to a writable ignored staging file, initially copied from the first
selected controlled start, and `SF2_EXPLORATION_OBSERVATION_OUTPUT` to a fresh ignored output file.
Launch the configured Godot with the checkout's `remake/game` path,
`--script res://probes/engine_private_exploration_observation.gd -- --private-exploration-start`
followed by that exact staging path. The probe replaces only the staged input between fresh hosts in
the same process. Use the normal configured native audio driver and no screenshots.

Keep90seconds and10MiB per case,30MiB combined; retain launcher exit, stdout/stderr and resource limits.
The probe checkpoints completed cases with `complete:false` and only reports `passed:true` after
normal completion without failures. Startup failure is saved before stopping, rather than querying
exploration methods on the startup-error battle view. JSON numbers and direct Godot/CLR numeric
variants compare by numeric value. Preserve prior failures and partial files; they are not passes.
Run the affected adapter build and these direct observations; no test of this probe or full-route run
is required by this bounded seam.
