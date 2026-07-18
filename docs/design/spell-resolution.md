# Spell Damage Resolution Contract

- Contract version: `0.1`
- Scope: attack-spell element lookup, damage resistance, spell critical, shared downward variance,
  temporary HP application, and scene-construction restoration
- Evidence state: **Confirmed subset**; unsupported spell families remain **Unknown**
- Evidence owner: [`runtime-rng-and-battle-math.md`](../research/runtime-rng-and-battle-math.md)

This contract describes original-fidelity arithmetic independently of an engine or presentation
layer. It currently owns one BLAZE 2 matrix. It must not be generalized to healing, drain, status,
instant-death, breath, or summon behavior until those paths have their own H3 evidence.

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

**Confirmed for unpromoted SDMN and BLAZE 2:** the spell definition supplies power 10. The caster
does not receive the promoted-class adjustment, so adjusted power remains 10.

**Confirmed statically, not yet in this runtime matrix:** a promoted caster multiplies spell power
by 5 and shifts right by 2, producing `floor(power * 5 / 4)`. DAO, APOLLO, NEPTUN, and ATLAS then
divide adjusted power by the number of targets. These branches remain expansion gates for H3.

### 2. Apply the element resistance with integer truncation

Let `quarter = floor(adjustedPower / 4)`. The original applies exactly one setting branch:

```text
neutral:   damage = adjustedPower
minor:     damage = adjustedPower - quarter
major:     damage = floor(adjustedPower / 2)
weakness:  damage = adjustedPower + quarter
```

For BLAZE 2 power 10, the confirmed four-target result is `10, 8, 5, 12`. The minor and weakness
paths use the already-truncated quarter value 2; they are not floating-point 75%/125% operations.

### 3. Roll spell critical before shared variance

**Confirmed for BLAZE:** critical uses `rng.next(32)`, and roll zero succeeds. On success it adds the
same truncated `quarter` computed from adjusted power and sets the battle-scene critical flag. The
matrix resets seed `0x1234` at each calculation entry; all four original calls return 29, so no
critical modifier is applied. This confirms the noncritical branch and call ordering, not a runtime
successful spell critical.

### 4. Reuse the common downward damage spread

The spell path then enters the same `InflictDamage` spread used by physical damage:

```text
range = floor(damage / 8) + 1
damage = max(damage - rng.next(range) - rng.next(range), 1)
```

The fixed seed produces two zero rolls for each matrix member. Observed ranges are `2, 2, 1, 2`,
and final damages therefore remain `10, 8, 5, 12`.

### 5. Preserve construction and replay phases

The original temporarily changes four 100-HP targets to `90, 92, 95, 88` while constructing the
battle scene, appends reactions, and restores all four snapshots to 100 before returning from scene
construction. The fixture exits at this restoration boundary, before command playback; caster MP
therefore remains 20 even though the generated BLAZE 2 action carries an MP cost of 6.

A remake does not need to duplicate the original command-buffer internals, but it must expose an
equivalent ordered trace and avoid treating snapshot restoration as healing. Persistent HP and MP
after spell-command playback remain a later fixture.

## H4 Fixture

| Fixture ID | File | Required parity |
| --- | --- | --- |
| `sf2-spell-damage-resistance-v1` | `tests/fixtures/h3/spell-damage-resistance-v1.json` | FIRE setting extraction; adjusted/quarter/post-resistance power; critical and variance calls; temporary and restored HP |

The H4 adapter must consume this fixture rather than copying its expected numbers into an
engine-specific test.

## Original Fidelity and Modernization

The compatibility adapter preserves packed resistance semantics, operation order, truncation, RNG
order, minimum damage, and the construction/replay distinction. A modernization may present
resistance as percentages or a typed enum, but serialization must preserve enough information to
round-trip the original two-bit setting. Deliberate balance changes require a separate decision and
expected-deviation fixture.

## Unknown / Expansion Gates

- **Unknown at runtime:** promoted-caster +25%, summon division, and successful spell critical.
- **Unknown:** persistent HP/MP after spell command playback and attack-spell EXP award.
- **Unknown:** healing, status resistance/immunity, drain, instant death, breath attacks, and special
  spell-effect dispatch.
- **Unknown:** multi-target ordering produced naturally by map geometry. This fixture supplies the
  ordered four-target list at the pre-initialization seam to isolate resolution arithmetic.
