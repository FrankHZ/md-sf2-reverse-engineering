# Technical Interrupt, DMA, and Trap Services

- Status: **Confirmed** for the pinned 21-file layout-owned inventory, representative H1 addresses,
  VInt call order, contextual slots, wait/sleep handshake, DMA queue routing, fade control, input-repeat
  counters, trap dispatch rules, and the bounded direct input-stage H3 observation below
- Status: **Inferred** for hardware-facing intent where static register writes are clear but exact cycle
  behavior has not been observed
- Status: **Unknown** for Z80/VDP bus timing, DMA queue overflow behavior, visual fade timing, and
  hardware/emulator differences
- Evidence date: 2026-08-08
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Complete Static Boundary

All 21 files under `code/common/tech/interrupts` are directly included by the pinned layout and have
representative symbols in the H1 listing. Together they contain 2,320 lines, 105 global labels, 50
direct call sites, and one indirect call site. The inventory includes the active VInt, VDP/DMA,
fading, input/Z80, trap, and contextual-function paths as well as error, HInt, palette, and three
explicitly unused VInt helpers.

## VInt and Contextual Functions

When the enable bit is set, `VInt` waits for DMA, disables display, processes VDP queues, enables
display, processes a VRAM read, applies fading, performs Z80/input work, and finally calls contextual
functions. It then clears the flag awaited by `WaitForVInt`. The frame counter increments even when
the update block is skipped. During the intro, Start can jump through the one-shot escape pointer.

There are eight contextual-function pointer slots controlled by an eight-bit enable field. Their
frame counter advances the seconds counter every 60 frames. Trap 9 exposes five actions: clear all
pointers, set a function and trigger, clear a function and trigger, clear a trigger, and set a
trigger. Static evidence establishes slot and bitfield behavior; caller-specific scheduling remains a
runtime question.

`WaitForVInt` sets the update-enable bit and spins until VInt clears its waiting flag. `Sleep(0)`
returns without waiting; positive values invoke that handshake for the requested frame count.

The accepted controller H3 launch retains the direct `ApplyZ80BusUpdates` repeat seam, and additionally
calls the original input wait helpers directly. The latter exercises their original `WaitForVInt` call,
the enabled original `VInt` entry, its `ApplyZ80BusUpdates` input stage, and the return that releases
the wait flag. It does not establish a normal game/UI caller progression, the skipped-VInt path or its
ordering against the enabled path, contextual-slot behavior, or fade completion; those remain
**Unknown**.

## DMA, Fading, and Input

The immediate VRAM DMA path masks interrupts and acquires the Z80 bus. The queued path temporarily
disables VInt, appends a command, advances the queue pointer, and increments queue size. Queue
processing updates the sprite table before queued transfers and resets the pointer afterward. The
static rail deliberately does not claim safe queue capacity or exact bus latency.

Four fade entry points select in/out from black/white. Execution initializes all four palettes, waits
until VInt clears the fade setting, then waits one additional VInt. Each color component is clamped to
its nibble range before a CRAM DMA is queued.

After `UpdatePlayerInputs` samples raw controller state, unchanged input is suppressed until the repeat
delay reaches 24 frames. After a repeated input is emitted, subtracting six from the delay produces a
six-frame repeat cadence. The bounded one-launch H3 fixture
`sf2-controller-input-runtime-v1` in `tests/fixtures/h3/controller-input-v1.json`, verified by
`src/sf2tool/h3/controller_input.py`, directly calls the original `ApplyZ80BusUpdates` input stage and
observes new press, release/repress, held-input suppression through the 24-frame threshold, and the
six-frame cadence. In the same one-launch fixture, eight direct cases across the four wait helpers observe the original
`WaitForVInt` entry/return counts and enabled-VInt input-stage progression: immediate/delayed player
input, release/repress new-input, and early/60/180-cycle timed boundaries. Run
`uv run sf2 h3 controller-input --timeout-seconds 180` to reproduce it. This confirms the bounded
input-stage delay/cadence and direct wait/VInt boundary only: normal VInt caller progression, skipped/
enabled ordering, contextual slots, fade completion, latency, UI behavior, and controller hardware
behavior remain **Unknown**. The raw sampling and wait-helper boundary is in
[`input-system.md`](../design/contracts/input-system.md).

## Trap Boundary

Trap 0 has four sound-command slots, accepts `d0` when its inline parameter is `-1`, and drops commands
while sound commands are disabled. Traps 1-4 wrap flag checks/set/clear. Trap 5 treats text index `-1`
as close-dialogue and otherwise displays text. Trap 6 activates the entity VInt function before
executing a map script.

## Concentrated Runtime Queue

The accepted direct input-stage launch closes the 24/6 repeat boundary and the bounded direct
wait-helper/VInt progression. Under ADR 0005's
2026-07-23 priority decision, exact VDP/DMA/Z80 cycle timing, DMA queue-capacity behavior, and raw
controller latency are frozen once the visible contract is adequate. A future shared matrix is active
only for a concrete user-visible acceptance gap: skipped/enabled VInt ordering, contextual slot
activation, or fade completion behavior. It must reuse this VInt/DMA seam; do not start a
hardware-fidelity matrix merely to refine cycle counts or a second-emulator comparison.

## Reproduction

```powershell
uv run sf2 h2 tech-interrupts
uv run sf2 h3 controller-input --timeout-seconds 180
uv run sf2 research-index test
```

Generated JSON stays under ignored `local/derived/tech-interrupts-static.json`.
