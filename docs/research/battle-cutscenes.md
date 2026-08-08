# Battle Cutscene Routing

- Status: **Confirmed** for the pinned ten-file inventory, intro/after/defeated gating, leader-death
  position preparation, and region-cutscene admission order
- Status: **Inferred** for story meaning encoded only in upstream table names/comments
- Status: **Unknown** for individual map-script content and rendered sequencing
- Evidence date: 2026-08-08
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Confirmed Routing

All ten files under `code/gameflow/battle/cutscenes` are H1-bound. The before-battle and battle-start
wrappers share the intro flag; before-battle checks but does not set it, while battle-start sets it
before dispatch. After-battle skips its map script when the battle-completed flag is already set,
then reaches a shared per-battle join-table call.

The defeated wrapper requires Bowie alive, enemy slot 128 dead, and the completion flag clear. Its
shared tail can append every still-living enemy to the dead-combatant list when the per-battle leader
flag is present. The leader-death position listener has the same life/death gate and scans six-byte
battle records. Its DBF counter is `COMBATANT_ALLIES_COUNTER` (29), so it executes 30 iterations for
ally indexes 0..29. Each iteration writes X=`-1` to that ally and to its enemy-bit-mapped slot
128..157, then writes current HP=`0` to that same enemy slot. The `SetCombatantX` and `SetCurrentHp`
helpers consume `d1` and preserve it through their shared combatant-entry helper. After the loop,
slots 158 and 159 receive X=`0` (the loop's last `d1` value) through two X-only calls; no
`SetCurrentHp` occurs before the position-table pointer load. Thus this function neither moves every
enemy slot offscreen nor zeros every enemy HP slot. The later four-byte position-record loop is a
separate bounded phase and does not establish final HP or caller-visible results. A dead-list write
after an unconditional branch is retained as explicitly unreachable code.

This is a **Confirmed** H2 source/dataflow boundary from
`code/gameflow/battle/cutscenes/afterenemyleaderdeathpositions.asm`, the combatant definitions in
`sf2enums.asm`, the `SetCombatantX`/`SetCurrentHp` wrappers in
`code/common/stats/combatantstats_2.asm`, and their byte/word/address helpers in
`code/common/stats/combatantstats_3.asm`. The local jump-interface aliases are checked in
`code/common/tech/jumpinterfaces/s02_jumpinterface.asm`. No H3 observation is claimed.

Region cutscenes scan eight-byte records until `-1`, then test current battle, played flag, and region
trigger in that order. The played flag is set before the `MAPSCRIPT` trap. Table contents and the map
scripts themselves remain separate data/scripting batches; this routing batch adds no emulator run.

## Reproduction

```powershell
uv run sf2 h2 battle-cutscenes
uv run sf2 research-index test
```

Generated JSON stays under ignored `local/derived/battle-cutscenes-static.json`.
