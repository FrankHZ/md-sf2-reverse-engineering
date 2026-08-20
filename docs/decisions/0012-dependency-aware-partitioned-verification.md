# ADR 0012: Dependency-Aware Partitioned Verification

- Status: **Accepted**
- Proposal date: 2026-08-20
- Decision date: 2026-08-20
- Scope: repository verification planning and later affected-gate orchestration
- Accepted option: **two verification layers with conservative path/dependency partitions**

## Context

The normal `uv run sf2 verify` gate is intentionally small and currently completes in seconds. The
milestone `uv run sf2 verify --full` profile is intentionally expensive: it runs the complete Python
suite and the maintained H1/H2/H3 milestone rails. It is useful for phase transitions, release
readiness, shared-harness changes, and explicit full-parity checks, but it is not a suitable default
for ordinary static-first reverse-engineering slices.

The repository now has 68 registered narrow H2 commands and 71 registered H3 commands. Most owning
source modules, schemas, fixtures, manifests, and runtime observers have disjoint dependency surfaces.
Treating every evidence change as if it invalidated every surface wastes time and makes it harder to
see which subsystem actually failed. Conversely, letting a worker choose tests informally makes a
missed dependency silent and gives no durable explanation for a skip.

[ADR 0006](./0006-parallel-worktrees-and-topic-branch-integration.md) already establishes that gate
invalidation is path- and dependency-based. This decision makes that rule machine-readable and gives
future orchestration stable scheduling boundaries.

## Decision

Verification has two layers:

1. **Public core:** every accepted change still runs `uv run sf2 verify` and the Public
   tracked-input boundary. This layer is never skipped by the affected planner.
2. **Affected evidence:** a deterministic planner maps a committed Git range to stable Python, H1,
   H2, and H3 partitions. Only selected partitions need run for an ordinary slice. Running every
   partition remains the full milestone meaning even while the current legacy-compatible `--full`
   implementation is migrated toward that model.

The accepted partitions are:

| Partition | Owner | Scheduling boundary |
| --- | --- | --- |
| `tooling-python` | Python tooling and focused regression tests | parallel-safe |
| `h1-original` | bit-perfect rebuild and source/toolchain identity | serialized on `original-rebuild` |
| `h2-battle-logic` | battle control, AI, action, scene-engine, and routing evidence | parallel-safe |
| `h2-stats-items` | ally/enemy/stat/item evidence | parallel-safe |
| `h2-map-scripting` | map data, events, scripts, entities, and imports | parallel-safe |
| `h2-presentation` | graphics, layouts, fonts, text, palettes, and compressed assets | parallel-safe |
| `h2-services-state` | gameflow, menus, services, interrupts, and shared state | parallel-safe |
| `h2-sound` | music data and sound-driver static evidence | parallel-safe |
| `h3-battle01` | `battle01-intro-skip` runtime profile | serialized on `bizhawk-original-runtime` |
| `h3-map-debug` | `map-debug-host` runtime profile | serialized on `bizhawk-original-runtime` |
| `h3-direct-seam` | `direct-function-seam` runtime profile | serialized on `bizhawk-original-runtime` |
| `h3-witch` | `witch-menu` runtime profile | serialized on `bizhawk-original-runtime` |
| `h3-sound` | `sound-driver` runtime profile | serialized on `bizhawk-original-runtime` |

`public-core` is also represented in every plan as the always-run first layer. H3 boundaries consume
the existing closed bootstrap registry rather than duplicating runtime ownership. H2 command
ownership is a closed registry checked against the CLI parser. A new CLI command cannot be added
without assigning a partition.

## Accepted Planner Contract

`uv run sf2 verify plan --base <revision> [--head <revision>]` is read-only. It:

- resolves both revisions to exact commits and diffs their merge base against the head;
- reports normalized changed paths, selected partitions, exact selection reasons, suggested narrow
  commands, resource locks, and unclassified evidence paths as deterministic JSON;
- maps owned H2 modules and named H2 fixtures/schemas/manifests to narrow commands;
- maps H3 dispatch modules and observers through the accepted bootstrap profile registry;
- scans Python test imports and transitive reverse dependencies of shared Python modules to retain
  their H2/H3 owners while suggesting the changed test file;
- fans shared harness, toolchain, legacy, or unknown evidence inputs out conservatively;
- supports repeated `--include-partition` arguments for semantic dependencies that a path diff cannot
  express; and
- never runs a gate, writes a cache, changes Git state, or changes the behavior of `verify` or
  `verify --full`.

`unclassifiedPaths` is a visible maintenance queue, not permission to omit verification. An unknown
path under an evidence-owning root selects all plausible partitions. Documentation-only paths select
only the always-run public core unless their change is accompanied by, or explicitly declares, an
evidence dependency.

The planner operates on committed revisions. Dirty and untracked files are deliberately outside its
claim; a lane must commit an exact candidate or use explicit partition inclusion before treating the
plan as merge evidence.

## Current Implementation Boundary

This decision's first implementation slice adds the registry, planner, CLI surface, and coverage
tests only. It does **not**:

- execute partitions or replace either existing verification profile;
- cache success by tree or dependency digest;
- create signed/attested gate receipts;
- start parallel agents or decide worktree ownership;
- split the complete Python suite beyond suggesting directly changed test modules; or
- extract a standalone H1 command. Until that later slice exists, an affected `h1-original` result
  still requires the existing serialized rebuild/full-profile route.

A later executor may hand independent selected partitions to subagents, but the CLI-generated plan is
the authority for what must run. Agents may diagnose and report one partition; they may not silently
remove a selected partition. H2 partitions can run in isolated worktrees in parallel. H3 sessions
remain serialized when they share the host runtime/private scratch boundary, and private inputs stay
isolated as required by the root worktree contract.

## Consequences

- Ordinary reverse-engineering changes gain a reproducible affected gate list without weakening the
  normal commit gate.
- Partition ownership is maintained next to executable command registries and checked for complete
  CLI coverage.
- Broad or ambiguous changes fail conservatively, so a planner defect costs time rather than evidence.
- The first slice improves planning but not wall-clock time by itself; execution, per-partition
  receipts, and selective cache reuse remain separately reviewable follow-ups.
- Existing full-profile results and invalidation rules remain valid during the migration.
