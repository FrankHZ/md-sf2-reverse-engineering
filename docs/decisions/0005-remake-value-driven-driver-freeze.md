# ADR 0005: Remake-Value-Driven Driver and Hardware Freeze

- Status: **Accepted**
- Decision date: 2026-07-23
- Scope: Phase 2 priority for sound-driver and other low-remake-value driver/hardware work

## Decision

Phase 2 preserves every accepted H1/H2/H3 finding, fixture, parser, schema, and normal verification
gate. A priority freeze does not delete evidence, weaken a verifier, or change a finding's
**Confirmed**, **Inferred**, or **Unknown** label. It only stops default expansion of a research area
whose remaining exactness has low marginal value for a modern remake.

The main frontier is now event semantics and state flow, maps and interactions, UI/menu/save behavior,
and implementation-neutral content contracts.

Sound is frozen at the current proven command/header/channel/SFX seam. Do not by default pursue
YM2612/PSG register-level fidelity, PCM sample-rate calibration, per-song opcode replay, or audible
waveform parity. Continue to preserve music loop, transition, fade, and resume semantics when a
remake acceptance contract needs them, along with SFX selection, priority, and interruption behavior.

Apply the same remake-value test after an implementation-neutral import/schema and user-visible
contract are adequate. The default freeze covers decompressor and copy-loop micro-implementation,
VDP/DMA cycle accuracy, raw controller electrical or latency behavior, and SRAM hardware-failure
exactness. It does not freeze asset IDs or call-site mapping, visible UI/menu/save/load flows, or
other acceptance-relevant behavior.

A frozen area reopens only for one bounded question triggered by one of these conditions:

1. a concrete remake acceptance failure or missing contract;
2. a required import-format, asset, or provenance decision;
3. an explicit original-hardware-fidelity target; or
4. conflicting evidence that invalidates the existing seam.

The reopened question must reuse the existing parser/fixture/observation seam rather than starting a
new open-ended driver investigation.

Publicly available MIDI is an input candidate, not a presumed redistributable asset. Before it is
adopted, record its source provenance and license/permission; this decision authorizes neither a
download nor redistribution.

## Why

The project already has a source/ROM-verified music command, header, channel, and SFX boundary plus a
bounded live-state fixture. For a modern remake with replaceable music assets, exact chip-register,
sample-rate, and waveform behavior does not normally improve the player-facing contract enough to
justify deeper original-driver work. The same principle applies to lower-level hardware behavior when
the accepted data import and visible interaction contract already cover the remake need.

This focuses limited Phase 2 capacity on decisions that determine a modern implementation's
correctness: story/event state, map interaction and collision, battle rules, user-visible menus and
save/load behavior, and content ownership/mapping.

## Consequences

- Existing sound, technical-services, interrupt, and graphics evidence remains reproducible through
  the normal verification rails.
- Research documents retain their evidence dates and labels; a priority/decision date records this
  policy without rewriting historical evidence.
- Future queues distinguish user-visible semantics from frozen driver/hardware exactness.
- A later remake may deliberately choose licensed replacement music or another implementation, but
  that choice requires separate provenance and implementation decisions.
