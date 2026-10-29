# Battle19 initialization, Shining Ball custody and first conditional control

Evidence date: 2026-10-23. **Confirmed:** the complete12/15/3/1 selection
connects actual party placement, six full base definitions, F519 repeat selection,
three initially untriggered regions and live orders to first conditional control.
Successful AddItem masks slot129's declared Shining Ball flags away; possession
does not establish automatic use. Its independent action-drop row requires the
item still to be held. High Priest's learned HEAL3 has a different faulty-level
consumer from the earlier HEAL2 case. **Inferred:** this is the intended encounter
outside Elven Village. **Unknown:** natural arrival/state, substitutions, actual
party, first actor/actions, obtained loot, service completion and persistence.
This is static content-conversion guidance, not played or admitted production19.

## Agreement and provenance

This Standard-rigor agreement implements
[Issue1244](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1244)
under [Epic1053's finite scope](../../story-content-coverage.md#scope-and-reading-rules).
Cross-owner party, repeated-definition, activation, healing and inventory
lifetimes justify Standard rigor. The outcome is a source-backed account usable
for future content conversion, ending at conditional individual control before
performed movement/action. Inspect the item action/drop helpers only to explain
their custody predicates; do not simulate a battle, victory or natural loot.

The [Caravan owner](./caravan-boarding-and-southwest-parmecia-travel.md) retains
boarding/travel/landing and walking's packed zone/reload request. The
[outcome owner](./battle19-outcomes-through-elven-village-entry.md) retains the
four empty hooks' outcome tails, village entrance/population and returns.
[Battle17](./battle17-initialization-and-first-control.md) owns shared repeat
selection, [Battle18](./battle18-initialization-and-first-control.md) the prior
LEADER/item/region distinctions, and
[Battle10](../eastern-parmecia/battle10-initialization-and-first-control.md) inventory/drop rules.
Scenes, Rick, Caravan services and Elven Village content are not reopened.

Owned tracked writes are this new primary, Battle19 consistency in
`story-content-coverage.md` and `source-coverage.md`, and the old outcome owner's
narrow intermediate/final-CURSE correction and primary link. No stacked
dependency or competing shared writer exists; PR692 is unconsumed. No source,
fixture, schema, code, allowlist, new importer/comparison platform or runtime
implementation is changed. Acceptance is full selected source/consumer review,
72 finite triangle substitutions, direct document/link/private/four-path checks,
unchanged retained metadata/counters/frontiers, clean committed planner and
exact-head scoped CI. Stop at a clean pushed frozen Draft PR, Review / Verify
and successful main-gate notification; integration remains independent.

Accepted base is `061a764ff6561f8062c6f1c301a43392be760c14`,
tree `66dd84160cb9373b21946d7ea0e8e16f1ab509c4`. Original paths below are
relative to `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at that pin; the
[USA baseline](../../../../README.md#pinned-original-baseline) owns ROM identity.
No fresh ROM-equivalence claim is made. Short battle-code paths use prefix
`code/gameflow/battle/`; sibling filenames keep their directory. Fnnn denotes
flag nnn; indices and template numbers are zero-based. Resolve `j_` aliases to
actual bodies, enum expressions to their declared radix, and comments to opcodes.

| Claim | Pinned declaration and actual consumer |
| --- | --- |
| Selected content | `data/battles/spritesets/entries.asm` slot19 → `spritesets/spriteset19.asm:BattleSpriteset19`, `0x1B46BE..0x1B482C`; `sf2macros.asm` combatant/behavior macros and `sf2enums.asm`; `getbattlespritesetsubsection.asm`, `0x1B1630..0x1B169E`. |
| Admission/globals/hooks | `code/common/maps/getbattle.asm:CheckBattle`, `0x799C`; `code/gameflow/mainloop.asm`; complete `data/battles/global/battlemapcoords.asm`, `custombackgrounds.asm`, `enemyleaderpresence.asm`, `battleneutralentities.asm`, `halvedexpearnedbattles.asm`, `afterbattlepositions.asm`; terrain slot19; four cutscene-table slots19 and their shared wrappers. |
| Actual party and definitions | `battleloop_1.asm:BattleLoop`, `0x23A84`; complete `battleloop/initializecombatants.asm`, `0x1B1272..0x1B15F8`, `heallivingandimmortalallies.asm`; `code/common/stats/battleparty.asm`, `updatecombatantstats.asm`; `battlefunctions/loadBattle.asm` → entity `PositionBattleEntities`; `data/stats/enemies/enemydefs.asm` records11/12/43/60/70/79. |
| Repeat selection | Complete `battleloop/upgradeenemies.asm`, `0x1B1780..0x1B1A28`; `data/battles/global/randombattles.asm`, `0x1B6DB0..0x1B6DDA`; `sf2macros.asm:battles/upgradeRange/excludedEnemies`; `code/common/tech/randomnumbergenerator.asm`. |
| Regions and orders | Complete `battleloop/activateenemies.asm`, `triggerregions.asm`, `0x1AC9FC..0x1ACF30`, `clearaimemory.asm`, `populateenemyspawns.asm`; code/data `cutscenes/regionactivatedcutscenes.asm`; stats `GetMoveOrders/GetTriggerRegions/SetTriggerRegions`; `battlefield/getmoveorderposition.asm`; `ai/startaicontrol.asm`, both `determineaistandbymovement_*.asm`. |
| Commands and healing | `data/battles/global/aicommandsets.asm`, `0xE1AC..0xE25B`, `aipriority.asm`; `ai/executeaicommand.asm`, `command/moveorder.asm`, its `buildmovestringformoveorder.asm`, `command/attack.asm`, `move.asm`, `heal.asm`, `heal/determinehealingspelllevel.asm`, `heal/calculatehealtargetpriority.asm`, `heal/doescombatantrequirehealing.asm`, `support.asm`, `getnextuseableaiaction.asm`, `gethighestusablespelllevel.asm`; `code/common/stats/spellstats.asm`, `data/stats/spells/spelldefs.asm`. |
| Item custody/actions/control | `code/common/stats/itemstats.asm:AddItem/IsItemUsableInBattle/GetItemSlotContainingIndex/RemoveItemBySlot`, `dealsinventory.asm:AddItemToDeals`, `0x99DE..0x9A3C`; `data/stats/items/itemdefs.asm`; `battleactions/useitem.asm`, `breakuseditem.asm`, `dropenemyitem.asm`, `battleactionsengine_1.asm`, `initbattlesceneproperties.asm`, `createbattlesceneanimation.asm:PerformAnimation`; independent `data/battles/global/enemyitemdrops.asm`; `battleloop/turnorderfunctions.asm`, `battlefunctions/executeindividualturn.asm`, `0x23EB0..0x24242`. |

The [spriteset](../../battle/battle-spriteset-data.md), [routing](../../battle/battle-routing-data.md),
[global](../../battle/battle-global-data.md), [loop](../../battle/battle-loop.md), [AI](../../battle/battle-ai.md)
and [cutscene](../../battle/battle-cutscenes.md) fixtures retain their existing boundaries.
`rowContentTracked:false` and `routeTargetsParsed:false` are unchanged. This
prose adds no executable Battle19 fixture or indexed count.

## Map68 selection and fresh versus saved lifetimes

**Confirmed:** BATTLE_OUTSIDE_ELVEN_VILLAGE19 is Map68's sole battle-map row:
output crop `(5,19,48,32)`, wildcard trigger255/255. CheckBattle requires F419,
not geometric containment, Caravan mode, F65, F518, F762 or membership. F519 set
clears419 after selection **without canceling that entry**. F469 intro and F88
suspension are independent. Battle numbering does not prove a natural route.
Walking's packed reload/residual parameters and mode1's lack of that zone
producer remain with the Caravan owner, not a new travel claim here.

Terrain slot19 decodes BattleTerrain19 into shared48×48 storage, distinct from
the48×32 crop. Background19 is OVERWORLD255; the background getter selects map
blocks rather than a fixed custom scene. Leader-presence19 is-1, independently
of Orc128's LEADER commandset. The half-EXP list selects only1, post-leader
position overrides only5, and the complete neutral table has no19 entry.
LoadBattle therefore adds no selected Caravan or other neutral actor here.

| Supplied state | Ordered selected entry work |
| --- | --- |
| F88 clear/F469 clear | Clear PLAYER_TYPE to0/set399; clear seconds; save/install map/battle/base VInts; empty before19; restore/reassert map/battle; clear90–105; heal; actual ally/enemy initialization, conditional upgrades and final refresh; clear AI memory; LoadBattle(0); start wrapper sets469 before its empty body; prepare round. |
| F88 clear/F469 set | Skip both script bodies; fresh healing/placement/upgrades/refresh/load/new round still run. F519 is a separate selector. |
| F88 set | After mode0/set399, restore saved seconds, clear88/AI memory/view target, LoadBattle with incoming focus, then retained queue/cursor. No fresh map/battle/area installation, region reset, healing, formation, upgrade, before/start or initial round occurs. A later sentinel can prepare a round. |

All four slot19 pointers select `ms_Empty`, whose `$FFFF` still reaches shared
script termination/conditional scroll wait/camera-speed clear. Before tests469;
start tests/sets469. End/after wrappers still have leader/join/outcome consumers
owned by the outcome document; empty does not mean no shared work. These hooks
do not recruit, repair following/AI, board the Caravan or set an egress map.

LoadBattle clears/redeclares physical actors and lookups from actual placed
combatants, then loads map/sprites/terrain/music/fade services. Prior field actors
are not a roster. Crop addition makes template0 local`(40,12)` world`(45,31)`
and enemy128 local`(14,19)` world`(19,38)`; neither is the village entrance nor
a guaranteed victory seed. Saved map/battle/area/roster/turn state must agree;
an incoming19 argument or469-set state does not make a valid resumed battle.
Service completion remains Unknown.

## Twelve templates and the actual party

**Confirmed:** header4 +27 combatants×12 +3 regions×12 +1 point×2 =366
(`0x16E`), matching exclusive end`0x1B482C`. Every ally row declares HEALER1/0,
NOTHING127, primary/secondary NONE255, regions15/15, filler0 and STARTING0.
Complete local coordinates in source order are:

| Template | X,Y | Template | X,Y |
| --- | --- | --- | --- |
| 0 | `(40,12)` | 6 | `(42,13)` |
| 1 | `(40,13)` | 7 | `(42,12)` |
| 2 | `(40,11)` | 8 | `(43,13)` |
| 3 | `(41,11)` | 9 | `(43,12)` |
| 4 | `(41,12)` | 10 | `(42,11)` |
| 5 | `(41,13)` | 11 | `(40,10)` |

Initialization clears all30 ally Xs to-1 and rebuilds ascending joined/active
lists from flags0–29/32–61. Only formation X/Y is copied to actual living
members; template IDs/AI/items do not install twelve fixed heroes. A dead ordinary
member consumes a list/loop iteration without advancing the placement cursor.
Twelve iterations limit capacity even when deaths leave rows unused; later
members enter LeaveBattleParty (active/X cleared, joined kept), without another
list rebuild. The zero-template guard is not an empty-party pre-loop guard.

JoinForce sets joined/rebuilds lists and calls JoinBattleParty only below12 active
members. At capacity a new inactive recruit remains reserve; an already-set
active bit remains. Direct JoinBattleParty has no capacity guard. Joining does
not itself revive, place, queue or choose control. Healing independently scans
all30: living allies and immortal Peter7/Lemon28 regain max HP/MP; other dead
allies stay dead. Intermediate `status & 7` retains STUN/POISON/CURSE, then
UpdateCombatantStats preserves STUN/POISON and derives **final CURSE from equipped
cursed items**. Old CURSE alone is not the final state. Class/status/gear affects
the refreshed stats; the old outcome owner's limited wording is corrected here.

Resolve actual ally enums: Bowie0, Sarah1, Peter7, Luke10, Rohde11, Rick12,
Lemon28. Rohde's active flag is43, not51; decimal11 is not hexadecimal`$13`.
Caravan acquisition/F65 and Rohde/F84 following do not grant joined11/active43.
Rick's optional following is not membership or a fresh combatant row. Prior
Peter/Luke recruitment, AI and follower producers keep their owners. A revived
Peter may still be reserve or follower-only; deployed Peter's live bit2/MUDDLE/
auto-battle state matters. Ally AI uses set6 independently of template HEALER1.
ClearAiMemory resets48 target/memory bytes, not combatant AI-control bit2.

## Fifteen rows and six full base definitions

**Confirmed:** every enemy has secondary NONE255/region15, filler0 and STARTING0.
Only129 adds a non-NOTHING placement item; its complete expression is below.
Slots128–142 initialize immediately; undeclared143–159 take HP0/X=-1 clearing.
Fresh mode0 does not use the later spawn-reset occupancy gate.

| Row /slot | Base and local X,Y | Commandset | Primary order | Primary region |
| --- | --- | --- | --- | --- |
| 0 /128 | ORC12 `(14,19)` | LEADER14 | MOVE_TO\|0 (`$40`) | 15 |
| 1 /129 | HIGH_PRIEST79 `(15,18)` | HEALER3/2 | FOLLOW_ENEMY\|0 (`$80`) | 15 |
| 2 /130 | SOULSOWER11 `(16,21)` | ATTACKER3/9 | FOLLOW_ENEMY\|0 | 15 |
| 3 /131 | GARGOYLE43 `(13,18)` | ATTACKER2/7 | NONE | 2 |
| 4 /132 | HIGH_PRIEST79 `(16,12)` | HEALER3/2 | FOLLOW_ENEMY\|5 (`$85`) | 1 |
| 5 /133 | ORC12 `(17,13)` | ATTACKER1/6 | NONE | 1 |
| 6 /134 | DARK_MADAM70 `(26,14)` | ATTACKER3/9 | FOLLOW_ENEMY\|7 (`$87`) | 0 |
| 7 /135 | SOULSOWER11 `(24,16)` | ATTACKER1/6 | NONE | 0 |
| 8 /136 | GARGOYLE43 `(26,21)` | ATTACKER1/6 | NONE | 0 |
| 9 /137 | ORC12 `(23,27)` | ATTACKER1/6 | NONE | 0 |
| 10 /138 | ARROW_LAUNCHER60 `(30,13)` | ATTACKER3/9 | FOLLOW_ENEMY\|11 (`$8B`) | 15 |
| 11 /139 | SOULSOWER11 `(32,15)` | ATTACKER1/6 | NONE | 15 |
| 12 /140 | SOULSOWER11 `(38,21)` | ATTACKER1/6 | NONE | 15 |
| 13 /141 | ARROW_LAUNCHER60 `(40,22)` | ATTACKER3/9 | FOLLOW_ENEMY\|12 (`$8C`) | 15 |
| 14 /142 | GARGOYLE43 `(38,3)` | ATTACKER1/6 | NONE | 15 |

All six definitions have spellPower REGULAR, initialStatus NONE and unknownByte
equal to their base ID. That byte is not another interpreted behavior. “Rest
NOTHING” fills all unused positions to four items and four spells.

| Definition | Level; HP/MP; base ATT/DEF/AGI/MOV | Gear / spells | Movement; original AI word |
| --- | --- | --- | --- | --- |
| Orc12 | 17;30/0;22/37/27/5 | Steel Lance75 equipped, rest NOTHING / all NOTHING | REGULAR; PRIORITYMOD_2\|BIT5 (`$2020`) |
| High Priest79 | 17;30/29;20/31/23/5 | Power Stick89 equipped, rest NOTHING / HEAL3, BLAST2, NOTHING, NOTHING | HEALER; PRIORITYMOD_3 (`$3000`) |
| Soulsower11 | 16;27/0;35/32/25/4 | all NOTHING / all NOTHING | REGULAR; PRIORITYMOD_2\|BIT5 (`$2020`) |
| Gargoyle43 | 15;29/0;31/23/21/6 | all NOTHING / all NOTHING | FLYING; PRIORITYMOD_2\|BIT4\|BIT5 (`$2030`) |
| Dark Madam70 | 17;29/17;19/31/29/6 | Power Stick89 equipped, rest NOTHING / FREEZE2, rest NOTHING | MAGE; PRIORITYMOD_2\|BIT4 (`$2010`) |
| Arrow Launcher60 | 17;33/0;15/24/23/4 | Steel Arrow47 equipped, rest NOTHING / all NOTHING | BRASS_GUNNER; PRIORITYMOD_2\|BIT4 (`$2010`) |

| Definition | Complete resistance | Complete prowess |
| --- | --- | --- |
| Orc | 0 | CRITICAL150_1IN8\|DOUBLE_1IN32\|COUNTER_1IN16 |
| High Priest | 0 | CRITICAL150_1IN32\|DOUBLE_1IN32\|COUNTER_1IN32 |
| Soulsower | LIGHTNING_WEAKNESS | CRITICAL_STUN\|DOUBLE_1IN32\|COUNTER_1IN32 |
| Gargoyle | 0 | CRITICAL150_1IN32\|DOUBLE_1IN32\|COUNTER_1IN32 |
| Dark Madam | ICE_MINOR | CRITICAL150_1IN32\|DOUBLE_1IN32\|COUNTER_1IN32 |
| Arrow Launcher | LIGHTNING_WEAKNESS\|STATUS_MINOR | CRITICAL150_1IN32\|DOUBLE_1IN32\|COUNTER_1IN32 |

InitializeEnemyStats calls repeat selection **before** copying56 definition bytes,
restores HP/MP, applies row position/identity/orders/regions, combines selected
movement upper nibble with row commandset, handles row items, reconstructs
activation, adjusts difficulty and refreshes stats. Priority upper nibble survives;
filler0/mode0 replaces definition BIT4/5. Those raw bits do not establish fresh
activation or automatic-control state. NOTHING returns before AddItem, preserving
definition gear. Difficulty's second BEQ makes every non-SUPER apply
floor(baseATT×5/4) once, SUPER twice, before equipped bonuses. With difficulty0
and unchanged bases, current ATT is43 Orc,40 Priest,43 Soulsower,38 Gargoyle,
38 Madam,35 Launcher. These are attack stats, not damage or performed prowess.

Steel Lance75: KNTE/PLDN/PGNT, WEAPON, range1–1, price810, ATT+16. Power Stick89:
MAGE/PRST/WIZ/SORC/VICR, WEAPON, range1–1, price1050, ATT+15. Steel Arrow47:
ACHR/RNGR/SNIP/BRGN/BWNT, WEAPON, range2–2, price1270, ATT+17. Each useSpell is
NOTHING63, remaining two equipEffects NONE0. Original definition equipment is
copied already equipped without validating the enemy's ordinary equip class.
Unarmed physical range defaults1–1; AI station building separately uses0–1.
Usability's incorrect255 comparison makes these weapons'63 fall back to HEAL,
which fails offensive TYPE_ATTACK filtering. Gear/range support does not imply
automatic item use or action execution.

## F519 and level-dependent repeat selection

**Confirmed:**19 is seventh in the length-prefixed random-battle list.
IsBattleUpgradable plus DetermineBattleUpgrade admits selection only with519 set,
independently of419/469. Clear519 returns base without upgrade RNG. F88 bypasses
fresh initialization; there is no per-round definition substitution.

Category reads the original definition's movement upper nibble before copy,
not the row commandset, enemy name or the substitute's later movement:

| Original types | Category/range R | Inclusive bounds | Exclusions |
| --- | --- | --- | --- |
| REGULAR Orc12/Soulsower11; BRASS_GUNNER Launcher60 | Melee5 | OOZE0..REAPER35 | KRAKEN_LEG10, PAWN13, KNIGHT14, RAT15, BURST_ROCK32, ZEON_GUARD38 |
| MAGE Madam70 | Mage1 | GALAM_MAGE67..CHAOS_WIZARD74 | QUEEN71 |
| HEALER Priest79 | Healer2 | DARK_CLERIC76..EVIL_BISHOP84 | BISHOP80 |
| FLYING Gargoyle43 | Airborne2 | HUGE_BAT40..DEMON52 | Count0, unsafe scan below |

Unrecognized movement returns base early; all six selected bases have recognized
branches. Bowie's current level supplies E, with+20 when signed-byte class>11.
Subtract **19**; nonpositive difference repeats that read once using shared D4,
then returns base. Positive difference divided by10 retains quotient low byte q;
q0 returns base. Ordinary eligibility begins at **E29**, not Battle17's27.
For an ordinary unsigned level byte plus optional20, eligible q is1..25.

Center D2 is base+`5q−5` for R5, otherwise base+q. GenerateRandomNumber(range R+1)
draws0..R and advances the16-bit main seed `(13×seed+7) mod65536`. Signed-byte
lower/upper bounds precede exclusions. One failed candidate consumes shared D4,
restores center and draws once more; another failure returns original base. No
clamp, unlimited retry, guaranteed strength increase or bulk random census follows.

| Base | q1 pre-filter domain | Relevant later ordinary q boundary |
| --- | --- | --- |
| Orc12 | 12..17;13/14/15 excluded | q5:32..37, with32 excluded/36–37 out; q≥6 has no in-bound candidate. |
| Soulsower11 | 11..16;13/14/15 excluded | q5:31..36, with32 excluded/36 out; q≥6 has no in-bound candidate. |
| Madam70 | 71..72;71 excluded | q2:72..73; q3:73..74; q4:74..75 with75 out; q≥5 all out. |
| Priest79 | 80..82;80 excluded | q2:81..83; q3:82..84; q4:83..85; q5:84..86; q≥6 all out. |
| Gargoyle43 | 44..46, before unsafe scan | Bounds and the same defective exclusion consumer still precede any claimed result. |
| Launcher60 | 60..65, all above35 | Ordinary q1..25 produces60..185:60..127 fails signed upper35;128..185 fails signed lower0. |

Thus ordinary eligible Launcher calls return60 after two draws despite no
substitution; earlier ineligible calls return it without those draws. It uses
melee, not ranged-family bounds. Gargoyle's blank excludedEnemies macro emits0;
the consumer subtracts1 and executes DBF across65,536 following bytes unless an
equality intervenes. A match uses retry/base fallback; exhaustion can accept the
candidate. The comment does not prove a safe base return. No out-of-table byte
census or successful Gargoyle roster is asserted.

All fifteen calls occur128–142 in order. Fallback draws and unsafe scans can
change subsequent seeds/returns and turn order. Returned substitutes change
stats/HP/MP/movement/gear/spells/resistance/prowess/priority; placement commandset,
coordinates, regions, orders and item request remain the row's operands. The
base stats, healing and turn claims here are conditional on unchanged definitions.

## Three ordered regions, signed projection and standby

**Confirmed:** all three regions have type4, byte1=0 and trailing0/0.

| Region /flag | Vertices1,2,3,4 | Assigned primary slots |
| --- | --- | --- |
| 0 /90 | `(0,0),(0,31),(35,31),(35,0)` | 134–137 |
| 1 /91 | `(0,0),(0,31),(28,31),(28,0)` | 132/133 |
| 2 /92 | `(0,0),(0,31),(23,31),(23,0)` | 131 |

IsRegionTriggered scans all30 allies with positive signed HP and nonnegative
signed-byte X/Y; joined/active/following are not extra predicates. Type4 tries
triangles1/2/4 then3/2/4. Each triangle tests edges1→2,3→1,3→2 in order.
Segment projection uses signed MULS/DIVS truncated toward zero, horizontal
dominance on equal spans, SUB.W unsigned borrow, and bounded collinearity.
For horizontal dominance `Yproj=Yb+trunc((Yb−Ya)×(Xc−Xb)/(Xb−Xa))`; vertical
dominance exchanges X/Y. In-bounds collinearity returns01 and immediately
accepts. Ordinary flags must equal the opposite vertex's flag; bit0 exceptions
skip either early edge but reject at the third. Negative projection explicitly
writes CCR=1 and returns10 despite its misleading11 comment. Arbitrary
cross-product geometry is not presumed equivalent.

All twelve positions substituted into all six triangles yield36 region-position
misses /72 triangle substitutions. Complete first-rejection witnesses are:

| Region /triangle | First rejecting edge, original vertices | Ally projection/flag; opposite projection/flag |
| --- | --- | --- |
| 0 /1,2,4 | Third,4→2 | Y−7..−4 /10; vertex1 Y31 versus0 /00 |
| 0 /3,2,4 | Second,4→3 | X35 versus40..43 /10; vertex2 X35 versus0 /00 |
| 1 /1,2,4 | Third,4→2 | X16..18 versus40..43 /10; vertex1 X28 versus0 /00 |
| 1 /3,2,4 | Second,4→3 | X28 versus40..43 /10; vertex2 X28 versus0 /00 |
| 2 /1,2,4 | Third,4→2 | X13..15 versus40..43 /10; vertex1 X23 versus0 /00 |
| 2 /3,2,4 | Second,4→3 | X23 versus40..43 /10; vertex2 X23 versus0 /00 |

Region0 diagonal4→2 is horizontally dominant because35>31:
`Yproj=31+trunc(31×(X−0)/−35)`, producing the negative values above.
Regions1/2 diagonals are vertically dominant:
`Xproj=trunc(−W×(Y−31)/31)`, W28/23. The table plus twelve coordinates
reproduces all cases without an ignored helper. Actual living fresh placements
use a prefix/subset; dead ordinary members do not advance that cursor and other
Xs reset-1. No fresh90/91/92 hit follows; later/saved positions are separate.

ActivateEnemies scans128–159 and falls through once for160 because the outer
body lacks RTS; the [alias owner](../northern-travel-mitula/battle26-activation-and-first-turn-dispatch.md#the-extra-fall-through-and-its-selected-ram-alias)
retains its conditional defect. TriggerRegionsAndActivateEnemies tests every
declared region before this actor's assignments. NEWLY_TRIGGERED_BATTLE_REGIONS
is a tested-word cache, including misses, distinct from persistent90–105. With
clear incoming tested bits these fresh checks leave cache7 and flags clear.
Incoming tested bits can suppress work; fresh flag reset/ClearAiMemory does not
explicitly clear this word. Normal enemy StartAiControl clears it.

A primary hit sets activation bit0 then returns, retaining existing bit1; only
a secondary hit sets bits0/1. Here every secondary is15 and no region15 exists.
Activation does not exchange orders. Region programs select only32/40, not19;
all selected/undeclared fresh modes0 yield no hidden/respawn selection.

Inactive132/134 have non-NONE follows with assigned regions: eligibility returns
nonzero d1, whose caller **stops**, producing STAY/empty move string. Earlier
thinking RNG may also stop. Inactive131/133/135/136/137 have NONE orders; the
zero d1/d2 branch permits local standby movement about their starting position,
subject to memory/terrain/occupancy/RNG. It does not execute the active stream.
The other eight slots128/129/130/138/139/140/141/142 have15/15 and bypass inactive
standby irrespective of bit0. None of these distinctions fixes a performed move.

## Live orders, declared point and complete streams

**Confirmed:**129/130 `$80` resolve live enemy128,132 `$85` live133,134 `$87`
live135,138 `$8B` live139 and141 `$8C` live140. These are current combatants,
not formation templates, ally IDs or frozen coordinates. Primary128 `$40` selects
the **declared** point0 `(14,19)` at the spriteset's final two bytes. Unlike18,
it does not read the next spriteset's header. NONE255 must precede lookup.

On the activated/unconditional path a dead primary live referent promotes
secondary NONE and clears secondary, leaving both NONE. This precedes stream
selection even for a set lacking MOVE_ORDER; inactive132/134 delay the mutation
until the active path. Point orders skip the HP test. Actual command streams
stop at the first d1=0 success:

| Set /slots | Complete stream | Preparatory restriction |
| --- | --- | --- |
| HEALER3/2 /129,132 | HEAL2, MOVE_ORDER4, SUPPORT, ATTACK3, MOVE2, STAY | Block non-movable |
| ATTACKER1/6 /133,135,136,137,139,140,142; actual AI allies independently | ATTACK1, HEAL1, SUPPORT, MOVE1, STAY | Regular |
| ATTACKER2/7 /131 | MOVE_ORDER1, ATTACK1, HEAL1, SUPPORT, MOVE1, STAY | Regular |
| ATTACKER3/9 /130,134,138,141 | MOVE_ORDER3, ATTACK3, HEAL2, SUPPORT, MOVE_ORDER4, MOVE2, STAY | Block non-movable |
| LEADER14 /128 | MOVE_ORDER5, ATTACK2, HEAL3, SUPPORT, MOVE_ORDER2, MOVE3, STAY | Block and carve |

Set6 has no MOVE_ORDER. Set7's NONE fails that command before continuing.
MOVE_ORDER1/4/5 use target type0;2 uses type2 and3 type1 (duplicate follow paths).
The stored bit6 still chooses point versus live position, so target type0 does not invent a point destination
for129/132. Type0 tries current/adjacent stations and uses `tst.b` on returned X:
X0 fails, whereas-1 is nonzero and may continue. Nonzero types test temporary obstruction
before its ordered attack/movement branch. Both retain MOV/NONE/dead-target
guards, quadrant marking, conditional ATTACK1 and movement construction. Reverse
search uses selected movement costs, budget128, preliminary MOV×2 and stop radii
0/1/2/3. Successful movement may publish STAY with a nonempty string. Failure
advances to later commands; command-local restriction processing can still run.

Orc128 MOV5 permits a real point consumer. LEADER's prepass already obtains that
valid point before the stream, marking temporary obstruction/carving radius0–2
with fresh lastTarget=-1; its last-target variant still gets the order center
and uses radii0–4. The actor starts at the point, but that does not promise
idleness, pursuit or execution of every later command. Ordinary MOVE scores live
opponents/path costs. Top-level exit clears temporary obstruction flags. New
enemy names need no replacement for these generic point/live/NONE mechanisms.

## Learned HEAL3, BLAST2 and FREEZE2

**Confirmed:** High Priest learns HEAL3 and BLAST2; Dark Madam learns FREEZE2.
BLAST2 is silence-affected TYPE_ATTACK, MP5/range1–2/radius1/power8; BLAST1 is
MP2/range1–1/radius0/power6. FREEZE2 has the same type/silence property,
MP7/range1–2/radius1/power12; FREEZE1 is MP3/range1–2/radius0/power10.
GetNextUsableAttackSpell/GetHighestUsableSpellLevel filter learned type,
confusion and affordable levels, allowing lower known levels. Fresh29/17MP
covers the base costs, not a guaranteed target/station/cast. No selected base
learns SUPPORT. Soulsower stun is prowess rather than a spell.

HEAL levels1/2/3/4 declare MP3/5/**10**/20, ranges0–1/0–2/0–3/0–1,
radius0 and powers15/15/30/255. These definitions differ from the AI's
hardcoded11MP promotion threshold. The healing command rejects Muddle2,
requires at least3MP for HEAL, and builds living friendly candidates satisfying
3×HP≤2×maxHP. MissingHP M≤2 fails. HEAL2 in the command name is an unused
parameter, not a mandated spell level. No base enemy is initially injured.

CalculateHealTargetPriority actually tests commandsets13/14 (CRITICAL/LEADER),
not its comment's14/15, and returns maximum13 for Orc128. Other selected base
priorities from `aipriority.asm` are Priest12, Madam11, Gargoyle6, Launcher5,
ordinary Orc/Soulsower2. The command sums each affected target's priority+4,
sorts descending with ties retained, then tries targets until a valid station
is found. These priorities precede the per-target level/range choice and do not
grant a heal to a dead, full-health or unreachable actor.

For a candidate whose **current incoming local spellEntry is`$80`**, initially
read from learned HEAL3, DetermineHealingSpellLevel extracts incoming rank2
(zero-based). M3–14 initially selects d2=0; M≥15 selects2, including M>28
because incoming rank3/HEAL4 is absent. Its faulty shift5 plus **unmasked
d4=$80** checks the following exact getter keys. This table is conditional on
that current input; it does not describe every candidate in one HEAL command.

| Trial d2 | Getter key /resolved cost | Returned rank when affordable |
| --- | --- | --- |
| 2 | `$C0` /HEAL4,20MP | 2 =HEAL3 |
| 1, after decrement | `$A0` /ATLAS3 absent, getter defaults to first HEAL1,3MP | Converted1→0 =HEAL1 |
| 0 | `$80` /HEAL3,10MP | 0 =HEAL1 |

The getter compares whole entry bytes and does not mutate d4. For **current
input`$80`** and the command's initial3MP gate, the helper fails M3–14 at MP3–9,
returns HEAL1 there at MP≥10; for M≥15 it returns HEAL1 at MP3–19 or HEAL3 at
MP≥20. The caller promotes returned HEAL1 to **HEAL2** for a nonself target at
MP≥11 only when the **current local entry's rank** is≥2. Self targets do not
receive that promotion. On that same`$80` input, nonself M3–14 selects HEAL1
at10MP, HEAL2 at≥11; nonself M≥15 selects HEAL1 at3–10, HEAL2 at11–19,
HEAL3 at≥20. Actual casting cost/range follows the selected spell, not the
erroneous checked key. These are single-candidate input conditions;29MP and a
learned HEAL3 slot alone do not establish the result for a later candidate.

The caller's local lifetime matters. `heal.asm:@CheckSpellLevel` reads
`spellEntry(a6)` into d4 at line315. `@UpdateSpellEntry` writes this candidate's
selected rank back to that local byte at344–348 **before** range/station
selection. If DetermineAttackPosition fails,373–381 advances d6/decrements d5
and branches back to `@FindPositionForTargets_Loop`, without calling
GetNextHealingSpell again. The next candidate therefore receives the retained
local`$80`, downgraded`$40`, or downgraded`$00`, rather than automatically
receiving the original learned HEAL3. If the helper returns-1, the caller skips
the local write and advances with the previous entry unchanged. No MP is spent
by these failed selection attempts.

| Current incoming local entry after an earlier candidate | Next candidate's helper/promotion boundary |
| --- | --- |
| `$40` /HEAL2 | Incoming rank1 prevents the M≥15 promotion to rank2. For any M>2 the helper starts d2=0, checks key`$40`/HEAL2 cost5, and returns HEAL1 at MP≥5 or failure at MP3/4. The caller's current-rank≥2 test fails even at29MP; no nonself HEAL2 promotion occurs. A successful level choice writes`$00` before its station test. |
| `$00` /HEAL1 | Incoming rank0 likewise starts d2=0 for any M>2, checks key`$00`/HEAL1 cost3 and returns HEAL1 at MP≥3. No nonself promotion occurs; the local remains`$00`. |

M≤2 still fails without a local update; these rows remain subject to the earlier
living/injury candidate filters, current MP, order and an available station.
Neither local downgrade changes the combatant's learned spell slots. A new
independent `aiCommand_Heal` invocation initializes its locals; on reaching
`@CheckHealingSpell` it calls GetNextHealingSpell again. With the unchanged learned
slot it starts from`$80` again. This is neither permanent spell unlearning nor
a reset for each candidate inside the current command.

A source-backed supplied-state counterexample is activated Priest132 at29MP,
leader Orc128 at20/30HP (priority13), and Priest129 at10/30HP (priority12).
If Orc128 has no HEAL2 station and Priest129 subsequently has a HEAL1 station,
Orc's deficit10 first changes local`$80→$40` through nonself promotion, then
fails station selection. Priest129's deficit20 now enters with`$40`, selects
HEAL1/check5MP and changes local`$40→$00`; it cannot select the HEAL3 claimed
by an unconditional deficit≥15/MP≥20 rule. The conditional stations are supplied
premises, not a naturally reached position or performed heal. Candidate order,
path/range, earlier stream success, status and resource depletion remain conditional.

## Shining Ball: declaration, retained bits and use

**Confirmed:** row129 declares16|USABLE_BY_AI`$2000`|UNUSED_ITEM_DROP`$4000`
=`$6010`. Item16 is equipFlags NONE, range0–1, price1000,
RARE|CONSUMABLE, useSpell FBALL37, all three equipEffects NONE0. FBALL1 is
TYPE_ATTACK without AFFECTEDBYSILENCE, MP0, spell range1–1/radius0/power20,
BLAZE|VARIATION3 animation. Item range and spell range are separate operands;
neither20 nor a selected animation proves final damage or presentation.

InitializeEnemyItems calls AddItem unless the **whole word** is NOTHING127.
AddItem scans four slots with index mask`$7F`; on success it ANDs d1 with`$807F`,
stores/returns that word and d2=0. Therefore `$6010` becomes **`$0010`**:
equipped bit7, AI-use bit13 and unused-drop bit14 are all clear. Base Priest's
slot0 remains equipped Power Stick; slot1 receives plain16. The following bit7
test uses returned d1 and does not call EquipItemBySlot. No new stat effect follows.
With a substituted full inventory AddItem returns failure without masking d1;
initialization ignores d2 but original`$6010` still has no equip bit, so it adds
nothing and attempts no equipment scan. Pre-existing definition inventory is a
separate supplied condition; placement creates neither capacity nor a reward.

IsItemUsableInBattle can accept FBALL, but automatic attack selection additionally
requires equipped or AI-use bit13. Fresh16 fails. Moreover GetNextUsableAttackItem
first examines equipped Power Stick89 at slot0: its NOTHING63→fallback HEAL
fails TYPE_ATTACK and returns NOTHING **without continuing to slot1**. Merely
restoring bit13 on the Ball would not bypass that earlier termination. With a
different inventory that actually reaches a permitted16, nonconfused TYPE_ATTACK
can admit FBALL; confused selection's BLAZE/FREEZE/BOLT/BLAST whitelist excludes
FBALL37. These are changed-state predicates, not a performed use on fresh19.

Healing-item selection permits Healing Rain or AI-use bit13 then requires
TYPE_HEAL; Ball is neither Rain nor healing and fresh16 lacks bit13. HEALER3's
first HEAL2 command uses the learned-healing path above before its later attack
command. If that fails, MOVE_ORDER4 may succeed before ATTACK3. Attack decision
builds separate physical/spell/item candidate lists; it cannot resurrect an item
rejected by the getter. With actual viable item and spell lists it uses a
thinking-RNG range2 choice; single item-or-spell versus physical branches use
main-RNG range6, with their own availability fallbacks. Fresh Priest's available
learned BLAST is distinct from an unavailable Ball. No fixed tactical choice follows.

If a USE_ITEM action is actually admitted, UseItem unpacks FBALL and delegates
to CastSpell. PerformAnimation charges MP only for CAST_SPELL; the initial
spell-silence branch also distinguishes CAST_SPELL from USE_ITEM. BreakUsedItem
tests equipment type first: Shining Ball is neither weapon nor ring, so it
**removes the action's actual item slot**, before the enemy-user exemption that
applies to breakable equipment. No break RNG or spare copy is granted by RARE
or the source CONSUMABLE spelling. This conditional consumption differs from
Taros's equipped sword exemption. Lost/used inventory cannot later be dropped
merely because its placement once carried UNUSED_ITEM_DROP.

## Independent action-drop custody

**Confirmed:** `enemyitemdrops.asm` independently selects battle19, enemy row1
(combatant129), item16, once-bit6. This is a battle/slot match, not a High Priest
type match or placement-bit authority. DropEnemyItem follows ordinary target
effects, with separate second/counter calls. It requires ally actor, enemy target,
local targetDies, matching table row and GetItemSlotContainingIndex finding an
actual held16. Masked flags do not prevent that identity search; absence after
consumption/removal or failed placement does prevent this selected drop.

Only Taros Sword/Iron Ball/Counter Sword use the range32 branch. Shining Ball
takes **no drop RNG**. The obsolete IsBattleUpgradable/random code is unreachable
after the unconditional branch, so519 does not impose a second loot roll.
Once-bit6 is byte0/bit6 of ENEMY_ITEM_DROPPED_FLAGS, not story F6. BSET returns
if already set. Otherwise it sets the bit, removes the target's actual slot,
then checks the acting ally's current HP and AddItem capacity. Living actor plus
capacity receives plain16. HP0/full inventory takes RARE-deals fallback via
AddItemToDeals because16 is RARE; it does not leave the item on the defeated
target or try another ally. GetDealsItemInfo maps even item16 to byte8's upper
nibble; AddItemToDeals adds`$10` unless its count is already15. At saturation
the item is discarded, with source removal/once-bit already committed. Receipt,
source removal, once-bit and deals are distinct transitions.

The outcome owner's independent leader-presence sweep can remove remaining
enemies after128 dies; it does not run targetDies/action loot for every swept
combatant. Victory neither conjures16 nor guarantees an earlier action receipt.
A consumed last copy cannot drop; a separately held matching copy must satisfy
the same predicates. Stop at these helper conditions, without claiming an actual
kill, use, drop, deals transaction or winning trajectory.

## Round generation and first conditional control

**Confirmed:** fresh round order is activation → region program → spawn selection
→ turn generation. The generator clears64 sentinels, scans30 allies then128–157,
and admits nonnegative-X/nonzero-HP actors independently of activation/standby.
All fifteen unchanged base enemies qualify, with AGIs in slot order
`27,23,25,21,23,27,29,25,21,27,23,25,25,23,21`. None has extra-turn bit128;
they supply fifteen entries, including inactive standby actors. Repeated
substitutions can change stats and extra-turn behavior; no repeated queue is fixed.

Ordinary score is AGI + RNG(AGI>>3) − RNG(AGI>>3) + RNG(3) −1. Actual bit128
adds the distinct second score using floor((AGI&127)×5/6) and two draws. Range0
still advances the main seed.62 signed-byte stable-sort passes preserve append
order ties and set cursor0. Actual ally gear/stats, initial seed and earlier
upgrade/fallback draws affect order. Standby uses thinking RNG separately; not
all AI decisions use the same generator. No fixed first actor/player-ready seam
follows from template or definition data.

ExecuteIndividualTurn masks actor identity and skips HP0. MUDDLE/AI bit2 chooses
AI preparation first; otherwise enemy **opponent-control=0** selects AI and
nonzero player preparation. Ally auto-battle separately selects AI. Cursor/view/
window/grid preparation precedes SLEEP/STUN NoAction, so preparation may already
occur before status prevents action. Dispatch repeats MUDDLE/bit2/control
polarity before AI execution versus player input. F88 can instead supply a retained
queue/cursor, changed positions/orders/cache/HP/MP/inventory; fresh prose repairs
none of them. Stop at this conditional control boundary.

## Accepted production consumers and conversion limits

**Confirmed by accepted implementation inspection, separately from original-game
evidence:**19 is not exported, trusted/admitted or played by the current route.
[Trust](../../../../remake/docs/content/profiles-and-trust.md#selected-third-encounter)
and [Application](../../../../remake/docs/application/session-and-programs.md#yeel-guide-zone-request-and-sleep-continuation)
retain selected2/3 consumption. No consumer is changed/executed here.

| Actual consumer | Reusable support and selected19 limit |
| --- | --- |
| [prepare_encounter / compile_routes](../../../../src/sf2tool/remake_exploration_battles.py), [scene producer](../../../../src/sf2tool/remake_battle_scene_content.py) | Encounter producer admits2/3, scene producer1/2/3. Generic wildcard/400–450–500 routing and before/load/start phases exist; no19 export/route is admitted. Caravan zone-request conversion stays with its owner. |
| [PrivateBattleEncounterReader](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs), [PrivateBattleDefinitionReader](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleDefinitionReader.cs) | Generic complete formation, rectangular regions, declared points, definitions/items/spells/metadata exist; trusted pins admit1/2/3. These type4 rectangles satisfy generic four-vertex/nondegenerate representation, unlike18's type3 rows. Generic lookup needs no new enemy-family mechanism merely for Orc/Priest/Madam names. No519/effective-level/category/exclusion/RNG substitution is implemented. |
| [PrivateBattleScenarioReader.Assemble](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs), [PrivateBattleActionBindings](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleActionBindings.cs) | Independent region-program admission1/2/3 rejects19. Sets6/7/9/14, point/live/NONE, REGULAR/HEALER/FLYING exist; HEALER3/2 rejects, as does the selected Shining Ball placement expression. Existing Herb-specific placement lowering stores0 and cannot be generalized by changing admission alone. Gargoyle/Madam/Launcher BIT4 fail AI-word binding; MAGE/BRASS_GUNNER movers reject. Orc and Soulsower prowess reject; Priest's ordinary prowess can fit. Steel Lance/Power Stick adjacent WEAPON metadata can fit descriptively, Steel Arrow2–2 rejects physical binding. HEAL definitions can bind healing actions; BLAST/FREEZE/FBALL do not gain executable support from descriptions. Controlled classes0/1/2/4 exclude arbitrary natural parties. |
| [BattleEntry](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs), [BattleInitializationRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleInitializationRules.cs) | Membership rebuild/carried resources/seeds, formation/living-immortal healing, difficulty0 ATT/activation exist. Excess active count rejects instead of source eviction. General gear/final-CURSE refresh remains limited; enemy loadouts0/127 reject the equipped weapons and plain16. F88 rejects rather than restoring queue; other difficulty/control policies and natural party producers remain limits. |
| [BattleActivationRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActivationRules.cs), [BattleTurnFlow](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleTurnFlow.cs), [TurnOrderRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/TurnOrderRules.cs) | Mode0/RegionProgram.None, primary-first activation,15/15 and randomized/extra turns exist. Cross-product geometry and fresh tested-state handling do not prove signed-projection/CCR/cache equivalence, even though these fresh misses agree. No19 observation validates the join. |
| [SourceEnemyAi](../../../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceEnemyAi.cs), [SourceMoveOrderAi](../../../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceMoveOrderAi.cs) | Standby,6/7/9/14 streams, live-follow/dead-target promotion and generic valid-point LEADER restrictions exist. Point0 is in range here;18's missing-point exception does not apply. Activated AI rejects2, learned spells and loadouts beyond0/127; automatic allies reject. Exact HEAL3 bug/promotion, healing/attack-item/learned-spell priorities, gear/ranged actions and repeat definitions need explicit conversion/support. |
| [BattleControlRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleControlRules.cs), [physical action](../../../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2PhysicalAction.cs), [capability owner](../../../../remake/docs/capability-status.md) | Existing physical/control/reward primitives do not bind selected16 action-use/consumption/once-bit/drop/deals or full resistance/prowess effects. Generic LEADER/point/live orders need reuse, not another enemy-name special case. Party, service, suspend and19 adapter/content support remain separate limits. |

These are explicit conversion/support boundaries, not an implementation request.
Description, trusted selection, assembled executable capability and played
behavior are different. No admission-only edit or new platform is proposed.

## Unknowns, retained failures and reproduction

**Unknown:** natural flags/route/party/capacity/levels/classes/gear/status/seeds,
Gargoyle scan/substitution results, later crossings/cache state, actual targets/
paths/healing/item/spell choices, first actor/action, performed damage/prowess,
obtained Ball/deals, resumed-state validity, completed services/presentation/
hardware timing and persistence. These do not create an automatic runtime queue.

Accepted Battle01 observations remain at their own boundary. #1047's twelve
**completed unresolved comparisons** remain at the
[verification owner](../../../../remake/docs/verification/field-programs.md#yeel-guide-through-battle03),
not passed or interrupted. Preserve
[PR1221's control/summary corrections](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1220#issuecomment-6354981415),
intermediate-status/final-CURSE results,
[Battle12's HEAL/identity correction](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1234),
[Battle13's neutral correction](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1236),
and Battle16/17's completed findings/failures. PR1243's initial `a30bc4c5`
mechanical PASS did not catch Rohde19/51; corrected `50baf80c` establishes11/43
before accepted merge. This document does not replay or relabel those failures.
PR1245's initial `16f4233a` direct checks/CI passed, but
[independent P2 review](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1245#issuecomment-6366643680)
found that its HEAL3 MP summary generalized current`$80` input across all
candidates. The local-entry lifetime and`$40/$00` boundaries above correct that
claim; the original PASS receipts remain mechanical results, not semantic closure.

Reproduce at the pin/base by reading all27 rows, three regions, declared point
and six full bases, then named callers/consumers above. Resolve Map68's sole19
and independent globals/hooks before fresh/saved claims. Re-derive E−19/q,
categories/bounds/exclusions/retry/seed and copy order; keep unsafe airborne
outcomes Unknown. Substitute every placement into both triangles of each region
with signed projection/CCR and edge order. Separate cache, region assignments,
NONE/follow standby, actual referents and stream short-circuiting. Trace current
local`$80` through helper keys`$C0/$A0/$80`, default lookup, cost20/3/10 and
nonself promotion, then follow UpdateSpellEntry and failed-station continuation
into subsequent`$40/$00` inputs before reporting HEAL3 behavior. Distinguish
unchanged learned slots and the next independent HEAL command's fresh read.
Follow`$6010→$0010`, slot0 getter
termination, conditional use/removal and independent targetDies/held16/bit6/
capacity/RARE-deals in their actual order. Inspect each production rejection
independently of descriptive source data.

This is bounded selected-source/consumer reading plus72 arithmetic cases, linear
in selected records/edges, one process and small ignored `local/issue1244/`
receipts, budgeted below64 MiB/one minute for arithmetic/check work. Triangle
substitution completed in0.001 seconds rounded; peak memory was not measured.
The retained18,285,979-byte pair and pair.txt are checked only by existing
length/mtime metadata against `local/issue1230/retained-before.json`; neither
payload is copied, parsed or regenerated. Reassess material scope/resource growth.

Direct checks cover UTF-8/LF, links/anchors, private boundary, exact four paths,
unchanged fixture metrics and four frontiers. On a clean committed candidate,
load `. ./local/private-inputs.ps1` in the same command process, then run
`uv run sf2 verify plan --base origin/main --head HEAD` and exact-head scoped CI.
Docs-only policy controls generic suite suggestions: no normal/full/SDK/Godot/
emulator/H3/H4, screenshots, historical comparison or collection rerun. The
Issue/PR handoff retains actual checks, inspection errors, exact Git objects,
stopped writer/process state and successful main-gate notification. Stop at
the frozen Draft PR; do not merge, clean up, archive or start the next task.
