# ADR 0017: Heavy Boundaries, Light Internals

- Status: **Accepted**
- Decision date: 2026-08-30
- Scope: remake public-protocol granularity, internal delegation, and refactor sequencing
- Relationship: refinement of [ADR 0011](./0011-phase4-remake-runtime-architecture.md), not a reversal

## Context

ADR 0011 accepts a deterministic modular monolith with four production assemblies, inward dependency
direction, plain-C# authoritative state, one Application mutation facade, validated Content ports, and
thin Godot adapters. Those boundaries remain appropriate for a remake that must separate private inputs,
deterministic gameplay, presentation, and eventual H4 observations.

Early Phase 4 slices applied the same degree of protocol structure to many smaller same-process
capabilities. A bounded interaction could add its own identifiers, request and acknowledgement types,
status, snapshot, cue, receipt, diagnostic, result hierarchy, capability label, Godot projection, smoke
marker, and status prose. Individual types remained testable, but the aggregate public surface and the
central orchestration files grew with every capability.

This is not primarily a class-count problem. The material risks are:

- public protocol growth without a corresponding trust, mutation, versioning, or observation boundary;
- responsibility concentration in the Application facade and Godot composition root despite many
  surrounding types;
- change amplification, where one small feature requires coordinated edits across every assembly,
  projection, receipt, smoke marker, and status document; and
- apparent precision from capability labels or receipts that do not establish an independent contract.

The project needs a granularity rule that preserves ADR 0011's safeguards without making every internal
implementation choice a durable public protocol.

## Decision

Use **heavy boundaries and light internals**.

### Preserve the accepted outer architecture

This decision does not change ADR 0011's four production assemblies or dependency direction:

- `Sf2.Remake.Domain` owns deterministic gameplay values, state, and reducers;
- `Sf2.Remake.Application` owns the logical session facade, command admission, orchestration, and
  observations;
- `Sf2.Remake.Content` implements validated public and private input adapters; and
- `Sf2.Remake.Godot` owns composition, platform input, scenes, presentation, capture, and lifecycle.

`GameSession` remains the sole logical gameplay mutation facade. Authoritative gameplay state remains
deterministic and independent of Godot. Private and public Content inputs remain fail-closed. Stable H4
and compatibility observations remain explicit. No scene, presenter, input adapter, smoke driver, or
Content helper becomes a competing gameplay authority.

### Require heavy typing for four boundary reasons

A new public capability, receipt, or type must name at least one of these reasons:

1. **Trust boundary:** the seam crosses untrusted bytes, a local path, a runtime profile, provenance, or
   a public/private redistribution boundary.
2. **Authority boundary:** the seam admits or performs authoritative gameplay mutation, deterministic
   state transition, clock/RNG behavior, or a legal-state invariant.
3. **Versioned port boundary:** the seam is an independently versioned cross-assembly contract whose
   implementation is replaceable or whose compatibility must be validated before use.
4. **Observation boundary:** the seam is a stable H4, replay, export, smoke, or other compatibility
   observation that external tooling must parse or compare durably.

When one of these reasons applies, use the smallest typed request, result, diagnostic, receipt, snapshot,
or capability surface that closes that boundary. A boundary may need several types; this decision does
not impose a one-type rule or measure quality by a raw count.

When none applies, default to direct, internal implementation. Same-process Godot presentation,
formatting, scene construction, project-authored helper state, and ordinary orchestration do not require
new public protocols merely because they are distinct methods or visible fields. A diagnostic field does
not receive its own capability or receipt unless it independently meets a boundary reason above.

### Keep internal code explicit without making it public

Light internals are not untyped dictionaries, implicit globals, hidden gameplay state, catch-all event
buses, or silent failure. Internal classes, records, enums, and focused functions remain appropriate when
they make code clear or testable. They should be `internal` or private by default and may use direct calls
within the owning assembly.

Project-authored Godot presentation may construct nodes, choose diagnostic colors, format labels, and
map an admitted view model directly. Those choices remain disposable presentation state. Godot still
sends semantic commands to Application and reconstructs its view from authoritative observations.

The stable part of a smoke or H4 surface is its externally parsed observation and ordering contract. Its
driver, formatting helpers, scene traversal, and setup mechanics remain internal unless they satisfy a
separate boundary reason.

### Retain one facade and delegate internally

There remains one logical `GameSession` facade. It may delegate command routing, pending-command gates,
navigation and transitions, interactions, dialogue, search and acquisition, and snapshot projection to
focused internal collaborators. Those collaborators do not expose a second mutation entry point or keep
competing authoritative state.

Do not introduce a universal generic event engine merely to reduce the number of visible lifecycle
types. Domain-specific invariants and command ordering remain explicit. Shared machinery is extracted
only after multiple accepted behaviors demonstrate the same semantic need.

### Make the Godot root a composition and lifecycle root

The Godot root should converge on profile selection, dependency construction, lifecycle, and wiring.
Replaceable internal adapters may own:

- `InputMap` to semantic-command translation;
- authoritative snapshot or bounded view-model projection;
- public-synthetic and private-local composition; and
- deterministic smoke driving and stable observation serialization.

These adapters remain thin. Input and presenters do not decide gameplay rules, Content trust, route
truth, or mutation validity.

### Split Content readers internally, not around weaker gates

The public Content ports and their fail-closed behavior remain stable. Large readers may be separated
internally into raw-identity verification, parsing, semantic validation, and mapping. The owning reader
still enforces the accepted order, including digest or fixed-root validation before parsing where the
contract requires it.

Internal splitting must not create public byte factories, caller-selected trust roots, fallback parsers,
or alternate admission paths. It also must not generalize unrelated public-synthetic and private-local
formats into one weaker abstraction.

### Use bounded, characterization-preserving refactors

The preferred sequence is:

1. extract Godot input, presentation, profile composition, and smoke responsibilities from the current
   root while preserving commands, observations, marker bytes, and behavior;
2. extract Application command dispatch, pending gates, and snapshot projection behind the existing
   `GameSession` facade; and
3. split a large Content reader internally when an owning change next reaches it, preserving its public
   port and validation order.

Do not perform a repository-wide public-type rewrite or combine these steps with a new gameplay feature.
Existing public types may remain temporarily when changing them would create unrelated compatibility
risk. New work follows this policy immediately; old ceremony is removed only through bounded refactors
with characterization coverage.

### Review architecture by responsibility and amplification

Architecture review considers:

- whether authoritative state or mutation has more than one owner;
- whether a public type names a real boundary reason;
- whether a small behavior change requires unrelated cross-layer edits;
- whether orchestration, projection, and trust validation are concentrated in one file or object;
- whether a view can be reconstructed from authoritative observations; and
- whether failure is attributed to the correct trust, state, or adapter layer.

Raw file length and class count are useful signals, not acceptance criteria. Splitting one monolith into
many public wrappers without reducing responsibility concentration does not satisfy this decision.

## Examples

| Seam | Default weight | Reason |
| --- | --- | --- |
| private canonical or visual-payload admission | Heavy | untrusted local bytes, fixed provenance, and profile trust |
| semantic command admitted by `GameSession` | Heavy | authoritative deterministic mutation |
| stable H4 or smoke observation | Heavy at the serialized boundary | external compatibility and comparison |
| Godot label formatting or project-authored diagnostic color | Light | disposable same-process presentation |
| scene construction and node lookup | Light | reconstructable adapter state |
| helper used only while validating one reader | Light and internal | no independent port or authority |
| one field added to an existing inspector | Extend the existing observation | no capability per diagnostic field |

## Alternatives Rejected

### Make all remake code light-first

Rejected because private input admission, deterministic gameplay, cross-assembly replacement, and H4
comparison require explicit durable contracts. Letting Godot nodes, file readers, or implicit callbacks
own those rules would reverse ADR 0011's accepted architecture.

### Continue the same protocol family for every capability

Rejected because consistency of naming alone does not establish an independent boundary. Repeating the
full protocol shape for same-process helpers expands public surface and orchestration cost without adding
trust, determinism, replaceability, or observation value.

### Replace the lifecycle surface with a universal event framework

Rejected because the current behaviors are heterogeneous and strongly ordered. A generic framework would
hide domain-specific invariants and create a second architectural commitment before reuse is demonstrated.

## Consequences

- Security, provenance, deterministic mutation, cross-assembly ports, and H4 compatibility remain
  deliberately strict.
- Godot presentation and internal orchestration can evolve quickly without inventing public contracts.
- New features must state why each added public type or capability is durable.
- Existing large roots and facades receive bounded, behavior-preserving decomposition before further
  broadening.
- Tests should characterize stable behavior and boundary observations rather than require every helper
  to remain public.
- Some existing public ceremony remains until an owning refactor can remove it safely.

## Non-Goals and Authorization Boundary

This ADR does not change current code, runtime profiles, package identities, capabilities, receipts,
smoke markers, fixtures, or gameplay behavior. It does not report any refactor complete and does not
authorize a public API break, private-input weakening, H4 relaxation, second mutation facade, renderer,
asset publication, or new feature slice.

It authorizes only this granularity policy and future separately owned, characterization-preserving
refactors. Reversing ADR 0011's assembly direction, state ownership, Content trust, or H4 separation still
requires a separate decision.

## References

- [ADR 0008: Godot 4.7.2 .NET/C# CLI-First Remake Tooling](./0008-godot-csharp-cli-first-remake-tooling.md)
- [ADR 0011: Phase 4 Remake Runtime Architecture](./0011-phase4-remake-runtime-architecture.md)
- [ADR 0012: Dependency-Aware Partitioned Verification](./0012-dependency-aware-partitioned-verification.md)
- [ADR 0016: Remake-Start Evidence Deferral](./0016-remake-start-evidence-deferral.md)
