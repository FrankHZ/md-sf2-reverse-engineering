# ADR 0004: One Terra Research Worker with Root Acceptance

- Status: **Accepted**
- Decision date: 2026-07-20
- Scope: ordinary Phase 2 reverse-engineering slices
- Integration scope: supplemented by [ADR 0006](./0006-parallel-worktrees-and-topic-branch-integration.md)

## Decision

Use one project-scoped `terra_reverse_engineer` custom agent for each ordinary Phase 2 slice within its
research topic worktree. The root
agent scopes the slice and its acceptance criteria, launches the worker, then reviews the handoff and
diff, reruns verification, scans the tracked/private boundary, stages exact accepted files, and commits
to the current topic branch rather than directly to `main`.
The root does not independently perform the reverse engineering or implementation assigned to the
worker. Review questions return to the same worker through a follow-up. Concurrent write workers are
not used in the slice worktree. ADR 0006 permits a separate design-synthesis lane in another worktree and
owns the serialized integration rules between topic branches.

The worker remains static-first under ADR 0003: inventory a coherent subsystem, create a structured
parser or contract and project-owned tests, document the evidence, and leave only a grouped H3 runtime
question queue. It reports its scope, files, evidence labels and provenance, counter changes, commands,
remaining runtime questions, contract impacts, and clean unstaged/uncommitted status.

The role is defined in `.codex/agents/terra-reverse-engineer.toml`; `.codex/config.toml` limits the
project to two agent threads and one nesting level. When role selection is unavailable to the current
surface, the root explicitly selects `gpt-5.6-terra` when spawning the worker.

The role remains at `xhigh` reasoning effort. Tested bounded checkpoints improved execution without
requiring a broader model change: the Shop slice needed many partial/rejection rounds, while Church and
Caravan each needed one narrow semantic-root rejection. Keep `xhigh`; do not raise the ordinary worker to
`max` or `ultra` on this history alone.

## Why

Phase 2 has a mature static-first and batched-runtime cadence, but discovery, documentation, and final
acceptance require different attention. A dedicated execution worker concentrates on one evidence
slice while the root preserves an independent acceptance boundary. One worker avoids conflicting writes
and keeps the durable repository record coherent.

## Workflow

1. The root specifies the owning topic, bounded source surface, tracked outputs, and one narrow H2/H3
   acceptance command.
2. The Terra worker performs the complete static slice and returns a structured handoff without staging
   or committing. Before handoff it performs an adversarial self-review against the acceptance checklist
   below and reports the weaknesses it corrected.
3. The root compares the handoff with the diff, checks labels, provenance, and counters, and sends any
   correction back to the same worker.
4. The root runs `uv run sf2 verify` and the owning narrow command, scans the private-artifact boundary,
   stages exact accepted paths, reviews the cached diff, and commits to the current research topic
   branch. Push, pull-request, and final-main integration follow ADR 0006.

## Safeguards and Limits

- The worker must not stage, commit, push, branch, or alter ignored private/generated inputs, including
  `local/`, `artifacts/`, `reports/generated/`, ROMs, patches, saves, traces, extracted assets, tools,
  or the upstream checkout.
- The worker must not choose a phase, remake engine, licensing/distribution posture, or other material
  project direction, and must not read or update external memory.
- Neither role runs `uv run sf2 verify --full` by default. The full gate remains limited to milestones,
  release/merge readiness, shared harness changes, or an explicit full-parity request.
- Within the research slice, the root is the sole stager and committer and must inspect the staged file
  list and cached diff before committing. It does not commit ordinary slice work directly to `main`.
- Codex custom-agent configuration is not a security boundary. It supplies role instructions and model
  defaults; it cannot by itself prevent a worker from invoking Git or writing an ignored path. The
  worker policy plus root review, verification, and artifact scan provide the operational safeguard.

## Worker Acceptance Checklist

The first delegated slices showed that passing narrow commands alone does not guarantee an acceptance-
quality contract. Before handoff, the worker therefore checks all of the following:

1. Extractor output, golden fixture, output schema, fixture schema, focused tests, research prose, and any
   design contract agree on one complete data shape.
2. New nested schemas forbid extra fields and require exact known names, counts, values, and ordered
   arrays; a property-count-only schema is not sufficient when the evidence is exact.
3. Focused tests assert the complete new semantic object, not only representative fields already covered
   by the golden comparison.
4. Confirmed control-flow facts have smallest-stable-section guards that preserve branch polarity,
   selector behavior, call/mutation order, and result values.
5. Caller inventories parse call instructions with comments excluded and retain both target identity and
   per-target site counts.
6. Storage-byte counts, logical sizes, physical address intervals, transfer lengths, encoded sizes, and
   loop counters use distinct field names and prose.
7. Static evidence does not promote hardware persistence, caller-visible lifecycle, timing, or rendered
   behavior without the appropriate runtime observation.
8. Every new or materially revised Evidence-date field matches the actual current project date supplied by
   the execution environment; upstream timestamps, stale copied dates, and invented future dates do not
   stand in for the observation date.
9. Canonical field names, fixture IDs, and design terms use original source labels or neutral structural
   language until a semantic interpretation is proven. Original labels and later interpretations remain
   distinguishable when their wording differs.
10. Authoritative constants are parsed once. Dependent masks, strides, spans, widths, and counts are
    derived and, where possible, cross-checked against an independent declaration or consumer. Fixtures
    and schemas pin exact results, but extractor and guard logic do not duplicate the same magic literal as
    a circular second truth.
11. A derivation states a source-backed semantic relationship. Boolean arithmetic, container/string
    cardinality, arithmetic identities, and similar disguises are treated as repeated magic literals, not
    independent evidence.
12. Output and fixture schemas recursively close nested objects reached through properties, array items,
    and definitions. Mutation tests reject missing or renamed required fields, extra nested fields, wrong
    exact order, and boundary violations. Editing a shared contract preserves all pre-existing siblings.
13. New or changed parsers have positive, negative, boundary, comment, near-miss, and legal instruction-
    suffix tests. Text in labels, comments, operands, or similar symbol names cannot inflate an instruction
    or reference inventory.
14. The diff contains no file-wide/module-wide lint escape and no incidental formatting, import-order,
    generated-file, or unrelated semantic churn. Any narrow suppression is unavoidable and documented.
15. If conversational context is unavailable, the worker reconstructs the assignment from the explicit
    slice contract, required project documents, current diff, owning sources/tests, and recent commits. It
    asks one precise question only after those durable sources cannot resolve a genuine blocker.
16. Large exact corpora define a closed reusable item/record shape once and use a compact exact-order
    constraint such as an array `const`; schemas do not duplicate a full singleton-property tree for every
    record. Both schemas still enforce the complete exact value and order.
17. Generated fixture/schema JSON is emitted by a real UTF-8 JSON serializer and parsed immediately. A
    patch transport containing literal `\\n` text is never treated as serialized JSON.
18. Direct-caller audits resolve pinned jump-interface aliases and retain instruction target, effective
    target, and per-target site counts. Alias calls cannot be hidden behind a zero implementation-symbol
    count.
19. The first worker turn begins implementation after repository orientation. If capacity requires internal
    checkpoints, every checkpoint contains a tested bounded change, is resumed from the current diff, and
    is reported as partial rather than acceptance-ready. A no-progress return names a concrete blocker.
20. A reported semantic summary (constant, mask, offset, width, selector scale, capacity, branch, call
    order, or caller total) identifies its specific parsed use-site record, table, or operand. Symbolic
    operands resolve through the one authoritative constants map where applicable, and the parser validates
    identity, order, polarity, and width against that use-site relationship; independently parsed inputs do
    not establish a derivation.
21. A smallest-scope source mutation of the reported use-site operand, opcode, or order makes parser
    construction fail before golden-fixture comparison. Fixture/schema exactness alone is not a derivation
    guard. Internal and external effective-target total maps are zero-inclusive across the complete declared
    target set, not merely positive occurrences.
22. The worker's pre-handoff summary-provenance audit asks whether every reported derived field links to a
    parsed use site and to a mutation capable of falsifying it before fixture comparison.

The root repeats these checks independently. A rejection returns to the same worker and becomes input to
its next self-review; the root does not silently patch the research implementation.

## Consequences

- Normal Phase 2 work has one accountable author and one independent acceptor.
- Worker handoffs become durable input to review rather than a substitute for evidence in tracked docs.
- The root may reject or return a slice, but it should not silently complete the worker's research.
- The tested checkpoint/review history supports retaining `xhigh` reasoning effort; it is not evidence for
  raising the ordinary worker to `max` or `ultra`.
- The existing evidence, copyright, and verification rules remain unchanged.
