# Shining Force II Reverse Engineering & Remake

This repository studies the USA Mega Drive/Genesis release of **Shining Force II**, turns observed
original behavior into reproducible research and implementation-neutral contracts, and uses those
contracts as the basis for an independently maintained remake.

The repository is the durable project record. It must be possible to resume work without an old chat
or an external memory store. Findings, open questions, reproduction commands, design boundaries, and
project decisions belong in their owning tracked documents.

## Current Status

Three responsibilities run alongside each other; the historical phase numbers in ADRs are not a
sequential delivery plan.

| Responsibility | Current accepted boundary | Read next |
| --- | --- | --- |
| Original rebuild and research | The pinned disassembly rebuilds the private USA ROM byte-for-byte. Static-first research and bounded original runtime observations retain their own provenance and Unknowns. | [Reproducible original](./docs/research/reproducible-original.md), [research coverage](./docs/research/source-coverage.md) |
| Design explanation in English and Chinese | The indexed design corpus has complete, independently accepted Chinese mirrors: gameplay themes, detailed chapter guides and evidence-bound contracts. Selected static story connections reach ending/reset; this is neither a naturally played campaign nor game-script localization. | [English reading routes](./docs/README.md#design), [中文阅读入口](./docs/README.zh-CN.md) |
| State/content-driven remake | Godot 4.7.2 .NET/C# hosts a common session with Domain rules, Application programs and validated Content. Authored packages and bounded private-local content are runnable. | [Play and develop](./remake/README.md), [capabilities and limits](./remake/docs/capability-status.md) |

The private continuous Map 3 → Battle01 victory and usable-return milestone is accepted at the
[current composed boundary](./docs/design/synthesis/map3-battle01-readiness.md#accepted-current-milestone).
Accepted extensions cover Battle02 victory/defeat, field return and re-entry, plus selected early Yeel guidance,
Kazin following and Battle03 play/control. **Battle03 outcomes, Hawel recruitment, later campaign,
full services and save/load are not delivered by those extensions.**

Current presentation includes continuous exploration with contextual Talk, a shared 1920×1080 world
with 96px cells, blue/gold story dialogue, reachable battle tiles, image-based HP/MP cards, battle
messages, return feedback and a read-only Party Status page. The
[presentation owner](./remake/docs/godot/presentation.md) records the accepted presentation scope,
modern deviations and observation limits. Broader art/UI/UX remains under
[#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438); bounded engineering acceptance
does not claim final aesthetic or complete product acceptance.

[ADR 0019](./docs/decisions/0019-state-and-content-driven-remake-engine.md) owns the adopted engine
direction and behavior-test policy. Production is independent of verification; the transitional
reference implementation is retired and only [controlled comparison inputs](./remake/reference/README.md)
remain. Full-game play, hardware-exact parity and distribution of original assets are outside current
acceptance. Public builds require project-authored or properly licensed content.

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

| Your goal | Start with |
| --- | --- |
| Understand gameplay and design | [Gameplay overview](./docs/design/synthesis/gameplay-overview.md) → [themes and chapter guides](./docs/README.md#design) |
| 用中文阅读玩法、章节和合同 | [中文文档目录](./docs/README.zh-CN.md) → [玩法总览](./docs/design/zh-CN/synthesis/gameplay-overview.md) |
| Run or develop the current remake | [Remake README](./remake/README.md#run-locally) → [capability status](./remake/docs/capability-status.md) |
| Verify an original-game claim | [Research routes](./docs/README.md#research) → the named source/fixture and owning command |
| Resume repository work | Apply [AGENTS](./AGENTS.md) once, inspect live Git/worktree state, then follow [agent resume](./docs/operations/agent-resume.md) to the closest owner |

English is canonical for design review. Chinese design mirrors follow the same relative hierarchy
under `docs/design/zh-CN/` and the [glossary](./docs/design/glossary.md); research, ADRs and engine
usage retain their English owners. The [documentation roadmap](./docs/design/documentation-roadmap.md)
owns design scope and synchronization. Existing rule/flow explanations are delivered; the
[lightweight battle-design analysis pilot #1402](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1402)
is separate future work.

## Project Goals

The project is building a repeatable path from private original input to independently maintained
outputs:

1. identify, split, and rebuild the original ROM reproducibly;
2. document code, data formats, state machines, and runtime behavior with provenance;
3. export canonical structured data under explicit schemas;
4. reconstruct implementation-neutral game-design rules from accepted evidence;
5. compare admitted remake behavior against controlled original references;
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
| H4 | Remake comparison | Admitted implementation behavior is compared at named reference boundaries; deviations and unavailable observations remain explicit. |
| H5 | Distribution boundary | Published outputs contain no ROM, extracted copyrighted assets, or unreviewed third-party code. |

A static reference proves a relationship, not normal-play reachability. A handler-local runtime fixture
proves its observation seam, not complete caller behavior or presentation. When owners disagree, keep
the disagreement visible and design a focused test.

## Pinned Original Baseline

The canonical private input is the registered USA retail ROM, selected read-only through ignored
[local configuration](./docs/operations/local-private-inputs.md). Its stable
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
[the remake architecture route](./remake/docs/architecture.md), with separate Domain, Application,
Content, Godot and verification owners; new paths are added only when a concrete responsibility owns them.

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

Main-gate dispatches authorized, bounded Issues into fresh execution tasks under
[Project governance](./docs/operations/github-project-governance.md#task-lifecycle). Keep corrections
to the same outcome in that task. Roles and reusable environments may outlive individual tasks.

`main` is the serialized integration branch. Start a short-lived topic from accepted `origin/main` in
an available dedicated isolated worktree after its old topic is merged, tracked state is clean,
owned processes are settled and ownership is transferred. Create another worktree only for concurrent
writers or a concrete reproduction/isolation need:

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
contract. Distributable remake builds must use project-authored placeholders or properly licensed assets.

## Roadmap

Current work is selected through [Issues and the Project](./docs/operations/github-project-governance.md),
with independent acceptance for each bounded outcome:

- [Research coverage](./docs/research/source-coverage.md) owns evidence gaps and cadence;
  [story coverage](./docs/research/story-content-coverage.md#four-different-frontiers) separates static,
  observed, design and playable boundaries.
- [Design documentation](./docs/design/documentation-roadmap.md) owns the accepted bilingual corpus
  and continuing synchronization. Wider analysis is separately scoped, including #1402 above.
- [Remake capabilities](./remake/docs/capability-status.md) own current gameplay and Unsupported
  behavior. [#1370](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1370) tracks the original
  six special-mechanic families; [#1369](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1369)
  tracks wider engine extensions. Neither epic implies whole-game support.
- [Presentation](./remake/docs/godot/presentation.md) owns implemented interface choices and the
  remaining #438 work. The [H4 refactor #638](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/638)
  remains paused.

Historical phases, proposals and superseded decisions stay in [ADRs](./docs/README.md#decisions)
and Git. New content, service, save, localization or release work needs its own accepted scope.
