# ADR 0018: Astra Role Routing After Trial

- Status: **Accepted**; trial closed, Astra research default adopted
- Proposal date: 2026-09-05
- Decision date: 2026-09-06
- Scope: continuing role routing and task handoffs after the bounded trial

## Context

The project has completed a bounded Astra migration trial across main-gate, remake implementation,
research, and a tooling investigation. The user authorized formal closeout and ordinary Astra work
for main-gate, godot-architect, and research. Model choice continues to preserve independent
integration, evidence boundaries, and proportional verification.

A role is a continuing responsibility; a task/session hosts that role; a lane is its owned work;
a slice is one bounded change, normally one PR; a subagent is a temporary bounded worker.

## Current Routing

Use these defaults for ordinary authorized work. No further slice or task-management smoke is
required solely to evaluate the model; the completed startup/rename experiment stays closed.

| Role | Default model / reasoning | Responsibility |
| --- | --- | --- |
| main-gate | `gpt-6-astra` / `xhigh` | cross-lane scope, independent review, and serialized integration |
| godot-architect | `gpt-6-astra` / `xhigh` | remake architecture, evidence-to-product boundaries, and implementation ownership |
| investigator | `gpt-6-astra` / `high` or `xhigh` | bounded investigation of suspected workflow or systemic problems |
| research | `gpt-6-astra` / `xhigh` | complete evidence slices, static extraction, and justified H3 execution |
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

The accepted work supports Astra as the continuing main-gate, godot-architect, and research default:

| Evaluated responsibility | Accepted evidence and limit |
| --- | --- |
| Main-gate and remake implementation | [Map 19 royal exit](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/305), [controlled palace first visit](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/306), and [royal return](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/308) reached independent acceptance within their declared ownership and evidence boundaries. Corrections and unavailable checks remain in the owning PR records. |
| Research | [Original warp-record facing](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/309) and [warp-facing source guard](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/311) support adopting Astra for complete research slices. Source, fixture and regression checks passed independent acceptance without promoting source structure to natural runtime behavior. |
| Bounded investigation | [BizHawk debug bridge](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/310) exercised a separately requested Astra task and retained unsuccessful experiments and runtime limitations. It supports the existing investigator default but did not migrate the old investigator task. |

Design-doc remains on Sol because this trial did not exercise that role. Terra retains its bounded
reverse-engineering role. Existing accepted work and frozen tasks remain valid; the routing decision
does not require recreating tasks, repeating completed slices, or adding a benchmark program.

The user also observed old instructions being replayed after compaction and had to request a reminder.
Current-state anchors and explicit continuation instructions allowed work to proceed, but this trial
does not establish that the underlying context-replay problem is fixed. The resumption rule above
remains an operating requirement.

The bridge investigation also exposed a main-gate scope error: its follow-up expanded into a large
legacy regression queue that did not use the bridge. The user had to redirect the gate. Main-gate
must reconcile broad planner selections with the actual change before launching an expensive queue,
keep direct experiment evidence distinct from surrounding regression results, and record any
explicitly authorized acceptance exception without changing the planner or relabeling unexecuted
checks. The [bridge owner](../operations/bizhawk-debug-bridge.md#bounded-merge-acceptance-for-pr-310)
retains that PR's failed full-suite discovery, separate corrections, and 47 paused H3 commands.
Those commands remain **NOT RUN**; trial closeout does not resume them or generalize that exception.

This is a small project-local acceptance sample with no matched Sol comparison, per-role cost
measurement, or design-doc trial. It does not establish general Astra superiority or quota savings.
Future routing changes use a concrete observed need and independent review. If a role regresses,
correct the affected slice and
recommend returning that role to Sol without automatically undoing accepted code or evidence.

Verification remains governed by ADR 0012: normal public core and affected gates, retained failure
history, targeted retries, and no full-suite rerun solely for a new model or task. Role migration alone
does not resume game development; a separate user instruction does. This trial changes no product scope, original-game claim,
private-input rule, runtime launch budget, or H4 acceptance requirement. Formal closeout adopts the
routing above; it does not start a new product slice or authorize worktree/ref cleanup.

## References

- [OpenAI Astra guidance](https://developers.openai.com/api/docs/guides/latest-model), accessed
  2026-09-05: instruction sensitivity, clarification, delegation, and proportional testing motivate
  explicit task boundaries; this is guidance, not project benchmark evidence.
- [ADR 0012: Partitioned verification](./0012-dependency-aware-partitioned-verification.md).
- [ADR 0013: Agent workflow without a benchmark program](./0013-token-efficient-agent-research-workflow.md).
- [Phase 2 lane runbook](../operations/phase2-lane-runbook.md).
