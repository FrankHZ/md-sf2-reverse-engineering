# Map-Event Direct Fixed-RAM State

- Status: **Confirmed** for the bounded source/H1/ROM direct-access inventory.
- Evidence date: 2026-08-25.
- ROM: USA retail SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
- Source: `ShiningForceCentral/SF2DISASM` `master` commit `c834c652b6862bc5679fd7f69a38a7093206efc6`.

`sf2-map-event-direct-state-static-v1` fresh-builds and validates the retained map-events and map-data
owners, then scans their complete 914 target-program contexts for raw-68000 direct fixed-RAM operands.
It finds 65 positive contexts (45 entity, 18 zone, 2 item) and proves the remaining 849 contain none.
There are 127 contextual instruction sites / 124 physical PCs and 152 contextual access edges / 148
physical edges (33 contextual reads / 119 contextual writes; 32 physical reads / 116 physical writes).
Map6_DefaultEntityEvent and Map6_EntityEvent13 retain their
three shared PCs and four shared edges as separate contexts while physical totals deduplicate them.

The 13 source-defined symbols are `CURRENT_PORTRAIT`, `CURRENT_SHOP_INDEX`, `CURRENT_SPEECH_SFX`,
`DIALOGUE_NAME_INDEX_1`, `EGRESS_MAP`, `ENTITY_FACING`, `EVENT_RELATIVE_POSITION`, `MAP_EVENT_TYPE`,
`MESSAGE_SPEED`, `RAFT_MAP`, `RAFT_X`, `RAFT_Y`, and `SPEECH_SFX_COPY`. `sf2const.asm` supplies each
address once; `sf2enums.asm` supplies immediate enum identities. The parser preserves `.b`/`.w`/`.l`,
operand order, source-shaped value kind, and H1/ROM instruction hash; it records eight longword
`MAP_EVENT_TYPE` writes without assigning state meaning.

This is only a direct source footprint, not normal execution, service effects, persistence, timing,
or story evidence. Provenance is `sf2const.asm`, `sf2enums.asm`, the 38 positive event-table source
identities, `build/sf2build-h1.lst`, and the private ROM. Reproduce with:

```powershell
uv run sf2 h2 map-event-direct-state
```

## Grouped H3 Runtime Questions

1. **Unknown:** normal-story-program-reachability; caller-entry-state; runtime-branch-and-access-order.
2. **Unknown:** runtime-before-after-values; callee-and-service-side-effects; cross-map-state-lifetime;
   save-load-persistence.
3. **Unknown:** input-text-portrait-audio-vint-timing; player-facing-story-meaning.
