# ADR 0018: Astra Role Routing Trial

- Status: **Accepted** for the bounded trial; integration requires independent review
- Proposal date: 2026-09-05
- Decision date: 2026-09-05
- Scope: a bounded trial of model routing and fresh role-task handoffs

## Context

The user is migrating the project's long-lived agents from Sol toward Astra and has designated a
fresh Astra main-gate. The choice is whether to move every role immediately or first try Astra in
roles with broad review and architecture responsibility. Model choice must preserve independent
integration, evidence boundaries, and proportional verification.

A role is a continuing responsibility; a task/session hosts that role; a lane is its owned work;
a slice is one bounded change, normally one PR; a subagent is a temporary bounded worker.

## Trial Decision

After acceptance, use these defaults for the next two accepted substantive slices, with a third
only if the ordinary review record is inconclusive. The routing decision and task-management smoke
experiment do not themselves count as substantive slices.

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

For the separately requested task-management experiment, create one read-only Astra task, record the
returned task identity and requested model/reasoning, wait for its bounded acknowledgement, rename
that same identity, and read back the title. Treat queued creation as pending, not successful startup.
Verify model selection from tool or host configuration evidence when available; an agent's claim
about its own model is not proof. Leave the test task idle and report any unavailable verification.
Its creation does not replace a product owner, resume development, or authorize old-task cleanup.

## Evaluation and Exit

Use existing PRs, corrections, and task handoffs to judge evidence completeness, scope mistakes,
review severity and rework, unnecessary clarification, and repeated or overly broad verification.
Elapsed time and account usage are optional operational impressions, not per-role cost measurements.
Under ADR 0013, add no telemetry, benchmark harness, copied prompts, usage ledger, or extra test runs.
API token prices do not establish Codex account quota consumption or project cost savings.

At the second accepted slice, main-gate gives a concise recommendation to retain, narrow, or expand
the defaults. If a third slice is needed, make that the final trial review before expanding routing.
Record the resulting durable routing decision in this owner through normal independent review.
If a role regresses, correct the affected slice and recommend returning that role to Sol; do not
automatically undo accepted code or evidence. Successful task creation alone does not prove the
model improves project outcomes or that all five roles should move to Astra.

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
