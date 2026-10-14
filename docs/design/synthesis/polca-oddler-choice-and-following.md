# Polca's Oddler Choices and Following

Polca offers two requests to take Oddler along. In the first, the mayor asks the group to
care for him, and Peter asks Bowie to decide. Refusing ends that exchange. In the second,
Oddler asks for himself: Yes reaches the shared acceptance exchange, while No leads to a
longer appeal that also ends with the group taking him. There is no second prompt in that
appeal. The common conclusion requests a new following chain and changes future follower
flags; it does not recruit Oddler or Luke into the force.

**Confirmed static content and control flow:** the accepted
[research owner](../../research/polca-oddler-choice-and-following.md) supports the callers,
attributed dialogue, branch graph and ordered effects below. **Inferred:** the exchange depicts
a reluctant group accepting responsibility for Oddler. **Unknown:** natural access, actual
input and actor state, completed services, presentation and persistence. Fnnn means flag nnn;
coordinates are map tiles unless another unit is named.

Read [Battle13 outcomes through Polca's raft grant](battle13-outcomes-through-polca-raft-grant.md)
for Luke accompanying the party, the changed town and the mayor's bargain. This guide consumes
that accepted predecessor and stops after the two Oddler callers and their conditional control
returns. Actual departure, boarding/river travel, Battle14, Luke recruitment and later Oddler
story remain outside its boundary. Continue with
[Polca departure and Luke recruitment](polca-departure-through-luke-recruitment.md) for the selected
south warp, independent init guard and explicit joining through conditional exploration control.
That continuation requires neither Oddler following nor a raft and proves no natural route order.
This guide adds no remake capability or product decision.

## Which Request Can Run

**Confirmed caller selection:** all four Polca setups share the same zone table. Its rows are
X24/any Y, any X/Y11, any X/Y30, then default. X24 wins at both `(24,11)` and `(24,30)`.
Even when its selected handler returns without a scene, scanning does not resume at the Y row.
The earlier X24 confrontation/arrival conditions remain with the predecessor research.

The movement service must produce a type6 zone request, not merely leave an actor at those
coordinates. It derives X/Y from destination plus direction offsets after recognizing the
`$1400` map marker. `ProcessMapEvent` clears pending before dispatch; type6 installs `eas_Init`
on physical0, then passes the request's X/Y to the current setup's ordered zone dispatcher.
These source connections prove neither a legal walking route nor natural scene order.

| Selected caller | Independent admission | Effect after its script returns |
| --- | --- | --- |
| Y11: `Map13_ZoneEvent1`, `0x5827C` | F716 set and F717 clear. No F718 test. | Run `cs_58AE2`, then set F717 and return. |
| Y30: `Map13_ZoneEvent2`, `0x58294` | F716 set and F718 clear. No F717 prerequisite. | Run `cs_58BEA`, then set F718 and return. |
| Default: `Map13_DefaultZoneEvent`, `0x582AA` | No specific row matched. | Return only; this is also Y30's final return instruction. |

Neither Y caller requires F513, F64, F68, F75, membership, HP or a previous contact.
F716 is the mayor caller's completed-conversation flag, but supplying it does not prove that
conversation or a valid raft tuple. F717 is set after either completed Y11 answer, despite
its source comment about Peter declining. F718 likewise does not identify a particular choice.
Flags select behavior; they are not receipts for prior scenes.

Fresh setup selection remains F513 > F799 > F710 > default. F513's thirteen regular NPC rows
include mayor137 `(15,3)` Up and Oddler140 `(14,4)` Right, both `eas_Init`. Peter7 and Luke10
are follower declarations under F67/F75 rather than fixed rows. With only those two flags,
Peter/Luke occupy physical 1/2 and mayor/Oddler physical 12/15; other supplied followers shift
those slots. Writing a flag without rebuilding does not establish this population or repair
current mappings. Shared init checks F711, F716 and F718 independently in that order: F711
clear stages 130; F716 set faces 137 Down; F718 set moves 140 off-map without deleting its mapping.

The graph has **two physical corpus programs**, `cs_58AE2` and `cs_58B7A`, containing 36 and
125 records. Five named entry regions divide those records; `cs_58BEA`, `cs_58D4C` and
`cs_58DD2` are internal positions in the second program. They are not five natural scenes.

| Choice, assuming each required service returns | Connected regions | Completion writes |
| --- | --- | --- |
| Y11 Yes | `58AE2` → `58B7A` → `58D4C` → `58DD2` | Internal F68 → clear F75 → F718; caller then F717. |
| Y11 No or B cancel | `58AE2` refusal tail → script end | Caller F717 only; preserves supplied F68/F75/F718 and following. |
| Y30 Yes | `58BEA` → `58D4C` → `58DD2` | Internal F68 → clear F75 → F718; caller sets F718 again. |
| Y30 No or B cancel | `58BEA` longer appeal → `58DD2` | Same internal writes and redundant caller F718; no further prompt. |

Y11 refusal leaves Y30 eligible while F716 remains set and F718 clear. This is an available
source branch, not a promise of a later natural prompt. Y30 can run with F717 clear. Conversely,
F718 set/F717 clear suppresses Y30 while still allowing Y11, even with an already rebuilt
Oddler follower or off-map NPC140. The callers do not repair such supplied states.

## How the Yes/No Prompt Works

**Confirmed prompt semantics:** script `yesNo` calls `csc11_promptYesNoForStoryFlow`, which
uses the ordinary input-aware `j_YesNoPrompt` / `YesNoPrompt` entry, preserving A6 around the
call. The prompt starts with **Yes selected**. It snapshots `PLAYER_1_INPUT`, opens the prompt
and moves existing dialogue/gold windows where present. A nonzero entry snapshot requires
release to zero before the choice loop; a zero snapshot bypasses that release wait.

The loop checks `CURRENT_PLAYER_INPUT` in order **Left, Right, B, C, A**. Left selects Yes 0;
Right selects No `$FF`. Directions do not confirm and take priority over buttons in the same
sample. B selects No and exits; C/A confirms the selected byte. Initial Yes still requires
confirmation. No input continues waiting/animation/service work without timeout, automatic
acceptance or a third returned result.

After closing/deleting the prompt, the selected byte returns sign-extended: word 0 for Yes,
`$FFFF` for No/B. The script handler tests that word, sets **F89 for zero/Yes**, clears F89 for
nonzero/No, then calls Sleep10 before returning. `jumpIfFlagSet F89` takes the acceptance
target when set. A prior F89 value cannot answer a pending prompt. Window completion, result
mutation, the ten services, script continuation and caller return remain separate boundaries.
Actual sampled inputs, release timing and visible selection are **Unknown**.

## The Mayor's Request at Y11

**Confirmed requested staging:** `cs_58AE2` starts cursor 1517 and requests all eleven texts
1517–1527 across its alternatives. It first moves the camera to `(16,8)`, places mayor137
`(17,7)` Right and Oddler140 `(16,7)` Right, then moves the camera to `(10,4)` after the
interruption. The mayor turns Left, waits 50, turns Right, waits 30 and later turns Left again.
A sleep20 separates his request from Luke's criticism.

| Text IDs | Attributed speaker | Paraphrased content |
| --- | --- | --- |
| 1517–1518 | Mayor137 | Interrupts the group and tentatively asks a favor. |
| 1519 | Mayor137 | Asks them to take the boy along. |
| 1520–1521 | Luke10 | Accuses the mayor of laziness and avoiding the boy's care. |
| 1522 | Mayor137 | Hesitates in response. |
| 1523–1524 | Peter7 | Addresses the leader and asks for a decision. |
| 1525 | Peter7, refusal | Agrees that the journey would burden a blind boy. |
| 1526 | Peter7, refusal | Apologetically declines the request. |
| 1527 | Mayor137, refusal | Protests their refusal. |

Before Peter's question, the script explicitly places Bowie `(22,12)` Right, Peter `(23,12)`
Left and Luke `(23,11)` Down, sets camera `(16,6)` and waits 30. Yes branches to `cs_58B7A`.
No/B instead reaches Peter's refusal, sleep30, Bowie/Peter/Luke facing Up, his apology, camera
`(15,2)`, mayor Right and the protest, then ends. It never enters the common following tail.
Mayor texts use `$0,137`; Luke uses `$C0,ALLY_LUKE`. Peter's prompt/apology use `$80`, while
1525 uses `$0`. Text operands attribute speech without proving a live actor or a diagnosis.

**Confirmed Yes bridge:** `cs_58B7A` resets the cursor to 1528. Peter's `$80` response precedes
Oddler's waited speed `(10,10)`/idle body. The mayor starts Right3/faceLeft1 without a wait;
Oddler waits Right3, followed by sleep20. The script starts that mayor action again and waits
Oddler Right3 again, then faces 137/140 Down and Bowie/Peter/Luke Up. The second mayor action
can replace the first before completion.

| Text IDs | Attributed speaker | Paraphrased content |
| --- | --- | --- |
| 1528 | Peter7 | Expresses surprise at the leader's kindness. |
| 1529 | Mayor137 | Thanks the group. |
| 1530–1531 | Mayor137 | Tells Oddler the leader will take him and can be trusted. |

After 1529 the mayor faces Left, but the next facing command targets **FOLLOWER_B31 Right**,
not NPC140. No preceding bridge command creates 31 or aliases it to 140. Without an admitted
31 declaration its default mapping may resolve physical0; F74/F70 can instead map 31 to Astral.
The nearby speech does not change that operand. Only after 1530–1531 does NPC140 face Right,
then the script jumps to the shared acceptance region `cs_58D4C`.

## Oddler's Own Request at Y30

**Confirmed independent staging:** `cs_58BEA` starts cursor 1541. Oddler140 calls out, is placed
`(14,21)` Down, camera moves to `(10,18)`, and he waits Down3 before asking to accompany them.
Peter then Luke receive waited `eas_Init`. The script places Bowie `(14,30)`, Peter `(15,30)`
and Luke `(13,30)`, all Up, and selects camera `(10,22)`. It resets the cursor to 1523 for
Peter's **same two prompt IDs**, then runs the prompt. Yes jumps directly to `cs_58D4C`.
The different Y11 positions are not inherited or normalized.

| Text sites | Attributed speaker | Paraphrased content |
| --- | --- | --- |
| 1541–1542 | Oddler140 | Calls for a wait, then asks to accompany the group. |
| 1523–1524, repeated here | Peter7 (`$80`) | Again addresses the leader and asks for a decision. |
| 1543, No/B path | Oddler140 | Reacts with confusion. |
| 1544–1545 | Peter7 (`$80`) | Warns him and urges caution. |
| 1546 | Oddler140 | Agrees with that warning. |
| 1547–1548 | Oddler140 | Says devils will return while he stays, and suggests the leader could resist them. |
| 1549 | Peter7 (`$80`) | Fears that he could be injured. |
| 1550–1552 | Luke10 (`$0`) | Favors taking him, fears he could be killed, and appeals to the leader. |
| 1553–1555 | Peter7 (`$80`) | Says the boy needs a guardian, offers to care for him, then urges departure. |
| 1532, at the appeal's end | Oddler140 | Thanks the group. |

All Oddler speech uses `$0,140`. The longer No/B path resets cursor 1543 and hides text before
his confused response. It waits 140's speed10/idle body, faces 137 Down, waits 140 Down3 and faces
him Up. A waited body turns animation OFF, sets orientation Down, updates the sprite and idles;
`removeShadow 140` follows. After sleep30 Bowie and Peter each start Up2 without waiting,
while Luke waits Up2. Peter's warnings precede 140's waited init, Down facing, headshake, nod
and agreement. Sleep50/Up/sleep50/Down staging precedes Oddler's account of the devils.

Peter then waits Up1, faces Left and 140 Right for his injury concern. Oddler faces Down,
Bowie Left and Luke Right before Luke's appeal. Peter waits Down1, faces Left and Bowie Right
before offering guardianship; then Peter waits Up1, faces Left, 140 Right and Bowie Up before
urging departure. Finally cursor 1532, a nod, thanks and 140 Down precede a jump to `cs_58DD2`.
This path **bypasses** `cs_58D4C` and contains no new prompt. Its narrated agreement and care
remain distinct from engine membership or healing.

## Shared Acceptance and Conclusion

**Confirmed shared acceptance:** Y11 Yes and Y30 Yes both reach `cs_58D4C`. Cursor 1532,
hideText, sleep20 and a nod precede Oddler's thanks. He receives waited speed10/idle; mayor137
faces Down; 140 waits Down2, faces Up, receives the animation-OFF/orientation/sprite/idle body
and `removeShadow`. Sleep30 precedes nonwaited Bowie/Peter Up2 and waited Luke Up2; sleep40
precedes Peter's warnings. Oddler then waits init, faces Down, headshakes, nods and agrees.
This falls through to the common conclusion without another prompt.

| Text sites | Attributed speaker | Paraphrased content |
| --- | --- | --- |
| 1532, acceptance site | Oddler140 (`$0`) | Thanks the group; the No/B appeal has a separate site for this same ID. |
| 1533–1534 | Peter7 (`$80`) | Warns him and urges caution; wording repeats 1544–1545 at different IDs. |
| 1535 | Oddler140 (`$0`) | Agrees; wording repeats 1546 at a different ID. |

**Confirmed common conclusion:** `cs_58DD2` resets cursor 1536 and requests five texts:

| Text ID | Attributed speaker | Paraphrased content |
| --- | --- | --- |
| 1536 | Oddler140 (`$0`) | Identifies himself and says he was nearly dead when he arrived. |
| 1537 | Oddler140 | Says he does not remember. |
| 1538 | Oddler140 | Says he does not know why the devils pursue him. |
| 1539 | Oddler140 | Fears causing trouble for the group. |
| 1540 | Narrator (`$FF,ENTITY_NONE`) | Announces the leader's decision to take Oddler along. |

After sleep30, the script requests **Peter→Bowie, NPC140→Peter, Luke→NPC140**, all selector2,
then sets F68, clears F75, sets F718 and ends. The caller's F717 or redundant F718 comes later.
There is no `joinForce`, membership/active-party change, heal, HP write or force-AI write in
any of the five regions. Blindness, injury risk, near-death, amnesia and pursuit are attributed
speech, not medical/combat state or proof of later plot. The narrator's text is not a receipt
for completed following, later writes or durable persistence.

The tables preserve **42 text sites / 39 distinct IDs**, spanning 1517–1555 (`05ED..0613`).
The region site counts are 11/4/18/4/5. Explicit cursor writes in physical order are
1517,1528,1541,1523,1543,1532,1532,1536. Prompt 1523/1524 and thanks 1532 each occur twice;
repeated wording at 1533–1535 and 1544–1546 remains separate IDs. A complete inventory is not
one execution through every text. `nextSingleText` closes text/portrait and requests Sleep10;
`nextText` retains its own continued-dialogue lifecycle and controls.

## Actions, Health and Visible Staging

**Confirmed service limits:** placements scale tile bytes by 384 internal units and write both
position/destination and facing; facing commands also update the sprite. Their packed entity
byte drives an ally word-HP check: Bowie0/Peter7/Luke10 can be skipped when that ally has zero
HP, while NPC137/140 and lookup 31 bypass that test. Mapping resolution adds no presence guard.
This byte encoding must not be substituted for the follow command's high-byte rule below.

Action installers replace pointers; a waited form polls only its chosen actor for `eas_Idle`.
Waiting for 140 does not join mayor137's nonwaited moves. Waiting Luke Up2 does not join Bowie or
Peter Up2. Later action/follow installation can replace unfinished work. Speed10 is an internal
per-axis parameter, not ten tiles per frame or health. Actual achieved positions are Unknown.

Animation OFF sets counter `-1`; it proves neither immobility nor injury. Orientation Down is
separate from facing. `eas_Init` restores speed 32, movement/collision/auto-facing parameters,
orientation Up, immersion and size 24, then updates the sprite and idles; it is not healing.
Headshakes/nods transform and refresh sprite data with animation-counter and VInt/Sleep work.
The 21 inline bodies are 5 in the Yes bridge, 10 in the Y30 region and 6 in shared acceptance.
There is no `shiver` opcode here; the mayor's shivering belongs to the predecessor bargain.

Both `removeShadow 140` sites resolve the current mapping and load a mapsprite working copy,
transform it, queue DMA, then wait one VInt. The immediate consumer rearranges 576 bytes in
two passes, clearing selected color2 nibbles and `$2222` words; DMA queues 288 words to the
mapped actor's VRAM region. This changes sprite data, not a persistent shadow flag, HP or actor
existence. Later init/facing/gesture refreshes reload ordinary sprite data; permanent shadow
removal across rebuild is not established. Actual pixels and DMA completion remain Unknown.

Camera, text, prompt, gestures and termination each own services. The five regions' standalone
sleep totals 160/20/130/90/30 exclude prompt Sleep10 and other waits; branches skip parts.
Their inventory sum 430 is not a single played duration. Neither a global actor barrier nor
completed rendering/audio/input follows from those counts or a script terminator.

## Current Following and a Later Rebuilt Population

**Confirmed current requests:** follow words are `$0007,$0000,$0002`, `$008C,$0007,$0002`
and `$000A,$008C,$0002`. `csc2C_followEntity` reads the **high byte of the first word**, zero
in all three, before `AdjustScriptPointerByCharacterAliveStatus`. Every call therefore checks
**Bowie0's word HP**, including NPC140's call. Zero skips six operand bytes; nonzero permits
mapping resolution regardless of Peter/Luke HP. It never tests Oddler's health.

Both follower and target resolve through current `ENTITY_INDEX_LIST`. There is no target
HP/presence guard. A missing default-zero mapping can resolve physical0; an explicitly
invalidated `$FF` is another invalid state, also unrepaired. These are lookup identities,
not fixed physical slots. The intended chain requires supplied valid mappings.

`AddFollower` copies/patches a 42-byte action, replaces the current pointer, advances allocation
and appends the physical index to `EXPLORATION_ENTITIES` only if absent. Existing entries stay
in order. Selector2 supplies `(-24,0)` pixel offsets; the action consumer derives destinations
and velocities from target state and collision. It neither teleports nor waits for settling.
Thus an existing Peter,Luke list can become **Peter,Luke,140**, while target pointers describe
**Peter→Bowie,140→Peter,Luke→140**. List order and target chain are different state.

F68 set/F75 clear does not rebuild those current actors, remove Luke, remap 140 to 31 or change
membership F7/F39/F10/F42. Zero Bowie HP can skip all following while flag/caller writes still
complete. Other already-active followers are not normalized.

On a later **fresh** initialization, follower declarations run in table order independently
of force membership. Polca permits them and supplies no overworld caravan prefix. F67 declares
Peter; F68 declares Oddler's sprite under **FOLLOWER_B31**, after earlier Sarah/Peter/Kazin/
Slade/Astral/Chester rows. Cleared F75 omits Luke's row. F513 regular NPC140 is separately
recreated, then F718 init moves 140 to internal `$7000,$7000` position/destination without
removing its mapping or transferring it to 31.

With only F67/F68 set, Peter/31 occupy physical 1/2 and F513 NPC140 physical 15 before that init
move. Luke has no fixed F513 row to retain him. Other follower flags change allocation and
can reuse 31; later F70 can overwrite it with Astral. No universal two-follower population,
conflict repair or later-map/save/load outcome is established. A transition without rebuilding
has a different boundary from this fresh population.

## Sequential Writes and the Two Control Returns

**Confirmed partial-state boundary:** the operations are sequential. A prompt can be pending
without a new F89 result. After return, choice state can exist before later services finish.
Each following pointer may be installed before the next; all precede F68 → clear F75 → F718.
A stalled service can leave a prefix without rollback. Internal F718 can precede caller F717
at Y11 or the redundant caller F718 at Y30; no transaction or mismatch repair is supplied.

| Supplied state | Selected consequence |
| --- | --- |
| F716 clear | Both Y callers return without a scene. |
| F716 set, F717 set, F718 clear | Y11 suppressed; Y30 admitted, without proving Y11 refusal. |
| F716 set, F717 clear, F718 set | Y30 suppressed; Y11 admitted despite future/off-map actor conditions. |
| F717/F718 both set | Both suppressed; following, raft, actors and membership are not validated. |
| F68/F718 disagree | Future follower construction, NPC init and Y30 eligibility follow their own predicates. |
| Completed Y11 refusal | Caller sets F717 only; supplied F68/F75/F718 and current following stay as supplied. |
| Completed common conclusion | Internal F68 set/F75 clear/F718 set; caller F717 or F718 follows. No new raft/warp request. |

After either selected handler, the zone wrapper closes portrait, closes dialogue, waits one
unconditional VInt, then compares **physical0** position with destination and waits/rechecks
until equal. It does not wait for every other actor. The nested script trap activates entity
updates before execution; this zone path does not inherit the mayor entity-contact wrapper's
facing/suppression rules. Its services still require the supplied live state and actual returns.

Type6 returns to exploration. `WaitForEvent` retargets camera0 and sets the controlled entity's
action only when pending is zero; ordinary player type0 selects character control. The scripts
queue no warp or other pending event, but another supplied pending/player state still matters.
**Stop after each caller and this conditional control return.** Neither branch is an observed
responsive-input receipt, automatic Polca exit, river access or recruitment.

## Evidence Review and Remaining Limits

This guide consumes research accepted in
[PR #809](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/809) and the accepted
predecessor guide from main. Research owns USA retail identity and pinned SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6`, Map13 script lines 514–727, all 161 records,
21 inline bodies and the text/caller/consumer comparisons. Its
[reproduction boundary](../../research/polca-oddler-choice-and-following.md#reproduction-and-acceptance-limits)
retains those checks. This synthesis reviews accepted prose, contracts and bounded tracked
fixture selections without original-source reinvestigation, payload copying/generation or runtime.

| Reviewed boundary | Existing evidence surface | Disposition |
| --- | --- | --- |
| Y callers | [sf2-map-events-static-v1:zone-programs](../../../tests/fixtures/h2/map-events/zone-programs.json), programs 10/11/12; [zone schema](../../../schemas/h2/map-events-zone-programs.schema.json), [loader](../../../src/sf2tool/h2/map_events_fixture.py). | Seven operations per Y caller preserve independent predicates, script then write; default shares the final return. Accepted research owns row priority and natural-request limits. |
| Prompt | [sf2-common-menus-static-v1](../../../tests/fixtures/h2/common-menus-static-v1.json), `expected.menuFacts.yesNoPrompt`; [menu verifier](../../../src/sf2tool/h2/menus.py), [choice contract](../contracts/map-exploration.md#source-bound-choice-consumer). | Initial0, Yes 0, No/cancel-1, directions and A/C agree. Accepted input research supplies release, priority and pending behavior; compact fields are not a timing observation. |
| Branches, actors and sprite services | [sf2-map-script-engine-static-v1](../../../tests/fixtures/h2/map-script-engine-static-v1.json), story/action/placement/lifecycle/relationship families; [map-script verifier](../../../src/sf2tool/h2/map_script_engine.py). | Command-family facts do not contain the complete scene corpus or prove natural presentation. Research owns all sites, inline composition, high-byte HP and current/future mappings. |
| Fresh init | [sf2-map-init-static-v1](../../../tests/fixtures/h2/map-init-static-v1.json), body 18; [init verifier](../../../src/sf2tool/h2/map_init.py), [party/roster](../contracts/party-roster-state.md). | Thirteen operations preserve separate 711/716/718 branches and coordinate-only move of 140. No follower/member equivalence or rebuild receipt. |
| Caller/control | [zone contract](../contracts/dialogue-system.md#source-zone-caller-through-return), [request-consumption fixture](../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json). | Window/VInt/physical0 destination order differs from script return and pending-gated player setup; no global actor or input-ready claim. |

The [menu schema](../../../schemas/h2/common-menus-fixture.schema.json),
[init schema](../../../schemas/h2-map-init-static-fixture.schema.json) and
[map-script schema composition](../../../src/sf2tool/h2/map_script_schema.py) bound fixture shapes.
Focused existing checks are `tests/python/test_map_events.py`, `test_common_menus.py`,
`test_map_init.py` and `test_map_script_engine.py`; narrow routes include `uv run sf2 h2 map-events`,
`common-menus`, `map-init`, `map-script-engine` and `map-event-request-consumption`.
These are evidence navigation, not newly executed documentation gates.

**Unknown:** natural Y11/Y30 reach/history; actual flags, HP, mappings, membership/deployment/AI;
concurrent positions/action completion; release/input samples; text/camera/gesture/shadow/audio
presentation and service returns; responsive control and save/load persistence. They do not block
this static guide or authorize acquisition. Accepted research retains
[PR805's handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/805#issuecomment-6159053815),
its superseded CI, scheduling-gap watcher exit and resolved navigation failures, plus linked earlier
failure history. None is erased or ritually rerun. This guide claims neither continuous playability
nor campaign/epic completion.
