# Bounded Artifact, Diff, Handoff, and Review Runbook

Use this runbook when a task inspects a large tracked artifact or topic diff, prepares a root or
main-gate handoff, or independently reviews a candidate. It uses existing Git, GitHub, and `sf2`
outputs; it does not introduce telemetry, a benchmark, a new acceptance gate, or a substitute for
owning evidence.

## Inspect Identity and Shape First

For a clean committed candidate, reproduce its identity and changed shape before opening content:

```powershell
$reviewBase = 'origin/main'
$reviewHead = 'HEAD'

git status --short --branch
git rev-parse $reviewBase
git rev-parse $reviewHead
git rev-parse "$reviewHead^{tree}"
git merge-base $reviewBase $reviewHead
git diff --name-status "$reviewBase...$reviewHead"
git diff --stat "$reviewBase...$reviewHead"
git diff --numstat "$reviewBase...$reviewHead"
git diff --check "$reviewBase...$reviewHead"
uv run sf2 verify plan --base $reviewBase --head $reviewHead
```

The committed-range planner reports current executable selection, not semantic correctness or
permission to override [current verification scope](../../remake/docs/development-and-verification.md#scope).
Research fanout requires resolving the affected evidence boundary. New-engine legacy-test retirement
requires the coordinated code/CI/planner cutover; a documentation slice records that pending work
without implementing it or rerunning every old selection.

After classifying the paths, inspect only the owning file or bounded hunk needed for the current
judgment:

```powershell
git diff --unified=20 "$reviewBase...$reviewHead" -- path/to/owner
```

Increase context or open a neighboring owner only when a concrete reference, invariant, or finding
requires it. Do not paste a full controller history, full repository diff, or unrelated source tree
into another agent's prompt.

## Reduce Large Artifacts before Reading Payloads

For a large schema, fixture, manifest, trace, or generated report, start with tracked identity and
shape:

```powershell
$artifactPath = 'path/to/artifact'
Get-Item -LiteralPath $artifactPath | Select-Object FullName, Length
Get-FileHash -LiteralPath $artifactPath -Algorithm SHA256
git ls-files --stage -- $artifactPath
git status --short --ignored -- $artifactPath
```

An index row proves the path is tracked. A `??` status is non-ignored untracked content and a `!!`
status is ignored content; both require the owning private/generated policy before use or handoff. No
output from `git ls-files` alone is not a tracked/private/generated boundary proof. `FullName` is a
local diagnostic only: a public handoff uses the repository-relative owner path or allowed identity
and provenance, never a private artifact's absolute machine path.

Then run the artifact's owning extractor, schema validator, H2/H3 command, or contract test and retain
its bounded summary: identity, counts, owner paths, hashes where public, validation failures, and the
minimum differing records or source excerpts required for a decision. Use
`uv run sf2 research-index list --summary` for current index totals and the owning narrow command for
fixture or runtime semantics.

Pass file paths directly to deterministic tools. Do not stream a multi-megabyte JSON document through
the console or open its complete payload in model context merely to count records. If the owning tool
reports a failure, inspect the exact failing pointer, record, source range, or callback case. If the
summary is inconclusive, expand deliberately until the claim can be judged; bounded inspection is not
permission to ignore malformed detail.

Private ROM-derived payloads, traces, captures, and extracted assets remain local. A public handoff
may include allowed identity, provenance, aggregate counts, and gate status, never the private payload
or a transcript that embeds it.

## Handoff Contract

A worker-to-root, root-to-main-gate, or correction handoff is concise and self-contained. Record the
execution task's current handoff on its Issue and link the PR; a bounded subagent reports to that
executor, which consolidates the result. Include:

1. repository, worktree, branch, exact base, head, merge base, and candidate tree;
2. PR URL and number when a PR exists, plus its Draft/open, mergeable, and check state;
3. clean worktree status and explicit proof that the local candidate head equals the pushed remote
   topic head;
4. exact changed paths, classified by owner and purpose;
5. dependencies, active-lane/shared-file relationships, and required rebase order;
6. the bounded evidence or decision summary with Confirmed, Inferred, and Unknown labels where
   research claims are involved;
7. affected planner partitions and every reproduced command with PASS, FAIL, SKIPPED, or unavailable
   status plus the exact reason;
8. private/generated/tracked-boundary result;
9. findings first, ordered P0, P1, then P2, with an exact path and line, Git object, fixture, address,
   or command result;
10. residual risks and meaningful test gaps;
11. ACCEPT, ACCEPT-WITH-FOLLOW-UP, CORRECTION-REQUIRED, or REJECT, plus the smallest correction owner
   and path scope when applicable; and
12. the Issue, execution task identity, next stopping condition, owned process state, and any local
   environment/result retention needs. Keep absolute private paths in the local handoff, not public
   Issue/PR text. Task retirement follows [Project governance](./github-project-governance.md#worktree-selection-and-retirement).

Exact base, head, and tree identify Git objects but do not prove that the reviewed handoff matches the
pushed remote candidate. For a PR handoff, reproduce the freeze before review:

```powershell
git status --short --branch
git rev-parse HEAD
git rev-parse '@{upstream}'
gh pr view --json number,url,state,isDraft,mergeable,headRefOid,statusCheckRollup
```

The local and upstream object IDs must match, and `headRefOid` must name that same head. Record the
actual check conclusions rather than only saying that checks exist. Before a worker-to-root handoff
has a PR or remote topic, mark PR identity, PR state, and remote equality **NotApplicable**, with the
reason; do not invent remote state.

Do not attach raw prompts, accumulated chat history, repeated progress commentary, complete command
transcripts, or entire large artifacts. A command's relevant failure excerpt is evidence; unrelated
successful output is summarized.

## Consolidated Independent Review

For an engine change, check the behavior and responsibility boundary before counting passing gates:

- Does legality follow live state, content and a supported rule, or a fixed reference history?
- Can another valid actor/configuration/history use the same capability without a new production case?
- Does Application execute the reached program/turn flow, with one state authority and meaningful
  failure/atomicity, rather than publish a known endpoint or depend on Godot scheduling?
- Does a small feature force unrelated allowlist/snapshot/protocol edits, and can the owning scope
  remove that coupling instead of adding another patch?
- Do engine unit assertions have independent expected results, and is reference/probe code outside
  production and used directly without another test layer?

Use the counterexample relevant to the changed capability; this is a semantic review, not a demand
for a repository-wide test matrix. Passing a controlled trajectory does not close these questions.

The reviewer first verifies candidate identity, exact scope, dependencies, and private boundary. It
then performs one bounded semantic pass across every owned path and its direct contracts. Findings
that are independently discoverable in the original candidate return as one consolidated batch with
severity, evidence, owner, and smallest correction scope.

Report a destructive or externally harmful P0 immediately; consolidation is never a reason to delay
containment. A later correction round is appropriate when the correction introduces a new defect, the
base advances, a required check was unavailable, or new evidence changes the review boundary. Do not
intentionally drip-feed issues that were visible in the same original candidate.

After correction, independently inspect the correction diff and perform only invalidated checks
required by its scope. Documentation uses direct checks; new-engine behavior uses useful unit tests
and any affected direct verification; research keeps its owning requirements. Preserve completed full
results and failures under the dependency rules in `AGENTS.md` and ADR 0012. Main-gate integration
remains serialized and separate from investigator or worker acceptance.
