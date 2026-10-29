# Battle26 Admission and the Geshp Confrontation

**Confirmed:** an admitted fresh Battle26 can stage Geshp's confrontation with
Higins and two Pacalon soldiers before battle initialization. The scene presents
Geshp's cruelty, his claim to Higins's body, and an interruption from the
Granseal force. Its field actors and apparent deaths are separate from combat
records, HP, allegiance and recruitment.

**Inferred:** this is a candidate continuation of
[Ketto interactions and merchant relocation](ketto-interactions-and-merchant-relocation.md).
**Unknown:** a natural journey from Ketto, the actual supplied flags and party,
completed services, visible timing, usable control and persistence. This guide
explains accepted source behavior through return from the before-battle wrapper;
it stops before fresh healing, formation, battle loading and start.

The evidence owner is the complete accepted
[Battle26 admission and Geshp confrontation research](../../../../research/chapters/northern-travel-mitula/battle26-admission-and-geshp-confrontation.md).
Its pinned source is SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`
against the USA ROM identity recorded there. Source paths below are relative to
that pin's `disasm/`. Fnnn means flag nnn; indexes are zero-based. Dialogue
IDs identify actual literals in `data/scripting/text/gamescript.txt`; the scene
program is `data/battles/entries/battle26/cs_beforebattle.asm:bbcs_26`.
The prose paraphrases the dialogue while retaining its complete meaningful
content and speaker order; exact controls/literals remain with the research owner.

## From Ketto's Exit to a Selected Encounter

**Confirmed:** Ketto's sole outgoing warp row matches wildcard X255/Y31 and
requests no-scroll Map72, nominal destination (31,27), Down. It tests neither
shopping nor F800, victory 525 or unlock 426. Ordinary walking still needs a
qualifying warp layout kind `$1000` and position. A coordinate row is not proof
of a reachable exit.

The producer writes mutable type 1/PARAM1–5 and warp sound. Consumption clears
the event type before dispatch, consumes/clears sound, idles physical actor 0
and unwinds exploration. The no-scroll consumer samples position, substitutes
nonnegative destination low bytes and applies facing, returning d4=0. Nominal
(31,27) assumes ordinary sampled high bytes. A replacement request or interrupted
consumer can change the result. The accepted
[request consumer](../../../../research/world/map-event-request-consumption.md) owns this seam.

MainLoop runs SwitchMap, then CheckBattle; Map72 has no accepted flag-switched
replacement. CURRENT255 resolves through CURRENT_MAP. The complete Map72
candidate order is:

| Candidate | Unlock / completion | Output crop X,Y,width,height | Trigger X,Y |
| --- | --- | --- | --- |
| Battle8 | 408 / 508 | 1,32,32,30 | 255,255 |
| Battle24 | 424 / 524 | 1,32,32,30 | 255,255 |
| Battle26 | 426 / 526 | 28,0,36,32 | 255,255 |

CheckBattle selects the first unlocked matching row and writes its crop.
These rectangles are outputs, not spatial containment gates. If the selected
row's completion is held, the selector clears its unlock and **still returns
that battle**; it does not continue scanning. F426 cannot outrank held 408/424,
and held 526 does not suppress an unlocked 26. When no row is admitted, MainLoop
enters exploration.

Later Map72 zones at X32/33/34, any Y, select helper 26. Walking zone production
requires layout kind `$1400` and writes type 6/coordinate words; the first matching
row wins. A declined specific callback does not fall through to the default.
The other cave rows and default helper 8/24 selection from F523 remain with the
[predecessor route](battle25-outcomes-through-ketto-entry.md#separate-cave-departures-and-map72-admission).

| Helper 26 state | Result |
| --- | --- |
| 526 clear | Request immediately; no counter test or RNG draw. |
| 526 set, STEP_COUNTER nonzero | Decline. |
| 526 set, counter zero, range 8 draw zero | Request. |
| 526 set, counter zero, range 8 draw nonzero | Draw range 4 + 2, store counter 2–5 and decline. |

A request sets 426, writes `$000100FF` at MAP_EVENT_TYPE (type 1, PARAM1=0,
PARAM2=CURRENT255), and stores STEP_COUNTER=30000. PARAM3–5 remain supplied.
Only afterward does it wait for scroll and request boost sound/flash. The writes
do not depend on completed presentation. A later scan can still select 8/24 first.
F800 and victory 525 neither set 426 nor substitute for helper 26's 526 test.
Neither zone coordinates nor Ketto's completion proves actual traversal.

## Fresh Entry, Held Intro and Supplied Continuation

**Confirmed:** BattleLoop clears PLAYER_TYPE and sets 399 before testing 88.
With 88 clear, it clears seconds, saves incoming map/battle words, stores
CURRENT_MAP=72/CURRENT_BATTLE=26, sets base VInts and calls the before wrapper.
These are earlier caller effects, separate from the scene's local commands.

| Hook | Actual slot 26 selection | Flag behavior |
| --- | --- | --- |
| Before | Relative word at `$47ABC`, table base `$47A88`, resolves `bbcs_26` at `$4B958`. | Check 476; skip when held. Before does not set 476. |
| Start | Relative word at `$47B60`, table base `$47B2C`, resolves `ms_Empty` at `$47A4E..$47A50`. | Check 476; when clear, set it before dispatch. |

The displacements are signed and relative to each table's base. The
[routing contract](../../../contracts/battle/battle-cutscene-routing.md#shared-intro-gate)
owns the shared intro rule; the scene-specific target proof belongs to the
accepted research, not aggregate public route counts. Held 476 skips both
scripts but does not skip all subsequent fresh initialization.

With 88 set, BattleLoop restores saved seconds, clears 88/AI, sets view target 0,
loads supplied current battle state and jumps to the individual-turn loop.
It does not assign incoming 72/26, recreate this confrontation, run before/start,
or perform fresh healing/formation/region reset. The following jump to @Start
is unreachable. This continuation is not a second route through the fresh
before-return endpoint.

## The Scene's Cast and Opening Arrangement

**Confirmed:** the program first sets text cursor 2596, loads Map72 with camera
(2,20), and rebuilds scene entities. Its load/fade sequence waits for fading,
disables/re-enables display/VInts around loading, and waits one VInt. This is
not ExplorationLoop population, healing, temporary-flag reset or map-setup init.
Camera coordinates do not place Bowie, and loading does not change battle 26.

The loader clears actors and the 64-byte lookup, declares eligible followers
from physical 1, then the scene list, then main/Bowie at physical 0. The list
declares Peter, Higins and seven ordinary NPC sprites. These are **logical field
selectors**, even when their numbers resemble combatant IDs:

| Logical actor | Nominal declaration | Role in this scene |
| --- | --- | --- |
| Bowie 0 | (7,25), Right; initially Idle | Main actor; receives explicit waited Init. |
| Peter 7 | (6,26), Right | Fixed ally row unless an existing follower lookup deduplicates it; receives waited Init and later setPos. |
| Higins 19 | (18,14), Up | Pacalon leader confronted by Geshp; declaration does not require joined membership. |
| Geshp 128 / GESHP 159 | (18,11), Down | Questioner, tormentor and departing adviser. |
| Demon 129 / LESSER_DEMON 109 | (17,11), Down | Advances against the second soldier and reacts to the interruption. |
| Bishop 130 / DARK_BISHOP 145 | (19,11), Down | Declared attendant; no separate dialogue or movement command. |
| Lizardman 131 / LIZARDMAN 83 | (20,10), Down | Approaches the first soldier. |
| Cloud 132 / EVIL_CLOUD 106 | (63,63), Down | Initially offscreen, then staged near Higins. |
| First centaur 133 / CENTAUR_MALE 233 | (17,15), Up | Defends Higins, approaches and undergoes the first apparent death. |
| Second centaur 134 / CENTAUR_MALE 233 | (19,15), Up | Challenges the insults and undergoes the second apparent death. |

The list's actions are eas_Init. Fixed allies are deduplicated against an
existing nonzero ally lookup; the follower aliases are not. Let N be the actual
preceding follower count and P=1 when Peter was absent from that lookup,
otherwise 0. Peter's newly allocated row is N+1 when needed, Higins is N+P+1,
and NPC 128..134 occupy N+P+2 through N+P+8. These formulas do not validate
arbitrary supplied flags or guarantee follower capacity.

The opening explicitly waits for Bowie/Peter/FOLLOWER_B Init, places Peter
(6,26), B (6,24), A (6,25), all Right, then requests fadeInB and camera speed 48.
FOLLOWER_A (30) and FOLLOWER_B (31) identify current aliases, not named actors in
every supplied state. F65 can add Caravan/A; B can come from Astral or Oddler
rows. Other accepted follower rows include A/King or Old Man and repeated
B/Astral; duplicate declarations overwrite the lookup while allocating separately.
Only five action pointers exist per follower table, without a count guard.
Excess simultaneous flags do not establish a supported cast.

For example, coherent flags 65/67/70 alone give N=3,P=0: A at physical 1,
Peter 2, B 3, Higins 4 and NPC 128..134 at 5..11. This is an allocation example,
not evidence that this state was supplied. Absent A/B lookup bytes remain 0
and can alias Bowie. Script lookup masks the logical byte, subtracts 96 for
high-bit IDs and uses the resulting physical byte without missing/corrupt
validation. `$FF` is not a safe no-op. The rebuild normally removes old Ketto
mappings, but follower duplicates and later lookup mutation still matter.

F64 plus RAFT_MAP72 can separately declare saved raft at physical 31, with
saved coordinates masked `$7F`. Fresh PLAYER_TYPE=0 excludes the mode 2 override
without clearing those saved facts. Physical 31 and logical FOLLOWER_B (31) are
different namespaces.

Actual class selects ally mapsprites. The dead-blue-flame branch requires
CURRENT_BATTLE=255, so fresh 26 bypasses it even for zero HP; this revives no one.
setPos, setFacing and stopEntity skip dead actual allies 0..29. Aliases 30/31 and
NPCs bypass that guard; action installation, shiver, lookup and text have no
equivalent HP guard. Dead Peter can receive Init but skip setPos; dead Higins
can skip facing/stop while still receiving shiver and the final inline action.

## Geshp Interrogates Higins

**Confirmed source sequence, commands 11–26 / dialogue 2596–2606:** Geshp demands
an answer and asks where Higins is going. The camera moves to (13,9). Higins
hesitates; Geshp asks whether he meant to seek help in South Parmecia. Higins
stammers again. Geshp calls Pacalon's paladins cowards, addresses Higins by name,
questions his pride and calls him a wimp.

The first centaur moves Up 2 then Right 1 in one waited stream, nominally
(17,15)→(17,13)→(18,13), and faces Up. He tells Geshp to be quiet and calls
Mr. Higins the world's bravest soldier. Higins says he can defend himself;
Geshp answers with a half-breed insult. Geshp turns Up, asks whether they fear
him and offers his back so the cowards can flee. The soldier performs a waited
Jump, then declares that Geshp has gone too far.

These questions contain no YesNoPrompt or choice branch. The dialogue describes
the confrontation; it neither offers an escape selection nor mutates courage,
illness or allegiance. The nominal approach assumes the declared origin and
normal progress; the relative-movement consumer uses actual position.

## The First Soldier's Apparent Death

**Confirmed source sequence, commands 27–56 / dialogue 2607–2608:** the soldier
receives waited paired speed 48, then starts asynchronous Up 3. After csWait 5,
Geshp gets MOSAIC_OUT (6); the interpreter explicitly waits for the soldier's
Idle pointer, waits 5, starts his asynchronous Right/Left/Right/Left look-around,
and waits 120. This look-around is an action script, not headshake.

Lizardman faces Left and the soldier Down. After wait 20, Lizardman receives
waited speed 64 and starts asynchronous Left 1. After wait 3, the soldier receives
waited speed 64. Quake 2 accompanies a waited JumpLeft; quake 0 follows, then
another waited JumpLeft. The soldier cries out (2607) and executes waited Die.
The two JumpLeft calls are not proof of an exact horizontal endpoint.

The camera moves to (13,13); Geshp is set to (18,19), Up, and gets MOSAIC_IN (7).
He condemns Pacalon soldiers as dishonorable for attacking while his back was
turned (2608). The second centaur performs a waited Jump; Higins and that
soldier face Down. After wait 30, the second soldier moves Left 1 then Down 1
in a waited stream, nominally (19,15)→(18,15)→(18,16).

The first soldier's Die plays a scene death action and parks him at (63,63).
It makes no combatant HP/status/dead-list or outcome write. Mosaic requests
likewise do not delete Geshp. Apparent disappearance and death retain field
allocation and service boundaries described below.

## The Second Soldier Challenges the Devils

**Confirmed source sequence, commands 57–77 / dialogue 2609–2615:** the second
soldier reacts in disbelief and says Geshp would be defeated if Mr. Higins
were not ill. Geshp asks whether Higins is ill, then invites the soldier to
challenge him instead. The soldier says he can no longer endure the insults;
Higins objects twice.

Demon 129 is set to (13,18), Right, receives waited speed 80, and the soldier
receives waited speed 40. The soldier starts asynchronous Down 2, then Demon
starts asynchronous Right 4. After wait 20, the soldier's new waited speed 64
program replaces his earlier stream without an Idle join. Quake 3 accompanies
waited JumpRight; quake 0 follows, then a second waited JumpRight. The soldier
groans (2615), executes waited Die, and Demon faces Down. Geshp moves Up 2
in a waited stream.

The Demon and soldier's asynchronous motion can overlap. Neither wait 20 nor
later program replacement proves both original paths reached their final tiles.
The second apparent death has the same scene-only boundary as the first;
Higins's illness remains spoken context, not an executable status write.

## Geshp's Identity and Claim to Higins's Body

**Confirmed source sequence, commands 78–101 / dialogue 2616–2628:** Geshp remarks
on Higins's sadness. Higins recognizes that he is more than an ordinary devil
soldier. Geshp agrees, acknowledges Higins's leadership of the Pacalon Army,
and identifies himself as Geshp, an adviser to Zeon. Higins asks how Geshp knows
his name; Geshp claims to know everything. Texts 2622–2623 form Higins's demand
to explain why Geshp is keeping him alive.
Geshp laughs and calls him smart (2624).

Cloud 132 receives SLIDE_OUT (4) while still at its supplied offscreen position;
Geshp receives flashWhite 128,30. The camera moves to (13,11), Cloud is placed
(18,12), Down, and gets SLIDE_IN (5). Higins asks what Geshp did (2625). Cloud
moves Down 1 in a waited stream and gets SLIDE_OUT (4). Higins says he cannot move
his body (2626), then receives shiver and stopEntity.

Geshp says he came to borrow Higins's body and welcomes their new leader
(2627–2628). A second stopEntity is followed by a waited inline Higins program:
motion OFF, orientLeft, updateSprite, jump Idle. Both stopEntity and motion OFF
disable animation; neither is a general velocity stop. Orientation bits differ
from facing. The dialogue and visual sequence support a possession threat;
the commands create no possession, paralysis, enemy allegiance or membership
state and do not recruit Higins.

## The Granseal Interruption and Geshp's Departure

**Confirmed source sequence, commands 102–129 / dialogue 2629–2636:** FOLLOWER_B
shouts for Geshp to stop (2629). The nominal returning-party context can make
this Astral, but the actual operand is B (31) and can resolve Oddler, another
supplied sprite or a missing lookup alias. Neither Peter nor Bowie speaks a
separate line here; their field actors are part of the opening arrangement.

Demon receives waited speed 32 and starts asynchronous Jump. Geshp performs
waited Jump, the camera moves to (13,15), and Geshp moves Down 3 in a waited
stream. After wait 5 he starts asynchronous look-around and wait 120 follows;
then he moves Left 1 in a waited stream. Geshp asks who spoke (2630). The
camera returns to (2,20), waits 60, then moves back to (13,15).

Geshp questions how a force from the south could be here and asks where it
came from (2631–2632). B answers that they are from Granseal (2633). Geshp
begins an unfinished objection that Granseal is on the other side (2634).
The program waits for Geshp's DeactivateAutoFacing action, installs waited
speed 48, and moves him Right 2 in a waited stream.

Geshp decides he should leave (2635), faces Up, and orders the devil soldiers
to stop the Granseal force (2636). He faces Down; Demon starts another
asynchronous Jump; Geshp receives MOSAIC_OUT (6) and the script ends. No final
join waits for Demon's Jump. Geshp's departure is a visual request, not
deletion, battle victory or a control grant.

The opening and five confrontation stages retain all 41 dialogue IDs 2596–2636 and the meaningful
ordered action sequence. The research owner's
[complete ordered program](../../../../research/chapters/northern-travel-mitula/battle26-admission-and-geshp-confrontation.md#complete-ordered-program)
retains the exhaustive 130-command table and source-width accounting; it is
the route for exact encoded detail rather than another copied encoding table.

## What Waits, Movement and Effects Actually Mean

**Confirmed:** waited action installation and waitIdle poll for the actor's
action pointer to equal eas_Idle. They depend on entity-service progress and
are not themselves destination tests. Packed movement streams add signed tile
deltas×384 to actual current position, derive travel/velocity from stored speed,
wait for position=destination between moves, then jump Idle. Installing these
streams clears FLAGS_A bits 5/6; bit 5 had gated the obstruction helper.

eas_Init configures speed 32/32, clears acceleration factors, disables acceleration/
deceleration, enables entity obstruction, disables entity/map uncollidable
settings, enables auto-facing, orients Up, clears immersed, sets shared size 24,
updates sprite and reaches Idle. Speed 48/64/80/40/32 in later programs changes
paired speed bytes, not distances or frame counts. Replacing an action stream
can abandon an earlier destination sequence.

Jump temporarily disables auto-facing and requests relative (0,-1), (0,1),
(0,0), with timer waits 5/5. JumpLeft uses (-1,-1)/(-1,1), JumpRight
(1,-1)/(1,1), with waits 6/6. They do not wait for destination completion between
legs: each next request uses the position then reached. A returned waited action
establishes an Idle pointer at that poll, not exact tile displacement or
observed duration. The look-around alternates Right/Left/Right/Left with waits 30
after each; wait 120 does not join it under arbitrary timers/service state.
DeactivateAutoFacing is OFF then jump Idle; subsequent bytes from that entry
are unreachable.

setPos writes position/destination/facing and requests sprite update but does
not clear velocity/travel or replace actions. setFacing writes facing/update.
stopEntity writes ANIMCOUNTER=-1. Higins's motion OFF uses the animation-disable
consumer; orientLeft changes orientation bits before explicit sprite update.
These distinctions matter after changed state or interrupted action progress.

Die disables animation, cycles Left/Up/Right/Down three times with explicit
updates and eleven waits 3, sends battlefield-death sound, changes mapsprite to
EFFECT1, cycles Up/Left/Down with updates/three waits 8, sets position and
destination (63,63), clears travel and jumps Idle. It writes no HP, status,
membership or combat outcome. `$5E80` (63×384) remains below the `$7000`
inactive-X threshold, so both parked soldiers and the initially staged Cloud
remain allocated and eligible for entity updates.

Camera destinations set view target-1 and wait for view scrolling; speed writes
VIEW_SCROLLING_SPEED=48. Quake 2/3/0 directly writes amplitude without ramp bits.
The normal path contains both zero clears; interruption after a nonzero write
need not. Mosaic 6/7 performs 16 mask/load/DMA iterations with three VInt waits
each. Slide 4/5 performs 23 crop/transform/DMA iterations with two waits each;
slide-out adds a final all-masked DMA request. None removes or relocates actors,
clears action pointers or stops their updates.

flashWhite 128,30 yields eight set/update/wait/clear/update/wait iterations,
with 32 explicit VInt waits. Shiver temporarily uses shared size 21/counter-1,
runs three flag-bit 3 on/off cycles with Sleep 5 each, then restores the saved
size/counter; it has no ally-HP guard. These are ordered requests and bounded
source loops, not measured total duration, completed DMA/audio, or guaranteed
rendered results. A hidden Geshp or Cloud can still receive other sprite updates.

## Dialogue Names, Windows and Skip State

**Confirmed:** `{NAME;19}` reads ally 19's actual combatant name. It does not
look up physical scene actor 19 or guarantee the unchanged English name Higins.
The research compared actual hex keys 0A24..0A4C, not dialogue comments alone.
The [dialogue contract](../../../contracts/interface/dialogue-system.md) owns names, commands,
windows and the separate reached text-wait boundaries; those observations
are not new runtime evidence for this scene.

nextText retains the window; nextSingleText closes portrait/text and Sleep 10
after display. Each normal call prepares portrait/speech, waits for scroll,
displays the actual literal and increments the cursor. From 2596, all returned
non-skipped calls leave 2637. Macro choice does not replace literal controls:
2611 is nextText with W1, 2622 is nextText with a trailing newline and no wait
control, and 2635 is nextSingleText with W2. Other nextText calls are 2600, 2609,
2618, 2627 and 2631; all remaining calls use nextSingleText. W1/W2 acknowledgements
do not create choices. There is no choice opcode or YesNoPrompt in this program.

Geshp uses portrait control `$00`; all other speakers use `$C0`. Portrait/bleep
resolution follows actual logical lookup, physical mapsprite and sprite-dialogue
properties. Nominal Geshp and HIGINS_PROMO select their named portraits/bleep 6;
CENTAUR_MALE has no portrait/bleep 6. B can select Astral/bleep 6 or Oddler/bleep 4;
missing property defaults portrait/bleep 6. A changed class/sprite/alias changes
that context. An already open PORTRAIT_WINDOW_INDEX can prevent opening a new
portrait even while speech resolves again; a logical name alone proves no
rendered portrait.

ExecuteMapScript clears SKIP_CUTSCENE_TEXT at entry; debug mode plus P2 Start
can set it at the command loop. Skipped nextText/nextSingleText consume the
speaker operand but do not display, increment the cursor, close windows or
Sleep 10. Negative csWait operands likewise skip their Sleep. Loads, motion,
effects and camera commands still execute. Midscene skip retains only the
completed cursor prefix and can change asynchronous overlap; it is not proof
of equivalent timing or a guaranteed 2637 cursor.

## Direct Writes and the Return Boundary

**Confirmed:** the local scene has no story set/clear flag, JoinForce,
HP/status mutation, combatant creation, outcome, inventory/payment, save,
map-event request or choice/branch. Earlier admission can set/clear unlocks,
and BattleLoop sets 399; the before scene does not set 476 or completion 526.
It changes field population, positions, scripts/speeds, animation/orientation,
camera/fade/effect/text fields through the consumers above. There is no rollback.

csc_end conditionally waits for scrolling with an open dialogue window, clears
VIEW_SCROLLING_SPEED, restores interpreter registers and returns. The before
wrapper restores d1 and returns at loc_47AE8. The tails do not restore all actors
or speeds/effects, remove scene allocations, join Demon's final asynchronous
Jump, initialize combat or grant player control.

The next accepted caller boundary is
[fresh BattleLoop initialization](../../../../research/chapters/northern-travel-mitula/battle26-admission-and-geshp-confrontation.md#before-dispatcher-and-exact-endpoint):
only after this return does the caller restore/reassign saved map/battle, reset
region flags 90–105, heal living/immortal allies, initialize ally then enemy
positions, clear AI, LoadBattle for combatant 0 and invoke start. The scene
establishes none of the initial combatant/neutral roster, activated enemies,
turns, outcomes or Higins recruitment.

[Battle26 initial formation and neutral Higins](battle26-initialization-and-roster.md)
continues from this return through actual party/enemy initialization, physical
replacement, completion-gated neutral Higins and the empty start hook. It stops
immediately before ActivateEnemies, retaining supplied-state, upgrade, lookup,
service and presentation limits. Activation, turns, outcomes and recruitment
remain separate scope.

| Changed state or interruption | Consequence that must remain visible |
| --- | --- |
| Earlier 408/424 held or 426 absent | The requested Map72 does not necessarily select 26. |
| 526 held | Selector unlock cleanup still returns its selected battle; helper RNG/counter gating is a different operation. |
| Replaced request, changed map/layout/mode | No unconditional Ketto→26 route. |
| 476 held | Before population/confrontation skipped; later fresh initialization remains. |
| 88 set | Load supplied continuation, without this scene or fresh endpoint. |
| Changed followers/A/B aliases, Peter present/absent | Actual mappings, deduplication and capacity govern the cast. |
| Dead or changed-class allies, missing/corrupt lookup | Sprite selection and per-command guards differ; zero can alias main. |
| Changed origin/speed/timers or interrupted movement | Relative destinations/async overlap differ; new scripts can replace old ones. |
| Die completed or Cloud at 63,63; visually hidden sprites | Allocated actors continue updating below the inactive sentinel. |
| Existing portrait or midscene text skip | Windows, speech and cursor differ; nontext commands still execute. |
| Interrupted request/load/speed/quake/effect prefix | Earlier writes survive without completed services or rollback. |
| Before wrapper returns | No roster, start flag, turn, victory, control or Higins join is established. |

## Evidence Review and Retained Results

The complete accepted research was reviewed against its ordered program,
all nine declarations/main header, all 41 literal entries and changed-state
matrix. The staged explanation preserves both soldiers, actual A/B aliases,
class/HP/lookup distinctions, asynchronous replacement, the animation-only stop,
skip cursor and direct-write limits. Selected existing public surfaces were
inspected with their schemas and maintained consumers:

| Surface | Reviewed owner and public limit |
| --- | --- |
| Admission/helper | `sf2-battle-global-data-static-v1` and `sf2-map-event-random-battle-state-static-v1`; global factSummary is not the private Map72 rows, and helper rules do not prove traversal. |
| Before/start routing | `sf2-battle-cutscenes-static-v1` and `sf2-battle-routing-data-static-v1`; shared intro checks/write order is public, but routeTargetsParsed=false does not prove slot 26 targets. |
| Loading/population | `sf2-map-init-static-v1` and `sf2-map-script-engine-static-v1`; lifecycle, population and placement consumers do not establish a natural cast or combat roster. |
| Actions/camera/effects | Selected map-script action-bridge, lifecycle-presentation, gesture, camera and FX facts; waits and field writes do not prove completed animation/presentation. |
| Request/dialogue | `sf2-map-event-request-consumption-static-v1`, `sf2-map-event-dialogue-state-static-v1` and the dialogue contract; mutable requests and supplied text context do not prove delivery/control. |

The [battle verifier](../../../../../src/sf2tool/h2/battle_cutscenes.py),
[routing verifier](../../../../../src/sf2tool/h2/battle_routing_data.py),
[global-data verifier](../../../../../src/sf2tool/h2/battle_global_data.py),
[random-battle consumer](../../../../../src/sf2tool/h2/map_event_random_battle_state.py),
[map-script verifier](../../../../../src/sf2tool/h2/map_script_engine.py),
[map-init verifier](../../../../../src/sf2tool/h2/map_init.py),
[request consumer](../../../../../src/sf2tool/h2/map_event_request_consumption.py) and
[dialogue-state consumer](../../../../../src/sf2tool/h2/map_event_dialogue_state.py)
own those fixtures and schemas. Their focused owning commands are `uv run sf2 h2`
with `battle-cutscenes`, `battle-routing-data`, `battle-global-data`,
`map-event-random-battle-state`, `map-script-engine`, `map-init`,
`map-event-request-consumption` and `map-event-dialogue-state`.

Public validation and prose review are **not a fresh comparison** of private
rows, scene targets or literals. Source/pair absence in this design environment
is accepted. No acquisition/regeneration/runtime/environment change or new
maintained machinery follows. No in-scope contradiction was found; a source
disagreement returns to research rather than being repaired in Layer B.

[#1001 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1001#issuecomment-6181719337)
and [PR1004 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1004#issuecomment-6181778859)
retain head `1ac04fb9`, tree `dab707d3`, base `af894656`, CI 38661497541
(scope 11s/research 34s), original CI 38661384831, the 0.082/0.080s retained-pair
checks and all 130 command/9 entity/41 literal joins. The fixture-layout and
HTTPS-check corrections remain recorded, along with the
[removed ignored Markdown cap](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1001#issuecomment-6181773472).

[#1002 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1002#issuecomment-6181676037)
and [PR1003 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1003#issuecomment-6181691009)
retain head `1d73f70d`, tree `f5a58e2a`, CI 38661190951 (scope 15s), eight
schemas plus composed map-events/539 links, unavailable source/pair and completed
diagnostic corrections. The
[Ketto retained review](ketto-interactions-and-merchant-relocation.md#evidence-review-and-retained-results)
keeps earlier actor 0 waiting, stock-index corrections, completed failures and
Unknowns with their owners. No completed suite is reset.

The #1006 Issue/PR handoff records full prose/diff/navigation/link/anchor,
UTF-8/private/exact-five-path checks, git diff --check, configured clean committed
planner and actual exact-head CI. Documentation scope launches no blanket
normal/full/Python/.NET/Godot/H3/H4/translation suite. Natural reach, supplied
state, service completion, rendered timing/control and persistence remain
**Unknown**, without creating an automatic acquisition queue. Independent
acceptance and integration remain with main-gate.
