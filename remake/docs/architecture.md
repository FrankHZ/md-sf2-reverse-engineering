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
| `Sf2.Remake.Domain` | typed values, immutable state, deterministic reducers, map traversal, working-layout mutation, and inventory rules | .NET base libraries only |
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

The public-synthetic and private-local profiles share the assembly direction and Godot host. They do
not share a weaker content reader or silently convert one profile into the other.

## Current Implementation Shape

The implemented Domain boundaries are already independently testable and engine-free. Application
owns public-synthetic session lifecycles plus a separate admitted private Map 3 traversal state behind
the same logical `GameSession` facade. Content exposes three public readers: one tracked synthetic
package reader and two private-local readers for canonical map import and base visual payload admission.

Two areas currently concentrate more responsibility than the target shape:

- `Map3Root` and its private partial own profile dispatch, composition, private input polling, command
  calls, and smoke orchestration. Public-synthetic action registration and polling delegate to the
  internal `Map3InputAdapter`; public-synthetic node ownership, formatting, and snapshot projection
  delegate to the internal `Map3Presenter`; private-local and unavailable diagnostic node ownership,
  formatting, and typed viewport projection delegate to the internal `PrivateMap3Presenter`. None of
  these collaborators owns session or gameplay state.
- `GameSession` and its partials own command routing, pending gates, lifecycle handlers, broad snapshot
  construction, private admission, and projection-facing result types.

The Content public seams are appropriately narrow, but each large reader currently combines raw
identity verification, parsing, semantic validation, and mapping in one implementation file.

These are accepted implementation facts, not a claim that their planned refactors are complete.

## Target Internal Delegation

### Godot host

`Map3Root` should converge on profile selection, dependency construction, lifecycle, and wiring.
Internal replaceable collaborators may own:

- `Map3InputAdapter`: public-synthetic `InputMap` actions to semantic Application commands
  (**implemented**; private-local polling remains in the existing partial);
- `Map3Presenter`: public-synthetic authoritative snapshot to a bounded internal view model, nodes, and
  labels (**implemented**);
- `PrivateMap3Presenter`: display-only private-local/unavailable plans plus authoritative private
  snapshot to diagnostic nodes, status, and the existing typed traversal viewport (**implemented**;
  smoke only reads its current projection);
- public-synthetic and private-local composition builders returning one runtime handle; and
- `Map3SmokeDriver`: deterministic command scripts and stable observation serialization.

Presentation helpers may directly construct nodes, choose project-authored diagnostic colors, and
format labels. They remain disposable adapter state and never become another gameplay authority.

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

Future refactors are serialized and behavior-preserving:

1. continue extracting composition, private input, and smoke responsibilities; public-synthetic input
   and presentation plus private diagnostic presentation are already delegated, with profile selection,
   commands, marker bytes, and observations preserved;
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
