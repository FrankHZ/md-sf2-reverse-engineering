# Promotions, Enemy Definitions, and Rewards

- Status: **Confirmed storage contract, static consumers, and core drop runtime branches**
- Evidence date: 2026-07-18
- ROM: USA retail, SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`
- Source baseline: `ShiningForceCentral/SF2DISASM` commit
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Reproduction

```powershell
pwsh ./scripts/Test-EnemyPromotionExtraction.ps1
uv run sf2 h2 enemy-gold
uv run sf2 h2 enemy-drops
uv run sf2 h3 enemy-drops
```

The verifier exports the pinned assembly contract and independently decodes the locked ROM, validates
both schemas and golden hashes, repeats both exports, then compares 2,722 fields with zero mismatch.
Generated names and full records remain under ignored `local/derived/`.
The Python-owned gold rail separately parses the source's explicit used/unused boundary, decodes the
locked ROM words, byte-compares all 172 entries, validates the generated schema and pinned hash, and
writes only to `local/derived/enemy-gold-data.json`.
The companion drop rail applies the same source/ROM/hash discipline to the 30-entry enemy-item table
and its terminating word.

## ROM Tables

| Table | Range `[start,end)` | Encoding | Records |
| --- | --- | --- | --- |
| promotions | `0x21046..0x21072` | five length-prefixed byte lists | 5 sections / 39 values |
| enemy names | `0xFB8A..0xFF87` | length byte followed by ASCII payload | 103 |
| enemy definitions | `0x1B1A66..0x1B30EE` | fixed 56-byte records, big-endian words | 103 |
| enemy item drops | `0xBE52..0xBECC` | 30 fixed four-byte entries + `0xFFFF` | 30 |
| enemy gold | `0xBECC..0xC024` | big-endian words; explicit used/tail boundary | 103 used + 69 unused |

The source paths are `data/stats/allies/classes/promotions.asm`,
`data/stats/enemies/enemynames.asm`, `data/stats/enemies/enemydefs.asm`, and
`data/stats/enemies/enemygold.asm`; enemy drops are stored in
`data/battles/global/enemyitemdrops.asm`. Enum resolution comes from
`sf2enums.asm`. The extraction manifest pins all four source hashes.

## Confirmed: Enemy Gold Table and Unused Tail

The kill-reward routine doubles the enemy index and uses it as a word offset into
`table_EnemyGold`. The source marks the first 103 words as enemy-indexed values, exactly matching the
enemy definition count. They occupy `0xBECC..0xBF9A`; the values range from 0 to 3500, with the
maximum at enemy index 97 and exactly one used zero entry.

The same assembled range continues for 69 words through `0xC024`, but the source places them after
an explicit `; unused` boundary. Twenty-five of those words are nonzero and the final word is 255,
so treating the entire 172-word region as an enemy table would manufacture 69 invalid enemy rows.
The H2 fixture preserves the tail for source/ROM parity while exposing only the first 103 values as
canonical enemy data. The DESOUL H3 fixture independently confirms enemy index 0 contributes its
table value 10 once per successful kill target.

## Confirmed: Enemy Item Drop Table and Consumer Policy

`table_EnemyItemDrops` contains 30 four-byte records at `0xBE52..0xBECA`, followed by word
terminator `0xFFFF`. Each record stores battle index, enemy combatant index (`128 + entity`), item
index, and one persistent dropped-flag index. The flags are unique and contiguous 0-29, using 30 of
the four-byte flag area's 32 available bits. The records cover 22 battles; entity indexes range
from 0 to 15. The Python H2 rail resolves every battle/item enum and byte-compares all 122 ROM bytes.

`battlesceneScript_DropEnemyItem` only searches this table when an ally defeats an enemy. A matching
record must agree on battle and entity, and the target must still carry the named item. Taros Sword,
Iron Ball, and Counter Sword alone consume `RNG(32)` and drop only on zero; the other 27 entries are
guaranteed once their preconditions match. Before removing the target item, the routine sets the
record's persistent flag and aborts if it was already set. It gives the item to a living actor when
inventory permits; otherwise only rare items enter deals. The source contains an unreachable
battle-upgrade/random-chance block after an unconditional branch, so it is not part of the rule.

The H3 replay matrix confirms the core branches on the original ROM. Taros Sword seed `1281`
produces roll 8, leaves flag 24 clear, and leaves the item on the enemy. Seed 0 produces roll 0,
sets flag 24, removes the sword, and gives it to the actor. Starting with flag 24 already set still
consumes the successful rare roll but aborts before removal or delivery. A non-random Short Rod row
sets flag 3 and transfers the item without reaching the drop-RNG checkpoint.

Recipient-failure replays confirm removal precedes routing. With either a full inventory or a dead
actor, Taros Sword is removed from the enemy and its packed deals count changes from 0 to 1. Under
the same two recipient conditions, Short Rod is removed but enters neither actor inventory nor
deals because its item definition is not rare. A Taros Sword deals count of 14 increments to 15;
an initial count of 15 remains saturated at the four-bit maximum.

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
  distinguishes the promotee from the special-item holder; reward cases should exercise rare-drop
  RNG, full inventory/deals routing, and an already-set drop flag.
- Battle sprites, map sprites, battle placement, and enemy-upgrade ranges are not
  part of this contract yet.
