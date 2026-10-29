# Bedoe's Hatchling Flight and Rescue

A hatchling in Bedoe offers to demonstrate flying, struggles with two attempts and ends by claiming
qualified success. A separate scene stages a fall, a birdwoman's alarm and her thanks to Bowie.
These are optional interactions with independent callers. Rescue does not require the demonstration
or the king's acceptance, and the birdwoman's report of safety establishes no later recruitment.

**Confirmed static content and control flow:** the accepted
[research owner](../../../../research/chapters/eastern-parmecia/bedoe-hatchling-flight-and-rescue.md) supports the selected dialogue,
actions and state boundaries below. **Inferred:** their staging depicts attempted flight and rescue.
**Unknown:** natural interaction/zone reach, supplied actors, completed services, visible flight,
fall/contact and playable continuity. This guide adds no remake capability or product decision.
Fnnn denotes flag nnn; coordinates and cardinal move counts are map tiles unless specified.

The [Volcanon return and king-choice guide](volcanon-return-through-bedoe-king-choice.md) explains
the surrounding Bedoe setup. Its choice is separate from these events. Stop here at the two selected
caller returns and their conditional control handoffs; Bedoe exit, Polca, Battle13 and future Skreech
recruitment remain outside this guide.

## Finding the Two Optional Interactions

**Confirmed selection:** F722 alone selects Map10's alternate entities, entity events, zone events
and init. Setup lookups resolve current flags again; setting F722 does not itself rebuild actors or
invoke either interaction. The default F722-clear setup has neither lookup 153's fixed row nor this
rescue-zone caller. F723/F724/F799 cannot substitute for F722 selection.

| Selected caller | Admission and response |
| --- | --- |
| Address lookup 153, F723 clear | Run flight program `cs_5766C`; only after script return set F723. |
| Address lookup 153, F723 set | Request closing text 1685 again; no flight, repositioning or new flag write. |
| Zone `(6,23)`, F13 set | Return without the rescue program or F724 write. |
| Zone `(6,23)`, F13 clear and F724 set | Return without the rescue program or a new flag write. |
| Zone `(6,23)`, F13 and F724 clear | Run rescue program `cs_5779C`; only after script return set F724. |

F13 is the joined-Elric flag, used here only as a suppression discriminator. The rescue caller reads
neither F723 nor F799. Conversely, F724 does not suppress flight. This permits either order at the
predicate boundary, without proving that both are naturally reachable in every save or that their
shared actor state is identical. Supplied completion flags are not receipts of prior scenes.

Ordinary C activation idles/waits main and scrolling, then addresses a tile using the view target and
facing, with counter-tile extension and follower exclusion. The activated physical actor must actually
reverse-map to 153. The event row's `DOWN=3` is a wrapper bitfield: it requests facing toward the player
and later restoration of the saved physical actor's pre-interaction facing. It is not a Down-facing
prerequisite. The wrapper disables entity updates; the scene's script trap enables them again.

The zone instead requires a produced pending type 6 request with coordinates `(6,23)`. Its movement
producer needs layout attribute `$1400`; the row alone proves no accessible floor or natural crossing.
Pending consumption clears the type before dispatch. The zone wrapper installs `eas_Init` on physical 0
and matches X/Y, without the entity wrapper's facing protocol. Suppressed branches still pass through
its window/VInt/main-stop services.

## Who Is Present Depends on the Supplied Map State

**Confirmed alternate declarations:** regular NPC numbering advances separately from ally rows.
The last two regular rows supply lookup 152, BIRDWOMAN at `(6,10)` Right, and lookup 153, CHICK at
`(16,6)` Left, both with `eas_Init`. These are lookup identities, not physical slots 152/153.
Bowie/main is separately physical 0. Text 1688/1689 uses `{NAME;20}`, whose ally constant is Skreech;
that attribution does not map NPC 153 to ally 20 or join him to the force.

Peter lookup 7 has no fixed row here. F67 supplies him as a follower during a fresh rebuild, before
fixed entities. F67-clear leaves his cleared mapping zero, which an unguarded request can resolve to
physical 0. F75 can likewise supply Luke before his fixed row, changing allocation while regular NPC
numbering remains unchanged. Other supplied follower flags can also change physical slots.

Follower declaration tests neither force/active-party membership, word HP nor AI. Neither hatchling
scene joins anyone, changes membership or AI, heals/revives, or sets F67. Entry healing of living or
immortal allies establishes no later HP, mapping or follower state. The
[party/roster contract](../../../contracts/party/party-roster-state.md) keeps those operations distinct.

Lookup resolution masks the selector to a byte, translates NPC 128+ by subtracting `$60`, then reads
and scales the physical index by 32. Zero maps to physical 0; `$FF` resolves index 255 without a general
presence guard. These scenes create or repair no mappings. Actor consequences described below require
the named valid, distinct mappings and the relevant completed services.

Speech properties come from the currently mapped sprite. Normal CHICK selects portrait NONE/bleep 1;
BIRDWOMAN selects NONE/bleep 6. Changed supplied sprites can change these inputs. Attributed dialogue
and service selection are supported; their rendered appearance and sound remain **Unknown**.

## The Demonstration: Two Attempts and a Repeat Claim

**Confirmed ordered content:** cursor 1680 supplies six texts, all by lookup 153. The first two use
continuing `nextText`; the remaining four use `nextSingleText`. Research retains the complete
48-record program and inline bodies, so this explanation groups the experience rather than repeating
its command ledger.

The chick describes a dream of flying (1680), then announces a demonstration (1681). Camera tracking
selects 153. After facing Right, a sleep 40, nod and waited rightward jump, the sprite becomes
FLYING_CHICK. Auto-facing is disabled; speed pairs change to `(4,4)` for the Up 1 request, then `(48,48)`
for Down 6 with falling sound. The sprite returns to CHICK and the chick expresses puzzlement (1682).
A waited Init and Left 2/faceUp 1 sequence follows; Bowie and Peter are requested to face Down. The
chick acknowledges failure (1683) and asks them to stay for another attempt (1684).

The second attempt again faces Right, sleeps 40 and nods. Camera speed becomes `$40`; Right 1 and a
rightward jump precede FLYING_CHICK, auto-facing OFF and Down 1. Speed 4 accompanies Up 1, then speed 64
accompanies Down 8 and falling sound. A nonwaited counterclockwise action is installed; `startEntity 153`
and sleep 12 follow. Camera speed returns to `$20`, and a waited InitSlow replaces that action before
the waited Up 2/Right 1/Up 11 sequence. The chick is requested Left and CHICK, Bowie/Peter Right, then
claims qualified success (1685). A later F723-set interaction requests that same claim only.
There are six distinct flight texts and seven request sites including repeat.

**Confirmed state limit:** the counterclockwise action loops through relative destinations
`(-1,+1)`, `(-1,-1)`, `(+1,-1)` and `(+1,+1)`, waiting each destination. The later InitSlow interrupts
its pointer rather than waiting for the loop to complete, and does not reset current/destination
coordinates. Flight has no `setPos`, so adding nominal moves to the fresh `(16,6)` declaration cannot
establish an unconditional final coordinate.

After completed waited actions, flight leaves an idle action, CHICK and InitSlow speed `(16,16)`;
it never restores Init's `(32,32)` speed at its end. The script's Left request is subsequently replaced
by the interaction wrapper's saved physical chick-facing restore. Fresh Left happens to agree;
a different supplied pre-interaction facing is restored instead. Lookup 152 and priority entries are
unchanged. Camera tracking remains 153 until the later conditional exploration handoff.

## The Independent Fall and Rescue

**Confirmed ordered content:** cursor 1686 supplies four single-text requests: two by 153 and two
by 152. The complete 78-record program is retained by research. It changes no mapsprite, health,
membership or AI, rebuilds no entities and contains no internal completion-flag write.

### Bouncing and Alarm

The scene puts 153 at `(6,10)` Down, faces 152 Up and selects camera destination `(1,5)`. It waits
Peter's Init, requests Bowie `(5,22)` Down and Peter `(11,22)` Up, then installs Peter's nonwaited
wait 60/Left 4 action and faces Bowie Up. Lookup 152 is only faced here: her fresh `(6,10)` declaration
is not imposed on a supplied current actor.

The chick's `eas_46198` enables acceleration/deceleration factors without setting speed. Fresh Init
state can therefore begin with speed 32; completed flight supplies speed 16/InitSlow instead. The first
two Up 2/faceUp 5/Down 2 sequences, each with jump sound, end in laughter 1686 and 1687 and sleep 40.
The third uses Down 1, followed by Init and a waited leftward jump. That Init normalizes speed to 32.
The chick faces Up, the birdwoman Left; camera tracking selects 153 at speed `$30`. The birdwoman
calls the hatchling's name (1688), using `{NAME;20}`. It is attributed alarm, not a recruitment event.

### Falling, Chasing and the Report of Safety

The scene writes Bowie priority 0 and 153 priority 1, disables 153 auto-facing, sets speed 48 and requests
falling sound before installing nonwaited Down 13. Peter is faced Left, the priorities are written
again and sleep 35 follows. Lookup 152 gets speed 64 and a nonwaited Left 1/Down 11 chase. The explicit
`waitIdle 153` waits for the chick alone, not her arrival or contact. Quake 3, Bowie Up, a waited custom
animation/orientation action, sleep 40 and quake 0 follow.

The chick then receives Init, Down and four waited jumps, each followed by sleep 20, including after
the fourth. The chick faces Up and camera tracking becomes neutral. Two waited 152 jumps precede her
thanks to the leader and statement that the named hatchling is all right (1689). She nods, sleep 30
follows, then both 152 and 153 receive waited Init. **Inferred:** this staging depicts rescue.
**Unknown:** actual flight/fall/contact, visibility and timing. Her reported safety is not an HP write.

Peter has no initial idle barrier before his Left-facing request; his wait 60/Left 4 can still be pending.
His later Left 1 action replaces the installed pointer rather than queuing behind it. The first waited 152
jump similarly replaces her chase without first waiting for it. Sleeps and other actors' waits are
not proofs that these nonwaited movements arrived.

### Departure and Local State Afterward

Lookup 152 receives nonwaited Right 8 while 153 is waited through Up 1/Right 8. The scene hides 152, then
explicitly puts 153 `(16,6)` Left. Her departure has no dedicated completion barrier before hide;
the chick's waited exit does not supply one. Peter receives waited Left 1, sleep 30 and a waited
BumpLeft. Bowie receives Init, Down and a headshake, then priority 1. The final command attempts
Peter following Bowie with selector 2. Script return precedes the caller's F724 write.

With valid mappings and completed services, 153's final current/destination position is `(16,6)` Left,
with idle/Init speed 32. Its mapsprite is the supplied one, normally CHICK after a fresh declaration or
completed flight, and its priority entry remains 1. Rescue has no sprite-normalization command.
Bowie retains `(5,22)` only if his HP-guarded position request ran and no other movement intervened;
his final Down-facing request also has its HP condition. Peter's final follow request fixes no current
coordinate and supplies no wait for settled following.

## Reading Waits, Animation and Following Correctly

**Confirmed consumer distinctions:** the [research consumer section](../../../../research/chapters/eastern-parmecia/bedoe-hatchling-flight-and-rescue.md#action-animation-priority-hp-and-hide-consumers)
and [common-scripting owner](../../../../research/world/common-scripting.md) preserve the following limits:

- Installing an action immediately replaces its pointer. Waited forms poll that actor's pointer for
  `eas_Idle`; they are not queues, global barriers or guarantees of unobstructed movement. Cardinal
  deltas multiply by 384 internal units per tile and use current coordinates. Speed 4/16/32/48/64 are
  paired X/Y operands, not tile counts or durations; `faceUp 5` is facing/sprite/wait work, not Up 5.
  Jump/Bump actions issue timed relative destinations, so their names alone promise no displacement.
- Init sets speed 32, zero acceleration factors, acceleration/deceleration OFF, obstruction ON,
  entity/map uncollidable OFF, auto-facing ON, orientation Up, immersed OFF and size 24, then idle.
  InitSlow sets speed 16, factors `(1,1)` and the source `(-2,ON)` acceleration/deceleration pairs,
  with the same later obstruction/auto-facing/orientation/size setup. Neither resets coordinates.
- `startEntity 153` writes animation counter 0; it does not start/resume an action. FLYING_CHICK and
  CHICK are mapsprite selectors, not combat movement-type changes. Bowie's `motion OFF` writes counter
  `$FF`, not a movement freeze; orientDown changes orientation separately from his Up facing. Final
  Init restores orientation 0 but has no animation-on command; completed headshake ends at counter 0.
- Priority writes map zero to 0 and any nonzero word to 1 in the physical actor's priority array. The
  ordinary sprite-link consumer places nonzero entries before zero entries; this differs from the
  sprite tile's `VDPTILE_PRIORITY`. Rescue sets 153 before Bowie in that scheme, then sets Bowie 1;
  it saves/restores no prior entry and leaves 153 at 1. Actual overlap and VDP visibility remain Unknown.
- `setPos`/`setFacing` test the byte-selected ally's **word HP**: zero skips their operand bytes.
  NPC 152/153 bypass that check. Actions, sprites, priority, gestures and hide do not inherit it;
  zero Bowie/Peter HP therefore does not suppress the whole scene or imply healing.
- `followEntity Peter,Bowie,2` encodes `$0007,$0000,$0002`, but its admission helper probes the first
  word's **high byte**, hence Bowie 0's HP rather than Peter 7's. Nonzero resolves both mappings and
  installs/appends following with selector 2 offsets `(-24,0)` pixels. Even an already-listed follower
  gets its pointer replaced. The follow consumer shifts offsets by 4 into internal coordinates and
  applies its own distance/direction/layout rules. This writes neither F67 nor membership and proves
  no immediate position, settled following or future persistence.

Camera target selection does not wait scrolling; camera destination selection uses neutral tracking
and waits view-scroll completion. Map-script sleeps use low-byte service counts and may be bypassed
by debug text skip; Peter's wait 60 is an entity-action wait, a separate consumer. None is a wall-clock
measurement. Text/portrait/close, sound, sprite/DMA, VInt and input services retain completion limits.
The terminator waits scrolling only when a dialogue window remains, then clears scrolling speed;
it writes neither the caller flag nor control. The
[dialogue contract](../../../contracts/interface/dialogue-system.md) distinguishes cursor/display/close services from
observed natural text and responsive input.

## Hiding and Re-entry Do Not Produce the Same State

**Confirmed lifecycle boundary:** `hide 152` writes current/destination coordinates to internal
`$7000,$7000`, invalidates every matching mapping to `$FF` and removes a matching follower-list entry.
It leaves the action pointer intact, so hide does not prove the nonwaited departure reached its target.
Later alternate init independently reads F799 to move Luke 10 out of map and F724 to move 152 out of map.
It reads neither F723 nor F13. Its `MoveEntityOutOfMap` changes coordinates only, retaining mappings,
following and action pointers; F799 controls Luke alone here.

| Supplied order or entry boundary | Consequence and limit |
| --- | --- |
| Rescue before flight, F723 clear | F13/F724 alone admit rescue; F799 is irrelevant to it. Completed rescue sets F724. A later valid 153 interaction can still run flight from current position/sprite/priority 1. Flight does not reset that priority. |
| Flight before rescue | F723 does not block rescue. Completed flight supplies CHICK/InitSlow speed 16; rescue retains that speed initially, then its later Init sets 32. Different state does not prove different observed duration. |
| F724 supplied before alternate init | Init coordinate-removes 152 without invalidating a valid mapping. F13-set suppression alone does not remove her. |
| F724 changes after init, 152 still present | Suppressed zone return does not rerun removal or repair the actor. A partial scene can instead hide 152 while F724 remains clear. |
| Current-map entry (`MAP_CURRENT=255`) | Main/raft/follower placement and map/init services run without rebuilding fixed actors/mappings. They do not reset 153 to its table speed/sprite/priority/position. Init cannot repair an already `$FF` 152 mapping; it resolves that invalid index as supplied, so valid cleanup is not guaranteed. |
| Fresh explicit Map10 entry | Population/mappings rebuild before selected init. Alternate 153 starts CHICK `(16,6)` Left/Init, with regular priority 0; 152 starts BIRDWOMAN `(6,10)` Right/Init, then F724 may coordinate-remove her while preserving the new valid mapping. F67 independently supplies Peter. |

Fresh clearing resets the priority array; follower initialization sets main priority 1 and each
follower's declared priority, while regular 152/153 retain 0. That fresh state differs from rescue's
retained 153 priority 1. F723 does not replay/reposition 153 at init. Default F722-clear init is a
separate guard-state owner and cannot be imported into this alternate population.

## Returning from Each Caller to Conditional Control

**Confirmed separate wrappers:** after flight script return the entity caller writes F723. Its wrapper
restores the saved physical chick's pre-interaction facing, closes portrait/text and enables entity
updates, including on repeat. It adds no post-caller main-movement wait. After rescue script return
the zone caller writes F724; its wrapper closes windows, waits one VInt and waits physical 0's
current/destination equality, including when the scene was suppressed.

After either ordinary route returns to exploration, `WaitForEvent` retargets camera 0 and installs
physical 0's controlled action only when pending map-event type is zero. `PLAYER_TYPE=0` selects
`eas_ControlledCharacter`; other types select their own action. Completed caller/window/VInt/movement
services and zero pending are needed for this conditional handoff. Neither a completion flag nor a
script terminator proves responsive player input. Stop at these two boundaries.

## Evidence Review and Remaining Limits

This guide consumes [PR #798's accepted research](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/798)
and [PR #797's preceding English guide](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/797).
Original provenance remains pinned SF2DISASM `master` commit
`c834c652b6862bc5679fd7f69a38a7093206efc6` and the USA retail identity in the
[research agreement](../../../../research/chapters/eastern-parmecia/bedoe-hatchling-flight-and-rescue.md#agreement-provenance-and-resources).
The review used bounded tracked selections; no optional derived payload was copied/generated, source
investigation repeated or runtime acquired. The selected callers and init agree with the accepted
research. Handler fixtures establish source structure; they are not a composed hatchling observation.

| Reviewed surface | Trace and disposition |
| --- | --- |
| Ten distinct texts, two full scenes and inline bodies | [Flight](../../../../research/chapters/eastern-parmecia/bedoe-hatchling-flight-and-rescue.md#complete-flight-demonstration), [rescue](../../../../research/chapters/eastern-parmecia/bedoe-hatchling-flight-and-rescue.md#complete-fall-and-rescue-scene) and [reproduction selection](../../../../research/chapters/eastern-parmecia/bedoe-hatchling-flight-and-rescue.md#reproduction-and-review-boundary) retain 48/78 records and keys `0690..0699`. This guide paraphrases all 1680–1689 plus repeat 1685, without promoting attributed safety or rendered motion. Full command-ledger comparison remains retained research evidence, not a new execution here. |
| Independent caller predicates | Fixture ID `sf2-map-events-static-v1`: [entity shard](../../../../../tests/fixtures/h2/map-events/entity-programs.json) program 30 at `0x56F68` and [zone shard](../../../../../tests/fixtures/h2/map-events/zone-programs.json) program 5 at `0x56FC8`. Reviewed all seven operations each, branch targets/polarity and one table reference each. They confirm post-return F723/F724 writes, repeat 1685 and no F723/F799 rescue test. |
| Alternate init | Fixture ID `sf2-map-init-static-v1`, [fixture](../../../../../tests/fixtures/h2/map-init-static-v1.json), `expected.primarySourceBodies[15]` at `0x5708E`. All nine operations preserve independent F799/Luke and F724/152 removal, with no F13/F723 read. Coordinate-only effect is supported by the research consumer, not the call name. |
| Units, action/placement, animation, priority and follow | Fixture ID `sf2-map-script-engine-static-v1`, [fixture](../../../../../tests/fixtures/h2/map-script-engine-static-v1.json), selected `entityPlacementCommandFacts`, `entityActionBridgeCommandFacts`, `entityLifecyclePresentationCommandFacts` and `entityGestureRelationshipMotionCommandFacts`; [research consumers](../../../../research/chapters/eastern-parmecia/bedoe-hatchling-flight-and-rescue.md#action-animation-priority-hp-and-hide-consumers) supply interpretation. Word/high-byte HP, tile/speed/pixel units, animation versus motion and link versus tile priority remain separate. |
| Activation, zone production and conditional control | [Caller admission](../../../../research/chapters/eastern-parmecia/bedoe-hatchling-flight-and-rescue.md#callers-and-admission-boundaries), [caller return/re-entry](../../../../research/chapters/eastern-parmecia/bedoe-hatchling-flight-and-rescue.md#caller-return-re-entry-and-independent-ordering), and the [map/exploration contract](../../../contracts/world/map-exploration.md). No facing prerequisite, natural crossing, global actor barrier, repaired invalid mapping or unconditional control is inferred. |

The [map-event schema](../../../../../schemas/h2/map-events-target-program.schema.json) and
[loader](../../../../../src/sf2tool/h2/map_events_fixture.py) retain ordered operations, targets and shard
composition. The [init schema](../../../../../schemas/h2-map-init-static-fixture.schema.json) and
[verifier](../../../../../src/sf2tool/h2/map_init.py) retain body/provenance equality. The
[map-script verifier](../../../../../src/sf2tool/h2/map_script_engine.py) and
[schema composition](../../../../../src/sf2tool/h2/map_script_schema.py) keep distinct command families and
consumers. Focused owners are `tests/python/test_map_events.py`, `test_map_init.py` and
`test_map_script_engine.py`; narrow reproduction routes are `uv run sf2 h2 map-events`, `map-init`
and `map-script-engine`. These are evidence navigation, not freshly executed gate claims.

**Unknown:** natural interaction/zone access and floors/collision; actual flags, mappings, actors,
positions, HP, party/AI/follower/player state; interrupted/nonwaited movement, service/input/VInt timing
and completion; rendered flight/fall/contact, orientation/overlap/hide, dialogue/camera/audio; pending
requests, responsive input, later persistence and playable continuity. Neither scene establishes
future Skreech joining. These gaps admit no automatic runtime work, and this optional guide completes
neither the campaign nor the design epic.
