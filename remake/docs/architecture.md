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
semantic commands and observations. General presentation/program waits remain future capabilities.

M0 implements consumed internal Domain RNG, ordinary priest healing arithmetic, turn-order generation
and Manhattan action range, with a dedicated engine unit project and scoped verification entries.
`PhysicalStrikeRules`, `BattleRewards` and `PhysicalBattleAction` now own ordinary physical construction,
settlement and atomic state transition. `PhysicalBattleAction` constructs at most three source-ordered
hits on temporary HP, carries sticky reaction decisions and aggregates one ally award before publication.
Both player commands and `EnemyPhysicalDecision` call that calculator; `BattleActionCommitter` is
the single publication/queue-consumption mechanism. Automatic advancement catches failures at each
enemy ACTION, preserving earlier commits and retaining the failed enemy's queue entry. The typed
ATTACK1/script3 controller scores all reachable physical targets in reverse slot order and uses
shared `PhysicalTargetRules` for signed raw-priority cohorts, class selection and movement ties.
The existing class definition supplies source identity only for admitted named classes; missing
identity rejects a reached critical comparison. Regular movement fixes the class table; content
cannot supply an independent rank. Source thinking, scoring and selection are shared with reference
consumers. Wider AI stays explicitly Unsupported.
Shared strike/reward functions remain the calculation owners; reference DTOs only project their
results. Semantic observations carry both actor and target for reversal. Dead combatants retain
identity/HP/kill-and-defeat accounting but have no
battlefield position; occupancy and presentation read that authoritative state.
The [current M1 boundary](../../docs/decisions/0019-state-and-content-driven-remake-engine.md#current-m1-implementation)
adds movement/cancellation, common session/content admission and connected authored battles.
Profile-specific snapshots, fixed import checks, endpoint handlers and old Godot battle dispatch
remain only in the named transitional reference consumers below; A1–A8 remain open. Public/private trust
and the incomplete accepted 8C/H4 target remain distinct from this migration.

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
windows. Authored UI follows the actual viewport; the explicit legacy composition retains its own
window policy. A target-cycle cursor stores only the last attempted UI candidate, so an engine range
rejection cannot trap navigation. The accepted target remains exclusively in the session snapshot.

The old scenario-bound implementation is isolated in the separate
[transitional reference assembly](../reference/README.md), with no reverse project dependency from
production Domain/Application/Content. Its inventory names current consumers and M2/M3 retirement
points for controlled presets, import/trust mapping, actual story handlers and fixed comparisons.
New authored sessions never require a Map3/Battle01 class or ADR0009 trace field. This isolation does
not complete private import or program migration, or close A1–A8. M5 is remaining cleanup only.

| Assembly | Current responsibility | Dependency direction |
| --- | --- | --- |
| `Sf2.Remake.Domain` | typed immutable battle state, RNG/healing/physical/reward/range/turn/movement rules; retained reusable map/layout/item reducers | .NET base libraries only |
| `Sf2.Remake.Application` | thin `Runtime.GameSession`, common contracts, independent command dispatcher and automatic battle advancer, typed scenario port | Domain |
| `Sf2.Remake.Content` | configurable authored package parsing, reference resolution, numeric and capability validation | Application and Domain |
| `Sf2.Remake.Godot` | profile selection, dependency composition, `InputMap`, scene/view projection, local diagnostics, smoke hosting, and platform lifecycle | Application, Content, Domain, Godot; explicit legacy startup also consumes Reference |

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

`Application.Runtime.GameSession` is the authored engine's only logical gameplay mutation facade. Godot may request a command or project a
result; it does not change position, flags, inventory, request state, RNG, or flow state directly.
Content constructs admitted immutable definitions but does not mutate a running session.
The following private protocol belongs to the separate legacy reference consumer. For its canonical Map 3 port, the import definition is the sole owner of the selected setup's
ordered entity population and the private snapshot exposes that exact immutable object. Content maps
source-shaped records to stable resource/ordinal identities, raw and masked coordinates, opaque facing
and map-sprite values, and a defensively owned opaque tail. Application does not receive source
addresses and does not promote those bytes into visibility, walking, action, interaction, or rendering
rules.

The public-synthetic and private-local profiles share the assembly direction and Godot host. They do
not share a weaker content reader or silently convert one profile into the other.

## Legacy Reference Implementation Shape

The isolated reference implementation remains a legacy controlled-route engine. It has useful deterministic
Domain reducers, validated public/private readers, and one named `GameSession` facade, but the
[audit](./architecture-audit.md) identifies incomplete architectural boundaries:

- public-synthetic and private-local APIs/snapshots diverge behind the facade;
- fixed import/party presets and reference-history predicates constrain ordinary battle actions;
- some story handlers assign accepted endpoints instead of running the reached program;
- `PrivateBattle01Composition` schedules rounds and individual turns in Godot;
- scenario-specific snapshot fields and allowlists amplify unrelated changes.

The [capability matrix](./capability-status.md) owns current support. The
[Map 3 implementation/reference record](./map03-playability-plan.md) owns each selected input,
method, controlled state, reproduction and Unsupported boundary. Those details describe current
code; copying them into a general engine would preserve the audited defects.

Private startup currently admits canonical maps and selected Battle01 exports before initializing
one battle snapshot. Live battle state owns roster, terrain/occupancy, phase, RNG and turn order;
older map snapshots remain frozen provenance. Current movement preview/confirmation/cancellation,
combat and return methods have their named controlled domains. They do not yet implement ADR 0019's
common command model or general configurable content loader.

The public tactical micro-battle uses project-authored simplified rules and is not an SF2 oracle.
Keep private/source trust separate from authored configuration while migrating actual overlapping
gameplay through the same state/rule path. Neither current fixed package identities nor source
record ordinals define universal gameplay legality.

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

Choose the scope explicitly: an internal behavior-preserving refactor retains meaningful observable
behavior, while ADR 0019 directs behavioral migration away from fixed histories and incompatible
runtime paths. M1's common authored battle uses independent runtime collaborators. M2/M3 must
migrate real rules/content/programs and remove each obsolete reference family with its last caller.

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
