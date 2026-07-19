# Source Coverage and Research Cadence

- Status: **Confirmed** for the pinned-source inventory and current evidence counters
- Evidence date: 2026-07-19
- ROM: USA retail, SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`
- Source baseline: `ShiningForceCentral/SF2DISASM` commit
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## What “Covered” Means

There is no honest single percentage for the project yet. A file can contain many unrelated
functions, one H3 fixture can exercise several branches, and a parsed data table does not prove the
runtime semantics of every consumer. Report the evidence surface with explicit denominators instead
of treating fixture or address counts as line coverage.

The strictest current executable-code proxy is **indexed file reach**: a source file counts once when
at least one named symbol in it is connected through the research index to executable H2/H3 evidence.
It says that the file has been reached, not that every instruction in the file is understood.

| Metric | Current value | Meaning |
| --- | ---: | --- |
| Pinned ASM files | 2,106 | 387 under `disasm/code`, 1,690 under `disasm/data`, 29 root/support files |
| Indexed findings | 313 | Confirmed symbol/table records in `manifests/research-index.json` |
| Indexed source files | 245 | 237 code files and 8 data files |
| Executable code-file reach | 61.24% | 237 indexed code files / 387 pinned code files; **not** line or function coverage |
| Indexed data-file reach | 0.47% | 8 indexed data files / 1,690; deliberately undercounts other H2 manifests |
| H3 fixture files | 54 | Runtime contracts, often containing multiple cases |
| Address bindings | 709 | Checked ROM/RAM relationships between fixtures and symbols/state |
| H2 ROM table ranges | 14 | Deterministic source/ROM dual-path extraction ranges |

The H2 surface is much broader than the three indexed data files. It currently includes 281 fixed
ally/class/item/spell records, five 29-point growth curves, 59 class-growth records, 122 spell-learn
entries, five promotion sections, 103 enemy names, 103 enemy definitions, 30 enemy-drop entries,
103 used enemy-gold words plus the explicit 69-word unused tail, and the Battle 01 placement/scene
slice. These heterogeneous structures must not be added into a fake “records completed” percentage.

## Subsystem Boundary

The current evidence is deep but narrow:

- **Strongest:** reproducible ROM baseline; core stats/growth tables; physical combat, EXP/gold,
  many spell-resolution paths, and Battle 01 initialization/activation.
- **Partial:** battle AI has a complete source inventory plus static action-filter, attack-priority,
  healing/support decisions, final attack action/target selection, movement/move-order contracts,
  terrain obstruction/carving helpers, top-level commandsets, swarm activation, and special attackers,
  plus at least one indexed entry in every one of its 26 files; dispatcher/standby are parsed but
  deeper special-helper semantics remain open. Battlefield/pathfinding now has a complete 17-file
  inventory and representative address binding in every file; its 48×48 RAM grids, initialization,
  occupancy, movement-neighbor admission, range rings, target admission, attack-position selection,
  move-string backtracking, move-order, trapped-chest semantics, and 32-bucket weighted propagation
  are modeled. One five-case/one-launch H3 matrix confirms weighted propagation, budget-128 bucket
  wrap, controlled flat row crossing, and pre-check out-of-range helper entries. Other battle systems
  cover selected boundaries rather than every caller and state transition. The adjacent 18-file
  battle-loop directory now has complete static inventory plus roster, terrain, spawn, death cleanup,
  and between-battle healing contracts. All nine top-level battle-control files are inventoried too,
  with main-loop, victory/defeat, difficulty, spriteset, music/VInt, and laser-ray contracts;
  upgrade/egress, suspended persistence, table content, and visual sequencing remain partial. All 29
  battle-action files now have static/H3 reach, with engine sequencing, item break/use, Taros gating,
  and target sorting modeled. The seven shared battle-function files are now inventoried too, with
  individual-turn AI/player routing, Kiwi Flame Breath, exits, loading, and move SFX modeled;
  presentation, large cursor/input state machines, and remaining ailment subroutes are still partial.
  The twelve root battle-scene files and all 55 animation descendants are now separately inventoried,
  including the 21-command script interpreter, initialization/selectors, 32 setup/update pairs,
  shared setup files, reused update targets, and root-owned update targets. Frame/VDP behavior is
  still explicitly outside that static credit. All ten battle-cutscene routing files are now
  inventoried too, closing file-level reach for all 183 files under `code/gameflow/battle`; map-script
  content and story semantics are not implied by that boundary milestone. Common scripting now has
  a complete 29-file inventory, 28 H1-bound files, 90/80-slot map/entity dispatch tables, and text
  Huffman state; one unlabeled 288-byte data blob is H2-verified but excluded from symbol reach.
  Common maps now has a complete seven-file inventory covering switch/trigger/egress routing,
  8 KiB layout output shape, load ordering, and VInt gates; camera/VDP timing remains open.
  Common stats now inventories all 20 files and models flags, party/inventory services, spell
  learning, and new-game order. Seventeen have independent evidence; three unassembled alternate
  item sources are tracked but excluded from strict reach rather than borrowing their canonical twins.
- **Minimal or unindexed:** exploration/world state, individual event-script content, conversations, menus/UI,
  shops/church flows beyond selected consumers, save format, maps beyond the Battle 01 slice,
  graphics/audio engines, and most content tables.

Therefore 61.24% is the useful current code-file-reach snapshot, while whole-game semantic and remake
completion remain **Unknown**. Any later percentage must name its denominator and evidence level.

## Reproduction

The tracked evidence counters are reproduced by:

```powershell
uv run sf2 research-index list --summary
uv run sf2 research-index test
```

For the pinned checkout, `rg --files local/upstream/SF2DISASM/disasm/code -g '*.asm'` yields 387
files and the corresponding `data` query yields 1,690. The index summary reports 313 records; its
verifier reports 54 H3 fixtures and 709 bindings. The default `uv run sf2 verify` checks those
relationships on every ordinary commit.

## Static-First Cadence

Phase 2 now works in subsystem batches:

1. **Inventory first:** enumerate the subsystem's source files, public symbols, call edges, tables,
   constants, state reads/writes, and obvious unreachable or build-conditional paths.
2. **Parse and model:** turn stable tables and branch rules into Python-owned structured output and
   independent tests. Confirm source/ROM shape statically where possible; label runtime meaning
   `Inferred` when static evidence alone cannot prove it.
3. **Queue runtime questions:** keep only ambiguity involving timing, state persistence, caller
   context, signedness/overflow, RNG, undocumented hardware behavior, or conflicting source comments.
4. **Run one matrix:** group related questions into one generated input table and one BizHawk launch,
   write all observed results into a compact RAM/output buffer, and validate the batch after exit.
5. **Close the subsystem:** promote only the reproduced conclusions to `Confirmed`, document remaining
   unknowns, and update the research index/design contract together.

A one-case emulator fixture is now exceptional: use it only when the scenario cannot share setup or
observation points safely. The normal batch target is one launch for a coherent branch matrix, as
demonstrated by the eight-case muddled ally/enemy action guard and fourteen-case final action-choice
fixtures.

## Next Batch

The `battle.ai` and adjacent `battlefield/pathfinding` directories now have complete 26-file and
17-file static inventories respectively. The 18-file `battleloop` directory and nine top-level battle
control files are complete at the inventory level too, raising strict code-file reach by 23 files
across two static-only batches without adding emulator starts. Their models cover the main loop,
victory/defeat, roster scans, AI-memory reset, forced enemy deaths, between-battle healing, terrain
decompression, spawn admission, and killed-combatant cleanup. The battlefield slice models grid layout, initialization,
occupancy, neighbor admission, range rings, target admission, attack-position selection, move-string
backtracking, move orders, trapped chests, and weighted propagation. Its first concentrated runtime
batch confirms five movement/boundary cases in one launch. Remaining battle-AI signed-priority,
critical-class, dispatcher/standby questions and the attack-position comment conflict stay queued
until they form another coherent shared-boundary matrix. The 29-file battle-actions directory is now
fully reached without another emulator launch; the seven-file `battlefunctions` boundary is now also
reached. The twelve-file battle-scene root engine and all 55 animation descendants are now fully
reached without an emulator launch. Their 32 setup/update pairs are classified; frame timing and VDP
effects can now be run later as one grouped presentation matrix. The final ten cutscene-routing
files close the entire 183-file `code/gameflow/battle` tree at file-reach level, still without an
emulator launch. The 29-file common scripting boundary is now inventoried next, with 28 symbol-bound
files and one explicitly unlabelled byte range; it adds no emulator launch. Static work moves to
shared maps/stats boundaries while the runtime queue remains consolidated. The seven-file common
maps boundary is now complete at the static inventory level too. The 20-file stats boundary followed
and is now inventoried as well, with its alternate-source exception explicit;
the next static batch moves to menus/technical services while runtime queues remain grouped.
