# Elven Village Conversations and Local Exploration

Elven Village offers conversations about a fairy and Elric, searchable books and items, a priest,
and an internal stair pair. Those opportunities have different consequences. The residents' selected
dialogue bodies read or describe story state; they do not recruit Elric or open a route to Creed.
An admitted item search can change inventory and a collection flag. The stairs move within the
current map while retaining its existing residents and working layout.

**Confirmed:** the accepted evidence establishes these static selections and immediate consumers.
**Inferred:** they form an optional village conversation and exploration segment.
**Unknown:** a natural itinerary, actual caller state, successful services, completed presentation
and responsive input. The coordinates below identify content and conditional triggers, not a
walkthrough or a reconstructed terrain layout.

This implementation-neutral English guide consumes the accepted
[local research](../../research/elven-village-conversations-and-local-exploration.md) and
[arrival guide](battle19-outcomes-through-elven-village-entry.md). Under
[Issue #863](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/863), Standard rigor is
appropriate because interaction, inventory, request consumption and map reload have separate owners.
Stop at an admitted church call and conditional return, or conditional Map23 control after a local
conversation, search or stair. External exits, Map24/Battle20, Fairy Cave/Creed travel, Elric's rescue
or recruitment, the F762 producer, item use/promotion and church transactions are excluded.
This guide selects no remake rule, runtime acquisition or Chinese synchronization.

## Choosing a Conversation

**Confirmed:** Map23 selects `ms_map23` by default, or `ms_map23_flag762` when supplied F762 is set.
Only entities and entity-event targets differ between these setups; zone, description, item and init
targets are shared. F13 selects one conversation branch, not a population. A fresh setup gives its
NPCs logical IDs starting at 128, independently of the physical slots occupied after followers.

Ordinary `ProcessPlayerAction` idles main entity 0 and waits for movement/view-scroll completion.
A takes the field-menu path. C reaches interaction after the separate debug and overlapping-Caravan
branches. `GetActivatedEntity` requires a nonnegative view target and computes a candidate from its
facing. Desk/counter blocks can extend that candidate. The physical scan skips the viewed entity and
current followers, then accepts the first entry at Manhattan distance below 384 internal units from
the candidate. An initial NPC coordinate therefore does not prove that a particular approach works.

`GetEntityEventIndex` resolves the physical entry through `ENTITY_INDEX_LIST`; a missing match falls
back to 0. `RunMapSetupEntityEvent` selects the first matching logical-ID row, otherwise its `$FD`
default. It stores the player's supplied facing in `EVENT_RELATIVE_POSITION`, obtains portrait/SFX
state through `GetEntityPortaitAndSpeechSfx`, and calls the handler with entity updates suspended.
Actual actor mappings, portraits, speech and action completion remain conditional.

### Row Action Bits Are Not Approach Directions

The second `msEntityEvent` operand is an action bitfield, although the macro comment calls it facing
and the source uses direction-shaped names. UP is byte 1; DOWN is byte 3:

| Row byte | Selected action |
| --- | --- |
| UP /1 | Bit 0 turns the actor to `(playerFacing+2)&3` after a VInt; bit 1 is clear, so the dispatcher does not restore the saved facing. |
| DOWN /3 | Bit 0 turns the actor the same way; bit 1 restores the saved facing after handler return. |
| Default /0 | Neither turn nor restore is requested by those bits. |

These values do not require Bowie to approach from Up or Down, and do not replace the NPC's initial
facing. Returning paths close portrait/text and resume entity updates; the priest's path reaches
that cleanup only if the church call returns.

### Default Residents and Their Literal Dialogue

**Confirmed:** the following rows join the default population to `Map23_EntityEventN` in
`s2_entityevents.asm`. Placement and facing describe initial rows, not permanent positions.
Text below paraphrases the accepted annotations; it is not observed displayed dialogue.

| Logical actor / initial placement | Action byte / handler | Ordered content |
| --- | --- | --- |
| 128 MAN, `(18,9)` Down, walking range 1 | UP /1, Event 0 | 1690 welcomes travelers who stop to rest (`{N}`, `{W2}`); 1691 permits a stay as long as desired (`{N}`, `{W1}`). |
| 129 DWARF, `(17,11)` Right, fixed | DOWN /3, Event 1 | 1692 says a fairy lives nearby and asks whether the listener knows her (`{N}`, `{N}`, `{W1}`). |
| 130 OLD_MAN, `(18,11)` Left, fixed | DOWN /3, Event 2 | 1693 asks about the fairy (`{W2}`); 1694 says she has not returned from Creed's mansion (`{N}`, `{W1}`). |
| 131 WORKER, `(54,3)` Down, fixed | UP /1, Event 3 | 1695 says only the fairy knows the way to Creed Mansion (`{N}`, `{N}`, `{W2}`); 1696 suggests her friend `{NAME;13}` may know (`{NAME;13}`, `{N}`, `{N}`, `{W1}`). |
| 132 ELF_FEMALE, `(8,12)` Right, walking range 1 | UP /1, Event 4 | Reads F13 and chooses the two-line branch below. |
| 133 PRIEST, `(61,3)` Down, fixed | UP /1, Event 5 | Calls `j_ChurchMenu`, then returns if the service returns; no local text sequence. |

Events 0–3 are their listed text calls followed by return. The dwarf's question is text, not a
`YesNoPrompt` or a choice whose answer changes a flag. References to the fairy, Creed or Elric do
not admit travel, demonstrate those events, or create a story mutation.

### F13 Changes the Elf's Words, Not Elric's Membership

**Confirmed:** Event 4 executes `chkFlg 13` and `bne.s byte_59B94`. Clear falls through to 1697/1698
and branches to the return; set selects 1699/1700:

| Supplied F13 | Ordered content |
| --- | --- |
| Clear | 1697 says `{NAME;13}` is away seeking the fairy to help a sick dwarf (`{NAME;13}`, `{N}`, `{N}`, `{W2}`); 1698 warns that the western forest is dangerous (`{N}`, `{W1}`). |
| Set | 1699 describes a forest elf repaying kindness (`{N}`, `{W2}`); 1700 tells `{LEADER}` that `{NAME;13}` will help (`{LEADER}`, `{N}`, `{NAME;13}`, `{W1}`). |

Ally 13 is Elric and F13 is his joined flag in the accepted common-stats source join. This handler
only reads it. It does not join Elric, set F13, restore the fairy, heal the dwarf or write a map
request. The substitutions need supplied name state. `{N}`, `{W1}` and `{W2}` retain their control
identities; this segment establishes neither frame durations nor completed button advances.

### Supplied F762 Residents

**Confirmed:** `s2_entityevents_762.asm` binds four alternate population rows. Its three text bodies
return without testing F13 or mutating membership, flags or requests.

| Logical actor / initial placement | Action byte / handler | Ordered content |
| --- | --- | --- |
| 128 MAN, `(18,9)` Down, walking range 1 | UP /1, `Map23_2FA_EntityEvent0` | 1701 asks whether the listener met Creed (`{N}`, `{W1}`). |
| 129 WORKER, `(56,6)` Down, walking range 1 | UP /1, alternate Event 1 | 1702 says evil has lifted completely from the forest (`{N}`, `{W1}`). |
| 130 OLD_MAN, `(18,11)` Up, fixed | DOWN /3, alternate Event 2 | 1703 asks whether the sick dwarf is better (`{W2}`); 1704 asks whether he can now see the fairy (`{W1}`). |
| 131 PRIEST, `(61,3)` Down, fixed | UP /1, default Event 5 | The same church call and conditional return. |

These statements are speaker content, not evidence that Creed was visited or the dwarf recovered.
The F762 producer is outside scope. Dispatch resolves supplied setup state again; an existing
population can survive a current-map stair without being rebuilt to match changed flags. Logical 131
is the default worker but the alternate priest. Use the admitted logical/physical mapping rather
than assuming a sprite name or old setup identifies the handler.

`Map23_DefaultEntityEvent` is only `rts` and is shared by the alternate table. Both setups also share
return-only `Map23_DefaultZoneEvent` and `Map23_DefaultItemEvent0`, with no selected specific rows in
those two tables. The item-use default is a different dispatcher from chest/non-chest search: it
neither grants the Secret Book nor proves that downstream generic item use has no effect.

The priest's alias join is `j_ChurchMenu` at `0x20004` to `ChurchMenu` at `0x20A02` in
`code/common/menus/church/churchactions_1.asm`. **Unknown:** service return, savepoint changes,
healing, revival, promotion, prices and completed transactions. This guide stops at the call and
conditional return, without importing those service outcomes.

## Search Chooses a Kind Before a Description

**Confirmed:** C tries an entity first; an admitted entity handler consumes that interaction without
searching. With no activated entity, it calls `CheckArea` with d6=1. The FieldMenu search action
instead reaches `j_CheckArea` with d6=0 independently of that entity lookup.

Search requires a nonnegative view target, clears `DIALOGUE_NAME_INDEX_1` and speech SFX, and targets
one faced tile using the direction offsets. It divides coordinates by 384, indexes the 64-wide
working layout and masks its word with `$3C00`. It does not repeat through desk/counter blocks as
NPC activation does. The actual view target, pose and working-layout kind are required inputs.

| Kind after masking / dispatch order | Consumer | Initial / empty text IDs |
| --- | --- | --- |
| Chest `$1800` | `OpenChest` | 403 /408 |
| Vase `$2C00` | `CheckNonChestItem` | 404 /409 |
| Barrel `$3000` | `CheckNonChestItem` | 405 /410 |
| Bookshelf `$3400` | `CheckNonChestItem` | 427 /412 |
| Searchable area `$1C00` | `CheckNonChestItem` | 423 /412 |
| Any other masked value | `j_RunMapSetupAreaDescription` | Matching description, if any. |

Every item-kind branch ends through item handoff or its empty result and common closure. None falls
through to descriptions. A handled description returns d0=-1. With no description match, d6=1
returns zero so the ordinary C caller can open its menu; d6=0 emits 423 then 412 and returns -1.
The older area-description contract's sole-call discussion covers its no-entity d6=1 path;
the later field-search owner explicitly records FieldMenu's ordinary d6=0 caller to `CheckArea`.
That static caller join does not establish natural execution of conditioned callbacks.

### The Three Description Rows

**Confirmed:** shared `s4_descriptions.asm` uses description base `$1015` /4117 and table `byte_59BE8`.
Each six-byte `msDesc` row has zero condition and function-kind bytes, investigation offset 4 and
the following description offset. There is no d6 restriction or callback on these three rows.

| Coordinate | Offset / text ID | Literal subject |
| --- | --- | --- |
| `(5,20)` | 0 /4117 | A book titled AN APPLE A DAY. |
| `(51,2)` | 1 /4118 | A book titled HOME-MADE POTIONS. |
| `(52,2)` | 2 /4119 | A book titled FAIRIES. |

`DisplayAreaDescription` takes the first packed X/Y match and emits investigation `423+4=427`
(`{NAME}`, `{N}`, `{W2}`, `{CLEAR}`), then `4117+offset` (each title has `{N}`, `{W1}`). It closes
portrait/text and returns d7=-1; no match leaves d7=0. The wrapper tests d7 before restoring registers,
preserving the condition codes used by `CheckArea`. These are callable content rows, not guaranteed
results whenever the coordinates are searched.

### Why `(52,2)` Has Two Rows but One Selected Result

At `(52,2)`, an admitted bookshelf-kind search chooses the other-item table. With F158 clear it
selects SECRET_BOOK; with F158 set it reports 427 then 412. Full inventories take the rollback path
below. None displays 4119 on that call or by an automatic retry. The other non-chest item kinds have
the same precedence, with their own initial/empty lines. A chest-kind lookup has no chest row there
and gives its empty result.

Only a working-layout kind bypassing all five item branches allows the matching 427/4119 description.
That path neither consults nor sets F158. **Unknown:** the natural block value, actual approach,
collection or later layout change. Two coordinate matches do not imply two effects from one search,
or that collecting the Secret Book reveals the FAIRIES description on the next search.

## Item Availability, Recipient Choice and Rollback

**Confirmed:** the four-byte `mapItem` format stores X,Y,flag byte,item byte. `7-chest-items.asm` and
`8-other-items.asm` each have two rows followed by `endWord`:

| Table / zero-based row | Coordinate | Collection flag | Item byte / identity |
| --- | --- | --- | --- |
| Chest 0 | `(29,27)` | F202 | `$7B` /123 MITHRIL |
| Chest 1 | `(2,3)` | F221 | `$7A` /122 VIGOR_BALL |
| Other 0 | `(24,9)` | F157 | `$05` /5 FAIRY_POWDER |
| Other 1 | `(52,2)` | F158 | `$79` /121 SECRET_BOOK |

The flag byte is already the game-flag index. Do not add a chest base or substitute a row number.
All four items differ from ITEM_NOTHING 127 and lie below the gold threshold 128. Their names alone
establish neither use nor promotion effects; the other-item table does not identify each natural
layout kind.

`GetChestItem` / `GetNonChestItem` choose the current-map table, and `GetItem` takes its first X/Y
match. Admitted Map23 exploration has CURRENT_BATTLE=255, so it skips battle-coordinate adjustment.
A negative row X terminates; no match returns -1, which the later `$7F` mask classifies as
ITEM_NOTHING. Supplied battle-mode input is not this village exploration contract.

`OpenChest` substitutes ITEM_NOTHING when a matching collection flag is already set. Matching
chests set that flag and replace the working-layout word with open chest `$D802` /55298, requesting
the selected plane update. Closed `$D801` /55297 and open chest both mask to `$1800`; a repeat can
still select the chest branch and its 403/408 result. No matching row means no flag/block mutation.
`CheckNonChestItem` similarly tests/substitutes and sets a matching flag but does not change layout.
While the flag stays set, an admitted repeat cannot grant a second copy or fall through to a
description.

### Leader First, Then Joined Members

For a nonempty item, `loc_239EE` sets its name parameter and emits 413
(`{NAME}`, `{N}`, `{ITEM}`, `{W2}`, `{CLEAR}`). It tests leader index 0 with inventory capacity 4.
With room it calls `j_AddItem`, requests item music, emits 415 (`{NAME}`, `{N}`, `{ITEM}`; no annotated
W-token), then calls `FadeOut_WaitForP1Input`. With the leader full it rebuilds force lists through
`UpdateForce`, scans `OTHER_FORCE_MEMBERS_LIST` with counter `TARGETS_LIST_LENGTH-2` and DBF, and
gives to the first member with room. It sets name 3, requests the same music, emits 416
(`{NAME}`, `{N}`, `{ITEM}`, `{NAME}`), and uses the same wait before common closure/return -1.

The list is joined membership in ally-index order, not active battle-party membership or visible
village followers. This search contains no HP eligibility test. OTHER_FORCE_MEMBERS_LIST aliases
TARGETS_LIST+1, skipping Bowie under ordinary Bowie-joined state. With a supplied missing-Bowie flag,
leader 0 is still tested first and the rebuilt list still skips its first entry; the caller introduces
no membership repair. A reserve or currently invisible joined member can therefore be eligible in
the admitted static scan; actual recipient state remains Unknown.

Slots are counted by masked item ID against 127. `AddItem` writes the first empty slot, retains
index/broken bits and returns d2=0; with no slot it returns d2=1. The search caller does not branch
on that return after its capacity check. A successful handoff is conditional on stable inventory
across that seam; the source does not establish recovery if the earlier room check and AddItem differ.

### When Every Scanned Inventory Is Full

If no eligible recipient has room, including a negative initial scan counter, text 417 reports full
hands (`{N}`, `{W1}`). The caller restores searched X/Y, calls `CloseChest` then `RefillNonChestItem`,
and closes/returns -1. CloseChest clears a matching chest flag, restores `$D801` and requests the
plane update. RefillNonChestItem clears a matching other-item flag. These tables have no overlapping
chest/other coordinates, so the irrelevant lookup does nothing.

The selected item is available to a later admitted search after rollback. This preserves the source's
retry boundary; it does not prove a naturally collected item, rendered chest animation, completed
inventory write, input wait or save/load persistence.

## Internal Stairs Keep the Current Map

**Confirmed:** selected warp rows 4/5 in `6-warp-events.asm` are a no-scroll current-map pair:

| Source-comment stair | Trigger | Type / map | Destination / facing |
| --- | --- | --- | --- |
| Up | `(21,8)` | 0 / MAP_CURRENT 255 | `(57,3)`, Left 2 |
| Down | `(57,3)` | 0 / MAP_CURRENT 255 | `(21,8)`, Right 0 |

Walking calls `WarpIfSetAtPoint` only for candidate kind `$1000` after movement/collision checks;
vehicle consumers have their own preceding checks. The producer divides candidate X/Y by 384, scans
rows in order, publishes MAP_EVENT_TYPE=1 and type/map/destination/facing parameters, and sets
WARP_SFX. It does not relocate the party itself. Neither stair matches an earlier external row;
the four external exits are outside this local continuation. Natural stair access in any mode is Unknown.

`ProcessMapEvent` clears pending before dispatch. The normal no-scroll consumer requests/clears
WARP_SFX, idles 0 and unwinds to MainLoop, carrying d0=255, the selected destination, facing and d4=0.
SwitchMap has no 255 row. CheckBattle resolves 255 to current 23 for its scan; the complete accepted
battle table has no Map23 row, so it restores incoming 255. The local continuation keeps the sentinel
unless intervening pending state changes which request is consumed.

### Existing Actors and Layout Survive the Reload

ExplorationLoop still clears pending, reduces the step counter by 20000 with a zero floor, calls
HealLivingAndImmortalAllies and starts fade-out. On its low-byte 255 branch it waits for fade completion,
updates main 0's positive destination/current coordinates and facing, and refreshes Bowie's supplied
class/HP sprite. It calls DeclareRaftEntity, but does not rebuild setup entities or mapsprites, reset
temporary flags 256–383, or newly set F80 through the fresh-map branch. CURRENT_MAP=23 and the existing
CURRENT_BATTLE survive. This is not a new default/F762 population.

| Supplied mode | Existing followers and separate raft |
| --- | --- |
| 0 / walking | Relocates every physical entry in the existing EXPLORATION_ENTITIES list; then applies F64/saved-raft selection. |
| 1 / Caravan | Skips that follower relocation; reaches F64/saved-raft selection. |
| 2 / raft | DeclareRaftEntity returns immediately. |

Other mode values follow the non-2/non-1 relocation branch without an established repair. Relocation
updates both current and destination coordinates without rebuilding the list; it adds or clears no
membership/follower flags. A current-only follower is retained, while ordinary NPCs are not relocated
by this list operation. For modes other than 2, F64 clear returns. F64 set with saved RAFT_MAP=23
declares the separate raft at masked saved X/Y; a mismatch clears lookup 63 and parks its raw
current/destination position at `$70007000`. Saved raft coordinates/map and PLAYER_TYPE are not rewritten.

The shared tail initializes sprite frame counters, selects view target 0, calls LoadMap with 255,
installs base VInts and runs the currently selected init. LoadMap's negative-byte branch skips block/
layout decoding and retains the working layout before area/roof evaluation. Both setups' init is
the same one-`rts` `ms_map23_InitFunction`: no scene, request, actor, flag or transport repair.
Map loading/init therefore still occurs without fresh population or temporary-flag clearing.
The four collection flags, F13 and F762 are not cleared by these selected calls.

### Conditional Return to Control

After conditional music/fade work, WaitForEvent installs control only when pending is zero. It follows
physical 0 and selects `SetControlledEntityActScript`: mode 0 uses eas_ControlledCharacter; mode 1 uses
eas_Raft, actually Caravan; other values use eas_Caravan, actually raft for mode 2. The numeric mapping
controls the interpretation despite those source labels. Stairs do not disembark or normalize mode;
a refreshed Bowie sprite does not prove walking. An intervening request wins before control installation.

Ordinary returning conversation/search resumes the existing exploration loop subject to pending,
rather than forcing fresh entry. **Unknown:** completed services, actions, fades, map rendering,
usable input and persistence. Stop here; no mandatory village itinerary or subsequent journey follows.

The separate [Elven Village departure through Harpy Pond entry](elven-village-departure-through-harpy-pond-entry.md)
explains selected external exits, independent Devil's Tail scroll and the two initial help scenes.
It preserves this guide's local stop and supplies no mandatory route or prerequisite village activity.

## Evidence Trace and Review Boundary

**Confirmed provenance:** accepted research binds SF2DISASM master
`c834c652b6862bc5679fd7f69a38a7093206efc6` and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Its [reproduction owner](../../research/elven-village-conversations-and-local-exploration.md#reproduction-and-retained-limits)
retains source paths, addresses, literal controls, comparisons and immediate consumer joins.
Here Map23 setup filenames are relative to `data/maps/entries/map23/mapsetups/`, map tables to
`data/maps/entries/map23/`, in that pinned `disasm/`. No source reinvestigation is performed.

| Explanation | Contract / exact executable evidence | Deliberate limit |
| --- | --- | --- |
| Entity-first input, logical mapping, facing and action bits | [exploration flow](../contracts/exploration-control-flow.md), [map exploration](../contracts/map-exploration.md); `sf2-map-event-interaction-state-static-v1` in [interaction](../../../tests/fixtures/h2/map-event-interaction-state-static-v1.json), sourceContext and interactionState.dispatchContexts/producerWrites | Fixed-state/dispatcher seams are not actual Map23 caller state. Selected row bits and population joins come from accepted local research. |
| Local text/F13/church/default handlers | `sf2-map-events-static-v1` [index](../../../tests/fixtures/h2/map-events-static-v1.json); [entity shard](../../../tests/fixtures/h2/map-events/entity-programs.json), expected.entityTargetPrograms programOrder 278–287; [zone shard](../../../tests/fixtures/h2/map-events/zone-programs.json), expected.zoneTargetPrograms[47]; [item shard](../../../tests/fixtures/h2/map-events/item-programs.json), expected.itemTargetPrograms[22]; [dialogue contract](../contracts/dialogue-system.md) | Ordered operations/targets are public facts; literal paraphrases/controls use accepted research annotations. They do not prove displayed content, story events or church transactions. |
| Kind priority, d6 callers, four-slot handoff/rollback | [exploration flow](../contracts/exploration-control-flow.md); `sf2-field-search-control-static-v1` in [search](../../../tests/fixtures/h2/field-search-control-static-v1.json), fieldSearchSpine.callers/blockDispatch/itemRecipientPath/fullInventoryRollback/returnContract | Shared source/H1/ROM spine does not serialize selected village rows. PR862's later reuse compared its source projection only; it was not a fresh H1/ROM run. |
| Description row selection | [area descriptions](../contracts/map-area-description-routing.md); `sf2-map-descriptions-static-v1` in [descriptions](../../../tests/fixtures/h2/map-descriptions-static-v1.json), expected.consumerFacts | Public summary/conditioned metadata does not serialize these three titles/coordinates. Selected six-byte rows and d7 result use accepted research, not aggregate counts. |
| Joined versus active recipient list, inventory flags | [membership](../contracts/party-membership-state.md); `sf2-common-stats-static-v1` in [stats](../../../tests/fixtures/h2/common-stats-static-v1.json), expected.statsFacts.party/flags/inventoryBoundary | Four party summary facts do not encode this complete recipient scan. Numeric F13/item flags, slots, AddItem and rollback details use accepted field-search/local source joins. |
| No-scroll request and current-map retention | [entry routing](../contracts/map-entry-routing-state.md), [map exploration](../contracts/map-exploration.md); `sf2-map-event-request-consumption-static-v1` in [request](../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json), eventRequestConsumption.consumerContexts/accessSites | Shared publication/poll/clear shape is not a runtime stair. Exact row 4/5 directions, no Map23 battle and existing-population/layout consumers use accepted research and arrival evidence. |
| Shared return-only init and lifecycle | [setup](../contracts/map-setup-data.md), [exploration flow](../contracts/exploration-control-flow.md); `sf2-map-init-static-v1` in [init](../../../tests/fixtures/h2/map-init-static-v1.json), expected.primarySourceBodies sourceOrder 38, routeJoins[59]/[60], pointerTableJoins[62]/[63]; `sf2-map-script-engine-static-v1` in [engine facts](../../../tests/fixtures/h2/map-script-engine-static-v1.json), expected.mapLifecycleCommandFacts | Retained full init/script comparisons guard their defined facts; they do not compose local stairs, rendering or usable input. |

Schema/consumer pairs are [interaction schema](../../../schemas/h2/map-event-interaction-state-static-fixture.schema.json)/[map_event_interaction_state.py](../../../src/sf2tool/h2/map_event_interaction_state.py),
[event index schema](../../../schemas/h2/map-events-fixture-index.schema.json) and
[entity](../../../schemas/h2/map-events-entity-programs.schema.json),
[zone](../../../schemas/h2/map-events-zone-programs.schema.json),
[item](../../../schemas/h2/map-events-item-programs.schema.json) section schemas/[map_events_fixture.py](../../../src/sf2tool/h2/map_events_fixture.py)
and [map_events.py](../../../src/sf2tool/h2/map_events.py),
[search schema](../../../schemas/h2/field-search-control-static-fixture.schema.json)/[field_search_control.py](../../../src/sf2tool/h2/field_search_control.py),
[description schema](../../../schemas/h2-map-descriptions-static-fixture.schema.json)/[map_descriptions.py](../../../src/sf2tool/h2/map_descriptions.py),
[stats schema](../../../schemas/h2/common-stats-fixture.schema.json)/[stats.py](../../../src/sf2tool/h2/stats.py),
[request schema](../../../schemas/h2/map-event-request-consumption-static-fixture.schema.json)/[map_event_request_consumption.py](../../../src/sf2tool/h2/map_event_request_consumption.py),
[init schema](../../../schemas/h2-map-init-static-fixture.schema.json)/[map_init.py](../../../src/sf2tool/h2/map_init.py),
and [engine schema](../../../schemas/h2/map-script-engine-fixture.schema.json)/[map_script_engine.py](../../../src/sf2tool/h2/map_script_engine.py).
Existing command owners are `uv run sf2 h2` with `map-event-interaction-state`, `map-events`,
`field-search-control`, `map-descriptions`, `common-stats`, `map-event-request-consumption`, `map-init`
and `map-script-engine`. Load ignored worktree configuration in the same process before controlled
reproduction. These routes identify existing evidence; they do not select new prose-only runs.

Pre-synthesis review inspected accepted research, the stricter shared contracts, these selected
fixture operations/fields and schema/consumer shapes. Public summaries, retained full comparisons
and PR862's source-only field-search reuse remain distinct. The older description and membership
contracts' narrower Unknowns are not silently enlarged by their aggregate fixtures; later selected
source joins supply the bounded content here. No Layer A correction is required.

Counterexamples reviewed include F13 clear/set independently of F762, changed setup flags with retained
actors, C intercepted by an entity, counter versus search targeting, non-item kind at `(52,2)`, repeat/
full inventory, missing Bowie membership, changed inventory across AddItem, stale current followers,
modes 0/1/2, saved-raft mismatch and replaced pending state. These constrain static claims; they are
not executed scenarios.

Preserve [PR862 original handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/860#issuecomment-6162414060),
[rebased handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/860#issuecomment-6162442630)
and [acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/862#issuecomment-6162494122),
including unchanged retained comparisons, original/final CI, corrected truncated/lookup/table inspections
and linked predecessor failures. Preserve [PR861 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/861#issuecomment-6162400984)
and its [handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/859#issuecomment-6162382778).
No fixture, schema, golden, private payload or completed failure is regenerated or replaced here.

**Unknown:** natural access/terrain, actual actor lookup/facing/counter/layout, caller flags,
membership/inventory/recipient, service return, church outcomes, text substitutions/cadence, item
collection/persistence, current mode/followers/raft, completed stair/load/init and responsive input.
These gaps remain deferred under [ADR0014](../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../../decisions/0016-remake-start-evidence-deferral.md), not an automatic acquisition queue.
This optional segment completes neither the design epic nor a campaign route.
