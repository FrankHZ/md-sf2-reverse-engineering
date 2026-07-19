# Battle Scene Engine

- Status: **Confirmed** for the pinned twelve-file root inventory, 21-command scene-script
  dispatch, scene initialization order, actor/weapon/background selectors, and 32-entry spell
  setup/update dispatch tables
- Status: **Inferred** for the player-visible intent of named tint and graphics helpers where only
  static call structure has been reproduced
- Status: **Unknown** for exact frame timing, interrupt/VDP effects, and the visual output of the
  55 animation implementation files
- Evidence date: 2026-07-19
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Confirmed Engine Boundary

The twelve root files under `code/gameflow/battle/battlescenes` contain 6,261 source lines, 387
global labels, and 376 direct call sites. The rail deliberately excludes the 55 descendants under
`animation/`: those implementations form the next semantic batch rather than receiving coverage
credit from directory proximity.

`ExecuteBattlesceneScript` reads word commands from `$FF0000`, clears and seeds the dead-combatant
list, terminates on `$FFFF`, dispatches through a 21-entry relative jump table, and returns zero.
The command set covers actor action/position transitions, actor switches, reactions, idle/end/sleep,
EXP award, message display/close/input wait, and one null command.

Scene initialization clears scene state, resolves enemy and ally graphics, weapon, and background,
clears existing VInts, loads palettes/layouts, conditionally loads enemy/ally/ground/weapon layers,
adds graphics and window VInts, loads status tiles, applies status animation state, then fades in.
Weapon graphics are ally-only and use `-1/-1` for an invalid sprite/palette. Background selection
prioritizes Zeon, then battle-specific overrides, then terrain. Ally animation selection contains
explicit spear/javelin handling for KNTE, PLDN, and PGNT and a separate dodge block.

Spell animation setup and update each have 32 dispatch entries. Setup preserves the mirror bit and
stores the decoded variant as one-based; both disabled setup and index `-1` return without dispatch.
The update path requires its toggle and phase state before jumping to the selected updater.

## Runtime Queue

Exact frame duration, palette-transition appearance, VInt/VDP write effects, and setup/update visual
pairing cannot be established from names or call graphs alone. They remain one grouped presentation
matrix to run after the 55 animation files have been statically paired and classified. This batch
adds no emulator launch.

## Reproduction

```powershell
uv run sf2 h2 battle-scene-engine
uv run sf2 research-index test
```

Generated JSON stays under ignored `local/derived/battle-scene-engine-static.json`.
