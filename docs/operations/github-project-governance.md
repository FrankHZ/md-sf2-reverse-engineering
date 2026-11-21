# GitHub Project Governance

This guide governs goal tracking, Issue dispatch and independent integration.

## Authority Boundary

Tracked repository documents and exact Git objects remain the durable source of truth for mission,
evidence, accepted behavior, and working rules, per `AGENTS.md` and the
[root README](../../README.md). The GitHub Project and Issues hold **coordination state only**:
what work exists, who owns it, its status, dependencies, and blockers. They never define original
evidence, accepted behavior, or engine semantics, and they are never evidence for a claim about the
original game.

| Information | Owning surface |
| --- | --- |
| What work exists, who owns it, status, dependencies, blockers | GitHub Issues and Project |
| Implementation diff, review, CI, merge result | Pull requests and exact Git objects |
| Original-game facts, contracts, architecture, decisions, reproduction | Tracked documents and executable contracts |
| Accepted behavior and Unknowns | Tracked documents and executable contracts |
| Transient discussion | Issue and PR comments; durable conclusions move to the owning document |

Issues and the Project are not evidence, and evidence documents do not schedule people. A repository
document must not be edited just to sync a task status; a Project status never validates an
original-game claim. This coordination-location rule supersedes older instructions
that require scheduling or session handoffs to live in tracked documents. It does not supersede
Git/worktree ownership, private-input protections, verification scope or independent acceptance.
Accepted capability matrices and coverage denominators remain technical knowledge, not task queues.

## Project

The private [Goals view](https://github.com/users/FrankHZ/projects/1/views/1) shows open
parent Issues with `Work Type = Epic`. It displays the title and status. Add a goal when it
becomes a project commitment and change its status when the goal itself changes:

| Status | When to use it |
| --- | --- |
| `Backlog` | Queued, paused, or waiting for playtest or another dependency. Record a material dependency on the Issue. |
| `In Progress` | The goal is a current focus. |
| `Done` | The goal's acceptance owner has accepted the outcome. Setting Done closes the Issue and removes it from the open Goals view. |

Child Issues carry scope, dependencies and blockers. Their linked PRs carry delivery and review.
Keep automatic sub-issue addition off and let executors work from Issues. No per-slice board
updates or extra fields are required. [All items](https://github.com/users/FrankHZ/projects/1/views/6)
retains the existing cards for lookup. Canceled or superseded goals keep their Issue close reason.

## Work Items

### Epic / Milestone

A milestone or epic parent records the target, linked evidence owners, and the explicit acceptance
boundary. Completing child slices does not by itself complete the milestone acceptance; only the
named acceptance owner closes it. Prefer the existing accepted targets, e.g. the ADR 0009/0010
Map 3–Battle 01 milestone; do not renumber the historical phases.

### Product Constraint Workflow

Before choosing tools or splitting work, name the product, research or operational decision the
result will support. A request to verify fidelity does not by itself authorize a general comparison
platform or complete-trajectory acquisition. Apply this workflow through the existing Issue,
dispatch and independent review; a small change needs only a short paragraph, not a separate plan.

Cost control serves sound software design and total engineering time, including implementation,
review, debugging, rework and maintenance. Do not minimize one model call or local task at the expense
of those outcomes. Estimates are reassessment signals, not acceptance targets or reasons to omit
necessary design. Reuse the current agreement until scope, risk or complexity materially changes;
do not add routine cost ledgers, repeated budget reports or generic benchmarks. Measure performance
for a concrete credible regression, preferably using information from necessary checks or consumers.

1. **Constrain the outcome.** Main-gate records the decision or user-visible behavior, minimum
   sufficient evidence, non-goals and Unknowns that can remain without blocking that decision.
   Link the accepted contract rather than inventing a broader completeness target. Research may
   deliver a bounded finding; it need not manufacture a gameplay change to justify its value.
2. **Choose and budget the method.** Prefer existing evidence, small actual-engine behavior tests
   and focused observations appropriate to the claim. Explain why a simpler method is insufficient
   before adding acquisition or verification infrastructure. Budget implementation, execution,
   storage, review, debugging, integration and maintenance together, including supporting work.
   For costly or uncertain expansion, estimate cumulative effort and name reassessment triggers;
   distinguish revisable estimates from explicit user budgets and physical resource limits.
   Splitting slices or replacing a task does not erase prior work or reset actual limits. Use the existing
   [scale-planning owner](./bounded-inspection-and-review.md#plan-before-scaling) for estimates.
3. **Prove the method before expansion.** Where cost or diagnostic value is uncertain, run the
   smallest representative pilot covering the difficult boundary, a failure and insufficient
   evidence. Check whether its output changes the intended decision, diagnoses failures and can
   be rerun affordably after an implementation change. Main-gate records a continue, narrow,
   replace or stop decision before expanding; a successful happy path alone is insufficient.
4. **Reassess cumulative value.** At slice review, compare the product/research result with total
   effort and new interfaces, state owners, protocols and maintenance obligations. Smaller files,
   more passing checks or more captured data do not establish reduced complexity or useful progress.
   A material estimate change prompts routine engineering reassessment, including extra work that
   prevents rework; it does not automatically stop sound design or require user approval. Stop the
   affected expansion for an explicit budget/resource limit, an unplanned cross-system platform, or
   repeated verification repair without useful progress. Replan rather than justify sunk cost.
5. **Close the bounded outcome.** Report delivered behavior/findings, remaining uncertainty and
   supporting-system cost separately. Main-gate chooses and explains the smallest sufficient next
   step within authorization; it does not delegate routine engineering decisions back to the user.
   A change to the user's requested outcome, accepted fidelity or explicit budget needs the user's
   decision. Retain failures and required evidence: narrower scope cannot turn missing evidence into
   PASS or silently weaken a contract. Deferred work is explicit and is not an automatic future queue.

#### Defensive Code

During design and implementation, place validation at the boundary that owns the input or state.
Before adding a fallback, retry, compatibility branch or repeated check, identify the credible
failure, the guarantee already supplied by callers or types, and the observable failure behavior.
Reuse an existing error path when it covers that failure. Validate again when data can change or
crosses a new trust boundary. Keep external-input validation, supported old data, resource cleanup
and safe storage publication grounded in their accepted contracts.

If the proposed branch only compensates for an internal invariant violation, fix its owner or
surface the failure with useful context. Default values and catch-and-continue require an accepted
degraded behavior. Retries require a recoverable cause, a bound and safe repeated execution.
Review these choices in the existing diff and explanation, with detail proportional to the risk.
When simplifying existing defenses, trace callers and supported inputs before removing a check.

#### Audit and Design Assignments

Dispatch the decision to support, the evidence owner, constraints and the stopping condition.
The executor selects sources, competing explanations, representative legal states and the
smallest observation that distinguishes them. Reading a caller, consumer or adjacent contract
is within scope when it can change the answer. Declared write ownership still applies.

For an audit, report the trigger, consequence, evidence and remaining uncertainty. A bounded
search with no finding can close the question. For design, select and justify a workable
option within accepted product constraints, including the behavior by which it can be judged.
A recommendation for a product change stays a proposal until accepted. The executor can finish
either assignment without waiting for approval of intermediate investigative choices.

A chat review can deliver its answer directly. A durable project finding belongs in its tracked
owner through the usual reviewed change. Issue creation, implementation, runtime acquisition and
publication follow their existing authority and evidence rules. Ask for a product decision when
the available evidence leaves a material tradeoff outside the assignment.

#### Resolve Evidence Conflicts Before Product Choices

When a contract and observed behavior disagree, the executor and main-gate first locate the
assumption's evidence and scope. Trace the relevant initialization, final entry, caller and later
writers/readers of the value. Reuse pinned source, fixtures and retained observations appropriate
to that boundary; a reset helper's output alone cannot establish the value at gameplay entry.

- An implementation omission or incorrect factual contract within the authorized original-behavior
  scope is a correction. Fix its owning consumer and current contract together, record the evidence
  and superseded assumption, and verify actual behavior through the affected normal entry point.
  Preserve historical failures and supported-data compatibility; independent review owns acceptance.
- An explicit, accepted remake deviation retains its design authority. Evidence about the original
  informs any proposed change to that decision; it does not revoke the user's choice.
- Conflicting or insufficient evidence goes to main-gate with the smallest unresolved boundary and
  a focused way to resolve it. Apply the existing runtime-admission and resource rules.

Use the existing Issue and owning documents. A correction above needs no renewed user approval
merely because it changes accepted prose or a test expectation. Ask the user when an unresolved
product tradeoff changes the intended outcome, accepted fidelity, compatibility commitment or
explicit budget. Main-gate checks this classification before integration and before escalating.

### Issue Anatomy

Every executable Issue carries, in its body:

```text
Goal
Product constraint (decision, minimum evidence, non-goals; estimates/limits and replan triggers where relevant)
Accepted base / Context
Owned paths and shared-path needs
Acceptance criteria and commands
Dependencies
Relevant owners (design, English evidence and configuration tables, implementation, ADRs)
Known failures / Unknowns affecting this slice
Out of scope
Stopping condition
```

Sections may be brief but must stay explicit; `gh issue view <n>` should be sufficient to start a
slice once Git, worktree, and ownership state are checked. Keep the private-boundary and
evidence-label rules of `AGENTS.md` applying verbatim to issue bodies and comments: no private
inputs, absolute private paths, generated artifacts, or redistribution rights; state reproduction
commands and fixture identity instead of pasted dumps.

#### Design-Driven Remake Issues

Start with the affected [design owners](../design/documentation-roadmap.md). Use the relevant levels
from L0 vision, L1 overall gameplay, L2 system design and L3 mechanism or behavior specification.
Name the specific behavior or decision needed, then link the English original-game evidence and applicable
configuration tables. Use those links to define the remake implementation boundary and proportional
acceptance. Independent review follows the same chain from design through evidence and data to
implemented behavior. Reuse accepted links for bounded fixes or translations. Add design work only
where a missing or conflicting decision materially affects the slice, and resolve it before
dispatching the dependent implementation.

Before selecting a chapter slice, compare the affected system designs with the chapter's behavior and
configuration inventory, including optional interactions. Use the campaign route to identify callers
and exercise accepted behavior. If a newly discovered caller requires developing a whole mechanism,
reassess the outcome, dependencies and stopping point before taking on that work. Main-gate updates
the affected Issue or links a separately scoped dependency for a distinct outcome.

For example, a church-promotion Issue starts with [the class and promotion design](../design/synthesis/themes/progression-and-economy.md#definitions-current-class-and-promotion).
It checks the [English church evidence](../design/contracts/party/service-interactions.md#church-source-boundary)
and [class and promotion tables](../design/contracts/party/ally-definition-data.md#class-and-promotion-definition-topology).
The [playable HERO boundary](../../remake/docs/application/church-promotion-design.md#playable-hero-and-reproduction)
gives the current implementation and acceptance reference. A slice for that rite can exercise
ordinary selection, class change and Continue while placing further destinations in their own scope.

For a player-facing remake change, put a link to the affected [campaign feature row](../../remake/docs/capability-status.md#campaign-feature-trace) or battle row and its detailed owner in the Issue. Name the player action, affected chapter/map/setup and expected result or return. A chapter extension includes newly reachable optional interactions even when their engine rule stays the same. During review, compare the published callers or placements with the owner's inventory and distinguish ordinary input from static or controlled evidence. Update the affected row or linked owner in the implementation PR where its claim changes. Link a remaining actionable gap to its existing Issue, or open one for a distinct outcome. Group equivalent variants in the owner; the linked Issues track work and dependencies.

For a chapter change that affects a flag writer, reader, lifetime or acceptance, link the relevant [campaign flag row](../../remake/docs/content/campaign-flags.md) in the feature Issue and update that row or its linked evidence owner in the same PR. Review the changed branch and its entry, retry or save boundary through the existing feature review.

When a newly reachable player state relies on a changed or separately admitted consumer, name its next ordinary use and the visible result or return required for acceptance. For example, after a new weapon can be bought and equipped, check its first use in a supported encounter. Compare the newly reachable class/weapon pairs with the encounter artwork; one live route can exercise a shared implementation. If a missing binding stops ordinary play, fix and verify it or link the blocker and keep that route marked blocked.

For already covered prerequisites such as leveling or traversal, use an explicitly labeled, legal controlled state and exercise the changed behavior through its real consumer. Use a targeted natural route when reachability itself changes or is uncertain, for example a recruitment condition, story flag or route entrance. Record which evidence each result uses.

Where similar inputs can preserve or transfer player control, check the cases that distinguish those outcomes. For example, an event tile that returns immediately should preserve a held direction; a scene that takes control should consume it. For a timing claim, observe the named transition (such as step completion to the next step) with a short input sequence. Reuse existing evidence for unchanged behavior. Record a newly requested feel or timing target as a product decision and retain the scope of earlier observations.

### Task Lifecycle

Main-gate plans, dispatches, independently reviews, integrates and tracks work. Within the user's
authorized scope, it may create a fresh execution task for each bounded, executable Issue without
requiring the user to create that task manually. An epic supplies a target; dispatch its scoped child Issues rather than assign an
unbounded epic to a permanent role session. No task may select unrelated Backlog work for itself.

Roles and lanes describe responsibilities, not permanent conversations. One execution task owns one
coherent Issue outcome through implementation and review corrections. A new independent outcome gets
a fresh task; a test rerun, correction, PR update or compaction does not. Main-gate can be longer-lived,
but its planning and acceptance state must be recoverable from Issues, PRs and repository owners.
Main-gate-authored changes still require another independent reviewer.

#### Dispatch and Execution

Before dispatch, main-gate checks the Issue's [product constraint](#product-constraint-workflow),
scope, dependencies, authorization and competing owners. Apply the [model routing](../decisions/0018-astra-role-routing-trial.md#current-routing)
and select an available isolated worktree under [the environment rules below](#worktree-selection-and-retirement).
Create the task with the Issue number and outcome in its title. The initial message names the Issue,
selected checkout, accepted base, local ownership constraints, stopping condition, and main-gate
task/thread ID. Link stable rules and relevant results. The core instruction is:

> Work on #NNN following the current lane runbook and repository rules; stop at the declared
> stopping condition.

For routine continuation, send the current objective, changed ownership or base, remaining failures
and stopping condition. Link the existing handoff for completed work.

Record the returned task identity in the Issue's current handoff. Queued creation is pending, not a
running executor; wait for the task's read-only ownership check before reporting execution started.
If the creation result is uncertain, locate the existing task before retrying so one Issue does not
gain two writers. Fresh tasks normally start without a fork of the old controller or role history.

The executor:

1. Reads the Issue, its parent/dependencies, and the linked necessary owners.
2. Performs the read-only Git/worktree/ownership check required by `AGENTS.md`, checking other
    active Issues and PRs as well. Confirm the assignment, selected checkout and ownership before writing.
3. Resolves engineering details within the assigned scope and runs the selected checks. Reports
   concrete blockers, shared-path conflicts or material environment handoffs as they arise. Record
   blockers on the Issue with the evidence, what was tried and the decision or dependency needed.
4. Delivers a committed, pushed Draft PR at the declared handoff. Use `Fixes #NNN` when merge
   satisfies the whole Issue. Otherwise use `Refs #NNN` and keep the unmet criteria open.

At completion, freeze the Draft PR and record the Issue/PR handoff. The final answer identifies
the Issue and PR, exact head, decisive checks and CI, completed failures, material limits and
stopped writer/process state. The frozen handoff and final answer constitute the completion report.
Workers report at completion or when a concrete blocker or ownership change needs attention.
Use a cross-chat message only with the direct user authorization required by the tool. Delivery and
review can proceed through the final answer and PR if a separate message is unavailable.

#### Completion Intake

Main-gate owns dependency order, shared resource ownership and independent acceptance. In an active
coordinating turn, use available completion or attention events to receive results. A dispatch
acknowledgment is commentary during end-to-end work. An explicit dispatch-only or stop request
controls the stopping point.

1. Retain the Issue and the task creation result's actual thread and host IDs. Resolve queued
   creation before waiting.
2. Use compact bounded `wait_threads` calls for active targets when available. Pass the latest
   cursor for each target and use a positive timeout for ordinary waiting. Between events, advance
   independent work or renew the bounded wait. Limit status checks to a concrete sign of failure or
   a user request. Rely on the delivery or blocker report for review and skip routine progress requests,
   full-history reads and interim review.
3. On completion or attention, check the task result and named Issue/PR handoff. Route blockers to
   their owner and return same-outcome corrections to the same executor. Independently review
   the exact PR head against scope and acceptance. When authorized and accepted, merge, update
   tracking and continue the authorized batch.

If the coordinating turn ends with work pending, record the task and host IDs, last cursor,
unreviewed result if any, and next action in the existing handoff. Resume with a read-only completion
snapshot. The wait runs only during the active turn. Use the existing tools and handoff for intake
without a scheduler, hook or service.

Record the independent decision, decisive check and material limit in a short PR review or comment
that identifies the reviewed head. The PR holds the review record.

#### Recovery

If an execution task cannot continue, main-gate first confirms its writer and active commands have
stopped and accounts for any retained idle instance, then creates one replacement for the same Issue
within the original authorization. The Issue
handoff names the old and new task identities and one compact current-state anchor: selected checkout, excluded paths, owned files, base, branch,
candidate commit, dirt/process/gate state, preserved failures and Unknowns, and stopping condition.
Keep private checkout and input locations in the local handoff. Preserve the candidate and local
results; recovery is not a new slice or a reset of runtime budgets.

Require a read-only state check before mutation. The current assignment and anchor supersede stale
or replayed instructions from old history. Reconcile them with live Git and completed commands;
preserve failing node IDs and process completion rather than rerunning a completed suite to discover
its state. Do not replace the accountable executor with an unannounced in-thread subagent. A change
of main-gate itself needs an explicit coordinator assignment and exclusive integration ownership.

This unit-of-work policy follows the official
[Codex best practices](https://learn.chatgpt.com/guides/best-practices#organize-long-running-chats):
keep the same task for the same problem, without a fixed duration or compaction-count limit.

### Worktree Selection and Retirement

Task lifetime and environment lifetime are separate. Prefer an available existing isolated worktree
for the next Issue after the previous topic is accepted/merged, tracked state is clean, its owned
processes are settled and exclusive ownership is transferred. Start the new topic from accepted
`origin/main` after the previous slice's post-merge cleanup.
Record any retained idle Godot/debug instance and transfer its control to the new executor rather
than restarting it solely because the task changed, including during recovery.
A new task alone does not justify a new SDK, Godot installation, cache or worktree.
Load the retained worktree's ignored shared installation selections under the
[local input/tool owner](./local-private-inputs.md). JDK, H1, .NET and Godot installations are shared;
BizHawk executes its registered installation under serial ownership with explicit local
configuration/save/movie paths; follow the local-input owner for executable-base exceptions. Keep
source/build checkouts and Python/NuGet caches owned by one worktree during its use.

Treat each checkout's top-level ignored `local/` as disposable. During review it can hold scratch,
failed runs and handoffs. Before merge, put durable findings and reproduction commands in their Git
owners and record the acceptance result on the PR. Keep canonical private inputs, player saves and
any package needed by a current consumer under their operator-owned roots outside all worktrees.
For a delivery that uses local material, make the producing scratch unavailable and exercise the
affected entry point with the published inputs. A changed build recipe also needs a clean build.

After every accepted merge, main-gate removes the slice's `local/` before handing the checkout on,
including on a reused worktree. Check the exact checkout path and that writers and consumers have
finished. If the clean check fails, repair the dependency and rerun that check before deleting.
Use [fresh setup](./local-private-inputs.md#fresh-worktree-setup) for the next slice. Discard raw
runs, caches and old handoffs together. Recollect historical observations if needed. This requires
neither a file inventory nor an archive. The cleanup applies only to checkout scratch.

For an accepted idle checkout selected for retirement, resolve uncommitted work and nested Git
ownership, then use Git's worktree removal command. The operator authorizes this removal after
the same checks. Keep reusable checkouts that still have a consumer.

Select the destination before calling the task-creation tool. For reuse through the app, use an
existing isolated worktree registered as its own saved project and explicitly select local execution
there; an app permanent worktree supports this arrangement. Do not select local execution in the
canonical integration checkout. If the tool cannot target the reusable checkout, resolve that project
selection first and report the missing capability; do not silently create another environment.
New isolation still follows ADR 0006's concurrency or concrete reproduction requirement.

After the Issue's own acceptance and merge, its execution task is eligible for archival. Main-gate
may use the app's archive tool when the user has authorized that retirement or a policy covering it;
eligibility alone is not permission to archive every old role task or delete its environment. Check
that its handoff is recorded, no writer remains and required local material has a verified retention
location before archiving. Declined or superseded work retains its close reason and unfinished state.

The app distinguishes [managed and permanent worktrees](https://learn.chatgpt.com/docs/environments/git-worktrees#codex-managed-and-permanent-worktrees).
Archiving a managed task can trigger worktree cleanup; permanent worktrees are not automatically
removed by task archival. If a current consumer still depends on the checkout, leave the task idle
and record the specific active dependency. Ref deletion requires separate authorization.

When retiring old role sessions, first reconcile their active work and retained results with Issues.
Do not revive completed slices, copy entire chat histories, or reset a runtime budget during migration.

### Migration and Backfill

Migrate coordination state into Issues when a lane owner confirms it; do not mass-import history.
Completed PRs may be summarized by linked references, not duplicated as cards. Unknowns stay in
their owning documents; only Unknowns that gate a current slice become Issues. A PR that documents
or re-runs a completed slice is history, not an open task. Accepted main remains the baseline for
new work regardless of import order.

### Maintenance

When an owning document changes a conclusion, update the affected open Issues the same way the
document update is reviewed, rather than leaving a stale queue. When a slice is superseded or
blocked, the Issue (not a side channel) records it with the named blocker. Update the goal on the Project when its priority as a current focus or its acceptance changes.
