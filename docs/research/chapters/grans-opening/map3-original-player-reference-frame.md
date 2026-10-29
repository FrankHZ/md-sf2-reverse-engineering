# Map 3 Original Player Reference Frame

- Status: **Confirmed static** for the bounded source/H1/ROM selection and import-shape contract
- Evidence date: 2026-09-02
- Fixture: `sf2-map3-original-player-reference-frame-static-v1`
- ROM: USA retail, SHA-256
  `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`
- Source baseline: `ShiningForceCentral/SF2DISASM` commit
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Boundary

This static H2 slice selects one public-safe original-player reference frame for a future private
importer. It joins the existing map-entity, map-sprite-assignment, regular map-sprite graphics, and
technical-graphics owners without reopening their complete corpora or publishing original graphics
or palette content. The tracked fixture contains identities, selection rules, dimensions, policy
labels, retained-owner links, and Unknowns only.

This slice defines no Application DTO or API. A future consumer must preserve the validated selection,
shape, palette-format, and policy labels, but its application-facing model belongs to a later owner.

## Controlled Player Identity

**Confirmed static:** `InitializeMapEntities` creates the controlled entity explicitly from ally 0
after the entity-list loop. The selection is not inferred from the order of Map 3 entity rows.

The derivation is source-shaped and ordered:

1. the controlled declaration clears the combatant selector to ally 0 before calling
   `GetAllyMapsprite`;
2. row 0 of `table_AllyMapsprites` has source value 1;
3. `GetAllyMapsprite` obtains class `CLASS_SDMN`, whose source value is 0, and applies its SDMN
   subtract-one transform;
4. the resulting regular map-sprite ID is 0; and
5. `DeclareNewEntity` stores that result in `ENTITYDEF_OFFSET_MAPSPRITE` while retaining the supplied
   facing.

These facts identify the controlled entity and stored map-sprite value. They do not establish a
visible frame at a later runtime checkpoint.

## Facing, Source Slot, and Mirror

**Confirmed static:** the source enums are `RIGHT=0`, `UP=1`, `LEFT=2`, and `DOWN=3`. The accepted
facing transform, zero special case, three-slot pointer arithmetic, and sprite mirror table produce:

| Direction | Facing | Source slot | Horizontal mirror |
| --- | ---: | ---: | --- |
| UP | 1 | 0 | no |
| LEFT | 2 | 1 | no |
| RIGHT | 0 | 1 | yes |
| DOWN | 3 | 2 | no |

The controlled facing is 3 (`DOWN`), so regular map-sprite ID 0 selects source slot 2,
`Mapsprite000_2`. The RIGHT transform is a bounded horizontal-mirror import rule; it does not claim
rendered output parity.

## Decoded Frame Shape and Import Policy

**Confirmed static:** the selected `Mapsprite000_2` Basic stream matches its accepted source, H1,
and ROM identity and decodes without a trailing input remainder to 576 bytes. The decoded output is
two contiguous 288-byte halves. Each half is a 24×24-pixel, 3×3-tile, 4bpp frame, with 32 bytes per
tile and column-major tile placement.

**Project import policy:** half 0 is labeled `initial-reference-frame`. The choice is rooted in the
source initialization of the controlled entity's animation counter and the first-frame branch in
`VInt_UpdateSprites`. It is not an observed standing or idle frame, and this slice does not claim
that half 0 was visible at the first `WaitForEvent`. Original idle/walk cadence remains deferred.

## Palette and Pixel Policy

**Confirmed static:** ordinary entity sprites use the exact source symbol `palette_Base`.
`InitializeDisplay` copies that palette to palette 3. The selected palette matches accepted source,
H1, and ROM identity.

The bounded importer format policy may retain:

- 16 big-endian words in 32 encoded bytes;
- the `0x0EEE` word mask;
- transparent palette index 0;
- high-nibble-left pixels inside each 4bpp byte;
- column-major tiles; and
- a horizontal mirror for RIGHT.

RGB channel expansion is explicitly a **project/inferred rendering policy**, not evidence of original
display parity. Screenshot and broader visual-fidelity parity remain deferred.

## Exact Unknown Register

The public fixture preserves exactly these runtime questions:

- `admissionAnimCounter`: **Unknown**
- `admissionVisibleFrame`: **Unknown**
- `livePalette3AtAdmission`: **Unknown**
- `originalRenderedColorParity`: **Unknown**
- `movementFacingTiming`: **Unknown**
- `dmaCacheCompletion`: **Unknown**

No item in this register is promoted by the deterministic import policy. In particular, the selected
half, RGB expansion, and RIGHT mirror rules do not close admission visibility, live palette state,
DMA completion, animation cadence, screenshots, or original-rendered fidelity.

## Private Boundary and Reproduction

The verifier reads only the repository-authorized ignored ROM, pinned upstream checkout, and H1
outputs. It fail-closes the relevant source sections, selected pointer identity, source/H1/ROM seams,
Basic decode terminator and output shape, and palette source/destination relation before comparing the
public fixture. The tracked fixture, manifest, schemas, documentation, index, logs, and PR handoff must
not contain compressed, decoded, frame, or palette bytes; PNGs; private absolute paths; private resource
ranges; per-resource hashes; or payload commitments.

Reproduce the bounded contract with:

```powershell
uv run sf2 h2 map3-original-player-reference-frame
uv run pytest tests/python/test_map3_original_player_reference_frame.py
uv run sf2 research-index test
```

The default command writes only a public-safe ignored JSON projection. It does not export an image or
an original asset.
