# Remake Implementation Architecture

## Purpose

This document describes the current implementation topology and its intended bounded decomposition.
It is an implementation guide, not a replacement for the normative decisions in
[ADR 0011](../../docs/decisions/0011-phase4-remake-runtime-architecture.md) and
[ADR 0017](../../docs/decisions/0017-heavy-boundaries-light-internals.md).

The open [architecture and verification audit](./architecture-audit.md) records the user's clarified
modern-engine direction, separation of 0009 verification from gameplay, Application findings, and
current prohibition on screenshot-based verification. Read it before proposing the next engine slice;
the findings are not a claim that remediation has been implemented.

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
A1–A8 closure remains tracked by the audit. Public/private trust
and the incomplete accepted 8C/H4 target remain distinct from this migration.

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
sessions never require a Map3/Battle01 class or ADR0009 trace field. Retirement does not close A1–A8,
natural continuity or 8C/H4.

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
