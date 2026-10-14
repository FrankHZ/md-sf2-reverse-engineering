# Cotton Balloon through conditional Floor World entry

Evidence date: 2026-10-14. **Confirmed:** the Desktop chest records F212 before
inventory delivery; the exit guard uses F212/F258, not present item custody or
F777. The item-event caller matches the player's own `(35,24)` tile and Cotton
Balloon, then the handler independently searches joined members, conditionally
removes one item and runs `cs_599B2`. Its surviving warp requests Map26 at
`(26,8)`, Down, which has no battle-table entry. **Inferred:** the six-actor
sequence depicts a Cotton Balloon descent. **Unknown:** natural reach, actual
custody/state, completed movement/services, presentation, control and persistence.

## Agreement and provenance

Standard rigor under [Issue #916](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/916)
and [content epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
The uncertainty is in source-to-caller and action-to-return joins for one local
progression outcome. Accepted starting base is
`546ccbb21ab1e9da42f5899fe7eae86ead18d255`; no stacked dependency. This document
is the sole owned path. No index, registry, fixture, schema, design, contract,
tooling or remake change is required. Stop at conditional destination admission,
before Floor World population/init, interior content or any Battle23 opening.

Consume accepted [Battle22 outcomes](./battle22-outcomes-and-desktop-return.md),
[Desktop interactions](./desktop-kingdom-interactions-through-battle22-entry.md),
[first arrival](./desktop-kingdom-first-arrival.md),
[item transactions](./map-event-item-transactions.md) and
[request consumption](./map-event-request-consumption.md).
These owners retain their own limits; this document joins only the selected
chest, guard, item event, complete departure scene and immediate consumers.

All source paths below are relative to `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at that pin. Retained evidence binds USA ROM
SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Short setup names mean `data/maps/entries/map22/mapsetups/`; map-script handlers
are in `code/common/scripting/map/mapscriptengine_1.asm` or `_2.asm`.
Fnnn means flag nnn. Command indices are zero-based; coordinates, speeds and
waits are source operands, not measured trajectories or durations.

Reuse the unchanged 18,285,979-byte map-init/map-script-engine pair and small
selected sources/fixtures. Linear comparison is budgeted below 512 MiB and
one minute, plus small prose/receipts; pair and cutscene comparison passed in
0.081 seconds. No new extraction, environment, emulator, bulk capture or
verification infrastructure. Reassess before material resource/scope expansion.

## Chest appearance, search and inventory delivery

**Confirmed:** the complete `data/maps/entries/map22/3-flag-events.asm` has two
F522 copies: `(39,0)`, 1x1 to `(25,10)`, and `(40,0)`, 1x1 to `(25,40)`.
The source comments identify chest-for-chessboard and blank-for-blank changes.
Full map loading applies these before chest overlays. The entire
`7-chest-items.asm` is `mapItem 25,10,212,COTTON_BALLOON`, then endWord;
`8-other-items.asm` contains only endWord. F522 chooses the layout/population,
not chest custody. Ponpei's announcement neither calls AddItem nor sets F212.
A coordinate/data row alone does not prove a visible or walkable chest.

Actual search begins in `code/gameflow/exploration/explorationvints.asm`:
ProcessPlayerAction idles physical0, waits for its destination and view scroll,
then handles A or C. A opens FieldMenu; its SEARCH arm clears d6 and calls
j_CheckArea. C first handles debug/overlapping-Caravan/entity alternatives;
only no activated actor sets d6=1 and calls CheckArea, potentially falling back
to the menu. An entity interaction is not simultaneously a chest search.

`explorationfunctions_0.asm:CheckArea` rejects a negative view target. In the
ordinary target0 case it offsets the viewed actor's current position by its
facing, divides by MAP_TILE_SIZE=384, and reads the working layout word.
Only `(word & $3C00)==$1800` takes the chest path. This is the tile in front
of the player, unlike the item-use row below. Nonzero supplied camera targets
must use the actual source indexing; this review establishes no alternate-view
search itinerary. The empty Map22 area-description callback does not grant it.

`exploration.asm:GetChestItem` selects CURRENT_MAP's chest table. GetItem scans
four-byte coordinate records to their sentinel; in ordinary exploration,
CURRENT_BATTLE=255 prevents battle-origin adjustment. A matching `(25,10)`
returns layout offset, flag212 and item125 (`$7D`). No match returns -1 values.
OpenChest checks F212: already set replaces the returned item with NOTHING;
otherwise it keeps125. Both matched paths set212, write opened block `$D802`
and request the appropriate plane update, **before** text/inventory delivery.
No match skips these writes. A supplied non-exploration state has a separate
battle-offset path, not this accepted Desktop search.

`explorationfunctions_1.asm:loc_239C8` handles the returned item. Cotton Balloon
is below the gold-chest range, so it follows this complete delivery branch:

1. Display found-item text, then count Bowie's four slots with
   GetItemBySlotAndHeldItemsNumber. If below4, call AddItem for Bowie.
2. If Bowie is full, UpdateForce rebuilds joined IDs in ascending0–29 order.
   OTHER_FORCE_MEMBERS_LIST is TARGETS_LIST+1 (`$FFB653` versus `$FFB652`),
   not the reserve-party list. With N at least2 joined members, the DBF counter N-2
   scans N-1 entries starting at the second joined ID, selecting the first
   with fewer than4 items; fewer than2 joined members skips that loop.
   It does not test HP or battle-party membership.
   Normal joined Bowie is the skipped first ID; a supplied force lacking
   Bowie still tries Bowie first and then skips the first actual joined ID.
3. AddItem (`code/common/stats/itemstats.asm`) scans four slots for the first
   masked NOTHING, writes the item with mask `$807F`, returns d2=0; no space
   returns1 without writing. The search caller relies on its earlier count
   and does **not** branch on AddItem's result. Successful custody requires
   that actual write; a completed announcement alone is insufficient.
4. A successful selected branch requests MUSIC_ITEM, displays receipt/pass
   text and calls FadeOut_WaitForP1Input, then closes text. With no recipient,
   show full-hands text, call CloseChest at the saved candidate coordinates,
   then RefillNonChestItem, and close text. CloseChest clears212 and restores
   `$D801`; Map22's empty non-chest table gives the latter no flag to clear.

Repeat search of a matching opened chest produces empty text, not a duplicate
item. Full-party failure ordinarily restores the closed chest/F212-clear state
only after its text and close calls return. Interruption before delivery can
leave212 set without custody; interruption after an actual item write can leave
custody without completed presentation. There is no general transaction rollback.
Coherent inventories are packed; the helper counts non-NOTHING across all four
slots. Full does not mean all characters are alive or in the active battle party.

Literal controls from `data/scripting/text/gamescript.txt` are:

| ID / key | Exact text |
| --- | --- |
| 403 /0193 | `{NAME} opened the chest.{W2}{CLEAR}` |
| 408 /0198 | `But, it was empty.{W1}` |
| 413 /019D | `{NAME} found{N}{ITEM}.{W2}{CLEAR}` |
| 415 /019F | `{NAME} recieved{N}{ITEM}.` |
| 416 /01A0 | `{NAME} passed the{N}{ITEM} to {NAME}.` |
| 417 /01A1 | `But, everyone's hands are{N}full.{W1}` |

CheckArea sets name1=Bowie0 and speech0; found/receipt uses item name2,
and the pass branch puts its recipient in name3. The original `recieved`
spelling is retained. These are requests and controls, not observed output.

## Guard136: chest flag, temporary clearance and wrapper facing

**Confirmed:** both Map22 setup tables share `s2_entityevents.asm`. The row
`msEntityEvent 136,DOWN,Map22_EntityEvent10-...` uses DOWN=3 as action bits,
not required approach facing. The accepted interaction caller reverse-maps the
activated physical actor to logical136. GetEntityAddressFromCharacter subtracts
96 for NPC IDs and reads ENTITY_INDEX_LIST; ordinary two-follower allocation
makes Guard136 physical13 in either population. Changed followers/aliases
require the actual lookup. GUARD206 selects no portrait and speech bleep5.

The complete handler checks212 first. Clear: text1802, then return. Set:
text1803, check258; set returns, clear runs cs_599A4 and sets258 **after** its
return. There is no F522, F777, F778 or inventory predicate in this callback.

| ID / key | Exact text |
| --- | --- |
| 1802 /070A | `Halt!  I can't let you{N}pass without the King's{N}permission.{W1}` |
| 1803 /070B | `Floor World is very{N}dangerous.  Take care!{W1}` |

`scripts.asm:24–28`, cs_599A4 at `0x599A4`, has three top-level commands:
entityActionsWait136 containing `moveUp 1; endActions`; setFacing136,Down;
csc_end. The inline action compiles to relative `(0,-1)`, destination wait,
then idle jump. Under unchanged coherent mapping/current `(33,24)`, its
requested destination is `(33,23)`; a later invocation starts from the actual
current position, not an absolute reset. Its wait polls only that actor's idle
pointer; the destination-scan bypass described below applies here too.

The entity-event wrapper turns the guard toward the player before dispatch,
saving the original facing. Its bit1 restoration runs **after** the callback,
so the scene's Down is not the promised final facing: normal fresh Left is
restored on wrapper return. The wrapper then closes portrait/text and activates
entity updates. The `script` macro invokes trap6, which activates
VInt_UpdateEntities before ExecuteMapScript even though the surrounding callback
had cleared its update flag. Those service returns remain prerequisites.

F258 suppresses a second move within the same load but not warning1803.
An explicit fresh exploration load clears temporary256–383 and rebuilds the
guard at its selected row; later dialogue can move it again if212 remains set.
A map-script reload of the existing layout does not reset258. Interrupted motion
can leave moved position with258 clear; externally clearing258 without rebuilding
can request another relative upward step. F212 set without a current item still
admits clearance; an item with212 clear does not. The text's King permission is
narrative framing, not an F777 test or evidence of a conversation with Ponpei.

## Actual item-use caller, match and consumption

**Confirmed:** `code/common/menus/main/mainactions.asm:FieldMenu` ITEM > USE
rebuilds the generic list from all joined force members. It invokes the item
summary screen with CURRENT_ITEM_SUBMENU_ACTION=GIVE and SELECTED_ITEM_INDEX=
NOTHING: that mode permits choosing held items rather than applying the ordinary
field-use whitelist at member selection. `memberslistscreen.asm` rejects a
member with no items, processes the item selection and returns member/slot/item;
cancelling the member screen returns to the item submenu; cancelling the icon
choice returns to member selection, both without this event. Actual successful selection
and custody remain caller premises, not facts inferred from the handler.

Cotton Balloon is not Angel Wing and is absent from the complete
`data/stats/items/usableoutsidebattleitems.asm`: Antidote, Fairy Powder, Power
Water, Protect Milk, Quick Chicken, Running Pimento, Cheerful Bread, Bright Honey,
Brave Apple, terminator. `menus/item/isitemusableonfield.asm` therefore returns
-1, taking map-event dispatch rather than the ordinary target-member use path.
It does not ask which character to use the balloon on.

GetPlayerEntityPosition in `code/common/menus/getplayerentityposition.asm`
reads physical0's **current** X/Y, divides by384, masks facing to0–3. FieldMenu
supplies these in d1/d2/d3, selected item in d4 and slot in d5 to
j_RunMapSetupItemEvent. These are the player's own tile/facing, not the searched
tile ahead, the selected item's owner's location, or a target NPC's position.

The entire `s5_itemevents.asm` table at `0x59604` is:

| Row | Match / target |
| --- | --- |
| Explicit six-byte row | X35, Y24, NO_DIRECTION=`$FF`, COTTON_BALLOON125; Map22_ItemEvent0 relative to Section5 |
| `$FD` default row | Map22_DefaultItemEvent1, bare rts |

`mapsetupsfunctions_1.asm:RunMapSetupItemEvent` saves d0–5/d7–a1, clears speech
and d6, masks d4 with `$7F`, stores d2's low byte in EVENT_RELATIVE_POSITION,
and resolves current setup again. It compares byte X, Y, facing and item in
order, with `$FF` coordinate/facing wildcards; here any facing matches but
both coordinates are exact. No setup returns with0; the default callback also
leaves0. A matched callback is followed by portrait/text closure, a VInt wait
and WaitForEntityToStopMoving for physical0, then register restoration excluding
d6. Event-relative-position here is the Y byte, not a new target/facing gate.

At `Map22_ItemEvent0`, the complete order is:

1. d1=125; j_GetItemInventoryLocation.
2. Compare d0.w with -1. If equal, skip removal; otherwise j_RemoveItemBySlot
   using that search's member/slot, **not** the menu-selected member/slot.
3. Run cs_599B2 unconditionally; only after return set d6=-1 and fall through
   the default rts. Removal's d2 status is not tested.

The actual s02 jump aliases are GetItemInventoryLocation `$81D0` → `$9146`
and RemoveItemBySlot `$819C` → `$8E76`. Their selected bodies are in
`code/common/stats/itemstats.asm` (included by layout section02), not the
similarly named iteminventory helpers or alternate `stats/items/` implementations.
The retained item-transaction fixture binds these exact service/caller seams;
its former no-callee-body boundary is extended only for this local join.

GetItemInventoryLocation rebuilds UpdateForce's ascending joined0–29 list,
including reserves and dead members. It searches members in that order and,
for each with n held items, searches slots n-1 down to0, comparing masked item
indices. With coherent packed inventories, first joined holder wins and its
highest matching occupied slot wins; duplicates elsewhere remain. It neither
searches Caravan storage nor prefers the selected member, and full inventory
does not prevent removal. With no match it returns d0=d1=-1. Supplied holes
above slot n-1 can conceal an item; a zero-length force has no early guard before
the first loop iteration. These are source-state limits, not ordinary reachable
empty-party or malformed-inventory scenarios.

RemoveItemBySlot masks the addressed item. Non-NOTHING shifts higher slots left,
fills the final slot with NOTHING and returns d2=0; NOTHING returns3. Both tails
update combatant stats. It does not perform the drop action's cursed-item gate.
Normal matched search/removal consumes one balloon. Directly invoking the
handler with no matching joined inventory still starts the scene; natural menu
use first requires selection of a held item. Supplied d4 alone is not proof of
normal custody or reach. No branch checks212/258/522/776/777/778, and no flag is
cleared when the item is removed. Later scene/wrapper failure does not restore it.

FieldMenu tests returned d6: nonzero exits the whole menu; zero displays text73
(`{NAME} used the{N}{ITEM}.{W2}`) and422 (`But nothing happened.{D1}`), closes
text and returns to the item submenu. The matched returning balloon scene skips
those fallback lines. Its -1 is a handled status, not proof of a consumed item,
finished physical descent or admitted destination. The script has no own dialogue.

## All actors and the complete departure program

**Confirmed:** the accepted population owners enumerate all30 baseline rows and
all14 F522-alternate rows. Both setup pointer tables select the same Section5
and scripts. InitializeMapEntities clears entities/lookups, declares selected
followers, consumes rows, then declares Bowie at physical0. Ordinary NPC IDs
start128 independently of ally rows. Under only F67/Peter and F68/Oddler,
no skipped ally declaration and no raft overwrite, the selected actors are:

| Logical identity | Physical example | F522-clear population | F522-set population |
| --- | --- | --- | --- |
| Bowie0 | 0 | Main ally | Main ally |
| Peter7 | 1 | F67 follower | F67 follower |
| B31 | 2 | F68 Oddler follower | F68 Oddler follower |
| Guard136 | 13 | GUARD206, `(33,24)`, Left, eas_Init | Same |
| NPC137 | 14 | KING153, `(24,5)`, Down, eas_Init | OBJECT1=173, `(63,63)`, Up, eas_InitFixedSprite |
| NPC138 | 15 | QUEEN135, `(26,5)`, Down, eas_Init | OBJECT1=173, `(63,63)`, Up, eas_InitFixedSprite |
| NPC139 | 16 | BISHOP144, `(22,5)`, Down, eas_Init | OBJECT1=173, `(63,63)`, Up, eas_InitFixedSprite |

The three OBJECT1 rows are existing allocated actors, not items spawned by
cs_599B2. The script never sets their mapsprite. A supplied F522-clear load
therefore manipulates King/Queen/Bishop at the same logical IDs, not balloons;
it does not select the baseline ORB153–155. Changing F522 after allocation
changes future setup lookup, not already allocated sprites. The F522 switch
alone neither creates a follower nor guarantees the scene's visual identities.

`data/scripting/entity/followers.asm` and the first-arrival owner retain the
complete ordered flag-driven allocation. Non-overworld Map22 omits the F65
Caravan prefix. F67 supplies Peter; B is selected by F74/Astral, F68/Oddler and
F70/Astral in that order when admitted, so later B lookup writes can replace
earlier ones while old physical actors remain. Other followers shift NPC
physical slots. Missing zero lookups can resolve to physical0, not a no-op.
HP/class choose ally mapsprites (including a dead ally's BLUE_FLAME in exploration);
they do not supply missing follower flags. No scene command recruits/revives
Peter or creates B. The allocator's mode/raft31 overwrite and possible alias
cases remain as in the accepted population owners; ordinary F67/F68 alternate
allocation ends at16, leaving31 separate. Resolve actual mappings before use.

`scripts.asm:29–101`, cs_599B2 at `0x599B2`, contains exactly29 top-level
commands. The complete inline/custom bodies are included below; none is omitted
as an assumed animation:

| Index | Command and complete nested body |
| --- | --- |
| 0 | setActscriptWait Bowie,eas_Init |
| 1 | setActscriptWait Peter,eas_Init |
| 2 | setActscriptWait B,eas_Init |
| 3 | setDest Bowie,35,24 |
| 4 | setDest Peter,34,24 |
| 5 | setDest B,33,24 |
| 6 | entityActions Bowie: jumpRight1; faceDown1; moveDown2; endActions |
| 7 | entityActions Peter: moveRight1; jumpRight1; moveDownRight1; faceDown1; endActions |
| 8 | entityActionsWait B: moveRight1; jumpRight1; faceDown1; moveDown1; endActions |
| 9 | setPos137,36,25,Up |
| 10 | setPos138,37,24,Up |
| 11 | setPos139,35,24,Up |
| 12 | stopEntity Bowie |
| 13 | stopEntity Peter |
| 14 | stopEntity B |
| 15 | customActscriptWait Bowie: ac_setSpeed10,10; ac_jump eas_Idle; ac_end |
| 16 | customActscriptWait Peter: identical speed10,10/idle/end body |
| 17 | customActscriptWait B: identical speed10,10/idle/end body |
| 18 | customActscriptWait137: identical speed10,10/idle/end body |
| 19 | customActscriptWait138: identical speed10,10/idle/end body |
| 20 | customActscriptWait139: identical speed10,10/idle/end body |
| 21 | entityActions Bowie: moveDown6; endActions |
| 22 | entityActions Peter: moveDown6; endActions |
| 23 | entityActions B: moveDown6; endActions |
| 24 | entityActions137: moveDown6; endActions |
| 25 | entityActions138: moveDown6; endActions |
| 26 | entityActionsWait139: moveDown6; endActions |
| 27 | warp MAP_FLOOR_WORLD,26,8,Down |
| 28 | csc_end |

There is no story-flag write, camera-target command, fade, dialogue, sound,
load/population replacement, attachment/carry relation or hide command in this
program. The six actors can share intended movement, but they remain independent
records. Entry/surrounding services can still perform their own work.

### Action installation, waits and movement limits

`sf2cutscenemacros.asm` encodes setActscriptWait as csc15 with wait byte `$FF`;
customActscriptWait is csc14 with `$FF`. Both resolve the actor, seed its action
wait timer from the physical index, replace its action pointer and poll **only
that pointer** for eas_Idle. csc14 then skips the inline data through `$8080`.
No timeout, all-actor join or HP guard exists in those installers. Installing a
new action replaces an unfinished previous action for the same physical actor.
Trap6 activates entity updates, so menu update deactivation does not by itself
prove these waits deadlock; actual service progress is still unobserved.

`data/scripting/entity/eas_actions.asm:eas_Init` sets acceleration factors0/0,
speed32/32, acceleration/deceleration off, obstructable on, entity/map
uncollidable off, autofacing on, orientation Up, immersion off, shared size24,
updates the sprite, then jumps idle. It does not set position, HP or animation
on. InitFixedSprite additionally turns autofacing/animation off. Shared size
and sprite orientation are not persistent world scale or gameplay geometry.

csc29 setDest multiplies absolute tile X/Y by384, writes destinations,
absolute travel distances and signed velocities of magnitude32. All three IDs
here have bit15 clear, so each call waits for current X/Y to equal its **current**
destination via WaitForEntityToStopMoving. They are sequential placement
requests, not teleports. The wait does not retain an immutable target for later
comparison; changed destinations/actions/aliases can alter what completion
means. csc29 has no HP check, no revival and no separate pathfinder.

csc2D compiles the nine inline sequences and clears FLAGS_A bits5/6 with `$9F`
on each target. moveUp/Right/Down/DownRight compile esc04 relative displacement
and esc01 destination wait; DownRight uses both positive deltas. esc04 scales
signed deltas by384 **from current position**, writes new destinations/travel
and signed velocities from the actor's speed bytes. The existing
HasSameDestinationAsOtherEntity bit5-clear arm returns Z set, so esc04's `bne`
conflict branch is bypassed. Do not reintroduce a destination-occupancy block
for these unmodified actions or confuse this with a general physics guarantee.
esc01 advances only when current and current-destination X/Y match.

faceDown1 compiles facing3, sprite update, then esc00 wait1. Facing alone
neither displaces an actor nor waits for travel. jumpRight1 selects eas_JumpRight
and copies its first34 bytes: autofacing off; relative `(1,-1)`; wait6;
relative `(1,1)`; wait6; relative `(0,0)`; autofacing on. The input1 is not a
horizontal tile count or duration override in this compiler arm. The source's
terminal idle jump is not part of those34 bytes; subsequent inline commands
continue, and csc2D appends its own idle jump after the sequence.

These jump relative writes are separated by timer waits, **not** esc01 arrival
waits. Each uses the then-current position, so summing `(1,-1)+(1,1)` into a
guaranteed two-tile jump is invalid. The final `(0,0)` rewrites destination to
the current position. No exact trio landing positions, paired attachment,
ballistic physics or frame duration is established by this source sequence.
Only B's first inline action is explicitly waited before positioning137–139;
B reaching idle does not join Bowie/Peter. Their subsequent speed-script
installation can replace an unfinished earlier action.

csc19 setPos writes both current/destination tile×384 and facing, then sprite
update; it does not reset action, velocity or speed. Its HP guard skips true
allies0–29 at HP0, but all three calls here target NPC137–139 and bypass it.
csc1C stopEntity writes animation counter -1, not zero velocity or an idle
action. Unlike the action installers/setDest, these three stop calls skip dead
Bowie/Peter; B31 bypasses the true-ally guard. Dead actors are therefore not
uniformly skipped by this scene. Missing/aliased lookups can make several
commands replace or reposition the same physical actor.

The six custom bodies use esc10 to write adjacent X/Y speed bytes10/10 and
esc34 to jump idle; they do not restore Init, animation or position. Finally,
six csc2D actions request relative `(0,+6)`, each followed by its own destination
wait/idle jump. They are installed in order, allowing concurrent progress,
not simultaneous start. Only139 is waited by the map script. Under unchanged
coherent placeholders the fixed starts give requested endpoints137 `(36,31)`,
138 `(37,30)`,139 `(35,30)`; trio endpoints depend on actual earlier positions.
Even139's completed unchanged action proves only its own destination wait,
not completion of the other five. Sprite-load capacity, replaced action,
aliased/absent actor or stalled update can change or prevent completion.

The immediate consumers are `code/common/scripting/entity/entityscriptengine_2.asm`
(esc00/01/04/0A/10/22/34, HasSameDestinationAsOtherEntity and VInt_UpdateEntities)
and `entityfunctions_2.asm:WaitForEntityToStopMoving`. The VInt skips actors
whose X is at least `$7000`; an installed action on such a supplied actor need
not progress. Coordinates `(63,63)` are below that threshold. These mechanisms
explain conditional progress without claiming an observed descent.

## Pending warp, wrapper return and admission boundary

**Confirmed:** csc07 writes MAP_EVENT_TYPE=1, PARAM1=0 and PARAM2–5=
`26,26,8,3` (Map26, X26, Y8, Down). It does not immediately change CURRENT_MAP,
load destination actors, unlock a battle or wait for a transition. It replaces
an older pending request/tuple if reached. A later writer can likewise replace
or clear this request before consumption; it is not a queue or reservation.

csc_end reaches ExecuteMapScript's common tail: if the dialogue window index
is nonzero, wait for view scrolling; clear VIEW_SCROLLING_SPEED; restore
registers and return through trap6. That tail supplies no all-six-actor wait.
Debug P2-Start text skipping does not bypass this scene's action commands.
The item handler then sets d6=-1. Its wrapper closes portrait/text, waits one
VInt and waits for physical0's current destination before returning. This
extra main-actor wait is neither an idle-pointer wait for all actors nor proof
that Peter/B/137/138 finished their scripts. FieldMenu exits on -1;
ProcessPlayerAction restores view/entity VInts and returns to ExplorationLoop.
Any failure before these returns leaves only completed prefix effects, including
possible item removal and pending warp, with no transactional undo.

ExplorationLoop returns to SetMoveSfx/WaitForEvent. A nonzero pending type takes
priority over installing a controlled action on physical0. WaitForEvent returns
its type in d0; ProcessMapEvent clears MAP_EVENT_TYPE before dispatching that
sampled type. For type1/PARAM1=0 it submits and clears WARP_SFX, idles physical0,
unwinds the exploration call frame, and reads PARAM2–5 into the return tuple.
UpdatePlayerPosFromMapEvent supplies fallback state, then the nonnegative
explicit X26/Y8 replace the low coordinate bytes and facing3 is read directly.
With coherent nonnegative local coordinates this returns26/8; arbitrary supplied
negative/high-byte state is not normalized by those byte writes. The selected command
does not set WARP_SFX; no particular sound follows from this warp alone.
A replaced type or PARAM1/tuple follows its own consumer, not this promised route.

The return reaches the normal `MainLoop:@Start` in `code/gameflow/mainloop.asm`:
SwitchMap, then CheckBattle, unlike Battle22's outcome alternate entry which
bypasses the battle check. `code/common/maps/mapinit_0.asm:SwitchMap` scans the
complete three-row `data/maps/global/flagswitchedmaps.asm`. Only Granseal
under640, drought Pacalon under530 and Ancient Tower underground under999
have substitutions; there is no Map26 row. The requested map therefore stays26
regardless of those flags.

`code/common/maps/getbattle.asm:CheckBattle` scans all45 rows of
`data/battles/global/battlemapcoords.asm`, requiring a map match before unlocked
flag400+battle and trigger tests. **No row names Map26**. It returns d7=-1
and MainLoop selects j_ExplorationLoop for `(26,26,8,Down)`. In particular,
Battle22's Map22/F422/522 and Battle23's Map27/F423/523 cannot match Map26;
F88 suspension and intro/completion flags do not create a missing map row.
The selected item/scene writes no such flag and does not update EGRESS_MAP.
A later substituted request could admit its own matching battle, but that is
not the surviving explicit Floor World request.

This is the stopping boundary: conditional entry to the destination exploration
loader. Do not infer Floor World population, initialization, usable control,
an immediate encounter or Battle23 opening from it. Those consumers and later
interior travel remain outside this document.

## Reproduction, checks and retained limits

Load `. ./local/private-inputs.ps1` in the same process as controlled commands.
Verify the pinned checkout's exact HEAD and clean status. The existing
[retained comparison recipe](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary)
compares map-init equality, `_verify_map_script_engine_fixture` and source
`_cutscene_facts` without regenerating either payload. Select entryLabel
cs_599A4/cs_599B2 from `local/derived/map-script-engine-static.json`'s
programCorpus.programs: 3/29 top-level commands at scripts.asm24–28/29–101.
Read every nested source body directly; top-level equality alone does not
validate movement or prove item-use admission.

The existing `tests/fixtures/h2/map-event-item-transactions-static-v1.json`
binds Map22's table/handler, aliases, location comparison, remove call and
FieldMenu return seam, including d6=-1 at `$5962A`, FieldMenu call `$2152A`,
wrapper `$47586` via `$44088`, and caller test `$21530`. Its
`eventItemTransactions`/`sourceContext` are top-level fields, not an expected
wrapper. Use those facts as seam evidence, not as a new runtime custody claim.
The selected actual AddItem/location/removal bodies and their section02 jump
aliases, menu whitelist/selection, chest/search, trap6, action/wait and pending
consumers above must be read to reproduce the composed result.

Direct review checks complete selected tables and source lines, exact text
keys/controls, source enum operands, entire owned prose/diff, local links/anchors
and private/generated boundaries. Run `git diff --check` and configured
`uv run sf2 verify plan --base origin/main --head HEAD` on the clean committed
candidate; record the plan and actual final-head CI on Issue916. No local
normal/full/engine/Godot/H3/H4 suite solely for this prose; no new fixture or
runtime acquisition is needed to raise a coverage count.

Preserve [#912 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/912#issuecomment-6175333338)
and [PR914 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/914#issuecomment-6175364090):
head `9cd21b5766caa4e939913aef2c818e9097681f98`, tree
`6c76d0b0cd5d30d36f4201d57f815d01aa98aa41`, base
`f8cd67cd3818a1790b76354922015c13d8af083a`, CI38614495457 and unchanged
18,285,979-byte pair/0.084s. Its oversized inspection, guessed source/config
paths and corpus-key repairs, Windows206 rejected write and routing-romSha256
KeyError remain recorded, together with every linked predecessor correction,
failure and original/rebased CI identity. Preserve [#911 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/911#issuecomment-6175244286)
and [PR913 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/913#issuecomment-6175260041)
with its separate scope-only CI38613823392. No new result erases these records.

**Unknown:** natural chest/guard/item-use reach; actual supplied flags, inventory,
force/HP/class, actor/follower/raft mappings, layout and positions; selected menu
result; completed input, text, action, scroll, sprite and transition services;
actual motion, visual/audio presentation, usable control and persistence.
The supplied-state counterexamples are static distinctions, not played scenarios.
[ADR0014](../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../decisions/0016-remake-start-evidence-deferral.md) retain them
without creating an automatic runtime queue. Floor World init/interior and
Battle23 opening remain expressly unresearched here.
