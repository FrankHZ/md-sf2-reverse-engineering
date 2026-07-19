# Source Coverage and Research Cadence

- Status: **Confirmed** for the pinned-source inventory and current evidence counters
- Evidence date: 2026-07-18
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
| Indexed findings | 159 | Confirmed symbol/table records in `manifests/research-index.json` |
| Indexed source files | 91 | 83 code files and 8 data files |
| Executable code-file reach | 21.45% | 83 indexed code files / 387 pinned code files; **not** line or function coverage |
| Indexed data-file reach | 0.47% | 8 indexed data files / 1,690; deliberately undercounts other H2 manifests |
| H3 fixture files | 54 | Runtime contracts, often containing multiple cases |
| Address bindings | 555 | Checked ROM/RAM relationships between fixtures and symbols/state |
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
  and between-battle healing contracts; upgrade/egress and visual sequencing remain partial.
- **Minimal or unindexed:** exploration/world state, event scripting, conversations, menus/UI,
  shops/church flows beyond selected consumers, save format, maps beyond the Battle 01 slice,
  graphics/audio engines, and most content tables.

Therefore 21.45% is the useful current code-file-reach snapshot, while whole-game semantic and remake
completion remain **Unknown**. Any later percentage must name its denominator and evidence level.

## Reproduction

The tracked evidence counters are reproduced by:

```powershell
uv run sf2 research-index list --summary
uv run sf2 research-index test
```

For the pinned checkout, `rg --files local/upstream/SF2DISASM/disasm/code -g '*.asm'` yields 387
files and the corresponding `data` query yields 1,690. The index summary reports 159 records; its
verifier reports 54 H3 fixtures and 555 bindings. The default `uv run sf2 verify` checks those
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
17-file static inventories respectively. The 18-file `battleloop` directory is now complete at the
inventory level as well, raising strict code-file reach by 14 files without adding emulator starts.
Its first lifecycle model covers roster scans, AI-memory reset, forced enemy deaths, between-battle
healing, terrain decompression, spawn admission, and killed-combatant cleanup. The battlefield slice models grid layout, initialization,
occupancy, neighbor admission, range rings, target admission, attack-position selection, move-string
backtracking, move orders, trapped chests, and weighted propagation. Its first concentrated runtime
batch confirms five movement/boundary cases in one launch. Remaining battle-AI signed-priority,
critical-class, dispatcher/standby questions and the attack-position comment conflict stay queued
until they form another coherent shared-boundary matrix; static work proceeds toward the remaining
15 unindexed `battleactions` files and then the smaller battle-function boundary.
