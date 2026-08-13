# Map Tileset Data Contract

- **Confirmed original structure:** the ordered 115-entry map-tileset pointer table, 115 Stack-
  compressed source resources with fixed 4,096-byte decoded forms, complete table/payload parity,
  the private 79-map five-slot reference relation, the private 32-animation reference relation, and
  their bounded public usage summaries described below.
- **Inferred original behavior:** none promoted here. Source symbols and owner prose identify map-
  graphics and animation use, but they do not prove final rendered meaning or runtime reachability.
- **Unknown original behavior:** natural reachability of each map and animation reference, dynamic or
  encoded access to `MapTileset029`, runtime cache/reload/modification and persistence, animation
  scheduling, palette and VRAM placement, VInt/DMA cadence, transfer completion, frame composition,
  final rendering, malformed or replacement input policy, accessibility treatment, and player-facing
  meaning.
- Remake status: implementation-neutral Phase 3 private-import contract; no runtime graphics format,
  renderer, cache, animation system, replacement asset policy, or distribution license is selected.
- Evidence date: 2026-08-13
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Contract Boundary

This contract defines the static storage, decoding, reference, parity, and private-import boundary
for the original map-tileset corpus:

1. the top-level pointer and ordered 115-entry resource table;
2. 115 distinct compressed source identities and 115 fixed-size decoded identities;
3. 79 ordered private map-header records containing five ordered tileset slots each;
4. 32 ordered private animation-header records referring to tilesets and tile-count metadata;
5. bounded public totals, usage counts, distributions, parity results, and provenance.

The sole executable owner consumed here is fixture id `sf2-map-tileset-decode-v1` in
[`tests/fixtures/h2/map-tileset-decode-v1.json`](../../../tests/fixtures/h2/map-tileset-decode-v1.json).
Its source-backed owners are [Map Content Tables and Binary Payload Parity](../../research/map-content.md)
and [Technical Graphics](../../research/technical-graphics.md). The bounded source roots are
`data/graphics/maps/maptilesets/entries.asm`, 115 private compressed resource files, the 79 map-header
sources under `data/maps/entries/mapXX/00-tilesets.asm`, the 32 present animation headers under
`data/maps/entries/mapXX/9-animations.asm`, and the resource consumer identities in
`code/common/maps/mapload.asm`.

The exact future research-index association is only `auxiliary.data.pt-maptilesets`. The fixture's
`p_pt_MapTilesets`, `LoadStackCompressedData`, `LoadMapTilesets`, and `LoadMapArea` identities remain
bounded provenance. They do not gain this contract and do not turn a private static-resource contract
into a pointer-interface, decompression-service, map-lifecycle, animation, transfer, or rendering
contract.

## Pre-Contract Evidence Audit

The dedicated owner was reproduced from the accepted baseline:

```text
sf2-map-tileset-decode-v1
SHA256 2EA6AB3485CAE4F92F31647C05233F0E1C07E81CCB02806706A51F9F0C1E087F
Tilesets 115 / DecodedBytes 471040 / UsedTilesets 114 / PASS
```

The fixture directly binds exactly one research-index record:

- `auxiliary.data.pt-maptilesets` — unique, currently unassociated, and the sole future association
  for this contract.

That record also carries the broad `sf2-auxiliary-data-static-v1` inventory owner. This contract does
not consume that aggregate fixture, import any sibling record, or use the aggregate inventory as
authority for this tileset corpus. Only `sf2-map-tileset-decode-v1` is executable evidence for this
document.

No `tech.interfaces.ptr-s08`, `LoadStackCompressedData`, `LoadMapTilesets`, `LoadMapArea`,
`map.data.*`, map-header, animation, graphics-service, interrupt, DMA, or presentation record gains
this contract. Those identities remain provenance, private reference inputs, or separate-owner
boundaries rather than semantic associations.

The tracked fixture contains bounded metadata only: addresses, corpus dimensions, compressed and
decoded totals, parity counts, reference totals, usage summaries, aggregate decoder diagnostics, one
unused index, one animation tile-count distribution, and two runtime questions. Raw compressed bytes,
decoded art, per-resource hashes, individual compressed sizes, complete map-slot assignments, complete
animation assignments, and rendered captures remain private/generated.

## Identity and Pointer Topology

**Confirmed static:** `p_pt_MapTilesets` is the top-level pointer at ROM `0x64000` (`409600`). It
resolves to `pt_MapTilesets` at ROM `0x6400C` (`409612`). The table contains 115 ordered longword
entries resolving by index to `MapTileset000` through `MapTileset114`.

The accepted loader/service identities retained as provenance are:

| Identity | ROM address | Contract meaning |
| --- | ---: | --- |
| `LoadStackCompressedData` | 7752 | Stack decoder entry identity only |
| `LoadMapTilesets` | 10722 | map-tileset consumer identity only |
| `LoadMapArea` | 11756 | map-area/animation consumer identity only |

All 115 pointer-table entries and all 115 compressed payload ranges match the accepted source, H1,
and ROM boundary. The fixture retains table and payload parity as two separate `115/115` counters;
pointer parity does not substitute for payload parity.

A private importer MUST preserve the order, numeric index, source symbol, source path, source address,
and source/decoded identity of every resource. It MUST NOT deduplicate or renumber resources merely
because compressed sizes, decoded hashes, or visible-looking content happen to match. Public evidence
may expose table identity, dimensions, counts, and parity, but not resource payloads or per-resource
hashes.

## Private Compressed and Decoded Corpus

**Confirmed static:** every one of the 115 private source resources is Stack-compressed and decodes to
exactly 4,096 bytes. The complete accepted corpus totals:

| Surface | Accepted value |
| --- | ---: |
| ordered compressed identities | 115 |
| ordered decoded identities | 115 |
| aggregate compressed bytes | 198,514 |
| decoded bytes per resource | 4,096 |
| aggregate decoded bytes | 471,040 |
| pointer-table parity | 115 |
| payload parity | 115 |

The fixture also records aggregate decoder diagnostics:

- 8,418 command groups;
- 111,246 literal words;
- 22,485 copy commands and 124,274 copied words;
- 32 through 47 trailing bits;
- maximum copy offset 2,000 words;
- maximum copy length 33 words.

These diagnostics confirm the accepted corpus and verifier boundary. They are not a requirement to
reproduce the original bit reader, history storage, command grouping, copy loop, register allocation,
instruction order, or trailing-bit treatment in a remake. Another private decoder may conform when it
reproduces the accepted ordered decoded outputs and provenance without exposing original content.

Private import retains compressed and decoded bytes plus their hashes for verification. The public
contract retains only aggregate byte counts, fixed decoded size, parity, diagnostics, and provenance.
Neither form assigns tile semantics, palette selection, layout position, transparency, animation role,
or player-visible meaning to individual decoded bytes.

## Ordered Map Reference Boundary

**Confirmed static:** the 79 private map headers each contain five ordered tileset slots. Across the
complete `79 * 5 = 395` positions:

| Surface | Accepted count |
| --- | ---: |
| real tileset references | 326 |
| absent-slot sentinel positions | 69 |
| unique ordinarily referenced tileset indices | 100 |

Every real reference is within index `0..114`; every absent position stores the accepted `255`
sentinel. All 79 six-byte palette/tileset header records match the source/H1/ROM boundary checked by
the dedicated owner.

The private importer MUST retain each map index and all five slot positions in source order, including
every sentinel. It MUST NOT collapse the relation into the set of 100 referenced indices, compact the
five positions, or assign an invented gameplay role to a slot solely from its ordinal position.

The public projection retains only the 79-map, 395-position, 326-reference, 69-sentinel, and 100-
unique-index totals. It does not publish the complete map-to-slot assignment graph. Static reference
does not establish natural story reachability, visit order, actual load frequency, or final rendering.

The palette byte sharing the six-byte map header belongs to
[map-palette-data](map-palette-data.md). This contract verifies the tileset-reference boundary without
claiming or duplicating palette identity, color-zero transformation, or presentation semantics.

## Ordered Animation Reference Boundary

**Confirmed static:** 32 maps have a private four-byte animation header. Each header contributes one
tileset index and one accepted tile-count value. The reference surface contains:

| Surface | Accepted count |
| --- | ---: |
| animation-bearing maps | 32 |
| animation tileset references | 32 |
| unique animation tileset indices | 15 |

The public tile-count distribution is:

| Tile count | Header count |
| ---: | ---: |
| 4 | 1 |
| 16 | 1 |
| 32 | 3 |
| 64 | 13 |
| 96 | 14 |
| **Total** | **32** |

The private importer retains the ordered map index, source path, header address, tileset index, and
tile count for each animation header. The public projection retains only the totals and distribution,
not the complete assignment list.

An animation header proves a static reference and bounded metadata shape. It does not prove callback
cadence, cache layout, replacement ordering, VRAM target, visible animation, frame timing, normal-story
reachability, or the meaning of the tile-count field beyond the accepted source/consumer boundary.
Those behaviors remain with [map-exploration](map-exploration.md) and its runtime owners.

## Combined Static Usage Boundary

**Confirmed static:** the union of ordinary map-slot and animation-header references reaches 114 of
the 115 resource indices. The one index absent from both complete static source surfaces is `29`, the
identity `MapTileset029`.

This is a static absence result only. It MUST NOT be rewritten as dead code, runtime unreachability,
unused content in every original mode, or permission to delete or renumber the resource. Dynamic or
encoded writes, debug paths, raw-RAM injection, modified content, and other caller behavior remain
**Unknown**.

A conforming private import preserves all 115 identities, including index 29. It may report the
accepted `combinedUsedTilesetCount=114`, `unusedTilesetCount=1`, and `unusedTilesetIndices=[29]` as
public metadata, but it may not discard the private resource based on those counters.

## Implementation-Neutral Import Model

The minimum complete logical import keeps private payloads and assignments separate from public
metadata:

```text
MapTilesetCorpus {
  privateResources[115]: PrivateMapTilesetResource
  privateMapHeaders[79]: PrivateMapTilesetHeader
  privateAnimationHeaders[32]: PrivateMapAnimationTilesetHeader
  publicSummary: MapTilesetPublicSummary
}

PrivateMapTilesetResource {
  tilesetIndex
  sourceSymbol
  sourcePath
  sourceAddress
  privateCompressedBytes
  privateDecodedBytes[4096]
  privateCompressedHash
  privateDecodedHash
}

PrivateMapTilesetHeader {
  mapIndex
  sourcePath
  mapAddress
  tilesetSlots[5]  // ordered indices or accepted 255 sentinel
}

PrivateMapAnimationTilesetHeader {
  mapIndex
  sourcePath
  headerAddress
  tilesetIndex
  tileCount
}

MapTilesetPublicSummary {
  fixtureId = "sf2-map-tileset-decode-v1"
  topLevelPointerAddress = 409600
  pointerTableAddress = 409612
  resourceCount = 115
  fixedDecodedBytesPerResource = 4096
  aggregateCompressedByteCount = 198514
  aggregateDecodedByteCount = 471040
  tableParityCount = 115
  payloadParityCount = 115
  mapCount = 79
  mapSlotCount = 395
  mapReferenceCount = 326
  absentMapSlotCount = 69
  uniqueMapReferenceCount = 100
  animationMapCount = 32
  animationReferenceCount = 32
  uniqueAnimationReferenceCount = 15
  animationTileCountDistribution
  combinedUsedResourceCount = 114
  unusedResourceIndices = [29]
  aggregateDecoderDiagnostics
  fixtureProvenance
}
```

This is a private import/provenance model, not a required renderer API, GPU texture format, cache,
scene graph, animation scheduler, or asset-bundle layout. A remake may transform private decoded data
into another internal representation only when it can still verify the accepted identities, order,
reference relations, decoded results, and intentional transformation provenance.

The public projection MUST NOT contain compressed payloads, decoded art, per-resource hashes,
individual compressed sizes, complete map-slot assignments, complete animation assignments, rendered
captures, or other original content. Public reports may retain only bounded metadata, aggregate
counts, distributions, parity results, unused-index result, addresses, and provenance.

## Cross-System Separation

This contract does not own:

- map selection, loading chronology, reload, working state, area selection, runtime block mutation,
  animation scheduling, or persistence, which remain with
  [map-exploration](map-exploration.md) and its evidence owners;
- blocksets, decoded 64-by-64 layouts, aliasing, collision, and passability, which remain with
  [map-layout-data](map-layout-data.md) and adjacent map owners;
- palette identities, color words, color-zero transformation, fades, or palette presentation, which
  remain with [map-palette-data](map-palette-data.md) and graphics owners;
- the Stack decompression service ABI or any required codec microimplementation, which remains with
  [graphics-service-state](graphics-service-state.md) and the technical-graphics owner;
- VInt scheduling, CRAM/VRAM DMA, interrupt cadence, transfer completion, and hardware timing, which
  remain with [interrupt-dma-and-trap-state](interrupt-dma-and-trap-state.md);
- map-sprite graphics, special sprites, UI graphics, battle graphics, portraits, tileset-to-layout
  composition, tile semantics, animation frames, or final visible presentation;
- `sf2-auxiliary-data-static-v1`, `tech.interfaces.ptr-s08`, `map.data.*`, function/service records,
  and all sibling associations;
- private original compressed bytes, decoded art, hashes, and complete assignment graphs;
- malformed, truncated, out-of-range, injected, modified, or replacement input admission;
- accessibility remapping, localization, story meaning, balance, or distribution policy.

The [map-design principles synthesis](../synthesis/map-design-principles.md) may consume the bounded
static resource facts while retaining these separations. It MUST NOT turn a static reference count
into normal-story reachability or source/ROM parity into rendered equivalence.

## Judgment Boundary

### Confirmed

- fixture/source provenance through `sf2-map-tileset-decode-v1` and
  `auxiliary.data.pt-maptilesets`;
- exact top-level pointer, table, decoder-entry, `LoadMapTilesets`, and `LoadMapArea` provenance
  identities/addresses;
- 115 ordered private compressed and decoded resource identities;
- fixed 4,096-byte decoded size, 198,514 compressed bytes, and 471,040 decoded bytes;
- complete 115-pointer and 115-payload parity;
- 79 ordered private five-slot map headers: 395 positions, 326 references, 69 sentinels, and 100
  unique ordinary indices;
- 32 ordered private animation headers, 15 unique animation indices, and the exact public tile-count
  distribution;
- combined static use of 114 resources and static absence of index 29 from both complete reference
  surfaces;
- aggregate decoder diagnostics as corpus-verification metadata, not a required decoder algorithm;
- public metadata/private original-content separation.

### Inferred

- none promoted by this contract.

### Unknown

- dynamic, encoded, debug, injected, or modified-content reachability of `MapTileset029`;
- natural-story reachability, load frequency, cache/reload behavior, and persistence of every reference;
- runtime modification, animation scheduling, replacement order, and final tileset-to-layout mapping;
- palette selection, VRAM placement, VInt/DMA cadence, transfer completion, frame composition, and
  rendered presentation;
- malformed or replacement input admission, diagnostics, and fallback behavior;
- modern runtime format, renderer, cache, accessibility transformation, replacement assets, and
  distribution policy.

## H4 Acceptance Contract

A remake-facing H4 adapter passes this contract only when it can:

1. identify fixture `sf2-map-tileset-decode-v1`, the pinned baseline, and the accepted pointer/table/
   consumer provenance identities;
2. privately preserve 115 ordered compressed identities and 115 ordered decoded identities without
   deduplication, renumbering, or dropping index 29;
3. reproduce the exact 4,096-byte decoded shape per resource and the aggregate 198,514 compressed /
   471,040 decoded byte counts from private accepted inputs;
4. verify complete 115-entry pointer-table and 115-payload parity while keeping original payloads and
   per-resource hashes private;
5. privately preserve all 79 ordered five-slot map headers, including 326 references and 69 sentinel
   positions, while publicly retaining only bounded aggregate counts;
6. privately preserve all 32 ordered animation headers and publicly reproduce their reference totals,
   15-unique-index count, and exact tile-count distribution;
7. reproduce the combined 114-used/one-statically-unreferenced result without claiming runtime
   unreachability or discarding `MapTileset029`;
8. detect pointer reorder, resource renumbering, payload truncation, decoded-size drift, lost sentinel,
   map-slot reassignment, animation-header reassignment, and private-source loss through private or
   synthetic tests;
9. permit an independent decoder implementation rather than requiring the original command grouping,
   copy loop, history representation, trailing-bit behavior, register use, or instruction order;
10. keep raw compressed bytes, decoded art, hashes, complete assignment graphs, screenshots, and other
    original content outside public fixtures and reports;
11. report map lifecycle, animation, persistence, palette/VRAM placement, VInt/DMA, presentation,
    malformed input, replacement, and accessibility policy through separate owners or as **Unknown**.

H4 may decode and transform private tileset data during an import build, lazily, or ahead of runtime.
Those choices conform only when the accepted identity/order/reference graph, decoded-output evidence,
and public non-disclosure boundary remain independently verifiable.

## Evidence Matrix

| Contract surface | Evidence label | Exact owner | Preserved boundary |
| --- | --- | --- | --- |
| pointer and payload corpus | **Confirmed static** | `sf2-map-tileset-decode-v1`; [fixture](../../../tests/fixtures/h2/map-tileset-decode-v1.json) | 115 ordered private resources, fixed decoded shape, exact parity; payloads/hashes remain private |
| ordinary map references | **Confirmed static** | same fixture; [map-content research](../../research/map-content.md) | 79 private five-slot headers and public totals; no natural reachability or runtime lifecycle claim |
| animation references | **Confirmed static** | same fixture; map-content/technical-graphics owners | 32 private headers and public distribution; no cadence, cache, transfer, or visible-frame meaning |
| combined static usage | **Confirmed static absence** | same fixture | 114 referenced and index 29 absent from static surfaces; runtime unreachability remains Unknown |
| decoder diagnostics | **Confirmed static metadata** | same fixture; [technical-graphics research](../../research/technical-graphics.md) | corpus diagnostics do not mandate codec microimplementation |
| auxiliary aggregate | excluded executable owner | `sf2-auxiliary-data-static-v1` | broad inventory supplies no registration or sibling association here |
| map construction and animation lifecycle | separate-owner evidence | [map-exploration](map-exploration.md) | private asset corpus does not own load order, mutation, cache, persistence, or animation runtime |
| decompression, DMA, and rendering | separate owner / **Unknown** | [graphics service](graphics-service-state.md); [interrupt contract](interrupt-dma-and-trap-state.md) | provenance identities do not prove transfer completion, timing, or final presentation |

## Open Questions

1. Can a future grouped runtime rail test whether `MapTileset029` is reachable through any original
   dynamic or encoded index write without publishing its payload or decoded art?
2. Which original runtime paths cache, replace, or reload decoded map tilesets after initial load, and
   how do animation references interact with those paths?
3. What explicit validation and replacement policy should a remake importer use for out-of-range
   indices, truncated compressed inputs, decoded-size drift, or intentionally modified resources?

## Reproduction

```powershell
uv run sf2 h2 map-tilesets
uv run sf2 design-contracts test
uv run sf2 research-index test
```

Generated output remains under ignored `local/derived/map-tileset-decode.json`. Public acceptance uses
bounded metadata and provenance, not original compressed payloads, decoded art, per-resource hashes,
or complete map/animation assignment graphs.
