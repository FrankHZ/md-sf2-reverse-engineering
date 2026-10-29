# Battle18 initialization, Taros equipment and first conditional control

Evidence date: 2026-10-23. **Confirmed:** the complete12/14/5/0 selection joins
actual party placement, seven full enemy definitions, five initially untriggered
regions and selected command streams to first conditional control. Taros has two
round entries and an equipped item-use source for BOLT2. His LEADER terrain
prepass consumes the absent point0 through the following spriteset's header,
before the MOV0 command guard. The initial ineffective-attack gate requires
**Bowie0's equipped Achilles Sword**, not merely an ally or possession. Caravan's
HP0 combatant159 and its physical sprite do not establish a working lookup159.
**Inferred:** these are the intended shrine guardian encounter's selected rules.
**Unknown:** natural arrival/state, actual first actor, performed actions, damage,
loot, service completion, presentation and persistence. This is static research
for content conversion, not an observed or production-admitted encounter.

## Agreement and provenance

This Standard-rigor agreement implements
[Issue1242](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1242)
under [Epic1053's finite scope](../../story-content-coverage.md#scope-and-reading-rules).
Party, AI, equipment, action and entity consumers cross owner boundaries; two
existing prose claims require reconciliation against actual instructions. The
outcome is a maintainable account through first conditional control and the
narrow Taros attack/drop rules. It does not cover every tactic or a victory route.

Exactly five tracked paths are owned: this document, Battle18 consistency in
`story-content-coverage.md` and `source-coverage.md`, the narrow Taros correction
in `battle-actions.md`, and refresh/neutral corrections plus this primary link in
`southwest-parmecia-through-taros-battle18-entry.md`. No competing writer or
stacked dependency was found. PR692 is unconsumed. Source, code, fixtures,
schemas, indexes, counters, design and runtime frontiers remain read-only.
No environment, acquisition, comparator, parser platform or engine change is
introduced. Acceptance is complete selected source/consumer review, finite
arithmetic, direct UTF-8/LF/link/anchor/private/exact-path checks, unchanged
retained metadata/frontiers/counters, clean committed planner and exact-head
scoped CI; stop at a clean pushed frozen Draft PR for independent main-gate.

Accepted repository base is `ef71bc91c0bd38b87e8bc3b122da9f1732fd41c0`,
tree `e66bfc6e893249e08b887753ed5fec4d6a77dcb4`. Original paths below resolve
under `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at that pin; the
[USA baseline](../../../../README.md#pinned-original-baseline) owns ROM identity.
No new ROM equivalence or runtime observation is claimed. Short battle-code
paths below use `code/gameflow/battle/`; sibling names retain their preceding
directory. Resolve `j_` aliases to their implementation bodies. Opcodes and
branch polarity control over comments and suggestive labels.

Reuse the complete [entry/warning/before scene](./southwest-parmecia-through-taros-battle18-entry.md)
and [outcomes/Caravan acquisition](./battle18-outcomes-and-caravan-acquisition.md)
without retelling them. [Ribble's sword acquisition](./ribble-tree-opening-and-achilles-sword.md)
and [Rohde's choice](./hassan-ancestry-and-rohde-caravan-choice.md) own possession
and unlock producers. [Battle13](../eastern-parmecia/battle13-initialization-and-first-control.md)
owns the accepted actual-party/neutral offset distinction;
[Battle16](./battle16-initialization-and-first-control.md) owns the special-sprite
and extra-turn comparison; [Battle17](./battle17-initialization-and-first-control.md)
owns the six reused definitions and healing/follow mechanisms;
[Battle10](../eastern-parmecia/battle10-initialization-and-first-control.md) owns common item/drop
custody. Their separate natural-route and runtime limits remain.

| Claim | Declaration and actual consumer |
| --- | --- |
| Selection/entry | `code/common/maps/getbattle.asm:CheckBattle`, `0x799C`; `battleloop_1.asm:BattleLoop`, `0x23A84`; `data/battles/global/{battlemapcoords,custombackgrounds,enemyleaderpresence,halvedexpearnedbattles,afterbattlepositions,battleneutralentities}.asm`; before/start table slots18 and wrappers. |
| Formation/definitions/refresh | `data/battles/spritesets/entries.asm` and `spriteset18.asm`, `0x1B4546..0x1B46BE`; `sf2macros.asm`, `sf2enums.asm`; `getbattlespritesetsubsection.asm`, `0x1B1630..0x1B169E`; complete `battleloop/initializecombatants.asm`, `0x1B1272..0x1B15F8`; `heallivingandimmortalallies.asm`; `code/common/stats/{battleparty,updatecombatantstats}.asm`; seven selected records in `data/stats/enemies/enemydefs.asm`; `battleloop/upgradeenemies.asm` and `data/battles/global/randombattles.asm`. |
| Sprite/neutral identity | `battlefunctions/loadBattle.asm`; `code/common/scripting/entity/entityfunctions_1.asm:PositionBattleEntities/DeclareNewEntity`; `battlefunctions/battlefunctions_0.asm:GetEntityIndexForCombatant`; `data/stats/enemies/{enemymapsprites,enemybattlesprites}.asm`; neutral table and `data/scripting/entity/eas_battleneutralentities.asm`. |
| Activation/control | Complete `battleloop/{activateenemies,triggerregions,clearaimemory,populateenemyspawns,turnorderfunctions}.asm`; region code/data tables; `ai/startaicontrol.asm`, `0xDEFC..0xE1AC`, both `determineaistandbymovement_*.asm`; `code/common/stats/combatantstats_1.asm:GetMoveOrders/GetTriggerRegions`, `_2.asm:SetTriggerRegions`; `battlefunctions/executeindividualturn.asm`, `0x23EB0..0x24242`. |
| Orders and item/spell decisions | `data/battles/global/aicommandsets.asm`, `0xE1AC..0xE25B`; `ai/executeaicommand.asm`; `ai/movementhelperfunctions.asm:BlockAndCarveAroundDestination/BlockAndCarveForTetheredTarget`; `battlefield/getmoveorderposition.asm`; following `spriteset19.asm` header; `ai/command/{moveorder,attack,heal,support,move}.asm`, their selected path/target/healing helpers, `getnextuseableaiaction.asm`, `gethighestusablespelllevel.asm`; selected `data/stats/{items/itemdefs,spells/spelldefs}.asm`. |
| Achilles gate | `battleactions/battleactionsengine_1.asm:WriteBattlesceneScript`, `0x9B92..0x9EC4`; `determineineffectiveattack.asm`, `0x9EC4..0x9F28`; `inflictdamage.asm`, `0xACEA..0xAE32`; `battleactionsengine_2.asm`, `attack.asm`, `initbattlesceneproperties.asm`, `isabletocounterattack.asm`; `code/common/stats/{combatantstats_1,itemstats}.asm:GetEnemy/GetEquippedWeapon`. |
| Taros Sword/drop | Item110 and spell BOLT2; `ai/command/attack/{prioritizetargets,determinebattleaction}.asm`; `battleactions/{useitem,createbattlesceneanimation,breakuseditem,dropenemyitem}.asm`; `data/battles/global/enemyitemdrops.asm` row18/enemy0/bit24; item inventory/deals helpers and `code/common/tech/randomnumbergenerator.asm`. |

Existing [spriteset](../../battle/battle-spriteset-data.md), [routing](../../battle/battle-routing-data.md),
[global](../../battle/battle-global-data.md), [loop](../../battle/battle-loop.md), [AI](../../battle/battle-ai.md),
[action](../../battle/battle-actions.md) and [cutscene](../../battle/battle-cutscenes.md) fixtures retain
their boundaries. `rowContentTracked:false` and `routeTargetsParsed:false`
remain unchanged. This account adds no executable Battle18 fixture or counter.

## Map12 selection and fresh versus suspended entry

**Confirmed:** row18 is Map12, crop origin`(0,0)`, size26×32, wildcard triggers
255/255. CheckBattle admits supplied F418 before shrine exploration init; crop
dimensions are outputs, not a containment test. F518 clears418 on a qualifying
scan but still returns18 on that call. F468 separately gates before/start hooks;
F88 selects suspension. Neither sword possession, F725 nor party composition is
an admission predicate in this row. The entry and Rohde owners retain producers.

Terrain18 loads into shared48×48 storage. Background18 is TAROS_SHRINE15,
not OVERWORLD255's map-block selection. Leader-presence18 is-1; Taros128's
death therefore reaches the common leader sweep independently of commandset14.
Half-EXP selects only1 and post-battlescene position override only5. Their tables
give no special18 entry. Defeat/victory/Egress and Caravan flags remain with the
outcome owner; an action drop is not the later victory script.

| Supplied state | Ordered work |
| --- | --- |
| F88 clear/F468 clear | Set PLAYER_TYPE0/F399; clear seconds; save/install Map12/Battle18/base VInts; execute `bbcs_18`; restore/reassert map/battle; clear90–105; heal/refresh eligible allies; place actual party; initialize enemies; clear AI memory; `LoadBattle(0)`; start wrapper sets468 before `ms_Empty`; activation/region/spawn/round. |
| F88 clear/F468 set | Skip both hook bodies but still perform fresh clearing, healing/refresh, placement, enemy initialization/load and new round. F518 remains independent. |
| F88 set | After mode0/F399, restore saved seconds, clear88/AI memory/view target, LoadBattle with incoming focus and enter retained queue/cursor. No fresh field installation,90–105 reset, healing, placement, before/start or new round precedes individual turns; a sentinel later requests a round. |

The staged scene main`(12,32)` and Taros/Caravan rows are replaced by battle
population. First ally template`(12,27)` is neither scene main nor shrine request
`(12,30)`. Scene Taros128 and Caravan129 do not preserve their entity state through
LoadBattle; battle129 is Master Mage. The before scene does not join Peter/Rohde,
equip Achilles, acquire Caravan or prove a functioning vehicle. Its speech and
follower lookups are distinct from combat membership. Saved map/battle/area,
roster and queue must already agree; F88 does not repair them or supply fresh d0=0.
Map, fade, text, motion and cursor service completion remain Unknown.

## Twelve templates and the actual party

**Confirmed:** 4-byte header +26×12 combatant bytes +5×12 region bytes +0×2
point bytes =376 (`0x178`), ending exclusively at`0x1B46BE`. Every ally row uses
HEALER1/0, NOTHING127, NONE255/15 and NONE255/15, filler0 and STARTING0.
Only its coordinates are copied into actual living party members:

| Template | X,Y | Template | X,Y |
| --- | --- | --- | --- |
| 0 | `(12,27)` | 6 | `(11,29)` |
| 1 | `(11,28)` | 7 | `(12,29)` |
| 2 | `(13,28)` | 8 | `(13,29)` |
| 3 | `(11,27)` | 9 | `(12,30)` |
| 4 | `(13,27)` | 10 | `(13,30)` |
| 5 | `(12,28)` | 11 | `(11,30)` |

Initialization resets all30 ally Xs to-1, rebuilds ascending joined/active lists
from flags0–29/32–61, and traverses that actual list. An ordinary dead member
uses a list/loop iteration without advancing the formation row. Twelve iterations
bound capacity even when deaths leave coordinates unused. Later list members
enter LeaveBattleParty: active/X cleared, joined retained, with no subsequent
list rebuild here. The zero-template guard is not an empty-party pre-loop guard.
Arbitrary empty/inconsistent supplied state is not normalized into safe absence.

JoinForce sets joined and rebuilds lists, then calls JoinBattleParty only below12
active members. At capacity a new inactive member remains reserve; an already-set
active bit is not erased. Direct JoinBattleParty lacks that capacity guard.
Membership does not itself revive, place, queue or choose control for an ally.

Healing scans all30 independently of membership: living allies and immortal
Peter7/Lemon28 receive max HP/MP. Other dead allies are skipped. Its intermediate
`status & 7` keeps STUN/POISON/CURSE; UpdateCombatantStats then preserves
STUN/POISON and derives **final CURSE from equipped cursed items**. Old CURSE
alone does not survive refresh. Current class/gear/status affects current stats.
Healing/refresh happens after the before scene, not before its actor operations.

Peter's AI bit2/actual membership, Luke's independent recruitment and Rohde's
following/unlock state retain their prior owners. Peter may revive while remaining
reserve/follower-only. Rohde11 following does not set joined11/active43. Template
AI/gear does not overwrite actual allies; an AI-controlled ally uses set6, not
template HEALER1. ClearAiMemory resets48 last-target bytes to-1 and48 memory
bytes to0, not combatant AI bit2 or the tested-region word.

## Combatants, special Taros sprite and neutral Caravan

**Confirmed:** fresh rows128–141 are placed immediately; undeclared142–159
get max/currentHP0 and X=-1. All fourteen rows add NOTHING127, have secondary
order NONE255, filler0 and STARTING0. Their complete varying operands are:

| Row / slot | Base / X,Y | Commandset | Primary order | Primary/secondary region |
| --- | --- | --- | --- | --- |
| 0 /128 | TAROS88 `(12,8)` | LEADER14 | MOVE_TO\|0 (`$40`) | 15/15 |
| 1 /129 | MASTER_MAGE69 `(11,9)` | ATTACKER3/9 | FOLLOW_ENEMY\|0 (`$80`) | 15/15 |
| 2 /130 | BLACK_MONK78 `(13,9)` | HEALER3/2 | FOLLOW_ENEMY\|0 (`$80`) | 15/15 |
| 3 /131 | GARGOYLE43 `(14,10)` | ATTACKER2/7 | FOLLOW_TARGET\|0 (`$00`) | 15/15 |
| 4 /132 | ARROW_LAUNCHER60 `(7,11)` | ATTACKER1/6 | NONE | 3/4 |
| 5 /133 | GOLEM9 `(3,10)` | ATTACKER1/6 | NONE | 3/4 |
| 6 /134 | SOULSOWER11 `(15,8)` | ATTACKER1/6 | NONE | 0/15 |
| 7 /135 | GOLEM9 `(18,9)` | ATTACKER1/6 | NONE | 0/15 |
| 8 /136 | GOLEM9 `(20,17)` | ATTACKER3/9 | FOLLOW_ENEMY\|9 (`$89`) | 1/2 |
| 9 /137 | MASTER_MAGE69 `(21,15)` | ATTACKER1/6 | NONE | 1/2 |
| 10 /138 | ARROW_LAUNCHER60 `(16,19)` | ATTACKER1/6 | NONE | 15/15 |
| 11 /139 | GARGOYLE43 `(19,21)` | ATTACKER1/6 | NONE | 15/15 |
| 12 /140 | SOULSOWER11 `(5,24)` | ATTACKER1/6 | NONE | 15/15 |
| 13 /141 | GARGOYLE43 `(4,16)` | ATTACKER1/6 | NONE | 3/4 |

GetCombatantMapsprite selects Taros88→special255. PositionBattleEntities uses
physical47, ENTNUM32, and writes47 at enemy128's ordinary lookup offset32.
It restores the ordinary physical cursor without incrementing it; the thirteen
other enemies consume ordinary slots after actual placed allies. The battle-sprite
selection is separate. Rebuilding this special actor does not change combatant
identity88/128, preserve scene state or prove rendering.

After ordinary placement, F518 clear admits the neutral table's Caravan62 at
`(12,5)`, Left, `eas_Motionless`. Combatant159 gets max/currentHP0, status0,
activation OR8 and these coordinates. F518 set skips the entire neutral tail.
This is independent of F468 and the scene's Down-facing, animation-stopped
Caravan. The neutral action turns motion off then branches idle.

The actual neutral mapping matches the accepted Battle13 defect: a1 resets to
`ENTITY_INDEX_LIST_ENEMIES=$FFB160`; `move.b d0,-(a1)` writes the new physical
index at`$FFB15F`, lookup31 relative to`ENTITY_INDEX_LIST=$FFB140`.
DeclareNewEntity stores physical d0/d6 as ENTNUM and does not fix lookup63.
GetEntityIndexForCombatant(159) subtracts`$60`, reading offset63/`$FFB17F`;
in this fresh path that byte remains`$FF` from skipped undeclared enemy159.
Thus HP0 combatant159, lookup31, physical Caravan and vehicle controller are four
different facts. No living enemy, working159→sprite lookup, joined member or
vehicle acquisition follows. HP0 prevents activation and remaining-enemy count;
the ordinary enemy turn scan stops at157 in any case. LoadBattle also runs this
F518-gated rebuild on suspension, but without fresh enemy clearing; set518 is
not proof all supplied saved neutral fields were erased.

## Seven complete definitions and equipment refresh

**Confirmed:** all seven use spellPower REGULAR and initialStatus NONE. The raw
unknownByte equals the definition ID for9/11/43/60/69/78, but is0 for Taros88.
It is not an independently interpreted commandset. “Rest NOTHING” completes all
four item/spell slots; NONE below means all four NOTHING entries.

| Definition | Level; HP/MP; base ATT/DEF/AGI/MOV | Items / spells | Movement; original AI word |
| --- | --- | --- | --- |
| Taros88 | 20;49/0;9/32/151/0 | Taros Sword110 equipped, rest NOTHING / NONE | REGULAR; PRIORITYMOD_1 (`$1000`) |
| Master Mage69 | 14;28/14;11/18/22/5 | Iron Rod88 equipped, rest NOTHING / BLAZE2, rest NOTHING | MAGE; PRIORITYMOD_2\|BIT4 (`$2010`) |
| Black Monk78 | 14;30/19;27/18/21/6 | NONE / HEAL2, BLAST1, NOTHING, NOTHING | HEALER; PRIORITYMOD_3 (`$3000`) |
| Gargoyle43 | 15;29/0;31/23/21/6 | NONE / NONE | FLYING; PRIORITYMOD_2\|BIT4\|BIT5 (`$2030`) |
| Arrow Launcher60 | 17;33/0;15/24/23/4 | Steel Arrow47 equipped, rest NOTHING / NONE | BRASS_GUNNER; PRIORITYMOD_2\|BIT4 (`$2010`) |
| Golem9 | 12;24/0;28/24/19/4 | NONE / NONE | REGULAR; PRIORITYMOD_2\|BIT5 (`$2020`) |
| Soulsower11 | 16;27/0;35/32/25/4 | NONE / NONE | REGULAR; PRIORITYMOD_2\|BIT5 (`$2020`) |

| Definition | Complete resistance expression | Complete prowess expression |
| --- | --- | --- |
| Taros | WIND_MAJOR\|LIGHTNING_MAJOR\|ICE_MAJOR\|FIRE_MAJOR\|STATUS_IMMUNITY | CRITICAL150_1IN4\|DOUBLE_1IN16\|COUNTER_1IN8 |
| Master Mage | FIRE_MINOR | CRITICAL150_1IN32\|DOUBLE_1IN32\|COUNTER_1IN32 |
| Black Monk | 0 | CRITICAL150_1IN4\|DOUBLE_1IN16\|COUNTER_1IN16 |
| Gargoyle | 0 | CRITICAL150_1IN32\|DOUBLE_1IN32\|COUNTER_1IN32 |
| Arrow Launcher | LIGHTNING_WEAKNESS\|STATUS_MINOR | CRITICAL150_1IN32\|DOUBLE_1IN32\|COUNTER_1IN32 |
| Golem | LIGHTNING_MINOR\|ICE_MAJOR\|FIRE_WEAKNESS\|STATUS_IMMUNITY | CRITICAL150_1IN8\|DOUBLE_1IN32\|COUNTER_1IN32 |
| Soulsower | LIGHTNING_WEAKNESS | CRITICAL_STUN\|DOUBLE_1IN32\|COUNTER_1IN32 |

Battle18 is absent from the complete random-battle admission list. Supplied518
does not enable upgrades, effective-level selection or the airborne defect from
Battle17. InitializeEnemyStats copies the selected56-byte base, restores HP/MP,
applies row position/identity/regions/orders, combines movement with commandset,
and handles row item. NOTHING returns before AddItem, retaining definition gear.
Activation keeps the priority upper nibble; filler0/mode0 replaces raw BIT4/5.
Those definition bits are not fresh activation or AI-control bits here.

The difficulty body's second BEQ makes every non-SUPER apply floor(baseATT×5/4)
once, and SUPER apply it twice, before equipment. With difficulty0 and fresh
status, current ATT is43 Taros,25 Mage,33 Monk,38 Gargoyle,35 Launcher,35 Golem,
43 Soulsower. Taros's calculation is floor(9×5/4)+32=43, not base9 or damage43.
His DEF32, AGI151 and MOV0 remain; no selected equipment alters those fields.
No performed critical, double, counter, stun or resisted spell follows from a
prowess/resistance declaration.

| Item | Full relevant definition | Selected consequence |
| --- | --- | --- |
| Steel Arrow47 | ACHR/RNGR/SNIP/BRGN/BWNT; range2–2; price1270; WEAPON; use NOTHING63; ATT+17 and two NONE0 effects | Equipped Launcher range2–2 and ATT refresh. |
| Iron Rod88 | MAGE/PRST/WIZ/SORC/VICR; range1–1; price560; WEAPON; use NOTHING63; ATT+12 and two NONE0 effects | Equipped Mage range1–1 and ATT refresh. |
| Achilles Sword61 | SDMN/HERO; range1–1; price1350; WEAPON\|RARE; use NOTHING63; ATT+19 and two NONE0 effects | The initial Taros gate tests Bowie's actual equipped weapon identity61. |
| Taros Sword110 | equipFlags NONE; range1–2; price10000; WEAPON\|RARE\|BREAKABLE; use BOLT2; ATT+32 and two NONE0 effects | Definition-copy equipment is already equipped despite no ordinary equip classes; physical range1–2 and item attack below. |

The ordinary GetAttackRange default for no equipped weapon is1–1. The AI attack
station builder separately uses0–1 for ordinary unarmed actors; neither creates
a special ranged rule for Golem, Monk, Gargoyle or Soulsower. Item usability's
incorrect255 no-use comparison remains: Rod/Arrow's63 falls back through the spell
getter to HEAL and fails offensive TYPE_ATTACK filtering. They lack AI-use bit13
and are not Healing Rain; equipped weapons do not thereby become healing items.

## Five regions, arithmetic and activation lifetime

**Confirmed:** all records have byte1=0 and trailing0/0. Types3 keep a fourth
`(0,0)` pair that their triangle consumer does not read.

| Region / flag | Type | Ordered vertices1,2,3,4 | Assigned rows |
| --- | --- | --- | --- |
| 0 /90 | 4 | `(0,0),(0,16),(24,16),(24,0)` | Primary134/135 |
| 1 /91 | 3 | `(10,18),(23,31),(23,18),(0,0)` | Primary136/137 |
| 2 /92 | 4 | `(0,4),(11,17),(24,17),(24,0)` | Secondary136/137 |
| 3 /93 | 3 | `(1,18),(1,31),(14,18),(0,0)` | Primary132/133/141 |
| 4 /94 | 4 | `(0,4),(0,17),(12,17),(24,4)` | Secondary132/133/141 |

IsRegionTriggered checks all30 allies with positive signed HP and nonnegative
signed-byte X/Y; joined/active/following are not additional predicates. Type4
tries triangles1/2/4 then3/2/4; type3 tries1/2/3. Within a triangle the edge order
is1→2,3→1,3→2. Each segment chooses horizontal projection on equal absolute
spans, otherwise the dominant axis, with signed MULS/DIVS truncated toward zero.
For horizontal dominance, `Yproj=Yb+trunc((Yb−Ya)×(Xc−Xb)/(Xb−Xa))`;
the vertical form exchanges X/Y. SUB.W supplies unsigned borrow for side flags.
Collinear and within dominant-axis bounds returns01 and immediately accepts;
out-of-bounds collinearity returns10. Ordinary side flags must equal the opposite
vertex's flag. Bit0 exceptions skip either early edge but reject at the third.
Negative projection explicitly writes CCR=1, returning10, despite the comment
calling it11. A cross-product drawing is not a substitute for these operations.

All twelve coordinates above were substituted into all eight selected triangles:
60 region-position cases /96 triangle substitutions, with no hits. Complete
first-rejection witnesses (edge numbers use the original region vertices) are:

| Region / triangle | First rejecting edge | Ally projection / flag; opposite projection / flag |
| --- | --- | --- |
| 0 /1,2,4 | Third,4→2 | Y8–9 versus27–30 /10; vertex1 Y16 versus0 /00 |
| 0 /3,2,4 | First,3→2 | Y16 versus27–30 /10; vertex4 Y16 versus0 /00 |
| 1 /1,2,3 | First,1→2 | Y19–21 versus27–30 /10; vertex3 Y31 versus18 /00 |
| 2 /1,2,4 | First,1→2 | X19–22 versus11–13 /00; vertex4 X−3 /10 |
| 2 /3,2,4 | First,3→2 | Y17 versus27–30 /10; vertex4 Y17 versus0 /00 |
| 3 /1,2,3 | Third,3→2 | Y19–21 versus27–30 /10; vertex1 Y31 versus18 /00 |
| 4 /1,2,4 | Third,4→2 | Y10–12 versus27–30 /10; vertex1 Y17 versus4 /00 |
| 4 /3,2,4 | First,3→2 | Y17 versus27–30 /10; vertex4 Y17 versus4 /00 |

For region2's first edge, the actual vertical formula is
`11+trunc(11×(Y−17)/13)`; vertex4 at Y0 gives`11−14=−3`, hence10.
This is the discriminating signed/CCR case. Living fresh placements consume a
prefix/subset of these templates, so ordinary deaths cannot create a fresh hit.
Later movement, invalid supplied positions and saved state are not covered by
this finite substitution.

ActivateEnemies scans128–159 then, lacking an outer RTS, falls through once for
160; the [existing alias account](../northern-travel-mitula/battle26-activation-and-first-turn-dispatch.md#the-extra-fall-through-and-its-selected-ram-alias)
retains that defect. TriggerRegionsAndActivateEnemies tests **all five regions
before inspecting the enemy's assignments**. NEWLY_TRIGGERED_BATTLE_REGIONS
is a tested-word cache, including misses, distinct from persistent90–105.
With clear incoming tested bits and fresh positions it becomes31 without setting
90–94. Fresh reset clears the persistent flags, not that word; incoming bits may
suppress tests. Normal enemy StartAiControl clears the word before standby/orders.

Primary hit sets activation bit0 then returns, preserving existing bit1. Only
secondary hit sets both bits0/1. Thus simultaneous3/4 or1/2 chooses primary on
fresh state; primary-only, secondary-only, both and neither are distinct inputs.
Activation does not swap the stored orders. Region0/15 differs from those pairs;
15/15 bypasses standby without needing a region hit. Sparse region-program
tables contain32/40, not18. All selected/undeclared fresh spawn modes are0,
so no hidden/respawn wave is selected here.

Seven assigned actors132–137/141 are initially inactive. NONE-order132/133/
134/135/137/141 may take the existing random local standby path around their
starting positions: thinking-RNG early stays,3/4 candidate memory, previous
candidate exclusion, occupancy/terrain and path generation still decide it.
136's live137 order with assigned primary1 makes ValidateAiStandbyEligibility
return nonzero d1, which its caller treats as **stop**, despite its comment:
STAY/empty move string. The remaining128–131/138–140 have15/15 and enter their
streams. Inactive does not mean absent from the turn queue or guaranteed immobile
for the six NONE-order actors.

## Live orders, complete streams and Taros's zero-point read

**Confirmed:** FOLLOW_ENEMY0 means current combatant128, and FOLLOW_ENEMY9
means current137, not static formation coordinates. FOLLOW_TARGET0 means live
ally0/Bowie, not the first active member. StartAiControl, on the activated path,
promotes a dead followed target's secondary order into primary and sets secondary
NONE; here every secondary is NONE. This mutation is separate from region
secondary activation. MOVE_TO skips the followed-target HP test.

| Selected set | Complete ordered stream | Initial pathfinding mode |
| --- | --- | --- |
| HEALER3/2 | HEAL2, MOVE_ORDER4, SUPPORT, ATTACK3, MOVE2, STAY | block-non-movable1 |
| ATTACKER1/6 | ATTACK1, HEAL1, SUPPORT, MOVE1, STAY | regular0 |
| ATTACKER2/7 | MOVE_ORDER1, ATTACK1, HEAL1, SUPPORT, MOVE1, STAY | regular0 |
| ATTACKER3/9 | MOVE_ORDER3, ATTACK3, HEAL2, SUPPORT, MOVE_ORDER4, MOVE2, STAY | block-non-movable1 |
| LEADER14 → shared stream13 | MOVE_ORDER5, ATTACK2, HEAL3, SUPPORT, MOVE_ORDER2, MOVE3, STAY | block-and-carve2 |

The loop stops at the first command returning d1=0; declared suffixes are not
performed actions. MOVE_ORDER1/4/5 pass targetType0 with modes0/1/2;
MOVE_ORDER3 passes FOLLOW1/mode1 and MOVE_ORDER2 FOLLOW2/mode2. MOV0 returns
STAY/failure before reading orders **inside aiCommand_MoveOrder**, but mode
post-processing still runs. Ordinary MOVE commands score live opponents/path
costs; they do not guarantee the stored destination. No selected source stream
needs a new commandset solely because its enemy has a new name.

Taros's zero-point question has a concrete consumer. Normal enemy StartAiControl
bypasses standby for15/15, skips dead-follow promotion for MOVE_TO0, then applies
LEADER14's mode2 **before reading the command stream**. ClearAiMemory supplied
lastTarget=-1, so BlockAndCarveAroundDestination reads primary`$40` and calls
GetMoveOrderPosition without a MOV guard. The latter requests subsection4:
GetBattleSpritesetSubsection returns count0 and pointer`0x1B46BE`, after the five
regions. GetMoveOrderPosition ignores count, masks point index0 and reads bytes
at that pointer and+1. `entries.asm` includes spriteset19 immediately after18;
its header starts12,15. The consumed coordinates are therefore`(12,15)` from the
next spriteset's ally/enemy counts, **not a declared Battle18 AI point**.

The prepass marks non-obstructed terrain with temporary bits6/7 and carves
radius0/1/2 around that derived center. MOVE_ORDER5 later hits MOV0, but its
mode2 post-processing can read the same bytes again. If earlier commands fail,
MOVE_ORDER2 and MOVE3 also use mode2. The last-target variant still fetches the
center from the move order (rather than the last-target position), with radii0–4.
StartAiControl clears temporary obstruction flags on exit. This proves the
selected out-of-table **read and terrain restriction**, not movement to`(12,15)`,
a successful cast, a hang, or an observed target. A changed MOV/order/saved state
requires its own consumer analysis; silently inventing a valid point changes
original content semantics.

Master Mage BLAZE2 is TYPE_ATTACK, silence-affected, MP6/range1–2/radius1/power10;
Black Monk BLAST1 is MP2/range1–1/radius0/power6 with the same type/silence property.
GetNextUsableAttackSpell/GetHighestUsableSpellLevel filter learned type, confusion
and affordable level; BLAZE may fall back. HEAL2 declares MP5/range0–2/radius0/
power15. The Monk's actual healing helper returns HEAL1 for this learned level
while checking HEAL2's5MP due to shift5/unmasked-entry bugs; minimum3MP admission
does not make3/4MP sufficient. Candidates must be living friendly actors at
3×HP≤2×maxHP; missingHP≤2 fails. Nonself higher-level promotion needs known
level3+, absent here. Fresh full HP supplies no injured candidate. None of these
definitions learns SUPPORT; the command cannot create a spell. Soulsower's stun
is prowess, not a learned spell.

### Taros Sword use is separate from learned spells and loot

**Confirmed:** GetNextUsableAttackItem scans actual inventory. Taros's equipped
110 passes IsItemUsableInBattle and the equipped-or-AI-use check; no ordinary
equip-class check is made there. Its BOLT2 passes TYPE_ATTACK filtering, including
the allowed BOLT branch under confusion. Taros has no learned spell and0MP, but
this is an **item** route. BOLT2 declares MP15, range1–3, radius2 and power16;
those are spell operands, not a promise of16 final damage. Physical range1–2 is
independent. Item reachability, movement grid, target priorities and the LEADER
restriction above still apply.

PrioritizeTargetsForAttackAiCommand builds item, learned-spell and physical lists.
With item and physical candidates but no learned spell, DetermineBattleaction
draws main RNG range6: results3/5 select item, otherwise physical; when physical
has no candidate it selects the available item. No probability experiment is
made. It then chooses among actual highest-priority candidates. No candidates
returns STAY/failure. The item branch writes USE_ITEM, actual slot and target;
its AI_LAST_TARGET write mistakenly records the actor after d0 is overwritten.
This is not evidence that an item use happened at the initial positions.

UseItem unpacks BOLT2 and delegates to CastSpell. PerformAnimation charges MP
only for CAST_SPELL, and InitializeBattlesceneProperties applies the spell-silence
check on that branch, not USE_ITEM. BreakUsedItem recognizes weapon/breakable
but exits for an enemy user before break/destroy RNG. Thus this enemy's item use
does not consume MP or break the sword through that path. These distinctions do
not assert a general ally permission to equip/use Taros Sword or guaranteed loot.

## Bowie's equipped Achilles gate and its lifetime

**Confirmed:** WriteBattlesceneScript assigns a3 to CURRENT_BATTLEACTION,
a4 to BATTLESCENE_ACTOR (written from incoming combatant d0), and a5 to the
sorted TARGETS_LIST. ATTACK makes a one-target list from its action operand.
After initial properties it calls DetermineIneffectiveAttack once, before actor
animation/action. Therefore `tst.b (a4)` is a **zero identity test**, not an
ally-side sign test and not a test of physical sprite index.

Within Battle18 the helper clears F112, then sets it only when all four tests
hold: actor byte0/Bowie; action ATTACK; current target's GetEnemy result TAROS88;
GetEquippedWeapon(actor0) returns Achilles61. GetEnemy returns-1 for an ally and
the actual enemy-definition byte for an enemy; this target test is not a hardcoded
slot128 test. GetEquippedWeapon scans four live slots for equipped, non-NOTHING,
WEAPON and returns the masked item ID; it does not substitute possession or
revalidate class compatibility. The normal sword equip classes are SDMN/HERO.

| Initial script setup in Battle18 | Gate consequence |
| --- | --- |
| Bowie ATTACK, equipped Achilles61, target type88 | F112 set; freshly zeroed ineffective toggle stays0. Damage still depends on reaching damage, dodge and later rules. |
| Bowie merely carries61, or another ally attacks with61 | F112 clear; target type88 makes ineffective toggle-1. |
| Bowie CAST_SPELL/USE_ITEM, even while equipped61 | F112 clear; target type88 makes the toggle-1 at this setup seam. |
| Target is not enemy type88 | F112 may remain clear; helper does not set the toggle for that target. |
| Current battle is not18 | Helper skips its F112/gate work; it is not a global enemy-type immunity rule. |

InflictDamage checks target type88 and the local ineffective toggle before counter
halving, random variance, HP decrease, death flag and EXP/gold. If blocked it emits
the ineffective message and returns. It does not read F112 directly. This is
a damage-helper gate, not proof that every possible effect or script branch is
rejected. The single initial call is not a per-target reevaluation for every
multi-target spell; no broad multi-target immunity theorem is claimed here.

F112 is also cleared at ExecuteIndividualTurn's `@Start` for Bowie in Battle18,
including reentry from the battlefield menu. The helper clears/recomputes it on
each reached initial script construction in18. It is neither F418 admission nor
a saved possession entitlement. Ribble's chest adds the masked item without
auto-equipping it; Rohde's inventory search need not find it on Bowie. Dialogue,
F725 and chest success do not satisfy this equipped-actor gate.

Double attacks retain the original actor/target and toggle; there is no second
gate call. Counter setup reverses a4/a5 and explicitly clears the toggle, again
without calling this helper. Crucially ValidateCounterAttack reads original
actor through a5 and rejects enemy type88: ordinary Taros-origin attacks cannot
be countered. Its comment suggesting Taros cannot counter reverses the actual
pointers. Taros as the original target is not in the other prohibited-target
list (Burst Rock/Kraken Head/Prism Flower/Zeon Guard); its conditional counter
still requires flags, survival, status and its own range. DebugCounter overrides
cleared eligibility at the tail. Thus the counter reset is not an ordinary
non-Bowie loophole asserted here; debug and altered flows must not be generalized
into the initial Achilles rule. No all-action/counter trajectory is simulated.

## Taros Sword action-drop custody

**Confirmed:** the independent special-drop table selects battle18, enemy row0
(combatant128), item110 and once-bit24. It matches a combatant slot, unlike the
ineffective gate's enemy-definition test. Every ordinary target effect is followed
by DropEnemyItem; second/counter effect paths have their own calls. The helper
requires ally actor, enemy target and local targetDies, then matching battle/slot
and an actual held110 found by GetItemSlotContainingIndex. A table row alone
does not conjure a missing sword.

Taros Sword takes GenerateRandomOrDebugNumber with range32 and continues only
on zero. **This draw precedes the once-bit check**, so an already-set bit does
not bypass the draw after the earlier predicates. Nonzero returns without
setting bit/removing item. The unconditional branch after the zero test skips
the obsolete extra upgradable-battle draw; it is unreachable. Once-bit24 means
byte3/bit0 of ENEMY_ITEM_DROPPED_FLAGS, not story flag24. BSET returns on an
already-set bit. Otherwise it sets the bit, removes the target's actual slot,
then checks the acting ally's current HP and attempts AddItem.

A living actor with capacity receives masked110, without equipped/AI flags.
Actor HP0 or full inventory takes the rare-item fallback; item110 is RARE, so
AddItemToDeals handles it. Neither fallback preserves the sword in the defeated
target nor retries another ally. Setting once-bit, removing the source item,
inventory receipt and rare-deals storage are distinct operations. Enemy use's
break exemption does not guarantee a later drop; a kill, successful range32
draw, clear once-bit and actual item are still required. Leader sweep/victory
does not retroactively run this targetDies action helper for every removed enemy.
The outcome owner retains the independently reached Caravan/Rohde/return scene.

## Round generation and conditional control

**Confirmed:** after activation, absent region program and empty spawn selection,
GenerateBattleTurnOrder clears64 sentinels, scans30 allies then enemies128–157,
and admits nonnegative-X/nonzero-HP actors regardless of standby. All fourteen
fresh enemies qualify. AGIs by row are
`151,22,21,21,23,19,25,19,19,22,23,21,25,21`. Taros151=`128+23` supplies two
entries, giving fifteen enemy entries; HP0 neutral159 supplies none.

The ordinary score is maskedAGI + RNG(AGI>>3) − RNG(AGI>>3) + RNG(3) −1.
Taros's second uses floor(23×5/6)=19 plus/minus two draws with range19>>3=2.
Main RNG range0 still advances its seed. The62 signed-byte stable-sort passes
preserve append-order ties and set cursor0. Actual ally stats/gear and incoming
seed affect the queue; no18 repeat-upgrade draws precede it. Standby uses the
separate thinking RNG, while item choice above uses main RNG. Neither two Taros
entries nor any template establishes the first actor or a player-ready seam.

ExecuteIndividualTurn masks identity and skips HP0. MUDDLE or AI bit2 selects
AI preparation first. Otherwise enemy **opponent-control=0** selects AI, nonzero
player preparation; ally auto-battle independently selects AI. Cursor/view,
windows and grid preparation precede SLEEP/STUN NoAction. Dispatch then repeats
the MUDDLE/bit2/control polarity before AI execution versus player input, so AI
preparation may already have occurred before status prevents action. Stop at
this conditional control endpoint. F88 may instead enter with a retained queue,
cursor and changed positions/orders/resources; none is repaired by fresh prose.

## Accepted production consumers and conversion limits

**Confirmed by accepted implementation inspection, separate from original-game
evidence:**18 is not exported/admitted/played by the current private route.
[Trust](../../../../remake/docs/content/profiles-and-trust.md#selected-third-encounter)
and [Application](../../../../remake/docs/application/session-and-programs.md#yeel-guide-zone-request-and-sleep-continuation)
retain selected2/3 consumption. No consumer is changed or executed here.

| Actual consumer | Reusable support and selected18 boundary |
| --- | --- |
| [prepare_encounter / compile_routes](../../../../src/sf2tool/remake_exploration_battles.py), [scene producer](../../../../src/sf2tool/remake_battle_scene_content.py) | Encounter producer admits2/3, scene producer1/2/3. Generic wildcard/400–450–500 routing and before/load/start phases exist; no18 conversion/scene route is admitted. This does not request a new importer platform. |
| [PrivateBattleEncounterReader](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs), [PrivateBattleDefinitionReader](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleDefinitionReader.cs) | Formation/zero-point data, definitions/items/spells and metadata have generic representations, but trusted pins admit1/2/3. Region decoding requires four distinct bounded vertices and two nondegenerate triangles; type3 regions1/3 reject. New enemy names are not by themselves a missing family mechanism. Descriptive equipment/resistance/prowess does not implement their effects. |
| [PrivateBattleScenarioReader.Assemble](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs), [PrivateBattleActionBindings](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleActionBindings.cs) | Independent region-program admission rejects18. STARTING/NOTHING, sets0/6/7/9/14, REGULAR/HEALER/FLYING and live/point/NONE orders exist; HEALER3/2 rejects. Mage/Gargoyle/Launcher BIT4 fail AI-word binding; MAGE/BRASS_GUNNER movers reject. Taros/Monk/Golem/Soulsower prowess reject. Steel Arrow2–2 fails adjacent-range binding. Taros Sword's WEAPON\|RARE\|BREAKABLE and Achilles's WEAPON\|RARE fail the exact WEAPON expression; Taros1–2 is also outside range support. Rod's descriptive physical binding can fit. Only selected HEAL definitions bind executable spells, not BOLT/BLAZE/BLAST. Classes0/1/2/4 omit arbitrary natural party classes. |
| [BattleEntry](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs), [BattleInitializationRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleInitializationRules.cs) | Membership rebuild/carried resources/seeds, fresh formation/healing of living/immortal allies, difficulty0 ATT and activation exist. Excess active count rejects instead of source eviction; general gear/final-CURSE refresh remains limited. Enemy loadouts0/127 exclude all three equipped weapons. F88 rejects rather than restoring a queue; other difficulty/control policies remain unsupported. |
| [BattleActivationRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActivationRules.cs), [BattleTurnFlow](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleTurnFlow.cs), [TurnOrderRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/TurnOrderRules.cs) | Mode0/RegionProgram.None, primary-first dual regions,15/15, ordinary/extra randomized turns exist. Domain geometry also requires four vertices. Cross-product geometry and fresh round tested-state handling are not proof of signed-projection/cache equivalence. No18 observation validates that join. |
| [SourceEnemyAi](../../../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceEnemyAi.cs), [SourceMoveOrderAi](../../../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceMoveOrderAi.cs) | Standby,6/7/9/14 ordered streams, live follow/dead-target promotion and temporary restrictions exist. Activated AI rejects2, learned spells, equipped loadouts and automatic allies. LEADER is already represented; its generic point lookup explicitly rejects index≥point count (`source-ai-point`), so Taros's out-of-table read is not supported merely by accepting set14. Full selected item/spell/healing decisions remain limits. |
| [BattleControlRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleControlRules.cs), [physical action](../../../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2PhysicalAction.cs), [capability owner](../../../../remake/docs/capability-status.md) | Existing control/physical/EXP/gold/reward primitives do not bind the Bowie61/type88/F112/toggle gate, selected110 random/once-bit/drop/deals relationship or Taros-specific counter exclusion. Neutral lookup31/63, special sprite47, services, party producers and suspend require their own conversion/behavior decisions. No18 adapter/content route is established. |

These are conversion/support boundaries, not an assertion that every feature
needs new machinery. Generic definitions, formation, dual activation, extra turns,
Flying and LEADER can be reused where their actual semantics fit. Original18
needs no repeat-upgrade mechanism. Admission changes alone cannot resolve the
specific point, equipment, actor gate, spell/item or neutral discrepancies.

## Unknowns, retained failures and reproduction

**Unknown:** natural flags/arrival/party/capacity/gear/status/difficulty/seeds,
later region crossings and cache state, actual target/path/item/spell/heal choices,
first actor, damage/critical/counter outcomes, obtained sword/deals, saved-state
restoration, service completion, rendering/audio/hardware timing and persistence.
No runtime/implementation queue is created by these Unknowns.

Accepted Battle01 observations remain at their own boundary. #1047's twelve
**completed unresolved comparisons** remain at the
[verification owner](../../../../remake/docs/verification/field-programs.md#yeel-guide-through-battle03),
not interrupted or passed. Preserve
[PR1221's control/summary corrections](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1220#issuecomment-6354981415),
intermediate-status/final-CURSE results, the
[Battle12 HEAL correction](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1234),
[Battle13 neutral correction](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1236),
and Battle16/17's completed findings/failures. None is rerun or relabeled by this
document pass. Earlier truncated/guessed-path inspections are inspection errors,
not missing source, test failures or runtime observations.

Reproduce from the pin and accepted base by reading the complete selected
spriteset and seven definitions, then the named callers/consumers above. Join
scene replacement and fresh/saved branches before party/neutral/round claims.
Substitute the twelve positions into every region triangle using the stated
projection and edge order, including region2's negative projection. Trace
LEADER's prepass before its MOV0 command guard and verify the adjacent19 header.
Follow actual a3/a4/a5 through initial script, double, counter validation/reset
and InflictDamage before claiming the Achilles gate; follow targetDies/actual
inventory/RNG/once-bit/removal/capacity/rare-deals in that order for loot. Inspect
every named producer/trusted reader/Content/Domain/Application/AI boundary.

This is bounded source/consumer reading plus96 triangle substitutions, linear
in selected records and edges, one process and small receipts below64 MiB/one
minute. The retained18,285,979-byte pair and pair.txt are checked only by their
existing length/mtime metadata; neither payload is regenerated, copied or parsed.
Reassess before material resource or scope growth. Load
`. ./local/private-inputs.ps1` in the same command process. Direct document/scope
checks and `git diff --check` precede a clean committed
`uv run sf2 verify plan --base origin/main --head HEAD` and exact-head scoped CI.
Docs-only policy overrides generic suite recommendations: no normal/full, SDK,
Godot, emulator, H3/H4 or historical comparison rerun is selected. The Issue/PR
handoff records actual check results, inspection errors, exact Git objects and
the stopped clean writer; main-gate owns independent review and merge.
