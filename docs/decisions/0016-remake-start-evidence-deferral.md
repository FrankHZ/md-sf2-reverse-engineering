# ADR 0016: Remake-Start Evidence Deferral

- Status: **Accepted**
- Decision date: 2026-08-27
- Scope: the relationship between deferred Phase 2 evidence and a separately authorized Phase 4 implementation start

## Context

[ADR 0009](./0009-first-phase4-playable-slice.md) selects the eventual first playable milestone: one
continuous natural scenario from Map 3 through completion of Battle 01. [ADR 0010](./0010-map3-battle01-product-acceptance.md)
preserves its accepted private-local 7C/8C product profile. The [Research audit](../research/map3-battle01-audit.md)
and [readiness ledger](../design/synthesis/map3-battle01-readiness.md) therefore remain **OPEN** and
**NOT READY** for that eventual milestone acceptance surface: natural route, admission, playthrough,
victory, endpoint, original-reference replay, complete 8C capture, continuous-scenario contract, and
H4 work are not accepted facts.

Those evidence gaps are important acceptance work, but treating every one as a default prerequisite
for creating any bounded remake implementation would make the eventual end-to-end acceptance target a
mandatory first implementation slice. That is neither required to preserve the target nor consistent
with static-first research. The project needs an explicit distinction between an eventual milestone
acceptance gate and a separately authorized implementation start.

## Decision

### Preserve the milestone; separate the start decision

This ADR is the controlling amendment for remake-start prerequisites. Where earlier Phase 4
pre-entry wording conflicts with this distinction, this ADR controls the start policy while preserving
the earlier eventual milestone acceptance requirements.

The continuous natural Map 3-through-Battle 01 milestone remains the eventual product and acceptance
target selected by ADRs 0009 and 0010. Its evidence, private-reference, continuous-scenario, and H4
requirements are not weakened, declared complete, or replaced by a bootstrap result.

Natural continuous original-game execution — including natural admission, route, playthrough, victory,
endpoint, original-reference replay, complete 8C capture, the continuous-scenario contract, and H4
completion — is **not a default prerequisite** for starting remake implementation. This is not a claim
that the evidence is unneeded or that it will never be required. A future implementation or acceptance
ambiguity may still require a bounded evidence slice.

The user retains the separate explicit Phase 4 start decision. Under that decision, the user may
authorize a concrete, bounded implementation or bootstrap slice while the readiness ledger remains
**NOT READY**. That authorization starts only the named slice; it does not report the continuous
milestone ready, authorize a product acceptance result, or silently change any Research label.

### Use static-first, conditional evidence routing now

ADR 0014's three-part H3 admission gate applies now, not only after continuous-scenario closure. A
deferred runtime question may receive a new bounded H3 slice only when all of the following hold:

1. accepted static source, H1, ROM, and H2 evidence cannot establish the caller-dependent semantic;
2. the answer would materially affect an accepted implementation-neutral contract or acceptance
   behavior for the concrete work; and
3. an accepted reusable batched scenario or semantic rail cannot be extended to cover it.

The triggering slice first reuses accepted static owners and fixtures. If those are insufficient, it
authorizes only the smallest bounded H3 needed to answer the stated ambiguity. Natural end-to-end
continuity is not the fallback just because a runtime question exists. Questions that do not pass all
three conditions remain **Inferred** or **Unknown** at their existing owner and do not block an
otherwise authorized implementation start by default.

### Retain stop-losses and non-evidence results

This amendment changes priority and routing only. It does not reopen, rename, reset, or reuse failed
R2b or original-reference replay launches or candidates. Their stop-loss and non-evidence status remain
with their owning records. A nominal new slice cannot reset a launch budget or create a fourth launch;
the stop-loss rules in ADR 0015 continue to apply.

### Require only concrete-slice dependencies at bootstrap

The first authorized bootstrap or implementation step requires only the accepted owners necessary for
that concrete slice. It must identify those owners, preserve their proven boundaries, and fail closed
on a missing required input. It does not require every R1-R4, natural-continuity, private 7C/8C, or H4
owner before work begins. Private 7C/8C and H4 capabilities remain conditional later acceptance work
when their concrete implementation or acceptance ambiguity triggers the gate above.

## Consequences

- The readiness ledger remains **NOT READY** and the research audit remains **OPEN** for the eventual
  continuous milestone; those statuses do not automatically bar a separately user-authorized start.
- The first playable milestone continues to require the full accepted profile before its acceptance;
  an implementation bootstrap is not milestone completion.
- The current Research frontier is independent, honest static H2 coverage. Deferred runtime questions
  are routed through the immediate three-part gate rather than an automatic R2/R3/R4 continuity queue.
- Existing evidence labels, fixtures, counters, failure history, and private-input boundaries remain
  unchanged.

## References

- [ADR 0009: First Phase 4 Playable Slice](./0009-first-phase4-playable-slice.md)
- [ADR 0010: Map 3 to Battle 01 Product Acceptance Profile](./0010-map3-battle01-product-acceptance.md)
- [ADR 0014: Static-First Runtime Evidence after Map 3 to Battle 01](./0014-static-first-runtime-evidence-after-map3-battle01.md)
- [ADR 0015: Original-Reference Replay and H4 Boundary](./0015-original-reference-replay-and-h4-boundary.md)
- [Map 3 to Battle 01 Research Gap Audit](../research/map3-battle01-audit.md)
- [Map 3 to Battle 01 Readiness Ledger](../design/synthesis/map3-battle01-readiness.md)
