# Shining Force II Reverse Engineering & Remake

This repository studies the USA Mega Drive/Genesis release of **Shining Force II**, turns observed
original behavior into reproducible research and implementation-neutral contracts, and uses those
contracts as the basis for an independently maintained remake.

The repository is the durable project record. It must be possible to resume work without an old chat
or an external memory store. Findings, open questions, reproduction commands, design boundaries, and
project decisions belong in their owning tracked documents.

## Current Status

- **Phase 1 — Reproducible Original:** complete. The pinned community disassembly rebuilds the
  user-provided USA ROM byte-for-byte under the maintained verification workflow.
- **Phase 2 — Discovery and Contracts:** active. Research proceeds static-first, with grouped runtime
  observations only where source and ROM evidence cannot close behavior.
- **Design synthesis:** active alongside Phase 2. Cross-subsystem documents may explain accepted
  evidence from `main`; they do not promote guesses into original-game facts.
- **Phase 4 engine/tooling baseline:** accepted in
  [ADR 0008](./docs/decisions/0008-godot-csharp-cli-first-remake-tooling.md): Godot 4.7.2 .NET with
  C#, CLI-first gates, a plain-C# domain layer, a thin Godot adapter, and optional removable MCP
  tooling.
- **Remake implementation:** active within a bounded Phase 4 start explicitly authorized by the user
  on 2026-08-28 under
  [ADR 0016](./docs/decisions/0016-remake-start-evidence-deferral.md). The tracked [`remake/`](./remake/)
  project now contains plain-C# Domain and Application assemblies, validated public/private Content
  adapters, and a Godot host. Its current public-synthetic and private-local Map 3 capabilities
  and retained Unknowns are summarized by the [remake capability owner](./remake/docs/capability-status.md).
  The accepted first playable milestone is still one continuous scenario from Map 3 through
  completion of Battle 01 under
  [ADR 0009](./docs/decisions/0009-first-phase4-playable-slice.md). Its Research/Design closures,
  main-gate readiness, and H4 result remain incomplete eventual acceptance work, not default
  prerequisites for the authorized bounded implementation. No distributable asset strategy or MCP
  implementation has been selected.

The [state/content-driven engine direction](./docs/decisions/0019-state-and-content-driven-remake-engine.md)
is adopted. The common session now runs two authored battle packages through movement, cancellation,
HEAL/STAY, automatic AI waiting and natural rounds. Production rules and content do not depend on the
[transitional reference implementation](./remake/reference/README.md). Private battle/program migration
and A1–A8 remain open; the [current M1 boundary](./docs/decisions/0019-state-and-content-driven-remake-engine.md#current-m1-implementation)
names implemented support and limitations. Tests cover actual engine behavior; reference/probe/gate
programs are used directly without another test layer. Old tests may migrate or retire by behavior.

This README intentionally does **not** maintain fixture totals, address counts, coverage percentages,
or per-subsystem corpus sizes. Those snapshots became stale as soon as another research slice merged.
Use the executable sources of truth instead:

- [`docs/research/source-coverage.md`](./docs/research/source-coverage.md) defines coverage
  denominators, evidence cadence, and the current research frontier.
- [`docs/operations/agent-resume.md`](./docs/operations/agent-resume.md) is the compact task-resume
  route; it points to durable owners without copying their changing counters.
- [`manifests/research-index.json`](./manifests/research-index.json) owns indexed findings and their
  source, address, fixture, and document relationships.
- [`docs/README.md`](./docs/README.md) routes readers to the closest research, design, or decision
  owner.
- `uv run sf2 research-index test` and `uv run sf2 verify` reproduce current tracked counters and
  validate their relationships.

## Start Here

For any new task:

1. Apply the stable repository rules and owner links in [`AGENTS.md`](./AGENTS.md) once; if the client
   already injected it, do not reread it or copy its routed details into a task handoff.
2. Inspect the exact Git identity, status, active worktrees, and recent commits at runtime.
3. Use [`docs/operations/agent-resume.md`](./docs/operations/agent-resume.md) to find the closest
   owner, then read only that owner and the decisions or checklists required by the task.
4. Open the complete [`docs/research/source-coverage.md`](./docs/research/source-coverage.md) or
   [`docs/README.md`](./docs/README.md) only for global coverage, frontier, routing, or index work.
5. Reproduce a claim from its named command and fixture rather than copying a progress summary.

The design-synthesis entry point is
[`docs/design/documentation-roadmap.md`](./docs/design/documentation-roadmap.md). Current synthesis
documents include:

- [`gameplay-overview.md`](./docs/design/synthesis/gameplay-overview.md);
- [`tactical-battle-loop.md`](./docs/design/synthesis/tactical-battle-loop.md);
- [`progression-and-economy.md`](./docs/design/synthesis/progression-and-economy.md);
- [`story-progression.md`](./docs/design/synthesis/story-progression.md).

English is the canonical authoring and review language for current design synthesis. zh-CN
localization proceeds as dedicated batches under the accepted glossary at
[`docs/design/glossary.md`](./docs/design/glossary.md), with mirrors under `docs/design/zh-CN/`
that preserve each English source's relative hierarchy; English remains the review baseline.

## Project Goals

The project is building a repeatable path from private original input to independently maintained
outputs:

1. identify, split, and rebuild the original ROM reproducibly;
2. document code, data formats, state machines, and runtime behavior with provenance;
3. export canonical structured data under explicit schemas;
4. reconstruct implementation-neutral game-design rules from accepted evidence;
5. run the same behavioral fixtures against a future remake;
6. distinguish original fidelity from intentional modernization.

This is not an emulator project, a pre-patched ROM distribution, or a collection of one-off ROM
experiments. ROM hacks, community patches, and editors may be comparative research inputs, but they
are not the original baseline and do not define the remake architecture.

## Evidence Model

Every non-trivial reverse-engineering claim uses one of three labels:

- **Confirmed:** reproduced by a project-owned script/test, or directly supported by named
  disassembly locations and observed runtime behavior.
- **Inferred:** strongly supported but not yet reproduced independently.
- **Unknown:** an explicit open question that must not be filled with a convenient assumption.

The verification layers keep different kinds of evidence separate:

| Layer | Purpose | Acceptance boundary |
| --- | --- | --- |
| H0 | Input identity | Size, hashes, header, product/region data, and ROM checksum match the pinned manifest. |
| H1 | Original rebuild | The pinned upstream source rebuilds byte-for-byte against the private input. |
| H2 | Static extraction | Source/ROM structure validates against schemas and deterministic canonical output. |
| H3 | Original runtime behavior | Grouped scenarios execute in the pinned emulator and match small state facts or traces. |
| H4 | Remake parity | The future implementation consumes the same implementation-neutral fixtures; deviations are explicit decisions. |
| H5 | Distribution boundary | Published outputs contain no ROM, extracted copyrighted assets, or unreviewed third-party code. |

A static reference proves a relationship, not normal-play reachability. A handler-local runtime fixture
proves its observation seam, not complete caller behavior or presentation. When owners disagree, keep
the disagreement visible and design a focused test.

## Pinned Original Baseline

The canonical private input is the USA retail ROM stored under ignored `local/` state. Its stable
identity is:

| Field | Value |
| --- | --- |
| Size | 2,097,152 bytes |
| SHA-256 | `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9` |
| SHA-1 | `22DEFC2E8E6C1DBB20421B906796538725B3D893` |
| MD5 | `6473B1505334EF5620D13191C18251FE` |
| Product code | `GM MK-1315 -00` |
| Region | `U` |
| Header/computed checksum | `0x8921` / `0x8921` |

These values identify the input; they do not authorize redistribution.

The primary community reference is
[`ShiningForceCentral/SF2DISASM`](https://github.com/ShiningForceCentral/SF2DISASM). Original-game
research is pinned to branch `master`, commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`. The upstream `build/standard` branch is comparative
community work, not the original baseline.

The pinned upstream checkout did not provide an explicit license file at the reviewed revision. It
therefore stays under ignored `local/upstream/`; this repository records provenance and behavior rather
than vendoring or relicensing that source.

## Repository Layout

Directories are added only when a concrete slice owns content:

```text
AGENTS.md              Workflow, evidence, safety, and completion contract
README.md              Stable project entry point
docs/research/          Source-backed reverse-engineering findings
docs/design/contracts/  Evidence-bound implementation-neutral subsystem contracts
docs/design/synthesis/  Cross-subsystem and player-facing design synthesis
docs/design/            Shared design governance and localization roots
docs/decisions/         Durable architecture and tooling decisions
schemas/                Canonical extracted-data and fixture contracts
manifests/              Input, extraction, toolchain, and research indexes
src/sf2tool/            Maintained Python CLI, extractors, verifiers, and harnesses
tools/                  Repeatable inspection and emulator-support tools
scripts/                Frozen compatibility layer for remaining legacy H1–H3 rails
tests/fixtures/         Small redistributable metadata and behavioral expectations
tests/python/           Project-owned unit and contract tests
remake/                 Bounded Phase 4 implementation under the accepted runtime architecture
local/                  Ignored ROMs, saves, traces, upstream checkouts, and generated output
```

The tracked `remake/` path now exists for the explicitly authorized bounded Phase 4 implementation.
It currently contains concrete Domain, Application, Content, and thin Godot layers. Its architecture,
runtime profiles and trust boundaries, capability status, and verification workflow are owned under
[`remake/docs/`](./remake/docs/); new paths are still added only when a concrete slice owns them.

Research explains evidence. Evidence-bound design contracts express implementation-neutral behavior.
Cross-subsystem design synthesis connects accepted owners. Decision records own project choices. A
design document cannot be used to prove the research it cites.

## Setup and Verification

Python 3.12+ is the maintained tooling language. `uv` owns dependency resolution and execution.

```powershell
uv sync --locked
uv run sf2 init --rom-path <path-to-a-legally-owned-USA-ROM>
uv run sf2 verify
```

The existing normal `uv run sf2 verify` command runs research/public checks and private input/toolchain
provenance stages. Research slices pair it with their owning narrow H2/H3 command. For remake or
documentation work, use the [current verification scope](./remake/docs/development-and-verification.md#scope):
`uv run sf2 verify engine`, affected adapter compilation/direct observations, or direct document checks.
The scoped CI/planner no longer selects the old whole-solution gate for engine-only changes.

Useful research/navigation commands, selected only for the relevant work, include:

```powershell
uv run sf2 design-contracts test
uv run sf2 research-index list --summary
uv run sf2 verify plan --base origin/main --head HEAD
uv run sf2 texture --help
```

The texture commands and [technical graphics owner](./docs/research/technical-graphics.md) describe
private extraction, map/UI rendering and their original-fidelity limits. Extracted images are private
outputs, not evidence of a running Godot scene. Do not repeat extraction because a topic changes.

`verify plan` inspects a clean checked-out committed head and reports changed paths, current selected
partitions, reasons and unresolved ownership. It does not run gates or change Git. Remake paths use
the engine scope automatically; shared CLI/harness/planner changes retain conservative research
selection unless their reviewed engine-only wiring scope is explicit. The verification owner documents
`--scope engine` and its limits. Research dependencies retain their owning acceptance requirements.

`verify --full` remains exceptional for applicable research milestones, shared evidence-harness
semantics, release boundaries or explicit full-parity work. Neither ordinary engine changes nor
documentation updates automatically require it. Preserve completed runs and failures, then rerun only
actually invalidated checks required by the owning scope.

Generated outputs belong under ignored `local/` paths. Tools must support read-only input or an
explicit output directory; they must never patch the canonical ROM in place.

## Branch, Worktree, and Review Workflow

`main` is the serialized integration branch. Start a short-lived topic from accepted `origin/main` in
the task's existing dedicated isolated worktree after its old topic is merged, tracked state is clean,
and owned processes are settled. Create another worktree only for concurrent writers or a concrete
reproduction/isolation need:

- `codex/research-*` for Phase 2 evidence, parsers, fixtures, and owning research docs;
- `codex/design-*` for cross-subsystem design synthesis from accepted `main` evidence;
- `codex/tooling-*` for maintained tooling changes;
- `codex/remake-*` for engine and Godot work;
- `codex/repo-*` for repository governance and documentation structure.

Research and design lanes may run concurrently only when their path ownership is explicit. Each lane
rebases onto current `origin/main`, runs its path-dependent acceptance commands, stages exact paths,
and leaves final acceptance and merge to the integration owner.

The detailed contracts are:

- [ADR 0003 — Static-first batched runtime research](./docs/decisions/0003-static-first-batched-runtime-research.md);
- [ADR 0004 — Single Terra worker with root acceptance](./docs/decisions/0004-single-terra-worker-with-root-acceptance.md);
- [ADR 0006 — Parallel worktrees and topic-branch integration](./docs/decisions/0006-parallel-worktrees-and-topic-branch-integration.md).

## Private Inputs and Distribution

The user-provided ROM and any user-provided patches are private research inputs. Do not commit, upload,
attach, or redistribute:

- ROM images, patched/rebuilt ROMs, or pre-patched downloads;
- SRAM, save states, traces, emulator movies, or memory dumps;
- extracted dialogue, graphics, maps, music, sound effects, or other original assets;
- downloaded executable tools or unreviewed third-party source.

Tracked fixtures should contain only the minimum redistributable state facts needed to reproduce a
contract. A future remake must use placeholders or properly licensed assets for distributable builds.

## Roadmap

- **Phase 0 — Bootstrap:** repository safety, private-input isolation, and tool discovery — complete.
- **Phase 1 — Reproducible Original:** pinned input, toolchain, split/build, and byte comparison — complete.
- **Phase 2 — Discovery and Contracts:** static subsystem inventory, structured extraction, grouped original
   runtime fixtures, and evidence-bound design contracts — active.
- **Phase 3 — Game Design Reconstruction:** connected player-facing rules, maps, roster space, numerical curves,
   battle simulation, and explicit modernization choices — partially prepared by current design
   synthesis; upper-layer decisions remain future work.
- **Phase 4 — Modern Engine Vertical Slice:** the Godot 4.7.2 .NET/C# baseline and first continuous
  Map 3-through-Battle 01-completion milestone are accepted as the eventual acceptance target;
  Research/Design gap audits, main-gate readiness, and H4 remain open for that target. The user
  explicitly authorized a bounded implementation start on 2026-08-28 under ADR 0016. The current
  `remake/` includes bounded public-synthetic and private-local Map 3 runtime capabilities across the
  four accepted layers, without claiming the eventual milestone, original presentation, or H4. Those
  eventual acceptance gaps are not default prerequisites for continuing an authorized bounded slice.
- **Phase 5 — Content and Productization:** licensed/placeholder assets, localization, accessibility,
   distribution, and release QA — not started.

Near-term design direction, evidence prerequisites, and stop conditions are owned by
[`docs/design/documentation-roadmap.md`](./docs/design/documentation-roadmap.md). Current research
direction is owned by [`docs/research/source-coverage.md`](./docs/research/source-coverage.md), not by
a copied statistics block in this README.
