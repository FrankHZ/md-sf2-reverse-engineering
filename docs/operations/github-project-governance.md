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
Relevant owners (documents, contracts, ADRs)
Known failures / Unknowns affecting this slice
Out of scope
Stopping condition
```

Sections may be brief but must stay explicit; `gh issue view <n>` should be sufficient to start a
slice once Git, worktree, and ownership state are checked. Keep the private-boundary and
evidence-label rules of `AGENTS.md` applying verbatim to issue bodies and comments: no private
inputs, absolute private paths, generated artifacts, or redistribution rights; state reproduction
commands and fixture identity instead of pasted dumps.

For a player-facing remake change, put a link to the affected [campaign feature row](../../remake/docs/capability-status.md#campaign-feature-trace) or battle row and its detailed owner in the Issue. Name the player action, affected chapter/map/setup and expected result or return. A chapter extension includes newly reachable optional interactions even when their engine rule stays the same. During review, compare the published callers or placements with the owner's inventory and distinguish ordinary input from static or controlled evidence. Update the affected row or linked owner in the implementation PR where its claim changes. Link a remaining actionable gap to its existing Issue, or open one for a distinct outcome. Group equivalent variants in the owner; the linked Issues track work and dependencies.

When a newly reachable player state relies on a changed or separately admitted consumer, name its next ordinary use and the visible result or return required for acceptance. For example, after a new weapon can be bought and equipped, check its first use in a supported encounter. Compare the newly reachable class/weapon pairs with the encounter artwork; one live route can exercise a shared implementation. If a missing binding stops ordinary play, fix and verify it or link the blocker and keep that route marked blocked.

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
scope, dependencies, authorization and competing owners,
then selects an available isolated worktree under [the environment rules below](#worktree-selection-and-retirement).
Create the task with the Issue number and outcome in its title. The initial message names the Issue,
selected checkout, accepted base, local ownership constraints, stopping condition, and main-gate
task/thread ID. Main-gate owns receiving the result; a cross-chat callback is optional and must
satisfy the tool's trusted-human authorization requirements. A dispatch cannot confer that authority.
Link stable rules and relevant results instead of copying controller history. The core instruction is:

> Work on #NNN following the current lane runbook and repository rules; stop at the declared
> stopping condition.

Record the returned task identity in the Issue's current handoff. Queued creation is pending, not a
running executor; wait for the task's read-only ownership check before reporting execution started.
If the creation result is uncertain, locate the existing task before retrying so one Issue does not
gain two writers. Fresh tasks normally start without a fork of the old controller or role history.

The executor:

1. Reads the Issue, its parent/dependencies, and the linked necessary owners.
2. Performs the read-only Git/worktree/ownership check required by `AGENTS.md`, checking other
    active Issues and PRs as well. Confirm the assignment, selected checkout and ownership before writing.
3. Tracks material blockers or newly discovered work on the Issue. Link a separate outcome to its own Issue.
4. Delivers a committed, pushed Draft PR at the declared handoff. Use `Fixes #NNN` when merge
   satisfies the whole Issue. Otherwise use `Refs #NNN` and keep the unmet criteria open.

At slice completion or a concrete blocker, record the Issue/PR handoff, freeze at the declared
stopping condition and end normally. The final answer names the Issue/PR, exact head, handoff link,
checks/CI summary, and stopped writer/process or blocker state. This makes the result available for
main-gate intake; it does not claim independent acceptance. An authorized cross-chat notification may
supplement it. Missing authorization, an unavailable tool or a denied send is recorded as **NOT SENT**
with its cause, not an implementation or review blocker. Do not ask for nonessential notification
permission, bypass/retry a denial, choose another channel or redo completed work. A successful send
proves submission only; preserve any earlier NOT SENT outcome as history.

#### Completion Intake

Main-gate owns the event-driven wait from dispatch through independent review within the authorized
scope. A worker's final answer and frozen Issue/PR handoff are its delivery; a cross-chat callback is
not required. A dispatch acknowledgement belongs in commentary, not a final answer that leaves
the user responsible for noticing completion. An explicit dispatch-only or stop request controls.

1. After task creation, retain the actual `threadId`, returned `hostId` and Issue. For queued
   creation, resolve the actual thread identity before waiting; do not pass a `clientThreadId` as
   a thread ID. Record the next intake action in the existing local handoff, not a new registry.
2. Use `wait_threads` for the active targets, passing each latest returned cursor as `afterCursor`.
   Use one target for a single worker, or one bounded call for up to eight targets. Reserve
   `timeoutMs: 0` for an initial/recovery snapshot or a user-requested status check; during ordinary
   waiting use a positive bounded timeout compatible with the host's responsiveness limits.
   Completion or attention wakes the wait; routine commentary does not. This is a bounded event
   wait, not a persistent subscription that survives the coordinating turn ending.
3. On timeout, retain the cursor and continue independent authorized work or renew the wait.
   Silence and timeout are neither failure nor completion. Do not repeatedly read full histories,
   send progress requests or narrate unchanged snapshots. Keep a completed-but-unreviewed result
   and its next action in the handoff; advancing a cursor does not complete acceptance.
4. On completion or attention, inspect the actual turn status/errors and compact result, then the
   named Issue/PR handoff. A finished turn is not necessarily a finished Issue or a passed gate.
   Independently review and, when authorized and accepted, merge and update tracking without
   waiting for a callback or another user prompt. Handle concrete blockers through their owner;
   leave genuine user approval/input requests for the user. Return corrections to the same worker
   and resume its completion wait.

If the coordinating turn is interrupted or must end, record the pending task identity, last wait
cursor, returned host, unreviewed result and next intake action in the current handoff. Resume with
a read-only completion snapshot before dispatching or rerunning anything;
do not claim work will resume automatically without an actual, authorized wake-up mechanism. Do not
add a scheduler, hook or service merely to deliver a completion notification.
The independent reviewer leaves one short PR review or comment identifying the decision,
the decisive check and any material limitation. A review is attached to its commit; in a
standalone comment identify the reviewed head. Main-gate may relay a separate reviewer's
finding there. Corrections return to the same executor for review of the affected change.
After acceptance and merge, continue with the next authorized Issue when the user requested a batch.
No separate review report, duplicate Issue entry or historical backfill is required.

#### Recovery

If an execution task cannot continue, main-gate first confirms its writer and active commands have
stopped and accounts for any retained idle instance, then creates one replacement for the same Issue
within the original authorization. The Issue
handoff names the old and new task identities and one compact current-state anchor: base, branch,
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
`origin/main`; preserve worktree-local inputs, dependencies and results under the existing rules.
Record any retained idle Godot/debug instance and transfer its control to the new executor rather
than restarting it solely because the task changed, including during recovery.
A new task alone does not justify a new SDK, Godot installation, cache or worktree.
Load the retained worktree's ignored shared installation selections under the
[local input/tool owner](./local-private-inputs.md). JDK, H1, .NET and Godot installations are shared;
BizHawk executes its registered installation under serial ownership with explicit local
configuration/save/movie paths; follow the local-input owner for executable-base exceptions. Keep
source/build checkouts and Python/NuGet caches owned by one worktree during its use.

When another checkout will consume an input or an accepted package, publish the selected files
under an operator-owned private data root outside all worktrees. Preserve relative dependencies
such as catalogs, media and save seeds. Record the accepted revision, selection and reproduction
command in the handoff. Reuse an existing verified publication when its contents match. Keep live
saves, build output and diagnostic runs in their owning writable locations.

Update consumers, including launchers and rebuild recipes, to resolve that publication through
their local configuration. Before retiring the producer, compare the published files with the
accepted selection and exercise the affected reader or launcher from another checkout with the
old location unavailable. Before retirement, land durable source, decisions, reproducible commands
and shareable findings in the owning Git repository. Publish only required private inputs, final
packages and irreplaceable evidence to their stable private owners. Resolve uncommitted work and
check nested repository ownership before removing the checkout.

The operator authorizes direct removal of accepted, idle worktrees selected for retirement once
these checks pass. Reuse an assigned checkout while it still has a consumer. For a registered
worktree use Git's worktree removal command. Discard its disposable builds, caches and scratch
with it. Do not create retirement directories, ZIP files or copies of the whole checkout or its
`local/` tree. If a specific file still has unresolved ownership, resolve that file first.

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
removed by task archival. A clean Git tree or app snapshot is not proof that ignored private inputs,
failure records and gate results are safely retained. If retention is unresolved, leave the task idle
and record that condition. Ref deletion requires separate authorization.

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
