# Input-System Contract

- **Confirmed original behavior:** the five raw two-port sampling cases, three direct VInt input-repeat
  cases, and eight bounded direct wait-helper cases in the one-launch matrix below.
- **Unknown original behavior:** `sub_15A4`, controller electrical latency, three- versus six-button
  negotiation, controller-model compatibility, normal game/UI caller meaning, and user-visible UI timing.
- Remake status: implementation-neutral input pipeline contract; hardware adapters and platform event
  timing remain implementation choices until the grouped runtime matrix observes the original.
- Evidence date: 2026-08-08
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`
- Traceability: `sf2-tech-services-static-v1` in
  `tests/fixtures/h2/tech-services-static-v1.json`; `sf2-tech-interrupts-static-v1` in
  `tests/fixtures/h2/tech-interrupts-static-v1.json`; `sf2-controller-input-runtime-v1` in
  `tests/fixtures/h3/controller-input-v1.json`; `src/sf2tool/h2/services.py`;
  `src/sf2tool/h2/interrupts.py`; and `src/sf2tool/h3/controller_input.py`.

## Confirmed Static Contract

`UpdatePlayerInputs` samples `DATA1` then `DATA2` (a two-byte port stride). It writes TH low (`0`),
reads, writes TH high (`$40`), shifts the first read left two and masks `$C0`, reads and masks `$3F`,
combines the parts, inverts the byte, and stores it. Its local sampler produces two state bytes per
port in contiguous Player 1 then Player 2 state storage. This is raw controller sampling; it is not
by itself a statement that every modern controller exposes the same electrical protocol.

The VInt-owned repeat stage is a distinct contract. It calls the raw sampler, derives
`CURRENT_PLAYER_INPUT` and `LAST_PLAYER_INPUT`, suppresses unchanged input through the initial
24-frame delay, and then subtracts six to create the static repeat cadence. The source establishes
the counter operations; it does not establish externally observed input-to-frame latency.

The static source inventory separately describes `WaitForPlayerInput`, `WaitForPlayer1NewInput`, the
one/three-second waits, and `sub_15A4`. The first four have the bounded direct runtime observations
below; `sub_15A4` remains queued rather than inferred from static control flow.

## Runtime Matrix Boundary

`sf2-controller-input-runtime-v1` is exactly one direct-function-seam launch. Five calls to original
`UpdatePlayerInputs` observe neutral, Player 1 Up+B, Player 2 C+Start, simultaneous combined basic
buttons, and release. All record the two raw bytes for each port. Three direct calls to original
`ApplyZ80BusUpdates` observe new press, release/repress, and a held C input at the source-derived
24-frame threshold and six-frame cadence. Repeat execution is direct VInt input-stage observation.

Eight direct helper cases execute original `WaitForPlayerInput`, `WaitForPlayer1NewInput`,
`WaitForInputFor1Second`, and `WaitForInputFor3Seconds`. They cover immediate and delayed current-input
return; neutral-to-press and held-to-release-to-repress new-input return; and early-input versus full
60/180-cycle DBF boundaries. For each actual wait cycle the observer confirms the source call/target/RTS/
return chain for `WaitForVInt`, the enabled original `VInt` entry, and its original input stage, then
records the raw/repeat state; the timed helpers also prove that `d5` is restored. This is direct helper
and enabled-VInt progression, not normal game/UI caller provenance, skipped-VInt ordering, or hardware
latency evidence.

The direct seam performs one harness-only normalization at the first source `WaitForVInt` call after
its SR-unmask preamble: it writes/reads current and last input from the fixture's initial Player 1 byte
and clears the repeat delay, excluding any unowned preamble VInt state change. It never resets later
source wait cycles, and does not establish natural normal-caller state.

The observer also checks direct call/target/return triples and the original nested source
call/target/return `ApplyZ80BusUpdates` → `UpdatePlayerInputs` path. Its `CheckSram` return redirect
only enters the temporary work-RAM probe, whose gate arms one direct call before each host frame and
pauses after return. `sub_15A4`, three-/six-button negotiation, hardware latency, normal game/UI caller
meaning, and UI/menu behavior remain grouped Unknown questions.

## Remake Boundary

A remake can separate raw device sampling, normalized button state, repeat filtering, and consumer
waits. It should preserve the confirmed stage boundaries and helper behavior where fidelity is desired,
while making platform polling, controller capability negotiation, latency policy, and accessibility
repeat settings explicit modern decisions.
