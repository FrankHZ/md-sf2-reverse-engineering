# Cotton Balloon Acquisition and the Departure to Floor World

**Confirmed:** the Desktop chest opens before inventory delivery, and Guard136
checks that chest flag rather than current item custody. Using a selected Cotton
Balloon on the player's own `(35,24)` tile dispatches a separate inventory search,
optional removal and the complete departure scene. A surviving warp conditionally
admits Map26 exploration at `(26,8)`, Down. **Inferred:** the six actors depict a
Cotton Balloon descent. **Unknown:** natural reach, actual selection/custody/state,
completed movement/services, presentation, usable control and persistence.

This English guide follows [Chessboard outcomes and Desktop return](battle22-outcomes-and-desktop-return.md).
It consumes [accepted Cotton Balloon research](../../../../research/chapters/southern-travel-creed/cotton-balloon-through-floor-world-entry.md)
under [Issue #919](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/919)
and the [current Standard agreement](../../../documentation-roadmap.md#current-baseline-and-near-term-synthesis).
It stops at conditional destination exploration-loader admission, before Floor
World population/init, interior content or Battle23 opening. Fnnn names flag nnn;
command indices are zero-based. Coordinates, speeds and waits are source operands,
not measured movement, frame durations or a proven walkable itinerary.

## Finding the Gift Is Separate From Receiving It

**Confirmed:** F522 selects the postbattle Desktop population and two layout
copies: the chest-for-chessboard change at `(25,10)` and blank-for-blank at
`(25,40)`. Full map loading applies those changes before chest overlays. The
chest record is `(25,10)`, F212, COTTON_BALLOON125 (`$7D`); the non-chest item
table is empty. Ponpei's announcement does not grant an item or set F212.
These rows alone prove neither chest visibility nor a collision-free route.

The ordinary search examines the tile **in front** of the viewed player. A's
FieldMenu SEARCH invokes CheckArea; C invokes it only after its debug, Caravan
and activated-entity alternatives do not take priority. The player first idles
and waits for destination/scroll services. A negative view target rejects
search; an alternate supplied target requires its actual indexing. A chest
layout word and matching coordinate record are necessary for the chest branch.
Ordinary exploration uses CURRENT_BATTLE=255, without battle-origin adjustment.

For this matched chest, OpenChest checks F212, then sets it and writes opened
block `$D802` **before** the found-item text or AddItem call. If F212 was already
set, it returns NOTHING and the subsequent search says the chest is empty.
An unmatched chest record produces none of these writes.

The delivery branch first tries Bowie's four slots. If Bowie is full, it builds
joined members in ascending IDs 0–29 order and, with at least two joined members,
scans from the second joined ID for the first member with fewer than four items.
This is OTHER_FORCE_MEMBERS_LIST, not a reserve-only list; neither HP nor active
battle membership gates the scan. Ordinarily Bowie is the skipped first ID.
Supplied state without joined Bowie still tries him first, then skips the first
actual joined ID. With fewer than two joined members, the other-member loop is
skipped. Coherent inventories are packed; held count covers all four slots.

AddItem writes the first masked NOTHING slot and returns success 0, or failure 1
without writing if there is no space. The caller relies on its earlier slot count
and does not test that return. Therefore successful inventory delivery means the
actual item write, not simply an opened chest, selected recipient or receipt line.
The selected branch requests item music, receipt/pass text and an input wait.
The [literal text/control table](../../../../research/chapters/southern-travel-creed/cotton-balloon-through-floor-world-entry.md#chest-appearance-search-and-inventory-delivery)
retains found/received/passed/empty/full messages, including the original spelling.

If no recipient is found, full-hands text precedes CloseChest, which clears F212
and restores `$D801`. RefillNonChestItem has no Map22 record to clear. Normal
completion permits retry; interruption before that closure can leave the chest
flag set without an item. Conversely, interruption after an item write can leave
custody without completed presentation. Repeat search of an opened chest does
not duplicate the item. There is no general rollback across these services.

## The Exit Guard Reads Chest State

**Confirmed:** Guard136 uses the shared entity-event callback in either Map22
population. Its action bits turn it toward the player and restore its saved
facing on return; DOWN in the event row is not an approach-facing restriction.
The ordinary Peter/Oddler follower example maps it to physical 13, GUARD206 at
`(33,24)`, Left, no portrait and speech bleep 5. Other allocations require actual
logical-to-physical lookup rather than that physical example.

| Supplied flags | Callback consequence if its services return |
| --- | --- |
| F212 clear | King's-permission refusal 1802; no movement or F258 write. |
| F212 set, F258 clear | Warning 1803; run cs_599A4; set F258 only after scene return. |
| F212 set, F258 set | Warning 1803 again; no second move. |

cs_599A4 has three commands: wait for Guard136's `moveUp 1; endActions`, set
its facing Down, then end. The relative action requests one tile up from actual
current position; coherent unchanged `(33,24)` requests `(33,23)`. Its wait polls
only this actor's idle pointer. Trap6 activates entity updates during the script,
but static activation does not establish completed movement. After the callback,
the wrapper restores saved facing, normally the fresh Left, superseding the
scene's Down; it closes windows and reactivates updates only as its tail returns.

F258 is temporary. An explicit fresh exploration load clears flags 256–383 and rebuilds
the guard; with F212 still set it may move again. A working-layout script reload
does not reset F258. Interrupted movement can leave changed position with F258 clear;
externally clearing F258 without rebuilding permits another relative upward request.
F212 with no current balloon still clears the guard; custody with F212 clear does
not. This callback never tests F777, F522, F778 or inventory. Its permission text
does not establish a prior audience or King's-permission flag.

## Using the Balloon Matches the Player's Own Tile

**Confirmed:** FieldMenu ITEM > USE selects held items from joined members.
Its item-summary mode is GIVE, permitting selection before the ordinary field-use
whitelist check. A member without items is rejected. Canceling the member screen
returns to the item submenu; canceling the icon choice returns to member selection.
Neither cancellation dispatches this event. Actual successful selection remains
a caller premise, not a conclusion from the event handler.

Cotton Balloon is neither Angel Wing nor an ordinary whitelisted field-use item.
It goes through map-item dispatch without a target-member question. The caller
reads physical 0's current X/Y divided by 384 and facing masked 0–3. The selected
holder's location, an NPC's tile and the search tile ahead do not supply this match.
The complete table has one explicit row and one default:

| Match | Result |
| --- | --- |
| Player X35/Y24, any facing, masked item 125 | Map22_ItemEvent0. |
| Default `$FD` row | Bare return with d6=0. |

The map-setup wrapper clears d6 and masks item index with `$7F` before matching
X, Y, facing and item in order. NO_DIRECTION=`$FF` accepts all four facings;
the exact coordinates are still required. EVENT_RELATIVE_POSITION receives the
Y byte, not another facing or target predicate. No selected setup also returns 0.

### The Handler Searches Again Before the Scene

**Confirmed:** Map22_ItemEvent0 calls GetItemInventoryLocation for 125, compares
returned d0 with -1, removes the found slot if present, then runs cs_599B2
unconditionally. It sets d6=-1 only after the scene returns. It does not remove
the menu-selected holder/slot by identity and does not test removal's d2 result.

The exact accepted aliases are `$81D0` → GetItemInventoryLocation `$9146` and
`$819C` → RemoveItemBySlot `$8E76`, whose selected bodies are in
`code/common/stats/itemstats.asm`. GetItemInventoryLocation rebuilds ascending
joined 0–29 IDs, including dead and reserve members. For each member with n held
items it checks slots n-1 down to 0, comparing masked indices. Under coherent
packed inventories, the **first joined holder's highest matching occupied slot**
wins, even if another holder/slot was selected in the menu. Other copies remain.
Caravan storage is excluded; full inventory does not prevent removal.

No match returns d0=d1=-1. Supplied holes above n-1 can hide an item, and the
zero-member loop lacks an early guard before its first iteration. These limits
are not claims of naturally reachable malformed inventories or an empty force.
RemoveItemBySlot shifts higher slots left and fills the final slot with NOTHING,
returning 0; an addressed NOTHING returns 3. Both tails update stats. The normal
matched search consumes one item without the drop action's cursed-item gate.

A direct handler invocation without a found item still starts the scene; ordinary
menu use first requires selecting a held item. Neither selected d4 alone nor
the eventual handled status proves custody. The handler checks none of flags 212/258/522/
776/777/778 flag and clears none on removal. Removal precedes the scene, so later
scene/wrapper failure does not automatically restore the item.

FieldMenu exits on nonzero d6. Default 0 produces used-item text 73, then
“But nothing happened” (422) and returns to the item submenu. A returning matched
scene's -1 skips those lines. It means handled, not proof of consumption,
completed descent or destination admission. The departure script itself has no dialogue.

## Six Independent Actors Make the Departure

**Confirmed:** both setup tables share this item event and script. Fresh loading
rebuilds entities/lookups, allocates flag-selected followers, consumes population
rows and declares Bowie0. The selected scene targets Bowie0, Peter7, B31 and
NPC137/138/139. Guard136 is separate. With only F67/Peter and F68/Oddler,
no skipped ally declaration or raft overwrite, their physical examples are
0/1/2 and 14/15/16; these numbers are not portable actor identities.

| Logical scene actor | F522-clear population | F522-set population |
| --- | --- | --- |
| Bowie0 | Main ally | Main ally |
| Peter7 | F67 follower | F67 follower |
| B31 | F68 Oddler example | F68 Oddler example |
| NPC137 | KING153, `(24,5)`, Down | OBJECT1=173, `(63,63)`, Up |
| NPC138 | QUEEN135, `(26,5)`, Down | OBJECT1=173, `(63,63)`, Up |
| NPC139 | BISHOP144, `(22,5)`, Down | OBJECT1=173, `(63,63)`, Up |

The alternate objects are already allocated fixed-sprite actors, not new items
spawned or attached to allies. The scene never replaces their mapsprites.
A baseline load instead moves King/Queen/Bishop at those IDs, not old ORB153–155.
Changing F522 after allocation affects later setup selection, not live sprites.
The [exact population/follower owner](../../../../research/chapters/southern-travel-creed/cotton-balloon-through-floor-world-entry.md#all-actors-and-the-complete-departure-program)
retains the thirty/fourteen rows and allocation premises.

Map22 is not overworld, so F65 does not prepend Caravan/A. Peter needs F67;
B aliases follow F74/Astral, F68/Oddler and F70/Astral admission order, with later
lookup writes able to replace earlier B mappings while old actors remain.
Other followers shift NPC physical slots. Absent zero lookups can resolve to
physical 0; several commands may then act on the same actor. HP/class determines
ally sprites, including BLUE_FLAME for dead exploration allies, without supplying
follower flags or revival. Retained mode/raft31 allocation can overwrite a physical
slot without repairing aliases. The ordinary alternate two-follower allocation
ends at 16, separate from 31. No scene command recruits, revives or boards a raft.

### Reading All Twenty-Nine Commands

**Confirmed:** cs_599B2 at `0x599B2` has twenty-nine top-level commands. This
grouping accounts for every index; the [exact ordered command and nested-body table](../../../../research/chapters/southern-travel-creed/cotton-balloon-through-floor-world-entry.md#all-actors-and-the-complete-departure-program)
is the operand authority.

| Indices | Ordered work and consequence |
| --- | --- |
| 0–2 | Install/wait eas_Init on Bowie, Peter, B, each separately. |
| 3–5 | Sequential setDest to Bowie `(35,24)`, Peter `(34,24)`, B `(33,24)`, each waiting for its current destination. |
| 6 | Bowie: jumpRight 1; faceDown 1; moveDown 2; endActions. |
| 7 | Peter: moveRight 1; jumpRight 1; moveDownRight 1; faceDown 1; endActions. |
| 8 | B: moveRight 1; jumpRight 1; faceDown 1; moveDown 1; endActions; wait only for B. |
| 9–11 | setPos 137 `(36,25)`, 138 `(37,24)`, 139 `(35,24)`, all Up. |
| 12–14 | stopEntity Bowie, Peter, B: stop animation counting, not travel or actions. |
| 15–20 | Six separate customActscriptWait bodies, in Bowie/Peter/B/137/138/139 order: speed 10/10; jump eas_Idle; end. |
| 21–26 | Install moveDown 6/endActions on those six in the same order; only 139 is explicitly waited. |
| 27 | Request Map26 warp to `(26,8)`, Down. |
| 28 | csc_end and the common script-return tail. |

eas_Init establishes speed 32/32, zero acceleration factors, no acceleration/
deceleration, autofacing, Up orientation, shared size 24, sprite update and idle;
it sets obstructable on, uncollidable off and immersion off. It does not set
position, HP or animation on. The alternate InitFixedSprite additionally disables
autofacing/animation. Size/orientation is not permanent world scale or geometry.

setDest is absolute tile×384 movement, not teleportation. It writes travel and
signed velocities of magnitude 32, then waits for current position to equal the
actor's **current** destination. It has no HP guard, revival or separate pathfinder.
Changing an action, destination or alias changes what the wait can complete.

The nine inline sequences use csc2D, clearing FLAGS_A bits 5/6. Their ordinary
moves write relative tile×384 destinations from actual current position, use
current speed bytes and wait for arrival. moveDownRight has two positive deltas.
The unmodified bit 5-clear destination-scan arm returns Z set, bypassing the
esc04 conflict branch; it does not establish a general collision/physics rule.
faceDown 1 updates facing/sprite and waits 1, without displacement.

jumpRight 1 instead copies the first 34 bytes of eas_JumpRight: autofacing off,
relative `(1,-1)`, wait 6, relative `(1,1)`, wait 6, relative `(0,0)`, autofacing on.
The operand 1 changes neither its distance nor waits. These are **timer** waits,
not destination-arrival waits; every relative write starts from then-current
position. The final zero displacement rewrites destination to that position.
Adding the deltas cannot prove a two-tile landing or exact trio positions. The
copied bytes exclude the source idle jump; subsequent inline commands continue
before csc2D's appended idle jump.

Waiting for B at index 8 does not join Bowie/Peter. Later action installation can replace
their unfinished actions. Each setActscriptWait/customActscriptWait polls only
its resolved actor's idle pointer, with no timeout, HP guard or all-actor join.
The action wait timer is seeded from physical index. Trap6 enables entity updates;
actual VInt progress is still a premise. Actors with X at least `$7000` are skipped
by the update loop; `(63,63)` is below that cutoff.

setPos writes current/destination coordinates and facing without resetting action,
speed or velocity. Its true-ally HP guard does not apply to NPC137–139. stopEntity
only writes animation counter -1; it skips HP0 Bowie/Peter, while B31 bypasses
that guard. Dead actors are not uniformly skipped. Custom bodies change X/Y
speed to 10/10 and jump idle; they restore neither animation nor Init/position.

The final six downward actions are installed in order, allowing concurrent progress
without a simultaneous start. Each requests `(0,+6)` from actual position and
waits for its own destination. With unchanged coherent object starts, requested
endpoints are 137 `(36,31)`, 138 `(37,30)`, 139 `(35,30)`; trio endpoints depend on
earlier motion. Completion of 139 does not prove completion of the other five.
The program contains no attachment/carry relation, mapsprite assignment, hide,
dialogue, sound, fade, camera target, story-flag write or population replacement.
Its descent framing is **Inferred**, not an observed animation or general physics contract.

## The Warp Must Survive and Be Consumed

**Confirmed:** command 27 writes MAP_EVENT_TYPE=1, PARAM1=0 and PARAM2–5=
`26,26,8,3`. This pending request does not change CURRENT_MAP or load the
destination immediately. It overwrites an older request; a later writer may
replace or clear it. It is neither a queue nor a reserved transition.

csc_end conditionally waits for scrolling if a dialogue window remains, clears
scrolling speed and returns through trap6. It adds no six-actor completion wait.
The handler then sets d6=-1. The item wrapper closes portrait/text, waits one
VInt and waits for physical 0's current destination before returning. This extra
main-actor wait does not join Peter/B/137/138 or establish all action scripts idle.
FieldMenu exits; ProcessPlayerAction restores view/entity VInts and returns.
Unreturned prefixes leave completed effects, including removal/request, without undo.

ExplorationLoop reaches WaitForEvent. A pending nonzero type takes priority over
installing a controlled action on physical 0. ProcessMapEvent clears the sampled
event type before dispatch; type 1/PARAM1=0 submits/clears WARP_SFX, idles physical 0,
unwinds exploration and reads the tuple. The scene does not write WARP_SFX,
so the warp alone selects no particular sound. Its nonnegative X26/Y8 replace
fallback low bytes and facing 3 is read directly; arbitrary supplied negative/
high-byte state is not normalized into a guaranteed coordinate by these writes.

The surviving return enters MainLoop's normal SwitchMap **then CheckBattle** path,
unlike Battle22's immediate outcome return. The complete three-row flag-switched
map table has no Map26 substitution. All forty-five battle rows require a map
match; none names 26. CheckBattle returns d7=-1 and selects exploration for
`(Map26,26,8,Down)`. Battle22 uses Map22; Battle23 uses Map27. Their unlock,
intro/completion or F88 suspension flags cannot manufacture a Map26 battle row.
The selected scene also leaves EGRESS_MAP unchanged. A replaced request follows
its own consumers and cannot be cited as this Floor World admission.

This is conditional destination **loader admission**, before population/init.
It proves no Floor World interior, immediate encounter, usable control or Battle23
opening. [Floor World first arrival and conditional exploration](floor-world-first-arrival.md)
continues from this seam using accepted #920 research: fresh population, the complete
landing/pop scene, F779/EGRESS and independent F780 parking, then conditional control.
That guide preserves supplied-state, service and presentation limits and stops
before residents, the giant ring, searches/exits and Battle23. These later consumers
remain outside this departure guide; the navigation link does not broaden its claims.

## Evidence Review and Remaining Limits

**Confirmed provenance:** accepted main `63fab597ee78136cd88f7cdefa2bc6c4512cf6b5`
contains [#916/PR918 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/918#issuecomment-6175709354)
and [#915/PR917 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/917#issuecomment-6175632640).
Research pins [SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm)
and USA ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
This synthesis adds no source reacquisition, extraction or runtime observation.

Adversarial review checks the complete accepted research, return/interaction/
population owners, [authored program contract](../../../contracts/world/standalone-map-script-program-data.md),
and selected public fixture/schema/verifier boundaries:

| Existing fixture / reproduction command | What it binds and what this guide additionally consumes |
| --- | --- |
| [sf2-map-event-item-transactions-static-v1](../../../../../tests/fixtures/h2/map-event-item-transactions-static-v1.json), `uv run sf2 h2 map-event-item-transactions` | Table/handler, exact aliases, location sentinel, removal call and d6 caller seam; callee algorithms and actual custody require the accepted local join. |
| [sf2-map-event-request-consumption-static-v1](../../../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json), `uv run sf2 h2 map-event-request-consumption` | Poll/reset/pre-dispatch-clear topology; selected type/tuple and later admission are direct research joins, not natural reach. |
| [sf2-map-init-static-v1](../../../../../tests/fixtures/h2/map-init-static-v1.json), `uv run sf2 h2 map-init` | Selected setup/init structure; allocated followers, live population and chest delivery are separate premises/consumers. |
| [sf2-map-script-engine-static-v1](../../../../../tests/fixtures/h2/map-script-engine-static-v1.json), `uv run sf2 h2 map-script-engine` | Handler-local semantics and program structure; exact 3/29 commands, nine inline and six custom bodies plus caller/action returns belong to the complete accepted selection. |

Their existing schemas/verifiers are
[transactions schema](../../../../../schemas/h2/map-event-item-transactions-static-fixture.schema.json)/[verifier](../../../../../src/sf2tool/h2/map_event_item_transactions.py),
[consumption schema](../../../../../schemas/h2/map-event-request-consumption-static-fixture.schema.json)/[verifier](../../../../../src/sf2tool/h2/map_event_request_consumption.py),
[init schema](../../../../../schemas/h2-map-init-static-fixture.schema.json)/[verifier](../../../../../src/sf2tool/h2/map_init.py),
and [script schema](../../../../../schemas/h2/map-script-engine-fixture.schema.json)/[verifier](../../../../../src/sf2tool/h2/map_script_engine.py).
The transaction fixture stores sourceContext/eventItemTransactions at top level;
its grouped Unknowns and aggregates do not verify the service bodies or natural selection.
The authored-program contract owns data, not movement execution or rendering.

The [transaction overview's per-service ownership](../../../../research/world/map-event-item-transactions.md#ownership-and-unknown-boundary)
and its fixture agree with #916's exact resolution: GetItemInventoryLocation and
RemoveItemBySlot belong to `code/common/stats/itemstats.asm`; ReceiveMandatoryItem
and RemoveItemFromInventory belong to `code/common/stats/iteminventory.asm`.
The alternate `code/common/stats/items/itemfunctions_s7_0.asm` remains excluded.
The overview retains its caller-only boundary; accepted #916 research owns the
callee algorithms used here. Runtime-question queues remain deferred under ADR0014/0016.

Preserve [#916's handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/916#issuecomment-6175681254):
original head `9aaa845048a3be6d3c9011849e1f832c220ecde4`, rebased head
`48d5175f0027a2a01088d171a2aaca02260da8a8`,
[original CI38616821310](https://github.com/FrankHZ/md-sf2-reverse-engineering/actions/runs/38616821310)
and [rebased CI38617104204](https://github.com/FrankHZ/md-sf2-reverse-engineering/actions/runs/38617104204).
Both scope/research-public checks succeeded; engine/adapter/H4 were skipped.
The unchanged 18,285,979-byte pair comparison passed in 0.081s. Its oversized reads,
guessed source/config/corpus paths, literal-wildcard repairs, fixture `expected`
KeyError and URL-matching private-regex AssertionError remain with their corrections.
Its linked predecessor objects, results and inspection/transport failures remain
at those receipts; this guide does not rerun or replace them.
[#915's handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/915#issuecomment-6175613847)
retains separate scope-only [CI38616608492](https://github.com/FrankHZ/md-sf2-reverse-engineering/actions/runs/38616608492)
and its preserved inspection/navigation/check failures and linked predecessors.

Direct acceptance reviews complete prose/diff, every command group and quantity,
coordinates/identities/wait scopes, provenance, links/fragments, English/private
boundary and whitespace. Inspect the configured clean committed dependency planner
and actual final-head CI. No local normal/full/engine/Godot/H3/H4 or translation
suite is selected solely for prose; the research's
[reproduction boundary](../../../../research/chapters/southern-travel-creed/cotton-balloon-through-floor-world-entry.md#reproduction-checks-and-retained-limits)
continues to own its comparisons and inputs.

**Unknown:** natural chest/guard/item-use reach and custody/selection; actual flags,
force/inventory/HP/class, follower/actor/raft aliases, layout/current-saved positions;
completed input/text/actions/scroll/sprite/transition services; physical motion,
visual/audio presentation, usable control and persistence. Counterexamples here
are supplied source states, not executed scenarios or an automatic runtime queue
under [ADR0014](../../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../../../../decisions/0016-remake-start-evidence-deferral.md).
This completes neither the campaign nor [epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
