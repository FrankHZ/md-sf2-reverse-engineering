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
- `tests/fixtures/h3/level-up-refresh-v1.json` / `sf2-level-up-refresh-v1` owns five controlled
  Slade refresh calls through `UpdateCombatantStats`, including current/base separation, full and
  partial status counters, ordinary/cursed equipment, and a NINJ prowess-mask case.
- `tests/fixtures/h3/ally-initialization-prowess-v1.json` /
  `sf2-karna-heal3-prowess-v1` owns Karna's unmodified startup path through the HEAL 3 prowess
  special case in `InitializeAllyStats`.
- `tests/fixtures/h3/battle-exp-level-up-v1.json` / `sf2-battle-exp-level-up-v1` owns the connected
  natural Battle 01 path from a 24-point EXP command through the 100-point threshold, one
  source-modeled Bowie/SDMN `LevelUp`, and final persistent combatant state.
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
`0x89CE`; the call, function entry, and its `0x8A24` return are observed.

The original leaves current HP/MP unchanged while increasing maximum HP/MP and base ATT/DEF/AGI.
`UpdateCombatantStats` then resets current ATT/DEF/AGI/MOV/resistance/prowess from the new base and
class values before reapplying status and equipped-item effects. With no status effects, Slade ends
at current ATT 52 (base 47 plus Short Knife 5), DEF 39, AGI 40, MOV 7, resistance 0, and prowess
`0x13`; current HP remains 7. Items, spells, status, and EXP remain unchanged.

The second case combines maximum-counter ATTACK, BOOST, and SLOW (`3/8` each) with STUN, then equips
a Thieve's Dagger. Status adjustments use the refreshed base values: ATT 47 gains 17; DEF 39 gains
and loses 14; AGI 40 gains and loses 15, then STUN subtracts 5; MOV 7 loses 1. Equipment is applied
after status, so the dagger adds ATT 17 and AGI 5 last. Final current values are ATT 81, DEF 39,
AGI 40, and MOV 6, while the `0xFC01` status word is preserved. This confirms ordering and per-step
flooring for the observed maximum-counter combination, not every possible counter value.

The partial-counter case uses ATTACK `1/8`, BOOST `2/8`, and SLOW `1/8`. From refreshed bases
ATT/DEF/AGI `47/39/40`, separate floor operations produce current `52/44/45`; the two-bit fields are
therefore magnitudes rather than present/absent flags.

The cursed case equips a Black Ring and Short Knife together. ATT +10 and +5 produce current ATT
62, then the Black Ring causes CURSE (`0x0004`) to be present in the final status word. Curse is
thus derived again from currently equipped item definitions during refresh.

The fifth case puts Slade/NINJ at level 98 with a Ninja Katana. Level 99 gains are `[2,2,1,2,2]`;
the katana adds ATT 39 and increments double-attack prowess. NINJ base prowess `0x94` contains
critical 1/8, double 1/16, and counter 1/8, but `INCREASE_DOUBLE` keeps only the critical nibble
before inserting double 1/8. Current prowess becomes `0x24`, unintentionally resetting counter to
1/32. This is a confirmed original equipment bug, not an automatic remake default.

A remake should model maximum/current resources and base/derived combat stats as separate fields.
Level-up must not heal by merely copying new maxima into current HP/MP, and equipment effects must be
recomputed from the new base rather than incrementally stacked onto stale derived values.

## Confirmed Battle Award Entry

The connected battle fixture starts Bowie/SDMN level 1 at 99 EXP with source-modeled base stats and
empty equipment. Natural Battle 01 resolution supplies a 24-point command. `bsc0F_giveExp` applies
it before testing the threshold, so stored EXP passes through `99 -> 123 -> 23`; only then does it
call `LevelUp`. Seed `0x1234` produces payload `[2,2,0,1,1,1,255]`, level 2, maximum HP 14, and base
ATT/DEF/AGI `7/5/5`. Current HP/MP remain `12/8`, and the action-only current ATT/AGI values are
replaced by refreshed values `7/5`.

The fixture observes one call and exits at the containing EXP command's return, so it connects the
already-confirmed growth routine to persistent battle state. It does not generalize to an award
that reaches the 200 EXP clamp, more than one threshold, cap-level allies, random +1/-1 award
branches, or gold.

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

- low-stat underflow and ATT/DEF/AGI/MOV cap-saturation edges;
- enemy curse suppression and the remaining critical/counter/set prowess equip-effect functions;
- the latent HEAL 3 behavior with a synthetic nonzero counter setting.

The future remake growth module should consume the same six fixtures first, then extend them rather
than embedding untested curve or class assumptions in engine code.
