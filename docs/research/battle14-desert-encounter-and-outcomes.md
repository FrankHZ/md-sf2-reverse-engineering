# Battle14: Southeast Desert encounter and outcomes

Evidence date: 2026-10-22. **Confirmed (static):** Map70's selected zone
producer can request Battle14, whose fresh initialization declares twelve ally
positions, fifteen enemies, three regions and no AI points. Completed repeats
can substitute enemy definitions. Empty local hooks retain shared initialization,
join and outcome effects; the Power Stick drop depends on actual held equipment.
This chapter connects those conditions through MainLoop returns and Map70 exits.
It establishes neither a naturally played encounter nor a mandatory14→15/16 route.

## Scope and provenance

**Standard** rigor follows [Issue1202](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1202)
under Epic1053: one connected static encounter, including repeat upgrade and
changed valid states. Full strategies, every possible substituted formation,
original runtime/presentation/persistence, new tooling and engine implementation
are outside this acceptance. [Story coverage](./story-content-coverage.md#four-different-frontiers)
keeps research, original observations, English design and remake playability separate.

Original paths below are relative to `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
USA baseline SHA-256 is
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
The read-only source and retained map-init/script pair were reused, without
rebuilding, lowering or running the game. Addresses are pinned source/retained
H1 anchors; no new ROM-equivalence claim is made. Relevant existing owners are
[random request](./map-event-random-battle-state.md), [request consumption](./map-event-request-consumption.md),
[global tables](./battle-global-data.md), [spritesets](./battle-spriteset-data.md),
[routing](./battle-routing-data.md), [loop](./battle-loop.md) and
[cutscene wrappers](./battle-cutscenes.md).

## Map70 request, cooldown and ordered admission

**Confirmed:** [Ribble/May's route](./ribble-may-through-battle10-admission.md#selected-map86970-connection)
connects69's rightward scroll to70, X1 with retained Y. That scrolling consumer
loads the destination/tiles and repositions current main/raft/followers; it does
not run SwitchMap, CheckBattle, ordinary entity rebuilding or map init. A held414
therefore does not select14 on the scroll itself. Map70 has only its default
setup (`data/maps/mapsetups.asm`); `s1_entities.asm` is an empty fixed list,
not an absence of main/flag-selected followers. `ms_map70_InitFunction`
`$4FE42..$4FE44` is `rts`. Ordinary entry and retained CURRENT reload differ
from scroll; neither init supplies a new Battle14 story scene.

`map70/mapsetups/s3_zoneevents.asm` at `$4FDFE` matches X16,17,18,19 with
wildcard Y to `Map70_ZoneEvent0`; it calls `CheckRandomBattle(14)` then returns.
Only the default checks510: held returns, clear sets410 and posts `$000100FF`.
A specific match whose helper declines never falls through to that default.
Coordinates alone do not invoke a zone: `entityscriptengine_2.asm:loc_5294`
requires layout attribute `$1400`, writes type6 plus word X into parameters1/2
and word Y into3/4. `ProcessMapEvent` clears type before dispatch; its type6
consumer applies main's init actscript; `RunMapSetupZoneEvent` dispatches the handler,
closes windows, waits for VInt and physical entity0 stop, and preserves its new request.

`mapsetupsfunctions_1.asm:CheckRandomBattle`, `$47856..$478C6`, has these branches:

| Supplied state | Actual helper result |
| --- | --- |
|514 clear|Request immediately, independent of step counter and without RNG.|
|514 held, STEP_COUNTER nonzero|Decline, including a supplied negative word; no draw or countdown here.|
|514 held, counter0, range8 result0|Request.|
|514 held, counter0, range8 result1–7|Draw range4; write result+2 (2–5), decline.|

A request orders set414 → long `$000100FF` at `MAP_EVENT_TYPE` → counter30000 →
WaitForViewScrollEnd → boost SFX → flash script. `randomnumbergenerator.asm`
advances the low-word seed by `(13*seed+7) mod65536`, then scales the unsigned
word into `[0,d6)` as D7.w. Range8 is a branch domain, not a measured encounter
frequency or independent-probability claim. D1/D6/D7 are restored on helper return.

The counter's actual consumers matter: `esc02_controlCharacter:loc_530C`
decrements a positive word once after the accepted control path updates destination,
not once per frame or on every blocked attempt. `ExplorationLoop` entry subtracts
20000 then clamps a negative signed result to0. Thus an intact request's30000
normally becomes10000 at the next such entry; arbitrary supplied words retain
68000 word/signed branch semantics. Scroll stays within the loop and performs
no entry subtraction. These consumers do not imply a naturally traversed cooldown.

The long write covers type word1, parameter1=0 and parameter2=255/CURRENT only;
parameters3/4/5 survive. Under intact ordinary type6 lineage and Y0–63,3/4 are
`00,Y`. The later no-scroll consumer samples current main position, overrides
nonnegative parameter bytes, and always takes facing from5: return is
CURRENT, X0, Y from the retained low byte, retained facing. A preceding scroll
can have written its requested facing to5, but intervening writers may replace it.
This is a mutable slot, not a queue or an atomic full-position reload.

After that exploration return MainLoop runs SwitchMap then CheckBattle. Map70
has no flag-map substitution. `battlemapcoords.asm` has exactly these matching
rows in its scan order:

| Battle | Crop X,Y / width,height | Trigger X,Y | Unlock / completion |
| --- | --- | --- | --- |
|10|0,0 /32,32|255,255|410 /510|
|14|15,26 /32,32|255,255|414 /514|

`getbattle.asm:CheckBattle` tests map/unlock/trigger, not crop containment. Held410
selects10 even outside that crop and even with414 held. Held510 additionally clears
410 **after selecting10**, without continuing to14. Only clear410 permits14 with
held414; held514 then clears414 but still admits14. A held completion never clears
an unselected unlock. Neither unlock gives no battle. Default510-held does not
clear a carried410. A14 helper request can therefore result in10; a helper decline
does not run admission merely because an unlock is held. No14-completion predicate
is added to the separate [shrine15](./battle15-shrine-approach-and-outcomes.md) or
[Kraken16](./south-parmecia-through-kraken-battle16-entry.md) routes.

## Fresh entry, empty hooks and complete formation

**Confirmed:** for selected14/F88-clear, `battleloop_1.asm:BattleLoop` clears
PLAYER_TYPE, sets399, clears elapsed seconds; establishes70/14 and base VInts;
runs before; restores selectors; clears region flags90–105; heals living allies
plus Peter7/Lemon28; places actual allies, initializes enemies, clears AI memory,
LoadBattle(0), then start wrapper. Healing restores HP/MP and first applies
status AND7 (STUN/POISON/CURSE); UpdateCombatantStats then refreshes stats and
recomputes CURSE from equipped cursed items. Supplied CURSE without such
equipment clears; equipped cursed gear sets it.
It does not make every supplied party member living or player controlled.

The actual build's `layout/sf2-07-0x044000-0x064000.asm` includes the four pointer
tables under `data/battles/cutscenes/`. Zero-based row14 in **each** before/start/
enemy-defeated/after table points to `ms_Empty`, whose whole program is the `$FFFF`
terminator in `code/common/scripting/map/ms_empty.asm`. Directory absence is not
this proof. `mapscriptengine_2.asm:ExecuteMapScript` still initializes interpreter
state, checks the terminator, conditionally waits for view scrolling when the
dialogue-window word is nonzero, then clears scrolling speed and restores registers.
No local dialogue, story reward or explicit warp is hidden behind these empty rows.

Before checks464 without setting it; start sets464 before dispatch. Held464 skips
both local programs, not placement/healing. After checks514: clear runs empty14,
held skips it; both reach the included **cutscenes** `afterbattlejoins.asm` row14=0,
not the unused global alternate. `JoinForce(0)` sets F0, rebuilds force lists and
sets F32 if active count<12; at capacity it does not clear pre-existing F32.
It does not rebuild the list again after that activation write or recalculate the
already constructed return seed. This shared call is not recruitment of a new
story character. Region-cutscene table has only32/40; after-position table only5;
neutral table has1/7/8/13/18/20/22/26/28/42/43, all with terminators, and no14.
Their actual dispatchers therefore add no14 region scene, position override or
neutral actor. Enemy-leader-presence row14 is0.

`data/battles/spritesets/spriteset14.asm`, `$1B4000..$1B416C`, header12/15/3/0,
is the complete declared formation. Coordinates here are local to the crop;
source-map coordinates add `(15,26)` (e.g. first ally `(4,25)`→`(19,51)`).
`LoadBattle` uses the battle crop for map projection; these coordinates are not
walkable route evidence or the source map's full64×64 layout.

Ally formation ordinals0–11 are `(4,25),(4,24),(3,25),(4,23),(3,23),(3,24),
(2,23),(2,24),(2,25),(1,24),(1,25),(1,23)`. Every row declares HEALER1,
NOTHING, NONE/15 primary, NONE/15 secondary, filler0, STARTING. These are capacity
positions, not twelve joined identities or orders applied as enemy AI to allies.
`InitializeAllAlliesBattlePositions` resets thirty ally Xs, UpdateForce, then
traverses the real active member list: HP0 skips placement and does not advance
the formation pointer; list/iteration progress still advances. Its remaining-tail
branch can LeaveBattleParty. Joined flags, active list, living placement and current
control are separate. Empty/malformed lists are not validated into a safe natural start.

Enemy rows below all have secondary NONE/15, filler0, STARTING0 and added item
NOTHING, except rows0–2 whose secondary region is1. Actual combatant ID is128+row,
independently of the enemy-type ID:

| Row / ID | Base enemy type | Local X,Y | Commandset | Primary order / region |
| --- | --- | --- | --- | --- |
|0 /128|Golem9|22,11|ATTACKER2|NONE /0|
|1 /129|Golem9|23,12|ATTACKER2|NONE /0|
|2 /130|Evil Cloud42|24,9|ATTACKER1|NONE /0|
|3 /131|Master Mage69|10,28|ATTACKER1|NONE /15|
|4 /132|Zombie8|9,29|ATTACKER1|NONE /15|
|5 /133|Evil Cloud42|10,19|ATTACKER1|NONE /15|
|6 /134|Death Archer58|12,22|ATTACKER1|NONE /15|
|7 /135|Black Monk78|11,25|HEALER1|NONE /15|
|8 /136|Zombie8|10,12|ATTACKER1|NONE /15|
|9 /137|Death Archer58|11,16|ATTACKER1|NONE /15|
|10 /138|Golem9|15,20|ATTACKER1|NONE /15|
|11 /139|Zombie8|14,18|ATTACKER1|NONE /15|
|12 /140|Zombie8|19,17|ATTACKER3|FOLLOW_ENEMY13 /2|
|13 /141|Black Monk78|16,15|HEALER3|FOLLOW_ENEMY14 /2|
|14 /142|Golem9|15,13|ATTACKER1|NONE /2|

`data/stats/enemies/enemydefs.asm:table_EnemyDefinitions` supplies these six
56-byte definitions, copied by InitializeEnemyStats after conditional upgrade.
The table below names **base** ATT/DEF/AGI/MOV, not final combat stats:

| Type | Level / HP / MP | ATT,DEF,AGI,MOV | Held equipment / learned spells | Move / initial priority word |
| --- | --- | --- | --- | --- |
|Zombie8|11 /27 /0|26,20,18,5|None /none|REGULAR /PRIORITYMOD_2 with BIT5|
|Golem9|12 /24 /0|28,24,19,4|None /none|REGULAR /PRIORITYMOD_2 with BIT5|
|Evil Cloud42|10 /22 /0|24,18,17,5|None /none|HOVERING /PRIORITYMOD_2|
|Death Archer58|10 /27 /0|13,19,20,5|equipped Iron Arrow /none|ARCHER /PRIORITYMOD_2 with BIT4|
|Master Mage69|14 /28 /14|11,18,22,5|equipped Iron Rod /Blaze2|MAGE /PRIORITYMOD_2 with BIT4|
|Black Monk78|14 /30 /19|27,18,21,6|None /Heal2, Blast1|HEALER /PRIORITYMOD_3|

All six start with status NONE and REGULAR spell power; definition resistance/
prowess and equipment remain part of refresh, not omitted constants of a universal
physical attack. Copy→HP/MP maxima→positions→movetype/placement commandset→actual
enemy index→regions/orders→added item→activation→difficulty→UpdateCombatantStats
is the consumer order. Here added NOTHING performs no AddItem. Activation retains
only the definition's upper priority nibble, plus source spawn/filler: initial
words are `$2000` except Monks `$3000`; BIT4/5 do not survive as initially active bits.
AdjustEnemyBaseAttForDifficulty's paired BEQs do **not** skip non-SUPER: all
0–3 difficulties apply floor(baseATT*5/4), SUPER2 applies it twice; equipment/stat
refresh follows. Difficulty comes from flags78/79, not the displayed level.
Slots143–159 are undeclared/HP0/maxHP0/X−1, not additional hidden enemy rows.
All fifteen declared rows are STARTING; their normal spawn mask0 adds no respawns.

## Completed repeats and actual enemy upgrade selection

**Confirmed:** `initializecombatants.asm:InitializeEnemyStats` calls
`upgradeenemies.asm:UpgradeRandomBattleEnemies` **before** copying the selected
definition. Its full consumer is `$1B1780..$1B1A28`; the included
`global/randombattles.asm`, `$1B6DB0..$1B6DDA`, length-prefix list contains14.
IsBattleUpgradable tests this list, not completion. DetermineBattleUpgrade then
tests514 for this selected battle. Clear514 returns each base type; held514 may
substitute it even though CheckBattle already cleared414.464 has no upgrade role.

Selection uses the original definition's movement upper nibble. All recognized
categories and their actual table bytes/exclusions are:

| Move category | Range byte R | Inclusive enemy-type bounds | Excluded IDs |
| --- | --- | --- | --- |
|REGULAR/CENTAUR/STEALTH/BRASS_GUNNER|5|Ooze0..Reaper35|Kraken Leg10, Pawn13, Knight14, Rat15, Burst Rock32, Zeon Guard38|
|FLYING/HOVERING|2|Huge Bat40..Demon52|Length0, with the unsafe scan described below|
|ARCHER/CENTAUR_ARCHER/STEALTH_ARCHER|2|Galam Archer56..Dark Gunner65|Kraken Arm59, Rook61|
|MAGE|1|Galam Mage67..Chaos Wizard74|Queen71|
|HEALER|2|Dark Cleric76..Evil Bishop84|Bishop80|

Thus Zombies/Golems use melee, Clouds airborne, Archers ranged, Mage mage and
Monks healer. An unrecognized movement category returns the original type.
These are ordinal type-ID bounds, not an equal-level or same-named-enemy guarantee;
not every excluded ID is even inside its category's bounds.

The actual level consumer zeros D0 for **Bowie0**, reads his level, then class;
signed byte class>11 adds20 (including supplied class values other than a normal
promoted Bowie). Let E be that effective level. It subtracts current battle14.
Zero or negative difference repeats the same level calculation once using shared
retry marker D4, then returns base if still nonpositive; it does not change E.
Positive difference DIVU10 then masks the quotient to its low byte q. q0 returns
base, so ordinary positive inputs first reach selection at E24. Source word and
signed-byte arithmetic remain relevant to corrupt/unsupported states; there is
no validation or general clamp into a legitimate class/level snapshot.

D3 starts as the base type. For R5 only, offset is `5*q−5`; for R1/2 it is q.
That offset is added to D3 and copied to D2 as the **retry baseline**, despite
the comment calling it a strongest enemy. D6 becomes R+1 and RNG produces
D7 in `[0,R]`, which is added to D3. The first/last bound comparisons are signed
byte branches. A below/above-bound or exclusion hit uses the one D4 retry:
reset D3 to saved D2, draw again with the same D6, then retry the checks. A second
failure returns base; it never clamps to an endpoint or keeps drawing until success.
A successful candidate is zero-extended from D3.b. Every attempted draw advances
the shared seed, even if ultimately falling back. Different rows' upgrade calls
therefore affect later selection/turn RNG. This is not a formation-wide one-time draw.

The empty airborne exclusion is a real defect, not an accepted empty-set filter:
`excludedEnemies` emits count0; the consumer subtracts1 to D1.w=$FFFF **without a
zero guard**, then reads the next byte and DBFs. Absent an earlier equality it
can inspect65,536 bytes past the declared empty list, starting in the following
ranged definition and continuing into subsequent storage. An accidental matching
byte uses the same retry/fallback path; exhaustion can instead accept a candidate.
The source comment says upgrade failure, but this code does not justify a safe
unconditional base return or a valid exclusion domain. No new ROM scan/runtime
simulation was run to assert all resulting Cloud substitutions. This out-of-table
behavior and actual natural seed/level remain explicit limits.

After selection the copied type changes HP/MP/base stats, equipment, spells,
movement and priority, while placement X/Y, commandset, regions and move orders
are reapplied from the same spriteset row. A substituted Monk still has HEALER1/3
as declared for that slot. Do not reuse the base stats or held Iron Rod when
reasoning about a repeat drop, first queue or command legality.

## Regions, first queue and conditional control

**Confirmed:** the three twelve-byte region records are:

| Region / flag | Type and ordered vertices |
| --- | --- |
|0 /90|Quad `(4,12),(22,28),(31,18),(14,1)`|
|1 /91|Triangle `(16,31),(31,31),(31,17)`; fourth pair is padding|
|2 /92|Quad `(12,0),(0,12),(18,31),(31,18)`|

`triggerregions.asm:IsRegionTriggered` uses triangle1/2/4 then3/2/4 for quads;
triangle uses1/2/3. Its dominant-axis integer projection uses signed division
with truncation, collinear segment bounds and source side flags. It scans all
thirty allies with positive signed HP and nonnegative X/Y, **not membership**.
Untouched declared ally positions X1–4/Y23–25 miss all three under that consumer;
region1 lies at X≥16, and those rows lie west of both quads' relevant edges.
Changed live allies, including nonmembers, can trigger regions. Geometry is not
a terrain/collision route proof.

Fresh entry clears90–105 but not NEWLY_TRIGGERED_BATTLE_REGIONS. ActivateEnemies
checks128–159 and its missing outer RTS falls through once for160; eligible
nonnegative-X/nonzero-HP enemy state invokes region testing. The shared checked
word suppresses tests already marked and marks misses too; with clear incoming
checked bits/unchanged formation it becomes7 while flags90–92 remain clear.
Incoming checked bits may suppress even changed allies. A later non-swarm AI
entry clears this word. The [extra160/deals alias owner](./battle26-activation-and-first-turn-dispatch.md#the-extra-fall-through-and-its-selected-ram-alias)
retains its conditional boundary; an empty local hook does not remove that caller.

Rows0–2 check primary0 then secondary1; rows12–14 primary2/secondary15; other
rows15/15 need no region bit to enter commands. A held primary sets bit0 and
returns, preserving other bits; only a missed/absent primary reaches a held
secondary and sets bits0/1. Thus secondary1 matters to0–2 when0 is not active;
a later primary does not automatically clear a retained secondary bit. Normal
fresh masks remain `$2000/$3000` until a qualifying trigger. FOLLOW_ENEMY13 encodes
$8D, combatant141 (row13 Monk); FOLLOW_ENEMY14=$8E targets142 (row14 Golem),
not ally13/14, self rows or AI points. There are no point references. GetMoveOrderPosition
reads the followed combatant's live X/Y; StartAiControl promotes secondary NONE
if primary target HP0. Declaration is not a performed follow/move.

GenerateBattleTurnOrder clears64 two-byte records; offers allies0–29 then enemies
128–157 with X≥0 and HP≠0, irrespective of Y/membership/neutral/region-active bit.
All fifteen base enemies can enter; base AGIs in row order are
19,19,17,22,18,17,20,21,18,20,19,18,18,21,19, none with the second-turn bit.
Ordinary base queue has15+N+T records, where N is eligible allies and T their
second turns (at most39 for twelve ordinary placed allies). Changed/substituted
stats can change both eligibility and second turns. Shared advancing RNG perturbs
AGI, stores low bytes and signed-sorts, preserving append-order ties. There is
no fixed first actor, first named player or required number of rounds.

Cursor0's nonFF reaches ExecuteIndividualTurn; FF repeats the round prefix.
Actual body clears death-list length and exits HP0; MUDDLE/AI_CONTROLLED, enemy
versus control-opponent, and ally auto-battle determine AI entry. Cursor/view/UI
and movement construction still precede Sleep/Stun's action rejection. First
control is a conditional dispatch, not completed input or action.

For these non-swarm rows, inactive region-linked actors take standby/STAY;
otherwise the actual command streams in `global/aicommandsets.asm` are:
HEALER1=HEAL1,SUPPORT,ATTACK1,MOVE1,STAY;
HEALER3=HEAL2,MOVE_ORDER4,SUPPORT,ATTACK3,MOVE2,STAY;
ATTACKER1=ATTACK1,HEAL1,SUPPORT,MOVE1,STAY;
ATTACKER2=MOVE_ORDER1,ATTACK1,HEAL1,SUPPORT,MOVE1,STAY;
ATTACKER3=MOVE_ORDER3,ATTACK3,HEAL2,SUPPORT,MOVE_ORDER4,MOVE2,STAY.
StartAiControl/ExecuteAiCommand stop at the first success; HEALER3/ATTACKER3 use
the nonmovable-space pathfinding preparation. This chapter binds the real entry
and orders, leaving all tactics, spell/item/target/move sequences and outcomes Unknown.

Held88 instead restores saved seconds, clears88/AI memory/view target, LoadBattle
with incoming D0 and jumps to the supplied individual-turn cursor. It skips fresh
selector assignment, region reset, healing/placement/upgrade/hooks and new queue.
LoadBattle's physical reconstruction is still a service, not proof of unchanged
saved state. Valid saved selectors/party/queue and natural resume remain Unknown.

## Terminal, Power Stick, victory and retreats

**Confirmed:** after a returned individual turn the loop runs defeated hook →
ProcessKilledCombatants → faction counts, testing allies before enemies; then
actual actor's after-turn effects → deaths → the same ordered counts. There is
no second defeated hook after status effects. Counts use X≥0/HP≠0, ignore Y and
membership; BowieHP0 forces ally count0. Simultaneous exhaustion loses. Defeated
wrapper's Bowie-alive/enemy128-dead/514-clear branch runs empty14; held514 skips
that script. Both reach leader-presence byte0, so killing row0 Golem alone does
**not** sweep the other enemies or end14. All counted enemies must be gone for
victory with a surviving ally count. Poison/death cleanup and the debug kill-all
branch are not action-drop callers; cleanup moves positions offscreen, clears
status, refreshes stats and credits kills/defeats, without itself awarding items.

`global/enemyitemdrops.asm` selects14, row3→combatant131, POWER_STICK89,
droppedFlag5. `sf2macros.asm:enemyEntity` adds the enemy bit. The actual
`battleactionsengine_1.asm` calls battlesceneScript_DropEnemyItem after action
effect for target iteration, second attack and counter; actor/target pointers
at that site determine the recipient. It requires ally actor, enemy target and
local targetDies set, then matching battle/entity **and actual held item** via
GetItemSlotContainingIndex's four-slot masked lookup. It neither conjures the
table item nor treats generic removal/poison/leader cleanup as such a hit.

Unchanged base131 holds Iron Rod88, not Power Stick89; placement adds NOTHING.
It therefore fails this item-presence test. A completed-repeat Mage substitution
can change that: definition70 Dark Madam carries equipped Power Stick; excluded71
Queen also declares it, whereas72 Wizard has Flail and74 Chaos Wizard Great Rod.
For example E24–33 gives q1:69+1 plus range2 draw0/1 yields70 or excluded71;
retry can accept70 or fall back69. This is a finite source illustration, not a
simulated/naturally reached repeat or exhaustive upgrade catalogue.

Power Stick is not one of the three special random-drop items; unreachable
IsBattleUpgradable code below that branch does not impose another roll. On a
matching held item, bset of bit5 in ENEMY_ITEM_DROPPED_FLAGS precedes removal;
already-set returns without removal/award. Clear bit becomes set, target item
is removed, then actual actor HP≠0 and AddItem capacity determine receipt.
Dead/full recipient falls back only for RARE items. `items/itemdefs.asm` row89
has WEAPON, no RARE: this fallback loses the removed Stick, with bit5 still set.
A living recipient with space gets the item and drop/receipt messages; it is
not automatically Bowie or a post-battle reward. Battle14 victory does not clear
this separate drop bit. No durable inventory/save claim is made.

Victory orders eligible healing → member-derived return seed → after/join wrapper →
clear414/set514 → D4=1. Seed is CURRENT_MAP plus the first actual active member's
X/Y translated by crop `(15,26)` as bytes, mapped facing, PARAM1=0; no empty-list,
HP or mapping validation occurs here. Empty after14 posts no replacement warp.
Both first/repeat wins retain that seed and perform shared JoinForce0;464 is not
cleared. Completion writes reread live CURRENT_BATTLE, return reads parameter2–5;
interruptions/changed selectors do not make these operations atomic.

| Unchanged14 route | Actual result boundary |
| --- | --- |
|Defeat|UpdateBattleUnlockedFlag; sad music/text363/close; restore BowieHP=max; unsigned gold>>1; GetEgressPositionForBattle; D4=−1. No battle4 losable override.|
|Admitted Egress|Selected learned spell, confirmed target, sufficient MP (cost8); debit MP, text274, common cast/flash/close/unlock, unwind individual-turn frame, egress helper, D4.w=0. The special execution branch adds no Silence rejection.|
|Admitted Angel Wing|Selected held usable consumable4/confirmed target; remove selected item, text275, common escape suffix/helper, D4.w=0; no half-gold charge.|
|Cancellation/illegal target/insufficient MP/Sleep or Stun|No admitted escape merely from menu/handler presence.|
|Normal Suspend|14 passes battle0's pre-prompt exclusion. Prompt0/YesNo: negative returns menu; nonnegative saves seconds, sets88, SaveGame, fade/sound→WitchSuspend. Debug+Start clears88 and returns menu **after save**. No after14, completion or exploration-return suffix.|

UpdateBattleUnlockedFlag leaves414 supplied with514 clear and clears414 when514
held; defeat/escape do not newly set514/464. Actual14 has no egress special case:
EGRESS_MAP feeds GetSavepointForMap, not victory seed/old coordinates. With399 held,
Ribble8 returns22,22 Up, New Granseal6 returns11,22 Up, other supplied maps use
their row or retain map/default1,1 Up. If supplied399 was cleared, fallback is
3,56,3 Down. Held64 also resets raft fields through its selected table. Earlier
items/HP/status/flag writes can remain; saves, town service and a natural retreat
itinerary remain Unknown. [Battle0's Suspend exclusion](./battle00-bonus-battle-and-reset.md)
does not apply to14.

## MainLoop return and Map70 exits

**Confirmed:** actual14 caller is MainLoop. After BattleLoop returns it runs
SwitchMap then ExplorationLoop **without another CheckBattle**. Unchanged victory
seed is explicit70, so ordinary fresh exploration heals eligible allies, sets
not-in-battle, rebuilds main/followers/empty fixed population, clears temp256–383,
sets80, loads map and runs empty init. Current combat entities are not simply
retained as field actors. An externally changed seed CURRENT255 instead takes
UpdateMainEntityProperties/DeclareRaftEntity and retained-load arm; it does not
perform the explicit-map entity/temp reset, but shared init still runs. No empty
local hook creates CURRENT255 by itself. Egress/defeat use their actual destinations.
Step subtraction at loop entry applies to either branch.

All three `map70/6-warp-events.asm` rows require the ordinary warp producer's
layout attribute and matching coordinates; they do not prove collision-free paths:

| Source X,Y / mode | Full request destination | Actual consumer boundary |
| --- | --- | --- |
|43,40 /no-scroll|62,6,18,Up|Type1 is cleared before dispatch; warp sound/idle/discard exploration return frame; MainLoop SwitchMap and CheckBattle. No62 substitution or battle row; ordinary62 entry selects default/F77 setup, rebuilds actors and runs empty init.|
|0,255 /scroll Left|69,62,255,Left|Sets CURRENT_MAP69, ProcessMapTransition, X62 with current Y retained; DeclareRaftEntity/current actor continuation. No ordinary entry, switch, battle scan, init or new-facing assignment.|
|255,0 /scroll Up|71,255,62,Up|Scrolling consumer explicitly checks destination71/F530: held substitutes78, clear keeps71; transition retains current X and sets Y62. No general SwitchMap, battle scan, setup/init/rebuild or consumption of parameter5 as facing.|

The northern substitution is present in the actual scroll consumer, so absence
of a SwitchMap call does not mean71 is always kept. In either scrolling route,
parameter5 stores the requested facing while actual current actor facing is
retained by this path. Raft/follower adjustment does not establish membership or
next-town service. A later no-scroll/reload can run a different admission scan:
69 may select9 before17 from held unlocks; scroll itself consumes neither.
Use [East Shrine visit](./battle10-outcomes-through-east-shrine-visit.md),
[its return route](./east-shrine-return-through-battle11-admission.md),
[Map69 travel](./polca-raft-boarding-through-south-parmecia-entry.md) and
[71/78/Pacalon boundary](./battle26-return-through-pacalon-entry.md) for adjacent
accepted accounts. Stop at the named admissions/scroll handoffs; no full revisit,
Creed/Pacalon/tunnel/service itinerary is implied.

## Actual producer and current remake consumers

**Confirmed by accepted-base code inspection:** no14 lowering, package preparation
or engine run was performed. An existing type or parser is not14 execution.

| Surface | Actual consumer and limitation |
| --- | --- |
|H2 producers|`src/sf2tool/h2/` map setup/event/data/init/script, battle global/routing/spriteset/cutscene/loop owners retain source operands, tables and wrappers. They do not produce a naturally reached state or automatically make an encounter playable.|
|Python conversion|`remake_exploration_programs.py:OriginalPrograms.compile` has an actual CheckRandomBattle case emitting source-random-battle with D0 and an onRequest program. Other unrecognized assembly reaches native-call. `remake_exploration_battles.py:compile_routes` reads selected battle IDs, wildcard rows and hooks, builds wait/boost/flash presentation plus optional outcome hooks. These mechanisms are separate from `prepare_encounter`, which explicitly admits2/3 only; there is no14 package from this task.|
|Content|`ExplorationContentReader.cs` admits SourceRandomBattle IDs0–43 and resolves onRequest; native-call becomes UnsupportedInstruction. `PrivateBattleScenarioReader.Assemble` admits1/2/3 only, requires STARTING/selected item expressions and lacks HEALER3, BIT4 enemy words, MAGE/ARCHER movement and wider class/derived-state admission. Generic geometry/FOLLOW_ENEMY expression does not admit14 or repeat substitutions.|
|Domain|`BattleInitializationRules.cs` requires controlled roster-only/already-refreshed starts, difficulty0, limited enemy loadouts0/127 and no auto/opponent control; it consumes given definitions, not the source514/category upgrade algorithm. `Gameplay/Sf2/SourceEnemyAi.cs` reaches standby/order logic but activated commandsets only6/7/9/14, no learned spells or equipped items;14's Monks/Mage/Archers need capabilities beyond that subset. Generic physical/heal/terminal rules are not the complete action-drop/upgrade encounter.|
|Application random request|`SourceRandomBattleRunner.Check` really consumes completion, nullable step counter, main RNG and writes flag/counter/presentation continuation. Unbound repeat counter rejects; Consume uses BattleEntry.First on admitted routes and throws when no route, rather than reconstructing all raw retained map-event bytes/ordinary no-battle reload. `ExplorationDispatcher` decrements counter for moved field input. This is implemented bounded request behavior, not proof of every original caller byte or14 admission.|
|Application admission/return|`BattleEntry.cs` routes selected hooks, flags and initialization. `Sf2StoryPolicy.EnterCompletedBattle` permits completed admission for PrivateDefinitions, while authored policy declines; this supplies no upgrade algorithm. Suspended controlled entry rejects. `BattleOutcome.cs` uses selected outcome-return policy and declared hooks/join/flags. Full14 upgrades, loadouts/drop and caller composition are not supplied by those generic stages. `ProgramRunner.cs` stops at UnsupportedInstruction before that instruction commits; earlier prefixes can remain.|

C# paths are under `remake/src/` in their named Content/Domain/Application projects;
Python conversion files are under `src/sf2tool/`. Production code is evidence of
its own limits, never evidence of the original. This research does not extend
English design, original runtime observations, current playable campaign or PR692.

## Reproduction, review and remaining Unknowns

Verify the configured read-only pin/clean source, then read the named full14
spriteset, all six definitions, full upgrade consumer/category tables/macros/RNG,
selected hooks and their include/wrapper/terminator, and actual zone/request/step/
selection/initialization/region/queue/action-drop/terminal/escape/Suspend/return
consumers together. The retained18,285,979-byte init/script pair and its provenance
can be reused without regeneration; Map70 init is body77. Existing fixtures under
`tests/fixtures/h2/` include map-data/setup/events/init/script, map-event-random-battle-
state/request-consumption, battle-global/routing/spriteset/cutscene/loop/control
owners. Named fixture existence is not new semantic/runtime verification.

The Issue handoff records claim-to-source/consumer/prose review and changed-state
checks:410/414/464/510/514/88/78/79/530, request overwrite/decline, party/HP/class/
level/RNG, primary/secondary/checked bits, base/substituted loadout, dropped bit5,
recipient capacity, first/repeat/interrupted outcomes and scroll/fresh/retained
caller differences. Check exact three-file diff, local links/anchors, UTF-8/LF,
private boundary and unchanged ledger tables/counters. On clean committed head,
load ignored `local/private-inputs.ps1` in the same process then run
`uv run sf2 verify plan --base origin/main --head HEAD`, interpreting its plan
under the docs policy, and inspect actual exact-head scoped CI. No normal/full,
SDK/engine/Godot/H3/H4/emulator/screenshots, old comparison or lowering is selected
merely for this prose. The handoff contains any one-off check's complete recipe.

**Unknown:** natural caller/route/state/seed/level, out-of-table airborne scan's
actual results, physical routes/collision, all tactics/actions, real service/wait/
control/presentation completion and save durability. These are not passed results
or an automatic acquisition queue. Fairy Woods44 and optional-content gaps remain;
no Epic1053/full-game closure is claimed. Retain1200's initial author/main/CI PASS
omissions and both corrections,1198's high-byte/startEntity0 corrections,1196's
runtime-boundary/1194's Suspend corrections and #1047's twelve completed unresolved
comparisons in their owning records. They are not interrupted suites to restart.
