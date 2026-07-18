# Level-Up and Stat-Growth Contract

- Status: **Confirmed core level-up path, scan boundaries, class caps, and derived-stat refresh**
- Evidence date: 2026-07-17
- Scope: original stat gain, level increment, learned-spell threshold, and level-up result payload

## Evidence Owners

- `tests/fixtures/h3/stat-gain-v1.json` / `sf2-calculate-stat-gain-startup-v1` owns 18
  controlled `CalculateStatGain` calls across curve-none, randomized growth, and minimum-stat pity.
- `tests/fixtures/h3/level-up-v1.json` / `sf2-level-up-tort-boundary-v1` owns natural
  `InitializeAllyStats → LevelUp` calls for Kazin's MAGE control and Kiwi's TORT boundary.
- `tests/fixtures/h3/level-up-boundaries-v1.json` / `sf2-level-up-boundaries-v1` owns seven
  controlled natural `LevelUp` calls covering post-projection growth, both class caps, both
  immediately preceding levels, promoted effective levels, inherited spell lists, cross-ally
  class-block scanning, the final missing-class sentinel, and a successful spell upgrade.
- `tests/fixtures/h3/level-up-refresh-v1.json` / `sf2-level-up-refresh-v1` owns Slade's controlled
  THIF level 39→40 call through `UpdateCombatantStats`, including current/base stat separation and
  an equipped Short Knife.
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

No learned spell is encoded as `0xFF`. If the class is already at its cap, or the forward class-block
scan reaches a negative control byte before finding a match, the routine writes the no-level result
(`level=0xFF`, zero gains, spell `0xFF`). `LEVELUP_ARGUMENTS` is shared scratch state: fixtures that
assert it capture it inside the owning call, not after surrounding initialization resumes.

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
registers. Seven source-modeled cases pass:

- Randolf/GLDT at level 30 starts from his stored projected stats. The level-31 call takes the
  post-projection branch and applies HP/MP/ATT/DEF/AGI gains `[2,0,2,1,2]`.
- Gyan/GLDT at promoted level 98 applies `[2,0,2,1,2]` and reaches level 99; this is distinct from
  Chaz/WIZ starting at level 99, which takes the cap exit.
- Slade/THIF starting at level 39 reaches base level 40. The refresh fixture independently covers
  that same boundary with the complete combatant entry described below.
- Slade/THIF at base level 40 and Chaz/WIZ at promoted level 99 both take the cap exit, leave stats
  and seed unchanged, and emit `[255,0,0,0,0,0,255]`.
- Kazin/WIZ at level 1 takes the promoted offset to effective level 22. WIZ's `$FE` control byte
  reuses Kazin's first stats block spell list, so his existing BLAZE 1 (`0x0B`) becomes BLAZE 3
  (`0x8B`) and the payload ends in `139`.
- Peter forced to WIZ has no WIZ block of his own. The original does not enforce an ally-local end;
  it continues through the contiguous table until Tyrin's WIZ block at `0x1EE653`. Because that
  borrowed block contains `$FE`, spell lookup redirects to Peter's first spell list, which is empty.
- Claude forced to SDMN has no later matching block. The scan reaches the final negative sentinel,
  takes the missing-class exit, preserves state/seed, and emits the no-level payload.

The original therefore uses class ID 12 as the cap boundary (40 below it, 99 from it onward), but
uses the defective class-11 comparison described below for effective spell-learning levels.

## Confirmed Current and Derived Stat Refresh

The refresh fixture starts Slade/THIF at level 39 with projected base stats
`[HP 42, MP 0, ATT 45, DEF 38, AGI 38]`, current HP 7, deliberately stale current
ATT/DEF/AGI/MOV/resistance/prowess values, and an equipped Short Knife. Seed `0x1234` produces base
gains `[2,0,2,1,2]` and level 40. The call site at `0x95BA` enters `UpdateCombatantStats` at
`0x89CE`; both points and the final `LevelUp` return are observed.

The original leaves current HP/MP unchanged while increasing maximum HP/MP and base ATT/DEF/AGI.
`UpdateCombatantStats` then resets current ATT/DEF/AGI/MOV/resistance/prowess from the new base and
class values before reapplying status and equipped-item effects. With no status effects, Slade ends
at current ATT 52 (base 47 plus Short Knife 5), DEF 39, AGI 40, MOV 7, resistance 0, and prowess
`0x13`; current HP remains 7. Items, spells, status, and EXP remain unchanged.

A remake should model maximum/current resources and base/derived combat stats as separate fields.
Level-up must not heal by merely copying new maxima into current HP/MP, and equipment effects must be
recomputed from the new base rather than incrementally stacked onto stale derived values.

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

- status-effect ordering and rounding when `UpdateCombatantStats` refreshes a level-up result;
- refresh behavior for non-attack equipment effects and cursed equipment;
- the latent HEAL 3 behavior with a synthetic nonzero counter setting.

The future remake growth module should consume the same five fixtures first, then extend them rather
than embedding untested curve or class assumptions in engine code.
