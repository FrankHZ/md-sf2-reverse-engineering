# Harpy Pond Departure through Fairy Cave Entry

Leaving Harpy Pond requests a return to Devil's Tail. A separate nearby zone can stage Elric
opening a passage: he explains that a fairy told him how to move the rock, celebrates his success
and calls the leader onward. The scene changes the current map, hides its newly created Elric actor
and requests Fairy Cave. Force membership, the passage flag, the live map change and completed
cave entry are separate states; reaching one does not establish all the others.

**Confirmed:** accepted static programs and their consumers establish the conditional order below.
**Inferred:** Elric's speech and the block change depict opening the passage. **Unknown:** natural
travel, supplied actor/party/transport state, completed services, rendered presentation, responsive
input and save persistence. The dialogue is attributed speech, not proof of a prior fairy encounter.

This guide continues [Battle20 outcomes and Elric recruitment](battle20-outcomes-and-elric-recruitment.md)
through accepted [departure and cave-entry research](../../../../research/chapters/southern-travel-creed/harpy-pond-departure-through-fairy-cave-entry.md).
It stops at conditional Map32 entry control, an unrelated pending continuation, or Battle21 admission
before opening. Continue with [Fairy Cave exploration and Devil's Tail return](fairy-cave-exploration-and-devils-tail-return.md)
for the separately scoped local content and returns. Cave exploration, chests, search, rock inspection
dialogue, subsequent movement,
Creed content and battle opening/outcomes are outside this guide. Fnnn means flag nnn; record and
row indices are zero-based. Coordinates, waits and sound IDs are source operands rather than
observed positions, elapsed durations or audible results.

## Leaving the Pond Is Separate from Its Warning

**Confirmed:** Map51's only external warp row matches any X at Y25 and requests a no-scroll return
to Map67 (Devil's Tail), `(57,26)`, Down. Its table does not test F470 or F520. The separate zone
events at `(9,24)` and `(10,24)` run the accepted exit warning when F470 is clear and bypass it when
set. The warning is not a predicate in the Y25 exit row. Whether movement naturally reaches that
row while the warning is active remains **Unknown**.

Ordinary movement selects a working-layout branch before the external table lookup: `$1000`
reaches `WarpIfSetAtPoint`, while `$1400` submits zone event 6 with word coordinates. The external
lookup scans ordered rows, treating negative-byte coordinates as wildcards; a table match does
not establish accessible terrain or player input. Vehicle producers have separate conditions.

The external writer publishes event 1, parameters 1–6 and WARP_SFX. `ProcessMapEvent` clears pending
before dispatch. Its no-scroll consumer submits and clears WARP_SFX, idles physical 0 and returns
to MainLoop with `(67,57,26,Down,d4=0)`. It combines the player's current coordinates with the
nonnegative requested X/Y; it does not normalize PLAYER_TYPE. MainLoop runs SwitchMap and
CheckBattle **before** fresh exploration.

### Battle21 Can Be Selected before Map67 Exploration

**Confirmed:** the complete flag-switched-map table has no Map67 or Map32 substitution. The
complete 45-row battle table has one Map67 row: slot 21, area `(21,0)`, size 32x42, trigger X/Y 255.
Supplied F421 admits Battle21 regardless of area containment; arrival X 57 being outside that
rectangle does not reject it. F521 set clears F421 after selection but does not cancel this
admission. Neither the pond exit nor the passage scene creates F421/F521. Stop before opening if
the supplied state selects this branch. F421 clear allows the fresh-exploration branch below.

### A Fresh Return Rebuilds the Cast

**Confirmed:** fresh ExplorationLoop clears pending, subtracts 20000 from the signed-word step
counter with negative-result clamp to 0, heals eligible living/immortal allies and performs its
fade/resource work. It sets CURRENT_MAP=67 and CURRENT_BATTLE=255, rebuilds entities and sprites,
clears temporary flags 256–383, sets F80, loads map/view, installs base VInts and calls map init.
F13, F470, F520, F751 and follower selectors are outside that temporary reset.

Map67 has one setup pointer table, an empty fixed entity list and a return-only init at `0x4FD4A`.
Shared population still clears old entities/lookups, rebuilds flag-selected followers, declares
Bowie physical 0 at the arrival tuple, terminates the follower list and runs the raft tail. An empty
fixed list does not mean an empty population.

Map67 is an overworld map. With only F65/F67/F68 among follower selectors, physical 1/2/3 are
Caravan/A, Peter and Oddler/B. F66 adds Sarah and Chester; other selected rows retain table order.
F74 declares Astral as B before F68's Oddler row; F70 can overwrite B again. F74/F77/F83 can
overwrite A. The complete follower table has no Elric row. His active or reserve membership does
not declare an exploration follower, and the previous pond scene's temporary actor/following is
discarded by fresh population.

PLAYER_TYPE and saved raft state remain supplied. Mode 2 gives physical 0 the raft sprite, parks
current followers at `(64,64)` with standing actions and declares separate raft physical 31 there.
Other modes declare that raft only with F64 and saved RAFT_MAP=67, using masked saved coordinates.
Mode 1 does not repeat boarding. The population tail neither repairs arbitrary modes nor changes
the saved raft location. Music/fade and pending-zero WaitForEvent can then install the current-mode
controller. Arriving at `(57,26)` does not itself submit its zone callback.

## Elric's Passage Has Its Own Admission Test

**Confirmed:** the ordered Map67 zone table selects three independent branches:

| Submitted zone | Selection |
| --- | --- |
| `(57,25)`, row 0 | F751 set returns immediately. Otherwise F13 clear or Elric HP 0 returns. Joined, living Elric runs `cs_4FC32`; F751 is set only after script return. |
| `(56,26)`, `(57,26)`, `(58,24)`, `(58,25)`, rows 1–4 | F520 set returns. Clear runs the complete accepted call-for-help scene, with its current A/B/Peter lookups, text 2543–2549, movement/relocation and final Map51 request. |
| Other coordinates | Default calls CheckRandomBattle for Battle21; retain its accepted step/random/completion/request limits and stop at the selected continuation. |

The passage callback tests ally 13's current HP word for zero. It has no active-party F45, F520,
F470, class, transport, terrain, follower or fairy-conversation test. A joined reserve Elric with
nonzero HP is eligible. Missing membership and HP 0 are not repaired. F751 set bypasses the later
checks without replaying the scene or immediately reapplying its live block copy.

F520-clear/F13-set/nonzero-HP is not excluded by the passage predicate. F520 controls a different
callback, and Elric thanking a fairy creates no extra history prerequisite. These supplied-state
combinations constrain the explanation; their natural reach is **Unknown**.

Walking's zone producer and the type 6 consumer are separate from the external warp path. Type 6
installs Init on physical 0 and passes the two word coordinates to `RunMapSetupZoneEvent`. That
wrapper selects the ordered coordinate row or default, calls it, closes portrait/text, waits one
VInt and waits for physical 0's current position to equal its destination. A submitted zone coordinate
does not establish Bowie's exact starting position for the scene.

## What the Complete Passage Scene Requests

**Confirmed:** `cs_4FC32` at `0x4FC32` has 33 top-level records and four complete inline Up 1/endActions
bodies. The following sequence includes every selected record. Bowie=0, Peter=7, Elric=13 and
logical FOLLOWER_B=31; B's actual character depends on supplied lookups.

| Records | Ordered speech and action |
| --- | --- |
| 0–3 | Set text cursor 2593; create Elric at `(57,25)`, Up, sprite operand 255; install/wait his Init; request Up 1/endActions and wait for idle. |
| 4–7 | Face Bowie, Peter and B Up in that order. Elric says a fairy told him how to move the rock (2593). |
| 8–14 | Wait 30; submit SECRET_PATH_ROCK (96); wait 30; face Elric Down; install/wait Jump twice, separately. Elric declares success and thanks the fairy (2594). |
| 15–21 | Set quake 1; submit BIG_DOOR_RUMBLE (93); wait 20; face Elric Up; wait 20; submit DESOUL_HOVERING (110); copy a 3x2 block rectangle from `(0,0)` to `(56,22)`. |
| 22–26 | Wait 30; set quake 0; wait 30; face Elric Down. He calls the leader onward (2595). |
| 27–32 | Elric Up 1/endActions with idle wait; hide 13; Bowie Up 1/endActions with idle wait; Bowie Up 1/endActions without idle wait; request Map32 `(29,3)`, Left; end. |

The literal-text joins are 2593/`0A21`, 2594/`0A22`, 2595/`0A23`. Each single-text command uses
modifier 0 and Elric's current mapped portrait/speech resolver. Text 2593 contains one `{N}` and
then `{W1}`; 2594 contains `{W1}`; 2595 contains `{LEADER}` and `{W1}`. The leader substitution uses
the active-force leader, not an unconditional Bowie literal. Modifier 0 sets neither right nor
mirrored bits. There is no choice/refusal, join, heal, class/status reset or persistent follower
selection in this program.

### The New Actor Is Not a New Force Member

**Confirmed:** `newEntity` supplies Init and calls InitializeNewEntity for ally 13. Sprite operand 255
is replaced by GetAllyMapsprite using current HP/class and row 13=ELRIC_SPECIAL (32): ACHR (5) selects 30,
SNIP (21) selects 31 and BRGN (22) selects 32. HP 0 in exploration would select blue flame, but the
callback's earlier HP test normally excludes it. Allocation writes neither HP nor class.

The allocator scans lookup bytes 0–62 with signed-byte comparison from zero, chooses maximum+1
and overwrites lookup 13. It neither checks for an existing Elric nor reuses an old actor; it does
not check membership or capacity. Lookup 63's raft is outside that scan, and negative bytes do not
increase the maximum. A supplied duplicate can leave the old physical actor while later commands
target the new mapping. This differs from fixed-population duplicate checking.

Declaration writes current/destination coordinates at tile×384, facing, sprite, layer/action,
initial flags and zero motion/acceleration; properties are then updated. It does not add Elric to
EXPLORATION_ENTITIES or set F13/F45. With only F65/F67/F68 in fresh Map67 population, new Elric is
physical 4 and Bowie/Peter/B resolve to 0/2/3. Without F65, with only F67/F68, he is physical 3 and
Peter/B are 1/2. Extra flags, retained scroll population and invalid mappings can change these
examples. A cleared lookup 0 can alias physical 0; 255 is not validated as safe absence. Naming B
does not independently prove Oddler's presence.

Hide resolves the newly mapped Elric, parks current/destination at raw `$70007000`, changes every
lookup alias to that physical actor to 255 and removes it from the follower list if present. It
does not clear F13, remove membership, heal Elric or delete a distinct old duplicate. This new
actor was never automatically added to the follower list.

### Actions and Presentation Retain Their Own Completion Limits

**Confirmed:** Init sets speed 32/32, zero acceleration factors, disables acceleration/deceleration,
enables obstruction/collision/autofacing, orients Up, disables immersion, sets size 24, refreshes
and idles. `setActscriptWait` polls for the exact Idle action pointer. Elric's explicit Init wait
precedes his first move.

Facing skips HP 0 allies 0–29, while B (31) bypasses that test; Bowie's and Peter's turns therefore
depend on their own HP and mappings. Init, inline actions, text and hide do not inherit this HP gate.
Inline actions clear flags 5/6 and compile Up 1 to a relative destination one tile (384 internal
units) above the current actor, a destination wait and an Idle jump. With flag 5 clear, this bypasses
the same-destination scan. It does not invoke walking's warp or zone producers. With consistent
actors and completed movement, Elric's first Up 1 reaches `(57,24)`. His later Up 1 reaches `(57,23)`
only if the intervening actions leave him at `(57,24)` and that movement completes before hide.
Bowie's moves are relative to his supplied position; the scene never explicitly places him.

Jump disables autofacing, requests relative `(0,-1)`, waits 5, requests `(0,1)`, waits 5, requests
`(0,0)`, restores autofacing and jumps to Idle. These timed requests are not three destination
barriers or proof of an exact return tile. The scene waits each Jump's Idle pointer, not an
observed trajectory.

Single text waits for view scroll, resolves portrait/speech, displays, increments the cursor,
closes windows and sleeps 10. ExecuteMapScript clears debug skip at entry; debug plus player 2
Start can set it and skip these displays/cursor increments and csWait sleeps. Action waits remain.
Sounds have no completion barrier. Quake operands 1/0 directly write QUAKE_AMPLITUDE; the scroll
consumer uses nonzero amplitude for jitter. None of these calls proves rendered shake, audible
sound, acknowledged speech or completion of every enabled service.

## Live Blocks, F751 and the Cave Request Are Separate

**Confirmed:** `setBlocks` calls CopyMapBlocks to copy six layout words, including their flags,
from the 3x2 source rectangle to `(56..58,22..23)` in the live 64-wide RAM layout (128-byte row
stride), then requests both view-plane updates. It writes no F751 and alone proves neither
passability nor presentation.

Map67's single flag-block row contains the same rectangle gated by F751. A later
LoadMapBlocksAndLayout loads the base map, scans flag rows and reapplies that copy for a set flag.
This loader is separate from setup selection and Map67's return-only init. Setting F751 after an
already loaded map does not itself execute the loader; a skipped zone does not repair current blocks.

The final `warp` writes event 1 and parameters 1–5 as `(no-scroll 0,map 32,X 29,Y 3,Left)`. It neither
dispatches the map change nor sets WARP_SFX or rewrites parameter 6. Script end conditionally waits
for view scrolling when the dialogue-window index is nonzero, clears camera scrolling speed and
restores registers. It is not an all-actor barrier. After script return, the zone callback sets
F751; the wrapper then closes windows, waits one VInt and tests physical 0's position/destination
equality.

The last Bowie action is asynchronous, and the cave request is published before that wrapper
finishes. Its physical 0 test is neither a final-action-pointer wait nor a guarantee that both
upward moves completed under supplied mappings or unfinished services. A stalled wrapper can
leave F751 set and a request pending without reaching cave consumption.

If the request survives until exploration resumes, WaitForEvent returns it without reinstalling
the controller. ProcessMapEvent clears pending; no-scroll consumption submits/clears the supplied
WARP_SFX, idles 0 and returns `(32,29,3,Left,d4=0)`. The explicit cave X/Y do not depend on how far
Bowie's last movement got. A later producer can replace the single pending slot; consumption then
follows that actual request. There is no queue or transactional rollback.

| Supplied or partial state | Supported consequence |
| --- | --- |
| F751 already set | No scene/spawn/live copy/cave request; a later map load independently reapplies the flag blocks. |
| F751 clear, F13 clear or Elric HP 0 | No scene and no repair; Battle20 completion alone is insufficient. |
| F751 clear, joined living Elric, including reserve and F520-clear combinations | Passage admitted without a fairy-history test; later services still conditional. |
| Service stops before block copy | Earlier actor/text/action changes may exist; later blocks/request/F751 are not established. |
| Copy completed but script has not returned | Live blocks can change while F751 remains clear; quake reset, hide and request depend on the stopping point. |
| Script returned but wrapper/consumption unfinished | F751 can be set before cave load/control; motion, request retention and consumption remain separate. |
| Duplicate Elric, missing Peter/B or conflicting follower flags | Passage admission does not normalize the cast or validate every lookup. |

## Independent Cave Entrances and Conditional Arrival

**Confirmed:** Map67's complete external table orders Creed, cave, cave, pond and east-border scroll.
Its selected cave rows are row 1 `(51,22)` → Map32 `(2,3)`, Right, and row 2 `(57,23)` → Map32
`(29,3)`, Left, both no-scroll. These exact-coordinate rows have no F751/F13/HP/F520 predicate;
the movement/layout producer conditions still apply. Row 2 shares the scene's destination but is
a separate request producer. The opposite entrance does not prove a westward route. At `(57,25)`
the external table returns to the pond while the zone table selects the passage; the working-layout
branch distinguishes them, so sharing coordinates does not invoke both.

For a retained scripted or selected external cave request, SwitchMap leaves 32 unchanged and
CheckBattle finds no Map32 row anywhere in the complete battle table, even with supplied F421.
This cave entry does not admit Battle21.

Map32's actual setup fallback is `ms_Void: dc.w $FFFF`. The complete MapSetups table skips 31→33
and has no later 32 row; GetCurrentMapSetup reaches its terminator and returns this sentinel.
GetMapSetupEntityList returns it unchanged, shared population skips fixed rows, and the init
dispatcher returns without a callback. This is table/consumer evidence, not an inference from
a missing guessed filename or a substitute empty story script.

Fresh exploration repeats the pending clear, step adjustment, eligible healing, fade/resources,
entity/sprite rebuild, temporary-flag reset, F80 and map-load sequence with CURRENT_MAP=32 and
CURRENT_BATTLE=255. The temporary passage Elric and any previous map-only following are discarded.
F13/F45/F751 remain outside that reset; Map32's flag-block list is empty.

Map32 is not an overworld map, so F65 does not prepend Caravan. Other selected follower rows
still apply, including Peter/F67 and B/Oddler/F68 and possible lookup overwrites. With only those
two flags, Peter/B are physical 1/2; Elric still has no follower row. The raft tail retains the
supplied mode/F64/saved RAFT_MAP=32 rules. Neither the passage nor cave setup boards, disembarks
or repairs transport mode. Bowie physical 0 is declared at `(29,3)`, Left for the scene/right-hand
entrance, or `(2,3)`, Right for the other entrance.

After the return-only fallback, exploration conditionally submits music/fade. Pending 0 lets
WaitForEvent follow physical 0 and install the controller: mode 0 walking, mode 1 source-named
`eas_Raft` (actual Caravan control), other modes `eas_Caravan` (actual raft control). An unrelated
pending request takes precedence. Controller installation is the conditional entry boundary,
not an observation of usable input or support for arbitrary supplied modes.

Map32's external exits are at these same arrival coordinates: `(2,3)` requests Map67 `(51,23)`,
Down, and `(29,3)` requests Map67 `(57,24)`, Down. Fresh population/init and WaitForEvent do not
call the external warp producer. Walking first needs an admitted directional movement candidate
and the relevant layout branch; vehicle producers remain separate. Coordinate overlap alone proves
no immediate bounce. This guide stops before any new cave movement, exit selection or local content.

## Evidence, Review and Remaining Questions

**Confirmed provenance:** original source is
[SF2DISASM master at `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm),
joined by the research owners to USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
The [research reproduction/review boundary](../../../../research/chapters/southern-travel-creed/harpy-pond-departure-through-fairy-cave-entry.md#reproduction-and-remaining-limits)
owns source paths, commands, all 33 line/macro/argument joins, four inline bodies, literal text and
immediate consumers. This synthesis consumes that accepted result without raw-source reinvestigation,
payload regeneration or new runtime acquisition.

| Surface reviewed | What it supports and what it leaves open |
| --- | --- |
| Accepted departure research, complete scene account and [request consumption](../../../../research/world/map-event-request-consumption.md) / [writes](../../../../research/world/map-event-request-state.md) | Conditional callbacks, live/flag/load order, pending retention/replacement, tuple return and actual Map32 fallback. Natural producer selection/completion remains Unknown. |
| `sf2-map-script-engine-static-v1` ([public fixture](../../../../../tests/fixtures/h2/map-script-engine-static-v1.json)), retained `local/derived/map-script-engine-static.json` selected `programCorpus.programs` id=`cs_4FC32` | All 33 ordered retained records were inspected against accepted research. Public summaries/family facts alone do not expose this whole scene or prove natural execution; the research-owned inline/text/consumer joins remain necessary. |
| `sf2-map-init-static-v1` ([public fixture](../../../../../tests/fixtures/h2/map-init-static-v1.json)) and its retained map-init payload | Setup/dispatcher provenance and retained source-shaped profiles; the accepted complete MapSetups/follower/overworld/battle and selected-table review supplies the Map32 absence/fallback and cast conclusions. Aggregate equality alone does not prove them. |
| [Map exploration](../../../contracts/world/map-exploration.md), [dialogue](../../../contracts/interface/dialogue-system.md), [party/roster](../../../contracts/party/party-roster-state.md), [map-script verifier](../../../../../src/sf2tool/h2/map_script_engine.py), [fixture schema](../../../../../schemas/h2/map-script-engine-fixture.schema.json) and [output schema](../../../../../schemas/h2/map-script-engine-output.schema.json) | Shared ordered-state/command/provenance boundaries, closed public versus full-program shapes and actor-versus-membership distinction. These do not expand controlled seams into a campaign or completed presentation. |

Pre-synthesis review found no owner conflict requiring a new original-game conclusion. The strict
static boundary is retained throughout. Existing configured commands and retained comparisons
belong to the [research reproduction route](../../../../research/chapters/southern-travel-creed/harpy-pond-departure-through-fairy-cave-entry.md#reproduction-and-remaining-limits);
their accepted PASS does not establish natural travel or every prose claim independently.
Preserve the [original receipt](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/876#issuecomment-6171704588),
[rebased receipt](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/876#issuecomment-6171750883)
and [independent acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/878#issuecomment-6171765502),
including their inspection/quoting/path failures, corrections and distinct original/final CI results.
They are review records; the tracked research and exact accepted Git objects own the claims.

**Unknown:** natural route/terrain/producer choice, actual flags/membership/HP/class/lookups/mode/raft,
completed action/text/sound/fade/scroll services, visible passage, usable input and persistence.
Reserve/dead/missing/duplicate actors, supplied flags, debug skip, partial return and pending replacement
are static discriminating cases, not executed bugs. Expansion needs separately accepted evidence;
these Unknowns do not automatically queue runtime work. This guide neither completes the campaign
nor changes remake behavior or acceptance.
