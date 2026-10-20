# Nazca ship boarding, exploration and departure admission

Evidence date: 2026-10-20. **Confirmed (static):** the Map55 boarding
program sequentially installs the shrinking action on Bowie, Peter and
FOLLOWER_B, conditionally on Zynk, then requests Map37 at29,8/Left.
The first-visit ship setup allocates fixed crew positions and suppresses
ordinary followers. Using the Sky Orb at8,11/Left dispatches the voyage
only while999 is clear and836 is set. Neither a flag nor a displayed Orb
is inventory possession. **Stop at the call to cs_5FB6A; its body is not
covered.** Natural reach, completed services, presentation, usable control
and saved persistence remain **Unknown**. Current lowering rejects this
boarding program before producing a usable program; no playable ship is
established.

## Agreement and evidence route

Standard rigor for [Issue #1106](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1106)
under [#1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
provide source-backed entry, exploration, reward, exit and launch conditions
for later content/engine decisions. Accepted base is
`05a52989bfe7dc31c97840c9ab119071e371b396`, including
[Battle34 outcomes and Sky Orb custody](./battle34-outcomes-and-sky-orb-handoff.md).
Own this document and navigation only in that predecessor. No shared
index, registry, schema, fixture, tool, design or engine change; no stacked
dependency or competing UI692 path.

Include complete cs_5E27C/eas_5E2C4,999-clear Map37 setup, its area and
event/item tables, selected initialization/allocation/quote/item helpers,
exit and Orb callback.999-set setup selection is a later-story boundary;
only the shared default entity handler physically stored in its file is
needed here. Exclude its other dialogue/init bodies, voyage/shootdown,
later encounters, full-game quote acquisition and runtime services.

Original paths below are relative to `disasm/` at clean, read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
USA ROM SHA-256 remains
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`;
this slice makes no new ROM/H1/runtime equality claim. `sf2enums.asm` and
`sf2const.asm` resolve constants and RAM widths. `map/` and `entity/`
abbreviate `code/common/scripting/`; `exploration/` abbreviates
`code/gameflow/exploration/`. Fnnn means flag nnn, ordinals are zero-based,
address intervals half-open, text IDs decimal unless prefixed with `$`.

Reuse [map request production](./map-event-request-state.md),
[consumption](./map-event-request-consumption.md),
[common maps](./common-maps.md), [common scripting](./common-scripting.md)
and [tactical quotes](./map-event-tactical-base-quote-state.md), including
its existing fixture. The immutable18,285,979-byte init/script pair and
0.083s provenance at `local/issue1005/pair.txt` are reused, not regenerated.
Bounded linear source/table reads, one pair load and selected in-memory
lowerings are budgeted at one minute/512MiB per direct audit; small raw
receipts stay ignored under `local/issue1106/`. No network data acquisition,
emulator, SDK, Godot, runtime, screenshots, old comparison or new platform.
Reassess a material scope/resource deviation before expansion.

## Boarding request and actor preconditions

**Confirmed:** `data/maps/entries/map55/mapsetups/s3_zoneevents.asm`
selects7,7 for an actual zone-event request and reads534; clear534 returns,
held534 invokes cs_5E27C. The accepted predecessor owns this admission.
The complete selected boarding prefix in `map55/mapsetups/scripts.asm`
occupies `$5E27C..$5E2C4`; the action occupies `$5E2C4..$5E320`.

| Order / label | Operation and boundary |
| --- | --- |
|0|Install eas_5E2C4 on ALLY_BOWIE0 and wait for idle.|
|1–2|Install eas_Init on ALLY_PETER7, wait; then eas_5E2C4, wait.|
|3–4|Install eas_Init on FOLLOWER_B31, wait; then eas_5E2C4, wait.|
|5|Read F76 independently; clear jumps to cs_5E2BC. No836,999 or inventory read occurs here.|
|6–7, held76|Install eas_Init on ALLY_ZYNK26, wait; then eas_5E2C4, wait.|
|cs_5E2BC|Write warp request Map37,29,8,LEFT2, then csc_end.|

`map/mapscriptengine_1.asm:csc15_setEntityActscript` resolves the logical
character through GetEntityAddressFromCharacter, stores the returned
physical slot byte in ACTSCRIPTWAITTIMER, installs the action pointer and
busy-waits until that pointer equals eas_Idle. This opcode has no
AdjustScriptPointerByCharacterAliveStatus, joined, active or missing-alias
test. All admitted actors are attempted even with zero HP. Bowie does not
receive a new eas_Init first; incoming motion/speed/flags matter. The other
selected actors get its resets before shrinking. F76 only chooses whether
the two Zynk installations occur; it does not validate his alias or HP.

GetEntityAddressFromCharacter masks the selector, maps negative enemy
selectors by subtracting96 and reads ENTITY_INDEX_LIST, then scales the
physical slot by32 bytes. A zero/stale alias is not rejected and can select
the player or another slot. FOLLOWER_B is logical31, not physical31 and
not the ship's later Astral128. The Map55 follower source includes Peter
under67, Astral/FOLLOWER_B under70 (also other conditional rows) and Zynk
under76; allocation and duplicate aliases depend on current flags. The
scene's previous follow chain does not establish fresh Map55 aliases.

The complete eas_5E2C4 order is:

1. Set entity uncollidable OFF, thus clear flags-A bit5; write absolute
   destination7,7, then wait6.
2. Enable resize (flags-B resize bit); set global SPRITE_SIZE to22 and
   request sprite update, wait1; repeat20,18,16,14, each followed by
   sprite update and wait1; set12 and request sprite update.
3. Wait for actual destination equality, set mapsprite BLANK60, request
   sprite update, wait1, then jump to eas_Idle.

These are26 action macros/92 encoded bytes; command count is not elapsed
time. `esc05_moveToAbsoluteDest`
checks shared destination obstruction (skipped when flags-A bit5 is clear),
then writes destination/travel/velocity using384 units per tile and current
speed; it does not itself wait for arrival. `esc01_waitUntilDestination`
compares both current
coordinates against destination. `esc00_wait` uses the actor's byte timer.
`esc0B_setSpriteSize` writes shared SPRITE_SIZE, not a per-actor scale;
`esc1F_setResizable` toggles resize; `esc17_setSpriteNumber` changes the mapsprite.
BLANK plus update is not HideEntity: the program does not erase aliases,
remove interaction/follower entries, set HP or change membership. The
six size requests and their waits do not prove visible shrinking or elapsed
hardware timing. A blocked movement or unserviced action can prevent the
later warp indefinitely; there is no timeout in this source loop.

`map/mapscriptengine_2.asm:csc07_warp` writes MAP_EVENT_TYPE.w=1 followed
by five bytes0,37,29,8,2. `explorationfunctions_2.asm:ProcessMapEventType1_Warp`
takes parameter1=0 through its script-warp path: request WARP_SFX, clear
it, idle player, unwind exploration, seed registers from the request and
return to MainLoop. It does not synchronously load Map37. The next
MainLoop checks map switching/battle admission before ExplorationLoop.
Positive37 performs fresh map/entity construction and setup init; it is
not the retained-map d0=$FF path. ExplorationLoop first calls
HealLivingAndImmortalAllies: living allies are restored, Peter7/Lemon28
are restored even from zero HP, and other dead allies stay dead. The
fresh path loads tilesets, constructs entities, loads mapsprites, clears
temp flags256–383 and sets80, then loads the map, installs VInt services
and runs the selected init before conditional music/fade and event waiting.
These entry effects do not set836/999/76. HP cases below refer to the
actual service seam after any preceding heal. Request publication, return from the
interpreter, map construction and restored control are separate seams.

## First-visit Map37 setup, area and entity allocation

**Confirmed:** `data/maps/mapsetups.asm` selects ms_map37 by default and
ms_map37_flag999 when F999 is set. GetCurrentMapSetup rereads live flags
on each service call, so selection is not a promise made only at entry.
`map37/mapsetups/pointertable.asm` (`$5F630..$5F660`) contains the same
six pointers twice: Entities, EntityEvents, ZoneEvents, AreaDescriptions,
Section5, InitFunction. The selected six-pointer tuple is24 bytes;
the physical repeated block is48 bytes. `pointertable_999.asm`
(`$5F660..$5F678`) substitutes entities/events/init and shares zone,
description and item pointers. Its scene bodies remain outside this slice.

`map37/2-areas.asm` (`$AEECC..$AEEEC`) has one area: main start0,0,
end31,19; second foreground start0,0/background start48,44; both parallax
256,256 and autoscroll0,0; type255 and MUSIC_SHRINE. These area bounds
are distinct from the64×64 working-layout address space and from rendered
walkability. Flag/step/roof tables are terminator-only at `$AEEEC..$AEEF2`.
No local flag-copy, step-copy or roof record is declared.
`data/maps/entries.asm:Map37` binds Map37s1_Layout to the local
1-layout.bin and the named area/event tables. No new binary layout decode
is claimed here; concrete working block words, collision and marker
admission remain separate from this static table/area inventory. The chosen
description routine returns directly; the zone table contains only its
default rts. A missing local event does not remove shared field behavior.

InitializeMapEntities clears entities/aliases and calls
InitializeFollowerEntities. Its explicit Map37 comparison returns before
the ordinary follower loop, independently of follower-unlock flags.
The selected s1 table (`$5F678..$5F772`) has31 fixed records of8 bytes
plus a two-byte terminator. It allocates all records regardless of joined,
active or HP. With ordinary player type/no raft declaration, the complete
allocation is below; every record uses eas_Init. D/U mean Down3/Up1.

| Record / physical slot | Logical character | Position / facing | Entity event |
| --- | --- | --- | --- |
|0 /1|Sarah1|16,6 D|0|
|1 /2|Chester2|17,6 D|1|
|2 /3|Jaha3|18,6 D|2|
|3 /4|Kazin4|19,6 D|3|
|4 /5|Slade5|20,6 D|4|
|5 /6|Kiwi6|21,6 D|5|
|6 /7|Peter7|17,12 U|6|
|7 /8|May8|22,6 D|7|
|8 /9|Gerhalt9|23,6 D|8|
|9 /10|Luke10|17,9 D|9|
|10 /11|Rohde11|22,12 U|10|
|11 /12|Rick12|19,9 D|11|
|12 /13|Elric13|20,9 D|12|
|13 /14|Eric14|22,9 D|13|
|14 /15|Karna15|23,9 D|14|
|15 /16|Randolf16|19,12 U|15|
|16 /17|Tyrin17|20,12 U|16|
|17 /18|Janet18|16,15 U|17|
|18 /19|Higins19|17,15 U|18|
|19 /20|Skreech20|18,15 U|19|
|20 /21|Taya21|19,15 U|20|
|21 /22|Frayja22|20,15 U|21|
|22 /23|Jaro23|21,15 U|22|
|23 /24|Gyan24|22,15 U|23|
|24 /25|Sheela25|63,9 D|default|
|25 /26|Zynk26|16,9 D|24|
|26 /27|Chaz27|63,5 U|default|
|27 /28|Lemon28|63,5 U|default|
|28 /29|Claude29|15,6 D|25|
|29 /30|Astral128, sprite209|16,12 U|26|
|30 /31|Caravan129, sprite62|23,12 U|27|

Bowie0 is constructed separately in physical0 using the entry seed
29,8/Left and GetAllyMapsprite. Nonally aliases128/129 use index-list
offsets32/33, distinct from logical follower31. No ordinary follower is
appended to EXPLORATION_ENTITIES. `sub_44404` still runs after fixed
allocation: raft player type changes the player's sprite and declares raft
in physical31; or F64 with RAFT_MAP=37 declares it there using saved
coordinates. This can overwrite Caravan's physical slot while its129
alias still points there. The ordinary table above therefore is not a
universal guarantee under changed raft state. This slice does not sanitize
or repair that original conditional overlap.

`ms_map37_InitFunction` (`$5FA88..$5FA9C`) always calls
InitializeNazcaShipForceMembers, then independently reads836; held836
invokes cs_5FA9C (`$5FA9C..$5FAA4`). The initializer loops29 physical
slots starting OTHER_ENTITIES_DATA while testing flags1 through29.
Clear joined flag writes `$5E80` (63×384) to X/Y/XDEST/YDEST, facing-byte
Down and eas_Idle; held flag leaves the slot alone. It does not test active
flags32+n or HP, repack slots, remove aliases or update sprites itself.
Its loaded tactical-position pointer is unused; this is not the active
party seating routine InitializeTacticalBaseForceMembers.

cs_5FA9C requests setPos Zynk26,8,10,Right0. This opcode does call
AdjustScriptPointerByCharacterAliveStatus: zero HP skips the four-byte
character/position/facing operands for an ally; nonzero HP resolves the current alias and
writes the position/facing. It does not check joined26 or follower76.
Thus held836/nonzero HP can reposition an unjoined26 after the initializer
moved it off-map; clear836 leaves the selected fixed/hidden position.
Allocation, joined visibility, active quote choice, HP skip and logical
alias are independent conditions. Neither location implies natural reach.

## Crew interactions and selected quote rows

**Confirmed:** s2 (`$5F86C..$5F9D2`) has28 four-byte entity-event records
plus one default. Direction-looking operands encode handler flags, not
required player facing: Down3 enables both facing/restore bits, Up1 only
the first, Right0 neither. RunMapSetupEntityEvent matches the logical
selector, resolves portrait/speech, resolves its current physical alias,
performs the flags' facing waits, suppresses entity updates, invokes the
handler, then restores/closes/reactivates as prescribed. Request admission
still requires an actual interaction producer; a table row alone is not
proof that its character is present or reachable.

The actual `explorationvints.asm:ProcessPlayerAction` idles player0 and
waits for movement/view before reading A/C branches. A opens the field
menu; ordinary C first checks the special Caravan-overlap shortcut using
physical slot1. In this fixed setup that slot is Sarah, not Caravan.
The ordinary path calls GetActivatedEntity (one-tile facing offset,
counter-marker continuation, physical distance search excluding player
and followers), GetEntityEventIndex (reverse alias lookup) and
j_RunMapSetupEntityEvent. If no entity is selected, it calls CheckArea
before possible menu entry. Ship Caravan129/physical31 is therefore not
that slot1 shortcut. Debug and malformed-alias paths are not natural
interaction evidence.

The25 selected tactical callers are events0–23 and25. Each is exactly
moveq of its table ally followed by tail jmp DisplayTacticalBaseQuote.
The existing [quote owner](./map-event-tactical-base-quote-state.md) and
`tests/fixtures/h2/map-event-tactical-base-quote-state-static-v1.json`
retain the complete54 registered caller corpus. The additional local
sub_5F9A0/5F9AE/5F9B6 code names Sheela/Chaz/Lemon but has no selected
entity-event row; it must not inflate the25 count or replace the default.

DisplayTacticalBaseQuote opens the name window, reads HP.w, and selects1
if zero. Otherwise live active flag32+ally chooses active `$DC3+ally` or
reserve `$DE1+ally`; joined is not checked by the quote function. It then
displays text and closes the name window. The following are short authored
paraphrases; each ID has its complete ordered tokens. The common dead
row1 is a silent response with `["{W1}"]`.

| Event / ally | Active ID / short summary / ordered tokens | Reserve ID / short summary / ordered tokens |
| --- | --- | --- |
|0 /1|3524 / leader survival warning / `["{N}","{N}","{W1}"]`|3554 / Jaha height remark / `["{N}","{NAME;3}","{W1}"]`|
|1 /2|3525 / eager to fight / `["{N}","{W1}"]`|3555 / height comparison / `["{N}","{NAME;3}","{W1}"]`|
|2 /3|3526 / military readiness / `["{N}","{W1}"]`|3556 / Sarah height complaint / `["{NAME;1}","{N}","{W1}"]`|
|3 /4|3527 / Hawel strategy praise / `["{N}","{N}","{W1}"]`|3557 / magic fatigue / `["{N}","{W1}"]`|
|4 /5|3528 / stealth boast / `["{N}","{W1}"]`|3558 / treasure plans / `["{N}","{N}","{W1}"]`|
|5 /6|3529 / exhaustion / `["{N}","{N}","{W1}"]`|3559 / dodge practice / `["{N}","{N}","{W1}"]`|
|6 /7|3530 / Phoenix revival claim / `["{N}","{W1}"]`|3560 / reputation worry / `["{LEADER}","{N}","{N}","{W1}"]`|
|7 /8|3531 / centaur archery / `["{N}","{W1}"]`|3561 / city curiosity / `["{N}","{N}","{W1}"]`|
|8 /9|3532 / unarmed strength / `["{N}","{W1}"]`|3562 / growling practice / `["{N}","{W1}"]`|
|9 /10|3533 / aerial attack enthusiasm / `["{N}","{W1}"]`|3563 / prefers combat duty / `["{N}","{W1}"]`|
|10 /11|3534 / cannon enthusiasm / `["{N}","{W1}"]`|3564 / ancient transport admiration / `["{N}","{N}","{W1}"]`|
|11 /12|3535 / centaur combat pride / `["{N}","{N}","{W1}"]`|3565 / adventure excitement / `["{LEADER}","{N}","{N}","{W1}"]`|
|12 /13|3536 / anti-air archery / `["{N}","{N}","{W1}"]`|3566 / postbattle rest / `["{N}","{W1}"]`|
|13 /14|3537 / training and technique / `["{N}","{N}","{W1}"]`|3567 / centaur self-ranking / `["{N}","{N}","{W1}"]`|
|14 /15|3538 / ready for devils / `["{N}","{W1}"]`|3568 / Jaha affection / `["{NAME;3}","{N}","{W1}"]`|
|15 /16|3539 / combat challenge / `["{N}","{W1}"]`|3569 / reserve frustration / `["{N}","{W1}"]`|
|16 /17|3540 / magic contribution / `["{N}","{W1}"]`|3570 / generational puzzlement / `["{N}","{N}","{W1}"]`|
|17 /18|3541 / courage despite fear / `["{N}","{N}","{W1}"]`|3571 / Elric companionship / `["{N}","{NAME;13}","{W1}"]`|
|18 /19|3542 / recovered readiness / `["{N}","{W1}"]`|3572 / vampire worry / `["{N}","{W1}"]`|
|19 /20|3543 / aerial support / `["{N}","{N}","{W1}"]`|3573 / rapid growth / `["{N}","{N}","{W1}"]`|
|20 /21|3544 / natural power / `["{N}","{W1}"]`|3574 / Mitula age / `["{N}","{W1}"]`|
|21 /22|3545 / anti-evil resolve / `["{N}","{N}","{W1}"]`|3575 / excitement / `["{N}","{W1}"]`|
|22 /23|3546 / self-interested combat / `["{N}","{N}","{W1}"]`|3576 / food appreciation / `["{W1}"]`|
|23 /24|3547 / frontline toughness / `["{N}","{W1}"]`|3577 / travel souvenir / `["{N}","{N}","{W1}"]`|
|25 /29|3552 / ancient magical purpose / `["{N}","{N}","{W1}"]`|3582 / body cleanliness / `["{N}","{N}","{W1}"]`|

Zynk/event24 bypasses this HP/active quote function and requests3376
(cockpit instruction). Astral128/event26 requests11 (caution to leader)
and falls through to event27's rts; Caravan129/event27 is just rts. That
empty handler still has the caller's window/event suffix. Neither recruits,
launches, opens a Caravan menu nor checks Orb possession.

The shared Map37_DefaultEntityEvent is physically in
`s2_entityevents_999.asm`, but the first-visit table explicitly references
it. It sets RNG range4, calls GenerateRandomNumber, adds d7 to `$EAC`
(3756) and calls DisplayText, then falls through to the shared return.
Rows3756–3759 are brief discomfort responses; each has `["{W1}"]`.
This is a selected default code join, not admission of the alternate
setup's other conversations. Actual RNG state and chosen row are Unknown.

## Search tables and Nazca Cannon chest185

**Confirmed:** the complete chest table (`$AEEFC..$AEF02`) has one
four-byte row25,8,185,NAZCA_CANNON51 plus terminator.185 is a collected
flag, not a chest ordinal or item index. Other-items (`$AEF02..$AEF04`)
is terminator-only. There is no Sky Orb search pickup or additional local
item-event row. The layout marker and producer still control searching.

`explorationfunctions_0.asm:CheckArea` derives the candidate one tile ahead
of the view-target entity using actual pixel coordinates/facing, reads its
layout word and tests marker `$1800` under mask `$3C00`. That branch calls
OpenChest, then403. `exploration.asm:GetChestItem/GetItem` searches the
current map's four-byte table by coordinate; in ordinary exploration the
selected25,8 row resolves flag185/item51 and word offset
`2*((8&63)*64+(25&63))` into working layout. The helper's battle-coordinate
adjustment is outside ordinary exploration, not an alternative ship rule.

OpenChest tests185, substitutes NOTHING127 if held, sets185, writes the
open-chest block and requests the appropriate plane update **before**
inventory admission. Held185 therefore selects408 (empty). Clear185
proceeds through CheckArea's `loc_239C8` continuation in
`explorationfunctions_1.asm`:51 is below the gold range, so413, room check
on Bowie0, and AddItem if fewer than four held items. It requests415,
item music and input/fade wait. AddItem's result is not rechecked here.

If Bowie is full, UpdateForce rebuilds joined-member lists; it then scans
OTHER_FORCE_MEMBERS_LIST with TARGETS_LIST_LENGTH−2 loop counter, choosing
the first with fewer than four items. HP and active state do not filter
that search. Successful add requests416. If all candidates are full,417
is followed by CloseChest and RefillNonChestItem at the saved coordinates;
CloseChest clears185 and restores the closed block, while the absent
other-items row makes refill inapplicable. There is no mandatory discard
dialogue on this path. Cancellation, interruption after open/before close,
unchecked AddItem failure and malformed lists do not gain transactional
rollback from these instructions. No spare-room or successful award is
inferred merely from the table.

## Sky Orb producer, callback and launch boundary

**Confirmed:** this is a field Item > Use callback, not a zone request
caused by standing in the cockpit. The actually included
`code/common/menus/item/isitemusableonfield.asm` and
`data/stats/items/usableoutsidebattleitems.asm` select ordinary usable
field items; Sky Orb113 is absent, so IsItemUsableOnField returns d2=−1
and mainactions.asm enters the map-event branch. UpdateForce supplies the
current joined list to the item selection screen, including reserves and
without an HP filter. The returned member, slot and item are stored;
cancelled member=−1 returns to the item submenu before callback dispatch.

GetPlayerEntityPosition uses physical player0 current X/Y divided by384
and facing masked by3, independently of the inventory holder's position.
The producer passes d1/d2/d3 position/facing, d4 selected item and d5 slot
to j_RunMapSetupItemEvent → RunMapSetupItemEvent. The alias is a returning
jump-interface shim, not another producer. The callback consumer saves
d0–d5/d7–a1 but deliberately leaves d6 as the result, clears speech SFX,
masks d4 by ITEMENTRY_MASK_INDEX127, stores event-relative position and
initializes d6=0. It selects the live setup and scans six-byte rows in order.
X/Y/facing may have FF wildcards in the general table format; item must
match. This selected row uses exact8,11,LEFT2,SKY_ORB113, then the default.
Coordinates refer to the player's tile, not the tile in front of it.

`s5_itemevents.asm` (`$5FA2A..$5FA88`) has that one row and a default
record. The default handler is rts, leaving0. Selected Map37_ItemEvent0:

| Condition / ordered work | Result at this boundary |
| --- | --- |
|F999 held|Immediate return; d6 remains0. No item-name write,836 read, text or voyage call.|
|999 clear|Set DIALOGUE_NAME_INDEX_1.w=113; independently read836.|
|836 clear|Request3373; close text; copy CURRENT_SPEECH_SFX to SPEECH_SFX_COPY; resolve logical128 through GetEntityPortaitAndSpeechSfx; store returned portrait/speech, request DisplayCurrentPortrait; request3374.|
|836 held, byte_5FA76|Request3375; close text; invoke cs_5FB6A. **Stop before its body.**|
|loc_5FA84, after either returning branch|Set d6=−1; rts. The voyage branch reaches this only if its script returns.|

Logical128 ordinarily aliases physical30/Astral, but portrait/speech lookup
reads the currently aliased entity's mapsprite and its dialogue properties;
it is not a hardcoded portrait number. The source copies the prior speech
value, then replaces current speech/portrait. The outer consumer subsequently
closes portrait/text, waits VInt and waits for player movement to stop.
Those requests do not establish completed UI/audio services or restoration
of every prior scratch value.

After RunMapSetupItemEvent returns, mainactions tests d6: nonzero exits
the main menu; zero requests73 then422, closes text and resumes the Item
submenu. A nonmatching coordinate/facing/item or held999 therefore falls
through the generic nothing-happened path for an actual item-use request.
The callback does not set836/999/76/534, remove the Orb, check the holder,
or validate inventory. Its caller's nonzero exit also does not remove the
item; removal in the distinct ordinary-use target branch is not reached.
Thus no Orb consumption occurs in the audited prefix or returning failure
path. Consumption, flags and effects **inside cs_5FB6A remain outside this
boundary**; a successful full voyage's custody is not established here.

An injected d4=113 request can match without possession because the
consumer never rereads inventory. Natural menu selection ordinarily
provides an existing inventory entry; this is a producer precondition,
not a callback guard. Held534/836 with no Orb, or Orb possession with
clear836, are different states. Zynk's spoken cockpit advice does not
prove the use conditions have been met.

## Leaving and possible later reboarding

**Confirmed:** `map37/6-warp-events.asm` (`$AEEF2..$AEEFC`) has one
eight-byte no-scroll record29,8→Map55,7,7,UP1 and terminator, with no
story/item condition. WarpIfSetAtPoint is called by the controlled-movement
producer only when the candidate layout marker is `$1000`; matching the
table then writes a type1 request. Parameter1=0 follows the same
exploration-unwind/new MainLoop route described above. Existing position
29,8 alone is not a producer call, including the boarding spawn there.

Fresh positive55 rebuilds its empty-fixed setup and currently unlocked
followers. Its init reads999: clear returns; held dispatches the later
Map55 return programs owned beyond this first-visit boundary. MainLoop
also evaluates battle admission; the accepted Battle34 owner specifies
the separate434 unlock condition. With ordinary post-victory clear434,
the7,7 seed itself does not automatically run the534-gated zone script.
`esc02_controlCharacter` evaluates candidate movement/layout markers,
calls the warp helper for `$1000`, or publishes type6/zone request for
`$1400` using destination coordinates. Zone-event table membership is
not polled merely because the actor's spawn equals7,7. Reboarding needs
a later actual zone request plus held534; neither arrival nor534 alone
proves an automatic loop. Interrupted requests/control handoff remain
Unknown; no automatic repair or trigger is added to the original facts.

## Changed-state checks and current consumer boundary

The direct audit checks complete selected source sequences/tables and
these small counterexamples; they are static derivations, not runtime
observations:

| Change | Source-backed consequence |
| --- | --- |
|F76 clear/set; Peter or Zynk HP0|76 chooses only Zynk boarding pair. setActscriptWait does not HP-skip even when setPos later would.|
|No/stale FOLLOWER_B31 alias|Boarding resolver can select slot0/another slot; no membership or alias rejection.|
|Joined26 clear, HP nonzero,836 held|Init first hides slot26 at63,63; cs_5FA9C can reposition it8,10. With HP0, setPos skips instead.|
|Joined held, active toggled, HP toggled|Ship initializer leaves slot; live quote chooses active/reserve, dead quote always1; none implies the other flags.|
|F999 held at service selection|Alternate setup selected; shared Orb callback itself returns0 before836. Other alternate scenes are deferred.|
|F64/RAFT_MAP37 or raft player type|Postallocation raft declaration can reuse physical31, invalidating the ordinary Caravan identity assumption.|
|Wrong X/Y/facing/item;113 with extra entry bits|Mismatch selects default0; matching consumer masks item to127 first. No item-use request means no callback at all.|
|999 clear/836 clear vs held|Failure dialogue and−1 versus launch dispatch followed by−1 only on return; neither branch in this prefix removes inventory.|
|Chest185 held vs clear/all full|Empty127 versus tentative open/award; all-full closes and clears185.|
|Return at7,7 without new zone request|No table-driven auto-reboard is proved; actual producer and534 remain necessary.|

**Confirmed (current code):** Content/Domain/Application structures can
represent fixed entities/aliases, flag-dependent setups, areas/layouts,
ordinary warp/zone/interact events, HP/flags/text, shared sprite size and
action waits. `MapEntityAllocator` in Domain's `Maps/MapEntityAllocation.cs`
can describe the fixed-slot allocation, and Application's SceneEntities
uses it with validation. Its current population builder imports ordinary
flag-driven followers and SceneEntities.Build filters them by flags without
a Map37 exclusion; that source special case still needs a binding. That
capability does not import Map37, suppress
its followers or implement InitializeNazcaShipForceMembers automatically.

The actual `OriginalPrograms` compiler (`src/sf2tool/remake_exploration_programs.py`)
fails selected cs_5E27C at `selected source label is missing: eas_5E2C4`:
action_stream searches its fixed shared action file, whereas this stream
is local to map55/scripts. Selected ms_map37_InitFunction separately fails
`source program target not imported: @Return`; register_file imports
global labels only. These observed static failures are retained; neither
is repaired or bypassed to claim a usable lowering. There is no complete
boarding/init compiled program.

Applying the compiler's existing action function to the26 selected action
macros alone retains native ac_moveAbs, ac_resizable, ac_waitDest,
ac_setSprite and ac_jump. Shared flags, wait, size and refresh map to
typed actions; isolated action mapping is not a successful action-stream
import or a native implementation. Successful selected lowerings are:

| Program | Emitted instruction count / limit |
| --- | --- |
|cs_5FA9C|2: position and end-map-script; no full init wrapper.|
|Map37_EntityEvent0|1: native jmp DisplayTacticalBaseQuote; no typed quote implementation.|
|Map37_EntityEvent24|3: text/close/end; imported text alone does not supply Orb use.|
|Map37_EntityEvent26|3: text/close/jump to event27.|
|Map37_EntityEvent27|1: end.|
|Map37_DefaultEntityEvent|3: native jsr, add.w, jsr; no accepted random-text callback.|

The successful cs_5FA9C lowering also omits the original HP-dependent
setPos skip and sprite-update request. Its `position` instruction becomes
SetEntityPosition; ProgramRunner directly edits X/Y/destination/facing
without the source HP check or UpdateEntitySprite_0 continuation. Its
two instructions therefore establish representation only, not source
equivalence for a dead or unjoined Zynk.

`ExplorationContentReader.cs` maps native calls/actions to
UnsupportedInstruction/UnsupportedEntityAction. ProgramRunner stops with
`program-opcode`; EntityActionRunner stops with `entity-action` rather
than acknowledging the missing service. Its global-size/refresh operations
wait on actual SpriteRequest/SpriteReady. The Godot presentation adapter
in `remake/game/src/Exploration/ExplorationPresentation.cs` handles its
existing sprite-ready requests; this does not implement Nazca resize,
blank-sprite replacement, menu item callbacks or quote services.

`ExplorationDefinition.cs` event kinds are Step/Interact/Warp/SourceZone,
with no item-use callback kind or search/chest award flow. `compile_maps`
in `remake_exploration_maps.py` lowers selected ordinary entities/warps/
entity/zone events; it does not consume this s5 match/callback protocol.
Representing flags or item inventory in battle code does not bind field
member/slot selection, d6 continuation, chest185 rollback or Orb custody.
PrivateBattleEncounterReader pins only encounters1/2/3; no composed34/37
package, travel completion or runnable later story is established. Minimal
future gaps are local action import/native operations, setup initializer,
quote/random text and field item/search consumers plus concrete content
binding. They are implementation boundaries, not automatically dispatched
work or a reason to launch runtime in this slice.

## Text keys, verification and retained limits

All rows are short authored summaries plus complete ordered tokens from
the pinned gamescript; raw lines remain ignored. Dialogue is not a rule
source for conditions, item removal, reachability or service completion.

| ID | Short authored summary | Ordered tokens |
| --- | --- | --- |
|1|silent response|`["{W1}"]`|
|11|leader caution|`["{LEADER}","{W1}"]`|
|73|item-use acknowledgement|`["{NAME}","{N}","{ITEM}","{W2}"]`|
|403|chest opening|`["{NAME}","{W2}","{CLEAR}"]`|
|408|empty chest|`["{W1}"]`|
|413|item discovery|`["{NAME}","{N}","{ITEM}","{W2}","{CLEAR}"]`|
|415|item receipt|`["{NAME}","{N}","{ITEM}"]`|
|416|pass to another member|`["{NAME}","{N}","{ITEM}","{NAME}"]`|
|417|no inventory room|`["{N}","{W1}"]`|
|422|no effect|`["{D1}"]`|
|3373|unsuccessful Orb operation|`["{LEADER}","{N}","{ITEM}","{W2}","{N}","{W1}"]`|
|3374|seek ship expertise|`["{LEADER}","{N}","{N}","{W1}"]`|
|3375|cockpit placement|`["{LEADER}","{ITEM}","{N}","{W1}"]`|
|3376|cockpit advice|`["{N}","{N}","{W1}"]`|
|3756|groan|`["{W1}"]`|
|3757|disorientation|`["{W1}"]`|
|3758|rough journey|`["{W1}"]`|
|3759|pain|`["{W1}"]`|

Direct reproduction after loading the worktree's current ignored input
selection in the same PowerShell process:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1106/review.py <registered-source-root> --committed
uv run sf2 verify plan --base origin/main --head HEAD
```

The ignored recipe joins selected source/table/helper facts, existing quote
fixture, changed-state cases, actual consumers/lowerings and the exact
two-path/navigation-only diff; it checks UTF-8/LF, links, tokens and private
publication boundaries. Clean committed-head receipts and actual exact-head
scoped CI are distinct from author probes and planner suggestions. Under
this explicit documentation policy, no normal/full, old comparison,
SDK/Godot/emulator/H3/H4 or screenshot gate is run or claimed. The planner
does not turn its suggested commands into executed passes.

Preserve the two initial selected-lowering failures (local stream and
local @Return) as consumer-gap evidence. Preserve #1104's no-drop author
misread and missed-prose P2: original d84ecb32/tree8d3518ff, author/main
direct0.290s/CI39514888171; corrected8ca293ed/treed468e006, focused author
0.483s/main0.484s/CI39516038102, accepted into this base. The correction
separates dead ally's own defeats from FIRST_ALLY's enemy kills. No prior
audit is rerun to replace those completed receipts.

Also retain #1102's completed results, #1100's public-full-text P2 before/
after correction, #1088's49→48 clear-count correction and untouched checked
word, #1096's withdrawn accepted-Tristan attribution, #1098 diagnostics and
the twelve completed old comparison failures in their existing owners/
handoffs. None becomes an automatic rerun or correction queue here.

**Unknown:** natural entry/interactions/chest/exit/launch reach, actual
membership/active/HP/follower/raft/alias/inventory/request/RNG state, text
selection under actual state, interrupted or stale lists/requests/actions,
completed movement/DMA/sprite/resize/input/portrait/text/fade/audio services,
visible presentation and timing, usable control and saved persistence.
The voyage body, shootdown and alternate-setup scenes are explicitly
outside the acceptance boundary. No inference of natural travel, supported
ship content or complete subsystem follows from this static evidence.

Freeze a clean committed/pushed Draft PR with base/head/tree, direct
receipt, clean planner, actual scoped CI, failures/Unknowns and ignored
handoff. Move Project to Review / Verify, send the assigned main-gate
completion notification and stop this writer/processes. Independent review
and merge belong to main-gate; no next Issue, merge, cleanup or archive.
