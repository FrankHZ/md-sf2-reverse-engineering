# ADR 0014: Static-First Runtime Evidence after Map 3 to Battle 01

- Status: **Accepted**
- Decision date: 2026-08-21
- Scope: Phase 2 H2/H3 selection during and after the ADR 0009 scenario closure

## Context

[ADR 0003](./0003-static-first-batched-runtime-research.md) established static-first subsystem
research and batched runtime observation. It correctly prevents one emulator launch per source branch,
but its runtime-question queue can still be misread as an automatic backlog of future H3 fixtures.
That interpretation would make complete optional and full-game coverage generate per-NPC,
per-dialogue, per-area, and per-item runtime cases even when deterministic source and ROM parsing
already establish the implementation-relevant contract.

[ADR 0009](./0009-first-phase4-playable-slice.md) and
[ADR 0010](./0010-map3-battle01-product-acceptance.md) deliberately require one continuous scenario
from Map 3 through completion of Battle 01. Under the current user scheduling direction, the R2b and
R2c continuations and later R3 and R4 Research slices retain scenario-specific observation because
together they must close that exact route, admission, playthrough, victory, after-battle, and endpoint
evidence boundary. They are a bounded milestone exception, not the default cadence for later
full-game inventory. These labels govern scheduling only; each slice becomes accepted evidence only
after its normal independent review and integration to `main`.

The long-term full-game reverse-engineering mission remains unchanged. The question is how much new
runtime observation is justified after the selected scenario's evidence gate is closed.

## Decision

### Concentrate scenario observation on the first playable slice

Retain the scenario-specific H3 work required by the user-scheduled R2b/R2c/R3/R4 closure sequence
because it collectively proves the continuous Map 3-to-Battle 01-completion boundary selected by
ADR 0009. Do not generalize that scenario cadence into a requirement to observe every optional or
later-game content record.

Until the scenario evidence closure, new scenario-specific H3 is limited to that owned closure
sequence. Optional and full-game inventory may proceed statically, but it does not create a parallel
content-by-content H3 lane.

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

### Admit new H3 only through a three-part gate

After the ADR 0009 scenario evidence closure, a new H3 fixture is allowed only when all three
conditions hold:

1. the relevant caller-dependent semantic cannot be established by deterministic static analysis;
2. its answer would materially affect an accepted implementation-neutral contract or acceptance
   behavior; and
3. no existing batched scenario or semantic rail can cover the question without creating a new
   fixture.

Before ownership is granted, the proposed slice contract must identify the exact caller-dependent
question, the affected accepted contract, and why an existing rail cannot be extended or reused. If
the gate passes, related cases still use one generated matrix and the smallest practical number of
emulator launches. A one-case fixture continues to require a concrete isolation reason.

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
question that passes this decision's gate.

## Consequences

- The current Map 3-through-Battle 01 scenario retains the runtime evidence needed to close its exact
  continuous milestone boundary.
- Later optional and full-game coverage scales primarily through complete H2 corpora rather than a
  linear growth of content-specific emulator fixtures.
- Deferred H3 question groups remain useful records of uncertainty without becoming an automatic
  work queue.
- New runtime work has an explicit implementation-value and rail-reuse justification before it
  consumes a Research lane.
- Phase 4, Godot work, H4 implementation, and the ADR 0009 phase-transition gate are unchanged.

## References

- [ADR 0003: Static-First Research with Batched Runtime Observation](./0003-static-first-batched-runtime-research.md).
- [ADR 0005: Remake-Value-Driven Driver and Hardware Freeze](./0005-remake-value-driven-driver-freeze.md).
- [ADR 0009: First Phase 4 Playable Slice](./0009-first-phase4-playable-slice.md).
- [ADR 0010: Map 3 to Battle 01 Product Acceptance Profile](./0010-map3-battle01-product-acceptance.md).
- [ADR 0013: Token-Efficient Agent Research without Weakening Evidence](./0013-token-efficient-agent-research-workflow.md).
- [Map 3 to Battle 01 Research Audit](../research/map3-battle01-audit.md).
