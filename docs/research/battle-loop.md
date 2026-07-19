# Battle Loop and Combatant Lifecycle

- Status: **Confirmed** for the pinned 18-file lifecycle and 9-file top-level control inventories,
  representative entry symbols,
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

The two rails inventory every ASM file under `code/gameflow/battle/battleloop` plus all nine ASM files
directly under `code/gameflow/battle`. They pin each file hash, global/local labels, direct calls, and
one H1-listed representative symbol per file. Existing H3 contracts already own turn order, region
activation, and after-turn status behavior; H2 connects the remaining control/lifecycle files without
pretending that one symbol means every instruction is semantically complete.

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
the Stack codec into `0xFF5F00`. The project-owned decoder confirms all 45 selections, including the
two aliases, and all 43 unique payloads as fixed 48×48 grids containing only terrain 0-8 or `0xFF`.
Spawn admission scans all 32 enemy slots and recognizes initialization
modes `0x0100` (respawn), `0x0200` (hidden/region-triggered), and `0x0300` (both). Successful reset
candidates are written to `TARGETS_LIST`; a reset carry/failure skips the candidate.

## Top-Level Battle Control

`BattleLoop` has separate suspended and new-battle entries. A suspended battle restores the saved
seconds counter, clears flag 88 and AI memory, reloads state, and resumes the individual-turn loop.
A new battle clears elapsed seconds, runs the before/start cutscenes, clears region flags 90–105,
heals the living/immortal party, initializes both rosters, and loads the battle. Each round then runs
enemy activation, the region cutscene, spawn admission/animation, and turn-order generation.

After an action, the loop processes deaths and checks both factions before applying after-turn
effects; it processes deaths and checks both factions again afterward. A `0xFF` turn-order entry
restarts the round. Victory heals the party, runs the after-battle cutscene, clears the unlocked flag,
sets the completed flag at offset +100, and returns `D4=1`. Defeat restores the leader's HP, halves
gold with unsigned floor division, obtains the egress position, and normally returns `D4=-1`; the
hardcoded battle-4 loss path completes/upgrades that battle and returns `D4=0`.

Difficulty is the weighted sum of flags 78 and 79 (weights 1 and 2), producing 0–3. Battle spriteset
subsections are sizes, allies, enemies, regions, and AI points; entity and region entries are both 12
bytes. An absent combatant starting entry returns `(-1,-1)`. Battle VInt setup clears the previous
list and installs seven ordered map/entity/view/scroll/sprite/window/animation updates.

Map music is preserved outside battle. In battle, music IDs 0/8/14 map to battle theme 3, while
40/38 map to battle theme 1. The laser helper rejects non-laser battles or facing `-1`; otherwise it
marks a ray to the map edge and appends every occupying combatant. The function at `0x1B120A` is
tracked as unused/debug evidence and ends in a self-loop; it is not a gameplay contract.

## Evidence Limits and Runtime Queue

The inventory confirms source shape and instruction order, not display timing or every caller state.
Turn order, primary/secondary region activation, and after-turn status branches already have focused
H3 fixtures. Upgrade selection, Jaro/egress special cases, spawn reset failures, suspended-battle
persistence, laser table content, and death/spawn visual sequencing remain static or unknown until
several questions can share a concentrated runtime matrix.

## Reproduction

```powershell
uv run sf2 h2 battle-loop
uv run sf2 h2 battle-control
uv run sf2 h2 battle-terrain
uv run sf2 research-index test
```

Generated inventory JSON is written only to ignored `local/derived/battle-loop-static.json` and
`local/derived/battle-control-static.json`.
