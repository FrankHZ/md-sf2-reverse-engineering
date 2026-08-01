# ADR 0006: Parallel Worktrees with Topic-Branch Integration

- Status: **Accepted**
- Decision date: 2026-08-01
- Scope: repository branches, worktrees, research/design concurrency, and integration gates

## Decision

Keep `main` as the serialized integration branch. Ordinary agent changes use a short-lived topic branch
created from an up-to-date `origin/main` and an isolated Git worktree. A worktree may persist across
slices, but its topic branch does not become a second durable source of truth.

Run at most one active Phase 2 research write lane and one active design-synthesis write lane by default.
The research lane retains ADR 0004's single Terra worker plus independent root acceptance inside its
worktree. The design lane may concurrently explain accepted evidence from `main`, but it cannot alter or
promote research findings, schemas, fixtures, manifests, extractors, or evidence-bound subsystem design
contracts without a separately assigned research slice and declared merge dependency.

Parallel writers never share a worktree. Every slice declares owned files, shared-file needs, acceptance
commands, and branch dependencies. Aggregate files and registries receive one branch owner per change.
Multiple research write lanes are allowed only after their complete parser, fixture, schema, manifest,
index, CLI, and documentation surfaces are demonstrably disjoint.

Integrate one branch at a time. Before acceptance, update the topic branch onto current `main`, resolve
semantic conflicts with the owning lane, rerun the lane-specific checks and `uv run sf2 verify`, scan for
private/generated inputs, stage exact paths, and review the cached diff. The accepted branch is pushed and
reviewed before it enters `main`. Unmerged branch content is collaboration state, not accepted evidence.

## Lane Ownership

The research lane normally owns:

- `docs/research/`, schemas, manifests, extractors, verifiers, and project-owned tests/fixtures;
- evidence-bound subsystem contracts in `docs/design/` when a finding changes; and
- the exact narrow H2/H3 command for its slice.

The design-synthesis lane normally owns:

- cross-subsystem or player-facing design explanations and documentation roadmaps;
- trace links to already accepted research, contracts, and fixtures; and
- explicit **Confirmed**, **Inferred**, and **Unknown** boundaries without creating new evidence.

`README.md`, `docs/README.md`, `docs/research/source-coverage.md`, `AGENTS.md`, central manifests,
`src/sf2tool/cli.py`, `src/sf2tool/design_contracts.py`, and shared schemas/fixtures are integration
hotspots. A slice that needs one names it explicitly; two active branches do not silently edit the same
hotspot.

## Branch and Dependency Rules

- Use `codex/research-*`, `codex/design-*`, `codex/tooling-*`, or `codex/repo-*` topic names.
- Base new work on current `origin/main`; update again immediately before final acceptance.
- Prefer consuming only merged evidence. A stacked branch records its upstream topic branch and required
  merge order in the pull request.
- Keep commits coherent and reviewable. Merge or close a completed topic branch instead of accumulating
  unrelated future slices on it.
- Reserve the primary `main` worktree for integration once the active research worktree has been created.

## Verification and Remote Checks

Research branches run `uv run sf2 verify` plus the owning narrow H2/H3 command. Design-synthesis branches
run `uv run sf2 design-contracts test` and the tracked-input public test profile, followed by the normal
`uv run sf2 verify` at final integration. `uv run sf2 verify --full` remains limited to milestone,
release/merge-readiness, shared-harness, or explicitly requested parity gates.

Full-gate reuse is decided by changed paths and dependencies rather than by commit identity alone. Record
the research topic head/tree that passed the full profile and inspect the path delta from its tested base
to the final `origin/main`. An upstream delta limited to accepted, non-registered Layer B synthesis docs
under `docs/design/` and their `docs/README.md` index entries does not invalidate that result: rebase, then
rerun the research branch's narrow command and normal `uv run sf2 verify`. Do not cancel or restart a full
run merely because such a design branch merged.

The full result is invalid when the research diff changes after it ran, conflict resolution changes
semantics, or an upstream delta touches executable code, tests, schemas, fixtures, manifests,
harness/toolchain configuration, evidence-bound design contracts, or another full-profile input. Any
delta not demonstrably confined to the design-only exception is invalidating. Thus design integration
never requests `verify --full`, while a release or materially affected research integration still can.

GitHub-hosted checks must not receive the private ROM, upstream checkout, derived ROMs, emulator state, or
extracted assets. They may run Ruff, `tests/python/test_native_harness.py`, and design-contract
traceability because those gates use tracked inputs. Private H0/H1/H2/H3 verification remains a local
integration responsibility.

Ignored scratch stays per-worktree. Immutable private inputs may be copied and hash-verified or exposed
through narrow read-only paths, but the complete `local/` tree is not shared when tools could concurrently
write derived artifacts into it.

## Why

ADR 0004 deliberately prevented conflicting writes inside the research worktree, but the project now has
an independent design-documentation stream that can safely synthesize already accepted evidence while
research continues. Direct work on `main` would serialize authoring unnecessarily and make branch state
ambiguous; unrestricted parallel research would instead collide in central fixtures, manifests, counters,
and CLI registries.

Two isolated lanes preserve the proven research acceptance boundary while allowing useful document work
to proceed. A serialized merge queue keeps the repository record, aggregate counters, and evidence labels
coherent.

A design-only merge changes the research branch's eventual commit ancestry but not the executable inputs
covered by its long-running full profile. Treating every new `main` SHA as automatic invalidation wastes
that result and couples independent lanes again. The explicit path exception preserves the result only
where the dependency boundary is demonstrable; all ambiguous or executable deltas remain conservative.

## Consequences

- Ordinary agent work no longer commits directly to `main`.
- The Terra worker still does not stage, commit, branch, or push; its root accepts on the research topic
  branch.
- Design synthesis can progress beside research but consumes accepted evidence by default.
- Branches may require a final rebase and correction when shared contracts changed while they were open.
- A design-only `main` advance does not discard an otherwise applicable passing research full gate.
- Remote CI provides a public tracked-input signal, not a substitute for local private-evidence gates.
- Additional research concurrency requires structural separation of shared outputs before it is enabled.
