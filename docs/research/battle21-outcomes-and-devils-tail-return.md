# Battle21 outcomes and Devil's Tail return

Evidence date: 2026-10-14. **Confirmed:** Battle21 has a leader-death cleanup
path, an empty defeated script and an F521-gated after scene. The complete
scene stages B/Peter's conversation, installs temporary follower actions and
requests Map67 `(26,23)`, Down. Its actors still use the cropped battle layout;
the later exploration request uses full-map coordinates. Repeat victory skips
the scene and uses the pre-scene seed instead. **Inferred:** B's lines are
intended as Oddler's response to Peter. **Unknown:** natural outcome, supplied
state, completed services, actual presentation/control and save persistence.

## Agreement and provenance

Standard rigor under [Issue #888](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/888)
and [content epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514):
resolve the outcome, complete after scene and conditional Devil's Tail return
for the next story guide. Accepted starting base is
`0a4e8c6980086ad712ce1bcde740583d21f69a87`. This is the sole owned document;
no shared edits or stacked dependencies. Consume accepted
[Battle21 opening/roster](./battle21-opening-and-initial-roster.md),
[Battle20 outcome consumers](./battle20-outcomes-and-elric-recruitment.md),
[battle lifecycle](./battle-loop.md), [cutscene routing](./battle-cutscenes.md),
[Fairy Cave return](./fairy-cave-exploration-and-devils-tail-return.md) and
[map request consumption](./map-event-request-consumption.md).

Stop at conditional Map67 control or pending return, with its separate mansion
doorway identified only as the next boundary. Exclude mansion initialization,
interior/Chessboard/Battle22, later recruitment/lore, general combat/AI/damage or
item/drop audits, terrain reconstruction, runtime acquisition and new verification
machinery. Static service sequences do not establish a played encounter.

Source paths are relative to `disasm/` in
[SF2DISASM master at `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The assigned read-only checkout is clean at that pin. Retained evidence binds
USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
No fresh ROM extraction or runtime observation is claimed. Fnnn denotes flag
nnn; table slots and command indices are zero-based. Coordinates, speeds and
waits below are source operands, not measured motion or elapsed durations.
Short battle paths start at `code/gameflow/battle/`; map-script consumers at
`code/common/scripting/map/`; entity consumers at `code/common/scripting/entity/`.
Map67 setup files live in `data/maps/entries/map67/mapsetups/`.

Reuse the unchanged 18,285,979-byte map-init/map-script-engine pair and small
selected source/fixture reads. Linear comparisons are budgeted below 512 MiB
and one minute plus small receipts/prose; retained pair/source comparison passed
in 0.102 seconds. No environment, capture or payload was created. Reassess before
material scope or resource expansion.

## Actual outcome routing

**Confirmed:** these are actual selected rows and their immediate consumers:

| Source | Battle21 selection and effect |
| --- | --- |
| `data/battles/cutscenes/battleendcutscenes.asm`, slot 21 | `ms_Empty`; its complete body in `code/common/scripting/map/ms_empty.asm` is the terminator. The defeated wrapper first requires Bowie HP nonzero and enemy 128 HP zero. F521 clear dispatches the empty script; set bypasses it. Both eligible paths reach the leader tail. |
| `data/battles/global/enemyleaderpresence.asm`, slot 21 | -1. The tail scans enemy IDs 128–159 and appends every nonzero-HP entry to DEAD_COMBATANTS_LIST, maintaining the terminator and length. It does not test placement X or set these HP values to zero. |
| `data/battles/cutscenes/regionactivatedcutscenes.asm` | The complete table selects battles 32/40 only. ExecuteBattleRegionCutscene scans past all rows for 21 and returns; no Battle21 region scene. This is not a claim that its AI regions are inactive. |
| `data/battles/cutscenes/afterbattlecutscenes.asm`, slot 21 | `abcs_battle21` at `0x4B88C`. ExecuteAfterBattleCutscene checks F521; clear runs the program, set skips it. Both reach EndAfterBattleCutscene. |
| `data/battles/cutscenes/afterbattlejoins.asm`, slot 21 | 0, consumed by EndAfterBattleCutscene through j_JoinForce: Bowie, not a new named recruit. |

Wrapper bodies are `cutscenes/battleendcutscenesstart.asm` and
`battleendcutscenesend.asm`, `afterbattlecutscenesstart.asm` and
`afterbattlecutscenesend.asm`. The s07 layout
`layout/sf2-07-0x044000-0x064000.asm` includes the **cutscenes** join table after
the after-wrapper tail. The similarly named `data/battles/global/afterbattlejoins.asm`
is not this table; an unused-feature comment does not negate the executed call.

`battleloop_1.asm` calls the defeated hook after an individual turn, then
ProcessKilledCombatants and CountRemainingCombatants. The killed consumer in
`battleloop/processkilledcombatants.asm` performs presentation/bookkeeping,
sets listed X/Y to -1, clears status, updates stats and moves mapped actors
off-map. Thus the leader tail can remove still-living enemies from the counted
population without clearing their HP. Under coherent state, leader death with Bowie surviving
can reach victory without individually killing all twelve initial
enemies. The roster alone is neither an all-enemies objective nor a tactical
success prediction.

`battleloop/countremainingcombatants.asm` scans 30 allies and 32 enemies,
counting only nonnegative X with nonzero HP; Bowie HP zero forces the ally count
to zero. BattleLoop tests zero allies **before** zero enemies: both-zero means
defeat. Bowie dead also bypasses the defeated hook's leader cleanup. If both
counts remain nonzero, ProcessAfterTurnEffects runs, followed by a second killed
pass and the same ordered counts. There is no second defeated hook there. A
leader reaching HP0 during after-turn effects therefore does not itself invoke
the leader tail at that second check; subsequent execution/state matters.
An eligible hook/empty script or death service must return before counting.

`cutscenes/afterenemyleaderdeathpositions.asm` and the complete
`data/battles/global/afterbattlepositions.asm` select only Battle05. No Battle21
staged-position override runs there. The common debug kill-all path is separate
from the ordinary leader consumer and is not evidence of natural victory.

| Outcome | Selected order and result |
| --- | --- |
| First victory, F521 clear | Heal eligible allies; seed return parameters; run complete after scene, including warp override; common JoinForce(0); clear F421; set F521; read current return parameters; d4=1. |
| Repeat victory, F521 set | Heal/seed; bypass every after-scene record; common JoinForce(0); clear F421; set F521; read seed unless another writer replaced it; d4=1. |
| Defeat | Update unlock; exhaustion music/text; restore Bowie HP to maximum; unsigned gold divide by two; select Egress destination; d4=-1. The special Battle04 losable branch is not selected. |
| Admitted Egress / Angel Wing | Deduct spell MP / remove selected item slot; text/flash/closure; update unlock; unwind individual-action frame; select Egress destination; d4=0. No victory scene/join/completion or defeat gold penalty. |

`battlefunctions/executeindividualturn.asm` checks action type and masked spell
or item ID before dispatching the escape bodies in `battlefunctions/battlefunctions_1.asm`.
That admission does not prove menu availability, MP sufficiency or item custody.
UpdateBattleUnlockedFlag preserves F421 if F521 is clear and clears F421 if
F521 is set. Neither defeat nor escape sets F521 or changes F471.

Suspension is a different continuation. `battlefunctions/battlefunctions_2.asm:BattlefieldMenu`
allows the suspension prompt when CURRENT_BATTLE is nonzero, including 21.
A negative prompt result returns to the menu; acceptance saves the seconds,
sets F88, calls SaveGame with the current slot, then fades and jumps to
WitchSuspend. Debug plus P1 Start instead clears F88 and returns to the menu
after that save call. No victory result or F521 write follows from suspension.
On later F88-set BattleLoop entry, mode 0/F399 are installed, saved seconds are
restored, F88/AI memory are cleared and LoadBattle leads directly to individual
turns. It bypasses fresh map/battle assignment, before/start scenes, healing and
roster initialization. Supplied saved map/battle/area/party must be coherent;
this source path does not prove successful save/load persistence.

## Victory seed and coordinate lifetime

**Confirmed:** `battleloop_2.asm:BattleLoop_Victory` first calls
`battleloop/heallivingandimmortalallies.asm`. Across all 30 ally slots it heals
living allies and immortal Peter 7/Lemon 28 to HP/MP maxima. Its intermediate
mask retains STUN/POISON/CURSE; UpdateCombatantStats preserves STUN/POISON and
derives final CURSE from equipped cursed items, so an old CURSE bit alone does
not survive refresh. It neither requires membership nor joins anyone, and does
not revive other dead allies. The
[initialization/item owner](./battle21-initialization-and-first-control.md#item-selection-conditional-use-and-independent-drops)
retains actual Herb/Ring slots and independent Broad Sword135/Ring129 once7/8
drops; the leader sweep here does not guarantee loot from every removed enemy.
Battle44's timer-window special case is not selected.

Before checking F521, victory writes parameter2=CURRENT_MAP; parameters3/4=
first active member's combatant X/Y plus BATTLE_AREA_X/Y, using byte arithmetic;
parameter5=that member's mapped battle-entity facing; parameter1=0.
`code/common/scripting/text/textfunctions_1.asm:UpdateForceAndGetFirstBattlePartyMemberIndex`
calls UpdateForce then reads the first active-list byte. The list is rebuilt
from joined/active flags in ascending ally-ID order. This is not a first-living
search or an unconditional Bowie selection, and has no empty-list repair.
The seed's facing lookup uses GetEntityIndexForCombatant before scene replacement.

Normal coherent Battle21 state supplies Map67 and area `(21,0)`, size 32x42,
from slot 21 of `data/battles/global/battlemapcoords.asm` through CheckBattle.
The seed converts combatant coordinates back toward the full source-map domain
by adding those offsets. Starting placements and Fairy Cave arrivals are not
the current member's return position. Changed party/HP/placement/lookup or
stale saved geometry can change the seed; the later common join cannot repair
which member already supplied it.

The after wrapper does **not** clear CURRENT_BATTLE. The script's
loadMapFadeIn Map67 camera `(4,18)` invokes `mapscriptengine_1.asm:csc37` → csc48:
start fade-out, disable view following, load tilesets, wait fading, deactivate
VInts, pack camera operands and call LoadMap, then enable display/VInts.
`code/common/maps/mapload.asm:LoadMap` writes CURRENT_MAP=67, loads base
blocks/layout, flag copies and chest overlays; CURRENT_BATTLE still 21 makes
LoadMapBlocksAndLayout copy the supplied battle rectangle to working `(0,0)`.
`code/gameflow/exploration/exploration.asm:CopyMapBlocks` consumes the packed
origin/size on the 64-wide word layout. LoadMapArea uses battle layer-1 start 0
and ends `(31,41)*384`, not ordinary full-map bounds.

| Operand / consumer | Domain on the coherent Battle21 path |
| --- | --- |
| Scene main `(10,22)`, Peter `(11,23)`, B `(11,21)`, A `(11,22)` | Scene entity/setPos operands directly become tile×384 within the cropped working layout. No subtraction or addition of battle origin occurs. Source-layout correspondences are `(31,22)`, `(32,23)`, `(32,21)`, `(32,22)`. |
| Scene camera `(4,18)` | csc48 packs/multiplies by 3; LoadMap converts block units by 128. It is a camera input in the same working domain, not a return tuple or player position. |
| Inline moves | Relative to each mapped actor's current coordinates through esc04_moveToRelativeDest; no battle-origin conversion. |
| Final warp Map67 `(26,23)`, Down | csc07 writes literal request bytes; it neither moves scene actors nor adds 21. Victory later returns these parameters; fresh ExplorationLoop clears CURRENT_BATTLE to 255 before loading Map67. The arrival is therefore full-map `(26,23)`, not cropped 26 or global 47. |

Neither the scene loader nor warp recomputes BATTLE_AREA fields. They remain
available while battle21 is current; after CURRENT_BATTLE=255 the full-map
loader ignores them. The scene itself does not walk the actors to the mansion
doorway. Source correspondences above explain storage domains without decoding
terrain, proving passability or claiming the rendered camera/actor positions.

## Complete scene, population and identity

**Confirmed:** `data/battles/entries/battle21/cs_afterbattle.asm` contains all
33 top-level records of abcs_battle21, its complete custom body and two inline
move bodies. `ce_4B948` at `0x4B948` is main `(10,22)`, Left; one fixed row Peter 7
`(11,23)`, Left, eas_Init; then cscEntitiesEnd. No fixed A, B or enemy row exists.
A/B below mean logical FOLLOWER_A 30/FOLLOWER_B 31, not guaranteed identities.

`csc42_loadMapEntities` deactivates VInts/display, reads the main header and
calls InitializeMapEntities, loads mapsprites, then reenables display/VInts.
It does not call ordinary Map67 setup/init. `mapfunctions.asm` clears entities
and lookup bytes, declares selected followers starting at physical 1, handles
the fixed Peter row, then declares Bowie at physical 0 with eas_Idle.
A nonzero existing Peter lookup skips the fixed declaration; zero creates it.
That duplicate check is not an HP, joined or active-party test.

Map67 is in `data/maps/global/overworldmaps.asm`. In
`followersfunctions_1.asm` and `data/scripting/entity/followers.asm`, F65 selects
the overworld Caravan/A prefix; F67 selects Peter; F68 selects Oddler/B.
With only F65/F67/F68 set among follower selectors, these occupy physical 1/2/3
and the fixed Peter is skipped. Without F67, a fixed Peter is still declared
after whichever followers exist. F66 adds Sarah/Chester; the full ordered table
also permits other allies and repeated A/B assignments. F70's later Astral/B
row can overwrite Oddler's lookup; later A rows can overwrite Caravan's.
F521/F471 and battle-party membership do not establish these follower flags.

`entityfunctions_1.asm:ClearEntities` zeros lookup bytes. In
`mapscriptengine_1.asm:GetEntityAddressFromCharacter`, the masked logical ID
indexes that byte (enemy IDs subtract 96) and produces a physical address with
no missing-actor guard. Absent A/B therefore alias physical 0. A's setPos or B's
actions can redirect to Bowie; a mapping 255 is not a validated absence sentinel.
These are not harmless no-ops. This scene provides a Peter fallback but no A/B
fallback, and does not re-resolve story identity independently of those lookups.

`getallymapsprite.asm:GetAllyMapsprite` under the entity consumer root uses the
HP-zero blue-flame branch only when CURRENT_BATTLE=255. Here it remains 21,
so Bowie/Peter use class-sensitive mapsprites. Declaration is not revival;
victory healing is the separate earlier operation. Position/facing helpers
check HP for true allies 0–29, skipping zero-HP Bowie/Peter; IDs 30/31 bypass it.
Actions, text and nod do not inherit that HP guard. Ordinary coherent victory
has living Bowie and heals Peter, but a direct/supplied zero-HP or changed state
must not be described as automatically normalized by the script.

| Commands | Complete ordered program |
| --- | --- |
| 0–2 | Cursor 2583; load Map67 with camera `(4,18)`; rebuild ce_4B948. |
| 3–9 | Install/wait Bowie Init, Peter Init; set Peter `(11,23)` Left; install/wait B Init; set B `(11,21)` Left, A `(11,22)` Left; fadeInB. There is no A Init/wait. |
| 10–13 | Custom B body: ac_setSpeed 28,28; ac_jump eas_Idle; ac_end. Wait for idle; csWait 5; start B eas_2xRightLeft without an idle wait; csWait 120. |
| 14–16 | B inline moveLeft 3, moveDown 1, moveLeft 1, endActions, waiting for idle; csWait 40; face B Right. |
| 17–22 | Six nextSingleText calls: B 2583, Peter 2584, B 2585, Peter 2586, B 2587, Peter 2588. B uses upper byte `$0`; Peter `$C0`. |
| 23–27 | Peter inline moveLeft 1/endActions with idle wait; Peter Up; Peter single 2589 with `$C0`; Bowie Down; nod Bowie. |
| 28–32 | Follow A→Bowie relative 2; Peter→A relative 1; B→A relative 3; warp Map67 `(26,23)` Down; csc_end. |

Under consistent mapping, admitted placements and completed moves without
intervening displacement, B requests `(11,21)`→`(8,21)`→`(8,22)`→`(7,22)`;
Peter requests `(11,23)`→`(10,23)`. A retains its supplied follower action while
setPos changes its position/destination; the script never freezes or waits for
A. Later following may move the cast again. These coordinates are conditional
requests, not guaranteed fixed end positions.

## Text and action services

**Confirmed:** the complete literal join is `data/scripting/text/gamescript.txt`
keys `0A17`–`0A1D`, not just scene comments. Every line uses nextSingleText and
ends `{W1}`; there is no choice or explicit recruitment command.

| Decimal / hex | Speaker operand | Literal text and controls |
| --- | --- | --- |
| 2583 / 0A17 | `$0`, B | `Hurry!  Let's go into{N}Creed's mansion.{W1}` |
| 2584 / 0A18 | `$C0`, Peter | `Oddler, can you see?{W1}` |
| 2585 / 0A19 | `$0`, B | `No, why?{W1}` |
| 2586 / 0A1A | `$C0`, Peter | `You're moving around as{N}if you can see.{W1}` |
| 2587 / 0A1B | `$0`, B | `Am I?  Maybe my other{N}senses have sharpened{N}since I lost my sight.{W1}` |
| 2588 / 0A1C | `$C0`, Peter | `Oh, I see.{W1}` |
| 2589 / 0A1D | `$C0`, Peter | `{LEADER}, let's go see{N}Mr. Creed!{W1}` |

B's sight claim and Peter's attribution remain dialogue, not independent
physiological/lore findings. `{LEADER}` rebuilds the force lists and resolves the
first active member's name, not an unconditional Bowie literal. Peter is a
logical ally lookup; B is the current follower lookup, even if its sprite is
not Oddler. `getentityportaitandspeechsfx.asm` resolves portrait/speech from the
mapped actor's current mapsprite, with its own table fallback.

`mapscriptengine_2.asm:csc00_displaySingleTextbox` shows portrait, selects speech,
waits for scrolling, displays the cursor text and increments it, then closes
portrait/text and sleeps 10. `mapscriptengine_1.asm:csc1D_showPortrait` tests bits 15
and 14 of the packed word; `$C0` sets both. Through
`code/common/menus/portraitwindow.asm:OpenPortraitWindow` they become right-side
and mirrored toggles respectively. `$0` sets neither. An already open portrait
can bypass opening; a missing portrait result skips it. Logical speaker and
modifier alone do not prove a particular rendered portrait.

The complete selected action services are:

- `data/scripting/entity/eas_actions.asm:eas_Init`: zero acceleration factors,
  speed 32/32, acceleration/deceleration off, obstructable/collidable settings,
  autofacing on, orientation Up, immersion off, size 24, sprite update, idle jump.
  Orientation and facing are distinct fields. `eas_2xRightLeft` sets and refreshes
  Right/Left/Right/Left with wait 30 after each, then jumps idle. It does not move
  the actor or restore its speed.
- csc14 installs B's custom action pointer and polls eas_Idle, then scans through
  ac_end's `$8080`. `sf2cutscenemacros.asm:ac_setSpeed` packs X/Y bytes 28/28;
  `entityscriptengine_2.asm:esc10_setSpeed` writes their word at XSPEED. No later
  Init intervenes before B's inline movement. csc15 installs the named action
  and optionally waits for precisely eas_Idle. Neither helper checks HP.
- csWait 5/120/40 are Sleep operands, not action completion barriers. In particular
  wait 120 does not independently prove four look waits have finished before
  csc2D replaces B's action. csc2D clears action flags 5/6, compiles relative
  movement plus destination waits, appends an idle jump and optionally waits for
  that actor to reach idle. esc04 multiplies movement tiles by 384; with flag 5
  cleared, HasSameDestinationAsOtherEntity bypasses its destination conflict
  scan. These calls do not dispatch ordinary player zone/warp events or prove
  natural terrain traversal.
- setPos writes current and destination tile×384/facing and refreshes; setFacing
  writes facing and refreshes, subject to the ally-HP guard above. csc26 nod
  stops animation, sleeps 10, transforms/DMAs the sprite, sleeps 20, refreshes,
  sleeps 10 and restores animation. It has no consent or membership operation.

ExecuteMapScript clears debug text skip on entry; debug plus P2 Start can set
it, suppressing these text displays/cursor increments and csWait sleeps. It does
not suppress the custom/inline actions or warp. At csc_end, a nonzero dialogue
window index causes a scroll wait; camera speed is cleared and registers are
restored. This is not an all-actor completion barrier. Fade/display/VInt,
scroll, action, portrait/text and input services must return for later commands
to run. Earlier RAM writes are not rolled back on a stalled service.

## Temporary following and the return request

**Confirmed:** `sf2cutscenemacros.asm:followEntity` encodes follower/target/place
as words. csc2C first passes the **first byte** of the follower word to
AdjustScriptPointerByCharacterAliveStatus. For 30,7,31 that byte is 0, so these
three commands test **Bowie's HP**, not each named follower's. Zero-HP Bowie
skips the entire corresponding command. Otherwise physical lookups and
`table_FollowerPositions` supply:

| Order | Logical relation | Relative table operand |
| --- | --- | --- |
| 1 | A 30 follows Bowie 0 | 2 → `(-24,0)` |
| 2 | Peter 7 follows A 30 | 1 → `(0,-24)` |
| 3 | B 31 follows A 30 | 3 → `(0,24)` |

`entityfunctions_2.asm:AddFollower` installs a 42-byte copy of eas_Follower1 with
physical target and offset operands, and appends the physical follower to
EXPLORATION_ENTITIES only if absent. A fallback fixed Peter can be added here;
ordinary selected followers are already listed. No joined/active or persistent
follower-selection flag changes. With missing A, the first call can make Bowie
follow himself; no alias check rejects it. Calls are ordered and can replace
an earlier action if lookups alias the same actor.

The installed template in `data/scripting/entity/eas_main.asm` sets speed 32/32,
zero acceleration, acceleration/deceleration/obstructability off and uncollidable
bits on, then loops follow/destination wait. Thus B's custom speed 28 is replaced
when that template executes; installation alone is not proof it ran before the
map transition. esc03_follow still uses target-relative distance, facing,
destination and layout: separation at most `$1E0` retains the destination;
otherwise it tries the rotated offset, zero-lateral fallback, then the target
destination if blocked. These relative operands are not absolute landing tiles.
No follow call waits for settling.

`mapscriptengine_2.asm:csc07_warp` writes MAP_EVENT_TYPE=1, parameter1=0, then
literal Map67/X26/Y23/Down bytes to parameters2–5. It overwrites the shared slot;
there is no queue or previous-request preservation. It does not fade/load the
map, clear CURRENT_BATTLE, set F521 or end the script by itself. csc_end returns
to the after wrapper. The victory seed had set parameter1=0 without clearing
MAP_EVENT_TYPE; repeat victory need not produce a type1 event at all.

After full scene return, or the F521 bypass, EndAfterBattleCutscene calls
JoinForce(0). `code/common/stats/battleparty.asm` sets F0, rebuilds force lists,
and sets active F32 only when the resulting active count is below 12. A full
party with F32 clear leaves Bowie reserve; supplied F32 is not cleared at or
above capacity. No duplicate list entry is appended, and no member is displaced.
JoinBattleParty does not rebuild the lists again immediately. This common call
has no join music/text and does not heal or alter classes/equipment/positions.
There is no explicit Peter/B recruitment in the after program.

BattleLoop then clears F421, sets F521, clears d0–d3 words and reads parameters2–5
into map/X/Y/facing, returning d4=1. F471 is unchanged. The result consumes the
parameters currently in the shared slot, not an immutable copy of the warp or
a type-validation step. A later replacement before this read can change the
returned tuple. A stall before the warp leaves the seed or another request;
a stall after the warp can leave a pending request without completion flags or
a returned battle result. No partial state is a guaranteed completed return.

## Fresh Map67 installation and stopping boundary

**Confirmed:** `code/gameflow/mainloop.asm:alt_MainLoopEntry` calls SwitchMap
then ExplorationLoop **without CheckBattle**. The complete flag-switched table
has no Map67 row, so a returned Map67 tuple keeps its coordinates/facing.
It selects fresh exploration even when CURRENT_MAP was already 67; numeric map
equality does not preserve the after-scene cast.

`code/gameflow/exploration/explorationfunctions_2.asm:ExplorationLoop`:

1. Clears MAP_EVENT_TYPE, subtracts 20000 from STEP_COUNTER with a negative-result
   clamp, heals eligible allies, starts fade and disables view following.
2. For provided Map67, writes CURRENT_MAP=67 and CURRENT_BATTLE=255, loads
   tilesets/waits fading, clears VInts, selects setup entities and initializes
   them using the returned X/Y/facing; loads their mapsprites.
3. Clears temporary flags 256–383, sets F80, initializes sprite-frame state,
   selects view target 0, loads the map/view, installs base VInts and calls the
   selected map init. The full-map load no longer crops by battle area.
4. Conditionally plays map music/fades according to palette comparison, then
   reaches WaitForEvent. Pending zero follows physical 0 and installs the
   current-mode controlled action; nonzero pending takes precedence.

`j_GetMapSetupEntities` resolves to GetMapSetupEntityList; it and
RunMapSetupInitFunction use GetCurrentMapSetup in `mapsetupsfunctions_1.asm`.
The complete `data/maps/mapsetups.asm` Map67 entry has no flag alternate:
`ms_map67`. Its pointertable selects `ms_map67_Entities` (only msEntitiesEnd)
and `ms_map67_InitFunction` (only rts). Neither F471 nor F521 selects a new
population or another scene here. There is no Map67 init repair of missing
followers, no prompt or automatic mansion warp.

Fresh InitializeMapEntities clears the scene lookups/actions and rebuilds
followers at the returned coordinates from the current flags. It declares
Bowie at physical 0 with the returned facing and no fixed Peter fallback.
For only F65/F67/F68, physical 1/2/3 are Caravan/A, Peter, Oddler/B. Their supplied
sequence selects eas_Follower1, eas_OverworldFollower2 and eas_OverworldFollower3,
respectively; other flags change allocation and script assignment order.
These are newly selected templates, not the three temporary scene follow copies.
Absent F67 means Peter's scene fallback is gone even if he is a joined/active
force member. Absent F68 does not become Oddler following merely because his
name was spoken. Current class/HP still influence ally sprites; CURRENT_BATTLE=255
now allows the blue-flame branch for zero-HP allies.

BattleLoop initially cleared PLAYER_TYPE to 0; neither this scene nor the return
path changes it. For that coherent path, control uses eas_ControlledCharacter.
F64 and RAFT_MAP/X/Y are not written by victory or the warp. The population tail
`followersfunctions_2.asm:sub_44404` can declare separate raft physical 31 if F64
is set and the saved raft map matches 67, masking its coordinates with `$7F`.
It does not board it, add the battle origin or repair the saved raft position.
Independently supplied mode 2 instead uses the existing raft-main/standing-follower
tail. SetControlledEntityActScript selects mode 0 walking, mode 1 eas_Raft (actual
Caravan), or other-mode eas_Caravan (actual raft); arbitrary mode is not validated.

| Changed/supplied state | Discriminating consequence |
| --- | --- |
| F471 clear, F521 clear | After scene still runs. F471 is not its gate and is not set by this outcome; Map67 init is still rts. |
| F521 set, regardless of F471 | Scene/population/movement/text/follow/warp all skipped. Pre-scene active-member seed survives absent replacement; common join and completion writes still run. |
| Active membership, combatant position or facing changed | Changes the seed; full completed scene overrides its coordinates/facing with the warp, repeat does not. Empty/stale active list is not repaired. |
| F65/F67/F68 missing or other follower flags set | A/B may alias/resolve elsewhere; Peter has a scene fallback only. Fresh return rebuilds from flags, discarding temporary follows and custom speed. |
| Dead/mismapped actors, supplied invalid geometry, unfinished service | HP guards, aliasing, crop and wait semantics remain operative; no promised scene completion/control. |
| Request overwritten before battle-result read | Latest parameter bytes determine returned tuple; the intended warp is not an immutable itinerary. |
| New pending event after exploration's entry reset | WaitForEvent gives it priority over controller installation. Map67's rts init itself creates none. |

F421/F471/F521, joined/active and follower flags are outside the temporary reset.
Their surviving RAM values are not a save-persistence proof. A supplied map 255
return takes exploration's existing-population branch instead: no fresh
CURRENT_BATTLE assignment, entity setup or temporary reset; it updates main
properties/raft and loads/init through that branch. It is not the normal Map67
victory return. Arbitrary replacement destinations require their own consumers.

The next independent boundary is `data/maps/entries/map67/6-warp-events.asm`:
the doorway at `(26,22)` has warpNoScroll and requests MAP_CREED_MANSION (33),
`(15,30)`, Up. The after scene arrives at `(26,23)`, Down. Arrival does not itself
produce that separate doorway event; adjacency is not natural passability,
player input, mansion entry or Battle22 admission. Stop here before mansion
setup/init or any subsequent route.

## Defeat/escape continuation and verification limits

**Confirmed:** `battleloop/getegresspositionforbattle.asm` has no Battle21 special
case: use supplied EGRESS_MAP. `code/common/maps/egressinit.asm:GetSavepointForMap`
uses F399 (set at BattleLoop entry) and the matching
`data/maps/global/savepointmapcoords.asm` row. No match preserves the supplied
map and defaults to `(1,1)`, Up. A direct F399-clear helper call instead returns
initial Map3 `(56,3)`, Down before scanning savepoint/raft tables.

With F64 clear, saved raft coordinates remain unchanged. With it set, the complete
`raftresetmapcoords.asm` selects Hassan (9)→Map69 `(28,55)`, Polca (13)/Bedoe (10)→Map72
`(43,48)`, otherwise default→Map69 `(42,15)`. These writes do not board the raft.
MainLoop's alternate entry still skips battle admission and applies SwitchMap:
the complete switch table maps 3→4 under F640, 71→78 under F530, 58→59 under F999,
preserving coordinates/facing. Stop at that supplied destination's continuation;
no claim that defeat/escape returns to Devil's Tail or a particular last savepoint.

Reproduction loads `. ./local/private-inputs.ps1` in the same process as controlled
commands and verifies the assigned source pin/clean status. The unchanged
[retained comparison recipe](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary)
compares map-init equality, `_verify_map_script_engine_fixture` and source
`_cutscene_facts` equality. Use named paths/symbols/slots above, actual aliases and
macro/enum operands for direct joins; strip comments for instruction comparison.

| Evidence surface | Selected review |
| --- | --- |
| Retained `local/derived/map-script-engine-static.json`; `tests/fixtures/h2/map-script-engine-static-v1.json` | Exact source-line/macro/argument join of all 33 abcs_battle21 commands; complete custom and two inline bodies, ce_4B948 and literal 2583–2589 directly. |
| `tests/fixtures/h2/battle-cutscenes-static-v1.json` | Shared source equality plus actual slot 21 leader/defeated/after/join, region absence and their immediate callers. |
| `tests/fixtures/h2/battle-loop-static-v1.json`, `common-stats-static-v1.json` | Accepted lifecycle/membership context plus direct leader append, ordered counts, healing, active-member seed, common join, Egress and suspension consumers. |
| Retained `local/derived/map-init-static.json`; `tests/fixtures/h2/map-init-static-v1.json` | Shared setup/follower equality plus direct crop/full-map lifetime, scene versus fresh population, complete Map67 setup/init and doorway request, pending/control and mode/raft consumers. |

These comparisons do not automatically guard every prose claim; complete selected
source/consumer review supplies the joins. Acceptance uses the entire owned diff,
provenance/links/private boundary, `git diff --check`, clean committed configured
`uv run sf2 verify plan --base origin/main --head HEAD` and actual final-head CI.
No blanket local normal/full/engine/Godot/H3/H4 suite is selected solely for prose.
Exact Git/CI/check identities and inspection failures belong in Issue888's handoff.
Preserve [Issue884's original handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/884#issuecomment-6172444717),
[rebased receipts](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/884#issuecomment-6172589433)
and [PR886 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/886#issuecomment-6172605301),
including their linked failures and unchanged retained evidence.

**Unknown:** natural outcome/reach, actual party/HP/flags/class/lookup/geometry,
mode/raft/savepoint, completed action/text/input/fade/scroll services, rendered
scene, usable control and persistence. The changed-state rows are source
counterexamples, not executed scenarios. They do not create a runtime queue
under [ADR0014](../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../decisions/0016-remake-start-evidence-deferral.md).
Leave a clean frozen Draft PR for independent review; no integration or cleanup.
