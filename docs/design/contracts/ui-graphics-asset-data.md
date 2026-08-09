# UI Graphics Asset Data Contract

- **Confirmed original structure:** the complete shared base/menu/prompt graphics corpus, its
  heterogeneous nine-entry menu table, the complete contiguous assembled icon block, source-only
  payload exceptions, physical storage roles, and bounded copy/highlight operations described below.
- **Inferred original behavior:** none promoted here.
- **Unknown original behavior:** runtime menu admission and selection, dynamic or nonstandard icon
  reachability, invalid indexes, palette selection, VInt/DMA cadence, frame timing, rendered
  composition, caller-visible results, input, audio, localization, accessibility, and player-facing
  menu meaning.
- Remake status: implementation-neutral Phase 3 private-import contract; no renderer, asset format,
  resolution, animation system, widget toolkit, replacement artwork, or distribution license has
  been selected.
- Evidence date: 2026-08-09
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Contract Boundary

This contract defines the static identity and private-import boundary for two dedicated UI graphics
rails:

1. the nine shared base, diamond-menu, yes/no, and main-menu resources, their pointer identities,
   and the ordered heterogeneous menu table;
2. the 167 source-available icon payload identities, exact 163-payload vanilla assembly, physical
   storage roles, highlight mask, and bounded copy/highlight transformations.

It additionally defines a public H4 surface based on symbols, addresses, counts, route metadata,
storage roles, hashes, and synthetic examples rather than copyrighted graphics payloads.

It does not own layout grids, window allocation, menu state machines, service admission, controller
input, runtime icon choice, palette upload, transfer scheduling, visible animation, localization, or
final presentation. The adjacent [UI layout data contract](ui-layout-data.md) owns the accepted
layout/pointer/border/direct-layout-asset corpus, while the
[window-system contract](window-system.md) owns shared runtime window state. The
[graphics-service contract](graphics-service-state.md) owns only its bounded decompression/display
service surface. None of those contracts proves a rendered menu frame.

The executable owners are:

- `sf2-ui-graphics-decode-v1` in
  [`tests/fixtures/h2/ui-graphics-decode-v1.json`](../../../tests/fixtures/h2/ui-graphics-decode-v1.json);
- `sf2-icon-graphics-static-v1` in
  [`tests/fixtures/h2/icon-graphics-static-v1.json`](../../../tests/fixtures/h2/icon-graphics-static-v1.json).

The research owners are the graphics sections of
[Common Menu Engines and Services](../../research/common-menus.md) and
[Technical Graphics and Decompression Services](../../research/technical-graphics.md).

## Pre-Contract Evidence Audit

Both dedicated owners were reproduced from current `main` on the evidence date:

```text
sf2-ui-graphics-decode-v1
SHA256 58C9089F1DD43BE8F0EA049F8CEE4102019231336A3A6B5F7729F1D3B70B52FC
Resources 9 / DecodedBytes 23168 / MenuEntries 9 / PASS

sf2-icon-graphics-static-v1
SHA256 CF3C4E928698EEC45A1F1F21D12EBE63BEC418CF0F6F40E7820AE0B8FFE5AE1F
AvailablePayloads 167 / AssembledIcons 163 / UnassembledPayloads 4 / PASS
```

The audit checked the fixtures, verifiers, research prose, generated private outputs, and current
research-index bindings. The two fixtures currently bind eight index records. Six are unassociated
and form this contract's exact future association set:

- `menus.diamond`;
- `menus.tile-pointers`;
- `tech.interfaces.ptr-s16`;
- `tech.services.resource-icon`;
- `gameflow.start.base-tiles`;
- `auxiliary.data.itemicon000`.

The other two fixture-linked records already belong to accepted contracts and must remain
semantically unchanged:

- `tech.graphics.stack-decompression` remains associated with
  [graphics-service-state](graphics-service-state.md);
- `tech.services.resource-graphics` remains associated with
  [text-and-font-system](text-and-font-system.md).

The candidate records also carry aggregate evidence from common menus, technical interfaces,
technical services, gameflow, or auxiliary data. This contract deliberately does not consume
`sf2-common-menus-static-v1`, `sf2-tech-interfaces-static-v1`,
`sf2-tech-services-static-v1`, `sf2-gameflow-core-static-v1`, or
`sf2-auxiliary-data-static-v1`. The dedicated fixtures own every claim promoted here.

The audit preserves these limits:

- the nine resources and nine pointer identities are not the same domain as the nine menu-table
  entries. The table contains three packed main-menu rows and six compressed-menu pointer rows; it
  does not route the base or yes/no resources;
- main-menu icon index 6 has no reference in the complete accepted menu table. This is static
  absence, not proof of universal runtime unreachability;
- the source tree contains 167 available payloads, while the vanilla assembly contains 163. Source
  availability and assembled physical storage must remain separate;
- storage slot 129 retains source identity `OtherIcon002`; only its accepted enum name is null. It
  must not be normalized to an anonymous or unnamed resource;
- storage slots 146 through 148 carry `OtherIcon003` through `OtherIcon005` and coincide with the
  arithmetic spell-slot positions for spell indexes 16 through 18. The corresponding three spell
  payloads are source-only exceptions, not a second set of assembled bytes in those slots;
- tracked fixtures contain metadata and hashes. Compressed streams, decoded tiles, icon bytes,
  highlight-mask bytes, screenshots, and rendered captures stay private/generated.

## Shared UI Resource Corpus

**Confirmed static:** the corpus contains nine resources:

| Resource class | Count | Storage form | Confirmed static boundary |
| --- | ---: | --- | --- |
| base tiles | 1 | Stack-compressed | source/ROM resource and pointer parity |
| diamond-menu tiles | 6 | Stack-compressed | two frames of four 288-byte icon transfers per stream |
| yes/no tiles | 1 | Stack-compressed | two frames of two icons; 1,152 decoded bytes |
| main-menu tiles | 1 | uncompressed | seven ordered 576-byte / 18-tile icon records |

The eight compressed streams occupy 7,848 bytes and decode to 23,168 bytes. The uncompressed
`tiles_MainMenu` payload occupies 4,032 bytes. Nine resource identities, nine pointer identities,
and the separate nine-entry menu table match the accepted source/H1/ROM boundary. Their combined
accepted source/ROM parity accounting is 11,952 bytes:

```text
compressed resources  7848
main-menu payload      4032
nine pointer words       36
nine menu-table rows     36
total                 11952
```

These terms are an accounting partition, not a runtime transfer sequence. The base resource can be
consumed by more than one source path, but this contract does not assign startup, ending-credit,
palette, or timing semantics to those uses.

A private importer MUST preserve every resource symbol, source and pointer address, compressed or
uncompressed storage kind, source/ROM range parity, decoded length where applicable, pointer
identity, and whole-resource metadata hash. It MUST preserve the main-menu payload as seven ordered
records rather than one anonymous 4,032-byte blob.

The Stack fixture also closes aggregate stream structure: 315 command groups, 3,111 literal words,
1,869 copy commands producing 8,473 copied words, 32 through 46 trailing bits, maximum copy offset
1,904 words, and maximum copy length 33 words. These validate private imports; they do not make
decoder micro-implementation or malformed-stream recovery part of this contract.

## Heterogeneous Menu-Table Contract

**Confirmed static:** `pt_tiles_Menu` has nine ordered longword entries in two formats. The first
three have bit 31 set and pack four main-menu icon indexes each:

| Table row | Ordered packed icon indexes |
| ---: | --- |
| 0 | `[5, 1, 2, 4]` |
| 1 | `[0, 1, 2, 3]` |
| 2 | `[0, 1, 2, 4]` |

Rows 3 through 8 instead contain the ordered pointer identities
`p_tiles_ItemMenu`, `p_tiles_BattlefieldMenu`, `p_tiles_ChurchMenu`,
`p_tiles_ShopMenu`, `p_tiles_CaravanMenu`, and `p_tiles_DepotMenu`.

An importer MUST retain all nine route positions, the format tag of each row, the four ordered
indexes inside each packed row, and the exact pointer identity inside each compressed row. It MUST
NOT normalize the table to a set of referenced resources or reinterpret packed values as pointers.

The three packed rows reference main-menu icon indexes 0 through 5. Index 6 has no table reference.
This proves only the accepted table relationship. Direct calls, modified tables, debug state,
malformed inputs, and player-visible selection remain **Unknown**.

## Main-Menu Record Boundary

**Confirmed static:** `tiles_MainMenu` contains seven contiguous 576-byte records, each holding
eighteen 32-byte tiles. The fixture retains the resource symbol, `p_tiles_MainMenu`, source path,
definition and pointer owner paths, source/pointer addresses, whole-resource and pointer hashes, and
per-record addresses and hashes.

A private importer MUST retain indexes 0 through 6 independently even though index 6 has no accepted
table reference. It MUST NOT remove, merge, or renumber index 6 as an optimization. Public fixtures
may retain record addresses, sizes, hashes, and reference counts, but not the original tile bytes.

This record shape does not establish animation order, palette, icon meaning, menu admission, or
visible timing. Those remain with callers or future presentation evidence.

## Contiguous Icon Storage

**Confirmed static:** the source tree exposes 167 fixed 192-byte payloads, totaling 32,064 bytes.
The vanilla assembly contains exactly 163 payloads, totaling 31,296 contiguous bytes:

| Source/assembly class | Count | Vanilla assembly boundary |
| --- | ---: | --- |
| item payloads | 127 | assembled |
| spell payloads | 30 | assembled |
| other payloads | 6 | assembled |
| explicit source-only payloads | 4 | not assembled |

Every assembled payload contains six tiles and matches the accepted ROM range. The physical address
formula is `p_Icons + storageIndex * 192`; there is no per-icon pointer table. A canonical importer
MUST retain all 167 exact source paths, the 192-byte file size, and vanilla assembly membership. For
the 163 assembled rows it additionally MUST retain the accepted source symbol, physical storage
index, ROM address, payload hash, and private bytes. The four source-only exceptions carry no
contract claim for a source symbol, storage index, ROM address, ROM parity, or payload hash.

The four source-only exceptions are:

- `data/graphics/icons/item/icon127.bin`;
- `data/graphics/icons/spell/icon016.bin`;
- `data/graphics/icons/spell/icon017.bin`;
- `data/graphics/icons/spell/icon018.bin`.

They receive no borrowed vanilla address or ROM-parity claim. Their exclusion from the assembled
corpus does not prove they are dead, universally unreachable, or irrelevant to every alternate
build.

## Physical Storage Roles and Collisions

**Confirmed static:** the six assembled `OtherIcon` resources retain these physical roles:

| Storage index | Source symbol | Accepted enum identity | Spell-index collision |
| ---: | --- | --- | ---: |
| 127 | `OtherIcon000` | `ICON_NOTHING` | — |
| 128 | `OtherIcon001` | `ICON_UNARMED` | — |
| 129 | `OtherIcon002` | no accepted enum name | — |
| 146 | `OtherIcon003` | `ICON_JEWEL_OF_LIGHT` | 16 |
| 147 | `OtherIcon004` | `ICON_JEWEL_OF_EVIL` | 17 |
| 148 | `OtherIcon005` | `ICON_CRACKS_OVERLAY` | 18 |

Slot 129 is not unnamed data: its source symbol is `OtherIcon002`. Only `enumName` is null in the
accepted fixture. Likewise, the three collision rows retain both the assembled other-icon identity
and the colliding arithmetic spell index. The source-only spell payloads 16 through 18 do not create
co-resident second payloads.

A remake may use typed resource references internally, but its original-format adapter MUST preserve
each physical slot, source symbol, optional enum identity, and optional spell-index collision. It
MUST NOT deduplicate slots 146 through 148 into spell resources or fabricate an enum name for
`OtherIcon002`.

## Copy and Highlight Operations

**Confirmed static:** the dedicated owner closes only these consumer-local data transformations:

- direct icon copy produces 192 bytes, or six tiles;
- four corner-clean word operations apply `0xF000` at byte offsets 0 and 156, and `0x000F` at
  byte offsets 34 and 190;
- the highlight path produces two 192-byte frames, totaling 384 bytes;
- its tracked operation identity is source-bitwise-AND-mask;
- the 192-byte highlight mask and the accepted functions/table identities match ROM.

These facts define reproducible byte transformations for a privately imported payload. They do not
define which caller selects an icon, whether an invalid index is admitted, palette choice, DMA
order, frame alternation, highlight timing, or rendered corner pixels.

## Implementation-Neutral Import Model

The following is a logical data contract, not an engine-class prescription:

```text
UIGraphicsAssetCorpus {
  sharedResourceIdentities[9] {
    resourceId
    sourceSymbol
    storageKind: stack-compressed | uncompressed-records
    sourceAddress
    pointerIdentity
    pointerAddress
    payloadRef
    sourceRomParity
  }

  stackCompressedPayloads[8] {
    resourceRef
    compressedByteCount
    decodedByteCount
    privateCompressedBytes[]
    privateDecodedBytes[]
    payloadHash
  }

  uncompressedMainMenuPayload {
    resourceRef
    storedByteCount: 4032
    recordCount: 7
    recordByteCount: 576
    privateStoredBytes[4032]
    payloadHash
  }

  menuRoutes[9] {
    routeIndex
    routeKind: packed-main-menu-indexes | compressed-pointer
    orderedPackedIndexes[4]
    pointerIdentity
  }

  mainMenuRecords[7] {
    recordIndex
    address
    byteCount: 576
    tileCount: 18
    payloadHash
    privateTileBytes[]
    tableReferenceCount
  }

  iconSources[167] {
    sourcePath
    byteCount: 192
    assembledInVanilla
    optionalAssembledSlotRef
  }

  assembledIconSlots[163] {
    storageIndex
    sourcePath
    sourceSymbol
    optionalEnumName
    optionalSpellIndexCollision
    romAddress
    payloadHash
    privatePayloadBytes[192]
  }

  iconTransform {
    directCopyByteCount: 192
    cornerCleanOperations[4]
    highlightFrameCount: 2
    highlightOutputByteCount: 384
    highlightOperation: source-bitwise-and-mask
    privateHighlightMaskBytes[192]
    highlightMaskHash
  }
}
```

For storage slot 129, `sourceSymbol` is `OtherIcon002` and `optionalEnumName` is absent. The
model never uses absence of an enum name as absence of a source identity.

The public form omits every `private*` field and original payload. It retains each evidence-bounded
symbol, path, address, storage membership, dimension, count, route kind/order, packed index row,
physical role, collision, operation, and hash so a user-provided private corpus can be validated
without making copyrighted graphics a repository dependency. It does not invent a symbol, storage
index, address, ROM parity result, or hash for any of the four source-only exceptions.

## Cross-System Separation

Keep the following outside this contract:

- Stack decompressor implementation, invalid-stream behavior, and global display initialization;
- UI layouts, borders, pointer-to-layout routes, and window allocation/movement/composition;
- diamond-menu, yes/no, shop, church, Caravan, depot, battlefield, or main-menu input/state flow;
- caller admission, enum-to-icon selection beyond the accepted storage-role facts, invalid indexes,
  dynamic/debug reachability, and return behavior;
- palette selection, VInt, DMA, clipping, layer composition, frame pacing, audio, and pixels;
- localization, accessibility, replacement artwork, resolution policy, and licensed distribution.

The aggregate research owners may describe adjacent callers and services. This contract does not
consume their aggregate fixtures and does not turn those control-flow facts into asset-import
requirements.

## Fidelity, Modernization, and Copyright Boundary

Original-data compatibility requires preserving resource/pointer identities, compressed and
uncompressed storage shapes, decoded lengths, menu-table formats and routes, all seven main-menu
records, source/assembly membership for all 167 icon payload identities, exact 163-slot vanilla
storage, physical roles/collisions, and copy/highlight transformation metadata when importing a
private original corpus.

A remake may deliberately choose new art, palettes, resolution, layout, animation, input, responsive
composition, accessibility, and localization. Those choices must be tracked separately from original
data parity. A private import adapter may transcode accepted assets into a modern format if it can
reproduce the accepted metadata and hashes and reports intentional deviations.

Original compressed streams, decoded tiles, main-menu/icon/highlight bytes, screenshots, and
rendered captures are private/generated copyrighted inputs. Do not commit or redistribute them.
Public builds require newly authored or properly licensed replacement assets.

## H4 Acceptance Surface

A remake-side private importer or compatibility adapter can claim this contract only when automated
tests prove:

1. all nine shared resource and pointer identities, addresses, storage forms, and source/H1/ROM
   parity match the dedicated owner; exactly eight Stack resources preserve their compressed and
   decoded counts plus accepted aggregate stream counters, while the uncompressed main-menu resource
   preserves its 4,032-byte / seven-record shape;
2. the nine menu-table routes preserve exact order, three packed versus six pointer row kinds, all
   packed index positions, and all six pointer identities;
3. all seven main-menu records preserve index, address, 576-byte/18-tile shape, private byte order,
   and hash, including table-unreferenced index 6;
4. all 167 source payload identities preserve exact source path, 192-byte size, and vanilla assembly
   membership, with exactly 163 assembled and exactly four explicitly listed source-only exception
   paths; no symbol, storage index, address, ROM parity, or hash is required for those four
   exceptions;
5. all 163 physical slots preserve their source paths and symbols, 192-byte/six-tile shape, storage
   index, address, payload hash, private byte order, and exact 127-item/30-spell/6-other partition;
6. slots 127, 128, 129, and 146 through 148 preserve their source symbols, optional enum names, and
   optional spell-index collisions, including `OtherIcon002` at slot 129 with no accepted enum name;
7. direct copy, four corner-clean operations, two-frame/384-byte highlight output, highlight-mask
   identity/hash, and source-bitwise-AND-mask operation match the accepted owner;
8. public fixtures and reports expose only metadata, hashes, and synthetic examples, never original
   compressed streams, decoded tiles, icon bytes, highlight masks, screenshots, or rendered captures;
9. caller behavior, invalid inputs, rendering, localization, accessibility, and intentional
   presentation changes are tested and reported separately from static original-data parity.

H4 does not require a modern renderer to use Genesis tile formats or the original physical slot
layout at runtime. It requires provenance-preserving import and explicit deviation reporting when a
private original-compatible corpus is used.

## Evidence Matrix

| Contract area | Evidence label | Executable owner | Remaining boundary |
| --- | --- | --- | --- |
| nine shared resources/pointers, eight Stack streams, one main-menu payload, and parity accounting | **Confirmed static** | `sf2-ui-graphics-decode-v1` ([`ui-graphics-decode-v1.json`](../../../tests/fixtures/h2/ui-graphics-decode-v1.json)) | Decoder implementation, transfer execution, palette, timing, and pixels |
| ordered three-packed/six-pointer menu table and seven main-menu records | **Confirmed static** | `sf2-ui-graphics-decode-v1` ([`ui-graphics-decode-v1.json`](../../../tests/fixtures/h2/ui-graphics-decode-v1.json)) | Index-6 dynamic reachability, caller selection, animation, and player-facing meaning |
| 167 source payloads, exact 163-payload vanilla assembly, and four source-only exceptions | **Confirmed static** | `sf2-icon-graphics-static-v1` ([`icon-graphics-static-v1.json`](../../../tests/fixtures/h2/icon-graphics-static-v1.json)) | Alternate-build use and nonstandard reachability remain **Unknown** |
| physical icon roles/collisions and bounded copy/corner/highlight operations | **Confirmed static** | `sf2-icon-graphics-static-v1` ([`icon-graphics-static-v1.json`](../../../tests/fixtures/h2/icon-graphics-static-v1.json)) | Caller admission, invalid indexes, DMA/frame cadence, palette, and rendered output |
| layout grids, runtime windows, menus, services, and input | **Separate owner** | [UI layout](ui-layout-data.md), [window system](window-system.md), and accepted menu/service/input contracts | End-to-end visible UI remains unclosed |
| renderer architecture, accessibility, localization, replacement art, and distributable content | **Deliberate design** | Future product/content decisions | Requires provenance, licensing, and separate acceptance |

## Reproduction

```powershell
uv run sf2 h2 ui-graphics
uv run sf2 h2 icon-graphics
uv run sf2 design-contracts test
uv run sf2 verify
```

Generated detailed outputs remain under ignored `local/derived/ui-graphics-decode.json` and
`local/derived/icon-graphics-static.json`.
