# ADR 0004: One Terra Research Worker with Root Acceptance

- Status: **Accepted**
- Decision date: 2026-07-20
- Scope: ordinary Phase 2 reverse-engineering slices

## Decision

Use one project-scoped `terra_reverse_engineer` custom agent for each ordinary Phase 2 slice. The root
agent scopes the slice and its acceptance criteria, launches the worker, then reviews the handoff and
diff, reruns verification, scans the tracked/private boundary, stages exact accepted files, and commits.
The root does not independently perform the reverse engineering or implementation assigned to the
worker. Review questions return to the same worker through a follow-up. Concurrent write workers are
not used in the shared worktree.

The worker remains static-first under ADR 0003: inventory a coherent subsystem, create a structured
parser or contract and project-owned tests, document the evidence, and leave only a grouped H3 runtime
question queue. It reports its scope, files, evidence labels and provenance, counter changes, commands,
remaining runtime questions, contract impacts, and clean unstaged/uncommitted status.

The role is defined in `.codex/agents/terra-reverse-engineer.toml`; `.codex/config.toml` limits the
project to two agent threads and one nesting level. When role selection is unavailable to the current
surface, the root explicitly selects `gpt-5.6-terra` when spawning the worker.

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
   stages exact accepted paths, reviews the cached diff, and commits.

## Safeguards and Limits

- The worker must not stage, commit, push, branch, or alter ignored private/generated inputs, including
  `local/`, `artifacts/`, `reports/generated/`, ROMs, patches, saves, traces, extracted assets, tools,
  or the upstream checkout.
- The worker must not choose a phase, remake engine, licensing/distribution posture, or other material
  project direction, and must not read or update external memory.
- Neither role runs `uv run sf2 verify --full` by default. The full gate remains limited to milestones,
  release/merge readiness, shared harness changes, or an explicit full-parity request.
- The root is the sole stager and committer and must inspect the staged file list and cached diff before
  committing.
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

The root repeats these checks independently. A rejection returns to the same worker and becomes input to
its next self-review; the root does not silently patch the research implementation.

## Consequences

- Normal Phase 2 work has one accountable author and one independent acceptor.
- Worker handoffs become durable input to review rather than a substitute for evidence in tracked docs.
- The root may reject or return a slice, but it should not silently complete the worker's research.
- The existing evidence, copyright, and verification rules remain unchanged.
