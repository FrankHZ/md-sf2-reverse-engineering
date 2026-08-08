# Spell Definition Data Contract

- Status: **Confirmed static spell identity, element rows, definition packing, and definition source/ROM parity**
- Evidence date: 2026-08-08
- Scope: 44 original base spell identities and element rows, 89 fixed spell definitions, packed
  identity/level and animation fields, and the radius-3 storage exception

## Judgment Boundary

This contract defines immutable spell records. It does not infer runtime effects from field or enum
names and does not own spell-range tables or target geometry.

- **Confirmed**: 44 ordered spell names; 44 ordered element values; 89 fixed eight-byte definition
  records; definition source/ROM parity; field packing; and the stored radius-3 exception.
- **Inferred**: none. Labels such as element, properties, animation, radius, and power remain storage
  vocabulary until a dedicated runtime owner establishes their behavior.
- **Unknown**: complete element and property semantics; target admission and range geometry; status
  and effect dispatch; MP checking/payment; caller reachability; animation and presentation; the
  runtime meaning of radius 3; balance intent; localization; and distributable content.

The [spell-resolution contract](spell-resolution.md) owns accepted damage, healing, status, targeting,
and EXP cases. [Battle-scene presentation](battle-scene-presentation.md) owns the separate animation/
graphics pipeline. The [level-up contract](level-up.md) owns learned-spell mutation, while
[ally definition data](ally-definition-data.md) owns stored learn lists. None of those behaviors are
redefined here.

## Evidence Owners

`sf2-core-stats-data-static-v1`
([`core-stats-data-static-v1.json`](../../../tests/fixtures/h2/core-stats-data-static-v1.json)) is the
dedicated H2 inventory owner for the three associated tables. It confirms their H1 addresses and the
`44 names / 44 elements / 89 definitions` cardinality. Its source-backed explanation is the
[item, spell, and enemy data inventory](../../research/core-stats-data-inventory.md).

The [static core owner](../../research/static-core-data.md),
[manifest](../../../manifests/extractions/static-data.json),
[schema](../../../schemas/static-data.schema.json),
[ROM layout](../../../manifests/extractions/rom-static-layout.json), and
[ROM schema](../../../schemas/rom-static-data.schema.json) own canonical name extraction and
independent definition source/ROM parity. The broader rail compares 281 fixed-width records with zero field
mismatches; this contract consumes its spell names and 89 spell-definition rows.

The separate `spellranges.asm` pointer/ring table and battlefield range behavior are intentionally
not evidence dependencies. Definition-local min/max/radius bytes are preserved as raw fields only.

## Identity and Definition Domains

The source exposes two related domains:

| Domain | Confirmed shape | Contract consequence |
| --- | ---: | --- |
| base spell identity | 44 ordered name rows and 44 ordered element rows | IDs `0..43` join names and stored element values positionally. |
| spell definition | 89 ordered eight-byte rows | Multiple rows may reference one base identity at different packed levels or represent special entries. |

The 89-row definition domain must not be collapsed into 44 rows. A remake may key ordinary variants
by base ID and level, but it must preserve definition order, raw packed identity, and every accepted
row so special or nonuniform entries remain explainable.

Names are display resources, not behavioral specifications. Element enum labels are stable imported
values, not proof of resistance arithmetic, effect family, targeting, or animation.

## Fixed Eight-Byte Record

Each spell definition stores exactly eight bytes:

| Byte | Stored field | Packing boundary |
| ---: | --- | --- |
| `0` | spell identity and level | low six bits: base spell ID; high two bits: level code |
| `1` | MP cost | raw byte |
| `2` | animation | five index bits, two variation bits, one mirrored bit |
| `3` | properties | raw bitfield |
| `4` | maximum range | raw byte |
| `5` | minimum range | raw byte |
| `6` | radius | raw byte |
| `7` | power | raw byte |

The storage order is maximum range before minimum range. An importer must not silently reorder the
raw record even if its normalized model exposes `minRange` before `maxRange`.

The packed level values encode levels 1 through 4 in the high two bits. This is an identity/storage
rule. It does not prove level availability, acquisition order, MP affordability, or effect scaling.

## Radius-3 Exception

The source syntax comment describes radius as `0..2`, but definition 58 stores radius 3 for the LASER
entry. The independent ROM decoder agrees with the source byte, and the schema therefore accepts
`0..3`.

This is a **Confirmed data exception**, not a confirmed geometry rule. A lossless importer preserves
the value; target shape, caller reachability, and visible behavior for radius 3 remain **Unknown**.
Validation must follow accepted data rather than narrowing the schema to an inaccurate source comment.

## Element and Consumer Separation

The element table contains one byte for each of the 44 base spell IDs. The positional join is
**Confirmed**. Complete element semantics are not: runtime resistance selection, weakness/major/minor
adjustments, non-damage spell handling, and special-case dispatch belong to spell resolution.

Likewise, definition properties, animation bits, MP cost, range bytes, radius, and power are imported
facts. Their consumers may interpret the same field differently by action family. This contract
requires lossless availability, not one universal gameplay formula.

## Implementation-Neutral Import Model

```text
SpellIdentity
  spellId
  rawNameExpression
  displayResourceRef
  elementValue

SpellDefinition
  definitionIndex
  rawSpellAndLevelByte
  spellId
  levelCode
  mpCost
  rawAnimationByte
  animationIndex
  animationVariation
  mirrored
  propertyBits
  maxRange, minRange
  radius
  power
```

This notation is logical, not a required engine layout. Raw bytes and normalized fields both remain
available for parity diagnostics. References to range tables, runtime effect handlers, targets, and
presentation resources belong in separate layers.

## Original Fidelity and Modernization

Original-fidelity mode preserves all 44 identities and element rows, all 89 definitions, table order,
packed bytes, definition-local range fields, and radius 3. It does not invent behavior for source
labels that lack an accepted consumer contract.

Modern spell taxonomies, revised MP costs, targeting previews, new levels, balance changes, explicit
effect types, accessible presentation, and replacement names are deliberate design/content layers.
They must be tested separately and must not overwrite the imported original baseline.

Generated names and full numeric definitions are private original content. Public fixtures retain
counts, addresses, hashes, and structural facts only. A distributable remake requires replacement or
separately cleared names and data.

## H4 Acceptance Gates

A future remake spell-data importer passes this contract only when:

1. all 44 spell identity/name/element rows preserve order, numeric ID, raw name provenance, and raw
   element value;
2. all 89 eight-byte definition rows preserve order and every raw field;
3. packed spell ID/level and animation index/variation/mirror values round-trip exactly;
4. maximum/minimum range storage order remains traceable even if normalized APIs use another order;
5. definition 58 retains radius 3 without inventing geometry or caller reachability;
6. the 44-row identity domain and 89-row definition domain remain distinct and joinable;
7. original-compatible private data imports deterministically while public artifacts expose only
   cleared content or non-expressive metadata;
8. resolution, targeting, effects, MP transactions, presentation, acquisition, balance,
   localization, and modernization are tested by separate owners or reported as deliberate deviations.

## Evidence Matrix

| Contract area | Evidence label | Executable owner | Remaining boundary |
| --- | --- | --- | --- |
| three in-scope tables within the broader four-file spell source boundary, with 44/44/89 counts | **Confirmed static** | `sf2-core-stats-data-static-v1` ([`core-stats-data-static-v1.json`](../../../tests/fixtures/h2/core-stats-data-static-v1.json)) | Spell-range table, complete consumers, and design intent |
| spell names, 89 fixed records, packing, and source/ROM parity | **Confirmed static** | [static core owner](../../research/static-core-data.md), [manifest](../../../manifests/extractions/static-data.json), and [ROM layout](../../../manifests/extractions/rom-static-layout.json) | Runtime field semantics and copyrighted content |
| 44 positional element rows | **Confirmed static** | `sf2-core-stats-data-static-v1` ([`core-stats-data-static-v1.json`](../../../tests/fixtures/h2/core-stats-data-static-v1.json)) | Resistance/effect interpretation |
| damage, healing, status, target admission, and EXP behavior | **Separate owner** | [spell resolution](spell-resolution.md) | Complete action families and caller reachability |
| range rings/geometry and battle presentation | **Separate owners** | Future/adjacent battlefield and [battle-scene presentation](battle-scene-presentation.md) owners | Geometry, animation timing, rendered output |
| MP curves, spell balance, localization, replacement content | **Unknown / deliberate design** | Future synthesis, simulation, and content owners | Do not infer intent from stored rows |

## Reproduction

```powershell
uv run sf2 h2 core-stats-data
pwsh ./scripts/Test-StaticExtraction.ps1
pwsh ./scripts/Test-RomStaticParity.ps1
uv run sf2 design-contracts test
uv run sf2 verify
```
