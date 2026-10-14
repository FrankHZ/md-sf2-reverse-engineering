# Ribble tree opening and Achilles Sword acquisition

Evidence date: 2026-10-13. **Confirmed:** two Ribble item rows admit the Wooden
Panel opening script. Its return precedes F731, a fresh force-wide removal by
item identity, and handled d6. Two separate Map50 chest rows offer the Panel and
Achilles Sword through the ordinary chest-capacity path. **Inferred:** these
facts describe the intended Panel/tree/Sword puzzle. **Unknown:** a continuous
walk between the chest regions, natural acquisition, actual actor and service
completion, rendered opening, usable control and save/resume persistence.

## Agreement and provenance

Standard rigor under [Issue #832](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/832)
and [the content epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514):
item custody, partial-state ordering and conditional map admission need explicit
boundaries. Accepted base is `f722e81f51be4937f13b67352838bf6b3d8c815a`.
Sole tracked owner is this document, with no shared-path or stacked dependency.

Scope is the two selected chest boundaries, complete Map8 item handler and
opening program, immediate motion/block/quake/follow/inventory consumers,
selected Map8-to-Map50 request, selected Ribble return and the resulting
possession predicate. External shrine approach, terrain/path reconstruction,
unrelated chests/tunnels, battle simulation, Rick, town services, Rohde replay
and Caravan operation are excluded. This is not a numbered-battle progression
requirement. No extraction, runtime acquisition or new verification machinery
is introduced.

Accepted owners are [Ribble setup/welcome](./battle09-outcomes-through-ribble-welcome.md),
[Hassan entry](./battle16-outcomes-through-hassan-entry.md),
[Rohde's possession caller](./hassan-ancestry-and-rohde-caravan-choice.md),
[item transactions](./map-event-item-transactions.md), [common stats](./common-stats.md),
[map data](./map-data-inventory.md), [scripting](./common-scripting.md),
[menus](./common-menus.md) and [requests](./map-event-request-consumption.md).
Source paths below are relative to `disasm/` in
[SF2DISASM master at `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at that pin. Retained map-init/map-script-engine
payloads bind the pin and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Fnnn means flag nnn; record positions are zero-based; coordinates and waits are
source operands, not measured motion or elapsed-time receipts.

The reused pair totals 18,285,979 bytes. Existing linear comparisons were
budgeted below 512 MiB and one minute and completed in 0.084 seconds. Remaining
work is bounded source/record reads and small prose/receipts, with no new corpus
or game-material output. Reassess before material expansion of scope or resources.

## Resolve map and item identities first

**Confirmed:** `sf2enums.asm` defines `MAP_RIBBLE=8`,
`MAP_ACHILLES_SHRINES=50`, `MAP_RIBBLE_SHRINE_EXTERIOR=61`,
`ITEM_WOODEN_PANEL=$70`, `ITEM_ACHILLES_SWORD=$3D`, `UP=1`,
`ALLY_BOWIE=0`, `ALLY_PETER=7`, `COMBATANT_ITEMSLOTS=4` and
`ITEMENTRY_MASK_INDEX=$7F`. Map50 is shared content storage, not Map61 and not
proof that its distinct regions connect by walking.

`data/maps/entries/map50/7-chest-items.asm` has the following selected rows;
the other chest is outside this slice:

| Physical row | Coordinates | Flag | Item |
| --- | --- | --- | --- |
| 1 | `(14,2)` | F152 | Wooden Panel `$70` |
| 2 | `(35,6)` | F147 | Achilles Sword `$3D` |

Neither row tests F731, possession of the other item, Rohde flags, party
membership or a battle completion. These are chest-content facts conditional on
reaching the search consumer with the right map, coordinates and layout. The
Panel may be explained at its own chest boundary without inventing its external
approach or a path from the Sword region.

## Chest eligibility, capacity and custody

**Confirmed:** `code/common/menus/main/mainactions.asm:FieldMenu` Search calls
`j_CheckArea` and exits the main menu. Ordinary C interaction in
`code/gameflow/exploration/explorationvints.asm:ProcessPlayerAction` first waits
for physical 0 and view scrolling; after debug/Caravan alternatives it checks
for an activated entity. Only the no-entity branch calls `CheckArea`. A nearby
actor can therefore select talk instead of chest search.

`explorationfunctions_0.asm:CheckArea` rejects a negative view target, derives a
facing-offset candidate and divides by `MAP_TILE_SIZE=384`, then tests the
working-layout attribute masked by `$3C00`. The chest branch requires `$1800`;
standing on a chest row is not the predicate. Ordinary exploration supplies
view target 0. The source both advances the entity base by the scaled view
index and uses indexed X/Y reads; this document does not generalize that
coordinate calculation to arbitrary supplied nonzero view targets.

`exploration.asm:GetChestItem` uses `CURRENT_MAP` and its chest table.
`GetItem` scans four-byte exact X/Y records to the terminator, returning the
layout offset, flag in d1 and item in d2, or -1 on no match. The admitted
exploration path has `CURRENT_BATTLE=NOT_CURRENTLY_IN_BATTLE`; its separate
in-battle coordinate adjustment is outside this contract. Map50's
`8-other-items.asm` is terminator-only, so the failure tail's second refill call
has no separate Map50 non-chest item to reset at either selected coordinate.

`OpenChest` checks the returned flag. Already set replaces d2 with
`ITEM_NOTHING`; clear retains the row's item. For a matching row it sets the
flag **before receipt**, writes the open-chest block and requests the applicable
view-plane update. `CheckArea` requests opened text 403 and then empty text 408
if the masked item is Nothing. An already-open chest grants nothing even if
its old item is no longer in the force. Supplied clear flags can offer another
copy despite an existing copy; there is no unique-item inventory predicate.

Both selected IDs are below `ITEMINDEX_GOLDCHESTS_START=128`, so they take
`explorationfunctions_1.asm:loc_239EE`, not the gold or mandatory-receive path:

| Supplied capacity/state | Ordered result if services return |
| --- | --- |
| Bowie has fewer than four items | Request found text 413; call `AddItem` for Bowie; request item music and receipt text 415; wait for the shared input/fade service; close windows. Chest flag stays set. |
| Bowie full; another joined member has space | `UpdateForce`; scan `OTHER_FORCE_MEMBERS_LIST`; add to the first member with space; request music and passed-item text 416; same completion tail. Flag stays set. |
| No available slot | Request full-hands text 417; restore saved candidate X/Y; call `CloseChest`, then `RefillNonChestItem`; close windows. No item is added. `CloseChest` clears the matching flag, restores its closed block and requests a view update. |
| Flag already set or no matching row | Empty result; no capacity scan or item grant. No receipt establishes custody. |

`code/common/stats/battleparty.asm:UpdateForce` builds `TARGETS_LIST` from
joined flags in ascending ally order, separately building active and reserve
lists, without an HP test. `sf2const.asm` places `OTHER_FORCE_MEMBERS_LIST`
exactly one byte after `TARGETS_LIST`; the chest loop runs force-length minus
one entries after that first byte. Its ordinary invariant is joined Bowie
first. It is not a search restricted to the active battle party, nor a general
repair for a supplied force missing Bowie. The initial capacity check hardcodes
Bowie regardless of membership. Dead or reserve joined carriers are not filtered.

`itemstats.asm:GetItemBySlotAndHeldItemsNumber` counts four slots whose masked
ID is not Nothing. `AddItem` scans for the first empty slot, stores the entry
masked by `$807F` (no automatic equip), and returns d2=0; full returns d2=1.
The chest caller checks capacity first but does **not** test this later AddItem
result. Under unchanged ordinary inventory the check and insertion agree;
concurrent/corrupt supplied state is not an atomic receipt guarantee. No forced
discard, inventory-choice prompt or `ReceiveMandatoryItem` is used here.

The flag/open block, item insertion, music, text and input completion are
separate effects. Interruption after the opening write but before insertion can
leave the flag set without custody; interruption after insertion can leave
custody before presentation finishes. The completed full-capacity tail restores
availability, but it is not a transaction rollback for every interrupted state.

## Wooden Panel use at Ribble

**Confirmed:** `data/maps/mapsetups.asm` selects Map8's default or F708 override.
Both `map08/mapsetups/pointertable.asm` and `pointertable_708.asm` point their
item section to `ms_map8_Section5`. In `s5_itemevents.asm`, physical rows 0/1
require `(15,19)` or `(16,19)`, Up, Wooden Panel; both target `Map8_ItemEvent0`
at `0x562F2`. The default is its shared `rts` at `0x56312`. F708 changes town
population but not these predicates; F709 is not a caller prerequisite either.

FieldMenu Item → Use obtains the selected carrier, slot and entry. Panel is
absent from `data/stats/items/usableoutsidebattleitems.asm`, so it takes the
map-event path. `GetPlayerEntityPosition` reads **physical 0's current** X/Y
and facing, not the forward search tile or selected carrier's location.
`j_RunMapSetupItemEvent` (`0x44088` → `0x47586`) masks the selected item to `$7F`,
starts d6=0 and scans six-byte rows. Neither row here uses a direction wildcard.
Wrong position, facing or item reaches the default; possession elsewhere alone
does not trigger the event. Ordinary menu cancellation never calls it.

`Map8_ItemEvent0` is ten physical operations, source lines 12–30:

| Supplied F731 | Ordered handler work |
| --- | --- |
| Set | Explicit d6=0; branch to the shared return. No script or removal. |
| Clear | Run `cs_56314`; after return set F731; d0=`ITEM_WOODEN_PANEL`; call `RemoveItemFromInventory`; then d6=-1 and return. |

The macro `script` in `sf2macros.asm` loads the entry and invokes Trap6.
`code/common/tech/interrupts/trap6_mapscript.asm` activates entity updates before
`ExecuteMapScript`; the menu's prior entity-update deactivation does not by
itself stop these waits. It preserves registers around the interpreter.

## Complete opening program and immediate consumers

**Confirmed:** `cs_56314` resides in `s5_itemevents.asm:32–65`, not the separate
Map8 `scripts.asm`. Retained `programCorpus.programs[id=cs_56314]` contains
30 records, ending at `csc_end`, with two inline movement bodies and no text
sites or internal jumps. The event fixture's ten-operation interval ends at
`0x56314`; it does not include this embedded script. The parser in
`src/sf2tool/h2/map_script_engine.py` groups recognized macros physically until
`csc_end` (or its admitted final jump); inline action instructions are separate
body material, not extra entries in the 30-command count. Here the map-script
graph is sequential, but actor completion is still asynchronous where unawaited.

| Records | Complete ordered work |
| --- | --- |
| 0–4 | Hide text/portrait; wait Bowie `eas_Init`; wait Peter `eas_Init`; quake 2; Sleep 60. |
| 5–10 | Start Bowie `eas_Jump` without waiting; wait Peter `eas_Jump`; start Bowie inline Down 2; wait Peter inline Down 2; face Bowie Up, then Peter Up. Both inline bodies end with `endActions`. |
| 11–15 | Request `SFX_BATTLEFIELD_DEATH`; copy `(32,17)`, size 2×2, to `(15,19)`; Sleep 30; quake 0; Sleep 30. |
| 16–21 | Quake 2; same SFX; copy `(34,17)`, size 2×2, to `(15,19)`; Sleep 30; quake 0; Sleep 30. |
| 22–27 | Quake 2; same SFX; copy `(32,14)`, size 2×2, to `(15,19)`; Sleep 30; quake 0; Sleep 30. |
| 28–29 | Request Peter→Bowie following selector 2; end. Caller F731/removal/d6 writes occur afterward. |

In `mapscriptengine_1.asm`, csc15 installs an action and its Wait form polls
that actor's script pointer for `eas_Idle`. csc2D compiles each Down 2 body to a
relative-destination action and installs it; Wait likewise awaits that actor.
These are not all-actor barriers or guaranteed unobstructed motion. In
`data/scripting/entity/eas_actions.asm`, `eas_Init` sets speed 32/32, disables
acceleration/deceleration, restores entity/map collision and auto-facing,
clears immersion, sets size 24, updates the sprite and reaches idle. `eas_Jump`
temporarily disables auto-facing, requests its three relative offsets with
two Wait 5 commands, restores auto-facing and reaches idle. A wait on Peter
alone does not establish that Bowie's previous jump/down action has completed
before a later action replaces it.

Action installation has no HP/presence guard. Logical actors resolve through
the current `ENTITY_INDEX_LIST`; missing/default-zero or stale mappings can
address the wrong physical entity. Facing csc23 uses the packed ally byte and
skips a zero-HP ally. Following csc2C instead tests the first byte of the
word-encoded follower operand: for Peter 7 that byte is zero, so this call tests
**Bowie's HP**, not Peter's. Zero-HP Bowie can skip follow while the caller still
sets F731 and handled d6 after returning services. Actual Peter presence is not
an admission predicate, and no actor is created or force member joined here.

The following consumer resolves both current mappings, converts selector 2 to
`(-24,0)` and calls `entityfunctions_2.asm:AddFollower`. That installs a copied
42-byte follower action with leader/offset operands, advances its allocation
pointer, and appends the physical follower to the terminated exploration list
unless already present. Duplicate-list avoidance does not skip action allocation.
It changes neither joined/active flags nor F67, and does not validate capacity,
repair mappings or prove completed following. Fresh population is a separate
flag-driven operation; the Ribble owner supplies that boundary.

For these unflagged quake operands, csc33 writes `QUAKE_AMPLITUDE` directly.
`exploration.asm:FinalizeScrollDataUpdate` consumes nonzero amplitude through
random horizontal/vertical offsets and VDP scroll updates. Final quake 0 is a
source write, not proof of the visible shake or a precise duration. csc34 packs
the six byte operands into three words and calls `CopyMapBlocks`, which copies
whole layout words in a 64-column working buffer, then requests updates of both
planes. Block attributes travel with those words; no collision-path or rendered
parity claim follows without decoding/observing the relevant layout.

`ExecuteMapScript`'s terminator conditionally waits for view scrolling if a
dialogue window remains, clears scroll speed and restores registers. Its debug
P2-Start text-skip state can skip Sleep commands. Neither ordinary script return
nor summed wait operands proves a measured presentation duration.

## Removal, repeat and fresh-map state

**Confirmed:** the canonical `code/common/stats/iteminventory.asm` body at
`0x4F542` owns `RemoveItemFromInventory`; the alternate `stats/items/` source
is not a second implementation. It passes item d0 as d1 to
`j_GetItemInventoryLocation` (`0x81D0` → `itemstats.asm:0x9146`). That service
calls UpdateForce and searches joined members in order, each held inventory
from reported last slot down to zero, masking entries with `$7F`. It relies on
ordinary nonempty-force and compact-inventory invariants; it does not scan all
possible storage or filter by HP, active membership or equipment status.

A match returns carrier/slot and removal calls `j_RemoveItemBySlot`
(`0x819C` → `0x8E76`). That shifts later slots left, writes Nothing in the last
slot and updates combatant stats; it reports d2=0, or d2=3 for an empty slot.
Remove-by-item returns d0=0 for removal, -1 for not found or empty-slot failure,
restoring its saved d1/d2. The Map8 handler ignores that result and writes d6=-1.
It never uses the item wrapper's selected d5 slot or selected carrier to remove.
With duplicates, the searched carrier/last matching held slot can differ from
the menu selection. There is no curse/discard-confirmation check in this path.

| Supplied or partial state | Static consequence |
| --- | --- |
| F731 set with Panel still held | Suppress opening/removal; generic no-effect menu result. A retained Panel does not reopen the puzzle. |
| F731 clear with opening blocks already supplied | Run the whole script again; no block-state idempotence check. |
| Script incomplete | No caller F731/removal/handled write yet; actors, quake or any earlier block stage can already differ. |
| Script returned, F731 set, removal fails | F731 remains set and completed handler still reports handled. No rollback or retry predicate. |
| Removal completed before wrapper/control return | Panel can be gone while menus, waits or player control remain incomplete. |
| Duplicates or changed inventory | At most the searched match is removed; selected-item use does not guarantee removal of the selected copy. |

The item wrapper closes windows, waits one VInt and physical 0's position to
match its destination, preserving d6. FieldMenu nonzero d6 exits the menu;
zero requests used-item 73/no-effect 422 and returns to the item submenu, without
automatic consumption. Only later menu exit and `ProcessPlayerAction`'s update
reactivation can reach `WaitForEvent`; pending zero permits controlled-entity
action installation. Supplied player mode and services still determine control.

`map08/3-flag-events.asm` independently selects F731 to copy `(32,14)`, size 2×2,
to `(15,19)`. `code/common/maps/mapload.asm:LoadMapBlocksAndLayout` loads the
base layout, then applies each set flag's block copy. Thus a fresh Map8 load
with F731 uses the final opening copy without replaying the scene/removal.
F731 alone need not prove a completed scene or consumed Panel. Clearing it can
restore base content on reload even after prior in-memory copies. Temporary-map
flag clearing covers 256–383, not 731, 147 or 152; persistence across save/load is
still unobserved. Chest flags separately restore their open block on map load.

## Selected shrine entry, Sword and Ribble return

**Confirmed:** Map8 `6-warp-events.asm` row 3 is wildcard X255, Y20, no scroll,
`MAP_ACHILLES_SHRINES`, destination `(42,23)`, Up. Its broad row does not make
every tile on Y20 an exit: ordinary movement in `entityscriptengine_2.asm`
first requires the target layout's masked `$1000` warp attribute.
`exploration.asm:WarpIfSetAtPoint` matches the row and writes event 1, the tuple
and warp SFX. It checks neither F731 nor Panel possession itself. Opening
requests layout replacement; actual reach from either use position remains
Unknown, including the actors' Down 2 movement and subsequent player input.

The no-scroll consumer in `explorationfunctions_2.asm` clears pending before
dispatch, plays/clears warp SFX, idles physical 0, removes the exploration return
frame and returns the explicit destination tuple to `MainLoop`. A surviving
request is required. MainLoop applies `SwitchMap` then `CheckBattle`; neither
Map8 nor Map50 has a flag-switched replacement.

Map50 **does** appear at zero-based battle row 15 in
`data/battles/global/battlemapcoords.asm`: area `(0,0,32,24)`, trigger `(255,255)`.
`code/common/maps/getbattle.asm:CheckBattle` checks map equality, F415 and
trigger coordinates, not whether `(42,23)` lies inside the battle rectangle.
F415 clear permits ordinary exploration. F415 set selects Battle15 even on the
tree-side tuple; F515 set only clears F415 after selection and does not skip
that selected battle. No F415/F515 value or prior battle completion is inferred
from possession or F731. This admission seam is the stopping point for such a
supplied battle state; its combat and outcomes are not simulated here.

For admitted exploration, Map50 is absent from `data/maps/mapsetups.asm`.
`GetCurrentMapSetup` returns `ms_Void=$FFFF`; entity-list lookup returns that
sentinel and the init/event wrappers skip a map-specific handler. It is not a
missing Map50 script to reconstruct. Shared supplied-map entry still heals
living/immortal allies, initializes main and configured followers, clears
temporary flags, loads content/chest flags and reaches the conditional
pending/control boundary. No map-specific init grants either item.

The Sword is available only when the separate `(35,6)` chest search succeeds
under the capacity/flag conditions above. Nothing here proves a walk from
`(42,23)` to that chest, from the Panel chest to the Sword chest, or between
Map50's other regions. The shared map number and battle rectangle are not
terrain evidence.

Selected Map50 return row 2 is `(42,24)`, no scroll, Map8, `(16,20)`, Down. It
has no Sword or chest-flag predicate and uses the same request/consumer seam.
Map8 has no battle row. Admitted fresh Ribble entry chooses its F708 setup,
applies F731's block copy if set, runs the existing May-related init and reaches
the conditional control boundary. The warp and reload do not remove the Sword;
conditional successful receipt puts it in the chosen carrier's inventory before
this return. Natural walking to the exit and uninterrupted custody are Unknown.

Stop at that custody boundary. The accepted Rohde talk caller searches joined
inventories for `$3D`; it does not require Bowie to carry/equip it. Its selected
item caller requires actual selection plus its own position. F147 or F731 alone
is not Rohde's possession predicate, and either can be set while the Sword is
absent. Rohde does not consume it. This investigation adds no replay, recruitment,
Battle17/18 or Caravan-operability claim.

## Reproduction, coverage and unresolved observations

Load `. ./local/private-inputs.ps1` in the same process as controlled commands.
Verify the clean pinned checkout and retained size/identity, then use the
[existing retained comparison recipe](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary):
map-init versus fixture, map-script-engine versus fixture, and `_cutscene_facts`
versus `battle-cutscenes-static-v1.json`. All pass unchanged. No new ROM/H1
extraction or runtime observation is claimed.

For direct review, select `cs_56314` and compare all 30 sourceLine/macro/arguments
against `s5_itemevents.asm`, separately checking both inline Down 2/endActions
bodies. Select `tests/fixtures/h2/map-events/item-programs.json` programs 76/77:
all ten handler operations and its shared return, with exact branch polarity,
F731/removal/d6 order and two table rows. Resolve enum values before joining
Map8/50/61, pointer tables, chest flags and destinations. Review every selected
consumer named above; aggregate equality does not prove these composed paths.

| Existing fixture under `tests/fixtures/h2/` | Retained evidence boundary |
| --- | --- |
| `map-script-engine-static-v1.json`, `map-init-static-v1.json` | Whole retained comparisons; selected script records plus explicit direct body/consumer review. No all-actor scene observation. |
| `map-events/item-programs.json`, `map-event-item-transactions-static-v1.json` | Handler/return and inventory-call shape; no acquired item, removal result or runtime atomicity. |
| `common-stats-static-v1.json`, `common-menus-static-v1.json` | Existing inventory/menu contracts, not per-chest capacity or carrier observations. |
| `map-event-request-consumption-static-v1.json`, map-data/content owners | Request and data surfaces; no natural movement, collision path or composed Map50 custody test. |

Discriminating source states include wrong item/facing/coordinates; F731 set
with Panel present; interrupted block stages; ignored removal failure; duplicate
Panel carriers; missing Peter/stale mappings; zero Bowie versus Peter HP;
unawaited Bowie motion; chest flag set with item absent or clear with duplicates;
Bowie full versus reserve capacity versus all full; malformed force/inventory;
F415/F515 combinations; pending requests and transport modes. These are static
comparisons of supplied states, not an executed scenario matrix.

**Unknown:** natural approach/search/use/warp, actual inventories and flags,
HP, logical mappings and physical lists, allocation/collision/action completion,
input and release, window/audio/VDP/quake presentation, service return values,
usable control, continuous custody and save/resume persistence. These remain
explicit under [ADR 0014](../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR 0016](../decisions/0016-remake-start-evidence-deferral.md), not an automatic
runtime queue.

Preserve [#828's original results and inspection failures](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/828#issuecomment-6160524404)
and [accepted final handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/828#issuecomment-6160569868),
including its linked PR826/822/818 failures and completed results. This slice's
inspection failures, corrected source joins and final checks belong in its
Issue/PR handoff; no completed suite is relabeled or restarted.

Acceptance is full owned-source/document/provenance/link/private review,
`git diff --check`, clean committed `uv run sf2 verify plan --base origin/main --head HEAD`
and actual final-head CI. Documentation scope runs no local blanket normal/full,
engine, Godot, H3 or H4 suite. Freeze the clean Draft PR for main-gate's independent
acceptance and integration.
