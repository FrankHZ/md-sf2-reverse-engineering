# Remake Implementation Architecture

## Purpose

This document describes the current implementation topology and its intended bounded decomposition.
It is an implementation guide, not a replacement for the normative decisions in
[ADR 0011](../../docs/decisions/0011-phase4-remake-runtime-architecture.md) and
[ADR 0017](../../docs/decisions/0017-heavy-boundaries-light-internals.md).

The architecture is a deterministic modular monolith hosted by Godot. It is not a scene-owned game,
a service mesh, a general ECS, or an emulator-backed gameplay core.

## Production Assemblies

| Assembly | Current responsibility | Dependency direction |
| --- | --- | --- |
| `Sf2.Remake.Domain` | typed values, immutable state, deterministic reducers, map traversal, working-layout mutation, inventory rules, and bounded Battle01 initialization | .NET base libraries only |
| `Sf2.Remake.Application` | `GameSession`, semantic commands, orchestration, content ports, admission compatibility, snapshots, cues, and diagnostics | Domain |
| `Sf2.Remake.Content` | tracked public-synthetic and ignored private-local readers, fixed identity checks, closed parsing, semantic validation, and mapping | Application and Domain |
| `Sf2.Remake.Godot` | profile selection, dependency composition, `InputMap`, scene/view projection, local diagnostics, smoke hosting, and platform lifecycle | Application, Content, Domain, and Godot |

Dependencies point inward. Tests and repository gate hosts are consumers, not production dependencies.

## State and Command Flow

The logical flow is:

```text
device input
  -> Godot semantic input adapter
  -> GameSession command admission
  -> deterministic Domain transition
  -> authoritative Application snapshot and ordered observations
  -> Godot presenter and disposable scene state
```

`GameSession` is the only logical gameplay mutation facade. Godot may request a command or project a
result; it does not change position, flags, inventory, request state, RNG, or flow state directly.
Content constructs admitted immutable definitions but does not mutate a running session.
For the private canonical Map 3 port, the import definition is the sole owner of the selected setup's
ordered entity population and the private snapshot exposes that exact immutable object. Content maps
source-shaped records to stable resource/ordinal identities, raw and masked coordinates, opaque facing
and map-sprite values, and a defensively owned opaque tail. Application does not receive source
addresses and does not promote those bytes into visibility, walking, action, interaction, or rendering
rules.

The public-synthetic and private-local profiles share the assembly direction and Godot host. They do
not share a weaker content reader or silently convert one profile into the other.

## Current Implementation Shape

The implemented Domain boundaries are already independently testable and engine-free. Application
owns public-synthetic session lifecycles plus a separate admitted private Map 3 traversal state behind
the same logical `GameSession` facade. Content readers own tracked synthetic packages and fixed
private-local admission for canonical maps, offline visual payloads, and selected Battle01 startup
data. Playable PrivateLocal composition consumes the canonical import plus the separately reviewed
local presentation pack; it does not reopen ROM or extraction metadata at startup.
The canonical reader also admits the selected setup's entity population behind the same fixed private
trust boundary. This is typed session context only; neither `GameSession` nor Godot currently creates
mutable entity lifecycles or presentation from it.

Controlled Battle01 initialization is a distinct Domain transition, independent of the synthetic
`TacticalBattle` duel. Application maps the admitted startup definition and named effective party
preset into Domain values. `GameSession.InitializePrivateOriginalBattle01` validates the exact
current Prepared/Pending, performs all potentially failing projections, then consumes Pending and
installs one `PrivateOriginalBattle01SessionSnapshot`. Its `Battle` owns current Map57, roster,
terrain/occupancy, phase-dependent AI/region state, RNG and round order.

`PrivateOriginalFlowStage` and `PrivateOriginalCurrentMap` identify the live private state.
After initialization, `PrivateOriginalMapSnapshot` and locomotion access reject; all old movement,
animation, interaction and bridge mutation entries consequently close. The old backing map remains
only as the private-profile marker and the exact frozen `SourceSnapshot` under battle provenance,
alongside source locomotion and bridge. Its historical Exploration flow and Map40 identity do not
describe the current map. A new session starts at controlled Map3 without Pending or Battle01.
No broad snapshot-reducer refactor or second mutable battle authority is introduced.

The Godot consumer branches on private flow before reading an exploration snapshot. The initialization output is the
next round-entry input, before activation, spawn admission, turn ordering or actor selection. Its
fixed enemy difficulty and already-refreshed effective ally policies are recorded in the
[owning plan](./map03-playability-plan.md#implemented-controlled-initialization).

`EnterPrivateOriginalBattle01FirstRound` accepts only the exact current battle snapshot in
`BeforeFirstRound`. Domain projects enemy activation, the fixed empty Battle01 cutscene/spawn
routes, and turn generation in that order; Application then replaces the single current snapshot.
The same phase-capable battle state now owns the updated `AiBitfield`, region flags, separate
`NewlyTestedRegionMask`, `RandomSeedImage` and `FirstRound` buffer. `InitializationAiBitfield`
retains only the source-derived initial value. `GeneratorWord` is derived from the image's high
16 bits; it is not another RNG authority. The low word remains unchanged. Independent nullable
`RandomSeedCopy` comes from the named controlled preset as1234 and is retained across the round
and both player turns. Its retention policy does not establish the original natural seed lifetime.

The completed phase is `FirstRoundGenerated`, with a full 64-slot signed/stable buffer and current
turn offset0. `FirstCandidate` is data consumed by the distinct control entry, not an executed action. Old
immutable snapshots and Prepared inputs remain provenance; none is exposed as another current map
or live round state. Duplicate/foreign/stale/invalid requests commit nothing, and old exploration
commands remain closed. The [round policy](./map03-playability-plan.md#implemented-controlled-first-round)
records the RNG storage interpretation and the exact shared comparison seam.

`EnterPrivateOriginalBattle01FirstControl` consumes the exact current generated snapshot and
requested actor identity. It classifies the real first candidate without skipping or reordering the
buffer, then constructs the supported class4/HEALER range. A named caller preset supplies only that
candidate's missing ally activation word and fixes the two control toggles; existing activation words
and current status fields retain authority. The supplement and complete movement projection commit
together. AI, disabled action and unsupported-profile branches return unavailable without mutation.

`Battle01InitializedState.FirstControl` owns the player movement selection. The battle roster's live
`Position` is separate from immutable source `Deployment.Position`. The range keeps an explicitly
historical `OriginOccupancy` projection, raw-cost/reachable grids and opponent-blocked terrain; current
occupancy remains `Battle.Occupancy`. Preview only changes the cursor/path. Confirm atomically changes
live position/occupancy and enters `PlayerActionChoice`; cancel restores the turn origin and returns
to `PlayerMovementSelection`. No action or turn is submitted by either transition.

The three movement session facades enforce exact snapshot, actor and phase before a single replacement.
`Battle01MovementPath.PolicyId` identifies the controlled preview policy, whose selected path cost is
checked against both the propagation cost and budget. This is not original cancellation-animation
execution. The [owning policy](./map03-playability-plan.md#implemented-controlled-first-player-api)
records the source-contract disagreement and bounded scene/preview rules.

`PrivateBattle01Composition` is the thin Godot consumer. Its three all-or-none private path options
construct the existing Content reader; only explicit N at the current Pending starts preparation,
initialization, first round and first control. Each call uses the returned current snapshot and
current turn-buffer candidate. Rejection displays its stage; after initialization the view remains
on the actual battle even when later entry fails. No retry rerolls or reinitializes an existing battle.
The process router prioritizes battle state over exploration and the synthetic bridge, and physics
checks the battle flow stage before reading the closed exploration locomotion getter.

`PrivateBattle01Presenter` projects immutable live positions, terrain/range and preview data into
24-pixel grid geometry. It owns no cursor, roster, RNG or turn state. Root hides the complete old
Map40/HUD/synthetic canvas subtrees before attaching it. Existing I/J/K/L, Space and Backspace keys
route once through the private battle poller; action choice accepts STAY or cancellation.

`CommitPrivateOriginalBattle01Stay` requires the exact current snapshot and the accepted comparison
party, then applies the named controlled unchanged-effective-stats policy. Domain checks the defeated
wrapper's early return, empty death cleanup, both factions, no-effect after-turn admission, another
empty cleanup and both factions again before advancing. It retains the same immutable stats and
live roster/occupancy, clears `FirstControl`, and installs a `TurnCompletion` receipt with both counts.
The movement range retains its actor's immutable entry stats for drift rejection; it is not a second
stat authority. A previously receipted HP0/unplaced enemy remains cleaned on later no-effect turns;
it is not a fresh worklist entry and receives no repeated award. Generic stat refresh remains outside
this bounded fixed-profile consumer.

`FirstRound.CurrentTurnOffset` is a raw byte offset; `CurrentCandidate` uses offset divided by the
two-byte entry size. Advancing from 0 to 2 shares the complete unchanged 64-slot order and preserves
the historical `FirstCandidate`. Each completed player advances exactly one entry; offset2 becomes4.
`PlayerTurnCompleted` is the committed STAY boundary. `EnterPrivateOriginalBattle01NextPlayerControl`
requires that exact snapshot and actor, then reuses the first-control classifier, candidate-only
missing-word preset and range/preview construction. The first-control API remains restricted to
offset0. Fixed class1/CENTAUR2 and class0/REGULAR1 join class4/HEALER12; current effective MOV determines the budget.
The new range captures live occupancy, so its cancel restores only this actor's turn origin.

The previous immutable completion receipt survives next entry and movement; a later receipt links it
through `Previous` as provenance. Active control determines the movement phase and presenter overlays
even when that historical receipt exists. The UI invokes next entry once after each successful STAY,
with no extra N. Unavailable or rejected entry leaves the exact committed STAY object in place and
shows the actual candidate/reason; completed-phase key handling returns no replacement status.
There is no frame retry or actor skip. A bounded dispatch follows actual slots, permits at most one
new round per call and yields as soon as an actual player is ready. At first-round offset4, actual
enemy128 / OpponentAi begins six enemy completions. `CompletePrivateOriginalBattle01EnemyStandby`
keeps strict first128 admission; subsequent entries require the actual candidate, accepted completed
prefix and chained seed-copy/own memory. Domain runs the inactive regular branch's thinking RNG,
standby tables, raw Hovering6 grid plus separate live occupancy and bounded source move string.
The separate seed-copy, own memory, live move and shared no-effect completion are local allocations
until all checks pass and Application replaces one snapshot. A later rejection retains the last
successful actor and every prior receipt; the relay stops with that actor's precise diagnostic.

The first128 move to(6,3) produces seed-copy3934 and memory[0]=14h. The remaining actual order
131,133,129,130,132 consumes evolving occupancy and ends with seed-copy0134, while mainA4991234,
all effective stats and the full turn buffer remain unchanged. Only the complete eight-receipt
prefix at offset16 admits Bowie0 through the shared next-player API. Its candidate-only missing-word
policy and Regular1 range give independent move/cancel/STAY with budget12. Bowie's completion adds
the ninth receipt and reaches the existing first sentinel at offset18. The same dispatch invokes
`EnterPrivateOriginalBattle01NextRound`: local activation from current allies and retained flags/tested
mask, empty region-cutscene/STARTING-spawn admission, then signed stable ordering from current main RNG.
It admits coherent primary-region flags for the six starting GIZMOs and rejects unsupported flags,
secondary orders and actor state before installing flags/order/RNG. The explicit roster-only policy
excludes the original activation routine's deals-memory alias. Successful generation retains
positions, effective stats, deployment anchors, memory, seed-copy and every previous receipt.
`RoundNumber` belongs to the current order and each immutable receipt; `RoundGenerated` distinguishes
new order from historical completion. Later control/standby admission validates the actual current
generation prefix and retained thinking history. Standby uses the live origin/grid and original anchor,
including nonzero own memory. Each generation or actor transition has one Application commit.
Failed control after generation preserves that generated round; later enemy failure preserves the last
successful actor. The presenter shows the current candidate or completed actor with the precise
diagnostic and no stale movement overlays; input cannot retry a rejected dispatch.
The same actual-candidate relay routes active commandsets6/7 to
`CompletePrivateOriginalBattle01EnemyPursuit`. Domain first scans physical eligibility on the
opponent-blocked budget10 grid. A nonempty cohort returns a typed attack-selection requirement before
priority, RNG or any commit. Otherwise raw budget128 target costs, the source accumulated-mask walk,
radius0/1 occupancy fallback and final reverse/invert move string feed shared no-effect STAY.
Exactly one typed standby, pursuit or physical decision belongs to each enemy receipt; player
receipts carry none. Pursuit retains memory and seed-copy, which backward history validation checks
alongside standby thinking. A source-valid empty/fallback move completes origin STAY; malformed paths reject.

The relay consumes the pursuit classifier's physical-cohort boundary once through
`CompletePrivateOriginalBattle01EnemyPhysicalAttack`. That facade independently revalidates the
current snapshot, cohort, priority, source path and supported combat profile. Its Domain reducer
constructs the temporary HP effect, restores the source snapshot and replays one semantic reaction
locally before physical cleanup/after-turn validation and one Application replacement. The explicit
class0/Wooden Sword profile supplies prowess3; regular GIZMO prowess0, difficulty0/type2 and the
two independent RNG channels retain their source semantics. Miss/ordinary critical results are
calculated. True validated follow-ups, lethal/status/curse/reward and unsupported after-turn effects
reject atomically. This is a bounded consumer, not a general battle-scene VM.

The physical completion policy is distinct from strict STAY. History rewinds physical main-RNG
changes before comparing earlier pursuit stamps, links thinking/last-target/memory and HP effects,
and retains the generated64-slot order. HP copies preserve all non-HP fields; startup/prepared
stats remain provenance. Subsequent movement/cancel/STAY, admitted AI and round generation retain
the damaged current stats. Actual round6 attack132 produces HP12→9 and yields Bowie0 control;
the presenter derives its persistent current-round attack summary from the existing receipt chain.
Original scene animation/timing, broader profiles, multi-strike/counter/death and victory remain open.

Manual player attacks use `Battle01PlayerPhysicalAttack` and a distinct player physical/EXP policy.
The existing movement selection gains a target stage, holding the ordered live down/right/up/left
range1 cohort. Target cancellation returns to the provisional action choice; movement cancellation
then restores the origin. `PrivateOriginalBattle01PlayerPhysicalAttack` requires the exact session
snapshot and a separately named `PlayerAttackComparison` or `FirstDefeatComparison` preparation.
Nullable current EXP/gold/kills distinguish unspecified inputs from explicit authored zeroes.
Godot uses the latter supplement; both earlier presets remain unchanged. Immutable copies preserve
every channel, while preparation retains HP12/EXP0/gold0/kills0 as provenance.

Confirmation rechecks the actor, live target list, occupancy and admitted profiles before resolving
the player's dodge8/critical16/+quarter damage, two spread calls, both follow-ups and EXP award.
Local HP restore/reaction and EXP replay publish together only after completion validation; required
level changes and actual extra attacks reject the entire action. The older policy still rejects
lethal results. The first-defeat policy permits one regular enemy death with known accounting inputs.
It keeps full overkill damage, returns before double/counter draws, adds kill EXP50 with cap49 and
Battle01 halving, and applies source gold60 during local construction. HP snapshot restore/reaction,
EXP replay, first worklist kill credit/removal, actor normalization, cleared second worklist and both
3/5 faction counts finish before one Application snapshot replacement. A late failure retains every
old channel. Gold caps at9,999,999 including source carry; Bowie kills cap at9999.

Live battle placement is nullable: HP0/null represents cleaned FF/FF while immutable deployment,
source stats and the pre-death attack row remain intact. Copies never infer a corpse's placement
from deployment. Living consumers require a position, and occupancy/target/presentation omit the
cleaned row. Mixed history
rewinds player HP/EXP without enemy last-target writes, requires the full source HP5 GIZMO profile,
and permits damaged or cleaned enemy HP only with linked reaction/cleanup provenance. It rewinds
placement, EXP, gold, kills and count transitions through the same receipt chain. Same-round
generation validation restores its pre-kill candidate set from receipt before-images; a later
generation excludes the dead row and its draws. Where a preceding
main endpoint is recorded, the existing generator reproduces the following main image and current
64-slot order. This adds no stored seed authority or history cache.

Godot keeps ordinary action-choice Space as STAY; A opens targeting and target Space explicitly
attacks. The existing finite relay reaches round7 player2, then actual131/132/133 and Bowie HP6.
His explicit lethal attack creates receipt59 and immediately yields actual player1 movement/cancel.
The presenter derives the persistent defeated132/+24 EXP/+60 gold result from that receipt and
shows live gold60/kills1, Bowie EXP39 and five enemies.
Eight calls and the E9F01234/CF491234 comparison are construction/award semantics under the
presentation-omitted diagnostic policy: original reaction flags1 also cause24 range7 jitter draws,
and VInt/menu/text timing can change both RNG channels. No original playback state is claimed.

When base art and Battle01 inputs are both requested, the existing catalog requires the separately
accepted Map57 asset transaction and exact bucket. `PrivateBattle01BaseViewProjection` reads only
`Preparation.Pending.Definition` from the current battle: its validated layout, block catalog,
area and visual selection. It rejects definition drift and any referenced unloaded slot, then
reuses `PrivateOriginalMapBaseViewProjection` block/tile sampling for the fixed 16-by-20 area.
The 384-by-480 logical base maps the full 2x/4x raster with nearest sampling. No exploration runtime,
camera, parallel layout or position authority is created. All nine units remain diagnostic markers.
The existing diagnostic mode remains available without requesting art. Missing/wrong requested art
fails visibly; a failed base projection closes input without displaying a substitute battlefield.
Original scene/layers, animation, VRAM persistence, fidelity and other battle actions remain
outside this consumer.

Two areas currently concentrate more responsibility than the target shape:

- `Map3Root` and its private partial own profile dispatch, composition, interactive command calls,
  private smoke scheduling, and private stage tracing. Public-synthetic action registration and polling
  plus private-local movement polling delegate to the internal `Map3InputAdapter`; public-synthetic
  node ownership, formatting, and snapshot projection delegate to the internal `Map3Presenter`;
  private-local and unavailable
  diagnostic node ownership, formatting, and typed viewport projection delegate to the internal
  `PrivateMap3Presenter`; deterministic public and private smoke commands, marker serialization, and
  smoke-only quit delegate to their internal drivers. None of these collaborators owns session or
  gameplay state.
- `GameSession` and its partials own command routing, pending gates, lifecycle handlers, broad snapshot
  construction, private admission, and projection-facing result types.

The Content public seams are appropriately narrow, but each large reader currently combines raw
identity verification, parsing, semantic validation, and mapping in one implementation file.

These are accepted implementation facts, not a claim that their planned refactors are complete.

## Target Internal Delegation

### Godot host

`Map3Root` should converge on profile selection, dependency construction, lifecycle, and wiring.
Internal replaceable collaborators may own:

- `Map3InputAdapter`: public-synthetic `InputMap` actions to semantic Application commands plus
  private-local movement polling to one existing semantic Domain direction (**implemented**);
- `Map3Presenter`: public-synthetic authoritative snapshot to a bounded internal view model, nodes, and
  labels (**implemented**);
- `PrivateMap3Presenter`: display-only private-local/unavailable plans plus authoritative private
  snapshot to diagnostic nodes, status, and the existing typed traversal viewport (**implemented**;
  smoke only reads its current projection);
- `PublicSyntheticMap3SmokeDriver`: the deterministic public command script, stable observation
  serialization, smoke-only failure projection, and quit (**implemented**); and
- `PrivateMap3SmokeDriver`: the deterministic private command script, four stable observation markers,
  smoke-only failure projection, and quit over an already-started session (**implemented**).

Profile selection and composition intentionally remain in `Map3Root`. Public startup consumes tracked
Godot bytes and the public typed admission result, while private startup alone owns the local path,
timed source wrapper, and private typed admission result. Unifying those seams would require a profile
discriminant, optional receipt, callback, or new cross-profile result protocol without removing a real
authority. Their direct typed branches are the bounded composition root, not incomplete delegation.

Presentation helpers may directly construct nodes, choose project-authored diagnostic colors, and
format labels. They remain disposable adapter state and never become another gameplay authority.
The project-authored private battle bridge binds to the already-admitted private session and controlled
start, not to a presentation payload. Presentation pack admission remains an outer composition concern.

### Application facade

Keep one public `GameSession`. Delegate internally to focused collaborators for:

- command dispatch and the single pending-command gate;
- navigation and map transitions;
- interaction, dialogue, search, and acquisition coordination; and
- authoritative snapshot projection.

Internal coordinators do not expose another mutation entry point or retain competing state. Do not
replace explicit lifecycle invariants with a premature universal event framework.

### Content readers

Keep each public trust port and its fail-closed result surface. When an owning change reaches a large
reader, internal stages may be separated as:

```text
raw identity verification -> parse -> semantic validation -> admitted mapping
```

The reader still owns ordering. A split must not add an alternate parser, caller-selected trust root,
public byte factory, silent fallback, or shared abstraction that weakens public/private separation.

## Public Surface Test

Under ADR 0017, a new public capability, receipt, or protocol type must identify at least one durable
reason:

1. untrusted bytes, path, profile, provenance, or redistribution trust;
2. authoritative mutation, deterministic transition, or state invariant;
3. an independently versioned cross-assembly port; or
4. a stable H4, replay, export, smoke, or compatibility observation.

Otherwise the implementation defaults to internal types and direct calls. A new diagnostic field
extends an existing bounded inspector or observation unless it has an independent boundary reason.

## Refactor Sequence

Future refactors remain serialized and behavior-preserving:

1. treat the current Godot host cluster as complete: public-synthetic and private-local input,
   presentation, and smoke are delegated, while composition remains intentionally direct and profile
   selection, interactive commands, marker bytes, stage tracing, and observations stay preserved;
2. extract Application dispatch, pending gates, and snapshot projection behind `GameSession`; and
3. split a Content reader internally only when an owning change requires it.

Do not combine those refactors with a new gameplay feature or a repository-wide public-type rewrite.
Characterization tests must prove existing commands, snapshots, profile failures, and smoke output remain
unchanged before a refactor is accepted.

## Review Questions

- Does authoritative state or mutation still have exactly one owner?
- Does each new public protocol name a real boundary reason?
- Can the Godot view be reconstructed from authoritative observations?
- Does a small behavior change require unrelated cross-layer edits?
- Are trust validation, orchestration, and presentation concentrated unnecessarily?
- Does failure remain attributed to the correct Content, Application, Domain, or Godot layer?

File length and class count are signals, not acceptance criteria. More wrappers do not improve the
architecture unless they reduce responsibility concentration or change amplification.
