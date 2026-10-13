# Peter's Castle Audience and the Return to Town

Peter's reception in New Granseal leads to a separate castle audience. Astral and the king hear
about the phoenix and Volcanon; Astral proposes accompanying Peter east, but the minister and king
insist that he stay. The party is sent instead. Peter introduces himself, the king orders a gift,
and Astral gives his farewell before the party's departure. **Inferred narrative meaning:** Peter
becomes a travelling companion while Astral remains behind. **Confirmed static behavior:** the
audience joins Peter with AI control, stages temporary followers and requests a return to town;
its caller then sets separate completion and follower flags. **Unknown:** natural travel, completed
actors/services, the observed gold balance and actual usable destination control.

Read this after [New Granseal town: Peter's first introduction](new-granseal-town-through-peter-introduction.md).
The accepted [research owner](../../research/peter-castle-audience-through-town-return.md) supplies
the scene, text, handler and caller review. This guide stops at the first town post-init
pending/control read. Overworld departure, Battle09 and later audiences are outside this segment.
It adds no remake capability or decision about the gift. The
[roadmap](../documentation-roadmap.md#current-baseline-and-near-term-synthesis) owns the Standard-rigor
agreement for [#751](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/751).

`Fnnn` denotes flag nnn. Source identities use `disasm/` at
[`ShiningForceCentral/SF2DISASM` commit `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
Research and fixtures bind the USA retail ROM in the [root baseline](../../../README.md#pinned-original-baseline).
Command indices below are zero-based source records, not frames or measured durations.

## 1. Returning to the Castle Is a Separate Admission

**Confirmed conditional connection:** the preceding Peter event ends with caller F701. It neither
joins Peter nor sets F702/F67, reloads the population or requests this castle journey. The town's
north-exit row matches Y=0 with wildcard X and requests castle Map7 `(11,22)`, Up, through a
no-scroll warp. Ordinary movement tests the row on the selected warp layout attribute after earlier
movement/collision work. A matching row does not prove an accessible tile or natural walk from
Peter's reception.

If that request survives, exploration consumes and clears it before dispatch and returns the
explicit tuple to `MainLoop`. Map7 has no substitution or battle-coordinate row, so ordinary
exploration entry follows. It clears pending, applies the accepted
[living/immortal heal and status limits](../../research/battle07-outcomes-and-aftermath-handoff.md#outcome-branches-and-caller-order),
sets map/not-in-battle state, rebuilds entities/sprites, clears temporary flags 256–383, sets F80,
loads the map and runs init before control. This is not a full-force reset.

The normally completed predecessor lineage carries F707/F411, `EGRESS_MAP=6` and `PLAYER_TYPE=0`.
F706 belongs to the independent sailor/guard scene and need not be set with F701. Other party,
follower and later-history state remain supplied inputs.

## 2. Audience Eligibility and the Available Cast Differ

**Confirmed setup priority:** Map7 scans default, F701, F702 and F805 in that order; the last
applicable row wins. All variants share the zone table and init function.

| Supplied history | Population selected before init |
| --- | --- |
| F805 set | F805 population: 11 NPC declarations, without fixed Astral. |
| F805 clear, F702 set | Default population: 12 NPC declarations, even with F701 set. |
| F805/F702 clear, F701 set | Audience population: five allies and 12 NPC declarations. |
| All three clear | Default population. |

The audience population places Peter `(11,6)`, Left, opposite Astral reference 139 `(10,6)`, Right.
The king 129 stands `(11,4)`, Down, with minister 128 `(12,5)`, Down. Sarah, Chester, Jaha and Kazin
face Up across `(12,8)`, `(10,8)`, `(13,8)` and `(9,8)`. These are declared operands, not guaranteed
physical slots or observed positions: initialization allocates flag-selected followers first and
skips a fixed ally declaration if that ally is already mapped.

Shared init tests F707 alone. Set returns without changing this population or egress/F411. Clear
runs the [year-later address](new-granseal-castle-year-later-control.md), sets egress 6 and then
F707/F411; that existing scene explicitly restores the default population. Consequently supplied
F701-set/F707-clear history does not retain the ordinary five-ally audience staging. The audience
itself does not reload actors before using them.

**Confirmed audience predicate:** all five zone points `(9..13,8)` select `Map7_ZoneEvent0`.
Movement must separately produce a zone request on the zone layout attribute; neither these rows
nor the castle entry tuple proves natural zone reach. The consumer clears pending before dispatch
and resolves the setup again.

| Caller input | Immediate result |
| --- | --- |
| F802 clear, F701 set, F702 clear | Run `cs_55A36`; after normal return set F702, then F67. |
| F802 clear, F701 clear | Skip the audience, regardless of F702. |
| F802 clear, F701/F702 set | Skip; absent Peter membership or F67 is not repaired. |
| F802 set | Take the later F805 selector first; its other audience body is outside this guide. |

F707, F706, F67, membership, AI and actor presence do not guard the early audience. F805 is also
not a guard when F802 is clear: an unusual F805-set/F701-set/F702-clear history can admit the early
script with a different cast. These supplied states constrain interpretation; they are not claims
of naturally reachable histories.

## 3. Astral Offers to Go; the Party Goes Instead

**Confirmed ordered content:** `cs_55A36` has one finite, unbranched 88-command sequence and
39 sequential dialogue IDs 1198–1236. The accepted research compared the source comments and
dialogue text. The following explains the exchange without reproducing a transcript or treating
the characters' claims as independently established world facts.

The camera moves toward the audience. Astral and the king discuss Peter's phoenix identity and
Volcanon; Peter describes an injured wing and asks for help. Astral moves and nods toward him,
then turns to the party and proposes travelling east with Peter. The minister advances to challenge
him. Astral's individually awaited jump-left action, a wait and rightward movement precede his
reply. The minister and king insist that he remain.

The king sends the party instead and Astral agrees. Peter approaches, introduces himself and is
described as accompanying the party in text 1230 (`04CE`). That narration precedes the actual
`joinForceAI` command. The script then moves Peter, installs the five temporary follower chains
and pauses before the gift exchange. Astral's staying behind is **Inferred narrative meaning**;
the program does not dismiss him or clear independently supplied F70/F74 follower flags.

Named movement waits join only their named actor. The minister's up-one action during the decision
is unjoined, while Astral's next left/up action is awaited. Timed waits do not independently prove
earlier actions completed, and their operands are not measured presentation time. Dead-ally guards
can skip relevant actor commands; the join command has no corresponding alive guard. Missing
mappings or unusual follower history also constrain the staging. Script return does not prove
every actor, camera, text, sound or movement service completed visibly.

## 4. Joining, Scene Following and Town Following Are Separate

**Confirmed join semantics:** command 60 is `joinForceAI ALLY_PETER,$FFFF`. Its second word is a
nonzero selector, not an activation-field replacement. The handler reads Peter's old activation
word, ORs in `AIBITFIELD_AI_CONTROLLED` (4), calls `JoinForce` and stores the ORed word afterward.
Other activation bits survive. A zero selector would clear only the AI bit and would not join.

`JoinForce` sets joined membership, rebuilds the force lists with `UpdateForce`, checks the rebuilt
active count and sets active membership only when below the source-defined capacity. For this
pinned source Peter's flags are joined F7 and active F39, and capacity is 12. Thus an absent Peter
joins and becomes active with room available; at capacity he joins without setting a previously
clear active flag. Already active Peter remains active; an already joined reserve Peter can become
active when room exists. Repeating the command reasserts AI control and repeats the rebuild/capacity
decision without creating another member. These local numeric facts do not expand the
[membership contract's symbolic capacity boundary](../contracts/party-membership-state.md#joinforce-chronology).

The rebuild precedes the possible active-flag write, with no second rebuild inside `JoinForce`.
Its lists can therefore lag a newly set active flag. Normal text processing later in this audience
matters: the gift text's `{LEADER}` substitution calls `UpdateForceAndGetFirstBattlePartyMemberIndex`,
rebuilding before looking up the first active member's name. Debug text skipping can bypass that
rebuild. Neither helper-local fixtures nor this static order establishes observed lists at town
control. The join handler does not reset HP, rename Peter, play join music, install followers or
set F67.

**Confirmed temporary following:** commands 62–66 install Peter behind Bowie, Sarah behind Peter,
Chester behind Sarah, and Jaha/Kazin behind Bowie at separate offsets. `followEntity` resolves both
references and, under its alive guard, calls `AddFollower` to install a walking script and register
the physical follower. Reusing a follower already in the exploration list still replaces its walking
script. These operations do not join or activate anyone and do not set persistent story follower
flags. Their table offsets do not promise observed final positions.

**Confirmed destination following:** only the zone caller sets F67, after the script returns.
The destination rebuild replaces those temporary actors/scripts and uses the ordinary follower
table's F67 row to allocate Peter. That row tests F67, rather than F7/F39 or AI control; Map6 uses
this ordinary table even with supplied caravan F65. Other follower flags remain independent.
No persistent Sarah/Chester/Jaha/Kazin follower flags are set by this scene. Membership, active
membership, AI control, temporary following and F67 must remain distinct in any state account.

## 5. The Gift Narration Has No Effective Award in the Reviewed Chain

The king requests a gift; the minister nods, moves away, pauses and returns to offer it. Text 1234
(`04D2`, command 76) narrates a gift of 100 gold to the leader. **Confirmed at the reviewed static
boundary:** the string contains a leader substitution, ordinary literal digits 100, a newline and
W1 wait. It contains no numeric reward operand or hidden gold instruction.

The complete program has no custom call or gold-mutating command. The reviewed display/token
chain rebuilds lists for `{LEADER}` and performs text/layout/input work; it does not invoke a gift
award through `SetGold`, `IncreaseGold` or `DecreaseGold`, or directly write `CURRENT_GOLD`.
The zone caller only sets F702/F67. Thus this bounded source chain does not implement the advertised
+100 award. The list rebuild is a real text-side state effect, separate from crediting money.

**Unknown:** the before/after gold balance in a naturally reached retail audience, authorial intent
and any remake decision about the discrepancy. The guide preserves the narrative/state mismatch;
it supplies no runtime balance claim or fix. The [research gift review](../../research/peter-castle-audience-through-town-return.md#the-narrated-gold-gift)
owns the exact text and service anchors.

## 6. Departure Produces a Request before the Caller Flags

Astral moves to give farewell and advice; Bowie nods. After a pause, the camera follows Bowie and
his down-18 movement is individually awaited. That movement does not establish the destination:
command 86 explicitly requests town Map6 `(11,1)`, Down, with a no-scroll type-1 warp; command 87
ends the program.

**Confirmed order:** joining/AI and temporary followers precede request production; the request
precedes script return and caller F702, then F67. The warp writes the pending event word and tuple,
without entering town, changing `CURRENT_MAP` or assigning `WARP_SFX`. Script-end handling can wait
for view scrolling when a dialogue window is present before returning. The zone wrapper then closes
portrait/text, waits one VInt and waits for physical entity 0 to stop; it does not join the whole
follower chain or clear this newly produced request.

On the next exploration `WaitForEvent`, surviving nonzero pending bypasses ordinary control
installation. `ProcessMapEvent` clears it before dispatch, and the no-scroll consumer plays/clears
the supplied warp SFX, idles main and returns the explicit tuple to `MainLoop`. This is one mutable
pending slot, not a queue: interruption or overwrite can break the chain. An interrupted audience
can leave joined/AI or temporary followers changed while F702/F67 remain unreached.

## 7. Town Re-entry Rebuilds the Population before the Control Read

**Confirmed conditional admission:** Map6 has no substitution or battle-coordinate row. Ordinary
exploration entry clears pending, applies the shared heal limits, selects map 6, rebuilds entities/
sprites, clears temporary flags 256–383, sets F80, loads the map and runs shared init. The named
membership and story flags lie outside that temporary-clear range. Main uses `(11,1)`, Down under
these entry assumptions; Peter is readmitted through F67, replacing scene following.

F702 now wins over F701 in the town setup order. Its population contains 14 NPC declarations,
references 128–141, without fixed Peter or minister. Peter's presence depends on follower selection,
not that population list. Shared init adjusts this selected population separately:

| Init history | Adjustment before the pending/control read |
| --- | --- |
| F802 set | Place priest 132 at `(11,18)`, Down; do not test F706. |
| F802 clear, F706 set | Place worker 138 at `(63,63)`, Down, and guard 139 at `(15,23)`, Down. |
| F802/F706 clear | Return without a script or repositioning. |

Normal early-audience completion carries F802 clear, so the last two rows are its alternatives;
the first is a supplied-state control. These init scripts neither write flags/requests, join anyone,
award gold nor restart Peter's introduction. The audience assigns neither player type nor egress.

After conditional palette/music work, the first `WaitForEvent` read is the stopping boundary:

| State at that read | Immediate source result |
| --- | --- |
| Nonzero pending | Bypass control installation and handle the request; usable control is not established. |
| Zero pending, carried player type 0 | Set view target 0 and call `SetControlledEntityActScript(0)` to install `eas_ControlledCharacter`; the following loop reads pending again and tests A/C. |
| Zero pending, supplied nonzero player type | Retain the vehicle-control branch. |

No unconditional pending clear occurs between init return and this read. F702-set/F67-clear reentry
skips the audience and does not repair Peter's follower flag. **Unknown:** actual usable input,
natural travel, reachable history/mappings, actor/service completion, pending survival and save
persistence. Stop here; later town services, overworld departure and Battle09 require separate
accepted owners and do not follow automatically from this conditional control boundary.

## Evidence Review and Limits

The accepted research supplies scene-specific dialogue, populations, inline actions, the gold-chain
review and the composed request/caller chronology. Tracked fixture review preserves the narrower
executable seams below. No disagreement requires a research/contract correction: general contracts
leave caller-specific list synchronization open, while this accepted audience review identifies the
normal `{LEADER}` rebuild and its debug-skip limit. Compact fixtures do not execute this whole scene.

| Relation | Reviewed executable owner | Limit |
| --- | --- | --- |
| Setup, actors and init | [sf2-map-setup-static-v1](../../../tests/fixtures/h2/map-setup-static-v1.json), `expected.sourceFacts` / `selectionCases`; [sf2-map-entities-static-v1](../../../tests/fixtures/h2/map-entities-static-v1.json), `expected.consumerFacts`; [sf2-map-init-static-v1](../../../tests/fixtures/h2/map-init-static-v1.json), `expected.primarySourceBodies[10]` / `[11]`. | Shared selectors, mapping and Map6/7 init; not natural travel or guaranteed scene actors. |
| Audience caller | [sf2-map-events-static-v1](../../../tests/fixtures/h2/map-events-static-v1.json) and [zone-program shard](../../../tests/fixtures/h2/map-events/zone-programs.json), `expected.zoneTargetPrograms` `Map7_ZoneEvent0`. | F802/F701/F702 branches and script → F702 → F67; not natural zone admission. |
| Join/AI and scene commands | [sf2-map-script-engine-static-v1](../../../tests/fixtures/h2/map-script-engine-static-v1.json), `expected.forceStateCommandFacts.activePartyCommandFacts` including `cs_55A36:60:joinForceAI`; [sf2-common-stats-static-v1](../../../tests/fixtures/h2/common-stats-static-v1.json). | Command/handler and helper order, not completed audience services or town list state. |
| Handler-local party/following | [sf2-force-state-active-party-runtime-v1](../../../tests/fixtures/h3/force-state-active-party-v1.json), with the [roster contract](../contracts/party-roster-state.md) and [membership contract](../contracts/party-membership-state.md). | Controlled helper cases; no audience gold delta, natural recruitment or save/control claim. |
| Pending consumer/control | [sf2-map-event-request-consumption-static-v1](../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json), `eventRequestConsumption.consumerContexts.waitForEvent`; [sf2-common-maps-static-v1](../../../tests/fixtures/h2/common-maps-static-v1.json) and [sf2-common-scripting-static-v1](../../../tests/fixtures/h2/common-scripting-static-v1.json). | Shared access/consumer facts, without an executed continuous audience-to-town composition. |

The [script verifier](../../../src/sf2tool/h2/map_script_engine.py),
[schema composition](../../../src/sf2tool/h2/map_script_schema.py),
[state-block schema](../../../schemas/h2/map-script-engine-state-block.schema.json),
[map-init verifier](../../../src/sf2tool/h2/map_init.py) / [fixture schema](../../../schemas/h2-map-init-static-fixture.schema.json),
and [request-consumption verifier](../../../src/sf2tool/h2/map_event_request_consumption.py) retain
their bounded comparisons. Their existing narrow commands use `uv run sf2 h2` followed by
`map-script-engine`, `common-stats`, `map-setup`, `map-entities`, `map-init`, `map-events`,
`map-event-request-consumption`, `common-maps` or `common-scripting`.
**These rails and the H3 fixture were not rerun for this documentation slice.** The
[research reproduction owner](../../research/peter-castle-audience-through-town-return.md#coverage-discriminators-and-reproduction)
retains the source anchors and remaining composition. No source re-investigation or runtime
observation is claimed, and the preserved Unknowns do not authorize acquisition automatically.

This guide completes neither campaign chronology nor epic #514. Chinese synchronization remains
deferred under the roadmap; original-game evidence and future remake decisions retain their owners.
