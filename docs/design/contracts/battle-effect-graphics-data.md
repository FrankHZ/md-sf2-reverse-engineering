# Battle Effect Graphics Data Contract

- Status: **Confirmed static resource identities, container partition, and decode shape**
- Evidence date: 2026-08-14
- Scope: original spell, invocation, status-animation, and battle-transition graphics corpora as a
  private, engine-neutral import

## Judgment Boundary

This contract owns the static identities and private import shape of the original battle-effect
graphics corpus. It does not own effect selection, scene control, loader behavior, decompression
service behavior, transfer completion, or presentation.

- **Confirmed**: 23 spell container/stream owners; four invocation container owners containing 15
  frames and 30 ordered streams; one status-animation stream; two battle-transition streams; exact
  private container/offset/palette/stream relations; aggregate compressed-stream and decoded byte
  counts; ROM parity counts; and bounded aggregate decoder diagnostics reproduced by the dedicated
  fixture.
- **Inferred, non-normative**: names such as `Spell`, `Invocation`, `StatusAnimation`,
  `BattlesceneTransition`, `frame`, and `layer` preserve source identities and bounded consumer
  vocabulary only. They do not establish visible purpose, composition, spatial arrangement, palette
  meaning, timing, or authorial intent.
- **Unknown / separate owner**: natural selection and reachability of every resource; invalid indices
  and malformed data; the contents, stability, and visibility of invocation transfer tails; palette
  meaning; layer order; frame timing; transition composition; transfer completion; rendered output;
  caching, runtime modification, persistence, replacement policy, and licensing.

## Evidence Owner and Consumed Surface

The sole executable data owner consumed by this contract is
`sf2-battle-effect-graphics-decode-v1`
([fixture](../../../tests/fixtures/h2/battle-effect-graphics-decode-v1.json),
[verifier](../../../src/sf2tool/h2/battle_effect_graphics.py),
[schema](../../../schemas/h2-battle-effect-graphics-decode-fixture.schema.json), and
[manifest](../../../manifests/extractions/battle-effect-graphics-decode.json)). Its prose owners are
[Technical Graphics and Decompression Services](../../research/technical-graphics.md) and
[Battle Scene Engine](../../research/battle-scene-engine.md).

This data contract consumes:

- all eight top-level table/root and pointer-slot identities in `table`;
- the family, stream, compressed-stream byte, decoded byte, parity, and aggregate decoder fields in
  `summary`;
- the complete generated private rows in `spellGraphics[23]`, `invocationContainers[4]`,
  `invocationStreams[30]`, `statusAnimation`, and `transitionGraphics[2]`; and
- the accepted upstream, ROM, and canonical-output provenance.

The `function` identities and `summary.invocationTransferByteCount` /
`summary.invocationTransferTailByteCount` are retained only as external consumer/service witness
metadata. Effect selection, the spell/invocation/status/transition consumer seams, Stack handoff,
fixed transfer requests, and the unknown invocation tail remain with
[Battle Scene Command and Presentation Data](battle-scene-presentation.md).
`LoadStackCompressedData` remains with [Graphics Service State](graphics-service-state.md), and
hardware-facing transfer services remain with
[Interrupt, DMA, and Trap State](interrupt-dma-and-trap-state.md). None of those function records
gains this contract.

The generated private rows remain under ignored
`local/derived/battle-effect-graphics-decode.json`. Their complete graphs, source symbols and paths,
resource addresses, exact container and stream offsets, per-resource sizes and hashes, palette
bytes, compressed bytes, and decoded art are private verification inputs, not public contract
payloads.

The aggregate `sf2-auxiliary-data-static-v1` and
`sf2-compression-consumers-static-v1` fixtures are explicitly excluded. The battle-scene engine and
animation fixtures remain presentation/control owners and are not data owners here.

## Direct Binding and Association Boundary

The dedicated fixture directly binds exactly five research-index records:

| Record ID | Fixture role | Contract treatment |
| --- | --- | --- |
| `auxiliary.data.pt-spellgraphics` | `pt_SpellGraphics` at ROM address 1,830,624 | new association candidate |
| `auxiliary.data.pt-invocationsprites` | `pt_InvocationSprites` at ROM address 1,221,368 | new association candidate |
| `auxiliary.data.tiles-statusanimation` | `tiles_StatusAnimation` at ROM address 1,745,262 | new association candidate |
| `auxiliary.data.pt-battlescenetransitiontiles` | transition pointer table at ROM address 1,745,686 | new association candidate |
| `battle.scene.load-invocation-frame` | invocation consumer at ROM address 105,458 | unchanged; retained only by `battle-scene-presentation` |

No other `auxiliary.data.*`, `battle.scene.*`, `tech.graphics.*`, spell-resolution, animation,
interrupt, DMA, palette, audio, or presentation record is associated by this contract.

## Top-Level Roots and Pointer Identities

The dedicated fixture tracks four bounded source roots:

| Root identity | ROM address |
| --- | ---: |
| `pt_SpellGraphics` | 1,830,624 |
| `pt_InvocationSprites` | 1,221,368 |
| `tiles_StatusAnimation` | 1,745,262 |
| `pt_BattlesceneTransitionTiles` | 1,745,686 |

It also tracks four source pointer-slot identities at addresses 1,802,252; 1,048,580; 1,572,868;
and 1,572,872. These bounded identities and addresses are public provenance metadata. The complete
resource graphs and resource addresses remain private.

The four roots do not all have the same physical shape. Spell and invocation roots lead to ordered
container tables, the status root identifies one direct stream, and the transition root leads to an
ordered two-entry table. A conforming importer MUST preserve those distinctions.

## Spell Container Identities

The private spell table has 23 ordered source container definitions. Each container retains:

1. its source decoded-size header identity;
2. its six-byte source palette prefix; and
3. one Stack-compressed stream beginning after the eight-byte pre-stream region.

Every private stream decodes to the count stored by its own source header. The 23 count is a source
definition/owner-identity count, not proof that every compressed byte sequence or decoded hash is
mutually distinct. The complete decoded-size sequence, palette values, stream sizes, hashes, and
bytes remain private.

## Invocation Container, Frame, and Stream Identities

Four ordered source invocation container definitions retain a complete private relation among:

- one container identity;
- its source frame-offset area;
- one 32-byte palette region;
- its ordered frame identities; and
- two ordered source stream slots per frame.

The four containers contain 15 frames and therefore 30 ordered streams. Every invocation stream
decodes to 4,096 bytes. `frame` and the two source slot positions are logical source identities; this
contract does not promote them into visible animation, foreground/background, layer-priority, or
timing semantics. The four container owners and 30 stream slots are source identities, not a claim
that every container or decoded stream has a mutually distinct byte hash.

Two source-shaped consumer paths request 4,608 bytes for an invocation stream. That fixed request is
a presentation/consumer seam, not additional decoded data owned here. The exact difference is:

```text
4,608 requested - 4,096 decoded = 512 bytes per stream
30 * 512 = 15,360 aggregate tail bytes
```

The tail contents, source, initialization, stability, transfer completion, and visibility are
**Unknown**. A private importer MUST NOT infer or synthesize those bytes from adjacent palettes,
streams, memory, or container data.

## Status and Transition Stream Identities

The status-animation root identifies one private Stack stream that decodes to 1,248 bytes. The
transition table identifies two ordered source stream definitions, each decoding to 6,144 bytes.
Those are source resource identities and decoded-size facts, not proof of runtime reachability,
visible status meaning, transition direction, ordering intent, or byte-hash uniqueness.

The presentation contract retains the status consumer's fixed `0x270`-word request and the
transition selector/pointer handoff. This data contract owns only the canonical private records
consumed at those seams.

## Field-Exact Aggregate Accounting

The accepted stream denominator closes as:

```text
23 spell + 30 invocation + 1 status + 2 transition = 56 streams
```

Those streams occupy 46,364 compressed bytes and decode to 200,992 bytes in total.
`compressedStreamByteCount=46,364` counts only the compressed stream spans. It is not a complete
source-container denominator and MUST NOT be described as including spell headers, spell palettes,
invocation offset areas, invocation palettes, pointer tables, or other container material.

The invocation consumer metadata closes separately:

```text
30 * 4,608 requested bytes = 138,240
30 *   512 unknown tail    =  15,360
```

The resource ROM parity count is 30, partitioned as 23 spell containers, four invocation
containers, one status resource, and two transition resources. Pointer-slot ROM parity is four.
Table ROM parity is three: spell, invocation, and transition. The direct status stream is not a
fourth pointer table.

## Decode Shape and Aggregate Diagnostics

The dedicated verifier records these aggregate diagnostics across the 56 streams:

| Diagnostic | Accepted value |
| --- | ---: |
| command groups | 1,630 |
| literal words | 19,091 |
| copy commands | 6,480 |
| copied words | 81,405 |
| maximum copy offset | 1,998 words |
| maximum copy length | 33 words |
| observed trailing span | 32..53 bits |

These values validate this corpus against the maintained decoder. They do not require a remake to
reproduce the original Stack microimplementation. The trailing span is only the stored span after
each logical terminator; it is not proven padding, zero-filled data, stability, or invisibility.

## Implementation-Neutral Logical Model

A complete private importer may use a model equivalent to:

```text
BattleEffectGraphicsCorpus {
    provenance {
        fixtureId
        upstreamCommit
        romSha256Identity
        verifierOutputSha256
    }
    spellRoot
    privateSpellContainers[23] {
        logicalSpellResourceId
        privateSourceIdentity
        privateDecodedSizeHeader
        privatePaletteBytes[6]
        privateStream
    }
    invocationRoot
    privateInvocationContainers[4] {
        logicalInvocationResourceId
        privateFrameOffsetRelation
        privatePaletteBytes[32]
        privateFrames[] {
            logicalFrameId
            privateLayerStreams[2]
        }
    }
    privateStatusStream
    transitionRoot
    privateTransitionStreams[2]
}
```

Every private stream record retains its complete source identity, offset, compressed bytes, decoded
bytes, hashes, and decoder diagnostics. Complete source/H1/ROM addresses, big-endian pointer and
offset storage, raw headers and palettes, compressed bytes, decoded art, source paths, per-resource
sizes/hashes, and other non-public details are private import and round-trip evidence. The bounded
root, pointer-slot, and external witness identities/addresses plus aggregate provenance and metadata
named in the public projection remain public.

After verification, a conforming remake may use engine-native resource references, palettes,
textures, formats, and storage. It is not required to reproduce Mega Drive address space,
big-endian pointer/container layout, the Stack codec, original buffers, or original source files.

## Public and Private Projection

The public projection may retain only:

- fixture, upstream, ROM, and canonical-output provenance hashes;
- the four bounded root identities/addresses and four pointer-slot addresses;
- the six external function witness identities/addresses;
- family/stream, compressed-stream, decoded, parity, invocation-request/tail, and decoder-diagnostic
  aggregate counts;
- the bounded `23 + 30 + 1 + 2` family partition and `4 containers / 15 frames / 30 streams`
  invocation shape; and
- the explicit runtime questions and Unknown boundaries already tracked by the fixture.

It MUST NOT publish complete pointer/container/frame/stream graphs, resource symbols/source paths or
addresses, per-resource offsets/sizes/hashes, palette values, raw headers, compressed bytes, decoded
art, ROM excerpts, screenshots, emulator captures, or rendered presentation.

## H4 Remake Acceptance Surface

A future H4 implementation conforms when it can show that:

1. its private import retains the four distinct root shapes and all bounded pointer/table identities;
2. all 23 private spell container owners retain their decoded-size header, six-byte palette, and
   one-stream relation, and each stream reproduces its accepted private decoded identity;
3. all four private invocation container owners retain 15 ordered frame identities, two ordered
   stream identities per frame, the complete private offset relation, and the separate 32-byte
   palette region;
4. all 30 invocation streams reproduce their accepted 4,096-byte private decoded identities without
   inventing the separate 512-byte consumer tails;
5. the private status stream reproduces its accepted 1,248-byte decoded identity and the two private
   transition streams reproduce their accepted 6,144-byte decoded identities;
6. aggregate stream accounting closes at 56 streams, 46,364 compressed-stream bytes, and 200,992
   decoded bytes with the accepted parity/decoder diagnostics;
7. engine-native resources can replace original pointers, offsets, Stack storage, and address layout
   without changing logical owners, container/frame/stream relations, or private decoded identities;
   and
8. public reports expose only the bounded aggregate/provenance surface while copyrighted payloads
   and complete private identity material remain private.

H4 does not require original selectors, loader microimplementation, invocation transfer-tail
contents, transfer completion, palette meaning, layer composition, CRAM/VInt/DMA behavior, rendered
output, or timing. Those seams are tested by their owning presentation/service contracts.

## Cross-System Separation

- [Battle Scene Command and Presentation Data](battle-scene-presentation.md) consumes canonical
  records from this contract and retains invocation/spell loaders, status initialization consumption,
  transition selection, Stack handoffs, fixed transfer requests, the 512-byte unknown invocation
  tail boundary, scene chronology, and presentation Unknowns. It no longer independently owns or
  re-verifies this static catalog.
- [Graphics Service State](graphics-service-state.md) retains Stack decompression. Aggregate decoder
  diagnostics here do not transfer service ownership.
- [Interrupt, DMA, and Trap State](interrupt-dma-and-trap-state.md) retains VInt, DMA, CRAM, transfer
  completion, and hardware timing boundaries.
- spell resolution and battle action contracts retain effect choice and gameplay consequences; they
  do not own these graphics payloads.
- actor sprites/animations, backgrounds, weapons/grounds, portraits, terrain, special/map/UI
  graphics, localization, accessibility, replacement assets, licensing, and rendering remain with
  their own contracts or as Unknown.

## Evidence Matrix

| Contract area | Evidence label | Owner | Remaining boundary |
| --- | --- | --- | --- |
| 23 spell container/stream owners | **Confirmed static/private import** | `sf2-battle-effect-graphics-decode-v1` | Selection, byte uniqueness, visible spell meaning |
| 4 invocation containers, 15 frames, 30 streams | **Confirmed static/private import** | same fixture/verifier | Layer meaning, timing, natural reachability |
| 1 status and 2 transition streams | **Confirmed static/private import** | same fixture/verifier | Visible status/transition meaning and composition |
| aggregate compressed/decoded counts, parity, diagnostics | **Confirmed static** | same fixture/verifier | Complete source-container denominator, Stack microimplementation, tail-bit meaning |
| loaders, Stack handoffs, transfer requests, invocation tail boundary | **Separate-owner Confirmed static witness** | `battle-scene-presentation` / `graphics-service-state` | Transfer completion and tail contents/stability/visibility |
| source-label visual intent | **Inferred, non-normative** | source vocabulary only | Composition, palette/layer meaning, authorial intent |
| reachability, presentation, persistence, replacement | **Unknown / separate owner** | future bounded evidence or product decision | Not an H4 data-fidelity requirement here |

## Reproduction

```powershell
uv run sf2 h2 battle-effect-graphics
uv run sf2 design-contracts test
uv run sf2 verify
```

The complete private rows remain under ignored `local/derived/battle-effect-graphics-decode.json`.
They are reproducible private evidence, not tracked or distributable contract content.
