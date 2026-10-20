# Tristan entry, interactions and exits

Evidence date: 2026-10-20. **Confirmed:** the explicit Map15 seed admitted by
the [Battle27 predecessor](./battle27-outcomes-through-tristan-admission.md)
rebuilds exploration before calling a shared RTS init. F530 selects either
12 NPC declarations or an ally20 declaration plus ten NPCs, and also selects
their interaction and zone tables. The default visit offers nine conversations,
two shops, a church and three descriptions; it declares no collectible items.
Four warps request the caves and one requests North Parmecia. **Inferred:**
the source-named records describe Tristan and its connections. **Unknown:**
natural arrival, actual flags/actors/layout, completed services, usable control,
presentation/timing and persistence. This is static organization, not a claim
that Tristan runs in the remake.

## Agreement and provenance

Standard rigor for [Issue #1054](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1054)
under [data epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
one coherent town joins entry, source identities, interactions, changed state,
exit requests and current data consumers. Accepted base is
`13cce997c05db395017f6843deba8cec0e0edb83`, including #1041/PR1043.
This is the sole content owner; the predecessor gains only a navigation link.
No shared registry, schema, fixture, exporter, engine or design change is needed.
UI692 is unrelated and unconsumed. Research establishes original facts; the
remake survey below describes consumers only.

Begin at MainLoop's call into ExplorationLoop with explicit 15, nominal
`(31,30)`, Up1, d4=0. The main premise is F530 clear; preserve alternate-table
deltas and the gated Skreech callback frontier. Stop each exit after ordered
map/battle admission, before destination initialization or battle execution.
No collision-route search, later story execution, runtime acquisition, asset
export, generic service re-audit, Chinese or Layer B synthesis is included.

Source paths below are relative to `disasm/` at pinned
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The retained source is clean/read-only. Existing provenance binds USA retail
ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`;
this slice makes no new ROM/runtime observation. `sf2enums.asm`/`sf2const.asm`
own constants/RAM. Fnnn denotes flag nnn; indices are zero-based and address
ranges half-open. Town files are under `data/maps/entries/map15/`;
short `map/` and `entity/` paths mean `code/common/scripting/`.

Reuse the accepted 18,285,979-byte map-init/script pair and its 0.083s provenance,
not a new extraction. Selected source/record comparisons are linear in this
map's rows and callbacks, budgeted below one minute/512 MiB per direct check,
with small ignored receipts. Reassess contradictions or material support-tool
expansion. Unknowns do not authorize an emulator or bulk-data run.

## Entry construction and selected setup

**Confirmed:** the predecessor already resolves the incoming no-scroll warp,
consumption-time coordinate words and absence of a Map15 substitution/battle
row. Its nominal 31/30 bytes do not erase preserved high coordinate bytes.
This section starts after that admission, not at a guessed walked endpoint.

`code/gameflow/exploration/explorationfunctions_2.asm:ExplorationLoop` clears
pending MAP_EVENT_TYPE.w, subtracts 20000 from STEP_COUNTER.w with signed
negative-result clearing, heals living/immortal allies, requests fade-out and
temporarily selects view -1. The explicit-map arm writes CURRENT_MAP.b=15
and CURRENT_BATTLE.b=255, loads tilesets, waits for fade and clears VInts.
It selects setup entities, calls InitializeMapEntities and LoadEntityMapsprites,
clears F256–383 and sets 80. Sprite counters, view 0, full LoadMap with saved 15
and view coordinate -1, base VInts and RunMapSetupInitFunction follow in order.
Music/fade-in depend on the palette comparison; SetMoveSfx and WaitForEvent
follow. An RTS init removes none of this caller work.

The [shared entry owner](./battle26-return-through-pacalon-entry.md)
and [party/follower owner](./ribble-may-through-battle10-admission.md) retain
healing, allocation and raft details. Healing scans 30 allies: Peter7/Lemon28
always, others with nonzero current HP; eligible actors receive max HP/MP,
retain only already-present STUN/POISON/CURSE and recompute stats. This is not
revival of every dead ally. Entry does not clear persistent F530, F724, F820,
membership/follower flags, F399 or EGRESS_MAP. PLAYER_TYPE is supplied state,
not normalized by the town init.

`data/maps/entries.asm:Map15` references blocks/layout and all ten map-resource
slots. `00-tilesets.asm` at `$A0594..$A059A` declares palette 0 and tilesets
1,17,45,54,59. The sole area at `$A05C2..$A05E2` has main rectangle
`(0,0)..(36,31)`, secondary foreground origin `(0,32)`, background `(0,0)`,
parallax256/256 on both layers, zero autoscroll, layer type 0 and MUSIC_TOWN.
LoadMap selects by actual viewed-actor coordinates and falls back to the first
area when no rectangle matches. These declarations supply no decoded collision
route, visible doorway or successful rendering claim.

`data/maps/mapsetups.asm` orders `msMap 15,ms_map15`, then
`msFlag 530,ms_map15_flag530`. GetCurrentMapSetup in
`map/mapsetupsfunctions_1.asm` rereads CURRENT_MAP and flags each time.
The retained map-init `routeJoins` sourceOrder30/31 and `pointerTableJoins`
sourceOrder33/34 join these pointers:

| Setup / address | Entity / entity-event / zone tables | Shared descriptions / items / init |
| --- | --- | --- |
|ms_map15 /`$5CDFE..$5CE16`|ms_map15_Entities /ms_map15_EntityEvents /ms_map15_ZoneEvents|ms_map15_AreaDescriptions /ms_map15_Section5 /ms_map15_InitFunction|
|ms_map15_flag530 /`$5CE16..$5CE2E`|ms_map15_flag530_Entities /ms_map15_flag530_EntityEvents /ms_map15_flag530_ZoneEvents|same three targets|

`s6_initfunction.asm` at `$5D04C..$5D04E` is exactly RTS for both selections.
Population construction, init and later callbacks do not share a frozen F530
snapshot. A flag change selects different callbacks against retained actors;
it does not itself rebuild, move or relabel them.

After init, WaitForEvent reads pending type before input. Zero installs main
control and view 0; the actual numeric selector installs character control for 0,
eas_Raft for 1 and eas_Caravan for other nonzero values. This differs from the
raft **construction** branch's enum-valued 2 test. Nonzero pending state bypasses
control installation and is dispatched first. Actor/VInt work may progress
before this conditional boundary; completed services/control remain Unknown.

## Default population and conversations

**Confirmed:** `s1_entities.asm`, `$5CE2E..$5CE90`, has 12 eight-byte rows plus
a sentinel. All are regular fixed-sprite NPCs, not ally selectors. Map15 is
neither overworld nor follower-excluded: ordinary flag-followers allocate
first, with no F65 Caravan prefix. The complete ordered follower list remains
in the [entry owner](./battle26-return-through-pacalon-entry.md#conditional-map02-population-and-first-eventcontrol-read).
Flags admit those rows independently of membership/HP; duplicate aliases can
allocate extra actors and overwrite lookup entries. No row there names Skreech20.

For n actual follower declarations, source row j below becomes physical
`n+1+j`, logical `128+j`, lookup `32+j`, subject to successful bounded allocation.
No-followers gives physical 1–12. ClearEntities removes prior actors/mappings/
walking scratch; main physical 0 is declared afterward from returned coordinate
words/facing, with Bowie's class/HP sprite and eas_Idle. Main is not the first
active battle member. Arbitrary mass flag enables have no capacity repair.
Raft construction still consumes supplied numeric mode/64/RAFT_MAP: mode 2
replaces main's sprite, stands/hides followers and declares raft31 at 64,64;
other modes use the registered raft-map gate. It does not reset PLAYER_TYPE.

Sprite abbreviations below have MAPSPRITE_ prefix. Fixed rows use eas_Init;
`walk r` uses the listed X,Y as origin and range r. Mask1 turns toward the
player; mask 3 also restores saved facing on callback return.

| j / logical | X,Y / facing / sprite | Motion | Event / mask | Text IDs and meaning, or service |
| --- | --- | --- | --- | --- |
|0 /128|27,27 /Down /DUCK218|walk1|Event0 /1|1979→1980: Mitula rules; no king|
|1 /129|15,27 /Down /BEASTMAN1 220|walk1|Event1 /1|1981→1982: confidence against devils|
|2 /130|29,13 /Left /BEASTMAN2 222|fixed|Event2 /3|1983→1984: shrine beyond the caverns|
|3 /131|20,14 /Down /DWARF226|fixed|Event3 /1|1985→1986: shrine storytellers and legends|
|4 /132|26,15 /Down /KAPPA221|walk2|Event4 /1|1987→1988: drought and confidence in Mitula|
|5 /133|12,26 /Up /ELF_FEMALE224|fixed|Event5 /3|1989→1990: welcome to visitors|
|6 /134|6,16 /Down /ELF_MALE223|walk2|Event6 /1|1991→1992: missing shrine visitors|
|7 /135|28,6 /Down /WARRIOR225|walk2|Event7 /1|1993→1994: weather and nature magic|
|8 /136|31,13 /Up /FAIRY219|walk1|Event8 /1|1995→1996: Ground Orb at Mitula's statue|
|9 /137|3,23 /Down /MERCHANT193|fixed|Event9 /1|item shop index26|
|10 /138|12,15 /Down /PRIEST192|fixed|Event10 /3|j_ChurchMenu|
|11 /139|2,23 /Down /MERCHANT193|fixed|Event11 /3|weapon shop index11|

The six walking rows copy 50 bytes each into walking scratch and patch
origin/range. Their random-step action loop depends on live layout, obstruction
and destinations; origin/range does not prove a route or immutable clamp.
eas_Init sets speed 32/32, motion/collision flags, Up orientation, size 24 and
sprite update before eas_Idle. Orientation is distinct from declared facing.

`s2_entityevents.asm`, `$5CEEA..$5CF9E`, has 12 four-byte named rows plus
the $FD default. Event names in the table mean `Map15_EntityEventN`.
The default Map15_DefaultEntityEvent is RTS with mask 0.
`map-events/routing-setup.json` and `entity-programs.json` retain exact table
identities and callback bodies, including alternate callbacks below.

Ordinary C activation follows its debug/Caravan alternatives. Reuse
[interaction state](./map-event-interaction-state.md),
[dialogue state](./map-event-dialogue-state.md) and the
[Pacalon activation/wrapper owner](./pacalon-interactions-and-castle-handoff.md):
GetActivatedEntity scans physical actors near the tile ahead, steps through
counter/desk layout and excludes viewed/follower actors. Reverse lookup selects
the first alias or 0 on no match. RunMapSetupEntityEvent selects the current
setup, stores player facing in EVENT_RELATIVE_POSITION, and prefills portrait/
speech from the mapped actor's actual sprite. UP1/DOWN3 in the event table are
action masks, not required player approach directions. Bit0 waits a VInt and
faces `(playerFacing+2)&3`; bit1 restores the saved facing after return.
The wrapper suppresses VInt_UpdateEntities before the callback, closes portrait/
text afterward and activates it again. Even default RTS retains wrapper work.

All default dialogue pairs join decimal 1979–1996 to the actual hexadecimal
keys in `data/scripting/text/gamescript.txt`. Each first line contains W2,
each second W1, with N line breaks where declared. These are text/input
continuations, not story flags or automatic passage. The nine callbacks
contain only their two txt calls and RTS: no local quest mutation, item grant
or map request. Sprite dialogue properties, actual alias/class/HP and fallback
still govern portrait/bleep; a prose NPC label does not fix live presentation.
Incomplete display or return retains its reached prefix, not an atomic visit.

## Shops, church, searches and map callbacks

**Confirmed:** Event9 writes CURRENT_SHOP_INDEX.b=SHOP_ITEM_TRISTAN (26) before
j_ShopMenu; Event11 similarly writes SHOP_WEAPON_TRISTAN (11). Event10 calls
j_ChurchMenu directly, with no local warning, quest gate or heal. The shop
consumer `code/common/menus/shop/shopactions.asm:GetShopInventoryAddress`
skips exactly the selected count of length-prefixed rows; comments are one-based
within each shop category, while these selectors are zero-based overall.

| Selector / physical row comment | Ordered stock, ITEM_ prefix |
| --- | --- |
|11 /Weapon shop12|BUSTER_SWORD, GREAT_SWORD, JAVELIN, CHROME_LANCE, LARGE_AXE, GREAT_AXE, ASSAULT_SHELL, GREAT_SHOT, GUARDIAN_STAFF, INDRA_STAFF, POWER_GLOVE, BRASS_KNUCKLES|
|26 /Item shop12|MEDICAL_HERB, HEALING_SEED, HEALING_DROP, ANTIDOTE, FAIRY_POWDER, ANGEL_WING|

These are actual `data/stats/items/shopinventories.asm` rows; stock plus
invocation does not prove an affordable or completed transaction. The
[service contract](../design/contracts/service-interactions.md) and
[existing service caller owner](./ketto-interactions-and-merchant-relocation.md#shops-priest-and-reload-lifetime)
retain buy/deals/sell/repair, church raise/cure/promote/save, cancellation and
ordered effects. Church entry alone leaves EGRESS_MAP unchanged. Its accepted
Save path, after the initial zero response, copies CURRENT_MAP to EGRESS_MAP,
sets 399 and calls SaveGame with CURRENT_SAVE_SLOT; here that map is 15.
`data/maps/global/savepointmapcoords.asm` declares 15 `(12,16)`, Up.
The savepoint consumer requires 399 for table selection; this is a return datum,
not a town-entry teleport or proof of saved persistence. The reached path
retains payment/mutation/save prefixes if later service work does not return.

`s4_descriptions.asm` at `$5D020..$5D044` supplies a 16-byte wrapper,
base `$1041=4161`, three six-byte rows at $5D030 and $FD00 termination.
Every row is condition 0/kind 0; no local function or d6=0 gate is selected.

| Row / target X,Y | Investigation offset →ID | Description offset →ID / meaning |
| --- | --- | --- |
|0 /31,3|9 →432|0 →4161 /Mitula statue|
|1 /2,27|2 →425|1 →4162 /weapons and items welcome sign|
|2 /22,14|4 →427|2 →4163 /Mitula's Code book|

The [description contract](../design/contracts/map-area-description-routing.md)
owns exact matching/arithmetic. CheckArea uses the tile ahead, clears name
slot 1 and speech SFX, and prioritizes chest/vase/barrel/bookshelf/area-item
layout kinds before setup descriptions. Ordinary no-entity C supplies d6=1.
Only when this path reaches DisplayAreaDescription does packed X/Y first-match
emit 423+investigation then 4161+description, close windows and return d7=-1;
no match returns 0 for caller fallback. Investigation425/427/432 has NAME,
N,W2,CLEAR; NAME resolves the actual member0 name in this caller. Selected
description literals have N as declared and end W1. A coordinate table alone
does not prove which search branch wins in a live layout.

`7-chest-items.asm` `$A0652..$A0654` and `8-other-items.asm`
`$A0654..$A0656` each contain only endWord: no declared pickup or acquisition
flag. Shared OpenChest/CheckNonChestItem still perform their own search/fallback
when reached. This does not disable searching or prove no searchable block.
`s5_itemevents.asm` `$5D044..$5D04C` has only the default item-event record
to Map15_DefaultItemEvent0/RTS, shared across 530. RunMapSetupItemEvent still
masks the item, clears speech, stores relative position, selects current setup,
invokes the default, closes windows, waits a VInt and main motion. No local
item match, consumption or successful field-item effect follows from this stub.

The default zone table `$5CFFA..$5CFFE` likewise contains only its default
RTS target. A type 6 event first applies main's init actscript, then reads pending
X from PARAM_1 and Y from PARAM_3. RunMapSetupZoneEvent selects current setup,
calls the matching/default callback, closes windows, waits a VInt and waits
for main motion. Entry does not invoke this callback merely because coordinates
are supplied. Candidate kind$1400 and the mutable request slot retain the
[request-state](./map-event-request-state.md) and
[request-consumption](./map-event-request-consumption.md) boundaries.

The flag-block table `$A05E2..$A05E4` is empty. The four step and four roof
records remain separate from zone callbacks:

| Door trigger | Step source / size / destination | Roof source / size / destination |
| --- | --- | --- |
|21,17|39,0 /1×1 /21,17|255,255 /6×5 /18,44|
|12,18|39,0 /1×1 /12,18|255,255 /5×5 /10,45|
|3,27|39,0 /1×1 /3,27|255,255 /5×6 /1,53|
|11,28|39,0 /1×1 /11,28|255,255 /6×5 /8,55|

Step rows at `$A05E4..$A0606` feed exploration.asm:OpenDoor, which requires
CURRENT_BATTLE=255 and copies the selected layout block before door SFX/plane
update. Roof rows at `$A0606..$A0628` feed PerformMapBlockCopyScript and
ToggleRoofOnMapLoad; signed negative source Y selects clearing rather than
reading coordinate255,255. Their save/restore/toggle services depend on live
roof state. Map load also tests the actor against secondary-layer-adjusted
roof rectangles. These are mutable layout operations, not a transfer or a
new story-flag producer, and do not establish collision reachability.

## F530 alternate population and event frontier

**Confirmed:** `s1_entities_530.asm`, `$5CE90..$5CEEA`, has 11 eight-byte
rows plus sentinel. Row 0 is ally selector 20/Skreech, not NPC128. If lookup 20
already exists, InitializeMapEntities consumes its action longword but skips
allocation; otherwise it declares 20 without testing joined20. Its actual
sprite follows ally class/HP rules. Remaining rows consume the NPC cursor:
with n followers and a=1 if 20 allocates,0 if deduplicated, NPC j is physical
`n+1+a+j`, logical 128+j/lookup 32+j. No ordinary follower row declares 20.
Joined 20 or held 820 does not itself hide this source row; the shared init is RTS.

| Source row / logical | X,Y / facing / sprite | Motion | Selected callback / mask |
| --- | --- | --- | --- |
|0 /20|63,63 /Up /ally20|eas_Init|no 20 row; default RTS /0|
|1 /128|27,28 /Down /DUCK|walk1|Event0 /1, same1979→1980|
|2 /129|14,28 /Down /BEASTMAN1|walk1|212_Event1 /1,1997→1998: Zeon threat|
|3 /130|31,12 /Left /BEASTMAN2|eas_Init|212_Event2 /3,1999→2000: stolen spirit/vampires|
|4 /131|20,14 /Down /DWARF|eas_Init|212_Event3 /1,2001→2002: purging the devil|
|5 /132|22,19 /Down /KAPPA|walk1|212_Event4 /1,2003: ship in western sky|
|6 /133|10,25 /Down /ELF_FEMALE|eas_Init|212_Event5 /3,2004: thanks for Mitula/Pacalon|
|7 /134|15,6 /Down /WARRIOR|walk2|212_Event6 /1,2005: Moun gate closed|
|8 /135|3,23 /Down /MERCHANT|walk1|Event9 /1, item26|
|9 /136|12,15 /Down /PRIEST|eas_Init|Event10 /1, church|
|10 /137|2,23 /Down /MERCHANT|eas_Init|Event11 /3, weapon11|

The source's row 8 is msWalkingEntity, unlike the default item merchant;
five alternate walkers use 250 scratch bytes. The elf male and fairy rows
disappear; warrior and service logical indices shift. Alternate entity table
`$5CF9E..$5CFFA` reuses Event0/9/10/11/default by signed relative offsets;
it does not map NPC128 to the first **source** row. The alternate priest mask
is 1, so it does not restore facing as default 138/mask 3 does. Paired new dialogue
ends W2 then W1; singles 2003–2005 end W1. Descriptions/items/warps do not change.

`s3_zoneevents_530.asm`, `$5CFFE..$5D020`, adds `(24,18)` before the shared
default. Map15_212_ZoneEvent0 returns immediately when 724 is clear or 820 set.
Only 724 set/820 clear calls `script cs_5D04E`; **after that call returns** it
sets 820. F724 is the accepted Bedoe fall/rescue flag, not joined20. F820 is
outside the temporary clear range and is not reset by this entry.

`scripts.asm:$5D04E..$5D1CA` names a substantial Skreech flight/conversation
program, text cursor 2006, later join 20 and follower changes. Discovery of that
target is necessary to bound the zone callback; its complete execution,
dialogue 2006 onward, flight path and join-service outcome are deferred.
If the script does not return, the callback has not reached set 820; its already
reached actor/scene/party prefix is not rolled back by this later instruction.
The gate does not prove natural reach, usable 20, a completed join, or persistence.

Changing 530 after construction can, for example, send default warrior 135 to
the item-shop callback, default fairy 136 to church, and default priest 138 to
default RTS. The reverse change can give alternate priest 136 the Ground Orb
conversation. Physical positions, sprites and aliases remain as supplied.
Do not silently align actors to the currently selected table by row number.

## Exit requests and ordered destination admission

**Confirmed:** `6-warp-events.asm`, `$A0628..$A0652`, has five ordered
eight-byte rows plus endWord. All request no-scroll; no F530 gate is present.
Facing numbers are Up1/Down3.

| Row / source X,Y | Requested destination / nominal X,Y / facing |
| --- | --- |
|0 /18,5|Tristan caves35 /15,30 /Up|
|1 /19,5|Tristan caves35 /16,30 /Up|
|2 /4,9|Tristan caves35 /24,15 /Up|
|3 /7,2|Tristan caves35 /4,14 /Up|
|4 /255,31|North Parmecia74 /52,21 /Down|

Source 255 is signed wildcard X, not a requirement to stand at X255. Walking
activation still depends on the actual candidate layout kind/caller state;
no route or collision search was performed. The request owners and
[predecessor warp join](./battle27-outcomes-through-tristan-admission.md)
retain candidate production, type-before-parameter writes, first-match order
and no-match preservation of an older pending request. The single mutable
slot is not a queue or an immutable saved route.

On consumption ProcessMapEvent clears type before dispatch. The no-scroll
handler handles/clears WARP_SFX, idles main and unwinds to MainLoop. It clears
d0–d4 words, reads destination map, then samples main's **current** signed
position words divided by 384. Nonnegative destination bytes replace only
the low coordinate bytes; negative bytes preserve the sampled word. Every
local destination byte is nonnegative, so its high byte can still be retained.
Facing is the row byte and d4=0. A changed slot or actor between production
and consumption changes the actual result; the table does not restore them.

MainLoop@Start orders SwitchMap then CheckBattle, not the battle-return
alt_MainLoopEntry. The full flag-switched map table has no 35 or 74 source row.
All 45 battle-coordinate rows have no 35, so an intact explicit 35 request
reaches `j_ExplorationLoop` with d7=-1. Stop there before cave setup/init.

For 74, the full battle table has matching rows 27 then 32, both crop
`(2,16,32,48)` and wildcard trigger X/Y. Held 427 selects 27; otherwise held 432
can select 32; if neither is held, no row matches and exploration is selected.
On a match CheckBattle writes that crop, checks completion 527/532 and clears
the corresponding unlock if completed, **while still returning the selected
battle**. Completion does not skip to the later row in this call. Other-map
unlocks do not match 74. Thus held 427 can reselect 27 even with 527 set; clear 427
and held 432 can select 32 even with 532 set. Stop at the resulting BattleLoop
call or ExplorationLoop call, before either body. No cave/shrine/battle story
progression or unconditional peaceful return is established.

## Consumption readiness at the accepted base

**Confirmed about repository consumers:** existing research shapes and current
production consumers are different boundaries. H2 routing/program shards have
Map15 rows; they are evidence, not runtime packages. No Tristan package or
Map15-specific admission/observation is claimed by this survey. The
[capability owner](../../remake/docs/capability-status.md) remains authoritative
for implemented/Unsupported boundaries.

| Data category | Existing representation and actual consumer | Remaining bounded gap |
| --- | --- | --- |
|Setup/init and identities|Retained map-init route/pointer/body joins; `map-events/routing-setup.json` and entity/zone/item program shards, with their `schemas/h2/map-events-*.schema.json` owners, express the selected original facts.|No new evidence format needed. They do not constitute a Content package or runtime proof.|
|Area/layout/doors/roofs|`ExplorationMapDefinition`, `ExplorationViewArea`, `ExplorationLayoutEvents` in [ExplorationDefinition](../../remake/src/Sf2.Remake.Application/Content/Scenarios/ExplorationDefinition.cs); [Content reader](../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs) and [MapEventDispatcher](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapEventDispatcher.cs) consume prepared layouts, areas and block copies.|Directly expressible categories; selected private Map15 layout/visual/material bindings and reach validation have not been prepared or run. Metadata alone is insufficient.|
|Population, facing, walking, ordinary text|ExplorationPopulation, ExplorationEntityDefinition and ExplorationEvent, [StoryProgram](../../remake/src/Sf2.Remake.Application/Content/Scenarios/StoryProgram.cs), Content token parsing and [ProgramRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs) support flags, calls/returns, entity actions, text cursor/display/waits. [SceneEntities](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/SceneEntities.cs) owns allocation.|Directly expressible subsets need source-slot/alias/sprite/action/text bindings. Existing interaction admission is adjacent/facing by EntityRef; original counter/desk scanning and reverse alias selection cannot be inferred from that.|
|F530 alternatives and callback masks|MapSetupRoute/SelectSetup, setup definitions, BranchFlag and EntityFlags represent alternatives and masks1/3. [MapTransfer](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs) selects setups at load/transfer; interaction reads the loaded definition.|A bounded mapping/support review must preserve original live setup reselection without silently rebuilding population, including indices 135–139 and changed priest mask. Shape availability does not prove that mixed state is supported.|
|Shops/church/save|Accepted [service contract](../design/contracts/service-interactions.md), common-menus fixture/schema and savepoint source own original selectors/effects.|No ShopMenu/ChurchMenu service instruction or service UI is provided by the selected StoryProgram/reader switch. Unknown native calls remain UnsupportedInstruction and stop in ProgramRunner. Stock, transactions, church/save/persistence need explicit support; text or SetEgressMap alone cannot replace them.|
|Search, descriptions and pickups|`map-descriptions-static-v1.json` and its schema/consumer owner express matching rules; original local rows are checked directly. Empty pickup tables need no invented acquisition rows.|Capability owner marks item acquisition/area descriptions Unsupported. ExplorationEventKind has Step/Interact/Warp/SourceZone, with no area-search or pickup dispatch; a bounded original search-priority/description consumer is needed.|
|Warp requests and main-loop scans|Prepared Warp/SourceZone events, QueueMapWarp/TransferToMap and MapTransfer/BattleEntry consume explicit destination and battle routes.|Directly expressible destination/flag subsets require ordered 35/74 map and 27/32 admission bindings, source sentinel/coordinate-width review and actual reached consumers. No destination init or battle integration is delivered here.|
|Alternate Skreech callback|The accepted zone-program fixture records 724/820 polarity, script target and post-return 820; general flag/call/join/motion instructions exist.|Only the callback frontier is organized here. Complete scene lowering, affected join/follower/flight consumers and an actual reached observation remain unassessed. Do not mark recruitment ready.|

The existing preparer [remake_exploration_maps.py](../../src/sf2tool/remake_exploration_maps.py)
already lowers selected entities, entity/zone events, warps and layout copies;
[remake_exploration_content.py](../../src/sf2tool/remake_exploration_content.py)
registers selected sources and calls the compiler. It does not automatically
import Map15 into the current world. It registers entity/zone/init/scripts,
not a complete description/item/service importer. The
[PrivateExplorationReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateExplorationReader.cs)
admits prepared provenance-bound world/start documents; runtime does not read
research fixtures or disassembly on demand. No preparer or engine was run here.

## Reproduction and remaining boundaries

Load `local/private-inputs.ps1` in the launching process. Reuse the retained
pair and the source pin above; do not regenerate assets to reproduce this text.
The ignored `local/issue1054/review.py` receipt recipe runs with:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1054/review.py <configured-source-disasm-root>
git diff --check
```

It compares both complete populations, all five table/category joins, selected
entity/zone/item callback operations against existing fixtures, their existing
schemas, route/pointer/RTS records, descriptions/text IDs/control endings, stock,
block-copy/warp rows and actual caller/request/admission order. It also checks
the concrete reader/definition/runtime boundaries named above, UTF-8/LF, links,
owned paths and private boundaries; it creates no maintained verification layer.
The original source and reused pair remain unchanged. This source/fixture check
does not freshly establish H1/ROM equality, timing or runtime behavior.

After commit use `uv run sf2 verify plan --base origin/main --head HEAD` and
retain actual exact-head scoped CI in the Issue/PR handoff. Documentation scope
does not launch normal/full Python, .NET, Godot, H3 or H4 suites. The predecessor's
completed failures/corrections and the twelve unrelated private comparisons
remain preserved in their existing handoffs; none is relabeled or rerun.

**Unknown:** actual incoming words/mode/pending state, selected flags and
interleaving, live mappings/capacity/HP/class, natural approach or collision
route, completion of text/shop/church/layout/script services, resulting usable
control, hardware presentation/timing and persistence. Deferred scene execution
and every destination's init/battle body remain outside this town slice.
