# Battle18 outcomes and Caravan acquisition

Evidence date: 2026-10-14. **Confirmed:** Battle18 has a leader-death cleanup
path and a first-completion after scene. The scene stages a new Caravan actor,
ends by setting F65 and clearing F84, and preserves the victory caller's return
seed. Repeated victory skips the scene but still runs the shared Bowie join.
Defeat and admitted escape select the supplied EGRESS_MAP, not Battle17's forced
Hassan destination. **Inferred:** the scene presents Rohde as the Caravan's
driver. **Unknown:** natural outcomes, supplied state, completed presentation
and action services, usable control and persistence.

## Agreement and provenance

Standard rigor under [Issue #844](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/844)
and [the content epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
Accepted starting base: `a54e0d8140b944b5348fd8d919a88e0c2c53a817`.
This sole document owns Battle18's selected outcome consumers, the complete
first-victory scene and immediate conditional exploration return. No shared
paths or stacked dependencies. Stop at that return/control boundary or an
unrelated battle admission. Damage/Achilles Sword effectiveness, combat
simulation, Caravan operation/menus/storage, Rick expansion, later travel and
natural campaign-state enumeration are excluded.

Read-only accepted dependencies are [Taros entry](./southwest-parmecia-through-taros-battle18-entry.md),
[Hassan/Battle17 outcomes](./hassan-departure-and-battle17-outcomes.md),
[Rohde's choice](./hassan-ancestry-and-rohde-caravan-choice.md),
[battle lifecycle](../../battle/battle-loop.md), [cutscene routing](../../battle/battle-cutscenes.md),
[map requests](../../world/map-event-request-consumption.md),
[common scripting](../../world/common-scripting.md) and [membership](../../party/common-stats.md).
Source paths below are relative to `disasm/` in
[SF2DISASM master at `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at that pin. Retained map-init/map-script-engine
payloads bind it and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Battle-loop and cutscene wrapper filenames below are under `code/gameflow/battle/`;
map-script/population consumers are under `code/common/scripting/map/`, and
entity-script consumers under `code/common/scripting/entity/`.
Fnnn means flag nnn; indices are zero-based. Dialogue is paraphrased; script
coordinates, action names and waits are source operands, not observed timing.

The retained pair is 18,285,979 bytes. Existing linear comparisons were budgeted
below 512 MiB and one minute; they passed in 0.088 seconds. Additional work is
bounded complete selected source/fixture reads and small prose/receipts, with
no regeneration, runtime, environment creation or new verification machinery.
Reassess before acquisition or material resource/scope growth.

## Outcome selection and shared battle boundary

**Confirmed:** `sf2enums.asm` resolves Taros Shrine12, Southwest Parmecia68,
Hassan9, `BATTLE_VERSUS_TAROS=18`, Bowie0, Peter7, Rohde11, FOLLOWER_B31,
Caravan mapsprite62, and mode0/1/2 as Bowie/Caravan/raft. Flag bases400/450/500
select F418/F468/F518. Rohde's accepted choice writes F725/F84/F418; none is
an alias for F65, force membership or current transport mode.

The [entry owner](./southwest-parmecia-through-taros-battle18-entry.md#battle18-before-scene-and-start-handoff)
retains fresh/saved-entry and roster limits. BattleLoop writes mode0 and F399
before its F88 saved-entry test. Fresh entry uses Map12 and battle18; F468
controls before/start, not outcome completion. Its 12 ally/14 enemy placements
include Taros as enemy128. This document does not simulate turns or establish
how that enemy takes damage.

Select slot18 from actual tables, not inferred per-battle filenames:

| Source table under `data/battles/` | Slot18 and consumer |
| --- | --- |
| `cutscenes/battleendcutscenes.asm` | `ms_Empty` at `0x47A4E`, shared terminator. |
| `global/enemyleaderpresence.asm` | -1, enabling the shared leader tail. |
| `cutscenes/afterbattlecutscenes.asm` | `abcs_battle18` at `0x4B2F2`. |
| `global/afterbattlejoins.asm` | 0, passed to JoinForce after either after-wrapper branch. |

`cutscenes/battleendcutscenesstart.asm:ExecuteBattleCutscene_Defeated` requires
Bowie HP nonzero and enemy128 HP zero before reaching either dispatch or the
leader tail. F518 clear executes the selected empty script; F518 set skips it.
Both eligible paths reach `battleendcutscenesend.asm:loc_47C48`. Because the
leader row is -1, it scans all 32 enemy slots and appends every nonzero-HP enemy
to DEAD_COMBATANTS_LIST, maintaining the trailing -1. It does not check their
X placement or set HP to zero in this tail. F518 does not suppress that cleanup.

`battleloop_1.asm` calls this hook after an individual turn, then
`battleloop/processkilledcombatants.asm:ProcessKilledCombatants`, then
`CountRemainingCombatants`. The killed-list consumer performs death presentation
and bookkeeping, writes listed combatant X/Y=-1, clears status, updates stats
and moves mapped entities to raw `(0x7000,0x7000)`. Its special-sprite path also
moves the special actor during death effects. Thus surviving enemies appended
by the leader tail are removed from counted placement; do not describe the
tail itself as directly zeroing their HP.

The counter counts nonnegative-X/nonzero-HP combatants and forces the ally
count to zero when Bowie's HP is zero. Zero allies is checked **before** zero
enemies. If neither ends battle, after-turn effects, another killed-list pass
and the same defeat-before-victory checks follow, without another defeated-hook
call at that second checkpoint. Taros's death is therefore a conditional
leader-cleanup trigger at the named hook, not an unconditional outcome override
when Bowie also dies. Empty defeated dialogue does not mean empty outcome work.

| Outcome | Selected mutation and return boundary |
| --- | --- |
| First victory, supplied F518 clear | Heal eligible allies, seed return parameters, run complete acquisition scene, join tail, clear F418, set F518; d4=1. |
| Repeated victory, supplied F518 set | Same seed and join tail, skip acquisition script; clear F418, retain/set F518; d4=1. F65/F84 are not repaired. |
| Defeat | Update unlock, exhaustion text/music, restore Bowie's HP, halve gold by unsigned shift, select savepoint; d4=-1. Battle4's special losable branch does not apply. |
| Admitted Egress / Angel Wing | Consume MP / remove selected item slot, text/flash/closure, update unlock, unwind action frame, select savepoint; d4=0. No victory scene/join/completion or defeat gold penalty. |

The escape dispatch in `battlefunctions/executeindividualturn.asm` tests the
masked action identity; `battlefunctions/battlefunctions_1.asm` owns the actual
consumption and return. This is not proof of menu availability or custody.
Its `UpdateBattleUnlockedFlag` leaves F418 unchanged if F518 is clear, otherwise
clears F418. Defeat/escape do not set F518, reset F468 or acquire the Caravan.

## Victory seed, scene population and membership

**Confirmed:** `battleloop_2.asm:BattleLoop_Victory` heals living/immortal allies
before the scene. The shared healer restores living allies and immortal
Peter/Lemon, retaining the supported status bits; it does not revive every dead
ally or supply party membership. It then seeds parameter2 with CURRENT_MAP,
parameters3/4 with the first current active member's combatant X/Y plus the
battle-area origin using byte arithmetic, parameter5 with that member's mapped
entity facing, and parameter1 with0.

`UpdateForceAndGetFirstBattlePartyMemberIndex` rebuilds the flag-selected active
list before choosing its first entry. The seed is not necessarily Bowie, the
first living ally, the initial placement or the after-scene position. Empty or
inconsistent lists/mappings are not repaired. For consistent Battle18 state,
map12 and area origin `(0,0)` preserve the selected member's final local X/Y.
The seed is taken **before** the scene rebuild and shared join.

`afterbattlecutscenesstart.asm` tests F518, not F65/F84/F725/F468. Clear executes
`data/battles/entries/battle18/cs_afterbattle.asm:abcs_battle18`; set jumps straight
to `EndAfterBattleCutscene`. No command in the selected scene writes a warp,
return parameters, CURRENT_BATTLE, PLAYER_TYPE or saved RAFT_MAP/X/Y. Its
immediate action bodies likewise contain no such writes. Scene `setPos` and
Caravan movement do not override the saved return tuple.

The scene loads Map12 with camera `(7,5)`, then `ce_4B6BE` at `0x4B6BE`.
`mapscriptengine_1.asm:csc42_loadMapEntities` consumes three main-header words:
main0 `(12,9)`, Up. The sole subsequent row is Caravan `(12,5)`, Down,
MAPSPRITE_CARAVAN, `eas_Init`, followed by the list terminator. The header is
not an NPC. This load does not run Map12's exploration init.

`mapfunctions.asm:InitializeMapEntities` clears actors/lookups, declares current
flag-selected followers, consumes fixed rows, then creates main0 separately.
The sole non-ally row now maps logical128 to the next ordinary physical slot.
There is no Taros row in this scene. This Caravan128 is distinct from the
before-scene Caravan129 and rebuilt battle neutral159. Reusing128 does not
modify Taros's combatant HP, create a combatant, or declare a controlled vehicle.

Map12 uses the normal follower table even if F65 was independently supplied set.
`data/scripting/entity/followers.asm` admits Peter via F67, Rohde via F84,
Oddler/FOLLOWER_B via F68; flags74/70 can also overwrite FOLLOWER_B with Astral
rows. Other supplied follower flags retain their order and effects. The scene
supplies none of these three actors itself and does not test their presence.
For example, only F67/F68/F84 set produces physical1 Peter,2 FOLLOWER_B,3 Rohde,
then4 Caravan128. This is a derived conditional allocation, not an asserted
natural party. Missing mappings start at0, so logical actor commands can alias
main0; invalidated255 is a different invalid state. Excessive/conflicting
followers are not given a safe capacity or identity repair by the scene.

## Complete first-victory program

**Confirmed:** the retained `programCorpus.programs` entry `abcs_battle18`
contains 212 commands, including 16 complete inline movement bodies, four custom
action bodies and 15 text sites. The following covers the whole program.
“Start” installs an action without waiting. “Wait” polls only that actor's
`eas_Idle` action pointer; it does not join every previous action. Every inline
move below ends with `endActions`; every custom body ends with `ac_end` after
its explicit jump to idle. Caravan below means this scene's logical128. Direction-plus-number denotes
a tile displacement; speech names followed by numbers denote text IDs.

| Commands | Ordered work |
| --- | --- |
| 0–13 | Cursor2528; load Map12/camera `(7,5)` and `ce_4B6BE`; wait Init for Bowie, Peter, FOLLOWER_B, Rohde; set Peter `(13,9)` Up, FOLLOWER_B `(14,9)` Up, Rohde `(5,9)` Right; stop Caravan animation; fadeInB; wait 60; Rohde2528. |
| 14–25 | Bowie/Peter/FOLLOWER_B Left; wait 30; camera follows Rohde; wait 120; wait Rohde Right6; other three Up; wait Rohde Up4; Rohde Right. |
| 26–40 | Neutral camera; destination `(7,1)`; wait 30; Rohde positions `(12,6)` Up, `(13,5)` Left, `(12,6)` Up, `(11,5)` Right, `(12,6)` Up, `(13,5)` Left, with wait 20 after each. |
| 41–52 | Rohde2529; start Rohde Left1; wait 3; start Rohde `eas_452BA`; wait 3 then 60; start Caravan animation, wait 30, stop, wait 60, start, wait 120. |
| 53–67 | Caravan faces Left/Up/Right/Down/Left/Up/Right/Down, wait 10 after each except final Down. |
| 68–74 | Start Caravan Jump; wait 10; Rohde `(63,63)` Down; wait custom Caravan speed48/48 then idle; falling SFX; start Caravan Down9; wait 20. |
| 75–83 | Wait custom Bowie speed64/64 then idle; wait Bowie auto-facing off; start Bowie Left1; Bowie Right, Peter/FOLLOWER_B Left; wait 60; camera destination `(7,4)`; wait Bowie Init. |
| 84–97 | Bowie/Peter/FOLLOWER_B Down; Peter2530; wait 60; Bowie Right, Peter/FOLLOWER_B Left; Peter2531; wait 30; all three Down; wait 150. |
| 98–104 | Wait Caravan auto-facing off; start Caravan Up9; falling SFX; wait 20; Caravan/FOLLOWER_B Left; wait 20. |
| 105–128 | Bowie/Peter face Up/Up, Left/Right, Down/Down, Right/Left; repeat that four-pair sequence once, wait 10 after every pair. |
| 129–139 | Wait custom Bowie, then Peter: motion OFF, orientLeft, updateSprite, jump idle; stop Bowie/Peter animation; wait 30; Caravan Down; wait 60; stop Caravan animation; wait FOLLOWER_B BumpLeft twice; wait 30. |
| 140–150 | Rohde `(12,5)` Right; wait `eas_4536C`; wait 3; wait Bowie Init then Peter Init; Bowie/Peter/FOLLOWER_B Up; start Bowie/Peter animation; wait Rohde Down3. |
| 151–160 | Rohde2532; wait Rohde Left2; Rohde Down,2533; wait Rohde Right1; Rohde Up,2534; wait Rohde Up2; Rohde Down,2535. |
| 161–170 | Wait 30; narrator2536; wait Peter Up1; Peter2537; wait Peter Down1; Peter Left, Bowie Right, FOLLOWER_B Left; Peter2538; wait Rohde Down1. |
| 171–179 | Bowie/Peter/FOLLOWER_B Up; Rohde2539; Peter2540; Bowie Right, Peter/FOLLOWER_B Left; Peter2541. |
| 180–189 | Wait Rohde Up1; Rohde Down; wait 20; wait Rohde Jump twice; Bowie/Peter/FOLLOWER_B Up; Rohde2542; wait 30. |
| 190–198 | Start Rohde Up1; wait 3; start Rohde `eas_452BA`; wait 3 then 60; start Caravan animation; Rohde `(63,63)` Down; wait 60; wait Caravan Init. |
| 199–211 | Start Caravan Down9; wait 30; Bowie Right, Peter/FOLLOWER_B Left; wait 10; all three Down; wait 180; setF65; clearF84; end. |

All 15 speech commands are `nextSingleText`. All actor modifiers are `$0`;
command 162 alone uses `$FF,ENTITY_NONE` (packed -1). No Yes/No prompt, nod,
JoinForce, JoinBattleParty, item award or membership test occurs in this program.

| Text / key | Attribution and meaning; control tokens |
| --- | --- |
| 2528 / `09E0` | Rohde compliments the named leader's strength; LEADER/N/W1. |
| 2529 / `09E1` | Rohde remarks on the small size and attempts entry; N/W1. |
| 2530 / `09E2` | Peter reacts to the disappearance; W1. |
| 2531 / `09E3` | Peter hopes for a return and questions Rohde's honesty; N/NAME;11/W1. |
| 2532 / `09E4` | Rohde denies lying; W1. |
| 2533 / `09E5` | Rohde praises the Ancients; N/W1. |
| 2534 / `09E6` | Rohde explains the small Caravan in terms of miniaturized people and items; N/W2. |
| 2535 / `09E7` | Rohde claims driving ability and asks to accompany them as driver; N/W1. This is spoken wording, not an interactive choice. |
| 2536 / `09E8` | Narration says the named historian accompanies the force; NAME;11/N/W1. |
| 2537 / `09E9` | Peter expresses disbelief; W1. |
| 2538 / `09EA` | Peter suggests they can bring many soldiers; N/W1. |
| 2539 / `09EB` | Rohde says miniaturized soldiers inside cannot enter battle; N/W1. |
| 2540 / `09EC` | Peter reserves judgment; W1. |
| 2541 / `09ED` | Peter expresses dislike; W1. |
| 2542 / `09EE` | Rohde anticipates adventures with the Caravan; N/W1. |

Text is joined to `data/scripting/text/gamescript.txt` by hexadecimal key.
The last selected key is 09EE; the next Oddler scene is not part of this program.
These statements establish attributed dialogue, not operational storage,
miniaturization rules for combat membership, or a recruited Rohde combatant.

## Immediate action, identity and service consumers

**Confirmed:** `sf2cutscenemacros.asm` and the `mapscriptengine_1.asm` consumers
resolve action installation rather than treating command order as completed
motion. `csc2D_entityActionSequence` compiles each move into relative movement,
destination waiting and idle. `csc14` installs the inline custom body; `csc15`
installs the named body. Their wait forms poll that mapped actor's idle pointer.
Nonwaited installs can overlap and replace unfinished actions.

`entityscriptengine_2.asm:esc04_moveToRelativeDest` scales offsets by384 from
current coordinates, checks destination conflict, then enters the destination/
travel/velocity setup. `esc01_waitUntilDestination` advances only when current
X/Y equal destination X/Y. Speed48/48 and64/64 are two byte operands copied as
one word to adjacent X/Y speed fields by `esc10`; they are not durations.
`eas_Init` restores speed32/32, acceleration settings, obstruction/collision,
auto-facing, Up orientation, immersion and size24 before refreshing and idling.
It does not wait for an old destination merely because the scene waits for Init.

`data/scripting/entity/eas_actions.asm:eas_Jump` disables auto-facing, requests
relative `(0,-1)`, waits5, requests `(0,1)`, waits5, requests `(0,0)`, restores
auto-facing and idles. BumpLeft uses `(-1,0)`, `(1,0)`, `(0,0)` with the same
5/5 waits. The two waited FOLLOWER_B bumps and two waited Rohde jumps are
actor-local. At commands 68–71 Caravan Jump is started, followed by only csWait10
and another action installation; its completion is not independently joined.
Similarly commands 98/129/130/198 do not prove earlier nonwaited moves finished.

The custom Bowie/Peter bodies use `ac_motion OFF`, which `esc14` implements as
animation counter=-1, not removal or translation cancellation. `ac_orientLeft`
selects orientation1, distinct from Left facing2, then `ac_updateSprite` refreshes.
`startEntity`/`stopEntity` write animation counter0/-1, not a vehicle controller.
`setPos` changes current and destination coordinates and refreshes the sprite;
it does not cancel the installed action. Rohde `(63,63)` is a scripted relocation,
not deletion, loss of HP or membership, and a still-running action can later
write that actor's position again.

### Shrink and return bodies have distinct physical operands

**Confirmed:** the two installs of `code/common/scripting/map/vehiclefunctions.asm:eas_452BA`
start **after** the preceding `eas_452B2` movement command, so they do not execute
its move-to-Caravan request. `eas_452BA` waits6, enables resizing, submits warp
SFX, sets size22/20/18/16/14/12 with sprite refreshes and wait 1 between successive
sizes, waits for its existing destination, clones `ENTITY_CARAVAN`, then branches
to idle. `esc0B_setSpriteSize` writes the shared SPRITE_SIZE word; `esc1F` toggles
the actor's resize bit and sprite refresh remains a service dependency.

`ENTITY_CARAVAN=1` here is a **raw physical slot**, not logical128/FOLLOWER_A.
`esc0D_clonePosition` shifts the operand directly into ENTITY_DATA, with no
logical lookup. It copies physical1's current X/Y, destination, travel and facing
and refreshes Rohde's sprite. Under the F67/F68/F84-only allocation above,
physical1 is Peter and the staged Caravan is4. With no admitted followers the
sole Caravan row can occupy1, but Rohde's mapping is then absent. Other supplied
flags change allocation. The symbolic name is not proof that the clone targets
the staged Caravan in this scene, nor does it prove an observed visual defect.

Command 141 instead enters `eas_4536C` **after** `byte_45368`'s clone instruction.
It requests one tile relative to Rohde's current facing, enables resizing,
submits warp SFX, refreshes size12/14/16/18/20/22 with wait 1 after each, disables
resizing, refreshes size16, waits for destination and idles. `esc09` derives the
relative vector from current facing plus its offset. With the preceding accepted
`setPos (12,5),Right`, this requests `(13,5)`; it is not intrinsically a downward
exit or an implicit clone of Caravan128. If that setPos is skipped or the actor
is missing, the actual destination differs. Neither body calls the enclosing
MapEventType2/4 routines that write transport mode. Reusing their action labels
therefore does not prove boarding, disembarking or vehicle control.

### Health, text and completion limits

**Confirmed:** `GetEntityAddressFromCharacter` resolves current lookup without a
presence check. `setPos`/`setFacing` and start/stop-animation call
`AdjustScriptPointerByCharacterAliveStatus`; zero-HP allies below30 skip the
command, while FOLLOWER_B31 and Caravan128 bypass that ally guard. Action
installation and custom bodies do not use that guard. Victory healing makes
eligible Peter live but does not guarantee a dead Rohde is healed or declared.
Absent actors, HP and physical slots remain separate dependencies.

`mapscriptengine_2.asm:csc00_displaySingleTextbox` waits for view scrolling,
uses the mapped portrait/speech properties for actor text, displays the cursor
text, increments it, closes portrait/text and sleeps10. Packed -1 narrator text
uses speech SFX0 and bypasses actor portrait selection. LEADER reads the first
flag-rebuilt active member's name; NAME;11 selects Rohde's name. Neither is a
membership operation. The accepted [text/service owner](../grans-opening/map3-messenger-acceptance.md)
is not a runtime observation of these 15 sites. W1/W2 input polling, VInts and
RNG updates retain their shared dependencies; W2 remains inside the 2534 text
even though its command is `nextSingleText`.

The reader clears skip state at entry, but debug/player2-Start can set it;
text handlers then skip their arguments without display/cursor increment and
csWait skips Sleep. Ordinary csWait is its low-byte VInt sleep, not guaranteed
elapsed wall time. This affects even the short intervals preceding overwrites.
Load/fade, sprite refresh, sound submission and text acknowledgement are not
all-actor motion barriers. `fadeInB` uses the shared half-in-from-black fade;
fixed camera destinations disable tracking and wait for scrolling, while camera
entity selection maps the current actor. Neutral `$FFFF` disables tracking.
End conditionally waits for scrolling when a dialogue window index is present,
clears camera speed and restores registers; it does not join outstanding actions.

## Terminal writes and immediate exploration

**Confirmed:** after the last nonwaited Caravan Down9 and the source waits,
commands 209/210 set F65 and clear F84. They neither set F725 nor join Rohde nor
write PLAYER_TYPE/F64/saved raft. Clearing F84 does not retroactively delete the
currently staged actor; it changes subsequent flag-selected population.
Other follower flags, including conflicting identities, remain supplied.
A first-completion scene still runs with F65 already set, or F84 absent, because
its wrapper tests only F518. Repeated completion does not repair F65-clear or
F84-set inconsistent state because it skips all 212 commands.

Both wrapper paths reach `afterbattlecutscenesend.asm:EndAfterBattleCutscene`.
Its row18 value0 calls `j_JoinForce(0)`, resolving via the s02 interface to
`code/common/stats/battleparty.asm:JoinForce`: set Bowie's joined F0, update force,
and activate him if the active count is below 12. The source comment calling
the table unused does not remove this call. It is not Rohde recruitment and
occurs after return-member selection. Only then does BattleLoop_Victory clear
F418, set F518, read parameters2–5 and return d4=1. F468 is retained.

### Victory Map12: control or warning/retreat

**Confirmed:** under consistent selected state, the return is Map12 with the
pre-scene seeded X/Y/facing, not `(12,9)` or the staged Caravan's final position.
MainLoop resumes at `alt_MainLoopEntry`, SwitchMap then ExplorationLoop, **without
CheckBattle**. The complete flag-switched-map table has no source12 row. F418
was cleared; even an independently supplied unlock does not cause a scan at
this direct outcome handoff. A later ordinary request has its own admission.

The supplied-map exploration branch clears pending, subtracts 20000 from the
word STEP_COUNTER with a negative-result clamp, heals eligible allies, fades/
loads tiles, sets CURRENT_MAP=12/CURRENT_BATTLE=255, rebuilds setup entities,
clears temporary flags256–383, sets F80, loads map, installs base VInts and runs
init. It does not preserve the scene population or its outstanding animations.
Map12's sole setup declares Taros128 at `(12,8)` Down using special physical47;
there is no Caravan row. Map12 is not an overworld, so F65 does not add the
Caravan follower here. After first acquisition F84 clear omits Rohde's follower;
Peter/FOLLOWER_B and other supplied flags still choose their own actors.

`map12/mapsetups/s6_initfunction.asm` checks F725 first. Clear runs the complete
accepted warning `cs_57D22`, then it checks F518 and moves Taros128 off-map.
Set skips the warning and proceeds to that same F518 check. Victory has just
set F518, so both stable-state paths ultimately move Taros out, but completion
alone does not suppress the warning. Neither init nor acquisition repairs F725.

| Supplied F725 on victory exploration | Immediate consumer and stopping condition |
| --- | --- |
| Set | Skip warning; move Taros out for F518; conditional music/fade; absent a new pending event, WaitForEvent follows physical0 and installs current-mode control. Normally mode0 remains from battle entry. |
| Clear | Run the accepted 51-command warning before moving Taros out; its scene requests Map68 `(55,44)` Down. After init/music/fade, pending consumes that request without an intervening ordinary-control installation. |

The [complete warning and return owner](./southwest-parmecia-through-taros-battle18-entry.md#map12-population-complete-warning-and-retreat)
retains every command/text/action, including missing actors and asynchronous
final moves. Its csc07 warp writes type1/no-scroll/Map68/X55/Y44/Down but no
WARP_SFX; the consumer clears pending, submits/clears the supplied sound, idles
main0 and returns `(68,55,44,Down,d4=0)` to MainLoop's normal scan. No guaranteed
playable shrine pause is inserted. Replaced pending state is not an atomic queue.

At Map68 the sole battle row is19: supplied F419 admits it, with F519 able to
clear its unlock after selection. Stop at that unrelated admission. Otherwise
fresh Map68 exploration uses its empty setup and return-only init. F65 set
selects the overworld follower table, which starts with a Caravan FOLLOWER_A
row at physical1 and then the normal table. Later conflicting FOLLOWER_A flags
can overwrite that logical lookup; declaration is not vehicle-control admission.
First acquisition has cleared F84; repeated victory preserves its supplied value.
Absent another pending event, the usual WaitForEvent control boundary follows.

F64 and saved RAFT_MAP/X/Y are preserved by victory and warning. For mode0 a
separate physical31/lookup63 raft is declared only when F64 is set and the saved
map matches the newly populated map. Mode2's distinct population path changes
main's sprite and hides followers; neither the acquisition nor warning produces
that mode. F65, Caravan declaration, current mode and usable operation remain
separate facts. This slice stops before any boarding/movement/menu/storage use.

### Defeat/Egress: supplied savepoint, not a fixed Hassan return

**Confirmed:** `battleloop/getegresspositionforbattle.asm` has a hardcoded Hassan
case for `BATTLE_TO_TAROS_SHRINE=17`, **not** `BATTLE_VERSUS_TAROS=18`. Battle18
falls through to EGRESS_MAP and `code/common/maps/egressinit.asm:GetSavepointForMap`.
Normal BattleLoop already set F399. Savepoint selection scans
`data/maps/global/savepointmapcoords.asm`; a match supplies its X/Y/facing. No
match retains the supplied map byte and defaults `(1,1)`, Up. This is not a
validated arbitrary-map contract. A direct abnormal F399-clear call returns the
initial Bowie's-room tuple before the table and raft scan.

If F64 is clear, saved raft coordinates remain unchanged. If set, the selected
savepoint map chooses `raftresetmapcoords.asm`: Hassan9 resets raft to Map69
`(28,55)`; Polca13 or Bedoe10 resets it to Map72 `(43,48)`; any other map reaches
the sentinel default Map69 `(42,15)`. These are saved-tuple writes, not boarding,
transport normalization or a claim that the raft appears on the return map.

EGRESS_MAP itself is supplied state. For the concrete conditional selection9,
return is Hassan `(8,9)`, Up. Its accepted
[population/init owner](./battle16-outcomes-through-hassan-entry.md#fresh-hassan-population-init-and-conditional-control)
selects the default/F518 alternate cast and shared init; normal followers,
F65's Rohde movement-out and other supplied flags retain the owner's limits.
First loss leaves F518 clear; repeated loss retains it. Neither loss sets F65
or clears F84. No new Rick investigation or interaction follows from using this
accepted init. With F64 set the reset raft is on69, so it is not placed in town9.
After init and conditional services, no pending event permits ordinary control.

All direct battle outcome returns enter MainLoop's post-battle exploration path
without CheckBattle. Other supplied savepoint maps undergo their own SwitchMap,
setup/init and pending-event behavior; this slice does not assert they all use
Hassan's empty-request return. The actual selected savepoint and any non-Hassan
init continuation remain **Unknown** without supplied state. No natural loss
itinerary, all-map init closure or automatic runtime acquisition is claimed.
The shared fresh branch applies when a map is supplied; an inconsistent 255
uses ExplorationLoop's separate existing-population branch, not a fresh rebuild.

## Reproduction, acceptance and deferred evidence

Load `. ./local/private-inputs.ps1` in the same process as controlled commands.
Verify the read-only source pin/clean status. Run the existing
[retained comparison recipe](../grans-opening/battle06-to-battle07-content-route.md#reproduction-and-review-boundary):
map-init payload equality, `_verify_map_script_engine_fixture`, and source
`_cutscene_facts` against `battle-cutscenes-static-v1.json`. All passed without
rewriting retained payloads. No ROM extraction, simulation or runtime is claimed.

Select `abcs_battle18` from retained `programCorpus.programs`; join every indexed
command to its source line and review all 16 movement/four custom bodies, the
complete ce_4B6BE header/row, and text keys 09E0–09EE. Inspect actual slot18
pointers/leader/join rows and complete wrapper/caller bodies. Resolve the action
macros to csc14/15/2D and esc01/04/09/0B/0D/10/14/1B/1F consumers; in particular
join raw physical1 to follower-before-fixed-row allocation. The s05/s07/s02
jump interfaces resolve battle/entity, scripting and JoinForce aliases.

| Existing fixture under `tests/fixtures/h2/` | Reuse boundary |
| --- | --- |
| `map-init-static-v1.json`, `map-script-engine-static-v1.json` | Retained comparisons and direct complete selected scene/init joins, not a new executable composed outcome trace. |
| `battle-cutscenes-static-v1.json`, `battle-routing-data-static-v1.json` | Source wrapper comparison and direct slot18/return/Map68 routing joins. |
| `battle-loop-static-v1.json`, `battle-control-static-v1.json`, `common-stats-static-v1.json` | Shared outcome/roster/membership limits, no new combat simulation. |
| `map-event-request-consumption-static-v1.json` | Shared pending/warp consumers, with conditional warning return reviewed directly. |

Discriminating supplied states include Bowie/enemy128 HP combinations, the
second end checkpoint, independent F518/F468/F65/F84/F725, inconsistent member
lists/mappings, dead or absent Rohde, conflicting follower flags/physical1,
interrupted or overwritten actions, debug text skipping, EGRESS_MAP variants,
F399/F64/saved raft and mode0/1/2, and replaced pending requests. These constrain
source/prose review; they are not an executed runtime state matrix.

**Unknown:** natural battle outcome/reach, actual flags/HP/roster/lookups/seed/
savepoint/transport, all-actor and sprite/text/fade/camera/audio completion,
rendered shrinking/disappearance/return, input usability and save/resume
persistence. They remain deferred under
[ADR0014](../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../../../decisions/0016-remake-start-evidence-deferral.md). Attributed
miniaturization and driver claims do not close later operational capability.

The [PR842 final handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/842#issuecomment-6161224116)
and [independent acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/842#issuecomment-6161232331)
preserve its original/rebased checks, inspection failures and predecessor links.
Guessed-path/read failures are not absence evidence. This Issue/PR handoff owns
current corrected inspections, exact Git identities and actual final-head CI.
Acceptance requires complete selected-source/owned-prose/provenance/link/private
review, `git diff --check`, clean committed
`uv run sf2 verify plan --base origin/main --head HEAD` and final-head CI. No
blanket local normal/full/engine/Godot/H3/H4 suite is selected solely for prose.
Freeze a clean pushed Draft PR for independent main-gate acceptance and merge.
