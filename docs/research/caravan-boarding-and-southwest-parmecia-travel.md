# Caravan boarding and Southwest Parmecia travel

Evidence date: 2026-10-14. **Confirmed:** the selected Taros Shrine exit returns
through battle admission before fresh Southwest Parmecia population. Walking
can request numeric event 2; its complete consumer changes PLAYER_TYPE to 1 only
after the boarding scripts and current-list wait. Mode 1 installs opcode 7,
whose landing request is numeric 4; that consumer restores conditional walking.
Map68's default Battle19 callback is reached through walking's separate zone
producer, absent from opcode 7. **Inferred:** these operations provide Caravan
travel. **Unknown:** a naturally traversed river/route, actual supplied state,
completed services, usable control and persistence.

## Agreement and provenance

Standard rigor under [Issue #852](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/852)
and [the content epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
Accepted starting base: `556058c2a0bb6478ce3b99240fc288af1ed6e53a`.
This sole document owns the immediate Map12 exit/Map68 entry, Caravan boarding,
mode 1 movement and landing, and separate walking/default Battle19 request and
admission. No shared edits or stacked dependencies. Stop at conditional walking
control after landing or Battle19 admission. Exclude terrain reconstruction,
mandatory routes, CaravanMenu/storage/party transactions, destination village
content, Battle19 opening/roster/turns/outcomes, later travel and runtime acquisition.

Reuse accepted [acquisition and return](./battle18-outcomes-and-caravan-acquisition.md),
[Taros entry/warning](./southwest-parmecia-through-taros-battle18-entry.md),
[Rick current following](./hassan-rick-recruitment.md),
[raft/controller selection](./polca-raft-boarding-through-south-parmecia-entry.md),
[request writes](./map-event-request-state.md),
[request consumers](./map-event-request-consumption.md) and
[random-battle structure](./map-event-random-battle-state.md).
All source paths below are relative to `disasm/` in
[SF2DISASM master at `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at that pin. Retained payloads bind it and USA
retail ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Fnnn means flag nnn; indices are zero-based. Coordinates, sizes and waits below
are source operands, not observed positions, durations or rendered effects.

The retained map-init/map-script-engine pair is 18,285,979 bytes. Reuse its
linear comparisons plus bounded complete selected source/fixture reads; budget
below 512 MiB and one minute for comparisons, small prose/receipts and no payload
regeneration, installation or new verification machinery. Reassess before
material scope/resource growth. No game-material upload is required.

## Shrine exit, admission and fresh population

**Confirmed:** `data/maps/entries/map12/6-warp-events.asm` has one row: any X,
Y 31, no scroll, Map68 `(55,44)`, Down. The accepted acquisition scene sets F65
and clears F84; it does not request boarding or write PLAYER_TYPE. Its conditional
Map12 warning/retreat is already owned by the dependencies above. A selected
south-exit row does not prove access to Y 31 or the next Caravan boarding tile.

`code/gameflow/exploration/exploration.asm:WarpIfSetAtPoint` scans ordered rows
against coordinates divided by 384, with negative coordinate bytes as wildcards.
A match writes type 1, parameters 1–4, facing/padding at parameter 5, and WARP_SFX.
`explorationfunctions_2.asm:ProcessMapEvent` clears pending before dispatch.
The no-scroll consumer submits/clears that sound, idles physical 0, unwinds the
exploration return frame, samples current position, applies nonnegative requested
coordinates and requested facing, and returns `(68,55,44,Down,d4=0)` to MainLoop.
It neither clears transport mode nor proves a completed walk to the exit.

`code/gameflow/mainloop.asm` then calls SwitchMap and CheckBattle before
ExplorationLoop. The complete `data/maps/global/flagswitchedmaps.asm` has no
Map68 substitution. `data/battles/global/battlemapcoords.asm` row 19 is the only
Map68 row: area `(5,19)`, size 48x32, trigger `(255,255)`.
`code/common/maps/getbattle.asm:CheckBattle` requires F419 and matching trigger
bytes, not containment in that area rectangle. Both wildcard triggers match
regardless of entry `(55,44)`. F519 set clears F419 **after selection but still
admits Battle19 for that call**. F65, F518, F725, sword custody and transport
mode are not its admission predicates. Stop here if F419 was independently set.

Otherwise the supplied-map ExplorationLoop clears pending, subtracts 20000 from
STEP_COUNTER with its signed-word negative clamp, heals living/immortal allies,
fades/loads, writes CURRENT_MAP=68 and CURRENT_BATTLE=255, rebuilds entities,
clears temporary flags 256–383, sets F80 and runs the selected init. The single
Map68 setup in `data/maps/mapsetups.asm` points to `map68/mapsetups/pointertable.asm`:
its fixed entity list is empty and `ms_map68_InitFunction` is just return.
After conditional map music/fade, pending zero permits WaitForEvent to follow
physical 0 and install the controller selected by the supplied PLAYER_TYPE.

`code/common/scripting/map/mapfunctions.asm:InitializeMapEntities` clears
entities/lookups, allocates flag-selected followers starting at physical 1,
processes fixed rows, declares main 0 separately, and terminates the current
EXPLORATION_ENTITIES list. Main 0 is not appended to that fresh list.
`followersfunctions_1.asm:InitializeFollowerEntities` uses the overworld table
only for F65 set and IsOverworldMap true; `data/maps/global/overworldmaps.asm`
includes 68. `data/scripting/entity/followers.asm` then puts the F65 Caravan row
first: logical FOLLOWER_A 30, mapsprite 62, physical 1, followed by the normal table.

Thus F65 controls fresh population, independently of PLAYER_TYPE. F65 clear
selects normal followers and does not repair a supplied mode 1. F65 set/mode 0
creates a separate Caravan follower but still selects walking. Fresh mode 1
alone does not replay boarding: this initialization's separate transport special
case `followersfunctions_2.asm:sub_44404` handles mode 2/raft, not mode 1.
Consistent first acquisition cleared Rohde F84; repeated victory preserves its
supplied value. F67 Peter, F68 Oddler and all other follower flags remain inputs.

Later F74/F77/F83 rows can overwrite FOLLOWER_A's lookup while the Caravan
remains physical 1. No HP test guards follower allocation. Under supplied
F65/F67/F68 only, the list is physical `[1,2,3,-1]`: Caravan, Peter, FOLLOWER_B.
FOLLOWERS_LIST starts one byte after EXPLORATION_ENTITIES (`sf2const.asm`), so
it selects `[2,3,-1]` in this case. With only F65, it is immediately empty.
This skip-first convention is not a logical search for the Caravan. F65-clear,
changed flags, missing mappings, excess followers, or an existing list extended
by Rick have their own effects; no capacity/identity repair is established.
Fresh table population has no Rick row, as the accepted Rick owner explains.

## Numeric names and the walking boarding producer

**Confirmed:** resolve `sf2enums.asm`, `sf2cutscenemacros.asm`, the entity opcode
table and the actual exploration dispatcher together:

| Value | Source enum / selector | Actual selected consumer |
| --- | --- | --- |
| PLAYER_TYPE 0 | PLAYERTYPE_BOWIE | eas_ControlledCharacter, ac_controlCharacter, opcode 2. |
| PLAYER_TYPE 1 | PLAYERTYPE_CARAVAN | eas_Raft, ac_controlRaft, opcode 7. |
| PLAYER_TYPE 2 | PLAYERTYPE_RAFT | eas_Caravan, ac_controlCaravan, opcode 8. Other values except 0/1 also take this selector branch. |
| Event 1 | MAP_EVENT_WARP | Warp consumer; parameter 1 selects scroll/no-scroll. |
| Event 2 | MAP_EVENT_GETINTOCARAVAN | ProcessMapEventType2_GetIntoCaravan → j_MapEventType2 → MapEventType2. |
| Event 3 | MAP_EVENT_GETINTORAFT | MapEventType3; separate raft alignment/boarding owner. |
| Event 4 | MAP_EVENT_GETOUTOFRAFT | ProcessMapEventType4_GetOutOfCaravan → j_MapEventType4 → MapEventType4. Actual Caravan exit. |
| Event 5 | MAP_EVENT_GETOUTOFCARAVAN | ProcessMapEventType5_GetOutOfRaft → MapEventType5. Actual raft exit. |
| Event 6 | MAP_EVENT_ZONE_EVENT | Physical 0 init action, then selected zone callback. |

The GETOUT and vehicle action labels disagree with actual numeric routing at
this pin. The s07 jump interface resolves the named aliases directly. The
`MAP_EVENT_RELOADMAP=$100FF` constant describes a packed longword write,
not a separate dispatch case; its byte consequences are below.

`code/common/scripting/entity/entityscriptengine_2.asm:esc02_controlCharacter`
(lines 194–504) supplies the selected walking producer. Physical 0 uses
CURRENT_PLAYER_INPUT and layer 1 bounds shrunk by one tile; other slots use
PLAYER_1_INPUT and unshrunk bounds. It considers Up/Down/Left/Right in order;
later eligible input can overwrite axis/facing choices, and the horizontal
slope branch can adjust Y. No eligible direction advances the script without
movement. Bit 5 of FLAGS_A admits a 32-slot obstruction scan against bit 7 actors:
both axis distances must be below `$100`, using current positions and destinations.
A block skips layout handling and movement but reaches sprite/facing update.

At `loc_5220`, working-layout bits `$3C00` equal `$3800`, CURRENT_BATTLE 255
and F65 set write **only the pending event word 2** and branch to `loc_531E`.
No destination/velocity/travel advance or STEP_COUNTER decrement occurs on
this boarding branch. Facing can change through UpdateEntitySprite, conditional
on auto-facing and sprite-load capacity; it is not an unconditional write.
This precedes the later raw-layout `$C000` blocking test. The following `$3C00`
branch with F64 is separate raft boarding. There is no Caravan-coordinate,
PLAYER_TYPE, F518, F725, HP, membership or logical-mapping test at event 2 creation.
An obstructing entity can prevent reaching it; the Caravan's follower action
clears its obstructable bit when that action executes.

This is not C-button CaravanMenu admission. The ordinary C path in
`explorationvints.asm:ProcessPlayerAction`, after idle/motion/view waits and
debug alternatives, checks physical 1's Caravan mapsprite and equality of main 0/
physical 1 destination coordinates before its shrink/menu call sequence.
That distinct path ends here at `j_CaravanMenu`; no menu transaction is claimed.

## Complete event 2 boarding and current-list effects

**Confirmed:** `code/common/scripting/map/vehiclefunctions.asm:MapEventType2`
(lines 10–20) idles physical 0, executes `cs_45284`, applies `byte_45316` to
FOLLOWERS_LIST, waits through WaitForFollowersStopped, then writes mode 1 and
returns. Unlike raft event 3, it has no separate alignment gate. It does not
recheck F65/current battle, create an actor, heal, join or save vehicle coordinates.
MakeEntityIdle installs only the idle pointer; existing motion is not cleared.

The complete retained program `cs_45284` at 0x45284 has five records (lines 23–27):

| Record | Command and complete immediate action |
| --- | --- |
| 0 | Wait FOLLOWER_A on eas_452A4: request position one tile in physical 0's facing direction from physical 0's **current** position; wait for own destination; branch to eas_Idle. |
| 1 | Wait Bowie on eas_452B2: request physical 1's current position at distance 0, then enter eas_452BA: wait 6, resize ON, warp SFX, sizes 22/20/18/16/14/12 with sprite refreshes and wait 1 between successive sizes; wait own destination; clone physical 1; branch idle. |
| 2 | setPos FOLLOWER_A `(64,64)`, Down: update its current/destination coordinates to `$6000,$6000`, facing and sprite. Mapping/list remain intact. |
| 3 | Wait Bowie on eas_45308: set Caravan mapsprite 62, resize OFF, refresh, branch idle. |
| 4 | csc_end: normal script return, with the shared conditional view-scroll wait and camera-speed clear. |

`mapscriptengine_1.asm:csc15` resolves logical actors through current lookup and
waits for that actor's action pointer to equal eas_Idle, without HP/presence
validation. `csc19` has the shared ally-alive helper, but FOLLOWER_A 30 is outside
the guarded ally range 0–29, so its relocation is not skipped for Bowie/Peter HP 0.
Missing lookup entries cleared to 0 can alias main 0; invalid 255 is not a safe
absence sentinel for these consumers. The script does not verify the mapped
FOLLOWER_A is physical 1 or has a Caravan sprite.

The actions use raw ENTITY_PLAYER_CHARACTER 0 and ENTITY_CARAVAN 1.
`esc0E_moveToEntityFacingRelativePosition` shifts that physical operand directly
into ENTITY_DATA and reads the target's current position/facing; it does not use
logical lookup or the target's destination. Its `loc_55C8` tail sets destination,
travel and signed velocity using the actor's existing speeds, bypassing the
walking layout/obstruction tests. `esc0D_clonePosition` copies physical position,
destination, travel and facing and refreshes; it does not copy velocity, sprite
identity, membership or controller. With conflicting FOLLOWER_A, the first move
and off-map relocation can affect a different actor while Bowie still clones 1.
No observed success or visual defect follows from that supplied-state possibility.

`byte_45316` first moves each selected follower toward physical 0 at distance 0,
then branches into eas_452BA, skipping eas_452B2's physical 1 movement request.
After shrinking/waiting, it still **clones physical 1**. In the fresh consistent
case physical 1 has just been relocated to `(64,64)`, so selected followers
converge on main and then copy the off-map Caravan state; their sprite identities
are retained. No HP guard filters this current physical list.

`entityfunctions_3.asm:ApplyActscriptToFollowers` installs the action on each
byte in FOLLOWERS_LIST until 255. WaitForFollowersStopped uses the same suffix;
WaitForPartyEntitiesIdle returns immediately if empty, otherwise waits a VInt,
finds the last listed actor, then polls **only that actor's idle pointer** after
VInts. It does not join all followers independently. The three earlier mapped
actor waits and this last-actor wait are distinct predicates. Shared SPRITE_SIZE
is a word, while the resize bit is per actor; interleaved service behavior is not
an observed simultaneous animation. Wait 6/wait 1 are entity-script timer operands,
not measured seconds. Missing services, zero speeds, malformed lists or partial
execution need not reach the final mode write.

Only after these returns does MapEventType2 set PLAYER_TYPE 1. Its caller returns
to ExplorationLoop; pending zero lets WaitForEvent install mode 1 control on
physical 0. No map reload occurs. An intervening pending write takes precedence
over installing control. Sprite/position/list/mode mutations are not atomic.
F65, follower flags, membership and saved RAFT_MAP/X/Y remain unchanged.

## Actual mode 1 movement and landing request

**Confirmed:** `entityfunctions_2.asm:SetControlledEntityActScript` installs
`data/scripting/entity/eas_main.asm:eas_Raft`. It sets FLAGS_A bits 7/6/5, speed 32,
acceleration factors 0/0 and acceleration/deceleration operands -2/ON; then loops
map-block-copy check, opcode 7 and destination wait. The macro names containing
“Uncollidable” do not disable the actual bit 5 obstruction branch.
`esc40_checkMapBlockCopy` handles current-layout `$0800/$0C00` copy markers when
not fading; no terrain reconstruction follows from those markers.

The complete `esc07_controlRaft` body (lines 899–1052) samples PLAYER_1_INPUT,
uses unshrunk layer 1 bounds and considers Up/Down/Left/Right in order. Eligible
axes can combine and later checks choose facing. No eligible direction yields
without movement. Its bit 5-enabled scan checks 32 bit 7-obstructable physical
slots using **current positions only**, with both axis differences below `$100`.
Unlike walking, it does not check the other actors' destinations. An obstruction
reaches sprite update without a new move. Then the selected working-layout tests are:

| Ordered test | Actual mode 1 result |
| --- | --- |
| Masked `$0400` | Call OpenDoor and reread/mask the word. |
| Masked `$1000` | Call WarpIfSetAtPoint, then movement tail whether or not a row matched. |
| Masked `$3800` | Movement tail. |
| Otherwise raw word unsigned below `$C000` | Write numeric event 4, skip movement tail. |
| Otherwise raw word at least `$C000` | Skip movement without landing request. |

`OpenDoor` itself returns when CURRENT_BATTLE is not 255. Otherwise it scans
the map step-event rows; a matching row copies or clears working-layout words,
submits door SFX and marks the relevant view plane for update. No matching row
means no door mutation. The controller rereads the layout after this helper;
the marker alone does not establish an opened door.

The movement tail sets velocities, sets 384 travel on nonzero axes, adds tile
offsets to destination and requests conditional sprite/facing update. Landing
therefore retains the current destination while it can turn toward the attempted
step. Opcode 7 has no direct F65/unlock/current-battle gate. It also has no
walking slope branch, type 6 zone producer, MOVE_SFX submission or STEP_COUNTER
decrement.
Mode 2/opcode 8 instead recognizes `$3C00` and requests numeric 5; it remains the
separate actual raft behavior. These bodies do not establish a path across
Map68's terrain or that a particular landing square is naturally reachable.

Map68's `6-warp-events.asm` also has town rows `(57,24)` and `(48,30)` and
north/west/east border rows. They are independent warp alternatives, not a
mandatory itinerary. Their scroll versus no-scroll lifecycles remain those of
the accepted entry/raft owners; destination village content is excluded here.

## Complete event 4 landing and conditional walking

**Confirmed:** dispatch clears event 4, then calls MapEventType4 (lines 69–80).
It idles physical 0, runs `cs_45348`, applies `byte_45368` to FOLLOWERS_LIST,
waits through the same last-actor helper, calls InitializeFollowerActscripts,
and only then writes PLAYER_TYPE 0. It does not run raft event 5's saved-position
writer, test a physical alignment, or recheck landing terrain/F65/HP.

The complete `cs_45348` at 0x45348 has four records (lines 83–86):

| Record | Ordered work |
| --- | --- |
| 0 | Wait FOLLOWER_A on eas_45360: clone raw physical 0's position/destination/travel/facing, refresh, branch idle. |
| 1 | setSprite Bowie,Bowie: resolve mapped Bowie, obtain his current ally mapsprite, write it, wait a VInt and refresh. |
| 2 | Wait Bowie on eas_4536C: request one tile in his own current facing; resize ON; warp SFX; refresh sizes 12/14/16/18/20/22 with wait 1 after each; resize OFF; size 16 and refresh; wait own destination; branch idle. |
| 3 | csc_end and ordinary script return. |

`csc1A_setEntitySprite` has no alive-skip guard. Its second Bowie operand is an
ally identity resolved by `getallymapsprite.asm:GetAllyMapsprite`, not literal
mapsprite 0: class affects the sprite, and outside battle HP 0 selects blue flame.
Neither this command nor the following action heals Bowie or proves membership.
The growth movement is relative to Bowie's **current position and facing**, not
an unconditional Down step, a collision-checked route, or an implicit clone.

The suffix action `byte_45368` first clones raw physical 1, then enters the same
`eas_4536C` growth body. Under consistent mappings, record 0 placed the separate Caravan at main 0's landing origin/facing. Bowie and
suffix followers move one tile forward from that origin; the Caravan remains
there until subsequent following actions move it. If logical FOLLOWER_A points
elsewhere, the suffix still clones 1. Current list contents govern who grows.
No mapping/list entry is removed or rebuilt by these scripts.

`followersfunctions_2.asm:InitializeFollowerActscripts` then chooses the normal
or F65/overworld action-pointer table and scans **current flag rows**, assigning
one action pointer per admitted row to consecutive physical slots starting 1.
It does not traverse FOLLOWERS_LIST, resolve logical lookup, redeclare actors,
move positions or wait for those newly installed actions. Changed flags or an
extra current-only Rick can therefore disagree with the population/list that
just grew. Do not claim this routine reconstructs or normalizes that list.

After mode 0 is written, return to ExplorationLoop plus pending zero installs
ordinary walking on physical 0. Partial execution can leave restored sprites or
moved followers while mode is still 1. Landing itself neither requests a zone
event nor decrements STEP_COUNTER; a later eligible walking attempt is separate.
No battle selection is performed merely by this in-loop mode change.

## Walking zone callback and Battle19 admission

**Confirmed:** walking opcode 2's later masked `$1400` branch writes event 6,
word parameter 1 = `(XDEST + selected X offset)/384`, and word parameter 3 =
`(YDEST + selected Y offset)/384`. This happens before its raw-layout blocking
test. The subsequent admitted movement tail submits MOVE_SFX, sets movement and
decrements STEP_COUNTER only if its signed word is positive. A blocked tail can
therefore leave a zone request without that movement/decrement. Boarding's
earlier `$3800` branch bypasses both. Opcode 7 has no equivalent zone request;
a separately supplied/replaced pending event is not ruled out by that absence.

`ProcessMapEventType6_ZoneEvent` installs physical 0's eas_Init and passes those
two parameter words to `mapsetupsfunctions_1.asm:RunMapSetupZoneEvent`.
The Map68 pointer selects `s3_zoneevents.asm`, whose sole entry is default marker
`$FD` → Map68_DefaultZoneEvent. This bypasses coordinate matching **once the
callback dispatcher is entered**; it does not call the callback every frame or
on every terrain tile. The complete callback loads BATTLE_OUTSIDE_ELVEN_VILLAGE 19,
calls CheckRandomBattle and returns. The wrapper then closes portrait/text,
waits a VInt and waits physical 0's current position to equal destination.
`eas_Init` resets motion configuration and sprite settings, then jumps idle; applying it does not zero position/destination or existing travel.

`CheckRandomBattle` at 0x47856–0x478C6 keeps these ordered branches:

| Supplied state | Result before wrapper return |
| --- | --- |
| F519 clear | Request path immediately, irrespective of STEP_COUNTER; no RNG call. |
| F519 set, STEP_COUNTER nonzero | No request and no RNG; counter unchanged by this helper. Negative values also take this branch. |
| F519 set, counter zero, first RNG with range 8 returns 0 | Request path; no second RNG. |
| F519 set, counter zero, first RNG nonzero | No request; second RNG with range 4, store result+2 in STEP_COUNTER (2–5). |

`GenerateRandomNumber` updates the 16-bit seed with multiply 13/add 7 and scales
by the supplied range. This is source arithmetic, not a measured probability,
independence assumption or seed distribution. The request path sets F419,
**writes a longword `$000100FF` at MAP_EVENT_TYPE**, writes STEP_COUNTER 30000,
waits view scrolling, submits SFX_BOOST and executes the flash script. It does
not set F519 or call CheckBattle/BattleLoop itself.
`ExecuteFlashScreenScript → cs_FlashScreen` has opcode `$41`, operand 30, end;
`csc41_flashScreenWhite` shifts 30 by 3 and uses DBF to call LaunchFading four
times, then DuplicatePalettes. LaunchFading waits for the fading setting to
clear through VInts. These are service-dependent ordered requests, not proof
of a completed flash or usable control.

### Packed reload and residual parameters

**Confirmed:** `sf2const.asm` lays out event word at `$FFA84A`, parameter bytes 1–5
at `$FFA84C` through `$FFA850`. The longword write therefore means event 1,
parameter 1=0 (no scroll), parameter 2=255 (MAP_CURRENT); **parameters 3/4/5 and
WARP_SFX are not initialized by CheckRandomBattle**. Pending is one replaceable
slot, not a queue or an atomic battle-entry transaction.

In the unmodified selected walking-zone chain with ordinary nonnegative byte
coordinates, word parameter 3 still contains the zone Y: byte 3=0, byte 4=Y.
Thus the no-scroll consumer substitutes **X 0 and that Y**, while parameter 5
supplies residual facing. It submits/clears residual WARP_SFX. It must not be
summarized as “reload at unchanged player position.” Other supplied parameter
bytes or intervening writes change that tuple; negative coordinate bytes retain
the corresponding sampled current coordinate under the warp consumer's rule.
The old zone X word at parameters 1/2 is overwritten completely.

After callback/wrapper return, WaitForEvent sees the new event before installing
ordinary control. The ordinary type 1 consumer clears it, idles main 0, unwinds
exploration and returns map 255 plus the resulting coordinates/facing to MainLoop.
SwitchMap has no 255 row; CheckBattle resolves 255 through CURRENT_MAP 68 and scans
ordered rows. With F419 still set, sole row 19 and wildcard triggers select
Battle19 and write area `(5,19)`, size 48x32, irrespective of residual X/Y. F519
set clears F419 but does not cancel this selected entry. F519 clear leaves it
set here. No F65/mode/HP/sword gate is added. The stopping point is MainLoop's
selected BattleLoop handoff, before opening, placement, turns or outcomes.

If pending is replaced or F419 cleared before consumption/admission, that result
is conditional, not guaranteed. A no-battle map 255 return selects ExplorationLoop's
existing-population branch: update main properties/raft declaration, load/init,
without fresh setup entities or temporary-flag clearing; this is not a fresh
Map68 population assertion. Mode 1's absence of a zone producer also does not
prevent an independently supplied F419 from admitting Battle19 at a later
no-scroll map return, including the initial shrine exit described above.

## Reproduction and remaining limits

Load `. ./local/private-inputs.ps1` in the same process as controlled commands;
verify the clean pinned source and use the existing
[retained comparison recipe](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary).
Map-init and map-script-engine payload/fixture comparisons plus `_cutscene_facts`
against `battle-cutscenes-static-v1.json` passed unchanged in 0.150 seconds. These
are retained comparisons, not new ROM extraction or a natural travel capture. Select exact IDs and source lines;
remove comments before comparing mnemonics/macros and ordered operands.

| Retained owner | Selected source join and limit |
| --- | --- |
| `local/derived/map-script-engine-static.json` programCorpus.programs | cs_45284 records 0–4/lines 23–27 and cs_45348 records 0–3/lines 83–86; all nine macro/argument joins. The public map-script-engine fixture guards summaries/shared command facts, not this full corpus. Immediate action bodies must be read separately. |
| `tests/fixtures/h2/map-events/zone-programs.json` | Exact canonicalSymbol Map68_DefaultZoneEvent, three operations and its default table row. No terrain reach is implied. |
| `tests/fixtures/h2/map-event-random-battle-state-static-v1.json` | Caller 19 and complete CheckRandomBattle branch/write/service structure; direct consumer review supplies packed-byte and downstream admission semantics. |
| `tests/fixtures/h2/map-init-static-v1.json`, `map-setup-static-v1.json`, `map-entities-static-v1.json` | Existing population/init context; selected Map68 pointer/list/return and follower tables reviewed directly. |
| `tests/fixtures/h2/map-event-request-consumption-static-v1.json` | Poll/clear/dispatch boundary; no composed Caravan or battle-admission runtime receipt. |

Read complete vehiclefunctions MapEventType2 through the event 4 action tail,
entity opcode 2/7 and the named shared opcode/lookup/list/follower consumers;
read the actual dispatcher, no-scroll consumer, zone wrapper, random-battle
helper/flash, MainLoop and CheckBattle/table joins. The raw action targets,
current-list suffix and flag-driven follower-action reinstall are three different
authorities. Evidence for one must not substitute for either of the others.

**Unknown:** natural arrival, tile/door/warp/landing reach and actual input;
F65/F419/F519/other flags, counter/seed, HP/class, actor mappings/list and residual
parameters; motion, resize, sprite, sound, fading and view-service completion;
responsive control and save/resume persistence. Supplied-state alternatives here
are static review cases, not executed scenarios. They do not authorize runtime
work under [ADR0014](../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../decisions/0016-remake-start-evidence-deferral.md).

Preserve [PR849 checks/failures](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/849#issuecomment-6161609727)
and [independent acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/849#issuecomment-6161628177),
including their linked PR846/842 results. Failed source-path guesses, wrong
fixture nesting assumptions and truncated inspections are not absence evidence;
resolve owners and reread the required complete bodies. No prior failure is
relabeled or fixture weakened. Acceptance is full owned-diff/source/provenance/
link/private review, `git diff --check`, clean committed configured
`uv run sf2 verify plan --base origin/main --head HEAD`, and actual final-head CI.
No blanket local normal/full/engine/Godot/H3/H4 suite is selected solely for prose.
Exact candidate/check/failure/process state belongs in the PR handoff; independent
review and integration remain main-gate's.
