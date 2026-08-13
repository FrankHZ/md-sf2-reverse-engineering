# Save-System Contract

- **Confirmed original behavior:** the static two-slot SRAM representation, byte-interleaved copy
  direction, additive checksum/check order, occupied-flag operations, save/load/copy/delete helper
  sequence, witch-menu selector/action routing, and the bounded in-process H3 matrix described below.
- **Unknown original behavior:** cross-process physical persistence, power-loss/partial-write
  outcomes, corruption behavior outside the checked byte checksum, player-driven New-game naming/menu
  presentation or input cadence, and caller-visible pixels/audio/suspend timing.
- Remake status: implementation-neutral contract; in-process service effects are observed, while
  durable-medium behavior remains unobserved.
- Evidence date: 2026-08-13
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`
- Traceability: `sf2-tech-services-static-v1` in
  `tests/fixtures/h2/tech-services-static-v1.json`; `src/sf2tool/h2/services.py`; and
  `docs/research/technical-services.md`. The static witch-menu contract is
  `sf2-special-screens-static-v1` in
  `tests/fixtures/h2/special-screens-static-v1.json`; `src/sf2tool/h2/screens.py`; and
  `docs/research/special-screens.md`. The in-process runtime contract is
  `sf2-witch-save-actions-runtime-v1` in
  `tests/fixtures/h3/witch-save-actions-v1.json`; `src/sf2tool/h3/witch_save_actions.py`; and
  `docs/research/special-screens.md`. The separate Witch action-admission runtime contract is
  `sf2-witch-save-menu-actions-runtime-v1` in
  `tests/fixtures/h3/witch-save-menu-actions-v1.json`;
  `src/sf2tool/h3/witch_save_menu_actions.py`; and `docs/research/special-screens.md`. The bounded
  New-game runtime contract is
  `sf2-witch-new-game-lifecycle-runtime-v1` in
  `tests/fixtures/h3/witch-new-game-lifecycle-v1.json`;
  `src/sf2tool/h3/witch_new_game_lifecycle.py`; and `docs/research/special-screens.md`. The
  direct-service lifecycle contract is `sf2-sram-lifecycle-runtime-v1` in
  `tests/fixtures/h3/sram-lifecycle-v1.json`; `src/sf2tool/h3/sram_lifecycle.py`; and
  `docs/research/technical-services.md`.

## Confirmed Static Contract

There are two logical save slots. A selector value of zero addresses slot 1; any nonzero selector
addresses slot 2. Each slot stores 4,016 logical bytes as 4,016 physical storage-byte writes.  With
the two-byte address step, those writes occupy a reserved 8,032-byte SRAM address interval; this
interval is not 8,032 stored-byte writes. Slot occupied flags are bits 0 and 1 of the save-flags
field.

`CheckSram` validates the signature first, then slot 2, then slot 1. For an occupied slot it copies
the slot through the interleaved reader, compares its computed low-byte additive checksum with the
selected stored checksum, and clears the occupied bit on mismatch. Its static results are 1 for a
valid occupied slot, 0 for an unoccupied slot, and -1 for a failed occupied slot. A signature
mismatch clears all 8,192 logical SRAM bytes, writes the signature, and then clears save flags.

Saving copies combatant data into the selected slot, stores the low checksum byte, then sets that
slot's occupied bit. Loading copies the selected slot back to combatant data but does not perform a
checksum comparison locally. Copy loads the selected slot and saves it to the opposite slot; delete
only clears the selected occupied bit. The contract intentionally distinguishes those static helper
operations from a durable-medium guarantee.

## Confirmed In-Process Runtime Matrix

**Confirmed:** one BizHawk 2.11.1 / Genesis Plus GX launch invokes original `SaveGame`, `LoadGame`,
`CopySave`, and `ClearSaveSlotFlag` after the original `CheckSram` return. The nine direct cases use
seed 19 for slot 1 (stored/computed checksum 71) and seed 20 for slot 2 (stored/computed checksum
247). `LoadGame` restores each selector's four poisoned-and-rechecked payload samples. Copy selector
0 transfers slot 1 to slot 2 with destination selector 1 and checksum 71; after restoring the second
source payload, copy selector 1 transfers slot 2 to slot 1 with destination selector 0 and checksum
247. Delete clears occupied bits 3→2→0 without changing the observed payload samples or stored/computed
checksum byte 247. These are observed values, not a claim that the 8,032-byte physical address interval is
an 8,032-byte stored payload; each slot stores 4,016 physical bytes at a two-byte address step.

**Confirmed:** Load with source flag 88 clear reaches `GetSavepointForMap` as both instruction and
effective target at 30188. With flag 88 set, it reaches instruction target `j_BattleLoop` at 131124
and jump-interface effective target `BattleLoop` at 146052. The source label `flag 88` is retained;
its player-facing lifecycle meaning is not inferred.

**Unknown:** the one-process service fixture does not establish cross-process SRAM survival, physical
power-cycle behavior, partial/interrupted-write recovery, player-driven New-game naming/menu results,
pixels, audio, input cadence, or suspend presentation. Those remain the grouped H3 questions named in
`docs/research/special-screens.md`.

## Confirmed Witch Save-Menu Action Admission Matrix

**Confirmed:** a separate ten-case, one-launch matrix executes the original
`witchMenuAction_Load`, `witchMenuAction_Copy`, and `witchMenuAction_Del` entries after the original
`CheckSram` bootstrap. Its controlled menu/prompt seam supplies only source-compatible return values.
It observes Load and Delete page 2, the `SAVE_FLAGS & 3` then one-bit selector scale, the selector
minus-one write to `CURRENT_SAVE_SLOT`, and Copy's masked/minus-one source selector. Menu cancel and
nonzero prompt returns reach the existing Witch loop without an original service call. Confirmed Load,
Copy, and Delete cases reach and return from the original `LoadGame`, `CopySave`, and
`ClearSaveSlotFlag` entries respectively. Load stops at the source-derived `GetSavepointForMap` or
`j_BattleLoop`/`BattleLoop` handoff; it does not promote downstream loop behavior.

**Confirmed harness boundary:** callback exceptions are status-bearing and nonzero, including case,
phase, role, expected/actual callback state, and pending role. The one dispatcher owns each physical
callback PC. Finite bootstrap-to-first-case and per-active-case watchdogs fail through the same
restore/unlink/clear status path rather than relying on the external timeout; the successful run
leaves no Lua Console error or registered callback and restores only
the scoped menu state, two logical slot payloads/checksums, generated RAM, stack/frame, and session
cart patches. This matrix deliberately adds no payload, checksum, service-result, durable-media, or
player-driven UI claim beyond the existing direct-service owner.

## Confirmed Direct-Service Lifecycle Matrix

**Confirmed:** a separate, one-launch, fourteen-case direct-service matrix exercises `CheckSram`,
`SaveGame`, `LoadGame`, `CopySave`, and `ClearSaveSlotFlag` from a harness-defined work-RAM probe. It
uses the original `CheckSram` return only as bootstrap and does not treat the title or Witch UI as an
observed service caller. The fixture-defined cases cover signature initialization,
empty/valid/invalid slots, both save and load selectors, both copy directions, and both occupied-flag
clears.

The fixture compares all 4,016 logical bytes for each tracked slot using compact checksum,
mismatch-count, boundary, and sentinel facts. It independently checks the full 8,192-logical-byte
signature-mismatch clear, then its 17-byte source-defined checked signature prefix and cleared flags.
Runtime confirms both nested function entries, while the source guard establishes `CopySave`'s
`LoadGame`-then-`SaveGame` order. These are
in-process helper effects only; they do not establish a player path, persistent physical medium, or
recovery policy.

**Confirmed harness boundary:** callback exceptions become a nonzero observer status/exit result with
case, phase, role, and expected/actual PC diagnostics. Shared-PC roles dispatch deterministically.
The accepted run leaves no Lua Console error, no registered callback, and zero logical SRAM residue.

**Unknown:** cross-process survival, power-cycle or torn-write behavior, hardware bus/bank/cycle
details, normal story/church/battle persistence, and player-facing save UI remain outside this direct
matrix and do not reopen ADR 0005 hardware fidelity.

## Confirmed New-game Runtime Matrix

**Confirmed:** one BizHawk 2.11.1 / Genesis Plus GX launch saves a core-state checkpoint after the
original `CheckSram` return and replays four independent New-action cases. Flag preconditions 0, 1,
2, and 0 enter the page-1 menu with observed selector/page/availability `1/1/6`, `2/1/4`, `1/1/2`,
and `1/1/6`; injected page-1 results select slot 1, slot 2, slot 1, and slot 1. Page-3 injected
difficulty results 0/1/2/3 produce flags 78/79 clear/clear, set/clear, clear/set, and set/set. Every
case calls the original `SaveGame` and transfers to `MainLoop` with `CURRENT_MAP`/`EGRESS_MAP` 3 and
D0–D4 `3/56/3/3/1`.

**Confirmed harness boundary:** this observation uses session-only `MD CART` patches after exact
readback proves the same writes through `M68K BUS` did not alter ROM instruction bytes. It injects
both menu returns, bypasses NameAlly and DisplayText, clears player-1 input for the original
configuration helper's Start-clear branch, and pulses C for text waits. It therefore does not establish
what a player sees or chooses while naming or navigating menus. A fixture-owned 4,800-frame deadline
logs a timeout milestone and exits BizHawk with failure before the 120-second Python observer timeout.

## Remake Boundary

A remake can represent two independently addressed save records with explicit valid/occupied state,
an integrity check, and a save-copy/delete workflow. It must choose its own atomic-write, corruption
recovery, platform storage, and completion-state policy if future runtime work observes those
behaviors; none of those choices is established by this source contract.

## Witch Menu Routing Boundary

**Confirmed static contract:** the witch action dispatcher has four word-table indices in this exact
order: New, Load, Delete, Copy. The page-0 action selector receives one of the source masks 1, 6, or
15 after a `SAVE_FLAGS & 3` decision. New uses a page-1 free-slot selector formed by XORing that mask
and shifting left once; Load and Delete use the non-inverted, once-shifted page-2 selector. All three
subtract one from the returned selector before writing `CURRENT_SAVE_SLOT`. Copy masks save flags with
3, subtracts one, and then calls `CopySave` after its source-level nonzero prompt-result branch.

**Confirmed static contract:** New performs its naming/configuration path, writes `GAMESTART_MAP` to
`CURRENT_MAP`/`EGRESS_MAP`, then calls `SaveGame`; it then sets map/X/Y/facing/`d4` for `MainLoop`
(`GAMESTART_MAP`, `GAMESTART_SAVEPOINT_X`, `GAMESTART_SAVEPOINT_Y`, `GAMESTART_FACING`, and `1` in
that order) before branching. Load calls `LoadGame`; source flag operand 88 chooses
between a `j_BattleLoop` path and a `GetSavepointForMap` path, both ending at
`alt_MainLoopEntry`. Delete reaches `ClearSaveSlotFlag` only after its nonzero prompt-result branch is
not taken. These statements preserve source branch polarity and call order; they do not assign a
player-facing meaning to either prompt result.

**Unknown original behavior:** no static evidence here establishes menu timing, input debouncing,
rendered labels, confirmation UX, SRAM durability, or the visible consequences of those loop handoffs.
