# Ally and Class Data Inventory

- Status: **Confirmed** for all 42 source files, their transitive build ownership and H1 addresses,
  table dimensions, growth invariants, class/spell-list counts, and overlap with existing H2 rails
- Status: **Inferred** for presentation fallback behavior
- Status: **Unknown** for two concentrated runtime questions
- Evidence date: 2026-07-19
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Boundary and Transitive Layout

`data/stats/allies` contains 42 ASM files: five at the root, six class tables, and 31 files under
`stats`. Twelve are included directly by the ROM layout. `stats/entries.asm` then includes all 30
`allystatsNN.asm` files, so transitive build ownership is 42/42. Every file exposes one named table or
ally block, and every representative symbol is present in the bit-perfect H1 listing.

This distinction matters for coverage: a direct-layout-only scan would miss 30 real source files and
their individual ROM addresses even though their bytes are assembled into the original game.

## Static Contracts

The Python inventory confirms:

- 30 ally names, 30 map-sprite entries, and 90 battle-sprite/class/palette entries (three per ally);
- 32 start-definition records: 30 named allies plus two trailing records whose runtime reachability is
  still unknown;
- 32 class names, types, and definitions; 16 critical definitions; 15 blacksmith-eligible classes;
- four promotion sections sized 12, 12, 5, and 5, paired with five special-promotion items;
- five 29-level growth curves (145 rows), each maintaining its cumulative-gain invariant and ending at
  the 256-point projection scale;
- 30 ally stat files with 59 class records, 52 explicit spell lists, seven inherited lists, and 122
  learned-spell entries; each ally's first class owns an explicit list;
- a 32-slot pointer table with 30 unique targets; slots 30 and 31 reuse `AllyStats29` through the
  30-file nested include boundary.

The existing growth extractor remains authoritative for canonical growth/spell-learning output, and
the existing static-core extractor remains authoritative for names, class records, and start
definitions. This inventory independently reparses their structural counts and fails if either
manifest disagrees, while adding per-file build reach and H1 provenance that those older rails lacked.

Generated row-level data and names stay under ignored `local/derived/ally-data-static.json`. The
tracked fixture stores only table addresses, counts, dimensions, and hashes.

## Runtime Queue

No emulator was launched. Existing H3 growth and combat fixtures already cover the important growth,
class scanning, spell inheritance, promotion-level, critical, and derived-stat behavior. Two remaining
questions are held for shared future matrices:

1. whether the two trailing start-definition records are naturally reachable;
2. battle-sprite selection and presentation when a class slot contains `NONE`.

## Reproduction

```powershell
uv run sf2 h2 ally-data
uv run sf2 research-index test
```
