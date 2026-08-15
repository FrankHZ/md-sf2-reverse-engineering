# ADR 0009: First Phase 4 Playable Slice

- Status: **Accepted**
- Decision date: 2026-08-14
- Scope: first Phase 4 playable milestone and its pre-entry evidence gate

## Context

[ADR 0008](./0008-godot-csharp-cli-first-remake-tooling.md) accepts the Godot 4.7.1 .NET/C# CLI-first
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

## Pre-Entry Gap Gate

Phase 4 implementation may begin only after all of the following are complete:

1. **Research audit:** the Research lane independently inventories the accepted evidence needed for
   the complete Map 3-to-Battle 01-completion scenario, records every evidence gap in durable
   research-owned artifacts, and closes the gaps required for implementation-neutral fidelity.
2. **Design audit:** the Design lane independently inventories the accepted contracts and synthesis
   needed to implement and accept the same end-to-end scenario, records every contract or product
   acceptance gap in durable design-owned artifacts, and closes the required gaps using accepted
   research from `main`.
3. **Main-gate readiness:** main-gate verifies that the required audit outputs and closures are
   accepted on `main` and reports the scenario ready for a phase-transition decision.
4. **Separate start action:** after readiness is reported, the user must explicitly authorize the
   Phase 4 transition. Closing the gap gate does not itself start implementation.

The two audits are independently owned even where their inventories overlap. Neither may treat the
other lane's unmerged findings as accepted project evidence.

## Required Audit Outputs, Not Decisions Here

The audits must make the following scenario details explicit before implementation, without this ADR
preselecting their answers:

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
entry, the stopping condition.

This decision does **not** create `remake/`, install Godot, run an MCP bakeoff, choose assets, begin the
Research or Design audits, or authorize implementation. Those remain separate owned slices under the
pre-entry gate and the later explicit phase transition.
