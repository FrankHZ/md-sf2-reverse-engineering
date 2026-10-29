# South Parmecia through Kraken / Battle16 entry

Evidence date: 2026-10-13. **Confirmed:** the selected Map69 warp resolves
`MAP_KRAKEN_BATTLEFIELD` to Map49 and returns through battle admission. F416 admits
Battle16 independently of raft mode; F466 separately suppresses its introduction.
The complete introduction loads scene actors, requests the river-monster reveal,
then returns to fresh combatant initialization and an empty battle-start script.
**Inferred:** the staging represents a Kraken stopping the raft. **Unknown:**
natural traversal, actual caller state, service completion, rendered presentation,
input response and persistence. No battle turn or outcome is established here.

## Agreement and provenance

Standard rigor under [Issue #820](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/820)
and [the content epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514):
the map selector, scene actors and combatant initialization have different state
boundaries. This sole-file research slice starts at conditional Map69 control and
stops after Battle16's start wrapper returns, before enemy activation, region
scripts, spawning, turn generation or combat. It does not assert Battles14/15
occurred or that numbered battles prescribe a natural route.

Accepted base is `16c5ca95ce02e9c8c022a04c46248db6ab1509ba`, including corrected
[raft boarding / South Parmecia entry](./polca-raft-boarding-through-south-parmecia-entry.md)
and [Luke recruitment](../eastern-parmecia/polca-departure-through-luke-recruitment.md). Reused owners
are [request consumption](../../world/map-event-request-consumption.md),
[common scripting](../../world/common-scripting.md), [battle cutscenes](../../battle/battle-cutscenes.md),
[battle lifecycle](../../battle/battle-loop.md), [spriteset data](../../battle/battle-spriteset-data.md)
and [membership services](../../party/common-stats.md). No stack, shared navigation,
fixture/schema/parser, design, translation or remake change. Other exits,
disembark completion, terrain/pathfinding, continuous travel, outcomes, generalized
graphics research and new runtime acquisition are excluded.

Source paths below are relative to `disasm/` in
[SF2DISASM master at `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The reused read-only checkout is clean at that pin. Retained map-init and
map-script-engine payloads bind it and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
The unchanged pair totals 18,285,979 bytes; fixture/source comparisons passed in
0.083 seconds. Selected linear comparisons were budgeted below 512 MiB and one
minute, plus small prose/receipts; there was no fresh export or corpus growth.
Only immediate scene, actor, request and battle-entry consumers were added to the
bounded read. Fnnn denotes flag nnn; rows and program record indices are zero-based.

## Selected request and conditional battle admission

**Confirmed:** `data/maps/entries/map69/6-warp-events.asm` has eight ordered rows.
Row0 matches `(28,51)`, is `warpNoScroll`, selects `MAP_KRAKEN_BATTLEFIELD=49`,
and supplies destination `(15,16)`, Up. The symbol resolves in `sf2enums.asm`
before selecting `map49` owners. Rows1–4 are other exact points; rows5–7 match
X0, X63 or Y0 with the other axis wildcard. None competes at `(28,51)`, and row0
already has first-match priority. Negative coordinate bytes are wildcards in
`exploration.asm:WarpIfSetAtPoint`; absence of a row match does not request a warp.

The predecessor's actual mode2 controller is `eas_Caravan` /
`entityscriptengine_2.asm:esc08_controlCaravan`, despite its source name. After
input/bounds/actor-collision checks, a working-layout `$1000` marker calls
`WarpIfSetAtPoint` with proposed coordinates; its movement tail still follows.
The producer divides coordinates by384 and writes numeric type1, the row tuple
and warp SFX. Neither that row nor its matching routine tests F416, F516, F466,
raft ownership or membership. Ordinary walking can also reach the warp producer;
supplied mode alone is not proof of eligibility or of natural water traversal.
The prior Map72 scroll route reaches Map69 without a fresh battle scan, whereas
its no-scroll route may admit Map69 battles9/17 first. This slice assumes the
conditional Map69 control boundary, not that either earlier admission was avoided.

`explorationfunctions_2.asm:ProcessMapEvent` clears pending before dispatch.
`ProcessMapEventType1_Warp` with parameter1=0 requests/clears warp SFX, idles
physical0, removes the exploration return frame, samples the player's position,
replaces nonnegative X/Y and takes the supplied facing. The result is
`MainLoop/@Start(49,15,16,Up)`. PLAYER_TYPE and RAFT_MAP/X/Y are unchanged here;
the request does not itself place the player or update a saved raft tuple.
The pending word/parameters are mutable, not an atomic queue. A nonzero supplied
parameter1 instead stays in scrolling exploration and does not take this scan.

`mapinit_0.asm:SwitchMap` scans `data/maps/global/flagswitchedmaps.asm`; none of
its source-map symbols resolves to49. `getbattle.asm:CheckBattle` then scans the
ordered `data/battles/global/battlemapcoords.asm` rows. The **only** Map49 row is16,
also `BATTLE_VERSUS_KRAKEN=16`: area output `(0,0,30,30)`, triggers `(255,255)`.
It tests map, F416 and then trigger coordinates. The area is output, not a
containment test; arbitrary supplied entry coordinates are not rejected by it.

| Supplied state | Selected call result |
| --- | --- |
| F416 clear, either F516 | No battle; F516 alone cannot admit it. |
| F416 set, F516 clear | Admit16 and write the area outputs. |
| F416 and F516 set | Clear F416, **still admit16 on this call**; no fallthrough or immediate exploration. |
| Any of the above with different PLAYER_TYPE, F64, F67/F68 or membership | Those values are not admission predicates. F466 is tested later. |

F416's accepted producer is Luke's scene. No prerequisite on Battles14/15,
Oddler following, Peter's HP or raft alignment is added here. F516 can prevent a
later admission by the unlock clear, but is not itself the before-scene skip flag.
Battle return's `alt_MainLoopEntry` skips CheckBattle; an arbitrary saved/reload
entry cannot be equated with this original no-scroll call.

## No-admission fallback and transport boundary

**Confirmed:** with no admitted battle, MainLoop selects `ExplorationLoop` for49.
There is **no Map49 entry** in `data/maps/mapsetups.asm` and no `map49/mapsetups`
directory. `mapsetupsfunctions_1.asm:GetCurrentMapSetup` reaches `ms_Void`, a
`$FFFF` sentinel. `GetMapSetupEntityList` returns that sentinel, and
`RunMapSetupInitFunction` skips the callback. This is not a named empty Map49 init
function and does not invent one from neighboring map conventions.

The supplied-map exploration path clears pending, heals living/immortal allies,
loads tiles, rebuilds entities, clears temporary flags256–383 and sets F80,
loads the map and calls the sentinel-guarded init wrapper. Its ordinary
palette/music/fade and `WaitForEvent` path can then install control. The sentinel
means no fixed setup rows; the initializer still declares main0 and flag-selected
followers. Map49 is outside the two follower-exclusion maps and the overworld
map table. F65 therefore does not select the overworld follower table here.

Mode2 is preserved on this fallback: `followersfunctions_2.asm:sub_44404` sets
main's raft sprite, moves listed followers to `(64,64)` with `eas_Standing`, and
declares physical raft31 at `(64,64)`.
For other modes it instead tests F64 and RAFT_MAP=49 before declaring the separate
raft from its saved tuple. Actual flags/mappings/mode remain supplied; neither
branch proves reachable or usable exploration of this battle-only map. Further
fallback travel, exits and disembark are outside scope.

## Fresh caller, scene population and identities

**Confirmed:** `mainloop.asm` passes d0=49,d1=16 through `j_BattleLoop` in
`s05_jumpinterface.asm` to `battleloop_1.asm:BattleLoop`. It clears PLAYER_TYPE to0
and sets F399 **before** choosing the F88 saved/new branch. Thus even a supplied
walking/Caravan/raft mode converges here; no disembark action is performed and
RAFT_MAP/X/Y remain unchanged by this entry.

For F88 clear, it clears elapsed seconds, stores current map/battle, installs base
VInts and calls `j_ExecuteBeforeBattleCutscene`. The s07 jump interface resolves
that alias to `beforebattlecutscenesstart.asm:ExecuteBeforeBattleCutscene`.
The wrapper computes `BATTLE_INTRO_CUTSCENE_FLAGS_START(450)+16=466`.
F466 set skips the introduction but still returns to new-battle initialization.
With F466 clear, relative-pointer slot16 selects `bbcs_16` at `0x4AFBE` in
`data/battles/entries/battle16/cs_beforebattle.asm`. The before wrapper does **not**
set F466; the later start wrapper owns that write. F516 and F466 have separate roles.

The scene first loads Map49 with camera `(9,10)` through
`mapscriptengine_1.asm:csc37_loadMapAndFadeIn → csc48_loadMap`. This configures a
black fade-out, waits for it, loads tiles/map with VInt/display sequencing and
sets view target to-1. The map/camera operands feed `mapload.asm:LoadMap`; they
do not position Bowie. The explicit later `fadeInB` requests the fade-in.

Next, `csc0A_executeSubroutine` calls exactly `csub_4B1CA` at `0x4B1CA`:
clear PLAYER_TYPE byte, then return. It is defined in adjacent `cs_afterbattle.asm`,
but only this called body is part of entry. No outcome body is required or claimed.
The clear repeats BattleLoop's normal-entry clear; the script itself therefore
also normalizes transport before population when called directly.

`loadMapEntities ce_4B15C` calls `csc42_loadMapEntities → InitializeMapEntities`.
The header supplies main `(15,14)`, Right, independently of warp `(15,16)` and
camera `(9,10)`. `ClearEntities` clears physical state and all64 lookup bytes;
population rebuilds current followers before fixed rows, then main0 and the
exploration-list terminator. Hidden positions/mappings from raft boarding are
not copied forward. Mode0 selects Bowie's normal ally mapsprite. No scene command
joins, removes, revives or sets following flags. A separate raft is declared only
if F64 and RAFT_MAP=49; the accepted raft-grant tuple at Map72 does not satisfy it.

The table has **11 fixed rows**, each initially `(63,63)`, Down, `eas_Init`:
one Kraken-head sprite254, two arm sprites123 and eight leg sprites74. These are
scene rows, not enemy-stat initialization. The head's special-sprite branch uses
physical47 and ENTNUM32; the remaining rows consume ordinary physical slots after
the actual followers. The sequential lookup32–42 entries expose logical128–138.
Under only F67/F68/F75, Peter, FOLLOWER_B and Luke occupy physical1/2/3; arms then
occupy4/5 and legs6–13. Other admitted followers change those physical numbers.

Map49 uses the normal follower table: F67 supplies Peter7 and F68 supplies
FOLLOWER_B31 with Oddler's sprite. Later enabled declarations for the same logical
follower can overwrite its lookup; this is not unconditional Oddler identity.
`GetEntityAddressFromCharacter` masks the logical byte, subtracts96 for enemy-form
indices, reads its lookup and uses a32-byte physical stride. Missing fresh mappings
are zero and can alias main0; commands do not validate a live logical actor.
FOLLOWER_B31 is not physical raft31 or FOLLOWER_D159/lookup63.

`AdjustScriptPointerByCharacterAliveStatus` skips position/facing commands for
zero-HP allies0–29. FOLLOWER_B31 and scene128–138 bypass this ally test. Action
installation, waits and cloning have no equivalent HP guard. Consequently a dead
or missing Peter can skip a placement/facing yet still receive actions, possibly
through an aliased lookup. The normal fresh-battle heal occurs **after** this
introduction, so it cannot justify its initial HP/mapping assumptions.

## Complete pre-battle program and dialogue

**Confirmed:** retained `programCorpus.programs[id=bbcs_16]` contains **78 records**,
source lines4–90, ending in `csc_end`. Each sourceLine/macro/argument tuple was
compared to pinned source. These top-level records omit the contents of **four
inline bodies**, read separately below. Actor-table rows are also separate.

| Records | Ordered work (all coordinates are command operands) |
| --- | --- |
| 0–4 | Cursor2513; Map49/camera9,10; `csub_4B1CA`; scene population; waited Bowie `eas_Init`. |
| 5–6 | Copy3×3 blocks `(13,4)→(0,0)`, then `(31,4)→(13,4)`. |
| 7–14 | Clone129→130, then131→132,133,134,135,136,137,138, in that order. |
| 15–19 | Waited Peter init, waited FOLLOWER_B init; Peter `(15,15)`, Left; FOLLOWER_B `(13,14)`, Down; fade-in. |
| 20–27 | Wait30, wait5; install Bowie's `eas_2xRightLeft`; wait5; install Peter's; wait5; install FOLLOWER_B's; wait120. The three installs are not waited. |
| 28–33 | Waited Peter inline `moveRight 1; endActions`; wait20; face Bowie, Peter and FOLLOWER_B Down; text2513. |
| 34–40 | Bowie Left, Peter Left, FOLLOWER_B Right; text2514; wait30; Peter Up; wait30. |
| 41–48 | Two separately waited Peter `eas_Jump` calls; text2515; Bowie Up, FOLLOWER_B Up; wait20; waited Peter inline `moveUp 2; endActions`; text2516. |
| 49–55 | Camera `(9,0)`; rumble; quake `$8002`; rumble; wait30; waited logical128 `eas_DeactivateAutoFacing`; waited custom body `ac_setSpeed 16,16; ac_jump eas_Idle; ac_end`. |
| 56–62 | Logical128 `(14,5)`, Down; waited inline `moveUp 2; endActions`; restore3×3 `(0,0)→(13,4)`;129 `(12,4)`, Down;130 `(16,4)`, Down; rumble;129 `SLIDE_IN`. |
| 63–71 | Set131–138, all Down, to `(13,2),(15,2),(17,3),(11,4),(12,5),(13,5),(16,5),(14,6)` respectively;131 `SLIDE_IN`. |
| 72–77 | Quake `$4002`; text2517,2518,2519; wait60; ordinary script end/caller return. |

The seven text sites use original `data/scripting/text/gamescript.txt` keys
`09D1`–`09D7`; source comments were compared against those IDs, not used as a
substitute for them. No transcript is reproduced here.

| Record / ID | Speaker operand and text form | Content role |
| --- | --- | --- |
| 33 / 2513 | `$C0,ALLY_PETER`, `nextSingleText` | Notices the stopped raft. |
| 37 / 2514 | `$0,FOLLOWER_B`, `nextSingleText` | Questions the water's motion. |
| 43 / 2515 | `$C0,ALLY_PETER`, `nextSingleText` | Reacts to something ahead. |
| 48 / 2516 | `$C0,ALLY_PETER`, `nextSingleText` | Directs the leader's attention. |
| 73 / 2517 | `$C0,ALLY_PETER`, `nextSingleText` | Identifies a river monster. |
| 74 / 2518 | `$0,FOLLOWER_B`, `nextText` | Connects it to the report in Polca. |
| 75 / 2519 | `$0,FOLLOWER_B`, `nextSingleText` | Warns the party. |

Peter has four sites; FOLLOWER_B has three. Bowie has no text site. FOLLOWER_B's
Oddler attribution is conditional on the populated sprite/mapping, not a hardcoded
dialogue actor identity. `csc00_displaySingleTextbox` and `csc02_displayTextbox`
use `code/common/scripting/entity/getentityportaitandspeechsfx.asm:GetEntityPortaitAndSpeechSfx`
to select portrait/speech properties from the resolved actor's mapsprite. They
wait for view scrolling, display the current cursor and then increment it.
Single-text additionally closes portrait/
text windows and sleeps10; record74 leaves that closure to the following single
site. There is no choice or branch in this script. `ExecuteMapScript` clears text
skip at entry; debug plus player2 Start can set it during dispatch, suppressing
these display calls and their cursor increments and skipping encoded `csWait`s.
It does not skip every action, quake or FX service. The listed IDs describe the
ordinary non-skipped cursor path, not every debug-interleaved display history.

## Immediate action and presentation consumers

**Confirmed:** `csc15_setEntityActscript` and `csc14_setEntityActscriptManual`
resolve the logical actor, write its action-read timer from the resolved physical
index and replace the action pointer. A waited form spins until that pointer
equals `eas_Idle`; the custom form then scans to `$8080`. Neither proves that all
actors or graphics/audio services have completed. `eas_Init` configures zero
acceleration factors, speed32, acceleration/deceleration off, obstruction/collision
flags, automatic facing, Up orientation, immersion off, size24, sprite refresh
and idle. It does not heal, join or reset position. Source is
`data/scripting/entity/eas_actions.asm`.

`eas_2xRightLeft` requests Right/Left/Right/Left with a sprite refresh and wait30
after each, then idle. The three non-waited scene installs plus fixed `csWait`s
do not independently join those three pointers. `eas_Jump` disables automatic
facing, requests relative `(0,-1)`, wait5, `(0,1)`, wait5, `(0,0)`, restores facing
automation and idles. These are action requests, not observed jumps or a measured
real-time duration. `eas_DeactivateAutoFacing` reaches idle immediately after
turning it off; instructions after that jump are not part of this invocation.

`csc2D_entityActionSequence` clears FLAGS_A bits5/6, constructs relative-destination
opcode4 plus destination-wait opcode1 for each move, appends a jump to idle and
waits on that actor's action pointer. The three inline movements are Peter Right1,
Peter Up2 and head Up2. `entityscriptengine_2.asm:esc04_moveToRelativeDest` adds
tile-scaled offsets to current position and installs destination/travel/velocity;
its collision helper gates on bit5, which this builder clears. `esc01` waits for
both position/destination differences to be zero. Missing mappings, concurrent
action replacement or stalled entity updates are not repaired by these waits.

`csc19` writes position **and destination**, scaling coordinates by384, then
facing and sprite refresh; it does not clear travel/velocity. `csc23` writes
facing/refresh. The head's `(14,5)` and requested Up2 are therefore a conditional
path to `(14,3)`, not evidence of a reached/rendered position. Bowie's warp,
scene header and later combat placement likewise are three different inputs.

`csc25_cloneEntity` copies only `ENTITYDEF_OFFSET_ENTNUM=$12` from the first
resolved physical actor to the second. It does not copy positions, actions,
mapsprite IDs, lookups, HP, enemy definitions or membership. `DmaMapsprite` uses
that number to select the VRAM sprite slot (`$7000 + ENTNUM*576`, transfer288 words).
Thus the arm pair shares129's sprite slot and the eight legs share131's after
cloning; two explicit FX commands can address these shared resources without ten
separate FX calls. Visible synchronization is **Inferred**, not an observed result.

`csc34_setBlocks → exploration.asm:CopyMapBlocks` copies working-layout words
with64-word row stride; each3×3 call copies nine words, then sets both view-plane
update bits. The first copy saves the `(13,4)` region at `(0,0)`, the second replaces
it, and record58 restores it. It does not rewrite the ROM or prove which picture
the blocks depict. `LoadBattle` subsequently reloads the map.

`csc32_setCameraDestInTiles` sets target-1, requests `(9,0)` and waits for scroll
completion. `csc33_setQuakeAmount` with `$8002` writes amplitudes1,2 with Sleep40
after each; `$4002` writes1,0 with Sleep40 after each. These are two-step ramps,
not an instantaneous toggle. `csc05_playSound` submits numeric
`SFX_BIG_DOOR_RUMBLE=93` three times without waiting for audio completion.

`animEntityFX` is opcode `$22`, `SLIDE_IN=5`. Its handler resolves129 or131,
uses `table_EntityFadingDefinitions` row5 `(22,1,-1,1)`, and performs23 iterations
of mapsprite load, crop-effect service, DMA and two VInt waits, adjusting the two
operands after each iteration. It changes sprite material, not actor coordinates;
the extra slide-out tail applies only to mode4. Quake, camera, DMA, VInt, fades,
text waits and the crop service remain required execution dependencies rather
than completed service receipts. `csc_end` conditionally waits for view scrolling
when the dialogue-window index is nonzero, clears scrolling speed and returns.

## Battle initialization and stopping boundary

**Confirmed:** after the before wrapper returns, `BattleLoop` restores map49 and
battle16 from its saved d0/d1, clears battle-region flags90–105, calls
`HealLivingAndImmortalAllies`, initializes ally positions, initializes enemy
positions, clears AI memory, calls `LoadBattle(0)` and then the start wrapper.
Healing restores living allies and immortal Peter/Lemon; it does not revive every
dead ally. Scene actor coordinates and effects are not combatant state authority.

`getbattlespritesetsubsection.asm` selects the current battle's pointer in
`data/battles/spritesets/entries.asm`; slot16 is `BattleSpriteset16` at `0x1B42DA`.
Its source header and macro counts agree: **12 ally placement rows, 11 enemy rows,
zero AI regions and zero AI points**. The enemy rows are one head, two arms and
eight legs, all STARTING. Their AI/item/behavior records are input to initialization;
zero regions/points does not mean no AI, and this slice does not execute it.

`battleloop/initializecombatants.asm:InitializeAllAlliesBattlePositions` first
sets all30 ally X values to-1 and calls `UpdateForce` to rebuild membership lists.
It takes identities from `BATTLE_PARTY_MEMBERS`, not the ally row's ordinal or
the scene's follower list. Zero-HP members consume a list/loop slot but no placement
row; living members take the next row. Capacity overflow can remove remaining
members from the active battle party. Twelve placement rows therefore do not
prove twelve living allies, Peter's participation or FOLLOWER_B membership.

`InitializeAllEnemiesBattlePositions` scans32 slots beginning at128 and uses the
selected spriteset count and initialization mode. The11 STARTING rows initialize
enemy stats/positions; out-of-range slots follow hidden/cleared state. The special
Battle32/Jaro branch is not selected by16. The initializer does not promote the
11 scene actors into combatants by copying their coordinates or clone links.

`battlefunctions/loadBattle.asm:LoadBattle` fades out, loads tiles and clears VInts,
calls `PositionBattleEntities` to rebuild actors from placed combatant state,
selects the requested combatant's mapped view target, initializes sprites, loads
the map and mapsprites, installs base VInts, loads terrain, requests map music and
fade-in. This rebuild clears scene entity state/lookups again; neutral/unplaced
combatants have their own exclusion branches. New entry supplies combatant0.
The first placement row is `(15,15)`, conditional on the actual active/living list;
the scene header `(15,14)` and warp `(15,16)` do not override it.

`j_ExecuteBattleStartCutscene` resolves through s07 to
`battlestartcutscenesstart.asm:ExecuteBattleStartCutscene`. F466 set skips it.
Otherwise it sets F466 **before** executing pointer slot16, which is `ms_Empty`,
the shared `$FFFF` terminator in `code/common/scripting/map/ms_empty.asm`.
The battle16 directory contains only before/after sources; there is no dedicated
Battle16 start scene. Interruption before this
wrapper can leave F466 clear after some introduction effects; a set flag is not
a receipt for every earlier effect. Stop at the wrapper's return into `@Start`,
before `ActivateEnemies` and all subsequent region/spawn/turn work. No first
player turn, completed encounter or battle outcome is asserted.

F88 set instead restores saved seconds, clears F88 and AI memory, clears the view
target, calls LoadBattle and branches directly to the individual-turn loop.
It bypasses the before/start wrappers, healing, fresh roster placement and new
turn-order generation; the later branch to `@Start` is unreachable on that path.
It does not overwrite current map/battle from the incoming tuple or perform the
fresh branch's explicit `clr.w d0` before LoadBattle. Saved current map/battle,
combatant/turn state and caller operands must be consistent; this warp tuple is
not a repair mechanism. Saved-state consistency and resumed combat are excluded.

## Reproduction, coverage and preserved limits

Load `. ./local/private-inputs.ps1` in the command's process, verify the read-only
source pin/clean status, then use the existing
[retained comparison recipe](../grans-opening/battle06-to-battle07-content-route.md#reproduction-and-review-boundary).
It compares map-init and map-script-engine fixtures against retained payloads and
`_cutscene_facts` against `battle-cutscenes-static-v1.json`, without rewriting input.
No new ROM verification/extraction or runtime is claimed.

For the selected direct check, resolve Map69 row0's symbol using `sf2enums.asm`,
then use that numeric result to select map owners and filter battle-map rows;
use the selected battle index to select both cutscene pointer slots and spriteset
pointer/header. Do not independently assume the numeric owners. Compare all78
`bbcs_16` sourceLine/macro/arguments records, all four inline source bodies,
all seven text keys/comments and the exact called `csub_4B1CA` body. Check all
eight warp rows for priority/overlap and enumerate actual Map49/battle16 filenames.
Immediate consumers and the s05/s07/s13 jump aliases named above complete the
static call review. These small direct selections passed without a new checker
platform or fixture.

| Existing fixture under `tests/fixtures/h2/` | Reuse and limit |
| --- | --- |
| `map-script-engine-static-v1.json`, `map-init-static-v1.json` | Retained comparisons passed;78 selected top-level records agree with source. Four inline bodies and adjacent subroutine were inspected separately, not represented as additional top-level records. |
| `map-event-request-consumption-static-v1.json`, `battle-routing-data-static-v1.json` | Shared request/admission owners; exact Map69→symbol→49→row16 join is direct source review, not new composed executable coverage. |
| `map-setup-static-v1.json`, `map-entities-static-v1.json` | Shared selector/population owners; Map49 absence selects sentinel fallback. No per-Map49 init record is invented. |
| `battle-cutscenes-static-v1.json`, `battle-control-static-v1.json`, `battle-loop-static-v1.json` | Existing cutscene-facts comparison passed; actual before/start slots, flags and caller ordering were also read. No natural-entry/service-completion claim. |
| `battle-spriteset-data-static-v1.json`, `common-stats-static-v1.json` | Aggregate input inventory and membership boundaries; selected12/11/0/0 header/row counts and placement consumers were checked directly. Scene/roster equality is not inferred from matching enemy counts. |

Discriminating supplied states include F416 clear with F516 set; both set; F466
set with an unlocked fresh battle; F88 set; non-raft entry; missing F67/F68 or
overwritten FOLLOWER_B mapping; Peter HP0 before the later heal; nonzero warp
parameter1; debug text skip; and incomplete action/scroll services. They constrain
the prose, not a newly executed runtime matrix. Existing aggregates are not
promoted to per-scene or composed-route coverage, and no candidate is manufactured
solely to turn this document into a verification project.

**Unknown:** natural reach from Map69 control, actual flags/mode/HP/membership/
lookup/parameter state, action and all-actor completion, original rendering and
audio timing, usable input, save/resume consistency and raft/state persistence.
They remain deferred under [ADR 0014](../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR 0016](../../../decisions/0016-remake-start-evidence-deferral.md), not an automatic
acquisition queue or an invitation to follow battle outcomes.

The predecessor's rejected South-Parmecia=70 selection remains preserved in
[PR818's finding](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/818#issuecomment-6159788942),
its [corrected join and final CI handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/818#issuecomment-6159852108)
and linked original receipts. Passing independent symbolic/numeric checks was
insufficient there; this slice joins them before selecting owners. Unaffected
successful evidence, original-candidate CI and earlier inspection failures remain
preserved with their original status.

Acceptance is direct full-document/source/provenance/link/private-boundary review,
`git diff --check`, clean committed
`uv run sf2 verify plan --base origin/main --head HEAD` and actual final-head CI.
Local blanket normal/full/engine/Godot/H3/H4 suites are not selected for prose.
Exact Git identity, check receipts and current inspection failures belong in the
Issue/PR handoff; independent acceptance and merge remain main-gate's.
