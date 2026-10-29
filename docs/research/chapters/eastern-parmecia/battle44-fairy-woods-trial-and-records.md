# Fairy Woods trial, Battle44 and records

Evidence date: 2026-10-22. **Confirmed:** this owner joins the selected Map23 east
exit to Map24 invitation, Battle44 and conditional results/returns using pinned
static source. It is distinct from Fairy Cave32. **Unknown:** natural entry and
caller state, completed presentation, exact visible time and save durability.

## Agreement and provenance

Standard rigor under [Issue #1204](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1204)
and [Epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
one connected optional trial crosses map admission, combat population, timer and
result consumers. Accepted starting base is
`547670fed6176618d870e1457c8574a3f3e110c7`. Own this document, only the Battle44
local gap/link/recommendation in [story coverage](../../story-content-coverage.md), and
a short [source frontier](../../source-coverage.md) reference; no global table/count,
code, schema, fixture, test or asset change. No stacked dependency. Stop at the
accepted Elven Village return/admission boundary. Exclude all-tactics simulation,
terrain reachability, complete village revisits, new acquisition, runtime,
English design and engine implementation. Static coverage does not close the epic.

All original source paths are relative to `disasm/` at
[SF2DISASM master `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
Retained map-init/map-script-engine evidence binds that pin and USA retail ROM
SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Reuse the existing 18,285,979-byte pair; no rebuild or new ROM parity claim.
Coordinates, waits, region vertices and control requests are source operands,
not observed routes, duration or rendered positions. Fnnn denotes flag nnn;
indices are zero-based. Selected dialogue is paraphrased with numeric IDs and
control/substitution tokens; no source transcript or extracted content is published.

Finite selected-source and actual-consumer reads suffice. Direct checks target
under 60 seconds and 512 MiB; new local receipts under 4 MiB, linear in selected
text/retained evidence, without a corpus, simulator or new infrastructure.
Reassess material expansion before proceeding. Documentation acceptance uses
manual source-to-consumer-to-prose review, direct links/anchors/encoding/scope/
private-boundary checks, a clean committed planner under the documentation policy,
and exact-head scoped CI. No normal/full, SDK, engine, Godot, H3/H4 or emulator run.

## East exit, admission and invitation

**Confirmed:** `data/maps/entries/map23/6-warp-events.asm`'s fourth row matches
`(31,24)` and requests no-scroll Map24, `(1,27)`, Right. Earlier north/west/south
rows do not match this tile. The accepted [departure owner](../southern-travel-creed/elven-village-departure-through-harpy-pond-entry.md)
covers the other exits; [local exploration](../southern-travel-creed/elven-village-conversations-and-local-exploration.md)
covers village conversations/search/stairs. Map24 is the Elven Village battlefield,
not the separate Fairy Cave Map32. No mandatory story position or physically walked
route follows from this warp.

`exploration.asm:WarpIfSetAtPoint`, `explorationfunctions_2.asm:ProcessMapEvent`
and `code/gameflow/mainloop.asm:MainLoop` supply the actual request/consumer chain:
ordered tile matching, pending warp parameters and SFX, no-scroll unwind, SwitchMap,
then CheckBattle **before** exploration population/init. The complete
`data/maps/global/flagswitchedmaps.asm` has no source23/24 substitution.
`code/common/maps/getbattle.asm:CheckBattle` and the complete
`data/battles/global/battlemapcoords.asm` select sole Map24 row44: origin `(0,0)`,
18x32, trigger X/Y both255 (wildcards), unlock444 and completion544. The crop is
battle-local geometry, not an additional admission-containment test. F494 is the
intro/start flag; it is not the unlock. Supplied F88 selects the common saved-battle
path only after battle admission, rather than changing this row.

| Supplied state on a MainLoop admission | Static continuation |
| --- | --- |
| F444 clear, F544 clear | No Battle44; fresh Map24 exploration then invitation init. |
| F444 clear, F544 held | No Battle44; fresh Map24 exploration then result init, using supplied snapshot. No preceding victory is proved by the flag alone. |
| F444 set, F544 clear | Battle44 admitted before invitation/init. Fresh or suspended entry then depends on F88. |
| F444 set, F544 held | Row44 is selected; CheckBattle clears444 but still admits this call. F544 can suppress defeated/after scripts; it does not cancel admission. |

Fresh supplied-map `ExplorationLoop` clears pending, subtracts20000 from the signed
word step counter with negative-result clamp0, heals living/immortal allies, loads
Map24 tilesets and writes CURRENT_MAP24/CURRENT_BATTLE255, rebuilds actors, clears
temporary flags256..383, sets F80, loads map, installs base VInt functions and calls
selected init. `data/maps/mapsetups.asm` has only `ms_map24`; its pointer table
selects an empty fixed-entity list, return-only default entity/zone/item handlers,
return-only area-description body, and `s6_initfunction.asm`. Empty fixed actors do
not remove supplied followers or main0 allocated by shared population. These are
setup declarations and requested control, not observation of a visible fairy NPC.

Map24 init requests HEADQUARTERS music, FadeInFromBlack and speech SFX2 before
checking544. With544 clear, text467 welcomes the special stage,468 asks how fast
all hidden monsters can be defeated (both use `{N}`);469 invites a new record.
All three terminate `{W2}`. Text470 formats the best minutes using `{N}{#}`,
then471 uses `{DICT}`, `{#}` seconds and `{W2}`. Text472 asks readiness. The numeric
source DIVS60 format is detailed below; it is not a hardware stopwatch claim.

The actual `code/common/menus/yesnoprompt.asm:alt_YesNoPrompt` starts with choice0
and no initial release-wait operand. Left selects0, Right selects-1 (stored byte255);
C/A initially zero-extend the choice byte, but the shared return epilogue reloads
CURRENT_DIAMOND_MENU_CHOICE and applies EXT.W before RTS. Thus the final returned
word is0 for Yes and-1 for confirmed No; B also ultimately returns-1. Direction
checks precede B, then C, then A. Init tests that final **word** for zero, so only
0 accepts; confirmed No and B both decline. Accepted text473 is the start
cue (`{W2}`), followed by `cs_StartSpecialBattle`: setStoryFlag44 means F444,
then warp24 `(1,24)`, Left. This is a pending request subsequently consumed by
exploration/MainLoop, not an immediate direct BattleLoop call. Decline text474
invites another visit (`{N}`, `{W1}`), then `cs_LeaveSpecialBattle` requests
23 `(30,24)`, Left. Neither invitation branch clears a supplied544;544 clear is
already the invitation predicate. Neither start nor decline produces a new story
prerequisite elsewhere.

## Fresh battle, hooks and actual force placement

**Confirmed:** `battleloop_1.asm:BattleLoop` clears PLAYER_TYPE, sets399 and tests88.
F88-clear resets only the long SECONDS_COUNTER, writes current map/battle around
SetBaseVIntFunctions and ExecuteBeforeBattleCutscene, clears flags90..105, heals
living/immortal allies, initializes ally/enemy positions, clears AI memory, LoadBattle,
then ExecuteBattleStartCutscene. It does not reset the separate frame subcounter
here. F88-held instead restores SAVED_SECONDS_COUNTER, clears88/AI memory, LoadBattle
and resumes the saved CURRENT_BATTLE_TURN; it skips fresh formation, region clearing,
healing, before/start hooks and new queue generation at this entry.

The actual build `layout/sf2-07-0x044000-0x064000.asm` includes
`data/battles/cutscenes/{beforebattlecutscenes,battlestartcutscenes,battleendcutscenes,afterbattlecutscenes}.asm`.
All four selected row44 offsets resolve to `ms_Empty`; the common wrappers still
execute. Before tests494; start tests/sets494 even though its body is empty.
Defeated tests living Bowie, dead enemy128 and544 before its empty script; the
leader sweep remains after that script/completion gate. After tests544 before its
empty script but always reaches its join tail. No row44 region-cutscene entry or
neutral-entity record is present in the actual selected tables.

The included join table is **`data/battles/cutscenes/afterbattlejoins.asm`**, not
the similarly named unused global alternate. Row44 is0. The after wrapper calls
`j_JoinForce` with0 even when the script is skipped. `code/common/stats/battleparty.asm:JoinForce`
sets Bowie's joined flag, updates force lists, and joins him to the battle party
when its count is below12; it is not a new named ally award. The separately called
`ApplyPositionsAfterEnemyLeaderDies` checks living Bowie/dead128, but its actual
`global/afterbattlepositions.asm` has only Battle5; no44 repositioning occurs.
`entityfunctions_1.asm:PositionBattleEntities` also has no44 neutral record;
its completion-held shortcut does not manufacture one.

`data/battles/spritesets/spriteset44.asm:BattleSpriteset44` declares **12 allies,
15 enemies,14 regions,1 point**. `battleloop/initializecombatants.asm:InitializeAllAlliesBattlePositions`
first sets all30 ally X=-1, updates force, then takes actual battle-party IDs in
order. UpdateForce scans IDs0..29 and includes joined-and-active flag pairs; it
does not use spriteset identity bytes as a recruitment list. A dead member consumes its party entry without advancing the placement
pointer; living members consume successive placement rows. Therefore the row's
printed ally number is not a guaranteed character identity. Remaining party
members after placement capacity are removed from the active party. All12 rows
carry HEALER1/NOTHING and behavior NONE/15,NONE/15,activation0,STARTING; this ally
position initializer reads their X/Y, not those values as a new ally AI policy.

| Placement ordinal | Local coordinate = map coordinate (crop0,0) |
| --- | --- |
| 0 | (11,28) |
| 1 | (11,29) |
| 2 | (11,30) |
| 3 | (10,30) |
| 4 | (12,30) |
| 5 | (12,29) |
| 6 | (10,29) |
| 7 | (9,30) |
| 8 | (13,30) |
| 9 | (13,29) |
| 10 | (9,29) |
| 11 | (14,30) |

Healing is shared `battleloop/heallivingandimmortalallies.asm`: all30 slots with
nonzero HP, plus Peter/Lemon regardless of HP, get maxHP/MP. The intermediate
status AND7 retains STUN/POISON/CURSE, then UpdateCombatantStats recomputes CURSE
from equipped cursed items. Supplied CURSE without cursed equipment clears;
cursed equipment sets it. This is not unconditional final CURSE preservation,
and ordinary dead allies remain dead. Fresh battle, victory and exploration
call this shared rule at their respective points.

## Complete enemy records and initialization

**Confirmed:** enemy ordinal n maps to combatant128+n; every placement item is
NOTHING, every behavior filler is0, every secondary order/region is NONE/15.
The following is the complete selected formation, with local coordinates also
map coordinates because the actual crop is0,0. Region15 means absent.

| Ordinal / combatant | Enemy definition | X,Y | Commandset | Primary order / region | Source spawn |
| --- | --- | --- | --- | --- | --- |
| 0 /128 | BUBBLING_OOZE16 | 9,2 | LEADER14 | MOVE_TO point0 /15 | STARTING0 |
| 1 /129 | BLUE_SHAMAN_0 102 | 5,2 | HEALER5 4 | FOLLOW_ENEMY0 /0 | 3 |
| 2 /130 | ZOMBIE8 | 14,4 | ATTACKER2 7 | NONE /1 | 3 |
| 3 /131 | MIST_DEMON50 | 9,6 | ATTACKER1 6 | NONE /2 | 3 |
| 4 /132 | MIST_DEMON50 | 4,6 | ATTACKER2 7 | NONE /3 | HIDDEN2 |
| 5 /133 | LESSER_DEMON45 | 13,8 | ATTACKER2 7 | NONE /4 | HIDDEN2 |
| 6 /134 | LESSER_DEMON45 | 6,10 | ATTACKER2 7 | NONE /5 | HIDDEN2 |
| 7 /135 | ORC_LORD22 | 4,13 | ATTACKER2 7 | NONE /6 | 3 |
| 8 /136 | MIST_DEMON50 | 9,12 | ATTACKER2 7 | NONE /7 | 3 |
| 9 /137 | MASTER_MONK82 | 15,11 | HEALER1 0 | NONE /8 | HIDDEN2 |
| 10 /138 | DARK_SOLDIER18 | 10,16 | ATTACKER2 7 | NONE /9 | HIDDEN2 |
| 11 /139 | DARK_MADAM70 | 14,16 | ATTACKER2 7 | NONE /10 | 3 |
| 12 /140 | HARPY44 | 5,20 | ATTACKER2 7 | NONE /11 | 3 |
| 13 /141 | MIST_DEMON50 | 12,21 | ATTACKER2 7 | NONE /12 | 3 |
| 14 /142 | LESSER_DEMON45 | 9,23 | ATTACKER2 7 | NONE /13 | 3 |

`sf2macros.asm:combatantBehavior` encodes primary order/region, secondary
order/region, filler byte, spawn byte. `sf2enums.asm` defines STARTING0,
RESPAWN1 and HIDDEN2; literal3 is the combined respawn/hidden mode, not a fourth
named unrelated rule. MOVE_TO is64 and indexes the point subsection with its low
nibble: point0 is `(9,2)`. FOLLOW_ENEMY is128; `GetMoveOrderPosition` instead reads
combatant128's live coordinates. It does not follow ally0 or point0 merely because
the suffix is0. `ai/startaicontrol.asm:@CheckSpecialMoveOrders` promotes the
secondary order and sets it to NONE when a followed combatant has zero HP; here
that can replace Shaman's follow128 with NONE. No tactical success/path is inferred.

`data/stats/enemies/enemydefs.asm` supplies these ten distinct definitions. The
numbers below are **source base** values before difficulty/equipment/status
refresh, not displayed/current attack. All initialStatus values are NONE. Both
item and spell inventories have four slots; unnamed slots are NOTHING. Power R/E means
REGULAR/ENHANCED spellPower. All AGI entries are below the second-turn bit128.

| Definition | Level | HP/MP | ATT/DEF/AGI/MOV | Power; move type | Held loadout (E = equipped) | Learned spells (unmarked level1) |
| --- | --- | --- | --- | --- | --- | --- |
|16 Bubbling Ooze|21|32/0|38/35/26/4|R; REGULAR|None|AQUA2|
|102 Blue Shaman_0|42|72/60|64/54/51/5|E; HEALER|HOLY_STAFF E, HEALING_RAIN|AURA3, BLAST4|
|8 Zombie|11|27/0|26/20/18/5|R; REGULAR|None|None|
|50 Mist Demon|36|68/28|80/41/51/6|E; HOVERING|None|MUDDLE2|
|45 Lesser Demon|22|40/21|49/37/32/6|E; FLYING|None|BLAZE3, BOOST2|
|22 Orc Lord|27|46/13|40/44/37/5|E; REGULAR|HEAVY_LANCE E|DISPEL|
|82 Master Monk|28|47/39|22/45/31/6|E; HEALER|BRASS_KNUCKLES E|HEAL4, DISPEL|
|18 Dark Soldier|23|35/17|31/46/34/4|E; REGULAR|LARGE_AXE E|ATTACK|
|70 Dark Madam|17|29/17|19/31/29/6|R; MAGE|POWER_STICK E|FREEZE2|
|44 Harpy|17|32/10|39/38/29/7|R; FLYING|None|DISPEL|

Blue Shaman_0 is numeric102, distinct from BLUE_SHAMAN85:102 includes Healing
Rain in slot1. A spriteset item of NOTHING does not erase this base inventory.
Ooze's MP0 with AQUA2 is retained as declared; a learned entry does not prove its
cast is affordable or selected. The source's `unknownByte` is0 for82/102 and
matches the listed ID for the other eight definitions; it is not their identity
lookup authority. Resistance/prowess and original definition AI are also explicit:

| Definition | Base resistance expression | Base prowess expression | Definition AI expression |
| --- | --- | --- | --- |
|16|ICE_MAJOR, FIRE_WEAKNESS|CRITICAL_SILENCE, DOUBLE_1IN32, COUNTER_1IN16|PRIORITYMOD_2, BIT5|
|102|0|CRITICAL_SILENCE, DOUBLE_1IN32, COUNTER_1IN32|PRIORITYMOD_3|
|8|WIND_WEAKNESS, ICE_MAJOR, FIRE_WEAKNESS, STATUS_IMMUNITY|CRITICAL_POISON, DOUBLE_1IN32, COUNTER_1IN32|PRIORITYMOD_2, BIT5|
|50|WIND_WEAKNESS, ICE_MAJOR, STATUS_MINOR|CRITICAL_SILENCE, DOUBLE_1IN32, COUNTER_1IN16|PRIORITYMOD_2, BIT4|
|45|STATUS_MINOR|CRITICAL150_1IN32, DOUBLE_1IN32, COUNTER_1IN16|PRIORITYMOD_2, BIT4, BIT5|
|22|0|CRITICAL150_1IN8, DOUBLE_1IN32, COUNTER_1IN16|PRIORITYMOD_2, BIT5|
|82|0|CRITICAL125_1IN4, DOUBLE_1IN8, COUNTER_1IN16|PRIORITYMOD_3|
|18|0|CRITICAL150_1IN32, DOUBLE_1IN32, COUNTER_1IN16|PRIORITYMOD_2|
|70|ICE_MINOR|CRITICAL150_1IN32, DOUBLE_1IN32, COUNTER_1IN32|PRIORITYMOD_2, BIT4|
|44|WIND_WEAKNESS, FIRE_WEAKNESS|CRITICAL_POISON, DOUBLE_1IN32, COUNTER_1IN32|PRIORITYMOD_2, BIT4, BIT5|

`initializecombatants.asm:InitializeEnemyStats` calls UpgradeRandomBattleEnemies,
copies the56-byte selected definition, fills current HP/MP, assigns source X/Y,
commandset/move type, enemy ID, triggers/orders and optional placement item,
constructs activation, applies difficulty to baseATT, then UpdateCombatantStats.
The complete actual `global/randombattles.asm:list_RandomBattles` excludes44.
The shared upgrade consumer's IsBattleUpgradable gate therefore returns the
original definition even with544 held. The conditional Battle14 repeat upgrade,
including its airborne exclusion defect, must not be inherited by this trial.

For a STARTING enemy, activation construction preserves only the definition's
upper priority nibble and adds rotated spawn/filler operands. Ooze starts `$2000`,
not its original definition `$2020`; the commandset is the separate byte14.
Initial hidden modes take another branch: spawn low nibble>=2 becomes `$0200`
or `$0300`, maxHP/currentHP0 and X=-1; trigger regions are retained, rather than
copying a live definition/loadout yet. All17 undeclared enemy slots143..159 also
get zero HP and X=-1. Thus the declared15 are **one initialized living enemy and
fourteen dormant definitions**, not15 initially live enemies or15 kills required.
Actual initial activation can subsequently change at region/spawn consumers below.

`AdjustEnemyBaseAttForDifficulty` reaches its first multiply5/shift2 in every
supplied difficulty branch; SUPER repeats that truncated operation. Equipment is
then applied by `code/common/stats/updatecombatantstats.asm:UpdateCombatantStats`
and ApplyItemOnStats. Selected `data/stats/items/itemdefs.asm` gives attack bonuses:
Holy Staff29, Heavy Lance23, Brass Knuckles39, Large Axe25 and Power Stick15.
Holy Staff also declares HP_RECOVERY3; actual after-turn recovery tests equipped
Holy Staff. These are not blanket attacks equal to the base table, nor guaranteed
healing/action choices. The shared prowess/resistance/spell consumers remain
those of [battle functions](../../battle/battle-functions.md) and [battle AI](../../battle/battle-ai.md).

## Regions, checked bits, hidden appearance and reappearance

**Confirmed:** all14 complete12-byte region records in spriteset44 have unknown
byte0 and both trailing bytes0. Four coordinate pairs are stored even for the
single triangle (its unused fourth pair is0,0). Regions are local to the0,0 crop.

| Region / persistent flag | Vertex count | Ordered vertices |
| --- | --- | --- |
|0 /90|4|(0,0), (6,6), (11,2), (11,0)|
|1 /91|4|(14,2), (12,4), (14,6), (16,4)|
|2 /92|4|(6,0), (6,5), (9,8), (17,0)|
|3 /93|4|(0,1), (0,9), (6,9), (6,6)|
|4 /94|4|(13,6), (11,8), (13,10), (15,8)|
|5 /95|4|(6,7), (3,10), (6,13), (9,10)|
|6 /96|4|(4,10), (1,13), (4,16), (7,13)|
|7 /97|4|(9,9), (6,12), (9,15), (12,12)|
|8 /98|3|(17,8), (12,13), (17,13); stored fourth(0,0)|
|9 /99|4|(10,13), (7,16), (10,19), (13,16)|
|10 /100|4|(14,13), (11,16), (14,19), (17,16)|
|11 /101|4|(5,16), (1,20), (5,24), (9,20)|
|12 /102|4|(12,17), (8,21), (12,25), (16,21)|
|13 /103|4|(9,20), (6,23), (9,26), (12,23)|

The actual consumer is `battleloop/triggerregions.asm`, not the conceptual
comment alone. IsRegionTriggered accepts type3 as triangle1/2/3; other selected
type4 records use triangles1/2/4 and3/2/4. AreAnyAlliesInTriangle scans all30 ally
slots (IDs0..29: COMBATANT_ALLIES_COUNTER29 with DBF), requiring positive signed HP and nonnegative signed-byte X/Y, independently
of current active-party membership. It does not require the acting ally itself.
IsAllyInTriangle tests edges1→2,3→1,3→2 against the point and opposite vertex.
A segment result1 returns inside immediately; otherwise the low-bit exception
branches and equal side results control the continuation, with0 returned for
inside and low byte-1 for outside. The enclosing any-ally function inverts that
into-1 for a trigger and0 for none.

CheckTestPointPositionRelativeToSegment chooses horizontal when absolute ΔX>=ΔY,
otherwise vertical. It sign-extends coordinate bytes, uses signed word multiply,
then sign-extends the **low product word** before DIVS, truncates the quotient,
and adds B's coordinate to project the line at C. Horizontal zero denominator
falls to vertical; vertical zero denominator takes its degenerate branch.
Projection equality uses inclusive dominant-axis bounds; out-of-bounds and negative
projection enter explicit CCR synthesis paths. The result is masked to two bits
and consumed in the edge order above. Do not replace this source integer/CCR
predicate with an ideal continuous polygon or infer every boundary from comments.
The supplied region IDs0..13 are within count14; the separate sentinel is15.

TriggerRegionsAndActivateEnemies returns immediately for zero region count.
Otherwise NEWLY_TRIGGERED_BATTLE_REGIONS is a **checked-this-pass** word: set bits
skip geometry; after each tested region its bit is set whether or not triggered.
A true geometry result sets persistent F90+region; no false result clears it.
It then reads the supplied enemy's primary/secondary regions. A held primary
sets activation bit0 and returns; only otherwise is secondary tested, setting
bits0/1. Every44 secondary is15. Fresh battle clears90..105, but that loop does
not clear this checked word. ClearAiMemory is separate; the actual non-swarm
`ai/startaicontrol.asm` writes checked word0 before AI activation/standby handling.
Thus later tests depend on that consumer lifetime; it is not a freshly cleared
per-round word by declaration, nor a list of newly true regions. No natural
first trigger or fixed round is claimed.

`ActivateEnemies` considers all32 enemies but calls this trigger consumer only
for negative-byte enemy IDs with X>=0 and currentHP!=0. Initially Ooze can supply
that call; dormant enemies cannot. Round order is activation, region-cutscene
wrapper (no44 record), spawning list, each SpawnEnemyWithCamera, queue generation.
`populateenemyspawns.asm` scans all32 activation words masked with `$0F00`:

| Mode in this trial | Exact eligibility and lifetime |
| --- | --- |
|`$0200` HIDDEN (132/133/134/137/138)|UpdateEnemyActivationIfDead requires currentHP0 and a held non15 primary, otherwise secondary. It sets primary-active bit0 (secondary path also bit1). Only maxHP0 then permits ResetSpawningEnemyStats. Successful first appearance leaves maxHP>0; ordinary death does not reset maxHP, so this mode does not reappear again in the same unchanged battle.|
|`$0300` literal3 (129/130/131/135/136/139/140/141/142)|The same dead/held-region test, without the maxHP0 restriction, permits reset. A later death can therefore reappear at a later round prefix while its persistent flag remains held and reset succeeds. It does not regenerate immediately inside the death action or promise a finite total population.|
|STARTING0 (128)|No spawn candidate through this scan; it remains the source leader.|

ResetSpawningEnemyStats checks the declared enemy bound, then the row's **starting**
X/Y against every ally/enemy's coordinates. Occupancy does not filter dead HP or
exclude the same slot. It copies the full original definition again on success,
restoring max/current HP/MP, base loadout, positions, orders and triggers, applying
difficulty/equipment, and finally restores the passed **pre-reset activation word**.
For an initially hidden slot that includes its `$0200/$0300` mode and newly set
active bit, rather than silently inheriting the definition's priority/BIT4/BIT5.
Failed bound/occupancy sets max/currentHP0 and X=-1, sets carry and is not appended
to TARGETS_LIST; held flags/activation remain and allow later eligible retries.
For HIDDEN, a failed reset's maxHP0 still allows a later attempt; after a successful
appearance then death, maxHP>0 prevents it. Reinitializing a fresh trial is separate.

Successful candidates alone set TARGETS_LIST_LENGTH; SpawnEnemyWithCamera calls
SpawnEnemyEntity, requests camera/cursor work, the twelve source spin iterations
with delay3, SFX_SPAWN and text397 (`{CLEAR}{NAME}`, appearance, `{D3}`), then closes
text. These requests are not an observed animation duration or guarantee of
rendered completion. Death cleanup later moves X/Y to-1, clears status, refreshes
stats and parks the visual entity; it does not erase spawn mode, maxHP or region
flags. Actual occupancy/party/HP and region history determine reappearance.

## Queue, control, terminal and rewards

**Confirmed:** `battleloop/turnorderfunctions.asm:GenerateBattleTurnOrder` clears
all slots, enumerates placed/nonzero-HP allies then selected enemy slots, computes
randomized current agility, adds a second slot only for the high AGI bit, sorts
by signed-byte altered agility descending with equal entries left ordered, and
sets CURRENT_BATTLE_TURN0. Declared formation order and point0 do not select a
fixed first actor/round. Supplied allies, HP/status, derived agility and seed
remain inputs. Suspended entry resumes its saved queue instead of this prefix.

`battlefunctions/executeindividualturn.asm` clears the dead list and skips zero
HP. Muddle, AI_CONTROLLED, enemy/opponent-control and ally auto-battle choose AI
versus player paths; ordinary living nonmuddled/manual allies can reach cursor,
view, status/land windows, movement grid and player input. Sleep/Stun route to
NoAction. Menu cancellation can reopen the battlefield menu and return to the
same turn start. AI labels alone do not prove a chosen attack, spell, target or
move. The accepted [battle-loop](../../battle/battle-loop.md), [cutscene](../../battle/battle-cutscenes.md)
and [Battle28 account](../northern-travel-mitula/battle28-bridge-and-outcomes.md) retain shared details;
this owner does not simulate the complete tactics of these fifteen definitions.

The actual `global/enemyleaderpresence.asm` row44 is **-1 (present)**. Here enemy128
is Bubbling Ooze. After the individual action, ExecuteBattleCutscene_Defeated
requires BowieHP!=0 and128HP0. With544 clear it executes the empty44 defeated
script; with544 held it skips the script but reaches the same leader-present
tail. That tail appends **every still-nonzero-HP enemy** to DEAD_COMBATANTS_LIST.
ProcessKilledCombatants then increments ally defeats or enemy kills credited to
BATTLESCENE_FIRST_ALLY, clears X/Y to-1 and status, refreshes stats and parks
entities. Its cleanup does not itself set all appended enemies' HP to0.
CountRemainingCombatants excludes X<0 or HP0 and forces ally count0 when Bowie
HP0. Therefore removing128 via the action-side wrapper can sweep the living
remainder and terminate even though hidden rows were never exposed. Invitation468's
challenge wording does not replace this predicate with an all-fifteen-kills rule.

After action cleanup the loop tests ally count **first**, then enemy count; only
if neither is terminal does it apply after-turn effects, clean deaths and test
ally then enemy again. The latter includes equipment recovery and poison2 damage.
Poison death records/cleans the affected combatant and can therefore end the
battle by counts, but there is **no second defeated-wrapper call in that
post-effect suffix**. A leader dying there with other placed enemies can reach
the wrapper at a later action rather than an immediate same-suffix sweep.
Victory skips remaining after-turn effects; simultaneous terminal counts prefer
defeat. These are action/after-turn boundaries, not a promised natural kill sequence.

`battleactions/dropenemyitem.asm:battlesceneScript_DropEnemyItem` is called by
selected action-script paths, requiring ally actor, enemy target, targetDies,
a matching battle/enemy/item row **and** the item still held. The complete actual
`global/enemyitemdrops.asm` has no44 entry. Consequently even Shaman102's Healing
Rain and equipped Holy Staff produce no44 drop through this consumer. Leader sweep,
poison cleanup and victory are not alternative calls to this transaction. No
trial item award is written by the empty hooks or record congratulations.
The two map chests are a separate mechanism below; source enemy gold/EXP handling
is ordinary action accounting, not a newly declared completion prize.

## Timer lifetime, snapshot, record and save boundary

**Confirmed:** distinguish three longwords and one byte from `sf2const.asm`:
SECONDS_COUNTER at `$FFDF1E`, SECONDS_COUNTER_FRAMES at `$FFDEF9`,
SPECIAL_BATTLE_TIME at `$FFDF22` and SPECIAL_BATTLE_RECORD at `$FFF7A0`.
SAVED_SECONDS_COUNTER is another longword at `$FFF79C` used for Suspend/resume.
A fresh BattleLoop clears the seconds long only; its fresh/suspended arms do not
reset the frame byte. `code/common/tech/interrupts/vint.asm:VInt` reaches
CallContextualFunctions only when the ENABLE_VINT gate was set. That function
increments the frame byte; exactly60 resets it to0 and increments the seconds
long, **before** iterating the eight enabled contextual callback slots. The raw
FRAME_COUNTER also increments in the skipped-update path, which is not the same
seconds clock. No observed refresh frequency, exact visible duration or pause
policy follows merely from these source names.

`battlefunctions/loadBattle.asm` fades/loads/positions, clears the contextual
list, loads map/graphics/terrain, installs base VInt functions, requests map music
and FadeInFromBlack, and for CURRENT_BATTLE44 calls OpenTimerWindow. The latter
is guarded by TIMER_WINDOW_INDEX0. It creates/writes the window from current
seconds, stores slot+1, adds VInt_UpdateTimerWindow, sets SPECIAL_BATTLE_TIME=-1
and increments WINDOW_IS_PRESENT. It does not zero the seconds counter.
The callback requires a nonzero timer index and changed seconds; it copies seconds
to SPECIAL_BATTLE_TIME, rewrites digits, requests menu-selection SFX and conditionally
shows the window when HIDE_WINDOWS_TOGGLE is clear. Unchanged seconds perform none
of those updates. A displayed number, the last copied snapshot and saved best
record have distinct write sites.

CloseTimerWindow requests its destination, waits a VInt, deletes it, zeros index,
removes the callback and decrements window presence. It does not read/freeze
seconds directly, clear seconds, clear frame subcounter or erase the last snapshot.
The waited VInt can still update the snapshot before callback removal. Removing
this callback, or clearing the contextual pointer list, leaves the counter update
at the start of CallContextualFunctions in place whenever VInt updates are enabled.
Thus callback inactivity can freeze the **snapshot** while seconds keep advancing.

| Consumer context | Actual timer/clock consequence |
| --- | --- |
|Fresh44 before first control|Seconds reset precedes fresh setup; opening occurs at the end of LoadBattle, so setup's enabled contextual updates can already contribute. No reset to a displayed first-frame zero is promised.|
|Ordinary action scene|ExecuteIndividualTurn closes the timer before two VInt waits and scene construction/execution. After EndBattlescene/leader-position listener it calls LoadBattle, reopening the timer and resetting snapshot to-1; its following WaitForVInt permits another copy. The close is not an underlying-clock pause. STAY/NoAction avoids that scene close/reload path.|
|Victory|HealLivingAndImmortalAllies runs first, then44-specific CloseTimerWindow, then return seed/after/join/flags. The saved trial result is the last callback copy, potentially including the close's wait; it is not a newly sampled exact instant of killing the leader.|
|Defeat / Egress / Angel Wing|No44-specific CloseTimerWindow in these return branches. CloseBattlefieldWindows closes land/mini-status, not this timer. MainLoop then exploration reload clears contextual functions; LoadMap/SetBaseVIntFunctions resets window properties (including timer index), replacing the callbacks. No completion record is newly produced by those returns.|
|Normal Suspend|Copies seconds to SAVED_SECONDS_COUNTER before SaveGame; no local timer-close or underlying counter-stop write. Normal fade/WitchSuspend is a separate flow. Debug+Start returns to menu after saving and clearing88.|
|F88 saved resume|Restores only saved seconds at BattleLoop entry, clears88/AI memory, LoadBattle opens timer with snapshot-1, resumes saved turn. No fresh second reset, new formation or new queue at this entry. The frame subcounter is not serialized by the save range below.|

`code/common/windows/windowengine.asm:InitializeWindowProperties` explicitly
zeros TIMER_WINDOW_INDEX; `battle/battlevints.asm:SetBaseVIntFunctions` invokes it,
clears the contextual list and adds seven ordinary map/entity/view/scroll/sprite/
window/animation callbacks. `code/common/maps/mapload.asm:LoadMap` also initializes
window properties. `trap9_contextualfunctions.asm:trap9_ClearPointers` zeros its
eight pointers and enabled bitfield, not SECONDS_COUNTER. These real consumers
explain why returning to exploration removes the timer callback without equating
window deletion, callback removal and seconds stopping.

`timerwindow.asm:WriteTimerDigits` performs **DIVU.W60** on seconds, sign-extends
the quotient word and applies an unsigned comparison/clamp to999 minutes, then
writes three minute digits. It restores the packed divide result, swaps to the
remainder, sign-extends, adds100 and writes two second digits. Map24 invitation
and result init instead use **DIVS.W60**, sign-extend quotient/remainder separately
and put them into DIALOGUE_NUMBER for text. They do not apply the999-minute clamp.
`code/common/stats/newgame.asm` sets the best record to359999 (5999 minutes59
seconds in the supported signed divide range), distinct from the window's clipped
minutes. Neither formatter is a general unbounded-time conversion: unsigned/signed
16-bit quotient limits and machine divide-overflow behavior remain; no normalized
large/negative/corrupt-time promise is added. Snapshot-1 is an opening sentinel,
not a valid elapsed-time award.

SaveGame/LoadGame in `code/common/tech/sram/sramfunctions.asm` copy4016 logical
bytes from/to COMBATANT_DATA `$FFE800` through `$FFF7AF`, exclusive end `$FFF7B0`.
This contains CURRENT_MAP/BATTLE and SAVED_SECONDS_COUNTER/RECORD, but **not**
SECONDS_COUNTER, frame byte or SPECIAL_BATTLE_TIME. The copy writes every other
SRAM byte and sums a byte checksum; selector0 uses slot1, nonzero slot2, then
save writes checksum/occupied flag. Static inclusion establishes serialization
ownership of record/saved seconds, not a successful physical SRAM write, selected
natural save/load route or durability. Updating RECORD in Map24 is a RAM write;
that result branch does not call SaveGame. No newly observed persistence is claimed.

## Victory result, record comparison and retry

**Confirmed:** `battleloop_2.asm:BattleLoop_Victory` heals first and closes44's
timer. It seeds MAP_EVENT_PARAM_2 with CURRENT_MAP24, then selects the first
updated battle-party member, adds battle crop0,0 to its local X/Y, gets
that member's physical entity facing and seeds no-scroll parameter1=0. The empty
row44 after script does not replace this seed. The shared join0 tail follows;
then the loop clears444, sets544, returns the seeded map/position/facing with
D4=1. `textfunctions_1.asm:UpdateForceAndGetFirstBattlePartyMemberIndex` simply
updates the flag-derived force list and reads its first active member; it does not
filter HP or search for a valid placed actor. No fixed post-victory location or
repair of malformed supplied party state is derived from placement ordinal0.

The actual caller is MainLoop: after BattleLoop returns it runs SwitchMap then
ExplorationLoop **without another CheckBattle**. For the unchanged explicit24
seed, fresh population/init runs as above with CURRENT_BATTLE255. F544 now selects
Map24's result branch: text475 supplies elapsed minutes (`{N}{#}`),476 appends
`{DICT}` seconds and `{W2}` from SPECIAL_BATTLE_TIME. It then reloads that snapshot
and compares the full longword against RECORD with CMP/BCC. Only an **unsigned
strictly smaller** snapshot takes congratulations477 (`{N}`), writes RECORD,
requests MUSIC_ITEM and calls FadeOut_WaitForP1Input. Equal or greater takes478's
near-miss text (`{W2}`) without changing RECORD. No best-time update is performed
merely by opening the timer or entering the invitation.

Both branches then request farewell479 (`{N}`, `{W1}`), clsTxt, execute
cs_LeaveSpecialBattle (pending warp23 `(30,24)`, Left), **then clear544 before
returning from init**. The warp's actual consumption occurs after that return;
this is not clear544 before choosing the result texts or before issuing the warp.
The return/init wrapper restores registers, so the pending request is the state
that drives exploration's subsequent unwind. With444 already cleared by victory,
544 now cleared, an ordinary later23 east exit can reach the invitation again;
accepted Yes sets444 again. Fresh battle rebuilds all hidden definitions/regions
and resets seconds;494 may remain held, suppressing its empty intro/start scripts.
No record-success requirement is imposed for retry.

If544 was externally held without a valid snapshot, init still selects this
result branch and its signed formatting/unsigned comparison. A carried flag is
not proof of a naturally completed timed trial. If caller-seeded444 is also held,
the earlier MainLoop admission can bypass init for one battle call as documented
above. Changed flags, pending events or invalid timer values are not repaired by
inventing a normal outcome.

## Defeat, escape, Suspend and the village stopping boundary

**Confirmed:** defeat calls UpdateBattleUnlockedFlag first. With544 clear it
leaves444 as supplied; with544 held it clears444. It requests sad music and
text363 (`{LEADER}`, exhaustion, `{W1}`), closes text, restores BowieHP to max,
halves gold with logical right shift, obtains the egress position and returns
D4=-1. Battle4's special losable suffix does not apply to44. This branch does
not set544, update RECORD or run after/join0.

An admitted EGRESS action masks the spell index at ExecuteIndividualTurn's early
escape test, charges its actual spell cost from MP and requests text274
(`{NAME}`, `{N}{SPELL}`, level `{#}`). An admitted Angel Wing action masks item
index4, removes the selected held slot and requests text275 (`{NAME}`, `{N}{ITEM}`).
Their shared suffix closes ordinary battlefield windows, requests cast sound/
flash, closes text, calls UpdateBattleUnlockedFlag, unwinds the individual-turn
frame, obtains egress position and returns D4=0. They avoid the normal action-scene
and victory suffixes; no half-gold penalty, completion, record update or join0.
Handler presence does not bypass item/target/MP/menu legality or Sleep/Stun.

`GetEgressPositionForBattle` has no44 special destination. It passes supplied
EGRESS_MAP to `code/common/maps/egressinit.asm:GetSavepointForMap`. With399 held
(as BattleLoop sets), `data/maps/global/savepointmapcoords.asm` selects a matching
row, otherwise retains supplied map with default `(1,1)`, Up. For EGRESS_MAP23 the
actual row is **23 `(61,4)`, Up**, not trial exit `(30,24)`. Other examples remain
supplied:6 `(11,22)`, Up;8 `(22,22)`, Up. If399 was independently cleared, the
helper instead returns3 `(56,3)`, Down. F64-held may also reset raft fields through
its own table. The trial invitation/start does not set EGRESS_MAP, so a guaranteed
village retreat cannot be inferred. MainLoop subsequently enters exploration at
the returned destination without rechecking battle at that return point.

Normal battlefield Suspend is allowed for44: only CURRENT_BATTLE0 is excluded
before its prompt. `battlefunctions_2.asm:BattlefieldMenu` text0 asks for Suspend
(`{N}`); negative prompt returns menu, nonnegative saves seconds, sets88 and calls
SaveGame. Normal path requests fade-out sound, FadeOutToBlack and WitchSuspend;
debug+held Start instead clears88 and returns to menu **after** the save call.
It does not execute after44, complete544 or create an exploration-return seed.
Resumption's actual88 path was specified above. This preserves the accepted
[Battle0 exclusion](../final-confrontations-ending/battle00-bonus-battle-and-reset.md) rather than copying its
reset caller onto44. Presentation/save completion remain Unknown.

Map24's separate ordinary warp record is exact `(0,27)` →23 `(30,24)`, Left,
no-scroll. It is a map exploration warp, not a free battlefield Egress action.
The same destination is requested by decline and result farewell. `ProcessMapEvent`
clears pending before dispatch; no-scroll submits/clears warp SFX, idles main0,
unwinds exploration and supplies explicit map/coordinates/facing to MainLoop.
No23 substitution or battle row exists, so unchanged ordinary return permits
fresh Map23 setup/population/init under its supplied flags. The accepted village
[entry owner](../southern-travel-creed/battle19-outcomes-through-elven-village-entry.md) and
[local owner](../southern-travel-creed/elven-village-conversations-and-local-exploration.md) own that
conditional population/control. Stop at this return/admission; no full revisit,
service completion, Harpy Pond route or extra optional chain is asserted.

## Map content and actual chest availability

**Confirmed:** `data/maps/entries/map24/2-areas.asm` declares one area, main
`(0,0)..(17,31)`, second foreground origin `(0,32)`, background `(0,0)`, both
parallax256/256, no autoscroll, layer type0 and HEADQUARTERS music.
`3-flag-events.asm` has flag0-conditioned copy from `(32,0)`, size18x64, to `(0,0)`.
`code/common/maps/mapload.asm:LoadMapBlocksAndLayout` checks the flag before
CopyMapBlocks; the table is a supplied layout replacement, not a proof of a walked
path. Step, roof and other-item tables terminate empty. The map's sole warp was
specified above. These area/copy dimensions do not establish collision reach.

The complete `7-chest-items.asm` has two records: Right of Hope at `(6,14)` with
F215, and Life Ring at `(10,1)` with F216. The item definitions distinguish
Right of Hope111 (RARE|CONSUMABLE, SHINE use) from Life Ring124 (RING|RARE,
HP_RECOVERY5 equip effect); neither is a trial-completion reward. Item-use,
equipping and an exhaustive inventory subsystem are outside this chapter.

For admitted exploration search, `explorationfunctions_0.asm:CheckArea` derives
the faced tile using384 internal units and direction offsets, reads the working
layout's `$3C00` class and dispatches chest `$1800` **before** descriptions and
other item classes. `exploration.asm:OpenChest/GetChestItem/GetItem` matches map
row coordinates, checks the existing item flag (already held means NOTHING), sets
that flag and open block before delivery. With CURRENT_BATTLE255, GetItem applies
no battle-coordinate offset. Item delivery tries Bowie capacity4 first, then
UpdateForce/OTHER_FORCE_MEMBERS_LIST with TARGETS_LIST_LENGTH-2/DBF. If no recipient
has room, it requests full-hands text and CloseChest/RefillNonChestItem, restoring
availability rather than silently granting or losing a successful award.
No selected non-chest row or description callback adds another Map24 reward.

Battle search uses `battlefunctions_2.asm:ProcessBattleEntityControlPlayerInput`:
CheckChestItem decides Search versus Stay from the faced coordinate/layout,
sub_256E6 can detect another actor distributing items, then OpenChest precedes
nonempty/trap/gold/item branches. A successful ordinary item goes only to the
searching actor; full hands closes the chest and returns to menu. Successful or
empty search ends with STAY. The trap consumer requires a matching enemy starting
coordinate, exact `$0200` activation, absent15/15 triggers and maxHP0; none of44's
rows matches either chest coordinate and every dormant row has a real primary
region. No chest-trap enemy is introduced here.

Crucially, the actual USA `exploration.asm:GetItem` multiplies CURRENT_BATTLE by
**BATTLEMAPCOORDINATES_ENTRY_SIZE5**, although the coordinate table's full records
are7 bytes (`sf2enums.asm` declares both constants). For44, the byte reads at
44*5+1/+2 are table offsets221/222: row31 fields4/5, **32 and255**, from row31
`(map2,0,0,48,32,255,255)`. It adds those bytes to candidate X/Y for table matching,
not44's true origin0,0. A crop-valid X0..17 therefore probes X32..49 and cannot
match chest X6 or10. The supplied Y is also decremented modulo256. The later
layout offset uses the original candidate; that does not repair the failed match.
Thus the unchanged normal battle-domain chest lookup at either declared location
fails before awarding those contents. Do not silently substitute stride7 because
it is the intended crop. Exploration can match the records with no battle offset,
but invitation/result init normally posts another warp before free exploration;
physical access, altered caller states and a naturally successful collection are
Unknown. Table presence is not accepted custody or battlefield availability.

## Actual research and remake consumer boundaries

**Confirmed by accepted-base code inspection:** this slice performs no44
conversion, package preparation, lowering, runtime test or playable acceptance.
These are actual distinct consumers, not a claim that a type's existence admits44.

| Surface | Actual support and limit at the accepted base |
| --- | --- |
|H2 structural owners|`src/sf2tool/h2/battle_spriteset_data.py` enumerates all45 source-form slots0..44 and validates count/macro/include shape. Battle global/routing/terrain/cutscene/loop/functions and map setup/init/event owners retain tables, operands and shared instruction rules. Their structural coverage includes44; it is not a natural trial run, timer/persistence observation or an encounter-specific playable binding.|
|Python route/conversion|`remake_exploration_programs.py:OriginalPrograms.compile` recognizes CheckRandomBattle and emits source-random-battle plus onRequest; unsupported instructions fall through native-call. Map24's direct DIVS/longword snapshot/record operations have no complete trial program lowering here. `remake_exploration_battles.py:compile_routes` indexes selected coordinate/hook rows and wildcard admission separately from `prepare_encounter`, which explicitly accepts only2/3. Its generic source-battle-load presentation does not implement44's timer window merely by naming LoadBattle.|
|Content encounter admission|`PrivateBattleEncounterReader.Pin` trusts only1/2/3; its general parser can carry raw spawn expressions but requires each region's vertexCount4, while44 has triangle8. `PrivateBattleScenarioReader.Assemble` independently accepts1/2/3, requires STARTING, and lacks HEALER5 commandset4. Its enemy AI-expression and movement admission do not supply the wider BIT4/MAGE/loadout cases. MOVE_TO point and FOLLOW_ENEMY128 expressions do not admit the trial.|
|Content story admission|`ExplorationContentReader.cs`'s **source-random-battle** opcode explicitly bounds IDs0..43;44 is outside that opcode's range. That bound is not a universal original/H2 table limit, nor proof that44 should be routed through random battles. ReadBattleRoute separately requires a supplied admitted encounter; native-call becomes UnsupportedInstruction. Best-record number formatting, trial clock/snapshot, result music/wait and retry still need real supported instructions/bindings.|
|Domain|`BattleInitializationRules.cs` requires controlled roster-only/already-refreshed starts, difficulty0, no auto/opponent control and narrow source loadouts0/127. `BattleActivationRules.cs` rejects nonzero SpawnMode and non-four-vertex regions; its polygon predicate is not the original integer/CCR algorithm above. `Gameplay/Sf2/SourceEnemyAi.cs` has activated commandsets6/7/9/14 and rejects other items/learned spells. These do not admit44's hidden/respawn population, Shaman/Monk/healing/spell/equipment or complete leader/drop behavior.|
|Application and outcome|`Runtime/Battles/BattleAdvancer.cs` advances admitted rules and publishes observations such as spawn-modes-admitted; its actual rules reject the modes above, so that observation name is not hidden-spawn implementation. Story/outcome routes require actual supplied encounters and programs. The selected load/outcome continuation has no complete44 timer/record/return binding. Source leader-present44 and script0 join are facts to consume, not executable proof from generic terminal types.|

The [story frontier](../../story-content-coverage.md#four-different-frontiers) keeps
original static research, original runtime, English design and engine playability
separate. This closes the named **connected static44 gap** only: no natural
chronology/state/party/flags/seed, collision routes, all tactical decisions,
completed wait/audio/rendering, hardware seconds/pause behavior or durable save
is established. Unknowns do not become an automatic emulator/acquisition queue.
Optional villages/recruitment/revisits and the wider epic remain open.

## Reproduction and acceptance boundary

Read the complete selected `spriteset44.asm`, all Map24 setup/event records and
Map23 east warp; join the actual include tables and consumer functions named in
each section. Existing [global battle](../../battle/battle-global-data.md),
[spriteset](../../battle/battle-spriteset-data.md), [routing](../../battle/battle-routing-data.md),
[loop](../../battle/battle-loop.md), [cutscene](../../battle/battle-cutscenes.md),
[functions](../../battle/battle-functions.md), [search](../../world/field-search-control.md) and
[request-consumption](../../world/map-event-request-consumption.md) owners retain their
accepted fixtures/commands. This unregistered prose adds no fixture count or
new gate. Manual semantic review is distinct from mechanical link/encoding checks.

The local Issue handoff retains direct checks/recipe/dependencies, the committed
planner and exact-head CI, plus preserved failure records. On the clean committed
candidate, load the owning ignored private-input configuration in the same process
before `uv run sf2 verify plan --base origin/main --head HEAD`; interpret its
selection under [documentation verification scope](../../../../remake/docs/development-and-verification.md#scope).
No normal/full/SDK/engine/adapter/Godot/H3/H4, old comparison, emulator, screenshot,
new acquisition, simulation or retained-pair rebuild is required or reported.
Independent review/integration remains main-gate's acceptance boundary.
