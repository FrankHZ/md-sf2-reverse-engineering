# Complete Map ASM Inventory

- Status: **Confirmed** for the complete 1,390-file ASM boundary, build reachability, internal-symbol
  addresses, map/setup file classes, pointer/include counts, global table row counts, all 64 setup
  routing rows, last-set-flag selection, 126 six-pointer setup tables, and event dispatcher record shapes
- Status: **Inferred** for event-script side effects and transition-state persistence
- Status: **Unknown** for the description `d6` condition, presentation timing, and binary consumers
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

## Concentrated Queue

No emulator was launched. Setup priority and dispatcher order are now closed by source/H1/ROM
evidence. Remaining questions are grouped as:

1. the area-description byte-2/`d6` condition;
2. selected event-script side effects, transition-state persistence, and roof/step/warp precedence;
3. portrait/text/entity-facing presentation timing and binary block/layout/animation consumers.

Continue parsing the actual setup sections statically. Only ambiguities that survive that pass should
share the prepared map initialization/event-dispatch runtime matrix.

## Harness Performance

This batch more than doubles the research-index record count. The index verifier now parses the H1
listing into a symbol-address map once per run instead of rescanning the complete listing for every
record. The provenance checks are unchanged: each indexed symbol must still occur in its claimed
source file and at the fixture/index address in the H1 listing.

## Reproduction

```powershell
uv run sf2 h2 map-data
uv run sf2 h2 map-setup
uv run sf2 research-index test
```
