# Floor World interactions and outbound exits

Evidence date: 2026-10-14. **Confirmed:** Map26 has six resident callbacks,
one F780-gated zone scene, and three no-scroll warp records. The scene speaks
as resident129, requests an eight-tile upward move, waits for that actor,
hides it, then returns before the caller sets F780. No selected exit tests
F780. **Inferred:** the scene depicts the young man's unsuccessful attempt
to lift the giant ring. **Unknown:** natural approach, supplied state,
completed services and movement, actual presentation, usable control and
persistence.

## Agreement and provenance

Standard rigor under [Issue #924](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/924)
and [content epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
The uncertainty lies in local activation, zone/action/hide and outbound
request-consumer joins. This evidence supports the next English story/design
direction. Starting accepted base is
`373b1a6a20f7ad2041e95ca093fdf4c270e8b7a8`; no stacked dependency.
This document is the sole owned path. No shared index, registry, schema,
fixture, tooling, design or remake change is required.

Begin after accepted [first-arrival conditional control](./floor-world-first-arrival.md).
Consume the [interaction-state](../../world/map-event-interaction-state.md),
[setup-selection](../../world/map-event-flag-route-selection.md),
[request-consumption](../../world/map-event-request-consumption.md),
[common-menu](../../presentation/common-menus.md) and
[service contract](../../../design/contracts/party/service-interactions.md) owners.
Stop at outbound request consumption and normal map/battle admission, before
destination population/init, tunnel scripts, Battle23 initialization or later
Creed/recruitment content. No runtime acquisition, service-algorithm expansion,
general comparator, new fixture or verification machinery is admitted.

All source paths below are relative to `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at that pin. Retained evidence binds USA ROM
SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Short setup paths mean `data/maps/entries/map26/mapsetups/`; map/entity
script consumers mean `code/common/scripting/map/` and
`code/common/scripting/entity/`. Fnnn means flag nnn. Indices are zero-based;
coordinates, speeds and waits are source operands, not measured performance.

Reuse the unchanged 18,285,979-byte map-init/map-script-engine pair, selected
existing fixtures and bounded source reads. Linear comparisons are budgeted
below 512 MiB and one minute, plus prose and small receipts. No payload
regeneration, installation, new environment or capture is needed; reassess
before material expansion. Equality alone cannot establish natural continuity.

## Resident activation and all callbacks

**Confirmed:** `data/maps/mapsetups.asm:90–91` has only the default Map26
setup. Its pointer table selects the local entity/zone/description/item
tables. Neither F779 nor F780 selects a different event table. The arrival
owner supplies the nine population rows and their allocation conditions.
With only Peter/B followers and coherent distinct lookups, Randolf16,
Tyrin17,128,129,130–132,133,134 occupy physical slots3–11 respectively.
These are conditional example slots, not stable identities: follower flags,
ally declaration skips, aliases and supplied state can change them.

The ordinary activation path is
`code/gameflow/exploration/explorationvints.asm:ProcessPlayerAction` →
`explorationfunctions_0.asm:GetActivatedEntity` →
`code/gameflow/battle/battlefunctions/battlefunctions_0.asm:GetEntityEventIndex`
→ `mapsetupsfunctions_1.asm:RunMapSetupEntityEvent` (`$4761A`).
ProcessPlayerAction first idles physical0 and waits for its destination and
view scrolling. A opens the field menu. C reaches resident activation only
after the debug and overlapping physical1-Caravan alternatives.

With ordinary view target0, GetActivatedEntity uses the player's current
position/facing, advances across desk/counter blocks (`$2800` under `$3C00`),
then selects the first eligible physical actor at Manhattan distance less
than MAP_TILE_SIZE=384 from that candidate. It skips the viewed actor and
current followers. Negative view target returns without activation; arbitrary
nonzero camera state is not a proven normal activation context. The successful
return gives saved actor facing in d1 and player facing in d2.
GetEntityEventIndex reverse-searches ENTITY_INDEX_LIST, switches its logical
counter to128 after the ally segment, and falls back to logical0 on no match.
Aliases can select an earlier logical identity than the expected resident.

RunMapSetupEntityEvent stores d2 in EVENT_RELATIVE_POSITION, scans four-byte
records for the first logical-ID match or `$FD` default, resolves the actual
mapsprite's portrait/speech and resolves the actor again for facing changes.
The second `msEntityEvent` operand is an action bitfield, despite the macro's
facing comment. UP=1 turns to `(playerFacing+2)&3` after a VInt, without saved
facing restoration. DOWN=3 turns and restores the saved facing after return.
RIGHT=0 does neither. They are not required approach directions.

The complete `s2_entityevents.asm` (`$59D6A..$59DB6`) has six explicit rows
and one default. Event N means Map26_EntityEventN. The following is the whole
callback work, including returns, not merely a sample walkthrough.

| Logical resident; baseline placement | Row / callback | Ordered callback work |
| --- | --- | --- |
| Randolf16; `(27,22)` Right, eas_Init | UP1 / Event0 | txt1742, txt1743, rts |
| Tyrin17; `(29,22)` Left, eas_Init | UP1 / Event1 | txt1744, txt1745, rts |
| Old man128; walking around `(50,12)`, radius3 | UP1 / Event2 | txt1746, txt1747, rts |
| Young man129; `(5,24)` Down, eas_Init | UP1 / Event3 | rts |
| Priest133; `(4,35)` Down, eas_Init | DOWN3 / Event4 | jsr j_ChurchMenu, rts |
| Caravan134; `(11,35)` Down, eas_Init | RIGHT0 / Event5 | jsr j_CaravanMenu, rts |
| Unmatched logical identity | default0 / Event3 | rts |

Objects130–132 have no explicit row. The young man's direct interaction is
silent; it does not start the ring scene. Even the no-op callback is inside
the wrapper's portrait/speech/facing work. Before invoking any callback the
wrapper clears the VInt_UpdateEntities flag. A returning callback permits
optional facing restoration, portrait/text closure and reactivation of that
VInt function. No actor destination wait is added by this wrapper. Its caller's
earlier physical0 wait and the zone wrapper's later wait are distinct.

`data/scripting/text/gamescript.txt` keys06CE–06D3 are IDs1742–1747:

| IDs | Content and termination |
| --- | --- |
| 1742 / 1743 | Randolf questions wanting to see Creed (W2), then warns against the hole in the wall leading to him (W1). |
| 1744 / 1745 | Tyrin says they came down to escape but devils live in the tunnel (W2), then says they are too ferocious and he will stay in Floor World (W1). |
| 1746 / 1747 | The old man says they left Desktop Kingdom long ago (W2), then calls `{NAME;16}` a brave trusted leader (W1). |

NAME16 names ally16/Randolf through the text's explicit name-index form;
`textfunctions_1.asm:symbol_player` consumes the next text symbol and calls
j_GetCombatantName. It is not the current party leader selector or recruitment.
These callbacks contain no join, party update, flag write, item receipt,
map request or EGRESS write. Their statements supply narrative context, not
proof of a prior escape or permission to enter another map. Repeats use the
same text; no conversation flag gates or suppresses them.

### Speaker properties and existing service boundaries

`getentityportaitandspeechsfx.asm:GetEntityPortaitAndSpeechSfx` resolves the
logical actor, reads the actual MAPSPRITE byte and searches
`data/spritedialogproperties.asm`. Baseline Randolf sprites39/40/41 select
portrait16 and bleep6; Tyrin42/43/44 select portrait17 and bleep6. Old man198
selects NONE/bleep6, young man196 NONE/bleep2, priest192 portrait34/bleep5.
Caravan62 has no matching entry and receives DEFAULT/bleep6. NONE255 is
sign-extended to -1; DEFAULT is also -1. Neither opens a new portrait.
Actual class/HP-dependent ally sprites or altered mappings may select other
properties. No callback-level HP/recruitment guard is present.

The shared resolver maps NPC129 through lookup129−96=33; a zero lookup
addresses physical0 rather than safely rejecting an absent actor. Speaker
identity, movement and hiding therefore require coherent mappings. A portrait
already open is not proof that a new portrait was selected.

Event4's j_ChurchMenu alias at `$20004` reaches ChurchMenu `$20A02`
(`code/common/menus/church/churchactions_1.asm`). Event5's j_CaravanMenu
at `$20010` reaches CaravanMenu `$21FD2`
(`code/common/menus/caravan/caravanactions_1.asm`), through the S05 jump
interface. Each local callback returns only after its service returns.
There is no local F65, F780, vehicle-mode or story prerequisite in these
callbacks. Do not import the separate C-button Caravan shortcut's shrink,
music and view-update work into Event5.

Use the existing service contract for Church and Caravan selection, cancellation
and transactions. Its controlled Church seams do not prove this caller's
natural reach or successful return. In particular, local absence of an EGRESS
write does not mean a selected Church Save action cannot change EGRESS.
No completed heal/save/promotion, depot transfer, party change, persistence
or restored control is established by these two calls. Service algorithms
are outside this slice.

## Zone admission, ring scene and return

**Confirmed:** `s3_zoneevents.asm` (`$59DB6..$59DD2`) contains exactly
`msZoneEvent 14,255,Map26_ZoneEvent0` and the default return callback.
RunMapSetupZoneEvent (`$4751A`, `mapsetupsfunctions_1.asm:30–77`) compares
the low bytes of d1/d2 with X/Y; `$FF` is a wildcard. Thus the selected row
requires X14 and any Y. It does not mean tile `(14,255)`, all tiles in column14
automatically trigger, or the party's current X is necessarily14.

In `entityscriptengine_2.asm:esc02_controlCharacter`, current coordinates
seed a movement candidate. Direction, area bounds, slope handling and entity
obstruction precede its layout-event classification. At loc_5220/loc_5294,
the candidate block masked by `$3C00` must equal `$1400` to write
MAP_EVENT_ZONE_EVENT=6. The writer stores word parameters derived from
`(XDEST+d4)/384` and `(YDEST+d5)/384`, where d4/d5 are the selected step
deltas. It does not store EVENT_RELATIVE_POSITION. These destination-derived
event coordinates are not the current-position reads, and the write precedes
the remaining terrain/debug/collision branch and movement assignment.
Consequently structural admission proves neither a completed step nor natural
reach to the zone. The working layout, action, mode, bounds and state matter.

WaitForEvent prioritizes a pending nonzero event over A/C input and skips
controller installation if one is already pending. ProcessMapEvent clears
MAP_EVENT_TYPE before dispatching its sampled type. Type6 first calls
j_ApplyInitActscript for physical0, installing eas_Init without waiting for
it to finish, then loads d1 from PARAM1 and d2 from PARAM3 and invokes
j_RunMapSetupZoneEvent. This register context, not the actor-interaction
relative-position byte, feeds the zone comparisons.

| Selected callback state | Ordered work |
| --- | --- |
| X14, F780 clear when checked | script cs_59F20; on return setFlg780; return |
| X14, F780 set | Branch directly to return; no scene or new hide |
| No specific row match | Map26_DefaultZoneEvent returns |

The callbacks do not test F779, party membership, ring ownership or a resident
position. A supplied clear F780 can therefore admit the scene even with
incoherent/missing actors; a supplied set F780 suppresses it even if no attempt
occurred. F780 is checked on entry, not reserved atomically for the scene.

### Every scene command and immediate consumer

`scripts.asm:cs_59F20` at `$59F20..$59F42`, lines4–13, has eight top-level
commands and one inline movement body:

| Index | Complete command / body |
| --- | --- |
| 0 | textCursor1740 |
| 1 | setCamDest0,20 |
| 2 | nextSingleText modifier0, actor129: ID1740 |
| 3 | csWait50 |
| 4 | nextSingleText modifier0, actor129: ID1741 |
| 5 | entityActionsWait129: moveUp8; endActions |
| 6 | hide129 |
| 7 | csc_end |

The script trap activates VInt_UpdateEntities before ExecuteMapScript.
csc04 writes CUTSCENE_DIALOG_INDEX=1740. csc32 sets view target -1, passes
0/20 to j_SetCameraDestination, then waits for view scrolling. The effective
SetCameraDestination in `battlefunctions_0.asm` multiplies both operands by
384 and calls SetViewDestination in `code/common/tech/graphics/display.asm`.
That consumer applies the map area's parallax factors and layer/background
offsets, writes each plane's destination only when that axis is not under
autoscroll, then invokes its scroll-data helpers. This is a view request;
it neither teleports the player nor defines resident129's movement origin.
Actual area parameters, scrolling completion and presentation remain
supplied-state/service premises, not observed camera framing.

`sf2cutscenemacros.asm:nextSingleText` encodes modifier0/actor129 as `$0081`,
not the `$FFFF` no-speaker sentinel. `mapscriptengine_2.asm:csc00` calls
csc1D for portrait handling and obtains the actual actor's speech properties.
Modifier0 requests neither mirroring nor right-side placement. With the
baseline young-man sprite no new portrait opens and bleep2 is selected.
csc1D preserves an already open portrait; csc00 still resolves speech.
Each unskipped command waits for scrolling, displays the cursor's text,
increments the cursor, closes portrait/text and sleeps10.

Literal1740/key06CC is `Ungh!{W2}`. Literal1741/key06CD says the object is too
heavy, he cannot lift it and gives up, ending W1. In
`code/common/scripting/text/textfunctions_1.asm`, W2's @wait2 and W1's
symbol_wait1 poll the directional/A/B/C input mask with VInt waits; W2 also
updates its prompt marker and requests SFX_VALIDATION on acceptance. W1
does not add that W2 validation request. The explicit csWait50 is a sleep,
not an action or physics completion condition; no measured duration follows.

ExecuteMapScript clears SKIP_CUTSCENE_TEXT at entry; debug mode plus P2 Start
can latch it during command dispatch. A skipped csc00 consumes its operands
without display or cursor increment, and script sleep commands are bypassed.
Camera, movement, hide and caller flag work still execute. If skip latches
between the two text commands, the later cursor/value differs from the fully
displayed path. No unconditional claim that both lines were seen is valid.

csc2D resolves129, seeds its wait timer with the physical index, clears FLAGS_A
bits5/6 (`$9F`) and compiles moveUp8 into esc04 relative `(0,-8)`, esc01's
destination wait, then an appended esc34 jump to eas_Idle. It installs that
action and, because the wait byte is `$FF`, polls **that actor's** action
pointer until idle. There is no all-actor barrier, HP guard or timeout.

`entityscriptengine_2.asm:esc04` multiplies the signed delta by384 and adds
it to **current** position when executed, not the original population row or
previous destination. With bit5 clear, HasSameDestinationAsOtherEntity returns
Z set and esc04's conflict branch is bypassed. At loc_55C8 it writes destination,
travel and signed velocities using that actor's current speed bytes. This
scene sets no speed. The baseline eas_Init writes32/32 if it has completed
and those values remain unchanged; the zone's physical0 Init is not an Init
of129. From unchanged `(5,24)` the requested destination is `(5,16)`, but a
different current position or speed yields different movement/progress.

esc01 tests current X/Y against the actor's current destination; it does not
retain an immutable original target. The waited pointer can be replaced,
updates can stall and an already parked actor at raw X≥`$7000` is skipped by
VInt_UpdateEntities. Movement completion under coherent uninterrupted state
does not establish exact timing or collision physics. The commands contain
no ring lift, mass, attachment, inventory receipt or force test. The failed
lifting interpretation comes from text/context, not a simulated lifting rule.

### Hide versus later parking, flag order and usable control

csc2E resolves129 again and calls `entityfunctions_2.asm:HideEntity`. It
writes current/destination X/Y to raw `$7000,$7000`, invalidates **all** matching
ENTITY_INDEX_LIST entries with `$FF`, and removes a matching follower-list
entry by shifting EXPLORATION_ENTITIES. It does not recruit, grant an item,
free an inventory slot or request a warp. Alias changes can redirect the
hide or invalidate several logical identities. After invalidation the resolver
has no safe `$FF` missing-actor no-op; later supplied-state accesses must not
be treated as an ordinary valid129.

csc_end conditionally waits scrolling if a dialogue window remains, clears
VIEW_SCROLLING_SPEED and restores caller registers. Only its return permits
the zone callback to set F780. The zone wrapper then closes portrait/text,
waits one VInt and waits for **physical0** current position to equal its current
destination, restoring saved registers on return. This is separate from129's
scene wait and does not join every actor. The exploration loop then reaches
SetMoveSfx/WaitForEvent: no pending event permits view target0 and the
current-mode controller installation; a pending event takes precedence.
No unconditional usable-control claim follows from the callback's rts.

Compare the accepted arrival owner's independent F780 init tail:
MoveEntityOutOfMap → j_GetEntityIndexForCombatant → j_SetEntityPosition
only writes current/destination X/Y. It preserves the lookup and follower
list. A later explicit fresh Map26 load rebuilds the population and then
parks129 if F780 remains set; that differs from same-visit HideEntity lookup
invalidation. MAP_CURRENT does not rebuild those lookups. Its later init tail
would receive the supplied existing mappings, including invalidated ones;
no safe hide-to-parking continuity is claimed here. Destination init execution
is beyond this slice, while the accepted arrival owner retains its behavior.

Failure before scene return can leave camera/text/action/hide effects with
F780 clear. Failure after hide but before setFlg780 can permit a later attempt
with invalidated lookup. Conversely, supplied F780 set can skip hiding a
present resident. There is no general rollback or repair in the zone callback.
These are source-order distinctions, not observed interruptions or persistence.

## Scoped absence of local search and item data

**Confirmed:** `s4_descriptions.asm` (`$59DD2..$59DD4`) is only rts.
RunMapSetupAreaDescription starts d7=0, so this callback supplies no handled
description. `s5_itemevents.asm` (`$59DD4..$59DDC`) contains only the default
record and Map26_DefaultItemEvent0's rts. RunMapSetupItemEvent starts d6=0;
the local callback contributes no handled item use, mutation or request.
Its wrapper still clears speech, stores the item caller's d2/Y into
EVENT_RELATIVE_POSITION, closes windows and waits for physical0 after return.

`data/maps/entries/map26/7-chest-items.asm` (`$A8CEC..$A8CEE`) and
`8-other-items.asm` (`$A8CEE..$A8CF0`) each contain only endWord.
`exploration.asm:GetChestItem/GetNonChestItem` select the current map's lists;
GetItem sees the negative first byte and returns -1 for the missing coordinate,
flag and item results. These lists supply no local item record. CheckArea has
other layout-dependent paths, and field/item/menu behavior has other owners.
This does not prove no search response, held item, service reward or collectible
anywhere. In particular the scene's narrative ring is not a granted item.

## All three outbound records through admission

**Confirmed:** `data/maps/entries/map26/6-warp-events.asm` at
`$A8CD2..$A8CEC` has exactly three eight-byte records followed by endWord.
`sf2mapmacros.asm:159–184` encodes trigger X/Y, type0 for warpNoScroll,
target map, destination X/Y, facing and a final zero byte.
`sf2enums.asm` gives MAP_FLOOR_WORLD=26, MAP_WILLARD_BURROW=27,
MAP_CURRENT=255 and UP=1.

| Record | Trigger X/Y | Map byte | Destination X/Y/facing | Type |
| --- | --- | --- | --- | --- |
| 0 | `(59,3)` | 27, WILLARD_BURROW | `(2,7)`, Up1 | 0 |
| 1 | `(32,11)` | 255, CURRENT | `(12,41)`, Up1 | 0 |
| 2 | `(12,42)` | 255, CURRENT | `(32,12)`, Up1 | 0 |

UP is the destination facing, not a required approach direction. All trigger
coordinates are exact; no selected row uses the negative-coordinate wildcard.
In the ordinary walking consumer esc02, a candidate layout classification
`$1000` calls `exploration.asm:WarpIfSetAtPoint` (`:1377–1423`). It divides
the supplied signed raw X/Y by384, selects CURRENT_MAP's warp table, scans
in order and stops at the first match or word -1. On a match it writes
MAP_EVENT_TYPE=1, copies type/map/X/Y into PARAM1–4 and facing/padding into
PARAM5–6, then requests WARP_SFX=SFX_WARP. Merely standing at a listed tile
is not proof this movement consumer ran; layout, action and actual candidate
state remain premises. Other vehicle-control paths also call this common
matcher; their movement legality is not expanded here.

The event storage is a shared mutable request, not a queued immutable warp.
WaitForEvent prioritizes its nonzero type over input, and ProcessMapEvent
clears the type before dispatch. The selected consumer reads the parameters
then present; an overwritten/supplied request can change the outcome. All
three rows have PARAM1=0, selecting the no-scroll arm of
`explorationfunctions_2.asm:ProcessMapEventType1_Warp` (`:228–259`).
It requests the stored sound, clears WARP_SFX, idles physical0, removes its
caller return address to leave ExplorationLoop, and zeroes d0–d4. It loads
the map into d0, samples actual player X/Y/facing through
UpdatePlayerPosFromMapEvent, then replaces only the low X/Y bytes with each
nonnegative destination byte and always loads facing. With coherent
nonnegative local coordinates, the outbound tuples equal the table values
with d4=0. The signed divisions in UpdatePlayerPosFromMapEvent first fill
the coordinate words; the later move.b writes do not normalize their high
bytes. For example, supplied raw X=-384 gives quotient `$FFFF`; destination2
then produces d1.w=`$FF02`, not `$0002`. The same limitation applies to Y.
It returns directly to MainLoop. It is not the nonzero-type scroll-transition
arm, and it does not execute destination initialization itself.

`MainLoop` calls SwitchMap then CheckBattle before selecting exploration or
battle. `data/maps/global/flagswitchedmaps.asm` has no26/27/255 substitution.
CheckBattle resolves MAP_CURRENT through CURRENT_MAP for its lookup, retaining
the original255 request on the no-battle return. The complete
`data/battles/global/battlemapcoords.asm` has no Map26 row; the two current-map
requests therefore admit exploration with d0=255 when CURRENT_MAP is26.
This does not convert them into explicit Map26 reloads.

Map27 has the Battle23 row with wildcard triggers. Its ordinary admission
depends on the battle-unlocked flag423; clear skips that row and admits Map27
exploration. Set admits the battle path; CheckBattle's completed-flag523 test
may clear423 but does not cancel that current selection. These are generic
admission effects only. No selected resident/ring/warp callback sets423.
Stop before either destination population/init or Battle23 initialization;
the tunnel and its later scripts are only the next boundary.

At the existing exploration entry boundary, d0=255 preserves CURRENT_MAP,
population/lookups and working layout, skips explicit-load tilesets/entity
allocation, temporary-flag clearing and F80 set, and uses the existing
UpdateMainEntityProperties/DeclareRaftEntity path. Under the same coherent
coordinate premise, the destination values update main current/destination
and facing; retained actors are not reset to their population rows.
UpdateMainEntityProperties tests each coordinate's low byte for negativity,
then multiplies the whole word by384 and stores the low product word. It does
not clear a retained abnormal high byte before that multiplication. It still
passes through the common exploration entry work and later init dispatch;
it is not a position-only
shortcut. Explicit-map exploration instead selects the new map/population,
sets CURRENT_BATTLE=255 and clears temporary flags256–383 before later init.
F779/F780 are outside that temporary range. Map27 battle admission follows
its separate owner, not these exploration effects.

None of the selected warp records, ordinary match/consume functions,
SwitchMap rows or battle-admission predicates tests F780. F780-clear and
F780-set states thus have the same structural exit admission for identical
other inputs. Natural reach, geometry and later destination behavior remain
separate. These exits themselves neither consume a ring/balloon nor write
EGRESS_MAP, PLAYER_TYPE, saved raft state or a new story/recruitment flag.

## Reproduction and retained limits

Load `. ./local/private-inputs.ps1` in the same process as controlled commands;
verify the clean read-only source pin above. Use the existing
[comparison recipe](../grans-opening/battle06-to-battle07-content-route.md#reproduction-and-review-boundary)
for retained map-init equality, _verify_map_script_engine_fixture and
_cutscene_facts; do not regenerate either payload. Select cs_59F20 from
`local/derived/map-script-engine-static.json`'s programCorpus.programs and
compare all eight source-line/macro/operand records, then read its complete
moveUp8/endActions body and each immediate consumer named above.

Under `tests/fixtures/h2/map-events/`, routing-setup.json retains Map26's
seven entity rows, two zone rows and default item record. entity-programs.json
programOrder298–303 retains all six entity callbacks; zone-programs.json
programOrder50/51 retains the guarded scene caller and default. Match every
selected operation to its pinned source line, including branch polarity,
script-before-flag order and returns. These numbers are corpus identities,
not gameplay ordering. The request-consumption fixture
`tests/fixtures/h2/map-event-request-consumption-static-v1.json` retains the
explorationLoop/waitForEvent/processMapEvent contexts; it does not itself
prove this particular warp or zone naturally reaches them.

Direct source checks also cover all three complete warp records, numeric
enums/macros, all eight text IDs1740–1747, the fixed-name consumer, actual
activation/zone/warp register contexts, camera/action/hide consumers,
empty local description/item/chest/other-item data and admission tables.
Existing service owners define the unchanged call/return boundary.
Review the entire owned prose/diff, provenance, relative links/anchors,
English scope and private/generated boundary; run git diff --check and the
configured clean committed `uv run sf2 verify plan --base origin/main --head HEAD`.
Record the plan and actual final-head CI on Issue924. Documentation scope
requires no local blanket normal/full/engine/Godot/H3/H4 suites or new tests
of verification programs. Aggregate equality does not guard every prose claim.

Preserve [#920 original handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/920#issuecomment-6175992255),
[rebased handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/920#issuecomment-6176088792)
and [PR922 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/922#issuecomment-6176117381):
original head `bd40b052b04ab087738f5fc170e406f97a244ad6`, rebased head
`7a83e84530bf01bd41c3e071886a24b086a5783e`, separate successful
CI38619448174/38620203806, retained pair/0.079s, all inspection/transport
failures and linked predecessor corrections. Their exact trees, bases and
completed results remain in those handoffs; this slice replaces none of them.

**Unknown:** natural arrival/activation/zone/exit reach; actual flags,
positions/destinations, layout, lookups/aliases, party/HP/class, travel mode
and request state; completed services, text/input, camera, movement, hiding
and control; visible ring performance, exact duration/audio, usable
exploration and save/load persistence. Changed-state cases above are static
distinctions, not played scenarios. [ADR0014](../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../../../decisions/0016-remake-start-evidence-deferral.md) keep those
limits explicit without creating an automatic runtime queue.
