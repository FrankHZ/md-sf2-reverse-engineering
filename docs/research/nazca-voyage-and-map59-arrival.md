# Nazca voyage and Map59 story admission

Evidence date: 2026-10-20. **Confirmed (static):** cs_5FB6A changes
between ship interior37 and Nazca overworld75, rebuilds its stage actors,
requests scrolling/special-sprite/palette services, then publishes a warp
to59,63,63/Right. It neither removes the Sky Orb nor sets999 or the later
battle unlocks. Map59's name does not itself admit a battle. With442/443
clear and543/999 clear, fresh exploration selects ms_map59_InitFunction
and first dispatches cs_5EB44. **Stop before that scene's body.** Natural
completion, presentation, timing, usable control and persistence remain
**Unknown**; the current compiler rejects the voyage's special entity.

## Agreement and evidence route

Standard rigor for [Issue #1108](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1108)
under [#1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
connect accepted [boarding and Orb admission](./nazca-ship-boarding-and-departure-admission.md)
to the next story boundary, supporting content/engine decisions. Accepted
base is `96f70ab6cce06b4b38df047f2fd9d250a6373a9e`. Own this document
and navigation only in that predecessor; no shared registry, index,
schema, fixture, tool, design or engine edit, and no stacked dependency.

Include the complete cs_5FB6A, its inline actions, six selected native
helpers, ce_5FDD4, map-load/allocation/service consumers and final request,
Map59 battle admission and setup/init selection. Changed543/999/443/444
states stop at their selected battle or first scene dispatch, not inside
it. Exclude the five attack/shootdown bodies, alternate ending, Battle43/44
initialization,999 ship revisit, later exploration and runtime acquisition.

Original paths below are relative to `disasm/` at clean, read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
USA ROM SHA-256 remains
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`;
no new ROM/H1 equality or runtime claim is made. Constants/widths resolve
from sf2enums.asm and sf2const.asm. `map/` and `entity/` abbreviate
`code/common/scripting/`; `exploration/` abbreviates
`code/gameflow/exploration/`. Fnnn is flag nnn; IDs and ordinals are
decimal, ordinals zero-based, intervals half-open.

Reuse [map request production](./map-event-request-state.md),
[consumption](./map-event-request-consumption.md), [common maps](./common-maps.md)
and [scripting](./common-scripting.md). Reuse the immutable18,285,979-byte
init/script pair and `local/issue1005/pair.txt`0.083s provenance; do not
regenerate it. Bounded linear source/consumer reads, one pair load and
selected in-memory lowering are budgeted at one minute/512MiB per direct
audit, with small ignored receipts under `local/issue1108/`. Reassess a
material scope/resource expansion. No new extraction, image/video/frame
collection, parser platform, SDK, Godot, emulator or installation.

## Admission and complete voyage order

**Confirmed:** Map37_ItemEvent0 invokes cs_5FB6A only after the matching
8,11/Left/113 item callback, clear999 and held836. Its preceding3375 text
and close are owned by the predecessor. The callback has no inventory
recheck, and the voyage itself does not reread836,76 or999. Its complete
body in `data/maps/entries/map37/mapsetups/scripts.asm` is
`$5FB6A..$5FD3A`. The following table retains every top-level operation
in order; each grouped cell is sequential, not an unordered effects list.
Text IDs advance from3377; all selected text commands are nextSingleText.

| Stage | Ordered commands |
| --- | --- |
|Initial vibration|textCursor3377; SFX_BIG_DOOR_RUMBLE; quake1; install/wait eas_Init on Peter7,128,Zynk26; camera11,6; install/wait Peter eas_Jump twice;3377 `$80`,Peter; face128 Up;3378 `$00`,128.|
|Zynk's route|setDest26 to11,11 then14,11 then15,10 then16,10; face26 Down;3379 `$00`,26; install/wait Peter eas_Jump;3380 `$80`,Peter.|
|Lift request|camera4,6; face Bowie0 Up; SOUND_COMMAND_CUT_OUT; SFX_BIG_DOOR_RUMBLE; quake3; wait20; quake1; MUSIC_MITULA_SHRINE; call csub_5FD3A; quake0.|
|Interior actor movement|Start128: Up1,Left1,DownLeft1,Left6, endActions; start/wait Peter: Up1,Left2,DownLeft1,Left5, endActions; face128 Up; face Peter Up;3381 `$00`,Peter; face Zynk Left; camera11,6;3382 `$80`,Zynk; wait40.|
|First exterior|fadeOutB; camera entity CAMERA_NEUTRAL; mapLoad75,40,12; loadMapEntities ce_5FDD4; install/wait Bowie eas_Init; setBlocks60,59,4,5,44,15; fadeInB; call sub_5FD7C; wait60;3383 `$00`,FOLLOWER_B31; wait60.|
|Interior reconstruction|fadeOutB; mapLoad37,4,6; loadEntitiesFromMapSetup9,11,Right; wait1; install/wait eas_Init on Peter,128,Zynk; setPos128,9,12,Up; setPosPeter,10,12,Up; setPosZynk,10,11,Left; fadeInB;3384 `$80`,Zynk.|
|Interior travel request|quake1; wait10; quake0; call sub_5FD4C; face128 Left;3385 `$00`,128;3386 `$00`,Peter; face128 Up; faceZynk Down;3387 `$80`,Zynk; wait60.|
|Second exterior and handoff|fadeOutB; mapLoad75,40,12; loadMapEntities ce_5FDD4; install/wait Bowie eas_Init; setBlocks60,59,4,5,44,15; customActscriptWait128: ac_setSpeed16,16, ac_jump eas_Idle, ac_end; entityActions128: Left30, endActions; call sub_5FD5E; fadeInB; wait15; call sub_5FD66; wait120; warp59,63,63,Right; csc_end.|

No conditional branch occurs within this voyage body. Entry state and
service/actor behavior still matter. The nonwaiting128 movement before
Peter's waiting movement does not independently wait for128 to finish;
Peter becoming idle does not prove the other actor is idle. The final
Left30 is also nonwaiting. The later waits/helpers do not test its
destination or guarantee completion before warp.

`mapscriptengine_1.asm:csc29_setEntityDest` consumes a **word** selector,
resolves its alias and writes X/Y destinations multiplied by384. It
computes unsigned travel magnitudes and signed X/Y velocities32 from
current coordinates. Bit15 of the saved selector controls waiting: clear
calls WaitForEntityToStopMoving; held returns without that wait. The four
Zynk operands are26, so each waits before the next is issued. There is no
AdjustScriptPointerByCharacterAliveStatus, membership or alias-validity
guard. Entity motion must actually make current X/Y equal destinations;
`entityfunctions_2.asm:WaitForEntityToStopMoving` polls those words with
WaitForVInt and has no timeout. This is not an immediate position write,
an action-pointer wait or a guaranteed four-segment completed journey.

`csc15_setEntityActscript`, `csc14_setEntityActscriptManual` and
`csc2D_entityActionSequence` resolve the actor and seed its action timer
with its physical slot byte. Waiting forms poll the pointer for eas_Idle;
none HP-skips. Manual scripts scan the `$8080` delimiter after installation.
Sequence installation also masks FLAGS_A with `$9F`, clearing bits5/6,
and builds relative movement actions with a terminal idle jump. The final
custom speed script uses the manual installation, retaining flags; the
following sequence uses the clearing installation. The installed speed16
therefore survives into that movement unless another service changes it.

The shared `data/scripting/entity/eas_actions.asm:eas_Init` resets
acceleration factors, speed32, acceleration/deceleration modes,
obstruction/collision/autofacing/immersion, faces Up, requests shared size24
and sprite update, then jumps idle. eas_Jump disables autofacing, requests
relative0,−1/wait5,0,1/wait5,0,0, restores autofacing and jumps idle.
These are action/service requests, not evidence of visible jumps or elapsed
wall time. `setPos` and `setFacing` differ: csc19/csc23 call the HP helper
and skip their4/2 operand bytes for zero-HP allies below30. They then
resolve the alias and request UpdateEntitySprite_0. Logical128 bypasses
the ally HP check. A dead Zynk still receives setDest/actions/text, while
his later position/facing requests can skip; do not generalize one opcode's
guard to the whole scene.

## Scene loads, actor identity and layout boundary

**Confirmed:** csc48_loadMap detaches the camera, loads selected tilesets,
waits for fading to finish, deactivates VInt services, calls LoadMap with
the map and packed camera coordinates, enables display/interrupts,
reactivates services and waits one VInt. LoadMap changes CURRENT_MAP for a
positive selection and loads its area properties. It does not invoke
MainLoop battle admission, fresh exploration setup/init or item callbacks.
Thus the script's37→75→37→75 changes are scene loads, separate from its
final warp request. No battle, joined status, HP or inventory write is
implied by one of these map names.

`csc42_loadMapEntities` consumes mainEntity's three words, then calls
InitializeMapEntities/LoadEntityMapsprites with display/services controlled.
ce_5FDD4 (`$5FDD4..$5FDE4`) is exactly:

| Record | Fields |
| --- | --- |
|mainEntity|0,0,Down; player seed, not the ship position.|
|entity|45,17,Right,MAPSPRITE_NAZCA_SHIP253,eas_Init.|
|end|cscEntitiesEnd.|

`mapfunctions.asm:InitializeMapEntities` clears entity storage and aliases,
creates currently selected followers, reads the fixed records, then declares
Bowie0 and applies the conditional raft helper. Special mapsprites use
**physical47** (ENTITY_SPECIAL_SPRITE), append that slot to the non-ally
alias list and do not advance the ordinary slot counter. The scene's first
non-ally is logical128→47, not ordinary physical128 or Astral30.
`ENTITY_DATA +47*32 +ENTITYDEF_OFFSET_LAYER17` equals
ENTITY_SPECIAL_SPRITE_LAYER `$FFAEF3`; the helpers below write that actor's
layer byte. Declaration clears its layer through the upper half of d6;
the first exterior ramp starts from0 under this successful rebuild.

Map75 is in `data/maps/global/overworldmaps.asm`. If F65 is held,
InitializeFollowerEntities starts at table_OverworldFollowers's
65/FOLLOWER_A30/Caravan prefix, then continues directly into table_Followers:
there is no intervening terminator. The consumer advances four bytes per
row until the final tableEnd.w, admitting rows by their flags. If65 is
clear, it starts at table_Followers and skips only that Caravan prefix.
With only65/70 held, Caravan is physical1 and Astral/FOLLOWER_B31 physical2;
with only70 held, Astral is physical1. Other admitted rows shift slots.
Rows74/Astral,68/Oddler and70/Astral target31 in that order; later admitted
rows overwrite its alias without removing earlier physical actors. If none
is admitted,31 retains cleared alias0. 3383 resolves the live result, not a
hardcoded Astral. No dialogue phrase proves presence. Flags76/836
are not equivalent to65/70, and the scene's ship128 is a separate identity.
Unresolved aliases are not rejected by GetEntityAddressFromCharacter.
These allocation facts depend on each declaration returning; arbitrary
enabled-row counts and action-pointer capacity are not validated here.
Reuse the accepted [overworld population explanation](./north-cliff-opening-and-rohde-recruitment.md)
and [prefix/alias examples](./polca-departure-through-luke-recruitment.md).

Returning to37 uses csc49_loadEntitiesFromMapSetup, which reselects the
**live** setup, rebuilds its entities with Bowie9,11/Right and loads sprites.
It does not call ms_map37_InitFunction or
InitializeNazcaShipForceMembers. With999 clear, the predecessor's complete
31 fixed records apply: ally1..29, Astral128/physical30 and
Caravan129/physical31; ordinary followers are suppressed on37. Their
joined-state hiding is an initializer effect and is not automatically
reapplied by this opcode. With999 held through an injected/stale path,
the alternate31-record table is selected instead (different stage
positions, same ally/non-ally identity order). No alternate scene body is
admitted. Subsequent128/7/26 setPos requests remain subject to the actual
opcode/HP/alias rules. A conditional raft declaration can overwrite31;
player type and raft state remain preconditions, not repaired assumptions.

Both exterior setBlocks calls copy the working-layout rectangle60,59
through63,63 to44,15 through47,19:4×5 words,40 bytes. csc34_setBlocks
passes three packed words to `exploration.asm:CopyMapBlocks` and requests
both view-plane updates; CopyMapBlocks uses64-word rows and copies the
current RAM layout, not source asset bytes. Map75's area is0,0..63,63,
parallax256/256 on both layers, zero declared autoscroll, type254. Map37's
area is0,0..31,19, second foreground0,0/background48,44, same parallax and
zero autoscroll, type255. LoadMapArea copies all four declared scroll bytes
into contiguous RAM on each load, so earlier interior ramps do not carry
unchanged across those reloads. Binary layout payloads are absent in the
registered source checkout. No replacement acquisition/decoding is made;
actual copied block values, collision/markers and visible composition
remain Unknown.

## Native scrolling, special sprite and palette services

**Confirmed:** six helpers are selected; sub_5FD7C also calls sub_5FD92.
The code range is `$5FD3A..$5FDB4`; palette data is a separate32-byte
region before ce_5FDD4. Register widths and dbf determine these facts:

| Helper | Complete selected behavior |
| --- | --- |
|csub_5FD3A|d7=7;8 iterations of subq.b1 on layer2 Y, d0=4/Sleep, dbf; rts.|
|sub_5FD4C|d7=7;8 iterations of subq.b1 on layer2 X, d0=4/Sleep, dbf; rts.|
|sub_5FD5E|move.b `$18` to special-sprite layer; rts.|
|sub_5FD66|d7=`$B`;12 iterations, decrement layer1 X byte then layer2 X byte, d0=4/Sleep, dbf; rts.|
|sub_5FD7C|bsr sub_5FD92 first; d7=`$17`;24 iterations incrementing special layer byte then d0=4/Sleep, dbf; rts.|
|sub_5FD92|write1 to FADING_TIMER_BYTE; point a0 at palette_5FDB4 and a1 at PALETTE_4_BASE; d7=CRAM_PALETTE_SIZE32; CopyBytes; a0=PALETTE_1_BASE; UpdateBasePalettesAndBackupCurrent; rts.|

Each decrement/addition is modulo256; no clamp, saturation or sign guard.
From the declared zero scroll bytes the first two ramps finish248
(signed−8); the final exterior's freshly zero X bytes finish244 (signed−12)
on both layers. sub_5FD7C finishes layer24 after successful declaration's
zero seed; sub_5FD5E explicitly assigns24 on the second exterior regardless
of incoming layer. With a changed byte seed b, decrement ramps end
`(b−8)&255`/`(b−12)&255`, and the increment ramp ends`(b+24)&255`.
Sleep4 is requested after every update, including the last:32,32,48,96
VInt waits for the respective loops if every Sleep returns. Sleep preserves
d0 and calls WaitForVInt four times; those counts are not hardware/wall-time
observations or a guarantee that other work completes in that interval.

`exploration.asm`'s autoscroll update at loc_3C44 and its layer2 continuations
read each byte:0 bypasses,128 takes a special camera-derived branch, other
values are sign-extended and added to plane pixel destinations with absolute
speed and scrolling-plane request bits. A changed seed can cross0 or128
during a ramp and select different behavior. Interpreting every byte as a
uniform signed speed misses that128 case. Map/camera/plane services must
run to realize those requests; these loops only alter configuration.

`code/common/tech/graphics/specialsprites.asm` dispatches mapsprite253 as
special sprite2. Its exploration update zero-extends the entity's layer
byte, subtracts it from Y for the first nine sprite records, and from X for
the second nine; frame parity can replace the second group's X with384.
It writes the sprite table beginning SPRITE_46 using
SpecialSpriteData_NazcaShip. Layer is therefore an offset consumed by this
special projection, not a scale, altitude rule, join flag or logical plane
number. Existing compressed sprite/DMA data is not republished here, and
no visible flight or shadow is claimed from these writes.

CopyBytes uses d7 as a byte length and handles overlap;32 means16 words,
not32 colors. sub_5FD92 overwrites only base palette4
`[$FFD0E0,$FFD100)`. UpdateBasePalettesAndBackupCurrent uses CRAM_SIZE128
to copy the complete base bank from PALETTE_1_BASE to itself, then copies
the current bank`[$FFD000,$FFD080)` to backup
`[$FFDF2A,$FFDFAA)`. It writes byte32 at FADING_TIMER_WORD `$FFDFAA`;
the earlier byte1 at `$FFDFAB` is distinct and remains1. Do not turn that
high-byte write into a whole-word assignment or claim immediate CRAM/DMA
completion. UpdateFadingPalette consumes the high byte over later updates.
Raw palette words/assets stay ignored; only ranges/roles are public.

Camera11,6/4,6 requests detach VIEW_TARGET_ENTITY and call
SetCameraDestination/WaitForViewScrollEnd. quake1/3/0 have neither ramp
modifier bit, so csc33 writes QUAKE_AMPLITUDE directly. Fade requests,
text/portrait and audio still rely on their actual services. executeSubroutine
loads the function pointer and calls it, preserving a0 around the call;
it is not a fire-and-forget effect. Trap6 activates VInt_UpdateEntities;
blocked fades, cameras, pointer-idle/movement waits or interrupts can prevent
subsequent work. There is no scene-level timeout or transactional recovery.

## Text, stage roles and custody

**Confirmed:** all eleven rows are single mode, explicit selected entity
speakers. `$80` is the portrait right-side bit; `$00` does not request it.
Neither value selects a different game rule. csc00 resolves live alias/
mapsprite portrait and speech, waits for view scroll, displays the cursor
row, increments it, closes portrait/text and requests Sleep10. With debug
P2 Start text skipping active, it consumes the speaker word without display
or cursor increment; natural debug/input state is not assumed. Short
authored summaries below are navigation aids, not original dialogue copies.

| ID | Speaker selector / modifier | Short summary | Ordered tokens |
| --- | --- | --- | --- |
|3377|Peter7 / `$80`|vibration surprise|`["{W1}"]`|
|3378|128 / `$00`|asks cause|`["{W1}"]`|
|3379|Zynk26 / `$00`|reassurance|`["{W1}"]`|
|3380|Peter7 / `$80`|alarm|`["{N}","{W1}"]`|
|3381|Peter7 / `$00`|ship lifting|`["{N}","{W1}"]`|
|3382|Zynk26 / `$80`|continued ascent|`["{N}","{W1}"]`|
|3383|FOLLOWER_B31 / `$00`|unprecedented journey|`["{N}","{N}","{W1}"]`|
|3384|Zynk26 / `$80`|destination announcement|`["{N}","{W1}"]`|
|3385|128 / `$00`|flight reaction|`["{W1}"]`|
|3386|Peter7 / `$00`|admiration for builders|`["{N}","{W1}"]`|
|3387|Zynk26 / `$80`|thanks|`["{W1}"]`|

Dialogue does not identify the actual current actor behind31, prove
geographic arrival, complete flight, or transfer an item. The full selected
voyage/helpers contain no RemoveItem call, item-slot mutation, join, HP,
999,836,76 or battle-unlock write. Rebuilding stage actors is not recruitment
or inventory consumption. The accepted item-use caller's returning path
also does not remove the Orb. Subject to all calls returning and no external
mutation, this voyage leaves custody unchanged; possession at entry and
later shootdown custody are separate questions.

## Warp consumption and Map59 selection

**Confirmed:** csc07_warp writes type1 word, parameter1=0, then map59,
X63,Y63,Right0 as bytes. csc_end ends the script; warp is a pending request,
not a synchronous scene load. It overwrites those request fields without
testing old request state. Until consumption, CURRENT_MAP is the final75.
On normal return the Orb callback sets d6=−1, its caller closes the menu,
and WaitForEvent/ProcessMapEvent consumes the request. The latter clears
the request word before dispatch; type1/parameter0 makes player idle,
unwinds ExplorationLoop and returns explicit map/position/facing to MainLoop.
Stale/reentrant/interrupted requests and external writes remain Unknown.

MainLoop runs SwitchMap then CheckBattle **before** ExplorationLoop.
The complete flagswitched table has no source-map59 row, so this request
stays59. CheckBattle scans all45 seven-byte battle rows in order, tests
map byte and unlock400+index, then trigger X/Y (255 means wildcard).
The complete selected rows from `data/battles/global/battlemapcoords.asm` are:

| Battle index / order | Map,X,Y,width,height,triggerX,triggerY | Unlock / completion |
| --- | --- | --- |
|42, first matching map|59,0,0,28,38,255,255|442 /542|
|43, second matching map|59,0,0,28,38,255,255|443 /543|

Both match63,63 because those are trigger wildcards; the rectangle is the
battle-area seed, not an admission bounds test. If442 is set,42 wins even
with443 set. If442 is clear and443 set,43 wins. If both are clear, d7=−1
and fresh exploration begins. There is **no Map59 row for Battle44**;
its row is24,0,0,18,32,255,255 with444/544. Changing444 alone cannot admit
it here. Map name and a position outside the28×38 rectangle do not decide
this branch.

For a matching unlocked row, CheckBattle writes the area's four bytes,
then checks completion+100. Clear completion triggers the battle. **Held
completion clears that unlock and still returns the same battle this time**;
it does not continue scanning or reject entry. Do not collapse unlock and
completion into an assumed uncompleted-only gate. Subsequent admission
after that clear is a different call. This slice stops before Battle42/43
initialization for those changed states; Battle44 initialization also stays
outside scope.

With442/443 clear, fresh positive59 exploration heals living/immortal allies,
sets CURRENT_MAP59/CURRENT_BATTLE−1, selects/rebuilds the live setup entities,
loads sprites, clears temporary256..383, sets80, loads map/area, installs
base VInt services and invokes RunMapSetupInitFunction. Those temporary
clears do not clear543/999/443. Map59 area is0,0..26,37 with zero declared
autoscroll and type255; the63,63 seed is not proof of a valid controllable
tile or a separate zone event. No normal exploration control is claimed
before the initializer returns.

`data/maps/mapsetups.asm` selects ms_map59 by default, replaced by
ms_map59_flag543 when543 is held. Both pointer tables select all six
corresponding setup services. The default entity table is terminator-only;
ordinary currently unlocked followers can still be allocated. The alternate
table is a separate stage population; none of its ending scene body is
covered. GetCurrentMapSetup rechecks live flags at each service selection,
so asynchronous flag changes could select different services; no atomic
setup snapshot is promised.

| Live state at init selection (after no battle) | First selected boundary |
| --- | --- |
|543 clear,999 clear|ms_map59_InitFunction reads999, dispatches cs_5EB44. Stop before body.|
|543 clear,999 held,443 clear|Default init takes byte_5EB18, reads443 clear, returns; no scene dispatch.|
|543 clear,999 held,443 held at this later read|Default init dispatches cs_5EB26. Stop before body; a stable443 held earlier would already have admitted Battle43.|
|543 held|Alternate ms_map59_flag543_InitFunction rereads543; if still held dispatches cs_615E6, else returns. Stop before body.|

The default initializer's999-clear path orders five calls:
cs_5EB44→cs_5EBFC→cs_5ED06→cs_5EDB8→cs_5EF60. **Only if all five return**
does it set999, write EGRESS_MAP=MAP_DOJO45, set436,440,442,443, then rts.
Those are reachable later instructions, not effects completed by cs_5FB6A,
warp publication or first dispatch. None sets444 in this prefix. The five
scene bodies and alternate/returned scenes are the successor research
boundary; no text, visuals, crash route or final actor state from them is
asserted here.

## Changed states and actual current consumers

These small source-derived counterexamples are static, not runtime tests:

| Change | Consequence |
| --- | --- |
|76 or836 toggled after admitted launch|No new voyage branch; initial callback admission remains separately guarded. Reloaded stage/followers can depend on other flags.|
|Peter/Zynk HP0|Actions and Zynk setDest still run; ally setPos/setFacing skip. Non-ally128 is not ally HP-filtered.|
|Alias26 or31 missing/stale|Source resolver can target player/another slot; there is no rejection in selected actions/dest/text. Rebuild can replace aliases later.|
|On75, only65/70 held versus only70 held|Continuous prefix-plus-ordinary walk gives Caravan1/Astral2 versus Astral1; other flags shift slots and later31 rows can overwrite its alias. With no admitted31 row it remains0;3383 is not hardcoded Astral.|
|999 held injected into voyage|No branch in voyage; live37 entity reload chooses alternate table. Ordinary item callback would have returned before launch.|
|Scroll seed0,8,128,255; layer250|Byte arithmetic wraps; a ramp may pass0/128 service cases; layer250+24 wraps18. Explicit second exterior layer assignment resets24.|
|Old request present before final warp|Selected warp overwrites type/parameters; no preservation, queue or collision guard. No warp command reached if an earlier wait never returns.|
|442/443 both set|Map59 picks42 first;542 held clears442 but still picks42 this time.|
|442 clear/443 set/543 set|43 admits, clears443 and still enters this time; alternate exploration init is not reached on that call.|
|Only444 set|No Map59 battle admission; default/alternate init selected by543/999.|

**Confirmed (current code):** `src/sf2tool/remake_exploration_programs.py`
OriginalPrograms can lower selected text, quake, camera target/wait,
motion installation, flags, position and ordinary queue-warp instructions.
Representation is not source-equivalent execution or bound Map37/59/75
content. The actual cs_5FB6A compile raises
`ValueError: special cutscene entity not admitted` at its first
loadMapEntities ce_5FDD4. scene_entities rejects mapsprites at/above the
special threshold. Its retained75-instruction partial ends with native
mapLoad after earlier native setDest instructions; **that partial is not a
successful program**. No source/implementation is changed to bypass this.

Separate selected helper lowerings return these objects successfully,
but retain unsupported work:

| Selected symbol | Emitted operations / limitation |
| --- | --- |
|csub_5FD3A /loc_5FD3C|jump; target: native(subq.b), native(jsr), native(dbf), end.|
|sub_5FD4C /loc_5FD4E|jump; target: native(subq.b), native(jsr), native(dbf), end.|
|sub_5FD5E|native(move.b), end.|
|sub_5FD66 /loc_5FD68|jump; target: native(subq.b), native(subq.b), native(jsr), native(dbf), end.|
|sub_5FD7C|native bsr.w only; its terminal native prevents automatic fallthrough to loc_5FD82.|
|loc_5FD82, separately selected|native(addq.b), native(jsr), native(dbf), end; not a connected implemented ramp.|
|sub_5FD92|native(move.b), native(lea), native(lea), native(jsr), native(lea), native(jsr), end.|

Immediate moveq register tracking does not implement native loop state,
Sleep, dbf, palette or RAM writes. The source's complete loops remain the
original evidence. mapLoad, setBlocks, loadEntitiesFromMapSetup and setDest
have no typed compiler branch; their default native-call is unsupported.
No full post-failure suffix or Map59 scene body is compiled here. A first
bounded init probe omitted branch labels and returned missing-target
errors byte_5EB18/return_615E4; those are deliberately incomplete-import
diagnostics, **not established compiler defects**. Selected initializer
source/branch checks replace that probe, without synthesizing runnable
frontiers or expanding into the deferred bodies.

Content's `Scenarios/ExplorationContentReader.cs` maps native-call to
UnsupportedInstruction/UnsupportedEntityAction. Application's
`Runtime/Exploration/ProgramRunner.cs` stops with program-opcode and
EntityActionRunner with entity-action. Typed SetEntityPosition directly
writes motion coordinates/destinations/facing without the original HP
skip or UpdateEntitySprite_0, so its presence does not establish parity.
StartEntityMotion does distinguish slot-timer/manual versus collision-clear
sequence installation and ScriptIdle waits. EntityActionRunner supports
speed/flags/shared size and actual SpriteRequest/SpriteReady waits.

Domain's `Maps/MapEntityAllocation.cs` treats non-ally records generically;
it does not allocate special253 into47. Application SceneEntities uses
that allocator and ordinary flag-selected followers; the compiler rejects
the special stage before that route could prove it. Godot's
`game/src/Exploration/ExplorationPresentation.cs` handles its current
camera/quake/fade/sprite cues and bound audio, not this source's native
scroll registers, eighteen-piece ship projection or palette service.
No selected consumer binds this complete voyage, field item handoff,
Map59 init or later battle content. Existing PrivateBattleEncounterReader
still pins only encounters1/2/3. Minimum future gaps are concrete stage/
special-entity allocation, native motion/map/block/scroll/palette services,
actual guard equivalence and content binding; this does not dispatch that
implementation or authorize runtime.

## Verification, preserved failures and remaining Unknowns

After loading the worktree's current ignored selection in the same
PowerShell process, reproduce the selected audit and clean-head plan:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1108/review.py <registered-source-root> --committed
uv run sf2 verify plan --base origin/main --head HEAD
```

The direct recipe checks complete selected command/helper/table sequences,
constants/use sites, actual compiler outputs and failure, current consumers,
small changed-state derivations, text IDs/ordered tokens, relative links,
UTF-8/LF/private boundary and exact two-path/navigation-only diff. It loads
the existing pair without modification; it executes no game services.
Committed-head receipts, planner suggestions and actual exact-head scoped
CI are reported separately. No normal/full, old comparison, SDK/Godot,
emulator/H3/H4/runtime/screenshots or new fixture/schema/parser is run or
claimed. The planner's suggested command is not an executed pass.

Preserve #1106's local eas_5E2C4 and local @Return compiler failures,
original513d546b/treea9f29733 author0.292s/main0.291s and CI39517713722
which missed the text/close operation-label P2, then correction648a7ca9/
treedc90db27 author0.272s/main0.260s/CI39518456897 accepted in this base.
The main-gate's end-map-script versus actual end naming diagnostic remains
in that handoff; current24 ends,26 jumps27,27 ends and caller cleanup is
conditional. No old audit rerun replaces those completed results.

Preserve this slice's original7ee6a237/treeccb2227d direct0.262s and
CI39520102343 as completed passing results that missed the follower-table
boundary P2. The original review.py split at table_Followers and proved
only the Caravan prefix, not runtime termination. Focused correction via
`uv run --locked python -X utf8 local/issue1108/correction.py <registered-source-root> --committed`
checks the contiguous table through its terminator, actual four-byte
consumer walk/flag/alias ordering, selected changed states and corrected
prose, while retaining original receipts and unchanged navigation. It does
not rerun the prior audit or expand into runtime.

Also retain #1104's count-attribution/no-drop diagnostics and missed-prose
P2 before/after checks, #1100's public-full-text P2 before/after checks,
#1088's clear49→48/untouched checked word, #1096's withdrawn attribution,
#1098 diagnostics and twelve completed old comparison failures through
their existing owning documents/handoffs. They are not a new queue.

**Unknown:** natural launch/arrival reach; actual flags, HP, membership,
aliases, follower/raft/player type, inventory, debug/input, scroll/plane
and pending-request state; binary layout/collision/marker values; interrupted
or reentrant execution; completed VInt/movement/action/DMA/sprite/palette/
camera/fade/portrait/text/audio services; rendered appearance, timing,
usable control and saved persistence. The five later scenes, alternate
ending and battle initialization remain outside this acceptance boundary.
Static completion does not mean a playable voyage or supported subsystem.

Continue at [Nazca ship shootdown and crash handoff](./nazca-ship-shootdown-and-crash-handoff.md)
for the five scenes through fresh Map37's first post-crash dispatch.

Freeze a clean committed/pushed Draft PR, exact base/head/tree, direct/
planner/actual-CI results, failures/Unknowns and ignored handoff. Move
Project to Review / Verify, send the assigned main-gate completion and
stop the writer/processes. Independent acceptance/merge stays with
main-gate; no next Issue, merge, cleanup or archive.
