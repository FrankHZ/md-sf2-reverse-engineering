# Battle11 initialization, regions and first conditional control

Evidence date: 2026-10-22. **Confirmed:** the complete selected12/10/4/1
declaration connects to actual party placement, base enemy refresh, four ordered
regions and conditional player/AI/no-action dispatch. Battle11 is absent from
the random-upgrade list: completion511 does not select substitute definitions.
**Inferred:** this is the intended cave encounter on the eastern continuation.
**Unknown:** natural party/state, first actor/actions, darkness presentation,
service timing and persistence. Static composition is not observed play or
engine-ready content.

## Agreement and provenance

This is the Standard-rigor agreement for
[Issue1232](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1232),
within [Epic1053's finite scope](./story-content-coverage.md#scope-and-reading-rules).
The outcome is content maintenance and future conversion review: connect every
selected operand to its actual consumer through first conditional control.
Party, map-entity, battle-state and AI boundaries justify Standard rigor.
The [admission owner](./east-shrine-return-through-battle11-admission.md) already
owns the route, complete34-record/seven-text opening, entity mappings and Plane-B
helper. The [outcome owner](./battle11-outcomes-through-polca-arrival.md) owns
terminal/retreat returns and the Polca connection. Reuse both; neither cave
crossing, another opening transcript nor a darkness-rendering audit is added.
Shared lifecycles remain at [Battle02](./battle02-initialization-and-first-control.md),
[Battle09](./battle09-initialization-and-first-control.md) and
[Battle10](./battle10-initialization-and-first-control.md).

Owned writes are this document and Battle11 consistency in the two coverage
owners. Other code/documents, fixtures/schemas/registries/counters and runtime
frontiers are read-only. No parser, platform, allowlist, implementation,
acquisition, rebuild, comparison or lowering is added. Acceptance is complete
selected source/consumer review, direct document/private/scope checks, clean
committed planner and exact-head scoped CI, then a frozen pushed Draft PR for
independent main-gate acceptance. PR692 is unconsumed.

Original paths below resolve under `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at that pin; the
[USA baseline](../../README.md#pinned-original-baseline) retains input identity.
Accepted repository base is `6784311f3de85eb8610aa8e03063efaf6aef3729`, tree
`eaa512035b6fa5c1535247fa932c9e2c5c8f54d3`. No fresh ROM-equivalence or runtime
claim is made. Short battle code paths below have prefix
`code/gameflow/battle/`; sibling filenames retain the preceding directory.
`j_` interfaces resolve to their named implementation bodies. Instructions,
returned registers and branch polarity control over comments or macro names.

| Claim | Exact declaration and consuming boundary |
| --- | --- |
| Complete12/10/4/1 | `data/battles/spritesets/entries.asm` slot11 → `spritesets/spriteset11.asm:BattleSpriteset11`, `0x1B3BF2..0x1B3D30`; `sf2macros.asm:allyCombatant/enemyCombatant/combatantAiAndItem/combatantBehavior`, `sf2enums.asm`; `getbattlespritesetsubsection.asm:GetBattleSpritesetSubsection`, `0x1B1630..0x1B169E`. |
| Admission/globals/lifetimes | `code/common/maps/getbattle.asm:CheckBattle`, `0x799C`, `code/gameflow/mainloop.asm`; row11 of `data/battles/global/battlemapcoords.asm`, `custombackgrounds.asm`, `enemyleaderpresence.asm`; complete `halvedexpearnedbattles.asm`, `afterbattlepositions.asm`, `battleneutralentities.asm`; `data/battles/terrainentries.asm` slot11 → `battleloop/loadbattleterraindata.asm`; `battleloop_1.asm:BattleLoop`, `0x23A84`, before/start wrappers and row11 of `data/battles/cutscenes/beforebattlecutscenes.asm`, `battlestartcutscenes.asm`; `data/battles/entries/battle11/cs_beforebattle.asm:bbcs_11`, `0x4ABFE`. |
| Actual party/scene replacement | `battleloop/heallivingandimmortalallies.asm`, complete `initializecombatants.asm`, `0x1B1272..0x1B15F8`; `code/common/stats/battleparty.asm`, `updatecombatantstats.asm`; `battlefunctions/loadBattle.asm` and `code/common/scripting/entity/entityfunctions_1.asm:PositionBattleEntities`; admission owner's map clear/follower/init and script consumers. |
| Definitions/upgrade/gear/actions | `data/stats/enemies/enemydefs.asm` records6/7/41/57/68; `battleloop/upgradeenemies.asm:UpgradeRandomBattleEnemies/IsBattleUpgradable`, `0x1B1780..0x1B1A28`; complete `data/battles/global/randombattles.asm`, `0x1B6DB0..0x1B6DDA`; `data/stats/items/itemdefs.asm` records33/35/46/87, `data/stats/spells/spelldefs.asm` BLAZE/FREEZE; `code/common/stats/itemstats.asm`, `ai/getnextuseableaiaction.asm`, `gethighestusablespelllevel.asm`, `command/attack.asm`, `command/heal.asm`, `command/support.asm`; complete `data/battles/global/enemyitemdrops.asm` → `battleactions/dropenemyitem.asm`. |
| Regions/standby/orders | Complete `battleloop/activateenemies.asm`, `triggerregions.asm`, `0x1AC9FC..0x1ACF30`, `clearaimemory.asm`, `populateenemyspawns.asm`; `data/battles/cutscenes/regionactivatedcutscenes.asm`; `code/common/stats/combatantstats_1.asm:GetMoveOrders/GetTriggerRegions`, `combatantstats_2.asm:SetTriggerRegions`; `battlefield/getmoveorderposition.asm`, `0x1B15F8`; `ai/startaicontrol.asm`, both `determineaistandbymovement_*.asm`, `executeaicommand.asm`, `command/moveorder.asm`, `command/move.asm`; `data/battles/global/aicommandsets.asm`, `0xE1AC..0xE25B`. |
| Round/control | `battleloop/turnorderfunctions.asm:GenerateBattleTurnOrder`, `battlefunctions/executeindividualturn.asm:ExecuteIndividualTurn`, `0x23EB0..0x24242`; actual status/activation/control branches and main versus thinking RNG consumers. |

The accepted [spriteset](./battle-spriteset-data.md),
[routing](./battle-routing-data.md), [global](./battle-global-data.md),
[loop](./battle-loop.md), [AI](./battle-ai.md) and
[cutscene](./battle-cutscenes.md) inventories remain unchanged.
`rowContentTracked:false` and `routeTargetsParsed:false` retain their existing
executable limits; these prose relationships do not create an executable Battle11
composition or change fixture counts.

## Selection, globals and entry lifetimes

**Confirmed:** `BATTLE_CAVE_OF_DARKNESS=11` selects Map48 crop`(0,0,27,32)`
only at exact trigger`(15,26)` with F411 set. The ordered table's other Map48
row is25, crop`(28,0,32,32)`, trigger`(55,29)`, unlock425/completion525.
Row11 is tested first; at15,26 row25 cannot substitute even with425 set.
Set511 clears411 on a qualifying row11 but still selects11 on that call.
Clear411 selects neither at15,26. Rectangle fields are outputs, not containment
tests. Intro461, completion511 and suspension88 are independent supplied states.
The [F411 producer and missing Map48 setup](./east-shrine-return-through-battle11-admission.md#cave-admission-f411-and-the-no-battle-alternative)
remain with the admission owner. May, Peter, old-man following, a shrine visit
and Battle10 completion are not extra predicates.

Terrain slot11 selects `BattleTerrain11`, Stack-decoded into the shared48×48
array, distinct from the27×32 operating crop. Background11 is CAVE10, a fixed
selection consumed by `GetBattlesceneBackground`; it does not prove darkness or
current pixels. Leader-presence11 is0. `LEADER` on Hobgoblin128 is commandset14,
not authority for a leader-death sweep or special victory condition. The half-EXP
list contains only1, after-position overrides only5, and the complete neutral
table has no11 row: their consumers add no selected override or neutral.

| Supplied state | Actual entry sequence |
| --- | --- |
| F88 clear/F461 clear | Clear PLAYER_TYPE, set399; clear seconds; save/install map/battle/base VInts; run before11; restore/reassert map/battle; clear90–105; heal; initialize actual allies and ten enemies; clear AI memory; `LoadBattle(0)`; start wrapper sets461 and runs `ms_Empty`; prepare round. |
| F88 clear/F461 set | Both hook bodies skip; fresh healing/placement/base refresh/load and new round still run.511 is not461. |
| F88 set | Restore saved seconds; clear88/AI memory/view target; load using incoming focus; enter retained queue/cursor. No fresh map/battle installation, hook, healing, placement, region reset, definition refresh or new round precedes that loop. A reached sentinel later requests a round. |

The before program contains no join/active/force-AI/EGRESS_MAP/story write; its
camera/music/actions and graphics helper do not repair party or stored egress.
The empty start still reaches the script terminator's conditional scrolling wait
and speed clear. Stored EGRESS_MAP is consumed through the shared savepoint/F399
route; no fixed cave egress tuple follows. Natural service completion, saved-state
integrity and the supplied egress remain Unknown.

## Scene population versus twelve party templates

**Confirmed:** header4 +22 combatants×12 +4 regions×12 +1 point×2 =318
(`0x13E`), matching `0x1B3BF2..0x1B3D30` with an exclusive end. The getter
uses the declared counts/strides. Every ally row has HEALER1/0, item NOTHING127,
primary NONE255/region15, secondary NONE255/region15, filler0 and STARTING0.
All template positions in order are:

| Template | Local X,Y | Template | Local X,Y |
| --- | --- | --- | --- |
| 0 | `(15,23)` | 6 | `(13,24)` |
| 1 | `(14,23)` | 7 | `(16,24)` |
| 2 | `(14,24)` | 8 | `(12,24)` |
| 3 | `(13,23)` | 9 | `(16,22)` |
| 4 | `(16,23)` | 10 | `(12,23)` |
| 5 | `(15,24)` | 11 | `(11,23)` |

The [actual membership/cursor rule](./battle02-initialization-and-first-control.md#joined-active-dead-and-capacity)
resets all30 ally Xs, rebuilds ascending joined/active lists and copies only
template X/Y to actual living members. It does not copy template identity/AI/
inventory. Dead ordinary members consume list iterations without advancing the
formation cursor. Twelve iterations bound this encounter; a remaining active
tail goes through `LeaveBattleParty`, losing active/X while retaining joined.
Lists are not rebuilt there. Death-created spare coordinates do not enlarge
capacity. No pre-loop empty-party guard proves a safe zero-party natural entry.

Living allies plus immortal Peter7/Lemon28 are healed regardless of membership;
ordinary dead allies remain dead. Intermediate `status & 7` retains existing
STUN/POISON/CURSE; later `UpdateCombatantStats` preserves STUN/POISON and recomputes
final CURSE from equipped cursed gear. An old CURSE bit alone is not final curse.
Class/level/gear/status/activation and joined/active history remain actual inputs.

The [explicit prebattle list](./east-shrine-return-through-battle11-admission.md#new-battle-wrappers-and-explicit-prebattle-population)
rebuilds main, flag followers, optional fixed Peter and three regular NPCs.
Scene128/129/130 mean Hobgoblin/Bat/Witch, while the battle's128/129/130 mean
Hobgoblin/Witch/Bat. These are separately rebuilt lookups, not ten enemies
derived from three map sprites. `LoadBattle`/`PositionBattleEntities` rebuilds
battle entities from actual combatants; scene coordinates/actions are not the
formation or a retained battle AI state.

Peter's scene row can exist even unjoined/inactive/dead; F67 can instead supply
his earlier follower mapping and skip that fixed row. His F7 joined, conditional
F39 active, force-AI bit2 and F67 follower remain independent, as traced by
[Battle09](./battle09-initialization-and-first-control.md#twelve-templates-and-peters-independent-states).
The castle `joinForceAI PETER,$FFFF` consumer sets bit2, but this before program
does not join him or change it. Actual selected immortal Peter heals/places using
the real cursor; a retained AI bit sends his reached turn to ally commandset6.
May F8/F40/HP/AI and her transient following remain separate; no permanent May
follower row places her here. Old man F77→FOLLOWER_A30 is a map follower identity,
not a battle-party member. None establishes twelve fixed heroes.

## Ten enemies, base definitions and no repeat substitution

**Confirmed:** all ten rows declare added item NOTHING127, secondary NONE255/
region15, filler0 and STARTING0. Slots128–137 initialize immediately without
the spawning-position occupancy gate; undeclared138–159 have current/maxHP0
and X=-1, not hidden live additions. Complete row differences:

| Row / slot | Base type / local X,Y | Commandset | Primary order / region |
| --- | --- | --- | --- |
| 0 /128 | HOBGOBLIN7 `(15,4)` | LEADER14 | MOVE_TO\|0 (`$40`) /15 |
| 1 /129 | WITCH68 `(16,5)` | ATTACKER3/9 | FOLLOW_ENEMY\|0 (`$80`) /15 |
| 2 /130 | VAMPIRE_BAT41 `(17,3)` | ATTACKER1/6 | NONE /2 |
| 3 /131 | VAMPIRE_BAT41 `(5,7)` | ATTACKER2/7 | NONE /15 |
| 4 /132 | HUNTER_GOBLIN57 `(12,14)` | ATTACKER1/6 | FOLLOW_TARGET\|0 (`$00`) /0 |
| 5 /133 | DARK_DWARF6 `(13,15)` | ATTACKER1/6 | FOLLOW_TARGET\|0 /0 |
| 6 /134 | DARK_DWARF6 `(8,16)` | ATTACKER1/6 | FOLLOW_TARGET\|0 /0 |
| 7 /135 | WITCH68 `(18,10)` | ATTACKER1/6 | NONE /1 |
| 8 /136 | HUNTER_GOBLIN57 `(20,9)` | ATTACKER1/6 | NONE /1 |
| 9 /137 | VAMPIRE_BAT41 `(11,4)` | ATTACKER1/6 | NONE /3 |

Point0 is fixed`(15,4)`. It initially equals128's position but is not live128:
`GetMoveOrderPosition` tests bit6 and reads the point subsection for MOVE_TO;
otherwise it reads the referenced combatant's current X/Y. Witch129's `$80`
therefore follows live enemy128, and132–134's `$00` refers to live ally0.
Neither means scene128, template0 or an automatic move to the starting point.

Each full definition is56 bytes. All five have spellPower REGULAR, initialStatus
NONE and priority flags PRIORITYMOD_2. Four item/spell slots each are accounted
for: everything outside the listed slot0 entries is NOTHING.

| Type | Level / HP / MP | ATT,DEF,AGI,MOV | Slot0 gear / spell | Movement / resistance / other AI flags |
| --- | --- | --- | --- | --- | --- |
| Hobgoblin7 | 9 /21 /0 | 10,19,19,5 | equipped Middle Axe /none | REGULAR /0 /BIT5 |
| Dark Dwarf6 | 7 /21 /0 | 18,17,18,4 | equipped Short Axe /none | REGULAR /0 /BIT5 |
| Hunter Goblin57 | 7 /24 /0 | 8,12,14,5 | equipped Iron Arrow /none | STEALTH_ARCHER /0 /BIT4 |
| Witch68 | 7 /21 /10 | 9,12,15,6 | equipped Bronze Rod /FREEZE1 | MAGE /ICE_MINOR /BIT4 |
| Vampire Bat41 | 7 /20 /8 | 21,16,16,6 | none /BLAZE2 | FLYING /0 /none |

Hobgoblin/Dwarf/Witch prowess is CRITICAL150_1IN32/DOUBLE_1IN32/COUNTER_1IN32;
Hunter doubles/counters are1IN16; Bat has CRITICAL_POISON with1IN32 double/counter.
These operands differ from a universal ordinary physical-action rule.

Initialization calls `UpgradeRandomBattleEnemies` before definition copy. The
complete random list is3,8,9,10,14,17,19,21,24,27,35; **11 is absent**.
`IsBattleUpgradable` returns0; the caller restores original d1 from d5 and exits
before `DetermineBattleUpgrade`, effective-level/category/exclusion processing or
upgrade RNG. This holds with F511 clear or set and regardless of Bowie's level.
No Battle11 airborne exclusion overread follows from this path. The shared
defect remains recorded for admitted repeat encounters such as Battle10; its
existence does not substitute Bat definitions here. Other round/AI RNG still runs.

The returned base definition is copied completely, HP/MP restored to maxima,
X/Y installed, definition movement merged with the row commandset, actual enemy
index/regions/orders written, then placement item processed. NOTHING immediately
returns from `InitializeEnemyItems`: no AddItem mask or extra loot is reached.
Only activation's priority upper nibble survives; zero filler/spawn yields
initial word`$2000`, not definition BIT4/BIT5 as initially active region bits.
Difficulty78/79 selects floor(baseATT×5/4) for every non-SUPER value; SUPER2
applies that truncated multiplication twice. Equipment/stat refresh follows.

Middle Axe35 adds13 ATT with range1–1; Short Axe33 adds5/range1–1;
Iron Arrow46 adds12/range2–2; Bronze Rod87 adds8/range1–1. At difficulty0 the
fresh current ATT is25 Hobgoblin,27 Dwarf,22 Hunter,19 Witch and26 Bat.
All are WEAPON/useSpell NOTHING63; the
[item-getter caveat](./battle05-initialization-and-first-control.md#eight-enemies-equipment-and-available-action-operands)
retains its erroneous255 comparison, fallback HEAL and actual type/AI checks.
Weapon declarations do not grant healing items. FREEZE1 is silence-affected
TYPE_ATTACK, MP3/range1–2/radius0/power10; BLAZE2 is MP6/range1–2/radius1/power10.
Their base10/8 MP covers those costs, but actual getters still need known,
affordable, confusion-eligible levels, targets/priorities and casting positions.
Earlier successful commands or standby can prevent reaching spell selection.
HEAL/SUPPORT command names create no learned healing/support spell.

The independent complete enemy-drop table has no11 entry. Its actual battle/
enemy/item/once-bit lookup cannot award special Hobgoblin/LEADER loot here.
Definition weapon possession, an added NOTHING and a table-authorized drop are
distinct. The existing outcome owner retains ordinary elimination/rewards/return;
neither a leader-only terminal nor a special battle victory item is invented.

## Four ordered regions and fresh coordinates

**Confirmed:** each region is type4, byte1=0, four ordered vertices and two
trailing zero bytes. It consumes12 bytes. Persistent flags are90–93:

| Region / flag | Vertex1 /2 /3 /4 |
| --- | --- |
| 0 /90 | `(0,0)` /`(0,21)` /`(25,21)` /`(25,0)` |
| 1 /91 | `(0,0)` /`(0,16)` /`(26,16)` /`(26,0)` |
| 2 /92 | `(0,0)` /`(0,13)` /`(26,13)` /`(26,0)` |
| 3 /93 | `(0,0)` /`(0,11)` /`(17,11)` /`(26,0)` |

`IsRegionTriggered` tries triangles1/2/4 then3/2/4. It scans all30 allies with
positive signed HP and nonnegative signed-byte X/Y, independently of joined/
active/follower flags. `CheckTestPointPositionRelativeToSegment` uses signed
MULS/DIVS truncated toward zero, horizontal dominance on equal spans, integer
projection versus actual coordinate, bounded collinearity and synthesized Z/C.
In-bounds01 accepts immediately; ordinary side flags must match the opposite
vertex per edge. Bit0 exceptions skip early edges but reject on the third.
Negative projection writes CCR=1 and produces10 despite the misleading11 comment.
Cross-product geometry is not a substitute for those instructions.

All12 fresh positions were substituted into both triangles of all four regions:

| Region | Triangle1/2/4 first rejection | Triangle3/2/4 first rejection |
| --- | --- | --- |
| 0 | Third edge4→2: Yproj8–12 < allyY22–24; ally10 versus vertex1's00 (Yproj21 >0). | First edge3→2: Yproj21 < allyY22–24; ally10 versus vertex4's00 (21 >0). |
| 1 | Third edge4→2: Yproj7–10; ally10 versus vertex1's00 (16 >0). | First edge3→2: Yproj16; ally10 versus vertex4's00 (16 >0). |
| 2 | Third edge4→2: Yproj5–8; ally10 versus vertex1's00 (13 >0). | First edge3→2: Yproj13; ally10 versus vertex4's00 (13 >0). |
| 3 | Third edge4→2: Yproj5–7; ally10 versus vertex1's00 (11 >0). | First edge3→2: Yproj11; ally10 versus vertex4's00 (11 >0). |

Every listed ally projection is below actualY22–24 in the numeric comparison;
none is collinear in bounds. Fresh living actual placements use a prefix/subset
of these coordinates, while unplaced X resets to-1. Thus no fresh90–93 trigger
follows. These48 region/96 triangle substitutions establish no later crossing,
suspended-state result or general geometric equivalence.

`ActivateEnemies` scans128–159, then missing outer RTS falls through once for160.
Negative X, HP0 or nonenemy skips testing; the
[extra160/deals alias owner](./battle26-activation-and-first-turn-dispatch.md#the-extra-fall-through-and-its-selected-ram-alias)
retains that conditional defect. NEWLY_TRIGGERED_BATTLE_REGIONS is a tested-word
cache, including misses, separate from persistent90–105. Fresh flag reset and
ClearAiMemory do not explicitly clear it; non-swarm `StartAiControl` later does.
Incoming tested bits can suppress tests. With incoming bits clear, these fresh
positions produce tested15 and no90–93; arbitrary saved state is different.

Assigned primaries are0 for132–134,1 for135/136,2 for130 and3 for137; every
secondary is15. Primary hit sets bit0 and returns, preserving existing bit1;
only a secondary hit sets bits0/1. No region15 is declared.128/129/131 have15/15
and bypass inactive-region standby. Activation alone does not swap orders.
On the activated path a dead follow target's HP0 promotes secondary NONE and
clears secondary:129 for128,132–134 for ally0.128's MOVE_TO bypasses that HP test.

## Standby, command streams, round and first control

**Confirmed:** the region program table contains only32/40, not11. All spawn
modes are0 and the selected neutral table adds none; no hidden/respawn candidate
or eleventh combat enemy follows. With fresh region flags clear:

| Enemies | Actual standby or unconditional path |
| --- | --- |
| 132–134: assigned region0 plus follow ally0 | `ValidateAiStandbyEligibility` returns nonzero d1; caller stops with empty move string/STAY. The misleading permission comment does not reverse that branch. Earlier thinking RNG can also stop. |
| 130/135/136/137: assigned region plus NONE | Returns d1=0/d2=0; if earlier RNG stop is avoided, anchor at actual starting position and conditionally construct local movement. STAY need not mean no movement. |
| 128/129/131:15/15 | Enter actual commandset regardless of region activation bit0. |

Streams are attempted in order until a command returns success:

| Set / enemies | Commands / initial pathfinding mode |
| --- | --- |
| LEADER14 /128 | MOVE_ORDER5, ATTACK2, HEAL3, SUPPORT, MOVE_ORDER2, MOVE3, STAY /block-and-carve |
| ATTACKER3/9 /129 | MOVE_ORDER3, ATTACK3, HEAL2, SUPPORT, MOVE_ORDER4, MOVE2, STAY /block-non-movable |
| ATTACKER2/7 /131 | MOVE_ORDER1, ATTACK1, HEAL1, SUPPORT, MOVE1, STAY /regular |
| ATTACKER1/6 /130,132–137 | ATTACK1, HEAL1, SUPPORT, MOVE1, STAY /regular |

MOVE_ORDER5 is target type0/block-and-carve,2 follow type2/block-and-carve,
3 follow type1/block-non-movable,4 target type0/block-non-movable and1 target
type0/regular. Stored bit6 still selects fixed point versus live combatant;
the command name alone does not change the reference. Hobgoblin128 uses fixed
point0 in its ordered attempts even though its fallback command is follow-type;
Witch129 resolves live128, subject to HP/terrain/target/path conditions. The
actual commands can fail or return STAY with movement, not guaranteed pursuit.

Bat131's set7 has primary/secondary NONE. `aiCommand_MoveOrder` sets empty move
string/STAY and returns d1=-1 at the NONE branch, so MOVE_ORDER1 fails and
StartAiControl tries ATTACK1 then the remaining stream. It does not follow
point0/ally0 or remain permanently inactive because of ATTACKER2's name.
Set6 contains **no MOVE_ORDER**:132–134's ally0 still affects standby and dead-
target promotion, but does not promise pursuit after activation. MOVE1 uses
opponent scoring/pathfinding; regular mode skips order obstructions. Ally AI
independently uses6; template HEALER1/0's HEAL1,SUPPORT,ATTACK1,MOVE1,STAY is
not installed into an actual ally by formation placement.

Round prefix is activation → region program → spawn selection → turn order.
The [shared round rule](./battle02-initialization-and-first-control.md#first-round-and-conditional-control-endpoint)
clears64 sentinel records, scans30 allies then first30 enemy slots, admits
living placed actors independently of activation and uses refreshed AGI/main
RNG. All ten base enemies qualify with AGIs19,15,16,16,14,18,18,15,14,16;
none has a second-turn bit. Ordinary score is
`AGI + RNG(AGI>>3) − RNG(AGI>>3) + RNG(3) −1`;
extra-turn AGI bit128 has its distinct two-draw extra score.
Range0 still advances the seed.62 signed-byte stable-sort passes preserve
append-order ties; cursor becomes0. No upgrade draws precede this in Battle11;
standby uses separate thinking RNG. Actual allies/seeds determine the queue.

A sentinel requests another round; HP0 actor skips. `ExecuteIndividualTurn`
gives MUDDLE/AI bit2 priority to AI preparation. Otherwise an enemy with
**opponent-control=0** uses AI, nonzero player preparation; ally auto-battle
independently selects AI. Shared cursor/view/windows/grid work follows;
SLEEP/STUN select NoAction before execution. Dispatch repeats MUDDLE/bit2/
control polarity before AI execution or player input. Stop at this conditional
boundary, before a performed action. F88 can instead reach a saved queue/cursor.
Neither a fixed first actor nor an initial player-ready state is established.

## Accepted production consumers and conversion limits

**Confirmed by accepted implementation inspection, separate from original-game
evidence:** Battle11 is not admitted/converted/supported by the current private
route. [Trust](../../remake/docs/content/profiles-and-trust.md#selected-third-encounter)
and [Application](../../remake/docs/application/session-and-programs.md#yeel-guide-zone-request-and-sleep-continuation)
retain selected2/3 consumption. No preparation, comparison, allowlist change or
engine execution is performed. Existing mechanisms are not missing merely
because the complete encounter has no admitted consumer.

| Actual accepted consumer | Reusable mechanism and specific Battle11 boundary |
| --- | --- |
| [prepare_encounter / compile_routes](../../src/sf2tool/remake_exploration_battles.py) | Producer/CLI admits2/3 only; coordinate-triggered route selection rejects. Hook/load and400/450/500 flags exist, but no11 export/route. |
| [PrivateBattleEncounterReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs) / [PrivateBattleDefinitionReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleDefinitionReader.cs) | Placements/regions/points/metadata parse; trusted encounter pins admit1/2/3, not11. Definition lookup selects declared bases and referenced items/spells without a Hobgoblin-specific prohibition. Hobgoblin7/Middle Axe35 descriptive data needs trusted selection/conversion; a new name alone does not imply a missing engine mechanism. No repeat substitution is needed by this original11 path. |
| [PrivateBattleScenarioReader.Assemble](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs) / [PrivateBattleActionBindings](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleActionBindings.cs) | Independent region-program admission1/2/3 rejects11. STARTING/NOTHING, sets0/6/7/9/14, point/live-follow/NONE and Flying already bind. Hobgoblin's REGULAR/ordinary prowess and Middle Axe's adjacent WEAPON/ATT-only effects fit existing descriptive/physical binding. Hunter/Witch BIT4 fails AI-word regex, STEALTH_ARCHER/MAGE movers reject; Hunter range2–2/double-counter and Bat poison prowess reject. Executable spells bind HEAL/healing Herb, not BLAZE/FREEZE. Controlled classes0/1/2/4 omit Peter/arbitrary natural parties. |
| [BattleEntry](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs) | Before/load/start flags, actual membership rebuilding and carried resources/growth/gold/seeds exist; missing member definitions reject. F88 rejects instead of restoring saved queue/cursor. Map scene Peter mapping is not an admitted battle member definition. |
| [BattleInitializationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleInitializationRules.cs) / [BattleTurnFlow](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleTurnFlow.cs) | Controlled already-refreshed allies, roster-only/difficulty0, living/immortal healing, formation and activation/ATT exist. Source's excess-tail eviction is instead capacity rejection. Enemy loadouts admit only0/127, so equipped Middle Axe and other definition weapons still reject even where physical metadata fits. General equipment/CURSE refresh, other difficulty/control policies and saved-state restoration remain Unsupported. |
| [BattleActivationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActivationRules.cs) / [TurnOrderRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/TurnOrderRules.cs) | Mode0/RegionProgram.None, living placed allies, primary-first/sticky-secondary and randomized/extra turns exist. Cross-product geometry/fresh per-round tested word does not prove original integer projection/cache equivalence. No Battle11 observation validates that composition. |
| [BattleControlRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleControlRules.cs) / [SourceEnemyAi](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceEnemyAi.cs) / [SourceMoveOrderAi](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceMoveOrderAi.cs) | Unsupported status/automatic-ally/control policies are exposed; enemy AI rejects ally actors, so original Peter AI is not production support. Standby/NONE/15/15, sets6/7/9/14, point/live-follow, dead-target promotion and set7 NONE fallback exist. Activated AI permits only0/127/no learned spells: Hobgoblin/other weapons, Bat/Witch spells reject. Physical station search does not supply general ranged/attack/support spell AI. |

These are explicit requirements/Unsupported boundaries at actual consumers, not
a new implementation task or engine-defect acceptance. Declaration/conversion
does not equal successful consumption or natural observation. Remake code proves
no original-game fact; existing scene/graphics-service limits stay at admission.

## Unknowns and reproduction boundary

**Unknown:** natural admission/flags, joined/active/dead/capacity/follower history,
May/Peter deployment/AI, classes/levels/status/gear, actual difficulty/seeds,
later region crossings, first actor/action/target/tactics, complete action
behavior, darkness/other presentation, movement/fading/text completion,
continuous traversal, hardware timing and durable saves. Static no-substitution
does not identify a natural seed or first turn. No Unknown becomes an automatic
capture or repair queue.

Accepted Battle01 admission/victory/stable-return/later-Down observations remain.
#1047's twelve **completed unresolved comparisons** remain at the
[verification owner](../../remake/docs/verification/field-programs.md#yeel-guide-through-battle03),
not interrupted runs or passes. Preserve the intermediate status/equipment-CURSE
correction. PR1221's mechanical PASS missed stale coverage summary/denominator
and opponent-control polarity; its **two P2** findings remain in the
[handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1220#issuecomment-6354981415).
Mechanical checks do not replace source/consumer semantic review.

Reproduce by reading all22 combatant rows, four ordered regions/one point, five
complete definitions and the consumers named above at the pin. Trace actual
party placement and scene-to-battle reconstruction separately; follow the real
upgrade entry's early return, NOTHING item return, difficulty/equipment refresh,
independent drop table, activation cache/flags, standby return polarity and all
selected commandsets through turn dispatch. Substitute all12 fresh positions
into both triangles per region with source integer division/CCR/edge order.
Inspect the accepted production admission and rejection bodies directly.

This is linear selected reading plus48 region-position/96 triangle substitutions,
one small arithmetic process and bounded text/receipt output; no bulk run.
The arithmetic/check process is expected below64 MiB and one minute; it does
not parse the retained map payloads. Those two payloads total18,285,979 bytes and
`local/issue1005/pair.txt` stay unchanged. New bounded methods/fresh receipts live
only in `local/issue1232/`, without overwriting old results. No prose line ceiling
is imposed. Replan for a concrete required cross-system or acquisition expansion.

Direct checks cover UTF-8/LF, relative links/anchors separately from external
references, explicit three-path `origin/main` diff including committed head,
private/ownership, unchanged counters/four frontiers and retained metadata.
On clean committed head load the ignored selections in the same process:

```powershell
. ./local/private-inputs.ps1
uv run sf2 verify plan --base origin/main --head HEAD
```

Record exact-head scoped CI and completed failures/process status in the Issue
handoff. No normal/full, SDK/Godot/emulator/H3/H4, screenshots, new extraction,
comparison/lowering or observation is selected. Stop at clean pushed frozen
Draft PR, Review / Verify, stopped writer/processes and successful notification
to the assigned main-gate. Independent acceptance/merge, cleanup and another
slice remain outside this delivery.
