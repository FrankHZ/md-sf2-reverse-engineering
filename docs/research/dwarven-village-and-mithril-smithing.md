# Dwarven Village and Mithril smithing

Evidence date: 2026-10-22. **Confirmed (static):** the accepted Dry Stone
branch leads to Map77's declared entrance to Village29. Seven residents,
search records, eighteen warps and the actual smith caller connect to a
four-word order service. Payment selects the weapon immediately; readiness
uses F80 from explicit exploration entry, without an elapsed-time field.
**Inferred:** the texts present a custom weapon commission and later collection.
**Unknown:** naturally walked access, actual player choices/caller state,
complete presentation, successful service visits and durable save/reload.

## Agreement and provenance

Standard rigor under [Issue1206 / Epic1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1206):
one connected static account of admission, all local interactions/search/stairs,
placement, readiness, collection and exit/revisit. Acceptance is the complete
source-to-consumer-to-prose review and direct documentation/scope checks; it
adds no fixture, lowering run, runtime observation, English design or playable
capability. The owned paths are this document, bounded village/smith coverage
in [story coverage](./story-content-coverage.md), and a short frontier in
[source coverage](./source-coverage.md); all source-ledger pipe tables/counters
remain unchanged. Other optional recruitment, exhaustive late revisits, full
church/shop/economy behavior and engine implementation are outside this slice.

Consume accepted [Dry Stone/Map77](./roft-to-galam-and-battle36-admission.md),
[earlier mine/recovery/Cannon](./creed-departure-dwarf-recovery-and-cannon-gift.md),
[menus and bounded service observations](./common-menus.md#static-service-menu-state-machines),
[item auxiliary data](./core-stats-data-inventory.md#item-auxiliary-catalog),
[field search](./field-search-control.md), [field item use](./field-item-effects.md)
and [request consumption](./map-event-request-consumption.md).
They retain their own evidence and exclusions; helper observations do not
establish a natural village journey.

All original-source paths below are relative to read-only
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6/disasm](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
USA ROM identity remains SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
`sf2enums.asm`/`sf2const.asm` own constants/RAM;
`sf2mapmacros.asm`, `sf2mapsetupmacros.asm` and `sf2macros.asm` own encoding.
Fnnn denotes flag nnn; row ordinals are zero-based; intervals are half-open.
Local setup files mean `data/maps/entries/map29/mapsetups/`.

The finite source surface includes the complete 19,798-byte smith actions,
2,989-byte picker, local tables and their named shared consumers. Reuse the
18,285,979-byte map-init/map-script pair and existing provenance from
`local/issue1005/pair.txt`; do not regenerate it. Linear direct checks target
under 60 seconds/512MiB, with new ignored receipts under 4MiB in `local/issue1206`;
peak memory is unmeasured. No seed enumeration, acquisition, simulator, new
checker platform or paid API batch is needed. Reassess material scope growth.

## Admission, separate mine and return

**Confirmed:** the [Dry Stone owner](./roft-to-galam-and-battle36-admission.md#dry-stone-caller-scene-and-changed-layout-lifetime)
retains possession/use selection at77 `(43,42,Left)`, first-use F900 before
scene, handled d6=-1 and no material removal on first or repeated use. The
scene copies one working-layout block; held900 reapplies it on a base-layout
load. Matching a warp, changed terrain, movement admission and a naturally
walked route are separate claims. There is no additional village story gate
or Stone consumption test in the following records:

| Table / row / address | Trigger | Complete destination payload |
| --- | --- | --- |
|77 /0 /`$C5D78`|40,42|no-scroll0;29 `(15,20)` Up1|
|29 /0 /`$AA930`|any,0|no-scroll0;77 `(41,42)` Down3|
|29 /1 /`$AA938`|any,21|no-scroll0;77 `(41,42)` Down3|

`exploration.asm:WarpIfSetAtPoint` requires its movement/layout-marker caller,
divides signed pixel coordinates by 384 and selects the first matching eight-byte
record; negative source coordinate bytes are wildcards. It publishes event1,
parameters and facing/padding. `ProcessMapEvent` clears the request before
`explorationfunctions_2.asm:ProcessMapEventType1_Warp` consumes it. No-scroll
idles physical0, submits/clears WARP_SFX and unwinds to MainLoop. Sampled tile
coordinates receive only requested low-byte replacements, so ordinary high
bytes are a premise. MainLoop calls SwitchMap then CheckBattle before exploration.
Neither29 nor77 is flag-switched; CheckBattle has no29 record.

The return to77 can select Battle37 through F437/F537. The accepted
[77 conditional admission owner](./roft-to-galam-and-battle36-admission.md#map53-default-setup-and-battle36-scan)
retains that scan,77's default zone and Battle36's separate53 route; stop there.
Returning does not itself prove that a77 zone ran, that37 is forced, or that36
was completed. Fresh29 admission is distinct from already running its init or
returning usable control.

Earlier `MAP_DWARVEN_VILLAGE_INTERIOR=30` is the mine reached from68 `(57,24)`;
its sole return is30 `(any,21)`→68 `(57,25)` Down. The
[accepted chapter](./creed-departure-dwarf-recovery-and-cannon-gift.md)
owns all three30 population selectors, Fairy recovery, Cannon helper and
immediate return. `MAP_DWARVEN_VILLAGE_EXTERIOR=29` is a source name for this
later77-connected village. Neither complete29 nor30 warp table connects them.
Other30 revisits, physical travel and later service completion remain bounded
by that earlier owner's exclusions.

## Complete Village29 setup and interactions

**Confirmed:** `data/maps/mapsetups.asm` has only unflagged29→ms_map29.
`pointertable.asm` `$5A09E..$5A0B6` owns six pointers. Its fixed/walking records
occupy `$5A0B6..$5A0F0`; common fresh-entry player/followers are additional.
Logical residents128..134 use lookup offsets32..38; physical slots depend on
allocated followers. A resident label is neither an ally ID nor a stable
physical index. Map29 is outside the overworld Caravan-admission list.

| Logical ID | Sprite / initial tile / facing / action | Handler / wrapper bits / complete body |
| --- | --- | --- |
|128|Dwarf226 /39,5 /Right0 /eas_Init|EntityEvent0;Down3; tail `jmp j_BlacksmithMenu`|
|129|Priest192 /55,9 /Down3 /eas_Init|EntityEvent1;Up1; tail `jmp j_ChurchMenu`|
|130|Dwarf226 /59,10 /Left2 /eas_Init|EntityEvent2;Up1; CURRENT_SHOP_INDEX.b=SHOP_DWARVEN_VILLAGE28; tail `jmp j_ShopMenu`|
|131|Dwarf226 /46,5 /Right0 /eas_Init|EntityEvent3;Up1; txt2125;RTS|
|132|Dwarf226 /59,16 /Down3 /walk origin59,16 radius1|EntityEvent4;Up1; txt2126→2127;RTS|
|133|Dwarf226 /37,12 /Down3 /walk origin37,12 radius1|EntityEvent5;Up1; txt2128→2129;RTS|
|134|Maid208 /37,7 /Up1 /eas_Init|EntityEvent6;Up1; txt2130→2131;RTS|

`s2_entityevents.asm` `$5A0F0..$5A14E` includes all seven rows and default
253/0→RTS. There are no flag, membership, item or battle predicates inside
these seven handlers. DOWN3/UP1 in this event table are **wrapper action bits**,
not required player facings. `RunMapSetupEntityEvent` bit0 turns the actor toward
the player, bit1 restores its saved facing after the callback. Thus128 turns
and restores;129..134 turn without that restore. Portrait/speech selection,
entity-update deactivation, callback, window close and update reactivation
surround the tail service return.

`explorationvints.asm:ProcessPlayerAction` waits for movement/view stop, prioritizes
A into FieldMenu, and routes ordinary C through GetActivatedEntity and
GetEntityEventIndex. GetActivatedEntity advances the faced target through counter blocks$2800,
then, with ordinary view target0, scans physical slots0..47 for Manhattan
distance below384, excluding the view target and listed followers. Its pointer
already includes the view-target offset; supplied nonzero targets are not
normalized into the same physical scan. GetEntityEventIndex resolves the first
inverse-lookup match (logical32 switches to128); missing matches fall back0.
Actual aliases and layout therefore affect interaction selection. A callback
table does not prove successful talking.
All local speech is paraphrased; ordered controls are source tokens, without
asserted frame durations:

| txt ID | Authored meaning | Ordered controls |
| --- | --- | --- |
|2125|Resident recognizes the party|N,W1|
|2126|Danger outside prevents travel to Galam|N,W2|
|2127|Residents nevertheless expect to leave soon|N,W1|
|2128|Praise for the local smith|N,W2|
|2129|Custom orders are available|N,W1|
|2130|Mithril is scarce|N,W2|
|2131|Take held Mithril to the smith|N,W1|

The church tail resolves `j_ChurchMenu` `$20004`→ChurchMenu `$20A02`.
The shop tail resolves `j_ShopMenu` `$20000`→ShopMenu `$20064`;
GetShopInventoryAddress reads the actual CURRENT_SHOP_INDEX byte and skips
count-prefixed inventories. This slice stops at these actual service boundaries;
[common menus](./common-menus.md) retains church/shop transactions, catalog and
save evidence. No full economy, promotion or service UI census is implied.

`s3_zoneevents.asm` `$5A14E..$5A154` contains only default→RTS;
`s5_itemevents.asm` `$5A16C..$5A174` only default→RTS;
`s6_initfunction.asm` `$5A174..$5A176` is RTS. None adds a smith readiness,
Mithril use, recovery, battle or story flag producer.

## Search and working-map state

**Confirmed:** `2-areas.asm` `$AA8EC..$AA92A` declares rectangles
`(0,0)..(31,21)` and `(32,0)..(63,21)`, with secondary foreground/background
origins0,0; both planes' parallax256,256; autoscroll0,0; layer type255;
MUSIC_MITHRIL_DIGGERS37. The resource header selects palette3 and tilesets
0,89,48,255,65. `3-flag-events.asm`, `4-step-events.asm` and `5-roof-events.asm`
are each endWord only: no local flag-copy, door-step or roof record. This does
not establish passable terrain or a physical path through every storage region.
`9-animations.asm` declares header operands77,64 and two entries
`(0,32,$300,20)` then `(32,32,$300,20)` before endWord; actual animation
timing/display remains outside the table claim.

`s4_descriptions.asm` `$5A154..$5A16C` sets base d3=$104F=4175 and calls
DisplayAreaDescription for the sole `msDesc 54,9,4,0`, then sentinel.
The macro encodes a zero special-condition/function word, investigation offset4
and description offset0. At the matching investigated tile `(54,9)`, the common
consumer emits427 (bookshelf investigation;NAME,N,W2,CLEAR), then4175 (book
about cave life;N,W1), closes windows and reports d7=-1. That coordinate is the
searched tile, not the standing player coordinate or a facing requirement.

Complete item tables:

| Owner / address | Search tile | Flag / item |
| --- | --- | --- |
|7-chest-items row0 /`$AA9C2`|7,2|193 /WHITE_RING|
|7-chest-items row1 /`$AA9C6`|18,18|194 /HEALING_WATER|
|8-other-items row0 /`$AA9CC`|44,19|195 /DEMON_ROD|

The chest terminator ends at$AA9CC; the other-item terminator at$AA9D2.
`CheckArea` dispatches the faced working-layout kind: chest$1800, vase$2C00,
barrel$3000, bookshelf$3400, searchable$1C00, then area description.
Coordinates in a content table alone do not determine this dispatch.
`OpenChest` checks/sets the associated flag and writes the opened block;
`CheckNonChestItem` checks/sets its flag without that chest-block write.
Held flags return ITEM_NOTHING127. The accepted
[search handoff](./field-search-control.md#confirmed-static-spine) tries leader
capacity4 then the joined-force alternatives. Total capacity failure closes
and refills through CloseChest/RefillNonChestItem; it is not a lost-item award.
Fresh base-layout loading reapplies held chest flags as opened blocks; CURRENT
reuse retains the existing working layout. No natural custody or persistence
result is added here.

## All eighteen warps and F80 lifetime

**Confirmed:** `6-warp-events.asm` spans `$AA930..$AA9C2`: eighteen eight-byte
rows plus endWord. All use no-scroll0. R/U/L/D mean facing0/1/2/3. CURRENT255
retains map identity and population rather than requesting a fresh29 entry.

| Row / address | Trigger X,Y | Destination map / X,Y / facing |
| --- | --- | --- |
|0 /$AA930|any,0|77 /41,42 /D|
|1 /$AA938|any,21|77 /41,42 /D|
|2 /$AA940|3,3|CURRENT /35,3 /R|
|3 /$AA948|35,3|CURRENT /3,3 /L|
|4 /$AA950|6,15|CURRENT /38,15 /L|
|5 /$AA958|38,15|CURRENT /6,15 /R|
|6 /$AA960|8,10|CURRENT /40,10 /L|
|7 /$AA968|40,10|CURRENT /8,10 /R|
|8 /$AA970|11,16|CURRENT /44,16 /L|
|9 /$AA978|44,16|CURRENT /11,16 /R|
|10 /$AA980|18,10|CURRENT /50,10 /L|
|11 /$AA988|50,10|CURRENT /18,10 /R|
|12 /$AA990|21,4|CURRENT /53,4 /R|
|13 /$AA998|53,4|CURRENT /21,4 /L|
|14 /$AA9A0|21,16|CURRENT /53,16 /R|
|15 /$AA9A8|53,16|CURRENT /21,16 /L|
|16 /$AA9B0|28,6|CURRENT /60,6 /L|
|17 /$AA9B8|60,6|CURRENT /28,6 /R|

These eight reciprocal stair pairs cross the two area rectangles. The no-scroll
caller still unwinds through SwitchMap/CheckBattle, then ExplorationLoop.
For negative/CURRENT map, SwitchMap/CheckBattle use current29; no battle row
exists. ExplorationLoop skips new actors/mapsprites, temporary-flag clearing
and **SetFlag80**. It updates the main actor/raft declaration, loads current
area/planes/roof state and runs the return-only init. LoadMap with negative
d1 retains base layout rather than reloading chest/flag copies.

An explicit29 exploration entry instead sets CURRENT_MAP29/CURRENT_BATTLE255,
loads resources, rebuilds player/followers/residents, clears only temporary
flags256..383, then **sets80** before LoadMap/init. F193–195/900 and orders are
outside that temp range. Both entry forms first subtract20000 from STEP_COUNTER
with a floor at0 and call HealLivingAndImmortalAllies. Its counter29/DBF covers
**30 allies0..29**, not32: living allies plus Peter/Lemon receive HP/MP restore;
status is first AND7 (stun/poison/curse), then UpdateCombatantStats recomputes
**equipment-derived CURSE**. It is not a blanket promise to preserve raw curse.

| Changed boundary | F80 / order consequence |
| --- | --- |
|Fresh explicit29, including a later return from77|Sets80; existing nonzero orders classify ready on next smith invocation.|
|CURRENT stair or ordinary CURRENT reload|Retains80, orders, actors, temps and working layout; no new readiness. Area/plane/init/common healing still run.|
|Same visit, walking, area change or repeated talking|No local SetFlag80 or timer producer; retains the current bit.|
|Successful placement|Clears80 after picker, even if the picker finds no empty word.|
|Collection/refusal/cancel without placement|No80 clear in these actions; uncollected words survive.|
|Regional nonzero scroll|Different retained transition; no exploration entry/SetFlag80. Not any29 stair.|

Thus leave/reenter via explicit77/29 can change pending to ready under the
stated admission premises. Walking stairs or waiting alone does not supply
that source transition. An arbitrary direct LoadMap/reload scene call does not
imply ExplorationLoop's SetFlag80; save/load and natural continuation remain
separate observations.

## Smith entry, party list and order classification

**Confirmed:** Map29_EntityEvent0 tail at$5A110 resolves
`j_BlacksmithMenu` `$2000C`→BlacksmithMenu `$21A3A`. It inherits the entity
wrapper's return address, rather than creating a post-JSR map-handler resume.
`blacksmithactions.asm` `$21A3A..$21EB6` saves d0-a5, links a6 with 24 bytes,
conditionally opens the current portrait, emits194 and closes it. It clears
ready(-18), pending(-14), fulfilled(-16), and fulfillFlag(-20), then calls
ProcessBlacksmithOrders. On return: conditional portrait,198, text/portrait
close, UNLK and restored d0-a5/RTS back to the wrapper.

ProcessBlacksmithOrders first calls UpdateForce. The joined-flag loop begins
at ally0 with COMBATANT_ALLIES_COUNTER29/DBF; joined flags0..29 admit0..29,
including reserves, without an HP gate. Active flags separately classify the
battle party. TARGETS_LIST_LENGTH.w copies to GENERIC_LIST_LENGTH.w; target
bytes copy to GENERIC_LIST using its word length followed by **SUBQ.B1** and
DBF. For lengths1..30 this copies that many IDs. Supplied zero joined members
produces d7=$00FF and 256 copies, not a guarded empty list. No natural zero-force
reach or repair is claimed. The member-list UI's actual selection remains its
own consumer, rather than an active-party-only or alive-only smith rule.

Order storage is four two-byte words at MITHRIL_WEAPONS_ON_ORDER
`$FFF7A8..$FFF7B0`. Zero is empty; any nonzero word is treated as occupied.
The words retain only selected items, without material owner, customer,
recipient, price, per-order ready bit or elapsed time.
CountPendingAndReadyToFulfillOrders checks80: nonzero sets fulfillFlag=1.
Its four-word scan counts all nonzero words as ready if that flag equals1,
otherwise pending. Within one fresh menu frame initial orders cannot split
between ready and pending by age. Malformed nonzero words are not validated.

With fulfillFlag1, the outer loop visits physical slots0..3, skipping zero.
It stores ordersCounter=4,3,2,1 respectively and calls fulfillment; that action
preserves d0-a1, so cancellation returns to the next slot, not an immediate
exit from the whole smith menu. Afterwards the effective count is
`ready + pending - fulfilled`. Initial ready/pending counters are snapshots;
placement increments pending, fulfillment increments fulfilled. There is no
rescan or per-order reservation around later selections.

| Menu-entry state / later change | Actual control result |
| --- | --- |
|Empty orders,80 either|No delivery;195 then material selection.|
|1..3 occupied,80 clear|206 (more time),196, then material selection.|
|4 occupied,80 clear|206 then return; no placement through this ordinary branch.|
|1..4 occupied,80 held|Try each nonzero word in ascending slot order. Refused/full/cancelled words remain; other slots still get attempts.|
|Ready visit leaves effective count4|Return without placement.|
|Ready visit leaves effective count below4|196 then material selection; can hold old uncollected ready words plus newly placed pending words in this invocation.|
|Reopen after a new placement cleared80|All surviving nonzero words classify pending, including previously ready/refused ones.|
|Raw malformed items or inconsistent/mutated counters|Tests remain zero-word, literal equality1 and effective-count equality4; there is no general capacity clamp or state repair.|

The placement admission itself contains no free-order precheck. Ordinary outer
routing avoids starting with four unfulfilled orders, and the continuation stops
when its counter equals4. A direct supplied frame or intervening storage change
can still reach payment with all slots occupied. The accepted helper H3 proves
only its all-occupied no-write return, not that ordinary play reaches this loss.

## Complete placement gates and committed order

**Confirmed:** BlacksmithAction_PlaceOrder `$21CDA` saves d0-d2 and follows
these branches in the complete original body:

| Stage | Exact predicate / result |
| --- | --- |
|Material prompt199|GIVE action, SELECTED_ITEM_INDEX=NOTHING; member screen returns owner d0, slot d1, item d2. d0=-1 exits action.|
|Material check|Save owner/slot/item; d2.w must equal BLACKSMITH_MITHRIL_ITEM123 (ITEM_MITHRIL$7B). Other returns emit200 and repeat material prompt.|
|Customer prompt201|USE action, selected item NOTHING; d0=-1 returns to material prompt, without charge/removal. Otherwise save targetMember.|
|Promotion|GetClass into clientClass; unsigned class>=CHAR_CLASS_FIRSTPROMOTED12 continues; otherwise211 and customer retry.|
|Eligibility|IsClassBlacksmithEligible scans the 15 word classes from its count prefix; d0=0 accepts, nonzero212 then customer retry.|
|Confirmation202|Target name and cost5000; open gold window→actual yes/no→close gold window. Zero accepts; nonzero197 then customer retry.|
|Funds|GetGold longword; unsigned gold>=5000 accepts (equality included); otherwise203 and action return.|

The fifteen eligible classes are HERO,PLDN,PGNT,GLDT,BRN,WIZ,SORC,VICR,MMNK,
SNIP,BRGN,BDBT,BWNT,NINJ,RDBN. `classes` emits a word count and word IDs;
this is separate from the nine picker class groups.
For GIVE, ValidateMembersListScreenConfirmationInput requires a nonempty material
owner. LoadMemberSummaryIcons selects a slot and returns its raw displayed
item word in d2; member-screen exit preserves d0-d2. The smith compares that
exact word to123 without a local mask: flag-bearing Mithril words do not pass
merely because their low seven bits equal123. Cancelling the item-slot prompt
returns d1=-1 and reopens member selection; cancelling that list returns d0=-1
to the smith. USE0 bypasses the material-owner nonempty test for customer or
recipient selection; no HP/active-party filter is introduced there.

`yesnoprompt.asm:alt_YesNoPrompt` ultimately reloads CURRENT_DIAMOND_MENU_CHOICE.b
and **EXT.W**: confirmed Yes returns0; No and B return-1. An earlier zero-extended
byte255 is intermediate. The smith uses zero/nonzero predicates, never a
special distinction between No and B. Controlled H3 prompt words remain harness
inputs, without claiming original UI execution.

At `@PlaceOrder` `$21DC2`, the irreversible source order is:

1. DecreaseGold(5000), writing CURRENT_GOLD.l.
2. Increment this invocation's pending word.
3. DropItemBySlot(material owner, selected material slot), compacting that
   owner's four item words and refreshing combatant stats.
4. PickMithrilWeapon(customer class): consume RNG and write the first empty
   order word if one exists.
5. ClearFlag80, then204→205 presentation.
6. Compute pending+ready-fulfilled. Equality4 emits210 and exits. Otherwise196
   plus yes/no: zero repeats material selection; nonzero197, close and return.

Material owner, weapon customer and later collection recipient are three
independent selections. The target class affects the picker but the order does
not retain that person. Gold charge/material removal precede order writing;
neither drop result nor picker success is checked, and no rollback appears.
Source-valid Mithril is not cursed equipment and ordinary selected slots are
valid. Supplied stale/empty/cursed-equipped material or full order storage is
not revalidated into a guaranteed safe transaction. Pending still increments
and 80 clears even on the helper's all-occupied no-write return. Presentation
interruption cannot undo the preceding mutations.

## Complete class mapping and sequential RNG

**Confirmed:** `data/stats/items/mithrilweapons.asm` `$21F62..$21FD2` and
`data/stats/allies/classes/blacksmitheligibleclasses.asm` `$21EB6..$21ED6`
join the actual picker `$21ED6..$21F62`. `classes` count-prefix encoding and
`mithrilWeapons` alternating parameter/item **bytes** are checked with the
macros/enums, rather than relying on the table's probability comment.

| Declared class group | Actual row selection |
| --- | --- |
|0 HERO,BDBT|0|
|1 PLDN,PGNT|1|
|2 GLDT|2|
|3 WIZ,SORC|3|
|4 VICR|4|
|5 SNIP,BRGN,BWNT|5|
|6 NINJ|6|
|7 MMNK|7|
|8 BRN,RDBN|Not scanned; fallback draw range2: zero→row2, nonzero→row0.|

MITHRIL_WEAPON_CLASSES_COUNTER7/DBF scans only the first eight groups.
The ninth is declared data, not a ninth weapon row. The fallback applies to
**any unmatched supplied class**; the ordinary eligibility gate restricts its
normal admitted domain to BRN/RDBN. Row index shifts left3 for eight-byte rows.

| Row | First pair, range16 | Next, range8 | Next, range4 | Last, range1 |
| --- | --- | --- | --- | --- |
|0|LEVANTER|COUNTER_SWORD|BATTLE_SWORD|CRITICAL_SWORD|
|1|HOLY_LANCE|HALBERD|MIST_JAVELIN|VALKYRIE|
|2|RUNE_AXE|GROUND_AXE|ATLAS_AXE|HEAT_AXE|
|3|MYSTERY_STAFF|FREEZE_STAFF|GREAT_ROD|SUPPLY_STAFF|
|4|MYSTERY_STAFF|GODDESS_STAFF|GREAT_ROD|HOLY_STAFF|
|5|GRAND_CANNON|GRAND_CANNON|HYPER_CANNON|BUSTER_SHOT|
|6|GISARME|NINJA_KATANA|KATANA|CRITICAL_SWORD|
|7|GIANT_KNUCKLES|GIANT_KNUCKLES|MISTY_KNUCKLES|MISTY_KNUCKLES|

Each pair reads parameter then item, calls GenerateRandomNumber with that
range, and accepts only d7=0; nonzero consumes the next pair. Range1 still
advances the seed and guarantees zero for this generator, so the row always
terminates. An ordinary matched class consumes1..4 draws; fallback adds one
preceding range2 draw, hence2..5. Different early decisions change subsequent
seed consumption. The picker then scans four words in increasing address order,
writes only the first zero word and returns; all occupied means no order write,
after selection draws already occurred. Its MOVEM restores d0-a0, including d7.

`randomnumbergenerator.asm:GenerateRandomNumber` updates the word seed to
`(13 * seed + 7) mod 65536`, doubles the word range, multiplies unsigned,
takes the product's upper word then shifts it right1. For these small ranges
this yields `floor(newSeed * range /65536)`. The draws are sequential and
correlated; range16/8/4/1 are conditional draw bounds, not independent final
item percentages. This account makes no frequency/distribution claim and runs
no all-seed simulation. Menu/text RNG before the helper can affect its input.

## Fulfillment, inventory and optional equipment

**Confirmed:** BlacksmithAction_FulfillOrder `$21B42..$21CDA` starts207→166,
sets selected item and USE action, then enters member selection at$21B58.
Recipient d0=-1 gives197 and returns with the order intact. Otherwise the
recipient becomes clientMember, independent of the original material/customer.

| Admission branch | Actual continuation |
| --- | --- |
|Held count>=4|208 then prompt; zero reselects recipient, nonzero197→Done; order intact.|
|Capacity below4, equipment type TOOL0|Go directly to AddItem.|
|Non-tool, IsWeaponOrRingEquippable carry set|Go directly to AddItem.|
|Non-tool, carry clear|167 then prompt; zero accepts carrying anyway; nonzero reselects recipient.|

GetItemBySlotAndHeldItemsNumber masks all four inventory words with$7F and
counts every non-NOTHING127. GetEquipmentType checks weapon before ring and
returns weapon1/ring-1/tool0. IsWeaponOrRingEquippable tests the recipient's
class mask and equipment type, preserving its carry through register restore.
It does not require the recipient to equal the commission customer.

At `@AddItem` `$21BE4` the exact committed sequence is AddItem(recipient,item),
physical order read→word clear→fulfilled increment, then equippability check.
Physical slot is `4 - ordersCounter`, scaled by2. AddItem masks input with$807F,
stripping equipped/other flags while retaining the broken bit, writes the first
masked-NOTHING word and returns d2=0. Full returns1. **This caller does not test
d2 before clearing the order.** Ordinary capacity was checked earlier; supplied
or changed full storage is not protected by rollback. AddItem itself does not
refresh combatant stats. An interruption after add but before clear is not
atomic fulfillment; no duplicate-prevention recovery is supplied.

After clear, carry-clear means209 and Done. Carry-set asks173 whether to equip;
zero continues, nonzero209 and Done. The received item/order clear remain
committed even if equipment is declined or blocked. On acceptance:

1. GetEquipmentType: weapon1 chooses GetEquippedWeapon; every other branch
   chooses GetEquippedRing. A returned d1=-1 skips old-item removal.
2. Existing equipment calls UnequipItemBySlotIfNotCursed. Result2 emits176
   then209/Done without replacing it. Other results continue.
3. Recount held items, use **count-1** as the new equip slot, call EquipItemBySlot.
   This assumes ordinary packed inventory; arbitrary holes can make count-1
   differ from AddItem's first-empty slot. No general inventory repair is made.
4. Result2 requests cursed-item music, PlayMusicAfterCurrentOne(previous music)
   and WaitForPlayerInput, then175/Done. Every other result emits174/Done;
   the caller does not separately reject helper results1 or3 here.

EquipItemBySlot checks NOTHING/equippability/curse, sets the low-byte equipped
bit7 when allowed, and tails UpdateCombatantStats even on a rejected path.
Cursed-aware unequip clears that bit only for result0, then also tails stat
refresh. Refresh clears raw curse from its retained-status mask and rebuilds
CURSE4 from equipped cursed definitions. Refusing a cursed weapon can therefore
set status CURSE during refresh despite no equipment replacement; the accepted
controlled Dark Sword case records0→4. DropItemBySlot likewise compacts and
refreshes, with equipped-cursed refusal2 preserved.

The complete 32-choice/26-distinct Mithril domain has weapon type, zero ring
and zero cursed definitions, as retained by
[common menus](./common-menus.md#static-service-menu-state-machines). The ring,
tool and newly-equipped-cursed branches remain complete source branches,
without claiming them as ordinary Mithril outputs. A previously equipped cursed
weapon is a valid obstruction. Fulfillment failures before AddItem retain the
order; equipment failures after it retain the newly carried item with the order
already gone. Neither refunds gold or recreates Mithril.

## Existing observations and serialization boundary

**Confirmed (retained bounded observations):** [common menus](./common-menus.md#static-service-menu-state-machines)
owns the five helper cases, three committed-order cases, three direct AddItem
cases, five precommit controlled-result cases, five optional-equip cases and
four prompt-routing cases, 25 in the retained grouped blacksmith rail. Their
entry boundaries, disposable session-ROM patches/stubs, scoped restoration,
neutral presentation stops and zero-callback completion remain there. The
separate 15-case service-entry/return rail observes the Map29 smith **tail**
representative, outer saved frame and controlled ProcessBlacksmithOrders return;
it bypasses the service body. Together these are reusable narrow evidence,
not a naturally traversed village/order/revisit/collection session. No H3 is
rerun and no presentation/input/save behavior is promoted from these seams.

**Confirmed (static serialization ownership only):** `sramfunctions.asm:SaveGame`
starts at COMBATANT_DATA=$FFE800 and copies SAVE_SLOT_REAL_SIZE4016 logical bytes,
covering `$FFE800..$FFF7B0`. CopyBytesToSram decrements the count then DBF copies
each byte to every second SRAM address, accumulating an eight-bit checksum;
SaveGame writes checksum/occupied flag. LoadGame copies the corresponding
logical bytes back. This range contains all 30 ally records and their four item
words, CURRENT_GOLD.l `$FFF600`, GAME_FLAGS `$FFF686` (including80/193–195/900)
and all four order words ending exactly at$FFF7B0. The smith's24-byte stack
frame/counters are transient and outside that serialization authority.
The selection menu does not call SaveGame as part of payment or delivery.

A saved F80 byte and saved orders do not establish later ready behavior: a
subsequent explicit exploration entry can set80 regardless of the loaded bit.
The selected church save lifecycle has its own egress/flag399/Suspend choices
in [common menus](./common-menus.md#church-save-lifecycle); this village chapter
does not execute them or assert cross-process, power-loss, physical SRAM,
torn-transaction or automatic-save durability. Exit/revisit readiness follows
the actual entry producer above, not a promised persistence result.

## Actual research and production consumers

**Confirmed (code inspection, without new lowering/execution):** the following
owners retain data/control rather than establishing village implementation.

| Consumer | Actual capability / boundary |
| --- | --- |
|[H2 menus](../../src/sf2tool/h2/menus.py), `_blacksmith_static_contract`|Complete eight named-function instruction/control records, use-site constants/frame/order width/copy/readiness/placement/fulfillment/picker relations. This is verification data, not the gameplay service.|
|[H2 item auxiliary](../../src/sf2tool/h2/item_auxiliary.py), `_parse_mithril` and consumer audit|Nine class groups/eight rows, source/H1/ROM joins, fallback/denominators. Its remaining/denominator Fraction fields model sequential ideal draws; they are not measured correlated-generator frequencies. Complete shop records remain owned there.|
|[Map compiler](../../src/sf2tool/remake_exploration_maps.py), `compile_maps`|Selected canonical map/setups, entity interactions and source zones, layout flags/doors/roofs, entry temp clears+80 and Preserve/Rebuild warps. It does not bind description/item-event/chest-service tables or a smith order model. A structurally selected map is not an admitted playable29.|
|[Program compiler](../../src/sf2tool/remake_exploration_programs.py), `OriginalPrograms.compile`|Typed flags/text/calls and supported scene operations exist. The actual `jmp j_BlacksmithMenu` and church/shop tails have no typed service case and fall through to native-call. Shop's preceding byte RAM store is also outside the typed service family. No fake lowering result or imported service body is claimed.|
|[Content reader](../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs)|native-call→UnsupportedInstruction, with source identity. No Blacksmith order/selection/fulfillment opcode is defined.|
|[Application runner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs)|UnsupportedInstruction raises program-opcode before that instruction commits; earlier committed prefixes remain. Generic text/flag/scene/warp support does not execute the missing service.|
|[Map transfer](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs)|Build/Apply retain admitted map entry flags and layout lifetime; same-map Preserve and Rebuild exist. Source runtime admission, original scroll lifetime and missing content are not supplied by these primitives.|
|[Choice runner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationChoiceRunner.cs)|Real story yes/no selection exists; it is not the item/member list, commission UI or service transaction.|
|[Domain loadout](../../remake/src/Sf2.Remake.Domain/Battles/State/BattleInitializationDefinition.cs), [item action](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2ItemAction.cs)|SourceLoadout item words and selected healing-item query/consumption/compaction exist. Source equipment metadata/refresh serve admitted battle inputs; no village inventory exchange, commission customer/recipient or optional-equipment service is added.|
|[Domain RNG](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleRandom.cs)|NextWord implements the source word recurrence/range arithmetic for its battle callers. It has no bound Mithril row/order transaction; a matching primitive is not a smith consumer.|

The [capability owner](../../remake/docs/capability-status.md#milestone-and-deferred-surfaces)
and [four frontiers](./story-content-coverage.md#four-different-frontiers) retain
accepted early-map/battle production, deferred village services/save/load and
independent PR692 UI work. Static closure here changes none of them.

## Verification and remaining questions

For direct source reproduction, `$sourceCheckout` is the explicitly selected
read-only SF2DISASM checkout, not this worktree's cwd. Verify its pin/clean state,
then read complete selected bodies; these commands do not execute the game:

```powershell
git -C $sourceCheckout rev-parse HEAD
git -C $sourceCheckout status --short
git -C $sourceCheckout show c834c652b6862bc5679fd7f69a38a7093206efc6:disasm/code/common/menus/blacksmith/blacksmithactions.asm
git -C $sourceCheckout show c834c652b6862bc5679fd7f69a38a7093206efc6:disasm/code/common/menus/blacksmith/pickmithrilweapon.asm
git -C $sourceCheckout show c834c652b6862bc5679fd7f69a38a7093206efc6:disasm/data/stats/items/mithrilweapons.asm
git -C $sourceCheckout show c834c652b6862bc5679fd7f69a38a7093206efc6:disasm/data/maps/entries/map29/6-warp-events.asm
```

Read the other complete29 setup/content files named above, both77/30 entrance
owners, `code/gameflow/exploration/{exploration,explorationvints,explorationfunctions_0,explorationfunctions_2}.asm`,
`code/common/maps/{mapload,getbattle}.asm`,
`code/common/scripting/map/mapsetupsfunctions_1.asm`,
`code/common/menus/{memberslistscreen,yesnoprompt}.asm`,
`code/common/stats/{battleparty,itemstats,gold,updatecombatantstats}.asm`,
`code/common/tech/{randomnumbergenerator,sram/sramfunctions}.asm`,
`code/gameflow/battle/battleloop/heallivingandimmortalallies.asm` and
`code/gameflow/battle/battlefunctions/battlefunctions_0.asm` for the shared
use sites. Source/table inspection and the production-code links above are
required: a mechanical link PASS alone cannot establish the semantic account.

Manual review follows complete local records and named shared use sites through
actual Python/Content/Domain/Application consumers, then checks the prose and
changed-state alternatives. Direct checks cover the exact three paths,
UTF-8/LF, local links/anchors, private boundary, retained pair provenance and
unchanged complete source-ledger tables. On the clean committed head, load
`local/private-inputs.ps1` in the same process and use
`uv run sf2 verify plan --base origin/main --head HEAD`; interpret the docs-only
selection under the [verification scope](../../remake/docs/development-and-verification.md#scope).
Exact-head scoped CI and the complete local handoff record actual results.
No normal/full suite, SDK/engine/adapter/Godot, lowering, emulator/H3/H4,
screenshot, export or old comparison rerun is an acceptance step here.

**Unknown:** natural Dry Stone-to29 walking/actual terrain and actors; actual
member/material/customer/recipient inputs and carried seed; complete UI/text/
portrait/audio timing and return to usable control; full church/shop visits;
original save/reload durability and interruption recovery. These do not block
the defined static outcome and do not create an acquisition queue.
The complete later-revisit/Arm-Claude/Yeel-Chaz optional branches remain in
[story coverage](./story-content-coverage.md#optional-content-and-branches).

The [Fairy Woods correction](./battle44-fairy-woods-trial-and-records.md) retains
its original author/main/CI passes and their missed ally-domain/prompt-return
errors. This slice explicitly uses30 allies0..29, final EXT.W prompt return,
and the healing AND7 followed by equipment-derived CURSE. The linked chapter
owners and [retained comparisons](../../remake/docs/evidence/retained-comparisons.md)
continue to preserve completed failures/Unknowns, including #1047's twelve
unresolved comparisons. No completed run is reset or relabeled as interrupted.
