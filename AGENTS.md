# Agent Guide

## Mission and Source of Truth

This repository reverse engineers the US Mega Drive/Genesis release of **Shining Force II**, turns
original behavior into reproducible evidence and implementation-neutral contracts, and uses those
contracts to build an independently maintained remake.

Tracked repository owners and exact Git objects are the durable source of truth. Chats, handoffs,
ignored reports, and external agent memory are coordination state, not project evidence. Record every
durable finding, decision, reproduction command, unresolved question, and acceptance boundary in its
owning tracked document or executable contract.

Research describes the original. Design contracts express accepted behavior without choosing an
engine. Remake code consumes those contracts and must not become evidence for claims about the
original game.

## Start and Route

1. Apply this file once. If the client already injected it, do not reread it.
2. Derive the live commit, tree, status, worktrees, branches, and recent history from Git before
   assuming any lane, artifact, counter, or directory exists.
3. Read the compact [agent resume route](./docs/operations/agent-resume.md), then the closest owning
   document and only the decisions or checklists required by the task.
4. Read the complete root README, documentation index, or source-coverage ledger only when changing
   or auditing their global scope, routing, counters, or frontier.
5. Reproduce a claim from its named command and fixture instead of copying an old progress summary.

Use these durable routes rather than duplicating their details here:

| Concern | Owner |
| --- | --- |
| Project entry, stable status, and layout | [README](./README.md) and [documentation index](./docs/README.md) |
| Fresh-task routing | [Agent Resume Route](./docs/operations/agent-resume.md) |
| Ordinary Phase 2 lane mechanics | [Phase 2 Lane Runbook](./docs/operations/phase2-lane-runbook.md) and [ADR 0004](./docs/decisions/0004-single-terra-worker-with-root-acceptance.md) |
| Research coverage and cadence | [Source Coverage](./docs/research/source-coverage.md), [ADR 0003](./docs/decisions/0003-static-first-batched-runtime-research.md), and [ADR 0014](./docs/decisions/0014-static-first-runtime-evidence-after-map3-battle01.md) |
| Verification selection | [ADR 0012](./docs/decisions/0012-dependency-aware-partitioned-verification.md) |
| Local private inputs | [Local Private Inputs](./docs/operations/local-private-inputs.md) |
| Remake implementation | [Remake README](./remake/README.md) and its linked architecture, profile, capability, and verification owners |
| Bounded implementation start | [ADR 0016](./docs/decisions/0016-remake-start-evidence-deferral.md) |

An ordinary Phase 2 handoff uses the complete slice contract defined by ADR 0004; its Worker
Acceptance Checklist is the normative detailed acceptance profile.
For H3, callback exceptions must reach the status/exit contract. New schema placement follows
[`schemas/README.md`](./schemas/README.md).

## Agent and Session Routing

Use [ADR 0018](./docs/decisions/0018-astra-role-routing-trial.md) for accepted Astra/Sol role routing,
its default models, task-migration boundary, and completed trial's exit decision. Dedicated owners
may execute their complete lane; reserve `gpt-5.6-terra` for an explicitly bounded single-file, single-assembly, or
single-function reverse-engineering subtask, never a whole lane or integration. This routing replaces
ADR 0004's earlier model and mandatory-worker choice; its evidence, handoff, and root-acceptance
checklist remains normative for any bounded Phase 2 task.

Do not silently replace a long-lived lane owner with an in-thread subagent when its context becomes
unreliable or a fresh session is needed. Stop, tell the user, and let the user create the replacement
session. Keep in-thread subagents limited to small, independent subtasks.

Before assigning work to any replacement session, send one compact current-state anchor: canonical
repository and forbidden paths, worktree and branch, accepted base, exact owned paths, current
commit/dirt/process/gate state, preserved failures and Unknowns, and the next stopping condition.
Require a read-only state check before mutation, and state that this anchor supersedes stale or
replayed instructions from compacted history. The main gate retains independent review and merge
authority.

## Git, Ownership, and Integration

`main` is the serialized integration branch, not an ordinary write worktree. Start a new slice from
current accepted `origin/main` in an isolated worktree and a short-lived topic branch whose prefix
matches the lane: `codex/research-*`, `codex/design-*`, `codex/tooling-*`, `codex/remake-*`, or
`codex/repo-*`.

Before editing, declare the exact owned paths, shared-path needs, dependencies, semantic boundary,
and acceptance commands. Check all live worktrees and open topics for competing ownership. One path
has one active writer; never run parallel writers in one worktree. Serialize shared registries,
indexes, routing documents, schemas, fixtures, and aggregate tests.

Consume accepted `main` only. If a slice truly depends on unmerged work, declare the stacked
dependency and merge order. Never cite an unmerged conclusion as a repository fact or silently copy
another topic's code.

Keep ignored writable state isolated per worktree. A specifically registered immutable private input
may be resolved read-only as described by the local-private-input owner, but writable emulator state,
derived assets, exports, traces, reports, and scratch remain local to their owning worktree.

Before handoff, update onto current `origin/main`, resolve ownership conflicts semantically, run the
selected gates, stage only the declared paths, inspect the cached diff and private boundary, commit,
push, and leave a Draft PR for independent integration. An unmerged branch is collaboration state,
not a second source of truth. Remove only an accepted topic's own worktree and refs after explicit
merge-cleanup authorization.

## Evidence and Provenance

Use exactly these evidence labels:

- **Confirmed:** reproduced by a project-owned command/test or directly supported by named source,
  ROM, and runtime observations appropriate to the claim.
- **Inferred:** strongly supported but not independently reproduced at the required boundary.
- **Unknown:** an explicit open question; never fill it with a convenient assumption.

For non-trivial original-game claims, retain enough provenance to reproduce the result: private input
identity, pinned upstream repository and commit, source symbol or ROM/RAM address, tool version,
command, fixture, and observed result as applicable. Preserve disagreements and design a focused test
instead of selecting the convenient source.

Static source shape proves structure, not natural reach, caller state, timing, presentation, or
hardware behavior. A bounded H3 observation proves only its named seam. Apply the static-first and
three-part runtime-admission rules in ADRs 0003, 0014, and 0016; an Unknown register is not an
automatic emulator-work queue.

Do not claim strict clean-room development when implementers have inspected disassembly. Preserve the
practical boundary: research owns provenance and behavior; remake code consumes accepted contracts
and uses project-authored, properly licensed, or explicitly local private content.

## Baseline and Private Boundary

The original baseline is the USA retail ROM identified by tracked manifests and the root README, plus
the pinned `ShiningForceCentral/SF2DISASM` `master` revision. Never rely on a floating default branch,
substitute a community feature branch for the original, or modify the only canonical private input to
match a tool's filename convention. Copy required inputs into ignored scratch and verify their exact
identity before use.

ROMs, patches, rebuilt ROMs, SRAM, save states, traces, movies, memory dumps, extracted game content,
downloaded executables, ignored upstream checkouts, and generated exports are private or generated by
default. Do not commit, upload, attach, redistribute, or leak their absolute paths. Public fixtures
contain only the minimum redistributable facts needed to express a contract. Public remake builds use
project-authored or properly licensed assets; a successful local/private gate grants no distribution
right.

Third-party source or binaries require pinned provenance and an explicit compatible license before
vendoring. A public repository is not itself permission to copy or relicense its contents.

## Verification Rules

`uv` owns the Python environment and lock. Use `uv sync --locked`; do not create a parallel
requirements workflow or install project dependencies into the system interpreter.

The normal public commit gate is:

```powershell
uv run sf2 verify
```

Pair it with the narrow test or H2/H3/remake command that owns the changed slice. On a clean committed
head, use `uv run sf2 verify plan --base origin/main --head HEAD` to obtain the dependency-aware gate
selection. The planner is authoritative for selected partitions; an unclassified path causes visible,
conservative fanout rather than permission to skip a gate.

`uv run sf2 verify --full` is exceptional. Run it only for a phase milestone, release/merge readiness,
shared harness or legacy-rail semantics, an upstream change that invalidates the full profile, or an
explicit full-parity request. It is not the default for ordinary research, design, documentation, or
bounded remake work. Follow ADR 0012 and the owning runbook for exact invalidation rules.

Treat a completed long-running suite that reports failures as discovery evidence, not as a reason to
repeat the whole suite after every correction. Preserve its exact failing node IDs and result. Verify
the correction by rerunning those nodes, or their owning test files when node IDs are unavailable,
plus the narrow gates selected by the changed paths. Do not rerun the complete slow suite in the same
slice merely to replace a red aggregate result with a green one. Repeat it only when the user
explicitly requests that rerun or the authoritative planner or runbook requires it because the
correction itself broadened invalidation.

Carry completed suite results, failing nodes, and process completion state in replacement-session
handoffs. Never describe a completed failing run as interrupted or unknown and restart it from
scratch.

Gate invalidation is path- and dependency-based, not commit-SHA-based. A `main` advance limited to
accepted, non-registered Layer B design-synthesis documents does not invalidate an already passing
research full gate; after rebase, run the owning narrow command plus the normal `uv run sf2 verify`.
A design-synthesis branch or a design-only advance of `main` never triggers it.

Never weaken a golden, schema, fixture, digest, or accepted comparison merely to make a change pass.
First determine whether evidence, extraction, documentation, or implementation is wrong, then update
the owning surface with preserved failure history.

Tools must treat canonical inputs as read-only and write reproducible output only to an explicit
ignored destination. Do not overwrite, normalize, or mutate source evidence as a side effect of
verification. Keep private/generated artifacts out of Git and public CI.

## Change Discipline

- Make one narrow, reviewable change with one clear owner and acceptance boundary.
- Search for an existing owner, parser, schema, fixture, port, or reducer before creating another.
- Do not add empty scaffolding, speculative abstractions, or duplicate state authorities.
- Keep original-fidelity facts separate from remake design and intentional deviations.
- Do not report a subsystem complete while its unsupported capabilities and Unknowns are hidden.
- Route ordinary Phase 2 delegation, correction, recovery, and blockers through the complete Phase 2
  lane runbook instead of recreating its mechanics here.
- Keep the legacy `scripts/*.ps1` compatibility surface frozen; new maintained logic belongs in
  `src/sf2tool/` unless an accepted owner says otherwise.

On Windows, prefer PowerShell-native filesystem operations and pass native executable arguments as
separate values; keep paths literal, validated, and UTF-8-safe.

## Definition of Done

A slice is done only when its exact diff matches declared ownership, relevant docs and executable
contracts agree, provenance and Unknowns are explicit, outputs are reproducible, private/generated
artifacts remain untracked, the committed planner and proportional owning gates pass, normal public
verification is honestly reported, the worktree is clean, and a Draft PR is frozen for independent
review. If a required gate cannot run, report the exact dependency or boundary; do not substitute
confidence, broaden authority, or claim success.
