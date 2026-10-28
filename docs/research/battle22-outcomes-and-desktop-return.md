# Battle22 outcomes and Desktop return

Evidence date: 2026-10-14. **Confirmed:** Battle22's defeated and after scripts
are both `ms_Empty`, but leader cleanup, the common Bowie join and victory flag
writes still execute. The returning exploration load selects the F522 population
and layout; Map22 init independently gates the twelve-command `cs_5996E` with
F778. It announces the Cotton Balloon without acquiring or using it.
**Inferred:** the scene presents Ponpei's reward for winning his chess challenge.
**Unknown:** natural outcome, supplied state, service completion, actual
presentation/control and persistence.

## Agreement and provenance

Standard rigor under [Issue #912](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/912)
and [content epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514):
resolve the complete outcome-to-post-victory Desktop exploration boundary for
the next English content guide. The uncertainty is in selected static
source/consumer joins. Accepted base is
`f8cd67cd3818a1790b76354922015c13d8af083a`; no stacked dependency. This is the
sole owned document, with no registry/index/schema/fixture/contract, tooling,
remake or design changes. Accepted design work is disjoint from this scope.

Consume accepted [Battle22 initialization/roster](./battle22-initialization-and-roster.md),
[Desktop first arrival](./desktop-kingdom-first-arrival.md),
[Desktop interactions/admission](./desktop-kingdom-interactions-through-battle22-entry.md),
[battle loop](./battle-loop.md), [cutscene routing](./battle-cutscenes.md),
[shared outcome consumers](./battle21-outcomes-and-devils-tail-return.md) and
[map request consumption](./map-event-request-consumption.md). Battle21's
particular after scene, warp and random-battle facts are not Battle22 behavior.
Stop after conditional local exploration/control or pending return, plus the
changed King/Maid callbacks. Chest search, Cotton Balloon inventory/use, guard
clearance and Floor World departure are the next boundary, not part of this
slice. No general combat/AI/damage/terrain/renderer audit or runtime acquisition.

Source paths are relative to `disasm/` in
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only source checkout is clean at that pin. Retained evidence binds
USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
This is source/retained-evidence review, not a new extraction or playthrough.
Short battle paths start at `code/gameflow/battle/`; setup filenames at
`data/maps/entries/map22/mapsetups/`; map-script consumers at
`code/common/scripting/map/`. Fnnn means flag nnn; slots/command indices are
zero-based. Coordinates, speeds and waits are operands, not measured results.

Reuse the unchanged 18,285,979-byte map-init/map-script-engine pair and small
selected sources/fixtures. Linear comparisons are budgeted below 512 MiB and
one minute, plus small prose/receipts; the pair/cutscene comparison passed in
0.084 seconds. No environment, bulk payload or capture is created. Reassess
before material scope/resource expansion.

## Outcome selection and actual wrapper returns

**Confirmed:** complete selected tables and immediate consumers establish:

| Source | Battle22 selection and effect |
| --- | --- |
| `data/battles/cutscenes/battleendcutscenes.asm`, slot 22 | `ms_Empty`. ExecuteBattleCutscene_Defeated first requires Bowie HP nonzero and enemy 128 HP zero. F522 clear dispatches the terminator; set bypasses it. Both eligible paths reach `loc_47C48`, the leader tail. |
| `data/battles/global/enemyleaderpresence.asm`, slot 22 | -1. The tail scans enemy 128–159, appending every nonzero-HP slot to DEAD_COMBATANTS_LIST and maintaining its length/terminator. It does not test X or clear HP. |
| `data/battles/cutscenes/afterbattlecutscenes.asm`, slot 22 | `ms_Empty`. ExecuteAfterBattleCutscene dispatches only with F522 clear; both dispatch return and F522-set skip reach EndAfterBattleCutscene. |
| `data/battles/cutscenes/afterbattlejoins.asm`, slot 22 | 0: EndAfterBattleCutscene calls j_JoinForce for Bowie, restores registers and returns. This is not an Eric/Karna or other new named recruit. |
| Complete `data/battles/cutscenes/regionactivatedcutscenes.asm` | Only battles 32/40 occur. ExecuteBattleRegionCutscene finds no 22 row and returns. |
| Complete `data/battles/global/afterbattlepositions.asm` | Only Battle05 occurs. ApplyPositionsAfterEnemyLeaderDies performs no Battle22 staging override. |

Wrappers are `cutscenes/battleendcutscenesstart.asm`,
`battleendcutscenesend.asm`, `afterbattlecutscenesstart.asm` and
`afterbattlecutscenesend.asm`. `layout/sf2-07-0x044000-0x064000.asm` includes
the **cutscenes** join table; the similarly named global table is not the
selected owner. Its unused-feature comment does not cancel the executed join.
`code/common/scripting/map/ms_empty.asm` contains only the script terminator.
ExecuteMapScript still takes its normal termination tail: conditional dialogue
scroll wait, clear VIEW_SCROLLING_SPEED, restore registers, return. Empty does
not mean the call has no service dependency or completion effects.

`battleloop_1.asm` calls the defeated hook after the individual turn, then
ProcessKilledCombatants and CountRemainingCombatants. The former's complete
body in `battleloop/processkilledcombatants.asm` runs death presentation and
bookkeeping, writes listed X/Y=-1, clears status, updates stats and moves mapped
actors off-map. Thus leader cleanup removes still-living enemies from counting
without first reducing their HP to zero. In the fresh accepted roster, enemy 128
is King; Ponpei/Maid's neutral state slots 159/158 have HP 0 and are not appended
by this nonzero-HP test. Supplied state can differ from that fresh roster.

`battleloop/countremainingcombatants.asm` counts 30 allies and 32 enemies with
nonnegative X and nonzero HP; Bowie HP 0 forces the ally count to zero. Both
outcome checks test zero allies before zero enemies: simultaneous loss means
defeat. Bowie dead also bypasses leader cleanup. With Bowie surviving and
coherent state, King death through the hook can end the encounter without
individually killing every chess enemy. Zero counted enemies is also sufficient
at the normal count boundary; the roster is not an all-enemies-only objective.

If both counts remain nonzero, after-turn effects run, followed by another
killed pass and the same ordered counts. That second check has no defeated
hook. Leader HP becoming zero during those effects does not by itself execute
the leader tail at that check. The debug kill-all branch is a separate caller,
not evidence of natural victory. All earlier services must return to reach
the next count/write; no rollback is supplied for an unfinished prefix.

The [first-control account](./battle22-initialization-and-first-control.md) owns
SWARM, live/point orders, the two neutral actors and conditional turn dispatch.

## Victory seed, completion flags and exploration entry

**Confirmed:** `battleloop_2.asm:BattleLoop_Victory` performs this order for both
first and repeat victory:

1. Heal living allies and immortal Peter 7/Lemon 28 via
   `battleloop/heallivingandimmortalallies.asm`: HP/MP maxima, retain only
   existing STUN/POISON/CURSE bits only as an intermediate mask, then update stats.
   `code/common/stats/updatecombatantstats.asm:UpdateCombatantStats` removes incoming
   CURSE and derives final ally CURSE from equipped cursed items; stale CURSE
   with uncursed gear clears, while cursed gear can add it. STUN/POISON remain.
   Other dead allies stay dead;
   membership is not a healing prerequisite. Battle44's timer special is absent.
2. Seed PARAM2=CURRENT_MAP; PARAM3/4=first active member's combatant X/Y plus
   BATTLE_AREA_X/Y with byte arithmetic; PARAM5=that member's mapped battle-actor
   facing; PARAM1=0. `textfunctions_1.asm:UpdateForceAndGetFirstBattlePartyMemberIndex`
   under `code/common/scripting/text/` calls UpdateForce before taking the first
   list byte. Joined/active IDs are rebuilt in ascending order; this is not a
   first-living search and has no empty-list repair.
3. Call ExecuteAfterBattleCutscene. First F522-clear victory executes ms_Empty;
   F522-set repeat skips it. Both run JoinForce(0) through the shared tail.
4. Clear F422, set F522, then read PARAM2–5 into d0–d3 and return d4=1.

`code/common/stats/battleparty.asm:JoinForce` sets Bowie's joined flag,
rebuilds the force lists, and sets his active flag if the active count is below
FORCE_MAX_SIZE=12. It does not unconditionally make room in a full party, and
JoinBattleParty does not immediately rebuild the list/count again. This later
join cannot repair which member already supplied the return seed. Under normal
joined/active Bowie state it is not a newly recruited character.

`data/battles/global/battlemapcoords.asm`, slot 22, supplies Map22, area
`(9,0)`, size 30x30. Battle actors use the cropped layout, so a current member
at local `(x,y)` seeds full-map `(x+9,y)` modulo the byte arithmetic. Neither
initial roster placements nor the prior `(0,0,Right)` admission warp is the
current return position. No selected Battle22 after command overrides the
seed. Changed membership, HP/position/lookup or saved area state changes the
result; arbitrary inconsistent state is not normalized.

The victory path reads these parameter bytes directly; it does not wait for
ProcessMapEventType1_Warp to consume a new csc07 event. It does not write a
new MAP_EVENT_TYPE here. A replacement parameter writer before the read would
change the returned tuple. `code/gameflow/mainloop.asm` resumes at
`alt_MainLoopEntry`: SwitchMap then ExplorationLoop, **bypassing CheckBattle**
for this return. The complete flag-switched-map table has no Map22 row.

F422 admission, F472 intro completion, F522 battle completion and F88 suspension
remain independent. Victory clears 422/sets 522 without writing 472 or 88. The
initialization owner proves F88-set entry uses supplied current/saved state,
bypassing fresh assignment/roster/hooks; the current battle must still actually
be 22 for the selected outcome claims here. F472 is not an outcome/init gate.
F776/F778 are separate Map22 scene gates, outside the temporary flag range.

For the normal explicit Map22 tuple, ExplorationLoop in
`code/gameflow/exploration/explorationfunctions_2.asm` clears MAP_EVENT_TYPE,
adjusts STEP_COUNTER, heals eligible allies again, starts fade-out and disables
view following. It sets CURRENT_MAP=22 and CURRENT_BATTLE=255; loads tilesets,
waits fading, clears VInts, selects/rebuilds entities and loads their mapsprites.
It then clears temporary flags 256–383, sets F80, initializes sprite frames,
targets physical 0, calls LoadMap, installs base VInts and runs the map init.
Old battle actors/lookups/actions are gone; the full-map loader no longer crops
the battle rectangle. A supplied map255 instead takes the existing-population
branch, with no fresh entity selection/temp reset/CURRENT_BATTLE assignment;
that is not this normal return.

## F522 population and layout

**Confirmed:** `data/maps/mapsetups.asm` selects default ms_map22 or the F522
alternate ms_map22_flag522. The two pointer tables share entity events, zones,
descriptions, Section5 and init, differing only in entity-list pointer.
GetCurrentMapSetup is rerun by each accessor; F776/F778 do not select population.

`s1_entities_522.asm` (`0x59402..0x59474`) has these fourteen complete rows,
then msEntitiesEnd. P assumes only F67/Peter and F68/B(Oddler) followers,
no skipped ally row and no raft overwrite. Init=eas_Init; Fixed=eas_InitFixedSprite.

| Row | Logical identity | P | Sprite / action | Position, facing |
| --- | --- | --- | --- | --- |
| 0 | Eric14 | 3 | Ally sprite; walking origin (15,14), range 1 | `(15,14)`, Down |
| 1 | Karna15 | 4 | Ally sprite; Init | `(5,15)`, Down |
| 2 | NPC128 | 5 | CARAVAN62; Init | `(3,6)`, Down |
| 3 | NPC129 | 6 | YOUNG_MAN196; Init | `(29,21)`, Right |
| 4 | NPC130 | 7 | YOUNG_WOMAN197; Init | `(30,21)`, Left |
| 5 | NPC131 | 8 | OLD_MAN198; walking origin (24,22), range 1 | `(24,22)`, Down |
| 6 | NPC132 | 9 | PRIEST192; Init | `(5,6)`, Down |
| 7 | NPC133 | 10 | MAID208; Init | `(14,3)`, Down |
| 8 | NPC134 | 11 | KING_PONPEI188; Init | `(15,2)`, Down |
| 9 | NPC135 | 12 | GUARD206; Init | `(8,7)`, Left |
| 10 | NPC136 | 13 | GUARD206; Init | `(33,24)`, Left |
| 11 | NPC137 | 14 | OBJECT1=173; Fixed | `(63,63)`, Up |
| 12 | NPC138 | 15 | OBJECT1=173; Fixed | `(63,63)`, Up |
| 13 | NPC139 | 16 | OBJECT1=173; Fixed | `(63,63)`, Up |

`mapfunctions.asm:InitializeMapEntities` clears entity records/lookups, declares
followers from physical 1, consumes these rows in order, then declares Bowie at
physical 0 with the returned seed/facing and eas_Idle. Fixed/walking macro rows
encode four bytes plus an action pointer or walking descriptor. Coordinates
are masked by `$3F` and multiplied by 384. Ally sprite operands below 30 use
ally lookup slots; an existing nonzero slot skips that whole row without
consuming a physical slot. No joined/active test admits Eric/Karna. The current
follower table contains neither, so normal fresh selection does not supply
that skip case. Walking declarations install the existing walking action;
the listed position is not a prediction of later random motion.

Ordinary sprite rows append lookups from offset 32, independently of ally rows.
GetEntityAddressFromCharacter maps logical 128+ by subtracting 96. Thus NPC128
is Caravan and NPC134 is Ponpei, not combatant128 King or the battle B31/A30
neutral lookups. The sixteen chess rows and three old ORB153–155 rows are not
retained. Shared event entries 137–152 still exist: placeholders 137–139 now
have those logical IDs, so do not infer that the old event table was removed
or that an arbitrarily invoked placeholder must take the default callback.
Natural interaction with off-map placeholders is not established.

Map22 is not an overworld map. `followersfunctions_1.asm`,
`followersfunctions_2.asm`, `data/scripting/entity/followers.asm` and the
[first-arrival allocation owner](./desktop-kingdom-first-arrival.md#fresh-population-and-setup-selection)
retain the complete ordered follower selection. F65 does not add Caravan/A
here. With only F67/F68, physical 1 is Peter/eas_Follower1 and physical 2 is
B/eas_Follower2, initially at the return seed. Other flags change allocation;
later duplicate B/A/D writes can replace lookups while old physical actors
remain. F522 does not itself create Peter or Oddler, and battle membership is
not follower admission. There is no fixed Peter fallback in this population.

ClearEntities zeros lookups; absent Peter/B can therefore resolve to physical 0,
not a safe no-op. Current HP/class selects ally sprites, including BLUE_FLAME
for zero HP now that CURRENT_BATTLE=255. The allocator's mode/raft tail remains:
F64 plus RAFT_MAP=22 declares physical 31 at saved masked coordinates; mode 2
instead changes main to RAFT, parks followers and declares 31 at `(64,64)`.
The ordinary two-follower population ends at 16, so the extra raft does not
overwrite those rows. Additional follower allocations can occupy 31 and be
overwritten, retaining aliases. No outcome or selected scene boards the raft.

The full-map load's `code/common/maps/mapload.asm:LoadMapBlocksAndLayout`
reads `data/maps/entries/map22/3-flag-events.asm` in order:

| Flag | Source | Size | Destination |
| --- | --- | --- | --- |
| F522 | `(39,0)` | 1x1 | `(25,10)` |
| F522 | `(40,0)` | 1x1 | `(25,40)` |

The first source comment identifies a chest replacing a chessboard tile; the
second identifies blank replacing blank. CopyMapBlocks in
`code/gameflow/exploration/exploration.asm` copies words on the 64-wide working
layout. The full-load pass applies these flag copies before chest overlays;
a previously set chest flag can then apply its opened-chest word `$D802`.
`data/maps/entries/map22/7-chest-items.asm` names `(25,10)`, F212,
COTTON_BALLOON. This is the next search boundary, not proof of award, custody or usability. F522 does not set 212.
No layout decoding, collision route or rendered chest is established here.

## Init gates and complete post-victory scene

**Confirmed:** the entire `s6_initfunction.asm` first checks F776. If clear,
it runs the complete cs_59656 and writes 776 only after return. Independently,
it then checks F522; clear returns. Set checks F778; set returns, clear runs
cs_5996E and writes 778 only after return. Both pointer tables use this body.

| Supplied F776 / F522 / F778 | Ordered init behavior |
| --- | --- |
| set / clear / either | No arrival or victory scene. |
| clear / clear / either | Arrival scene, then 776; no victory scene. |
| set / set / clear | Victory scene, then 778. |
| set / set / set | Neither scene; alternate population/layout still selected. |
| clear / set / clear | Arrival scene first, then 776; victory scene, then 778, only if each returns. |
| clear / set / set | Arrival scene first, then 776; victory scene skipped. |

F522-set/F776-clear does **not** rebuild the baseline population before the
arrival script. That script references old ORB153–155, absent from this
alternate list; their zero lookups can alias main. The accepted arrival
command/action analysis still applies, but its baseline actor example cannot
be reused to promise normal staging or completion. An unfinished arrival
prevents the later F776 write and subsequent post-victory dispatch. Setting a
gate externally proves neither a previous played scene nor persistence.

`scripts.asm:12–23`, cs_5996E at `0x5996E`, is exactly this program:

| Index | Command | Immediate result |
| --- | --- | --- |
| 0 | textCursor 1794 | csc04 writes CUTSCENE_DIALOG_INDEX=1794. |
| 1 | setCameraEntity CAMERA_NEUTRAL | csc24 consumes `$FFFF` directly and writes VIEW_TARGET_ENTITY=-1; no actor lookup. |
| 2 | reloadMap 10,0 | csc46 reuses the current working map/layout and establishes camera/display state. |
| 3 | setPos Bowie,25,12,Up | csc19 writes admitted actor current/destination/facing. |
| 4 | setPos Peter,24,13,Up | Same, through the actual Peter lookup. |
| 5 | setPos B,25,13,Up | Same, through follower B31. |
| 6 | fadeInB | csc39 calls FadeInFromBlack and waits through its service. |
| 7 | nextSingleText $0,134 | csc00 displays 1794, increments cursor, closes windows, sleeps 10. |
| 8 | setCamDest 20,7 | csc32 disables following, requests camera destination and waits for scrolling. |
| 9 | nextText $0,134 | csc02 displays 1795 and increments cursor, leaving continuation open. |
| 10 | nextSingleText $0,134 | csc00 displays 1796, increments cursor, closes windows, sleeps 10. |
| 11 | csc_end | Terminator tail, conditional scroll wait, clear scrolling speed, register restoration and return. |

`sf2cutscenemacros.asm` encodes these commands; immediate handlers live in
mapscriptengine_1/2.asm. There is no warp, item grant, flag command, choice,
new population load, movement sequence or follower-action replacement in this
program. F778 is a caller write after command 11 returns, not an early scene
receipt. A later service failure leaves earlier effects intact.

### Camera, actor and text services

csc46 deactivates VInts, sets d1=-1, packs `(10<<8|0)*3=$1E00` into d0 and
calls LoadMap. Its negative-d1 arm reuses CURRENT_MAP 22's existing working
blocks/layout; it does not recopy flag changes, reload entities or invoke init.
It then enables display/interrupts, reactivates VInts and waits a VInt. The
preceding fresh exploration load already selected the full F522 layout.
LoadBattle's nonnegative-d1 full load and cropped battle domain are different
consumers; this reload does not crop again or restore the chessboard roster.

LoadMap's explicit camera interpretation splits the packed bytes and shifts
by 7, supplying `(3840,0)` before area/parallax handling. setCamDest 20,7 calls
`battlefunctions/battlefunctions_0.asm:SetCameraDestination`, multiplying both
operands by 384 before SetViewDestination. These are camera-border requests,
not player coordinates. `code/common/maps/camerafunctions.asm:WaitForViewScrollEnd`
polls with VInt waits and rechecks, without a timeout. FadeInFromBlack in
`code/common/tech/interrupts/fadingcommands.asm` sets the fade mode and waits
for the VInt-driven setting to clear. No source wait proves elapsed time or
visible/heard completion.

csc19's AdjustScriptPointerByCharacterAliveStatus skips the four operand bytes
and the operation for HP 0 true allies 0–29. B31 bypasses that HP test. An absent
but living Peter can write physical 0; absent B can then overwrite main at
`(25,13)`. Different B aliases affect a different actor. Ordinary victory heals
Peter and has living Bowie, but does not supply missing follower flags.
The helper writes X/Y and destinations as tile×384, facing and sprite update;
it does not replace action scripts, freeze actors or wait for idle. With
coherent mappings the requests are full-map Bowie `(25,12)`, Peter `(24,13)`
and B `(25,13)`, all Up, with no added battle-origin 9. Follower actions can subsequently move them;
these are not immutable final positions or a new return-parameter tuple.

The literal text join is `data/scripting/text/gamescript.txt`:

| ID / key | Command speaker | Text and controls |
| --- | --- | --- |
| 1794 /0702 | $0,NPC134 | `Checkmate!{N}This is for you.{W1}` |
| 1795 /0703 | $0,NPC134 | `A Cotton Balloon is inside.{W2}` |
| 1796 /0704 | $0,NPC134 | `With it you can leave{N}Desktop Kingdom safely.{N}Good luck!{W1}` |

GetEntityPortaitAndSpeechSfx under `code/common/scripting/entity/` resolves the
actual mapped actor's mapsprite through `data/spritedialogproperties.asm`.
Normal NPC134 is KING_PONPEI188: portrait NONE 255 sign-extends to -1, speech
bleep 6. csc1D opens no new portrait for -1; modifier $0 sets neither right-side
nor mirror bit, and an already open portrait bypasses opening. This is not
KING_GRANSEAL or the chess KING. Aliased/supplied sprites change the result.
Both text handlers wait for scrolling before DisplayText; only csc00 performs
the final window closure/sleep. ExecuteMapScript's debug P2-Start skip can skip
these text handlers without skipping camera, placement, fade or terminator.
F778 therefore is not proof the player saw every line.

After script return, init sets 778 and returns through RunMapSetupInitFunction.
Exploration then conditionally submits map music/fade based on palette
comparison and reaches WaitForEvent. Pending type 0 targets physical 0 and
installs the current-mode controlled action; a new pending event takes priority
and can bypass controller installation. BattleLoop set PLAYER_TYPE=0 and this
path does not change it, so the coherent path uses eas_ControlledCharacter.
Supplied mode changes still select the existing alternate control scripts.
The scene itself produces no pending warp. Completed init alone does not
prove that later input/scroll/services returned or that usable control occurred.

## Changed King and Maid conversations

**Confirmed:** `s2_entityevents.asm` retains the same shared event table. The
[interaction owner](./desktop-kingdom-interactions-through-battle22-entry.md#actual-activation-event-selection-and-return)
provides actual player activation, physical-to-logical event mapping and
service returns. Post-victory population changes must be applied before using
its old physical examples. Normal Maid133/Ponpei134 remain physical 10/11 in
the two-follower example. Their changed arms are complete here:

| Callback and supplied state | Ordered body and return |
| --- | --- |
| Map22_EntityEvent8, F522 set | Text 1797/0705: Ponpei praises the party and offers an army if they stay; `{W1}`, rts. No F777 or force-join write. |
| Event8, F522 clear | Existing F777 permission branch, as owned by the predecessor. F778 does not gate it. |
| Map22_EntityEvent7, F522 set/F256 clear | Text 1798/0706 then 1799/0707: the King is kidding, but the Cotton Balloon is special treasure; `{W2}` then `{W1}`. Set 256 only after both text calls return; rts. |
| Event7, F522 set/F256 set | Text 1800/0708 then 1801/0709: adventurers northeast in Floor World, with a request to visit; `{W2}` then `{W1}`; rts. No further flag write. |
| Event7, F522 clear | Text 1777, `{W1}`, rts; no F256 check/write. |

F256 resets on each explicit fresh exploration load, even of the same map.
The scene's reloadMap does not reset it. Repeated Maid interaction within that
load advances to the second arm; reentry can restore the first arm even with
F778 set. An interrupted first conversation before SetFlag leaves 256 clear;
interruption after it does not roll it back. F522/776/777/778 lie outside the
reset range, and none proves an observed conversation.

The entity-event row's UP 1 for Maid turns her toward the player without the
restoration bit; DOWN 3 for Ponpei turns then restores saved facing on return.
They are action bitfields, not approach constraints. The wrapper clears the
entity-update VInt flag before the callback, conditionally restores facing,
closes portrait/text, reactivates entity updates and returns afterward.
MAID208 selects no portrait/bleep 3; Ponpei selects no portrait/bleep 6. Text and
wrapper services must return; service calls alone are not usable-control proof.
No army recruit, item acquisition or new map request occurs in these arms.

Other residents, Caravan/Church and guard callbacks remain linked to the
[existing interaction owner](./desktop-kingdom-interactions-through-battle22-entry.md).
F522 suppresses Map22_ZoneEvent0's chess-start arm independently of F777.
It does not clear the exit guard: Guard136's separate F212/F258 branch and
cs_599A4 belong to the next boundary. The chest row/announcement does not
satisfy its predicate, remove an inventory item or depart for Floor World.

## Nonvictory, repeat and interruption boundaries

**Confirmed:** defeat calls UpdateBattleUnlockedFlag, delivers exhaustion
music/text, restores Bowie's HP to maximum, halves unsigned gold and selects
Egress coordinates with d4=-1. Battle04's special losable branch does not
apply. Admitted Egress/Angel Wing in `battlefunctions/battlefunctions_1.asm`
deducts spell MP/removes the selected item, performs text/flash/closure,
updates unlock, unwinds the action frame and returns Egress coordinates with
d4=0. Admission is not proof of menu availability, MP or item custody.
Neither path runs the victory after/join or sets 522/472. UpdateBattleUnlockedFlag
preserves 422 when 522 is clear and clears 422 when 522 is set.

`battleloop/getegresspositionforbattle.asm:GetEgressPositionForBattle` has no
Battle22 special: use supplied EGRESS_MAP.
The accepted Map22 admission zone writes EGRESS_MAP=22 before the battle-start
script. With that value retained and F399 set by BattleLoop, GetSavepointForMap
in `code/common/maps/egressinit.asm` selects the Desktop savepoint `(5,8)`, Up,
from `data/maps/global/savepointmapcoords.asm`.
A changed EGRESS_MAP selects its own row or retains that map with default (1,1), Up;
a direct F399-clear call instead takes initial Map3/56,3,Down. The existing
`data/maps/global/raftresetmapcoords.asm` F64 table remains operative,
independently of boarding: Map22 uses
its default reset Map69/42,15 when F64 is set. F64 clear preserves saved raft
coordinates. Do not import a victory seed or post-victory actor placement into
this loss/escape result.

MainLoop's alternate entry bypasses CheckBattle for loss/escape too. Thus
F422 still set after an incomplete first encounter does not immediately
readmit Battle22 on this return. A later normal MainLoop admission can use it.
An explicit Map22 return loads the actual F522 population and init gates:
nonvictory with 522 clear has no post-victory scene; independently supplied 522
set/778 clear can still run it. Outcome sign is not the init predicate.

| Changed premise | Discriminating effect |
| --- | --- |
| First victory with 776 set/778 clear | Empty battle after script; common join/422 clear/522 set; alternate load; twelve-command scene;778 after return. |
| Repeat victory with 522/778 set | Common victory seed/join/flag writes still run; alternate load; scene skipped. No automatic (25,12) staging; the member-derived seed matters. |
| Repeat or nonvictory with 522 set/778 clear | Post-victory scene can run on Map22 init despite the supplied outcome/history. |
| 472 clear or externally set | No change to selected outcome or map-init gates. No previous intro is proved. |
| 776 clear with 522 set | Arrival runs against alternate population first; absent old ORBs and aliases prevent a baseline staging guarantee. |
| Missing/dead/aliased scene actor | HP skipping and lookup alias effects above; no fallback population, revival or guaranteed final position. |
| Incomplete killed/after/join/return service | Only completed prefix effects hold; later flags, load and control cannot be claimed. |
| Incomplete cs_5996E | Earlier placement/camera/text effects can hold with 778 still clear; next admitted init can attempt it again. |
| New pending event before WaitForEvent | Event processing takes precedence over installing controlled action. |

Suspension is separate: BattlefieldMenu in `battlefunctions/battlefunctions_2.asm`
can save seconds/set 88/call SaveGame and jump to WitchSuspend; debug P1-Start
can instead clear 88 and return to the menu. It is not a victory/522 write.
F88-set BattleLoop continuation reloads supplied current state and goes to
individual turns without fresh roster/intro/healing. The accepted initialization
owner retains the exact fresh/resumed distinction and actual loader effects.
None of these RAM gates establishes cross-process save persistence.

## Reproduction and retained limits

Load `. ./local/private-inputs.ps1` in the same process as controlled commands;
verify the source pin and clean status. The existing
[retained comparison recipe](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary)
compares map-init equality, `_verify_map_script_engine_fixture` and source
`_cutscene_facts`. Also compare `_build_lifecycle_facts` from
`sf2tool.h2.battle_loop` to `battle-loop-static-v1.json` expected.lifecycleFacts,
and `_build_control_facts` from `sf2tool.h2.battle_control` to
`battle-control-static-v1.json` expected.controlFacts, using the pinned disasm
root. All named fixtures live under `tests/fixtures/h2/`; these comparisons
passed without regenerating the retained pair.

Direct reproduction must join complete selected source, not just aggregate
fixture equality:

- Enumerate the full defeated/after/join/leader/region/position tables and
  select 22. Read complete wrappers, ms_Empty, killed/count/victory/escape/seed
  consumers and their actual returns. Resolve j_ aliases through s02/s05/s07
  under `code/common/tech/jumpinterfaces/`.
- Select `programCorpus.programs` entryLabel cs_5996E from retained
  `local/derived/map-script-engine-static.json`: twelve commands, source lines
  12–23, operands and terminator. Match each to scripts.asm and the macros/
  handlers above; read the complete init, fourteen population rows, both flag
  copies and changed callbacks. Read literal 0702–0709 text keys.
- Trace MainLoop/ExplorationLoop, full-load/reload arms, setup/entity/follower,
  actor guards, camera/text/fade and pending-control consumers. Existing
  routing aggregates with routeTargetsParsed=false cannot prove selected
  slot targets, and separate helper equality cannot prove the composed scene.

Use whole-document/source/provenance/link/private-boundary checks and
`git diff --check`; run configured `uv run sf2 verify plan --base origin/main --head HEAD`
on the clean committed head and record actual final-head CI in Issue912's
handoff. No local blanket normal/full/engine/Godot/H3/H4 suites solely for
prose, new parser/fixture or automatic runtime queue.

Preserve [#908 original handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/908#issuecomment-6174872245),
[pure-rebase handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/908#issuecomment-6174994479)
and [PR910 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/910#issuecomment-6175006823).
Original head `0a7048f0fbfa38a69d6470a73e40445cebc55acd`, tree
`eb68c08e958a868dd163f9e128edffffa5147f14`, base
`ccf4a57360fea5b62160fe959a5d2dbca6dd8736` and rebased
head `9b054f217a07b1ab8926dcf90f30ac5fd90f8668`, tree
`6da75413c07be7587231e10e6659f3dde40c7eca`, base
`2c13623a3720199d2e92b13f9b353bcf054cc756` remain distinct, as do
CI38610992302/38612004333 and the unchanged 18,285,979-byte pair/0.084s.
Retain their linked #904/#903/#900/#899 failures and corrections; csc2D's
FLAGS_A bits5/6 destination-scan bypass, reloadMap working-layout reuse and
RightLeft facing findings are not replaced. [#907/PR909 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/909#issuecomment-6174944499)
retains its separate scope-only CI38610983007 result.

**Unknown:** natural combat outcome, reached return/interaction, actual supplied
flags/party/HP/class/items/actor mappings/current or saved geometry, mode/raft,
completed input/action/text/scroll/fade/service returns, visible/heard scene,
usable control and persistence. Counterexamples are source states, not executed
scenarios. [ADR0014](../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../decisions/0016-remake-start-evidence-deferral.md) keep these from
becoming automatic acquisition work. Chest acquisition/use, guard clearance
and Floor World departure remain explicitly outside this completed boundary.
