# Enemy Definition and Presentation-Index Contract

- Status: **Confirmed static identity, spawn-baseline, presentation-index, and map-sprite domain**
- Evidence date: 2026-08-08
- Scope: the 103 original enemy identities and fixed definitions, their ordered battle/map-sprite
  references, the 63-row NPC map-sprite tail, and the bounded map-sprite lookup consumer

## Judgment Boundary

This contract reconstructs the immutable enemy-definition data that runtime battle systems consume.
It does not turn source-table order into encounter design or balance intent.

- **Confirmed**: 103 ordered names, 103 fixed 56-byte definitions, 103 ordered battle-sprite rows,
  166 map-sprite rows, complete source/ROM field parity, zero-valued reserved bytes, the first-103
  definition join, the 63-row map-sprite tail, and the unchecked enemy map-sprite lookup boundary.
- **Inferred**: none. Source labels remain storage vocabulary; their complete gameplay or visual
  meaning is not inferred here.
- **Unknown**: intentional original debug or other nonstandard reachability of enemy map-sprite
  indexes `103..165`; renderer-visible effects of the embedded null in one accepted name; complete
  spawn transformation effects; encounter and upgrade selection; AI behavior; turn scheduling;
  combat outcomes; reward assignment; presentation timing; difficulty curves; and authorial or
  balance intent.

The [battle encounter definition contract](battle-encounter-definition.md) owns battle-local enemy
selection, placement, and region/terrain data. The [battle AI contract](battle-ai-decision.md) owns
action and movement decisions. [Battle control](battle-control-lifecycle.md),
[combat resolution](combat-resolution.md), and [battle-scene presentation](battle-scene-presentation.md)
own their respective runtime and visual behavior. This document defines the stable input data those
systems may reference.

## Evidence Owners

`sf2-core-stats-data-static-v1`
([`core-stats-data-static-v1.json`](../../../tests/fixtures/h2/core-stats-data-static-v1.json)) is the
primary H2 inventory owner. It proves the five-file enemy-data source boundary, representative H1
addresses, and the cardinalities of the four in-scope name, definition, battle-sprite, and map-sprite
tables. The fifth file contains the separately owned enemy-gold table. Its
source-backed explanation is the [item, spell, and enemy data inventory](../../research/core-stats-data-inventory.md).

The [enemy definitions research owner](../../research/enemy-promotions.md),
[source manifest](../../../manifests/extractions/enemy-promotion-data.json),
[source schema](../../../schemas/enemy-promotion-data.schema.json),
[ROM layout](../../../manifests/extractions/enemy-promotion-rom-layout.json), and
[ROM schema](../../../schemas/rom-enemy-promotion-data.schema.json) independently export the source
and ROM representations. The accepted rail compares 2,722 fields with zero mismatches and preserves
the fixed definition layout described below. Promotion and reward material in that broader owner is
outside this contract.

`sf2-enemy-map-sprites-static-v1`
([`enemy-map-sprites-static-v1.json`](../../../tests/fixtures/h2/enemy-map-sprites-static-v1.json))
byte-compares all 166 map-sprite rows, proves the first-103/tail split, audits all built battle and
random-upgrade input ranges, and closes the `GetCombatantMapsprite` lookup boundary. This dedicated
owner is sufficient for the consumer seam; the aggregate common-scripting inventory is deliberately
not an evidence dependency here.

## Enemy Identity Domain

The definition-index domain is exactly `0..102`. Four ordered tables share that definition identity:

| Table | Confirmed rows | Contract role |
| --- | ---: | --- |
| enemy names | 103 | identity/display resource reference with raw encoded provenance |
| enemy definitions | 103 | fixed spawn-baseline record |
| enemy battle sprites | 103 | battle-sprite and palette reference pair |
| enemy map sprites | first 103 of 166 | exploration/tactical map-sprite reference |

The positional join across these first 103 rows is an original storage fact. It does not prove that
all definitions appear in built battles, that presentation resources are unique, or that a remake
must use array position as its internal foreign-key representation.

One accepted name record, enemy ID 99, stores a four-byte payload ending in a null byte. Source and
ROM exports agree on the byte. A lossless importer preserves the raw payload and the anomaly marker;
the displayed result and any modernization are separate decisions.

## Fixed Spawn-Baseline Record

Each enemy definition occupies 56 bytes and is copied as 14 longwords into a spawning combatant
entry. The accepted decoded fields are:

| Offset | Stored field | Width or packing |
| ---: | --- | --- |
| `0` | source-labeled unknown value | byte |
| `10` | source-labeled spell-power mode | byte |
| `11` | level | byte |
| `12` | maximum HP | big-endian word |
| `16` | maximum MP | byte |
| `18`, `20`, `22`, `24` | base ATT, DEF, AGI, MOV | bytes separated by reserved storage |
| `26` | resistance | big-endian word |
| `30` | prowess | byte |
| `32` | four item slots | four big-endian words |
| `40` | four spell slots | four packed bytes |
| `44` | initial status | big-endian word |
| `49` | movement type | upper nibble of the stored byte |
| `52` | AI bitfield | big-endian word |

Every one of the 27 reserved/padding bytes is zero in all 103 accepted records. Items retain a
seven-bit item ID plus the equipped bit. Spells retain a six-bit spell ID plus a two-bit spell level.
All 103 initial-status fields store `NONE`. These are storage invariants, not claims that padding is
safe extension space or that an enemy cannot acquire status after initialization.

Twelve records store an AGI byte at or above 128. The source treats the high bit as relevant to a
second-turn path, but this contract preserves the raw byte without deriving scheduling semantics.
Turn ordering and its arithmetic boundaries remain owned by battle control.

## Spawn Transformation Boundary

The accepted source-static `InitializeEnemyStats` order is:

1. apply the random-battle upgrade selector;
2. copy the selected 56-byte definition into the combatant entry;
3. initialize current HP and MP from their maxima;
4. merge movement type with battle-local AI command-set state;
5. apply battle placement and order data;
6. adjust base attack for difficulty.

The definition is therefore a spawn baseline, not a guarantee of the final combatant state. A remake
must keep import of the immutable record separate from upgrade selection, battle-local composition,
difficulty adjustment, derived-stat refresh, and status/equipment consumers. This document does not
specify those transformations beyond preserving the accepted source-static handoff order.

## Map-Sprite Table and Tail

The 166-byte map-sprite table contains two distinct index domains:

| Index range | Rows | Confirmed meaning |
| --- | ---: | --- |
| `0..102` | 103 | one row per enemy definition |
| `103..165` | 63 | NPC map-sprite tail, not additional enemy definitions |

The tail contains 62 unique map-sprite values spanning 167 through 229. Value 189 is absent and
value 199 appears twice. No tail value overlaps a definition-row value.

The built source domains remain bounded below the tail:

- 627 enemy references across the 45 accepted battle spritesets use indexes `0..102`, touch 102
  unique definitions, and omit index 100;
- all five random-upgrade ranges end at or below index 84;
- `InitializeEnemyStats` is the only named caller of `SetEnemyIndex` in the accepted source audit.

`GetCombatantMapsprite` detects an enemy combatant, reads its stored enemy-index byte through
`GetEnemy`, and performs an unsigned byte lookup in `table_EnemyMapsprites`. The lookup has no local
bounds check. Normal built battle initialization therefore cannot select the tail, while a raw,
debug, malformed, or corrupt index could. Whether any original nonstandard route intentionally does
so remains **Unknown**.

## Presentation Reference Boundary

The 103 battle-sprite rows each store a battle-sprite identity and palette selector. The first 103
map-sprite rows store map-sprite identities. Those ordered references are **Confirmed**; decoded
graphics containers, animation sequences, palette composition, loader behavior, and visible timing
remain owned by presentation and graphics contracts.

Neither table establishes one-to-one visual uniqueness. Multiple enemy definitions may share a
sprite family or palette. A modern asset system may replace positional tables with stable resource
IDs, but it must preserve original references for parity diagnostics and must report replacement or
remapping as a deliberate content decision.

## Implementation-Neutral Import Model

A complete logical import keeps the 103-definition domain distinct from the 166-row map-sprite
storage domain:

```text
EnemyDefinition
  enemyId
  nameResourceRef
  rawNameProvenance
  spawnBaselineRef
  battleSpriteRef
  definitionMapSpriteRef

EnemySpawnBaseline
  raw56ByteProvenance
  unknownByte
  spellPowerMode
  level
  maxHp, maxMp
  baseAtt, baseDef, baseAgi, baseMov
  resistanceBits, prowessBits
  items[4], spells[4]
  initialStatusBits
  movementType
  aiBits

EnemyMapSpriteTable
  definitionRows[103]
  npcTailRows[63]
```

This is a logical contract, not a required engine class layout. The importer must not expose tail
rows as extra enemy definitions, discard the raw name anomaly, reinterpret reserved zero bytes as
new fields, or flatten runtime transforms into the immutable baseline.

## Original Fidelity and Modernization

Original-fidelity mode preserves all 103 identities, raw name encodings, 56-byte record fields and
padding, item/spell packing, presentation references, the 166-row map-sprite table, and the exact
definition/tail split. It also preserves the distinction between a baseline record and a runtime
combatant.

Rebalanced stats, revised enemy rosters, new difficulty scaling, changed AI data, new rewards,
replacement names, and replacement art are deliberate design or content layers. Future enemy/player
numeric-curve work should compare imported baselines and observed runtime transforms through battle
simulation; it must not describe stored numbers as evidence of intended difficulty.

Generated names, full definition rows, and presentation assets remain private original content.
Tracked fixtures retain only structural metadata, ranges, counts, hashes, and bounded rules. A
distributable remake requires replacement or separately cleared content.

## H4 Acceptance Gates

A future remake enemy-definition importer passes this contract only when:

1. all 103 identity, definition, battle-sprite, and definition-map-sprite rows preserve their ordered
   joins and original numeric IDs;
2. every definition preserves all decoded fields, four items, four spells, packing, 27 zero reserved
   bytes, and the complete 56-byte provenance boundary;
3. the ID-99 name payload retains its trailing-null anomaly without claiming a visible result;
4. imported definitions remain immutable spawn baselines, separate from upgrade, placement/order,
   difficulty, derived-stat, equipment, and status transforms;
5. all 166 map-sprite rows preserve the `103 + 63` split, tail value range, missing/duplicate facts,
   and unchecked lookup boundary;
6. normal original built inputs remain limited to definition indexes `0..102`, while nonstandard tail
   reachability remains explicit and **Unknown**;
7. original-compatible private data imports deterministically while public artifacts expose only
   cleared content or non-expressive metadata;
8. encounter selection, AI, turn order, combat, rewards, presentation, balance, and modernization
   are tested by separate owners or reported as deliberate deviations.

H4 does not require the original table layout at runtime. It requires a provenance-preserving import
whose transformations can be replayed and whose definition IDs remain joinable to separate battle,
AI, presentation, and simulation data.

## Evidence Matrix

| Contract area | Evidence label | Executable owner | Remaining boundary |
| --- | --- | --- | --- |
| four in-scope tables within the broader five-file enemy source boundary, with 103/103/103/166 counts | **Confirmed static** | `sf2-core-stats-data-static-v1` ([`core-stats-data-static-v1.json`](../../../tests/fixtures/h2/core-stats-data-static-v1.json)) | Enemy gold, complete consumer behavior, and design intent |
| 103 names and fixed 56-byte definitions with source/ROM field parity | **Confirmed static** | [enemy definitions owner](../../research/enemy-promotions.md), [manifest](../../../manifests/extractions/enemy-promotion-data.json), and [ROM layout](../../../manifests/extractions/enemy-promotion-rom-layout.json) | Name rendering and complete spawn transformations |
| 166 map-sprite rows, `103 + 63` split, built input domains, and unchecked consumer | **Confirmed static** | `sf2-enemy-map-sprites-static-v1` ([`enemy-map-sprites-static-v1.json`](../../../tests/fixtures/h2/enemy-map-sprites-static-v1.json)) | Nonstandard tail reachability and visible loader result |
| battle selection, placement, regions, and local command data | **Separate owner** | [battle encounter definition](battle-encounter-definition.md) | Runtime admission and story selection |
| action/movement choice, turn scheduling, combat, rewards, and presentation | **Separate owners** | [battle AI](battle-ai-decision.md), [battle control](battle-control-lifecycle.md), [combat resolution](combat-resolution.md), and [battle-scene presentation](battle-scene-presentation.md) | End-to-end multi-turn behavior and player-visible output |
| enemy numeric curves, roster difficulty, balance, replacement content | **Unknown / deliberate design** | Future synthesis, simulation, and content owners | Do not infer intent from stored definitions |

## Reproduction

```powershell
uv run sf2 h2 core-stats-data
uv run sf2 h2 enemy-map-sprites
pwsh ./scripts/Test-EnemyPromotionExtraction.ps1
uv run sf2 design-contracts test
uv run sf2 verify
```
