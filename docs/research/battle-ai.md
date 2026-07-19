# Battle AI Static Inventory and Decision Contracts

- Status: **Confirmed** for the pinned-source inventory, call metadata, action-filter, attack-priority,
  healing/support/final-action/movement decision code shape, constants, and H1 symbol addresses
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
extracts global/local labels and direct/indirect call metadata, parses the action filters, attack
priority, healing, support, final action choice, and movement, checks their fixtures and schemas, and writes canonical output to ignored
`local/derived/battle-ai-static.json`.

The canonical SHA-256 is
`4653315F38216A984364B0712C1CFB4C613E97C9D49F0D58E9240DE1B6C38035`.

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
records across four files in the first batch. Attack priority raised the subtree total to 18 records
across seven files; healing raised it to 23 across 12; support raised it to 31 across 14; final action
choice raised it to 32 across 15; movement raises it to 35 across 18. Four linked data-table symbols
live outside the subtree.

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

## Healing Eligibility and Entry Choice

**Confirmed static model:** `aiCommand_Heal` exits for a confused caster. It checks Healing Rain
before any spell; the item is used only when the first enemy (`COMBATANT_ENEMIES_START`) is at or
below half HP, and its action targets the caster because the item spell is area-based. Otherwise the
command accepts only HEAL or AURA, requiring 3 or 7 MP respectively before target search. Failure
falls back to an ordinary healing item. If both a stored spell and item reach action loading, the
item wins.

Targets are living members of the caster's own side. `DoesCombatantRequireHealing` computes
`3 × current HP <= 2 × max HP`; exactly two-thirds therefore qualifies. The separately named
`IsCombatantAtLessThanHalfHp` computes `2 × current HP <= max HP`, so its “less than” name/comment
also includes equality. Both helper bodies contain two alternate entry fragments that normal control
flow jumps over.

## Healing Spell-Level Decision

`DetermineHealingSpellLevel` uses missing HP and returns zero-based spell levels:

| Missing HP | Static selection |
| ---: | --- |
| 0–2 | `-1`, do not cast |
| 3–14 | level 1 (`0`) |
| 15–28 | level 3 (`2`) if known, otherwise level 1 |
| 29+ | level 4 (`3`) if known; otherwise level 3 if known, else level 1 |

Its MP fallback is source-confirmed defective. It shifts the candidate level by five bits instead of
six and adds the packed stored spell entry without masking its existing level bits, then decrements
the candidate until the looked-up cost appears affordable. It also converts a resulting level 2
(`1`) back to level 1 (`0`). The caller can later reintroduce level 2 only when level 1 was selected
for someone other than the caster, the caster has at least 11 MP, and knows level 3 or higher.

## Healing Target Priority

AI command sets CRITICAL (13) and LEADER (14) receive the maximum score 13. Other targets use this
descending table; an unlisted movement type scores zero:

| Movement type | Score |
| --- | ---: |
| healer / mage | 12 / 11 |
| stealth archer / centaur archer / archer | 10 / 9 / 8 |
| hovering / flying | 7 / 6 |
| brass gunner / stealth / centaur / regular / aquatic | 5 / 4 / 3 / 2 / 1 |

For every candidate AOE center, the command sums `movement score + 4` for each affected target,
stores the total as a byte, sorts centers descending, and selects the first one for which a usable
cast/item position exists. The byte store makes overflow a queued runtime boundary rather than an
assumed wide-integer score.

## Support Admission and Reachable Scoring

**Confirmed static model:** `aiCommand_Support` is enemy-only; allies and confused enemies stay.
It asks for the first support-type spell from slot zero and does not continue if that entry is not
exactly MUDDLE 2 (`0x47`) or DISPEL 1 (`0x06`). Accepted spells use their definition MP cost. Their
target side comes from the spell targeting-property bit.

MUDDLE 2 scores an AOE center only by affected target count and removes centers below three. DISPEL
adds one per affected target that has at least one usable attack spell or healing spell, checking the
healing list only if no attack spell was found, and removes centers below two. The final scan chooses
the later candidate on equal byte priority. If `DetermineAttackPosition` fails for that winner, the
command stays; it does not try the next ranked center.

## Unreachable ATTACK and BOOST 2 Support Routes

The command contains ATTACK and BOOST 2 dispatch branches after its admission gate, but that gate
accepts only MUDDLE 2 and DISPEL 1. With normal entry through `aiCommand_Support`, both branches are
therefore unreachable.

Their dormant code also contains independent defects:

- ATTACK finds reachable centers with ATTACK but populates each AOE using DISPEL 2 (`0x46`). Its
  intended lower-ATT score executes `cmpi #255`, then `addi #1` overwrites the condition flags before
  `ble`; consequently any eligible target saturates the center to 255. Eligibility requires no
  attack spell plus a recorded, still-living last target.
- BOOST 2 uses DISPEL 2 for both reach and AOE instead of BOOST 2 (`0x43`). It counts the same kind of
  eligible target and retains a center only at count two or greater.

These are confirmed source/control-flow properties. Whether any debug, patch, or unintended entry
can call the helper routes directly is outside the original command contract.

## Final Attack Action and Target Choice

`DetermineBattleactionForAttackAiCommand` first records whether physical, spell, and item target
lists are non-empty. With no option it returns Stay; physical alone always attacks. When physical and
one special option coexist, `RNG(6)` gives the special option two rolls and physical four:

| Viable special option | Physical rolls | Special rolls |
| --- | --- | --- |
| spell | 0, 1, 3, 5 | 2, 4 |
| item | 0, 1, 2, 4 | 3, 5 |

If physical is unavailable, the sole spell/item is always used. AQUA bypasses the 2/6 spell roll and
is always cast when spell and physical are the only viable categories. When both spell and item are
viable, `RNG(2)` chooses spell on 0 and item on 1; physical is ignored even if viable.

Target priorities are byte values compared as **signed bytes**, starting from maximum zero. Values
128–255 therefore act negative and can be ignored rather than outranking ordinary scores. The
returned priority is capped at 15, but every target tied at the original maximum is retained in
reverse input order.

Multiple tied targets normally proceed to a signed-byte movement comparison. With ordinary 0–127
movement values, the branch direction selects the **largest** stored movement value, not the
smallest, and equal values select the later collected target. For an enemy with critical priority
15+, the command first applies one of four 32-class order tables selected by movement type; a spell
forces the mage table. Only the earliest represented class cohort survives before movement tie-break.
The 16 movement-type pointers and all four class arrays are preserved in canonical output.

## Move and Move-Order Commands

`aiCommand_Move` builds movement with budget 128. A confused unit directly picks its side's first
index without checking that target's HP or map position; otherwise it collects living, on-map
opponents, with no empty-list guard before its cost loop. Costs are sorted ascending as unsigned
bytes and the first result is selected. A later neighbor pass runs only when the target list contains
enemies, using the mover's class-order table and swapping adjacent targets when class ranks differ by
at most one and combatant indexes by at most three.

Kraken Leg, Arm, and Head bypass the normal move-cost table and use the extracted 16-byte Kraken
table. After a preliminary move string with hardcoded budget 4, the command searches attack positions
at radii 0 then 1; failure changes the action to Stay, but the function still returns success.

`aiCommand_MoveOrder` is enemy-oriented and tries an Attack command before movement. Zero MOV, no
order, a dead follow target, or failed terrain check produces Stay. A movement-only outcome is also
encoded as Stay plus a non-empty move string. The pathfinding mode independently applies regular,
block-non-movable, or block-and-carve post-processing. The ally early-stay path reaches that
post-processing without initializing its stack-local mode byte.

`BuildMoveStringForMoveOrder` uses movement-array budget 128, a preliminary budget of `MOV × 2`, then
tries acceptable attack-space radii 0, 1, 2, and 3. Exhausting all four invalidates the move string.

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
11. healing eligibility at exact half and two-thirds HP, including odd max-HP rounding;
12. missing-HP thresholds 2/3, 14/15, and 28/29 across known levels and MP values, specifically the
    five-bit packed-entry lookup bug and the caller's level-2 override;
13. Healing Rain first-enemy gate, item fallback/precedence, byte-priority overflow, and first-reachable
    target selection.
14. MUDDLE 2 count-three and DISPEL count-two gates, equal-priority later-target selection, and
    no-fallback behavior when the selected target has no valid attack position;
15. first-slot unsupported spell shadowing a later MUDDLE 2/DISPEL, plus direct helper probes showing
    the dormant ATTACK score 255 and BOOST 2/DISPEL 2 entry mismatch.
16. all seven viability masks, confirming the 4:2 physical/special rolls, 1:1 spell/item roll, AQUA
    bypass, and physical suppression when both special lists are present;
17. priority bytes 127/128/255, critical class-order cohorts, reversed collection order, and movement
    values below/above 128 to confirm the signed comparison and largest-value selection.
18. empty normal-move target lists, confused first-side targets that are dead/off-map, neighbor class
    swaps, Kraken costs, and radius 0/1 post-move fallback;
19. Move Order ally-mode stack value, attack-before-move behavior, Stay-with-move-string semantics,
    and radius 0–3 exhaustion in the builder.

Do not split these into one emulator startup per question. Static setup and outputs are compatible
with a small number of case tables; split only when the action-getter and priority seams cannot be
shared safely.

## Remaining Static Batches

- standby movement, path obstruction, line-attacker/exploder special AI;
- command dispatcher and swarm/activation control.

These remain **Unknown** at subsystem-contract level even though their files and calls are now
inventoried.
