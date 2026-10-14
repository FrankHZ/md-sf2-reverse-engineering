# New Granseal Return and Astral Following

Back at New Granseal Castle, the minister welcomes the party and Astral asks
about its new allies. Peter says Elis is alive. Bowie and Peter then explain
their experiences, leading the king to call for Zeon's defeat and Elis's
rescue. Astral proposes seeking knowledge in Tristan and asks to accompany
the party as adviser. The scene announces that role, then changes current
following and records flags for subsequent castle visits.

**Inferred:** this is a retrospective audience that brings Astral into the
journey as adviser. **Confirmed:** the report, announcement, current actor
relationship, caller flags and later follower reconstruction are separate
operations. The scene does not rescue Elis, recruit a battle ally or travel
north. **Unknown:** natural arrival, actual supplied state, completed
presentation, usable control and save persistence.

This continues [Creed departure, dwarf recovery and the Cannon gift](creed-departure-dwarf-recovery-and-cannon-gift.md)
from [accepted #965 research](../../research/new-granseal-return-and-astral-following.md)
and predecessors on main `af7a1e656eddc4a0252b9f5cee4bbe31288369a3`.
The guide ends at the audience's conditional caller return and relevant
current/fresh-entry consequences. Janet, the inventor's gift and the North
Cave blast are later boundaries. Concurrent #969 research supplies no evidence
or dependency here. This Layer B explanation selects no remake rule or new
observation work.

## Returning from the Mine

**Confirmed:** the selected connections are mine interior Map30 → Southwest
Parmecia Map68 → Map73 → town Map6 → castle Map7. The
[route owner](../../research/new-granseal-return-and-astral-following.md#selected-return-route-and-admission)
retains exact rows, request consumers and coordinate premises. These are
conditional alternatives, not a mandatory itinerary or an observed walk.
Cannon possession and F761 are not prerequisites for these rows or the audience.

Map30's outgoing row at any X/Y21 requests Map68 `(57,25)` Down without
scrolling. That no-scroll return can encounter Map68's Battle19 admission:
F419 selects it, and an already set F519 clears F419 only after selection,
still returning Battle19 for that call. The returned rectangle is battle
geometry, not a containment test. Continued travel assumes no intercept
(normally F419 clear); an admitted battle is a stopping boundary.

Map68's north border at any X/Y0 requests an Up scroll17 into Map73,
retaining X and placing Y at 62. This stays inside exploration: actors and
following are retained, current facing is sampled instead of applying the
row's Up facing, and fresh healing, battle selection, resident construction,
temporary reset and Map73 init are skipped. Map68's random Battle19 zone is
an independent movement seam; Map73's default zone returns. Walking's zone
producer is absent in Caravan mode1, and this route neither boards nor lands
a vehicle or normalizes carried player/raft state.

Map73's row `(29,52)` requests town6 `(20,30)` Up without scrolling; town6's
north edge requests castle7 `(11,22)` Up, also without scrolling. Map73/6/7
have no battle-coordinate row, and none of Maps68/73/6/7 is globally flag-switched.
Other exits or pending events can interrupt the proposed route. Ordinary
movement still needs the appropriate working-layout attributes, collision
admission and a request that survives until dispatch. Supplied coordinate
high bytes also matter to no-scroll byte replacement.

A fresh admitted map rebuilds actors, applies the accepted living/immortal
healing, clears temporary flags 256–383 and pending state, sets F80 and runs
selected init. CURRENT255 reuse and border scrolling retain a different
cohort. Map68 has empty residents and return-only init. Fresh73 has empty
residents but can run the earlier voyage if F700 is clear, or construction
if F700/F508 are set and F705 clear; the selected scroll bypasses those branches.
The ordinary later-history route carries F700/F508/F705/F707/F701/F702 and Peter's F67.
These are history premises, not extra warp or audience tests.

## Who Is Present on Return

**Confirmed:** [population and init selection](../../research/new-granseal-return-and-astral-following.md#town-and-castle-population-and-init)
depend on older history independently of the return audience. Town Map6 selects
default, then F701, then F702, with the last applicable selector winning.
F802/F805 do not choose its resident list. The earlier default has Peter and
19 NPC declarations; each later list has 14 NPC declarations without fixed
Peter. Their positions/actions differ and remain with the complete research
tables. Flag-selected followers are constructed before residents, so actual
Peter presence is not established merely by selecting a later town setup.

Town's shared init gives F802 priority: it places priest132 at `(11,18)` Down
and returns without checking F706. With F802 clear/F706 set it instead places
worker138 at `(63,63)` Down and guard139 at `(15,23)` Down; both clear returns.
These are current lookup relocations, not hides or persistent population
changes, and none runs the audience.

Castle Map7 selects default, F701, F702→default, then F805. Thus F702 restores the default
choice over F701, while F805 wins over both. Default has 12 NPCs; F805 has the same
first 11 but omits Astral139. F701 has five fixed ally declarations before its
12 residents, including Peter. If Peter is already mapped as a follower,
his fixed declaration is skipped. Default instead needs a supplied Peter
mapping, ordinarily F67. All pointer tables share the normal castle init and
zone table; there is no separate F805 init.

The intended audience actors are minister128, King Granseal129, Astral139,
Peter7 and Bowie0. Default minister/king/Astral declarations are `(12,5)` Down,
`(11,4)` Down and `(10,5)` Down with slower Init. F701 instead gives them
ordinary Init, with Astral at `(10,6)` Right. The full guard/soldier/maid and
ally declarations remain in the research owner; a changed cohort is not
silently equivalent staging.

Shared castle init first checks F707. Set returns immediately, retaining
supplied Egress destination and F411. Clear assigns Egress map6, runs the
[earlier year-later scene and explicit default resident reload](../../research/new-granseal-castle-year-later-control.md#reload-caller-flags-and-first-control-read),
then sets F707 and F411 after that scene returns. Even an F805-selected entry can
therefore be replaced with default residents when F707 is clear. F411 itself
is not an immediate battle request. Ordinary return staging assumes F707 set;
missing history remains a different case.

References are lookup selectors, not physical slots. The resolver masks to
a byte, reads the mapping and multiplies the physical index by 32. Missing
ordinary mappings can remain 0 and alias the main actor; hidden mappings can
be -1 without a safe resolver guard. Duplicate mappings can overwrite each
other's actions, speech context or following. Resident139 and follower B31
remain independent even when both use Astral's sprite. No declaration proves
force membership, HP, completed Init or a serviceable actor.

## Admitting the Audience

**Confirmed:** five explicit castle zone rows, X9–13 at Y8, target
Map7_ZoneEvent0. The [zone owner](../../research/new-granseal-return-and-astral-following.md#complete-castle-zone-admission)
retains their exact shared-default return. Coordinates alone do not invoke
it: ordinary movement must produce the layout zone request, whose consumer
initializes physical 0 and dispatches the selected callback.

| Callback state | Conditional result |
| --- | --- |
| F802 set, F805 clear | Return scene → caller sets F805 → caller sets F70 → return. |
| F802 set, F805 set | Return without the scene or a missing F70 repair. |
| F802 clear, F701 set, F702 clear | Earlier Peter audience → set F702 → set F67 → return; F805 is not tested here. |
| F802 clear, F701 clear or F702 set | Return without either audience. |

F707, F70, Cannon and F761 are not local admission tests. F802 takes priority
over the older701/702 branch; with F802 clear, supplied F805 does not suppress
that older branch. Selecting a callback from changed flags also does not
rebuild the current cast. The following story describes the admitted return
scene with intended mappings and services able to return.

## Welcome and Peter's Report

**Confirmed:** the [complete scene](../../research/new-granseal-return-and-astral-following.md#complete-return-scene-and-action-premises)
begins by initializing Peter with a wait. The minister welcomes LEADER;
Bowie is given destination `(11,7)` without a coordinate wait and Peter
`(10,8)` with one. The camera requests `(6,2)`, and Bowie/Peter face Up.
The encoded word 32768 for Bowie's destination resolves to actor 0 with a
no-wait bit; it is not an additional actor. Neither destination tests HP.

Astral comments on trading allies, the minister welcomes new friends and
praises the party, and Astral reports news from Bedoe: Zeon's revival and
Volcanon's anger. The minister asks about Elis. Peter says she is alive.
The king, Astral and minister receive speed 48/48 custom actions and their
animation is paused across a scene pause and the battlefield-death sound
request. They jump in two groups, each waiting only the minister, then
resume animation. Animation pause is not movement suspension.

The king receives speed 56/56 and a waited Down2,Left1,Down1 path. Peter jumps;
the king asks whether Elis lives, Peter answers hesitantly, Astral asks for
confirmation and Peter confirms. King/Astral/minister then receive speed 32/32;
the king takes the waited Up1,Right1,Up2 path and faces Down. Under normal
completion those king paths cancel, but changed motion/collision or mappings
must not be assigned a universal endpoint.

The king's questions and Peter's answers are dialogue. They neither create
a yes/no menu nor mutate a rescue state. LEADER is the first active force
member's name after UpdateForce and can differ from fixed actor Bowie0;
Peter's inserted name similarly does not prove his presence or life.

## Recounting the Journey

**Confirmed:** the scene calls its palette-remap helper before the retrospective
staging. Peter receives speed 48/48, moves Up1 with a wait, and the cast nods,
jumps and turns as the explanation develops. A later Peter Up1 is unawaited;
the subsequent jump can replace that action pointer. Simply adding all of
Peter's movement operands cannot prove his final position.

The narrator says LEADER and Peter explain their experiences. Peter moves
Left1,Up1 and nods; Astral turns Right, the minister Left, and the king jumps
and nods. Bowie moves Left1,Up1, nods while Astral turns Down, then returns
Down1,Right1 and faces Up. The narration recounts the ancient ruins, Creed
and Zeon's message through Evil Spilit (the source spelling), then marks
the explanation complete. This reports earlier events; it does not replay
those scenes or prove natural traversal of the intervening campaign.

Peter's Down1,Left1,Down1 action is installed without an immediate wait;
the minister faces Down, then the scene explicitly waits Peter's idle
pointer and faces him Up. The palette-restore call follows, then a pause.
The minister expresses amazement at LEADER and Peter and their experiences;
Astral ponders, and the minister addresses him.

The [palette owner](../../research/new-granseal-return-and-astral-following.md#palette-remap-and-restore)
reuses the accepted consultation helpers at these two calls. The first
remaps 64 CURRENT colors into BASE through the accepted color table; it is
not evidence of measured grayscale or sepia appearance. The second copies
the selected COPY snapshot into BASE **before replacing COPY** with the
then-current palette. It does not reload an immutable room palette. Both
clear the automatic-reverse byte and request the shared 32-update interpolation
and CRAM DMA; neither waits for all 32 updates. Pauses/text do not prove visible
completion, and interruption or other palette services can affect the result.

## Choosing the Next Aim and an Adviser

**Confirmed:** Astral asks what to do next. The king calls for defeating Zeon
and saving Elis. The minister receives speed 40/40, steps Left, faces Up and
objects. After a pause Astral turns Up and agrees with the king. The minister
jumps and turns Left in surprise; the king thanks Astral and asks the first
step. The minister moves Right and faces Left again.

Astral proposes North Parmecia and Tristan's storytellers. The minister
shivers and exclaims. Astral turns Right; the narrator describes him winking
and smiling, then he faces Up and requests permission to accompany LEADER.
The wink is narration, not a separate wink action. The king consents and
asks him to save Elis. The minister cautions him about danger; Astral turns
Right, nods and assents. He then faces Down, addresses LEADER and says he
will accompany the party as adviser.

This is a conversation about a next aim, not an instruction that automatically
moves the party there. The announcement requests joining music and narrates
the adviser joining the force. There is no joinForce, joinForceAI,
joinBattleParty, naming or party-capacity operation in this scene. Map
following therefore must not be described as battle-force recruitment.

## Announcement, Input and Current Following

**Confirmed:** [text and immediate services](../../research/new-granseal-return-and-astral-following.md#text-identity-and-service-sequence)
retain 41 ordered sites 1249–1289 without making this guide a transcript.
The ordinary minister has no portrait and uses bleep 5; king portrait 32/bleep 6,
Astral portrait 31/bleep 6, and Peter's portrait/bleep depends on his sprite.
Changed sprites, aliases or an already open portrait window change context.
Narrator operands select speech 0 and bypass portrait resolution; they are
not actor 255. The king's portrait modifier changes options, not actor 129.

Single text waits view scrolling, displays, advances the cursor, then closes
portrait/text and sleeps 10. Continuing text leaves windows open. N means
newline; W1/W2 wait input, with W2's continuation/validation sound behavior.
The final advice has an internal W2 as well as terminal W1; the earlier
continuing Zeon report's terminal W1 remains a wait. No yes/no decision
occurs. Debug/P2 Start can latch text skipping after script entry clears it,
skipping display/cursor advancement and encoded script pauses/waits while
other commands continue. Later displayed text need not keep the nominal
site-to-ID mapping under that latch.

The adviser announcement itself has no W1/W2. Its following direct helper
requests previous music after current music through the sound service, then
waits for player input. Despite its FadeOut name, this helper executes no
palette fade. It accepts any currently held direction/A/B/C/Start, or waits
and retries; a fresh edge or A/C specifically is not required. The sound
poll is an immediate command-service boundary, not proof that a jingle
ended audibly. Skipped narration still reaches this direct helper/input call.

Only after it returns does the scene close text and let Astral advise on
rocks, the northern passage and an explosive. This neither uses the Cannon
nor gives an item or blasts a cave. Bowie nods, then the scene requests
Peter following Bowie with direction2. That word's **high byte** is the HP
guard: Peter's word 7 checks Bowie's HP, not Peter's. Bowie HP0 skips that
relation; Peter HP0 separately skips Peter-facing commands. Destinations,
Init/actions, gestures and the Astral addition have no corresponding HP guard.

The admitted Peter relation installs a follower action with offset `(-24,0)`
and appends his physical actor if absent from the exploration list. Already
listed Peter is not moved to the end. addNewFollower139 then resolves current
Astral and selects the list's **actual last physical entry**, or 0 if empty,
with the same offset. Newly appended Peter can be that target; an existing
Peter with followers after him need not be. The operation does not force a
Peter target, validate list capacity or change joined/active/AI state or F67.
Duplicate suppression happens after action installation. A forced repeat with
Astral already last can install self-following.

Init/custom/inline actions replace action pointers without independently
repairing mapping, position or list order. Custom speeds change packed X/Y
speed bytes, retaining inherited collision/acceleration settings; they grant
no noclip. A named actor's idle wait, coordinate wait, camera-scroll wait or
nod/headshake/shiver service is not an all-cast barrier. Other actors can
continue during text and gestures. These distinctions also apply to the
unawaited staging earlier in the scene.

## Returning Now and Rebuilding Later

**Confirmed:** [following and caller return](../../research/new-granseal-return-and-astral-following.md#following-caller-return-and-changed-state)
leave current Astral as resident139. The scene does not rename him B31 or
reload the cast. It sets F70 inside the script, then ends; only after script
return does the zone caller set F805 and F70 again. Script end conditionally
waits view scrolling and returns, and the zone wrapper closes windows,
waits one VInt and physical 0 coordinate equality. None waits all followers,
acknowledges usable control or performs an atomic save transaction.

On a later ordinary rebuild, F70 selects the B31/Astral follower declaration
before residents. Its physical allocation and leader depend on earlier
enabled follower declarations. Multiple B declarations are not deduplicated:
later F70 can overwrite lookup 31 while earlier F74/Astral or F68/Oddler allocations
remain in the list. Castle Map7 uses the ordinary follower table, without the
Caravan overworld prefix. Independently, F805 selects residents without139.

| Changed state | Consequence |
| --- | --- |
| Fresh F805/F707 set, F70 clear | No resident139 or F70-derived B; F802-set zone skips without repairing F70. Other followers remain independent. |
| Fresh F70 set, F805 clear | B/Astral can coexist with resident139; an eligible audience can append another Astral. |
| F707 clear, even with F805 | Earlier init reloads default residents; preserve that different history/cohort. |
| F701 only with F802 set | Return scene wins despite the older audience not completing; selected positions/actions differ. |
| Missing/hidden/duplicate actors, changed HP or follower order | Actual resolver/guard/list effects replace nominal staging; there is no automatic safe reconstruction. |
| Interrupt before scene F70 / after scene F70 / after caller F805 | No new F70 yet / F70 may precede F805 / F805 may precede repeated F70 and wrapper completion. No rollback repairs a prefix. |
| Direct script call or forced repeat | No F802/F805 test or F805 write inside the script; text/actions/following can repeat. Normal F802+F805 zone reentry skips. |

Janet's town zone has its own F805-set/F806-clear admission. The inventor's
later callback gives F802 priority, tests F804 before F805, and on the relevant
branch searches actual Cannon inventory. Those are separate next boundaries,
not audience prerequisites. Their scenes, item gift and North Cave blast
remain outside this guide. The audience writes no F761/F804/F806, transfers no
item and requests no warp or battle.

**Unknown:** natural route/zone reach; supplied flags, layout, coordinates,
mode, HP, class, population, lookup/list order and serviceable actors;
completed movement/collision/text/portrait/input/view/palette/DMA/audio;
visible retrospective/adviser presentation, usable control and save/load
persistence. These limits do not create an automatic capture or acquisition
queue.

## Evidence Review and Retained Results

The adversarial review checked accepted Layer A prose and selected public
fixture/schema/consumer surfaces. No blocking contradiction was found.
Public handler facts and source joins do not contain or freshly reproduce
the complete private scene corpus. The guide uses the accepted stricter
static boundary throughout.

| Reviewed surface | Disposition |
| --- | --- |
| [#965 research](../../research/new-granseal-return-and-astral-following.md) and accepted route, town, year-later, palette and dwarf predecessors | Complete return story, selected populations/history, Battle19 intercept, actor identities, service/order and changed-state limits agree. |
| `sf2-map-events-static-v1`: [routing](../../../tests/fixtures/h2/map-events/routing-setup.json), [zones](../../../tests/fixtures/h2/map-events/zone-programs.json), [zone schema](../../../schemas/h2/map-events-zone-programs.schema.json) | Five explicit rows plus default, 16+1 callback operations/shared rts and F802/F805 versus701/702 polarity agree. Inventor scope stays at its identified admission boundary. |
| `sf2-map-init-static-v1`: [fixture](../../../tests/fixtures/h2/map-init-static-v1.json) | Selected Map6/7/68/73 bodies retain 8/7/1/13 ordered operations; town F802 priority and shared F707 castle init do not imply fresh scroll init or a separate F805 body. |
| `sf2-map-script-engine-static-v1`: [fixture](../../../tests/fixtures/h2/map-script-engine-static-v1.json), [schema](../../../schemas/h2/map-script-engine-fixture.schema.json) | Selected placement/action/lifecycle/relationship/roster facts retain operand widths, guards, calls and source identity. A legal handler fixture is not proof of natural scene completion. |
| [Event loader](../../../src/sf2tool/h2/map_events_fixture.py), [event verifier](../../../src/sf2tool/h2/map_events.py), [script verifier](../../../src/sf2tool/h2/map_script_engine.py), [roster contract](../contracts/party-roster-state.md#handler-boundary) | Schema/shard identity/order, complete source-output equality and provenance are separate checks; follower allocation is not force membership or scene-specific save acceptance. |

The [reproduction owner](../../research/new-granseal-return-and-astral-following.md#reproduction-and-review-boundary)
pins SF2DISASM master `c834c652b6862bc5679fd7f69a38a7093206efc6` and USA ROM
SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Owning commands remain `uv run sf2 h2 map-init`, `uv run sf2 h2 map-events`
and `uv run sf2 h2 map-script-engine`. This synthesis checkout lacks the
pinned upstream/retained pair: no acquisition, regeneration or fresh
comparison claim.

[#965 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/965#issuecomment-6179605996)
and [PR968 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/968#issuecomment-6179629067)
retain final 4f09a34a/tree fe46c1e9/base 7c8a9760 and original 97181f64/CI38645456942;
final CI38645631608 passed scope 15s/research 46s. The unchanged 18,285,979-byte
pair's 0.087/0.081/0.080s receipts remain distinct; main-gate retained its own
0.080s complete-join comparison. Those joins include 134 records, nine custom
and 11 movement bodies, 41 text sites and a 774-byte interval. Preserve the
Map68 Battle19 and 32-byte entity-stride corrections and inspection diagnostics.

[#966 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/966#issuecomment-6179544821)
and [PR967 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/967#issuecomment-6179565201)
retain final bebcdeda/CI38645261417, original CI38645151326 and the flash/fade
ordering clarification. The predecessor guide's
[retained results](creed-departure-dwarf-recovery-and-cannon-gift.md#evidence-review-and-retained-results)
preserve #961 original/rebase/acceptance receipts, encoded-size correction,
linked #957/#953/#949/#945/#941/#937 failures/results/Unknowns,
[#954 callback correction](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/954#issuecomment-6178761171)
and [#962 Fairy correction](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/962#issuecomment-6179318374).
Fairy's `(17,10)` Up/eas_InitSlow mansion declaration remains separate from
mine reconstruction. No completed failure is restarted or called interrupted.
Contradictions return to their research owner without Layer B research edits.

Direct complete prose/diff, links/anchors/navigation, scope/private and
`git diff --check`, configured clean committed planner and actual exact-head
CI belong to this slice. No blanket runtime or translation suite follows
from the prose change. The Issue handoff records candidate identity and
checks for independent acceptance.
