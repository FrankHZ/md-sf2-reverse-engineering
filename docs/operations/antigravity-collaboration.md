# Antigravity execution and Codex integration

## Responsibilities

Antigravity Desktop supplies independent audits, second opinions and Issue creation/updates
through `gh`. Audit work is read-only. It may also execute a bounded documentation task when the
user selects the task and main-gate has posted its dispatch comment. That assignment grants only
the named paths in the assigned isolated checkout, not general production-code write access.

Codex main-gate owns priority, dependencies, worktree allocation, dispatch comments, independent
acceptance and merge. An audit suggestion or Issue edit alone does not start implementation.
For the current documentation-governance batch, the requested model is **Gemini 4 Neon (High)**.
Record the actual model setting when the host exposes it; do not infer it from the agent's prose.

Use the existing [task lifecycle](github-project-governance.md#task-lifecycle) and
[completion intake](github-project-governance.md#completion-intake). The selected execution
surface changes neither repository ownership nor independent acceptance.

## Dispatch comment

Main-gate updates the Issue to its current bounded outcome, then posts one actionable dispatch:

- Accepted base, assigned reusable checkout and topic branch.
- Exact writable files, explicitly transferred drafts and shared-path exclusions.
- Required reading, dependencies, acceptance checks and remaining uncertainty.
- Draft PR delivery, stopped-writer requirement and the reporting destination.

The user starts the Desktop executor from that comment. Main-gate does not start a competing CLI
worker or treat the prepared assignment as proof that Desktop execution has begun.
Corrections stay with that executor. Extra paths require coordination before editing.

For documentation work, check content, links, scope and the actual staged diff. Do not run game,
engine or full research suites just to validate prose. Commit only the declared public files,
push the topic and freeze a Draft PR; do not self-merge, publish Wiki, clean shared data or start
another Issue without its assignment.

## Completion via Codex CLI

After freezing the delivery, the Desktop executor uses the user's configured Codex CLI route to
report to the designated main-gate chat. The local dispatch supplies that exact chat UUID; do not
use an ambiguous latest-session selector or match by thread title alone when duplicate titles exist.
Verify the installed CLI's supported route instead of inventing flags or treating a zero exit code
alone as proof that main-gate resumed. For example, `codex queue --thread <UUID> --resume` can exit
zero while warning that the thread was queued without a local daemon to resume it.

Send the Issue/PR, exact head, decisive checks and CI, retained failures, unresolved limits and
writer/process state. Report concrete blockers through the same route. If delivery or automatic
resume is unavailable, keep the handoff on the Issue/PR and give the user its link and exact CLI
warning; do not create a polling service. Main-gate resumes on that report, independently reviews,
merges when accepted and performs the authorized scratch cleanup. It does not remain active
waiting for ordinary progress.

## agy CLI status

Automatic `agy` dispatch is not the active route. On the current host, terminal sandbox support is
unavailable and a headless command request was denied under `--print`, despite a zero process exit
and `SUCCESS` envelope. No implementation was delivered. In addition, `agy --conversation <ID>`
uses the CLI store rather than Antigravity Desktop's conversation store, so it cannot directly post
replies back into an existing Desktop chat (importing clones history into CLI). Do not retry
through permission-skipping flags or change global settings as part of a documentation task. Any
future CLI trial needs a deliberate routing decision and checks of actual tool execution, not just
a model-list or exit-code check.
