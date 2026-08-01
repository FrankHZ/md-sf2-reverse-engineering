# Agent Guide

## Mission

This repository studies the US Mega Drive/Genesis release of **Shining Force II**,
documents its systems and data, and uses that evidence to build an independently
maintained remake in a modern engine.

This is an engineering and preservation project, not a collection of ad-hoc ROM
experiments. Every useful discovery should become one or more of:

- a cited research note;
- a machine-readable data contract;
- a deterministic extractor or verifier;
- a behavioral fixture;
- an implementation-neutral design rule.

## Read First

1. Read `README.md` for scope, current evidence, phases, and repository layout.
2. Inspect the real workspace before assuming a tool, directory, or generated
   artifact exists.
3. Keep changes within the current phase. Do not jump into engine code while the
   relevant original behavior and data contract are still undefined.

External agent memory is not a project dependency or source of truth. To resume work, read the
root `README.md`, `docs/research/source-coverage.md`, `docs/README.md`, and the closest owning topic
document, then inspect `git status` and recent commits. The repository must contain every durable
decision, current frontier, evidence counter, reproduction command, and unresolved question needed
to continue. Do not leave an important project fact only in a chat summary, local agent memory, or
an ignored generated report.

External agent memory is disabled for this repository. Do not consult or update a personal/global
memory store for routine work. If the user explicitly requests a one-time migration audit, copy only
still-valid, project-specific facts into their owning tracked documents; do not make the memory store
a continuing synchronization target.

## Parallel Worktree and Topic-Branch Contract

`main` is the integration branch, not an ordinary agent worktree. New research, design synthesis,
tooling, and governance slices use short-lived topic branches created from an up-to-date `origin/main`
and an isolated Git worktree. Use the `codex/research-*`, `codex/design-*`, `codex/tooling-*`, or
`codex/repo-*` prefix that matches the lane. A worktree may be reused after a branch is merged, but do
not turn a topic branch into a permanent parallel source of truth.

Do not run parallel writers in one worktree. Parallel work is allowed across isolated worktrees only
when each slice declares its owning files, shared-file needs, acceptance commands, and dependencies.
The default capacity is one active Phase 2 research lane and one active design-synthesis lane. Starting
multiple research write lanes requires explicitly disjoint parser, fixture, schema, manifest, index, and
documentation ownership; superficial subsystem separation is insufficient when the branches still edit
the same aggregate contract.

Topic branches consume accepted `main` state. A branch that must consume an unmerged branch declares a
stacked dependency and merge order; otherwise do not cite or promote unmerged findings as repository
facts. Files that commonly require an explicit single owner include `README.md`, `docs/README.md`,
`docs/research/source-coverage.md`, `manifests/research-index.json`, `src/sf2tool/cli.py`,
`src/sf2tool/design_contracts.py`, shared schemas/fixtures, and this file.

Integration is serialized. Before a branch is accepted, update it onto current `main`, resolve semantic
conflicts with the owning lane, rerun its lane gates plus `uv run sf2 verify`, scan the tracked/private
boundary, stage exact paths, and review the cached diff. Commit and push the accepted topic branch, then
merge it through the integration queue. An unmerged branch or remote worktree is visible collaboration
state, not the durable project source of truth.

Keep ignored runtime scratch isolated per worktree. Private immutable inputs may be copied and
hash-verified per worktree or exposed through narrowly scoped read-only paths, but do not share an entire
`local/` root when concurrent tools could write derived ROMs, traces, emulator state, or reports into it.

## Root/Worker Orchestration Contract

For ordinary Phase 2 work in the active research topic worktree, the root thread scopes one coherent
slice and its acceptance commands but does not personally reverse engineer or implement that slice. It starts exactly one
`terra_reverse_engineer` worker and gives it the owning document, bounded source surface, expected
tracked outputs, and one narrow H2/H3 acceptance command. If the named role is unavailable, the root
must explicitly spawn `gpt-5.6-terra`. Do not run parallel write workers in the slice worktree.

The worker performs the slice: complete static inventory, structured parser/contract, project-owned
tests and research documentation, and a grouped H3 question queue. It must preserve evidence labels
and provenance, avoid all project-direction decisions, and hand the completed work back without
staging or committing. Questions, incomplete evidence, and review findings go back to the same worker
through a follow-up rather than causing the root to take over reverse engineering.

Before handoff, the worker performs an adversarial acceptance pass. Extractor output, golden fixture,
both output and fixture schemas, focused tests, research prose, and any design contract must describe the
same complete shape. New nested schema objects use exact required fields and
`additionalProperties: false`; known names, counts, values, and array order are constrained exactly rather
than only by broad types or property counts. Focused tests assert the whole new semantic object and its
boundaries. Each Confirmed control-flow claim is guarded within the smallest stable named function or
section, including relevant branch polarity and mutation/call order. Caller inventories use parsed call
instructions and retain target identity and per-target site counts. Stored byte counts, address spans,
encoded sizes, transfer sizes, and loop counters remain separately named and documented. The handoff lists
the weaknesses found and corrected during this self-review. New or materially revised Evidence dates use
the actual current project date from the execution environment. Canonical names remain source-faithful or
neutral until their semantics are proven. Extractors parse authoritative constants once and derive masks,
strides, spans, widths, and counts from them instead of duplicating magic values in implementation guards;
golden fixtures and strict schemas still pin the resulting exact values. Boolean arithmetic, container or
string cardinality, and arithmetic identities do not qualify as derivation. Both schemas close every nested
object recursively and focused mutation tests reject missing, renamed, extra, reordered, and out-of-bound
content. Changed parsers cover positive, negative, boundary, comment, and legal instruction-suffix cases.
Reported semantic summaries—constants, masks, offsets, widths, selector scales, capacities, branches, call
orders, and caller totals—must identify the specific parsed use-site record/table/operand that expresses
the relationship, resolve symbols through the one parsed constants map where applicable, and validate
identity/order/polarity/width there. Independently parsing a constant and an instruction corpus is not a
derivation. A smallest-scope source mutation of that use-site operand/opcode/order must make parser
construction fail before golden-fixture comparison; fixture/schema exactness is not a derivation guard.
Caller effective-target total maps are zero-inclusive across the complete declared target set for both
internal and external inventories, not only positive occurrences. The worker's pre-handoff
summary-provenance audit asks whether every reported derived field has both a parsed use-site link and a
mutation capable of falsifying it.
Workers do not use file-wide lint suppression or include unrelated formatting/generated churn. When chat
context is compact, they recover from the repository, current diff, and explicit slice contract before
asking the root a precise blocking question. Large exact corpora use one closed reusable record schema plus
a compact exact-order constraint instead of per-record schema expansion; generated JSON uses a real JSON
serializer and is parsed before gates. Caller audits resolve jump-interface aliases while retaining both
instruction and effective target identities. A worker begins implementation in its first assigned turn;
capacity checkpoints must contain a tested bounded change and are never presented as completed handoffs.

The root accepts the slice only after it reviews the worker handoff, changed-file list, diff, evidence,
and counters; reruns the owning narrow command plus `uv run sf2 verify`; scans for private/generated
inputs and unintended changes; stages only the accepted paths; reviews the cached diff; and commits to
the current research topic branch, never directly to `main`.
`uv run sf2 verify --full` remains a milestone, release/merge-readiness, shared-harness, or explicit
full-parity gate, never the default worker or root command. This workflow is an operational division of
responsibility, not a security boundary: worker instructions and root review are both required.

Within the accepted Phase 2 direction, continue autonomously through the root/worker workflow: the
root scopes, accepts, scans, and commits on the research topic branch; the worker performs the assigned reverse engineering or
implementation, documentation, and slice-local narrow checks. Do not pause for approval or produce a
user report after every ordinary slice. Ask the user only before a phase change, modern-engine choice,
new distribution/licensing posture, destructive treatment of private inputs, or another decision that
materially changes project direction. Preserve unrelated or unfinished work already present in the
worktree, and have the root stage only files owned by the accepted slice. After acceptance, the root may
push the topic branch and open or update its pull request; final integration remains serialized under the
parallel-worktree contract above.

When more documents exist, use these ownership boundaries:

- `docs/research/`: source-backed reverse-engineering findings.
- `docs/design/`: implementation-neutral game design reconstructed from evidence.
- `docs/decisions/`: durable architecture and tool decisions.
- `schemas/`: canonical extracted-data contracts.
- `src/sf2tool/`: maintained Python CLI, extractors, verifiers, and harness code.
- `tools/`: repeatable inspection, extraction, conversion, and validation code.
- `tests/python/`: project-owned Python unit and contract tests.
- `tests/fixtures/`: small redistributable metadata and behavioral expectations.
- `remake/`: modern-engine implementation after its contracts are accepted.

Evidence-bound subsystem documents in `docs/design/` remain part of their research slice when accepted
findings change. Cross-subsystem or player-facing synthesis documents use the design-synthesis lane and
may explain only accepted evidence from `main`; that lane does not edit `docs/research/`, schemas,
fixtures, manifests, extractors, or evidence-bound design contracts unless a separate research slice and
merge dependency explicitly assigns those files. Shared indexes and registries receive one branch owner
per change.

Do not create empty scaffolding for these paths. Add a directory when a concrete
slice owns content in it.

## Evidence Rules

Label reverse-engineering statements as one of:

- **Confirmed**: reproduced by a script/test, or directly supported by named
  disassembly locations and observed runtime behavior.
- **Inferred**: the evidence is strong but not yet reproduced independently.
- **Unknown**: an explicit open question, never silently filled with an assumption.

For each non-trivial finding, record enough provenance to reproduce it: ROM hash,
upstream repository and commit, branch, file/symbol or ROM address, tool version,
command, and observed result as applicable. Prefer stable source links over forum
paraphrases. When sources disagree, keep the disagreement visible and design a
test instead of choosing the convenient answer.

Never claim strict clean-room development if the implementer has inspected the
disassembly. The practical boundary here is provenance plus separation: research
describes behavior and contracts; remake code should consume those contracts and
use project-owned or properly licensed assets.

## ROM, Patch, and Copyright Boundary

The user-provided ROM and any user-provided patches are local research inputs.
Treat all of the following as private/generated by default:

- ROM images, patched or rebuilt ROMs;
- SRAM, save states, traces, emulator movies, and memory dumps;
- extracted dialogue, graphics, maps, music, sound effects, and other game assets;
- downloaded executable tools;
- pre-patched downloads.

Do not commit, upload, attach, or redistribute them. Before initializing Git, add
ignore rules covering at least `*.bin`, `*.gen`, `*.smd`, `*.ips`, `*.bps`,
`*.ups`, `*.xdelta*`, save-state formats, and the local work directory. Mega Drive
ROMs sometimes use `.md`, but never add a global `*.md` rule because it would also
hide Markdown documentation; keep those ROMs under the ignored local work root.
Inspect staged files before every commit.

A patch file may be tracked only after its redistribution terms and absence of
copyrighted payload have been reviewed. Do not download a pre-patched ROM. When a
patch is needed for comparative research, prefer the patch-only artifact, record
its source and expected base hash, and keep it local unless its license allows
redistribution.

Third-party source may be vendored only with a compatible explicit license,
provenance, and pinned revision. A public GitHub repository is not by itself
permission to copy or relicense its contents. When licensing is missing or
unclear, link to it and use a pinned local checkout rather than copying it here.
This guidance is project hygiene, not legal advice.

## Upstream Baselines

The primary community reference is
[`ShiningForceCentral/SF2DISASM`](https://github.com/ShiningForceCentral/SF2DISASM).
Use its branches deliberately:

- `master`: original-game documentation and the bit-perfect reconstruction
  baseline.
- `build/standard`: a maintained combination of community features and fixes;
  useful as comparative research, not as the original baseline.

The repository's default branch may not be `master`. Always record and pin the
exact commit instead of relying on a floating default branch. Do not modify the
only local ROM to satisfy an upstream filename convention; copy it into an ignored
workspace and name that copy `sf2.bin`.

Review tool source and license before use. Prefer source builds or well-known
package sources over opaque binaries. Download only the minimum needed for the
active slice.

## Python and Harness Contract

Python 3.12+ is the maintained tooling language and `uv` owns dependency resolution, locking, and
execution. Bootstrap with `uv sync --locked`; never install project dependencies into the system
interpreter or maintain a parallel requirements file. Run Ruff and pytest through `uv run`.

The root verification entry point is `uv run sf2 verify`. Keep it non-interactive, deterministic,
and safe to rerun. It currently implements design-contract
traceability, input/toolchain provenance, original rebuild, source/ROM static parity, ally-growth,
promotion/enemy/enemy-gold/enemy-drop, Battle 01 scene extraction, and the complete battle-AI source
inventory/action-filter/attack-priority contracts, the complete Stack-compressed battle-terrain,
battle-background, battle-sprite, weapon/ground, and portrait corpora, the complete 208-table
battle-sprite animation sequence corpus, the complete Basic-compressed
map-sprite corpus, the complete Stack-compressed special-sprite corpus/routing boundary, plus
the complete special-screen Stack-compressed tile corpus/transfer boundaries, plus
the complete witch-menu choice palette, bubble-animation table, and timer-phase corpus, plus
the complete uncompressed special-screen palette/layout presentation corpus, plus
the complete base/diamond-menu/yes-no Stack-compressed UI corpus and seven-icon uncompressed main-menu payload, plus
the complete 163-entry icon storage corpus and menu copy/highlight boundaries, plus
the complete assembled UI/window layout, spell-pointer, border, and direct-asset corpus, plus
the complete variable-width font, ASCII-map, pointer, and loader corpus, plus
the complete 255-entry context-Huffman offset table, 86 reachable trees, and 1,536 leaf-code corpus, plus
the complete 17-bank, 4,267-string static Huffman decode corpus, plus
the complete nine-range shop/debug-shop/chest/break/mithril/Caravan/field-item/weapon-graphics corpus, plus
the complete 166-row enemy map-sprite table and normal-vs-NPC-tail reachability boundary, plus
the complete original built map-sprite assignment domains and reserved-ID exclusion audit, plus
the complete two-bank/37-song Z80 music source/range/pointer/ROM, 29-macro command corpus, and
39-header/five-parser/shared-loop/ten-slot channel-role/64-command bank-selection static driver
contract and the 84-entry YM/64-entry PSG frequency tables with complete note/shift CFG audit, plus
the 17-entry DAC load table with complete music sample-call audit, plus
the complete YM/PSG instrument-index and level-call domain, plus
the complete map-script dispatcher/macro/handler/source-use inventory, plus
the complete shared battle player-input/cursor function inventory and static menu/item/chest contract, plus
the complete shared/distributed entity-action command/control-flow/reference corpus,
80-slot dispatcher/macro/handler access/state/flow and parameter-ABI inventory, and the nine-phase
`UpdateEntityData` movement core plus its destination/sprite/map-offset helper boundary, plus
the complete 119-row map-sprite/portrait/speech-SFX dialogue-property table and consumer contract, plus
the complete four-stream unused-cloud payload and two-palette unused-base corpus, plus
the complete spell/invocation/status/transition battle-effect graphics corpus, plus
the complete 115-stream map-tileset corpus and map/animation usage boundary, plus
the complete 16-entry map-palette corpus, 79-map usage table, and effective color-zero boundary, plus
the complete direct named Basic/Stack/compressed-DMA consumer inventory, plus
BizHawk base/debug-aware RNG,
stat-gain/complete level-up/stat-clamp/enemy-curse boundaries, battle-EXP level-up, kill-EXP level differences,
final EXP halving/randomization/minimum, EXP-command clamp/threshold, gold cap/carry, enemy-item-drop behavior,
   turn-order, region-activation, physical-attack-chain, dodge, follow-up-validation, and grouped
   map-animation VInt/DMA/VRAM behavior, the four-command Z80 music/live-channel-state matrix,
   and the 13-case/20-tick entity movement/action matrix fixtures, plus the ten-case map setup
   selector and six-case map init dispatch runtime matrices;
extend the same entry
point as later rails become available:

1. **Input identity**: size, hashes, console header, product code, region, and ROM
   checksum.
2. **Original rebuild**: split and assemble with a pinned `SF2DISASM master`, then
   byte-compare the result with the input ROM.
3. **Extraction determinism**: validate schemas and prove identical canonical
   output from repeated runs.
4. **Behavioral replay**: run scripted scenarios in a pinned emulator and compare
   small state facts, traces, or hashes rather than committing captured assets.
5. **Remake parity**: run the same implementation-neutral fixtures against remake
   systems and report intentional deviations separately.

Use `uv run sf2 verify` as the normal commit gate. It owns a full Ruff scan, the shared critical
`tests/python/test_native_harness.py` suite, design-contract traceability, the research index, ROM
identity, and toolchain provenance; it is not broad Python regression. Pair it with only the narrow
H2/H3 command that owns the changed slice (for example, `uv run sf2 h2 map-setup` or
`uv run sf2 h3 battle-exp`). Run the complete Python suite explicitly with `uv run pytest`. Do not
run the 10+ minute `uv run sf2 verify --full` after every ordinary commit. That full profile runs the
complete Python suite followed by H1/H2/H3 and is reserved for phase milestones, release/merge
readiness, changes to shared harness orchestration or legacy rails, and explicit full-parity requests.

Phase 2 research is static-first and subsystem-batched. Inventory the complete source scope, parse
stable tables and control-flow rules, and record a runtime-question queue before creating H3 work.
Static source/ROM shape and canonical map imports may be confirmed by deterministic parsers;
caller-dependent behavioral
meaning stays inferred until observed. Group related runtime questions into one generated case table
and one BizHawk launch whenever setup and observation points can be shared. Do not add a one-case
emulator fixture without a concrete isolation reason. The detailed cadence and coverage denominator
are owned by `docs/research/source-coverage.md` and ADR 0003.

Do not weaken a golden expectation merely to make a change pass. First decide
whether the original evidence, extractor, fixture, or remake behavior is wrong.
If docs and executable evidence diverge, investigate the mismatch and update the
owning document in the same change.

Every tool should support a read-only or output-directory workflow. Never patch
the canonical input in place. Generated outputs go under an ignored local work
root and must be reproducible from documented inputs.

The remaining scripts under `scripts/*.ps1` are a temporary, frozen compatibility surface for the
already-proven H1-H3 rails. Python invokes them only through `sf2tool.legacy`; do not add new
PowerShell scripts or grow the aggregate PowerShell line budget. Migrate a complete rail to
`src/sf2tool/`, verify parity, then delete its old script. Do not build new logic in the adapter.

## Change Discipline

- Make narrow, reviewable changes with one clear owner and acceptance test.
- Search for an existing parser, schema, note, or fixture before adding another.
- Keep generic Mega Drive facts separate from SF2-specific claims.
- Prefer structured output plus a thin renderer over one-off reports.
- Record meaningful tool and engine choices in `docs/decisions/` before coupling
  the project to them.
- Keep original fidelity facts separate from remake design choices. A deliberate
  modernization is not evidence about the original game.
- Use placeholder or properly licensed assets for distributable remake builds.
- Do not report a subsystem as documented until its unknowns and verification
  coverage are explicit.

On Windows, maintained Python code must launch native tools with `subprocess` argument lists and no
shell. The frozen PowerShell layer retains the repository's existing native-command rules until
each rail is migrated. Keep text files UTF-8 and pass paths directly instead of piping non-ASCII
JSON between runtimes.

## Definition of Done

A slice is done when its outputs are reproducible, provenance is recorded,
relevant docs and contracts agree, generated/private artifacts remain untracked,
the topic branch is updated onto current `main`, and `uv run sf2 verify` plus the owning narrow
verification command pass. A design-synthesis-only slice uses `uv run sf2 design-contracts test` and
the public tracked-input gate before the final integration `uv run sf2 verify`. If
verification cannot run, report the exact missing dependency or evidence instead
of substituting confidence.
