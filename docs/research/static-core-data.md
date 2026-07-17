# Static Core Data Contract

- Status: **Confirmed** for source structure, counts, ranges, and deterministic extraction
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

## What H2 Does Not Yet Prove

- The extractor currently parses SF2DISASM macro source. H1 proves that source rebuilds the ROM, but
  H2 does not yet independently decode these records from raw ROM bytes.
- Token names such as resistance, prowess, equip effects, spell properties, and animation variations
  are preserved; their complete runtime semantics are not established by this slice.
- The unused/reachable status of ally slots 30–31 is unknown.
- The runtime interpretation of spell radius 3 is unknown.
- Growth curves, ally stat progressions, learned-spell tables, promotions, shops, enemy definitions,
  and map/battle data are outside this contract.

## Next Evidence Slice

Implement raw-byte decoders for the four fixed-width tables and compare their fields with the
source-derived canonical records. This requires documenting macro bit packing rather than trusting
macro names. Once the two paths agree, add growth and learned-spell tables, then promote uncertain
battle semantics into emulator-backed H3 scenarios.
