# Hassan Rick Recruitment and Following

Rick can join the force through a separate conversation in Hassan. He credits the leader for helping
him reach Hassan, then notices the Caravan. Peter explains where it came from, Rick asks to accompany
the party, and the scene joins him before requesting that his current actor follow the group. This is
an optional interaction branch, not a required return from the shrine or a new travel instruction.

**Confirmed:** the accepted [research owner](../../research/hassan-rick-recruitment.md) establishes
conditional population, ordinary activation, the complete recruitment scene and its immediate
membership/following/control consumers. **Inferred:** Rick's curiosity about the Caravan motivates
his request to leave with the party. **Unknown:** natural reach, actual supplied flags/HP/roster and
actor mappings, completed services/presentation, usable input and save/resume persistence.

Read [Hassan entry](battle16-outcomes-through-hassan-entry.md),
[Rohde's proposal](hassan-ancestry-and-rohde-caravan-choice.md) and
[Caravan acquisition](battle18-outcomes-and-caravan-acquisition.md) for their separate boundaries.
Their scenes do not establish a mandatory journey to this conversation. This guide stops at the
conditional ordinary control/reentry boundary. Other NPC conversations, Caravan operation,
later travel/Battle19 and Rick's growth, equipment or combat are outside its scope.
Fnnn means flag nnn; indices are zero-based. Coordinates, waits and action requests below are
source operands, not measured animation or input receipts. Dialogue is paraphrased.

## The States That Select Different Parts of the Event

**Confirmed:** the states below have different consumers and lifetimes.

| State | What it controls here | What it does not establish |
| --- | --- | --- |
| F518 | Selects Hassan's alternate population, including Rick's fixed actor. | Caravan operation, a completed journey or talk admission. |
| F12 | Rick's joined-force membership; Hassan init moves his mapped actor off-map when set. | Active-party membership or permanent following. |
| F44 | Rick's active battle-party membership, separate from F12. | A synchronized roster-list snapshot, living HP or a follower action. |
| F256 | Temporary suppression checked by the Rick handler and set after scene return. | Persistent recruitment or an atomic completion receipt. |
| F65 | Separate Caravan context; Hassan init uses it to move Rohde off-map. | A prerequisite checked by Rick's handler, or Rick's transport/controller mode. |
| Current physical follower list/action | Chooses the follow target, excludes listed actors from ordinary talk and stores the current following request. | A persistent flag that recreates Rick on a fresh map population. |

There is no F65, Sword-possession, F516 or battle-order test in this recruitment handler. The
Kraken and shrine references below are attributed speech. Population can make Rick available
without the handler having checked those narrated events.

## Where Rick Appears and What Init Does

**Confirmed:** Map9's default setup has 11 rows and no Rick. F518 selects the 12-row alternate,
which adds Rick immediately after Rohde at `(12,3)`, facing Right with `eas_Init`. Both lists share
the entity-event table and map init. The alternate also changes three other actors' placement/action
operands; their conversations are outside this guide. Ten non-ally rows map to logical128–137;
ally rows do not consume that sequence. Rick's logical identity is ally12, not an assumed physical
slot beside Rohde.

Fresh entity initialization clears physical state and all 64 lookup bytes, declares flag-selected
followers from physical1, declares setup actors, then creates main physical0. An ally setup row
reuses an existing nonzero lookup rather than declaring a duplicate. Rick's joined flag is not a
population admission test. His initial action sets speed32, disables acceleration/deceleration,
enables obstruction and map/entity collision, enables auto-facing, sets Up, disables immersion,
sets size24, refreshes the sprite and idles. It does not heal him.

Outside battle, Rick at HP0 receives the blue-flame mapsprite identity; a living Rick uses his
current class-based sprite. HP0 does not remove the setup actor or its event row. No fixed class
or equipment value follows from this population rule.

Hassan uses the normal follower table even with F65 set. That table has no Rick declaration.
Peter is conditional on F67 and has no fixed row in either Hassan list. F84 can declare Rohde
before his fixed row and cause that row to reuse his lookup. Neither Peter's presence/living HP
nor a particular follower order follows merely from choosing the alternate setup.

The complete shared init has 13 operations. In order, it moves logical137 off-map if F518 is
clear, moves Rohde11 off-map if F65 is set, moves Rick12 off-map if F12 is set, then returns.
It neither recruits nor writes a transport or new battle flag. `MoveEntityOutOfMap` resolves the
current lookup and writes `$7000` to position X/Y and both destinations. It does not delete the
actor/mapping/list, stop actions or velocity, call `HideEntity`, or change HP.

Consequently, a fresh F518-set/F12-set population still declares Rick, then moves him away. In a
supplied fresh F518-clear/F12-set state, no Rick row fills lookup12; its default zero aliases
physical0, which init moves instead. These are source consequences of supplied states, not an
observed natural bug or proof of usable control. The helper does not repair missing mappings.

## Admitting an Ordinary Conversation

**Confirmed:** ordinary player action first idles physical0 and waits for its movement and view
scrolling. A has priority over C. On the ordinary C branch after debug/Caravan alternatives,
`GetActivatedEntity` uses the current view target/facing and layout to select the first qualifying
nearby actor, with Manhattan distance below one internal map tile (384). It extends the addressed
point across `$2800` counter blocks and excludes the view target and physical actors already in
`EXPLORATION_ENTITIES`. It does not simply select the fixed Rick coordinate or test F518/F12.

The reverse lookup must resolve that physical actor to logical12 to select Rick's event row.
Lookup aliases can resolve to an earlier identity in the scan. Calling `Map9_EntityEvent1` directly
with an artificial state proves neither proximity admission nor the existence of a Rick actor.
A default-zero lookup is likewise insufficient evidence of a talkable Rick.

The row `ALLY_RICK, DOWN, Map9_EntityEvent1` uses DOWN=3 as **action flags**, not as a required
player-facing direction. The entity wrapper saves player facing and supplies portrait/speech
properties. Bit0 turns the mapped actor opposite the player, `(playerFacing+2)&3`, before the
handler. Bit1 restores the saved actor facing afterward, using the saved physical index. Thus the
scene's final facing need not survive the ordinary wrapper.

The wrapper disables updates around the handler, but the Trap6 script bridge enables entity
updates before executing the scene. That provides a path for waited actions to update; it does
not prove they finish. After the handler, the wrapper restores facing, closes portrait/text and
reactivates updates.

The handler itself has only five operations: check F256, return if set, run `cs_56B02`, set F256,
return. It does not check membership, capacity or HP. F256 set suppresses the recruitment script
while the admitted wrapper's facing/window work still occurs. F256 clear enters the complete scene;
F256 is written only after that scene returns.

## The Complete Conversation and Staging

**Confirmed:** `cs_56B02` at `0x56B02` contains 29 commands with no choice, internal branch or inline
action bodies. The retained physical corpus groups it within `cs_56AE6`: records6–34 are Rick's
scene; records0–5 are Rohde's repeat prompt and branch away. Rick's handler enters its own label
directly. The Rohde prefix is not part of Rick's speech or a recruitment choice.

| Physical records | Ordered scene meaning and requests |
| --- | --- |
| 6–11 | Set text cursor1411; set Rick priority `$FFFF`, Bowie priority0. Rick asks whether the leader killed the Kraken (1411), credits him for reaching Hassan (1412), then describes the town as dull (1413). |
| 12–18 | Wait20; set Rick Up; wait30; run/wait Rick's `eas_Jump`; wait20; face Rick toward Bowie; Rick asks about what he sees (1414). |
| 19–23 | Face Peter toward Rick, then Rick toward Peter. Peter identifies the Caravan as an ancient vehicle found in the shrine (1415). Wait20; face Rick toward Bowie. |
| 24–28 | Rick shows interest (1416), asks about the discovery (1417), requests to accompany the party (1418); join Rick; Rick celebrates leaving (1419). |
| 29–34 | Run/wait Rick's jump twice; wait20; set Bowie priority `$FFFF`; add Rick as a current follower; end. |

These are all nine scene text sites, IDs1411–1419, source text keys `0583..058B`. Rick's speaker
operands are `$0,ALLY_RICK`; Peter's are `$C0,ALLY_PETER`. IDs1411,1412,1416,1417 use `nextText` with
source `{W2}`; 1413,1414,1415,1418,1419 use `nextSingleText` with `{W1}`. The generic join announcement
446 (`01BE`) is an additional service text using Rick's name/current class, not a tenth cursor site.

The text handlers obtain portrait/speech properties, wait for view scrolling, request display and
advance the cursor. Single-text also closes portrait/text and waits10. W1/W2 are text-service
controls, not additional scene commands or receipts of acknowledgement. At script entry the skip-text
byte clears. Debug mode with P2 Start can set it and suppress ordinary display/cursor increments and
script sleeps; that does not bypass every action or join service and is not the ordinary story path.

**Inferred:** the brief look away, jumps and exchanged attention present boredom giving way to
excitement. Peter's explanation supplies the conversational context. **Confirmed:** none of that
speech checks an actual vehicle actor, F65, Sword custody, Kraken outcome or a natural shrine route.

### Facing, Priority and Jump Completion

**Confirmed:** the two Rick/Bowie priority writes initially give Rick nonzero sprite-link priority
and Bowie zero; the later Bowie write gives him nonzero priority. Nonzero entries are processed
before zero entries. These are explicit values rather than restores of previous values, and are
neither battle-turn ordering nor direct VDP tile-priority writes.

The explicit Rick-Up command passes its packed ally selector through the character-alive helper
and skips at Rick HP0. The four `faceEntity` commands have no HP guard. They use actors' **destinations**, not visible positions:
choose horizontal only when absolute X separation exceeds Y separation; otherwise choose vertical.
A target left/up selects Left/Up, the other side Right/Down; coincident destinations select Down.
Each writes facing, refreshes the sprite and waits one VInt. Missing Peter lookup7 can redirect
these operations to physical0; it does not remove Peter's speech.

Each of the three waited jumps replaces Rick's action pointer, then polls until that actor's
pointer is `eas_Idle`. It has no HP guard or whole-party barrier. The jump disables auto-facing,
requests relative `(0,-1)`, waits5, requests `(0,1)`, waits5, requests `(0,0)`, re-enables auto-facing
and returns to idle. Relative movement scales by384 from current position and checks other-entity
destination conflicts. There is no destination-equality wait between those requests. An idle action
pointer therefore does not prove an exact visible arc or settled position. Stalled updates/services
can prevent membership, following or the caller's later F256 write while retaining earlier changes.

## Joining the Force Comes Before Following

**Confirmed:** `join ALLY_RICK` is the ordinary csc08 word12 form, without sad-join or special128
Sarah/Chester behavior. It clears current speech SFX, waits view scrolling, selects join music,
calls `JoinForce`, reads the current class, supplies name/class dialogue indices and requests446.

`JoinForce` performs three distinct steps:

1. Set F12, including when it was already set.
2. Rebuild joined, active and reserve counted lists from their independent flags, without filtering HP.
3. If the rebuilt active count is below12, set Rick's active flag F44. At12 or more it skips this write;
   it does not clear an existing F44 or replace another member.

The rebuild precedes that possible F44 write, and this join tail has no second rebuild. Newly
activated Rick can remain in the reserve snapshot until a later `UpdateForce`. A full party blocks
only the new active write; it does not block F12, the announcement or the later follower request.
HP0 blocks none of these membership writes. Neither the scene nor join heals/revives, resets combat
stats, grants AI control or reselects Rick's mapsprite identity.

The announcement tail schedules the previous music through `PlayMusicAfterCurrentOne`, then
`WaitForPlayerInput` waits VInts until `CURRENT_PLAYER_INPUT` contains a standard direction/button
bit. It does not require release or a new input edge; already-held current input can qualify.
The helper's `FadeOut_WaitForP1Input` name proves neither a screen fade nor completed audio. The
join handler then closes text and waits10. Rick's final speech and two jumps still follow this tail.

## Following Belongs to the Current Population

**Confirmed:** `addNewFollower ALLY_RICK` uses csc56, distinct from an explicit `followEntity`
command. It has no Rick-HP or Bowie-HP guard. It resolves Rick12, scans the current physical list
through its `$FF` sentinel and chooses its **last listed actor**, or physical0 when empty. It
passes follower offsets `(-24,0)` pixels. Peter is not selected by name and no missing Rick is created.

`AddFollower` copies a 42-byte follower action block, patches its target/offsets and advances the
walking-parameter allocation. It appends Rick's physical actor before the sentinel only when absent.
An existing entry is neither moved nor removed, but even a duplicate installs/allocates another
block. If Rick is already the tail, he becomes his own target; there is no self-target rejection.
A default missing lookup can instead install this action on physical0. These supplied-state limits
are not naturally observed bugs or evidence of capacity safety.

The action sets speed32, disables acceleration/deceleration and obstruction, enables map/entity
uncollidability, then loops through follow/destination wait. Its movement consumer acts when the
larger coordinate separation exceeds `$1E0` internal units; it derives destination from the target's
destination/facing and offsets, checks layout with fallbacks, and writes travel/velocity. Installation
is not immediate placement or completed following. The scene has no final distance/all-actor wait.

No Rick follower flag is written. F12/F44 record membership; F256 suppresses this conversation;
the current physical list/action stores following. Fresh normal-table population has no Rick row
among its persistent followers. `InitializeFollowerActscripts`, which writes consecutive physical
slots from current follower flags, is a separate routine and does not rebuild/resolve this list.
Neither that routine nor fresh follower initialization is called by Rick's scene.

## Completion, Partial Execution and Reentry

**Confirmed:** priority/facing/first-jump changes precede membership; membership precedes the last
speech/two jumps, Bowie priority write, following and caller F256. This is not an atomic transaction
or rollback procedure. Any completed prefix may remain when a later service fails to finish.
F518/F65 are not changed, and no warp, Sword transaction, Caravan operation or transport-mode
normalization occurs.

After scene end, the script interpreter conditionally waits for scrolling when its dialogue-window
index is nonzero, clears scroll speed and restores registers. Trap6 returns; the handler sets F256;
the wrapper restores saved Rick facing, closes windows and enables updates; ordinary action returns.
`WaitForEvent` retargets view to physical0 and installs controlled action only with pending event zero.
PLAYER_TYPE0 selects ordinary walking; other supplied modes retain their shared selection. Return
order is established statically; responsive input, settled followers and a next journey are Unknown.

| Supplied state/boundary | Conditional consequence |
| --- | --- |
| Fresh F518 set/F12 clear | Fixed Rick is declared and left in place; fresh F256 is clear. Ordinary proximity/reverse lookup must still select him. F65 need not be set. |
| Fresh F518 clear/F12 clear | No Rick actor is supplied by the setup. A shared event row alone does not make him interactable. |
| F256 set/F12 clear | Handler suppresses recruitment despite missing membership; admitted wrapper work still occurs. |
| F256 clear/F12 set | If reached, the handler can replay the whole scene. There is no already-joined shortcut; init/population and talk exclusion are separate obstacles. |
| Same-population completed scene | F12 set, F44 conditionally set, current following installed, then F256 set. The listed physical actor is excluded from ordinary activation; a direct handler call is separately suppressed by F256. |
| Stopped after joining | Membership can have changed while follow installation and F256 remain unfinished. A retry does not undo that prefix. |
| Fresh F518/F12-set entry | Rebuild discards the old actor/list/action population and clears temporary flags. Rick's fixed row is declared then moved away by F12; clearing F256 does not restore ordinary visibility or permanent following. |
| Existing-population exploration entry (-1) | List/mappings/F256 are not reset. Init still moves mapped Rick off-map because F12 is set without stopping his follow action; later effects remain unproved. |
| Fresh F518-clear/F12-set entry | Unfilled lookup12 aliases main0, which init moves off-map. This does not establish a natural failure or safe usable control. |
| Full party, HP0, missing Peter or changed list | The separate capacity, HP, lookup and current-target rules apply; no shared eligibility check repairs them. |

Before either exploration-entry variant, the shared healing consumer restores living allies and
immortal Peter/Lemon, skipping ordinary dead Rick. That is an exploration service, not recruitment
healing. An explicit-map entry rebuilds actors, clears F256–383 and sets F80; entry with map -1
updates main/raft state instead and skips the population/temp reset. Both subsequently run current
map init. Calling exploration again therefore does not invariably reset Rick's conversation or
current following. Transport remains independently supplied. This conditional boundary ends the guide.

## Evidence, Executable Owners and Review Boundary

This Layer B explanation consumes accepted [#848](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/848)
under [#851](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/851).
The [research agreement/provenance](../../research/hassan-rick-recruitment.md#agreement-and-provenance)
and [reproduction/coverage owner](../../research/hassan-rick-recruitment.md#reproduction-and-coverage-limits)
retain the selected source joins at SF2DISASM master
`c834c652b6862bc5679fd7f69a38a7093206efc6`, USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`,
scene source134–162, all29 commands/nine texts, complete immediate consumers and generic446.
No source reinvestigation or new runtime acquisition is performed for this guide.

| Explained boundary | Contract, exact fixture and selected fields | Limit |
| --- | --- | --- |
| Setup/actor/init | [setup](../contracts/map-setup-data.md), [entities](../contracts/map-entity-data.md), [exploration](../contracts/map-exploration.md); `sf2-map-setup-static-v1` [setup fixture](../../../tests/fixtures/h2/map-setup-static-v1.json), `sf2-map-entities-static-v1` [entities fixture](../../../tests/fixtures/h2/map-entities-static-v1.json), `sf2-map-init-static-v1` [init fixture](../../../tests/fixtures/h2/map-init-static-v1.json) | Init `expected.primarySourceBodies[sourceOwnerSymbol=ms_map9_InitFunction]` has13 ordered operations. Selected actual setup lists, enum/lookup/init consumers and fresh/-1 callers are research-owned joins, not a natural visit capture. |
| Ordinary interaction and temporary guard | [exploration](../contracts/map-exploration.md); `sf2-map-events-static-v1` [entity shard](../../../tests/fixtures/h2/map-events/entity-programs.json) | `expected.entityTargetPrograms[canonicalSymbol=Map9_EntityEvent1]`, program672/address0x56842, contains5 operations, one physical row/two setup references. Exact handler invocation does not prove ordinary actor admission. |
| Scene speech/action/facing/priority | [dialogue](../contracts/dialogue-system.md), [exploration](../contracts/map-exploration.md); `sf2-map-script-engine-static-v1` [script fixture](../../../tests/fixtures/h2/map-script-engine-static-v1.json) | `dialogueCommandFacts`, action/gesture/lifecycle fields retain shared handlers/sites; retained `programCorpus.programs[id=cs_56AE6]` records6–34 supply complete scene order via research. The fixture's13 dialogue commands include Rohde's prefix and two cursors; they are not Rick's nine speech sites. |
| Join versus active-list snapshot | [membership](../contracts/party-membership-state.md), [roster](../contracts/party-roster-state.md); `sf2-common-stats-static-v1` [stats fixture](../../../tests/fixtures/h2/common-stats-static-v1.json), script `forceStateCommandFacts` | Script join site27/source155 has Rick12. Shared membership contract retains symbolic ranges/capacity and rebuild-before-active-write; Rick's F12/F44, threshold12 and HP/input consequences use the accepted selected research, not expanded contract claims. |
| Current following | [roster](../contracts/party-roster-state.md), [accepted following research](../../research/polca-oddler-choice-and-following.md); script `forceStateCommandFacts.activePartyCommandFacts` | Rick add site33/source161 is separate from `followEntity`. Shared handler facts and bounded direct-handler observations do not prove permanent Rick following, natural target order or scene completion. |
| Conditional control/request return | [exploration flow](../contracts/exploration-control-flow.md), [entry routing](../contracts/map-entry-routing-state.md) and [request research](../../research/map-event-request-consumption.md); `sf2-map-event-request-consumption-static-v1` [request fixture](../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json) | Shared request-access facts do not execute their consumer callees; selected control order remains research-owned, without actual Rick acknowledgement, responsive control or persistence. |

Schema/consumer pairs are [setup schema](../../../schemas/h2-map-setup-static-fixture.schema.json)/[map_setup.py](../../../src/sf2tool/h2/map_setup.py),
[entities schema](../../../schemas/h2-map-entities-static-fixture.schema.json)/[map_entities.py](../../../src/sf2tool/h2/map_entities.py),
[init schema](../../../schemas/h2-map-init-static-fixture.schema.json)/[map_init.py](../../../src/sf2tool/h2/map_init.py),
[entity-section schema](../../../schemas/h2/map-events-entity-programs.schema.json) and
[shard schema](../../../schemas/h2/map-events-fixture-shard.schema.json)/[map_events_fixture.py](../../../src/sf2tool/h2/map_events_fixture.py)
with [map_events.py](../../../src/sf2tool/h2/map_events.py),
[script schema](../../../schemas/h2/map-script-engine-fixture.schema.json)/[map_script_engine.py](../../../src/sf2tool/h2/map_script_engine.py)
with [component schema ownership](../../../src/sf2tool/h2/map_script_schema.py),
[stats schema](../../../schemas/h2/common-stats-fixture.schema.json)/[stats.py](../../../src/sf2tool/h2/stats.py)
and [request schema](../../../schemas/h2/map-event-request-consumption-static-fixture.schema.json)/[map_event_request_consumption.py](../../../src/sf2tool/h2/map_event_request_consumption.py).
The existing narrow commands are `uv run sf2 h2` followed by `map-setup`, `map-entities`, `map-init`,
`map-events`, `map-script-engine`, `common-stats` or `map-event-request-consumption` respectively.
These identify executable owners, not extra evidence runs required by prose. Any controlled command
loads the ignored worktree configuration in the same process.

Pre-synthesis review checks accepted research, contract boundaries and bounded fixture/schema/consumer
selections for independent flags, physical versus logical identities, command versus text counts,
HP guards, destination-facing, current-input acknowledgement, roster snapshots, duplicate/self-target
following and partial/fresh/-1 execution. No Layer A repair is required. Stricter selected Rick joins
control where aggregate contracts deliberately leave numeric capacity or follower lifecycle Unknown.
The supplied-state table is a discriminating static review matrix, not executed natural gameplay.

[PR849's final handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/849#issuecomment-6161609727)
and [independent acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/849#issuecomment-6161628177)
preserve complete source/fixture joins, retained comparisons, CI and corrected/failed inspections.
[PR850's rebased handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/850#issuecomment-6161661260)
and [acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/850#issuecomment-6161671119)
preserve its original/rebased results and predecessor histories. These accepted checks are not
replayed solely for this synthesis, and no completed failure is relabeled or fixture weakened.

**Unknown:** natural visit/talk reach, actual flags/HP/class/active roster/mappings/list order,
input/acknowledgement, action/position/priority/sprite/audio and other service completion, usable
control and save/resume persistence. Aggregate static facts and shared handler observations do not
prove a naturally composed recruitment scene. These gaps do not automatically admit runtime work
under [ADR0014](../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md) or
[ADR0016](../../decisions/0016-remake-start-evidence-deferral.md).
