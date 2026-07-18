# Battle AI Static Inventory and Action Filters

- Status: **Confirmed** for the pinned-source inventory, call metadata, action-filter code shape,
  constants, and H1 symbol addresses
- Status: **Inferred** for caller-visible behavior not already reproduced by an H3 fixture
- Evidence date: 2026-07-18
- ROM: USA retail, SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`
- Source baseline: `ShiningForceCentral/SF2DISASM` commit
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Reproduction

```powershell
uv run sf2 h2 battle-ai
```

The Python-owned rail scans the complete
`disasm/code/gameflow/battle/ai` subtree, validates the pinned Git commit, hashes every source file,
extracts global/local labels and direct/indirect call metadata, parses the five action filters, checks
their fixture and schema, and writes canonical output to ignored
`local/derived/battle-ai-static.json`.

The canonical SHA-256 is
`FAE1CE85A017B06A9B35F2BAA337FD503E6E9296B26E2A870A6B0E63C6487607`.

## Complete Subtree Inventory

| Metric | Confirmed value |
| --- | ---: |
| ASM files | 26 |
| Source lines | 5,991 |
| Non-comment statements | 3,462 |
| Global labels | 82 |
| Local `@` labels | 467 |
| Direct call sites | 388 |
| Indirect call sites | 2 |
| Unique direct targets | 118 |
| Targets implemented inside the subtree | 46 |
| External direct targets | 72 |

This is an inventory denominator, not a claim that all 82 labels are behaviorally understood. Before
this batch, the research index reached only `IsCombatantConfused`, `DetermineMuddledBattleaction`, and
`aiCommand_Attack` across three files. It now also binds the five action getters, for eight indexed
records across four files in the first batch. The priority batch raises the subtree total to 18
records across seven files, plus one linked data-table symbol outside the subtree.

## Action Getter Addresses

The H1 assembler listing independently binds the source symbols:

| Function | ROM entry | Static role |
| --- | ---: | --- |
| `GetNextUsableAttackSpell` | `0xCF74` | Find an attack spell from the requested starting slot |
| `GetNextHealingSpell` | `0xD018` | Find a healing spell |
| `GetNextSupportSpell` | `0xD062` | Find a support spell |
| `GetNextUsableAttackItem` | `0xD0AC` | Find a battle-usable attack item |
| `GetNextUsableHealingItem` | `0xD160` | Find a battle-usable healing item |

All five scan at most four slots. No spell returns `SPELL_NOTHING = 0x3F`; no item returns
`ITEM_NOTHING = 0x7F`.

## Spell Filtering

**Confirmed static code shape:** `GetNextUsableAttackSpell` calls `IsCombatantConfused`, but then
unconditionally sets its local confusion flag to one for every ally caster. Consequently the static
filter applied to allies and confused enemies accepts only these base spell indexes before checking
that the spell definition has attack type:

| Spell | Base index |
| --- | ---: |
| BLAZE | `0x0B` |
| FREEZE | `0x0C` |
| BOLT | `0x0D` |
| BLAST | `0x0E` |
| KATON | `0x1B` |
| RAIJIN | `0x1C` |

An unconfused enemy bypasses this name allowlist but still requires spell type `0` (attack). A
rejected candidate advances to the next slot. Once accepted, the function calls
`GetHighestUsableSpellLevel`; the healing and support getters instead return the stored entry and
slot after checking type `1` or `2`. Neither healing nor support getter applies a confusion filter.

The ally-forced filter is source-confirmed, but its natural caller-visible consequences remain
**Inferred** until the planned batched observation compares the same spell list across ally,
unconfused-enemy, and confused-enemy casters.

## Item Filtering and Asymmetry

**Confirmed static code shape:** attack items first pass `IsItemUsableInBattle`. An equipped entry
bypasses `ITEMENTRY_BIT_USABLE_BY_AI`; an unequipped entry requires it. Allies and confused enemies
then use a smaller spell allowlist—BLAZE/FREEZE/BOLT/BLAST only—followed by the attack-type check.

The rejection path is asymmetric:

- a genuinely unusable item advances to the next slot;
- an unequipped item missing the AI-use bit, a confused-disallowed use spell, or a non-attack use
  spell jumps to `@Nothing` and aborts the entire search with `ITEM_NOTHING`;
- the healing-item getter instead continues scanning rejected candidates;
- Healing Rain (`ITEM 8`) bypasses the AI-use-bit requirement, while other healing items require it;
- healing items must resolve to spell type `1` and do not apply a confusion filter.

This stop-versus-continue difference is a high-value H3 question because inventory ordering can make
a later valid attack item unreachable. Static source establishes the branch graph; runtime must still
confirm the caller-visible result and that no surrounding command loop retries from the next slot.

## Attack Potential-Damage Model

**Confirmed static model:** physical target scoring does not call the battle-scene damage function.
It computes `max(current ATT - current DEF, 1)`, then applies the defender's land setting using a
fixed-point multiplier and floors after the multiply:

| Land setting | Multiplier | Static estimate |
| ---: | ---: | --- |
| 0 | `256/256` | `floor(damage × 256 / 256)` |
| 1 | `230/256` | `floor(damage × 230 / 256)` |
| 2 or other | `205/256` | `floor(damage × 205 / 256)` |

Because the minimum of one is applied before terrain, `1 × 230 >> 8` and `1 × 205 >> 8` both become
zero. The AI estimate can therefore predict zero physical damage even though the construction step
first clamps to one.

Spell scoring starts from definition power and applies only the resistance setting: minor subtracts
`floor(power/4)`, major halves with floor, and weakness adds `floor(power/4)`. It does not run the
full spell-damage spread/critical path. For an area spell, each affected target is scored and the
priorities are summed. Potential remaining HP clamps to zero after subtraction.

## Difficulty and Activation Script Matrix

`pt_TargetPriorityScripts` contains 16 pointers indexed as `difficulty × 4 + activation-column`.
Allies force activation column 2. Enemy spell scoring masks to two activation bits; regular attacks
extract a rotated low nibble with mask `0x0F`, although the table only has four columns.

| Difficulty | Column 0 | Column 1 | Column 2 | Column 3 |
| ---: | ---: | ---: | ---: | ---: |
| 0 | Script 1 | Script 2 | Script 3 | Script 4 |
| 1 | Script 1 | Script 2 | Script 1 | Script 4 |
| 2 | Script 2 | Script 2 | Script 2 | Script 2 |
| 3 | Script 2 | Script 2 | Script 2 | Script 2 |

The four source-confirmed score shapes are:

- **Script 1:** base 1, +15 if predicted lethal, +2 for the previous target, then optional class
  adjustment.
- **Script 2:** Script 1 plus +1 when remaining HP is at most one third of current HP and +1 when it
  is at most one fifth of max HP. The carry comparisons include equality despite comments saying
  “less than.”
- **Script 3:** `RNG(3)` selects lethality scoring for one outcome; the other two use
  `max(19 - 2 × movement, 1)`. It does not apply class adjustment.
- **Script 4:** base 1, +15 if predicted lethal, +1 at or below one fifth max HP; no class adjustment.

## Class/Movement Adjustment Tables

The rail parses four 32-byte tables with values 0–4 and the 16-entry movement-type pointer table.
Regular/centaur/stealth/brass-gunner/aquatic/healer/default types share the regular table; flying and
hovering share flying; archer and centaur-archer share archer; stealth-archer and mage share mage.

`AdjustTargetPriority` returns immediately for enemy attackers and for confused attackers, so this
extra 0–4 score applies only to non-confused allies. A second oddity checks whether the **previous
target** (`d7`) equals Sarah (ally index 1); if so it forces the mage table instead of selecting by
the attacker's movement type. This is source-confirmed code shape. Whether natural caller state makes
the Sarah condition reachable in the intended way remains **Inferred**.

## Runtime Question Queue

The next BizHawk batch should share one derived-ROM seam and one result buffer for at least:

1. the same `[DESOUL, BLAZE]` spell list on an ally, unconfused enemy, and confused enemy;
2. KATON/RAIJIN acceptance for spells but rejection for attack-item use spells;
3. attack-item slot 0 rejection followed by a valid slot 1, distinguishing abort from continue;
4. equipped versus unequipped AI-use-bit handling;
5. Healing Rain versus another healing item with and without the AI-use bit;
6. MP-limited `GetHighestUsableSpellLevel` and the caller's retry/fallback behavior.
7. exact one-third/one-fifth equality boundaries and the zero estimate after one-point terrain damage;
8. all four difficulty/activation script selections, especially regular attack's `0x0F` mask against
   a four-column table;
9. previous-target Sarah forcing the mage adjustment table, plus enemy/confused early exits;
10. Script 3's `RNG(3)` 1:2 lethality-versus-movement split and area-spell score summation.

Do not split these into one emulator startup per question. Static setup and outputs are compatible
with a small number of case tables; split only when the action-getter and priority seams cannot be
shared safely.

## Remaining Static Batches

- healing/support target eligibility and scoring;
- final attack/item/spell action choice and RNG weighting after the three priority lists are populated;
- movement orders, standby movement, path obstruction, line-attacker/exploder special AI;
- command dispatcher and swarm/activation control.

These remain **Unknown** at subsystem-contract level even though their files and calls are now
inventoried.
