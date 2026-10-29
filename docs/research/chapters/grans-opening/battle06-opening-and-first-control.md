# Battle06 opening, initialization and first control

Evidence date: 2026-10-22. **Confirmed:** Battle06's complete6/9/1/0 selection
connects an unskipped guard encounter, six placement templates, nine equipped
enemies and one initially untriggered region to conditional individual control.
Dark Cleric's follow order and the other region0 enemies have different standby
branches. **Inferred:** the dialogue presents the failed package excuse during
the escape toward Granseal. **Unknown:** natural entry/party/status/RNG, first
actor, all actions, presentation/timing and persistence. This is static composition.

## Scope and provenance

This is the **Standard**-rigor agreement for
[Issue1222](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1222),
under [Epic1053's finite denominator](../../story-content-coverage.md#scope-and-reading-rules).
The outcome supports content maintenance and conversion review: explain the
complete selected opening through first player/AI/no-action dispatch, including
changed valid party state. Reuse the [Battle02 lifecycle](./battle02-initialization-and-first-control.md)
and [Battle03](./battle03-initialization-and-first-control.md),
[Battle04](./battle04-initialization-and-first-control.md) and
[Battle05](./battle05-initialization-and-first-control.md) differences. The
[admission owner](./battle05-to-battle06-content-route.md) retains Jewel/Slade and
Galam exits; the [outcome owner](./battle06-to-battle07-content-route.md) retains
victory/defeat/egress and the conditional Granseal/tower continuation.

Owned writes are this document and Battle06 narratives in the two coverage
owners. Sources, consumers, fixtures, registries, counters and runtime frontiers
are read-only. No parser, fixture, schema, platform, allowlist, implementation,
extraction or acquisition is added. Acceptance is direct semantic/document/
private-boundary review, committed-head planner and actual exact-head scoped CI,
then a clean pushed frozen Draft PR for independent acceptance.

Original paths below are relative to `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The selected checkout was clean at that pin. Accepted repository base is
`d1397a9843b7f4d69c8afe3cb1bca93e0894c1b7`, tree
`d5049bf08318fe8020a9a412097f1d6b3e7a92ca`. The
[USA baseline](../../../../README.md#pinned-original-baseline) retains input identity;
no fresh ROM-equivalence claim is made. Complete selected records/functions and
return tails, rather than comment meanings alone, supply the facts. Short battle
paths below resolve under `code/gameflow/battle/`; `j_` interfaces resolve to the
named implementations.

| Claim | Declaration and actual consumer |
| --- | --- |
| Complete formation/behavior | `data/battles/spritesets/entries.asm` slot6 → `spritesets/spriteset06.asm:BattleSpriteset06`, `0x1B365C..0x1B3720`; `sf2macros.asm:allyCombatant/enemyCombatant/combatantAiAndItem/combatantBehavior`, `sf2enums.asm`; `getbattlespritesetsubsection.asm:GetBattleSpritesetSubsection`, `0x1B1630`. |
| Admission/metadata | `code/common/maps/getbattle.asm:CheckBattle`, `0x799C`; `code/gameflow/mainloop.asm:MainLoop`; `data/battles/global/battlemapcoords.asm` row6, `terrainentries.asm` slot6, `global/custombackgrounds.asm`, `enemyleaderpresence.asm`, `halvedexpearnedbattles.asm`; `battleloop/loadbattleterraindata.asm`, `battlescenes/getbattlescenebackground.asm`, `battleactions/giveexpandgold.asm`. |
| Before program/actors/text | `data/battles/entries/battle06/cs_beforebattle.asm:bbcs_06/ce_49CBA`, `0x49BCA..0x49CE2`; `data/scripting/text/gamescript.txt` keys0926–0934; `sf2cutscenemacros.asm` script macros; `code/common/scripting/map/mapscriptengine_1.asm`, `0x46506..0x47102`, and `_2.asm`, `0x4712C..0x474FC`; `mapfunctions.asm:InitializeMapEntities`, `0x440CC`, and `followersfunctions_1.asm:InitializeFollowerEntities`, `0x44298..0x44338`. |
| Hook gates/entry lifecycle | Before/start wrapper start/end files under `cutscenes/`, corresponding tables under `data/battles/cutscenes/`; `code/common/scripting/map/ms_empty.asm`; complete `battleloop_1.asm:BattleLoop`, `0x23A84`. |
| Actual party/refresh/entities | `battleloop/heallivingandimmortalallies.asm`, complete `initializecombatants.asm`, `0x1B1272..0x1B15F8`; `code/common/stats/battleparty.asm:UpdateForce/LeaveBattleParty`, `updatecombatantstats.asm`; `data/stats/enemies/enemydefs.asm` records2/3/56/67/76, `data/stats/items/itemdefs.asm` records33/45/72/86; `battlefunctions/loadBattle.asm`, `code/common/scripting/entity/entityfunctions_1.asm:PositionBattleEntities`. |
| Actions/orders | `data/stats/spells/spelldefs.asm:HEAL/BLAZE`; `ai/startaicontrol.asm`, `executeaicommand.asm`, `command/heal.asm`, `command/attack.asm`, `command/moveorder.asm`, `getnextuseableaiaction.asm`, `gethighestusablespelllevel.asm`; `data/battles/global/aicommandsets.asm`; `battlefield/getmoveorderposition.asm`; `code/common/stats/combatantstats_1.asm:GetMoveOrders/GetTriggerRegions`, `combatantstats_2.asm:SetTriggerRegions`. |
| Upgrade/activation/standby | `battleloop/upgradeenemies.asm:IsBattleUpgradable/UpgradeRandomBattleEnemies`, `data/battles/global/randombattles.asm`; `battleloop/clearaimemory.asm`, `activateenemies.asm`, complete `triggerregions.asm`, `populateenemyspawns.asm`; both `cutscenes/regionactivatedcutscenes.asm` and its data table; `data/battles/global/battleneutralentities.asm`; both `ai/determineaistandbymovement_*.asm`. |
| Round/control endpoint | `battleloop/turnorderfunctions.asm`, `0x25544`; `code/common/tech/randomnumbergenerator.asm:GenerateRandomNumber`; complete `battlefunctions/executeindividualturn.asm`, `0x23EB0..0x24242`. |

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

**Confirmed:** `BATTLE_TO_GRANSEAL=6` selects row6 (`0x7A60`): Map66,
origin`(21,2)`, area20×32, wildcard triggers255/255. `CheckBattle` scans in table
order, testing map, unlock and trigger coordinates; area is output, not an
inside-rectangle test. Earlier Map66 rows2/3/4 can win when independently armed.
F406/F506 both set clears406 but still returns6 on that call. F456 is the intro
gate, F506 completion and F88 suspended entry. Map66 alone does not arm6.

Terrain slot6 selects `BattleTerrain06`, a Stack-decoded48×48 terrain array,
separate from the20×32 area. Background row6 is OVERWORLD255, selecting the
ordinary map-block/terrain background path rather than Battle05's fixed CASTLE8.
Leader-presence row6 is0; the defeated wrapper's leader-death sweep is absent.
The separate post-battlescene position table contains only5, so it supplies no6
override. Half-EXP selects only Battle1. The outcome owner retains these later
consumers; Dark Cleric's first slot does not itself define a victory condition.

| Supplied state | Selected sequence |
| --- | --- |
| F88 clear, F456 clear | Clear PLAYER_TYPE/set399; clear seconds; save incoming map/battle, install current fields/base VInt, execute before6; restore supplied fields; clear90–105; heal; initialize actual allies/enemies; clear AI memory; load focused on0; set456 and execute empty start; prepare round. Before slot6 (`0x47A94`) checks456 without setting it; start slot6 (`0x47B38`) sets456 before dispatch. |
| F88 clear, F456 set | Both hook bodies skip; fresh healing/placements/enemy refresh/load/round still run. No new407 write follows from a skipped before. F506 is not this skip gate. |
| F88 set | Restore saved seconds, clear88/AI memory/view target, load using incoming focus and enter retained queue/cursor. No fresh current-field installation, region reset, healing, placements, hooks or new round first; a saved sentinel later requests round preparation. This requires supplied saved state, not proved save durability. |

Start6 selects `ms_Empty` (`0x47A4E`), whose terminator can wait for scrolling
and clears scroll speed. There is no separate Battle06 start-scene body to infer.
Before6 does not assign `EGRESS_MAP`; the predecessor owns conditional Map16
egress state and F406 from Battle05's after-program. The selected Map16 savepoint
is`(27,7)`, Up with F399 set, only when the incoming egress map is16. Another
incoming value changes the return; no fixed egress state is inferred here.

## Complete before program and text consumers

**Confirmed:** `bbcs_06` starts text cursor2342, requests Map66/camera`(25,5)`,
and loads `ce_49CBA`. `csc37_loadMapAndFadeIn` falls through `csc48_loadMap`;
these map/view operands are not an exploration warp to player`(25,5)`.
`csc42_loadMapEntities` reads the main tuple and calls `InitializeMapEntities`:
clear entities, initialize state-selected followers, then explicit rows, then
main actor. Ally identity already supplied by a follower can suppress its
explicit duplicate. Declared staging is therefore distinct from arbitrary
incoming follower placement or a proved natural scene population.

The table declares Bowie`(30,9)`, Down; Slade`(29,8)`, Down; soldier128`(32,20)`,
Up; soldier129`(36,13)`, Left; archer130`(37,14)`, Up, then terminator.
Slade's scene row is not JoinForce, an active flag or proof of membership.
The predecessor owns the actual recruitment. Scene soldier128 is not the later
Dark Cleric128; `LoadBattle` rebuilds entities from live combatant coordinates.
No Battle06 neutral-table row preserves any of these scene guards.

The complete ordered operations, grouped only at text/action boundaries, are:

1. Initialize act scripts for Bowie, Slade and128–130 with waits; request MAIN_THEME
   and fadeInB; show2342 from129. Slade jumps asynchronously, Bowie jumps with wait;
   set camera destination`(31,9)` and wait for view completion.
2. Show2343–2345 (129,130,Slade); face129 Down, shake130's head, face129 Left;
   show2346 from129. Move128 Up5 with wait, face Right; show2347. Face130 Left,
   show2348; face128 Up, `csWait30`, face Right; show2349 with `nextText`, then2350
   with `nextSingleText`. Face128 Up and show2351; show2352 from Slade.
3. Track128; move it Left2/Up4 with wait; show2353. Set camera neutral(-1);
   wait for128's DeactivateAutoFacing act script; move Down1 with wait; show2354.
   Reinitialize128 with wait, face Down; show2355 and Slade's2356.
4. `setStoryFlag7` → `csc13_setStoryFlag` adds400 and sets F407; terminate.
   No inventory delivery, join, F506 write or warp to Battle07 occurs.

`csc15_setEntityActscript` installs the script and, when requested, waits for
`eas_Idle`; `csc2D_entityActionSequence` compiles the directional sequence and
its separate `loc_46928` tail installs it and waits. Facing resolves character
identity before writing; headshake performs its VInt-driven animation loop.
`csc24_setCameraTargetEntity` translates through the entity lookup, while a
negative operand releases tracking. `csWait` uses the interpreter's negative
sleep opcode; debug text skipping can bypass it. These waits name source
synchronization, not measured wall time or completed rendering.

| Decimal / hexadecimal key | Text caller / actor | Brief content and exact control tokens |
| --- | --- | --- |
| 2342 /0926 | nextSingleText,129 | Halt; `{W1}`. |
| 2343 /0927 | nextSingleText,129 | How they passed the Galam guards; `{N}`, `{W1}`. |
| 2344 /0928 | nextSingleText,130 | Departure forbidden until Granseal is conquered; `{N}`, `{W1}`. |
| 2345 /0929 | nextSingleText,Slade | Claims the king left something behind; two`{N}`, `{W1}`. |
| 2346 /092A | nextSingleText,129 | Hesitation about carrying it to him; `{N}`, `{W1}`. |
| 2347 /092B | nextSingleText,128 | Asks what is happening; `{W1}`. |
| 2348 /092C | nextSingleText,130 | Describes a package for the king; `{N}`, `{W1}`. |
| 2349 /092D | nextText,128 | Treats it as important; `{N}`, `{W2}`. |
| 2350 /092E | nextSingleText,128 | Orders still stand; offers to deliver; two`{N}`, `{W1}`. |
| 2351 /092F | nextSingleText,128 | Will collect the package; `{N}`, `{W1}`. |
| 2352 /0930 | nextSingleText,Slade | Predicts trouble; `{W1}`. |
| 2353 /0931 | nextSingleText,128 | Demands package, then recognizes them; two`{N}`, `{W1}`. |
| 2354 /0932 | nextSingleText,128 | Asks how they escaped jail; `{N}`, `{W1}`. |
| 2355 /0933 | nextSingleText,128 | Identifies Granseal spies; `{N}`, `{W1}`. |
| 2356 /0934 | nextSingleText,Slade | Admits the excuse failed; `{W1}`. |

`csc04_setTextIndex` supplies2342; `csc00_displaySingleTextbox` and
`csc02_displayTextbox` consume/increment the cursor only on the ordinary
unskipped path. The former closes portrait/text and sleeps10; the latter leaves
the window for continuation. All guards use portrait modifier0; Slade uses$C0 (mirrored/right);
Bowie has no text operand. Keys/tokens were checked against `gamescript.txt`,
not just script comments. The actual text consumer is
`code/common/scripting/text/textfunctions_1.asm:DisplayText/ParseSpecialTextSymbol`:
line handling advances/scrolls the dialogue line; wait1 and wait2 suspend
typewriting until a direction/A/B/C input, with wait2 additionally updating
its prompt and issuing validation sound. Both wait loops call the main RNG
with range256, so incoming seed alone does not fix the post-dialogue round seed. No substituted-name or item-award token occurs.
F407 is from this before-program, not victory or natural arrival at7.

## Six templates and the actual force

**Confirmed:** four header bytes, fifteen12-byte combatant records and one
12-byte region total196 bytes, matching`0x1B365C..0x1B3720`; there are zero AI
points. Ordered templates0–5 place at`(11,6)`, `(10,6)`, `(9,6)`, `(11,7)`,
`(10,7)`, `(9,7)`. Every template declares HEALER1/0, NOTHING, NONE/NONE,
15/15 regions, filler0 and STARTING0.

`InitializeAllAlliesBattlePositions` resets all30 ally X coordinates, calls
`UpdateForce`, then copies **only X/Y** into actual ascending joined+active
IDs0–29. Template IDs do not require Bowie through Slade or install HEALER1,
inventory or spells. Joined5, active37 and following73 are distinct consumers.
Reuse the [member-scan/cursor rule](./battle02-initialization-and-first-control.md#joined-active-dead-and-capacity)
with six iterations: dead ordinary members consume an iteration without
advancing the template. A seventh active member leaves the active party even
if a death left a template unused; joined flags survive and the tail does not
rebuild RAM lists again. Reserves stay unplaced. General JoinForce capacity12
is separate. There is no pre-loop zero-party guard to promote to natural entry.

Healing scans all30 slots; ordinary HP0 stays dead, while Peter7/Lemon28 always
receive HP/MP restoration. For handled allies `status & 7` is intermediate:
`UpdateCombatantStats` preserves STUN/POISON and recomputes CURSE from equipped
cursed gear. An old CURSE bit alone does not establish final curse. Equipment
and STUN can change current movement/agility; templates fix neither.

## Nine enemies, equipment and first selectable actions

**Confirmed:** all nine rows have placement NOTHING, secondary order NONE255,
secondary region15, filler0 and STARTING0. The complete differing operands are:

| Slot / combatant | Base type / position | Commandset | Primary order | Primary region |
| --- | --- | --- | --- | --- |
| 0 /128 | DARK_CLERIC76 `(7,29)` | HEALER3/2 | FOLLOW_ENEMY\|1 (`$81`) | 0 |
| 1 /129 | GALAM_MAGE67 `(6,28)` | SENTRY8 | NONE | 0 |
| 2 /130 | GALAM_SOLDIER2 `(15,12)` | ATTACKER1/6 | NONE | 15 |
| 3 /131 | GALAM_KNIGHT3 `(7,13)` | ATTACKER1/6 | NONE | 15 |
| 4 /132 | GALAM_ARCHER56 `(16,13)` | ATTACKER1/6 | NONE | 15 |
| 5 /133 | GALAM_KNIGHT3 `(8,18)` | ATTACKER1/6 | NONE | 15 |
| 6 /134 | GALAM_MAGE67 `(10,20)` | ATTACKER1/6 | NONE | 15 |
| 7 /135 | GALAM_SOLDIER2 `(6,25)` | ATTACKER1/6 | NONE | 0 |
| 8 /136 | GALAM_SOLDIER2 `(8,29)` | SENTRY8 | NONE | 0 |

`GetMoveOrderPosition` tests bit6:128's$81 follows combatant129's **live** X/Y,
not ally1, the officer's scene entity or point1. There are no point records.
`GetMoveOrders` returns separate order bytes; `Set/GetTriggerRegions` pack/unpack
region nibbles. Region15 and order255 have different meanings.

Fresh initialization visits32 enemy slots from128; these nine mode0 records
initialize directly and the unused slots receive max/current HP0 and X=-1.
The spawning reset helper's occupied-position check is not a mode0 entry gate.
`InitializeEnemyStats` selects type, copies its56-byte definition, restores
HP/MP, assigns coordinates/movement/commandset/type/regions/orders, processes
the placement item, reconstructs activation, adjusts base ATT and refreshes
stats. Placement NOTHING returns before AddItem; it does not erase equipment.

| Definition | Level; HP/MP; base ATT/DEF/AGI/MOV | Movement / equipped item / resistance / AI declaration |
| --- | --- | --- |
| DARK_CLERIC76 | 3;15/7;11/11/13/5 | HEALER; SHORT_ROD86;0; PRIORITYMOD_3 |
| GALAM_MAGE67 | 3;14/5;8/9/11/5 | MAGE; SHORT_ROD86; FIRE_MINOR; PRIORITYMOD_2\|BIT4 |
| GALAM_SOLDIER2 | 2;11/0;8/10/8/6 | REGULAR; SHORT_AXE33;0; PRIORITYMOD_2\|BIT5 |
| GALAM_KNIGHT3 | 3;16/0;9/11/12/7 | CENTAUR; SHORT_SPEAR72;0; PRIORITYMOD_2\|BIT5 |
| GALAM_ARCHER56 | 3;15/0;7/10/10/5 | ARCHER; WOODEN_ARROW45;0; PRIORITYMOD_2\|BIT4 |

All declare status NONE, REGULAR spell power, three other empty item slots and
four spell slots: cleric HEAL1, mage BLAZE1, each followed by three NOTHING;
others four NOTHING. Prowess is CRITICAL150_1IN32/DOUBLE_1IN32/COUNTER_1IN32
except the cleric's COUNTER_1IN16. No selected AGI has the second-turn bit.
The initializer preserves only the AI word's high nibble and installs zero
mode/filler: cleric`$3000`, others`$2000`; BIT4/BIT5 do not survive. This is
separate from the dedicated commandset byte.

Short Rod ATT+5/range1–1, Short Axe ATT+5/range1–1, Short Spear ATT+6/range1–2
and Wooden Arrow ATT+7/range2–2 all declare WEAPON/useSpell NOTHING. Refresh
applies those effects after difficulty adjustment: floor(baseATT×5/4) once on
every non-SUPER difficulty, twice on SUPER. At difficulty0 final ATT is18 for
cleric,15 mage,15 soldier,17 knight,15 archer. These are static derived stats,
not damage or a naturally selected difficulty.

HEAL1 is silence-affected teammate healing, MP3, range0–1, radius0, power15;
BLAZE1 is silence-affected attack, MP2, range1–2, radius0, power6. Actual spell
getters/MP checks can admit them from7/5MP. `aiCommand_Heal` rejects Muddle2,
checks HEAL's minimum3MP, then requires a living target with3×HP≤2×maxHP and a
reachable casting position before publishing CAST_SPELL. Its HEAL1/2/3 command
parameter is stored but unused: HEALER3 does not mean spell level3. Fresh full
HP gives no ordinary healing need, and standby can bypass the entire commandset.
`aiCommand_Attack` can choose BLAZE or physical/item categories through its
actual getters. Availability proves neither first cast, access, damage nor
completion. No support spell or placement healing consumable is supplied.

Retain the [selected item-getter caveat](./battle05-initialization-and-first-control.md#eight-enemies-equipment-and-available-action-operands):
useSpell NOTHING63 is not the erroneous255 no-use comparison; the spell getter
falls back to HEAL. Attack-item selection rejects its non-attack type, and the
healing-item getter skips these non-Rain weapons with item-entry AI-use bit13
clear (separate from the activation word). Equipment
alone therefore does not enable a healing item. This is not a full action audit.

`IsBattleUpgradable` scans the actual length-prefixed random list, which omits
TO_GRANSEAL6. `UpgradeRandomBattleEnemies` restores each declared type and
returns before completion/level/category/RNG selection. F506 cannot upgrade
these enemies or consume upgrade RNG. Battle03's airborne exclusion defect is
retained at its owner; neither Battle03 nor Battle05's outcome is copied here.

## Region0, standby and conditional first dispatch

**Confirmed:** region0 is type4, second byte0, vertices`(0,15)`, `(0,31)`,
`(19,31)`, `(19,15)`, trailing0/0. The source tests triangles1/2/4 and3/2/4
with integer projection/CCR rules. Subjects are all30 living allies with
nonnegative X/Y, not enemies, templates or scene actors. Fresh placed allies
have X9–11/Y6–7: below both triangles' minimum Y15, so neither test includes
them. In the actual edge tests, triangle1 separates them from its other vertex
across horizontal Y15; triangle2 separates them across the diagonal from
`(19,15)` to`(0,31)` (integer-projected Y22–24 at X9–11). Enemy positions within
the rectangle do not trigger it. This selected
geometry does not establish equivalence for arbitrary polygons/saved positions.

`ActivateEnemies` visits placed living enemies; `TriggerRegionsAndActivateEnemies`
tests declared regions subject to `NEWLY_TRIGGERED_BATTLE_REGIONS`, setting
persistent F90 on a hit. That RAM word is a tested cache: fresh90–105 clearing
and `ClearAiMemory` do not explicitly clear it; non-swarm `StartAiControl` does
so later. An inherited cache can suppress testing, but fresh F90 stays clear
here even with a test. Primary success sets active bit0, taking precedence over
secondary success (bits0/1). All secondary regions are15. A later hit can
retain F90 until a clearing consumer; suspended state is not fresh state.

There is no Battle06 region-program row (sparse table32/40), no neutral row,
and no hidden/respawn candidate among these mode0 entries. Region activation
adds neither dialogue nor a character. The five15/15 enemies130–134 bypass
standby regardless of active bit0. Four0/15 enemies128/129/135/136 enter standby
while inactive, but their actual orders distinguish its result:

| Unactivated group | `ValidateAiStandbyEligibility` result and caller |
| --- | --- |
| 128, FOLLOW_ENEMY1 with primary region0 | Special order/non15 returns d1=1. `DetermineAiStandbyMovement` treats nonzero as stop: empty move string and STAY. Its earlier random-stop branches also stop. The comment saying this allows movement does not reverse the caller. |
| 129/135/136, primary NONE with region0 | NONE/non15 returns d1=0,d2=0. After the early random-stop outcomes, the caller can evaluate local standby positions around each starting coordinate, update AI memory and build a move string, still with action STAY. They do not execute SENTRY/ATTACKER attacks on this standby path. No unconditional local move is guaranteed. |

| Set / selected users | Commands attempted in order | Preparatory mode |
| --- | --- | --- |
| HEALER1/0, templates only | HEAL1, SUPPORT, ATTACK1, MOVE1, STAY | Regular; not installed by ally placement |
| HEALER3/2,128 after activation | HEAL2, MOVE_ORDER4, SUPPORT, ATTACK3, MOVE2, STAY | Block non-movable |
| SENTRY8,129/136 after activation | ATTACK1, HEAL1, SUPPORT, STAY | Regular |
| ATTACKER1/6,130–134;135 after activation | ATTACK1, HEAL1, SUPPORT, MOVE1, STAY | Regular |

`StartAiControl` promotes a dead followed129 to secondary NONE before activated
command selection, leaving both orders NONE. MOVE_ORDER4 selects target-type0/block-non-movable, entering the command's
point-mode algorithm; this does not change the stored$81 order. Its path builder
`ai/command/moveorder/buildmovestringformoveorder.asm:BuildMoveStringForMoveOrder`
calls `GetMoveOrderPosition`, whose bit6 test resolves129's live coordinates,
rather than a nonexistent point. The command checks MOV/order/target HP, can
try an attack and can build movement with STAY. SENTRY has no MOVE_ORDER/MOVE command, but ATTACK1 can build
an attack path; that is separate from its unactivated local standby movement.
AI allies select set6 rather than template HEALER1. Later tactics are outside scope.

Rounds reuse the [common rule](./battle02-initialization-and-first-control.md#first-round-and-conditional-control-endpoint):
64 sentinel slots; living placed candidates from30 allies/first30 enemies;
current numerical AGI and main-RNG draws, optional high-bit second turn,62
signed-byte stable-sort passes, cursor0. All nine enemies can enter independently
of activation. No upgrade draw precedes it and no seed reset/fixed first actor
follows. Even range0 advances main RNG; standby uses the separate thinking RNG.

A sentinel requests round preparation; HP0 actors skip. `ExecuteIndividualTurn`
first gives MUDDLE/AI-controlled bit2 priority for AI preparation. Otherwise an
enemy with opponent-control=0 chooses AI; nonzero chooses player preparation.
Ally auto-battle independently chooses AI. Shared cursor/view/status-window and
movement-grid work follows. SLEEP/STUN then select NoAction. The predicates are
checked again: MUDDLE/AI-controlled still take priority; absent them, enemy
opponent-control=0 executes AI, nonzero reaches player input; allies use their
auto-battle predicate. Stop at that conditional dispatch, before an executed
action or guaranteed player-ready stop. Suspended entry can use a saved queue.

## Current conversion and production consumers

**Confirmed by accepted production code inspection, separate from original
evidence:** Battle06 is not admitted, converted or supported by the current
private encounter path. The [trust](../../../../remake/docs/content/profiles-and-trust.md#selected-third-encounter)
and [Application](../../../../remake/docs/application/session-and-programs.md#yeel-guide-zone-request-and-sleep-continuation)
owners retain selected2/3 consumption. No preparation/lowering/comparison is
performed; unmerged PR692 supplies no dependency.

| Actual consumer | Reusable mechanism and explicit Battle06 limit |
| --- | --- |
| [prepare_encounter / compile_routes](../../../../src/sf2tool/remake_exploration_battles.py) | Producer/CLI admit2/3 only, with selected headers/source/ROM checks. Generic route compilation binds supplied hooks/load and400/450/500 flags; it does not export6 or this before scene. |
| [PrivateBattleEncounterReader](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs) | Parses complete placements, region/point, metadata and pinned terrain/ranges. `Pin` admits1/2/3, rejects6; no6 pin/export exists. |
| [PrivateBattleScenarioReader.Assemble](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs) / [PrivateBattleActionBindings](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleActionBindings.cs) | Independent region-program admission1/2/3; commandset switch0/6/7/9/14 rejects HEALER3/2 and SENTRY8. Mover lacks ARCHER/MAGE; enemy AI expression rejects BIT4. Physical binding rejects cleric's COUNTER_1IN16 prowess and non1/1 weapons (spear1/2, arrow2/2). Controlled class switch lacks MAGE3 and Slade's THIF10; templates require neither. Assembled actions bind HEAL/healing items, not BLAZE. Stored spell/transport shape is not executable support. |
| [BattleEntry](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs) | Route/before/load/start continuations, live membership rebuild and carried resources/growth/gold/both seeds are reusable. Missing ally definitions and suspended F88 reject; original queue/cursor restoration is absent. No admitted6 content reaches it. |
| [BattleInitializationRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleInitializationRules.cs) / [BattleTurnFlow](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleTurnFlow.cs) | Controlled already-refreshed allies/roster-only/difficulty0, living/immortal healing and formation placement exist. Excess active count rejects instead of source excess-party tail; enemy items allow0/127, rejecting all selected equipped weapons. General equipment/CURSE refresh, other difficulty/control policies and saved-state restoration remain Unsupported. |
| [BattleActivationRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActivationRules.cs) / [TurnOrderRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/TurnOrderRules.cs) | Living placed ally tests, primary/secondary activation, RegionProgram.None, mode0 and randomized rounds are reusable. Cross-product geometry/fresh per-round tested word do not prove equality to original projection/cache lifetime for arbitrary state. No Battle06 equivalence observation. |
| [SourceEnemyAi](../../../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceEnemyAi.cs) / [SourceMoveOrderAi](../../../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceMoveOrderAi.cs) | Standby,15/15, points/live follow and dead-target promotion exist. Activated sets6/7/9/14 exclude2/8; loadout checks permit only0/127 items and no learned spells, rejecting these weapons, HEAL and BLAZE. Adjacent physical searches do not provide general ranged/mage/healer behavior. |
| [BattleAdvancer](../../../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleAdvancer.cs) / [BattleControlRules](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleControlRules.cs) | Queue-driven automatic progression/reached-player admission exist with control/status rejection. A player stop can follow automatic work; a reference actor/seed does not define legality. This implementation is not original evidence. |

These are admission/support limits, not a Battle06 run or authorization to
broaden allowlists/repair the engine. No adjacent implementation change is made.
The [four frontiers](../../story-content-coverage.md#four-different-frontiers) remain distinct.

## Unknowns and verification boundary

**Unknown:** naturally reached406/456/506/88 and egress/follower state, actual
party/HP/status/level/difficulty/RNG, first actor, continuous travel, all AI/actions
and tactical legality, presentation/hardware timing and save durability. No fixed
six-member party, Slade participation, first cast or winning strategy is proved.
The #1047 twelve **completed unresolved comparisons** remain at the
[verification owner](../../../../remake/docs/verification/field-programs.md#yeel-guide-through-battle03);
they are not interrupted runs or an automatic repair/acquisition queue.
Accepted Battle01 natural admission/victory/stable return/later Down observations
are not downgraded. Prior mechanical PASS results missed the intermediate-status/
equipment-CURSE distinction; the correction above remains. PR1221's initial
mechanical PASS also missed a stale coverage summary/denominator and opponent-
control polarity. Its two P2 findings and corrected accepted semantics remain
in the [correction handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1220#issuecomment-6354981415); mechanical checks do not prove semantics.

Reproduction is direct reading of the complete named source/consumer bodies.
Direct checks cover links/anchors, UTF-8/LF, explicit three-path base diff,
ownership/private boundary, unchanged fixture counters and four frontiers.
Retained map-init/map-script outputs total18,285,979 bytes and remain unchanged,
as does `local/issue1005/pair.txt`. Only small fresh receipts go under ignored
`local/issue1222/`; reuse the simple document method with explicit `origin/main`
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
