# Oddler Stays and the Fairy Handoff

Oddler says he wants to stay with Creed, although he cannot explain why. Creed
advises him to rest and say farewell. The scene stages that farewell, hides
Oddler's current actor, and requests Peter and the Fairy to follow the party.
Only after the scene returns does the caller change the flags used to select
Oddler and Fairy on a later fresh entry.

**Inferred:** this expresses Oddler remaining with Creed while Fairy accompanies
the party. **Confirmed:** the admitted source sequence separates current actor
operations, ordered caller flags and later population reconstruction.
**Unknown:** natural arrival at the zone, actual supplied state, completed
presentation/following, usable control and saved persistence.

This guide continues [Creed return and the recruitment choices](creed-return-fairy-and-recruitment-choices.md)
and consumes [accepted #957 research](../../research/oddler-stays-and-fairy-handoff.md)
and accepted predecessors from main `936748b0715fdc992b05833745b566953dc77de3`.
It stops after the immediate conditional zone return and relevant fresh-entry
consequences. A walked mansion departure, overworld route and dwarf healing
have separate boundaries; Creed's advice to go north does not perform that journey.
This Layer B guide selects no remake rule or new observation work.

## When the Farewell Is Admitted

**Confirmed:** the room's zone table associates `(15,21)` with
Map33_ZoneEvent0. A matching callback first checks F774. Clear selects the
[earlier Goliath scene](creed-mansion-entry-and-first-goliath-scene.md), sets
F774 only after its return, and returns without falling through to Oddler's
farewell. F785/F786 cannot override that priority.

With F774 set, F785 must be set and F786 clear to call `cs_5B466`. F785 comes
from the earlier Fairy interaction even when its Bowie-HP-guarded following
was skipped. The farewell gate tests neither F787 nor recruit joined flags:
choosing a recruit is not a local prerequisite. It also supplies no F784,
F783, F762, F67/F68, HP, capacity or successful-follow prerequisite.
The [request and gate owner](../../research/oddler-stays-and-fairy-handoff.md#request-gate-and-actual-actors)
retains all three table rows, six setup routes and exact branch/PC joins.

A zone request still depends on working-layout type, movement/collision and
coordinate production. Dispatch clears the pending event before selecting
the callback; the wrapper reselects current setup and compares coordinate low
bytes. A supplied `(15,21)` callback is not evidence of ordinary walking there.
F785 clear or F786 set skips the farewell and all three later writes while
still returning from an invoked callback. The default callback simply returns;
the other specific row `(12,14)` has its separate F783-gated scene.

## The Actors Already in the Room

**Confirmed:** under the usual F784 population with F786/F22 clear, the residents
are Goliath128, Creed129, Fairy130 and the four recruits. Normal init leaves
Goliath at `(17,14)` Up, Creed at `(17,5)` Down and Fairy at `(18,5)` Down;
living unjoined recruits can be placed around the room. Peter is ally7 and
Oddler is the ordinary FOLLOWER_B31, separate from those residents.
The [preceding population guide](creed-return-fairy-and-recruitment-choices.md#returning-to-the-room)
retains selection, init, membership and HP premises.

With only F67/F68 ordinary followers, Bowie is physical0, Peter/Oddler are1/2,
Goliath/Creed/Fairy are3/4/5 and the recruits are6–9. These are conditional
allocations, not fixed actor slots. F69 independently selects FOLLOWER_C158/Fairy,
lookup offset62, distinct from Fairy130's offset34. Other follower rows can
repeat B31 and overwrite its lookup while leaving earlier physical actors.
Missing ordinary lookups can alias physical0; hiding invalidates them to -1.
The scene does not repair absent, hidden or aliased mappings.

Actual HP, class, mode, list order and mapsprites matter. Bowie/Peter placement
and facing can skip at HP0, while NPC129/130 and B31 bypass that named-ally
guard. Init, gestures, inline actions and hides have no equivalent HP skip.
A changed population can therefore admit the script without supplying the
ordinary cast. Actor labels below describe the intended distinct mappings;
they are not assertions that every supplied state contains those people.

## The Farewell in the Room

**Confirmed:** the [complete scene owner](../../research/oddler-stays-and-fairy-handoff.md#complete-scene-and-embedded-actions)
orders the following staging, conversation and actions. Its camera destinations
are view requests, not player movement or proof of a displayed camera position.

The scene requests a view at `(12,15)`, pauses, turns Peter and Bowie Up and
makes Oddler shiver. Peter asks about Oddler; Creed says he understands.
Oddler and Fairy face Up, Creed is placed at `(17,15)` Down, and a second view
is requested at `(12,7)`. Peter, Oddler and Fairy each receive a waited Init
action. This replaces their previous actions, including Fairy's earlier
following and slower Init properties; it does not reset their positions or
remove an existing exploration-list entry.

Bowie is then placed at `(15,23)`, Peter at `(15,22)`, Oddler at `(16,22)` and
Fairy at `(16,23)`, all Up, subject to the named-ally HP guards. Eric, Randolf,
Tyrin and Karna are hidden in that order, regardless of choice, HP, joined or
active status or previous shrinking. The camera targets Creed while he walks
Down2, Left2, Down4, conditionally arriving at `(15,21)`.

A view at `(10,18)` precedes Peter asking what Creed understands. Creed says
Oddler will remain, moves Right1 and faces Down, then asks Oddler to agree.
Peter faces Right and Oddler Left. Peter asks if this is true; Oddler says he
wants to stay without knowing why, then insists he must stay. These questions
have no yes/no menu or choice mutation.

Creed moves Left1 and faces Down; Peter and Oddler face Up. Creed calls
Oddler weak and recommends staying and saying farewell. Oddler moves Up1,
faces Left while Creed faces Right, and Creed nods after a pause. Creed then
moves Left1 and faces Down; Oddler moves Left1 and faces Down. Oddler thanks
the leader for travelling together, thanks the party and asks them to be
careful. He nods, pauses, moves Up1, then turns Down and Up across further
pauses. His waited Up3 is followed by hiding his current B31 actor.

Creed moves Right1 and faces Down. He tells the leader they will learn the
reason later, asks them not to worry and directs them north. The final commands
request Peter→Bowie, then current Fairy130→Peter, and end the script. There
is no reload, live block copy, warp, force join/leave, healing, story-flag
command, explicit music change or Egress/player-mode write in this scene.
The earlier recruitment is not undone and no replacement recruit is chosen.

Under distinct mappings and completed placements/moves, Creed's path is
`(17,15)→(17,17)→(15,17)→(15,21)→(16,21)→(15,21)→(14,21)→(15,21)`.
Oddler's is `(16,22)→(16,21)→(15,21)→(15,20)→(15,17)` before hide.
The five `faceDown1` operands change facing, update the sprite and wait1;
they add no Down movement. These coordinate derivations require completed
relative actions and do not establish collision-free observed motion.

The [dialogue and action consumer owner](../../research/oddler-stays-and-fairy-handoff.md#dialogue-action-consumers-and-limits)
retains all fourteen text sites1932–1945, attributed speakers, forms and
controls. Oddler's four speech entries use encoded `$801F`; actual mapped
sprites determine portrait/bleep, and an open portrait is not replaced.
LEADER rebuilds the force and reads its first active member's name, which
need not be Bowie. It does not change the independent Bowie HP guard.
Weakness, wishes and Creed's explanation are speech, not an implemented cure
or proof of the cause of Oddler's condition.

N is newline; W1/W2 wait for input, with W2's continuation-arrow/sound behavior.
Single text closes windows and sleeps10; continuing text does not. Debug/P2
Start can latch text skipping after interpreter entry clears it, omitting
display and cursor advancement while other commands still execute. Shiver,
nods, camera scrolling, sprite updates and source sleeps are service requests,
not measured presentation or elapsed-time evidence. Inline action waits poll
only their selected actor's Idle pointer, without timeout or a global barrier.
Asynchronous entity service and pointer replacement remain relevant throughout.

## Hiding and the New Current Following

**Confirmed:** [HideEntity and follow consumers](../../research/oddler-stays-and-fairy-handoff.md#five-hides-two-current-follows-and-ordered-return)
keep visible staging, lookup/list state and force membership distinct.
Each hide sets current/destination coordinates to `$7000`, invalidates every
matching lookup and removes the physical actor from the exploration list if
present. It retains the record, sprite/flags and old action pointer; ordinary
entity service skips it while current X is at least `$7000`, making it dormant.

Thus the four recruit hides remove current following/list participation without
removing recruitment or active membership. They do not await prior shrinking.
The earlier shrink tail parked at `(63,63)` below that service threshold and
retained lookup/list entries. Hide and shrink have different consequences.
An alias can make one hide invalidate several logical actors and redirect
later operations; the five hides are not guaranteed to affect five distinct
physical actors or establish rendered absence.

Both final follow words have high byte0: `$0007` for Peter and `$0082` for
Fairy130. The handler reads that encoded high byte before decoding the actor
words, so both relations test Bowie HP. Bowie HP0 skips each relation's six
argument bytes. It does not undo the earlier Init, hides or caller writes if
the script returns. In particular, a skipped final Fairy follow does **not**
preserve the earlier Fairy→Oddler action: Init has already replaced it.
A living Bowie with dead Peter can admit both follows although Peter's own
placement/facing skipped.

When admitted, each relation resolves current mappings, allocates/copies a
42-byte follower action and installs offset `(-24,0)`. An already listed actor
still receives a replacement action; only list insertion is deduplicated.
These are Peter→Bowie and Fairy130→Peter requests, not teleportation or waited
settling. They create no Fairy158, grant no force membership and set no F69.
Missing/self/aliased targets have no local validation. The current Fairy
remains resident130 even after the later caller flags change.

## Script Return and Ordered Flags

**Confirmed:** script end conditionally waits for view scrolling when the
dialogue-window index is nonzero, clears scrolling speed and returns through
the script trap. Register restoration does not restore mutated global state.
It neither polls all actors nor restores a Bowie camera target. Only after
that return does zone0 set F786, set F69, clear F68 and return, in that order.

The zone wrapper then closes portrait/text, waits one VInt and waits for
physical0's current/destination X/Y equality. This is a coordinate wait, not
an Idle-pointer, follower or all-service join. Default and skipped callbacks
also reach this wait. If no setup is valid, callback/window closure are bypassed,
but the physical0 wait remains. Callback invocation, scene admission and usable
controller return are separate claims.

The sequence is not atomic. Before script return, hides/actions/positions can
remain changed while caller flags retain their old values. After set786,
69/68 may still be unchanged; after set69,68 may still be set. A later callback
with786 set skips instead of completing that interrupted prefix. A full return
reasserts already set69 or already clear68. Direct scene invocation omits the
gate and caller writes; direct repetition or clearing786 can encounter hidden
or changed actors. Neither safe replay, rollback nor save persistence follows.

## What a Later Fresh Entry Selects

**Confirmed:** [fresh-entry and changed-premise evidence](../../research/oddler-stays-and-fairy-handoff.md#current-population-fresh-entry-and-changed-premises)
is separate from those current operations. There is no room reload at the
farewell return. Goliath128 remains its current actor, Creed129 its staged
actor, Fairy130 the current Fairy; B31 and recruits are hidden/dormant. Last
explicit Bowie/Peter/Fairy placements are not guaranteed settled positions
once following/service runs. No resident Oddler130 has yet been constructed.

An explicit fresh LoadMap reconstructs followers/residents and runs selected
init. CURRENT255 reuse retains current actors/layout. Fresh entry clears
temporary256–383 and sets80, preserving the non-temporary flags discussed here;
that is load behavior, not verified saved storage. Accessors reselect current
flags, so changes between them can produce mixed premises.

| Later supplied state | Fresh selection and its limit |
| --- | --- |
| F786 set, F22 clear | F786 residents are Goliath128 `(22,4)` Up with walking origin/range1, Creed129 `(17,5)` Down Init, and Oddler130 `(17,7)` Up Init. No recruit residents; alternate init is only return, despite its flag312 label. |
| Full caller return, F22 clear, ordinary F67/69 set and68 clear | Followers are Peter and separately constructed Fairy158. With no extras they occupy physical1/2 before residents128/129/130 occupy3/4/5. Hidden old B31 is not the new resident Oddler130; old Fairy130 is not the new Fairy158. Other B-producing flags still matter. |
| F786 set, F22 clear, with69 clear or68 still set | Resident Oddler130 still selects, while independent follower flags omit Fairy158 and/or include B31/Oddler. Two Oddler sprites are possible. The zone skip does not reconcile flags. |
| F22 set, even with786 | Last-set-wins order is default,523,783,784,786,22. F22 selects the six baseline residents: Goliath, Creed and four recruits, initially parked at63,63; no resident130. F69 independently selects Fairy158. Normal init places Goliath `(14,14)` Up. |
| F22 and784 both set | The normal init's independent784 tail still places living unjoined Eric/Randolf/Tyrin/Karna at `(15,14)/(16,15)/(18,15)/(19,14)` Down. Joined recruits normally stay parked;787 does not govern placement. The earlier523-set/783-clear restoration branch can still run under supplied flags. |
| Fresh F22 layout | Its flag-copy table copies `(28,0)`, size25×32, to `(0,0)`. It has no786/69/68 replay of the farewell or earlier Fairy one-cell copy. Collision reach is not inferred. |
| Retained population with newly changed786/22 | Existing actors do not morph, move or rebuild. Fairy130 can now receive Oddler's1948–1949 callback words; flags alone do not replace her, unhide recruits or create158. F22 table selection does not supply an absent130. |

The scene/caller leaves F67,762,774,783–785,787 and recruit joined/active flags
unchanged. Lack of recruitment or F787 does not reject admission, and missing
F784 can supply a different cast. These changed-state paths constrain the
interpretation without establishing natural campaign chronology.

**Unknown:** natural zone reach and exact layout/request; supplied flags, HP,
class, mode, roster/list, population and aliases; action, view, portrait, text,
audio and collision completion; usable control and save/load persistence.
Later departure, northward travel and dwarf recovery remain outside this guide.
These questions do not create an acquisition or emulator queue.

## Evidence Review and Retained Results

The adversarial Layer A review checked the accepted owner and selected public
payload/schema/consumer surfaces. No blocking contradiction was found; the
stricter source and service boundaries above govern the synthesis.

| Reviewed surface | Disposition |
| --- | --- |
| [#957 research](../../research/oddler-stays-and-fairy-handoff.md) and [#953 predecessor](../../research/creed-return-fairy-and-recruitment-choices.md) | Complete farewell, actors/actions, text, Init replacement, encoded-high-byte guard, ordered return and fresh/reused selection remain distinct. |
| `sf2-map-events-static-v1`: [routing-setup](../../../tests/fixtures/h2/map-events/routing-setup.json), [zone-programs](../../../tests/fixtures/h2/map-events/zone-programs.json) | Selected payloads/schema checks agree on three rows, six routes, all fourteen zone0 operations, adjacent zone1 and default; no natural reach claim. |
| `sf2-map-init-static-v1`: [fixture](../../../tests/fixtures/h2/map-init-static-v1.json) | Schema and complete selected normal/alternate bodies agree on independent784 tail, F22 branch and alternate return. |
| `sf2-map-script-engine-static-v1`: [fixture](../../../tests/fixtures/h2/map-script-engine-static-v1.json) | Schema and selected Init/inline action/hide/follow guards agree; handler facts do not supply the private full scene corpus. |
| [Event loader](../../../src/sf2tool/h2/map_events_fixture.py), [event verifier](../../../src/sf2tool/h2/map_events.py), [script verifier](../../../src/sf2tool/h2/map_script_engine.py), [script schema](../../../schemas/h2/map-script-engine-fixture.schema.json) | Shard identity/shape/order and provenance/expected equality are separate checks; legal shape alone does not prove original behavior. |
| [Party/roster](../contracts/party-roster-state.md#handler-boundary) and [dialogue](../contracts/dialogue-system.md#confirmed-static-contract) | Following, membership, name/cursor and input services retain their bounded scopes, not natural reach or persistence. |

The [reproduction owner](../../research/oddler-stays-and-fairy-handoff.md#reproduction-and-review-boundary)
pins SF2DISASM master `c834c652b6862bc5679fd7f69a38a7093206efc6` and USA ROM
SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
It owns the complete67 records, nine inline bodies and fourteen text joins.
This synthesis checkout lacks the pinned upstream and retained pair: there is
no acquisition, regeneration or fresh source/output comparison claim.
Owning commands remain `uv run sf2 h2 map-init`, `uv run sf2 h2 map-events` and
`uv run sf2 h2 map-script-engine`; none is run here merely for prose.

[#957's frozen handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/957#issuecomment-6178978855)
and [independent acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/959#issuecomment-6179038849)
retain head5f45f3f8/treee851dcc4/base17718cd3, unchanged18,285,979-byte pair0.085s,
CI38640917634 and complete joins. Init/F22/encoded-end corrections and corrected
source-path/truncated-output diagnostics remain retained inspection evidence.

[#958's original handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/958#issuecomment-6178996339),
[rebase receipt](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/958#issuecomment-6179069477)
and [acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/960#issuecomment-6179080841)
retain the unchanged five-file guide,518 links and CI38641099043/38641713008.
The [predecessor retention owner](creed-return-fairy-and-recruitment-choices.md#evidence-review-and-retained-results)
preserves #953's original/rebased0.081s comparison, #954's callback correction
and linked #949/#945/#941/#937 results, completed failures and Unknowns.
No completed failure is reclassified as interrupted or restarted. Contradictions
return to the research owner without Layer B research edits. Direct prose,
link/anchor, scope/private and diff checks plus configured committed planner and
actual exact-head CI belong to this slice; no blanket runtime or translation suite.
