# Level-Up and Stat-Growth Contract

- Status: **Confirmed core level-up path, projection boundary, class caps, and spell inheritance**
- Evidence date: 2026-07-17
- Scope: original stat gain, level increment, learned-spell threshold, and level-up result payload

## Evidence Owners

- `tests/fixtures/h3/stat-gain-v1.json` / `sf2-calculate-stat-gain-startup-v1` owns 18
  controlled `CalculateStatGain` calls across curve-none, randomized growth, and minimum-stat pity.
- `tests/fixtures/h3/level-up-v1.json` / `sf2-level-up-tort-boundary-v1` owns natural
  `InitializeAllyStats → LevelUp` calls for Kazin's MAGE control and Kiwi's TORT boundary.
- `tests/fixtures/h3/level-up-boundaries-v1.json` / `sf2-level-up-boundaries-v1` owns four
  controlled natural `LevelUp` calls covering post-projection growth, both class caps, promoted
  effective levels, inherited spell lists, and a successful spell upgrade.
- `tests/fixtures/h3/ally-initialization-prowess-v1.json` /
  `sf2-karna-heal3-prowess-v1` owns Karna's unmodified startup path through the HEAL 3 prowess
  special case in `InitializeAllyStats`.
- [`../research/ally-growth.md`](../research/ally-growth.md) owns the static curve and class-block
  storage contract; [`../research/runtime-rng-and-battle-math.md`](../research/runtime-rng-and-battle-math.md)
  owns addresses and runtime interpretation.

## Confirmed Original Sequence

For a matching ally/class stats block below its class level cap, `LevelUp` processes HP, MP, base
attack, base defense, and base agility in that order. Curve `NONE` returns zero without consuming
RNG. Every active curve consumes two `GenerateRandomNumber` calls with range 128:

```text
randomizedGain = floor((projection * thisLevelFraction + rng1 - rng2 + 128) / 256)
expectedMinimum = start + floor((projection * cumulativeFraction + 128) / 256)
gain = randomizedGain + (current + randomizedGain < expectedMinimum ? 1 : 0)
```

After applying all five gains, the routine increments level, computes the effective spell-learning
level, learns at most the spell whose threshold exactly equals that effective level, and refreshes
derived combatant stats. `LEVELUP_ARGUMENTS` is a seven-byte result payload:

```text
[new level, max HP gain, max MP gain, base ATT gain, base DEF gain, base AGI gain, learned spell]
```

No learned spell is encoded as `0xFF`. If the class is already at its cap or no matching class block
exists, the routine writes the no-level result (`level=0xFF`, zero gains, spell `0xFF`).

At current level 30 or later, active curves no longer read their 29-entry projection tables. The
routine instead uses a fixed 1.5-point base and treats the stored projected value as the expected
minimum:

```text
randomizedGain = floor((384 + rng1 - rng2 + 128) / 256)
gain = randomizedGain + (current + randomizedGain < projected ? 1 : 0)
```

Curve `NONE` still returns zero before this boundary logic and consumes no RNG.

## Confirmed Projection, Cap, and Spell Boundaries

The boundary fixture lets the original startup path call `LevelUp` naturally, then controls only
the selected combatant entry and seed at function entry. It does not jump the PC or write CPU
registers. Four source-modeled cases pass:

- Randolf/GLDT at level 30 starts from his stored projected stats. The level-31 call takes the
  post-projection branch and applies HP/MP/ATT/DEF/AGI gains `[2,0,2,1,2]`.
- Slade/THIF at base level 40 and Chaz/WIZ at promoted level 99 both take the cap exit, leave stats
  and seed unchanged, and emit `[255,0,0,0,0,0,255]`.
- Kazin/WIZ at level 1 takes the promoted offset to effective level 22. WIZ's `$FE` control byte
  reuses Kazin's first stats block spell list, so his existing BLAZE 1 (`0x0B`) becomes BLAZE 3
  (`0x8B`) and the payload ends in `139`.

The original therefore uses class ID 12 as the cap boundary (40 below it, 99 from it onward), but
uses the defective class-11 comparison described below for effective spell-learning levels.

## Confirmed Karna HEAL 3 Initialization Rule

`InitializeAllyStats` scans every spell whose threshold is at or below the ally's starting effective
level before replaying earlier level-ups. When natural new-game initialization reaches Karna/PRST at
starting level 24, the HEAL 3 entry (`0x80`, threshold 22) takes a dedicated branch at `0x967A`.
It changes base prowess from `0x03` (critical 1/16, double 1/32, counter 1/32) to `0x13`
(critical 1/16, double 1/16, counter 1/32). The observer records the write after `SetBaseProwess` at
`0x969E`; it performs no state or register mutation.

This special branch changes prowess but deliberately skips `LearnSpell` during the preliminary scan.
The subsequent `LevelUp` replay reaches effective level 22 and learns HEAL 3 through the ordinary
path. A remake must therefore preserve the resulting prowess and spell state without depending on
this two-stage initialization implementation.

The source instruction keeps only the critical nibble before writing the incremented double setting,
so a synthetic nonzero counter setting would be cleared. Karna's natural PRST base prowess has zero
counter bits, and the H3 fixture does not generalize that latent behavior beyond the observed case.

## Confirmed TORT Boundary Defect

The original comparisons use `class < CHAR_CLASS_LASTNONPROMOTED` to skip the promoted-level offset.
Because TORT equals `CHAR_CLASS_LASTNONPROMOTED` (11), both `InitializeAllyStats` and `LevelUp`
misclassify it as promoted even though the first promoted class is 12.

For the natural Kiwi startup case, initialization changes effective spell level 7 → 27 and the
first level-up changes 2 → 22. Kazin's MAGE control remains 4 and 2. Kiwi has no TORT spell list, so
the confirmed defect changes the internal threshold but produces no learned-spell side effect in
this scenario. Its four active stat curves still produce gains `[1,0,1,1,1]`, level 2, and payload
`[2,1,0,1,1,1,255]`.

This is an original fidelity fact, not an automatic remake choice. A fidelity mode may preserve the
comparison; a corrected rules mode may classify promoted classes from 12 onward. The project must
record that choice explicitly before H4 treats either behavior as normative.

## Unknown and Future H4 Cases

**Unknown** runtime boundaries still requiring dedicated fixtures:

- the level 39→40 and 98→99 transitions immediately before the confirmed cap exits;
- the no-matching-class-block exit path;
- current HP/MP and equipment-derived stat refresh behavior after base maxima change;
- the latent HEAL 3 behavior with a synthetic nonzero counter setting.

The future remake growth module should consume the same four fixtures first, then extend them rather
than embedding untested curve or class assumptions in engine code.
