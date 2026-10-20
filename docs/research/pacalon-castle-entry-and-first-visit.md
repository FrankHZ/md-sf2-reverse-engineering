# Pacalon castle entry and first visit

Evidence date: 2026-10-14. **Confirmed:** the three accepted north exits from
Pacalon request Map36 exploration. With F530 clear, its default population
and conversations describe the drought; the king asks for help in dialogue
without setting a quest flag. An optional centaur interaction runs a movement
and gesture script before setting F810. Castle stairs retain the population
but rerun initialization; the south exit requests Map02. **Unknown:** natural
reach, actual supplied state, completed presentation/services, usable control
and persistence. The later F530-set Frayja event belongs to the
[restored-rain return owner](./pacalon-return-frayja-and-battle31-admission.md).

## Agreement and provenance

Standard rigor for [Issue #1025](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1025)
under [epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514):
entry, actor identity, current table selection, script completion and retained
state must agree. Accepted starting base is
`e9da6214b1ddfe4fad68fbc226df701d44b66081`, including the accepted
[Pacalon interaction and request owner](./pacalon-interactions-and-castle-handoff.md)
at `ac35fe192edba718a3261fef67d92874a477fd76`. This document is the sole tracked
write, with no shared write or stacked dependency. Independent synthesis and
UI692 supply no evidence.

Reuse the accepted [entry/lifecycle owner](./battle26-return-through-pacalon-entry.md),
[interaction](./map-event-interaction-state.md),
[dialogue](./map-event-dialogue-state.md),
[request state](./map-event-request-state.md),
[request consumption](./map-event-request-consumption.md),
[common maps](./common-maps.md), [common scripting](./common-scripting.md),
[pickup/services](./ketto-interactions-and-merchant-relocation.md),
[service contract](../design/contracts/service-interactions.md) and
[description routing](../design/contracts/map-area-description-routing.md).
The main premise is F530 clear. Record the alternate pointer/population
boundary; the [return owner](./pacalon-return-frayja-and-battle31-admission.md)
consumes the restored-rain callbacks and `cs_5DA28` separately.
Stop the south request at MainLoop before the town battle scan. Battle27/31/
33/34 progression, promotion/item-use algorithms, runtime, extraction, layout
acquisition and new maintained machinery are excluded.

Source paths below are relative to `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The retained read-only source is clean at that pin. USA retail ROM identity is
SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Fnnn denotes flag nnn; rows/indices are zero-based and address ranges half-open.
`sf2enums.asm` and `sf2const.asm` own numeric meanings. Short `map/`, `entity/`
and `text/` paths mean `code/common/scripting/`; castle tables are under
`data/maps/entries/map36/`, including `mapsetups/`.

Minimum evidence is selected pinned source, exact existing fixture/consumer
joins and retained map-init/script records. The dependency-valid 18,285,979-byte
pair and accepted 0.083s receipt are reused, not regenerated. Linear selected
reads are budgeted below one minute/512 MiB with small ignored receipts and
no runtime/network acquisition. Replan contradictions or material supporting-
system expansion. No new parser, fixture, schema or platform is needed.

## Incoming seed, admission and exploration construction

**Confirmed:** the accepted Map02 warp rows, in order, give the following
nominal no-scroll seeds; the third row is reached only after the first two
fail. Walking activation additionally needs the actual layout-kind/caller
conditions established by the request owner.

| Town source | Requested map / X,Y / facing |
| --- | --- |
|19,0|36 /20,30 /Up1|
|23,0|36 /22,30 /Up1|
|negative X wildcard,0|36 /preserve sampled X,30 /Up1|

The mutable single event slot is not a saved route. Production writes type
before parameters; no match preserves earlier pending state. On no-scroll
consumption, ProcessMapEvent clears the type before dispatch; the warp handler
submits/clears WARP_SFX, idles physical0 and unwinds to MainLoop. It clears
d0–d4 words, reads map36, then samples physical0's current signed coordinate
words divided by384. Nonnegative destination bytes replace only coordinate
low bytes; negative bytes preserve whole sampled words. Thus20/22/30 do not
erase a supplied high byte, and the wildcard X preserves consumption-time X,
not necessarily the earlier trigger X. Facing becomes1 and d4 becomes0.

`code/gameflow/mainloop.asm:@Start` calls SwitchMap, then CheckBattle.
`code/common/maps/mapinit_0.asm` and `data/maps/global/flagswitchedmaps.asm`
contain no36 replacement. `code/common/maps/getbattle.asm:CheckBattle` scans
all45 seven-byte BattleMapCoordinates records in order. None has map36, so
the intact explicit36 request returns d7=-1 without testing a matching
unlock or writing a battle crop. This is not the battle-return
`alt_MainLoopEntry`, which bypasses CheckBattle. Changed map/pending state
must be consumed as supplied; no castle warp record grants unconditional
town control or natural passage.

`explorationfunctions_2.asm:ExplorationLoop` clears pending type, performs
the signed word STEP_COUNTER subtraction20000/conditional clear, heals
living/immortal allies, requests fade-out and temporarily selects view-1.
Explicit36 writes CURRENT_MAP36 and CURRENT_BATTLE255, loads tilesets,
waits for fade, clears VInts, selects setup entities, rebuilds them and their
mapsprites, clears temporary F256–383 and sets80. It initializes sprite
counters, sets view0, calls LoadMap with the saved map word and d0=-1,
installs base VInts, and invokes the currently selected setup init. Music/
fade-in is conditional on the palette comparison, followed by movement SFX
and WaitForEvent. The shared heal scans all30 allies: Peter7/Lemon28 always,
others only when current HP is nonzero; eligible allies receive max HP/MP,
retain only already-present STUN/POISON/CURSE and recompute stats. Dead
Frayja22 stays dead; F22 is not that heal predicate.

This rebuild clears previous actors, lookup mappings and walking scratch.
It does not clear F22, F82, F530, F810 or item flags174–176. Main physical0
uses the returned coordinate **words** scaled by384 and facing1 with Bowie's
actual class/HP sprite semantics, not an inferred route endpoint or first
active battle member. Actor/VInt execution may progress before control.

## Selected map, area and setup

**Confirmed:** `data/maps/entries.asm:Map36` points to Map36s0_Blocks,
Map36s1_Layout and its area/flag/step/roof/warp/chest/other-item tables.
`00-tilesets.asm`, `$ADF6E..$ADF74`, selects palette10 and tilesets
90,52,50,92,58. The two area records in `2-areas.asm`, `$ADF9C..$ADFDA`, are:

| Row | Main rectangle | Secondary foreground / background origin | Layer type / music |
| --- | --- | --- | --- |
|0|0,0 through47,31|0,32 /0,0|0 /MUSIC_CASTLE|
|1|50,0 through61,15|0,0 /0,0|255 /MUSIC_CASTLE|

Both have256,256 parallax on each layer and zero autoscroll. The LoadMap
consumer in `code/common/maps/mapload.asm` loads blocks/layout, scans area
rectangles using the current viewed actor's internal position when the
supplied view coordinate is-1, and calls LoadMapArea. Normal north seeds
20/22 atY30 select row0. Another preserved X selects by actual coordinates;
the table terminator falls back to the first area if no rectangle matches.
The stairs below normally switch between these two area rectangles.
Area metadata is not a decoded collision map. The empty flag-event table,
five step block-copy records and nine roof records remain distinct loaded
map resources, not evidence of a traversable path through any doorway.

`data/maps/mapsetups.asm` selects default ms_map36 or F530 override
ms_map36_flag530. Retained map-init routeJoins sourceOrder81/82 and
pointerTableJoins82/83 join `$5D756..$5D76E` / `$5D76E..$5D786` to shared
ms_map36_InitFunction at$5DA16. The six pointers are entity list, entity
events, zone events, descriptions, item events and init. Only the first two
change under530; the other four are shared. GetCurrentMapSetup rechecks
CURRENT_MAP and the flag at each call. Population construction and later
interaction selection do not share a frozen F530 snapshot.

## Default population, mapping and init

**Confirmed:** `s1_entities.asm`, `$5D786..$5D7F0`, contains13 eight-byte rows
and a sentinel. The first is **ALLY_FRAYJA22**, not NPC128. M/F below mean
CENTAUR_MALE233/CENTAUR_FEMALE234; king231, princess232 and priest192 are
fixed sprites. `walk1` gives the listed origin and range1; other rows use
eas_Init. Every listed event row has UP1 (turn toward player, no restore).

| Source row / logical | Declared X,Y / facing / sprite | Script | Default callback / work |
| --- | --- | --- | --- |
|0 /22|28,6 /Down /ally22|eas_Init|Event0:3301→3302|
|1 /128|18,24 /Down /M|walk1|Event1:3281|
|2 /129|33,26 /Left /M|eas_Init|Event2:3282→3283|
|3 /130|7,5 /Right /M|eas_Init|Event3:3284→3285|
|4 /131|4,22 /Right /M|eas_Init|Event4:F810 branch below|
|5 /132|21,16 /Down /M|walk1|Event5:3288→3289→3290|
|6 /133|31,5 /Left /M|eas_Init|Event6:3291→3292→3293|
|7 /134|31,10 /Down /M|walk1|Event7:3294→3295|
|8 /135|29,3 /Down /king|eas_Init|Event8:3296→3297→3298|
|9 /136|38,5 /Down /princess|eas_Init|Event9:3299→3300|
|10 /137|16,10 /Down /F|walk1|Event10:3303→3304|
|11 /138|22,9 /Down /F|eas_Init|Event11:3305→3306→3307|
|12 /139|40,4 /Down /priest|eas_Init|Event12:j_ChurchMenu|

Map36 is neither overworld nor follower-excluded, so F65 supplies no
Caravan prefix. Ordinary flag-followers are allocated first in the complete
order retained by the [entry owner](./battle26-return-through-pacalon-entry.md#conditional-map02-population-and-first-eventcontrol-read).
Each enabled row independently allocates/appends a physical actor; force
membership is not its admission test. F82's row names ally22. Duplicate
aliases can allocate multiple followers and overwrite lookup entries;
capacity is finite and the initializer does not bounds-check mass enables.

`map/mapfunctions.asm:InitializeMapEntities` treats sprite values below30
as allies. If lookup22 is already nonzero, it consumes Frayja's action
longword but skips declaration and physical increment. Thus F82 normally
keeps the follower's position/script instead of placing another22 at28,6.
Otherwise row0 declares22 at the next physical slot using GetAllyMapsprite,
without checking joined22. For n actual follower declarations, let a=1
when the Frayja row allocates,0 when deduplicated. NPC row j=0..11 then
maps logical128+j/lookup32+j to physical n+1+a+j. The ally row does not
consume the NPC lookup cursor. With no followers, Frayja is1 and NPCs2–13;
with only82, Frayja is follower1 and NPCs again2–13, but his origin/script
differs. These are successful bounded allocations, not guaranteed mappings.

Ally class/HP controls Frayja's and Bowie's sprites; outside battle zero HP
uses blue flame. NPC fixed sprites do not use ally class/HP. The four
walkers copy50 bytes each into local walking scratch and patch their
origin/range. Their action loops request random steps subject to actual
movement state; origin/range is not a measured route. eas_Init establishes
speed32/32, motion/collision flags, Up orientation, size24 and sprite update
before jumping to eas_Idle; orientation is distinct from declared facing.

`s6_initfunction.asm`, `$5DA16..$5DA28`, tests **joined22**. Clear22 returns.
Set22 resolves logical22 via GetEntityIndexForCombatant, then
`mapsetupsfunctions_1.asm:MoveEntityOutOfMap` passes `$7000,$7000` to
SetEntityPosition. The latter writes actual X/Y **and** destination X/Y.
It does not delete the actor, clear lookup/follower-list membership, stop its
action script, change HP/class, join22 or set82. VInt_UpdateEntities skips
actors whose X is at least$7000, so the retained follower script does not
immediately move him back; later repositioning can make it eligible again.
This operation is also used on retained CURRENT
reloads: lookup22=0 resolves physical0, with no safe-absence guard, so a
supplied missing/corrupt mapping can move main or another actor. The init
is not a recruitment event or a guarantee that Frayja stays absent.

F530 set instead selects an11-row list at`$5D7F0..$5D84A`: ally22 at27,8
Left, then NPC128..137 at25,24 Left;12,25 Down walk1;21,16 Down walk1;
8,3 Down;5,22 Left;40,17 Down; king29,3 Down;31,5 Left;31,11 Left;
priest40,4 Down. Non-royal/priest NPCs here are male centaurs. The same
dedup and init rules apply. Its event pointer changes to the table at$5D924;
the Frayja callback and restored-rain dialogue/`cs_5DA28` are covered by the
[return owner](./pacalon-return-frayja-and-battle31-admission.md).
Changing530 after population construction can select callbacks against old
actors (including the changed priest index); it does not rebuild/relabel
them. This document supplies no completion claim for that alternate visit.

After init, WaitForEvent reads pending type before A/C. Zero installs control
on physical0 and resets view0; numeric PLAYER_TYPE0 selects character,1
raft, other nonzero Caravan in the actual control selector. Nonzero pending
state bypasses installation and is dispatched first. This first conditional
control/event boundary is not an observation of usable castle control.

## Default interactions and actual dialogue

**Confirmed:** `s2_entityevents.asm`, `$5D84A..$5D924`, has the13 named
four-byte rows above plus the$FD default. Relative offsets use the table
base, joining Map36_EntityEvent0..12 and Map36_DefaultEntityEvent/RTS.
The latter has action mask0. `tests/fixtures/h2/map-events/routing-setup.json`
and `entity-programs.json` retain those exact rows and14 callback bodies.

Reuse the actual activation and wrapper consumers from the Pacalon owner:
ordinary C follows debug/Caravan alternatives; GetActivatedEntity scans
physical actors near the tile ahead, stepping through desk/counter layout
and excluding viewed/follower actors. GetEntityEventIndex returns the first
reverse lookup alias, or0 on no match. RunMapSetupEntityEvent selects the
**current** setup, stores player facing in EVENT_RELATIVE_POSITION, obtains
portrait/speech from the mapped sprite, and conditionally opens its portrait.
UP1 waits a VInt and faces the actor `(playerFacing+2)&3`; it is not a required
approach direction and does not restore saved facing. Before the callback
the wrapper clears the VInt_UpdateEntities flag; on return it closes portrait/
text and activates that VInt. Default RTS still has this surrounding work.

Speaker labels in the population table are source declarations, not proof
of the actor actually activated. `entity/getentityportaitandspeechsfx.asm`
reads that actor's mapsprite and `data/spritedialogproperties.asm`, falling
back to default portrait/speech if unmatched. Altered class/HP/mapping can
change the presentation independently of the callback text.

The declared king/male-centaur rows select portrait NONE and DIALOG_BLEEP_6;
princess/female-centaur rows select NONE and DIALOG_BLEEP_2. Priest selects
portrait PRIEST and DIALOG_BLEEP_5. The FRAYJA_PROMO sprite row selects
portrait FRAYJA and DIALOG_BLEEP_3; other class/HP sprites follow their actual
row or fallback rather than inheriting that portrait from logical22.

All default text literals below join actual `data/scripting/text/gamescript.txt`
keys0CD1..0CEB, not only source comments. `{N}` is newline.

| ID | Complete literal |
| --- | --- |
|3281|`Are you from South Parmecia?{N}Oh, great!{W1}`|
|3282|`It's been dry here since the{N}devils appeared.{W2}`|
|3283|`They must have done something{N}to Goddess Mitula!{W1}`|
|3284|`Brave Pacalons?{W2}`|
|3285|`None at all.  There are no{N}soldiers who can fight{N}against the devils.{W1}`|
|3286|`Devils are coming to town?{N}OK, I'll beat them up!{W1}`|
|3287|`Oh, I can't.  I'm too hungry!{W1}`|
|3288|`They came from Nazca, which{N}is west of Moun.{W2}`|
|3289|`We locked the gate of Moun{N}to stop their progress.{W2}`|
|3290|`Vicar {NAME;22} has the{N}key to open the gate.{W1}`|
|3291|`Devils came to Pacalon{N}through Moun.{W2}`|
|3292|`We blocked the invaders{N}by locking the gate of Moun.{W2}`|
|3293|`That means, we abandoned{N}the people living there.{W1}`|
|3294|`We don't have enough food{N}because of the long drought.{W2}`|
|3295|`Just one rainfall would make{N}us happy.{W1}`|
|3296|`{NAME;19} asked you to drop{N}by here?  Thank you!{W2}`|
|3297|`May I ask you a favor?{N}Please save Mitula.{N}She's in Tristan.{W2}`|
|3298|`We will be ruined by the{N}drought long before the{N}devils destroy us.{W1}`|
|3299|`You'll see a route through{N}the mountains when you go{N}north from here.{W2}`|
|3300|`You can get to Tristan by{N}traveling along that route.{W1}`|
|3301|`I committed a sin....{W2}`|
|3302|`I abandoned the people of{N}Moun to protect the{N}Parmecia continent.{W1}`|
|3303|`What happened to the{N}soldiers who went into{N}Mitula's Shrine?{W2}`|
|3304|`Were they all killed?{W1}`|
|3305|`I saw it!{W2}`|
|3306|`The Princess ate food in the{N}kitchen one night.{W2}`|
|3307|`I didn't wish to see such{N}an uncouth princess.{W1}`|

`sf2macros.asm:txt` uses trap5/DisplayText; each call resets the substitution
pointer to DIALOGUE_NAME_INDEX_1 and decodes the selected bank. Explicit
`{NAME;22}`/`{NAME;19}` use `textfunctions_1.asm:symbol_player` and the next
encoded index to GetCombatantName (Frayja/Higins), not the activated NPC or
first active member. Ordinary `{NAME}` below consumes the current substitution
slot. Consecutive txt calls do not imply a close between them. W1 and W2
retain their distinct input/RNG/VInt/cursor/sound/cleanup rules from the
dialogue owner; neither is a YesNoPrompt. No new timing/rendering is observed.

Event8's3296–3298 requests saving Mitula in Tristan solely as dialogue:
no YesNoPrompt, flag write, join, item grant or map/battle request follows.
The reference to Higins does not test F19. Other default text bodies likewise
end in RTS. Only Event4 has the local F810 write; priest invocation has
shared conditional effects. **Inferred:** drought, remorse and frightened
bravado are conversation themes. Claims about Moun's gate/key, a mountain
route, casualties or the princess are spoken statements, not independent
proof of accessibility, world state or mandatory objectives.

Event12 directly calls j_ChurchMenu with no local first-visit flag. Its shared
entry requests110 (`Welcome!{W2}{N}Your desire will be fulfilled!{W2}`), then
offers raise/cure/promote/save under the accepted service contract. Contact
alone neither heals nor saves. If the Save path is actually selected and its
first prompt returns zero, the consumer copies current CURRENT_MAP (nominally36)
to EGRESS_MAP, sets399 and calls SaveGame with CURRENT_SAVE_SLOT. A nonzero
first answer avoids saving; after save, zero exits and nonzero can suspend
instead of returning normally. `savepointmapcoords.asm` gives Map36 `(40,5)`,
Up, as return data, not an immediate teleport. Interrupted services, actual
mutations and persistence remain separate from successful invocation.

## Optional centaur scene and F810 timing

**Confirmed:** Event4 tests810. Held810 jumps directly to3287 and RTS.
Clear810 requests3286, closes text, invokes `script cs_5DA7A`, sets810 only
after that call returns, then requests3287 and returns. It does not test
party membership, hunger, battle flags or a route prerequisite. F810 is
outside the temporary-flag range; revisiting/reloading does not clear it.

`sf2macros.asm:script` loads the PC-relative target and enters trap6.
`Trap6_TriggerAndExecuteMapScript` activates VInt_UpdateEntities before
ExecuteMapScript, allowing action progress despite the callback wrapper's
earlier flag clear. The interpreter initializes action scratch at
FF9004_LOADING_SPACE and resets SKIP_CUTSCENE_TEXT. Its complete selected
program, `$5DA7A..$5DAE4`, has17 top-level commands including terminator;
the inline payloads below are additional data, not omitted commands.

| Index | Exact request / inline payload |
| --- | --- |
|0|setCameraEntity131|
|1|setDest ALLY_BOWIE,4,23|
|2|setFacing ALLY_BOWIE,UP|
|3|entityActionsWait131: moveRight2; endActions|
|4|entityActionsWait131: moveDown1; endActions|
|5|setFacing ALLY_BOWIE,RIGHT|
|6|csWait60|
|7|customActscriptWait131: ac_setSpeed16,16; ac_jump eas_Idle; ac_end|
|8|entityActionsWait131: moveRight2; endActions|
|9|headshake131|
|10|shiver131|
|11|csWait120|
|12|shiver131|
|13|customActscriptWait131: ac_setSpeed8,8; ac_jump eas_Idle; ac_end|
|14|entityActionsWait131: moveRight1; endActions|
|15|shiver131|
|16|csc_end|

The retained script `programCorpus.programs[id=cs_5DA7A]` binds these
top-level commands. `entityActionBridgeCommandFacts.sourceSites` binds
all four packed movement payloads and both inline action payloads. Their
real consumers are `map/mapscriptengine_1.asm` and the entity engine:

- csc24 resolves131 through lookup35 to its current physical actor and
  writes VIEW_TARGET_ENTITY. It does not wait for camera arrival. No command
  here resets that view target before returning to Event4's final text.
- csc29 scales4/23 by384 into Bowie's destination, sets absolute travel and
  signed velocities32 on each axis, then waits because the high no-wait bit
  is clear. WaitForEntityToStopMoving polls actual X/Y against destination
  and waits VInts. This is destination/motion setup, not SetEntityPosition
  or a teleport, and has no path search or observed completion guarantee.
- csc23's facing request checks ally HP first; dead Bowie skips the operand
  and returns without the facing write. A living mapped actor gets Up1 then
  Right0 with a sprite update. This does not make csc29 itself HP-gated.
- csc2D replaces the actor action pointer with compiled scratch. It seeds
  the action-read timer with the mapped physical ID, clears flagsA bits5/6
  via `$9F`, reads each packed direction/distance byte, and emits relative
  destination opcode4 plus signed word deltas and trailing opcode1
  (esc01_waitUntilDestination, not a one-frame delay). Right2,
  Down1, Right2, Right1 become `(2,0),(0,1),(2,0),(1,0)` tile deltas. Each
  payload ends$8080 and appends opcode$34/jump eas_Idle; the nonzero Wait
  byte polls for that exact idle pointer. This replaces rather than stacks
  an old actor action script. It does not save/restore the actor's flags.
- esc04 scales the signed deltas by384 relative to the actor's **then-current**
  X/Y. Its destination-overlap helper is gated by flagsA bit5, cleared by
  this compiler. The shared loc_55C8 writes destinations, absolute remaining
  travel and signed velocities from the actor's speed bytes; VInt movement
  consumes them. This is not evidence of a collision-checked walking route.
  If every move completes from the nominal4,22 origin without intervening
  changes, the successive destinations are6,22;6,23;8,23;9,23. An altered
  mapping/position/script changes this conditional arithmetic.
- csc14 installs the inline action address, seeds the timer with physical
  ID, waits for eas_Idle, then scans words through the$8080 terminator.
  ac_setSpeed is opcode$10 followed by the two speed bytes: esc10 writes
  their packed word to adjacent X/Y speed fields. ac_jump is opcode$34 and
  an absolute pointer consumed by esc34_jump. Each payload has10 action
  bytes plus a2-byte terminator. Reached completion leaves speed16/16 for
  the middle move, then8/8 for the last; neither restores the initial32/32.
- csc27 headshake sets animation counter-1, then performs seven DBF
  iterations of two mapsprite transformations/DMA/update sequences
  (`entityfunctions_4.asm:sub_45D1C` / `sub_45D46` via the nibble-copy
  helper sub_45DA4 on both animation frames), each
  with2+1 VInt waits. It ends with animation counter0, not the saved value.
  csc2A shiver saves SPRITE_SIZE and animation counter, sets size21/counter-1,
  then toggles flagsB bit3 on/off three times with Sleep5 after each sprite
  update, finally restoring saved size/counter. Each of the three calls
  leaves bit3 clear; it does not restore an initially held bit3.
  Bit3 is ENTITYDEF_FLAGS_B_RESIZE: UpdateEntitySprite_0 calls
  ChangeEntityMapsprite, whose ordinary-sprite path decompresses the facing
  graphics, conditionally calls ResizeSprite with size21 and queues VRAM
  DMA. Special sprites/entnum32 can skip that graphics path. Shiver changes
  graphics state, not entity X/Y; the calls alone prove no rendered result.
- csWait60/120 encode `$803C/$8078`. ExecuteMapScript passes the low byte
  to Sleep, which performs that many WaitForVInt calls. Debug/P2-Start can
  set SKIP_CUTSCENE_TEXT and skip these negative-word sleep commands. The
  shiver's explicit Sleep5 and headshake waits are separate calls. Counts
  describe requested waits, not measured seconds or rendered animation.

At csc_end ($FFFF), ExecuteMapScript conditionally waits for view scrolling
when DIALOGUE_WINDOW_INDEX is nonzero, clears VIEW_SCROLLING_SPEED, restores
registers and returns. It does not restore camera target, old action scripts,
speed, positions or flagsA. The caller then sets810 **before**3287; interruption
before script return can leave movement/gesture effects with810 clear, while
interruption during final text can leave810 set without completed dialogue.
The interaction wrapper closes windows and reactivates entity updates on
return. The next WaitForEvent resets view0 and installs main control only
if pending type is zero. Script return, callback return and usable control
are three different boundaries; none has a castle runtime observation here.

## Nine shared descriptions and collection records

**Confirmed:** both setups point to `s4_descriptions.asm`,
`$5D9C6..$5DA0E`:16-byte wrapper, nine6-byte records at$5D9D6, then$FD00.
Base `$1038` is4152. All records have condition0 and payload-kind0:

| Row | X,Y | Investigation offset →ID | Description offset →ID / complete literal |
| --- | --- | --- | --- |
|0|35,3|4 →427|0 →4152 /`A book...{N}PACALON KINGS{W1}`|
|1|36,3|4 →427|1 →4153 /`A book...{N}PALADIN HONOR{W1}`|
|2|56,8|10 →433|2 →4154 /`It's locked.{W1}`|
|3|19,15|9 →432|3 →4155 /`A statue of a fine soldier.{W1}`|
|4|23,15|9 →432|4 →4156 /`A statue of a fine soldier.{W1}`|
|5|26,8|9 →432|5 →4157 /`A statue of a fine soldier.{W1}`|
|6|32,8|9 →432|6 →4158 /`A statue of a fine soldier.{W1}`|
|7|26,11|9 →432|7 →4159 /`A statue of a fine soldier.{W1}`|
|8|32,11|9 →432|8 →4160 /`A statue of a fine soldier.{W1}`|

Actual gamescript investigation strings are427
`{NAME} investigated{N}the book shelves.{W2}{CLEAR}`,432
`{NAME} investigated{N}the statue.{W2}{CLEAR}`,433
`{NAME} investigated{N}the door.{W2}{CLEAR}`. CheckArea clears the name slot
to0 and speech to0, so NAME selects Bowie's name. The wrapper displays
423+investigation then4152+description, closes portrait/text and returns
d7=-1 for the first admitted match. All local condition0 rows admit ordinary
d6=1 examination. No match returns0; d6=1 allows FieldMenu fallback,
while supplied d6=0 uses generic area/empty text under the accepted owner.

Activation precedes CheckArea; within CheckArea, actual layout kinds$1800
chest,$2C00 vase,$3000 barrel,$3400 bookshelf and$1C00 area-item take priority
over the description wrapper. Thus a book-labelled coordinate is not proof
of which path executes. The locked-door literal neither writes an unlock
flag nor checks a key, opens a door or proves passage. No description has
a function payload, choice or story-state mutation.

`7-chest-items.asm`, `$AE07A..$AE080`, contains one four-byte row plus
endWord. `8-other-items.asm`, `$AE080..$AE08A`, contains two plus endWord:

| Table / row | Coordinate | Flag | Item / numeric index |
| --- | --- | --- | --- |
|Chest0|56,4|176|MITHRIL123 ($7B)|
|Other0|22,4|174|FAIRY_POWDER5|
|Other1|30,19|175|PEGASUS_WING118 ($76)|

These are shared across530 and independent of810. Map headers and four-byte
mapItem order select them; no layout decode establishes their objects or
reachability. GetChestItem/GetNonChestItem select current map pointers and
GetItem compares candidate low-coordinate bytes in row order. The original
non255 CURRENT_BATTLE alternative uses the accepted erroneous stride5 crop
calculation, not a silently repaired seven-byte stride.

For an actual chest-kind examination, CheckArea calls OpenChest, which
finds the row, substitutes NOTHING127 when176 is held, sets176, writes
open-chest block55298 ($D802), and requests the appropriate plane update:
type0 sets bit1; nonzero layer type sets bit0. These writes precede pickup.
It requests403 (`{NAME} opened the chest.{W2}{CLEAR}`); an empty result
requests408 (`But, it was empty.{W1}`). The separate CheckChestItem helper
only tests flag/layout and does not perform OpenChest's writes. Repeats
cannot be claimed to grant duplicate Mithril; actual retained/loaded block
kind still determines examination routing.

Qualifying non-chest branches call CheckNonChestItem: held174/175 substitutes
NOTHING, and a matched row sets its flag before return. No match returns-1
(masked to127), selecting the reached object's empty text. Item5/118/123
are real inventory items below the gold-chest threshold. The common
CheckArea tail requests413 (found item), checks Bowie's four-slot capacity,
then AddItem, music,415 and input/fade. If full it UpdateForce/scans the
ordered other joined members for space, AddItem,416 and input/fade. The
actual helper returns failure if no empty slot; this caller does not test
that return after its capacity read. Concurrently altered capacity is not
repaired by a success-sounding text.

If the whole reached force is full,417 precedes **both** CloseChest and
RefillNonChestItem at saved candidate coordinates. Each re-finds its own
table; for this Mithril row CloseChest clears176 and writes closed block55297
($D801) with plane update. For the other rows RefillNonChestItem clears174/175.
Retry is enabled only after that recovery is reached. Interruption before
AddItem/recovery can leave a held flag without an item; after AddItem it can
leave inventory changed without completed presentation. The accepted pickup
owner retains actual413/415/416/417 literals, substitutions and force ordering.
No promotion, Pegasus Wing use or Mithril forging follows from collection.

Shared zone and item-event tables have only Map36_DefaultZoneEvent/RTS and
Map36_DefaultItemEvent0/RTS. Their wrappers retain cleanup/waits and the
item-use d6=0 default, distinct from collection. Neither grants an item,
joins Frayja or produces a map request.

## Ordered stairs, return request and lifecycle

**Confirmed:** `6-warp-events.asm`, `$AE050..$AE07A`, has five eight-byte
records and endWord. All are no-scroll (type0); warpFacing emits the facing
byte followed by zero padding. The source order is:

| Row | Source X,Y | Map operand | Destination X,Y | Facing |
| --- | --- | --- | --- | --- |
|0|255,31|MAP_PACALON2|255,1|Down3|
|1|2,4|MAP_CURRENT255|52,3|Right0|
|2|52,3|MAP_CURRENT255|2,4|Left2|
|3|55,11|MAP_CURRENT255|5,12|Right0|
|4|5,12|MAP_CURRENT255|55,11|Right0|

These are requests, not compulsory stair routes. The shared WarpIfSetAtPoint
consumer scans first match, treats negative source bytes as wildcards and
checks$FFFF terminator first. Ordinary character walking needs layout kind
$1000; raft/Caravan have their own callers. None of these rows tests530,
810, joined22, services or items. Each posts event1, PARAM1..4 longword,
PARAM5/padding word and WARP_SFX as the incoming owner describes.

The no-scroll handler consumes mutable parameters, idles main, samples its
current coordinate words, performs independent sign/low-byte replacement
and returns to MainLoop with d4=0. CURRENT255 is not a scroll and is not
resolved inside SwitchMap. CheckBattle temporarily resolves CURRENT_MAP;
with actual36 its45-row scan finds no candidate and restores the original
map operand255 before exploration. Changed CURRENT_MAP can alter that scan.

For the intact internal rows, ExplorationLoop's CURRENT branch still clears
pending, adjusts STEP_COUNTER, heals, fades, waits, updates main facing/sprite
and nonnegative-low-byte coordinates, runs DeclareRaftEntity, initializes
sprite counters, calls LoadMap for the current map, installs base VInts and
reruns the current setup init. It does **not** rebuild actors/mappings/walking
scratch, load a new fixed list, replace CURRENT_BATTLE or clear temp flags.
UpdateMainEntityProperties scales whole coordinate words only if their low
byte is nonnegative; changed negative low bytes skip that coordinate update.

DeclareRaftEntity is not a no-op: numeric mode2 returns immediately; mode1
skips follower relocation; other modes move listed followers' actual and
destination coordinates to the supplied main tile. It then uses F64/RAFT_MAP
to declare or move off-map physical raft31 under the accepted consumer.
Follower scripts/mappings stay live. Castle init runs **after** this relocation
and again moves mapped22 to$7000 when22 is set; it does not erase the follower.
F810 and the centaur's reached action/speed/position normally survive these
internal requests, subject to later actor execution. A fresh explicit36 entry
rebuilds its source position/script even when810 is already held, so repeat
dialogue does not require the scene's final position to persist.

With ordinary coordinates, row1/row4 select the50..61 area; row2/row3 select
the0..47 area. LoadMap sign-extends its map byte: CURRENT255 takes the reload
branch directly to the area's pointer. It skips new palette/tileset transfers
and LoadMapBlocksAndLayout, retaining the loaded block/layout state while
refreshing area/view/display and roof handling. An explicit-map load instead
rebuilds blocks/layout and reapplies held chest flags as open blocks. These
are different consumers; stairs do not reset174–176 or unlock the door.
Later F530 changes select the current init/event pointers without implicit
actor reconstruction. Conditional palette/music and WaitForEvent boundaries
remain the same as explicit entry.

South row0 instead returns an explicit Map02 operand, facing3, destination
Y low byte1 and preserved consumption-time X. This document stops at that
MainLoop return **before** the town SwitchMap/CheckBattle/setup lifecycle.
It does not claim a completed rebuild or town control. Explicit-map requests
take the fresh-population/temp-clear branch if subsequent admission reaches
exploration, as established by the accepted entry owner; that conditional
lifecycle is distinct from these four CURRENT stair requests.

## Changed states, reproduction and retained limits

| Changed state / interrupted boundary | Required distinction |
| --- | --- |
|530 clear/set or changes between reads|List/event pointers differ; shared init/descriptions remain; the return owner covers the alternate visit separately from drought callbacks.|
|22/82 independently clear/set|Ally declaration/dedup, follower exclusion and init displacement are different predicates; no new join.|
|Missing/aliased lookup, extra followers, class/HP/capacity|Actual physical actor and first reverse alias govern;0 is not a safe absent result; bounded allocation formulas are conditional.|
|810 clear/set or interrupted scene/final text|First scene precedes flag write; repeat skips scene; reached motion/flag/text are not atomic.|
|Actor origin/action/speed/flags or debug changes|Relative destinations, overwritten actions, waits and sprite effects use current state; no forced route or measured duration.|
|Camera, PLAYER_TYPE, pending event or approach differs|Activation/control selection and later camera reset are conditional; pending wins over input.|
|Description layout/d6 or item flags174–176 differ|Consumer priority and first admitted match matter; coordinates alone do not prove access.|
|Bowie/force full, membership changes or pickup interrupted|Ordered fallback; rollback only when reached; flag-before-item and item-before-presentation remain.|
|Church cancel/save/suspend/interruption|Invocation differs from service mutation, normal return and persistence.|
|CURRENT versus explicit map, sentinel/high bytes or replaced request|Retained actor/temp state differs from fresh construction; all coordinates/payloads are sampled as consumed.|

Load ignored `local/private-inputs.ps1` in each command's launching process.
Use the retained clean pinned source selection and
[retained-pair recipe](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary)
without regeneration or changing pair size/mtime. Direct review:

1. Join all three incoming rows and all five outgoing rows to actual
   MainLoop/SwitchMap/CheckBattle/ExplorationLoop/request consumers. Compare
   all45 battle-map records for absence36, area selection and CURRENT versus
   explicit lifetime. Preserve the outgoing south stopping boundary.
2. Compare both pointer tables and population lists, all14 default event
   rows/bodies, shared init's five operations and concrete mapping consumers
   with `map-events/routing-setup.json`, `entity-programs.json` and retained
   map-init joins/body. Check22/82/530 changes and missing mappings explicitly.
3. Compare27 default dialogue literals, service110 and all investigation/
   description/chest literals with gamescript and their actual consumers.
   Join all nine description rows and three item records to existing
   `map-descriptions-static-v1.json` and pickup/service owners; do not promote
   text/coordinate evidence into layout or state mutation.
4. Compare `cs_5DA7A`'s17 top-level records and six inline/packed payloads
   with retained program/action-bridge records and actual macros/handlers,
   including motion, speed, waits, flag timing and caller return. Aggregate
   script counts alone do not establish payload or rendered behavior.
5. Review the whole prose/diff, changed states, links/anchors, UTF-8/LF,
   sole-path/private boundary and `git diff --check`. On the clean committed
   head run `uv run sf2 verify plan --base origin/main --head HEAD`; retain
   planner and actual exact-head scoped CI in the Issue handoff. This
   documentation scope skips blanket normal/full/Python/.NET/Godot/H3/H4 suites.

**Unknown:** actual supplied flags, HP/class, mappings/followers/capacity,
inventory, modes/pending state, natural reach, completed dialogue/services/
collection/transitions/scene, usable control, rendering/timing and persistence.
The optional compressed-layout absence remains a no-decoded-route limit,
not a ROM blocker or acquisition authorization. No new runtime was launched;
Unknowns are not an automatic observation queue. The
[return owner](./pacalon-return-frayja-and-battle31-admission.md) covers the
F530-set Frayja event through Battle31 admission; later battle bodies remain
outside this first-visit scope.

Preserve [#1021 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1021#issuecomment-6182880836)
and [PR1023 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1023#issuecomment-6182901348),
including linked #1017/predecessor failures, Unknowns, all original/rebased
receipts and completed CI. The retained pair/0.083s comparison is reused;
no completed result is reset or relabeled. Source/configuration/pair and new
small review receipts stay local. Freeze a clean current-main-based pushed
Draft PR, Project Review handoff and main-gate callback; no merge, cleanup,
archive, runtime or next Issue.
