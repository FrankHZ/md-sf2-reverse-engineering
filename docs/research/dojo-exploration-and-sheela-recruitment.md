# Dojo exploration, Sheela recruitment and overworld return

Evidence date: 2026-10-20. **Confirmed (static):** Map75 declares an
optional no-scroll entrance to Map45/Dojo. Map45 supplies two areas,
nine fixed-table records including one walker, four explicit interaction
callbacks, church access, three searchable items and five warps. Its
wildcard-X zone at Y17 calls cs_600CE only while joined flag25 is clear.
The scene eventually calls join25 and installs temporary following; its
dialogue does not itself establish membership or a required main route.
**Stop at Map75 return/reentry context.** Natural access, completed services,
usable control, rendering and saved persistence remain **Unknown**.

## Agreement and evidence route

Standard rigor for [Issue #1114](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1114)
under [#1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
provide the complete bounded Dojo branch for content/engine decisions.
Accepted base: `1e43b0057f9c1d53394fc611027ac0ef48b80628`.
Own this document and navigation only in the
[post-crash return owner](./post-crash-ship-and-nazca-overworld-return.md).
No index, registry, schema, fixture, tool or engine change; no stacked
dependency. Roft, Battle35, other towns/recruitments and later story are excluded.

All original paths below are relative to `disasm/` in the clean read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The baseline USA ROM identity remains
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`;
no new ROM/runtime equality claim is made. `sf2enums.asm`/`sf2const.asm`
own constants and RAM widths. `map/` and `entity/` abbreviate
`code/common/scripting/`; `exploration/` abbreviates `code/gameflow/exploration/`.
Fnnn means flag nnn. Ordinals are zero-based; address intervals are half-open.

Selected originals: all Map45 `mapsetups/` files (`$5FF62..$603AE`),
its area/event/item/warp/resource declarations (`$B2BFE..$B2CE0`),
Map75's selected Dojo entrance, and the named consumers below.
Reuse the predecessor's complete Map75 tables and
[route/request owner](./moun-to-nazca-and-battle34-admission.md),
[follower allocation owner](./nazca-voyage-and-map59-arrival.md),
[church/service owner](./tristan-entry-interactions-and-exits.md#shops-church-searches-and-map-callbacks)
and [search/join owner](./mitula-shrine-exploration-and-taya-recruitment.md).
Selected claims are checked at their actual source consumers; these owners
are not instructions to repeat their complete audits.

Reuse the immutable18,285,979-byte init/script pair and provenance
`local/issue1005/pair.txt`. The existing canonical export is read-only and
checked against `manifests/extractions/canonical-map-import.json`'s registered
identity before selected `compile_maps`; no export is regenerated/copied.
Selected linear reads, in-memory compilation and unique ignored receipts
are budgeted at one minute/512MiB per audit. Peak memory is unmeasured.
No acquisition, emulator, SDK, Godot, screenshot or comparison-platform work;
reassess material growth before expanding scope/resources.

## Entrance, live setup and the two areas

**Confirmed:** `map75/6-warp-events.asm` row4 is source2,27,
no-scroll0, destination45,13,43,Up1. Controlled movement's esc02 evaluates
candidate motion/collision and the current working-layout kind. Only its
warp-marker `$1000` branch invokes `WarpIfSetAtPoint`; that consumer divides
pixel coordinates by384, matches CURRENT_MAP's ordered table, publishes
MAP_EVENT_TYPE.w=1 and payload0,45,13,43,Up/padding, and requests WARP_SFX.
Standing/spawning at2,27 alone is insufficient. No F25/F999/joined/HP
prerequisite appears in this row or warp consumer.

WaitForEvent observes the mutable request; ProcessMapEvent clears the type
word. `ProcessMapEventType1_Warp` with parameter1=0 requests/clears the SFX,
idles physical0, unwinds to MainLoop and returns45/13/43/Up/d4=0.
MainLoop @Start calls SwitchMap then CheckBattle. The complete
`data/maps/global/flagswitchedmaps.asm` has no45/75 replacement; the complete
`data/battles/global/battlemapcoords.asm` has no45/75 battle row. Neither
entrance nor return therefore admits a battle at this seam, regardless of
unrelated battle unlocks. No later route is inferred.

Fresh explicit45 in ExplorationLoop clears the event type, subtracts20000
from STEP_COUNTER (negative clamps to0), heals living/immortal allies,
requests fade, sets CURRENT_MAP45/CURRENT_BATTLE255, loads tilesets,
selects/builds entities, loads mapsprites, clears temp256..383 and sets80.
Then it resets exploration sprite counters, calls LoadMap, installs base
VInts and runs the live setup initializer. Healing scans all30 ally records,
not only joined/active: living HP/MP become maxima, Peter7/Lemon29 revive,
other dead allies stay dead; status retains stun/poison/curse and updates
stats. It does not initialize Sheela's class/items or set25/57.

`data/maps/mapsetups.asm:msMap45` has only ms_map45 and no flag variants.
`GetCurrentMapSetup` reads live CURRENT_MAP; `pointertable.asm` supplies,
in order, Entities, EntityEvents, ZoneEvents, AreaDescriptions, Section5,
InitFunction. F25 changes init/zone behavior, not table selection.

`2-areas.asm` has exactly two30-byte records plus terminator:

| Area | Main bounds | Secondary foreground / background | Type / music |
| --- | --- | --- | --- |
|0|0,0 through31,48|32,0 /0,0|0 /MUSIC_BATTLE_THEME_3|
|1|0,49 through17,63|0,0 /0,0|255 /MUSIC_BATTLE_THEME_3|

Both layers in both areas have parallax256,256 and autoscroll0,0.
LoadMap's area-selection loop tests the view-target pixel position (ordinary
physical0 here) against ordered inclusive
area bounds; an unmatched coordinate falls back to the first record.
Fresh13,43 and internal22,23 select0; internal15,52 selects1 when those
positions reach the consumer. This is declaration/selection evidence,
not decoded reachability or a claim that a reference endpoint defines legality.

Palette0/tilesets4,88,43,38,86 are declared by `00-tilesets.asm`.
Flag-block and step tables are empty terminators. Roof records, in order,
trigger11,33;12,33;13,33;19,30, each source255,255, size16×13,
destination40,20. Signed negative source Y selects clear/save/restore roof
work, not an actual read of255,255. Map-load roof selection accounts for
secondary-layer origins. Animation87/base64 has entries0/32/$300/20 then
32/32/$300/20 and terminator. Layout/collision/marker words and actual
palette/roof/animation rendering remain unassessed here.

## Population, aliases and the local training loop

**Confirmed:** `mapfunctions.asm:InitializeMapEntities` clears entities
and alias bytes, builds flag-selected followers first, then fixed records,
then player0 and the raft/player-type helper. Map45 is not an overworld
or either follower-excluded map37/46: it starts at table_Followers, excluding
the preceding overworld Caravan65 row. It still walks the entire ordinary
table to its real terminator. Ordinary67/70/76 yields Peter7 physical1,
Astral/FOLLOWER_B31 physical2 and Zynk26 physical3. F65 alone adds no
Caravan here. Other supplied follower flags can add duplicates/replace
aliases; later mappings win. No ordinary row declares Sheela25.

The complete `s1_entities.asm` file spans `$5FF7A..$60024`.
Its table `$5FF7A..$5FFC4` is nine eight-byte records plus a two-byte
terminator; local actions occupy `$5FFC4..$60024`. With n actual followers,
Sheela ordinarily allocates n+1; the eight nonallies allocate n+2+j,
logical128+j/lookup32+j for j=0..7. Ally allocation uses its existing
nonzero alias if present, consuming the action longword but skipping a
duplicate allocation. The ally row does not consume the NPC alias cursor.

| Row / logical / ordinary physical | X,Y / facing | Sprite / action |
| --- | --- | --- |
|0 /25 /4|6,6 /Down3|ally Sheela, eas_Init|
|1 /128 /5|12,30 /Down3|MONK, walking origin12,30/radius1|
|2 /129 /6|12,27 /Right0|MONK, eas_Init|
|3 /130 /7|13,27 /Left2|MONK, eas_Init|
|4 /131 /8|12,24 /Down3|MASTER_MONK, eas_Init|
|5 /132 /9|15,28 /Up1|POSE5, eas_5FFC4|
|6 /133 /10|9,26 /Up1|POSE5, eas_5FFC8|
|7 /134 /11|63,63 /Up1|OBJECT4, eas_Init|
|8 /135 /12|63,63 /Up1|OBJECT3, eas_Init|

These are logical aliases and ordinary examples, not invariant physical
IDs. Nonally134 is OBJECT4, not Sheela or an enemy combatant134.
ALLY_SHEELA25 resolves through ally alias25. F25 is not checked during
fixed allocation. `GetAllyMapsprite` selects Blue Flame at HP0 in exploration,
otherwise actual class sprite; being unjoined does not suppress25.
Declared POSE/OBJECT sprites are not those ally HP/class substitutions.
The walking macro copies the existing eas_Walking template into local
scratch, sets its origin/range and repeats random-walk/destination waiting;
this is neither a prescribed path nor a runtime RNG observation.

The complete training action chain must cross labels:

| Entry / order | Source operations and consumer meaning |
| --- | --- |
|eas_5FFC4|wait16, then **fall through** eas_5FFC8; no end/jump at the label boundary.|
|eas_5FFC8 prefix|acceleration factors0,0; speed20,20; acceleration OFF/OFF; deceleration OFF/OFF; entityObstructable ON; motion OFF; facingDown; sprite update.|
|First half|relative0,−1; wait8; relative0,0; facingRight; sprite update; wait8.|
|byte_60000 half|facingUp; sprite update; relative0,+1; wait8; relative0,0; facingRight; sprite update; wait8.|
|word_60020|relative ac_branch displacement back to eas_5FFC8; prefix and both halves repeat, initial wait16 does not.|

`entityscriptengine_2.asm` esc10 writes the packed X/Y speed word;
esc11 writes acceleration factors, esc12/13 clear the acceleration/deceleration
bits, esc18 sets flagsA bit7, and esc14 motion OFF writes animation counterFF.
This stops frame animation, not action parsing/movement. No local operation
clears map/entity collision flags or sets auto-facing; those fields retain
their initialization/live state. esc04 adds signed offsets×384 to **current**
position and checks HasSameDestinationAsOtherEntity before advancing.
Relative0,0 requests the then-current position; it is not a destination wait.
esc00 uses a service-counted byte wait timer; esc30 adds the signed relative
branch displacement. There is no eas_Idle termination in this loop. The two
pose actors' phase, exact displacement and collision outcomes depend on
VInt scheduling and live state; source waits are not measured elapsed time.

`ms_map45_InitFunction` `$60098..$600CE` reads25. Held→
MoveEntityOutOfMap25 then return_600B0; clear→byte_600AA invokes cs_600B2
then the same return. MoveEntityOutOfMap resolves combatant25, calls
SetEntityPosition with `$7000,$7000` and retains alias/follower/script data.
It is distinct from csc2E HideEntity, which removes aliases/follower entry.
No HP or active57 guard is added by this initializer.

cs_600B2's complete five commands are: setActscriptWait25,eas_Immersed;
setSprite25,POSE7; setActscriptWait25,eas_InitFixedSprite;
setFacing25,Up; csc_end. Immersed sets flagsB immersed bit then sprite
update/idle. InitFixedSprite subsequently sets accel0/speed32, clears
acceleration/deceleration and collision-disable bits, enables obstruction,
sets auto-facing then orientationUp, **clears immersed**, disables
auto-facing and animation, sets size24, updates sprite and enters idle.
Thus immersed is an intermediate request, not a retained final-state claim.
setSprite uses explicit POSE7; final setFacing alone has the ally HP guard.
Action installation/waits and sprite change do not test joined25/HP25.
Successful return still depends on action and sprite services.

## Interaction, church and searches

**Confirmed:** complete `s2_entityevents.asm` table `$60024..$60060`
has four explicit rows plus default. Direction values are handler masks,
not a required player-facing direction. The real C-button producer, entity
reach, reverse alias lookup and live table selection precede these callbacks.

| Logical | Mask / callback | Ordered local work / text paraphrase |
| --- | --- | --- |
|128|Up1 /Event0|2077 monk study;2078 training; rts.|
|129|Down3 /Event1|2079 warning about approaching from behind; rts.|
|130|Down3 /Event2|2080 mental control; rts.|
|131|Up1 /Event3|2081 priest background;2082 offer; j_ChurchMenu; rts.|
|Default|0 /Map45_DefaultEntityEvent|rts; includes25,132..135 and followers when actually selected.|

No local flag/write/recruitment/HP gate is in these handlers. Mask bit0
requests the shared face-player wait; bit1 also requests facing restoration,
so129/130 restore while128/131 do not. RunMapSetupEntityEvent owns
portrait/speech setup, entity-service suppression and close/reactivation
cleanup. Speaker is the actual selected entity/sprite, not a new hardcoded
Astral or Sheela portrait. Church is reached only after both131 texts
return; its selection/exit and return to this rts follow the
[service contract](../design/contracts/service-interactions.md) and
[menu/save owner](./common-menus.md). The local call does not automatically heal,
save, recruit or prove a working church UI. Inventory/HP/fees and menu
choice remain supplied service inputs.

The selected `church/churchactions_1.asm:ChurchMenu` saves registers,
uses CURRENT_PORTRAIT, shows shared greeting110 and runs MENU_CHURCH.
Diamond result−1 reaches @ExitMenu: shared farewell113, window closure,
frame/register restoration and rts back to Event3, then outer entity-event
cleanup. Completed service branches can ask for another choice rather than
return immediately. The existing save path's continue result0 exits the
menu; nonzero fades and jumps WitchSuspend instead of this caller return.
No Dojo-specific branch overrides those shared conditions.

`s4_descriptions.asm` sets base `$104A`=4170, selects byte_60088 and jumps
DisplayAreaDescription. The sole msDesc21,33,5,0 plus end selects
investigation423+5=428 then description4170. The consumer matches candidate
coordinates, checks the record's search/action eligibility, displays the
investigation and description, closes portrait/text and returns d7=−1;
no-match returns0. This is a well description, not an item/join trigger.
`s5_itemevents.asm` has only default→Map45_DefaultItemEvent0/rts. The
shared item callback starts d6=0; returning without changing it leaves the
ordinary generic item-use fallback, not special-item success or removal.

Complete map item tables:

| Table / coordinate | Collected flag | Item constant / ID |
| --- | --- | --- |
|chest26,42|189|MITHRIL /123|
|other5,19|186|VIGOR_BALL /122|
|other22,26|187|BRAVE_APPLE /15|

`exploration.asm:GetChestItem/GetNonChestItem/GetItem` reads live CURRENT_MAP
and coordinate bytes, returns flag/item and masked64×64 offset. CheckArea's
ahead-coordinate/layout-kind selection precedes the table: chest-kind$1800,
then container/other-item and area-description routes. A table coordinate
alone does not prove that search reaches it. CURRENT_BATTLE255 avoids battle
crop adjustment. OpenChest sets189 and working block `$D802` before award;
already-held189 substitutes NOTHING. CheckNonChestItem suppresses already-held
186/187, otherwise sets the matching flag before continuation.

All three items are below the gold threshold. The existing
`explorationfunctions_1.asm:loc_239C8` award path names the item, checks
Bowie's four slots and AddItem0 if room, else UpdateForce and scans the
counted other-force-member prefix for a free slot. It includes joined
reserve/dead members according to the actual rebuilt list; unjoined carriers
do not supply capacity. Successful award uses received/passed messages and
item music; no assumption that Bowie holds the item is made. All hands full
displays417, calls CloseChest **and** RefillNonChestItem at that coordinate:
chest189 closes/clears, other186/187 clears its collection flag. Interruptions
between selection, flag/block writes, award or repair are distinct prefixes,
not an atomic transaction. No items were actually granted by this audit.

## Zone admission and complete recruitment command order

**Confirmed:** `s3_zoneevents.asm` `$60060..$60078` declares exactly
zone255,17→Map45_ZoneEvent0 plus default→Map45_DefaultZoneEvent/rts.
255 is wildcard X; Y17 must match. Controlled esc02's zone-kind `$1400`
publishes type6/coordinates only through the actual marker/movement branch.
ProcessMapEventType6 passes the request to RunMapSetupZoneEvent. The latter
selects the ordered row, invokes callback, closes portrait/text, waits one
VInt and for physical0 to stop, then returns. The callback reads F25;
held→return_60076/rts; clear→script cs_600CE then that rts. It sets no
separate scene-complete latch. No F70/76/999/HP/inventory gate is implicit.

The complete173 source records of cs_600CE are below, including nested
motion/delimiters. `S`=Sheela25, `B`=Bowie0, `P`=Peter7, `Z`=Zynk26,
`F`=FOLLOWER_B31. AW means entityActionsWait, A means nonwaiting
entityActions, and each listed block includes its endActions. All text
calls increment the cursor from2083; their modes/speakers are listed next.

| Ordinals | Ordered source work |
| --- | --- |
|0–6|textCursor2083; setActscriptWait F,P,Z to eas_Init (three calls); single2083/S; camera1,1; single2084/S.|
|7–12|position Z8,12/Left; F6,11/Up; B6,13/Up; P6,14/Up; camera1,9; single2085/F.|
|13–18|AW F: Down1,end; single2086/F; setActscriptWait B then P,eas_DeactivateAutoFacing.|
|19–30|Three customActscriptWait blocks, B then F then P: ac_setSpeed4,4; ac_jump eas_Idle; ac_end.|
|31–41|A F: Down3,end; A B: eaWait20,Down3,end; A P: eaWait40,Down3,end. These three action streams can overlap.|
|42–50|csWait50; headshake B twice; wait10; headshake B twice; wait10; headshake B once; waitIdle B.|
|51–59|next2087/S; single2088/S; face F Up; single2089/F; setActscriptWait F,Init; AW F: Up4,end; single2090/F.|
|60–72|setActscriptWait B then P,Init; A B: Up4,Right1,Up1,end; A P: Up4,Left1,Up1,end; camera1,5.|
|73–81|single2091/S; face F Down; single2092/F; single2093/S; shiver F; face F Up; single2094/F; next2095/S; single2096/S.|
|82–91|AW F: Up1,end; next2097/F; single2098/F; setActscriptWait F,Jump twice; single2099/S; shiver F; single2100/F.|
|92–101|AW P: Right1,end; face P Up; face B Down; single2101/P; AW F: Down1,end; single2102/F.|
|102–110|quake3; wait40; quake0; wait20; face F,P,B Up (three calls); next2103/F; single2104/F.|
|111–119|shiver S; single2105/S,2106/F,2107/S,2108/F,2109/S; nod F; next2110/F; single2111/F.|
|120–128|shiver S; single2112/S,2113/F,2114/S; wait60; single2115/S; position134 at6,8/Up; camera entity S; single2116/S.|
|129–139|setActscriptWait S,DeactivateAutoFacing; face S Left; AW S: Up2,end; face S Down; startEntity S; setActscript S,Transparent; AW S: Down4,end.|
|140–145|hide134; setActscriptWait S,Init; setSprite S,S; AW S: Down2,end.|
|146–155|setActscript B,Jump; P,Jump; setActscriptWait F,Jump; repeat those three; startEntity S; wait30; setActscriptWait S,Init; camera1,5.|
|156–163|single2117/S,2118/F,2119/S,2120/F,2121/S; setActscriptWait F,Jump twice; single2122/F.|
|164–167|join25; single2123/P; face S Left; single2124/S.|
|168–172|follow P→B offset2; F→P offset2; Z→F offset2; S→B offset5; csc_end.|

Action installation csc14/15 and sequence csc2D resolve physical slots,
initialize their wait timer from slot number, install script addresses and
optionally wait for the **eas_Idle address**, not merely destination arrival.
csc2D additionally clears flagsA bits5/6 (the collision-disable flags),
restoring the sequence's entity/map collision tests. They
do not test ally HP/joined; waitIdle/csc16 also has no HP guard. Blocking,
interruption and alias reuse can change or prevent completion. Nonwaiting
Jump calls can overlap F's awaited Jump; source order is not a frame schedule.

setPos, setFacing and startEntity use AdjustScriptPointerByCharacterAliveStatus:
ally0..29 at HP0 skips operands/operation; selector31 and nonally134 bypass
that ally check. F may therefore refer to a different live alias or default0
when missing. csc2C follow reads the high byte of a word selector for its HP
precheck: these ordinary00xx operands test **Bowie0**, before resolving the
actual follower/leader. Dead Zynk is not independently excluded by follow;
dead Bowie skips all four. No source command manufactures missing actors.

At129 onward, Sheela's own position is relative to her actual current slot.
Nominal unblocked completed6,6→6,4→6,8→6,10 is only a source arithmetic
example. startEntity writes animation counter0; Transparent sets the
blinking/transparency flagsB bit, not alias removal. Its nonwaiting install
and the immediately following action install are distinct service seams;
executing the transparency byte before replacement is not established.
Init later resets orientation/collision/immersed fields and enables
auto-facing; it does not itself clear the transparency bit. setSprite S,S
uses actual ally sprite resolution, not hardcoded POSE7 or a class reset.

134 starts as the fixed OBJECT4 slot at63,63; setPos134 moves it6,8, and
hide134 later invokes HideEntity without an HP check. HideEntity sets
coordinates/destinations7000h, clears every alias for that physical slot toFF
and removes its follower-list entry if any. It does not delete the source
record, set25, change HP/items or hide135. A later fresh population reconstructs
134; retained room warp does not. These operations differ from the init's
MoveEntityOutOfMap25, and from a visible/BLANK-sprite change.

## Dialogue identities and control tokens

**Confirmed:** `data/scripting/text/gamescript.txt` and source text
commands yield this complete local selection. `N2` below means two ordered
`{N}` tokens; `N` means one. `single` means nextSingleText (which also closes
its text window), `next` means nextText. S/P flags00, F flags80. Ordinary
txt callbacks use the actual event speaker and shared cleanup. Summaries
are short paraphrases; original prose remains private.

| ID | Speaker/mode | Paraphrase | Ordered tokens |
| --- | --- | --- | --- |
|2077|128/txt|Monk studies|N2,W2|
|2078|128/txt|Training|W1|
|2079|129/txt|Back warning|N,W1|
|2080|130/txt|Mental control|N,W1|
|2081|131/txt|Former priest|N,W2|
|2082|131/txt|Offers help|W1|
|2083|S/single|Challenges visitor|W1|
|2084|S/single|Sacred place|W1|
|2085|F/single|Surprise|W1|
|2086|F/single|Retreat instruction|N,W1|
|2087|S/next|Stop|W1|
|2088|S/single|Recognizes Astral|W1|
|2089|F/single|Question|W1|
|2090|F/single|Identity question|N,W1|
|2091|S/single|Asks recognition|W1|
|2092|F/single|Apology|W1|
|2093|S/single|Former pupil|NAME25,N2,W1|
|2094|F/single|Recognition|NAME25,W1|
|2095|S/next|Chest scar|N,W2|
|2096|S/single|Recalls incident|N2,W1|
|2097|F/next|Scar explanation|N2,W2|
|2098|F/single|Private memory|NAME25,N2,W1|
|2099|S/single|Apologizes|W1|
|2100|F/single|Begins inquiry|W1|
|2101|P/single|Requests introduction|N,W1|
|2102|F/single|Agrees|W1|
|2103|F/next|Defers|W2|
|2104|F/single|Purpose question|N,W1|
|2105|S/single|Red Baron motive|N,W1|
|2106|F/single|Sympathy|N,W1|
|2107|S/single|Return question|N,W1|
|2108|F/single|Zeon purpose|W1|
|2109|S/single|Zeon question|W1|
|2110|F/next|Explains violence|NAME25,N2,W2|
|2111|F/single|World threat|N,W1|
|2112|S/single|Baron allegiance|W1|
|2113|F/single|Possibility|W1|
|2114|S/single|Considers|W1|
|2115|S/single|Decision|W1|
|2116|S/single|Eyes closed|W1|
|2117|S/single|Requests to accompany|N,W1|
|2118|F/single|Asks ability|N,W1|
|2119|S/single|Body training|N,W1|
|2120|F/single|Priest question|N,W1|
|2121|S/single|Master monk claim|N,W1|
|2122|F/single|Welcomes fighter|N,W1|
|446|join service|Force announcement|NAME,CLASS,N|
|2123|P/single|New ally|W1|
|2124|S/single|Western soldiers|N2,W1|
|428|search investigation|Looks into well|NAME,N,W2,CLEAR|
|4170|description|Deep well|W1|

NAME25 spells `{NAME;25}` and resolves the supplied combatant name;
NAME/CLASS spell `{NAME}`/`{CLASS}`. N/W1/W2/CLEAR spell their braced
tokens. No tokens are omitted or reordered by the shorthand. Text446
is the join service's own ID, not an increment of the2083 cursor.
F's speaker31 resolves current sprite/portrait/speech; the narrative name
Astral is not a guard on F70 or alias identity.

## Join effects, changed states and five returns

**Confirmed:** ordinal164 selects csc08_joinForce with0019 (25), no
sad-music bit15 and no special128 case. It clears speech, waits for view
scroll, requests MUSIC_JOIN, calls j_JoinForce25, reads current class/name
for446, waits through music/input return, closes text and sleeps10. It
does not consume the scene text cursor. `JoinForce` sets joined25 and
UpdateForce rebuilds all30 joined/active/reserve lists in identity order.
If rebuilt active count<12, JoinBattleParty sets57; at12+ it skips that write
without clearing a preheld57. Lists are rebuilt **before** that conditional
write, so a newly active25 can be absent from the current counted active
list until another rebuild. Preheld57 without25 becomes eligible when25
is written. No already-joined rejection is present in the join service.

Join calls no stat/item initializer, heal, status reset, alias allocation,
map-event publication or save. Sheela's existing HP/MP/class/status/items
survive it; sprite and movement were separate earlier commands. The later
follow operations append/update transient field following through AddFollower,
with offset2=(0,24) and offset5=(−24,−24), not a new persistent follower flag.
There is no Sheela25 row in the ordinary follower table. Callback return
does not prove post-join message acknowledgement, completed following or control.

| Supplied change / interruption | Conditional consequence |
| --- | --- |
|25 clear|Init attempts pose setup; admitted Y17 zone attempts full scene. Neither checks70/76/999.|
|25 held|Init moves current alias25 off map; zone returns without join/dialogue. Does not repair57 or followers.|
|25 toggled after init|Zone checks current25 independently; setting it skips despite visible pose, clearing it can run with25 off map.|
|25 HP0|Fresh nonimmortal healing skips revival; fixed sprite can be Blue Flame, pose setSprite still executes, final facing/scene positions skip; action waits and join do not. No revival inferred.|
|Z26 HP0 / P7 HP0|Z positioning/facing skip; its unguarded action install/follow remains separate. Fresh entry revives P; manually changed retained HP can still affect guarded commands.|
|B0 HP0|Guarded positions/facing skip; all four follow prechecks skip, but join25 has no HP guard.|
|Missing31 /duplicate follower flags|Fresh absent alias31 is0, potentially selecting player; duplicate31 maps to the later follower. Text/action/position intent does not validate identity.|
|Active count11 /12+, preheld57|Join may newly set57 below12; at12+ existing57 is retained and clear57 remains reserve. No inventory capacity test applies to recruitment.|
|All item slots full|Item award repairs189 or186/187 as above; recruitment is independent.|
|Interrupt before164 /after25 /before final return|Different actor/alias/party/text/follower prefixes; no scene-complete transaction or rollback. Subsequent zone25 guard can skip an unfinished scene.|
|Repeat zone with25 held|Immediate return; no ceremony/follower reinstallation. Directly calling cs_600CE bypasses this guard and is not the natural route.|

Complete `6-warp-events.asm` `$B2C90..$B2CBA`: five eight-byte rows
plus terminator, all no-scroll0 and facing/padding word:

| Order / source | Destination | Facing / lifetime |
| --- | --- | --- |
|0 /255,44|75,2,27|Down3 /fresh explicit map|
|1 /4,255|75,2,27|Down3 /fresh explicit map|
|2 /27,255|75,2,27|Down3 /fresh explicit map|
|3 /22,23|CURRENT255,15,52|Left2 /retained current map|
|4 /15,52|CURRENT255,22,23|Left2 /retained current map|

Wildcard negative coordinate bytes are matched by the shared source
consumer, with first-match order and terminator first; actual marker/collision
still gates publication. TARGET_CURRENT255 is distinct from coordinate255.
All five go through the no-scroll unwind/MainLoop seam, not the retained
scroll path used by Map74↔75. Map45 and Map75 have no battle rows.

CURRENT destination returns low-byteFF. ExplorationLoop's byte comparison
selects @MapIndexNotProvided: its common prefix **still heals** and clears
the event type/subtracts the step counter, fades, updates physical0's
position/facing/properties and raft state, resets sprite counters, reloads
the current area/display and sets base VInts, then reruns Map45 init.
It does **not** rebuild population, clear temp256..383, reload tilesets or
freshly set80. LoadMap's negative path preserves the mutable working layout;
area1/0 follows the new main position. Joined25 can consequently be moved
off map on an internal warp while aliases and its existing follower entry
remain. Unjoined25 replays pose setup at its retained position. hide134's
removed alias stays removed until a fresh population.

The three explicit75 exits use fresh exploration: CURRENT_MAP75/
CURRENT_BATTLE255, resources/population rebuild, temp clear/80 and its rts
initializer. The [predecessor](./post-crash-ship-and-nazca-overworld-return.md#map55-initializer-and-the-resulting-map75-context)
owns Map75 area/layout/flag-copy/event context. F999 still selects the
two crash-site copies; F25 neither selects them nor gates the entrance.
Ordinary65/67/70/76 rebuilds Caravan30/physical1, P7/2, F31/3, Z26/4;
Sheela's temporary following is lost because no table row declares25.
No Map55 departure scene runs on this direct75 return. On a later fresh45
entry the fixed25/134/135 records are built again, then held25 moves25 off
map and suppresses the zone. Final WaitForEvent uses the pending-request
priority and physical0 control route from the predecessor; usable control
and real movement away from2,27 remain **Unknown**.

## Actual compiler/content and current engine boundary

**Confirmed (direct offline execution):** the selected recipe imports
every defining Map45 file, including all local action/continuation labels,
before invoking the maintained
[OriginalPrograms](../../src/sf2tool/remake_exploration_programs.py) and
[compile_maps](../../src/sf2tool/remake_exploration_maps.py).
Each standalone root uses a fresh compiler with its defining file first.
No failed cache is reused or filled by synthetic action data.

| Selected root / conversion | Actual outcome |
| --- | --- |
|Event0/1/2|Complete5/3/3 instructions respectively (cursor/show per txt plus end).|
|Event3|6 instructions; native jsr ChurchMenu at4, then end.|
|Default entity/zone/item callbacks|Each one end instruction.|
|Map45_ZoneEvent0|3 instructions; return_60076 one; complete cs_600CE dependency339.|
|ms_map45_InitFunction|3 instructions; typed hide25/removeAliases=false; byte_600AA2, cs_600B2 five, return_600B0 one. Local labels resolve successfully here.|
|cs_600B2|Complete5: motion/sprite/motion/face/end-map-script; actions4 and16 for Immersed/InitFixedSprite.|
|cs_600CE|Complete339 compiler instructions ending end-map-script. Top-level native waitIdle at source50 and setSprite ally-selector at142; nested Transparent action has native ac_transparency. Emission is not executable completion.|
|ms_map45_AreaDescriptions /byte_60088|3 native instructions /2 native data declarations; no reached description transaction.|
|eas_5FFC4 and eas_5FFC8 action_stream|Both fail ValueError: selected source label is missing, despite both present in raw imported labels. action_stream reads only global eas_actions.asm; does not use that local raw import or join fallthrough/loop.|
|Actual event-enabled compile_maps45 from registered canonical export|Fails at eas_5FFC4 with the same ValueError after initial fixed/walking actions. No successful full Map45 content conversion or playable package is claimed.|

Compiler instruction counts include lowered text/window/ceremony actions;
they are not source-command counts, frames or completed game services.
Verified canonical input identity reaches the real local-action failure
above. Source and implementation are unchanged; authoring diagnostics
remain in the ignored handoff rather than changing these consumer results.

[ExplorationContentReader](../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs)
maps native-call to UnsupportedInstruction;
[ProgramRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs)
stops Unsupported at program-opcode. Its typed motion/pose/hide/camera/text
support still needs actual state/service binding. Position/facing/animation
and follow lowering do not add the original ally/high-byte HP guards.
Hide distinguishes removeAliases=false from actual alias-removing hide;
these are not leave-force operations.

[SceneEntities](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/SceneEntities.cs)
and [MapEntityAllocator](../../remake/src/Sf2.Remake.Domain/Maps/MapEntityAllocation.cs)
provide physical slots and alias allocation, but source HP-dependent Blue
Flame appearance is not established by their initial class sprite data.
Correct local actions, ordinary follower selection and all state guards must
be bound before claiming this population. Native church/search/item/description
services are not supplied by successful text lowering.

[MapPartyMembership](../../remake/src/Sf2.Remake.Domain/Maps/MapPartyMembership.cs)
models joined/active/rebuilt-list ordering; ProgramRunner JoinPartyMember
requires partyFlags and JoinResources requires unique member deployment/start
when encounter starts are present. No25 binding is established. Resource
admission is distinct from the source join's no-stat-reset behavior.
[MapTransfer](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs)
and MapEventDispatcher consume simple preserve/rebuild warp definitions,
but successful selection does not prove this map's reached services. Its
ReloadScene replaces working layout; the original negative LoadMap retains
it, as the predecessor records. Area fallback/profile/material bindings
also need assessment. Godot is a projection/service adapter, not evidence
that these source effects completed; no SDK/reader/engine/Godot run occurred.

Minimum later implementation gaps are local-action/fallthrough/loop import,
native waitIdle/transparency/dynamic ally sprite and service support,
HP/alias/follower/join-resource parity, search/church binding and actual
map materials/transfer lifecycle. This research authorizes none of those
repairs or a general new exploration platform. The encounter reader still
admits1/2/3 only; no later encounter is made ready by this document.

## Reproduction, preservation and Unknowns

After same-process configuration, use the existing registered source root
(directory containing sf2enums.asm/code/data) and canonical-export path:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1114/review.py <registered-source-root> --canonical <registered-canonical-export> --committed --receipt local/issue1114/direct-reproduction-01.json
uv run sf2 verify plan --base origin/main --head HEAD
```

Choose an unused ignored receipt path; --receipt is required and existing
outputs are rejected. The direct recipe checks selected full source tables,
action fallthrough, command order, tokens, consumers and changed-state
examples; actual compiler/content outcomes; pair/provenance unchanged;
two-path/navigation-only scope, UTF-8/LF, links and private boundary.
Clean committed planner and actual exact-head scoped CI are separate
receipts. Suggested gates are not executed passes. No normal/full/old
comparison/SDK/Godot/emulator/H3/H4/screenshots, new parser/fixture/schema
or source export is run/introduced.

Preserve #1112's genuine complete-import @Return partial and native crew/
RNG/mapLoad/changed-layout ReloadScene limits; #1110/#1108/#1106 corrected
and original failures stay in their accepted owners/ignored handoffs.
Twelve completed old-comparison failures are retained and not rerun or
described as interrupted. The selected compile_maps/local-action refusal
and native emissions above remain explicit; no cache/source workaround
turns them into a passed playable-content check.

**Unknown:** natural entry/zone/search/interaction/warp reach; actual
flags, HP/MP/status/class/items/party counts, aliases/follower/player/raft
state and request delivery; binary layout/collision/markers; scheduling,
movement completion, VInt/DMA/sprite/pose/transparency/roof/camera/palette/
text/portrait/audio/rendering completion/timing; usable control, save/load
and persistence. Static evidence and source arithmetic do not close these
questions or make the Dojo a required main-route step.

Freeze a clean committed/pushed Draft PR with exact head/tree/base,
checks/actual CI, preserved failures/Unknowns and stopped process state.
Project Review / Verify and successful notification to assigned main-gate
precede task stop. Independent acceptance/merge stays with main-gate;
no next Issue, merge, cleanup or archive.
