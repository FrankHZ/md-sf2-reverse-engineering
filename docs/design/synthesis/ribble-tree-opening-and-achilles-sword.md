# Ribble's Tree Opening and the Achilles Sword

The accepted item content supports a puzzle: obtain a Wooden Panel, use it facing the tree in
Ribble, then search a separate shrine chest for the Achilles Sword. The opening requests jumps,
downward movement, shaking and three block replacements before marking the tree open and attempting
to remove a Panel. The Sword can then satisfy Rohde's initial possession check in Hassan.

**Inferred:** this is the intended Panel/tree/Sword connection. **Confirmed:** the selected chest,
item-use, scene, inventory and map-request rules below come from the accepted
[research owner](../../research/ribble-tree-opening-and-achilles-sword.md). **Unknown:** natural
approach, a walk through the shared shrine map, actual scene/service completion, rendered opening,
continuous custody, usable control and save/resume persistence. This is a conditional gameplay-content
explanation, with no mandatory order relative to numbered battles or Hassan's conversations.

Fnnn means flag nnn; coordinates are map tiles; record indices are zero-based. Map8 is Ribble,
Map50 is `MAP_ACHILLES_SHRINES`, and Map61 is the distinct `MAP_RIBBLE_SHRINE_EXTERIOR`.
The shared Map50 identity does not establish a walk between its separate regions. Read
[Ribble's welcome](battle09-outcomes-through-ribble-welcome.md) for town setup and
[Hassan's Rohde choice](hassan-ancestry-and-rohde-caravan-choice.md#talking-to-rohde) for the later
possession caller. Neither is a newly imposed prerequisite.

## Two Chests, Two Separate Custody Decisions

**Confirmed selected content:** Map50's chest table has these rows. Neither tests F731, possession
of the other item, Rohde's flags, party membership or a battle completion.

| Physical row | Search candidate | Item offered while flag clear | Chest flag |
| --- | --- | --- | --- |
| 1 | `(14,2)` | Wooden Panel `$70` | F152 |
| 2 | `(35,6)` | Achilles Sword `$3D` | F147 |

These coordinates identify chest candidates, not places where merely standing acquires an item.
FieldMenu Search calls `CheckArea` and exits the main menu. Ordinary C interaction first waits for
physical 0 and scrolling, then can select a nearby actor; only its no-actor branch searches the area.
For the ordinary view target 0, search derives a **facing-offset tile**, tests the working layout's
chest attribute `$1800` under mask `$3C00`, and looks for an exact chest-table coordinate. A negative
view target is rejected. The accepted research does not generalize this calculation to arbitrary
supplied nonzero view targets. Its selected exploration boundary is outside battle.

`OpenChest` checks the matching flag. Clear offers the row's item; set substitutes Nothing. It then
sets the flag, writes the open block and requests a view update **before item receipt**. Opened text
403 is followed by empty text 408 for Nothing. An open flag can coexist with no held item; conversely,
a clear flag can offer another copy when one is already held. These rows enforce no unique-item rule.

Both items use the ordinary capacity path, with four slots per carrier:

| Supplied capacity | Conditional result when the called services return |
| --- | --- |
| Bowie has room | Found text 413; add to Bowie's first empty slot; request item music and receipt text 415; shared input/fade wait and window closure. The chest flag stays set. |
| Bowie full, another joined member has room | Rebuild force lists; scan other joined members in ascending ally order; add to the first with room; music and passed-item text 416; same completion tail. Flag stays set. |
| All available carriers full | Full-hands text 417; restore saved candidate coordinates; close/refill. `CloseChest` clears the flag, restores the closed block and requests a view update. No item is added. |
| Flag already set or no matching row | Empty result, without an inventory-capacity scan or grant. |

The other-member search includes joined reserves and does not filter by HP. It relies on the ordinary
list invariant that joined Bowie is first; the initial Bowie capacity check is hardcoded even for
inconsistent supplied membership. This is no inventory-choice prompt or forced-discard transaction.
`AddItem` masks the entry with `$807F`, so receipt does not automatically equip it. Although the
caller checks capacity first, it ignores the later insertion result; unchanged ordinary inventory
makes those steps agree, without an atomic guarantee for altered or malformed state.

The full-capacity tail restores availability when it completes. Map50's non-chest item table is
terminator-only, so the tail has no separate non-chest item at either selected coordinate to restore.
Interruption after the open write can still leave F152/F147 set without custody, or custody before
music/text/input completion. Chest availability, carrier choice and presentation are distinct effects.
No external approach to the Panel chest or path from it to the Sword chest is supplied here.

## Using the Panel at the Ribble Tree

**Confirmed admission:** FieldMenu Item → Use selects a carrier, slot and item. Panel is absent
from the ordinary field-usable allowlist, so it enters the map-item-event branch. The event compares
the masked selected ID `$70` and **physical 0's current position and facing**:

| Map8 item row | Current player tile | Required facing | Target |
| --- | --- | --- | --- |
| 0 | `(15,19)` | Up | `Map8_ItemEvent0` |
| 1 | `(16,19)` | Up | `Map8_ItemEvent0` |

This uses neither search's forward tile nor the selected carrier's location. Holding a Panel
somewhere in the force is insufficient; cancellation never invokes the event. Wrong coordinates,
facing or item reach the shared default return. Default and F708 town setups share these rows;
F708/F709 are not additional Panel-use gates. Peter's presence is not tested for admission.

With F731 already set, the handler returns d6=0 without opening or removing anything. With it clear,
the complete order is:

1. Execute the opening script `cs_56314`.
2. **After script return**, set F731.
3. Search joined-force inventory afresh and attempt removal by item identity Wooden Panel.
4. Ignore that removal result; set handled d6=-1 and return.

No chest flag or preexisting block-state test replaces F731. The handler's ten physical operations
and shared return are separate from the embedded script's 30 records.

## What the Opening Scene Requests

**Confirmed complete static program:** the following accounts for all 30 records of `cs_56314`
and its two inline movement bodies. It contains no text sites or internal jumps. Script invocation
activates entity updates before interpretation despite the menu's earlier deactivation.

| Records | Ordered scene work |
| --- | --- |
| 0–4 | Hide text/portrait; wait for Bowie `eas_Init`, then Peter `eas_Init`; quake 2; Sleep 60. |
| 5–10 | Start Bowie's jump without waiting; wait for Peter's jump; start Bowie Down 2 without waiting; wait for Peter Down 2; face Bowie Up, then Peter Up. Both inline bodies end with `endActions`. |
| 11–15 | Request `SFX_BATTLEFIELD_DEATH`; copy source `(32,17)`, size 2×2, to `(15,19)`; Sleep 30; quake 0; Sleep 30. |
| 16–21 | Quake 2; same SFX; copy `(34,17)`, size 2×2, to `(15,19)`; Sleep 30; quake 0; Sleep 30. |
| 22–27 | Quake 2; same SFX; copy `(32,14)`, size 2×2, to `(15,19)`; Sleep 30; quake 0; Sleep 30. |
| 28–29 | Request Peter→Bowie following, selector 2; end. Only afterward does the caller set F731 and attempt Panel removal. |

`eas_Init` sets speed 32/32, disables acceleration/deceleration, restores map/entity collision and
auto-facing, clears immersion, sets size 24 and updates the sprite before idle. `eas_Jump` disables
auto-facing, requests three relative offsets with two Wait 5 commands, restores auto-facing and idles.
Each Down 2 body installs a relative-destination action. Wait forms poll **that mapped actor's idle
script pointer**; they guarantee no all-actor barrier or unobstructed movement. Peter's wait does
not prove Bowie's previous action finished before its next installation replaces it.

Action installation does not check HP or presence. It resolves current logical-to-physical mappings;
a missing/default-zero or stale Peter mapping can address the wrong physical actor. Facing commands
use the ally byte and can skip zero-HP Bowie/Peter. The word-encoded Peter follower operand instead
has a zero high byte: the follow handler tests **Bowie's HP**, not Peter's. Dead Bowie can skip that
request while the returning caller still marks F731 and handled use. No actor is created or force
member joined by this script.

An admitted follow request resolves the current actors and selector 2's `(-24,0)` offset, installs a
copied 42-byte follower action and appends the physical follower if absent. Avoiding duplicate list
entries does not avoid action allocation. This does not set F67, repair mappings, prove allocation
capacity or establish settled following; fresh Ribble population is a separate operation.

Each block copy replaces whole layout words in the 64-column working buffer and requests both view
planes, retaining their attributes. Quake 2/0 writes amplitude; the scrolling consumer requests random
horizontal/vertical offsets and VDP updates. These are source operations, with rendered shaking,
collision access and audible SFX still Unknown. Sleep operands are not measured elapsed time, and
the debug text-skip state can skip them. Script termination's conditional scroll wait and register
restoration prove neither presentation completion nor responsive player control.

## Consuming the Panel, Repeating and Reloading

**Confirmed removal boundary:** `RemoveItemFromInventory` calls `GetItemInventoryLocation`, which
rebuilds the joined force and searches members in order, each inventory from its reported last held
slot down to zero, comparing masked IDs. It uses ordinary compact-inventory/nonempty-force
invariants; neither active membership, HP nor equipment state filters the search. A match removes
that slot, shifts later entries left, writes Nothing in the last slot and updates stats.

The selected menu carrier/slot is **not** the removal target. With duplicates it may remove another
carrier's copy or a different held slot. A failure returns a failure result, but the Map8 caller
ignores it. This path adds no curse/discard confirmation and provides no rollback.

| Supplied or interrupted state | Consequence at the static boundary |
| --- | --- |
| F731 set, Panel still held | No opening/removal; zero-result menu fallback. |
| F731 clear, final opening blocks already supplied | Request the entire scene again; no block-based idempotence test. |
| Script has not returned | Caller has not yet set F731/removal/handled; earlier actors, quake and block stages can already differ. |
| Script returned, F731 written, removal fails | F731 remains set and d6 still becomes -1. No retry/rollback repairs custody. |
| Removal complete, wrapper not returned | Panel can be absent while waits/menu/control are unfinished. |

The item wrapper closes windows, waits one VInt and physical 0's position/destination equality,
preserving d6. Nonzero d6 exits FieldMenu. Zero requests used-item text 73/no-effect 422 and returns
to the item submenu without automatic consumption. Later menu exit/update reactivation reaches the
pending-event/control seam; supplied mode, pending state and returning services still matter.

Fresh Map8 loading independently applies the F731 flag-event copy `(32,14)`, size 2×2, to `(15,19)`
after base layout loading. It requests the final open state without replaying the scene or removal.
F731 alone therefore proves neither a completed scene nor consumed Panel. Clearing it can restore
base content on a fresh reload despite prior in-memory copies. Chest flags independently restore
open chest blocks. Temporary flag clearing 256–383 excludes F731/F147/F152, while durable save/resume
persistence remains unobserved.

## The Shrine Request and Conditional Sword Return

**Confirmed selected request:** Map8 warp row 3 uses wildcard X255, Y20, no scroll, Map50,
destination `(42,23)`, Up. Ordinary movement must first encounter the working-layout warp attribute
`$1000` under mask `$3C00`; the broad row does not make every Y20 tile an exit. It writes the
request tuple and warp SFX without checking F731 or Panel possession. Opening replaces layout
words, but natural access from either use position remains Unknown.

For a surviving no-scroll request, the consumer clears pending, plays/clears warp SFX, idles
physical 0 and returns the explicit tuple to MainLoop. Map switching precedes battle selection;
Map8/Map50 have no flag-switched replacement. The tree-side tuple does not itself guarantee
exploration: Map50 has battle row 15, area `(0,0,32,24)`, **wildcard trigger `(255,255)`**.
`CheckBattle` tests map, F415 and trigger, not whether `(42,23)` lies in that rectangle.

| Supplied entry flags | Admission result |
| --- | --- |
| F415 clear, either F515 value | No Battle15 candidate; admit the ordinary exploration branch. |
| F415 set, F515 clear | Select Battle15. Stop this branch at admission. |
| F415 set, F515 set | Select Battle15, then clear F415; completion does not cancel the already-selected admission. Stop here too. |

Panel/Sword possession and F731 establish neither flag value nor an earlier battle completion.
Combat and outcomes are outside this guide. When exploration is admitted, Map50 has no map-specific
setup and uses `ms_Void=$FFFF`; init/event wrappers skip such a handler rather than selecting an
unreconstructed shrine scene. Shared entry still heals living/immortal allies, initializes main and
configured followers, clears temporary flags and loads map/chest content before conditional control.
No map-specific init grants either item.

The Sword chest at `(35,6)` remains its own facing-offset search/capacity boundary. The accepted
content does not prove a walk from `(42,23)` to it, from the Panel region to it, or between other
Map50 regions. Shared storage and battle-area bounds do not reconstruct terrain.

Selected return row 2 requires `(42,24)`, no scroll, Map8, destination `(16,20)`, Down. It has no Sword
or chest-flag test. If the request survives, the same consumer reaches Ribble without a Map8 battle
row; fresh entry chooses the flag-selected town setup, applies F731 when set and runs the accepted
May-related init before conditional control. The warp/reload does not remove the Sword. **Conditional
successful receipt** gives custody to the chosen carrier before this return; natural exit walking,
uninterrupted custody and usable input are still Unknown.

That custody can satisfy Rohde's initial talk search of joined inventories, including reserves;
Bowie need not carry or equip the Sword. Rohde's selected-item route instead needs actual Sword
selection and its own `(16,58)` predicate. F147/F731 are not possession checks, and Rohde does not
consume the Sword. See the [separate Hassan guide](hassan-ancestry-and-rohde-caravan-choice.md)
for his choices. This guide stops at conditional Ribble return/custody and that cross-reference,
before external shrine approach, Rick, Battle17/18, Rohde replay or Caravan operation.

## Evidence Trace and Review Limits

**Confirmed provenance:** the accepted research binds SF2DISASM master
`c834c652b6862bc5679fd7f69a38a7093206efc6` and the registered USA retail ROM. Its
[reproduction/coverage section](../../research/ribble-tree-opening-and-achilles-sword.md#reproduction-coverage-and-unresolved-observations)
owns the complete source/body/consumer joins and unchanged retained comparisons. This guide consumes
those results; it performs bounded tracked-fixture review without new source investigation,
payload generation, runtime acquisition or remake implementation.

| Explained boundary | Existing contract, fixture and consumer trace | Remaining limit |
| --- | --- | --- |
| Search and chest receipt | [exploration control](../contracts/exploration-control-flow.md), [map exploration](../contracts/map-exploration.md); accepted research's `CheckArea`/`GetItem`/`OpenChest`/`CloseChest`/capacity/`AddItem` joins; `sf2-common-menus-static-v1` and `sf2-common-stats-static-v1` in [menu](../../../tests/fixtures/h2/common-menus-static-v1.json)/[stats](../../../tests/fixtures/h2/common-stats-static-v1.json) fixtures | Shared inventory/menu facts are not observed per-chest grants, carrier capacity or atomicity. |
| Item selection and handler | `sf2-map-events-static-v1`, [item-program shard](../../../tests/fixtures/h2/map-events/item-programs.json) programs 76/77; [item transactions](../../research/map-event-item-transactions.md), `sf2-map-event-item-transactions-static-v1` [fixture](../../../tests/fixtures/h2/map-event-item-transactions-static-v1.json) | Ten handler operations/shared return and caller service/d6 shape do not prove natural item selection or actual removal. |
| Complete opening and fresh blocks | [map exploration](../contracts/map-exploration.md), [roster](../contracts/party-roster-state.md); `sf2-map-script-engine-static-v1` [fixture](../../../tests/fixtures/h2/map-script-engine-static-v1.json), `sf2-map-init-static-v1` [fixture](../../../tests/fixtures/h2/map-init-static-v1.json); accepted csc15/2D/23/2C/33/34, action/follow/block/quake consumers | Tracked family projections are distinct from the research's full 30-record programCorpus/two inline bodies and fresh flag-table joins; none proves all-actor completion or presentation. |
| Shrine admission and return | [map-entry routing](../contracts/map-entry-routing-state.md), [exploration flow](../contracts/exploration-control-flow.md); `sf2-common-maps-static-v1` [fixture](../../../tests/fixtures/h2/common-maps-static-v1.json), `sf2-map-event-request-consumption-static-v1` [fixture](../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json); accepted warp/consumer/default-setup joins | Helper/request facts do not prove a surviving natural request, continuous terrain path, current flags, custody or usable control. |

The [map-event fixture loader](../../../src/sf2tool/h2/map_events_fixture.py) and
[item-program schema](../../../schemas/h2/map-events-item-programs.schema.json), with its
[target-operation schema](../../../schemas/h2/map-events-target-program.schema.json), own the
selected operation graph. The [map-script verifier](../../../src/sf2tool/h2/map_script_engine.py)
and [schema composition](../../../src/sf2tool/h2/map_script_schema.py) own the command-fact
projections; [item](../../../src/sf2tool/h2/map_event_item_transactions.py) and
[request](../../../src/sf2tool/h2/map_event_request_consumption.py) verifiers own caller/request
facts. Owning commands include `uv run sf2 h2 map-events`, `map-script-engine`, `map-init`,
`common-menus`, `common-stats`, `common-maps`, `map-event-item-transactions` and
`map-event-request-consumption`. Controlled reproduction loads ignored private-input configuration
in the same process; prose review alone does not require those extraction runs.

This synthesis reviews programs 76/77, selected opening command/block/action/follow projections,
shared inventory/menu/routing and schema/consumer limits against accepted research. Aggregate
comparison success is not a composed Panel→opening→Sword→Ribble observation. No Layer A conflict
requires a new claim or correction here.

The [PR833 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/833#issuecomment-6160733380)
and [independent acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/833#issuecomment-6160756298)
preserve actual retained comparisons, CI and corrected inspection/check failures. Its linked
[#828 original](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/828#issuecomment-6160524404)
and [final handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/828#issuecomment-6160569868)
retain PR826/822/818 results/failures. The
[#831 corrected handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/831#issuecomment-6160802747)
separates Rohde's initial 13 text sites from its repeat 2; those counts do not belong to this text-free
opening program. None of these completed runs is restarted or relabeled by this guide.

**Unknown:** natural approach/search/use/warp, supplied inventories/flags/HP/mappings/lists,
collision/allocation/action and service completion, input/release, audio/windows/VDP/quake
presentation, usable control, continuous custody and save/resume persistence. These retain their
[ADR0014](../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)/
[ADR0016](../../decisions/0016-remake-start-evidence-deferral.md) boundary rather than becoming a
runtime queue or an expanded campaign/remake claim.
