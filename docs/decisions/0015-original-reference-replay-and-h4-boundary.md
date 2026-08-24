# ADR 0015: Original-Reference Replay and H4 Boundary

- Status: **Accepted**
- Decision date: 2026-08-24
- Scope: original-console scenario replay, private reference capture, and remake H4 ownership

## Context

[ADR 0009](./0009-first-phase4-playable-slice.md) selects one continuous Map 3-through-completion-of-
Battle 01 milestone. [ADR 0010](./0010-map3-battle01-product-acceptance.md) requires a private-local
original-fidelity profile and exact reached frame, audio, and hardware acceptance. [ADR 0011](./0011-phase4-remake-runtime-architecture.md)
separates original evidence, deterministic remake behavior, Godot adapters, and layered H4 results.
Those decisions do not yet name the boundary between replaying the original ROM to acquire reference
evidence and passing H4 against a remake.

That distinction is necessary before any R4b or H4 runner work. An original-console replay can prove
what the pinned game did under one controlled input trace. It cannot prove that a remake matches it.
Conversely, a remake H4 runner must consume accepted evidence; it must not generate its golden by
running an original emulator inside the same comparison and must not treat Lua as a second gameplay
implementation.

The current accepted frontier is deliberately **static-only / Unknown** at this boundary. The accepted
R4a evidence records a static victory/after-battle/return spine, while natural execution and the stable
endpoint remain Unknown. R4b has not been implemented. For R4b original-reference replay and remake H4,
the launch counts are therefore **H3 = 0** and **H4 = 0**. This decision adds no fixture, schema,
research-index record or association, address binding, CLI command, verification partition, or
counter: every such delta is zero in this governance slice.

## Decision

### Use distinct names and evidence owners

The following terms are normative:

- **Original-reference replay** means deterministic execution of the pinned original ROM in the
  accepted original-runtime toolchain from declared private-local start inputs, driven by one frozen
  non-adaptive input trace.
- **Original-reference capture** means the private output set produced by that replay, such as a movie,
  save state, SRAM image, frame/audio capture, trace, or bounded state observation.
- **Original-reference receipt** means the typed result that identifies the replay inputs, runner,
  observed completion status, private artifact identities, and cleanup outcome.
- **H4 fixture** means an accepted, implementation-neutral comparison definition that consumes
  Research-owned reference evidence and the product/architecture decisions. It does not execute the
  original ROM as gameplay logic.
- **H4 PASS** means that the remake has executed the accepted H4 profile and satisfied every applicable
  fixture layer, private-reference comparison, cleanup rule, and declared-deviation rule. An
  original-reference replay, matching original capture, emulator exit code, or clean Lua Console is
  never by itself an H4 PASS.

Original-reference replay is a Phase 2 Research/H3 evidence operation. H4 definition is a Design-owned
acceptance surface before Phase 4, and H4 execution is a later remake acceptance operation after the
separate Phase 4 start action. Neither owner may rename original replay as remake parity or use an H4
label to promote unaccepted runtime observations.

### Keep the original observer passive

An original-reference replay uses the original program as the sole route, collision, zone, warp,
battle, and campaign-mechanics implementation. Its Lua observer may:

- register bounded callbacks at source-backed lifecycle checkpoints;
- read registers, memory, bus facts, and emulator-provided frame/audio facts;
- emit typed observations and bounded diagnostics;
- verify callback completion, timeout, exit, and cleanup state; and
- identify or hash private artifacts without embedding their payloads in public output.

For this replay class, Lua must not write RAM or registers, redirect the program counter, patch the
ROM, choose an input based on observed state, synthesize a missing transition, or reconstruct route,
collision, zone, warp, pathfinding, battle, reward, or after-battle mechanics. The frozen input trace
is supplied by the runner or emulator movie/input boundary; Lua does not become an input policy.

A checkpoint table may name what to observe and when to fail. It may not calculate the state that the
original game should have reached and then substitute that calculated state for observation. Host-side
validation may compare the emitted receipt with an accepted fixture only after the original program
has produced the facts.

### Preserve the private and copyright boundary

Original ROMs, movies, save states, SRAM images, input recordings that embed emulator state, frame or
audio captures, screenshots, traces, memory dumps, decoded assets, and detailed replay receipts remain
ignored private/generated inputs under `local/`. They are never committed, uploaded, attached to a PR,
redistributed, required by Public CI, or pasted into a public handoff.

Tracked artifacts may contain only separately reviewed, public-safe schema and fixture definitions,
allowed identity/provenance, non-reconstructive aggregates, and PASS/FAIL/Unknown results. A hash may
identify a private local input when the owning contract explicitly permits that identity; it does not
make the payload public or safe to distribute. Missing private inputs make the private profile
Unavailable, not Passed and not eligible for a silent substitute.

### Give every future artifact one lane owner

This decision reserves ownership classes without creating their paths or commands:

| Surface | Future owner and boundary |
| --- | --- |
| Original-reference runner capability | One Phase 2 Research tooling slice under the H3/original-runtime boundary. It owns runner preflight, passive-observer enforcement, typed receipts, timeouts, cleanup, and capability tests, but no R4b scenario claim. |
| R4b scenario reference evidence | A later, separate Research scenario-evidence slice. It consumes the accepted capability runner and owns the frozen R4b input trace, private captures, public-safe Research projection, evidence labels, provenance, and any justified research-index associations. |
| H3 fixture and counter | The scenario-evidence slice owns any future `schemas/h3/` and `tests/fixtures/h3/` entry plus its H3 registry/native-counter update. Capability-only diagnostics do not increment scenario-fixture counters. |
| H4 fixture and counter | A later Design/H4-definition slice owns future `schemas/h4/` and `tests/fixtures/h4/` definitions and a distinct H4 counter. It consumes accepted Research from `main`; it does not relabel an H3 receipt. |
| Research index | The Research scenario-evidence slice owns reference-evidence records/associations. A later H4-definition slice may add only the accepted H4 association needed by its fixture. Both are serialized shared-file changes with exact parsed-object proofs. |
| CLI | The capability/evidence runner is registered in the H3 original-reference namespace. A future remake comparator is registered in a distinct H4 namespace by the H4 harness owner. Exact leaf command spelling is selected by those implementation slices. |
| Verification planner | Original replay selects an H3 partition serialized on `bizhawk-original-runtime`. H4 receives a distinct future partition and resource lock when its executable harness exists; it is not hidden inside an H2 or H3 partition. |

No later slice may combine these owners merely to avoid a serialized review. A capability result is not
scenario evidence, a scenario reference is not an H4 definition, and an H4 definition is not a remake
PASS.

### Require a deterministic replay receipt and typed cleanup

Every diagnostic or frozen acceptance launch writes to a fresh ignored output directory and emits one
typed receipt. The receipt records, at minimum:

- scenario and run-class identity (`diagnostic` or `frozen-acceptance`);
- exact ROM, emulator/core, runner, observer, configuration, start-state, SRAM/save-state/movie/input-
  trace, and accepted evidence identities as applicable;
- the declared non-adaptive logical input/replay identity and deterministic clock/configuration facts;
- launch ordinal, bounded timeout result, process exit, Lua/parser/callback status, observed terminal
  checkpoint, and typed failure reason;
- private capture inventory by allowed identity and hash, never embedded payload;
- callback unregistration, emulator/process termination, temporary-file disposition, and residual-
  process/callback checks; and
- the canonical receipt identity after excluding explicitly non-semantic local diagnostics such as a
  wall-clock timestamp or absolute machine path.

An acceptance receipt is valid only when the declared frozen inputs were fixed before launch, the
expected original lifecycle produced the terminal observation, the Lua Console and typed callback
status are clean, and cleanup is complete. Timeout, callback exception, missing terminal state,
unexpected process survival, cleanup failure, or input/capture identity drift is a failed receipt. The
runner must not overwrite a prior receipt or delete an arbitrary caller path to recover from a
collision.

### Sequence capability before scenario evidence

Runtime work proceeds in two separately reviewed slices:

1. **Runner capability slice:** establish deterministic input replay, passive observation, receipt
   shape, bounded process lifecycle, private-output isolation, and cleanup against a capability case.
   It makes no R4b natural-route, victory, endpoint, or H4 claim.
2. **Scenario evidence slice:** only after the capability slice is independently accepted and merged,
   consume the unchanged runner for the R4b scenario. Freeze the scenario inputs before its acceptance
   launch, classify every observed and unobserved claim, and land only the reviewed public-safe
   projection plus required associations.

Only accepted scenario evidence on `main` may feed a later H4-definition slice. H4 runner
implementation and remake H4 execution remain after the ADR 0009 readiness gate and separate user
Phase 4 start action.

### Enforce a hard launch stop-loss

Each authorized runner-capability or scenario-evidence slice may perform at most **two diagnostic
launches followed by one frozen acceptance launch**. There is no fourth launch. A launch counts when
the emulator process starts; a preflight failure before process creation does not count. A diagnostic
launch may improve observation or identify a runner defect, but it may not change original mechanics
or generate scenario truth through a simulator.

The acceptance inputs, observer, expected checkpoints, and output contract are frozen before the
acceptance process starts. If that run diverges, times out, exposes a missing capability, or fails
cleanup, the result remains Failed or Unknown and the lane stops. It must not add Lua route logic,
collision handling, zone/warp selection, battle simulation, adaptive input, or another launch to make
the golden pass. Opening a nominally new slice only to reset the launch count is prohibited; further
runtime work requires independently accepted new static evidence or a separately corrected runner
capability and fresh main-gate ownership.

## Relationship to Existing Decisions

[ADR 0001](./0001-bizhawk-for-h3-runtime-observation.md) remains authoritative for the pinned BizHawk
toolchain, parser/callback failure boundary, and ignored runtime outputs. This decision narrows the new
original-reference replay class to passive observation even though older bounded H3 fixtures may use
controlled RAM inputs.

[ADR 0012](./0012-dependency-aware-partitioned-verification.md) remains authoritative for the current
public core and H1/H2/H3 planner. This decision reserves a future first-class H4 owner; it does not
change the planner. [ADR 0014](./0014-static-first-runtime-evidence-after-map3-battle01.md) continues
to require static-first Research and does not turn a runtime-question register into automatic replay
authorization.

## Consequences

- The original emulator remains an evidence source, not the remake gameplay core.
- A clean original replay can be reviewed without being mislabeled as remake parity.
- Private movies, states, SRAM, and captures remain usable locally without entering Git or Public CI.
- Runner bring-up has a bounded diagnostic budget and cannot grow into a Lua reimplementation.
- H3 and H4 fixtures, counters, indexes, CLI commands, and planner partitions acquire explicit,
  serialized owners when their separately authorized slices begin.
- Current R4b and H4 state remains static-only / Unknown with zero launches and zero repository
  evidence-counter delta.
- This decision does not start R4b, H4, Godot, `remake/`, Phase 4, or any runtime process.

## References

- [ADR 0001: BizHawk for H3 Runtime Observation](./0001-bizhawk-for-h3-runtime-observation.md).
- [ADR 0009: First Phase 4 Playable Slice](./0009-first-phase4-playable-slice.md).
- [ADR 0010: Map 3 to Battle 01 Product Acceptance Profile](./0010-map3-battle01-product-acceptance.md).
- [ADR 0011: Phase 4 Remake Runtime Architecture](./0011-phase4-remake-runtime-architecture.md).
- [ADR 0012: Dependency-Aware Partitioned Verification](./0012-dependency-aware-partitioned-verification.md).
- [ADR 0014: Static-First Runtime Evidence after Map 3 to Battle 01](./0014-static-first-runtime-evidence-after-map3-battle01.md).
- [Map 3 Battle 01 Victory and Return Static Evidence](../research/map3-battle01-victory-return.md).
