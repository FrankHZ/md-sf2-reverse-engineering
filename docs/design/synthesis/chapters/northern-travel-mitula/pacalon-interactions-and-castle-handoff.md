# Pacalon: conversations, supplies and the castle handoff

Pacalon offers two sets of resident conversations, the same weapon and item
shops, a priest, examination text and two possible non-chest pickups. North
exit records request the castle; a south record requests the overworld.
**Confirmed:** these are separate local interactions and request paths. No
conversation, purchase, church service or pickup is a prerequisite in those
exit records. **Unknown:** which interactions a naturally arriving player
can reach, what completes on screen, and what persists.

This guide continues [Battle26 return through conditional Pacalon entry](battle26-return-through-pacalon-entry.md)
on its Map02 exploration alternative. The predecessor's possible Battle31
diversion supplies no town interaction. This guide stops when an outgoing
no-scroll warp returns operands to MainLoop, before destination battle
selection, setup or initialization. It explains accepted original evidence;
it adds no remake behavior or new observed campaign frontier.

Continue the three north requests with
[Pacalon castle: entry and the first drought-state visit](pacalon-castle-entry-and-first-visit.md).
That guide consumes destination admission/setup/init, default castle content
and CURRENT stair lifetimes; this guide's request endpoint remains unchanged.

## Evidence and reading boundary

**Confirmed evidence owner:** [Pacalon interactions and castle handoff](../../../../research/chapters/northern-travel-mitula/pacalon-interactions-and-castle-handoff.md),
accepted through [#1021 / PR1023](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1023)
at `ac35fe192edba718a3261fef67d92874a477fd76`, supplies the selected local
tables, complete literals and consumer joins. The accepted predecessor
[PR1024](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1024)
supplies entry/population premises. Source identities below are relative to
`disasm/` at SF2DISASM `master` commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`; the research owner retains the
USA retail ROM and source/address provenance. F530 means flag 530, indices
are zero-based, and address ranges are half-open.

Standard rigor is appropriate for this bounded English synthesis: current
table selection, retained actors, service commits and mutable requests have
different lifetimes. [#1026](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1026)
owns its five Markdown paths and direct acceptance. Castle content, later
battles, a general service audit, source/layout acquisition, runtime,
tooling and Chinese synchronization are outside scope. Concurrent castle
research and UI692 supply no evidence. Evidence disagreements return to the
research owner rather than being repaired in this explanation.

## Finding a resident and selecting the current conversation

**Confirmed:** `data/maps/mapsetups.asm` selects `ms_map2` or its F530
override `ms_map2_flag530`. Their six-pointer tables differ in population,
entity events and descriptions; they share zone events, item events and an
empty init. `GetCurrentMapSetup` reads the current map and selector each
time. Population, conversation and examination therefore need not use the
same F530 value. These local callbacks do not change F530.

Ordinary C interaction follows `ProcessPlayerAction`'s idle and
movement/view-scroll waits; A takes the field-menu branch first. Debug and
overlapping physical1 Caravan alternatives precede ordinary actor lookup.
For view target0, `GetActivatedEntity` uses position/facing, steps through
desks/counters of layout kind $2800 under mask $3C00, and scans physical
actors in order for distance below 384 from the target. It skips the viewed
actor and follower-list actors. A layer read is not a layer-equality test;
negative or other camera states are not proof of ordinary activation.

`GetEntityEventIndex` then reverse-searches `ENTITY_INDEX_LIST` from its
start. After 32 ally entries, logical numbering becomes 128. The first alias
wins; failure returns 0. For n successfully declared ordinary followers,
fixed row j normally maps logical `128+j`, lookup offset `32+j`, to physical
`n+1+j`, while Bowie is physical0. Actual followers, aliases, HP/class,
storage and retained mappings remain premises, not a guaranteed cast.

`RunMapSetupEntityEvent` stores supplied player-facing d2 in
`EVENT_RELATIVE_POSITION`, selects the current setup, then takes the first
four-byte logical match or $FD default. Callback offsets are signed words
relative to the selected table. The set530 table's backward offsets reuse
the original service/default bodies; they are not new services.

Portrait and speech SFX come from the actual mapped sprite. The wrapper
prefills `CURRENT_SPEECH_SFX` and `CURRENT_PORTRAIT`, conditionally opens a
portrait, and resolves the actor again. Every named row below uses UP1:
bit0 waits a VInt and requests `(playerFacing+2)&3`; bit1 is clear, so the
saved actor facing is not restored after the callback. UP is an action
mask, not a required approach direction. The default uses 0 and requests
neither turn nor restoration. The wrapper disables VInt_UpdateEntities
before the callback, then closes portrait/text and activates that VInt
function on ordinary return. An RTS default still has wrapper work;
failed mapping is not useful dialogue with Bowie or a mapping repair.

## Both resident-event alternatives

**Confirmed:** the complete clear530 table in
`data/maps/entries/map02/mapsetups/s2_entityevents.asm`,
`$5E938..$5E9C8`, has ten named rows and a default. M/F below mean declared
male/female centaur sprites 233/234; merchant 193 and priest 192 are distinct
sprites. Origins and walking declarations belong to the
[entry population tables](battle26-return-through-pacalon-entry.md#town-population-is-conditional-content).
They do not freeze actors at their origins.

| Logical / declared actor | Callback in Map2_ | Complete local work and dialogue theme |
| --- | --- | --- |
|128 /M|EntityEvent0|3583 →3584: asks whether the visitors are devils and how they arrived safely.|
|129 /M|EntityEvent1|3585: hunger.|
|130 /F|EntityEvent2|3586: asks strong visitors to kill the devils.|
|131 /M|EntityEvent3|3587 →3588: calls Pacalon's proud soldiers cowards hiding in the castle.|
|132 /F|EntityEvent4|3589: fear of losing.|
|133 /F|EntityEvent5|3590 →3591 →3592: asks about Geshp, calls him friendly and wonders whether all devils are like him.|
|134 /M|EntityEvent6|3593 →3594: asks whether Pacalon can win, urges an affirmative answer and praises its fighters.|
|135 /merchant|EntityEvent7|Write weapon-shop index 10; call j_ShopMenu.|
|136 /merchant|EntityEvent8|Write item-shop index 25; call j_ShopMenu.|
|137 /priest|EntityEvent9|Call j_ChurchMenu.|
|$FD /default|DefaultEntityEvent|RTS only.|

**Confirmed:** the complete set530 table in `s2_entityevents_530.asm`,
`$5E9C8..$5EA20`, has nine named rows and a default:

| Logical / declared actor | Callback | Complete local work and dialogue theme |
| --- | --- | --- |
|128 /M|Map2_212_EntityEvent0|3595 →3596: introduces a kingdom of fine centaurs and welcomes the visitors.|
|129 /M|Map2_212_EntityEvent1|3597: attributes rain to a present from the gods.|
|130 /M|Map2_212_EntityEvent2|3598: no longer thirsty after rain, still hungry.|
|131 /F|Map2_212_EntityEvent3|3599 →3600: asks whether the visitors saved Mitula and why they do not defeat the devils.|
|132 /F|Map2_212_EntityEvent4|3601 →3602: asks whether devils occupied Tristan and connects that to the lack of rain.|
|133 /M|Map2_212_EntityEvent5|3603: reports seeing Geshp frequently staring at the castle.|
|134 /merchant|Map2_EntityEvent7|Write 10; call j_ShopMenu.|
|135 /merchant|Map2_EntityEvent8|Write 25; call j_ShopMenu.|
|136 /priest|Map2_EntityEvent9|Call j_ChurchMenu.|
|$FD /default|Map2_DefaultEntityEvent|RTS only.|

Every text sequence and service call has a local RTS. None of the 17 unique
callback bodies recruits, writes story/membership flags, installs a cutscene,
posts a map request or directly calls YesNoPrompt. The question and demanded
answer in 3593–3594 are prose, with no choice-result branch. Shared services retain
their conditional effects; that does not make their invocation a completed
transaction or a side-effect-free callback.

Changed F530 after population is significant. With a retained clear530 cast
but set530 event selection, logical135's weapon merchant can invoke the
item shop,136's item merchant the church, and137's priest the default.
With a retained set530 cast but clear530 selection,134 invokes old Event6
text,135 the weapon shop and136 the item shop. Current mapping and selection
explain these possibilities; no natural flag transition was observed here.

**Inferred:** drought/anxiety and restored-rain themes connect these
conversations. Residents' claims about Geshp, gods, soldiers, Tristan or
Mitula remain dialogue, not independent world-state proof or travel
requirements. Rain does not remove every expressed fear or hunger.

The research owner supplies all 21 exact literals at gamescript keys 0DFF..0E13.
The first text of each multi-text sequence ends W2; 3591 also ends W2.
Every final text ends W1. There are no name/item substitutions in these
resident strings. `txt` emits the TEXTBOX trap plus a text word;
`trap5_textbox.asm` calls DisplayText. Each DisplayText call resets its
substitution pointer to `DIALOGUE_NAME_INDEX_1` and decodes its selected
bank/Huffman text. Consecutive txt calls imply neither a close nor a sleep,
and do not increment the cutscene text cursor.

The [dialogue contract](../../../contracts/interface/dialogue-system.md#poll-order-and-distinct-consumers)
keeps W1's RNG draw/copy →VInt wait →masked input test and typewriting
restoration distinct from W2's draw/copy/cursor-helper/wait/input loop,
validation sound and cleanup. Neither is a YesNoPrompt. Text termination,
CLEAR and wrapper clsTxt are separate operations. These source rules add
no Pacalon rendering, input-read or timing observation.

## The two shops and priest

**Confirmed:** Event7 writes byte `SHOP_WEAPON_PACALON=10`; Event8 writes
`SHOP_ITEM_PACALON=25` to `CURRENT_SHOP_INDEX`. Both call j_ShopMenu.
`GetShopInventoryAddress` skips that many length-prefixed rows;
`PopulateShopInventoryList` copies the selected count/items. The accepted
`data/stats/items/shopinventories.asm` rows are:

| Index / source comment | Ordered catalog (ITEM_ prefix) |
| --- | --- |
|10 /Weapon shop11|BUSTER_SWORD, GREAT_SWORD, HEAVY_LANCE, JAVELIN, LARGE_AXE, ASSAULT_SHELL, GREAT_SHOT, GUARDIAN_STAFF, INDRA_STAFF, POWER_GLOVE|
|25 /Item shop11|MEDICAL_HERB, HEALING_SEED, HEALING_DROP, ANTIDOTE, FAIRY_POWDER, ANGEL_WING|

These ten/six offerings are not granted items or a separate local shop-type
byte. Both event alternatives reuse the same catalogs. ShopMenu
conditionally opens the current portrait, requests 158, closes text/portrait
and enters buy/sell/repair/deals selection. Text 158's guarantee of good
condition ends D3, a delay command, not a purchase or acknowledgement.

The [service contract](../../../contracts/party/service-interactions.md#shop-source-boundary)
owns the shared gates and order. Selected item definitions/prices, gold,
four-slot member capacity, equipment, unsellable/rare/broken flags and deals
stock matter. Buy/deals decrease gold before AddItem; deals then remove
the deals entry. Sell grants gold before dropping the slot and can add a
rare item to deals. Repair charges before repairing the selected slot.
Cancellation and continued menu loops remain possible. Catalog selection
proves no successful purchase, equipment change or persistent resource result.

Event9 directly invokes j_ChurchMenu, without a local first-visit warning
or flag. ChurchMenu conditionally opens the portrait, requests 110 with its
two W2 controls, closes text/portrait and reaches raise/cure/promote/save
selection. Contact does not automatically heal, save or change EGRESS_MAP.
The accepted Raise/Cure/Save observations are bounded shared-service seams,
not natural Pacalon observations.

For the selected Save route, the first YesNoPrompt result 0 reaches
@DoSaveGame: copy the then-current CURRENT_MAP byte (nominally 2) to
EGRESS_MAP, load CURRENT_SAVE_SLOT, set 399, and call SaveGame. A nonzero
first result avoids those save writes and reaches @ExitSave's text 112,
"Do you have another desire?": zero loops to @StartMenu for another service,
nonzero exits the menu. After saving, music/input work precedes texts 115/116
and the separate continue-adventure/suspend prompt: zero reaches @ExitMenu,
nonzero requests text 117 and reaches FadeOutToBlack/WitchSuspend instead
of ordinary callback completion.
These are actual choice branches, unlike the resident's requested answer.

The existing Map02 savepoint is `(41,10)`, Up. It is return data; it does
not teleport a visitor on priest contact. The accepted savepoint consumer
uses the table under 399, with its initial fallback when 399 is clear.
The [Save seam](../../../contracts/party/service-interactions.md#church-source-boundary)
observes original SaveGame entry/return in controlled cases and a controlled
Fade return/WitchSuspend entry. It supplies neither a Pacalon save-completion
observation nor cross-process persistence.

## Looking at signs, statues and the well

**Confirmed:** the two area-description wrappers are 16 bytes each, followed
by seven six-byte rows and a two-byte $FD00 terminator. Clear530 is
`$5EA26..$5EA62`, set530 `$5EA62..$5EA9E`; both use base $1030=4144.
Every local row has condition 0 and text payload-kind 0. The complete ordered
alternatives differ only in the well's description offset:

| Row / coordinate | Investigation offset →ID | Clear530 description offset →ID / content | Set530 description offset →ID / content |
| --- | --- | --- | --- |
|0 /8,13|2 →425, sign|0 →4144, weapons advertised within|0 →4144, same|
|1 /15,10|2 →425, sign|1 →4145, tools advertised within|1 →4145, same|
|2 /17,14|9 →432, statue|2 →4146, fine soldier|2 →4146, same|
|3 /17,18|9 →432, statue|3 →4147, fine soldier|3 →4147, same|
|4 /25,18|9 →432, statue|4 →4148, fine soldier|4 →4148, same|
|5 /6,5|5 →428, well|5 →4149, `A sturdy well.{N}No water there.{W1}`|6 →4150, `A very deep well.{W1}`|
|6 /25,14|9 →432, statue|7 →4151, fine soldier|7 →4151, same|

Investigation texts 425/428/432 name the sign/well/statue using NAME, W2 and
CLEAR, before the final W1 description. `CheckArea` clears
DIALOGUE_NAME_INDEX_1 and speech SFX to 0: NAME selects member0's name, not
an activated resident. The well's changed wording grants no water and
writes no F530; no local description has a function payload or choice.

Examination first derives the tile ahead for ordinary view target0.
Layout kinds $1800 chest, $2C00 vase, $3000 barrel, $3400 bookshelf and
$1C00 area item take priority over the description wrapper. A coordinate
does not establish a decoded object, collision or walkable inspection route.
For ordinary no-entity C, d6=1. `DisplayAreaDescription` scans packed
coordinates in order; nonzero conditions require d6=0, while these condition 0
rows remain eligible. The first admitted row displays 423+investigation
offset then 4144+description offset, closes windows and returns d7=-1.
No match returns d7=0; CheckArea with d6=1 returns 0, permitting the field-menu
fallback. Supplied d6=0 instead selects generic 423/412 on no match. No
activated actor, a default entity callback and no matching description
are different outcomes.

## Two possible pickups and their failure boundaries

**Confirmed:** `7-chest-items.asm`, `$96A7E..$96A80`, is only endWord.
The other-item table, `$96A80..$96A8A`, has two four-byte rows and endWord:

| Candidate coordinate | Collection flag | Item symbol / index |
| --- | --- | --- |
|33,6|172|ITEM_CHEERFUL_BREAD /13 ($0D)|
|27,24|173|ITEM_MEDICAL_HERB /0|

These flags are outside the temporary 256–383 range and independent of F530.
Empty chest data does not disable generic examination. The two coordinates
alone identify neither object placement nor item use, delivery or a walking route.
`CheckArea` reaches `CheckNonChestItem` only through an eligible layout branch.
`GetNonChestItem` uses the current map's other-item pointer at offset 38;
`GetItem` compares low coordinate bytes in row order. Its changed
CURRENT_BATTLE≠255 crop path retains the pinned erroneous stride5, not
the seven-byte battle-coordinate record size. No corrected lookup is assumed.
The original-coordinate layout offset, flag and item return on a match;
negative terminator/no match returns -1.

`CheckNonChestItem` substitutes ITEM_NOTHING for a held collection flag.
Both matched paths set the flag before returning, so a clear 172/173 is
committed before capacity or AddItem succeeds. Items 13/0 take the ordinary
item branch, not a gold chest. The caller sets item substitution index 2,
requests 413, then tries member0's four-slot inventory. If full, UpdateForce
builds the joined-force list, and the ordered OTHER_FORCE_MEMBERS_LIST
fallback tries members with room. It is not limited to the active party and
has no HP gate. That list begins one byte after TARGETS_LIST and assumes
joined Bowie is first: altered unjoined-Bowie state still tries 0 separately
but omits the actual first joined member in fallback.

At the first capacity match the caller invokes AddItem, retains the flag,
requests item music and text 415 for Bowie or 416 with recipient name index 3,
then reaches FadeOut_WaitForP1Input. Text 413 uses W2/CLEAR;
415/416 contain neither W1 nor
W2, so the caller's separate music/plain-input boundary must not become an
invented text wait. Receiving bread/herb does not apply its stat/healing use.
AddItem writes the first empty slot or returns 1 if no slot remains; this
caller does not test returned d2. A capacity check followed by changed
inventory is not a proven successful insertion.

All-full instead requests 417, calls CloseChest and RefillNonChestItem at
the saved candidate coordinates. Refill re-finds the row and clears its
flag, allowing retry. A held flag/no item selects empty-object text rather
than a duplicate grant. Item 0 is a Medical Herb; 127 is the no-item sentinel,
also produced by masking no-match-1. Interruption before AddItem/refill can
leave a flag without an item; after AddItem it can leave changed inventory
without completed presentation. Collection is not atomic.

Both setups' zone and item-use tables each contain only a default RTS.
The zone wrapper still closes windows and waits a VInt/physical0 destination.
The item-use wrapper clears speech, masks the used item, stores player-Y
in EVENT_RELATIVE_POSITION, initializes d6=0 and cleans up/waits. Its default
return leaves generic field-menu handling with its existing owner. Item use
and other-item collection are distinct; neither default produces the warp.

## North to the castle, or south to the overworld

**Confirmed:** `6-warp-events.asm`, `$96A5C..$96A7E`, stores four eight-byte
records then endWord. All are warpNoScroll/type0; warpFacing emits a facing
byte and zero padding. Exact records precede the north wildcard:

| Order | Source X,Y | Requested map | Destination X,Y | Facing |
| --- | --- | --- | --- | --- |
|0|19,0|MAP_PACALON_CASTLE=36|20,30|Up1|
|1|23,0|MAP_PACALON_CASTLE=36|22,30|Up1|
|2|255,0|MAP_PACALON_CASTLE=36|255,30|Up1|
|3|255,31|MAP_OVERWORLD_PACALON_KINGDOM_DROUGHT=71|22,16|Down3|

`esc02_controlCharacter` requires masked layout kind $1000 before
`WarpIfSetAtPoint`; raft and Caravan control have separate $1000 call sites
with their own movement/collision admission. The producer sign-extends
internal coordinate words, divides by 384, and scans current-map rows.
Word $FFFF ends the scan; negative source bytes are wildcards, otherwise
low bytes must match. First match wins. Thus qualifying 19,0 and 23,0 select
their exact destination X20/22 before the north wildcard; another admitted
X at Y0 selects preservation of X. Y31 takes the south alternative.
No-match leaves existing pending state unchanged. None of these rows tests
F530, conversations, services, collection or castle story state; the
wildcards do not prove every edge cell is a reachable warp tile.

The producer writes word MAP_EVENT_WARP=1 to MAP_EVENT_TYPE ($FFA84A),
then longword type/map/X/Y to PARAM1..4 ($FFA84C..$FFA850), then word
facing/padding to PARAM5 and its following byte ($FFA850..$FFA852), then
word SFX_WARP to WARP_SFX. Event type precedes parameters, without a
prior-event guard or queued immutable snapshot. Interrupted writes or
replacement can change the payload actually consumed.

WaitForEvent reads the pending event word before its A/C poll. Zero permits
physical0 control installation under numeric mode 0 character, 1 raft, other
nonzero Caravan; nonzero bypasses installation. ProcessMapEvent clears
the stored type before dispatching its already-read d0. With type 1 and
PARAM1 zero, the no-scroll consumer:

1. Submits WARP_SFX, clears that word and makes physical0 idle.
2. Removes the exploration return address, clears low words d0..d4 and
   loads PARAM2's map byte into d0.
3. Samples physical0's then-current X/Y words and facing; sign-extension
   and division by 384 yield coordinate words at consumption time.
4. Reads destination X/Y separately. Negative bytes preserve each sampled
   coordinate word; nonnegative bytes replace only its low byte. Changed
   high bytes survive. Row2's 255 therefore preserves sampled X, not a
   frozen trigger coordinate. Nominal small positive seeds give the table's
   ordinary destinations; this is not general whole-word normalization.
5. Replaces facing with PARAM5's byte, reads PARAM1 into d4, and RTS to
   MainLoop. An unchanged payload yields d0=36/71, d3=1/3, d4=0.

Facing is an outgoing operand, not a completed town-actor turn. Parameters
are reread around sound/idle/sampling, so they need not still match the
initially selected row. No-scroll itself neither writes destination
CURRENT_MAP nor constructs its population.

**This is the endpoint:** MainLoop's subsequent @Start would call SwitchMap
before CheckBattle/setup/init, but those destination paths are outside this
guide. The accepted switch rule can later substitute 71→78 under a fresh
F530 read; the posted south request remains 71. No castle admission, south
transition completion or usable destination control has been established.

## Changed states and retained review

| Changed premise or interruption | Required reading |
| --- | --- |
|530 changes between population/event/description reads|Current routing can disagree with retained sprites; no automatic relabel/rebuild.|
|Aliases, followers, HP/class, storage or mapping differs|Physical activation and first reverse alias govern; the row formula is conditional.|
|Camera, movement, desk/layout or mode differs|No guaranteed reach/activation; UP1 turns without restoration.|
|Actor missing, default selected or description unmatched|Distinct wrapper, CheckArea and field-menu outcomes.|
|Gold, capacity, equipment, deals, cancel or service interruption differs|Entry/catalog is not a completed resource transaction.|
|Church decline, Save, post-save continue/suspend differs|Only reached save branches write EGRESS_MAP/399; persistence stays bounded.|
|Description kind/coordinate/d6 differs|Layout priority and first admitted row precede displayed-text claims.|
|172/173 held, all-full or joined list differs|Repeat, member0/fallback and reached refill govern; no guaranteed delivery.|
|Inventory changes after capacity read or pickup is interrupted|Untested AddItem result and flag/item/presentation order are not atomic.|
|Exact north, wildcard north, south or no-match differs|Ordered row selection and pending preservation remain separate.|
|Negative bytes, high bytes or player motion differs|Source wildcard, consumption sampling and low-byte writes are distinct.|
|Pending replacement or F530 changes before next MainLoop|Mutable request and later switch do not promise one destination outcome.|

The pre-synthesis review consumed the complete accepted Pacalon research,
predecessor entry guide, narrower
[interaction](../../../../research/world/map-event-interaction-state.md),
[dialogue](../../../../research/world/map-event-dialogue-state.md),
[request writers](../../../../research/world/map-event-request-state.md),
[request consumers](../../../../research/world/map-event-request-consumption.md),
[Ketto shared service/pickup paths](../../../../research/chapters/northern-travel-mitula/ketto-interactions-and-merchant-relocation.md),
and the service/dialogue/description contracts. Selected public payloads,
their schemas and actual verifier consumers were inspected directly:

| Public fixture and consumer | Checked boundary and limit |
| --- | --- |
|`sf2-map-events-static-v1:entity-programs`; [entity-programs.json](../../../../../tests/fixtures/h2/map-events/entity-programs.json), [map_events.py](../../../../../src/sf2tool/h2/map_events.py)|All17 local bodies, ordered text operands, service alias targets and returns. Program bodies alone do not prove table offsets, natural activation or actual display.|
|`sf2-map-event-interaction-state-static-v1`; [map_event_interaction_state.py](../../../../../src/sf2tool/h2/map_event_interaction_state.py)|Dispatch/multiplexed facing-versus-item-Y inputs, actual acquisition/reverse-map/wrapper ranges. Aggregate counts do not prove Pacalon caller state.|
|`sf2-map-event-dialogue-state-static-v1`; [map_event_dialogue_state.py](../../../../../src/sf2tool/h2/map_event_dialogue_state.py)|Actual dispatcher lookup/prefill and reaching-definition consumers; selected-corpus evidence does not supply all local literals or natural presentation.|
|`sf2-map-event-request-state-static-v1` and `sf2-map-event-request-consumption-static-v1`; [request state](../../../../../src/sf2tool/h2/map_event_request_state.py), [consumption](../../../../../src/sf2tool/h2/map_event_request_consumption.py)|Local shop writes/alias handoffs and actual byte shop read, pending word read/clear/dispatch contexts. These bounded access shapes do not prove a completed service or enter the full warp callee.|
|`sf2-common-menus-static-v1`; [menus.py](../../../../../src/sf2tool/h2/menus.py)|Actual shop mutation/capacity/price guards and church Save operations/prompt polarity. Selected local catalogs and savepoint remain accepted research joins, not aggregate-menu facts.|
|`sf2-map-descriptions-static-v1`; [map_descriptions.py](../../../../../src/sf2tool/h2/map_descriptions.py)|Six-byte/terminator/first-match/text-base/d6 facts and actual consumer guards. Its public projection does not expose both local tables or decoded object layout.|
|`sf2-church-save-lifecycle-runtime-v1`; [church_save_lifecycle.py](../../../../../src/sf2tool/h3/church_save_lifecycle.py)|Five accepted controlled decline/continue/suspend cases and chronology. No Map02 natural save, durable storage or completed Witch lifecycle follows.|

The owning commands are `uv run sf2 h2 map-events`, `map-event-interaction-state`,
`map-event-dialogue-state`, `map-event-request-state`,
`map-event-request-consumption`, `common-menus`, `map-descriptions`, and
`uv run sf2 h3 church-save-lifecycle`. Their selected public surfaces were
reviewed; no private-source comparison or runtime command was rerun.
No blocking disagreement was found. A future contradiction belongs with
research before this guide can expand.

[#1021's frozen handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1021#issuecomment-6182880836)
and [independent acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1023#issuecomment-6182901348)
retain the clean pinned-source/table/literal/consumer review, author and
independent 0.091s results, prior 0.090s pass, exact-head CI38669951857 and
the corrected private-path regex/source-path/filename/category inspections.
The unchanged 18,285,979-byte pair's accepted 0.083s receipt remains reused by
provenance, not regenerated. All linked #1017/#1018 receipts, completed
failures and optional compressed-layout limits remain preserved.

[#1022's corrected frozen handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1022#issuecomment-6182928256)
and [PR1024 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1024#issuecomment-6182986240)
retain the Map71-versus-flag71 correction, exact final head 5dd17992/tree b78c4384/
base ac35fe19, configured 0.905s/independent 0.878s checks, final CI38670343968,
distinct earlier CI and original diagnostics. Private source and pair remain
unavailable in this design environment; no acquisition, copy, new comparison
or completed-suite reset follows.

This slice's Issue handoff records the complete prose/changed-state/five-path
review, selected schemas and consumer joins, links/anchors/encoding/private
checks, git diff --check, configured clean-head planner and actual exact-head
CI. Documentation scope skips blanket normal/full/Python/.NET/Godot/H3/H4
and translation suites. **Unknown:** natural reach/layout, supplied flags,
mapping/followers/HP/class/mode, inventory/gold/pending state, completed
services/pickups/travel, presentation/timing, usable control and persistence.
These limits do not create a runtime queue or advance remake acceptance.
