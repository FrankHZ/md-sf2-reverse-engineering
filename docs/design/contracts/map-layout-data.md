# Map Layout Data Contract

- **Confirmed original structure:** the complete 77-owner block/layout payload corpus, its 79 map
  references, two shared-owner aliases, decoded block and layout dimensions, aggregate decoder
  command-family counts, and source/ROM parity boundaries described below.
- **Inferred original behavior:** none promoted here.
- **Unknown original behavior:** codec intent, malformed-stream recovery, dynamic or modified payload
  admission, collision and passability meaning, event-driven working-layout mutation, reload and save
  persistence, map-transition behavior, rendered VDP parity, presentation timing, and player-facing
  map meaning.
- Remake status: implementation-neutral Phase 3 private-import contract; no renderer, collision model,
  navigation representation, asset format, replacement map set, or distribution license has been
  selected.
- Evidence date: 2026-08-09
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Contract Boundary

This contract defines the static data and private-import boundary for the original blockset and map
layout corpus:

1. 77 block-payload owners and 77 paired layout-payload owners;
2. 79 ordered map references to those owners;
3. the exact two shared-owner relationships;
4. decoded block and layout shapes, corpus totals, index bounds, and aggregate decoder-family
   counters;
5. source/ROM parity for every owned compressed payload.

The executable owner is `sf2-map-layout-decode-v1` in
[`tests/fixtures/h2/map-layout-decode-v1.json`](../../../tests/fixtures/h2/map-layout-decode-v1.json).
The research owners are [Common Map Engine](../../research/common-maps.md) and
[Map Content Tables and Binary Payload Parity](../../research/map-content.md).

The fixture also binds the identities of `LoadMapLayoutData` and `LoadMapBlocks`. Both identities are
part of this data boundary, but `LoadMapBlocks` does not create a second research-index association.
The sole associated record remains `maps.map-layout`.

The existing [map-exploration contract](map-exploration.md) is the higher-level consumer. It owns
accepted map construction, setup, event, working-layout, entity, movement, and runtime handoff rules.
This contract does not duplicate those rules, change that contract's fixture registrations, or add
associations on its behalf.

## Pre-Contract Evidence Audit

The dedicated owner was reproduced from current `main` on the evidence date:

```text
sf2-map-layout-decode-v1
SHA256 EF5E7C493BC50AFDC64197D51350B2E69D5C9E33870FF56565694F79F39E3691
PayloadPairs 77 / DecodedBlocks 19771 / DecodedLayoutWords 315392 / PASS
```

The audit checked the fixture, verifier, owning research prose, ignored private output, and current
research-index binding. `sf2-map-layout-decode-v1` binds exactly one index record:

- `maps.map-layout`.

That record is unique and currently unassociated. It is this contract's exact future association
set. No aggregate fixture or second record is required.

The audit also preserves four denominator boundaries:

- 77 is the number of unique block/layout payload owners, not the number of map references;
- 79 is the number of map references, not the number of independently stored payload pairs;
- every owner contributes one compressed block payload and one compressed layout payload;
- maps 24 and 46 are references to existing owner pairs, not additional payload owners.

Tracked evidence contains aggregate metadata and parity results. Original compressed payloads,
decoded block words, decoded layout words, per-payload hashes, and rendered captures remain
private/generated.

## Owner and Reference Graph

**Confirmed static:** the original corpus has 77 independently stored payload pairs serving 79 map
references. The exact shared-owner relations are:

| Referencing map | Block owner | Layout owner |
| ---: | ---: | ---: |
| 24 | 23 | 23 |
| 46 | 7 | 7 |

Every other accepted map reference resolves to its own owner pair. An importer MUST preserve the
reference graph rather than expanding it into 79 anonymous copies or reducing it to an unordered set
of 77 resources.

Block and layout ownership are retained as two explicit references even though both aliases currently
point to the same owner map in each row. The data model MUST NOT infer that block and layout ownership
can never diverge in a modified import merely because the original two aliases move together.

The alias graph is a storage-identity fact. It does not establish runtime cache identity, shared
mutable working memory, event persistence, or transition behavior. Those remain separate-owner or
**Unknown**.

## Decoded Blockset Corpus

**Confirmed static:** the 77 block payloads decode to 19,771 ordered blocks. Each block contains a
3-by-3 grid of nine 16-bit tile words, producing 177,939 block words across the complete corpus.
Individual payloads contain from 22 through 666 blocks.

The public fixture retains these eight aggregate block decoder-family counts:

| Fixture field | Aggregate count |
| --- | ---: |
| `absoluteNewFlags` | 9,165 |
| `repeat` | 53,813 |
| `adjacent` | 23,792 |
| `relativeSameFlags` | 19,211 |
| `rightHistory` | 25,076 |
| `absoluteSameFlags` | 19,545 |
| `bottomHistory` | 11,765 |
| `relativeNewFlags` | 13,493 |

These are complete-corpus aggregate decoder facts. They are not per-map histograms, gameplay
categories, codec-design intent, performance requirements, or proof that every malformed stream has
a defined result. A remake may use a different internal decoder only if its private import reproduces
the same ordered block words from accepted original inputs.

Block order and every 16-bit word are part of the private imported identity. An importer MUST NOT
deduplicate equal blocks, reorder blocks, discard unknown word bits, or replace raw words with named
fields that cannot round-trip exactly.

## Decoded Layout Corpus

**Confirmed static:** every owned layout decodes to exactly 4,096 ordered 16-bit words arranged as
64 rows by 64 columns. The 77 layouts therefore contain 315,392 words or 630,784 decoded bytes.

The public fixture retains these six aggregate layout decoder-family counts:

| Fixture field | Aggregate count |
| --- | ---: |
| `nextBlock` | 19,540 |
| `upperHistory` | 7,938 |
| `copyLeft` | 102,770 |
| `leftHistory` | 28,303 |
| `copyUpper` | 145,135 |
| `literal` | 11,706 |

The six counts sum to the complete 315,392-word decoded layout corpus. This arithmetic closes the
aggregate output classification only. It does not publish which command family produced any specific
map position and does not assign collision, layer, terrain, or presentation meaning to a word.

Every decoded layout block index is within its paired blockset. The greatest accepted index is 665,
against the largest accepted blockset size of 666. This is a corpus-wide validity boundary, not a
general decoder policy for modified or malformed inputs.

A private import MUST preserve the 64-by-64 position of every raw word and its paired block-owner
identity. It MUST reject or explicitly report an original-fidelity import that changes word order,
changes an owner reference, or produces an out-of-range block index.

## Compressed Payload and Parity Boundary

**Confirmed static:** all 77 compressed block payloads and all 77 compressed layout payloads match
their accepted ROM ranges. The public parity counters are therefore:

| Payload class | Owner count | Source/ROM parity count |
| --- | ---: | ---: |
| compressed block payload | 77 | 77 |
| compressed layout payload | 77 | 77 |

The tracked fixture does not redistribute compressed bytes, decoded words, or per-payload hashes. A
private importer may retain those values under the user-owned local input boundary so it can prove
exact round trips, but a public report MUST expose only aggregate counts, dimensions, ranges, alias
metadata, parity results, fixture provenance, and non-content diagnostics.

Parity proves that the selected source payloads match the pinned original ROM. It does not prove
rendered equivalence, hardware timing, visual correctness, event behavior, or the intended meaning of
any tile or layout bit.

## Implementation-Neutral Import Model

The minimum complete logical import keeps owned storage separate from references:

```text
MapLayoutCorpus {
  blockOwners[77]: BlockPayloadOwner
  layoutOwners[77]: LayoutPayloadOwner
  mapReferences[79]: MapLayoutReference
  publicSummary: MapLayoutPublicSummary
}

BlockPayloadOwner {
  ownerMapId
  privateCompressedBytes
  privateCompressedHash
  privateSourceRange
  blocks[]: Block3x3
}

LayoutPayloadOwner {
  ownerMapId
  privateCompressedBytes
  privateCompressedHash
  privateSourceRange
  width = 64
  height = 64
  privateWords[4096]
}

Block3x3 {
  privateWords[9]
}

MapLayoutReference {
  mapId
  blockOwnerRef
  layoutOwnerRef
}

MapLayoutPublicSummary {
  payloadPairCount = 77
  mapReferenceCount = 79
  aliasReferences[2]
  decodedBlockCount = 19771
  decodedBlockWordCount = 177939
  decodedLayoutWordCount = 315392
  decodedLayoutByteCount = 630784
  blockCountRange = 22..666
  maximumLayoutBlockIndex = 665
  blockCommandFamilyTotals[8]
  layoutCommandFamilyTotals[6]
  blockPayloadParityCount = 77
  layoutPayloadParityCount = 77
  fixtureProvenance
}
```

The model is logical, not an engine API. `private*` fields remain local to the user's import and
verification process. A distributable implementation may replace them with project-owned maps, but
it MUST keep the reference/owner distinction and record an intentional content substitution instead
of presenting replacement data as extracted original content.

The model deliberately has separate owner collections and reference records. Storing decoded data
directly inside 79 map rows would erase the two original aliases. Storing only the two aliases would
erase the other 77 reference identities. Both layers are required.

## Cross-System Separation

This contract does not own:

- the palette or five tileset slots attached to a map definition;
- area, flag, step, roof, warp, item, animation, or setup records;
- working-layout copies, chest state, reset/reload behavior, or save persistence;
- collision, passability, pathfinding, entity placement, battle bounds, or terrain semantics;
- map-script selection, story reachability, transition control, or current-map state;
- renderer construction, VDP upload, scrolling, layers, animation cadence, or final pixels;
- malformed-stream recovery, mod admission, editor UX, accessibility, or balance.

The [map-exploration contract](map-exploration.md) and
[map-design synthesis](../synthesis/map-design-principles.md) may consume this static corpus while
retaining their own evidence and **Unknown** boundaries. They MUST NOT use the aggregate command
counts as a substitute for runtime or presentation evidence.

## Judgment Boundary

### Confirmed

- exact function identities and addresses for `LoadMapLayoutData` and `LoadMapBlocks`;
- 77 unique payload owners serving 79 references;
- both owner references for aliases 24 to 23 and 46 to 7;
- complete block/layout decoded dimensions, totals, count range, and maximum index;
- exact eight and six aggregate decoder-family counters;
- complete 77-plus-77 source/ROM parity.

### Inferred

- none promoted by this contract.

### Unknown

- why the original codec selected its command families or history rules;
- behavior for truncated, malformed, injected, or modified compressed streams;
- runtime caching and mutable sharing across aliased map references;
- collision, passability, event, persistence, transition, and story meaning;
- VDP-visible rendering, animation, timing, and player-facing presentation.

## H4 Acceptance Contract

A remake-facing H4 adapter passes this contract only when it can:

1. identify the executable fixture exactly as `sf2-map-layout-decode-v1` and retain its provenance;
2. preserve 77 block owners, 77 layout owners, and 79 separate map references without flattening or
   inventing owners;
3. preserve both references of map 24 to owner 23 and map 46 to owner 7;
4. reproduce 19,771 ordered 3-by-3 blocks, 177,939 ordered block words, and the 22-through-666
   per-owner count range from private accepted inputs;
5. reproduce 77 ordered 64-by-64 layouts, 315,392 layout words, 630,784 decoded bytes, and maximum
   block index 665, with every reference in range;
6. reproduce the exact eight block and six layout aggregate decoder-family counters without
   presenting them as per-map statistics or gameplay semantics;
7. verify all 77 block and 77 layout compressed payload parity relationships locally;
8. detect alias flattening, reference renumbering, word reordering, lost raw bits, and out-of-range
   indexes through synthetic or private-input tests;
9. keep compressed bytes, decoded words, per-payload hashes, and rendered output outside public
   fixtures and reports;
10. report collision, events, persistence, transitions, rendering, and timing as separate contracts
    rather than implicit success conditions here.

An H4 implementation may decode eagerly, lazily, or during an import build. Those choices are
conforming only when they preserve the complete private identity graph and the public non-disclosure
boundary.

## Evidence Matrix

| Contract surface | Evidence label | Exact owner | Preserved boundary |
| --- | --- | --- | --- |
| function identities | **Confirmed static** | `sf2-map-layout-decode-v1`; [fixture](../../../tests/fixtures/h2/map-layout-decode-v1.json) | `LoadMapLayoutData` and `LoadMapBlocks` identities/addresses only; no second record association |
| owner/reference graph | **Confirmed static** | same fixture; [common-map research](../../research/common-maps.md) | 77 owners, 79 references, and both exact aliases; no mutable runtime-sharing claim |
| decoded blocksets | **Confirmed static** | same fixture; [map-content research](../../research/map-content.md) | 19,771 3-by-3 blocks, 177,939 words, 22..666 range; raw words remain private |
| decoded layouts | **Confirmed static** | same fixture; [map-content research](../../research/map-content.md) | 77 64-by-64 layouts, 315,392 words, 630,784 bytes, maximum index 665; no collision or layer meaning |
| decoder-family totals | **Confirmed static** | same fixture | exact aggregate 8-plus-6 counters; no per-map classification, codec intent, or malformed-input behavior |
| compressed parity | **Confirmed static** | same fixture | 77 block plus 77 layout source/ROM matches; no public payloads or hashes |
| map construction and mutation | separate-owner evidence | [map-exploration contract](map-exploration.md) | runtime load phases, events, working-layout state, resets, and persistence are not duplicated here |
| rendering and presentation | **Unknown** | no consumed runtime/presentation fixture | VDP parity, scrolling, animation, visible timing, and final pixels remain open |

## Open Questions

1. Can a future grouped presentation rail compare private decoded maps with the complete VDP-visible
   result without publishing original graphics or layout content?
2. Do aliased references share any runtime cache or mutable state, or only immutable source storage?
3. Which malformed-stream cases need an explicit remake rejection policy rather than unspecified
   decoder behavior?
4. Which raw layout-word bits can eventually receive stable implementation-neutral names without
   losing round-trip fidelity?
