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
| Indexed findings | 1,488 | Confirmed symbol/table records in `manifests/research-index.json` |
| Indexed source files | 1,361 | 381 code files and 980 data files |
| Executable code-file reach | 98.45% | 381 indexed code files / 387 pinned code files; **not** line or function coverage |
| H2 data-ASM inventory | 100.00% | 1,690 / 1,690 pinned data ASM files belong to deterministic inventory rails |
| Indexed data-file reach | 57.99% | 980 indexed data files / 1,690; deliberately undercounts other H2 manifests |
| H2 fixture files | 72 | Deterministic source/ROM contracts, often covering complete corpora |
| H3 fixture files | 58 | Runtime contracts, often containing multiple cases |
| Address bindings | 2,046 | Checked ROM/RAM relationships between fixtures and symbols/state |
| H2 ROM table ranges | 25 | Deterministic source/ROM dual-path extraction ranges |

The H2 surface now covers all 1,690 data ASM files. It includes the complete 1,390-file map ASM build
graph, the 41-file Z80 music graph with two bank/ROM parity checks, plus 281 fixed
ally/class/item/spell records, five 29-point growth curves, 59 class-growth records, 122 spell-learn
entries, five promotion sections, 103 enemy names, 103 enemy definitions, 30 enemy-drop entries,
103 used enemy-gold words plus the explicit 69-word unused tail, 119 sprite-dialogue property rows,
and the Battle 01 placement/scene slice. These heterogeneous structures must not be added into a
fake “records completed” percentage.

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
  Huffman state. The entity-action source surface is additionally closed across three shared and 75
  distributed files. The shared 2,864-byte corpus has 118 labels and 732 commands; the distributed
  corpus uniquely owns 1,472 commands in 361 inline programs and 11 standalone ROM ranges, with 17
  named entries, 5,684 action bytes, and complete static targets for 14 branches and 364 jumps. All
  distributed entries have a source reference; command timing and story-route reachability remain
  outside that credit. One
  unlabeled 288-byte data blob is H2-verified but
  excluded from symbol reach.
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
  23 weapon streams, ten shared ground streams behind 30 ground slots, and 52 portrait payloads/56
  pointer slots. This includes 167 battle-sprite palettes, 42 weapon palettes, 27 ground palettes,
  background/portrait palette boundaries, and portrait eye/mouth metadata. The battle-sprite
  animation rail closes 87 ally and 121 enemy pointer/payload entries: 3,800 payload
  bytes parse into 421 frame entries, of which the consumers play 334 as attack frames. Ally entry
  zero doubles as idle frame two and is skipped during attacks; enemy attacks consume every entry.
  Selector offsets, seven embedded spell-animation headers, 43 hold-previous markers, weapon fields,
  and all source/H1/ROM bytes are deterministic; base-index reachability and rendered timing remain
  grouped presentation questions. The separate Basic
  decoder covers 669 valid map-sprite payloads behind 720 slots and preserves one shared `0xFFFF`
  free-spot sentinel as an explicit boundary. The special-sprite rail covers all six source streams,
  five palettes, ten pointers, and both nine-slot dispatch tables. Only IDs 247-255 are fully routed;
  original source references use 251-255, while 246 is pointer-only and 240-245 are unbacked. The
  assignment-domain rail additionally closes all five writers, 81 built script payloads, 20 property
  callers, 980 initial records, and ally/enemy derivation: no original built path produces IDs
  237-250. Deliberate malformed/raw injection, animation sequencing, and rendered frames remain
  optional or queued. The complete nine-resource
  special-screen tile corpus adds 50,176 decoded bytes with source/H1/ROM parity. Three fixed
  transfers match decoder output; five transfer 27,648 aggregate bytes past the decoded boundary,
  leaving those staging tails as an explicit grouped runtime question.
  The witch-menu rail closes the adjacent uncompressed presentation data: one 32-byte choice palette,
  one 960-byte table of twelve unique 5×8 bubble frames, and two source pointers. All 1,000 bytes
  match source, H1, and ROM. Static control flow proves four option groups, three frames each, and the
  selected 20-state 0→1→2→1 phase; exact CRAM/window timing remains in the shared witch matrix.
  The complete uncompressed special-screen presentation rail adds seven palettes (240 color words)
  and five layouts (4,176 words): all twelve resources and 8,832 bytes match H1 and ROM. Title A/B's
  2,560-byte ASM expansion also matches both editor binary mirrors. Compressed tile streams stay with
  their existing owner; palette upload, layout mutation/scrolling, and pixels remain grouped runtime
  questions.
  The adjacent UI rail closes all eight base/diamond-menu/yes-no Stack streams: 23,168 decoded bytes,
  plus the 4,032-byte uncompressed main-menu payload as seven 576-byte/18-tile icons. Nine source
  pointers and the complete nine-entry menu table match ROM. Its first three high-bit entries select
  icon combinations using IDs 0-5, leaving icon 6 without a static table reference; only the
  remaining six entries are indirect compressed-resource pointers. A cross-contract audit now maps
  all twenty technical incbins to eight deep H2 owners with zero unowned entries.
  The complete uncompressed icon-storage rail distinguishes 167 available 192-byte payloads from the
  163 actually assembled into one contiguous 31,296-byte block. It proves all payload/base-pointer/
  highlight-mask ROM bytes, six special storage roles, four source-only exceptions, three physical
  spell-index collisions, the 192-byte member/shop copy with four forced corner nibbles, and the
  384-byte base/highlight output. Unnamed slot 129, collision reachability, palette, DMA, and rendered
  presentation stay grouped rather than inferred from resource shape.
  The adjacent UI-layout rail independently expands all 19 vanilla-built graphics/tech ASM owners:
  27 leaf tilemaps, 2,394 VDP words, the 16-entry/10-target spell-level pointer table, four 48-byte
  diamond borders, and four direct UI tile payloads. All 5,614 unique bytes and 36 indexed symbols
  match source, H1, and ROM. The window-border aggregate and fighter mini-status alternate remain
  explicit non-build sources; runtime overwrites, palette/DMA order, motion, and rendered frames stay
  grouped presentation questions.
  The variable-width-font rail closes the adjacent text data path: 80 fixed 32-byte glyph records,
  the 256-entry ASCII conversion table, its longword pointer, and three consumer entry points. All
  2,820 font/pointer/map bytes match source, H1, and ROM. ASCII conversion emits 78 glyph IDs and
  defaults 145 inputs to glyph 1. The adjacent context-Huffman rail closes the 510-byte/255-entry
  offset table and 1,952-byte tree payload as 86 trees and 1,536 leaf codes. Their spans exactly cover
  the payload, all defined contexts are reachable from 254, and no emitted symbol selects one of the
  169 `$FFFF` entries. Combined Huffman and ASCII input still omits glyph IDs 70-71; only nonstandard
  direct injection and final typewriter presentation remain grouped questions.
  All seventeen text banks now replay through that tree contract: 79,013 source bytes contain 4,267
  records and decode to 152,679 symbols with one terminator each. All 86 contexts occur, the 17-entry
  pointer table/top pointer and 79,086 total bytes match ROM, and control symbol 253 occurs zero times.
  Plaintext and per-record symbols stay under ignored `local/derived`; tracked evidence is aggregate.
  The remaining nominally unused technical payloads are now byte-closed too: one 5,694-byte container
  resolves into four unique 8,192-byte Stack streams, while one 64-byte payload contains two valid
  16-color palettes differing at two indices and has a ROM-matching pointer. No symbolic ASM consumer
  exists; raw/computed/debug reach and rendered meaning remain Unknown.
  Battle-effect graphics now covers 23 spell containers, four invocation containers with 15 frames/
  30 streams, one status stream, and two transition streams. The 56 streams decode to 200,992 bytes
  with complete resource/pointer/table ROM parity. Invocation output is consistently 4,096 bytes
  against a 4,608-byte transfer, leaving 15,360 aggregate tail bytes queued for one presentation
  matrix rather than guessed as padding.
  The map-tileset rail covers all 115 pointer slots and payloads, each with a fixed 4,096-byte output
  (471,040 bytes total). It also ROM-checks all 79 five-slot map headers and 32 animation headers:
  326 ordinary references plus 32 animation references reach 114 unique tilesets. Only index 29 has
  no static reference, so dynamic unreachability remains an explicit question.
  The adjacent map-palette rail covers all sixteen 32-byte payloads, their pointer table/top-level
  pointer, and all 79 map-header palette bytes. All sixteen palettes are referenced; the corpus has
  256 mask-valid Genesis color words and 69 unique source values. Fifteen source palettes have a
  nonzero first word, but `LoadMap` clears palette-1 color zero after copying, so every effective map
  palette starts at zero. Fade/transition and rendered per-map presentation remain one grouped
  runtime question.
  A source-wide denominator now finds 46 direct named compression consumers across 23 files: 35
  Stack calls, four Basic calls, and seven compressed-DMA wrapper calls. All 46 map to twelve
  complete corpus/infrastructure owners; unowned direct calls are zero. Dynamic indirect or
  self-modifying decoder entry remains outside this explicitly named-call metric.
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
  groups. Eighteen resource routes, all nine compressed tile resources, and the
  save/reset/cheat/effect control structure are static contracts; rendered parity and five
  oversized DMA tails are queued as three presentation matrices.
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
  The deeper item-auxiliary rail expands eight source tables into a canonical catalog: 30 shop
  inventories with 235 references, the complete 128-item debug shop, 13 chest-gold tiers, 25 item-
  break offsets, nine mithril class groups and eight four-choice weapon rows, one special Caravan
  description, nine field-usable items, and 84 weapon-graphics rows. Nine ranges totaling 768 bytes
  match source, H1, and ROM. Static consumer flow proves that BRN/RDBN randomly choose mithril row 0
  or 2 instead of consuming a ninth weapon row; caller admission, persistence, presentation, and
  observed RNG frequency remain grouped runtime questions.
  The enemy-map-sprite rail separately closes all 166 bytes of its table and the only consumer.
  Rows 0-102 align with the 103 enemy definitions. Rows 103-165 are an NPC-sprite tail containing
  values 167-229, except value 189 is absent and value 199 is duplicated. All 627 pinned battle-
  spriteset references remain at 0-102, upgrade tables top out at 84, and the only named
  `SetEnemyIndex` caller is combatant initialization. Normal battle construction therefore cannot
  reach the tail; the unchecked lookup leaves only raw/debug/corrupt state as an explicit boundary.
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
files and the corresponding `data` query yields 1,690. The index summary reports 1,488 records; its
verifier reports 381 unique code files, 980 unique data files, 72 H2 fixtures, 58 H3 fixtures, and
2,046 bindings. The
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
The distributed entity-action frontier is now closed rather than provisional. Its 75 non-shared ASM
files comprise 42 under `data/maps`, 26 under `data/battles`, six under `data/scripting`, and one
under `code`. All 1,472 commands have exactly one owner: 1,217 commands in 361 terminated inline
`customActscript`/`customActscriptWait` programs and 255 commands in 11 labeled standalone ROM
ranges. Those ranges expose 17 `eas_*` entries and total 942 source/H1/ROM-checked bytes; combined
with the inline payloads, the distributed surface accounts for 5,684 action bytes. All 14 relative
branches and 364 absolute jumps resolve, every absolute jump targets `eas_Idle`, and all 17 named
entries have at least one same-file or cross-file source reference.

The next semantic-depth batch should bind the 44 defined `ac_*` opcode forms to the 80-slot entity
action dispatcher and record handler RAM reads/writes, signedness, collision/obstruction flags, and
wait/termination transitions. Frame timing, collision outcomes, and normal-story reachability remain
runtime questions and should be grouped only after that handler contract identifies real ambiguity.

Map-content source/byte closure now covers all 79 map entries, 662 source-form sections, and 154
private blocks/layout payloads. The 77 payload pairs also decode deterministically to 19,771 blocks
and 77 complete 64x64 layouts with every block reference in range. Those structures now join into a
deterministic 79-map, 1,859-resource canonical import with 15,805 logical records/operations. Its 64 setup routes
and 126 six-pointer definitions resolve entity/event/description/init ownership; complete generated
content remains ignored and only aggregate evidence is tracked. All 32 animation tables also have
complete cache/source/target/counter/cycle bounds, with all 108 source ranges inside cache:

1. keep the completed ten-case setup-selector, six-case init-dispatch, nine-case grouped
   entity/zone/item dispatch, and four-case animation VDP matrices as the runtime boundary;
2. compare rendered map output and icon/menu presentation through grouped graphics/VDP matrices;
3. preserve normal-story direct-`rts` event reachability, nonstandard description callers, and
   script side effects as `Inferred` or `Unknown` until stronger evidence exists;
4. extend runtime work into individual init/script side effects only after event integration leaves a
   concrete ambiguity; keep UI/presentation, SRAM hardware, and VDP/Z80/audio timing in
   their own later shared matrices.

Historical subsystem closure details live in the owning research documents and Git history. This
section intentionally states only the active frontier so it cannot masquerade as a stale roadmap.
