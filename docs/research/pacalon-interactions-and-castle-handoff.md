# Pacalon interactions and castle handoff

Evidence date: 2026-10-14. **Confirmed:** F530 independently selects Pacalon's
resident-event and area-description alternatives. The callbacks offer dialogue,
two shops and a church; two non-chest items use the common collection path.
Ordered north exits request Map36, and the south exit requests Map71. This
document follows no-scroll consumption back to MainLoop, before destination
admission or initialization. **Unknown:** natural reach, supplied state,
completed services, usable control, presentation and persistence.

## Agreement and provenance

Standard rigor for [Issue #1021](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1021)
under [epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514):
interaction identity, table selection, shared consumers and mutable event state
must agree. Accepted starting base is
`748706569390e458ef69ae1ca1a8a667c57258d2`. This is the sole tracked write;
there is no shared-path or stacked dependency. Independent synthesis and
UI692 supply no evidence.

Consume accepted [conditional Pacalon entry](./battle26-return-through-pacalon-entry.md),
[Ketto service and pickup consumers](./ketto-interactions-and-merchant-relocation.md),
[interaction state](./map-event-interaction-state.md),
[dialogue state](./map-event-dialogue-state.md),
[common scripting](./common-scripting.md), [common maps](./common-maps.md),
[request writers](./map-event-request-state.md),
[request consumption](./map-event-request-consumption.md),
[services](../design/contracts/service-interactions.md),
[dialogue](../design/contracts/dialogue-system.md) and
[area descriptions](../design/contracts/map-area-description-routing.md).
Start only on the conditional Map02 exploration alternative established by
the entry owner; its Battle31 diversion does not establish town control.
Stop at the outgoing MainLoop return. Castle CheckBattle/setup/init, castle
content, Battle27/31, later story, whole generic-service engines, layout
acquisition/decoding and runtime are excluded.

Source paths below are relative to `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The retained read-only checkout is clean at that pin. USA retail ROM identity
is SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
`sf2enums.asm`/`sf2const.asm` own numbers and RAM symbols. Fnnn means flag nnn;
indices are zero-based and address extents half-open. Short `map/`, `entity/`
and `text/` paths are under `code/common/scripting/`; local tables are under
`data/maps/entries/map02/`, including `mapsetups/`.

Minimum evidence is selected pinned source, existing fixtures/consumers and
the retained 18,285,979-byte map-init/script pair. Its accepted 0.083s receipt
is reused, not regenerated. Selected joins are linear, budgeted below one
minute/512 MiB with small ignored receipts. No new maintained checker,
production, schema, fixture, translation or asset work is required. Replan
contradictions or supporting-tool expansion before enlarging the slice.

## Selection, activation and actor identity

**Confirmed:** `data/maps/mapsetups.asm` supplies default `ms_map2` and F530
override `ms_map2_flag530`. Their six-longword pointer tables at
`$5E86C..$5E884` and `$5E884..$5E89C` differ in entity list, entity events and
area descriptions; they share zone events, item events and the empty init.
`map/mapsetupsfunctions_1.asm:GetCurrentMapSetup` reads CURRENT_MAP and checks
the selector at each call. Entity-list construction, later interaction and
later examination do not share one frozen F530 snapshot. No local callback
below changes530, but a changed supplied flag between these reads matters.

The entry owner gives both complete NPC lists and follower construction.
For n successfully declared ordinary followers, fixed row j normally maps
logical `128+j` through lookup offset `32+j` to physical `n+1+j`; main is0.
The tables below join every row to its declared sprite/origin. M means
CENTAUR_MALE=233, F means CENTAUR_FEMALE=234, merchant=193 and priest=192.
`walk1` means the declared origin/range1 walking script; other rows use
eas_Init. Positions can change before activation. Followers, duplicate
aliases, capacity, actual HP/class, retained mappings and player mode remain
the entry owner's premises, not a stable logical-to-physical cast guarantee.

`explorationvints.asm:ProcessPlayerAction` samples PLAYER_1_INPUT, idles
physical0 and waits for movement/view scroll. A takes the field-menu branch
before C. Ordinary C passes debug and overlapping physical1-Caravan
alternatives before `GetActivatedEntity`. With view-target0, that helper
uses position/facing, steps through layout kind$2800 desks/counters under
mask$3C00, and scans physical actors in order for distance below384 from
the target. It skips the viewed actor and follower-list actors; reading
layer is not a layer-equality test. Negative view target returns; nonzero
camera states are not proven ordinary activation.

`battlefunctions_0.asm:GetEntityEventIndex` searches ENTITY_INDEX_LIST for
the physical ID from its beginning, changing the logical counter to128 after32 ally entries;
the first alias wins and no match returns0. `RunMapSetupEntityEvent` stores
player-facing d2 into EVENT_RELATIVE_POSITION, gets the current setup, and
scans four-byte rows for the first logical match or $FD default. Relative
word offsets resolve from the selected table base; the set530 table's
backward references really reuse the default table's service/default bodies.

The wrapper obtains portrait and speech SFX from the actual mapped sprite,
prefills CURRENT_PORTRAIT/CURRENT_SPEECH_SFX, conditionally opens a portrait
and resolves the actor again. Every named local row uses UP1: bit0 waits a
VInt then requests facing `(playerFacing+2)&3`; bit1 is clear, so it does not
restore the saved actor facing after the callback. UP is an action mask,
not a required approach direction. Default0 turns/restores neither. The
wrapper clears the VInt_UpdateEntities flag before the callback, then closes
portrait/text and activates that VInt function on return. An RTS default
still has this surrounding work; failed reverse lookup is not a useful
interaction with Bowie or a safe repair of a corrupt mapping.

## Both complete resident-event tables

**Confirmed:** `s2_entityevents.asm`, `$5E938..$5E9C8`, contains ten named
rows plus default; its callbacks are `Map2_EntityEvent0..9`. Each listed
text sequence ends in RTS; service calls likewise have a local RTS.

| j / logical | Declared actor and origin | Action / callback suffix | Complete local work |
| --- | --- | --- | --- |
|0 /128|M (21,22), Down, walk1|UP1 /0|3583 →3584|
|1 /129|M (34,26), Down|UP1 /1|3585|
|2 /130|F (8,21), Right|UP1 /2|3586|
|3 /131|M (23,4), Up|UP1 /3|3587 →3588|
|4 /132|F (16,23), Down|UP1 /4|3589|
|5 /133|F (34,7), Down, walk1|UP1 /5|3590 →3591 →3592|
|6 /134|M (31,15), Down, walk1|UP1 /6|3593 →3594|
|7 /135|merchant (9,8), Down|UP1 /7|write shop10; j_ShopMenu|
|8 /136|merchant (15,5), Down|UP1 /8|write shop25; j_ShopMenu|
|9 /137|priest (41,8), Down|UP1 /9|j_ChurchMenu|
|10 /default|actual supplied mapping|0 /Map2_DefaultEntityEvent|RTS only|

**Confirmed:** `s2_entityevents_530.asm`, `$5E9C8..$5EA20`, has nine named
rows plus default. Its six text bodies are `Map2_212_EntityEvent0..5`:

| j / logical | Declared actor and origin | Action / target | Complete local work |
| --- | --- | --- | --- |
|0 /128|M (22,25), Down, walk1|UP1 /Map2_212_EntityEvent0|3595 →3596|
|1 /129|M (41,26), Up|UP1 /Map2_212_EntityEvent1|3597|
|2 /130|M (30,17), Right|UP1 /Map2_212_EntityEvent2|3598|
|3 /131|F (8,21), Right|UP1 /Map2_212_EntityEvent3|3599 →3600|
|4 /132|F (34,7), Down, walk1|UP1 /Map2_212_EntityEvent4|3601 →3602|
|5 /133|M (21,7), Down|UP1 /Map2_212_EntityEvent5|3603|
|6 /134|merchant (9,8), Down|UP1 /Map2_EntityEvent7|write shop10; j_ShopMenu|
|7 /135|merchant (15,5), Down|UP1 /Map2_EntityEvent8|write shop25; j_ShopMenu|
|8 /136|priest (41,8), Down|UP1 /Map2_EntityEvent9|j_ChurchMenu|
|9 /default|actual supplied mapping|0 /Map2_DefaultEntityEvent|RTS only|

Changing530 after a clear530 population build does not relabel actors:
logical135 can invoke the item shop from the former weapon merchant,
136 church from the former item merchant, and137 default from the priest.
Conversely, a set530 population followed by clear530 event selection gives
134 the old Event6 text,135 weapon shop and136 item shop. This follows table
selection and current mappings; no such natural transition is observed.

None of the17 unique local callback bodies sets/clears a story or membership
flag, recruits, installs a cutscene, asks YesNoPrompt or posts a map request.
Service bodies have their accepted conditional effects; an invocation must
not be described as side-effect-free completion. Text3593–3594 asks for an
answer in prose but has no choice-result branch. Conversations do not gate
the listed services, pickups or warp rows.

## Complete selected dialogue and text consumers

**Confirmed:** every literal below was compared with actual
`data/scripting/text/gamescript.txt` keys0DFF..0E13, not only ASM comments.
These21 strings contain no name/item substitutions. `{N}` is newline;
W1 and W2 retain their distinct consumer work.

| ID | Complete literal |
| --- | --- |
|3583|`You're not devils, are you?{W2}`|
|3584|`How did you get here safely?{W1}`|
|3585|`I'm hungry....{W1}`|
|3586|`Are you strong?  Will you{N}please kill the devils?{W1}`|
|3587|`Pacalon soldiers are so{N}proud!{W2}`|
|3588|`But, they're just cowards{N}that hide in the castle.{W1}`|
|3589|`What will we do when we{N}lose?{W1}`|
|3590|`Do you know Geshp?{W2}`|
|3591|`He sometimes comes here.{N}He seems so friendly.{W2}`|
|3592|`Are all devils like him?{N}If so, I like them.{W1}`|
|3593|`Do you think Pacalon can{N}beat the devils?{W2}`|
|3594|`Say yes!  Pacalon has the{N}best fighters in the world!{W1}`|
|3595|`This is Pacalon, a kingdom{N}of fine centaurs.{W2}`|
|3596|`We welcome you.{W1}`|
|3597|`Rain!  This must be a{N}present from the gods!{W1}`|
|3598|`Thanks to the rain, I'm no{N}longer thirsty.{N}But, I'm still hungry.{W1}`|
|3599|`Did you save Mitula?{W2}`|
|3600|`Then, why don't you defeat{N}those devils?{W1}`|
|3601|`Was Tristan occupied by{N}devils?{W2}`|
|3602|`That's why we've had no{N}rain lately.{W1}`|
|3603|`I see Geshp frequently.{N}He always stares at the{N}castle.{W1}`|

**Inferred:** the alternatives present drought/anxiety and restored-rain
conversation themes. Statements about gods, Geshp's friendliness, soldiers,
Tristan or Mitula are spoken beliefs/questions, not independent evidence of
world state, a required route or an additional flag condition. Even the
restored alternative retains hunger and fear of devils.

`sf2macros.asm:txt` emits the TEXTBOX trap and text word; the
`code/common/tech/interrupts/trap5_textbox.asm` handler calls DisplayText.
`text/textfunctions_1.asm:DisplayText`
creates/reuses the dialogue window, resets its substitution pointer to
DIALOGUE_NAME_INDEX_1 per call, decodes the selected text-bank/Huffman data
and returns at termination. Consecutive txt calls do not imply a close or
sleep between them; this is not the cutscene command's cursor increment.
The accepted dialogue owner preserves W1's RNG draw/copy →VInt wait →masked
input test and restored typewriting state. W2 has its own cursor helper,
draw/copy/wait/input loop, validation sound and cleanup. Neither token is
YesNoPrompt. Text termination, CLEAR and wrapper clsTxt are different
operations, with no new Pacalon timing/rendering or acceptance observation.

## Shop catalogs and priest boundary

**Confirmed:** Map2_EntityEvent7 writes byte SHOP_WEAPON_PACALON=10;
Event8 writes SHOP_ITEM_PACALON=25. Both call j_ShopMenu. In
`code/common/menus/shop/shopactions.asm:GetShopInventoryAddress`, the byte
index skips that many length-prefixed rows in list_ShopInventories;
PopulateShopInventoryList copies the selected count/items. Actual rows10/25
of `data/stats/items/shopinventories.asm` are:

| Index / source comment | Ordered stock symbols (ITEM_ prefix) |
| --- | --- |
|10 /Weapon shop 11|BUSTER_SWORD, GREAT_SWORD, HEAVY_LANCE, JAVELIN, LARGE_AXE, ASSAULT_SHELL, GREAT_SHOT, GUARDIAN_STAFF, INDRA_STAFF, POWER_GLOVE|
|25 /Item shop 11|MEDICAL_HERB, HEALING_SEED, HEALING_DROP, ANTIDOTE, FAIRY_POWDER, ANGEL_WING|

These are ten and six stock entries, not prices, granted items or a separate
local shop-type byte. Both F530 tables call these same bodies. ShopMenu
conditionally opens CURRENT_PORTRAIT, requests158, closes text/portrait and
enters the shared buy/sell/repair/deals selector. The accepted service/menu
owner supplies capacity, affordability, item/equipment, cancellation and
looping rules: buy/deals pay before AddItem (then deals removal); sell grants
gold then drops the item and may add rare deals; repair pays then repairs.
Invocation and catalog availability do not establish any completed purchase,
successful mutation, equipped item or retained gold/inventory outcome.

Event9 directly calls j_ChurchMenu with no local first-visit flag or warning.
`code/common/menus/church/churchactions_1.asm:ChurchMenu` conditionally opens
the portrait, requests110, closes text/portrait and reaches raise/cure/promote/
save selection. It does not automatically heal, save or change EGRESS_MAP.
The existing bounded Raise/Cure/Save observations remain those owners' seams,
not Pacalon runtime evidence. Shared entry literals were also checked:

| ID | Complete literal |
| --- | --- |
|158|`What's up, boy!{N}We guarantee all items to{N}be in good condition!{D3}`|
|110|`Welcome!{W2}{N}Your desire will be fulfilled!{W2}`|

D3 is a text delay command, not W1, a purchase or consent. Later prompts and
complete service algorithms remain with the existing service contract.
For the Save alternative specifically, zero first prompt reaches @DoSaveGame:
copy CURRENT_MAP (nominally2) to EGRESS_MAP, set399, call SaveGame with CURRENT_SAVE_SLOT,
then music/input and continue/suspend prompt. Nonzero first prompt avoids
that save path; post-save zero exits, nonzero reaches fade/WitchSuspend
instead of ordinary callback completion. The selected existing savepoint
row is Map02 `(41,10)`, Up; it is return data, not a town-entry teleport.
Cross-process persistence and actual save completion remain Unknown here.

## Both description tables and examination priority

**Confirmed:** `s4_descriptions.asm` and `s4_descriptions_530.asm` each contain
a16-byte wrapper, seven six-byte msDesc rows and a two-byte $FD00 terminator:
`$5EA26..$5EA62` and `$5EA62..$5EA9E`. Bases are both$1030=4144;
table addresses are$5EA36 and$5EA72. All14 rows have condition0/payload-kind0.
Only row5's description offset changes; the full ordered join is:

| Row | X,Y / subject | Investigation offset →ID | Description offset/ID, clear530 | Description offset/ID, set530 |
| --- | --- | --- | --- | --- |
|0|8,13 /sign|2 →425|0 /4144|0 /4144|
|1|15,10 /sign|2 →425|1 /4145|1 /4145|
|2|17,14 /statue|9 →432|2 /4146|2 /4146|
|3|17,18 /statue|9 →432|3 /4147|3 /4147|
|4|25,18 /statue|9 →432|4 /4148|4 /4148|
|5|6,5 /well|5 →428|5 /4149|6 /4150|
|6|25,14 /statue|9 →432|7 /4151|7 /4151|

The actual gamescript keys01A9/01AC/01B0 and1030..1037 give:

| ID | Complete literal |
| --- | --- |
|425|`{NAME} investigated{N}the sign.{W2}{CLEAR}`|
|428|`{NAME} looked into{N}the well.{W2}{CLEAR}`|
|432|`{NAME} investigated{N}the statue.{W2}{CLEAR}`|
|4144|`Various weapons!{N}-inquire within{W1}`|
|4145|`Various tools!{N}-inquire within{W1}`|
|4146|`A statue of a fine soldier.{W1}`|
|4147|`A statue of a fine soldier.{W1}`|
|4148|`A statue of a fine soldier.{W1}`|
|4149|`A sturdy well.{N}No water there.{W1}`|
|4150|`A very deep well.{W1}`|
|4151|`A statue of a fine soldier.{W1}`|

`explorationfunctions_0.asm:CheckArea` derives the tile ahead for ordinary
view-target0, clears DIALOGUE_NAME_INDEX_1 to0 and speech SFX to0. Thus NAME
here selects member0's name, not the activated NPC. Layout kind$1800 chest,
$2C00 vase, $3000 barrel, $3400 bookshelf and $1C00 area-item handling take
priority over the area-description wrapper. A coordinate alone proves no
object kind, walkability or inspection route.

Normal no-entity C passes d6=1. DisplayAreaDescription packs the coordinates,
scans six-byte rows in order, rejects a nonzero condition when d6 is nonzero,
and takes the first admitted match. These local condition0 rows are eligible
without d6=0. It displays423+investigation offset, then4144+description offset,
closes portrait/text and returns d7=-1. No match returns d7=0; CheckArea with
d6=1 returns0, allowing ProcessPlayerAction's FieldMenu fallback. Supplied
d6=0 instead selects generic423/412 on no match. Missing actor, no matched
description and default entity callback therefore have different results.
The well's text change neither grants water nor writes530. The descriptions
contain no function payload, choice, story mutation or castle prerequisite.

## Empty chests, two non-chest items and default handlers

**Confirmed:** `7-chest-items.asm`, `$96A7E..$96A80`, is only endWord.
`8-other-items.asm`, `$96A80..$96A8A`, has exactly two four-byte rows and
endWord. `sf2mapmacros.asm:mapItem` encodes X,Y,flag,item as bytes:

| Row | Candidate coordinate | Collection flag | Item symbol / index |
| --- | --- | --- | --- |
|0|33,6|172|ITEM_CHEERFUL_BREAD /13 ($0D)|
|1|27,24|173|ITEM_MEDICAL_HERB /0|

These flags are not temporary256–383 and are independent of530. No new
layout decode identifies a vase/barrel/bookshelf at either coordinate.
CheckArea reaches CheckNonChestItem only on a qualifying layout branch.
`exploration.asm:GetNonChestItem` selects the current map's other-item pointer
at MAPDATA_OFFSET_ITEM_OTHER=38. GetItem compares low coordinate bytes in
four-byte row order, applying its battle-crop branch only when CURRENT_BATTLE
is not255. That branch actually scales by the pinned erroneous stride5,
not the seven-byte battle-coordinate record size; changed battle state must
not be described as a corrected crop lookup. It returns the original-coordinate
layout offset, flag and item; negative first-byte terminator/no match yields-1.
This is not a table keyed
by actor or F530, and empty chest data does not disable generic examination.

CheckNonChestItem tests the matched flag. Held flag substitutes ITEM_NOTHING;
both matched paths set the flag before returning. Clear flag therefore commits
before capacity/AddItem succeeds. Ordinary items13 and0 take
`explorationfunctions_1.asm:loc_239EE`, not the gold-chest alternative:
set item substitution index2, request413, try member0's four-slot inventory,
then, if full, UpdateForce and the ordered OTHER_FORCE_MEMBERS_LIST.
The scan uses joined force members, not just the active party, and has no HP
test. This list starts one byte after TARGETS_LIST and assumes joined Bowie
is its first member. Changed unjoined-Bowie state still tries0 separately and
omits the actual first joined member in the fallback; no repair is implied.

The first member with room receives AddItem; success retains172/173, requests
415 for Bowie or416 with recipient name index3, item music and
FadeOut_WaitForP1Input. Text415/416 has no W1/W2 token; the caller supplies
that separate music/plain-input boundary. No item-use/stat/heal effect follows
merely from acquisition. All-full requests417, then CloseChest and
RefillNonChestItem at saved candidate coordinates. Refill re-finds the row
and clears its flag, allowing retry. Held flag/no item selects the layout's
empty-object text, not a duplicate grant. Interruption before AddItem/refill
can leave a collection flag without an item; interruption after AddItem can
leave inventory changed without completed presentation. This is not atomic.
The caller does not test AddItem's returned d2: that helper writes the first
empty slot or returns1 if none remains. A changed inventory between capacity
read and insertion is not repaired or proven successful by the text path.

The shared pickup texts were checked against gamescript as well:413 is
`{NAME} found{N}{ITEM}.{W2}{CLEAR}`,415 is
`{NAME} recieved{N}{ITEM}.` (source spelling),416 is
`{NAME} passed the{N}{ITEM} to {NAME}.`, and417 is
`But, everyone's hands are{N}full.{W1}`. Item0 is a real Medical Herb, not
the no-item sentinel127. The no-match-1 value becomes127 under the item mask.

Both setups share `s3_zoneevents.asm`'s sole default Map2_DefaultZoneEvent/RTS
and `s5_itemevents.asm`'s sole default Map2_DefaultItemEvent0/RTS. The zone
wrapper still closes windows and waits a VInt/physical0 destination. The
item-use wrapper clears speech, masks the used item, stores supplied player-Y
in EVENT_RELATIVE_POSITION, initializes d6=0 and performs cleanup/wait. Its
default d6 return leaves generic FieldMenu handling with its existing owner.
That item-use table is distinct from item collection and area descriptions;
neither default grants items, sets story flags or produces the castle warp.

## Ordered exits and outgoing MainLoop request

**Confirmed:** `6-warp-events.asm`, `$96A5C..$96A7E`, contains four eight-byte
records followed by endWord. `warpNoScroll` emits0; `warpFacing` emits the
facing byte then a zero padding byte. Exact source order matters:

| Row | Source X,Y | Type / map | Destination X,Y | Facing |
| --- | --- | --- | --- | --- |
|0|19,0|0 /MAP_PACALON_CASTLE=36|20,30|Up1|
|1|23,0|0 /MAP_PACALON_CASTLE=36|22,30|Up1|
|2|255,0|0 /MAP_PACALON_CASTLE=36|255,30|Up1|
|3|255,31|0 /MAP_OVERWORLD_PACALON_KINGDOM_DROUGHT=71|22,16|Down3|

`entity/entityscriptengine_2.asm:esc02_controlCharacter`'s ordinary warp
path requires masked layout kind$1000 before calling WarpIfSetAtPoint.
esc07_controlRaft and esc08_controlCaravan have separate $1000 call sites;
their movement/collision admission is not established by these records.
`exploration.asm:WarpIfSetAtPoint` sign-extends internal coordinate
words and divides by384, then scans the current map's records. Word$FFFF
ends the scan; a negative source X or Y byte is wildcard, otherwise low bytes
must compare equal. The first match writes the request and returns.

Thus qualifying `(19,0)` selects row0, `(23,0)` row1; the later north-edge
wildcard cannot override their destination X20/22. Another qualifying X at
Y0 selects row2, which requests preservation of destination X. Y31 instead
selects the south return. No row tests530, dialogue, shopping, inventory,
membership or castle-story state. A wildcard is not proof that every edge
cell has the required layout or is naturally reachable.

The producer writes word MAP_EVENT_WARP=1 to MAP_EVENT_TYPE at$FFA84A,
then a longword containing type/map/destination-X/destination-Y into
PARAM1..4 at$FFA84C..$FFA850, then a word containing facing/padding into
PARAM5 and the following byte at$FFA850..$FFA852. It writes word SFX_WARP
to WARP_SFX. Type is written before parameters, with no queued immutable
snapshot, prior-event guard or transaction. No-match leaves pending state
unchanged. Replacement or interrupted production can change which payload
is later consumed; row selection alone does not establish an atomic request.

WaitForEvent reads the event word before its A/C poll. Pending zero installs
physical0's control: numeric mode0 character,1 raft, other nonzero Caravan,
as established by the entry owner. Pending nonzero bypasses installation.
ProcessMapEvent clears the stored event type before dispatching its already
read d0 value. For type1/PARAM1 zero, ProcessMapEventType1_Warp:

1. Submits WARP_SFX and clears that word, then makes physical0 idle.
2. Removes the exploration return address to return directly to MainLoop;
   clears low words d0..d4 and reads PARAM2's byte into d0.
3. UpdatePlayerPosFromMapEvent samples physical0's current X/Y words,
   sign-extends/divides by384, and samples its facing. These are consumption
   coordinates, not a saved copy of the trigger point or an NPC's coordinates.
4. Reads destination X/Y bytes independently. Negative bytes preserve the
   sampled coordinate word; nonnegative bytes replace only its low byte.
   Ordinary small positive sampled coordinates give the nominal table
   destinations. Changed high bytes survive low-byte replacement: this is
   not zero-extension or whole-word normalization. Sentinel255 preserves
   actual sampled X on row2, not necessarily the earlier producer's X.
5. Replaces facing with PARAM5's byte and reads PARAM1's byte into d4, then
   RTS to MainLoop. With unchanged payload d0=36 or71, d3=1 or3, d4=0.

The facing seed is an outgoing operand, not a turn of the town actor in this
consumer. Re-reading mutable parameters after sound/idle/sampling does not
guarantee they still match the row initially selected. Interrupted service,
prior event replacement or changed mode/register state remains explicit.
No-scroll does not itself write destination CURRENT_MAP or construct actors.

MainLoop's exploration return branches to @Start, where SwitchMap precedes
CheckBattle and subsequent setup/init. This document stops at that outgoing
handoff. The already accepted switch rule would substitute71→78 if530 is
set when later read; the posted south request is still71. No new castle
admission/initialization, south-route completion or usable destination
control is claimed. Optional conversations, service completion and item
collection are not established prerequisites for producing these rows.

## Changed-state review and reproduction

| Changed state / interrupted boundary | Required distinction |
| --- | --- |
|530 differs or changes between list/event/description reads|Current table selection can mismatch retained actors; no rebuild or automatic relabel.|
|Followers, duplicate aliases, HP/class, capacity or lookup differs|Actual physical activation and first reverse alias govern; declaration formulas are conditional.|
|Camera, approach, desk/layout, actor movement or mode changes|UP1 turns without restore; no approach gate or guaranteed activation/reach.|
|No actor versus unmatched entity versus unmatched description|CheckArea/FieldMenu fallback versus default callback/cleanup.|
|Shop gold/capacity/equipment/deals or cancellation differs|Selected catalog and service entry do not guarantee mutation/completion.|
|Church decline, save, post-save suspend or interrupted service|Savepoint and suspend effects belong to reached service branches, not priest contact.|
|Description coordinate/layout/d6 differs|Priority and first admitted row matter; no decoded-object or route claim.|
|172/173 held, Bowie/full-force capacity or joined list differs|No duplicate; ordered fallback; flag reset only on reached full-force recovery.|
|Item/presentation interrupted|Flag-before-item and item-before-presentation boundaries remain.|
|Exact19/23 versus wildcard north, south or no matching row|First match determines request; no-match preserves prior state.|
|Source/destination negative bytes, high bytes or moved player|Wildcard, sampled-coordinate preservation and low-byte replacement are distinct.|
|Pending event replaced or interrupted;530 changes before next loop|Mutable payload and later switch read prevent an unconditional destination claim.|

Load ignored `local/private-inputs.ps1` in each launching process. Check the
retained source's clean pinned HEAD and use the
[retained-pair recipe](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary)
without regeneration or changing pair size/mtime. Bounded review:

1. Join both setup pointers, all21 event records including defaults, both
   complete NPC lists and all17 unique callback bodies to actual source and
   `tests/fixtures/h2/map-events/entity-programs.json`. Check activation,
   reverse lookup, relative offsets, facing/portrait/speech and cleanup.
2. Compare every selected English literal with gamescript, and the actual
   DisplayText/W1/W2/control consumers with accepted dialogue owners.
   Resolve shops10/25 from enums, length-prefixed rows and actual consumer;
   use `common-menus-static-v1.json` serviceStateMachines and selected
   shop-index accesses in `map-event-request-consumption-static-v1.json`.
3. Join all14 description rows/base arithmetic to actual wrappers/macros and
   `map-descriptions-static-v1.json` consumer facts. Read both item rows,
   empty chest and default zone/item targets; follow actual pickup/failure/
   repeat consumers. Aggregate fixtures alone do not prove local literals,
   stock rows, physical layout or completed services.
4. Compare all four warp rows/macros to actual producer and no-scroll
   consumer widths, first-match, sign/low-byte behavior, pending reads and
   MainLoop return. Review changed states above against those branches.
5. Review entire prose/diff, UTF-8/LF, relative links/anchors, sole-path and
   private boundary; run `git diff --check`. On the clean committed head run
   `uv run sf2 verify plan --base origin/main --head HEAD` and record actual
   exact-head scoped CI in the Issue handoff. Documentation-only scope skips
   blanket local normal/full/Python/.NET/Godot/H3/H4 suites.

**Unknown:** natural interaction/accessibility, actual flags/mapping/HP/
followers/mode, pending/inventory state, completed text/menu/pickup/transition,
presentation/timing, usable control and save/load persistence. Optional
compressed-layout data remained unavailable in the predecessor; this is
not a ROM blocker or authorization for acquisition. No decoded route is
inferred from table coordinates, and these Unknowns are not a runtime queue.

Preserve [#1017 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1017#issuecomment-6182642561)
and [PR1020 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1020#issuecomment-6182664667):
all original/rebased receipts, pair0.083s, author0.071s/main-gate0.062s,
completed CI, corrected inspection failures and linked predecessor Unknowns
remain retained. No completed suite is reset or relabeled. Source/configuration/
pair/receipts remain local. Freeze a clean current-main-based pushed Draft PR
and Project Review handoff for independent main-gate acceptance; no merge,
cleanup, archive, runtime or next Issue.
