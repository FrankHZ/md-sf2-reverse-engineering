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
session. It cannot restart it at the transfer endpoint.

Before entry, `ExplorationState.Party` owns the explicit candidate battle start's resources, seeds
and accounting. Entry consumes that exploration payload to initialize battle actors. This bounded
model does not implement a travelling roster, follower membership, inventory changes, joins or a
second persistent battle state. Those operations need their own admitted rules before the private
route can continue through them.

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
wait boundary. The host does not drain later dialogue or choose an answer automatically.

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

## Original content and admitted comparisons

`python -m sf2tool.remake_exploration_content` consumes the registered canonical map export and the
pinned source checkout. It verifies the existing canonical manifest identity, upstream commit and
selected source bytes against that commit. It reuses the existing source statement/equate parsing;
no comparison fixture supplies runtime program definitions. The reference selection
[`map3-programs.json`](../reference/inputs/map3-programs.json) selects resources/programs and explicit
controlled motion defaults. Actual operations, layouts, entity records, warp rows and battle hook
references come from those private sources. Branch/fallthrough targets are retained. Unsupported
source services remain at their real operation rather than being dropped.

`PrivateExplorationReader` reads the prepared world and a separate controlled start once, joins its
upstream/ROM provenance to the existing private battle source, validates the typed content, and
retains both preparation and start boundaries. The prepared file is the output of the named offline
trust boundary; this JSON reader does not independently re-extract or authenticate its transformed
program bytes. Original-source claims therefore require that preparation command and its selected
inputs, not an arbitrarily edited prepared file. All prepared content, original dialogue, assets and
native observations remain ignored/private. Public start files contain only minimal controlled facts.

**Confirmed (bounded engine/reference/native observations):**

- `cs_513D6` executes Sarah's two movement segments. `cs_5148C` and `cs_513A0` apply their actual
  explicit position instructions without claiming the earlier route executed.
- `cs_51652` moves both guards, presents its dialogue requests, then moves both back and returns.
- `cs_5149A` executes its initial waits, multi-entity motion, dialogue and speed change before the
  first `nod` at source operation39. That gesture remains **Unsupported**; later F600/join effects
  have not executed.
- `cs_5145C` enters actual `eas_Init`, commits its configuration and global size24, then stops at
  `ac_updateSprite`. Compressed sprite/VRAM queue completion is **Unsupported**, not an idle receipt.
- `cs_53EF4` completes guard128's move, then stops at `setFacing 135`. The runtime alias/population
  of135 is **Unknown** in the accepted controlled owner; F401 is not invented or published afterward.
- The actual Map40 input plan reaches the marked wildcard warp into Map57. With intro flag clear,
  the actual `bbcs_01` sets text cursor2292 and stops at `loadMapFadeIn`; its coordinates are camera
  operands, not a player warp. With explicitly controlled F451 set, both hooks skip and the same
  session initializes Battle01 and matches the existing first-player/RNG boundary.
- The movement core/destination projection matches all13 cases in the existing H3 entity movement
  matrix. This is a bounded component comparison, not a full EAS interpreter or natural timing claim.

Map3/Map21 starts are explicit whole-program entries with source default placements and controlled
inactive autonomous scripts. Their unported input/setup/event services retain source frontiers.
The existing `MapSetupSelector` selects the last set-flag alternative before entity construction;
an unsupported selected setup cannot publish the default setup's population. All source warp rows are retained; unadmitted destination/scroll/reload branches stop before transfer.
Map40's supported flag-guarded init preserves ordered setup alternatives; an unsupported selected
alternative stops. A same-row cell without the warp marker does not transfer.

Original follower population, joins/items, general init/layout mutation, same-map reload, source
text-tag/name substitution, camera/fade/gesture/FX/audio, the full before-battle body and natural
Map3→Battle01 continuity remain **Unknown** or **Unsupported** at the named owners. Source tags remain
private text data; displaying a request does not prove original text timing or presentation. The
ordinary view reports `AdapterError` for a presentation cue it cannot perform and leaves the wait
pending; Enter cannot fabricate its completion. Outcome/after-program/return remain M4.

## Reference migration boundary

The new ordinary source path has no reference assembly dependency or fixed-route session classes.
G3/G4 are not closed by a controlled program-entry comparison. The existing reference
`PrivateMap3Composition` still calls full Sarah/dialogue/zone, messenger, castle/palace and guard
handlers; the pending `PrivateOriginalBattle01Admission` and M4 return paths still require their
original-map context. Those handlers include unported init/event/follower/presentation services
outside the executed program bodies above. No last caller has disappeared for those families, so
none is deleted on the strength of a partial comparison. The [caller inventory](../reference/README.md)
is the removal boundary; retire each complete owner together with its final caller when that flow
migrates. M3's common capability group is available; the full original-route migration and A1–A8 are
not reported complete.

## Reproduction

Use the current worktree's existing environment/input selectors, verified SDK/Godot installation and
fresh ignored output directory. Do not replay an old research worktree's scripts. Set the variables
below to explicitly selected local inputs/output; private absolute paths are not repository content.

```powershell
uv run --locked python -m sf2tool.remake_exploration_content `
  --canonical $env:SF2_PRIVATE_CANONICAL_MAP_IMPORT `
  --upstream $selectedUpstreamRoot `
  --selection remake/reference/inputs/map3-programs.json `
  --output $env:SF2_PRIVATE_EXPLORATION_CONTENT

uv run sf2 verify engine
uv run sf2 verify adapter
uv run sf2 verify reference-host

$env:SF2_REQUIRE_PRIVATE_TESTS = '1'
# Also select the existing private encounter/static/enemy/gold inputs and battle01-actions.json.
& $env:DOTNET_BIN test remake/tests/Sf2.Remake.Engine.Tests/Sf2.Remake.Engine.Tests.csproj `
  --configuration Release --no-restore --filter 'FullyQualifiedName~PrivateExplorationTests'
```

For an ordinary interactive source start, use
`--path remake/game -- --private-exploration-start <absolute controlled-start JSON>`.
`SF2_PRIVATE_CONTROLLED_START` selects the existing separate battle party/accounting input;
`SF2_PRIVATE_EXPLORATION_CONTENT` selects the prepared world. The tracked `map*-start.json` files
provide explicit starting alternatives. The authored exploration packages use `--authored-package`.

For the native comparisons, build the ordinary Debug project, select a fresh output path in
`SF2_EXPLORATION_OBSERVATION_OUTPUT`, and run the retained Godot executable with
`--headless --path remake/game --fixed-fps 60 --script res://probes/engine_exploration_observation.gd`
for each authored package. The private observer is
`res://probes/engine_private_exploration_observation.gd`; its external
`SF2_PRIVATE_EXPLORATION_CASE` selects `map3-sarah`, `map3-gate`, `map3-messenger`, `map3-sprite-init`,
`map21-guard`, `map40-intro` or `map40-seen`. Supply the matching `*-start.json` through the ordinary
startup option. `SF2_PRIVATE_EXPLORATION_PLAN` selects the existing
`tests/fixtures/h3/map3-battle01-player-ready-v1.json` for the Map40 input plan. That fixture is read
only by the observer/comparison, never by the game. Require a successful process exit, `passed: true`
and no script/process errors. No screenshots or state setters are used. The existing planner/CI selectors register these consumed
controlled starts and both H3 fixtures. The offline content frontend and selection use the existing
engine/shared-wiring partitions; the committed `--scope engine` plan retains `research-public`
checks and excludes unchanged original H2/H3 runtime evidence. No tests of the selectors are added.
