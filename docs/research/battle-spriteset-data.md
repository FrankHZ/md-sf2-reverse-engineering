# Battle Spriteset Data Inventory

- Status: **Confirmed** for the complete 46-file directory, pointer/include topology, H1 addresses,
  header totals/ranges, and aggregate combatant macro counts
- Status: **Inferred** for non-Battle 01 integration with battle routing and delayed activation
- Status: **Unknown** for three grouped runtime questions
- Evidence date: 2026-07-19
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Complete Build Boundary

`data/battles/spritesets` contains 46 ASM files and 6,628 lines. The original layout directly
includes `entries.asm`; its 45 longword slots point in order to `BattleSpriteset00` through
`BattleSpriteset44`, and it transitively includes the corresponding 45 files. Every file has its own
representative H1 symbol and address, so this directory reaches 46/46 strict indexed-file coverage
without an emulator launch or an invented extra battle slot.

## Static Shape

The 45 headers declare 500 ally placements, 627 enemy placements, 133 AI regions, and 37 AI points.
Per-slot ranges are 3–13 allies, 6–20 enemies, 0–14 regions, and 0–2 points. The parser independently
counts 500 `allyCombatant`, 627 `enemyCombatant`, and 1,127 each of `combatantAiAndItem` and
`combatantBehavior`, and rejects disagreement between the header and macro bodies.

These are structural facts, not a claim that every roster choice, coordinate, AI-region polygon,
item, activation value, follow target, or hidden-spawn transition has been behaviorally explained.
Battle 01 remains owned by its deeper source/ROM placement rail. Full row content stays in ignored
`local/derived/battle-spriteset-data-static.json`; the tracked fixture contains aggregate counts and
addresses rather than redistributing the source tables.

## Concentrated Queue

No emulator was launched. Remaining questions are grouped as:

1. roster, placement, and AI-region integration for battles beyond Battle 01;
2. selection of the 45 slots across map and story routing;
3. hidden/delayed spawns and follow-target state transitions.

The first and third groups share battle initialization and turn/region observation points and should
be exercised together in a later generated case matrix.

## Reproduction

```powershell
uv run sf2 h2 battle-spriteset-data
uv run sf2 research-index test
```
