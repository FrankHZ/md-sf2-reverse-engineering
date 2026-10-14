# Battle13 Outcomes through Polca's Raft Grant

After the Polca battle, Luke proposes asking the mayor for a raft. On a fresh victory return,
Polca has a different population, and the mayor's optional conversation can lead to a bargain:
Luke will keep quiet about the mayor's conduct if he supplies a raft for Bowie. The conversation
ends with Peter and Luke following requests, then the caller records a raft for a different map.

**Confirmed static content and control flow:** the accepted
[research owner](../../research/battle13-outcomes-through-polca-raft-grant.md) supports the outcome,
cast, attributed texts, services and ordered state writes below. **Inferred:** their staging
depicts Luke accompanying Bowie and extracting the raft promise from an apologetic mayor.
**Unknown:** natural battle/contact reach, supplied actor/state validity, service completion,
presentation and river access. This explanation adds no remake capability or product decision.
Fnnn means flag nnn; coordinates are map tiles unless a different unit is named.

Read [Bedoe return through Battle13 admission](bedoe-return-through-battle13-admission.md) for
the preceding confrontation, Gerhalt joining and battle handoff. This guide stops after the
selected mayor caller and conditional control return. The Y11/Y30 Oddler choices, departure,
river travel, boarding and Battle14 remain outside its boundary.

## Battle Outcome and Where It Returns

**Confirmed outcome selection:** Battle13 is `BATTLE_POLCA_VILLAGE`, with a nonzero
leader-present row, an empty defeated script `ms_Empty` and aftermath `abcs_battle13`.
After an individual action, the loop calls the defeated wrapper, processes deaths and counts
remaining combatants. The wrapper requires Bowie to have nonzero **word HP** and enemy128
to have zero word HP. Failure of either test skips both script and leader tail.

F513-clear runs the empty defeated script; F513-set skips it but still reaches the leader tail.
That tail appends living enemies128–159 to the death list, regardless of their placed X; it
does not itself zero every HP value. Completed death processing moves killed combatants and
mapped actors off-map, clears status and updates stats. The count requires nonnegative X and
nonzero word HP. Allies are tested before enemies, and a dead Bowie forces zero allies, so
simultaneous exhaustion loses. Victory still requires the admitted state and completed services;
an empty script alone proves no win. The after-turn-effects death/count path has no additional
defeated-wrapper call at that point.

| Outcome | Return and state consequence |
| --- | --- |
| Victory | Heal living/immortal allies, update force and select the first battle-party member. Seed return map from `CURRENT_MAP`, position from that member plus battle-area origin, and facing from its mapped actor; this need not be Bowie's position. Run the after wrapper, clear F413, set F513, read the return tuple and return D4=1. |
| Defeat | Update battle unlock, request exhaustion text363, restore Bowie's HP, halve gold by unsigned right shift, resolve Egress and return D4=-1. No new completion F513 or aftermath is granted. |
| Egress / Angel Wing | Consume MP/item and perform the text/flash services, update battle unlock, unwind the action frame and resolve Egress with D4=0. This has neither defeat's gold penalty nor victory's aftermath/completion. |

Battle13's Egress explicitly selects **Map10 Bedoe `(27,35)`, Up**, rather than carried
`EGRESS_MAP`. F399 set on battle entry excludes the game-start bedroom fallback. If F64 is
already supplied, Bedoe's raft-reset row also restores Map72 `(43,48)`. That is a reset of
an existing unlocked raft, not a grant for losing. The unlock helper clears F413 only if F513
was already set; first-completion defeat/Egress can leave F413 available. No natural route
back from Bedoe to Polca is established here.

The after wrapper separately tests F513. Clear runs the full aftermath; set skips it, keeps
the variable victory seed and leaves F75 as supplied. Both reach the shared `j_JoinForce(0)`
tail: Bowie membership F0, force refresh and capacity-dependent active F32. It runs after return
member selection and does not join Luke. The aftermath does not test F715 or repair missing
confrontation history. A supplied F513-set/F75-clear state does not replay Luke's scene.

## Luke's Aftermath and Accompanying the Party

**Confirmed requested content:** `abcs_battle13` loads Polca and its special aftermath actors,
then requests all eight cursor texts2495–2502. The speaker attribution below is supported by
the script; it does not validate a current physical actor or turn narrated recovery into HP.
Luke's portrait operand is `$C0`, Peter's `$0`, and narrator uses `$FF,ENTITY_NONE`.

| Text | Speaker | Paraphrased content |
| --- | --- | --- |
| 2495 | Peter / 7 | Asks whether Luke is all right. |
| 2496 | Luke / 10 | Says he was knocked senseless. |
| 2497 | Luke | Asks whether Bowie defeated the devils. |
| 2498 | Luke | Thanks Bowie and praises his fighting. |
| 2499 | Peter | Praises Bowie and criticizes Volcanon's judgment. |
| 2500 | Luke | Expects Volcanon to learn from this. |
| 2501 | Luke | Proposes seeing the mayor for a raft. |
| 2502 | Narrator / `ENTITY_NONE` | Says Bowie takes Luke along. |

The scene fades out sound, requests `loadMapFadeIn Map13,11,8`, loads `ce_4AD96`, resets
Bowie's action and places Peter `(15,13)`, Right. The load operands `(11,8)` are camera
coordinates, not a player destination. `ce_4AD96` declares main `(16,12)`, Down and a fixed
Luke `(16,13)`, Down, with no fixed Peter. Map/entity load services replace the battle
population and mappings, but this special load neither runs ordinary town init nor clears
temporary flags. It is distinct from the fresh F513 town population described below.

Followers are allocated before fixed actors and main. With only F67, Peter is physical1,
Luke physical2 and main physical0. Without follower flags, fixed Luke is physical1 and
absent Peter's zero mapping resolves physical0. Supplied F75 instead declares follower Luke
first and skips his fixed row. Other supplied followers change allocation. Names in speech
and `setPos Peter` therefore do not guarantee a valid Peter or fixed physical slots.

Luke first receives a waited animation-off/Left-orientation/sprite-update/idle body. Town
music and fade-in precede Peter's question. Luke later receives waited `eas_Init` and a jump,
then a headshake before describing his condition. His nonwaited `eas_2xRightLeft` before the
question about the devils alternates facing and sprite updates with four 30-frame waits;
it is not a walking path. The scene's sleep60 does not join that action. Subsequent Up/Left
facing and speech may overlap it. Animation OFF changes an animation counter, not a position
freeze; init resets movement/obstruction/auto-facing and sprite parameters, not health.
The four sleeps request 165 frames, excluding services/debug skipping, not elapsed duration.

At the end, **F75 is set**, then a pending type1 request supplies Map13 `(16,12)`, Down with
parameter1 zero. This replaces the victory seed; it does not immediately enter exploration.
The order is **F75 → pending tuple → script return → Bowie join tail → clear F413 → set F513 →
read return tuple**. Conditional scrolling wait and speed reset at termination are not a
barrier for every actor. A stalled service can leave earlier mutations without later ones.

F75 controls future Luke follower construction. It neither sets Luke membership F10 or active
F42, invokes `join Luke`, changes force AI nor establishes a playable turn. Victory/exploration
healing restores living/immortal allies' HP/MP while retaining STUN/POISON/CURSE; Peter7 and
Lemon28 are processed even dead, but dead Luke is not revived. Luke can still be declared with
his death mapsprite. His narrated recovery and accompanying the party are separate from health,
recruitment and deployment.

## Returning to the Changed Polca

**Confirmed fresh entry:** `alt_MainLoopEntry` switches map then enters exploration without an
immediate `CheckBattle`. An intact first-completion request enters Polca `(16,12)`, Down;
an F513-set repeat uses its variable seed instead. Exploration clears the earlier pending slot
rather than replaying it as a second warp. Later ordinary admission retains its independent
F413/F513 rules; this return implies no immediate battle retry.

For supplied Map13, exploration adjusts steps, heals living/immortal allies, stores map/not-in-
battle, rebuilds entities/mapsprites, clears temporary **F256–383**, sets F80 and loads/runs init.
Setup priority is **F513 > F799 > F710 > default**. Victory has set F513 before selection. The
special aftermath's Luke pose, action and fixed location are not instructions carried into
this rebuild. Same-map/no-index entry can retain current actors and bypass the temporary reset.

The F513 population has thirteen regular NPC rows128–140 and no fixed Peter or Luke. They
depend on F67/F75 following, independently of HP, membership, active-party state and AI.
Polca admits followers but is not overworld, so F65 adds no caravan prefix here.

| Lookup | Fresh declaration | Physical slot with only F67/F75 |
| --- | --- | --- |
| Peter7 / Luke10 | Follower prefix, Peter before Luke | 1 / 2 |
| 128–132 | Centaur man/woman, kappa and two merchants | 3–7 |
| 133 | Beastman `(23,6)`, Right, addressed as Ripster | 8 |
| 134–136 | Priest and two walking beastmen | 9–11 |
| 137 | Man `(15,3)`, Up, addressed as mayor | 12 |
| 138 / 139 | Dwarf `(38,6)`, Up / orc `(63,63)`, Down | 13 / 14 |
| 140 | Oddler `(14,4)`, Right | 15 |

Lookup identities are not stable physical indices. Other follower flags shift slots; F68 can
add an Oddler-sprite follower under another lookup, without replacing140. Lookup masks the
character byte and adjusts NPC128+ before reading the mapping. Zero is not a missing-actor
safe skip, and `$FF` is not checked as a live actor. `setPos`/`setFacing` skip allies below30
only on zero **word HP**; HP256 passes. Actions, gestures, destinations and text have no such
inherited guard. Writing F513 against an existing population can select the new handlers
without rebuilding their expected cast; dialogue cannot relabel arbitrary current actors.

Shared init checks F711, F716 and F718 in order. F711-clear stages130 with facing and a waited
animation/orientation/sprite/idle body, then resets its animation. F716-set faces137 Down.
F718-set moves140 to internal `$7000,$7000` without deleting its mapping. These branches
neither hide133 nor grant/repair the raft nor invoke the mayor conversation. After init,
music and fade services, only zero pending lets `WaitForEvent` reset camera target0 and
request `j_SetControlledEntityActScript(0)`. That is conditional restoration, not observed input.

## Optional Mayor Contacts

**Confirmed caller priority:** the F513 entity-event row9 selects lookup137 with **RIGHT=0**
and `Map13_201_EntityEvent9`. This direction-valued byte supplies wrapper bits; it does not
require approach from the right. Actual contact must select an actor and recover its event
identity through adjacency/layout, view target and mappings. A table row proves no natural reach.

The wrapper prepares portrait/speech and resolves the mapping. Both bits0/1 are clear: no
pre-handler automatic turn toward Bowie and no post-handler property/facing restoration.
It disables entity updates during the handler, then closes portrait/text and reactivates
updates on return. Window cleanup still applies with bit1 clear.

| First matching supplied state | Mayor response / caller effect |
| --- | --- |
| F718 set | Text1444: reflects on avoiding the fight; returns. |
| F718 clear, F717 set | Text1451: asks again to take the boy; returns. |
| F718/F717 clear, F716 set | Text1512: suggests inspecting the raft; returns. |
| Those three clear, F260 clear | Text1448: asks to take the boy; sets F260 only after text returns. |
| Those clear, F260 set, F261 clear | Text1449: pleads for the boy's life; sets F261 after text returns. |
| Those clear, F260/F261 set, F262 clear | Text1450: repeats the plea; sets F262 after text returns. |
| F718/F717/F716 clear, F260/F261/F262 all set | Runs the full bargain scene below; only its return reaches the raft-state writes. |

With freshly cleared temporaries and unchanged higher flags, three completed contacts prepare
a fourth to run the scene. Each returns rather than falling through several contacts. Arbitrary
supplied flags can shorten the progression; a missing earlier temporary wins even when later
ones are set. Fresh map entry resets all three; current-map state need not. The handler tests
no F75, membership, HP, cast, F64 or existing raft tuple. F716-set with a missing raft receives
repeat speech without repair. F717/F718 responses are preserved as supplied-state branches;
this guide does not establish their later history.

## The Mayor's Bargain

**Confirmed requested speech:** `cs_58856` uses cursor1487 for all thirty texts1487–1516.
Mayor137 speaks sixteen, Luke eight, Peter two, Ripster133 three and narrator one. Peter's
portrait operand is `$C0`, narrator uses `$FF,ENTITY_NONE`, and the others use `$0`. These
attributions describe requests, not guaranteed mappings, truthful history or completed delivery.

| Texts | Speaker and paraphrased content |
| --- | --- |
| 1487–1488 | Mayor cries out, then begs not to be possessed. |
| 1489 | Peter asks what the mayor means. |
| 1490–1491 | Luke calls him confused, then insists they are alive. |
| 1492 | Mayor reacts in surprise. |
| 1493 | Narrator describes the mayor examining Luke. |
| 1494 | Mayor acknowledges Luke is warm and alive. |
| 1495–1498 | Mayor expresses regret, says devils appeared, claims he could not resist, and says they offered safety in exchange for Oddler. |
| 1499 | Luke accuses the mayor of sending him instead. |
| 1500 | Mayor apologizes and asks that King Bedoe not be told. |
| 1501–1502 | Luke agrees, on condition that the mayor supplies a raft for Bowie. |
| 1503 | Mayor thanks him and agrees. |
| 1504 | Mayor calls Ripster. |
| 1505 | Ripster answers. |
| 1506 | Mayor orders a raft for Prince Luke. |
| 1507 | Ripster says it is their last raft. |
| 1508–1509 | Mayor dismisses the objection and demands that another be made if needed. |
| 1510 | Ripster agrees. |
| 1511–1512 | Mayor promises a raft on the river and suggests looking now. |
| 1513 | Peter says the mayor wants them gone. |
| 1514–1516 | Luke agrees, says they now have a raft and urges departure. |

The scene first resets Peter then Luke. The mayor turns Down/Up/Down with pauses, receives
one waited jump and another nonwaited jump, then shivers around his alarmed speech.
`setDest Luke (17,4)` must complete before `setDest Peter (17,5)`: both are destination waits,
not teleports or parallel starts. After Peter's question, Luke faces Down, waits Left1, faces
Up and insists they are alive. Bowie starts Down1 without a wait while the mayor waits Down1.
Bowie faces Up, the mayor Right and Luke Left before the examination and acknowledgment.
Two waited mayor jumps, a pause and Down facing lead into his explanation.

The mayor receives waited speed `(10,10)`/idle, auto-facing disable, waited Up1 and a nod.
Luke waits Left1 and faces Up for the accusation; the mayor headshakes for his apology.
Luke turns Down to agree, then Up to demand the raft. The mayor resets, jumps once waited
and once nonwaited, and accepts. Luke and Peter disable auto-facing; Luke starts Right2/Up1,
while the mayor waits Down1/Right2. They face Down. Peter starts Left1, while the mayor waits
Down3 and faces Right; camera `(11,2)` frames the Ripster exchange.

The mayor jumps waited then nonwaited before calling Ripster. The script places133 `(23,7)`,
Left, waits its speed `(48,48)`/idle body and Left5 approach, and requests the answer/order.
Ripster resets, waits `eas_JumpRight`, pauses and waits Left1 before objecting. The mayor's
waited/nonwaited jump pair accompanies his insistence. Ripster nods, agrees, receives another
waited speed48/idle body and Right5, then is hidden. Camera `(11,0)` and waited mayor Up3
precede his river promise. He waits Left2/Up1, faces Down and pauses. Peter then Luke reset.
Luke waits Down1/Left1 and faces Down for Peter's comment, then waits Left1 and faces Down
before urging departure. The final requests configure Peter→Bowie, then Luke→Peter following.
Oddler140 is discussed but receives no command in this scene.

**Confirmed service limits:** destination operands scale tiles by384 internal 1/16-pixel units,
write travel and ±32 velocity, and wait for position equality here because bit15 is clear.
Collision, supplied actor state and VInt progress can prevent completion. Speed10/48 are
internal parameters, not tiles per frame. Action installers replace pointers/timers; a waited
form polls only its chosen actor for `eas_Idle`. Waiting the mayor does not join Bowie's Down1,
Luke's Right2/Up1 or Peter's Left1. Later resets can replace nonwaited actions, so their achieved
positions remain Unknown. Second jumps can overlap speech/shiver/later commands. Jump/reset,
gesture, camera, text/window and audio services have distinct waits; twelve sleeps request 350
frames without proving duration or rendering. The terminator is no global completion barrier.

## Following, Ripster's Absence and the Raft State

**Confirmed current following:** both final `followEntity` calls use selector2. Their first words
are `$0007` and `$000A`, but the handler reads the **high byte** before its HP helper: both
test **Bowie0's word HP**, not Peter's or Luke's. Zero skips each command's six operand bytes;
nonzero permits it regardless of the followers' HP. With valid mappings, `AddFollower` copies/
patches a 42-byte action, replaces its pointer and adds its physical index only if absent.
Selector2 supplies `(-24,0)` pixel offsets, without immediately placing or settling the chain.
Target mappings have no presence/HP guard; absent Peter can resolve physical0.

Current Peter→Bowie and Luke→Peter requests set no F67/F75, F7/F39/F10/F42, health or AI.
Future declarations still depend on supplied flags and completed aftermath F75. Even if zero
Bowie HP skips both follow commands, script return can still reach the caller's raft writes.

Hiding133 moves position/destination to internal `$7000,$7000`, invalidates every mapping alias
to that physical actor with `$FF` and removes it from the follower list if present. It changes
neither the population table nor a persistent Ripster flag. Future fresh F513 Polca recreates133
at `(23,6)`; init does not hide it. A transition retaining mappings differs. The scene has no
later133 lookup, and its current absence proves no durable departure.

**Confirmed grant order:** only script return reaches **F716 → F64 → RAFT_MAP72 → RAFT_X43 →
RAFT_Y48**, as separate caller writes. The scene's raft speech writes none of these. A stalled
text/movement/scroll service may prevent later writes; interruption between writes can leave
partial state. This is no atomic receipt, repeat-mismatch repair or guarantee that the scene ran
merely because supplied flags/tuple match. The caller queues no warp and changes no `PLAYER_TYPE`.

For an ordinary player, `DeclareRaftEntity` and initialization's `sub_44404` require **F64 and
`CURRENT_MAP==RAFT_MAP`** before declaring the stored raft. Coordinates are masked by `$7F`
and scaled by384; the separate raft uses physical31, mapping offset63, Left, raft sprite and
`eas_Standing`. F716 is not a substitute for those gates. `DeclareRaftEntity` returns immediately
for an existing raft player; on ordinary unlocked-map mismatch it clears the raft mapping and
moves it off-map, while its matching branch also relocates ordinary followers. `sub_44404` has
a distinct supplied raft-player branch: main's sprite changes, followers stand and the separate
entity uses `(64,64)`. These are conditional consumers, not an unconditional spawn.

Thus **Map72 `(43,48)` configures a future raft; it is not a raft present in current Polca/Map13**.
The mayor's river promise and Luke's asserted receipt establish neither river availability nor
travel, collision access, sprite presence or boarding. Existing F64 reset and arbitrary supplied
state can produce the same tuple without this conversation.

After the handler, the RIGHT=0 wrapper cleans windows and restores entity updates without
restoring the mayor's old facing. `ProcessPlayerAction` returns to exploration. Only zero pending
lets `WaitForEvent` retarget camera0 and invoke controlled-entity action; ordinary `PLAYER_TYPE=0`
selects character control, while other supplied types differ. This scene adds no pending event,
but the handoff remains conditional, with no input receipt or proof all nonwaited actors finished.
**Stop at that caller/control return.**

## Evidence Review and Remaining Limits

This guide consumes research accepted in
[PR #805](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/805) and the predecessor
English guide from accepted main. Research owns USA retail identity, pinned SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6`, aftermath `0x4ACF8`, population `0x4AD96`,
mayor caller `0x581A2` and scene `0x58856`. Its
[reproduction boundary](../../research/battle13-outcomes-through-polca-raft-grant.md#reproduction-and-evidence-limits)
retains the complete scene/text/inline/source comparisons. This slice reviews accepted prose,
contracts and bounded tracked-fixture selections without source reinvestigation, optional payload
copying/generation or runtime observation.

| Reviewed boundary | Existing fixture / contract / consumer | Disposition |
| --- | --- | --- |
| Mayor caller | [sf2-map-events-static-v1:entity-programs](../../../tests/fixtures/h2/map-events/entity-programs.json), program 57; [entity schema](../../../schemas/h2/map-events-entity-programs.schema.json), [fixture loader](../../../src/sf2tool/h2/map_events_fixture.py). | All 34 operations retain F718/F717/F716 priority, temporary contacts, scene then five writes and all response branches. No natural-contact or repair claim. |
| Outcome | [sf2-battle-cutscenes-static-v1](../../../tests/fixtures/h2/battle-cutscenes-static-v1.json), [routing contract](../contracts/battle-cutscene-routing.md), [cutscene verifier](../../../src/sf2tool/h2/battle_cutscenes.py). | Shared wrapper/leader/join order is separate from row13 targets and full loop/Egress composition supplied by accepted research; aggregate inventories do not prove this complete outcome. |
| Fresh town | [sf2-map-init-static-v1](../../../tests/fixtures/h2/map-init-static-v1.json), body 18; [init verifier](../../../src/sf2tool/h2/map_init.py), [map exploration](../contracts/map-exploration.md). | Thirteen init operations preserve independent F711/F716/F718 tests. Full population/follower allocation and fresh/current-state distinction stay with selected research. |
| Text, actors and following | [sf2-map-script-engine-static-v1](../../../tests/fixtures/h2/map-script-engine-static-v1.json), [dialogue](../contracts/dialogue-system.md), [party/roster](../contracts/party-roster-state.md), [map-script verifier](../../../src/sf2tool/h2/map_script_engine.py). | Handler/family facts retain action, destination, hide and following boundaries; they are not the complete scene corpus or natural presentation proof. Research owns all 38 texts, inline composition and high-byte HP admission. |
| Requests and raft | [sf2-map-event-request-state-static-v1](../../../tests/fixtures/h2/map-event-request-state-static-v1.json), [sf2-map-event-request-consumption-static-v1](../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json), [request-consumption owner](../../research/map-event-request-consumption.md). | Pending-return and conditional declaration facts do not make five caller writes atomic or prove access/boarding. Selected immediate consumers remain research-owned. |

The [init schema](../../../schemas/h2-map-init-static-fixture.schema.json) and
[map-script schema composition](../../../src/sf2tool/h2/map_script_schema.py) preserve their
bounded evidence shapes. Existing focused checks include `tests/python/test_map_events.py`,
`test_map_init.py` and `test_map_script_engine.py`. Existing narrow routes are
`uv run sf2 h2 map-events`, `map-init`, `map-script-engine`, `battle-cutscenes`, `battle-routing-data`,
`map-setup`, `map-entities`, `map-event-request-state` and `map-event-request-consumption`.
Naming them is not a claim of new execution or a new documentation gate.

**Unknown:** natural battle/contact reach; actual flags, HP, membership/deployment/AI, actors and
mappings; concurrent positions/actions and all service/pending returns; rendered/audible content,
responsive control, save/load persistence, river availability and boarding. These gaps neither
block this static explanation nor authorize acquisition. Accepted research retains predecessor
comparisons and corrected navigation/fixture-read failures, including
[Issue800's handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/800#issuecomment-6158870823).
This guide claims no fresh runtime pass, continuous playability or campaign/epic completion.
