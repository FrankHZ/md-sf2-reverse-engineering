# Spell Damage Resolution Contract

- Contract version: `0.1`
- Scope: attack-spell element lookup, damage resistance, spell critical, shared downward variance,
  damage/status EXP, healing, instant death, MP absorption, temporary-state lifecycle, and command
  replay
- Evidence state: **Confirmed subset**; unsupported spell families remain **Unknown**
- Evidence owner: [`runtime-rng-and-battle-math.md`](../research/runtime-rng-and-battle-math.md)

This contract describes original-fidelity arithmetic independently of an engine or presentation
layer. It currently owns one BLAZE 2 resistance matrix, one four-target DAO 1 division case, one
HEAL 1 self-recovery case, one SLEEP 1 status-resistance matrix, one DESOUL 1 success case, and one
SPOIT MP-absorption case, BOOST 1 fresh/recast behavior, a SLOW 1 resistance matrix, a DISPEL 1
spell-count/resistance/recast case, SILENCE gating of marked versus unmarked spell actions, and one
combined after-turn expiry case. It must not be generalized to adjacent branches until those paths
have their own H3 evidence.

## Required Inputs

The resolver receives the action spell entry, caster class, ordered targets, each target's packed
resistance word and HP, and a deterministic RNG source. The original action entry carries the base
spell index in its low six bits and the spell level in its upper bits. Resistance lookup uses the
base spell's element and selects the matching two-bit setting from the target's current resistance.

For the confirmed BLAZE path, element FIRE selects bits 6-7:

| Packed FIRE bits | Setting | Meaning in damage calculation |
| --- | --- | --- |
| `00` | 0 | neutral |
| `01` | 1 | minor resistance |
| `10` | 2 | major resistance |
| `11` | 3 | weakness |

The same numeric setting 3 is used as immunity by status-effect consumers. A remake must keep the
consumer context; it cannot globally label setting 3 as either weakness or immunity.

## Confirmed Damage Pipeline

### 1. Resolve base power and caster adjustment

**Confirmed for unpromoted SDMN and BLAZE 2:** the spell definition supplies power 10. The first
three calls do not receive the promoted-class adjustment, so adjusted power remains 10.

**Confirmed at the calculation seam:** before the fourth call, the fixture changes the caster class
to the first promoted class value 12. The original `AdjustSpellPower` multiplies power by 5 and
shifts right by 2, producing `floor(10 * 5 / 4) = 12`. This owns the class-threshold arithmetic;
it does not claim a naturally promoted caster performed the complete BLAZE action.

**Confirmed for all four invocation indexes at the calculation seam:** a promoted DAO caster first
changes power `18 -> floor(18 * 5 / 4) = 22`. Before each target's `AdjustSpellPower` call, the
fixture supplies DAO, APOLLO, NEPTUN, or ATLAS as the scene spell index. Every index reaches the
same unsigned division by current target-list length and changes `22 -> floor(22 / 4) = 5`. The
division happens per target but does not consume or shrink the list. This confirms the four
hard-coded comparator branches; because the enclosing cast remains DAO, it does not establish the
other spells' complete natural dispatch, critical chance, or animation path.

### 2. Apply the element resistance with integer truncation

Let `quarter = floor(adjustedPower / 4)`. The original applies exactly one setting branch:

```text
neutral:   damage = adjustedPower
minor:     damage = adjustedPower - quarter
major:     damage = floor(adjustedPower / 2)
weakness:  damage = adjustedPower + quarter
```

For the first three unpromoted calls, neutral/minor/major produce `10, 8, 5`. The promoted fourth
call enters weakness with adjusted power 12 and quarter 3, producing 15. Minor and weakness use the
already-truncated quarter value; they are not floating-point 75%/125% operations.

### 3. Roll spell critical before shared variance

**Confirmed for BLAZE:** critical uses `rng.next(32)`, and roll zero succeeds. On success it adds the
same truncated `quarter` computed from adjusted power and sets the battle-scene critical flag to
`0xFF`. The first three calls reset seed `0x1234` and return 29, preserving `10, 8, 5`. The promoted
weakness call resets seed 0, returns 0, sets the flag, and changes `15 -> 18`.

### 4. Reuse the common downward damage spread

The spell path then enters the same `InflictDamage` spread used by physical damage:

```text
range = floor(damage / 8) + 1
damage = max(damage - rng.next(range) - rng.next(range), 1)
```

The controlled seeds produce two zero rolls for each matrix member. Observed ranges are
`2, 2, 1, 3`, and final damages therefore remain `10, 8, 5, 18`.

### 5. Accumulate and award attack-spell EXP

For every damage target, the original computes a kill-value bracket from the caster's effective
level and target level, scales it by `finalDamage / targetMaxHp` with integer truncation, and adds
the result to the action accumulator with a cap of 49. Promoted caster classes add 20 for this
effective-level comparison. The confirmed bracket function is:

```text
difference = actorLevel + (promoted ? 20 : 0) - targetLevel
killBracket = difference < 3 ? 50
            : difference == 3 ? 40
            : difference == 4 ? 30
            : difference == 5 ? 20
            : difference == 6 ? 10
            : 0
damageExp = floor(killBracket * finalDamage / targetMaxHp)
accumulator = min(accumulator + damageExp, 49)
```

In the controlled BLAZE case, all combatants are level 1 and each target has max HP 100. The first
three unpromoted damage results add `floor(50 * damage / 100) = 5, 4, 2`, so the accumulator moves
`0 -> 5 -> 9 -> 11`. The fourth call uses promoted class 12: effective level 21 versus target level
1 is outside the rewarded brackets, so its 18 damage adds zero and the accumulator remains 11.

Battle 01 then halves the accumulated value with integer truncation, `11 -> 5`. Two range-16 RNG
rolls are 4 and 4; because only a zero first roll adds one and only a zero second roll subtracts one,
the emitted EXP command remains 5. The command replay changes Bowie EXP `0 -> 5`.

The promoted DAO case confirms the zero/minimum boundary. Effective level 21 versus four level-1
targets contributes zero after every hit. Battle 01 therefore enters award processing with zero;
range-16 rolls 1 and 7 do not change it, and the final minimum emits a one-EXP command. Replay
changes Bowie EXP `0 -> 1`.

**Confirmed across the dedicated BLAZE replay matrix:** final damage 10 against max HP 100 produces
damage EXP `5/4/3/2/1/0` for differences `<3/3/4/5/6/>=7`. A raw level-1 HERO is effective level
21 and produces 4 against a level-18 target. Starting the accumulator at 48, a nonlethal five-point
contribution saturates at 49. A lethal 10-damage hit against current HP 10 first stores proportional
damage EXP 5, then adds the full 50-point kill bracket through the natural post-damage call and also
saturates at 49. Consumers must keep these two additions ordered and independently capped.

Battle 01 halves the accumulator before its two range-16 adjustments. The independent award matrix
confirms first-roll zero adds one, second-roll zero subtracts one, two zeroes cancel, and the final
minimum is one. At a controlled `GiveExpAndGold` seam, changing only the battle ID from 1 to 0 makes
the one-entry halving table miss, so accumulator 5 remains 5 with nonzero rolls. This owns the table
lookup branch, not a complete naturally scheduled non-Battle-01 cast.

### 6. Restore snapshots, then replay HP and MP reactions

The original temporarily changes four 100-HP targets to `90, 92, 95, 82` while constructing the
battle scene, appends reactions, and restores all four snapshots to 100 before returning from scene
construction. Caster MP is still 20 at this boundary.

**Confirmed:** command playback then applies one ally reaction `(HP 0, MP -6)` to Bowie, changing
MP `20 -> 14`, followed by four enemy reactions in target-list order: `-10, -8, -5, -18`. Persistent
target HP becomes `90, 92, 95, 82`. The final snapshot is taken only after
`ExecuteBattlesceneScript` reads the command-list end marker.

A remake does not need to duplicate the original command-buffer internals, but it must expose an
equivalent ordered trace and avoid treating snapshot restoration as healing. The same replay also
applies the EXP command after the damage and MP reactions.

## Confirmed HEAL 1 Subset

The controlled healing fixture starts from a normal SDMN/full-HP Battle Test setup so the original
AI schedules Bowie's attack, then changes the action to a self-targeted HEAL 1 and sets current HP
to 95 at `WriteBattlesceneScript`. At the genuine `spellEffect_Heal` entry it changes only the class
to PRST (4), allowing the original healer-only EXP check to execute. This seam confirms resolution
and replay, not that Bowie naturally knows or casts HEAL.

HEAL 1 supplies power 15 and costs 3 MP. The unpromoted caster leaves power at 15; the target has
max HP 100 and current HP 95, so the missing-HP cap changes recovery `15 -> 5`. Construction appends
the recovery reaction but does not temporarily change current HP: HP remains 95 and MP remains 20
until playback.

For PRST, healing EXP starts as `floor(25 * recoveredHp / targetMaxHp)` with a minimum of 10 and an
action cap of 25. This case therefore changes `floor(25 * 5 / 100) = 1` to 10. The same-side flag is
nonzero at the award decision, so `GiveExpAndGold` skips the Battle 01 halving table. Starting from
seed `0x1234`, range-16 rolls are 14 and 0: the first does not add one, the second subtracts one,
and the command carries 9 EXP.

Playback applies the MP reaction first (`20 -> 17`), then the +5 HP reaction (`95 -> 100`), then
the EXP command (`0 -> 9`). This confirms one recovery cap, healer-class minimum, same-side award
branch, downward random adjustment, and persistent replay.

**Confirmed across the healing boundary matrix:** ordinary spell power receives the promoted
5/4 adjustment before the missing-HP cap. VICR/MMNK HEAL 1 therefore reaches power 18, while a VICR
HEAL 3 reaches 37 and recovers 37/50 HP for 18 EXP. HEAL 4 power 255 is a sentinel rather than a
numeric power: it copies missing HP directly into the recovery amount and skips promotion scaling.
Missing 99/100 HP therefore recovers 99/100 and computes 24/25 EXP.

Healing EXP is awarded only when the actor is an ally and its current class is PRST, VICR, or MMNK.
SDMN and a controlled enemy actor index both leave the accumulator unchanged. A max-HP-zero target
also skips before division and before the minimum. Otherwise:

```text
raw = floor(25 * recovery / targetMaxHp)
contribution = max(raw, 10)
accumulator = min(accumulator + contribution, 25)
```

Starting accumulator 20 plus a minimum contribution stores 25, proving that the cap is applied to
the cumulative healing action rather than each contribution in isolation. Multi-target AURA
geometry and ordered accumulation remain outside the matrix.

## Confirmed SLEEP 1 Status-Resistance Matrix

SLEEP uses the STATUS element, whose setting occupies resistance bits 14-15. For each target, the
effect adds the base threshold 5 to the extracted setting, rolls `rng.next(8)`, and succeeds when
`roll >= threshold`:

| STATUS setting | Threshold | Controlled roll | Result |
| --- | --- | --- | --- |
| 0 | 5 | 7 | success |
| 1 | 6 | 7 | success |
| 2 | 7 | 7 | success |
| 3 | 8 | 7 | failure / immunity |

The fixture resets seed `0x1234` at each genuine `spellEffect_Sleep` entry, producing roll 7 every
time. Successful targets receive status word `0x00C0` during command replay. Setting 3 cannot
succeed because an eight-way RNG result never reaches threshold 8; the failure path emits no status
reaction and awards no per-target EXP.

Each successful status effect adds 5 EXP, moving the action accumulator `0 -> 5 -> 10 -> 15`; the
failed fourth target leaves it at 15. Battle 01 halves this to 7. The remaining seed `0xECAB`
produces award rolls 0 and 3, so the first adjustment adds one and the command carries 8 EXP.
Playback changes caster MP `20 -> 16`, applies SLEEP to the first three targets only, and changes
caster EXP `0 -> 8`.

This confirms SLEEP 1 only. Other status spells may alter the base threshold, pre-existing status,
reaction payload, message, or EXP/kill behavior and require their own cases.

## Confirmed DESOUL 1 Resistance and Multi-Target Rewards

DESOUL 1 reuses the STATUS effectiveness rule. With controlled range-8 roll 7, settings 0-3 produce
thresholds `5/6/7/8` and outcomes `success/success/success/failure`. Every success emits HP change
`0x8000`, adds kill rewards, and sets the per-target `targetDies` flag to `0xFF`. The target loop
clears that flag before the next effect. Failure unwinds without a reaction, reward, or persistent
mutation; consumers must not synthesize a zero-damage reaction for it.

The four controlled targets are enemy definition 0 (OOZE), level 1, max/current HP 100, and worth
10 gold each. Against level-1 unpromoted Bowie, each successful kill calculates 50 EXP. The shared
per-action cap stores 49 on the first and remains 49 after the next two, while gold accumulates
`10 -> 20 -> 30`. Battle 01 halves EXP to 24; award rolls 0 and 3 produce a 25-EXP command. Gold is
neither randomized nor halved.

Replay applies commands in order
`ally:-8 -> enemy:-32768 -> enemy:-32768 -> enemy:-32768`, changes caster MP `20 -> 12`, leaves
target HP `0/0/0/100`, applies EXP `0 -> 25`, and increases force gold `0 -> 30`. This confirms
DESOUL failure, multi-target resolution ordering, per-action EXP saturation, cumulative gold, and
persistent replay. DESOUL 2 natural geometry, ally/enemy-caster reward skips, boss immunity, and
battle-victory side effects remain outside the case.

## Confirmed SPOIT MP-Absorption Boundary Matrix

SPOIT has zero MP cost. For each target it rolls `rng.next(3)`, adds 3, and clamps that candidate to
the target's current MP. With a controlled roll of 2, candidate 5 against targets holding 0, 2, and
10 MP emits transfers 0, 2, and 5. Even the empty target emits a target MP-zero reaction, a caster
MP-zero reaction, and the standard 5 status-effect EXP. Three targets therefore accumulate 15 EXP
while construction leaves caster and target MP unchanged.

Command order is observable and significant. The common zero-cost reaction precedes target/caster
pairs in target-list order:
`ally:0 -> enemy:0 -> ally:0 -> enemy:-2 -> ally:2 -> enemy:-5 -> ally:5`. Replay drains targets to
`0/0/5`. Caster MP starts at 18, reaches its maximum 20 on the `+2` command, and remains 20 when the
later `+5` payload is applied. The replay stat primitive, rather than SPOIT construction, owns this
second clamp to caster maximum MP. Consumers must preserve the command payload separately from the
post-command state.

Battle 01 halves accumulated EXP `15 -> 7`; range-16 award rolls 0 and 3 raise the command to 8, so
caster EXP changes `0 -> 8` after the MP reactions. The caster starts with SILENCE status `0x0300`,
but SPOIT's properties are exactly `TYPE_SPECIAL` and omit `AFFECTEDBYSILENCE`, so the spell executes
normally and preserves that status. This remains the negative control for the marked-spell gate
below. Enemy-caster behavior, naturally selected multi-target geometry, and other drain effects
remain outside the case.

## Confirmed BOOST 1 Fresh Application and Recast Quirk

BOOST owns the two-bit mask `0x3000`; the counter unit is `0x1000`, so a fresh application writes
counter value 3. A separate after-turn fixture confirms both `3 -> 2` without a message and `1 -> 0`
with an expiry message; it does not yet replay a complete three-counter lifetime.

For a fresh ally, the effect writes `0x3000` during construction, emits one status reaction, and
adds 5 status-effect EXP. Its displayed and replayed stat bonuses are integer floors of three
eighths of the base values. With base DEF 41 and AGI 23, the bonuses are 15 and 8. Construction
does not call `UpdateCombatantStats`, so status is already `0x3000` while current DEF/AGI remain
41/23 until command playback.

Reapplication has a non-obvious original-fidelity order. Against an ally already at `0x1000`, the
effect first ORs the stored status to `0x3000`, then notices the old nonzero BOOST field and calls
effectiveness with threshold 8. Controlled roll 7 fails and the shared failure routine unwinds the
BOOST caller. No reaction, stat messages, or additional EXP are emitted, but the earlier status
write is not reverted. The ally therefore ends with counter 3 while current DEF/AGI remain at the
old one-eighth values 45/27. They become consistent only when another path later calls the shared
stat refresh.

The fresh target is also the caster, exposing command order. Construction leaves caster MP 20 and
status `0x3000`. Playback first applies the pre-effect cost command `(MP -2, status 0)`, clearing
BOOST and refreshing base DEF/AGI, then applies `(MP 0, status 0x3000)`, producing persistent
DEF/AGI 56/31. Only the fresh target contributes 5 EXP. Because the targets are allies, Battle 01
does not halve it; post-failure seed `0xECAB` yields award rolls 0 and 3 and a 6-EXP command.

A remake fidelity mode must preserve this mutation/reaction ordering. A modernization may make a
failed recast a true no-op or recompute stats immediately, but that is an explicit behavioral
deviation rather than a cleanup that can be hidden inside the resolver.

## Confirmed SLOW 1 STATUS-Resistance Matrix

SLOW owns mask `0x0C00` and counter unit `0x0400`, so a fresh success writes counter value 3. Its
effectiveness thresholds are not the same as SLEEP despite sharing the STATUS resistance setting:

| STATUS setting | SLOW threshold | Controlled roll | Result |
| --- | --- | --- | --- |
| 0 | 0 | 7 | success |
| 1 | 6 | 7 | success |
| 2 | 7 | 7 | success |
| 3 | 8 | 7 | failure / immunity |

Setting 0 takes a dedicated branch that leaves the threshold at zero, so an eight-way RNG result
always succeeds. Every nonzero setting adds the constant 5 to its numeric value. Setting 3 remains
immunity because the resulting threshold 8 is unreachable. A generic STATUS resolver therefore
cannot assume one threshold table for all status spells.

Each success writes `0x0C00` during construction, emits an enemy reaction, and adds 5 EXP. As with
BOOST, construction changes the stored status before it changes derived stats: three successful
targets carry `0x0C00` but remain at base DEF/AGI 41/23. During replay, the reaction calls the
shared stat refresh and subtracts `floor(base*3/8)`, producing DEF/AGI 26/15. The immune target
stays at status 0 and 41/23.

Three successes accumulate 15 EXP. Enemy targets take the Battle 01 branch, so integer halving
produces 7; award rolls 0 and 3 raise the command to 8. Playback orders caster MP cost first
(`20 -> 17`), the three successful enemy reactions, then caster EXP `0 -> 8`.

The after-turn fixture confirms `0x0C00 -> 0x0800` without a message and `0x0400 -> 0` with an expiry
message, followed by final stat refresh. Repeated full duration, SLOW reapplication, SLOW 2 geometry,
and interactions with BOOST/equipment remain separate cases.

## Confirmed DISPEL 1 Spell Gate and Recast

DISPEL owns SILENCE mask `0x0300` and counter unit `0x0100`. Before effectiveness, it counts the
target's four spell slots after masking each entry to its six-bit base-spell index. A target with no
known spell receives threshold 8 and is immune regardless of STATUS setting. Otherwise the
threshold is `5 + setting`:

| STATUS setting | Known spells | Threshold | Controlled roll | Result |
| --- | --- | --- | --- | --- |
| 0 | 1 | 5 | 7 | success |
| 1 | 1 | 6 | 7 | success |
| 2 | 1 | 7 | 7 | success |
| 3 | 1 | 8 | 7 | failure / immunity |
| 0 | 0 | 8 | 7 | failure / immunity |

Each success ORs the target's existing status with `0x0300`, emits a status reaction, and adds 5
EXP. Unlike SLOW, DISPEL does not update the stored status while constructing commands. Playback is
the mutation boundary. An already-silenced target at `0x0100` is eligible for the same roll; success
refreshes it to `0x0300` and awards EXP again rather than rejecting the recast.

The confirmed five-target case accumulates 15 EXP, applies Battle 01 enemy-target halving and award
rolls to produce 8, then replays MP `20 -> 15`, three ordered SILENCE reactions, and EXP `0 -> 8`.
A remake fidelity resolver must preserve the target-spell gate, threshold 8 immunity, construction
versus replay boundary, and successful recast refresh. Whether a modernization should suppress EXP
for a refresh is a deliberate deviation.

The after-turn fixture confirms every one-step SILENCE outcome for one, two, and three counters.
A repeated full lifetime is not yet an H3 fixture.

## Confirmed SILENCE Cast Gate and Cost Order

The resolver blocks a silenced caster only when the selected spell has `AFFECTEDBYSILENCE`. In the
confirmed positive case, Bowie has status `0x0300` and selects BLAZE 1, which carries that property.
Initialization sets `silencedActor=0xFF`; the scene emits the silenced-action message and skips the
allowed-action branch, target-effect dispatch, BLAZE effect, and EXP command. The target therefore
keeps HP 100, MP 2, and status 0, and Bowie keeps EXP 0 and status `0x0300`.

The original command order still charges the spell. The animation phase constructs the caster's
MP-cost reaction before the scene checks `silencedActor`; playback changes MP `20 -> 18` even though
no target effect occurs. A fidelity implementation must preserve this cost-before-block ordering,
the absence of target/EXP commands, and the spell-property condition. Moving the gate before cost or
blocking every spell while SILENCE is nonzero is a deliberate modernization, not equivalent behavior.

SPOIT provides the unmarked control: under the same `0x0300` caster status it executes its complete
MP-drain and EXP path because it lacks `AFFECTEDBYSILENCE`.

## Confirmed After-Turn Counter Transition Matrix

After each living combatant's action, the original battle loop calls one status lifecycle pass. The
five confirmed inputs combine equal counter settings for SILENCE, SLOW, ATTACK, and BOOST with
CURSE. Processing order is observable and must remain stable:

1. SILENCE uses its current field as the RNG range and masks the result with `0x0300`. Zero clears
   the field and emits expiry; nonzero subtracts `0x0100` without a message.
2. SLOW, ATTACK, and BOOST always subtract their units `0x0400`, `0x4000`, and `0x1000`; each emits
   expiry only when its updated field is zero.
3. One-counter SILENCE always expires because a range-256 result cannot carry bits `0x0100` or
   `0x0200`. Controlled cases exercise both zero and nonzero masked results for fields `0x0200` and
   `0x0300`, confirming direct expiration or one-unit continuation at each setting.
4. The packed status after all four fields is `0x0004` for one-counter expiry; `0x5404`/`0x5504`
   for two-counter SILENCE expiry/continuation; and `0xA804`/`0xAA04` for three-counter
   expiry/continuation.
5. One final stat refresh rebuilds current ATT/DEF/AGI from the remaining ATTACK/BOOST/SLOW
   counters: 40/40/24, 45/40/24, or 50/40/24 for resulting settings zero, one, or two.

The final refresh also owns equipment-derived status normalization. CURSE is excluded from the
preserved status mask and re-added only when an equipped item is cursed. Four empty item slots make
the five intermediate endings normalize by clearing bit `0x0004`. A fidelity implementation must
therefore separate per-field status writes from the final derived-stat/equipment refresh; clearing
or retaining CURSE earlier produces a different observable sequence. A repeated full lifetime with
naturally carried state remains outside these cases.

## H4 Fixture

| Fixture ID | File | Required parity |
| --- | --- | --- |
| `sf2-spell-damage-resistance-v1` | `tests/fixtures/h3/spell-damage-resistance-v1.json` | FIRE setting extraction; adjusted/quarter/post-resistance power; critical and variance calls; per-target and awarded EXP; temporary/restored/persistent HP; MP/EXP replay |
| `sf2-spell-damage-exp-v1` | `tests/fixtures/h3/spell-damage-exp-v1.json` | all effective-level brackets; promoted +20; proportional damage EXP; ordered lethal kill bonus; per-addition 49 cap; Battle 01 halving and controlled non-halved table miss |
| `sf2-spell-summon-division-v1` | `tests/fixtures/h3/spell-summon-division-v1.json` | promoted DAO power 18→22; DAO/APOLLO/NEPTUN/ATLAS comparator hits; four per-target divisions 22→5; zero accumulation/minimum-one EXP award; neutral damage and persistent replay |
| `sf2-heal1-self-recovery-v1` | `tests/fixtures/h3/spell-healing-v1.json` | HEAL 1 power capped by missing HP; PRST minimum EXP; same-side Battle 01 skip; second zero-roll decrement; HP/MP/EXP replay |
| `sf2-healing-exp-boundaries-v1` | `tests/fixtures/h3/spell-healing-exp-boundaries-v1.json` | PRST/VICR/MMNK whitelist; ally/enemy/max-HP-zero guards; promoted ordinary power; power-255 full recovery; proportional/minimum EXP and cumulative 25 cap |
| `sf2-sleep-resistance-matrix-v1` | `tests/fixtures/h3/spell-status-sleep-v1.json` | STATUS settings 0-3; thresholds 5-8; success/failure unwind; 5 EXP per success; immunity at setting 3; MP/status/EXP replay |
| `sf2-desoul-instant-death-v1` | `tests/fixtures/h3/spell-desoul-v1.json` | STATUS settings 0-3; success/failure unwind; three ordered `0x8000` commands; targetDies reset; 49 EXP per-action saturation; cumulative enemy gold; HP/MP/EXP/gold replay |
| `sf2-spoit-mp-absorb-v1` | `tests/fixtures/h3/spell-mp-absorb-v1.json` | silenced-caster unmarked-spell control; empty/clamped/unclamped target MP matrix; zero-delta and ordered drain/gain commands; cumulative status EXP; caster-max-MP clamp; persistent status/MP/EXP replay |
| `sf2-boost1-fresh-and-recast-v1` | `tests/fixtures/h3/spell-boost-v1.json` | `0x3000` counter; 3/8 DEF/AGI floor; same-side status EXP; cost/status replay; failed recast status-write/stat-refresh mismatch |
| `sf2-slow1-status-resistance-v1` | `tests/fixtures/h3/spell-slow-v1.json` | STATUS thresholds 0/6/7/8; setting-3 immunity; `0x0C00` counter; 3/8 DEF/AGI penalty; construction/replay timing; MP/EXP persistence |
| `sf2-dispel1-spell-gate-and-recast-v1` | `tests/fixtures/h3/spell-dispel-v1.json` | known-spell count gate; thresholds 5/6/7/8 and no-spell 8; `0x0300` counter; successful `0x0100` recast refresh; construction/replay timing; MP/EXP persistence |
| `sf2-silenced-caster-blocks-blaze1-v1` | `tests/fixtures/h3/spell-silence-gate-v1.json` | `AFFECTEDBYSILENCE` gate; silenced message; cost-before-block command order; no target effect or EXP; persistent actor/target state |
| `sf2-after-turn-status-lifecycle-v1` | `tests/fixtures/h3/after-turn-status-lifecycle-v1.json` | complete one-step SILENCE 1/2/3 counter branch matrix; SLOW/ATTACK/BOOST 1→0, 2→1, 3→2; ordered writes/messages; final stat and CURSE normalization |

The H4 adapter must consume this fixture rather than copying its expected numbers into an
engine-specific test.

## Original Fidelity and Modernization

The compatibility adapter preserves packed resistance semantics, operation order, truncation, RNG
order, minimum damage, and the construction/replay distinction. A modernization may present
resistance as percentages or a typed enum, but serialization must preserve enough information to
round-trip the original two-bit setting. Deliberate balance changes require a separate decision and
expected-deviation fixture.

## Unknown / Expansion Gates

- **Unknown at runtime:** natural full APOLLO/NEPTUN/ATLAS casts and a naturally promoted full BLAZE
  action.
- **Unknown:** a complete naturally scheduled non-Battle-01 attack-spell action. The reward table
  miss itself is confirmed at its original entry seam.
- **Unknown:** multi-target AURA geometry and accumulation, status spells beyond the confirmed
  SLEEP/DESOUL/BOOST/SLOW/DISPEL subsets, BOOST/SLOW 2,
  reapplication and repeated lifetime edges, enemy-caster SPOIT and other drain branches,
  DESOUL 2 natural geometry, breath attacks, and special spell-effect dispatch.
- **Unknown:** multi-target ordering produced naturally by map geometry. This fixture supplies the
  ordered four-target list at the pre-initialization seam to isolate resolution arithmetic.
