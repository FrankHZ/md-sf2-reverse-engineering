# Southwest Parmecia through Taros / Battle18 entry

Evidence date: 2026-10-13; refresh/neutral recheck: 2026-10-23. **Confirmed:** the western Map69 transfer scrolls
into Map68 in place; the shrine request instead returns through MainLoop's
battle scan. F418 admits Battle18 before Map12 exploration init can test F725.
Without admission, F725 clear runs the warning and requests a retreat; F725 set
skips it. Fresh Battle18 with F468 clear stages Taros and the Caravan before
rebuilding combatants and reaching the start handoff. **Inferred:** these scenes
present the shrine as an obstacle to obtaining the Caravan. **Unknown:** natural
travel, actual supplied state, completed services, usable input and persistence.

## Agreement and provenance

Standard rigor under [Issue #840](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/840)
and [the content epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
Accepted starting base is `0469862f50230b5c10eb8df0792d38e8ddcd18b6`.
This sole research document owns the selected Map69/68 transfer, conditional
Map12 warning/init/retreat and complete Battle18 before/start graph. There are
no shared-path writes or stacked dependencies. Stop at conditional exploration
return, unrelated battle admission, or Battle18's pre-activation start handoff.
Turns, damage, outcomes, Caravan acquisition/operation, Rick, later travel,
terrain decoding and a mandatory campaign route are excluded.

Accepted dependencies are [Hassan departure/Battle17](./hassan-departure-and-battle17-outcomes.md),
[Rohde's choice](./hassan-ancestry-and-rohde-caravan-choice.md),
[map requests](../../world/map-event-request-consumption.md),
[battle lifecycle](../../battle/battle-loop.md), [cutscene routing](../../battle/battle-cutscenes.md),
[common scripting](../../world/common-scripting.md), [membership](../../party/common-stats.md) and
[Oddler/following](../eastern-parmecia/polca-oddler-choice-and-following.md).
Source paths below are relative to `disasm/` in
[SF2DISASM master at `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at that pin. Retained map-init/map-script-engine
payloads bind it and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Fnnn means flag nnn; command/table indices are zero-based. Dialogue below is
paraphrased; coordinates, waits and action names are source operands.

The retained pair totals 18,285,979 bytes. Existing linear comparisons were
budgeted below 512 MiB and one minute and passed in 0.085 seconds. Additional
work consists of bounded selected source/fixture reads and small prose/receipts.
No payload regeneration, runtime, environment creation or new verification
machinery is required. Reassess before acquisition or material resource/scope
growth; no game-material network transfer or bulk publication is involved.

## Selected western transfer and shrine request

**Confirmed:** resolve `sf2enums.asm` first: South Parmecia69, Southwest
Parmecia68, Taros Shrine12, `BATTLE_VERSUS_TAROS=18`, `LEFT=2`, `UP=1`,
`DOWN=3`, `MAP_CURRENT=255`, Bowie0, Peter7, Rohde11 and FOLLOWER_B31.
Battle flag bases400/450/500 give F418/F468/F518. F725 belongs to Rohde's
acceptance; it is not another name for battle unlock or completion.

`data/maps/entries/map69/6-warp-events.asm` row5 matches X0, any Y and supplies
scroll Left, Map68, X62/Y255, facing Left. Earlier rows use X28/43/26/44/41,
so none wins at X0. At the corner `(0,0)`, row5 wins before the later any-X/Y0
row. `sf2mapmacros.asm:warpScroll` encodes `$10+LEFT=$12` in parameter1.
Map68's same-named file row0 matches `(55,43)` and supplies **no scroll**,
Map12 `(12,30)`, Up. These are selected table matches, not proof of traversable
terrain, an arriving mode or a continuous walk between them.

`code/gameflow/exploration/exploration.asm:WarpIfSetAtPoint` divides supplied
positions by384, scans the current map's rows in order and treats negative
coordinate bytes as wildcards. The first match writes type1, the four bytes
parameter1–4, facing plus its padding word at parameter5, and `SFX_WARP`.
It tests no sword, Battle17 completion, F725, F418 or force membership.
The immediate walking and vehicle callers in `entityscriptengine_2.asm` reach
it from their `$1000` layout branch after their own movement checks. Mode0
uses opcode2; actual Caravan mode1 uses `eas_Raft`/opcode7 and actual raft
mode2 uses `eas_Caravan`/opcode8, as resolved by
`entityfunctions_2.asm:SetControlledEntityActScript`. The upstream script
names alone do not identify the vehicle. This slice does not establish access
by any of these modes.

### Scrolling is an in-place consumer

**Confirmed:** `explorationfunctions_2.asm:ProcessMapEvent` clears pending
before dispatch. Nonzero parameter1 selects `ProcessMapEventType1_Warp`'s
scroll branch. It idles main0, writes CURRENT_MAP=68 and calls
`mapload.asm:ProcessMapTransition`; the special Pacalon substitution cannot
match68. That function loads selected tiles and blocks/layout and calls
`ApplyOverworldMapTransition`. Masking parameter1 with the direction mask
selects `overworldMapTransition_Left`, with its plane/sprite DMA and VInt loop.
Its `sub_29A2` adjusts VDP sprite positions, not the entity-data coordinates.

After that service, the consumer sets physical0's current/destination X to
62*384. Negative Y255 leaves its entity Y unchanged. It samples physical0's
position/facing and calls `DeclareRaftEntity`, then returns **inside the existing
ExplorationLoop**. It does not use requested parameter5 to assign facing on
this branch; the actor's supplied/controller-facing state remains relevant.
There is no MainLoop return/CheckBattle, fresh setup population, Map68 init,
healing or temporary-flag clear at this handoff. CURRENT_BATTLE is not rewritten
here. A supplied F419 does not by itself trigger Battle19 during this scroll.
Later event production is a separate boundary.

PLAYER_TYPE and saved RAFT_MAP/X/Y are not changed. `DeclareRaftEntity` returns
immediately for mode2; for normal mode it collapses listed followers' current
and destination coordinates to the sampled player position; mode1 skips that
follower loop. F64 and a matching saved raft map select separate physical31,
lookup63, at saved coordinates; mismatch follows its removal branch. This is
conditional placement, not a saved raft move to Map68 or a new follower list.
F64 clear skips placement/removal after the applicable follower loop.
After return, absent another pending event, WaitForEvent follows main0 and
installs the current mode's controller. Pending writes and service completion
remain supplied, not an atomic transition or an observed input handoff.

### No-scroll request and admission before init

**Confirmed:** Map68 row0 writes parameter1=0, so the same consumer submits
and clears WARP_SFX, idles main0, removes the exploration return frame, samples
physical0's position, substitutes nonnegative requested coordinates and takes
requested facing. It returns `(12,12,30,Up,d4=0)` to `MainLoop/@Start` without
normalizing transport or the saved raft tuple.

MainLoop calls SwitchMap, then CheckBattle, then chooses BattleLoop or
ExplorationLoop. The complete `data/maps/global/flagswitchedmaps.asm` has no
source12/68/69 row. `data/battles/global/battlemapcoords.asm` row18 is the only
Map12 row: area `(0,0)`, size26x32, trigger `(255,255)`. `getbattle.asm:CheckBattle`
requires F418 and matching trigger bytes. The area rectangle is returned
geometry, not a player-containment test; entry Y30 is not the battle predicate.
On selection it writes the area globals. F518 set clears F418 **but still
returns battle18 for that call**. F468, F725, F517, sword possession, Rohde
following and membership are not admission checks.

| Supplied state at the Map12 scan | Selected boundary |
| --- | --- |
| F418 set, F725 either | Battle18; no Map12 exploration init or warning first. F518 can clear unlock after selection without canceling entry. |
| F418 clear, F725 clear | No battle; Map12 init runs the warning, then checks F518. |
| F418 clear, F725 set | No battle; init skips warning, then checks F518. Conditional exploration return. |

Rohde's accepted Yes path writes F725/F84/F418 together, but neither this consumer
nor these scripts repairs independently supplied or inconsistent flags. Battle17
completion and sword custody therefore cannot replace the actual admission test.

## Map12 population, complete warning and retreat

**Confirmed:** supplied-map ExplorationLoop clears pending, reduces STEP_COUNTER
by20000 with its signed-word negative clamp, heals living/immortal allies, fades
out and sets CURRENT_MAP=12/CURRENT_BATTLE=255. It loads tiles, rebuilds entities
from the selected setup, clears temporary flags256–383, sets F80, loads map,
installs base VInts and invokes RunMapSetupInitFunction. Map12 has one setup
in `data/maps/mapsetups.asm`; its pointer table selects `ms_map12_InitFunction`.
The one fixed row in `map12/mapsetups/s1_entities.asm` supplies Taros at `(12,8)`,
Down, special mapsprite255 and `eas_Init`.

`mapfunctions.asm:InitializeMapEntities` clears entity data and lookups, declares
flag-selected followers, processes fixed rows, then separately creates main0
from the requested position/facing. Taros's special-sprite row writes logical128's
lookup to physical47 (`ENTITY_SPECIAL_SPRITE=$2F`) and does not consume the next
ordinary physical slot. This is a scene actor, not enemy combatant128's stats.
Map12 has no fixed Peter, Rohde or FOLLOWER_B row and no Caravan row.

Map12 uses the normal follower table even with F65 set. In
`data/scripting/entity/followers.asm`, F67 supplies Peter and F84 supplies Rohde;
F68 supplies Oddler as FOLLOWER_B, while other supplied flags74/70 can also
write that logical identity to Astral rows. Table order, not the scene's story
meaning, determines the final lookup when flags conflict. F725 does not populate
these actors. The main entry is not a membership join or an active-party list.
In mode2 the population tail makes main0 a raft and hides followers; in other
modes F64 plus a matching RAFT_MAP can place the separate raft. Neither warp
nor this init acquires/normalizes a vehicle.

`map12/mapsetups/s6_initfunction.asm` at `0x57D04` has exactly this order:
check F725; if clear run `cs_57D22`; **after script return** check F518; if set
move logical128 out of map; return. With F725 set this init creates no pending
retreat; after the usual conditional music/fade, absent another event, WaitForEvent
follows main0 and installs the current mode's controller. F518 does not suppress
the warning. Thus
F725-clear/F518-set exploration still requests the warning and retreat before
moving Taros off-map. F725-set/F518-clear can leave Taros present without a
warning or battle. No selected init write sets F725, F418, F468 or F518.

`MoveEntityOutOfMap` resolves the current lookup via
`GetEntityIndexForCombatant` and calls `SetEntityPosition` with raw X/Y `$7000`.
It changes current and destination coordinates; it does not clear the lookup,
remove membership, set HP or report Taros defeated.

### Warning program `cs_57D22`

**Confirmed:** this is the complete 51-command program at `0x57D22`, using cursor
3197. In the table, “start” means install without waiting; “wait” means wait for
that selected actor's `eas_Idle` pointer, subject to the shared limits below.
All inline movement bodies are included; each ends with `endActions`.

| Commands | Ordered work |
| --- | --- |
| 0–8 | Cursor3197; camera speed `$30`; wait `eas_Init` for Bowie, Peter, FOLLOWER_B; set positions `(12,31)`, `(13,31)`, `(11,31)`, all Up; `fadeInB`. |
| 9–13 | Start Bowie Up5; start Peter Up5; wait FOLLOWER_B Up5; Taros text3197; csWait10. |
| 14–20 | Start Bowie jump, start Peter jump, wait FOLLOWER_B jump; repeat that three-actor sequence once; csWait30. |
| 21–27 | csWait5; start Bowie Right/Left twice; csWait5; start Peter Up/Down twice; csWait5; start FOLLOWER_B Right/Left twice; csWait80. |
| 28–35 | Wait Peter Up1; Bowie and FOLLOWER_B Up; Peter3198; camera destination `(7,5)`; Peter3199; csWait30; Taros3200. |
| 36–42 | Camera destination `(7,21)`; wait Peter Down1; Bowie and FOLLOWER_B Right, Peter Left; Peter3201 via `nextText`, then3202 via `nextSingleText`. |
| 43–50 | csWait20; nod Bowie; Peter3203; start Bowie Down5, start Peter Down5, wait FOLLOWER_B Down5; request Map68 `(55,44)`, Down; end. |

| Text ID / hexadecimal key | Attributed meaning and control |
| --- | --- |
| 3197 / `0C7D` | Taros orders a stop; modifier `$0`, W1. |
| 3198 / `0C7E` | Peter asks who spoke; `$80`, W1. |
| 3199 / `0C7F` | Peter remarks on the figure's size; `$80`, W1. |
| 3200 / `0C80` | Taros orders them away and threatens an attack; `$0`, N/W1. |
| 3201 / `0C81` | Peter reacts to the threat and urges the named leader to leave; `$80`, LEADER/N/W2. This is the sole continuing `nextText`. |
| 3202 / `0C82` | Peter says they do not want the fight; `$80`, W1. |
| 3203 / `0C83` | Peter urges haste; `$80`, W1. |

There is no Yes/No prompt, Rohde speech, inventory test or flag write in this
program. A currently supplied Rohde follower can still exist; absence from this
script is not deletion or loss of membership.

### Pending retreat and actual return

**Confirmed:** `mapscriptengine_2.asm:csc07_warp` writes type1, no-scroll0 and all
four map/X/Y/facing bytes. It does not load Map68, exit the script immediately
or write WARP_SFX. `csc_end` returns through the ordinary script termination
service; init then performs the F518 movement-out check. RunMapSetupInitFunction
returns directly, with no all-actor or main-position barrier.

ExplorationLoop next compares the palette base/current longword and conditionally
requests map music/fade. Only then does it enter SetMoveSfx/WaitForEvent. With
the retreat still pending, WaitForEvent returns it without first reinstalling
ordinary control. ProcessMapEvent clears pending and the no-scroll consumer
returns `(68,55,44,Down,d4=0)` to MainLoop. It submits/clears whatever WARP_SFX
is then supplied, not a new scene-authored warp sound. There is no guaranteed
playable pause in the shrine between warning and retreat.

The new scan has only row19 for Map68: area `(5,19)`, size48x32, trigger255/255.
If F419 is supplied set, it admits19 (F519 may clear that unlock after selection);
this slice stops at that unrelated admission. If not, supplied-map68 exploration
rebuilds the population, clears temporary flags and runs its return-only init.
Its setup has no fixed entities and no alternate flag row. Followers come from
current flags, with F65 selecting the overworld table; the mode/F64/saved-raft
rules still apply. No F419, F65, F64 or raft tuple is written by the warning.
Absent another pending event, WaitForEvent then follows main0 and installs the
current controller. Natural return, completed services and usable input remain
Unknown. If pending state is independently replaced, this is not an atomic queue
that guarantees the original retreat survives.

## Battle18 before scene and start handoff

**Confirmed:** `battleloop_1.asm:BattleLoop` first writes PLAYER_TYPE=0 and F399,
then tests F88. Fresh entry clears seconds, writes map12/battle18 and installs
base VInts before calling the before wrapper. Slot18 in
`data/battles/cutscenes/beforebattlecutscenes.asm` selects `bbcs_18` at `0x4B1D8`;
slot18 in `battlestartcutscenes.asm` selects `ms_Empty` at `0x47A4E`.
`beforebattlecutscenesstart.asm` tests F468 without setting it; the start wrapper
skips when F468 set, otherwise sets it **before** executing the empty terminator.
F468-set fresh entry therefore skips both scripts but still rebuilds combatants.
F518 is not an intro guard; F725 is not tested by either wrapper or `bbcs_18`.

F88 set instead restores saved seconds, clears F88 and AI memory, clears the
view target, calls LoadBattle and branches to the individual-turn loop. It
bypasses incoming map/battle assignment, before/start, fresh healing/placement
and new turn-order generation; unlike fresh entry it does not explicitly clear
d0 before LoadBattle. The saved map/battle, area, roster and turn state must
already agree. This is a saved-entry bypass, not a repaired Taros opening. Stop
at that branch; the subsequent `@Start` branch in source is unreachable there.

### Scene population is distinct from the battle roster

**Confirmed:** `bbcs_18` command1 is `loadMapFadeIn Map12,7,22`. Its
`csc37`/`csc48` consumer starts a fade-out, waits for its completion, loads tiles
and the map with those camera operands, and restores display/VInt services.
Despite the macro name, the selected body supplies its visible fade separately
at command12. This map load does not invoke Map12's exploration init.

Command2's `csc42_loadMapEntities` reads the **three-word main header** of
`ce_4B2DA` before passing the remaining rows to InitializeMapEntities. The header
is main0 `(12,32)`, Up; it is not a third NPC row. The following two rows are
Taros `(12,8)`, Down, special sprite255, and Caravan `(12,5)`, Down, sprite62,
both with `eas_Init`, then the entity-list terminator. Taros again maps logical128
to physical47. Caravan consumes the next ordinary physical slot and lookup129;
its physical index depends on the preceding flag-selected followers. It is not
logical FOLLOWER_A, a combatant, or the transport's controller.

This clears old exploration actors and rebuilds followers from Map12's normal
table. Peter/FOLLOWER_B/Rohde are **not** supplied by `ce_4B2DA`; their current
flags/lookups remain required as above. F84-clear with F418-set does not synthesize
Rohde. Neither staged Caravan nor Rohde's speech sets F65, changes transport,
grants inventory, joins the force or proves vehicle operation. No command in
this scene checks/consumes the Achilles Sword or establishes its damage effect.

### Complete program `bbcs_18`

**Confirmed:** 51 commands, cursor2520. Every inline action body ends after its
single move; no hidden second scene or start text is inferred from filenames.

| Commands | Ordered work |
| --- | --- |
| 0–13 | Cursor2520; load Map12/camera `(7,22)`; load `ce_4B2DA`; wait `eas_Init` for Bowie, Peter, FOLLOWER_B, Rohde in that order; position Peter `(13,33)`, FOLLOWER_B `(12,33)`, Rohde `(11,33)`, all Up; stop animation of129; request shrine music; `fadeInB`; camera speed `$40`. |
| 14–19 | Start Bowie Up6, Peter Up6, Rohde Up6; wait FOLLOWER_B Up6; Taros2520; csWait5. |
| 20–27 | Start Bowie Right/Left twice; csWait5; start Peter Right/Left twice; csWait5; start Rohde Up/Down twice; csWait5; start FOLLOWER_B Right/Left twice; csWait30. |
| 28–38 | Wait Peter Up1; Peter2521; wait Rohde Up1; Rohde2522; Bowie/Peter/FOLLOWER_B Up; csWait30; camera follows128; Rohde2523; Taros2524. |
| 39–45 | Camera destination `(7,22)`; Bowie Right, Peter Left, Rohde Right; Peter2525; Bowie Left; Rohde2526. |
| 46–50 | Bowie/Peter/Rohde Up; Taros2527; end. No scene warp or story-flag write. |

| Text ID / hexadecimal key | Attributed meaning; all use `nextSingleText` and W1 |
| --- | --- |
| 2520 / `09D8` | Taros orders a stop; modifier `$0`. |
| 2521 / `09D9` | Peter asks who the large figure is; `$C0`. |
| 2522 / `09DA` | Rohde asks whether Peter recognizes him; `$0`. |
| 2523 / `09DB` | Rohde identifies Taros as the ancient shrine's guardian; `$0`, N. |
| 2524 / `09DC` | Taros repeats the warning and threatens attack; `$0`, N. |
| 2525 / `09DD` | Peter asks what they should do; `$C0`. |
| 2526 / `09DE` | Rohde points to the Caravan behind Taros and insists on obtaining it; `$0`, N. |
| 2527 / `09DF` | Taros announces that time is up and battle begins; `$0`. |

The dialogue establishes what the speakers say. It does not prove Taros's
history, a required party composition, the Caravan's usability, a completed
camera view or that battle services have already finished.

## Shared command, actor and service limits

**Confirmed:** selected commands use `mapscriptengine_1.asm` and
`mapscriptengine_2.asm`; text IDs above join directly to
`data/scripting/text/gamescript.txt`. `textCursor` writes the cursor;
`nextSingleText` resolves portrait/speech from the mapped actor's mapsprite,
waits for scrolling, displays the cursor text, advances it, closes portrait/text
and sleeps10. `nextText` advances without that close/sleep tail. Modifier `$80`
selects the right portrait and `$C0` also mirrors it; `$0` selects neither bit.
The portrait-open guard can retain an existing window. N, LEADER, W1 and W2
remain text-engine controls/substitution, not extra scene commands or a new
choice prompt. Rendered text, acknowledgement and speaker/audio services are
not observed here. The mapped sprite table `data/spritedialogproperties.asm`
selects Taros's portrait and special dialogue bleep, Peter's base/promoted
portrait/bleep variant, and Rohde's portrait/bleep for his special mapsprite.
A missing/wrong mapping cannot be repaired by the speaker operand.

`textfunctions_1.asm:symbol_leader` calls UpdateForce, reads the first current
BATTLE_PARTY_MEMBERS byte and gets that combatant's name; LEADER is not hardcoded
to Bowie and does not validate an empty/inconsistent list. Its `@wait2` and
`symbol_wait1` temporarily clear CURRENTLY_TYPEWRITING and poll directional/A/B/C
input through CURRENT_PLAYER_INPUT with VInt waits. Each loop also updates the
RNG and RANDOM_SEED_COPY; W2 additionally services the arrow and submits validation
SFX on acceptance. Typewriting0 alone is not a control-ready signal. The existing
[text-service evidence](../grans-opening/map3-messenger-acceptance.md) remains scoped to its own
observations; none is promoted to a Taros acknowledgement here.

ExecuteMapScript clears its skip byte on entry, but debug mode plus player2
Start can set it while reading commands. These text handlers then skip the
packed argument without display or cursor increment; csWait also skips Sleep.
Ordinary csWait passes its low-byte operand to Sleep, which waits that many
VInts. These counts are not measured wall time or universal scene durations.
`playSound` submits a command; it does not wait for the entire music track.
`fadeInB` selects the half-in-from-black operation over all palettes with factor6;
LaunchFading polls completion with VInts. Fixed camera destination disables
entity tracking, sets the destination and waits for scrolling. Setting camera
entity128 writes its mapped physical target; following text provides its own
scroll wait. End conditionally waits for scrolling when the dialogue-window
index is nonzero, clears camera speed and restores registers. None of these
services is an all-actor motion join.

`GetEntityAddressFromCharacter` resolves the current logical lookup without a
presence test. ClearEntities initializes lookup bytes to0, so an absent Peter,
Rohde or FOLLOWER_B can alias physical0; an invalidated255 is a different invalid
state. Conflicting follower flags can overwrite a logical mapping and excessive
supplied followers do not gain a new safe list/capacity contract from these scenes.
The scripts do not validate membership or repair these states.

`setPos` and `setFacing` test the actual packed ally byte and skip their work
for a zero-HP ally below30; FOLLOWER_B31 and Taros128 bypass that ally guard.
Action installation (`csc15`, `csc2D`) and nod have no equivalent HP/presence
guard. Thus actor existence, health and completed placement are separate facts.
Exploration heals before its warning, whereas fresh BattleLoop heals **after**
the before scene; a dead ally supplied to that scene is not healed first merely
because later battle initialization heals it. The Caravan stop-animation command
writes its mapped animation counter to-1; it is not a general AI/vehicle stop.

`eas_Init` sets acceleration factors0, speed32, acceleration/deceleration off,
obstruction/collision settings, auto-facing on, orientation Up, immersion off
and size24, refreshes the sprite and jumps idle. `eas_Jump` disables auto-facing,
requests relative `(0,-1)`, waits5, requests `(0,1)`, waits5, requests `(0,0)`,
restores auto-facing and idles. The two look actions request Right/Left/Right/Left
or Up/Down/Up/Down, updating the sprite and waiting30 after **each** facing,
then idle. They are not instant completed turns.

Inline Up/Down actions compile relative opcode4 movement followed by opcode1
destination-equality waiting, then jump to `eas_Idle`. `esc04_moveToRelativeDest`
scales by384 from the actor's current position, checks destination conflicts and writes destination/travel and
signed velocity from current speed. `esc01_waitUntilDestination` advances only
when both current/destination coordinates match. `entityActionsWait` and
`setActscriptWait` poll **only that actor's** action pointer for `eas_Idle`. Nonwaited actions can overlap text/other actors;
later action writes can replace unfinished work. The warning's waited FOLLOWER_B
jumps do not independently join Bowie's/Peter's jumps, nor do its final moves
establish every actor's final position. The nod stops animation, sleeps10,
loads/transforms/DMAs the sprite, sleeps20, refreshes, sleeps10 and restores
animation; it neither asks for consent nor checks HP. Collision, VInts, DMA,
text, fade, lookup and action progress remain conditional service dependencies.

## Fresh combat population and stopping boundary

**Confirmed:** after the before wrapper returns, BattleLoop restores incoming
map12/battle18, clears region flags90–105, heals living allies plus immortal
Peter/Lemon, initializes ally/enemy positions, clears AI memory, calls LoadBattle
with d0=0 and invokes the start wrapper. Healing restores HP/MP maxima for those
eligible. Its intermediate status mask retains STUN/POISON/CURSE; the subsequent
UpdateCombatantStats clears CURSE and reconstructs it from actually equipped
cursed gear for an ally. The intermediate mask is not the final status promise.
It does not revive other dead allies or grant membership. The complete
[Battle18 initialization/first-control account](./battle18-initialization-and-first-control.md)
continues beyond this entry owner's stopping boundary.

Slot18 of `data/battles/spritesets/entries.asm` selects `BattleSpriteset18`
at `0x1B4546`: 12 ally placements, 14 enemies, five AI regions, zero AI points.
The first placement is `(12,27)` with area origin `(0,0)`. This is distinct from
the scene main header `(12,32)` and shrine request `(12,30)`.
`initializecombatants.asm:InitializeAllAlliesBattlePositions` clears all30 ally
X values, calls UpdateForce and consumes the current BATTLE_PARTY_MEMBERS list.
Dead members consume list/loop slots without advancing the placement row;
remaining members beyond capacity can leave the active party. Twelve rows do
not force twelve living allies, Peter, Rohde or Oddler into combat. UpdateForce
builds the list from joined/active flags; the placement routine reads its first
entry before a zero-member exit check. An empty/inconsistent supplied list is
not guaranteed safe empty initialization or repaired by the scene.

All14 selected enemy rows are STARTING, supplying slots128–141. Row0 is
TAROS at `(12,8)`; the rest of the32-slot initialization follows its hidden/cleared
path. These are combat initialization inputs, separate from the two scene rows.
Battle18 is absent from `data/battles/global/randombattles.asm`, so the shared
random-battle upgrade admission does not become enabled merely by supplied
F518. AI data and Taros's enemy identity do not establish activation, turns,
sword effectiveness, damage or a winning trajectory.

`battlefunctions/loadBattle.asm:LoadBattle` fades out, loads tiles, clears VInts
and calls `entityfunctions_1.asm:PositionBattleEntities`. That routine clears
entities/lookups and declares the placed combatants, excluding negative
coordinates and neutral enemies. It does not preserve the staged Caravan or
exploration follower list as combat membership. LoadBattle then selects d0's
mapped view target, initializes sprites, loads map/mapsprites, installs base
VInts, loads terrain, requests map music and fade-in. The start wrapper sets
F468 and executes the empty terminator when not already set. Return reaches
`@Start` **before** ActivateEnemies, region/spawn processing or turn generation.
That is this fresh-entry stopping condition, not the first observed playable turn.

The rebuild also has a distinct completion-flag-gated neutral tail. With F518
clear, `data/battles/global/battleneutralentities.asm`'s `VERSUS_TAROS` record
adds Caravan at `(12,5)`, **Left**, with `eas_Motionless`. It starts with logical
combatant159, zeroes max/current HP and status, ORs the neutral bit into activation,
writes its combatant X/Y and declares a physical actor from that row. The lookup
write is a separate predecrement from ENEMIES ($FFB160), landing at $FFB15F,
lookup index31. GetEntityIndexForCombatant(159) reads index63 at $FFB17F, which
remains $FF in the fresh rebuild; DeclareNewEntity does not repair this mapping.
F518 set skips this neutral tail, even if F468 clear allowed the before-scene
Caravan to appear. The old scene lookup129 is not retained: battle lookup129
now belongs to its enemy row. Combatant159 and the new physical Caravan do not
establish a working logical159 lookup. This is a fresh
neutral declaration, not vehicle acquisition, party membership or a carry-over
of the scene's Down-facing animation-stop command. Taros likewise gets a rebuilt
special-sprite actor from its combatant row; repeated coordinates do not preserve
the old scene's state. `data/scripting/entity/eas_battleneutralentities.asm`
defines `eas_Motionless` as motion-off followed by a branch to idle; this is
separate from the scene's animation-counter write.

## Reproduction, acceptance and deferred evidence

Load `. ./local/private-inputs.ps1` in the same process as controlled commands.
Verify the read-only source pin/clean status and use the existing
[retained comparison recipe](../grans-opening/battle06-to-battle07-content-route.md#reproduction-and-review-boundary):
map-init payload equality, `_verify_map_script_engine_fixture`, and source
`_cutscene_facts` against `battle-cutscenes-static-v1.json`. These passed without
rewriting either retained payload. No fresh ROM extraction or runtime is claimed.

Direct review selects the two complete 51-command programs from retained
`programCorpus.programs`, joins every command/source line and inline body,
and reads both text ranges. Resolve enum → warp row → producer/consumer →
MainLoop/CheckBattle before selecting Map12 setup or slot18. Read the complete
init, before/start wrappers, scene main header/follower/fixed-row builder,
selected command/action consumers and fresh/saved roster/LoadBattle handoffs.
Jump-interface aliases resolve to the named implementations, including
s05 BattleLoop/entity lookup/position, s07 scene/population wrappers and s13
combatant initialization.

| Existing fixture under `tests/fixtures/h2/` | Reuse boundary |
| --- | --- |
| `map-init-static-v1.json`, `map-script-engine-static-v1.json` | Retained comparisons plus direct selected-source joins; no new composed-route executable coverage. |
| `battle-cutscenes-static-v1.json`, `battle-routing-data-static-v1.json` | Source wrapper comparison and direct slot18/Map12/Map68 joins; pointer identity is not inferred from a filename. |
| `map-event-request-consumption-static-v1.json` | Shared pending/warp consumer evidence; actual scroll/no-scroll call order is reviewed directly here. |
| `battle-loop-static-v1.json`, `battle-spriteset-data-static-v1.json`, `common-stats-static-v1.json` | Shared lifecycle/roster/membership boundary; no combat simulation or fixed-party claim. |

Discriminating supplied states are independently set/clear F725/F418/F468/F518,
F88 saved entry, F419 on retreat versus scrolling, F84/F67/F68 and conflicting
FOLLOWER_B flags, absent/invalid mappings, dead allies, different party lists,
mode0/1/2, F64 with matching/mismatched raft tuple, replaced pending requests,
and stalled text/fade/scroll/action services. These guide source/prose review;
they are not a newly executed runtime state matrix.

**Unknown:** natural western transfer and shrine admission, actual flag/HP/list/
lookup/transport/request state, completed actions and service ordering in a live
run, rendered camera/text/audio, usable control and save/resume persistence.
They remain deferred under [ADR0014](../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../../../decisions/0016-remake-start-evidence-deferral.md), without an
automatic acquisition queue. Scene actors/speech do not close operation,
membership or damage claims.

The [accepted PR838 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/838#issuecomment-6160960756)
retains its original/rebased completed CI and comparisons, inspection failures,
and linked #832/#828 predecessor histories. Failed guessed source/document
reads are not absence evidence. This Issue/PR handoff retains current corrected
inspections, exact Git identities, direct checks and actual final-head CI.
Acceptance requires complete owned-document/source/provenance/link/private
review, `git diff --check`, clean committed
`uv run sf2 verify plan --base origin/main --head HEAD`, and final-head CI.
No blanket local normal/full/engine/Godot/H3/H4 suite is selected solely for prose.
Freeze a clean pushed Draft PR for independent main-gate acceptance and merge.
