# Map Camera Update Control-Flow Contract

- Status: **Draft evidence-bound contract**
- Original fidelity: **Confirmed static** for the bounded `VInt_UpdateViewData` identity, signed
  branch topology, source-order destination handoff, speed-selection precedence, and four
  word-width parallax update paths described below
- Modernization: **Allowed** to use engine-native camera state and scheduling while preserving an
  equivalent abstract decision/update trace over admitted compatibility inputs
- Unknown: natural caller admission, runtime target domains, callee effects, register/CCR ABI,
  interrupt cadence, scroll trajectory, VDP-visible output, frame timing, and presentation

## Purpose

This contract preserves the remaining unassociated function in the accepted common-map inventory:
`VInt_UpdateViewData` in `camerafunctions.asm`. The source first chooses whether to run an
entity-target adjustment path, conditionally requests a new view destination, then derives four
plane-axis scroll-speed words through an ordered override and per-axis gate sequence.

The function name and source comments suggest camera/view-target follow behavior. That vocabulary is
not evidence of player-visible intent, a complete camera subsystem, or the cadence at which a
particular callback list executes it. The contract owns only the bounded source-static update
algorithm. Camera commands, area data, destination-service behavior, callback registration, and
rendered presentation remain with their existing owners.

## Judgment Boundary

**Confirmed static:** [`sf2-common-maps-static-v1`](../../../tests/fixtures/h2/common-maps-static-v1.json)
binds `VInt_UpdateViewData` to ROM address `0x45C2` (`17858`) and identifies
`camerafunctions.asm` as its representative source. The H1 listing places the function in the
exclusive interval `0x45C2..0x4708` (`326` bytes). The separate `WaitForViewScrollEnd` entry begins
at `0x4708`; its loop is not part of this contract.

Direct review of the pinned source confirms the signed target branch, signed word-comparison
branches, destination-request order, counter update, speed-value precedence, four autoscroll gates,
and exact `move.w`/`mulu.w`/`lsr.w`/`move.w` width sequence. Within the bounded function there is one
direct call to `IsMapScrollingToViewTarget` and one direct call to `SetViewDestination`.

These are source operations and branch identities. They do not confirm callee meaning, request
acceptance, destination completion, scroll motion, VInt frequency, or visible output.

**Inferred:** the source identity and comments suggest an automatic camera-target update and
scroll-speed preparation role. Foreground/background visual meaning, viewport intent, and
player-facing behavior are not Confirmed by the selected owner.

**Unknown or excluded:** the naturally admitted `VIEW_TARGET_ENTITY` and entity-record domains;
whether signed-negative bytes represent one or multiple sentinel classes; caller setup and
reachability; meaning of `word_FFA828` beyond its exact source dataflow; word overflow and malformed
state; register, stack, and CCR guarantees; runtime effects of `IsMapScrollingToViewTarget` and
`SetViewDestination`; command-handler and wait behavior; callback activation and ordering; interrupt
atomicity; VDP state; rendered trajectory; frame cadence; persistence; presentation; and
debug/raw-RAM injection behavior.

## Evidence Contract

This contract consumes only these fields and identities from
[`sf2-common-maps-static-v1`](../../../tests/fixtures/h2/common-maps-static-v1.json):

- `function.cameraAddress`;
- `expected.representativeSymbols["camerafunctions.asm"]`;
- `expected.mapFacts.inventoryBoundary.cameraStateMachineInventoried`;
- `expected.mapFacts.inventoryBoundary.cameraAndVdpTimingRemainQueued`;
- `upstreamCommit` and `romSha256` provenance;
- the source-path membership relation for `code/common/maps/camerafunctions.asm`.

The contract explicitly does **not** consume:

- `expected.mapFacts.mapSwitch`, `battleTrigger`, `egress`, `mapLayout`, or `vint`;
- `unusedRandomMaploadInventoried`;
- `sf2-tech-graphics-static-v1.graphicsFacts.viewDestination`;
- any H3 camera, map, interrupt, graphics, or presentation fixture;
- `sf2-map-data-static-v1` or any aggregate map-data record.

The owning [common-map research](../../research/common-maps.md), executable
[`maps.py`](../../../src/sf2tool/h2/maps.py), and extraction
[`manifest`](../../../manifests/extractions/common-maps-static.json) retain the complete seven-file
inventory and accepted digest. The generated inventory row hashes the complete two-function source
file, so its file-wide call counts must not be mistaken for this function's bounded two-call surface.

The bounded source shape is reviewed directly in pinned
[`camerafunctions.asm`](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/maps/camerafunctions.asm).
The H1 listing supplies the entry and exclusive-end identities. The current H2 owner does not prove
byte-for-byte instruction-body parity against H1 and ROM, and this contract does not claim it.

### Exact research-index denominator

The fixture's source-membership surface contains eight records across seven source paths. Six carry
direct `sf2-common-maps-static-v1` evidence; two are membership-only rows whose executable evidence
belongs elsewhere.

| Record | Relation to this fixture | Design ownership after this contract |
| --- | --- | --- |
| `maps.camera` | direct H2 binding | this contract; currently unassociated before registration |
| `maps.animations` | direct H2 binding | unchanged: `map-exploration` |
| `maps.switch-map` | direct H2 binding | unchanged: `map-entry-routing-state` |
| `maps.battle-trigger` | direct H2 binding | unchanged: `map-entry-routing-state` |
| `maps.savepoint` | direct H2 binding | unchanged: `map-entry-routing-state` |
| `maps.unused-mapload` | direct H2 binding | unchanged: `unused-mapload-control-flow` |
| `map.camera-control.wait-for-view-scroll-end` | source membership only | unchanged: `map-exploration`; dedicated H3 owner |
| `maps.map-layout` | source membership only | unchanged: `map-layout-data`; dedicated layout owner |

The future semantic association is exactly `maps.camera`. No wait, destination-service, entity,
area-data, VInt-dispatch, map-animation, graphics-service, map-routing, or map-data record gains this
contract.

## Source-Static Update Flow

### Signed target branch and plane-word selection

The function clears `d0.w`, loads the raw `VIEW_TARGET_ENTITY` byte into `d0.b`, and executes
`bmi.w loc_468C`. When that signed-negative branch is taken, control goes directly to speed
derivation. It bypasses all entity reads, the `IsMapScrollingToViewTarget` call, every
`SetViewDestination` path, and the no-adjust `clr.w word_FFA828` path. The existing
`word_FFA828` value is therefore left unchanged before speed selection.

For a nonnegative target byte, the source shifts the word by `ENTITYDEF_SIZE_BITS` (`5`), adds that
offset to `ENTITY_DATA`, and reads two ordered words into `d4` and `d5`. The source comments call
those words entity X and Y. The function then selects the current `d2`/`d3` pair by testing
`MAP_AREA_LAYER_TYPE`: zero reads View Plane B pixel words; nonzero reads View Plane A pixel words.
This is raw source selection vocabulary, not a rendered foreground/background contract.

After clearing `d6.w`, the function calls `IsMapScrollingToViewTarget`. Its `bne.w return_4706`
branch returns immediately on a nonzero condition, before destination adjustment, counter mutation,
speed selection, or any of the four speed-word writes. This contract preserves that branch result
only; it does not assign semantics to the callee.

### Exact signed threshold and bound branches

All comparisons below are word comparisons followed by signed `bge` or `ble` branches. The decimal
constants and `MAP_TILE_SIZE = 384` remain original internal source units. They are not declared
screen pixels, tile dimensions, viewport sizes, or a safe domain for arbitrary inputs.

| Axis path | Exact compare and signed branch | Operation only when the branch does not skip it |
| --- | --- | --- |
| X first threshold | compare `d4` with `d2 + 1536`; `bge.s loc_4616` | continue the lower-side candidate |
| X lower bound | compare `d2` with `MAP_AREA_LAYER1_STARTX`; `ble.w loc_4638` | subtract `MAP_TILE_SIZE` and increment `d6.w` |
| X second threshold | compare `d4` with `d2 + 2304`; `ble.s loc_4638` | continue the upper-side candidate |
| X upper bound | compare `d2` with `MAP_AREA_LAYER1_ENDX - 3840`; `bge.w loc_4638` | add `MAP_TILE_SIZE` and increment `d6.w` |
| Y first threshold | compare `d5` with `d3 + 1536`; `bge.s loc_4654` | continue the lower-side candidate |
| Y lower bound | compare `d3` with `MAP_AREA_LAYER1_STARTY`; `ble.w loc_4676` | subtract `MAP_TILE_SIZE` and increment `d6.w` |
| Y second threshold | compare `d5` with `d3 + 2304`; `ble.s loc_4676` | continue the upper-side candidate |
| Y upper bound | compare `d3` with `MAP_AREA_LAYER1_ENDY - 3456`; `bge.w loc_4676` | add `MAP_TILE_SIZE` and increment `d6.w` |

The table preserves source branch polarity rather than replacing it with an unsigned or
implementation-chosen geometric predicate. Word overflow, cross-sign comparisons, invalid entity
indexes, and malformed bounds retain no accepted runtime meaning.

### Destination handoff and counter paths

After both axes converge, the function tests `d6.w`:

1. nonzero copies `d2.w`/`d3.w` to `d0.w`/`d1.w`, calls `SetViewDestination`, increments
   `word_FFA828` once, and branches to speed derivation;
2. zero executes `clr.w word_FFA828` and falls through to speed derivation.

The counter increments once per destination handoff, not once per adjusted axis. The earlier
signed-negative target branch reaches speed derivation without taking either counter path, while the
nonzero `IsMapScrollingToViewTarget` branch returns before speed derivation. These three routes must
remain distinct.

The `SetViewDestination` call is a handoff identity. Destination-axis calculation, downstream state
writes, runtime command behavior, and visible scroll remain owned by
[`map-exploration`](map-exploration.md) and its accepted camera evidence.

## Speed Selection and Four Width-Bounded Writes

Speed selection is source-ordered, and later applicable values replace earlier ones:

1. load `word_FFA828`; signed `cmpi.w #6` plus `ble.s` selects `24`, otherwise select `32`;
2. raw target byte equal to `ENTITY_CURSOR` (`0x30`) replaces the value with `64`;
3. `FADING_SETTING` equal to `PULSATING_1` (`5`) replaces it with `32`;
4. nonzero `VIEW_SCROLLING_SPEED` replaces it with that stored word.

The contract preserves this precedence without assigning physical speed, frame-rate, or
presentation meaning to the values.

The source then processes these four axes in exact order:

1. Layer 1 X -> `PLANE_A_SCROLL_SPEED_X`;
2. Layer 1 Y -> `PLANE_A_SCROLL_SPEED_Y`;
3. Layer 2 X -> `PLANE_B_SCROLL_SPEED_X`;
4. Layer 2 Y -> `PLANE_B_SCROLL_SPEED_Y`.

For each axis, `tst.b` on its autoscroll byte is followed by `bne` to the next axis or terminal. A
nonzero byte therefore preserves the existing speed word. Only the zero path performs this exact
width sequence:

1. `move.w d7,d0`;
2. unsigned `mulu.w` by the corresponding parallax word, producing a product in `d0.l`;
3. `lsr.w #BYTE_SHIFT_COUNT,d0`, shifting only the low word of that product by eight;
4. `move.w d0` to the corresponding speed word.

The high product word is not shifted into the stored result. This must not be normalized into a
generic full-32-bit `(speed * parallax) >> 8` formula. A compatibility model either preserves the
instruction-width sequence or proves an exactly equivalent low-word result for its admitted input
domain.

## Cross-System Separation

- [`map-exploration`](map-exploration.md) retains camera command records, H3 target/destination/speed
  observations, `SetCameraDestination`, `SetViewDestination` behavior, `WaitForViewScrollEnd`, area
  parallax/autoscroll inputs, and map lifecycle. This contract owns only the bounded update function.
- [`graphics-service-state`](graphics-service-state.md) continues to exclude
  `graphicsFacts.viewDestination`; it delegates this source-static update algorithm here while
  retaining its graphics-service responsibilities.
- [`interrupt-dma-and-trap-state`](interrupt-dma-and-trap-state.md) owns callback registration,
  VInt/interrupt transport, DMA seams, and timing boundaries. The `VInt_` source prefix does not
  duplicate that contract.
- [`map-entity-data`](map-entity-data.md) and the entity-state owners retain record identity,
  population, and runtime state. The two word reads here do not create a second entity schema.
- [`map-layout-data`](map-layout-data.md), [`map-entry-routing-state`](map-entry-routing-state.md),
  [`unused-mapload-control-flow`](unused-mapload-control-flow.md), and the map-animation owner retain
  their existing data and control-flow records.
- Renderer composition, accessibility policy, replacement content, presentation, and hardware
  fidelity remain future deliberate design or **Unknown**.

## Implementation-Neutral Model

A private compatibility layer may represent the accepted surface as:

```text
MapCameraUpdateControlFlow {
  identity {
    fixtureId
    sourcePath
    sourceSymbol
    entryAddress
    exclusiveEndAddress
    upstreamCommit
    romSha256
  }
  targetRoute {
    signedNegativeBranchTarget
    negativeLeavesCounterUnchanged
    nonnegativeEntityOffsetShift
    orderedCoordinateWordReads[2]
    layerTypePlaneWordSelection
    scrollingNonzeroEarlyReturn
  }
  signedAdjustmentBranches[8] {
    comparisonWidth
    branchCondition
    branchTarget
    sourceUnitOperand
    adjustmentWhenNotSkipped
  }
  destinationRoute {
    adjustmentCountWord
    handoffOnNonzero
    incrementCounterOnce
    clearCounterOnZero
  }
  speedPrecedence[4]
  axisUpdates[4] {
    autoscrollByteGate
    speedWordInput
    parallaxWordInput
    unsignedWordMultiply
    lowWordLogicalShiftBy8
    wordStore
  }
}
```

The public contract may retain bounded source path/symbol, selected H1 addresses, raw constants,
branch polarities, ordered operation identities, fixture digest, and provenance. Complete source,
H1, or ROM instruction bodies, full encodings, and other non-public verification material remain
private or optional future evidence.

After verifying the pinned-source chronology and H1 entry/boundary under accepted ROM provenance, a
remake may use typed engine-native state, references, and callbacks. It need not reproduce Mega Drive
addresses, the 68000 register file, callback tables, VInt micro-scheduling, or the original
instruction sequence. Compatibility is measured at the abstract branch/update trace and admitted
word-result boundary, not at visible frames or instruction bytes.

## Fidelity and Modernization

Original-fidelity evidence requires preserving these distinctions:

- signed-negative target bypass versus nonnegative entity processing;
- negative-target counter preservation versus no-adjust clear versus adjustment increment;
- nonzero scrolling-result early return versus speed-derivation routes;
- eight exact signed word branch identities versus unsigned or geometry-normalized tests;
- destination handoff identity versus downstream destination behavior;
- four ordered speed overrides versus a chosen physical speed model;
- autoscroll nonzero preservation versus zero-path recalculation;
- `mulu.w` product followed by low-word-only `lsr.w` and `move.w` versus full-product scaling;
- source-static callback code versus VInt cadence and rendered presentation.

A modern engine may express the same admitted trace through ordinary camera-system code. Synthetic
compatibility cases should cover at least:

- a signed-negative target with a seeded `word_FFA828`, proving direct speed derivation and no
  destination/counter-clear path;
- a nonnegative target whose scrolling check branches to the terminal before all later writes;
- taken and not-taken sides of every `bge`/`ble` threshold and bound identity without reinterpreting
  them as unsigned comparisons;
- no-adjust clear and one-or-two-axis adjustment with one destination handoff/counter increment;
- every applicable speed override precedence edge;
- each autoscroll gate and a product whose high word would distinguish low-word shifting from a
  full-32-bit shift.

Those are compatibility tests over accepted source relations, not observations of natural runtime
states, elapsed frames, or visible camera motion.

## H4 Acceptance Checklist

1. Preserve the field-closed fixture identity, `0x45C2..0x4708` function boundary, source
   provenance, and accepted owner relation without consuming sibling map-fact subtrees.
2. Preserve the signed-negative target branch directly to speed derivation, including bypass of
   `SetViewDestination` and the no-adjust counter clear, leaving `word_FFA828` unchanged.
3. Preserve the nonnegative target's ordered entity/plane reads and nonzero scrolling-result early
   return without importing entity schema, callee meaning, or visible camera behavior.
4. Preserve all eight word comparison operands and exact signed `bge`/`ble` branch polarities,
   source-unit constants, and adjustment operations without unsigned/geometric normalization.
5. Preserve distinct adjustment-nonzero handoff/increment and adjustment-zero clear paths; the
   counter increments once per handoff.
6. Preserve speed precedence `24/32 -> cursor 64 -> pulsating 32 -> nonzero explicit speed` as
   source dataflow, not timing or physical speed.
7. Preserve all four ordered autoscroll gates and the exact `move.w`, unsigned `mulu.w`, low-word
   `lsr.w #8`, `move.w` store sequence; do not substitute a generic full-product formula unless
   result equivalence is proven for the admitted compatibility domain.
8. Keep commands/H3, destination-service behavior, area inputs, VInt scheduling, hardware effects,
   presentation, malformed state, and runtime reachability with their separate owners or Unknown.
9. Keep complete instruction bodies and encodings outside the public projection; expose only the
   bounded metadata, operation relations, and provenance listed here.
10. Register only `maps.camera`; keep the five existing direct sibling associations, two
    membership-only owners, and every other research-index object unchanged.

## Evidence Matrix

| Claim | Evidence | Label |
| --- | --- | --- |
| `VInt_UpdateViewData` identity and `0x45C2` entry | common-map H2 fixture and H1 listing | Confirmed static |
| `0x45C2..0x4708` exclusive interval and separate wait entry | pinned source and H1 listing | Confirmed static source |
| signed target route and distinct counter paths | pinned `camerafunctions.asm` | Confirmed static source |
| eight signed threshold/bound branch identities | pinned source and H1 listing | Confirmed static source |
| speed-value precedence and four autoscroll gates | pinned `camerafunctions.asm` | Confirmed static source |
| word multiply, low-word shift, and word-store widths | pinned source and H1 listing | Confirmed static source |
| automatic camera-follow/player-visible meaning | source vocabulary only | Inferred |
| callee effects, runtime reachability, VInt cadence, trajectory, visible result | not established by selected owner | Unknown |
| exact one-record association boundary | research-index and fixture membership audit | Confirmed metadata |

## Reproduction

```powershell
uv run sf2 h2 common-maps
uv run sf2 design-contracts test
uv run sf2 research-index test
```

The generated inventory remains under ignored `local/derived/common-maps-static.json`. Private ROM,
H1, complete source-body, emulator, trace, and captured presentation materials remain outside the
tracked contract.
