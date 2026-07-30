# Source Coverage and Research Cadence

- Status: **Confirmed** for the pinned-source inventory and current evidence counters
- Evidence date: 2026-07-30
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
| Indexed findings | 1,586 | 1,549 H1-backed plus 37 Z80 music-bank records |
| Indexed source files | 1,398 | 381 code files and 1,017 data files |
| Executable code-file reach | 98.45% | 381 indexed code files / 387 pinned code files; **not** line or function coverage |
| H2 data-ASM inventory | 100.00% | 1,690 / 1,690 pinned data ASM files belong to deterministic inventory rails |
| Indexed data-file reach | 60.18% | 1,017 / 1,690: 980 H1 files plus 37 explicitly domain-bound Z80 songs |
| H2 fixture files | 74 | Deterministic source/ROM contracts, often covering complete corpora |
| H3 fixture files | 73 | Runtime contracts, often containing multiple cases |
| Address bindings | 2,319 | Checked ROM/RAM relationships between fixtures and symbols/state |
| H2 ROM table ranges | 25 | Deterministic source/ROM dual-path extraction ranges |

The H2 surface now covers all 1,690 data ASM files. It includes the complete 1,390-file map ASM build
graph, the 41-file Z80 music graph with two bank/ROM parity checks, 37 song-level pointer/range
bindings, and a 29-macro/39,290-invocation source grammar tied to five driver parsers and one shared
loop state machine, with all 390 channel slots and zero incompatible macro/role uses audited, plus
all 39 four-byte music headers and the complete 64-slot/two-bank command-selection map with no
cross-bank fallback, and the 84-entry YM/64-entry PSG frequency tables bound to 21,841 note calls,
including a 218-command shift/loop CFG audit with zero invalid effective indices, plus the 17-entry
DAC load table bound to 1,559 music sample calls and the complete 8,376-call instrument/level domain.
The embedded driver now also has full 56-command SFX header coverage: 28 type-1 and 28 type-2
entries, 364 source/binary pointers, 115 unique targets, and 66 active channel references, with the
complete 8 KiB driver byte-matched at ROM `0x1EC000`. All 66 active stream starts are statically
decoded as 786 unique tokens covering 1,447 unique bytes. Their only loop form is seven matched
counted-loop edges, every terminal is `FF 0000`, and no absolute stream redirect is present.
A single-launch four-command sound H3 now joins those static
contracts to 12 checkpoints/120 live Z80 channel snapshots, confirming command clearing, bank/DAC
header state, exact initial pointers, active-channel progression, and the silent control boundary.
The remaining H2 surface includes 281 fixed
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
  six player-control/cursor/menu entry points additionally own nine source ranges, 1,039 statements,
  231 branches, 207 direct calls, and the static selection/cancel/suspend decisions. The same rail
  fixes cursed equipment exchange, give/drop, Deals recovery, and normal/trapped/gold/item chest
  outcomes. Runtime input cadence and presentation timing are still partial.
  The twelve root battle-scene files and all 55 animation descendants are now separately inventoried,
  including the 21-command script interpreter, initialization/selectors, 32 setup/update pairs,
  shared setup files, reused update targets, and root-owned update targets. Frame/VDP behavior is
  still explicitly outside that static credit. All ten battle-cutscene routing files are now
  inventoried too, closing file-level reach for all 183 files under `code/gameflow/battle`; map-script
  content and story semantics are not implied by that boundary milestone. Common scripting now has
  a complete 29-file inventory, 28 H1-bound files, 90/80-slot map/entity dispatch tables, and text
  Huffman state. The map-script side additionally closes 82 non-filler opcodes, eight filler slots,
  83 unique handlers, 93 primary/alias/special macros, and 13,515 complete-source invocations across
  169 files. Its handler catalog captures 955 instructions, 16 entity fields, 25 global states, and
  62 direct-call targets. Its ABI adds 133 primary parameters/operand fields, 234 operand bytes, exact
  2/4/6/8-byte width counts, and 77 sequential/1 absolute/4 conditional/1 inline cursor-flow handlers;
  all 13,515 commands additionally belong to 304 programs/348 labels. Their 62 script jumps resolve
  as 42 same-program and 20 cross-program edges, while 122 subroutine calls resolve to 68000 symbols.
  A complete 2,077-file token scan gives 297 referenced programs (187 cross-file, 110 same-file-only)
  and seven zero-reference programs; 347/348 program labels have a reference.
  Story-state extraction adds 51 reads over six flags and 95 write-producing commands over 56 flags
  across 89 programs, including prompt flag 89 and battle-unlock base 400.
  The adjacent map-block-copy extraction adds all 208 `setBlocks`/`setBlocksVar` commands across the
  same 304 zero-inclusive program rows, exact macro/handler/cursor/call order, paired helper shift/
  byte-offset use sites, and the two-call direct/effective caller map. Its seven-case H3 matrix now
  confirms direct layout-copy chronology/readbacks and the `$34`/`$35` update-toggle boundary; three
  consumer, reachability/persistence, and VDP/timing questions remain explicit.
  Story reachability and timing/presentation remain batched runtime questions.
  The entity-action source surface is additionally closed across three shared and 75
  distributed files. The shared 2,864-byte corpus has 118 labels and 732 commands; the distributed
  corpus uniquely owns 1,472 commands in 361 inline programs and 11 standalone ROM ranges, with 17
  named entries, 5,684 action bytes, and complete static targets for 14 branches and 364 jumps. All
  distributed entries have a source reference. Its 80-slot dispatcher is also closed as 37 filler
  slots plus 43 real handlers: 43 runtime macro names map to 40 opcodes, `ac_end` is a separate
  `$8080` copy terminator, and three handler-only conditional/random branch opcodes close the
  non-filler table. Command timing and story-route reachability remain outside that credit. One
  unlabeled 288-byte data blob is H2-verified but
  excluded from symbol reach.
  Common maps now has a complete seven-file inventory covering switch/trigger/egress routing,
  8 KiB layout output shape, load ordering, and VInt gates; camera/VDP timing remains Unknown and is
  priority-frozen unless an acceptance gap reopens it under ADR 0005.
  Common stats now inventories all 20 files and models flags, party/inventory services, spell
  learning, new-game order, the complete 31-entry getter corpus, complete 53-entry mutation wrapper
  corpus, seven-routine byte/word/long clamp algorithm/caller contract, and final combatant-distance
  function contract. Seventeen have independent evidence; three
  unassembled alternate item sources are tracked but excluded from strict reach rather than borrowing
  their canonical twins.
  Common menus now inventories 42 files and binds all 41 layout-owned sources. Prompt input/results,
  text controls, field items, and the built eight-source shop/church/caravan/blacksmith state-machine
  surface are static contracts; one overlapping member-list alternate remains excluded, while UI,
  persistence, caller-return, and presentation timing remain queued for one concentrated simulation.
  Technical services now additionally models the complete seven-entry SRAM save surface: two-slot
  layout, interleaved copying, checksum/flag transitions, and its caller inventory, with durable-media
  hardware behavior retained as Unknown but priority-frozen; its six-entry input surface now binds two-port
  raw sampling, state storage, wait helpers, 11 direct call sites, and one controller/input H3 matrix.
  The paired six-entry RNG surface now records both seed states, base/debug control flow, exact
  low-byte bounded domains, 163 direct named call sites, and the six-site jump-alias boundary; retry
  behavior and seed-copy isolation remain one grouped H3 question.
  Technical interrupts now binds all 21 layout-owned VInt/DMA/fade/trap files and models the update
  order, eight contextual slots, wait/sleep handshake, input repeat, and queue routing. Hardware timing
  remains Unknown but is priority-frozen rather than queued by default.
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
  controller sampling/wait shape, SRAM checksum/slot shape, and the sound build chain are static
  contracts; only acceptance-relevant visible semantics remain active, while driver/hardware timing is
  priority-frozen under ADR 0005.
  Startup/main-loop/exploration now binds all thirteen layout-owned files and models cold/system
  initialization, region admission, battle/exploration routing, six map-event types, entity/area
  interaction, item handoff, and event-before-action polling. Reset hardware, VInt-edge event/input
  perception, and visible map transitions remain grouped runtime questions.
  All nineteen special-screen files are inventoried across logo, title, witch, suspend, and ending
  groups. Eighteen resource routes, all nine compressed tile resources, and the
  save/reset/cheat/effect control structure are static contracts; rendered parity and five
  oversized DMA tails are queued as three presentation matrices.
  The witch slice now additionally closes the four-row New/Load/Delete/Copy dispatcher, all four menu
  page identities, save-flag selector masks, New/Load/Copy/Delete branch and call order, initial-loop
  constants, and five SRAM service callers with zero-inclusive internal/external target totals. Its 118
  source-line/operand provenance records are all referenced by exact ordered semantic summaries.
  One nine-direct-case/two-Load-branch BizHawk launch confirms in-process service writes, distinct
  source checksums 71/247, both Copy directions, delete's occupied-bit-only effect, sampled Load
  restoration, and flag-88 direct/effective target identities. A second one-launch/four-core-replay
  New matrix now confirms free-slot selector boundaries for save flags 0/1/2, all four difficulty
  flag outcomes, selected-slot checksums/samples, and the 3/56/3/3/1 MainLoop handoff while retaining
  the MD CART session-patch readback boundary. Cross-process persistence/recovery plus player-driven
  naming/menu presentation/input cadence and witch/suspend presentation timing remain grouped H3
  questions rather than being inferred from the harness controls.
  The ROM header, window engine, battle test, configuration mode, and debug battle actions close the
  final five primary layout sources. The window contract now records all 16 stable entries, 32
  instruction-scoped external direct-caller files, six separate VInt pointer references, the 16-byte
  entry layout, derived allocation/address formulas, and VInt composition/DMA call order. The bounded
  three-file debug contract additionally pins eight H1 entries, the ordered 29-label roster, four
  configuration choices, seven action-table routes, four stack-alias writes, and the complete
  instruction-scoped caller map; only the grouped window/debug presentation matrices remain queued.
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
files and the corresponding `data` query yields 1,690. The index summary reports 1,579 records; its
verifier reports 381 unique code files, 1,017 unique data files, 74 H2 fixtures, 72 H3 fixtures, and
2,305 bindings. Of the records, 1,542 use H1 and 37 use the restricted Z80 music-bank domain. The
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
deterministic H2 inventory; its 1,017/1,690 domain-aware reach includes 37 Z80 song files while the
remaining gap reflects include-site-only map bodies, alternates, and unlabeled storage rather than
unknown files.

The next work is semantic depth, not another sweep for filenames or artificial index percentage. Under
ADR 0005's 2026-07-23 priority decision, the active frontier is event semantics/state flow, map
interactions, UI/menu/save behavior, and implementation-neutral content contracts. Existing H1/H2/H3
driver and hardware evidence remains in its normal verification rails, but sound-driver exactness and
other low-remake-value hardware work are no longer default runtime-matrix targets.
The map-script engine is now structurally closed as 90 slots, 82 non-filler opcodes, eight filler
slots, 83 unique handlers, and 93 macro forms. A complete code/data scan owns all 13,515 macro calls
and makes eleven unused definitions explicit. All 133 primary operand fields and their 234 bytes are
typed by width/offset/expression, including the shorthand-encoded transition word, and all 83 unique
handlers have a cursor-flow class. Its remaining three runtime questions are grouped by
story reachability, multi-service frame timing, and visible presentation. The full program graph now
owns 304 programs, 348 labels, 303 `csc_end` terminations plus one jump termination, 62 resolved
script jumps, and 122 resolved assembly-subroutine calls; none justifies a one-case emulator launch.
The adjacent reference graph scans all 2,077 code/data files and separates 297 statically referenced
programs from seven zero-reference source bodies. This is an input-selection bound, not proof that
the corresponding caller state occurs during normal play.
The same corpus now owns every explicit story-flag access: six read flags, 56 write flags, and only
three read/write overlaps (71, 76, 89). Runtime work can therefore select complete flag cohorts rather
than launching one case per command.
Its bounded dialogue-command family is separately exact: six primary macros account for 2,883 ordered
program-command references (2,058 `nextSingleText`, 0 `nextSingleTextVar`, 577 `nextText`, 0
`nextTextVar`, 234 `textCursor`, 14 `hideText`) and zero-inclusive totals for all 304 programs.
The static contract retains physical macro bytes separately from handler word reads; guards the named
skip/sentinel/call/index/close sections; and joins the independent 4,267-line text-ID domain and
119-row sprite-dialogue table by source/ROM provenance. Its only new H3 work is the grouped
`dialogue-presentation/runtime-matrix`; timing, rendering, and story-specific meaning remain outside
this static credit.
Its directly coupled transition family is also exact: five primary source forms account for 146 ordered
sites across all 304 zero-inclusive program totals (38 `warp`, seven `resetMap`, 60 `loadMapFadeIn`,
24 `reloadMap`, and 17 `mapLoad`). The contract separately keeps the 79-ID canonical map domain and
the exact `MAP_CURRENT` source sentinel, named handler cursor/call/fall-through structure, the shared
parsed packed-coordinate multiplier, and zero-inclusive direct/effective caller maps for four service
targets. The sole grouped H3 follow-up is `map-script-transition-presentation-matrix`; map/camera,
event-consumer, fade, and display outcomes remain runtime questions rather than static claims.
The adjacent camera-control family is exact as three source-named forms: 125 `setCameraEntity` `$24`,
247 `setCamDest` `$32`, and 43 `cameraSpeed` `$45` commands (415 total) in 123 non-empty source
groups, with the same complete 304-row zero-inclusive program domain. Its contract keeps the physical
macro widths/comments, named csc24 branch-target and constant use sites, csc32/csc45 cursor/state/call
order, and the `j_SetCameraDestination` direct identity separately from its
`SetCameraDestination` effective target. The parsed helper's two `MAP_TILE_SIZE` multiplication use
sites resolve to 384, while per-handler internal/external direct/effective caller maps stay
zero-inclusive. Its seven-case H3 fixture now confirms the bounded negative/ally/enemy lookup paths,
two source-word destination transfers (including a 16-bit transfer boundary), two speed writes, and
the source call/wait/return sequence from one launch. The only remaining grouped H3 questions are
`map-script-camera-control/normal-story-reachability` and
`map-script-camera-control/vdp-player-visible-behavior`; source labels still do not establish
player-facing target/destination meaning, units, or presentation.
The adjacent entity-placement slice is exact for four source-named forms: 608 `setPos` `$19`, two
`setPosFlash` `$17`, 1,579 `setFacing` `$23`, and 99 `setDest` `$29` commands (2,288 total) across
204 non-empty groups and the same 304-row zero-inclusive program domain. It preserves physical macro
bytes/comments, four named handler section guards, alive-status cursor-adjustment and shared-tail
boundaries, parsed `MAP_TILE_SIZE` use sites, source-shaped state reads/writes, resolved local branch
targets, and six-target zero-inclusive direct/effective caller maps. The provenance join to the
independently parsed entity-action static fixture does not promote a runtime movement/animation fact.
Its seven-case, one-launch H3 matrix now confirms alive/dead current-HP cursor branches for `setPos`
and `setFacing`, source-scaled entity-record words/facing, 31 exact local flash loop triplets plus
the shared csc19 tail, and both signed destination-delta polarities with bit-15 wait/bypass. The
remaining grouped H3 questions are normal-story reachability, full animation/visibility/presentation,
and collision/pathfinding/persistence; the source labels still do not establish coordinate units or
player-visible meaning.
The adjacent map-script to entity-action bridge slice is exact for six source-named forms: 1,015
`setActscriptWait` and 436 `setActscript` `$15` commands, 359 `customActscriptWait` and two
`customActscript` `$14` commands, and 957 `entityActionsWait` and 487 `entityActions` `$2D` commands
(3,256 total). It retains 196 ordered source-site rows and the same 304-row zero-inclusive program
domain, exact `$FF`/zero control-byte aliases, compact source/total order hashes, the three named
handler/cursor/branch/call sections, exact inline terminator and payload boundaries, zero-inclusive
direct/effective caller maps, and provenance-only joins to the map-event and entity-action fixtures.
Its csc14 records derive two-byte scan transfers and exact iteration counts from each word-aligned
custom payload, separately from csc2D's two-byte interpreted-command reads and its resolved local tail
transfer target. Its one-launch, six-case H3 matrix now confirms handler/callback reach, seeded
entity-index-to-wait-timer resolution, zero versus bounded-wait paths, csc14 inline terminator, csc2D
indexed target/terminal reach, source-shaped entity fields/cursor results, and an exact write-time
snapshot of the selected csc2D record plus terminal idle payload. The snapshot PC is immediately after
the source payload write; it does not claim stable post-handler buffer contents. The remaining grouped
questions are normal-story reachability, full action/motion/collision effects, and presentation/timing/
persistence.
The adjacent entity lifecycle/presentation slice is exact for eight source-named forms: 141 `hide`
`$2E`, 70 `startEntity` `$1B`, 107 `stopEntity` `$1C`, 30 `waitIdle` `$16`, 56 `setSprite` `$1A`,
51 `setPriority` `$53`, five `removeShadow` `$30`, and four `setSize` `$50` commands (464 total).
It retains 105 ordered source-site rows, the complete 304-row zero-inclusive program domain, raw
macro operand comments/widths, eight complete named handler guards, parsed constant/literal use sites,
branch target identity, and nine-target zero-inclusive direct/effective caller maps. Its three joined
fixtures are provenance-only identities. The 11-case, one-launch H3 matrix now covers all eight handler
entries, live and zero-current-HP start/stop boundaries, the controlled second `waitIdle` compare,
both `setSprite` selector sides, zero/nonzero priority, the complete remove-shadow callback chain, and
source-backed set-size temporary/restore words. It confirms only those bounded callback/state records.
The remaining grouped H3 queues are normal-story reachability, full entity-state/callback effects, and
player-visible presentation/timing/collision/persistence; source names do not credit those outcomes as
static coverage.
The adjacent entity gesture/relationship/motion slice is exact for seven source-named forms: 191
`shiver` `$2A`, 169 `nod` `$26`, 160 `followEntity` `$2C`, 15 `faceEntity` `$52`, seven
`moveNextToPlayer` `$28`, two `fly` `$2F`, and one `moveEntityAboveAnother` `$31` command (545 total).
It retains 133 ordered source-site rows, the complete 304-row zero-inclusive program domain, raw macro
operand comments/widths, seven complete named handler guards, parsed source constant/literal use sites,
branch and loop target identity, and ten-target zero-inclusive direct/effective caller maps.
Its 17-case, one-launch H3 matrix now reaches all seven handler entries and confirms only its exact
controlled records: shiver's three source-local temporary/restore cycles, nod's final guarded counter
write, follow's two selector records plus high-byte zero-HP helper boundary, five face operand/tie
records, four move operand/forced-wait records, both fly layer writes, and the above-handler register
record. The remaining grouped H3 queues are normal-story reachability, full entity-state/callback
effects, and player-visible presentation/timing/collision/persistence; source spelling does not credit
those outcomes as static coverage.
The adjacent screen/map-presentation slice is exact for twelve source-named forms: 194 `setQuake`
`$33`, 98 `fadeInB` `$39`, ten `fadeOutB` `$3A`, one `slowFadeInB` `$3B`, zero `slowFadeOutB` `$3C`,
11 `tintMap` `$3D`, five `flickerOnce` `$3E`, 15 `mapFadeOutToWhite` `$3F`, 15
`mapFadeInFromWhite` `$40`, 96 `flashScreenWhite` `$41`, eight `fadeInFromBlackHalf` `$4A`, and six
`fadeOutToBlackHalf` `$4B` commands (459 total). It retains 115 ordered source-site rows, the complete
304-row zero-inclusive program domain, raw macro comments/widths, twelve complete named handler guards,
source immediate/operand records, branch/loop target identity, and five-target zero-inclusive
direct/effective caller maps that preserve seven PC-relative `LaunchFading` sites.
`map-script-screen-presentation/runtime-effects-matrix` is the sole grouped H3 follow-up; source names
do not credit visual, timing, palette, VDP, persistence, or reachability behavior as static coverage.
**Confirmed:** the adjacent entity-presentation-FX slice is exact for three source-named forms: 66
`animEntityFX` `$22`,
63 `headshake` `$27`, and 48 `entityFlashWhite` `$18` commands (177 total). It retains 61 ordered
source-site rows, the complete 304-row zero-inclusive program domain, exact direct/shorthand operand
annotations, three complete handler guards, the separately marked `loc_46BE2` function-chunk target for
the two matching branches, and nine-target zero-inclusive direct/effective caller maps. The sole grouped
H3 follow-up is `map-script-entity-presentation-fx/runtime-effects-matrix`; source names, fields,
literals, tables, and callee names do not credit an entity effect, selector meaning, visual, timing,
persistence, or reachability behavior as static coverage.
**Confirmed:** the adjacent UI-primary-command boundary is exact for four `showPortrait` `$1D`, one
`hidePortrait` `$1E`, and zero `menu` `$12` occurrences (five commands in four source-site rows). It
retains all 304 zero-inclusive program rows, the source byte/word operand annotations, three complete
named handler guards, a provenance join to the already parsed dialogue portrait-helper record, and
seven-target alias-aware direct/effective caller maps. The sole grouped H3 follow-up is
`map-script-ui-command/runtime-effects-matrix`; source names, packed fields, aliases, literals, and
callee names do not credit UI output, input/choice, timing, persistence, save behavior, or reachability
as static coverage.
**Confirmed:** the residual source-named `cloneEntity` `$25` boundary is exact for nine commands in
two source rows and the same 304-row zero-inclusive program domain. It retains two two-byte operand
comments, the complete seven-statement `csc25_cloneEntity` section, two ordered A6 word reads and
`GetEntityAddressFromCharacter` calls, and the one-byte `ENTITYDEF_OFFSET_ENTNUM` read/write transfer
at parsed offset 18. The section has no parsed loop, counter, or whole-record span, so static coverage
does not promote the macro name into record-copy, lifetime, allocation, collision, rendering,
persistence, or reachability behavior. Its sole H3 follow-up is
`map-script-entity-clone/runtime-effects-matrix`.
**Confirmed:** after the dedicated map-script command-family records, exactly ten tracked macro
definitions remain without a dedicated `*CommandFacts` macro list: `csc_end`, `csc06`, `csc14`,
`csc15`, `csc2D`, `cscNop`, `csWait`, `executeSubroutine`, `jump`, and `playSound`. This is a
source-inventory ownership boundary only; it does not make those forms semantically unclassified or
remove the separate alias, handler, and program-corpus coverage already recorded elsewhere.
The adjacent roster/death family is likewise exact for six primary source forms: 34 `join`, zero
`jumpIfDefeatedByLastAttack`, zero `jumpIfDead`, five `allyDefeated`, one
`updateDefeatedAllies`, and three `reviveAlly` sites across the same 304 zero-inclusive program rows.
Its contract preserves the source macro/handler label differences, physical byte layouts, six bounded
named-section branch/mutation/call guards, seven direct/effective caller identities (including alias
resolution), and a provenance-only join to the common-stats roster source. The sole grouped H3
follow-up is `force-state/roster-death-persistence-visible-outcomes`; story reachability, list/roster
capacity, persistence, and visible effects are not credited as static coverage.
The adjacent active-party/AI/follower/battle-stat slice is exact for four primary source forms: one
`joinBatParty`, four `joinForceAI`, five `resetForceBattleStats`, and 19 `addNewFollower` sites (29
total) across a second complete 304-row zero-inclusive program corpus. It retains four named-section
cursor/branch/mutation/call guards, 11 direct/effective caller identities with zero-inclusive
per-handler and derived scope maps, and source-identity joins to battle-party, activation-bit, and
follower-owner and battle-stats-owner sources. Its grouped H3 follow-up is
the nine-case one-launch `sf2-force-state-active-party-runtime-v1` matrix. It confirms bounded local
handler/service chronology, handler-local roster timing, activation/join mutation, reset-order, and
follower allocation/list effects, but does not credit normal-story reachability, save-load/capacity lifecycle,
or player-visible presentation; those three source-faithful queues remain explicit.
The adjacent story-state branch/prompt slice is exact for seven source forms: 24 `jumpIfFlagSet`, 27
`jumpIfFlagClear`, zero primary `csc10`, 37 `setF` aliases, 16 `clearF` aliases, 22 `yesNo`, and 20
`setStoryFlag` sites (146 total) across another complete 304-row zero-inclusive corpus. It preserves
the primary carrier's physical two-word layout apart from its aliases, five named handler branch/cursor/
mutation guards, five direct/effective caller identities with alias resolution and zero-inclusive
per-handler/scope maps, and source-only joins to the game-flag and yes/no owners. Its one-launch,
ten-case H3 matrix confirms handler-local branch/cursor outcomes, set/clear bit results, both yes/no
return sides plus the `Sleep` call, and base/wrap battle-unlock writes using parsed GAME_FLAGS
base/span/mask facts. It deliberately leaves exactly normal-story reachability, save-load lifecycle
persistence, and player-visible yes/no presentation/timing as separate Unknown questions.
The adjacent map-block-copy slice is exact for two source forms: 201 `setBlocks` `$34` and seven
`setBlocksVar` `$35` sites across a further complete 304-row zero-inclusive corpus. It retains all
six one-byte source-label fields, the two 8-byte layouts, three exact A6 word reads, direct helper
identity/order, the `$34`-only source bit-set order, and the called helper's paired 8/6/2/128 shift/
offset use sites with independently named stream, transfer, copy, and row-offset quantities. Its
two direct/effective caller rows preserve the zero internal and two external `CopyMapBlocks` totals.
Its one-launch seven-case H3 matrix now confirms direct bounded FF0000-layout word-copy chronology,
overlap behavior, `$34` update-bit order, `$35` no-toggle behavior, and exact readbacks. Collision/
pathfinding consumer effects, normal-story reachability plus map-reload/save persistence, and visible
VDP presentation/cycle-pixel timing remain three explicit H3 questions; none is implied by this
direct-layout coverage.
The adjacent entity population/reload slice is exact for four source forms: 18 `newEntity` `$2B`, 69
`loadMapEntities` `$42`, two `reloadEntities` `$44`, and seven `loadEntitiesFromMapSetup` `$49` sites
(96 total) across a further complete 304-row zero-inclusive corpus. It retains physical command
layouts and source comments (including the three deliberately blank `$49` labels), four exact named
handler cursor/read/VInt/call/constant guards, and a zero-inclusive direct/effective caller inventory.
The direct `j_InitializeMapEntities` identity remains distinct while the parsed jump-interface alias
resolves it to an `InitializeMapEntities` effective total of three. Its one-launch 12-case H3 matrix
now confirms handler-local callback/cursor/list/record results for three selected `newEntity` identity
seeds, one direct-table load, one identity-list-selected reload record, and all seven map-setup input
rows. It does not credit capacity beyond high-water 49, normal-story/save/map-reload persistence,
player-visible rendering/animation/VDP timing, or collision/pathfinding consumer effects; those remain
four explicit grouped Unknown questions.
The adjacent map lifecycle slice is exact for four source-faithful forms: seven `resetMap` `$36`,
60 `loadMapFadeIn` `$37`, 24 `reloadMap` `$46`, and 17 `mapLoad` `$48` sites (108 total) across a
further complete 304-row zero-inclusive corpus. It preserves macro operand comments, four named
handler guards, the `csc37` physical continuation into `csc48`, exact A6 transfer/probe widths,
the `csc48` `bne.s loc_465C4` target-to-first-`WaitForVInt` identity, VInt records, packed-operand
use sites, and zero-inclusive direct/effective caller maps for `ResetCurrentMap`, `LoadMapTilesets`,
`LoadMap`, `EnableDisplayAndInterrupts`, and `WaitForVInt`. Its H3 fixture adds one five-case/
one-launch handler observation: exact direct-H1-JSR-site order, handler return, `CURRENT_MAP`,
`VIEW_PLANE_A_PIXEL_X/Y`, and two nonasset first/final layout-clear markers. The marker observations
do not establish whole-layout or asset content; direct-call-site hits do not establish service effects;
and the fade case deliberately releases `FADING_SETTING` at its first observed wait. The remaining
grouped H3 queue is `map-lifecycle/layout-collision-pathfinding-effects`,
`map-lifecycle/entity-reload-player-placement`,
`map-lifecycle/presentation-fade-hardware-timing`, and
`map-lifecycle/story-reachability-persistence`.
The adjacent source-named trigger slice is exact for two forms: two `roofEvent` `$43` and six
`stepEvent` `$47` sites across a further complete 304-row zero-inclusive corpus. It preserves both
two-word physical layouts and `trigger X`/`trigger Y` source comments, six-statement named-section
guards for two advancing A6 word reads, two parsed `MAP_TILE_SIZE` (384) multiplier use sites, direct
call/order/return identity, and two-target zero-inclusive direct/effective caller maps. A
provenance-only join retains the `PerformMapBlockCopyScript`/`OpenDoor` owner source and independently
parsed 79 step-table/79 roof-table and 94-step-record/114-roof-record boundaries from map content and
the canonical import decoder. Its six-case, one-launch H3 matrix records Map 02 record-0 hit,
terminator miss, and source-named busy/battle gate rows, with exact direct H1 call-site identity,
D0/D1 words, selected table/terminator boundary, post-handler words, toggle bits, and two nonasset
markers. It does not establish full layout contents, callee service effects, collision/pathfinding,
presentation, audio, timing, hardware effects, normal-play reachability, or persistence. The grouped
remaining queue is `map-interaction-trigger/full-layout-collision-pathfinding-effects`,
`map-interaction-trigger/presentation-audio-timing-hardware-effects`, and
`map-interaction-trigger/persistence-story-reachability`.
The distributed entity-action frontier is now closed rather than provisional. Its 75 non-shared ASM
files comprise 42 under `data/maps`, 26 under `data/battles`, six under `data/scripting`, and one
under `code`. All 1,472 commands have exactly one owner: 1,217 commands in 361 terminated inline
`customActscript`/`customActscriptWait` programs and 255 commands in 11 labeled standalone ROM
ranges. Those ranges expose 17 `eas_*` entries and total 942 source/H1/ROM-checked bytes; combined
with the inline payloads, the distributed surface accounts for 5,684 action bytes. All 14 relative
branches and 364 absolute jumps resolve, every absolute jump targets `eas_Idle`, and all 17 named
entries have at least one same-file or cross-file source reference.

The 80-slot dispatcher is now structurally closed: 37 filler slots, 43 non-filler handlers, 40
macro-addressable runtime opcodes, three handler-only branch opcodes, and one non-dispatched `$8080`
inline terminator. The catalog captures all handler H1 addresses, source spans, parameter reads with
byte/word/long widths, direct calls, and exit routes. Its source-shaped access classifier finds 18
entity fields (11 read, 17 written) and 15 global-state symbols (ten read, five written); the entity
count includes the implicit X field accessed directly as `(a0)`. The semantic catalog now builds on
eight complete source-role families (16 entity-property, eight movement, six control-flow,
five motion-state, three direct-control, three wait, one audio, one map-effect), 22 entity-bit access
records, and 46 fixed/relative/absolute script-pointer actions. `FLAGS_A` bits 5/6/7 are now tied to
the handlers that test or update entity collision, map collision, and obstruction state. Parameter
ABI coverage now joins 46 macro-declared parameters/86 bytes to handler reads: 40 runtime macros read
their full declarations, `ac_pass` ignores its word, and `ac_setId`/`ac_setSprite` consume only the
low byte of their declared words. `ac_branch` is the only named macro with an external operand (a
word-relative displacement). All three handler-only six-byte
layouts are now explicit and absent from the 2,204-command source corpus; overall, 35/43 handlers are
used and eight are absent. Wait/continue/yield transitions are now classified for all handlers: 39
can redispatch, 11 can yield, and seven have both outcomes; four continuous-control handlers are
yield-only. Remaining parameter roles/signedness and
the predicates selecting dual outcomes are now closed too: all 46 declared parameters divide into ten
signed numeric, twenty unsigned numeric, fifteen boolean, and one ignored value, while all seven dual
handlers retain their selecting source statements. `ac_randomWalk` is the only macro-comment/data-flow
disagreement and remains explicit. The next static pass follows those commands into
`UpdateEntityData` movement arithmetic and flag consumption. That 560-byte/190-instruction core is
now split into nine H1-bound phases with 15 entity fields (14 read, nine written), four motion flag
consumers, three direct calls, and a ROM-matched 16-byte facing table. It confirms 3/4 acceleration
and 1/4 deceleration thresholds, velocity-to-position integration, +/-8 facing dominance, animation
delta shift 5/-1 disable/>30 reset, axis crossover snapping, and arrival layer/immersed updates. The
helpers are now closed too: four functions total 434 bytes/135 instructions with 22 call sites. The
conflict helper proves a 49-slot, Manhattan-distance-below-384 test and exposes a source-comment error:
conflict leaves Z clear, no conflict leaves Z set. Sprite auto-facing fallthrough, special/entity-32
bypasses, effect/DMA calls, and the coordinate hash formula are also explicit. A 13-case/20-tick,
one-launch H3 matrix now confirms wait thresholds, blocked/unblocked relative and absolute setup,
successive acceleration/deceleration, facing/animation rules, crossover snap, and three arrival-tile
states against an independent model. Normal-story reachability remains separate.

Map-content source/byte closure now covers all 79 map entries, 662 source-form sections, and 154
private blocks/layout payloads. The 77 payload pairs also decode deterministically to 19,771 blocks
and 77 complete 64x64 layouts with every block reference in range. Those structures now join into a
deterministic 79-map, 1,859-resource canonical import with 15,805 logical records/operations. Its 64 setup routes
and 126 six-pointer definitions resolve entity/event/description/init ownership; complete generated
content remains ignored and only aggregate evidence is tracked. The map-event rail additionally closes
all 684 entity-event target-program boundaries (1,015 labels, 2,624 non-comment source operations,
8,928 physical program bytes, 512 parsed branch/call/jump sites, nine source/H1-resolved jump
interfaces, and zero-inclusive 332-identity instruction/effective target maps), all 150 zone-event
boundaries (251 labels, 809 operations, 2,934 bytes, 183 sites, seven aliases, and 119-identity maps),
and all 80 item-event boundaries (94 labels, 146 operations, 414 bytes, 19 sites, two aliases, and
18-identity maps). The sole additional zone profile is an explicit raw-expression non-program
exclusion; the zone source-stream `csc_end` boundary is H1-delimited before the adjacent body. This
now also has a complete 54-mnemonic/3,579-operation vocabulary: nine source-faithful families, 34
parsed non-CPU definition joins, zero unclassified or ambiguous operations, per-program
physical/setup/route-weighted operation totals, and four retained Map 21 action-payload segments.
Macro-use guards compare parsed definition emission/order and H1 macro-expansion rows before fixture
comparison. The four wrapper/payload context pairs are derived from the maintained map-script
macro/handler cursor-flow contract, the entity-action inline-terminator contract, parsed macro
aliases/emissions, and the smallest csc14/csc2D handler use sites: csc14's parsed-word compare,
not-equal branch, and return, plus csc2D's first-byte negative branch, second-byte read, and
branch-target byte skip. This remains static source/H1 identity and control flow, not effect,
dialogue, persistence, or presentation evidence. The same rail now derives the complete direct
numeric flag-state source surface: 493 sites across every entity/zone/item target program (316
source-label reads, 169 sets, 8 clears), 151 observed operands, zero-inclusive totals for all 914
programs, four independently named physical/reference weights, and every immediate `chkFlg` branch
consumer (264 `bne.s`, 49 `beq.s`, 3 `bne.w`). Parsed macro definitions, numeric operands, consumer
order/polarity/target identities, and source/H1 operation records are guarded before fixture
comparison; this does not claim a flag's persistence, story meaning, or presentation effect. The
same rail now also closes the direct `script` source-reference graph: its one parsed
`lea \1(pc),a0`/`trap #MAPSCRIPT` definition joins 147 source/H1 sites (52 entity, 87 zone, 8 item)
to 138 instruction labels and 135 effective map-script program owners, while retaining all 348
declared labels and 304 declared programs as zero-inclusive target domains. This is source/H1
identity and ownership only, not evidence of execution, side effects, persistence, story reachability,
or presentation. The same complete 914-program source surface now additionally retains 1,006 direct
`TEXTBOX` source/H1 references: 981 numeric line-reference operands and 25 operand-free `$FFFF`
sentinel emissions. It joins the maintained `sf2-text-banks-static-v1` declared contiguous ID domain
through its source/ROM parser, not its golden fixture and without decoding text, making all 4,267 IDs
from 0 through 4,266 zero-inclusive line totals; 942 IDs are observed (11 through 4,178). This
records macro/operand/sentinel and caller/reference identity only, not displayed content, speaker,
window, wait, input, story, or presentation behavior.
Evidence date: 2026-07-27. All 32 animation tables also have
complete cache/source/target/counter/cycle bounds, with all 108 source ranges inside cache:

1. keep the completed ten-case setup-selector, six-case init-dispatch, nine-case grouped
   entity/zone/item dispatch, and four-case animation VDP matrices as the runtime boundary;
2. compare rendered map output and icon/menu presentation through grouped graphics/VDP matrices;
3. preserve normal-story direct-`rts` event reachability, nonstandard description callers, and
   script side effects as `Inferred` or `Unknown` until stronger evidence exists;
4. extend runtime work into individual init/script side effects only after event integration leaves a
   concrete ambiguity; freeze SRAM hardware-failure, VDP/DMA/Z80 cycle, raw controller-latency, and
   audio-register/waveform exactness unless ADR 0005's bounded reopen criteria are met.

Historical subsystem closure details live in the owning research documents and Git history. This
section intentionally states only the active frontier so it cannot masquerade as a stale roadmap.
