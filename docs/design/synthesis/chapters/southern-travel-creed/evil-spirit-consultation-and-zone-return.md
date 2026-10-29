# Evil Spirit Consultation and Conditional Zone Return

Creed summons the Evil Spirit to ask about Zeon. Its first attempt at contact is
blocked, so Peter draws attention to the leader's jewels. After inspecting them,
Creed asks that they be shown to the spirit. The spirit reports a renewed threat;
a voice identifying itself as Zeon interrupts, demands the Jewel of Evil at Arc
Valley and offers Elis in exchange. The staging then depicts the spirit's
explosion. Creed and the Fairy discuss the loss and leave the room.

**Inferred:** this is the narrative meaning of the ordered staging.
**Confirmed:** these are attributed dialogue claims and source commands, not an
implemented jewel removal, exchange, Elis return or recruitment. The program
contains no inventory transaction. **Unknown:** natural reach, actual supplied
state, completed services, rendered presentation, usable control and persistence.

This guide continues [shrine entry and scene admission](evil-spirit-shrine-entry-and-scene-admission.md)
and consumes the accepted [complete consultation research](../../../../research/chapters/southern-travel-creed/evil-spirit-consultation-and-zone-return.md)
from #949/PR952 on accepted main `9de649eb1da588065f513b5d0f32819636bb8933`.
It explains the complete scene and stops at its conditional caller/wrapper return.
Later walked return, mansion reentry, Fairy events, recruitment and Oddler's
departure remain separate boundaries. This Layer B explanation selects no remake
behavior or new runtime work.

Continue with [Creed return, Fairy following and recruitment choices](creed-return-fairy-and-recruitment-choices.md)
for the separate mansion return and immediate room interactions.

## Who Is Present, and What Admits the Scene

**Confirmed:** a separately produced type6 request matching Map34's wildcard X
and Y low byte13 selects Map34_ZoneEvent0. The consumer clears the pending type
and installs Init on physical 0 before the callback checks F784. Clear admits
cs_5B6C0; set skips the scene and both caller writes. F783, F523 and F802 are not
extra predicates, and entering the shrine does not itself trigger consultation.
The [entry owner](../../../../research/chapters/southern-travel-creed/evil-spirit-shrine-entry-and-scene-admission.md#actual-zone-request-and-scene-call-boundary)
retains the layout, candidate/destination and request-production premises.

Fresh F784-clear entry declares 21 residents. Their logical IDs resolve through
actual lookup state; logical, physical and stored ENTNUM values are distinct:

| Scene role | Baseline identity and allocation |
| --- | --- |
| Creed128 and Fairy129 | CREED212 and FAIRY219; physical N+1 and N+2. |
| Evil Spirit130 | EVIL_SPIRIT252; special physical 47, stored ENTNUM 32. |
| Dialogue131 | BEASTMAN2=222; ordinary physical N+3. It supplies the Zeon voice, not a newly created Zeon actor. |
| Unused132 | BOY200; ordinary physical N+4, with no selected scene speech or motion. |
| Effects133–148 | Sixteen distinct EFFECT1=63 residents, physical N+5 through N+20. |
| Bowie0, Peter7 and FOLLOWER_B31 | Separate main/follower lookups; B is Oddler only under the baseline follower premises. |

N counts ordinary followers before residents. With only F67/F68, N=2: Peter/B
are physical 1/2, Creed/Fairy3/4, 131/132 physical 5/6 and effects7–22. The spirit
remains physical 47. Different follower flags can make B resolve to Astral;
repeated B allocations overwrite that lookup. Absent rows can resolve to
physical 0 through zero mappings. Hiding changes matching lookups to -1, with
no local repair. Fresh F784-set entry creates no residents; changing a flag on
retained population does not rebuild it. The [accepted actor premises](../../../../research/chapters/southern-travel-creed/evil-spirit-consultation-and-zone-return.md#actor-and-admission-premises)
own HP/class/mode/raft and allocation limits.

The nominal story below assumes distinct baseline mappings and returning
services. No scene command creates this cast, changes its resident mapsprite IDs,
or acts as 132. Special graphics selections and dialogue actor131 remain separate.

## Summoning and the First Blocked Contact

**Confirmed, scene lines 4–105 / texts 3207–3226:** Creed invokes the spirit and the
Fairy announces its appearance. The program repeatedly places the spirit at
(7,6), facing Down, then parks it at (60,60), with changing sleeps between visits.
After nine appearance/park pairs it leaves the spirit at (7,6). This is a source
placement sequence, not an observed materialization animation.

The spirit receives Transparent, which requests blinking ON and jumps to Idle;
it does not interpolate opacity or replace the sprite. Init is installed and
waited separately on Bowie, Peter and B, then they are placed at (7,12), (6,12)
and (8,12), facing Up. Creed approaches and asks what Zeon is doing. The spirit
claims it can contact him. Flashes and white-map fades accompany its attempt;
the party advances while the Fairy explains the channeling and requests silence.

The spirit reports a barrier hiding Grans. Creed asks it to try again, and it
requests an object associated with Zeon. These reports do not independently
prove the barrier's world state. The camera visits (2,4), then (2,2), then (2,4);
the later camera/service discussion explains what those requests establish.

## The Jewels Are Inspected and Shown

**Confirmed, lines 106–179 / texts 3227–3242:** Peter mentions the leader's Jewel
of Evil. Creed identifies a sealing jewel and asks for it. Peter says it cannot
be removed; Creed initially says he can remove it, then asks to inspect it.
Creed's brief Down/Up actions, flashes and gestures accompany the inspection.
He sees two lights, Peter names the Jewel of Light, and Creed says the Mithril
chain prevents removal. He asks that both jewels be shown to the spirit.

Bowie advances toward the spirit while Creed moves aside asynchronously. The
unattributed text 3240 narrates the leader displaying the jewels; Bowie is also
flashed, and the spirit claims improved contact. Alternating shared palette
helper requests accompany another concentration attempt.

**Inferred:** this depicts inspection and display. **Confirmed:** the complete
program performs no inventory transfer or jewel-removal command. The Mithril
chain explanation is Creed's dialogue. The text's `{LEADER}` substitution uses
the current first battle-party member's name; the nod and placement commands
address Bowie. Those authorities can differ under supplied membership.

## What the Spirit Reports

**Confirmed, lines 180–196 / texts 3243–3257:** the spirit says Arc Valley has
opened, Zeon has revived and devils are flying toward North Parmecia. Creed
suspects an event involving Mitula. The spirit says it cannot see Zeon, places
him in Arc Valley and says his strength has not fully recovered. Creed asks
about his power; the spirit starts to explain.

These are attributed reports, not newly verified campaign facts. Creed's
statement across 3254–3255 remains one continuation: 3254 ends only `{N}`; 3255
continues with `{N}{W2}`. Do not insert an acknowledgement or close between them.
The [exact text/control owner](../../../../research/chapters/southern-travel-creed/evil-spirit-consultation-and-zone-return.md#text-identity-speaker-and-continuation-state)
retains all 73 joins, including this N-only segment.

## Zeon's Interruption and Demand

**Confirmed, lines 197–256 / texts 3258–3270:** the special-graphics helpers
alternate EvilSpiritAlt and EvilSpirit selections as Creed asks the spirit to
finish and receives a broken reply. A longer screen-flash request precedes the
Zeon-labelled graphics selection. Two rounds install Jump on Bowie, Peter, B,
and Creed, followed by a waited Jump on the Fairy. Each round waits for the
Fairy's action only; it does not join the other four actors.

The voice addressed through 131 identifies itself as Zeon, threatens the caller
and recognizes the Jewel of Evil. Creed says Zeon is seeing through the spirit
and calls it impossible. The voice identifies the leader and demands the jewel
at Arc Valley. Peter objects; the voice offers Elis in exchange and repeats the
destination. The source then alternates special graphics and flashes, sets quake
amplitude 5 and requests the big-door rumble sound.

**Inferred:** the staging depicts Zeon taking over the contact. **Confirmed:**
131 retains its declared BEASTMAN2 mapsprite and ordinary allocation. The
special-sprite helpers update shared graphics independently; the Zeon label
creates no actor, changes no dialogue actor into a new sprite and performs no
exchange or Elis return. There is no consent menu here.

## Sixteen Effects, Five Bursts and the Spirit's Hide

**Confirmed, lines 257–540:** each effect 133–148 separately receives a waited
custom action setting speed 64/64 then jumping to Idle. This does not reset its
motion, collision, transparency or other inherited state. The source separates
rays from two sets of cyclic actors:

| Group | Requested trajectory and completion boundary |
| --- | --- |
| Inner 141–144 | Placed at (8,6), (7,5), (6,6), (7,7), Right; enter four phases of the clockwise cyclic program. |
| Outer 145–148 | Placed at (8,6), (7,7), (6,6), (7,5), Left; receive Right1/Down1/Left1/Up1 respectively, sleep 2 then StopMoving; enter four phases of the counterclockwise cyclic program. |
| Rays 133–140 | Before each of five bursts all are reset to (7,6), Right. They receive Right4, Up4, Left4, Down4, UpRight4, UpLeft4, DownLeft4 and DownRight4 respectively. Only 140 is waited. |

The cyclic programs consist of relative moves followed by destination waits and
a branch back, with no Idle exit. Their source labels do not reliably describe
the first operand: `eas_ClockwiseMoveUpRight` starts with (-1,+1). The inner moves,
if completed, traverse phases of a diamond; outer trajectories start at the
actual stopped coordinates, not an assumed whole-tile offset. The
[complete trajectories](../../../../research/chapters/southern-travel-creed/evil-spirit-consultation-and-zone-return.md#sixteen-effect-trajectories-and-hides)
retain every entry, delta and cyclic tail. Neither group proves a measured
radius, synchronized ring or rendered circle.

Each burst requests death SFX before its rays. Repositioning and replacing
unjoined ray actions can interrupt their prior movement. After burst 3 the spirit
is hidden and another rumble is requested; bursts 4 and 5 follow. After burst 5,
133–140 hide in order, then after sleep 40, 141–148 hide in order. These effects
implement no combatant death, EXP award or `eas_Die` transaction.

Hide writes current and destination coordinates to ($7000,$7000), invalidates
all matching lookups and removes the physical actor from the exploration list
if present. It preserves action pointers, sprites and flags and does not wait
for Idle or deallocate the record. Crucially, the ordinary entity loop skips
current X at least $7000 before entity/action dispatch. Hidden cyclic slots
therefore remain dormant while that X remains; retaining a pointer is not
continued orbit motion. Later reactivation and rendered absence are **Unknown**.
Aliasing can make a hide invalidate several logical IDs at once.

## Aftermath and the Attendants' Exit

**Confirmed, lines 541–580 / texts 3271–3279:** quake $4005 requests the descending
ramp 4,3,2,1,0, with an internal Sleep 40 after each write. The final palette-restore
helper request and sleep 60 precede the discussion. Creed says the spirit exploded,
attributes that to Zeon's power and warns about full revival. The Fairy questions
the spirit's reliability; Creed affirms it, wonders why Zeon wants the jewel and
says the spirit can no longer answer.

Creed receives a waited Down1, Left1, Down5 path and is hidden. The Fairy receives
a waited Left1, Down1 path, calls after him, jumps twice with waits, asks him to
wait, then receives Down5 and is hidden. Peter and B are faced Down before her
last lines; Bowie's earlier Down facing has its own HP guard. Movement after
Creed's Init can change facing through autofacing, so the last explicit facing
is not an unconditional final orientation.

Finally the program requests Peter→Bowie direction 2, then B→Peter direction 2.
The encoded follower words are 7 and 31, but the handler tests their high byte
before decoding them. Both high bytes are 0, so both calls are guarded by Bowie's
HP, not by Peter's or B's. If admitted, direction 2 selects offset (-24,0);
AddFollower installs the patched 42-byte following template and appends the
physical follower to the exploration list only if absent. It does not recruit,
set F67/F68, teleport, repair a lookup or wait for settled following. Aliases can
produce self-follow or replace the first relation. The
[conditional-end owner](../../../../research/chapters/southern-travel-creed/evil-spirit-consultation-and-zone-return.md#conditional-end-state-and-zone-caller-return)
retains these exact guards and the template's reset/follow boundary.

## Reading Text, Motion and Presentation Requests

**Confirmed:** the complete scene contains 409 top-level records, 58 movement
bodies, 16 custom bodies and 73 exact text/control joins. The six narrative
phases above cover scene lines 4–580 without omitting its non-dialogue explosion interval.
The [complete ordered owner](../../../../research/chapters/southern-travel-creed/evil-spirit-consultation-and-zone-return.md#complete-ordered-scene)
retains all contiguous record partitions; this guide does not duplicate the
transcript or opcode inventory.

The nominal cursor runs 3207–3279 and ends 3280. `nextSingleText` displays,
increments, closes portrait/text and sleeps 10; `nextText` leaves continuation
open. W1/W2 are input waits, not choices, and the form does not determine the
number of waits. Creed uses modifier $80, requesting a right-side unmirrored
portrait; other actors use 0. Baseline Creed selects CREED/bleep 8, Fairy NONE/1,
spirit NONE/8 and 131 NONE/6. Peter's actual base/promoted sprite selects its
portrait and bleep 4/5; changed sprites can differ. Narration 3240 uses `$FFFF`
and speech SFX 0. An already open portrait or NONE result can suppress opening;
selecting 131 does not request a Zeon portrait.

ExecuteMapScript clears the text-skip latch on entry. Debug mode with P2 Start
can latch it, skipping subsequent text display/cursor advancement and dispatcher
csWait sleeps. Other action/helper commands and their internal waits remain.
Thus 73 nominal joins are not 73 observed displays, and debug skipping changes
the time available for concurrent motion.

Action installation replaces the resolved actor's pointer and seeds its wait
timer. Waited installations poll that actor for Idle without timeout; generated
movement waits compare its current and destination coordinates. They do not
join other actors. Bowie/Peter Up1 concurrent with B's waited Up1, Creed moving
aside during Bowie's waited approach, the grouped jumps and all five ray resets
must retain that distinction. StopMoving retargets from current coordinates via
moveRel(0,0); it does not finish the overwritten move. `stopEntity`/`startEntity`
write animation counters -1/0 and are different operations. Init resets its named
movement defaults but neither coordinates nor every flag. Ally positioning and
facing can skip IDs below 30 at HP 0; action installations and the selected flashes/
gestures do not share that skip. Under aliases or delayed service, replacements
can alter the nominal choreography. See [actions and completion](../../../../research/chapters/southern-travel-creed/evil-spirit-consultation-and-zone-return.md#actions-placement-and-completion).

Presentation has separate immediate effects and completion limits:

| Surface | Accepted request/effect and limit |
| --- | --- |
| Camera | Destinations (2,4),(2,2),(2,4),(2,5),(2,4); following target becomes -1 and each command waits for scrolling. No command restores following; a camera wait proves no player-control handoff. |
| Special graphics | Helpers select EvilSpirit+$20, EvilSpiritAlt or Zeon+$20; decompress into the loading buffer and queue $480 words ($900 bytes) to VRAM $AF00..$B800. EnableDmaQueueProcessing sets DMA_REQUEST without waiting for VInt consumption. Last selection is EvilSpiritAlt; no final graphics restore. |
| Shared palette helpers | P1 remaps 64 CURRENT colors into BASE using the accepted lookup/rounding rule, then backs up CURRENT into COPY. P0 copies its existing COPY to BASE before replacing COPY with then-current CURRENT. Both start the 32-update transition with the automatic-return byte cleared. |
| Palette interpolation | Timer-derived weights sum to 8; updates write CURRENT and request CRAM DMA. Intermediate snapshots, helper calls and flash resets can interact. Final P0 targets its selected COPY snapshot, not a guaranteed original room palette; sleep 60 is no completion poll. |
| White fades/screen flash | Before contact: three flash 4 calls and white-map out/in fades; interruption: flash 40; aftermath: four flash 4 calls. csc41's shifted counter requests one launch for 4, six for 40. LaunchFading conditionally waits for its setting to clear and another VInt; operands are not frame counts. |
| Entity flash/gestures | Creed 100 requests 26 flash cycles; each Bowie 40 requests 11. Sprite flags/update waits, nod/headshake/shiver transforms and shared-size restoration have their own effects; they do not complete all motion. |
| Quake/audio | Amplitude 5 begins the explosion; completed $4005 leaves 0 through the five descending writes. Internal sleeps survive debug text skip. Sound requests prove neither playback nor audiovisual completion. |

The [selected helper/service owner](../../../../research/chapters/southern-travel-creed/evil-spirit-consultation-and-zone-return.md#special-graphics-palettes-and-presentation-services)
retains exact sequences, palette arithmetic, flash counters and consumers.
Names, sleeps and service returns establish no measured timing or rendered result.

## Conditional Caller and Wrapper Return

**Confirmed:** csc_end conditionally waits for view scrolling if the dialogue
window is open, clears VIEW_SCROLLING_SPEED, restores saved registers and returns.
It does not poll every actor or palette/DMA queue, restore camera following,
reset hidden slots or normalize a newly written pending event. Register restores
also do not roll back global scene state.

Only a returned scene reaches caller `setFlg 784` and then `setFlg 802`. After callback
return, the wrapper closes portrait/text, waits one VInt, then waits for physical 0's
current X/Y to equal destination before restoring its registers and returning.
This tail also applies to the F784-set or default callback path. It is a
coordinate wait for one physical actor, not an Idle, follower or service barrier.

The ordering is non-atomic. Interruption during the scene can leave partial
positions, actions, graphics and hides without either new caller flag;
interruption between the two writes can leave F784 set/F802 unchanged. Both
flags may be set while physical 0's wrapper wait is still pending. Neither proves
usable control, completed presentation or save/load persistence.

These **Confirmed static discriminators** bound changed supplied state:

- Fresh F784-clear entry rebuilds baseline residents; a qualifying request still
  needs to be produced. Fresh F784-set entry creates no residents; the callback
  skips the scene call and both caller flag writes.
- Setting F784 on retained baseline changes admission without removing residents.
  Clearing it on empty/hidden population admits a call without recreating actors.
  Repetition is not a reset; explicit fresh34 and CURRENT255 reuse differ.
- Changing F783/F802 alone leaves the local gate unchanged. Already-set F802
  does not prevent consultation. Other supplied follower/HP/class/mode/raft
  values can change actors, aliases, sprites and admitted commands independently.
- A supplied Y13 request does not establish natural reach. Different Y low byte
  selects default; actual request writers and layout remain separate premises.
- A nonreturning action/text/camera/fade wait prevents later source operations.
  Debug skipping changes cursor/scheduling; it does not skip every internal wait.
- A new request written during scene/wrapper is not explicitly cleared here;
  callback flags and physical 0 settling do not prove the next usable event state.

**Unknown:** natural route and caller state, actual population/HP/mappings,
service completion, exact presentation, control and persistence. These gaps do
not automatically authorize runtime acquisition. Later mansion/Fairy/recruitment
and Oddler events require separate accepted evidence and synthesis scope.

## Evidence Review and Retained Results

Adversarial review covered the complete accepted #949 research, its entry/action
predecessor boundaries and the dialogue/graphics/special-sprite contracts. It
checked the full narrative order, text 3254's continuation, actual actor
identities, guarded commands, unjoined/replaced motion, dormant hides, helper
snapshot/DMA boundaries and the non-atomic caller/wrapper tail. No contradiction
was found in the selected accepted joins; broad runtime/presentation questions
remain Unknown, not evidence that this nominal scene completed.

| Directly inspected public owner | Review boundary |
| --- | --- |
| sf2-map-events-static-v1:routing-setup, [routing-setup.json](../../../../../tests/fixtures/h2/map-events/routing-setup.json) | Exact ms_map34_ZoneEvents wildcard255/Y13 and default records, targets and source lines. |
| sf2-map-events-static-v1:zone-programs, [zone-programs.json](../../../../../tests/fixtures/h2/map-events/zone-programs.json) | All six Map34_ZoneEvent0 operations, addresses/operands/line order, clear/set polarity, cs_5B6C0, F784 then F802 and default rts. |
| sf2-map-script-engine-static-v1, [map-script-engine-static-v1.json](../../../../../tests/fixtures/h2/map-script-engine-static-v1.json) | cs_5B6C0's 409-command aggregate; dialogue, action bridge, placement, lifecycle, gesture/follow, camera, screen and entity-FX handler facts/scene references. It contains no full programCorpus. |
| [Map-script schemas](../../../../../schemas/h2/map-script-engine-fixture.schema.json) and [verifier](../../../../../src/sf2tool/h2/map_script_engine.py) | Composed dialogue/entity/presentation/script boundaries and provenance/semantic fact comparison; source references and aggregate facts do not replace full command bodies. |
| [Event shard consumer](../../../../../src/sf2tool/h2/map_events_fixture.py) and [verifier](../../../../../src/sf2tool/h2/map_events.py) | Shard/section identity, target-program schema and complete expected output/provenance equality, rather than legal shape alone. |

Both event shards/expected sections and the map-script fixture were directly
schema-validated. Exact callback joins and the selected handler facts were
checked against the stricter research boundaries. The [dialogue contract](../../../contracts/interface/dialogue-system.md),
[graphics service contract](../../../contracts/graphics/graphics-service-state.md) and
[special-sprite data contract](../../../contracts/graphics/special-sprite-graphics-data.md)
retain their own controlled/aggregate scopes; none independently proves this
consultation's natural text, rendered graphics or completed transfer.

The research pins [SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm)
and USA ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
The [reproduction owner](../../../../research/chapters/southern-travel-creed/evil-spirit-consultation-and-zone-return.md#changed-premises-evidence-limits-and-reproduction)
names the retained comparison recipe, full program selection by id=cs_5B6C0,
source/text joins and immediate consumers. This checkout lacks that upstream
and retained pair; no new comparison, extraction, acquisition or runtime result
is claimed. Narrow owners remain `uv run sf2 h2 map-init`, `uv run sf2 h2 map-events`
and `uv run sf2 h2 map-script-engine`, with the existing retained recipe.

The [#949 frozen handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/949#issuecomment-6178414274)
and [PR952 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/952#issuecomment-6178451561)
retain head 3917f0e9/tree 46e77101/base 4a87e98e, the unchanged 18,285,979-byte
pair comparison 0.079s, complete 409/58/16/73 source/body/text joins and
CI 38636725332 scope 14s/research-public 36s PASS. Their corrected dormant-slot,
text and encoded-follow facts and all inspection/parse diagnostics remain
retained. These are accepted results, not fresh reruns.

The linked [#945 original](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/945#issuecomment-6178009849),
[rebased handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/945#issuecomment-6178108485)
and [acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/948#issuecomment-6178118055)
retain the 0.084s pair, original/rebased CI and newline/index diagnostics.
[#941's handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/941#issuecomment-6177767348)
and [acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/944#issuecomment-6177797802)
retain the 0.080s result, corrected 1892 controls and high-byte/Bowie-HP reasoning.
Their [#937 original](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/937#issuecomment-6177275252),
[review](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/939#issuecomment-6177302395),
[corrected handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/937#issuecomment-6177356550)
and [acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/939#issuecomment-6177369382)
keep completed failures and Unknowns. No completed-failure suite is restarted.
