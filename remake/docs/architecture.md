# Remake Implementation Architecture

The remake is a deterministic modular monolith hosted by Godot. Validated content, live state and
implemented rules determine behavior; reference routes are external comparisons. The common session
runs authored packages and the connected private Map 3 through Battle 01 world.

The adopted boundaries are [ADR 0011](../../docs/decisions/0011-phase4-remake-runtime-architecture.md),
[ADR 0017](../../docs/decisions/0017-heavy-boundaries-light-internals.md) and
[ADR 0019](../../docs/decisions/0019-state-and-content-driven-remake-engine.md).
The [capability owner](capability-status.md) distinguishes runnable behavior, Unsupported and Unknown.
The [accepted private milestone](../../docs/design/synthesis/map3-battle01-readiness.md#accepted-current-milestone)
does not establish full-game coverage, natural original reach, hardware parity or distribution rights.

## Purpose

Use the owner that matches the responsibility being changed:

| Responsibility | Current owner |
| --- | --- |
| Domain rules, selected algorithms, deterministic state transitions and author workflow | [Gameplay rules](domain/gameplay-rules.md) |
| Application session authority, program execution, waits, map and outcome continuation | [Session and programs](application/session-and-programs.md) |
| Content profiles, admission, explicit starts and private provenance | [Profiles and trust](content/profiles-and-trust.md) |
| Local source/master/runtime assets and offline preparation | [Presentation assets](content/presentation-assets.md) |
| Godot entry, semantic input, field projection and UI boundaries | [Presentation](godot/presentation.md) |
| Audio cue delivery and source limits | [Audio](godot/audio.md) |
| Battle scene, movement and death presentation | [Battle scenes](godot/battle-scenes.md) |
| Environment, checks and observation selection | [Development and verification](development-and-verification.md) |
| Accepted compositions, failed comparisons and retired implementation records | [Retained evidence](evidence/retained-comparisons.md) |

## State and Content Direction

Definitions contain reusable immutable actors, deployments, terrain, actions and programs. Separate
start inputs supply controlled vitals, flags, positions and independent RNG seeds. Content admits
them once. A selected immutable rule composition is bound before session creation and remains fixed
for that session. The [finite replacement scope](domain/gameplay-rules.md#scope-and-fixed-services)
supports ordinary C# algorithm changes and external typed programs, with explicit fixed services.

Gameplay legality never depends on a comparison ID, receipt count, named round or one walkthrough's
character sequence. A genuine source guard or unsupported capability remains explicit. Private
source identities are Content trust checks, not universal authored gameplay predicates.

## Production Assemblies

| Assembly | Responsibility | Dependencies |
| --- | --- | --- |
| `Sf2.Remake.Domain` | Immutable numerical/state primitives, battle rules, layout, traversal and entity motion | .NET base libraries |
| `Sf2.Remake.Application` | `GameSession`, command admission, selected rules, program/battle continuation, content ports and observations | Domain |
| `Sf2.Remake.Content` | External byte/path/provenance admission and immutable definition/start construction | Application and Domain |
| `Sf2.Remake.Godot` | Ordinary startup, semantic input and disposable node/audio/texture projections | Application, Content, Domain and Godot |

Production has no Reference, test or verification-tool dependency. `GameRoot` joins source and rule
composition; its views submit commands and actual completion. Focused dispatchers/runners compute
transitions behind the facade. No second gameplay state authority lives in a presenter or reader.

## State and Command Flow

```text
device input -> Godot semantic input -> GameSession command admission
             -> selected deterministic rule / program continuation
             -> authoritative snapshot + ordered observations
             -> disposable Godot presentation -> actual matching completion
```

`GameSession` alone publishes its current snapshot. The explicit active-state union distinguishes
battle and exploration, with shared story continuation. Stale session/revision/actor commands reject
before rule invocation. Preparation failure publishes no speculative movement/resources/RNG; a later
scene/program failure retains earlier accepted commits and the live token/cursor/queue.
See [session authority](application/session-and-programs.md#session-authority) and
[rule transaction boundaries](domain/gameplay-rules.md#implementable-api-and-transaction-boundary).

## Review Constraints

Keep public protocols for actual trust, authoritative mutation, independently versioned ports or
stable observations. Internal helpers use direct calls unless another boundary is justified.
Content readers preserve validation ordering and fail-closed profiles. Avoid alternate parsers,
caller-selected trust roots, silent fallback and a second mutable state owner.

Use small tests of actual engine behavior and meaningful changed states/content. Comparisons, probes,
planners and reports are already verification; use them directly. Counts, private spelling and a
green retired aggregate are not acceptance targets. Current setup and checks belong to the
[verification scope](development-and-verification.md#scope).

## Review Questions

- Does authoritative state still have exactly one publisher?
- Does valid changed content/history work without case-ID or receipt edits?
- Do query, confirmation and actual delivery retain their distinct authority?
- Are deterministic RNG, staged publication, failure attribution and private trust preserved?
- Can the view be reconstructed from current observations without choosing gameplay policy?
- Are Unsupported, original-game Unknowns and fixed-service exclusions visible?

Historical audit findings and migration plans are available through the [evidence route](evidence/retained-comparisons.md#historical-implementation-and-audits).
They do not impose a new repair queue or test-retention quota.
