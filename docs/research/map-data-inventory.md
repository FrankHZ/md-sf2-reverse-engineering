# Complete Map ASM Inventory

- Status: **Confirmed** for the complete 1,390-file ASM boundary, build reachability, internal-symbol
  addresses, map/setup file classes, pointer and include counts, and global table row counts
- Status: **Inferred** for setup/event dispatch and transition precedence
- Status: **Unknown** for four grouped runtime and binary-format questions
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
rows, four raft-reset rows, and 23 save-point rows. These are table-shape facts only. The fixture does
not reproduce map layouts, dialogue, entity lists, event scripts, descriptions, item placements, or
binary payloads. Full file hashes and include edges remain in ignored `local/derived/map-data-static.json`.

## Concentrated Queue

No emulator was launched. Remaining questions are grouped as:

1. map-setup flag priority and fallback selection;
2. entity, zone, item, and description dispatch order;
3. transition-state persistence plus roof, step, and warp precedence;
4. binary block/layout and animation consumers.

The first three share map initialization and event-dispatch observation points and should become one
generated runtime matrix. Binary decoding remains static-first and should only add runtime cases for
ambiguities that survive source/ROM parsing.

## Harness Performance

This batch more than doubles the research-index record count. The index verifier now parses the H1
listing into a symbol-address map once per run instead of rescanning the complete listing for every
record. The provenance checks are unchanged: each indexed symbol must still occur in its claimed
source file and at the fixture/index address in the H1 listing.

## Reproduction

```powershell
uv run sf2 h2 map-data
uv run sf2 research-index test
```
