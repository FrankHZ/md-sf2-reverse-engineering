# ADR 0018: Astra Role Routing Trial

- Status: **Accepted**; bounded trial complete, routing defaults retained
- Proposal date: 2026-09-05
- Decision date: 2026-09-05
- Scope: continuing role routing and task handoffs after the bounded trial

## Context

The user is migrating the project's long-lived agents from Sol toward Astra and has designated a
fresh Astra main-gate. The choice is whether to move every role immediately or first try Astra in
roles with broad review and architecture responsibility. Model choice must preserve independent
integration, evidence boundaries, and proportional verification.

A role is a continuing responsibility; a task/session hosts that role; a lane is its owned work;
a slice is one bounded change, normally one PR; a subagent is a temporary bounded worker.

## Current Routing

Retain these defaults after the two-slice trial. The routing decision and task-management smoke
experiment are coordination work, not substantive product slices.

| Role | Default model / reasoning | Responsibility |
| --- | --- | --- |
| main-gate | `gpt-6-astra` / `xhigh` | cross-lane scope, independent review, and serialized integration |
| godot-architect | `gpt-6-astra` / `xhigh` | remake architecture, evidence-to-product boundaries, and implementation ownership |
| investigator | `gpt-6-astra` / `high` or `xhigh` | bounded investigation of suspected workflow or systemic problems |
| research | `gpt-5.6-sol` / `xhigh` | complete evidence slices, static extraction, and justified H3 execution |
| design-doc | `gpt-5.6-sol` / `high` or `xhigh` | contracts and synthesis from accepted evidence |

These are defaults, not an obligation to switch models between planning and implementation. A lane
owner may propose a bounded override for conflicting evidence or cross-owner decisions; the assigned
model and scope belong in the task handoff. A frozen implementation can remain with its current
owner. Do not create an extra executor solely to spend emulator or test wait time on another model.

Terra remains available only for explicitly bounded single-file, single-assembly, or single-function
reverse-engineering work. It does not own a complete research lane or integration. A dedicated lane
owner may execute a slice directly; a Terra subagent is not mandatory. Where older ADR 0004 or
Phase 2 wording prescribes a model or mandatory worker, this routing controls. ADR 0004's evidence,
Worker Acceptance Checklist, handoff, and independent acceptance requirements remain binding for
Phase 2 work. Main-gate-authored changes also require independent review before integration.

## Task Migration and Smoke Experiment

Task creation requires the user's explicit request; this decision is not blanket authority to
replace or archive every role. Keep the old owner paused during a requested migration. Start the
replacement with a compact current-state anchor as required by `AGENTS.md`, and require a read-only
Git/ownership check and acknowledgement of its stopping condition before assigning mutations. A
fresh role must consume accepted `main`, not a pending decision branch or replayed old instructions.

After compaction or a progress-report interruption, reconcile the latest instruction with live Git,
completed command results, and owned process state before resuming. The current-state anchor
supersedes replayed history. Use read-only checks to distinguish completed, pending, and superseded
work; do not execute an operation again to discover whether it already finished. A completed status
report does not pause or replace an unfinished slice. Repeated old instructions and a same-tree merge
commit do not invalidate completed gates. Preserve failures and use the existing dependency-based
verification rules for actual new changes.

The requested startup/rename smoke is complete. A future task-management smoke requires a new explicit
user request. For that smoke, create one read-only Astra task, record the returned task identity and
requested model/reasoning, wait for its bounded acknowledgement, rename
that same identity, and read back the title. Treat queued creation as pending, not successful startup.
Verify model selection from tool or host configuration evidence when available; an agent's claim
about its own model is not proof. Leave the test task idle and report any unavailable verification.
Smoke creation alone does not replace a product owner, resume development, or authorize old-task cleanup.

## Evaluation and Outcome

Use existing PRs, corrections, and task handoffs to judge evidence completeness, scope mistakes,
review severity and rework, unnecessary clarification, and repeated or overly broad verification.
Elapsed time and account usage are optional operational impressions, not per-role cost measurements.
Under ADR 0013, add no telemetry, benchmark harness, copied prompts, usage ledger, or extra test runs.
API token prices do not establish Codex account quota consumption or project cost savings.

The trial is complete after the accepted [Map 19 royal exit](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/305)
and [controlled palace first-visit result](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/306).
These two substantive slices support retaining the current division of responsibility. No third
trial slice is needed. Astra continues as the main-gate and godot-architect default; research and
design-doc retain Sol. The investigator default remains unexercised by this trial, and its existing
task is not migrated by this decision. Terra retains its bounded reverse-engineering role.

The ordinary review record supports this operational choice: both slices stayed within declared
ownership, preserved static-versus-natural evidence boundaries, and reached independent acceptance.
The first needed fixture repairs and a consolidated correction for PowerShell input resolution and
local receipt redaction. The second needed no main-gate code correction. Completed test failures and
smoke-parser failures were retained with targeted corrections; H0 input unavailability was disclosed
instead of reported as a full verification pass. Exact execution details remain with those PRs.

The user also observed old instructions being replayed after compaction and had to request a reminder.
Current-state anchors and explicit continuation instructions allowed work to proceed, but this trial
does not establish that the underlying context-replay problem is fixed. The resumption rule above
remains an operating requirement.

This is a small project-local acceptance sample with no matched Sol comparison, per-role cost
measurement, or investigator/research/design migration exercise. It does not establish general Astra
superiority, quota savings, or a reason to migrate all five tasks. Future routing changes use a
concrete observed need and independent review. If a role regresses, correct the affected slice and
recommend returning that role to Sol without automatically undoing accepted code or evidence.

Verification remains governed by ADR 0012: normal public core and affected gates, retained failure
history, targeted retries, and no full-suite rerun solely for a new model or task. Role migration alone
does not resume game development; a separate user instruction does. This trial changes no product scope, original-game claim,
private-input rule, runtime launch budget, or H4 acceptance requirement.

## References

- [OpenAI Astra guidance](https://developers.openai.com/api/docs/guides/latest-model), accessed
  2026-09-05: instruction sensitivity, clarification, delegation, and proportional testing motivate
  explicit task boundaries; this is guidance, not project benchmark evidence.
- [ADR 0012: Partitioned verification](./0012-dependency-aware-partitioned-verification.md).
- [ADR 0013: Agent workflow without a benchmark program](./0013-token-efficient-agent-research-workflow.md).
- [Phase 2 lane runbook](../operations/phase2-lane-runbook.md).
