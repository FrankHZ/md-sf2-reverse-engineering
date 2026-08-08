# Input-System Contract

- **Confirmed original behavior:** the raw two-port sampling sequence, Player 1/2 state-byte
  storage, VInt-derived current/repeat-input stage, and the input wait helper control flow below.
- **Unknown original behavior:** controller electrical latency, three- versus six-button protocol
  behavior, controller-model compatibility, and frame-exact player-visible repeat timing.
- Remake status: implementation-neutral input pipeline contract; hardware adapters and platform event
  timing remain implementation choices until the grouped runtime matrix observes the original.
- Evidence date: 2026-07-20
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`
- Traceability: `sf2-tech-services-static-v1` in
  `tests/fixtures/h2/tech-services-static-v1.json`; `sf2-tech-interrupts-static-v1` in
  `tests/fixtures/h2/tech-interrupts-static-v1.json`; `src/sf2tool/h2/services.py`; and
  `src/sf2tool/h2/interrupts.py`.

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

`WaitForPlayerInput` masks the VInt-derived current input and returns only when a recognized button is
nonzero, otherwise it waits for another VInt. `WaitForPlayer1NewInput` first waits for recognized
Player 1 input to be released, then waits for a new recognized press. The bounded Player 1 waits poll
the raw state before each VInt: one second permits at most 60 waits and three seconds at most 180, with
an early return on a recognized press. `sub_15A4` is separately modeled as scratch-mask overlap and
counter control flow: overlap below 10 clears Player 1 input, while zero overlap or a counter at least
10 clears its scratch state. Its caller role remains unproven.

## Runtime Matrix Boundary

One future controller/input matrix should cover raw state A/B to `LAST`/`CURRENT`, new press and
release/repress, held 24-frame initial delay and six-frame cadence, one/three-second early exit and
timeout, and three- versus six-button/controller-latency edge cases. These share the same VInt and
controller setup; no one-case emulator fixtures are warranted.

## Remake Boundary

A remake can separate raw device sampling, normalized button state, repeat filtering, and consumer
waits. It should preserve the confirmed stage boundaries and helper behavior where fidelity is desired,
while making platform polling, controller capability negotiation, latency policy, and accessibility
repeat settings explicit modern decisions.
