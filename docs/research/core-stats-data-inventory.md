# Item, Spell, and Enemy Data Inventory

- Status: **Confirmed** for all 19 source files, H1 addresses, table dimensions, field cardinalities,
  ownership by existing extraction/runtime rails, and the complete item-auxiliary source/ROM catalog
- Status: **Inferred** for presentation and caller admission details
- Status: **Unknown** for four grouped runtime questions
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

## Item Auxiliary Catalog

The deeper Python-owned item rail expands eight of the ten item sources and validates nine separately
addressed ranges against the H1 listing and ROM. It records all 30 count-prefixed shop inventories
(15 weapon and 15 item shops), 235 item references across 15 unique row contents, the 128-item debug
shop, chest-gold values 10 through 130, 25 item-break message offsets, nine mithril class groups and
eight four-choice weapon rows, the one Chirrup Sandals description, nine field-usable items, and all
84 weapon sprite/palette rows for item indexes 26 through 109. The nine ranges total 768 parity bytes.

The consumer audit also fixes behavior that row counts alone could not express:

- shop index 0 selects the first record; later indexes skip count-prefixed records sequentially;
- BRN and RDBN occupy class group 8, but the class scan covers only groups 0-7. The fallback performs
  a two-way random choice between mithril weapon rows 0 and 2, while each selected row tests its
  `16, 8, 4, 1` denominators in order;
- chest gold uses `word[(itemIndex-128)&127]` without a local bounds check;
- an item-break match adds its byte offset to the already-selected base message; field items use a
  `255`-terminated allowlist; special Caravan entries display consecutive messages;
- weapon graphics are ally-only and accept equipped item indexes 26-109. Each row returns signed
  sprite/palette bytes, so sprite `255` becomes `-1`; all rejected cases return `-1/-1`.

The complete generated catalog stays under ignored `local/derived/item-auxiliary-static.json`; the
tracked fixture stores counts, addresses, rules, questions, and canonical hashes.

## Runtime Queue

No emulator was launched. Existing H3 spell, item, enemy, reward, and battlefield matrices remain the
behavior owners. Later concentrated runs should answer:

1. how the 63 enemy map-sprite rows beyond the 103 enemy definitions are selected;
2. special Caravan description presentation;
3. story/debug caller admission for the 30 shop indexes, including gaps in named item-shop enums;
4. blacksmith order persistence, presentation, and observed frequencies for the statically derived
   mithril row selection.

## Reproduction

```powershell
uv run sf2 h2 core-stats-data
uv run sf2 h2 item-auxiliary
uv run sf2 research-index test
```
