# Polca Raft Boarding and South Parmecia Entry

Boarding is a separate player action after Luke's recruitment scene. An eligible movement attempt
can request it, but the consumer must still find the physical raft immediately ahead of Bowie's
destination. Accepted boarding changes actors and presentation in stages, then commits raft mode.
From that mode, the selected southern exits from Map72 both request South Parmecia, **Map69**,
while taking different routes to exploration or battle admission.

**Confirmed static content and control flow:** the accepted
[research owner](../../research/polca-raft-boarding-through-south-parmecia-entry.md) supports the
request, actor, controller and selected-entry consequences below. **Inferred:** these operations
implement boarding and water travel. **Unknown:** natural contact and traversal, actual supplied
state, completed services, visible boarding/arrival, responsive input and persistence. Fnnn means
flag nnn; coordinates are map tiles unless internal units are explicitly named.

Read [Polca departure and Luke recruitment](polca-departure-through-luke-recruitment.md) for the
preceding conditional Map72 control boundary, and [the raft grant](battle13-outcomes-through-polca-raft-grant.md)
for the configured Map72 `(43,48)` tuple. This guide ends at a numeric type5 disembark request,
an admitted battle before initialization, or conditional Map69 exploration control. It adds no
remake capability, continuous-playability claim or downstream story/combat explanation.

Continue with [the Kraken reveal and Battle16 entry](south-parmecia-through-kraken-battle16-entry.md)
for the selected Map69 row0→Map49 route, conditional admission, complete encounter and fresh
combatant rebuild through the empty start wrapper. That separate guide preserves this segment's
natural-travel, supplied-state and completion limits; it adds no Battle14/15 prerequisite.

## Requesting Boarding Is Not Yet Boarding

**Confirmed producer:** ordinary character movement considers directional input against the
current bounds and checks actor obstruction before reading the proposed point's working layout.
With masked layout `$3C00`, no current battle (`$FF`) and F64 set, it writes the pending type3
boarding request. That branch can change facing but does not advance the destination/travel or
decrement the step counter. An obstructing actor can prevent reaching it. The scan uses bit5 of
the controlled actor and bit7 of potential blockers, considering both their positions and destinations.
The raft's standing action clears its own obstructable bit when that action runs.

This producer checks neither the saved raft tuple nor physical raft coordinates, Luke membership,
HP, F716, F718, F10, F75 or F416. The grant's F64 and narrative placement therefore do not prove
contact. Luke's waited Down1 followed by hiding is Luke's action; it does not board Bowie or issue
type3. Door, warp and zone markers retain their separate producer branches.

**Confirmed consumer gate:** `ProcessMapEvent` clears pending before dispatching type3 to
`MapEventType3`. Its alignment helper reads physical main0's destination and facing, adds one tile
(384 internal units) in that direction, and compares the result with **physical31's destination**.
Only an exact facing-adjacent destination match admits the boarding body. Current-position overlap,
a raft-looking sprite or a logical follower mapping is insufficient. The helper does not recheck
F64, current battle or `RAFT_MAP`; a supplied aligned state can pass without those producer gates.
A rejected request stays consumed, with player mode unchanged.

Pending is a mutable slot, not a queue or atomic transaction. Which write survives an intervening
service remains Unknown. The [request and alignment owner](../../research/polca-raft-boarding-through-south-parmecia-entry.md#movement-request-and-the-separate-alignment-gate)
retains the exact producer and destination-offset consumers.

## What Changes Before Raft Mode Commits

**Confirmed admitted order:** idle physical0, clear the sprite-link ordering byte `byte_FFAFB0`,
run the four-record `cs_453F2`, install the following action on the current exploration list,
wait through the list helper, then write `PLAYER_TYPE=2`. Installing idle does not clear earlier
travel/velocity fields. Clearing the ordering byte is not an HP, membership or mode change.

| Stage | Requested action and completion boundary |
| --- | --- |
| Bowie approaches | The first waited Bowie action requests movement to physical31 at relative distance0, waits6, requests warp SFX, waits for its own position to equal destination, clones physical31's position/destination/travel/facing, then becomes idle. The script caller separately waits for the mapped Bowie's action pointer to become `eas_Idle`. |
| Separate raft moves off-map | `setPos FOLLOWER_D,64,64,DOWN` resolves logical159 through lookup63 and sets that mapped actor's position and destination to internal `$6000,$6000`, facing Down, with sprite refresh. This is a position command, not removal. |
| Bowie takes raft presentation | The second waited Bowie action sets the raft mapsprite, disables resizing, requests sprite refresh and becomes idle. Its caller again waits for that mapped actor's idle pointer. |
| Scene returns | The fourth record terminates, including the conditional view-scroll wait and scroll-speed clear. This returns to the boarding consumer before the list action and mode write. |
| Listed actors converge and relocate | `byte_45434` requests movement toward physical0, then wait6/warp SFX/own destination wait/clone physical31/idle. Under normal mappings physical31 is now off-map; actors keep their own sprites while copying its state. |
| Raft mode commits | The list wait returns under its actual predicate, then `PLAYERTYPE_RAFT=2` is written. Earlier sprite and position effects have already been requested. |

The two waited Bowie commands resolve lookup0. Neither wait has a per-actor HP guard. The
FOLLOWER_D position command takes the non-ally branch of its alive-status helper, so zero
Bowie/Peter/Luke HP does not suppress that relocation. None of these commands heals, joins,
clears follower flags, invokes `HideEntity` or changes maps. The separate action scripts and
[complete boarding owner](../../research/polca-raft-boarding-through-south-parmecia-entry.md#complete-boarding-and-actor-identity)
retain requested services and completion limits.

Three identities must remain distinct:

| Identity | Meaning at this boundary |
| --- | --- |
| Physical31 (`ENTITY_RAFT`) | Direct target of alignment, movement-to-entity and clone-position actions. These actions use physical indices without logical lookup. |
| Logical159 (`FOLLOWER_D`) → lookup63 | Target of the off-map position command; normal raft declaration maps it to physical31. Missing or aliased mapping can instead affect main0 or another actor, while direct clones still target physical31. |
| Logical31 (`FOLLOWER_B`) → lookup31 | Oddler or another flag-selected follower from the earlier population. Its logical number matching physical31 does not make it the raft. |

**Confirmed list semantics:** despite the helper name `ApplyActscriptToHeroAndFollowers`, fresh
population declares main0 separately and appends admitted followers from physical1 to
`EXPLORATION_ENTITIES`, then terminates it. Main0 is not appended. `FOLLOWERS_LIST` starts one
byte later; it is not another rebuilt authority. The action installer walks the actual physical
bytes in the current list, rather than reconstructing actors from membership or flags. Fresh
normal state therefore acts on followers; supplied or retained lists can differ.

`WaitForHeroAndFollowersStopped` returns immediately for an empty list. Otherwise, after scanning
to the terminator, it polls **only the last listed physical actor's** pointer for `eas_Idle` after
VInts. It does not join every follower's completion. An earlier listed actor may still be busy
when mode2 is written. These two Bowie waits and the last-actor list wait are separate predicates.
Malformed/changing lists, absent actors and stalled services are not repaired here.

**Confirmed partial-state consequence:** interruption can leave Bowie with raft presentation or
followers relocated while `PLAYER_TYPE` is still0. Boarding leaves `RAFT_MAP/X/Y` supplied and
performs no saved-tuple update. These ordered effects do not establish a save history or transport
persistence; reaching the final mode write does not prove all-follower or presentation completion.

## Raft Control Uses the Actual Numeric Dispatch

**Confirmed selector:** when pending is zero, `WaitForEvent` retargets physical0 and installs the
player-type-selected action. The pinned source names disagree with the raft-mode meaning:

| Player type | Installed source script | Actual controller |
| --- | --- | --- |
| 0 | `eas_ControlledCharacter` | `ac_controlCharacter`, opcode2 |
| 1 (`PLAYERTYPE_CARAVAN`) | `eas_Raft` | `ac_controlRaft`, opcode7 |
| 2 (`PLAYERTYPE_RAFT`), or another nonzero/non1 supplied value | `eas_Caravan` | `ac_controlCaravan`, **opcode8** |

For boarding's committed mode2, the consumer is `esc08_controlCaravan`. Do not replace it with
opcode7 because a label says “Raft.” Likewise **numeric type5**, symbolically
`MAP_EVENT_GETOUTOFCARAVAN`, dispatches to `MapEventType5`, the raft-exit consumer. The differently
named `MAP_EVENT_GETOUTOFRAFT` is type4. This guide stops at the type5 request and does not consume it.

**Confirmed input and obstruction boundary:** the installed mode2 script sets speed32 and its
acceleration/deceleration parameters, then loops block-copy checks, opcode8 and destination wait.
Opcode8 considers Up, Down, Left and Right in that order against unshrunk layer1 bounds. Eligible
axes can combine; later checks determine final facing. With no eligible direction it advances
without requesting movement. The script's FLAGS_A bits7/6/5 do not disable the bit5 collision
branch despite the “Uncollidable” macro names. Its scan considers other bit7 actors' **current
positions only**, using both axis differences below `$100`; ordinary movement also considers
their destinations. Obstruction skips movement.

After that scan, the working layout determines the next action:

| Test, in order | Consequence |
| --- | --- |
| Masked `$0400` | Request door opening, then reread/mask the layout word. |
| Masked `$1000` | Call `WarpIfSetAtPoint`, then take the movement tail even if no row matched. |
| Masked `$3C00` | Take the movement tail. |
| Otherwise, raw word unsigned below `$C000` | Write numeric type5 and skip movement: the disembark-request stopping alternative. |
| Otherwise, raw word at least `$C000` | Skip movement without that request. |

The movement tail writes selected velocity, sets 384 travel per moving axis and advances the
destination by selected tile offsets before sprite update. It has no new F64/current-battle/unlock
gate, type6 zone producer or step-counter decrement. Block-copy markers `$0800/$0C00` can affect
the working layout when not fading. Neither these tests nor dialogue supplies a decoded water
path, collision-free travel or an input receipt. The
[controller owner](../../research/polca-raft-boarding-through-south-parmecia-entry.md#installed-raft-controller-and-stopping-alternatives)
retains the script and layout order without generalizing to Caravan operation.

## Two Southern Exits, One Correct Destination

**Confirmed selected content:** Map72's ordered warp table has the following southern and
competing edge rows. Row numbers are zero-based. Each source symbol must be resolved before
selecting the destination's numeric setup, init or battle owner.

| Row | Match `(X,Y)` | Source destination → number | Selected request |
| --- | --- | --- | --- |
| 6 | `(40,62)` | `MAP_OVERWORLD_SOUTH_PARMECIA` → **69** | No-scroll to `(41,16)`, Down. |
| 7 | `(0,255)` | `MAP_OVERWORLD_NEW_GRANSEAL_KINGDOM` → 73 | Scroll Left; wins at `(0,63)` before row9. |
| 8 | `(63,255)` | `MAP_OVERWORLD_PACALON_KINGDOM_DROUGHT` → 71 | Scroll Right; wins at `(63,63)` before row9. |
| 9 | `(255,63)` | `MAP_OVERWORLD_SOUTH_PARMECIA` → **69** | Scroll Down, tuple `(255,1)`, Down. |

South Parmecia69 is distinct from `MAP_OVERWORLD_SOUTHEAST_PARMECIA=70`. Negative coordinate bytes
are wildcards; first match wins. Rows0–5 do not match the selected points, and rows6/9 do not overlap.
At other X on Y63, row9 can match if opcode8 reaches the `$1000` layout call. Warp matching converts
proposed internal coordinates by signed division by384, then writes type1, its tuple and warp SFX;
it does not require raft mode or story flags.

Map72 and Map69 declare layer1 bounds `(0,0)..(63,63)`. Opcode8 uses those unshrunk bounds, making
the selected point and southern edge compatible with that producer subject to input, occupancy and
layout. Ordinary character control has a one-tile shrink in its `d7==0` branch; `d7` is the remaining
entity-scan counter, not a fixed physical0 identity. This compatibility proves no natural approach
to the selected rows. The [exit owner](../../research/polca-raft-boarding-through-south-parmecia-entry.md#selected-map72-exits-and-producer-compatibility)
retains the symbol-to-number join and precedence.

## Why the Exits Have Different Arrival Consequences

**Confirmed type1 consumption:** pending is cleared before either branch. Both preserve
`PLAYER_TYPE` and the saved raft tuple; service completion and intervening parameter writes
remain Unknown. The [entry lifecycle owner](../../research/polca-raft-boarding-through-south-parmecia-entry.md#no-scroll-admission-versus-scrolling-continuation)
separates the calls below.

**Row6, no-scroll:** the consumer requests/clears warp SFX, idles main, unwinds exploration and
returns Map69 `(41,16)`, Down to the original `MainLoop/@Start`. `SwitchMap` has no Map69
replacement. `CheckBattle` then scans these actual Map69 rows in order:

| Battle row | Area output `(X,Y,width,height)` | Trigger | Unlock / completion |
| --- | --- | --- | --- |
| 9 | `(0,0,48,32)` | `(255,255)` | F409 / F509 |
| 17 | `(0,31,30,32)` | `(255,255)` | F417 / F517 |

Map, unlock and trigger admit a row. The wildcard triggers admit either location; rectangles are
outputs, not containment tests, so `(41,16)` excludes neither. F409 wins when both unlocks are set.
A set completion flag clears the selected unlock **but still admits that selected battle on this
call**, without falling through to the next row. With both unlocks clear, ordinary exploration
follows. Luke's F416 unlocks Battle16 on Map49; it alone proves no immediate Map69 admission.
Supplied completion/unlock flags are not reconstructed from a presumed story history. Stop at
admission before battle initialization or combat; a later battle return has its separate entry.

**Row9, scroll:** the consumer idles main, sets `CURRENT_MAP=69` and calls the Down map transition.
The special Map71/F530 replacement does not apply. The service loads tiles, blocks and working
layout with conditional copies, then scrolls/redraws with DMA/VInt waits. On return, negative X
preserves main's current X, while Y position/destination becomes384 (tile1). The tuple's Down
facing is **not consumed**; current actor facing is retained. The handler samples main's current
position/facing, calls `DeclareRaftEntity`, and returns inside the same exploration loop.

This selected call performs no `SwitchMap/CheckBattle`, fresh actor setup, temporary-flag clear,
entry heal, Map69 init or fresh map-music/fade-in lifecycle. F409/F417 do not interrupt this scroll
call. In mode2, `DeclareRaftEntity` returns immediately, leaving current mappings, list and hidden
actors intact. Applying row6's battle scan or fresh population to row9 would invent a call.
Subsequent events remain outside this guide.

## Fresh Map69 Population and Conditional Control

**Confirmed fresh-entry boundary:** only row6 without an admitted battle reaches the fresh
`ExplorationLoop(69,41,16,Down)` here. It clears pending, adjusts step count, heals living/immortal
allies, fades out, sets Map69/not-in-battle, loads tiles and initializes actors/mapsprites. Setup
clears temporary F256–383 and sets F80 before map loading, base VInts and init. Map69 has a
default-only setup, an **empty fixed entity list** and `ms_map69_InitFunction` at `0x4FDE2`, a sole
`rts`. No fresh story scene or F416-dependent init follows from that body.

Empty fixed population does not mean no actors. Map69 is an overworld map outside follower
exclusions. Main0 and the admitted flag followers are rebuilt: F65 can prefix the caravan,
F67 admits Peter, F68 supplies Oddler under FOLLOWER_B31 subject to conflicting rows, and F75
can recreate Luke despite the preceding current hide. Membership alone admits none of those
followers. The [predecessor population boundary](../../research/polca-departure-through-luke-recruitment.md#fresh-population-raft-declaration-and-init-guard)
retains HP-dependent ally sprites, order and mapping limits. No fixed NPC repairs an absent follower.

**Confirmed preserved mode2:** the fresh raft consumer gives main0 the raft sprite and moves each
listed follower to `$6000,$6000` with a standing action. Main0 remains at `(41,16)` because it is
outside that fresh list. A separate physical31 is declared at `(64,64)`, Left, with raft sprite
and standing action, and lookup63 is set to31. This branch needs neither F64 nor matching
`RAFT_MAP`, and does not update the saved tuple.

Supplied non-raft mode takes another branch: ordinary separate-raft declaration requires F64 and
`CURRENT_MAP==RAFT_MAP`. A still-stored Map72 tuple does not declare it on Map69. Scroll's
no-rebuild `DeclareRaftEntity` is also distinct: outside mode2 it can relocate listed followers;
with F64 set and map mismatch it clears lookup63 and moves physical31 to `$7000,$7000`.
These alternatives explain supplied states without asserting that boarding naturally disembarks.

After fresh entry's no-op init, `PlayMapMusic` and `FadeInFromBlack` occur only if base/current
palettes differ. There is no unconditional second music request. At the next `WaitForEvent`,
pending zero retargets physical0 and installs the preserved mode2's opcode8 controller; nonzero
pending defers control setup to event processing. Scroll reaches that conditional setup with its
retained population, without the fresh-entry lifecycle. **Stop at conditional control.** Neither
path alone proves rendered arrival, responsive input or subsequent zone/story behavior. The
[population and control owner](../../research/polca-raft-boarding-through-south-parmecia-entry.md#ordinary-map69-population-and-conditional-control)
retains the fresh and retained actor distinction.

## Evidence Trace and Remaining Questions

**Confirmed provenance:** this synthesis consumes accepted PR818 research from main, not a new
source investigation. Its owner binds pinned SF2DISASM master
`c834c652b6862bc5679fd7f69a38a7093206efc6` and the registered USA retail ROM, names the exact
producer/action/controller/transition source bodies, and provides
[reproduction and coverage limits](../../research/polca-raft-boarding-through-south-parmecia-entry.md#reproduction-and-coverage-limits).
The [exploration control-flow contract](../contracts/exploration-control-flow.md) owns top-level
handoff order; the [map-entry routing contract](../contracts/map-entry-routing-state.md) owns
helper-local selection, not this content-specific route or downstream completion.

| Tracked fixture and exact ID | Selected relation and limit |
| --- | --- |
| [map-script-engine-static-v1.json](../../../tests/fixtures/h2/map-script-engine-static-v1.json), `sf2-map-script-engine-static-v1` | Established placement/action/clone handlers and corpus identity. Accepted research compared all four `cs_453F2` records in the retained corpus. Immediate physical-action/list/controller bodies are named source evidence, not composed fixture coverage. |
| [map-event-request-state-static-v1.json](../../../tests/fixtures/h2/map-event-request-state-static-v1.json), `sf2-map-event-request-state-static-v1` | Pending and raft-state writes; no guarantee that a particular request survives or that saved state persists. |
| [map-event-request-consumption-static-v1.json](../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json), `sf2-map-event-request-consumption-static-v1` | Pending clear/dispatch and raft consumer topology; does not compose boarding, travel and arrival. |
| [map-setup-static-v1.json](../../../tests/fixtures/h2/map-setup-static-v1.json), `sf2-map-setup-static-v1`; [map-entities-static-v1.json](../../../tests/fixtures/h2/map-entities-static-v1.json), `sf2-map-entities-static-v1` | Aggregate setup/population boundaries. Actual Map69's default/empty fixed list is selected by the corrected research's resolved destination; dynamic followers remain separate. |
| [map-init-static-v1.json](../../../tests/fixtures/h2/map-init-static-v1.json), `sf2-map-init-static-v1` | `expected.primarySourceBodies` sourceOrder76, Map69 init path, owner/address and sole `rts`; no boarding or input-completion assertion. |
| [battle-routing-data-static-v1.json](../../../tests/fixtures/h2/battle-routing-data-static-v1.json), `sf2-battle-routing-data-static-v1` | Aggregate routing evidence; actual Map69 row9/17 operands and caller admission are named direct research evidence, not per-case fixture coverage. |

Pre-synthesis review inspected the accepted research, predecessor boundary, helper contracts,
these bounded tracked fixture selections and their schema/verifier surfaces. The map-script
fixture preserves handler/corpus facts; the map-init fixture retains the exact selected body;
request fixtures preserve structural relations. Their verifiers compare their owning projection,
not a natural journey. Aggregate PASS must not be described as per-case boarding/arrival coverage.
Existing owning commands are `uv run sf2 h2 map-script-engine`, `map-event-request-state`,
`map-event-request-consumption`, `map-setup`, `map-entities`, `map-init` and `battle-routing-data`
under that same H2 prefix. This prose slice does not rerun them or generate payloads. The research
recipe loads the current ignored input configuration in the launch process before any reproduction.

The [corrected accepted PR818 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/818#issuecomment-6159852108)
preserves the failed initial South Parmecia=70 assumption and successful earlier checks/CI that did
not validate the destination join. The corrected check resolves the warp symbol, then selects
that numeric map's setup/entity/area/init and battle owners, including corner competitors.
Corrected final checks/CI and earlier inspection, quoting/path/propagation and rebase failures remain
at that handoff and its linked records. No ritual replay or independently assumed map owner replaces
the corrected join.

**Unknown:** natural reach from Luke's exit to a boarding marker and either southern exit; actual
layout, flags, HP, actor mappings and list; surviving pending writes; action, VInt/DMA, sound,
sprite and transition completion; all followers finishing; visible boarding/arrival, input response
and saved transport persistence. Misalignment despite F64, aliased lookup63, empty/reordered lists,
an earlier follower still busy, interrupted pre-mode presentation, corner-row precedence,
completed-but-unlocked admission and the same F417 state taking different row6/row9 lifecycles
remain discriminating supplied cases, not observed natural histories.

These limits retain **Inferred** boarding/travel meaning and stay deferred under
[ADR0014](../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md) and
[ADR0016](../../decisions/0016-remake-start-evidence-deferral.md). They do not authorize acquisition,
full type5 consumption, later zones/Creed/Kraken story, combat or campaign/epic completion.
