# Floor World First Arrival and Conditional Exploration

**Confirmed:** a surviving Cotton Balloon departure admits Map26 exploration and
rebuilds nine population rows. With F779 clear, the destination init runs the
complete landing scene, then sets F779 and writes EGRESS_MAP=26. F780 independently
controls parking resident 129. **Inferred:** the six actors portray the balloon
landing and popping. **Unknown:** natural arrival, actual supplied state, completed
actions/services, audiovisual presentation, usable control and persistence.

This English guide continues [Cotton Balloon acquisition and departure](cotton-balloon-through-floor-world-entry.md)
from its conditional loader admission. It consumes only accepted
[Floor World arrival research](../../research/floor-world-first-arrival.md) under
[Issue #926](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/926) and the
[current Standard agreement](../documentation-roadmap.md#current-baseline-and-near-term-synthesis).
It ends at conditional local controller installation or pending-event handling.
Resident conversations/services, the giant-ring scene, search, exits/tunnel,
Battle23 and recruitment are later boundaries. Fnnn means flag nnn; command and
population indices are zero-based. Coordinates, speeds and waits are source
operands, not measured movement, pixels or elapsed time.

## A New Population Replaces the Desktop Cast

**Confirmed:** the predecessor's surviving request is Map26 `(26,8)`, Down.
SwitchMap does not substitute another map, and CheckBattle finds no Map26 battle
row; Battle23 uses Map27. Admission begins exploration loading, without proving
natural reach or control. The departure wrote no EGRESS_MAP; the destination's
later write is a separate effect.

On the explicit-map branch, ExplorationLoop clears the pending event, adjusts
the step counter, heals eligible living/immortal allies, starts fade-out and
disables camera following. It writes CURRENT_MAP=26 and CURRENT_BATTLE=255,
loads tilesets, waits fading, clears entity interrupts, initializes the selected
population and loads sprites. It clears temporary flags256–383, sets F80,
initializes sprite-frame state and targets physical 0. LoadMap and base-interrupt
installation precede RunMapSetupInitFunction. This neither clears F779/F780 nor
establishes that every ally is alive.

Fresh loading replaces Desktop actors, lookups and motion; Desktop137–139 are
not carried into Floor World130–132. Map26 has one default setup, `ms_map26`,
and no flag-selected alternate population. Its six setup pointers select
entities, entity events, zones, area descriptions, section 5 and init; the current
map is resolved by each accessor. Changing F522/F779/F780 does not select a
different nine-row table.

InitializeMapEntities clears records/lookups, allocates selected followers from
physical 1, then consumes the nine rows in order. It declares Bowie at physical 0
at the requested position/facing with eas_Idle, writes shared SPRITE_SIZE=16,
terminates the follower list and performs the separate raft refresh.

The complete [population and mapping owner](../../research/floor-world-first-arrival.md#complete-population-and-logical-to-physical-mapping)
is `s1_entities.asm` at `$59D20..$59D6A`. Physical examples below assume only
F67/Peter and F68/Oddler, no skipped ally row, distinct mappings and no overwrite.
Init/Fixed mean eas_Init/eas_InitFixedSprite.

| Row | Logical actor | Physical example | Sprite / initial action | Tile, facing |
| --- | --- | --- | --- | --- |
| 0 | Randolf16 | 3 | Ally sprite lookup; Init | `(27,22)`, Right |
| 1 | Tyrin17 | 4 | Ally sprite lookup; Init | `(29,22)`, Left |
| 2 | NPC128 | 5 | OLD_MAN198; walk origin `(50,12)`, range3 | `(50,12)`, Down |
| 3 | NPC129 | 6 | YOUNG_MAN196; Init | `(5,24)`, Down |
| 4 | NPC130 | 7 | OBJECT1=173; Fixed | `(63,63)`, Up |
| 5 | NPC131 | 8 | OBJECT1=173; Fixed | `(63,63)`, Up |
| 6 | NPC132 | 9 | OBJECT1=173; Fixed | `(63,63)`, Up |
| 7 | NPC133 | 10 | PRIEST192; Init | `(4,35)`, Down |
| 8 | NPC134 | 11 | CARAVAN62; Init | `(11,35)`, Down |

Randolf and Tyrin are present without a joined-force or active-party admission
test. This is not recruitment. Their HP/class determines the actual sprite,
including BLUE_FLAME for a dead exploration ally. A preexisting nonzero ally
lookup skips that row without consuming a physical slot; the ordinary fresh
follower table itself declares neither of them. Seven ordinary sprite rows use
the NPC lookup cursor independently of the two ally rows. Logical129 therefore
means the young man, ordinarily physical 6, rather than Tyrin or a balloon object.
The old man's copied walking program does not prove his later observed position.

### Followers and Transport Remain Supplied Premises

Map26 is neither overworld, Tactical Base nor Nazca Ship, so F65 adds no
overworld Caravan/A prefix. With only F67/F68, Peter7 is physical 1 with
eas_Follower1 and B31 is physical 2 with eas_Follower2, initially at the requested
position/facing. B is a script identity: the Oddler example requires F68.
The accepted follower declaration order also permits Astral/B and other A/B/D
aliases. Later declarations replace a logical lookup while earlier physical
records remain; other followers shift resident slots.

Joined membership alone allocates neither Peter nor B here. A missing zero
lookup resolves to physical 0, so nominally different commands can address
Bowie; the scene creates no missing follower and revives/recruits nobody.
Six logical subjects mean six distinct physical actors only under coherent,
distinct mappings. Actual HP, class, followers and lookups must remain premises.

The fresh-load raft helper can replace main's sprite in mode 2, park followers
at raw `$6000,$6000` and declare physical 31 as RAFT at `(64,64)` without a
saved-map test. Otherwise F64 plus RAFT_MAP=26 admits physical 31 at saved X/Y
masked by `$7F`. Lookup offset `$3F` and the physical record can be overwritten.
The ordinary two-follower cast occupies 0–11, separate from 31; Desktop's slot31
overlap is not imported. Neither loading nor landing proves ordinary walking
mode, repairs aliases, or clears saved raft state.

## Reading the Landing as Complete Phases

**Confirmed:** init `ms_map26_InitFunction` at `$59DDC` tests F779 first. Clear
runs `cs_59E04` at `$59E04`; set bypasses it. This grouping accounts for all
41 top-level commands in source lines 28–98. The
[exact command and nested-body table](../../research/floor-world-first-arrival.md#init-dispatcher-and-all-41-landing-commands)
remains the operand authority. The embedded map-init fixture also records
71 statements, including eighteen custom-body and twelve inline-body statements;
those are not 71 top-level scene commands.

Ordered-six below means Bowie, Peter, B,130,131,132. Every custom body ends
ac_end; every inline movement body ends endActions.

| Indices | Ordered work |
| --- | --- |
| 0–2 | Select text 1739; neutral camera; reload current map view at `(21,8)`. |
| 3–5 | Install/wait eas_Init separately for Bowie, Peter, B. |
| 6–8 | Stop animation counting on Bowie, Peter, B. |
| 9–14 | Six separate customActscriptWait bodies in ordered-six order: speed 10/10; jump eas_Idle; end. |
| 15–17 | Place Bowie `(26,7)`, Peter `(27,6)`, B `(25,6)`, each Down. |
| 18–20 | Place 130 `(26,6)`,131 `(25,5)`,132 `(27,5)`, each Up. |
| 21 | Call fadeInB. |
| 22–26 | Install independent `moveDown 7; endActions` on Bowie/Peter/B/130/131. |
| 27 | Install the same movement on 132; wait only for 132's idle pointer. |
| 28–32 | Sleep 20; restart trio animation in Bowie/Peter/B order; sleep 40. |
| 33–36 | Hide 130, then131, then132; sleep 30. |
| 37 | nextSingleText `$FF,255`, using text 1739. |
| 38–40 | Install Peter-follow-Bowie offset 2, then B-follow-Peter offset 2; end scene. |

### Preparing the View and the Actors

Trap6 activates entity updates before ExecuteMapScript. The neutral camera
writes negative target `$FF` without resolving an actor. reloadMap disables
following, deactivates interrupts and calls LoadMap with map index -1 and packed
view operands21/8 multiplied by 3. It reuses the current map's area properties
and working layout, bypassing LoadMapBlocksAndLayout; display/interrupts are
enabled and one VInt is waited afterward. It does not rebuild actors, clear
temporary flags, restore initial positions or call init recursively. `(21,8)`
is view input, not another spawn or warp.

The three waited Init commands establish zero acceleration factors,
speed 32/32, acceleration/deceleration off, obstructable on, entity/map
uncollidable off, autofacing on, Up orientation, immersion off and shared
SPRITE_SIZE=24, then update the sprite and jump idle. They do not reset position,
HP, travel/velocity or turn animation on. The three object rows begin with Fixed,
which additionally disables autofacing/animation; the scene does not give them
waited Init. Completion of their original Init is not separately joined before
custom speed actions replace it. Shared sprite size is not persistent world scale.

Each of the six custom bodies changes X/Y speed to10/10 and jumps idle,
without restoring other Init properties. Waited action installations seed the
timer with physical index and poll that actor's action pointer until eas_Idle.
They have no HP/missing-actor guard, timeout or all-actor barrier. Their progress
requires actual entity updates; a later pointer installation can replace work.

stopEntity writes animation counter-1; startEntity later writes0. These change
animation counting, not velocity, destination or action completion. setPos writes
current and destination coordinates to absolute tile×384, writes facing and
updates the sprite, without resetting action, velocity or speed. Placement and
animation commands skip true allies 0–29 at HP0, including Bowie/Peter; B31 and
NPC130–132 bypass that guard. Action installation remains unguarded. Thus dead
allies are not uniformly skipped, and requested starts are not guaranteed.

fadeInB calls FadeInFromBlack, whose service waits fading-state clearance and
another VInt. Only its return permits the movement commands. This is a source
service condition, not an observed display or measured duration.

### Six Downward Movements Have One Selected Wait

csc2D clears target FLAGS_A bits 5/6 and compiles each moveDown 7 into relative
`(0,+7)` from the actor's actual current position, a wait for its current
destination, then an idle jump. Speed bytes determine signed travel velocities.
The bit5-clear destination-scan arm bypasses the selected conflict branch;
this does not prove a general collision/physics rule or destination-occupancy gate.
Installations occur sequentially and permit concurrent progress, without a
simultaneous start, actor attachment or common balloon physics object.

If every placement took effect and remains current when its movement executes,
requested destinations are Bowie `(26,14)`, Peter `(27,13)`, B `(25,13)`,130
`(26,13)`,131 `(25,12)`,132 `(27,12)`. These are conditional destinations,
not guaranteed final coordinates. A movement wait compares that actor's current
position against its current destination, not a reserved immutable endpoint.

Only132's idle-pointer wait joins movement at the map-script level. Even an
unchanged 132 reaching its destination proves nothing about the other five.
Sleeps20/40/30 add no completion check. Animation restart, hiding and following
can therefore precede established completion of other actors. Changed pointers,
destinations or aliases and stalled updates can change/prevent progress.
Entity updates skip raw X at least `$7000`; `(63,63)` is below that cutoff,
but an already hidden supplied actor can fail to execute its new action.
Sprite-load capacity can also defer action progress.

### Removing the Objects and Naming the Pop

hide 130/131/132 calls HideEntity independently, without an HP guard or other
actor wait. It writes current/destination raw `$7000,$7000`, rewrites **all
matching** ENTITY_INDEX_LIST entries to `$FF`, and removes a matching entry
from EXPLORATION_ENTITIES by shifting the list. This invalidates lookups and
following-list membership; it does not recruit, free inventory, change sprite
or request a pop sound. Missing zero mappings can hide main; aliases can
invalidate several logical identities. Later access to invalidated lookups
is not a safe missing-actor no-op or normal population access.

Text 1739, key06CB, literally reads `The Cotton Balloon popped.{W1}`.
The nextSingleText operands encode word `$FFFF`: the handler bypasses portrait
and speaker-sprite selection and sets CURRENT_SPEECH_SFX=0. This is a sentinel,
not NPC255 speaking or a Peter/Oddler portrait/bleep request. A previously open
window does not establish a newly selected portrait.

Ordinarily the handler waits view scrolling, displays text, increments the
cursor, closes portrait/text and sleeps10. W1 waits on directional/A/B/C input.
Debug mode with P2 Start can latch SKIP_CUTSCENE_TEXT, bypassing presentation,
cursor increment and csWait sleeps while action/flag commands still execute.
These are bounded source facts, not observed UX. The literal names a pop;
there is no explicit playSound, explosion or physics command. The portrayal
is **Inferred**; visible/audio performance and duration remain **Unknown**.

### Following Is Installed, Not Observed Complete

followEntity first reads a **byte** before the word follower operand. Peter's
`$0007` and B's `$001F` both have high byte0, so both admission checks test
**Bowie's HP**, rather than Peter's or B's. If admitted, the full words resolve
the actual follower/target mappings. Offset 2 selects signed `(-24,0)`.

AddFollower copies 42 bytes of eas_Follower1 into walking-parameter storage,
patches target/offset words, installs the pointer and appends the physical
follower only if absent. Peter follows Bowie; B then follows Peter. Executing
the copies configures speed 32/32 and follower collision/acceleration settings;
the loop uses target current/destination/facing and layout to request travel,
wait for destination and repeat. This is not a teleport, absolute formation,
healing, recruitment, story/follower-flag write or completed following observation.
It can replace unfinished motion; altered mappings can form a different or
self-referential chain. No final waited Init resets the trio. Bowie's controller
is installed separately after the caller returns.

## Returning, Repeating and Supplying a Different State

**Confirmed:** csc_end conditionally waits view scrolling if a dialogue window
exists, clears VIEW_SCROLLING_SPEED and restores caller registers. It adds no
all-actor wait. Only scene return allows init to set F779, then write
EGRESS_MAP=26, then check F780 independently. RunMapSetupInitFunction restores
its saved registers without the item/zone wrappers' extra main-destination
wait or window closure.

| Flags when each check is reached | Conditional init result |
| --- | --- |
| F779 clear, F780 clear | Scene return → F779 set → EGRESS_MAP26 → return;129 remains. |
| F779 clear, F780 set | Same scene/flag/EGRESS sequence → park129 → return. |
| F779 set, F780 clear | Skip scene and both writes; preserve existing EGRESS_MAP. |
| F779 set, F780 set | Skip scene/EGRESS write; park129; return. |

F780 does not suppress landing or get set by this tail. MoveEntityOutOfMap at
`$477EA` resolves logical129 via the NPC129-minus96 lookup and writes only
current/destination X/Y to raw `$7000`. It preserves lookup, follower list,
action, sprite and animation. This **parks** the young man, ordinarily physical 6;
it is not HideEntity's lookup/list removal. Changed aliases can park another
record. The helpers are not interchangeable.

A fresh repeat load still rebuilds all nine rows, including objects at `(63,63)`.
F779 alone skips their scene placement/hiding and final follow copies; the fresh
follower declarations still apply. F780 separately parks129. Supplying F779
proves neither earlier completed landing nor EGRESS_MAP26, and the skipped
branch does not repair an old EGRESS value. Flags are read when reached,
without reserving an entry snapshot.

Supplied map index 255 instead retains current population/lookups/layout. It
updates main properties and calls DeclareRaftEntity, then joins sprite-frame,
LoadMap and init work, without fresh allocation, tilesets, temporary-flag clearing
or F80 set. Nonnegative supplied X/Y update main current/destination and its
facing/sprite. Mode2 returns from this raft helper immediately; mode 1 skips
follower repositioning; other modes reposition listed followers. F64 then
controls a separate raft declaration: matching saved map declares physical 31
and lookup `$3F`; differing map clears that lookup and parks31. This differs
from the fresh-load raft refresh. Neither the scene's current-layout reload
nor its return invokes this alternate exploration-entry branch.

An interrupted prefix can leave placement, hiding or following while F779
remains clear. After F779 is set but before the EGRESS write, an interruption
can leave an old EGRESS value that a repeat will not repair. After EGRESS but
before the F780 tail,129 can remain present. There is no general rollback.
These are static changed-state distinctions, not observed interruption,
save or persistence outcomes.

## Scene Return Is a Conditional Control Handoff

**Confirmed:** after callback return, ExplorationLoop compares longwords
PALETTE_1_BASE_02 and PALETTE_1_CURRENT_02. Only inequality calls PlayMapMusic
then FadeInFromBlack. The earlier scene fade establishes neither this comparison
result nor actual music/screen state. SetMoveSfx and WaitForEvent follow.

With MAP_EVENT_TYPE zero at WaitForEvent entry, physical 0 receives the current
mode's controller: mode 0 eas_ControlledCharacter; mode 1 the source-named eas_Raft
(Caravan control); other nonzero modes eas_Caravan (raft control). The source
names do not swap their consumer meaning or validate arbitrary supplied modes.
The walking action configures speed 32 and its control loop when executed;
scene return alone has not restored every field or admitted usable input.

A nonzero pending event skips controller installation and wins over A/C input.
ProcessMapEvent clears the sampled event type before dispatch; its current
parameters decide subsequent work. Landing itself writes no warp/battle request,
PLAYER_TYPE or saved raft state. Services, event state, mode, aliases and actual
controller execution therefore remain premises for usable exploration.

This guide stops here. The accepted research identifies resident/service callbacks
in s2_entityevents and the independent F780-gated `cs_59F20` giant-ring zone in
s3_zoneevents as next boundaries. It does not explain those interactions,
searches, exits/tunnel, Battle23 or later recruitment, and consumes no
[#924](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/924) conclusion.

## Evidence Review and Retained Limits

**Confirmed provenance:** accepted main `6e8803feebaf95931246e6e435c305a8916a9aac`
contains #920/PR922 and #923/PR925. Research pins
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm)
and USA ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Source paths above are relative to disasm, with map26 setup files under
data/maps/entries/map26/mapsetups. This synthesis creates no extraction,
payload, runtime observation or remake decision.

Adversarial review consumes the whole accepted arrival owner and its predecessor,
the [exploration control contract](../contracts/exploration-control-flow.md),
and the selected existing boundaries below. The
[standalone program-data contract](../contracts/standalone-map-script-program-data.md)
owns data/provenance rather than executing or rendering this init-embedded scene.

| Existing fixture and reproduction route | Selected boundary and limit |
| --- | --- |
| [sf2-map-init-static-v1](../../../tests/fixtures/h2/map-init-static-v1.json), `uv run sf2 h2 map-init` | routeJoins sourceOrder 63, pointerTableJoins sourceOrder 66 (`$59D08` → `$59DDC`), scriptCallSites siteOrder 44 and directCallSites siteOrder 40 bind setup/init/scene/park calls. The complete ten-operation init and embedded 71-statement scene match the accepted 41-command grouping. Source-order identities are not gameplay chronology. |
| [sf2-map-script-engine-static-v1](../../../tests/fixtures/h2/map-script-engine-static-v1.json), `uv run sf2 h2 map-script-engine` | Selected lifecycle/reload, action, placement, animation/hide, text and follow handler facts/guards bind local seams. They do not join every callee's completion or prove physical/presentation outcomes. |
| [sf2-map-event-request-consumption-static-v1](../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json), `uv run sf2 h2 map-event-request-consumption` | explorationLoop reset, waitForEvent polls and processMapEvent pre-dispatch clear; controller execution and usable control require their actual consumers. |

The existing schema/verifier routes are
[init schema](../../../schemas/h2-map-init-static-fixture.schema.json)/[verifier](../../../src/sf2tool/h2/map_init.py),
[script schema](../../../schemas/h2/map-script-engine-fixture.schema.json)/[verifier](../../../src/sf2tool/h2/map_script_engine.py),
and [request schema](../../../schemas/h2/map-event-request-consumption-static-fixture.schema.json)/[verifier](../../../src/sf2tool/h2/map_event_request_consumption.py).
Init/script fixture projections live under expected; request consumers are
top-level eventRequestConsumption.consumerContexts. The flag-route-selection
fixture has no ms_map26_InitFunction program-route context: the explicit init
joins own this route. Aggregate guards do not independently verify every prose
claim; population/callee details remain the accepted research's direct joins.

Preserve [#920's original handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/920#issuecomment-6175992255),
[rebased handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/920#issuecomment-6176088792)
and [PR922 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/922#issuecomment-6176117381):
original head `bd40b052b04ab087738f5fc170e406f97a244ad6`, tree
`fc16905271421b3eda529e8bd30960516712cf2d`, base `63fab597ee78136cd88f7cdefa2bc6c4512cf6b5`;
rebased head `7a83e84530bf01bd41c3e071886a24b086a5783e`, tree
`cb3b2d356b73e72290bfc66fd0359c0974b6e42a`, base `5e874fc507c0d740fb7a64536a69aa9a2c0d1d18`.
[CI38619448174](https://github.com/FrankHZ/md-sf2-reverse-engineering/actions/runs/38619448174)
and [CI38620203806](https://github.com/FrankHZ/md-sf2-reverse-engineering/actions/runs/38620203806)
passed scope/research-public; engine/adapter/H4 were scoped skips. The unchanged
18,285,979-byte pair comparison passed in 0.079s. Retained output-truncation,
literal-wildcard, mixed-path ValueError, guessed source/macro paths and rejected
pre-launch write diagnostics remain at those receipts with their corrections.
No completed result is replaced or rerun here.

[#923's handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/923#issuecomment-6176246113)
and [PR925 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/925#issuecomment-6176272977)
retain corrected per-service ownership and the address conflict: stale
`$4F4AA/$4F566` versus accepted `$4F48A/$4F542`, with original failed assertion,
authorized correction and [CI38621272226](https://github.com/FrankHZ/md-sf2-reverse-engineering/actions/runs/38621272226).
GetItemInventoryLocation `$81D0` → `$9146` and RemoveItemBySlot `$819C` → `$8E76`
belong to itemstats.asm; ReceiveMandatoryItem `$4F48A` and RemoveItemFromInventory
`$4F542` belong to iteminventory.asm. The alternate items/itemfunctions_s7_0.asm
remains excluded. The predecessor's
[per-service ownership boundary](../../research/map-event-item-transactions.md#ownership-and-unknown-boundary)
and caller/callee distinction remain intact.
[#919 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/919#issuecomment-6175962879)/[PR921 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/921#issuecomment-6176035685)
and its [#916 evidence receipts](cotton-balloon-through-floor-world-entry.md#evidence-review-and-remaining-limits)
retain all earlier objects, comparisons, completed failures and Unknowns.

Direct acceptance covers complete owned prose/diff, quantities and command
coverage, wait/HP/lookup distinctions, provenance, relative links/fragments,
tables/fences, English/private boundary and git diff --check. Inspect the
configured clean committed `uv run sf2 verify plan --base origin/main --head HEAD`
and actual final-head CI. The research's
[reproduction owner](../../research/floor-world-first-arrival.md#reproduction-and-retained-limits)
retains inputs/comparison commands; this prose slice selects no local blanket
normal/full/engine/Godot/H3/H4 or translation suite, acquisition or regeneration.

**Unknown:** natural arrival; actual force/HP/class, flags, followers/lookups,
mode/raft, layout and positions; completed action, sprite, scroll, text/input,
fade/music/control services; motion/duration, audiovisual presentation, usable
exploration, saving and persistence. Changed-state examples are source
counterexamples, not played scenarios or an automatic runtime queue under
[ADR0014](../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../../decisions/0016-remake-start-evidence-deferral.md).
This guide neither completes the campaign nor expands remake acceptance.
