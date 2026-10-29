# Creed Mansion entry and first Goliath scene

Evidence date: 2026-10-14. **Confirmed:** the separate Devil's Tail doorway
requests Map33 `(15,30)`, Up. First-visit initialization does not run the
Goliath scene. An admitted zone `(15,21)` event with F774 clear runs the complete
`cs_5AC58`; only its return permits the callback's F774 write. The scene changes
two live map blocks, stages the obstruction and installs two follower relations,
without a warp, recruitment or choice. **Inferred:** its B speaker is intended
as Oddler. **Unknown:** natural reach, supplied state, service completion,
rendered performance, usable control and save persistence.

## Agreement and provenance

Standard rigor under [Issue #892](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/892)
and [content epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
The novel joins are entrance admission, setup precedence, actor allocation and
complete scene/action consumers. This sole owned document supplies the next
story guide; no shared/index/contract/schema/fixture/code/design/remake edits.
Accepted starting base is `f43add920e4c905195f35cb6009d870433153f19`.
Consume accepted [Battle21 outcomes](./battle21-outcomes-and-devils-tail-return.md),
[map inventory](../../world/map-data-inventory.md),
[request consumption](../../world/map-event-request-consumption.md) and
[map setup selection](../../world/map-event-flag-route-selection.md). No stacked dependency.

The selected first visit has F523/F783/F784/F786/F22 clear and initially F774
clear. Identify alternate setup/init/zone guards, but do not research their
later scenes. Include the immediate F774-set/F785-clear same-zone return and
baseline F774-set reentry placement. Stop before zone `(12,14)`'s separate
shrinking/collection program, later departure/restoration/basement/recruitment,
Chessboard/Battle22, whole-mansion exploration or general rendering/terrain work.
No runtime acquisition or new verification machinery is admitted.

Paths below are relative to `disasm/` in
[SF2DISASM master at `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The assigned read-only source checkout is clean at that pin. Retained evidence
binds USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
This is source/retained-evidence review, not a new ROM extraction or playthrough.
Fnnn means flag nnn. Command indices are zero-based; all coordinates, speeds and
waits are source operands rather than measured motion or time. Short setup paths
mean `data/maps/entries/map33/mapsetups/`; short map/entity consumer paths mean
`code/common/scripting/map/` and `code/common/scripting/entity/`, respectively.

Reuse the unchanged 18,285,979-byte map-init/map-script-engine pair, with bounded
source/fixture reads. Linear comparison budget: under 512 MiB and one minute,
plus small prose/receipts. Retained pair/source comparisons completed in 0.081
seconds. No environment, payload or capture was created. Reassess before any
unplanned subsystem or material acquisition/resource expansion.

## Doorway request and fresh exploration

**Confirmed:** `data/maps/entries/map67/6-warp-events.asm` selects `(26,22)`:
warpNoScroll, MAP_CREED_MANSION=33, destination `(15,30)`, Up. The predecessor's
victory scene returned to `(26,23)`, Down. Neither that adjacent arrival nor the
doorway tuple proves a player subsequently reaches or activates the doorway.

The warp consumer in `code/gameflow/exploration/explorationfunctions_2.asm`,
ProcessMapEventType1_Warp, consumes the shared request parameters, idles main,
unwinds the exploration caller and returns the map/position/facing to MainLoop.
For this no-scroll request parameter1 is zero. MainLoop's ordinary post-warp
loop calls SwitchMap **then CheckBattle**; this differs from the predecessor's
alternate post-battle entry that skips CheckBattle.

`code/common/maps/mapinit_0.asm:SwitchMap` and the complete
`data/maps/global/flagswitchedmaps.asm` have no Map33 switch. The complete
`data/battles/global/battlemapcoords.asm` has no Map33 battle row.
`code/common/maps/getbattle.asm:CheckBattle` therefore scans to d7=-1 and
preserves the input tuple; no supplied unlock flag alone makes this entrance
Battle22. MainLoop then calls ExplorationLoop.

Fresh exploration, with explicit map 33, clears MAP_EVENT_TYPE, subtracts 20000
from STEP_COUNTER with a negative clamp, heals eligible allies, starts fade-out
and disables view following. It writes CURRENT_MAP=33/CURRENT_BATTLE=255,
loads tilesets, waits fading, clears VInts, selects the setup entity list,
initializes population at the requested coordinates and loads mapsprites. It
clears temporary flags 256–383, sets F80, initializes sprite-frame state, selects
view target 0, loads the map, installs base VInts and runs the selected init.
Palette comparison controls the subsequent music/fade calls.

`code/common/maps/mapload.asm:LoadMapBlocksAndLayout/LoadMapArea` establish
the full-map coordinate domain: CURRENT_BATTLE=255 avoids the cropped battle-map
loading branch. Stale Battle21 area offsets do not get added to
Map33's arrival or scene operands. The map-255 preserve-population branch is not
this explicit Map33 request. WaitForEvent installs view target physical 0 and
the current-mode controller only when pending MAP_EVENT_TYPE is zero; pending
work takes precedence. Source order is conditional on all called services
returning, not proof of a usable arrival.

## Setup selection, population and init

**Confirmed:** `mapsetupsfunctions_1.asm:GetCurrentMapSetup` starts with the map's
default pointer and replaces it for **each** set flag until msMapEnd. The last
set entry wins, not the first. `data/maps/mapsetups.asm` orders Map33 as follows:

| Priority in source order | Pointer selected when flag is set |
| --- | --- |
| Default | ms_map33 |
| F523 | ms_map33_flag523 |
| F783 | ms_map33 |
| F784 | ms_map33_flag784 |
| F786 | ms_map33_flag786 |
| F22, last | ms_map33 |

Thus all five clear selects ms_map33, but that pointer alone does not establish
first-visit state: F783 or F22 can restore it after earlier alternates. F774 and
F785 do not select setup pointers. Each setup accessor calls the selector;
selection is not a promise that flags remain fixed across later calls.
`pointertable.asm` supplies Entities, EntityEvents, ZoneEvents,
AreaDescriptions, Section5 and InitFunction in that order.

The complete `s1_entities.asm` has six fixed rows, then msEntitiesEnd:

| Row | Logical lookup | Mapsprite source | Initial position/facing/action |
| --- | --- | --- | --- |
| 0 | 128, first ordinary NPC | WORKER=202; scene identifies Goliath | `(63,63)`, Up, eas_Init |
| 1 | 129, second ordinary NPC | CREED=212 | `(63,63)`, Left, eas_Init |
| 2 | Eric 14 | Ally sprite lookup | `(63,63)`, Down, eas_Init |
| 3 | Randolf 16 | Ally sprite lookup | `(63,63)`, Up, eas_Init |
| 4 | Tyrin 17 | Ally sprite lookup | `(63,63)`, Down, eas_Init |
| 5 | Karna 15 | Ally sprite lookup | `(63,63)`, Up, eas_Init |

`mapfunctions.asm:InitializeMapEntities` clears entities and the lookup table,
allocates selected followers starting at physical 1, then consumes these rows.
It masks table X/Y with `$3F` and scales by MAP_TILE_SIZE=384. Ordinary non-ally
rows append physical indices to the lookup region beginning at offset 32, which
GetEntityAddressFromCharacter resolves from logical 128 onward by subtracting
96. Ally rows use their ally lookup; a nonzero existing lookup skips the entire
fixed row, including allocation. No joined/active membership test gates these
fixed declarations. There is no fixed Peter row or scene newEntity fallback.
Bowie is then declared at physical 0 with requested `(15,30)`, Up, eas_Idle.
These parked declarations are not evidence of visible room occupants.

`followersfunctions_1.asm:InitializeFollowerEntities`,
`data/scripting/entity/followers.asm` and `data/maps/global/overworldmaps.asm`
show that Map33 is **not** overworld. F65 only selects the Caravan/A prefix on
an overworld map; it contributes no prefix here. Ordinary follower rows still
use their independent flags. With only F67/F68 selected among those rows,
Peter 7 is physical 1/eas_Follower1, B 31 (Oddler sprite) is physical 2/eas_Follower2;
128/129 then map to physical 3/4, Eric/Randolf/Tyrin/Karna to 5/6/7/8.
All selected followers initially use the supplied arrival coordinates/facing.

That physical example is conditional. Other selected rows consume slots and
actscript pointers in declaration order. Repeated B rows (F74 Astral, F68 Oddler,
F70 Astral) are not deduplicated by follower initialization; later rows overwrite
the logical B lookup while earlier physical entries remain. Missing F67/B rows
leave zero lookups, which can alias physical Bowie 0. Party membership does not
substitute for these follower flags. The same logical operand can therefore
address a different actor or overwrite an earlier actor's action.
GetAllyMapsprite uses class and HP; zero-HP allies outside battle select the
blue-flame sprite. Exploration healing does not revive every dead ally.

PLAYER_TYPE is not changed by this doorway/init/scene. The coherent predecessor
supplies walking mode 0. `followersfunctions_2.asm:sub_44404` also admits an F64
raft at physical 31 when RAFT_MAP=33, using saved coordinates masked with `$7F`;
it does not board or relocate the saved raft. Supplied mode 2 instead changes
main to the raft sprite and makes followers stand before its raft declaration.
SetControlledEntityActScript selects mode 0 eas_ControlledCharacter, mode 1
Caravan control through the source-named eas_Raft, otherwise source-named
eas_Caravan (raft control); arbitrary mode is not validated here.

The **complete** `s6_initfunction.asm` guards establish the selected boundary:

| Ordered guard | Local effect and scope |
| --- | --- |
| F523 set and F783 clear | Runs cs_5B016, then F783 and EGRESS_MAP=Hassan; later restoration scene excluded. |
| F22 set | cs_5ABA8 positions 128 `(14,14)`, Up; bypasses the following placement alternatives. |
| Else F784 set | Skips the intervening placements. |
| Else F783 set | cs_5ABCC positions 128 `(17,10)`, Down. |
| Else F774 set | cs_5ABE6 positions 128 `(17,12)`, Down. |
| Tail: F784 set | For each not-joined F14/F16/F17/F15, positions Eric/Randolf/Tyrin/Karna; later recruitment context excluded. |

With the five later-story flags clear and F774 clear, init reaches rts with
none of those scripts/writes. It does not run cs_5AC58 or create a zone event.
Baseline reentry with only F774 changed runs cs_5ABE6, then returns; the six-row
population is rebuilt, and Goliath alone receives that immediate placement.
This placement does not replay the first scene's block copies or conversation.

## Zone admission and complete first scene

**Confirmed:** the zone producer in `entityscriptengine_2.asm` (loc_5294) sees
layout type `$1400`, writes MAP_EVENT_ZONE_EVENT=6 and derives word X/Y from
main destination plus movement offsets divided by MAP_TILE_SIZE. A setup row
alone does not prove that layout/input/collision naturally reaches this writer.
ProcessMapEvent clears the pending type before dispatch. Type6 applies main's
init actscript and passes parameter1/3 words to RunMapSetupZoneEvent.

The complete `s3_zoneevents.asm` selects `(15,21)`→Map33_ZoneEvent0,
`(12,14)`→Map33_ZoneEvent1, otherwise the rts default. The wrapper resolves the
current setup and first matching zone row; `(15,21)` is not the `(15,30)` arrival.
Zone0 checks F774 first. Clear calls `script cs_5AC58`; **after the complete
script returns**, setFlg 774 executes and branches directly to rts. It does not
fall through to the later F785 branch on the first invocation.

The complete selected program is `scripts.asm:cs_5AC58`, lines 4–191,
address `0x5AC58`, ending at csc_end before `cs_5AF36` at `0x5AF36`.
All 129 retained commands were joined to source line, macro and arguments;
all inline/custom bodies were read through their terminators. P=Peter 7,
B=FOLLOWER_B (31); 128/129 are logical NPC lookups, not physical slot numbers.
The following ranges cover the whole program in order:

| Commands | Complete local sequence |
| --- | --- |
| 0–14 | textCursor 1804; 129 then 128 speak. Camera destination `(12,11)`; wait 30; stepEvent `(17,11)`; set 128 `(17,11)` Down. Waited 128 moves Down 3; wait 5; install 128 eas_2xRightLeft without waiting; wait 80; text 1806. Camera follows 128; waited 128 moves Left 5; face 128 Up. |
| 15–23 | Waited eas_Init on P then B. Set P `(15,21)`, Bowie `(15,22)`, B `(15,23)`, all Up. stepEvent `(15,20)`; camera destination `(8,12)`; wait 30; portrait-none narration 1807. |
| 24–39 | Install P Up 4/Left 3/Up 1; Bowie Up 5/Left 3; B Up 6/Left 2 **waited only for B**. Face Bowie/B Up; P text 1808; shiver 128; 128 text 1809; stop 128 animation; wait 30; face 128 Right; wait 60; face 128 Down; wait 30; start 128 animation; 128 text 1810. |
| 40–52 | Waited P Up 1; P text 1811, 128 text 1812–1813, P text 1814. Waited P Down 1; headshake Bowie; waited P Up 1; P text 1815, 128 text 1816–1817. Waited P Down 1; P text 1818. |
| 53–62 | Face Bowie Right/B Left; B text 1819–1820. Waited P Right 1; face P Down, Bowie/B Up; P text 1821–1822. |
| 63–74 | Face 128 Up; wait 10; shiver 128. Waited P Left 1; face Bowie/P/B Up; shiver 128; wait 10; 128 text 1823; wait 20; P text 1824. |
| 75–89 | Camera follows Bowie. Install Bowie/P eas_Jump, then B eas_Jump waited. Three complete custom waited bodies, Bowie/P/B: ac_setSpeed 40,40; ac_jump eas_Idle; ac_end. Install P, B, Bowie movement Right 5/Up 3 without waiting; wait 10; 128 WIPE_OUT; set 128 `(17,12)` Down; 128 WIPE_IN; waitIdle Bowie. |
| 90–99 | Install P/B eas_BumpUp, then Bowie eas_BumpUp waited. P text 1825; shiver 128; 128 text 1826–1827. Waited eas_DeactivateAutoFacing on Bowie, P, B in that order. |
| 100–112 | Three complete custom waited bodies, Bowie/P/B: ac_setSpeed 10,10; ac_jump eas_Idle; ac_end. Install Bowie Down 1, P Down 1, B Down 1 waited. Waited eas_Init on Bowie/P/B. wait 25 then wait 5; install P eas_2xRightLeft without waiting; wait 120. |
| 113–125 | P text 1828; face P Down; P text 1829. Face Bowie Right/B Left; B text 1830; nod Bowie; face P Up; P text 1831–1832; face B/Bowie Up; B text 1833. |
| 126–128 | followEntity P,Bowie,2; followEntity B,P,2; csc_end. No warp, flag write, join, prompt or call into cs_5AF36 occurs in this program. |

## Text order and actual consumers

**Confirmed:** `data/scripting/text/gamescript.txt` IDs `070C`–`0729` are decimal
1804–1833. All thirty literals, including spacing and controls, exactly match
the selected source comments in order. The table paraphrases their content;
the controls column preserves the ordered source tokens for reproduction.
S=nextSingleText; T=nextText. Every speaker uses modifier `$0` except 1807.

| ID | Command/speaker | Content, paraphrased | Ordered controls |
| --- | --- | --- | --- |
| 1804 | S/129 | Dismisses the servant to speak with the fairy | `{N}{W1}` |
| 1805 | S/128 | Acknowledges his master | `{W1}` |
| 1806 | S/128 | Hums, then remembers checking the collections | `{N}{N}{W1}` |
| 1807 | S/ENTITY_NONE, `$FF` | Narrates Goliath unlocking the door | `{N}{W1}` |
| 1808 | S/P | Requests attention | `{W1}` |
| 1809 | S/128 | Startled exclamation | `{W1}` |
| 1810 | S/128 | Hesitant response | `{W1}` |
| 1811 | S/P | Requests an audience with Creed | `{W1}` |
| 1812 | T/128 | Asks whether they mean his master | `{W2}` |
| 1813 | S/128 | Asks about an appointment | `{W1}` |
| 1814 | S/P | Does not understand | `{W1}` |
| 1815 | S/P | Admits having no appointment | `{W1}` |
| 1816 | T/128 | Explains that Creed is busy | `{W2}` |
| 1817 | S/128 | Tells them to return later | `{W1}` |
| 1818 | S/P | Reports the refusal | `{W1}` |
| 1819 | T/B | Reacts to the refusal | `{W2}` |
| 1820 | S/B | Urges the leader and Peter to force entry | `{LEADER}{NAME;7}{N}{W1}` |
| 1821 | T/P | Calls Oddler bold | `{W2}` |
| 1822 | S/P | Approves the idea | `{W1}` |
| 1823 | S/128 | Disapproving reaction | `{W1}` |
| 1824 | S/P | Signals the attempt | `{W1}` |
| 1825 | S/P | Questions Goliath's sudden interception | `{W1}` |
| 1826 | T/128 | Rebukes the group and bars entry | `{N}{W2}` |
| 1827 | S/128 | Repeats that Creed is busy and orders them away | `{N}{W1}` |
| 1828 | T/P | Suggests the door on their left may reach Creed | `{LEADER}{N}{N}{W2}` |
| 1829 | S/P | Proposes avoiding the servant via that door | `{N}{W1}` |
| 1830 | S/B | Urges the leader onward, unwilling to fail | `{LEADER}{N}{W1}` |
| 1831 | T/P | Pretends to concede | `{W2}` |
| 1832 | S/P | Says they will see Creed later and leave | `{N}{W1}` |
| 1833 | S/B | Agrees with the apparent retreat | `{W1}` |

`sf2cutscenemacros.asm` encodes the speaker/modifier word and csc04 text cursor.
`mapscriptengine_2.asm:csc00/csc02` consume that cursor, wait for view scrolling,
call DisplayText and increment it. S then closes portrait/text and sleeps 10;
T keeps the window for continuation. `{N}` is a line break; W1/W2 retain their
text wait/continuation distinction, not a branch/choice. Name controls are
runtime substitutions, not hardcoded proof of the supplied leader/name.
The accepted text consumers are covered by the retained dialogue facts.

For 1807, `$FF,ENTITY_NONE` encodes `$FFFF`: the consumer bypasses portrait/speaker
lookup and writes speech SFX0. Other text resolves the **actual mapped sprite**
through `getentityportaitandspeechsfx.asm` and its dialogue-property table.
Modifier 0 sets neither the right-side nor mirrored portrait bit; an already open
portrait or missing portrait result still affects opening. A speaker number
does not guarantee a rendered portrait. Debug/P2-Start can latch SKIP_CUTSCENE_TEXT;
selected text and csWait handling is then bypassed according to the dispatcher.
The nominal thirty-text sequence assumes that skip mode is not latched.

**Inferred:** Peter's explicit address to Oddler and the F68 B-row sprite support
the intended B attribution. This does not prove F68 was supplied, exclude another
B-row override or make B's lines a truth claim about a successful later route.
The narration identifies the servant; the source's fixed sprite remains WORKER.

## Block copies, actions and presentation boundaries

**Confirmed:** `mapscriptengine_1.asm:csc47_StepEvent` scales its two tile
operands by 384 and calls the actual OpenDoor consumer in
`code/gameflow/exploration/exploration.asm`. OpenDoor returns immediately if
CURRENT_BATTLE is not 255. Otherwise its coordinate conversion and current map's
step table select these exact eight-byte records in
`data/maps/entries/map33/4-step-events.asm`:

| Scene command | Trigger | Source block | Size | Destination |
| --- | --- | --- | --- | --- |
| 5 | `(17,11)` | `(26,0)` | 1x1 | `(17,11)` |
| 20 | `(15,20)` | `(25,0)` | 1x1 | `(15,20)` |

Both source Y operands are nonnegative, so the selected consumer copies one
layout word from FF0000's 64-word-stride map layout to the destination. It plays
SFX_DOOR_OPEN and sets the plane-update bit selected by MAP_AREA_LAYER_TYPE.
No save, F774 update, warp or persistent event flag is written by this consumer.
The complete Map33 flag-event table has only F22's large replacement, not an
F774 replay of these two edits. A fresh baseline reentry therefore does not
reconstruct these copies from F774 alone. This is a live layout mutation;
rendered opening, collision/passability and eventual traversal remain separate
questions. The text's unlocking statement does not prove a natural itinerary.

`sf2cutscenemacros.asm`, `mapscriptengine_1.asm` and
`data/scripting/entity/eas_actions.asm` resolve the selected commands:

- csc32 disables camera following, sets the requested camera destination and
  waits for view scroll end. csc24 resolves a logical actor to the view-target
  physical slot. The ordered destinations are `(12,11)`, follow 128, `(8,12)`,
  follow Bowie. A later successful pending-zero WaitForEvent also selects main 0.
- csc19 sets position **and destination**, scales tiles by 384 and updates facing
  and sprite. csc23 changes facing/sprite. Their ally-alive guards skip their
  operands for a zero-HP ally. They do not repair missing lookups. csc1B/1C
  start/stop **animation counters** (0/-1); they are not motion locks.
- csc14/custom and csc15/named actions install on the resolved physical actor.
  Wait variants poll that actor's actscript address until eas_Idle. csc16 does
  the same. They do not wait for every concurrently installed action, and have
  no timeout or missing-actor recovery. These installers do not have csc19's
  HP skip. Custom bodies are delimited by ac_end (`$8080`). Their ac_setSpeed
  operands 40/40 and 10/10 are packed speed values consumed by esc10_setSpeed,
  not measured pixels per frame. Each complete body jumps to eas_Idle.
- csc2D compiles every inline relative move into the action buffer, clears
  flags A bits 5/6, appends a jump to eas_Idle and installs it. The waited form
  polls only its selected actor. esc04_moveToRelativeDest scales signed tile
  deltas by 384 from current position and checks other-entity destination
  conflict before movement setup. Installed motion, a csWait or another actor's
  idle does not prove each destination was reached. All 17 inline bodies in the
  table above include their endActions; all six custom bodies include ac_end.
- eas_Jump and eas_BumpUp both disable autofacing, move relative `(0,-1)`, wait 5,
  move `(0,1)`, wait 5, move `(0,0)`, restore autofacing and jump to eas_Idle.
  eas_DeactivateAutoFacing disables it and jumps idle. eas_Init resets speed 32/32,
  acceleration factors 0/0, acceleration/deceleration off, obstructability on,
  entity/map uncollidable off, autofacing on, orientation Up, immersion off,
  size 24 and sprite, then idle. Reset installation is not an extra position set.
  eas_2xRightLeft performs Right/Left/Right/Left with sprite update and wait 30
  after each, then idle; the scene's initial wait 80 is not a wait for that
  entire named action, which is subsequently replaced by movement.
- csc2A/shiver temporarily changes sprite size/animation and toggles a sprite
  flag through three wait 5/wait 5 iterations, then restores them. csc26/nod and
  csc27/headshake run their selected synchronous sprite/DMA/wait sequences and
  restore animation. These helpers do not acquire a rendered observation.
- csc22 resolves 128, consumes WIPE_OUT=2 then later WIPE_IN=3. Its selected
  table rows are `(1,0,1,0)` and `(22,0,-1,0)`; each uses 23 crop/DMA iterations
  with two VInt waits per iteration. The explicit setPos between them supplies
  `(17,12)`, Down. No actor creation, map warp, mosaic branch or general
  animation/renderer claim follows from these two selected effects.

The source's final `followEntity` macro encodes follower/target/place as words.
csc2C's alive guard first reads the **high byte** of the follower word: both 7
and 31 supply 0, so both commands test Bowie's HP, not Peter's or B's. If admitted,
they resolve physical actors and select place 2→`(-24,0)` from
`table_FollowerPositions`. Ordered relations are Peter→Bowie then B→Peter.

`entityfunctions_2.asm:AddFollower` installs a 42-byte eas_Follower1 copy with
physical target and offset operands, advancing the walking buffer; it appends
the physical follower to EXPLORATION_ENTITIES only if absent. No joined/active
or persistent follower flag changes. Its template in
`data/scripting/entity/eas_main.asm` resets speed 32/32 and its movement flags
when executed, then uses follow/destination waits. The relation does not specify
an absolute landing tile and neither call waits for settling. Missing lookups
can produce self-follow or overwrite another relation; no alias check prevents
it. Reentry discards these temporary actions and rebuilds from follower flags.

## Return, repeat and stopping boundary

**Confirmed:** ExecuteMapScript's csc_end conditionally waits for view scrolling
when a dialogue window is present, clears VIEW_SCROLLING_SPEED and restores its
caller registers. It does not itself set F774. Only the subsequent Zone0 callback
writes that flag. RunMapSetupZoneEvent then closes portrait/text, waits one VInt
and calls WaitForEntityToStopMoving for **physical 0**. That consumer waits for
current X/Y to equal X/Y destination, not for all followers' scripts to be idle.
The exploration loop reaches WaitForEvent again; a new pending request is handled
before it can reinstall normal camera/control. Earlier changes are not rolled
back if any later action, scroll, text, input, DMA/VInt or destination service
stalls. F774 may already be set when a later wrapper wait stalls.

| Changed/supplied state | Immediate discriminating consequence |
| --- | --- |
| F774 clear; later baseline flags clear | Full first scene selected on admitted zone0; F785 is not checked on this branch; F774 written only on return. |
| F774 set, F785 clear | Zone0 returns with no scene, movement, text or flag change; wrapper closure/waits still run. |
| F774 set, F785 set, F786 clear | Later cs_5B466 then F786/F69 set and F68 clear; identified only, outside the selected scene. |
| F774 set, F786 set | Later departure is bypassed; setup precedence may already have selected another population/init. |
| Fresh baseline reentry with F774 set | Six fixed rows rebuilt; cs_5ABE6 places 128 `(17,12)` Down. No first-scene replay or F774-based block-copy restoration. |
| F67/F68 missing; conflicting B flags; changed HP/class | Logical/physical aliasing, different B sprite or HP-skipped placement/facing/follows; no guaranteed cast or completion. |
| Supplied mode/raft state | Controller/population tail follows the actual mode and F64/RAFT_MAP state, not dialogue. |
| Pending work or stalled service | A partial mutation is not a completed scene/control handoff; latest pending event takes priority. |

Zone `(12,14)` separately checks F783; clear invokes cs_5AF36, set returns. Its
shrinking/collection body begins exactly after this selected csc_end and is the
next boundary. No first-scene fallthrough invokes it. Neither the characters'
plan nor the two follower relations guarantees that event or any later route.
F774 and follower/later-story flags survive the temporary 256–383 reset, but
surviving RAM state is not evidence of successful save/load persistence.

## Reproduction and review boundary

Load the worktree's `. ./local/private-inputs.ps1` in the same process as each
controlled tool launch. Verify source HEAD/clean status at the pin above. Reuse
the [retained comparison recipe](../grans-opening/battle06-to-battle07-content-route.md#reproduction-and-review-boundary):
map-init exact equality, `_verify_map_script_engine_fixture`, and source
`_cutscene_facts` equality. Do not regenerate the retained pair merely to review
this prose. Selected sources/labels/lines/tables above make direct joins repeatable.

| Evidence surface | Selected check |
| --- | --- |
| `local/derived/map-script-engine-static.json`; `tests/fixtures/h2/map-script-engine-static-v1.json` | All 129 cs_5AC58 source-line/macro/argument joins, complete 17 inline and six custom bodies; selected macro/handler/action/follow consumers. |
| `data/scripting/text/gamescript.txt` | Sequential decimal 1804–1833 exact literal/comment equality including spacing and controls; caller speakers/modifiers and continuation distinction. |
| `local/derived/map-init-static.json`; `tests/fixtures/h2/map-init-static-v1.json`; `map-setup-static-v1.json` | Retained equality and actual Map33 setup precedence, complete baseline pointer/entity/init/zone bodies, follower/allocation/mode/HP/alias consumers. Fixture paths after the first two are relative to `tests/fixtures/h2/`. |
| `tests/fixtures/h2/map-events/zone-programs.json`; selected source | Zone0 full F774/F785/F786 branch, adjacent zone1 guard only, callback versus wrapper return. |
| Selected Map67 warp, complete switch/battle-coordinate tables and exploration/step consumers | Doorway-to-MainLoop admission, fresh full-map population, both actual step-copy records, pending/control and immediate reentry. |

Retained comparisons do not automatically guard every new prose claim; the
complete selected source/consumer review supplies those joins. Acceptance uses
the entire owned diff, links/provenance/private boundary, whitespace,
clean committed configured `uv run sf2 verify plan --base origin/main --head HEAD`
and actual final-head CI. No blanket local normal/full/engine/Godot/H3/H4 suite
is selected solely for prose. Exact Git/CI identities and inspection failures
belong in Issue892's handoff. Preserve
[Issue888's original handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/888#issuecomment-6173000296),
[rebase receipts](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/888#issuecomment-6173114839)
and [PR890 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/890#issuecomment-6173127050),
including earlier linked failures and unchanged evidence.

**Unknown:** natural reach/itinerary, actual flags/party/HP/class/lookups,
geometry/mode/raft/pending state, completed services, rendered scene, usable
control and persistence. Changed-state rows are static discriminators, not
executed scenarios or an automatic runtime queue under
[ADR0014](../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../../../decisions/0016-remake-start-evidence-deferral.md).
Stop at the bounded local return/repeat and leave a clean frozen Draft PR for
independent review; no integration, cleanup or successor selection.
