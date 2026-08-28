# ADR 0014: Static-First Runtime Evidence after Map 3 to Battle 01

- Status: **Accepted**
- Decision date: 2026-08-21
- Scope: Phase 2 H2/H3 selection before and after the ADR 0009 scenario closure

## Context

[ADR 0003](./0003-static-first-batched-runtime-research.md) established static-first subsystem
research and batched runtime observation. It correctly prevents one emulator launch per source branch,
but its runtime-question queue can still be misread as an automatic backlog of future H3 fixtures.
That interpretation would make complete optional and full-game coverage generate per-NPC,
per-dialogue, per-area, and per-item runtime cases even when deterministic source and ROM parsing
already establish the implementation-relevant contract.

[ADR 0009](./0009-first-phase4-playable-slice.md) and
[ADR 0010](./0010-map3-battle01-product-acceptance.md) deliberately retain one continuous scenario
from Map 3 through completion of Battle 01 as the eventual milestone acceptance target. The R2b/R2c
static contracts and later R3/R4 labels do not create an automatic scenario-observation queue. Each
becomes accepted evidence only after its normal independent review and integration to `main`.

The long-term full-game reverse-engineering mission remains unchanged. The question is how much new
runtime observation is justified for a concrete ambiguity, whether or not the selected scenario's
eventual acceptance evidence has closed.

## Decision

### Keep continuous-scenario evidence conditional

Retain the continuous Map 3-to-Battle 01 boundary selected by ADR 0009 as an eventual acceptance
target. Do not treat R2b/R2c/R3/R4 labels as a default requirement to observe that route before a
separately authorized implementation starts, and do not generalize them into a requirement to observe
every optional or later-game content record.

Optional and full-game inventory may proceed statically. A continuous-scenario question uses the same
immediate H3 admission gate as every other deferred question; it does not create a parallel
content-by-content H3 lane or an end-to-end fallback requirement.

For this decision, closing the ADR 0009 scenario means that the required Research and Design evidence
and contract gaps for the continuous scenario have been accepted on `main`. It does not mean Phase 4
has started, that the playable slice has been implemented, or that its H4 acceptance has passed.

### Make complete static corpora the default

Optional and full-game coverage outside the selected scenario defaults to complete deterministic H2
parsing of the owning source/ROM surface. The durable result includes the parser, provenance, schema,
public-safe fixture or ignored private generated JSON as appropriate, and adversarial tests. Static
source shape, data identity, tables, references, and control-flow relationships may be Confirmed by
those deterministic owners.

Do not add per-NPC, per-dialogue, per-area-description, per-item, or equivalent per-record H3 merely
because the static corpus records an unresolved runtime question. A runtime-question group is a
deferred ambiguity register. It is not an automatic emulator-fixture queue, acceptance requirement,
or promise that every Unknown will be observed.

### Admit new H3 only through a three-part gate, now

A new H3 fixture is allowed only when all three conditions hold, including before ADR 0009 scenario
evidence closure:

1. the relevant caller-dependent semantic cannot be established by deterministic static analysis;
2. its answer would materially affect an accepted implementation-neutral contract or acceptance
   behavior; and
3. no existing batched scenario or semantic rail can cover the question without creating a new
   fixture.

Before ownership is granted, the proposed slice contract must identify the exact caller-dependent
question, the affected accepted contract or acceptance behavior, the accepted static/H1/ROM/H2 owners
already reused, and why an existing rail cannot be extended or reused. If the gate passes, related
cases still use one generated matrix and the smallest practical number of emulator launches. A
one-case fixture continues to require a concrete isolation reason.

If the gate does not pass, retain the behavior as Inferred or Unknown at the appropriate boundary.
Do not promote it to Confirmed, infer player-visible semantics from static shape, or block an
otherwise complete static corpus merely to force runtime closure.

### Preserve accepted runtime evidence

This decision removes no accepted H3 fixture, callback/error requirement, evidence label, or affected
verification partition. Existing runtime rails remain durable evidence and continue to run when
selected by their dependencies. This decision changes the threshold for creating additional H3 work;
it does not weaken already accepted evidence or normal verification.

## Relationship to Existing Decisions

This decision supplements ADR 0003. ADR 0003 remains authoritative for static-first inventory,
static-versus-runtime evidence labels, and batching once runtime observation is justified. ADR 0014
clarifies that the question queue is deferred by default and adds the three-part admission gate for
new H3 after the ADR 0009 scenario closure.

ADR 0005's remake-value test and ADR 0013's evidence-preserving efficiency rules remain in force.
Neither token cost nor emulator runtime alone is evidence for skipping a contract-relevant runtime
question that passes this decision's gate. ADR 0016 controls the separate implementation-start policy;
this ADR supplies its immediate runtime admission rule.

## Consequences

- The current Map 3-through-Battle 01 scenario retains its eventual continuous-milestone evidence
  boundary without becoming a default precondition for implementation start.
- Later optional and full-game coverage scales primarily through complete H2 corpora rather than a
  linear growth of content-specific emulator fixtures.
- Deferred H3 question groups remain useful records of uncertainty without becoming an automatic
  work queue.
- New runtime work has an explicit implementation-value and rail-reuse justification before it
  consumes a Research lane.
- Phase 4 start authorization is separately controlled by ADR 0016; milestone acceptance, Godot work,
  and H4 completion remain distinct later gates.

## References

- [ADR 0003: Static-First Research with Batched Runtime Observation](./0003-static-first-batched-runtime-research.md).
- [ADR 0005: Remake-Value-Driven Driver and Hardware Freeze](./0005-remake-value-driven-driver-freeze.md).
- [ADR 0009: First Phase 4 Playable Slice](./0009-first-phase4-playable-slice.md).
- [ADR 0010: Map 3 to Battle 01 Product Acceptance Profile](./0010-map3-battle01-product-acceptance.md).
- [ADR 0013: Token-Efficient Agent Research without Weakening Evidence](./0013-token-efficient-agent-research-workflow.md).
- [ADR 0016: Remake-Start Evidence Deferral](./0016-remake-start-evidence-deferral.md).
- [Map 3 to Battle 01 Research Audit](../research/map3-battle01-audit.md).
