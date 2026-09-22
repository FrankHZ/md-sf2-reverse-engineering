# ADR 0018: Task Model Routing After the Astra Trial

- Status: **Accepted**; trial closed, task-based routing adopted
- Proposal date: 2026-09-05
- Decision date: 2026-09-06
- Routing update: 2026-09-22
- Scope: task model routing and handoffs after the bounded trial

## Context

The project has completed a bounded Astra migration trial across main-gate, remake implementation,
research, and a tooling investigation. The user authorized formal closeout and ordinary Astra work
for main-gate, godot-architect, and research. Model choice continues to preserve independent
integration, evidence boundaries, and proportional verification.

A role is a continuing responsibility; a lane identifies an area of work. An execution task hosts one
coherent Issue outcome, including its corrections, rather than all future work for a role. A slice is
one bounded change, normally one PR; a subagent is a temporary bounded worker. Main-gate retains
cross-Issue planning and independent integration; Issues and repository owners hold the durable state.

## Current Routing

Choose the model for the task's reasoning difficulty within its assigned role and Issue ownership:

| Work | Default model / reasoning |
| --- | --- |
| Main-gate planning, independent review and serialized integration; independently assigned investigation of systemic problems | `gpt-6-astra` / `high` |
| Difficult architecture, conflicting evidence, novel reverse engineering or runtime-admission design | `gpt-6-astra` / `high` |
| Ordinary scoped implementation or tooling, accepted-evidence extraction and design synthesis | `gpt-6-sol` / `medium` |
| Focused repeatable work with settled semantics, explicit ownership and direct acceptance, such as mechanical documentation or translation synchronization and routine bounded implementation | `gpt-6-luna` / `high` |

The godot-architect, research and design-doc roles retain their responsibilities; a lane name alone
does not select a model. Main-gate and independently assigned investigator retain Astra/High. Keep
unclear evidence and integration authority with the accountable owner rather than assigning Luna
solely for cost. A user-specific choice overrides these defaults.

Model and reasoning effort are separate. Use High above Medium only for a concrete reasoning
difficulty; reserve XHigh for an exceptional named difficulty where High is insufficient or comparable
work shows a concrete benefit. Record an override reason in the Issue handoff. Distinguish reasoning
difficulty or scope limitations from missing input, tool or environment failures and ordinary failed
checks. Escalate Luna to Sol or Sol to Astra when a named reasoning or scope limitation warrants it;
escalation changes neither scope nor acceptance and does not reset runtime budgets.

Preserve running or frozen executors and completed results. No model switch, task recreation, extra
verification, benchmark or investigator dispatch follows from this routing update. Do not create an
extra executor solely to spend emulator or test wait time on another model. Official model guidance
informs these defaults; it does not establish project performance, account quota savings or a model
ranking for this repository.

At dispatch, main-gate requests the selected model and supported reasoning level through the
task-creation tool instead of inheriting an application default. Record the request and any override
reason in the current Issue handoff. Respect the callable tool's current choices and constraints.
For an existing task, a supported setting change can apply on its next continuation; do not restart
the task or repeat completed gates solely to change settings. A tool request establishes the requested
selection; confirm it through tool or host evidence when available. If selection cannot be applied or
verified, report that limitation instead of claiming the task is running at the chosen settings. An
agent's self-description is not verification.

Terra remains available only for explicitly bounded single-file, single-assembly, or single-function
reverse-engineering work. It does not own a complete research lane or integration. A dedicated lane
owner may execute a slice directly; a Terra subagent is not mandatory. Where older ADR 0004 or
Phase 2 wording prescribes a model or mandatory worker, this routing controls. ADR 0004's evidence,
Worker Acceptance Checklist, handoff, and independent acceptance requirements remain binding for
Phase 2 work. Main-gate-authored changes also require independent review before integration.

## Task Lifecycle and Recovery

The user-authorized Issue-based workflow replaces the former manual creation of replacement role
sessions. Main-gate may create an execution task for an authorized, scoped Issue under
[GitHub Project Governance](../operations/github-project-governance.md#task-lifecycle), which owns
dispatch, recovery, worktree selection and retirement. This does not authorize arbitrary new work,
bulk archival or cleanup. Model defaults above are independent of task lifetime.

Keep same-outcome implementation and review corrections in the same task. Recover a failed task with
the compact current-state anchor required by `AGENTS.md`, after its old writer has stopped. Require a
read-only Git/ownership check before mutation. New work starts from accepted `main`; recovery preserves
the existing candidate and results instead of discarding them or replaying completed instructions.

After compaction or a progress-report interruption, reconcile the latest instruction with live Git,
completed command results, and owned process state before resuming. The current-state anchor
supersedes replayed history. Use read-only checks to distinguish completed, pending, and superseded
work; do not execute an operation again to discover whether it already finished. A completed status
report does not pause or replace an unfinished slice. Repeated old instructions and a same-tree merge
commit do not invalidate completed gates. Preserve failures and use the existing dependency-based
verification rules for actual new changes.

### Closed Smoke Experiment

The requested startup/rename smoke is complete. Ordinary Issue dispatch does not require another
startup/rename experiment. A future task-management smoke requires a new explicit
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

At the 2026-09-06 trial closeout, the accepted work supported Astra as the main-gate,
godot-architect, and research default. This is the trial's historical outcome, superseded for new
tasks by Current Routing above:

| Evaluated responsibility | Accepted evidence and limit |
| --- | --- |
| Main-gate and remake implementation | [Map 19 royal exit](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/305), [controlled palace first visit](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/306), and [royal return](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/308) reached independent acceptance within their declared ownership and evidence boundaries. Corrections and unavailable checks remain in the owning PR records. |
| Research | [Original warp-record facing](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/309) and [warp-facing source guard](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/311) support adopting Astra for complete research slices. Source, fixture and regression checks passed independent acceptance without promoting source structure to natural runtime behavior. |
| Bounded investigation | [BizHawk debug bridge](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/310) exercised a separately requested Astra task and retained unsuccessful experiments and runtime limitations. It supports the existing investigator default but did not migrate the old investigator task. |

Design-doc remained on Sol at trial closeout because this trial did not exercise that role. Terra
retained its bounded reverse-engineering role. Existing accepted work and frozen tasks remained
valid; the trial decision did not require recreating tasks, repeating completed slices, or adding a
benchmark program.

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
correct the affected slice and reconsider its model without automatically undoing accepted code or
evidence.

Verification remains governed by ADR 0012: normal public core and affected gates, retained failure
history, targeted retries, and no full-suite rerun solely for a new model or task. Role migration alone
does not resume game development; a separate user instruction does. This trial changes no product scope, original-game claim,
private-input rule, runtime launch budget, or H4 acceptance requirement. The trial closeout did not
start a new product slice or authorize worktree/ref cleanup.

## References

- [Codex Sol and Luna selection guidance](https://learn.chatgpt.com/docs/whats-new#choose-gpt-6-sol-and-luna),
  accessed 2026-09-22: recommends Sol Medium for everyday coding and Luna High for focused,
  repeatable tasks, subject to plan, client and workspace availability.
- [GPT-6 Sol](https://developers.openai.com/api/docs/models/gpt-6-sol) and
  [GPT-6 Luna](https://developers.openai.com/api/docs/models/gpt-6-luna), accessed 2026-09-22:
  describe their intended workloads; API prices do not measure this project's Codex account usage.
- [OpenAI Astra guidance](https://developers.openai.com/api/docs/guides/latest-model), accessed
  2026-09-05: instruction sensitivity, clarification, delegation, and proportional testing motivate
  explicit task boundaries; this is guidance, not project benchmark evidence.
- [OpenAI reasoning-effort guidance](https://developers.openai.com/api/docs/guides/reasoning#reasoning-effort),
  accessed 2026-09-19: Medium balances ordinary workloads, High targets difficult reasoning, and
  XHigh needs a benefit that justifies its additional cost. This informs task selection, not a
  measured Codex quota multiplier or a new project benchmark requirement.
- [ADR 0012: Partitioned verification](./0012-dependency-aware-partitioned-verification.md).
- [ADR 0013: Agent workflow without a benchmark program](./0013-token-efficient-agent-research-workflow.md).
- [Phase 2 lane runbook](../operations/phase2-lane-runbook.md).
