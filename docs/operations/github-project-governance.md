# GitHub Project Governance

- Status: **Accepted governance; hosted configuration applied and read back.**
  This document describes the current Project configuration and its use.
- Scope: division of authority between repository knowledge and GitHub work coordination;
  conventions for Issues, Project fields, and milestones.
- Applies to: agents and humans planning, dispatching, executing, or integrating work in this
  repository.

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

Use [SF2 Modernization](https://github.com/users/FrankHZ/projects/1) as the single project.
Keep it private. Do not create per-lane projects; use multiple views of one item pool. Link this
owner from the Project rather than duplicating its rules.

The statuses, fields, views and built-in workflows below are configured on that Project. Its
README links here. Track work ownership, migration handoffs and outstanding acceptance in
Issues, not a running checklist in this document. A queue entry is not permission to execute.

### Status Model

| Status | Entry condition |
| --- | --- |
| `Backlog` | Candidate work whose scope, dependencies, or authorization is not yet settled |
| `Ready` | Goal, owned paths, acceptance commands/observations, and dependencies are declared |
| `In Progress` | An executor has confirmed Git/worktree/path/process ownership and started |
| `Review / Verify` | Candidate committed and pushed; awaiting independent review or its selected verification |
| `Blocked` | A concrete blocker with the resolving owner and condition named in the Issue |
| `Done` | The item's own acceptance is met; for code or docs, independently accepted and merged |

Rules:

- **Unknown is not Blocked.** An open question that does not currently gate the slice stays with its
  knowledge owner. `Blocked` requires a named resolving owner and condition.
- **Closing is not delivering.** Canceled, duplicate, or declined items keep their close reason and
  do not count as delivered work. Only accepted/merged implementation counts as Done.
- A merged PR closes its implementation slice; it does not by itself close an epic, milestone
  acceptance, or any original-game acceptance claim.
- Lane worktrees are worktrees, not board state. Branch or worktree existence alone never sets
  `In Progress`; a named executor plus declared ownership does.

### Fields and Labels

Fields (single project-wide set):

- `Area` single-select: `Research`, `Battle Engine`, `Map & World`, `Presentation`, `Content`,
  `Tooling`, `Design`, `Governance`.
- `Work Type` single-select: `Epic`, `Feature`, `Research`, `Task`, `Bug`. GitHub reserves the name
  `Type` and rejects it for this personal Project's custom field; use `Work Type` consistently.
- `Priority` single-select: `P0`–`P3`. `P0` is reserved for a reproduced, actively harming defect
  that must be addressed immediately; it is not a popularity rank.

- `Assignees`: existing GitHub field for the accountable account; agent sessions are not accounts.
- `Lane` single-select: `Research`, `Design`, `Remake`, `Tooling`, `Governance`. These are stable
  responsibilities; record the actual executor/session in the current Issue handoff.
- `Milestone`: existing field for an accepted target, not a new phase numbering scheme.

Optional `graphics` and `audio` labels can distinguish presentation work. Do not duplicate Area,
Work Type or Status into separately maintained labels.

Explicit non-goals:

- No single-item `Confidence` field. Issue bodies state evidence per claim with the repository's
  **Confirmed / Inferred / Unknown** labels and links to owning evidence; a card cannot be stamped
  "Verified" wholesale.
- No `Effort` field until a real scheduling need appears; if added later, size only, never an
  evidence claim.
- Graphics and audio work starts under optional labels; split `Area` values only when a durable
  dedicated queue exists.

### Views

The configured views use table layout and the same item pool:

| View | Filter | Grouping | Sorting |
| --- | --- | --- | --- |
| [All items](https://github.com/users/FrankHZ/projects/1/views/1) | None; includes retained completed items | None | Manual order |
| [Active](https://github.com/users/FrankHZ/projects/1/views/2) | `-status:Backlog,Done` | Status | Priority, then Lane, ascending |
| [Agent Queue](https://github.com/users/FrankHZ/projects/1/views/3) | `status:Ready` | None | Priority, then Lane, ascending |
| [Backlog](https://github.com/users/FrankHZ/projects/1/views/4) | `status:Backlog` | None | Priority, then Lane, ascending |
| [Milestones](https://github.com/users/FrankHZ/projects/1/views/5) | `has:milestone` | Milestone | Priority ascending |

Visible columns are Title, Status, Area, Work Type, Priority, Lane, Assignees, Milestone and Linked
pull requests. An empty Agent Queue means no item is Ready; an empty Milestones view means no item
has a milestone. Neither is an execution failure. No Roadmap is configured; add one only when items
carry real dates. Views are presentation only; field values and statuses remain the coordination
authority.

### Built-in Workflows

| Workflow | Configuration | Boundary |
| --- | --- | --- |
| Item added to project | Enabled: new Issue/PR items enter Backlog | Adding an item does not assign it |
| Auto-add sub-issues to project | Enabled | Explicit sub-issues of existing items join the same queue; this does not create or dispatch work |
| Pull request merged | Enabled: the PR item enters Done | Its merge does not complete a parent epic or milestone |
| Auto-close issue | Enabled: setting an Issue item to Done closes it | Only the acceptance owner sets Done after that Issue's own criteria are met |
| Item closed | Disabled | Closing canceled, duplicate or unmerged work must not mark it delivered |
| Pull request linked to issue | Disabled | A PR link must not assign In Progress before executor ownership is confirmed |

Issue closure and Project status are distinct operations: after independent acceptance, the owner
sets Done; the configured workflow can then close the Issue. If a closing keyword closes an Issue
on merge, the acceptance owner still sets its Project status. Canceled or superseded Issues retain
their close reason and are not promoted to Done. Do not enable automatic review or dispatch status
changes that bypass the entry conditions above.

## Work Items

### Epic / Milestone

A milestone or epic parent records the target, linked evidence owners, and the explicit acceptance
boundary. Completing child slices does not by itself complete the milestone acceptance; only the
named acceptance owner closes it. Prefer the existing accepted targets, e.g. the ADR 0009/0010
Map 3–Battle 01 milestone; do not renumber the historical phases.

### Issue Anatomy

Every executable Issue carries, in its body:

```text
Goal
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

### Task Lifecycle

Main-gate plans, dispatches, independently reviews, integrates and tracks work. Within the user's
authorized scope, it may create a fresh execution task for each bounded, executable Issue without
requiring the user to create that task manually. Ready is eligibility, not authorization or an
automatic claim. An epic supplies a target; dispatch its scoped child Issues rather than assign an
unbounded epic to a permanent role session. No task may select unrelated Backlog work for itself.

Roles and lanes describe responsibilities, not permanent conversations. One execution task owns one
coherent Issue outcome through implementation and review corrections. A new independent outcome gets
a fresh task; a test rerun, correction, PR update or compaction does not. Main-gate can be longer-lived,
but its planning and acceptance state must be recoverable from Issues, PRs and repository owners.
Main-gate-authored changes still require another independent reviewer.

#### Dispatch and Execution

Before dispatch, main-gate checks the Issue's scope, dependencies, authorization and competing owners,
then selects an available isolated worktree under [the environment rules below](#worktree-selection-and-retirement).
Create the task with the Issue number and outcome in its title. The initial message names the Issue,
selected checkout, accepted base, local ownership constraints, stopping condition, and exact main-gate
recipient task/thread ID; explicitly require completion or blocker notification via the available
`send_message_to_thread` tool to that ID before the executor finalizes its task. Link stable
rules and relevant results instead of copying controller history. The core instruction is:

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
3. Sets `In Progress` (with executor identity) only after step 2 confirms it may proceed.
4. Discovers adjacent work → new linked Issue; never silently expands a slice.
5. Delivers a committed, pushed Draft PR; moves the item to `Review / Verify` only at its declared
   handoff, not merely when a draft opens. Use `Fixes #NNN` only when merge will satisfy the entire
   Issue; otherwise use `Refs #NNN` and retain the unmet acceptance work.
6. After independent review, acceptance, and merge, the item may move to `Done`. Closing the Issue
   or the PR never substitutes for the task's own acceptance boundary.

At slice completion or a concrete blocker, record the Issue/PR handoff and freeze at the declared
stopping condition, then send one concise notification to the dispatched main-gate ID before the local
final answer: Issue/PR, exact head, handoff link, checks/CI summary, and stopped writer/process or
blocker/approval state. Link detailed evidence. Confirm that `send_message_to_thread` returns success;
this proves submission, not independent acceptance or that anyone read it. A local final, Issue comment,
PR creation or Project status does not substitute for this send. If the recipient is missing or the
tool is unavailable/fails, retain the handoff and report **NOT SENT** with the concrete cause in the
local final. Treat this as a reporting blocker: do not claim delivery, choose unrelated recipients,
blindly duplicate sends or redo completed work/gates. Send a new completed correction handoff once;
do not add routine status polling.
Review corrections return to the same execution task and scoped Issue, with In Progress restored when
the executor resumes. An optional bounded subagent reports to that executor; it is not another
top-level Issue owner. Completion stops execution rather than starting the lane's next Issue.

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
source/build checkouts and Python/NuGet caches owned by one worktree, and preserve old copies and
completed results until separately authorized cleanup.

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
and record that condition. Worktree/ref deletion remains separately authorized under `AGENTS.md`.

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
blocked, the Issue (not a side channel) records it with the named blocker. Periodically review the
Project for items whose entry conditions no longer hold; demote or close them with reasons instead
of letting the board drift.
