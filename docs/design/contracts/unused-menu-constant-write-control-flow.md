# Unused Menu Constant-Write Control-Flow Contract

- Status: **Draft evidence-bound contract**
- Original fidelity: **Confirmed static** for the bounded `sub_15268` identity, source interval,
  constant-write order, two disjoint admitted memory ranges, and thirteen-iteration `DBF` loop
- Modernization: **Allowed** to omit a production endpoint until a separate accepted caller requires
  one, or to use an engine-native typed write relation in a private compatibility adapter
- Unknown: natural or computed admission, source vocabulary meaning, RAM purpose, later consumers,
  register/CCR/stack behavior, interrupt visibility, timing, and player-visible output

## Purpose

This contract preserves the smallest complete unassociated source file in the accepted common-menu
inventory: `sub_15268` in `unusedsub_15268.asm`. The routine writes the same source constant once at
an isolated negative offset, then thirteen times through an ordered post-increment loop, and returns.

The upstream path and comment call the function “unused” and a “menu engine function.” Those labels
are archival vocabulary, not proof of purpose, dead-code status, natural unreachability, or visible
menu behavior. This contract owns only the bounded static write algorithm and its provenance.

## Judgment Boundary

**Confirmed static:** [`sf2-common-menus-static-v1`](../../../tests/fixtures/h2/common-menus-static-v1.json)
binds `sub_15268` to ROM address `0x15268` (`86632`). The accepted H2 inventory row for
`code/common/menus/unusedsub_15268.asm` records:

- SHA-256 `349BAB7EDDEC2739965C898EEB4415CEB3AF934EC8835CBE8A0E89EF42A054D4`;
- 22 source lines and six statements;
- the two global labels `sub_15268` and `loc_15278`;
- zero local labels, zero outgoing direct calls, and zero indirect call sites.

The H1 listing places the complete function in the exclusive interval `0x15268..0x15284`, a
28-byte span. The separate `YesNoPrompt` source begins at `0x15284`. That adjacency is neither a
call nor fallthrough: the bounded routine ends with `rts`.

Direct review of the pinned source confirms one isolated constant write, one thirteen-body loop,
and no call or handoff. A complete source-tree token search finds `sub_15268` only at its definition
and end comment. That is a bounded symbolic-occurrence inventory, not proof against an indirect,
computed, raw-address, debug, or externally injected invocation.

**Inferred:** only the upstream `unused` and `menu engine` vocabulary. The constant, destination
symbol, and location do not establish intended layout content, clearing behavior, initialization,
presentation, or product exposure.

**Unknown or excluded:** natural caller admission; indirect or computed reachability; caller state;
why the routine exists; the semantic meaning of `byte_FFCC86`; whether a later reader observes any
write; memory ownership outside the two admitted ranges; `a0` and `d7` post-state as a portable ABI;
CCR effects; stack and return-address behavior; interruption or atomicity; hardware bus effects;
VInt, DMA, VDP, or rendering behavior; visible tiles, windows, or menus; timing; persistence;
malformed invocation; and instruction-body parity among source, H1, and ROM.

## Evidence Contract

This contract consumes only:

- `function.sub_15268` from
  [`sf2-common-menus-static-v1`](../../../tests/fixtures/h2/common-menus-static-v1.json);
- the accepted H2 generated owner-row identity, source hash, counts, and no-outgoing-call inventory;
- `upstreamCommit` and `romSha256` provenance;
- the bounded chronology in pinned
  [`unusedsub_15268.asm`](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/menus/unusedsub_15268.asm);
- the H1 entry, instruction-boundary, and exclusive-end identities.

Its executable owner is [`menus.py`](../../../src/sf2tool/h2/menus.py), and its prose owner is
[common-menu research](../../research/common-menus.md).

The contract consumes none of the fixture's `expected.menuFacts`, root `menuFacts`, or
`alternateSource` fields. In particular it does not consume diamond-menu, yes/no prompt, portrait,
service, item, shop, member-screen, timing, input, or presentation facts. It also consumes no H3
fixture and no UI-graphics, UI-layout, window, graphics-service, interrupt, or technical-interface
fixture.

The selected H2 owner resolves the representative entry and validates the accepted source inventory
under pinned ROM provenance. It does not compare this function's complete encoded body byte for byte
between H1 and ROM. Complete instruction encodings may be checked privately by a future stronger
verifier, but they are not Confirmed here and are not an H4 requirement.

### Exact fixture-linked denominator

The fresh H2 join contains 42 research-index records:

- 41 records carry direct `sf2-common-menus-static-v1` evidence;
- `menus.load-portrait` is the one membership-only record, with dedicated portrait evidence and
  [`portrait-window-state`](portrait-window-state.md) ownership.

The 41 direct bindings partition exactly as follows:

| Partition | Count | Ownership after this contract |
| --- | ---: | --- |
| `menus.unused-15268` | 1 | This contract only |
| Already associated direct records | 11 | Unchanged |
| Other unassociated direct records | 29 | Remain unassociated |

The eleven existing direct associations remain exactly:

- `menus.blacksmith-actions`, `menus.caravan-actions`, `menus.church-actions`, and
  `menus.shop-actions` with [`service-interactions`](service-interactions.md);
- `menus.ally-portrait`, `menus.combatant-portrait`, `menus.name-under-portrait`,
  `menus.portrait-functions`, and `menus.portrait-window` with
  [`portrait-window-state`](portrait-window-state.md);
- `menus.diamond` and `menus.tile-pointers` with
  [`ui-graphics-asset-data`](ui-graphics-asset-data.md).

`menus.unused-12606`, `menus.unused-156a8`, `menus.yes-no-prompt`, and the other 26 unassociated
direct siblings remain unassociated. The eventual semantic association diff is exactly:

```text
menus.unused-15268
  + docs/design/contracts/unused-menu-constant-write-control-flow.md
```

## Source-Shaped Write Relation

The source establishes this exact order:

1. load the source symbol `byte_FFCC86` (`0xFFCC86`) into `a0`;
2. write longword `0xC020C020` at `-0x50(a0)`, the half-open byte range
   `0xFFCC36..0xFFCC3A`;
3. execute `moveq #0xC,d7`;
4. at `loc_15278`, write longword `0xC020C020` to `(a0)+`;
5. execute `dbf d7,loc_15278`;
6. return with `rts` after the loop terminates.

The loop body runs before each `DBF` test. Starting the low word at `0x000C` therefore produces
thirteen body executions, for indexes `i=0..12`, not twelve. The loop writes at
`0xFFCC86 + 4*i`, covering the half-open range `0xFFCC86..0xFFCCBA`.

The complete admitted memory-write projection is thus:

| Region | Longwords | Bytes | Stored value |
| --- | ---: | ---: | --- |
| isolated `base-0x50` range | 1 | 4 | `0xC020C020` |
| ordered `base+4*i`, `i=0..12` range | 13 | 52 | `0xC020C020` |
| total | 14 | 56 | same source constant |

The two ranges are disjoint. The source does not write the intervening bytes. This is an exact
source memory relation; it is not evidence that the constant represents blank tiles, a window
layout, a clear value, a palette, or any other player-visible concept.

## Implementation-Neutral Model

A private evidence/import model may retain:

```text
UnusedMenuConstantWriteEvidence
  identity
    sourceSymbol = sub_15268
    sourcePath
    sourceSha256
    h1EntryAddress = 0x15268
    exclusiveEndAddress = 0x15284
    upstreamCommit
    acceptedRomSha256Provenance

  sourceInventory
    sourceLineCount = 22
    statementCount = 6
    globalLabels = [sub_15268, loc_15278]
    localLabelCount = 0
    outgoingDirectCallCount = 0
    indirectCallSiteCount = 0
    externalSymbolicCallerOccurrenceCount = 0

  admittedMemoryProjection
    baseIdentity = byte_FFCC86
    baseSourceAddress = 0xFFCC86
    constantLongword = 0xC020C020
    isolatedWriteOffset = -0x50
    loopInitialLowWord = 0x000C
    loopBodyCount = 13
    loopStrideBytes = 4
    orderedLoopIndexes = 0..12
    totalLongwordWrites = 14
    totalWrittenBytes = 56
```

This notation is a provenance and compatibility model, not a required runtime class, public memory
layout, or 68000 emulator API. After private source and H1 identity verification under the fixture's
accepted ROM provenance, a remake may express the admitted write result with a typed array, a list
fill, or a trace-only archival adapter.

It need not reproduce the Mega Drive address space, `a0`, `d7`, `DBF`, big-endian longword storage,
or the original instruction sequence in production code. Exact instruction-body parity is not an
accepted premise of this contract.

## Public and Private Projection

The public contract may retain:

- fixture ID, source path/hash, upstream commit, and ROM-identity provenance;
- source symbol, entry, exclusive end, physical span, and bounded owner-row counts;
- `byte_FFCC86`, the selected source address, relative offset, constant identity, loop count/stride,
  two half-open ranges, and aggregate 14-longword/56-byte totals;
- the bounded zero-outgoing-call and zero-external-symbolic-caller summaries;
- the exact 42/41 association partition.

Complete source and H1 bodies, encoded instruction bytes, ROM excerpts, surrounding RAM contents,
emulator state, traces, captures, and any later-read data remain private. Public artifacts must not
publish original UI, text, graphics, audio, or other copyrighted payloads.

## Original Fidelity and Modernization

Original-fidelity evidence preserves the distinction between:

- one isolated write and thirteen contiguous loop writes;
- `moveq #0xC` and thirteen body executions;
- the source constant and any unproven visual meaning;
- zero symbolic callers and universal runtime unreachability;
- the admitted memory projection and complete machine state.

A modern engine may omit, erase, or inline a production endpoint until a separate accepted caller
contract requires one. It may retain a private compatibility seam for archival testing. If such a
seam exists, synthetic admitted memory can verify the two exact write ranges, order, count, and
constant.

Any claim that admitted bytes outside those two ranges remain unchanged is scoped only to the
synthetic admitted memory projection. It is never an all-machine-state invariant. The original
source explicitly mutates `a0` and `d7` and returns through `rts`; register post-state, CCR, stack,
return-address mechanics, and concurrent machine state remain Unknown and excluded.

## H4 Acceptance Surface

A future private compatibility adapter or archival importer satisfies this contract when:

1. the fixture, source path/hash, upstream commit, accepted ROM provenance, entry, and exclusive end
   remain traceable;
2. the source inventory retains 22 lines, six statements, two global labels, zero local labels,
   zero outgoing direct calls, and zero indirect call sites;
3. the admitted memory relation preserves one `base-0x50` write followed by thirteen ordered
   `base+4*i` writes for `i=0..12`, all using `0xC020C020`;
4. tests keep the two ranges disjoint and count exactly 14 longwords/56 admitted bytes;
5. a synthetic compatibility check, if implemented, asserts unchanged bytes only outside the two
   ranges within its admitted memory projection, never unchanged registers, CCR, stack, timing,
   interrupts, or complete machine state;
6. an engine-native implementation may replace the source loop while preserving the admitted write
   result and provenance, without reproducing Mega Drive addresses or instruction mechanics;
7. omission of a production endpoint remains allowed until a separate accepted caller contract
   closes admission and reachability;
8. zero symbolic caller occurrences are not reported as dead code or universal unreachability;
9. `YesNoPrompt` adjacency creates no call, fallthrough, data dependency, or ownership claim;
10. only `menus.unused-15268` is registered, while all 41 sibling/membership records keep their
    existing association state.

## Cross-System Separation

- [`window-system`](window-system.md) retains window allocation, movement, update, and lifecycle
  behavior. This contract does not identify either write range as a window buffer.
- [`ui-layout-data`](ui-layout-data.md) retains canonical UI layout assets and their private import
  fidelity. No layout payload or assignment is consumed here.
- [`graphics-service-state`](graphics-service-state.md) and
  [`interrupt-dma-and-trap-state`](interrupt-dma-and-trap-state.md) retain graphics-service and
  VInt/DMA/transfer boundaries. This source makes no such request.
- `menus.yes-no-prompt` remains unassociated. Its separate source begins at `0x15284`; adjacency to
  the `rts`-terminated routine establishes no behavior seam.
- `menus.unused-12606` and `menus.unused-156a8` remain unassociated. Similar upstream names do not
  establish shared behavior.
- Text, sound, input, UI presentation, persistence, and hardware timing remain with their existing
  owners or Unknown.

## Evidence Matrix

| Claim | Evidence | Label |
| --- | --- | --- |
| `sub_15268` identity and `0x15268` entry | common-menu H2 fixture | Confirmed static |
| owner-row hash and 22/6/2/0/0/0 counts | accepted H2 generated inventory | Confirmed static |
| exclusive `0x15268..0x15284` interval | pinned source and H1 listing | Confirmed static source |
| isolated write, `0xC` loop setup, thirteen writes, and `rts` order | pinned source and H1 listing | Confirmed static source |
| zero external symbolic caller occurrences | complete accepted source-tree token search | Confirmed bounded inventory |
| “unused menu engine” purpose | upstream path/comment vocabulary only | Inferred |
| indirect reachability, RAM meaning, machine state, timing, visible result | not established by selected owner | Unknown |
| exact one-record future association | H2 join and research-index audit | Confirmed metadata |

## Reproduction

```powershell
uv run sf2 h2 common-menus
uv run sf2 design-contracts test
```

The generated inventory remains under ignored `local/derived/common-menus-static.json`. Private ROM,
H1, complete source-body, emulator, trace, RAM, and presentation artifacts remain outside the tracked
contract.
