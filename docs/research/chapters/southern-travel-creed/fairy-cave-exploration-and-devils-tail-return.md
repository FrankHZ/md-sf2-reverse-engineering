# Fairy Cave exploration and Devil's Tail return

Evidence date: 2026-10-14. **Confirmed:** Map32 has four chest-item rows and
no other-item rows. Its missing map setup resolves to `ms_Void` through the
actual dispatchers; shared search and population still operate. Its two exits
request Devil's Tail `(51,23)` and `(57,24)`, Down, where supplied F421 can admit
Battle21 before exploration. A separately selected rock description at `(57,23)`
uses F751, F13 and Elric's HP but does not open the passage. **Inferred:** the
rock dialogue is a hint about Elric's separate passage interaction. **Unknown:**
natural access, selected layout/actors/state, completed services, presentation,
usable control and saved persistence. This is conditional static evidence, not
a continuous played route or a promise that every reward is reachable.

## Agreement and provenance

Standard rigor under [Issue #880](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/880)
and [content epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514):
establish the optional cave rewards, both returns and the outside rock interaction
for the next content guide. Accepted starting base is
`4721d145166eb16b307cdc6a65f72b0a74ea4afa`. This document is the sole owned path;
no shared edits, stacked dependency, new machinery or runtime acquisition.
Stop at the completed conditional search/interaction, selected Map67 control or
request boundary, or Battle21 admission before opening. Exclude terrain/path
reconstruction, cave-to-Creed traversal, Creed content, item-use effects/economy,
and Battle21 scenes, roster, combat and outcomes.

Consume accepted [Fairy Cave entry](./harpy-pond-departure-through-fairy-cave-entry.md),
[Elven Village search, recipients and rollback](./elven-village-conversations-and-local-exploration.md),
[complete F520 call-for-help branch](./elven-village-departure-through-harpy-pond-entry.md#complete-call-for-help-program),
[request writes](../../world/map-event-request-state.md),
[request consumption](../../world/map-event-request-consumption.md),
[random-battle structure](../../world/map-event-random-battle-state.md),
[walking and packed-reload consumers](./caravan-boarding-and-southwest-parmecia-travel.md#packed-reload-and-residual-parameters)
and [mode/raft selection](./polca-raft-boarding-through-south-parmecia-entry.md).

Source paths below are relative to `disasm/` in
[SF2DISASM master at `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The assigned read-only checkout is clean at that pin. Retained evidence binds
USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Short map paths start at `data/maps/entries/`; setup/follower consumers at
`code/common/scripting/map/`; exploration consumers at `code/gameflow/exploration/`.
Fnnn means flag nnn, and table rows are zero-based. Coordinates, sound IDs and
wait operands are source values, not observed locations or durations.

Reuse the unchanged 18,285,979-byte map-init/map-script-engine pair and small
source/fixture selections. Linear comparisons are budgeted below 512 MiB and
one minute, with only small receipts and prose; no new capture, extraction,
environment or payload. The retained and field-search comparisons passed in
0.122 seconds. Reassess before material scope/resource growth.

## Cave setup and search admission

**Confirmed:** the complete `data/maps/mapsetups.asm` contains no Map32 entry.
`mapsetupsfunctions_1.asm:GetCurrentMapSetup` reaches its terminator and returns
`ms_Void: dc.w $FFFF`. This is the evidence for the fallback, rather than guessed
Map32 setup files. The complete Map32 flag, step, roof and other-item tables
(`3-flag-events.asm`, `4-step-events.asm`, `5-roof-events.asm`, `8-other-items.asm`)
each contain only `endWord` (`0xAC37A..0xAC380`, `0xAC3A4..0xAC3A6`).

| Actual consumer | Selected Map32 behavior |
| --- | --- |
| GetMapSetupEntityList (effective target of j_GetMapSetupEntities) | Returns the sentinel. InitializeMapEntities skips fixed rows but still clears/rebuilds shared followers and Bowie, terminates the follower list and runs its raft tail. |
| RunMapSetupInitFunction | Sentinel skips the init callback; registers restored, return. No cave-specific story or repair. |
| RunMapSetupEntityEvent | Writes EVENT_RELATIVE_POSITION, then sentinel bypasses event/portrait/facing/VInt callback work and returns. An already activated ordinary C entity still consumes that interaction; the caller does not fall through to Search. |
| RunMapSetupAreaDescription | Clears d7; sentinel skips a callback and tests d7 before restoring registers. Returned zero condition means no description handled. |
| RunMapSetupItemEvent | Clears CURRENT_SPEECH_SFX, masks item ID, writes relative position and clears d6; sentinel skips callback/window closure, then still waits one VInt and physical 0 position/destination equality. This is an item-use dispatch boundary, not a searchable-item award or proof that all generic item use is inert. |
| RunMapSetupZoneEvent | Sentinel skips callback/window closure but still waits one VInt and physical 0 position/destination equality. No Map32 default random-battle callback is invented. |

The accepted entry owner establishes CURRENT_MAP=32/CURRENT_BATTLE=255, shared
fresh population, and pending-zero conditional control. Map32 is not in the
overworld list: F65 alone adds no Caravan follower here; with only F67/F68,
Peter and B/Oddler occupy physical 1/2. No follower row declares Elric. Other
supplied follower selectors and mode/raft state retain their shared meanings.
The cave fallback does not force walking, membership, HP or a particular cast.

### Entity-first C versus explicit Search

**Confirmed:** `explorationvints.asm:ProcessPlayerAction` first idles physical 0,
waits for its movement and view scrolling, then selects the input branch.
On the ordinary C path after debug/Caravan special handling, GetActivatedEntity
runs first. A nonnegative result is mapped to an event and dispatched, then
returns even if the map callback does nothing. With no activated entity, C
calls CheckArea with d6=1. `code/common/menus/main/mainactions.asm:FieldMenu`
Search instead clears d6 and calls j_CheckArea directly, then exits the menu.

`explorationfunctions_0.asm:CheckArea` requires a nonnegative view target,
clears DIALOGUE_NAME_INDEX_1 and CURRENT_SPEECH_SFX, computes the one faced tile
using direction offsets/384, and reads the 64-wide working layout. NPC activation
can extend through counter blocks; CheckArea does not repeat through counters.
The masked layout word (`& $3C00`) selects before any description-coordinate row:

| Kind | Consumer | Initial / empty text IDs |
| --- | --- | --- |
| `$1800` chest | OpenChest | 403 /408 |
| `$2C00` vase | CheckNonChestItem | 404 /409 |
| `$3000` barrel | CheckNonChestItem | 405 /410 |
| `$3400` bookshelf | CheckNonChestItem | 427 /412 |
| `$1C00` searchable area | CheckNonChestItem | 423 /412 |
| Other masked values | RunMapSetupAreaDescription | Only a matched admitted description handles it. |

The first five branches never fall through to descriptions, even when empty,
already collected or unable to award an item. In Map32 the non-chest table is
empty, so those four non-chest kinds select their empty result. A chest kind
only awards a matching chest row. Other kinds reach the void description:
d6=1 returns d0=0 and the C caller opens its menu; d6=0 emits 423 then 412,
closes text and returns d0=-1. Handled item/description paths return -1. Neither
a table coordinate nor an empty callback proves natural reach, visibility or
layout kind; a changed layout can prevent collection at a listed chest.

## Four chests, recipients and persistence

**Confirmed:** complete `map32/7-chest-items.asm` (`0xAC392..0xAC3A4`) contains
four `mapItem` records and `endWord`. `sf2mapmacros.asm:mapItem` encodes X,Y,
flag byte,item byte. `sf2enums.asm` resolves the item symbols:

| Row | Coordinate | Flag | Item byte / identity |
| --- | --- | --- | --- |
| 0 | `(23,15)` | F160 | 4 /`$04`, ANGEL_WING |
| 1 | `(24,15)` | F161 | 15 /`$0F`, BRAVE_APPLE |
| 2 | `(25,15)` | F162 | 0 /`$00`, MEDICAL_HERB |
| 3 | `(19,26)` | F163 | 123 /`$7B`, MITHRIL |

The flag byte is the game flag index, with no chest-base addition. Item 0 is a
real Medical Herb, not an empty slot. All four differ from ITEM_NOTHING=127 and
are below gold-chest threshold 128; no gold branch or effect-on-use is implied.

`exploration.asm:GetChestItem/GetItem` selects the current map's chest table and
first X/Y match; exploration's CURRENT_BATTLE=255 skips battle-coordinate
adjustment. A match returns the layout offset, zero-extended flag and item.
A negative row X ends the search, returning -1 in d0/d1/d2. CheckArea's item mask
converts that no-match item to 127. No-match OpenChest changes no flag or block.

For a matching row, OpenChest tests its flag: set substitutes 127, clear retains
the item. Both paths then set the flag, write open-chest block `$D802` (55298)
into the working layout, and request the selected plane update. Closed `$D801`
and open `$D802` both mask to `$1800`. A repeat with its flag still set therefore
emits 403/408 and grants no second copy; it does not reveal another description.
Flag state, rather than possession of an identical item, controls this duplicate
boundary. Flag clear with an already open-looking tile still selects the item.

The [accepted recipient/rollback owner](./elven-village-conversations-and-local-exploration.md#four-item-rows-and-immediate-inventoryflag-consumers)
applies unchanged through `explorationfunctions_1.asm:loc_239EE`,
`code/common/stats/itemstats.asm` and `battleparty.asm`:

1. Emit found-item text 413; test ally index 0's four slots first. With capacity,
   AddItem writes the first slot whose masked ID is 127, retaining index/broken
   bits, then the caller submits item music, emits 415 and waits through
   FadeOut_WaitForP1Input.
2. With index 0 full, UpdateForce rebuilds joined members in ally-index order.
   OTHER_FORCE_MEMBERS_LIST aliases TARGETS_LIST+1; the scan uses length-2 and
   DBF. Under ordinary Bowie-joined state this skips Bowie and awards the first
   other joined member with room, including reserve or HP-zero members. It sets
   the recipient name, submits item music, emits 416, uses the same wait and
   closes text.
3. No eligible free slot, including a negative initial scan counter, emits 417.
   Restore searched X/Y, call CloseChest then RefillNonChestItem. The matching
   chest flag clears, `$D801` is restored and its plane update requested; the
   empty other-item table makes the latter call a no-op. A later admitted search
   may retry. A repeated already-taken chest takes the empty branch instead and
   does not enter this inventory rollback.

A supplied missing-Bowie membership does not repair the list convention: index
0 is still tested first and the list still skips its first joined entry. AddItem
returns success/failure in d2, but the caller does not branch on that result
after checking capacity. Successful handoff is conditional on coherent inventory
and returning services; partial progress has no general transaction rollback.

Text joins in `data/scripting/text/gamescript.txt` are 403 `{NAME}`, `{W2}`,
`{CLEAR}`; 408 `{W1}`; 413 `{NAME}`, `{N}`, `{ITEM}`, `{W2}`, `{CLEAR}`;
415 `{NAME}`, `{N}`, `{ITEM}` and 416 `{NAME}`, `{N}`, `{ITEM}`, `{NAME}` have no
W-token and use the explicit item wait; 417 has `{N}`, `{W1}`. The initial name
parameter is index 0, not a dynamic active-leader selection. These controls do
not prove display, recipient mutation, sound or wait completion.

`code/common/maps/mapload.asm:LoadMapBlocksAndLayout` reloads base blocks/layout,
applies flag-block rows, then iterates chest rows. Each set chest flag writes
`$D802` at its X/Y. Thus a later full cave load reapplies each open chest from
F160–F163 independently of Map32's empty flag-block table. Those flags are
outside exploration's 256–383 temporary reset. Changing a flag alone does not
immediately redraw the current layout; current-map loading can retain existing
layout. Actual saving/reloading and rendered persistence remain Unknown.

## Both external exits and conditional Map67 control

**Confirmed:** complete `map32/6-warp-events.asm` (`0xAC380..0xAC392`) contains:

| Row / selected coordinate | Request |
| --- | --- |
| 0 /`(2,3)` | no-scroll 0, Map67 `(51,23)`, Down 1 |
| 1 /`(29,3)` | no-scroll 0, Map67 `(57,24)`, Down 1 |

WarpIfSetAtPoint divides supplied internal coordinates by 384 and scans ordered
external rows, with negative-byte wildcards. Walking reaches it only through
its movement/layout `$1000` branch; vehicle producers have their own checks.
The row writes event 1, parameters 1–6 and WARP_SFX. It tests no chest flag,
F751, F13, F520, F421 or F521. Arrival at a cave entrance that is also an exit
coordinate is not a fresh movement producer and does not prove an immediate
bounce. Neither exit establishes a natural westward/cave-to-Creed route.

After the retained request reaches ProcessMapEvent, pending is cleared before
dispatch. Type 1 no-scroll submits and clears WARP_SFX, idles physical 0, unwinds
ExplorationLoop, samples current position and substitutes the nonnegative row
coordinates. It returns `(67,51,23,Down,d4=0)` or `(67,57,24,Down,d4=0)` to
MainLoop without normalizing PLAYER_TYPE. Later writes can replace the one
pending slot; publication is not consumption.

MainLoop runs SwitchMap then CheckBattle before fresh exploration. The complete
flag-switched-map table has no Map32/67 replacement. The complete 45-row battle
table has no Map32 row and one Map67 row, slot 21: area `(21,0)`, size 32x42,
trigger X/Y=255/255. The area rectangle does not constrain these wildcard
triggers. Supplied F421 selects Battle21 from either exit even though `(57,24)`
is beyond the area's X extent. F521 set clears F421 **after selection** without
canceling admission; F521 clear retains it. Stop before BattleLoop opening.
Neither the exits nor the rock description sets either flag.

With F421 clear, fresh ExplorationLoop clears pending, subtracts 20000 from
STEP_COUNTER with signed-word negative-result clamp to 0, heals eligible
living/immortal allies and fades. It sets CURRENT_MAP=67/CURRENT_BATTLE=255,
loads tilesets, clears/rebuilds entities/lookups, loads mapsprites, clears flags
256–383, sets F80, loads map/view, installs base VInts and invokes map init.
F13, F160–F163, F421/F521, F520/F751 and follower selectors are not cleared by
that temporary reset.

Map67's sole six-pointer setup at `0x4FAEC` has no flag alternate. Its fixed
entity list is empty; `s6_initfunction.asm:ms_map67_InitFunction` at `0x4FD4A`
is only `rts`. Shared InitializeMapEntities still declares Bowie physical 0 at
the requested tuple and rebuilds followers from flags, not membership/HP. Map67
is an overworld map: with only F65/F67/F68, physical 1/2/3 are Caravan/A, Peter,
Oddler/B. F66 adds Sarah/Chester; other rows keep their ordered lookup writes,
including possible A/B replacement. No Elric row is present. A prior cave-only
actor or following relationship is not retained by fresh population.

The fresh raft tail `followersfunctions_2.asm:sub_44404` keeps supplied mode
and saved raft state: mode 2 gives physical 0 the raft sprite, parks followers at
`(64,64)` with standing scripts and declares separate raft 31 there. Other modes
declare raft 31 only when F64 is set and saved RAFT_MAP=67, using masked saved X/Y.
Mode 1 does not repeat boarding. No selected exit/init rewrites saved raft map
or coordinates, disembarks or repairs arbitrary transport state.

F751 independently controls `map67/3-flag-events.asm`'s one 3x2 copy from `(0,0)`
to `(56,22)` during LoadMapBlocksAndLayout. The prior passage owner separates
that reapplication from its live copy and later post-script F751 write. Neither
fresh return nor description replays the passage scene. Text alone proves no
passability, visibility or live-layout repair after a supplied flag change.

After conditional map music/fade, pending 0 lets WaitForEvent follow physical 0
and install SetControlledEntityActScript: mode 0 walking; mode 1 eas_Raft (actual
Caravan); other values eas_Caravan (actual raft for mode 2). A pending request
wins first. Controller installation is a conditional control boundary, not
observed responsive input or guaranteed service completion.

## Separate zone admission and Battle21 request boundary

**Confirmed source correction to the Issue:** east return `(57,24)` does **not**
share a call-for-help zone coordinate. The complete `map67/mapsetups/s3_zoneevents.asm`
selects `(57,25)` for the separately owned F751/F13/HP passage; `(56,26)`,
`(57,26)`, `(58,24)`, `(58,25)` for F520 call-for-help; all other coordinates
select Map67_DefaultZoneEvent. Neither fresh return tuple invokes a zone merely
by arriving. A separately submitted zone at either `(51,23)` or `(57,24)` would
select the default. This corrects the Issue premise without inferring a route.

If a later admitted event selects a call-for-help coordinate, F520 set returns;
clear executes the complete accepted cs_4FB64 branch, including current B/A/Peter
Init/Jump, text 2543–2549, movements/camera/relocations and the final Map51
`(11,22)`, Up request. Consume the [whole accepted branch and its conditional pond admission](./elven-village-departure-through-harpy-pond-entry.md#independent-pond-requests-and-admission-before-init),
including partial-service and replaced-request limits; no new pond investigation
is made here. The distinct passage zone remains the [accepted owner](./harpy-pond-departure-through-fairy-cave-entry.md#separate-map67-zone-selection-and-passage-admission),
not a rock-description side effect or rerun in this slice.

For default-zone selection, walking's `$1400` producer submits event 6 and word
X/Y before the later raw-layout blocking test. Its admitted movement tail alone
decrements a positive STEP_COUNTER; a blocked tail can leave the zone request
without that movement/decrement. Caravan control has no equivalent zone producer.
Type 6 installs physical 0 Init, then RunMapSetupZoneEvent selects the row/default,
calls it, closes portrait/text, waits one VInt and physical 0 current/destination
equality. Applying Init is not proof of position completion.

Map67_DefaultZoneEvent (`0x4FB58`) loads BATTLE_DEVIL_TAIL=21, calls
CheckRandomBattle and returns. Reuse the accepted shared function at
`0x47856..0x478C6`, with F521/F421 in place of F519/F419:

| Supplied state | Result in CheckRandomBattle |
| --- | --- |
| F521 clear | Request path regardless of counter; no RNG call. |
| F521 set, counter nonzero (including negative) | No new request, no RNG, counter unchanged by helper. |
| F521 set, counter 0, first range 8 RNG returns 0 | Request path, no second RNG. |
| F521 set, counter 0, first RNG nonzero | No new request; range 4 RNG, store result+2 (2–5) as counter. |

These are source RNG/branch rules, not measured encounter probability. The
request path sets F421, writes packed longword `$000100FF` at MAP_EVENT_TYPE,
sets counter 30000, waits view scroll, submits SFX_BOOST and executes the existing
flash script. No F521 write or immediate battle call occurs. Flash/scroll and
zone-wrapper completion remain prerequisites to normal later consumption.

The packed write is event 1, parameter 1=0, parameter 2=255/MAP_CURRENT. It leaves
parameters 3–5 and WARP_SFX untouched. In the unchanged ordinary walking-zone
chain, word parameter 3 still contains zone Y, so the warp substitutes X 0 and
that Y; facing comes from residual parameter 5, and residual WARP_SFX is submitted
and cleared. It is not necessarily a reload at unchanged player position.
Other supplied/replaced bytes follow the no-scroll signed-coordinate rules.

After callback/wrapper return, retained pending takes priority over controller
installation. Type 1 returns map 255 to MainLoop; SwitchMap has no 255 row,
CheckBattle resolves current 67 and admits sole row 21 if F421 remains set,
regardless of residual coordinates. F521 set clears F421 after selection;
stop before opening. If pending is replaced, stop at that selected continuation.
If F421 is cleared before admission, no-battle map 255 uses the existing-population
reload branch: main properties/raft, map load/init and conditional control,
without fresh entity setup or temporary reset. No new uninterrupted route or
battle guarantee is claimed.

## Rock description, speech and return

**Confirmed:** complete `map67/mapsetups/s4_descriptions.asm`
(`0x4FCCE..0x4FD42`) contains one wrapper, one function row and its complete body.
The wrapper sets d3=`$1B2` (434), points a0 to byte_4FCDE and jumps to
DisplayAreaDescription. `msDescFunction 57,23,Map67_DescFunc0-byte_4FCDE`
encodes X, Y, condition 0, function-kind 1, relative function offset, then msDescEnd.
This is not msDescFunctionD6. DisplayAreaDescription compares packed X/Y;
condition 0 bypasses the d6 filter, while a nonzero condition would skip when
d6 is nonzero. Kind 1 calls the callback with d6 saved/restored. Thus C's d6=1
and explicit Search's d6=0 both admit this row **if** CheckArea reaches the
description branch. The wrapper's d3 base is not an extra unconditional 434;
the function emits its own text.

The same five item kinds listed above preempt this row. Complete Map67 chest
items are empty; its sole other-item row is `(45,9)`, F214, Mithril, with no row
at `(57,23)`. At the rock coordinate an item-kind layout therefore emits its
kind's empty result, never the rock callback. A `$1000` warp-kind value bypasses
those item branches and can reach the description when searched; walking into
a warp candidate is a separate producer. No decoded natural block is asserted.
An activated ordinary C entity likewise returns through the entity handler,
not through the description.

| Ordered callback branch | Text and immediate services |
| --- | --- |
| Every admitted Map67_DescFunc0 call | Emit 424: investigator `{NAME}` with `{W2}{CLEAR}`. Check F751. |
| F751 set | Emit 434: nothing special, `{W1}`; return. No Peter resolver or F13/HP test. |
| F751 clear | Emit 2590: wobbly rock, `{W1}`; clsTxt. Copy CURRENT_SPEECH_SFX to SPEECH_SFX_COPY, resolve logical Peter 7 portrait/speech, write CURRENT_PORTRAIT and CURRENT_SPEECH_SFX, then test F13. |
| F751 clear, F13 clear | DisplayCurrentPortrait; emit 2591: Peter regards the rock as suspicious, `{N}{W1}`; return. No Elric HP lookup. |
| F751 clear, F13 set, Elric HP word 0 | j_GetCurrentHp for 13, DisplayCurrentPortrait; emit 2592: suggests Elric can help, `{NAME;13}{N}{W1}`; return. |
| F751 clear, F13 set, Elric HP nonzero | No further line and no DisplayCurrentPortrait call; return after the already performed Peter portrait/speech resolution. |

These IDs join gamescript lines `01A8`, `01B2`, `0A1E`, `0A1F`, `0A20`.
The zero/nonzero HP polarity is literal, despite any expectation suggested by
the hint. F13 is joined membership, not active-party F45; reserve membership is
not excluded. This description does not join/heal/spawn Elric, move an actor,
copy blocks, set F751 or request a warp. F520 is not tested. The passage zone's
opposite nonzero-HP admission remains a separate callback.

### Actual speech and closure consumers

**Confirmed:** `GetEntityPortaitAndSpeechSfx` (source spelling) in
`code/common/scripting/entity/getentityportaitandspeechsfx.asm` follows the
current logical 7 lookup through `mapscriptengine_1.asm:GetEntityAddressFromCharacter`
and selects by that physical actor's mapsprite. `data/spritedialogproperties.asm`
provides Peter base/promoted portrait and bleep 4/5 respectively; absent table
match selects the shared default. This does not allocate Peter or check F67,
joined/active flags or his HP. Fresh unassigned lookup 0 can alias Bowie; supplied
stale/255 mappings are not safely rejected by this resolver. Peter attribution
is the intended logical speaker, conditional on the actual actor mapping.

DisplayCurrentPortrait skips a negative portrait; otherwise calls OpenPortraitWindow
with side/mirror 0. The latter returns if a portrait is already open, or creates,
loads, moves/waits and registers blinking. A call does not prove that a fresh
Peter portrait is rendered. No single-text map-script opcode or its debug-skip
behavior is substituted for these direct txt operations.

`sf2macros.asm:txt/clsTxt` use Trap5_TextBox: it saves/restores CPU registers,
dispatches DisplayText or CloseDialogueWindow, then returns. DisplayText starts
the name pointer at DIALOGUE_NAME_INDEX_1; CheckArea supplied 0 makes 424's NAME
Bowie's current name. `{NAME;13}` explicitly resolves combatant 13's name, not
the current speaker. `textfunctions_1.asm` handles `{N}` as a newline, `{W1}`
and `{W2}` as separate input-wait paths (W2 includes cursor/validation handling),
and `{CLEAR}` as dialogue-layout clearing. These are service consumers, not
proof of a particular number of frames, input response or rendered text.
`textfunctions_2.asm:HandleDialogueTypewriting` reads CURRENT_SPEECH_SFX for
selected non-space glyph sound submissions.

The copy to SPEECH_SFX_COPY is **not a restore**. On the F751-clear path CheckArea
has already cleared speech to 0, which is copied before the Peter resolution.
Map67_DescFunc0 never reads the copy back. DisplayAreaDescription then closes
portrait/text and returns d7=-1; RunMapSetupAreaDescription tests it before
restoring registers; CheckArea closes text again and returns d0=-1. The inspected
ClosePortraitWindow and CloseDialogueWindow manage their windows and do not
restore the speech value or CURRENT_PORTRAIT. The enclosing C/FieldMenu/ExplorationLoop
return likewise has no copy-back. Consequently the selected resolved speech/
portrait state remains after this conditional return, including the joined,
nonzero-HP branch with no Peter line, until another writer changes it. F751-set
search leaves CheckArea's cleared speech value and never invokes that resolver.
No global permanent-state or audible-result claim follows.

Map67's complete entity/item tables select only Map67_DefaultEntityEvent and
Map67_DefaultItemEvent0, both `rts`. The entity wrapper can still resolve/display
the activated actor's portrait/speech, deactivate/reactivate entity VInts and
close windows; its default row's modifier 0 requests no facing change. The item
wrapper retains its clear/mask/relative-position/window/VInt/physical 0-wait work.
Neither callback is the rock function or a special item solution. Their existence
does not bypass search precedence or establish all generic item-use outcomes.

After a completed local C/search return, exploration resumes with its existing
population and live state, subject to the pending request. It does not force a
fresh map load or controller before WaitForEvent's pending check. A stalled
text/window service can leave intermediate speech/window changes; no transactional
rollback or automatic passage action is present.

## Reproduction and retained limits

Load `. ./local/private-inputs.ps1` in the same process as controlled tools and
verify the assigned upstream revision/clean status. Reuse the unchanged
[retained comparison recipe](../grans-opening/battle06-to-battle07-content-route.md#reproduction-and-review-boundary):
map-init fixture equality, `_verify_map_script_engine_fixture` and source
`_cutscene_facts` equality. Add the existing field-search source-only comparison:
`_read_source_surface(upstream / "disasm")` then `_validate_source_contract(source)`
from `sf2tool.h2.field_search_control`; compare its `fieldSearchSpine` with
`tests/fixtures/h2/field-search-control-static-v1.json`. This validates its 17-file
source surface, not a fresh H1/ROM or runtime acquisition.

| Evidence surface | Selected direct join/review |
| --- | --- |
| Complete Map32 event/item files, mapItem macro, enums | Exact four rows/flags/item IDs, two exits and four empty tables; no natural layout/access assertion. |
| MapSetups, ms_Void and actual shared consumers | Void entity/init/description/item/zone behavior, full search precedence, four-slot recipients, duplicate/full rollback, chest reapplication on later load. |
| Retained map-init/map-script-engine pair and accepted request/follower owners | Actual Map67 pointer/init; fresh versus map 255 return, independent flags, ordered battle scan, follower/mode/raft and pending boundaries. |
| `map-events/entity-programs.json`, `item-programs.json`, `zone-programs.json` under `tests/fixtures/h2/` | Programs 616, 63, 124 respectively: Map67 return-only entity/item and default-zone operations at their source lines. Accepted F520/passage programs remain with their existing owners. |
| `map-descriptions-static-v1.json` and complete direct Map67 description | Shared dispatcher shape; direct complete row/body/text/macro and immediate speech/closure reads supply this selected semantic join. Aggregate counts do not prove the rock's branch or dialogue. |
| `map-event-random-battle-state-static-v1.json` and accepted packed-reload owner | Default 21 setup, completion/counter/RNG/write/service order, residual bytes and later conditional CheckBattle admission. |

Read complete selected functions/tables rather than treating a lookup miss,
truncated output or source comment as absence evidence. Resolve actual aliases
and strip annotations when comparing fixture operations to source. Direct review
covers the full owned diff, provenance, link/fragment/private boundary and the
Issue's source correction. No fixture/schema/index/code or retained payload is
changed and no new verification program is added.

**Unknown:** natural cave/rock/exit access and working block values, caller
inventory/flags/HP/membership, actual actor/follower/raft mappings, selected
recipient, completed text/window/movement/scroll/flash/load services, sound and
presentation, usable control, saved persistence and natural Battle21 admission.
Counterexamples include item 0 versus empty 127, flag-set repeat, full inventory,
missing Bowie membership, entity-intercepted C versus Search, preempting tile
kinds, F751 independent of F13/HP, reserve/dead Elric, missing Peter, F421/F521
independence, negative/nonzero counters and replaced pending requests. These
are source cases, not executed scenarios or an automatic runtime queue under
[ADR0014](../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../../../decisions/0016-remake-start-evidence-deferral.md).

Preserve [Issue876's final handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/876#issuecomment-6171750883)
and [PR878 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/878#issuecomment-6171765502),
including original/rebased receipts and linked failures. Current inspection
corrections, exact Git identities, direct checks/planner, final-head CI and
process state belong in Issue880's handoff. Acceptance requires whitespace and
scope checks, clean committed configured
`uv run sf2 verify plan --base origin/main --head HEAD`, and actual final-head CI.
No blanket local normal/full/engine/Godot/H3/H4 suite is selected solely for prose.
Freeze a clean pushed Draft PR for independent main-gate review; no integration,
cleanup or successor selection.
