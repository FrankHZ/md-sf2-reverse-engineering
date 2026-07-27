# Save-System Contract

- **Confirmed original behavior:** the static two-slot SRAM representation, byte-interleaved copy
  direction, additive checksum/check order, occupied-flag operations, save/load/copy/delete helper
  sequence, and witch-menu selector/action routing described below.
- **Unknown original behavior:** physical SRAM persistence, power-loss/partial-write outcomes,
  corruption behavior outside the checked byte checksum, and caller-visible lifecycle/input timing.
- Remake status: implementation-neutral static contract; runtime persistence remains unobserved.
- Evidence date: 2026-07-27
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`
- Traceability: `sf2-tech-services-static-v1` in
  `tests/fixtures/h2/tech-services-static-v1.json`; `src/sf2tool/h2/services.py`; and
  `docs/research/technical-services.md`. The static witch-menu contract is
  `sf2-special-screens-static-v1` in
  `tests/fixtures/h2/special-screens-static-v1.json`; `src/sf2tool/h2/screens.py`; and
  `docs/research/special-screens.md`.

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

## Remake Boundary

A remake can represent two independently addressed save records with explicit valid/occupied state,
an integrity check, and a save-copy/delete workflow. It must choose its own atomic-write, corruption
recovery, platform storage, and completion-state policy after the grouped original-runtime matrix
observes those behaviors; none of those choices is established by this source contract.

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
