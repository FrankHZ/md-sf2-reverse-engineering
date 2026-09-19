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

### Agent Workflow

Dispatch says *what* to work on; it does not replace repository rules. The instruction form is:

> Work on #NNN following the current lane runbook and repository rules; stop at the declared
> stopping condition.

An executor:

1. Reads the Issue, its parent/dependencies, and the linked necessary owners.
2. Performs the read-only Git/worktree/ownership check required by `AGENTS.md`, checking other
   active Issues and PRs as well. Ready is eligibility, not an automatic claim or role replacement;
   obtain assignment and resolve competing ownership before writing.
3. Sets `In Progress` (with executor identity) only after step 2 confirms it may proceed.
4. Discovers adjacent work → new linked Issue; never silently expands a slice.
5. Delivers a committed, pushed Draft PR; moves the item to `Review / Verify` only at its declared
   handoff, not merely when a draft opens. Use `Fixes #NNN` only when merge will satisfy the entire
   Issue; otherwise use `Refs #NNN` and retain the unmet acceptance work.
6. After independent review, acceptance, and merge, the item may move to `Done`. Closing the Issue
   or the PR never substitutes for the task's own acceptance boundary.

Session replacement hands off in the Issue with a compact current-state anchor: base, worktree,
branch, commit, dirt/process/gate state, preserved failures and Unknowns, and the next stopping
condition. Do not paste whole chat logs; superseded in-thread instructions do not override the
Issue's declared stopping condition. Reconcile the anchor with live Git and the latest authorized
instruction before mutation. Preserve completed failing node IDs/results and process completion;
do not relabel a completed failing run as interrupted. Private workspace details stay local.

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
