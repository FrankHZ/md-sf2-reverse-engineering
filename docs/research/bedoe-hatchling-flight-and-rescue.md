# Bedoe hatchling flight and rescue

Evidence date: 2026-10-13. **Confirmed:** Bedoe's F722-selected entity interaction
runs the hatchling's flight demonstration while F723 is clear, then sets F723;
repeat interaction requests text 1685. Independently, the selected `(6,23)` zone
runs the fall/rescue scene only while F13 and F724 are clear, then sets F724.
Neither F723 nor king acceptance F799 is a rescue prerequisite. **Inferred:** the
staging and attributed dialogue depict an attempted flight and a rescue.
**Unknown:** natural reach, supplied actors/state, completed asynchronous services,
visible flight/fall/contact and playable continuity. The rescue's future payoff
and Skreech recruitment are outside this evidence boundary.

## Agreement, provenance and resources

Standard rigor under [Issue #796](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/796)
and [the content epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514):
independent callers share an actor and asynchronous services, so admission,
current state and completion need separate boundaries. This is the sole tracked
owner, with no shared-path changes. Consume accepted main
`c03b5ef4b52967ce3e37cb29fc276d2eb0c416a7`, especially
[Bedoe entry](./battle12-outcomes-through-bedoe-entry.md),
[the shrine audience](./bedoe-to-volcanon-audience.md),
[the king choice and return](./volcanon-return-through-bedoe-king-choice.md),
[interaction state](./map-event-interaction-state.md),
[request consumption](./map-event-request-consumption.md) and
[common scripting](./common-scripting.md). No stacked dependency is used.

Scope is the two selected callers and complete `cs_5766C` / `cs_5779C` programs,
relevant actor/flag consumers, and their conditional control returns. Stop before
`cs_5797A`; exclude further king choice, Bedoe exit, Polca/raft, Battle13, later
recruitment, a campaign permutation inventory, layout/floor traversal, runtime
acquisition and new verification machinery. No scope expansion is needed.

Source paths below are relative to `disasm/` in
[pinned SF2DISASM `master` revision `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at that pin. The retained map-init/map-script-engine
pair binds that revision and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Existing payload/fixture and cutscene-source comparisons pass; this is retained
static evidence, not a new composed observation. Fnnn means flag nnn. Table rows
are zero-based; scene records below are one-based positions in
`programCorpus.programs[].commands`, including sleep and terminator records.

The selected `data/maps/entries/map10/mapsetups/scripts.asm` spans are exactly
415–496 (82 lines) and 497–614 (118 lines): **200 source lines**, ending before
`cs_5797A` at 615. Reuse the unchanged 18,285,979-byte pair in one process, linear
selected reads/comparisons, estimated below 512 MiB peak and one minute, with
small prose/receipts only. Existing comparisons took 0.081 seconds. All 126 scene
records match their source-line macro/operands. Inline bodies and named consumer
sections are reviewed separately; no full export, new parser, comparator,
per-scene fixture or capture is needed. Replan before materially expanding these
bounds. Unknowns do not authorize acquisition. Local acceptance is direct
source/fixture/document review plus the clean committed dependency planner;
actual final-head CI and independent main-gate review complete the handoff.

## Selection and actual actor identities

**Confirmed:** `data/maps/mapsetups.asm` gives Map10 the single F722 override;
`GetCurrentMapSetup` resolves current flags at each lookup. Alternate
`pointertable_722.asm` selects its entities, entity events, zone events and init.
F722-clear uses the default setup, whose entity table has no lookup 153 row and
whose zone table does not select this `(6,23)` rescue caller. F723/F724/F799 alone
cannot select the alternate setup. A flag write does not itself rebuild actors
or invoke an interaction.

`data/maps/entries/map10/mapsetups/s1_entities_722.asm` starts with ally Luke's fixed row, followed
by 26 regular NPC rows. In `mapfunctions.asm:InitializeMapEntities`, ally rows
use their ally mapping; regular rows advance the separate NPC mapping cursor
beginning at lookup 128. Therefore the last two rows are:

| Source row / regular ordinal | Lookup and initial declaration |
| --- | --- |
| 25 / 24 | 152, BIRDWOMAN, `(6,10)`, Right, `eas_Init`. |
| 26 / 25 | 153, CHICK, `(16,6)`, Left, `eas_Init`. |

These are lookup identities, not physical slots 152/153 or ally 20. Main/Bowie is
separately declared at physical 0. Followers are allocated before fixed entities;
Map10 is neither a no-follower map nor an overworld map. F67 supplies Peter 7 via
`data/scripting/entity/followers.asm`. No fixed Peter row exists. F67-clear fresh
construction leaves his cleared mapping zero, so an unguarded Peter request can
resolve physical 0. F75 may similarly supply Luke before his fixed row, which is
then skipped. This changes physical allocation without changing regular NPC
numbering. Other supplied follower flags can also change physical slots.

Follower declaration is not a force-membership, active-party, word-HP or AI test.
Neither selected scene joins anyone, changes party/AI membership, heals/revives,
or sets F67. F13 is the joined-Elric flag: `sf2enums.asm` defines `ALLY_ELRIC=13`,
and `code/common/stats/battleparty.asm:JoinForce` sets the ally-index flag. Here it is
only the rescue suppression discriminator; no later Elric lifecycle is audited.
Text keys 1688/1689 name `{NAME;20}`, whose ally constant is `ALLY_SKREECH=20`.
That attribution does not turn NPC153 into an ally 20 mapping or establish joining.

`GetEntityAddressFromCharacter` (`mapscriptengine_1.asm:1841–1860`) masks to a
byte, subtracts `$60` for NPC128+, reads `ENTITY_INDEX_LIST`, and scales the
physical index by 32. Zero entries resolve physical 0; `$FF` entries resolve
index 255 without a general presence guard. Scene commands neither create nor
repair these mappings. Conclusions below assume the stated valid, distinct
mappings where an actor outcome is described.

## Callers and admission boundaries

**Confirmed:** alternate `s2_entityevents_722.asm` row 25 encodes
`153, DOWN, Map10_2D2_EntityEvent25`. `DOWN=3` is consumed as two wrapper bits,
not a required player/chick facing. Entity fixture program 30 at `0x56F68`, source
122–137, has seven operations and one table reference. Alternate
`s3_zoneevents_722.asm` row 0 encodes `(6,23), Map10_2D2_ZoneEvent0`; zone fixture
program 5 at `0x56FC8`, source 11–24, likewise has seven operations and one reference.

| Caller and supplied state | Complete branch effect |
| --- | --- |
| Entity, F723 clear | `chkFlg 723; bne repeat` falls through to `script cs_5766C`; after return, `setFlg 723; bra return; rts`. |
| Entity, F723 set | Branch to `byte_56F7A`, request text 1685, return. No scene, actor reposition or new flag write. |
| Zone, F13 set | First `chkFlg 13; bne return_56FDE` skips all remaining work. |
| Zone, F13 clear / F724 set | `chkFlg 724; bne return_56FDE` skips the script/write. |
| Zone, both clear | `script cs_5779C; setFlg 724; rts`. Neither F723 nor F799 is read. |

There is no compulsory demonstration-before-rescue order in these predicates,
and F724 does not suppress the demonstration. Neither script writes its caller's
completion flag internally. A supplied set flag is not proof of scene history;
partial services can change actors before the flag write is reached.

The ordinary entity route is the accepted `WaitForEvent` / `ProcessPlayerAction`
route: main is idled and waited at destination and scrolling is waited; ordinary
C proceeds to activation after menu/debug/caravan alternatives. `GetActivatedEntity`
uses the view target/facing and addressed tile, extends over counter tiles,
excludes the view target/listed followers and returns the first nearby entity.
`GetEntityEventIndex` reverse-scans mappings in order, with the ally 31→NPC128 jump
and lookup 0 fallback. The successful case must actually reverse-map to 153; a
nickname, screen location or aliased mapping is insufficient.

`RunMapSetupEntityEvent` (`mapsetupsfunctions_1.asm:162–247`) stores player facing
in `EVENT_RELATIVE_POSITION`, selects speaker properties from the current mapped
sprite, and matches the lookup only. CHICK selects portrait NONE / bleep 1 in
`data/spritedialogproperties.asm`; BIRDWOMAN selects NONE / bleep 6 for her scene
texts. Bit 0 waits one VInt and faces the actor opposite the player. Bit 1 later
restores the saved **physical actor's pre-interaction facing**, including on the
repeat branch. Entity updates are deactivated before the caller; the `script`
trap 6 reactivates them before `ExecuteMapScript`. Thus a script's waited actions
can progress, subject to services, whereas repeat does not enter the script trap.

The zone producer is `entityscriptengine_2.asm:loc_5294`: a selected layout
attribute `$1400` writes event type 6 and tile-coordinate words computed from
destination plus attempted offsets, divided by `MAP_TILE_SIZE=384`. Merely having
a row at `(6,23)` does not prove the required layout attribute or natural crossing.
`WaitForEvent` polls pending; `ProcessMapEvent` clears type before dispatch.
`ProcessMapEventType6_ZoneEvent` installs `eas_Init` on physical 0, loads the
request X/Y words and calls `RunMapSetupZoneEvent`. The latter scans four-byte
rows in order with `-1` wildcards/default `$FD`; this selected row requires both
coordinates. It has no entity-facing bit protocol. After the caller it closes
portrait/text, waits a VInt and waits for physical 0's current/destination equality,
including after a suppressed branch. No floor or natural route is established.

## Complete flight demonstration

**Confirmed:** `cs_5766C` at `0x5766C` has **48 records: 44 opcode commands,
three sleeps and one terminator**. Cursor 1680 yields six texts, keys `0690..0695`:
two `nextText` and four `nextSingleText`, all speaker 153. The caller repeat reuses
1685; there are seven request sites but six distinct flight texts. The following
accounts for every record and inline body. A waited action means that specific
actor's action pointer reaches `eas_Idle`, not that all services/actors finish.

| Records | Ordered requests and inline actions |
| --- | --- |
| 1–4 | Cursor 1680; camera tracks 153; chick describes a dream of flying (1680) and announces a demonstration (1681). |
| 5–8 | Face 153 Right; sleep 40; nod 153; wait `jumpRight 1`. |
| 9–15 | Sprite FLYING_CHICK; wait auto-facing OFF; custom wait speed `(4,4)` then jump idle; wait Up 1; custom wait speed `(48,48)` then idle; falling SFX; wait Down 6. |
| 16–23 | Sprite CHICK; puzzled text 1682; wait `eas_Init`; wait Left 2, faceUp 1; Bowie/Peter Down; chick acknowledges failure 1683 and asks them to stay 1684. |
| 24–31 | Face 153 Right; sleep 40; nod; camera speed `$40`; wait Right 1, jumpRight 1; sprite FLYING_CHICK; wait auto-facing OFF; wait Down 1. |
| 32–36 | Custom wait speed `(4,4)` then idle; wait Up 1; custom wait speed `(64,64)` then idle; falling SFX; wait Down 8. |
| 37–42 | Install **nonwaited** `eas_CounterClockwiseMoveUpRight`; `startEntity 153`; sleep 12; camera speed `$20`; wait `eas_InitSlow`; wait Up 2, Right 1, Up 11. |
| 43–48 | Face 153 Left; sprite CHICK; Bowie/Peter Right; chick claims qualified success 1685; end. |

`data/scripting/entity/eas_actions.asm:460–473` supplies the actual counterclockwise
loop: relative destinations `(-1,+1)`, `(-1,-1)`, `(+1,-1)`, `(+1,+1)`, each followed
by `ac_waitDest`, then branch back. The name is not an instantaneous move Up/Right.
There is no wait for this loop to finish; the later `eas_InitSlow` replaces its
pointer after sleep 12 and camera-speed work. InitSlow changes speed/acceleration,
not current/destination coordinates. A fixed final `(16,6)` cannot be inferred
from the starting table row and a nominal sum of movements. The program has no
`setPos` and its final relative movement depends on actual interrupted state.
It leaves an idle action after the last waited sequence, CHICK and requested Left
before the interaction wrapper restores the saved facing. Fresh ordinary Left
happens to agree; a different supplied facing is restored instead.

`eas_Init` sets speed `(32,32)`, zero acceleration factors, acceleration/deceleration
OFF, entity obstruction ON, entity/map uncollidable OFF, auto-facing ON,
orientation Up, immersed OFF and size 24, updates sprite, then jumps idle.
`eas_InitSlow` instead sets speed `(16,16)`, factors `(1,1)` and the source
`(-2,ON)` acceleration/deceleration pairs, followed by the same obstruction,
auto-facing/orientation/size setup. Flight leaves this slow configuration; it
never restores Init's speed 32 at its end. It does not change 152 or priority flags.
The final camera target remains 153 until the conditional exploration handoff.

## Complete fall and rescue scene

**Confirmed:** `cs_5779C` at `0x5779C` has **78 records: 67 opcode commands,
ten sleeps and one terminator**. Cursor 1686 yields four `nextSingleText` requests,
keys `0696..0699`; the first two use 153, the last two 152. There is no sprite-change
command, join, health restoration, map transition, entity rebuild or internal
story-flag write. Every record and inline body follows:

| Records | Ordered requests and inline actions |
| --- | --- |
| 1–9 | Cursor 1686; put 153 `(6,10)` Down; face 152 Up; camera destination `(1,5)`; wait Peter Init; put Bowie `(5,22)` Down and Peter `(11,22)` Up; install **nonwaited** Peter wait 60/Left 4; face Bowie Up. |
| 10–15 | Wait 153 `eas_46198`, then auto-facing OFF; jump SFX; wait Up 2, faceUp 5, Down 2; laughter 1686; sleep 40. |
| 16–19 | Jump SFX; wait 153 Up 2, faceUp 5, Down 2; laughter 1687; sleep 40. |
| 20–28 | Jump SFX; wait 153 Up 2, faceUp 5, Down 1; wait Init; wait jumpLeft 1; face 153 Up and 152 Left; camera tracks 153, speed `$30`; 152 calls the hatchling's name 1688. |
| 29–38 | Set Bowie priority 0, 153 priority `$FFFF`; wait 153 auto-facing OFF; custom wait speed `(48,48)` then idle; falling SFX; install **nonwaited** 153 Down 13; face Peter Left; repeat both priority writes; sleep 35. |
| 39–46 | Custom wait 152 speed `(64,64)` then idle; install **nonwaited** 152 Left 1/Down 11; explicitly `waitIdle 153`; quake 3; face Bowie Up; custom wait Bowie motion OFF/orientDown/updateSprite/jump idle; sleep 40; quake 0. |
| 47–56 | Wait 153 Init; face Down; four waited `eas_Jump` calls, each followed by sleep 20, including after the fourth. |
| 57–65 | Face 153 Up; camera neutral; two waited 152 `eas_Jump` calls; 152 thanks the leader and says the named hatchling is all right 1689; nod 152; sleep 30; wait 152 Init, then 153 Init. |
| 66–69 | Install **nonwaited** 152 Right 8; wait 153 Up 1/Right 8; hide 152; put 153 `(16,6)` Left. |
| 70–78 | Wait Peter Left 1; sleep 30; wait Peter `eas_BumpLeft`; wait Bowie Init; face Bowie Down; headshake Bowie; set Bowie priority `$FFFF`; attempt Peter→Bowie follow selector 2; end. |

Rescue imposes 153's initial/final positions, but only faces 152 at entry; it does
not put 152 back at `(6,10)`. That coordinate is a fresh alternate declaration,
not a guarantee about a supplied current actor. Likewise it does not normalize
153's sprite at any point. Text naming the hatchling and reporting that he is
safe is attributed content, not proof of contact/collision or an HP change.

`eas_46198` sets factors `(1,1)` and acceleration/deceleration `(-2,ON)`, then idle;
it does **not** set speed. Consequently a fresh Init-configured chick can begin
these bounces with speed 32, while completed flight leaves speed 16/InitSlow before
rescue. Record 22's Init later normalizes it to 32. Independent predicates do not
make the two orders identical in all actor state. This is a concrete shared-state
difference, not a claim about observed duration or a campaign-wide route.

Peter's initial arrival has no explicit idle barrier before his Left-facing
request. It can still be pending; later installation of Left 1 replaces his
current action rather than queuing behind it. Similarly `waitIdle 153` waits only
for the falling chick, not 152's chase. The first waited 152 Jump installs a new
pointer without first waiting for the old chase; her Right 8 departure also has
no dedicated completion wait before hide. The chick's waited exit gives time
for her action but is not a 152 barrier. Do not convert sleeps or concurrent
commands into confirmed arrival/contact.

With valid mappings and completed services, rescue's final explicit 153 position
and destination are `(16,6)`, Left, with its last action idle and Init configuration.
Its mapsprite remains the supplied one (normally CHICK after fresh declaration
or completed flight), and its priority-array byte remains 1. Bowie retains the
scene's `(5,22)` placement if his nonzero-HP position command ran and no other
movement intervened; the final facing request is Down subject to his HP check.
Peter's current position is not fixed by the final follow request. These state
boundaries are stronger than an inferred narrative, but are not observed gameplay.

## Action, animation, priority, HP and hide consumers

**Confirmed:** use `sf2cutscenemacros.asm`, `mapscriptengine_1.asm`,
`entityscriptengine_1.asm`, `entityscriptengine_2.asm` and `entityfunctions_2.asm`
for the following consumer distinctions, reusing the common-scripting evidence.

- `entityActions` / `entityActionsWait` encode opcode `$2D` with wait 0/`$FF`.
  The handler clears actor flags with `$9F`, constructs and installs an action
  pointer ending in a jump to `eas_Idle`, and only the waited form polls that
  pointer. Named/custom `$15`/`$14` actions likewise install immediately and
  optionally poll idle. None of these is an HP check, a queue, a global barrier,
  or a guarantee that movement is unobstructed.
- Cardinal movements emit `esc04_moveToRelativeDest`: signed deltas multiply by
  **384 internal units per tile** and add to current coordinates, with the
  destination-conflict consumer still relevant. Speed pairs are encoded X/Y
  bytes, written together by `esc10_setSpeed`; 4/16/32/48/64 are speed operands,
  not tile counts or durations. `faceUp 5` sets facing, updates sprite and waits 5;
  it is not Up 5. `eaWait 60` is an action wait, not a map-script sleep.
- Jump macros copy the shared action body. `eas_JumpRight` disables auto-facing,
  requests `(1,-1)`, waits 6, requests `(1,1)`, waits 6, requests `(0,0)`, enables
  auto-facing, then idle; JumpLeft uses `(-1,-1)`/`(-1,1)`. These timed relative
  requests are not a simple one-tile displacement guarantee. `eas_Jump` uses
  `(0,-1)`/wait 5/`(0,1)`/wait 5/`(0,0)`; BumpLeft uses `(-1,0)` then `(1,0)` with
  the same wait 5 structure. Movement timing/collision can matter.
- `csc1B_startEntityAnim` at `0x46A6C` writes animation counter 0 after its word
  lookup/HP helper. It does not start, resume or join an action pointer. For 153
  the NPC selector bypasses the ally HP check. `VInt_UpdateSprites` consumes the
  counter for walking-frame choice/increment; the source request does not prove
  rendered flight. FLYING_CHICK=182 and CHICK=230 are mapsprite selectors, not
  changes to a combatant's movement type.
- `csc53_setPriority` at `0x46FBE` resolves the physical actor and writes the
  byte at `byte_FFAFB0 + physicalIndex`: zero→0, any nonzero word→1. No HP test.
  `VInt_UpdateSprites` calls `sub_4E24`, which links the 48 ordinary entity
  sprites with nonzero entries first, then zero entries. This is distinct from
  the sprite tile's `VDPTILE_PRIORITY`, selected separately from window/layer
  state. Rescue requests 153 before Bowie in this linking scheme, then sets
  Bowie 1 at the end; it does not restore a saved prior byte or clear 153's 1.
  Actual overlap/visibility and VDP results remain Unknown.
- Bowie's custom `ac_motion OFF` is `esc14_setAnimationCounter` writing `$FF`,
  not a movement freeze. `ac_orientDown` sets orientation bits to 2 (flipped),
  separately from facing Up. Sprite update follows. Final Init restores
  orientation 0 but has no animation-on command; the later completed headshake
  explicitly ends with counter 0. Nod also temporarily sets `$FF` and ends at 0.
  Neither gesture has a general ally HP guard.
- `setPos` / `setFacing` probe their actual byte lookup: zero **word HP** skips
  the ally's four/two operand bytes. NPC152/153 bypass that test. Successful
  position writes current and destination X/Y in 384-unit tiles, plus facing.
  Action installs, sprites, priority, gestures and hide are not skipped by this
  helper. Zero Peter/Bowie HP therefore does not suppress an entire scene.
- `followEntity Peter,Bowie,2` encodes words `$0007,$0000,$0002`; its helper
  probes the first word's **high byte**, hence Bowie 0 HP, not Peter 7 HP. Zero
  skips all six bytes; nonzero resolves both mappings and calls `AddFollower`
  with selector 2's `(-24,0)` pixel offsets. It copies/patches a 42-byte action and
  appends the physical follower unless already listed; even a duplicate gets
  its pointer replaced. `esc03_follow` shifts offsets by 4 and applies its own
  direction/distance/layout rules. No wait for settled following, immediate
  fixed coordinate, F67 write, membership or future persistence is implied.
- `hide 152` calls `HideEntity`: current/destination X/Y become internal
  `$7000,$7000`; every matching mapping becomes `$FF`, and a matching follower
  entry is removed. It does not clear the actor's action pointer. Thus even
  completed hide is not proof that the nonwaited departure reached its target.
  This differs from init's coordinate-only removal below.

Map-script sleeps pass their low byte to `Sleep`, except that the existing debug
text-skip path can bypass them. They are service counts, not observed wall-clock
seconds. `setCameraEntity` resolves the mapping (or writes neutral `$FF`) without
waiting for scrolling. `setCamDest` selects neutral tracking and waits for view
scroll end. Camera speeds are service operands; quake 3/0 directly writes the
masked amplitude for these operands. The terminator waits for scrolling only
when the dialogue-window index is nonzero, then clears scrolling speed. Text,
sound, sprite/DMA, VInt and input-dependent services remain completion boundaries;
none supplies a natural or rendered-scene observation here.

## Caller return, re-entry and independent ordering

**Confirmed:** after flight return the caller sets F723, then the entity wrapper
restores the saved physical chick's facing, closes windows and reactivates entity
updates. It adds no post-caller main-movement wait. After rescue return the caller
sets F724, then the zone wrapper closes windows and waits for physical 0 to stop.
Neither wrapper turns a set flag into proof of completed services.

After either ordinary route returns to `ExplorationLoop`, `WaitForEvent` retargets
camera 0 and calls `SetControlledEntityActScript(0)` only if pending map-event type
is zero. `PLAYER_TYPE=0` selects `eas_ControlledCharacter`; other types select
their own action. Completed caller/window/VInt/movement services and zero pending
are necessary for this conditional handoff; no responsive-input observation is
claimed. Stop at this boundary.

Alternate `s6_initfunction_722.asm` (retained init body 15 at `0x5708E`) tests F799
and conditionally moves Luke 10 out of map, then **independently** tests F724 and
moves 152 out of map. It reads neither F723 nor F13. `MoveEntityOutOfMap` resolves
the current mapping and calls `SetEntityPosition` with `$7000,$7000`, writing
current and destination coordinates without invalidating mappings, clearing
following or changing action pointers. F799 only controls Luke here. F13-set
suppression does not hide 152; F723 does not replay or reposition 153. The default
F722-clear init is different: its F719/F720 guard/guard-removal behavior cannot
be imported into this alternate actor population.

| Boundary or supplied ordering | Retained distinction |
| --- | --- |
| Fresh explicit Map10 entry | `ExplorationLoop` heals living/immortal allies, rebuilds population/mappings, then runs selected init. Alternate 153 starts CHICK `(16,6)` Left/Init; 152 starts `(6,10)` Right/Init but F724 moves her out of map. Newly valid mapping 152 survives that removal. F67 supplies Peter independently of prior scene following. |
| Current-map entry (`MAP_CURRENT=255`) | Updates main properties/position and raft/follower placement, loads map and reruns init **without rebuilding fixed actors/mappings**. It does not reset 153's prior action/speed/sprite/priority to its table declaration. Init does not repair a 152 mapping already invalidated by hide; it resolves the current `$FF` as-is, so no valid-actor cleanup is guaranteed. |
| Rescue with F723 clear / F799 either value | Admitted if F13/F724 clear; completion sets only F724. Flight remains eligible at a later valid entity interaction. Its relative actions start from current 153 state; it does not restore rescue's priority 1 to fresh priority 0. |
| Flight followed by rescue | F723 does not gate rescue. Completed flight leaves CHICK/InitSlow; rescue sets 153's coordinates but initially retains its speed until record 22's Init. |
| F724 supplied set before init versus after init | Before alternate init, 152 is coordinate-removed with a retained mapping. Set after init, the suppressed zone branch does not repair a still-present 152. A partial scene can instead hide 152 while F724 remains clear. |

Fresh `ClearEntities` also calls `graphics_2.asm:sub_19B0`, clearing 64 bytes from
`byte_FFAFA0`, which includes all 48 priority entries starting `byte_FFAFB0`.
Follower initialization sets main's priority entry to 1 and each declared follower's table
priority; regular 152/153 retain 0. This fresh state differs from rescue's retained
153 priority 1. Entry healing processes immortal Peter but proves neither F67 nor
a valid mapping or particular supplied maximum/current HP at interaction time.
No campaign ordering, later persistence, collision or recruitment follows from
these bounded state distinctions.

## Reproduction and review boundary

Load `. ./local/private-inputs.ps1` in the owning checkout's command process and
select the verified clean read-only pinned `$upstream`. Reuse the existing
[retained payload/cutscene comparison recipe](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary).
The comparisons do not interpret every source consumer or prove natural reach.
Use these direct discriminators for this document:

| Evidence | Reproduction selection |
| --- | --- |
| Full scenes | Retained `programCorpus.programs` entries `cs_5766C` / `cs_5779C`; match all 48/78 records' `sourceLine`, macro and operands to exactly 415–496/497–614; review all inline bodies in those 200 lines. Kinds 44+3+1 / 67+10+1; no omitted final sleeps or nonwaited actions. |
| Text | Resolve cursor 1680–1685 and 1686–1689 to `gamescript.txt` keys `0690..0699`; repeat caller reuses 1685. Ten distinct text IDs across 11 request sites. Preserve speaker 153 versus 152 and `{NAME;20}` attribution. |
| Callers | `tests/fixtures/h2/map-events/entity-programs.json` program 30 and `zone-programs.json` program 5: all seven operations each, branch polarity, targets, source lines and one table reference each. Entity row 25 and zone row 0 in alternate tables. |
| State/services | Named F722 selection, alternate rows/init, population/lookup, source action bodies, action/HP/animation/priority/hide/follow consumers, both wrappers, zone producer and pending-dependent control. Contrast supplied/partial/fresh/current-map state, not merely the favorable route. |

**Unknown:** natural zone/interaction reach and layout/collision/floors; actual
supplied flags, actors, mappings, HP, party/AI/follower and player state;
nonwaited action progress, sleep/input/VInt/service timing and completion;
rendered flight/fall/contact, sprite order/orientation, dialogue/camera/audio and
hide; pending events, responsive control and later persistence/playability.
No runtime work is automatically admitted and no future Skreech payoff is claimed.

Record direct document/link/provenance/private/full-diff checks, preserved failures,
clean committed `uv run sf2 verify plan --base origin/main --head HEAD` and actual
final-head CI in the Issue/PR handoff. The explicit documentation-only scope does
not select local blanket normal/full/engine/H3/H4/Godot suites. Deliver only this
path in a clean pushed Draft PR, set Project Review / Verify, freeze and confirm
the authorized main-gate notification. Independent integration remains main-gate's;
no cleanup or next-Issue selection is authorized.
