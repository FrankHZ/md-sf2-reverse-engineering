# Battle20 outcomes and Elric recruitment

Evidence date: 2026-10-14. **Confirmed:** Battle20 has no leader-death shortcut
and selects the Elric scene only while F520 is clear. The complete scene joins
ally 13 and conditionally installs three follower actions. Victory subsequently
clears F420, sets F520 and rebuilds Map51 from the saved pre-scene return tuple.
That rebuild removes the temporary Elric scene actor: force membership and
exploration following are separate state. **Inferred:** the conversation presents
Elric's rescue and voluntary offer to help. **Unknown:** natural caller state,
completed services, rendered presentation, responsive input and save persistence.

## Agreement and provenance

Standard rigor under [Issue #872](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/872)
and [content epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
This document is the sole owned path, with accepted starting base
`d715de24f91594c488e7a08d77e3feba84ba0db5`. It consumes the accepted
[Battle20 entry](./harpy-pond-approach-through-battle20-entry.md),
[pond population/init](./elven-village-departure-through-harpy-pond-entry.md),
[battle lifecycle](./battle-loop.md), [cutscene routing](./battle-cutscenes.md),
[map request consumption](./map-event-request-consumption.md) and
[preceding outcome context](./battle19-outcomes-through-elven-village-entry.md).
There are no shared edits, stacked dependencies, new fixtures or verification
machinery. The outcome is a complete bounded static account for the content guide.

Stop at immediate conditional Map51 exploration control, or the selected unrelated
savepoint continuation. Exclude individual combat/AI/damage simulation, subsequent
pond departure, F751 passage, Fairy Cave/Creed travel, Battle21 content, terrain
reconstruction and runtime acquisition. The scene's final destination dialogue
is included; it does not authorize investigation of that destination.

All source paths below are relative to `disasm/` in
[SF2DISASM master at `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The assigned read-only checkout is clean at that pin. Retained evidence binds
USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`;
no fresh ROM extraction or runtime observation is claimed. Fnnn denotes flag nnn;
slots and command indices are zero-based. Coordinates, waits and actor actions
are source operands, not observed elapsed time or movement.
Short battle paths start at `code/gameflow/battle/`; map-script consumers at
`code/common/scripting/map/`; Map51 setup filenames at
`data/maps/entries/map51/mapsetups/`. Entity consumers are under
`code/common/scripting/entity/`; their eas_Init/2xRightLeft and follower templates
are in `data/scripting/entity/eas_actions.asm` and `eas_main.asm` respectively.

Reuse the existing 18,285,979-byte map-init/map-script-engine pair, selected
fixtures and bounded source. Linear comparisons were budgeted below 512 MiB and
one minute plus small receipts/prose, with no regeneration; the pair/source
comparison passed in 0.112 seconds. Reassess material scope or resource growth.

## Actual outcome selection and end checks

**Confirmed:** the pointer and consumer joins are:

| Owning source | Battle20 selection and gate |
| --- | --- |
| `data/battles/cutscenes/battleendcutscenes.asm`, slot 20 | `ms_Empty`, whose complete body is the terminator in `code/common/scripting/map/ms_empty.asm`. The defeated wrapper requires Bowie HP nonzero and enemy128 HP zero; F520 clear dispatches, set skips the script. |
| `data/battles/global/enemyleaderpresence.asm`, row 20 | 0. Both eligible defeated-wrapper paths reach the common tail, which returns without appending remaining enemies to the killed list. Killing slot 128 alone is not Battle20 victory. |
| `data/battles/cutscenes/regionactivatedcutscenes.asm` | Complete table contains only battles32/40. ExecuteBattleRegionCutscene scans to the terminator for 20 and runs no region scene. This is not an empty indexed pointer or proof about every AI region operation. |
| `data/battles/cutscenes/afterbattlecutscenes.asm`, slot 20 | `abcs_battle20` at `0x4B6CE`. ExecuteAfterBattleCutscene tests F520; clear runs the complete program, set bypasses it. Both reach EndAfterBattleCutscene. |
| `data/battles/cutscenes/afterbattlejoins.asm`, slot 20 | 0, consumed by the common after tail through j_JoinForce. This is Bowie, separate from the scene's explicit Elric 13 join. |

The actual s07 layout `layout/sf2-07-0x044000-0x064000.asm` includes the cutscenes
join table after `afterbattlecutscenesend.asm`. The similarly named
`data/battles/global/afterbattlejoins.asm` is not this consumer's table. Its
unused-feature comment does not erase the executed join. Wrapper bodies are in
`code/gameflow/battle/cutscenes/{battleendcutscenesstart,battleendcutscenesend,afterbattlecutscenesstart,afterbattlecutscenesend,regionactivatedcutscenes}.asm`.
Jump interfaces resolve j_JoinForce to `code/common/stats/battleparty.asm:JoinForce`.

`battleloop_1.asm` calls the defeated hook after an individual turn, then
ProcessKilledCombatants and CountRemainingCombatants. Zero allies branches to
defeat before zero enemies can branch to victory. If neither ends the battle,
ProcessAfterTurnEffects runs, followed by another killed pass and the same
ordered count checks; no second defeated-hook invocation occurs there.
`battleloop/countremainingcombatants.asm` scans 30 ally slots and 32 enemy slots,
counts only nonnegative X with nonzero HP, and forces the ally count to zero
when Bowie's HP is zero. Simultaneous zero counts therefore mean defeat.
The neutral Elric 159 described by the entry owner has HP0 and does not add a
living-enemy count merely because its entity is present.

The shared killed consumer performs its presentation/bookkeeping, sets listed
combatant X/Y=-1, clears status, refreshes stats and moves mapped actors off-map.
Battle20's leader row contributes no additional victims. The separate
ApplyPositionsAfterEnemyLeaderDies/table_AfterBattlePositions selects only
Battle05; no Battle20 staged position replaces the victory seed. These shared
rules do not establish a particular sequence of kills or an observed victory.

| Outcome | Selected ordering |
| --- | --- |
| First victory, F520 clear | Heal eligible allies; save return tuple; run Elric scene; common Bowie join; clear F420; set F520; read saved tuple; return d4=1. |
| Repeated victory, F520 set | Same healing/seed/common join/completion writes and d4=1; bypass the entire Elric program, including join13 and its follower operations. |
| Defeat | Update unlock; exhaustion music/text; restore Bowie's HP; halve gold; select savepoint; return d4=-1. Battle04's special losable branch does not apply. |
| Admitted Egress / Angel Wing | Consume spell MP / remove selected item slot; text/flash/closure; update unlock; unwind the action frame; select savepoint; return d4=0. No victory join/completion or defeat gold penalty. |

UpdateBattleUnlockedFlag in `battlefunctions/battlefunctions_1.asm` preserves
F420 when F520 is clear and clears it when F520 is set. Neither loss nor escape
sets F520, resets F470 or recruits Elric. The action-type and masked spell/item
checks in `battlefunctions/executeindividualturn.asm` establish the admitted
escape consumer, not menu availability, MP sufficiency or item custody.

## Victory seed precedes every scene placement

**Confirmed:** `code/gameflow/battle/battleloop_2.asm:BattleLoop_Victory` first calls
HealLivingAndImmortalAllies. That helper scans all 30 ally slots, skips HP-zero
allies except Peter 7/Lemon 28, restores HP/MP to maxima, preserves only
STUN/POISON/CURSE and rebuilds stats. It does not require joined membership and
is not a join operation. Dead Elric is not revived by it.

The next writes are parameter2=CURRENT_MAP, parameters3/4=the first current
active member's combatant X/Y plus BATTLE_AREA_X/Y using byte arithmetic,
parameter5=that member's mapped entity facing, then parameter1=0.
`code/common/scripting/text/textfunctions_1.asm:UpdateForceAndGetFirstBattlePartyMemberIndex`
rebuilds the flag-selected lists and reads their first active entry. It is not a
first-living search and does not repair an empty/stale first entry. Under
consistent Battle20 admission, the saved map is 51 and both area offsets are 0.
Do not substitute the approach warp `(0,0)`, a roster's starting position, or
later scene main `(6,10)`.

Only after this seed does the after wrapper test F520. The selected complete
scene has no warp, return-parameter write, CURRENT_BATTLE clear or mode change.
Its map loader writes CURRENT_MAP=51, but does not replace the saved tuple or
convert the caller into ExplorationLoop. The common join and F420/F520 writes
occur after ExecuteMapScript returns. Thus all scene positions/facings remain
separate from the tuple ultimately returned by BattleLoop.

## Complete selected Elric program and replacement population

**Confirmed:** `data/battles/entries/battle20/cs_afterbattle.asm`, lines 4–41,
contains all 36 top-level records of abcs_battle20, including one complete
inline move. ce_4B780, lines 42–44, is main `(6,10)`, Up; one fixed Elric 13 row
`(6,8)`, Down, eas_Init; then the entity terminator. B below is FOLLOWER_B31.

| Records | Ordered content |
| --- | --- |
| 0–2 | Cursor 2566; loadMapFadeIn Map51 with camera `(2,6)`; loadMapEntities ce_4B780. |
| 3–8 | Install/wait Bowie Init, Peter Init, B Init; position Peter `(5,10)`, Up and B `(7,10)`, Up; fadeInB. |
| 9–12 | Elric nextText 2566, single 2567; Peter nextText 2568, single 2569. |
| 13–19 | Peter Right; csWait 5; start B 2xRightLeft; csWait 120; B Up; B single 2570; Peter Up. |
| 20–24 | Elric nextText 2571, single 2572; Peter Right; Bowie Left; Peter single 2573. |
| 25–29 | Elric inline moveDown 1/endActions, waiting for idle; Elric single 2574; Bowie Up; nod Bowie; join Elric 13. |
| 30–35 | Peter Up; Peter single 2575; Peter follows Bowie, B follows Peter, Elric follows B, each relative 2; end. |

`sf2cutscenemacros.asm:loadMapFadeIn` emits opcode$37 with map/camera words.
`mapscriptengine_1.asm:csc37_loadMapAndFadeIn` starts OUT_TO_BLACK and falls into
csc48_loadMap: disable entity camera following, load tilesets, wait through
VInts for the fade, deactivate VInts, pack camera operands and call LoadMap,
then enable display/interrupts and reactivate VInts. The operand `(2,6)` is a
camera input, not Bowie's tile. `code/common/maps/mapload.asm:LoadMap` selects
map data/layout/area/palettes and projection; it writes CURRENT_MAP for this
explicit map but does not initialize the scene population or set battle255.
The later opcode$39/fadeInB calls FadeInFromBlack explicitly.

csc42_loadMapEntities deactivates VInts/display, reads the three main-entity
words, calls InitializeMapEntities with the remaining row pointer, loads entity
mapsprites, then reenables display/VInts. It does not select Map51's default or
F520 fixed rows. `code/common/scripting/map/mapfunctions.asm:InitializeMapEntities`
clears entity state, logical lookups and walking allocation, declares current
flag-selected followers starting at physical 1, declares fixed rows, then declares
Bowie at physical 0 and terminates EXPLORATION_ENTITIES. Its raft tail still applies.

For the ordinary supplied F67/F68-only follower example, the new lookups are
Peter 7→physical 1, B 31/Oddler→2, Elric 13→3 and Bowie 0→0. Other follower flags can
change physical indices or overwrite B's binding; the script does not test that
B really depicts Oddler. The normal follower table has no Elric row, so this
fixed Elric is newly declared independently of F13/F45. The common fixed-ally
consumer skips a row whose logical lookup is already populated. It chooses
sprites through GetAllyMapsprite; CURRENT_BATTLE remains 20, so its exploration
HP-zero blue-flame branch is not taken here, although class still matters.

The old neutral battle entity159 is cleared with the battle population. Nothing
converts that neutral into ally 13 or copies its HP/status into Elric's ally slot.
The scene is a replacement cast, not continuity of the neutral's physical actor.
The three parked exploration monsters from Map51's default setup are also not
rows of ce_4B780. No joined-flag test is required to declare the scene's Elric.

## Dialogue, actions and service boundaries

**Confirmed:** the complete literal join is in `data/scripting/text/gamescript.txt`.
The table paraphrases content, preserving controls and order rather than copying
a full transcript. Portrait/speech resolution uses the current mapped mapsprite;
`$C0` selects mirrored/right portrait bits and `$0` selects neither.

| Decimal / hex | Speaker / modifier | Content and controls |
| --- | --- | --- |
| 2566 / `0A06` | Elric / `$0` | Introduces himself as an archer and says he did not know the pond was dangerous; `{NAME;13}`, two `{N}`, `{W2}`. |
| 2567 / `0A07` | Elric / `$0` | Thanks them for saving his life; `{N}`, `{W1}`. |
| 2568 / `0A08` | Peter / `$C0` | Credits Oddler; `{W2}`. |
| 2569 / `0A09` | Peter / `$C0` | Says Oddler heard the cry and brought them; `{N}`, `{W1}`. |
| 2570 / `0A0A` | B / `$C0` | Modestly dismisses the thanks; `{W1}`. |
| 2571 / `0A0B` | Elric / `$0` | Says an elf repays a favor; `{N}`, `{W2}`. |
| 2572 / `0A0C` | Elric / `$0` | Offers to help; `{W1}`. |
| 2573 / `0A0D` | Peter / `$C0` | Tells the leader they have another friend; `{LEADER}`, `{N}`, `{W1}`. |
| 2574 / `0A0E` | Elric / `$0` | Expresses thanks; `{W1}`. |
| 446 / `01BE` | Generic join service | Announces the named character and class joining; `{NAME}`, `{CLASS}`, `{N}`; no literal `{W1}`/`{W2}`. Separate music/input service follows. |
| 2575 / `0A0F` | Peter / `$C0` | Proposes going to see Mr. Creed; `{W1}`. |

The generic join text is inserted by opcode$08 after 2574 and does not consume
the cutscene cursor;2575 remains the next scene text. `{LEADER}` rebuilds force
lists and resolves the first active member's name, not an unconditional Bowie
literal. Peter's attribution and Elric's statements remain attributed dialogue,
not independent evidence of a natural route or the missing people's history.
There is no YesNoPrompt, choice result or refusal branch. The nod is an action;
join-service input advances presentation, not a recruitment decision.

Immediate consumers in `code/common/scripting/map/mapscriptengine_1.asm` and
`_2.asm` retain these limits:

- csc15 replaces the action/timer and optionally polls for exactly eas_Idle.
  Init sets speed 32/32, zero acceleration factors, disables acceleration and
  deceleration, enables obstructability/collision/autofacing, orients Up,
  disables immersion, sets size 24, refreshes and idles. The scene explicitly
  waits Bowie/Peter/B Init; Elric's table Init is not separately joined by such
  a record. B's 2xRightLeft refreshes/waits30 at Right/Left/Right/Left, then
  idles. csWait 120 is a separate Sleep request, not an action-pointer barrier.
- csc19 positions current/destination at tile×384 and refreshes; csc23 changes
  facing and refreshes. Their alive helper skips HP-zero allies 0–29. Peter and
  Bowie's position/facing operations are conditional on their own HP; B 31
  bypasses that gate. The helper does not establish membership or valid lookup.
- csc2D clears action flags 5/6 and builds relative-motion opcode 4, a destination
  wait and final idle jump. The selected Down 1 is relative to Elric's current
  actor position and has no HP gate. Under consistent population, completed
  Init/motion and no intervening displacement it moves `(6,8)` to `(6,9)`.
  With flag 5 cleared, HasSameDestinationAsOtherEntity bypasses the destination
  conflict scan. This path does not itself submit an ordinary walking zone/warp
  event or guarantee an elapsed duration. Its idle wait is for this actor only.
- csc26 nod stops animation, sleeps 10, loads/transforms/DMAs the sprite, sleeps 20,
  refreshes, sleeps 10 and restores animation. It has no HP/consent test.
- Selected nextText/singleText wait for scrolling and resolve current actor
  portrait/speech; display advances the cursor. Single text also closes windows
  and sleeps 10. Script entry clears debug text skip; debug plus player 2 Start
  can set it, suppressing these text displays/cursor increments and csWait Sleep.
  The generic opcode$08 join has its own unconditional service path.

Missing lookups cleared to 0 can alias physical 0 through
GetEntityAddressFromCharacter; a value 255 is not validated as a safe absence
sentinel. Actions/text/nod do not inherit the position/facing HP check. Absent
Peter or B can therefore redirect operations despite the successful creation of
Elric. Supplied dead/mismapped actors, ongoing actions and unfinished services
remain meaningful counterexamples; the scene does not normalize them all.

## Membership and the three temporary follow commands

**Confirmed:** csc08 clears speech SFX, waits for view scroll, reads 13, selects
normal MUSIC_JOIN (no sad bit), and takes the ordinary single-ally branch.
JoinForce sets joined F13, calls UpdateForce, compares active count with
FORCE_MAX_SIZE=12, and only if below 12 calls JoinBattleParty to set F45 (=32+13).
UpdateForce scans joined flags in ascending ally order 0–29, separates active and
reserve by the active flags, and writes counts. JoinBattleParty sets the flag;
it does not rebuild those lists again in that call. Later UpdateForce consumers
reflect that activation. No duplicate member is appended on a repeated join.

With a full active party and F45 clear, Elric remains a joined reserve; no
member is displaced. Supplied F45 remains set even at/above capacity; JoinForce
does not cap or repair an already inconsistent roster. An already joined reserve
can become active if room exists. F13 supplied before the first scene does not
skip its dialogue, music or join. Neither JoinForce nor csc08 restores HP/MP,
class, status, equipment or combatant coordinates. Dead Elric can be joined
without being revived. The surrounding lifecycle's eligible healing is separate.

After JoinForce, csc08 reads Elric's current class, fills dialogue name/class
slots and displays 446, then calls FadeOut_WaitForP1Input, closes text and sleeps 10.
Despite its name, that helper in `battlefunctions/battlefunctions_0.asm` requests
previous music through PlayMusicAfterCurrentOne and calls WaitForPlayerInput;
it does not call a screen fade. `code/common/tech/sound/music.asm` submits the
wait-music/parameter commands and polls WAIT_FOR_MUSIC_END with Sleep 3.
`code/common/tech/input.asm:WaitForPlayerInput` accepts any nonzero masked current
input (directions, A/B/C or Start), waiting through VInts otherwise. It does not
require a new press/release edge or produce a yes/no answer.
The hardcoded Sarah/Chester combined case 128 and its text 447 are not selected.
These services can prevent later records from being reached after membership
has already changed. No rollback is present.

All three csc2C_followEntity calls test the first byte of the **word-encoded**
follower ID. For 7,31,13 this is 0, so the alive helper tests **Bowie**, not Peter,
B or Elric. HP-zero Bowie skips each entire command. Otherwise the calls map
follower/target, read relative 2 as `(-24,0)` from table_FollowerPositions and call
`entityfunctions_2.asm:AddFollower` in order: Peter→Bowie, B→Peter, Elric→B.

AddFollower installs a 42-byte eas_Follower1 copy with physical target and offset
operands, and adds the physical follower to EXPLORATION_ENTITIES only if absent.
For the ordinary cast, Peter/B are already listed and Elric's physical 3 is appended. It
writes no joined/active flag and no persistent follower-selection flag. The
source template sets speed 32/32, zero acceleration, disables acceleration,
deceleration and obstructability, enables uncollidable bits, and loops
follow/wait-destination. `entityscriptengine_2.asm:esc03_follow` still checks
layout and target-relative distance/destination. It keeps its destination while
maximum axis separation is at most $1E0; otherwise it tries the target-facing
offset, then a zero-lateral fallback, then the target destination if still blocked.
Relative 2 is not a promised world-coordinate endpoint. These commands do not
wait for the new following actions to settle before returning.

At csc_end, ExecuteMapScript conditionally waits for view scrolling when the
dialogue-window index is nonzero, clears camera scrolling speed and restores
registers. It is not an all-actor completion barrier. No scene record restores
camera following after loadMapFadeIn; later exploration supplies its own camera.
If the program stops in a service, earlier placements/membership remain while
later follow commands, common join and completion writes are not guaranteed.

## Complete return and conditional Map51 control

**Confirmed:** after full script return (or the F520 repeat bypass),
EndAfterBattleCutscene consumes actual join-table row 20=0 and calls JoinForce(0).
This sets Bowie's joined F0 and rebuilds the lists, including Elric's previously
set F45 if any; Bowie activates only if the resulting active count is below 12.
There is no generic join music/text in this wrapper. Its ordering is after the
pre-scene seed, not a correction to which member supplied that seed.

BattleLoop then clears F420, sets F520, reads the saved map/X/Y/facing and returns
d4=1. F470 is left as supplied. `code/gameflow/mainloop.asm:alt_MainLoopEntry`
calls SwitchMap and ExplorationLoop **without CheckBattle**. The complete
flag-switched table has no Map51 row; SwitchMap preserves coordinates/facing.
A normal saved map51 therefore selects the fresh-map branch, not map255's
existing-population branch. Same numeric map does not mean retained actors.

`code/gameflow/exploration/explorationfunctions_2.asm:ExplorationLoop` clears
pending, subtracts 20000 from STEP_COUNTER with a negative-result clamp, heals
eligible allies again, fades, writes CURRENT_MAP=51/CURRENT_BATTLE=255, loads
tilesets, obtains the current setup and initializes entities from the saved
return coordinates/facing. It then loads mapsprites, clears temporary flags
256–383, sets F80, loads the map/view, installs base VInts and calls map init.
F13/F45/F470/F520 and follower flags are outside that temporary reset.
The actual j_GetMapSetupEntities alias resolves to GetMapSetupEntityList,
which calls GetCurrentMapSetup and returns the selected first pointer.

`data/maps/mapsetups.asm` and GetCurrentMapSetup select ms_map51_flag520 now that
F520 is set. Its `s1_entities_520.asm` is empty. Both default/alternate pointer
tables select the same ms_map51_InitFunction. Fresh InitializeMapEntities clears
lookups/actions and rewrites the follower list from
`data/scripting/entity/followers.asm`; this complete table has no Elric row.
Thus neither the scene's Elric actor nor its new follower action/list entry
survives this fresh population. F13/F45 persist as membership; being active is
not a rule to spawn every force member as an exploration follower.

Peter/F67 and B/Oddler/F68 are rebuilt if those flags remain set. Other supplied
follower rows retain their ordering/possible B overwrite. F65 does not add the
Caravan on Map51 because IsOverworldMap excludes it. BattleLoop entry set
PLAYER_TYPE=0 and F399; this scene changes neither. F64 and saved raft map/X/Y
are unchanged by victory; the population tail can declare a separate raft if
F64 set and saved map51 matches. That is not boarding. Independently supplied
mode 2 takes the existing raft population behavior; later control still selects
mode 0 walking, mode 1 eas_Raft (actual Caravan), or the other-mode eas_Caravan
(actual raft) branch. No selected operation validates arbitrary modes.

| Supplied state at return | Immediate consequence |
| --- | --- |
| F470 set, ordinary first victory | F520 selects empty fixed rows; init returns without a scene. Saved pre-scene position/facing are retained through setup, subject to its ordinary services. Elric remains joined, not an exploration follower. |
| F470 clear, first or repeat victory | The same F520-empty population still runs cs_5C3D8. It repositions the cast and calls the earlier pond-help text despite absent fixed Elric. F520 does not gate that prompt; F470 is not repaired. |
| F520 already set on victory entry | No Elric scene or join13/follow13 occurs; common Bowie join and fresh population still occur. Supplied F13 is preserved, missing F13 is not repaired. |
| Full party at explicit join13 | Joined/reserve result is independent of the temporary scene follow; subsequent fresh setup omits Elric in either active/reserve case. |
| Missing/mismapped followers, dead actors or incomplete services | Position/facing gates and lookup aliasing still apply. A returned call is conditional on all required waits; earlier writes do not prove later completion or usable control. |

The F470-clear continuation is the complete already accepted
[pond init](./elven-village-departure-through-harpy-pond-entry.md): B/Peter Init;
Bowie `(11,22)`, B `(10,22)`, Peter `(9,22)`, all Up; fade; B Up 2, wait/look/wait,
face Up and text 2550; Peter Up 2/text 2551; camera `(2,2)`, wait 50; Elric 2552 and
Peter 2553–2554; camera follows Bowie; Peter follows Bowie and B follows Peter;
end. It writes no F470 or new Elric membership/actor. Under consistent actors
and completed moves B/Peter reach `(10,20)`/`(9,20)` before following; its
placements can supersede the saved return position. Missing Elric lookup0 can
resolve that help line through Bowie. Do not call this a guaranteed post-rescue
presentation or silently assume this supplied state is naturally reachable.

After init, exploration conditionally submits map music/fade. Pending zero lets
WaitForEvent follow physical 0 and install the current-mode controller; a pending
request takes precedence. This is the conditional Map51 control boundary, not
an observed playable pause. Stop before later zone/warp/departure producers.

## Bounded defeat and escape return

**Confirmed:** `battleloop/getegresspositionforbattle.asm` has no Battle20 special
case and uses supplied EGRESS_MAP. GetSavepointForMap in `code/common/maps/egressinit.asm`
uses F399 (set at normal BattleLoop entry), then the matching
`data/maps/global/savepointmapcoords.asm` row; no match retains the supplied map
with `(1,1)`, Up. A direct abnormal F399-clear helper call returns the initial
Map3 `(56,3)`, Down before savepoint or raft scanning.

With F64 clear, saved raft position is unchanged. With it set,
`raftresetmapcoords.asm` selects Hassan9→Map69 `(28,55)`, Polca13/Bedoe10→Map72
`(43,48)`, otherwise sentinel default→Map69 `(42,15)`. These writes do not board
or change PLAYER_TYPE. Supplied EGRESS_MAP23, for example, returns Elven Village
`(61,4)`, Up with the default raft reset if enabled; it is not proof of the last
savepoint. Stop before that map's unrelated setup/content.

MainLoop again calls SwitchMap then ExplorationLoop without battle admission.
The complete switch table substitutes Map3 with earthquake Map4 if F640,
Map71 with Map78 if F530, and Map58 with Map59 if F999; only a matching source/flag
applies, preserving return coordinates. This existing exception context does
not establish arbitrary savepoint validity or a natural loss itinerary.
Supplied map255 uses exploration's existing-population path, updating main
properties/raft and loading/init without fresh setup or temporary-flag clearing;
it is not the normal Map51 victory path. Later map-specific initialization can
change presentation/requests, so no universal responsive-control claim follows.

## Reproduction and remaining limits

Load `. ./local/private-inputs.ps1` in the same process as controlled commands.
Check the assigned read-only source pin/clean status. Reproduce the unchanged
[retained comparison recipe](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary):
map-init equality, `_verify_map_script_engine_fixture` and source `_cutscene_facts`
equality all pass. No retained payload was rewritten.

| Evidence owner | Selected direct review |
| --- | --- |
| Retained `local/derived/map-script-engine-static.json`, fixture `tests/fixtures/h2/map-script-engine-static-v1.json` | programCorpus.programs abcs_battle20: compare all 36 source-line/macro/argument records and the complete inline Down 1/endActions; read ce_4B780 and actual text 2566–2575/446 directly. |
| `tests/fixtures/h2/battle-cutscenes-static-v1.json` | Shared wrapper-source equality; actual defeated/after slot 20, absent region20, leader row 20=0, included join row 20=0 and immediate consumers supply selected semantics. |
| `tests/fixtures/h2/battle-loop-static-v1.json`, `common-stats-static-v1.json` | Existing lifecycle/membership context; direct branch order, healing, pre-scene seed, join capacity/list timing, return and Egress joins. No claim of a new combat simulation. |
| Retained `local/derived/map-init-static.json`, fixture `tests/fixtures/h2/map-init-static-v1.json` | Shared setup/follower context; direct csc37/csc42, ClearEntities, complete follower table, Map51 default/F520 pointers, init and ExplorationLoop establish replacement versus preservation. |

Use the exact source paths/symbols/slots above, strip comments for instruction
comparison, resolve macros/enums and actual jump aliases, and read complete
selected programs rather than guessed filenames or counts alone. The bounded
comparisons do not automatically guard every selected prose claim; complete
direct source/consumer review supplies those joins.

**Unknown:** natural combat/outcome/route, supplied flags/HP/classes/membership,
actual mappings/mode/raft/savepoint, completed action/text/fade/camera/input
services, rendered scene and persistence. Independent F420/F470/F520, repeated
or supplied membership, full/over-capacity/empty lists, dead/missing actors,
debug skip and partial return are static discriminating states, not executed
scenarios. No automatic runtime queue follows under
[ADR0014](../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../decisions/0016-remake-start-evidence-deferral.md).

Preserve [Issue868's handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/868#issuecomment-6170840743)
and [PR870 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/870#issuecomment-6170954167),
including retained comparisons, inspection corrections and linked prior failures.
Current exact Git identities, direct checks, actual CI and process state belong
in Issue872's handoff. Acceptance is complete source/prose/provenance/link/private
review, `git diff --check`, clean committed configured
`uv run sf2 verify plan --base origin/main --head HEAD`, and actual final-head CI.
No blanket local normal/full/engine/Godot/H3/H4 suite is selected solely for prose.
Leave a clean frozen Draft PR for independent review; no integration or cleanup.
