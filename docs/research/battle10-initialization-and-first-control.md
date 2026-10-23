# Battle10 initialization, first control and conditional Power Ring custody

Evidence date: 2026-10-22. **Confirmed:** the pinned source connects the complete
selected12/12/3/1 formation to initialization, repeat upgrades, activation and
conditional player/AI/no-action dispatch. Witch129's declared equipped Power Ring
passes through an item-mask consumer before equipment refresh; its conditional
drop has a separate table and one-time bit. **Inferred:** TO_THE_EAST is the
intended eastern encounter after Ribble. **Unknown:** natural party/state,
substitutions, actual first actor/actions, presentation and durable custody.
Static source composition is not observed play or engine-ready content.

## Agreement and provenance

This is the Standard-rigor agreement for
[Issue1230](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1230), within
[Epic1053's finite scope](./story-content-coverage.md#scope-and-reading-rules).
It supports content maintenance and future conversion by joining all selected
operands to actual consumers, with one bounded Power Ring initialization/use/drop
relationship. Cross-owner state and item-return semantics justify Standard rigor.
Stop at first conditional control, plus that item's conditional custody conclusion;
do not execute a battle or expand into every action/item. The
[admission owner](./ribble-may-through-battle10-admission.md) retains May, the
Map8→69→70 route, partial reload, ordered selection and empty hooks. The
[outcome owner](./battle10-outcomes-through-east-shrine-visit.md) retains outcomes,
returns and the optional shrine. Shared rules remain at
[Battle02](./battle02-initialization-and-first-control.md),
[Battle09](./battle09-initialization-and-first-control.md) and
[Battle14](./battle14-desert-encounter-and-outcomes.md).

Owned writes are this document and Battle10 synchronization in the two coverage
owners. All other documents/code, fixtures/schemas/registries/counters and runtime
frontiers are read-only. No parser, new platform, allowlist, engine implementation,
extraction, lowering, comparison or acquisition is added. Acceptance is complete
selected source/consumer review, direct document/scope/private checks, clean
committed planner and exact-head scoped CI, then frozen pushed Draft PR and
independent main-gate review. PR692 is unconsumed.

Original paths resolve under `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at that pin. Accepted repository base is
`1ab9cdbfd99b605ebb4c67f2d56052cbcf91609f`, tree
`0cf4c56b6e2dcc4e6b180d6787c7f33ed522ffbc`. The
[USA baseline](../../README.md#pinned-original-baseline) retains input identity;
no new ROM-equivalence or runtime claim is made. Short battle code paths below
have prefix `code/gameflow/battle/`; sibling filenames in one table cell retain
the preceding directory. `j_` interfaces resolve to the named bodies.
Read instructions and return tails, not only comments or symbolic declarations.

| Claim | Declaration and actual consumer |
| --- | --- |
| Complete12/12/3/1 | `data/battles/spritesets/entries.asm` slot10 → `spritesets/spriteset10.asm:BattleSpriteset10`, `0x1B3AA8..0x1B3BF2`; `sf2macros.asm:allyCombatant/enemyCombatant/combatantAiAndItem/combatantBehavior`, `sf2enums.asm`; `getbattlespritesetsubsection.asm:GetBattleSpritesetSubsection`, `0x1B1630..0x1B169E`. |
| Selection, terrain/globals and entry | `code/common/maps/getbattle.asm:CheckBattle`, `0x799C`, `code/gameflow/mainloop.asm`; row10 in `data/battles/global/battlemapcoords.asm`, `custombackgrounds.asm`, `enemyleaderpresence.asm`; complete EXP/after-position/neutral tables; `terrainentries.asm` slot10 → `battleloop/loadbattleterraindata.asm`; `battleloop_1.asm:BattleLoop`, `0x23A84`; before/start wrappers and selected empty10 hooks; `battlefunctions/loadBattle.asm`, entity `PositionBattleEntities`. |
| Actual party and refresh | `battleloop/heallivingandimmortalallies.asm`, complete `initializecombatants.asm`, `0x1B1272..0x1B15F8`; `code/common/stats/battleparty.asm`, `updatecombatantstats.asm`; map `csc08_joinForce/csc54_joinForceAi/csc56_addFollower`, follower initialization/list consumers named by admission/Battle09. |
| Base definitions and repeated entry | `data/stats/enemies/enemydefs.asm` records4/6/41/57/68; `battleloop/upgradeenemies.asm`, `0x1B1780..0x1B1A28`; `data/battles/global/randombattles.asm`, `0x1B6DB0..0x1B6DDA`; upgrade/exclusion macros, constants and `GenerateRandomNumber`. |
| Ring initialization/use | `InitializeEnemyItems` → S02 `j_AddItem` → `code/common/stats/itemstats.asm:AddItem/GetEquippedItemByType/IsItemUsableInBattle/IsItemUsableByCombatant`; `updatecombatantstats.asm`; `data/stats/items/itemdefs.asm` row19 and weapon rows33/46/87; `data/stats/spells/spelldefs.asm:ATTACK/BLAZE/FREEZE`; `ai/getnextuseableaiaction.asm`, `gethighestusablespelllevel.asm`, `command/support.asm`, `command/attack.asm`. |
| Ring conditional drop | `data/battles/global/enemyitemdrops.asm`, `0xBE52..0xBECC`; `sf2macros.asm:enemyEntity/droppedFlag`; `battleactions/battleactionsengine_1.asm` three after-effect call sites → `battleactions/dropenemyitem.asm:battlesceneScript_DropEnemyItem`, `0xBD24..0xBE52`; `code/common/stats/itemstats.asm:GetItemSlotContainingIndex/RemoveItemBySlot/AddItem`, `code/common/stats/dealsinventory.asm`, `0x99DE..0x9A3C`. |
| Regions, orders and first control | Complete `battleloop/activateenemies.asm`, `triggerregions.asm`, `0x1AC9FC..0x1ACF30`, `clearaimemory.asm`, `populateenemyspawns.asm`; sparse region-cutscene table; stats `GetMoveOrders/GetTriggerRegions/SetTriggerRegions`; `battlefield/getmoveorderposition.asm`; `ai/startaicontrol.asm`, both `determineaistandbymovement_*.asm`, `executeaicommand.asm`, `command/moveorder.asm`, `command/move.asm`; `data/battles/global/aicommandsets.asm`; `battleloop/turnorderfunctions.asm`; `battlefunctions/executeindividualturn.asm`, `0x23EB0..0x24242`. |

The existing [spriteset](./battle-spriteset-data.md), [global](./battle-global-data.md),
[routing](./battle-routing-data.md), [loop](./battle-loop.md), [AI](./battle-ai.md)
and [cutscene](./battle-cutscenes.md) inventories/fixtures remain unchanged.
Spriteset `rowContentTracked:false` and routing `routeTargetsParsed:false` remain
their executable limits; they do not establish a new executable Battle10 composition.

## Selection, globals and entry lifetimes

**Confirmed:** `BATTLE_TO_THE_EAST=10` selects Map70, area`(0,0,32,32)` and
wildcard triggers255/255. The admission owner's complete10-before14 scan retains
F410/F510 semantics: set410 selects10 even with414; set510 clears410 but still
selects10 on that call. The default Map70 zone sets410 only with510 clear, while
a carried410 can select a completed repeat. Area fields are outputs, not bounds
checked against the incoming position. Intro460, completion510 and suspension88
are independent. None of these gates requires May, Peter or a fixed party.

Terrain slot10 selects `BattleTerrain10`, Stack-decoded into the shared48×48
array, distinct from the32×32 operating crop. Background10 is OVERWORLD255;
`GetBattlesceneBackground` resolves that through combatant terrain/map blocks,
not a uniform image. Leader-presence10 is0, so no leader-death sweep follows
from killing Dark Dwarf128. The half-EXP list contains only1, after-position
overrides only5, and neutral table has no10 row. Their consumers supply no10
override or extra neutral. Terrain traversal/rendering is not newly decoded.

| Supplied state | Actual entry sequence |
| --- | --- |
| F88 clear, F460 clear | Clear PLAYER_TYPE/set399; clear seconds; save/install map/battle/base VInts; call empty before10; restore/reassert map/battle; clear90–105; heal; initialize actual allies then enemies; clear AI memory; `LoadBattle(0)`; start wrapper sets460 then runs empty10; prepare round. |
| F88 clear, F460 set | Skip both hook bodies; fresh healing, placement, upgrades, refresh, load and round still run.510 is not460. |
| F88 set | Restore saved seconds; clear88/AI memory/view target; load using incoming focus; enter retained queue/cursor. No fresh map/battle installation, region reset, healing, formation, upgrade, hook or new round precedes that loop. A retained sentinel later requests a round. |

Both selected scripts are `ms_Empty`; the interpreter terminator can still
wait for scrolling and clear scroll speed. Neither hook sets EGRESS_MAP, joins
May/Peter, clears their AI or installs followers. Supplied stored egress is
consumed by the existing savepoint/F399 route; no fixed egress tuple is inferred.
`LoadBattle` rebuilds entities from actual combatants and loads map/sprites/
terrain/music/fade services. Map followers are not a second battle roster.
Natural admission, successful services and saved-state restoration stay Unknown.

## Twelve templates versus actual May and Peter

**Confirmed:** header4 +24 combatant records×12 +3 regions×12 +1 point×2
=330 (`0x14A`), matching `0x1B3AA8..0x1B3BF2`. The subsection getter consumes
those counts/strides. Every ally template declares HEALER1/0, NOTHING127,
NONE255/region15 twice, filler0 and STARTING0. Complete ordered positions:

| Template | Local X,Y | Template | Local X,Y |
| --- | --- | --- | --- |
| 0 | `(2,9)` | 6 | `(1,7)` |
| 1 | `(3,8)` | 7 | `(1,9)` |
| 2 | `(1,8)` | 8 | `(4,7)` |
| 3 | `(2,8)` | 9 | `(3,6)` |
| 4 | `(3,7)` | 10 | `(2,6)` |
| 5 | `(2,7)` | 11 | `(1,6)` |

The [shared membership/cursor rule](./battle02-initialization-and-first-control.md#joined-active-dead-and-capacity)
resets all30 ally Xs, rebuilds ascending joined/active lists, then copies only
template X/Y to actual living members. It does not install template identities,
AI or inventory. Dead ordinary members consume list iterations without advancing
the placement cursor. Twelve iterations are the local capacity; any remaining
active tail is passed to `LeaveBattleParty`, clearing active/X but preserving
joined. Lists are not rebuilt again there. Even with unused positions after
deaths, a thirteenth member is removed. No pre-loop empty-party guard establishes
a safe natural zero-party entry.

May's [actual producer](./ribble-may-through-battle10-admission.md#membership-active-capacity-ai-and-current-following)
uses ordinary `join`: F8 joined, then F40 activated only if the rebuilt active
count is below12; a pre-existing40 is not cleared by the full-party branch.
It does not revive or set/clear her AI bit. `addNewFollower` installs a transient
entity relation, not F40; the permanent follower table has no May row. A later
ordinary map rebuild does not derive her following from F8. Empty10 hooks repair
none of these states. Actual8/40/HP/list cursor determine whether and where she
is placed; current AI/status determines reached control, not template8.

Peter's [independent states](./battle09-initialization-and-first-control.md#twelve-templates-and-peters-independent-states)
remain F7 joined, conditional F39 active, combatant AI bit2 and map follower F67.
The castle `joinForceAI PETER,$FFFF` sets bit2 through the actual activation-word
consumer; map follower initialization instead tests its own flag row. F67 or a
visible Peter is not proof of F7/F39 or deployment. If actually selected, immortal
Peter7 is healed even from HP0 and uses the real placement cursor. A retained AI
bit takes ally commandset6 at his reached turn, independently of template HEALER1.
`ClearAiMemory` clears48 target bytes/48 memory bytes, not these identity/AI flags.

Healing scans all30 allies independently of membership, restoring max HP/MP to
living allies and immortal Peter7/Lemon28; ordinary dead allies stay dead.
Intermediate `status & 7` retains existing STUN/POISON/CURSE. Subsequent stat
refresh preserves STUN/POISON and recomputes final CURSE from equipped cursed
gear. An old CURSE bit alone proves no final curse. Actual gear/status/class/
level/active history and capacity remain inputs, not twelve fixed heroes.

## Twelve enemies and five base definitions

**Confirmed:** all rows have secondary NONE255/region15, filler0 and STARTING0;
all added items are NOTHING127 except row1. Slots128–139 initialize immediately;
undeclared140–159 have current/maxHP0 and X=-1, not hidden live enemies. Mode0
initialization has no spawning-position occupancy gate. Complete differences:

| Row / slot | Base type / local X,Y | Commandset | Added item | Primary order / region |
| --- | --- | --- | --- | --- |
| 0 /128 | DARK_DWARF6 `(27,25)` | ATTACKER3/9 | NOTHING | MOVE_TO\|0 (`$40`) /0 |
| 1 /129 | WITCH68 `(28,24)` | ATTACKER3/9 | POWER_RING\|EQUIPPED\|UNUSED_ITEM_DROP (`$4093`) | MOVE_TO\|0 /0 |
| 2 /130 | VAMPIRE_BAT41 `(23,19)` | ATTACKER1/6 | NOTHING | MOVE_TO\|0 /1 |
| 3 /131 | GOBLIN4 `(20,28)` | ATTACKER1/6 | NOTHING | NONE /2 |
| 4 /132 | HUNTER_GOBLIN57 `(22,29)` | ATTACKER1/6 | NOTHING | NONE /2 |
| 5 /133 | VAMPIRE_BAT41 `(5,29)` | ATTACKER2/7 | NOTHING | FOLLOW_TARGET\|0 (`$00`) /0 |
| 6 /134 | WITCH68 `(16,16)` | ATTACKER1/6 | NOTHING | NONE /15 |
| 7 /135 | VAMPIRE_BAT41 `(15,7)` | ATTACKER1/6 | NOTHING | FOLLOW_TARGET\|0 /2 |
| 8 /136 | GOBLIN4 `(10,10)` | ATTACKER1/6 | NOTHING | FOLLOW_TARGET\|0 /15 |
| 9 /137 | HUNTER_GOBLIN57 `(8,14)` | ATTACKER1/6 | NOTHING | FOLLOW_TARGET\|0 /15 |
| 10 /138 | GOBLIN4 `(7,15)` | ATTACKER3/9 | NOTHING | FOLLOW_ENEMY\|9 (`$89`) /15 |
| 11 /139 | DARK_DWARF6 `(18,15)` | ATTACKER3/9 | NOTHING | FOLLOW_ENEMY\|6 (`$86`) /15 |

Orders and regions are separate bytes. NONE255 differs from region15; MOVE_TO
bit6 resolves the low nibble through the point subsection. The sole point0 is
`(17,25)`, not a combatant or region vertex. FOLLOW_TARGET0 resolves live ally0;
FOLLOW_ENEMY9/6 resolve live137/134, not ally9/6 or point9/6. The position getter
reads current X/Y, not starting positions; declaration alone guarantees no motion.

Each definition is56 bytes; all four item/spell slots are accounted for below
(unlisted slots are NOTHING), all five have REGULAR spell power and status NONE.
Values are base stats, before difficulty, equipment and status refresh:

| Type | Level / HP / MP | ATT,DEF,AGI,MOV | Definition equipment / spells | Move / resistance / priority flags |
| --- | --- | --- | --- | --- | --- |
| Dark Dwarf6 | 7 /21 /0 | 18,17,18,4 | equipped Short Axe /none | REGULAR /0 /PRIORITYMOD_2\|BIT5 |
| Goblin4 | 6 /18 /0 | 14,13,13,5 | equipped Short Axe /none | REGULAR /0 /PRIORITYMOD_2\|BIT5 |
| Hunter Goblin57 | 7 /24 /0 | 8,12,14,5 | equipped Iron Arrow /none | STEALTH_ARCHER /0 /PRIORITYMOD_2\|BIT4 |
| Witch68 | 7 /21 /10 | 9,12,15,6 | equipped Bronze Rod /FREEZE1 | MAGE /ICE_MINOR /PRIORITYMOD_2\|BIT4 |
| Vampire Bat41 | 7 /20 /8 | 21,16,16,6 | none /BLAZE2 | FLYING /0 /PRIORITYMOD_2 |

Dwarf/Goblin/Witch prowess is CRITICAL150_1IN32/DOUBLE_1IN32/COUNTER_1IN32;
Hunter doubles/counters are1IN16; Bat has CRITICAL_POISON with1IN32 double/counter.
These are different physical-action operands, not all ordinary critical rules.
Initialization selects an optional substitute **before** copying its complete
definition; restores HP/MP to maxima; installs row X/Y, definition movement plus
placement commandset, actual enemy index, regions/orders; adds placement item;
constructs activation; adjusts difficulty ATT; calls `UpdateCombatantStats`.
Only priority upper nibble survives definition activation flags; zero filler/mode
make initial words `$2000`, not initially active BIT4/5. Difficulty flags78/79
produce floor(baseATT×5/4) for every non-SUPER value and twice for SUPER2, before
equipment. Difficulty0 base current ATT is27 Dwarf,22 Goblin,22 Hunter,19 Witch,
26 Bat; Witch129 does **not** get another5 from its fresh placement Ring.

Short Axe33 is WEAPON ATT+5/range1–1; Bronze Rod87 ATT+8/range1–1; Iron Arrow46
ATT+12/range2–2. Their useSpell NOTHING63 differs from the erroneous255 no-use
comparison; the [item-getter caveat](./battle05-initialization-and-first-control.md#eight-enemies-equipment-and-available-action-operands)
retains fallback HEAL/type/AI-bit checks. They are physical gear, not healing items.
BLAZE2 is silence-affected TYPE_ATTACK, MP6/range1–2/radius1/power10; FREEZE1
MP3/range1–2/radius0/power10. Base fresh Bat8MP/Witch10MP cover those costs.
Actual attack getters still require a usable affordable known level, confusion
eligibility, target/priority and casting station; earlier move-order success or
standby can bypass casting. HEAL command suffixes install no HEAL spells.

## F510 and repeated-entry definition selection

**Confirmed:** TO_THE_EAST10 is the fourth member of the actual length-prefixed
random-battle list. `IsBattleUpgradable` succeeds; `DetermineBattleUpgrade`
tests **F510**, not460/410. Clear510 returns base without upgrade RNG. Set510
permits per-row selection even after CheckBattle clears410. The
[Battle14 algorithm](./battle14-desert-encounter-and-outcomes.md#completed-repeats-and-actual-enemy-upgrade-selection),
[Battle03 airborne account](./battle03-initialization-and-first-control.md) and
[Battle09 selection](./battle09-initialization-and-first-control.md#f509-effective-level-and-actual-repeat-selection)
retain shared implementation. This encounter selects these four categories:

| Original move / bases | R | Inclusive bounds | Excluded IDs |
| --- | --- | --- | --- |
| REGULAR /Dwarf6,Goblin4 | 5 | OOZE0..REAPER35 | KRAKEN_LEG10, PAWN13, KNIGHT14, RAT15, BURST_ROCK32, ZEON_GUARD38 |
| FLYING /Bat41 | 2 | HUGE_BAT40..DEMON52 | count0; unsafe consumer below |
| STEALTH_ARCHER /Hunter57 | 2 | GALAM_ARCHER56..DARK_GUNNER65 | KRAKEN_ARM59, ROOK61 |
| MAGE /Witch68 | 1 | GALAM_MAGE67..CHAOS_WIZARD74 | QUEEN71 |

Category uses the original definition's movement upper nibble, not placement AI
or substituted movement. Bowie0 level supplies E; signed-byte class>11 adds20.
Compute `E−10`; nonpositive values repeat the same read once using shared D4,
then return base. Positive values DIVU10 and retain quotient low byte q; q0
returns base. Ordinary selection first becomes possible at **E20**. Corrupt/
wrapped values are not normalized. D3 starts at base; offset is`5q−5` for R5,
q for R1/2; D2 saves base+offset. RNG range R+1 supplies0..R. Signed-byte bounds
and exclusion tests accept or consume the one shared D4 retry, restoring D3=D2
before the next draw. Second failure returns base, without clamp/unlimited reroll.
At q1 pre-filter candidates are6..11 Dwarf,4..9 Goblin,42..44 Bat,58..60 Hunter,
69..70 Witch. Dwarf10 and Hunter59 are exclusions. These are candidate domains,
not selected outcomes, probabilities or guaranteed improvements.

Airborne `excludedEnemies` emits count0. Its actual consumer subtracts1 without
a zero guard and DBFs over **65,536 subsequent bytes** unless an equality occurs.
An accidental match takes retry/fallback; exhaustion can accept a candidate.
The comment does not establish safe base fallback or a valid empty-set filter.
This affects Bat130/133/135; no out-of-table census/ROM scan/runtime simulation
resolves their actual substitutions here. All twelve calls occur128–139; earlier
draws/retries advance the shared main seed and affect later selection/round RNG.

Returned definitions change stats/gear/spells/movement/resistance/prowess/priority;
placement commandset/X/Y/orders/regions remain selected row operands. Row129
still passes its Ring word to `InitializeEnemyItems` after **whichever** definition
was copied. F88 skips fresh selection entirely. This is per-initialization
definition replacement, not in-place level growth or a per-round upgrade.

## Power Ring: declaration, returned item and use predicates

**Confirmed:** placement row1 declares Ring19|EQUIPPED128|UNUSED_ITEM_DROP$4000
=`$4093`, separately from Witch68's equipped Bronze Rod87. Item19 has equipFlags
ALL, range0–0, price3000, type RING|RARE|BREAKABLE, useSpell ATTACK1 and equip
effect INCREASE_ATT5. ATTACK1 is TYPE_SUPPORT|TARGET_TEAMMATES|AFFECTEDBYSILENCE,
MP7, range1–3/radius0/power15; its name is not an offensive attack-spell category.

`InitializeEnemyItems` compares the complete placement word to NOTHING, calls
`j_AddItem`, then tests EQUIPPED bit7 in the **returned d1**, not the saved input.
`AddItem` scans four slots by index mask `$7F`. On success it ANDs d1 with
`$807F`, stores it and returns d2=0 without restoring d1. Thus `$4093` becomes
`$0013`: equipment bit7, AI-use bit13 and unused-drop bit14 are absent. The next
bit7 test fails, skipping `EquipItemBySlot`. Base Witch's empty slot1 receives
unequipped Ring19; slot0 remains equipped Bronze Rod. `UpdateCombatantStats`
only applies item effects for equipped entries, so this Ring gives no fresh
ATT+5. The declared EQUIPPED spelling is not the actual post-AddItem state.

For a substituted definition with an empty slot, the same mask/return path adds
unequipped19 to its first empty slot, preserving its other definition gear.
If all four slots are occupied, AddItem returns d2=1 **without masking d1**.
Initialization ignores d2, sees original bit7 and scans existing entries for a
matching low byte `$93`; only a match can invoke EquipItemBySlot. It adds no
new Ring in that branch. A pre-existing Ring and its flags are supplied by that
definition, not conjured by placement. No exhaustive substitution/inventory result
is claimed, including outcomes of the airborne defect. Actual held index19 is
the later drop prerequisite whether newly added or already present.

Battle usability and automatic selection are separate consumers:

- `IsItemUsableInBattle` tests useSpell against255 (its known NOTHING bug);
  Ring's real ATTACK passes. `IsItemUsableByCombatant` additionally checks equipment
  compatibility for weapons/rings. This establishes a predicate, not performed use.
- `GetNextUsableAttackItem` requires equipped or AI-use bit13 before accepting an
  offensive TYPE_ATTACK use spell. Fresh `$0013` fails that permission; ATTACK1's
  support type would also fail the offensive filter if permission were supplied.
  Base Witch's earlier equipped Bronze Rod slot already reaches the known
  NOTHING→fallback-HEAL/type rejection and terminates this getter before Ring;
  checking Ring directly would not make it selectable. Confused actors have an
  additional BLAZE/FREEZE/BOLT/BLAST whitelist.
- `GetNextUsableHealingItem` permits Healing Rain or AI-use bit13, then requires
  TYPE_HEAL. Ring is neither Rain nor healing and fresh13 has no bit13.
- `aiCommand_Support` scans **learned** support spells, not item use spells; its
  initial whitelist admits MUDDLE2/DISPEL1 only. Later ATTACK prioritization code
  does not bypass that gate. Witch's learned FREEZE1 and held Ring do not provide
  an automatic support-item path. Generic usability alone proves no Witch use.

Different substituted learned spells/loadouts remain conditional inputs; no full
item-action, breaking, damage/status or tactical audit is added to this slice.

## Power Ring: conditional action-drop custody

**Confirmed:** the separate drop table row for battle10 declares enemyEntity1
→actual129 (`enemyEntity` adds128), item19, droppedFlag4. It does not inspect the
placement word's unused-drop bit14. The
[Battle14 shared action-drop boundary](./battle14-desert-encounter-and-outcomes.md#terminal-power-stick-victory-and-retreats)
retains caller/cleanup distinctions; this Ring's actual RARE property changes
the fallback outcome compared with the nonrare Power Stick.

`battleactionsengine_1.asm` clears local `targetDies`, applies the action effect,
then calls `battlesceneScript_DropEnemyItem` for each target, second attack and
counter branch. A4/A5 at each call define actual actor/target; a counter can reverse
their original roles. The function requires ally actor, enemy target, local
targetDies nonzero, current battle10 and target129. `GetItemSlotContainingIndex`
scans all four actual target slots with `$7F` masking for19: unequipped/broken
flags do not prevent an index match, but absent19 rejects before any drop bit.
It never awards a missing table item or uses the Witch's original type as identity.

Ring is not Taros Sword/Iron Ball/Counter Sword, so no special1/32 roll applies.
The unreachable upgradable-battle block adds no repeat chance. On a match:

1. DIVU8 locates bit4 in `ENEMY_ITEM_DROPPED_FLAGS` (`0xFFF7A4`). BSET occurs
   before removal; an already-set bit returns with item/recipient unchanged.
2. A clear bit becomes set; `RemoveItemBySlot` removes the matched target slot,
   compacts the four-item array, appends NOTHING and refreshes target stats.
3. Read the **actual actor's** current HP. Nonzero HP then successful AddItem
   gives plain index19 to that actor and emits dropped/received messages. No
   search for another party member occurs; actor is not necessarily Bowie.
4. HP0 or full inventory instead reads item's definition RARE bit. Ring has it,
   so `AddItemToDeals` increments its nibble unless already15. Index19 maps to
   odd low nibble at `DEALS_ITEMS+9` (`0xFFF60D`); max15 silently keeps15. No
   receipt message or guaranteed increment at saturation is implied. The item
   has already been removed and bit4 remains set even when deals is saturated.

This ties original/substituted initialization to actual custody without assuming
death equals receipt. Poison/death cleanup, debug kill-all, leader cleanup and
victory are not these action-drop call sites. No automatic victory Ring reward,
drop-bit reset by F510, guaranteed acquisition or actual save persistence follows.
The outcome owner retains victory/defeat/escape/return behavior; this subsection
adds only the selected item's conditional relation, not another completed battle.

## Three ordered regions and activation state

**Confirmed:** all region records are type4, byte1=0, trailing0/0:

| Region / persistent flag | Vertex1 /2 /3 /4 |
| --- | --- |
| 0 /90 | `(20,0)` /`(6,31)` /`(30,31)` /`(30,0)` |
| 1 /91 | `(24,0)` /`(11,31)` /`(30,31)` /`(30,0)` |
| 2 /92 | `(16,0)` /`(2,31)` /`(30,31)` /`(30,0)` |

`IsRegionTriggered` tries triangles1/2/4 then3/2/4, scanning all30 allies with
positive signed HP and nonnegative signed-byte X/Y, independently of joined/
active/follower flags. `CheckTestPointPositionRelativeToSegment` uses signed
MULS/DIVS truncated toward zero, horizontal dominance on equal spans, projected
coordinate versus actual, bounded collinearity and synthesized Z/C flags.
Collinear in-bounds01 is immediate inside. Ordinary side flags must match the
opposite vertex for each edge; bit0 exceptions skip either early edge but reject
at the third. Negative projection explicitly writes CCR=1 →10, despite the
misleading11 comment. This is not arbitrary cross-product geometry.

Direct substitution of **all12** fresh positions gives complete witnesses:

| Region / triangle | First rejection for every template | Integer projection witness |
| --- | --- | --- |
| 0 /1,2,4 | First edge1→2: ally00 versus vertex4's10 | Ally X projection15–17 at Y6–9; vertex4's projection20 < actual30. |
| 0 /3,2,4 | Third edge4→2: ally00 versus vertex3's10 | Ally X projection23–25; vertex3 at Y31 projects6 < actual30. |
| 1 /1,2,4 | First edge1→2: ally00 versus vertex4's10 | Ally X projection20–21; vertex4 projects24 < actual30. |
| 1 /3,2,4 | Third edge4→2: ally00 versus vertex3's10 | Ally X projection24–26; vertex3 projects11 < actual30. |
| 2 /1,2,4 | First edge1→2: ally00 versus vertex4's10 | Ally X projection11–13; vertex4 projects16 < actual30. |
| 2 /3,2,4 | Third edge4→2: ally00 versus vertex3's10 | Ally X projection21–24; vertex3 projects2 < actual30. |

Actual fresh living placements use a prefix/subset of these positions; ordinary
dead members consume iterations but not coordinates, and unplaced X resets to-1.
Thus no fresh90/91/92 hit follows. This finite substitution proves no later
crossing, suspended-state result or geometry equivalence for other coordinates.

`ActivateEnemies` scans128–159 then its missing outer RTS falls through once
for160; negative X/HP0/nonenemy skips testing. The
[extra160/deals alias owner](./battle26-activation-and-first-turn-dispatch.md#the-extra-fall-through-and-its-selected-ram-alias)
retains that conditional defect. NEWLY_TRIGGERED_BATTLE_REGIONS is a tested-word
cache, distinct from persistent90–105: misses are cached too. Fresh flag reset/
ClearAiMemory do not explicitly clear it; later non-swarm `StartAiControl` does.
Incoming tested bits can suppress checks. Clear incoming bits with these fresh
positions yield tested7 and no90/91/92.

Assigned primaries:0 for128/129/133,1 for130,2 for131/132/135. All secondaries
are15. Primary hit sets bit0 then returns, preserving existing bit1; only a
secondary hit sets bits0/1. No region15 exists.134/136–139 have15/15 and bypass
inactive-region standby. Activation does not swap orders. On the activated path,
dead-follow HP0 promotes secondary NONE and clears secondary:133/135/136/137
for ally0,138 for137,139 for134. MOVE_TO0 bypasses that follow-HP check.

## Standby, commands, round and conditional control

**Confirmed:** the sparse region program table has only32/40, not10. All spawn
modes are0; no hidden/respawn population candidate or selected neutral adds a
thirteenth enemy. With fresh region flags clear the actual standby paths are:

| Slots | Actual inactive/unconditional path |
| --- | --- |
| 128/129/130: point0 with assigned region;133/135: ally0 with assigned region | `ValidateAiStandbyEligibility` returns nonzero d1; caller treats it as stop and writes empty move string/STAY. Its misleading permission comment does not reverse this branch. Earlier thinking RNG can also stop. |
| 131/132: NONE with region2 | Returns d1=0/d2=0; if earlier thinking-RNG stop is avoided, anchor at actual starting position and conditionally build a local move string. STAY action need not mean no movement. |
| 134/136–139:15/15 | Enter placement commandset regardless of activation bit0. |

Actual streams, attempted in order until success:

| Set / selected enemies | Commands / preparation |
| --- | --- |
| ATTACKER1/6:130–132,134–137 | ATTACK1, HEAL1, SUPPORT, MOVE1, STAY /regular |
| ATTACKER2/7:133 | MOVE_ORDER1, ATTACK1, HEAL1, SUPPORT, MOVE1, STAY /regular |
| ATTACKER3/9:128/129/138/139 | MOVE_ORDER3, ATTACK3, HEAL2, SUPPORT, MOVE_ORDER4, MOVE2, STAY /block non-movable |

MOVE_ORDER1 is target type0/regular;3 is follow type1/block-non-movable;4 is
target type0/block-non-movable. The stored order's bit6 still resolves point0
versus live follow position: command names do not convert133's ally0 into an
AI point. Type-specific movement/attack tests can fail or return STAY plus a move
string. Set6 has **no MOVE_ORDER**;130's point0 and135/136/137's ally0 still affect
standby/dead-follow logic, but do not promise pursuit of those references.
Its MOVE1 uses opponent scoring/pathfinding, with regular mode skipping order
obstructions. Set9's referenced137/134 and128/129's point0 reach actual ordered
movement attempts, not guaranteed movement. Ally AI uses set6 separately from
template HEALER1/0's HEAL1,SUPPORT,ATTACK1,MOVE1,STAY.

Round prefix is activation → region program → spawn selection → turn order.
The [shared round rule](./battle02-initialization-and-first-control.md#first-round-and-conditional-control-endpoint)
clears64 sentinel records, scans30 allies then first30 enemy slots, admits
living placed actors independently of activation and uses refreshed AGI/main RNG.
All twelve base enemies qualify with AGIs18,15,16,13,14,16,15,16,13,14,13,18;
none has a second-turn bit. Actual allies/substitutions can change that.
Ordinary score is AGI + RNG(AGI>>3) − RNG(AGI>>3) + RNG(3) −1; AGI bit128
adds the distinct two-draw extra score. Range0 still advances the seed.62
signed-byte stable-sort passes preserve append-order ties; cursor becomes0.
Upgrade draws precede this; standby uses separate thinking RNG. No fixed first
actor, exact queue or guaranteed initial player-ready state follows.

Sentinel requests a round; HP0 actor skips. `ExecuteIndividualTurn` gives
MUDDLE/AI bit2 priority to AI preparation. Otherwise enemy **opponent-control=0**
selects AI, nonzero player preparation; ally auto-battle independently selects
AI. Shared cursor/view/windows/grid work follows; SLEEP/STUN select NoAction
before execution. Dispatch repeats the MUDDLE/bit2/control polarity before AI
execution or player input. Stop at this conditional boundary, before a performed
action. F88 may instead reach a retained cursor in a saved queue.

## Accepted production consumers and conversion limits

**Confirmed by accepted implementation inspection, separate from original-game
evidence:** Battle10 is not admitted/converted/supported by the current private
route. [Trust](../../remake/docs/content/profiles-and-trust.md#selected-third-encounter)
and [Application](../../remake/docs/application/session-and-programs.md#yeel-guide-zone-request-and-sleep-continuation)
retain selected2/3 consumption. No allowlist edit, preparation, comparison or
engine execution is performed. Existing mechanisms are not reported missing;
their presence does not establish an admitted whole encounter.

| Actual consumer | Reusable mechanism and explicit Battle10 boundary |
| --- | --- |
| [prepare_encounter / compile_routes](../../src/sf2tool/remake_exploration_battles.py) | Producer/CLI admit2/3 only with selected source/ROM checks. Hook/load and400/450/500 flags are reusable; no10 export. |
| [PrivateBattleEncounterReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs) / [PrivateBattleDefinitionReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleDefinitionReader.cs) | Placement/point/region/metadata parsing exists; trusted encounter pins admit1/2/3, not10. Definition selection loads declared bases and referenced items/use spells, so descriptive Ring/ATTACK fields can be represented; no F510/level/category/RNG substitution is implemented. |
| [PrivateBattleScenarioReader.Assemble](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs) / [PrivateBattleActionBindings](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleActionBindings.cs) | Independent region-program admission1/2/3 rejects10. Sets0/6/7/9, point/follow/NONE and Flying/Hovering exist. Placement items admit only NOTHING or the specific Herb expression; Ring declaration rejects before its initialization. Physical binding permits at most one equipped WEAPON with adjacent range and limited prowess: a ring's effects are not implemented by that binding, Hunter2–2/prowess and Bat poison reject. Hunter/Witch BIT4 fail AI-word regex; STEALTH_ARCHER/MAGE movers reject. Executable spells bind HEAL/healing Herb, not FREEZE/BLAZE/ATTACK or general breakable items. Controlled classes0/1/2/4 omit Peter/arbitrary natural parties. |
| [BattleEntry](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs) | Hook/load/start flags, actual membership rebuilding and carried resources/growth/gold/seeds exist; missing member definitions reject. F88 rejects instead of restoring queue/cursor. Completed admission is no source510 upgrade selection. |
| [BattleInitializationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleInitializationRules.cs) / [BattleTurnFlow](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleTurnFlow.cs) | Controlled already-refreshed allies, roster-only/difficulty0, living/immortal healing, formation and ATT/activation exist. Excess active count rejects rather than evicting the source tail. General equipment/CURSE refresh, other difficulty/control policies and suspension remain Unsupported. Enemy loadouts admit0/127, rejecting weapons and even unequipped19. |
| [BattleActivationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActivationRules.cs) / [TurnOrderRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/TurnOrderRules.cs) | Mode0/RegionProgram.None, living placed ally tests, primary-first/sticky-secondary and randomized/extra turns exist. Cross-product geometry/fresh per-round tested word are not proof of original integer projection/cache lifetime equivalence; no Battle10 observation exists. |
| [BattleControlRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleControlRules.cs) / [SourceEnemyAi](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceEnemyAi.cs) / [SourceMoveOrderAi](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceMoveOrderAi.cs) | Classifiers expose unsupported automatic-ally/status/control policies; enemy AI rejects ally actors, so Peter's original AI is not production support. Standby/NONE/15/15, sets6/7/9, point/live-follow and dead-target promotion exist. Activated AI permits only0/127 and no learned spells, rejecting Ring/gear/Bat/Witch loadouts. Physical station search does not implement general ranged/attack/support-item AI. |
| [BattleActorState](../../remake/src/Sf2.Remake.Domain/Battles/State/EngineBattleState.cs) / [BattleOutcome](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleOutcome.cs) and action binding above | SourceLoadout preserves selected words and generic healing-item use exists; this is not arbitrary Ring inventory/equipment, rare deals or source action-drop custody. No row10/targetDies/item19/once-bit4/deals consumer exists in the inspected production route. Generic terminal rewards/return do not award this Ring. |

These are requirements/Unsupported boundaries at actual consumers, not an engine
defect acceptance or new implementation task. Remake code proves no original fact.

## Unknowns and reproduction boundary

**Unknown:** natural request/flags, joined/active/dead/capacity and follower history,
May/Peter's actual deployment/AI, classes/levels/gear/status, difficulty/seeds,
substitution results (especially airborne overread), later region crossings,
first actor/action/target/tactics, Ring use/drop/recipient/inventory/deals state,
continuous continuation, presentation/hardware timing and save durability.
No Unknown is silently replaced by a safe algorithm or an automatic acquisition task.

Accepted Battle01 admission/victory/stable-return/later-Down observations remain.
The #1047 twelve **completed unresolved comparisons** stay at the
[verification owner](../../remake/docs/verification/field-programs.md#yeel-guide-through-battle03),
not interrupted runs or an automatic repair queue. Preserve the intermediate
status/equipment-CURSE correction. PR1221's mechanical PASS missed stale coverage
summary/denominator and opponent-control polarity; its **two P2** findings remain
in the [handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1220#issuecomment-6354981415).
Mechanical checks cannot replace semantic review.

Reproduce by reading all24 combatant rows, three ordered regions/one point, all
five definitions and the named complete consumers at the pin. Follow510 through
level/category/retry/RNG to definition copy, then Ring AddItem's actual d1 return
to equipped refresh and AI filters. Separately trace action actor/target/targetDies,
actual item lookup, one-time bit, removal, recipient capacity and RARE/deals.
Substitute all12 fresh positions into both triangles per region with source
integer division/CCR/edge order; inspect production admission and rejection bodies.

Work is linear selected reads plus36 region-position/72 triangle substitutions,
one process, small memory/output and no bulk run. Existing two map payloads total
18,285,979 bytes and `local/issue1005/pair.txt` stay unchanged. New small methods/
fresh receipts are confined to `local/issue1230/` without overwriting prior results.
No artificial prose line ceiling is used. Replan for concrete required acquisition
or cross-system expansion, not merely another Unknown.

Direct checks cover strict UTF-8/LF, relative links/anchors separately from
external references, explicit three-path `origin/main` diff including committed
head, private/ownership, fixed counters/frontiers and retained metadata. On clean
committed head load the ignored selections in the same process:

```powershell
. ./local/private-inputs.ps1
uv run sf2 verify plan --base origin/main --head HEAD
```

Record exact-head scoped CI and completed failures in the Issue handoff. No
normal/full, SDK/Godot/emulator/H3/H4, screenshot, comparison/lowering or new
extraction is selected. Stop at clean pushed frozen Draft PR, Review / Verify,
stopped writer/processes and successful notification to the assigned main-gate.
Independent acceptance/merge and any cleanup remain with main-gate.
