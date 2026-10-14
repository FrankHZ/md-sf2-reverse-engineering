# Creed Departure, Dwarf Recovery and the Cannon Gift

Fairy asks to see the sick dwarf, examines him, and stages a bright recovery
sequence. The dwarf resumes animation and says he feels fine. Fairy declines
a reward for herself and asks the dwarves to help the party's leader instead.
Their gratitude leads to an offered Cannon and a return to digging toward Grans.

**Inferred:** this is the story of Fairy helping the dwarves and the dwarves
repaying that help. **Confirmed:** recovery staging, inventory handling,
digging, caller flags and later map reconstruction are separate operations.
**Unknown:** natural traversal and interaction, actual supplied state, completed
presentation, usable control and save persistence. The recovery changes no
combatant HP, and the gift is neither guaranteed nor universally one-time.

This continues [Oddler stays and the Fairy handoff](oddler-stays-and-fairy-handoff.md)
using [accepted #961 research](../../research/creed-departure-dwarf-recovery-and-cannon-gift.md)
and its accepted predecessors on main
`c430f26f69a01a0f6e917660e34503c6bcccbec4`. It stops at the immediate
entity/zone returns and relevant fresh-entry consequences. Cannon use,
subsequent journeys and New Granseal/Astral content are outside this guide.
This Layer B explanation selects no remake rule or new observation work.

## Leaving Creed and Entering the Mine

**Confirmed:** the selected route is Mansion33 → Devil's Tail67 → Southwest
Parmecia68 → mine interior30. The
[route owner](../../research/creed-departure-dwarf-recovery-and-cannon-gift.md#selected-departure-and-mine-entry-route)
retains exact table rows, request consumers, coordinates, battle premises and
mode distinctions. These are conditional connections, not a proved mandatory
walk across the terrain.

The mansion's row at any X/Y31 requests Map67 `(26,23)` Up without scrolling.
Map67's border at X63/any Y instead requests a Right scroll16 into Map68,
with X1 and retained Y. That scrolling path changes the current map and
working layout inside exploration while retaining actors and the follower
lookup/list. It skips the no-scroll MainLoop battle check, fresh actor build,
temporary-flag reset and selected init. The request's Right facing byte is
not applied there; the main actor's current facing is sampled. Its ordinary
X1 derivation also requires the returned coordinate high byte to be zero.

Map68's row `(57,24)` requests Map30 `(8,20)` Up without scrolling. Map30 is
the mine interior reached directly from 68; the distinct village exterior29
is not an implied intermediate stop. Its sole outgoing row requests Map68
`(57,25)` Down from any X/Y21, establishing only this local return boundary.
Other table alternatives remain optional alternatives, not required detours.

No-scroll entry unwinds exploration and can select a battle before ordinary
map admission. Map67's Battle21 and Map68's Battle19 flags and independent
walking/random-battle zones retain their accepted consumers; the border scroll
does not itself call CheckBattle. Stop at an admitted battle rather than
assuming uninterrupted travel. Map30 has no battle-table row. No selected
map is flag-switched by the global substitution table.

On an explicit fresh, no-battle Map30 entry, exploration clears pending state,
performs its step-counter and living/immortal-ally healing, loads the map,
rebuilds actors, clears temporary flags256–383, sets80 and invokes selected
init. This entry healing is separate from Fairy's recovery scene. F759–762,
F785 and follower flags survive that temporary reset. CURRENT255 reuse retains
the current population; later accessors can select tables from changed flags
without rebuilding those actors. Music/fade/init and pending-event priority
still separate map entry from usable control.

Actual working-layout warp/zone types, supplied coordinate high bytes,
movement/collision, pending consumption and player mode remain premises.
Caravan mode1 lacks walking's zone producer. The
[accepted travel guide](caravan-boarding-and-southwest-parmecia-travel.md)
retains the distinct mode0/1/2, raft and follower rules; this route does not
establish a boarding, landing or natural walking observation.

## The Dwarves and the Fairy Are Different Actors

**Confirmed:** fresh Map30 selection is last-set-wins: baseline, then F785,
then F761. F761 wins even with F785 set. All three setups share callbacks
and init but have different resident lists; F69/F760/F762 do not select them.

| Selected population | Residents and initial arrangement |
| --- | --- |
| Baseline | Dwarf128 walks near `(6,10)`; 129 is `(7,4)` Up; 130 `(15,12)` Down; 131 `(15,11)` Down. No 132. |
| F785 | Dwarf128 `(11,11)` Down; 129 `(16,12)` Left; 130 `(15,12)` Down; 131 `(15,11)` Down; 132 `(15,13)` Up. All begin with Init. |
| F761 | No residents. Callback addresses still exist, but a fresh build supplies no dwarves. |

The [population/init owner](../../research/creed-departure-dwarf-recovery-and-cannon-gift.md#complete-map30-population-and-init)
retains declaration actions, lookup allocation and changed-state cases.
Resident130 is the sick dwarf, not Fairy. Fairy is the independently selected
FOLLOWER_C158 under F69, with lookup62; residents128–132 use lookups32–36.
They are NPC sprites rather than force members or combat-HP patients.

The normal post-Creed selectors F67/F69 set and F68 clear supply Peter and
Fairy but omit ordinary B31/Oddler. With no extra followers, F785 residents
then occupy physical3–7 after Bowie0, Peter1 and Fairy2. **Missing B's zero
lookup resolves to physical0/Bowie**, so this scene's B commands cannot simply
be narrated as Oddler remaining with the party. Its B-follow request can make
Bowie follow Peter; B placement/facing can also affect Bowie. The later explicit
Bowie placement can overwrite that placement. With F68 also set, B is instead
a distinct actor under the stated ordinary allocation. F74/F70 and duplicate
IDs can supply other B mappings; hidden lookups become -1. No local repair
normalizes missing or aliased actors.

Map30 init first runs the F761 layout program if 761 is set, then independently
runs illness setup when 760 is clear. Both can run on an empty F761 population,
where missing130 may redirect illness work to physical0. Illness setup disables
130's animation, orients Right, updates the sprite, waits its action's idle
pointer and stops animation again. It does not globally pause movement or heal
HP. With 760 set, fresh init skips illness setup without replaying recovery or
relocating residents. Changing flags live does not replace the current cast.

## Conversations and the Recovery Invitation

**Confirmed:** the
[six entity callbacks and zone owner](../../research/creed-departure-dwarf-recovery-and-cannon-gift.md#six-entity-rows-and-the-two-row-zone-table)
separates talk from recovery admission. Dwarf128 initially describes coming
from Grans for Mithril. After 760 is set, his callback invokes the gift helper
instead, without a 761 guard. Dwarf129 reports an earthquake-blocked tunnel and
the need for the sick friend; 130 shivers before and after complaining of cold.
Both conversations set 759. Dwarf131 says a dwarf went to seek Fairy and sets759
while 761 is clear; with 761 set, he instead invites the party to Dwarven Village
on Grans. Dwarf132 sings about digging toward Grans. The default simply returns.

These callbacks are not all gated by recovery: a directly invoked130 callback
still shivers after 760, 129 still reports sickness, and 131 checks761 alone.
The baseline lacks132 and fresh761 lacks all residents. A selected callback
address therefore does not prove that an ordinary player can talk to that actor.
Targeting range, layout, list exclusions, lookup order and actual sprite remain
part of the interaction. The wrapper turns 128 toward the player without
restoration, turns/restores 129/131/132, and does neither for 130/default.

The zone at `(7,13)` admits recovery exactly when **F69 is set and F760 clear**.
It does not require 759,762,785,761, HP, available inventory or the expected
population. Earlier conversations are not local prerequisites. F785 normally
supplies the five-dwarf staging, but it is not the admission flag. The default,
69-clear or760-set paths return without the scene/helper/caller writes. Walking
must first produce the appropriate zone request; supplied callback coordinates
do not prove natural arrival.

## Fairy Examines the Patient

**Confirmed:** the [complete recovery scene](../../research/creed-departure-dwarf-recovery-and-cannon-gift.md#complete-recovery-scene)
orders these phases. Actor labels describe the intended distinct mappings;
the preceding alias, HP, population and mode premises still apply.

Fairy receives a waited slower Init. The scene then requests B→Peter following
with direction2; its encoded high byte tests Bowie's HP before decoding B.
Bowie HP0 skips that relation, not the recovery as a whole. Fairy next receives
a waited custom zero-speed action, a destination at `(8,11)`, and asks for the
sick dwarf. Init and custom actions replace previous action pointers without
removing existing exploration-list entries or resetting position. The later
destination has its own movement/coordinate wait.

Fairy turns from side to side without an action-completion wait while the
camera changes view. Dwarf128 steps Left, welcomes her, jumps twice with waits,
then moves Right3, faces her and asks for an examination. The camera follows
Fairy through Right4, Down1, Right2 toward the patient, then shifts its view.
After another pause, Fairy hopes she arrived in time and begins examining him.
She shivers twice across pauses, requests white flashing, a white fade-out,
another flashing sequence and a white fade-in, then pauses again. These are presentation instructions and
service waits, not an observed medical effect or measured recovery time.

Dwarf129 turns from side to side, moves Down1 and asks how the patient is.
Dwarf132 shakes his head and says he does not yet know. The patient130 then
receives waited Init and Jump actions, resumes animation and exclaims. After
his own unawaited side-to-side action and pause he faces Up and asks a question.
Dwarf129 moves back Up, asks if he is OK, and 130 says he feels fine.

The animation restoration and self-report express recovery in the story;
**Inferred:** Fairy has helped him. **Confirmed:** these commands do not change
combatant HP, revive an ally or change force membership. Named-ally HP guards
on placement/facing differ from the unguarded Init/action/gesture/flash/hide
paths; NPC130's animation commands do not test Bowie's HP. Changed actors,
dead allies or aliases retain those differences.

## Gratitude and Fairy's Departure

**Confirmed:** dwarf128 steps Right and turns toward Fairy while the other
dwarves turn Left and Fairy Up. He asks how to thank her; 130 nods. Fairy
declines a personal reward and asks them to help LEADER instead. The scene
places B at `(7,11)` Right, Peter at `(7,10)` Right and Bowie at `(8,11)` Right,
subject to their distinct placement guards and actual lookups.

The camera follows Fairy Left2, Up1, Left3 before she says farewell and wishes
LEADER a safe journey. Camera following is then released. She moves Up2,
Left3, Down3; Bowie, Peter and B face Down; she moves Right1, Down5 and is
hidden. The camera requests `(4,7)`. Bowie receives an unawaited Right2 action
while 128 moves Left3 with a wait, then the scene returns. Its ending does not
wait for Bowie's action pointer; the helper's later Init can replace it.

Hide moves Fairy's current/destination coordinates to `$7000`, invalidates
matching lookups and removes her physical actor from the exploration list.
The record/action remains dormant under ordinary entity service. This hides
the current C actor without clearing F69; the zone caller clears 69 only later.
It is not force-member removal or proof of a saved departure.

The [text/service owner](../../research/creed-departure-dwarf-recovery-and-cannon-gift.md#exact-text-and-immediate-services)
retains all 28 exact text joins 1705–1732, speaker operands and controls.
LEADER names the first active force member after UpdateForce, which can differ
from Bowie. Fairy/Dwarf sprites select distinct bleeps and no portrait under
ordinary mappings; changed sprites or aliases change that context. There is
no yes/no menu in the recovery, gratitude or digging conversation.

N inserts a newline; W1/W2 wait for input, with W2's continuation/sound behavior.
Single text closes windows and sleeps 10; continuing text does not. The internal
W2 in Fairy's single farewell 1725 and the terminal W2 in single 1731 are distinct
wait sites. Debug/P2 Start can latch cutscene text skipping after entry clears
it, skipping cursor advancement/display and encoded waits while other commands
still execute. Direct helper txt calls and its Sleep100 are separate consumers.
No rendered timing, completed input or observed text/audio follows from the
source sequence alone.

## Accepting or Deferring the Cannon

**Confirmed:** after recovery returns, the zone calls the
[Cannon helper](../../research/creed-departure-dwarf-recovery-and-cannon-gift.md#cannon-helper-and-complete-digging-scene).
It credits the recovery to the addressee and offers a gift, then counts all
four of **Bowie's** non-NOTHING item slots. Slot0 need not be free. This fixed
recipient is combatant0, independently of the name substituted for LEADER;
there is no party-wide capacity fallback.

If the word count equals 4, the helper says the addressee's hands are full and
defers the gift. It returns before AddItem, item music, 761 or digging. Recovery
has still happened in the admitted sequence, and the zone still proceeds to
set 760 and clear 69. With current dwarf128 retained, his later 760-set callback
can retry the offer after space is made.

Otherwise the helper calls AddItem with Cannon `$72` for Bowie. AddItem uses
the first masked NOTHING slot and returns success/failure, but the caller
does not inspect that result. It requests item music, narrates LEADER receiving
the Cannon, sleeps 100, closes text, sets 761, requests previous music and runs
digging. Under unchanged valid inventory, count below 4 supplies a free slot;
the receipt narration still is not an independent checked AddItem result.

There is no helper guard for 761, an existing Cannon, HP, class or membership.
A repeated direct helper or retained 128 interaction can award again if Bowie
has space, including with 761 already set. If full, it defers again. Fresh761
entry removes residents, limiting ordinary access by population reconstruction;
that does not implement a universal one-time gift transaction. A direct helper
does not itself set 760 or clear 69.

The helper inherits speech context rather than explicitly selecting 128. An
entity interaction supplies its wrapper-resolved context; in the ordinary
unskipped zone chain the initial direct txts follow Fairy's last farewell
context. The digging script explicitly selects 128 later. Narrated gratitude
and the fixed recipient must not be used to invent a helper speaker selector.

## The Dwarves Return to Digging

**Confirmed:** the [complete digging scene](../../research/creed-departure-dwarf-recovery-and-cannon-gift.md#cannon-helper-and-complete-digging-scene)
turns 129–132 Left and requests a view at `(6,7)`. Dwarf128 explains that the
Cannon destroys large rocks, steps Right1, calls the dwarves back to Grans
and orders digging. This is advice and intention, not Cannon use or travel.

The four dwarves 129–132 jump in two groups. Each group installs actions on
all four but waits only 132. Bowie receives waited Init and Left1, and Peter
a destination `(8,10)`. The dwarves shift Left: 128/132 by 2, 129/130/131 by 1,
with only 132 waited. Bowie, Peter and B then face Right.

The five dwarves receive different northward paths. Dwarf128 goes Up2, Left1,
Up3; 131 Left3, Up2, Left1, Up3; 132 Up2, Left3, Up2, Left1, Up3; 129 Up1,
Left5, Up2, Left1, Up3. These four installs are unawaited. Dwarf130 instead
waits 40 in its own action, moves Up1, Left4, Up1 with a wait, pauses, moves
Down1, faces Left and nods, then takes a waited Up2, Left1, Up3 path.

Finally, 128/129/130/131/132 are placed at `(7,3)/(8,3)/(7,4)/(8,4)/(7,5)` Up.
The script copies a 2×2 layout rectangle from `(21,6)` to `(7,5)` and returns.
These placements set current/destination coordinates without clearing the
other action pointers. Waiting 132 or 130 does not prove every dwarf finished;
earlier asynchronous actions can remain relevant under changed timing/mappings.
No dwarf hide or warp occurs here.

The live scene performs only that 2×2 copy. A later fresh761 init repeats it
and additionally copies 4×4 from `(21,0)` to `(6,0)`, while constructing no
residents. Neither the live copy nor the extra fresh copy proves rendered
excavation, a collision opening or a naturally usable onward route. The
[action/layout owner](../../research/creed-departure-dwarf-recovery-and-cannon-gift.md#action-layout-and-return-semantics)
retains per-actor waits, placement, camera, flash/fade and copy semantics.

## Returning, Repeating and Interrupting

**Confirmed:** the zone order is recovery → helper → set 760 → clear 69 → return.
Within the helper's available-space branch, AddItem precedes narration, 761
and digging. Interruption can therefore leave an item before 761, 761 before
completed digging, or completed recovery/helper effects before 760/69. After
set 760 but before clear 69, both flags can remain set; a repeated zone skips
on 760 instead of completing that interrupted prefix. There is no rollback,
atomic item/scene transaction or local save operation.

An existing 761 does not reject a supplied 69-set/760-clear zone. Fresh761 can
then supply an empty cast and alias missing residents to main rather than
repairing the scene. Baseline/F785 populations, current versus rebuilt state,
changed HP/class/membership/mode, absent C/B and aliases retain their actual
branches. The zone/helper do not write 759/762/785; separate illness callbacks
write 759. Direct recovery-script invocation omits the gate, helper and caller
flags; direct helper invocation omits recovery and those caller flags.

The zone wrapper closes portrait/text, waits one VInt and physical0 coordinate
equality after callback return. That is neither an all-actor/action barrier
nor a usable-control acknowledgement. An entity-call helper instead returns
through the entity wrapper's facing/window/update services without the zone's
physical0 wait. Script end may wait view scrolling and returns to its caller;
it does not grant control directly. Following/list changes, hide and persistent
follower selectors remain distinct throughout.

**Unknown:** natural route/zone/interaction reach; actual flags, layout, mode,
HP, class, inventory, membership, population, lookup/list and AddItem result;
completed collision/action/text/portrait/view/fade/audio services, visible
recovery, usable control and save/load persistence. These questions do not
create an acquisition or emulator queue or authorize later campaign expansion.

## Evidence Review and Retained Results

The adversarial review checked accepted Layer A prose and selected public
payload/schema/consumer surfaces. No blocking contradiction was found. The
public script fixture retains handler facts and references, not the private
complete scene corpus; that distinction limits this review.

| Reviewed surface | Disposition |
| --- | --- |
| [#961 research](../../research/creed-departure-dwarf-recovery-and-cannon-gift.md) and its accepted travel/farewell predecessors | Route, actual actors, complete recovery/helper/digging order, text and changed-state limits agree at the accepted static boundary. |
| `sf2-map-events-static-v1`: [routing](../../../tests/fixtures/h2/map-events/routing-setup.json), [entity programs](../../../tests/fixtures/h2/map-events/entity-programs.json), [zone programs](../../../tests/fixtures/h2/map-events/zone-programs.json) | Selected Map30 rows, six entity contexts, two zone contexts and shared return overlap preserve branch polarity and caller order. Helper coverage is not falsely expanded from callback targets. |
| `sf2-map-init-static-v1`: [fixture](../../../tests/fixtures/h2/map-init-static-v1.json) | Three routes share init; independent 761/760 decisions and complete embedded layout/illness bodies agree. |
| `sf2-map-script-engine-static-v1`: [fixture](../../../tests/fixtures/h2/map-script-engine-static-v1.json) and [schema](../../../schemas/h2/map-script-engine-fixture.schema.json) | Selected placement/action/hide/follow/layout/dialogue facts preserve guards and service boundaries without proving natural execution. |
| [Event loader](../../../src/sf2tool/h2/map_events_fixture.py), [event verifier](../../../src/sf2tool/h2/map_events.py), [script verifier](../../../src/sf2tool/h2/map_script_engine.py) | Schema/identity/order checks and source-output provenance/equality are separate responsibilities; legal fixture shape is not fresh source evidence. |
| [Roster contract](../contracts/party-roster-state.md#handler-boundary), [dialogue contract](../contracts/dialogue-system.md#confirmed-static-contract), [item-transaction owner](../../research/map-event-item-transactions.md) | Membership, speech, service and transaction seams do not supply this scene's natural reach, completed gift or persistence. Cannon-helper details remain with #961. |

The [reproduction owner](../../research/creed-departure-dwarf-recovery-and-cannon-gift.md#reproduction-and-review-boundary)
pins SF2DISASM master `c834c652b6862bc5679fd7f69a38a7093206efc6` and USA ROM
SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Its retained comparison is unchanged 18,285,979 bytes in 0.081s, with 144 records,
25 complete inline bodies, two custom bodies and 28 text joins. Owning commands
remain `uv run sf2 h2 map-init`, `uv run sf2 h2 map-events` and
`uv run sf2 h2 map-script-engine`. This synthesis checkout lacks the pinned
upstream/retained pair: no acquisition, regeneration or fresh comparison claim.

[#961's original handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/961#issuecomment-6179316611),
[rebase receipt](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/961#issuecomment-6179363538)
and [PR964 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/964#issuecomment-6179370463)
preserve original 608987b9/tree6c4bf429/base936748b0 and
final eea8829e/tree2b329756/base3b5df736, CI38643445482/38643827378 and the
completed ac_setSpeed encoded-size inspection failure/correction. Their linked
original diagnostics and #957/#953/#949/#945/#941/#937 receipts, failures and
Unknowns, including [#954's callback correction](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/954#issuecomment-6178761171),
remain retained without restart.

[#962's original handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/962#issuecomment-6179258820),
[PR963 review](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/963#issuecomment-6179272262),
[correction](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/962#issuecomment-6179318374)
and [acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/963#issuecomment-6179327445)
retain original 155899b8/CI38643054890 and corrected 7793bf19/CI38643473924.
Fairy's mansion declaration is correctly `(17,10)` Up with eas_InitSlow;
that declaration is separate from her mine-follower reconstruction.
No completed failure is reclassified as interrupted or rerun. Contradictions
return to the research owner without Layer B research edits. Direct prose,
link/anchor/navigation, scope/private and diff checks, configured committed
planner and actual exact-head CI belong to this slice; no blanket runtime
or translation suite follows from these prose changes.
