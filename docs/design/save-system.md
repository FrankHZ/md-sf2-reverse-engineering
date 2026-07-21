# Save-System Contract

- **Confirmed original behavior:** the static two-slot SRAM representation, byte-interleaved copy
  direction, additive checksum/check order, occupied-flag operations, and the save/load/copy/delete
  helper sequence described below.
- **Unknown original behavior:** physical SRAM persistence, power-loss/partial-write outcomes,
  corruption behavior outside the checked byte checksum, and caller-visible lifecycle timing.
- Remake status: implementation-neutral static contract; runtime persistence remains unobserved.
- Evidence date: 2026-07-20
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`
- Traceability: `sf2-tech-services-static-v1` in
  `tests/fixtures/h2/tech-services-static-v1.json`; `src/sf2tool/h2/services.py`; and
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

## Remake Boundary

A remake can represent two independently addressed save records with explicit valid/occupied state,
an integrity check, and a save-copy/delete workflow. It must choose its own atomic-write, corruption
recovery, platform storage, and completion-state policy after the grouped original-runtime matrix
observes those behaviors; none of those choices is established by this source contract.
