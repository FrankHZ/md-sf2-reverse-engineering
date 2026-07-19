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
| Indexed findings | 145 | Confirmed symbol/table records in `manifests/research-index.json` |
| Indexed source files | 77 | 69 code files and 8 data files |
| Executable code-file reach | 17.83% | 69 indexed code files / 387 pinned code files; **not** line or function coverage |
| Indexed data-file reach | 0.47% | 8 indexed data files / 1,690; deliberately undercounts other H2 manifests |
| H3 fixture files | 53 | Runtime contracts, often containing multiple cases |
| Address bindings | 525 | Checked ROM/RAM relationships between fixtures and symbols/state |
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
  move-string backtracking, move-order, and trapped-chest semantics are modeled, while full weighted
  propagation remains shallow. Other battle systems cover selected
  boundaries rather than every caller and state transition.
- **Minimal or unindexed:** exploration/world state, event scripting, conversations, menus/UI,
  shops/church flows beyond selected consumers, save format, maps beyond the Battle 01 slice,
  graphics/audio engines, and most content tables.

Therefore 17.83% is the useful current code-file-reach snapshot, while whole-game semantic and remake
completion remain **Unknown**. Any later percentage must name its denominator and evidence level.

## Reproduction

The tracked evidence counters are reproduced by:

```powershell
uv run sf2 research-index list --summary
uv run sf2 research-index test
```

For the pinned checkout, `rg --files local/upstream/SF2DISASM/disasm/code -g '*.asm'` yields 387
files and the corresponding `data` query yields 1,690. The index summary reports 145 records; its
verifier reports 53 H3 fixtures and 525 bindings. The default `uv run sf2 verify` checks those
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

The `battle.ai` inventory, five action filters, potential-damage model, attack priority scripts,
healing, support, final action choice, movement, dispatcher, and standby commands are now parsed, and
all 26 battle-AI files have an H2 foothold. The movement-helper batch additionally parses all eight
terrain helpers and five Manhattan-ring tables; the control batch parses 16 commandsets, 18
pathfinding entries, three swarm battles and thresholds, activation gates, line attackers, and the
Burst Rock exploder, plus all five explicitly unused MP/slot helpers. The first concentrated H3
matrix confirms final action/target choice in one launch. Standby memory now also has both movement
tables, its full 16-configuration eligibility matrix, and one-past-map precheck recorded. Static work
next moves across the boundary into battlefield/pathfinding, while the remaining signed-priority,
critical-class, movement, dispatcher, and standby ambiguities accumulate for the next small set of
shared-boundary launches. The adjacent battlefield/pathfinding directory now also has all 17 files
inventoried (2,299 lines, 126 global labels, 116 direct call sites), with grid layout,
initialization, occupancy, neighbor admission, range rings, target admission, attack-position, and
move-string backtracking, move-order, and trapped-chest handling modeled. The next static batch
finishes weighted propagation/tie-break behavior before
any new runtime launch.
