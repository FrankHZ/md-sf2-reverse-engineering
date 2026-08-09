# Special-Screen Asset Data Contract

- **Confirmed original structure:** the complete nine-resource Stack-compressed special-screen tile
  corpus, twelve-resource uncompressed palette/layout corpus, witch-menu palette and ordered bubble
  table, source/H1/ROM parity boundaries, and the bounded transfer metadata described below.
- **Inferred original behavior:** none promoted here.
- **Unknown original behavior:** the contents, initialization, stability, and visible use of five
  fixed-transfer tails; palette upload order; fades; layout mutation and scrolling; pixel-fill
  chronology; VInt, DMA, and CRAM cadence; rendered composition; exact animation pacing; caller
  reachability; input, audio, and player-facing screen meaning.
- Remake status: implementation-neutral Phase 3 private-import contract; no renderer, asset format,
  resolution, animation system, replacement artwork, accessibility policy, or distribution license
  has been selected.
- Evidence date: 2026-08-08
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Contract Boundary

This contract defines the static identity and private-import boundary for three dedicated
special-screen asset rails:

1. nine Stack-compressed tile or font resources and their decoded-length/consumer-transfer
   boundaries;
2. twelve uncompressed palettes and layouts, including the two ASM-expanded title layouts;
3. the witch choice palette, ordered four-option/three-frame bubble table, pointer identities, word
   transformation, and source-static timer phase table;
4. a public H4 surface based on symbols, addresses, counts, relationships, and hashes rather than
   copyrighted graphics payloads.

It does not own the logo, title, witch, suspend, or ending screen engines; save-service or new-game
lifecycle; title/logo input and cheat handling; window allocation; palette-upload or transfer-service
semantics; fades; audio; pixel-fill execution; or final presentation. The adjacent
[graphics-service contract](graphics-service-state.md) owns only its bounded decompression and
display-service state, while the [window-system contract](window-system.md) owns shared window state.
Neither contract proves a rendered special-screen frame.

The executable owners are:

- `sf2-special-screen-graphics-decode-v1` in
  [`tests/fixtures/h2/special-screen-graphics-decode-v1.json`](../../../tests/fixtures/h2/special-screen-graphics-decode-v1.json);
- `sf2-special-screen-presentation-static-v1` in
  [`tests/fixtures/h2/special-screen-presentation-static-v1.json`](../../../tests/fixtures/h2/special-screen-presentation-static-v1.json);
- `sf2-witch-menu-graphics-static-v1` in
  [`tests/fixtures/h2/witch-menu-graphics-static-v1.json`](../../../tests/fixtures/h2/witch-menu-graphics-static-v1.json).

The research owners are [Special Screens](../../research/special-screens.md) and the relevant asset
corpora in [Technical Graphics and Decompression Services](../../research/technical-graphics.md).
The aggregate `sf2-special-screens-static-v1` fixture is deliberately not consumed.

## Pre-Contract Evidence Audit

All three dedicated owners were reproduced from current `main` on the evidence date:

```text
sf2-special-screen-graphics-decode-v1
SHA256 4FCD9DCA7ED4FA4D5D667B1E1A85FC0A6D1A786DA78D24699EC883062A55604C
Resources 9 / DecodedBytes 50176 / OversizedTransfers 5 / PASS

sf2-special-screen-presentation-static-v1
SHA256 56E0FBEC2B6F917AD8916B2ABC9226EE24A9732F25D408D7876A460651A57E84
Resources 12 / PaletteColors 240 / ParityBytes 8832 / PASS

sf2-witch-menu-graphics-static-v1
SHA256 12C8B6818CDE7A8F8A808893AB32711F9DC7DA7A9380DC3999C3991A97B2DE15
PaletteColors 16 / BubbleFrames 12 / ParityBytes 1000 / PASS
```

The audit checked the three fixtures, verifiers, owning research prose, generated private outputs,
and current research-index bindings. The three fixtures currently bind eleven index records. Eight
are unassociated and form this contract's exact future association set:

- `screens.endkiss.resources`;
- `screens.jewelend.resources`;
- `screens.suspend.resources`;
- `screens.title.resources`;
- `screens.title.compressed-tiles`;
- `screens.witch.resources`;
- `screens.witch.menu`;
- `screens.witchend.resources`.

The other three fixture-linked records already belong to accepted contracts and must remain
semantically unchanged: `tech.graphics.stack-decompression`, `tech.interfaces.ptr-s06`, and
`tech.services.resource-graphics`. Aggregate-only screen records, including
`screens.title.font`'s `LoadTitleScreenFont` function identity, remain outside this data contract.

The audit preserves these limits:

- the fixture field `transferPaddingByteCount = 27,648` is interpreted only as the arithmetic delta
  between five fixed transfer lengths and their decoder outputs. It does not prove padding bytes,
  zeros, initialized memory, stability, or invisibility;
- the witch table contains 480 ordered words and 240 distinct source-word values. Repeated equality
  between positions may be retained by a private logical model, but the count does not prove a
  source-level alias abstraction;
- source-static palette, priority, mirror, flip, timer, and destination facts do not prove exact
  hardware cadence or visible pixels;
- tracked fixtures retain metadata and hashes. Original compressed bytes, decoded tiles, palette
  words, layout words, menu words, screenshots, and rendered captures stay private/generated.

## Stack-Compressed Resource Corpus

**Confirmed static:** nine resources occupy 23,296 compressed bytes and decode to 50,176 bytes.
Seven resources enter the Stack decoder directly, while speech-balloon and Sega-logo tiles use the
accepted compressed-DMA wrapper. Six direct source pointers and all nine source ranges have accepted
H1/ROM parity.

| Resource symbol | Decoded bytes | Accepted consumer boundary | Fixed transfer bytes | Tail delta |
| --- | ---: | --- | ---: | ---: |
| `tiles_TitleScreen` | 8,192 | direct Stack, immediate DMA | 8,192 | 0 |
| `font_TitleScreen` | 4,096 | direct Stack, immediate DMA | 4,096 | 0 |
| `tiles_SuspendString` | 448 | direct Stack, queued DMA | 2,048 | 1,600 |
| `tiles_EndingKissPicture` | 6,144 | direct Stack, pixel-fill consumer | — | — |
| `tiles_EndingWitch` | 7,808 | direct Stack, immediate DMA | 16,384 | 8,576 |
| `tiles_EndingJewels` | 1,856 | direct Stack, immediate DMA | 16,384 | 14,528 |
| `tiles_Witch` | 13,568 | direct Stack, immediate DMA | 16,384 | 2,816 |
| `tiles_SpeechBalloon` | 1,920 | compressed immediate-DMA wrapper | 2,048 | 128 |
| `tiles_SegaLogo` | 6,144 | compressed immediate-DMA wrapper | 6,144 | 0 |

Exactly eight resources have fixed transfer lengths: three equal decoded output, while five are
larger. Those eight transfer lengths total 71,680 bytes. The five positive deltas total 27,648 bytes.
`tiles_EndingKissPicture` has a source-static pixel-fill consumer and no comparable fixed DMA length,
so it must not be forced into either the exact-transfer or oversized-transfer class.

A private importer MUST preserve every resource symbol, ROM address, compressed range, decoded byte
count, source/ROM parity result, consumer-boundary identity, and optional fixed transfer length. For
an oversized boundary it MUST preserve the decoded extent and the distinct tail extent; it MUST NOT
materialize the tail as zeros or copy decoder output beyond its accepted length without new runtime
evidence.

The fixture also closes aggregate Stack-stream structure: 964 command groups, 12,185 literal words,
3,161 copy commands producing 12,903 copied words, 34 through 52 trailing bits, maximum copy offset
2,008 words, and maximum copy length 33 words. These facts validate imported streams. They do not
make codec micro-implementation, malformed-stream recovery, or Genesis transfer services part of
this contract.

## Uncompressed Palette and Layout Corpus

**Confirmed static:** twelve resources occupy 8,832 bytes and match source, H1, and ROM. Seven
palettes contain 240 big-endian color words in 480 bytes; five layouts contain 4,176 big-endian words
in 8,352 bytes.

| Resource class | Count | Words | Bytes | Confirmed static boundary |
| --- | ---: | ---: | ---: | --- |
| palettes | 7 | 240 | 480 | addresses, sizes, hashes, 107 distinct values, 25 zero values |
| layouts | 5 | 4,176 | 8,352 | addresses, sizes, hashes, exact source/H1/ROM parity |
| complete uncompressed corpus | 12 | 4,416 | 8,832 | ten direct incbins plus two ASM-expanded layouts |

The five layout identities are `layout_TitleScreenA`, `layout_TitleScreenB`, `layout_Witch`,
`layout_EndingWitch`, and `layout_EndingJewels`. The two title layouts are assembled from `vdpTile`
source rather than treated as direct incbins. Their 1,792- and 768-byte expansions total 2,560 bytes
and match the two upstream binary mirrors. An importer MUST preserve this provenance distinction;
binary-mirror parity does not convert the ASM sources into incbin-owned resources.

A lossless private import retains the complete ordered palette and layout words because order and
full 16-bit values are part of data identity. The public contract retains symbols, addresses, sizes,
hashes, aggregate counters, and provenance only. Static word values do not establish upload order,
fade behavior, layout writes, scrolling, clipping, layer placement, or final composition.

## Witch Choice and Bubble Table

**Confirmed static:** the witch-menu presentation boundary contains two data resources and two
longword pointers:

- one 32-byte, 16-color choice palette with 15 distinct values and two zero entries;
- one 960-byte animation table containing four option groups, three frames per option, and forty
  words per frame;
- two ordered four-byte pointers, producing a complete 1,000-byte source/ROM parity boundary.

The twelve frames are ordered 5-by-8 word grids. Across all frame positions, the table contains 480
ordered words and 240 distinct source-word values. A private import MUST retain each position and
the equality/repetition relation between positions. It MUST NOT reduce the corpus to 240 values or
invent a source-level alias object merely from repeated equality.

`DrawWitchMenuBubble` applies `-$5D00` to each written word. In the accepted table, all 480 adjusted
words select palette 2 with priority; 240 carry the mirror flag, 240 carry the flip flag, and sixty
distinct tile indexes span 1,024 through 1,083. These are source-static word-transformation facts,
not claims about final hardware pixels.

The selected-option timer resets to 20 and maps states to frame indexes in this order:

| Timer states | Frame index |
| --- | ---: |
| 1..4 | 0 |
| 5..9 | 1 |
| 10..14 | 2 |
| 15..20 | 1 |

Unselected options use frame zero. The four option source offsets are `0`, `240`, `480`, and `720`;
their destination offsets are `392`, `4`, `36`, and `432`. This closes the static selector and write
table only. Menu redraw cadence, CRAM upload timing, window motion, controller input, perceived
pacing, and rendered output remain **Unknown**.

## Implementation-Neutral Import Model

The following is a logical data contract, not an engine-class prescription:

```text
SpecialScreenAssetCorpus {
  compressedResources[9] {
    resourceId
    sourceSymbol
    romAddress
    compressedByteCount
    decodedByteCount
    sourceRomParity
    privateCompressedBytes[]
    privateDecodedBytes[]
    consumerBoundary
    optionalFixedTransferByteCount
    optionalTailExtent {
      start: decodedByteCount
      byteCount: fixedTransferByteCount - decodedByteCount
      contents: unknown
    }
  }

  presentationResources[12] {
    resourceId
    sourceSymbol
    kind: palette | layout
    provenance: direct-incbin | asm-expanded
    romAddress
    wordCount
    byteCount
    payloadHash
    privateOrderedWords[]
    optionalBinaryMirrorParity
  }

  witchMenuPresentation {
    palette {
      sourceSymbol
      romAddress
      colorCount: 16
      payloadHash
      privateOrderedWords[16]
    }
    bubbleTable {
      sourceSymbol
      romAddress
      optionCount: 4
      framesPerOption: 3
      frameShape: 5x8
      privateOrderedWords[4][3][5][8]
      positionalEqualityRelation
      payloadHash
    }
    pointers[2] {
      pointerIdentity
      romAddress
      targetResourceId
    }
    selectedTimerPhases[4]
    unselectedFrameIndex: 0
    writeWordAdjustment: -0x5D00
  }
}
```

The public form omits every `private*` field and original payload. It preserves the same identities,
addresses, sizes, ordering metadata, equality/repetition metadata, provenance, hashes, transfer
extents, and phase-table facts so that a user-provided private corpus can be validated without
making copyrighted graphics a repository dependency.

## Cross-System Separation

The asset contract ends at explicit data and consumer-boundary identities. Keep the following in
their owning systems or future evidence slices:

- Stack decoder implementation, invalid-stream behavior, and global display initialization;
- immediate or queued DMA/CRAM service execution, VInt scheduling, and hardware cadence;
- logo checksum/input/cheat flow and title Start polling, scroll loops, fades, or return result;
- witch save-menu admission, save/load/copy/delete services, new-game lifecycle, dialogue, input, and
  window motion;
- suspend sleep/reset flow and ending-witch, ending-jewel, or ending-kiss renderer execution;
- palette upload order, layout mutation, scrolling, pixel-fill order, layer composition, audio, and
  visible timing;
- localization, accessibility, replacement art, resolution policy, and licensed distribution.

The aggregate [Special Screens](../../research/special-screens.md) owner may describe those adjacent
control-flow identities. This contract does not consume its aggregate fixture and does not turn those
identities into asset-import requirements.

## Fidelity, Modernization, and Copyright Boundary

Original-data compatibility requires deterministic preservation of the accepted resource symbols,
addresses, sizes, ordering, parity metadata, private payloads, decoded lengths, transfer extents,
title-layout provenance, witch pointer/table order, positional equality/repetition relation, word
transformation, and static phase table when importing a private original corpus.

A remake may deliberately choose new images, palettes, layouts, resolution, animation timing,
transitions, input flow, audio, responsive composition, and accessibility behavior. Those decisions
must be tracked separately from original data parity. An adapter may transcode the private original
assets into a modern format, provided it can reproduce the accepted metadata and hashes and reports
intentional deviations.

Original compressed streams, decoded tiles, fonts, palettes, layouts, bubble-frame words,
screenshots, and rendered captures are private/generated copyrighted inputs. Do not commit or
redistribute them. Public builds require newly authored or properly licensed replacement assets.

## H4 Acceptance Surface

A remake-side private importer or compatibility adapter can claim this contract only when automated
tests prove:

1. all nine compressed resource identities, source symbols, addresses, compressed ranges, decoded
   byte counts, consumer boundaries, aggregate stream counters, and source/H1/ROM parity match the
   accepted owner;
2. the eight fixed-transfer resources preserve their exact lengths and `3 exact + 5 oversized`
   classification, while the ending-kiss resource remains a pixel-fill consumer without an invented
   fixed DMA length;
3. each oversized resource preserves its decoded extent and tail extent, totaling 27,648 tail bytes,
   without asserting zero, padding, initialization, stability, or visible use;
4. all twelve presentation resources preserve identity, symbol, H1/ROM address, type, word/byte
   count, source/ROM hash and parity, and exact private word order;
5. both title layouts preserve ASM-expanded provenance, exact `1,792 + 768 = 2,560` byte shape, and
   binary-mirror parity without being relabeled as direct incbins;
6. the witch palette, two pointers, four option groups, twelve ordered 5-by-8 frames, 480 ordered
   words, 240 distinct source-word values, positional equality/repetition relation, word adjustment,
   adjusted metadata, offsets, and timer phase table match the accepted owner;
7. public fixtures and reports expose only metadata, hashes, and synthetic examples, never original
   compressed bytes, decoded tiles, palette/layout words, bubble-table words, or rendered captures;
8. rendering, transfer cadence, menu/control flow, localization, accessibility, and intentional
   presentation changes are tested and reported separately from static original-data parity.

H4 does not require a modern renderer to use Genesis tile or color formats at runtime. It requires
provenance-preserving import and an explicit deviation boundary when private original-compatible
inputs are used.

## Evidence Matrix

| Contract area | Evidence label | Executable owner | Remaining boundary |
| --- | --- | --- | --- |
| nine Stack resources, stream counters, decoded lengths, consumer classes, and source/H1/ROM parity | **Confirmed static** | `sf2-special-screen-graphics-decode-v1` ([`special-screen-graphics-decode-v1.json`](../../../tests/fixtures/h2/special-screen-graphics-decode-v1.json)) | Codec implementation, malformed input, transfer execution, and rendered pixels |
| eight fixed transfers, three exact and five oversized, plus the distinct ending-kiss pixel-fill boundary | **Confirmed static** | `sf2-special-screen-graphics-decode-v1` ([`special-screen-graphics-decode-v1.json`](../../../tests/fixtures/h2/special-screen-graphics-decode-v1.json)) | Tail contents, initialization, stability, and visible use remain **Unknown** |
| seven palettes, five layouts, 8,832 parity bytes, and title ASM/mirror relation | **Confirmed static** | `sf2-special-screen-presentation-static-v1` ([`special-screen-presentation-static-v1.json`](../../../tests/fixtures/h2/special-screen-presentation-static-v1.json)) | Upload order, fades, mutation, scrolling, composition, and presentation |
| witch palette, pointers, ordered 4x3x5x8 table, positional repetition, transformation, and timer phase table | **Confirmed static** | `sf2-witch-menu-graphics-static-v1` ([`witch-menu-graphics-static-v1.json`](../../../tests/fixtures/h2/witch-menu-graphics-static-v1.json)) | CRAM/DMA cadence, redraw, window motion, perceived timing, and pixels |
| logo/title/witch/suspend/ending control flow and save lifecycle | **Separate owner** | [Special Screens](../../research/special-screens.md) and accepted sibling contracts | Aggregate fixture is not consumed here |
| renderer architecture, accessibility, replacement art, localization, and distributable content | **Deliberate design** | Future product/content decisions | Requires provenance, licensing, and separate acceptance |

## Reproduction

```powershell
uv run sf2 h2 special-screen-graphics
uv run sf2 h2 special-screen-presentation
uv run sf2 h2 witch-menu-graphics
uv run sf2 design-contracts test
uv run sf2 verify
```

Generated detailed outputs remain under ignored `local/derived/special-screen-graphics-decode.json`,
`local/derived/special-screen-presentation-static.json`, and
`local/derived/witch-menu-graphics-static.json`.
