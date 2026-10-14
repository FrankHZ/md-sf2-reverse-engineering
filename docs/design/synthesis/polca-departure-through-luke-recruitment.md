# Polca Departure and Luke Recruitment

Luke's departure scene presents him volunteering to travel and fight with Bowie. A selected
south exit from Polca requests the overworld, where the recruitment scene can run during map
initialization. It creates and stages a Luke actor, joins Luke to the force, then moves and hides
that actor before writing a later battle unlock. These are separate ordered effects: accompanying
the group, appearing in the scene, joining the force and being active in battle are distinct states.

**Confirmed static content and control flow:** the accepted
[research owner](../../research/polca-departure-through-luke-recruitment.md) supports the admission,
complete scene and immediate consumers below. **Inferred:** the attributed exchange depicts Luke
volunteering for the journey. **Unknown:** natural departure, actual supplied state, completed
services, visible presentation, responsive input and save/load persistence. Fnnn means flag nnn;
coordinates are map tiles unless another unit is named.

Read [Polca's Oddler choices and following](polca-oddler-choice-and-following.md) for the two
requests and current/future following, and [the raft grant](battle13-outcomes-through-polca-raft-grant.md)
for the mayor's bargain. This guide explains the selected departure through conditional exploration
control. Boarding, river travel and subsequent battle admission remain beyond its boundary.
It adds no remake capability or product decision. Continue with
[Polca raft boarding and South Parmecia entry](polca-raft-boarding-through-south-parmecia-entry.md)
for the separate player request, staged mode2 commit and two selected Map69 entry lifecycles.
That guide preserves this scene's contact and natural-route limits.

## Leaving Polca and Reaching the Scene

**Confirmed selected request:** Polca's south warp matches any X at Y31 and requests Map72
`(43,47)`, Down. Two earlier same-map blacksmith doors do not leave town; the later east exit
requests Mount Volcano. At `(31,31)`, the south row wins over the east row. Ordinary movement
must produce a warp request from the appropriate layout marker, and its consumer must return
through map selection. The tuple alone proves neither a legal walk nor completed travel.

The south row has no Oddler, raft, recruitment or earlier-scene prerequisite. Polca's independent
Y30 zone can select the Oddler request under F716-set/F718-clear, but the selected south warp
does not prove that zone was reached, answered or bypassed. No natural scene order follows from
adjacent coordinates. The [exit/admission owner](../../research/polca-departure-through-luke-recruitment.md#selected-exit-and-interruption-before-entry)
retains request production, pending-slot consumption and source priority.

Before exploration init, ordinary admission checks Map72's battle rows in order:

| First eligible row | Unlock / completion flags | Consequence for this request |
| --- | --- | --- |
| Battle08 | F408 / F508 | Can interrupt before recruitment. |
| Battle24 | F424 / F524 | Considered only if the earlier row did not select a battle. |
| Battle26 | F426 / F526 | Considered only if neither earlier row selected one. |

All three have wildcard triggers. Their area rectangles are battle outputs, not containment
checks, so `(43,47)` does not exclude them. A set completion flag clears the selected row's
unlock but still admits that battle on this call. With all three unlocks clear, ordinary Map72
exploration is selected. A later battle return uses its separate entry path; no return tuple or
immediate repeated admission check is assumed here.

**Confirmed init admission:** Map72's ordinary init checks only F716 and Luke membership F10.

| Supplied state | Recruitment scene |
| --- | --- |
| F716 clear | Skip, regardless of F10; no repair of a missing later unlock. |
| F716 set, F10 set | Skip; no consistency check of actors, raft, following or F416. |
| F716 set, F10 clear | Run `cs_4FF5A`, even with F416 already set. |

F718, F68, F75, F64, HP, party capacity and coordinates are not extra init gates. Thus the
conversation-completion flag F716 can admit Luke without Oddler following or a raft. Supplying
that flag does not prove the mayor's conversation or valid raft state. Unused suspend-scene
bytes are outside this ordinary path.

## Who Appears on the Overworld

**Confirmed fresh entry:** the explicit south request rebuilds Map72's actors before init,
without an intervening player-control handoff. Entry includes the ordinary heal of living/immortal
allies, but the later creation and joining commands do not heal. Map72's fixed entity list is empty;
flag followers and main physical 0 still populate it. Polca's NPC140 and its installed following
pointers are not carried over as the same actors.

F67 declares Peter7, F68 declares an Oddler sprite under **FOLLOWER_B lookup 31**, and F75 declares
Luke10. Membership and HP do not admit these follower rows. F65 prefixes the caravan and shifts
physical slots; other supplied flags can also change the population. F74/F70 can supply Astral
under lookup 31, with ordered overwrites when conflicting flags coexist. The scene repairs none
of those conflicts or absent followers.

The raft is separate: ordinary non-raft player state plus F64 and a matching `RAFT_MAP` declares
it before init. The accepted grant tuple places it on Map72 `(43,48)`. Its **physical 31, mapping 63**
is distinct from FOLLOWER_B's **lookup 31, mapping 31**. F716 alone does not declare or repair it.
Supplied raft-player state uses its separate main-sprite/standing-follower/off-map-raft branch.

A supplied `MAP_CURRENT` request follows the no-rebuild alternative: it updates main and invokes
the separate raft consumer without reconstructing actors or clearing temporary flags. Init can
still run, with different current mappings. The selected south row is explicit 72. The
[population and entry owner](../../research/polca-departure-through-luke-recruitment.md#fresh-population-raft-declaration-and-init-guard)
retains both entry branches and raft limits.

**Confirmed new actor:** the scene creates Luke10 at `(43,47)`, Down, choosing one above the
maximum signed nonnegative physical index in mappings 0–62. It excludes raft mapping 63, overwrites
lookup 10 and performs no existing-Luke reuse or explicit capacity check. Its `NOTHING_BYTE` sprite
operand is replaced by Luke's HP/class sprite selection: zero word HP outside battle selects blue
flame; otherwise his class/table sprite is used. Creation neither joins, heals nor appends him to
the exploration follower list. The following one-frame sleep is not an initialization-completion join.

With fresh entry, ordinary player and only F67/F68, Peter and FOLLOWER_B occupy physical 1/2 and
the new Luke occupies 3. If F75 is also supplied, old Luke3 remains in the follower list while new
Luke4 replaces lookup 10. Later commands and hide resolve the new Luke; they do not remove every
Luke sprite. Adding only F65 shifts these follower/new indices by one. These examples require
the stated mappings, rather than defining permanent character slots.

Placement and facing can skip Bowie/Peter/Luke on zero word HP; FOLLOWER_B31 bypasses that guard.
Mapping lookup adds no safe presence check. An absent Peter with nonzero HP can resolve main0;
an undeclared 31 can move main to `(45,48)`, Left, while an Astral mapping moves that actor instead.
The script creates neither Peter nor 31. See the
[allocation and staging owner](../../research/polca-departure-through-luke-recruitment.md#new-luke-actor-and-explicit-staging)
for the exact lookup and sprite consumers.

## Luke Volunteers and Joins

**Confirmed requested sequence:** the complete scene has six attributed cursor texts 1556–1561
(`0614..0619`), plus join message 446 (`01BE`). It has no Yes/No or naming prompt. The dialogue
below is paraphrased; directions in speech do not prove an accessible route.

| Stage | Requested action and attributed content |
| --- | --- |
| Gather | Create Luke, sleep1, then place Bowie `(45,47)` Left, Peter `(44,47)` Down, Luke `(43,47)` Down and FOLLOWER_B `(45,48)` Left, subject to the HP/mapping limits above. |
| Propose the journey | Request main-theme music, `fadeInB` and sleep40. Luke 1556 proposes Devil's Tail to see Creed. Request two separately waited Luke jumps, then sleep30. |
| Volunteer | Turn Peter Left and Luke Right. Peter 1557 notes Luke is coming; Luke 1558 says he wants to fight with the leader in continued text, and Luke 1559 expresses excitement. Request `join ALLY_LUKE`. |
| Join notice | Request join music, commit membership and any conditional active flag, then request message 446 using Luke's class/name, followed by fade/input/text services. |
| Departing exchange | Turn Peter Right for Peter 1560's comment on Luke's adventurousness, then Left. Luke 1561 urges departure; sleep30. |
| Leave the current scene | Wait for Luke's inline Down1 action, hide his mapped actor, set story operand 16 (F416), then terminate and return to init. |

Each jump waits for its chosen actor's action pointer to reach `eas_Idle`; the final Down1 waits
on that actor in the same way. These waits do not inherit placement's HP guard or prove that
all followers have stopped. Initialization restores movement/sprite parameters, not health.
The four standalone sleeps request 101 frames in total, excluding dialogue and other services;
this is not measured elapsed duration. Single text closes text/portrait and requests Sleep10;
continued text has its separate lifecycle. Rendered jumps, text, music and fades remain Unknown.

## Membership, Hiding and the Later Unlock

**Confirmed membership chronology:** `JoinForce(10)` sets **F10**, rebuilds joined/active/reserve
caches with `UpdateForce`, then checks the rebuilt active count against 12. Below 12 it sets
**F42 (`32+10`)**; otherwise it preserves the supplied active flag. It neither evicts another
member nor guarantees a new active slot at full capacity. An already-set F42 remains set even
in an overfull supplied state.

The rebuild occurs before that conditional F42 write, with no second rebuild in the helper.
A previously inactive Luke can therefore remain in that cached reserve/count snapshot until
another `UpdateForce`. Membership does not establish immediate cache synchronization, HP,
revival or a player turn. There is no F75 clear, follower append, follow command or force-AI write
in this scene. The [membership chronology contract](../contracts/party-membership-state.md#joinforce-chronology)
and [scene owner](../../research/polca-departure-through-luke-recruitment.md#membership-current-hiding-and-the-later-unlock)
keep these boundaries distinct.

After the waited Down1, hide moves the currently mapped Luke off-map, invalidates every mapping
to that physical actor and removes its exploration-list entry if present. The ordinary new actor
was never appended; an older F75 follower at a different physical index can remain. Hide clears
neither F10, F42 nor F75. On a later fresh entry, F75 can reconstruct Luke independently of F10;
F75 clear omits him from the empty fixed list. F10 still suppresses recruitment. Current hide is
therefore not permanent visual removal, and a no-rebuild entry is not fresh reconstruction.

The later story write sets **F416**, unlocking Battle16. It neither sets F414 nor starts combat
or calls battle admission. Battle14/16 numbering proves no natural route order. The sequence is
actor/staging → F10/rebuild/conditional F42 → join services → further dialogue → waited Down1 →
hide → F416 → caller return. It is not an atomic transaction or automatic recovery mechanism.

A stall before joining can leave F10 clear and a staged actor; another eligible init creates
rather than reuses Luke. A stall after F10 is set can leave F416 clear and movement/hide incomplete,
yet a later init skips the scene. Conversely, F416-set/F10-clear still runs it. These are static
partial-state consequences, not observed save histories or promised repairs.

## Conditional Exploration Control

**Confirmed boundary:** Luke's Down1 points toward the configured raft tile only under the
intended staging and completed movement. It is Luke's action followed by hide, not player boarding
or guaranteed raft contact. The scene writes neither player type nor raft coordinates and queues
no boarding or warp event. Collision, HP-skipped placement and alternate mappings remain relevant.

The terminator conditionally waits for scrolling when a dialogue window remains, clears scrolling
speed and returns to init. Exploration requests map music and fade only if its base/current palette
comparison differs; there is no guaranteed second music request. At `WaitForEvent`, pending zero
retargets the camera to physical 0 and sets the controlled entity action; ordinary player type0
selects character control. Nonzero pending or another player type takes its separate handling.
**Stop at this conditional control handoff.** A returned script is not a responsive-input receipt,
boarding completion, subsequent travel or Battle14/16 admission.

## Evidence Review and Remaining Limits

This guide consumes [research accepted in PR814](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/814)
and its accepted predecessor owners from main. Research retains USA retail identity, pinned
SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`, complete scene/text/inline-body comparisons,
and immediate-consumer review. Its
[reproduction boundary](../../research/polca-departure-through-luke-recruitment.md#reproduction-and-acceptance-limits)
owns original-source checks. This synthesis reviews prose, contracts and bounded tracked-fixture
selections, without fresh source investigation, payload generation or runtime acquisition.

| Reviewed boundary | Existing evidence surface | Disposition |
| --- | --- | --- |
| Request/admission | [sf2-map-event-request-state-static-v1](../../../tests/fixtures/h2/map-event-request-state-static-v1.json), [sf2-map-event-request-consumption-static-v1](../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json), [sf2-battle-routing-data-static-v1](../../../tests/fixtures/h2/battle-routing-data-static-v1.json). | Writer/consumer and routing facts do not prove natural walking. Research owns four selected warp rows and three actual Map72 admission rows. |
| Fresh population/init | [sf2-map-setup-static-v1](../../../tests/fixtures/h2/map-setup-static-v1.json), [sf2-map-entities-static-v1](../../../tests/fixtures/h2/map-entities-static-v1.json), [sf2-map-init-static-v1](../../../tests/fixtures/h2/map-init-static-v1.json), body 78. | Six operations and both return targets confirm F716/F10 selection; summary population facts are not live mappings or a raft receipt. |
| Scene/lifecycle | [sf2-map-script-engine-static-v1](../../../tests/fixtures/h2/map-script-engine-static-v1.json), population, placement, action, lifecycle, force and story families; [map-script verifier](../../../src/sf2tool/h2/map_script_engine.py). | Command-family coverage is not complete per-scene runtime coverage. Research owns all 29 records, six cursor texts, join446, allocation/HP and immediate consumers. |
| Joined/active/cache order | [membership contract](../contracts/party-membership-state.md#joinforce-chronology), [party/roster](../contracts/party-roster-state.md). | Rebuild before conditional activation agrees; numeric Luke flags/capacity are scene-owner facts, not new guarantees of player control or persistence. |

The [init schema](../../../schemas/h2-map-init-static-fixture.schema.json),
[init verifier](../../../src/sf2tool/h2/map_init.py) and
[map-script schema composition](../../../src/sf2tool/h2/map_script_schema.py) bound fixture shapes.
Existing narrow routes include `uv run sf2 h2 map-init`, `map-script-engine`, `map-setup`,
`map-entities`, `battle-routing-data` and `map-event-request-consumption`; focused checks include
`tests/python/test_map_init.py` and `test_map_script_engine.py`. They are evidence navigation,
not newly executed documentation gates. No evidence conflict requiring an owner correction was found.

**Unknown:** natural Y30/exit selection and earlier choices; actual battle, follower, raft,
membership, deployment, HP, AI and mappings; concurrent positions, action/service completion,
rendered text/sprites/camera/audio/fades, responsive input and durable save/load behavior.
These limits do not block the static guide or authorize automatic acquisition. The
[accepted research handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/814#issuecomment-6159534926)
preserves successful final CI and resolved inspection, quoting, path-check and propagation failures,
with earlier linked results. No unresolved comparison failure is hidden or ritually replayed.
This guide claims neither continuous playability nor campaign/epic completion.
