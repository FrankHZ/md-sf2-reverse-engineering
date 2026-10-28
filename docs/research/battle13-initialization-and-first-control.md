# Battle13 actual party, neutral entity and first conditional control

Evidence date: 2026-10-22. **Confirmed:** the complete selected12/16/3/1
declaration connects to actual membership/placement, six base enemy definitions,
three ordered regions, fixed/live orders and first conditional control. All
twelve fresh positions miss all three regions. Gerhalt's force admission,
Peter's prior AI-bit clear and Luke's HP0 neutral159 are independent mechanisms.
Battle13 has no random-definition substitution or special-drop row.
**Inferred:** this composition describes the intended Polca rescue encounter.
**Unknown:** natural party/state, first actor/actions, completed services,
presentation, timing and persistence. This is static content, not observed play
or an admitted production encounter.

## Agreement and provenance

This is the Standard-rigor agreement for
[Issue1236](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1236),
within [Epic1053's finite scope](./story-content-coverage.md#scope-and-reading-rules).
The outcome is maintainable content for conversion review: connect every selected
formation, definition, region and order operand to its actual consumer through
first conditional control. Recruitment/control, field/neutral/combatant populations
and production admission boundaries justify Standard rigor.

Reuse [Bedoe return/admission](./bedoe-return-through-battle13-admission.md) for
the complete confrontation, texts, Gerhalt join and posted warp; reuse
[Volcanon](./bedoe-to-volcanon-audience.md#peter-membership-capacity-ai-and-following)
for Peter's membership/AI producer, [outcomes/raft](./battle13-outcomes-through-polca-raft-grant.md)
for the leader terminal and Luke following, and
[later Luke recruitment](./polca-departure-through-luke-recruitment.md) for his
separate join. Their complete scenes are not duplicated. Shared lifecycles remain
at [Battle02](./battle02-initialization-and-first-control.md),
[Battle11](./battle11-initialization-and-first-control.md) and
[Battle12](./battle12-initialization-and-first-control.md).

Owned tracked writes are this document, `story-content-coverage.md` and
`source-coverage.md`, with only Battle13 consistency in the latter two. No shared
writer or stacked dependency was found. Fixtures, schemas, registries, counters,
code, design and runtime frontiers are outside this diff. No parser/platform,
allowlist, implementation, acquisition, comparator, lowering or rebuild is added.
Acceptance is complete selected source/consumer review, direct document/private/
scope checks, clean committed planner and exact-head scoped CI, then a frozen
pushed Draft PR for independent main-gate acceptance. PR692 is unconsumed.

Original paths below resolve under `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at that pin; the
[USA baseline](../../README.md#pinned-original-baseline) retains input identity.
Accepted repository base is `93c82f532b6b6b849ff9117d788afcec952c7e6e`, tree
`171c56ad0420204a8354c8049d0ceb8dc00b3120`. No fresh ROM-equivalence or runtime
claim is made. Short battle-code paths have prefix `code/gameflow/battle/`;
sibling filenames retain the preceding directory. `j_` aliases resolve to the
named implementation bodies. Actual opcodes, values and branch polarity control
over comments, labels or command names.

| Claim | Complete selected declaration and consuming boundary |
| --- | --- |
| Complete12/16/3/1 | `data/battles/spritesets/entries.asm` slot13 → `spritesets/spriteset13.asm:BattleSpriteset13`, `0x1B3E86..0x1B4000`; `sf2macros.asm:allyCombatant/enemyCombatant/combatantAiAndItem/combatantBehavior`, `sf2enums.asm`; `getbattlespritesetsubsection.asm:GetBattleSpritesetSubsection`, `0x1B1630..0x1B169E`. |
| Map, globals and entry | `code/common/maps/getbattle.asm:CheckBattle`, `0x799C`; `code/gameflow/mainloop.asm`; row13 of `data/battles/global/battlemapcoords.asm`, `custombackgrounds.asm`, `enemyleaderpresence.asm`; `data/battles/terrainentries.asm` slot13 → `battleloop/loadbattleterraindata.asm`; complete `data/battles/global/halvedexpearnedbattles.asm`, `afterbattlepositions.asm`, `battleneutralentities.asm`; `battleloop_1.asm`, `battlefunctions/loadBattle.asm`; `data/battles/cutscenes/` before/start table rows13 and their wrappers in `code/gameflow/battle/cutscenes/`. |
| Actual party/scene/neutral | `battleloop/initializecombatants.asm`, `0x1B1272..0x1B15F8`; `battleloop/heallivingandimmortalallies.asm:HealLivingAndImmortalAllies`; `code/common/stats/battleparty.asm:JoinForce/JoinBattleParty/LeaveBattleParty/UpdateForce`; `updatecombatantstats.asm`; Map13 `scripts.asm:cs_585FE` record80 and Map11 `scripts.asm:cs_57AAA` records110/111; `code/common/scripting/map/mapscriptengine_2.asm:csc08_joinForce`, `mapscriptengine_1.asm:csc54_joinForceAi`; `code/common/scripting/entity/entityfunctions_1.asm:PositionBattleEntities/DeclareNewEntity` (containing file `0x444A2..0x448C4`), `sf2const.asm:ENTITY_INDEX_LIST/ENTITY_INDEX_LIST_ENEMIES`, `battlefunctions/battlefunctions_0.asm:GetEntityIndexForCombatant` and `data/scripting/entity/eas_battleneutralentities.asm:eas_LyingLeft`. |
| Six definitions/gear/spells/drops | `data/stats/enemies/enemydefs.asm` records7/8/42/58/69/78; complete `data/battles/global/randombattles.asm` and `enemyitemdrops.asm`; `battleloop/upgradeenemies.asm:UpgradeRandomBattleEnemies/IsBattleUpgradable`, `0x1B1780..0x1B1A28`; `battleloop/initializecombatants.asm:InitializeEnemyStats/InitializeEnemyItems/AdjustEnemyBaseAttForDifficulty`; `data/stats/items/itemdefs.asm` records35/46/88; `code/common/stats/itemstats.asm`, `updatecombatantstats.asm`; `data/stats/spells/spelldefs.asm`; `ai/getnextuseableaiaction.asm`, `gethighestusablespelllevel.asm`, `command/attack.asm`, `command/heal.asm`, `command/heal/determinehealingspelllevel.asm`, `command/support.asm`; `battleactions/dropenemyitem.asm`. |
| Regions, standby and orders | Complete `battleloop/activateenemies.asm`, `triggerregions.asm`, `0x1AC9FC..0x1ACF30`, `clearaimemory.asm`, `populateenemyspawns.asm`; `data/battles/cutscenes/regionactivatedcutscenes.asm`; `code/common/stats/combatantstats_1.asm:GetMoveOrders/GetTriggerRegions`, `combatantstats_2.asm:SetTriggerRegions`; `battlefield/getmoveorderposition.asm`, `0x1B15F8`; `ai/startaicontrol.asm`, both `determineaistandbymovement_*.asm`, `executeaicommand.asm`, `command/moveorder.asm`, `command/move.asm`; `data/battles/global/aicommandsets.asm`, `0xE1AC..0xE25B`. |
| Round/first control | `battleloop/turnorderfunctions.asm:GenerateBattleTurnOrder`; `battlefunctions/executeindividualturn.asm:ExecuteIndividualTurn`, `0x23EB0..0x24242`; actual HP/status/activation/control branches and main versus thinking RNG. |

The accepted [spriteset](./battle-spriteset-data.md), [routing](./battle-routing-data.md),
[global](./battle-global-data.md), [loop](./battle-loop.md), [AI](./battle-ai.md)
and [cutscene](./battle-cutscenes.md) inventories retain their executable limits.
`rowContentTracked:false` and `routeTargetsParsed:false` are unchanged; prose
composition does not become executable Battle13 coverage or add a fixture.

## Map13 selection, globals and entry lifetimes

**Confirmed:** `BATTLE_POLCA_VILLAGE=13` selects the sole Map13 row, wildcard
trigger`(255,255)`, output crop`(0,0,32,32)`, with F413 set. The crop is an output,
not a containment predicate. F413 clear admits no Battle13; F513 set clears413
on a qualifying selection but **still selects13 on this call**. F463 intro and
F88 suspension are independent. Admission needs neither F715 nor Gerhalt/Peter/
Luke membership, HP, following or prior scene completion. The intact scene warp
`Map13,(0,0),Right` can qualify, conditional on its services and pending consumer;
reuse its owner for the full preceding scene/request sequence.

Terrain slot13 selects `BattleTerrain13`, Stack-decoded into shared48×48 storage,
distinct from the32×32 operating crop. Background13 is TOWN12, a fixed selection
at `GetBattlesceneBackground`, not a rendered result. Leader-presence13 is-1;
the [outcome owner](./battle13-outcomes-through-polca-raft-grant.md#battle13-outcome-selection-and-return)
owns its actual enemy128/death-list terminal. LEADER14 independently describes
128's commandset. Half-EXP selects only1; after-position overrides only5.
The neutral table does contain13 and is consumed during `LoadBattle` below.

| Supplied state | Actual entry sequence |
| --- | --- |
| F88 clear/F463 clear | Clear PLAYER_TYPE/set399; clear seconds; save/install map/battle/base VInts; run empty before13; restore/reassert map/battle; clear90–105; heal; initialize actual allies and sixteen enemies; clear AI memory; `LoadBattle(0)` including entity/neutral placement; start wrapper sets463 then runs empty start13; prepare round. |
| F88 clear/F463 set | Skip both hook bodies, while fresh healing/formation/base-definition refresh/load/new round still run.513 is not463. |
| F88 set | Restore saved seconds; clear88/AI memory/view target; load with incoming focus, including completion-gated neutral consumer; enter retained queue/cursor. No fresh field installation, hooks, healing, formation, region reset, definition refresh or new round precedes that loop. A reached sentinel later requests a round. |

Both dispatch rows select `ms_Empty`, not `cs_585FE`. Before only tests463;
start tests/sets it. The empty script's terminator can still wait for scrolling
and clears scroll speed. Neither hook joins, clears AI, repairs scene flags,
changes followers or writes EGRESS_MAP. Stored egress remains an input through
the shared savepoint/F399 path. F513 separately suppresses neutral declaration;
it does not itself suppress initialization or replace enemy definitions.

## Twelve templates, Gerhalt and Peter's actual participation

**Confirmed:** header4 +28 combatants×12 +3 regions×12 +1 point×2 =378
(`0x17A`), matching `0x1B3E86..0x1B4000` with exclusive end. All twelve ally
templates declare HEALER1/0, item NOTHING127, primary/secondary NONE255 with
regions15/15, filler0 and STARTING0. Complete local positions in source order:

| Template | X,Y | Template | X,Y |
| --- | --- | --- | --- |
| 0 | `(22,9)` | 6 | `(21,8)` |
| 1 | `(23,9)` | 7 | `(23,7)` |
| 2 | `(24,8)` | 8 | `(22,7)` |
| 3 | `(23,8)` | 9 | `(20,8)` |
| 4 | `(22,8)` | 10 | `(20,9)` |
| 5 | `(21,9)` | 11 | `(24,7)` |

The [membership/cursor rule](./battle02-initialization-and-first-control.md#joined-active-dead-and-capacity)
sets all30 ally Xs to-1, rebuilds ascending joined/active lists using flags0–29
and32–61, then copies only template X/Y to actual living members. Template IDs,
AI, items and spells are not copied. A dead ordinary member consumes a list
iteration without advancing the formation cursor; twelve iterations bound this
encounter. A remaining active tail loses active flag/X via `LeaveBattleParty`,
retaining joined; lists are not rebuilt there. Dead-created spare positions do
not enlarge capacity. The header does not select twelve fixed heroes. The zero
template-count guard is not an empty-active-party pre-loop guard.

Healing scans all30 independently of membership. Living allies and immortal
Peter7/Lemon28 receive max HP/MP; ordinary dead members remain dead. The initial
`status & 7` preserves STUN/POISON/CURSE **as intermediate state**. Subsequent
`UpdateCombatantStats` preserves STUN/POISON and recomputes final CURSE from
equipped cursed gear. An old CURSE bit alone does not establish final curse.
Actual class/level/gear/status/activation history still determines refreshed
stats, life/placement and control.

`cs_585FE`'s ordinary `join GERHALT` sets F9, calls `UpdateForce`, then sets
F41 only when the rebuilt active count is below12. At full capacity a previously
clear41 stays clear; already-set41 stays set even if overfull. No eviction occurs
at joining, and the cache precedes the new active write. It neither tests/revives
HP nor changes activation/AI or following. Fresh Battle13 rebuilds lists again;
Gerhalt9 can receive the actual next position only when joined, active, within
the twelve member iterations and alive. Ordinary HP0 Gerhalt is not healed.
The fixed field actor at `(11,9)` and his later scene motions prove no such
deployment or guaranteed player turn. No template is reserved for him.

The earlier Volcanon scene separately performs ordinary Peter join (F7 and
capacity-dependent F39), then `joinForceAI PETER,0`. `csc54_joinForceAi` reads
the activation word and writes old word AND65531: selector0 clears bit2 and
preserves all other bits; this branch itself does not join. It leaves F67
unchanged. This supersedes the earlier castle's forced-AI producer only if the
Volcanon clear is actually reached. F722 and dialogue are not substitutes for
that write. Fresh immortal healing can revive Peter, but reserve/follower-only
Peter does not become deployed. A reached living Peter turn without bit2 can
still use AI under MUDDLE/auto-battle or fail action under SLEEP/STUN. Supplied
bit2 instead routes ally AI to set6, independently of template HEALER1.

`PositionBattleEntities` clears old entity data and rebuilds from placed
combatants. The prebattle scene's 128 Dark Soldier,129/130 Minotaurs and131/132
birdmen do not retain those identities as battle128–132, and its Gerhalt/Peter/
Luke field actors are not preserved combatant records. Following is not force
membership. Field action/animation completion cannot establish the new roster.

## Luke's neutral159, field identity and later join

**Confirmed:** `battleneutralentities.asm` has one Battle13 row: local`(16,13)`,
Down, `LUKE_BASE`, `eas_LyingLeft`. `PositionBattleEntities` first builds ordinary
allies/enemies, then tests completion F513. Clear513 admits this row; set513 skips
the entire neutral pass. F463 is not its gate, and no F10/F42/F75/HP condition
selects the row. On the admitted pass it sets combatant159 max/currentHP0 and
status0, ORs AIBITFIELD_NEUTRAL8 into its existing activation word, writes X16/Y13
and declares a separate entity at those coordinates (crop origin0,0).
The pose script selects Left orientation, updates the sprite, then falls through
to motion-off and an idle branch. It does not inflict damage or join/revive ally10.

Before that pass, fresh enemy initialization has given undeclared144–159 HP0
and X=-1. Neutral construction places159 while retaining HP0; it is **not a
seventeenth ordinary enemy**. The normal enemy positioning pass skips neutral
bit3. The neutral pass resets a1 to `ENTITY_INDEX_LIST_ENEMIES=$FFB160`, then
predecrements its mapping write: the one physical index goes to `$FFB15F`, offset31
from `ENTITY_INDEX_LIST=$FFB140`, not enemy159's offset63. `DeclareNewEntity`
stores that physical index as ENTNUM and does not repair the mapping. In the fresh
path,159's normal mapping at `$FFB17F` remains `$FF` from the skipped unplaced
enemy pass; `battlefunctions_0.asm:GetEntityIndexForCombatant` resolves159 via
`159−$60=63`, whereas lookup31 reaches the neutral entity. Combatant159's HP/
coordinates and the declared Luke sprite therefore do not establish an ordinary
159→entity mapping. Activation skips159 on HP0; the ordinary turn scan only
includes128–157 and would also reject HP0. Remaining-enemy counting scans all32
but does not count this HP0 record.
Thus the source population declaration is not a targetable living Luke combatant
or an additional defeat requirement. No claim of observed rendered injury follows.

`LoadBattle` also runs this F513-dependent placement on the suspended branch;
it does not replay fresh enemy clearing there. Set513 skips new declaration,
rather than proving every supplied saved neutral field has been erased.

Lookup10 in the earlier confrontation is the Luke field actor; battle neutral159
is a different identity. With arbitrary supplied F10/F42/HP, an actual joined Luke
could independently qualify as ally10 in the ordinary formation, while clear513
still declares neutral159. The row neither prohibits that state nor guarantees
deployed Luke. Accepted aftermath sets F75/current following, without joining him;
the later Map72 init scene separately performs `JoinForce(10)` (F10/conditional42)
and unlock416. These are distinct lifetimes, not a carried seventeenth enemy or
an implicit recruitment during Battle13 entry.

## Six definitions and all sixteen enemy rows

**Confirmed:** every declared enemy adds NOTHING127, uses secondary order NONE255,
filler0 and STARTING0. Slots128–143 initialize immediately, without the spawning
occupancy gate. Undeclared144–159 get max/currentHP0 and X=-1; the neutral consumer
above separately places159. Complete differences are:

| Row / slot | Base type / local X,Y | Commandset | Primary order | Primary/secondary regions |
| --- | --- | --- | --- | --- |
| 0 /128 | MASTER_MAGE69 `(9,24)` | LEADER14 | MOVE_TO\|0 (`$40`) | 15/15 |
| 1 /129 | BLACK_MONK78 `(10,25)` | HEALER3/2 | FOLLOW_ENEMY\|0 (`$80`) | 15/15 |
| 2 /130 | DEATH_ARCHER58 `(9,23)` | ATTACKER3/9 | FOLLOW_ENEMY\|0 | 15/15 |
| 3 /131 | EVIL_CLOUD42 `(16,28)` | ATTACKER1/6 | NONE | 2/15 |
| 4 /132 | DEATH_ARCHER58 `(15,29)` | ATTACKER1/6 | NONE | 2/15 |
| 5 /133 | ZOMBIE8 `(14,29)` | ATTACKER1/6 | NONE | 2/15 |
| 6 /134 | HOBGOBLIN7 `(8,22)` | ATTACKER3/9 | FOLLOW_ENEMY\|7 (`$87`) | 2/2 |
| 7 /135 | DEATH_ARCHER58 `(6,23)` | ATTACKER2/7 | NONE | 2/15 |
| 8 /136 | ZOMBIE8 `(5,22)` | ATTACKER3/9 | FOLLOW_ENEMY\|7 | 2/2 |
| 9 /137 | ZOMBIE8 `(26,19)` | ATTACKER1/6 | NONE | 1/15 |
| 10 /138 | EVIL_CLOUD42 `(25,20)` | ATTACKER1/6 | NONE | 1/15 |
| 11 /139 | HOBGOBLIN7 `(10,9)` | ATTACKER1/6 | NONE | 0/15 |
| 12 /140 | DEATH_ARCHER58 `(8,13)` | ATTACKER1/6 | NONE | 0/15 |
| 13 /141 | ZOMBIE8 `(11,17)` | ATTACKER1/6 | NONE | 0/15 |
| 14 /142 | HOBGOBLIN7 `(15,12)` | ATTACKER1/6 | NONE | 15/15 |
| 15 /143 | ZOMBIE8 `(16,14)` | ATTACKER1/6 | NONE | 15/15 |

Same-region2/2 applies to134/136, whose follow target is135. Slot138 is Cloud
with1/15/NONE. Source declaration and row→128 offset determine these identities
independently of scene numbers and follow-target names.

The six full records all declare spellPower REGULAR and initialStatus NONE.
The `unknownByte` equals the base type ID here; it is not an interpreted AI rule.
Four item/spell slots are retained below: “rest NOTHING” fills all unused slots.

| Definition | Level; HP/MP; base ATT/DEF/AGI/MOV | Four-slot gear / spells | Movement; base AI word |
| --- | --- | --- | --- |
| Master Mage69 | 14;28/14;11/18/22/5 | Iron Rod88 equipped, rest NOTHING / BLAZE2, rest NOTHING | MAGE; PRIORITYMOD_2\|BIT4 (`$2010`) |
| Black Monk78 | 14;30/19;27/18/21/6 | all NOTHING / HEAL2, BLAST1, NOTHING, NOTHING | HEALER; PRIORITYMOD_3 (`$3000`) |
| Death Archer58 | 10;27/0;13/19/20/5 | Iron Arrow46 equipped, rest NOTHING / all NOTHING | ARCHER; PRIORITYMOD_2\|BIT4 (`$2010`) |
| Evil Cloud42 | 10;22/0;24/18/17/5 | all NOTHING / all NOTHING | HOVERING; PRIORITYMOD_2 (`$2000`) |
| Zombie8 | 11;27/0;26/20/18/5 | all NOTHING / all NOTHING | REGULAR; PRIORITYMOD_2\|BIT5 (`$2020`) |
| Hobgoblin7 | 9;21/0;10/19/19/5 | Middle Axe35 equipped, rest NOTHING / all NOTHING | REGULAR; PRIORITYMOD_2\|BIT5 (`$2020`) |

Master Mage resistance is FIRE_MINOR; Monk/Archer/Hobgoblin have0. Cloud declares
WIND_WEAKNESS|ICE_MAJOR|FIRE_WEAKNESS|STATUS_MINOR; Zombie substitutes STATUS_IMMUNITY
for that last token. Ordinary prowess on Mage/Archer/Cloud/Hobgoblin is
CRITICAL150_1IN32|DOUBLE_1IN32|COUNTER_1IN32. Monk instead has
CRITICAL150_1IN4|DOUBLE_1IN16|COUNTER_1IN16; Zombie has
CRITICAL_POISON|DOUBLE_1IN32|COUNTER_1IN32. These complete operands require their
actual attack/spell/status consumers; they are not a performed critical/poison
or proof that every resistance effect is implemented by the remake.

The complete random list omits POLCA_VILLAGE13. `UpgradeRandomBattleEnemies`
therefore returns the original base before Bowie's effective-level/category/
exclusion/RNG processing, even with513 set. No Battle10 airborne upgrade scan
or alternate Master Mage100 definition is selected here. Fresh initialization
copies definitions, fills HP/MP, installs X/Y/mover/commandset/orders/regions,
returns immediately for added NOTHING, constructs activation from priority's
upper nibble plus STARTING/filler0, adjusts difficulty ATT and refreshes stats.
Initial activation is `$2000` except Monk `$3000`; definition BIT4/BIT5 do not
survive that initialization-word composition as active bits.

`AdjustEnemyBaseAttForDifficulty`'s repeated BEQ makes its nominal early return
unreachable: all ordinary settings apply floor(baseATT×5/4) once; SUPER2 applies
that floor twice. Equipped effects then add ATT. Iron Rod and Iron Arrow each
add12; Middle Axe adds13, with their other two effects NONE0 and useSpell NOTHING.
Rod/Axe range1–1, Arrow2–2. Complete resulting fresh ATT relationships:

| Type | Ordinary settings / SUPER, after gear |
| --- | --- |
| Mage69 | 25 /28 |
| Monk78 | 33 /41 |
| Archer58 | 28 /32 |
| Cloud42 | 30 /37 |
| Zombie8 | 32 /40 |
| Hobgoblin7 | 25 /28 |

AGI stays22/21/20/17/18/19 respectively, with no extra-turn bit. None of the three
weapons is cursed. Enemy item application bypasses ally curse assignment; ally
intermediate status versus recomputed CURSE remains a separate qualification.
The independent complete drop table has no13 row. Its actual battle/enemy/item/
once-bit/action lookup supplies no special selected drop; equipped weapons,
placement NOTHING and LEADER14 do not authorize one. Terminal/reward behavior
remains with the existing outcome owner.

### Actual selected attack and healing spell decisions

| Declared spell | MP / range / radius / power | Properties |
| --- | --- | --- |
| BLAZE1 /2, Mage's usable levels | 2 /1–2 /0 /6; 6 /1–2 /1 /10 | TYPE_ATTACK\|AFFECTEDBYSILENCE |
| BLAST1, Monk | 2 /1–1 /0 /6 | TYPE_ATTACK\|AFFECTEDBYSILENCE |
| HEAL1 /2, Monk's known levels | 3 /0–1 /0 /15; 5 /0–2 /0 /15 | TYPE_HEAL\|TARGET_TEAMMATES\|AFFECTEDBYSILENCE |

Attack selection passes a masked base ID to `GetHighestUsableSpellLevel`; with
Mage's BLAZE2, MP≥6 permits2, MP2–5 permits1, MP<2 permits neither. Fresh14 MP
covers2. Monk's fresh19 MP covers BLAST1. Confusion/type/priority/range/casting
position and earlier command success remain predicates. Silence-affected spell
properties are not a demonstrated silence predicate in these AI getters or an
executed effect.

Healing follows the actual [accepted Battle12 helper account](./battle12-initialization-and-first-control.md#all-twelve-enemies-and-complete-selected-definitions),
with Black Monk's own19 MP and HEAL2 operand. `aiCommand_Heal` rejects Muddle2,
checks HEAL's preliminary3 MP, then admits living friendly targets with
3×HP≤2×maxHP. Fresh full-HP enemies supply no injured target. In
`DetermineHealingSpellLevel`, missingHP≤2 returns-1; otherwise highest known
level2 (`d5=1`) leaves selected `d2=0`, including missingHP>14 because the higher
threshold requires known level3. The MP loop shifts by5 and adds **unmasked**
`d4.b=HEAL|LV2=$40`. For this selected d2=0 it therefore looks up HEAL2's **5 MP**
cost while returning level0/HEAL1. MP3/4 passes the preliminary test but fails
this loop; MP≥5 can proceed as HEAL1. The caller's non-self ≥11 MP promotion
also requires known level3+, so fresh19 MP does not promote this Monk. The final
rewrite/range/position uses HEAL1/range0–1/power15, conditional on success.
HEAL2 in commandset2 is an unused command parameter here, not spell level2.
No learned support spell exists. This preserves the helper's defect, without
repairing it or inferring an actual first heal/cast from names.

## Three ordered regions and all fresh arithmetic

**Confirmed:** each12-byte region is type4, byte1=0, four ordered vertices,
then two zero bytes. Persistent flags are90–92:

| Region / flag | Vertex1 /2 /3 /4 |
| --- | --- |
| 0 /90 | `(0,0)` /`(0,31)` /`(26,31)` /`(12,0)` |
| 1 /91 | `(0,12)` /`(0,31)` /`(31,31)` /`(31,12)` |
| 2 /92 | `(0,11)` /`(0,31)` /`(31,31)` /`(31,23)` |

`IsRegionTriggered` tests triangles1/2/4 then3/2/4. It scans all30 allies with
positive signed HP and nonnegative signed-byte X/Y, independently of joined,
active and follower flags. `IsAllyInTriangle` tests edges a→b, c→a, c→b against
opposite c,b,a. `CheckTestPointPositionRelativeToSegment` takes horizontal
dominance on equal absolute spans and signed MULS/DIVS, truncating toward zero:

- Horizontal: `Yproj = By + trunc((By−Ay)×(Cx−Bx)/(Bx−Ax))`.
- Vertical: `Xproj = Bx + trunc((Bx−Ax)×(Cy−By)/(By−Ay))`.

Projection smaller than the actual other coordinate produces10; larger produces00.
Equality is01 only within dominant-axis segment bounds, otherwise10. Negative
projection writes CCR=1 and produces10 despite the11 comment. Degenerate fallback
has separate CCR behavior; no selected edge is degenerate. In-bounds01 accepts
immediately; otherwise ordinary flags must match the opposite vertex. Bit0
exceptions skip the first two edges but reject at the third. Cross products
cannot substitute for this original integer projection/CCR procedure.

All twelve positions were substituted into both triangles of all three regions,
36 region-position / **72 triangle substitutions**. The first rejection for each
position in a triangle is consistently:

| Region | Triangle1/2/4 | Triangle3/2/4 |
| --- | --- | --- |
| 0 | Third edge4→2, vertical Xproj8–9 < allyX20–24 gives10; opposite1 X0 versus12 gives00. | Second edge4→3, vertical Xproj16–17 < allyX gives10; opposite2 X0 versus26 gives00. |
| 1 | Second edge4→1, horizontal Yproj12 > allyY7–9 gives00; opposite2 Y31 versus12 gives10. | Third edge4→2, horizontal Yproj17–19 > allyY gives00; opposite3 Y31 versus12 gives10. |
| 2 | Second edge4→1, horizontal Yproj18–20 > allyY gives00; opposite2 Y31 versus11 gives10. | Third edge4→2, horizontal Yproj25–26 > allyY gives00; opposite3 Y31 versus23 gives10. |

No fresh position is collinear in bounds on a reached edge. Actual fresh living
placements are a prefix/subset; other ally Xs reset to-1. Thus fresh placements
trigger none of90–92. This proves neither later crossings, arbitrary saved
positions nor equivalence of another geometry algorithm.

`ActivateEnemies` scans128–159 and falls through once for160 because the outer
RTS is absent; the [extra160/deals alias owner](./battle26-activation-and-first-turn-dispatch.md#the-extra-fall-through-and-its-selected-ram-alias)
retains that defect. Negative X, HP0 or nonenemy skips testing. The tested-word
cache NEWLY_TRIGGERED_BATTLE_REGIONS includes misses and differs from persistent
90–105. Fresh flag reset and `ClearAiMemory` do not explicitly clear that word;
non-swarm `StartAiControl` later does. Incoming tested bits can suppress tests.
With incoming bits clear, the selected formation produces tested7 and no90–92.
Even15/15 enemies test all three regions before assigned activation.

| Assigned enemies | Actual primary/secondary result |
| --- | --- |
| 139–141,0/15 | F90 sets bit0; sentinel secondary supplies no polygon. |
| 137/138,1/15 | F91 sets bit0. |
| 131–133/135,2/15 | F92 sets bit0. |
| 134/136,2/2 | F92 hits primary first and returns after bit0; it cannot independently reach a secondary2 path in this ordinary call. |
| 128–130/142/143,15/15 | No assigned-region activation write; both sentinels bypass inactive standby. |

Misses clear neither persistent flags nor activation bits. Primary preserves an
already-set secondary bit; an independently reached secondary hit sets both bits.
Thus supplied bit1 can remain sticky even on2/2, but same-region assignment does
not itself set it or select a different stored order. Activation never swaps
primary/secondary order bytes. A fresh2/2 word with F92 set gains bit0 only.

## Standby, all selected streams and live referents

**Confirmed:** region-program table entries are only32/40, not13. All selected
spawn modes are0; no hidden/respawn candidate is declared. With fresh persistent
flags clear,131–141 take inactive standby, while128–130/142/143 enter commandsets.
The two follow-assigned inactive actors134/136 return d1=1 from
`ValidateAiStandbyEligibility` (non-NONE primary with assigned region2).
After any initial thinking-RNG stop, the caller treats nonzero as stop with an
empty move string/STAY, despite the “allow movement” comment. It does not follow
135 while inactive. The nine inactive NONE actors return d1=0/d2=0 and anchor
conditional standby movement at their own starting positions. Terrain, occupancy,
candidate memory and thinking RNG still decide whether a move string is built;
NONE does not mean no possible movement. STAY can follow movement.

Commands are attempted in order; d1=0 ends successfully, nonzero advances:

| Set / actual users | Complete stream / initial pathfinding mode |
| --- | --- |
| HEALER3/2 /129 | HEAL2, MOVE_ORDER4, SUPPORT, ATTACK3, MOVE2, STAY /block-non-movable |
| ATTACKER1/6 /131–133,137–143; actual AI allies independently | ATTACK1, HEAL1, SUPPORT, MOVE1, STAY /regular |
| ATTACKER2/7 /135 | MOVE_ORDER1, ATTACK1, HEAL1, SUPPORT, MOVE1, STAY /regular |
| ATTACKER3/9 /130,134,136 | MOVE_ORDER3, ATTACK3, HEAL2, SUPPORT, MOVE_ORDER4, MOVE2, STAY /block-non-movable |
| LEADER14 /128 | MOVE_ORDER5, ATTACK2, HEAL3, SUPPORT, MOVE_ORDER2, MOVE3, STAY /block-and-carve |

Ally-template HEALER1/0 is not installed by placement; its declaration is not a
sixth actual enemy stream. MOVE_ORDER1 uses target type0/regular,4 type0/
block-non-movable,5 type0/block-and-carve;3 uses follow type1/block-non-movable,
2 follow type2/block-and-carve. The stored order's bit6 still determines the
coordinate referent, independently of the command's target-type name:

- 128's MOVE_TO0 selects the sole fixed point`(9,24)`, including its follow-type
  fallback. Equal starting position is not permanent immobility.
- 129/130's FOLLOW_ENEMY0 reads live enemy128, not fixed point0, ally0 or the
  prebattle Dark Soldier scene actor. Monk129's MOVE_ORDER4 still resolves live128.
- 134/136's FOLLOW_ENEMY7 reads live135 (Death Archer, initially`(6,23)`), not
  enemy134/136, scene lookup7, ally Peter7 or an undeclared AI point7.

`StartAiControl` promotes a dead primary live referent to secondary NONE and clears
secondary before commandset dispatch. Here128's death affects129/130,135's death
affects134/136; the point order bypasses that HP test. This is not a region-order
swap. `aiCommand_MoveOrder` rejects absent orders, zero MOV or dead live referents;
its point path tests returned X with `tst.b`, not the-1 failure sentinel. Follow
mode has terrain restrictions; either path can succeed with attack or build
movement/STAY. Empty movement can keep a nonzero result and advance the stream;
temporary restrictions are reapplied and finally cleared. No fixed first move
or attack follows from an order. Set7's NONE makes MOVE_ORDER1 return-1 before
trying ATTACK1 and the remainder; set6 has **no MOVE_ORDER**. HEAL1/2/3 command
parameters do not choose known spell levels in this healing body.

## Round and conditional first control endpoint

**Confirmed:** round prefix is activation → sparse region program → spawn
selection → turn order. The shared rule clears64 sentinel entries, scans30 allies
then the first30 enemies, admits placed/living actors independently of activation,
and uses refreshed AGI/main RNG. All sixteen declared enemies qualify; neutral159
does not. Their ordered AGIs are22,21,20,17,20,18,19,20,18,18,17,19,20,18,19,18;
none has the extra-turn bit. Ordinary score is
`AGI + RNG(AGI>>3) − RNG(AGI>>3) + RNG(3) −1`. The extra-turn branch has its
separate two-draw score; range0 still advances the seed. Sixty-two signed-byte
stable-sort passes preserve append-order ties and cursor becomes0. No random-
upgrade draws precede this selected battle. Standby uses separate thinking RNG.
Actual ally state and supplied seeds still determine the first actor.

A sentinel starts another round; an HP0 actor skips. `ExecuteIndividualTurn`
gives MUDDLE/AI bit2 priority to AI preparation. Otherwise an enemy with
**opponent-control=0** uses AI; nonzero reaches player preparation. Ally
auto-battle independently chooses AI. After cursor/view/windows/grid work,
SLEEP/STUN choose NoAction; dispatch repeats MUDDLE/bit2/control polarity before
AI execution versus player input. Stop at that conditional boundary, before a
performed action. Clearing Peter's bit2 does not guarantee a player turn; Gerhalt
membership guarantees neither placement nor action, and neutral Luke has no
ordinary turn. F88 can supply a retained queue/cursor instead. Initialization
does not itself run the leader terminal or establish a player-ready observation.

## Accepted production consumers and conversion limits

**Confirmed by accepted implementation inspection, separate from original
evidence:** Battle13 is not admitted, converted or supported by the current private
route. [Trust](../../remake/docs/content/profiles-and-trust.md#selected-third-encounter)
and [Application](../../remake/docs/application/session-and-programs.md#yeel-guide-zone-request-and-sleep-continuation)
retain selected2/3 consumption. No preparation/comparison/allowlist change or
engine execution is performed. Enemy names alone imply no missing mechanism:

| Actual accepted consumer | Existing mechanism and specific Battle13 boundary |
| --- | --- |
| [prepare_encounter / compile_routes](../../src/sf2tool/remake_exploration_battles.py) | Producer/CLI admits2/3 only; wildcard Map13 does not need11's coordinate-trigger extension, but no13 export/route exists. Hook/load and400/450/500 flag mechanisms exist. This does not provide confrontation/recruitment/neutral conversion. |
| [PrivateBattleEncounterReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs) / [PrivateBattleDefinitionReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleDefinitionReader.cs) | Formats preserve placements/regions/points/scene metadata; trusted encounter pins admit1/2/3, not13. Generic lookup selects base definitions/referenced items/spells without a name prohibition; Mage/Black Monk/Zombie require trusted selection/conversion. No original repeat substitution is required. Descriptive resistance/prowess is not execution. No neutral row is converted by this placement/definition pair. |
| [PrivateBattleScenarioReader.Assemble](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs) / [PrivateBattleActionBindings](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleActionBindings.cs) | Independent region-program admission1/2/3 rejects13. STARTING/NOTHING, sets6/7/9/14, Regular/Healer/Hovering and point/live-follow/NONE bind; HEALER3/2 rejects at source-commandset. Mage/Archer BIT4 fails AI-word regex; MAGE/ARCHER reject mover binding; Arrow2–2 rejects physical binding. Monk's critical/double/counter and Zombie poison reject prowess. Rod/Axe adjacent ATT metadata fit existing binding, before equipped-refresh/AI limits. HEAL data binds, BLAZE/BLAST execution does not. Controlled classes0/1/2/4 omit Gerhalt/Peter/Luke and arbitrary natural parties. |
| [BattleEntry](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs) | Before/load/start flags, membership rebuilding, carried resources/growth/gold/seeds exist; missing actual member definitions reject. F88 rejects instead of restoring saved queue/cursor. Scene Peter/Luke/Gerhalt mapping is not an admitted battle-member definition or their join/control program. |
| [BattleInitializationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleInitializationRules.cs) / [BattleTurnFlow](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleTurnFlow.cs) | Controlled already-refreshed allies, roster-only/difficulty0, living/immortal healing, formation/activation/ATT exist. Excess active tail rejects capacity instead of source eviction. Enemy loadout0/127 excludes equipped Rod/Arrow/Axe. General equipment/CURSE refresh, other difficulty/control policies and saved restoration remain Unsupported. Neutral HP0 entity placement/F513 gating is not supplied by ordinary actor initialization. |
| [BattleActivationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActivationRules.cs) / [TurnOrderRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/TurnOrderRules.cs) | Mode0/RegionProgram.None, living placed allies, primary-first/sticky-secondary and randomized/extra turns exist. Same-region2/2 needs no new activation mechanism or alternate-order selection. Cross-product geometry/fresh per-round tested word does not prove original projection/cache equivalence. No Battle13 observation validates composition. |
| [BattleControlRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleControlRules.cs) / [SourceEnemyAi](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceEnemyAi.cs) / [SourceMoveOrderAi](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceMoveOrderAi.cs) | Unsupported status/automatic-ally/control policies stay exposed; enemy AI rejects ally actors. NONE/follow standby,15/15, sets6/7/9/14, points/live-follow/dead-target promotion exist. Activated AI admits only6/7/9/14 and0/127/no learned spells: Monk2/healing/BLAST, Mage BLAZE/gear, equipped attackers reject. Cloud's spell-free6/hovering physical path is expressible; Zombie additionally needs poison prowess. General ranged/attack/support/healing-spell AI and resistance-aware damage/status do not follow from descriptive records or shared station search. |

These are requirements/Unsupported boundaries at actual consumers, not an
implementation task. Declaration, conversion, successful consumption and natural
observation remain separate. Remake code proves no original-game fact. Existing
scene/graphics-service and outcome consumer limits remain with their owners.

## Unknowns, retained failures and reproduction

**Unknown:** natural route/flags, actual joined/active/dead/capacity/AI/follower
history, Gerhalt/Peter/Luke deployment, classes/levels/status/gear, difficulty/
seeds, saved state/cache, later region crossings, first actor/action/targets/
tactics, complete action behavior, services/presentation, continuity, timing
and durable saves. No Unknown automatically authorizes acquisition or repair.

Accepted Battle01 admission/victory/stable-return/later-Down observations remain.
#1047's twelve **completed unresolved comparisons** remain at the
[verification owner](../../remake/docs/verification/field-programs.md#yeel-guide-through-battle03),
not interrupted runs or passes. Preserve the intermediate-status/final-CURSE
distinction. PR1221's mechanical PASS missed stale coverage/denominators and
opponent-control polarity; its two P2 findings remain in the
[handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1220#issuecomment-6354981415).
Battle12's pre-stage scene-identity and HEAL-helper corrections remain in its
[handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1234).
Mechanical document checks do not replace complete source/consumer review.

Reproduce from all28 combatant rows, three ordered regions/one point and six
full definitions at the pin. Read actual membership/join/AI-clear consumers and
the HP0 neutral pass separately from scene actors. Follow the upgrade early
return, NOTHING return, difficulty/equipment refresh and independent drop table;
then tested cache/persistent flags, primary-first2/2, NONE versus assigned-follow
standby, all five actual streams and conditional turn dispatch. Apply the stated
projection/CCR edge sequence to all72 triangle substitutions; retain the first
rejections above. Read every named production admission/rejection body directly.
These operands/expected results make the account reproducible without depending
on an ignored arithmetic helper.

This is linear selected source reading plus72 triangle substitutions and small
receipts, estimated below64 MiB and one minute for arithmetic. The arithmetic
completed in0.001 seconds; peak memory was not measured. No retained payload was
parsed, regenerated or copied. The retained map-init/script pair totals18,285,979
bytes and `local/issue1005/pair.txt` stays unchanged by length/mtime checks against
retained metadata. New arithmetic/checks/receipts live only in `local/issue1236/`;
old results are preserved. Replan before a concrete required scope/resource
expansion, not for each Unknown.

Direct checks cover UTF-8/LF, relative links/anchors separately from external
references, exact-three-path committed diff, private/ownership, unchanged fixture
counters/four frontiers and retained input metadata. On a clean committed head
load ignored selections in the same launching process:

```powershell
. ./local/private-inputs.ps1
uv run sf2 verify plan --base origin/main --head HEAD
```

Record exact-head scoped CI, semantic review, completed failures and process state
in the Issue handoff. No normal/full, SDK/Godot/emulator/H3/H4, screenshots, new
extraction, comparison/lowering or runtime observation is selected. Stop at clean
pushed frozen Draft PR, Review / Verify, stopped writer/processes and successful
notification to assigned main-gate. Independent acceptance/merge, cleanup,
archival and another slice remain outside this delivery.
