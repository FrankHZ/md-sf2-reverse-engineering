# Byte-Copy Service Contract

- Status: **Draft evidence-bound contract**
- Original fidelity: **Confirmed static** for the bounded entry identity, direction facts, and
  source-shaped loop/ABI chronology described below
- Modernization: **Allowed** for engine-native buffers and a platform `memmove` equivalent within
  the admitted domain
- Unknown: zero-length support, cross-sign-boundary ordering, address/length wrap, malformed or
  hardware-mapped ranges, caller reachability, atomicity, concurrency, timing, and hardware effects

## Purpose

This contract defines the smallest implementation-neutral byte-copy primitive supported by the
accepted technical-services evidence. It owns the logical operation represented by the original
`CopyBytes` entry without turning the original 68000 instruction sequence into a required remake
implementation.

The contract is intentionally narrower than a general memory API. It specifies result equivalence
only for an admitted positive-length, non-wrapping domain where the original signed address ordering
has the intended relation. Unsupported source edges remain explicit rather than being normalized into
modern library guarantees.

## Judgment Boundary

**Confirmed static:** the accepted fixture binds `CopyBytes` to ROM address `0x16D6` (`5846`) and
records two direction facts. The pinned source saves full `d7`, `a0`, and `a1`, compares the two
address registers with `cmpa.l a0,a1`, takes `bgt.w @Decrement`, otherwise executes a forward
post-increment byte loop, and restores the three saved registers before returning. The decrement path
adds `d7.w` to both addresses, then uses a pre-decrement byte loop before the same restore/return seam.

**Confirmed bounded result:** for lengths `1..0x7FFF` (`1..32767`), non-wrapping source and
destination ranges, and address representations for which the signed comparison orders the two
range starts as intended, the operation has ordinary overlap-preserving move results. The backward
path handles a destination beginning above the source; the forward path handles a destination at or
below the source. This is a bounded result statement, not an unsigned-address reinterpretation of the
original branch.

**Inferred:** the source name, small register interface, and reuse shape suggest a general-purpose
memmove-like utility. That engineering role is not a player-facing meaning, a complete caller
contract, or a requirement that a remake expose a public service with the same name.

**Unknown or excluded:** whether zero length was intentionally supported; behavior for `0x8000`
through `0xFFFF` lengths on the backward path; cross-sign-boundary address pairs; source or
destination wrap; malformed, inaccessible, or memory-mapped I/O ranges; complete caller admission;
interrupt or DMA interaction; atomicity; cycle counts; performance; and any caller-visible UI,
graphics, audio, persistence, or presentation result.

## Evidence Contract

This contract consumes only the following fields from
[`sf2-tech-services-static-v1`](../../../tests/fixtures/h2/tech-services-static-v1.json):

- `function.CopyBytes`;
- `expected.serviceFacts.byteCopyChoosesBackwardWhenDestinationIsHigher`;
- `expected.serviceFacts.byteCopyChoosesForwardOtherwise`;
- the fixture's ROM and pinned-upstream provenance.

The phrase “destination is higher” in the fixture is interpreted only through the original source
condition: `cmpa.l a0,a1` followed by signed-greater-than `bgt.w`. It MUST NOT be silently widened to
an unsigned comparison over arbitrary 32-bit address representations.

Bounded source chronology is checked against the pinned
[`bytecopy.asm`](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/tech/bytecopy.asm)
and described by the owning
[`technical-services.md`](../../research/technical-services.md). The executable verifier remains
[`services.py`](../../../src/sf2tool/h2/services.py).

This contract does **not** consume `expected.resourceFacts`, `expected.soundDriverFacts`,
`expected.inputFacts`, `expected.randomServicesFacts`, `expected.sramFacts`, music-wait facts, or
any other sibling field from the aggregate fixture.

### Exact research-index denominator

The accepted fixture is linked directly to ten research records. This contract changes the semantic
association of exactly one:

| Record | Design ownership after this contract |
| --- | --- |
| `tech.services.byte-copy` | this contract; currently unassociated before registration |
| `tech.services.music-wait` | remains unassociated and outside this contract |
| `tech.services.resource-icon` | unchanged: `ui-graphics-asset-data` |
| `tech.services.resource-graphics` | unchanged: `text-and-font-system` |
| `tech.services.resource-text-trees` | unchanged: `text-and-font-system` |
| `tech.services.resource-title` | unchanged: `unused-technical-asset-data` |
| `tech.services.resource-base` | unchanged: `unused-technical-asset-data` |
| `tech.services.input` | unchanged: `input-system` |
| `tech.services.sram` | unchanged: `save-system` |
| `tech.services.thinking-rng` | unchanged: `randomness` |

No caller record, resource record, sound record, SRAM record, input record, or RNG record gains this
contract merely because it shares the fixture.

## Original Static Operation

### Logical inputs

The source comment and instructions identify three operation inputs:

| Source identity | Logical role | Accepted boundary |
| --- | --- | --- |
| `a0` | source start | private original address; engine-native source reference in a remake |
| `a1` | destination start | private original address; engine-native destination reference in a remake |
| `d7.w` | byte length | admitted common domain `1..0x7FFF` only |

The original routine saves full `d7`, `a0`, and `a1` values with `movem.l d7-a1,-(sp)` and restores
the same range before `rts`. Its temporary stack use is balanced on both branches. This establishes
preservation of those three full register values and stack balance at the source seam. It does not
establish CCR neutrality or an all-register ABI guarantee.

### Signed direction decision

The source performs a 32-bit address-register comparison and takes `bgt`, the signed
greater-than condition. Therefore the contract preserves two distinct statements:

1. the original private fidelity record retains `cmpa.l a0,a1; bgt.w @Decrement` exactly;
2. the public logical result contract applies only where the representation's signed ordering gives
   the intended relative ordering of the two range starts.

The simplest admitted original-address case is that both starts lie in the same signed-order region,
so signed and ordinary monotonic ordering agree. Cross-sign-boundary address pairs are outside this
contract even if a platform's unsigned pointer comparison would order them.

### Forward source chronology

When the signed-greater-than branch is not taken, the source:

1. subtracts one from `d7.w`;
2. copies one byte from the current source to the current destination;
3. post-increments both addresses;
4. repeats with `dbf` until the admitted count is exhausted;
5. restores full `d7`, `a0`, and `a1` and returns.

For admitted positive lengths, this path is result-equivalent to a forward byte move. It preserves a
lower-destination overlap because bytes are read before later source positions can be overwritten.
Equal source and destination references mechanically rewrite each byte with itself; the logical
destination result is unchanged.

### Backward source chronology

When the signed-greater-than branch is taken, the source:

1. adds `d7.w` to both address registers with `adda.w`;
2. subtracts one from `d7.w`;
3. pre-decrements both addresses and copies one byte;
4. repeats with `dbf` toward the range starts;
5. restores full `d7`, `a0`, and `a1` and returns.

`adda.w` sign-extends its word operand. The common admitted domain therefore stops at `0x7FFF`;
`0x8000..0xFFFF` MUST NOT be described as positive backward-copy lengths in this contract. Within the
admitted domain, moving from the end preserves a higher-destination overlap.

### Unsupported source edges

The source subtracts one before entering either `dbf` loop. This document deliberately does not turn
the mechanical zero-word edge into a supported API guarantee. Likewise, it does not define a broader
forward-only length domain, because the common contract is intended to be symmetric across the two
direction paths.

An original-instruction emulator may naturally reproduce additional edges. A remake adapter is not
required to accept them until a separate evidence owner defines their admission and expected result.

## Implementation-Neutral Import Model

A conforming logical model may use the following shape:

```text
ByteCopyRequest {
  sourceRef
  destinationRef
  lengthBytes       // admitted: 1..0x7FFF
}

ByteCopyDomain {
  sourceRangeDoesNotWrap
  destinationRangeDoesNotWrap
  signedStartOrderingIsApplicable
  rangesAreReadableAndWritableOrdinaryMemory
}

ByteCopyResult {
  destinationBytes
  sourceRefPreserved
  destinationRefPreserved
  lengthValuePreserved
}

PrivateOriginalProvenance {
  sourceSymbol
  romAddress
  sourceInstructionIdentity
  h1AndRomVerification
}
```

`sourceRef`, `destinationRef`, and `lengthBytes` are logical values, not public ROM pointers or 68000
registers. A future adapter may use spans, slices, arrays, native memory, or another bounded buffer
type. Aliasing between the two logical ranges MUST remain representable because overlap is part of the
accepted result contract.

After private source/H1/ROM verification, a remake MAY implement the operation with a platform
`memmove` equivalent. It is not required to reproduce the forward/backward micro-loop, `dbf`, stack
frame, register names, big-endian storage, Mega Drive address space, or original instruction timing.

## Public and Private Projection

The public contract may retain:

- fixture ID and pinned provenance;
- the `CopyBytes` symbol and accepted address metadata;
- the raw signed comparison/branch identity in paraphrased form;
- the admitted `1..0x7FFF`, non-wrapping domain;
- logical overlap/result requirements;
- small project-authored synthetic byte vectors for future H4 tests.

Private verification inputs include original source/H1/ROM bytes, exact instruction encodings,
original address values, and any complete caller corpus. They are not required in a distributable
remake and MUST NOT be published merely to test this operation. This contract owns no original art,
text, audio, map, or other content payload.

## Cross-System Separation

- [`map-palette-data`](map-palette-data.md) owns its source-shaped 32-byte copy handoff and palette
  rule, not the general `CopyBytes` microimplementation.
- [`save-system`](save-system.md) owns `CopyBytesToSram` and `CopyBytesFromSram`, including their
  interleaved physical-byte/checksum behavior. Those are distinct routines.
- [`graphics-service-state`](graphics-service-state.md) owns decompression service boundaries, not
  generic byte-copy behavior.
- [`interrupt-dma-and-trap-state`](interrupt-dma-and-trap-state.md) owns accepted DMA, VInt, and trap
  seams. This contract establishes no transfer scheduling or interrupt behavior.
- Window, menu, title, suspend, entity, and graphics contracts retain their own caller transactions
  and visible outcomes. A source call to `CopyBytes` does not transfer those semantics here.
- `tech.services.music-wait` and the [`audio-system`](audio-system.md) remain separate; this contract
  owns no wait, mailbox, driver, or audible-result rule.

## H4 Acceptance Surface

Within the admitted domain, a future H4 adapter MUST verify destination-result equivalence for small
synthetic cases covering:

1. forward non-overlap with the destination ordered below the source;
2. higher-destination overlap requiring the original backward result;
3. lower-destination overlap requiring the original forward result;
4. equal source and destination references;
5. preservation of the logical source reference, destination reference, and length value presented
   to the adapter.

The test vectors MUST be project-authored and contain no original game payload. The adapter MAY call
the platform's overlap-safe move primitive. H4 MUST NOT require observation of the platform's copy
direction, instruction count, stack operations, register allocation, CCR state, or timing.

H4 MUST reject or classify outside this contract rather than guessing results for zero length,
lengths above `0x7FFF`, wrapping ranges, cross-sign-boundary original addresses, inaccessible memory,
or memory-mapped I/O. A future deliberate broader API is a modernization decision and needs its own
specified behavior; it is not evidence about the original routine.

## Evidence Matrix

| Contract area | Evidence label | Owner | Remaining boundary |
| --- | --- | --- | --- |
| entry identity/address | **Confirmed static** | `sf2-tech-services-static-v1`, `function.CopyBytes` | natural callers and reachability |
| direction facts | **Confirmed static** | same fixture, two named byte-copy fields | signed-order representation edges |
| save/compare/loop/restore chronology | **Confirmed static source** | pinned `bytecopy.asm` and technical-services prose | CCR, cycle timing, unsupported lengths/ranges |
| memmove-like utility purpose | **Inferred** | source identity and reusable call shape | public API placement and caller policy |
| UI, graphics, audio, persistence, DMA/VInt, presentation | **Unknown / separate owner** | caller and subsystem contracts | complete runtime and player-visible outcomes |

## Reproduction

```powershell
uv run sf2 h2 tech-services
uv run sf2 design-contracts test
uv run sf2 research-index test
```

The focused H2 command must continue to report fixture
`sf2-tech-services-static-v1`, `CopyBytes` address `5846`, and the accepted direction facts. Generated
reports remain under ignored `local/derived/`.
