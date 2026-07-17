# Battle 01 Placement and AI-Region Contract

- Status: **Confirmed storage contract; activation behavior pending H3**
- Evidence date: 2026-07-17
- Battle: ID 1, `INSIDE_ANCIENT_TOWER`
- ROM: USA retail, SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`
- Source baseline: `ShiningForceCentral/SF2DISASM` commit
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Result and Reproduction

```powershell
pwsh ./scripts/Test-Battle01Extraction.ps1
```

The pinned assembly source and an independent ROM decoder produce deterministic schema-valid
documents with fixed hashes. The verifier compares 148 structured values with zero mismatch. Full
generated placement data remains under ignored `local/derived/`.

## Table Layout

`data/battles/spritesets/spriteset01.asm` owns ROM range `0x1B32E2..0x1B3376`, exactly 148 bytes.
The first four bytes contain ally, enemy, AI-region, and AI-point counts. They are followed by
nine fixed 12-byte entity records, then three variable-length region polygons. This battle has no AI
points.

Each entity record stores:

| Offset | Field |
| --- | --- |
| `0` | ally slot or enemy-definition ID |
| `1..2` | starting X/Y |
| `3` | AI command-set ID |
| `4..5` | big-endian item bitfield |
| `6..9` | primary order/region and secondary order/region |
| `10` | source-labeled filler byte |
| `11` | spawn/initialization setting |

Each region begins with a vertex count and one source-unlabeled byte, followed by X/Y vertex pairs
and two trailing bytes. All three Battle 01 regions contain four vertices.

## Confirmed Placement Facts

- Three ally slots (0–2) start along the lower area at `(8,18)`, `(9,18)`, and `(7,18)`.
- Six enemy entities all reference enemy definition 39 (`GIZMO`). Their baseline record is already
  covered by [`enemy-promotions.md`](./enemy-promotions.md): level 0, HP 5, ATT 7, DEF/AGI/MOV 5,
  hovering movement.
- Four entities use `ATTACKER1`; the remaining two use `ATTACKER2`.
- All six use primary and secondary order `NONE`, but their primary activation regions are split
  across regions 2, 1, and 0. Secondary region is 15 for each enemy.
- Every entity is marked `STARTING`; no battle-level AI point entries are stored.
- Entity items are `NOTHING`. The last source-labeled filler byte is 96 for the first four enemies
  and 112 for the final two; its gameplay meaning remains unknown and is not renamed by the schema.

## Region Polygons

The three confirmed quadrilaterals are retained as ordered vertex lists:

1. `(0,0) → (0,19) → (15,7) → (15,0)`
2. `(0,0) → (0,7) → (15,19) → (15,0)`
3. `(0,0) → (0,12) → (15,12) → (15,0)`

The storage contract deliberately calls these ordered polygons and activation-region references. It
does not yet claim which boundary rule is used, when a combatant activates, how overlaps resolve, or
whether the two trailing bytes participate in runtime logic.

## Scope Boundary and H3 Entry

This slice does not decode `battle01/terrain.bin`, map-to-battle coordinates, cutscenes, victory
conditions, background selection, enemy upgrades, difficulty adjustment, or the AI command-set
programs themselves. Those are separate dependencies rather than hidden fields of the spriteset.

Battle 01 is now the preferred first scripted battle scenario: its six identical low-AGI enemies
make definition lookup constant while region selection and command-set differences remain visible.
The next runtime fixture should observe combatant initialization and the first generated turn-order
array, then add movement across one region boundary.
