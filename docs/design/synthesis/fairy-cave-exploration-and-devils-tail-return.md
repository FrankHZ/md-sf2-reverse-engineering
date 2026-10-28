# Fairy Cave Exploration and Devil's Tail Return

Fairy Cave contains four selected chest rewards: Angel Wing, Brave Apple, Medical Herb and Mithril.
An admitted search can collect a reward, leave a previously collected chest empty, or restore a chest
when nobody has inventory space. Both exits request Devil's Tail. Outside, inspecting the wobbly rock
can produce a Peter hint; that inspection is separate from Elric's passage-opening interaction.

**Confirmed:** accepted static research establishes the data, conditional dispatch and service order
below. **Inferred:** the rock dialogue hints at Elric's separately owned passage interaction.
**Unknown:** natural access, the actual layout and supplied party/transport state, completed services,
presentation, responsive input and saved persistence. These are conditional content rules rather than
a continuous played itinerary or a promise that all four chests can naturally be reached.

This guide continues [Harpy Pond departure through Fairy Cave entry](harpy-pond-departure-through-fairy-cave-entry.md)
and consumes [accepted exploration/return research](../../research/fairy-cave-exploration-and-devils-tail-return.md).
The continuation is [Battle21 opening and initial roster](battle21-opening-and-initial-roster.md).
It stops at a completed conditional local interaction/search, Map67 control or a selected request,
or Battle21 admission before opening. Creed traversal/content, Battle21 scene/roster/combat/outcomes,
terrain reconstruction, item-use effects and economy are outside this guide. Fnnn means flag nnn;
rows are zero-based. Coordinates, waits and sound IDs are source operands, not observed locations,
elapsed durations or audible results.

## Searching a Cave with No Local Setup Program

**Confirmed:** Map32 is absent from the complete MapSetups table. GetCurrentMapSetup reaches the
terminator and returns `ms_Void: dc.w $FFFF`. The actual consumers give that sentinel its meaning:
shared entity population skips fixed rows but rebuilds Bowie/followers and runs its raft tail; init
returns without a callback. The accepted entry boundary sets CURRENT_MAP=32/CURRENT_BATTLE=255 and
conditionally installs control. It does not disable shared search or force walking, membership or HP.
Map32's flag-block, step, roof and other-item tables are empty.

With only F67/F68, Peter and B/Oddler occupy physical 1/2. F65 alone adds no Caravan follower because
Map32 is not an overworld map. Elric has no follower row. Other supplied selectors, lookup replacements
and mode/raft state follow the shared owners; the passage's temporary Elric is not preserved by fresh
population. An empty local setup does not establish a particular cast.

Ordinary C first idles physical 0 and waits for movement/view scrolling. After the debug/Caravan
special branches, it tries GetActivatedEntity. An activated entity consumes that interaction even
when the cave's entity dispatcher returns without a callback: there is no fall-through to search.
That dispatcher writes EVENT_RELATIVE_POSITION, then the sentinel bypasses portrait, facing, entity
VInt and callback work.
Only C with no activated entity calls CheckArea with d6=1. FieldMenu's explicit Search clears d6
and calls CheckArea directly, then leaves the menu. Entity targeting can extend through counter
blocks; CheckArea inspects only the one faced tile.

CheckArea requires a nonnegative view target, clears the name index and speech value, computes the
faced tile from direction offsets and 384 internal units per tile, and reads the 64-wide working
layout. Its masked kind (`& $3C00`) takes precedence over description coordinates:

| Working-layout kind | Content selected | Initial / empty text IDs |
| --- | --- | --- |
| `$1800`, chest | OpenChest, cave chest table | 403 / 408 |
| `$2C00`, vase | CheckNonChestItem, empty cave other-item table | 404 / 409 |
| `$3000`, barrel | Same other-item consumer | 405 / 410 |
| `$3400`, bookshelf | Same other-item consumer | 427 / 412 |
| `$1C00`, searchable area | Same other-item consumer | 423 / 412 |
| Other masked values | Area-description dispatcher | Cave sentinel handles no description. |

None of the five item branches retries a description after an empty result, duplicate or failed
award. A chest-kind tile still needs a matching row. With another kind, the void description returns
unhandled: d6=1 returns d0=0 and C opens its menu; d6=0 emits investigation 423 then empty 412, closes
text and returns -1. Handled item/description paths return -1. A listed coordinate alone proves
neither the current tile kind nor accessibility.

The void area dispatcher clears d7 and tests its zero result. The void zone dispatcher skips its
callback and window closure but still waits a VInt and physical 0 position/destination equality;
it does not invent a cave random-battle event. The item-use dispatcher separately clears speech,
masks the item, writes relative position, clears d6 and skips its callback/window closure, while
retaining the VInt/physical 0 wait. That item-use boundary neither grants a searchable reward nor
establishes every generic item-use outcome.

## Four Rewards and the Inventory Boundary

**Confirmed:** the complete `map32/7-chest-items.asm` has these four rows and a terminator. Each
four-byte mapItem stores X, Y, flag byte and item byte; the flag is used directly, without a chest-base
addition. The enums supply the identities:

| Row | Coordinate | Collection flag | Item byte / reward |
| --- | --- | --- | --- |
| 0 | `(23,15)` | F160 | 4 / `$04`, Angel Wing |
| 1 | `(24,15)` | F161 | 15 / `$0F`, Brave Apple |
| 2 | `(25,15)` | F162 | 0 / `$00`, Medical Herb |
| 3 | `(19,26)` | F163 | 123 / `$7B`, Mithril |

Medical Herb's item 0 is a real reward. Empty is ITEM_NOTHING=127. All four are below the gold-chest
threshold 128; their names establish no use effects. GetChestItem/GetItem selects the current map's
chest table and its first X/Y match. CURRENT_BATTLE=255 skips battle-coordinate adjustment. No match
returns -1, masked to item 127; OpenChest changes no flag or block for that miss.

For a matching row, OpenChest substitutes 127 when its collection flag is already set. It then sets
the flag, writes open-chest block `$D802` and requests the plane update. Closed `$D801` and open `$D802`
both mask to the chest kind. A flag-set repeat emits 403/408 and grants nothing; possession of another
copy is not the duplicate predicate. Conversely, a supplied clear flag with an open-looking chest
can select its reward.

The [shared recipient and rollback owner](../../research/elven-village-conversations-and-local-exploration.md#four-item-rows-and-immediate-inventoryflag-consumers)
provides the item system detail. Its immediate consequences here are:

1. Found-item text 413 precedes testing ally index 0's four slots. With room, AddItem writes the first
   slot whose masked item ID is 127, retaining index/broken bits. The caller submits item music,
   emits 415 and uses FadeOut_WaitForP1Input. Index 0 is tested even if supplied membership omits Bowie;
   it is not a dynamic active-leader choice.
2. If index 0 is full, UpdateForce rebuilds joined members in ally-index order. The list convention
   OTHER_FORCE_MEMBERS_LIST=TARGETS_LIST+1 and the length-2/DBF scan skip its first joined entry. Under
   ordinary Bowie-joined state, the first other joined member with room receives the reward, including
   reserves and HP-zero members. The caller sets the recipient name, submits music, emits 416, waits
   and closes text. Missing Bowie membership does not repair that skip convention.
3. If no eligible free slot exists, including a negative initial scan counter, text 417 precedes
   restoring the searched coordinates and calling CloseChest then RefillNonChestItem. The chest flag
   clears, `$D801` returns and its plane update is requested. The cave's empty other-item table makes
   the second call a no-op. A later admitted search can retry. An already-taken chest follows its
   empty branch instead and never reaches this inventory rollback.

The caller checks capacity but does not branch on AddItem's returned d2 result. Coherent inventory
and returning services are assumptions of a successful handoff. Earlier flag/layout changes are not
a general transaction with automatic rollback if text, sound, waiting or another service stalls.

Text 403 uses `{NAME}{W2}{CLEAR}`; 408 has `{W1}`. Found-item 413 uses name/newline/item and
`{W2}{CLEAR}`. Receipt lines 415/416 use names/newline/item without a W-token and rely on the explicit
item wait; 417 uses newline and `{W1}`. The initial name is ally 0's current name. These controls
identify services and parameter flow, not completed display, inventory mutation or sound.

On a later full map load, LoadMapBlocksAndLayout loads base blocks/layout, applies flag-block rows,
then writes `$D802` for each set chest flag at its own coordinate. Thus F160–F163 independently
reapply open chests even though Map32's flag-block table is empty. They are outside the temporary
256–383 reset. Changing a flag alone does not redraw the current map, and current-map reload can
retain layout. Saved persistence and visibly open chests remain **Unknown**.

## Both Exits and the Return to Devil's Tail

**Confirmed:** Map32's complete external warp table has two rows:

| Row / cave coordinate | No-scroll request |
| --- | --- |
| 0 / `(2,3)` | Map67 `(51,23)`, Down 1, no-scroll 0 |
| 1 / `(29,3)` | Map67 `(57,24)`, Down 1, no-scroll 0 |

WarpIfSetAtPoint divides supplied internal coordinates by 384 and scans ordered rows with
negative-byte wildcards. Walking calls it only through the movement/layout `$1000` branch;
vehicle producers have separate checks. The selected row writes event 1, parameters 1–6 and WARP_SFX.
It tests none of F160–F163, F751, F13, F520, F421 or F521. Entry at the same cave coordinates is not
a new movement producer and proves no immediate bounce or westward route to Creed.

If retained until ProcessMapEvent, pending clears before dispatch. Type 1 no-scroll submits and
clears WARP_SFX, idles physical 0, unwinds ExplorationLoop and substitutes the nonnegative row
coordinates into the sampled position. MainLoop receives `(67,51,23,Down,d4=0)` or
`(67,57,24,Down,d4=0)` without normalizing PLAYER_TYPE. Later producers can replace the single pending
slot; a published request is not a consumed transition.

MainLoop calls SwitchMap then CheckBattle before fresh exploration. There is no flag-switched Map32
or Map67 replacement, no battle-table Map32 row, and exactly one Map67 battle row: slot 21, area
`(21,0)`, size 32x42, wildcard trigger 255/255. The area rectangle does not limit those triggers.
Supplied F421 admits Battle21 from either return, including `(57,24)` beyond the area's X extent.
F521 set clears F421 after selection and does not cancel admission; F521 clear retains it. Stop before
BattleLoop opening. Neither cave exit nor rock description sets these flags.

With F421 clear, fresh exploration clears pending, subtracts 20000 from STEP_COUNTER with signed-word
negative-result clamp to 0, heals eligible living/immortal allies, and fades. It sets
CURRENT_MAP=67/CURRENT_BATTLE=255, loads tilesets, rebuilds entities/lookups/mapsprites, clears flags
256–383, sets F80, loads map/view, installs base VInts and invokes init. That temporary reset leaves
F13, the four chest flags, F421/F521, F520/F751 and follower selectors intact.

Map67 has one six-pointer setup, an empty fixed entity list and return-only init. Shared population
places Bowie physical 0 at the requested tuple. Map67 is an overworld map: with only F65/F67/F68,
physical 1/2/3 are Caravan/A, Peter and Oddler/B. Extra selectors can add Sarah/Chester or replace A/B mappings.
These flags do not test membership/HP, and no Elric row is added. Fresh rebuilding discards cave-only
actors and map-local following.

The [shared mode/raft owner](../../research/polca-raft-boarding-through-south-parmecia-entry.md)
retains supplied state. In mode 2, physical 0 uses the raft sprite, followers park at `(64,64)` with
standing scripts and separate raft 31 is declared there. In other modes, raft 31 is declared only
with F64 and saved RAFT_MAP=67, using masked saved coordinates. Mode 1 does not repeat boarding.
Neither exit/init rewrites saved raft coordinates/map, disembarks or repairs arbitrary transport.

F751 separately gates the later loader's 3x2 block copy from `(0,0)` to `(56,22)`. The
[passage owner](../../research/harpy-pond-departure-through-fairy-cave-entry.md#live-block-change-completion-flag-and-request-return)
separates live copying, post-script flag write and later reapplication. Returning does not replay the
scene. A flag or text alone proves no passability, visible opening or live-layout repair.

After conditional music/fade, pending 0 lets WaitForEvent follow physical 0 and install control:
mode 0 walking; mode 1 source-named eas_Raft, actual Caravan; other values eas_Caravan, actual raft
for mode 2. A pending request wins first. This is conditional controller installation, with usable
input and service completion still **Unknown**.

## Arrival and Later Zone Selection Are Separate

**Confirmed:** east return `(57,24)` is not an F520 call-for-help coordinate. The accepted correction
preserves the complete Map67 zone selection: `(57,25)` selects the separate F751/F13/HP passage;
`(56,26)`, `(57,26)`, `(58,24)`, `(58,25)` select F520's call for help; other coordinates select the
default. Neither cave return dispatches a zone simply by arriving. Separately submitting either
return coordinate selects the default.

An admitted call-for-help event returns when F520 is set; otherwise it executes the
[complete accepted branch](../../research/elven-village-departure-through-harpy-pond-entry.md#complete-call-for-help-program),
including current B/A/Peter actions, text 2543–2549 and the final Map51 `(11,22)`, Up request.
Its [request/admission boundary](../../research/elven-village-departure-through-harpy-pond-entry.md#independent-pond-requests-and-admission-before-init)
retains partial-service/replacement limits. The `(57,25)` passage belongs to the
[separate accepted predicate and scene](../../research/harpy-pond-departure-through-fairy-cave-entry.md#separate-map67-zone-selection-and-passage-admission).
Neither branch is a consequence of inspection at `(57,23)`.

For a later default zone, walking's `$1400` producer submits event 6 and word X/Y before the raw-layout
blocking test. Only an admitted movement tail decrements a positive STEP_COUNTER; blocked movement
can leave the request without displacement/decrement. Caravan control has no equivalent zone
producer. Type 6 installs physical 0 Init; the wrapper selects/calls the zone, closes windows, waits
a VInt and physical 0 position/destination equality. Init installation does not prove completion.

Map67_DefaultZoneEvent loads BATTLE_DEVIL_TAIL=21 and calls CheckRandomBattle:

| Supplied state | Helper result |
| --- | --- |
| F521 clear | Request path regardless of counter; no RNG. |
| F521 set, counter nonzero, including negative | No new request/RNG; helper leaves counter unchanged. |
| F521 set, counter 0, range-8 RNG returns 0 | Request path; no second RNG. |
| F521 set, counter 0, first RNG nonzero | No request; range-4 RNG result+2 becomes counter 2–5. |

These are conditional source branches, not measured encounter probability. The request path sets
F421, writes packed `$000100FF` at MAP_EVENT_TYPE, sets counter 30000, waits view scroll, submits
SFX_BOOST and executes the flash script. It neither sets F521 nor calls a battle immediately.
Scroll/flash/wrapper completion and later request retention remain separate conditions.

The packed write supplies event 1, no-scroll parameter 1=0 and parameter 2=255/MAP_CURRENT; it leaves
parameters 3–5 and WARP_SFX unchanged. In the unchanged walking-zone chain, word parameter 3 still
contains zone Y: consumption substitutes X=0 and that Y, with residual parameter 5 as facing and
residual WARP_SFX submitted/cleared. This is not necessarily a reload at the unchanged position.
The [packed-reload owner](../../research/caravan-boarding-and-southwest-parmecia-travel.md#packed-reload-and-residual-parameters)
retains other supplied-byte and signed-coordinate cases.

After wrapper return, pending wins over control. Type 1 returns map 255; SwitchMap has no 255 row,
and CheckBattle resolves current Map67 and admits Battle21 if F421 remains set, regardless of those
residual coordinates. F521 again clears F421 after selection. Stop before opening. A replaced request
has its own continuation. If F421 clears before admission, no-battle map 255 follows the existing
population reload with main properties/raft, map load/init and conditional control, without fresh
entity setup or temporary reset. No uninterrupted route or automatic encounter is established.

## Inspecting the Rock and Hearing the Conditional Hint

**Confirmed:** Map67's complete area-description table has one function row at `(57,23)` with
condition byte 0, kind 1 and a table-relative offset to Map67_DescFunc0. The wrapper's d3=434 is not
an extra unconditional line. DisplayAreaDescription matches packed X/Y, bypasses the d6 filter for
condition 0, and calls the function with d6 saved/restored. C's d6=1 and explicit Search's d6=0 both
admit it if CheckArea reaches the description branch.

The five item kinds above preempt the description. Map67 has no chest rows; its sole other-item row
is `(45,9)`, F214, Mithril. At `(57,23)`, an item-kind layout therefore produces its empty result.
A `$1000` warp kind can reach this description when searched, while walking's warp producer is a
separate action. An activated ordinary C entity also intercepts inspection. Natural rock block kind
and access remain **Unknown**.

| Ordered condition | Dialogue and immediate consequences |
| --- | --- |
| Every admitted call | Investigation 424 uses `{NAME}` for ally 0, with `{W2}{CLEAR}`, then tests F751. |
| F751 set | 434 says nothing special, `{W1}`; return without Peter resolution or F13/HP tests. |
| F751 clear | 2590 describes a wobbly rock, `{W1}`; close text, copy current speech to SPEECH_SFX_COPY, resolve logical Peter 7 and write current portrait/speech, then test F13. |
| F751 clear, F13 clear | DisplayCurrentPortrait; 2591 attributes suspicion to Peter, `{N}{W1}`; no Elric HP lookup. |
| F751 clear, F13 set, Elric HP word 0 | Read combatant 13 HP; DisplayCurrentPortrait; 2592 suggests Elric's help, `{NAME;13}{N}{W1}`. |
| F751 clear, F13 set, Elric HP nonzero | Return with no further line or DisplayCurrentPortrait, after the already completed Peter resolution. |

The HP polarity is literal. F13 is joined membership, independently of active-party F45, so reserves
are not excluded. The passage zone instead requires nonzero Elric HP. Description neither joins,
heals or spawns Elric, moves actors, copies blocks, sets F751 nor requests a warp; F520 is not tested.
The hint establishes no fairy history or item solution.

GetEntityPortaitAndSpeechSfx (source spelling) follows Peter's current logical-7 lookup to the physical
actor and selects properties by its mapsprite. Peter base/promoted mappings supply portraits and
bleeps 4/5; an unmatched sprite uses the shared default. The resolver does not allocate Peter or
check F67, membership, active status or HP. Fresh unassigned lookup 0 can alias Bowie; stale/255
mappings are not safely rejected. Peter attribution therefore depends on actual mapping.
DisplayCurrentPortrait skips a negative portrait; otherwise OpenPortraitWindow can return for an
already open window or create/load/move/wait/register blinking. A call proves no newly rendered face.

These direct txt/clsTxt operations use Trap5_TextBox's register-preserving display/close services,
not the map-script single-text opcode or its debug-skip rules. CheckArea's name index 0 resolves
424's investigator name; `{NAME;13}` explicitly names combatant 13. `{N}` is newline; W1/W2 use
separate input-wait paths, with W2 cursor/validation handling; CLEAR clears dialogue layout.
Typewriting reads CURRENT_SPEECH_SFX for selected non-space glyph sound submissions. Display,
acknowledged input, audio and timing remain service limits rather than observed outcomes.

The speech copy is not restoration. CheckArea has cleared speech to 0 before copying it on the
F751-clear branch; no inspected return reads SPEECH_SFX_COPY back. DisplayAreaDescription closes
portrait/text and returns d7=-1; the outer dispatcher tests that result, and CheckArea closes text
again and returns d0=-1. Window closure and enclosing C/FieldMenu/exploration return restore neither
speech nor CURRENT_PORTRAIT. Resolved values therefore remain until another writer changes them,
including the joined, nonzero-HP branch without a Peter line. F751-set inspection leaves cleared
speech and never invokes that resolver. This is no global permanence or audible-result claim.

Map67's default entity/item callbacks are both return-only. The entity wrapper can still resolve
portrait/speech, suspend/resume entity VInts and close windows; modifier 0 requests no facing change.
The item wrapper retains its clear/mask/relative-position/window/VInt/physical 0-wait services.
Neither callback is the rock function or bypasses search precedence. Completed local return resumes
existing exploration/population subject to pending; no fresh load/control is forced before that check.
Stalled text/windows can leave intermediate state without rollback or an automatic passage action.

## Evidence and Remaining Questions

**Confirmed provenance:** this synthesis consumes the accepted research at main commit
`05e2fd194be07c6581e4fd3fe1abf3b38854ae65`, with original source
[SF2DISASM master `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm)
and USA retail ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
The [research reproduction boundary](../../research/fairy-cave-exploration-and-devils-tail-return.md#reproduction-and-retained-limits)
owns source paths, addresses, literal text/control joins and immediate consumers. No raw-source
reinvestigation, payload regeneration or new runtime acquisition is performed for this explanation.

| Reviewed surface | Claim boundary |
| --- | --- |
| Accepted exploration/return research and shared recipient, passage, call-for-help and packed-reload owners | Complete selected chest/exit/rock data and actual consumers. Original coordinates/text/actor effects remain research-owned; public aggregates alone do not reproduce them. |
| `sf2-field-search-control-static-v1`, [fixture](../../../tests/fixtures/h2/field-search-control-static-v1.json), fieldSearchSpine.callers/blockDispatch/itemRecipientPath/fullInventoryRollback/returnContract | Shared C/Search, kind precedence, four-slot recipient and rollback spine; not cave-specific rows or completed collection. |
| `sf2-map-events-static-v1` [index](../../../tests/fixtures/h2/map-events-static-v1.json), [entity](../../../tests/fixtures/h2/map-events/entity-programs.json) row 616, [item](../../../tests/fixtures/h2/map-events/item-programs.json) row 63, [zone](../../../tests/fixtures/h2/map-events/zone-programs.json) row 124 | Public ordered Map67 return-only callbacks and default Battle21 call, including source-line identities. These do not serialize complete rock content. |
| `sf2-map-descriptions-static-v1`, [fixture](../../../tests/fixtures/h2/map-descriptions-static-v1.json), expected.consumerFacts; [area-description contract](../contracts/map-area-description-routing.md) | Shared coordinate/condition/function-offset routing. The older sole-call d6=1 discussion covers that no-entity path; accepted field-search research also establishes explicit Search. Public conditioned rows do not contain this condition-0 rock body. |
| `sf2-map-event-random-battle-state-static-v1`, [fixture](../../../tests/fixtures/h2/map-event-random-battle-state-static-v1.json), eventRandomBattleState.callerSites.map67-default-battle21/completionFlagGate/stepCounterGate/randomCadence/requestWriteSequence | Helper branches and request services, distinct from arrival, later consumption and opening. |
| `sf2-map-init-static-v1`, [fixture](../../../tests/fixtures/h2/map-init-static-v1.json); `sf2-map-script-engine-static-v1`, [fixture](../../../tests/fixtures/h2/map-script-engine-static-v1.json) | Accepted retained setup/script comparisons support their source-shaped scope; complete missing-Map32/battle/follower and selected-table joins come from research. Aggregate equality proves no natural route or completed control. |

Schema/verifier boundaries reviewed are [field-search schema](../../../schemas/h2/field-search-control-static-fixture.schema.json)
and [field_search_control.py](../../../src/sf2tool/h2/field_search_control.py);
[event index schema](../../../schemas/h2/map-events-fixture-index.schema.json) and
[entity](../../../schemas/h2/map-events-entity-programs.schema.json),
[item](../../../schemas/h2/map-events-item-programs.schema.json),
[zone](../../../schemas/h2/map-events-zone-programs.schema.json) sections with
[map_events_fixture.py](../../../src/sf2tool/h2/map_events_fixture.py);
[description schema](../../../schemas/h2-map-descriptions-static-fixture.schema.json) and
[map_descriptions.py](../../../src/sf2tool/h2/map_descriptions.py).
[Exploration flow](../contracts/exploration-control-flow.md), [entry routing](../contracts/map-entry-routing-state.md),
[map exploration](../contracts/map-exploration.md), [dialogue](../contracts/dialogue-system.md) and
[party membership](../contracts/party-membership-state.md) retain their shared boundaries.
The narrower accepted C/Search distinction is used without expanding older contract Unknowns.

Preserve [PR882's original/rebased handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/880#issuecomment-6172113709),
[independent acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/882#issuecomment-6172145421)
and [east-return dispatch correction](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/880#issuecomment-6171924743),
including retained comparisons, original/final CI, resolved inspection failures and linked predecessor
receipts. Those review records locate results; tracked research and exact accepted Git objects own
original-game claims. This guide's direct checks, committed planner and final-head CI belong to its
Issue handoff. It creates no new evidence system.

**Unknown:** natural cave/chest/rock/exit terrain and layout kinds; actual flags, inventory, recipient,
HP, membership, actor mappings, mode and saved raft; completed text/window/movement/scroll/flash/load
services, visible or audible results, usable control, saved persistence and natural Battle21 admission.
Item 0 versus empty 127, duplicate/full inventory, missing Bowie, entity-intercepted C versus Search,
preempting tile kinds, independent F751/F13/HP, reserve/dead Elric, missing Peter, independent
F421/F521, negative counters and replaced requests constrain static claims; they are not executed
scenarios. Unknowns do not automatically queue runtime work or complete the campaign/design epic.
