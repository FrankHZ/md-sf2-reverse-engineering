# Global Battle Data Tables

- Status: **Confirmed** for the complete 18-file directory inventory, the 17 layout-owned tables and
  their H1 addresses, table dimensions, source-derived values, and the single unused alternate
- Status: **Inferred** for caller-dependent presentation and timing semantics
- Status: **Unknown** for the four concentrated runtime questions listed below
- Evidence date: 2026-07-19
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Boundary and Build Ownership

`data/battles/global` contains 18 ASM files, 1,512 lines, 1,233 statements, and 59 global labels.
Seventeen files are directly included by the original ROM layout and have representative symbols in
the H1 assembler listing. The exception is `global/afterbattlejoins.asm`: it is a 52-byte all-zero,
source-marked unused alternate. The original layout instead includes
`data/battles/cutscenes/afterbattlejoins.asm`, which owns the listing symbol at `$47D6A`. The inventory
hashes and checks the unused alternate but deliberately does not borrow that address or count the file
as indexed data reach.

Six layout-owned files already had deeper evidence in the battle-AI and enemy-drop rails:
`aicommandsets.asm`, `aipriority.asm`, `aistandbymovements.asm`, `enemyitemdrops.asm`,
`krakenmovecosts.asm`, and `swarmbattles.asm`. This batch preserves those owners and adds one H1-bound
record for each of the other eleven layout-owned files. The directory therefore reaches 17/18 files
without duplicating the existing semantic parsers.

## Confirmed Static Shape

The maintained Python extractor confirms these source structures:

- 45 seven-field battle-map coordinate rows covering 33 distinct maps; battle indexes 11, 25, and 41
  have a non-default trigger coordinate;
- 11 battles with 17 neutral entities using four entity-action scripts;
- 45 battle-wide custom-background entries, 30 enemy-switch flags with enabled indexes 3, 11, 18,
  and 27, plus 16 terrain-to-background entries;
- enemy-leader presence flags for all 45 battles, with 28 marked present;
- one halved-EXP battle entry;
- a 13-by-16 land-effect/move-cost matrix: 208 entries, 127 obstructed;
- three laser battles with per-enemy facing rows of 24, 16, and 12 entries, of which 8, 2, and 2 are
  active laser facings;
- 11 random battles and five enemy-upgrade categories, including the intentionally empty airborne
  exclusion list;
- one after-battle position table with three four-byte entity placements.

The generated local JSON retains the canonical rows and symbolic values. The tracked fixture keeps
only dimensions, selected indexes, addresses, and hashes so that the repository does not become a
redistribution of extracted game content.

## Semantics and Runtime Queue

Table structure and H1 placement are confirmed. Static callers strongly support the table names and
macro comments, but the following behavior still depends on caller state or rendered presentation and
is grouped for later simulation:

1. after-battle position selection and the fourth, source-marked ignored byte;
2. neutral-entity action-script timing and presentation;
3. visible orientation produced by the four background enemy-switch flags;
4. random-battle upgrade bounds across caller level and failed-upgrade cases.

No emulator was launched for this batch. Land-effect/pathfinding behavior, AI commandsets, swarm data,
enemy drops, halved EXP, and laser control already connect to their existing static or concentrated H3
rails; this inventory does not weaken or duplicate those owners.

## Reproduction

```powershell
uv run sf2 h2 battle-global-data
uv run sf2 research-index test
```

Canonical output is written to ignored `local/derived/battle-global-data-static.json`.
