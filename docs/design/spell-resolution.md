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

**Confirmed for DAO 1:** a promoted caster applies the same adjustment first, changing power
`18 -> floor(18 * 5 / 4) = 22`. Because DAO is one of four hard-coded invocation spell indexes,
each target calculation then performs unsigned integer division by the current target-list length.
With four targets, each call changes `22 -> floor(22 / 4) = 5`. The division happens per target but
does not consume or shrink the list; all four receive base damage 5. APOLLO, NEPTUN, and ATLAS share
the static branch but do not yet have separate runtime fixtures.

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
the result to the action accumulator with a cap of 49. Promoted caster classes add 20 only for this
effective-level comparison.

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
changes Bowie EXP `0 -> 1`. These cases do not yet cover other level-difference brackets, either
zero-roll adjustment, or the action cap.

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
branch, downward random adjustment, and persistent replay. It does not cover promoted healing
power, full-recovery power 255, multi-target AURA, non-healer/enemy EXP skips, or the 25-point cap.

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

## Confirmed DESOUL 1 Instant-Death Subset

DESOUL 1 reuses the STATUS effectiveness rule. Against setting 0, threshold 5 and controlled
range-8 roll 7 succeed. The effect emits enemy HP change `0x8000`, adds kill rewards, and sets the
scene's `targetDies` flag to `0xFF`; target HP remains 100 during construction.

The controlled target is enemy definition 0 (OOZE), level 1, max/current HP 100, and worth 10 gold.
Against level-1 unpromoted Bowie, `GetKillExp` returns 50, which the shared per-action cap truncates
to 49. Battle 01 halves this to 24. From effectiveness seed `0xECAB`, award rolls 0 and 3 add one
without subtracting, producing a 25-EXP command. Gold is not randomized or halved.

Replay applies caster MP `20 -> 12`, interprets signed HP change `-32768` as the death sentinel and
sets target HP `100 -> 0`, applies EXP `0 -> 25`, and increases force gold `0 -> 10`. This confirms
one successful enemy instant death, kill cap, reward lookup, and persistent replay. DESOUL failure,
DESOUL 2 multi-target behavior, ally/enemy-caster reward skips, boss immunity, and battle-victory
side effects remain outside the case.

## Confirmed SPOIT MP-Absorption Subset

SPOIT has zero MP cost. At the effect entry it rolls `rng.next(3)`, adds 3, and clamps that result
to the target's current MP. With seed `0x1234`, the roll is 2 and the candidate transfer is 5; the
controlled target has only 2 MP, so the emitted transfer is 2. The calculation adds the standard
5 status-effect EXP. Construction records the reactions but leaves caster/target MP at 10/2.

The command order is observable and significant: the common spell wrapper first emits a zero-cost
caster reaction, then SPOIT emits enemy MP `-2`, then caster MP `+2`. Replay therefore executes
`ally:0 -> enemy:-2 -> ally:2`, leaving caster MP 12 and target MP 0. Battle 01 halves accumulated
EXP `5 -> 2`; the post-effect seed `0xECAB` produces range-16 rolls 0 and 3, so the first adjustment
raises the command to 3. Caster EXP changes `0 -> 3` after the MP reactions.

This confirms one clamped ally-caster/enemy-target case, including the otherwise easy-to-discard
zero-delta command. The caster starts with SILENCE status `0x0300`, but SPOIT's properties are exactly
`TYPE_SPECIAL` and omit `AFFECTEDBYSILENCE`, so the spell executes normally and preserves that status.
This is the negative control for the marked-spell gate below. An unclamped transfer, empty target,
caster max-MP clamp, enemy caster, and other drain effects remain outside the case.

## Confirmed BOOST 1 Fresh Application and Recast Quirk

BOOST owns the two-bit mask `0x3000`; the counter unit is `0x1000`, so a fresh application writes
counter value 3. A separate after-turn fixture confirms that a one-counter field subtracts one unit,
emits its expiry message, and reaches zero; it does not yet replay a complete three-counter lifetime.

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

The after-turn fixture confirms that a one-counter SLOW field subtracts `0x0400`, emits its expiry
message, reaches zero, and participates in the final stat refresh. Multi-counter duration, SLOW
reapplication, SLOW 2 geometry, and interactions with BOOST/equipment remain separate cases.

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

The one-counter random after-turn SILENCE expiration branch is confirmed below. Two/three-counter
continuation probabilities are not yet H3 fixtures.

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

## Confirmed One-Counter After-Turn Lifecycle

After each living combatant's action, the original battle loop calls one status lifecycle pass. The
confirmed input combines one counter each of SILENCE, SLOW, ATTACK, and BOOST with CURSE:
`0x5504`. Processing order is observable and must remain stable:

1. SILENCE uses its current field as the RNG range. For field `0x0100`, every raw result is below
   `0x0100`, so masking with `0x0300` is zero and the effect expires.
2. SLOW, ATTACK, and BOOST deterministically subtract their counter units `0x0400`, `0x4000`, and
   `0x1000`. Because each result is zero, each emits its own expiry message.
3. The stored word progresses `0x5504 -> 0x5404 -> 0x5004 -> 0x1004 -> 0x0004`.
4. One final stat refresh rebuilds current ATT/DEF/AGI from bases and remaining effects. The case
   changes 45/40/24 to 40/40/24.

The final refresh also owns equipment-derived status normalization. CURSE is excluded from the
preserved status mask and re-added only when an equipped item is cursed. Four empty item slots make
the intermediate `0x0004` become final `0x0000`. A fidelity implementation must therefore separate
per-field status writes from the final derived-stat/equipment refresh; clearing or retaining CURSE
earlier produces a different observable sequence. Multi-counter continuation and full lifetimes
remain outside this case.

## H4 Fixture

| Fixture ID | File | Required parity |
| --- | --- | --- |
| `sf2-spell-damage-resistance-v1` | `tests/fixtures/h3/spell-damage-resistance-v1.json` | FIRE setting extraction; adjusted/quarter/post-resistance power; critical and variance calls; per-target and awarded EXP; temporary/restored/persistent HP; MP/EXP replay |
| `sf2-spell-summon-division-v1` | `tests/fixtures/h3/spell-summon-division-v1.json` | promoted DAO power 18→22; four per-target divisions 22→5; zero accumulation/minimum-one EXP award; neutral damage and persistent replay |
| `sf2-heal1-self-recovery-v1` | `tests/fixtures/h3/spell-healing-v1.json` | HEAL 1 power capped by missing HP; PRST minimum EXP; same-side Battle 01 skip; second zero-roll decrement; HP/MP/EXP replay |
| `sf2-sleep-resistance-matrix-v1` | `tests/fixtures/h3/spell-status-sleep-v1.json` | STATUS settings 0-3; thresholds 5-8; success/failure unwind; 5 EXP per success; immunity at setting 3; MP/status/EXP replay |
| `sf2-desoul-instant-death-v1` | `tests/fixtures/h3/spell-desoul-v1.json` | successful STATUS roll; `0x8000` death command; 49 EXP cap; enemy gold lookup; targetDies; HP/MP/EXP/gold replay |
| `sf2-spoit-mp-absorb-v1` | `tests/fixtures/h3/spell-mp-absorb-v1.json` | silenced-caster unmarked-spell control; range-3 roll plus 3; target-current-MP clamp; zero-cost/enemy-drain/caster-gain order; status EXP; persistent status/MP/EXP replay |
| `sf2-boost1-fresh-and-recast-v1` | `tests/fixtures/h3/spell-boost-v1.json` | `0x3000` counter; 3/8 DEF/AGI floor; same-side status EXP; cost/status replay; failed recast status-write/stat-refresh mismatch |
| `sf2-slow1-status-resistance-v1` | `tests/fixtures/h3/spell-slow-v1.json` | STATUS thresholds 0/6/7/8; setting-3 immunity; `0x0C00` counter; 3/8 DEF/AGI penalty; construction/replay timing; MP/EXP persistence |
| `sf2-dispel1-spell-gate-and-recast-v1` | `tests/fixtures/h3/spell-dispel-v1.json` | known-spell count gate; thresholds 5/6/7/8 and no-spell 8; `0x0300` counter; successful `0x0100` recast refresh; construction/replay timing; MP/EXP persistence |
| `sf2-silenced-caster-blocks-blaze1-v1` | `tests/fixtures/h3/spell-silence-gate-v1.json` | `AFFECTEDBYSILENCE` gate; silenced message; cost-before-block command order; no target effect or EXP; persistent actor/target state |
| `sf2-after-turn-one-counter-expiry-v1` | `tests/fixtures/h3/after-turn-status-expiry-v1.json` | SILENCE RNG range/mask; ordered SILENCE/SLOW/ATTACK/BOOST expiry and messages; intermediate status words; final stat and equipment-derived CURSE normalization |

The H4 adapter must consume this fixture rather than copying its expected numbers into an
engine-specific test.

## Original Fidelity and Modernization

The compatibility adapter preserves packed resistance semantics, operation order, truncation, RNG
order, minimum damage, and the construction/replay distinction. A modernization may present
resistance as percentages or a typed enum, but serialization must preserve enough information to
round-trip the original two-bit setting. Deliberate balance changes require a separate decision and
expected-deviation fixture.

## Unknown / Expansion Gates

- **Unknown at runtime:** APOLLO/NEPTUN/ATLAS division and a naturally promoted full BLAZE action.
- **Unknown:** remaining attack-spell EXP level-difference brackets, zero-roll adjustments, cap,
  kill bonus, and non-Battle-01 award behavior.
- **Unknown:** remaining healing branches, status spells beyond the confirmed SLEEP/DESOUL/BOOST/
  SLOW/DISPEL subsets, BOOST/SLOW 2,
  reapplication and multi-counter lifetime edges, unclamped/empty/full-caster SPOIT and other drain
  branches,
  DESOUL failure/multi-target branches, breath attacks, and special spell-effect dispatch.
- **Unknown:** multi-target ordering produced naturally by map geometry. This fixture supplies the
  ordered four-target list at the pre-initialization seam to isolate resolution arithmetic.
