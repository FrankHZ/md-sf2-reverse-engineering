# Level-Up and Stat-Growth Contract

- Status: **Confirmed core level-up path; Unknown boundary scenarios remain explicit**
- Evidence date: 2026-07-17
- Scope: original stat gain, level increment, learned-spell threshold, and level-up result payload

## Evidence Owners

- `tests/fixtures/h3/stat-gain-v1.json` / `sf2-calculate-stat-gain-startup-v1` owns 18
  controlled `CalculateStatGain` calls across curve-none, randomized growth, and minimum-stat pity.
- `tests/fixtures/h3/level-up-v1.json` / `sf2-level-up-tort-boundary-v1` owns natural
  `InitializeAllyStats → LevelUp` calls for Kazin's MAGE control and Kiwi's TORT boundary.
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

- projection level 30 and the post-projection fixed growth path;
- base level cap 40 and promoted cap 99 no-level payloads;
- a promoted-class level-up with effective spell threshold `level + 20`;
- successful spell acquisition and list inheritance during a complete level-up;
- current HP/MP and equipment-derived stat refresh behavior after base maxima change.

The future remake growth module should consume the same two fixtures first, then extend them rather
than embedding untested curve or class assumptions in engine code.
