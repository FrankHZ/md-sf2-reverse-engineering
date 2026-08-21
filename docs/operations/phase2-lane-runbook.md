# Phase 2 Research and Design Lane Runbook

The root/worker sections of this runbook are normative for an ordinary Phase 2 research slice. The
root reads them before delegating and includes this runbook among the worker's required references.
The correction, recovery, progress, and blocker sections also apply when those events occur in an
active research or design-synthesis lane. General repository safety, worktree, evidence,
private-input, and definition-of-done rules remain in `AGENTS.md`; ADR 0004 owns the complete Worker
Acceptance Checklist.

## Root and Worker Setup

The root scopes one coherent slice and its acceptance commands but does not personally reverse
engineer or implement it. It starts exactly one `terra_reverse_engineer` worker with no inherited
controller turns (`fork_turns: "none"`). If the named role is unavailable, the root explicitly starts
`gpt-5.6-terra` with the same bounded handoff. Never run parallel write workers in one slice
worktree.

The self-contained handoff names:

- the exact base and isolated worktree;
- the owning document and bounded source surface;
- owned tracked files and any shared-file needs;
- expected structured outputs and research documentation;
- one narrow H2/H3 acceptance command;
- required references, including
  [ADR 0004](../decisions/0004-single-terra-worker-with-root-acceptance.md) and its complete
  **Worker Acceptance Checklist**; and
- explicit exclusions and active-lane dependencies.

The worker performs the complete static inventory, structured parser or contract, project-owned
tests and research documentation, and grouped H3 question queue required by the slice. It preserves
evidence labels and provenance, avoids project-direction decisions, performs the ADR 0004 adversarial
checklist against its full diff, fixes the weaknesses it finds, reports those corrections, and hands
the work back without staging or committing.

Questions, incomplete evidence, and review findings return to the same worker through a follow-up.
The root does not take over reverse engineering or implementation.

## H3 Closure

A matching observation file alone is not acceptance. Callback exceptions must reach the status and
exit contract, diagnostics must identify the case plus expected and actual callback state, shared-PC
roles need deterministic dispatch, and acceptance requires both a passing command and a Lua Console
with no errors or residual callbacks.

After a bounded instrumentation or contract repair, the lane may repeat a failed emulator launch
without user approval when the run remains non-destructive, uses the already accepted runtime
question, preserves exact launch and failure accounting, and does not weaken a golden merely to pass.

## Root Acceptance and Handoff

The root accepts a completed slice only after it:

1. reviews the worker handoff, changed-file list, complete diff, evidence, and counters;
2. reruns the owning narrow command plus `uv run sf2 verify`;
3. scans for private or generated inputs and unintended changes;
4. stages only accepted paths and reviews the cached diff;
5. commits on the current research topic branch, never directly on `main`; and
6. pushes and opens or updates the Draft PR for serialized main-gate integration.

Use the bounded identity, diff, artifact, and findings-first handoff in
[`bounded-inspection-and-review.md`](./bounded-inspection-and-review.md). Do not paste the controller
history or complete large fixtures into the worker, root, or main-gate handoff.

`uv run sf2 verify --full` remains a milestone, release or merge-readiness, shared-harness, or
explicit full-parity gate. It is never the default worker or root command. A design-synthesis branch
or design-only advance of `main` does not trigger it. Worker instructions and root review are both
required; this division of responsibility is not a security boundary.

Progress checkpoints are commentary, not completion. While safe in-scope work remains, neither a
worker checkpoint nor a root review checkpoint ends the lane or waits for authorization. Continue
through worker handoff, root review, required gates, exact-path staging, commit, push, and Draft PR
handoff.

## Corrections and Owned-Path Expansion

An ordinary research or design failure is not a user-approval boundary. The active lane investigates
failed focused tests, H2/H3 runs, schema or golden comparisons, source/H1/ROM guards, counters,
rebases, generated-artifact scans, and other acceptance gates. It makes the smallest source-backed
correction inside the accepted slice, reruns invalidated gates, and records the correction in the
owning artifacts.

A mechanical owned-path correction remains lane-owned when required for internal consistency. This
includes a fixture registry entry, aggregate counter, translation hash re-anchor, research-index
binding, callback-role closure, or focused test inventory update. The root adds the path to the slice
contract and handoff, checks that no other active lane owns it, and serializes if another lane does.

Ask the user only when a correction would materially change project direction, phase, modern-engine
choice, licensing/distribution, private-input treatment, destructive authority, or another
user-reserved decision.

## Worker Recovery and Operational Blockers

If the sole worker is unresponsive, the lane verifies that no writer process or filesystem mutation
remains active, interrupts or closes that worker, and starts exactly one replacement with the complete
unchanged slice contract in the same topic worktree. The replacement is serial: do not start it until
the prior worker is confirmed stopped. Then continue the normal worker-review-gate flow without user
or main-gate approval. Worker failure is not permission for the root to take over implementation.

Use **stuck** or **blocked on the user** only for an operational inability to continue:

- required filesystem or service permission is unavailable;
- the scheduler cannot create or resume the required lane agent;
- a required tool or dependency is unavailable with no safe in-scope fallback;
- persistent infrastructure failure prevents required commands from running; or
- the responsible lane agent cannot be recovered or replaced.

Report a real blocker promptly with exact evidence and the smallest requested intervention. Evidence
conflicts, unexpected runtime behavior, bounded scope corrections, ordinary failed gates, and an
initially unresponsive worker are not blocker conditions.

Within the accepted Phase 2 direction, continue autonomously. Preserve unrelated or unfinished work,
stage only owned files, and leave final integration to the serialized main gate. Do not pause for
approval or issue a completion report after every ordinary checkpoint.
