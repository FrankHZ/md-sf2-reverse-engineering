# Roft town interactions, Petro scene and Zynk recruitment

Evidence date: 2026-10-20. **Confirmed (static):** Map38 has two independent
zone callbacks. The Petro scene changes presentation, not Petro's HP or
existence; its caller sets911 after return. Recruitment requires911 and
either76, or921 with joined26 clear and temp256 held. The76 branch omits
the latter guards. Join26 precedes clear76, final dialogue and transient
following; the zone clears76 again after scene return. None requires
Battle35 victory. **Stop at the Map38 exit request to76,8,48.** Natural
reach, actual carried state, completed services, usable control and saved
persistence remain **Unknown**.

Continue at [Roft to Galam, Dry Stone and Battle36 admission](./roft-to-galam-and-battle36-admission.md).

## Agreement and evidence route

Standard rigor for [Issue #1124](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1124)
under [#1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
make the town's state, interactions, recruitment and exit usable as evidence
for later content integration. Accepted base
`14c37c0549bac1c821ffbe26f91d997a2b226faf` contains the
[entry/init owner](./nazca-to-roft-and-battle35-admission.md#the-separate-roft-warp-and-fresh-population)
and [Battle35 return owner](./battle35-outcomes-and-roft-return.md).
Own this document and navigation only in the latter. No stacked dependency,
competing research writer or shared index/registry/schema/fixture/design/
engine change. Do not expand Map77, Battle36 or the later producer of921.

Original paths below are relative to read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6/disasm](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
`sf2enums.asm`, `sf2const.asm`, `sf2mapsetupmacros.asm`,
`sf2mapmacros.asm` and `sf2cutscenemacros.asm` own values and encodings.
`map/` and `entity/` abbreviate `code/common/scripting/` children;
`exploration/` means `code/gameflow/exploration/`. Local tables are under
`data/maps/entries/map38/`, scripts under its `mapsetups/`. Fnnn means
flag nnn; ordinals are zero-based and intervals half-open. USA ROM SHA-256
remains `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`;
this adds no ROM/source equality or runtime observation.

Minimum evidence is the complete selected local tables, two full scenes,
their helpers, changed-state branches and actual current consumers. Reuse
the immutable18,285,979-byte init/script pair and `local/issue1005/pair.txt`,
plus the registered canonical map export checked against
`manifests/extractions/canonical-map-import.json`. No full export is generated.
Linear selected reads/compilation, one pair/canonical load and small unique
ignored receipts have a one minute/512MiB budget per direct audit; peak memory
is unmeasured. Results stay in `local/issue1124/`. Reassess material expansion
before scaling; no acquisition, new parser/platform/fixture or tool tests.

## Population, callback selection and state lifetime

**Confirmed:** reuse the entry owner's complete15 fixed/walking records and
initializer, not a second population definition. Logical142 is the fixed
MAN194 actor at13,3/Down with eas_InitFixedSprite, used as Petro by the
selected scene. Logical137/138 are at12,3/Right and13,4/Up. Aliases32+j map
NPC128+j to physical slots after ordinary flag-followers; HP/Force membership
does not define those NPCs. There is no explicit142 interaction row: it reaches
default RTS when selected. Neither scene hides/removes142 or writes HP.

P=Peter7, F=FOLLOWER_B31, Z=Zynk26, B=Bowie0 below. F is a logical alias,
not physical raft31 or proof that its sprite is Astral. Ordinary76-following
can allocate Z without joined26. Init with921 held sets911 **first**, then
creates Z at11,8 only when joined26 and temp260 are both clear; after creation
it sets260. Creation does not join, heal, or append Z to the follower list.
It can redirect26 to a new physical slot while leaving an older follower
slot/list entry. Missing aliases can resolve to physical0; altered mappings
can select another actor. Scripts do not validate narrative identities.

The same init calls the greeting only when910 is clear; after return it sets
910 then temp258. Fresh explicit38 rebuilds actors, clears256..383 and sets80
before init. CURRENT retains actors, aliases, working layout and temporary
flags while rerunning init. Thus persistent910 suppresses greeting without
recreating258 on a fresh revisit; persistent921 can set911 without running the
Petro scene. Held260 on a retained visit can suppress repair of absent Z.
These flags are separate from35's435/535 and joined26/active58.

`s2_entityevents.asm` `$5DB76..$5DCD0` has15 explicit four-byte entries and
the default; all callback names below mean Map38_EntityEventN. UP/RIGHT/DOWN
encode **action masks1/0/3**, not approach directions. The complete routing is:

| Logical / callback | Mask | Ordered text IDs or service |
| --- | --- | --- |
|128 /0|1|2137,2138: welcome and peaceful visitors|
|129 /1|1|2139: Petro/Paseran came from the south|
|130 /2|1|2140: asks about Z's captivity|
|131 /3|1|2141: Petro's kidnapping and ship theft|
|132 /4|1|2142,2143,2144: southern tunnel and devils|
|133 /5|1|2145,2146: Red Baron warning|
|134 /6|1|2147,2148,2149: Mithril weapons|
|135 /7|1|2150,2151: Grans Mithril and blacksmith|
|136 /8|1|2152: blacksmith's exclusive craft|
|137 /9|0|910/258/911/256 branches below|
|138 /10|3|910/258/911/257 branches below|
|139 /11|1|j_ChurchMenu|
|140 /12|1|write CURRENT_SHOP_INDEX.b=13; j_ShopMenu|
|141 /13|1|write CURRENT_SHOP_INDEX.b=28; j_ShopMenu|
|26 /14|1|2153: breathless utterance|
|Default|0|RTS, including142 and unmatched selectors|

Reuse the [interaction/wrapper owner](../northern-travel-mitula/tristan-entry-interactions-and-exits.md#default-population-and-conversations):
GetActivatedEntity scans actual physical actors near the tile ahead, handles
counter/desk offsets, excludes the viewed/follower actors, then reverse lookup
selects an alias. A26 table row does not guarantee Z can be talked to while
his physical slot remains in EXPLORATION_ENTITIES. RunMapSetupEntityEvent
selects the live setup and actual sprite's portrait/speech; bit0 faces the actor
toward the player after a VInt and bit1 restores saved facing. It suspends
entity updates around the callback, closes windows and reenables them on
return. Default RTS still has wrapper work. Ordinary txt uses that inherited
speaker/context; it has no scene `$0` modifier or explicit single-text macro.

### Complete137/138 dialogue branches

Both callbacks first require910. They then choose by258, require911 in either
arm, and test their **own** first-talk flag (256 for137,257 for138). The shared
tail returns; neither writes911,258,26,76 or921.

|910|911|258|Own temp flag|137 output / mutation|138 output / mutation|
| --- | --- | --- | --- | --- | --- |
|clear|either|either|either|None|None|
|held|clear|either|either|None|None|
|held|held|held|clear|2161,2162; set256|2163,2164; set257|
|held|held|held|held|2161 only|2163 only|
|held|held|clear|clear|2165,2166; set256|2167,2168; set257|
|held|held|clear|held|2165 only|2167 only|

The258-held arm mourns Petro and the lost flight; the clear arm discusses the
crash/retaliation. Repeating137 or138 omits its second line until that actor's
temp flag clears. Changing258 while retaining256/257 switches the first line
without restoring the second. Talking to138 sets257, **not** the256 required
by recruitment's921 arm. Talking to137 sets256 only after the two text calls
return; a partial dialogue can leave it clear. Conversely externally held256
does not prove that dialogue was seen. Fresh revisit clears256/257/258 while
910/911 persist, ordinarily selecting the clear258 first-talk variants. A
CURRENT warp preserves all three unless an independently reached command
changes them. No topology or required conversational route is proved.

## Two zone callbacks and their changed-state admission

**Confirmed:** `s3_zoneevents.asm` `$5DCD0..$5DD26` has `(12,6)`→Event0,
`(12,8)`→Event1, then default RTS. Movement's admitted `$1400` candidate
posts event6 and X/Y words in the mutable request slot; standing at a tile or
supplying coordinates is not enough. ProcessMapEventType6 initializes main's
actscript, then RunMapSetupZoneEvent selects the current table. Its return
cleanup closes portrait/text, waits one VInt and main motion. Reuse the
[request consumer](../../world/map-event-request-consumption.md); no new runtime claim.

Event0: chkFlg911; held returns; clear calls cs_5DE2E, **then** set911.
It checks no910,76,921,joined26,258/260, actual Petro/party alias or HP.
F921 init can therefore suppress this scene by setting911 first. An interrupted
scene may leave presentation prefixes without911; interruption after its
return but before set911 can allow a replay. No transactional scene record.

Event1 first requires911, then gives76 precedence:

| Supplied state at Event1 | Actual branch/result |
| --- | --- |
|911 clear, all others arbitrary|Return;76 cannot bypass911.|
|911 held,76 held|Call cs_5DE76, clear76 after return, return. No921,joined26,256 or260 check.|
|911 held,76 clear,921 clear|Return.|
|911 held,76 clear,921 held,joined26 held|Return, irrespective256.|
|911 held,76 clear,921 held,joined26 clear,256 clear|Return, even if257 or260 held and Z visibly exists.|
|911 held,76 clear,921 held,joined26 clear,256 held|Call the same cs_5DE76, clear76 after return. No258,260,910,HP or alias guard.|

With76 held and26 already joined the entire ceremony can run again; capacity
does not reject it. Normally completing the scene sets26 and clears76, so a
repeat event returns through either921-clear or26-held. Reholding76 reopens
it. Clearing26 alone on a921/256-held retained visit also reopens it. On fresh
921-held/26-clear entry, init creates Z/sets260, but256 remains clear until its
own producer (normally137) returns. If910 is supplied clear, this producer is
silent even if911 held; Event1's76 arm still does not require910. With260 held
and missing Z, admission can still happen; this callback does not allocate
him. No natural route/state is inferred from these supplied alternatives.

## Complete Petro scene

**Confirmed:** `scripts.asm` cs_5DE2E occupies `$5DE2E..$5DE76`.
The18 source-operation ordinals below include cursor/end; text IDs advance
2154..2160. Every scene text has portrait modifier `$0`.

| Ordinal | Ordered command / meaning |
| --- | --- |
|0–3|textCursor2154; stopEntity142; playSound FADE_OUT; setCamDest8,0.|
|4–7|shiver137; playSound MUSIC_SAD_THEME_3; single2154/137; single2155/142.|
|8–12|shiver137; single2156/137; nextText2157/142; nextText2158/142; single2159/142.|
|13–17|shiver137; playSound FADE_OUT; single2160/137; playSound PLAY_PREVIOUS_MUSIC; csc_end.|

stopEntity is csc1C_stopEntityAnim: set the mapped actor's animation counter
to−1. Its word selector142 bypasses the ally HP skip. It is neither stop-motion,
HideEntity nor a combatant HP write. Petro remains in the fixed population
on fresh reentry; default interaction remains RTS regardless911.
csc32 sets VIEW_TARGET_ENTITY=−1, supplies camera tile destination8,0 and
waits for view scrolling. csc2A shiver saves animation counter/SPRITE_SIZE,
sets size21/counter−1, toggles FLAGS_B bit3 with update and Sleep5 on/off for
three iterations, then restores the saved counter/size. It does not move137.
These calls have no ally-HP/identity guard or completed-service guarantee.
Music requests/fades and text/input waits retain their actual service timing.
csc_end clears view scroll speed/restores interpreter state; no player-target
restoration,142 deletion, party mutation or911 write occurs in this stream.

## Complete Zynk scene, join and following

**Confirmed:** cs_5DE76 occupies `$5DE76..$5DF84`,65 source operations
including inline move/endActions rows. Text cursor2169 emits seven scene
messages, with join's446 independent of that cursor. All are single text
with portrait modifier `$0`. The full stream is:

| Source ordinals | Commands in exact order |
| --- | --- |
|0–3|textCursor2169; setActscriptWait P,eas_Init; setActscriptWait F,eas_Init; setCameraEntity Z.|
|4–8|setDest B,12,9; P,12,8; P,13,8; F,12,8; F,14,8.|
|9–13|setActscriptWait Z,eas_Init; setDest Z,11,8; setFacing B,Left; P,Left; F,Left.|
|14–22|Three separate entityActionsWait/endActions blocks: Z moveLeft2; P moveLeft2; F moveLeft1.|
|23–34|Four separate waiting blocks for Z: moveDown1; moveRight1; moveUp1; moveLeft2.|
|35–38|setActscriptWait Z,eas_Jump; setActscript Z,eas_Jump (nonwaiting); single2169/P; single2170/F.|
|39–50|Four separate waiting blocks for Z: moveUp1; moveRight1; moveDown1; moveLeft2.|
|51–57|csWait30; nod Z; setFacing Z,Right; single2171/Z;2172/Z;2173/Z;2174/Z.|
|58–60|join26; clearF76; single2175/Z.|
|61–64|followEntity P,B,2; F,P,2; Z,F,2; csc_end.|

csc29/setDest reads a **word** selector, resolves its physical alias, writes
destination/travel and signed32 velocity for each axis, and waits for actual
position=destination unless selector bit15 requests no wait. All six selected
selectors have bit15 clear. It does not perform an HP or map/entity-collision
check or first teleport to destination. Subsequent P/F destinations are
sequential waits, not alternatives. With intact distinct mappings, the last
absolute positions are B12,9; P13,8; F14,8; Z11,8. Completed inline movement
then gives P11,8 and F13,8; Z moves11,8→9,8→9,9→10,9→10,8→8,8,
then8,7→9,7→9,8→7,8. Jumps are relative excursions, not an absolute repair.
These are source arithmetic under motion completion, not observed routes.
Duplicate aliases can invalidate this independent-actor arithmetic.

csc15 installs Init/Jump with a physical-slot action-read timer; waiting
forms poll **eas_Idle's address**. No HP guard. eas_Init lowers to14 operations,
sets speed32/32, clears acceleration/deceleration and FLAGS_A collision-check
bits5/6, enables obstruction bit7/autofacing, resets orientation/immersion,
requests size24/sprite update then idles. csc2D inline actions also AND$9F:
bits5/6 clear **disable** entity/map collision checks. Reuse the entry owner's
[corrected actual collision consumers](./nazca-to-roft-and-battle35-admission.md#independent-zynk-creation-and-greeting-branches):
esc04 relative moves build movement and esc01 waits for destination;
UpdateEntityData has no new collision gate. Map obstacles alone do not prove
these sequences stall while those bits remain clear. Live aliases/actions,
entity-update scheduling and progress still matter. eas_Jump's9 operations
disable autofacing, move0,−1/wait5/0,1/wait5/0,0, reenable autofacing and idle.
The second jump can overlap text and be replaced by the next inline block.

csc23 facing checks the selected **byte** ally HP: dead B/P/Z skip their
facing writes, F31 bypasses it. setCameraEntityZ instead resolves the alias
without HP testing. nod/csc26 sets animation counter−1, Sleep10, one
load/lower/DMA/Sleep20/restore/Sleep10 pass, then counter0; no HP test.
csWait30 is a Sleep request, not proof of wall time. Camera target stays Z
in the scene; following does not explicitly switch it back to B.

### Actual join and field-follow effects

Source ordinal58 selects csc08_joinForce word001A: no sad bit15, no special
128/Sarah+Chester case. It clears speech, waits for view, requests MUSIC_JOIN,
calls j_JoinForce26, reads **current** class/name for446, performs music/input
return, closes text and sleeps10. Only then can clearF76 and2175 execute.
`code/common/stats/battleparty.asm:JoinForce` sets joined flag26, rebuilds
all30 joined/active/reserve prefixes in ID order, and sets active32+26=58
only if the rebuilt active count is below FORCE_MAX_SIZE12. At capacity it
does not clear preheld58 or reject joining. Lists are rebuilt **before** a
new58 write, so that counted snapshot can still omit the newly active Z.
Preheld58 without26 becomes counted when26 is set. Already-joined26 can
receive the same service again; no capacity or already-joined rejection.

JoinForce does not initialize HP/MP/class/status/items, heal, allocate an
alias, remove a field follower, post a map request or save. Dead Z can join
and stay dead. Clearing76 is separately reached after the join service;
the zone's second clear is reached only after2175, all three follows and
csc_end return. Interruptions can retain joined26 with76 held, or76 clear
without final dialogue/follows; no rollback is supplied.

`sf2cutscenemacros.asm:followEntity` emits follower/leader/offset as **words**.
csc2C's HP precheck reads the first **byte** of the follower word: each selected
0007/001F/001A reads0, so it tests **Bowie's HP**, not P/F/Z's. With Bowie HP0
it skips each six-byte operation. With Bowie alive, dead P/Z are not separately
excluded; neither leader HP nor narrative identity is tested. It resolves both
aliases, doubles offset2 and reads bytes4/5 of table_FollowerPositions:
**(−24,0)** signed pixel offsets, not(0,24) or tile coordinates.
AddFollower copies42 bytes of eas_Follower1 into walking scratch, patches
physical leader/offsets and appends the physical follower to
EXPLORATION_ENTITIES only if absent. An already-listed slot still receives a
new script copy; no allocation-capacity or identity repair is provided here.
This transient P→B, F→P, Z→F chain does not re-set76. Fresh reentry with26
held/76 clear does not create Z through either ordinary76-following or the
921/26-clear dynamic branch; other flags/mappings remain independently live.
Callback return routes through ordinary exploration cleanup and the next
WaitForEvent; a pending request takes priority over control-script/view-target
installation. No natural follow completion or restored usable control is claimed.

## Text IDs, speakers, modes and complete ordered tokens

**Confirmed:** `data/scripting/text/gamescript.txt` owns each hexadecimal key.
Tokens below spell their literal braces: N=`{N}`, W1=`{W1}`, W2=`{W2}`,
CLEAR=`{CLEAR}`, NAME26=`{NAME;26}`, LEADER=`{LEADER}`, NAME=`{NAME}`,
CLASS=`{CLASS}`. Repeated N entries are distinct and ordered. Public meanings
are short authored paraphrases; original prose remains private.

| ID | Speaker / mode | Meaning | Ordered tokens |
| --- | --- | --- | --- |
|2137|128/txt|Town welcome|W2|
|2138|128/txt|Peaceful visitors welcome|N,W1|
|2139|129/txt|Two southern arrivals|N,W1|
|2140|130/txt|Concern for captive Z|NAME26,N,N,W1|
|2141|131/txt|Kidnapping and theft|N,N,W1|
|2142|132/txt|Southern tunnel arrival|N,N,W2|
|2143|132/txt|Devils in tunnel|N,W2|
|2144|132/txt|More devils in town|N,N,W1|
|2145|133/txt|Red Baron danger|W2|
|2146|133/txt|Bloodied armor|N,W1|
|2147|134/txt|Mithril question|W2|
|2148|134/txt|Weapon strength|N,W2|
|2149|134/txt|Combat help|N,W1|
|2150|135/txt|Former Grans supply|N,W2|
|2151|135/txt|Blacksmith location|N,W1|
|2152|136/txt|Exclusive craftsman|N,N,W1|
|2153|Z/txt|Breathing|W1|
|2154|137/single|Asks Petro to stay|N,W1|
|2155|142/single|Apology to Paseran|W1|
|2156|137/single|Flight promise|N,W1|
|2157|142/nextText|Remembers ship promise|N,W2|
|2158|142/nextText|Home and family|N,W2|
|2159|142/single|Ancient wonders|N,W1|
|2160|137/single|Calls to Petro|W1|
|2161|137/txt|Grieving|N,W2|
|2162|137/txt|Lost flight opportunity|N,W1|
|2163|138/txt|Deception and killing|N,W2|
|2164|138/txt|Cruelty|W1|
|2165|137/txt|Crash question|W1|
|2166|137/txt|Wish for Petro beyond death|N,N,W1|
|2167|138/txt|Deception and Petro|N,W2|
|2168|138/txt|Request for vengeance|N,W1|
|2169|P/single|Concern for Z|NAME26,W1|
|2170|F/single|Disturbed behavior|N,W1|
|2171|Z/single|Addresses party, anger|LEADER,N,W2|
|2172|Z/single|Cannot forgive devils|N,W2|
|2173|Z/single|Fighting restriction overridden|N,W2|
|2174|Z/single|Offers soldier service|N,W1|
|446|join service/txt|Current26 name/class announcement|NAME,CLASS,N|
|2175|Z/single|Urges attack|N,W1|

The service announcement has no scene speaker byte and does not advance2169's
cursor. NAME/CLASS use current26 state; LEADER/name expansion and portraits
depend on actual caller/member/alias/sprite state. Text content does not prove
a story flag, HP death or successful recruitment.

## Searches, shops, church and local layout tables

**Confirmed:** `s4_descriptions.asm` `$5DD26..$5DD50` supplies base
`$104B=4171`, four six-byte msDesc rows at$5DD36 and FD00 termination.
All condition/kind bytes are0/0, no function or d6 gate:

| Target X,Y | Investigation offset →ID | Description offset →ID / meaning |
| --- | --- | --- |
|3,14|4→427|0→4171 /man-eating plants book|
|11,2|4→427|1→4172 /ancient travels book|
|19,26|2→425|2→4173 /weapon-shop sign|
|19,5|2→425|3→4174 /item-shop sign|

DisplayAreaDescription in `map/mapsetupsfunctions_1.asm` matches packed
candidate coordinates first, emits423+investigation then base+offset,
closes windows and returns d7=−1; miss returns0. Each425/427 has ordered
NAME,N,W2,CLEAR; each4171..4174 has N,W1. CheckArea's layout-kind search
branches precede this callback, so a matching coordinate does not guarantee
the description wins. NAME uses the search caller's member0 name.
`s5_itemevents.asm` `$5DD50..$5DD58` contains only the default item callback
to RTS. `8-other-items.asm` and `3-flag-events.asm` each contain only endWord.
No local item-use effect, non-chest reward or flag-block row is declared.

`7-chest-items.asm` `$AFD20..$AFD2A` has exactly two four-byte records:
2,29 /F190 /BLIZZARD17;43,6 /F191 /HEALING_DROP2; then endWord.
`exploration/exploration.asm:OpenChest` matches the actual candidate and
tests its flag; already-held returns NOTHING. It **sets the flag and opens
the working block before** CheckArea's receipt handling. Selected non-gold
flow uses403(opened),413(found), then415(received) for Bowie0 if his four
slots have room. Otherwise UpdateForce supplies the ordered joined-member
list, and OTHER_FORCE_MEMBERS_LIST is its second byte: it tries the remaining
count−1 members, not just the active12, and emits416 on first successful
pass. This assumes0 heads the joined prefix; supplied joined0-clear state
can make the suffix skip a different first member. No HP/class gate.
If all attempted inventories are full,417 then CloseChest clears the actual
flag and restores the closed block; RefillNonChestItem separately finds no
declared local non-chest row. No forced discard, Caravan or Deals fallback.
Held190/191 gives403 then408(empty), no new award. Interruption after OpenChest
can leave a flag set before any inventory receives an item; full rejection's
clear is later, not atomic. Fresh item processing uses retained flags; save/load
or actual inventory delivery has not been observed.

The selected search messages have complete tokens:403 NAME,W2,CLEAR;
408 W1;413 NAME,N,ITEM,W2,CLEAR;415 NAME,N,ITEM;416 NAME,N,ITEM,NAME;
417 N,W1. ITEM spells`{ITEM}`. IDs/ordering describe this service seam, not
a guarantee of natural target selection. Reuse the
[search/description owner](../northern-travel-mitula/tristan-entry-interactions-and-exits.md#shops-church-searches-and-map-callbacks)
and [service contract](../../../design/contracts/party/service-interactions.md) for shared
mechanics; the local selected stock/flags above remain independently checked.

Shop selectors13/28 are zero-based across `list_ShopInventories` in
`data/stats/items/shopinventories.asm`, consumed by
`code/common/menus/shop/shopactions.asm:GetShopInventoryAddress`'s skip of
length-prefixed rows. Comment labels are one-based within each category:

| Selector / source comment | Complete ordered ITEM_ stock |
| --- | --- |
|13 /Weapon shop14|BUSTER_SWORD, GREAT_SWORD, JAVELIN, CHROME_LANCE, LARGE_AXE, GREAT_AXE, ASSAULT_SHELL, GREAT_SHOT, GUARDIAN_STAFF, INDRA_STAFF, BRASS_KNUCKLES, IRON_KNUCKLES|
|28 /Item shop14|MEDICAL_HERB, HEALING_SEED, HEALING_DROP, ANTIDOTE, FAIRY_POWDER, ANGEL_WING|

No local quest/535/911 gate or grant surrounds either shop or church call.
ChurchMenu's common raise/cure/promote/save and cancellation algorithms remain
with the [shared service owner](../northern-travel-mitula/ketto-interactions-and-merchant-relocation.md#shops-priest-and-reload-lifetime).
At admitted save, `church/churchactions_1.asm:@DoSaveGame` writes live
CURRENT_MAP.b→EGRESS_MAP, reads the save slot, sets399 **before SaveGame**.
The separate `data/maps/global/savepointmapcoords.asm` row is38,21,27,Right0,
not town entry14,30. GetSavepointForMap with399 clear instead returns initial
3,56,3,Down; held399 selects the row and optional64 raft reset. Entry, opening
church, requesting save, completed save and a later egress destination are
different facts. No completed transaction, healing or persistence is claimed.

All five `4-step-events.asm` rows (`$AFC92..$AFCBC`) copy1×1:

| Trigger | Source | Destination | Matching roof row |
| --- | --- | --- | --- |
|18,5|26,1|18,5|None|
|20,26|26,1|20,26|None|
|4,18|26,0|4,18|255,255 /5×5 /1,45|
|12,7|26,0|12,7|255,255 /5×6 /10,33|
|21,13|26,0|21,13|255,255 /5×5 /20,40|

The three roof rows occupy `$AFCBC..$AFCD6`. OpenDoor requires no active
battle, copies the matched working layout block and requests door SFX/plane
updates. PerformMapBlockCopyScript/ToggleRoofOnMapLoad treat signed negative
source Y as clearing, not a read at255,255; roof save/restore and load tests use
live roof state and secondary-layer-adjusted rectangles. These are layout
operations, not zone scripts, warps, new story flags or proof of passability.

## Nine warps and CURRENT versus fresh departure

**Confirmed:** `6-warp-events.asm` `$AFCD6..$AFD20` has nine eight-byte rows
then endWord. All use no-scroll0 and have no party/911/535 prerequisite.

| Ordinal / source X,Y | Destination / X,Y / facing |
| --- | --- |
|0 /31,7|CURRENT255 /18,6 /Down3|
|1 /18,5|CURRENT /31,6 /Up1|
|2 /31,19|CURRENT /20,27 /Down3|
|3 /20,26|CURRENT /31,18 /Up1|
|4 /33,2|CURRENT /19,1 /Right0|
|5 /19,1|CURRENT /33,2 /Left2|
|6 /35,14|CURRENT /45,2 /Right0|
|7 /45,2|CURRENT /35,14 /Left2|
|8 /255,31|76 /8,48 /Down3|

Source255 is a signed X wildcard; the admitted movement branch still needs
layout marker$1000. WarpIfSetAtPoint tests terminator/first matching row and
posts the mutable event1/parameters/facing/SFX. ProcessMapEvent clears type,
the no-scroll handler idles main, handles SFX and unwinds to MainLoop.
It samples current signed main coordinates/384; nonnegative destination
bytes replace only their low bytes. Nominal ordinary coordinates above do
not erase supplied high bytes or prevent intervening request changes.
SwitchMap precedes CheckBattle; neither38 nor76 is a switched-map source.
There is no38 battle-coordinate row. This audit stops at explicit76,8,48
handoff, before expanding any destination encounter or later story producer;
the already accepted regional/battle admission remains in the entry owner.

| Lifetime | Eight intact CURRENT38 paths | Fresh explicit76 exit / later fresh38 entry |
| --- | --- | --- |
|Population/temp|Retain actor/alias/follower/scratch and256..383; no new80 write.|Rebuild from destination and supplied follower flags; clear temps, set80 before init.|
|Resources/layout|Negative LoadMap skips fresh palette/tileset/block-layout loading and retains working door/chest copies; area selection, display/plane updates and roof checks still run.|Load destination resources/base working layout and process destination flags/items; fresh population/mapsprites.|
|Init/story|Rerun live38 init:921 can set911; retained260/26 govern creation;910 still gates greeting/258.|Destination76 init is RTS; later fresh38 clears260/258 before its independent checks. Persistent26/76/910/911/921 retain their own meanings.|
|Common exploration|Heal living/immortal allies, fade and STEP_COUNTER.w subtraction still occur; not a stat/class reset or a new scene transaction.|Same common prelude, plus fresh path work; no guaranteed saved/actual state.|

Healing revives Peter7/Lemon28, retains other dead allies and retained status
rules; it does not join Z. Roof selection/working blocks, area/camera services,
init and usable player control are distinct. Declared doors/warp pairs do not
prove geometry, natural reach, shortest paths or a mandatory scene sequence.

## Actual lowering and current consumption boundary

**Confirmed (direct offline execution):** fresh OriginalPrograms instances
register all seven Map38 defining files, including local continuations,
before each selected compilation. Real compiler and event-enabled compile_maps
consume the registered source/canonical unchanged. No synthetic success,
failed cache, SDK, engine or Godot execution is used.

| Actual output | Result and practical limit |
| --- | --- |
|cs_5DE2E|48 compiler instructions, no native emission. Animation142, camera, gestures, music/text and end are typed; source18 operations are not48 frames.|
|cs_5DE76|86 instructions, **six native setDest** emissions at source4,5,6,7,8,10; the first is output4. Init/Jump actions lower to14/9 operations. Typed later join26, clear76, text and three follows exist but are not reached through a successful native move.|
|Zone0 /Zone1|Root4 /5 instructions, with return/cs and byte_5DD08 continuation6; flag polarity and scene-call/clear ordering represented.|
|137 /138 callbacks|Root10 each; all five continuations resolve, preserving selected258/911/256 or257 text branches.|
|Church139|2 instructions, first native jsr.|
|Shops140/141|3 each, native move.b then native jsr; neither selector store nor service is implemented by those emissions.|
|Description root|3 native instructions lea,nop,jmp; typed address move is not a working search service.|
|Default item callback|One end; no local item effect.|
|compile_maps38|One map,15 fixed entities,43 programs, init binding,16 interactions including default,3 source-zones including default; eight Preserve38 and one Rebuild76 warps;5 doors/3 clear roofs/empty flags; entry temp clears+80. No battle route supplied. No chest/search/shop/church service content is added to the map by this conversion.|

The unchanged [compiler](../../../../src/sf2tool/remake_exploration_programs.py)
and [map conversion](../../../../src/sf2tool/remake_exploration_maps.py) own those
outputs. [ExplorationContentReader](../../../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs)
reads native-call as UnsupportedInstruction; [ProgramRunner](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs)
stops Unsupported at program-opcode. Thus recruitment stops at the first
native destination before its later join/follows; church/shop stop at their
first native. Successful compilation is not complete scene support. Full38
conversion also retains init's native ally newEntity; the accepted entry
owner's creation/resource/HP/alias limits remain separate from these scenes.

Typed animation/facing/motion/follow operations do not encode native HP
prechecks, including the high-byte/Bowie follow gate.
[SceneEntities.Build](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/SceneEntities.cs)
initializes all64 normal aliases (0..31 and128..159) to physical0 before
overwriting allocated mappings. The [ExplorationState constructor and resolver](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationState.cs)
project those aliases through physical slots. Thus unallocated26 with no
follower or dynamic creation can resolve to player0, including for this
scene's early camera target; absence of a Z actor does not guarantee rejection.
TryResolveEntity separately rejects malformed references, selectors outside
the64-entry contract, absent alias mappings (including Hide's removed aliases)
and mapped slots>=49. A genuinely unresolved reference, or absent exploration
state, reaches ProgramRunner's program-entity failure. Fresh zero mappings
and removed mappings are different states; without a source population the
resolver uses its existing entity dictionary directly. Exact duplicate and
unchecked out-of-contract native alias behavior is not guaranteed. The selected Petro
stream has no native, but needs live142/137, correct text/portrait/materials,
camera, shiver and audio acknowledgements. Its PLAY_PREVIOUS_MUSIC is emitted
as generic Sound, while join lowering has explicit PreviousMusic; matching
native command/history consumption must not be inferred from that label.
ExplorationPresentation sends generic Sound to SessionAudio.Play(resource),
whereas PreviousMusic calls PlayPrevious; absent audio content raises
audio-content-unavailable. ExplorationMusicRunner.Request preserves current music when no admitted
audio binding exists or its command>=65. No host completion was observed.

[MapPartyMembership.Join](../../../../remake/src/Sf2.Remake.Domain/Maps/MapPartyMembership.cs)
expresses the30-member/12-capacity flag/list order when its layout is bound.
Application JoinResources additionally requires a unique actor/deployment/start
binding when encounter starts exist and can introduce admitted start resources
once. Native JoinForce preserves existing resources; current Battle01–03
content does not establish member26 bindings. ProgramRunner's follow and
[FollowerMotion](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/FollowerMotion.cs)
consume physical leaders/pixel offsets without that source Bowie-HP precheck.
Their existence does not repair native setDest or guarantee natural following.

[MapTransfer](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs)
supports same-map Preserve and Rebuild, with destination/setup/blocked-entry
admission and roof handling. Preserve retains working layout/entities/flags;
ReloadScene separately replaces working layout, unlike native negative LoadMap.
Shared map/traversal/copy rules and [MapEventDispatcher](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapEventDispatcher.cs)
express doors/roofs and source-zone admission. They do not supply these town
search inventories, shop/church algorithms, native mutable packet/high-byte
semantics or every common exploration heal/step/resource effect. Geometry and
actual scene/service/material binding need separate support.

Godot's [SessionAudio](../../../../remake/game/src/Audio/SessionAudio.cs) and
[ExplorationPresentation](../../../../remake/game/src/Exploration/ExplorationPresentation.cs)
project admitted Application state and acknowledge
selected resources/waits. They do not provide missing native scripts, recruit26
content, chest/shop/church services or original VInt/DMA timing. No running
instance was accessed. This is research complete at its bounded source/consumer
boundary, not a playable Roft or engine-ready town claim. Minimum future gaps
are native destination motion, state/alias/HP parity, member/resource bindings,
local search/services and actual presentation/material integration; none is
authorized for implementation by this slice.

## Reproduction, preservation and Unknowns

Load the current ignored selections in the launching process; source root ends
in disasm and canonical is the configured existing registered export. Use an
unused receipt filename:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1124/observe.py <configured-source-disasm-root> --canonical <registered-canonical-export> --receipt local/issue1124/observe-reproduction-01.json
uv run --locked python -X utf8 local/issue1124/review.py <configured-source-disasm-root> --receipt local/issue1124/review-reproduction-01.json --committed
git diff --check
uv run sf2 verify plan --base origin/main --head HEAD
```

The ignored recipes directly check selected source/consumer/token/state facts,
pinned clean upstream/unchanged pair/canonical, complete scene order and branch
polarity, exact two-path/navigation-only diff, links, UTF-8/LF and authored-text/
private boundary. Source and this prose own durable evidence; local recipes
and receipts are coordination state. Apply ADR0004's relevant selector/order/
width/polarity/provenance checklist; new parser/schema/fixture/H3 obligations
are N/A. Clean committed planner and actual exact-head scoped CI are separate
receipts; suggested gates are not executed passes. Normal/full/old comparisons,
SDK/.NET/Godot/emulator/H3/H4/screenshots and full exports remain excluded.

Preserve #1116's original collision-polarity missed check and accepted focused
correction; [Dojo](./dojo-exploration-and-sheela-recruitment.md) retains Lemon28,
offset2=(−24,0), missed checks and eas_5FFC4/5FFC8 /compile_maps45 refusals.
[Post-crash](./post-crash-ship-and-nazca-overworld-return.md) retains complete-import
@Return partial, crew/RNG/mapLoad/ReloadScene limits. The Battle35 return owner
retains #1122 diagnostics and #1104's listed-dead-ally defeats versus enemy
FIRST_ALLY kill-accounting correction. Twelve completed old comparison failures
remain discovery evidence, not interrupted work or a rerun queue. No earlier
whole audit or runtime gate is repeated here.

**Unknown:** natural movement/interaction/zone/chest/door/exit reach and scene
ordering; actual flags/party lists/HP/MP/status/class/items/gold/aliases/follower/
raft/player/step/EGRESS_MAP state; decoded geometry/collision/material bindings;
interruption/interleaving and completed camera/movement/animation/text/portrait/
music/gesture/load/roof/input services; VInt/DMA/render timing, usable control,
successful transactions and save/load persistence. These are neither passed
gates nor an automatic acquisition queue. Later921 production, Map77 and
Battle36 are outside the stopping boundary.

Freeze a clean committed/pushed Draft PR with exact base/head/tree, direct
checks/planner/actual CI, preserved failures/Unknowns and owned process state;
Project Review / Verify and one successful main-gate notification precede stop.
Independent acceptance/merge stays with main-gate; no next Issue, merge,
cleanup or archive.
