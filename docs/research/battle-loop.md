# Battle Loop and Combatant Lifecycle

- Status: **Confirmed** for the pinned 18-file source inventory, representative entry symbols,
  source hashes, static call edges, roster scans, AI-memory reset, remaining-combatant counting,
  forced enemy deaths, between-battle healing, terrain loading, spawn admission, and killed-combatant
  cleanup order
- Status: **Inferred** for broad roles taken only from upstream names/comments
- Status: **Unknown** for upgrade/egress edge cases, spawn/reset failure causes, visual timing, and
  caller-visible interactions not already covered by H3
- Evidence date: 2026-07-18
- ROM: USA retail, SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`
- Source baseline: `ShiningForceCentral/SF2DISASM` commit
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Scope and Inventory

This rail inventories every ASM file under `code/gameflow/battle/battleloop`. It pins each file hash,
global/local labels, direct calls, and one H1-listed representative symbol per file. Existing H3
contracts already own turn order, region activation, and after-turn status behavior; the new H2
inventory connects the remaining lifecycle files without pretending that one symbol means every
instruction is semantically complete.

The directory covers battle initialization, ally/enemy placement, terrain loading, enemy spawn and
upgrade paths, trigger regions, turn order, status processing, death cleanup, victory/egress helpers,
and debug reporting. Static questions are modeled here first; runtime questions accumulate until
they can share one natural caller and one emulator launch.

## Confirmed Static Contracts

The roster-wide loops use 30 ally slots and 32 enemy slots beginning at combatant `128`.
`ClearAiMemory` resets 48 last-target bytes to `0xFF` and 48 memory bytes to zero.
`CountRemainingCombatants` counts only entries with non-negative X and positive current HP, then
forces the returned ally count to zero when combatant 0 has zero HP. The results are returned in
`D2.w` (allies) and `D3.w` (enemies).

`KillRemainingEnemies` clears the dead-combatant list, scans placed living enemies, appends each
combatant index, increments the list length, and then writes current HP zero. The later
`ProcessKilledCombatants` returns immediately for an empty list; otherwise it performs its visual
passes, increments defeats for dead allies or credits enemy kills to `BATTLESCENE_FIRST_ALLY`, clears
X/Y to `-1`, clears status, refreshes derived stats, and moves the entity to `0x7000,0x7000`.

`HealLivingAndImmortalAllies` skips other dead allies but always processes Peter (7) and Lemon (28).
It restores current HP/MP to their maxima, preserves only STUN/POISON/CURSE (mask `0x0007`), then
rebuilds combatant stats. This is a between-battle lifecycle rule, not evidence that ordinary healing
spells behave the same way.

Battle terrain is selected from a four-byte pointer table by current battle ID and decompressed with
the Stack codec into `0xFF5F00`. Spawn admission scans all 32 enemy slots and recognizes initialization
modes `0x0100` (respawn), `0x0200` (hidden/region-triggered), and `0x0300` (both). Successful reset
candidates are written to `TARGETS_LIST`; a reset carry/failure skips the candidate.

## Evidence Limits and Runtime Queue

The inventory confirms source shape and instruction order, not display timing or every caller state.
Turn order, primary/secondary region activation, and after-turn status branches already have focused
H3 fixtures. Upgrade selection, Jaro/egress special cases, spawn reset failures, and death/spawn visual
sequencing remain static or unknown until several questions can share a concentrated runtime matrix.

## Reproduction

```powershell
uv run sf2 h2 battle-loop
uv run sf2 research-index test
```

Generated inventory JSON is written only to ignored `local/derived/battle-loop-static.json`.
