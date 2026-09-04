# Map 3 Entity 142 Interactable Reference

- Status: **Confirmed** for the bounded source/H1/ROM identity and route-specific logical-to-physical
  mapping; **Confirmed retained runtime** for the already accepted interaction route
- Evidence date: 2026-09-04
- Fixture: `sf2-map3-entity142-interactable-reference-static-v1`
- ROM: USA retail, SHA-256
  `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`
- Source baseline: `ShiningForceCentral/SF2DISASM` commit
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Boundary

This H2 owner joins one Map 3 interactable entity to a public-safe two-half drawable reference. It
does not run a new emulator observation and does not select an animation half. The runtime portion is
only a checked reference to the already accepted natural opening route.

The contract defines no Application DTO, API, exported image, or distributable original asset. It
records exact identities, small source records, hashes, dimensions, and selection rules needed by a
future authorized private importer.

## Source Record and Entity Identity

**Confirmed:** one-based Map 3 source record 17 (zero-based index 16) starts at ROM `0x50BB0` and is
the eight-byte record `36 11 01 D1 00 04 60 CE`. Its raw and masked coordinates are `(54,17)`, facing
is `UP=1`, and map-sprite is `MAPSPRITE_ASTRAL=209`. Its fixed tail names `eas_Init` in source; this
slice retains the tail bytes but leaves broader tail semantics **Unknown**.

The route-specific logical-to-physical mapping is also **Confirmed static**:

1. `InitializeMapEntities` starts follower allocation at physical slot 1.
2. Under the accepted opening-route follower state, flag 66 declares Sarah and Chester into slots 1
   and 2 and records their ally mappings.
3. The first two Map 3 source rows reuse those mappings and therefore do not allocate again.
4. Source rows 3 through 17 allocate sequentially from physical slot 3 through slot 17.
5. The regular non-ally mapping region starts at `ENTITY_INDEX_LIST+32`. Its raw negative-byte
   character encoding adds `ENTITY_ENEMY_INDEX_DIFFERENCE=96`, so offset 46 resolves to logical ID
   142. The byte at that offset points to physical slot 17.
6. `DeclareNewEntity` addresses slot 17 as `ENTITY_DATA + 17*32`.

Logical entity 142 must not be used as a physical `ENTITY_DATA` slot. A discarded diagnostic probe
made exactly that mistake and stopped at the entity-event callback after reaching
`Map3_EntityEvent15`; cleanup and restoration passed, but the failed probe contributes no positive
runtime evidence and was not rerun.

## Interaction Join

**Confirmed static:** zero-based Map 3 entity-event record 15 is the four-byte record
`8E 03 01 34` at ROM `0x50F4C`. It selects logical entity 142, requires player facing `DOWN=3`, and
targets `Map3_EntityEvent15` at `0x51044`.

**Confirmed retained runtime:** the accepted natural-route fixture places the player at `(55,17)`
facing Left and entity 142 at `(54,17)` facing Up. Its observed chronology reaches
`ProcessPlayerAction`, `GetActivatedEntity`, the original `RunMapSetupEntityEvent` dispatch with
`D0=142`, and then `Map3_EntityEvent15`. This owner validates that fixture and chronology without
launching BizHawk.

## Two-Half Drawable Reference

**Confirmed static:** `MAPSPRITE_ASTRAL=209` facing Up selects pointer slot 627 and
`Mapsprite209_0`. The Basic-compressed source matches H1 and ROM, consumes 406 bytes, and decodes
without remainder to 576 bytes. The result is exactly two contiguous 288-byte halves, each a 24×24,
3×3-tile, 4bpp frame with high-nibble-left pixels and column-major tile placement.

The ordinary entity palette is `palette_Base`; `InitializeDisplay` copies it to palette 3. Its 32
bytes form 16 big-endian words under mask `0x0EEE`, with palette index 0 transparent.

This is an asset-ready **two-half reference** only. Both half hashes are retained so an authorized
private importer can reproduce either half, but neither is promoted to the observed visible frame.

## Unknown Register

- `interactionTimeAnimCounter`: **Unknown**
- `selectedVisibleHalf`: **Unknown**
- `exactObservedFrameOrAnimationContract`: **Unknown**

Static initialization shape, half order, or deterministic import policy must not close these runtime
questions.

## Reproduction

```powershell
uv run sf2 h2 map3-entity142-interactable-reference
uv run pytest tests/python/test_map3_entity142_interactable_reference.py
uv run sf2 research-index test
```

The H2 command writes only an ignored JSON projection. It does not export decoded graphics, palette
bytes, images, screenshots, or emulator state.
