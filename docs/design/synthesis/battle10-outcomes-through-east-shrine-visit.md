# Battle10 Outcomes through the Optional East Shrine Visit

Battle10 victory returns the party to the overworld at a battle-derived position. East Shrine is a
separate optional visit: its residents mistake the visitors for Petro, the grandfather explains a
stolen ancient stone, and a later conversation offers help reaching Grans. That invitation changes
current following and later population selection through different mechanisms.

**Confirmed static content and control flow:** the accepted
[research owner](../../research/battle10-outcomes-through-east-shrine-visit.md) supports the outcomes,
selected Map70→62 connection, callers and state changes below. **Inferred:** this branch develops the
Sky Orb/Grans story after the eastward encounter. **Unknown:** natural traversal, actual actors and
mappings, service completion and continuous playability. The visit is not an established mandatory
route to Battle11 or a remake readiness claim.

Begin with [Battle10 admission](ribble-may-through-battle10-admission.md). Stop after the final
old-man caller and the first conditional restoration of exploration control. The south exit is only
a continuation boundary; [the selected return through Battle11 admission](east-shrine-return-through-battle11-admission.md)
has its own guide. Onward travel, the cave/Battle11, wooden panel, Sky Orb acquisition and the later
shrine scene are outside this guide. Fnnn means flag nnn; map/battle IDs retain source numbering.

## 1. Battle Outcomes Return through Different Destinations

**Confirmed static outcomes:** Battle10 has no enemy-leader shortcut. Victory needs at least one
counted ally and no counted enemies; Bowie's zero HP forces defeat, and simultaneous exhaustion takes
the defeat branch. Killing enemy slot 128 alone neither wins nor removes the other enemies. The
row-10 defeated and after-battle scripts are both `ms_Empty`; an empty script still executes its
terminator's conditional scrolling wait and speed clear.

| Outcome | Consequence at return |
| --- | --- |
| Victory | Heal living/immortal allies, select the first battle-party member after `UpdateForce`, and derive the return coordinates/facing from that member. The Map70 battle-area offset is `(0,0)`, so these are that member's combatant coordinates. Clear F410 and set F510. |
| Defeat | Present exhaustion text, restore Bowie's HP, halve gold by unsigned shift, and return the carried egress map's savepoint tuple. F510 is not newly set. |
| Egress / Angel Wing | Consume the spell MP or item and perform its services, then return the egress tuple without the defeat penalty or victory-completion write. |

Defeat/egress clear F410 only when F510 was already set; first-battle retreat retains the supplied
unlock. The carried egress map, including the savepoint lookup's unmatched-map fallback, determines
the destination. No last savepoint or shrine destination is established here. Victory healing does
not resurrect dead non-immortal allies.

The after-battle shared tail still invokes `JoinForce` for row 10's zero/Bowie entry, even if F510
skips the script. It sets F0 and conditionally F32 through the ordinary join/capacity consumer; it does
not recruit May or the grandfather. Position selection precedes that tail.

On immediate victory return, main switches maps and enters exploration without a second `CheckBattle`.
Map70 has no switched-map replacement, an empty fixed population and an `rts` init. Ordinary supplied
entry rebuilds entities and flag followers, clears temporary flags and pending, then loads/initializes
the map. Pending zero after services permits control restoration. Carried F414 does not intercept
this immediate return.

Later Map70 zone requests remain separate. The [admission guide](ribble-may-through-battle10-admission.md#6-a-zone-request-and-the-battle-selector-decide-what-starts)
retains the specific Battle14 helper, the default Battle10 request and ordered selector. F510 suppresses
the default request without clearing a supplied F410; F410/F510 both set can still select Battle10 on
that later call. These rules do not establish uninterrupted walking from the victory position.

## 2. Entering East Shrine Is a Separate Optional Connection

**Confirmed conditional connection:** the selected Map70 warp is `(43,40)`, no scroll, to East Shrine
(Map62), `(6,18)`, Up. Movement must encounter the warp layout attribute and matching row, produce the
complete request, and reach its consumer with that request intact. Coordinates alone do not prove a
navigable path. The selected warp and Map62 admission test neither Battle10 victory nor May
recruitment, Peter's presence or shrine-story flags. Optionality follows from these predicates.

Map62 has no battle-coordinate row or flag-switched substitution. Ordinary entry rebuilds its
population, resets temporary flags 256–383 and runs an `rts` init. Entry itself starts no visit scene.
If the services return with pending zero, exploration can first restore control at `(6,18)`, Up;
responsive input at that point has not been observed.

With F77 clear, the fixed population declares the young man as event identity 128 at `(6,11)`, Up,
and the grandfather as 129 at `(5,6)`, Down. These are authored origins and lookup identities, not
observed positions or physical slots. Flag followers are declared first and shift the physical NPC
slots. F67 can declare Peter as character 7 even when he is dead; dialogue requests alone do not
establish a living or present speaker.

The two NPC event rows turn the activated actor toward the player and restore its prior facing at
return. Their `DOWN=3` control byte is not an approach-direction requirement. Ordinary activation
requires a current actor/mapping and excludes the controlled entity and current followers.

## 3. Greeting, Explanation and Invitation Have Independent Gates

**Confirmed static content:** the greeting asks about Petro and mistakes the arrivals for him. The
old man demands the Sky Orb, moves toward the visitors and then recognizes the mistake. His first
conversation explains the family's ancient stone, its theft by his grandson and the grandson's
journey to Grans to operate an ancient vehicle. A separate zone scene offers help and invites the
party to talk. The next old-man conversation explains a usable tunnel to Grans; Peter asks about
reaching Grans, the old man invites the party, narration says the leader decides to accompany him,
and the old man asks for help recovering the Orb.

Those descriptions paraphrase the accepted text requests; their order as story beats does not make
one mandatory interaction sequence. The complete local predicates are:

| Trigger | Gate and result |
| --- | --- |
| Zone `(5,15)` or `(6,15)` | F729 clear and F256 clear: run greeting/mistaken identity `cs_5C876`, then caller sets F256. Otherwise return. |
| Zone `(5,16)` or `(6,16)` | F729 clear, F256 set and F257 clear: run help/conversation invitation `cs_5C8FE`, then caller sets F257. Otherwise return. |
| Old man 129, F729 clear / F728 clear | Run the grandfather's account `cs_5C8D4`, then caller sets F728. |
| Old man 129, F729 clear / F728 set | Run the travel invitation `cs_5C914`; after its follow attempt and F77 write, caller sets F729. |
| Old man 129, F729 set | Request text 2512, saying this route cannot reach Grans, then return. Natural activation after following is a separate question. |

Neither old-man conversation tests the zone flags, F510, F67 or F77. If physical activation is
possible, talking twice can reach the final conversation without either zone scene. Once F256 is set,
the second zone may run before the first talk because it does not test F728. Crossing Y16 with F256
clear does nothing; crossing later can request the invitation without any prior attempt to leave.

There is no yes/no command or decline branch in these four programs. They write no warp, join,
force-AI, population reload or map-block change. Invitation wording is not evidence of a player choice,
and describing a tunnel is not evidence that the party travels through it here.

The greeting requests the old man's total Down 6 movement; his first account requests Up 6. These
are relative actions with waits, not absolute position resets. Talking before the greeting uses the
actor's actual position. Camera and text services also wait; no explicit sleep record does not mean
no waiting (`nextSingleText` has a shared sleep service). Source order proves requests, not collision-free
completion, timing, rendered text or responsive input. Text IDs describe the ordinary non-skipped path.

## 4. Revisiting Retains Conversation State but Resets Local Encounters

**Confirmed state lifetimes:** F728, F729 and F77 are independent persistent game flags. F256, F257 and
F258 are entry-local temporary flags cleared by ordinary supplied-map entry. With F729 clear, reentry
can permit both zone scenes again even if F728 survives; the next old-man talk then uses F728. F729 set
suppresses both zones. F77 selects population, rather than gating either conversation or zone.

F77-set setup retains only young man 128, at authored `(7,5)`, Down. Setting F77 during a conversation
does not reload the current population. Conversely, a supplied F77-set/F729-clear entry can select a
zone script without a 129 mapping; it is not safely skipped or established as a natural state. No
implication among these three persistent flags is invented.

The young man supplies bounded context. F732 takes priority and selects a later lament about the
unavailable trip; this guide neither admits its producer nor proves that trip. Otherwise F258 clear
requests his account of the grandfather's Sky Orb study, ancestral flight and the Nazca Ship's last
known location. Only after those texts does F729 set cause F258 to be set. Thus F729 clear repeats
that account, while F729 set permits it once per ordinary entry before the F258-selected farewell
and message for his brother. A supplied F258 set selects the farewell even with F729 clear. The
exposition and later alternative are dialogue context, not acquisition or travel evidence.

## 5. Current Following and the Later F77 Follower Are Different

**Confirmed current command:** the final conversation requests `followEntity 129,FOLLOWER_B,2`, then
sets F77. `FOLLOWER_B=31` is a lookup identity distinct from Peter's 7 and `FOLLOWER_A=30`. The
three word operands are `$0081,$001F,$0002`. Before resolving either entity, the handler probes the
first word's high byte, 0, so its HP guard checks Bowie here. It does not validate the old man, Peter,
the target, mapping freshness or capacity. Bowie HP zero skips the follow operation; the subsequent
F77 and caller F729 writes still occur if execution otherwise completes.

When the guard permits execution, the target comes from the actual character-31 lookup. F67 alone
maps Peter as 7 and supplies no 31 mapping: its cleared byte resolves physical slot 0/main, a
zero-mapping consequence rather than an explicit Bowie fallback or a Peter synonym. F74 can map
Astral as 31, F68 can overwrite it with Oddler, and F70 can overwrite it with Astral, in that source
order. Other followers or transient May do not substitute the current list tail for 31. The
[research's mapping table](../../research/battle10-outcomes-through-east-shrine-visit.md#current-following-versus-the-f77-rebuild)
owns the full supplied-state controls.

`AddFollower` replaces the resolved 129 actor's action block with a looping follow relation, patches
the physical target and `(-24,0)` offsets, then appends its physical index unless already listed.
Replacement precedes the duplicate check. Unlike May's `addNewFollower`, this command does not choose
the list tail. It does not wait for visible following to finish or guarantee spacing. An absent 129
also resolves slot 0; forced missing mappings can therefore alter main, even to follow itself.

On successful ordinary-population following, the old man remains identity 129 in the current map but
is excluded from ordinary interaction as a current follower. The F729-repeat text is not proof that
he can immediately be spoken to again. The script's F77 and caller's F729 are separate results from
that current action/list mutation.

Later ordinary entry clears mappings and walking allocation and rebuilds the follower list. F77
declares the old-man sprite under character **30**, while fixed population no longer declares 129.
F77 alone gives the old man physical slot 1 following main; F67 plus F77 gives Peter slot 1 and old man
slot 2 following Peter through the ordinal-selected follower action. Other flags change this ordering;
no arbitrary-count target/capacity guarantee follows. A later F83 king row can overwrite character 30's
lookup. Character 30 has no explicit Map62 event row and is a follower, not preserved event 129.

F77/F728/F729 are in the game-flag save domain; the transient follower list, walking pointer and action
storage are outside the logical save span. That distinction does not prove natural save/reentry
presentation or physical cross-process durability. These remain **Unknown**.

## 6. The Final Caller Hands Back Conditional Exploration Control

**Confirmed static endpoint:** after the final script returns, the old-man caller sets F729. The
entity-event dispatcher restores prior facing, closes windows and reenables updates. At a pending-zero
`WaitForEvent` read, exploration restores camera target physical 0 and the controlled entity's action
script. This is the first conditional control handoff, not a wait for the asynchronous follower loop.
Zone callers have a different tail with a VInt/physical-main stop wait; neither tail proves every
service/actor finished or discards a newly produced pending request.

The south exit row is wildcard X, Y19, no scroll, to Map70 `(43,41)`, Down, without an F77/F728/F729
gate. It still requires the layout/request path. This is a continuation boundary only: no onward
admission, Grans travel, Orb acquisition or Battle11 is claimed here. Continue with the separate
[return and cave-admission guide](east-shrine-return-through-battle11-admission.md) for its selected connection.

## Evidence Review and Remaining Limits

This guide consumes research accepted in PR #766 and its predecessor guide accepted in PR #765.
Research binds USA retail to pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6` and owns exact
source/text composition and reproduction. Direct review of the following tracked selections and
schema/consumer boundaries found no required Layer A correction. Optional derived payloads were
not needed; no extraction, upstream re-investigation or fresh runtime observation is claimed.

| Reviewed relation | Executable/contract boundary and limit |
| --- | --- |
| Outcomes and wrappers | [sf2-battle-global-data-static-v1](../../../tests/fixtures/h2/battle-global-data-static-v1.json), [sf2-battle-routing-data-static-v1](../../../tests/fixtures/h2/battle-routing-data-static-v1.json), [sf2-battle-cutscenes-static-v1](../../../tests/fixtures/h2/battle-cutscenes-static-v1.json) and [cutscene contract](../contracts/battle-cutscene-routing.md). Routing has `routeTargetsParsed=false`; exact row-10 targets and return composition come from accepted research, not summary counts. |
| Entry and population | [sf2-map-data-static-v1](../../../tests/fixtures/h2/map-data-static-v1.json), [sf2-map-setup-static-v1](../../../tests/fixtures/h2/map-setup-static-v1.json), [sf2-map-entities-static-v1](../../../tests/fixtures/h2/map-entities-static-v1.json), [sf2-map-init-static-v1](../../../tests/fixtures/h2/map-init-static-v1.json) body 71 and [map/exploration contract](../contracts/map-exploration.md). Selectors, population declarations and `rts` do not establish natural actors or a traversable warp. |
| Conversation/zone predicates | [sf2-map-events-static-v1](../../../tests/fixtures/h2/map-events-static-v1.json), [entity shard](../../../tests/fixtures/h2/map-events/entity-programs.json) orders 610–612 and [zone shard](../../../tests/fixtures/h2/map-events/zone-programs.json) orders 112–114. Ordered operations/raw operands confirm independent gates and caller writes; the four complete short-script/text sequences remain with research. |
| Follow guard and lifetimes | [sf2-map-script-engine-static-v1](../../../tests/fixtures/h2/map-script-engine-static-v1.json), `entityGestureRelationshipMotionCommandFacts`, and [sf2-map-entity-gesture-relationship-motion-runtime-v1](../../../tests/fixtures/h3/map-entity-gesture-relationship-motion-v1.json), cases `follow-selector-zero-live`, `follow-selector-seven-live`, `follow-zero-current-hp`; [party/roster contract](../contracts/party-roster-state.md) distinguishes May's separate consumer. The handler guard and controlled high-byte HP observations do not runtime-close this shrine composition. |

The [event fixture loader](../../../src/sf2tool/h2/map_events_fixture.py) and
[verifier](../../../src/sf2tool/h2/map_events.py) retain ordered shards/raw operands;
[map-init schema](../../../schemas/h2-map-init-static-fixture.schema.json) and
[verifier](../../../src/sf2tool/h2/map_init.py) retain bodies. The
[map-script schema composition](../../../src/sf2tool/h2/map_script_schema.py),
[verifier](../../../src/sf2tool/h2/map_script_engine.py),
[relationship fixture schema](../../../schemas/h3-map-entity-gesture-relationship-motion-fixture.schema.json)
and [observer](../../../src/sf2tool/h3/map_entity_gesture_relationship_motion.py) preserve handler and
controlled-case boundaries. The research's
[verification/reproduction boundary](../../research/battle10-outcomes-through-east-shrine-visit.md#final-caller-continuation-and-verification-boundary)
owns the narrow comparisons; no blanket suite is selected solely for this prose.

**Unknown:** natural route/collision, actual activated actors/mappings/HP/party/followers, completed
text/input/scroll/entity services, pending-request survival, visible following, arbitrary follower
capacity, save/reentry presentation and continuous playability. These limits do not create an
emulator-work queue. This guide selects no engine/product choice and does not complete the campaign.
