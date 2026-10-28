# Granseal chamber interactions and ending kiss

Evidence date: 2026-10-22. **Confirmed (static):** Map20's F543 setup
contains six conversations, six searches and three zone callbacks. The two
Minister hints set independent flags; the kiss prompt itself checks none of
F982/F983/F984. Refusal returns to town music. Acceptance runs the complete
approach and kiss-picture service, then calls PlayEndingCredits. This account
stops at that call's entry. Static declarations do not establish natural reach,
service completion, an awakened actor, a saved ending or playability.

## Agreement, provenance and method

Standard rigor for [Issue #1190](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1190)
and [Epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
one complete chamber interaction → choice → kiss → credits-admission account.
Accepted base `f5b98478b18af7f235251a85a864cbd5038ad2c6`, tree
`0785669f990aea301edb4450d2255bb6b2b6bcac`; no stacked dependency. Own this
document and predecessor navigation only. Shared code, schemas, fixtures and
indexes do not change. Credits implementation/scrolling/input, end cutscene,
WitchEnd, extra battle and restart belong to the next scope. No full rendering,
pixel extraction, runtime collection or engine implementation is authorized.

The [predecessor](./granseal-royal-council-and-chamber-admission.md#shared-exits-and-ordinary-chamber-admission)
owns the royal council, F982 publication and ordinary Map20 admission. Reuse
[common scripting](./common-scripting.md), [common maps](./common-maps.md),
[interaction state](./map-event-interaction-state.md),
[request consumption](./map-event-request-consumption.md),
[search semantics](./dojo-exploration-and-sheela-recruitment.md#interaction-church-and-searches)
and [special screens](./special-screens.md#ending-screens).
All original source paths below are relative to read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6/disasm](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
`chamber/` means `data/maps/entries/map20/mapsetups/`, `map/` means
`code/common/scripting/map/`, and `entity/` means the sibling `entity/`.
Addresses/packed words are hexadecimal; intervals are half-open. IDs,
coordinates and waits are decimal. The registered USA ROM SHA-256 remains
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
No new ROM/source-equivalence or hardware claim is made.

The bounded target is six entity callbacks/default, three zones/default,
six descriptions, item default, eleven shared warps, short helpers, complete
cs_EndingKiss and PlayEndingKissSequence. Linear reads of selected source and
the existing 18,285,979-byte init/script pair suffice. Pair provenance stays
`local/issue1005/pair.txt`; no regeneration or full-world preparation. One
selected lowering addresses the concrete raw-choice, closed-eyes and native
kiss-service seams. It preserves native output without retry. Direct checks
target <60s/<512MiB, peak **unmeasured**, each small output <2MiB and all new
ignored output <4MiB under `local/issue1190/`; these are reassessment signals,
not content limits. Work is O(selected source + existing pair), without
acquisition, large intermediate corpora or model/API batches.

The two kiss source files contain4,869 UTF-8 bytes, modestly above the Issue's
approximate3.5KiB estimate; the fixed interaction/script scope is unchanged.
Physical instruction spans and encoded map-command bytes are counted separately.

## Map selection, population and shared exits

**Confirmed:** `data/maps/mapsetups.asm` selects Map20 in order
default → F501 → F609 → F506 → F543, last held flag wins. F982 is **not**
a selector. `chamber/pointertable_543.asm:$631EA..$63202` points to the six
543 sections. For population/admission, F982 changes only the short init:
held invokes cs_633B2 to place Minister128 at27,50/Left; clear leaves his
declared position. Both
then request MUSIC_TOWN, FadeInFromBlack and return. Neither writes983/984.
Interactions reselect live setup; population and init are separate reads.

The complete `s1_entities_543.asm:$63202..$63234` population is below.
Aliases128–133 are logical nonallies. With no persistent followers they
allocate physical slots1–6 after Bowie0; followers can shift these slots.
No logical-ID-as-slot or natural population observation follows.

| Alias / identity | Declared position/facing | Initial action stream |
| --- | --- | --- |
|128 Minister|27,51/Down|eas_Init|
|129 Elis|20,52/Down|eas_InitFixedSprite|
|130 Young Man|16,9/Up|eas_Init|
|131 Guard|35,11/Right|eas_Init|
|132 Guard|39,56/Right|eas_Init|
|133 Maid|22,7/Up|eas_Init|

eas_Init sets accel factors0, speed32/32, acceleration/deceleration off,
obstruction bit7 on, collision-check bits5/6 clear, autofacing on, orientation
0/Up in FLAGS_B low two bits, immersion clear, sprite size24, refresh and jump
idle. ac_orientUp encodes opcode$1B/operand0; entityscriptengine_2.asm's
esc1B_setEntityOrientation changes FLAGS_B, not the independent FACING field.
Both eas_Init and Elis's InitFixedSprite retain the incoming actual facing:
esc0A_updateSprite reads FACING into D6, then ChangeEntityMapsprite writes
that same value back. ac_setFacing/esc22 instead writes FACING explicitly.
Later movement/autofacing can also change it; orientation0 does not mean
facingUp. InitFixedSprite additionally disables autofacing and animation before
refresh/idle; it does not set sleep status or HP. F982's nonally setPos has no
ally-HP suppression.

Map20's chest table `$A5434..$A5436` and other-item table `$A5436..$A5438`
each contain only endWord. They supply no map-specific collectible. The
shared warp table `data/maps/entries/map20/6-warp-events.asm:$A53DA..$A5434`
contains all eleven rows below, each warpNoScroll. Map19=Castle2F,
Map21=Castle3F; MAP_CURRENT retains current map20. Coordinate declarations
do not establish a passable path or that the corresponding layout marker fires.

| Trigger | Destination | Facing |
| --- | --- | --- |
|38,10|Map19,38,8|Left|
|19,15|Map19,19,16|Down|
|35,15|Map19,35,16|Down|
|3,36|Map21,3,16|Right|
|23,37|Map19,23,3|Left|
|6,37|Map19,6,2|Left|
|25,35|current,25,50|Right|
|36,40|Map19,35,7|Right|
|39,39|current,39,53|Left|
|39,53|current,39,39|Left|
|25,50|current,25,35|Right|

## Conversations, wrapper and searches

**Confirmed:** `s2_entityevents_543.asm:$63234..$6329A` declares the six
routes below followed by default → Map20_21F_DefaultEntityEvent/rts.
The second msEntityEvent operand is a **bit mask**, despite direction names:
Down3 turns and restores, Right0 does neither, Up1 turns without restoration.
It is not a required player-facing direction. Callback number below abbreviates
Map20_21F_EntityEventN; all paths return with rts.

| Alias / callback | Mask | Complete callback work |
| --- | --- | --- |
|128 /0|Down3|Check982: clear → txt3942,3943,3944 → branch return; held → byte_63266: txt4010,4011 → return. No flag write.|
|129 /1|Right0|D0=1 → j_ClosePortraitEyes → txt4015 → return.|
|130 /2|Up1|txt4016 → return.|
|131 /3|Down3|txt4022 → return.|
|132 /4|Down3|txt4023 → return.|
|133 /5|Down3|txt4024,4025 → return.|

The actual `map/mapsetupsfunctions_1.asm:RunMapSetupEntityEvent` order is:
store EVENT_RELATIVE_POSITION from incoming D2; select live setup/row; look up
and store speech SFX/portrait; if signed portrait≥0 display it; resolve physical
alias; if mask bit0, WaitForVInt then turn opposite the incoming player facing;
**then** CLEAR_FLAG VInt_UpdateEntities → callback; if mask bit1 restore original
facing; close portrait/text; reactivate entity updates. A script trap can enable
services during a callback. A default rts still has the common wrapper. Portrait
and optional turn must not be moved after disabling updates in the account.

`entity/getentityportaitandspeechsfx.asm:$45638..$4567A` resolves the live
actor's map sprite and searches `data/spritedialogproperties.asm`. Declared
sprites give Minister/Guard no portrait + DIALOG_BLEEP_5, Young Man none +2,
Maid none +3, Elis portrait ELIS +2. A changed live sprite can change the lookup;
unmatched sprites use PORTRAIT_DEFAULT/DIALOG_BLEEP_6. These are table-derived
bindings, not observed portraits or voices.

`j_ClosePortraitEyes` aliases `code/common/menus/portraitfunctions.asm:ClosePortraitEyes`.
It clears BLINK_CONTROL_TOGGLE, waits one VInt, uses D0 bit0 for the blink
tiles and bit1 for mouth tiles, and updates both through UpdatePortrait.
D0=1 selects closed eyes and the zero mouth variant. It writes no combatant
HP/status or F982–984. The sleep-like speech and fixed sprite are presentation
requests, not evidence of a persistent sleep condition or its removal.

`s4_descriptions_543.asm:$63364..$6339A` loads D3=$FD7=4055 and calls
DisplayAreaDescription with byte_63374. msDesc emits X,Y,0,0,kind,offset.
The actual consumer reads row byte4 into D0 and byte5 into D1, **adds423**
to D0 and displays that investigation, then displays D1+D3. Matching is exact X/Y, six-byte stride;
all these rows use the ordinary text path, with no D6-only/function predicate.
End/no-match returns D7=0; matched completion closes windows and returns D7=−1.
The chain is kind → actual byte4/+423 use → gamescript meaning → prose, not
token similarity. This matters because unrelated investigation lines can share
the same ordered controls.

| Search X,Y | kind → investigation ID/meaning | offset → description ID/author paraphrase |
| --- | --- | --- |
|23,6|4 →427 shelves|0 →4055 body-building book|
|24,52|4 →427 shelves|1 →4056 masterpieces book|
|25,52|4 →427 shelves|2 →4057 castle-romance book|
|19,50|7 →430 closet|3 →4058 silk fragment; leader becomes flustered|
|17,36|4 →427 shelves|4 →4059 rug-making book|
|20,52|1 →424 generic investigation|5 →4060 Elis sleeping quietly|

Search routing still requires CheckArea's ahead coordinate/layout-kind path;
a listed actor or coordinate does not prove access. Empty chest/other tables
do not create an award. `s5_itemevents_543.asm:$6339A..$633A2` has only
default → Map20_21F_DefaultItemEvent0/rts. RunMapSetupItemEvent clears speech,
masks the item index, stores incoming Y in EVENT_RELATIVE_POSITION, initializes
D6=0 and selects this default. Return leaves D6=0 for generic item-use fallback;
it is not special-use success, consumption or a kiss trigger. Common closure
then waits a VInt and physical0 arrival, as owned by the linked search/request
accounts. No map-specific item/collection flag is added here.

### Complete dialogue bindings and ordered controls

`data/scripting/text/gamescript.txt` owns these IDs. Meanings are author
paraphrases; dialogue's respect, future rule, sleep and emotion are attributed
speech/narration, separate from executable writes. txt uses its literal ID;
there is no nextText cursor ledger in these callbacks. Controls are in exact
source order. N=newline, W1/W2=text wait controls, CLEAR=text clear; NAME and
LEADER are substitutions. No full translated dialogue is republished.

| ID | Speaker/context | Author paraphrase | Ordered controls |
| --- | --- | --- | --- |
|3942|Minister, clear982 entity|Calls leader|LEADER,W2|
|3943|same|Waiting for Astral|W2|
|3944|same|Asks leader to see Astral|LEADER,N,W1|
|4009|Minister, zone1|Addresses leader with respect|LEADER,N,W2|
|4010|Minister, entity held982 or zone1|Acknowledges burden for kingdom|N,W2|
|4011|same|Hopes leader rules with Elis|N,N,W1|
|4012|Minister, zone2|Urges leader to kiss her|LEADER,N,W1|
|4014|zone0, speech word cleared|Princess asleep; kiss question|N|
|4015|Elis, entity1|Sleeping sound|W1|
|4016|Young Man|Firewood work and sore back|N,W1|
|4022|Guard131|Watching east tower|N,W1|
|4023|Guard132|Admires Parmecia view|N,W1|
|4024|Maid|Startled, recognizes leader|N,LEADER,W2|
|4025|Maid|Would not scream if she recognized him|N,W1|
|424|search introduction kind1|Generic investigation|NAME,W2,CLEAR|
|427|search introduction kind4|Investigates shelves|NAME,N,W2,CLEAR|
|430|search introduction kind7|Investigates closet|NAME,N,W2,CLEAR|
|4055|search23,6|Body-building book|N,W1|
|4056|search24,52|Masterpieces book|N,W1|
|4057|search25,52|Castle-romance book|N,W1|
|4058|search19,50|Silk fragment and embarrassment|N,LEADER,W1|
|4059|search17,36|Rug-making book|N,W1|
|4060|search20,52|Sleeping princess|N,W1|

## Zone admission, flags and complete choice route

**Confirmed:** `s3_zoneevents_543.asm:$6329A..$63364` declares ordered
19,52 → ZoneEvent0; 26,255 → ZoneEvent1; 27,52 → ZoneEvent2; default →
Map20_21F_DefaultZoneEvent/rts. 255 is a wildcard Y for the second row,
not a literal off-map coordinate. The first/third rows require both coordinates.
RunMapSetupZoneEvent scans four-byte rows in order, uses byteFF as an axis
wildcard and FD as default, adds the table-relative callback displacement,
calls it, closes portrait/text, waits a VInt and physical0 motion completion.
These wrapper suffixes apply to returning/skipped/default paths. The accepted
kiss path instead proceeds into the ending chain before any ordinary return.

The ordinary producer is `entity/entityscriptengine_2.asm:esc02_controlCharacter`:
candidate layout word & $3C00=$1400 publishes MAP_EVENT_ZONE_EVENT and
coordinates from current destination plus the attempted tile delta, divided
by384. Obstruction, door/layout and later movement decisions remain part of
that producer. A marker request does not itself establish successful travel.
`code/gameflow/exploration/explorationfunctions_2.asm:ProcessMapEventType6_ZoneEvent`
installs Bowie eas_Init, loads request X/Y from MAP_EVENT_PARAM_1/3 into D1/D2,
and calls j_RunMapSetupZoneEvent. A table row alone is not a marker inventory,
reachability proof or continuously polled coordinate trigger.

| Callback | Complete condition/order | Repetition and publication |
| --- | --- | --- |
|ZoneEvent1,26/*|Check982; clear →rts. Held →check983; held →rts. Otherwise save CURRENT_SPEECH_SFX into SPEECH_SFX_COPY; lookup128; store CURRENT_PORTRAIT=D1 then CURRENT_SPEECH_SFX=D2; txt4009,4010,4011; set983; rts.|Only this callback sets983. No local portrait-display call. Repeating after983 is silent; interruption before set983 can leave it clear.|
|ZoneEvent2,27/52|Check984; held →rts. Clear →script cs_6335E (setFacing128,Down; end); save speech copy; lookup128; store portrait then speech; txt4012; set984; rts.|No982/983 check. Helper is six encoded bytes and nonally facing bypasses ally-HP check. Repeating after984 is silent;983 is unchanged.|
|ZoneEvent0,19/52|No flag predicate. Complete route below.|No completion/choice flag write. No dependency on prior hints is encoded.|

The saved speech word is not locally restored by zone1/2 or the generic zone
wrapper. Portrait/text closure is not a promise to restore all speech RAM.
Neither hint clears another flag; F982 retains the predecessor's init meaning.
All eight supplied 982/983/984 combinations preserve these independent
conditions: zone1 runs only held982/clear983, zone2 only clear984, zone0 always
enters when dispatched. This is branch analysis, not a natural state history.

ZoneEvent0's complete order is:

1. sndCom SOUND_COMMAND_FADE_OUT.
2. script cs_632EA `$632EA..$632FA`: setActscriptWait Bowie,eas_Init;
   csWait1; shiver Bowie; csc_end. This resets the player's action settings
   through the serviced init, then requests the gesture. It writes no story flag.
3. Clear the **word** CURRENT_SPEECH_SFX; txt4014.
4. jsr j_YesNoPrompt → tst.w D0 → bne byte_632E0.
5. D0≠0: SOUND_COMMAND_INIT_DRIVER → MUSIC_TOWN → rts. No map reload,
   flag reset, inverse action-settings restoration or saved choice follows.
6. D0=0: script cs_EndingKiss; after normal return jsr PlayEndingCredits.
   That call's **entry** is this account's stopping point.

`j_YesNoPrompt` resolves to `code/common/menus/yesnoprompt.asm:YesNoPrompt`
`$15284`, sharing alt_YesNoPrompt's body. It initially selects Yes (choice
byte0), optionally waits for held entry input release, gives Left Yes0 and
Right NoFF, B forces NoFF, C/A confirm the current choice. It closes/deletes
its menu and sign-extends the choice byte into D0: Yes0, No/cancel−1. The
callback tests the full word for nonzero. This is a raw return-register branch,
distinct from map-script yesNo's result flag; it writes no kiss/marriage flag.
After refusal,983/984 remain as they were, and a later dispatch can ask again.
There is no promise that a stationary player automatically retriggers it.

For next-owner navigation only, after credits returns the caller requests
D0=−1 → PlayIntroOrEndCutscene → tail jmp WitchEnd. Those implementations,
credits completion/input, extra battle and restart are deliberately unanalysed.

## Complete approach script and motion boundary

**Confirmed:** `chamber/scripts_2.asm:cs_EndingKiss:$633C4..$6343C`
is120 physical bytes. The retained corpus attributes22 map commands and84
encoded command bytes;36 inline action bytes account for the difference.
The following ledger includes every command and every inline action.
W installs and waits for the named actor's script pointer to reach eas_Idle;
A installs without that wait. Each entityActions includes endActions.

| Order | Ordered request |
| --- | --- |
|1|hideText: close portrait then text|
|2|setCameraEntity CAMERA_NEUTRAL=$FFFF|
|3|setFacing Bowie,Right|
|4|W custom Bowie: ac_setSpeed8,8 → ac_jump eas_Idle → ac_end|
|5|csWait30|
|6|playSound MUSIC_ENDING|
|7|A entityActions Bowie: moveUpRight1 → endActions|
|8|csWait10|
|9|W setActscript Bowie,eas_StopMoving|
|10|csWait80|
|11|A entityActions Bowie: moveRight1 → endActions|
|12|csWait10|
|13|W setActscript Bowie,eas_StopMoving|
|14|stopEntity Bowie|
|15|csWait50|
|16|W custom Bowie: ac_setSpeed4,4 → ac_jump eas_Idle → ac_end|
|17|A entityActions Bowie: moveRight1 → endActions|
|18|csWait16|
|19|W setActscript Bowie,eas_StopMoving|
|20|csWait120|
|21|executeSubroutine j_PlayEndingKissSequence|
|22|csc_end|

`map/mapscriptengine_1.asm:csc14/csc15/csc2D` resolve the physical actor
and write that **slot byte** to ACTSCRIPTWAITTIMER before installing a stream.
Bowie's slot is0; this is not an arbitrary universal timer reset. csc14
captures inline actions, waits for idle when its control byteFF, then scans
through8080; csc15 installs an external pointer. csc2D additionally ANDs flagsA
with9F, clearing bits5/6, constructs a temporary stream and appends jump idle.
Only the three A motion requests use that collision-bit clear. Custom/external
installation does not add it. Later action commands/jumps clear their timer
through esc_clearTimerGoToNextCommand separately.

moveUpRight1 expands to action04(+1,−1), action01 destination-wait; Right1
to action04(+1,0), action01. `entity/entityscriptengine_2.asm:esc04` multiplies
offsets by384 and adds **current X/Y**, not the prior destination; it checks
HasSameDestinationAsOtherEntity before advancing and writes destination,
travel and velocities via loc_55C8. The A map command returns after installing
the stream; its embedded destination wait is not a map-script join. A later
eas_StopMoving replaces that stream, so no full-tile endpoint is established.

eas_StopMoving is exactly ac_moveRel0,0 → ac_jump eas_Idle: request the
then-current position and enter idle; no destination-wait action. Its W means
the installed stream reaches idle, not that the earlier one-tile request
finishes. Relative0,0 can still meet the destination-conflict branch; live
state/scheduling controls whether and when it proceeds. stopEntity instead
sets ANIMCOUNTER=FF; it does not stop movement or action parsing.

The shared AdjustScriptPointerByCharacterAliveStatus tests ally live HP.
setFacing passes the **byte** actor operand and skips two bytes at HP0;
stopEntity passes the **word** actor operand and skips two bytes at HP0.
Both identify Bowie0 here, but their operand widths must remain distinct.
The nonally128 helper bypasses this ally predicate. Action installation,
speed, camera and native kiss service have no such HP guard. Thus HP0 can
skip Bowie's facing/animation write while later motion/service requests remain.
No supplied HP constant or implied resurrection is valid.

Camera neutral writes VIEW_TARGET_ENTITY=FF without a camera destination or
scroll wait in that command. csc0A preserves the map interpreter's A0 around
the subroutine and resumes only after it returns. On normal script end the
interpreter applies its ordinary view-scroll/view-speed cleanup; it does not
restore the prior camera target, speed32, animation or other changed actor
state. The second custom speed is4/4; no later local restart of animation is
present. Exact motion, interruption prefixes, final coordinates, audio and
elapsed frames remain Unknown. csWait constants are requested sleeps, with
the interpreter's debug skip behavior, not measured natural durations.

## Kiss service: resource/window interface and return

**Confirmed:** the script's single executeSubroutine target is
`code/common/tech/jumpinterfaces/s03_jumpinterface_2.asm:j_PlayEndingKissSequence`,
a jmp PlayEndingKissSequence(pc), resolving to
`code/common/menus/endingkiss.asm:$12DEC..$12F12` (294 physical bytes).
Its single renderer call uses j_DisplayEndingKissPicture →
`code/specialscreens/endkiss/endkissfunctions_0.asm:DisplayEndingKissPicture`
`$2C576..$2C5E4`. A direct implementation-symbol-only caller count would miss
the script's alias site. The service interface/order is:

| Phase | Complete caller-visible requests/state |
| --- | --- |
|Window entry|DEACTIVATE_WINDOW_HIDING byte=FF; increment WINDOW_IS_PRESENT byte; CreateWindow D0=$120E dimensions,D1=$0707 offset; retain returned slot inD4.|
|Layout|18×14 words: top E000×17,E800;12 interior rows E000,16 sequential A640..A6FF tile words,E800; bottom F000×17,F800. 192 picture tiles plus borders; packed dimensions are not byte count.|
|Palette setup|FADING_COUNTER_MAX=5; copy32 bytes current palette3→current4, then current3→base3. Copy p_palette_EndingKiss→current2 and base2; request ApplyVIntCramDma; copy all128 current palette bytes→base1.|
|Blank working tiles|Clear1536 longwords=6144 bytes at FF6802_LOADING_SPACE. DMA $600 words from$FF6802→VRAM$C800, type2; wait queue. DMA $600 words from byte_FF7402=$FF7402→$D400, type2; wait queue. Together these cover the6144-byte blank image buffer.|
|Show/renderer|SetWindowDestination slotD4,$8080; call renderer alias. No explicit WaitForWindowMovementEnd is added here.|
|Return cleanup|After renderer returns, Sleep180; DeleteWindow D4; decrement WINDOW_IS_PRESENT byte; rts. No explicit restoration of prior window-hiding byte, fading maximum, palettes, VRAM/scratch, camera, music or actor fields.|

`code/common/tech/bytecopy.asm:CopyBytes` preserves D7/A0/A1; therefore
the second copy after lea $80(A1) addresses **base palette2**, reusing the same
source, rather than reading the next32 source bytes. sf2const.asm gives
current1/2/3/4 at$FFD000/$FFD020/$FFD040/$FFD060 and corresponding bases
at+$80 (source spells the second base `PALLETE_2_BASE`); sf2enums.asm
gives CRAM_PALETTE_SIZE32 and CRAM_SIZE128.
Copies and DMA calls describe requested state/resource publication, not
observed VDP pixels. WINDOW_IS_PRESENT increment/decrement balances only
normal completion, relative to its incoming byte; no finally/rollback exists.

Pointer owner `code/common/tech/pointers/s13_pointers.asm` binds
p_palette_EndingKiss → palette_EndingKissPicture and p_tiles_EndingKiss →
tiles_EndingKissPicture. `code/specialscreens/endkiss/graphics.asm` includes
the palette and compressed tile resources. The accepted
[special-screen owner](./special-screens.md) records the6,144-byte decoded
kiss stream. No graphic bytes, raster or extracted assets are published here.

The renderer loads the stack-compressed picture to$FF6802, then calls
DrawEndingKissPictureWithPixelFilling with source$FF6802 and VRAM destination
$C800. The pixel-filler returns with rts after its bounded table-driven loop;
it uses12KiB scratch starting$FF0000, including its second bank$FF0C00, and
two queued$600-word DMA requests per outer pass to$C800/$D400 with queue waits.
At outer D7=$10 it calls sub_2C5E4: copy current→base palettes, darken palette1
and palettes3/4, clear FADING_TIMER_BYTE and call UpdateBasePalettesAndBackupCurrent.
The pixel algorithm/material parity stays with special-screens; this confirms
the real call/return/resource interface without starting its historical queue.

After pixel-fill returns, DisplayEndingKissPicture requests Sleep360; clears
eight longwords=32 bytes of base palette1; clears fading timer and calls
UpdateBasePalettesAndBackupCurrent; Sleep870; clears32 longwords=128 bytes
of all base palettes; clears fading timer and calls the same update; rts.
These update/fade requests are not a guarantee of visible black or elapsed
seconds. It returns to the outer Sleep180/delete/decrement, then csc0A resumes,
csc_end returns, and ZoneEvent0 enters PlayEndingCredits. No HP/status, marriage,
awakening or ending-complete flag write is present in these local bodies.

## Actual producer and consumer boundary

**Confirmed:** one selected OriginalPrograms lowering completed with no
exception,17 programs and two cached action streams; full output/native state
is retained. No compile_maps, final package, world publication or execution ran.

| Actual owner | Supported shape / refusal / publication |
| --- | --- |
|[OriginalPrograms](../../src/sf2tool/remake_exploration_programs.py), entity events|Entity0 is8 instructions with clear/held982 suffix branch; five other callbacks retain ordinary literal text bindings. Entity1's first instruction is native jsr for ClosePortraitEyes; later4015 is available text, not reached text.|
|Same producer, zones|Zone1 has10 instructions with live982/983 branches, three128 text bindings and flag983; zone2 has6 with984 branch, facing helper,128 text, flag984. Speech-copy/portrait/SFX stores are collapsed into text speaker bindings; this is not original RAM-store publication.|
|Same producer, raw choice|Zone0 has12 instructions: native sndCom at0, helper call1, native speech-clear2,4014 text/speaker, native jsr/tst.w/bne.s at5–7, kiss call8, native later services9–11. It emits **no typed yes-no or result flag**. First native stops ordinary execution; later text/kiss/credits are not demonstrated reachable. The No/cancel source suffix byte_632E0 is not compiled by this selected output because its native branch is not followed.|
|Same producer, kiss|cs_EndingKiss is23 instructions (hideText becomes two). Typed motion, speed, camera, sound, waits and animation exist; final call resolves compiled j_PlayEndingKissSequence, whose one instruction is native jmp. No native appears in the script itself; that does not support its transitive service. No alias/allowlist repair or second lowering is made.|
|[Map producer](../../src/sf2tool/remake_exploration_maps.py), [final preparation](../../src/sf2tool/remake_exploration_content.py)|Can retain event masks, ordered explicit/default rows, FF wildcard axes, source-zone marker1400/eas_Init and selected setup/population. Selection/admission still requires a prepared package. This run supplied no layout/material/map/event package and proves no543 publication.|
|[Content reader](../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs), [story types](../../remake/src/Sf2.Remake.Application/Content/Scenarios/StoryProgram.cs)|Read typed choice, flag, text/speaker/portrait, camera, motion/animation and action forms; native-call becomes UnsupportedInstruction. Reading typed yes-no support does not convert the source's raw D0 branch.|
|[ProgramRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs), [choice runner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationChoiceRunner.cs)|Unsupported throws program-opcode before Next/commit; prior instruction commits survive. BranchFlag reads live flags and WriteFlag commits individually. Typed ChooseYesNo publishes its named result flag; source Zone0 has no such instruction/flag. Typed face/animation omit original byte/word ally HP guards.|
|[MapEventDispatcher](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapEventDispatcher.cs), [portrait runner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationPortraitRunner.cs)|Existing interaction profile opens portrait before facing wait/entity suppression, restores facing and closes on return; source zones require matching marker/coordinates and run init/closure/arrival waits. Portrait service machinery exists, but selected ClosePortraitEyes remains native. Generic blinking/eyes projection is not this callback's bound service.|
|[EntityActionRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/EntityActionRunner.cs), [EntityMotion](../../remake/src/Sf2.Remake.Domain/Maps/EntityMotion.cs)|Relative move uses current X/Y plus signed offsets×384; named script-idle and motion waits differ. eas_StopMoving lowers to move0,0(no wait), jump,idle. Script installation uses physical-slot timer and optional AND9F; replacements clear typed follower state. Domain motion/speed/animation consumers exist, without a measured kiss schedule or HP predicates.|
|[Adapter view](../../remake/game/src/Exploration/ExplorationSessionView.cs), [presentation](../../remake/game/src/Exploration/ExplorationPresentation.cs), [capabilities](../../remake/docs/capability-status.md)|Projects current text/choice/portrait/motion/camera and errors for admitted content. This observation admitted no chamber package, ending window/pixel service, input, audio or returned credits seam. No SDK/Godot execution occurred.|

**Inferred:** existing primitives can express much of the hints and approach
after explicit content admission and raw-choice/HP/closed-eyes/native-service
work. That is a future scoped integration decision, not authorization here or
an engine-ready claim. No importer, engine, reference, allowlist or support table
was changed. Native refusal and available suffixes remain separate evidence.

## Reproduction, retained failures and open boundary

Ignored `local/issue1190/observe.py` is the **sole** selected lowering and must
not be rerun for ordinary review. `review.py` reads observe-01 and the existing
pair, checks the pin, complete target source/control order/corpus, entity/zone/
description/item/warp mappings, kind→ID→meaning, dialogue tokens, alias/service
interfaces, relevant consumers, changed-state branches, document links,
UTF-8/LF and exact two-file/private diff. It writes only a fresh receipt.
The local recipe and handoff retain precise commands, source byte counts,
resource measurements, exact-head plan/CI and process state. For independent
reproduction, choose an output that **does not exist**:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1190/review.py <configured-source-disasm-root> --receipt local/issue1190/reproduction-01.json --committed
git diff --check origin/main...HEAD
uv run sf2 verify plan --base origin/main --head HEAD
```

Every uv invocation loads current worktree configuration in the same process.
Normal/full/SDK/Godot/emulator/H3/H4, screenshots, old comparison/lowering/render
recipes, pair regeneration and full world are **SKIPPED**, not passed, under
this Issue's documentation-only acceptance. No new ROM observation/blocker is
claimed. ADR0004's relevant source/date/control/width/alias/provenance/consumer/
private checks apply; new maintained parser/schema/tool tests and H3 callbacks
are N/A. This slice's sole lowering had no exception and its native refusals
remain unchanged.

Direct01 failed on the local recipe's Windows path-key separator; direct02
failed because the recipe assumed the normally spelled palette-base label.
The pinned source actually spells it PALLETE_2_BASE. Correcting those local
lookups left source, pair and lowering untouched; direct03 passed. Both failed
receipts remain, with no source/evidence normalization or green-lowering retry.
Pre-freeze self-review also corrected prose that put the byte5 read after the
first description display, and an ambiguous typed-relative-motion formula.
Earlier direct PASS guarded source order but missed those prose expressions;
the current check explicitly binds their corrected wording. No old PASS is
relabelled or removed.

Independent review found a P2 after the author/main direct checks and original
exact-head CI passed: this owner called ac_orientUp "facingUp" and implied
servicing init would replace declared facing. The corrected relation above
distinguishes opcode1B's FLAGS_B orientation bits from opcode22's FACING write
and refresh's retained FACING input. All original PASS receipts remain with
that omission explicit. `local/issue1190/focused-orientation.py` checks only
this macro → dispatch → field → refresh → corrected-prose chain, the bounded
document diff/private boundary and preserved old receipts, using a fresh output.
Do not rerun the full direct recipe, capture, lowering or unrelated gates for
this correction.

Preserve `local/issue1188/handoff.md` and `handoff-initial.md`: original direct
and CI PASS missed kind4/5→427/428 meanings and portrait/turn-before-disable
order; independent P2 findings, focused corrections and direct01 CRLF failure
remain recorded. Preserve roof/list-write refusal, RotateRight's omitted
internal loop and HP-guard gaps. The
[predecessor failure owner](./granseal-royal-council-and-chamber-admission.md#reproduction-retained-failures-and-open-boundary)
retains #1186/#1184 and earlier native/partial/import/comparison failures and
Unknowns. No old receipt or failure is overwritten, rerun or relabelled.

**Unknown:** natural marker/interaction/search/warp reach, live flags/HP/aliases/
followers/inventory/input, interrupted prefixes, action/service scheduling,
exact motion/elapsed frames, rendered window/palette/pixels/portraits/audio,
source-ROM equivalence, saved persistence and original/remake playability.
Credits/endcutscene/WitchEnd implementation remains outside scope. These
Unknowns and special-screens' historical queue do not authorize runtime work.
Freeze a clean pushed Draft PR and stop the writer for independent main-gate
review/merge; same task owns corrections. No merge, cleanup, archive, ref
deletion or next dispatch belongs to this slice.

The [credits, finale and bonus-admission account](./ending-credits-finale-and-bonus-admission.md)
continues through the complete end sequence and the BattleLoop caller's
normal-return reset boundary.
