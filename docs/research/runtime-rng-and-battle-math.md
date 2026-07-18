# Runtime RNG and Battle-Math Call Chains

- Status: **Confirmed runtime fixtures for RNG, level up/stat refresh, turn order, physical attacks, and the first spell-damage matrix**
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
| derived-stat refresh | `UpdateCombatantStats` | `0x89CE..0x8A26` | `code/common/stats/updatecombatantstats.asm` |
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
| spell resistance | `GetResistanceToSpell` | `0xC22A..0xC24E` | `code/gameflow/battle/battleactions/getresistancetospell.asm` |
| spell damage | `battlesceneScript_CalculateSpellDamage` | `0xBB02..0xBB56` | `code/gameflow/battle/battleactions/calculatespelldamage.asm` |

Relevant RAM symbols are `DEBUG_MODE_TOGGLE=$FFB0A9`, `PLAYER_1_INPUT=$FFDE97`,
`RANDOM_SEED=$FFDEA4`, `RANDOM_SEED_COPY=$FFDFB0`,
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
uv run sf2 h3 rng
```

Generated configuration and observations are private/derived and remain under `local/derived/h3/`;
the reusable observer is tracked at `tools/bizhawk/rng_observer.lua`.

## Confirmed: Debug-Aware RNG Override

`GenerateRandomOrDebugNumber` accepts the range in D0.w and returns the result in D0.w while
preserving D6 and D7. With `DEBUG_MODE_TOGGLE` disabled, controller directions are ignored and the
routine falls through to `GenerateRandomNumber`. With debug enabled, the first matching direction
returns a fixed value without consuming `RANDOM_SEED`: Right → 0, Up → 1, Left → 2, Down → 3. The
branch order gives Right highest priority when multiple direction bits are set. Debug enabled with
no direction also falls through to the normal RNG.

The companion H3 fixture enters the original Battle Test from reset using controller input and
observes seven natural battle-action calls at entry `0x1674`, fallback `0x16B2`, and restored return
boundary `0x16BC`. It covers all four direction results, all-directions priority, debug/no-direction
fallback, and debug-disabled/direction fallback. For seed `0x1234`, both fallback cases advance the
seed to `0xECAB`; their natural range 32 produces 29. Direction overrides leave the seed unchanged,
and every case preserves the caller's full D6/D7 values. The executable contract is
`tests/fixtures/h3/debug-rng-v1.json`.

This is original debug tooling, not a player-facing combat rule or an automatic remake requirement.
It remains valuable harness evidence because many battle-action callers use the wrapper and because
the override explains controller-dependent results observed in the built-in Battle Test.

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
uv run sf2 h3 growth
```

The companion complete-caller fixture observes natural startup execution for Kazin/MAGE and
Kiwi/TORT. It controls only `RANDOM_SEED` at each selected `LevelUp` entry, then records the initial
stats, five applied gains, final seed, level, and seven-byte `LEVELUP_ARGUMENTS` payload. A Python
model independently parses the pinned growth curves, ally start definitions, class equates, and both
ally stats blocks before BizHawk runs.

Kazin is the base-class control: initialization effective level remains 4 and the first level-up
spell threshold remains 2. Kiwi confirms the original defect at both class comparisons. TORT equals
`CHAR_CLASS_LASTNONPROMOTED` (11), but the `blt` skips only classes below 11, so initialization changes
effective level 7 → 27 at `0x9628` and the first level-up changes 2 → 22 at `0x957A`. Kiwi has no TORT
spell list, so this scenario proves the internal threshold defect without claiming a learned-spell
side effect. Its applied gains are HP/MP/ATT/DEF/AGI `[1,0,1,1,1]`, producing payload
`[2,1,0,1,1,1,255]`. The remake decision boundary is specified in
[`../design/level-up.md`](../design/level-up.md).

A third growth fixture observes seven more natural startup `LevelUp` calls. At entry it controls the
selected combatant's class, level, base stats, spells, and seed, without changing the PC or CPU
registers. Its independent model parses the same pinned curves, equates, every ally stats block in
ROM order, and inherited spell lists before BizHawk runs. Callbacks at `0x94B2` and `0x94B8` record
negative sentinels and exact matching class-block addresses.

Randolf/GLDT confirms the post-projection path: from level 30 and stored projected stats
`[89,0,53,104,52]`, seed `0x1234` produces gains `[2,0,2,1,2]`, level 31, and final seed `0x621C`.
Gyan/GLDT confirms the same gains at level 98→99, immediately before the promoted cap.
Slade/THIF at base level 40 and Chaz/WIZ at promoted level 99 both exit at `0x94C6`, preserve the
input seed/state, and write the no-level payload `[255,0,0,0,0,0,255]`. Kazin/WIZ confirms the
promoted effective level 22 and `$FE` inheritance of his first stats block's spell list: BLAZE 1
(`0x0B`) upgrades to BLAZE 3 (`0x8B`), with payload `[2,2,1,0,1,3,139]`.

Peter forced to WIZ demonstrates that the class scan has no per-ally bound: after Peter's PHNK/PHNX
records fail to match, it continues through contiguous ally data and finds Tyrin's WIZ block at
`0x1EE653`. Its `$FE` control redirects spell lookup to Peter's first block, whose list is empty.
Claude forced to SDMN has no later matching block; the scan reaches the final negative sentinel,
preserves state/seed, and writes the no-level payload. This separates the actual sentinel exit from
the initially plausible but incorrect assumption that a missing local class exits immediately.

A fourth growth fixture controls Slade's complete THIF combatant entry at level 39 and follows the
original call at `0x95BA` into `UpdateCombatantStats` at `0x89CE`, observing its return at `0x8A24`.
Seed `0x1234` gives level 40 and
base gains `[2,0,2,1,2]`. Current HP remains 7 while max HP changes 42→44; stale current derived
values are reset from base/class values, then the equipped Short Knife's source-defined +5 ATT is
reapplied, producing base/current ATT 47/52. DEF, AGI, MOV, resistance, and prowess likewise refresh
to 39, 40, 7, 0, and `0x13`; items, spells, status, and EXP remain unchanged. `LEVELUP_ARGUMENTS`
is intentionally not asserted by this fixture because surrounding startup initialization shares
that scratch area; the complete-caller and boundary fixtures already own its in-call payload.

The second refresh run uses status `0xFC01` (maximum ATTACK/BOOST/SLOW counters plus STUN) and an
equipped Thieve's Dagger. The original computes the three status fractions independently from base
stats with floor division, applies STUN next, and equipment last. The observed current result is
ATT/DEF/AGI/MOV `81/39/40/6`: BOOST and SLOW cancel at equal counters, STUN's AGI -5 is restored by
the dagger's AGI +5, and the dagger's ATT +17 follows ATTACK's floor(47×3/8)=17 bonus.

Three more independent runs close adjacent branches. ATTACK `1/8`, BOOST `2/8`, and SLOW `1/8`
produce current ATT/DEF/AGI `52/44/45` from bases `47/39/40`, proving counter magnitudes and separate
flooring. Equipped Black Ring + Short Knife stack ATT +10/+5, then the cursed definition adds status
`0x0004`. Finally, Slade/NINJ at 98→99 with Ninja Katana gains `[2,2,1,2,2]`, reaches current ATT 93,
and changes prowess `0x94→0x24`: `INCREASE_DOUBLE` raises double from 1/16 to 1/8 but its original
mask also clears NINJ's counter 1/8 bits to the 1/32 encoding.

The companion initialization-prowess fixture observes Karna's unmodified new-game call. Her PRST
start level 24 makes HEAL 3's threshold 22 eligible during the preliminary spell scan. At `0x967A`
the original takes the dedicated HEAL 3 branch; after `SetBaseProwess`, the callback at `0x969E`
records base prowess `0x03→0x13`, upgrading double attack from 1/32 to 1/16 while leaving the natural
critical and counter settings unchanged. The branch itself does not call `LearnSpell`; the following
23-call `LevelUp` replay learns HEAL 3 at effective level 22 through the ordinary path.

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
uv run sf2 h3 battle-exp
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

The connected battle-EXP fixture starts Bowie/SDMN at level 1 and 99 EXP, with source-modeled base
stats and no equipment. The same natural Battle 01 damage path generates the 24-point command.
`bsc0F_giveExp` first calls `IncreaseExp`, producing 123, then compares current EXP with 100,
stores the remainder 23, and calls `LevelUp` exactly once. Seed `0x1234` at the genuine `LevelUp`
entry yields payload `[2,2,0,1,1,1,255]` and final base stats HP/MP/ATT/DEF/AGI
`14/8/7/5/5`; the final RNG seed is `0xC4DE`. Current HP/MP remain `12/8`, while the deliberately
high action-only ATT/AGI values are refreshed to the new unmodified bases `7/5`. The observer exits
only at `bsc0F_giveExp` return `0x191DE`, so the final snapshot owns persistent state rather than an
internal LevelUp scratch boundary.

`tests/fixtures/h3/battle-exp-level-up-v1.json` is independently modeled from the pinned EXP routine,
LCG, Bowie growth block, five growth curves, and empty-equipment refresh. It confirms this one
99 + 24 transition and its side effects; dialogue/death-animation timing, other EXP adjustments,
multiple-threshold/cap edges, and status behavior remain outside it.

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

## Confirmed: BLAZE 2 Fire-Resistance Matrix

The first spell-damage fixture uses Battle 01's original battle-action engine rather than jumping
to the calculation routine. At `WriteBattlesceneScript` entry `0x9B92`, it replaces Bowie's already
scheduled AI attack with action type 1 and spell entry `BLAZE|LV2` (`0x4B`). The original property
initializer decodes base spell 11 and the original cast dispatcher calls `GetResistanceToSpell` and
`battlesceneScript_CalculateSpellDamage` once per target.

To cover all resistance branches in one emulator boot, the harness supplies the ordered target list
`[128,129,130,131]` immediately after the original target-selection call and before
`battlesceneScript_InitializeBattlesceneProperties` at `0x9F28`. It gives those targets current FIRE
resistance words `0x0000`, `0x0040`, `0x0080`, and `0x00C0`. This is a controlled resolution seam:
the fixture confirms resistance extraction and subsequent action behavior, but does not claim that
this four-target geometry was naturally selected.

The pinned BLAZE 2 definition supplies power 10. For the first three calls Bowie remains
unpromoted SDMN, so `AdjustSpellPower` returns 10 at `0xBB16`; `floor(10/4)` is 2. Immediately before
the fourth genuine calculation call, the harness changes only the caster class to the first
promoted value 12. `AdjustSpellPower` then executes its original `mulu #5` and right shift, yielding
`floor(10*5/4)=12` with quarter 3. This controlled seam confirms the class threshold and arithmetic,
not a naturally promoted full action. Runtime observations at `0xBB32` confirm:

| FIRE setting | Operation | Damage before critical |
| --- | --- | --- |
| neutral 0 | unchanged | 10 |
| minor 1 | `10 - 2` | 8 |
| major 2 | `floor(10/2)` | 5 |
| weakness 3, promoted call | `12 + 3` | 15 |

The first three targets reset `RANDOM_SEED` to `0x1234` at the genuine spell-damage entry. BLAZE's
original `GenerateRandomOrDebugNumber(32)` returns 29, so no critical occurs. The fourth resets seed
0; the original call returns 0, writes the stack-frame critical flag `0xFF`, and adds quarter 3 to
change `15 -> 18`. The shared `InflictDamage` path derives spread ranges `[2,2,1,3]`; both original
variance rolls are zero in every case, preserving final damages `[10,8,5,18]`. Temporary HP becomes
`[90,92,95,82]`.
After all four reactions have been generated, `battlesceneScript_End` restores all four HP snapshots
to 100 before `0x9DCE`, while Bowie's MP is still 20. The observer then lets the original command
interpreter run with debug text skip. `bsc0B_executeAllyReaction` consumes the generated `(HP 0,
MP -6)` command first and changes Bowie's MP `20 -> 14`. Four ordered
`bsc0A_executeEnemyReaction` calls consume HP changes `[-10,-8,-5,-18]`, producing persistent target
HP `[90,92,95,82]`. The final snapshot is delayed until `ExecuteBattlesceneScript` reaches its end
marker at `0x183EA`, so it is distinct from both temporary calculation and restoration state.

The same original action also confirms attack-spell EXP rather than stopping at damage. With every
combatant fixed at level 1 and target max HP 100, `battlesceneScript_CalculateDamageExp` scales the
unpromoted kill value 50 by final damage. The first three calls add `[5,4,2]`, moving
`BATTLESCENE_EXP` through `[5,9,11]`. The controlled promoted fourth call has effective level 21
against level 1, so its kill value is zero and the accumulator remains 11. At
`battlesceneScript_GiveExpAndGold`, Battle 01 halves this to 5. The remaining seed `0x0501` produces
range-16 rolls 4 and 4, so neither random adjustment fires and the command carries 5 EXP.
`bsc0F_giveExp` persistently changes Bowie EXP `0 -> 5`.

The Python verifier independently checks the pinned BLAZE 2 power/cost, FIRE element, resistance
arithmetic, LCG calls, spread, temporary HP, and restored HP before launching BizHawk. Reproduce the
single-boot matrix with:

```powershell
uv run sf2 h3 spell-damage
```

The tracked contract is `tests/fixtures/h3/spell-damage-resistance-v1.json`; generated configuration
and observations remain under ignored `local/derived/h3/`.

## Confirmed: DAO Target-Count Power Division

The companion DAO fixture reuses the same tracked Battle Test choreography and observer but owns a
separate case/golden. It replaces the scheduled action with DAO 1 (base spell 29), supplies four
neutral-resistance targets, and keeps the caster at promoted class value 12 for all calls. The
pinned spell definition supplies power 18 and MP cost 8.

At `AdjustSpellPower`, the original promoted branch executes before the invocation check:
`floor(18*5/4)=22`. Each of the four calls then reaches `0xBBA0`, reads
`TARGETS_LIST_LENGTH=4`, and executes `divu`, producing 5. The observer records four division-entry
hits and four adjusted-power returns of 5. Seed `0x1234` makes each BLAST-family critical roll 29;
range-1 variance returns zero twice, so each target temporarily reaches 95 HP, restores to 100, and
then persistently replays to 95. The ally reaction applies DAO's MP cost as `20 -> 12`.

All four DAO damage-EXP calls see promoted effective level 21 against level 1 and leave the action
accumulator at zero. Its final damage seed `0x3D45` produces award rolls 1 and 7; after Battle 01
halving and unchanged randomization, the original minimum clamps the command to 1. Playback changes
Bowie EXP `0 -> 1`. This independently covers the zero-accumulator/minimum-one boundary in the same
shared observer boot.

This confirms the exact promoted-before-division order and integer truncation for DAO with four
targets. APOLLO, NEPTUN, and ATLAS are statically routed through the same branch but remain untested
at runtime. Reproduce with:

```powershell
uv run sf2 h3 spell-summon
```

The case lives in `tests/fixtures/h3/spell-summon-division-v1.json` and explicitly names the shared
Battle Test fixture from which its unchanged runtime addresses are inherited.

## Confirmed: HEAL 1 Recovery, Healing EXP, and Replay

The HEAL fixture reuses the Battle Test boot but isolates a self-recovery action. It first keeps
Bowie as full-HP SDMN and creates one live enemy scheduling target so the original AI reaches
`WriteBattlesceneScript`. At that seam it replaces the scheduled attack with HEAL 1, selects Bowie
as the sole target, and changes current HP to 95. At the genuine `spellEffect_Heal` entry it changes
the caster class to PRST (4), which is one of the three hard-coded classes accepted by
`battlesceneScript_CalculateHealingExp`. This does not claim a natural Bowie HEAL cast.

The pinned HEAL 1 definition supplies power 15 and MP cost 3. `AdjustSpellPower` leaves the
unpromoted power unchanged. Missing HP is 5, so the original comparison at `0xB144` caps d6 from 15
to 5 before emitting the ally reaction. HP and MP remain 95/20 during scene construction.

Healing EXP computes `floor(25 * 5 / 100) = 1`, raises it to the minimum 10, and writes 10 to
`BATTLESCENE_EXP`. At `0xA804`, the observed same-side stack flag is nonzero; the original branches
directly to randomization and does not apply Battle 01's normal halving table. Seed `0x1234`
produces range-16 rolls 14 and 0, so only the second adjustment fires and the EXP command becomes 9.
Playback executes `(HP 0, MP -3)`, then `(HP +5, MP 0)`, then `giveEXP 9`, leaving persistent state
HP/MP/EXP `100/17/9`.

The fixture, schema, independent source model, and observer are
`tests/fixtures/h3/spell-healing-v1.json`,
`schemas/h3-spell-healing-fixture.schema.json`, `src/sf2tool/h3/spell_healing.py`, and
`tools/bizhawk/spell_healing_observer.lua`. Reproduce it with:

```powershell
uv run sf2 h3 spell-healing
```

## Confirmed: SLEEP 1 STATUS-Resistance Matrix

The SLEEP fixture replaces Bowie's scheduled Battle 01 attack with SLEEP 1 and supplies four enemy
targets carrying STATUS resistance words `0x0000`, `0x4000`, `0x8000`, and `0xC000`. The original
`GetResistanceToSpell` rotation extracts settings `[0,1,2,3]`. This is the same controlled target
list seam used by the damage matrix, not naturally selected map geometry.

At each `spellEffect_Sleep` entry the observer resets `RANDOM_SEED` to `0x1234`. The effect adds its
base chance constant 5 to the resistance setting, and
`battlesceneScript_DetermineSpellEffectiveness` obtains range-8 roll 7. Runtime thresholds
`[5,6,7,8]` therefore produce `[success,success,success,failure]`. The threshold-8 case reaches
`0xBACC`, writes the no-effect toggle, unwinds the caller, and never emits a reaction or status EXP.
This dynamically confirms that STATUS setting 3 is immunity for SLEEP 1.

Each successful call reaches `battlesceneScript_AddStatusEffectSpellExp` and adds 5, yielding
accumulator states `[5,10,15,15]`. The targets remain status-free during construction. Battle 01
halves 15 to 7; final effectiveness seed `0xECAB` then produces award rolls 0 and 3, adding one and
emitting 8 EXP. During replay, the caster MP reaction changes `20 -> 16`, three enemy reactions set
status words to `0x00C0`, the immune target remains zero, and `giveEXP` changes Bowie `0 -> 8`.

The status observer distinguishes the existing HP-applied checkpoint `0x18F7E` from the later
`SetStatusEffects` return `0x18F9A`; only the latter owns persistent status. Reproduce with:

```powershell
uv run sf2 h3 spell-status
```

Tracked artifacts are `tests/fixtures/h3/spell-status-sleep-v1.json`,
`schemas/h3-spell-status-sleep-fixture.schema.json`, `src/sf2tool/h3/spell_status.py`, and
`tools/bizhawk/spell_status_observer.lua`.

## Confirmed: DESOUL 1 Instant Death and Kill Rewards

The DESOUL fixture supplies one neutral-STATUS enemy target and resets seed `0x1234` at the genuine
`spellEffect_Desoul` entry. Threshold 5 and range-8 roll 7 reach the shared success return. The
effect writes enemy reaction HP word `0x8000`, then calls
`battlesceneScript_AddExpAndGoldForKill` and sets stack-frame `targetDies` to `0xFF`.

The target is explicitly enemy definition 0 (OOZE), level 1, max/current HP 100. `GetKillExp`
returns 50 for level-1 unpromoted Bowie versus level 1, and the per-action cap stores 49. The OOZE
gold table entry adds 10 to `BATTLESCENE_GOLD`. Neither construction step changes persistent target
HP or caster MP.

Battle 01 halves 49 to 24. Final effectiveness seed `0xECAB` produces award rolls 0 and 3, so the
first adds one and the command carries 25 EXP; gold remains 10. Playback changes MP `20 -> 12`,
interprets reaction word `0x8000` as signed `-32768` and reduces target HP `100 -> 0`, changes EXP
`0 -> 25`, and changes `CURRENT_GOLD` `0 -> 10` through `IncreaseGold`.

Reproduce the fixture with:

```powershell
uv run sf2 h3 spell-desoul
```

Tracked artifacts are `tests/fixtures/h3/spell-desoul-v1.json`,
`schemas/h3-spell-desoul-fixture.schema.json`, `src/sf2tool/h3/spell_desoul.py`, and
`tools/bizhawk/spell_desoul_observer.lua`.

## Confirmed: SPOIT Random MP Absorption and Replay Order

The SPOIT fixture replaces Bowie's scheduled Battle 01 attack with spell entry 15 and supplies one
enemy target with current/max MP 2/2. SPOIT's pinned definition has MP cost 0. At the genuine
`spellEffect_AbsorbMp` entry `0xB5D6`, seed `0x1234` produces 2 from `RNG(3)`; the effect adds 3,
making an unclamped transfer of 5. The observation at `0xB5EC` confirms the source clamp reduces
that value to the target's current MP, 2. The effect then reaches the shared status-EXP routine and
adds 5. Actor/target MP remain 10/2 when scene construction returns.

The replay trace preserves three distinct commands: the common spell-cost reaction `ally:0`, the
effect's `enemy:-2`, and its `ally:2`. The enemy MP checkpoint is `0x18F92`, after
`DecreaseCurrentMp`; the earlier `0x18F7E` only proves HP application and cannot own MP state.
Playback changes target MP `2 -> 0`, actor MP `10 -> 12`, and retains the zero-delta command in the
ordered trace.

Battle 01 halves status EXP `5 -> 2`. The post-effect seed is `0xECAB`; award `RNG(16)` rolls 0 and
3 add one without subtracting, emitting 3 EXP and changing Bowie EXP `0 -> 3`. The Python verifier
independently models the LCG, `roll + 3`, target-MP clamp, award randomization, and replay order
before launching BizHawk. Reproduce with:

```powershell
uv run sf2 h3 spell-mp
```

Tracked artifacts are `tests/fixtures/h3/spell-mp-absorb-v1.json`,
`schemas/h3-spell-mp-absorb-fixture.schema.json`, `src/sf2tool/h3/spell_mp.py`, and
`tools/bizhawk/spell_mp_absorb_observer.lua`. This case does not establish unclamped transfer,
zero-MP target behavior, caster max-MP capping, enemy-caster behavior, or other drain effects.

## Unknown / Next Fixtures

- Add synthetic nonzero-counter input to the HEAL 3 branch and remaining stat-cap/underflow edges.
- Extend turn-order coverage beyond the now-confirmed AGI 127/128, second-turn, dead/unplaced,
  signed-byte, and stable-tie scenario to status-effect agility changes and multiple AGI >= 128
  combatants in one round.
- Add natural muddled/same-side/special-enemy action reachability, additional non-critical variance
  seeds, and other EXP randomization/level-difference branches to the confirmed physical path.
- Add APOLLO/NEPTUN/ATLAS runtime division, a naturally promoted full BLAZE action, remaining
  attack-spell EXP level/randomization/cap branches, promoted/full-recovery/multi-target healing,
  DESOUL failure/multi-target cases, other status spells, SPOIT boundary cases, and other
  non-damage spell families.
- The gameplay role and isolation guarantees of `RANDOM_SEED_COPY`, which source comments reserve for
  AI, need a traced scenario rather than a name-based assumption.
- The existing `LASER radius = 3` static anomaly remains in the behavior-test queue.
