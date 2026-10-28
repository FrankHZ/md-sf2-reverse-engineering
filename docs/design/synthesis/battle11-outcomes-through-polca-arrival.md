# Battle11 Outcomes through Polca's First Oddler Arrival

After the cave encounter, victory returns the party to the cave rather than transporting it to
Polca. A separate cave exit leads onto the Bedoe Kingdom overworld, and a selected entrance leads
into Polca. A later town zone stages an injured boy's arrival: a beastman checks him, calls the
mayor and asks for treatment. The scene ends by rebuilding the residents before its caller finishes
the town-state change and conditionally restores exploration control.

**Confirmed static content and control flow:** the accepted
[research owner](../../research/battle11-outcomes-through-polca-arrival.md) supports the outcomes,
connections, actors and first-event handoff below. **Inferred:** these form a candidate continuation
from the cave opening. **Unknown:** natural cave/town traversal, supplied story and actor state,
completed services and visible presentation. Table connectivity does not establish a mandatory or
playable sequence, and this guide adds no remake readiness claim.

Begin with [East Shrine return through Battle11 cave admission](east-shrine-return-through-battle11-admission.md).
Stop after the first Oddler scene's caller and its first conditional restored-control boundary.
Continue with [Polca's fairy interaction through Battle12 admission](polca-fairy-through-battle12-admission.md)
for the optional turtle encounter, independent east departure and searching-devils handoff.
The turtle interaction `cs_58512`, F710-set later scene, mayor/raft/recruitment content, Bedoe and
Battle12/13 are outside this guide. Fnnn means flag nnn; maps and battles retain source numbering.

## Returning from the Cave Battle

**Confirmed outcomes:** Battle11 has no leader-victory shortcut. The loop counts surviving allies
before enemies, using nonnegative battle X and nonzero **word** HP; Bowie's zero HP forces an ally
count of zero. Defeating enemy 128 alone does not defeat the remaining enemies. Zero enemies with
allies remaining wins; simultaneous exhaustion loses.

The battle-end and after-battle scene rows both select `ms_Empty`. The end wrapper requires Bowie
alive, enemy128 dead and F511 clear; the after wrapper runs its script only with F511 clear. Empty
scripts still perform their terminator's conditional scroll wait and speed reset. There is no new
story scene here that carries the party across the cave or into town.

| Outcome | Return and persistent consequence |
| --- | --- |
| Victory | Heal living/immortal allies, update the force and select its first battle-party member. Seed Map48 with that member's battle position plus area origin `(0,0)` and mapped facing; then run the after wrapper, clear F411 and set F511. |
| Defeat | Update the unlock, request exhaustion text, restore Bowie's HP and halve gold by unsigned right shift. Return through the carried egress map's savepoint lookup without newly setting F511. |
| Egress or Angel Wing | Consume spell MP or the item, perform text/flash services and update the unlock. Return through the carried egress tuple without the defeat gold penalty or victory completion. |

Victory does not prescribe the admission tuple `(15,26)` or the selected exit below. Its reference
member need not be Bowie. Healing does not resurrect dead non-immortal allies. The shared after-battle
join tail runs even when F511 skips the after script: its zero/Bowie row sets F0, updates force and
conditionally adds Bowie through F32 if capacity permits. It recruits neither Oddler nor Gerhalt
nor Peter, and does not change the already selected return member.

For loss/escape, `UpdateBattleUnlockedFlag` clears F411 only if F511 was already set; a first-battle
loss/egress retains the supplied unlock. Battle11 has no special egress override. The carried
`EGRESS_MAP` selects a savepoint, including the unmatched-map `(1,1)`, Up fallback; no fixed defeat
destination or last savepoint is established here.

The immediate battle return switches maps and enters exploration without another `CheckBattle`.
Victory therefore does not immediately re-admit Battle11 or Battle25. Ordinary return loading
clears pending, heals living/immortal allies, rebuilds entities/followers and clears temporary
flags 256–383. Map48's [absent setup/init](east-shrine-return-through-battle11-admission.md#cave-arrival-and-battle-admission-are-separate)
supplies no fixed residents. Pending zero after loading services permits control; it does not
establish an accessible path to an exit.

## Leaving the Cave and Entering Polca

**Confirmed selected connections:** these two fixed, non-scrolling requests are separate from
the battle return and from one another:

| Selected source point | Requested destination |
| --- | --- |
| Map48 `(3,12)` | Map72 `(56,46)`, Down |
| Map72 `(43,46)` | Map13 `(15,30)`, Up — Polca |

Map13 is Polca; `(15,30)` is an arrival coordinate, not Map15. The return-side Map72 point
`(56,45)` instead requests Map48 `(3,11)`, which is neither cave battle trigger. Other cave exits
do not substitute for this selected connection. Neither selected row tests F511, shrine completion,
Peter membership or Oddler flags.

Producing a warp requires the layout's `$1000` attribute and a matching row. The exploration
consumer idles main, handles the sound/return services and supplies the mutable map/X/Y/facing
request to map switching and battle selection. A row alone proves neither collision-free walking
nor that its parameters survive to consumption.

Map72 retains the accepted [interruption limits](../../research/east-shrine-return-through-battle11-admission.md#interruptions-before-the-cave-request):
ordered Battle8/24/26 unlocks can intercept ordinary entry; completion can clear a selected unlock
without declining that same call. Its init can invoke later content on supplied F716-set/F10-clear,
and its zones can request encounters or later story content. Arriving at `(56,46)` does not prove
these branches absent. This guide reuses that review without extending its scene scope.

Polca has a separate battle discriminator. Supplied F413 selects Battle13 at any requested point,
including `(15,30)`. If F513 is also set, the selector clears F413 and still selects Battle13 on
that call. F413 clear permits ordinary town entry regardless of F513. Battle13's area is output,
not a containment predicate; F511 completion establishes neither F413 nor F513. The town content
below describes the ordinary-entry branch, without explaining Battle13.

## The Town Population before the Incident

**Confirmed setup selection:** ordinary Polca entry rebuilds entities, performs shared map-loading
services and runs its init. Setup variants are scanned in order: default, F710, F799, F513. The
last set flag wins, giving F513 > F799 > F710 > default. All share the zone and init handlers;
F513 also substitutes the entity-event table. The filenames ending `_709` contain F710 symbols;
they do not add an F709 condition.

The default and F710 lists supply Gerhalt as ally 9 plus thirteen regular NPC rows. Gerhalt uses
the ally lookup and does not consume ordinary NPC-event numbering. Regular NPC IDs begin at 128,
independently of physical slots and any preceding followers:

| Lookup identity | Role in this bounded content |
| --- | --- |
| Ally 9, Gerhalt | Declared at `(23,13)`, Down, unless already mapped. His presence is not a recruitment event. |
| 128 and 129 | Male and female centaurs, the first two regular NPC rows. |
| 130 | Kappa/turtle at `(1,11)`, Down, using `eas_InitSlow`; not the female centaur. |
| 133 | Beastman at `(23,6)`, Right, who checks the boy and calls for help. |
| 137 | Man at `(15,3)`, Down, addressed as mayor in the scene. |
| 140 | Oddler sprite initially at `(63,63)`, Down; the scene explicitly places him for his arrival. |

There is no fixed Peter row in Polca. F67 can rebuild Peter7 independently of joined/active flags,
word HP or force-AI state. A dead ally outside battle can receive the shared blue-flame sprite
alternative; an actor or sprite proves neither living status nor membership. With no Peter mapping,
named script operations can resolve physical main slot 0 instead of skipping safely.

Supplied alternative states matter even when F710 is clear. F799 selects a shorter population:
130 is then a minotaur, and 133/137/140 have no fixed rows, despite shared handlers. F513 supplies
another population and entity-event table. Missing mappings can resolve slot 0. The first-event
test does not exclude these states, so it cannot prove a sensible cast under every supplied tuple.

Init makes three independent, ordered choices. F711 clear runs `cs_58310` on 130; F716 set faces
137 Down; F718 set moves 140 out of map. The short 130 script requests Down facing, installs and
waits for actions that turn motion off, write Right orientation and update the sprite, then resets
animation. **Inferred:** this stages the turtle's pose. **Unknown:** its visible appearance.
F711-set init alone does not hide 130. Moving an actor out of map writes off-map coordinates
rather than deleting it; the selected incident can later reposition 140 even with F718 supplied.
None of this is the excluded turtle interaction.

## The Injured Boy and the Villagers' Response

**Confirmed event selection:** town entry at `(15,30)` does not automatically run the incident.
A later zone request needs `$1400` and selects the first matching row. X24/wildcard Y precedes
the Y11, Y30 and default rows, so it wins even at `(24,11)` or `(24,30)`; a returning handler does
not fall through. No continuous walk to an eligible X24 tile has been established.

The first branch of `Map13_ZoneEvent0` tests F710 alone. Clear runs `cs_58330`; supplied
F711/F716/F718 do not skip it. F710 set reaches a separate later-scene discriminator outside this
guide. The first scene contains eleven text requests, IDs 1452–1462 (`05AC..05B6`): five beastman,
five mayor and one narrator request. Their ordered content and staging are:

1. A rumble sound, quake amplitude 6, white-flash request 90 and quake reset introduce the arrival.
   Bowie, the beastman and Peter receive oscillation actions with intervening sleeps. Oddler140
   is placed at `(29,7)`, Left, moves left at changing speeds, then receives a short down-motion
   request replaced by a stop action. Motion-off/Left-orientation staging follows; the beastman
   turns toward him.
2. The beastman reacts, moves toward the boy and faces him. The narrator describes the examination,
   and the beastman reports injury. Further facing/movement and two jump requests lead to his
   call for the mayor. These are the first four texts, 1452–1455.
3. The mayor is placed at `(17,7)`, Right. The camera destination is requested at `(13,3)`;
   Bowie and Peter are both requested at `(25,9)`, Up, and tracking switches to the mayor.
   The mayor moves right/up, asks what happened, hears the beastman's report of an injured boy
   from the mountains and reacts (1456–1458). He then moves down/right to approach the boy.
4. The mayor comments on the boy's injured eyes and attributes the injury to an eruption
   (1459–1460). The beastman approaches, asks for treatment, and the mayor orders the boy brought
   inside (1461–1462). The beastman receives a nod request. Text conveys the villagers' account;
   it is not an independently observed medical or eruption event.
5. The mayor, beastman and boy receive short movement requests interrupted by stop scripts.
   Tracking becomes neutral; the mayor and boy start moving Left9 while the beastman's Left9
   action is waited on. The boy and Bowie receive initialization actions, final facings are
   requested, Peter is instructed to follow Bowie, and the F710 population is explicitly reloaded.

These are paraphrases, not a transcript. The scene has no choice, join, warp, battle-completion test
or story-flag write. The mayor's first eye-injury text uses `nextText`; the other ten requests use
`nextSingleText`. Full ordered records, inline actions and text provenance remain with the
[scene research](../../research/battle11-outcomes-through-polca-arrival.md#first-x24-event-and-scene-records).

## Requests, Following and the Rebuilt Town

**Confirmed service boundaries:** quake commands write amplitude; the flash argument 90 leads to
twelve fading-service launches and palette duplication, not proof of 90 visible frames. The fourteen
explicit sleeps total 392 requested frames, not elapsed scene duration. Text, scrolling, fades and
action waits add services, while debug skipping can omit text/sleeps. Oscillation installation and
short sleeps followed by stop-script replacement do not certify completed tile movement.

Position/facing commands skip an ally only when its **word** HP is zero; HP256 is nonzero. Thus the
Bowie/Peter `(25,9)` requests are individually conditional. NPCs bypass that guard, and action
installation has no equivalent HP test. Camera destination waits for scrolling, while entity
targeting alone does not. Text services wait for scrolling; the terminator conditionally waits for
scrolling and clears its speed rather than waiting for every actor. Actual mappings, motion, camera
arrival, audible/visible output and service completion remain **Unknown**.

The Peter→Bowie `followEntity` request encodes words `$0007,$0000,$0002`. Its handler checks the
first word's high byte, so it tests **Bowie0's** word HP, not Peter7's. With Bowie alive and valid
mappings, selector 2 installs Peter following Bowie at offset `(-24,0)`, independently of Peter's
HP. Missing Peter can resolve main for both operands. This writes neither F67 nor joined/active
state, and does not wait for visible following.

The next command explicitly loads `ms_map13_flag710_Entities` while the caller's F710 is still
clear. It samples main's current coordinates/facing, clears entity/action state and mappings,
resets walking allocation and rebuilds flag followers plus that list. It does not query setup
flags, call map init, heal/reset temporary flags or run the separate mapsprite-load service.
Main is rebuilt idle at its sampled location, not necessarily the requested `(25,9)`.

Current transient following is therefore replaced by a rebuild. With only F67 among follower flags,
Peter is recreated in physical 1 using `eas_Follower1`, with the same Bowie target and `(-24,0)`
offset as the successful preceding request. Rebuilt does not necessarily mean visibly different
following. Other enabled followers change physical order/action selection; with F67 clear there is
no fixed Peter row to recreate him. Supplied F68 can independently build an Oddler-sprite follower
under 31, separate from resident 140; this scene does not set it.

The explicit resident list restores beastman133 to `(16,4)`, mayor137 to `(15,3)` and Oddler140 to
`(14,3)`, irrespective of their earlier movement endpoints. The other default/F710 declarations
are unchanged. The boy is now placed in the town's restored population, without recruitment.

**Confirmed caller order:** only after the scene and reload return does the caller handle F711.
Clear reruns `cs_58310` on rebuilt 130 to stage its pose; set moves that rebuilt actor out of map.
Then it sets F710 and returns. It writes no F711/F716/F718/F68 and adds no join or pending warp.
A later setup lookup still honors higher-priority F799/F513; the F710 write does not reload again
and need not make the current explicit population agree with that later selection.

The zone dispatcher closes portrait/text, waits a VInt and waits for physical main to stop.
Only pending zero lets `WaitForEvent` restore camera target 0 and the controlled-entity action.
This is the first conditional restored-control boundary and this guide's endpoint. It does not
certify all walking/follower actions, rendered town state or usable physical input.

## Evidence Review and Remaining Limits

This guide consumes research accepted in [PR #774](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/774)
and the preceding English guide from PR #773. Research owns the pinned SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6`, USA retail identity, exact row/text/population
composition and reproduction. Bounded review of accepted prose, tracked fixtures, contracts and
schema/verifier boundaries found no Layer A correction needed. No optional payload was generated
or copied, no source investigation repeated and no runtime observation performed.

| Reviewed relation | Existing evidence and discriminating boundary |
| --- | --- |
| Outcomes and return | [sf2-battle-global-data-static-v1](../../../tests/fixtures/h2/battle-global-data-static-v1.json), [sf2-battle-routing-data-static-v1](../../../tests/fixtures/h2/battle-routing-data-static-v1.json), [sf2-battle-cutscenes-static-v1](../../../tests/fixtures/h2/battle-cutscenes-static-v1.json) and [battle lifecycle contract](../contracts/battle-control-lifecycle.md). Aggregate routing retains `routeTargetsParsed=false`; exact row11 leader/join/empty targets and loop/egress composition come from accepted research. |
| Requests, setups and cast | [sf2-map-data-static-v1](../../../tests/fixtures/h2/map-data-static-v1.json), [sf2-map-setup-static-v1](../../../tests/fixtures/h2/map-setup-static-v1.json), [sf2-map-entities-static-v1](../../../tests/fixtures/h2/map-entities-static-v1.json) and [map exploration contract](../contracts/map-exploration.md). Inventories/consumer facts do not themselves prove the exact selected warp rows, resident identities or natural travel; research supplies those joins. |
| Init and first caller | [sf2-map-init-static-v1](../../../tests/fixtures/h2/map-init-static-v1.json), body18/embedded `cs_58310`, and [sf2-map-events-static-v1:zone-programs](../../../tests/fixtures/h2/map-events/zone-programs.json), program9 `Map13_ZoneEvent0`. Ordered operations preserve independent init flags and script→F711 handling→F710 write; later branches stay excluded. |
| Scene commands and population replacement | [sf2-map-script-engine-static-v1](../../../tests/fixtures/h2/map-script-engine-static-v1.json), Polca placement/relationship/population sites and handler facts; [party/roster contract](../contracts/party-roster-state.md). Family-site indexes are zero-based, unlike research's one-based scene records; they are not a complete public scene transcript. |
| Following guard | [sf2-map-entity-gesture-relationship-motion-runtime-v1](../../../tests/fixtures/h3/map-entity-gesture-relationship-motion-v1.json) and [relationship research](../../research/common-scripting.md#confirmed-map-script-entity-gesturerelationshipmotion-command-family). Controlled handler evidence supports the high-byte HP seam, not this composed Polca event or visible following. |

The existing [map-init verifier](../../../src/sf2tool/h2/map_init.py) and
[schema](../../../schemas/h2-map-init-static-fixture.schema.json) preserve body/operation order.
The [event fixture loader](../../../src/sf2tool/h2/map_events_fixture.py) and
[shard schema](../../../schemas/h2/map-events-fixture-shard.schema.json) preserve the selected caller.
The [script schema composition](../../../src/sf2tool/h2/map_script_schema.py) and
[verifier](../../../src/sf2tool/h2/map_script_engine.py) preserve operand, handler and family-site
limits. Tracked H2 fixtures were validated in memory against their owning schemas. Narrow owners
include `uv run sf2 h2 map-data`, `map-setup`, `map-entities`, `map-init`, `map-events`,
`map-script-engine`, `battle-global-data`, `battle-routing-data` and `battle-cutscenes`; these names
are traceability, not claims of fresh extraction. Research's
[reproduction boundary](../../research/battle11-outcomes-through-polca-arrival.md#caller-handoff-and-evidence-boundary)
owns the retained comparisons. Prose-only work selects direct checks and the committed planner/CI.

**Unknown:** natural collision-free cave/town traversal and interruption-free travel; actual
flags, actors, HP, party/AI and mappings; request survival, pending and walking/text/camera/fade
completion; quake/flash/sprite appearance and timing; and continuous playability. These limits
authorize neither acquisition nor new machinery. The guide makes no engine/product choice and
does not complete the campaign or the wider design epic.
