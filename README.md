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
- **Remake implementation:** not started. The accepted first playable milestone is one continuous
  scenario from Map 3 through completion of Battle 01 under
  [ADR 0009](./docs/decisions/0009-first-phase4-playable-slice.md). Research and Design must first
  complete targeted gap audits, close the required evidence and contract gaps, and receive a
  main-gate readiness report; Phase 4 still requires a separate explicit start action. No
  distributable asset strategy or MCP implementation has been selected.

This README intentionally does **not** maintain fixture totals, address counts, coverage percentages,
or per-subsystem corpus sizes. Those snapshots became stale as soon as another research slice merged.
Use the executable sources of truth instead:

- [`docs/research/source-coverage.md`](./docs/research/source-coverage.md) defines coverage
  denominators, evidence cadence, and the current research frontier.
- [`manifests/research-index.json`](./manifests/research-index.json) owns indexed findings and their
  source, address, fixture, and document relationships.
- [`docs/README.md`](./docs/README.md) routes readers to the closest research, design, or decision
  owner.
- `uv run sf2 research-index test` and `uv run sf2 verify` reproduce current tracked counters and
  validate their relationships.

## Start Here

For any new task:

1. Read [`AGENTS.md`](./AGENTS.md) for repository workflow, evidence rules, private-input handling,
   and the definition of done.
2. Read [`docs/research/source-coverage.md`](./docs/research/source-coverage.md) for the current phase
   boundary and research cadence.
3. Use [`docs/README.md`](./docs/README.md) to find the nearest owning topic document.
4. Inspect `git status`, active worktrees, and recent commits before assuming a branch or slice is
   complete.
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
local/                  Ignored ROMs, saves, traces, upstream checkouts, and generated output
```

The planned `remake/` path does not exist yet. It will be created only after the relevant contracts
and engine decision are accepted; the project does not create empty scaffolding for future phases.

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

The normal commit gate, `uv run sf2 verify`, runs the shared critical Python suite, design-contract
traceability, research-index validation, ROM identity, and toolchain provenance. Pair it with the one
focused H2 or H3 command that owns the changed slice.

Useful explicit commands include:

```powershell
uv run ruff check src tests/python
uv run pytest tests/python/test_native_harness.py
uv run pytest
uv run sf2 design-contracts test
uv run sf2 research-index test
uv run sf2 verify plan --base origin/main --head HEAD
uv run sf2 texture extract
uv run sf2 texture map --maps 3
uv run sf2 verify --full
```

`uv run sf2 texture extract` decodes the accepted map-texture corpus (115 Stack-compressed
`MapTilesetNNN` streams of 128 Mega Drive 4bpp 8x8 tiles each, plus the 16 `MapPaletteNN`
9-bit color records) into private PNG sheets under ignored `local/derived/graphics/`:
16x8 tile grids rendered with map palette 0 for direct viewing, plus all 16 palettes as
color strips. `uv run sf2 texture map --maps 3` renders real map main-layer regions
(64x64 block layout, 3x3-tile blocks, five tileset slots, flip/mirror flags) with the
map palette. Palette index 0 is transparent; equal RGB black at any nonzero index remains
opaque. The channel assignment is a tooling interpretation pending a reproducible original-game
observation, not a screenshot-verified claim. Every area of every requested map is emitted under
`local/derived/graphics/maps/mapNN/` (all 135 areas across the 79 maps are enumerable
from each `2-areas.asm`), plus per-area `...-overlay.png` (the second-layer region at the
`scndLayerFgndStart`/`scndLayerBgndStart` offset, with the roof/layer-2 `slbc` copy records
applied so source-stored facades such as the map-3 cell bars appear at their display
position) and `...-composed.png` (overlay over the main layer). `uv run sf2 texture ui`
renders a private source-shaped battle action-menu diagnostic under
`local/derived/graphics/windows/`: the source/H1/ROM-bound 18x6
`layout_DiamondMenu` grid with four source-shaped icon-slot builds and the selected option name
at (11,4). This is not an original-screen, palette-appearance, animation-timing, or screenshot
parity result, and it does not reconstruct the item-menu window. `uv run sf2 texture assets` extracts the font sheet,
portraits (8x8 tile grid + per-portrait palette), icons (2x3 tiles, base palette), and
map sprites (two 3x3 column-major frames per facing). `uv run sf2 texture misc` extracts
the UI tile sets and main-menu icons, special sprites (raw tile sheets; palette decode and
frame assembly still unconfirmed), diagnostic battle-background compositions (source layout table,
32 columns; final screen parity unconfirmed), and all four accepted unused-cloud streams under both
unused base palettes. The generated PNGs are private/generated graphics payloads and are never
tracked; manifests hold metadata and hashes only. Every ignored upstream binary payload is checked
against its exact H1-resolved range in the hash-verified ROM before rendering. Commands reject their
own existing output directories/manifests instead of silently overwriting or retaining stale files.

`verify plan` compares a base revision with a clean checked-out committed head without running a gate
or changing Git state. An explicit `--head` is accepted only when it resolves to the checked-out
`HEAD`; tracked changes and non-ignored untracked files are rejected because artifact and import
ownership are derived from the checked-out filesystem. The plan always includes the normal public
core, then reports the affected Python/H1/H2/H3 partitions, exact reasons, suggested narrow commands,
resource locks, and any conservatively fanned-out unclassified paths. Use `--include-partition <id>`
when a semantic dependency is not visible from the path diff. Planner mode rejects execution
modifiers such as `--full`, `--skip-runtime`, or non-default ROM/upstream paths instead of silently
ignoring them.

`verify --full` runs the complete Python suite plus the maintained H1/H2/H3 milestone profile wired
into the current harness. It is reserved for milestones, release/merge readiness, shared harness
changes, or explicit full-parity requests; it is not an enumeration of every registered narrow CLI
command. It is not the default gate for an ordinary research slice, and a design-only documentation
change does not trigger it.

Generated outputs belong under ignored `local/` paths. Tools must support read-only input or an
explicit output directory; they must never patch the canonical ROM in place.

## Branch, Worktree, and Review Workflow

`main` is the serialized integration branch. New work uses a short-lived topic branch in an isolated
worktree:

- `codex/research-*` for Phase 2 evidence, parsers, fixtures, and owning research docs;
- `codex/design-*` for cross-subsystem design synthesis from accepted `main` evidence;
- `codex/tooling-*` for maintained tooling changes;
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
  Map 3-through-Battle 01-completion milestone are accepted; Research/Design gap audits, main-gate
  readiness, and a separate explicit start action remain pending — implementation not started.
- **Phase 5 — Content and Productization:** licensed/placeholder assets, localization, accessibility,
   distribution, and release QA — not started.

Near-term design direction, evidence prerequisites, and stop conditions are owned by
[`docs/design/documentation-roadmap.md`](./docs/design/documentation-roadmap.md). Current research
direction is owned by [`docs/research/source-coverage.md`](./docs/research/source-coverage.md), not by
a copied statistics block in this README.
