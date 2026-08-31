# Agent Resume Route

This is the compact entry point for a fresh project task. It routes readers to durable owners without
duplicating their changing counters, findings, branch names, or commit identities.

Apply the stable repository rules in [`AGENTS.md`](../../AGENTS.md) once before using this route. This
file locates task owners; it does not replace the evidence, ownership, safety, verification, or
Definition-of-Done rules in that guide.

## Runtime Identity First

Derive live repository state from Git before reading project-wide prose:

```powershell
git status --short --branch
git rev-parse HEAD
git rev-parse 'HEAD^{tree}'
git worktree list --porcelain
git log -5 --oneline --decorate
```

Do not copy these results into this file. Topic branches consume accepted `origin/main`; active lanes
and dependencies are worktree and handoff state, not a second tracked source of truth.

## Stable Project Boundaries

- The long-term mission remains full-game reverse engineering of the pinned US release plus an
  independently maintained remake.
- Phase 2 research and evidence-bound contracts remain active.
- Godot 4.7.2 .NET/C# is the accepted Phase 4 baseline. The user explicitly authorized a bounded
  Phase 4 implementation start on 2026-08-28 under ADR 0016. [`remake/`](../../remake/) now contains
  bounded Domain, Application, validated Content, and thin Godot capabilities for public-synthetic and
  private-local Map 3 profiles. Its [capability owner](../../remake/docs/capability-status.md) records
  what is runnable, diagnostic, admitted-but-unconsumed, Unsupported, or Unknown.
- Research scheduling and its live frontier belong to
  [`research/source-coverage.md`](../research/source-coverage.md), not this route. The continuous Map
  3-through-completion-of-Battle 01 milestone and H4 result remain incomplete eventual acceptance
  work, not an automatic runtime-closure queue or default implementation prerequisite for an
  authorized bounded slice.
- Private ROMs, extracted assets, runtime captures, downloaded tools, and generated binaries remain
  local and untracked.
- Repository documents and exact Git objects are durable state. Old chats and external memory are not.

## Route by Task Ownership

Read the smallest owning surface that can answer or govern the task:

| Task | Required owner |
| --- | --- |
| Ordinary Phase 2 evidence slice | [`phase2-lane-runbook.md`](./phase2-lane-runbook.md), [ADR 0004](../decisions/0004-single-terra-worker-with-root-acceptance.md), its Worker Acceptance Checklist, the closest [`research/`](../research/) owner, and only the bounded sources named in the slice |
| Research coverage, aggregate frontier, or cadence | [`research/source-coverage.md`](../research/source-coverage.md), [ADR 0003](../decisions/0003-static-first-batched-runtime-research.md), and [ADR 0016](../decisions/0016-remake-start-evidence-deferral.md) |
| Evidence-bound subsystem contract | Closest [`design/contracts/`](../design/contracts/) owner and its accepted research dependencies |
| Cross-subsystem or player-facing synthesis | [`design/documentation-roadmap.md`](../design/documentation-roadmap.md) and the closest [`design/synthesis/`](../design/synthesis/) owner |
| Phase 4 implementation, readiness, or first playable milestone | [`remake/README.md`](../../remake/README.md) and its architecture/profile/capability/verification owners, plus [ADR 0008](../decisions/0008-godot-csharp-cli-first-remake-tooling.md), [ADR 0009](../decisions/0009-first-phase4-playable-slice.md), [ADR 0010](../decisions/0010-map3-battle01-product-acceptance.md), [ADR 0011](../decisions/0011-phase4-remake-runtime-architecture.md), and [ADR 0016](../decisions/0016-remake-start-evidence-deferral.md) |
| Verification selection | [ADR 0012](../decisions/0012-dependency-aware-partitioned-verification.md) and `uv run sf2 verify plan --base origin/main --head HEAD` on a clean committed head |
| Large artifact or diff inspection; PR handoff or review | [`bounded-inspection-and-review.md`](./bounded-inspection-and-review.md) |
| Global documentation routing or decision inventory | [`../README.md`](../../README.md), [`docs/README.md`](../README.md), and the affected index owners |

Use `uv run sf2 research-index list --summary` when current indexed totals matter. Do not copy totals
from an old handoff or maintain them here.

## Bounded Worker Handoff

For an ordinary independent Phase 2 slice, follow the normative
[`phase2-lane-runbook.md`](./phase2-lane-runbook.md). It owns the exact role, bounded no-history
handoff, same-worker correction, recovery, acceptance, and escalation rules. This resume route does
not duplicate them.

## When to Read the Global Documents

Read the complete root README, documentation index, or source-coverage ledger when the task owns or
audits their project-wide scope, routing, counters, coverage, or frontier. A bounded subsystem task
should otherwise follow the links above and avoid loading unrelated global inventories.
