# ADR 0003: Static-First Research with Batched Runtime Observation

- Status: **Accepted**
- Decision date: 2026-07-18
- Scope: Phase 2 reverse engineering, H2 extraction, and H3 observation cadence

## Decision

Reverse one coherent subsystem statically before adding runtime cases. The static pass inventories
the complete local source scope, extracts stable data and control-flow contracts, records unknowns,
and produces an explicit queue of questions that require original-machine behavior.

Related runtime questions are then executed as a matrix in one BizHawk launch wherever their setup
and observation points can be shared. The observer should consume a generated case table and emit a
small result table or state facts. A new emulator launch per branch is not the default workflow.

Static code shape and source/ROM parity may be `Confirmed` by deterministic parsers and tests.
Behavior that depends on caller context, timing, persistence, RNG, signedness/overflow, or emulator/
hardware behavior remains `Inferred` until the batch observation reproduces it.

## Why

The first 52 H3 fixtures established trustworthy rails but reached only 29 of the pinned checkout's
387 executable-code ASM files. BizHawk startup and natural scenario setup dominate the cost of many
small fixtures, while the pinned bit-perfect disassembly already exposes large amounts of table and
control-flow structure that can be audited cheaply and consistently.

Batching preserves runtime evidence where it matters while moving discovery throughput toward whole
subsystems. It also creates a clearer remake handoff: static inventories define the full surface,
and the runtime matrix highlights only the rules whose meaning was not self-evident from source.

## Consequences

- New Phase 2 work starts with a source inventory and structured static model.
- Runtime fixtures are selected from an explicit ambiguity list, not created for every source branch.
- A coherent matrix should reuse one observer, one derived-ROM instrumentation seam if needed, and
  one emulator launch; single-case launches require a concrete isolation reason.
- Static and runtime evidence labels remain distinct in research and design documents.
- Ordinary commits still run `uv run sf2 verify` plus the owning narrow parser or runtime command;
  `verify --full` remains a milestone gate.
- The current `battle.ai` work becomes the first subsystem using this cadence.
