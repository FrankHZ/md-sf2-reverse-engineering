# Item, Spell, and Enemy Data Inventory

- Status: **Confirmed** for all 19 source files, H1 addresses, table dimensions, field cardinalities,
  and ownership by existing extraction/runtime rails
- Status: **Inferred** for presentation and caller admission details
- Status: **Unknown** for three grouped runtime questions
- Evidence date: 2026-07-19
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Boundary

This batch covers all ten files under `data/stats/items`, all four under `data/stats/spells`, and all
five under `data/stats/enemies`: 19 files, 6,067 lines, 5,340 statements, and 24 global labels. All
19 are direct original-layout includes and have representative H1 addresses. Two files already had
research-index ownership (`enemygold.asm` and `spellranges.asm`); the other seventeen now receive
their own table bindings without duplicating those existing records.

## Confirmed Shape

The Python inventory checks complete-record cardinality, not just labels:

- items: 128 names and six-field definitions, 30 shop inventories, all 128 debug-shop items, 13 chest
  gold tiers, 25 break-message rules, nine mithril class groups, eight four-choice mithril rows, one
  special Caravan description, nine field-usable items, and 84 weapon-graphics rows;
- spells: 44 names and elements, 89 complete spell-level definitions, plus four range rings sized
  1, 4, 8, and 12;
- enemies: 103 names, full definitions, and battle sprites; 166 map-sprite entries; 172 gold words
  split into the 103 used values and the explicitly rejected 69-word unused tail.

The existing static-core rail remains authoritative for canonical item/spell records, the enemy
promotion rail for enemy names/definitions, the enemy-gold rail for the used/unused boundary, and the
battlefield rail for spell-range semantics. This inventory independently enforces their source shape
and adds the missing per-file H1 provenance. Generated content remains under ignored
`local/derived/core-stats-data-static.json`; the tracked fixture contains only counts, dimensions,
addresses, and hashes.

## Runtime Queue

No emulator was launched. Existing H3 spell, item, enemy, reward, and battlefield matrices remain the
behavior owners. Later concentrated runs should answer:

1. how the 63 enemy map-sprite rows beyond the 103 enemy definitions are selected;
2. special Caravan description presentation;
3. shop/debug-shop admission and ordering at their caller boundary.

## Reproduction

```powershell
uv run sf2 h2 core-stats-data
uv run sf2 research-index test
```
