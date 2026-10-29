# Battle Routing and Terrain Data Inventory

- Status: **Confirmed** for the eight-file inventory, seven layout-owned files and H1 addresses,
  cutscene slot counts, region routes, unused joins, terrain aliases, the complete terrain decode
  corpus, and excluded legacy aggregate
- Status: **Inferred** for cutscene route admission and flag lifecycle
- Status: **Unknown** for two grouped runtime questions
- Evidence date: 2026-07-19
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Build Boundary

This batch covers the six files under `data/battles/cutscenes` plus the two top-level aggregate
files `terrainentries.asm` and `spritesetentries.asm`. All six cutscene tables and the terrain table
are directly included by the original layout and have representative H1 addresses.

The top-level `spritesetentries.asm` is a 45-payload binary aggregate that defines the same symbols
as the maintained source-form `data/battles/spritesets/entries.asm` tree. The original layout does
not include it. It is therefore H2-hashed as an explicit alternate but does not receive a borrowed
H1 address or strict indexed-file credit. This gives 8/8 inventory and 7/8 strict reach.

## Static Shape

The before-battle, battle-start, enemy-defeated, and after-battle relative-pointer tables each have
48 slots, with 27, 1, 3, and 25 non-empty targets respectively. These counts agree with the built
cutscene inventory. The region table contains four longword routes followed by its terminator. The
unused after-battle join table has 52 byte slots and every slot is zero.

The terrain pointer table has 45 battle slots backed by 43 binary payloads. Slot 4 reuses terrain 3,
and slot 32 reuses terrain 27. The project-owned Stack decoder now resolves every pointer, verifies
the pointer table and all 43 compressed payloads against the ROM, and produces exactly one 48×48
grid (2,304 bytes) from each unique payload. Across the corpus, all 99,072 decoded bytes are terrain
types 0-8 or the obstructed value `0xFF`. The private terrain grids and legacy spriteset payloads
remain under the local checkout/output root and are never copied into the repository.

`LoadBattleTerrainData` indexes the table directly with `CURRENT_BATTLE * 4`, writes to the fixed
battle-terrain array, and calls the same Stack decoder. This closes alias selection and initial grid
construction statically. It does not prove cutscene caller admission, empty-slot fallback, flag
persistence, or repeatability; those meanings remain inferred or unknown.

## Concentrated Queue

No emulator was launched. Remaining questions are grouped as:

1. cutscene route admission and empty-slot fallback;
2. region-cutscene flag lifecycle and repeatability;

The two questions share the cutscene dispatcher boundary and should be exercised in one generated
matrix. Terrain no longer needs an emulator case merely to restate deterministic pointer selection
or decompression.

## Reproduction

```powershell
uv run sf2 h2 battle-routing-data
uv run sf2 h2 battle-terrain
uv run sf2 research-index test
```
