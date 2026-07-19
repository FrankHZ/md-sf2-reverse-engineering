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
| Indexed findings | 1,444 | Confirmed symbol/table records in `manifests/research-index.json` |
| Indexed source files | 1,361 | 381 code files and 980 data files |
| Executable code-file reach | 98.45% | 381 indexed code files / 387 pinned code files; **not** line or function coverage |
| H2 data-ASM inventory | 100.00% | 1,690 / 1,690 pinned data ASM files belong to deterministic inventory rails |
| Indexed data-file reach | 57.99% | 980 indexed data files / 1,690; deliberately undercounts other H2 manifests |
| H2 fixture files | 49 | Deterministic source/ROM contracts, often covering complete corpora |
| H3 fixture files | 58 | Runtime contracts, often containing multiple cases |
| Address bindings | 1,881 | Checked ROM/RAM relationships between fixtures and symbols/state |
| H2 ROM table ranges | 14 | Deterministic source/ROM dual-path extraction ranges |

The H2 surface now covers all 1,690 data ASM files. It includes the complete 1,390-file map ASM build
graph, the 41-file Z80 music graph with two bank/ROM parity checks, plus 281 fixed
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
  Common menus now inventories 42 files and binds all 41 layout-owned sources. Prompt input/results,
  text controls, field items, and service entry points are static contracts; one overlapping member-list
  alternate remains excluded, while UI/presentation timing is queued for concentrated simulation.
  Technical interrupts now binds all 21 layout-owned VInt/DMA/fade/trap files and models the update
  order, eight contextual slots, wait/sleep handshake, input repeat, and queue routing. Hardware timing
  remains queued for one technical runtime matrix.
  Technical graphics now binds all 11 layout-owned decompression/display/palette/special-sprite files.
  Calling conventions and state routing are static contracts. The project-owned Stack decoder now
  covers all 43 battle-terrain payloads/45 pointer slots, 27 battle-background payloads/30 pointer
  slots with two 6,144-byte tilesets each, 86 ally/enemy battle-sprite containers with 408 frames,
  and 52 portrait payloads/56 pointer slots. This includes 167 battle-sprite palettes,
  background/portrait palette boundaries, and portrait eye/mouth metadata. Remaining Basic/embedded
  Stack corpora, animation sequencing, and rendered frames remain queued.
  Technical interfaces bind all 25 jump/pointer files and hash the complete 331-stub/60-pointer map;
  this routing structure requires no runtime replay.
  Remaining technical services inventory all twelve resource/sound/SRAM/input/copy/RNG files. Eleven
  main-layout files are H1-bound; the standalone Z80 source is separately assembled and H2-hashed
  without pretending it owns a 68000 listing symbol. Resource routing, overlap-copy direction,
  controller scan shape, SRAM checksum/slot shape, and the sound build chain are static contracts;
  hardware and timing questions remain in four grouped runtime queues.
  Startup/main-loop/exploration now binds all thirteen layout-owned files and models cold/system
  initialization, region admission, battle/exploration routing, six map-event types, entity/area
  interaction, item handoff, and event-before-action polling. Reset hardware, VInt-edge event/input
  perception, and visible map transitions remain grouped runtime questions.
  All nineteen special-screen files are inventoried across logo, title, witch, suspend, and ending
  groups. Eighteen resource routes and the save/reset/cheat/effect control structure are static
  contracts; rendered parity is queued as three presentation matrices.
  The ROM header, window engine, battle test, configuration mode, and debug battle actions close the
  final five primary layout sources. Header/vector, eight-slot window, and debug route shapes are
  static contracts; window/debug presentation remains queued.
  The first complete data-directory batch now inventories all 18 files under `data/battles/global`.
  Seventeen are layout-owned and H1-addressed; the unused all-zero `global/afterbattlejoins.asm`
  alternate is hashed but excluded rather than borrowing the cutscene table's same-named symbol.
  Existing battle-AI/drop owners are preserved while eleven new tables cover battle coordinates,
  neutral entities, backgrounds, leader flags, halved EXP, movement costs, lasers, random upgrades,
  and after-battle positions. Caller-dependent behavior remains four grouped runtime questions.
  The complete 42-file `data/stats/allies` tree is now indexed too. Twelve files are direct layout
  includes and thirty ally-stat files enter transitively through `stats/entries.asm`; every file has a
  distinct H1 address. The inventory independently rechecks the existing growth/static-core rails:
  30 allies, 32 start definitions, 32 classes, five growth curves, 59 class-growth blocks, seven
  inherited spell lists, and 122 learned-spell rows. Only trailing start records and battle-sprite
  `NONE` presentation stay queued.
  The adjacent ten item, four spell, and five enemy data files are now completely indexed as well.
  Their 128 item records, 44 spell names/elements and 89 level definitions, 103 enemy definitions,
  166 enemy map-sprite rows, shops, mithril, weapon graphics, range rings, and gold used/unused split
  are structurally rechecked while existing core/enemy/battlefield rails retain semantic ownership.
  Battle cutscene data now has a complete 61-file inventory: 59 labeled cutscenes enter through one
  unlabeled storage include container and have H1 bindings; one Battle 01 region script is an orphan
  absent from the original build. Type/command shape is indexed, but story content and side effects
  are explicitly not counted as reconstructed.
- **Minimal or unindexed:** individual event-script content, conversations,
  detailed shops/church flows, save payload semantics, maps beyond the Battle 01 slice,
  graphics/audio runtime output, and most content tables.

Therefore 98.45% is the useful current code-file-reach snapshot, while whole-game semantic and remake
completion remain **Unknown**. Any later percentage must name its denominator and evidence level.

The six files outside strict symbol reach are explicit exceptions, not uninspected backlog:

- `code/common/stats/items/fielditemeffects.asm`, `itemactions_1.asm`, and
  `itemfunctions_s7_0.asm` are unassembled alternate item implementations covered by the stats H2
  inventory;
- `code/common/menus/writememberlisttext.asm` overlaps its canonical assembled implementation and is
  covered by the menus H2 inventory;
- `code/common/scripting/text/unused_textfunctionsdata.asm` is an unlabeled byte range covered by the
  scripting H2 inventory but cannot satisfy a named-symbol index contract;
- `code/common/tech/sound/sounddriver.asm` is separately assembled for the Z80 and H2-hashed by the
  technical-services inventory, so its labels do not exist in the 68000 H1 listing.

## Reproduction

The tracked evidence counters are reproduced by:

```powershell
uv run sf2 research-index list --summary
uv run sf2 research-index test
```

For the pinned checkout, `rg --files local/upstream/SF2DISASM/disasm/code -g '*.asm'` yields 387
files and the corresponding `data` query yields 1,690. The index summary reports 1,444 records; its
verifier reports 381 unique code files, 980 unique data files, 49 H2 fixtures, 58 H3 fixtures, and
1,881 bindings. The
default `uv run sf2 verify` checks those
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

## Current Direction

Code-file discovery is at its honest symbol-index ceiling: 381/387 files have strict reach and the six
remaining files are explicitly owned H2 exceptions. Data-file discovery is also closed at 1,690/1,690
deterministic H2 inventory; its lower 980/1,690 strict reach reflects include-site-only map bodies,
alternates, unlabeled storage, and the separate Z80 address space rather than unknown files.

The next work is semantic depth, not another sweep for filenames or artificial index percentage.
Map-content source/byte closure now covers all 79 map entries, 662 source-form sections, and 154
private blocks/layout payloads. The 77 payload pairs also decode deterministically to 19,771 blocks
and 77 complete 64x64 layouts with every block reference in range. Those structures now join into a
deterministic 79-map, 1,859-resource canonical import with 15,805 logical records/operations. Its 64 setup routes
and 126 six-pointer definitions resolve entity/event/description/init ownership; complete generated
content remains ignored and only aggregate evidence is tracked. All 32 animation tables also have
complete cache/source/target/counter/cycle bounds, with all 108 source ranges inside cache:

1. keep the completed ten-case setup-selector, six-case init-dispatch, nine-case grouped
   entity/zone/item dispatch, and four-case animation VDP matrices as the runtime boundary;
2. compare rendered output through one graphics/VDP matrix;
3. preserve normal-story direct-`rts` event reachability, nonstandard description callers, and
   script side effects as `Inferred` or `Unknown` until stronger evidence exists;
4. extend runtime work into individual init/script side effects only after event integration leaves a
   concrete ambiguity; keep UI/presentation, SRAM hardware, and VDP/Z80/audio timing in
   their own later shared matrices.

Historical subsystem closure details live in the owning research documents and Git history. This
section intentionally states only the active frontier so it cannot masquerade as a stale roadmap.
