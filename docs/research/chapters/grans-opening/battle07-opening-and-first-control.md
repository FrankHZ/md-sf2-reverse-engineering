# Battle07 opening, initialization and first control

Evidence date: 2026-10-22. **Confirmed:** Battle07's complete7/10/4/1 selection
connects the Galam/Astral confrontation, seven placement templates, ten enemies
and five conditional neutral actors to initially untriggered regions and
conditional individual control. Dark Smokes have numerical AGI13 plus a second
turn, DISPEL1 and special prowess; scene summoning does not implement spawning.
**Inferred:** the staged confrontation frames the ensuing encounter as a rescue
attempt. **Unknown:** natural entry/party/status/RNG, first actor, all actions,
presentation/timing and persistence. This is static composition.

## Scope and provenance

This is the **Standard**-rigor agreement for
[Issue1224](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1224),
under [Epic1053's finite denominator](../../story-content-coverage.md#scope-and-reading-rules).
The bounded outcome supports content maintenance and conversion review: explain
the complete selected opening through first player/AI/no-action dispatch,
including changed valid party state. Reuse the
[Battle02 lifecycle](./battle02-initialization-and-first-control.md) and the
[Battle06 account](./battle06-opening-and-first-control.md). The
[admission owner](./battle06-to-battle07-content-route.md) retains occupied
Granseal, optional Kiwi, tower travel and Map58-to59 switching. The
[outcome owner](./battle07-outcomes-and-aftermath-handoff.md) retains all returns,
the long after scene and later earthquake connection. Stop before an executed
individual action; those surrounding scenes are not duplicated here.

Owned writes are this document and Battle07 narratives in the two coverage
owners. Sources, consumers, fixtures, registries, counters and runtime frontiers
are read-only. No parser, fixture, schema, platform, allowlist, implementation,
extraction or acquisition is added. Acceptance is direct semantic/document/
private-boundary review, committed-head planner and actual exact-head scoped CI,
then a clean pushed frozen Draft PR for independent acceptance.

Original paths below are relative to `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The selected checkout was clean at that pin. Accepted repository base is
`d7409e1213f4218a865f027fc3711e355700e640`, tree
`23591b657ce998d4612d259d3e1d254a7b6e719a`. The
[USA baseline](../../../../README.md#pinned-original-baseline) retains input identity;
no fresh ROM-equivalence claim is made. Complete selected records/functions and
return tails, rather than comment meanings alone, supply the facts. Short battle
paths below resolve under `code/gameflow/battle/`; `j_` interfaces resolve to the
named implementations.

| Claim | Declaration and actual consumer |
| --- | --- |
| Complete formation/behavior | `data/battles/spritesets/entries.asm` slot7 → `spritesets/spriteset07.asm:BattleSpriteset07`, `0x1B3720..0x1B3822`; `sf2macros.asm:allyCombatant/enemyCombatant/combatantAiAndItem/combatantBehavior`, `sf2enums.asm`; `getbattlespritesetsubsection.asm:GetBattleSpritesetSubsection`, `0x1B1630`. |
| Admission/metadata | `code/common/maps/getbattle.asm:CheckBattle`, `0x799C`; `code/gameflow/mainloop.asm:MainLoop`; `data/battles/global/battlemapcoords.asm` row7, `terrainentries.asm` slot7, `global/custombackgrounds.asm`, `enemyleaderpresence.asm`, `halvedexpearnedbattles.asm`; `battleloop/loadbattleterraindata.asm`, `battlescenes/getbattlescenebackground.asm`, `battleactions/giveexpandgold.asm`. |
| Complete before/text | `data/battles/entries/battle07/cs_beforebattle.asm:bbcs_07/ce_49F36`, `0x49CE2..0x49F7E`; `data/scripting/text/gamescript.txt` keys0935–0950; `sf2cutscenemacros.asm`; `code/common/scripting/map/mapscriptengine_1.asm`, `0x46506..0x47102`, and `_2.asm`, `0x4712C..0x474FC`; `code/common/scripting/text/textfunctions_1.asm:DisplayText/ParseSpecialTextSymbol`. |
| Scene identity and action execution | `mapfunctions.asm:InitializeMapEntities`, `0x440CC`, `followersfunctions_1.asm:InitializeFollowerEntities`, `0x44298..0x44338`, under `code/common/scripting/map/`; `data/scripting/entity/followers.asm`, `eas_actions.asm`, `eas_main.asm:eas_Idle`; `mapscriptengine_1.asm:GetEntityAddressFromCharacter/AdjustScriptPointerByCharacterAliveStatus`; `code/common/scripting/entity/entityscriptengine_2.asm` selected action handlers. |
| Entry/party/refresh/load | Before/start wrapper start/end files under `cutscenes/`, corresponding tables under `data/battles/cutscenes/`; `code/common/scripting/map/ms_empty.asm`; `battleloop_1.asm:BattleLoop`, `0x23A84`; `battleloop/heallivingandimmortalallies.asm`, complete `initializecombatants.asm`, `0x1B1272..0x1B15F8`; `code/common/stats/battleparty.asm:UpdateForce/LeaveBattleParty`, `updatecombatantstats.asm`; `battlefunctions/loadBattle.asm`. |
| Combatants and neutral population | `data/stats/enemies/enemydefs.asm` records3/56/67/76/86, `data/stats/items/itemdefs.asm` records45/72/86, `data/stats/spells/spelldefs.asm:HEAL/BLAZE/DISPEL`; `data/battles/global/battleneutralentities.asm` row7 and `code/common/scripting/entity/entityfunctions_1.asm:PositionBattleEntities`, lines266–482. |
| Orders/actions | `ai/startaicontrol.asm`, `executeaicommand.asm`, `command/heal.asm`, `attack.asm`, `support.asm`, `support/prioritizetargetsforsupportspells.asm:PrioritizeTargetsForSupportSpell_Dispel/CalculateTargetPriorityForSpell_Dispel`, `command/moveorder.asm`, `command/moveorder/buildmovestringformoveorder.asm`, `getnextuseableaiaction.asm`, `gethighestusablespelllevel.asm`; `data/battles/global/aicommandsets.asm`; `battlefield/getmoveorderposition.asm`; `code/common/stats/combatantstats_1.asm:GetMoveOrders/GetTriggerRegions`, `combatantstats_2.asm:SetTriggerRegions`. |
| Upgrade/activation/standby | `battleloop/upgradeenemies.asm:IsBattleUpgradable/UpgradeRandomBattleEnemies`, `data/battles/global/randombattles.asm`; `battleloop/clearaimemory.asm`, `activateenemies.asm`, complete `triggerregions.asm`, `populateenemyspawns.asm`; both `cutscenes/regionactivatedcutscenes.asm` and its data table; both `ai/determineaistandbymovement_*.asm`. |
| Round/control endpoint | `battleloop/turnorderfunctions.asm`, `0x25544`; `code/common/tech/randomnumbergenerator.asm:GenerateRandomNumber`; `battlefunctions/executeindividualturn.asm:ExecuteIndividualTurn`, `0x23EB0..0x24242`, through its control predicates and NoAction return. |

Shared [spriteset](../../battle/battle-spriteset-data.md), [global](../../battle/battle-global-data.md),
[routing](../../battle/battle-routing-data.md), [loop](../../battle/battle-loop.md), [AI](../../battle/battle-ai.md)
and [cutscene](../../battle/battle-cutscenes.md) owners retain their inventories and observations.
Existing H2 fixtures `battle-spriteset-data-static-v1.json`,
`battle-global-data-static-v1.json`, `battle-routing-data-static-v1.json`,
`battle-loop-static-v1.json`, `battle-control-static-v1.json`,
`battle-ai-static-v1.json` and `battle-cutscenes-static-v1.json` are unchanged.
Their `rowContentTracked:false` / `routeTargetsParsed:false` limits do not become
executable proofs of this selected composition; the source route above owns it.

## Selection and three entry lifetimes

**Confirmed:** `BATTLE_VERSUS_DARK_SMOKES=7` selects row7 (`0x7A67`): Map58,
origin`(0,0)`, area28×38, wildcard triggers255/255. `CheckBattle` scans in table
order and tests effective map, unlock and trigger coordinates; area is output,
not an inside-rectangle test. F407 is supplied by the predecessor's executed
Battle06 before-program, not by Battle06 victory or this before-program.
F407/F507 both set clears407 but still returns7 on that call. F457 is the
intro gate, F507 completion and F88 suspended entry. Map58 alone does not arm7.
The predecessor retains `SwitchMap`'s F999 replacement of requested58 with59
before battle lookup; that different effective map is not this row.

Terrain slot7 selects `BattleTerrain07`, a Stack-decoded48×48 terrain array,
separate from the28×38 area. Background row7 is TOWER_INTERIOR9, a fixed
background, not the OVERWORLD255 map-block fallback. Leader-presence row7 is0;
the defeated wrapper's leader-death sweep is absent. The post-battlescene
position table contains only5, so supplies no7 override; half-EXP selects only1.
Dark Smoke128's first slot and Galam's scene prominence do not define a leader
victory rule. Later outcome consumers remain at the outcome owner.

| Supplied state | Selected sequence |
| --- | --- |
| F88 clear, F457 clear | Clear PLAYER_TYPE/set399; clear seconds; save incoming map/battle, install current fields/base VInt, execute before7; restore supplied fields; clear90–105; heal; initialize actual allies/enemies; clear AI memory; load focused on0; set457 and execute empty start; prepare round. Before slot7 (`0x47A96`) checks457 without setting it; start slot7 (`0x47B3A`) sets457 before dispatch. |
| F88 clear, F457 set | Both hook bodies skip; fresh healing/placement/enemy refresh/load/round still run. No new F83 clear follows from the skipped before. F507 is not the intro skip gate. |
| F88 set | Restore saved seconds, clear88/AI memory/view target, load using incoming focus and enter retained queue/cursor. No fresh current-field installation, region reset, healing, placement, hook or new round first. A saved sentinel later requests round preparation. Supplied saved fields/queue are necessary; durability is not proved. |

Start7 selects `ms_Empty` (`0x47A4E`), whose terminator conditionally waits for
scrolling and clears scroll speed. There is no separate start scene. Before7
assigns no `EGRESS_MAP`; the predecessor retains conditional Map16 egress state.
With incoming EGRESS_MAP16 and F399 set, the ordinary savepoint is Map16
`(27,7)`, Up. Other incoming state changes that return. No fixed natural escape
state or continuous itinerary is inferred.

## Complete before program and text consumers

**Confirmed:** `bbcs_07` sets cursor2357, clears F83, requests Map58/camera
`(8,29)` and loads `ce_49F36`. `csc10_toggleFlag` passes83 to ClearFlag without
the400 offset used by `setStoryFlag`. `csc37_loadMapAndFadeIn` falls through
`csc48_loadMap`; the camera operands are not a player warp. `csc42_loadMapEntities`
reads the main tuple and calls `InitializeMapEntities`: clear old entities,
rebuild flag-selected followers, declare scene entries, then main entity0.
F83 clear suppresses that flag's king/minister follower rows in this rebuild;
other follower flags remain inputs. It does not recruit or dismiss force members.

| Reference in this scene | Declaration / role |
| --- | --- |
| Bowie /0 | Main`(13,35)`, Up; initialized with a wait. This tuple does not fix the later actual party formation. |
| 128 /129 /130 | Evil Galam`(13,17)`, Down; Astral`(13,21)`, Up; Elis`(12,14)`, Down. |
| 131 /132 | King Granseal`(12,35)`, Up; minister`(14,35)`, Up. The minister has no selected dialogue/movement command beyond the declared Init script. |
| 133 /134 /135 | Off-map`(63,63)`: GALAM_MAGE, Up; GALAM_ARCHER, Down; GALAM_KNIGHT, Down. Later positioned`(12,18)`, Up /`(14,18)`, Down /`(15,18)`, Down. These mapsprites are neither DARK_SMOKE nor enemy-definition declarations. |

These are character aliases resolved through entity mappings, not persistent
combatant identities. Battle enemy128 later denotes a Dark Smoke, not Galam;
battle133 denotes a knight, not scene133's mage sprite. The complete ordinary
unskipped operation sequence, grouped at scene boundaries, is:

1. After the load, wait for Bowie's Init; SLIDE_OUT133; fadeInB; camera speed$28;
   show2357. Set camera`(8,15)`; show2358–2360; camera`(8,13)`; show2361.
   Elis moves Down1 with wait; wait40; Galam faces Up; wait20; Elis jumps with
   wait; wait30; deactivate her auto-facing with wait; move Up1 with wait.
   Galam and Astral face Down; show2362–2364; camera`(8,15)`; Astral faces Up;
   show2365–2367. Galam moves Down1 with wait; show2368; Astral moves Up1 with wait.
2. Galam BumpDown with wait; quake2/LIGHTNING_1; Astral BumpUp with wait;
   quake0/wait50. Galam BumpDown asynchronously; quake3/LIGHTNING_2; Astral
   BumpUp with wait; quake0; Astral Jump asynchronously; nod Galam. Astral
   BumpDown with wait; quake2/DOOR_OPEN; Galam BumpUp with wait; quake0/wait40.
   Galam BumpDown asynchronously; quake3/HIT_2; Astral BumpUp with wait;
   quake0; Galam Jump asynchronously; nod Astral. Deactivate auto-facing for
   Galam then Astral with waits; wait30; Galam Up1 asynchronously and Astral
   Down1 with wait; show2369–2370; stop both animations.
3. Both receive separate waited inline scripts: motion OFF, orientLeft,
   updateSprite, jump Idle, inline end marker. Face Galam Up; show2371–2373;
   camera`(8,13)`; show2374. Elis receives waited inline speed48/48 → Idle;
   move Down3 asynchronously; wait10; position133`(12,18)`, Up; SLIDE_IN133;
   wait for Elis Idle; show2375; Elis Jump with wait; show2376; move Up2 with wait.
4. Camera`(8,15)`; show2377–2379; shiver Galam; stop his animation; show2380.
   Position134`(14,18)`, Down; wait40; position135`(15,18)`, Down; face133 Down;
   show2381–2384. Jump133 and134 asynchronously, Jump135 with wait, then `csc_end`.

All quake operands are direct amplitudes2/3/0, with final0. Sound requests,
bumps, falls/poses and the summoning dialogue write no battle HP, force join,
spawn mode, inventory or new battle unlock. No branch, call to a battle action,
warp or field-input interval appears in this before body.

`csc14_setEntityActscriptManual` installs inline code, optionally waits for
`eas_Idle`, then scans through$8080. `csc15_setEntityActscript` and
`csc16_waitUntilEntityIdle` use the same Idle-address synchronization;
`csc2D_entityActionSequence` compiles moveUp/Down into relative destinations,
and `loc_46928` appends the Idle jump, installs it and applies the wait flag.
`eas_Init`, Jump/BumpUp/BumpDown and DeactivateAutoFacing terminate at Idle;
`eas_Idle` is a wait/branch loop, not exploration control. Inline motion/
orientation/speed/sprite operations run through `entityscriptengine_2.asm`
handlers `esc14_setAnimationCounter`, `esc1B_setEntityOrientation`,
`esc10_setSpeed`, `esc0A_updateSprite` and `esc34_jump`.
`csc19_setEntityPosAndFacing`, `csc23_setEntityFacing` and `csc1C_stopEntityAnim`
resolve aliases and apply their alive-status pointer adjustment before entity
writes; this does not establish combat HP writes.
`csc22_animateEntityFadeInOrOut` uses its crop/DMA/VInt loop for slide4/5;
`csc26_entityNodHead` and `csc2A_entityShiver` have their own animation/sleep
loops. Camera destination32 waits for view completion; camera-speed45 writes
the scroll speed, quake33 writes amplitude, sound05 sends its command, and
fade39 requests FadeInFromBlack. `csWait` uses the interpreter's negative sleep
opcode and debug text skipping can bypass it. The terminator's `loc_47234`
tail conditionally waits for scrolling, clears scroll speed and restores
registers before returning to the wrapper and BattleLoop. Script synchronization
does not establish measured wall time, completed rendering or simultaneous motion.

The cursor feeds22 `nextSingleText` and6 `nextText`: all28 actual keys below
were compared with `gamescript.txt`, not just source comments. Galam128 uses
$C0 (mirrored/right portrait); every other selected speaker uses0.

| Decimal / hex key | Caller / actor | Brief content and exact token sequence |
| --- | --- | --- |
| 2357 /0935 | single128 | Galam addresses Astral; `{W1}`. |
| 2358 /0936 | single129 | Astral asks Galam's intent; `{N}{W1}`. |
| 2359 /0937 | single128 | Refusal to explain; `{W1}`. |
| 2360 /0938 | single131 | King asks Astral to save Elis; `{W1}`. |
| 2361 /0939 | single130 | Elis addresses her father; `{W1}`. |
| 2362 /093A | single129 | Astral recognizes king/leader; `{N}{LEADER}{W1}`. |
| 2363 /093B | next128 | Galam's exclamation; `{W2}`. |
| 2364 /093C | single128 | Hostage threat; `{N}{W1}`. |
| 2365 /093D | next129 | Astral cites reinforcements; `{N}{W2}`. |
| 2366 /093E | single129 | Challenges the opposing magic; `{N}{W1}`. |
| 2367 /093F | single128 | Counter-threat; `{N}{W1}`. |
| 2368 /0940 | single129 | Accepts challenge; `{W1}`. |
| 2369 /0941 | single128 | Galam addresses Astral; `{W1}`. |
| 2370 /0942 | single129 | Astral groans; `{W1}`. |
| 2371 /0943 | single128 | Claims victory; `{N}{W1}`. |
| 2372 /0944 | next129 | Claims mutual immobilization; `{N}{W2}`. |
| 2373 /0945 | single129 | Tells Elis to escape; `{N}{W1}`. |
| 2374 /0946 | single130 | Agrees to try; `{W1}`. |
| 2375 /0947 | single133 | Blocks her; `{W1}`. |
| 2376 /0948 | single130 | Reacts; `{W1}`. |
| 2377 /0949 | single128 | Claims she cannot escape; `{N}{W1}`. |
| 2378 /094A | next129 | Calls the leader; `{N}{LEADER}{N}{W1}`. |
| 2379 /094B | single129 | Requests rescue; `{W1}`. |
| 2380 /094C | single128 | Summoning declaration; `{N}{N}{W1}`. |
| 2381 /094D | single134 | Cry; `{W1}`. |
| 2382 /094E | next128 | Turns away from Astral; `{N}` only. |
| 2383 /094F | next128 | Threatens Astral; `{W2}`. |
| 2384 /0950 | single128 | Orders attack before rescue; `{N}{W1}`. |

`csc04_setTextIndex` supplies2357; `csc00_displaySingleTextbox` and
`csc02_displayTextbox` consume/increment only on the ordinary unskipped path.
The former closes portrait/text and sleeps10; the latter retains the window,
so2382's line token continues into2383. The actual text parser handles line
advance/scroll; `{LEADER}` rebuilds force lists and names the first active
member, without independently fixing Bowie. No award token occurs. Wait1/2 suspend typewriting until direction/A/B/C input, with wait2's
prompt/validation sound. Both wait loops draw main RNG with range256. Even a
known pre-scene seed would not fix the post-dialogue round seed without inputs.

## Seven templates and actual combat/neutral population

**Confirmed:** four header bytes + seventeen12-byte combatant records +
four12-byte regions + one2-byte point =258 bytes, matching the declared range.
Templates0–6 place at`(13,33)`, `(13,34)`, `(13,35)`, `(12,34)`, `(14,34)`,
`(12,35)`, `(14,35)`. All declare HEALER1/0, NOTHING, NONE/NONE255,
15/15 regions, filler0 and STARTING0.

`InitializeAllAlliesBattlePositions` resets all30 ally X coordinates, rebuilds
force/party lists, then copies **only X/Y** into ascending joined+active IDs.
Seven templates neither recruit Kiwi6 nor install template AI/items/spells.
Reuse the [member-scan/cursor rule](./battle02-initialization-and-first-control.md#joined-active-dead-and-capacity)
with seven iterations: an ordinary dead member consumes an iteration without
advancing the placement cursor. An eighth active member leaves the active party
even if a death left a template unused; joined flags survive, and the tail does
not rebuild RAM lists again. Reserves remain unplaced; general JoinForce
capacity12 is separate. There is no pre-loop zero-party guard to promote to
natural entry. Kiwi's optional admission stays at its existing owner.

Healing scans all30 slots; ordinary HP0 remains dead, while Peter7/Lemon28
receive HP/MP restoration even at0. For handled allies `status & 7` is
intermediate: `UpdateCombatantStats` preserves STUN/POISON and recomputes CURSE
from equipped cursed gear. An old CURSE bit alone does not establish final
curse. Actual equipment/status affects refreshed movement/agility; templates
fix neither. A living placed ally can still fail later action predicates.

All ten enemies initialize directly as mode0 in slots128–137; the remaining
32-slot initialization receives HP0/X=-1. `InitializeEnemyStats` selects type,
copies its56-byte definition, restores HP/MP, sets position/movement/commandset/
identity/regions/orders, processes placement item, reconstructs activation,
adjusts base ATT and refreshes stats. Placement NOTHING returns before AddItem;
it does not erase equipped definition items. The spawn-reset helper's occupied
start test is not a mode0 initialization gate. `ClearAiMemory` resets48 target
bytes to-1 and48 memory bytes to0, not all combatant state or RNG.

`LoadBattle` then rebuilds entities from combatant coordinates. Its
`PositionBattleEntities` clears scene mappings, declares placed allies/enemies
using live mapsprites (hovering adds an aerial layer), then checks F507.
With F507 clear, the actual row7 neutral table declares:

| Neutral combatant slot, allocated downward | Position / facing / mapsprite / actscript |
| --- | --- |
| 159 | `(13,21)`, Down, ASTRAL, LyingLeft |
| 158 | `(13,17)`, Down, EVIL_GALAM, LyingLeft |
| 157 | `(12,15)`, Down, ELIS, Standing |
| 156 | `(15,35)`, Up, MINISTER, Standing |
| 155 | `(11,35)`, Up, KING_GRANSEAL, Standing |

The consumer explicitly sets each neutral's max/current HP and status to0,
sets AIBITFIELD_NEUTRAL and writes X/Y before declaring its entity. Thus these
are real non-turn combatant slots as well as map actors, independent of the
before scene's aliases. F507 set skips neutral declaration even on a fresh
intro-skipped load; F457 alone does not. Suspended loading also runs this
completion-gated consumer, without implying fresh enemy initialization.
They are not a force join, five additional live enemies or scene damage.

## Ten enemies, refresh and available action operands

**Confirmed:** all rows declare placement NOTHING, filler0 and STARTING0.
The complete differing operands are:

| Slot / combatant | Base type / position | Commandset | Primary order / region | Secondary order / region |
| --- | --- | --- | --- | --- |
| 0 /128 | DARK_SMOKE86 `(10,14)` | ATTACKER2/7 | NONE /1 | NONE /15 |
| 1 /129 | DARK_SMOKE86 `(16,16)` | ATTACKER1/6 | NONE /0 | NONE /15 |
| 2 /130 | GALAM_MAGE67 `(22,17)` | ATTACKER3/9 | FOLLOW_ENEMY1 (`$81`) /2 | FOLLOW_ENEMY1 /3 |
| 3 /131 | DARK_CLERIC76 `(23,18)` | HEALER3/2 | FOLLOW_ENEMY1 (`$81`) /2 | FOLLOW_ENEMY1 /3 |
| 4 /132 | GALAM_ARCHER56 `(18,22)` | ATTACKER1/6 | NONE /15 | NONE /15 |
| 5 /133 | GALAM_KNIGHT3 `(9,24)` | ATTACKER1/6 | NONE /15 | NONE /15 |
| 6 /134 | GALAM_ARCHER56 `(7,25)` | ATTACKER3/9 | FOLLOW_ENEMY5 (`$85`) /15 | NONE /15 |
| 7 /135 | GALAM_ARCHER56 `(19,30)` | ATTACKER3/9 | FOLLOW_ENEMY8 (`$88`) /15 | NONE /15 |
| 8 /136 | GALAM_KNIGHT3 `(22,31)` | ATTACKER1/6 | NONE /15 | NONE /15 |
| 9 /137 | GALAM_KNIGHT3 `(7,29)` | ATTACKER1/6 | NONE /15 | NONE /15 |

`GetMoveOrderPosition` tests bit6: all selected non-NONE orders follow live
combatants129/133/136, respectively, not scene aliases or point indexes.
`GetMoveOrders` returns separate bytes; `Set/GetTriggerRegions` packs/unpacks
region nibbles. Point0`(13,16)` exists but no selected primary/secondary order
references it. MOVE_ORDER's algorithm parameter cannot turn$81/$85/$88 into a
MOVE_TO point operand. Region15 and NONE255 are different sentinels.

| Definition | Level; HP/MP; base ATT/DEF/AGI/MOV | Movement / equipment / resistance / AI declaration |
| --- | --- | --- |
| DARK_SMOKE86 | 7;18/20;18/13/141/6 | HOVERING; four NOTHING; WIND_WEAKNESS\|ICE_MAJOR\|FIRE_WEAKNESS\|STATUS_MAJOR; PRIORITYMOD_0 |
| GALAM_MAGE67 | 3;14/5;8/9/11/5 | MAGE; SHORT_ROD86; FIRE_MINOR; PRIORITYMOD_2\|BIT4 |
| DARK_CLERIC76 | 3;15/7;11/11/13/5 | HEALER; SHORT_ROD86;0; PRIORITYMOD_3 |
| GALAM_ARCHER56 | 3;15/0;7/10/10/5 | ARCHER; WOODEN_ARROW45;0; PRIORITYMOD_2\|BIT4 |
| GALAM_KNIGHT3 | 3;16/0;9/11/12/7 | CENTAUR; SHORT_SPEAR72;0; PRIORITYMOD_2\|BIT5 |

All declare initial status NONE and REGULAR spell power. The raw unknownByte
is3/56/67/76 for those definitions and0 for Smoke86; no role is inferred from
that label. Equipped definitions
have three further NOTHING items; spell slots are DISPEL1 for Smoke, BLAZE1
for mage, HEAL1 for cleric, followed by three NOTHING; archer/knight have four
NOTHING. Prowess is CRITICAL150_1IN32/DOUBLE_1IN32/COUNTER_1IN32 for Galam
types, with cleric COUNTER_1IN16. Smoke instead declares
CRITICAL_SILENCE/DOUBLE_1IN16/COUNTER_1IN16. AGI141 means13+128, not numerical
speed141; both Smokes can supply a second round entry. No other selected enemy
has that bit. These declarations do not prove a critical, silence or cast occurs.

Activation reconstruction preserves only the definition word's high nibble:
Smoke0, mage/archer/knight$2000, cleric$3000, with zero mode/filler. BIT4/BIT5
do not survive; commandset remains its own byte. Short Rod ATT+5/range1–1,
Short Spear ATT+6/range1–2, Wooden Arrow ATT+7/range2–2 all declare WEAPON and
useSpell NOTHING. Refresh applies equipment after floor(baseATT×5/4), once on
every non-SUPER difficulty and twice on SUPER. At difficulty0 final ATT is22
Smoke,15 mage,18 cleric,15 archer,17 knight. These are static derived stats,
not damage or a naturally selected difficulty.

HEAL1 is silence-affected teammate healing, MP3, range0–1, radius0, power15;
BLAZE1 is silence-affected attack, MP2, range1–2, radius0, power6. DISPEL1 is
silence-affected SUPPORT, MP5, range1–2, radius1, power0, not a damage spell.
`GetNextUsableAttackSpell` excludes SUPPORT; `GetNextSupportSpell` admits it.
`aiCommand_Support` rejects Muddle2/allied casters, explicitly accepts DISPEL,
checks MP and reachability, and calls its actual priority builder. That builder
scores attack/healing-spell-bearing targets in the affected area and retains
only candidate areas with score≥2 before the command chooses a reachable target
and attack position and publishes CAST_SPELL. In its occupancy setup, the
`move.w #-1,d3` and `clr.w d3` overwrite the preceding bit-test CCR before
branches; the table's targeting property alone does not describe those branches.
Smoke's20MP permits the cost; standby or earlier ATTACK success can bypass SUPPORT.
Availability does not guarantee a first cast, affected target or status result.

Cleric's `aiCommand_Heal` rejects Muddle2, checks HEAL's minimum3MP and requires
a living target with3×HP≤2×maxHP plus a reachable casting position. The stored
HEAL command parameter is unused: HEALER3 is not spell level3. Fresh full HP
provides no ordinary healing need. Mage's actual attack getter can admit BLAZE.
Retain the [item-getter caveat](./battle05-initialization-and-first-control.md#eight-enemies-equipment-and-available-action-operands):
useSpell NOTHING63 is not the erroneous255 no-use comparison; the spell getter
falls back to HEAL. Attack-item selection rejects that non-attack type, and
healing-item selection skips these non-Rain weapons with item-entry AI-use bit13
clear. That item bit is separate from activation. No full action audit is claimed.

`IsBattleUpgradable` scans the actual length-prefixed random list, which omits
VERSUS_DARK_SMOKES7. `UpgradeRandomBattleEnemies` restores the declared type
and returns before completion/level/category/RNG selection. F507 cannot upgrade
these enemies or consume upgrade RNG. Battle03's airborne exclusion defect
remains at its owner; Smoke's hovering type does not enter that code here.

## Four regions, standby and conditional first dispatch

**Confirmed:** every region has type4, second byte0 and trailing0/0. Ordered
vertices are:

| Region / persistent flag | Vertices1/2/3/4 |
| --- | --- |
| 0 /90 | `(0,0)`, `(0,29)`, `(26,29)`, `(26,0)` |
| 1 /91 | `(0,0)`, `(0,25)`, `(26,25)`, `(26,0)` |
| 2 /92 | `(0,0)`, `(0,27)`, `(26,27)`, `(26,0)` |
| 3 /93 | `(0,17)`, `(0,29)`, `(12,29)`, `(26,17)` |

`IsRegionTriggered` tests triangles1/2/4 then3/2/4 via integer projection/CCR
rules, against all30 living allies with nonnegative X/Y. Fresh ally placements
are X12–14/Y33–35, south of every selected triangle. For regions0/1/2 the
bottom horizontal edge separates those points from the third vertex in one
triangle; the other triangle's diagonal projection is north of them. Region3's
first triangle has its diagonal from`(0,29)` to`(26,17)` north of all placements;
the second rejects across horizontal Y29. Hence no initial F90–93 hit follows,
even though several enemies/neutral actors stand inside. Neither group is a
trigger subject. This proves the selected fresh geometry, not arbitrary polygons.

`ActivateEnemies` visits placed living enemies and its update tail calls
`TriggerRegionsAndActivateEnemies`. `NEWLY_TRIGGERED_BATTLE_REGIONS` is a
tested cache, distinct from persistent F90–105: fresh flag clearing and
`ClearAiMemory` do not explicitly clear that word; non-swarm `StartAiControl`
clears it later. An inherited tested bit can suppress testing; here fresh flags
remain clear even if tested. Primary-region success sets bit0 and takes
precedence over secondary success (bits0/1). For130/131, a region3-only hit
sets secondary activation; a region2 hit takes the primary branch first. That
branch preserves any already-set secondary bit; branch priority does not reset
prior secondary state. The identical stored follow operands do not erase that
distinction. Persistent flags can survive later testing until a clearing
consumer; suspend is a separate lifetime.

There is no Battle07 region-program row (sparse table32/40). All ten enemies
have mode0; fresh neutral HP0 and unused hidden slots supply no spawn candidate.
The region-program and PopulateTargetsListWithSpawningEnemies stages therefore
add no dialogue or summon at this first fresh boundary. Enemy132–137 have15/15
and bypass inactive standby regardless of bit0. Four assigned-region enemies
128–131 use the actual standby predicates while inactive:

| Group | Standby result |
| --- | --- |
| 128/129, primary NONE with region1/0, secondary NONE/15 | ValidateAiStandbyEligibility returns d1=0,d2=0. After early random-stop outcomes, local positions around the starting coordinate can be evaluated, memory updated and a move string built, still with STAY. No ATTACK/SUPPORT commandset runs on that path; movement is conditional. |
| 130/131, FOLLOW_ENEMY1 with region2 (and secondary region3) | Special primary/non15 returns d1=1. DetermineAiStandbyMovement treats nonzero as stop: empty move string/STAY. Its earlier random outcomes also stop. The misleading helper comment does not reverse its caller. |

| Set / selected users | Commands attempted in order | Preparatory mode |
| --- | --- | --- |
| HEALER1/0, templates only | HEAL1, SUPPORT, ATTACK1, MOVE1, STAY | Regular; not installed by ally placement |
| ATTACKER2/7,128 after activation | MOVE_ORDER1, ATTACK1, HEAL1, SUPPORT, MOVE1, STAY | Regular |
| ATTACKER1/6,129 after activation;132/133/136/137 | ATTACK1, HEAL1, SUPPORT, MOVE1, STAY | Regular |
| ATTACKER3/9,130 after activation;134/135 | MOVE_ORDER3, ATTACK3, HEAL2, SUPPORT, MOVE_ORDER4, MOVE2, STAY | Block non-movable |
| HEALER3/2,131 after activation | HEAL2, MOVE_ORDER4, SUPPORT, ATTACK3, MOVE2, STAY | Block non-movable |

Commands stop on success.128's MOVE_ORDER1 has NONE, not point0. MOVE_ORDER3
selects follow-type1/block-non-movable; MOVE_ORDER4 uses target-type0/point-mode
algorithm with block-non-movable, but its path builder still resolves the stored
order through GetMoveOrderPosition. Before activated command selection, a dead
follow target promotes secondary and clears secondary:130/131 become$81/NONE
on that visit, so the dead reference can remain until another visit;134/135
become NONE/NONE. Allies under AI select set6 rather than template set0.
This binds selected orders without simulating all tactics or a winning route.

Rounds reuse the [common rule](./battle02-initialization-and-first-control.md#first-round-and-conditional-control-endpoint):
64 sentinel slots; living placed candidates from30 allies/first30 enemy slots;
refreshed current AGI and main RNG,62 signed-byte stable-sort passes, cursor0.
All ten enemies are eligible independently of activation; neutral slots155–157
have HP0, and158–159 are also outside the first30-enemy scan. Smoke's first
score uses13 with three draws; its second uses floor(13×5/6)=10 with two draws,
without the first score's extra RNG(3)-1. Two Smokes therefore provide twelve
enemy entries if fresh and living. No upgrade draw precedes this, no seed reset
or fixed first actor follows, and range0 still advances main RNG. Standby uses
the separate thinking RNG; before text input can already advance main RNG.

A sentinel requests another round; HP0 actors skip. `ExecuteIndividualTurn`
first gives MUDDLE/AI-controlled bit2 priority for AI preparation. Otherwise an
enemy with opponent-control=0 chooses AI, nonzero chooses player preparation;
ally auto-battle independently chooses AI. Shared cursor/view/status-window/
movement-grid work follows. SLEEP/STUN then select NoAction. The predicates
are checked again: MUDDLE/AI-controlled still take priority; absent them,
enemy opponent-control=0 executes AI, nonzero reaches player input; allies use
auto-battle. Stop at that conditional dispatch, before an executed action or
guaranteed player-ready stop. Suspended entry can instead use a retained queue.

## Current conversion and production consumers

**Confirmed by accepted production code inspection, separate from original
evidence:** Battle07 is not admitted, converted or supported by the current
private encounter path. The [trust](../../../../remake/docs/content/profiles-and-trust.md#selected-third-encounter)
and [Application](../../../../remake/docs/application/session-and-programs.md#yeel-guide-zone-request-and-sleep-continuation)
owners retain selected2/3 consumption. No preparation/lowering/comparison is
performed; unmerged PR692 supplies no dependency.

| Actual consumer | Reusable mechanism and explicit Battle07 limit |
| --- | --- |
| [prepare_encounter / compile_routes](../../../../src/sf2tool/remake_exploration_battles.py) | Producer/CLI admit2/3 only, with selected headers/source/ROM checks. Generic route compilation binds supplied hooks/load and400/450/500 flags; it does not export7 or this before scene. |
| [PrivateBattleEncounterReader](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs) | Parses complete placements, regions/points, metadata and pinned terrain/ranges. Pin admits1/2/3, rejects7; no7 pin/export exists. |
| [PrivateBattleScenarioReader.Assemble](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs) / [PrivateBattleActionBindings](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleActionBindings.cs) | Independent region-program admission1/2/3; commandset switch0/6/7/9/14 excludes HEALER3/2. HOVERING and numerical AGI13 plus ExtraRoundAction are expressible; Smoke PRIORITYMOD_0 matches the AI-word reader. Its CRITICAL_SILENCE/DOUBLE_1IN16/COUNTER_1IN16 fails the physical-prowess switch, as does cleric prowess. Mover lacks ARCHER/MAGE; AI expression rejects BIT4, weapon binding rejects spear1/2 and arrow2/2. Controlled class switch lacks MAGE3, THIF10 and Kiwi's class; seven templates require none of them. Assembled executable spells bind HEAL/healing items, not DISPEL/BLAZE. Stored spell data alone is not execution. |
| [BattleEntry](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs) | Route/before/load/start continuations, live membership rebuild and carried resources/growth/gold/both seeds are reusable. Missing ally definitions and suspended F88 reject; original queue/cursor restoration is absent. No admitted7 content reaches it. |
| [BattleInitializationRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleInitializationRules.cs) / [BattleTurnFlow](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleTurnFlow.cs) | Controlled already-refreshed allies/roster-only/difficulty0, living/immortal healing and formation placement exist. Excess active count rejects instead of the source tail; enemy items allow0/127, rejecting selected equipped Galam weapons. General equipment/CURSE refresh, other difficulty/control policies and saved-state restoration remain Unsupported. No accepted7 neutral-population binding is supplied. |
| [BattleActivationRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActivationRules.cs) / [TurnOrderRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/TurnOrderRules.cs) | Living placed ally tests, primary/secondary activation, RegionProgram.None, mode0 and extra round actions are reusable. Cross-product geometry/fresh per-round tested word do not prove equality to original projection/cache lifetime for arbitrary state. Smoke's second score is expressible, without a Battle07 equivalence observation. |
| [SourceEnemyAi](../../../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceEnemyAi.cs) / [SourceMoveOrderAi](../../../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceMoveOrderAi.cs) | Standby,15/15, points/live follow and dead-target promotion exist. Activated sets6/7/9/14 exclude2; loadout checks allow only0/127 items and no learned spells, rejecting Smoke DISPEL, cleric HEAL and mage BLAZE as well as equipped enemies. Adjacent physical searches do not supply general ranged, support-spell or healer AI. |
| [BattleAdvancer](../../../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleAdvancer.cs) / [BattleControlRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleControlRules.cs) | Queue-driven automatic progression/reached-player admission exist with control/status rejection. A player stop can follow automatic work; a reference actor/seed does not define legality. This implementation is not original evidence. |

These are admission/support limits, not a Battle07 run or authorization to
broaden allowlists/repair the engine. No adjacent implementation change is made.
The [four frontiers](../../story-content-coverage.md#four-different-frontiers) remain distinct.

## Unknowns and verification boundary

**Unknown:** naturally reached407/457/507/88/83 and egress/follower state,
actual party/HP/status/level/difficulty/RNG, first actor, continuous travel, all
AI/actions and tactical legality, presentation/hardware timing and save durability.
No fixed seven-member party, Kiwi participation, first DISPEL or winning strategy
is proved. The #1047 twelve **completed unresolved comparisons** remain at the
[verification owner](../../../../remake/docs/verification/field-programs.md#yeel-guide-through-battle03);
they are not interrupted runs or an automatic repair/acquisition queue.
Accepted Battle01 natural admission/victory/stable return/later Down observations
are not downgraded. Prior mechanical PASS missed the intermediate-status/
equipment-CURSE distinction; that correction remains. PR1221's initial
mechanical PASS also missed a stale coverage summary/denominator and opponent-
control polarity. Its two P2 findings and corrected accepted semantics remain
in the [correction handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1220#issuecomment-6354981415);
mechanical checks do not prove semantics.

Reproduction is direct reading of the complete named source/consumer bodies.
Direct checks cover links/anchors, UTF-8/LF, explicit three-path base diff,
ownership/private boundary, unchanged fixture counters and four frontiers.
Retained map-init/map-script outputs total18,285,979 bytes and remain unchanged,
as does `local/issue1005/pair.txt`. Only small fresh receipts go under ignored
`local/issue1224/`; reuse the simple document method with explicit `origin/main`
diff, including committed head. A mechanical PASS is not independent acceptance.
On a clean committed head, load current ignored configuration in the same
process, then record planner and actual exact-head scoped CI:

```powershell
. ./local/private-inputs.ps1
uv run sf2 verify plan --base origin/main --head HEAD
```

No normal/full, SDK/Godot/emulator/H3/H4, screenshots, comparison, lowering,
extraction or acquisition is selected. Stop with a clean pushed frozen Draft
PR, Review / Verify handoff and stopped processes. Main-gate retains independent
acceptance and merge authority.
