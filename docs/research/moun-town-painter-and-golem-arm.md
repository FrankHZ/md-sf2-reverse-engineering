# Moun town: residents, the painter and the Arm of Golem

Evidence date: 2026-10-20. **Confirmed (static):** flag830 selects the populated
Map31 setup. Talking to its painter sets832 before the first scene; declining
loops, and later interactions use a different scene. The Arm's appearance834,
award835 and loaded actor137 are separate authorities. Its custom description
requires Search's d6=0 and calls a force-wide inventory helper; it is absent
from the ordinary chest/ground-item tables. **Unknown:** naturally reached
interactions, completed services, actual presentation, usable control and saved
persistence. This catalogue does not deliver a playable town.

## Agreement and evidence route

Standard rigor for [Issue #1096](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1096)
under [#1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053).
Coupled setup/population, choices, inventory and native presentation justify
explicit caller and state review. The outcome is the complete local town
catalogue for subsequent content integration, on accepted base
`1e711690271e00cf29ebe0aac12488dd166cf787`. The accepted
[underground owner](./moun-underground-gyan-and-zynk.md) supplies entry/return
and Gyan/Zynk boundaries. Own this file and navigation only in that owner;
no stacked dependency, shared index/schema/fixture, design or implementation.
Stop at the Map31 outgoing request handoff, before later travel, battle or
Golem recruitment/item use.

Original paths are relative to `disasm/` at clean read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The registered USA ROM has SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`;
no new ROM/H1/runtime equality is claimed. Constants are from `sf2enums.asm`
and `sf2const.asm`. Intervals are half-open, ordinals zero-based, Fnnn means
flag nnn; `map31/` abbreviates `data/maps/entries/map31/`, `map/` and `entity/`
abbreviate `code/common/scripting/map/` and `entity/`.

Reuse [common maps](./common-maps.md), [common scripting](./common-scripting.md),
[event production](./map-event-request-state.md),
[consumption](./map-event-request-consumption.md),
[field search](./field-search-control.md),
[description routing](../design/contracts/map-area-description-routing.md),
and [service interactions](../design/contracts/service-interactions.md).
The [Tristan catalogue](./tristan-entry-interactions-and-exits.md#shops-church-searches-and-map-callbacks)
and [Ketto service owner](./ketto-interactions-and-merchant-relocation.md#shops-priest-and-reload-lifetime)
retain the shared shop/church mechanisms; this slice checks the selected rows
and immediate calls rather than reopening those systems.

Reuse the unchanged 18,285,979-byte init/script pair and its
`local/issue1005/pair.txt` provenance. Bounded linear source/table/text/consumer
reads and selected in-memory lowerings budget less than one minute/512MiB per
audit, with small distinct ignored `local/issue1096/` receipts. No regeneration,
new corpus/platform/environment, runtime/Godot/SDK/emulator, screenshot, old
comparison or blanket suite. Reassess a material contradiction or supporting
tool expansion before scaling. Unknowns alone do not create acquisition work.

## Arrival, profile and the two setups

**Confirmed:** the accepted [Battle33 return owner](./battle33-outcomes-and-zynk-handoff.md#returned-map31-reload-conditions-and-outgoing-requests)
and [underground return owner](./moun-underground-gyan-and-zynk.md#both-return-requests-and-immediate-map31-consequence)
distinguish immediate battle return from explicit-map SwitchMap/CheckBattle.
Map31 has no flag-switched-map substitution. Explicit31 with433 clear enters
exploration; held433 can admit Battle33 even with533 held, and the completed
match clears433. Neither arrival at39 nor830 establishes433's value.
Stop at that admission boundary if another encounter is selected.

For admitted exploration, InitializeMapEntities and the later init select
setups separately. The ordinary load clears temporary256–383 and sets80;
it does not infer832/834/835 from items or past scene completion. The
predecessors own the ordered loading/healing/VInt/request lifecycle.

| Map31 source / interval | Complete selected declaration |
| --- | --- |
| `00-tilesets.asm` `$AB610..$AB616` | Palette0; tilesets0,89,50,53,63. |
| `2-areas.asm` `$AB63E..$AB65E` | Main0,0 through31,38; secondary foreground32,0/background0,0; both parallax256,256 and autoscroll0,0; main type0; MUSIC_TOWN8; terminator. |
| `3-flag-events.asm` `$AB65E..$AB670` | F833 copies source3,39 size1×1 to16,8; F831 copies1,39 size2×2 to26,32; terminator. |
| `4-step-events.asm` `$AB670..$AB69A` | Five doors, below; terminator. |
| `5-roof-events.asm` `$AB69A..$AB6C4` | Five secondary-layer copies, below; terminator. |
| `6-warp-events.asm` `$AB6C4..$AB6EE` | Five eight-byte no-scroll rows; terminator. |
| `7-chest-items.asm` `$AB6EE..$AB6F0` | Terminator only. |
| `8-other-items.asm` `$AB6F0..$AB6F2` | Terminator only. |

| Door trigger / destination | Door source / size | Roof source / size / destination |
| --- | --- | --- |
|11,8|0,39 /1×1|255,255 /5×5 /42,3|
|20,10|0,39 /1×1|255,255 /5×6 /50,4|
|27,18|0,39 /1×1|255,255 /5×6 /56,12|
|21,26|0,39 /1×1|255,255 /4×5 /51,21|
|6,31|0,39 /1×1|255,255 /5×6 /37,25|

The ordinary control consumer checks collision/obstruction and layout kinds;
a `$0400` door copy precedes rereading the resulting kind, `$1000` admits
warp checks, `$1400` admits zone checks. Roof copying has its separate
secondary-layer mechanism. Raw255 sentinels and coordinates are retained;
no selected block rendering/collision decode or naturally traversed door is
claimed. F833/F831 copies and830 setup selection are independent.

`data/maps/mapsetups.asm` has default ms_map31 and830→ms_map31_flag830.
The six-long tables are `$5D40E..$5D426` and `$5D426..$5D43E`:

| Offset / pointer | Clear830 | Held830 |
| --- | --- | --- |
|0 /entities|ms_map31_Entities|ms_map31_flag830_Entities|
|4 /entity events|ms_map31_EntityEvents|ms_map31_flag830_EntityEvents|
|8 /zone events|ms_map31_ZoneEvents|ms_map31_flag830_ZoneEvents|
|12 /descriptions|ms_map31_AreaDescriptions|Same|
|16 /item events|ms_map31_Section5|ms_map31_flag830_Section5|
|20 /init|ms_map31_InitFunction|ms_map31_flag830_InitFunction|

GetCurrentMapSetup reads CURRENT_MAP and live flags each time. Clear830 has
an empty fixed/walking list, default-only entity/zone/item events and RTS
init; it still has the shared description pointer and map-level tables.
Held830 has the residents, Arm zone, same descriptions, default-only item
events and conditional init. No local callback joins Gyan or Zynk, changes
their active flags or installs persistent Zynk following. F76, joined/active
flags and HP/classes independently affect ordinary followers/ally sprites;
they do not choose this setup. Changing830 while loaded can select different
callbacks without rebuilding the physical population or aliases.

## Residents and ordinary interaction

**Confirmed:** `s1_entities.asm` `$5D43E..$5D440` is only msEntitiesEnd.
`s1_entities_830.asm` `$5D440..$5D49A` declares all eleven rows below then
msEntitiesEnd. Event identities are128+row, distinct from physical slots:

| Identity | Mapsprite / X,Y / facing | Initial action / event |
| --- | --- | --- |
|128|YOUNG_MAN /9,11 /Down3|Walking origin9,11 radius2 /Event0|
|129|MERCHANT /27,14 /Down3|eas_Init /Event1, weapon shop|
|130|WORKER /19,11 /Down3|eas_Init /Event2|
|131|BOY /11,23 /Left2|Walking origin11,23 radius2 /Event3|
|132|GIRL /20,24 /Right0|Walking origin20,24 radius1 /Event4|
|133|YOUNG_WOMAN /7,27 /Down3|eas_Init /Event5|
|134|YOUNG_WOMAN /12,5 /Down3|eas_Init /Event6|
|135|PRIEST /20,6 /Down3|eas_Init /Event7, church|
|136|YOUNG_WOMAN /29,6 /Up1|eas_Init /Event8, painter|
|137|POSE2=174 /63,63 /Down3|eas_Init /default RTS; custom Arm search|
|138|MERCHANT /26,14 /Down3|eas_Init /Event9, item shop|

InitializeMapEntities reconstructs admitted ordinary followers, main and these
declarations with fresh aliases. The accepted underground owner retains
follower flags67/82/76 and FOLLOWER_B's74/68/70, ally alias suppression and
HP/class sprites. Eleven rows do not mean eleven total entities or physical
slots128..138. Arm137 is an ordinary nonally row, not a joined Golem member;
no initial exclusion based on835 removes its off-map declaration.

The selected eas_Init sets acceleration factors0,0, speed32,32,
acceleration/deceleration OFF, obstructable ON, entity/map uncollidable OFF,
autoFacing ON, orientUp, immersed OFF, size24 and sprite update, then jumps
eas_Idle (wait1/branch). Walking patches the50-byte eas_Walking template at
22/24/26 with origin/radius: wait30, speed0,0, factors1,1,
acceleration/deceleration−2,ON, obstructable/map/entity uncollidable ON,
randomWalk, waitDest, wait20, branch. The consumer uses cardinal random
candidates bounded by origin±radius, layout/destination checks and its
DOWN=3 DBF retry counter; it can yield without movement. No random path is
asserted.

The real ordinary entity caller is
`code/gameflow/exploration/explorationvints.asm:ProcessPlayerAction`:
idle/wait main and view; A enters FieldMenu, ordinary C uses
GetActivatedEntity→GetEntityEventIndex→j_RunMapSetupEntityEvent, while no
entity instead uses CheckArea(d6=1) then possibly FieldMenu. Caravan/debug
branches remain separate. GetActivatedEntity scans48 physical entities,
excludes the view target and EXPLORATION_ENTITIES followers, uses the faced
sample/`$2800` counter extension and first Manhattan-distance<384 match.
The accepted [underground caller detail](./moun-underground-gyan-and-zynk.md#event-callers-and-ordinary-conversations)
retains the nonzero-view addressing and reverse-alias scan edge cases.

RunMapSetupEntityEvent reselects the live setup, matches identity then default,
prefills portrait/speech from current mapsprite and stores player facing in
EVENT_RELATIVE_POSITION. All ten explicit rows use source token UP=1 as a
**packed event flag byte**, not a facing precondition: bit0 turns the actor
toward the player before callback; bit1 would restore it afterward, but is
clear here. The wrapper suppresses VInt_UpdateEntities around the callback,
then closes portrait/text and reactivates it. This is distinct from a script's
own entity activation and wait work. Alias lookup, actual mapsprite, missing
population or service failure can prevent the apparent conversation.

| Handler in `s2_entityevents_830.asm` | Complete local content/effect |
| --- | --- |
|Event0 /128|2032 then2033: former busy town versus current ghost town.|
|Event1 /129|Byte CURRENT_SHOP_INDEX=SHOP_WEAPON_MOUN12, j_ShopMenu, RTS.|
|Event2 /130|2034: gratitude for saving the town; RTS.|
|Event3 /131|2035 then2036: ancient golem origins and good/bad golems; RTS.|
|Event4 /132|2040, j_YesNoPrompt, btst bit0 of d0: zero→2041 (moving/searching for its body); nonzero→2042 (where it is/surprises); RTS. No834/835 test/write.|
|Event5 /133|2037: missing NAME;26 (Zynk); RTS, no membership/follower test.|
|Event6 /134|2038 then2039: Arm movement and request to remove it; RTS, no prompt/award.|
|Event7 /135|j_ChurchMenu, RTS; no local quest/HP gate.|
|Event8 /136|832 clear: set832 then cs_5D652; held: cs_5D724; RTS.|
|Event9 /138|Byte CURRENT_SHOP_INDEX=SHOP_ITEM_MOUN27, j_ShopMenu, RTS.|

Entity/default table intervals are `$5D49A..$5D49E` and
`$5D49E..$5D552`. Map31_DefaultEntityEvent is RTS, as are nullsub_5D540 and
nullsub_5D572; neither nullsub is a selected resident callback.

## Selected shops and church

**Confirmed:** `code/common/menus/shop/shopactions.asm:GetShopInventoryAddress`
reads the byte selector. For each preceding row it reads one count byte and
skips that many item bytes; selector0 returns the first row immediately.
`sf2macros.asm:shopInventory` emits dc.b narg followed by that many item
bytes, without aFFFF terminator. `data/stats/items/shopinventories.asm`
rows12 and27 are:

| Selector / category row | Ordered inventory |
| --- | --- |
|12 /Weapon shop13|BUSTER_SWORD, GREAT_SWORD, JAVELIN, CHROME_LANCE, LARGE_AXE, GREAT_AXE, ASSAULT_SHELL, GREAT_SHOT, GUARDIAN_STAFF, INDRA_STAFF, BRASS_KNUCKLES, IRON_KNUCKLES|
|27 /Item shop13|MEDICAL_HERB, HEALING_SEED, HEALING_DROP, ANTIDOTE, FAIRY_POWDER, ANGEL_WING|

The selector is zero-based overall; the source category comments are one-based.
Stock is checked from actual rows, not inferred from the Moun label or the
preceding town's similar stock. Item definitions/prices, current gold,
inventory, deals and equipped/cursed state belong to the unchanged shared
shop owner. The local callback neither grants stock nor proves a completed
buy/deals/sell/repair action. The priest's direct ChurchMenu call delegates
raise/cure/promote/save, costs, membership and cancellation to the shared
church/save owners. There is no local automatic healing, save or town-specific
church choice. Shops and church remain calls requiring their real service UI.

## Painter: first choice, movement, revisit and picture

**Confirmed:** Event8 writes832 **before** calling cs_5D652. It does not wait
for a Yes answer, a finished portrait, movement or the native picture display.
Its later path cs_5D724 uses2030 then2031 and ends; it does not prompt, move,
display the painting or clear832. Interruption after the flag write can
therefore make a later visit take the repeat path despite incomplete work.

`cs_5D652` (`$5D652..$5D712`) has34 top-level corpus commands,164 encoded
command bytes plus seven four-byte inline streams=192 physical bytes. Its
interior label cs_5D65E is the choice-loop target, not a separately bounded
corpus program. The complete command order is:

| Corpus index | Operation |
| --- | --- |
|0–2|textCursor2024; nextSingleText $0,136 (2024); nextText $0,136 (2025).|
|3–5 /cs_5D65E|textCursor2027; yesNo; jumpIfFlagClear89→cs_5D712.|
|6–8|hideText; nextSingleText136 (2027); nextSingleText136 (2028).|
|9–10|setActscriptWait Peter7,eas_Init; FOLLOWER_B31,eas_Init.|
|11–16|setDest Bowie0→28,3; setFacing Bowie Down; setDest Peter→27,4; FOLLOWER_B→27,5; setFacing136 Up; setActscriptWait136,eas_DeactivateAutoFacing.|
|17–19|entityActionsWait136 moveLeft1/endActions; then moveRight1/endActions; csWait60.|
|20–21|entityActionsWait136 moveLeft1/endActions; csWait30.|
|22–25|entityActionsWait136 moveRight1, then moveLeft1, then moveRight1 (separate streams/endActions); csWait60.|
|26–28|setActscriptWait136,eas_InitSlow; setFacing136 Up; nextSingleText136 (2029).|
|29–30|entityActionsWait136 moveRight1/endActions; setFacing136 Left.|
|31–33|followEntity Peter→Bowie, offset2; FOLLOWER_B→Peter, offset2; csc_end.|

Each inline header is opcode$2D, actor136,wait$FF (4 command bytes), then
direction/count (Left2 or Right0,1) and endActions/$8080 (4 additional bytes).
The seven movements are Left,Right,Left,Right,Left,Right,Right; net one tile
Right **if all complete from the supplied start**. This is not a fixed final
position guarantee: prior movement, reentry, interference and service state
matter. csWait operands60/30/60 are engine waits, not measured wall time.

The named actions are fully followed: eas_DeactivateAutoFacing is
autoFacing OFF→jump eas_Idle, without falling into subsequent source code.
eas_InitSlow sets speed16,16, factors1,1, acceleration/deceleration−2,ON,
obstructable ON, both uncollidable OFF, autoFacing ON, orientUp, immersed OFF,
size24, refresh sprite→idle. eas_Init is as above. The map script wait
consumer csc15 reads actor/wait bytes and an action-address longword, installs
it and, for wait$FF, spins until ACTSCRIPTADDR equals eas_Idle. csc2D reads
actor/wait bytes, masks FLAGS_A with$9F, sign-extends each movement count,
dispatches its low-nibble direction, emits move-relative actions followed by
a jump to eas_Idle, and waits for that address when requested. Neither has
the separate ally-alive guard used by setPos/setFacing. The entity action/
motion service still owns destination completion; a wait-prefixed macro does
not independently prove arrival or repair a missing alias.

`map/mapscriptengine_2.asm:csc11_promptYesNoForStoryFlow` calls j_YesNoPrompt,
tests the returned **word**, sets89 for zero/Yes, clears89 otherwise and
Sleep10. The helper starts Yes0; Left selects0, Right selects$FF, B confirms
$FF, C/A confirm current choice, and cleanup sign-extends the byte to a word
(No/B=−1). Held raw input at entry first requires release. Thus clear89
branches to cs_5D712 (`$5D712..$5D724`,5 commands/18 bytes): cursor2026,
hideText, nextText136 (2026), jump cs_5D65E, trailing csc_end. The jump makes
that final end unreachable in ordinary flow. Declining/B repeats the plea
and the choice; it does not exit or undo832. Returning through the loop resets
cursor2027, so Yes later emits the intended2027/2028/2029 sequence. This
choice is not either dialogue W1/W2 input acknowledgement.

setDest consumes an actor **word**, computes tile×384 destinations/travel
and speed32, then waits unless actor bit15 is set; all three selected actors
have that bit clear. setPos/setFacing instead inspect the actor byte for the
shared ally-alive skip. followEntity reads the **high byte** of its word
actor operand for that check: Peter7 and FOLLOWER_B31 both have high byte0,
so the original guard tests Bowie's HP, not their individual HP. On admitted
execution each resolves aliases, reads signed table offset2=(−24,0), and
AddFollower copies/patches a42-byte follower action. These are scene-local
Peter→Bowie and B→Peter chains, with no membership join or persistent
follower-flag change. Dead/unjoined/missing aliases and existing cached
follower lists remain separate cases, not automatic scene exclusion.

On held830 load, `s6_initfunction_830.asm` first checks832. Held executes
cs_5D63C (`$5D63C..$5D644`,2 commands/8 bytes): setPos136,30,6,Left;
csc_end. Clear832 leaves the fresh29,6 declaration alone. Actor136 bypasses
the ally HP guard. This reload placement occurs for **any** held832, including
an interrupted first scene. Changing832 while loaded does not move the actor.

Description29,5 tests832; clear returns without a picture, held calls
cs_5D732 (`$5D732..$5D73E`,4 commands/12 bytes): executeSubroutine sub_5D73E,
csWait180, hideText, csc_end. The native body `$5D73E..$5D756` saves
d0–d2/a0 as longwords, supplies portrait52/PORTRAIT_BOWIE_PAINTING in d0.w,
d1=−1, d2.w=0 to j_OpenPortraitWindow, restores registers and returns.
`code/common/menus/portraitwindow.asm:OpenPortraitWindow` can return early
when a portrait window already exists; otherwise d1.b selects the right side
and d2.b the mirror toggle. This is a direct portrait-index call, not a
resident portrait lookup. csc0A executes the longword target with the map
script return context; the description wrapper later closes portrait/text.
No new832 write or actual displayed painting is established by that call.

## Arm of Golem: appearance and Search admission

**Confirmed:** the held830 zone table `$5D556..$5D574` has one row5,2 plus
the shared RTS default; base `$5D552..$5D556` has only default. Ordinary
esc02_controlCharacter produces type6 only after the `$1400` layout/collision
path. ProcessMapEvent clears the stored type, installs main eas_Init, and
RunMapSetupZoneEvent reselects live setup and matches coordinates/default;
the wrapper closes windows and waits main after one VInt. Loading at5,2
alone is not evidence of automatically running this callback.

Map31_33E_ZoneEvent0 checks834: held returns; clear runs cs_5D644 then
sets834 **after return**. The script (`$5D644..$5D652`,3 commands/14 bytes)
sets actor137 at8,3 Left, applies animEntityFX137,SLIDE_IN5, ends. It does not
test835, grant an item, join a member or allocate a new actor. Actor137 was
already declared off map. animEntityFX's consumer reads actor/effect words,
selects the crop-transition table by effect×8, and for non-mosaic effects
uses LoadMapsprite→ApplySpriteCropEffect→DmaMapsprite with two VInt waits
per DBF iteration (counter22,23 iterations). That call chain does not prove
the slide's rendered completion or visibility.

The shared area table is `$5D574..$5D60E`; wrapper supplies d3.w=$1045
(4165), a0=byte_5D584 and jumps DisplayAreaDescription. Its full seven rows:

| Coordinate | Row shape / effect |
| --- | --- |
|8,26|msDesc investigation4→427 (bookshelves), description0→4165 (Nazca book).|
|20,22|msDesc investigation4→427, description1→4166 (painting book).|
|4,33|msDesc investigation5→428 (well), description2→4167 (dry well).|
|25,18|msDesc investigation2→425 (sign), description3→4168 (weapon sign).|
|26,18|msDesc investigation2→425, description4→4169 (item sign).|
|29,5|msDescFunction byte2=0/byte3=1→Map31_DescFunc0;832-gated picture above.|
|8,3|msDescFunctionD6 byte2=1/byte3=1→Map31_DescFunc1; Arm award below.|

DisplayAreaDescription packs low-byte X/Y, matches six-byte rows untilFD00;
nonzero byte2 requires **d6.w=0**, not d6=1 or a bit mask test. Byte3 chooses
callback rather than the two text calls; callbacks save/restore d6.w, then
close portrait/text and set d7=−1 even when the callback was a flag-gated
no-op. Literal rows emit423+investigation then4165+offset. No facing field
exists in these rows; faced-position production is upstream.

The actual callers resolve the source's question about d6:

- Ordinary no-entity C in ProcessPlayerAction supplies moveq#1,d6 before
  direct CheckArea. It cannot select the Arm's nonzero-byte2 row.
- `code/common/menus/main/mainactions.asm:FieldMenu` Search branch executes
  clr.w d6→j_CheckArea→@ExitMain. This path can admit the Arm description.
  A opens the menu directly; C may reach it after no applicable quick action.
- CheckArea's actual j_RunMapSetupAreaDescription call occurs only after
  chest$1800, vase$2C00, barrel$3000, bookshelf$3400 and area-item$1C00
  branches. The remaining layout kind must reach the description dispatcher;
  empty pickup tables do not bypass this priority. Negative VIEW_TARGET_ENTITY
  returns early; its actual nonzero-view address calculation remains a shared
  caller caveat. Facing offsets and signed division384 supply coordinates.

Therefore a coordinate and visible sprite alone do not establish a natural
Search at8,3. Description29,5's byte2=0 has no d6 exclusion; a matched but
clear832 callback still counts as handled and closes windows.

## Award, removal and reload are separate state changes

**Confirmed:** Map31_DescFunc1 first requires834 held and835 clear. Then:

| Ordered boundary | Source effect |
| --- | --- |
|Before choice|txt2017 (found Arm), txt2018 (pick-up question), j_YesNoPrompt.|
|No/B|tst.w d0/bne→txt2023 (looking at Arm), RTS; no flag or item change.|
|Yes|d0.w=ITEM_ARM_OF_GOLEM$75=117; moveq0,d1; jsr ReceiveMandatoryItem.|
|Failure|btst#0,d0/bne→txt2020 (hands full), txt2023, RTS;834/835 and actor unchanged.|
|Success|set835; d0.w=137; MoveEntityOutOfMap; return without local2019/2021/2022.|

The helper actually used by the pinned build is
`code/common/stats/iteminventory.asm:ReceiveMandatoryItem` at `$4F48A`,
included by `layout/sf2-07-0x044000-0x064000.asm`. The similarly named
`items/itemfunctions_s7_0.asm` is not this layout's included owner. Reuse the
accepted shared inventory contract with these exact selected operands:

1. Save d1–d5/a0; copy item word to d4 and discard-option word to d5.
   j_UpdateForce rebuilds joined, active and reserve lists from flags0..29
   and32..61. It supplies all joined members in ascending identity order to
   TARGETS_LIST, including reserves and dead members; no HP filter exists.
2. Load list-length word, subtract1, read one member byte, call
   GetItemBySlotAndHeldItemsNumber with slot0, compare held-count word d2
   against COMBATANT_ITEMSLOTS4; unsigned BCS chooses the first non-full
   member. The byte list and word count/DBF loop are distinct authorities.
3. On room, j_AddItem then dialogue substitutions/name/item, MUSIC_ITEM,
   txt213, fade/input waits and clsTxt, finally clr.w d0. This is the
   generic recipient message, not local2019. Item mutation precedes these
   services and the caller's835 write.
4. With every scanned member full, return d0=1 when d5=0. This caller's
   zero disables the discard UI/return2 branch entirely. A helper name
   containing Mandatory does not force replacement here. Other callers can
   enable discard and receive2; this callback tests bit0 (0/2 succeed), not
   equality to zero.

UpdateForce refreshes cached membership lists; it does not make the award
depend on the player's visible actor, active12 capacity, Bowie alone, Zynk
following or Gyan's scene chain. The helper scans in joined identity order,
so the prose hands-full text does not limit capacity testing to LEADER.
An empty force is not a supported zero-iteration case: after length0−1 the
body still executes before DBF, allowing65536 iterations over subsequent
bytes if no apparent room is encountered. Do not replace this with an
invented safe empty-party contract. No distinct AddItem failure result is
checked on the chosen-room path before the helper clears d0.

MoveEntityOutOfMap saves d0–d3, resolves137 via
j_GetEntityIndexForCombatant (nonally alias subtraction/mask/byte lookup),
supplies **word** X/Y=$7000 to j_SetEntityPosition, restores and returns.
SetEntityPosition writes current X/Y and XDEST/YDEST words. It neither frees
the physical entity nor clears its alias, membership, follower list or sprite
definition. This is different from deleting actor137 and from its original
63,63 tile declaration. Missing/overwritten aliases can target another slot;
there is no local existence check.

Held830 init's full order is832 picture-placement script, then834 check,
then835 check, then **clear834 only when834 held and835 clear**. It never
replays the Arm slide, grants the item or sets835. Base init is RTS. Thus:

| Supplied834 /835 on held830 reload | Flag result / freshly declared137 |
| --- | --- |
|clear /clear|Both clear; off map63,63; future zone can show it.|
|held /clear|834 cleared;835 clear; off map63,63; future zone can show it again.|
|held /held|Both retained; off map63,63; zone returns, search refuses award.|
|clear /held|Both retained; off map63,63; zone can still show it and set834, but search refuses award.|

Clear830 reload selects empty residents and RTS init, so these resets and
placements do not occur. Changing live830 back later does not recreate137.
Neither shared description function checks830: supplied832 can call the
picture from the base setup, and supplied834/clear835 with d6=0 can attempt
the award without a loaded Arm row. Successful removal then resolves whatever
alias137 currently contains, with no protection against a zero/unrelated slot.
Changing834/835 while loaded does not move it. Inventory possession is never
tested, so supplied duplicate items with clear835 do not prevent another
grant, while missing/lost Arm with held835 does not reopen the award.
No835 clear occurs locally.

Interruption matters at each boundary: scene failure before834 allows repeat
appearance; item addition followed by interrupted message/waits can leave835
clear and allow a duplicate retry;835 set before actor removal can leave a
visible but unclaimable actor. Reload plus clear835 clears834 and hides the
fresh row; a supplied clear834/held835 can expose an unclaimable Arm because
the zone does not check835. These are ordered static consequences, not
observed saves, crashes or natural progression. There is no transaction or
inventory-to-flag reconciliation in this slice.

## Complete local text and input boundaries

**Confirmed:** `data/scripting/text/gamescript.txt` keys, rather than source
comments, supply the following token sequences. N is line break; W1/W2 are
distinct dialogue waits, not choices; CLEAR is retained. No game prose is
reproduced as an extracted corpus here.

| Decimal key(s) | Complete ordered control/substitution tokens |
| --- | --- |
|2017|LEADER,N,W2|
|2018|none|
|2020;2023|Each LEADER,N,W1|
|2024|N,W1|
|2025|none|
|2026|N,N|
|2027|W1|
|2028|N,N,W1|
|2029|W1|
|2030|W2|
|2031|N,W1|
|2032|N,W2|
|2035|N,N,W2|
|2033;2034|Each N,W1|
|2036|N,N,W1|
|2037|N,NAME;26,W1|
|2038|N,W2|
|2039|W1|
|2040|N,N|
|2041|N,N,W1|
|2042|N,W1|
|4165;4166|Each N,W1|
|4167;4168;4169|Each W1|
|425;427;428|Each NAME,N,W2,CLEAR|
|213|NAME,N,ITEM|

There are28 directly selected local keys plus three investigation keys and
the success-helper key213. Local2019 (pick-up),2021 (discard question) and
2022 (discard/pick-up) exist in the text bank but are not called by the
selected handler; they are not substituted for the generic213 or evidence
of a discard path. Full helper-enabled discard dialogue214/215 is outside
this caller's d1=0 branch. Unrelated shop/church text stays with its service
owner rather than becoming a second town-local inventory.

textCursor changes only the script cursor; nextText/nextSingleText read then
advance it. Flags$0 and speaker136 retain their actual token/display
semantics. nextSingleText has its extra close/wait work; explicit hideText,
W1, W2, YesNoPrompt and callback cleanup are separate operations. The
[accepted dialogue owner](./map-event-dialogue-state.md) owns original
typewriting, RNG/input and cleanup semantics. Completed rendering/input,
sound and the literal promise of a portrait remain Unknown.

## Outgoing requests and stopping boundary

**Confirmed:** all five declarations are no-scroll mode0/padding0 and have
no local830/832/834/835/membership condition:

| Source X,Y | Map / destination / facing |
| --- | --- |
|16,8|39 MOUN_UNDERGROUND /3,21 /Right0|
|27,7|39 MOUN_UNDERGROUND /14,20 /Left2|
|0,255|74 OVERWORLD_NORTH_PARMECIA /2,25 /Down3|
|31,255|74 /5,25 /Down3|
|255,38|74 /5,25 /Down3|

255 is a wildcard coordinate in the matching consumer, not a mandatory tile.
WarpIfSetAtPoint checks terminator/match, produces type1 before payload and
facing, then WARP_SFX. The accepted request owners retain pending priority,
clearing, physical-main sampling and destination-byte replacement of the
sampled coordinate words' low bytes. Neither a row nor a flag copy proves
walkable reach or successful transfer. The underground owner covers both39
destinations and returns; no74 travel, later battle or Golem event is
investigated here.

## Actual Content, Domain, Application and Godot consumers

**Confirmed:** the retained init/script fixtures and map-event/description
owners express original routes and parsed programs. They are evidence inputs,
not an executable town package. The current
[preparer](../../src/sf2tool/remake_exploration_maps.py),
[content coordinator](../../src/sf2tool/remake_exploration_content.py) and
[OriginalPrograms](../../src/sf2tool/remake_exploration_programs.py) support
selected in-memory lowering; runtime reads prepared provenance-bound content
through [PrivateExplorationReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateExplorationReader.cs),
not disassembly/fixtures on demand. No world/content preparation or session
execution was performed here.

| Concern / actual owner | Existing representation and remaining boundary |
| --- | --- |
|Map/profile/population/layout: [ExplorationDefinition](../../remake/src/Sf2.Remake.Application/Content/Scenarios/ExplorationDefinition.cs), [Content reader](../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs), [MapEventDispatcher](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapEventDispatcher.cs)|Prepared maps/populations, view areas, block copies and events exist. Selected Map31 visual/layout/material/text/sprite bindings and natural reach are not supplied here. [MapTransfer](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs) selects at load; subsequent interaction uses loaded definition, unlike original live setup reselection.|
|Ordinary interaction: [ExplorationDispatcher](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationDispatcher.cs)|Adjacent/facing EntityRef admission exists; original48-slot/counter/physical-alias scan and live830 versus loaded population need explicit mapping. Data shapes do not establish this equivalence.|
|Flags/text/choice/movement/follow: [StoryProgram](../../remake/src/Sf2.Remake.Application/Content/Scenarios/StoryProgram.cs), [ProgramRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs), [EntityActionRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/EntityActionRunner.cs)|Typed calls/branches/flags, dialogue, choice89, actions, set-position and follow instructions exist. Source-specific actor high-byte/HP guards, aliases, original motion/waits and cached-list lifetime are not established by similar names. [MapPartyMembership](../../remake/src/Sf2.Remake.Domain/Maps/MapPartyMembership.cs) is a membership authority, not an Arm inventory transaction or Golem recruitment.|
|Search/Arm award|ExplorationEventKind has Step/Interact/Warp/SourceZone, with no original area-search/D6/priority or ReceiveMandatoryItem transaction. [Capability owner](../../remake/docs/capability-status.md) retains search/item-acquisition gaps. No automatic custom grant follows from empty pickup tables or set835.|
|Shops/church|No selected ShopMenu/ChurchMenu service instruction/UI. Exact selectors/stock and shared transaction/save rules need actual service support; dialogue cannot stand in for them.|
|Portrait and slide: [Godot presentation](../../remake/game/src/Exploration/ExplorationPresentation.cs)|Gesture/EntityEffect cues and general portrait presentation exist. Arm SLIDE_IN lowers to EntityEffect, resource slide-in, duration46 ticks. Godot requires the bound sprite and completes this cue by elapsed duration/60; original crop/DMA/VInt equivalence is unassessed. Direct native portrait52 helper is not an imported resident portrait. No selected picture/material or actual display proof.|

The direct query compiles all ten entity handlers, defaults, both init paths,
zone, both description functions/wrapper, six corpus scenes and the interior
choice target; recursively inspects their calls, labels and all selected
eas_Init/eas_InitSlow/eas_DeactivateAutoFacing/inline/walking actions. Current
conditional frontiers (zero-based **lowered** instructions, not corpus ordinals):

| Selected entry | Lowering result and conditional frontier |
| --- | --- |
|First painter Event8→cs_5D652|Event sets832 before call. cs_5D652 has11 instructions and calls interior cs_5D65E (47); on a Yes path the first native is cs_5D65E[19] setDest Bowie, followed by natives[21]/[22] Peter/B. Decline cs_5D712 (8) loops back. Typed dialogue/choice/actions before that still require resources/services.|
|Repeat cs_5D724; painter init cs_5D63C|11 and2 instructions, respectively; no native in these selected programs. This does not prove complete text or placement from an actual caller.|
|Arm appearance cs_5D644|3 instructions, typed set-position/presentation/end; no native. Zone flag write remains after scene return, contingent on actual cue/service completion.|
|Picture description→cs_5D732|5 instructions call sub_5D73E (5). The first nested native is sub_5D73E[0] movem.l; later clr.w/jsr/movem.l are also native. Checking only the outer script would hide the failure.|
|Description wrapper|ms_map31_AreaDescriptions has3 native instructions lea/nop/jmp, starting[0]. It does not import the coordinate/D6 table or DisplayAreaDescription.|
|Arm description body|With834 held/835 clear, Map31_DescFunc1's15 instructions reach native[6] jsr j_YesNoPrompt, then unlowered tst.w/bne, ReceiveMandatoryItem jsr/bit test/branch. It does not provide a working award by compiling the later flag write. Flag-return paths can avoid this frontier.|
|Girl Event4|First native[2] jsr j_YesNoPrompt after2040, then btst/bne native; native prompt is distinct from the painter's supported yesNo macro.|
|Weapon/item shop Event1/9|3 instructions each: native[0] move.b selector store then[1] jsr ShopMenu.|
|Church Event7|2 instructions: native[0] jsr ChurchMenu.|

The later success fragment lowers MoveEntityOutOfMap to hide137 with
removeAliases=false. [ExplorationState.Hide](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationState.cs)
retains aliases and writes current/destination coordinates$7000, but also sets
Visible=false and clears Actions/Follower/WaitingForSprite. Those additional
mutations are not in the original position-only helper. This typed fragment
does not establish complete original removal semantics or bypass the earlier
prompt/inventory failures.

No selected nested action stream contains native; that is syntax admission,
not proof of original movement/HP guard/wait fidelity. Native nodes map to
UnsupportedInstruction/UnsupportedEntityAction in Content; ProgramRunner
raises `program-opcode` when reached, and EntityActionRunner rejects an
unsupported action. These frontiers assume preceding caller/bindings/services
are admitted: they are not a universal earliest runtime error. Bypassing one
would not provide selected assets, D6 routing, inventory, alias semantics,
picture/shopping/church services or actual reached observations.

**Inferred:** existing typed subsets can be reused for a future bounded town
integration once the selected content and semantic gaps are resolved. No
complete town capability, end-to-end portrait, Arm custody or usable outbound
return follows from successful parsing or an in-memory query.

## Reproduction, review and remaining Unknowns

After same-process private-input setup, run the direct ignored audit with the
configured source root, then inspect the committed scope plan:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1096/review.py <configured-source-disasm-root> --committed
git diff --check origin/main HEAD
uv run sf2 verify plan --base origin/main --head HEAD
```

Review complete declarations, all52 corpus commands, seven inline streams,
native helper, keyed tokens, aliases/actual callers, widths, branch polarity
and ordered effects; compare actual current consumers and nested lowerings.
Exercise reasoning against changed/preheld830/832/834/835/89, supplied duplicate
items, missing items with835, all-full/empty/dead/reserve force, missing or
overwritten aliases, changing loaded setup, repeated/declined/interrupted
scenes, skipped service calls and reload. No parser/schema/test introduced;
ADR0004 adversarial review applies proportionally to this direct audit.

Check exact two-path ownership, navigation-only predecessor, UTF8/LF, links,
tables, private boundary and full staged diff. On clean committed HEAD record
base/head/tree, direct checks, planner and exact-head scoped CI in PR/local
handoff. Suggested normal/full suites do not override documentation scope and
are not relabeled PASS. Preserve1094 author01 encoding and author03 helper-
composition failures,1092/1088/1090 corrections and twelve old completed
comparison failures in their existing owners/receipts. New unique receipts do
not overwrite or automatically rerun them.

**Unknown:** natural entrance/C/menu Search/zone/description/warp reach;
incoming flags, HP/classes, joined/active/reserve inventory and list state,
physical aliases/population, layout/collision/view/player mode/request state;
completed loading, motion/following, portrait/text/choice/input, crop/sprite/
DMA/audio/camera and return services; interruptions, timing, visual meaning,
usable control and saved persistence. These are explicit boundaries, not a
runtime queue. Done is a clean committed/pushed frozen Draft PR, scoped results
and successful report to independent main-gate, then stopped writer/process.
Acceptance/merge stays there; no next Issue, merge, cleanup or archive here.
