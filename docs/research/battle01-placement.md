# Battle 01 Static Scene Contract

- Status: **Confirmed static scene contract, first-turn initialization, and region activation**
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
pwsh ./scripts/Test-H3Battle01RegionActivationFixture.ps1
pwsh ./scripts/Test-H3Battle01SecondaryActivationFixture.ps1
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

The runtime path splits a quadrilateral into triangles `(1,2,4)` and `(3,2,4)`. It tests each living,
placed ally and treats a point on an edge as inside. If any ally is inside, the corresponding global
battle-region flag is set. The two source-unlabeled trailing bytes are not read by this predicate.

## Victory/Defeat and First Turn

`CountRemainingCombatants` at `0x23C58` counts only placed, living units. Battle victory occurs when
that enemy count reaches zero. Defeat occurs when no living placed ally remains, and is also forced
when combatant 0 (Bowie) has zero HP even if another ally survives. Battle 01 does not override this
generic loop with a leader-victory condition.

The H3 fixture enters the original built-in Debug Battle Test using real controller input, chooses
Battle 01, then uses Right+C at the second number prompt to select nonzero option 1. The original
`DebugModeBattleTest` flow adds `BATTLE_INTRO_CUTSCENE_FLAGS_START` to the selected battle and sets
that shared flag. The original before-battle and battle-start cutscene wrappers then see the flag and
skip their scripts; normal Battle 01/combatant initialization still runs. Player 2 Start remains in
the harness as a fallback during later battle-scene playback, not as the before-battle cutscene
skip mechanism. After the original game initializes combatants, the harness fixes only
`RANDOM_SEED` to `0x1234` at `GenerateBattleTurnOrder` (`0x25544`). At `0x2559E`, the sorted list contains exactly three allies
(0, 2, 1) and six enemies (128–133), proving unplaced joined allies are skipped and all six stored
Gizmos are live participants. Exact randomized scores are committed in
`tests/fixtures/h3/battle01-turn-order-v1.json`.

The companion turn-order boundary fixture reuses the same initialized scene and confirms the generic filters:
a dead placed ally and a living unplaced enemy are omitted, while AGI 128 adds a second turn. It also
locks the original signed-byte sort behavior at AGI 127/128; see
[`runtime-rng-and-battle-math.md`](./runtime-rng-and-battle-math.md).

## Region Activation Runtime

`ActivateEnemies` at `0x2550C` evaluates the three region polygons before the first turn-order pass.
The baseline H3 snapshot at `0x25544` confirms that allies `(8,18)`, `(9,18)`, and `(7,18)` trigger
none of them. The three global flags remain clear and enemy 128–133 activation bitfields remain,
in order, `0x2060`, `0x2060`, `0x2060`, `0x2060`, `0x2070`, and `0x2070`. The separate
`NEWLY_TRIGGERED_BATTLE_REGIONS` scan bitfield is `0b111`: it records that each polygon was tested,
not that each polygon triggered.

The controlled fixture changes only Bowie’s initialized X/Y immediately before `ActivateEnemies`,
from `(8,18)` to `(8,12)`. That point is on region 2’s horizontal edge and inside regions 0 and 1.
The original ROM sets all three battle-region flags, and the six enemy bitfields become `0x2061`
for enemies 128–131 and `0x2071` for 132–133. Thus activation preserves every existing AI field and
sets only bit 0 (`PRIMARY_ACTIVE`) for enemies whose primary region is active. A project-owned
cross-product model independently evaluates the same two positions and source-extracted polygons
before BizHawk launches.

All six natural secondary region values are 15 (`NONE`). The companion controlled fixture therefore
changes enemy 128 only at `ActivateEnemies` entry: its packed trigger byte goes from `0x2F`
(`primary=2, secondary=NONE`) to `0xF2` (`primary=NONE, secondary=2`). Bowie remains at `(8,12)`, so
all region flags become active. The original `TriggerRegionsAndActivateEnemies` secondary branch at
`0x1ACE4A` changes that enemy's bitfield from `0x2060` to `0x2063`, setting both `PRIMARY_ACTIVE`
and `SECONDARY_ACTIVE`. The other five enemies retain the primary-only results `0x2061`/`0x2071`.
This confirms that secondary activation deliberately enables both mode bits rather than selecting
only bit 1. First-round evaluation is covered; later-round clearing of the scan bitfield and region
cutscene timing remain open.

Cutscene command semantics, AI command-set programs, natural Battle 01 secondary-region data,
later-round region state, map graphics, and terrain-to-map-block rendering remain outside this contract.
