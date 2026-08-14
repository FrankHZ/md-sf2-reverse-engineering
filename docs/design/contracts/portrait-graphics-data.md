# Portrait Graphics Data Contract

- Status: **Confirmed static resource identities, container partition, aliases, and decode shape**
- Evidence date: 2026-08-14
- Scope: original portrait pointer and payload corpus as a private, engine-neutral import

## Judgment Boundary

This contract owns the static identity and private import shape of the original portrait graphics
corpus. It does not own portrait selection, loader control, window state, decompression service
behavior, transfer completion, or presentation.

- **Confirmed**: one 56-slot pointer table resolving to 52 source payload definitions; the exact four
  pointer aliases; counted four-byte eye and mouth entry sequences, one 32-byte palette, and one
  Stack stream per definition; exact aggregate byte/decode/parity counts; and bounded aggregate
  decoder diagnostics reproduced by the dedicated fixture.
- **Inferred, non-normative**: names such as `Portrait`, `eye`, and `mouth` preserve source
  identities and ordered fields only. They do not establish visible facial meaning, animation
  timing, expression semantics, scene use, or authorial intent.
- **Unknown / separate owner**: natural selection and reachability of every slot; invalid indices;
  malformed-data behavior; rendered tile and palette meaning; animation, composition, mirroring,
  CRAM/VInt/DMA cadence or completion, timing, caching, runtime modification, persistence,
  localization, accessibility, licensing, replacement policy, and presentation parity.

## Evidence Owner and Consumed Surface

The sole executable owner consumed by this contract is `sf2-portrait-graphics-decode-v1`
([fixture](../../../tests/fixtures/h2/portrait-graphics-decode-v1.json),
[verifier](../../../src/sf2tool/h2/portraits.py),
[schema](../../../schemas/h2-portrait-graphics-decode-fixture.schema.json), and
[manifest](../../../manifests/extractions/portrait-graphics-decode.json)). Its prose owner is
[Technical Graphics and Decompression Services](../../research/technical-graphics.md).

This data contract consumes:

- `table.portraitTableAddress`;
- every aggregate field in `summary`;
- the four tracked rows in `aliases`; and
- the accepted upstream, ROM, and canonical-output provenance.

The `function` identities are retained only as external consumer/service witnesses. `LoadPortrait`
selection, parsing, palette-copy, Stack-call, VRAM/CRAM handoff, and window-state seams remain with
[Portrait Window and State](portrait-window-state.md). `LoadStackCompressedData` remains with
[Graphics Service State](graphics-service-state.md). Neither function record gains this contract.

The complete generated `payloads[52]` rows remain under ignored
`local/derived/portrait-graphics-decode.json`. Their source paths, addresses, exact header entries,
per-payload sizes, metadata/palette hashes, compressed-stream details, and decoded hashes are
private verification inputs, not public contract payloads.

The aggregate `sf2-auxiliary-data-static-v1` fixture is explicitly excluded even though the target
research record also carries that evidence.

## Direct Binding and Association Boundary

The dedicated fixture directly binds exactly three research-index records:

| Record ID | Fixture role | Contract treatment |
| --- | --- | --- |
| `auxiliary.data.pt-portraits` | `pt_Portraits` at ROM address 1,867,780 | sole new association candidate |
| `menus.load-portrait` | `LoadPortrait` at ROM address 87,594 | unchanged; retained only by `portrait-window-state` |
| `tech.graphics.stack-decompression` | `LoadStackCompressedData` at ROM address 7,752 | unchanged; retained only by `graphics-service-state` |

No other `auxiliary.data.*`, `menus.*`, `tech.graphics.*`, sprite-dialogue, UI-layout, map-script,
battle-scene, interrupt, DMA, palette, window, dialogue, or presentation record is associated by this
contract.

## Ordered Pointer and Payload Identity

The complete table has 56 ordered pointer slots resolving to 52 ordered source payload definitions.
Here, 52 means distinct source definitions and owner identities accepted by the fixture. It is not a
claim that all payload byte sequences or decoded hashes are mutually distinct.

Four slots reuse earlier definitions:

| Portrait slot | Payload owner slot |
| ---: | ---: |
| 35 | 33 |
| 53 | 52 |
| 54 | 52 |
| 55 | 52 |

The complete identity rows remain private. The pointer table has exact original ROM parity for all
56 slots, and every one of the 52 source payload definitions has exact original ROM parity.

These are logical resource identities and aliases, not proof that every slot is naturally selected,
that the slot order is an appropriate public remake API, or that invalid selectors admit a fallback.

## Payload Header and Stream Partition

Each private payload definition has this ordered source shape:

1. one big-endian word count followed by that many four-byte eye entries;
2. one big-endian word count followed by that many four-byte mouth entries;
3. one 32-byte source palette; and
4. one Stack-compressed tile stream.

Across all 52 definitions, the accepted entry counts are 261 eye entries and 218 mouth entries. Each
byte-sized coordinate in those entries is within the observed `0..7` range. These names and bounds
describe the accepted source records; they do not prove visible animation, frame order, facial
meaning, or timing.

The complete aggregate byte accounting is:

```text
header bytes       = 3,788
compressed bytes   = 61,046
payload bytes      = 64,834
```

`summary.paletteByteCount=1664` is the 52 palettes already contained within
`summary.headerByteCount=3788`. It MUST NOT be added to the header denominator a second time. The
minimum and maximum complete header sizes are 36 and 100 bytes.

## Decode Shape and Aggregate Diagnostics

The 52 private Stack streams each decode to exactly 2,048 bytes, so the complete private decoded
corpus is 106,496 bytes.

The dedicated verifier also records these aggregate diagnostics:

| Diagnostic | Accepted value |
| --- | ---: |
| command groups | 2,510 |
| literal words | 37,017 |
| copy commands | 2,665 |
| copied words | 16,231 |
| maximum copy offset | 950 words |
| maximum copy length | 33 words |
| observed trailing span | 32..47 bits |

These values validate this corpus against the maintained decoder. They do not require a remake to
reproduce the original Stack microimplementation. The trailing span is only the stored span after
each logical terminator; it is not proven padding, zero-filled data, stability, or invisibility.

## Implementation-Neutral Logical Model

A complete private importer may use a model equivalent to:

```text
PortraitGraphicsCorpus {
    provenance {
        fixtureId
        upstreamCommit
        romSha256Identity
        verifierOutputSha256
    }
    pointerSlots[56] {
        logicalPortraitId
        payloadOwnerId
    }
    privatePayloadDefinitions[52] {
        logicalPayloadId
        privateSourceIdentity
        privateSourceAddress
        privateEyeEntries[]
        privateMouthEntries[]
        privatePaletteWords[16]
        privateCompressedBytes
        privateDecodedBytes[2048]
        privateHashesAndDecodeDiagnostics
    }
}
```

Per-definition source/H1/ROM addresses, big-endian count storage, raw entries/palette bytes,
compressed bytes, decoded art, per-resource sizes/hashes, complete payload source paths, and other
non-public details are private import and round-trip evidence. The bounded table and external witness
symbols/addresses, aggregate provenance, and metadata named in the public projection remain public.
After verification, a conforming remake may use engine-native resource references, animation
records, palettes, textures, and storage. It is not required to reproduce Mega Drive address space,
big-endian count words, the Stack codec, original buffers, or original file/container layout.

The importer MUST keep pointer-slot identity, payload-owner identity, entry-sequence identity,
palette identity, and decoded-tile identity distinct. Aliased pointer slots do not become duplicate
payload owners.

## Public and Private Projection

The public projection may retain only:

- fixture, upstream, ROM, and canonical-output provenance hashes;
- the `pt_Portraits` symbol and table address;
- the bounded external `LoadPortrait` and `LoadStackCompressedData` witness identities and addresses;
- the aggregate pointer/payload/alias, entry, byte, decode, parity, and decoder-diagnostic counts;
- the four tracked alias metadata rows; and
- the bounded counted-entry/palette/single-stream partition.

It MUST NOT publish raw pointers, complete non-alias assignments, payload symbols/source paths or
addresses, per-payload entries/offsets/sizes/hashes, palette words, compressed bytes, decoded art,
ROM excerpts, screenshots, emulator captures, or rendered presentation.

## H4 Remake Acceptance Surface

A future H4 implementation conforms when it can show that:

1. its private import retains 56 ordered logical slots and 52 ordered source payload owners;
2. the exact `35→33` and `53/54/55→52` alias relation is preserved without duplicating owners;
3. all 52 private payloads retain counted four-byte eye and mouth record identities, one 32-byte
   palette identity, and one ordered Stack-stream identity;
4. all 52 private streams deterministically reproduce their accepted 2,048-byte decoded identities;
5. complete private accounting closes at `3,788 + 61,046 = 64,834`, with 106,496 decoded bytes and
   without double-counting the 1,664 palette bytes;
6. engine-native resources can replace original pointers, count-word storage, Stack storage, and
   address layout without changing logical identity or aliasing; and
7. public reports expose only the bounded aggregate/provenance surface while copyrighted payloads
   and complete private identity material remain private.

H4 does not require original selector rules, loader microimplementation, window state, staging or
DMA operands, CRAM/VInt behavior, visible animation, rendered output, or timing. Those seams are
tested by their owning window/service/presentation contracts.

## Cross-System Separation

- [Portrait Window and State](portrait-window-state.md) consumes canonical records from this
  contract and retains portrait selection, `LoadPortrait` parsing/copy/call order, window state,
  eye/mouth update control, name-window behavior, transfer boundaries, and their Unknowns. It no
  longer independently owns or re-verifies this static catalog.
- [Graphics Service State](graphics-service-state.md) retains `LoadStackCompressedData`, its ABI, and
  service boundaries. Aggregate codec diagnostics here do not transfer that ownership.
- [Sprite-Dialogue Property Data](sprite-dialogue-property-data.md) retains entity map-sprite to
  portrait/SFX lookup identity and behavior.
- [UI Layout Data](ui-layout-data.md) retains normal/mirrored portrait-window layout payloads.
- [Interrupt, DMA, and Trap State](interrupt-dma-and-trap-state.md) retains VInt, DMA, and CRAM
  service/timing boundaries.
- dialogue, menu, battle-scene, map-script, text, localization, accessibility, licensing,
  replacement assets, and rendering remain with their own contracts or as Unknown.

## Evidence Matrix

| Contract area | Evidence label | Owner | Remaining boundary |
| --- | --- | --- | --- |
| 56 ordered slots, 52 source payload owners, four aliases | **Confirmed static** | `sf2-portrait-graphics-decode-v1` | Natural selection, byte uniqueness, visible identity |
| counted eye/mouth entries, palette, one stream per payload | **Confirmed static/private import** | same fixture/verifier | Visible animation, timing, malformed inputs |
| aggregate bytes, decode shape, diagnostics, parity | **Confirmed static** | same fixture/verifier | Stack microimplementation, tail-bit meaning, rendered art |
| selector, loader, window, update, and name-window chronology | **Separate-owner Confirmed static witness** | `portrait-window-state` | Runtime admission, transfer completion, visible frames |
| Stack service behavior | **Separate-owner Confirmed static** | `graphics-service-state` | Hardware/runtime timing |
| source-label visual intent | **Inferred, non-normative** | source vocabulary only | Facial meaning, scene role, authorial intent |
| reachability, presentation, persistence, replacement | **Unknown / separate owner** | future bounded evidence or product decision | Not an H4 data-fidelity requirement here |

## Reproduction

```powershell
uv run sf2 h2 portraits
uv run sf2 design-contracts test
uv run sf2 verify
```

The complete payload rows remain under ignored `local/derived/portrait-graphics-decode.json`. They
are reproducible private evidence, not tracked or distributable contract content.
