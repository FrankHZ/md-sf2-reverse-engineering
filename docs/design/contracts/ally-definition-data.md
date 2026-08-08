# Ally Definition and Growth-Data Contract

- Status: **Confirmed static definition topology, storage shape, counts, and table invariants**
- Evidence date: 2026-08-08
- Scope: original ally identities, start records, class metadata, presentation references, promotion
  mappings, growth projections, spell-learning lists, and pointer topology

## Judgment Boundary

This contract separates immutable ally-definition data from the systems that consume or mutate it.

- **Confirmed**: the complete 42-file ally-data source boundary and transitive include graph; table
  addresses and dimensions; 30 named identities; 32 start records and stat-pointer slots; 32 class
  records; promotion-table shape; growth-curve invariants; per-ally class-record and spell-list
  structure; and the 30-row ally map-sprite table.
- **Inferred**: none. Source labels such as movement, resistance, prowess, class type, and promotion
  are retained as storage vocabulary, but this document does not infer complete behavior from those
  labels.
- **Unknown**: natural reachability of start-record slots 30 and 31; the visible fallback when a
  battle-sprite class entry is `NONE`; story join order; runtime class/promotion admission; roster
  membership and active-party selection; complete presentation selection; balance intent; and the
  player-facing value of any ally, class, growth curve, or spell list.

The [party and roster state contract](party-roster-state.md) owns runtime membership and active-party
mutation. The [level-up contract](level-up.md) owns stat gain, class-block scanning, spell learning,
caps, clamps, and combatant refresh. The [service-interactions contract](service-interactions.md) owns
the bounded Church promotion path. None of those dynamic behaviors are redefined here.

## Evidence Owners

The primary executable owner is `sf2-ally-data-static-v1`
([`ally-data-static-v1.json`](../../../tests/fixtures/h2/ally-data-static-v1.json)). It inventories all
42 files under `data/stats/allies`, proves that 12 direct layout includes plus 30 nested stat includes
cover the complete source boundary, resolves one representative H1 address per file, and rechecks the
static facts summarized below.

Its detailed source-backed owner is the [ally and class data inventory](../../research/ally-data-inventory.md).
The inventory cross-checks two earlier canonical rails without replacing them:

- [static core data](../../research/static-core-data.md), its
  [manifest](../../../manifests/extractions/static-data.json), and
  [schema](../../../schemas/static-data.schema.json) own names, start records, class records, packing,
  and source/ROM parity;
- [ally growth and spell learning](../../research/ally-growth.md), its
  [manifest](../../../manifests/extractions/growth-data.json), and
  [schema](../../../schemas/growth-data.schema.json) own the locally generated row-level growth and
  learned-spell representation.

`sf2-map-sprite-assignments-static-v1`
([`map-sprite-assignments-static-v1.json`](../../../tests/fixtures/h2/map-sprite-assignments-static-v1.json))
is a secondary consumer owner. It confirms the 30-row ally map-sprite table and classifies the
accepted writer/caller boundary. It does not prove story placement, presentation timing, or the
validity of arbitrary injected map-sprite IDs.

## Stable Identity and Slot Topology

The original tables expose three related but non-identical domains:

| Domain | Confirmed shape | Contract consequence |
| --- | --- | --- |
| named ally identity | 30 ordered name rows, 30 map-sprite rows, and 30 stat-source files | Stable ally IDs are `0..29`; presentation and stat references may use the same ordered identity without implying story availability. |
| start-definition storage | 32 fixed six-byte records | Import all 32 records losslessly; do not invent names for slots 30 and 31. |
| stat pointer storage | 32 pointers, 30 unique targets | Slots `0..29` point to their corresponding `AllyStatsNN`; slots 30 and 31 both reuse `AllyStats29`. |

The two trailing start records store class `RDBN`, level 1, and four empty item values. Their bytes
and positions are **Confirmed**. Their purpose and runtime reachability are **Unknown**. They are also
distinct from the two trailing stat pointers: shared index positions do not prove shared semantics.

An ally start record stores one class byte, one level byte, and four item bytes. Item bit 7 records
the equipped flag and bits `0..6` record the item ID. This is an import/storage rule, not proof that a
particular story event uses the record unchanged or that the initial equipment is balanced.

## Class and Promotion Definition Topology

The source contains 32 ordered class names, 32 class-type rows, and 32 fixed five-byte class
definitions. Each definition stores:

1. a movement byte;
2. a big-endian resistance word;
3. a movement-type value in the high nibble of its storage byte;
4. a prowess byte.

The field boundaries and values are **Confirmed storage facts**. Their complete runtime effects are
owned by the relevant movement, combat, status, and progression contracts; a remake importer must not
turn enum comments into additional behavior.

The associated static tables also contain 16 two-byte critical definitions and 15
blacksmith-eligible class IDs. These are shared reference data. Critical resolution and blacksmith
admission/economy remain separate consumer contracts.

Promotion data is an ordered relation rather than an implicit arithmetic rule:

| Section | Stored rows |
| --- | ---: |
| regular base classes | 12 |
| regular promoted classes | 12 |
| special base classes | 5 |
| special promoted classes | 5 |
| special-promotion items | 5 |

The paired section lengths and five item references are **Confirmed**. Whether a caller admits a
promotion, consumes an item, changes equipment, or presents a choice is outside this static contract.

## Growth and Spell-List Storage

Five stored growth curves each contain 29 rows for levels 2 through 30. Every row stores a cumulative
fraction and a this-level fraction on a 256-point scale. For every accepted row, the cumulative value
equals the previous cumulative value plus the current gain, and every curve ends at 256. These are
data invariants, not a claim about random variance or the amount applied by a runtime level-up.

The 30 ally stat files contain 59 class records. Each class record begins with a class ID followed by
five three-byte stat projections in HP, MP, attack, defense, and agility order. A projection stores a
curve ID, starting value, and projected level-30 value; curve ID 0 is the stored `NONE` value.

After the projections, the record stores one of two spell-list forms:

- an explicit sequence of learn-level and packed spell bytes terminated by `0xFF`; or
- the control byte `0xFE`, which reuses the first class record's explicit spell list.

All 30 first-class records own an explicit list. Across the corpus there are 52 explicit lists, seven
inherited lists, and 122 learned-spell entries. The spell byte keeps a six-bit spell ID and a two-bit
spell level. The accepted pointer, sentinel, and packing rules must survive import even if a remake
uses a more explicit internal representation.

## Presentation References

The original ally-definition area contains 30 map-sprite rows and 90 battle-sprite/class/palette
entries, exactly three battle-sprite entries per named ally. These tables establish ordered
references, not a universal visible-selection rule.

The map-sprite assignment owner confirms that the ally table has 30 rows and that accepted callers
can derive a map-sprite value from it. Scripted and literal sprite writes remain separate paths. The
following remain **Unknown** here:

- which story state selects each presentation row;
- battle-sprite behavior when a class entry is `NONE`;
- palette, animation, DMA, and frame timing;
- whether malformed state or injected IDs reach a loader;
- accessibility, replacement assets, and modern presentation policy.

## Implementation-Neutral Import Model

A remake may normalize the original byte layout, but the import boundary must retain independently
addressable records and provenance. At minimum, an imported definition model needs:

```text
AllyDefinition
  allyId
  nameResourceRef
  startSlotRef
  statPointerSlotRef
  mapSpriteRef
  battleSpriteEntries[]

AllyStartSlot
  slotId
  allyId?
  classId
  level
  itemBytes[4]

AllyStatPointerSlot
  slotId
  targetRef

ClassDefinition
  classId
  classType
  movement
  resistanceBits
  movementType
  prowessBits

PromotionTables
  regularBaseClassIds[12]
  regularPromotedClassIds[12]
  specialBaseClassIds[5]
  specialPromotedClassIds[5]
  specialPromotionItemIds[5]

AllyClassGrowth
  allyId
  classId
  statProjections[HP, MP, ATT, DEF, AGI]
  spellList = Explicit(entries[]) | InheritFirst
```

This notation is a logical contract, not a required engine class layout. The 30 named
`AllyDefinition` records may reference slots `0..29`, but both 32-slot collections remain first-class:
the unnamed start slots retain no invented ally ID, and pointer slots 30 and 31 retain their separate
slot identities even though they share one target. `PromotionTables` preserves the five stored arrays
without asserting caller admission or mutation behavior. Original numeric IDs and raw values must
remain available for parity diagnostics; localized names, display labels, and modernized balance
metadata belong in separate layers.

## Original Fidelity and Modernization

Original-fidelity mode preserves the accepted identities, table order, raw values, pointer aliases,
promotion relations, curve rows, projections, list inheritance, and presentation references. It
must also preserve the distinction between known data and unknown reachability.

Modern roster choice, rebalanced growth, revised promotion paths, new classes, renamed identities,
or alternate presentation may be deliberate design work. Such changes must be represented as an
explicit overlay or replacement dataset and tested separately; they must not rewrite the imported
original contract or be presented as reverse-engineered intent.

The locally generated row-level ally, name, stat, spell, and presentation content is private source
material. Public fixtures retain structural metadata, addresses, counts, hashes, and invariants only.
A distributable remake requires replacement or separately cleared names, numeric content, and assets.

## H4 Acceptance Gates

A future remake ally-definition importer passes this contract only when:

1. all 30 named identities, 32 start records, 32 stat pointers, and 30 unique stat targets retain
   their accepted order and raw identity;
2. start records preserve class, level, four item bytes, and equipped-bit packing, while slots 30 and
   31 remain unnamed and do not gain invented reachability;
3. all 32 class names/types/definitions, 16 critical definitions, 15 blacksmith class references,
   four promotion sections, and five special-promotion item references retain their source values;
4. all five 29-row curves satisfy cumulative-gain and terminal-256 invariants;
5. all 59 class records retain the five ordered stat projections, curve IDs, start/projection values,
   52 explicit spell lists, seven `0xFE` inherited lists, 122 entries, and `0xFF` termination;
6. the 30 map-sprite rows and 90 ordered battle-sprite/class/palette entries remain lossless, without
   inventing a `NONE` fallback;
7. original-compatible private data can be imported deterministically while public artifacts expose
   only cleared content or non-expressive metadata;
8. runtime roster state, level-up mutation, promotion transactions, story availability, balance,
   localization, and presentation behavior are tested by their separate owners or reported as
   deliberate deviations.

H4 does not require the remake to reproduce the original source-file or byte-table layout. It
requires a deterministic, provenance-preserving transformation that can explain every imported
identity, relation, alias, and raw value.

## Evidence Matrix

| Contract area | Evidence label | Executable owner | Remaining boundary |
| --- | --- | --- | --- |
| 42-file boundary, H1 addresses, table dimensions, class/promotion/growth/stat/pointer summaries | **Confirmed static** | `sf2-ally-data-static-v1` ([`ally-data-static-v1.json`](../../../tests/fixtures/h2/ally-data-static-v1.json)) | Complete runtime consumption and design intent |
| names, 32 start records, 32 class records, fixed packing, and source/ROM parity | **Confirmed static** | [static-core owner](../../research/static-core-data.md), [manifest](../../../manifests/extractions/static-data.json), and [schema](../../../schemas/static-data.schema.json) | Slots 30/31 reachability and complete field semantics |
| five curves, 59 class records, and 122 learned-spell entries | **Confirmed static** | [growth owner](../../research/ally-growth.md), [manifest](../../../manifests/extractions/growth-data.json), and [schema](../../../schemas/growth-data.schema.json) | Runtime gain, scan, learning, cap, clamp, and refresh behavior is owned by [level-up](level-up.md) |
| 30 ally map-sprite rows and accepted derived caller boundary | **Confirmed static** | `sf2-map-sprite-assignments-static-v1` ([`map-sprite-assignments-static-v1.json`](../../../tests/fixtures/h2/map-sprite-assignments-static-v1.json)) | Story selection, injected IDs, loader failure modes, visible timing |
| membership, active-party selection, join/remove/rejoin mutation | **Separate owner** | [party and roster state](party-roster-state.md) | Campaign chronology and player choice space remain unclosed |
| promotion transaction and player-facing service flow | **Separate owner** | [service interactions](service-interactions.md) | Complete promotion mutation, inventory edges, persistence, and UX |
| roster composition, numeric curves, balance, and modern progression policy | **Unknown / deliberate design** | Future synthesis and simulation owners | Do not infer intent from the original tables |

## Reproduction

```powershell
uv run sf2 h2 ally-data
uv run sf2 h2 map-sprite-assignments
uv run sf2 design-contracts test
uv run sf2 verify
```
