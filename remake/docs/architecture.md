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

## Proposed State and Content Direction

[Proposed ADR 0019](../../docs/decisions/0019-state-and-content-driven-remake-engine.md) owns the
recommended command/state/result model, typed content and resumable programs, audit A1–A8 mapping,
new-engine unit tests, direct reference verification, old-test/CI retirement, and incremental migration.
It proposes live state, validated content,
and implemented capability as gameplay admission inputs, with fixed walkthrough constraints retained
in reference verification. Application would advance to actual input, presentation/tick waits, or an
attributed unsupported boundary; Godot would consume semantic commands and observations.

This is a proposed behavioral migration, not the current implementation or an accepted replacement for
ADRs 0011/0017. The profile-specific snapshots, fixed import checks, endpoint handlers, and Godot battle
dispatch described below still exist; the audit findings remain open. The proposal preserves distinct
public/private trust and leaves the accepted 8C/H4 target incomplete. It authorizes no implementation
or new observation transport by itself.

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

The current implementation remains a legacy controlled-route engine. It has useful deterministic
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
behavior, while ADR 0019 proposes behavioral migration away from fixed histories and incompatible
runtime paths. Its M0/M1 implementation and executable CI/local cutover remain separately owned.

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
