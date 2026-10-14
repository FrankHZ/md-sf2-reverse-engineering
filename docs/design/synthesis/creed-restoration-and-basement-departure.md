# Creed's Restoration and Main-Room Departure

**Inferred:** Creed's spell restores the party from its miniature predicament;
Peter then asks about Zeon, and Creed and the Fairy leave to consult the Evil
Spirit. **Confirmed:** the selected source orders that conversation, sprite
work and departure, then marks the scene complete and selects Hassan for later
Egress use. **Unknown:** natural arrival, the supplied actors and state,
completed animation/services, visible restoration, usable control and persistence.
The covered story ends in the main room, before any basement visit or recruitment.

This English guide continues [Battle23 outcomes and the return to Willard Burrow](battle23-outcomes-and-burrow-return.md)
under [#946](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/946)
and [epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
It consumes the accepted [restoration research](../../research/creed-restoration-and-basement-departure.md)
and explains its conditional consequences. Fnnn means flag nnn; command and
resident indices are zero-based. Story names below assume coherent logical-to-physical
actor mappings. Examples distinguish source branches, rather than report played scenarios.

## Returning to a Room Prepared for the Scene

**Confirmed:** the burrow's two existing exits to Creed Mansion33 request
`(15,9)`, Right: any X at Y0, or X21 at any Y, with first matching row winning.
Neither tests F523. Admission still needs eligible layout, the request consumer
and loading services; explicit destination bytes replace only the coordinate
low bytes. They do not repair arbitrary inherited high bytes or prove a natural
walk through the burrow. See the predecessor's [complete exit boundary](battle23-outcomes-and-burrow-return.md#existing-exits-remain-conditional-requests).

An explicit Map33 arrival takes fresh exploration loading. It clears the pending
event type, adjusts the step counter, heals eligible living/immortal allies,
requests fade-out, disables view following and sets CURRENT_MAP33/CURRENT_BATTLE255.
It loads tilesets, rebuilds actors and mapsprites, clears temporary flags 256–383,
sets F80 and loads the full map before calling init. The flags below survive
that temporary reset. A supplied CURRENT255 entry instead retains population
and working layout while updating main/raft properties; it does not create a new
seven-row room. The scene does not change PLAYER_TYPE; the coherent Battle23
predecessor supplied walking mode 0. Other supplied mode/raft states keep the
[Creed-entry owner's limits](../../research/creed-mansion-entry-and-first-goliath-scene.md).

Setup selection scans every override and keeps the **last set** one:

| Selection order | Population and init |
| --- | --- |
| Default | Baseline six residents; normal init |
| F523 | Seven return residents; normal init |
| F783 | Baseline six residents; normal init |
| F784 | Seven later residents; normal init |
| F786 | Three later residents; return-only alternate init |
| F22, last | Baseline six residents; normal init, overriding F786 too |

The return premise is F523 set with F783/F784/F786/F22 clear. F774 neither selects
this setup nor gates restoration. Each setup accessor reselects using current
flags; population and init can disagree if flags change between them. The four
pointer tables share events, descriptions and Section5. The accepted
[selection/population join](../../research/creed-restoration-and-basement-departure.md#admission-setup-selection-and-population)
retains the exact tables, allocation and jump aliases.

Under that return premise the complete resident declarations are:

| Row | Logical identity / sprite source | Initial position, facing and action |
| --- | --- | --- |
| 0 | Goliath128 / WORKER202 | `(63,63)`, Up, eas_Init |
| 1 | Creed129 / CREED212 | `(19,6)`, Left, eas_Init |
| 2 | Fairy130 / FAIRY219 | `(63,63)`, Up, eas_InitSlow |
| 3 | Eric14 / actual ally class and HP | `(63,63)`, Down, eas_Init |
| 4 | Randolf16 / actual ally class and HP | `(63,63)`, Up, eas_Init |
| 5 | Tyrin17 / actual ally class and HP | `(63,63)`, Down, eas_Init |
| 6 | Karna15 / actual ally class and HP | `(63,63)`, Up, eas_Init |

Parking at `(63,63)` leaves an allocated actor and lookup. It is not recruitment
or hiding. Existing nonzero ally lookups skip their entire resident row, without
a joined/active test. Class and HP choose the actual ally mapsprite; dead allies
outside battle can use a blue flame. Exploration healing does not revive every
dead ally, even though it includes Peter and Lemon among its exceptions.

Followers allocate before residents, from physical1, while Bowie is physical0.
With only F67/F68 among ordinary follower selectors, Peter7 and B31/Oddler occupy
physical1/2, Goliath/Creed/Fairy3/4/5 and Eric/Randolf/Tyrin/Karna6/7/8/9. These
are conditional slots. Other selectors shift them; repeated B rows for F74
Astral, F68 Oddler and F70 Astral are not deduplicated, and the last overwrites
B's lookup. Missing Peter or B leaves lookup0, aliasing Bowie. F69's FOLLOWER_C
Fairy is distinct from resident130. Map33 is not in the overworld map list, so
F65 does not add the overworld Caravan prefix here. Membership alone does not
supply the scene's Peter, and there is no newEntity fallback.

Changed flags matter beyond scene repetition. Baseline has Goliath, Creed and
the same four allies, all parked, with no resident130. F784 has Goliath
`(17,14)` Up, Creed `(17,5)` Down, Fairy `(17,10)` Up and the four parked allies.
F786 instead has a walking WORKER `(22,4)` Up, origin `(22,4)`/range1, CREED
`(17,5)` Down and ODDLER `(17,7)` Up. Its misleadingly named
ms_map33_flag312_InitFunction is just rts. F22 can override that alternate init.
The normal init tests **F523 set and F783 clear** independently of later flags:
it can run restoration with F784 or F22 population, without validating the cast.
A missing Fairy130 can alias main, so a later hide130 can hide the wrong actor.

## Goliath's Collection and the Voices Creed Hears

**Confirmed source sequence, commands 0–51 / texts 1840–1854:** Creed calls Goliath,
then calls louder. Goliath answers, approaches and addresses him. Creed remarks
on the noisy collection; Goliath hesitates and admits adding to it. Creed's anger
is staged with shivering and quake3/0 pairs. Goliath apologizes and explains that
the additions jumped through the magic door; Creed accuses him of leaving it
unlocked and repeats the locking instruction. He then demands to see the
additions, and Goliath agrees to fetch them and moves Down5.

The preparation is significant. The opening text cursor is 1840; reloadMap
`(12,2)` reuses this map's working layout and actors, rather than initializing
a fresh room or making the party small. Bowie, Peter and B are then parked in
that order, Down; fade-in and wait 40 precede Creed's first call. Creed's initial
Down1/Left2 is relative to his current position. Goliath is explicitly placed
`(17,14)` Up, the camera is sent to `(12,4)`, and his Up2 precedes stepEvent
`(17,11)` and Up3. The camera destination is distinct from actor placement.

That step event requests a one-word live-layout copy from `(26,0)` to `(17,11)`,
plus door sound and a plane update, when outside battle. It writes no persistence
flag. The complete flag-event table contains only F22's separate 25x32 copy;
F783 does not replay the opening. A copy request does not establish visible
opening, collision clearance or future traversal. Creed's waited custom speed 48,
Down1, then later speed 16/Up1, faces and nod exchange complete the fetch order;
Goliath's speed 48 precedes his Down5. Operand speeds and sleeps are not measured
pixel rates or elapsed durations.

**Commands 52–88 / texts 1855–1867:** after looking up and waiting 120, Creed reacts
to a sound, looks around, moves Down1/Left1 and hears an unattributed interjection.
He looks again, reports hearing something and moves Left1. Peter asks whether
he is Creed. Creed jumps and recognizes a voice in his collection, identifies
the additions as Goliath's, and hears Peter say they sought him. Peter asks for
his original size. Creed repeats the surprise that they sought him and asks
why; Peter requests enlargement, complaining of neck strain. Creed apologizes,
then slows, disables autofacing, moves Up1, shivers twice with intervening waits
and speaks the incantation.

The research's [complete command table](../../research/creed-restoration-and-basement-departure.md#complete-scene-and-embedded-actions)
fixes every wait, facing and movement in these phases and the rest of the scene.
Its 24 movement bodies and six custom bodies include their terminators; there
is no jump into the next program, cs_5B466. The six custom bodies are waited
speed assignments followed by an idle jump/ac_end. Movement uses signed deltas
from actual current coordinates and checks destination conflict, rather than
assuming the preceding story endpoint was reached. Action installation seeds its
wait timer from the resolved physical index. Waited forms poll that actor's
pointer for eas_Idle without a timeout; they do not join unrelated actors.
RightLeft/UpDown each perform four facings with sprite update/wait 30 before
idle. Jump disables autofacing, moves Up1/wait 5, Down1/wait 5, then relative0/0,
restores autofacing and idles. DeactivateAutoFacing clears that setting and
jumps idle; unrelated subsequent action bytes are not its continuation.

## The Restoration Spell's Concrete Effects

**Commands 89–124 / texts 1868–1871:** four white-flash calls with operand 4 are
separated by waits 40,30,20,10, followed by flash 30. Bowie is placed `(16,9)` Down
and given TwirlGrow, then Peter `(15,9)` Down and B `(17,9)` Down receive the same
action, in that order. After wait 60 and wait 5, Bowie's action is replaced with
RightLeft; after another wait 5 Peter gets UpDown, and after another wait 5 B
gets RightLeft, followed by wait 120. Peter faces Right and says they are normal
again, recalling being small. Bowie nods twice and faces Right, B nods twice, and Creed asks their
purpose. The three face Up in order, Peter asks about Zeon and the Fairy reacts
to that name before her explicit placement.

**Inferred:** this is narrative restoration. The concrete implementation is
mapsprite work. TwirlGrow enables that actor's resize bit, then orders facing,
shared size, sprite update and wait 5 for each of these ten steps:

| Facing sequence | Shared sizes, in the same order |
| --- | --- |
| Left, Down, Right, Up, Left, Down, Right, Up, Left, Down | 6,8,10,12,14,16,18,20,22,24 |

The complete tail disables resize, writes shared size 18 and jumps through
**eas_Init**. Completed Init sets factors 0/0, speed 32/32, acceleration/deceleration
off, obstructability on, entity/map uncollidable off, autofacing on, Up orientation,
immersion off, size 24, sprite update and idle. It does not reset coordinates.
Size 18 is therefore an intermediate write, not a guaranteed lasting size.
InitSlow uses speed 16/16, factors 1/1 and acceleration/deceleration `-2,ON`, then
the same tail; it is not evidence of a measured half-speed animation.

SPRITE_SIZE is shared scratch; resize is a bit on the resolved actor. The sprite
consumer resizes decompressed walking-frame pixels when its branch applies,
bypassing special sprites/entity32; sprite-load capacity can defer updates.
This establishes no combatant body scale, general collision-size rule, stat
restoration or persistent per-actor size. The three TwirlGrow installations
are **unawaited**. Their sleeps do not join all actors before later replacement,
and concurrent actors use the same size word. Aliases, stalled actions, load
pressure or debug skipping can prevent identical rendered growth or the final
Init tail. No explicit removeShadow or independent restored-shadow observation
exists here. Peter's line and the nods do not prove completed growth.

Flash operands also require their consumer: shift-right 3 plus DBF makes each 4
one LaunchFading and 30 four launches. Each selects all palettes, maximum 1 and
FLASH_QUICKLY_2, waits fading plus a VInt and restores the saved maximum; the
handler duplicates palettes afterward. Four/thirty are not frame guarantees.
Quake3/4/0 assign amplitude directly. Shiver synchronously saves size/animation,
uses size 21 and toggles resize/update with Sleep 5 on/off three times, then
restores the saved values; it does not preserve an originally set resize bit.
Nod stops animation, sleeps 10, transforms/DMAs the sprite, sleeps 20, updates it,
sleeps 10 and sets counter 0. Headshake uses seven alternating transformation/update
cycles with VInt waits and finishes at counter 0. These services can wait while
other actions continue; they provide no global action-completion barrier.

## The Question About Zeon Becomes a Wider Crisis

**Commands 125–147 / texts 1872–1879:** the Fairy is placed `(16,0)` Down, approaches
Down7 and repeats the Zeon question. Creed identifies him as the devil king
sealed in Arc Valley. She moves Down1 and asks whether he has revived; Bowie
nods and narration names the runtime leader. She looks around in disbelief.
Creed's Init and Down1 precede facing her Down/him Right and silencing her.
The Fairy disables autofacing and starts Right1 unawaited while Creed's Right1
is waited. Creed faces Down and asks whether the report is true; Bowie's nod
and a second leader-name narration answer. Waiting for Creed does not join
the Fairy's concurrent move.

**Commands 148–177 / texts 1880–1885:** Creed slows, moves Up1 and predicts Zeon's
destructive return. Goliath approaches Up4/Right1/Up3, faces Left and jumps twice
to report the additions missing. The Fairy faces Up and Creed Right; he says
he knows. Creed faces Down, Goliath moves Left1 and asks how, and Creed turns
Right, shivers and dismisses him. Quake4, Goliath's shiver, quake0, two more
jumps and Right1/Down2 stage the reaction. After wait 20 the Fairy's InitSlow,
Up1 and Left facing lead into her response to the crisis; Creed faces Right.

**Commands 178–200 / texts 1886–1896:** Creed reports a dark cloud over Grans and
drought in North Parmecia, contrasting South Parmecia. The Fairy credits
Volcanon and fears for Mitula. Creed says Zeon is the exception to their
invulnerability and emphasizes his power. Goliath faces Up, shivers, disables
autofacing, receives speed 16 and starts Down1 unawaited, reacting fearfully to
Zeon. Creed faces Down and challenges his continued presence. Quake4 accompanies
Goliath's later Init/Left1/Up facing, then quake0. Creed defers scolding him,
turns Left and proposes consulting the Evil Spirit.

**Inferred:** the exchange expands a personal request for restoration into a
shared concern and a plan to seek information. The speakers' statements about
weather, Zeon, Volcanon and Mitula are dialogue, not independently established
world-state facts or new gameplay rules. B is intended as Oddler with F68 but
has no text in this program. There is no consent menu or recruitment command.

### Who Speaks, What Waits, and What the Text Proves

The accepted [58-row text table](../../research/creed-restoration-and-basement-departure.md#text-identities-and-immediate-consumers)
joins every ID 1840–1897 to its command, speaker/modifier, paraphrased meaning and
ordered controls, and to gamescript keys 0730–0769. It is the exact companion
to the phases above, including unattributed 1856 and narrated 1875/1879. The latter
use `{LEADER}` from the first battle-party member after UpdateForce, while the
nod commands address Bowie; supplied membership need not make the names agree.

Creed uses modifier `$80` through 1880, modifier 0 for 1882/1884/1886–1888/1891–1892/
1894, then `$80` for 1895–1896. Goliath, Peter and the Fairy use 0. `$80` requests
a right-side, unmirrored portrait; 0 sets neither bit. Portrait/SFX lookup uses
the actual mapped mapsprite and existing portrait state can suppress opening.
Logical129 alone does not prove a Creed portrait. The unattributed/narrated
`$FFFF` calls bypass entity lookup and set speech SFX0.

nextSingleText closes portrait/text and sleeps 10; nextText leaves continuation
open. The continuation calls are 1849,1851,1859,1861,1863,1886,1887,1889,1891 and 1895.
Their `{W2}` input waits include the continuation-arrow/validation-sound path;
the others use `{W1}`. `{N}` controls line breaks, including `{N}{W1}` at 1892
and double breaks at 1868/1891. Neither wait is a choice. Display calls wait for
view scrolling and advance the cursor after text. ExecuteMapScript clears the
debug text-skip latch on entry, but debug mode/P2 Start can set it: dispatch
then skips csWait, and display handlers skip text **and cursor advancement**.
All sequential text meanings assume no latched skip; neither dialogue coverage
nor enough concurrent-action time is an observed guarantee.

## Leaving the Main Room and Returning to Exploration

**Confirmed source sequence, commands 201–211 / text 1897:** Creed receives waited
Init and the waited Left3/Up2/Left4 departure, then hide. The Fairy receives
waited InitSlow and Left1/Down1, tells the group to follow Creed, then takes the
waited Up1/Left3/Up2/Left4 departure and is hidden. With completed coherent earlier
moves both paths end at `(9,5)` before hide; the unawaited Fairy step remains
a premise. Later Goliath Init can replace his unjoined Down1. These relative
paths do not establish a completed downstairs route.

Hide writes current/destination `$7000,$7000`, invalidates every matching lookup
entry and removes the actor from exploration if present. It neither changes
membership nor writes a story flag or warp. It is stronger than parking at
`(63,63)`. Creed is already hidden when the Fairy delivers her final instruction.

The script then installs Peter→Bowie followed by B→Peter, both direction 2,
which maps to `(-24,0)`. Each followEntity first checks the **high byte of the
encoded follower word**: words 7 and 31 both supply 0, so **both test Bowie's HP**.
Only after admission do the full words resolve actors. This differs from
setPos/setFacing, which skip zero-HP logical allies below 30 but not NPC128–130
or B31. Action installation, nod, headshake and shiver have no such HP skip.
Dead Peter can therefore skip his placement while still receiving actions;
Bowie HP0 can suppress both following relations without suppressing gestures.

AddFollower installs a copied 42-byte action template, sets target/offset and
adds the actor to exploration only if absent. Its executed follower action
resets speed 32/32 and movement flags before following through destination waits.
These calls replace existing actions and establish relations; they do not recruit,
teleport, wait for settling or set persistent follower flags. Missing or aliased
lookups can cause self-follow or replacement of the other relation.

csc_end conditionally waits for view scrolling if the dialogue-window index is
nonzero, clears scrolling speed and returns without joining every actor. The
normal init then completes this ordered tail:

| Guard, in order | Consequence |
| --- | --- |
| F523 set and F783 clear | Run the scene; **after return**, set F783, then write EGRESS_MAP=Hassan9 |
| F22 set | Place Goliath `(14,14)` Up; bypass the next three alternatives |
| Else F784 set | Skip those alternatives |
| Else F783 set | Place Goliath `(17,10)` Down |
| Else F774 set | Place Goliath `(17,12)` Down |
| Independent tail: F784 set | For each clear joined flag, in order: F14 Eric `(15,14)`, F16 Randolf `(16,15)`, F17 Tyrin `(18,15)`, F15 Karna `(19,14)`, all Down |
| End | Return after the selected small placement scripts |

Ordinary completion takes the new F783 Goliath placement and skips the F784
ally tail. Clear joined flags in that tail are separate from placement's HP
guard: a dead ally need not be placed. F783 does not rebuild actors retroactively.
The existing seven-row room remains, with explicit Creed/Fairy hides; future
accessors can select baseline. A fresh ordinary repeat instead rebuilds baseline,
then places Goliath, and skips both the scene and Hassan assignment. Supplied
F783 proves no past restoration and repairs no stale EGRESS_MAP. F523 clear
also skips both writes; with other later flags clear, F774 alone selects the
earlier Goliath placement. F786's selected alternate init bypasses this tail.

RunMapSetupInitFunction restores registers and returns directly, adding neither
the zone wrapper's portrait/text closure nor its physical0-stop wait. Exploration
compares palettes, conditionally requests music/fade, sets movement sound and
enters WaitForEvent. Only MAP_EVENT_TYPE=0 selects camera physical0 and installs
the actual PLAYER_TYPE controller; pending work has priority. The scene writes
no warp/pending event, but supplies no atomic protection against another writer.

With coherent mappings, living Bowie/Peter, completed services and no intervening
displacement or pending event, explicit party placements are Bowie `(16,9)`,
Peter `(15,9)` and B `(17,9)`, with Goliath `(17,10)` Down and Creed/Fairy hidden.
The last explicit party facings are Up; live follower actions can change them.
Usable control still requires the caller and controller to complete. Hassan9
is a selector for later Egress savepoint lookup: it does not transport anyone
or save a new XYZ/facing tuple.

An interrupted prefix is not rolled back. Completed placements, copies, effects
or hides can survive without reaching the caller writes. Interruption between
F783 and EGRESS_MAP can leave completion set with the old selector; later failure
can leave both writes without exploration control. None establishes saved
persistence, basement arrival, an Evil Spirit scene, a recruit choice or Oddler's
departure. This guide stops at the conditional main-room exploration handoff.

## Evidence Review and Remaining Limits

**Confirmed provenance:** accepted main `2eddb2c6d00a748315d9a4671dca8429a8a1ea67`
contains #941/PR944 and #942/PR943. The research pins
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm)
and USA ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Its [reproduction owner](../../research/creed-restoration-and-basement-departure.md#reproduction-and-review-boundary)
retains source symbols/addresses and exact command recipes. The selected program
is cs_5B016 at `$5B016`, scripts.asm 247–535, ending at its own csc_end before
cs_5B466. The complete normal init is `$5AB7C..$5AC56`; the alternate is rts at
`$5AC56`. No running #945 research or unaccepted conclusion is consumed.

Adversarial review covered the whole accepted restoration research and predecessor,
all 212 commands across the phases 0–51,52–88,89–124,125–147,148–177,178–200,201–211,
all 58 meanings/speaker/control rows and 30 embedded bodies via their exact accepted
joins. It checked seven resident identities, complete override precedence,
changed HP/alias/flag/load premises, growth's Init tail, wait/replacement distinctions,
encoded follower guard and every caller placement/write order. The actual public
fixture payloads, schemas and verifier comparisons were inspected as follows:

| Executable owner | Selected evidence and limits |
| --- | --- |
| sf2-map-init-static-v1, tests/fixtures/h2/map-init-static-v1.json | expected.routeJoins/pointerTableJoins for Map33, primarySourceBodies for both init paths and embeddedPrograms for every placement. h2-map-init-static-fixture.schema.json and map_init.py verify_map_init_contract compare the full expected semantic contract and provenance. rts/init order proves no completed control. |
| sf2-map-script-engine-static-v1, tests/fixtures/h2/map-script-engine-static-v1.json | expected.largestPrograms includes cs_5B016/212; dialogue, transition, placement, action bridge, lifecycle, gesture/relationship and presentation facts retain handler order/operand boundaries. map_script_schema.py composes schemas/h2/map-script-engine-*.schema.json; map_script_engine.py _verify_map_script_engine_fixture compares those fields. This does not execute the scene. |

Schemas are under schemas/ and verifiers under src/sf2tool/h2/. Both public fixtures
were directly schema-validated. In particular, the retained followEntity handler
reads a byte before full words, and flash uses shift-right 3/DBF; labels such as
“follower” or “duration” do not replace those encoded consumers. Exact scene/text/
body joins and unrepresented deeper services remain owned by the accepted research.
The narrow reproduction commands are `uv run sf2 h2 map-init` and
`uv run sf2 h2 map-script-engine`, with the research's retained pair recipe.
This design checkout lacks the pinned upstream/pair: accepted comparisons remain
retained results, not fresh reruns, regenerated payloads or a new runtime claim.

The [#941 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/941#issuecomment-6177767348)
and [acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/944#issuecomment-6177797802)
preserve the 0.080s unchanged-pair result, exact source/text/body joins, corrected 1892
control and encoded follower-HP interpretation, original inspection diagnostics
and linked predecessor failures. The [#942 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/942#issuecomment-6177635072)
and [acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/943#issuecomment-6177652147)
preserve the predecessor's checks, absent-input limits and earlier failure records.
Completed failures remain completed; this guide does not restart them.

Direct whole-prose/diff, phase/text/action/order and changed-state review,
links/anchors, English/private boundary and git diff --check apply, followed by
the configured clean committed planner and actual exact-head CI. No blanket local
Python/.NET/Godot/H3/H4 suite or translation check is selected solely for prose.

**Unknown:** natural exit reach; actual flags, party/HP/class/mappings, mode/raft,
layout and pending state; completion of text, motion, sprite, camera, fading,
audio and controller services; rendered growth/shadows/gestures; usable control
and persistence. **Inferred** restoration/departure framing does not close these
gaps. [ADR0014](../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../../decisions/0016-remake-start-evidence-deferral.md) create no
automatic runtime queue. Later basement/recruitment content and remake acceptance
remain outside this guide.
