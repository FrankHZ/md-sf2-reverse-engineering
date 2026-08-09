# Special-Screen Control-Flow Contract

- **Confirmed original structure:** the bounded Sega-logo, title, witch-entry/menu, suspend, and
  ending control facts, representative function identities, source-static counters, and dispatch
  seams described below.
- **Inferred original behavior:** none promoted here.
- **Unknown original behavior:** normal player-driven reachability, controller cadence, wall-clock
  or visible duration, fades, VInt/DMA/CRAM timing, rendered pixels, audio timing, cross-process save
  persistence, recovery after interruption, alternate-build behavior, localization, accessibility,
  and player-facing meaning.
- Remake status: implementation-neutral Phase 3 compatibility contract; no screen framework,
  renderer, input model, save backend, replacement presentation, or distribution license has been
  selected.
- Evidence date: 2026-08-09
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Contract Boundary

This contract defines a bounded static control surface for five special-screen families:

1. Sega-logo checksum, configuration/debug-sequence ownership, and Start-early-exit structure;
2. title entry, two scroll-loop identities, bounded Start polling, exit choice, and title-font loader
   identity;
3. witch SRAM-result admission, top-level action availability/cancel/dispatch, and generic
   four-page menu navigation;
4. suspend source counters, Start-early-exit branch, and reset-vector handoff;
5. ending-kiss pixel-fill and ending-witch falling-jewel/control ownership identities.

The executable owner is `sf2-special-screens-static-v1` in
[`tests/fixtures/h2/special-screens-static-v1.json`](../../../tests/fixtures/h2/special-screens-static-v1.json).
The research owner is [Special Screens](../../research/special-screens.md), which records the pinned
source/H1 review and explicitly separates static control flow from runtime presentation.

This contract does not own original graphics bytes, palettes, layouts, decompression, transfer
execution, SRAM mutation semantics, per-action save selectors and service calls, new-game lifecycle,
player-driven input, or visible presentation. Those are separate contracts or remain **Unknown**.

## Pre-Contract Evidence Audit

The dedicated H2 owner was reproduced from current `main` on the evidence date:

```text
sf2-special-screens-static-v1
SHA256 56EA039B129BDB3F684513922357DEEA119F19BCEC5309B1753FD137FCD2CBD1
Files 19 / ScreenGroups 7 / Resources 18 / RuntimeQuestions 3 / PASS
```

Three denominators must remain distinct.

### Source-root membership: 21 records

The fixture's accepted `expected.indexedRecordIds` membership corpus contains 21 records. Nineteen
have a direct `sf2-special-screens-static-v1` research-index evidence binding. Two membership records
intentionally use a more specific executable owner instead:

- `screens.title.compressed-tiles` is owned by `sf2-special-screen-graphics-decode-v1` and
  [special-screen-asset-data](special-screen-asset-data.md);
- `screens.witch.new-game-lifecycle` is owned by
  `sf2-witch-new-game-lifecycle-runtime-v1` and [save-system](save-system.md).

They remain unchanged and are not evidence dependencies or association targets of this contract.

### Direct fixture bindings: 19 records

The 19 direct research-index bindings divide exactly into three groups.

Twelve control records are currently unassociated and form the new-contract candidate set:

- `screens.endkiss.engine`;
- `screens.segalogo.debug-cheat`;
- `screens.segalogo.engine`;
- `screens.suspend.engine`;
- `screens.suspend.witch`;
- `screens.title.engine`;
- `screens.title.font`;
- `screens.witch.functions`;
- `screens.witch.sound-test`;
- `screens.witch.start`;
- `screens.witchend.engine`;
- `screens.witchend.init`.

`screens.witch.menu` already belongs to
[special-screen-asset-data](special-screen-asset-data.md) through the dedicated witch graphics
owner. It is the single intentional overlap: that asset contract retains palette/frame data, while
this contract retains only the `ExecuteWitchMainMenu` control/page/navigation seam.

The remaining six direct bindings are resource records whose existing asset association must remain
semantically unchanged:

- `screens.endkiss.resources`;
- `screens.jewelend.resources`;
- `screens.suspend.resources`;
- `screens.title.resources`;
- `screens.witch.resources`;
- `screens.witchend.resources`.

Therefore eventual registration changes exactly 13 records: twelve new associations plus the one
intentional menu overlap. The other eight members of the 21-record source-root corpus remain
unchanged: the six resource records above plus the two more-specific-owner records.

### Audit limits

The audit also preserves these boundaries:

- the fixture inventories 19 source files across seven groups and eighteen resource identities, but
  this control contract does not claim ownership of the resource payloads;
- fixture booleans and scalar counters establish bounded facts, not an exhaustive instruction order
  for every representative function;
- per-action New/Load/Delete/Copy selector transforms, `CURRENT_SAVE_SLOT` writes, service-call
  order, post-load routing, and `MainLoop` handoffs remain with [save-system](save-system.md);
- the source-static values 60 and 600 are counter operands, not observed wall-clock or visible-frame
  durations;
- tracked fixtures contain small metadata only. ROMs, saves, screenshots, graphics, audio, traces,
  and emulator state remain private/generated.

## Sega-Logo Control Boundary

**Confirmed static:** the dedicated fixture and owning research prose establish that the Sega-logo
source group:

- contains the representative `DisplaySegaLogo` entry;
- computes the ROM checksum;
- owns configuration-mode and debug-mode input-sequence handlers;
- can return early when Start is pressed;
- contains `VInt_CheckDebugModeCheat`, whose source advances the accepted debug sequence one byte at
  a time and activates the debug toggle when the sequence terminates.

The executable owner retains the representative symbols, source paths, and ROM addresses. A
compatibility adapter MUST retain those identities and the distinction between the main logo entry
and the VInt debug-sequence helper.

This is not a complete controller contract. Exact input samples, debounce, VInt cadence, sequence
presentation, configuration screens, checksum-failure presentation, normal player reachability, and
alternate-build behavior remain **Unknown** or separate-owner.

## Title Control Boundary

**Confirmed static:** the accepted title source group has a `StartTitleScreen` entry, two distinct
scroll-loop functions, and a bounded Start-poll helper used at multiple phases. Its source exit
distinguishes reset from continuation to the witch screen. The accepted owner confirms this control
shape, not scroll/fade frame counts or a rendered transition.

`screens.title.font` contributes only the `LoadTitleScreenFont` function identity, source path, and
address. Font payload bytes, codec behavior, layout, transfer size, VRAM destination, glyph meaning,
and final rendering are outside this contract. The title compressed-tile membership record remains
with [special-screen-asset-data](special-screen-asset-data.md).

A compatibility adapter MAY replace title rendering entirely, but an original-route mode MUST keep
the entry, two-loop distinction, bounded poll identity, and reset-versus-witch continuation result as
separate observable route facts.

## Witch Entry and Menu Control

The witch boundary is deliberately split between top-level control owned here and action internals
owned elsewhere.

### Entry and action-page admission

**Confirmed static:** pinned review of `code/specialscreens/witch/witchstart.asm`, recorded in the
owning research document, establishes this bounded entry seam:

1. `StartWitchScreen` calls `CheckSram`;
2. it tests `d0` and then `d1` with ordered `bpl.s` branches before reaching the action page;
3. the action page masks `SAVE_FLAGS` with `3`;
4. the accepted availability cases are ordered `zero`, `allSet`, and `otherNonzero`, supplying masks
   `1`, `6`, and `15` respectively;
5. a negative menu result branches back to the witch text/menu loop;
6. a nonnegative result is doubled to index the four-row word dispatch table.

These facts define top-level admission and handoff only. They do not define save validity meaning,
rendered availability, player input cadence, or action success.

### Ordered dispatch identities

**Confirmed static:** `rjt_WitchMenuActions` has four ordered targets:

| Dispatch index | Target identity | H1 address |
| ---: | --- | ---: |
| 0 | `witchMenuAction_New` | `0x7406` |
| 1 | `witchMenuAction_Load` | `0x74E2` |
| 2 | `witchMenuAction_Del` | `0x7574` |
| 3 | `witchMenuAction_Copy` | `0x754C` |

This contract retains these as dispatch handoff identities only. It does not consume or restate the
actions' selector transforms, `CURRENT_SAVE_SLOT` writes, prompts, service calls, call order,
post-load flag route, or `MainLoop`/`alt_MainLoopEntry` handoffs. Those remain separate-owner facts
in [save-system](save-system.md).

### Generic menu control

**Confirmed static:** pinned review of `code/specialscreens/witch/witchmainmenu.asm` establishes that
`ExecuteWitchMainMenu`:

- masks its starting selector with `15`;
- returns `-1` on the documented B-button path;
- checks available bit positions 0 through 3;
- wraps navigation with mask `3`;
- distinguishes four source-labelled pages: actions, new-slot names, loaded-slot names, and
  difficulties.

The page identities, selector mask, wrap mask, availability-bit domain, and cancel result form this
contract's intentional `screens.witch.menu` overlap. Palette data, option frames, bubble animation,
rendered labels, input timing, perceived navigation, and action consequences are not owned here.

`screens.witch.functions` retains only the representative
`InitializeWitchSuspendVIntFunctions` identity and its source-group ownership. The aggregate fixture
does not by itself promote a complete callback order. `screens.witch.sound-test` retains the US
`j_SoundTest` return-only identity; it does not prove another release's implementation or a
player-visible route.

## Suspend Control Boundary

**Confirmed static:** the accepted fixture and direct source review recorded by the research owner
establish:

- `SuspendGame` and `WitchSuspend` as distinct representative entries;
- a source operand of 60 before suspend presentation work;
- a source operand of 600 for the later restart wait;
- a Start branch that can end the latter wait early;
- a reset handoff through the original start vector.

The two operands are source-static counters only. This contract does not call them seconds,
wall-clock durations, guaranteed displayed frames, or observed timing. It does not establish input
sampling cadence, fades, resource transfers, save persistence, hardware reset behavior, or visible
suspend composition.

## Ending Control Boundary

**Confirmed static:** the dedicated owner and research prose establish bounded ownership identities:

- `DisplayEndingKissPicture` owns a data-driven pixel-fill renderer;
- `WitchEnd` is the representative identity from the ending-witch initialization source;
- `EndGame` is the representative ending entry, while the ending-witch source group owns the
  falling-jewel and witch-blink control identities and connects to the end-game sequence.

These are function/source/address and operation-ownership facts, not complete instruction
chronologies. Pixel-fill order, jewel trajectories, blink cadence, VInt callbacks, audio, final
composition, normal story reachability, and visible parity remain **Unknown** or separate-owner.

## Implementation-Neutral Control Model

The following is a logical compatibility model, not an engine-class prescription:

```text
SpecialScreenControlCorpus {
  sourceMembership[21] {
    recordId
    sourcePath
    executableOwner
    optionalExistingContract
  }

  directStaticBindings[19] {
    recordId
    representativeSymbol
    representativeAddress
    role: control | menu-control | resource-separate-owner
  }

  segaLogo {
    entryRef
    debugSequenceHelperRef
    computesRomChecksum
    configurationAndDebugHandlersPresent
    startEarlyExitPresent
  }

  title {
    entryRef
    scrollLoopCount: 2
    boundedStartPollRef
    resetOrWitchExitKinds[2]
    titleFontLoaderIdentityRef
  }

  witchEntry {
    checkSramResultOrder[2]: d0, d1
    saveFlagsMask: 3
    availabilityCaseOrder[3]: zero, allSet, otherNonzero
    availabilityMasks[3]: 1, 6, 15
    cancelReturnsToTextLoop
    dispatchIndexScale: 2
  }

  witchDispatch[4] {
    dispatchIndex
    targetIdentity
    targetAddress
  }

  witchMenu {
    initialSelectorMask: 15
    cancelResult: -1
    availableBitPositions[4]: 0, 1, 2, 3
    navigationWrapMask: 3
    pageKinds[4]: actions, newSlotNames, loadedSlotNames, difficulties
  }

  suspend {
    entryRefs[2]
    sourceCounterBeforePresentation: 60
    sourceRestartWaitCounter: 600
    startEarlyExitPresent
    resetVectorHandoffPresent
  }

  endingOwnership {
    endingKissPixelFillRef
    endingWitchInitRef
    endGameEntryRef
    endingWitchFallingJewelAndBlinkGroupRef
  }
}
```

The model keeps the 21-member source corpus distinct from the 19 direct H2 evidence bindings. It
does not manufacture special-screen evidence for the dedicated title-tile or witch new-game records.
It also keeps route identity separate from rendered or player-facing meaning.

## Cross-System Separation

Keep these systems outside this contract:

- [special-screen-asset-data](special-screen-asset-data.md): compressed and uncompressed resources,
  palettes, layouts, witch frames, hashes, and private payloads;
- [graphics-service-state](graphics-service-state.md): decompression/display service state and its
  hardware-facing Unknowns;
- [save-system](save-system.md): action selectors, SRAM mutations, checksums, per-action service
  order, new-game lifecycle, post-load routing, and persistence boundaries;
- [input-system](input-system.md): controller sampling, repeat state, and wait-helper behavior;
- broader gameflow/story contracts: normal route admission, story reachability, and caller meaning;
- future presentation work: fades, audio, timing, pixels, accessibility, localization, and
  replacement content.

The active blacksmith runtime research is unrelated to this contract and supplies no evidence here.

## Fidelity, Modernization, and Copyright Boundary

Original-route compatibility requires preserving the bounded identities, branch/order facts,
selector/page domains, source counters, and separations named above. It does not require a modern
implementation to reproduce the original renderer, input loop, save backend, or physical timing.

A remake may replace title/logo/witch/suspend/ending presentation, menus, transitions, timing,
accessibility, localization, and content. Those choices must remain deliberate design decisions,
not claims about the original game.

Original graphics, palettes, layouts, text, audio, screenshots, saves, traces, and rendered captures
remain private/generated copyrighted inputs. Public fixtures and tests use symbols, addresses,
counts, small control metadata, hashes owned elsewhere, and synthetic state only.

## H4 Acceptance Surface

A remake-side compatibility adapter can claim this contract only when automated tests prove:

1. the source-root membership denominator remains exactly 21, the direct static binding denominator
   remains exactly 19, and the two more-specific-owner membership records remain distinct;
2. the 19 direct bindings preserve the twelve new control records, the one menu-control overlap, and
   six unchanged resource records without reassigning asset ownership;
3. Sega-logo entry/debug-helper identity, checksum/configuration/debug ownership, and Start-early-exit
   presence match the accepted owner without claiming controller timing;
4. title entry, two-loop distinction, bounded poll identity, reset-versus-witch exit kinds, and the
   identity-only `LoadTitleScreenFont` boundary match the accepted owner;
5. witch entry preserves ordered `d0`/`d1` checks, mask `3`, ordered availability cases/masks,
   negative-result return, dispatch scale, and exact four-row target order;
6. generic witch menu control preserves selector mask `15`, cancel result `-1`, available bits 0..3,
   navigation mask `3`, and four page identities, while per-action internals remain separate-owner;
7. suspend preserves the two representative entries, source operands 60 and 600, Start-early-exit
   presence, and reset-vector handoff without promoting visible timing;
8. ending-kiss pixel-fill ownership, ending-witch initialization and end-game representative
   identities, and the ending-witch falling-jewel/blink source-group identities remain bounded facts
   rather than complete rendering chronologies;
9. public tests expose no original graphics, layouts, text, audio, saves, traces, screenshots, or
   rendered captures, and all runtime/presentation deviations are reported separately.

## Evidence Matrix

| Contract area | Evidence label | Owner | Remaining boundary |
| --- | --- | --- | --- |
| 21-member source corpus and 19 direct-binding split | **Confirmed static** | `sf2-special-screens-static-v1` ([`special-screens-static-v1.json`](../../../tests/fixtures/h2/special-screens-static-v1.json)) plus exact research-index audit | Two more-specific-owner records remain outside this contract |
| Sega-logo and title bounded control facts | **Confirmed static** | Dedicated fixture and [Special Screens](../../research/special-screens.md) source/H1 review | Input cadence, fades, scroll timing, presentation, and normal reachability |
| witch SRAM-result/action-page/dispatch/menu seam | **Confirmed static** | Dedicated fixture and [Special Screens](../../research/special-screens.md) pinned three-source review | Per-action selectors, writes, services, lifecycle, persistence, and player-driven behavior are separate-owner |
| suspend counters, Start branch, and reset handoff | **Confirmed static** | Dedicated fixture and [Special Screens](../../research/special-screens.md) direct source review | Wall-clock/visible duration, input cadence, fades, transfers, and hardware behavior |
| ending operation-ownership identities | **Confirmed static** | Dedicated fixture and [Special Screens](../../research/special-screens.md) owner prose | Full chronology, VInt callbacks, pixels, audio, story reachability, and visible parity |
| graphics, save/new-game behavior, input, and presentation | **Separate owner** | Accepted sibling contracts named above | Not consumed as evidence here |
| modernization, accessibility, localization, and replacement content | **Deliberate design** | Future product/content decisions | Requires separate acceptance and licensing |

## Reproduction

```powershell
uv run sf2 h2 special-screens
uv run sf2 design-contracts test
uv run sf2 verify
```

Generated detailed output remains under ignored `local/derived/special-screens-static.json`.
