# ADR 0012: Dependency-Aware Partitioned Verification

- Status: **Accepted**
- Proposal date: 2026-08-20
- Decision date: 2026-08-20
- Scope: repository verification planning and later affected-gate orchestration
- Accepted option: **two verification layers with conservative path/dependency partitions**

## Current scope amendment

The explicit user test policy in [ADR 0019](./0019-state-and-content-driven-remake-engine.md) controls
new-engine work: add engine behavior unit tests, run needed verification directly, and add no tests
of verification infrastructure. Old engine tests may migrate or retire without count parity,
replacement-per-deletion or an old green aggregate. Documentation-only work uses direct checks.

The planner still emits legacy public-core/.NET/Godot/test selections. That current behavior is not
the revised engine policy. The M0/M1 workflow, harness, planner and required-check cutover remains
separately owned and unimplemented. Use the
[verification owner](../../remake/docs/development-and-verification.md#repository-planner) to keep
current commands, binding policy and future cutover distinct. Research evidence and genuinely shared
dependencies retain their affected requirements; this does not delete or weaken original evidence.

## Context

The normal `uv run sf2 verify` gate is intentionally small and currently completes in seconds. The
milestone `uv run sf2 verify --full` profile is intentionally expensive: it runs the complete Python
suite and the maintained H1/H2/H3 milestone rails. It is useful for phase transitions, release
readiness, shared-harness changes, and explicit full-parity checks, but it is not a suitable default
for ordinary static-first reverse-engineering slices.

Most research modules, schemas, fixtures, manifests, and runtime observers have disjoint dependency
surfaces. Current command ownership is derived from the executable registries, not copied totals.
Treating every evidence change as if it invalidated every surface wastes time and makes it harder to
see which subsystem actually failed. Conversely, letting a worker choose tests informally makes a
missed dependency silent and gives no durable explanation for a skip.

[ADR 0006](./0006-parallel-worktrees-and-topic-branch-integration.md) already establishes that gate
invalidation is path- and dependency-based. This decision makes that rule machine-readable and gives
future orchestration stable scheduling boundaries.

## Decision

Verification has two layers:

1. **Existing research/public core:** the research lane uses `uv run sf2 verify` and its public
   tracked-input boundary. The current planner always selects this layer; the scope amendment above
   prevents that implementation fact from imposing it on documentation or the new engine.
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
without assigning a partition. Closed artifact indexes are derived from those command modules,
recursive local or registry-URI schema references, fixture shard references, and H3 bootstrap launch
declarations. Explicit mappings retain the known command-less legacy H2/H3 milestone artifacts and
shared BizHawk libraries. Enumeration tests require every tracked H2 and H3 fixture, schema,
extraction manifest, and observer/library input to have exact command or known shared-partition
ownership with no unclassified owner artifact.

## Accepted Planner Contract

`uv run sf2 verify plan --base <revision> [--head <checked-out-HEAD-revision>]` is read-only. It:

- resolves both revisions to exact commits, requires the requested head to equal the checked-out
  `HEAD^{commit}`, requires a porcelain-clean analyzed worktree, and then diffs their merge base
  against that checked-out committed head;
- reports normalized changed paths, selected partitions, exact selection reasons, suggested narrow
  commands, resource locks, and unclassified evidence paths as deterministic JSON;
- maps owned H2 modules and their declared/recursive fixture, schema, and extraction-manifest graph
  to narrow commands;
- maps H3 dispatch modules, declared/recursive fixtures and schemas, observers, and case fixtures
  through the accepted bootstrap profile registry;
- selects an existing ordinary Python test file's own pytest command; importing a production module
  does not mean that module changed and does not invalidate its H1/H2/H3 owners;
- scans transitive reverse dependencies when a shared production Python module actually changes,
  retaining its H1/H2/H3 owners;
- fans the CLI, harness, ROM/toolchain identity manifests, Python toolchain/lock inputs, and legacy
  scripts to every evidence partition; a shared Python module that transitively reaches the harness
  also selects H1;
- retains broad fanout only for genuinely unknown evidence-root paths and reports each one;
- supports repeated `--include-partition` arguments for semantic dependencies that a path diff cannot
  express; and
- never runs a gate, writes a cache, changes Git state, or changes the behavior of `verify` or
  `verify --full`.

Planner mode rejects execution-only modifiers rather than silently ignoring them. `--full`,
`--quick`, `--skip-rebuild`, `--skip-extraction`, `--skip-runtime`, and non-default ROM or upstream
paths cannot be combined with the `plan` subcommand. Ordinary `verify`, `verify --full`, and an
unmodified `verify plan` retain their separate dispatch behavior.

`unclassifiedPaths` is a visible maintenance queue, not permission to omit verification. An unknown
path under an evidence-owning root selects all plausible partitions. Documentation-only paths select
only the always-run public core unless their change is accompanied by, or explicitly declares, an
evidence dependency. This describes the current selection output; the scope amendment determines
whether that output is an applicable obligation or pending engine-cutover work.

### Test Changes and Production Invalidation

This replaces the earlier test-import propagation rule. A historical-wording change in
`tests/python/test_native_harness.py` still imports the unchanged shared BizHawk helper and direct H2
owners for other tests in the file. Following those imports backward through production consumers
would incorrectly treat the helper and its consumers as changed. Ordinary existing test files now
select their own pytest command without that propagation, whether they import a direct H2/H3 command
owner or a shared helper.

Actual production, fixture, schema, manifest, and observer changes still select their owners and
reverse dependencies. Explicit remake, asset, and runner test mappings retain their additional gates;
a deleted ordinary test currently falls back to the complete Python suite. Deliberate engine-test
retirement must remove that obsolete fanout during the coordinated cutover; it does not require
running the old aggregate or adding tests of its replacement selection. If a test change introduces an
original-runtime acceptance requirement that the changed source paths do not express, the lane must
declare that semantic dependency through the existing `--include-partition` option. Test selection is
not permission to omit such a requirement.

The planner's own existing production-path mapping selects the complete `test_verification_plan.py`
and `test_native_harness.py` files for its Python gate. Its direct production consumer is the CLI;
the planner tests cover CLI parsing, dispatch, execution-mode separation, Git-range admission,
partition selection, and artifact ownership, while the native harness covers the shared public CLI
contract. This scoped command selection keeps the mapping's .NET/Godot gates and production reverse
dependencies. It does not narrow any other shared production module's default Python gate.

### Checked-Out Candidate Consistency

The planner operates only on a clean checked-out committed head. It rejects a different committed
head, including an arbitrary remote head, because artifact and import ownership are derived from the
checked-out filesystem rather than Git blobs. It also rejects tracked changes and non-ignored
untracked files before classification. A lane must check out and commit the exact candidate before
treating the plan as merge evidence; explicit partition inclusion does not bypass these consistency
checks.

## Current Implementation Boundary

The current planner is read-only selection infrastructure. Its earlier implementation added the
registry, CLI surface and coverage tests; those historical tests do not establish a new-test obligation.
It does **not**:

- execute partitions or replace either existing verification profile;
- cache success by tree or dependency digest;
- create signed/attested gate receipts;
- start parallel agents or decide worktree ownership;
- split the complete Python suite beyond suggesting directly changed test modules; or
- extract a standalone H1 command. Until that later slice exists, an affected `h1-original` result
  still requires the existing serialized rebuild/full-profile route.

A later executor remains separately owned. Interpret selection under the current scope amendment;
declare genuine research dependencies and any pending engine cutover rather than silently relabeling
an unrun command as passed. Planner output does not authorize creating agents or environments.
H2 partitions can run in isolated worktrees in parallel. H3 sessions
remain serialized when they share the host runtime/private scratch boundary, and private inputs stay
isolated as required by the root worktree contract.

## Consequences

- Ordinary reverse-engineering changes gain a reproducible affected gate list without weakening the
  normal commit gate.
- Partition ownership is maintained next to executable command registries and derived declarations,
  and is checked for complete CLI and tracked-artifact coverage.
- Broad or ambiguous changes fail conservatively, so a planner defect costs time rather than evidence.
- The first slice improves planning but not wall-clock time by itself; execution, per-partition
  receipts, and selective cache reuse remain separately reviewable follow-ups.
- Existing full-profile results and invalidation rules remain valid during the migration.
