# Core Battle Numbers and Growth Quick Reference

Why did stepping onto plains cut a Huge Rat's hit from 5 to 4 instead of 3? Why did a 15-power
HEAL 1 restore only 5 HP yet award 10 EXP before randomization? Why can a unit with higher Agility
still act later in the round, and why does leveling up raise max HP without healing existing wounds?

This guide answers those questions from accepted original-game rules and bounded runtime
observations. It organizes the numbers by the player decision they govern—stat refresh, physical
hits, spells and healing, turn order, and EXP/level-up—stating units, conditions, integer
truncation order, and limits, then linking each row to its exact L3 contract or research owner. It
does not duplicate full spell/status tables, prescribe balance curves, or treat remake behavior as
evidence about the original game.

## 1. Base, current, status, and equipped stats

A combatant's screen values are **derived current stats**, rebuilt from stored base/class values
whenever `UpdateCombatantStats` runs—never stacked incrementally on top of stale numbers:

| Stat layer | What it holds | Critical rule to avoid double-counting |
| --- | --- | --- |
| **Stored base stats & class defaults** | Stored max HP/MP, base ATT/DEF/AGI, **`MOV_BASE`** (initialized from class load), plus class resistance and prowess | Changed by level-up, promotion, or **permanent growth items** (e.g., Power Water, Running Pimento; [growth items](../../../../remake/docs/domain/permanent-growth-items-design.md#effects-and-feedback)). `InitializeCurrentStats` copies stored `MOV_BASE` to `MOV_CURRENT`, so Running Pimento persists across refreshes (while class load on promotion overwrites `MOV_BASE`). **Base ATT never includes worn weapon ATT.** |
| **Current HP / MP** | Live expendable battle pools | **Untouched** by `LevelUp` and `UpdateCombatantStats`—raising max HP/MP does not heal. |
| **Derived current stats** | Live ATT/DEF/AGI/MOV/resistance/prowess used in battle formulas | Rebuilt from stored bases/class defaults → status modifiers → worn items in carried-slot order. **Do not add weapon ATT again** to an already-refreshed combatant row (e.g., Battle02 reference rows). |

**Confirmed refresh order** ([stat refresh contract](../../contracts/party/level-up.md#confirmed-current-and-derived-stat-refresh) · [ring & refresh rules](../../../../remake/docs/domain/ring-equipment-design.md#original-rules-and-evidence)):

1. Copy stored base ATT, DEF, packed AGI, and `MOV_BASE` (plus class resistance and prowess) to current stats ([pinned base-to-current copy](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/stats/updatecombatantstats.asm#L293-L304)).
2. Apply active status counters independently using integer floor division against the **refreshed
   base stat** (`counter` is `1`, `2`, or `3`, not a boolean):
   - **ATTACK**: `currentATT += floor(baseATT * counter / 8)`
   - **BOOST**: `currentDEF += floor(baseDEF * counter / 8)`, `currentAGI += floor(baseAGI * counter / 8)`
   - **SLOW**: `currentDEF -= floor(baseDEF * counter / 8)`, `currentAGI -= floor(baseAGI * counter / 8)`
   - **STUN**: `currentAGI -= 5`, `currentMOV -= 1`
3. Walk the four inventory slots and apply each equipped item's stat operations **in actual
   carried-slot order**, clamping after each operation—there is **no fixed weapon-then-ring pass**,
   and summing multiple items' deltas is not generally equivalent when an intermediate step hits
   `0` or a cap ([equipment refresh order](../../../../remake/docs/domain/equipment-use-repair-design.md#whole-game-equipment-agreement-2268)).
   When only one item is worn and no intermediate clamp triggers, direct addition matches step-by-step
   refresh: for example, Slade's THIF base `ATT 47 / DEF 39 / AGI 40 / MOV 7` with max
   ATTACK/BOOST/SLOW + STUN + worn Thieve's Dagger (`ATT +17, AGI +5`, single worn item, no clamp
   hit) yields `ATT = 47 + 17 + 17 = 81`, `DEF = 39 + 14 - 14 = 39`, `AGI = 40 + 15 - 15 - 5 + 5 = 40`,
   `MOV = 6`. For allies, worn cursed gear re-applies `CURSE` during refresh; for enemies, item stat
   bonuses still apply while `CURSE` insertion is suppressed ([enemy curse suppression](../../contracts/party/level-up.md#confirmed-enemy-curse-suppression)).

**Confirmed stat clamps** ([clamp boundaries](../../contracts/party/level-up.md#confirmed-stat-clamp-boundaries)): base and current ATT/DEF/MOV increases saturate at **200**; **base AGI** clamps its low 7 bits to **100** (preserving bit 7), whereas **effective/current AGI** increases (e.g., Quick Ring, Quick Chicken) clamp their low 7 bits to **127** while preserving effective bit 7; unsigned current-stat subtractions clamp at **0** without wrapping. Extreme validation-seam edges (such as `IncreaseAndClampWord` wrapping `65535 + 2 → 1`) stay at the evidence boundary in [level-up.md](../../contracts/party/level-up.md#confirmed-stat-clamp-boundaries). **Unknown:** current-ATT decrease underflow and unused clamp helpers remain outside current runtime fixtures.

## 2. Physical damage, terrain land effect, and follow-ups

Every ordinary physical strike follows one ordered integer pipeline—no floating-point percentages
appear anywhere in original combat ([combat resolution](../../contracts/battle/combat-resolution.md#confirmed-resolution-pipeline)):

| Step | Formula / rule (integer truncation toward zero) | Scope and boundary |
| --- | --- | --- |
| 1. Dodge | Rolled **before** damage; a dodge skips all damage stages (`RNG(8) == 0` confirmed for ground non-archer vs. hovering target). | **Confirmed** for observed matchups; complete weapon/movetype dodge matrix remains **Unknown** ([dodge step](../../contracts/battle/combat-resolution.md#1-decide-dodge-before-calculating-damage)). |
| 2. Base gap | `damage = max(currentATT - currentDEF, 1)` | Uses already-refreshed current ATT and DEF ([base damage](../../contracts/battle/combat-resolution.md#2-calculate-base-physical-damage-with-integer-arithmetic)). |
| 3. Target land effect | Setting `0` (displayed `0%`): `floor(damage * 256 / 256)`<br>Setting `1` (displayed `15%`): `floor(damage * 230 / 256)`<br>Setting `2` / other (displayed `30%`): `floor(damage * 205 / 256)` | **Do not use displayed `15%` / `30%` as multipliers:** `230/256 ≈ 89.8%` (≈10.2% reduction) and `205/256 ≈ 80.1%` (≈19.9% reduction). Land effect reads the **target's** tile. |
| 4. Anti-air bonus | Flying or hovering target vs. Brass Gunner, Archer, Centaur Archer, or Stealth Archer: `damage += floor(damage / 4)` | Applied **after** land-effect truncation (`79 → floor(79*205/256) = 63 → 63 + floor(63/4) = 78`). |
| 5. Critical hit | Prowess definition 0 on first attack: `RNG(32) == 0` → `damage += floor(damage / 2)` (`78 → 117`). | Other critical definitions and second/counter critical eligibility remain **Unknown** ([critical step](../../contracts/battle/combat-resolution.md#3-apply-the-confirmed-critical-modifier)). |
| 6. Counter halving | Counterattack (attack type 2): `damage = floor(damage / 2)` **before** spread (`10 → 5`). | Applies only to counterattacks ([counter halving](../../contracts/battle/combat-resolution.md#4-apply-counter-half-damage-before-spread)). |
| 7. Downward spread & minimum damage | Ordinary path: `range = floor(damage / 8) + 1`; `damage = max(damage - RNG(range) - RNG(range), 1)` (same `range` for both draws). Burst Rock skips the two spread draws and also restores `0` to `1`. | **Minimum 1 applies after terrain, counter halving, and spread** ([pinned `InflictDamage`](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/inflictdamage.asm) · [shared spread contract](../../contracts/battle/spell-resolution.md#4-reuse-the-common-downward-damage-spread)). **Static check (Inferred):** a gap of `1` on plains (`floor(1 * 230 / 256) = 0`) or counter-halved `1` (`floor(1 / 2) = 0`) enters spread at `0`, draws `RNG(1) = 0` twice, and is restored to **`1` damage** (never `0`). Whenever pre-spread `damage < 8`, `range = 1` gives zero downward variance ([spread step](../../contracts/battle/combat-resolution.md#5-apply-the-two-roll-downward-spread)). |
| 8. HP clamp | Temporary HP subtracts final damage clamped at `0`; saved HP restores before persistent command replay (not healing). | Additional spread seeds and runtime lower-bound edges across all physical input ranges remain **Unknown**. |

**Single hit vs. double attack vs. counterattack:** a physical action resolves in fixed order
`first attack → second attack → counterattack` ([follow-up construction](../../contracts/battle/combat-resolution.md#7-construct-follow-up-attacks-in-original-order)).
Each strike runs its own dodge and damage stages, and the second hit reads the target's already
reduced temporary HP. A **lethal** first hit cancels both follow-ups; double attacks are also
blocked by a muddled actor or same-side target; counterattacks are blocked by out-of-range
distance, `SLEEP`, `STUN`, same-side target, or directional enemy exclusions (Burst Rock, Kraken
Head, Prism Flower, and Zeon Guard cannot counter; Taros cannot be countered).

### Worked Example 1: Battle02 plains vs. path survival (**Inferred** from confirmed rules)

**Declared setup** (`private-battle02-controlled-party` in
[`battle01-to-battle02-party.json`](../../../../remake/reference/inputs/battle01-to-battle02-party.json),
Normal/difficulty 0, fresh entry; stats are **already refreshed with equipped gear**—do not add
Short Axe ATT again; see [Battle02 first contact](../analyses/battle02-first-contact.md)):

- **Jaha**: `9 HP`, `ATT 14`, `DEF 7`, Short Axe (range 1–1), `MOV 5` (REGULAR).
- **Huge Rat (slot 131)** at `(10,13)` on plains: `10 HP`, `ATT 12`, `DEF 8`, range 1–1.
- **Sarah** (support): `11 HP`, `10 MP`, `HEAL 1` (`3 MP`, range 0–1, power `15`).

Assuming ordinary landed single hits (no dodge, critical, double, counter, or outside damage):

| Exchange & defender tile | Integer stages | Per-hit damage & survival result |
| --- | --- | --- |
| **Jaha → Rat** on plains (setting 1) | `max(14 - 8, 1) = 6` → `floor(6 * 230 / 256) = 5` → `range = floor(5/8)+1 = 1` → `5 - 0 - 0 = 5` | **5 damage** (Rat `10 → 5 → 0` in 2 hits; identical from either tile) |
| **Rat → Jaha** on plains `(11,13)` (setting 1, displayed 15%) | `max(12 - 7, 1) = 5` → `floor(5 * 230 / 256) = 4` → `range = 1` → `4 - 0 - 0 = 4` | **4 damage** (2 hits: `9 → 5 → 1 HP`; **survives** for Sarah's 3-MP HEAL 1) |
| **Rat → Jaha** on path `(10,12)` (setting 0, displayed 0%) | `max(12 - 7, 1) = 5` → `floor(5 * 256 / 256) = 5` → `range = 1` → `5 - 0 - 0 = 5` | **5 damage** (2 hits: `9 → 4 → 0 HP`; **defeated** before late healing) |

*Notice why the factor matters:* treating plains' displayed `15%` as `× 0.85` happens to match at
gap `5` (`4`), but diverges at gap `25` (`floor(25 * 230 / 256) = 22` versus `floor(25 * 0.85) = 21`),
and grass (`205/256`, displayed `30%`) at gap `20` gives `16`, not `14` (`× 0.70`).
**Unknown:** natural post-Battle01 party growth/loadout, actual RNG seeds, and whether Rat 131
naturally walks to `(10,13)` or attacks Jaha rather than Sarah remain unestablished; this is a
conditional comparison, not a guaranteed natural script.

## 3. Spell damage, elemental resistance, and healing

Attack spells and healing share the promoted-class `5/4` multiplier, but **resistances and caps
work very differently from physical combat** ([spell resolution](../../contracts/battle/spell-resolution.md#confirmed-damage-pipeline)):

| Stage | Attack-spell integer rule | Key caution & owner |
| --- | --- | --- |
| 1. Promotion scaling | If caster class ID `>= 12`: `adjusted = floor(basePower * 5 / 4)` (e.g., `10 → 12`, `18 → 22`). | Unpromoted casters keep `adjusted = basePower` ([caster adjustment](../../contracts/battle/spell-resolution.md#1-resolve-base-power-and-caster-adjustment)). |
| 2. Summon target split | For `DAO`, `APOLLO`, `NEPTUN`, `ATLAS`: `adjusted = floor(adjusted / targetCount)` per target (`22 / 4 → 5`). | Dividing by more targets shrinks per-target damage; complete natural dispatch for APOLLO/NEPTUN/ATLAS remains **Unknown**. |
| 3. Elemental resistance | Let `quarter = floor(adjusted / 4)`. Two-bit setting for the spell's element:<br>• `0` neutral: `adjusted`<br>• `1` minor resistance: `adjusted - quarter`<br>• `2` major resistance: `floor(adjusted / 2)`<br>• `3` **weakness**: `adjusted + quarter` | Uses truncated `quarter`, **not** floating-point `75%` / `125%`. **Critical consumer split:** setting `3` is **weakness** for damage spells, but **immunity** (unreachable threshold `8` on `RNG(8)`) for status spells like SLEEP, MUDDLE 2, SLOW, DISPEL, and DESOUL ([resistance step](../../contracts/battle/spell-resolution.md#2-apply-the-element-resistance-with-integer-truncation)). |
| 4. Spell critical | Confirmed for BLAZE: `RNG(32) == 0` adds the same `quarter` (`15 → 18`). | Other attack-spell critical branches remain **Unknown** ([spell critical](../../contracts/battle/spell-resolution.md#3-roll-spell-critical-before-shared-variance)). |
| 5. Downward spread | Reuses `InflictDamage`: `range = floor(damage / 8) + 1`, `max(damage - RNG(range) - RNG(range), 1)`. | Minimum post-spread spell damage is `1` ([shared spread](../../contracts/battle/spell-resolution.md#4-reuse-the-common-downward-damage-spread)). |

**Healing rules** ([HEAL 1 & healing boundaries](../../contracts/battle/spell-resolution.md#confirmed-heal-1-subset)):
ordinary `HEAL 1–3` and `AURA 1–3` apply promoted `5/4` scaling (`VICR`/`MMNK` `HEAL 1`: `15 → 18`;
`HEAL 3`: `30 → 37`) and then clamp recovery to `missingHp = maxHp - currentHp` (never overhealing
or reviving a `0 HP` ally). `HEAL 4` and `AURA 4` use power `255` as a **sentinel** that skips
promotion scaling and restores each living target's full missing HP. Status-spell thresholds,
recast quirks, and after-turn expiry are kept in their single owner
[spell-resolution.md](../../contracts/battle/spell-resolution.md) rather than duplicated here.

### Worked Example 2: BLAZE 2 resistance & HEAL 1 missing-HP cap (**Confirmed** controlled fixtures)

**2A. BLAZE 2 across all four FIRE resistance settings** (`sf2-spell-damage-resistance-v1`, base
power `10`, `6 MP`, four `100 HP` targets, controlled variance rolls `[0, 0]`):

| Target FIRE setting | Caster class | `adjusted` → `quarter` | Post-resistance | Critical (`RNG(32)`) | Spread `range` | Final damage |
| --- | --- | --- | --- | --- | --- | --- |
| `0` (neutral) | Unpromoted (`0`) | `10` → `2` | `10` | roll `29` (no) | `2` (`0, 0`) | **10** |
| `1` (minor res.) | Unpromoted (`0`) | `10` → `2` | `10 - 2 = 8` | roll `29` (no) | `2` (`0, 0`) | **8** |
| `2` (major res.) | Unpromoted (`0`) | `10` → `2` | `floor(10/2) = 5` | roll `29` (no) | `1` (`0, 0`) | **5** |
| `3` (weakness) | Promoted (`12`) | `floor(10*5/4) = 12` → `3` | `12 + 3 = 15` | roll `0` (`+3`) | `3` (`0, 0`) | **18** |

*Notice the integer truncation:* unpromoted minor resistance (`10 - floor(10/4)`) gives **8**, not
`7.5`. **Scope / Unknown:** this controlled fixture supplies the four-target list and switches
class 12 at the calculation seam; natural full promoted BLAZE and natural map-generated target
ordering for multi-target attack spells remain **Unknown**.

**2B. HEAL 1 missing-HP cap and healer minimum EXP** (`sf2-heal1-self-recovery-v1`, controlled
Battle 01 self-cast by unpromoted `PRST`, `HEAL 1` power `15`, `3 MP`, target `95 / 100 HP`):

1. Missing HP is `100 - 95 = 5`, capping recovery from `15` down to **`5 HP`** (HP `95 → 100`, MP `20 → 17`).
2. Because the caster is an allied `PRST` (`PRST`/`VICR`/`MMNK` whitelist), raw healing EXP is
   `floor(25 * 5 / 100) = 1`, which the healer minimum lifts to **`10`** (cumulative cap `25`).
3. Same-side targets **skip** Battle 01's EXP halving. Award `RNG(16)` rolls `14` and `0` apply
   `+0` and `-1`, producing persistent **`9 EXP`**.

## 4. Agility and turn order: why higher AGI never guarantees moving first

At the start of every round, only placed, living combatants enter the turn queue
([turn-order contract](../../contracts/battle/battle-control-lifecycle.md#turn-order-construction)):

1. Mask current agility to its low 7 bits: `base = currentAGI & 0x7F`, and compute `spread = base >> 3` (`floor(base / 8)`).
2. Compute the altered score: `score = (base + RNG(spread) - RNG(spread) + RNG(3) - 1)` wrapped to
   an **8-bit signed byte** (`-128..127`).
3. **Second turn in the same round:** if raw `currentAGI >= 128` (bit 7 set), add a second entry
   with `base2 = floor(base * 5 / 6)` and `base2 + RNG(base2 >> 3) - RNG(base2 >> 3)` (no `RNG(3) - 1`).
4. Stably bubble-sort all entries **descending as signed bytes**—equal scores keep insertion order
   (allies precede enemies in slot order).

Because `RNG(spread)` draws from `0 .. spread - 1` (or `0` when `spread == 0`), an ordinary
non-wrapping combatant's score spans `[base - spread, base + spread]` when `base >= 8` (and
`[base - 1, base + 1]` when `base < 8`).

### Worked Example 3: AGI overlap, tie-breaks, and signed-byte wrap

1. **Low-AGI overlap & tie-break** (Battle02 values — **Confirmed** rules, **Inferred** score
   bands; [Battle02 windows](../analyses/battle02-first-contact.md#when-does-sarah-actually-get-a-window)):
   Huge Rat (`AGI 7`), Sarah (`AGI 5`), and Jaha (`AGI 4`) all have `base >> 3 == 0`, so their
   scores are simply `AGI + RNG(3) - 1`: Rat `6..8`, Sarah `4..6`, Jaha `3..5`. Rat always beats
   Jaha, **yet** Sarah (`6`) beats Rat (`6`) on the tie because allies insert before enemies, and
   Jaha (`5`) can act before Sarah (`4`) despite lower AGI.
2. **Mid-AGI spread reversal** (**Inferred** from confirmed formula): once `AGI >= 8`, `spread = floor(AGI / 8)` widens the band. An `AGI 16` unit (`spread = 2`, each draw in `{0, 1}`) rolls in **`14..18`**, while an `AGI 14` unit (`spread = 1`, draws in `{0}`) rolls in **`13..15`**—so `AGI 14` rolling `15` acts before `AGI 16` rolling `14`.
3. **Signed-byte wrap & bit-7 double turn** (`sf2-turn-order-boundaries-v1`, **Confirmed** controlled
   Battle 01 runtime at seed `0x0000`):
   - Ally 1 set to `AGI 127` rolls raw byte **`135`**, which wraps to signed **`-121`** and sorts
     *last*, behind low-AGI enemies scoring `6, 5, 4`!
   - Ally 0 set to `AGI 128` (`bit 7` set, `low = 0`) receives **two turns** scored `0` and `255`
     (signed `-1`), while dead ally 2 and unplaced enemy 128 are omitted.

**Unknown:** status-modified agility, multiple `AGI >= 128` combatants interacting in one round,
and turn-order overflow outside these controlled Battle 01 seams remain unverified at runtime.

## 5. Action EXP, kill brackets, thresholds, and level-up

Combat rewards flow through **separate tracks**—never merge damage/kill EXP, healing EXP, status
EXP, and gold into one formula ([combat EXP](../../contracts/battle/combat-resolution.md#9-award-confirmed-battle-01-exp),
[spell EXP](../../contracts/battle/spell-resolution.md#5-accumulate-and-award-attack-spell-exp),
[level-up](../../contracts/party/level-up.md#confirmed-battle-award-entry)):

| Reward track | Accumulation formula (integer floor) | Per-action cap & modifiers |
| --- | --- | --- |
| **Damage & Kill EXP** | Effective level difference `diff = actorLevel + (promoted ? 20 : 0) - targetLevel` (`promoted`: class ID `>= 12`).<br>Kill bracket: `diff < 3 → 50`, `3 → 40`, `4 → 30`, `5 → 20`, `6 → 10`, `>= 7 → 0`.<br>Damage EXP: `floor(killBracket * finalDamage / targetMaxHp)`. A lethal hit adds damage EXP first, then full `killBracket`. | Each addition clamps the action accumulator at **`49`**. |
| **Healing EXP** | Allied `PRST`, `VICR`, or `MMNK` only (and `targetMaxHp > 0`): `max(floor(25 * recoveredHp / targetMaxHp), 10)` per target. | Shared cumulative cap **`25`** per action (e.g., `AURA 2` across 3 wounded allies accumulates `0 → 10 → 20 → 25`). |
| **Confirmed status / SPOIT EXP** | Observed for `ATTACK 1`, `BOOST 1`, `SLOW 1`, `SLEEP 1`, `MUDDLE 1/2`, `DISPEL 1`, effective `DETOX`, and `SPOIT` (including `SPOIT` on a `0 MP` target): `+5` per effective target (`0` on failure). **Exception:** `DESOUL` uses `STATUS` resistance, but successful kills grant **kill EXP (capped at `49`) + enemy gold**, not flat `+5` ([DESOUL rewards](../../contracts/battle/spell-resolution.md#confirmed-desoul-1-resistance-and-multi-target-rewards)). | Enters the action accumulator; other status spells remain **Unknown** until separately observed. |
| **Final EXP command** | Enemy-target actions in Battle 01 halve `floor(acc / 2)` (same-side actions skip halving). Then two ordered `RNG(16)` rolls: first `== 0` adds `+1`, second `== 0` subtracts `-1` (two zeroes cancel). | Final command has a **minimum of `1` EXP** even when `acc == 0` (e.g., promoted DAO vs. level-1 enemies). |
| **Gold** | Indexed from the 103-entry enemy gold table on kill; party balance saturates at **`9,999,999`**. | **Never** halved by Battle 01 and **never** randomized ([gold boundaries](../../../research/battle/runtime-rng-and-battle-math.md#confirmed-gold-addition-and-cap-boundaries)). |

**Stored EXP & single-threshold level-up:** when an EXP command replays, stored EXP saturates at
**`200`**, then checks `EXP >= 100` **at most once**—it never loops to grant two levels from one
command. Base classes (ID `< 12`) cap at **level 40**; promoted classes (ID `>= 12`) cap at
**level 99**. At the level cap, reaching `100 EXP` still subtracts `100` and invokes `LevelUp`,
which returns the no-level result (`255`) leaving level unchanged. Each non-capped active stat curve
consumes two `RNG(128)` draws plus an expected-minimum `+1` pity check (at level `>= 30`, a fixed
`1.5`-point base `384/256` replaces the 29-level projection table; [level-up sequence](../../contracts/party/level-up.md#confirmed-original-sequence)).
**Current HP and MP are never healed by leveling up.**

### Worked Example 4: EXP thresholds, 200 cap, and no-heal level-up (**Confirmed** controlled fixtures)

Four `24-EXP` Battle 01 command boundaries (`sf2-exp-command-boundaries-v1` and
`sf2-battle-exp-level-up-v1`):

| Starting state | `+24 EXP` command | Saturated sum | Post-threshold result |
| --- | --- | --- | --- |
| Lv 1, `75 EXP` | `75 + 24` | `99` (`< 100`) | Stays Lv 1, **`99 EXP`** (no level-up) |
| Lv 1, `76 EXP` | `76 + 24` | `100` (`>= 100`) | Subtracts `100` → Lv 2, **`0 EXP`** (1 level-up) |
| Bowie Lv 1, `99 EXP`, HP `12/12`, MP `8/8` | `99 + 24` | `123` (`>= 100`) | Subtracts `100` → Lv 2, **`23 EXP`**, max HP `12 → 14`, **current HP/MP stay `12/8`** |
| Lv 1, `199 EXP` | `199 + 24` | Capped at **`200`** | Subtracts **one** `100` → Lv 2, **`100 EXP`** remaining (still only 1 level-up) |

**Equipped refresh and cap companion** (`sf2-level-up-refresh-v1` / `sf2-level-up-boundaries-v1`):
Slade (`THIF`, level `39 → 40`) starts with base `ATT 45`, max HP `42`, **current HP `7`**, and
equipped Short Knife (`+5 ATT`). Leveling to `40` raises base ATT `45 → 47` and max HP `42 → 44`:
`UpdateCombatantStats` refreshes current ATT to **`47 + 5 = 52`** (recomputed from the new base,
never adding `+5` twice), while **current HP remains `7`**—Slade is still critically wounded. Once
at the unpromoted cap of `40`, another `76 + 24` command spends `100 EXP` (`→ 0 EXP`) with no level
gain. **Unknown:** non-Battle-01 natural EXP-modifier tables beyond the controlled `GiveExpAndGold`
table-miss seam remain unverified in natural play.

## 6. Original facts vs. accepted remake boundaries

- **Original facts stay separate from remake choices:** the formulas and quirks above describe the
  pinned USA Mega Drive/Genesis original (`SF2DISASM` `c834c652b6862bc5679fd7f69a38a7093206efc6` +
  pinned USA ROM). Original quirks such as TORT (class 11) spell-learning classification and failed
  `ATTACK`/`BOOST`/`SLOW` recast counter-vs-stat mismatches are **fidelity facts**, not automatic
  product defaults—any deliberate modernization requires its own decision and expected-deviation
  record. Extreme validation-seam overflow edges (such as `IncreaseAndClampWord` `65535 + 2 → 1`)
  remain bounded at [level-up.md](../../contracts/party/level-up.md#confirmed-stat-clamp-boundaries).
- **Accepted remake choices & deviations affecting observable runs:**
  - **Ninja Katana prowess (`0x94 → 0x24`):** preserving the original `INCREASE_DOUBLE` behavior
    (including clearing NINJ's counter field) is **already accepted and implemented** in the remake
    under `#2271` ([equipment prowess and Gisarme](../../../../remake/docs/domain/equipment-use-repair-design.md#equipment-prowess-and-gisarme-2271)).
  - **Finite music clock** ([music-wait policy](../../contracts/interface/music-wait-service.md#accepted-modern-finite-music-policy)):
    replaces original hardware wait-loop timing and can diverge whole-run RNG sequences without
    altering the individual combat/growth formulas above.
  - **Retained recast state across Continue:** the remake preserves unrefreshed `BOOST`/`SLOW`
    recast state across battle Continue until the next stat refresh ([design map](../../design-map.md#status-spells)).
- **Current playable support & Wiki note:** actual running engine coverage is owned by
  [capability status](../../../../remake/docs/capability-status.md#campaign-feature-trace). This
  reference is maintained in the canonical repository first; live Wiki synchronization happens in a
  separate reviewed publication step after acceptance.

## Basis and evidence map

| Topic | Canonical contract & evidence owners |
| --- | --- |
| Physical damage, land effect, anti-air, crit, spread, follow-ups & Battle 01 EXP | [Physical combat contract](../../contracts/battle/combat-resolution.md), [runtime battle math](../../../research/battle/runtime-rng-and-battle-math.md) |
| Attack spells, resistance, summon split, healing & healing EXP | [Spell resolution contract](../../contracts/battle/spell-resolution.md), [runtime battle math](../../../research/battle/runtime-rng-and-battle-math.md) |
| Movement costs, terrain decoding & range rings | [Battlefield navigation contract](../../contracts/battle/battlefield-navigation.md), [Battle02 first contact](../analyses/battle02-first-contact.md) |
| Turn-order score, second turns & signed-byte wrap | [Battle lifecycle contract](../../contracts/battle/battle-control-lifecycle.md#turn-order-construction), [runtime battle math](../../../research/battle/runtime-rng-and-battle-math.md#confirmed-statically-and-at-runtime-turn-order-score) |
| Stat refresh, status fractions, growth curves, caps & clamps | [Level-up contract](../../contracts/party/level-up.md), [runtime battle math](../../../research/battle/runtime-rng-and-battle-math.md#confirmed-statically-and-at-runtime-stat-gain-randomization) |
| Related L2 guides & encounter calibrations | [Tactical Battle Loop](tactical-battle-loop.md), [Progression and Economy](progression-and-economy.md), [Pre-voyage battle model coverage](../analyses/pre-voyage-battle-model-coverage.md), [Design Map](../../design-map.md) |
