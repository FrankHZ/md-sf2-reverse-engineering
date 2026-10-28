# Battle21 Outcomes and Devil's Tail Return

Battle21 can end after its leader dies while Bowie survives, without the force individually killing
all twelve initial enemies. On the first eligible victory, the after scene turns from combat to
Peter's questions about the moving companion and their intention to see Mr. Creed. A completed
scene requests a return to Devil's Tail at `(26,23)`, Down. Entering the mansion remains a separate
step; repeat victory skips this conversation and uses the current force member's seeded return.

**Confirmed:** accepted static evidence establishes the outcome checks, complete selected scene and
conditional return consumers below. **Inferred:** the companion's responses are intended as Oddler's
part of the conversation. **Unknown:** the natural outcome, actual supplied state, completed services,
rendered scene, usable control and saved persistence. Dialogue about sight does not establish a
physiological fact or later lore, and a requested return is not a played itinerary.

This guide continues [Battle21 opening and initial roster](battle21-opening-and-initial-roster.md)
and consumes [accepted outcome research](../../research/battle21-outcomes-and-devils-tail-return.md).
It stops at conditional Map67 exploration/control or pending return, or the supplied defeat/escape
destination's continuation. Mansion initialization/interior, Battle22, later recruitment/lore,
damage/AI/drop/terrain reconstruction and remake implementation remain outside it. Fnnn means flag
nnn; command indices are zero-based. Coordinates, speeds and waits are source operands.

## The Leader Cleanup Can End the Encounter

**Confirmed:** Battle21's leader-presence table slot is -1. After an individual turn, the defeated
hook first requires Bowie's HP to be nonzero and enemy 128's HP to be zero. With F521 clear it calls
Battle21's empty defeated program, `ms_Empty`; with F521 set it bypasses that program. Either admitted
path reaches the leader tail. The tail appends every nonzero-HP enemy among 128–159 to the death
worklist, maintaining its length and terminator. It neither tests placement X nor zeros their HP.

The subsequent killed-combatant consumer performs death bookkeeping/presentation work, moves those
combatants' X/Y to -1, clears status, updates stats and moves their mapped actors off-map. Counts then
exclude them because placement X is negative, even if their HP remains nonzero. This is different
from the debug kill-all path, which writes HP zero. Under coherent state, killing leader 128 with
Bowie alive can therefore reach victory without defeating each remaining enemy individually; Bowie
does not have to deliver the leader's killing blow. No Battle21 staged-position override is selected.

Both faction counts inspect placement X and positive HP: 30 ally slots and 32 enemy slots. Bowie's
zero HP forces the ally result to zero. At each outcome check the loop tests zero allies before zero
enemies, so **both-zero resolves as defeat**. Bowie dead also prevents the leader cleanup hook.

The full post-action sequence is defeated hook → killed pass → ordered counts. If both counts remain
nonzero, after-turn effects run, followed by another killed pass and the same ordered counts. There
is no second defeated hook at that point. A leader reaching HP zero in after-turn effects does not
itself invoke the leader tail at that second check. Later execution and supplied state matter, and
an earlier hook/death service must return before the counts are reached.

The complete region-script table has entries for battles 32/40 only; Battle21 returns without a
region cutscene. This does not say its AI regions are inactive or reconstruct their combat behavior.

## First Victory and Repeat Victory Have Different Returns

**Confirmed:** the victory path first heals living allies and immortal Peter 7/Lemon 28 across all
30 slots to maximum HP/MP, retains only existing STUN/POISON/CURSE bits and updates stats. It joins
nobody and does not revive other dead allies. It then seeds the shared return parameters before
checking F521:

- Map is CURRENT_MAP; X/Y are the first active member's combatant coordinates plus BATTLE_AREA_X/Y,
  using byte arithmetic.
- Facing comes from that member's mapped battle entity; parameter1 is zeroed.
- UpdateForce rebuilds joined/active lists in ascending ally-ID order before the first active byte
  is read. This is neither a first-living search nor unconditional Bowie selection; there is no
  empty-list repair.

On coherent Battle21 entry the map is 67, source area origin `(21,0)`, size 32x42. The seed adds that
origin to current combatant coordinates, rather than reusing Fairy Cave arrival or initial placement.
Changed party, position, facing lookup or saved geometry can change it.

| Outcome path | Ordered consequence |
| --- | --- |
| First victory, F521 clear | Heal and seed; run all of abcs_battle21, including its warp override; common JoinForce(0); clear F421; set F521; read current return bytes; return d4=1. |
| Repeat victory, F521 set | Heal and seed; skip the entire after scene, its cast/text/moves/follows/warp; common JoinForce(0) and the same completion writes/read/result. The seed survives unless another writer replaces it. |

F421 is admission/unlock state, F521 is completion and the after-scene gate, and F471 is the separate
opening/start flag. F471 neither gates this after scene nor changes at victory; F471 clear/F521 clear
still selects it. F521 set skips it regardless of F471. Neither flag selects alternate Map67 setup.

The shared after tail's slot 21 contains 0: JoinForce receives Bowie, not a newly named recruit. It
sets joined F0, rebuilds force lists, and activates F32 only when the resulting active count is below
12. A full party with F32 clear leaves Bowie reserve; an already set F32 is not cleared at capacity.
No duplicate member is appended or existing member displaced, and JoinBattleParty does not immediately
rebuild the lists a second time. This call has no join text/music, healing or class/equipment/position
change. It cannot repair which member supplied the earlier seed. The after program does not recruit
Peter or B, and force membership does not select exploration followers.

## The Scene Uses the Battle Crop; the Return Uses the Full Map

**Confirmed:** the after wrapper keeps CURRENT_BATTLE=21. Its loadMapFadeIn reloads Map67 with camera
`(4,18)` while that battle value still makes the loader copy the supplied rectangle into working
origin `(0,0)`. The battle view uses layer-1 bounds ending `(31,41)*384`, rather than ordinary full-map
bounds. Loading the same map number does not imply the same coordinate domain or population.

| Scene operand | Cropped working position | Corresponding source-map position on coherent area `(21,0)` |
| --- | --- | --- |
| Main Bowie | `(10,22)`, Left | `(31,22)` |
| Fixed Peter | `(11,23)`, Left | `(32,23)` |
| B setPos | `(11,21)`, Left | `(32,21)` |
| A setPos | `(11,22)`, Left | `(32,22)` |

setPos directly writes tile×384 current/destination coordinates and facing; it does not add or
subtract the battle origin. Camera `(4,18)` is packed/multiplied by 3 and consumed as block units×128;
it is a view operand, not a player arrival. Inline moves are relative to the mapped actor's current
coordinates. Source correspondences above explain storage, not terrain/passability or rendered view.

The final warp writes literal Map67 `(26,23)`, Down into the shared request slot. It does not move
scene actors, add 21 or clear CURRENT_BATTLE. Only after the complete scene and wrapper return does
victory return the current parameters. Fresh ExplorationLoop later sets CURRENT_BATTLE=255 before
loading Map67, so that normal arrival is full-map `(26,23)`, **not** cropped 26 or source X47.
BATTLE_AREA fields are not recomputed by the scene or warp; the exploration loader now ignores them.

## The Cast Depends on Follower Mappings

**Confirmed:** abcs_battle21's `ce_4B948` contains the main header above, one fixed Peter row with
eas_Init, and the terminator. There is no fixed A, B or enemy row. Its loader clears old entities and
lookup bytes, declares flag-selected followers, processes fixed Peter, then declares Bowie at
physical 0 with eas_Idle and loads sprites. It does not run ordinary Map67 setup/init.

A and B mean logical FOLLOWER_A 30 and FOLLOWER_B 31. On overworld Map67, F65 selects Caravan/A, F67
selects Peter and F68 selects Oddler/B. With only those selectors set they occupy physical 1/2/3,
and Peter's nonzero lookup skips the fixed row. Without F67 the fixed row still declares Peter after
the selected followers. That duplicate check tests the lookup, not HP, joined status or active party.
F66 adds Sarah/Chester; other selectors can add actors and replace lookups. In particular F70's later
Astral/B row can overwrite Oddler's B lookup; later A rows can overwrite Caravan's. F421/F471/F521 and
party membership establish none of these follower flags.

Cleared missing lookups are 0. GetEntityAddressFromCharacter uses the masked logical-ID lookup
(enemy IDs subtract 96) without a missing-actor guard; absent A/B therefore alias physical 0. A's
setPos or B's action/text can affect Bowie. A mapping 255 is not a validated absence sentinel.
The fixed Peter row supplies a scene fallback for logical 7, but there is no A/B fallback.

True ally IDs 0–29 have HP guards on setPos/setFacing; zero-HP Bowie/Peter skip those writes. A/B
bypass that guard, and actions/text/nod do not inherit it. Class-sensitive ally sprites remain in use
while CURRENT_BATTLE=21, even at zero HP; declaration is not revival. Coherent ordinary victory has
living Bowie and previously healed Peter, but direct/supplied altered state is not normalized by the
scene. Neither the logical speaker nor a follower name guarantees the displayed actor's identity.

## Peter's Questions and the Complete After Scene

**Confirmed:** all 33 top-level commands, the custom speed body, both inline moves and the entity
table belong to the accepted research. The full program proceeds as follows:

| Commands | Scene progression |
| --- | --- |
| 0–2 | Select text cursor 2583; load Map67 with camera `(4,18)`; rebuild ce_4B948. |
| 3–9 | Install/wait Bowie Init, then Peter Init; set Peter `(11,23)` Left; install/wait B Init; set B `(11,21)` Left and A `(11,22)` Left; fadeInB. A receives no Init/wait. |
| 10–13 | Install B's custom body: ac_setSpeed 28,28; ac_jump eas_Idle; ac_end. Wait for idle; csWait 5; start eas_2xRightLeft without an idle wait; csWait 120. |
| 14–16 | B inline moveLeft 3, moveDown 1, moveLeft 1, endActions, with idle wait; csWait 40; face B Right. |
| 17–22 | Six nextSingleText calls alternate B/Peter for 2583–2588. B's upper operand byte is `$0`; Peter's is `$C0`. |
| 23–27 | Peter inline moveLeft 1/endActions with idle wait; face Peter Up; Peter's single text 2589 with `$C0`; face Bowie Down; nod Bowie. |
| 28–32 | Install follows A→Bowie relative 2, Peter→A relative 1, B→A relative 3; warp Map67 `(26,23)` Down; csc_end. |

With consistent mapping, admitted placements and completed movement without intervening displacement,
B requests `(11,21)`→`(8,21)`→`(8,22)`→`(7,22)` and Peter `(11,23)`→`(10,23)`.
A retains its supplied action while setPos changes current/destination coordinates; no command
freezes or waits for A. Following may move the cast again. These are conditional destinations.

The seven literal keys `0A17`–`0A1D` contain the complete exchange:

| Decimal / hex | Speaker lookup | Literal text and controls |
| --- | --- | --- |
| 2583 /0A17 | B | `Hurry!  Let's go into{N}Creed's mansion.{W1}` |
| 2584 /0A18 | Peter | `Oddler, can you see?{W1}` |
| 2585 /0A19 | B | `No, why?{W1}` |
| 2586 /0A1A | Peter | `You're moving around as{N}if you can see.{W1}` |
| 2587 /0A1B | B | `Am I?  Maybe my other{N}senses have sharpened{N}since I lost my sight.{W1}` |
| 2588 /0A1C | Peter | `Oh, I see.{W1}` |
| 2589 /0A1D | Peter | `{LEADER}, let's go see{N}Mr. Creed!{W1}` |

**Inferred:** Peter's address identifies the intended dialogue partner as Oddler. **Confirmed:** the
commands address the current B lookup, which need not resolve to Oddler under changed flags. B's
account of sight remains dialogue; there is no independent physiological conclusion, choice branch
or explicit recruitment. Bowie's nod is an animation command, not consent or membership mutation.
`{LEADER}` rebuilds force lists and uses the first active member's name, rather than a fixed Bowie
literal. `{N}` is newline; `{W1}` is an input-wait service, not proof of acknowledged input.

### Actions, Portraits and Waiting Still Have to Complete

**Confirmed:** eas_Init sets zero acceleration, speed 32/32, acceleration/deceleration off,
obstructable/collidable settings, auto-facing on, orientation Up, no immersion, size 24, sprite update
and an idle jump. Orientation is separate from facing. B's custom body then writes speed 28/28;
there is no intervening Init before its inline movement. eas_2xRightLeft refreshes Right/Left/Right/Left
with wait 30 after each, then enters idle; it neither moves the actor nor restores speed.

csWait 5/120/40 submits Sleep counts. In particular 120 is not an independent completion barrier for
the four look waits before the inline action replaces that script. Action helpers install a pointer
and, when requested, poll for exactly eas_Idle without an HP guard. Inline movement clears action
flags 5/6, compiles relative tile×384 destinations with destination waits, and appends an idle jump.
Flag 5 clear bypasses the destination-conflict scan. These moves neither dispatch ordinary player
zone/warp events nor prove natural traversal. The nod stops animation, sleeps 10, transforms/DMAs the
sprite, sleeps 20, refreshes, sleeps 10 and restores animation.

Each nextSingleText resolves portrait/speech from the mapped actor's current sprite, waits for scroll,
displays/increments the cursor text, then closes portrait/text and sleeps 10. Peter's `$C0` sets
packed bits 15/14, consumed as right-side and mirrored portrait toggles; B's `$0` sets neither.
Existing-open and missing-portrait branches still apply. Speaker/modifier alone proves no rendered
portrait. ExecuteMapScript clears debug text skip at entry; debug plus P2 Start can suppress ordinary
text display/cursor increments and csWait, while custom/inline actions and warp remain.

At csc_end a nonzero dialogue-window index triggers a scroll wait, then camera speed is cleared and
registers restored. This is not an all-actor barrier. Fade/load/display/VInt, movement, scroll,
portrait/text and input services must return for later commands to execute. A stall does not roll
back earlier RAM writes or establish successful scene completion.

### Temporary Following Is Replaced at Return

**Confirmed:** the three followEntity calls install these ordered logical relations:

| Follower → target | Relative operand / table offset |
| --- | --- |
| A 30 → Bowie 0 | 2 / `(-24,0)` |
| Peter 7 → A 30 | 1 / `(0,-24)` |
| B 31 → A 30 | 3 / `(0,24)` |

The consumer passes the first byte of each follower word to its alive guard; for 30/7/31 that byte
is zero. These commands therefore test **Bowie's HP**, not the named follower's. Zero-HP Bowie skips
the respective call. Otherwise AddFollower installs a 42-byte eas_Follower1 copy with physical target
and offset operands, appending the physical follower to EXPLORATION_ENTITIES only if absent. Fixed
fallback Peter may be added here. Missing A can make Bowie follow himself, and aliased lookups can
replace an earlier action. There is no alias rejection, membership change or persistent selector write.

The template sets speed 32/32, zero acceleration, acceleration/deceleration/obstructability off and
uncollidable bits on, then loops follow/destination wait. B's speed 28 is replaced when it executes;
installation does not prove execution before transition. The follow consumer uses target-relative
distance/facing/destination/layout: separation at most `$1E0` retains the destination; otherwise it
tries a rotated offset, zero-lateral fallback and, if blocked, the target destination. Relative offsets
are not absolute landing tiles, and none of these calls waits for settling.

## A Pending Warp Is Not a Completed Victory

**Confirmed:** csc07 writes MAP_EVENT_TYPE=1, parameter1=0 and literal map/X/Y/facing bytes into one
shared slot. It overwrites any previous request; there is no queue. It does not load/fade the map,
clear CURRENT_BATTLE, write F521 or itself end the script. The earlier victory seed zeroed parameter1
without clearing MAP_EVENT_TYPE, so repeat victory need not produce a type1 event.

After full scene return, or the F521 bypass, the shared JoinForce(0) tail returns. Only then does
BattleLoop clear F421, set F521 and read parameters2–5 into the d4=1 battle result. It reads current
bytes without type validation or an immutable warp copy. Replacement before that read can change the
returned tuple. A stall before warp leaves the seed or another request; a stall after warp can leave
a pending event without completion flags or a returned result. Neither partial state promises return.

## Fresh Devil's Tail Population and Conditional Control

**Confirmed:** MainLoop's alternate return calls SwitchMap then ExplorationLoop without CheckBattle.
Map67 has no flag-switch row, so its normal returned coordinates/facing survive. Even if CURRENT_MAP
was already 67, this selects fresh exploration and replaces the after-scene population.

Exploration clears MAP_EVENT_TYPE, subtracts 20000 from STEP_COUNTER with a negative clamp, heals
eligible allies and begins fade/view changes. For provided Map67 it assigns CURRENT_MAP=67 and
CURRENT_BATTLE=255, loads tilesets/waits fade, clears VInts, selects/initializes setup entities at the
returned position/facing and loads sprites. It clears temporary flags 256–383, sets F80, initializes
sprite-frame state, selects view target 0, loads the full map/view, installs base VInts and runs init.
Music/fade depend on palette comparison before WaitForEvent. Pending zero selects physical 0 for
view following and installs the current-mode controlled action; nonzero pending takes priority.

Map67's complete setup has no flag alternate. `ms_map67_Entities` contains only its terminator and
`ms_map67_InitFunction` only rts. F471/F521 select neither another cast nor another scene; init repairs
no missing follower and creates no automatic mansion request. Fresh entity initialization discards
scene actions/lookups and rebuilds selected followers around the returned coordinates, then Bowie
at physical 0 with the returned facing. With only F65/F67/F68, Caravan/A, Peter and Oddler/B receive
new eas_Follower1, eas_OverworldFollower2 and eas_OverworldFollower3 templates. These are not the three
scene follow copies. Other flags change allocation/template order.

There is no fixed Peter fallback in this ordinary population. F67 absent removes that scene fallback
even if Peter is joined/active; speaking Oddler's name does not set F68. Class/HP still affect ally
sprites, and CURRENT_BATTLE=255 now permits the zero-HP blue-flame branch. Joined/active and follower
selectors, F421/F471/F521 are outside the temporary reset, but surviving RAM is not save persistence.

BattleLoop set PLAYER_TYPE=0, and this coherent scene/return does not change it; ordinary control uses
eas_ControlledCharacter. Victory/warp do not change F64 or RAFT_MAP/X/Y. With F64 set and saved map67,
the separate raft tail can declare physical 31 at saved coordinates masked by `$7F`; it does not board,
add the battle origin or repair position. Independently supplied mode2 instead uses the existing
raft-main/standing-follower tail. The controller selector uses mode0 walking, mode1 eas_Raft (actual
Caravan), and other-mode eas_Caravan (actual raft); arbitrary mode is not validated.

| Supplied change or unfinished work | Return boundary |
| --- | --- |
| F521 set | Entire scene/warp skipped; active-member seed applies absent replacement. Common join/completion still run. |
| Changed membership, position, facing lookup or saved geometry | Seed changes; a fully completed first scene overrides coordinates/facing, repeat does not. No empty/stale-list repair. |
| Missing/replaced A/B/Peter selector | Aliases or different identities apply in scene; fresh return removes temporary follows and scene Peter fallback. |
| Dead/mismapped actors or stalled service | HP/crop/action/wait rules remain operative; earlier writes need not reach completion or control. |
| Request replacement before battle result | Latest parameter bytes determine destination, not an immutable intended itinerary. |
| New event after exploration's entry reset | Pending takes precedence over controller installation; the rts init itself generates none. |

A supplied map255 result takes exploration's existing-population branch, without the fresh battle
assignment/entity setup/temporary reset; it updates main properties/raft and loads/init through that
branch. It is not the normal Map67 victory return. Other replacement destinations need their own
consumers and are not completed by this guide.

The next independent boundary is the Map67 doorway `(26,22)`, warpNoScroll, requesting Creed Mansion
Map33 `(15,30)`, Up. The after scene's full-map arrival is `(26,23)`, Down. Adjacency does not itself
dispatch that doorway, prove passability/input or enter the mansion. Stop before its setup/init.

## Defeat, Escape and Suspension Take Separate Continuations

**Confirmed:** ordinary defeat updates unlock, submits exhaustion music/text, restores Bowie's HP to
maximum, divides gold unsigned by two, selects Egress destination and returns d4=-1. Battle04's
special losable case is not selected. Admitted Egress/Angel Wing instead deducts spell MP/removes the
selected item slot, submits text/flash/closure, updates unlock, unwinds the individual-action frame,
selects Egress destination and returns d4=0. Action type/masked ID admission proves neither menu
availability nor MP/item custody. Escape has no victory scene/join/completion or defeat gold penalty.

For both paths UpdateBattleUnlockedFlag preserves F421 when F521 is clear and clears F421 when it is
set. Neither path sets F521 or changes F471. Battle21 has no special Egress destination: supplied
EGRESS_MAP and F399-set savepoint selection determine it. A missing row retains that map with default
`(1,1)`, Up. A direct F399-clear helper invocation instead uses initial Map3 `(56,3)`, Down; it is not
the ordinary battle entry, which sets F399.

F64 clear leaves raft coordinates unchanged. F64 set selects the complete reset table's Hassan9→
Map69 `(28,55)`, Polca13/Bedoe10→Map72 `(43,48)`, otherwise Map69 `(42,15)`; these writes do not board.
The alternate MainLoop return skips battle admission and SwitchMap can map 3→4 under F640, 71→78
under F530 or 58→59 under F999, preserving coordinates/facing. No claim selects a particular last
savepoint or Devil's Tail return. Stop at the supplied destination's continuation.

CURRENT_BATTLE=21 also admits the battlefield suspension prompt. A negative result returns to the
menu; acceptance saves seconds, sets F88, calls SaveGame for the current slot, then fades/jumps to
WitchSuspend. Debug plus P1 Start instead clears F88 and returns to the menu after the save call.
There is no victory result or F521 write. A later F88-set entry installs mode0/F399, restores seconds,
clears F88/AI memory and LoadBattle leads directly to individual turns, bypassing fresh map/battle
assignment, before/start scenes, healing and roster initialization. Supplied saved map/battle/area/
party must be coherent; this proves neither completed storage nor successful later load.

## Evidence Review and Remaining Questions

**Confirmed provenance:** this guide consumes research accepted in main
`f43add920e4c905195f35cb6009d870433153f19`, with
[SF2DISASM master `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm)
and USA retail ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
The [research outcome owner](../../research/battle21-outcomes-and-devils-tail-return.md#actual-outcome-routing)
and [reproduction boundary](../../research/battle21-outcomes-and-devils-tail-return.md#defeatescape-continuation-and-verification-limits)
own exact source paths, slots/addresses, complete selected commands/bodies/entities/text and consumers.
No raw-source reinvestigation, regenerated payload/comparison or runtime acquisition is claimed here.

| Reviewed evidence owner | Support and limits retained by this guide |
| --- | --- |
| Accepted outcome research and opening guide; predecessor Fairy Cave return | Exact leader/empty/after/join/region selection, both count orders, current-battle crop lifetime, all 33 scene commands/custom/two inline bodies/ce_4B948/seven literal texts, full-map return and supplied loss continuation. Accepted selected source joins establish this local content. |
| `sf2-battle-cutscenes-static-v1`, [fixture](../../../tests/fixtures/h2/battle-cutscenes-static-v1.json), expected.cutsceneFacts; [routing contract](../contracts/battle-cutscene-routing.md#after-battle-route-and-join-tail) | Shared HP/completed-flag gates, worklist append and join tail. Contract Unknowns for exact route targets/empty execution and join effect remain aggregate limits; accepted selected research supplies the narrower Battle21 and JoinForce(0) joins. |
| `sf2-battle-loop-static-v1`, [fixture](../../../tests/fixtures/h2/battle-loop-static-v1.json), expected.lifecycleFacts; [lifecycle contract](../contracts/battle-control-lifecycle.md#post-action-and-after-turn-order) | Slot counts, placement/HP counts, killed mutations and healing support shared mechanics. The accepted exact research supplies both-zero precedence and seed/outcome consumers; Battle01 observations prove no Battle21 runtime outcome. |
| `sf2-common-stats-static-v1`, [fixture](../../../tests/fixtures/h2/common-stats-static-v1.json), expected.statsFacts | Joined/active flag ranges and autoactivation context; accepted selected join/list consumers establish exact capacity/rebuild ordering. These flags do not select scene followers. |
| `sf2-map-init-static-v1`, [fixture](../../../tests/fixtures/h2/map-init-static-v1.json), expected dispatcher/entity/init facts | Shared setup/follower context. Accepted research supplies complete selected Map67 terminator/rts, fresh population, mode/raft/pending/controller and doorway joins; public aggregates alone select no natural itinerary. |
| `sf2-map-script-engine-static-v1`, [fixture](../../../tests/fixtures/h2/map-script-engine-static-v1.json), expected.programSummary and command-family facts | Shared interpreter/corpus context. The research's retained selection for abcs_battle21 has 33 commands; complete custom/inline bodies/entity table/literal text and immediate consumers remain separately owned joins. |

The existing fixture schema/provenance checks use
[cutscene schema](../../../schemas/h2-battle-cutscenes-static-fixture.schema.json),
[loop schema](../../../schemas/h2-battle-loop-static-fixture.schema.json),
[stats schema](../../../schemas/h2/common-stats-fixture.schema.json),
[map-init schema](../../../schemas/h2-map-init-static-fixture.schema.json) and
[script schema](../../../schemas/h2/map-script-engine-fixture.schema.json), through the existing
[JSON validator](../../../src/sf2tool/jsonio.py). Reviewed owning verifiers are
[battle_cutscenes.py](../../../src/sf2tool/h2/battle_cutscenes.py),
[battle_loop.py](../../../src/sf2tool/h2/battle_loop.py),
[stats.py](../../../src/sf2tool/h2/stats.py), [map_init.py](../../../src/sf2tool/h2/map_init.py) and
[map_script_engine.py](../../../src/sf2tool/h2/map_script_engine.py).
Their schema/source/provenance/semantic comparisons keep their own boundaries; shape validation is
not reproduction of every prose claim. The accepted research retains the unchanged 18,285,979-byte
map-init/script pair comparison and selected direct-source review, rather than this guide rerunning it.

Preserve [#888's original handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/888#issuecomment-6173000296),
[rebase handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/888#issuecomment-6173114839)
and [PR890 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/890#issuecomment-6173127050),
including original/final identities/CI, 0.102-second retained comparison, resolved inspection failures
and predecessor receipts. Preserve [#887's original handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/887#issuecomment-6172913854),
[corrected handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/887#issuecomment-6173044344)
and [PR889 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/889#issuecomment-6173054505):
its accepted lifecycle summaries order before → reset/heal/ally then enemy initialization/AI clear/load
→ start. The reported stale-summary P2 is resolved; original scope-only CI and later research-public
CI retain their distinct results. Handoffs locate checks; tracked research and accepted Git objects
own original-game facts. This guide's direct checks, committed planner and final CI belong to #891.

**Unknown:** natural victory/defeat/escape/suspension and route reach; supplied party/HP/classes/flags,
actor lookups/geometry/mode/raft/savepoint; completed action/text/input/fade/scroll/window services;
actual scene/portrait/sound, usable control and storage persistence. Changed-state examples constrain
static claims; they are not executed scenarios, automatic runtime work or evidence of later lore.
This guide completes neither the campaign nor the design epic and stops before mansion initialization.

Continue with [Creed Mansion entry and the first Goliath obstruction](creed-mansion-entry-and-first-goliath-scene.md)
for the separate doorway admission and zone scene; this guide retains its stop before mansion initialization.
