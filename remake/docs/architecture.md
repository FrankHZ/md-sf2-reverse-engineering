# Remake Implementation Architecture

## Purpose

This document describes the current implementation topology and its intended bounded decomposition.
It is an implementation guide, not a replacement for the normative decisions in
[ADR 0011](../../docs/decisions/0011-phase4-remake-runtime-architecture.md) and
[ADR 0017](../../docs/decisions/0017-heavy-boundaries-light-internals.md).

The [architecture and verification audit](./architecture-audit.md) records findings at its named
historical baseline and the user's modern-engine direction. Its A1–A8 are not a fresh defect list.
The [logic-separation plan](#replaceable-gameplay-logic-plan) below owns the finite #617 scope.
Its HEAL slice is implemented; the remaining action/AI/progression/story slices are planned.
Implementation and authored demonstrations do not establish new original-game evidence.

The architecture is a deterministic modular monolith hosted by Godot. It is not a scene-owned game,
a service mesh, a general ECS, or an emulator-backed gameplay core.

## State and Content Direction

[ADR 0019](../../docs/decisions/0019-state-and-content-driven-remake-engine.md) owns the adopted
direction for the command/state/result model, typed content and resumable programs, audit A1–A8 mapping,
new-engine unit tests, direct reference verification, old-test/CI retirement, and incremental migration.
Live state, validated content and implemented capability admit commands on M1's authored battle
path. The M2 physical rule chain uses that same dispatcher and state. Application advances until real player input or an automatic-work tick boundary; Godot consumes
semantic commands and observations. The common exploration/program path now retains typed dialogue,
entity, tick and presentation waits; unimplemented native presentation stays pending with an adapter
failure. [Exploration and programs](./exploration-programs.md) owns its content, execution and source
frontiers. The connected Battle01 outcome/after-program/ordinary-defeat return now uses the same
session. Natural original-route fidelity and broader outcome families remain incomplete.

M0 implements consumed internal Domain RNG, ordinary priest healing arithmetic, turn-order generation
and Manhattan action range, with a dedicated engine unit project and scoped verification entries.
`PhysicalStrikeRules`, `BattleRewards` and `PhysicalBattleAction` now own ordinary physical construction,
settlement and atomic state transition. `PhysicalBattleAction` constructs at most three source-ordered
hits on temporary HP, carries sticky reaction decisions and aggregates one ally award before publication.
Both player commands and `EnemyPhysicalDecision` call that calculator; `BattleActionCommitter` is
the single publication/queue-consumption mechanism. Automatic advancement catches failures at each
enemy ACTION, preserving earlier commits and retaining the failed enemy's queue entry. The semantic
attack-then-approach strategy scores all reachable physical targets in reverse processing order and uses
shared `PhysicalTargetRules` for signed raw-priority cohorts, class selection and movement ties.
The existing class definition supplies source identity only for admitted named classes; missing
identity rejects a reached critical comparison. Regular movement fixes the class table; content
cannot supply an independent rank. Source thinking, scoring and selection have one production owner.
`AttackThenApproachAi` sequences that decision and its zero-target continuation: unavailable
HEAL1/SUPPORT fail, then MOVE1 succeeds with movement or origin Stay. `AiMovementRules` owns the shared
stable cost selector, source decreasing-cost walk and radius station search used by authored and
private pursuit/standby. The existing movement commit and action publisher apply
the chosen destination once. MOVE1 draws no RNG and leaves last-target memory and resources unchanged;
physical rewards are required only on the reached attack branch. High or incomplete target costs
remain Unsupported, as does wider AI. Private region activation has its separate bounded owner below.
Shared strike/reward functions remain the calculation owners. Semantic observations carry both actor and target for reversal. Dead combatants retain
identity/HP/kill-and-defeat accounting but have no
battlefield position; occupancy and presentation read that authoritative state.
The [current M1 boundary](../../docs/decisions/0019-state-and-content-driven-remake-engine.md#current-m1-implementation)
adds movement/cancellation, common session/content admission and connected authored battles.
Profile-specific snapshots, fixed import checks, endpoint handlers and old Godot battle dispatch
were retired with the reference runtime at
[M5](../../docs/decisions/0019-state-and-content-driven-remake-engine.md#current-m5-implementation);
A1–A8 closure remains tracked by the audit. Public/private trust and the accepted composed 8D/H4
milestone remain distinct from the unsupported broader capabilities in this migration.

Encounter deployments own explicit `BattleFaction` and unique integer `ProcessingOrder`; intrinsic
actor definitions and session starts do not duplicate those roles. The definition sorts deployments
once for stable round RNG, AI candidates and adapter selection. Runtime actors derive faction/order
from their deployment; queue entries identify actors by `ActorRef` with a nullable sentinel. Faction drives
healing/opposition/rewards independently of order. Deployments also own `BattleControl` separately from
`BattleAiStrategy`; intrinsic actor definitions carry neither assignment. Player requires no strategy;
automatic authored actors explicitly select Stay or AttackThenApproach. Private source orders remain
SourceOrders and use the bounded source standby/activation/set6/set7 continuation. Shared deployment validation at Content
admission and reusable start preserves ally/player and enemy/automatic bounds plus physical/spellbook/
MOV requirements. The advancer consumes explicit combinations and never defaults an unknown policy to
Stay. The native view projects each actor’s current AI memory and immutable source anchor; there is no second control copy.
Actor definitions separately own numerical `Agility` (0–127) and boolean `ExtraRoundAction`.
Live-start capacity and shared generation consume explicit eligibility; only private Content decodes
the raw source byte's low seven bits and high-bit eligibility. The ordinary three draws and optional two draws at the
truncated five-sixths basis retain source arithmetic and signed sentinel ordering. The existing
advancer consumes both entries without an additional scheduler or physical double/counter changes.

`PhysicalCriticalRule` owns the two supported immutable chance/bonus definitions; Content selects
one from explicit semantic fields rather than a packed prowess number. Every actual hit, including
reversed counters, reads its attacker's selected rule. The scalar strike calculator already takes
mathematical operands and remains unchanged, as do its private source mappings. Other
physical fields retain their existing semantic ownership; no generalized profile system is introduced.

`BattleTerrain` holds immutable surface and independent protection; layout glyphs resolve through
explicit local definitions. `BattleTerrainRules` interprets them for the currently admitted
regular/healer/Centaur/hovering movers and target land reduction. Content/start, preview/commit, AI pursuit/scoring
and physical hits use that owner. Temporary opponent blocking changes only a fresh per-cell cost
array; friendly traversal and occupied stopping remain movement policy. Godot projects surface into
existing placeholder colors without supplying gameplay properties.

`WeightedMovement` consumes decoded signed cell costs, preserving its one first-admission/LIFO bucket
algorithm with logical row boundaries; the original flat-storage probe across a row seam is not
reproduced. Private Content maps the selected raw terrain to these semantic surfaces while retaining
the complete source encounter for later consumers.

## Initialized Private Entry

`PrivateBattleScenarioReader` composes the existing trusted encounter reader, pinned selected
`PrivateBattleDefinitionReader`, and external `ControlledBattleStartReader`. Selected classes, items,
spells and enemies retain their source fields/provenance in immutable `PrivateBattleDefinitions`;
there is no global catalog or production preset. The admitted `ScenarioDefinition` and explicit
`BattleStartInput` enter the same `Runtime.GameSession` and `BattleAdvancer` as authored content.
The session retains its admitted definition and never rereads files while running.

`BattleInitializationRules` handles the bounded new-battle policy: living status-free, already
refreshed/equipped allies, difficulty0, STARTING placements, controlled intro skip and roster-only
storage. It restores HP/MP, retains ally effective ATT, computes source enemy ATT plus its truncated
quarter once (source0–204), and clears AI memory/last targets/region state. Runtime `Attack` is distinct
from immutable source `Definition.Attack`. Missing EXP/kills/defeats/gold remain nullable; the actual
accounting operation rejects Unsupported before publication if its required value is unknown.

`BattleActivationRules` tests inclusive region geometry, preserves assigned activation word bits,
admits the no-region-program/no-hidden-spawn boundary, then `BattleTurnFlow` calls the shared queue
calculator with carried RNG. `BattleControlRules` classifies the actual generated candidate and applies
only the explicitly declared missing-candidate ally word. Other unknown ally words remain unknown.
Initialization/first supported control is one entry transaction: failure publishes no session or partial
RNG state. Later failures preserve earlier completed commands and the failing queue entry.

Godot selects this source with `--private-battle-start`; the existing battle view sends the same
movement/cancel commands and projects private origin and Unknown accounting. `SourceEnemyAi` uses
`AiStandbyRules` for anchor-relative standby and `AiMovementRules.Pursue` for active set6/set7. The source legal grid feeds
the common movement commit, and `BattleActionCommitter` publishes each completed action once.
Per-enemy memory/activation, retained region flags, cleared tested mask and independent thinking RNG
remain live state; no round or receipt admits a turn. An actual physical cohort uses Content-bound prowess, weapon range/effects and enemy gold through
`EnemyPhysicalDecision` and `PhysicalBattleAction`. Hovering selects Flying target priority and airborne
dodge while retaining terrain protection. Explicit controlled start accounting enables ordinary
rewards/deaths; unknown required counters reject atomically. `BattleActionCommitter` emits one
after-turn pass for each continuing action, whose admitted status/equipment require no further change. Terminal actions enter the outcome program before any after-turn or queue advance.
The connected map programs and Battle01 outcome/return use the common session.

## Production Assemblies

M1's new path is organized by cohesive responsibility: Domain `Battles/Rules` and `Battles/State`,
Application `Content/Scenarios` and `Runtime/Battles`, Content `Scenarios`, and Godot `Battles`.
`Application.Runtime.GameSession` is a plain thin entry/lifecycle/state publisher; independent
collaborators own command dispatch and automatic battle advancement. New partial files are not a
substitute for those responsibilities.

Godot's [map viewport](../game/src/Battles/BattleMapViewport.cs) owns disposable board nodes, clipping
and framing derived from the action's origin, preview path and target. It pans and, when necessary,
zooms to keep that context visible; no map size or actor placement is restricted by HUD coordinates.
The view arranges a separate scrollable HUD beside the map in wide windows and below it in narrow
windows. Authored UI follows the actual viewport. A target-cycle cursor stores only the last attempted UI candidate, so an engine range
rejection cannot trap navigation. The accepted target remains exclusively in the session snapshot.

The old scenario-bound implementation, its Godot host and the legacy test projects were retired at M5.
Only external controlled comparison inputs remain under [`reference/`](../reference/README.md). New
sessions never require a Map3/Battle01 class or ADR0009 trace field. Retirement alone does not close
A1–A8 or establish fidelity; the current composed milestone has its separate acceptance owner.

| Assembly | Current responsibility | Dependency direction |
| --- | --- | --- |
| `Sf2.Remake.Domain` | typed immutable battle state, RNG/healing/physical/reward/range/turn/movement rules; working layout, block-copy, traversal, setup selection, entity allocation and motion reducers | .NET base libraries only |
| `Sf2.Remake.Application` | thin `Runtime.GameSession`, common contracts, independent command dispatcher and automatic battle advancer, typed scenario port | Domain |
| `Sf2.Remake.Content` | authored and selected private input admission, source definition resolution, numeric and capability validation | Application and Domain |
| `Sf2.Remake.Godot` | ordinary GameRoot startup, common battle input/view projection and lifecycle; read-only state diagnostics | Application, Content, Domain and Godot only |

Dependencies point inward. Tests and repository gate hosts are consumers, not production dependencies.

## Ordinary Godot Host

[`game/Main.tscn`](../game/Main.tscn) binds [`GameRoot`](../game/src/GameRoot.cs). Its actual compile
items are the ordinary composition and common battle views, with only the three production project
references. Default, `--authored-package` and `--private-battle-start` all use the common source/session
path. Unknown, duplicate, conflicting and missing-path options fail before any session publication.
The external GDScript observer owns `SF2_OBSERVATION_*` settings; the game never parses diagnostic
case/output/shape options. The read-only view endpoint remains ordinary diagnostic support.

The external H4 probe can install `ObservationCapture` before Main. Existing view/result, audio
receipt and actual draw boundaries then publish copied primitive facts or newly built immutable
facts to one ordered writer. The worker owns UTF-8 serialization and file I/O; it cannot read
Godot nodes, live sessions or engine clocks. Native identities remain separate from capture
sequence/channel ordinals. Texture descriptors follow the actual view/resource lifetime, while
requirements and uses retain separate channels. Failure disconnects observation callbacks and
leaves an explicitly incomplete prefix. Offline comparison uses SQLite ordinal/native/resource
joins with detached published records; each saved report owns one relocatable companion database.
See the [capture protocol and reader](development-and-verification.md#bounded-h4-capture).

The reference host and its public-synthetic import/export smoke were retired at M5. Export configuration
excludes ordinary probes; a complete ordinary package/export is not claimed. The opening/messenger,
castle/tower, Battle01 entry and outcome consumers use common programs, physical entity slots with
logical aliases, joined/active flags, followers and typed presentation completions. G6 and bounded
private action binding use the common player interface. H cycles actual learned spells/levels;
selection and resources remain session authority. No second gameplay scheduler or state authority
was introduced.

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

`Application.Runtime.GameSession` is the common engine's only logical gameplay mutation facade. Godot may request a command or project a
result; it does not change position, flags, inventory, request state, RNG, or flow state directly.
Content returns reusable immutable `ScenarioDefinition` encounter/deployment/rule data and a separate
explicit `BattleStartInput`, or map/program definitions with an `ExplorationStartInput`. The
exploration/battle active-state union and outer story continuation have one snapshot owner.
`ExplorationDispatcher`, `ProgramRunner`, `EntityActionRunner` and `MapTransfer` compute results;
the facade alone publishes them. The source-based session entry delegates to the same definition-plus-start
entry used for another controlled start. Shared binding validates references/resources before creating
fresh runtime actors; definitions contain no instantiated actor state, seeds or counters. Genuine
encounter deployments remain content, while controlled position overrides belong to start inputs.
The [implemented split and remaining model work](../../docs/decisions/0019-state-and-content-driven-remake-engine.md#authored-definitions-and-explicit-session-starts)
supports the initialized private entry above. Content does not mutate a running session.
## Retired Legacy Implementation

The controlled-route reference engine audited in the [architecture audit](./architecture-audit.md) —
divergent public-synthetic/private-local facades, fixed presets and history predicates, endpoint
assignments and Godot-owned turn scheduling — was retired at M5 after every reached private capability
ran through the common session. Its last state is available at the accepted pre-M5 base
[`8a581a82`](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/8a581a82e297ea2947cc9837e661752163d2806d/remake/reference/README.md). The
[Map 3 implementation/reference record](./map03-playability-plan.md) remains historical evidence of
those controlled routes, not current code or an acceptance rule. The public tactical micro-battle used
project-authored simplified rules and was never an SF2 oracle.

## Target Internal Delegation

### Godot host

The ordinary `GameRoot` only selects a common source and attaches the common battle or exploration
view. Startup selection stays inline there because it joins distinct ports and results.

Presentation helpers may directly construct nodes, choose project-authored diagnostic colors, and
format labels. They remain disposable adapter state and never become another gameplay authority.
Presentation data enters through the prepared private world; admission remains a Content concern.

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

Choose the scope explicitly: an internal behavior-preserving refactor retains meaningful observable
behavior, while ADR 0019 directs behavioral migration away from fixed histories and incompatible
runtime paths. M1's common authored battle uses independent runtime collaborators. M2/M3/M4 migrated
real rules/content/programs, and M5 removed the remaining reference families with their last callers.

Use small engine unit assertions for the actual behavior being moved. Retire obsolete structural,
helper and trace-refusal tests as their owners migrate; there is no requirement to preserve every
command name, private snapshot shape, smoke marker, test method or old green aggregate. A reference
observation that is still consumed keeps a deliberate migration boundary outside production.

Extract internal command/control/projection responsibilities behind the single state authority and
split readers only where a concrete change needs it. Preserve genuine Content trust, atomicity and
Unsupported boundaries. The order is driven by the coherent behavioral dependency in ADR 0019,
not an obligation to finish all file splits before correcting an engine design defect.

## Review Questions

- Does authoritative state or mutation still have exactly one owner?
- Does each new public protocol name a real boundary reason?
- Can the Godot view be reconstructed from authoritative observations?
- Does a small behavior change require unrelated cross-layer edits?
- Would another valid history, actor or content package require production case-ID/round/receipt edits?
- Is a fixed reference predicate being mistaken for a rule, or a story endpoint for executed control flow?
- Do the selected checks observe actual engine behavior without testing the verification machinery?
- Are trust validation, orchestration, and presentation concentrated unnecessarily?
- Does failure remain attributed to the correct Content, Application, Domain, or Godot layer?

File length and class count are signals, not acceptance criteria. More wrappers do not improve the
architecture unless they reduce responsibility concentration or change amplification.

## Replaceable Gameplay Logic Plan

### Agreement and stopping boundary

**Accepted design for [#617](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/617),
with the bounded HEAL implementation in [#674](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/674).** Rigor is
**High**: action policy, source compatibility, shared RNG, content admission and presentation cross
owners; a wrong commit boundary would cause expensive behavioral rework. This means finite scope,
explicit dependencies and independent acceptance, not a new rules platform.

The product outcome is that a project author can replace the implemented action algorithms, AI
selection and conditional story behavior in ordinary C# and typed content, rebuild, and start a new
session without editing generic command/commit machinery or adding policy-name branches to Godot.
Preserve the single authoritative state, accepted source semantics, private provenance and current
[composed milestone](../../docs/design/synthesis/map3-battle01-readiness.md#accepted-current-milestone).
The [accepted gameplay overview](../../docs/design/synthesis/gameplay-overview.md) explains the
connected product; it does not extend the runnable capability frontier.

The HEAL slice delivers selected rules, immutable session composition, disposable query choices,
finite preparation validation and the existing staged scene consumer. Other action families, AI,
progression/outcome and source story policy still require their dependent slices and independent review.
#438 art/UI/UX awaits user discussion; this plan
supplies semantic affordances only. #638 remains paused and Chinese synchronization is deferred.

Non-goals are hot reload/state migration, an interpreter/VM, dynamic assembly discovery, a plugin
registry, a generic RPG effect language, ECS, new game capabilities, save/load, a second state owner,
and another comparator platform. Compile-free script authoring remains **Unknown** as a future
requirement; neither this choice nor excluding hot reload answers that different question.

### Pre-migration source evidence and caller ownership

**Confirmed — source structure only**, inspected at accepted `eec701e21bc0978d0bd9ba7c47c809a721f46ec9`.
The subsequent accepted `70767a32df59e83d6fb0aac254e5907a89edf77e` changes only overview acceptance
wording and its roadmap. Reproduce a trace with `git show <object>:<relative-path>` and the symbols
below. The historical paths in this table describe that accepted object; use `git show` for files
since moved or retired. The current HEAL owner is [below](#implemented-heal-seam).
Broader runtime equivalence and performance remain **Unknown**. These observations do not establish new facts about the original ROM.

| Rechecked concern | Concrete current path | Consequence for this plan |
| --- | --- | --- |
| S1: formula and rule selection | `GameSession.Start/Submit` reads/adopts a definition once. `BattleCommandDispatcher.Commit` called `PlayerHealing.Prepare`; `PlayerHealing` called `HealingRules.ResolvePriest` and preflighted growth. `HealingRules` owned proportional EXP and the ordered two award draws. There was no injected rule selection. | Move the existing algorithm into the selected implementation; do not copy a second calculator or merely externalize its constants. JSON programs already exist and should remain the story authoring path. |
| S2: action, target and UI coupling | [`SessionContract`](../src/Sf2.Remake.Application/Runtime/SessionContract.cs) fixes Stay/Heal/PhysicalAttack/Item. Dispatcher selection/target/commit branches select their calculators. [`BattleSessionView`](../game/src/Battles/BattleSessionView.cs) constructs candidates from HP/IsAlly/action, cycles raw inventory words using `& 127`, and attempts self-target after spell/item selection. | Application must query the selected rules and return semantic choices. Current submission rechecks range: broad UI candidates are not proof of an existing legality bug. |
| S3: capability versus legality | `PlayerHealing.RequireSpell` combines learned spell/MP checks with `healing-class` and level-based `healing-animation` Unsupported. [`AuthoredScenarioPackageReader.DecodeBattle`](../src/Sf2.Remake.Content/Scenarios/AuthoredScenarioPackageReader.cs) admits level 1–4 definitions but only heal/non-full-recovery effect data. `HealingSceneCursor.Create` checks the private cast/idle resources. | Separate a rule's legal conditions from implemented effect/scene support. Preserve HEAL4 and missing private-consumer rejection; moving a check does not implement the missing capability. |
| S4: source story exception | [`ExplorationContentReader.ReadInstruction`](../src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs) decodes `retired-map3-entity-scratch` into [`RetiredMap3EntityScratch`](../src/Sf2.Remake.Application/Content/Scenarios/StoryProgram.cs). [`ProgramRunner.Run`](../src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs) checks `byte-513a8:1`, map, continuation, closed window, flags and a real entity retirement/tombstone. | Move this exact compatibility policy behind a selected source-policy call. Keep every guard and the actual hide effect; it is not a disposable route assertion. Ordinary branch/call/wait instructions already execute real programs. |
| S5: state, definition and AI coupling | [`EngineBattleState`](../src/Sf2.Remake.Domain/Battles/State/EngineBattleState.cs) includes class/AI enums, class-to-source identity, healing-only definitions and immutable actor/deployment state. [`BattleAdvancer.Advance`](../src/Sf2.Remake.Application/Runtime/Battles/BattleAdvancer.cs) switches AI enum to `SourceEnemyAi`/`AttackThenApproachAi`; `BattleTurnFlow.ValidateDeployment` and Content repeat strategy admission. | Retain immutable state and control/faction/order separation. Resolve rule bindings at composition, remove strategy dispatch from the scheduler, and relocate source class mapping. A general spell/status/property-bag model is not needed for the finite implemented set. |

The ownership traces that constrain the replacement API are:

- **Composition/content:** [`GameRoot`](../game/src/GameRoot.cs) chooses the ordinary source;
  `BattleSessionView` also has a standalone source-start entry. `GameSession.Start(IScenarioSource)`
  reads once, then uses the same definition/start overloads. Authored format 7/8 readers validate and
  link data; [`PrivateBattleActionBindings`](../src/Sf2.Remake.Content/Scenarios/PrivateBattleActionBindings.cs)
  validates named private item/prowess/equipment source mappings. Those trust checks stay in Content.
  The `ruleProfile` string currently accepts one profile; it is not a runtime module loader.
- **Query/commit:** selection commits only provisional UI state/revision. Confirmation validates again.
  [`BattleActionResolution`](../src/Sf2.Remake.Domain/Battles/Rules/BattleActionResolution.cs) retains
  prepared state, typed reactions/reward and construction/completion facts. It is the replay input;
  the observation strings are not an executable effect language.
- **Staged publication:** [`BattleSceneContinuation.Begin/Submit/Continue`](../src/Sf2.Remake.Application/Runtime/Battles/BattleSceneContinuation.cs)
  publishes prepared movement/construction seed, later spell cost, HP reactions, EXP credit and growth
  at different accepted boundaries. HEAL fairy work also advances the live main seed. Growth uses the
  seed after that work, not the discarded preflight seed. [`BattleActionCommitter.Publish`](../src/Sf2.Remake.Application/Runtime/Battles/BattleActionCommitter.cs)
  owns final action/after-turn/outcome/queue completion, not every earlier scene-state publication.
  `GameSession` alone assigns the authoritative snapshot.
- **AI:** [`EnemyPhysicalDecision.TryResolve`](../src/Sf2.Remake.Domain/Battles/Rules/EnemyPhysicalDecision.cs)
  scores reachable targets in reverse processing order with thinking RNG, records last-target state,
  and discards a physical preparation used for admission. [`BattleMovementContinuation`](../src/Sf2.Remake.Application/Runtime/Battles/BattleMovementContinuation.cs)
  delivers movement before actual physical preparation. [`SourceEnemyAi.Resolve`](../src/Sf2.Remake.Domain/Battles/Rules/SourceEnemyAi.cs)
  additionally owns activation/standby/memory and set6/set7 branches. Scoring RNG, construction RNG
  and later scene RNG cannot be collapsed into one speculative calculation.
- **Growth/outcome:** [`BattleGrowthRules.Credit/Grow`](../src/Sf2.Remake.Domain/Battles/Rules/BattleGrowthRules.cs)
  separates EXP credit from post-message growth. [`BattleOutcomeRules`](../src/Sf2.Remake.Domain/Battles/Rules/BattleOutcomeRules.cs)
  selects defeat/victory and the defeated hook; [`BattleOutcome.Begin/Continue/Heal`](../src/Sf2.Remake.Application/Runtime/Exploration/BattleOutcome.cs)
  selects programs/return anchors, recovery, join/flags and defeat gold. These are policies called by
  common continuation machinery, not all generic lifecycle mechanics.
- **Existing behavior assertions:** `HealingRulesTests`, `EngineSessionTests`, `SpellSelectionTests`,
  `BattleSceneTests`, `PhysicalBattleTests`, `BattleControlAiTests`, `SourceEnemyAiTests`,
  `BattleGrowthTests` and `BattleOutcomeProgramTests` in
  [`Engine.Tests`](../tests/Sf2.Remake.Engine.Tests) already cover actual reducers/session behavior.
  In particular, `HealingGrowthStartsFromTheSeedCarriedThroughFairyRetirement`,
  `WrongDuplicateAndStaleCompletionsCannotSkipEffectsOrReleaseInput`, and
  `PostMessengerScratchRequiresTheExactNormalReloadAndRealEntityRetirement` constrain migration.
  This inspection read them; it did not run them or inherit a passing result.

### Implemented HEAL seam

[`SessionRules`](../src/Sf2.Remake.Application/Runtime/SessionRules.cs) retains one internal
[`IHealingRule`](../src/Sf2.Remake.Domain/Battles/Rules/IHealingRule.cs) for the session's lifetime.
[`RuleCompositions`](../src/Sf2.Remake.Application/Gameplay/RuleCompositions.cs) owns the default
SF2 and the two authored factories; its `ForGame` method is the ordinary host's compile/restart
selection. Source-start and definition/start overloads retain explicit composition without
mutable selection. Default direct starts use SF2. Diagnostic rule identity is separate from Content
profile and never chooses arithmetic in dispatcher/query/view.

[`Sf2HealingRule`](../src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2HealingRule.cs) owns source admission
and preparation; the single [scalar](../src/Sf2.Remake.Domain/Gameplay/Sf2/HealingRules.cs) and
[target helper](../src/Sf2.Remake.Domain/Gameplay/Sf2/HealingTargetRules.cs) are shared with Herb.
[`AuthoredHealingRules`](../src/Sf2.Remake.Domain/Gameplay/Authored/AuthoredHealingRules.cs)
implements capped/proportional award A and injured-only half-missing/no-award B. The default
source class/award assertions remain independent of the demonstrations.

`GameSession.QueryBattleChoices` returns immutable session/revision/actor/stage/destination facts,
spell labels/presentation/range and ordered same-side living candidates with enabled/reason data.
Query runs no preparation or RNG and retains no future seed. The actual view consumes these spell
and target choices; physical/item/Stay UI remains the explicit next-slice boundary. Confirmation
rechecks the selected rule and private cast capability, then `ValidateHealing` checks the existing
resolution before `Begin`: only selected movement/main construction RNG, bounded target recovery,
caster MP cost and later caster progress are permitted. Foreign state, queue, thinking RNG,
accounting, definition and observation effects cannot be published by malformed HEAL output.
Construction random facts must form a complete `BattleRandom` main-seed chain; their count,
purpose names and ranges belong to the selected rule. SF2's two range-16 EXP draws are asserted at
its own behavior boundary, rather than imposed on every HEAL implementation.
Unexpected selected-rule errors report `InvariantFailure` with rule/operation context, without
exposing exception details. Expected rule errors retain their existing categories. Source unknown
EXP remains a reached preparation failure; selection does not prematurely spend or require it.

The existing scene owns cost, HP, EXP, fairy work and later growth. Growth consumes the then-current
seed. A reached later growth failure retains the earlier committed resources, progress, token and
queue; no whole-action rollback or successful release is implied. The cursor's source operations
are unchanged. [Verification](./development-and-verification.md#replaceable-heal-observation)
owns engine assertions and the bounded actual A/B observation. Original timing, other rule families
and full interface replacement remain outside this implemented seam.

### Selected module and composition design

**Accepted design:** keep the four assemblies. A gameplay module is an
ordinary set of C# source implementations under `Domain/Gameplay/Sf2` and
`Application/Gameplay/Sf2`, selected by explicit construction. New authored variants live beside
that module under `Gameplay/Authored`; they are project-authored examples, not original-fidelity
profiles. No separately distributed binary or third-party plugin API is required.

The existing assembly friendship already permits Application/Content/tests to use Domain internals.
Use narrow internal strategy interfaces under `Domain/Battles/Rules` and
`Application/Runtime/Exploration`, instantiated by the gameplay module. Engine executors call those
interfaces; only composition/default factories reference concrete SF2 implementations. Domain never
references Application, Content, Godot or reference verification. The Application module can combine
Domain rules and Application story policy using the existing inward dependency. Content decodes
validated definitions, not executable C# names or factories.

A single immutable `SessionRules` composition in Application holds the selected Domain rule bundle
and story policy. `GameSession.Start` accepts it and retains it for the session; existing overloads
may delegate to one explicit default factory. That default is a composition decision, never a branch
inside arithmetic, dispatch, query, scheduler or view. Existing direct-start callers keep working;
Godot's two source-start entries use the same composition. New project-authored implementation files
and their composition factory can be compiled in the existing projects. A caller selects a factory
before start, not a package-name switch during play. There is no mutable global current profile.

Retain a diagnostic identity for the selected composition independently of the content's format/trust
profile. The first authored demonstrations select a C# factory in ordinary startup composition and
rebuild; they need no new package schema or runtime selector. Authored variants must not be reported
as SF2 fidelity. A private source entry keeps its admitted default rule/resource contract; it cannot
silently select an authored fallback. Later immutable action/AI content keys resolve once against the
selected bundle, with missing/duplicate bindings rejected at start, not discovered through reflection.

Do not create empty policy interfaces for all future families in the first slice. Add each interface
with its migrated caller and delete the old static implementation with its last caller. Ordinary
private helper functions stay private. The source modules are independently editable/selectable;
they are not assembly-enforced isolation or a security sandbox. A fifth gameplay assembly would
force currently internal state/transition/program types into cross-assembly contracts without a
current binary-distribution need. If that need becomes real, replan and amend ADR0011's production
assembly section, ADR0017's preserved outer architecture, and ADR0019's four-assembly direction
before changing references. This selected source-module design needs no reversal of those ADRs.

```text
Godot composition -> Content admission -> definition + explicit start
                 -> selected SessionRules -> one GameSession
GameSession executors -> narrow rule contracts -> chosen C# implementations
chosen implementations -> existing numerical/state primitives
GameSession -> ordered state/scene projection -> Godot -> completion token
```

### Implementable API and transaction boundary

HEAL names below are implemented; remaining seams are proposed. Reuse the existing state, reference,
command, resolution, wait and failure types; do not add copies whose only purpose is forwarding.

| Seam | Inputs/result and owner | Required behavior |
| --- | --- | --- |
| HEAL rule (first slice) | Internal `IHealingRule`: spell admission, target query/check and preparation over `EngineBattleState`, `ActorRef`, provisional `MapPosition`, `SpellRef`, optional target; preparation returns the existing `BattleActionResolution`. | The same selected implementation owns query and confirm legality/formula. No RNG in queries. Default implementation moves `PlayerHealing`/`HealingRules`; helpers are not duplicated. |
| Action selection (completed in slice 2) | Extend `BattleSelection` and the existing command surface with a typed action reference and immutable option/target data. `GameSession.QueryBattleChoices` returns choices for its current session/revision/actor/stage/origin, selection step, enabled status and `SessionFailure` reason. | Options carry stable action/spell/item identities, display labels, supported presentation kind and ordered candidate `ActorRef`s/range. Preserve `SelectSpell`/`SelectItem` as semantic inputs while callers migrate. Query is disposable; `CommandEnvelope` and confirm remain authoritative. |
| Action preparation/application | Selected action implementation returns existing prepared/reaction/reward structure; engine preparation validates it before `Begin`. Existing scene continuation and publication apply it. | Validate actor/target references, destination, supported reaction/consumer, HP/MP bounds and permitted field changes. Reject unrelated actor/queue/definition/story changes and malformed effects before publication. Do not execute string observations or accept arbitrary callbacks that mutate the session. |
| AI (slice 3) | Internal `IBattleDecisionRule` consumes the immutable battle/actor plus selected action rules; returns the existing `BattleAutomaticAction` shape with candidate state, path and target. | Stable tie order and separate RNG streams; no hidden strategy state. Memory/last target/activation stay in actor/region state. Scheduler owns turn/wait/publication, not strategy selection or target scoring. |
| Progress/outcome (slice 4) | Selected progression functions use current actor, award, copied seed and existing effects; selected outcome policy uses current battle/story plus admitted outcome content. | Keep credit, later growth, outcome check and story return as separate calls at their existing commit seams. Policies choose rules/results; generic runner executes the continuation. |
| Source story operation (slice 5) | Internal `ISourceStoryPolicy` takes the current definition/snapshot, `StoryInstruction` and `ProgramLocation`, returning the existing active/story candidate or a `BattleRuleException`. | Only the explicitly source-specific instruction delegates. Generic branch/call/return/wait does not ask a rule-name registry. Missing support fails at the exact PC; policy cannot advance/publish the facade. |

An enabled query option means eligible for preparation under the known prerequisites, not a promise
that every reached random-dependent branch is supported. Query and confirm share those prerequisite
checks; confirm additionally constructs and validates the action. For example, a reached growth branch
may still reject during preparation without publishing its speculative draws. Keep that diagnostic
distinct from a stale or illegal selection. Queries must not run the whole scene, consume RNG, or
cache a prepared result whose seed could later be mistaken for live state.

The first action uses existing typed preparation, not a universal effect algebra. Its result needs
structural validation because an injected implementation can return an impossible transition even
when trusted project code is not malicious. Validate allowed deltas at this real authority boundary;
do not copy whole state into a new editable request object or add a separate validation service.
For HEAL, prepared deltas are selected-actor movement and construction main seed only; reactions may
change the selected living target's bounded HP, spell cost the selected actor's MP, and reward the
selected actor's progress at the declared later phase. Preserve other actors, thinking seed, queue,
accounting, definition identity and selection ownership. Slice 2 extends this finite validation to
physical/item construction's existing inventory/gold/death obligations, not arbitrary property writes.

At every call, rules receive immutable definitions/state and local RNG values. Use `BattleRandom`
for deterministic draws; only the rule determines which draw is required and its stable iteration
order. Preserve existing ushort/byte widths, signed comparisons, truncation, saturation and explicit
unchecked source arithmetic. No wall clock, host RNG, unordered collection iteration or persistent
rule-object state may influence results. The local preflight result is discarded. Later growth/AI
must consume the then-current seed at the owning boundary; storing a preview's future seed is wrong.

Failures before preparation publishes preserve state, provisional selection, RNG and observations.
Stale session/revision/actor input rejects before rule invocation. Selection changes may increment
revision but never spend resources or random draws. At a later scene/program boundary, failure keeps
earlier accepted commits and the live wait/cursor/queue; it does not rewind already displayed effects
or rerun a spent award. Map expected rule errors to the existing IllegalCommand/UnsupportedCapability
categories; an invalid implementation result or unexpected rule exception becomes InvariantFailure
with rule identity, operation and existing field/PC context. Catch only around the selected rule call,
not the entire host, and preserve the last committed snapshot. Do not convert failure into Stay or
successful completion. Direct adapter errors remain AdapterError.

Content admission owns schema, references, provenance and known unsupported definition shapes.
Rule admission owns learned action, faction/target/range/resources and accepted class policy.
Application joins that result with implemented reaction/scene capability before starting an effect.
For example, HEAL4 cannot become supported by deleting `healing-animation`; the query must expose
Unsupported and confirm must enforce the same boundary. Private missing cast resources never use
the authored gesture fallback. Actual delivery failure holds the pending token and exposes an adapter
failure. Godot consumes options/targets/semantic inventory and scene requests; it may format labels,
cycle options, and select an existing presenter by effect kind, but cannot decode source inventory
bits, choose factions/ranges, calculate effects, or branch on strategy/module names. That is the #438
interface handoff; it specifies no layout, artwork or new input-device requirement.

### Finite migration disposition

All five implementation slices below are necessary for this finite #617 scope. A HEAL-only result
cannot close the Epic. This table is the coverage boundary, not a backlog for every original subsystem.

| Responsibility | Disposition and owning slice | Acceptance limit |
| --- | --- | --- |
| HEAL admission, recovery/EXP algorithm and ally/range targets | Independent selected C# action rule, slice 1. | Existing HEAL1–3 and two authored strategies; no new source spell capability. |
| Physical strike/reversal/double/counter/reward calculation, physical targets and terrain protection; Medical Herb effects/inventory encoding; Stay/action dispatch | Independent action implementations, slice 2; reuse one shared physical calculator for player and AI. Source decoding remains Content/module policy. | Existing supported physical and consumable branches only. Semantic item/target projection replaces raw Godot policy. |
| Attack-then-approach, standby/source set6/set7, target priority/class mapping, strategy bindings and runtime memory | Independent decision implementations, slice 3. Replace core AI enum dispatch with immutable resolved strategy bindings; retain faction/control/order as state. | Existing source orders retain guards. A second authored priority algorithm proves replacement; no new enemy command families. |
| EXP/kill/gold rules, growth curves/learning, defeat/victory, recovery and return/join/flag policy | Selected C# progression and outcome policies, slice 4; tables/route programs remain content. | Existing growth and ordinary outcome/return behavior only; preserve leader-loss precedence, unknown accounting rejection and delayed growth RNG. |
| Conditional story, call targets, flags, dialogue and waits | Externally authored typed programs, slice 5 proves alternate branch/call/wait content through the same runner. | No arbitrary-language interpreter; algorithmic source compatibility remains named C#. |
| Retired Map3 scratch exception and source outcome-return conditions | Exact guarded source-policy implementation, slice 5 (return policy already selected by slice 4). | Preserve true entity retirement and source context; do not translate the four inactive scratch stores into new visible effects. |
| Actor state, main/thinking seeds, queue cursor, program PC/stack/waits, revisions, observation order, staged effects and completion tokens | Retained engine mechanism. Rule-selected candidates are validated/published by the existing engine. | No second state store, speculative live mutation or alternate publication facade. |
| RNG recurrence, integer helpers, Manhattan geometry, weighted movement/path representation and fixed turn-order primitive | Retained deterministic primitives. Policy decides operands/call order; source arithmetic remains explicit. | This plan does not promise interchangeable RNG generators, pathfinding algorithms or turn schedulers. They are fixed mechanism contracts for this engine. |
| New-battle refresh/activation/control word policy, terrain movement-cost table, field motion/text/music and battle-scene logical service algorithms | Explicitly deferred replacement seams; preserve current owners/behavior. Selected rules may call these supported primitives without duplicating them. | They remain source-specific engine services. #617 may close only with this bounded replaceability limit accepted; an assertion that **all** gameplay/source policy is replaceable remains false. Broader decoupling needs a new scoped decision, not automatic expansion here. |
| Definition/source identity, package schema, public/private admission and selected class/spell/item data | Content retains parsing/provenance; slices 1–3 separate binding from runtime strategy. Keep finite typed healing/physical definitions and source metadata. | S5 is resolved for algorithm selection and state authority, not a universal ability/class system. Unsupported effects/statuses still stop explicitly. |
| Runtime language, no-compile authoring, hot reload, broader UI, full-game capabilities | Deferred/excluded as stated in the agreement. | No dependency for the selected compile/restart workflow; new intent must reopen design rather than silently expand it. |

The source-service deferral is deliberate: those services bind logical opportunities, source RNG and
consumer timing, while their replacement is not needed to prove the selected formula/AI/story authoring
outcome. Moving them now would add a separate timing-policy migration and observation obligation.
Keeping their fixed contracts avoids that expansion; it does not make them implementation-neutral.

### Dependency-ordered implementation and acceptance

Slices are tracked by #674–#678. The HEAL slice is implemented; subsequent slices require the
preceding independently accepted result, refreshed exact path ownership,
and a continue/narrow/stop decision; proposed interface filenames are provisional until then.
All shared contracts, default composition, `GameSession`, Content readers, views, tests and this owner
have one writer at a time. No stacked unaccepted implementation is assumed.

| Slice | Complete behavior and dependencies | Acceptance and retirement |
| --- | --- | --- |
| 1 — replace HEAL through the real session and presenter | Implemented by `IHealingRule`, `SessionRules`, `QueryBattleChoices` and finite `ValidateHealing`, using the existing view/scene. | Default source behavior and two authored algorithms cover query → select → confirm → staged cost/recovery/reward → completion. Malformed rule output, stale/illegal input and missing capability publish no partial preparation/RNG. `PlayerHealing` is removed. Independent main-gate integration remains separate. |
| 2 — complete the admitted action/target interface | Depends on slice 1's measured seam. Migrate physical, Herb and Stay; complete semantic inventory/action references and remove adapter target/raw-word policy. | Player and AI share one physical action calculation. Preserve reaction order, consumption, gold/death/queue boundaries. Queries and commit agree; unsupported items remain explicit. No action- or rule-name dispatch in Godot. Retire migration-only adapters. |
| 3 — select AI without scheduler strategy switches | Depends on slices 1–2. Resolve actor strategy bindings, move admitted source/approach algorithms and class priority mapping. | Two authored algorithms choose different legal actions/targets from the same state without scheduler/UI edits. Repeatable thinking/main seed and memory, target ties, zero-target branch, illegal result and failed-action queue retention. Preserve source standby/activation acceptance. |
| 4 — select progression and outcome policy | Depends on slices 1–3. Move award/growth and ordinary victory/defeat/recovery/return choices at their existing call sites. | Growth uses the live post-scene seed; EXP/growth message order, pending failure, defeat precedence, gold/recovery and usable program return remain real behavior. An authored alternate reward/outcome policy changes behavior without editing continuation machinery. |
| 5 — replace conditional story and isolate the exact source exception | Depends on slice 4's return seam. Move scratch policy, prove alternate external programs and finish authoring/semantic-interface documentation. | Different flag/choice branches execute different effects with call/return and an actual wait; stale/duplicate completion and unsupported PC preserve earlier commits. Wrong scratch context still fails; correct retirement remains. Review all S1–S5 and explicit deferrals before Epic acceptance. |

**First vertical slice:** choose HEAL over Stay (no meaningful formula/RNG/consumer proof), Herb
(source inventory encoding adds a second concern), or physical (multi-hit/reversal/death expands the
first pilot). Use an authored living priest with HP10/max40, MP8, EXP0, power10 and cost3. Variant A
recovers `min(power, missingHp)`, uses the existing proportional EXP/two-draw rule and permits living
same-side targets. Variant B recovers `min(power, ceil(missingHp/2))`, awards zero EXP without award
RNG, and permits only injured same-side targets. At HP10 they both recover10; at HP30 they recover10
versus5, and at full HP only A remains legal. These explicit algorithm/branch differences test more
than changed constants. They are demonstration rules, not proposed SF2 balance changes. Both use the
existing supported HEAL scene family, cost once, and retain later scene RNG/order. The default SF2
implementation remains the existing source behavior, independently asserted.

The first slice adapts `PlayerItemUse.cs` to the single extracted living
same-side/range/self-destination target owner,
`Domain/Gameplay/Sf2/HealingTargetRules.cs`; default HEAL and Herb call it directly.
The single scalar `HealingRules` now lives in that same module and owns Herb's EXP calculation.
Herb does not call the session-selected HEAL strategy: changing
the authored spell variant must not change Herb targeting, recovery, EXP, inventory or RNG behavior.
`PlayerHealing.cs` is removed after dispatcher, Herb, `HealingRulesTests` and
`HealingFairyTests` callers migrated; no forwarding class remains. The target/scalar helpers remain
single-owned shared policy while HEAL and Herb consume them, including after Herb's slice 2 migration.
`MedicalHerbTests` supplies the affected action-behavior regression boundary, not tests of the helper.

Run both variants through the ordinary authored content reader and session, not a fake dispatcher.
Observe the same actual Godot presenter through ordinary input and its current state/projection/error
endpoint. Existing keyboard shortcuts can choose semantic options; no new UI design is required.
Keep variant selection in composition or an explicit authored rule binding validated there, never a
probe-controlled production state setter. Include different actors, self/other target, movement/cancel,
insufficient MP, dead/opposing/out-of-range target, stale revision, a rule throwing after local RNG
work, malformed reaction output, missing private presentation, and duplicate/wrong token. Assert exact
preparation and later committed boundaries. A failure after an earlier accepted scene step retains
that step; it is not falsely described as whole-action rollback.

Implementation checks use the current [locked workflow](./development-and-verification.md#locked-net-workflow),
`uv run sf2 verify engine` for the actual behavior unit project and `uv run sf2 verify adapter` where
host/API changes require compilation. During correction use affected behavior filters, preserving
completed failures. Existing direct observers in `game/probes/engine_battle_observation.gd`,
`engine_battle_scene_observation.gd` and the exploration/outcome observers can be narrowly extended
and run directly; do not add tests of them. Use a small authored observation for variants and only an
affected private boundary when source consumers change. A required unavailable observation remains
unverified and prevents that slice's acceptance. No whole-route replay, full H4, screenshots or new
original acquisition follows from this migration.

The historical [HEAL cursor comparison](./development-and-verification.md#heal-scene-verification)
retains its completed recovery CP2059–2091 failure; historical composition reports remain unavailable
at their own limits. Do not reinterpret either as an interrupted run, fix the golden to match a
refactor, or require a fresh full route merely to replace old red results.

### Cumulative resources, pilot decisions and recovery

The original investigation was source-only. The bounded HEAL pilot now uses actual engine tests,
adapter compilation and short A/B Godot observations; [verification](./development-and-verification.md#replaceable-heal-observation)
owns its reproducible commands. The implementation estimates below are **Inferred planning ranges**, not acceptance
limits or reasons to omit necessary boundary analysis. Consider implementation, review, debugging,
integration and ongoing maintenance together; main-gate adjusts estimates using actual discoveries
and avoided rework, without a separate cost process or repeated per-task accounting.

| Slice | Engineering hours | Narrow verification, independent review and integration hours | Main uncertainty |
| --- | --- | --- | --- |
| 1 | 10–16 | 4–6 | Query/prepare validation and staged failure seam; actual presenter reuse. |
| 2 | 10–16 | 3–5 | Physical prepared-state deltas and raw-item projection consumers. |
| 3 | 8–14 | 3–5 | Strategy binding and preserving independent thinking RNG/preflight. |
| 4 | 8–14 | 3–5 | Live growth seed and source outcome/program composition. |
| 5 | 6–10 | 3–5 | Source exception provenance, conditional wait and final documentation. |
| **Finite total** | **42–70** | **16–26** | **58–96 hours** initially estimated, including supporting work; uncertainty may justify revision. |

Expected code footprint is roughly 1.5–3k existing handwritten lines moved/reworked across the five
families and 0.6–1.2k net new contract/query/behavior-test lines, uncertain by about 50%. This is not a
line target. Keep new/extracted source under1,000 physical lines; if a touched oversized file needs
extraction, move the affected responsibility without net growth. No extra project, package dependency,
background service, durable protocol, cache or database is proposed. Maintenance consists of the
finite strategy seams and authoring examples within existing projects/tests, not maintaining two
engines. New interfaces must have actual migrated consumers; no permanent old/new calculator pair.

The correctness pilot uses the two authored variants and their necessary behavior tests and actual
consumer intervals. Query target filtering should be O(A) time/output for A actors; preparation
retains current action complexity and O(A) immutable-state copies. AI retains its existing grid/
candidate work rather than adding a state copy per target or cell. Resolve rule selection once per
session, without per-frame file reads or reflection. Expect authored inputs around 1 MiB or less,
compact summaries on the order of 10 MiB and short-lived observation scratch around 100 MiB; these
are sizing assumptions, not fixed quotas. Reuse worktree build/import caches and existing tool
installations. No new network data, paid API, capture database or benchmark infrastructure is needed.

Prefer timing/allocation information already exposed by necessary builds, behavior tests and the
actual consumer pilot. Add targeted performance measurement only for an identified credible
regression, such as copying battle state for every queried target; measure that operation against
the existing case rather than imposing a generic rule-call microbenchmark or fixed call count.
Runtime correctness, useful replacement boundaries and diagnosable failures are acceptance criteria.
Do not claim measured performance from source shape, or require an unrelated benchmark to accept a
sound design. Main-gate reviews the real success, rule-failure and missing-consumer behavior before
expanding; material cost or complexity discoveries inform its engineering reassessment.

Design quality and total implementation, review, debugging and maintenance effort take precedence
over minimizing one run's spend. The provisional effort/storage estimates above
are reassessment signals, not design acceptance targets or reasons to omit necessary analysis.
Main-gate adjusts estimates as routine engineering judgment, including extra work that prevents
rework; this does not automatically require user approval. Stop for actual scope departures such as
a second state authority, new runtime/framework or unauthorized acquisition. Changed user outcomes,
fidelity or explicit user limits still require the user's decision. Source-service deferrals must
be accepted explicitly before bounded #617 closure; otherwise keep the Epic open for that decision.

Each accepted slice is a recoverable Git boundary: integrate only after independent checks and
retain its prior behavior/failure evidence. Before merge, correct or discard only that topic; after
merge, a semantic revert or forward fix is main-gate-owned and preserves accepted later dependencies.
Do not switch a live session's rule bundle or migrate its state; rebuild/restart from admitted content
and an explicit start. Preserve ignored inputs, reports and the transferred environment. Record the
actual selected rules, content identity, source/failure context and commands in the slice handoff,
without publishing private payloads or absolute input paths. The next slice begins only after the
previous writer stops and exact shared-path ownership is transferred.
