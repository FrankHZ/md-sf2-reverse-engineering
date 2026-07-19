# Complete Map ASM Inventory

- Status: **Confirmed** for the complete 1,390-file ASM boundary, build reachability, internal-symbol
  addresses, map/setup file classes, pointer/include counts, global table row counts, all 64 setup
  routing rows, last-set-flag selection, 126 six-pointer setup tables, event dispatcher record shapes,
  all 125 entity-list sources/980 physical entity records, all 263 entity/zone/item event sources with
  1,134 physical records, and all 75 area-description targets/227 physical entries
- Status: **Inferred** for event-script side effects, follower/entity collision state, and transition persistence
- Status: **Unknown** for direct-`rts` entity-event reachability, sequenced-orientation consumption,
  nonstandard description callers, presentation timing, and
  binary consumers
- Evidence date: 2026-07-19
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Complete Build Graph

`data/maps` contains 1,390 ASM files and 44,977 lines. Eight files are included directly by the
original layout: three root containers/tables and five global tables. Their transitive include graph
has 1,382 edges to 1,382 unique targets and reaches every ASM file in the directory. The graph
contains 79 map directories, 662 map-content files, and 720 map-setup files.

This batch therefore provides 1,390/1,390 deterministic H2 inventory. It does not claim 1,390 strict
symbol bindings:

- 727 files define at least one global label in their own source and receive a representative H1
  address plus research-index record;
- 662 map-content files contain bodies whose labels are attached at their include sites in
  `entries.asm`; they are hashed and graph-checked but do not receive a falsely relocated symbol;
- `mapsetupsstorage.asm` is the sole unlabeled include container.

The distinction keeps strict data-file reach conservative while still closing the complete source
discovery boundary.

## Static Shape

`entries.asm` defines 79 map pointer slots, includes 662 source-form map sections, and references 154
private binary payloads. The setup storage includes 720 internally labeled files. The top-level
setup selector contains 64 map rows, 66 flag rows, and 64 map terminators.

The five global tables contain 57 debug-map slots, three flag-switched map rows, 13 overworld-map
rows, four raft-reset rows, and 23 save-point rows. These are table-shape facts only. The inventory
fixture does not reproduce map layouts, dialogue, entity lists, event scripts, descriptions, item
placements, or binary payloads. Full file hashes and include edges remain in ignored
`local/derived/map-data-static.json`.

## Setup Selection and ROM Parity

`GetCurrentMapSetup` at ROM `0x4779E` loads the setup table for `CURRENT_MAP`. A map row starts with a
map word plus its default longword pointer, continues through zero or more flag/pointer pairs, and ends
at `$FFFD`; `$FFFF` ends the complete table. A map absent from the table returns `ms_Void`, whose first
word is `$FFFF`.

The selector does not stop at the first true flag. It loads the default pointer, scans every flag row
in source order, and replaces the candidate after each successful `CheckFlag`. Therefore the
**last set flag in source order wins**. Four flag rows intentionally point back to their map's default
setup: map 7/flag 702, map 33/flags 783 and 22, and map 40/flag 507. Those later aliases can undo an
earlier variant; treating variants as an unordered dictionary or first-match chain would be wrong.

The 64 map rows and 66 flag rows expand to 910 bytes at `MapSetups` (`0x4F6E2`) and byte-match the
canonical ROM. Their 130 pointer references resolve to 126 unique setup tables because of the four
aliases. Every table contains these six big-endian longword slots:

| Offset | Owner |
| ---: | --- |
| `0` | entity list |
| `4` | entity events |
| `8` | zone events |
| `12` | area descriptions |
| `16` | item events |
| `20` | initialization function |

All 126 tables, 756 target slots, and 3,024 pointer bytes are derived independently from source/H1
symbols and byte-match the ROM. The complete routes and pointers are written only to ignored
`local/derived/map-setup-static.json`; the tracked fixture retains compact selection cases, counts,
addresses, and rules.

## Event Dispatcher Shapes

The same H2 rail binds the dispatcher entries and checks their source order:

| Dispatcher | Entry | Static matching rule |
| --- | ---: | --- |
| zone | 4 bytes | `$FD` default; `$FF` wildcards for X/Y; first matching entry |
| item | 6 bytes | `$FD` default; `$FF` wildcards for X/Y/facing; masked item index must match; first match |
| entity | 4 bytes | entity byte or `$FD` default; byte 1 bits 0/1 turn toward actor/restore facing |
| area description | 6 bytes | coordinate word; byte 2 gates on `d6`; byte 3 chooses text indices or relative function |

Initialization calls slot 20 unless the selector returned `ms_Void`; entity-list lookup returns slot
0. These control-flow and record-layout facts are **Confirmed** statically. The meaning of the area
description byte-2/`d6` check remains **Unknown**, as do side effects inside the selected scripts and
their visible timing.

## Entity List Streams

The 126 setup tables contain 126 entity-pointer references but only 125 unique targets:
`ms_map21_Entities` is deliberately reused. The source boundary likewise contains 125
`s1_entities*.asm` files. Decoding from each unique pointer until the consumer's `$FF` first-byte
terminator reaches every source-owned record and no bytes outside that record set.

The physical source contains 980 eight-byte entity records:

| Encoding | Physical records | Per-list references |
| --- | ---: | ---: |
| fixed action pointer | 803 | 808 |
| `$FF` walking payload | 174 | 175 |
| `$FE` sequenced-action pointer | 3 | 4 |
| **Total** | **980** | **987** |

The seven-reference difference is intentional suffix sharing. Nine entity source fragments omit
their own `msEntitiesEnd`: eight fall directly into an adjacent terminator-only variant, while
`ms_map17_Entities` contributes five prefix records and falls into the seven-record
`ms_map17_flag505_Entities` suffix before their shared terminator. A per-file parser would either
invent nine missing terminators or lose this variant composition.

There are 116 unique terminator addresses, 30 empty selected lists, and at most 31 records in one
selected list. `InitializeMapEntities` consumes records in stream order, masks X/Y to six bits,
scales them by `MAP_TILE_SIZE`, and routes special mapsprites through the special-entity declaration
path. The complete numeric records stay in ignored `local/derived/map-entities-static.json`; the
tracked fixture contains only counts, encodings, fallthrough relationships, addresses, and rules.

## Entity, Zone, and Item Event Tables

The event-table rail follows all three event slots from the 126 setup tables, decodes each unique
target from ROM until its `$FD` default record, and checks every record address and kind against its
owning source macros. Relative branch words resolve from the start of the table, matching the
dispatcher rather than the current record address.

| Category | Source files / unique targets | Decoded tables | Physical records | Setup-level references |
| --- | ---: | ---: | ---: | ---: |
| entity | 105 | 103 | 850 (747 specific + 103 default) | 998 |
| zone | 84 | 84 | 202 (118 specific + 84 default) | 313 |
| item | 74 | 74 | 82 (8 specific + 74 default) | 140 |
| **Total** | **263** | **261** | **1,134** | **1,451** |

The difference between 263 unique targets and 261 decoded tables is explicit: entity-event targets
`ms_map52_EntityEvents` and `ms_map55_EntityEvents` are two-byte direct-`rts` stubs, not `$FD`-ended
record streams. They are referenced three times. The map 52 default setup pairs its stub with four
entities, while map 52/flag 512 and map 55 pair theirs with empty lists. The source/ROM shape and
pairing are **Confirmed**; why the non-empty map 52 setup cannot safely reach this dispatcher remains
**Unknown** and is retained as one runtime question rather than guessed away.

Map 44 has the other source exception: its zone default is written as raw `dc.w` values instead of
`msDefaultZoneEvent`, and its relative word resolves to `0x5486C`, four bytes into the cutscene entity
list beginning at `byte_54868`. The upstream source labels this as a bug. The exact bytes, offset 1,044,
and target are **Confirmed**; no intended behavior is inferred from the bad pointer.

The complete tables and decoded branch targets stay in ignored
`local/derived/map-events-static.json`. The tracked fixture keeps category totals, dispatcher rules,
macro counts, and both exception families without redistributing event content.

## Area-Description Wrappers and Tables

The 126 description-slot references resolve to 75 unique callable targets, not directly to record
tables. Thirty-eight targets are two-byte `rts` stubs. The other 37 are identical-shape 16-byte
wrappers: load a per-map description-text base into `d3`, load an internal table into `a0`, execute
`nop`, and jump to `DisplayAreaDescription`. All 75 callable bodies, the wrapper immediates and target
addresses, and the 37 internal `$FD00`-terminated tables byte-match the ROM.

| Payload | Physical entries | Setup-level references |
| --- | ---: | ---: |
| text offsets | 206 | 426 |
| relative function | 18 | 31 |
| `d6`-conditioned relative function | 3 | 4 |
| **Total** | **227** | **461** |

Text entries add byte 4 to global investigation-text base 423 and byte 5 to the wrapper's `d3`
description base. Function entries add their signed word to the internal table base. There are 37
two-byte terminators, 1,436 physical table bytes, and at most 23 entries in one table. Thirty-five
callable targets are reused; across all setups, 67 references select a wrapper and 59 select an
empty direct-return stub.

The formerly unknown byte-2 check is now closed for the normal exploration call graph. The sole
assembled `j_RunMapSetupAreaDescription` call is reached after `explorationvints.asm` executes
`moveq #1,d6` with the source comment “No entity event.” `DisplayAreaDescription` skips a matching
entry when byte 2 is nonzero and `d6` is nonzero. Therefore the three `msDescFunctionD6` entries
(map 31 byte 1, maps 41/42 byte `$FF`) are **Confirmed** unreachable through that one normal call
path. Their behavior under a direct or deliberately mutated `d6=0` caller remains **Unknown**; no
emulator case is justified until such a caller is found or intentionally isolated.

Complete decoded text indices and function targets remain in ignored
`local/derived/map-descriptions-static.json`; the tracked fixture retains counts, the three
conditioned entries, wrapper/dispatcher rules, and call-graph evidence.

## Concentrated Queue

No emulator was launched. Setup priority and dispatcher order are now closed by source/H1/ROM
evidence. Remaining questions are grouped as:

1. sequenced-entity orientation, direct-`rts` entity-event reachability, and description functions
   under nonstandard or mutated callers;
2. follower/map-entity collision state, selected event/description-script
   side effects, transition persistence,
   and roof/step/warp precedence;
3. walking/special-sprite, portrait/text/entity-facing presentation timing, and binary consumers.

Entity streams and all four setup interaction families are now closed statically. Continue with the
84 initialization sections, 47 adjacent setup scripts, and binary consumers; only ambiguities that
survive those passes should share the prepared map initialization/event-dispatch runtime matrix.

## Harness Performance

This batch more than doubles the research-index record count. The index verifier now parses the H1
listing into a symbol-address map once per run instead of rescanning the complete listing for every
record. The provenance checks are unchanged: each indexed symbol must still occur in its claimed
source file and at the fixture/index address in the H1 listing.

## Reproduction

```powershell
uv run sf2 h2 map-data
uv run sf2 h2 map-setup
uv run sf2 h2 map-entities
uv run sf2 h2 map-events
uv run sf2 h2 map-descriptions
uv run sf2 research-index test
```
