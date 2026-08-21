# ADR 0013: Token-Efficient Agent Research without Weakening Evidence

- Status: **Proposed**
- Proposal date: 2026-08-20
- Scope: agent context, review, telemetry, and artifact-inspection workflow
- Proposed option: **preserve full-game reverse engineering while reducing repeated context and
  orchestration work**

## Context

The repository mission has always included a full reverse engineering of the US Mega Drive/Genesis
release. The broad Phase 2 evidence surface is therefore not an accidental expansion from a narrowly
scoped first-map prototype. Its complete inventories, parsers, schemas, fixtures, runtime matrices,
research notes, and implementation-neutral contracts are durable project value.

[ADR 0009](./0009-first-phase4-playable-slice.md) and
[ADR 0010](./0010-map3-battle01-product-acceptance.md) later selected Map 3 through completion of
Battle 01 as the first playable milestone. That selection narrows the **current scheduling frontier**;
it does not replace the long-term full-game research mission or make already accepted evidence
unnecessary.

The user reports that the reverse-engineering effort has consumed roughly three to four billion
aggregate tokens. That order of magnitude is an operational signal, not a reproducible repository
metric. The repository does not contain the provider-side split among uncached input, cached input,
cache writes, reasoning, and output tokens, and it must not infer monetary cost or exact efficiency
from an aggregate counter alone.

Official OpenAI model guidance recommends tracking cached-token fields, states that long sessions can
amplify repeated prompt and tool content, and recommends stating instructions once and exposing only
relevant tools. It also reports a 41--66% total-token reduction in one internal coding-agent evaluation
after prompt simplification, while explicitly warning that the result is directional and must be
validated on representative workloads. Those figures are external guidance, not a forecast or an
acceptance target for this repository.

## Reproduced Repository Audit

The following tracked-state audit was reproduced on exact base
`2ef28bfeb5ad62ec21d2b711867ed2c290d2f714`, tree
`e7e2188327740cc8cafad6c8f4edb51f5a07c438`:

| Surface | Reproduced value |
| --- | ---: |
| Repository history | 578 commits from 2026-07-17 through 2026-08-20 |
| Current tracked tree | 1,139 files / 46,905,444 blob bytes |
| Historical text churn | 2,282,289 added lines / 1,031,630 deleted lines |
| Research / design-contract / decision documents | 38 / 67 / 12 |
| JSON schemas / JSON fixtures | 326 / 175 |
| `AGENTS.md` tracked blob | 26,125 bytes |
| Root `README.md` tracked blob | 17,308 bytes |
| `docs/README.md` tracked blob | 67,009 bytes |
| `docs/research/source-coverage.md` tracked blob | 79,836 bytes |

The repository metrics above use Git objects, not checkout newline conversion or filesystem length.
They reproduce with PowerShell 7 from any checkout containing the exact commit:

```powershell
$auditRevision = '2ef28bfeb5ad62ec21d2b711867ed2c290d2f714'
$paths = @(git ls-tree -r --name-only $auditRevision)

git rev-list --count $auditRevision
git log --reverse --format='%cs %H' $auditRevision | Select-Object -First 1
git log -1 --format='%cs %H' $auditRevision
$paths.Count

[long]$trackedBytes = 0
foreach ($entry in @(git ls-tree -r -l $auditRevision)) {
    if ($entry -match '^\d+\s+\w+\s+[0-9a-f]+\s+(\d+)\t') {
        $trackedBytes += [long]$matches[1]
    }
}
$trackedBytes

[long]$addedLines = 0
[long]$deletedLines = 0
foreach ($entry in @(git log --numstat --format= --no-renames $auditRevision)) {
    if ($entry -match '^(\d+)\s+(\d+)\s+') {
        $addedLines += [long]$matches[1]
        $deletedLines += [long]$matches[2]
    }
}
@{ Added = $addedLines; Deleted = $deletedLines }

@{
    ResearchDocs = @($paths | Where-Object { $_ -match '^docs/research/[^/]+\.md$' }).Count
    DesignContracts = @($paths | Where-Object { $_ -match '^docs/design/contracts/[^/]+\.md$' }).Count
    Decisions = @($paths | Where-Object { $_ -match '^docs/decisions/[^/]+\.md$' }).Count
    Schemas = @($paths | Where-Object { $_ -match '^schemas/.*\.json$' }).Count
    FixtureJson = @($paths | Where-Object { $_ -match '^tests/fixtures/.*\.json$' }).Count
}

git cat-file -s "${auditRevision}:AGENTS.md"
git cat-file -s "${auditRevision}:README.md"
git cat-file -s "${auditRevision}:docs/README.md"
git cat-file -s "${auditRevision}:docs/research/source-coverage.md"
```

The accepted source-coverage ledger on that base reports 381 / 387 pinned code files with executable
reach, all 1,690 pinned data ASM files assigned to deterministic H2 inventory, 74 H2 fixture files,
92 H3 fixture files, and 2,568 address bindings. These are not line-coverage percentages, but they
show why the research has substantial preservation and implementation value.

The audit also identifies repeatable amplification risks:

1. long-lived controller and reviewer tasks accumulate prior handoffs, corrections, and tool output;
2. a worker fork that inherits unrelated controller turns duplicates that history before reading its
   bounded source surface;
3. restart guidance can cause several large global documents to be read before the owning subsystem
   document;
4. worker self-review, root acceptance, main-gate review, correction, and re-review can each reread
   the same patch and surrounding prose;
5. multi-megabyte schemas and fixtures are efficient machine inputs but expensive model inputs when
   opened or echoed instead of summarized; and
6. dependency-aware verification reduces wall-clock work, but does not by itself reduce context or
   repeated semantic review.

The Git audit proves repository scale and churn, not the share of aggregate tokens attributable to
any one risk. Provider-side or client-side usage telemetry is required before assigning percentages.

## Proposed Decision

### Preserve the mission and evidence bar

Full-game reverse engineering remains the long-term research mission. The Map 3-through-Battle 01
milestone controls near-term ordering only. Token efficiency must not weaken evidence labels,
provenance, closed inventories, schema and fixture validation, H3 callback/error requirements,
private-input boundaries, root acceptance, or independent main-gate review.

A token budget is diagnostic. It is never permission to omit an accepted artifact, weaken a golden,
silently skip an affected partition, or mark an incomplete slice complete.

### Measure comparable slices before claiming savings

When the active client exposes the fields, each accepted slice should retain a compact operational
summary of:

- input, cached-input, cache-write, reasoning, and output tokens by worker, root, and reviewer role;
- number of agent turns, correction rounds, and runtime launches;
- first-pass acceptance or the severity and owner of each correction;
- accepted paths and the exact focused/public commands; and
- whether a compaction, worker replacement, or unrelated-history fork occurred.

Raw prompts, private source text, tool transcripts, ROM-derived content, and personal billing data
must not be committed. A later implementation may keep machine-local telemetry and publish only a
small aggregated, non-sensitive project report. Missing fields remain unknown; they are not estimated
from elapsed time or repository line counts.

Compare at least three representative slices after adoption with matched earlier or baseline slices.
Judge efficiency together with first-pass acceptance, evidence completeness, defects found after
handoff, and gate results. Lower token use is an improvement only when those quality measures do not
regress.

### Start ordinary workers with bounded context

An ordinary independent Phase 2 worker should be created with no inherited conversation turns
(`fork_turns: "none"`) and a self-contained ADR 0004 slice contract. That contract still names the
exact base, worktree, owning document, bounded source surface, owned tracked files, expected outputs,
one narrow H2/H3 command, required references, and explicit exclusions.

Continue an existing slice with the same worker so its relevant local reasoning is not discarded.
Inherit prior turns only when the new task genuinely depends on them and the handoff cannot be stated
compactly from durable repository facts. Controller chat history is collaboration state, not evidence
and not a substitute for the tracked owning artifacts.

### Route global context instead of reading it by default

`AGENTS.md` should remain the compact, always-applicable mission, safety, ownership, and phase
boundary. Incident-specific procedures and detailed lane checklists should move to routed operational
documents or skills that are read only when their trigger applies.

Resume guidance should offer a small current-frontier view containing exact main identity, active
milestone, active lanes, accepted counters, immediate dependencies, and owning links. A subsystem
worker reads that view, ADR 0004's checklist, its owning document, and the bounded sources named in its
slice. It does not routinely reread the complete root status, documentation index, and full
source-coverage ledger unless the slice changes or audits those global surfaces.

The later implementation must preserve a durable route to every detailed rule. Prompt reduction must
not strand a required constraint only in an external task or personal memory.

### Keep large artifacts machine-reduced

Agents should inspect multi-megabyte schemas, fixtures, manifests, traces, and diffs through scripts
that return bounded structured summaries: identity, counts, hashes, owner paths, validation failures,
and only the minimum differing records or source excerpts required for judgment. Raw large payloads
remain available to deterministic tools and exact local inspection, but are not copied wholesale into
worker prompts, review handoffs, or durable prose.

### Consolidate independent review

Worker self-review, root acceptance, and main-gate review remain separate responsibilities. Each
handoff supplies exact base/head/tree, exact paths, planner partitions, gates, and a concise evidence
summary rather than the full controller history.

Main-gate should finish one bounded review pass and return one consolidated findings batch before a
correction begins. A later round is appropriate when the correction creates a new defect, the base
advances, a required check was unavailable, or new evidence changes the review boundary. Issues that
were independently discoverable in the same original patch should not be intentionally drip-fed as
separate correction turns.

### Use the affected planner without changing milestone semantics

[ADR 0012](./0012-dependency-aware-partitioned-verification.md) remains authoritative. Every accepted
change runs the public core and its selected affected partitions. The old full profile remains a
milestone, release/merge-readiness, shared-harness, or explicit full-parity gate. This decision does
not reinterpret a long-running command as a large token consumer: gate wall-clock time and model
context are measured separately.

## Proposed Adoption Sequence

Acceptance of this ADR would authorize later, separately reviewed implementation slices in this
order:

1. define the compact, privacy-safe slice telemetry record and capture only fields the client exposes;
2. add a small current-frontier view and update resume routing to consume it;
3. reduce duplicated always-injected instructions while preserving routed normative rules;
4. change ordinary root-to-worker creation to the bounded no-history contract;
5. standardize machine-reduced artifact and diff summaries; and
6. evaluate three representative slices before accepting any quantitative savings claim.

Each slice must declare its exact paths and active-lane dependencies. The current proposal does not
edit `AGENTS.md`, change a model or reasoning effort, implement telemetry, alter the planner, or start
Phase 4.

## Consequences and Risks

- A smaller inherited context can omit relevant nuance. The mitigation is a complete slice contract,
  durable owning documents, and same-worker continuation for the active slice.
- Consolidated review can take longer before the first response. It should reduce correction churn
  without weakening independent review or preventing immediate reporting of a destructive P0 issue.
- Client token fields may be absent or use a provider-specific accounting boundary. Missing data stays
  explicit, and cached-token volume is not treated as uncached monetary cost.
- Moving detailed rules out of `AGENTS.md` can create routing failures. No rule is removed until its
  replacement location, trigger, and link are independently reviewed.
- Machine summaries can hide malformed detail if their reducers are weak. Reducers need deterministic
  tests and must retain exact owner paths and bounded failure excerpts.
- Full-game research remains valuable and may remain expensive even after orchestration improves.
  Efficiency is judged against accepted evidence quality, not against a promise that reverse
  engineering should become cheap.

## References

- [OpenAI model guidance: prompt, context, caching, and measurement](https://developers.openai.com/api/docs/guides/latest-model),
  accessed 2026-08-20.
- [ADR 0003: Static-First Research with Batched Runtime Observation](./0003-static-first-batched-runtime-research.md).
- [ADR 0004: One Terra Research Worker with Root Acceptance](./0004-single-terra-worker-with-root-acceptance.md).
- [ADR 0006: Parallel Worktrees with Topic-Branch Integration](./0006-parallel-worktrees-and-topic-branch-integration.md).
- [ADR 0012: Dependency-Aware Partitioned Verification](./0012-dependency-aware-partitioned-verification.md).
- [Source coverage ledger](../research/source-coverage.md).
