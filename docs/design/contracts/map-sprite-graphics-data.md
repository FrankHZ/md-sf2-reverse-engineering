# Map Sprite Graphics Data Contract

- **Confirmed original structure:** the ordered 720-slot regular map-sprite pointer table, 670 source
  payload identities, 50 pointer aliases, 669 valid Basic-compressed streams with fixed 576-byte
  decoded forms, the shared two-byte sentinel identity and its nine pointer slots, exact table/payload
  parity, and the bounded regular-versus-special consumer seam described below.
- **Inferred original behavior:** none promoted here. Source names identify map-sprite resources and
  consumer operations, but they do not prove player-visible direction, animation, or presentation.
- **Unknown original behavior:** nonstandard injection of IDs `237..250`, sentinel decoding and failure
  results, natural runtime reachability and load frequency, cache lifetime, facing or slot meaning,
  immersed effects, animation and palette selection, VRAM placement, VInt/DMA cadence, transfer
  completion, final rendering, malformed or replacement input policy, accessibility treatment, and
  player-facing meaning.
- Remake status: implementation-neutral Phase 3 private-import contract; no runtime texture format,
  renderer, cache, sprite animation model, replacement asset policy, or distribution license is
  selected.
- Evidence date: 2026-08-13
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Contract Boundary

This contract defines the static storage, decoding, alias, parity, and private-import boundary for
the regular original map-sprite graphics corpus:

1. 720 ordered pointer slots arranged as 240 logical IDs with three ordered source slots each;
2. 670 independently named source payload identities and the complete pointer-alias relation;
3. 669 valid Basic-compressed source/decoded pairs with fixed 576-byte decoded forms;
4. one distinct `0xFFFF` sentinel payload identity shared by nine ordered pointer slots;
5. bounded consumer-selection provenance without importing runtime presentation or transfer meaning;
6. public aggregate metadata/provenance separated from private original content.

The sole executable owner consumed here is fixture id `sf2-map-sprite-decode-v1` in
[`tests/fixtures/h2/map-sprite-decode-v1.json`](../../../tests/fixtures/h2/map-sprite-decode-v1.json).
Its source-backed owner is [Technical Graphics](../../research/technical-graphics.md). The bounded
source roots are `data/graphics/mapsprites/entries.asm` and the regular consumer seam in
`code/common/scripting/entity/entityscriptengine_2.asm`.

The exact future research-index association is only `auxiliary.data.pt-mapsprites`. The same fixture
also binds `tech.graphics.decompression`, which remains associated only with
[graphics-service-state](graphics-service-state.md). This contract does not add itself to that record
or turn a private resource corpus into a general Basic-decompression service contract.

## Pre-Contract Evidence Audit

The dedicated owner was reproduced from the accepted baseline:

```text
sf2-map-sprite-decode-v1
SHA256 48C09CA7F523DDEC289D0CD954BEA12E5A47824DE4A4BEC5697B8EEFF54FDB3E
Pointers 720 / DecodedPayloads 669 / DecodedBytes 385344 / PASS
```

The fixture directly binds exactly two research-index records:

- `tech.graphics.decompression` — already associated with `graphics-service-state`; unchanged;
- `auxiliary.data.pt-mapsprites` — unique, currently unassociated, and the sole future association
  for this contract.

The latter record also carries the broad `sf2-auxiliary-data-static-v1` inventory owner. This
contract does not consume that aggregate fixture, import any sibling record, or use the aggregate
inventory as authority for this graphics corpus.

The separate `sf2-map-sprite-assignments-static-v1` owner confirms that the complete accepted
original built input domains do not write IDs `237..250`. That result is linked through
[Common Scripting](../../research/common-scripting.md) as a separate-owner static boundary; the
assignment fixture is not executable evidence registered to this contract. Its result does not prove
universal runtime unreachability, sentinel safety, or behavior under raw-RAM, malformed, corrupt,
debug, or modified inputs.

No special-sprite, map-entity, ally/enemy definition, dialogue-property, map lifecycle, service,
interrupt, DMA, or presentation record gains this contract. All existing associations remain
unchanged.

The tracked fixture exposes aggregate counts, addresses, parity, bounded decoder diagnostics, and the
small sentinel structure. Raw compressed bytes, decoded art, per-resource hashes and sizes, the full
720-slot alias graph, and rendered captures remain private/generated.

## Ordered Pointer and Payload Topology

**Confirmed static:** `pt_Mapsprites` begins at ROM `0xC8000` (`819200`). The pointer table contains
720 ordered longword slots. Source construction groups those slots as 240 logical numeric IDs with
three ordered source slots per ID.

The term **source slot** is deliberate. This contract preserves positions `0`, `1`, and `2` as table
identities. It does not assign them player-visible left/right/up/down meaning, animation phase, facing
semantics, or a renderer coordinate system. Consumer source transforms a facing-state byte before
selecting a slot, but the visible interpretation and all caller state remain separate or **Unknown**.

The 720 pointer slots resolve to 670 independently defined source payload symbols. Fifty slots are
aliases: their slot-shaped expected symbol differs from the payload identity that the pointer actually
selects. A private importer MUST preserve both identities:

- the ordered pointer slot and its logical-ID/source-slot coordinates;
- the selected payload-owner symbol, path, and address.

It MUST NOT normalize the table into a 670-entry payload set, synthesize duplicate payloads to erase
aliases, or infer semantic equivalence merely because multiple slots share one owner.

All 720 pointer-table entries and all 670 source payload ranges match the accepted source, H1, and ROM
boundary. Pointer parity `720` and payload parity `670` remain independent results.

## Basic-Compressed Payload Corpus

**Confirmed static:** 669 of the 670 source identities are valid Basic-compressed streams. Together
they consume 225,542 compressed bytes, and every stream decodes to exactly 576 bytes (`0x240`). The
complete decoded corpus therefore totals 385,344 bytes.

| Surface | Accepted value |
| --- | ---: |
| ordered pointer slots | 720 |
| source payload identities | 670 |
| valid Basic streams | 669 |
| pointer aliases | 50 |
| aggregate source bytes including sentinel | 225,544 |
| aggregate valid compressed bytes | 225,542 |
| decoded bytes per valid stream | 576 |
| aggregate decoded bytes | 385,344 |
| pointer-table parity | 720 |
| payload parity | 670 |

The dedicated fixture also records aggregate decoder diagnostics:

- 6,946 command words;
- 87,031 literal words;
- 18,125 copy commands and 105,641 copied words;
- 1,500 repeat-last-word commands;
- maximum copy offset 273 words;
- maximum copy length 33 words.

These values verify the accepted private corpus. They do not require a remake to reproduce the
original Basic bit reader, command parser, history representation, repeat implementation, copy loop,
register allocation, or instruction order. An independent private decoder conforms when it produces
the accepted ordered decoded outputs and preserves provenance without publishing original content.

Private import retains every compressed stream, decoded result, source path/address, and private
hash. The public projection retains only dimensions, aggregate byte counts, diagnostics, parity, and
provenance. Neither projection promotes tile layout, palette, transparency, animation, or visible
sprite meaning from byte shape alone.

## Sentinel Identity and Nine Slots

**Confirmed static:** the one non-Basic payload identity is `Mapsprite237_0`. Its complete stored
structure is the two-byte word `0xFFFF`. Nine ordered pointer slots select this same sentinel:

```text
711, 712, 713, 714, 715, 716, 717, 718, 719
```

By the accepted three-slot table shape, those slots occupy all three source positions for logical IDs
`237`, `238`, and `239`. The sentinel is one private payload identity with nine incoming pointer
edges, not nine independent payloads and not a valid Basic-compressed stream.

A private importer MUST retain:

- the `Mapsprite237_0` symbol/path/address identity;
- the exact two-byte sentinel value;
- all nine ordered pointer slots and their shared-owner relation;
- the distinction between 669 valid Basic streams and this one sentinel identity.

The public contract may retain this small sentinel structure because it is already part of the
accepted tracked fixture. It MUST NOT expose any ordinary sprite payload, decoded art, per-resource
hash, or complete alias graph.

The sentinel does not establish dead content or universal runtime unreachability. The accepted built-
input owner excludes IDs `237..250` from its complete original built domains, but raw-RAM writes,
malformed scripts, corrupt state, debug paths, encoded values, and modified content remain outside
that proof. The effect of passing the sentinel to the Basic decoder remains **Unknown**.

## Bounded Consumer Selection Seam

**Confirmed static source shape:** `LoadBasicCompressedData` is bound at ROM `0x1A84` (`6788`). The
regular source consumers `ChangeEntityMapsprite` and `DmaEntityMapsprite` retain the following bounded
selection seam:

1. obtain a map-sprite byte and a source-facing-derived slot selector;
2. treat values below the symbolic special-sprite cutoff `240` as regular-table candidates;
3. compute one of three ordered pointer slots for the regular ID;
4. resolve the selected longword through `pt_Mapsprites`;
5. hand the selected private source to `LoadBasicCompressedData` and the loading-space destination;
6. in `DmaEntityMapsprite`, route values at or above `240` to the separate special-sprite loader;
7. retain source-visible `0x120`-word transfer operands on the regular DMA path.

This is source-shaped selection and handoff provenance, not a full runtime contract. It does not
claim that every numeric regular candidate is valid, that the sentinel decodes safely, that all
source-facing states occur naturally, or that DMA completes. It does not own immersed-effect behavior,
entity state, VRAM-slot calculation, cache capacity, animation, VInt scheduling, hardware timing, or a
visible frame.

The cutoff also does not make IDs `240..255` part of this regular corpus. Special-sprite assets and
their asymmetric pointer/dispatch routing remain with
[graphics-service-state](graphics-service-state.md) and the dedicated special-sprite owner.

## Implementation-Neutral Import Model

The minimum complete logical import keeps private slots, aliases, payloads, and decoded outputs
separate from the public metadata projection:

```text
MapSpriteGraphicsCorpus {
  privatePointerSlots[720]: PrivateMapSpritePointerSlot
  privatePayloads[670]: PrivateMapSpritePayload
  publicSummary: MapSpriteGraphicsPublicSummary
}

PrivateMapSpritePointerSlot {
  pointerSlotIndex
  logicalMapSpriteId
  sourceSlotIndex  // ordered 0..2 identity; no player-visible direction claim
  payloadOwnerRef
}

PrivateMapSpritePayload =
  | PrivateBasicMapSpritePayload
  | PrivateMapSpriteSentinel

PrivateBasicMapSpritePayload {
  payloadOwnerId
  sourceSymbol
  sourcePath
  sourceAddress
  privateCompressedBytes
  privateDecodedBytes[576]
  privateCompressedHash
  privateDecodedHash
}

PrivateMapSpriteSentinel {
  payloadOwnerId
  sourceSymbol = "Mapsprite237_0"
  sourcePath
  sourceAddress
  privateBytes = 0xFFFF
  incomingPointerSlots[9]
}

MapSpriteGraphicsPublicSummary {
  fixtureId = "sf2-map-sprite-decode-v1"
  pointerTableAddress = 819200
  basicDecoderEntryAddress = 6788
  pointerSlotCount = 720
  logicalIdCount = 240
  sourceSlotsPerId = 3
  payloadIdentityCount = 670
  validBasicStreamCount = 669
  sentinelIdentityCount = 1
  aliasPointerCount = 50
  sentinelPointerCount = 9
  sourceByteCount = 225544
  compressedByteCount = 225542
  decodedBytesPerBasicStream = 576
  decodedByteCount = 385344
  pointerTableParityCount = 720
  payloadParityCount = 670
  aggregateDecoderDiagnostics
  sentinelSymbol = "Mapsprite237_0"
  sentinelBytes = 0xFFFF
  sentinelPointerSlots[9]
  fixtureProvenance
}
```

This model is a private import/provenance boundary, not a required renderer API, texture atlas,
animation controller, cache, VRAM allocator, entity component, or asset-bundle layout. A remake may
transform private decoded data into another runtime form only when the accepted slot order, alias
graph, owner identities, sentinel distinction, decoded outputs, and transformation provenance remain
verifiable.

The public projection MUST NOT contain regular compressed payloads, decoded sprite art, per-resource
hashes or sizes, the complete 720-slot alias graph, rendered captures, or other original content.
Public reports may retain bounded metadata, aggregate counts, parity, addresses, decoder diagnostics,
the small sentinel structure, and provenance.

## Cross-System Separation

This contract does not own:

- Basic decompression service ABI or codec microimplementation, which remains associated with
  [graphics-service-state](graphics-service-state.md);
- special-sprite assets or routing for IDs `240..255`, which remain with the special-sprite owner and
  graphics-service contract;
- original built assignment domains, which remain with the map-sprite assignment owner described in
  [Common Scripting](../../research/common-scripting.md);
- initial entity-list records and their map-sprite values, which remain with
  [map-entity-data](map-entity-data.md);
- ally/enemy definition map-sprite tables, class derivation, NPC-tail reachability, or battle identity,
  which remain with [ally-definition-data](ally-definition-data.md) and
  [enemy-definition-data](enemy-definition-data.md);
- map-sprite-to-portrait/speech-SFX lookup, which remains with
  [sprite-dialogue-property-data](sprite-dialogue-property-data.md);
- entity lifecycle, movement, facing meaning, immersed effects, animation, map persistence, or cache;
- VInt scheduling, VRAM DMA, interrupt cadence, transfer completion, and hardware timing, which remain
  with [interrupt-dma-and-trap-state](interrupt-dma-and-trap-state.md);
- palettes, tile interpretation, texture layout, rendered frames, UI, dialogue, audio, or final
  presentation;
- `sf2-auxiliary-data-static-v1`, `sf2-map-sprite-assignments-static-v1`, `map.data.*`, or any sibling
  research-index association;
- private original payloads, decoded art, hashes, individual sizes, and complete aliases;
- malformed, injected, corrupt, debug, or replacement input policy;
- accessibility remapping, localization, story meaning, balance, or distribution policy.

## Judgment Boundary

### Confirmed

- fixture/source provenance through `sf2-map-sprite-decode-v1` and
  `auxiliary.data.pt-mapsprites`;
- exact `pt_Mapsprites` and `LoadBasicCompressedData` provenance identities/addresses;
- 720 ordered pointer slots arranged as 240 logical IDs with three ordered source slots each;
- 670 private payload identities and the complete 50-pointer alias relation;
- 669 valid Basic streams, 225,542 compressed bytes, fixed 576-byte decoded size, and 385,344 decoded
  bytes;
- one private `Mapsprite237_0` `0xFFFF` sentinel identity selected by slots `711..719`;
- complete 720-pointer and 670-payload parity;
- aggregate decoder diagnostics as corpus-verification metadata, not a required codec algorithm;
- source-shaped regular-table selection below cutoff 240 and separate special-sprite handoff at or
  above that cutoff;
- separate-owner static exclusion of IDs `237..250` from complete accepted original built input
  domains;
- public aggregate/sentinel metadata separated from private original content.

### Inferred

- none promoted by this contract.

### Unknown

- dynamic, encoded, malformed, corrupt, debug, raw-RAM, or modified-content injection of IDs
  `237..250`;
- Basic-decoder behavior and visible failure if the `0xFFFF` sentinel is supplied;
- natural reachability, load frequency, cache lifetime, reload behavior, and persistence;
- player-visible meaning of source slot positions and facing transforms;
- immersed effects, animation selection, palette, VRAM placement, VInt/DMA cadence, transfer
  completion, hardware timing, and rendered frames;
- malformed or replacement input admission, diagnostics, and fallback behavior;
- modern runtime format, renderer, cache, accessibility treatment, replacement assets, and
  distribution policy.

## H4 Acceptance Contract

A remake-facing H4 adapter passes this contract only when it can:

1. identify fixture `sf2-map-sprite-decode-v1`, the pinned baseline, and the accepted table/decoder
   provenance identities;
2. privately preserve all 720 ordered pointer slots as 240 logical IDs times three source slots,
   without assigning unsupported player-visible direction semantics;
3. privately preserve all 670 payload-owner identities and the complete 50-pointer alias relation;
4. reproduce 669 accepted Basic decoded outputs at exactly 576 bytes each and the aggregate 225,542
   compressed / 385,344 decoded byte totals from private inputs;
5. preserve the distinct `Mapsprite237_0` `0xFFFF` sentinel and all nine incoming slots without
   attempting to decode it as an ordinary Basic stream;
6. verify complete 720-pointer and 670-payload parity while keeping original assets, hashes, sizes,
   and aliases private;
7. detect pointer reorder, owner reassignment, alias flattening, payload truncation, decoded-size
   drift, sentinel replacement, sentinel decoding, and private-source loss through private or
   synthetic tests;
8. permit an independent decoder rather than requiring the original Basic command parser, copy loop,
   history representation, repeat implementation, register use, or instruction order;
9. preserve the bounded regular-versus-special selection seam without importing special-sprite assets,
   assignment domains, entity lifecycle, DMA completion, or visible rendering;
10. keep regular compressed bytes, decoded art, per-resource hashes/sizes, complete alias graph,
    screenshots, and other original content outside public fixtures and reports;
11. report injection, sentinel failure, runtime reachability, cache/persistence, animation, palette,
    VRAM/DMA, presentation, malformed input, replacement, and accessibility policy through separate
    owners or as **Unknown**.

H4 may decode and transform private map-sprite data during import, lazily, or ahead of runtime. Those
choices conform only when the accepted identity/slot/alias graph, sentinel distinction, decoded-output
evidence, and public non-disclosure boundary remain independently verifiable.

## Evidence Matrix

| Contract surface | Evidence label | Exact owner | Preserved boundary |
| --- | --- | --- | --- |
| pointer and payload corpus | **Confirmed static** | `sf2-map-sprite-decode-v1`; [fixture](../../../tests/fixtures/h2/map-sprite-decode-v1.json) | 720 private slots, 670 owners, 50 aliases, exact parity; assets/hashes remain private |
| valid Basic streams | **Confirmed static** | same fixture; [technical-graphics research](../../research/technical-graphics.md) | 669 fixed-size decoded forms and aggregate diagnostics; no required codec microimplementation |
| sentinel identity | **Confirmed static** | same fixture | one `0xFFFF` owner and slots 711..719; not a valid Basic stream or proof of runtime unreachability |
| built assignment exclusion | separate-owner **Confirmed static** | [common-scripting research](../../research/common-scripting.md) | complete built domains omit 237..250; malformed/debug/raw-RAM injection remains Unknown |
| decompression service | existing separate association | [graphics-service-state](graphics-service-state.md) | `tech.graphics.decompression` remains unchanged and gains no new design contract |
| special-sprite routing | separate-owner evidence | [graphics-service-state](graphics-service-state.md) | IDs 240..255 are outside this regular asset corpus |
| entity lifecycle, DMA, and rendering | separate owner / **Unknown** | [map exploration](map-exploration.md); [interrupt contract](interrupt-dma-and-trap-state.md) | source handoffs do not prove cache, timing, completion, or visible presentation |
| auxiliary aggregate | excluded executable owner | `sf2-auxiliary-data-static-v1` | broad inventory supplies no registration or sibling association here |

## Open Questions

1. Can a future bounded runtime rail inject IDs `237..239` without publishing assets and determine
   how the original Basic decoder/status path reports the shared sentinel?
2. Which original runtime paths cache, reload, replace, or transform regular map-sprite graphics after
   decoding, and how do source-slot selections map to visible animation?
3. What explicit validation and replacement policy should a remake importer use for out-of-range
   indices, truncated Basic inputs, sentinel access, alias changes, or modified assets?

## Reproduction

```powershell
uv run sf2 h2 map-sprites
uv run sf2 design-contracts test
uv run sf2 research-index test
```

Generated output remains under ignored `local/derived/map-sprite-decode.json`. Public acceptance uses
bounded metadata, aggregate diagnostics, and the small sentinel structure—not original payloads,
decoded sprite art, per-resource hashes/sizes, or the complete pointer-alias graph.
