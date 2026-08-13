# Map Palette Data Contract

- **Confirmed original structure:** the ordered 16-entry map-palette pointer table, sixteen 32-byte
  source palettes, complete pointer/payload/map-header parity, the ordered 79-map private reference
  surface and public usage histogram, the accepted Genesis color-word mask boundary, and the
  source-to-effective first-word transformation described below.
- **Inferred original behavior:** none promoted here. Source names and platform fields suggest palette
  and color intent, but they do not prove final visible meaning.
- **Unknown original behavior:** natural map reachability, runtime palette modification and cache
  lifecycle, reload or save persistence, palette animation, fade and transition behavior, CRAM/VInt/
  DMA cadence, transfer completion, hardware color presentation, final per-map rendering, malformed
  or replacement input policy, accessibility remapping, and player-facing meaning.
- Remake status: implementation-neutral Phase 3 private-import contract; no renderer, color-space
  conversion, palette animation model, replacement asset set, accessibility policy, or distribution
  license has been selected.
- Evidence date: 2026-08-13
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Contract Boundary

This contract defines the static storage, reference, parity, and private-import boundary for the
original map-palette corpus:

1. the top-level palette-table pointer and sixteen ordered pointer-table entries;
2. sixteen ordered source palette identities and their sixteen derived effective identities;
3. 79 ordered private map-header references and their public usage histogram;
4. bounded source color-word counts and the accepted `0x0EEE` mask check;
5. the source-shaped palette lookup/copy/first-word-clear chronology.

The sole executable owner consumed here is fixture id `sf2-map-palette-static-v1` in
[`tests/fixtures/h2/map-palette-static-v1.json`](../../../tests/fixtures/h2/map-palette-static-v1.json).
Its source-backed owners are [Map Content Tables and Binary Payload Parity](../../research/map-content.md)
and [Technical Graphics](../../research/technical-graphics.md). The bounded source roots are
`data/graphics/maps/mappalettes/entries.asm`, the 79 map-header sources under
`data/maps/entries/mapXX/00-tilesets.asm`, and `code/common/maps/mapload.asm`.

The exact future research-index association is only `auxiliary.data.pt-mappalettes`. The `LoadMap`
and `CopyBytes` identities are evidence within this fixture; they do not gain this contract and do
not turn a static resource contract into a map lifecycle, byte-copy implementation, DMA, or
presentation contract.

## Pre-Contract Evidence Audit

The dedicated owner was reproduced from the accepted baseline:

```text
sf2-map-palette-static-v1
SHA256 4F977B4B3EB8E731D2ABB6664F36030487DC186D267E66E9C2DAF3CB211007AB
Palettes 16 / Maps 79 / UsedPalettes 16 / PASS
```

The fixture directly binds exactly one research-index record:

- `auxiliary.data.pt-mappalettes` — unique, currently unassociated, and the sole future association
  for this contract.

That record also carries the broad `sf2-auxiliary-data-static-v1` inventory owner. This contract does
not consume that aggregate fixture, import any of its sibling records, or use its broader inventory
as authority for this palette corpus. Only `sf2-map-palette-static-v1` is registered as executable
evidence for this document.

No `map.data.*`, map-header, `LoadMap`, `CopyBytes`, graphics-service, fade, interrupt, DMA, or
presentation record gains this contract. Those identities remain provenance, private reference
inputs, or separate-owner boundaries rather than additional semantic associations.

The tracked fixture contains addresses, aggregate counts, complete usage counts, parity counts, the
effective-first-word rule, and one runtime question. Complete source/effective palette words,
payload bytes, per-palette hashes, per-map palette assignments, and rendered captures remain
private/generated.

## Identity and Pointer Topology

**Confirmed static:** `p_pt_MapPalettes` is the top-level table pointer at ROM `0x64004` (`409604`).
It resolves to `pt_MapPalettes` at ROM `0x9494A` (`608586`). The table contains sixteen ordered
longword entries resolving in index order to `MapPalette00` through `MapPalette15`.

Every entry refers to one independently stored 32-byte source payload containing sixteen big-endian
16-bit words. The complete source corpus therefore contains:

| Surface | Accepted count |
| --- | ---: |
| ordered palette identities | 16 |
| bytes per source palette | 32 |
| words per source palette | 16 |
| total source payload bytes | 512 |
| total source words | 256 |

All sixteen pointer-table entries and all sixteen payload ranges match the accepted ROM. The parity
counters are separately retained as `16/16`; a passing pointer table does not substitute for payload
parity, and equal-looking color words do not collapse resource identity.

A private importer MUST preserve the table order, each palette index, source symbol/path/address,
and source/effective relationship. It MUST NOT deduplicate palettes or reorder them merely because
some words, rows, or complete payloads appear equal. The public contract may expose the bounded table
and address metadata, but not the original payloads or per-palette hashes.

## Source Color-Word Boundary

**Confirmed static:** all 256 source words satisfy the accepted Genesis color mask `0x0EEE`. The
corpus contains 69 distinct observed source-word values.

The number 69 is a value-set count only. It is not:

- a continuous domain from zero through 68;
- a closed set of all values the hardware, engine, or modified content can accept;
- a color-name, semantic-role, visual-equivalence, or alias relation;
- proof that equal numeric words serve the same player-facing purpose on different maps.

Private import retains every ordered source word even when its numeric value repeats. Public evidence
retains only the 256-word total, the 69-distinct-value count, the mask result, payload dimensions,
parity counts, and provenance.

The mask check is static format evidence. It does not define analog output, brightness, gamma,
display calibration, emulator rendering, transparency, backdrop behavior, or a modern engine color
space.

## Ordered Map Reference Boundary

**Confirmed static:** the 79 map headers each contain one accepted palette index. Every header byte
matches the ROM, every reference is within index `0..15`, and every palette is referenced at least
once. The exact public usage histogram, ordered by palette index, is:

| Palette index | Map references |
| ---: | ---: |
| 0 | 47 |
| 1 | 2 |
| 2 | 3 |
| 3 | 6 |
| 4 | 6 |
| 5 | 2 |
| 6 | 2 |
| 7 | 1 |
| 8 | 1 |
| 9 | 1 |
| 10 | 2 |
| 11 | 1 |
| 12 | 2 |
| 13 | 1 |
| 14 | 1 |
| 15 | 1 |
| **Total** | **79** |

The complete private import retains the ordered mapping from each map index to its palette index.
The public contract publishes only the histogram and aggregate parity facts, not the per-map mapping.

Static reference by a map header does not establish natural story reachability, the order maps are
visited, whether every map is rendered during ordinary play, or whether runtime code later replaces
or modifies the selected palette. `usedPaletteCount=16` and `unusedPaletteCount=0` are complete static
header-reference facts only.

## Source-to-Effective First-Word Rule

**Confirmed static source shape:** the accepted `LoadMap` entry is at ROM `0x2A8C` (`10892`), and
the accepted `CopyBytes` identity is at `0x16D6` (`5846`). Within the bounded palette-loading source
sequence, the code:

1. obtains the palette table through `p_pt_MapPalettes` and selects the indexed source payload;
2. sets the destination identity to `PALETTE_1_BASE`;
3. sets the transfer count to the symbolic 32-byte palette size;
4. hands the source, destination, and count to `CopyBytes`;
5. clears the first word of `PALETTE_1_BASE` after the handoff returns through the ordinary source
   path.

Fifteen source palettes have a nonzero first word. One already has a zero first word. Applying the
accepted clear rule yields sixteen effective palettes whose first word is zero.

Source and effective palettes are distinct private identities even when the clear leaves a particular
first word unchanged. A private importer MUST retain the original source form and derive or verify the
effective form. It MUST NOT overwrite the only stored source representation and then claim a lossless
round trip.

This chronology does not import the microimplementation, performance, register preservation, error
behavior, or general contract of `CopyBytes`. It also does not prove a CRAM transfer, DMA completion,
VInt publication, rendered color, fade completion, visible frame, or presentation timing. The word
clear is a source-visible RAM transformation, not a full graphics-output observation.

## Implementation-Neutral Import Model

The minimum complete logical import keeps private source/effective payloads, private map references,
and the public summary separate:

```text
MapPaletteCorpus {
  privatePalettes[16]: PrivateMapPalette
  privateMapReferences[79]: PrivateMapPaletteReference
  publicSummary: MapPalettePublicSummary
}

PrivateMapPalette {
  paletteIndex
  sourceSymbol
  sourcePath
  sourceAddress
  privateSourceWords[16]
  privateEffectiveWords[16]
  privateSourceHash
  privateEffectiveHash
}

PrivateMapPaletteReference {
  mapIndex
  sourcePath
  mapAddress
  paletteIndex
}

MapPalettePublicSummary {
  fixtureId = "sf2-map-palette-static-v1"
  topLevelPointerAddress = 409604
  pointerTableAddress = 608586
  paletteCount = 16
  paletteByteCount = 512
  colorsPerPalette = 16
  sourceColorWordCount = 256
  distinctSourceWordValueCount = 69
  validColorMask = 0x0EEE
  validColorMaskCount = 256
  nonzeroSourceFirstWordCount = 15
  clearedEffectiveFirstWordCount = 16
  pointerTableParityCount = 16
  payloadParityCount = 16
  mapReferenceCount = 79
  mapHeaderParityCount = 79
  usedPaletteCount = 16
  unusedPaletteCount = 0
  usageCountsByPaletteIndex[16]
  fixtureProvenance
}
```

This is a private import/provenance model, not a required renderer API, GPU format, color object,
asset-bundle layout, cache, or scene lifecycle. A remake may convert private imported words into a
different runtime format only when it can still verify the source order, references, effective-first-
word rule, and intentional transformations.

The public projection MUST NOT contain raw source/effective words, palette payloads, per-palette
hashes, or the complete per-map assignment table. Public reports may retain bounded metadata,
aggregate counts, the exact usage histogram, addresses, parity results, the color-zero rule, and
non-content diagnostics.

## Cross-System Separation

This contract does not own:

- map definition parsing, map choice, palette-slot runtime selection, construction order, working
  layout state, reload behavior, or persistence, which remain with
  [map-exploration](map-exploration.md) and its evidence owners;
- blocksets, 64-by-64 layouts, aliasing, collision, or passability, which remain with
  [map-layout-data](map-layout-data.md) and other map owners;
- palette interpolation, transition timers, queue handoffs, display initialization, or flash state,
  which remain with [graphics-service-state](graphics-service-state.md);
- fade wait/control state, VInt scheduling, CRAM/VRAM DMA, interrupt cadence, or hardware timing,
  which remain with [interrupt-dma-and-trap-state](interrupt-dma-and-trap-state.md);
- tilesets, map-sprite graphics, special sprites, UI palettes, battle palettes, portraits, or
  special-screen assets;
- rendered composition, color-zero visual semantics, animations, final frames, screenshots, or
  player-facing presentation;
- private original palette words, payloads, hashes, or per-map assignments;
- malformed, truncated, out-of-range, injected, modified, or replacement input policy;
- accessibility remapping, localization, balance, story meaning, or campaign reachability.

The [map-design principles synthesis](../synthesis/map-design-principles.md) may consume these static
facts while retaining the same boundaries. It MUST NOT use a nonzero usage count as proof of normal
story reachability or use source/ROM parity as proof of rendered equivalence.

## Judgment Boundary

### Confirmed

- fixture/source provenance through `sf2-map-palette-static-v1` and
  `auxiliary.data.pt-mappalettes`;
- exact `p_pt_MapPalettes`, `pt_MapPalettes`, `LoadMap`, and `CopyBytes` identities/addresses;
- sixteen ordered source palette identities, 32 bytes and 16 words each, totaling 512 bytes and 256
  words;
- complete sixteen-pointer and sixteen-payload source/ROM parity;
- 69 distinct observed source-word values and all 256 words within mask `0x0EEE`;
- 79 ordered private map references, complete 79-header parity, exact public usage histogram, all
  sixteen indices used, and zero unused palettes;
- source-shaped lookup/copy/first-word-clear chronology, fifteen nonzero source first words, and
  sixteen cleared effective first words;
- public metadata/private original-payload separation.

### Inferred

- none promoted by this contract.

### Unknown

- whether and how all sixteen effective palettes render through original fade, transition, and
  per-map presentation paths;
- normal-story reachability of each map reference and runtime palette selection/modification;
- cache, reload, save, suspend, and cross-process persistence behavior;
- palette animation, CRAM/VInt/DMA cadence, transfer completion, interrupt timing, and final frames;
- hardware color conversion, color-zero transparency/backdrop meaning, brightness, gamma, and
  display-dependent output;
- malformed or replacement input admission, diagnostics, and fallback behavior;
- modern color-space conversion, accessibility remapping, replacement assets, localization, and
  distribution policy.

## H4 Acceptance Contract

A remake-facing H4 adapter passes this contract only when it can:

1. identify fixture `sf2-map-palette-static-v1`, the pinned baseline, and the accepted pointer,
   table, `LoadMap`, and `CopyBytes` provenance identities;
2. privately preserve sixteen ordered source palette identities and sixteen ordered effective
   identities without flattening repeated words or equal-looking payloads;
3. reproduce the exact 32-byte/16-word per-palette shape, 512-byte/256-word totals, and complete
   sixteen-pointer plus sixteen-payload parity from private accepted inputs;
4. verify every source word against mask `0x0EEE` and reproduce 69 distinct observed source-word
   values without turning that count into a closed, continuous, or semantic color domain;
5. privately preserve all 79 ordered map-to-palette references while publicly reproducing only the
   exact 16-entry usage histogram, 79 header parity count, all-used result, and zero-unused result;
6. preserve the source-shaped table lookup, 32-byte `CopyBytes` handoff, and subsequent first-word
   clear without requiring the original copy-loop microimplementation or claiming transfer completion;
7. preserve fifteen nonzero source first words and sixteen zero effective first words while retaining
   a lossless private source form;
8. detect pointer reorder, palette renumbering, reference reassignment, payload truncation, raw-word
   loss, mask violation, missing clear, and source/effective conflation through synthetic or private
   import tests;
9. keep raw words, payloads, per-palette hashes, complete per-map assignments, screenshots, and other
   original content outside public fixtures and reports;
10. report map lifecycle, persistence, animation, fade/presentation, CRAM/VInt/DMA, hardware output,
    malformed input, accessibility, and replacement policy through separate owners or as **Unknown**.

H4 may decode or transform private palette data during an import build, lazily, or ahead of runtime.
Those choices conform only when the accepted identities, reference graph, source/effective distinction,
and public non-disclosure boundary remain verifiable.

## Evidence Matrix

| Contract surface | Evidence label | Exact owner | Preserved boundary |
| --- | --- | --- | --- |
| pointer and payload corpus | **Confirmed static** | `sf2-map-palette-static-v1`; [fixture](../../../tests/fixtures/h2/map-palette-static-v1.json) | sixteen ordered resources, exact dimensions and parity; raw payloads/hashes remain private |
| source color-word boundary | **Confirmed static** | same fixture; [technical-graphics research](../../research/technical-graphics.md) | 256 mask-valid words and 69 distinct values; no continuous domain, alias, or visual meaning |
| map reference surface | **Confirmed static** | same fixture; [map-content research](../../research/map-content.md) | 79 private ordered references and exact public histogram; no natural reachability or runtime lifecycle claim |
| source-to-effective rule | **Confirmed static chronology** | same fixture and bounded `mapload.asm` source | 32-byte copy handoff then first-word clear; no CopyBytes microimplementation, DMA, transfer, or render completion |
| auxiliary aggregate | excluded executable owner | `sf2-auxiliary-data-static-v1` | broad inventory supplies no registration or sibling association here |
| map construction and reload | separate-owner evidence | [map-exploration](map-exploration.md) | palette corpus does not own map choice, construction, cache, mutation, or persistence |
| fades, interrupts, DMA, and rendering | separate owner / **Unknown** | [graphics service](graphics-service-state.md); [interrupt contract](interrupt-dma-and-trap-state.md) | state/control identities do not prove final colors, cadence, completion, or visible presentation |

## Open Questions

1. Can a future grouped presentation rail compare all sixteen private effective palettes through
   representative fade and transition paths without publishing original color words or frames?
2. Which runtime paths modify, cache, reload, or animate map palettes after the accepted initial
   source-to-effective transformation?
3. What explicit validation and replacement policy should a remake importer use for out-of-range map
   references, invalid color bits, or intentionally modified palette payloads?

## Reproduction

```powershell
uv run sf2 h2 map-palettes
uv run sf2 design-contracts test
uv run sf2 research-index test
```

Generated output remains under ignored `local/derived/map-palette-static.json`. Public acceptance uses
bounded metadata and provenance, not raw original palette content or per-palette hashes.
