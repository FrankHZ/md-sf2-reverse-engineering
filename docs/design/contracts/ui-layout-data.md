# UI Layout Data Contract

- **Confirmed original structure:** the complete assembled vanilla UI layout corpus, ordered
  spell-level pointer routes, diamond-border variants, direct tile assets, source/H1/ROM parity,
  and non-overlapping covered-address accounting described below.
- **Inferred original behavior:** none promoted here.
- **Unknown original behavior:** runtime allocation and mutation, text or tile overwrite, palette
  selection, VInt/DMA behavior, movement, clipping, rendered composition and timing, caller
  reachability, player-facing menu meaning, and the role of excluded alternate sources.
- Remake status: implementation-neutral Phase 3 import contract; no UI framework, rendering model,
  localization layout, accessibility policy, replacement artwork, or distribution license has been
  selected.
- Evidence date: 2026-08-20
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Contract Boundary

This contract defines the static identity and lossless private-import boundary for the UI layouts
assembled by the original vanilla build. It owns:

1. the 19 assembled source owners and their 5,116-byte source/H1/ROM parity boundary;
2. 27 ordered two-dimensional layout word grids, their dimensions, addresses, and metadata hashes;
3. the first-class 64-byte spell-level pointer table, including its source identity, address,
   provenance, hashes, and ordered 16-entry alias relation to ten layout targets;
4. four 48-byte diamond-border variants and four direct tile assets;
5. the exact 5,614-byte union of non-overlapping covered source/ROM address ranges;
6. a public H4 surface based on identities, dimensions, addresses, counts, and hashes rather than
   copyrighted layout or tile payloads.

It does not own the runtime window engine, menu admission, text composition, palette or DMA
selection, animation, input, audio, localization, accessibility, or final presentation. The adjacent
[window-system contract](window-system.md) owns the accepted eight-slot runtime allocation,
coordinate, movement-state, composition, and VInt/DMA call-order boundaries. Neither contract alone
proves a player-visible frame or caller lifecycle.

The executable owner is `sf2-ui-layout-static-v1` in
[`tests/fixtures/h2/ui-layout-static-v1.json`](../../../tests/fixtures/h2/ui-layout-static-v1.json),
implemented by [`src/sf2tool/h2/ui_layouts.py`](../../../src/sf2tool/h2/ui_layouts.py). The research
owners are the complete static UI layout sections of
[Common Menu Engines and Services](../../research/common-menus.md) and
[Technical Graphics](../../research/technical-graphics.md).

## Pre-Contract Evidence Audit

The owner was reproduced from current `main` on the evidence date:

```text
Contract sf2-ui-layout-static-v1
SHA256 3AE41F72AA12E5FA02A8DB2FCA34B8EDFE1722F07FA6130B4508D8B47DA37984
Status PASS
Layouts 27
LayoutWords 2394
Assets 4
```

The audit checked the fixture, verifier, source-owner prose, source/H1/ROM parity fields, all 36
fixture table bindings, and the current research index. It found an exact bijection between those
bindings and 36 currently unassociated records: 19 `auxiliary.data.static` records and 17
`ui.layout.static` records. Registration is intentionally deferred until preliminary semantic
acceptance; this document does not change the index.

The 36 exact table/address identities decompose into 27 layouts, one pointer table, four borders,
and four direct assets. The pointer table is not merely an edge list between the other resources; it
is an independently addressed, provenance-bearing resource.

The audit preserves these limits:

- `trackedUniqueByteCount = 5,614` counts bytes in non-overlapping covered address ranges. It does
  not claim that the ROM contains 5,614 distinct byte values.
- The four direct assets total 570 bytes, but the 72-byte alphabet highlight is already inside the
  5,116-byte assembled-source corpus. Only the three adjacent incbin resources, totaling 498 bytes,
  extend the covered union: `5,116 + 128 + 224 + 146 = 5,614`.
- The spell-level table is an ordered 16-entry route relation. A set of ten targets or the pair of
  counts `16/10` is insufficient because either representation would discard alias position.
- The two excluded source files are outside the assembled vanilla parity corpus. Their exclusion is
  not evidence that they are unused, runtime-unreachable, or unreachable in every alternate build.
- Original ordered word grids and tile payloads remain private inputs. The tracked fixture retains
  metadata and hashes rather than redistributing those bytes.

Issues #80 and #81 concern separate research owners and are not evidence dependencies of this
contract.

## Corpus and Coverage Accounting

**Confirmed static:** nineteen source owners assemble to 5,116 bytes. The complete accepted corpus
has the following logical components:

| Component | Confirmed count | Confirmed bytes |
| --- | ---: | ---: |
| assembled source owners | 19 | 5,116 |
| layout grids | 27 layouts / 2,394 words | 4,788 |
| spell-level pointer table | 1 table / 16 ordered entries / 10 unique targets | 64 |
| diamond-border variants | 4 | 192 |
| direct tile assets | 4 | 570 |
| non-overlapping covered union | — | 5,614 |

These rows are not additive partitions. Layouts, pointer bytes, borders, and the alphabet-highlight
asset are contained within the 5,116 assembled bytes. The price-tag blank, price-tag numbers, and
shop-item highlight payloads are adjacent incbin assets outside that assembled interval and add
exactly 498 more covered bytes. A remake importer MUST calculate coverage as an address-range union,
not by summing every descriptive subtotal.

All 19 assembled sources and all four asset payloads have source/H1/ROM parity in the accepted owner.
This closes their static storage identity, not their runtime use.

## Ordered Layout Grid Contract

**Confirmed static:** the 27 layout records contain 2,394 big-endian VDP attribute words, or 4,788
bytes. The accepted shapes range from the ten 3-by-2 spell-level indicators through menu, portrait,
status, alphabet, timer, and 32-by-12 battle-scene-background grids. Across the corpus the fixture
records 640 unique attribute words and 580 unique tile indexes. Priority, horizontal mirror,
vertical flip, and palette-selector bits are retained as static word metadata.

The diamond, magic, and item menu grids are each 18-by-6 (108 words). This shape is not an arbitrary
factorization of their byte counts: each owning menu loads `$1206` before its sole `CreateWindow`
request, `CreateWindow` consumes the high byte as width and low byte as height, and the same source
function later copies its corresponding 216-byte layout. The verifier checks all three consumer
relations in addition to source/H1/ROM parity.

A private original-data importer MUST preserve, for every layout:

- its source symbol and ROM address;
- width, height, and exact row-major word order;
- each complete 16-bit attribute word, rather than only its tile index;
- source/ROM hashes and its relationship to the owning source interval.

The word-grid requirement is a lossless private-import model. Public fixtures and compatibility
reports MUST use dimensions, addresses, hashes, aggregate attribute counters, or synthetic samples;
they MUST NOT publish the original ordered grids. Static palette and transform bits do not establish
runtime palette selection, clipping, movement, DMA order, or visible pixels.

## Spell-Level Pointer-Table Resource and Routing

**Confirmed static:** the pointer table occupies 64 bytes and contains 16 ordered longword entries.
Its source symbol is `pt_layouts_SpellLevelIndicator`, its ROM address is `0x110A4` (69,796), and
the owner proves source/H1/ROM parity for the table. A canonical private import retains that identity,
address, size, source and ROM hashes, and H1 provenance independently from the layouts it references.

The entries resolve to ten unique layout targets. Multiple positions intentionally alias the same
target, so the canonical resource and relation are:

```text
pointerTable(pt_layouts_SpellLevelIndicator) {
  route[0..15] -> layout target identity
}
```

An importer MUST retain the table as a first-class resource and retain all 16 route positions in
order with the target selected by each position. It MUST NOT normalize the table to a set of ten
targets or deduplicate it into ten anonymous resources. A modern runtime may use shared immutable
layout objects, provided the pointer-table identity, each original route, and its alias identity can
still be reproduced and tested.

The raw 64 pointer bytes remain private original data. Public fixtures and reports retain the table
identity, source symbol, address, size, provenance, hashes, and ordered target metadata without
publishing its raw payload.

The table does not by itself define spell-level arithmetic, caller validation, menu admission, or
the player-visible meaning of any route index. Those consumer semantics remain separate or
**Unknown**.

## Borders and Direct Assets

**Confirmed static:** four diamond-border variants each occupy 48 bytes, totaling 192 bytes. Four
direct tile assets have accepted sizes:

| Asset identity | Confirmed bytes | Coverage relation |
| --- | ---: | --- |
| price-tag blank | 128 | adjacent incbin; outside the 5,116 assembled interval |
| price-tag numbers | 224 | adjacent incbin; outside the 5,116 assembled interval |
| shop-item highlight | 146 | adjacent incbin; outside the 5,116 assembled interval |
| alphabet highlight | 72 | already inside the 5,116 assembled interval |

All identities, sizes, addresses, and hashes are import facts. Whether a runtime copies, recolors,
animates, masks, or displays any asset is outside this contract.

## Excluded Source Boundary

The accepted vanilla section layouts exclude exactly these two upstream source paths:

- `data/graphics/tech/windowborder/entries.asm`;
- `data/graphics/tech/windowlayouts/fighterministatuswindowlayout.asm`.

They receive no borrowed vanilla address or parity claim. An importer should retain them as explicit
out-of-corpus provenance records if a private source checkout contains them. Future evidence may
establish an alternate-build or runtime role; until then, intentional use, build selection, and
reachability are **Unknown**.

## Implementation-Neutral Import Model

The following is a logical contract, not an engine-class prescription:

```text
UILayoutCorpus {
  assembledSources[19] {
    sourcePath
    sourceAddressRange
    byteCount
    sourceHash
    romHash
  }

  layouts[27] {
    layoutId
    sourceSymbol
    romAddress
    width
    height
    orderedAttributeWords[]    // private import only
    layoutHash
    attributeMetadata
  }

  spellLevelPointerTable {
    pointerTableId
    sourceSymbol: pt_layouts_SpellLevelIndicator
    romAddress
    byteCount: 64
    sourceHash
    h1Provenance
    romHash
    rawPointerBytes[]          // private import only
    entries[16] {
      routeIndex
      targetLayoutId           // aliases remain explicit
    }
  }

  diamondBorders[4] {
    variantId
    romAddress
    byteCount
    payloadHash
  }

  directAssets[4] {
    assetId
    romAddress
    byteCount
    payloadHash
    coverageRelation
  }

  excludedSources[2] {
    sourcePath
    reason: outside-assembled-vanilla-corpus
  }

  coverage {
    assembledSourceBytes: 5116
    adjacentAssetBytes: 498
    nonOverlappingCoveredBytes: 5614
  }
}
```

The public form omits `orderedAttributeWords`, `rawPointerBytes`, and original asset payloads. It
retains the same identities, ordering, dimensions, addresses, sizes, relationships, provenance, and
hashes so that a user-provided private import can be validated without making copyrighted data a
repository dependency.

## Cross-System Separation

This static corpus and the runtime window system meet only at an explicit handoff: a caller or window
operation may select a layout identity, while the runtime system owns allocation, mutation,
composition, movement, and transfer behavior. This contract does not infer that every static layout
is admitted by every caller or rendered through one common route.

Keep the following outside this contract:

- window-slot allocation, capacity, deletion, and buffer ownership;
- runtime text, number, icon, portrait, or highlight writes;
- palette selection, priority interpretation, clipping, Plane-A composition, VInt, DMA, and timing;
- controller input, menu state, caller return behavior, audio, and campaign reachability;
- localization reflow, scalable UI, accessibility, responsive layout, and replacement art;
- licensing and distribution of any original layout, tile, screenshot, or captured frame.

## Fidelity, Modernization, and Copyright Boundary

Original-data compatibility requires preserving source symbols, addresses, dimensions, ordered grid
words, the first-class pointer-table identity/provenance/hash plus its ordered routes and aliases,
border and asset identities, exact coverage relationships, and accepted hashes when importing a
private original corpus.

A remake may deliberately choose a different resolution, coordinate system, widget toolkit, font,
palette, animation, input method, responsive layout, accessibility behavior, and newly authored art.
Those choices MUST be recorded separately from original-data fidelity, with an explicit adapter or
deviation report where original IDs remain relevant.

Original layout words, border bytes, tile bytes, screenshots, and rendered captures are
private/generated copyrighted inputs. Do not commit or redistribute them. Public builds require
newly authored or properly licensed UI assets.

## H4 Acceptance Surface

A remake-side importer or compatibility adapter can claim this contract only when automated tests
prove:

1. all 19 assembled source owners reproduce the accepted addresses, byte counts, and source/H1/ROM
   hashes for a private original input;
2. all 27 layout identities preserve width, height, 2,394-word total, exact row-major word order,
   full attribute words, and per-layout hashes;
3. the spell-level pointer table preserves its independent identity, source symbol, `0x110A4`
   address, 64-byte size, source/H1/ROM provenance and hashes, plus all 16 route positions in exact
   order and their alias relation to ten targets;
4. all four 48-byte border variants and four direct assets preserve identity, size, address, and
   hash;
5. coverage is computed as non-overlapping address ranges: 5,116 assembled bytes plus exactly 498
   adjacent bytes equals 5,614, without double-counting the 72-byte alphabet highlight;
6. both excluded sources remain explicitly outside the assembled vanilla parity corpus without an
   invented unused or unreachable status;
7. public fixtures and reports expose only metadata, hashes, and synthetic examples, never original
   layout grids, raw pointer-table bytes, or tile payloads;
8. runtime rendering, localization, accessibility, and intentional presentation changes are tested
   and reported separately from static original-data parity.

H4 does not require the remake to use Genesis VDP words or original layouts as its authoring or
runtime representation. It requires deterministic, provenance-preserving import when a private
original-compatible data source is used.

## Evidence Matrix

| Contract area | Evidence label | Executable owner | Remaining boundary |
| --- | --- | --- | --- |
| 19 source owners, 5,116 assembled bytes, 27 grids, 2,394 words, shapes, attributes, and parity | **Confirmed static** | `sf2-ui-layout-static-v1` ([`ui-layout-static-v1.json`](../../../tests/fixtures/h2/ui-layout-static-v1.json)) | Runtime selection, mutation, rendering, and caller reachability |
| first-class 64-byte spell-level pointer table, ordered 16-entry relation, and ten target identities | **Confirmed static** | `sf2-ui-layout-static-v1` ([`ui-layout-static-v1.json`](../../../tests/fixtures/h2/ui-layout-static-v1.json)) | Raw bytes stay private; route-index consumer semantics and player-facing meaning remain unclosed |
| four borders, four assets, and 5,614-byte non-overlapping coverage union | **Confirmed static** | `sf2-ui-layout-static-v1` ([`ui-layout-static-v1.json`](../../../tests/fixtures/h2/ui-layout-static-v1.json)) | Runtime copies, palette use, animation, and visible pixels |
| excluded window-border aggregate and fighter mini-status alternate | **Confirmed exclusion from vanilla assembled parity corpus** | `sf2-ui-layout-static-v1` ([`ui-layout-static-v1.json`](../../../tests/fixtures/h2/ui-layout-static-v1.json)) | Alternate-build intent, use, and reachability remain **Unknown** |
| eight-slot allocation, movement, composition, and transfer-call order | **Separate owner** | [window-system contract](window-system.md) | End-to-end presentation remains unclosed |
| localization, accessibility, replacement art, and distributable content | **Deliberate design** | Future product/content decisions | Requires provenance, licensing, and separate acceptance |

## Reproduction

```powershell
uv run sf2 h2 ui-layouts
uv run sf2 design-contracts test
uv run sf2 verify
```

The generated detailed output remains under ignored `local/derived/ui-layout-static.json`.
