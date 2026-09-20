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

- The long-term mission remains full-game reverse engineering of the pinned US release plus an
  independently maintained remake.
- Phase 2 research and evidence-bound contracts remain active.
- Godot 4.7.2 .NET/C# is the accepted Phase 4 baseline. The user explicitly authorized a bounded
  Phase 4 implementation start on 2026-08-28 under ADR 0016. [`remake/`](../../remake/) now contains
  bounded Domain, Application, validated Content, and a thin Godot host for authored packages and the
  connected private-local Map 3 through Battle 01 world. Its [capability owner](../../remake/docs/capability-status.md) records
  what is runnable, diagnostic, admitted-but-unconsumed, Unsupported, or Unknown.
- Research coverage, evidence gaps and cadence belong to
  [`research/source-coverage.md`](../research/source-coverage.md); work scheduling and assignments
  belong to Issues/Project. The continuous Map 3-through-completion-of-Battle 01 milestone and H4
  result remain incomplete eventual acceptance
  work, not an automatic runtime-closure queue or default implementation prerequisite for an
  authorized bounded slice.
- Private ROMs, extracted assets, runtime captures, downloaded tools, and generated binaries remain
  local and untracked.
- [ADR 0019](../decisions/0019-state-and-content-driven-remake-engine.md) records the adopted
  state/content-driven engine migration and the binding user test policy. Add engine behavior unit
  tests; use verification tools directly without tests of those tools. Old tests may migrate or retire.
  M1's common session consumes two authored packages through movement/cancel, HEAL/STAY and automatic
  rounds. M5 retired the transitional reference assembly, host and legacy tests; only external
  controlled inputs remain under `remake/reference/inputs`. After environment setup use `uv run sf2 verify engine`
  and affected `uv run sf2 verify adapter`. Main-gate owns independent acceptance and remote checks.
- Load the ignored [shared tool/input selections](./local-private-inputs.md) before tool commands.
  JDK, H1, .NET and Godot use shared installations; BizHawk receives a clean local runtime copy from
  its verified shared installation. Source/build state and caches remain worktree-local.
- Reuse an available isolated worktree, environments and Godot installation/project/instance across
  sequential tasks after explicit ownership transfer under
  [Project governance](./github-project-governance.md#worktree-selection-and-retirement).
  New isolation needs concurrent ownership or a concrete reproduction/launch failure. Godot acceptance
  uses actual state/input observations; screenshots are prohibited.
- Repository documents and exact Git objects are durable state. Old chats and external memory are not.

## Route by Task Ownership

Read the smallest owning surface that can answer or govern the task:

| Task | Required owner |
| --- | --- |
| Ordinary Phase 2 evidence slice | [`phase2-lane-runbook.md`](./phase2-lane-runbook.md), [ADR 0004](../decisions/0004-single-terra-worker-with-root-acceptance.md), its Worker Acceptance Checklist, the closest [`research/`](../research/) owner, and only the bounded sources named in the slice |
| Research coverage, aggregate frontier, or cadence | [`research/source-coverage.md`](../research/source-coverage.md), [ADR 0003](../decisions/0003-static-first-batched-runtime-research.md), and [ADR 0016](../decisions/0016-remake-start-evidence-deferral.md) |
| Evidence-bound subsystem contract | Closest [`design/contracts/`](../design/contracts/) owner and its accepted research dependencies |
| Cross-subsystem or player-facing synthesis | [`design/documentation-roadmap.md`](../design/documentation-roadmap.md) and the closest [`design/synthesis/`](../design/synthesis/) owner |
| Engine implementation or migration | [`remake/README.md`](../../remake/README.md), [architecture](../../remake/docs/architecture.md), [ADR 0019](../decisions/0019-state-and-content-driven-remake-engine.md), and the directly consumed behavior contracts |
| Eventual playable milestone or fidelity claim | [ADR 0009](../decisions/0009-first-phase4-playable-slice.md), [ADR 0010](../decisions/0010-map3-battle01-product-acceptance.md), [ADR 0016](../decisions/0016-remake-start-evidence-deferral.md), and the named readiness/capability owner; 8D/H4 remains incomplete; the old replay path and hardware-exact capture are not prerequisites |
| Verification selection | [Remake scope/current-command distinction](../../remake/docs/development-and-verification.md#scope) for engine/docs; [ADR 0012](../decisions/0012-dependency-aware-partitioned-verification.md) for research; inspect `uv run sf2 verify plan --base origin/main --head HEAD` on a clean committed head |
| Large artifact or diff inspection; PR handoff or review | [`bounded-inspection-and-review.md`](./bounded-inspection-and-review.md) |
| Global documentation routing or decision inventory | [`../README.md`](../../README.md), [`docs/README.md`](../README.md), and the affected index owners |

Use `uv run sf2 research-index list --summary` when current indexed totals matter. Do not copy totals
from an old handoff or maintain them here.

For documentation-only work, perform direct document and scope checks. M0's scoped CI/local selection
does not impose the old engine suites; research changes retain their owning dependencies. Preserve
completed results, failed nodes and process state across compaction.

## Bounded Worker Handoff

For an ordinary independent Phase 2 slice, follow the normative
[`phase2-lane-runbook.md`](./phase2-lane-runbook.md). It owns the exact role, bounded no-history
handoff, same-worker correction, recovery, acceptance, and escalation rules. This resume route does
not duplicate them.

## Work Items and Task State

Open work, ownership, and next steps are tracked as GitHub Issues in the
[SF2 Modernization project](https://github.com/users/FrankHZ/projects/1) under
[GitHub Project Governance](./github-project-governance.md#task-lifecycle). Main-gate creates a fresh
task for each dispatched coherent Issue outcome; its review corrections stay in that task. For a
dispatched task, read the Issue and current handoff first, then this route and the owners it names.
`gh issue view <n>` locates the work; it does not replace the read-only Git/worktree/ownership check.
Repository documents remain the durable source
of truth for evidence and accepted behavior; the Project holds coordination state only.

## When to Read the Global Documents

Read the complete root README, documentation index, or source-coverage ledger when the task owns or
audits their project-wide scope, routing, counters, coverage, or frontier. A bounded subsystem task
should otherwise follow the links above and avoid loading unrelated global inventories.
