# Item Definition and Auxiliary-Catalog Contract

- Status: **Confirmed static item identity, record packing, auxiliary catalogs, and bounded consumers**
- Evidence date: 2026-08-08
- Scope: 128 original item identities and fixed definitions, shop/debug/chest/break/mithril/Caravan/
  field-use tables, weapon-graphics references, and their accepted static lookup boundaries

## Judgment Boundary

This contract defines immutable item data and auxiliary lookup catalogs. It does not define complete
transactions, inventory mutation, persistence, presentation, random outcomes, or economy balance.

- **Confirmed**: 128 ordered item names and 16-byte definitions with source/ROM parity; nine
  auxiliary ROM ranges totaling 768 bytes; their complete table counts; and the accepted static
  shop, chest, break-message, mithril, Caravan, field-use, and weapon-graphics consumer rules.
- **Inferred**: none. Source labels and enum names remain storage vocabulary rather than evidence of
  player-facing meaning or design intent.
- **Unknown**: story/debug admission for all 30 shop indexes; special-Caravan presentation; observed
  blacksmith frequencies and order persistence; malformed-index effects; complete item-use/equip/
  break flows; transaction atomicity; save persistence; price progression; and balance intent.

The [service-interactions contract](service-interactions.md) owns the bounded shop, Caravan/depot,
church, and blacksmith action order. [Progression and economy synthesis](../synthesis/progression-and-economy.md)
connects accepted resource flows without claiming balance intent. [Combat resolution](combat-resolution.md),
[spell resolution](spell-resolution.md), and [battle-scene presentation](battle-scene-presentation.md)
own runtime effects and visible battle behavior. This contract supplies stable item/catalog inputs.

## Evidence Owners

`sf2-core-stats-data-static-v1`
([`core-stats-data-static-v1.json`](../../../tests/fixtures/h2/core-stats-data-static-v1.json)) proves
the complete ten-file item source boundary, H1 addresses, 128 names, 128 definitions, and the
auxiliary table cardinalities. Its source-backed owner is the
[item, spell, and enemy data inventory](../../research/core-stats-data-inventory.md).

The earlier [static core owner](../../research/static-core-data.md),
[manifest](../../../manifests/extractions/static-data.json),
[schema](../../../schemas/static-data.schema.json), and
[ROM layout](../../../manifests/extractions/rom-static-layout.json) own canonical item records and
independent source/ROM parity. The accepted rail compares all 281 fixed-width records in its broader
scope with zero mismatches; this contract consumes only its 128 item rows.

`sf2-item-auxiliary-static-v1`
([`item-auxiliary-static-v1.json`](../../../tests/fixtures/h2/item-auxiliary-static-v1.json)) is the
dedicated auxiliary owner. It byte-compares nine address ranges, inventories eight source files and
seven consumer files, and records the complete lookup boundaries summarized below. Generated
row-level content remains private under `local/derived/`.

## Item Identity and Fixed Record

The original item identity domain is exactly `0..127`. The ordered name and definition tables each
contain 128 rows. Every definition occupies 16 bytes:

| Stored field | Width or packing |
| --- | --- |
| equip flags | big-endian 32-bit value |
| maximum and minimum range | one byte each |
| price | big-endian word |
| item type | byte |
| use spell | six-bit spell ID plus two-bit spell level |
| three effect/parameter pairs | six bytes total |

These are storage facts. Equip-flag meaning, range geometry, spell execution, effect dispatch, and
price use remain separate consumer behavior unless a dedicated owner confirms them.

Item ID 127 preserves an important representation distinction: its enum code is `NOTHING`, while the
name/definition comments use `Empty`. A lossless import keeps stable numeric ID, enum code, raw name
expression, and display resource separate instead of collapsing them into one semantic string.

## Shop and Debug Catalogs

The shop catalog contains 30 count-prefixed records: 15 weapon shops and 15 item shops. Across those
records there are 235 item references and 15 unique inventory-row contents. Index 0 selects the first
record; higher indexes skip that many count-prefixed records from the list start. This establishes
storage traversal, not story availability or menu behavior.

The debug-shop catalog stores one count byte followed by all 128 item indexes. Whether a retail
player route can enter it, and what transaction policy applies there, remain outside this contract.

An implementation-neutral import should turn each count-prefixed row into an explicit ordered item
list while retaining the original shop index and row provenance. It must not deduplicate the 30 shop
identities merely because only 15 row contents are unique.

## Chest, Break-Message, and Field-Use Tables

The auxiliary owner confirms:

- 13 chest-gold tiers. The consumer selects `word[(itemIndex-128)&127]` without a local bounds check;
- 25 item-break message rules. A matching item byte adds its stored offset to an already selected
  base message;
- nine field-usable item IDs in a linear byte allowlist terminated by `255`.

These rules are bounded lookup contracts. They do not prove caller validation, message rendering,
gold transfer atomicity, consumption policy, target eligibility, or persistence. A remake may use
validated typed lookups, but original-fidelity tests must retain the accepted index arithmetic,
termination, and offset relations.

## Mithril Catalog

Mithril data contains nine class groups and eight weapon rows. Each weapon row stores four choices,
for 32 stored choices total. The accepted consumer boundary is:

1. class groups `0..7` select their corresponding weapon row;
2. BRN and RDBN occupy group 8, outside the direct `0..7` scan;
3. that fallback chooses row 0 or row 2;
4. the selected row tests denominators `16, 8, 4, 1` in that order.

This is a static selection algorithm, not an observed distribution or complete blacksmith order
lifecycle. RNG state, presentation, price/payment, item removal, order persistence, and fulfillment
remain owned elsewhere or **Unknown**.

## Special Caravan and Weapon Graphics

The special-Caravan table contains one accepted description entry. A match displays its stored count
of consecutive messages. Message content, window timing, normal reachability, and interaction with
ordinary depot descriptions are outside this data contract.

The weapon-graphics table contains 84 rows corresponding to item indexes `26..109`. Each row stores
two signed bytes for sprite and palette selection; 18 rows use the no-sprite value. The accepted
consumer admits only allies with an equipped item in that inclusive range, uses `itemIndex-26`, and
returns `-1/-1` for every rejected case. A stored byte `255` therefore decodes as signed `-1`.

The table establishes references, not decoded art, animation, palette composition, hand alignment,
or visible timing. Those remain presentation/graphics concerns.

## Implementation-Neutral Import Model

A complete logical import keeps item identities distinct from catalogs and consumers:

```text
ItemDefinition
  itemId
  enumCode
  displayResourceRef
  equipFlags
  minRange, maxRange
  price
  itemType
  useSpellRef
  effectParameterPairs[3]

ShopCatalog
  shopIndex
  category
  orderedItemIds[]
  sourceRowProvenance

MithrilCatalog
  classGroups[9]
  weaponRows[8][4]

ItemAuxiliaryCatalog
  chestGoldTiers[13]
  breakMessageRules[25]
  fieldUseAllowlist[9]
  specialCaravanDescriptions[1]
  weaponGraphicsRows[84]
```

This notation is a logical contract, not a required engine class layout. Original IDs, ordering,
packing, duplicate shop rows, sentinels, signed values, and source provenance must remain available
for parity diagnostics. Runtime services consume these records through separate interfaces.

## Original Fidelity and Modernization

Original-fidelity mode preserves all 128 identities and definition fields, all auxiliary row order,
shop identity despite duplicate content, lookup arithmetic, sentinels, mithril selection order, and
weapon-graphics signed values. It also preserves the distinction between immutable data and runtime
mutation.

Modern inventory UX, categorized shops, explicit bounds checks, revised prices, rebalanced item
effects, deterministic crafting, new equipment slots, and replacement content are deliberate design
layers. They must be tested and reported separately rather than presented as evidence about the
original tables.

Generated item names, full definitions, inventories, message references, and graphics remain private
original content. Public fixtures retain structural metadata, counts, hashes, and bounded rules only.
A distributable remake requires replacement or separately cleared content.

## H4 Acceptance Gates

A future remake item-data importer passes this contract only when:

1. all 128 item identities, names, definitions, numeric IDs, and 16-byte field values remain lossless;
2. item ID 127 retains separate numeric, enum-code, raw-name, and display-resource representations;
3. all 30 shop identities, 235 references, count-prefixed order, 15 unique contents, and the complete
   128-item debug catalog import deterministically;
4. all 13 chest tiers, 25 break-message rules, and nine terminated field-use IDs retain their
   accepted lookup arithmetic and sentinel/offset boundaries;
5. all nine mithril class groups, eight four-choice rows, fallback rows, and denominator order remain
   reproducible without claiming observed RNG distribution;
6. the one special-Caravan entry and all 84 weapon-graphics rows retain identity, signed-byte
   decoding, item-index range, rejected result, and 18 no-sprite rows;
7. original-compatible private data can be imported deterministically while public artifacts expose
   only cleared content or non-expressive metadata;
8. transaction order, inventory mutation, effects, RNG, persistence, presentation, economy balance,
   and modernization are tested by separate owners or reported as deliberate deviations.

H4 does not require the original byte tables at runtime. It requires a provenance-preserving import
that can explain every identity, catalog row, relation, and accepted lookup result.

## Evidence Matrix

| Contract area | Evidence label | Executable owner | Remaining boundary |
| --- | --- | --- | --- |
| ten-file item source boundary and complete table cardinalities | **Confirmed static** | `sf2-core-stats-data-static-v1` ([`core-stats-data-static-v1.json`](../../../tests/fixtures/h2/core-stats-data-static-v1.json)) | Complete runtime consumers and design intent |
| 128 names, fixed 16-byte definitions, and source/ROM parity | **Confirmed static** | [static core owner](../../research/static-core-data.md), [manifest](../../../manifests/extractions/static-data.json), and [ROM layout](../../../manifests/extractions/rom-static-layout.json) | Complete field semantics and item-use/equip behavior |
| nine auxiliary ranges, 768 parity bytes, table counts, and bounded consumers | **Confirmed static** | `sf2-item-auxiliary-static-v1` ([`item-auxiliary-static-v1.json`](../../../tests/fixtures/h2/item-auxiliary-static-v1.json)) | Caller admission, presentation, RNG observations, persistence |
| shop/Caravan/blacksmith transaction order | **Separate owner** | [service interactions](service-interactions.md) | Complete runtime edges, persistence, and UX |
| battle effects, spell use, rewards, and graphics presentation | **Separate owners** | [combat resolution](combat-resolution.md), [spell resolution](spell-resolution.md), and [battle-scene presentation](battle-scene-presentation.md) | Complete item effect dispatch and visible output |
| pricing curves, item balance, crafting policy, replacement content | **Unknown / deliberate design** | Future synthesis, simulation, and content owners | Do not infer intent from stored catalogs |

## Reproduction

```powershell
uv run sf2 h2 core-stats-data
uv run sf2 h2 item-auxiliary
pwsh ./scripts/Test-StaticExtraction.ps1
pwsh ./scripts/Test-RomStaticParity.ps1
uv run sf2 design-contracts test
uv run sf2 verify
```
