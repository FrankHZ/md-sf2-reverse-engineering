# Bedoe to the Volcanon Audience

Peter returns to the shrine, introduces Bowie and appeals to Volcanon for help. The conversation
turns from welcome to anger over Grans and Zeon, then to advice about the jewel. Peter ends by
committing himself to Bowie's cause. The script separately requests his joining message, changes
membership and an AI bit, and attempts to have him follow Bowie.

**Confirmed static content and control flow:** the accepted
[research owner](../../research/bedoe-to-volcanon-audience.md) supports the selected requests,
conversation and service boundaries below. **Inferred:** this is a candidate content continuation
of [Bedoe's first audience](bedoe-king-address-through-first-audience.md). **Unknown:** natural
floor traversal, actual supplied state, service completion, presentation and usable input. The
king's invitation is dialogue, not the shrine's admission requirement. This guide adds no remake
capability or product decision; the speakers' claims remain attributed speech.

Stop after the selected caller and first conditional control on Map11. The
[Volcanon return through the Bedoe king choice](volcanon-return-through-bedoe-king-choice.md)
continues separately with selected return requests, the optional question and Luke's requested
departure. Natural floor travel, hatchling/Skreech, town/services/floor inventory, later travel and
Battle13 remain outside this guide. Fnnn denotes flag nnn; coordinates are map tiles unless specified.

## Entering the Shrine Is Separate from Hearing the Conversation

**Confirmed selected requests:** the Bedoe row at `(45,50)` requests explicit Map11, destination
`(8,34)`, Up, without scrolling. It tests neither the prior address/audience flags F720/F721 nor
F722, Peter's membership or F67. The necessary shrine rows distinguish entry from return:

| Request predicate on Map11 | No-scroll request |
| --- | --- |
| Any X, Y12 | Current map, `(39,18)`, Up: selected shrine entrance. |
| Any X, Y19 | Current map, `(8,13)`, Down: exit discriminator. |
| X8, Y34 | Explicit Map10, `(45,50)`, Down: return discriminator only. |

The movement producer requires layout attribute masked to `$1000`. The warp consumer converts
internal coordinates to tiles, scans rows in order, accepts negative coordinate bytes as wildcards,
and emits the first matching request. Arrival at `(8,34)` does not automatically execute its reverse
row; another movement/event request is needed. Nor does arrival at `(39,18)` establish a walk to
the conversation's `(39,11)` zone. These rows specify requests, not collision-free floor connections.

Pending-event consumption clears the event type before dispatch. The no-scroll path requests sound,
idles main and returns to the main loop with map, coordinates and facing. Map10/11 have no selected
flag-switched replacement or battle-map row: these requests select exploration, independently of
battle-unlock flags. The current-map sentinel survives the no-battle return; it differs from an
explicit request to load Map11.

Explicit entry initializes mappings and population after loading the selected setup. A current-map
transition instead updates main and raft/follower placement, then loads the map and runs init while
retaining fixed population and mappings. Exploration healing occurs on either loop entry, including
Peter's special immortal eligibility; it proves neither his mapping nor his HP at the later zone.
The scene itself performs no healing, revival, map reload or population rebuild.

## A Single Audience Gate, with Distinct Actor Identities

**Confirmed setup:** Map11 has one ordinary setup without an F722 override. Its init is only `rts`;
it neither starts the conversation nor repairs state. Fresh initialization installs flag-selected
followers before the regular declarations. Peter 7 has no fixed row: F67 supplies his follower
declaration independently of F7/F39, HP and AI. Bowie/main is separately physical 0.

The first regular declaration is lookup 128, a sequenced birdman at `(8,19)`, Down. Lookup 129 is a
walking birdman at `(6,28)`; 130/131 are birdwomen at `(37,14)` / `(41,14)`. NPC lookup numbers are
not physical slots: follower allocation affects the latter.

The dialogue addresses Volcanon and is attributed to him at scene level, but its twenty requests
use **speaker lookup 128**. That lookup maps to the first birdman declaration; there is no separately
declared Volcanon entity in this population. The script never changes 128's sprite or action.
Portrait/speech selection reads the current mapped sprite; BIRDMAN selects portrait NONE and
`DIALOG_BLEEP_6` in the source table. These conditional consumer facts do not establish the rendered
Volcanon, a visible speaker or the audible composed scene. Changed mappings/sprites alter the input.

**Confirmed admission:** the sole `(39,11)` row selects `Map11_ZoneEvent0`. Its only gate is F722:

| Supplied flags | Selected caller result |
| --- | --- |
| F722 clear; F720/F721 clear or set | Run `cs_57AAA`; successful return sets F722, then F711. |
| F722 clear; F711 already set | Still run the scene; the final F711 write is idempotent. |
| F722 set; F711 clear or set | Return without scene or writes. F711-clear is not repaired. |

F719, prior audiences, membership/active/follower flags, HP and AI are not gate predicates. Both prior
audiences may be skipped in supplied state without proving a natural route that does so. The default
zone caller simply returns. Ordinary zone production needs layout attribute masked to `$1400` and
supplies coordinate words derived from destination plus attempted movement offsets, divided by 384.
Dispatch compares their low bytes, with independent 255 wildcards and first-match/default semantics,
and resolves the current setup again. Matching coordinates alone do not prove a request occurred.

## Peter's Return and Bowie's Introduction

**Confirmed ordered content:** cursor 3151 supplies 37 conversation requests, 3151–3187:
seventeen by Peter 7 and twenty by lookup 128, attributed below to Volcanon. Bowie has no
conversation text. The paraphrases cover every request; research retains the full command ledger.

1. **Returning and greeting (3151–3155).** After waiting Peter's `eas_Init`, the script sends Bowie
   and Peter to `(39,11)` through separate waited destination commands. Bowie moves Left1;
   Peter faces Up and moves Up1, while Bowie moves Right1/faces Up, then Peter moves Up1 again.
   Peter greets Volcanon (3151). A camera request to `(34,3)` and sleep 120 precede Volcanon's
   welcome (3152), Peter's account of his trip (3153), and Volcanon's approval and question
   (3154–3155).
2. **Friends on the ground (3156–3159).** Peter nods and a sleep 30 is requested. He describes
   ground dwellers and his friendship with them (3156–3157); Volcanon approves (3158).
   Another nod, camera `(34,5)` and sleep 30 precede Peter's introduction of a friend (3159).
3. **Introducing Bowie (3160).** Peter faces Down; Bowie moves Right1/faces Up. Peter moves
   Down2, faces Right, moves Left1 and faces Up; Bowie moves Left1/faces Up. Peter introduces
   Bowie as coming from Granseal (3160).

The introduction's map destinations and relative moves are requests with service dependencies,
not an observation of the actors reaching those positions. Text attribution establishes what is
said, rather than independently confirming the speakers' travels or friendship.

## Volcanon's Anger and Refusal

**Confirmed continuation of the same conversation:**

4. **Reacting to Granseal (3161–3163).** Camera `(34,2)`, a rumble request and quake amplitude 2
   surround sleep 90, followed by quake 0. Volcanon reacts and assigns blame (3161–3162).
   Another rumble/quake 3/sleep 90/quake 0 precedes camera `(34,6)` and sleep 5. Peter's
   `eas_2xRightLeft` starts without waiting, followed by sleep 120. Peter faces Up and appeals
   to Volcanon (3163).
5. **Protection and responsibility (3164–3173).** Camera `(34,2)` precedes Volcanon's accusation
   that Grans released Zeon, his account of his protection and his refusal to forgive
   (3164–3166). Rumble/quake 4/sleep 90/quake 0 follow. Camera `(34,5)`, Bowie Right1,
   Peter Right1, Bowie's Up facing and Peter Up3 stage Peter's plea and exoneration
   (3167–3168). Camera `(34,3)` precedes Volcanon's collective-responsibility argument
   (3169–3170), Peter's request for advice (3171), and Volcanon's withdrawal of protection
   and demand for self-reliance (3172–3173).
6. **Strength, destiny and courage (3174–3178).** Another rumble/quake 4/sleep 90/quake 0 precedes
   waited deactivation of Peter's auto-facing, Down1, then waited `eas_Init`. Peter objects
   and compares the greater devils' strength (3174–3175). Volcanon invokes destiny (3176).
   The fifth rumble/quake 4/sleep 90/quake 0 precedes his challenge to show courage and reseal
   the devil (3177–3178).

Blame, protection, comparative strength and destiny remain these speakers' claims. The scene does
not change a protection mechanic or establish that the challenge will be fulfilled. Rumble and
quake commands are presentation requests: five sound requests and amplitudes 2,3,4,4,4, each explicitly
reset to zero, do not measure rendered shaking, audibility or synchrony.

## Jewel Advice and Peter's Commitment

**Confirmed closing content:**

7. **Trusting the jewel (3179–3184).** Peter moves Up1 and again asks for advice (3179).
   Volcanon directs Bowie to trust the jewel (3180). Bowie moves Left1 then Up2. Volcanon
   describes light, darkness and the jewel, and dismisses them until Zeon is defeated
   (3181–3183). Peter moves Up1 and makes another appeal (3184).
8. **Choosing to help (3185–3187).** Sleep 30, two waited Peter jumps, sleep 30 and Peter Down1
   precede his promise, criticism and offer of help (3185–3187). The script then requests
   `join Peter`, `joinForceAI Peter,0` and `followEntity Peter,Bowie,2`, in that order, and ends.

The advice does not mutate inventory or the jewel. Peter's promise is distinct from the subsequent
membership service and from any demonstrated future success. There is no choice/menu or naming
prompt. Across the whole scene, twelve sleeps request 815 frames; dialogue, camera, action, nod,
jump and joining services add their own waits. That sum is not elapsed scene time.

## Joining, Active Capacity, AI and Following Have Separate Outcomes

**Confirmed joining service:** `join` uses word 7, clears current speech sound, waits for scrolling
and requests ordinary join music. It calls `JoinForce`, then supplies class/name indices to
**text 446** (key `01BE`), followed by fade/input, window close and sleep 10. This join-message
request is additional to the 37 cursor texts: a system announcement of Peter joining, using his
class/name, rather than another spoken conversation line. Already joined Peter still receives
these services.

`JoinForce` sets F7 and calls `UpdateForce` to rebuild joined/active/reserve lists, then tests the
resulting active count against 12. Only a count below 12 calls `JoinBattleParty` to set F39. That
helper does not rebuild lists again, so flags and cached lists can disagree temporarily:

| State at the membership service | Result |
| --- | --- |
| F39 clear, active count below 12 after the post-F7 rebuild | F7/F39 set; cached lists/counts retain the earlier F39-clear snapshot until another `UpdateForce`. |
| F39 clear, active count at least 12 | F7 set; F39 remains clear and the snapshot places Peter in reserve. No member is replaced. |
| F39 already set | It remains set, including full/overfull supplied states; the post-F7 rebuild counts Peter if he was previously unjoined. No eviction/normalization occurs. |

F7 may initially be clear or set. Joining neither tests HP nor heals/revives Peter, declares an
entity or sets F67. The flag setter changes only its selected bit. This ordinary join path differs
from the replacement algorithm of `joinBatParty`.

**Confirmed AI mutation:** selector 0 in `joinForceAI Peter,0` reads Peter's activation **word** and
writes old word AND 65531 (`$FFFB`): clear AI-controlled bit 2, preserving every other bit. This
selector does not call `JoinForce`; membership came from the prior command. It does not change F67.
The battle-turn consumer still checks HP, status including MUDDLE/sleep/stun and auto-battle at
their respective boundaries; membership and active placement remain relevant. Removing one forced-AI
condition does not establish an available player-controlled turn. No battle begins here.

**Confirmed current following:** the final command holds follower/target/place words
`$0007,$0000,$0002`. Its alive helper probes the first word's **high byte**, testing Bowie 0's
**word HP**, rather than Peter 7's. Bowie at zero skips all six operand bytes. With Bowie alive and
valid mappings, selector 2 installs Peter following Bowie independently of Peter's HP. The
`(-24,0)` offsets are pixels, shifted by 4 into internal units by the follower consumer, then
subject to facing/distance/layout rules. They are not an immediate fixed world position.

`AddFollower` patches and replaces the action before duplicate-list detection, so an already listed
follower still gets a changed pointer. There is no wait for following to settle. **F67 remains as
supplied**, independently of F7/F39, the AI bit and current following. Later full initialization can
declare Peter when F67 is set; clear F67 gains no future declaration from this scene. A current-map
transition retaining mappings has a different lifetime boundary from a full population rebuild.

## Motion and Partial Completion Depend on Supplied State

**Confirmed command distinctions:** `setDest` reads the actor as a word with **no HP guard**. It
writes destinations in 384 internal units per tile, derives absolute travel magnitudes and signed
velocities of magnitude 32, and here waits for current X/Y to equal destinations: both operands
have bit 15 clear. It is neither a teleport nor a pointer-idle test, and encodes no facing.
`setFacing` instead reads the actual byte actor and tests ally word HP, skipping at zero but not
at HP256. Thus Peter/Bowie facing can be skipped while destination/action/text requests still run.

Lookup resolution masks to a byte, translates NPC128+ by subtracting `$60`, then scales the mapped
physical index by 32. An uninitialized zero Peter mapping resolves physical 0; `$FF` resolves index
255 without a validity check. Missing/aliased actors are not safely skipped. A fresh F67-clear
population cannot be assumed to stage Peter normally even if his stats are alive and joined.

Relative movement adds signed tile deltas scaled by 384, subject to movement/obstruction services.
One tile is 24 pixels, with internal projection shifted by 4. Waited actions poll the selected
actor's pointer for `eas_Idle`; neither this nor scene end waits on all actors. Action installation
has no general HP/presence guard and can replace unfinished actions. `eas_Init` restores speed
32/32 and collision, obstruction, auto-facing/orientation and sprite properties. Deactivating
auto-facing permits the later Down1 retreat without demanding Down facing; the next init restores
ordinary behavior. Jumps temporarily disable and then restore auto-facing.

The nonwaited `eas_2xRightLeft` oscillates **facing**, Right/Left/Right/Left, with four waits 30;
it is not displacement. Sleep 120 is not a completion barrier, and Up facing does not cancel it.
The later Peter Right1 action replaces its pointer. **Inferred:** with valid distinct mappings,
unobstructed completed moves and ordinary updates, coordinate arithmetic leaves Bowie `(39,9)` and
Peter `(39,8)` just before following. This is conditional staging, not an observed endpoint or a
guaranteed position once following starts.

Camera tracking resolves Peter's current mapping without HP admission; destination requests clear
tracking and wait for scrolling. Quake operands select direct low-14-bit amplitude writes, not
gradual ramps. Sound dispatch does not wait for audible completion. Single-text services select
portrait/speech, wait for scrolling, display/advance, close windows and sleep 10; debug skips take
a different path. Nod/join services also have waits. The terminator conditionally waits on remaining
dialogue scrolling and clears scrolling speed; it does not restore control.

## Caller Flags and the First Conditional Map11 Control

**Confirmed return boundary:** only successful script return reaches F722 then F711. A stalled
earlier service establishes neither write. Joining presentation can stall **after membership
changes but before AI clear, following and caller flags**; partially completed state must not be
replaced with an all-or-nothing scene outcome. F722-set entry skips the sequence without repairing
F711, membership, AI, F67 or current actors. Flags alone do not reconstruct scene history.

F722 later selects Bedoe's alternate setup; this caller does not return there or execute that
setup's scenes. F711 suppresses Polca's fairy opportunity in its entity/init/first-zone consumers,
without proving acquisition/dislodgement or setting fairy follower F69. These are downstream
flag consequences only, not travel or completion claims.

After either selected caller path, the zone wrapper closes windows, waits one VInt and waits for
physical 0's X/Y to equal destination. It tests neither `eas_Idle`, other actors nor settled
following. Exploration returns to `WaitForEvent`. Only pending zero retargets camera 0 and calls
`SetControlledEntityActScript(0)`; another pending request takes priority. `PLAYER_TYPE=0` selects
`eas_ControlledCharacter`, while other supplied types select their own actions. Completed services
and zero pending establish this **conditional control handoff**, not observed responsive input.
Stop on Map11 at this boundary.

## Evidence Review and Remaining Limits

This guide consumes research accepted in
[PR #790](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/790) and the preceding English
guide from [PR #789](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/789). Research retains
the USA retail identity, pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`, all 113 scene
records, 37 conversation keys, separate join key and seventeen inline movements. Its
[reproduction boundary](../../research/bedoe-to-volcanon-audience.md#reproduction-and-evidence-limits)
owns source/retained comparisons. This slice reviewed accepted prose and bounded tracked-fixture
selections without source reinvestigation, optional payload generation/copy or new observation.

| Reviewed relation | Executable/contract owner | Adversarial disposition |
| --- | --- | --- |
| Caller/init | [sf2-map-events-static-v1:zone-programs](../../../tests/fixtures/h2/map-events/zone-programs.json), programs 6/7; [sf2-map-init-static-v1](../../../tests/fixtures/h2/map-init-static-v1.json), body 16; [request consumption](../../research/map-event-request-consumption.md). | Six ordered operations retain the sole F722 check, script and later 722/711 writes; default/init are returns. Coordinates are not natural movement proof. |
| Entry/setup/cast | [sf2-map-data-static-v1](../../../tests/fixtures/h2/map-data-static-v1.json), [sf2-map-setup-static-v1](../../../tests/fixtures/h2/map-setup-static-v1.json), [sf2-map-entities-static-v1](../../../tests/fixtures/h2/map-entities-static-v1.json), [map exploration](../contracts/map-exploration.md). | Summary inventories do not decode every selected row/population. Accepted research owns explicit/current-map composition and speaker128's actual mapping; attribution supplies no new rendering fact. |
| Text/motion/services | [sf2-map-script-engine-static-v1](../../../tests/fixtures/h2/map-script-engine-static-v1.json), dialogue/placement/action/relationship/presentation families; [dialogue](../contracts/dialogue-system.md). | Command-family facts do not contain this complete transcript or observe the scene. Research supplies all conversation keys and separate 446; requests/waits/units remain distinct from presentation. |
| Join/AI/follow | The same fixture's `forceStateCommandFacts` and `activePartyCommandFacts`, [sf2-common-stats-static-v1](../../../tests/fixtures/h2/common-stats-static-v1.json), [party/roster](../contracts/party-roster-state.md). | Macro/handler order and provenance joins are narrower than a capacity lifecycle or playable turn. Research owns this selected join/count snapshot, bit2 clear, high-byte HP admission and unchanged F67. |

The [map-init schema](../../../schemas/h2-map-init-static-fixture.schema.json) and
[verifier](../../../src/sf2tool/h2/map_init.py) retain complete body/provenance equality. The
[map-script verifier](../../../src/sf2tool/h2/map_script_engine.py) and
[schema composition](../../../src/sf2tool/h2/map_script_schema.py) retain separate command families
and consumers. The roster contract's general capacity/presentation Unknowns are not closed by this
selected static branch. No Layer A correction was needed. Existing narrow routes are
`uv run sf2 h2 map-events`, `map-init`, `map-script-engine`, `map-data`, `map-setup`, `map-entities`
and `common-stats`; listing them is not a fresh execution claim. Controlled H3 handler matrices
remain local seams, not evidence of this composed route.

**Unknown:** natural floor/collision traversal and request order; supplied flags, HP/status, active
capacity, AI, mappings and positions; completion of movement, nonwaited actions, camera/text/join
services and pending requests; rendered shrine/Volcanon, audio/quake/timing and responsive input;
future following, battle placement and continuous playability. These remain with their evidence
owners without an automatic acquisition queue. This guide completes neither the campaign nor the
design epic.
