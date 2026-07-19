# Shared Battle Functions

- Status: **Confirmed** for the pinned seven-file inventory, individual-turn control routing, Kiwi
  Flame Breath conversion, Egress/Angel Wing exits, battle load ordering, and move-SFX selection
- Status: **Inferred** for cursor/pulsating-grid helper roles based only on upstream names/comments
- Status: **Unknown** for the deeper player-input/cursor state machine and runtime presentation timing
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

The large cursor/grid and player-input files are hash/call inventoried, but their deeper branch state
machine is explicitly queued for subsequent static passes.

## Reproduction

```powershell
uv run sf2 h2 battle-functions
uv run sf2 research-index test
```

Generated JSON stays under ignored `local/derived/battle-functions-static.json`.
