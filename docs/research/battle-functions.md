# Shared Battle Functions

- Status: **Confirmed (static)** for the pinned seven-file inventory, individual-turn control
  routing, Kiwi Flame Breath conversion, Egress/Angel Wing exits, battle load ordering, move-SFX
  selection, and the selected player-input/cursor/menu control-flow contract
- Status: **Inferred** for unmodeled pulsating-grid presentation helper roles based on upstream
  names/comments
- Status: **Unknown** for runtime input timing, frame/presentation behavior, and remaining ailment
  subroutes
- Evidence date: 2026-07-18
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Confirmed Control Flow

The seven files under `code/gameflow/battle/battlefunctions` are now fully inventoried and H1-bound.
`ExecuteIndividualTurn` skips dead actors; MUDDLE, the AI-controlled bit, ally auto-battle, and normal
enemy control route through AI, while the opponent-control toggle can hand an enemy to the player.
SLEEP, STUN, and STAY consume the action without a battlescene. Ordinary actions write and execute a
battlescene, end it, then reload the battlefield.

EGRESS and Angel Wing exit before battlescene construction. Angel Wing removes its item; EGRESS
deducts the spell-definition MP cost. Both close battlefield windows, update the repeat-battle
unlocked flag, obtain the egress position, and return `D4=0`.

Kiwi's MNST physical attack calls `RNG(4)` and converts to KIWI spell level 0–3 only on result zero.
Level thresholds are 32, 40, and 50. Static instruction order establishes the conversion; it remains
a good candidate for a future grouped special-action runtime matrix rather than a standalone launch.

`LoadBattle` orders fade-out, tilesets, entity positioning, sprite/map/entity-sprite loading, battle
VInts, terrain decompression, music, then fade-in; Fairy Woods additionally opens its timer. Move SFX
is zero outside battle and walking in battle, but Chirrup Sandals override either state with BLOAB.

## Confirmed Player-Control State Machine

The H2 extractor now owns six entry points across nine source ranges: `ControlCursorEntity`,
`ControlCursorEntity_ChooseTarget`, `SetCursorDestinationToNextBattleEntity`,
`ProcessBattleEntityControlPlayerInput` plus its three split function chunks, `BattlefieldMenu`, and
`PerformAiTargetingVisualAct`. Their 1,039 parsed statements contain 231 branch sites, 207 direct
call sites to 84 unique targets, 59 referenced global states, and all eight directional/action input
bits. Six H1 symbol addresses bind the catalog back to the rebuilt ROM listing.

Static instruction order confirms these implementation-neutral decisions:

- cursor movement accepts A, B, or C as tile confirmation, stores the chosen coordinates, and hides
  the cursor;
- target selection returns `-1` for an empty list, uses B to cancel and A/C to confirm, wraps through
  candidates in all four directions, and returns a combatant index;
- the battle diamond menu selects attack, magic, item, or search/stay; cancel restores the original
  position and leaves action `-1`; committed outcomes include attack, spell, item, stay, and trapped
  chest, while the item menu indices are use, give, equip, and drop;
- the battlefield menu selects members, minimap, options, or suspend. Battle 0 rejects suspend;
  normal suspend copies the seconds counter, sets flag 88, saves, and transfers to `WitchSuspend`,
  while the debug Start path returns to the menu after saving.

These are deterministic source-shape and branch-order facts, not emulator evidence about key-repeat
cadence, animation duration, VInt timing, or on-screen presentation. Those questions remain grouped
for a later shared BizHawk matrix rather than one launch per branch.

## Reproduction

```powershell
uv run sf2 h2 battle-functions
uv run sf2 research-index test
```

Generated JSON stays under ignored `local/derived/battle-functions-static.json`.
