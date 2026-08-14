# Graphics Service and State Contract

- **Confirmed original structure:** the bounded decompression entry identities and register ABI,
  display-initialization order, sprite-link initialization shape, palette-transition state, fixed
  flash-script words, special-sprite load/update routing, and nominally unused helper inventory
  described below.
- **Inferred original behavior:** none promoted here. The grouping of these source services into a
  modern graphics boundary is implementation-neutral design synthesis, not evidence of an original
  engine architecture.
- **Unknown original behavior:** rendered frame parity, visible flash duration, VInt or CRAM-DMA
  cadence, hardware timing, final palette appearance, special-sprite frame presentation,
  caller-visible behavior for deliberately malformed/debug/raw-RAM inputs, and runtime meaning of
  nominally unused helpers.
- Remake status: implementation-neutral Phase 3 service/state contract; no renderer API, graphics
  engine, asset format, hardware-emulation target, timing model, presentation policy, or licensed
  content pack has been selected.
- Evidence date: 2026-08-08
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Contract Boundary

This contract defines a narrow compatibility boundary for original graphics services whose
source-static identities, input/output seams, ordering, or state transitions are already accepted.
It owns:

1. the Basic and Stack decompression entry identities plus their shared `a0` source, `a1`
   destination, and `d0` output-byte-count ABI;
2. the accepted display-initialization order without claiming visible-frame or hardware-timing
   parity;
3. the sprite-table link initializer and the existence of separately inventoried battle-sprite link
   helpers;
4. palette-transition timer/step/weight state and its bounded source-visible queue handoff;
5. the exact special-sprite load/update routing split and load-versus-refresh transfer seam, consuming
   canonical pointer/resource identities from
   [special-sprite-graphics-data](special-sprite-graphics-data.md);
6. the fixed three-word flash script;
7. identities and source-use inventory for the nominally unused display/graphics helpers.

It does not own map-camera destination calculation, parallax, autoscroll, compressed asset payloads,
decoded art, map/UI/battle presentation, VDP emulation, DMA scheduling cadence, frame timing,
localization, accessibility, or distributable game assets.

The selected executable owners are:

- `sf2-tech-graphics-static-v1` in
  [`tests/fixtures/h2/tech-graphics-static-v1.json`](../../../tests/fixtures/h2/tech-graphics-static-v1.json),
  implemented by
  [`src/sf2tool/h2/graphics.py`](../../../src/sf2tool/h2/graphics.py);
- `sf2-special-sprite-decode-v1` in
  [`tests/fixtures/h2/special-sprite-decode-v1.json`](../../../tests/fixtures/h2/special-sprite-decode-v1.json),
  implemented by
  [`src/sf2tool/h2/special_sprites.py`](../../../src/sf2tool/h2/special_sprites.py).

The owning research prose is
[Technical Graphics and Decompression Services](../../research/technical-graphics.md). Adjacent
presentation and asset-import contracts retain their own evidence and acceptance surfaces.

## Pre-Contract Evidence Audit

Fresh reproduction passed on the evidence date:

```text
Inventory sf2-tech-graphics-static-v1
SHA256 E915FC30C3E25983BE47A859D3DF2A169E9F53036727FF4F5C3FC5B889209EDC
Files 11
GlobalLabels 209
DirectCallSites 34
IndexedRecords 14
Status PASS

Contract sf2-special-sprite-decode-v1
SHA256 E3DF0CEDBA48E8A5BB30D639868B9CB90C6C4FFA660D8ADA56DAB9969CEEFCA7
Pointers 10
Streams 6
DecodedBytes 16704
FullyRoutedIds 9
Status PASS
```

The audit identified exactly thirteen currently unassociated `tech.graphics.*` records:

- `tech.graphics.animate-special-sprite`;
- `tech.graphics.battle-sprite-links`;
- `tech.graphics.decompression`;
- `tech.graphics.display`;
- `tech.graphics.display-init`;
- `tech.graphics.flash-white`;
- `tech.graphics.palette-transition`;
- `tech.graphics.special-sprite-anims`;
- `tech.graphics.special-sprites`;
- `tech.graphics.sprite-core`;
- `tech.graphics.stack-decompression`;
- `tech.graphics.unused-display`;
- `tech.graphics.unused-helpers`.

Every candidate has at least one of the two selected executable owners. Registration is deferred
until preliminary semantic acceptance.

The audit deliberately consumes only selected fields from the aggregate tech-graphics fixture:

- `graphicsFacts.viewDestination` is excluded in full. The extra indexed record
  `map.camera-control.set-view-destination`, separate plane parallax factors, autoscroll behavior,
  and destination-axis writes remain owned by the accepted map-camera H3 rail and
  [map-exploration contract](map-exploration.md).
- From `graphicsFacts.inventoryBoundary`, this contract consumes
  `unusedDisplayAndGraphicsHelpersInventoried` plus the explicit queues for visual/VDP timing and
  special-sprite frame presentation. It does not consume the battle, map, UI, portrait,
  special-screen, or other asset-corpus completion booleans, and it does not make the fixture's
  historical remaining-corpus queue a remake requirement.
- `stackHistoryBytes`, the initial history words, bitstream grammar, copy-loop details, and codec
  implementation are separate-owner static evidence. They are not H4 fidelity requirements here.
- Special-sprite service and routing claims use the dedicated special-sprite fixture rather than an
  aggregate inventory-completion flag. Static pointer/resource catalog and private-import fidelity
  are delegated to [special-sprite-graphics-data](special-sprite-graphics-data.md).

## Decompression Service ABI

**Confirmed static:** the original exposes Basic and Stack decompression entry identities. Both use
the same bounded call seam:

| Register | Contract role |
| --- | --- |
| `a0` | source address on entry |
| `a1` | destination address on entry |
| `d0` | output byte count on return |

`LoadBasicCompressedData` is H1-bound at ROM address 6,788. The dedicated special-sprite owner binds
the Stack entry used by its accepted corpus at ROM address 7,752. Entry identity and ABI are the
compatibility surface; the remake is not required to reproduce the original history storage,
bitstream parser, copy loop, register allocation, or instruction order.

A private importer may use project-owned decoders to validate original payloads. Public contracts
and fixtures retain metadata, hashes, sizes, and codec statistics rather than compressed or decoded
copyrighted art. Malformed-stream admission, recovery, partial writes, and diagnostics outside the
accepted corpora remain product/importer policy, not reconstructed runtime behavior.

## Display Initialization Order

**Confirmed static:** `InitializeDisplay` is H1-bound at ROM address 12,322 and performs these
accepted order constraints:

1. deactivate contextual VInt functions;
2. wait for VInt;
3. disable display and interrupts;
4. clear the sprite table;
5. configure H32/V32 non-interlaced planes and scroll tables;
6. load a black screen immediately;
7. load sprite masks and the base UI palette.

This is a source-visible initialization plan, not a rendered-frame timeline. The exact VInt boundary,
interrupt latency, VDP register cadence, DMA completion, first visible frame, and hardware-cycle
behavior remain **Unknown** or separate-owner.

The separate `tech.graphics.display` record preserves the `sub_30EE` service identity at ROM address
12,526 and its inventory boundary. It does not import the excluded view-destination facts from the
same source file. The seven numbered groups above are explanatory organization, not an independently
accepted source cardinality.

## Sprite-Link Initialization

**Confirmed static:** `InitializeSprites` is H1-bound at ROM address 6,000. The accepted source shape
uses a `dbf` counter, writes sequential sprite links, and terminates the final link. A compatible
initializer must preserve the resulting ordered link-chain invariant; it need not reproduce the
same loop instruction or register allocation.

The battle-sprite link helpers, represented by the H1-bound `sub_1942` entry at ROM address 6,466,
are completely inventoried as source identities, but this fixture does not assign them a complete
caller-visible state contract. Their precise admission, entity-to-sprite selection, frame
composition, visibility, and timing remain in adjacent battle/presentation owners.

## Palette Transition State

**Confirmed static:** `UpdateBasePalettesAndBackupCurrent` is H1-bound at ROM address 6,600. The
accepted state contract has:

- an initial timer value of 32;
- a blend-step selector derived by dividing the current timer by 4;
- two blend weights whose total is 8;
- one source-visible CRAM-DMA queue handoff on each palette-update invocation;
- an accepted completion branch that can promote the backup palette into the new base palette.

The values `32`, `4`, and `8` are state/weight facts, not a wall-clock duration. The source-visible
queue handoff does not establish VInt cadence, CRAM transfer cadence, hardware timing, dropped or
coalesced updates, rendered colors, or the player's perceived transition.

## Special-Sprite Routing and Transfer Seams

**Confirmed static:** `LoadSpecialSprite` begins at ROM address 154,660 and
`AnimateSpecialSprite` begins at 154,806. The pointer table begins at 154,620. The dedicated fixture
contains ten pointer slots resolving to five initial payloads, plus one animation-only stream, but
ten pointers do not mean ten fully routed map-sprite IDs.

The ten-slot pointer and six-resource catalog is canonicalized by
[special-sprite-graphics-data](special-sprite-graphics-data.md). This service contract consumes those
records for routing and transfer tests; it does not independently own or re-verify the resource
catalog, aliases, palettes, compressed/decoded payloads, sizes, or private import graph.

The accepted routing split is exact:

| Map-sprite IDs | Pointer state | Load/update dispatch state | Contract classification |
| --- | --- | --- | --- |
| `247..255` | backed | backed | 9 fully routed IDs |
| `246` | backed by the Kraken pointer | neither load nor update slot exists | 1 pointer-only ID |
| `240..245` | unbacked | unbacked | 6 unbacked special IDs |

For the nine fully routed IDs, special index 2 is the exploration route and the other accepted slots
use battle handling. Palette 4 is loaded before dispatch. Initial battle/exploration loads use the
immediate-DMA seam, while animation refresh uses the queued-DMA seam. These are operation and routing
facts, not transfer-timing or rendered-frame evidence.

The separately indexed special-sprite animation table identity `table_2784C` is H1-bound at ROM
address 161,868. This contract preserves that identity and its relationship to the special-sprite
service inventory; it does not claim decoded frame order, visible animation timing, or final output
from the table name or address alone.

The accepted [Common Scripting](../../research/common-scripting.md) owner has statically excluded IDs
`237..250` from the complete original built map-sprite assignment domains. This contract does not
consume that owner's fixture or duplicate its H4 surface, but it preserves the result as a
**Separate-owner Confirmed static boundary**. It therefore does not relabel all reserved/special
reachability as Unknown. Deliberately malformed scripts, debug-only assignment, raw-RAM injection,
caller-visible failure behavior, and final presentation for forced values remain **Unknown** or
deliberate test inputs.

## Fixed Flash Script

**Confirmed static:** `ExecuteFlashScreenScript` is H1-bound at ROM address 294,634 and its fixed word
sequence is:

```text
0x0041, 0x001E, 0xFFFF
```

The words and their order are the entire contract here. Their visible duration, update cadence,
palette result, interrupt synchronization, and hardware timing remain **Unknown**. A remake may
implement an accessible or photosensitivity-safe presentation policy, but that is a deliberate
modernization rather than evidence about the original display result.

## Nominally Unused Helper Inventory

**Confirmed static inventory:** the accepted source boundary contains the representative
`tech.graphics.unused-display` entry at ROM address 12,478 and
`tech.graphics.unused-helpers` entry at 6,338. Their source files and identities remain preserved for
provenance and import compatibility.

“Unused” is an upstream/source label and source-use inventory result. It is not proof that the code
is dead under raw-address calls, computed dispatch, debug behavior, modified ROMs, or injected state.
Natural runtime reachability and caller-visible effects remain **Unknown**.

## Implementation-Neutral Service Model

The following is a logical compatibility model, not an engine class hierarchy:

```text
DecompressionPort {
  algorithmIdentity: BASIC | STACK
  sourceAddress
  destinationAddress
  outputByteCount
}

DisplayInitializationPlan {
  orderedActions
}

SpriteLinkInitialization {
  orderedSequentialLinks
  finalLinkTerminated
}

PaletteTransitionState {
  timerInitial: 32
  blendStepDivisor: 4
  blendWeightTotal: 8
  updateQueueHandoff
  promoteBackupAtCompletion
}

SpecialSpriteRoute {
  mapSpriteId
  pointerState: BACKED | UNBACKED
  loadDispatchState: BATTLE | EXPLORATION | ABSENT
  updateDispatchState: BATTLE | EXPLORATION | ABSENT
}

FlashScript {
  orderedWords[3]: [0x0041, 0x001E, 0xFFFF]
}

GraphicsServiceInventoryEntry {
  researchRecordId
  sourceSymbol
  sourcePath
  romAddress
  acceptedRole
}
```

The model deliberately has no view-destination/parallax object and no asset-payload collection.
Canonical special-sprite resource records are consumed from
[special-sprite-graphics-data](special-sprite-graphics-data.md). Original compressed bytes, decoded
art, palettes, tilemaps, rendered frames, and screenshots remain private/generated or separately
licensed data.

## Cross-System Separation

This contract may hand validated decoded byte counts and service events to asset, display, map,
battle, UI, or presentation systems. It does not decide:

- which map-camera destination, parallax factor, or autoscroll state should be selected;
- which copyrighted asset payload is loaded or how it is transformed into distributable content;
- sprite identity, battle animation choice, map entity assignment, or caller admission;
- VDP register emulation, DMA scheduling, frame boundaries, palette appearance, or final composition;
- dialogue, menus, story ordering, persistence, localization, accessibility, or balance;
- malformed-input recovery, debug injection policy, or compatibility behavior outside accepted
  original corpora.

Those surfaces remain separate-owner, **Unknown**, or deliberate product design.

## Fidelity, Modernization, and Copyright Boundary

Compatibility requires stable service identities, accepted register roles, initialization order,
sprite-link invariant, palette-transition state, exact special-sprite routing classification over
canonical data-contract records, transfer seam identity, flash words, and provenance for the
inventoried helper entries.

A remake may replace the original decoders with validated import-time transcoders, use a modern GPU,
batch transfers, change internal palette representation, or implement accessibility-safe presentation.
Such choices must preserve the accepted external facts where compatibility is claimed and report
intentional presentation differences separately.

Original compressed streams, decoded graphics, palettes, tilemaps, screenshots, video captures, and
other game assets are private/generated copyrighted inputs. Do not commit or redistribute them.
Public fixtures and builds must use metadata, hashes, synthetic data, newly authored content, or
properly licensed assets.

## H4 Acceptance Surface

A remake-side graphics adapter can claim this contract only when automated tests prove:

1. Basic and Stack entry identities remain distinguishable and both preserve the `a0` source, `a1`
   destination, and `d0` output-byte-count seam without requiring original codec micro-implementation;
2. the accepted display-initialization actions preserve their documented order, while the seven
   explanatory groups are not treated as an independent source count and rendered timing and
   hardware cadence are tested separately;
3. sprite initialization produces sequential links and a terminated final link without claiming
   unaccepted battle-sprite helper semantics;
4. palette-transition state preserves timer 32, divisor 4, weight total 8, update queue-handoff
   identity, and the bounded completion promotion branch without treating them as wall-clock timing;
5. special-sprite routing consumes canonical records from `special-sprite-graphics-data` and preserves
   exactly nine fully routed IDs `247..255`, pointer-only ID `246`, and unbacked IDs `240..245`, with
   exploration slot 2 distinct from battle routes, without independently re-verifying the static
   resource catalog;
6. initial special-sprite load and animation refresh retain distinct immediate-versus-queued transfer
   seams, while DMA/VInt cadence and visible presentation remain outside parity;
7. flash-script words remain exactly `0x0041, 0x001E, 0xFFFF` without inferring visible duration;
8. the two nominally unused helper records retain identities and provenance without asserting dead
   code or runtime reachability;
9. the camera/view-destination facts, asset-corpus completion facts, codec history/bitstream details,
   copyrighted payloads, and separate-owner built-assignment reachability result are not silently
   absorbed into this contract's fidelity claim;
10. public fixtures and reports contain metadata, identities, counts, ranges, and hashes rather than
    original compressed bytes, decoded art, palettes, tilemaps, or captured frames.

H4 does not require a Mega Drive VDP emulator, instruction-identical decompressor, original asset
format at runtime, or frame-cycle parity unless a later explicit hardware-fidelity decision adds
those requirements.

## Evidence Matrix

| Contract area | Evidence label | Executable owner | Remaining boundary |
| --- | --- | --- | --- |
| decompression entry identities and `a0`/`a1`/`d0` ABI | **Confirmed static** | `sf2-tech-graphics-static-v1` ([`tech-graphics-static-v1.json`](../../../tests/fixtures/h2/tech-graphics-static-v1.json)); Stack entry also bound by `sf2-special-sprite-decode-v1` ([`special-sprite-decode-v1.json`](../../../tests/fixtures/h2/special-sprite-decode-v1.json)) | Codec history, grammar, copy loop, malformed-input behavior, and micro-implementation are outside H4 |
| display initialization, sprite-link shape, palette timer/weights/queue seam, flash words | **Confirmed static** | `sf2-tech-graphics-static-v1` ([`tech-graphics-static-v1.json`](../../../tests/fixtures/h2/tech-graphics-static-v1.json)) | Visible frames, VInt/CRAM-DMA cadence, hardware timing, and presentation remain **Unknown** |
| exact `9 + 1 + 6` special-ID routing split over canonical data records | **Confirmed static** | `sf2-special-sprite-decode-v1` ([`special-sprite-decode-v1.json`](../../../tests/fixtures/h2/special-sprite-decode-v1.json)); catalog owned by [special-sprite-graphics-data](special-sprite-graphics-data.md) | This contract retains route/loader/transfer seams but does not independently own the ten-pointer/six-resource catalog; forced invalid/debug behavior and rendered frames remain **Unknown** |
| complete original built assignment exclusion for IDs `237..250` | **Separate-owner Confirmed static** | [Common Scripting](../../research/common-scripting.md), outside this contract | This contract does not duplicate its fixture or H4 surface; forced malformed/debug/raw-RAM behavior remains **Unknown** |
| camera destination, parallax, autoscroll, and axis writes | **Separate owner** | [map-exploration contract](map-exploration.md) and its H3 camera owner | `graphicsFacts.viewDestination` is explicitly not consumed here |
| nominally unused display/graphics helper identities | **Confirmed static inventory** | `sf2-tech-graphics-static-v1` ([`tech-graphics-static-v1.json`](../../../tests/fixtures/h2/tech-graphics-static-v1.json)) | Dead-code status, runtime reachability, and caller effects remain **Unknown** |
| renderer architecture, accessibility policy, replacement assets, localization, and licensed content | **Deliberate design** | Future product/content decisions | Requires separate provenance and acceptance |

## Reproduction

```powershell
uv run sf2 h2 tech-graphics
uv run sf2 h2 special-sprites
uv run sf2 design-contracts test
uv run sf2 verify
```

Generated detailed outputs remain under ignored `local/derived/tech-graphics-static.json` and
`local/derived/special-sprite-decode.json`.
