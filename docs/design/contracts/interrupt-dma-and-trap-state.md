# Interrupt, DMA, and Trap State Contract

- Status: **Confirmed static interrupt scheduling, transfer-control, fade, and trap inventory**
- Evidence date: 2026-08-09
- Scope: implementation-neutral reconstruction of the original VInt scheduler, contextual-function
  slots, wait/sleep handshake, DMA control routes, fade state machine, and bounded trap services,
  without converting source-static order into hardware timing, visible presentation, queue safety,
  input/UI meaning, or downstream subsystem effects

## Judgment Boundary

This contract begins at the accepted interrupt and trap entry identities. It preserves scheduler
gates and order, state handshakes, bounded queue mutations, fade-control predicates, contextual-slot
actions, and selected trap transport facts. It ends at hardware-facing writes or any audio, input,
flag, dialogue, map-script, graphics, window, or other subsystem handoff.

- **Confirmed**: the 21-file source inventory has 105 global labels and 50 direct call sites; VInt
  uses an enable gate, preserves an eight-stage update order, clears the waiting flag after contextual
  functions, and increments its frame counter even when updates are skipped; eight contextual slots
  use an enable bitfield and a 60-frame seconds counter; Trap 9 exposes five accepted actions;
  `WaitForVInt` and `Sleep` preserve their accepted handshake; immediate and queued DMA routes have
  distinct interrupt/VInt control, request, queue-size, sprite-order, and pointer-reset facts; four
  fade modes share the accepted setting-clear plus one-extra-VInt wait; sound, text, map-script, and
  flag-trap inventory facts retain their exact bounded forms.
- **Inferred**: hardware-facing intent where the source register and bus operations are structurally
  clear. Exact device timing, transfer completion, and visible or audible results are not inferred.
- **Unknown**: Z80/VDP bus latency; DMA queue capacity, overflow, partial processing, and failure
  recovery; exact interrupt cadence and nesting; contextual-slot caller activation; natural intro
  escape reachability; visible fade duration and frames; controller latency and UI meaning; trap
  caller admission and downstream outcomes; exact flag-trap operand/return behavior; nominally unused
  helper reachability; malformed, modified, or injected state; hardware/emulator differences;
  persistence; and story or balance meaning.

The contract captures a portable scheduling and service boundary. It does not require a modern engine
to emulate Mega Drive registers or cycle timing unless an original-hardware adapter explicitly owns
that fidelity goal.

## Evidence Owner and Association Audit

`sf2-tech-interrupts-static-v1`
([`tech-interrupts-static-v1.json`](../../../tests/fixtures/h2/tech-interrupts-static-v1.json)) is the
sole executable owner consumed by this contract. Its verifier is
[`interrupts.py`](../../../src/sf2tool/h2/interrupts.py), and its source-backed explanation is
[Technical Interrupt, DMA, and Trap Services](../../research/technical-interrupts.md). This contract
consumes selected fields from `expected.interruptFacts` and no H3 fixture.

The executable owner directly binds 21 research records. The exact future association partition is:

- 20 currently unassociated `tech.interrupts.*` records;
- one intentional overlap, `tech.interrupts.trap-flags`, whose existing
  [global-flag state](global-flag-state.md) association remains intact.

The overlap is narrow. This contract consumes only the fixture-owned `flagTrapCount=4` inventory
fact. The global-flag contract remains the owner of the representative Trap 4 address and bounded
Check/Set/Clear grouping and storage seam. Inline operand decoding, saved-return movement,
caller-visible results, condition-code behavior, exact per-trap mapping, and runtime reachability are
not consumed here and remain **Unknown**.

The accepted source baseline is pinned upstream commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`. The 20 newly associated records retain these
representative entry identities and addresses:

| Research record | Representative entry | ROM address |
| --- | --- | ---: |
| `tech.interrupts.z80-fade-input` | `ApplyZ80BusUpdates` | `0x08DE` / 2,270 |
| `tech.interrupts.vram-fill` | `ApplyVramDmaFill` | `0x140E` / 5,134 |
| `tech.interrupts.enable-dma-queue` | `EnableDmaQueueProcessing` | `0x0F2A` / 3,882 |
| `tech.interrupts.errors` | `Int_AddressError` | `0x0490` / 1,168 |
| `tech.interrupts.fading` | `FadeInFromBlack` | `0x0CD6` / 3,286 |
| `tech.interrupts.hint` | `HInt` | `0x0592` / 1,426 |
| `tech.interrupts.trap-sound` | `Trap0_SoundCommand` | `0x045C` / 1,116 |
| `tech.interrupts.trap-text` | `Trap5_TextBox` | `0x0556` / 1,366 |
| `tech.interrupts.trap-map-script` | `Trap6_TriggerAndExecuteMapScript` | `0x057A` / 1,402 |
| `tech.interrupts.trap-contextual` | `Trap9_ManageContextualFunctions` | `0x07CE` / 1,998 |
| `tech.interrupts.unused-palette` | `SetBasePalette1` | `0x0CC4` / 3,268 |
| `tech.interrupts.unused-vint-request` | `RequestVdpCommandQueueProcessing` | `0x0F1A` / 3,866 |
| `tech.interrupts.unused-vint-queue` | `sub_F3A` | `0x0F3A` / 3,898 |
| `tech.interrupts.unused-vint-read` | `sub_13C0` | `0x13C0` / 5,056 |
| `tech.interrupts.scroll-data` | `sub_1234` | `0x1234` / 4,660 |
| `tech.interrupts.vdp-control` | `WaitDmaEnd` | `0x0B96` / 2,966 |
| `tech.interrupts.vint` | `VInt` | `0x0594` / 1,428 |
| `tech.interrupts.vint-engine-core` | `ClearVsramAndSprites` | `0x0DBA` / 3,514 |
| `tech.interrupts.vint-engine-dma` | `ApplyImmediateVramDma` | `0x10DC` / 4,316 |
| `tech.interrupts.vint-engine-compressed` | `ApplyImmediateVramDmaOnCompressedTiles` | `0x1382` / 4,994 |

`tech.interrupts.trap-flags` is deliberately absent from this address table because its address and
wrapper grouping remain with the global-flag owner. Sharing the fixture does not transfer that
evidence to this contract.

## VInt Scheduling

**Confirmed static:** the accepted VInt update block executes only when its source enable bit is set.
When enabled, its high-level order is exactly:

1. wait for DMA;
2. disable display;
3. process VDP queues;
4. enable display;
5. process the VRAM read stage;
6. apply fading;
7. perform the Z80/input stage;
8. manage contextual functions.

After contextual functions, VInt clears the flag awaited by `WaitForVInt`. The frame counter still
increments when the gated update block is skipped. These are source-static order and state facts, not
cycle counts, frame-perfect presentation, device completion, or proof that every stage performs work
on each call.

The source also contains a conditional intro escape through a configured one-shot pointer when Start
is observed. This contract retains the branch capability only. Pointer setup, exact input sample,
normal intro reachability, continuation behavior, visible transition, and player intent remain with
[startup control flow](startup-control-flow.md),
[special-screen control flow](special-screen-control-flow.md), the input owner, or **Unknown**.

## Contextual Functions, Wait, and Sleep

**Confirmed static:** contextual functions use eight pointer slots governed by an eight-bit enable
field. Their frame counter advances a seconds counter after the accepted 60-frame threshold. The
threshold is a source counter, not a wall-clock guarantee on every host.

Trap 9 exposes exactly five source actions:

1. clear pointers;
2. set a function and trigger;
3. clear a function and trigger;
4. clear a trigger;
5. set a trigger.

This is an action inventory, not a claim about slot admission, callback ABI, caller-specific
scheduling, reentrancy, execution frequency, or visible behavior.

`WaitForVInt` sets the update-enable bit and spins until VInt clears its waiting flag. `Sleep(0)`
returns without waiting; a positive source argument repeats the VInt handshake for the requested
frame count. The accepted relationship does not establish exact elapsed time, host thread behavior,
interrupt latency, skipped-VInt ordering, or failure recovery.

The [input-system contract](input-system.md) owns the accepted direct wait-helper H3 progression and
the static/runtime 24-frame initial input-repeat delay and six-frame repeat cadence. Although those
`inputRepeat` facts live in the same H2 fixture, this contract deliberately does not make them an H4
fidelity surface and adds no input-specific research association.

## DMA Control Routes

**Confirmed static:** the immediate VRAM DMA path masks interrupts and requests the Z80 bus. The
queued path instead temporarily disables VInt while appending its command and increments the queue
size. These are distinct control routes; the fixture does not establish that either is preferable or
safe for arbitrary workloads.

Queue processing preserves two accepted facts:

- the sprite-table update occurs before queued transfers;
- processing resets the queue pointer afterward.

The source predicate requires a processing request unless DMA is already active. The contract does
not infer queue capacity, overflow protection, exact command layout, atomicity, queue-size reset,
partial completion, transfer success, bus release timing, or recovery from invalid entries.

`ApplyVramDmaFill`, `WaitDmaEnd`, `ApplyImmediateVramDma`, and
`ApplyImmediateVramDmaOnCompressedTiles` remain entry/control identities. Compressed payload layout,
decoder micro-implementation, asset provenance, decoded bytes, destinations, and rendered results
remain with [graphics-service state](graphics-service-state.md), the appropriate asset contracts, or
**Unknown**.

## Fade Control

**Confirmed static:** the accepted fade entry identities select four modes:

- in from black;
- out to black;
- in from white;
- out to white.

Fade execution initializes the palette-selection bitfield to 15, waits until VInt clears the fade
setting, and then waits one additional VInt. Each source color component is clamped to the nibble
range before a CRAM DMA is queued after the color update.

These facts define control state and order only. They do not define visible duration, palette
contents, color-space equivalence, DMA cadence or completion, accessibility, scene ownership, or what
the player sees between entry and return. A modern renderer may implement a host-native transition
while retaining a compatibility trace for the accepted mode and setting-clear plus one-extra-VInt
control handshake.

## Trap Inventory and Handoffs

### Sound command trap

**Confirmed static:** the sound trap has four command slots. An inline parameter of `-1` selects the
value from `d0`, and disabled sound commands cause commands to be dropped. This is a bounded command
transport fact, not an audio queue-capacity proof, command-domain contract, audible-output claim, or
driver timing guarantee. Those remain with [audio-system](audio-system.md) or **Unknown**.

### Flag traps

**Confirmed static inventory only:** the fixture records `flagTrapCount=4`. No additional flag-trap
behavior is consumed here. The [global-flag state contract](global-flag-state.md) remains the sole
design owner of its accepted address/grouping/storage seam, and all inline-operand, return-address,
result, condition-code, mapping, and runtime questions remain outside this contract.

### Text and map-script traps

**Confirmed static:** the text trap treats source text index `-1` as its close-dialogue route. Other
indexes hand off to text display. The map-script trap activates the entity VInt function before its
map-script execution handoff.

The `-1` route and handoff order do not import text decoding, dialogue-window state, command timing,
map-script dispatch, entity behavior, story meaning, presentation, or caller reachability. Those
remain with [dialogue-system](dialogue-system.md), [map-exploration](map-exploration.md), their
evidence owners, or **Unknown**.

## Inventory-Only Handlers

The complete source boundary retains the error and HInt handlers plus a palette helper and three
explicitly unused VInt helpers. Their representative identities and addresses are Confirmed static.
The source names and inventory classification do not prove that code is unreachable under indirect
calls, modified builds, debug routes, injected state, or alternate platform conditions.

No error screen, HInt cadence, scroll presentation, palette result, recovery policy, or unused-helper
effect is promoted into a remake requirement. A compatibility adapter may retain identity-level
traceability without exposing these routines in a portable engine API.

## Cross-System Separation

Interrupt code is a transport and scheduler surface, not ownership of every callee:

- [input-system](input-system.md) owns controller sampling, repeat cadence, and direct wait-helper H3
  behavior;
- [global-flag state](global-flag-state.md) owns accepted flag storage and wrapper boundaries;
- [audio-system](audio-system.md) owns sound command domains, driver state, and audible boundaries;
- [dialogue-system](dialogue-system.md) owns text command and caller seams;
- [map-exploration](map-exploration.md) owns accepted map-script, entity, layout, and presentation
  behavior;
- [graphics-service state](graphics-service-state.md) owns graphics service and decompression entry
  boundaries;
- [window-system](window-system.md) owns window composition and its transfer-call order;
- [startup control flow](startup-control-flow.md) and
  [special-screen control flow](special-screen-control-flow.md) own startup and intro/title routes.

This contract does not associate any input, flag, audio, dialogue, map, graphics, window, or startup
research record by implication. The sole intentional overlap is `tech.interrupts.trap-flags`, and it
is limited to the four-trap inventory count described above.

## Implementation-Neutral State Model

The minimum logical projection stores control metadata rather than hardware payloads:

```text
InterruptServicePlan
  evidenceOwner: sf2-tech-interrupts-static-v1.expected.interruptFacts
  sourceCommit
  sourceFiles: 21
  representativeEntries[20]:
    researchRecordId
    symbol
    romAddress

  vint:
    updatesRequireEnableBit: true
    orderedStages[8]
    clearWaitingFlagAfterContextualFunctions: true
    frameCounterIncrementsWhenUpdateBlockSkipped: true
    introEscapePointerBranchIdentity

  contextualSlots:
    slotCount: 8
    enableBitfield
    secondsCounterThresholdFrames: 60
    trap9Actions[5]

  waitAndSleep:
    waitSetsEnableBit: true
    waitUntilVintClearsFlag: true
    zeroSleepReturns: true
    positiveSleepRepeatsHandshake: true

  dmaControl:
    immediateMasksInterruptsAndRequestsZ80Bus: true
    queuedTemporarilyDisablesVint: true
    queuedEntryIncrementsQueueSize: true
    processingRequestPredicate
    spriteBeforeQueuedTransfers: true
    processingResetsQueuePointer: true

  fadeControl:
    modes[4]
    initialPaletteBitfield: 15
    waitForSettingClear: true
    oneAdditionalVint: true
    componentNibbleClamp: true
    queueCramDmaAfterColorUpdate: true

  traps:
    soundSlots: 4
    soundMinusOneUsesD0: true
    disabledSoundDropsCommand: true
    flagTrapInventoryCount: 4
    textMinusOneCloseRoute: true
    mapScriptActivatesEntityVintFirst: true

  inventoryOnlyHandlerIdentities
```

The public model omits source code, ROM bytes, queue contents, palettes, compressed or decoded assets,
text, audio, live RAM, traces, emulator states, and captured frames. Private original-fidelity tooling
may reconstruct those values from licensed or user-provided inputs without publishing them.

## Original Fidelity and Modernization

Original-fidelity testing preserves scheduler order, state handshakes, bounded queue mutations, fade
control, trap inventory, and representative entry identities. It keeps hardware timing, downstream
effects, and caller admission as separate observations rather than silently filling them in.

A modern engine may use host event loops, typed callback registries, bounded transfer queues,
renderer-native fades, and explicit service interfaces. Such choices are modernizations. A
compatibility adapter should still be able to emit the accepted control trace and identify deliberate
deviations without exposing private payloads.

## H4 Acceptance Gates

A future interrupt-service adapter passes this contract only when:

1. the exact 21-record association boundary remains reproducible as 20 new records plus the one
   intentional `tech.interrupts.trap-flags` overlap, with no adjacent record change;
2. the 20 representative entry identities and addresses remain traceable, while the flag-trap
   address/grouping remains solely with the global-flag owner;
3. VInt preserves the enable gate, eight-stage order, post-context waiting-flag clear, and skipped-
   update frame-counter increment without converting them into cycle or presentation claims;
4. eight contextual slots, their enable bitfield, the 60-frame source counter, and five Trap 9 actions
   remain reproducible without asserting caller-specific activation;
5. the wait/sleep handshake preserves zero-versus-positive behavior while input repeat and direct H3
   progression stay with input-system;
6. immediate and queued DMA control facts, request predicate, sprite-before-queue order, and pointer
   reset remain distinct, with capacity, overflow, command layout, timing, and completion separately
   tested or **Unknown**;
7. four fade modes, bitfield 15, setting-clear plus one-extra-VInt wait, nibble clamps, and CRAM-DMA
   queue order remain reproducible without claiming visible frames;
8. sound, text, and map-script trap facts remain transport/handoff facts only, while flag traps expose
   only `flagTrapCount=4` in this contract;
9. error, HInt, palette, scroll, and unused-helper identities remain inventory facts without dead-code
   or runtime-effect claims;
10. public artifacts contain metadata and synthetic state only, never ROM, code, queue, palette,
    graphics, text, audio, live memory, trace, save, or emulator-state payloads.

## Evidence Matrix

| Contract area | Evidence label | Executable owner | Remaining boundary |
| --- | --- | --- | --- |
| 21-file inventory and 20 selected entry identities/addresses | **Confirmed static** | `sf2-tech-interrupts-static-v1` ([`tech-interrupts-static-v1.json`](../../../tests/fixtures/h2/tech-interrupts-static-v1.json)) | Indirect reachability, alternate builds, runtime effects |
| VInt gate/order, contextual slots, and wait/sleep handshake | **Confirmed static** | same `expected.interruptFacts` owner | Cadence, nesting, callback activation, failure behavior |
| immediate/queued DMA control and processing order | **Confirmed static** | same owner | Capacity, overflow, command layout, hardware timing/completion |
| fade state and setting-clear plus one-extra-VInt control handshake | **Confirmed static** | same owner | Transfer completion, palette content, visible fade completion/duration, CRAM-DMA cadence |
| sound/text/map-script trap transport and handoffs | **Confirmed static** | same owner | Downstream audio/dialogue/map behavior and caller admission |
| four flag traps | **Confirmed static inventory only** | same owner for count; [global-flag state](global-flag-state.md) for its accepted address/grouping/storage seam | Operands, return movement, results/CCR, exact mapping, runtime reachability |
| input repeat and direct wait-helper runtime progression | **Separate-owner Confirmed** | [input-system](input-system.md) | Not an H4 surface here |
| hardware-facing intent | **Inferred** | source register and bus operations | Exact device timing remains **Unknown** |
| visible and audible outcomes | **Unknown** | future bounded runtime evidence | Do not infer presentation parity from static order |

## Reproduction

```powershell
uv run sf2 h2 tech-interrupts
uv run sf2 design-contracts test
uv run sf2 verify
```

Generated JSON remains under ignored `local/derived/tech-interrupts-static.json`.
