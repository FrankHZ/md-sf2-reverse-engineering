# Runtime RNG and Battle-Math Call Chains

- Status: **Confirmed runtime fixtures for RNG, stat gain, turn order, and physical base damage**
- Evidence date: 2026-07-17
- ROM: USA retail, SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`
- Source baseline: `ShiningForceCentral/SF2DISASM` commit
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Evidence Map

Addresses come from the H1 assembler listing produced by the pinned source and independently match
the locked ROM instruction bytes.

| System | Symbol | ROM address/range | Owning source |
| --- | --- | --- | --- |
| RNG | `GenerateRandomNumber` | `0x1600..0x1628` | `code/common/tech/randomnumbergenerator.asm` |
| debug-aware RNG | `GenerateRandomOrDebugNumber` | `0x1674..0x16BE` | same file |
| level up | `LevelUp` | `0x9484` | `code/common/stats/levelup.asm` |
| physical damage | `battlesceneScript_CalculateDamage` | `0xABBE..0xAC4E` | `code/gameflow/battle/battleactions/calculatedamage.asm` |
| critical hit | `battlesceneScript_DetermineCriticalHit` | `0xAC4E..0xACCA` | `code/gameflow/battle/battleactions/determinecriticalhit.asm` |
| turn order | `GenerateBattleTurnOrder` | `0x25544` | `code/gameflow/battle/battleloop/turnorderfunctions.asm` |
| turn entry | `AddCombatantAndRandomizedAgiToTurnOrder` | `0x255A4` | same file |

Relevant RAM symbols are `RANDOM_SEED=$FFDEA4`, `RANDOM_SEED_COPY=$FFDFB0`,
`LEVELUP_ARGUMENTS=$FFAF82`, `BATTLE_TURN_ORDER=$FFF71A`, and
`CURRENT_BATTLE_TURN=$FFF79A` in `disasm/sf2const.asm`.

## Confirmed: Base RNG

Input is `D6.w = range`; output is `D7.w`. The original routine performs:

```text
newSeed = (oldSeed * 13 + 7) & 0xFFFF
RANDOM_SEED = newSeed
result = floor(newSeed * range / 65536)   // for the normal range <= 32767
```

The assembly obtains the result by multiplying `newSeed * (range * 2)`, taking the upper word, then
shifting it right once. D6 is saved and restored around the calculation.

H3 runs the original ROM in BizHawk 2.11.1 / Genesis Plus GX. Lua callbacks fire at entry `0x1600`
and immediately before `RTS` at `0x1626`. At entry the harness confirms the game's natural D6 is
128 and writes the case's 16-bit seed to `$FFDEA4`; at observation it reads the updated seed, D7, and
restored D6. Seven boundary/representative seeds pass the committed fixture
`tests/fixtures/h3/rng-v1.json`.

Reproduce with:

```powershell
pwsh ./scripts/Test-H3RngFixture.ps1
```

Generated Lua and observations are private/derived and remain under `local/derived/h3/`.

## Confirmed Statically and at Runtime: Stat-Gain Randomization

`CalculateStatGain` reads the selected growth curve and calculates the projected growth portion for
the current level. It then sets range 128, adds one RNG result, subtracts a second, adds 128 for
rounding, and shifts right eight bits. After adding that randomized gain to the current stat, it
compares against the rounded expected minimum; if the new value is below that minimum, the returned
gain receives the source-labeled “loser pity bonus” of +1.

The second H3 fixture observes natural startup calls at `CalculateStatGain` entry `0x96BA`, its
no-growth return at `0x96C2`, the pity branch at `0x972C`, and the normal return at `0x9734`. It
controls only the two RNG seeds used by each active growth calculation and records the natural D1-D5
inputs, both RNG results, selected return path, pity-branch execution, and returned D1 gain.

Eighteen ordered cases pass across curve IDs 0–3 and levels 1–3. Coverage includes one curve-None
early return, zero/one/two-point gains, and one observed pity increment. Before launching the
emulator, the verifier independently recomputes every expected result from the H2 growth curves and
the confirmed RNG formula; the runtime golden is therefore tied to both ROM execution and the static
data contract.

```powershell
pwsh ./scripts/Test-H3StatGainFixture.ps1
```

The source also documents a concrete original bug at two class comparisons in `LevelUp`: TORT equals
`CHAR_CLASS_LASTNONPROMOTED`, but the `blt` branch treats it as promoted and adds the 20-level promoted
offset. This is confirmed as executable source/ROM logic, but its player-visible scenarios are not
yet covered by H3. The fixture validates `CalculateStatGain`, not the complete `LevelUp` side effects.
A remake must not silently choose preserve/fix behavior before a design decision.

## Confirmed Statically and at Runtime: Turn-Order Score

Inactive or zero-HP combatants are skipped. For each active combatant:

1. Start from current agility masked to the low seven bits.
2. Set RNG range to `agility >> 3`; add one RNG result and subtract a second.
3. Add `RNG(3) - 1`.
4. Store `(combatant index, altered agility)` as a two-byte entry.
5. If raw agility is at least 128, add a second entry. Its base is
   `floor((agility & 0x7F) * 5 / 6)` and it receives the same add/subtract pair with range `base >> 3`,
   but not the extra `RNG(3)-1` term.

The fixed-size list is bubble-sorted by its signed agility byte in descending order, then
`CURRENT_BATTLE_TURN` is cleared. Tie stability and overflow/signed-edge behavior should be preserved
as explicit fixture cases rather than “cleaned up” from the prose formula.

The first end-to-end turn-order fixture uses the original debug-mode input sequence and built-in
Battle Test UI to enter Battle 01 from reset. Player 2 Start invokes the original debug text-skip
path; no save state, movie, patched ROM, or register write is used. After normal combatant
initialization, the harness writes seed `0x1234` at function entry `0x25544` and observes the sorted
array at `0x2559E`.

Nine entries pass: three placed allies and six Gizmos. Bowie (combatant 0, debug-test AGI 99) sorts
first with score 109; the remaining ordered `(combatant, score)` pairs are `(2,8)`, `(1,6)`,
`(128,6)`, `(133,6)`, `(129,4)`, `(130,4)`, `(131,4)`, and `(132,4)`. The stable equal-score order is
therefore executable evidence for this scenario, while the general tie/overflow boundary still
needs targeted cases.

```powershell
pwsh ./scripts/Test-H3Battle01TurnOrderFixture.ps1
```

A second fixture reuses the same reset-to-Battle-Test input harness, then mutates only four combatant
fields immediately before the original function runs: ally 0 AGI 128, ally 1 AGI 127, ally 2 HP 0,
and enemy 128 X = -1 (`255`). With seed `0x0000`, the original code produces eight entries:

- AGI 128 gives combatant 0 two turns, scored `0` and `255` (signed `-1`).
- AGI 127 produces raw byte score `135` (signed `-121`) for combatant 1, so it sorts after the
  smaller positive byte scores rather than numerically ahead of them.
- dead but placed combatant 2 and living but unplaced combatant 128 are both absent.
- equal positive scores retain source insertion order in the observed list, matching the static
  stable bubble-sort model.

Before launching BizHawk, the verifier independently applies the confirmed LCG, per-combatant RNG
consumption, second-turn formula, byte wrapping, and signed stable sort. Both this model and original
ROM execution must match the committed golden.

```powershell
pwsh ./scripts/Test-H3TurnOrderBoundariesFixture.ps1
```

## Confirmed Statically and at Runtime: Physical Base Damage

`battlesceneScript_CalculateDamage` computes `max(current ATT - current DEF, 1)`, then applies the
target's land-effect setting with integer truncation:

| Land-effect setting | Multiplier |
| --- | --- |
| 0 | `256 / 256` |
| 1 | `230 / 256` |
| other | `205 / 256` |

If the target movement type is flying or hovering and the attacker movement type is brass gunner,
archer, centaur archer, or stealth archer, it then adds `floor(damage / 4)`. Critical, double,
counter, dodge, resistance, status, and the final random damage spread occur in surrounding battle
action routines and are not implied by this base-damage function alone.

The physical-damage H3 fixture reuses the reset-to-Battle-Test harness. Immediately before the
first turn it sets Bowie to the original AI-controlled state, gives him archer movetype and ATT 99,
and places a hovering Gizmo with DEF 20 on adjacent grass. The original AI then selects a normal
attack and naturally calls `battlesceneScript_CalculateDamage`; the harness does not jump to the
routine or write CPU registers.

Callbacks confirm the complete integer path: base damage `99 - 20 = 79`; hovering-on-grass land
effect setting 2 selects multiplier 205; `floor(79 * 205 / 256) = 63`; the archer branch adds
`floor(63 / 4) = 15`; the routine returns 78. A static model recomputes those values before emulator
launch, while runtime callbacks separately prove the land-effect and archer branches executed.

```powershell
pwsh ./scripts/Test-H3PhysicalDamageFixture.ps1
```

This fixture ends at the base-damage return. It does not claim the later random spread, critical,
double, counter, dodge, resistance, HP application, EXP, or death-message behavior.

## Unknown / Next Fixtures

- Extend stat-gain runtime coverage through projection level 30 and promoted effective levels; cover
  the TORT effective-level bug through the complete `LevelUp` caller.
- Extend turn-order coverage beyond the now-confirmed AGI 127/128, second-turn, dead/unplaced,
  signed-byte, and stable-tie scenario to status-effect agility changes and multiple AGI >= 128
  combatants in one round.
- Extend the now-confirmed base-damage, land-effect, and archer-bonus path through random spread,
  critical/double/counter logic, HP application, and damage EXP.
- The gameplay role and isolation guarantees of `RANDOM_SEED_COPY`, which source comments reserve for
  AI, need a traced scenario rather than a name-based assumption.
- The existing `LASER radius = 3` static anomaly remains in the behavior-test queue.
