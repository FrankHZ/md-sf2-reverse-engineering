# Promotions and Enemy Definitions

- Status: **Confirmed storage contract and static consumers; runtime scenarios pending**
- Evidence date: 2026-07-17
- ROM: USA retail, SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`
- Source baseline: `ShiningForceCentral/SF2DISASM` commit
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Reproduction

```powershell
pwsh ./scripts/Test-EnemyPromotionExtraction.ps1
```

The verifier exports the pinned assembly contract and independently decodes the locked ROM, validates
both schemas and golden hashes, repeats both exports, then compares 2,722 fields with zero mismatch.
Generated names and full records remain under ignored `local/derived/`.

## ROM Tables

| Table | Range `[start,end)` | Encoding | Records |
| --- | --- | --- | --- |
| promotions | `0x21046..0x21072` | five length-prefixed byte lists | 5 sections / 39 values |
| enemy names | `0xFB8A..0xFF87` | length byte followed by ASCII payload | 103 |
| enemy definitions | `0x1B1A66..0x1B30EE` | fixed 56-byte records, big-endian words | 103 |

The source paths are `data/stats/allies/classes/promotions.asm`,
`data/stats/enemies/enemynames.asm`, and `data/stats/enemies/enemydefs.asm`. Enum resolution comes from
`sf2enums.asm`. The extraction manifest pins all four source hashes.

## Confirmed: Promotion Table and Church Mapping

The five sections are positional parallel lists:

1. 12 regular base classes;
2. 12 corresponding regular promoted classes;
3. 5 special-promotion base classes;
4. 5 corresponding special promoted classes;
5. 5 corresponding special-promotion items.

`FindPromotionSection` walks each length-prefixed section. `GetPromotionData` returns the matching
position within a base-class section, and the church code uses that same position in the target-class
and item sections. The minimum church promotion level is 20.

Static church logic additionally confirms:

- The special item search scans items held by the current force, not only the member being promoted;
  the item is removed from its actual holder after confirmation.
- SORC promotion clears the member's four spell slots and grants DAO.
- MMNK and NINJ promotion unequip the current weapon because their weapon type changes.
- Regular and special target-class relations are data-driven by the five lists, while prompts,
  consumption, spell replacement, and weapon handling remain executable policy.

These are source/ROM facts. Menu cancellation, inventory edge cases, and complete promotion stat
effects still need H3 scenarios before becoming remake acceptance tests.

## Confirmed: Enemy Definition Layout

Each 56-byte definition is copied as 14 longwords into a spawning combatant entry. The decoded
fields are:

| Offset | Field | Storage |
| --- | --- | --- |
| `0` | source-labeled unknown byte | byte |
| `10` | spell-power mode | byte |
| `11` | level | byte |
| `12` | maximum HP | big-endian word |
| `16` | maximum MP | byte |
| `18,20,22,24` | base ATT, DEF, AGI, MOV | bytes with padding |
| `26` | resistance | big-endian word |
| `30` | prowess | byte |
| `32` | four items | four big-endian words |
| `40` | four spells | four packed bytes |
| `44` | initial status | big-endian word |
| `49` | movement type | upper nibble |
| `52` | AI bitfield | big-endian word |

All 27 reserved/padding bytes in every locked record are zero. Items use the seven-bit item ID plus
the equipped bit. Spells use a six-bit spell ID plus a two-bit level. Twelve definitions have AGI at
or above 128, so they are direct second-turn candidates for the turn-order H3 queue. All 103 stored
initial-status fields are `NONE`; this does not prove enemies can never acquire status at runtime.

`InitializeEnemyStats` first calls `UpgradeRandomBattleEnemies`, then copies the selected definition,
sets current HP/MP from maxima, merges movement type with the battle entity's AI command set, applies
battle placement/orders, and finally calls `AdjustEnemyBaseAttForDifficulty`. Therefore a definition
record is a spawn baseline, not necessarily the final on-map stat block.

## Confirmed Data Anomaly

Enemy ID 99 stores a length-four name payload containing ASCII `JAR` followed by a null byte. The
source explicitly labels this as the JAR typo/bug. The independent ROM decoder verifies the trailing
null; its exact renderer-visible consequences remain a separate behavioral question.

## Contract Boundary and Next Fixtures

- Promotion mappings and the 56-byte enemy spawn baseline are safe canonical inputs for a remake data
  layer once enum names are converted to project-owned stable IDs.
- Enemy upgrade selection, difficulty adjustment, AI command-set merging, and second-turn behavior
  must be modeled as transformations, not flattened into the base record.
- Next H3 cases should use one AGI-below-128 and one AGI-above-128 enemy, plus a promotion case that
  distinguishes the promotee from the special-item holder.
- Battle sprites, map sprites, gold, drop tables, battle placement, and enemy-upgrade ranges are not
  part of this contract yet.
