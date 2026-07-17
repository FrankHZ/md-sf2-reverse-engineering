# Physical Combat Resolution Contract

- Contract version: `0.1`
- Scope: normal physical attacks, successful dodge, critical damage, double attack, counterattack,
  HP reaction replay, and Battle 01 EXP award
- Evidence state: **Confirmed subset**; incomplete systems stay **Unknown** and are not defaulted here
- Evidence owner: [`runtime-rng-and-battle-math.md`](../research/runtime-rng-and-battle-math.md)

This document translates reproduced original behavior into an implementation-neutral contract. It
does not prescribe an engine, UI, animation timing, or asset format. A remake may implement the
internals differently, but an original-fidelity rules adapter must produce the same ordered facts
for the committed fixtures.

## Contract Boundary

The subsystem accepts a battle-state snapshot, one physical-action request, and a deterministic RNG
source. It returns an ordered resolution record containing:

- attempted attacks and their type (`first`, `second`, or `counter`);
- dodge decision and, when not dodged, each integer damage stage;
- temporary HP changes used while constructing the battle scene;
- ordered reaction and EXP commands;
- persistent HP and EXP after command replay;
- RNG calls in consumption order as `(range, result)` pairs.

The contract deliberately exposes both temporary construction state and persistent replay state.
The original restores saved HP before playing the generated commands; treating that restoration as
healing would produce the wrong final state.

## Required Inputs

| Input | Required behavior |
| --- | --- |
| Combatants | Current/max HP, current ATT/DEF, level, movement type, prowess fields, side, placement, and status needed by the active rule |
| Action | Actor, target, and attack type; type 2 identifies a counter for the confirmed half-damage rule |
| Terrain | Target land-effect setting as the decoded rule value, not a display percentage |
| Battle rules | Battle identifier and EXP modifiers such as Battle 01's halving rule |
| RNG | A stateful `next(range)` operation with calls preserved in exact order |

Data consumers must not infer missing prowess, movement, status, or terrain values from presentation
assets. Those values belong in normalized battle data or explicit action context.

## Confirmed Resolution Pipeline

### 1. Decide dodge before calculating damage

**Confirmed:** a successful dodge bypasses the damage calculation routine completely. In the
airborne-target fixture, a non-archer ground attacker against a hovering target selects range 8;
roll 0 succeeds. No damage call occurs and both combatants remain at 100 HP.

The non-dodge chain observes rolls `7/8`, `7/8`, and `31/32` for first, second, and counter attacks.
This confirms those scenario ranges and that a nonzero roll continues to damage. It does not yet
define every movement/weapon eligibility branch.

### 2. Calculate base physical damage with integer arithmetic

**Confirmed:** begin with:

```text
damage = max(currentAttack - currentDefense, 1)
```

Apply the target land-effect setting with truncation toward zero:

| Setting | Operation |
| --- | --- |
| 0 | `damage = floor(damage * 256 / 256)` |
| 1 | `damage = floor(damage * 230 / 256)` |
| other | `damage = floor(damage * 205 / 256)` |

If the target is flying or hovering and the attacker is a brass gunner, archer, centaur archer, or
stealth archer, add `floor(damage / 4)` after land reduction. The verified vector is
`ATT 99 - DEF 20 = 79`, then `floor(79 * 205 / 256) = 63`, then `63 + floor(63 / 4) = 78`.

No floating-point percentage should appear in an original-fidelity adapter. The order and each
truncation boundary are observable behavior.

### 3. Apply the confirmed critical modifier

**Confirmed for prowess definition 0 and a first attack:** range 32 with roll 0 succeeds and adds
half of the current damage:

```text
damage = damage + floor(damage / 2)
```

The verified vector changes 78 to 117. Critical definitions other than the committed case, and
critical eligibility for second/counter attacks, remain outside this contract version.

### 4. Apply counter half-damage before spread

**Confirmed:** attack type 2 halves damage before variance. The verified counter enters damage
application at 10 and becomes 5 before its spread calls.

### 5. Apply the two-roll downward spread

**Confirmed:** for the observed physical paths:

```text
range = floor(damage / 8) + 1
damage = damage - rng.next(range)
damage = damage - rng.next(range)
```

Both calls use the range derived before either subtraction. Verified vectors include `117 - 0 - 0
= 117`, `24 - 3 - 3 = 18`, and `5 - 0 - 0 = 5`.

### 6. Clamp temporary HP and append reactions

**Confirmed:** subtract final damage from temporary current HP and clamp at zero. Append a signed
negative HP-change reaction in attack order. The lethal fixture turns HP `100 -> 0` and records
`-117`; the nonlethal chain records enemy `-18`, enemy `-18`, then ally `-5`.

### 7. Construct follow-up attacks in original order

**Confirmed for the valid adjacent fixture:** successful double and counter decisions produce:

```text
first attack -> second attack -> counterattack
```

The second attack reads the target's already reduced temporary HP. The counter reverses actor and
target and uses attack type 2. **Confirmed:** target death rejects both follow-ups. The lethal
fixture supplies true double/counter toggles at the validation boundary after the natural first hit
sets `targetDies`; each is false at return, with only one damage calculation. The successful chain
separately owns the natural prowess/RNG decision. Remaining special-enemy rejection rules are
not yet generalized; callers must not treat the one successful fixture as proof that every requested
follow-up is valid.

**Confirmed:** an otherwise eligible counter is also rejected when the target cannot reach the
original actor. The range fixture keeps the target alive, disables double, supplies a true counter
toggle, and moves the actor to produce Manhattan distance 25 before the original validator executes;
the counter returns false and no additional damage call occurs. Remaining special-enemy
rejections remain unverified.

**Confirmed:** sleep also rejects an otherwise eligible counter. The sleep fixture keeps the target
alive and adjacent, disables double, supplies a true counter toggle, and writes status word `0x00C0`
at the documented combatant status offset immediately before validation. The original status getter
identifies sleep, the counter returns false, and no additional damage call occurs.

**Confirmed:** stun independently rejects an otherwise eligible counter. The stun fixture uses the
same live adjacent target and forced-valid counter seam but writes status word `0x0001`. The original
status getter identifies stun after its separate sleep check, clears counter, and performs no second
damage calculation.

**Confirmed at the validation seam:** the same-side flag rejects a counter. The fixture observes the
natural opposing-side flag as false, then sets only `targetIsOnSameSide` true immediately before the
original validator. With a live adjacent target and forced-valid counter, the validator clears the
toggle and performs no second damage calculation. A later fixture should still exercise a naturally
same-side action producer; this result owns the validator branch, not target-selection reachability.

**Confirmed at the validation seam:** a Burst Rock cannot perform the counter. The fixture observes
the original target as enemy index 39 (Gizmo), then changes only its combatant enemy-index field to
32 (Burst Rock) before validation. The original `GetEnemy` path clears the forced-valid counter and
performs no second damage calculation. This establishes the direction of this exclusion: the
original attack target is the prospective counterattacker.

### 8. Restore snapshots, then replay commands persistently

**Confirmed:** battle-scene construction restores saved HP snapshots before playback. The command
interpreter then applies signed reactions in list order with zero clamping. For the chain, restored
HP is `ally=200, enemy=200`; replay ends at `ally=195, enemy=164`. For the lethal attack, restored
enemy HP 100 becomes persistent HP 0 after replaying `-117`.

The remake may avoid an actual restore internally, but its observable resolution trace and final
state must preserve this two-phase contract so animation/UI consumers cannot accidentally persist
the temporary snapshot.

### 9. Award confirmed Battle 01 EXP

**Confirmed for the lethal equal-level fixture:** damage and kill EXP accumulate with a per-action
cap of 49. Battle 01 halves 49 with integer truncation to 24. Two subsequent `RNG(16)` rolls of 4
leave the award unchanged, and command replay changes actor EXP `0 -> 24`.

The complete level-difference table, other battle modifiers, random +1/-1 outcomes, level-up at 100,
and gold are outside this contract version.

## Reference Adapter Shape

An implementation can use any language or engine if it presents an equivalent test seam:

```text
resolvePhysicalAction(
  initialBattleState,
  physicalAction,
  deterministicRng,
  battleRules
) -> {
  attacks[],
  temporaryState,
  commands[],
  persistentState,
  rngTrace[]
}
```

`attacks[]` must retain every integer intermediate used by the fixture: dodge range/roll, base
damage, pre-spread damage, spread range/results, final damage, and HP before/after. Production builds
may compile out the rich trace, but H4 tests require it.

## H4 Fixture Matrix

The modern adapter must consume the committed JSON directly or through a thin shared loader. It must
not copy expected numbers into a separate engine-specific test suite.

| Fixture ID | File | Required parity |
| --- | --- | --- |
| `sf2-physical-damage-land-archer-v1` | `tests/fixtures/h3/physical-damage-v1.json` | Base, land reduction, anti-air bonus, result |
| `sf2-physical-damage-application-v1` | `tests/fixtures/h3/physical-damage-application-v1.json` | Critical, spread, HP clamp, EXP accumulator |
| `sf2-battle-scene-replay-v1` | `tests/fixtures/h3/battle-scene-replay-v1.json` | Snapshot restore, EXP modification, persistent replay |
| `sf2-attack-chain-double-counter-v1` | `tests/fixtures/h3/attack-chain-v1.json` | Attack order, dodge misses, double/counter, half damage, reactions |
| `sf2-successful-airborne-dodge-v1` | `tests/fixtures/h3/dodge-v1.json` | Successful dodge, zero damage calls, unchanged HP |
| `sf2-lethal-followup-validation-v1` | `tests/fixtures/h3/lethal-followup-v1.json` | Target-death rejection of forced-valid double/counter toggles |
| `sf2-counter-range-validation-v1` | `tests/fixtures/h3/counter-range-v1.json` | Out-of-range rejection of a forced-valid counter toggle |
| `sf2-counter-sleep-validation-v1` | `tests/fixtures/h3/counter-sleep-v1.json` | Sleeping-target rejection of a forced-valid counter toggle |
| `sf2-counter-stun-validation-v1` | `tests/fixtures/h3/counter-stun-v1.json` | Stunned-target rejection of a forced-valid counter toggle |
| `sf2-counter-same-side-validation-v1` | `tests/fixtures/h3/counter-same-side-v1.json` | Same-side flag rejection at the original counter-validation seam |
| `sf2-counter-burst-rock-validation-v1` | `tests/fixtures/h3/counter-burst-rock-v1.json` | Burst Rock rejection as the prospective counterattacker |

H4 passes only when ordered RNG consumption, intermediate integers, command order, and persistent
state match. Rendering, animation duration, input feel, and audiovisual assets are not H4 assertions.

## Original Fidelity and Modernization

The default compatibility adapter preserves confirmed arithmetic, RNG order, clamping, attack order,
and replay results. A modernization may intentionally replace any of them only after a decision
record names the player-facing reason and adds a separate expected-deviation test. Modern behavior
must never overwrite the original fixture or be described as a newly discovered original rule.

## Unknown / Contract Expansion Gates

The following remain **Unknown** for a general implementation and block declaring physical combat
complete:

- all dodge eligibility and range-selection branches;
- remaining failed double/counter validation: Taros, Kraken Arm, and Zeon Guard exclusions plus natural same-side action reachability;
- critical definitions beyond the verified case and criticals on second/counter attacks;
- resistance, status effects, spell damage, healing, drain, and instant-death paths;
- additional spread seeds and the exact lower-bound behavior across all input ranges;
- full EXP/gold tables, random EXP adjustment branches, and level-up side effects;
- battle-scene command types beyond the confirmed HP reaction and EXP award subset.

Each expansion must add or extend H3 evidence first, update this contract, and then become an H4
fixture. Engine work must represent an unsupported branch as explicit incomplete behavior rather
than silently inventing a convenient rule.
