# Battle Cutscene Routing

- Status: **Confirmed** for the pinned ten-file inventory, intro/after/defeated gating, leader-death
  position preparation, and region-cutscene admission order
- Status: **Inferred** for story meaning encoded only in upstream table names/comments
- Status: **Unknown** for individual map-script content and rendered sequencing
- Evidence date: 2026-07-19
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Confirmed Routing

All ten files under `code/gameflow/battle/cutscenes` are H1-bound. The before-battle and battle-start
wrappers share the intro flag; before-battle checks but does not set it, while battle-start sets it
before dispatch. After-battle skips its map script when the battle-completed flag is already set,
then reaches a shared per-battle join-table call.

The defeated wrapper requires Bowie alive, enemy slot 128 dead, and the completion flag clear. Its
shared tail can append every still-living enemy to the dead-combatant list when the per-battle leader
flag is present. The leader-death position listener has the same life/death gate, scans six-byte
battle records, moves every ally/enemy slot offscreen, zeros enemy HP, then applies four-byte position
records. A dead-list write after an unconditional branch is retained as explicitly unreachable code.

Region cutscenes scan eight-byte records until `-1`, then test current battle, played flag, and region
trigger in that order. The played flag is set before the `MAPSCRIPT` trap. Table contents and the map
scripts themselves remain separate data/scripting batches; this routing batch adds no emulator run.

## Reproduction

```powershell
uv run sf2 h2 battle-cutscenes
uv run sf2 research-index test
```

Generated JSON stays under ignored `local/derived/battle-cutscenes-static.json`.
