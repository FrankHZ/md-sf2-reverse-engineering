# Battle 01 Static Scene Contract

- Status: **Confirmed static scene contract and first-turn initialization; region activation pending H3**
- Evidence date: 2026-07-17
- Battle: ID 1, `INSIDE_ANCIENT_TOWER`
- ROM: USA retail, SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`
- Source baseline: `ShiningForceCentral/SF2DISASM` commit
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Result and Reproduction

```powershell
pwsh ./scripts/Test-Battle01Extraction.ps1
pwsh ./scripts/Test-Battle01SceneExtraction.ps1
pwsh ./scripts/Test-H3Battle01TurnOrderFixture.ps1
```

The pinned assembly source and an independent ROM decoder produce deterministic schema-valid
documents with fixed hashes. The placement verifier compares 148 structured values with zero
mismatch; the scene verifier independently follows the ROM terrain pointer, decompresses the Stack
bitstream, and compares map/global metadata plus hashes and value counts. Full generated placement
and terrain data remain under ignored `local/derived/`.

## Map, Terrain, and Global Scene Metadata

Battle 01 uses map 57 (`MAP_ANCIENT_TOWER_ENTRANCE`). Its battle area starts at `(0,0)`, is 16×20
spaces, and stores trigger X/Y as 255 (`any`). The map link is the second seven-byte record in
`table_BattleMapCoordinates` at `0x7A36`.

The terrain pointer table starts at `0x1AD104`; Battle 01 points to the 284 compressed bytes at
`0x1AD344..0x1AD460`. The project-owned decoder reproduces `LoadStackCompressedData` and emits the
full 2,304-byte (48×48) `BATTLE_TERRAIN_ARRAY`. The source split and direct ROM slice both have
compressed SHA-256 `A0E6B0D4...DAABC4A`; the decompressed array has SHA-256
`ECA7CDDA...453C835`.

Only five stored values occur: 2,078 obstructed cells (`255`), 102 low-sky cells (`0`), 108 plains
cells (`1`), 11 road cells (`2`), and 5 grass cells (`3`). These values are original terrain-array
bytes; occupancy bits are added or cleared later at runtime and are not baked into the file.

Global tables add three confirmed Battle 01 facts:

- `table_CustomBackgrounds[1] = TOWER_INTERIOR` (ID 9), so battle scenes do not fall through to the
  per-terrain background table.
- `table_HalvedExpEarnedBattles` contains `INSIDE_ANCIENT_TOWER`.
- `table_EnemyLeaderPresentFlags[1] = 0`. This table controls defeated-cutscene cleanup; it is not a
  separate “kill the leader” victory rule.

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

## Victory/Defeat and First Turn

`CountRemainingCombatants` at `0x23C58` counts only placed, living units. Battle victory occurs when
that enemy count reaches zero. Defeat occurs when no living placed ally remains, and is also forced
when combatant 0 (Bowie) has zero HP even if another ally survives. Battle 01 does not override this
generic loop with a leader-victory condition.

The H3 fixture enters the original built-in Debug Battle Test using real controller input, chooses
Battle 01, and skips cutscene text with the original Player 2 Start behavior. After the original game
initializes combatants, the harness fixes only `RANDOM_SEED` to `0x1234` at
`GenerateBattleTurnOrder` (`0x25544`). At `0x2559E`, the sorted list contains exactly three allies
(0, 2, 1) and six enemies (128–133), proving unplaced joined allies are skipped and all six stored
Gizmos are live participants. Exact randomized scores are committed in
`tests/fixtures/h3/battle01-turn-order-v1.json`.

Cutscene command semantics, AI command-set programs, region boundary/overlap behavior, map graphics,
and terrain-to-map-block rendering remain outside this contract. The next Battle 01 runtime fixture
should cross one activation-region boundary and observe the affected enemy state.
