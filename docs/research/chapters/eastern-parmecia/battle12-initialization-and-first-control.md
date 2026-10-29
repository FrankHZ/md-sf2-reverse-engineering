# Battle12 initialization, dual regions and first conditional control

Evidence date: 2026-10-22. **Confirmed:** the complete selected12/12/4/1
declaration connects to actual party placement, five base enemy definitions,
ordered primary/secondary region activation and conditional player/AI/no-action
dispatch. No fresh formation position triggers a region. Battle12 is absent
from the random-upgrade list; F512 does not substitute its definitions.
**Inferred:** these connections describe the intended Mount Volcano encounter.
**Unknown:** natural party/state, first actor/actions, completed services,
presentation, timing and persistence. Static composition is not observed play
or an admitted production encounter.

## Agreement and provenance

This is the Standard-rigor agreement for
[Issue1234](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1234),
within [Epic1053's finite scope](../../story-content-coverage.md#scope-and-reading-rules).
The outcome is maintainable content for future conversion: connect every selected
formation, definition, region and order operand to its consumer through first
conditional control. Party, map-entity, battle-state and AI boundaries justify
Standard rigor. Reuse [Polca/admission](./polca-fairy-through-battle12-admission.md)
for the optional turtle, east exit and searching-devils scene, and
[outcomes/Bedoe](./battle12-outcomes-through-bedoe-entry.md) for leader death,
returns and the mountain continuation. No scene transcript is duplicated.
Shared lifecycles remain at [Battle02](../grans-opening/battle02-initialization-and-first-control.md),
[Battle09](./battle09-initialization-and-first-control.md) and
[Battle11](./battle11-initialization-and-first-control.md).

Owned writes are this document and Battle12 consistency in the two coverage
owners. Fixtures, schemas, registries, counters, code, design and runtime
frontiers stay outside this diff. No parser/platform, allowlist, implementation,
acquisition, comparison, lowering or rebuild is added. Acceptance is complete
selected source/consumer review, direct document/private/scope checks, clean
committed planner and exact-head scoped CI, followed by a frozen pushed Draft PR
for independent main-gate acceptance. PR692 is unconsumed.

Original paths below resolve under `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at that pin; the
[USA baseline](../../../../README.md#pinned-original-baseline) retains input identity.
Accepted repository base is `799d429131933c53e647b3cc462411117222093e`, tree
`92b1594c2280f3e03e96960b6f69d2d484829ae3`. No fresh ROM-equivalence or runtime
claim is made. Short battle-code paths below have prefix
`code/gameflow/battle/`; sibling filenames retain the preceding directory.
`j_` interfaces resolve to the named implementation bodies. Instructions,
register results and branch polarity control over comments or macro names.

| Claim | Exact declaration and consuming boundary |
| --- | --- |
| Complete12/12/4/1 | `data/battles/spritesets/entries.asm` slot12 → `spritesets/spriteset12.asm:BattleSpriteset12`, `0x1B3D30..0x1B3E86`; `sf2macros.asm:allyCombatant/enemyCombatant/combatantAiAndItem/combatantBehavior`, `sf2enums.asm`; `getbattlespritesetsubsection.asm:GetBattleSpritesetSubsection`, `0x1B1630..0x1B169E`. |
| Admission/globals/lifetimes | `code/common/maps/getbattle.asm:CheckBattle`, `0x799C`, `code/gameflow/mainloop.asm`; row12 of `data/battles/global/battlemapcoords.asm`, `custombackgrounds.asm`, `enemyleaderpresence.asm`; complete `halvedexpearnedbattles.asm`, `afterbattlepositions.asm`, `battleneutralentities.asm`; `data/battles/terrainentries.asm` slot12 → `battleloop/loadbattleterraindata.asm`; `battleloop_1.asm:BattleLoop`, `0x23A84`, before/start wrappers and row12 of `data/battles/cutscenes/beforebattlecutscenes.asm`, `battlestartcutscenes.asm`. |
| Actual party and entity replacement | `battleloop/heallivingandimmortalallies.asm`, complete `initializecombatants.asm`, `0x1B1272..0x1B15F8`; `code/common/stats/battleparty.asm`, `updatecombatantstats.asm`; `battlefunctions/loadBattle.asm`, `code/common/scripting/entity/entityfunctions_1.asm:PositionBattleEntities`; admission/outcome owners' Map52 population, follower and script consumers. |
| Definitions/upgrade/gear/spells/drop | `data/stats/enemies/enemydefs.asm` records7/42/58/68/77; `battleloop/upgradeenemies.asm:UpgradeRandomBattleEnemies/IsBattleUpgradable`, `0x1B1780..0x1B1A28`; complete `data/battles/global/randombattles.asm`, `0x1B6DB0..0x1B6DDA`; `data/stats/items/itemdefs.asm` records35/46/87, `data/stats/spells/spelldefs.asm` HEAL/FREEZE/BLAST; `code/common/stats/itemstats.asm`, `ai/getnextuseableaiaction.asm`, `gethighestusablespelllevel.asm`, `command/attack.asm`, `command/heal.asm`, `command/heal/determinehealingspelllevel.asm`, `doescombatantrequirehealing.asm`, `command/support.asm`, `code/common/stats/spellstats.asm:GetSpellDefinitionAddress`; complete `data/battles/global/enemyitemdrops.asm` → `battleactions/dropenemyitem.asm`. |
| Regions/standby/orders | Complete `battleloop/activateenemies.asm`, `triggerregions.asm`, `0x1AC9FC..0x1ACF30`, `clearaimemory.asm`, `populateenemyspawns.asm`; `data/battles/cutscenes/regionactivatedcutscenes.asm`; `code/common/stats/combatantstats_1.asm:GetMoveOrders/GetTriggerRegions`, `combatantstats_2.asm:SetTriggerRegions`; `battlefield/getmoveorderposition.asm`, `0x1B15F8`; `ai/startaicontrol.asm`, both `determineaistandbymovement_*.asm`, `executeaicommand.asm`, `command/moveorder.asm`, `command/move.asm`; `data/battles/global/aicommandsets.asm`, `0xE1AC..0xE25B`. |
| Round/control | `battleloop/turnorderfunctions.asm:GenerateBattleTurnOrder`, `battlefunctions/executeindividualturn.asm:ExecuteIndividualTurn`, `0x23EB0..0x24242`; actual status/activation/control branches and main versus thinking RNG consumers. |

The accepted [spriteset](../../battle/battle-spriteset-data.md),
[routing](../../battle/battle-routing-data.md), [global](../../battle/battle-global-data.md),
[loop](../../battle/battle-loop.md), [AI](../../battle/battle-ai.md) and
[cutscene](../../battle/battle-cutscenes.md) inventories remain unchanged.
`rowContentTracked:false` and `routeTargetsParsed:false` keep their executable
limits. Prose relationships do not create executable Battle12 composition or
change fixture counts.

## Map52 selection and entry lifetimes

**Confirmed:** `BATTLE_MOUNT_VOLCANO=12` is the sole Map52 row: crop
`(0,0,32,32)`, wildcard triggers255/255, unlock412 and completion512.
`CheckBattle` tests unlock and trigger bytes, not crop containment. With412
clear it selects no battle. With412/512 both set it clears412 and still selects12
on that call. Intro462 and suspension88 are separate supplied states.
The [admission owner](./polca-fairy-through-battle12-admission.md#east-exit-and-immediate-map52-admission)
owns both carried412 immediate admission and ordinary searching-scene412
production/reload. Turtle711, Oddler710, scene713, Peter membership/following,
and Battle11 completion are not extra predicates of this row.

Terrain slot12 selects `BattleTerrain12`, Stack-decoded into the shared48×48
array, distinct from the32×32 operating crop. Background12 is MOUNTAIN11,
consumed as a fixed battlescene background selection, not proof of pixels.
Leader-presence12 is-1. Its actual terminal/death-list consumer belongs to
[outcomes](./battle12-outcomes-through-bedoe-entry.md#battle12-outcome-and-return-order);
the independent LEADER14 commandset on128 is not its authority.
The half-EXP list contains only1, after-position overrides only5, and the
complete neutral table has no12 row; their consumers add no selected override
or neutral.

| Supplied state | Actual entry sequence |
| --- | --- |
| F88 clear/F462 clear | Clear PLAYER_TYPE/set399; clear seconds; save/install map/battle/base VInts; run empty before12; restore/reassert map/battle; clear90–105; heal; initialize actual allies and twelve enemies; clear AI memory; `LoadBattle(0)`; start wrapper sets462 then runs empty start12; prepare round. |
| F88 clear/F462 set | Skip both hook bodies, while fresh healing/formation/base definition refresh/load/new round still run.512 is not462. |
| F88 set | Restore saved seconds; clear88/AI memory/view target; load with incoming focus; enter retained queue/cursor. No fresh field installation, hooks, healing, placement, region reset, definition refresh or new round precedes that loop. A reached sentinel later requests a round. |

Both dispatch rows select `ms_Empty`, not missing bespoke before/start files.
Before only tests462; start tests/sets it. The script terminator can still wait
for view scrolling and clears scroll speed. Neither hook joins/activates Peter,
changes force-AI/followers, writes EGRESS_MAP or repairs story state. Stored
egress remains an input consumed through the shared savepoint/F399 route; no
fixed mountain/Bedoe egress tuple follows. Valid saved state and service return
are not established by this static lifetime account.

## Twelve templates versus actual party and scene actors

**Confirmed:** header4 +24 combatants×12 +4 regions×12 +1 point×2 =342
(`0x156`), matching `0x1B3D30..0x1B3E86` with exclusive end. The subsection
getter uses the declared counts and strides. Every ally row has HEALER1/0,
item NOTHING127, primary NONE255/region15, secondary NONE255/region15, filler0
and STARTING0. Complete positions in declaration order:

| Template | Local X,Y | Template | Local X,Y |
| --- | --- | --- | --- |
| 0 | `(5,21)` | 6 | `(4,23)` |
| 1 | `(4,21)` | 7 | `(3,23)` |
| 2 | `(3,21)` | 8 | `(2,21)` |
| 3 | `(4,22)` | 9 | `(2,23)` |
| 4 | `(3,22)` | 10 | `(1,22)` |
| 5 | `(2,22)` | 11 | `(1,23)` |

The [actual membership/cursor rule](../grans-opening/battle02-initialization-and-first-control.md#joined-active-dead-and-capacity)
resets all30 ally Xs, rebuilds ascending joined/active lists (flags0–29 and
32–61), then copies only template X/Y to actual living members. It does not
copy template IDs, AI, inventory or spells. Ordinary dead members consume list
iterations without advancing the formation cursor. Twelve iterations bound
this encounter; a remaining active tail loses active/X through `LeaveBattleParty`
while retaining joined. Lists are not rebuilt there. Death-created spare
positions do not enlarge capacity. `JoinForce`'s separate below12 check and
direct `JoinBattleParty`'s unchecked flag write remain distinct; a zero-party
pre-loop guard does not establish a safe natural empty-party start.

Healing scans all30 independently of membership. Living allies and immortal
Peter7/Lemon28 receive max HP/MP; ordinary dead allies remain dead. Intermediate
`status & 7` retains existing STUN/POISON/CURSE; `UpdateCombatantStats` preserves
STUN/POISON and recomputes final CURSE from equipped cursed gear. An old CURSE
bit alone proves no final curse. Actual classes/levels/gear/status/activation
and membership history determine refreshed stats and later eligibility.

The searching scene uses separately built Map52 entities128 Death Archer,
129 Hobgoblin,130 Witch and131 Evil Cloud. Battle slots128/129/130/131 instead
mean Death Archer/Evil Cloud/Death Monk/Witch. The scene's movement destinations
and four NPCs are not twelve placements or retained combatant AI state.
`LoadBattle`/`PositionBattleEntities` replaces them from actual combatants.
Following entities are not additional battle members.

[Peter's independent states](./battle09-initialization-and-first-control.md#twelve-templates-and-peters-independent-states)
remain joined7, capacity/history-dependent active39, force-AI bit2 and follower67.
The earlier castle `joinForceAI PETER,$FFFF` producer sets bit2, but neither
Map52's searching script nor these empty hooks joins him or changes that bit.
Map52 has no fixed Peter row in either population; follower67 supplies him
independently of joined/active/dead predicates. A missing character mapping
can resolve to physical0, not a safe absent-actor no-op. Actual selected immortal Peter heals
and places at the real cursor; retained bit2 sends his reached turn to ally
commandset6, not template HEALER1. Reserve/follower-only Peter proves no queued
Peter. May's joined8/active40/HP/AI and transient following, and old-man77's
FOLLOWER_A30 map identity, likewise do not establish battle participation or
twelve fixed natural heroes.

## All twelve enemies and complete selected definitions

**Confirmed:** all enemy rows have added item NOTHING127, secondary order
NONE255, filler0 and STARTING0. Slots128–139 initialize immediately without
the spawning-position occupancy gate. Undeclared140–159 receive current/maxHP0
and X=-1, not hidden live additions. Row differences are complete below;
primary/secondary regions are independent of order bytes.

| Row / slot | Base type / local X,Y | Commandset | Primary order | Regions primary/secondary |
| --- | --- | --- | --- | --- |
| 0 /128 | DEATH_ARCHER58 `(22,8)` | LEADER14 | MOVE_TO\|0 (`$40`) | 15/15 |
| 1 /129 | EVIL_CLOUD42 `(15,3)` | ATTACKER1/6 | NONE | 1/3 |
| 2 /130 | DEATH_MONK77 `(24,9)` | HEALER3/2 | FOLLOW_ENEMY\|0 (`$80`) | 15/15 |
| 3 /131 | WITCH68 `(25,15)` | ATTACKER2/7 | NONE | 0/1 |
| 4 /132 | DEATH_MONK77 `(18,14)` | HEALER1/0 | NONE | 2/15 |
| 5 /133 | HOBGOBLIN7 `(20,12)` | ATTACKER1/6 | NONE | 2/15 |
| 6 /134 | WITCH68 `(18,19)` | ATTACKER1/6 | NONE | 15/15 |
| 7 /135 | HOBGOBLIN7 `(19,18)` | ATTACKER1/6 | NONE | 15/15 |
| 8 /136 | DEATH_MONK77 `(23,21)` | HEALER1/0 | NONE | 2/15 |
| 9 /137 | HOBGOBLIN7 `(24,17)` | ATTACKER2/7 | NONE | 0/1 |
| 10 /138 | HOBGOBLIN7 `(11,19)` | ATTACKER1/6 | NONE | 15/15 |
| 11 /139 | HOBGOBLIN7 `(8,18)` | ATTACKER1/6 | NONE | 15/15 |

Point0 is fixed`(22,8)`, initially equal to128's position. `$80` resolves
combatant128's **current** X/Y; it is not point0 or ally0. NONE is255; region15
is a sentinel, not polygon15. `GetMoveOrders` returns stored primary/secondary
bytes; activation's secondary bit does not select another order byte.

The five full `enemydefs.asm` records have REGULAR spell power, initial status
NONE and `unknownByte` equal to their IDs. Items/spells each have four slots;
after the listed entries all remaining slots are NOTHING. Stats are definition
operands, before difficulty/equipment refresh:

| Definition / ID | Level / HP / MP | ATT,DEF,AGI,MOV | Items / highest learned spells | Movement / resistance / definition AI |
| --- | --- | --- | --- | --- |
| Death Archer58 | 10 /27 /0 | 13,19,20,5 | equipped Iron Arrow /none | ARCHER /0 /PRIORITYMOD_2\|BIT4 |
| Evil Cloud42 | 10 /22 /0 | 24,18,17,5 | none /none | HOVERING /WIND_WEAKNESS\|ICE_MAJOR\|FIRE_WEAKNESS\|STATUS_MINOR /PRIORITYMOD_2 |
| Death Monk77 | 8 /23 /13 | 22,14,18,6 | none /HEAL2, BLAST1 | HEALER /0 /PRIORITYMOD_3 |
| Witch68 | 7 /21 /10 | 9,12,15,6 | equipped Bronze Rod /FREEZE1 | MAGE /ICE_MINOR /PRIORITYMOD_2\|BIT4 |
| Hobgoblin7 | 9 /21 /0 | 10,19,19,5 | equipped Middle Axe /none | REGULAR /0 /PRIORITYMOD_2\|BIT5 |

Archer/Cloud/Witch/Hobgoblin prowess is
CRITICAL150_1IN32/DOUBLE_1IN32/COUNTER_1IN32. Monk instead has
CRITICAL150_1IN4/DOUBLE_1IN16/COUNTER_1IN16. HEALER3 is commandset2, not
spell level3; no HEAL3 is learned. Cloud's hovering/resistance data does not
grant a spell or a second turn. No listed definition has AGI's extra-turn bit.

`InitializeEnemyStats` calls `UpgradeRandomBattleEnemies` before definition
copy. The complete random list is3,8,9,10,14,17,19,21,24,27,35: **12 is absent**.
`IsBattleUpgradable` returns0; its caller restores original d1 from d5 and
exits before `DetermineBattleUpgrade`, effective-level/category/exclusion
processing or upgrade RNG. This holds with512 clear or set and any Bowie level.
Battle10's repeat behavior and airborne exclusion overread are not reached here.
Other round/AI RNG remains independent.

The complete returned definition is copied, HP/MP restored to maxima, X/Y set,
movement merged with row commandset, actual type/regions/orders installed,
then placement item processed. NOTHING exits `InitializeEnemyItems` before
AddItem's mask/equip path. The activation rewrite retains only the definition
priority upper nibble; zero filler/spawn yields`$3000` for Monks and`$2000`
for all others. Definition BIT4/BIT5 is not an initially active region bit.
Difficulty78/79 selects floor(baseATT×5/4) for every non-SUPER value; SUPER2
applies the truncated multiplication twice. Equipment/stat refresh follows.

Iron Arrow46 adds12 ATT/range2–2, Bronze Rod87 adds8/range1–1 and Middle Axe35
adds13/range1–1. Their other two equip effects are NONE0; all are WEAPON with
useSpell NOTHING63. At difficulty0 fresh current ATT is28 Archer,30 Cloud,
27 Monk,19 Witch and25 Hobgoblin. Unarmed Monk/Cloud use the ordinary1–1
physical range. Weapon possession grants no healing item or special loot.
The [item-getter caveat](../grans-opening/battle05-initialization-and-first-control.md#eight-enemies-equipment-and-available-action-operands)
retains its erroneous255 comparison, fallback HEAL and actual type/AI checks.

| Selected spell | MP / range / radius / power | Actual category |
| --- | --- | --- |
| HEAL1, lower available Monk level | 3 /0–1 /0 /15 | TYPE_HEAL, TARGET_TEAMMATES, AFFECTEDBYSILENCE |
| HEAL2, Monk maximum | 5 /0–2 /0 /15 | Same healing properties |
| BLAST1, Monk | 2 /1–1 /0 /6 | TYPE_ATTACK, AFFECTEDBYSILENCE |
| FREEZE1, Witch | 3 /1–2 /0 /10 | TYPE_ATTACK, AFFECTEDBYSILENCE |

Fresh13/10 MP covers these declared costs. Attack-spell selection uses
`GetHighestUsableSpellLevel` to limit known level by current MP; actual type,
confusion, target priority, range and casting-position branches still apply.
Silence-affected spell properties are not a demonstrated silence predicate in
these AI getters, nor proof of an executed effect.

Healing follows a different path. `aiCommand_Heal` rejects Muddle2, checks
HEAL's minimum3 MP and builds living friendly targets with3×HP≤2×maxHP.
`DetermineHealingSpellLevel` returns-1 for missingHP≤2. With Monk's highest
known level2 (`d5=1`), any larger missingHP leaves selected `d2=0`: the higher
threshold path needs known level3. Its MP loop incorrectly shifts by5 and
adds unmasked `d4`, rather than constructing an ordinary packed spell level.
For this selected input, `d2=0` plus `d4.b=HEAL|LV2=$40` looks up HEAL2's
**5 MP** cost, although the returned level is0/HEAL1. Consequently MP3/4 passes
the preliminary3 check but fails this loop; MP≥5 can proceed as HEAL1. The
caller's non-self ≥11 MP level2 promotion also requires known level3+, so does
not promote this Monk. The final entry rewrite, range and position lookup use
HEAL1/range0–1/power15, conditional on those branches succeeding. The helper's
separate level2→1 downgrade is not permission to assume general equivalence;
its known selected operands, not a repaired algorithm, establish this result.
HEAL1/2/3 command parameters are stored/read but unused in that decision.

Fresh enemies start at full HP, so learned HEAL alone
does not supply an injured friendly target. Earlier successful movement/attack
or inactive standby can prevent spell selection. No support spell is learned.

The complete independent `enemyitemdrops.asm` table has no12 entry. Its actual
battle/enemy/item/once-bit/action lookup cannot award a special selected drop
here. Added NOTHING, equipped weapons and table-authorized loot are separate.
Leader-present terminal behavior remains at the existing outcome owner; no
new victory reward or all-enemy HP-zero write is inferred.

## Four ordered regions, fresh arithmetic and two activation paths

**Confirmed:** each12-byte region is type4, byte1=0, four ordered vertices,
then two zero bytes. Persistent region flags are90–93:

| Region / flag | Vertex1 /2 /3 /4 |
| --- | --- |
| 0 /90 | `(16,22)` /`(14,27)` /`(31,27)` /`(31,17)` |
| 1 /91 | `(7,5)` /`(7,11)` /`(21,19)` /`(21,5)` |
| 2 /92 | `(7,0)` /`(7,19)` /`(21,19)` /`(21,0)` |
| 3 /93 | `(14,0)` /`(14,16)` /`(31,16)` /`(31,0)` |

`IsRegionTriggered` tries triangles1/2/4 then3/2/4. It scans all30 allies with
positive signed HP and nonnegative signed-byte X/Y, independently of joined,
active and follower flags. `IsAllyInTriangle` tests edges a→b, c→a, c→b
against opposite c,b,a. `CheckTestPointPositionRelativeToSegment` chooses
horizontal dominance on equal absolute spans and uses signed MULS/DIVS,
truncating toward zero. For edge A→B and test C:

- Horizontal: `Yproj = By + trunc((By−Ay)×(Cx−Bx)/(Bx−Ax))`.
- Vertical: `Xproj = Bx + trunc((Bx−Ax)×(Cy−By)/(By−Ay))`.

Compare the projection with the actual other coordinate. Smaller produces10,
larger00; equality is01 only within the segment's dominant-axis bounds,
otherwise10. Negative projection writes CCR=1 and produces10 despite the
misleading11 comment. Degenerate fallback has its distinct CCR behavior;
no selected edge is degenerate. In-bounds01 accepts immediately. Otherwise
ordinary flags must match the opposite vertex; bit0 exceptions skip the first
two edges but reject at the third. Cross-product geometry cannot substitute
for these instructions.

All12 fresh positions were substituted into both triangles of all four regions.
The first rejection for every position in each triangle is:

| Region | Triangle1/2/4 | Triangle3/2/4 |
| --- | --- | --- |
| 0 | Edge1→2, vertical Xproj15–16 > allyX1–5 gives00; opposite4 X31 versus projection18 gives10. | Third edge4→2, horizontal Yproj32–34 > allyY21–23 gives00; opposite3 Y27 versus projection17 gives10. |
| 1 | Edge1→2, Xproj7 > allyX gives00; opposite4 X21 versus7 gives10. | First edge3→2, Yproj8–10 < allyY gives10; opposite4 Y5 versus19 gives00. |
| 2 | Edge1→2, Xproj7 > allyX gives00; opposite4 X21 versus7 gives10. | First edge3→2, Yproj19 < allyY gives10; opposite4 Y0 versus19 gives00. |
| 3 | Edge1→2, Xproj14 > allyX gives00; opposite4 X31 versus14 gives10. | First edge3→2, Yproj16 < allyY gives10; opposite4 Y0 versus16 gives00. |

None is collinear in bounds. Actual fresh living placements use a prefix/subset
of these coordinates; unplaced X resets to-1. Thus no fresh90–93 trigger
follows from these48 region-position/96 triangle substitutions. This proves
neither later crossings nor saved-state results nor general geometric equivalence.

`ActivateEnemies` scans128–159 then, because the outer RTS is absent, falls
through once for160. Negative X, HP0 or nonenemy skips testing; the
[extra160/deals alias owner](../northern-travel-mitula/battle26-activation-and-first-turn-dispatch.md#the-extra-fall-through-and-its-selected-ram-alias)
retains that conditional defect. NEWLY_TRIGGERED_BATTLE_REGIONS is a tested-word
cache including misses, separate from persistent90–105. Fresh region reset and
`ClearAiMemory` do not explicitly clear that word; non-swarm `StartAiControl`
later clears it. Incoming tested bits can suppress tests. With incoming bits
clear, this formation produces tested15 and no90–93; arbitrary saved flags/cache
and positions are different inputs.

| Assigned enemies | Primary hit | Secondary hit when primary misses |
| --- | --- | --- |
| Cloud129, regions1/3 | F91 sets activation bit0, returns before secondary. | F93 sets bits0/1. |
| Witch131, Hobgoblin137, regions0/1 | F90 sets bit0, returns before secondary. | F91 sets bits0/1. |
| Monk132/136, Hobgoblin133, regions2/15 | F92 sets bit0. | No secondary polygon15 is tested. |
| 128/130/134/135/138/139, regions15/15 | Assigned-region activation returns without a write. | Both sentinels bypass inactive-region standby in AI preparation. |

Testing still scans **all four** declared regions before assigned activation,
even for a15/15 enemy. Persistent hits and activation bits are sticky: neither
miss clears them; a later primary hit preserves an already-set secondary bit.
If both flags are set on a fresh word the primary path wins, so only bit0 is
set. Activation does not swap stored primary/secondary orders. Every selected
dual-region enemy has NONE/NONE anyway; no alternate destination follows from
its secondary activation.

## Standby, ordered commands, round and conditional first control

**Confirmed:** the region-program table has only32/40, not12. All selected
spawn modes are0 and no neutral is declared; there is no selected hidden/
respawn candidate. With fresh persistent flags clear,129/131/132/133/136/137
take inactive standby, while the six15/15 enemies enter their commandsets.
All six inactive enemies have NONE orders. `ValidateAiStandbyEligibility`
returns d1=0/d2=0; if the earlier thinking RNG stop is avoided, standby anchors
at actual starting position and conditionally constructs local movement.
Nonzero d1 would stop with an empty move string, contrary to the permission
comment. **NONE standby does not mean no possible movement**. Its candidate,
occupancy, terrain, memory and RNG branches must succeed; action STAY can follow
movement. No guaranteed first advance or cast follows.

Commands are attempted in order until d1=0 success; nonzero advances the stream:

| Set / actual users | Complete stream / initial pathfinding mode |
| --- | --- |
| HEALER1/0 /132,136; also ally templates only | HEAL1, SUPPORT, ATTACK1, MOVE1, STAY /regular |
| HEALER3/2 /130 | HEAL2, MOVE_ORDER4, SUPPORT, ATTACK3, MOVE2, STAY /block-non-movable |
| ATTACKER1/6 /129,133–135,138,139; actual AI allies independently | ATTACK1, HEAL1, SUPPORT, MOVE1, STAY /regular |
| ATTACKER2/7 /131,137 | MOVE_ORDER1, ATTACK1, HEAL1, SUPPORT, MOVE1, STAY /regular |
| LEADER14 /128 | MOVE_ORDER5, ATTACK2, HEAL3, SUPPORT, MOVE_ORDER2, MOVE3, STAY /block-and-carve |

MOVE_ORDER1 uses target type0/regular,4 type0/block-non-movable,5 type0/
block-and-carve and2 follow type2/block-and-carve. The stored bit6 still
selects fixed point versus live combatant. Thus Monk130's MOVE_ORDER4 can
resolve live128 despite its target-type label; the command name does not
turn its `$80` into fixed point0. Archer128 uses fixed22,8 even in its follow-
type fallback. `StartAiControl` first promotes a dead primary follow target
to secondary NONE and clears secondary: here only130 follows128;128's MOVE_TO
bypasses the HP test. Region activation itself does no such promotion.

`aiCommand_MoveOrder` rejects missing orders, zero MOV or a dead live referent.
Its point path tests the returned X with `tst.b`, not the-1 failure sentinel;
the follow path has terrain restrictions. Conditional attack success ends the
command; otherwise move-string construction can return STAY with movement.
Empty movement can preserve a nonzero result and permit the next command.
Temporary terrain restrictions are reapplied and eventually cleared.
Sets7 on131/137 have NONE: MOVE_ORDER1 returns d1=-1 with empty move string/STAY,
then ATTACK1 and the rest are attempted once activated. Sets0/6 contain **no
MOVE_ORDER**; sets0 can try Monk healing but do not follow point0 or128.
Ally AI selects6 independently of the placement's HEALER1. HEAL1/2/3 command
parameters do not select learned spell levels; this healing body leaves them
unused after storing/reading them.

Round prefix is activation → sparse region program → spawn selection → turn
order. The [shared round rule](../grans-opening/battle02-initialization-and-first-control.md#first-round-and-conditional-control-endpoint)
clears64 sentinel records, scans30 allies then the first30 enemy slots, admits
living placed actors independently of activation and uses refreshed AGI/main
RNG. All twelve enemies qualify with AGIs20,17,18,15,18,19,15,19,18,19,19,19;
none has an extra-turn bit. Ordinary score is
`AGI + RNG(AGI>>3) − RNG(AGI>>3) + RNG(3) −1`.
The extra-turn bit has its separate two-draw extra score; range0 still advances
the seed.62 signed-byte stable-sort passes preserve append-order ties; cursor
becomes0. No upgrade draws precede this here. Standby uses separate thinking
RNG. Actual allies and supplied seeds still determine the first actor.

A sentinel starts another round; HP0 actor skips. `ExecuteIndividualTurn`
gives MUDDLE/AI bit2 priority to AI preparation. Otherwise an enemy with
**opponent-control=0** uses AI and nonzero reaches player preparation; ally
auto-battle independently chooses AI. Shared cursor/view/windows/grid work
follows; SLEEP/STUN select NoAction before execution. Dispatch repeats MUDDLE/
bit2/control polarity before AI execution or player input. Stop at this
conditional boundary, before a performed action. F88 can instead supply a
saved queue/cursor. No fixed first actor, player-ready state or first action
is established, and the leader terminal is not run merely by initialization.

## Accepted production consumers and conversion limits

**Confirmed by accepted implementation inspection, separate from original-game
evidence:** Battle12 is not admitted/converted/supported by the current private
route. [Trust](../../../../remake/docs/content/profiles-and-trust.md#selected-third-encounter)
and [Application](../../../../remake/docs/application/session-and-programs.md#yeel-guide-zone-request-and-sleep-continuation)
retain selected2/3 consumption. No preparation, comparison, allowlist change or
engine execution is performed. New enemy names alone do not imply missing
mechanisms; inspect the actual operands and rejection boundaries:

| Actual accepted consumer | Reusable mechanism and specific Battle12 boundary |
| --- | --- |
| [prepare_encounter / compile_routes](../../../../src/sf2tool/remake_exploration_battles.py) | Producer/CLI admits2/3 only. Wildcard Map52 does not need the coordinate-trigger extension required by11, but there is no12 export/route. Hook/load and400/450/500 flag mechanisms exist. |
| [PrivateBattleEncounterReader](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs) / [PrivateBattleDefinitionReader](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleDefinitionReader.cs) | Placement/region/point/metadata formats parse, while trusted encounter pins admit1/2/3, not12. Lookup selects declared bases and referenced items/spells without a name-specific prohibition; Death Archer/Cloud/Monk descriptive records need trusted selection/conversion. No repeat substitution is required by this original12 path. |
| [PrivateBattleScenarioReader.Assemble](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs) / [PrivateBattleActionBindings](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleActionBindings.cs) | Independent region-program admission1/2/3 rejects12. STARTING/NOTHING, sets0/6/7/14, point/live-follow/NONE, Regular/Healer/Hovering already bind. HEALER3/2 rejects at source-commandset. Archer/Witch BIT4 fails AI-word regex; ARCHER/MAGE reject at mover binding; Iron Arrow range2–2 rejects physical binding. Monk critical1IN4/double-counter1IN16 rejects prowess. Hobgoblin/Middle Axe and Cloud's ordinary adjacent physical metadata fit existing binding; this does not implement Cloud resistance effects. HEAL data can bind, but BLAST/FREEZE execution is absent. Controlled classes0/1/2/4 omit Peter/arbitrary natural parties. |
| [BattleEntry](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs) | Before/load/start flags, membership rebuilding and carried resources/growth/gold/seeds exist; missing member definitions reject. F88 rejects instead of restoring saved queue/cursor. A map scene's Peter mapping is not an admitted battle member definition. |
| [BattleInitializationRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleInitializationRules.cs) / [BattleTurnFlow](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleTurnFlow.cs) | Controlled already-refreshed allies, roster-only/difficulty0, living/immortal healing, formation/activation/ATT exist. Excess active tail is capacity rejection rather than the source's eviction. Enemy loadouts admit0/127, so equipped Iron Arrow/Bronze Rod/Middle Axe reject. General equipment/CURSE refresh, other difficulty/control policies and saved restoration remain Unsupported. |
| [BattleActivationRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActivationRules.cs) / [TurnOrderRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/TurnOrderRules.cs) | Mode0/RegionProgram.None, living placed allies, primary-first/sticky-secondary and randomized/extra turns exist. Dual-region activation needs no new mechanism. Cross-product geometry/fresh per-round tested word does not prove original integer projection/cache equivalence. No Battle12 observation validates the composition. |
| [BattleControlRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleControlRules.cs) / [SourceEnemyAi](../../../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceEnemyAi.cs) / [SourceMoveOrderAi](../../../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceMoveOrderAi.cs) | Unsupported status/automatic-ally/control policies remain exposed; enemy AI rejects ally actors, so Peter AI is not production support. Standby/NONE/15/15, sets6/7/14, point/live-follow and dead-target promotion exist. Activated AI admits only6/7/9/14 and0/127/no learned spells: Monk sets0/2 and healing/BLAST, Witch spell/gear and equipped attackers reject. Cloud's spell-free set6/hovering physical path is expressible; resistance-aware attack/spell behavior is not supplied by descriptive metadata. No general ranged/attack/support/healing-spell AI follows from shared station search. |

These are explicit requirements/Unsupported boundaries at actual consumers,
not an implementation task or acceptance of engine defects. Declaration,
conversion, successful consumption and natural observation remain separate.
Remake code proves no original-game fact. Existing scene/graphics-service and
leader-outcome consumer limits remain at their owning admission/outcome accounts.

## Unknowns and reproduction boundary

**Unknown:** natural route/flags and joined/active/dead/capacity/follower history,
Peter/May deployment/AI, classes/levels/status/gear, difficulty/seeds, later
region crossings, first actor/action/targets/tactics, complete action behavior,
service/presentation completion, continuous traversal, hardware timing and
durable saves. Static no-substitution identifies no natural seed or turn.
No Unknown becomes an automatic capture or repair queue.

Accepted Battle01 admission/victory/stable-return/later-Down observations remain.
#1047's twelve **completed unresolved comparisons** remain at the
[verification owner](../../../../remake/docs/verification/field-programs.md#yeel-guide-through-battle03),
not interrupted runs or passes. Preserve intermediate status versus equipment-
derived CURSE. PR1221's mechanical PASS missed stale coverage/denominator and
opponent-control polarity; its **two P2** findings remain in the
[handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1220#issuecomment-6354981415).
Mechanical document checks do not replace source/consumer semantic review.

Reproduce from all24 combatant rows, four ordered regions/one point and the
five full definitions at the pin. Trace actual membership and scene replacement
separately; follow the upgrade entry's early return, NOTHING item return,
difficulty/equipment refresh, independent drop table, tested cache/persistent
flags, primary/secondary activation, standby polarity and all five commandsets
through turn dispatch. Apply the projection formulas and edge order above to
each of the12 positions in each triangle; retain the first rejection table.
Inspect each named production admission/rejection body directly. This provides
reproducible operands and expected results without depending on an ignored helper.

This is linear selected reading plus48 region-position/96 triangle substitutions,
one small arithmetic process and bounded text/receipts, estimated below64 MiB
and one minute. The arithmetic completed in0.008 seconds; peak memory was not
measured. It does not parse or regenerate the retained map payloads. Those two
payloads total18,285,979 bytes and `local/issue1005/pair.txt` stay unchanged;
length/mtime checks reuse retained metadata. New methods/receipts live only in
`local/issue1234/`, preserving old results. Replan for a concrete required
cross-system or acquisition expansion, not every Unknown.

Direct checks cover UTF-8/LF, relative links/anchors separately from external
references, exact three-path `origin/main` diff including committed head,
private/ownership, unchanged fixture counters/four frontiers and retained input
metadata. On a clean committed head load ignored selections in the same process:

```powershell
. ./local/private-inputs.ps1
uv run sf2 verify plan --base origin/main --head HEAD
```

Record exact-head scoped CI, completed failures and process status in the Issue
handoff. No normal/full, SDK/Godot/emulator/H3/H4, screenshots, new extraction,
comparison/lowering or observation is selected. Stop at clean pushed frozen
Draft PR, Review / Verify, stopped writer/processes and successful notification
to assigned main-gate. Independent acceptance/merge, cleanup and another slice
remain outside this delivery.
