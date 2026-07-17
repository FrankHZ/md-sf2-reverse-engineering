# Runtime RNG and Battle-Math Call Chains

- Status: **Confirmed runtime fixtures for RNG, stat gain, turn order, and the physical attack chain**
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
| damage application | `battlesceneScript_InflictDamage` | `0xACEA..0xAE32` | `code/gameflow/battle/battleactions/inflictdamage.asm` |
| final EXP | `battlesceneScript_GiveExpAndGold` | `0xA7F8..0xA870` | `code/gameflow/battle/battleactions/giveexpandgold.asm` |
| enemy reaction | `bsc0A_executeEnemyReaction` | `0x18F4E..0x190A4` | `code/gameflow/battle/battlescenes/battlesceneengine_0.asm` |
| EXP replay | `bsc0F_giveExp` | `0x190DC..0x191E0` | same file |
| dodge | `battlesceneScript_DetermineDodge` | `0xAAFC..0xABBE` | `code/gameflow/battle/battleactions/determinedodge.asm` |
| double/counter | `battlesceneScript_DetermineDoubleAndCounter` | `0xB00E..0xB080` | `code/gameflow/battle/battleactions/determinedoubleandcounter.asm` |
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
action routines and are not implied by this base-damage function alone; the companion application
fixture observes the next two routines separately.

The physical-damage H3 fixture reuses the reset-to-Battle-Test harness. Immediately before the
first turn it sets Bowie to the original AI-controlled state, gives him archer movetype and ATT 99,
and places a hovering Gizmo with DEF 20 on adjacent grass. The original AI then selects a normal
attack and naturally calls `battlesceneScript_CalculateDamage`; the harness does not jump to the
routine or write CPU registers.

Callbacks confirm the base-function integer path: base damage `99 - 20 = 79`; hovering-on-grass land
effect setting 2 selects multiplier 205; `floor(79 * 205 / 256) = 63`; the archer branch adds
`floor(63 / 4) = 15`; the routine returns 78. A static model recomputes those values before emulator
launch, while runtime callbacks separately prove the land-effect and archer branches executed.

The same natural attack now continues through critical and `InflictDamage`. The harness sets Bowie's
current prowess low nibble to definition 0 (1/32 chance, +50% damage) and writes seed 0 only at
`battlesceneScript_DetermineCriticalHit` entry. The original RNG returns 0, so 78 becomes
`78 + floor(78/2) = 117`. Damage variance then uses range
`floor(117/8) + 1 = 15`; the two following original RNG calls both return 0, leaving final damage 117.

The target begins with max/current HP 100. `DecreaseCurrentHp` clamps it to zero, sets the stack-frame
death flag, and writes a signed -117 reaction command. Equal actor/target levels select a 50-point
kill value. Damage EXP computes `floor(50 * 117 / 100) = 58` and immediately reaches the per-action
cap of 49; adding kill EXP remains capped at 49. The committed application fixture and independent
PowerShell model lock every intermediate value and the relevant instruction addresses.

The separate lethal-validation fixture reuses the naturally selected Battle 01 attack with 10 target
HP, making the already-confirmed 18-point lower damage bound lethal. At the validation boundary the
harness sets both follow-up toggles true, isolating the original rejection code from the separately
confirmed prowess/RNG decision. With `targetDies` already true,
`battlesceneScript_ValidateDoubleAttack` clears double at `0xA486`, and
`battlesceneScript_ValidateCounterAttack` clears counter at `0xA538`. Both are true at entry and
false at return; only the lethal first damage calculation executes.

The counter-range companion keeps the target alive at 182 HP after the same 18-point first hit,
disables double, and supplies a true counter toggle. Immediately before the original counter
validator runs, it moves Bowie from the adjacent tile to `(0,0)` while the Gizmo remains at `(8,17)`.
The observed Manhattan distance is 25. With the ordinary opposing-side, conscious Gizmo and no
special-enemy exclusion changed, `battlesceneScript_ValidateCounterAttack` clears the counter at
`0xA538`; no second damage calculation executes. This isolates the out-of-range rejection branch.

The counter-sleep companion keeps the same 182 HP nonlethal target, disabled double, and true
counter toggle. Immediately before validation, it writes status word `0x00C0` at combatant offset
44. The original `GetStatusEffects` returns `STATUSEFFECT_SLEEP`, so
`battlesceneScript_ValidateCounterAttack` clears the counter at `0xA538`; only the first damage
calculation executes. This isolates the sleeping-target rejection from range and death checks.

The counter-stun companion instead writes status word `0x0001`. It passes the original sleep mask,
then the second `GetStatusEffects` call returns `STATUSEFFECT_STUN`; the same clear path rejects the
otherwise eligible live adjacent counter with one total damage calculation.

The counter-same-side companion leaves the opposing Battle 01 combatants unchanged and observes the
natural `targetIsOnSameSide` stack flag as false. Immediately before counter validation it changes
only that flag to true. The original early check clears the forced-valid counter, leaving the target
at 182 HP with one damage calculation. This confirms the validator branch without claiming that the
ordinary action selector naturally schedules an ally-on-ally physical attack.

The counter-Burst-Rock companion observes the prospective counterattacker's natural enemy index 39
(Gizmo), then changes only combatant offset 55 to enemy index 32 (Burst Rock). The original
`GetEnemy` call sees Burst Rock and clears the counter before range evaluation; target HP remains
182 with one damage calculation. In the actual call convention `a4` points at the original attack
target, so this runtime result also resolves the nearby source comment's ambiguous direction: Burst
Rock is prevented from counterattacking.

The special-enemy matrix completes the hard-coded list. Changing the prospective counterattacker
from Gizmo to Kraken Head (87), Prism Flower (93), or Zeon Guard (38) clears counter in all three
cases. The Taros case instead changes the original-attacker pointer from Bowie to enemy combatant
129 and its enemy index from Gizmo to Taros (88); that also clears counter. Thus Taros cannot be
countered, while Burst Rock, Kraken Head, Prism Flower, and Zeon Guard cannot counter. Every case
keeps the target alive at 182 HP and observes one damage calculation. The source comparison names
Kraken Head, not Kraken Arm; the runtime matrix deliberately follows the exact enum and address.

The double-validation matrix completes the corresponding smaller validator. Both cases keep the
target alive at 182 HP and force double true while disabling counter. The natural `muddledActor` and
`targetIsOnSameSide` flags are false. Setting either one true immediately at entry `0xA45E` makes the
original code clear double at `0xA486`; at return `0xA49C`, double is false and only the first damage
calculation has run. Together with the lethal fixture, these are all non-debug rejection checks in
`battlesceneScript_ValidateDoubleAttack`.

```powershell
pwsh ./scripts/Test-H3PhysicalDamageFixture.ps1
pwsh ./scripts/Test-H3LethalFollowupFixture.ps1
pwsh ./scripts/Test-H3CounterRangeFixture.ps1
pwsh ./scripts/Test-H3CounterSleepFixture.ps1
pwsh ./scripts/Test-H3CounterStunFixture.ps1
pwsh ./scripts/Test-H3CounterSameSideFixture.ps1
pwsh ./scripts/Test-H3CounterBurstRockFixture.ps1
pwsh ./scripts/Test-H3CounterSpecialEnemiesFixture.ps1
pwsh ./scripts/Test-H3DoubleValidationFixture.ps1
```

The application snapshot at `0xAD92` is deliberately not treated as the final state. During
`battlesceneScript_End`, the game first turns the 49-point accumulator into a `giveEXP` command.
Battle 01 appears in `table_HalvedExpEarnedBattles`, so integer halving yields 24. Starting from the
post-variance seed 1281, both subsequent `RNG(16)` results are 4; neither the +1 nor -1 branch fires,
and the command retains 24. The routine then restores the saved target HP snapshot from 0 to 100.

The command interpreter proves why that restoration is not healing. `bsc0A_executeEnemyReaction`
reads combatant 128 and signed HP change -117 from the generated command, calls the original clamped
decrease routine, and leaves persistent current HP at 0. Later, `bsc0F_giveExp` reads command amount
24 and changes Bowie's stored EXP from 0 to 24. The harness allows the original battle scene to run
and uses normal debug text-skip input during playback; it does not patch the command list or invoke
either interpreter directly. `tests/fixtures/h3/battle-scene-replay-v1.json` and the same independent
RNG model validate restoration, both EXP rolls, signed reaction, persistent HP, and final EXP.

The damage/replay fixture alone does not claim dialogue/death-animation semantics, level-up after
EXP >= 100, double, counter, dodge, resistance, or status behavior; the next fixture owns the
non-dodge double/counter chain.

## Confirmed: Double Attack and Counter Chain

The attack-chain fixture uses the same natural Battle 01 AI action but makes both combatants
nonlethal at 200 HP. Bowie has ATT 50, DEF 30, ground movetype, and prowess `0x38`; the Gizmo has
ATT 40, DEF 20, hovering movetype, and prowess `0xC8`. Low critical nibbles are 8 (`NONE`). The
double field in `0x38` and counter field in `0xC8` are both setting 3, selecting a 1/4 RNG range.

In the chain scenario, seed `0xFFFF` at each dodge RNG boundary produces nonzero rolls. Because the target is hovering and
Bowie is not an archer, the first and second attacks use airborne-target dodge range 8 and roll 7.
The counter targets a ground combatant and uses default range 32, rolling 31. These calls confirm the
range selection and non-dodge path; they do not yet execute the successful dodge branch.

At the first `DetermineDoubleAndCounter` entry, seed 0 makes both range-4 rolls zero, setting both
stack-frame toggles. The original engine validates them and constructs three attacks in order:

1. First attack (type 0): base 24, spread rolls 3 and 3, final damage 18.
2. Second attack (type 1): the same calculation, reducing temporary enemy HP from 182 to 164.
3. Counter (type 2): enemy ATT 40 minus ally DEF 30 gives 10; `InflictDamage` halves it to 5 before
   its range-1 spread, reducing temporary ally HP from 200 to 195.

`battlesceneScript_End` restores both snapshots to 200 before playback. The command interpreter then
replays enemy reactions `-18, -18` and ally reaction `-5`, leaving persistent enemy/ally HP 164/195.
The verifier independently models dodge ranges, all LCG transitions, land reduction, counter
halving, spread, temporary HP, restoration, and ordered reaction replay before launching BizHawk.

This scenario confirms one valid adjacent counter and one double attack. Natural muddled/same-side
and special-enemy action reachability before the now-confirmed validation seams, plus whether a
second/counter critical can occur, remain separate fixture cases.

The companion successful-dodge fixture keeps the same non-archer-versus-hovering geometry but sets
seed 0 at the range-8 dodge call. The original routine returns 0, writes the dodge stack flag, and
branches around `battlesceneScript_CalculateDamage`; an execution counter at `0xABBE` remains zero.
Follow-up double/counter settings both use range 32 with seed `0xFFFF`, producing rolls 31 and 31,
so no later attack is scheduled. Both combatants remain at 100 HP. This locks the successful dodge
control-flow contract independently of the earlier non-dodge observations.

## Unknown / Next Fixtures

- Extend stat-gain runtime coverage through projection level 30 and promoted effective levels; cover
  the TORT effective-level bug through the complete `LevelUp` caller.
- Extend turn-order coverage beyond the now-confirmed AGI 127/128, second-turn, dead/unplaced,
  signed-byte, and stable-tie scenario to status-effect agility changes and multiple AGI >= 128
  combatants in one round.
- Add natural muddled/same-side/special-enemy action reachability, resistance, additional non-critical
  variance seeds, and an EXP-caused level-up to the confirmed physical path.
- The gameplay role and isolation guarantees of `RANDOM_SEED_COPY`, which source comments reserve for
  AI, need a traced scenario rather than a name-based assumption.
- The existing `LASER radius = 3` static anomaly remains in the behavior-test queue.
