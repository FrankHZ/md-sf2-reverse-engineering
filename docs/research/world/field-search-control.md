# Field Search Control

Status: **Confirmed (static control spine only)**
Evidence date: 2026-08-24

The public H2 fixture `sf2-field-search-control-static-v1` reproduces the bounded
field-search control spine:

```text
uv run sf2 h2 field-search-control
```

## Provenance and boundary

- Upstream: `ShiningForceCentral/SF2DISASM`, commit
  `c834c652b6862bc5679fd7f69a38a7093206efc6`.
- ROM: USA retail SHA-256
  `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
- The source denominator is exactly 17 pinned files. The fixture records their
  identities and 22 named H1/ROM intervals; no source bytes or game assets are
  public payload.
- It retains the accepted gameflow-core, field-menu-control, map-descriptions,
  common-stats, core-stats-data, item-auxiliary, and tech-interfaces fixtures.
  Those owners retain the complete map-description, gold/item, party, and
  interface algorithms rather than duplicating them here.

## Confirmed static spine

- FieldMenu clears `d6`, reaches `CheckArea` through `j_CheckArea`, and exits.
  The no-entity `ProcessPlayerAction` path sets `d6` to one, calls `CheckArea`
  directly, and branches on its nonzero return.
- A negative `VIEW_TARGET_ENTITY` returns. Otherwise the source derives the
  faced tile from entity X/Y and direction pixel offsets, divides by map tile
  size 384, indexes the 64-wide layout, and masks the word with `$3C00`.
- Dispatch order is chest `$1800`, vase `$2C00`, barrel `$3000`, bookshelf
  `$3400`, generic searchable `$1C00`, then map area-description fallback.
  The first kind uses `OpenChest`; the other four use `CheckNonChestItem`. Item
  classification masks with `$7F` and uses `ITEM_NOTHING` 127.
- The area-description service is called through its jump interface. A handled
  result closes text and returns `-1`; an unhandled `d6=1` path returns zero;
  an unhandled `d6=0` path uses only structural default text IDs 423 and 412,
  closes text, and returns `-1`.
- Gold content begins at 128: the value is reduced by 128, masked with `$7F`,
  doubled for a word index, and selects exactly 13 values from 10 through 130.
  Its static call order reaches `IncreaseGold`, item music, public text ID 414,
  and the retained input-wait service.
- Item handoff tests leader index zero first with capacity four. If full, it
  updates force membership, scans `OTHER_FORCE_MEMBERS_LIST` using
  `TARGETS_LIST_LENGTH-2` and `DBF`, and hands the item to the first eligible
  recipient. If no recipient is eligible, it calls `CloseChest` then
  `RefillNonChestItem`.

All statements above are source/H1/ROM static facts. They do not establish a
natural player route, input/presentation cadence, caller-visible state, or
post-search persistence.

RA08 remains a static graph with its accepted R2/R2a **NotReached** boundaries;
this slice makes no readiness, R4b, H4, or Phase 4 claim.

## Runtime question queue

No emulator was launched and this H2 slice creates no H3 fixture. The following
remain **Unknown**, in the fixture's ordered register:

1. `natural-search-reachability`
2. `actual-caller-entry-state`
3. `actual-view-target-entity`
4. `actual-facing-and-target-coordinate`
5. `actual-block-kind`
6. `actual-area-description-row-or-callback`
7. `actual-chest-or-nonchest-content`
8. `actual-gold-before-and-after`
9. `actual-item-recipient-and-capacity`
10. `actual-item-flag-open-close-refill-state`
11. `actual-return-code-and-caller-branch`
12. `input-text-sound-and-fade-cadence`
13. `persistence-after-map-switch-save-load`
14. `route-specific-search-outcome`
