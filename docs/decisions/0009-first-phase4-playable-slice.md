# ADR 0009: First Phase 4 Playable Slice

- Status: **Accepted**
- Decision date: 2026-08-14
- Scope: first Phase 4 playable milestone and its eventual acceptance evidence gate

## Context

[ADR 0008](./0008-godot-csharp-cli-first-remake-tooling.md) accepts the Godot 4.7.2 .NET/C# CLI-first
engine and tooling baseline, but that acceptance does not start Phase 4. The first implementation
milestone also needs a bounded player-facing target and an evidence gate that prevents engine work
from silently defining original behavior or unresolved product scope.

The existing repository contains substantial Map 3, map-script, exploration, battle-definition, and
Battle 01 evidence. Those separate findings do not by themselves prove that every dependency of one
continuous playable scenario is closed. Reachability of Battle 01, entry into its control flow, and
completion of the battle are distinct boundaries.

## Decision

The first Phase 4 playable milestone is a **continuous playable vertical slice from Map 3 through
completion of Battle 01**.

The slice must not stop at reaching Battle 01, entering the battle, initializing its state, or
demonstrating an isolated battle mechanic. It ends only after the Battle 01 completion boundary
established by the accepted audits has been exercised as part of the same continuous scenario. This
ADR chooses that extent; it does not invent the exact route or define the observable completion
boundary ahead of the required evidence and contract audits.

## Eventual Milestone Acceptance Gate

The continuous playable milestone may be reported ready for its Phase 4 acceptance target only after all
of the following are complete:

1. **Research audit:** the Research lane independently inventories the accepted evidence needed for
   the complete Map 3-to-Battle 01-completion scenario, records every evidence gap in durable
   research-owned artifacts, and closes the gaps required for implementation-neutral fidelity.
2. **Design audit:** the Design lane independently inventories the accepted contracts and synthesis
   needed to implement and accept the same end-to-end scenario, records every contract or product
   acceptance gap in durable design-owned artifacts, and closes the required gaps using accepted
   research from `main`.
3. **Main-gate readiness:** main-gate verifies that the required audit outputs and closures are
   accepted on `main` and reports the scenario ready for a phase-transition decision.
4. **Separate start action:** the user must explicitly authorize Phase 4 implementation. Under
   [ADR 0016](./0016-remake-start-evidence-deferral.md), that authorization may name a bounded
   implementation slice before this eventual milestone gate closes; it does not make the milestone
   ready. Closing the gap gate also does not itself start implementation.

The two audits are independently owned even where their inventories overlap. Neither may treat the
other lane's unmerged findings as accepted project evidence.

## Accepted-Evidence Refresh (2026-08-30)

This refresh records later accepted evidence without changing the 2026-08-14 decision or declaring
the eventual milestone ready. The accepted runtime owners now close three bounded prefixes:

- `sf2-map3-admitted-start-runtime-v1` closes the controlled Map 3 start through the first
  `WaitForEvent`;
- `sf2-map3-battle01-natural-route-runtime-v1` closes the natural opening only through
  `cs_5149A` entry-before-body, with `FieldMenu` **NotReached**; and
- `sf2-map3-messenger-acceptance-runtime-v1` closes the accepted messenger continuation only through
  its follower-ready `WaitForEvent`, again with `FieldMenu` **NotReached**.

The accepted static chain now continues through
`sf2-map3-castle-battle-unlock-static-v1`,
`sf2-map3-battle01-admission-static-v1`,
`sf2-map3-battle01-turn-control-static-v1`,
`sf2-map3-battle01-action-effect-static-v1`,
`sf2-map3-battle01-action-completion-static-v1`,
`sf2-map3-battle01-turn-finalization-static-v1`, and
`sf2-map3-battle01-victory-return-static-v1`. These fixtures close source/H1/ROM topology and local
state/control/caller/content shapes only. They do not prove natural R2a-to-R2b-to-R2c continuity,
caller order or admission reach, a naturally initialized encounter, reached player/AI/action/result
branches, action replay or next-turn dispatch, multi-round play, victory-program execution, return to
exploration, or a stable endpoint.

[ADR 0014](./0014-static-first-runtime-evidence-after-map3-battle01.md) and
[ADR 0016](./0016-remake-start-evidence-deferral.md) keep those caller-dependent facts
**Unknown/Deferred** unless a concrete implementation or acceptance ambiguity passes the immediate
runtime-evidence gate. [ADR 0015](./0015-original-reference-replay-and-h4-boundary.md) keeps the failed
R2b and original-reference candidates non-evidence. None of this accepted static coverage turns
natural continuity, private-reference provenance, complete 8C observation, or continuous H4 into a
closed milestone gate.

The user separately authorized the first bounded Phase 4 implementation slice on 2026-08-28, as
recorded in [`remake/README.md`](../../remake/README.md). That action satisfies this ADR's historical
implementation-start gate only. It does not make the continuous milestone ready, close any evidence
gap above, or report milestone acceptance.

## Required Audit Outputs, Not Decisions Here

The audits must make the following scenario details explicit before eventual continuous-milestone
acceptance, without this ADR preselecting their answers. Under ADR 0016, this list is not a default
prerequisite for every separately authorized bounded implementation slice:

- the detailed Map 3 route, starting state, required transitions, and handoff into Battle 01;
- required story, dialogue, event, exploration, menu, UI, and presentation behavior;
- the exact observable meaning of Battle 01 completion and the slice's ending state;
- save/load scope, if any, for this milestone;
- placeholder or properly licensed asset needs and the private-input boundary;
- H4 parity fixtures, visual-parity expectations, and other acceptance evidence needed for the
  continuous scenario.

An item may be explicitly excluded by an accepted audit result when exclusion still permits the
chosen continuous milestone. Silence is not an exclusion and must not become an implementation
assumption.

## Consequences

This decision gives Research and Design one exact scenario for targeted gap audits and gives a later
Phase 4 implementation a bounded first milestone. It deliberately makes battle completion, not battle
entry, the eventual acceptance stopping condition. ADR 0016 controls whether deferred natural-
continuity evidence is a default prerequisite for a separately authorized implementation start.

At adoption, this decision did **not** create `remake/`, install Godot, run an MCP bakeoff, choose
assets, begin the Research or Design audits, or authorize implementation. Later accepted decisions
and explicit user actions may authorize bounded implementation slices under ADR 0016 without changing
this milestone extent or reporting its eventual acceptance gate complete.
