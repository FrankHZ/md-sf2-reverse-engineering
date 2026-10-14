# Elven Village conversations and local exploration

Evidence date: 2026-10-14. **Confirmed:** Map23 has six default NPC event rows,
four supplied-F762 rows, three area descriptions, two chest items, two other
items and a current-map stair pair. The search dispatcher selects an item-kind
branch before considering descriptions; the Secret Book and description at
`(52,2)` are not two effects of one search. **Inferred:** these form an optional
village conversation and exploration segment. **Unknown:** natural access,
actual caller state, selected layout kind, completed services and usable control.
No mandatory conversation, collection or stair itinerary follows from the data.

## Agreement and provenance

Standard rigor under [Issue #860](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/860)
and [the content epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514):
this bounded cross-owner join needs explicit admission and stopping conditions.
Accepted starting base: `4573e22ef865d56b1bfa026f07330f5c4d2674cc`.
This document is the sole owned path, with no shared edits or stacked dependency.
Consume the accepted [entry and population owner](./battle19-outcomes-through-elven-village-entry.md).
Stop at conditional Map23 control after an admitted local interaction/search/stair,
or at the church call/return boundary. Exclude the F762 producer, Fairy Cave/Creed
travel, Elric rescue/recruitment, Map24/Battle20, later travel, service transactions,
item use/promotion, terrain reconstruction, new verification machinery and runtime
acquisition. Material missing machinery or scope coupling requires replanning.

Reuse [field search](./field-search-control.md), [interaction state](./map-event-interaction-state.md),
[dialogue state](./map-event-dialogue-state.md), [item-event transactions](./map-event-item-transactions.md),
[common stats and membership](./common-stats.md), [map content](./map-content.md)
and [request consumption](./map-event-request-consumption.md). Item-event dispatch
is distinct from searching a map's chest/other-item rows. These shared owners
remain authoritative for their algorithms; this document selects village content
and its immediate consumers, not a new all-map inventory or replay contract.

All source paths below are relative to `disasm/` in
[SF2DISASM master at `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The reused read-only checkout is clean at that pin. Retained payloads bind USA
retail ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Map23 setup filenames are under `data/maps/entries/map23/mapsetups/`; map data
filenames are under `data/maps/entries/map23/`. Exploration consumers are under
`code/gameflow/exploration/`; map scripting consumers under
`code/common/scripting/map/`. Fnnn denotes flag nnn; row indices are zero-based.
Text summaries below paraphrase the selected source annotations while preserving
numeric IDs, substitution/control tokens and branch order. They are not displayed
text observations or new claims about events mentioned by speakers.

The reused map-init/map-script-engine pair is 18,285,979 bytes. Budget: one linear
retained comparison, bounded selected source and existing fixture reads, small
prose/receipts, below 512 MiB peak and one minute for comparisons. No source
regeneration, runtime, new environment or game-material upload is needed. The
retained pair plus existing cutscene-source comparison passed in 0.095 seconds;
selected event/source and field-search-spine comparisons also passed. Reassess
before materially increasing source, memory, output or runtime scope.

## Entity admission and row action flags

**Confirmed:** `data/maps/mapsetups.asm` selects default `ms_map23`, or
`ms_map23_flag762` when supplied F762 is set. Their pointer tables change entities
and entity events only; zone, description, item and init targets are shared.
`GetCurrentMapSetup` resolves the supplied flag state again at event dispatch.
F13 does not select a population. A fresh population's logical NPC identities
start at 128, independently of physical slots after admitted followers.

In the ordinary `explorationvints.asm:ProcessPlayerAction` path, main entity 0
is idled and movement/view-scroll completion is awaited. A takes the menu path;
C reaches interaction after the separate debug and overlapping-Caravan branches.
`explorationfunctions_0.asm:GetActivatedEntity` requires a nonnegative view target,
uses its facing and the direction offsets from
`code/gameflow/battle/battlefunctions/battlefunctions_0.asm:table_PixelOffsets_X/Y`,
and advances the candidate through working-layout desk/counter blocks (`$2800`
after `$3C00` masking). It scans physical entries, skips the viewed entity and
current follower-list entries, and admits the first whose Manhattan distance
from the candidate is less than one tile (384 internal units). It returns the
actor's old facing in d1 and the player's facing in d2. Initial placement alone
therefore does not establish activation, exact adjacency, or a natural approach.

The same battle-functions file's `GetEntityEventIndex` maps that physical entry
back through `ENTITY_INDEX_LIST`; after the ally segment it uses logical 128
onward. A missing match falls back to 0. In
`mapsetupsfunctions_1.asm:RunMapSetupEntityEvent`, the logical ID selects the first
matching four-byte row, otherwise the `$FD` default. The incoming d2 is stored
in `EVENT_RELATIVE_POSITION`. Portrait/speech state comes from
`GetEntityPortaitAndSpeechSfx`; the shared dispatcher conditionally opens the
portrait and calls the selected handler with entity updates suspended.

The second `msEntityEvent` operand is an action bitfield here, despite the macro's
facing comment and direction-shaped names. `sf2enums.asm` resolves UP=1 and DOWN=3:
bit 0 makes the actor face `(playerFacing+2)&3` after a VInt; bit 1 restores its
saved facing after the handler returns. Thus UP rows turn without that restore,
and DOWN rows turn and restore. This is not a requirement that Bowie approach
from the named direction, nor the actor's initial facing. Both return paths close
portrait/text and reactivate entity updates. The default byte 0 does neither turn.
Actual mappings, action progress, portrait/SFX results and asynchronous completion
remain supplied-state conditions.

### Complete default table and handlers

**Confirmed:** `s2_entityevents.asm` (`0x59B44..0x59BA8`) binds the following rows
to the accepted `s1_entities.asm` population. Event numbers abbreviate
`Map23_EntityEventN`; coordinates/facing identify initial rows, not fixed poses.

| Logical actor / initial placement | Row byte / handler | Ordered text IDs, literal paraphrase and controls |
| --- | --- | --- |
| 128 MAN, `(18,9)` Down, walking range 1 | UP 1 / Event 0 | 1690: welcome; travelers stop to rest (`{N}`, `{W2}`). 1691: permission to stay as long as desired (`{N}`, `{W1}`). |
| 129 DWARF, `(17,11)` Right, fixed | DOWN 3 / Event 1 | 1692: a fairy reportedly lives nearby; asks whether the listener knows her (`{N}`, `{N}`, `{W1}`). This is text, not a yes/no prompt. |
| 130 OLD_MAN, `(18,11)` Left, fixed | DOWN 3 / Event 2 | 1693: asks about the fairy (`{W2}`); 1694: says she has not returned from Creed's mansion (`{N}`, `{W1}`). |
| 131 WORKER, `(54,3)` Down, fixed | UP 1 / Event 3 | 1695: only the fairy knows the way to Creed Mansion (`{N}`, `{N}`, `{W2}`); 1696: suggests her friend `{NAME;13}` may know (`{NAME;13}`, `{N}`, `{N}`, `{W1}`). |
| 132 ELF_FEMALE, `(8,12)` Right, walking range 1 | UP 1 / Event 4 | Tests F13; exact alternatives below. |
| 133 PRIEST, `(61,3)` Down, fixed | UP 1 / Event 5 | `jsr j_ChurchMenu`, then `rts`; no local text sequence. |

Events 0–3 contain only their listed text calls and return. Event 4 uses
`chkFlg 13; bne.s byte_59B94`: clear falls through to 1697 then 1698, followed by
an unconditional branch to the return; set selects 1699 then 1700.

| Supplied F13 | Text order and controls |
| --- | --- |
| Clear | 1697: `{NAME;13}` is away seeking the fairy to help a sick dwarf (`{NAME;13}`, `{N}`, `{N}`, `{W2}`). 1698: the western forest is dangerous (`{N}`, `{W1}`). |
| Set | 1699: a forest elf repays kindness (`{N}`, `{W2}`). 1700: tells `{LEADER}` that `{NAME;13}` will help (`{LEADER}`, `{N}`, `{NAME;13}`, `{W1}`). |

`sf2enums.asm` identifies ally 13 as Elric; the common membership owner assigns
joined flag `allyIndex+0`, so F13 is his joined flag. `code/common/stats/gameflags.asm:CheckFlag`
returns zero for clear, nonzero for set. This handler reads that bit; it does not
join Elric, set F13, bring back a fairy, heal a dwarf or write a request. `{W1}`,
`{W2}` and `{N}` remain explicit control tokens; no frame duration or completed
button advance is inferred from them. The named substitutions require supplied
name state. Mentioning a destination does not admit travel to it.

`Map23_EntityEvent5` is shared by both setups. The retained alias join is
`j_ChurchMenu` at `0x20004` → `ChurchMenu` at `0x20A02` in
`code/common/menus/church/churchactions_1.asm`. Stop at that admitted call and
conditional return; no savepoint change, healing, revival, promotion, price,
transaction outcome or post-service state is established here. Shared dispatcher
cleanup follows only if the call returns. `Map23_DefaultEntityEvent` is only
`rts`, including when used by the alternate table.

### Supplied F762 table

**Confirmed:** `s2_entityevents_762.asm` (`0x59BA8..0x59BD2`) joins the four
`s1_entities_762.asm` rows. Alternate event names are `Map23_2FA_EntityEventN`.

| Logical actor / initial placement | Row byte / target | Ordered content |
| --- | --- | --- |
| 128 MAN, `(18,9)` Down, walking range 1 | UP 1 / alternate Event 0 | 1701: asks whether the listener met Creed (`{N}`, `{W1}`). |
| 129 WORKER, `(56,6)` Down, walking range 1 | UP 1 / alternate Event 1 | 1702: says evil has lifted completely from the forest (`{N}`, `{W1}`). |
| 130 OLD_MAN, `(18,11)` Up, fixed | DOWN 3 / alternate Event 2 | 1703: asks whether the sick dwarf is better (`{W2}`); 1704: asks whether he can now see the fairy (`{W1}`). |
| 131 PRIEST, `(61,3)` Down, fixed | UP 1 / default Event 5 | Shared church call/return boundary. |

The three alternate bodies are text then return, without F13 testing or a local
flag/membership/request mutation. Their story wording does not prove that any
mentioned event happened. F762's producer is excluded. Dispatch resolves the
current setup, while an existing population can survive a current-map stair;
there is no automatic re-population guarantee if supplied flags and actors are
inconsistent. In particular alternate logical 131 is the priest, default 131 the
worker. Use the admitted mapping, not a sprite name or stale setup assumption.

## Local descriptions and item-search precedence

**Confirmed:** both setups share `s3_zoneevents.asm:Map23_DefaultZoneEvent` and
`s5_itemevents.asm:Map23_DefaultItemEvent0`, each only `rts`. Their tables contain
only their respective defaults, no selected zone or item-specific rows. A
return-only item-use handler neither grants the searchable Secret Book nor proves
that downstream generic item use is inert; item use remains excluded.

`s4_descriptions.asm` (`0x59BD8..0x59BFC`) sets d3=`$1015` (4117), points a0 to
`byte_59BE8`, and jumps to `DisplayAreaDescription`. Each `msDesc` row encodes
X,Y,0,0,investigation offset 4,description offset; `$FD` terminates the table.

| Coordinate | Description offset / text ID | Paraphrase |
| --- | --- | --- |
| `(5,20)` | 0 /4117 | A book titled AN APPLE A DAY. |
| `(51,2)` | 1 /4118 | A book titled HOME-MADE POTIONS. |
| `(52,2)` | 2 /4119 | A book titled FAIRIES. |

`mapsetupsfunctions_1.asm:DisplayAreaDescription` uses the first packed X/Y match.
The selected rows have zero condition and function-kind bytes: no d6 restriction
and no callback. It emits investigation `423+4=427` (bookshelves; `{NAME}`, `{N}`,
`{W2}`, `{CLEAR}`), then `d3+offset` (each title has `{N}`, `{W1}`), closes
portrait/text and returns d7=-1. No match leaves d7=0. The wrapper tests d7 before
restoring registers, so `CheckArea` branches on the returned condition codes.
These are callable content rows, not unconditional search results.

### Admission before the coordinate lookup

**Confirmed:** ordinary C interaction tries an entity first; an admitted entity
handler returns without searching. With no activated entity it calls `CheckArea`
with d6=1. The FieldMenu search action calls `j_CheckArea` with d6=0 independently
of that entity lookup. `CheckArea` requires a nonnegative view target, clears
`DIALOGUE_NAME_INDEX_1` and speech SFX, computes one faced tile from the view
entity and direction offsets, divides by 384 and indexes the 64-wide working
layout. Unlike `GetActivatedEntity`, it does not repeat through counter blocks.
It masks the layout word with `$3C00` and dispatches in this order:

| Layout kind | Item consumer | Initial / empty text IDs |
| --- | --- | --- |
| `$1800` chest | `OpenChest` | 403 /408 |
| `$2C00` vase | `CheckNonChestItem` | 404 /409 |
| `$3000` barrel | `CheckNonChestItem` | 405 /410 |
| `$3400` bookshelf | `CheckNonChestItem` | 427 /412 |
| `$1C00` searchable area | `CheckNonChestItem` | 423 /412 |
| Any other masked value | `j_RunMapSetupAreaDescription` | Selected description, if matched. |

Each of the first five branches either enters item handoff or emits its empty
text and jumps to common closure; none falls through to descriptions. A handled
description also closes and returns d0=-1. With no match, d6=1 returns 0 so the
ordinary C caller can open its menu; d6=0 emits 423 then 412, closes and returns-1.
A counter can extend NPC activation but does not extend this search coordinate.
The tables alone cannot establish that a given pose/layout reaches a row.

At `(52,2)`, if the working layout is bookshelf `$3400`, the other-item row
selects the Secret Book when F158 is clear. If already taken (or no item row
matched),427 is followed by 412. If inventory is full, the item failure/rollback
path below runs. None of these branches emits 4119, on this call or by an automatic
retry. Other non-chest item kinds have the same item-before-description priority
with their own initial/empty lines. Only a layout value bypassing all five item
branches allows the matching 427/4119 description; that path does not consult or
set F158. A chest-kind value would instead search the chest table, which has no
row there, and produce its empty result. No natural block value, layout change
on collection, terrain approach or dual acquisition/description is claimed.

## Four item rows and immediate inventory/flag consumers

**Confirmed:** `7-chest-items.asm` (`0xA6EFE..0xA6F08`) and `8-other-items.asm`
(`0xA6F08..0xA6F12`) each have two four-byte `mapItem` rows then `endWord`.
`sf2mapmacros.asm` encodes X,Y,flag byte,item byte; `sf2enums.asm` resolves items:

| Table / row | Coordinate | Flag | Item byte / identity |
| --- | --- | --- | --- |
| Chest 0 | `(29,27)` | F202 | `$7B` /123 MITHRIL |
| Chest 1 | `(2,3)` | F221 | `$7A` /122 VIGOR_BALL |
| Other 0 | `(24,9)` | F157 | `$05` /5 FAIRY_POWDER |
| Other 1 | `(52,2)` | F158 | `$79` /121 SECRET_BOOK |

The flag byte is already the game-flag index; do not add a chest base or use
row 0/1 as the flag. All four IDs are below the gold threshold 128 and differ from
ITEM_NOTHING 127. No item-use or promotion effect follows from these names.

`exploration.asm:GetChestItem/GetNonChestItem` selects the corresponding current
map table, then `GetItem` scans four-byte rows for the first X/Y match. In admitted
Map23 exploration CURRENT_BATTLE=255, so the battle-coordinate adjustment is
skipped. A match returns the layout offset in d0, zero-extended flag in d1 and
item in d2. A negative row X terminates; no match returns-1 in all three. Later
`d2 & $7F` converts that no-match result to ITEM_NOTHING. Supplied battle-mode
inputs are not this village exploration contract.

`OpenChest` tests the flag; set substitutes ITEM_NOTHING, clear retains the row's
item. Both matching paths set the flag and replace the working-layout word with
MAP_BLOCKINDEX_OPEN_CHEST 55298 (`$D802`), scheduling the selected plane update.
Closed chest 55297 (`$D801`) and open chest both mask to `$1800`, so a repeated
search can still enter the chest branch and report 403/408 while its flag is set.
No matched row means no flag/block mutation. `CheckNonChestItem` applies the same
flag test/substitution and sets a matching flag, but does not change the layout.
Thus an admitted repeat cannot acquire a second copy while the flag remains set;
it takes the selected kind's empty line and does not reveal a description.

For a nonempty selected item, `explorationfunctions_1.asm:loc_239EE` sets the
item-name parameter and emits 413 (`{NAME}`, `{N}`, `{ITEM}`, `{W2}`, `{CLEAR}`).
It tests leader index 0 first with `GetItemBySlotAndHeldItemsNumber` and capacity 4.
With room it calls `j_AddItem`, requests item music, emits 415 and waits through
`FadeOut_WaitForP1Input`;415 has `{NAME}`, `{N}`, `{ITEM}` and no W-token in its
annotation. With leader full, it calls `UpdateForce`, scans
`OTHER_FORCE_MEMBERS_LIST` using `TARGETS_LIST_LENGTH-2` and DBF, and gives to the
first entry with room. It sets name 3, requests the same music, emits 416
(`{NAME}`, `{N}`, `{ITEM}`, `{NAME}`) and uses the same wait. Then common closure
closes text and returns-1.

`code/common/stats/battleparty.asm:UpdateForce` rebuilds joined members in ally
index order; `sf2const.asm` aliases OTHER_FORCE_MEMBERS_LIST to TARGETS_LIST+1.
Under ordinary Bowie-joined state this skips the first member, Bowie. The list
is joined membership, not just active combatants or visible village followers;
there is no HP test here. A supplied missing-Bowie flag does not become a repair
rule: leader 0 was still tested first and the list scan still skips its first
entry. `code/common/stats/itemstats.asm` counts slots by masked ID against 127;
`AddItem` writes the first empty slot, retains index/broken bits and returns d2=0,
or d2=1 when none is empty. The search caller does not branch on that AddItem
return after its capacity check. Stable inventory across that seam is therefore
part of the conditional successful handoff, not a guaranteed recovery behavior.

If no scanned member has room (including the negative initial loop counter),
text 417 reports full hands (`{N}`, `{W1}`). The caller restores the searched X/Y,
calls `CloseChest` then `RefillNonChestItem`, and closes/returns-1. CloseChest
clears a matching chest flag and restores `$D801` plus plane-update request;
RefillNonChestItem clears a matching other-item flag. These selected tables have
no overlapping chest/other coordinates, so the irrelevant lookup is a no-op.
The selected item remains available to a later admitted search after this
rollback. That is a static branch result, not proof of natural collection,
rendered opening, actual inventory mutation, wait completion or saved persistence.

## Internal stairs and conditional Map23 control

**Confirmed:** `6-warp-events.asm` (`0xA6ECC..0xA6EFE`) contains six eight-byte
rows. The first four are explicit exclusions from this local continuation:
north `(any,1)`, west `(0,any)` and south `(any,30)` select Map68 `(48,31)`,Down;
east `(31,24)` selects Map24 `(1,27)`,Right. No downstream content is activated
or analyzed here. Neither stair coordinate matches those earlier rows.

| Stair row | Trigger | Request type/map | Destination/facing |
| --- | --- | --- | --- |
| 4, source-comment stairs up | `(21,8)` | no-scroll 0 / MAP_CURRENT 255 | `(57,3)`, LEFT 2 |
| 5, source-comment stairs down | `(57,3)` | no-scroll 0 / MAP_CURRENT 255 | `(21,8)`, RIGHT 0 |

The walking consumer in
`code/common/scripting/entity/entityscriptengine_2.asm:loc_528A` calls
`WarpIfSetAtPoint` only when the candidate working-layout kind is `$1000`, after
its movement/collision checks; the vehicle consumers at loc_5782/loc_5904 have
their own preceding checks. This establishes conditional producer seams, not
natural ability to enter either stair in any mode. `WarpIfSetAtPoint` divides
candidate internal X/Y by 384 and scans rows in order, with negative coordinate
bytes as wildcards. It writes MAP_EVENT_TYPE=1, copies the type/map/X/Y bytes
into parameters 1–4 and facing/pad into 5–6, and sets WARP_SFX. It does not perform
the relocation itself; a later overwrite of pending state can change consumption.

`explorationfunctions_2.asm:ProcessMapEvent` clears pending before dispatch.
`ProcessMapEventType1_Warp` sees parameter 1=0, requests/clears WARP_SFX, idles 0
and unwinds the ExplorationLoop return to MainLoop. It loads d0=255, starts with
current position, replaces X/Y with the nonnegative destination bytes, loads
the row's facing into d3 and d4=0. `mainloop.asm` runs SwitchMap and CheckBattle.
The complete `data/maps/global/flagswitchedmaps.asm` has no 255 row; CheckBattle
resolves 255 to current 23 only for its scan and restores incoming 255 when no
battle matches. The accepted entry owner retains the complete battle table with
no Map23 row. Thus this admitted local continuation keeps the 255 sentinel.

### Existing population and current-map reload

**Confirmed:** `ExplorationLoop` still clears pending, reduces the step counter
by 20000 with a zero floor, calls HealLivingAndImmortalAllies and starts fade-out.
Its low-byte 255 branch waits for fade completion and calls
`UpdateMainEntityProperties`: supplied positive destination coordinates replace
main 0's current and destination X/Y, facing is replaced, and GetAllyMapsprite/
UpdateEntityProperties selects Bowie's supplied class/HP sprite. It calls
`mapfunctions.asm:DeclareRaftEntity`; it does not call GetMapSetupEntities,
InitializeMapEntities or LoadEntityMapsprites, does not reset flags 256–383, and
does not set F80 through the fresh-map branch. CURRENT_MAP=23 and the existing
CURRENT_BATTLE value are retained. It is not a new default/F762 population.

DeclareRaftEntity leaves actual mode 2 (raft) immediately. Mode 1 (Caravan) skips
current-follower relocation and reaches the F64/saved-raft test. Other modes
relocate every physical entry in the existing EXPLORATION_ENTITIES list to the
destination, updating current and destination coordinates without building a new
list. Thus a current-only follower is not discarded merely for lacking a fresh
setup row; ordinary NPCs are not moved by this list operation. No membership or
follower flag is added/cleared. For modes other than 2, F64 clear returns; F64 set with
saved RAFT_MAP=23 declares the separate raft at masked saved X/Y, while a mismatch
clears lookup 63 and parks its raw current/destination position at `$70007000`.
Saved RAFT_MAP/X/Y and PLAYER_TYPE are not rewritten by this path.

The common tail initializes exploration sprite frame counters, sets view target 0,
calls `code/common/maps/mapload.asm:LoadMap` with the incoming 255 sentinel,
installs base VInts and calls RunMapSetupInitFunction. Both supplied setup choices still select
`s6_initfunction.asm:ms_map23_InitFunction`, whose entire body is `rts`; it makes
no scene, request, flag, actor or transport repair. The negative-byte LoadMap
branch skips blocks/layout decoding and keeps the
existing working layout before area/roof evaluation. Map loading/init therefore
still occurs even though actor setup and temporary-flag clearing do not.
The four item flags, F13 and F762 are not cleared by these selected calls; save/load
persistence and actual completed map rendering remain Unknown.

After conditional music/fade work, WaitForEvent installs a controller only when
pending is zero. It follows 0 and uses
`code/common/scripting/entity/entityfunctions_2.asm:SetControlledEntityActScript`:
mode 0 selects eas_ControlledCharacter, mode 1 selects eas_Raft (actual Caravan),
and other values select eas_Caravan (actual raft for mode 2). Source names do not
reverse the numeric mapping. The stair does not disembark or normalize mode;
Bowie's refreshed sprite alone does not prove walking. An intervening request
wins before ordinary controller installation. Stop at this conditional Map23
control boundary, with all waits/action/service completion and usable input
unobserved. An ordinary local event/search return likewise resumes the existing
exploration loop subject to pending state, rather than forcing a new map entry.

## Reproduction and retained limits

Load `. ./local/private-inputs.ps1` in the same process as controlled commands.
Check the pinned checkout's exact revision and clean status, then reuse the
[retained comparison recipe](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary)
without changing either payload. This compares map-init to its fixture,
`_verify_map_script_engine_fixture` to the retained engine payload, and source
`_cutscene_facts` to `battle-cutscenes-static-v1.json`; it creates no composed
village runtime receipt.

| Existing evidence | Bounded reproduction/join |
| --- | --- |
| `tests/fixtures/h2/map-events/entity-programs.json` | Programs 278–287: compare every operation at its sourcePath/sourceLine, excluding annotations, to the complete selected default/F762 handlers. Preserve F13 branch polarity and shared church/default targets. |
| `map-events/zone-programs.json`, `map-events/item-programs.json` under the same fixture directory | Programs 47 and 22 respectively are the shared return-only Map23 defaults. |
| `field-search-control-static-v1.json` | Existing `_read_source_surface` and `_validate_source_contract` in `sf2tool.h2.field_search_control`, on the pinned disasm root, reproduce fieldSearchSpine exactly. This source-only comparison reuses the 17-file owner; it does not claim a fresh H1/ROM run. |
| `map-descriptions-static-v1.json`, `common-stats-static-v1.json` | Retain shared description/flag/item/membership semantics; directly read all three Map23 rows, both item tables, macros/enums and immediate named consumers. Aggregate description counts alone do not guard a selected title or coordinate. |
| `map-init-static-v1.json`, `map-script-engine-static-v1.json`, `map-event-request-consumption-static-v1.json` | Retained init/body 38 and route 59/60 and pointer 62/63 joins bind both setups to the same return. Direct complete warp rows and MainLoop/exploration/follower consumers establish current-map versus fresh-population behavior. |

Direct review must preserve all selected handler operations and text controls,
logical/physical mapping, action bits, first-match and item-before-description
order, flag identity, four-slot recipient/rollback behavior and the 255 sentinel.
Supplied-state counterexamples include F13 clear/set independently of F762,
changed flags with an old population, an entity intercepting C, counter versus
search targeting, a non-item layout at `(52,2)`, repeated/full inventory, missing
Bowie membership, stale current follower lists, modes 0/1/2 and a replaced pending
request. These constrain static claims; they are not executed scenarios.

**Unknown:** natural terrain/access, actual actor lookup/facing/counter/layout,
caller flags/membership/inventory, selected recipient and service return, item
collection/persistence, church outcomes, text substitutions and rendered cadence,
actual mode/follower/raft state, completed stairs/load/init and responsive input.
F762's producer and all excluded later story remain outside this outcome. These
gaps are deferred under [ADR0014](../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../decisions/0016-remake-start-evidence-deferral.md), not an automatic
runtime queue.

Preserve [PR857's handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/856#issuecomment-6162129601)
and [independent acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/857#issuecomment-6162154889),
including retained pair/results, corrected inspections and linked PR854/849/846/842
failures. A lookup miss or truncated inspection is not absence evidence; locate
and reread the real owner. Current corrections, exact Git identities, direct
checks/planner, final CI and process state belong to this Issue/PR handoff.
Acceptance requires complete owned prose/source/provenance/link/private review,
`git diff --check`, clean committed configured
`uv run sf2 verify plan --base origin/main --head HEAD`, and actual final-head CI.
No blanket local normal/full/engine/Godot/H3/H4 suite is selected solely for prose.
Leave a clean frozen Draft PR for independent main-gate integration; no cleanup.
