# Agent Resume Route

This is the compact entry point for a fresh project task. It routes readers to durable owners without
duplicating their changing counters, findings, branch names, or commit identities.

Apply the stable repository rules in [`AGENTS.md`](../../AGENTS.md) once before using this route. This
file locates task owners; it does not replace the evidence, ownership, safety, scoped verification, or
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

Read the [campaign agreement](../../remake/docs/application/remaining-campaign-risk-baseline.md)
and [capability trace](../../remake/docs/capability-status.md#campaign-feature-trace) for the playable boundary.
[Source coverage](../research/source-coverage.md) owns original-game evidence and open questions.
The [Goals view](https://github.com/users/FrankHZ/projects/1/views/1) and linked Issues hold current work.

## Route by Task Ownership

Read the smallest owning surface that can answer or govern the task:

| Task | Required owner |
| --- | --- |
| Audit or design assignment | [Decision authority and deliverables](./github-project-governance.md#audit-and-design-assignments) and the closest evidence or design owner |
| Outcome scope, verification investment or supporting-tool expansion | [Product Constraint Workflow](./github-project-governance.md#product-constraint-workflow), before tool selection or slice dispatch |
| Ordinary Phase 2 evidence slice | [`phase2-lane-runbook.md`](./phase2-lane-runbook.md), [ADR 0004](../decisions/0004-single-terra-worker-with-root-acceptance.md), its Worker Acceptance Checklist, the closest [`research/`](../research/) owner, and only the bounded sources named in the slice |
| Research coverage, aggregate frontier, or cadence | [`research/source-coverage.md`](../research/source-coverage.md), [ADR 0003](../decisions/0003-static-first-batched-runtime-research.md), and [ADR 0016](../decisions/0016-remake-start-evidence-deferral.md) |
| Evidence-bound subsystem contract | Closest [`design/contracts/`](../design/contracts/) owner and its accepted research dependencies |
| Cross-subsystem or player-facing synthesis | [`design/documentation-roadmap.md`](../design/documentation-roadmap.md) and the closest [`design/synthesis/`](../design/synthesis/) owner |
| Engine implementation or migration | [`remake/README.md`](../../remake/README.md), [architecture](../../remake/docs/architecture.md), [ADR 0019](../decisions/0019-state-and-content-driven-remake-engine.md), and the directly consumed behavior contracts |
| Playable milestone or fidelity claim | [Current accepted boundary](../design/synthesis/milestones/map3-battle01-readiness.md#accepted-current-milestone), [ADR 0009](../decisions/0009-first-phase4-playable-slice.md), [ADR 0010](../decisions/0010-map3-battle01-product-acceptance.md), [ADR 0016](../decisions/0016-remake-start-evidence-deferral.md), and the named capability owner; historical reports retain their results; the old replay path and hardware-exact capture are not prerequisites |
| Private inputs and tool selection | [Local private inputs](./local-private-inputs.md) and the relevant tool owner |
| Verification selection | [Remake scope/current-command distinction](../../remake/docs/development-and-verification.md#scope) for engine/docs; [ADR 0012](../decisions/0012-dependency-aware-partitioned-verification.md) for research; inspect `uv run sf2 verify plan --base origin/main --head HEAD` on a clean committed head |
| Large artifact or diff inspection; PR handoff or review | [`bounded-inspection-and-review.md`](./bounded-inspection-and-review.md) |
| Global documentation routing or decision inventory | [`../README.md`](../../README.md), [`docs/README.md`](../README.md), and the affected index owners |

Use `uv run sf2 research-index list --summary` when current indexed totals matter. Do not copy totals
from an old handoff or maintain them here.

For an assigned Issue, read its scope and active handoff; use
[governance](./github-project-governance.md#dispatch-and-execution) for dispatch and
[recovery](./github-project-governance.md#recovery) for a replacement session.
Read global inventories in full when auditing their project-wide scope, routing or counters.
