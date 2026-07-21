# Technical Interrupt, DMA, and Trap Services

- Status: **Confirmed** for the pinned 21-file layout-owned inventory, representative H1 addresses,
  VInt call order, contextual slots, wait/sleep handshake, DMA queue routing, fade control, input-repeat
  counters, and trap dispatch rules
- Status: **Inferred** for hardware-facing intent where static register writes are clear but exact cycle
  behavior has not been observed
- Status: **Unknown** for Z80/VDP bus timing, DMA queue overflow behavior, visual fade timing, and
  hardware/emulator differences
- Evidence date: 2026-07-20
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
six-frame repeat cadence. This is a VInt-derived current/last-input static counter contract, not yet a
controller-latency measurement; the raw sampling and wait-helper boundary is in
[`input-system.md`](../design/input-system.md).

## Trap Boundary

Trap 0 has four sound-command slots, accepts `d0` when its inline parameter is `-1`, and drops commands
while sound commands are disabled. Traps 1-4 wrap flag checks/set/clear. Trap 5 treats text index `-1`
as close-dialogue and otherwise displays text. Trap 6 activates the entity VInt function before
executing a map script.

## Concentrated Runtime Queue

No emulator is started for this batch. A later technical runtime matrix should share one launch and
observe: skipped/enabled VInt ordering, the 24/6 input-repeat boundary, contextual slot activation,
queued versus immediate DMA completion, fade completion frames, and queue-capacity behavior. Exact
VDP/Z80 timing should be checked under the pinned core and, only where useful, a second emulator.

## Reproduction

```powershell
uv run sf2 h2 tech-interrupts
uv run sf2 research-index test
```

Generated JSON stays under ignored `local/derived/tech-interrupts-static.json`.
