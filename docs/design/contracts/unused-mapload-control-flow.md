# Unused Mapload Control-Flow Contract

- Status: **Draft evidence-bound contract**
- Original fidelity: **Confirmed static** for the bounded source identity, entry address, call and
  operand order, loop branch, and directly reviewed internal helper described below
- Modernization: **Allowed** to retain this only as archival compatibility metadata, replace its
  collaborators with engine-native services, or omit a production endpoint unless later evidence
  establishes a required caller
- Unknown: natural or debug reachability, callee effects, loop liveness, RNG results, VDP/VInt and
  camera behavior, timing, presentation, and caller-visible ABI beyond the accepted helper-local
  word save

## Purpose

This contract preserves the smallest remaining source owner in the accepted common-map inventory:
`unused_mapload.asm`. Its indexed entry is `sub_2EC0`. The file requests four random values, stages
eight words, calls a separate display helper, waits for VInt, and then repeats a bounded source-shaped
request sequence while a source-named scrolling bitfield remains nonzero. Its internal `sub_2F24`
helper conditionally increments four source-named plane-scroll-speed words.

The upstream filename and comments call these routines unused map-loading functions. This contract
keeps that vocabulary visible without treating it as proof of dead code, normal gameplay purpose, or
runtime inaccessibility. It records the original control-flow identity so a fidelity adapter can
explain or test the source seam without forcing a modern remake to reproduce the Mega Drive services
behind it.

## Judgment Boundary

**Confirmed static:** [`sf2-common-maps-static-v1`](../../../tests/fixtures/h2/common-maps-static-v1.json)
binds `sub_2EC0` to ROM address `0x2EC0` (`11968`) and identifies `unused_mapload.asm` as its
representative source. The accepted source/H1 boundary places `sub_2EC0` at `0x2EC0..0x2F24` and the
directly reviewed internal helper `sub_2F24` at `0x2F24..0x2F6A`.

The source-shaped top-level order is also Confirmed static: four `GenerateRandomNumber` request
identities and their operands/dataflow; the eight staged `d0..d7` words; the `sub_36B2` call; an
initial `WaitForVInt` call; and a loop containing two ordered `SetVdpReg` request words, another
`WaitForVInt`, `sub_2F24`, and the scrolling-bitfield branch. The helper's four ordered word updates,
signed `bgt` decision, and balanced `d0.w` stack save are direct source facts.

These claims identify original instructions and request order. They do not confirm that any callee
accepted, completed, or visibly realized a request.

**Inferred:** the source filename, comments, and inventory label classify the routines as unused and
randomized map-loading code. That vocabulary suggests a development or experimental origin, but it
does not establish intent, a complete map-load operation, or any player-facing function.

**Unknown or excluded:** natural, debug, indirect, table-driven, or raw-address reachability; caller
admission and input state; RNG distribution, seed use, and returned values; behavior of `sub_36B2`;
VDP command transport or acceptance; VInt cadence or completion; who changes the tested bitfield;
loop termination; camera and scrolling results; signed overflow states; MMIO, interrupts, DMA,
rendering, timing, persistence, and presentation; top-level return values; stack, register, and CCR
behavior outside the exact helper-local `d0.w` save; and whether a remake needs a callable runtime
endpoint.

## Evidence Contract

This contract consumes only these fields and identities from
[`sf2-common-maps-static-v1`](../../../tests/fixtures/h2/common-maps-static-v1.json):

- `function.unusedMaploadAddress`;
- `expected.representativeSymbols["unused_mapload.asm"]`;
- `expected.mapFacts.inventoryBoundary.unusedRandomMaploadInventoried`;
- `upstreamCommit` and `romSha256` provenance;
- the source-path membership and accepted digest-bound owner row for
  `code/common/maps/unused_mapload.asm`.

The contract explicitly does **not** consume:

- `expected.mapFacts.mapSwitch`;
- `expected.mapFacts.battleTrigger`;
- `expected.mapFacts.egress`;
- `expected.mapFacts.mapLayout`;
- `expected.mapFacts.vint`;
- `cameraStateMachineInventoried` or `cameraAndVdpTimingRemainQueued` from the inventory boundary;
- any H3 camera, map, interrupt, graphics, or presentation fixture.

The owning [common-map research](../../research/common-maps.md), executable
[`maps.py`](../../../src/sf2tool/h2/maps.py), and extraction
[`manifest`](../../../manifests/extractions/common-maps-static.json) retain the complete seven-file
inventory and accepted output digest. The digest-bound generated row for the selected source records:

| Inventory field | Accepted value |
| --- | ---: |
| source SHA-256 | `6852E300E9705C57A77456FA5CE028686493AF0E4D592B644FE073FEC40C2C55` |
| source lines | `86` |
| parsed statements | `51` |
| global labels | `7` |
| local labels | `0` |
| outgoing direct call sites | `10` |
| distinct direct call targets | `5` |
| indirect call sites | `0` |

The ten outgoing sites are four requests to `GenerateRandomNumber`, two to `SetVdpReg`, two to
`WaitForVInt`, one to `sub_36B2`, and one to `sub_2F24`. These are outgoing-call counts. They do not
count incoming callers and do not establish that `sub_2EC0` is unreachable.

The bounded source shape is reviewed directly in pinned
[`unused_mapload.asm`](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/maps/unused_mapload.asm).
The H1 listing supplies the entry and exclusive-end boundaries. This contract does not claim
byte-for-byte instruction-body parity from the H2 fixture.

### Exact research-index denominator

The fixture's source-membership surface contains eight records across seven source paths. Six carry
direct `sf2-common-maps-static-v1` evidence; two are membership-only rows whose executable evidence
belongs elsewhere.

| Record | Relation to this fixture | Design ownership after this contract |
| --- | --- | --- |
| `maps.unused-mapload` | direct H2 binding | this contract; currently unassociated before registration |
| `maps.camera` | direct H2 binding | remains unassociated and outside this contract |
| `maps.animations` | direct H2 binding | unchanged: `map-exploration` |
| `maps.switch-map` | direct H2 binding | unchanged: `map-entry-routing-state` |
| `maps.battle-trigger` | direct H2 binding | unchanged: `map-entry-routing-state` |
| `maps.savepoint` | direct H2 binding | unchanged: `map-entry-routing-state` |
| `map.camera-control.wait-for-view-scroll-end` | source membership only | unchanged: `map-exploration`; dedicated H3 owner |
| `maps.map-layout` | source membership only | unchanged: `map-layout-data`; dedicated layout owner |

The future semantic association is exactly `maps.unused-mapload`. No helper, RNG, VDP, VInt,
camera, map-layout, map-routing, or aggregate map-data record gains this contract.

## Source-Static Control Flow

### Random request and staging prefix

`sub_2EC0` performs these source operations in order:

| Step | Source request or operation | Staged result identity |
| ---: | --- | --- |
| 1 | set `d6.w = 0x20`; call `GenerateRandomNumber` | copy returned `d7.w` to `d0.w` |
| 2 | set `d6.w = 4`; call `GenerateRandomNumber` | copy returned `d7.w` to `d1.w`, then add `0x1C` |
| 3 | set `d6.w = 0x10`; call `GenerateRandomNumber` | copy returned `d7.w` to `d2.w` |
| 4 | set `d6.w = 4`; call `GenerateRandomNumber` | copy returned `d7.w` to `d3.w` |
| 5 | write `4` to each of `d4.w`, `d5.w`, `d6.w`, and `d7.w` | eight staged words are now in `d0..d7` |
| 6 | call `sub_36B2` | separate-owner display-helper handoff |
| 7 | call `WaitForVInt` | separate-owner wait handoff |

The constants and dataflow are call-site facts. This contract does not assert that
`GenerateRandomNumber` returns any particular range or distribution, that `sub_36B2` consumes the
registers as inferred from its name, or that the wait completes a frame or display operation.

### Source-shaped loop

After the prefix, the source enters `loc_2F04`. Every iteration has this order:

1. write request word `0x8721` to `d0.w` and call `SetVdpReg`;
2. write request word `0x8700` to `d0.w` and call `SetVdpReg`;
3. call `WaitForVInt`;
4. call the internal helper `sub_2F24`;
5. test the source-named `VIEW_SCROLLING_PLANES_BITFIELD` byte;
6. branch back to step 1 when that byte is nonzero, otherwise execute `rts`.

This is post-helper test order: the body runs at least once after the initial prefix wait whenever
control reaches `loc_2F04`. The contract does not reinterpret the two request words as successful
VDP writes, define who clears the bitfield, or guarantee loop termination.

### Internal four-word helper

`sub_2F24` first pushes `d0.w`. It then processes these source-named words in exact order:

1. `PLANE_A_SCROLL_SPEED_X`;
2. `PLANE_A_SCROLL_SPEED_Y`;
3. `PLANE_B_SCROLL_SPEED_X`;
4. `PLANE_B_SCROLL_SPEED_Y`.

For each word, the helper loads it into `d0.w`, adds one, compares the word result with `128`, and
uses signed `bgt` to skip the store when the result is signed-greater than `128`. Otherwise it writes
the result back. Finally it pops `d0.w` and returns.

That rule is intentionally not summarized as an unconditional saturating increment. For ordinary
bounded values, `127` stores `128`, while a current value of `128` computes `129` and leaves the
stored word unchanged. Negative inputs, word overflow, and states outside such bounded synthetic
checks retain their exact source branch shape but no accepted runtime meaning.

The helper proves one balanced two-byte stack slot and restoration of the caller's `d0.w` value. It
does not prove CCR neutrality, all-register preservation, interrupt safety, or any top-level ABI.

## Cross-System Separation

- [`randomness`](randomness.md) owns the accepted RNG algorithm and runtime matrices. This contract
  owns only the four call-site request operands and result-staging order.
- [`graphics-service-state`](graphics-service-state.md) and the owning
  [technical-graphics research](../../research/technical-graphics.md) retain the display helper,
  VDP-register service, and graphics-effect evidence. Their effects are not imported here.
- [`interrupt-dma-and-trap-state`](interrupt-dma-and-trap-state.md) owns the accepted
  `WaitForVInt`/VInt handshake and DMA/interrupt boundaries. A source call identity here does not
  duplicate that contract.
- [`map-exploration`](map-exploration.md) owns camera/scroll state, map VInt, and runtime/presentation
  seams. This contract does not assign meaning to the four speed words or tested bitfield.
- [`map-entry-routing-state`](map-entry-routing-state.md) owns switch, battle-trigger, and savepoint
  selection. [`map-layout-data`](map-layout-data.md) owns the static decoded layout corpus.
- `sf2-map-data-static-v1`, map records, collision, entity state, story state, and visible map content
  are excluded.

The source label `sub_36B2` and raw RAM symbols are trace anchors, not newly owned service or state
records.

## Implementation-Neutral Model

A private fidelity/import layer may represent the accepted surface as:

```text
UnusedMaploadControlFlow {
  identity {
    fixtureId
    sourcePath
    sourceSha256
    entryAddress
    helperAddress
    exclusiveEndAddress
    upstreamCommit
    romSha256
  }
  randomRequests[4] {
    orderedBoundOperand
    orderedDestinationWord
    postAddend
  }
  stagedConstantWords[4]
  prefixHandoffs[2]
  loop {
    orderedVdpRequestWords[2]
    waitHandoff
    helperHandoff
    postHelperBitfieldTest
    repeatOnNonzero
  }
  speedHelper {
    orderedWordIdentities[4]
    addend
    signedThreshold
    storeWhenNotSignedGreater
    savedWordIdentity
  }
}
```

The public contract may retain the bounded source path and symbol, selected H1 addresses, constants,
call-count summary, source hash, branch/dataflow rules, fixture digest, and provenance named above.
Complete source/H1/ROM bodies, instruction bytes, and other non-public verification material are not
part of the public projection. The pinned upstream link is provenance, not copied source ownership.

After verifying the pinned-source chronology and H1 entry/boundary identities under the fixture's
accepted ROM provenance, a remake may represent the abstract requests with engine-native callbacks,
typed state, or a trace-only archival adapter. This does not claim instruction-body parity. The
remake is not required to reproduce Mega Drive addresses, the 68000 register file, word-stack
mechanics, VDP registers, a hardware interrupt loop, or the original instruction sequence in
production code.

## Fidelity and Modernization

Original-fidelity evidence requires preserving these distinctions:

- the indexed `sub_2EC0` entry versus the directly reviewed `sub_2F24` helper;
- four ordered RNG request operands versus any RNG result semantics;
- staged source registers versus callee effects;
- the one initial wait versus waits inside the post-helper-tested loop;
- two ordered VDP request words versus transport or rendering success;
- four ordered speed-word operations versus a universal clamp abstraction;
- one helper-local `d0.w` save versus a general ABI promise;
- source vocabulary versus runtime reachability.

A modern engine MAY omit a production endpoint, inline it into a private compatibility test, or use
injected services. If it retains a compatibility seam, synthetic traces can cover:

- a first bitfield observation of zero: the prefix requests, one loop body, then return;
- a nonzero observation followed by zero: the same prefix and two ordered loop bodies;
- helper inputs `127` and `128`: store `128` for the first and leave the second stored word at `128`;
- restoration of the supplied logical `d0.w` value after helper completion.

Those are modernization tests over accepted source relations. They are not observations of original
runtime reachability, elapsed frames, visible output, or natural state distributions.

## H4 Acceptance Checklist

1. Preserve the field-closed fixture identity, entry address, source provenance, and accepted owner
   row without consuming sibling map fact subtrees.
2. Preserve the four RNG request operands and `d0..d3` result-staging order, followed by `d4..d7 =
   4`, as source-static call-site facts only.
3. Preserve `sub_36B2` then initial `WaitForVInt`, followed by the exact two-request/wait/helper/
   post-helper-test loop order.
4. Preserve the internal helper's four word identities, add-one operation, signed compare with 128,
   conditional store, and exact `d0.w` save/restore boundary without claiming universal saturation or
   CCR/all-register neutrality.
5. Do not convert call identities into RNG, display, VDP, VInt, camera, transfer-completion, or
   presentation claims.
6. Keep natural reachability, liveness, malformed state, overflow meaning, timing, and production API
   necessity Unknown unless stronger accepted evidence closes them.
7. Keep the complete source/H1/ROM bodies and instruction encodings outside the public projection;
   expose only the bounded metadata and provenance listed by this contract.
8. Register only `maps.unused-mapload`; keep `maps.camera`, both membership-only rows, every existing
   sibling association, and every separate service record unchanged.

## Evidence Matrix

| Claim | Evidence | Label |
| --- | --- | --- |
| `sub_2EC0` identity and `0x2EC0` entry | common-map H2 fixture and H1 listing | Confirmed static |
| `sub_2EC0..sub_2F24..0x2F6A` boundaries | pinned source and H1 listing | Confirmed static source |
| 86/51/7/0 and 10-call owner-row inventory | H2 generated owner row under accepted digest | Confirmed static |
| four RNG request operands and register staging | pinned `unused_mapload.asm` | Confirmed static source |
| display/wait/VDP/helper/bitfield loop order | pinned `unused_mapload.asm` | Confirmed static source |
| four ordered helper words, signed branch, `d0.w` save | pinned source and H1 listing | Confirmed static source |
| “unused/randomized mapload” meaning | upstream vocabulary only | Inferred |
| callee effects, runtime reachability, liveness, timing, visible result | not established by selected owner | Unknown |
| exact one-record association boundary | research-index and fixture membership audit | Confirmed metadata |

## Reproduction

```powershell
uv run sf2 h2 common-maps
uv run sf2 design-contracts test
uv run sf2 research-index test
```

The generated inventory remains under ignored `local/derived/common-maps-static.json`. Private ROM,
H1, source-body, emulator, trace, and captured presentation materials remain outside the tracked
contract.
