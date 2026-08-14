# Battle Background Graphics Data Contract

- Status: **Confirmed static resource identities, container partition, aliases, and decode shape**
- Evidence date: 2026-08-14
- Scope: original battle-background pointer and container corpus as a private, engine-neutral import

## Judgment Boundary

This contract owns the static data identity and private import shape of the original battle-background
corpus. It does not own scene selection, loader control, decompression service behavior, transfer
completion, or presentation.

- **Confirmed**: one 30-slot pointer table resolving to 27 ordered container identities; the exact
  three pointer aliases; the 38-byte pre-stream prefix and two ordered compressed-stream identities
  per container; exact aggregate byte/decode/parity counts; and bounded aggregate decoder
  diagnostics reproduced by the dedicated fixture.
- **Inferred, non-normative**: names such as `Background`, `tileset 1`, and `tileset 2` preserve source
  identities and order only. They do not establish visible layer roles, scene meaning, spatial
  arrangement, or authorial intent.
- **Unknown / separate owner**: natural selection and reachability of every slot; rendered tile and
  palette meaning; composition, coordinates, palette priority, fades, CRAM/VInt/DMA cadence or
  completion, timing, caching, runtime modification, persistence, malformed-input behavior,
  replacement policy, and presentation parity.

## Evidence Owner and Consumed Surface

The sole executable owner consumed by this contract is `sf2-battle-background-decode-v1`
([fixture](../../../tests/fixtures/h2/battle-background-decode-v1.json),
[verifier](../../../src/sf2tool/h2/battle_backgrounds.py),
[schema](../../../schemas/h2-battle-background-decode-fixture.schema.json), and
[manifest](../../../manifests/extractions/battle-background-decode.json)). Its prose owner is
[Technical Graphics and Decompression Services](../../research/technical-graphics.md).

This data contract consumes:

- `table.backgroundTableAddress`;
- every aggregate field in `summary`;
- the three tracked rows in `aliases`; and
- the accepted upstream, ROM, and canonical-output provenance.

The `function` identities are retained only as external consumer/service witnesses. The
`LoadBattlesceneBackground` selection/loader/palette seam remains with
[Battle Scene Command and Presentation Data](battle-scene-presentation.md), while
`LoadStackCompressedData` remains with [Graphics Service State](graphics-service-state.md). Neither
function record gains this contract.

The complete generated `payloads[27]` rows remain under ignored
`local/derived/battle-background-decode.json`. Their source paths, addresses, exact offsets, sizes,
palette hashes, compressed-stream details, and decoded hashes are private verification inputs, not
public contract payloads.

The aggregate `sf2-auxiliary-data-static-v1` fixture is explicitly excluded even though the target
research record also carries that evidence.

## Direct Binding and Association Boundary

The dedicated fixture directly binds exactly three research-index records:

| Record ID | Fixture role | Contract treatment |
| --- | --- | --- |
| `auxiliary.data.pt-backgrounds` | `pt_Backgrounds` at ROM address 1,056,480 | sole new association candidate |
| `battle.scene.load-background` | `LoadBattlesceneBackground` at ROM address 105,344 | unchanged; retained only by `battle-scene-presentation` |
| `tech.graphics.stack-decompression` | `LoadStackCompressedData` at ROM address 7,752 | unchanged; retained only by `graphics-service-state` |

No other `auxiliary.data.*`, `battle.scene.*`, `tech.graphics.*`, palette, sprite, weapon, ground,
special-screen, UI, map, interrupt, or presentation record is associated by this contract.

## Ordered Pointer and Container Identity

The complete table has 30 ordered pointer slots resolving to 27 ordered container definitions. Three
slots reuse earlier definitions:

| Background slot | Payload owner slot |
| ---: | ---: |
| 21 | 12 |
| 22 | 12 |
| 29 | 13 |

The complete identity rows remain private. The pointer table has exact original ROM parity for all
30 slots, and every one of the 27 source payloads has exact original ROM parity.

These are logical resource identities and aliases, not proof that every slot is naturally selected
or that the original numeric order is an appropriate remake-facing API.

## Container Prefix and Stream Partition

Each private container begins with a 38-byte pre-stream prefix:

1. a six-byte header containing three big-endian relative words;
2. a 32-byte source palette beginning at container offset 6; and
3. the first compressed stream beginning at container offset 38.

The verifier resolves the private offsets field-exactly:

```text
tileset1Offset = word@0 = 38
tileset2Offset = 2 + word@2
paletteOffset  = 4 + word@4 = 6
paletteEnd     = 38
```

`summary.headerByteCount=1026` is the aggregate of the complete 38-byte pre-stream prefixes across
27 containers: `27 × (6 relative-word bytes + 32 palette bytes)`. It is not 1,026 bytes of
relative-word header. `summary.paletteByteCount=864` is a subset of that prefix count and MUST NOT be
added to it again.

The complete byte accounting closes as:

```text
prefix bytes       = 1,026
compressed bytes   = 163,742
payload bytes      = 164,768
```

The two ordered streams in each container are source identities. This contract does not assign them
visible halves, planes, directions, animation phases, or layer priority.

## Decode Shape and Aggregate Diagnostics

The 27 containers hold 54 Stack-compressed streams. Each private stream decodes to exactly 6,144
bytes, so the complete private decoded corpus is 331,776 bytes.

The dedicated verifier also records these aggregate diagnostics:

| Diagnostic | Accepted value |
| --- | ---: |
| command groups | 7,002 |
| literal words | 93,129 |
| copy commands | 18,472 |
| copied words | 72,759 |
| maximum copy offset | 2,014 words |
| maximum copy length | 33 words |
| observed trailing span | 32..47 bits |

These values validate this corpus against the maintained decoder. They do not require a remake to
reproduce the original Stack microimplementation. The trailing span is only the stored span after
each logical terminator; it is not proven padding, zero-filled data, stability, or invisibility.

## Implementation-Neutral Logical Model

A complete private importer may use a model equivalent to:

```text
BattleBackgroundCorpus {
    provenance {
        fixtureId
        upstreamCommit
        romSha256Identity
        verifierOutputSha256
    }
    pointerSlots[30] {
        logicalBackgroundId
        payloadOwnerId
    }
    privatePayloads[27] {
        logicalPayloadId
        privateSourceIdentity
        privateSourceAddress
        privatePrefixBytes[38]
        privatePaletteWords[16]
        orderedStreams[2] {
            privateCompressedBytes
            privateDecodedBytes[6144]
            privateHashesAndDecodeDiagnostics
        }
    }
}
```

Per-container source/H1/ROM addresses, relative-word storage, raw prefix/palette bytes, compressed
bytes, decoded art, per-resource hashes, complete payload source paths, and other non-public details
are private import and round-trip evidence. The bounded table and external witness symbols/addresses,
aggregate provenance, and metadata named in the public projection remain public. After verification,
a conforming remake may use engine-native resource references, palettes, assets, and storage. It is
not required to reproduce Mega Drive address space, big-endian relative words, the Stack codec,
original staging buffers, or original file/container layout.

The importer MUST keep pointer-slot identity, payload-owner identity, the two ordered stream
identities, and source palette identity distinct. Aliased pointer slots do not become duplicate
payload owners.

## Public and Private Projection

The public projection may retain only:

- fixture, upstream, ROM, and canonical-output provenance hashes;
- the `pt_Backgrounds` symbol and table address;
- the bounded external `LoadBattlesceneBackground` and `LoadStackCompressedData` witness identities
  and addresses;
- the aggregate pointer/payload/alias, byte, decode, parity, and decoder-diagnostic counts;
- the three tracked alias metadata rows; and
- the bounded 38-byte prefix partition and two-stream-per-container rule.

It MUST NOT publish raw pointers, complete non-alias assignments, container symbols/source paths or
addresses, per-resource offsets/sizes/hashes, palette words, compressed bytes, decoded art, ROM
excerpts, emulator captures, or rendered presentation.

## H4 Remake Acceptance Surface

A future H4 implementation conforms when it can show that:

1. its private import retains 30 ordered logical slots and 27 ordered payload owners;
2. the exact `21→12`, `22→12`, and `29→13` alias relation is preserved without duplicating owners;
3. all 27 private containers retain the exact 38-byte prefix partition and two ordered stream
   identities;
4. all 54 private streams deterministically reproduce their accepted 6,144-byte decoded identities;
5. complete private byte accounting closes at `1,026 + 163,742 = 164,768` without double-counting
   the 864 palette bytes;
6. engine-native resources can replace original pointers, relative words, Stack storage, and address
   layout without changing logical identity or aliasing; and
7. public reports expose only the bounded aggregate/provenance surface while copyrighted payloads
   and complete private identity material remain private.

H4 does not require original selection rules, loader microimplementation, staging addresses,
palette-operation chronology, DMA/CRAM/VInt behavior, rendered output, or timing. Those seams are
tested by their owning presentation/service contracts.

## Cross-System Separation

- [Battle Scene Command and Presentation Data](battle-scene-presentation.md) consumes canonical
  records from this contract and retains background selection, `LoadBattlesceneBackground`, the two
  consecutive staging destinations, palette word-0 clear/copy-15 chronology, transfer/presentation
  boundaries, and their Unknowns. It no longer independently owns or re-verifies this catalog.
- [Graphics Service State](graphics-service-state.md) retains `LoadStackCompressedData`, its ABI, and
  service boundaries. Aggregate codec diagnostics here do not transfer that ownership.
- battle AI, action construction, combat resolution, and battle lifecycle contracts retain scene
  selection inputs, battle effects, and outcomes.
- [Interrupt, DMA, and Trap State](interrupt-dma-and-trap-state.md) retains VInt, DMA, and CRAM
  service/timing boundaries.
- actor battle sprites, weapons, grounds, effects, invocations, status/transition graphics,
  special-screen/UI/map graphics, localization, accessibility, and rendering remain with their own
  contracts or as Unknown.

## Evidence Matrix

| Contract area | Evidence label | Owner | Remaining boundary |
| --- | --- | --- | --- |
| 30 ordered slots, 27 payload owners, three aliases | **Confirmed static** | `sf2-battle-background-decode-v1` | Natural selection, scene meaning, rendered arrangement |
| 38-byte prefix and two ordered streams per container | **Confirmed static/private import** | same fixture/verifier | Visual roles, platform storage, malformed inputs |
| aggregate bytes, decode shape, diagnostics, parity | **Confirmed static** | same fixture/verifier | Stack microimplementation, tail-bit meaning, rendered art |
| loader/staging/palette chronology | **Separate-owner Confirmed static witness** | `battle-scene-presentation` | Transfer completion, timing, visible palette |
| Stack service behavior | **Separate-owner Confirmed static** | `graphics-service-state` | Hardware/runtime timing |
| source-label visual intent | **Inferred, non-normative** | source vocabulary only | Layer role, direction, authorial intent |
| reachability, presentation, persistence, replacement | **Unknown / separate owner** | future bounded evidence or product decision | Not an H4 data-fidelity requirement here |

## Reproduction

```powershell
uv run sf2 h2 battle-backgrounds
uv run sf2 design-contracts test
uv run sf2 verify
```

The complete payload rows remain under ignored `local/derived/battle-background-decode.json`. They
are reproducible private evidence, not tracked or distributable contract content.
