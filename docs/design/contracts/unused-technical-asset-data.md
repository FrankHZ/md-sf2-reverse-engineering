# Unused Technical Asset Data Contract

- **Confirmed original structure:** one 5,694-byte source container with four ordered Stack streams,
  four distinct 8,192-byte decoded results, two ordered 16-color palette records, one palette
  pointer, exact source/H1/ROM parity, and the bounded comment-stripped symbolic-reference inventory
  described below.
- **Inferred original behavior:** none promoted here. `Unused`, `Cloud`, and `Base` are preserved as
  source identities and classifications, not as proof of rendered content, purpose, or reachability.
- **Unknown original behavior:** raw-address, computed-pointer, or debug-only reachability; frame or
  animation order; palette assignment; VDP destination; cache or lifetime; VInt, DMA, and CRAM
  timing; transfer completion; rendered meaning; malformed-input behavior; replacement policy;
  accessibility treatment; and player-visible use.
- Remake status: implementation-neutral Phase 3 private-import contract; no public asset payload,
  renderer, runtime texture format, animation model, replacement artwork, or distribution policy is
  selected.
- Evidence date: 2026-08-13
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Contract Boundary

This contract defines a static, data-only import boundary for two source-classified technical
resources:

1. the single `tiles_UnusedCloud` container and its four ordered Stack-stream spans;
2. the two ordered palettes in `palette_UnusedBase` and the separate
   `p_palette_UnusedBase` pointer identity;
3. bounded source/H1/ROM provenance, aggregate decoder results, and static symbolic-reference facts;
4. a private lossless import form and a public metadata-only projection.

The sole executable owner consumed here is fixture id `sf2-unused-technical-assets-static-v1` in
[`tests/fixtures/h2/unused-technical-assets-static-v1.json`](../../../tests/fixtures/h2/unused-technical-assets-static-v1.json).
Its source-backed owners are [Technical Graphics](../../research/technical-graphics.md) and
[Technical Services](../../research/technical-services.md).

The broader `sf2-tech-services-static-v1` fixture is deliberately excluded. Its general service,
compression, display, and resource inventory is not an executable dependency of this contract.
Likewise, the Stack decoder's implementation belongs to the existing
[graphics-service contract](graphics-service-state.md); this data contract consumes decoded-output
evidence without creating a new decompression-service association.

The exact future research-index association set is:

- `tech.services.resource-title`;
- `tech.services.resource-base`.

Both records are unique and currently unassociated. No other `tech.services.*`, title-screen,
base-tile, special-screen, UI, startup, renderer, interrupt, DMA, CRAM, or presentation record gains
this contract.

## Pre-Contract Evidence Audit

The dedicated owner was reproduced from the accepted baseline:

```text
sf2-unused-technical-assets-static-v1
SHA256 E28FC7F30311411B9D1822CF810454674E02F4AB13A7292FE36AFAFFAC0F0F12
CloudStreams 4 / DecodedBytes 32768 / ParityBytes 5762 / PASS
```

The fixture directly binds exactly the two future-association records above. Neither currently has a
design contract, and no other record consumes this fixture. The same records also carry the excluded
aggregate technical-services evidence; that additional inventory owner does not broaden this
contract's semantics or association set.

The parity denominator is exact:

| Stored component | Bytes | Boundary |
| --- | ---: | --- |
| `tiles_UnusedCloud` container | 5,694 | one source payload containing four ordered streams |
| `palette_UnusedBase` | 64 | two ordered 16-color palettes |
| `p_palette_UnusedBase` | 4 | one longword pointer representation |
| **Total source/ROM parity** | **5,762** | all three stored components |

Original compressed bytes, decoded art, complete palette words, pointer bytes, rendered captures,
and other private/generated artifacts are not distributable contract content.

## Cloud Container and Ordered Stream Boundary

### Confirmed static

`tiles_UnusedCloud` is one 5,694-byte source container at ROM address `182176`. It must not be
normalized into one compressed stream. Exhaustive testing of every even container offset for a
Stack decode producing exactly 8,192 output bytes yields exactly four starts:

| Stream | ROM address | Start | End exclusive | Stored bytes | Logical input bits | Stored-span tail bits | Decoded bytes | Tiles |
| ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| 0 | 182176 | 0 | 1328 | 1,328 | 10,587 | 37 | 8,192 | 256 |
| 1 | 183504 | 1328 | 2696 | 1,368 | 10,900 | 44 | 8,192 | 256 |
| 2 | 184872 | 2696 | 4460 | 1,764 | 14,078 | 34 | 8,192 | 256 |
| 3 | 186636 | 4460 | 5694 | 1,234 | 9,829 | 43 | 8,192 | 256 |

The four ordered decoded results total 32,768 bytes, or 1,024 32-byte tiles, and have four distinct
decoded hashes. Distinct hashes prove only that the accepted decoded byte sequences differ; they do
not establish animation frames, a display order, or visible meaning.

The four stored spans retain `37 + 44 + 34 + 43 = 158` bits after their logical Stack terminators.
Those are stored-span tail bits. This contract does not call them padding and does not claim that
they are zero, stable under rebuilding, ignored by every possible consumer, or visually invisible.

### Implementation freedom

An importer may use an independently implemented decoder, predecode private data, or transform the
four results into a modern private asset form. Fidelity requires the accepted container identity,
ordered boundaries, four decoded byte results, and parity relationship to remain independently
verifiable. It does not require the original Stack command parser, bit reader, copy loop, history
representation, register allocation, or instruction order.

Aggregate command counters and other decoder diagnostics may be used to detect corpus drift. They are
verification metadata, not a codec microimplementation requirement and not evidence of authorial
intent for any particular command sequence.

## Base Palette and Pointer Boundary

### Confirmed static

`palette_UnusedBase` is a 64-byte payload at ROM address `2028966`, represented as two ordered
16-color Mega Drive palette records. `p_palette_UnusedBase` is a separate four-byte pointer identity
at ROM address `2023460` and points to that palette payload in the accepted source/ROM image.

Across the ordered 32 palette words:

- exactly 17 distinct source word values are observed;
- exactly four positions contain zero;
- the two palettes differ only at color indices 1 and 5;
- at index 1, the first value is `3822` and the second is `1198`;
- at index 5, the first value is `1184` and the second is `1728`.

The count of 17 is an observed distinct-value count. It is not a continuous or closed color domain,
an alias relation, a palette-role taxonomy, or a constraint on replacement artwork. The two ordered
palette identities and all private word positions must remain distinguishable even where values are
equal.

The source names do not prove which decoded stream, frame, surface, or screen would use either
palette. Pointer identity and parity do not prove a runtime dereference.

## Static Symbolic-Reference Inventory

After comments are removed from the pinned `disasm/code` ASM corpus, the accepted token counts are:

| Symbol | Comment-stripped occurrences | Accepted interpretation |
| --- | ---: | --- |
| `tiles_UnusedCloud` | 1 | its definition only |
| `palette_UnusedBase` | 2 | its definition and the pointer initializer |
| `p_palette_UnusedBase` | 1 | its definition only |

Therefore the fixture confirms zero symbolic ASM consumers for the cloud container and palette
pointer, while the palette payload has only its pointer reference beyond its definition. This is a
complete static inventory of those exact symbolic tokens in the accepted source tree.

It is not evidence of dead code or universal runtime unreachability. Raw addresses, computed
pointers, tables without the symbol token, injected state, debug-only routes, modified builds, and
other non-symbolic access remain outside the static claim. A zero count also supplies no result,
timing, rendering, or hardware behavior.

## Implementation-Neutral Import Model

One complete logical private import may use the following closed structures. Names are illustrative;
the identities and relationships are normative.

```text
UnusedTechnicalAssetCorpus {
  fixtureId
  sourceBaseline
  cloudContainer: CloudContainer
  basePaletteSet: BasePaletteSet
  symbolicInventory: SymbolicReferenceInventory
}

CloudContainer {
  sourceSymbol
  sourcePath
  romAddress
  storedByteCount = 5694
  privateStoredBytes
  privateStoredHash
  orderedStreams[4]: CloudStream
}

CloudStream {
  streamIndex
  romAddress
  startOffset
  endOffsetExclusive
  storedByteCount
  logicalInputBitCount
  storedSpanTailBitCount
  decodedByteCount = 8192
  tileCount = 256
  privateStoredSpan
  privateStoredHash
  privateDecodedBytes
  privateDecodedHash
}

BasePaletteSet {
  sourceSymbol
  sourcePath
  romAddress
  storedByteCount = 64
  orderedPalettes[2]: PrivatePalette16
  pointerIdentity: PalettePointer
  privatePayloadBytes
  privatePayloadHash
}

PrivatePalette16 {
  paletteIndex
  orderedWords[16]
}

PalettePointer {
  sourceSymbol
  sourcePath
  romAddress
  storedByteCount = 4
  targetRef
  privateStoredBytes
  privateStoredHash
}

SymbolicReferenceInventory {
  commentStrippedTokenCounts
  rawOrComputedAccessExcluded = true
}
```

The model keeps the container distinct from its streams, keeps stream order and stored spans intact,
and keeps the pointer distinct from its target. It does not require runtime loading, animation,
palette assignment, or rendering. Equal palette words may be deduplicated internally only when the
ordered source identities and full private word arrays remain recoverable and verifiable.

## Public Projection and Copyright Boundary

A public contract or report may retain only bounded metadata and provenance already exposed by the
accepted fixture, including:

- fixture identity, source baseline, ROM identity, symbols, paths, and addresses;
- aggregate byte, stream, tile, palette, color, parity, and symbolic-reference counts;
- the four ordered stream boundaries and their logical/tail bit counts;
- already tracked verification hashes;
- the two already tracked palette-difference rows;
- explicit Confirmed, Inferred, Unknown, and separate-owner labels.

The public projection must not contain original compressed bytes, decoded tile art, complete palette
words, pointer bytes, screenshots, rendered captures, or newly expanded payload representations.
Private import tooling may hold and hash those forms only in ignored local storage. Tests intended for
public distribution should use metadata, hashes already accepted by the fixture, or synthetic data.

## Cross-System Separation

- [Graphics Service State](graphics-service-state.md) owns the bounded Stack decompression service
  contract. It does not own these two payload identities, and this contract creates no decompression
  record association.
- [Special-Screen Asset Data](special-screen-asset-data.md) owns its dedicated title, witch,
  suspend, and ending asset corpora. The source path containing the unused cloud definition does not
  make this payload part of that accepted corpus.
- [UI Graphics Asset Data](ui-graphics-asset-data.md) and [UI Layout Data](ui-layout-data.md) own
  their dedicated UI resources and layouts; they provide no palette or cloud consumer evidence here.
- [Startup Control Flow](startup-control-flow.md) and the accepted base-tile owner retain system
  initialization and base-tile handoffs. The `UnusedBase` source identity does not imply startup use.
- [Interrupt, DMA, and Trap State](interrupt-dma-and-trap-state.md) owns bounded queue/service state,
  not transfer completion, CRAM cadence, or a runtime route for these resources.
- Renderer, animation, presentation, accessibility, replacement, licensing, and distribution choices
  remain future owners. No modern-engine policy is selected by this static contract.

## Judgment Boundary

### Confirmed

- exact `sf2-unused-technical-assets-static-v1` fixture identity and pinned provenance;
- exact source symbols, paths, six accepted addresses, and 5,762-byte parity denominator;
- one 5,694-byte container with exactly four ordered 8,192-byte Stack results at the accepted even
  offsets;
- four distinct decoded hashes, 32,768 decoded bytes, 1,024 tiles, and exact stored-span tail-bit
  counts totaling 158;
- two ordered 16-color palettes, one separate pointer identity, exact size/parity, two difference
  rows, 17 distinct observed values, and four zero positions;
- exact comment-stripped token occurrence inventory and zero symbolic-consumer result;
- private original content separated from the bounded public metadata projection.

### Inferred

- source identifiers and comments classify the payloads as `Unused`, `Cloud`, and `Base`; those
  labels are retained for provenance without promotion to runtime or player-facing meaning.

### Unknown

- whether any original raw-address, computed-pointer, debug, malformed, or modified-state path reaches
  either resource;
- frame/animation ordering, palette-to-stream assignment, VDP destinations, cache lifetime, and
  runtime modification;
- DMA, CRAM, VInt, transfer, hardware, rendering, and visible timing behavior;
- whether stored-span tail bits are padding, stable, zero, consumed by any alternate route, or
  invisible;
- player-visible meaning, accessibility, replacement, localization, licensing, and distribution
  policy;
- malformed payload admission, diagnostics, fallback, and recovery.

## H4 Acceptance Contract

A remake-facing H4 adapter passes this contract only when it can:

1. identify fixture `sf2-unused-technical-assets-static-v1`, its pinned baseline, two resource
   records, source symbols/paths, and six accepted addresses;
2. privately preserve one 5,694-byte cloud container and four ordered stream spans at offsets
   `0`, `1328`, `2696`, and `4460`, rather than treating the container as one stream;
3. reproduce the four accepted 8,192-byte decoded results and distinct private decoded hashes from
   private input without requiring the original Stack microimplementation;
4. preserve each stream's stored byte count, logical consumed-bit count, and exact stored-span tail
   count `37`, `44`, `34`, and `43`, without classifying the 158-bit total as padding;
5. privately preserve two ordered 16-word palettes, all source word positions, the distinct pointer
   identity/target relation, and exact source/ROM parity;
6. verify the two accepted palette-difference rows, 17 distinct observed values, and four zero
   positions without promoting them into a closed color or alias domain;
7. preserve the static token inventory as a source audit while keeping dead-code, reachability,
   rendering, and timing claims out of the acceptance result;
8. detect container truncation, stream reorder, boundary drift, decoded-output drift, palette reorder,
   pointer drift, parity mismatch, and accidental public payload disclosure through private or
   synthetic tests;
9. publish only the bounded metadata/provenance surface and never original compressed bytes, decoded
   art, complete palette words, pointer bytes, or captures;
10. leave animation, palette assignment, runtime reachability, cache/persistence, VDP/DMA/CRAM,
    presentation, malformed-input handling, replacement, accessibility, and licensing as separate
    owners or **Unknown**.

H4 may decode at import time, build time, or another private preprocessing stage. Those choices
conform only when the ordered identities, boundaries, private decoded results, palette/pointer
structure, parity, and public non-disclosure boundary remain independently verifiable.

## Evidence Matrix

| Contract surface | Evidence label | Exact owner | Preserved boundary |
| --- | --- | --- | --- |
| cloud container and streams | **Confirmed static** | `sf2-unused-technical-assets-static-v1`; [fixture](../../../tests/fixtures/h2/unused-technical-assets-static-v1.json) | one private 5,694-byte container, four ordered streams/results; no animation or visible meaning |
| Stack decoding | bounded decoded-output evidence | same fixture; [technical-graphics research](../../research/technical-graphics.md) | exact results/diagnostics; no codec microimplementation requirement |
| base palettes and pointer | **Confirmed static** | same fixture | two private ordered palettes plus distinct pointer; no runtime dereference or assignment claim |
| symbolic reference inventory | **Confirmed static** | same fixture; [technical-services research](../../research/technical-services.md) | exact comment-stripped token counts; zero symbolic consumers is not dead code |
| source labels | **Inferred taxonomy only** | pinned source identities/comments | `Unused`/`Cloud`/`Base` retained without player-facing or runtime meaning |
| general technical services | excluded executable owner | `sf2-tech-services-static-v1` | no aggregate fixture registration or sibling association |
| renderer and hardware services | separate owner / **Unknown** | graphics, interrupt, UI, startup, and presentation contracts | no VDP/DMA/CRAM cadence, completion, rendering, or reachability claim |
| public payload | prohibited | copyright/private-input boundary | bounded metadata only; original bytes, art, full palettes, pointers, and captures remain private |

## Open Questions

1. Can a bounded future runtime rail demonstrate raw-address, computed-pointer, or debug-only access to
   either resource without publishing original payloads?
2. If a route exists, what ordered stream selection, palette assignment, VDP destination, and visible
   composition does it use?
3. What validation, fallback, replacement, accessibility, and distribution policy should a remake
   adopt without treating source taxonomy as original behavior?

## Reproduction

```powershell
uv run sf2 h2 unused-tech-assets
uv run sf2 design-contracts test
uv run sf2 research-index test
```

Generated output remains under ignored `local/derived/unused-technical-assets-static.json`. Public
acceptance uses bounded metadata, provenance, aggregate counts, accepted hashes, and the two tracked
palette-difference rows—not original compressed bytes, decoded tile art, full palette words, pointer
bytes, captures, or other redistributed content.
