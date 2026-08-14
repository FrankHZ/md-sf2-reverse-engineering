# Battle Sprite Graphics Data Contract

- Status: **Confirmed static resource identities, container partition, and decode shape**
- Evidence date: 2026-08-14
- Scope: original ally/enemy battle-sprite pointer and payload corpora as a private,
  engine-neutral import

## Judgment Boundary

This contract owns the static identity and private import shape of the original ally and enemy
battle-sprite graphics corpora. It does not own actor selection, animation-sequence selection,
loader control, decompression service behavior, transfer completion, or presentation.

- **Confirmed**: two ordered pointer tables with 32 ally and 54 enemy slots, each resolving in
  source order to the same number of source payload definitions; the fixed fields, relative palette
  boundary, relative frame words, palette records, and Stack streams within those definitions;
  exact aggregate byte/decode/parity counts; and bounded aggregate decoder diagnostics reproduced
  by the dedicated fixture.
- **Inferred, non-normative**: names such as `AllyBattlesprite`, `EnemyBattlesprite`, `frame`,
  `palette`, and `status offset` preserve source identities and ordered fields only. They do not
  establish visible actor identity, pose, animation timing, composition, placement, or authorial
  intent.
- **Unknown / separate owner**: natural selection and reachability of every slot and frame; invalid
  indices and malformed data; rendered tile, palette, and status-icon meaning; frame order and
  timing; placement, layering, mirroring, weapon composition, CRAM/VInt/DMA cadence or completion,
  caching, runtime modification, persistence, replacement policy, licensing, and presentation
  parity.

## Evidence Owner and Consumed Surface

The sole executable owner consumed by this contract is `sf2-battle-sprite-decode-v1`
([fixture](../../../tests/fixtures/h2/battle-sprite-decode-v1.json),
[verifier](../../../src/sf2tool/h2/battle_sprites.py),
[schema](../../../schemas/h2-battle-sprite-decode-fixture.schema.json), and
[manifest](../../../manifests/extractions/battle-sprite-decode.json)). Its prose owner is
[Technical Graphics and Decompression Services](../../research/technical-graphics.md).

This data contract consumes:

- `table.allyBattlespriteTableAddress` and `table.enemyBattlespriteTableAddress`;
- every aggregate field in `summary` and both ordered rows in `sideSummaries`; and
- the accepted upstream, ROM, and canonical-output provenance.

The `function` identities are retained only as external consumer/service witnesses. Ally/enemy
property selection, palette operations, frame lookup, Stack handoff, and fixed DMA requests remain
with [Battle Scene Command and Presentation Data](battle-scene-presentation.md).
`LoadStackCompressedData` remains with [Graphics Service State](graphics-service-state.md). None of
those function records gains this contract.

The complete generated `payloads[86]` rows remain under ignored
`local/derived/battle-sprite-decode.json`. Their source symbols, paths, addresses, exact relative
words, per-payload sizes, palette and decoded hashes, compressed-stream details, and raw or decoded
bytes are private verification inputs, not public contract payloads.

The aggregate `sf2-auxiliary-data-static-v1` fixture is explicitly excluded even though both target
research records also carry that evidence.

## Direct Binding and Association Boundary

The dedicated fixture directly binds exactly seven research-index records:

| Record ID | Fixture role | Contract treatment |
| --- | --- | --- |
| `auxiliary.data.pt-allybattlesprites` | `pt_AllyBattlesprites` at ROM address 1,572,892 | new association candidate |
| `auxiliary.data.pt-enemybattlesprites` | `pt_EnemyBattlesprites` at ROM address 1,245,188 | new association candidate |
| `battle.scene.load-enemy-sprite-properties` | enemy property/palette loader at 104,816 | unchanged; retained only by `battle-scene-presentation` |
| `battle.scene.load-enemy-sprite-frame` | enemy frame loader at 104,862 | unchanged; retained only by `battle-scene-presentation` |
| `battle.scene.load-ally-sprite-properties` | ally property/palette loader at 104,926 | unchanged; retained only by `battle-scene-presentation` |
| `battle.scene.load-ally-sprite-frame` | ally frame loader at 104,972 | unchanged; retained only by `battle-scene-presentation` |
| `tech.graphics.stack-decompression` | `LoadStackCompressedData` at 7,752 | unchanged; retained only by `graphics-service-state` |

No other `auxiliary.data.*`, `battle.scene.*`, `tech.graphics.*`, ally/enemy definition,
animation-sequence, weapon, ground, background, effect, portrait, map-sprite, interrupt, DMA, or
presentation record is associated by this contract.

## Ordered Tables and Payload Identities

The complete private import has two independent ordered domains:

| Side | Pointer slots | Source payload definitions | Table address |
| --- | ---: | ---: | ---: |
| ally | 32 | 32 | 1,572,892 |
| enemy | 54 | 54 | 1,245,188 |
| total | 86 | 86 | separate tables |

For each side, the fixture verifier closes a source-ordered one-to-one pointer/definition relation.
The 32 and 54 payload counts mean distinct source definitions and owner identities accepted by the
fixture. They are not claims that every stored byte sequence, compressed stream, palette, or decoded
hash is mutually distinct.

The complete identity rows remain private. Both pointer tables have exact original ROM parity for
all 86 slots, and every one of the 86 source payload definitions has exact original ROM parity.
These facts do not prove natural selection of every identity or make the numeric table order a
public remake API.

## Container Partition

Each private source payload begins with this ordered shape:

1. a two-byte animation-speed word;
2. one byte each for the two source-named status offsets;
3. a two-byte relative word at offset 4 that resolves the palette boundary;
4. one two-byte self-relative word per frame, beginning at offset 6;
5. one to four ordered 32-byte palettes; and
6. one ordered Stack-compressed stream for every frame.

The aggregate header size is field-exact:

```text
fixed six-byte prefixes       = 86 x 6  =    516
two-byte frame-relative words = 408 x 2 =    816
header bytes                              1,332
```

The 5,344 palette bytes are separate from the 1,332 header bytes. They MUST NOT be treated as a
subset of the header or counted twice. Complete stored-byte accounting closes as:

```text
header bytes       =   1,332
palette bytes      =   5,344
compressed bytes   = 492,594
payload bytes      = 499,270
```

The side-specific aggregate boundaries are:

| Side | Header bytes | Palettes/bytes | Frames | Compressed bytes | Payload bytes |
| --- | ---: | ---: | ---: | ---: | ---: |
| ally | 498 | 59 / 1,888 | 153 | 169,856 | 172,242 |
| enemy | 834 | 108 / 3,456 | 255 | 322,738 | 327,028 |

Ally definitions contain three to six frames and one to four palettes. Enemy definitions contain
two to seven frames and one to four palettes. The tracked animation-speed and status-offset minima
and maxima are observed source-field ranges only; they are not timing, coordinate, or visible-placement
domains.

## Decode Shape and Aggregate Diagnostics

Every ally frame stream decodes to exactly 4,608 bytes, giving 705,024 ally decoded bytes. Every
enemy frame stream decodes to exactly 6,144 bytes, giving 1,566,720 enemy decoded bytes. The complete
private decoded corpus is therefore 2,271,744 bytes.

The dedicated verifier also records these aggregate diagnostics:

| Diagnostic | Accepted value |
| --- | ---: |
| command groups | 15,889 |
| literal words | 204,635 |
| copy commands | 46,243 |
| copied words | 931,237 |
| maximum copy offset | 2,007 words |
| maximum copy length | 33 words |
| observed trailing span | 32..47 bits |

These values validate this corpus against the maintained decoder. They do not require a remake to
reproduce the original Stack microimplementation. The trailing span is only the stored span after
each logical terminator; it is not proven padding, zero-filled data, stability, or invisibility.

## Implementation-Neutral Logical Model

A complete private importer may use a model equivalent to:

```text
BattleSpriteGraphicsCorpus {
    provenance {
        fixtureId
        upstreamCommit
        romSha256Identity
        verifierOutputSha256
    }
    sides[2] {
        sideIdentity
        pointerTableIdentity
        pointerSlots[] {
            logicalSpriteId
            payloadOwnerId
        }
        privatePayloadDefinitions[] {
            logicalPayloadId
            privateSourceIdentity
            privateSourceAddress
            privateAnimationSpeedWord
            privateStatusOffsetBytes[2]
            privatePaletteBoundaryWord
            privateFrameRelativeWords[]
            privatePaletteWords[][16]
            orderedFrameStreams[] {
                privateCompressedBytes
                privateDecodedBytes
                privateHashesAndDecodeDiagnostics
            }
        }
    }
}
```

Per-definition source/H1/ROM addresses, big-endian relative-word storage, raw headers and palettes,
compressed bytes, decoded art, complete source paths, per-resource sizes/hashes, and other
non-public details are private import and round-trip evidence. The bounded table and external
witness symbols/addresses, aggregate provenance, and metadata named in the public projection remain
public. After verification, a conforming remake may use engine-native resource references, palettes,
textures, animation records, and storage. It is not required to reproduce Mega Drive address space,
big-endian pointer or relative-word storage, the Stack codec, original buffers, or original
file/container layout.

The importer MUST keep side identity, pointer-slot identity, payload-owner identity, header-field
identity, palette identity, and ordered frame-stream identity distinct. Equal-looking or
equal-decoding resources do not merge merely because this public contract omits their private hashes.

## Public and Private Projection

The public projection may retain only:

- fixture, upstream, ROM, and canonical-output provenance hashes;
- the two bounded table symbols and addresses;
- the four loader and one Stack-service external witness identities and addresses;
- the aggregate and side-specific pointer/payload, header/palette/frame, byte, decode, parity,
  source-field-range, and decoder-diagnostic counts; and
- the bounded header/palette/frame-stream partition.

It MUST NOT publish raw pointers, complete pointer-to-definition rows, payload symbols/source paths
or addresses, per-resource offsets/sizes/hashes, palette words, compressed bytes, decoded art, ROM
excerpts, screenshots, emulator captures, or rendered presentation.

## H4 Remake Acceptance Surface

A future H4 implementation conforms when it can show that:

1. its private import retains separate ordered 32-slot ally and 54-slot enemy pointer domains and
   their corresponding 32 and 54 source payload owners;
2. every private payload retains the exact fixed-field, relative palette boundary, ordered
   frame-relative-word, palette, and frame-stream identities;
3. all 153 ally streams deterministically reproduce their accepted 4,608-byte decoded identities,
   and all 255 enemy streams reproduce their accepted 6,144-byte decoded identities;
4. complete private accounting closes at `1,332 + 5,344 + 492,594 = 499,270`, with 2,271,744
   decoded bytes and without folding palette bytes into the header count;
5. engine-native resources can replace original pointers, relative words, Stack storage, and address
   layout without changing logical side, owner, palette, or frame identity;
6. aggregate codec diagnostics remain verification facts rather than runtime codec requirements;
   and
7. public reports expose only the bounded aggregate/provenance surface while copyrighted payloads
   and complete private identity material remain private.

H4 does not require original selector or animation-sequence rules, loader microimplementation,
palette-copy operations, staging or DMA operands, CRAM/VInt behavior, rendered output, or timing.
Those seams are tested by their owning presentation/service contracts.

## Cross-System Separation

- [Battle Scene Command and Presentation Data](battle-scene-presentation.md) consumes canonical
  records from this contract and retains actor selection, the four property/frame loader identities,
  animation-speed/status-offset consumption, palette selection and word-0-clear/copy-15 chronology,
  relative frame lookup, Stack handoff, fixed DMA requests, scene chronology, presentation seams,
  and their Unknowns. It no longer independently owns or re-verifies this static catalog.
- The distinct actor animation-sequence corpus and selector rules remain with
  `battle-scene-presentation` and `sf2-battle-sprite-animation-static-v1`.
- [Graphics Service State](graphics-service-state.md) retains `LoadStackCompressedData`, its ABI, and
  service boundaries. Aggregate codec diagnostics here do not transfer that ownership.
- ally/enemy definition contracts retain their sprite and palette selector fields; they do not own
  these graphics payloads.
- [Interrupt, DMA, and Trap State](interrupt-dma-and-trap-state.md) retains VInt, DMA, and CRAM
  service/timing boundaries.
- backgrounds, terrain, weapons, grounds, effects, portraits, special/map sprites, UI,
  localization, accessibility, licensing, replacement assets, and rendering remain with their own
  contracts or as Unknown.

## Evidence Matrix

| Contract area | Evidence label | Owner | Remaining boundary |
| --- | --- | --- | --- |
| two ordered tables, 32/54 source payload owners | **Confirmed static** | `sf2-battle-sprite-decode-v1` | Natural selection, byte uniqueness, visible identity |
| fixed fields, relative words, palettes, ordered frame streams | **Confirmed static/private import** | same fixture/verifier | Visual roles, timing, malformed inputs |
| aggregate bytes, decode shape, diagnostics, parity | **Confirmed static** | same fixture/verifier | Stack microimplementation, tail-bit meaning, rendered art |
| selector, loader, palette-operation, Stack handoff, DMA request | **Separate-owner Confirmed static witness** | `battle-scene-presentation` | Transfer completion, timing, visible presentation |
| animation-sequence corpus and selection rules | **Separate-owner Confirmed static** | `battle-scene-presentation` | Runtime reachability and visible animation |
| Stack service behavior | **Separate-owner Confirmed static** | `graphics-service-state` | Hardware/runtime timing |
| source-label visual intent | **Inferred, non-normative** | source vocabulary only | Actor/pose meaning, placement, authorial intent |
| reachability, presentation, persistence, replacement | **Unknown / separate owner** | future bounded evidence or product decision | Not an H4 data-fidelity requirement here |

## Reproduction

```powershell
uv run sf2 h2 battle-sprites
uv run sf2 design-contracts test
uv run sf2 verify
```

The complete payload rows remain under ignored `local/derived/battle-sprite-decode.json`. They are
reproducible private evidence, not tracked or distributable contract content.
