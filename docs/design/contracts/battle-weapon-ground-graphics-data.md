# Battle Weapon and Ground Graphics Data Contract

- Status: **Confirmed static resource identities, container partition, aliases, and decode shape**
- Evidence date: 2026-08-14
- Scope: original battle weapon, weapon-palette, and ground graphics corpora as a private,
  engine-neutral import

## Judgment Boundary

This contract owns the static identities and private import shape of the original weapon-sprite,
weapon-palette, and battle-ground graphics data. It does not own weapon or ground selection, loader
control, decompression service behavior, transfer completion, or presentation.

- **Confirmed**: one 23-slot weapon pointer table in source-order one-to-one relation with 23 source
  stream definitions; 42 contiguous four-byte weapon-palette entries; one 30-slot ground pointer
  table resolving to 27 source header owners with three exact aliases; 27 eight-byte ground headers
  resolving to ten source stream definitions; exact aggregate byte/decode/parity counts; and bounded
  aggregate decoder diagnostics reproduced by the dedicated fixture.
- **Inferred, non-normative**: names such as `Weapon`, `Ground`, `Palette`, `Tiles`, and `view`
  preserve source identities and bounded consumer vocabulary only. They do not establish visible
  equipment, angle, terrain, spatial arrangement, composition, palette meaning, or authorial intent.
- **Unknown / separate owner**: natural selection and reachability of every index; invalid indices
  and malformed data; visible angle or view choice; ground/background composition; rendered tile
  and palette meaning; placement, layering, CRAM/VInt/DMA cadence or completion, timing, caching,
  runtime modification, persistence, replacement policy, licensing, and presentation parity.

## Evidence Owner and Consumed Surface

The sole executable owner consumed by this contract is `sf2-battle-weapon-ground-decode-v1`
([fixture](../../../tests/fixtures/h2/battle-weapon-ground-decode-v1.json),
[verifier](../../../src/sf2tool/h2/battle_weapon_ground.py),
[schema](../../../schemas/h2-battle-weapon-ground-decode-fixture.schema.json), and
[manifest](../../../manifests/extractions/battle-weapon-ground-decode.json)). Its data prose owner is
[Technical Graphics and Decompression Services](../../research/technical-graphics.md), with bounded
loader context in [Battle Scene Engine](../../research/battle-scene-engine.md).

This data contract consumes:

- `table.groundTableAddress`, `table.weaponSpriteTableAddress`, and
  `table.weaponPaletteAddress`;
- every aggregate field in `summary` and both ordered rows in `sideSummaries`;
- the three tracked rows in `groundAliases`; and
- the accepted upstream, ROM, and canonical-output provenance.

The `function` identities are retained only as external consumer/service witnesses. Weapon and
ground selection, palette consumption, stream lookup, Stack or compressed-DMA handoff, and transfer
requests remain with [Battle Scene Command and Presentation Data](battle-scene-presentation.md).
`LoadStackCompressedData` and hardware-facing transfer services remain with
[Graphics Service State](graphics-service-state.md) and
[Interrupt, DMA, and Trap State](interrupt-dma-and-trap-state.md). None of those function records
gains this contract.

The complete generated `weaponSprites[23]`, `weaponPalettes[42]`, `groundHeaders[27]`, and
`groundTiles[10]` rows remain under ignored
`local/derived/battle-weapon-ground-decode.json`. Their complete graphs, source symbols, paths,
addresses, exact relative words, per-resource sizes, palette and decoded hashes, compressed-stream
details, and raw or decoded bytes are private verification inputs, not public contract payloads.

The aggregate `sf2-auxiliary-data-static-v1` fixture is explicitly excluded even though all three
target research records also carry that evidence.

## Direct Binding and Association Boundary

The dedicated fixture directly binds exactly seven research-index records:

| Record ID | Fixture role | Contract treatment |
| --- | --- | --- |
| `auxiliary.data.pt-grounds` | `pt_Grounds` at ROM address 1,802,280 | new association candidate |
| `auxiliary.data.pt-weaponsprites` | `pt_Weaponsprites` at ROM address 1,809,050 | new association candidate |
| `auxiliary.data.weaponpalette00` | contiguous weapon-palette root at ROM address 1,830,456 | new association candidate |
| `battle.scene.load-weapon-palette` | weapon-palette loader at 105,036 | unchanged; retained only by `battle-scene-presentation` |
| `battle.scene.load-weapon-sprite` | weapon-sprite loader at 105,052 | unchanged; retained only by `battle-scene-presentation` |
| `battle.scene.load-ground` | battle-ground loader at 105,092 | unchanged; retained only by `battle-scene-presentation` |
| `tech.graphics.stack-decompression` | `LoadStackCompressedData` at 7,752 | unchanged; retained only by `graphics-service-state` |

No other `auxiliary.data.*`, `battle.scene.*`, `tech.graphics.*`, item, ally/enemy definition,
actor-sprite, animation-sequence, background, terrain, effect, portrait, map-sprite, interrupt, DMA,
or presentation record is associated by this contract.

## Weapon Sprite and Palette Identities

The weapon-sprite table has 23 ordered pointer slots in source-order one-to-one relation with 23
source stream definitions and owner identities. The count 23 does not claim that every compressed
byte sequence or decoded hash is mutually distinct.

Each private stream decodes to exactly 8,192 bytes. The 23 streams occupy 21,314 compressed bytes
and decode to 188,416 bytes in total. The source-format statement that a decoded record contains four
64-tile views remains a bounded consumer/source witness in
[Battle Scene Command and Presentation Data](battle-scene-presentation.md); it is not a rendered
layout, angle-selection, or visibility rule in this data contract.

The weapon-palette root identifies 42 contiguous source entries. Every entry is exactly four bytes,
so this independent corpus occupies 168 bytes. These entries are source-named palette records, not
complete 16-color palettes or proof of color meaning. The complete private association between a
selector and any palette entry remains a presentation/consumer concern.

## Ground Pointer, Header, and Stream Identity

The ground pointer table has 30 ordered slots resolving to 27 source header owners. Three slots reuse
earlier owners:

| Ground slot | Header owner slot |
| ---: | ---: |
| 21 | 12 |
| 22 | 12 |
| 29 | 13 |

Each private source header occupies eight bytes:

1. one six-byte source palette prefix; and
2. one two-byte self-relative tileset word.

The 27 headers therefore occupy 216 bytes. Their 162 palette bytes are a subset of that header count
and MUST NOT be added to it again.

The complete private header-to-tileset graph resolves to ten source stream definitions and owner
identities. The count ten does not establish mutually distinct bytes or decoded hashes. Each stream
decodes to exactly 1,536 bytes; the ten streams occupy 6,434 compressed bytes and decode to 15,360
bytes in total.

The exact alias and header-to-stream relations are logical import identities. They do not prove that
all 30 slots are naturally selected, that aliased slots have different presentation meaning, or
that the source numeric order is an appropriate public remake API.

## Field-Exact Aggregate Accounting

The fixture's aggregate domains remain distinct:

```text
pointer slots             = 23 weapon + 30 ground                 = 53
graphic stream owners     = 23 weapon + 10 ground                 = 33
source-named palettes     = 42 four-byte entries + 27 six-byte prefixes = 69
palette bytes             = 168 weapon + 162 ground               = 330
```

The 162 ground palette bytes are already contained within the 216 ground-header bytes. Complete
stored source-object accounting therefore closes as:

```text
compressed stream bytes   = 27,748
standalone weapon palette =    168
complete ground headers   =    216
source payload bytes      = 28,132
```

It is incorrect to add `paletteByteCount=330` to this denominator: doing so would count the ground
palette prefixes twice.

The pointer-table ROM parity count is 53. The source-object ROM parity count is 102, partitioned as:

```text
23 weapon streams + 42 weapon palettes + 27 ground headers + 10 ground streams = 102
```

## Decode Shape and Aggregate Diagnostics

The 23 weapon and ten ground streams decode to 203,776 bytes in total. The dedicated verifier also
records these aggregate diagnostics:

| Diagnostic | Accepted value |
| --- | ---: |
| command groups | 753 |
| literal words | 7,308 |
| copy commands | 4,417 |
| copied words | 94,580 |
| maximum copy offset | 2,000 words |
| maximum copy length | 33 words |
| observed trailing span | 32..47 bits |

These values validate this corpus against the maintained decoder. They do not require a remake to
reproduce the original Stack microimplementation. The trailing span is only the stored span after
each logical terminator; it is not proven padding, zero-filled data, stability, or invisibility.

## Implementation-Neutral Logical Model

A complete private importer may use a model equivalent to:

```text
BattleWeaponGroundGraphicsCorpus {
    provenance {
        fixtureId
        upstreamCommit
        romSha256Identity
        verifierOutputSha256
    }
    weaponPointerSlots[23] {
        logicalWeaponGraphicId
        weaponStreamOwnerId
    }
    privateWeaponStreams[23] {
        logicalStreamId
        privateSourceIdentity
        privateSourceAddress
        privateCompressedBytes
        privateDecodedBytes[8192]
        privateHashesAndDecodeDiagnostics
    }
    privateWeaponPaletteEntries[42] {
        logicalPaletteEntryId
        privateSourceIdentity
        privateSourceAddress
        privateWords[2]
        privateHash
    }
    groundPointerSlots[30] {
        logicalGroundId
        groundHeaderOwnerId
    }
    privateGroundHeaders[27] {
        logicalHeaderId
        privateSourceIdentity
        privateSourceAddress
        privatePaletteWords[3]
        privateRelativeTilesetWord
        groundStreamOwnerId
    }
    privateGroundStreams[10] {
        logicalStreamId
        privateSourceIdentity
        privateSourceAddress
        privateCompressedBytes
        privateDecodedBytes[1536]
        privateHashesAndDecodeDiagnostics
    }
}
```

Complete source/H1/ROM addresses, big-endian pointer and relative-word storage, raw headers and
palettes, compressed bytes, decoded art, source paths, per-resource sizes/hashes, and other
non-public details are private import and round-trip evidence. The bounded root and external witness
symbols/addresses, aggregate provenance, and metadata named in the public projection remain public.
After verification, a conforming remake may use engine-native resource references, palettes,
textures, formats, and storage. It is not required to reproduce Mega Drive address space, big-endian
storage, the Stack codec, original buffers, or original file/container layout.

The importer MUST keep weapon pointer, weapon stream, weapon palette, ground pointer, ground header,
ground palette, and ground stream identities distinct. Aliased ground slots do not become duplicate
header owners, and shared ground streams do not become duplicate stream owners.

## Public and Private Projection

The public projection may retain only:

- fixture, upstream, ROM, and canonical-output provenance hashes;
- the three bounded table/root symbols and addresses;
- the three loader and one Stack-service external witness identities and addresses;
- aggregate and side-specific pointer, owner, alias, palette/header/stream, byte, decode, parity, and
  decoder-diagnostic counts;
- the three tracked ground alias rows; and
- the bounded weapon-stream, four-byte palette-entry, and ground-header/stream partitions.

It MUST NOT publish raw pointers, complete pointer/header/stream graphs, resource symbols/source
paths or addresses, per-resource offsets/sizes/hashes, palette words, compressed bytes, decoded art,
ROM excerpts, screenshots, emulator captures, or rendered presentation.

## H4 Remake Acceptance Surface

A future H4 implementation conforms when it can show that:

1. its private import retains 23 ordered weapon pointer slots and 23 source stream owners;
2. all 23 private weapon streams reproduce their accepted 8,192-byte decoded identities, separate
   from the 42 contiguous four-byte weapon-palette entries;
3. its private import retains 30 ordered ground slots, 27 header owners, the exact
   `21/22→12, 29→13` alias relation, and the complete header-to-ten-stream graph;
4. every ground header retains its six-byte palette identity separately from its self-relative
   tileset-word identity, and all ten streams reproduce their accepted 1,536-byte decoded identities;
5. complete private accounting closes at `27,748 + 168 + 216 = 28,132`, with 203,776 decoded bytes
   and without adding the 162-byte ground-palette subset twice;
6. engine-native resources can replace original pointers, relative words, Stack storage, and address
   layout without changing logical owners, aliases, palettes, headers, or stream relations; and
7. public reports expose only the bounded aggregate/provenance surface while copyrighted payloads
   and complete private identity material remain private.

H4 does not require original selectors, loader microimplementation, palette writes, staging or DMA
operands, CRAM/VInt behavior, rendered output, or timing. Those seams are tested by their owning
presentation/service contracts.

## Cross-System Separation

- [Battle Scene Command and Presentation Data](battle-scene-presentation.md) consumes canonical
  records from this contract and retains ally-only/invalid weapon selection, weapon-palette
  final-two-color consumption, the bounded four-view consumer seam, weapon Stack handoff, ground
  palette writes and self-relative lookup, compressed-DMA handoff and `0x300` request, scene
  chronology, presentation boundaries, and their Unknowns. It no longer independently owns or
  re-verifies this static catalog.
- [Graphics Service State](graphics-service-state.md) retains Stack decompression and compressed-DMA
  service boundaries. Aggregate decoder diagnostics here do not transfer that ownership.
- [Interrupt, DMA, and Trap State](interrupt-dma-and-trap-state.md) retains VInt, DMA, and CRAM
  service/timing boundaries.
- item and ally-definition contracts retain weapon identity/equipment fields; they do not own these
  graphics payloads or palette entries.
- actor battle sprites and animations, backgrounds, terrain, effects, portraits, special/map/UI
  graphics, localization, accessibility, licensing, replacement assets, and rendering remain with
  their own contracts or as Unknown.

## Evidence Matrix

| Contract area | Evidence label | Owner | Remaining boundary |
| --- | --- | --- | --- |
| 23 weapon pointers/streams and 42 palette entries | **Confirmed static/private import** | `sf2-battle-weapon-ground-decode-v1` | Selection, byte uniqueness, visual meaning |
| 30 ground slots, 27 headers, three aliases, ten streams | **Confirmed static/private import** | same fixture/verifier | Natural reachability, shared-stream meaning |
| aggregate bytes, decode shape, diagnostics, parity | **Confirmed static** | same fixture/verifier | Stack microimplementation, tail-bit meaning, rendered art |
| selector, loader, palette, relative lookup, service/DMA handoff | **Separate-owner Confirmed static witness** | `battle-scene-presentation` | Transfer completion, angle, placement, composition |
| Stack/compressed-DMA services | **Separate-owner Confirmed static** | `graphics-service-state` | Hardware/runtime timing |
| source-label visual intent | **Inferred, non-normative** | source vocabulary only | Equipment/terrain meaning, authorial intent |
| reachability, presentation, persistence, replacement | **Unknown / separate owner** | future bounded evidence or product decision | Not an H4 data-fidelity requirement here |

## Reproduction

```powershell
uv run sf2 h2 battle-weapon-ground
uv run sf2 design-contracts test
uv run sf2 verify
```

The complete private rows remain under ignored `local/derived/battle-weapon-ground-decode.json`.
They are reproducible private evidence, not tracked or distributable contract content.
