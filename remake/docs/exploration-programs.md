# Common exploration and resumable programs

## Current capability

The ordinary `GameSession` executes exploration, resumable story programs and map transfer, then
hands the same session to the existing battle flow. `harbor-arrival.json` and `hill-passage.json`
exercise different maps, actor identities, dialogue, choices and motion through this path. Accepting
runs entity actions, a call/return and a timer, transfers maps, executes map init and both intro
hooks, initializes the encounter and reaches first player control. Declining returns field input.
Neither package identity nor an expected route/receipt admits a command.

`GameSession` remains the sole snapshot publisher. `ExplorationDispatcher`, `ProgramRunner`,
`EntityActionRunner` and `MapTransfer` compute immutable results. The active payload is either
`ActiveExploration` or `ActiveBattle`; story flags, program PC/call stack, typed wait, text window,
simulation tick and observations survive the switch. The battle view attaches to the existing
session. It cannot restart it at the transfer endpoint. Startup and transfer use the same derived
`HasBattleControl` boundary: an active battle with a pending program or wait keeps the program view
until its dialogue, choice, timer or presentation service finishes, then exposes ordinary battle
control.

Before battle entry, `ExplorationState.Party` owns the explicit candidate resources, seeds and
accounting. Joined/active membership is separately owned by story flags and source-order counted
lists; follower links refer to physical map slots. `JoinForce` refreshes counted lists before its
active-flag write, so the immediately exposed list can lag that final write. This is intentional
source chronology. The opening input supplies the first three R1 allies' class, resources,
equipment and spells. Other initial ally appearances and names come from pinned source data;
this group does not claim a full travelling roster, mutable names, promotion/death appearance,
inventory operations or later battle admission.

## Definitions and execution

Format-v7 battle packages remain current. Format-v8 exploration packages contain `package`, an
embedded format-v7 `battle` definition/start, `world` and `start`. World data contains maps, ordered
events, programs and text; start data contains the selected map/player, position/facing/speed and
flags. An optional start `program` invokes instruction zero of a whole program in a fresh session.
It does not import a PC inside that program, a call stack, an active wait or any executed history.

The Content reader links all call/jump/branch targets, including untaken branches, and checks map
references, duplicate IDs, scalar domains and terminal fallthrough. Entity references that depend
on runtime population are checked when reached. Typed operations support flags, calls/returns,
conditional branches, text cursors, continued/single text, explicit close, yes/no, fixed-point
entity motion, facing/position/visibility, tick waits, presentation requests and map transfers.
Known unimplemented operations retain a source-attributed stop instruction.

A blocking instruction keeps its PC until its own wait completes. A stale token/revision cannot
release it. Continued text remains visible after acknowledgement until close or replacement;
single text closes on acknowledgement. Original dialogue speaker flag bytes remain in the typed
request/window; they are not silently discarded or treated as proof of original portrait rendering. Real ticks advance background actions even while text is
open, and increment `SimulationTick` only when executed. A tick batch stops at a newly reached
wait boundary. The host batches elapsed 60 Hz ticks and subtracts only ticks the engine executed.
Unused time remains available for automatic continuation on the next frame; reaching paused input
clears the accumulator. Lower frame rates therefore retain elapsed ticks without draining a newly
reached dialogue or choosing an answer automatically. Existing field input does not truncate a
background action batch; completing a foreground wait still yields at the newly reached input or
program boundary before further ticks are submitted.

Movement uses the existing `OriginalMapTraversal` area, collision and stair rules. The extracted
entity core uses 384 fixed units per tile, source signed-word arithmetic, acceleration/deceleration,
destination obstruction, facing/animation and arrival layer/immersed changes. Each entity's movement
runs before its action dispatch. Nonwaiting configuration actions execute in the same tick; movement
and timer actions yield. Replacing an action stream retains current physical motion. Source `setPos`
changes position, destination and facing without resetting unrelated speed/flags/action state.
`SPRITE_SIZE` is global. A later failing action preserves completed configuration and movement in
that tick and retains the failing action cursor. No destination assignment replaces a motion path.

The logical map/battle selector checks the unlock/completion flags after map loading. Entry runs
**before program → region/party/enemy initialization → battle load → start program → first round**.
Both hooks check the same intro flag; the start wrapper sets it. The internal `BeforeBattleRouted`
policy is produced only after Application completes that routing. External standalone starts still
require the existing explicit controlled skip. Battle region state and its activation rules retain
their existing Domain owner; general story access to battle-region flag aliases is outside the
selected private programs described below.

## Original Map 3 opening

The named `map3-opening-start.json` begins at R1's controlled Map3 initialization seam, at (56,3)
facing down with only Bowie joined/active. It is not natural title/menu reach. The ordinary host
consumes canonical layouts, ordered setup/entity/event tables, pinned native wrappers and complete
map-script bodies. No expected endpoint, route ID or reference receipt participates in execution.
The separately read R2 controller trace exercises house exit, both doors, Sarah's classroom
interaction, stairs, entity142, Astral's zone and the messenger trigger. The complete messenger
acceptance runs through its motion, text, choice, two nods, camera waits, join music/fade, membership,
follower installation and guard positions; the zone wrapper then sets F603 and returns field input.

**Confirmed:** common-session comparisons match the accepted R1 position/membership/start RNG,
R2 program order and route positions, and R2a text IDs/packed speaker operands, flags, guards,
follower links and (43,10)/down endpoint. Ordinary Godot input observations reach the same endpoint
and move down then back up through a follower-occupied tile. A second input run declines, verifies
that join/follower flags stay clear, then talks to Sarah and accepts. That alternate behavior is
supported by pinned source and reproduced in the remake; it is not an additional original H3 claim.
An independent live-flag start enables another follower and moves logical entity142 from physical
slot17 to18. Logical identity and source record identity do not depend on that slot number.

Population allocation preserves physical processing order, enabled source followers, source allies
that reuse those slots, and non-ally logical aliases. Source `eas_Init` configurations execute and
wait for a matching sprite mount. Source walking templates are lowered from their actual operations;
random movement uses the existing main RNG. Followers measure current separation, rotate source
offsets around the leader's destination, and apply terrain fallbacks. Player input checks the source
obstructable bit and both current/reserved positions; scripted motion keeps its distinct destination
check. Hiding removes the logical aliases and follower behavior while retaining the physical slot.

Ordered door copies run before traversal; flag copies run at rebuild; roof activation/restoration
uses the shared layout reducers and signed area overlays. Source map255 same-map reload preserves
entities and the working layout, updates player position/facing, performs roof-on-load and reruns
selected initialization. The original setup selector still refuses an unimplemented alternative
instead of publishing default entities. This group uses Map3's default setup; broader initialization,
healing/temp reset, dynamically promoted/dead allies and other setups are outside its start boundary.

## Presentation and private Content

Content validates private provenance, embedded raster identities/shapes, required map/sprite links
and portrait references before session creation. Preparation verifies the registered USA ROM and
pinned source/asset manifest, reuses the existing map atlas, and decodes required sprite directions
and portraits through maintained decoders. The generated world and all game text/raster bytes remain
ignored private output. No ROM, extracted asset, dialogue prose or private absolute path is tracked.

Godot draws the working block layout and source sprite frames, mounts requested entity sprites,
shows source portraits and resolves `{LEADER}`/`{NAME;n}` from configured source names. It scrolls
the camera to the requested destination and waits for arrival. Nod presentation freezes the entity
animation counter and uses a 40/60-second timeline, with the altered head band from 10/60 to 30/60
seconds and the restored sprite afterward. Completion follows elapsed time independently of viewport
culling or missed render phases. Actual visible nod and restored-sprite draws are counted separately;
an off-camera actor or a frame that skips a phase does not fabricate a draw or hold the story forever.
`CompletePresentation` validates both wait token and kind; `EntitySpriteReady` validates slot and
request generation. Enter acknowledges dialogue only and cannot complete an unperformed service.

The audio contract permits an explicit project-authored presentation mapping: MUSIC_JOIN and
MUSIC_SAD_JOIN play a generated C-major/C-minor chord loop through `AudioStreamPlayer`, with an actual
fade and stop. This is a modern cue, **not original audio parity**. Sound and fade services continue
while the common story owns control, including after
the active state has become a battle with no exploration world. Camera and gesture cues require a
current exploration world; without it they report `AdapterError` and retain their wait instead of
using stale map state. Camera smoothing, portrait crop,
text layout/full-line display, nod pixel transformation and cue duration are bounded presentation
choices. Original VRAM/DMA timing, portrait blink/mouth/typewriter RNG, speech SFX, door/warp music
and fade fidelity, waveform/tempo and hardware frame equivalence remain **Unknown or Unsupported**.
They are not silently reported as performed original services. An unbound explicit presentation cue
reports `AdapterError` and retains its wait.

## Remaining source boundaries

The Map21 guard still stops at logical entity135; its runtime alias/population is **Unknown**, so
F401 is not invented. The marked Map40 warp reaches the actual before-battle program and stops at
`loadMapFadeIn`; an explicitly supplied seen-intro flag retains the accepted controlled battle-entry
comparison. The complete before-battle body, castle/tower route, natural Map3-to-Battle01 continuity,
outcome and return remain outside this group. Other Map3 native branches such as ChurchMenu and
moveNextToPlayer retain executable source frontiers.

After F603, a later Map3 init can target entity142 after its alias has been removed. The source
lookup returns the255 sentinel and its helpers perform raw entity-address writes. That later RAM
boundary is not admitted here; the common session refuses the missing alias. Stable local field
movement after acceptance is confirmed, but another post-acceptance map reload is not claimed.

## Reference migration boundary

The ordinary source path has no Reference dependency. Complete legacy Sarah, zone601, entity142,
Astral-zone and messenger endpoint-writing methods, their F/G presentation branches and obsolete
prefix/atomic-route tests are removed. Whole-flow comparisons now use common commands and actual
native input. The old private probe retains only the separate Map21/Map40/frontier comparisons.

Castle/tower, pending Battle01 admission and M4 return remain reference consumers. Their tests use
`map3-post-opening-reference-start.json` as an explicit controlled starting context, with zero executed
history and no messenger receipt. The optional reference-host variable
`SF2_REFERENCE_POST_OPENING_START` selects this same input. This is not a bridge proving original
continuity. The default old private start remains a geometry diagnostic; its migrated interactions
report that the common host owns them. State/receipt DTOs and trust projections still used by frozen
later snapshots remain until their last caller migrates. Shared legacy geometry/visual bindings also
serve those later comparisons. G3/G4, A1–A8 and8C/H4 are not reported globally closed.

## Reproduction

Use the current worktree's retained environment, SDK/Godot and registered private input selectors.
Choose fresh ignored output paths. Preparation and actual opening launch are:

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
uv run sf2 verify reference-host
$env:SF2_REQUIRE_PRIVATE_TESTS = '1'
& $env:DOTNET_BIN test remake/tests/Sf2.Remake.Engine.Tests/Sf2.Remake.Engine.Tests.csproj `
  --configuration Release --no-restore --filter 'FullyQualifiedName~PrivateExplorationTests'
```

The owning engine project copies R1/R2/R2a comparison fixtures and explicit starts. Private input
checks run only with their required input configuration; public runs report their skips. Use the
committed `verify plan --scope engine` selection and proportionate direct checks. Completed failing
legacy runs remain recorded; rerun their corrected nodes rather than replaying the aggregate. The
remaining native observer is `engine_private_exploration_observation.gd`, with `map21-guard`,
`map40-intro`, `map40-seen` or the authored missing-presentation case. The Map40 observer reads the
existing PlayerReady fixture's static input plan; it does not supply runtime state.
