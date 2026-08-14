# Special-Sprite Graphics Data Contract

- **Confirmed original structure:** the ordered ten-slot special-sprite pointer catalog, five initial
  payload-owner identities, five aliases, six source payload definitions, five palette-bearing
  resources plus one animation-only stream, exact aggregate source/compressed/decoded accounting,
  complete pointer/payload parity, and the bounded private-import surface described below.
- **Inferred original behavior:** none promoted here. Source classifications and resource identities
  do not establish player-visible purpose, frame order, animation, palette use, or presentation.
- **Unknown original behavior:** natural or forced runtime admission, malformed/debug/raw-RAM behavior,
  cache and resource lifetime, frame selection, palette appearance, VInt/DMA/CRAM cadence, transfer
  completion, rendering, replacement policy, accessibility treatment, and player-facing meaning.
- Remake status: implementation-neutral Phase 3 private-import contract; no runtime texture format,
  renderer, animation model, cache, replacement asset policy, or distribution license is selected.
- Evidence date: 2026-08-14
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Contract Boundary

This contract defines the static catalog, parity, decoding, alias, and private-import boundary for
the accepted original special-sprite graphics corpus:

1. ten ordered source pointer slots resolving to five initial payload-owner identities;
2. the complete five-pointer alias relation;
3. six ordered source payload definitions: five palette-bearing resources and one animation-only
   stream;
4. exact aggregate source, palette, compressed, decoded, decoder-diagnostic, and parity metadata;
5. private source/H1/ROM round-trip verification separated from a bounded public projection.

The sole executable owner consumed here is fixture id `sf2-special-sprite-decode-v1` in
[`tests/fixtures/h2/special-sprite-decode-v1.json`](../../../tests/fixtures/h2/special-sprite-decode-v1.json),
implemented by
[`src/sf2tool/h2/special_sprites.py`](../../../src/sf2tool/h2/special_sprites.py). Its source-backed
owner is [Technical Graphics and Decompression Services](../../research/technical-graphics.md).

Consumption is field-closed. This data contract consumes:

- fixture provenance and the three `table` identities;
- exactly these data-corpus fields in `summary`: `pointerCount`, `uniquePointerPayloadCount`,
  `aliasPointerCount`, `resourceCount`, `paletteCount`, `animationOnlyStreamCount`,
  `battleSizedStreamCount`, `explorationSizedStreamCount`, `sourceByteCount`, `paletteByteCount`,
  `compressedByteCount`, `decodedByteCount`, `commandGroupCount`, `literalWordCount`,
  `copyCommandCount`, `copiedWordCount`, `minimumTrailingBits`, `maximumTrailingBits`,
  `maximumCopyOffsetWords`, `maximumCopyLengthWords`, `pointerTableRomParityCount`, and
  `payloadRomParityCount`;
- the private canonical output's ordered `aliases` and `resources` for import verification.

It does not consume the remaining dispatch, route, source-reference, or regular-sentinel summary
fields, nor `function`, `routing`, `regularSentinelReferences`, or `runtimeQuestions`, as data
fidelity. Those fields support service/routing ownership in
[graphics-service-state](graphics-service-state.md) or preserve open research boundaries. The broad
`sf2-auxiliary-data-static-v1`, aggregate `sf2-tech-graphics-static-v1`, and separate
`sf2-map-sprite-assignments-static-v1` fixtures are not executable owners for this contract.

The exact future research-index associations are only:

- `auxiliary.data.pt-specialsprites`;
- `auxiliary.data.specialsprite-taros`.

The same dedicated fixture also binds four existing service records. They remain unchanged and
associated only with `graphics-service-state`:

- `tech.graphics.animate-special-sprite`;
- `tech.graphics.special-sprite-anims`;
- `tech.graphics.special-sprites`;
- `tech.graphics.stack-decompression`.

## Pre-Contract Evidence Audit

The dedicated owner was reproduced from the accepted baseline:

```text
Contract sf2-special-sprite-decode-v1
SHA256 E3DF0CEDBA48E8A5BB30D639868B9CB90C6C4FFA660D8ADA56DAB9969CEEFCA7
Pointers 10
Streams 6
DecodedBytes 16704
FullyRoutedIds 9
Status PASS
```

The fixture-linked research-index denominator is exactly six: the two currently unassociated data
candidates above plus the four already associated service records. No aggregate auxiliary record,
map-sprite assignment record, entity record, dialogue record, renderer record, or presentation record
gains this contract.

The denominator distinctions are normative:

- ten pointers mean five initial payload-owner identities plus five aliases;
- six resources mean five palette-bearing payloads plus one animation-only stream;
- resource definitions and owner identities do not prove byte-hash uniqueness;
- the route inventory remains nine fully routed IDs, one pointer-only ID, and six unbacked IDs, but
  that classification belongs to the service contract rather than this data H4 surface.

## Ordered Pointer and Payload Catalog

**Confirmed static:** `pt_SpecialSprites` begins at ROM address `154620`. Its ten ordered longword
slots resolve to five initial payload-owner identities. Five slots are aliases to an earlier owner.
A private importer MUST preserve the complete ordered slot-to-owner relation rather than flattening it
to an unordered five-resource set or synthesizing duplicate resources to erase aliases.

The first source payload begins at ROM address `155126`, and the accepted contiguous payload corpus
ends at ROM address `161868`. These three addresses are provenance and round-trip boundaries. A remake
runtime may use engine-native identifiers and references after private verification; it is not
required to reproduce Mega Drive addresses, big-endian pointer storage, or the original in-memory
table layout.

The six ordered source definitions consist of five palette-bearing payloads and one distinct
animation-only stream. Five decoded streams have the fixture's battle-sized classification and one
has its exploration-sized classification. These are bounded source/corpus categories, not claims
about natural callers, visible battle/exploration behavior, animation state, or presentation.

The complete pointer graph, resource symbols, source paths, individual addresses and sizes, private
hashes, palette words, compressed bytes, and decoded art remain private import material.

## Corpus Accounting and Decoder Diagnostics

**Confirmed static:** the private source corpus has this exact aggregate accounting:

| Surface | Accepted value |
| --- | ---: |
| ordered pointer slots | 10 |
| initial pointer payload-owner identities | 5 |
| pointer aliases | 5 |
| source payload definitions | 6 |
| palette-bearing resources | 5 |
| animation-only streams | 1 |
| battle-sized decoded streams | 5 |
| exploration-sized decoded streams | 1 |
| source bytes | 6,742 |
| palette bytes | 160 |
| compressed bytes | 6,582 |
| decoded bytes | 16,704 |
| pointer-table parity | 10 |
| payload parity | 6 |

The byte denominator is exact: `160 + 6582 = 6742`. Palette bytes are contained in source bytes and
must not be added a second time.

The accepted canonical output also records aggregate decoder diagnostics:

- 262 command groups;
- 3,491 literal words;
- 653 copy commands and 4,861 copied words;
- trailing-bit range 36 through 44;
- maximum copy offset 960 words;
- maximum copy length 33 words.

These values verify the accepted private corpus. They do not require the remake to reproduce the
original Stack bit reader, history representation, command parser, copy loop, register allocation,
or instruction order. The 36-through-44 trailing bits are stored-span tails beyond logical
terminators; they are not proven padding, zeroes, stability, invisibility, or presentation data.

## Implementation-Neutral Import Model

The minimum logical model keeps private catalog fidelity separate from public metadata:

```text
SpecialSpriteGraphicsCorpus {
  privatePointerSlots[10]: PrivateSpecialSpritePointerSlot
  privateResources[6]: PrivateSpecialSpriteResource
  publicSummary: SpecialSpriteGraphicsPublicSummary
}

PrivateSpecialSpritePointerSlot {
  orderedSlotIndex
  payloadOwnerRef
  aliasOwnerSlot
}

PrivateSpecialSpriteResource {
  orderedResourceIndex
  sourceIdentity
  sourcePath
  sourceAddress
  sourceByteCount
  paletteByteCount
  privatePaletteBytes
  privateCompressedBytes
  privateDecodedBytes
  privateSourceHash
  privatePaletteHash
  privateDecodedHash
  aggregateDecoderContribution
}

SpecialSpriteGraphicsPublicSummary {
  fixtureId = "sf2-special-sprite-decode-v1"
  pointerTableAddress = 154620
  firstPayloadAddress = 155126
  corpusEndAddress = 161868
  pointerSlotCount = 10
  initialPayloadOwnerCount = 5
  aliasPointerCount = 5
  resourceCount = 6
  paletteCount = 5
  animationOnlyStreamCount = 1
  battleSizedStreamCount = 5
  explorationSizedStreamCount = 1
  sourceByteCount = 6742
  paletteByteCount = 160
  compressedByteCount = 6582
  decodedByteCount = 16704
  pointerTableParityCount = 10
  payloadParityCount = 6
  aggregateDecoderDiagnostics
  fixtureProvenance
}
```

This model is a private import/provenance boundary, not a renderer API, texture atlas, animation
controller, DMA plan, map-entity component, or runtime asset-bundle layout. A conforming importer may
decode and transform private content into engine-native forms only when the ordered slot/alias graph,
resource identities, decoded outputs, parity, and transformation provenance remain verifiable.

The public projection MUST NOT contain raw palettes, compressed streams, decoded art, per-resource
hashes or sizes, complete resource paths/addresses, or the full pointer/alias graph. Public fixtures
and reports may retain only the bounded aggregate counts, ranges, three table/provenance addresses,
canonical digest, parity, decoder diagnostics, and provenance listed above.

## Cross-System Separation

This contract does not own:

- special-sprite load/update functions, exact `9 + 1 + 6` route classification, battle-versus-
  exploration dispatch, palette-4 chronology, immediate-versus-queued transfer seams, or the
  `table_2784C` service identity, which remain with
  [graphics-service-state](graphics-service-state.md);
- the Stack decompression service ABI or codec microimplementation, also owned by
  `graphics-service-state`;
- built map-sprite assignment domains, which remain with
  [map-sprite-assignment-surface](map-sprite-assignment-surface.md) and its accepted Common Scripting
  owner; that result is not consumed as executable evidence here;
- regular map-sprite graphics, special-screen graphics, battle/UI/portrait corpora, entity definitions,
  map population, dialogue properties, or caller admission;
- animation sequence, frame selection, palette application, cache lifetime, persistence, VInt/DMA/
  CRAM scheduling, transfer completion, hardware timing, rendering, or visible presentation;
- `sf2-auxiliary-data-static-v1`, `sf2-tech-graphics-static-v1`,
  `sf2-map-sprite-assignments-static-v1`, or any sibling research-index association;
- malformed, injected, corrupt, debug, raw-RAM, or replacement input policy;
- accessibility, localization, story meaning, balance, replacement assets, or distribution policy.

The separate built-assignment owner confirms only that its complete accepted original input domains
do not write IDs `237..250`. That is a separate-owner **Confirmed static** boundary, not universal
runtime unreachability and not part of this data contract's H4 requirements.

## Judgment Boundary

### Confirmed

- fixture/source provenance through `sf2-special-sprite-decode-v1` and the two exact auxiliary data
  associations;
- three accepted table/corpus provenance addresses;
- ten ordered private pointer slots, five initial payload-owner identities, and the complete five-
  pointer alias relation;
- six private source definitions comprising five palette-bearing resources and one animation-only
  stream;
- exact `6742 = 160 + 6582` source accounting and 16,704 decoded bytes;
- complete 10-pointer and 6-payload parity;
- aggregate decoder diagnostics as corpus-verification metadata, not required codec logic;
- public aggregate metadata separated from private original content.

### Inferred

- none promoted by this contract.

### Unknown

- natural or forced runtime admission, load frequency, cache lifetime, reload behavior, and
  persistence;
- visual meaning of source identities, size classes, palettes, decoded forms, or the animation-only
  stream;
- animation/frame selection, DMA/VInt/CRAM cadence, transfer completion, hardware timing, and final
  rendering;
- malformed, debug, corrupt, raw-RAM, or modified-content behavior;
- replacement input admission, diagnostics, fallback, accessibility, and distribution policy.

## H4 Acceptance Contract

A remake-facing H4 importer passes this contract only when it can:

1. identify fixture `sf2-special-sprite-decode-v1`, the pinned baseline, and the three accepted
   table/corpus provenance addresses;
2. privately preserve all ten ordered pointer slots, five initial payload-owner identities, and the
   complete five-pointer alias relation without flattening or duplicating owners;
3. privately preserve all six ordered source definitions, keeping five palette-bearing resources
   distinct from the one animation-only stream without claiming byte-hash uniqueness;
4. reproduce the accepted private decoded outputs and exact aggregate `6742 = 160 + 6582` source,
   16,704 decoded, 10-pointer parity, and 6-payload parity results;
5. detect pointer reorder, alias reassignment, resource reorder, palette-boundary drift, payload
   truncation, decoded-output drift, and provenance loss through private or synthetic tests;
6. treat trailing bits as stored-span tails only and decoder counters as corpus diagnostics rather
   than requiring the original Stack microimplementation;
7. permit engine-native references and runtime formats after private round-trip verification without
   requiring Mega Drive addresses, big-endian pointer storage, or original table layout;
8. keep raw palettes, compressed streams, decoded art, per-resource hashes/sizes, complete source
   paths/addresses, and the full alias graph outside public fixtures and reports;
9. consume routing and loader behavior from `graphics-service-state` rather than promoting the
   fixture's service fields into a second data owner;
10. report admission, cache/persistence, animation, palette use, VInt/DMA/CRAM, presentation,
    malformed input, replacement, and accessibility policy through separate owners or as **Unknown**.

H4 may import, decode, and transform private special-sprite data ahead of runtime or lazily. Those
choices conform only when the accepted catalog identities, alias topology, decoded evidence, parity,
provenance, and public non-disclosure boundary remain independently verifiable.

## Evidence Matrix

| Contract surface | Evidence label | Exact owner | Preserved boundary |
| --- | --- | --- | --- |
| pointer/resource catalog and parity | **Confirmed static** | `sf2-special-sprite-decode-v1`; [fixture](../../../tests/fixtures/h2/special-sprite-decode-v1.json) | 10 private slots, 5 initial owners, 5 aliases, 6 resources; complete graph and assets remain private |
| corpus accounting and decoder diagnostics | **Confirmed static** | same fixture; [technical-graphics research](../../research/technical-graphics.md) | `6742 = 160 + 6582`, decoded 16,704, parity 10/6; no required Stack microimplementation |
| load/update routes, dispatch, palette and transfer seams | separate-owner **Confirmed static** | [graphics-service-state](graphics-service-state.md) | exact `9 + 1 + 6` service classification; no duplicate route H4 here |
| complete built assignment exclusion for IDs `237..250` | separate-owner **Confirmed static** | [map-sprite-assignment-surface](map-sprite-assignment-surface.md) and [Common Scripting](../../research/common-scripting.md) | not consumed evidence; malformed/debug/raw-RAM reachability remains Unknown |
| runtime admission, animation, DMA/VInt/CRAM and presentation | **Unknown** | future runtime/presentation evidence | source identities and decoded forms do not prove visible behavior |
| replacement assets, accessibility and distribution | deliberate design | future product/content decisions | requires independent provenance and acceptance |

## Reproduction

```powershell
uv run sf2 h2 special-sprites
uv run sf2 design-contracts test
uv run sf2 verify
```

Detailed aliases, resource records, original palettes/streams, decoded art, and private hashes remain
under ignored `local/derived/special-sprite-decode.json`. Public tracked evidence retains only the
bounded metadata, digest, aggregate diagnostics, parity, and provenance described above.
