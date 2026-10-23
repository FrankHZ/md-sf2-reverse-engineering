# Battle08 initialization, repeat upgrades and first control

Evidence date: 2026-10-22. **Confirmed:** North Cliff's complete13/11/4/1
declaration supplies thirteen placement templates, eleven enemies, four sloping
regions and one point. Templates are not thirteen recruited or active allies.
F508 can enable actual per-enemy type selection on fresh reentry; F458 gates
the intro independently. Fresh placements trigger none of the four regions.
**Inferred:** the already documented confrontation frames the battle as rescuing
the worker. **Unknown:** natural entry, actual party/status/level/difficulty/RNG,
first actor, all actions, presentation/timing and durable continuation. This is
selected static composition, not a playable or naturally observed encounter.

## Scope and provenance

This is the **Standard**-rigor agreement for
[Issue1226](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1226),
under [Epic1053's finite denominator](./story-content-coverage.md#scope-and-reading-rules).
Its outcome supports content maintenance and future conversion review: connect
the complete selected formation, actual initialization, repeat upgrade and
activation to the first conditional player/AI/no-action dispatch. Cross-owner
state and consumer distinctions justify Standard rigor. Stop before an executed
individual action. The [plank/admission owner](./damaged-ship-plank-through-battle08.md#map72-admission-and-battle08-beforestart)
already owns all35 before commands and seven actual text keys. The
[outcome owner](./battle08-outcomes-through-new-granseal-handoff.md)
owns victory/defeat/escape, after-scene, Map73 construction and outgoing castle
request. Neither surrounding account is rewritten here. Shared lifecycle rules
remain at [Battle02](./battle02-initialization-and-first-control.md).

Owned writes are this document and Battle08-related consistency edits in the
two coverage owners. Original source, production code, contracts, fixtures,
schemas, registries, counters and runtime frontiers are read-only. No parser,
platform, allowlist, engine implementation, export, acquisition or new runtime
comparison is added. Acceptance is complete selected source/consumer review,
direct prose/link/private/scope checks, committed-head planner and exact-head
scoped CI, then a clean pushed frozen Draft PR for independent acceptance.

Original paths below resolve under `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at this pin. Accepted repository base is
`70d14820f05193ae4f17fddabef38f40920c6c73`, tree
`478b4e5addda06afd0bfb862dc243eeed3604994`. The
[USA baseline](../../README.md#pinned-original-baseline) retains input identity;
this slice makes no fresh ROM-equivalence claim. Short battle paths below have
prefix `code/gameflow/battle/`; `j_` interfaces resolve to named implementations.
Complete selected bodies, including call order and return tails, supply the
facts rather than source comments alone.

| Claim | Declaration and actual consumer |
| --- | --- |
| Complete13/11/4/1 | `data/battles/spritesets/entries.asm` slot8 → `spritesets/spriteset08.asm:BattleSpriteset08`, `0x1B3822..0x1B3978`; `sf2macros.asm:allyCombatant/enemyCombatant/combatantAiAndItem/combatantBehavior`, `sf2enums.asm`; `getbattlespritesetsubsection.asm:GetBattleSpritesetSubsection`, `0x1B1630..0x1B169E`. |
| Admission and metadata | `code/common/maps/getbattle.asm:CheckBattle`, `0x799C`; `code/gameflow/mainloop.asm`; `data/battles/global/battlemapcoords.asm` row8, `terrainentries.asm` slot8, `global/custombackgrounds.asm`, `enemyleaderpresence.asm`, `halvedexpearnedbattles.asm`; `battleloop/loadbattleterraindata.asm`, `battlescenes/getbattlescenebackground.asm`. |
| Before/start and scene boundary | `data/battles/entries/battle08/cs_beforebattle.asm:bbcs_08/ce_4AA14`, `0x4A952..0x4AA74`; `data/scripting/text/gamescript.txt` keys099D–09A3; before/start start/end wrappers under `cutscenes/` and tables under `data/battles/cutscenes/`; `code/common/scripting/map/ms_empty.asm`. |
| Scene identity/alive filtering | `code/common/scripting/map/mapfunctions.asm:InitializeMapEntities`, `followersfunctions_1.asm:InitializeFollowerEntities`, `mapscriptengine_1.asm:csc19_setEntityPosAndFacing/AdjustScriptPointerByCharacterAliveStatus`, `_2.asm` interpreter return; `data/scripting/entity/eas_actions.asm:eas_Jump`, `eas_main.asm:eas_Idle`. |
| Actual party/refresh/enemies | `battleloop_1.asm:BattleLoop`, `0x23A84`; `battleloop/heallivingandimmortalallies.asm`; complete `initializecombatants.asm`, `0x1B1272..0x1B15F8`; `code/common/stats/battleparty.asm:UpdateForce/JoinForce/JoinBattleParty/LeaveBattleParty`, `0x9900..0x99DE`, and `updatecombatantstats.asm`. |
| Definition/equipment/spell/load | `data/stats/enemies/enemydefs.asm` records4/5/57/68; `data/stats/items/itemdefs.asm` records33/46/87; `data/stats/spells/spelldefs.asm:FREEZE`; `battlefunctions/loadBattle.asm`; `data/battles/global/battleneutralentities.asm` row8 → `code/common/scripting/entity/entityfunctions_1.asm:PositionBattleEntities`. |
| Repeat selection | Complete `battleloop/upgradeenemies.asm`, `0x1B1780..0x1B1A28`; `data/battles/global/randombattles.asm`, `0x1B6DB0..0x1B6DDA`; `sf2macros.asm:battles/upgradeRange/excludedEnemies`; `code/common/tech/randomnumbergenerator.asm:GenerateRandomNumber`. |
| Activation and orders | Complete `battleloop/activateenemies.asm`, `triggerregions.asm`, `0x1AC9FC..0x1ACF30`, `clearaimemory.asm`, `populateenemyspawns.asm`; both code/data `cutscenes/regionactivatedcutscenes.asm`; `code/common/stats/combatantstats_1.asm:GetMoveOrders/GetTriggerRegions`, `_2.asm:SetTriggerRegions`; `battlefield/getmoveorderposition.asm`. |
| AI and first control | `ai/startaicontrol.asm`, both `determineaistandbymovement_*.asm`, `executeaicommand.asm`, `command/moveorder.asm`, `command/moveorder/buildmovestringformoveorder.asm`, `command/attack.asm`, `getnextuseableaiaction.asm`, `gethighestusablespelllevel.asm`; `data/battles/global/aicommandsets.asm`; `battleloop/turnorderfunctions.asm`; `battlefunctions/executeindividualturn.asm`, `0x23EB0..0x24242`. |

Shared [spriteset](./battle-spriteset-data.md), [global](./battle-global-data.md),
[routing](./battle-routing-data.md), [loop](./battle-loop.md),
[AI](./battle-ai.md) and [cutscene](./battle-cutscenes.md) owners retain their
inventories. Existing `tests/fixtures/h2/battle-spriteset-data-static-v1.json`,
`battle-global-data-static-v1.json`, `battle-routing-data-static-v1.json`,
`battle-loop-static-v1.json`, `battle-control-static-v1.json`,
`battle-ai-static-v1.json` and `battle-cutscenes-static-v1.json` are unchanged.
`rowContentTracked:false` and `routeTargetsParsed:false` remain fixture limits,
not executable proofs of this selected composition.

## Admission, before return and three entry lifetimes

**Confirmed:** `BATTLE_NORTH_CLIFF=8` selects coordinate row8 (`0x7A6E`): Map72,
area origin`(1,32)`, width32/height30, wildcard triggers255/255. `CheckBattle`
tests effective map/unlock/trigger in table order; the area is output, not a
containment predicate. With F408 set, incoming`(1,1)` can select8 despite lying
outside that area. Later Map72 rows24/26 cannot preempt eligible8. F408/F508
both set clears408 but **still returns8 on this call**. F408 clear skips it.
Map72 has no flag-switched replacement. The plank owner's actual F408 producer
is `setStoryFlag 8` with the400 offset; F703 and F458 are not admission tests.

Terrain slot8 selects `BattleTerrain08`, decoded by `LoadStackCompressedData`
into the shared48×48 terrain array, separate from the32×30 encounter area.
Custom background8 is OVERWORLD255, so the scene-background consumer uses the
map-block fallback rather than one fixed battle background. Leader-presence8
is0; commandset LEADER on enemy128 does not install a leader-victory sweep.
The post-battlescene position table contains only5 and gives8 no override;
halved-EXP contains only1. Terrain tiles, reachability and scene rendering are
not newly decoded or observed here.

| Supplied state | Actual entry sequence |
| --- | --- |
| F88 clear, F458 clear | Clear PLAYER_TYPE/set399; clear seconds; save incoming map/battle, install current fields/base VInts; execute before8; restore saved fields and write them again; clear90–105; heal; initialize actual allies/enemies; clear AI memory; `LoadBattle(0)`; set458 before the empty start8 body; prepare round. |
| F88 clear, F458 set | Before/start bodies skip, while fresh healing/placement/enemy upgrade/refresh/load/round still run. F508 remains an independent completion/upgrade/neutral input. |
| F88 set | Restore saved seconds; clear88/AI memory/view target; load with incoming focus; jump into retained turn queue/cursor. No fresh current-field installation, flag reset, healing, formation, enemy upgrade, before/start or new round precedes that loop. A retained sentinel later requests round preparation. Valid saved fields/queue remain supplied inputs. |

Before-table8 at`0x47A98` checks458 without setting it. Start-table8 at
`0x47B3C` selects `ms_Empty`; the wrapper sets458 before dispatch. Empty script
termination can still wait for scrolling and clear scroll speed. F458 is neither
completion508 nor suspension88. The before body assigns no EGRESS_MAP; the
preceding Map14 caller sets14 only on its executed init path. With supplied
EGRESS_MAP14 and F399 set, the savepoint consumer selects Map14`(45,7)`, Down;
other stored maps change that result. Outcomes remain with their existing owner.

### Reused before scene versus battle rebuilding

**Confirmed:** the [complete before owner](./damaged-ship-plank-through-battle08.md#before-scene-program)
retains commands0–34 and all seven actual text keys. This recheck preserves its
actual `$C0,130` speaker on the confrontation, including the question about the
worker. Scene130 is the witch sprite, not the combatant later numbered130.
Loading Map72/view`(1,46)` and `ce_4AA14` rebuilds entities/followers; it does
not call ordinary Map72 setup/init. Main`(4,50)`, allies and workers are scene
declarations. F6 clear selects Kiwi's`setPos(63,63,Down)`; the handler skips its
payload for a zero-HP ally below30, while NPC aliases bypass that ally check.
Neither selection nor an effective relocation recruits, hides or kills Kiwi.
Follower mappings can replace fixed ally declarations; Slade has no fixed row.

Worker129's animation stop and waited motion-off/Left/sprite/Idle script assign
no HP. Three initially off-map creature actors are positioned, then witch130;
131/132 Jump installations are unjoined and133's Jump is awaited. Text, music,
camera and named action synchronization assign no enemy definitions, spawning,
force membership, inventory or damage. The interpreter's return tail is a
service boundary, not measured completion of every presentation operation.

After this return BattleLoop initializes actual combatants, then `LoadBattle`
calls `PositionBattleEntities`, clearing old scene mappings and rebuilding from
live combatant coordinates/mapsprites. Scene main`(4,50)` is not the formation
origin; local template0`(3,18)` corresponds to map`(4,50)` only after the battle
area's`(1,32)` offset. Apparent coordinate agreement is not identity or HP proof.

## Thirteen templates, real membership and neutral population

**Confirmed:** four header bytes +24 twelve-byte combatant records +four
twelve-byte regions +one two-byte point =342 bytes (`0x156`), matching
`0x1B3822..0x1B3978`. The getter advances by these actual sizes to each section.
Every ally row declares HEALER1/0, NOTHING127, NONE255/region15 twice, filler0,
STARTING0. Its complete ordered position selection is:

| Template | Local X,Y | Template | Local X,Y |
| --- | --- | --- | --- |
| 0 | `(3,18)` | 7 | `(1,17)` |
| 1 | `(3,19)` | 8 | `(1,19)` |
| 2 | `(2,19)` | 9 | `(1,20)` |
| 3 | `(3,17)` | 10 | `(1,16)` |
| 4 | `(2,17)` | 11 | `(2,16)` |
| 5 | `(2,18)` | 12 | `(1,15)` |
| 6 | `(1,18)` | — | — |

`InitializeAllAlliesBattlePositions` resets X for all30 ally slots, calls
`UpdateForce`, then scans ascending IDs with both joined flags0–29 and active
flags32–61. It copies only X/Y into actual living members, not template items,
AI, spells or identities. An ordinary dead member consumes a member/iteration
without advancing the placement cursor. Thus a living member after a dead one
gets the earlier unused position. Thirteen loop iterations are a local capacity,
not thirteen guaranteed placements; a fourteenth active entry reaches
`LeaveBattleParty` even if deaths left positions unused. That tail clears active
flags and X, retains joined flags, and does not rebuild the RAM lists again.
An empty party has no pre-loop safety guard establishing a valid natural start.

The [general membership consumer](./battle02-initialization-and-first-control.md#joined-active-dead-and-capacity)
is separate: `JoinForce` sets joined, rebuilds lists, and calls `JoinBattleParty`
only when the current active count is **below12**. At12 it leaves the incoming
member in reserve; it does not select thirteen because this spriteset has13
templates. Direct `JoinBattleParty` itself sets the active flag without that
capacity test. Already supplied flags can therefore differ from ordinary joining;
this is not proof such a thirteen-member state is naturally reached. No selected
before/start operation joins Kiwi or mandates any fixed party, class or level.

Healing scans all30 slots independently of membership. Living allies and
immortal Peter7/Lemon28 receive HP/MP maxima; other HP0 allies stay dead.
The intermediate `status & 7` retains existing STUN/POISON/CURSE, then
`UpdateCombatantStats` rebuilds current stats, preserves STUN/POISON and derives
final CURSE from equipped cursed gear. A historical CURSE bit alone does not
prove final curse. Live equipment/status, not the template, supplies current
movement/agility and later action eligibility.

All eleven enemy rows are STARTING0. Slots128–138 initialize immediately;
undeclared139–159 receive max/current HP0 and X=-1. The occupied-position test
in `ResetSpawningEnemyStats` is not a gate on ordinary mode0 initialization.
After optional type selection, `InitializeEnemyStats` copies the selected56-byte
definition; restores HP/MP; writes placement X/Y; combines its movement upper
nibble with the **placement** commandset; stores selected enemy ID, regions and
orders; handles added item; rebuilds activation; adjusts difficulty; refreshes.
Placement NOTHING returns before AddItem and does not remove definition gear.

The actual neutral table has one North Cliff record: local`(13,2)`, Down,
WORKER, `eas_LyingLeft`. On `PositionBattleEntities`, F508 clear admits it into
neutral combatant159 with max/current HP0, status0 and AIBITFIELD_NEUTRAL,
then declares its entity. Its map location becomes`(14,34)`, matching the prone
scene placement without proving persistent scene identity or an injury event.
F508 set skips this declaration; F458 alone does not. Suspended load uses the
same completion-gated consumer without fresh enemy initialization. Neutral159
is outside the first30-enemy turn scan and also has HP0; it is not a twelfth
live enemy, a joined worker or a source of region activation.

## Eleven enemies, definitions and action operands

**Confirmed:** every enemy has placement NOTHING127, filler0 and STARTING0.
All secondary orders are NONE255, with the distinct secondary regions below.
IDs128–138 are battle combatant slots, not before-scene NPC aliases.

| Row / slot | Base type / local X,Y | Commandset | Primary order / region | Secondary region |
| --- | --- | --- | --- | --- |
| 0 /128 | HUNTER_GOBLIN57 `(11,5)` | LEADER14 | MOVE_TO\|0 (`$40`) /0 | 1 |
| 1 /129 | WITCH68 `(10,6)` | ATTACKER3/9 | FOLLOW_ENEMY\|0 (`$80`) /0 | 1 |
| 2 /130 | GOBLIN4 `(17,8)` | ATTACKER2/7 | FOLLOW_TARGET\|0 (`$00`) /2 | 3 |
| 3 /131 | GREEN_OOZE5 `(19,9)` | ATTACKER2/7 | FOLLOW_TARGET\|0 (`$00`) /2 | 3 |
| 4 /132 | HUNTER_GOBLIN57 `(22,18)` | ATTACKER2/7 | FOLLOW_TARGET\|0 (`$00`) /1 | 2 |
| 5 /133 | GOBLIN4 `(16,14)` | ATTACKER2/7 | FOLLOW_TARGET\|0 (`$00`) /0 | 1 |
| 6 /134 | GREEN_OOZE5 `(9,21)` | ATTACKER1/6 | NONE /15 | 15 |
| 7 /135 | WITCH68 `(10,21)` | ATTACKER1/6 | NONE /15 | 15 |
| 8 /136 | GOBLIN4 `(11,22)` | ATTACKER1/6 | NONE /15 | 15 |
| 9 /137 | GOBLIN4 `(7,10)` | ATTACKER1/6 | NONE /15 | 15 |
| 10 /138 | HUNTER_GOBLIN57 `(8,12)` | ATTACKER1/6 | NONE /15 | 15 |

`GetMoveOrders` returns the high/low order bytes unchanged. `Set/GetTriggerRegions`
pack/unpack nibbles independently. `GetMoveOrderPosition` tests order bit6:
`$40` selects point0`(11,5)`; `$80` reads live enemy128; `$00` reads live
ally0/Bowie. The point currently matches128's starting coordinates but remains
a fixed point when128 moves. No following order refers to the worker or the
scene's witch alias. Region15 and order NONE255 are separate sentinels.

| Base definition | Level; HP/MP; base ATT/DEF/AGI/MOV | Movement; equipped item; resistance; original AI word |
| --- | --- | --- |
| HUNTER_GOBLIN57 | 7;24/0;8/12/14/5 | STEALTH_ARCHER; IRON_ARROW46;0; PRIORITYMOD_2\|BIT4 |
| WITCH68 | 7;21/10;9/12/15/6 | MAGE; BRONZE_ROD87; ICE_MINOR; PRIORITYMOD_2\|BIT4 |
| GOBLIN4 | 6;18/0;14/13/13/5 | REGULAR; SHORT_AXE33;0; PRIORITYMOD_2\|BIT5 |
| GREEN_OOZE5 | 5;17/0;18/12/12/4 | REGULAR; NOTHING; ICE_MAJOR\|FIRE_WEAKNESS; PRIORITYMOD_2\|BIT5 |

All four have REGULAR spell power, initialStatus NONE and raw unknownByte equal
to their listed type ID; no extra role is assigned to that label. Equipped
types have three additional NOTHING items; Ooze has four. Witch has FREEZE1
then three NOTHING spell slots; the others have four NOTHING. Goblin/Ooze/Witch
prowess is CRITICAL150_1IN32/DOUBLE_1IN32/COUNTER_1IN32; Hunter instead has
DOUBLE_1IN16/COUNTER_1IN16 with the same critical declaration. None of these
four base AGIs carries the128 extra-turn bit. These are declarations, not an
observed double/counter/cast.

Activation reconstruction keeps the definition's high priority nibble$2000
with zero mode/filler; original BIT4/BIT5 do not survive as initial active bits.
Iron Arrow is WEAPON, ATT+12/range2–2; Bronze Rod ATT+8/range1–1; Short Axe
ATT+5/range1–1. All useSpell NOTHING. Difficulty refresh applies
floor(baseATT×5/4) once on every non-SUPER difficulty and twice on SUPER,
then equipment. At difficulty0, unupgraded current ATT is22 Hunter,19 Witch,
22 Goblin,22 Ooze. That derived ATT is not inflicted damage or a natural
difficulty choice. Repeat-selected definitions must be refreshed anew.

FREEZE1 is silence-affected TYPE_ATTACK, MP3, range1–2, radius0, power10.
Witch's10MP supplies the cost. `GetNextUsableAttackSpell` and highest-usable-level
selection admit it when their status/MP predicates hold; attack AI then needs
a target, priority and reachable casting position before publishing CAST_SPELL.
Earlier move-order success or standby can bypass that selection. No first cast
or complete damage/resistance/action audit is claimed. HEAL1/2/3 command names
are AI command parameters, not spells newly installed on these enemies.

Retain the [actual item-getter caveat](./battle05-initialization-and-first-control.md#eight-enemies-equipment-and-available-action-operands):
useSpell NOTHING63 is not the erroneous255 no-use comparison; the spell getter
falls back to HEAL. Attack-item selection rejects that non-attack type;
healing-item selection skips these non-Rain weapons with AI-use bit13 clear.
Equipped items remain necessary physical-range/stat inputs, not usable potions.
An upgraded type may supply different spells/items/action families.

## F508 and actual repeated-entry upgrades

**Confirmed:** NORTH_CLIFF8 is the second member of the actual length-prefixed
random-battle list. `IsBattleUpgradable` succeeds; `DetermineBattleUpgrade`
tests **F508**, not458/408. F508 clear returns the base type without upgrade RNG.
F508 set can substitute each type even after `CheckBattle` clears408 on that
selected call. It neither promises a subsequent reentry nor proves the player
naturally replayed this battle. The [complete shared algorithm](./battle14-desert-encounter-and-outcomes.md#completed-repeats-and-actual-enemy-upgrade-selection)
retains general word/byte arithmetic and the unsafe airborne exclusion scan.
For Battle08 the actual category bindings are:

| Original movement / selected enemies | Range R | Inclusive type bounds | Excluded IDs |
| --- | --- | --- | --- |
| REGULAR /Goblin4, Green Ooze5 | 5 | OOZE0..REAPER35 | KRAKEN_LEG10, PAWN13, KNIGHT14, RAT15, BURST_ROCK32, ZEON_GUARD38 |
| STEALTH_ARCHER /Hunter57 | 2 | GALAM_ARCHER56..DARK_GUNNER65 | KRAKEN_ARM59, ROOK61 |
| MAGE /Witch68 | 1 | GALAM_MAGE67..CHAOS_WIZARD74 | QUEEN71 |

The category comes from the **original** type's movement upper nibble, not its
placement commandset, upgrade's resulting movement or current target. Bowie0's
level supplies E; signed-byte class>11 adds20. Compute `E−8`; nonpositive values
repeat the same read once using D4, then fall back. Positive values divide by10
and retain quotient low byte q. q0 returns base, so ordinary inputs first reach
selection at **E18**. Unsupported/corrupt class/level values are not normalized
into valid party state by this routine.

D3 starts at base type. For R5 its offset is`5q−5`; for R1/2 the offset is q.
That sum is saved in D2 as the retry baseline. RNG range R+1 supplies an
additive value0..R. Bounds are signed-byte comparisons; an out-of-bounds or
excluded candidate consumes the sole D4 retry: reset to D2, draw again, then
recheck. A second failure returns base; it is not clamping or unlimited reroll.
Even fallback draws advance shared RANDOM_SEED. At q1, pre-filter candidates
are4..9 for Goblin,5..10 for Ooze,58..60 for Hunter and69..70 for Witch;
Ooze10 and Hunter59 are excluded. This is a candidate-domain example, not a
probability distribution, selected result or guarantee of stronger statistics.

All eleven initialization calls occur in slot order128–138, so earlier draws
change later upgrades and the round seed. The actual replacement occurs before
the56-byte definition copy and enemy-index write. It changes HP/MP/stats,
equipment, spells, movement, resistance/prowess and priority; placement X/Y,
commandset, orders and regions remain this spriteset's operands. It is not an
in-place level increment on already loaded enemies, and does not repeat each
round. The F88 suspended branch skips these calls entirely.

No selected base type enters airborne/healer selection. The airborne table's
count0 underflows its unchecked exclusion scan; preserve that defect at the
shared/Battle03 owners without projecting it onto these three nonempty lists
or inventing safe behavior for arbitrary replacements. High/invalid values can
still wrap signed-byte comparisons or return base; no natural level/seed or
all possible upgraded loadout acceptance is established.

## Four sloping regions and activation lifetimes

**Confirmed:** all regions declare type4, second byte0, trailing0/0. Their
vertices are ordered, not bounding rectangles:

| Region / persistent flag | Vertex1 /2 /3 /4 |
| --- | --- |
| 0 /90 | `(8,3)` /`(0,11)` /`(7,18)` /`(15,10)` |
| 1 /91 | `(16,11)` /`(8,19)` /`(18,29)` /`(27,16)` |
| 2 /92 | `(11,0)` /`(2,9)` /`(10,16)` /`(18,7)` |
| 3 /93 | `(21,6)` /`(12,15)` /`(22,25)` /`(27,16)` |

`IsRegionTriggered` tests triangles1/2/4 then3/2/4. It scans all30 allies with
positive HP and nonnegative signed-byte X/Y, not enemies or neutral actors.
The actual segment helper chooses horizontal dominance on equal spans, uses
signed MULS/DIVS with truncation toward zero, projects at the test coordinate,
then synthesizes two bits from Z/C. In-bounds collinearity returns01 and is an
immediate triangle hit; ordinary side flags must match the opposite vertex at
each edge. An exception can skip an early edge but rejects at the third.
Negative projection explicitly writes CCR=1 before synthesis and yields10;
comments suggesting all negative projections are11 are not the instruction
contract. This is not a general exact cross-product polygon test.

Direct substitution of **all13** fresh template positions into those branches
rejects both triangles of every region. The finite rejection witnesses are:

| Region | Triangle1/2/4 rejection | Triangle3/2/4 rejection |
| --- | --- | --- |
| 0 | Third tested edge4→2: ally10 versus vertex1's00 | First edge3→2: ally10 versus vertex4's00 |
| 1 | First edge1→2: ally00 versus vertex4's10 | First edge3→2: ally10 versus vertex4's00 |
| 2 | Third tested edge4→2: ally10 versus vertex1's00 | First edge3→2: ally10 versus vertex4's00 |
| 3 | First edge1→2: ally00 versus vertex4's10 | First edge3→2: ally10 versus vertex4's00 |

All placed living allies use a prefix/subset of these positions even with dead
members consuming iterations; fresh initialization resets unplaced X to-1.
Thus no fresh F90–93 hit follows from this selection. More nominal templates
do not change that; suspended/externally supplied positions are separate states.
This finite result does not establish integer/cross-product equivalence at
arbitrary edges or prove a naturally reachable crossing.

`ActivateEnemies` visits32 enemy slots and falls through the update helper;
living placed enemies call `TriggerRegionsAndActivateEnemies`. Its tested word
NEWLY_TRIGGERED_BATTLE_REGIONS differs from persistent F90–105. Every tested
region sets its cache bit even on a miss; an inherited bit can skip testing.
Fresh F90–105 clear and `ClearAiMemory` do not explicitly clear this word;
non-swarm `StartAiControl` clears it later. `ClearAiMemory` resets48 last-target
bytes to-1 and48 memory bytes to0, not RNG or all combatant state.

Primary-region success sets activation bit0 and wins the branch before the
secondary test. Secondary-only success sets bits0/1. The primary branch retains
an old secondary bit; it does not reset prior secondary state. For128/129/133,
region0 has primary priority over1; for130/131,2 over3; for132,1 over2. A later
primary hit can therefore leave bit1 set after secondary activation. These
flags persist until an explicit clearing consumer; region15 is never one of
the four declarations.

Activation does **not** itself replace primary move orders with secondary NONE.
`GetMoveOrders` returns stored bytes, and these move-order consumers read the
primary byte. Secondary-only activation therefore still admits the stored
point/follow behavior. `StartAiControl` promotes secondary and clears it only
when the primary is a non-point live-combatant reference whose current HP is0.
Witch129 then changes `$80/NONE` to NONE/NONE if enemy128 dies;130/131/132/133
do likewise if ally0 dies. Point-bound128 bypasses that HP check. This is a
state-dependent consumer fact, not an instruction to interpret NONE as a new
region, fixed point, or automatic secondary-order switch.

## Standby, round preparation and first dispatch

**Confirmed:** there is no Battle08 region-program row in the sparse32/40
table. All declared modes are0; undeclared slots and neutral159 add no hidden/
respawn candidate to `PopulateTargetsListWithSpawningEnemies` at this fresh
boundary. Region activation adds no selected dialogue, summon or upgrade call.

The six region-assigned enemies128–133 start inactive when fresh flags are
clear. Their primary orders are all non-NONE with non15 primary regions, so
`ValidateAiStandbyEligibility` returns d1=1. The caller treats nonzero as stop:
empty move string/STAY. Its earlier thinking-RNG outcomes also stop. The
misleading helper comment does not reverse this actual branch. If changed
orders become NONE while regions remain assigned, d1=0/d2=0 can instead permit
conditional local standby movement, still STAY; assigned region alone does
not prove immobility. The five15/15 enemies134–138 bypass inactive standby
regardless of activation bit0 and enter their commandset immediately.

| Commandset / selected users | Commands attempted in order | Preparatory mode |
| --- | --- | --- |
| HEALER1/0, ally templates only | HEAL1, SUPPORT, ATTACK1, MOVE1, STAY | Regular; not installed by ally placement |
| LEADER14 /128 when active | MOVE_ORDER5, ATTACK2, HEAL3, SUPPORT, MOVE_ORDER2, MOVE3, STAY | Block-and-carve |
| ATTACKER3/9 /129 when active | MOVE_ORDER3, ATTACK3, HEAL2, SUPPORT, MOVE_ORDER4, MOVE2, STAY | Block non-movable |
| ATTACKER2/7 /130–133 when active | MOVE_ORDER1, ATTACK1, HEAL1, SUPPORT, MOVE1, STAY | Regular |
| ATTACKER1/6 /134–138 | ATTACK1, HEAL1, SUPPORT, MOVE1, STAY | Regular |

First successful command stops the scan. MOVE_ORDER5 is target-type0 with
block-and-carve; MOVE_ORDER2 is follow-type2 with the same mode, but both path
builders still decode128's stored `$40` through `GetMoveOrderPosition` to
point0. MOVE_ORDER3 is follow-type1/block-non-movable;4 is target-type0 with
that mode, both resolving129's stored `$80` to enemy128 while alive. MOVE_ORDER1
is target-type0/regular, but130–133's `$00` still resolves to ally0. Command
algorithm mode does not redefine the encoded destination. AI-controlled allies
select set6, independently of HEALER1 template declarations. Reachability,
obstructions, occupancy, refreshed movement, targets and RNG still affect results;
no guaranteed pursuit, initial attack or winning tactic follows.

At `BattleLoop/@Start`, activation → region-program dispatch → spawn selection
precedes `GenerateBattleTurnOrder`. The [shared round rule](./battle02-initialization-and-first-control.md#first-round-and-conditional-control-endpoint)
clears64 sentinel slots, scans30 allies then first30 enemy slots, admits living
placed combatants regardless of activation, uses refreshed current AGI/main RNG,
performs62 signed-byte stable-sort passes and sets cursor0. Unupgraded Battle08
contributes eleven ordinary enemy entries, not thirteen template entries or a
neutral turn. Changed ally state or upgraded definition AGI can change entries.
Each ordinary score uses AGI + RNG(AGI>>3) − RNG(AGI>>3) + RNG(3) −1;
AGI's128 bit adds the separate two-draw second score. Range0 still advances RNG.
No seed reset occurs; executed before text waits can advance main RNG, and
successful or failed upgrade draws precede this round. Standby uses the separate
thinking RNG. No fixed first actor or reproducible natural round is asserted.

A retained sentinel requests a new round; a HP0 actor skips. `ExecuteIndividualTurn`
first gives MUDDLE/AI-controlled bit2 priority to AI preparation. Otherwise enemy
opponent-control=0 selects AI, nonzero selects player preparation; ally auto-battle
independently selects AI. Shared cursor/view/status-window/grid work follows.
SLEEP/STUN then select NoAction; the dispatch predicates repeat with the same
MUDDLE/bit2 priority and opponent-control polarity before AI execution or player
input. Stop at that conditional boundary, including no-action, before an
individual action or a guaranteed player-ready state. F88 entry can start at
a retained queue position instead of this fresh round.

## Current conversion and production support

**Confirmed by accepted production-code inspection, separate from original
evidence:** Battle08 is not admitted, converted or supported by the current
private encounter route. [Trust](../../remake/docs/content/profiles-and-trust.md#selected-third-encounter)
and [Application](../../remake/docs/application/session-and-programs.md#yeel-guide-zone-request-and-sleep-continuation)
retain selected2/3 consumption. PR692 is unconsumed. No preparation, lowering,
comparison or engine execution is performed here.

| Actual consumer | Reusable mechanism and explicit Battle08 boundary |
| --- | --- |
| [prepare_encounter / compile_routes](../../src/sf2tool/remake_exploration_battles.py) | Producer/CLI admit2/3 only, with selected headers and pinned source/ROM checks. Route compilation can bind supplied hooks/load and400/450/500 flags, but exports no8 or its before scene. |
| [PrivateBattleEncounterReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs) / [PrivateBattleDefinitionReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleDefinitionReader.cs) | Generic parsing retains placement/regions/points and metadata; trusted pins admit1/2/3, not8. Definitions select declared base enemy codes and referenced items/spells. This is no F508/effective-level/category/RNG type-selection implementation. |
| [PrivateBattleScenarioReader.Assemble](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs) / [PrivateBattleActionBindings](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleActionBindings.cs) | Independent region-program admission1/2/3 rejects8. Sets0/6/7/9/14 and point/live-follow expressions are representable. REGULAR supports Goblin/Ooze; mover lacks STEALTH_ARCHER/MAGE. Enemy AI-word regex rejects Hunter/Witch BIT4. Hunter double/counter prowess rejects; Iron Arrow2–2 rejects adjacent-weapon binding. Bronze Rod/Short Axe adjacent range alone is expressible, but initialization/AI still reject equipped loadouts. Witch FREEZE data is not executable attack-spell support; assembled executable spells bind HEAL/healing items. Controlled classes0/1/2/4 do not cover Kazin/Slade/Kiwi or arbitrary party state; thirteen templates require none of those identities. |
| [BattleEntry](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs) | Before/load/start, flags and actual membership rebuild are reusable; carried resources/growth/gold/both seeds feed starts. Missing member definitions reject. F88 rejects instead of restoring an original queue/cursor. Completed-entry policy does not supply original508 upgrade selection; no admitted8 content reaches this path. |
| [BattleInitializationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleInitializationRules.cs) / [BattleTurnFlow](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleTurnFlow.cs) | Controlled already-refreshed allies, roster-only/difficulty0, living/immortal healing, formation and enemy activation/ATT calculation exist. Excess active count rejects instead of source tail eviction. General equipment/CURSE refresh, other difficulty/control policies, saved state and Battle08's neutral population are Unsupported/unbound. Enemy items allow0/127, rejecting selected equipped weapons. |
| [BattleActivationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActivationRules.cs) / [TurnOrderRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/TurnOrderRules.cs) | Mode0/RegionProgram.None, living placed ally tests, primary-first/sticky-secondary activation and randomized/extra round entries are reusable. Cross-product geometry and a fresh per-round tested word do not prove equality to original integer projection/cache lifetime for arbitrary state. No Battle08 equivalence observation exists. |
| [SourceEnemyAi](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceEnemyAi.cs) / [SourceMoveOrderAi](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceMoveOrderAi.cs) | Standby,15/15, commandsets6/7/9/14, point/live-follow and dead-target promotion are reusable. Activated AI admits only0/127 item words and no learned spells, rejecting equipped Hunter/Goblin/Witch and FREEZE. Adjacent physical station search is not general ranged/attack-spell AI. Plain Ooze's simpler loadout does not admit the whole encounter or arbitrary upgraded types. |

These are bounded support requirements at actual consumers, not an allowlist
patch or a new implementation task. Existing source projection/cache and refresh
limits remain explicit; this reading adds no new engine-defect acceptance claim.
Remake code cannot prove an original-game fact.

## Unknowns, retained failures and reproduction

**Unknown:** natural Map72 admission, actual joined/active/dead cohort and
capacity history, class/level/difficulty/equipment/status/RNG, actual substitutions,
natural region crossings, complete action/target/tactical legality, uninterrupted
continuation, presentation/hardware timing and save durability. Thirteen templates
do not establish a fixed party; the staged worker does not establish HP damage.
Selected completeness ends at first conditional dispatch. Existing outcome and
New Granseal owners keep their accepted scope and are not reopened.

Accepted Battle01 original admission/victory/stable-return/later-Down observations
remain intact. The #1047 twelve **completed unresolved comparisons** remain at
the [verification owner](../../remake/docs/verification/field-programs.md#yeel-guide-through-battle03),
not interrupted suites or an automatic acquisition/repair queue. Preserve the
intermediate-status/equipment-CURSE correction. PR1221's initial mechanical PASS
missed a stale coverage summary/denominator and opponent-control polarity; its
two P2 findings and corrected semantics remain in the
[correction handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1220#issuecomment-6354981415).
Mechanical checks cannot replace semantic review.

Reproduce by reading the complete named selected source/consumer bodies at the
pin. Check all24 combatant records, four ordered regions, point0 and all four
base definitions; follow F508 and each category/list consumer through the actual
copy/refresh. Substitute all13 fresh coordinates into the signed projection/CCR
branches above, preserving the early-hit and exception paths. Read the actual
production admission/binding/rejection bodies separately. No broad parser,
fixture or runtime gate is necessary for this prose claim.

Direct checks cover links/anchors, UTF-8/LF, explicit three-path `origin/main`
diff including committed head, ownership/private boundary, unchanged fixture
counters and four frontiers. Retained map-init/map-script outputs total
18,285,979 bytes; they and `local/issue1005/pair.txt` remain unchanged. Only small
fresh ignored receipts go under `local/issue1226/`; reuse the existing simple
document method, count relative/external references separately, and impose no
source-code line ceiling on prose. Linear source reads and52 finite geometry
substitutions use one process and small memory/output; no bulk run is planned.
Replan only for a concrete required acquisition or cross-system expansion.

On a clean committed head load current ignored selections in the same process:

```powershell
. ./local/private-inputs.ps1
uv run sf2 verify plan --base origin/main --head HEAD
```

Record actual exact-head scoped CI and any completed failures in the handoff.
No normal/full, SDK/Godot/emulator/H3/H4, screenshots, comparison/lowering,
extraction or acquisition is selected. Stop with clean pushed frozen Draft PR,
Review / Verify handoff, stopped writer/processes and completion notification
to the assigned main-gate. Independent acceptance and merge remain with it.
