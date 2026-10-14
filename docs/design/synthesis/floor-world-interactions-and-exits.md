# Floor World Residents, the Giant Ring and Outbound Exits

**Confirmed:** Floor World has six resident callbacks plus a silent default,
a separate F780-gated zone scene, and three no-scroll warp records. The scene
speaks as resident129, requests an eight-tile upward movement, waits for that
actor and hides it; only scene return permits the caller to set F780. None of
the selected exits tests F780. **Inferred:** the scene portrays an unsuccessful
attempt to lift the giant ring. **Unknown:** natural reach, actual supplied
state, completed services/actions, presentation, usable control and persistence.

This English guide continues [first arrival and conditional exploration](floor-world-first-arrival.md)
from its local control/pending-event boundary. It consumes accepted corrected
[interaction and exit research](../../research/floor-world-interactions-and-exits.md)
under [Issue #930](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/930)
and the [Standard agreement](../documentation-roadmap.md#current-baseline-and-near-term-synthesis).
It ends at conditional destination map/battle admission, before destination
population/init, tunnel content, Battle23 initialization or later recruitment.
Fnnn means flag nnn; indices are zero-based. Coordinates, speed bytes and waits
are source operands, not measured motion, pixels or elapsed time.

## Speaking to Residents Does Not Advance the Ring Scene

**Confirmed:** Map26 has one default setup. F779/F780 do not select different
resident, zone or item tables. The arrival guide owns the nine population rows
and their allocation. With only Peter/B followers, no skipped ally declarations
and coherent distinct lookups, Randolf16, Tyrin17,128,129,130–132,133,134 occupy
physical slots3–11. These are conditional examples, not stable actor identities.
Followers, HP/class-dependent sprites, skipped declarations and aliases remain
supplied premises; presence of Randolf/Tyrin is not recruitment.

Ordinary resident activation begins with ProcessPlayerAction idling physical0
and waiting for its destination and view scrolling. A opens the field menu;
C reaches this path after debug and overlapping physical1-Caravan alternatives.
With ordinary view target0, GetActivatedEntity uses current player position
and facing, advances across desk/counter blocks, and selects the first eligible
physical actor within Manhattan distance less than MAP_TILE_SIZE=384 from the
candidate point. It skips the viewed actor/current followers. Negative view
target admits no activation; arbitrary nonzero view state is not established
as ordinary resident access.

GetEntityEventIndex reverse-searches ENTITY_INDEX_LIST, switches to logical128
after the ally segment, and falls back to logical0 on no match. Aliases can
select another logical identity. The activation return supplies saved actor
facing in d1 and player facing in d2. RunMapSetupEntityEvent stores d2 in
EVENT_RELATIVE_POSITION and selects the first logical-ID match or default.
This byte therefore carries player facing here, not the zone coordinates.

The complete table and callbacks are `s2_entityevents.asm` at `$59D6A..$59DB6`.
EventN below means Map26_EntityEventN. The second record operand is an action
bitfield, although the macro comment calls it facing. UP1 turns the actor to
`(playerFacing+2)&3` after a VInt without restoring saved facing; DOWN3 turns
and restores it after return; RIGHT0 does neither. These are not required
approach directions.

| Resident; baseline placement | Record / callback | Complete ordered callback work |
| --- | --- | --- |
| Randolf16; `(27,22)`, Right | UP1 / Event0 | txt1742; txt1743; return |
| Tyrin17; `(29,22)`, Left | UP1 / Event1 | txt1744; txt1745; return |
| Old man128; walking around `(50,12)`, radius3 | UP1 / Event2 | txt1746; txt1747; return |
| Young man129; `(5,24)`, Down | UP1 / Event3 | Return silently |
| Priest133; `(4,35)`, Down | DOWN3 / Event4 | Call j_ChurchMenu; return after the service returns |
| Caravan134; `(11,35)`, Down | RIGHT0 / Event5 | Call j_CaravanMenu; return after the service returns |
| Unmatched logical identity | default0 / Event3 | Return silently |

Objects130–132 have no explicit callback row. Direct interaction with129 does
not start the ring scene. A silent callback still passes through portrait,
speech and facing work. The wrapper resolves the actual mapsprite and then the
physical actor again, clears VInt_UpdateEntities before the callback, and on
return optionally restores facing, closes portrait/text and reactivates updates.
It adds no actor-destination wait. The earlier physical0 activation wait is a
separate condition.

### The Conversations Supply Context, Not Permission or Recruitment

All six selected dialogue IDs come from gamescript keys06CE–06D3. They repeat
without a local conversation flag; each callback has the same ordered pair.

| IDs | What the resident says; text termination |
| --- | --- |
| 1742 / 1743 | Randolf questions wanting to see Creed (W2), then warns against the hole in the wall leading to him (W1). |
| 1744 / 1745 | Tyrin describes coming down to escape but finding devils in the tunnel (W2), then says they are too ferocious and he will stay in Floor World (W1). |
| 1746 / 1747 | The old man describes leaving Desktop Kingdom long ago (W2), then calls `{NAME;16}` a brave trusted leader (W1). |

NAME16 is the explicit ally16/Randolf name index. The text consumer reads the
following symbol and calls j_GetCombatantName; it does not select the current
party leader. None of these callbacks joins an ally, updates the party, writes
a flag, grants an item, requests a map or writes EGRESS_MAP. Their words provide
narrative context, not proof of earlier escape or permission to leave.

GetEntityPortaitAndSpeechSfx uses the resolved actor's actual MAPSPRITE byte.
Baseline Randolf sprites39–41 select portrait16/bleep6; Tyrin42–44 select
portrait17/bleep6; old man198 NONE/bleep6; young man196 NONE/bleep2; priest192
portrait34/bleep5. Caravan62 has no matching property row and gets DEFAULT/bleep6.
NONE255 sign-extends to -1, as does DEFAULT; neither requests a new portrait.
A preexisting portrait is not evidence of a newly selected one. Altered ally
sprites or mappings can change these properties; no callback HP guard repairs
them. NPC129 resolves through lookup129−96=33. A zero missing lookup addresses
physical0, rather than safely rejecting the absent resident.

### Church and Caravan Remain Call-and-Return Boundaries

Event4 calls the S05 alias j_ChurchMenu `$20004` → ChurchMenu `$20A02`;
Event5 calls j_CaravanMenu `$20010` → CaravanMenu `$21FD2`. Neither callback
tests F65, F780, travel mode or a story prerequisite. Event5 does not include
the separate C-button Caravan shortcut's shrink, music or view-update work.

The [service contract](../contracts/service-interactions.md) owns Church and
Caravan choices, cancellation and transaction algorithms. Its controlled Church
seams do not prove natural reach or successful return from this Map26 caller.
Local absence of an EGRESS write does not exclude Church Save's own EGRESS
mutation. A call establishes no completed heal/save/promotion, depot transfer,
party change, persistence or restored control. Those outcomes remain separate.

## The Ring Attempt Has Its Own Admission and Completion Conditions

**Confirmed:** `s3_zoneevents.asm` at `$59DB6..$59DD2` selects
`msZoneEvent 14,255,Map26_ZoneEvent0` plus a default return. The zone wrapper
compares low bytes of d1/d2, with `$FF` as wildcard: X14, any Y. This does not
mean tile `(14,255)`, every current tile in column14, or direct conversation
with the young man.

In esc02_controlCharacter, current position seeds a movement candidate;
direction, area bounds, slope and entity obstruction precede layout-event
classification. A candidate block masked by `$3C00` must equal `$1400` to
write zone event6. Its word parameters derive from `(XDEST+d4)/384` and
`(YDEST+d5)/384`, using the selected step deltas. These destination-derived
coordinates differ from current-position reads and EVENT_RELATIVE_POSITION.
The write precedes remaining terrain/debug/collision work and movement
assignment; structural admission does not establish a completed step or reach.

WaitForEvent prioritizes an existing nonzero event over A/C input and skips
controller installation when one is pending. ProcessMapEvent clears the sampled
type before dispatch. Type6 installs eas_Init on physical0 without waiting for
completion, loads d1 from PARAM1 and d2 from PARAM3, then invokes the zone wrapper.

| State when the selected callback checks it | Conditional result |
| --- | --- |
| X14, F780 clear | Execute cs_59F20; only its return permits setFlg780; return |
| X14, F780 set | Return without a new scene or hide |
| No specific row match | Default callback returns |

There is no F779, membership, ring-ownership or resident-position test. Supplied
clear F780 can admit incoherent/missing actors; supplied set F780 can suppress
the attempt without a prior completed scene. The check does not atomically
reserve the scene or establish a historical event.

### The Complete Attempt: View, Words, Movement and Hide

`cs_59F20` at `$59F20..$59F42`, scripts.asm lines4–13, contains eight top-level
commands and the complete inline body shown below. The script trap activates
VInt_UpdateEntities before ExecuteMapScript.

| Index | Complete command / body |
| --- | --- |
| 0 | textCursor1740 |
| 1 | setCamDest0,20 |
| 2 | nextSingleText modifier0, actor129: ID1740 on the unskipped path |
| 3 | csWait50 |
| 4 | nextSingleText modifier0, actor129: ID1741 on the unskipped path |
| 5 | entityActionsWait129: moveUp8; endActions |
| 6 | hide129 |
| 7 | csc_end |

textCursor sets CUTSCENE_DIALOG_INDEX. setCamDest writes view target -1,
passes0/20 to SetCameraDestination and waits for scrolling. That consumer
multiplies by384; SetViewDestination applies area parallax/layer/background
offsets, writes destinations only on axes not under autoscroll, and calls
scroll-data helpers. This requests a view; it neither teleports the player
nor defines129's origin. Actual area state, completed scrolling and framing
remain premises, not observed presentation.

The text operands encode `$0081`, not the `$FFFF` no-speaker sentinel.
Modifier0 requests neither mirroring nor right-side placement. Portrait/speech
resolve through the actual129 mapping; baseline young man opens no new portrait
and selects bleep2. csc1D can preserve an already open portrait while csc00
still resolves speech. Each unskipped line waits scrolling, displays the cursor's
text, increments it, closes portrait/text and sleeps10.

ID1740/key06CC is `Ungh!{W2}`. ID1741/key06CD says the object is too heavy,
he cannot lift it and gives up, ending W1. Both W2 and W1 poll directional/A/B/C
input with VInt waits. W2 also updates its prompt marker and requests
SFX_VALIDATION on acceptance; W1 adds no such request. csWait50 is a sleep,
not a physics/action-completion condition or measured duration.

ExecuteMapScript clears SKIP_CUTSCENE_TEXT at entry. Debug mode plus P2 Start
can latch it during dispatch: skipped csc00 consumes operands without display
or cursor increment, and script sleeps are bypassed. Camera, movement, hide
and caller flag work still execute. A skip between the two text commands can
change the later cursor/value; neither line is unconditionally seen.

csc2D resolves129, seeds its timer with the physical index, clears FLAGS_A
bits5/6 and compiles moveUp8 into esc04 relative `(0,-8)`, esc01's destination
wait and an appended esc34 jump to eas_Idle. The `$FF` wait byte polls only
that actor's idle action pointer. It supplies no HP guard, timeout or all-actor
barrier. esc04 multiplies the signed delta by384 and adds it to actual current
position when executed, not the population row or previous destination.
Bit5 clear bypasses the selected destination-conflict branch. Destination,
travel and signed velocities use inherited speed bytes; this scene sets none.
Baseline completed eas_Init supplies32/32 if unchanged, but the zone's Init
of physical0 is not Init of129. Only unchanged current `(5,24)` yields requested
`(5,16)`; neither that endpoint nor movement completion is unconditional.

esc01 compares current position with the actor's current destination, without
reserving an immutable target. Replaced pointers/destinations, stalled updates
or an already parked raw X≥`$7000` can alter or prevent progress. Completion
under coherent uninterrupted state does not prove timing or collision physics.
The scene implements no ring mass, lift, attachment, force test or inventory
receipt. Its failed-lifting portrayal is **Inferred** from words/context.

### Hiding, Later Parking and Control Are Different Boundaries

hide129 resolves again and calls HideEntity: current/destination X/Y become
raw `$7000,$7000`; **all** matching ENTITY_INDEX_LIST entries become `$FF`;
a matching follower-list entry is removed by shifting EXPLORATION_ENTITIES.
It grants no item, recruits nobody and requests no warp. Changed aliases can
redirect hiding or invalidate several logical identities. Subsequent access
to an invalidated mapping is not a safe missing-actor no-op.

csc_end conditionally waits scrolling if a dialogue window remains, clears
VIEW_SCROLLING_SPEED and restores caller registers. Only its return permits
F780. The zone wrapper then closes windows, waits one VInt and waits for
**physical0** current position to equal its current destination. This is
separate from129's action wait and does not join every actor. The exploration
loop's SetMoveSfx/WaitForEvent boundary permits target0 and the current-mode
controller only without a pending event; pending work wins. Callback return
alone does not establish usable control.

The arrival owner's F780 init tail instead uses MoveEntityOutOfMap → resolver
→ SetEntityPosition, writing only current/destination X/Y and preserving
lookup/follower lists. A later explicit fresh Map26 load rebuilds population
before this position-only parking. Same-visit HideEntity invalidation is a
different state; MAP_CURRENT retains supplied mappings, including invalidated
ones. No safe hide-to-parking continuity is established. Later init execution
is outside this guide, with its behavior retained by the arrival owner.

Failure before return can leave camera/text/action/hide effects with F780 clear;
a failure after hiding but before setFlg780 can admit another attempt with
invalidated lookups. Conversely, supplied set F780 can skip hiding a present
resident. No general rollback or repair is present. These are static state
and order distinctions, not observed interruptions or save/load results.

## Empty Local Lists Do Not Establish Universal Absence of Rewards

**Confirmed:** `s4_descriptions.asm` is only return; its wrapper starts d7=0,
so it contributes no handled description. `s5_itemevents.asm` has only a default
return; its wrapper starts d6=0, so the local callback contributes no handled
item use, mutation or request. The wrapper still clears speech, stores the
item caller's d2/Y in EVENT_RELATIVE_POSITION, closes windows and waits for
physical0. This is not the resident-facing meaning of that shared byte.

Map26 chest and other-item lists each contain only endWord. GetItem sees the
negative first byte and returns -1 for missing coordinate/flag/item results.
These lists supply no local item record; CheckArea has other layout-dependent
paths, and field/menu/item services have separate owners. No universal absence
of search responses, held items, collectibles or service rewards follows.
The narrative ring is not an item granted by this scene.

## Three Exit Requests Have Separate Destinations

**Confirmed:** `6-warp-events.asm` at `$A8CD2..$A8CEC` contains exactly
three eight-byte records and endWord. Each uses warpNoScroll/type0. UP1 is
destination facing, not an approach restriction; all triggers are exact.

| Record | Trigger X/Y | Requested map | Destination X/Y/facing |
| --- | --- | --- | --- |
| 0 | `(59,3)` | WILLARD_BURROW27 | `(2,7)`, Up1 |
| 1 | `(32,11)` | CURRENT255 | `(12,41)`, Up1 |
| 2 | `(12,42)` | CURRENT255 | `(32,12)`, Up1 |

In ordinary esc02 walking, candidate layout classification `$1000` calls
WarpIfSetAtPoint. It divides supplied signed raw X/Y by384, selects CURRENT_MAP's
warp table and stops at the first match or word -1. A match writes event type1,
copies type/map/X/Y into PARAM1–4 and facing/padding into PARAM5–6, and requests
WARP_SFX=SFX_WARP. Standing at a listed tile does not prove this consumer ran;
layout, candidate/action and state remain premises. Other vehicle paths share
the matcher without establishing their movement legality here.

### A Shared Pending Request Is Consumed Before Input

The request storage is mutable, not an immutable queued warp. WaitForEvent
prioritizes its nonzero type; ProcessMapEvent clears the type before dispatch
and the consumer reads the parameters then present. Overwritten or supplied
requests can change the result.

All three PARAM1 values select ProcessMapEventType1_Warp's no-scroll arm.
It requests stored sound, clears WARP_SFX, idles physical0, removes its caller
return address to leave ExplorationLoop, and zeroes d0–d4. It loads the map,
then UpdatePlayerPosFromMapEvent samples actual current player X/Y/facing.
Nonnegative destination bytes replace **only low bytes** of X/Y; facing is
always loaded and d4=0. The signed divisions have already filled coordinate
words. Thus exact table-equal outbound tuples require coherent nonnegative
local coordinates: supplied raw X=-384 gives quotient `$FFFF`, then destination2
produces d1.w=`$FF02`, not `$0002`. Y has the same limitation. This arm returns
to MainLoop without executing destination init; it is distinct from the
nonzero-type scroll-transition arm.

### Local Routes Retain State; Map27 Admission Is Conditional

MainLoop calls SwitchMap, then CheckBattle before selecting exploration/battle.
No flag-switched row substitutes26/27/255. CheckBattle resolves MAP_CURRENT
through CURRENT_MAP for lookup but retains the original255 request on a
no-battle return. With CURRENT_MAP26, no battle-coordinate row matches; both
local records admit exploration with d0=255, not an explicit Map26 reload.

At that exploration-entry boundary MAP_CURRENT retains CURRENT_MAP,
population/lookups and working layout. It skips explicit-load tilesets/entity
allocation, temporary-flag clearing and F80 set, using UpdateMainEntityProperties
and DeclareRaftEntity before common entry work and later init dispatch. Actors
are not reset to population rows. Under coherent coordinate premises, main
current/destination/facing receive the destination values. UpdateMainEntityProperties
tests each coordinate's low byte for negativity, then multiplies the **whole
word** by384 and stores the low product word; it does not normalize abnormal
retained high bytes. MAP_CURRENT is therefore neither a fresh population nor
a position-only shortcut. It can retain same-visit invalidated mappings.

Map27 has the Battle23 row with wildcard triggers. Clear battle-unlocked F423
skips it and admits Map27 exploration; set F423 selects the battle path.
CheckBattle's completed-F523 test can clear423 without canceling that current
selection. These are admission facts only: no resident, ring or selected warp
callback sets423. Explicit-map exploration selects the new map/population,
sets CURRENT_BATTLE255 and clears temporary flags256–383 before later init;
F779/F780 are outside that range. Battle admission has its own separate owner.

No selected warp, ordinary matcher/consumer, SwitchMap row or battle predicate
tests F780. Identical other inputs therefore give the same structural exit
admission with F780 clear or set. Natural geometry/reach and later destination
behavior remain separate. These exits consume neither ring nor balloon and
write no EGRESS_MAP, PLAYER_TYPE, saved raft state or new story/recruitment flag.
This guide stops before destination population/init, tunnel scripts and Battle23
hooks, roster, activation or outcomes. The accepted continuation,
[Willard Burrow exploration and Battle23 admission](willard-burrow-through-battle23-entry.md),
explains the new population, local traversal and separately admitted confrontation
through the BattleLoop call. It retains actor/alias, coordinate and pending-request
limits and stops before battle initialization or destination Floor World/Creed init.

## Evidence Review and Retained Limits

**Confirmed provenance:** accepted main `b9405e3d2649af6aee3ace4397ecbfa6faa9121e`
contains corrected #924/PR927 and #926/PR929. Research pins
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm)
and USA ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Source paths above are relative to disasm; local setup files are under
data/maps/entries/map26/mapsetups. The accepted
[research reproduction owner](../../research/floor-world-interactions-and-exits.md#reproduction-and-retained-limits)
keeps the direct source joins and unchanged payload comparison recipe. This
synthesis adds no extraction, runtime observation or remake decision.

Adversarial review reads the complete corrected research, arrival guide and
necessary [interaction-state](../../research/map-event-interaction-state.md),
[setup-selection](../../research/map-event-flag-route-selection.md),
[request-consumption](../../research/map-event-request-consumption.md),
[exploration](../contracts/map-exploration.md) and service boundaries. The selected
public fixture/schema/verifier owners are:

| Claim | Executable owner / selected boundary |
| --- | --- |
| Resident/zone/default records | `sf2-map-events-static-v1:routing-setup`, tests/fixtures/h2/map-events/routing-setup.json: all seven Map26 entity rows, two zone rows and default item row; schemas/h2/map-events-routing-setup.schema.json. |
| Complete returning callbacks | `sf2-map-events-static-v1:entity-programs`, entity-programs.json programOrder298–303; `:zone-programs`, zone-programs.json programOrder50/51; schemas/h2/map-events-entity-programs.schema.json, map-events-zone-programs.schema.json and shared map-events-target-program.schema.json. These are corpus identities, not gameplay order; src/sf2tool/h2/map_events.py and map_events_fixture.py own parsing and shard validation. |
| Facing/register and pending-event boundaries | `sf2-map-event-interaction-state-static-v1` and `sf2-map-event-request-consumption-static-v1` in tests/fixtures/h2; matching schemas/h2/*-static-fixture.schema.json and src/sf2tool/h2/map_event_interaction_state.py / map_event_request_consumption.py. The latter validates the retained request owner and exact consumption projection, not natural continuity. |
| Scene commands/consumers | `sf2-map-script-engine-static-v1`, tests/fixtures/h2/map-script-engine-static-v1.json: dialogue, camera, action bridge, lifecycle and script-control facts; schemas/h2/map-script-engine-{dialogue,map-control,entity,script-control}.schema.json via map_script_schema.py; map_script_engine.py `_verify_map_script_engine_fixture`. The retained corpus selects cs_59F20's eight commands and full inline body. |

Read the pinned complete resident/zone/scene/warp source and the selected
activation, wrapper, camera/action/hide, no-scroll and admission consumers at
the research's named seams. The scene table and callback operations agree with
the stricter static boundary. The corrected signed-quotient/low-byte replacement
limitation is retained rather than inferred away from ordinary table values.
Existing narrow reproduction commands are `uv run sf2 h2 map-events`,
`uv run sf2 h2 map-script-engine`, `uv run sf2 h2 map-event-interaction-state`
and `uv run sf2 h2 map-event-request-consumption`; their aggregate checks do not
independently guard every local prose or callee claim. The current review does
not regenerate payloads or replay those full extraction commands.

Preserve [#924 original handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/924#issuecomment-6176361906),
[corrected handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/924#issuecomment-6176448210),
[P2 review](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/927#issuecomment-6176402766)
and [acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/927#issuecomment-6176460244).
Those receipts retain separate original/corrected objects and
CI38622136093/38622845061, the unchanged 18,285,979-byte pair comparison/0.082s,
inspection failures, source-probe corrections and predecessor Unknowns.
[#926 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/926#issuecomment-6176550578)
and [acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/929#issuecomment-6176569261)
retain CI38623499458 and linked #920/#923/#919/#916 results/failures. These linked
records remain authoritative; this guide replaces no completed result.

Direct acceptance checks complete owned prose/diff, selected quantities/order,
links/fragments/tables/fences, provenance, English/private boundary and
`git diff --check`, then the configured clean committed
`uv run sf2 verify plan --base origin/main --head HEAD` and actual final-head CI.
No local blanket normal/full/engine/Godot/H3/H4 or translation suite, acquisition,
new fixture/tooling or payload regeneration is selected solely for this prose.

**Unknown:** natural arrival/activation/zone/exit reach; actual flags, layout,
lookups/aliases, HP/class/party, travel mode, request and current/destination state;
completed services, camera/text/input, motion, hiding and usable control; visible
ring performance, duration/audio and save/load persistence. Static changed-state
examples are not played scenarios. [ADR0014](../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../../decisions/0016-remake-start-evidence-deferral.md) retain these
limits without creating a runtime queue. This completes neither the campaign
nor the design epic and selects no desired gameplay/UI policy or remake scope.
