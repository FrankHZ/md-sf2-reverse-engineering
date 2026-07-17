# Static Core Data Contract

- Status: **Confirmed** for source structure, ROM-byte layout, counts, ranges, deterministic extraction, and source/ROM parity
- Evidence date: 2026-07-17
- Scope: ally start slots, classes, items, spell names, and spell definitions

## Result

The first H2 slice converts nine files from the pinned SF2DISASM commit into one canonical JSON
document and validates it against [`schemas/static-data.schema.json`](../../schemas/static-data.schema.json).
The generated document stays under ignored `local/derived/`; only its schema, summary counts, source
commit, and expected SHA-256 are tracked.

```powershell
pwsh ./scripts/Test-StaticExtraction.ps1
```

Two consecutive exports produce the same SHA-256:

```text
BCEB7EE6EC4AEF592A27EC1053CFBE547D45D8171CE6510343C80CC94D04235A
```

The expectation is owned by
[`manifests/extractions/static-data.json`](../../manifests/extractions/static-data.json). A changed
hash requires reviewing the pinned inputs, parser, schema, and resulting semantic diff; it must not
be updated merely to make verification pass.

The four fixed-width tables now have an independent ROM decoder:

```powershell
pwsh ./scripts/Test-RomStaticParity.ps1
```

It reads only the locked ROM plus
[`rom-static-layout.json`](../../manifests/extractions/rom-static-layout.json), emits a schema-validated
raw/decoded document with SHA-256
`482F03ACB4B3D1856DE82A62BB043739C139654BB5BCCB5CD9D1D025730617D3`, then compares the decoded fields
against the separately parsed ASM-source document. All 281 fixed-width records currently agree with
zero field mismatches.

## ROM Table Map

Addresses use half-open ranges: `[start, endExclusive)`.

| Table | ROM range | Records | Fixed bytes/record |
| --- | --- | ---: | ---: |
| Spell names | `0x00F9C4..0x00FAD6` | 44 | variable |
| Ally names | `0x00FAD6..0x00FB8A` | 30 | variable |
| Item definitions | `0x016EA6..0x0176A6` | 128 | 16 |
| Spell definitions | `0x0176A6..0x01796E` | 89 | 8 |
| Item names | `0x01796E..0x017F3D` | 128 | variable |
| Class names | `0x017F3E..0x017FDA` | 32 | variable |
| Ally start definitions | `0x1EE7D0..0x1EE890` | 32 | 6 |
| Class definitions | `0x1EE890..0x1EE930` | 32 | 5 |

The fixed sizes are independently derived from range length divided by record count and are checked
on every H2 run. The item-name range ends before `0x017F3D`; the assembler listing confirms that
`align` writes one `$FF` padding byte at `0x017F3D`, then the class-name table starts at `0x017F3E`.
The padding is not part of either table.

## Canonical Entities

The generated document contains:

- 32 ally start slots, of which IDs 0–29 have ally enum/name records;
- 32 class definitions;
- 128 item definitions;
- 44 base spell enum/name records;
- 89 spell definitions representing levels, variants, and special entries.

Enum IDs and codes come from `disasm/sf2enums.asm`; display-name expressions and structured fields
come from the eight table files. H2 rejects broken references from ally slots to classes/items, item
use-spells to spell enums, and spell definitions to spell enums.

The two trailing ally start slots, IDs 30 and 31, both contain `RDBN`, level 1, and four `NOTHING`
items but have no ally enum or name. Their bytes and position are **Confirmed**; whether they are
padding, reserved slots, or reachable runtime records is **Unknown**.

Item ID 127 is another representation distinction worth preserving: its enum code is `NOTHING`,
while the display-name table and definition comment call it `Empty`. The canonical contract keeps
code, display name, and raw name expression as separate fields rather than normalizing them into one
ambiguous string.

## Upstream Documentation Drift

The syntax comment at the top of `spelldefs.asm` says spell `radius` is in the range 0–2. Definition
58, `LASER`, explicitly stores `radius 3`. The data and deterministic output are **Confirmed**;
therefore the project schema accepts 0–3. What radius 3 does at runtime remains **Unknown** and is an
H3 behavioral-test candidate.

This is the first concrete example of why prose comments cannot be the only source of truth. The
harness must validate actual records and preserve exceptions before design docs simplify a rule.

## Contract Boundary

Tracked and redistributable outputs from this slice are limited to:

- extractor and validation code;
- structural JSON Schema;
- counts, addresses, record widths, source hashes, and output hash;
- research conclusions that do not reproduce the full original data tables.

The generated JSON includes original names and game data, so it remains ignored alongside the ROM.
Downstream tools may consume it locally, but a distributable remake must replace or separately clear
copyrighted content.

## Confirmed Byte Packing

- Ally start record: class ID, level, then four item bytes; item bit 7 means equipped and bits 0–6
  hold item ID.
- Class record: movement byte, big-endian resistance word, movement type in the high nibble, and
  prowess byte.
- Item record: big-endian 32-bit equip flags, max/min range bytes, big-endian price word, item type,
  spell ID/level byte, then three effect/parameter byte pairs.
- Spell record: spell ID/level, MP cost, animation, properties, max/min range, radius, and power.
  Spell level is the upper two bits of its entry; animation uses five index bits, two variation bits,
  and one mirrored bit.

These statements are about storage. Names such as prowess, resistance, animation, and properties
still require code-path or runtime evidence before they become complete gameplay rules.

## What H2 Does Not Yet Prove

- Token names such as resistance, prowess, equip effects, spell properties, and animation variations
  are preserved; their complete runtime semantics are not established by this slice.
- The unused/reachable status of ally slots 30–31 is unknown.
- The runtime interpretation of spell radius 3 is unknown.
- Growth is owned by [`ally-growth.md`](./ally-growth.md), and promotions/enemy definitions by
  [`enemy-promotions.md`](./enemy-promotions.md). Shops and map/battle data remain outside this
  contract.

## Next Evidence Slice

Continue from the completed base-RNG H3 into action order, damage, and the exceptional
`LASER radius = 3` record. The next static-data expansion should cover one battle map without
weakening the existing dual-source parity rail.
